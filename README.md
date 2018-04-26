# MimeSharp
Detects MIME type based on file content or file extension

## Description
This project was originally based on parsing  /usr/share/mime/magic (Kali Linux path) file (location may differ). More information about file structure [here](https://developer.gnome.org/shared-mime-info-spec/) at "The magic files" section. This file contains mime type definitions with a number of rule sets. They are applied iteratively to the file content to guess given file mime type.

Original file's format (calling **old**) is hard to modify it according to your (mine, for instance) needs. I decided to move on to json file format (calling **new**) with similary capabilities and some new features. New format could be easily modified and updated with new rule sets

Both formats are supported for now. Conversion from the old to the new format is supported too. Detection by extension can be used only with new file format. Template original and json files **will be** included soon.

## Structure
Each MIME definition contains a list of rule sets. Each rule set contains an hierarchical list of rules.
```C#
//  Level 0 rules: Rule 0
//  Level 1 rules: Rule 1, Rule 2, Rule 3
//  Level 2 rules: Rule 4
//  Level 3 rules: Rule 5, Rule 6
//	...
```
This ruleset is applied in the following manner.
```C#
//  Result = 0 && (1 || 2 || 3) && 4 && (5 || 6)...
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
using (MimeSharp.CMimeSharp MS = new MimeSharp.CMimeSharp(MagicFile, MimeSharp.EMagicFileType.Json, out string ErrorMessage))
{
    //  If no errors occured during reading
    if (ErrorMessage == null)
    {
		//	
        try
        {
            List<MimeSharp.CType> ContentMIME = MS.ByContent(Path.Combine(Environment.CurrentDirectory, "MimeSharp.dll"));
            List<MimeSharp.CType> ExtensionMIME = MS.ByContent(Path.Combine(Environment.CurrentDirectory, "MimeSharp.dll"), true);
        }
		//	Catch IO exception if occured
        catch (IOException) { }
    }
	//	Handle errors
    else
    { }
}
```
Convertion from original (old) format to json (new) is supported 
```C#
MimeSharp.CMimeSharp.ConvertFromOriginalToJson("magic_original", "magic_json_convert_test", out string ConvertError);
```

## Limitations
Only first 4096 bytes of file are read during mime detection procedure. You can extend this limit in source code easily.

## Other
Build in vs 2017

You can use or modify the sources however you want
