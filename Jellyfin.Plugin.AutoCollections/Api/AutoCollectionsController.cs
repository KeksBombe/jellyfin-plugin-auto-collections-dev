#nullable enable
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.AutoCollections.Api
{
    /// <summary>
    /// The Auto Collections API controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Authorize(Policy = "DefaultAuthorization")]
    public class AutoCollectionsController : ControllerBase
    {
        private readonly AutoCollectionsManager _autoCollectionsManager;
        private readonly ILogger<AutoCollectionsManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCollectionsController"/> class.
        /// </summary>
        /// <param name="providerManager">Instance of the provider manager.</param>
        /// <param name="collectionManager">Instance of the collection manager.</param>
        /// <param name="libraryManager">Instance of the library manager.</param>
        /// <param name="logger">Instance of the logger.</param>
        /// <param name="applicationPaths">Instance of the application paths.</param>
        public AutoCollectionsController(
            IProviderManager providerManager,
            ICollectionManager collectionManager,
            ILibraryManager libraryManager,
            ILogger<AutoCollectionsManager> logger,
            IApplicationPaths applicationPaths)
        {
            _autoCollectionsManager = new AutoCollectionsManager(
                providerManager,
                collectionManager,
                libraryManager,
                logger,
                applicationPaths);
            _logger = logger;
        }

        /// <summary>
        /// Triggers an update of all auto collections.
        /// </summary>
        /// <response code="204">Collections update started successfully.</response>
        /// <response code="500">An error occurred while updating collections.</response>
        /// <returns>A <see cref="ActionResult"/> indicating success or failure.</returns>
        [HttpPost("Update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateAutoCollections()
        {
            try
            {
                _logger.LogInformation("Manual update of Auto Collections triggered via API");
                await _autoCollectionsManager.ExecuteAutoCollectionsNoProgress()
                    .ConfigureAwait(false);
                _logger.LogInformation("Auto Collections update completed successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Auto Collections");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while updating collections. Check server logs for details." });
            }
        }
    }
}