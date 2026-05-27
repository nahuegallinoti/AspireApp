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

// Auth / SSO secrets — provided via user-secrets in dev, env vars in prod.
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceThisDevKeyWithAtLeast32CharactersLong!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AspireApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AspireApp.Clients";

var googleEnabled = builder.Configuration["Sso:Google:Enabled"] ?? "false";
var googleClientId = builder.Configuration["Sso:Google:ClientId"] ?? string.Empty;
var googleClientSecret = builder.Configuration["Sso:Google:ClientSecret"] ?? string.Empty;

var api = builder.AddProject<Projects.AspireApp_Api>("api")
    .WithReference(redis).WaitFor(redis)
    .WithReference(messaging).WaitFor(messaging)
    .WithEnvironment("EventBus__Provider", provider)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Sso__Google__Enabled", googleEnabled)
    .WithEnvironment("Sso__Google__ClientId", googleClientId)
    .WithEnvironment("Sso__Google__ClientSecret", googleClientSecret);

builder.AddProject<Projects.AspireApp_Client>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(redis).WaitFor(redis)
    .WithReference(api).WaitFor(api)
    .WithEnvironment("Sso__Google__Enabled", googleEnabled)
    .WithEnvironment("Sso__Google__ClientId", googleClientId)
    .WithEnvironment("Sso__Google__ClientSecret", googleClientSecret);

builder.Build().Run();
