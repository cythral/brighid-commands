using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Commands.Service
{
    /// <inheritdoc />
    public class DefaultCommandRepository : ICommandRepository
    {
        private static readonly Func<DatabaseContext, IAsyncEnumerable<Command>> ListCompiledQuery = EF.CompileAsyncQuery<DatabaseContext, Command>(
            (context) =>
                from command in context.Commands.AsQueryable()
                select command
        );

        private static readonly Func<DatabaseContext, CommandType, IAsyncEnumerable<Command>> ListByTypeCompiledQuery = EF.CompileAsyncQuery<DatabaseContext, CommandType, Command>(
            (context, type) =>
                from command in context.Commands.AsQueryable()
                where command.Type == type
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
        public void Add(Command command)
        {
            databaseContext.Add(command);
        }

        /// <inheritdoc />
        public void Delete(Command command)
        {
            databaseContext.Remove(command);
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
        public async Task<IEnumerable<Command>> ListByType(CommandType commandType, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await ListByTypeCompiledQuery(databaseContext, commandType).ToListAsync(cancellationToken);
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
