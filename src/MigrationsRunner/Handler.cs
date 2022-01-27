using System;
using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Attributes;
using Lambdajection.CustomResource;

using Microsoft.Extensions.Options;

#pragma warning disable IDE0060 // many parameters here go unused, but they are required nonetheless.

namespace Brighid.Commands.MigrationsRunner
{
    /// <summary>
    /// Handles running database migrations.
    /// </summary>
    [CustomResourceProvider(typeof(Startup))]
    public partial class Handler
    {
        private readonly DatabaseOptions databaseOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Handler"/> class.
        /// </summary>
        /// <param name="databaseOptions">Options to use when connecting to the database.</param>
        public Handler(
            IOptions<DatabaseOptions> databaseOptions
        )
        {
            this.databaseOptions = databaseOptions.Value;
        }

        /// <summary>
        /// Runs migrations.
        /// </summary>
        /// <param name="request">Request to run migrations.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The result of running migrations.</returns>
        public Task<OutputData> Create(CustomResourceRequest<MigrationsRequest> request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AppDomain.CurrentDomain.ExecuteAssemblyByName("MigrationsBundle", "--connection", $@"""{databaseOptions}""");
            return Task.FromResult(new OutputData());
        }

#pragma warning disable CS1591, SA1600 // no documentation required here, only create is used.
        public Task<OutputData> Update(CustomResourceRequest<MigrationsRequest> request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<OutputData> Delete(CustomResourceRequest<MigrationsRequest> request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
#pragma warning restore CS1591, SA1600
    }
}
