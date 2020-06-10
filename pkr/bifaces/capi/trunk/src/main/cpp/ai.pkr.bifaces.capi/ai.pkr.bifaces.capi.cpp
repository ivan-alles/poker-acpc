#include "stdafx.h"
#include "ai.pkr.bifaces.capi.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Text;
using namespace ai::pkr::metabots;
using namespace ai::lib::utils;
using namespace ai::pkr::metabots;
using namespace ai::pkr::metagame;

/// Use this to deactivate exception catching, can be useful for debugging.
#define CATCH_EXCEPTIONS  1

#if CATCH_EXCEPTIONS

#define EX_TRY try
#define EX_CATCH catch(Exception^ e) {ProcessException(e, errorDescription, errorDescriptionSize); return 0;}

#else

#define EX_TRY 
#define EX_CATCH 

#endif

ref struct ModuleData
{
	static int NextHandle = 1;
	static Dictionary<int, IPlayer^>^ Players = gcnew Dictionary<int, IPlayer^>();
};

gcroot<ModuleData^> md = gcnew ModuleData();

/// Copy a System.String to a C-style buffer.
void CopyStringToBuffer(String ^s, wchar_t * buffer, int bufferSize)
{
	int minLen = Math::Min(bufferSize - 1, s->Length);
	for(int i = 0; i < minLen; ++i)
	{
		buffer[i] = s[i];
	}
	buffer[minLen - 1] = 0;
}

void ProcessException(Exception^ e, wchar_t * errorDescription, int errorDescriptionSize)
{
	String ^ excDescr = e->ToString();
	CopyStringToBuffer(excDescr, errorDescription, errorDescriptionSize);
}

IPlayer^ CreatePlayer(String^ playerName, String^ className, String^ creationParamsFileName)
{
    ClassFactoryParams^ cfp = gcnew ClassFactoryParams(className);
	IPlayer^ iplayer = ClassFactory::CreateInstance<IPlayer^>(cfp);
    if (iplayer != nullptr)
    {
		Props^ creationParams = gcnew Props();
		if(creationParamsFileName != nullptr)
		{
			creationParams = XmlSerializerExt::Deserialize<Props^>(creationParamsFileName);
		}
        iplayer->OnCreate(playerName, creationParams);
    }
    return iplayer;
}


extern "C" 
{
	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_CreatePlayer(const wchar_t * playerName, const wchar_t * className, const wchar_t * creationParamsFileName, 
		wchar_t * errorDescription, int errorDescriptionSize,
		int * playerHandle)
	{
		EX_TRY
		{
			String ^playerNameS = gcnew String(playerName);
			String ^classNameS = gcnew String(className);
			String ^creationParamsFileNameS = gcnew String(creationParamsFileName);

			IPlayer^ player = CreatePlayer(playerNameS, classNameS, creationParamsFileNameS);

			if(player == nullptr)
			{
				return 0;
			}
			*playerHandle = md->NextHandle;
			md->NextHandle++;
			md->Players->Add(*playerHandle, player);
		}
		EX_CATCH;
		return 1;
	}

	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_DeletePlayer(int playerHandle,
		wchar_t * errorDescription, int errorDescriptionSize)
	{
		EX_TRY
		{		
			md->Players->Remove(playerHandle);
		}
		EX_CATCH;
		return 1;
	}

	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_OnServerConnect(int playerHandle, 
		wchar_t * errorDescription, int errorDescriptionSize,
		wchar_t * playerInfoText, int playerInfoTextLength)
	{
		EX_TRY
		{
			PlayerInfo ^ playerInfo = md->Players[playerHandle]->OnServerConnect();
			if(playerInfoText != NULL)
			{
				StringBuilder ^ sb = gcnew StringBuilder();
				sb->Append(String::Format("Name: {0}\n", playerInfo->Name));
				sb->Append(String::Format("Version: {0}\n", playerInfo->Version->ToString()));
				sb->Append("Properties:\n");
				array<String^> ^ propNames = playerInfo->Properties->Names;
				for(int i = 0; i < propNames->Length; ++i)
				{
					String ^ propName = propNames[i];
					String ^ propValue = playerInfo->Properties->Get(propName);
					sb->Append(String::Format("{0}: {1}\n", propName, propValue));
				}
				CopyStringToBuffer(sb->ToString(), playerInfoText, playerInfoTextLength);
			}
		}
		EX_CATCH;
		return 1;
	}

	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_OnSessionBegin(int playerHandle, const wchar_t * sessionName,
		wchar_t * errorDescription, int errorDescriptionSize)
	{
		EX_TRY
		{
			md->Players[playerHandle]->OnSessionBegin(gcnew String(sessionName), nullptr, nullptr);
		}
		EX_CATCH;
		return 1;
	}

	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_OnGameBegin(int playerHandle, const wchar_t * gameString, 
		wchar_t * errorDescription, int errorDescriptionSize)
	{
		EX_TRY
		{
			md->Players[playerHandle]->OnGameBegin(gcnew String(gameString));
		}
		EX_CATCH;
		return 1;
	}

	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_OnActionRequired(int playerHandle, const wchar_t * gameString, 
		wchar_t * errorDescription, int errorDescriptionSize,
		wchar_t * returnedAction, double * returnedAmount)
	{
		EX_TRY
		{
			PokerAction pa = md->Players[playerHandle]->OnActionRequired(gcnew String(gameString));
			switch(pa.Kind)
			{
			case Ak::f: *returnedAction = L'f'; break;
			case Ak::c: *returnedAction = L'c'; break;
			case Ak::r: *returnedAction = L'r'; break;
			default:
				throw "Unexpected response from agent";
			}
			*returnedAmount = pa.Amount;
		}
		EX_CATCH;
		return 1;
	}

}