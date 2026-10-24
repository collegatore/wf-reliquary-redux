using System.Collections.ObjectModel;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Manages the wishlist selector shown above the gear cards.
/// The <see cref="PromptForNameAsync"/> delegate must be provided by the view
/// layer; it opens a modal dialog and returns the confirmed name, or null on cancel.
/// </summary>
public sealed class WishlistViewModel : ObservableObject
{
    private readonly IWishlistStore store;
    private WishlistEntryViewModel? activeEntry;

    public WishlistViewModel(IWishlistStore store, Func<NameDialogRequest, Task<string?>> promptForNameAsync)
    {
        this.store = store;
        Wishlists = [];
        CreateCommand = new AsyncRelayCommand(async () => await CreateAsync(promptForNameAsync), () => true);
        RenameCommand = new AsyncRelayCommand(async () => await RenameAsync(promptForNameAsync), () => ActiveEntry is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => CanDelete);
        _ = ReloadAsync();
    }

    /// <summary>Raised after the active wishlist changes, so gear cards can reload.</summary>
    public Func<Task>? OnActiveWishlistChanged { get; set; }

    public ObservableCollection<WishlistEntryViewModel> Wishlists { get; }

    public WishlistEntryViewModel? ActiveEntry
    {
        get => activeEntry;
        private set
        {
            if (activeEntry == value) return;
            activeEntry = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedEntry));
            OnPropertyChanged(nameof(ActiveWishlistName));
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
    public WishlistEntryViewModel? SelectedEntry
    {
        get => ActiveEntry;
        set
        {
            if (value is null || value == ActiveEntry) return;
            SelectAsync(value).SafeFireAndForget();
        }
    }

    public string ActiveWishlistName => ActiveEntry?.Name ?? string.Empty;

    public bool CanDelete => Wishlists.Count > 1;

    public ICommand CreateCommand { get; }
    public ICommand RenameCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task SelectAsync(WishlistEntryViewModel entry)
    {
        if (ActiveEntry == entry) return;
        ActiveEntry = entry;
        await store.SetActiveAsync(entry.Id);
        if (OnActiveWishlistChanged is not null) await OnActiveWishlistChanged();
    }

    /// <summary>Reloads the wishlist list and active entry from the store. Called on
    /// construction and again whenever the active profile changes.</summary>
    public async Task ReloadAsync()
    {
        var all = await store.GetAllAsync();
        var active = await store.GetActiveAsync();

        Wishlists.Clear();
        foreach (var entry in all)
        {
            Wishlists.Add(new WishlistEntryViewModel(entry, this));
        }

        ActiveEntry = active is null
            ? null
            : Wishlists.FirstOrDefault(v => v.Id == active.Id);

        OnPropertyChanged(nameof(CanDelete));
        ((AsyncRelayCommand)DeleteCommand).NotifyCanExecuteChanged();
    }

    private async Task CreateAsync(Func<NameDialogRequest, Task<string?>> prompt)
    {
        var taken = Wishlists.Select(w => w.Name).ToList();
        var name = await prompt(new NameDialogRequest(null, taken));
        if (string.IsNullOrWhiteSpace(name)) return;

        // Authoritative guard: ignore a name the dialog should already have blocked.
        if (NameRules.Validate(name, taken) is not null) return;
        name = name.Trim();

        var entry = await store.CreateAsync(name);
        var vm = new WishlistEntryViewModel(entry, this);
        Wishlists.Add(vm);

        await store.SetActiveAsync(entry.Id);
        ActiveEntry = vm;

        OnPropertyChanged(nameof(CanDelete));
        ((AsyncRelayCommand)DeleteCommand).NotifyCanExecuteChanged();

        if (OnActiveWishlistChanged is not null) await OnActiveWishlistChanged();
    }

    private async Task RenameAsync(Func<NameDialogRequest, Task<string?>> prompt)
    {
        if (ActiveEntry is null) return;

        var taken = Wishlists.Where(w => w != ActiveEntry).Select(w => w.Name).ToList();
        var newName = await prompt(new NameDialogRequest(ActiveEntry.Name, taken));
        if (string.IsNullOrWhiteSpace(newName)) return;

        if (NameRules.Validate(newName, taken) is not null) return;
        newName = newName.Trim();

        await store.RenameAsync(ActiveEntry.Id, newName);
        ActiveEntry.Name = newName;
        OnPropertyChanged(nameof(ActiveWishlistName));
    }

    private async Task DeleteAsync()
    {
        if (ActiveEntry is null || !CanDelete) return;

        var toDelete = ActiveEntry;
        var next = Wishlists.FirstOrDefault(v => v != toDelete);
        if (next is null) return;

        await store.SetActiveAsync(next.Id);
        ActiveEntry = next;

        await store.DeleteAsync(toDelete.Id);
        Wishlists.Remove(toDelete);

        OnPropertyChanged(nameof(CanDelete));
        ((AsyncRelayCommand)DeleteCommand).NotifyCanExecuteChanged();

        if (OnActiveWishlistChanged is not null) await OnActiveWishlistChanged();
    }
}

public sealed class WishlistEntryViewModel : ObservableObject
{
    private string name;

    public WishlistEntryViewModel(WishlistEntry entry, WishlistViewModel parent)
    {
        Id = entry.Id;
        name = entry.Name;
        Parent = parent;
    }

    public long Id { get; }

    public WishlistViewModel Parent { get; }

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }
}
