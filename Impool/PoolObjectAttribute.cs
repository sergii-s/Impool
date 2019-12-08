using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace Impool
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(PoolObjectGenerator))]
    [Conditional("CodeGeneration")]
    public sealed class PoolObjectAttribute : Attribute
    {
        /// <summary>
        /// For the annotated type, generate <paramref name="features"/>.
        /// </summary>
        /// <param name="features">Value for <see cref="Features"/>.</param>
        public PoolObjectAttribute()
        {
        }
    }
}