using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.ECS.Model;
using Amazon.S3;
using Amazon.S3.Model;

using Brighid.Commands.Cicd.Utils;

using Microsoft.Extensions.Options;

using KeyValuePair = Amazon.ECS.Model.KeyValuePair;
using Task = System.Threading.Tasks.Task;

namespace Brighid.Commands.Cicd.DeployDriver
{
    /// <summary>
    /// Service for running migrations.
    /// </summary>
    public class MigrationsRunner
    {
        private readonly TaskRunner taskRunner;
        private readonly CommandLineOptions options;
        private readonly IAmazonCloudFormation cloudformation = new AmazonCloudFormationClient();
        private readonly IAmazonS3 s3 = new AmazonS3Client();

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationsRunner"/> class.
        /// </summary>
        /// <param name="options">Options from the command line.</param>
        /// <param name="taskRunner">Service used to run a task.</param>
        public MigrationsRunner(
            IOptions<CommandLineOptions> options,
            TaskRunner taskRunner
        )
        {
            this.taskRunner = taskRunner;
            this.options = options.Value;
        }

        /// <summary>
        /// Runs migrations for a specific environment.
        /// </summary>
        /// <param name="config">The config for the environment to run migrations in.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(EnvironmentConfig config, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var migrationsBundleUrl = GetMigrationsBundleUrl();
            var parameters = config.Parameters!;
            var runTaskRequest = new RunTaskRequest
            {
                TaskDefinition = "brighid-migrations:1",
                NetworkConfiguration = new NetworkConfiguration
                {
                    AwsvpcConfiguration = new AwsVpcConfiguration
                    {
                        AssignPublicIp = "ENABLED",
                        Subnets = await GetSubnets(cancellationToken),
                    },
                },
                Overrides = new TaskOverride
                {
                    ContainerOverrides = new List<ContainerOverride>
                    {
                        new ContainerOverride
                        {
                            Name = "migrator",
                            Environment = new List<KeyValuePair>
                            {
                                new KeyValuePair { Name = "MIGRATIONS_BUNDLE_URL", Value = migrationsBundleUrl },
                                new KeyValuePair { Name = "DATABASE_HOST", Value = parameters["DatabaseHost"] },
                                new KeyValuePair { Name = "DATABASE_NAME", Value = parameters["DatabaseName"] },
                                new KeyValuePair { Name = "DATABASE_USER", Value = parameters["DatabaseUser"] },
                                new KeyValuePair { Name = "ENCRYPTED_DATABASE_PASSWORD", Value = parameters["DatabasePassword"] },
                            },
                        },
                    },
                },
            };

            await taskRunner.Run(runTaskRequest, cancellationToken);
        }

        private async Task<List<string>> GetSubnets(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new ListExportsRequest();
            var response = await cloudformation.ListExportsAsync(request, cancellationToken);
            var query = from export in response.Exports where export.Name == "cfn-utilities:SubnetIds" select export.Value.Split(',');
            return query.First().ToList();
        }

        private string GetMigrationsBundleUrl()
        {
            var path = options.ArtifactsLocation!.AbsolutePath.TrimStart('/');
            var request = new GetPreSignedUrlRequest
            {
                BucketName = options.ArtifactsLocation!.Host,
                Key = $"{path}/migrator",
            };

            return s3.GetPreSignedURL(request);
        }
    }
}
