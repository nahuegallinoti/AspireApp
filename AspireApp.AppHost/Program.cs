var builder = DistributedApplication.CreateBuilder(args);

var provider = builder.Configuration["EventBus:Provider"]?.Trim() ?? "RabbitMq";

var redis = builder.AddRedis("redis")
    .WithRedisInsight()
    .WithDataVolume(isReadOnly: false);

IResourceBuilder<IResourceWithConnectionString> messaging = provider.Equals("Kafka", StringComparison.OrdinalIgnoreCase)
    ? builder.AddKafka("messaging").WithKafkaUI()
    : builder.AddRabbitMQ("messaging", port: 5672)
        .WithManagementPlugin()
        .WithDataVolume();

var api = builder.AddProject<Projects.AspireApp_Api>("api")
    .WithReference(redis).WaitFor(redis)
    .WithReference(messaging).WaitFor(messaging)
    .WithEnvironment("EventBus__Provider", provider);

builder.AddProject<Projects.AspireApp_Client>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(api).WaitFor(api);

builder.Build().Run();
