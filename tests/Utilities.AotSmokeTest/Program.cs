using Continuum.ArrayPools;
using Continuum.AsyncExtensions;

// Exercises Continuum.ArrayPools and Continuum.AsyncExtensions under Native AOT. Publish with:
//   dotnet publish tests/Utilities.AotSmokeTest -c Release
// then run the produced executable. A non-zero exit code means a check failed.

var failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition) failures++;
}

var pool = ArrayPoolUtilities.CreatePreWarmed<byte>(1024, 2);
var buffer = pool.Rent(512);
Check("Pre-warmed array pool rents", buffer.Length >= 512);
pool.Return(buffer);

var (number, text) = await (Task.FromResult(1), Task.FromResult("a"));
Check("Task tuple awaits", number == 1 && text == "a");

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;
