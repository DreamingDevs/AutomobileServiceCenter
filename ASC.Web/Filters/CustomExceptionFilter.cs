using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ASC.Web.Filters
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<CustomExceptionFilter> _logger;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IModelMetadataProvider modelMetadataProvider)
        {
            _logger = logger;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            var logId = Guid.NewGuid().ToString();
            _logger.LogError(new EventId(1000, logId), context.Exception, context.Exception.Message);

            var result = new ViewResult { ViewName = "CustomError" };
            result.ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
            result.ViewData.Add("ExceptionId", logId);
            context.Result = result;
        }
    }
}
