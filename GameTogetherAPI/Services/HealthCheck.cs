using Microsoft.Extensions.Diagnostics.HealthChecks;
using GameTogetherAPI.Database;

namespace GameTogetherAPI.Services {
    /// <summary>
    /// Provides a health check implementation to verify database connectivity and basic API status.
    /// </summary>
    public class HealthCheck : IHealthCheck {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheck"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context used to check database connectivity.</param>
        public HealthCheck(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Checks the health status of the API and verifies database connection.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A healthy or unhealthy <see cref="HealthCheckResult"/>.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default) {
            try {
                // Database connectivity check
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

                if (canConnect) {
                    return HealthCheckResult.Healthy("Database connected successfully.");
                }
                else {
                    return HealthCheckResult.Unhealthy("Cannot connect to the database.");
                }
            }
            catch (Exception ex) {
                // Return unhealthy if any exception occurs
                return HealthCheckResult.Unhealthy($"Exception during health check: {ex.Message}");
            }
        }
    }
}
