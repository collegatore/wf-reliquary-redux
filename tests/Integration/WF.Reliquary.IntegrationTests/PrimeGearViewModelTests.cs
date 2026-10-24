using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.PrimeGear;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class PrimeGearViewModelTests
{
    private const long ActiveWishlistId = 1;

    private static readonly WishlistEntry ActiveWishlist =
        new(ActiveWishlistId, "Default", DateTimeOffset.UnixEpoch);

    // ── Merged display set ────────────────────────────────────────────────────

    [Test]
    public async Task LoadAsync_ShowsImportParts_WithCorrectCheckedState()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics", "Systems"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        var neuroptics = card.Parts.Single(p => p.PartName == "Neuroptics");
        var systems = card.Parts.Single(p => p.PartName == "Systems");

        Assert.Multiple(() =>
        {
            Assert.That(neuroptics.IsChecked, Is.True);
            Assert.That(neuroptics.IsGhost, Is.False);
            Assert.That(systems.IsChecked, Is.False);
            Assert.That(systems.IsGhost, Is.False);
        });
    }

    [Test]
    public async Task LoadAsync_IncludesGhostParts_FromWishlistNotInImport()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Systems"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: true)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        var ghost = card.Parts.Single(p => p.PartName == "Neuroptics");

        Assert.Multiple(() =>
        {
            Assert.That(ghost.IsGhost, Is.True);
            Assert.That(ghost.IsChecked, Is.True);
        });
    }

    [Test]
    public async Task LoadAsync_CreatesGhostOnlyCard_WhenGearAbsentFromImport()
    {
        var reader = ReaderWith([]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: true)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        var card = vm.ActiveCards.SingleOrDefault(c => c.GearName == "Ash Prime");
        Assert.That(card, Is.Not.Null);
        Assert.That(card!.Parts.Single().IsGhost, Is.True);
    }

    // ── Check / uncheck calls store ───────────────────────────────────────────

    [Test]
    public async Task CheckingPart_CallsAddItemAsync_OnStore()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        var part = vm.ActiveCards.Single().Parts.Single();
        part.IsChecked = true;
        await SettleAsync();

        store.Verify(s => s.AddItemAsync(
            ActiveWishlistId,
            It.Is<string>(n => n.Contains("NEUROPTICS", StringComparison.OrdinalIgnoreCase)),
            "Ash Prime",
            "Neuroptics",
            default), Times.Once);
    }

    [Test]
    public async Task UncheckingPart_CallsRemoveItemAsync_OnStore()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        var part = vm.ActiveCards.Single().Parts.Single();
        part.IsChecked = false;
        await SettleAsync();

        store.Verify(s => s.RemoveItemAsync(
            ActiveWishlistId,
            It.Is<string>(n => n.Contains("NEUROPTICS", StringComparison.OrdinalIgnoreCase)),
            default), Times.Once);
    }

    // ── Owned state & Show owned gear filtering ───────────────────────────────

    [Test]
    public async Task LoadAsync_ReflectsPersistedOwnedState_AndShowsOwnedByDefault()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Mag Prime", ["Systems"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]);
        var owned = OwnedStore("Ash Prime");

        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object);
        await SettleAsync();

        var ashCard = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        Assert.Multiple(() =>
        {
            Assert.That(ashCard.IsOwned, Is.True);
            Assert.That(ashCard.IsShown, Is.True, "owned gear is shown by default");
            Assert.That(vm.ActiveCards.Single(c => c.GearName == "Mag Prime").IsShown, Is.True);
        });
    }

    [Test]
    public async Task HideOwned_HidesAndRevealsOwnedCards_InBothSections()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Boar Prime", ["Stock"], IsVaulted: true)
        ]);
        var store = StoreWithItems([]);
        var owned = OwnedStore("Ash Prime", "Boar Prime");

        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object);
        await SettleAsync();

        // Both owned cards are shown by default.
        var ashCard = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        var boarCard = vm.VaultedCards.Single(c => c.GearName == "Boar Prime");
        Assert.That(ashCard.IsShown, Is.True);
        Assert.That(boarCard.IsShown, Is.True);

        vm.HideOwned = true;

        Assert.Multiple(() =>
        {
            Assert.That(ashCard.IsShown, Is.False);
            Assert.That(boarCard.IsShown, Is.False);
        });

        vm.HideOwned = false;

        Assert.Multiple(() =>
        {
            Assert.That(ashCard.IsShown, Is.True);
            Assert.That(boarCard.IsShown, Is.True);
        });
    }

    [Test]
    public async Task MarkingOwned_WithNoWishedParts_PersistsAndHidesCard()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]);
        var owned = OwnedStore();

        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object);
        await SettleAsync();

        vm.HideOwned = true;
        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
        Assert.That(card.IsShown, Is.False, "newly-owned card is hidden while owned gear is hidden");
    }

    // ── Search filtering ──────────────────────────────────────────────────────

    [Test]
    public async Task Search_FiltersCardsByTitleSubstring()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Mag Prime", ["Systems"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "volt";

        var voltCard = vm.ActiveCards.Single(c => c.GearName == "Volt Prime");
        var magCard = vm.ActiveCards.Single(c => c.GearName == "Mag Prime");
        Assert.That(voltCard.IsShown, Is.True);
        Assert.That(magCard.IsShown, Is.False);
    }

    [Test]
    public async Task Search_IsCaseInsensitive()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "VOLT";

        Assert.That(vm.ActiveCards.Single(c => c.GearName == "Volt Prime").IsShown, Is.True);
    }

    [Test]
    public async Task Search_DoesNotMatchPartLabels()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics", "Systems"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "Neuroptics";

        // "Neuroptics" is a part label on Ash Prime's card, but is not in the gear name.
        Assert.That(vm.ActiveCards.Single(c => c.GearName == "Ash Prime").IsShown, Is.False);
    }

    [Test]
    public async Task Search_BelowThreshold_HasNoEffect()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Mag Prime", ["Systems"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "vo"; // 2 chars — below threshold

        Assert.That(vm.ActiveCards.All(c => c.IsShown), Is.True, "below threshold all cards remain visible");
    }

    [Test]
    public async Task Search_AtThreshold_Activates()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Mag Prime", ["Systems"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "vol"; // exactly 3 chars — threshold met

        Assert.That(vm.ActiveCards.Single(c => c.GearName == "Volt Prime").IsShown, Is.True);
        Assert.That(vm.ActiveCards.Single(c => c.GearName == "Mag Prime").IsShown, Is.False);
    }

    [Test]
    public async Task Search_Clearing_RestoresAllCards()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Mag Prime", ["Systems"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "volt";
        vm.SearchText = string.Empty;

        Assert.That(vm.ActiveCards.All(c => c.IsShown), Is.True);
    }

    // ── Hidden-match hint and empty state ─────────────────────────────────────

    [Test]
    public async Task Search_HiddenMatchHint_CountsOwnedMatches()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var owned = OwnedStore("Volt Prime");

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, owned.Object);
        await SettleAsync();

        vm.HideOwned = true;
        vm.SearchText = "volt";

        Assert.That(vm.HiddenOwnedMatchCount, Is.EqualTo(1));
        Assert.That(vm.HasHiddenMatches, Is.True);
        Assert.That(vm.HiddenMatchHint, Does.Contain("owned"));
    }

    [Test]
    public async Task Search_HiddenMatchHint_CountsVaultedMatches()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: true)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        // "volt" matches the vaulted card, hidden by Hide Vaulted.
        vm.HideVaulted = true;
        vm.SearchText = "volt";

        Assert.That(vm.HiddenVaultedMatchCount, Is.EqualTo(1));
        Assert.That(vm.HasHiddenMatches, Is.True);
        Assert.That(vm.HiddenMatchHint, Does.Contain("vaulted"));
    }

    [Test]
    public async Task Search_HiddenMatchHint_ClearsWhenNoHiddenMatches()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "volt";

        Assert.That(vm.HasHiddenMatches, Is.False);
        Assert.That(vm.HiddenMatchHint, Is.Empty);
    }

    [Test]
    public async Task Search_EmptyState_SetWhenNoCardsMatch()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "zzz";

        Assert.That(vm.HasNoSearchResults, Is.True);
        Assert.That(vm.HasVisibleCards, Is.False);
    }

    [Test]
    public async Task Search_EmptyState_ClearsWhenMatchReturns()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        vm.SearchText = "zzz";
        Assert.That(vm.HasNoSearchResults, Is.True);

        vm.SearchText = "volt";
        Assert.That(vm.HasNoSearchResults, Is.False);
        Assert.That(vm.HasVisibleCards, Is.True);
    }

    // ── Confirm-tracking dialog flow ──────────────────────────────────────────

    [Test]
    public async Task MarkingOwned_WithWishedParts_RequestsConfirmation()
    {
        var (vm, owned, _, confirmCalls) = MakeConfirmScenario(OwnedConfirmChoice.KeepTracking);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        Assert.That(confirmCalls, Has.Count.EqualTo(1));
        Assert.That(confirmCalls[0], Is.EqualTo("Ash Prime"));
        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
    }

    [Test]
    public async Task ConfirmKeepTracking_MarksOwned_LeavesWishlist()
    {
        var (vm, owned, store, _) = MakeConfirmScenario(OwnedConfirmChoice.KeepTracking);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        Assert.That(card.IsOwned, Is.True);
        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
        store.Verify(s => s.RemoveItemAsync(It.IsAny<long>(), It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task ConfirmReleaseTracking_MarksOwned_RemovesPartsFromActiveWishlist()
    {
        var (vm, owned, store, _) = MakeConfirmScenario(OwnedConfirmChoice.ReleaseTracking);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        Assert.That(card.IsOwned, Is.True);
        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
        store.Verify(s => s.RemoveItemAsync(
            ActiveWishlistId,
            It.Is<string>(n => n.Contains("NEUROPTICS", StringComparison.OrdinalIgnoreCase)),
            default), Times.Once);
    }

    [Test]
    public async Task ConfirmCancel_PersistsNothing_AndRevertsIsOwned()
    {
        var (vm, owned, store, _) = MakeConfirmScenario(OwnedConfirmChoice.Cancel);
        await SettleAsync();

        var card = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        card.IsOwned = true;
        await SettleAsync();

        Assert.That(card.IsOwned, Is.False, "misclick reverts the owned mark");
        owned.Verify(s => s.SetOwnedAsync(It.IsAny<string>(), It.IsAny<bool>(), default), Times.Never);
        store.Verify(s => s.RemoveItemAsync(It.IsAny<long>(), It.IsAny<string>(), default), Times.Never);
        Assert.That(vm.ActiveCards.Any(c => c.GearName == "Ash Prime"), Is.True);
    }

    // ── Group operations: selection ───────────────────────────────────────────

    [Test]
    public async Task IsGroupMode_DefaultsOff_AndIsEphemeral()
    {
        var reader = ReaderWith([new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)]);
        var vm = new PrimeGearViewModel(reader.Object, StoreWithItems([]).Object, OwnedStore().Object);
        await SettleAsync();

        Assert.That(vm.IsGroupMode, Is.False);
    }

    [Test]
    public async Task SelectingCards_UpdatesSelectedCount()
    {
        var vm = await MakeMultiCardVm();
        var ash = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        var banshee = vm.ActiveCards.Single(c => c.GearName == "Banshee Prime");

        ash.IsSelected = true;
        banshee.IsSelected = true;
        Assert.That(vm.SelectedCount, Is.EqualTo(2));

        ash.IsSelected = false;
        Assert.That(vm.SelectedCount, Is.EqualTo(1));
    }

    [Test]
    public async Task SelectAll_SelectsOnlyVisibleCards()
    {
        // Ember Prime is owned; with Hide Owned on it is hidden and must not be selected.
        var vm = await MakeMultiCardVm(ownedGear: "Ember Prime");
        vm.HideOwned = true;

        vm.SelectAllCommand.Execute(null);

        var ember = vm.ActiveCards.Single(c => c.GearName == "Ember Prime");
        Assert.Multiple(() =>
        {
            Assert.That(ember.IsSelected, Is.False, "hidden owned card is not selected");
            Assert.That(vm.ActiveCards.Where(c => c.IsShown).All(c => c.IsSelected), Is.True);
            Assert.That(vm.SelectedCount, Is.EqualTo(vm.ActiveCards.Count(c => c.IsShown)));
        });
    }

    [Test]
    public async Task SelectNone_ClearsSelection()
    {
        var vm = await MakeMultiCardVm();
        vm.SelectAllCommand.Execute(null);
        Assert.That(vm.SelectedCount, Is.GreaterThan(0));

        vm.SelectNoneCommand.Execute(null);
        Assert.That(vm.SelectedCount, Is.EqualTo(0));
    }

    // ── Group operations: selection status text ───────────────────────────────

    private const string SelectionHint = "Click cards to select them";

    [Test]
    public async Task SelectionStatusText_ShowsHint_WhenGroupModeStartsEmpty()
    {
        var vm = await MakeMultiCardVm();
        vm.IsGroupMode = true;

        Assert.That(vm.SelectionStatusText, Is.EqualTo(SelectionHint));
    }

    [Test]
    public async Task SelectionStatusText_ShowsCount_AfterSelecting_AndNotifies()
    {
        var vm = await MakeMultiCardVm();
        vm.IsGroupMode = true;
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.ActiveCards.Single(c => c.GearName == "Ash Prime").IsSelected = true;
        Assert.That(vm.SelectionStatusText, Is.EqualTo("1 selected"));

        vm.ActiveCards.Single(c => c.GearName == "Banshee Prime").IsSelected = true;
        Assert.Multiple(() =>
        {
            Assert.That(vm.SelectionStatusText, Is.EqualTo("2 selected"));
            Assert.That(raised, Does.Contain(nameof(PrimeGearViewModel.SelectionStatusText)));
        });
    }

    [Test]
    public async Task SelectionStatusText_ReturnsToHint_AfterSelectNone()
    {
        var vm = await MakeMultiCardVm();
        vm.IsGroupMode = true;
        vm.SelectAllCommand.Execute(null);
        Assert.That(vm.SelectionStatusText, Is.Not.EqualTo(SelectionHint));

        vm.SelectNoneCommand.Execute(null);
        Assert.That(vm.SelectionStatusText, Is.EqualTo(SelectionHint));
    }

    [Test]
    public async Task SelectionStatusText_ReturnsToHint_AfterBulkAction()
    {
        var vm = await MakeMultiCardVm();
        vm.IsGroupMode = true;
        vm.ActiveCards.Single(c => c.GearName == "Ash Prime").IsSelected = true;

        vm.MarkSelectedOwnedCommand.Execute(null);
        await SettleAsync();

        Assert.That(vm.SelectionStatusText, Is.EqualTo(SelectionHint));
    }

    // ── Group operations: actions inactive without a selection ────────────────

    [Test]
    public async Task SelectionActions_AreInactive_WhileNothingIsSelected()
    {
        var vm = await MakeMultiCardVm();
        vm.IsGroupMode = true;

        Assert.Multiple(() =>
        {
            Assert.That(vm.SelectNoneCommand.CanExecute(null), Is.False);
            Assert.That(vm.MarkSelectedOwnedCommand.CanExecute(null), Is.False);
            Assert.That(vm.UnmarkSelectedCommand.CanExecute(null), Is.False);
            Assert.That(vm.SelectAllCommand.CanExecute(null), Is.True, "Select all is always available");
        });
    }

    [Test]
    public async Task SelectNone_ActivatesOnFirstSelection_AndDeactivatesWhenEmptied()
    {
        var vm = await MakeMultiCardVm();
        vm.IsGroupMode = true;
        var changes = 0;
        vm.SelectNoneCommand.CanExecuteChanged += (_, _) => changes++;

        vm.ActiveCards.Single(c => c.GearName == "Ash Prime").IsSelected = true;
        Assert.That(vm.SelectNoneCommand.CanExecute(null), Is.True);

        vm.ActiveCards.Single(c => c.GearName == "Banshee Prime").IsSelected = true;
        Assert.That(changes, Is.EqualTo(1), "Select none is re-evaluated only when the selection becomes empty or non-empty");

        vm.SelectNoneCommand.Execute(null);
        Assert.Multiple(() =>
        {
            Assert.That(vm.SelectNoneCommand.CanExecute(null), Is.False);
            Assert.That(changes, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task MarkAndUnmark_FollowWhatIsSelected()
    {
        var vm = await MakeMultiCardVm(ownedGear: "Ember Prime");
        vm.HideOwned = false; // owned cards are shown (the default), so they can be selected
        await SettleAsync();
        vm.IsGroupMode = true;
        var ember = vm.ActiveCards.Single(c => c.GearName == "Ember Prime");
        var ash = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");

        ember.IsSelected = true;
        Assert.Multiple(() =>
        {
            Assert.That(vm.MarkSelectedOwnedCommand.CanExecute(null), Is.False, "only an owned card is selected");
            Assert.That(vm.UnmarkSelectedCommand.CanExecute(null), Is.True);
        });

        ash.IsSelected = true;
        Assert.Multiple(() =>
        {
            Assert.That(vm.MarkSelectedOwnedCommand.CanExecute(null), Is.True, "owned and unowned cards are selected");
            Assert.That(vm.UnmarkSelectedCommand.CanExecute(null), Is.True);
        });

        ember.IsSelected = false;
        Assert.Multiple(() =>
        {
            Assert.That(vm.MarkSelectedOwnedCommand.CanExecute(null), Is.True);
            Assert.That(vm.UnmarkSelectedCommand.CanExecute(null), Is.False, "only an unowned card is selected");
        });
    }

    [Test]
    public async Task MarkAndUnmark_ReevaluateOnEverySelectionChange()
    {
        var vm = await MakeMultiCardVm();
        vm.IsGroupMode = true;
        var markChanges = 0;
        var unmarkChanges = 0;
        vm.MarkSelectedOwnedCommand.CanExecuteChanged += (_, _) => markChanges++;
        vm.UnmarkSelectedCommand.CanExecuteChanged += (_, _) => unmarkChanges++;

        vm.ActiveCards.Single(c => c.GearName == "Ash Prime").IsSelected = true;
        vm.ActiveCards.Single(c => c.GearName == "Banshee Prime").IsSelected = true;

        Assert.Multiple(() =>
        {
            Assert.That(markChanges, Is.EqualTo(2));
            Assert.That(unmarkChanges, Is.EqualTo(2));
        });
    }

    // ── Group operations: bulk mark / unmark ──────────────────────────────────

    [Test]
    public async Task BulkMark_WithNoWishedParts_MarksDirectly_NoPrompt()
    {
        var promptCount = 0;
        var owned = OwnedStore();
        var vm = await MakeMultiCardVm(owned: owned, onBatchPrompt: () => promptCount++);

        var ash = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        ash.IsSelected = true;
        vm.MarkSelectedOwnedCommand.Execute(null);
        await SettleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(promptCount, Is.EqualTo(0), "no tracked parts → no dialog");
            Assert.That(ash.IsOwned, Is.True);
            Assert.That(vm.SelectedCount, Is.EqualTo(0), "selection empty after bulk action");
        });
        owned.Verify(s => s.SetOwnedAsync("Ash Prime", true, default), Times.Once);
    }

    [Test]
    public async Task BulkMark_WithWishedParts_PromptsOnce_KeepAppliesToWholeBatch()
    {
        var promptCount = 0;
        var owned = OwnedStore();
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "BANSHEE PRIME NEUROPTICS", "Banshee Prime", "Neuroptics", IsGhost: false),
            new WishlistItem(ActiveWishlistId, "VOLT PRIME NEUROPTICS", "Volt Prime", "Neuroptics", IsGhost: false)
        ]);
        var vm = await MakeMultiCardVm(owned: owned, store: store,
            batchChoice: OwnedConfirmChoice.KeepTracking, onBatchPrompt: () => promptCount++);

        vm.ActiveCards.Single(c => c.GearName == "Banshee Prime").IsSelected = true;
        vm.ActiveCards.Single(c => c.GearName == "Volt Prime").IsSelected = true;
        vm.MarkSelectedOwnedCommand.Execute(null);
        await SettleAsync();

        Assert.That(promptCount, Is.EqualTo(1), "a single dialog for the whole batch");
        owned.Verify(s => s.SetOwnedAsync("Banshee Prime", true, default), Times.Once);
        owned.Verify(s => s.SetOwnedAsync("Volt Prime", true, default), Times.Once);
        store.Verify(s => s.RemoveItemAsync(It.IsAny<long>(), It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task BulkMark_WithWishedParts_ReleaseRemovesPartsForWholeBatch()
    {
        var owned = OwnedStore();
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "BANSHEE PRIME NEUROPTICS", "Banshee Prime", "Neuroptics", IsGhost: false),
            new WishlistItem(ActiveWishlistId, "VOLT PRIME NEUROPTICS", "Volt Prime", "Neuroptics", IsGhost: false)
        ]);
        var vm = await MakeMultiCardVm(owned: owned, store: store,
            batchChoice: OwnedConfirmChoice.ReleaseTracking);

        vm.ActiveCards.Single(c => c.GearName == "Banshee Prime").IsSelected = true;
        vm.ActiveCards.Single(c => c.GearName == "Volt Prime").IsSelected = true;
        vm.MarkSelectedOwnedCommand.Execute(null);
        await SettleAsync();

        owned.Verify(s => s.SetOwnedAsync("Banshee Prime", true, default), Times.Once);
        owned.Verify(s => s.SetOwnedAsync("Volt Prime", true, default), Times.Once);
        store.Verify(s => s.RemoveItemAsync(ActiveWishlistId,
            It.Is<string>(n => n.Contains("BANSHEE", StringComparison.OrdinalIgnoreCase)), default), Times.Once);
        store.Verify(s => s.RemoveItemAsync(ActiveWishlistId,
            It.Is<string>(n => n.Contains("VOLT", StringComparison.OrdinalIgnoreCase)), default), Times.Once);
    }

    [Test]
    public async Task BulkMark_Cancel_MarksNothing_AndLeavesSelection()
    {
        var owned = OwnedStore();
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "BANSHEE PRIME NEUROPTICS", "Banshee Prime", "Neuroptics", IsGhost: false)
        ]);
        var vm = await MakeMultiCardVm(owned: owned, store: store,
            batchChoice: OwnedConfirmChoice.Cancel);

        vm.ActiveCards.Single(c => c.GearName == "Banshee Prime").IsSelected = true;
        vm.MarkSelectedOwnedCommand.Execute(null);
        await SettleAsync();

        Assert.That(vm.SelectedCount, Is.EqualTo(1), "Cancel leaves the selection unchanged");
        owned.Verify(s => s.SetOwnedAsync(It.IsAny<string>(), It.IsAny<bool>(), default), Times.Never);
    }

    [Test]
    public async Task BulkUnmark_ClearsOwnedForAllSelected()
    {
        var owned = OwnedStore("Ember Prime", "Volt Prime");
        var vm = await MakeMultiCardVm(owned: owned);
        vm.HideOwned = false; // owned cards are shown (the default), so they can be selected
        await SettleAsync();

        vm.ActiveCards.Single(c => c.GearName == "Ember Prime").IsSelected = true;
        vm.ActiveCards.Single(c => c.GearName == "Volt Prime").IsSelected = true;
        vm.UnmarkSelectedCommand.Execute(null);
        await SettleAsync();

        owned.Verify(s => s.SetOwnedAsync("Ember Prime", false, default), Times.Once);
        owned.Verify(s => s.SetOwnedAsync("Volt Prime", false, default), Times.Once);
        Assert.That(vm.SelectedCount, Is.EqualTo(0));
    }

    [Test]
    public async Task BulkMark_WithHideOwnedOn_HidesCards_AndEmptiesSelection()
    {
        var vm = await MakeMultiCardVm();
        vm.HideOwned = true;
        var ash = vm.ActiveCards.Single(c => c.GearName == "Ash Prime");
        ash.IsSelected = true;
        vm.MarkSelectedOwnedCommand.Execute(null);
        await SettleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(ash.IsOwned, Is.True);
            Assert.That(ash.IsShown, Is.False, "marked-owned card is hidden while Hide Owned is on");
            Assert.That(vm.SelectedCount, Is.EqualTo(0));
        });
    }

    // ── Group operations: selection clears on view changes ────────────────────

    [Test]
    public async Task ChangingSearch_ClearsSelection()
    {
        var vm = await MakeMultiCardVm();
        vm.ActiveCards.First().IsSelected = true;

        vm.SearchText = "prime";
        Assert.That(vm.SelectedCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TogglingHideVaulted_ClearsSelection()
    {
        var vm = await MakeMultiCardVm();
        vm.ActiveCards.First().IsSelected = true;

        vm.HideVaulted = true;
        Assert.That(vm.SelectedCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TogglingHideNotWishlisted_ClearsSelection()
    {
        var vm = await MakeMultiCardVm();
        vm.ActiveCards.First().IsSelected = true;

        vm.HideNotWishlisted = true;
        Assert.That(vm.SelectedCount, Is.EqualTo(0));
    }

    // ── Filters: defaults, marker, summary, persistence ───────────────────────

    [Test]
    public async Task Filters_DefaultToNothingHidden()
    {
        var vm = await MakeMultiCardVm();

        Assert.Multiple(() =>
        {
            Assert.That(vm.HideVaulted, Is.False);
            Assert.That(vm.HideOwned, Is.False);
            Assert.That(vm.HideNotWishlisted, Is.False);
            Assert.That(vm.IsFilterActive, Is.False);
            Assert.That(vm.FilterSummary, Is.EqualTo("Filters: nothing hidden"));
        });
    }

    [Test]
    public async Task FilterSummary_ListsEveryHiddenCategory()
    {
        var vm = await MakeMultiCardVm();

        vm.HideOwned = true;
        vm.HideNotWishlisted = true;

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsFilterActive, Is.True);
            Assert.That(vm.FilterSummary, Is.EqualTo("Filters: hiding owned, not wishlisted"));
        });
    }

    [Test]
    public async Task FilterChange_InvokesSaveHook_ButLoadDoesNot()
    {
        var vm = await MakeMultiCardVm(ownedGear: "Ember Prime");
        var saves = 0;
        vm.SaveFilterSettingsAsync = () => { saves++; return Task.CompletedTask; };

        vm.LoadFilterSettings(hideVaulted: true, hideOwned: true, hideNotWishlisted: false);
        Assert.Multiple(() =>
        {
            Assert.That(saves, Is.Zero);
            Assert.That(vm.HideOwned, Is.True);
            Assert.That(vm.ActiveCards.Single(c => c.GearName == "Ember Prime").IsShown, Is.False,
                "loaded filters apply to the cards");
        });

        vm.HideVaulted = false;
        Assert.That(saves, Is.EqualTo(1));
    }

    [Test]
    public async Task TogglingHideOwned_ClearsSelection()
    {
        var vm = await MakeMultiCardVm();
        vm.ActiveCards.First().IsSelected = true;

        vm.HideOwned = true;
        Assert.That(vm.SelectedCount, Is.EqualTo(0));
    }

    [Test]
    public async Task LeavingGroupMode_ClearsSelection()
    {
        var vm = await MakeMultiCardVm();
        vm.IsGroupMode = true;
        vm.ActiveCards.First().IsSelected = true;

        vm.IsGroupMode = false;
        Assert.That(vm.SelectedCount, Is.EqualTo(0));
    }

    // ── Wishlisted-only filter ────────────────────────────────────────────────

    [Test]
    public async Task HideNotWishlisted_False_AllCardsVisible()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Mag Prime", ["Systems"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]); // no parts checked

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        Assert.That(vm.ActiveCards.All(c => c.IsShown), Is.True,
            "with Hide Not wishlisted off, cards with no checked parts remain visible");
    }

    [Test]
    public async Task HideNotWishlisted_True_HidesCardWithNoCheckedParts()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]); // no parts checked

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        vm.HideNotWishlisted = true;

        Assert.That(vm.ActiveCards.Single(c => c.GearName == "Ash Prime").IsShown, Is.False);
    }

    [Test]
    public async Task HideNotWishlisted_True_ShowsCardWithCheckedPart()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics", "Systems"], IsVaulted: false),
            new PrimeGearEntry("Mag Prime", ["Systems"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: false)
        ]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        vm.HideNotWishlisted = true;

        Assert.Multiple(() =>
        {
            Assert.That(vm.ActiveCards.Single(c => c.GearName == "Ash Prime").IsShown, Is.True,
                "card with at least one checked part is shown");
            Assert.That(vm.ActiveCards.Single(c => c.GearName == "Mag Prime").IsShown, Is.False,
                "card with no checked parts is hidden");
        });
    }

    [Test]
    public async Task HideNotWishlisted_Unchecking_RestoresHiddenCards()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object, OwnedStore().Object);
        await SettleAsync();

        vm.HideNotWishlisted = true;
        Assert.That(vm.ActiveCards.Single().IsShown, Is.False);

        vm.HideNotWishlisted = false;
        Assert.That(vm.ActiveCards.Single().IsShown, Is.True);
    }

    [Test]
    public async Task HideNotWishlisted_OwnedFilterTakesPrecedence_OwnedCardWithCheckedPartsIsHidden()
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: false)
        ]);
        var owned = OwnedStore("Ash Prime");

        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object);
        await SettleAsync();

        // Both Hide Owned and Hide Not wishlisted on
        vm.HideOwned = true;
        vm.HideNotWishlisted = true;

        Assert.That(vm.ActiveCards.Single(c => c.GearName == "Ash Prime").IsShown, Is.False,
            "owned card is hidden regardless of wishlist state while Hide Owned is on");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a view model with several active cards (Ash, Banshee, Volt, Ember) and
    /// optional owned gear, an owned-store mock, a wishlist-store mock, and a batched
    /// tracking-prompt hook recording invocations and returning <paramref name="batchChoice"/>.
    /// </summary>
    private static async Task<PrimeGearViewModel> MakeMultiCardVm(
        string? ownedGear = null,
        Mock<IOwnedGearStore>? owned = null,
        Mock<IWishlistStore>? store = null,
        OwnedConfirmChoice batchChoice = OwnedConfirmChoice.KeepTracking,
        Action? onBatchPrompt = null)
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Banshee Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Volt Prime", ["Neuroptics"], IsVaulted: false),
            new PrimeGearEntry("Ember Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        owned ??= ownedGear is null ? OwnedStore() : OwnedStore(ownedGear);
        store ??= StoreWithItems([]);

        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object)
        {
            ConfirmOwnedTracking = gn => Task.FromResult(batchChoice),
            ConfirmOwnedTrackingBatch = _ =>
            {
                onBatchPrompt?.Invoke();
                return Task.FromResult(batchChoice);
            }
        };
        await SettleAsync();
        return vm;
    }

    private static (PrimeGearViewModel Vm, Mock<IOwnedGearStore> Owned, Mock<IWishlistStore> Store, List<string> ConfirmCalls)
        MakeConfirmScenario(OwnedConfirmChoice choice)
    {
        var reader = ReaderWith([
            new PrimeGearEntry("Ash Prime", ["Neuroptics"], IsVaulted: false)
        ]);
        var store = StoreWithItems([
            new WishlistItem(ActiveWishlistId, "ASH PRIME NEUROPTICS", "Ash Prime", "Neuroptics", IsGhost: false)
        ]);
        var owned = OwnedStore();

        var confirmCalls = new List<string>();
        var vm = new PrimeGearViewModel(reader.Object, store.Object, owned.Object)
        {
            ConfirmOwnedTracking = gearName =>
            {
                confirmCalls.Add(gearName);
                return Task.FromResult(choice);
            }
        };
        return (vm, owned, store, confirmCalls);
    }

    private static Mock<IOwnedGearStore> OwnedStore(params string[] ownedGearNames)
    {
        var mock = new Mock<IOwnedGearStore>();
        mock.Setup(s => s.GetOwnedGearNamesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ownedGearNames);
        mock.Setup(s => s.SetOwnedAsync(It.IsAny<string>(), It.IsAny<bool>(), default))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static Mock<IPrimeGearReader> ReaderWith(IReadOnlyList<PrimeGearEntry> entries)
    {
        var mock = new Mock<IPrimeGearReader>();
        mock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entries);
        return mock;
    }

    private static Mock<IWishlistStore> StoreWithItems(IReadOnlyList<WishlistItem> items)
    {
        var mock = new Mock<IWishlistStore>();
        mock.Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ActiveWishlist);
        mock.Setup(s => s.GetItemsAsync(ActiveWishlistId, It.IsAny<CancellationToken>())).ReturnsAsync(items);
        mock.Setup(s => s.AddItemAsync(
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        mock.Setup(s => s.RemoveItemAsync(It.IsAny<long>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static async Task SettleAsync()
    {
        await Task.Yield();
        await Task.Yield();
    }
}
