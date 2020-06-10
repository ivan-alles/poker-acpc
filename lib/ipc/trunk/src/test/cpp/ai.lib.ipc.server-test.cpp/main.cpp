#include "stdafx.h"
#include <ai.lib.ipc.cpp/message.h>
#include <ai.lib.ipc.cpp/server.h>
#include <boost/program_options/options_description.hpp>
#include <boost/program_options/variables_map.hpp>
#include <boost/program_options/parsers.hpp>


using namespace std;
using namespace boost::program_options;
using namespace ai::lib;

std::set<ipc::connection_ptr> active_connections;

void process_ipc_event(ipc::server & s)
{
	ipc::server::event_t e;
	while(s.deque_input_event(e))
	{
		switch(e.kind)
		{
		case ipc::server::connect:
			cout << "Connected\n";
			active_connections.insert(e.connection);
			break;
		case ipc::server::disconnect:
			cout << "Disonnected\n";
			active_connections.erase(e.connection);
			break;
		case ipc::server::rx_message:
			std::cout << "rx: ";
			for(std::size_t i = 0; i < e.message.size(); ++i)
			{
				std::cout << (char)e.message.data()[i];
			}
			std::cout << "\n";		
			// Send back to all clients
			BOOST_FOREACH(ipc::connection_ptr c, active_connections)
			{
				c->send(e.message);
			}
			break;
		}
	}
}

int main(int argc, char* argv[])
{
	try
	{
		options_description desc("Allowed options");
		desc.add_options()
			("help", "produce help message")
			("port", value<string>(), "port number")
			;

		variables_map vm;
		store(parse_command_line(argc, argv, desc), vm);
		notify(vm);    

		if (vm.count("help")) 
		{
			cout << desc << "\n";
			return 1;
		}

		if (vm.count("port") == 0) 
		{
			cout << "Port was not set.\n";
			cout << desc << "\n";
			return 1;
		}

		ipc::server s;
		s.start(vm["port"].as<string>().c_str());

		for(;;)
		{
			ipc::wait_event we = s.get_wait_event();
			DWORD wait_result = WaitForSingleObject(we, 2000);
			switch(wait_result)
			{
			case WAIT_OBJECT_0:
				process_ipc_event(s);
				break;
			case WAIT_TIMEOUT:
				//if(ic.is_connected())
				//{
				//	message msg("hallo", 6);
				//	ic.send(msg);
				//}
				break;
			default:
				throw std::runtime_error("WaitForSingleObject failed");
			}
		}

		s.stop();


	}
	catch (std::exception& e)
	{
		std::cerr << "Exception: " << e.what() << "\n";
	}

	return 0;
}
