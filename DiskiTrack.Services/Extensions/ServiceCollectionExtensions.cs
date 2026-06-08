using DiskiTrack.Services.Filters;

namespace DiskiTrack.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiPresentation(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }
}
