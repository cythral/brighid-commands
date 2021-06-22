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
        private static readonly Func<DatabaseContext, IAsyncEnumerable<Command>> ListCompiledQuery = EF.CompileAsyncQuery<DatabaseContext, Command>(
            (context) =>
                from command in context.Commands.AsQueryable()
                select command
        );

        private static readonly Func<DatabaseContext, string, IAsyncEnumerable<Command?>> FindCommandByNameCompiledQuery = EF.CompileAsyncQuery<DatabaseContext, string, Command?>(
            (context, name) =>
                from command in context.Commands.AsQueryable()
                where command.Name == name
                select command
        );

        private readonly DatabaseContext databaseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandRepository"/> class.
        /// </summary>
        /// <param name="databaseContext">Service for interacting with the database.</param>
        public DefaultCommandRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        /// <inheritdoc />
        public async Task Add(Command command, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await databaseContext.AddAsync(command, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Save(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await databaseContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Command>> List(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await ListCompiledQuery(databaseContext).ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Command> FindCommandByName(string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await FindCommandByNameCompiledQuery(databaseContext, name).FirstOrDefaultAsync(cancellationToken);
            return result ?? throw new CommandNotFoundException(name);
        }
    }
}
