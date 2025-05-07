#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.AutoCollections.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.AutoCollections
{
    /// <summary>
    /// The main plugin class for Auto Collections.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly IServiceProvider _serviceProvider;
        private AutoCollectionsManager? _autoCollectionsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Application paths.</param>
        /// <param name="xmlSerializer">XML serializer.</param>
        /// <param name="serviceProvider">Service provider for dependency injection.</param>
        /// <exception cref="ArgumentNullException">Thrown when a required dependency is null.</exception>
        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            IServiceProvider serviceProvider)
            : base(applicationPaths, xmlSerializer)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Instance = this;

            try
            {
                // Initialize configuration with defaults if needed
                InitializeConfigurationIfNeeded();
                ValidateConfiguration();
            }
            catch (Exception ex)
            {
                var logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Plugin>();
                logger?.LogError(ex, "Error initializing plugin configuration");
                throw;
            }
        }

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public override string Name => "Auto Collections";

        /// <inheritdoc />
        public override string Description => 
            "Creates dynamic collections based on Title, Genre, Studio, Actor, or Director matching with custom collection names";

        /// <inheritdoc />
        private readonly Guid _id = new Guid("06ebf4a9-1326-4327-968d-8da00e1ea2eb");
        public override Guid Id => _id;

        /// <summary>
        /// Gets the AutoCollectionsManager instance, creating it if necessary.
        /// </summary>
        /// <returns>The AutoCollectionsManager instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when required services cannot be resolved.</exception>
        internal AutoCollectionsManager GetAutoCollectionsManager()
        {
            if (_autoCollectionsManager == null)
            {
                var logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<AutoCollectionsManager>()
                    ?? throw new InvalidOperationException("Failed to create logger");

                var providerManager = _serviceProvider.GetRequiredService<IProviderManager>();
                var collectionManager = _serviceProvider.GetRequiredService<ICollectionManager>();
                var libraryManager = _serviceProvider.GetRequiredService<ILibraryManager>();
                var applicationPaths = _serviceProvider.GetRequiredService<IApplicationPaths>();

                _autoCollectionsManager = new AutoCollectionsManager(
                    providerManager,
                    collectionManager,
                    libraryManager,
                    logger,
                    applicationPaths);
            }

            return _autoCollectionsManager;
        }

        /// <summary>
        /// Initializes the plugin configuration with defaults if needed.
        /// </summary>
        private void InitializeConfigurationIfNeeded()
        {
            // Check if this is the first time the plugin is being loaded
            bool needsInitialization = Configuration.TitleMatchPairs == null || Configuration.TitleMatchPairs.Count == 0;
            
            if (needsInitialization)
            {
                // Add default collections with examples of different matching types
                Configuration.TitleMatchPairs = new List<TitleMatchPair>
                {
                    // Title-based collections
                    new TitleMatchPair("Marvel", "Marvel Universe", matchType: Configuration.MatchType.Title),
                    new TitleMatchPair("Star Wars", "Star Wars Collection", matchType: Configuration.MatchType.Title),
                    
                    // Genre-based collections
                    new TitleMatchPair("Action", "Action Movies", matchType: Configuration.MatchType.Genre),
                    new TitleMatchPair("Comedy", "Comedy Collection", matchType: Configuration.MatchType.Genre),
                    
                    // Studio-based collections
                    new TitleMatchPair("Disney", "Disney Productions", matchType: Configuration.MatchType.Studio),
                    
                    // Actor-based collections
                    new TitleMatchPair("Tom Hanks", "Tom Hanks Filmography", matchType: Configuration.MatchType.Actor),
                    
                    // Director-based collections
                    new TitleMatchPair("Christopher Nolan", "Nolan Collection", matchType: Configuration.MatchType.Director)
                };

                // For backward compatibility (empty, as we're using title-based matching)
                Configuration.TagTitlePairs = new List<TagTitlePair>();
                Configuration.Tags = Array.Empty<string>();

                SaveConfiguration();
            }
        }

        /// <summary>
        /// Validates the current configuration.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
        private void ValidateConfiguration()
        {
            if (Configuration.TitleMatchPairs == null)
            {
                throw new InvalidOperationException("TitleMatchPairs collection cannot be null");
            }

            // Check for duplicate collection names
            var duplicateNames = Configuration.TitleMatchPairs
                .GroupBy(p => p.CollectionName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNames.Any())
            {
                throw new InvalidOperationException(
                    $"Duplicate collection names found: {string.Join(", ", duplicateNames)}");
            }

            // Validate each title match pair
            foreach (var pair in Configuration.TitleMatchPairs)
            {
                if (string.IsNullOrWhiteSpace(pair.TitleMatch))
                {
                    throw new InvalidOperationException(
                        $"Empty match string found for collection: {pair.CollectionName}");
                }

                if (string.IsNullOrWhiteSpace(pair.CollectionName))
                {
                    throw new InvalidOperationException(
                        $"Empty collection name found for match: {pair.TitleMatch}");
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configurationpage.html"
                }
            };
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _autoCollectionsManager?.Dispose();
            base.Dispose();
        }
    }
}
