using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using CoreDNX.Services;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Web.Middleware
{
    public class RequestDispatchContext
    {
        public HttpContext HttpContext { get; set; }
    }

    public interface IRequestDispatcher
    {
        Task Dispatch(RequestDispatchContext requestDispatchContext);
    }

    public class FeatureActionDispatcher : IRequestDispatcher
    {
        private readonly IFeatureActionService _featureService;

        public FeatureActionDispatcher(IFeatureActionService featureService)
        {
            _featureService = featureService;
        }

        public Task Dispatch(RequestDispatchContext requestDispatchContext)
        {
            var req = requestDispatchContext.HttpContext.Request;
            var res = requestDispatchContext.HttpContext.Response;
            if (req.Method != "POST")
            {
                res.StatusCode = 405;
                return Task.FromResult(false);
            }

            var featuresToEnable = req.Form["enableFeatures"] as IEnumerable<string>;
            var featuresToDisable = req.Form["disableFeatures"] as IEnumerable<string>;

            featuresToEnable.ToList().ForEach(_featureService.EnableFeature);
            featuresToDisable.ToList().ForEach(_featureService.DisableFeature);

            res.StatusCode = (int)HttpStatusCode.NoContent;
            return Task.FromResult(true);
        }
    }

    public class FeatureDescriptorDispatcher : IRequestDispatcher
    {
        private readonly IFeatureInfoService _featureInfoService;

        public FeatureDescriptorDispatcher(IFeatureInfoService featureInfoService)
        {
            _featureInfoService = featureInfoService;
        }

        public async Task Dispatch(RequestDispatchContext requestDispatchContext)
        {
            var req = requestDispatchContext.HttpContext.Request;
            var res = requestDispatchContext.HttpContext.Response;
            if (req.Method != "POST")
            {
                res.StatusCode = 405;
                return;
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new JsonConverter[] { new StringEnumConverter { CamelCaseText = true } }
            };
            var serialized = JsonConvert.SerializeObject(_featureInfoService.GetFeaturesItems(), settings);

            res.ContentType = "application/json";
            await res.WriteAsync(serialized);
        }
    }

    public class RazorPageDispatcher : IRequestDispatcher
    {
        private readonly IApplicationEnvironment _appEnvironment;

        public RazorPageDispatcher(IApplicationEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        public async Task Dispatch(RequestDispatchContext requestDispatchContext)
        {
            var req = requestDispatchContext.HttpContext.Request;
            var res = requestDispatchContext.HttpContext.Response;
            if (req.Method != "GET")
            {
                res.StatusCode = 405;
                return;
            }

            var indexPage = Path.Combine(_appEnvironment.ApplicationBasePath, "ISwitch", "Index.html");
            using (var reader = new StreamReader(indexPage))
            {
                var output = await reader.ReadToEndAsync();
                await res.WriteAsync(output);
            }
        }
    }

    public interface IRequestDispatcherService
    {
        Task Invoke(HttpContext context);
    }

    public class RequestDispatcherService : IRequestDispatcherService
    {
        private readonly IIndex<string, IRequestDispatcher> _requestDispatchIndex;

        public RequestDispatcherService(IIndex<string, IRequestDispatcher> requestDispatchIndex)
        {
            _requestDispatchIndex = requestDispatchIndex;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestDispatchContext = new RequestDispatchContext {HttpContext = context};
            var key = requestDispatchContext.HttpContext.Request.Path;

            var dispatcher = _requestDispatchIndex[key];
            if (dispatcher == null)
            {
                return;
            }

            await dispatcher.Dispatch(requestDispatchContext);
        } 
    }

    public class FeatureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRequestDispatcherService _requestDispatcherService;

        public FeatureMiddleware(RequestDelegate next, IRequestDispatcherService requestDispatcherService)
        {
            _requestDispatcherService = requestDispatcherService;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _requestDispatcherService.Invoke(context);
            await _next.Invoke(context);
        }
    }

    public static class FeatureMiddlewareExtensions
    {
        public static IApplicationBuilder UseFeatureMiddleware(this IApplicationBuilder builder, string relativeRoute = null)
        {
            if (relativeRoute != null && !Uri.IsWellFormedUriString(relativeRoute, UriKind.Relative))
            {
                throw new FormatException(nameof(relativeRoute) + " is not a valid relative URL.");
            }

            return builder.Map(relativeRoute ?? "/features", applicationBuilder => applicationBuilder.UseMiddleware<FeatureMiddleware>());
        }
    }
}
