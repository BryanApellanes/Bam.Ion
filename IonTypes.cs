using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public static class IonTypes
    {
        static IonTypes()
        {
            All = new HashSet<Type>
            {
                String,
                Int,
                Long,
                ULong,
                DateTime
            };
        }
        public static HashSet<Type> All { get; private set; }

        public static Type String => typeof(string);
        public static Type Int => typeof(int);
        public static Type Long => typeof(long);
        public static Type ULong => typeof(ulong);
        public static Type DateTime => typeof(DateTime);
    }
}
