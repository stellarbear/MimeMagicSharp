# MimeMagicSharp
Detects MIME type based on file content or file extension. [Nuget package](https://www.nuget.org/packages/MimeMagicSharp/ is available. Sample are files included.

## Description
This project was originally based on parsing  **/usr/share/mime/magic** (Kali Linux) file (**mime database**). Location may differ. More information about this file structure could be found [here](https://developer.gnome.org/shared-mime-info-spec/) ("the magic files" section). 

This file contains mime type definitions with several rule sets, which are applied iteratively to the given file's content to guess mime type(s).

Original file's format (**old**) is hard to modify according to your needs. I decided to move on to json file format (**new**) with similary capabilities and some new features. New format could be easily modified and updated with new rule sets and mime type definitions.

Both database formats are supported. Conversion from the old to the new database format is supported. Detection by extension can be used only with new file format. Template original and json files **will be** included soon.

## Structure
Each MIME definition contains a list of rule sets. Each rule set contains an hierarchical list of rules. Rule set pseudocode:
```C#
//  Level 0 rules: Rule 0
//  Level 1 rules: Rule 1, Rule 2, Rule 3
//  Level 2 rules: Rule 4
//  Level 3 rules: Rule 5, Rule 6
//	...
```
When this ruleset is applied, result will be calculated in the following manner:
```C#
//  Result (boolean) = 0 && (1 || 2 || 3) && 4 && (5 || 6)...
```
Rule example:
```C#
"Name": "application/x-deb",
"Description": "Debian package",
"Extensions": ["deb"],
"RuleSet": [
    {
    "Rule": [
        {
        "Level": 0,
        "Offset": 0,
        "Range": 0,
        "Data": "213C617263683E",
        "DataUTF8": "!<arch>"
        },
        {
        "Level": 1,
        "Offset": 8,
        "Range": 0,
        "Data": "64656269616E",
        "DataUTF8": "debian"
        }
    ]
    }, "Rule":[...
    ]...
```
- **Level**. Explained earlier.
- **Offset**. Seek start position
- **Range**. Seek interval (0 means fixed position)
- **Data**. Data to seek in hex format
- **Description** and **DataUTF8** are for visual purposes only.

## Usage
```C#
//	Database File (new or old)
string MagicFile = Path.Combine(Environment.CurrentDirectory, "magic"),

//  Parse DB file
using (MimeMagicSharp.CMimeMagicSharp MS = new MimeMagicSharp.CMimeMagicSharp(MagicFile, MimeMagicSharp.EMagicFileType.Json))
{
	//	Mime type detection
	try
	{
		//	Multiple results could be returned
		List<MimeMagicSharp.CType> ContentMIME = MS.ByContent(Path.Combine(Environment.CurrentDirectory, "MimeMagicSharp.dll"));

		//	Only first result will be returned
		List<MimeMagicSharp.CType> ExtensionMIME = MS.ByContent(Path.Combine(Environment.CurrentDirectory, "MimeMagicSharp.dll"), true);
        
    }
	//	Handle errors
    catch (Exception Ex)
    { }
}
```
Convertion from original (old) format to json (new) is supported 
```C#
MimeMagicSharp.CMimeMagicSharp.ConvertFromOriginalToJson("magic_original", "magic_json_convert_test", out string ConvertError);
```

## Limitations
Only first 4096 bytes of file are read during mime detection procedure. You can extend this limit in source code easily.

## Other
Build in vs 2017

You can use or modify the sources however you want
