using System.Text.Json;
using NUnit.Framework;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class VsCodeWorkflowTests
{
    private const string PresentationProject = "src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj";
    private const string PresentationOutput = "${workspaceFolder}/src/Presentation/WF.Reliquary.Presentation/bin/Debug/net10.0/WF.Reliquary.Presentation.dll";
    private const string PresentationWorkingDirectory = "${workspaceFolder}/src/Presentation/WF.Reliquary.Presentation";

    [Test]
    public void TasksJson_DefinesRestoreBuildTestAndRunPresentationTasks()
    {
        using var document = ReadWorkspaceJson("tasks.json");
        var tasks = document.RootElement.GetProperty("tasks").EnumerateArray().ToArray();

        var restore = FindTask(tasks, "dotnet: restore");
        var build = FindTask(tasks, "dotnet: build");
        var test = FindTask(tasks, "dotnet: test");
        var run = FindTask(tasks, "dotnet: run presentation");

        Assert.Multiple(() =>
        {
            AssertDotnetTask(restore, "restore");
            AssertDotnetTask(build, "build");
            AssertDotnetTask(test, "test");
            AssertDotnetTask(run, "run", "--project", PresentationProject);
            AssertTaskGroup(build, "build");
            AssertTaskGroup(test, "test");
        });
    }

    [Test]
    public void LaunchJson_DefinesPresentationDebugConfiguration()
    {
        using var document = ReadWorkspaceJson("launch.json");
        var configurations = document.RootElement.GetProperty("configurations").EnumerateArray().ToArray();
        var configuration = configurations.Single(item => item.GetProperty("name").GetString() == ".NET: Debug Presentation");

        AssertPresentationLaunchConfiguration(configuration);
        Assert.That(configuration.TryGetProperty("noDebug", out var noDebug), Is.False);
    }

    [Test]
    public void LaunchJson_DefinesPresentationLaunchWithoutDebugConfiguration()
    {
        using var document = ReadWorkspaceJson("launch.json");
        var configurations = document.RootElement.GetProperty("configurations").EnumerateArray().ToArray();
        var configuration = configurations.Single(item => item.GetProperty("name").GetString() == ".NET: Launch Presentation");

        AssertPresentationLaunchConfiguration(configuration);
        Assert.That(configuration.GetProperty("noDebug").GetBoolean(), Is.True);
    }

    private static void AssertPresentationLaunchConfiguration(JsonElement configuration)
    {
        Assert.Multiple(() =>
        {
            Assert.That(configuration.GetProperty("type").GetString(), Is.EqualTo("coreclr"));
            Assert.That(configuration.GetProperty("request").GetString(), Is.EqualTo("launch"));
            Assert.That(configuration.GetProperty("preLaunchTask").GetString(), Is.EqualTo("dotnet: build"));
            Assert.That(configuration.GetProperty("program").GetString(), Is.EqualTo(PresentationOutput));
            Assert.That(configuration.GetProperty("cwd").GetString(), Is.EqualTo(PresentationWorkingDirectory));
        });
    }

    private static JsonDocument ReadWorkspaceJson(string fileName)
    {
        var path = Path.Combine(RepositoryPaths.Root, ".vscode", fileName);
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private static JsonElement FindTask(JsonElement[] tasks, string label)
    {
        return tasks.Single(task => task.GetProperty("label").GetString() == label);
    }

    private static void AssertDotnetTask(JsonElement task, params string[] args)
    {
        Assert.That(task.GetProperty("type").GetString(), Is.EqualTo("process"));
        Assert.That(task.GetProperty("command").GetString(), Is.EqualTo("dotnet"));
        Assert.That(ReadStringArray(task.GetProperty("args")), Is.EqualTo(args));
        Assert.That(task.GetProperty("options").GetProperty("cwd").GetString(), Is.EqualTo("${workspaceFolder}"));
    }

    private static void AssertTaskGroup(JsonElement task, string kind)
    {
        var group = task.GetProperty("group");

        Assert.That(group.GetProperty("kind").GetString(), Is.EqualTo(kind));
        Assert.That(group.GetProperty("isDefault").GetBoolean(), Is.True);
    }

    private static string[] ReadStringArray(JsonElement element)
    {
        return element.EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();
    }
}
