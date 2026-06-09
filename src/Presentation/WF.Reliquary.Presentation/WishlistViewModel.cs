using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Manages the wishlist selector shown above the gear cards.
/// The <see cref="PromptForNameAsync"/> delegate must be provided by the view
/// layer; it opens a modal dialog and returns the confirmed name, or null on cancel.
/// </summary>
public sealed class WishlistViewModel : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raised after the active wishlist changes, so gear cards can reload.</summary>
    public Func<Task>? OnActiveWishlistChanged { get; set; }

    /// <summary>Raised after the user selects a list, so the view can close the dropdown.</summary>
    public Action? FlyoutCloseRequested { get; set; }

    public ObservableCollection<WishlistEntryViewModel> Wishlists { get; }

    public WishlistEntryViewModel? ActiveEntry
    {
        get => activeEntry;
        private set
        {
            if (activeEntry == value) return;
            if (activeEntry is not null) activeEntry.IsActive = false;
            activeEntry = value;
            if (activeEntry is not null) activeEntry.IsActive = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ActiveWishlistName));
            OnPropertyChanged(nameof(CanDelete));
            ((AsyncRelayCommand)RenameCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)DeleteCommand).RaiseCanExecuteChanged();
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
        FlyoutCloseRequested?.Invoke();
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
        ((AsyncRelayCommand)DeleteCommand).RaiseCanExecuteChanged();
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
        ((AsyncRelayCommand)DeleteCommand).RaiseCanExecuteChanged();

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
        ((AsyncRelayCommand)DeleteCommand).RaiseCanExecuteChanged();

        if (OnActiveWishlistChanged is not null) await OnActiveWishlistChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public sealed class WishlistEntryViewModel : INotifyPropertyChanged
{
    private string name;
    private bool isActive;

    public WishlistEntryViewModel(WishlistEntry entry, WishlistViewModel parent)
    {
        Id = entry.Id;
        name = entry.Name;
        Parent = parent;
        SelectCommand = new AsyncRelayCommand(() => parent.SelectAsync(this), () => true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public long Id { get; }

    public WishlistViewModel Parent { get; }

    public string Name
    {
        get => name;
        set
        {
            if (name == value) return;
            name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    public bool IsActive
    {
        get => isActive;
        set
        {
            if (isActive == value) return;
            isActive = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
        }
    }

    public ICommand SelectCommand { get; }
}
