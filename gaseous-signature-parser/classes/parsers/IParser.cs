using System.Xml;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
    /// <summary>
    /// Common interface for all signature parsers
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parse the XML file and return a RomSignatureObject
        /// </summary>
        /// <param name="xmlFile">Path to the XML signature file</param>
        /// <param name="options">Optional parameters specific to the parser implementation</param>
        /// <returns>Parsed RomSignatureObject</returns>
        RomSignatureObject Parse(string xmlFile, Dictionary<string, object>? options = null);

        /// <summary>
        /// Determine if the provided XML document matches this parser's signature type
        /// </summary>
        /// <param name="xml">XML document to check</param>
        /// <returns>SignatureParser type if matched, otherwise Unknown</returns>
        parser.SignatureParser GetXmlType(XmlDocument xml);
    }
}
