#ifndef AI_LIB_IPC_SERVER_CPP_IPC_DEFINITIONS_H
#define AI_LIB_IPC_SERVER_CPP_IPC_DEFINITIONS_H

#include <boost/pool/detail/mutex.hpp>

namespace ai
{
	namespace lib
	{
		namespace ipc
		{

			/** A handle to IPC wait event. IPC will signalize the user about incoming data by setting such an event.
			Allow usage of an OS native handle to take advatage of OS-specific
			functions like WaitForMultipleObjects(). On the other hand, the usage of a typedef simplifies porting
			to different platforms.
			*/
			typedef HANDLE wait_event;

			/// A type for a mutex. Used only internally in IPC.
			typedef	boost::details::pool::default_mutex mutex;


		}
	}
}
#endif
