using System.Xml;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.bracketparsers
{
    /// <summary>
    /// Common interface for all signature parsers
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parse the dat file and return a RomSignatureObject
        /// </summary>
        /// <param name="datFile">Path to the signature file</param>
        /// <param name="options">Optional parameters specific to the parser implementation</param>
        /// <returns>Parsed RomSignatureObject</returns>
        RomSignatureObject Parse(string datFile, Dictionary<string, object>? options = null);

        /// <summary>
        /// Determine if the provided dat document matches this parser's signature type
        /// </summary>
        /// <param name="datFile">Content of the dat file to check</param>
        /// <returns>SignatureParser type if matched, otherwise Unknown</returns>
        parser.SignatureParser GetDatType(string datFile);
    }
}