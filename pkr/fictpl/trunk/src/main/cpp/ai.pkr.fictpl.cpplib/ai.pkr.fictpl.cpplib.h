// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the AIPKRFICTPLAYCPPLIB_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// AIPKRFICTPLAYCPPLIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef AIPKRFICTPLCPPLIB_EXPORTS
#define AIPKRFICTPLCPPLIB_API __declspec(dllexport)
#else
#define AIPKRFICTPLCPPLIB_API __declspec(dllimport)
#endif

#include <stdint.h>

typedef float ChanceValueT;

#ifdef __cplusplus
extern "C" {
#endif

AIPKRFICTPLCPPLIB_API void IncrementGameValue(double * pGameValues, uint32_t gameValuesCount, ChanceValueT * pChanceFactors, uint32_t * pChanceMasks, uint32_t chanceMaskIdx);

AIPKRFICTPLCPPLIB_API void IncrementGameValueNoMasks(double * pGameValues, uint32_t gameValuesCount, ChanceValueT * pChanceFactors);

#ifdef __cplusplus
} 
#endif
