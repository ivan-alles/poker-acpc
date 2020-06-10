#pragma once

/*  C API (CAPI) is a C wrapper for pkr bots.

    Conventions for all functions:

    All functions return an integer value, any non-zero value means success.
	Zero return value indicates an error, the description of the error is stored in errorDescription.
	errorDescriptionSize must contain the size of the buffer in characters.
*/

#ifdef AI_PKR_BIFACES_CAPI_EXPORTS
#define AI_PKR_BIFACES_CAPI_API __declspec(dllexport)
#else
#define AI_PKR_BIFACES_CAPI_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" 
{
#endif

	/**
	   Creates a player.
	   @param playerHandle a handle to the created player. It must be used in other function calls. 0 is an invalid handle, is returned on error.
	*/
	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_CreatePlayer(const wchar_t * playerName, const wchar_t * className, const wchar_t * creationParamsFileName, 
		wchar_t * errorDescription, int errorDescriptionSize, 
		int * playerHandle);

	/**
		Deletes a player.
	*/
	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_DeletePlayer(int handle,
		wchar_t * errorDescription, int errorDescriptionSize);

	/**
		Calls OnServerConnect() of the player.
		@param playerInfoText player information will be printed out to this buffer.
		@param playerInfoTextLength size of playerInfoText in characters.
	*/
	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_OnServerConnect(int playerHandle, 
		wchar_t * errorDescription, int errorDescriptionSize,
		wchar_t * playerInfoText, int playerInfoTextLength);

	/**
	    Calls OnSessionBegin().
	*/
	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_OnSessionBegin(int playerHandle, const wchar_t * sessionName, 
		wchar_t * errorDescription, int errorDescriptionSize);
	
	/**
	    Calls OnGameBegin().
	*/
	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_OnGameBegin(int playerHandle, const wchar_t * gameString, 
		wchar_t * errorDescription, int errorDescriptionSize);

	/**
	    Calls CApi_OnActionRequired().
		@param returnedAction contains an action character (r, c, f).
		@param returnedAmount contains action amount.
	*/
	AI_PKR_BIFACES_CAPI_API int __cdecl CApi_OnActionRequired(int playerHandle, const wchar_t * gameString, 
		wchar_t * errorDescription, int errorDescriptionSize,
		wchar_t * returnedAction, double * returnedAmount);


#ifdef __cplusplus
}
#endif


