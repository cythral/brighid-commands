using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Middleware for enabling request buffering.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReadableBodyStreamAttribute : Attribute, IAsyncActionFilter
    {
        /// <summary>
        /// Enables buffering on the executing context.
        /// </summary>
        /// <param name="context">The context to enable buffering for.</param>
        /// <param name="next">The action to execute.</param>
        /// <returns>The resulting task.</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.Request.EnableBuffering();
            context.HttpContext.Request.Body.Position = 0;
            await next();
        }
    }
}
