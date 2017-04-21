var configuration = Argument("configuration", "Debug");

Task("Clean")
.Does(() =>
{
    CleanDirectory("./artifacts/");
});

Task("Restore")
.Does(() => 
{
    DotNetCoreRestore();
});

Task("Build")
.Does(() =>
{
    var projects = GetFiles("./src/**/*.csproj");
    Console.WriteLine("Building {0} projects", projects.Count());

    foreach (var project in projects)
    {
        DotNetCoreBuild(project.FullPath, new DotNetCoreBuildSettings
        {
            Configuration = configuration
        });
    }
});

Task("Pack")
.Does(() =>
{
    new List<string>
    {
        "Stormpath.Owin.Abstractions",
        "Stormpath.Owin.Middleware",
        "Stormpath.Owin.Views.Precompiled"
    }.ForEach(name =>
    {
        DotNetCorePack("./src/" + name + ".csproj", new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = "./artifacts/"
        });
    });
});

Task("Test")
.IsDependentOn("Restore")
.IsDependentOn("Build")
.Does(() =>
{
    new List<string>
    {
        "Stormpath.Owin.UnitTest"
    }.ForEach(name =>
    {
        DotNetCoreTest("./test/" + name + ".csproj");
    });
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");


var target = Argument("target", "Default");
RunTarget(target);