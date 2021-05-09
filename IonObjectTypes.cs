using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    /// <summary>
    /// Enumeration of all parent ion types.
    /// </summary>
    public enum IonObjectTypes
    {
        /// <summary>
        /// A complex data value.
        /// </summary>
        Object,

        /// <summary>
        /// A collection, list or array of values.
        /// </summary>
        Collection,

        /// <summary>
        /// A leaf node.
        /// </summary>
        Value
    }
}
