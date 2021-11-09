using Newtonsoft.Json;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents a class that serializes to Ion Json.
    /// </summary>
    public interface IIonJsonable
    {
        /// <summary>
        /// Get the Ion Json representation of the current instance.
        /// </summary>
        /// <returns></returns>
        string ToIonJson();

        /// <summary>
        /// Get the Ion Json representation of the current instance.
        /// </summary>
        /// <param name="pretty">Specify `true` to use indentation.</param>
        /// <param name="nullValueHandling">Specifies null value handling for the underlying JsonSerializer.</param>
        /// <returns></returns>
        string ToIonJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore);
    }
}
