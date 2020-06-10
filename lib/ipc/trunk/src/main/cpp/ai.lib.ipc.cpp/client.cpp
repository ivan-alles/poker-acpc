#include "stdafx.h"

#include "ai.lib.ipc.cpp/message.h"
#include "ai.lib.ipc.cpp/protocol.h"
#include "ai.lib.ipc.cpp/client.h"
#include "ai.lib.ipc.cpp/internal_definitions.h"

using boost::asio::ip::tcp;

namespace ai
{
	namespace lib
	{
		namespace ipc
		{

			client::client()
				: _io_service(),
				_socket(_io_service),
				_read_header(ipc_protocol::header_length),
				_write_header(ipc_protocol::header_length),
				_write_buff_seq(2), 
				_state(stopped)
			{
				// User thread

				_write_buff_seq[0] = boost::asio::buffer(_write_header);
				create_wait_event(_wait_event);
			}

			client::~client()
			{
				// User thread

				delete_wait_event(_wait_event);
			}


			void client::start(const char * server_address)
			{
				// User thread
				{
					boost::lock_guard<mutex> lock(_mutex);
					if(_state != stopped)
					{
						return;
					}
				}

				_server_addr = std::string(server_address);
				_state = disconnected;
				_thread = boost::thread(boost::bind(&boost::asio::io_service::run, &_io_service));
				_io_service.post(boost::bind(&client::do_connect, this));
			}

			void client::stop()
			{
				// User thread
				{
					boost::lock_guard<mutex> lock(_mutex);
					if(_state == stopped)
					{
						return;
					}
					_state = stopped;
				}

				_io_service.post(boost::bind(&client::close, this));
				_thread.join();

				_in_queue.clear();
				_out_queue.clear();
				// Do not put disconnect event to the queue here, because it
				// triggered by the user.
			}

			bool client::is_connected()
			{
				// User thread

				boost::lock_guard<mutex> lock(_mutex);
				return _state == connected;
			}


			bool client::send(const message& msg)
			{
				// User thread

				{
					boost::lock_guard<mutex> lock(_mutex);
					if(_state != connected)
					{
						return false;
					}
				}
				_io_service.post(boost::bind(&client::queue_and_write, this, msg));
				return true;
			}

			bool client::deque_input_event(event_t & event)
			{
				// User thread

				boost::lock_guard<mutex> lock(_mutex);
				if(_in_queue.empty())
				{
					return false;
				}
				event = _in_queue.front();
				_in_queue.pop_front();
				return true;
			}

			void client::do_connect()
			{
				// IPC thread
				std::string host, port;
				std::string::size_type semicolon_pos = _server_addr.find_first_of(':');
				host = _server_addr.substr(0, semicolon_pos);
				port = _server_addr.substr(semicolon_pos + 1);

				tcp::resolver resolver(_io_service);
				tcp::resolver::query query(host, port);
				tcp::resolver::iterator iterator = resolver.resolve(query);

				_state = connecting;

				boost::asio::async_connect(_socket, iterator,
					boost::bind(&client::handle_connect, this,
					boost::asio::placeholders::error));
			}

			void client::reconnect()
			{
				// IPC thread
				bool was_connected;
				{
					boost::lock_guard<mutex> lock(_mutex);
					if(_state == stopped || _state == connecting)
					{
						// Do not reconnect in the stopped state.

						// Do not start reconnection again because of
						// subsequent socket errors (e.g. read failed, than write failed).
						return;
					}
					was_connected = _state == connected;
					_state = disconnected;
				}
				_socket.close();
				if(was_connected)
				{
					_in_queue.push_back(event_t(disconnect, message()));
					set_wait_event(_wait_event);
				}
				_out_queue.clear();
				::Sleep(100);
				do_connect();
			}

			void client::close()
			{
				// IPC thread

				_socket.close();
			}

			void client::handle_connect(const boost::system::error_code& error)
			{
				// IPC thread

				if (!error)
				{
					{
						boost::lock_guard<mutex> lock(_mutex);
						_state = connected;
						_in_queue.push_back(event_t(connect, message()));
						set_wait_event(_wait_event);
					}
					read();
				}
				else
				{
					// Reset state from connecting to allow reconnection.
					_state = disconnected;
					reconnect();
				}
			}

			void client::read()
			{
				boost::asio::async_read(_socket,
					boost::asio::buffer(_read_header),
					boost::bind(&client::handle_read_header, this,
					boost::asio::placeholders::error));
			}

			void client::handle_read_header(const boost::system::error_code& error)
			{
				// IPC thread

				if (!error)
				{
					_read_msg = message(ipc_protocol::decode_header(&_read_header[0]));
					boost::asio::async_read(_socket,
						boost::asio::buffer(_read_msg.data(), _read_msg.size()),
						boost::bind(&client::handle_read_body, this,
						boost::asio::placeholders::error));
				}
				else
				{
					reconnect();
				}
			}

			void client::handle_read_body(const boost::system::error_code& error)
			{
				// IPC thread

				if (!error)
				{
					{
						boost::lock_guard<mutex> lock(_mutex);
						_in_queue.push_back(event_t(rx_message, _read_msg));
						set_wait_event(_wait_event);
					}
					read();
				}
				else
				{
					reconnect();
				}
			}

			void client::queue_and_write(message msg)
			{
				// IPC thread

				bool write_in_progress = !_out_queue.empty();
				_out_queue.push_back(msg);
				if (!write_in_progress)
				{
					do_write(msg);
				}
			}

			void client::do_write(message msg)
			{
				// IPC thread

				ipc_protocol::encode_header(msg.size(), &_write_header[0]);
				_write_buff_seq[1] = boost::asio::const_buffer(msg.data(), msg.size());
				boost::asio::async_write(_socket,
					_write_buff_seq,
					boost::bind(&client::handle_write, this,
					boost::asio::placeholders::error));
			}


			void client::handle_write(const boost::system::error_code& error)
			{
				// IPC thread

				if (!error)
				{
					_out_queue.pop_front();
					if (!_out_queue.empty())
					{
						do_write(_out_queue.front());
					}
				}
				else
				{
					reconnect();
				}
			}


		}
	}
}