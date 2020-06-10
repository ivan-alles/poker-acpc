// ai.proto.cppcli.cpp : main project file.

#include "stdafx.h"
#include <stdint.h>

using namespace System;
using namespace ai::lib::utils;
using namespace ai::pkr::metastrategy;
using namespace ai::pkr::stdpoker;
using namespace ai::pkr::metagame;

void Callback(CardSet% cs)
{
    Console::WriteLine("{0}", cs.ToString());
}

int main(array<System::String ^> ^args)
{
    Console::WriteLine(L"Hello World");
    uint8_t * bytes = new uint8_t[100];
    for(int i = 0; i < 100; ++i)
    {
        bytes[i] = i;
    }
    String^ s = Props::Global->Get("ai.Root");
    Console::WriteLine(s);

    CardEnum::EnumerateActionDelegate ^ deleg = gcnew CardEnum::EnumerateActionDelegate(Callback);

    CardEnum::Combin(StdDeck::Descriptor, 1, CardSet::Empty, CardSet::Empty, 
        deleg);

    return 0;
}
