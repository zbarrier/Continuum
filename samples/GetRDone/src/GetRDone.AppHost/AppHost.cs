var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var clustering = storage.AddTables("clustering");

// Not proxied, so Docker publishes the container directly as host 2113 -> container 2113 and the
// KurrentDB desktop app can always connect to esdb://localhost:2113?tls=false.
var kurrentdb = builder.AddKurrentDB("kurrentdb", port: 2113)
    .WithEndpoint("http", endpoint => endpoint.IsProxied = false);

// Local SMTP capture for notification and account emails.
var mailpit = builder.AddMailPit("mailpit");

// ASP.NET Core Identity users for GetRDone.Identity. The data volume keeps users across AppHost restarts.
var identityDb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("identitydb");

var orleans = builder.AddOrleans("getrdone")
    .WithClustering(clustering);

var identity = builder.AddProject<Projects.GetRDone_Identity>("identity")
    .WithReference(identityDb)
    .WithReference(mailpit)
    .WaitFor(identityDb);

var silo = builder.AddProject<Projects.GetRDone_Silo>("silo")
    .WithReference(orleans)
    .WithReference(kurrentdb)
    .WithReference(mailpit)
    .WaitFor(kurrentdb)
    .WaitFor(clustering);

var web = builder.AddProject<Projects.GetRDone_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(orleans.AsClient())
    .WithEnvironment("IDENTITY_URL", identity.GetEndpoint("https"))
    .WaitFor(silo)
    .WaitFor(identity);

identity.WithEnvironment("WEB_BASE_URL", web.GetEndpoint("https"));

builder.Build().Run();
