using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Brighid.Commands.Core;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Commands
{
    /// <summary>
    /// API Route for commands.
    /// </summary>
    [Route("/commands")]
    public class CommandController : Controller
    {
        private readonly ICommandLoader loader;
        private readonly ICommandService service;
        private readonly ICommandRepository repository;
        private readonly ILogger<CommandController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandController"/> class.
        /// </summary>
        /// <param name="loader">Service to load commands with.</param>
        /// <param name="service">Service used to perform operations on commands.</param>
        /// <param name="repository">Repository to look for commands in.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public CommandController(
            ICommandLoader loader,
            ICommandService service,
            ICommandRepository repository,
            ILogger<CommandController> logger
        )
        {
            this.loader = loader;
            this.service = service;
            this.repository = repository;
            this.logger = logger;
        }

        /// <summary>
        /// Get a list of commands.
        /// </summary>
        /// <returns>A list of commands.</returns>
        [Authorize]
        [HttpGet(Name = "Commands:List")]
        public async Task<ActionResult<IEnumerable<Command>>> List()
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            var commands = await service.List(HttpContext.RequestAborted);
            return Ok(commands);
        }

        /// <summary>
        /// Get command parser restrictions.
        /// </summary>
        /// <param name="name">The name of the command to get info for.</param>
        /// <returns>The HTTP Response.</returns>
        [Authorize]
        [HttpGet("{name}/parser-restrictions", Name = "Commands:GetCommandParserRestrictions")]
        public async Task<ActionResult<CommandParserRestrictions>> GetCommandParserRestrictions(string name)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            try
            {
                var command = await repository.FindCommandByName(name, HttpContext.RequestAborted);
                service.EnsureCommandIsAccessibleToPrincipal(command, HttpContext.User);

                var parseInfo = new CommandParserRestrictions
                {
                    ArgCount = command.ArgCount,
                    ValidOptions = command.ValidOptions.ToArray(),
                };

                return Ok(parseInfo);
            }
            catch (CommandRequiresRoleException)
            {
                return Forbid();
            }
            catch (CommandNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="name">The name of the command to execute.</param>
        /// <param name="request">Options to use when running the command.</param>
        /// <returns>The HTTP Response.</returns>
        [HttpPost("{name}/execute", Name = "Commands:ExecuteCommand")]
        [ProducesResponseType(typeof(ExecuteCommandResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ExecuteCommandResponse), (int)HttpStatusCode.Accepted)]
        public async Task<ActionResult<ExecuteCommandResponse>> Execute(string name, [FromBody] ExecuteCommandRequest request)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            try
            {
                var command = await repository.FindCommandByName(name, HttpContext.RequestAborted);
                service.EnsureCommandIsAccessibleToPrincipal(command, HttpContext.User);
                await loader.LoadCommand(command, HttpContext.RequestAborted);

                var context = new CommandContext
                {
                    Arguments = request.Arguments,
                    Options = request.Options,
                };

                // Embedded Commands are typically very fast once loaded into the Assembly Load Context.
                // Save a call to the response topic + its transformer by just returning immediately and delegating
                // response responsibility to the adapter, which can most likely transform the response and send it
                // to the user a lot quicker.
                if (command.Type == CommandType.Embedded)
                {
                    var result = await command.Run(context, HttpContext.RequestAborted);
                    var okResponse = new ExecuteCommandResponse { Response = result, ReplyImmediately = true };
                    return Ok(okResponse);
                }

                _ = command.Run(context);
                var acceptedResponse = new ExecuteCommandResponse { ReplyImmediately = false };
                return Accepted(acceptedResponse);
            }
            catch (CommandRequiresRoleException)
            {
                return Forbid();
            }
            catch (CommandNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
