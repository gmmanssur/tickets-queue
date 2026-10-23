using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketsQueue.Application.Commons.Interfaces;
using TicketsQueue.Application.Messaging;
using TicketsQueue.Infrastructure.Messaging;
using TicketsQueue.Infrastructure.Persistence.Context;
using TicketsQueue.Infrastructure.Persistence.Repositories;

namespace TicketsQueue.Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TicketsQueueDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ITicketQueueRepository, TicketQueueRepository>();

            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

            services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

            return services;
        }
    }
}
