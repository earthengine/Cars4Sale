using Cars4Sale.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Cars4Sale.Attributes
{
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "ApiKey";

        public bool Optional = false;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var headers = context.HttpContext.Request.Headers;

            if (await CheckCriteria(!headers.TryGetValue(APIKEYNAME, out var extractedApiKey),
                    "Api Key was not provided", context, next)) { return; }

            if (await CheckCriteria(!Guid.TryParse(extractedApiKey.ToString(), out var apiKey),
                    "Api Key is not valid", context, next)) { return; }

            if (await CheckCriteria(!ApiClient.Clients.ContainsKey(apiKey),
                    "Api Key is not valid", context, next)) { return; }

            context.HttpContext.Items.Add("current_client", ApiClient.Clients[apiKey]);
            await next();
        }

        private async Task<bool> CheckCriteria(bool criteria, string msg, ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (criteria)
            {
                if (Optional)
                {
                    await next();                    
                } else
                {
                    context.Result = ApiError.Unauthorized(msg).ToObjectResult();
                }
                return true;
            }
            return false;
        }
    }
}
