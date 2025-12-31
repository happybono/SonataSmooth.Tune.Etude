#include "pch.h"
#include "SonataSmooth.Tune.Etude.h"

using namespace System;

int main(array<System::String^>^ args)
{
    Etude::Run();
    Console::WriteLine("Press Enter to exit.");
    Console::ReadLine();
    return 0;
}