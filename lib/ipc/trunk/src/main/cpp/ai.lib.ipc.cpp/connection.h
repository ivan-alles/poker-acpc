#ifndef AI_LIB_IPC_SERVER_CPP_CONNECTION_H
#define AI_LIB_IPC_SERVER_CPP_CONNECTION_H

#include <deque>
#include <set>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
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

			/** IPC connection for the server side. When a new client connects to the server, the server creates 
			    a new connection. 
			*/
			class connection
				: public boost::enable_shared_from_this<connection>
			{
				friend class server;
			public:

				~connection();

				/** Can be used to bind this object to any user data. Is not touched by IPC.
				/
				void * user_data;
				
				/** Connection status. When the remote client disappears, the connection object is kept, but it goes permanently 
				    in the disconnected state.
				*/
				bool is_connected();

				/** Sends a message if there is a connection, otherwise does nothing, ignores the message
				and returns false.
				*/
				bool send(message const & message);

			private:
				enum state_t { connected, disconnected };

				connection(server & server_);
				void read();
				void handle_read_header(const boost::system::error_code& error);
				void handle_read_body(const boost::system::error_code& error);

				void queue_and_write(message msg);
				void do_write(message msg);
				void handle_write(const boost::system::error_code& error);

				state_t _state;
				server & _server;
				boost::asio::ip::tcp::socket _socket;
				message _read_msg;
				std::deque<message> _out_queue;
				std::vector<boost::asio::const_buffer> _write_buff_seq;
				std::vector<boost::uint8_t> _write_header;
				std::vector<boost::uint8_t> _read_header;
			};

			/** A shared pointer to IPC connection.
			*/
			typedef boost::shared_ptr<connection> connection_ptr;

		}
	}
}

#endif // AI_LIB_IPC_SERVER_CPP_SERVER_H