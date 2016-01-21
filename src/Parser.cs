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

      StartJsonArray("actions");
      for (int i = 0; i < nrChildren; i++) {
	ReadAction();
      }
      EndJsonArray(true);

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

      StartJsonArray("events");
      for (int i = 0; i < nrChildren; i++) {
	ReadActionEvent();
      }
      EndJsonArray(true);
      
      Console.WriteLine("}");
    }

    void ReadActionEvent()
    {
      byte expanded = ReadByte();
      byte enabled = ReadByte();
      byte withDialog = ReadByte();
      byte dialogOptions = ReadByte();
      var name = readEventName();
      string displayName = ReadString();
      int hasDescriptor = ReadInt32();

      Console.WriteLine("{");

      FormatJson("expanded", expanded);
      FormatJson("enabled", enabled);
      FormatJson("withDialog", withDialog);
      FormatJson("dialogOptions", dialogOptions);
      FormatJson("name", name);
      FormatJson("displayName", displayName);
      FormatJson("hasDescriptor", hasDescriptor);

      // if (PreSix == false)
      {
	var classID = ReadUnicodeString();
	FormatJson("classID", classID);
	
	var classID2 = ReadTokenOrString();
	FormatJson("classID2", classID2);
      }

      ReadItems();

      Console.WriteLine("}");
    }

    void ReadItems()
    {
      int numberOfItems = ReadInt32();

      FormatJson("#items", numberOfItems);

      StartJsonArray("items");
      for (int i = 0; i < 1 /* numberOfItems */ ; i++)
	{
	  ReadItem();
	}
      EndJsonArray(true);
    } 

    void ReadItem()
    {
      var key = ReadTokenOrString();
      var type = ReadFourByteString();

      Console.WriteLine("{");
      FormatJson("key", key);
      FormatJson("paramType", type);

      switch (type)
	{
	case "enum":
	  ReadEnum();
	  break;
	default:
	  Console.WriteLine($"ReadItem: type {type} unknown!");
	  throw new Exception();
	}

      Console.WriteLine("}");
    }

    void ReadEnum()
    {
      var type = ReadTokenOrString();
      var value = ReadTokenOrString();
      FormatJson("type", type);
      FormatJson("value", value, true);
    }

    string readEventName()
    {
      string text = ReadFourByteString();
      
      if (text == "TEXT")
	{
	  return ReadString();
	}
      else if (text == "long")
	{
	  return ReadFourByteString();
	}
      else
	{
	  Console.WriteLine("Unknown text: " + text);
	  return null;
	}
    }

    // Formatting routines

    void StartJsonArray(string key) => 
      Console.WriteLine($"\"{key}\": [");

    void EndJsonArray(bool last = false) => 
      Console.WriteLine($"]{last ? "" : ","}");

    void FormatJson(string key, string value, bool last = false) => 
      Console.WriteLine($"\"{key}\": \"{value}\"{last ? "" : ","}");

    void FormatJson(string key, int value, bool last = false) => 
      Console.WriteLine($"\"{key}\": \"{value}\"{last ? "" : ","}");

    // Helper routines to read from binary file

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

    string ReadString(int length)
    {
      var buffer = Reader.ReadBytes(length);
      var encoding = Encoding.ASCII;
      return encoding.GetString(buffer);
    }

    public string ReadString() => ReadString(ReadInt32());

    string ReadFourByteString() => ReadString(4).Trim();

    public string ReadTokenOrString()
    {
      int length = ReadInt32();
      return (length == 0) ? ReadFourByteString() : ReadString(length);
    }
  }
}
