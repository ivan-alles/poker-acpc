#include "stdafx.h"
#include "ai.lib.ipc.cpp/protocol.h"
#include "ai.lib.ipc.cpp/server.h"
#include "ai.lib.ipc.cpp/internal_definitions.h"


using boost::asio::ip::tcp;

namespace ai
{
	namespace lib
	{
		namespace ipc
		{

			connection::connection(server & server_):
		_socket(server_._io_service), _server(server_),
			_read_header(ipc_protocol::header_length),
			_write_header(ipc_protocol::header_length),
			_write_buff_seq(2),
			_state(disconnected)
		{
			_write_buff_seq[0] = boost::asio::buffer(_write_header);
		}

		connection::~connection()
		{
		}

		bool connection::is_connected()
		{
			// User thread
			boost::lock_guard<mutex> lock(_server._mutex);
			return _state == connected;
		}


		bool connection::send(const message & msg)
		{
			// User thread
			{
				boost::lock_guard<mutex> lock(_server._mutex);
				if(_state != connected || _server._state != server::started)
				{
					return false;
				}
			}
			_server._io_service.post(boost::bind(&connection::queue_and_write, this, msg));

			return true;
		}

		void connection::queue_and_write(message msg)
		{
			// IPC thread

			bool write_in_progress = !_out_queue.empty();
			_out_queue.push_back(msg);
			if (!write_in_progress)
			{
				do_write(msg);
			}
		}

		void connection::do_write(message msg)
		{
			// IPC thread

			ipc_protocol::encode_header(msg.size(), &_write_header[0]);
			_write_buff_seq[1] = boost::asio::const_buffer(msg.data(), msg.size());
			boost::asio::async_write(_socket,
				_write_buff_seq,
				boost::bind(&connection::handle_write, this,
				boost::asio::placeholders::error));
		}

		void connection::handle_write(const boost::system::error_code& error)
		{
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
				_server.on_disconnect(shared_from_this());
			}
		}

		void connection::read()
		{
			boost::asio::async_read(_socket,
				boost::asio::buffer(_read_header),
				boost::bind(&connection::handle_read_header, this,
				boost::asio::placeholders::error));
		}

		void connection::handle_read_header(const boost::system::error_code& error)
		{
			if (!error)
			{
				_read_msg = message(ipc_protocol::decode_header(&_read_header[0]));
				boost::asio::async_read(_socket,
					boost::asio::buffer(_read_msg.data(), _read_msg.size()),
					boost::bind(&connection::handle_read_body, shared_from_this(),
					boost::asio::placeholders::error));
			}
			else
			{
				_server.on_disconnect(shared_from_this());
			}
		}

		void connection::handle_read_body(const boost::system::error_code& error)
		{
			// IPC thread.

			if (!error)
			{
				{
					boost::lock_guard<mutex> lock(_server._mutex);
					server::event_t e(shared_from_this(), server::rx_message, _read_msg);
					_server._in_queue.push_back(e);
					set_wait_event(_server._wait_event);
				}
				read();
			}
			else
			{
				_server.on_disconnect(shared_from_this());
			}
		}

		}
	}
}