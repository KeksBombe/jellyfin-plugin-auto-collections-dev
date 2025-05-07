#nullable enable
using System;
using System.Collections.Generic;
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
        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            IServiceProvider serviceProvider)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _serviceProvider = serviceProvider;

            // Initialize configuration with defaults if needed
            InitializeConfigurationIfNeeded();
        }

        /// <inheritdoc />
        public override string Name => "Auto Collections";

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public override string Description => "Creates dynamic collections based on Title, Genre, Studio, Actor, or Director matching with custom collection names";

        /// <inheritdoc />
        private readonly Guid _id = new Guid("06ebf4a9-1326-4327-968d-8da00e1ea2eb");
        public override Guid Id => _id;

        /// <summary>
        /// Gets the AutoCollectionsManager instance.
        /// </summary>
        /// <returns>The AutoCollectionsManager instance.</returns>
        internal AutoCollectionsManager GetAutoCollectionsManager()
        {
            if (_autoCollectionsManager == null)
            {
                _autoCollectionsManager = new AutoCollectionsManager(
                    _serviceProvider.GetRequiredService<IProviderManager>(),
                    _serviceProvider.GetRequiredService<ICollectionManager>(),
                    _serviceProvider.GetRequiredService<ILibraryManager>(),
                    _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AutoCollectionsManager>(),
                    _serviceProvider.GetRequiredService<IApplicationPaths>());
            }

            return _autoCollectionsManager;
        }

        /// <summary>
        /// Initializes the plugin configuration with defaults if needed.
        /// </summary>
        private void InitializeConfigurationIfNeeded()
        {
            // Check if this is the first time the plugin is being loaded or using old config
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
