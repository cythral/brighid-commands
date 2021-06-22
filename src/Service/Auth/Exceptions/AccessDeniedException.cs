using System;

namespace Brighid.Commands.Auth
{
    /// <summary>
    /// Exception that is thrown when the user is not allowed to perform an operation.
    /// </summary>
    public class AccessDeniedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessDeniedException"/> class.
        /// </summary>
        /// <param name="message">Message to use for the exception.</param>
        public AccessDeniedException(string message)
            : base(message)
        {
        }
    }
}
