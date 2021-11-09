namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents a link relation type.
    /// </summary>
    public class LinkRelationType
    {
        public static implicit operator string(LinkRelationType relationType)
        {
            return relationType.ToString();
        }

        public static implicit operator LinkRelationType(string value)
        {
            return new LinkRelationType(value);
        }

        /// <summary>
        /// Creates a new instance of `LinkRelationType`.
        /// </summary>
        public LinkRelationType() { }

        /// <summary>
        /// Creates a new instance of `LinkRelationType` with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public LinkRelationType(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Returns the Value.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
