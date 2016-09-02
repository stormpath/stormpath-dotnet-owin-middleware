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
    DotNetCoreBuild("./src/**/project.json", new DotNetCoreBuildSettings
    {
        Configuration = configuration
    });
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
        DotNetCorePack("./src/" + name + "/project.json", new DotNetCorePackSettings
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
        "Stormpath.SDK.UnitTest"
    }.ForEach(name =>
    {
        DotNetCoreTest("./test/" + name + "/project.json");
    });
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");


var target = Argument("target", "Default");
RunTarget(target);