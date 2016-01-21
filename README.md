atn-parser
==========

Parse PhotoScript action files. This code is extracted from the gimp-sharp
project. It formats the output in a readable form to json so it can easily 
be processed for example with jq.

```sh
$ mono atn-parser.exe Trim.atn | jq .
```

```json
{
  "version": "16",
  "set": "Trim",
  "expanded": "1",
  "#actions": "1",
  "actions": [
    {
      "index": "0",
      "shiftKey": "0",
      "commandKey": "0",
      "colorIndex": "0",
      "name": "Action 1",
      "expanded": "1",
      "#events": "1",
      "events": [
        {
          "expanded": "1",
          "enabled": "1",
          "withDialog": "0",
          "dialogOptions": "0",
          "eventName": "trim"
        }
      ]
    }
  ]
}
```