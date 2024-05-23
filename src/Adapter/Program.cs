using Adapter.Config;
using Adapter.Extensions;

var builder = WebApplication.CreateBuilder(args);
var transactionsAdapterConfiguration = new TransactionsAdapterConfiguration();

builder.Configuration.Bind("TransactionsAdapter", transactionsAdapterConfiguration);
transactionsAdapterConfiguration.Validate();

builder.RegisterServices(transactionsAdapterConfiguration);

var app = builder.Build().Configure();

app.Run();
