﻿using Encamina.Enmarcha.SemanticKernel.Abstractions;
using Encamina.Enmarcha.SemanticKernel.Plugins.QuestionAnswering;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

using Sample_SemanticKernelQuestionAnswering.QuestionAnsweringPlugin;

public static class Example_QuestionAnsweringFromContext
{
    public static async Task RunAsync()
    {
        Console.WriteLine("# Executing Example_QuestionAnsweringFromContext");

        // Create and configure builder
        var hostBuilder = new HostBuilder()
            .ConfigureAppConfiguration((configuration) =>
            {
                configuration.AddJsonFile(path: @"appsettings.json", optional: false, reloadOnChange: true);
            });

        // Configure service
        hostBuilder.ConfigureServices((hostContext, services) =>
        {
            // Add Semantic Kernel options
            services.AddOptions<SemanticKernelOptions>().Bind(hostContext.Configuration.GetSection(nameof(SemanticKernelOptions))).ValidateDataAnnotations().ValidateOnStart();

            services.AddScoped(sp =>
            {
                // Get semantic kernel options
                var options = hostContext.Configuration.GetRequiredSection(nameof(SemanticKernelOptions)).Get<SemanticKernelOptions>()
                ?? throw new InvalidOperationException(@$"Missing configuration for {nameof(SemanticKernelOptions)}");

                // Initialize semantic kernel
                var kernel = new KernelBuilder()
                    .WithAzureChatCompletionService(options.ChatModelDeploymentName, options.Endpoint.ToString(), options.Key)
                    .Build();

                // Import Question Answering plugin
                kernel.ImportQuestionAnsweringPlugin(sp, ILengthFunctions.LengthByTokenCount);

                return kernel;
            });
        });

        var host = hostBuilder.Build();

        // Initialize Q&A
        var testQuestionAnswering = new TestQuestionAnswering(host.Services.GetService<IKernel>());

        var result = await testQuestionAnswering.TestQuestionAnsweringFromContextAsync();
    }
}
