using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.RecruitingMessage;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class RecruitingMessageTemplateEditorViewModelTests
{
    private static RecruitingMessageTemplate CustomTemplate { get; } = new(
        "{participation} [{relic_name}]",
        [new RecruitingMessageOptionGroup("participation", [new RecruitingMessageOption("Hosting", "H"), new RecruitingMessageOption("LFG", "LF")], "LF")]);

    [Test]
    public void Parse_ReconcilesDraftGroupsWithNewPattern()
    {
        var store = NoOpStore();
        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, RecruitingMessageTemplate.Default);

        // Remove {refinement} from the pattern then parse
        vm.PatternText = "{participation} [{relic_name}]";
        vm.ParseCommand.Execute(null);

        Assert.That(vm.Groups, Has.Count.EqualTo(1));
        Assert.That(vm.Groups[0].Placeholder, Is.EqualTo("participation"));
    }

    [Test]
    public void Parse_InvalidPattern_ShowsErrorAndLeavesDraftUnchanged()
    {
        var store = NoOpStore();
        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, RecruitingMessageTemplate.Default);
        var originalGroupCount = vm.Groups.Count;

        vm.PatternText = "{unclosed";
        vm.ParseCommand.Execute(null);

        Assert.That(vm.ParseError, Is.Not.Empty);
        Assert.That(vm.Groups, Has.Count.EqualTo(originalGroupCount));
    }

    [Test]
    public void Save_IsBlocked_WhenAGroupHasNoOptions()
    {
        var store = NoOpStore();
        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, RecruitingMessageTemplate.Default);

        // Add a new placeholder that has no options
        vm.PatternText = "{newgroup} [{relic_name}]";
        vm.ParseCommand.Execute(null);

        // newgroup is empty → save should be disabled
        Assert.That(vm.SaveCommand.CanExecute(null), Is.False);
    }

    [Test]
    public async Task Save_PersistsCurrentTemplate()
    {
        UserSettings? saved = null;
        var store = new Mock<IUserSettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserSettings.Default);
        store.Setup(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()))
            .Callback<UserSettings, CancellationToken>((s, _) => saved = s)
            .Returns(Task.CompletedTask);

        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, RecruitingMessageTemplate.Default);
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.RecruitingMessageTemplate.Pattern, Is.EqualTo(RecruitingMessageTemplate.Default.Pattern));
    }

    [Test]
    public void RestoreDefaults_ResetsDraftToCodeDefaults()
    {
        var store = NoOpStore();
        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, CustomTemplate);

        vm.RestoreDefaultsCommand.Execute(null);

        Assert.That(vm.PatternText, Is.EqualTo(RecruitingMessageTemplate.Default.Pattern));
        Assert.That(vm.Groups, Has.Count.EqualTo(RecruitingMessageTemplate.Default.Groups.Count));
    }

    [Test]
    public async Task RestoreDefaults_DoesNotPersistUntilSaved()
    {
        var store = new Mock<IUserSettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserSettings.Default);
        store.Setup(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, CustomTemplate);
        vm.RestoreDefaultsCommand.Execute(null);

        // Restore alone should not call SaveAsync
        store.Verify(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()), Times.Never);

        // Save after restore does persist
        await vm.SaveCommand.ExecuteAsync(null);
        store.Verify(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void NotifySaved_ClearsDirtyState()
    {
        var store = NoOpStore();
        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, RecruitingMessageTemplate.Default);

        // Dirty the editor
        vm.PatternText = "{participation} [{relic_name}]";
        vm.ParseCommand.Execute(null);
        Assert.That(vm.HasUnsavedChanges, Is.True);

        // Simulate external save (e.g. via main Settings Save button)
        vm.NotifySaved(vm.CurrentDraftTemplate());

        Assert.That(vm.HasUnsavedChanges, Is.False);
    }

    [Test]
    public void NotifySaved_AllowsSubsequentDirtyDetection()
    {
        var store = NoOpStore();
        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, RecruitingMessageTemplate.Default);

        vm.PatternText = "{participation} [{relic_name}]";
        vm.ParseCommand.Execute(null);
        vm.NotifySaved(vm.CurrentDraftTemplate());

        // Further change after save should re-dirty
        vm.PatternText = "{participation} [{relic_name}] updated";
        Assert.That(vm.HasUnsavedChanges, Is.True);
    }

    [Test]
    public void Parse_EmptyGroup_ShowsWarning()
    {
        var store = NoOpStore();
        var vm = new RecruitingMessageTemplateEditorViewModel(store.Object, RecruitingMessageTemplate.Default);

        vm.PatternText = "{newgroup} [{relic_name}]";
        vm.ParseCommand.Execute(null);

        Assert.That(vm.ParseWarning, Does.Contain("newgroup").IgnoreCase);
    }

    private static Mock<IUserSettingsStore> NoOpStore()
    {
        var mock = new Mock<IUserSettingsStore>();
        mock.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserSettings.Default);
        mock.Setup(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }
}
