var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var clustering = storage.AddTables("clustering");

var kurrentdb = builder.AddKurrentDB("kurrentdb");

// Local SMTP capture for notification emails (wired up once notifications exist).
var mailpit = builder.AddMailPit("mailpit");

var orleans = builder.AddOrleans("getrdone")
    .WithClustering(clustering);

var identity = builder.AddProject<Projects.GetRDone_Identity>("identity");

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
