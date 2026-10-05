using Continuum.Orleans.KurrentDB;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage;

// Exercises the Continuum.Persistence providers under Native AOT. Publish with:
//   dotnet publish tests/Persistence.AotSmokeTest -c Release
// then run the produced executable. A non-zero exit code means a check failed.
// Orleans DeepCopier/serializer setup is not Native AOT compatible upstream, so no silo is started here; the
// publish itself verifies the provider code is free of trim/AOT warnings.

var failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition) failures++;
}

// Credentials resolve without reflection.
var credentials = new KurrentDBCredentialsOptions { UseDefault = false, Username = "admin", Password = "changeit" };
Check("KurrentDB credentials valid", credentials.IsValid);
Check("KurrentDB credentials convert", credentials.ToUserCredentials() is not null);
Check("KurrentDB default credentials", new KurrentDBCredentialsOptions { UseDefault = true }.ToUserCredentials() is null);

// Provider registration. Resolving the options needs Orleans silo services (serializer, type mapper),
// so only the registrations are checked here.
var services = new ServiceCollection();
services.AddKurrentDBGrainStorage("kurrent", o => o.ConnectionName = "kurrent");
Check("KurrentDB grain storage registered", services.Any(s => s.ServiceType == typeof(IGrainStorage) && Equals(s.ServiceKey, "kurrent")));
Check("KurrentDB grain storage options registered", services.Any(s => s.ServiceType == typeof(IConfigureOptions<KurrentDBGrainStorageOptions>)));

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;
