using Microsoft.AspNetCore.WebUtilities;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Provides extension methods to the `byte[]` class.
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Get the base 64 encoded value of the specified `byte[]`.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToBase64UrlEncoded(this byte[] data)
        {
            return WebEncoders.Base64UrlEncode(data);
        }
    }
}
