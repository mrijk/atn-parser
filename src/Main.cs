using System;

namespace Atn
{
  class Atn
  {
    static void Main(string[] args)
    {
      if (args.Length != 1) {
	Console.WriteLine("Usage...");
      } else {
	var parser = new Parser();
	parser.Parse(args[0]);
      }
    }
  }
}
