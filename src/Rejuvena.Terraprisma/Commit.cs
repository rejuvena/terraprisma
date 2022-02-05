using System;
using System.Collections.Generic;

namespace Rejuvena.Terraprisma
{
    /// <summary>
    ///     A commit structure containing minimal data.
    /// </summary>
    public readonly struct Commit
    {
        /// <summary>
        ///     The latest officially supported commit.
        /// </summary>
        public static readonly Commit Latest = new(
            "647454f",
            "647454f4dea0c3ee0fbda1f2a3b7f848b7833d47",
            "1.0.0.0"
        );
        
        /// <summary>
        ///     A list of known and officially supported commits.
        /// </summary>
        public static readonly List<Commit> Commits = new()
        {
            new Commit("647454f", "647454f4dea0c3ee0fbda1f2a3b7f848b7833d47", "1.0.0.0")
        };

        /// <summary>
        ///     Short-form commit used in tModLoader.
        /// </summary>
        public readonly string ShortForm;
        
        /// <summary>
        ///     Manually-mapped long-form commit.
        /// </summary>
        public readonly string LongForm;
        
        /// <summary>
        ///     Earliest supported Terraprisma version.
        /// </summary>
        public readonly string TerraprismaVersion;

        public Version Version => Version.Parse(TerraprismaVersion);

        public Commit(string shortForm, string longForm, string terraprismaVersion)
        {
            ShortForm = shortForm;
            LongForm = longForm;
            TerraprismaVersion = terraprismaVersion;
        }
    }
}