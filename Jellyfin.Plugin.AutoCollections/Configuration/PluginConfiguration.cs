#nullable enable
using MediaBrowser.Model.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Plugin.AutoCollections.Configuration
{
    // Kept for backward compatibility
    public enum TagMatchingMode
    {
        Or = 0,  // Default - match any tag (backward compatible)
        And = 1  // Match all tags
    }

    // Kept for backward compatibility
    public class TagTitlePair
    {
        public string Tag { get; set; }
        public string Title { get; set; }
        public TagMatchingMode MatchingMode { get; set; }

        // Add parameterless constructor for XML serialization
        public TagTitlePair()
        {
            Tag = string.Empty;
            Title = "Auto Collection";
            MatchingMode = TagMatchingMode.Or; // Default to OR for backward compatibility
        }

        public TagTitlePair(string tag, string title = null, TagMatchingMode matchingMode = TagMatchingMode.Or)
        {
            Tag = tag;
            Title = title ?? GetDefaultTitle(tag);
            MatchingMode = matchingMode;
        }

        private static string GetDefaultTitle(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return "Auto Collection";
                
            // If there are multiple tags, use the first one for the default title
            string firstTag = tag.Split(',')[0].Trim();
            return firstTag.Length > 0
                ? char.ToUpper(firstTag[0]) + firstTag[1..] + " Auto Collection"
                : "Auto Collection";
        }
        
        // Helper method to get individual tags as an array
        public string[] GetTagsArray()
        {
            if (string.IsNullOrEmpty(Tag))
                return new string[0];
                
            return Tag.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();
        }
    }

    /// <summary>
    /// Match types for Auto Collections.
    /// </summary>
    public enum MatchType
    {
        /// <summary>
        /// Match by movie/series title.
        /// </summary>
        Title = 0,

        /// <summary>
        /// Match by genre.
        /// </summary>
        Genre = 1,

        /// <summary>
        /// Match by studio.
        /// </summary>
        Studio = 2,

        /// <summary>
        /// Match by actor.
        /// </summary>
        Actor = 3,

        /// <summary>
        /// Match by director.
        /// </summary>
        Director = 4
    }

    /// <summary>
    /// Represents a title/pattern matching configuration for auto collections.
    /// </summary>
    public class TitleMatchPair
    {
        /// <summary>
        /// Gets or sets the string to match against (title, genre, studio, person name etc.).
        /// </summary>
        public string TitleMatch { get; set; }

        /// <summary>
        /// Gets or sets the collection name. If null, a default name will be generated.
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the match should be case-sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets the type of match to perform.
        /// </summary>
        public MatchType MatchType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleMatchPair"/> class.
        /// </summary>
        public TitleMatchPair()
        {
            TitleMatch = string.Empty;
            CollectionName = "Auto Collection";
            CaseSensitive = false;
            MatchType = MatchType.Title;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleMatchPair"/> class.
        /// </summary>
        /// <param name="titleMatch">The string to match against.</param>
        /// <param name="collectionName">Optional custom collection name.</param>
        /// <param name="caseSensitive">Whether the match should be case-sensitive.</param>
        /// <param name="matchType">The type of match to perform.</param>
        public TitleMatchPair(string titleMatch, string? collectionName = null, bool caseSensitive = false, MatchType matchType = MatchType.Title)
        {
            TitleMatch = titleMatch;
            CollectionName = collectionName ?? GetDefaultCollectionName(titleMatch, matchType);
            CaseSensitive = caseSensitive;
            MatchType = matchType;
        }

        private static string GetDefaultCollectionName(string matchString, MatchType matchType)
        {
            if (string.IsNullOrEmpty(matchString))
                return "Auto Collection";
                
            // Capitalize first letter of match string
            string capitalizedMatch = matchString.Length > 0
                ? char.ToUpper(matchString[0]) + matchString[1..]
                : matchString;

            return matchType switch
            {
                MatchType.Genre => $"{capitalizedMatch} Collection",
                MatchType.Studio => $"{capitalizedMatch} Productions",
                MatchType.Actor => $"{capitalizedMatch}'s Filmography",
                MatchType.Director => $"Directed by {capitalizedMatch}",
                _ => $"{capitalizedMatch} Collection" // Default for Title and any future types
            };
        }
    }

    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the collection matching configurations.
        /// </summary>
        public List<TitleMatchPair> TitleMatchPairs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            // Initialize with empty list - defaults will be added by Plugin.cs only on first run
            TitleMatchPairs = new List<TitleMatchPair>();
        }
    }
}
