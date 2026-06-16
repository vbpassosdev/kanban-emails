using KanbanEmails.Application;
using KanbanEmails.Infrastructure;
using KanbanEmails.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<EmailReaderWorker>();

var host = builder.Build();
host.Run();
