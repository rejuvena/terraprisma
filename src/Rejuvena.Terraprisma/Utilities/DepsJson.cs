using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rejuvena.Terraprisma.Utilities
{
    /// <summary>
    ///     Lack-luster <c>.deps.json</c> class with minimal parsing, extracting only bare necessities for library resolution.
    /// </summary>
    public class DepsJson
    {
        /// <summary>
        ///     Provides access to a collection of libraries as well as attributes such as specified paths.
        /// </summary>
        /// <remarks>
        ///     I use a dictionary here instead of a custom type because I don't care enough to create a proper parser.
        /// </remarks>
        [JsonProperty("libraries")]
        public Dictionary<string, Dictionary<string, string>> Libraries = new();
    }
}