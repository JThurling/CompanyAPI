using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateMediaTypeAttribute : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if there is an Accept Header
            var acceptHeaderPresent = context.HttpContext.Request.Headers.ContainsKey("Accept");

            // If there isn't an Accept Header, we return a BadRequest
            if (!acceptHeaderPresent)
            {
                context.Result = new BadRequestObjectResult($"Accept header is missing.");
                return;
            }

            // Get The media type from the Accept header
            var mediaType = context.HttpContext.Request.Headers["Accept"].FirstOrDefault();

            // If there isn't any value in the mediaType we Return a BadRequest - other we send back the outMediaType value
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue outMediaType))
            {
                context.Result = new BadRequestObjectResult($"Media type not present. Please add Accept header with the required media type");
                return;
            }

            // Add the outMediaType to the Items in our HttpContext
            context.HttpContext.Items.Add("AcceptHeaderMediaType", outMediaType);
        }
    }
}
