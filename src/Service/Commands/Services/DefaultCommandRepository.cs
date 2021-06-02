using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Database;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Commands.Commands
{
    /// <inheritdoc />
    public class DefaultCommandRepository : ICommandRepository
    {
        private readonly DatabaseContext databaseContext;
        private readonly Func<DatabaseContext, string, IAsyncEnumerable<Command?>> findCommandByNameCompiledQuery = EF.CompileAsyncQuery<DatabaseContext, string, Command?>(
            (context, name) =>
                from command in context.Commands.AsQueryable()
                where command.Name == name
                select command
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandRepository"/> class.
        /// </summary>
        /// <param name="databaseContext">Service for interacting with the database.</param>
        public DefaultCommandRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        /// <summary>
        /// Find a command by its name.
        /// </summary>
        /// <param name="name">The name of the command to find.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting command.</returns>
        public async Task<Command> FindCommandByName(string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await findCommandByNameCompiledQuery(databaseContext, name).FirstOrDefaultAsync(cancellationToken);
            return result ?? throw new CommandNotFoundException(name);
        }
    }
}
