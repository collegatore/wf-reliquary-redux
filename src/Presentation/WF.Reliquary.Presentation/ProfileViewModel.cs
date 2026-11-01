using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Profiles;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Manages the profile selector shown in the Settings tab. Mirrors
/// <see cref="WishlistViewModel"/> but operates one level up: switching the active
/// profile re-scopes which wishlists are visible. The <c>promptForNameAsync</c>
/// delegate is provided by the view layer; it opens a modal dialog and returns the
/// confirmed name, or null on cancel.
/// </summary>
public sealed class ProfileViewModel : ObservableObject
{
    private readonly IProfileStore store;
    private ProfileEntryViewModel? activeEntry;

    public ProfileViewModel(IProfileStore store, Func<NameDialogRequest, Task<string?>> promptForNameAsync)
    {
        this.store = store;
        Profiles = [];
        CreateCommand = new AsyncRelayCommand(async () => await CreateAsync(promptForNameAsync), () => true);
        RenameCommand = new AsyncRelayCommand(async () => await RenameAsync(promptForNameAsync), () => ActiveEntry is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => CanDelete);
        _ = ReloadAsync();
    }

    /// <summary>Raised after the active profile changes, so wishlist-scoped views reload.</summary>
    public Func<Task>? OnActiveProfileChanged { get; set; }

    /// <summary>Invoked once when a fresh profile is created (after it becomes active),
    /// to seed that profile's "Default" wishlist. Wired by the view layer to the
    /// wishlist store. Not called on plain profile switches.</summary>
    public Func<Task>? SeedDefaultWishlist { get; set; }

    public ObservableCollection<ProfileEntryViewModel> Profiles { get; }

    public ProfileEntryViewModel? ActiveEntry
    {
        get => activeEntry;
        private set
        {
            if (activeEntry == value) return;
            activeEntry = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedEntry));
            OnPropertyChanged(nameof(ActiveProfileName));
            OnPropertyChanged(nameof(CanDelete));
            ((AsyncRelayCommand)RenameCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)DeleteCommand).NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// The selector's two-way <c>SelectedItem</c>: reads as <see cref="ActiveEntry"/>, and
    /// a user's pick activates and persists that entry. Ignores <c>null</c>, which the
    /// selector pushes back while <see cref="ReloadAsync"/> clears the list.
    /// </summary>
    public ProfileEntryViewModel? SelectedEntry
    {
        get => ActiveEntry;
        set
        {
            if (value is null || value == ActiveEntry) return;
            SelectAsync(value).SafeFireAndForget();
        }
    }

    public string ActiveProfileName => ActiveEntry?.Name ?? string.Empty;

    public bool CanDelete => Profiles.Count > 1;

    public ICommand CreateCommand { get; }
    public ICommand RenameCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task SelectAsync(ProfileEntryViewModel entry)
    {
        if (ActiveEntry == entry) return;
        ActiveEntry = entry;
        await store.SetActiveAsync(entry.Id);
        if (OnActiveProfileChanged is not null) await OnActiveProfileChanged();
    }

    public async Task ReloadAsync()
    {
        var all = await store.GetAllAsync();
        var active = await store.GetActiveAsync();

        Profiles.Clear();
        foreach (var entry in all)
        {
            Profiles.Add(new ProfileEntryViewModel(entry, this));
        }

        ActiveEntry = active is null
            ? null
            : Profiles.FirstOrDefault(v => v.Id == active.Id);

        OnPropertyChanged(nameof(CanDelete));
        ((AsyncRelayCommand)DeleteCommand).NotifyCanExecuteChanged();
    }

    private async Task CreateAsync(Func<NameDialogRequest, Task<string?>> prompt)
    {
        var taken = Profiles.Select(p => p.Name).ToList();
        var name = await prompt(new NameDialogRequest(null, taken));
        if (string.IsNullOrWhiteSpace(name)) return;

        // Authoritative guard: ignore a name the dialog should already have blocked.
        if (NameRules.Validate(name, taken) is not null) return;
        name = name.Trim();

        var entry = await store.CreateAsync(name);
        var vm = new ProfileEntryViewModel(entry, this);
        Profiles.Add(vm);

        await store.SetActiveAsync(entry.Id);
        ActiveEntry = vm;

        // A fresh profile starts with its own "Default" wishlist (one-time seed).
        if (SeedDefaultWishlist is not null) await SeedDefaultWishlist();

        OnPropertyChanged(nameof(CanDelete));
        ((AsyncRelayCommand)DeleteCommand).NotifyCanExecuteChanged();

        if (OnActiveProfileChanged is not null) await OnActiveProfileChanged();
    }

    private async Task RenameAsync(Func<NameDialogRequest, Task<string?>> prompt)
    {
        if (ActiveEntry is null) return;

        var taken = Profiles.Where(p => p != ActiveEntry).Select(p => p.Name).ToList();
        var newName = await prompt(new NameDialogRequest(ActiveEntry.Name, taken));
        if (string.IsNullOrWhiteSpace(newName)) return;

        if (NameRules.Validate(newName, taken) is not null) return;
        newName = newName.Trim();

        await store.RenameAsync(ActiveEntry.Id, newName);
        ActiveEntry.Name = newName;
        OnPropertyChanged(nameof(ActiveProfileName));
    }

    private async Task DeleteAsync()
    {
        if (ActiveEntry is null || !CanDelete) return;

        var toDelete = ActiveEntry;
        var next = Profiles.FirstOrDefault(v => v != toDelete);
        if (next is null) return;

        await store.SetActiveAsync(next.Id);
        ActiveEntry = next;

        await store.DeleteAsync(toDelete.Id);
        Profiles.Remove(toDelete);

        OnPropertyChanged(nameof(CanDelete));
        ((AsyncRelayCommand)DeleteCommand).NotifyCanExecuteChanged();

        if (OnActiveProfileChanged is not null) await OnActiveProfileChanged();
    }
}

public sealed class ProfileEntryViewModel : ObservableObject
{
    private string name;

    public ProfileEntryViewModel(ProfileEntry entry, ProfileViewModel parent)
    {
        Id = entry.Id;
        name = entry.Name;
        Parent = parent;
    }

    public long Id { get; }

    public ProfileViewModel Parent { get; }

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }
}
