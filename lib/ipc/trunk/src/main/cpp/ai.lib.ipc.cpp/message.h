#ifndef AI_LIB_IPC_SERVER_CPP_MESSAGE_H
#define AI_LIB_IPC_SERVER_CPP_MESSAGE_H

#include <boost/shared_array.hpp>
#include <boost/cstdint.hpp>

namespace ai
{
	namespace lib
	{
		namespace ipc
		{

			/** IPC message. It is reference-counted.
			*/
			class message
			{
			public:
				message()
				{
				}

				explicit message(std::size_t size)
					: _size(size), _buffer(size > 0 ? new boost::uint8_t[size] : 0)
				{
				}

				explicit message(const void * data, std::size_t size)
					: _size(size), _buffer(size > 0 ? new boost::uint8_t[size] : 0)
				{
					memcpy(_buffer.get(), data, size);
				}

				std::size_t size() const 
				{
					return _size; 
				}

				/// Pointer to the data.
				boost::uint8_t * data() 
				{
					return _buffer.get();
				}

				/// Const pointer to the data. 
				const boost::uint8_t * data() const
				{
					return _buffer.get();
				}
			private:
				boost::shared_array<boost::uint8_t> _buffer;
				std::size_t _size;
			};

		}
	}
}

#endif 