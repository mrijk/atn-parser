using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Atn
{
  class Parser
  {
    BinaryReader Reader {get; set;}

    Dictionary<string, Action> Lookup {get;}

    public Parser()
    {
      Lookup = new Dictionary<string, Action>() {
	["bool"] = ParseBool,
	["doub"] = ParseDouble,
	["enum"] = ParseEnum,
	["long"] = ParseLong,
	["name"] = ParseName,
	["obj"]  = ParseReference,
	["prop"] = ParseProperty,
	["Enmr"] = ParseEnmr,
	["UntF"] = ParseDoubleWithUnit,
	["VlLs"] = ParseList
      };
    }

    void ParseType(string type)
    {
      Action func;
      if (Lookup.TryGetValue(type, out func))
	{
	  func();
	}
      else
	{
	  Console.WriteLine($"ReadItem: type {type} unknown!");
	  throw new Exception();
	}
    }

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
      for (int i = 0; i < numberOfItems; i++)
	{
	  ReadItem(i == numberOfItems - 1);
	}
      EndJsonArray(true);
    } 

    void ReadItem(bool last)
    {
      var key = ReadTokenOrString();
      var type = ReadFourByteString();

      Console.WriteLine("{");
      FormatJson("key", key);
      FormatJson("paramType", type);

      ParseType(type);
      Console.WriteLine("}" + ((last) ? "" : ","));
    }

    void ParseBool()
    {
      var value = ReadByte();
      FormatJson("value", value, true);      
    }

    void ParseDouble()
    {
      var value = ReadDouble();
      FormatJson("value", value, true);
    }

    void ParseDoubleWithUnit()
    {
      var units = ReadFourByteString();
      var value = ReadDouble();
      FormatJson("units", units);
      FormatJson("value", value, true);
    }

    void ParseEnum()
    {
      var type = ReadTokenOrString();
      var value = ReadTokenOrString();
      FormatJson("type", type);
      FormatJson("value", value, true);
    }

    void ParseLong()
    {
      var value = ReadInt32();
      FormatJson("value", value, true);
    }

    void ParseName()
    {
      var classID = ReadTokenOrUnicodeString();
      var classID2 = ReadTokenOrString();
      var key = ReadTokenOrUnicodeString();
      FormatJson("classID", classID);
      FormatJson("classID2", classID2);
      FormatJson("key", key, true);
    }

    void ParseReference()
    {
      int number = ReadInt32();
      FormatJson("#referenceItems", number);
      StartJsonArray("referenceItems");
      for (int i = 0; i < number; i++) {
	  ParseReferenceItem(i == number - 1);
      }
      EndJsonArray(true);
    }

    void ParseReferenceItem(bool last)
    {
      Console.WriteLine("{");
      var type = ReadFourByteString();
      FormatJson("type", type);
      ParseType(type);
      Console.WriteLine("}" + ((last) ? "" : ","));
    }

    void ParseList()
    {
      int number = ReadInt32();
      FormatJson("#listItems", number);

      StartJsonArray("listItems");
      for (int i = 0; i < number; i++) {
	  ParseListItem(i == number - 1);
      }
      EndJsonArray(true);
    }

    void ParseEnmr()
    {
      var classID = ReadTokenOrUnicodeString();
      var key = ReadTokenOrString();
      var type = ReadTokenOrString();
      var value = ReadTokenOrString();
      FormatJson("classID", classID);
      FormatJson("key", key);
      FormatJson("type#1", type);
      FormatJson("value", value, true);
    }

    void ParseProperty()
    {
      var classID = ReadTokenOrUnicodeString();
      var classID2 = ReadTokenOrUnicodeString();
      var key = ReadTokenOrString();
      FormatJson("classID", classID);
      FormatJson("classID2", classID2);
      FormatJson("key", key, true);
    }

    void ParseListItem(bool last)
    {
      Console.WriteLine("{");
      var type = ReadFourByteString();
      FormatJson("paramType", type);
      ParseType(type);
      Console.WriteLine("}" + ((last) ? "" : ","));
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

    void FormatJson(string key, double value, bool last = false) => 
      Console.WriteLine($"\"{key}\": \"{value}\"{last ? "" : ","}");

    // Helper routines to read from binary file

    byte[] ReadBytes(int length) => Reader.ReadBytes(length);

    byte ReadByte() => Reader.ReadByte();

    int ReadInt16(byte[] b) => b[1] + 256 * b[0];

    int ReadInt16() => ReadInt16(ReadBytes(2));

    int ReadInt32(byte[] b) => b[3] + 256 * (b[2] + 256 * (b[1] + 256 * b[0]));

    int ReadInt32() => ReadInt32(ReadBytes(4));

    double ReadDouble()
    {
      var buffer = ReadBytes(8);
      Array.Reverse(buffer);
      var memoryStream = new MemoryStream(buffer);
      var reader = new BinaryReader(memoryStream);
      return reader.ReadDouble();
    }

    string ReadUnicodeString(int length)
    {
      length--;	// Strip last 2 zero's
      var buffer = Reader.ReadBytes(2 * length);
      ReadBytes(2);	// Read and ignore 2 zero's

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

    string ReadString(int length) => 
      Encoding.ASCII.GetString(ReadBytes(length));

    string ReadString() => ReadString(ReadInt32());

    string ReadFourByteString() => ReadString(4).Trim();

    string ReadTokenOrString()
    {
      int length = ReadInt32();
      return (length == 0) ? ReadFourByteString() : ReadString(length);
    }

    string ReadTokenOrUnicodeString()
    {
      int length = ReadInt32();
      return (length == 0) ? ReadFourByteString() : ReadUnicodeString(length);
    }
  }
}
