#ifndef AI_LIB_IPC_SERVER_CPP_IPC_PROTOCOL_H
#define AI_LIB_IPC_SERVER_CPP_IPC_PROTOCOL_H

namespace ai
{
	namespace lib
	{
		namespace ipc
		{
			/** Encapsulates IPC protocol. 

			IPC sends first 4-bytes message header. The header contains the length 
			of the message body (not including the header) as uint32_t in big-endian.

			This class implement functions to work with the message header.
			*/
			class ipc_protocol
			{
			public:
				static const std::size_t header_length;

				static void encode_header(std::size_t body_length, boost::uint8_t * header);
				static std::size_t decode_header(boost::uint8_t const * header);
			};

		}
	}
}

#endif