# Gaseous Signature Parser

This package provides parsing of TOSEC and other DAT files for the Gaseous Server project (see https://github.com/gaseous-project/gaseous-server).

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