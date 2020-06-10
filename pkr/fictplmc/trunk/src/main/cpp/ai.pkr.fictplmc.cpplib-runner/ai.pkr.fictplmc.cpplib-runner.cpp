// ai.pkr.fictpl.cpplib-runner.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "ai.pkr.fictplmc.cpplib.h"

#define REP_COUNT 1000000
#define ARR_SIZE  5000

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

void Test_IncrementGameValueNoMasks()
{
	AlignedPtr s, d;
	s.Alloc(ARR_SIZE*8);
	d.Alloc(ARR_SIZE*8);

	ChanceValueT * src = (ChanceValueT*)s.alignedPtr;
	double * dst = (double*)d.alignedPtr;

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		dst[i] = i;
		src[i] = (ChanceValueT)2*i;
	}

	IncrementGameValueNoMasks(dst, ARR_SIZE, src);

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

void Benchmark_IncrementGameValueNoMasks()
{
	AlignedPtr s, d;
	s.Alloc(ARR_SIZE*8);
	d.Alloc(ARR_SIZE*8);

	ChanceValueT * src = (ChanceValueT*)s.alignedPtr;
	double * dst = (double*)d.alignedPtr;

	for(int i = 0; i < ARR_SIZE; ++i)
	{
		dst[i] = i;
		src[i] = (ChanceValueT)i;
	}

	unsigned start = GetTickCount();

	for(int i = 0; i < REP_COUNT; ++i)
	{
		IncrementGameValueNoMasks(dst, ARR_SIZE, src);
	}

	unsigned stop  = GetTickCount();
	unsigned diff = stop - start;
	printf("Ticks: %d\n", diff);
}


int _tmain(int argc, _TCHAR* argv[])
{
	//IncrementGameValueNoMasks(0, 0, 0);
	Test_IncrementGameValueNoMasks();
	Benchmark_IncrementGameValueNoMasks();
	return 0;
}
