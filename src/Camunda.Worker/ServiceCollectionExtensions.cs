using System;
using Camunda.Worker.Client;
using Camunda.Worker.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Camunda.Worker
{
    public static class CamundaWorkerServiceCollectionExtensions
    {
        public static ICamundaWorkerBuilder AddCamundaWorker(this IServiceCollection services,
            Action<CamundaWorkerOptions> configureDelegate)
        {
            services.AddOptions<CamundaWorkerOptions>()
                .Configure(configureDelegate);
            services.AddHttpClient<IExternalTaskClient, ExternalTaskClient>()
                .ConfigureHttpClient((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptions<CamundaWorkerOptions>>().Value;
                    client.BaseAddress = options.BaseUri;
                });

            services.TryAddTransient<ITopicsProvider, StaticTopicsProvider>();
            services.TryAddTransient<ICamundaWorker, DefaultCamundaWorker>();
            services.TryAddTransient<IExternalTaskSelector, ExternalTaskSelector>();
            services.AddHostedService<WorkerHostedService>();

            services.TryAddTransient<IExternalTaskRouter, ExternalTaskRouter>();
            services.TryAddTransient<IHandlerFactoryProvider, TopicBasedFactoryProvider>();
            services.TryAddTransient<IExceptionHandler, DefaultExceptionHandler>();

            return new CamundaWorkerBuilder(services);
        }
    }
}
