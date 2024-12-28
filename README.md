# Gaseous Signature Parser

This package provides parsing of TOSEC and other DAT files for the Gaseous Server project (see https://github.com/gaseous-project/gaseous-server).

## Supported DATs
* TOSEC: https://www.tosecdev.org/downloads/category/56-2023-01-23
* MAME Arcade and MAME Mess: https://www.progettosnaps.net/dats/MAME
* No-Intro: https://no-intro.org - both standard XML and DB
* Redump: http://redump.org/downloads/
* Amiga `whdload_db.xml` (see notes): https://github.com/BlitterStudio/amiberry/blob/master/whdboot/game-data/whdload_db.xml

## How to use

* Install the package into your dotnet project
* Add a reference to your class to ```gaseous_signature_parser```
* Add a reference to your class to ```gaseous_signature_parser.models.RomSignatureObject```
* Example:
```c#
string xmlFilePath = "<path to xml file>"

parser Parser = new parser();
RomSignatureObject signatureObject = Parser.ParseSignatureDAT(xmlFilePath);
```

The definition for RomSignatureObject can be found here: https://github.com/gaseous-project/gaseous-signature-parser/blob/main/gaseous-signature-parser/models/RomSignatureObject.cs

## Notes
When using the TestApp, `whdload_db.xml` support requires that the extension be changed to `.dat`: `whdload_db.dat`