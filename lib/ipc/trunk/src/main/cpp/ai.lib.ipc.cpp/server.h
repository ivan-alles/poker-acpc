#ifndef AI_LIB_IPC_SERVER_CPP_SERVER_H
#define AI_LIB_IPC_SERVER_CPP_SERVER_H

#include <deque>
#include <set>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include <boost/thread/thread.hpp>
#include <ai.lib.ipc.cpp/definitions.h>
#include <ai.lib.ipc.cpp/message.h>
#include <ai.lib.ipc.cpp/connection.h>

namespace ai
{
	namespace lib
	{
		namespace ipc
		{

			/** IPC server. Is running in an own thread after a call to start(). The caller can wait for IPC events 
				via wait_event (see get_wait_event()). 
				All incoming events (connect, disconnect, rx message) are queued and can be retrieved via deque_input_event().
				Use connection object to send messages.
			*/
			class server
			{
				friend class connection;
			public:

				/// Input event kind.
				enum event_kind_t { connect, disconnect, rx_message };

				/// Input event.
				struct event_t
				{
					event_t()
					{}

					event_t(connection_ptr connection_, event_kind_t kind_, message & message_):
					connection(connection_), kind(kind_), message(message_)
					{}

					connection_ptr connection;
					event_kind_t kind;
					message message;
				};

				server();
				~server();

				/** Starts IPC. Begins accepting client connections. 
					@param address: address of the server (port).
				*/
				void start(const char * address);

				/** Stops the thread, clears queues and removes all 
					connections. No disconnect events will be raised for active connections. 
				*/
				void stop();

				bool is_started();

				/** Retrieves a queued input event, if available, otherwise returns false.
				*/
				bool deque_input_event(event_t & event);

				/** Returns the wait event.
				*/
				wait_event get_wait_event() 
				{
					return _wait_event;
				}

			private:
				void start_accept();
				void handle_accept(connection_ptr session, const boost::system::error_code& error);

				void on_connect(connection_ptr connection);
				void on_disconnect(connection_ptr connection);

				boost::asio::io_service _io_service;
				boost::asio::ip::tcp::acceptor * _acceptor;
				std::set<connection_ptr> _connections;

				boost::thread _thread;
				mutex _mutex;
				wait_event _wait_event;
				enum state_t { stopped, started };
				state_t _state;
				std::deque<event_t> _in_queue;
			};

		}
	}
}

#endif // AI_LIB_IPC_SERVER_CPP_SERVER_H