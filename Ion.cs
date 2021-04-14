using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    /// <summary>
    /// A convenience entry point to Ion functionality, abstracts away the need to know when to use IonMember, IonValue or IonCollection.
    /// </summary>
    public abstract class Ion
    {
        public static IonCollection<T> Read<T>(string json)
        {
            return IonCollection<T>.Read(json);
        }
    }
}
