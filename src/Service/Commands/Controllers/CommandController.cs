using System.Threading.Tasks;

using Brighid.Commands.Core;

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
        private readonly ICommandRepository repository;
        private readonly ILogger<CommandController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandController"/> class.
        /// </summary>
        /// <param name="loader">Service to load commands with.</param>
        /// <param name="repository">Repository to look for commands in.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public CommandController(
            ICommandLoader loader,
            ICommandRepository repository,
            ILogger<CommandController> logger
        )
        {
            this.loader = loader;
            this.repository = repository;
            this.logger = logger;
        }

        /// <summary>
        /// Get command info (headers only).
        /// </summary>
        /// <param name="name">The name of the command to get info for.</param>
        /// <returns>The HTTP Response.</returns>
        [HttpHead("{name}", Name = "Commands:GetCommandInfoHeaders")]
        public async Task<IActionResult> GetCommandInfoHeaders(string name)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            try
            {
                var command = await repository.FindCommandByName(name, HttpContext.RequestAborted);
                HttpContext.Response.Headers["x-command-argcount"] = command.ArgCount.ToString();
                HttpContext.Response.Headers["x-command-options"] = command.ValidOptions.ToArray();
                return Ok();
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
        /// <returns>The HTTP Response.</returns>
        [HttpPost("{name}/execute", Name = "Commands:ExecuteCommand")]
        public async Task<IActionResult> Execute(string name)
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();
            var command = await loader.LoadCommandByName(name, HttpContext.RequestAborted);
            var context = new CommandContext { };

            // Embedded Commands are typically very fast once loaded into the Assembly Load Context.
            // Save a call to the response topic + its transformer by just returning immediately and delegating
            // response responsibility to the adapter, which can most likely transform the response and send it
            // to the user a lot quicker.
            if (command.Type == CommandType.Embedded)
            {
                var result = await command.Run(context, HttpContext.RequestAborted);
                return Ok(result);
            }

            _ = command.Run(context);
            return Accepted();
        }
    }
}
