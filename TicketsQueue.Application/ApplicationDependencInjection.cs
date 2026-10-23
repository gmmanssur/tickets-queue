using Microsoft.Extensions.DependencyInjection;
using TicketsQueue.Application.Commons.Interfaces;
using TicketsQueue.Application.UseCases;

namespace TicketsQueue.Application
{
    public static class ApplicationDependencInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ITicketsQueueService, TicketsQueueService>();

            return services;
        }
    }
}
