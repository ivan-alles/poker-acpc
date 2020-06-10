#include "stdafx.h"
#include "protocol.h"

namespace ai
{
	namespace lib
	{
		namespace ipc
		{

			const std::size_t ipc_protocol::header_length = 4;

			void ipc_protocol::encode_header(std::size_t body_length, boost::uint8_t * header)
			{
				header[0] = (body_length >> 24) & 0xFF;
				header[1] = (body_length >> 16) & 0xFF;
				header[2] = (body_length >> 8) & 0xFF;
				header[3] = body_length & 0xFF;
			}

			std::size_t ipc_protocol::decode_header(boost::uint8_t const * header)
			{
				std::size_t msg_len = (header[0] << 24) + (header[1] << 16) + (header[2] << 8) + header[3];
				return msg_len;
			}

		}
	}
}