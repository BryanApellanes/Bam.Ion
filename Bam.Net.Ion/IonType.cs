using Newtonsoft.Json;
using System;
using YamlDotNet.Serialization;

namespace Bam.Net.Ion
{
    /// <summary>
    /// A base class for Ion types.
    /// </summary>
    public abstract class IonType 
    {
        public IonType()
        {
            TypeContextKind = TypeContextKind.TypeName;
        }
        
        /// <summary>
        /// Gets or sets the kind of the type context.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public TypeContextKind TypeContextKind
        {
            get;
            set;
        }
        
        private Type _type;
        /// <summary>
        /// Gets or sets the Type context.  Setting this value to a non null value causes IncludeTypeContext to return
        /// true regardless if the IncludeTypeContext value is explicitly set to false.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public Type Type
        {
            get => _type;
            set
            {
                _type = value;
                SetTypeContext();
            }
        }

        /// <summary>
        /// Returns a json string representation of the current `IonObject`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <param name="nullValueHandling">Specifies null handling options for the JsonSerializer.</param>
        /// <returns>json string.</returns>
        public virtual string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            return JsonExtensions.ToJson(this, pretty, nullValueHandling);
        }

        protected abstract void SetTypeContext();
    }
}
