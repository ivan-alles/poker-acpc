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

		server::server():
		_state(stopped),
			_acceptor(0)
		{
			// User thread
			create_wait_event(_wait_event);
		}

		server::~server()
		{
			if(_acceptor)
			{
				delete _acceptor;
			}
			delete_wait_event(_wait_event);
		}

		void server::start(const char * address)
		{
			// User thread
			{
				boost::lock_guard<mutex> lock(_mutex);
				if(_state != stopped)
				{
					return;
				}
			}
			tcp::endpoint endpoint(tcp::v4(), atoi(address));
			_acceptor = new tcp::acceptor(_io_service, endpoint);
			_state = started;
			//start_accept();
			_thread = boost::thread(boost::bind(&boost::asio::io_service::run, &_io_service));
			_io_service.post(boost::bind(&server::start_accept, this));
		}

		void server::stop()
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
			_thread.join();
			_in_queue.clear();
			// Do not notify about disconnection of clients,
			// because it is triggered by the user.
			_connections.clear();
		}

		bool server::is_started()
		{
			// User thread

			boost::lock_guard<mutex> lock(_mutex);
			return _state == started;
		}

		void server::on_connect(connection_ptr connection)
		{
			// IPC thread.

			_connections.insert(connection);
			{
				boost::lock_guard<mutex> lock(_mutex);
				connection->_state = connection::connected;
				_in_queue.push_back(event_t(connection, connect, message()));
				set_wait_event(_wait_event);
			}
			connection->read();
		}

		void server::on_disconnect(connection_ptr connection)
		{
			// IPC thread.

			_connections.erase(connection);
			{
				boost::lock_guard<mutex> lock(_mutex);
				connection->_state = connection::disconnected;
				_in_queue.push_back(event_t(connection, disconnect, message()));
				set_wait_event(_wait_event);
			}
		}

		bool server::deque_input_event(event_t & event)
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

		void server::start_accept()
		{
			// IPC thread

			connection_ptr new_connection(new connection(*this));
			_acceptor->async_accept(new_connection->_socket,
				boost::bind(&server::handle_accept, this, new_connection,
				boost::asio::placeholders::error));
		}

		void server::handle_accept(connection_ptr connection,
			const boost::system::error_code& error)
		{
			// IPC thread

			if (!error)
			{
				on_connect(connection);
			}
			start_accept();
		}
		}
	}
}