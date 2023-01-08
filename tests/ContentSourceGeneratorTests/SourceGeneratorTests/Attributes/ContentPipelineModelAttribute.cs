using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentSourceGeneratorTests.SourceGeneratorTests.Attributes
{
    /// <summary>
    /// Enables the content for use in the content pipeline
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal sealed class ContentPipelineModelAttribute : Attribute
    {
        /// <summary>
        /// the constructor for ContentApiModelAttribute
        /// </summary>
        /// <param name="group"></param>
        /// <exception cref="ArgumentException">if group is null or empty</exception>
        public ContentPipelineModelAttribute(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new ArgumentException($"'{nameof(group)}' cannot be null or empty.", nameof(group));
            }

            Group = group;
        }

        public string Group { get; }
    }
}
