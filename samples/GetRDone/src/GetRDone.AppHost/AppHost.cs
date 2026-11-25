var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var clustering = storage.AddTables("clustering");

// The files of supporting documents. The web app uploads them; orders record only their path.
var orderDocuments = storage.AddBlobContainer("order-documents");

// Not proxied, so Docker publishes the container directly as host 2113 -> container 2113 and the
// KurrentDB desktop app can always connect to esdb://localhost:2113?tls=false.
var kurrentdb = builder.AddKurrentDB("kurrentdb", port: 2113)
    .WithEndpoint("http", endpoint => endpoint.IsProxied = false);

// Local SMTP capture for notification and account emails.
var mailpit = builder.AddMailPit("mailpit");

// ASP.NET Core Identity users for GetRDone.Identity. The data volume keeps users across AppHost restarts.
// Not proxied, so Docker publishes host 14330 -> container 1433 and SSMS can always connect to
// localhost,14330 (14330 avoids clashing with a locally installed SQL Server on 1433).
var identityDb = builder.AddSqlServer("sql", port: 14330)
    .WithEndpoint("tcp", endpoint => endpoint.IsProxied = false)
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

// Hosts the automations' KurrentDB catch-up subscriptions and calls the silo as a cluster client. Exactly one instance:
// a catch-up subscription doesn't share its work with other instances, so a second one would handle every event again.
builder.AddProject<Projects.GetRDone_Subscriptions_Worker>("subscriptions-worker")
    .WithReplicas(1)
    .WithReference(orleans.AsClient())
    .WithReference(kurrentdb)
    .WaitFor(silo);

var web = builder.AddProject<Projects.GetRDone_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(orleans.AsClient())
    .WithReference(orderDocuments)
    .WithEnvironment("IDENTITY_URL", identity.GetEndpoint("https"))
    .WaitFor(silo)
    .WaitFor(identity)
    .WaitFor(orderDocuments);

identity.WithEnvironment("WEB_BASE_URL", web.GetEndpoint("https"));
silo.WithEnvironment("WEB_BASE_URL", web.GetEndpoint("https"));

builder.Build().Run();
