using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Brighid.Commands.Auth;
using Brighid.Commands.Sdk;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Brighid.Commands.Service
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
        [HttpGet(Name = "Commands:List")]
        public async Task<ActionResult<IEnumerable<Command>>> List()
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            var commands = await service.List(HttpContext.RequestAborted);
            return Ok(commands);
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="request">Request object describing the command to create.</param>
        /// <returns>The resulting command.</returns>
        [Authorize(Roles = "CommandManager,Administrator")]
        [HttpPost(Name = "Commands:CreateCommand")]
        public async Task<ActionResult<Command>> CreateCommand([FromBody] CommandRequest request)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            var result = await service.Create(request, HttpContext.User, HttpContext.RequestAborted);
            return Ok(result);
        }

        /// <summary>
        /// Update a command by its name.
        /// </summary>
        /// <param name="name">Name of the command to update.</param>
        /// <param name="request">Request object describing the data to update the command with.</param>
        /// <returns>The deleted command.</returns>
        [Authorize(Roles = "CommandManager,Administrator")]
        [HttpPut("{name}", Name = "Commands:UpdateCommand")]
        public async Task<ActionResult<Command>> UpdateCommand(string name, [FromBody] CommandRequest request)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            try
            {
                var result = await service.UpdateByName(name, request, HttpContext.User, HttpContext.RequestAborted);
                return Ok(result);
            }
            catch (CommandNotFoundException)
            {
                return NotFound();
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Delete a command by its name.
        /// </summary>
        /// <param name="name">Name of the command to delete.</param>
        /// <returns>The deleted command.</returns>
        [Authorize(Roles = "CommandManager,Administrator")]
        [HttpDelete("{name}", Name = "Commands:DeleteCommand")]
        public async Task<ActionResult<Command>> DeleteCommand(string name)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            try
            {
                var result = await service.DeleteByName(name, HttpContext.User, HttpContext.RequestAborted);
                return Ok(result);
            }
            catch (CommandNotFoundException)
            {
                return NotFound();
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Get command parameters.
        /// </summary>
        /// <param name="name">The name of the command to get parameters for.</param>
        /// <returns>The parameters that belong to the requested command.</returns>
        [Authorize]
        [HttpGet("{name}/parameters", Name = "Commands:GetCommandParameters")]
        public async Task<ActionResult<IEnumerable<CommandParameter>>> GetCommandParameters(string name)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            try
            {
                var command = await service.GetByName(name, HttpContext.User, HttpContext.RequestAborted);
                return Ok(command.Parameters);
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
        /// <param name="sourceSystem">The system where the command execution is being requested from.</param>
        /// <param name="sourceSystemId">The channel within the source system where the command execution is being requested from.</param>
        /// <returns>The HTTP Response.</returns>
        [HttpPost("{name}/execute", Name = "Commands:ExecuteCommand")]
        [ReadableBodyStream]
        [ProducesResponseType(typeof(ExecuteCommandResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ExecuteCommandResponse), (int)HttpStatusCode.Accepted)]
        public async Task<ActionResult<ExecuteCommandResponse>> Execute(
            string name,
            [FromHeader(Name = "x-source-system")] string? sourceSystem = null,
            [FromHeader(Name = "x-source-system-id")] string? sourceSystemId = null
        )
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            try
            {
                var command = await service.GetByName(name, HttpContext.User, HttpContext.RequestAborted);
                await service.LoadCommand(command, HttpContext.RequestAborted);
                var context = new CommandContext(HttpContext.Request.Body, HttpContext.User, sourceSystem!, sourceSystemId!);

                // Embedded Commands are typically very fast once loaded into the Assembly Load Context.
                // Save a call to the response topic + its transformer by just returning immediately and delegating
                // response responsibility to the adapter, which can most likely transform the response and send it
                // to the user a lot quicker.
                if (command.Type == CommandType.Embedded)
                {
                    var result = await command.Run(context, HttpContext.RequestAborted);
                    var okResponse = new ExecuteCommandResponse { Response = result.Message, ReplyImmediately = true, Version = command.Version };
                    return Ok(okResponse);
                }

                _ = command.Run(context);
                var acceptedResponse = new ExecuteCommandResponse { ReplyImmediately = false, Version = command.Version };
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
