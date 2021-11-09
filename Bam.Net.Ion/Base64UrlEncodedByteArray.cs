namespace Bam.Net.Ion
{
    /// <summary>
    /// A base 64 encoded byte array.
    /// </summary>
    public class Base64UrlEncodedByteArray
    {
        public Base64UrlEncodedByteArray() { }

        string _value;
        /// <summary>
        /// Gets or sets the base 64 encoded value.
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _array = value.FromBase64UrlEncoded();
            }
        }

        byte[] _array;
        /// <summary>
        /// Gets or sets the byte array.
        /// </summary>
        public byte[] Array
        {
            get
            {
                return _array;
            }
            set
            {
                _array = value;
                _value = value.ToBase64UrlEncoded();
            }
        }
    }
}
