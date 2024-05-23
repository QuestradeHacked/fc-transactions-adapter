using Adapter.Config;
using Application.Config;
using Domain.Models.Messages;
using Domain.Models.Messages.Publisher;
using Domain.Services;
using FluentValidation;
using Infra.Config.PubSub;
using Infra.Models.PubSub;
using Infra.Services.DataDog;
using Infra.Services.Publisher;
using Infra.Services.Subscriber;
using Infra.Services.Xceed;
using Infra.Validators;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Questrade.Library.HealthCheck.AspNetCore.Extensions;
using Questrade.Library.PubSubClientHelper.Extensions;
using Questrade.Library.PubSubClientHelper.Primitives;
using Serilog;
using StatsdClient;
using Refit;

namespace Adapter.Extensions;

public static class ServiceCollectionExtensions
{
    internal static WebApplicationBuilder RegisterServices(
        this WebApplicationBuilder builder, TransactionsAdapterConfiguration transactionsAdapterConfiguration
    )
    {
        builder.AddQuestradeHealthCheck();
        builder.Host.UseSerilog((_, logConfiguration) =>
            logConfiguration.ReadFrom.Configuration(builder.Configuration));

        builder.Services.AddControllers();
        builder.Services.AddMemoryCache();
        builder.Services.AddAppServices();
        builder.Services.AddDataDogMetrics(builder.Configuration);
        builder.Services.AddSubscribers(transactionsAdapterConfiguration);
        builder.Services.AddPublisher(transactionsAdapterConfiguration);
        builder.Services.AddXceedContext(transactionsAdapterConfiguration.XceedConfiguration!);

        return builder;
    }

    private static IServiceCollection AddXceedContext(this IServiceCollection services,
        XceedConfiguration configuration)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(configuration.Retry,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var timeoutPolicy = Policy
            .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(configuration.Timeout));

        services.TryAddSingleton(configuration);

        services.AddSingleton<IXceedService>(provider =>
            new XceedService(
                configuration,
                provider.GetService<IXceedRest>()!,
                provider.GetService<ILogger<XceedService>>()!,
                provider.GetService<IMetricService>()!
                ));

        services.AddRefitClient<IXceedRest>()
            .ConfigureHttpClient(x => x.BaseAddress = new Uri(configuration.BaseUrl!))
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(timeoutPolicy);

        return services;
    }

    private static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Services
        services.AddTransient<IMetricService, DataDogMetricService>();
        services.AddTransient<IValidator<TransactionsAdapterMessage>, TransactionsAdapterMessageValidator>();
        services.AddTransient<IValidator<XceedConfiguration>, XceedConfigurationValidator>();

        return services;
    }

    private static void AddDataDogMetrics(this IServiceCollection services, IConfigurationManager configuration)
    {
        var dataDogConfiguration = configuration.GetSection("DataDog:StatsD").Get<DataDogConfiguration>();

        services.AddSingleton<IDogStatsd>(_ =>
        {
            var statsdConfig = new StatsdConfig
            {
                Prefix = dataDogConfiguration!.Prefix,
                StatsdServerName = dataDogConfiguration.HostName
            };

            var dogStatsdService = new DogStatsdService();
            dogStatsdService.Configure(statsdConfig);

            return dogStatsdService;
        });
    }

    private static IServiceCollection AddSubscribers(this IServiceCollection services,
        TransactionsAdapterConfiguration configuration)
    {
        if (!configuration.TransactionsAdapterSubscriberConfiguration.Enable)
        {
            return services;
        }

        services.AddSingleton<IJsonSerializerOptionsProvider<PubSubMessage<TransactionsAdapterMessage>>, JsonSerializerOptionsProvider<PubSubMessage<TransactionsAdapterMessage>>>();
        services.RegisterSubscriber<
            TransactionsAdapterSubscriberConfiguration,
            PubSubMessage<TransactionsAdapterMessage>,
            TransactionsAdapterSubscriberMessageHandler,
            TransactionsAdapterPubSubSubscriberBackgroundService
        >(configuration.TransactionsAdapterSubscriberConfiguration);

        return services;
    }

    private static IServiceCollection AddPublisher(this IServiceCollection services,
        TransactionsAdapterConfiguration configuration)
    {
        services.AddSingleton<IJsonSerializerOptionsProvider<TransactionAdapterResultMessage>, JsonSerializerOptionsProvider<TransactionAdapterResultMessage>>();
        services.AddSingleton(configuration.TransactionAdapterPublisherConfiguration);
        services.AddSingleton<ITransactionAdapterPublisher, TransactionAdapterPublisher>();
        return services;
    }
}
