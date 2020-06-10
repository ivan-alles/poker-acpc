#include "stdafx.h"

#include <ai.lib.ipc.cpp/message.h>
#include <ai.lib.ipc.cpp/client.h>

using namespace std;
using namespace ai::lib;

void process_ipc_event(ipc::client & ic)
{
	ipc::client::event_t e;
	while(ic.deque_input_event(e))
	{
		switch(e.kind)
		{
			case ipc::client::connect:
				cout << "Connected\n";
				break;
			case ipc::client::disconnect:
				cout << "Disonnected\n";
				break;
			case ipc::client::rx_message:
				std::cout << "rx: ";
				for(std::size_t i = 0; i < e.message.size(); ++i)
				{
					std::cout << (char)e.message.data()[i];
				}
				std::cout << "\n";			
				break;
		}
	}
}

int main(int argc, char* argv[])
{
	try
	{
		if (argc != 2)
		{
			std::cerr << "Usage: ai.lib.ipc.client-test.cpp host:port\n";
			return 1;
		}

		ipc::client ic;
		ic.start(argv[1]);

		for(;;)
		{
			ipc::wait_event we = ic.get_wait_event();
			DWORD wait_result = WaitForSingleObject(we, 2000);
			switch(wait_result)
			{
			case WAIT_OBJECT_0:
				process_ipc_event(ic);
				break;
			case WAIT_TIMEOUT:
				if(ic.is_connected())
				{
					ipc::message msg("hallo", 6);
					ic.send(msg);
				}
				break;
			default:
				throw std::runtime_error("WaitForSingleObject failed");
			}
		}

		ic.stop();
		

	}
	catch (std::exception& e)
	{
		std::cerr << "Exception: " << e.what() << "\n";
	}

	return 0;
}