#ifndef AI_LIB_IPC_CLIENT_CPP_CLIENT_H
#define AI_LIB_IPC_CLIENT_CPP_CLIENT_H

#include <deque>
#include <boost/asio.hpp>
#include <boost/thread/thread.hpp>
#include <ai.lib.ipc.cpp/definitions.h>
#include <ai.lib.ipc.cpp/message.h>

namespace ai
{
	namespace lib
	{
		namespace ipc
		{

			/** IPC client. Is running in an own thread after a call to start(). The caller can wait for IPC events 
			via wait_event (see get_wait_event()). 
			All incoming events (connect, disconnect, rx message) are queued and can be retrieved via deque_input_event().
			A message can be send by a call to send().
			*/
			class client
			{
			public:

				/// Input event kind.
				enum event_kind_t { connect, disconnect, rx_message };

				/// Input event.
				struct event_t
				{
					event_t()
					{}

					event_t(event_kind_t kind_, message & message_): kind(kind_), message(message_)
					{}

					event_kind_t kind;
					message message;
				};

				client();

				~client();

				/** Starts IPC. Tries to connect to the server. If connected, can send and receive messages.
				If the server is gone, clears the out queue and tries to reconnect. 
				@param server_address: address of the server (address:port).
				*/
				void start(const char * server_address);

				/** Closes the connection and stops the background thread.
				No disconnect event will be raised.
				*/
				void stop();

				/** Connection status.
				*/
				bool is_connected();

				/** Retrieves a queued input event, if available, otherwise returns false.
				*/
				bool deque_input_event(event_t & event);

				/** Sends a message if there is a connection, otherwise does nothing, ignores the message
				and returns false.
				*/
				bool send(message const & message);

				/** Returns the wait event.
				*/
				wait_event get_wait_event() 
				{
					return _wait_event;
				}

			private:
				void do_connect();
				void reconnect();
				void close();

				void queue_and_write(message msg);
				void do_write(message msg);
				void read();

				void handle_connect(const boost::system::error_code& error);
				void handle_read_header(const boost::system::error_code& error);
				void handle_read_body(const boost::system::error_code& error);
				void handle_write(const boost::system::error_code& error);

				boost::asio::io_service _io_service;
				boost::asio::ip::tcp::socket _socket;
				std::deque<message> _out_queue;
				std::deque<event_t> _in_queue;
				message _read_msg;
				std::vector<boost::asio::const_buffer> _write_buff_seq;
				std::vector<boost::uint8_t> _write_header;
				std::vector<boost::uint8_t> _read_header;
				boost::thread _thread;
				mutex _mutex;
				wait_event _wait_event;
				enum state_t { stopped, disconnected, connecting, connected };
				state_t _state;
				std::string _server_addr;
			};

		}
	}
}

#endif