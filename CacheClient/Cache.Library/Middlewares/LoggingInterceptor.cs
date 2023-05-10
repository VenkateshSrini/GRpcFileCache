using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache.Library.Middlewares
{
    public class LoggingInterceptor:Interceptor
    {
        private readonly ILogger<LoggingInterceptor> _logger;
        private IHttpContextAccessor _httpContextAccessor;
        public LoggingInterceptor(ILogger<LoggingInterceptor> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context, 
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)

        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            _logger.LogInformation($"Starting call. Type: {context.Method.Type}. " +
            $"Method: {context.Method.Name}.");
            return continuation(request, context);
        }
    }
}
