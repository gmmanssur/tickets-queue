using TicketsQueue.Worker;
using TicketsQueue.Worker.Email;
using TicketsQueue.Worker.RabbitMq;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddHostedService<TicketWorker>();

var host = builder.Build();

await host.RunAsync();