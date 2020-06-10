// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the AILIBKMEANSKML_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// AILIBKMEANSKML_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef AILIBKMEANSKML_EXPORTS
#define AILIBKMEANSKML_API __declspec(dllexport)
#else
#define AILIBKMEANSKML_API __declspec(dllimport)
#endif

// This class is exported from the ai.lib.kmeans.kml.dll
class AILIBKMEANSKML_API Cailibkmeanskml {
public:
	Cailibkmeanskml(void);
	// TODO: add your methods here.
};

extern AILIBKMEANSKML_API int nailibkmeanskml;

AILIBKMEANSKML_API int fnailibkmeanskml(void);
