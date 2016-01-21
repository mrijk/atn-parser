using System;
using System.IO;
using System.Text;

namespace Atn
{
  class Parser
  {
    BinaryReader Reader {get; set;}

    public void Parse(string file)
    {
      Reader = new BinaryReader(File.Open(file, FileMode.Open));
      int version = ReadInt32();
      var set = ReadUnicodeString();
      byte expanded = ReadByte();
      int nrChildren = ReadInt32();

      Console.WriteLine("{");
      FormatJson("version", version);
      FormatJson("set", set);
      FormatJson("expanded", expanded);
      FormatJson("#actions", nrChildren);

      Console.WriteLine("\"actions\": [");
      for (int i = 0; i < nrChildren; i++) {
	ReadAction();
      }
      Console.WriteLine("]");

      Console.WriteLine("}");
      Reader.Close();
    }

    void ReadAction()
    {      
      int index = ReadInt16();
      byte shiftKey = ReadByte();
      byte commandKey = ReadByte();
      int colorIndex = ReadInt16();
      var name = ReadUnicodeString();
      byte expanded = ReadByte();
      int nrChildren = ReadInt32();

      Console.WriteLine("{");
      FormatJson("index", index);
      FormatJson("shiftKey", shiftKey);
      FormatJson("commandKey", commandKey);
      FormatJson("colorIndex", colorIndex);
      FormatJson("name", name);
      FormatJson("expanded", expanded);
      FormatJson("#events", nrChildren);

      Console.WriteLine("\"events\": [");
      for (int i = 0; i < nrChildren; i++) {
      }
      Console.WriteLine("]");
      
      Console.WriteLine("}");
    }

    void FormatJson(string key, string value, bool last = false) => 
      Console.WriteLine($"\"{key}\": \"{value}\"{last ? "" : ","}");

    void FormatJson(string key, int value, bool last = false) => 
      Console.WriteLine($"\"{key}\": \"{value}\"{last ? "" : ","}");

    byte ReadByte() => Reader.ReadByte();

    int ReadInt16()
    {
      var val = Reader.ReadBytes(2);      
      return val[1] + 256 * val[0];
    }

    int ReadInt32()
    {
      var val = Reader.ReadBytes(4);
      return val[3] + 256 * (val[2] + 256 * (val[1] + 256 * val[0]));
    }

    string ReadUnicodeString(int length)
    {
      length--;	// Strip last 2 zero's
      var buffer = Reader.ReadBytes(2 * length);
      Reader.ReadBytes(2);	// Read and ignore 2 zero's

      for (int i = 0; i < 2 * length; i += 2)
	{
	  byte tmp = buffer[i];
	  buffer[i] = buffer[i + 1];
	  buffer[i + 1] = tmp;
	}

      var encoding = Encoding.Unicode;
      return encoding.GetString(buffer);
    }

    string ReadUnicodeString() => ReadUnicodeString(ReadInt32());
  }
}