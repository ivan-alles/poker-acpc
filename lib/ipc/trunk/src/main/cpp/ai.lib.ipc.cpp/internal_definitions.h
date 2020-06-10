#ifndef AI_LIB_IPC_SERVER_CPP_IPC_INTERNAL_DEFINITIONS_H
#define AI_LIB_IPC_SERVER_CPP_IPC_INTERNAL_DEFINITIONS_H

namespace ai
{
	namespace lib
	{
		namespace ipc
		{
			inline void create_wait_event(wait_event & we) 
			{
				we = ::CreateEvent(NULL, FALSE, FALSE, NULL);
			}

			inline void set_wait_event(wait_event & we) 
			{ 
				::SetEvent(we); 
			}

			inline void delete_wait_event(wait_event & we) 
			{
				::CloseHandle(we);
			}
		}
	}
}

#endif