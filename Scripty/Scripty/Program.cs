using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Scripty.Engine;
using Gameboy32;

namespace Scripty
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config("scripty.xml");
            Rom rom = new Rom("rom.gba");
            Decompiler decomp = new Decompiler(rom, config);
                foreach (string line in decomp.Decompile(0x008015c0))
                {
                    Console.WriteLine(line);
                }
            Console.WriteLine("done");
            Console.ReadLine();
        }
    }
}
