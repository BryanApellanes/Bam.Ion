﻿using System;
using System.Linq;
using System.Reflection;

namespace Bam.Net.Ion
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Construct an instance of the type
        /// </summary>
        /// <typeparam name="T">The type to cast the result as</typeparam>
        /// <param name="type">The type whose constructor will be called</param>
        /// <param name="ctorArgs">The parameters to pass to the constructor if any</param>
        /// <returns></returns>
        public static T Construct<T>(this Type type, params object[] ctorArgs)
        {
            return (T)type.Construct(ctorArgs);
        }

        /// <summary>
        /// Construct an instance of the specified type passing in the
        /// specified parameters to the constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctorArgs"></param>
        /// <returns></returns>
        public static object Construct(this Type type, params object[] ctorArgs)
        {
            ConstructorInfo ctor = type.GetConstructor(ctorArgs.Select(arg => arg.GetType()).ToArray());
            object val = null;
            if (ctor != null)
            {
                val = ctor.Invoke(ctorArgs);
            }

            return val;
        }
    }
}
