using Updater.Settings;

Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((context, services) =>
    {
        services.Configure<UpdaterSettings>(context.Configuration.GetSection("Updater"));
        services.Configure<TargetSettings>(context.Configuration.GetSection("Target"));
        services.Configure<BackupSettings>(context.Configuration.GetSection("Backup"));
        services.Configure<WindowsServiceSettings>(context.Configuration.GetSection("WindowsService"));

        services.AddHttpClient<IUpdaterClient, UpdaterClient>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<UpdaterSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        })
        .AddPolicyHandler(ResilienceExtensions.GetRetryPolicy())
        .AddPolicyHandler(ResilienceExtensions.GetCircuitBreakerPolicy());

        services.AddScoped<IFileHandler, FileHandler>();
        services.AddScoped<IWindowsServiceHandler, WindowsServiceHandler>();
        services.AddHostedService<App>();

    })
    .Build()
    .Run();