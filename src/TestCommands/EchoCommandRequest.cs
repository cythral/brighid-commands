namespace Brighid.Commands.TestCommands
{
    /// <summary>
    /// Request for a ping command.
    /// </summary>
    public class EchoCommandRequest
    {
        /// <summary>
        /// Gets or sets the message to echo.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
