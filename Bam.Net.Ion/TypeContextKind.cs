using System;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Enumeration of kinds of types.  Describes the source of the `type` member.
    /// </summary>
    public enum TypeContextKind
    {
        Invalid,
        /// <summary>
        /// The name of a type.
        /// </summary>
        TypeName,
        /// <summary>
        /// The full name of a type.
        /// </summary>
        FullName,
        /// <summary>
        /// The assembly qualified name of a type.
        /// </summary>
        AssemblyQualifiedName
    }
}