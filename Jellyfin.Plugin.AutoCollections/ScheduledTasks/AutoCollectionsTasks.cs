#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.AutoCollections.ScheduledTasks
{
    /// <summary>
    /// Class ExecuteAutoCollectionsTask. This class cannot be inherited.
    /// Implements the <see cref="IScheduledTask" />.
    /// </summary>
    public sealed class ExecuteAutoCollectionsTask : IScheduledTask
    {
        private readonly ILogger<AutoCollectionsManager> _logger;
        private readonly AutoCollectionsManager _autoCollectionsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteAutoCollectionsTask"/> class.
        /// </summary>
        /// <param name="providerManager">Instance of the <see cref="IProviderManager"/> interface.</param>
        /// <param name="collectionManager">Instance of the <see cref="ICollectionManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{AutoCollectionsManager}"/> interface.</param>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        public ExecuteAutoCollectionsTask(
            IProviderManager providerManager, 
            ICollectionManager collectionManager, 
            ILibraryManager libraryManager, 
            ILogger<AutoCollectionsManager> logger, 
            IApplicationPaths applicationPaths)
        {
            _logger = logger;
            _autoCollectionsManager = new AutoCollectionsManager(
                providerManager, 
                collectionManager, 
                libraryManager, 
                logger, 
                applicationPaths);
        }

        /// <inheritdoc />
        public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting scheduled Auto Collections task");
            return _autoCollectionsManager.ExecuteAutoCollections(progress, cancellationToken);
        }

        /// <inheritdoc />
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // Run this task every 24 hours
            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval,
                IntervalTicks = TimeSpan.FromHours(24).Ticks
            };
        }

        /// <inheritdoc />
        public string Name => "Update Auto Collections";

        /// <inheritdoc />
        public string Key => "AutoCollectionsUpdate";

        /// <inheritdoc />
        public string Description => 
            "Updates dynamic collections based on Title, Genre, Studio, Actor, or Director matching rules";

        /// <inheritdoc />
        public string Category => "Library";
    }
}
