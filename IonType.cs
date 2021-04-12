using Bam.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Bam.Ion
{
    public abstract class IonType
    {
        private bool _includeTypeContext;

        [YamlIgnore]
        [JsonIgnore]
        public bool IncludeTypeContext
        {
            get
            {
                return _includeTypeContext;
            }
            set
            {
                _includeTypeContext = value;
                if (_includeTypeContext)
                {
                    SetTypeContext();
                }
            }
        }

        Type _type;
        protected Type Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                if (IncludeTypeContext)
                {
                    SetTypeContext();
                }
            }
        }

        public virtual string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            return Extensions.ToJson(this, pretty, nullValueHandling);
        }

        protected abstract void SetTypeContext();
    }
}
