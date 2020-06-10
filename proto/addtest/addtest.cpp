// addtest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "assert.h"
#include <emmintrin.h>
#include <xmmintrin.h>

#define REP_COUNT 500000
#define ARR_SIZE  10000

#if 0
void AddArrayDouble(double * dst, double * src, int count)
{
	for(; count > 0; count--)
	{
		*dst++ += *src++;
	}
}

#else
void AddArrayDouble(double * dst, double * src, int count)
{
    __m128d a, b;

	// Check alignment
	assert((((long)src) & 0xF) == 0);
	assert((((long)dst) & 0xF) == 0);
	// We can process blocks of 4 numbers 
	assert((count % 4) == 0);
	
	count /= 4;
	for(; count > 0; count--)
	{
		a = _mm_load_pd(dst);
		b = _mm_load_pd(src);
		a =_mm_add_pd(a,b);
		_mm_store_pd(dst, a);

		src+=2;
		dst+=2;

	    a = _mm_load_pd(dst);
		b = _mm_load_pd(src);
		a =_mm_add_pd(a,b);
		_mm_store_pd(dst, a);

    	src+=2;
		dst+=2;
	}
}
#endif

#if 0
void AddArrayInt(__int64 * dst, __int64 * src, int count)
{
	for(; count > 0; count--)
	{
		*dst++ += *src++;
	}
}

#else
void AddArrayInt(__int64 * dst, __int64 * src, int count)
{
    __m128i a, b;

	// Check alignment
	assert((((long)src) & 0xF) == 0);
	assert((((long)dst) & 0xF) == 0);
	// We can process blocks of 4 numbers 
	assert((count % 4) == 0);
	
	count /= 4;
	for(; count > 0; count--)
	{
		a = _mm_load_si128((const __m128i*)dst);
		b = _mm_load_si128((const __m128i*)src);
		a = _mm_add_epi64 (a, b);
		_mm_store_si128 ((__m128i*)dst, a);

		src+=2;
		dst+=2;

		a = _mm_load_si128((const __m128i*)dst);
		b = _mm_load_si128((const __m128i*)src);
		a = _mm_add_epi64 (a, b);
		_mm_store_si128 ((__m128i*)dst, a);

    	src+=2;
		dst+=2;
	}
}
#endif

#if 0
void AddArrayFloat(double * dst, float * src, int count)
{
	count /= 2;
	for(; count > 0; count--)
	{
		*dst++ += *src++;
		*dst++ += *src++;
	}
}

#else
void AddArrayFloat(double * dst, float * src, int count)
{
    __m128 f;
	__m128d fd;
    __m128d d;

	// Check alignment
	assert((((long)src) & 0xF) == 0);
	assert((((long)dst) & 0xF) == 0);
	// We can process blocks of 4 numbers 
	assert((count % 4) == 0);
	
	count /= 4;
	for(; count > 0; count--)
	{
		f = _mm_load_ps(src);

		d = _mm_load_pd(dst);
		fd = _mm_cvtps_pd (f);
		d =_mm_add_pd(d, fd);
		_mm_store_pd(dst, d);

		f = _mm_shuffle_ps(f, f, _MM_SHUFFLE(1, 0, 3, 2));

		d = _mm_load_pd(dst + 2);
		fd = _mm_cvtps_pd (f);
		d =_mm_add_pd(d, fd);
		_mm_store_pd(dst + 2, d);


		src+=4;
		dst+=4;
	}
}
#endif

#if 0
void MulArrayDouble(double * dst, double * src, double factor, int count)
{
	for(; count > 0; count-=2)
	{
		*dst++ += factor * (*src++);
		*dst++ += factor * (*src++);
	}
}

#else
void MulArrayDouble(double * dst, double * src, double factor, int count)
{
    __m128d a, b, f;

	// Check alignment
	assert((((long)src) & 0xF) == 0);
	assert((((long)dst) & 0xF) == 0);
	// We can process blocks of 4 numbers 
	assert((count % 4) == 0);
	
	count /= 4;

	f = _mm_load_pd(&factor);

	for(; count > 0; count--)
	{
		a = _mm_load_pd(dst);
		b = _mm_load_pd(src);
		b = _mm_mul_pd(b, f);
		a =_mm_add_pd(a,b);
		_mm_store_pd(dst, a);

		src+=2;
		dst+=2;

	    a = _mm_load_pd(dst);
		b = _mm_load_pd(src);
		b = _mm_mul_pd(b, f);
		a =_mm_add_pd(a,b);
		_mm_store_pd(dst, a);

    	src+=2;
		dst+=2;
	}
}
#endif


class AlignedPtr
{
	char * ptr;
	
public:

	AlignedPtr(): ptr(NULL)
	{}

	~AlignedPtr()
	{
		delete [] ptr;
	}


	void Alloc(int size)
	{
		ptr = new char[size+15];
		unsigned long mask = 0xF;
		alignedPtr = (char*)((unsigned long)(ptr + 15)  &  ~mask);
	}
	void Free()
	{
		delete [] ptr;
	}
	char * alignedPtr;
};

void Test_AddArrayDouble()
{
	AlignedPtr s, d;
	s.Alloc(ARR_SIZE*8);
	d.Alloc(ARR_SIZE*8);

	double * src = (double*)s.alignedPtr;
	double * dst = (double*)d.alignedPtr;

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		dst[i] = i;
		src[i] = 2*i;
	}

	AddArrayDouble(dst, src, ARR_SIZE);

    for(int i = 0; i < 20; ++i)
	{
		printf(" %f", dst[i]);
	}
	printf("\n");

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		if(dst[i] != 3*i)
		{
			throw "Wrong sum";
		}
	}
	printf("OK\n");
}


void Benchmark_AddArrayDouble()
{
	AlignedPtr s, d;
	s.Alloc(ARR_SIZE*8);
	d.Alloc(ARR_SIZE*8);

	double * src = (double*)s.alignedPtr;
	double * dst = (double*)d.alignedPtr;

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		dst[i] = i;
		src[i] = i;
	}

	unsigned start = GetTickCount();

	for(int i = 0; i < REP_COUNT; ++i)
	{
		AddArrayDouble(dst, src, ARR_SIZE);
	}

	unsigned stop  = GetTickCount();
	unsigned diff = stop - start;
	printf("Ticks: %d\n", diff);
}

void Test_AddArrayFloat()
{
	AlignedPtr s, d;
	s.Alloc(ARR_SIZE*4);
	d.Alloc(ARR_SIZE*8);

	float * src = (float*)s.alignedPtr;
	double * dst = (double*)d.alignedPtr;

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		dst[i] = i;
		src[i] = (float)(2*i);
	}

	AddArrayFloat(dst, src, ARR_SIZE);

    for(int i = 0; i < 20; ++i)
	{
		printf(" %f", dst[i]);
	}
	printf("\n");

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		if(dst[i] != 3*i)
		{
			throw "Wrong sum";
		}
	}
	printf("OK\n");
}

void Benchmark_AddArrayFloat()
{
	AlignedPtr s, d;
	s.Alloc(ARR_SIZE*4);
	d.Alloc(ARR_SIZE*8);

	float * src = (float*)s.alignedPtr;
	double * dst = (double*)d.alignedPtr;

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		dst[i] = i;
		src[i] = (float)i;
	}

	unsigned start = GetTickCount();

	for(int i = 0; i < REP_COUNT; ++i)
	{
		AddArrayFloat(dst, src, ARR_SIZE);
	}

	unsigned stop  = GetTickCount();
	unsigned diff = stop - start;
	printf("Ticks: %d\n", diff);
}

void Benchmark_AddArrayInt()
{
	AlignedPtr s, d;
	s.Alloc(ARR_SIZE*8);
	d.Alloc(ARR_SIZE*8);

	__int64 * src = (__int64*)s.alignedPtr;
	__int64 * dst = (__int64*)d.alignedPtr;

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		dst[i] = i;
		src[i] = i;
	}

	unsigned start = GetTickCount();

	for(int i = 0; i < REP_COUNT; ++i)
	{
		AddArrayInt(dst, src, ARR_SIZE);
	}

	unsigned stop  = GetTickCount();
	unsigned diff = stop - start;
	printf("Ticks: %d\n", diff);
}

void Benchmark_MulArrayDouble()
{
	AlignedPtr s, d;
	s.Alloc(ARR_SIZE*8);
	d.Alloc(ARR_SIZE*8);

	double * src = (double*)s.alignedPtr;
	double * dst = (double*)d.alignedPtr;

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		dst[i] = i;
		src[i] = i;
	}

	unsigned start = GetTickCount();

	for(int i = 0; i < REP_COUNT; ++i)
	{
		MulArrayDouble(dst, src, 3, ARR_SIZE);
	}

	unsigned stop  = GetTickCount();
	unsigned diff = stop - start;
	printf("Ticks: %d\n", diff);
}

int _tmain(int argc, _TCHAR* argv[])
{
	//double x = 1.11;
	//double y = 2.22;
	//Add(&x, &y);
	//printf("%f\n", x);

	//printf("Int64: ");
	//Test<__int64>();

	printf("Float");
	Test_AddArrayFloat();
	Benchmark_AddArrayFloat();


	printf("Double");
	Test_AddArrayDouble();
	Benchmark_AddArrayDouble();


	printf("int");
	//Test_AddArrayInt();
	Benchmark_AddArrayInt();


	printf("Mul double");
	//Test_AddArrayInt();
	Benchmark_MulArrayDouble();

	return 0;
}

