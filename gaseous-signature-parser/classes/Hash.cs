using System.Security.Cryptography;

namespace gaseous_signature_parser.classes
{
    /// <summary>
    /// Provides functionality to generate MD5 and SHA1 hashes for XML files, which can be used for integrity verification and caching purposes in the signature parsing process.
    /// </summary>
    public static class Hash
    {
        /// <summary>
        /// Generates MD5 and SHA1 hashes for the provided XML file stream. The stream position is reset to the beginning after hashing to allow for subsequent XML processing.
        /// </summary>
        /// <param name="xmlStream">The XML file stream to hash.</param>
        /// <returns>A tuple containing the MD5 and SHA1 hashes as hexadecimal strings.</returns>
        public static (string md5, string sha1) GenerateHashes(FileStream xmlStream)
        {
            string md5Hash;
            string sha1Hash;

            // Compute MD5 and SHA1 hashes
            using (var md5 = MD5.Create())
            {
                byte[] md5HashByte = md5.ComputeHash(xmlStream);
                md5Hash = BitConverter.ToString(md5HashByte).Replace("-", "").ToLowerInvariant();
            }

            // Reset the stream position to the beginning before computing the SHA1 hash
            xmlStream.Position = 0;

            using (var sha1 = SHA1.Create())
            {
                byte[] sha1HashByte = sha1.ComputeHash(xmlStream);
                sha1Hash = BitConverter.ToString(sha1HashByte).Replace("-", "").ToLowerInvariant();
            }

            // Reset the stream position to the beginning for XML loading
            xmlStream.Position = 0;

            return (md5Hash, sha1Hash);
        }
    }
}