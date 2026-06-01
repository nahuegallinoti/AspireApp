var builder = DistributedApplication.CreateBuilder(args);

var provider = builder.Configuration["EventBus:Provider"]?.Trim() ?? "RabbitMq";

var redis = builder.AddRedis("redis")
    .WithRedisInsight()
    .WithDataVolume(isReadOnly: false);

// RabbitMQ: usuario/clave fijos (guest/guest) para dev, así no se desincronizan con el data volume.
var rabbitUser = builder.AddParameter("messaging-username", "guest");
var rabbitPassword = builder.AddParameter("messaging-password", "guest", secret: true);

IResourceBuilder<IResourceWithConnectionString> messaging = provider.Equals("Kafka", StringComparison.OrdinalIgnoreCase)
    ? builder.AddKafka("messaging").WithKafkaUI()
    : builder.AddRabbitMQ("messaging", rabbitUser, rabbitPassword, port: 5672)
        .WithManagementPlugin()
        .WithDataVolume();

// JWT: env vars JWT_KEY, JWT_ISSUER, JWT_AUDIENCE (o appsettings / user-secrets)
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? "ReplaceThisDevKeyWithAtLeast32CharactersLong!!";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "AspireApp";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "AspireApp.Clients";

// Google SSO: GOOGLE_CLIENT_ID, GOOGLE_CLIENT_SECRET, SSO_GOOGLE_ENABLED (ver SsoConfiguration)
var google = SsoConfiguration.ReadGoogle(builder.Configuration);

var api = builder.AddProject<Projects.AspireApp_Api>("api")
    .WithReference(redis).WaitFor(redis)
    .WithReference(messaging).WaitFor(messaging)
    .WithEnvironment("EventBus__Provider", provider)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Sso__Google__Enabled", google.Enabled.ToString().ToLowerInvariant())
    .WithEnvironment("Sso__Google__ClientId", google.ClientId)
    .WithEnvironment("Sso__Google__ClientSecret", google.ClientSecret);

builder.AddProject<Projects.AspireApp_Client>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(redis).WaitFor(redis)
    .WithReference(api).WaitFor(api)
    .WithEnvironment("Sso__Google__Enabled", google.Enabled.ToString().ToLowerInvariant())
    .WithEnvironment("Sso__Google__ClientId", google.ClientId)
    .WithEnvironment("Sso__Google__ClientSecret", google.ClientSecret);

builder.Build().Run();
