using System;
using AlphaXServer;

namespace AlphaConsoleXService
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server().Reset();
            Console.ReadLine();
        }
    }
}
