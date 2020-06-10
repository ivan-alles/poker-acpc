// ai.pkr.fictplay.cpplib.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ai.pkr.fictplmc.cpplib.h"
//#include <stdio.h>
#include <assert.h>
#include <emmintrin.h>
#include <xmmintrin.h>



extern "C" 
{


AIPKRFICTPLMCCPPLIB_API void IncrementGameValueNoMasks(double * pGameValues, uint32_t gameValuesCount, ChanceValueT * pChanceFactors)
{
	// We can process blocks of 4 numbers. The caller must reserve memory for it.
#if 0
    for (;gameValuesCount > 0; --gameValuesCount)
    {
		*pGameValues++ += *pChanceFactors++;
    }
#elif 0 // Double ChanceValueT
    __m128d a, b;

	// Check alignment
	assert((((long)pChanceFactors) & 0xF) == 0);
	assert((((long)pGameValues) & 0xF) == 0);
	assert(sizeof(ChanceValueT) == 8);
	
	gameValuesCount = (gameValuesCount+3)/4;
	for(; gameValuesCount > 0; gameValuesCount--)
	{
		a = _mm_load_pd(pGameValues);
		b = _mm_load_pd(pChanceFactors);
		a =_mm_add_pd(a,b);
		_mm_store_pd(pGameValues, a);

	    a = _mm_load_pd(pGameValues + 2);
		b = _mm_load_pd(pChanceFactors + 2);
		a =_mm_add_pd(a,b);
		_mm_store_pd(pGameValues + 2, a);

    	pChanceFactors+=4;
		pGameValues+=4;
	}
#else // float ChanceValueT
    __m128d a, b;
	__m128 f;

	// Check alignment
	assert((((long)pChanceFactors) & 0xF) == 0);
	assert((((long)pGameValues) & 0xF) == 0);
	assert(sizeof(ChanceValueT) == 4);
	
	gameValuesCount = (gameValuesCount+3)/4;
	for(; gameValuesCount > 0; gameValuesCount--)
	{
		a = _mm_load_pd(pGameValues);
		f = _mm_load_ps(pChanceFactors);
		b = _mm_cvtps_pd(f);
		a =_mm_add_pd(a,b);
		_mm_store_pd(pGameValues, a);

	    a = _mm_load_pd(pGameValues + 2);
		f = _mm_shuffle_ps(f, f, _MM_SHUFFLE(1, 0, 3, 2));
		b = _mm_cvtps_pd(f);
		a =_mm_add_pd(a, b);
		_mm_store_pd(pGameValues + 2, a);

    	pChanceFactors+=4;
		pGameValues+=4;
	}
#endif

}


}