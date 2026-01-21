using Grpc.Core;
using Grpc.Core.Interceptors;

namespace SemanticAnalysis.Service.Interceptors;

public class ApiKeyInterceptor(IConfiguration configuration, ILogger<ApiKeyInterceptor> logger) : Interceptor
{
    private const string ApiKeyHeaderName = "x-api-key";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var apiKey = context.RequestHeaders.GetValue(ApiKeyHeaderName);
        var validKey = configuration["Auth:ApiKey"];

        if (string.IsNullOrEmpty(apiKey) || apiKey != validKey)
        {
            logger.LogWarning("Unauthorized access attempt detected from {Peer}", context.Peer);
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or missing API Key."));
        }

        return await continuation(request, context);
    }
}
