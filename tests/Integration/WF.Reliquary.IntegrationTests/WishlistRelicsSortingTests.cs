using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistRelicsSortingTests
{
    // ── Test data ──────────────────────────────────────────────────────────

    // Lith A1: 1 Common + 1 Rare wishlisted → score 3+5=8
    private static WishlistRelicEntry LithA1 { get; } = new(
        "Lith A1 Relic", "Lith", IsVaulted: false,
        [
            new WishlistRelicReward("Common Part", "Common", IsWishlisted: true),
            new WishlistRelicReward("Rare Part", "Rare", IsWishlisted: true),
        ]);

    // Lith B2: 1 Uncommon wishlisted → score 4
    private static WishlistRelicEntry LithB2 { get; } = new(
        "Lith B2 Relic", "Lith", IsVaulted: false,
        [
            new WishlistRelicReward("Uncommon Part", "Uncommon", IsWishlisted: true),
        ]);

    // Meso C3: 2 Common wishlisted → score 6
    private static WishlistRelicEntry MesoC3 { get; } = new(
        "Meso C3 Relic", "Meso", IsVaulted: false,
        [
            new WishlistRelicReward("Common Part A", "Common", IsWishlisted: true),
            new WishlistRelicReward("Common Part B", "Common", IsWishlisted: true),
        ]);

    // Axi D4: 1 Rare wishlisted + 1 non-wishlisted Rare → score 5
    private static WishlistRelicEntry AxiD4 { get; } = new(
        "Axi D4 Relic", "Axi", IsVaulted: false,
        [
            new WishlistRelicReward("Rare Part", "Rare", IsWishlisted: true),
            new WishlistRelicReward("Other Rare", "Rare", IsWishlisted: false),
        ]);

    // Neo E5: 1 Common wishlisted → score 3  (same score as score-tie scenario)
    private static WishlistRelicEntry NeoE5 { get; } = new(
        "Neo E5 Relic", "Neo", IsVaulted: false,
        [
            new WishlistRelicReward("Common Part", "Common", IsWishlisted: true),
        ]);

    // LithA1 score=8, MesoC3 score=6, AxiD4 score=5, LithB2 score=4, NeoE5 score=3

    // ── Sort mode — Era ────────────────────────────────────────────────────

    [Test]
    public async Task SortMode_Era_Ascending_OrdersByEraRankThenName()
    {
        var vm = await LoadedViewModelAsync(AxiD4, MesoC3, LithB2, LithA1, NeoE5);

        vm.SortMode = RelicSortMode.Era; // already default but explicit
        vm.SortDescending = false; // default is descending

        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Lith A1 Relic", "Lith B2 Relic", "Meso C3 Relic", "Neo E5 Relic", "Axi D4 Relic" }));
    }

    [Test]
    public async Task SortMode_Era_Descending_ReversesEraButKeepsNamesAscendingWithinEra()
    {
        var vm = await LoadedViewModelAsync(LithA1, LithB2, MesoC3, NeoE5, AxiD4);

        vm.SortMode = RelicSortMode.Era;
        vm.SortDescending = true;

        // Era reversed: Axi, Neo, Meso, Lith — names still A→Z within each era
        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Axi D4 Relic", "Neo E5 Relic", "Meso C3 Relic", "Lith A1 Relic", "Lith B2 Relic" }));
    }

    // ── Sort mode — Alphabetical ───────────────────────────────────────────

    [Test]
    public async Task SortMode_Alphabetical_Ascending_OrdersByNameOnly()
    {
        var vm = await LoadedViewModelAsync(MesoC3, LithA1, AxiD4);

        vm.SortMode = RelicSortMode.Alphabetical;

        // Alphabetical: Axi D4, Lith A1, Meso C3
        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Axi D4 Relic", "Lith A1 Relic", "Meso C3 Relic" }));
    }

    [Test]
    public async Task SortMode_Alphabetical_Descending_ReversesName()
    {
        var vm = await LoadedViewModelAsync(LithA1, MesoC3, AxiD4);

        vm.SortMode = RelicSortMode.Alphabetical;
        vm.SortDescending = true;

        // Descending: Meso C3, Lith A1, Axi D4
        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Meso C3 Relic", "Lith A1 Relic", "Axi D4 Relic" }));
    }

    // ── Sort mode — Wishlist Score ─────────────────────────────────────────

    [Test]
    public async Task SortMode_WishlistScore_Descending_OrdersHighestFirst()
    {
        // LithA1=8, MesoC3=6, AxiD4=5, LithB2=4, NeoE5=3
        var vm = await LoadedViewModelAsync(LithB2, NeoE5, MesoC3, AxiD4, LithA1);

        vm.SortMode = RelicSortMode.WishlistScore;
        // Default direction for WishlistScore is descending — verify
        Assert.That(vm.SortDescending, Is.True);

        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Lith A1 Relic", "Meso C3 Relic", "Axi D4 Relic", "Lith B2 Relic", "Neo E5 Relic" }));
    }

    [Test]
    public async Task SortMode_WishlistScore_Ascending_OrdersLowestFirst()
    {
        // LithA1=8, MesoC3=6, AxiD4=5, LithB2=4, NeoE5=3
        var vm = await LoadedViewModelAsync(LithA1, MesoC3, AxiD4, LithB2, NeoE5);

        vm.SortMode = RelicSortMode.WishlistScore;
        vm.SortDescending = false;

        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Neo E5 Relic", "Lith B2 Relic", "Axi D4 Relic", "Meso C3 Relic", "Lith A1 Relic" }));
    }

    [Test]
    public async Task SortMode_WishlistScore_NonWishlistedRewards_DoNotContributeToScore()
    {
        // AxiD4 has 1 wishlisted Rare + 1 non-wishlisted Rare → score 5, not 10
        // NeoE5 has 1 wishlisted Common → score 3
        var vm = await LoadedViewModelAsync(NeoE5, AxiD4);

        vm.SortMode = RelicSortMode.WishlistScore;
        vm.SortDescending = true; // highest first

        // Axi D4 (score 5) before Neo E5 (score 3)
        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Axi D4 Relic", "Neo E5 Relic" }));
    }

    [Test]
    public async Task SortMode_WishlistScore_TieBreak_Descending_ByReversedEraThenAscendingName()
    {
        // LithB2 and NeoE5 both have score=3 (one Uncommon=4... wait, LithB2=4, NeoE5=3)
        // Let me use entries with the same score:
        // Lith A1 Common Only: score=3, Neo E5 Common Only: score=3
        var lithA1Common = new WishlistRelicEntry(
            "Lith A1 Relic", "Lith", IsVaulted: false,
            [new WishlistRelicReward("Common Part", "Common", IsWishlisted: true)]);
        var neoA1Common = new WishlistRelicEntry(
            "Neo A1 Relic", "Neo", IsVaulted: false,
            [new WishlistRelicReward("Common Part", "Common", IsWishlisted: true)]);
        var axiA1Common = new WishlistRelicEntry(
            "Axi A1 Relic", "Axi", IsVaulted: false,
            [new WishlistRelicReward("Common Part", "Common", IsWishlisted: true)]);

        var vm = await LoadedViewModelAsync(lithA1Common, neoA1Common, axiA1Common);

        vm.SortMode = RelicSortMode.WishlistScore;
        // Descending score + reversed era (Axi first) + ascending name
        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Axi A1 Relic", "Neo A1 Relic", "Lith A1 Relic" }));
    }

    [Test]
    public async Task SortMode_WishlistScore_TieBreak_Ascending_ByEraAscendingThenName()
    {
        var lithA1Common = new WishlistRelicEntry(
            "Lith A1 Relic", "Lith", IsVaulted: false,
            [new WishlistRelicReward("Common Part", "Common", IsWishlisted: true)]);
        var neoA1Common = new WishlistRelicEntry(
            "Neo A1 Relic", "Neo", IsVaulted: false,
            [new WishlistRelicReward("Common Part", "Common", IsWishlisted: true)]);
        var axiA1Common = new WishlistRelicEntry(
            "Axi A1 Relic", "Axi", IsVaulted: false,
            [new WishlistRelicReward("Common Part", "Common", IsWishlisted: true)]);

        var vm = await LoadedViewModelAsync(axiA1Common, neoA1Common, lithA1Common);

        vm.SortMode = RelicSortMode.WishlistScore;
        vm.SortDescending = false; // ascending score + era Lith first + ascending name

        Assert.That(
            vm.Cards.Select(c => c.RelicName),
            Is.EqualTo(new[] { "Lith A1 Relic", "Neo A1 Relic", "Axi A1 Relic" }));
    }

    // ── Default direction on mode change ──────────────────────────────────

    [Test]
    public async Task SortMode_Change_ToWishlistScore_DefaultsToDescending()
    {
        var vm = await LoadedViewModelAsync(LithA1);

        vm.SortMode = RelicSortMode.Era; // start at Era ascending
        vm.SortDescending = false;

        vm.SortMode = RelicSortMode.WishlistScore;

        Assert.That(vm.SortDescending, Is.True);
    }

    [Test]
    public async Task SortMode_Change_ToEra_DefaultsToDescending()
    {
        var vm = await LoadedViewModelAsync(LithA1);

        vm.SortMode = RelicSortMode.Alphabetical; // sets ascending
        vm.SortMode = RelicSortMode.Era;

        Assert.That(vm.SortDescending, Is.True);
    }

    [Test]
    public async Task SortMode_Change_ToAlphabetical_DefaultsToAscending()
    {
        var vm = await LoadedViewModelAsync(LithA1);

        vm.SortMode = RelicSortMode.WishlistScore; // sets descending
        vm.SortMode = RelicSortMode.Alphabetical;

        Assert.That(vm.SortDescending, Is.False);
    }

    [Test]
    public async Task SortMode_SameMode_Reselect_LeavesDirectionUnchanged()
    {
        var vm = await LoadedViewModelAsync(LithA1);

        vm.SortMode = RelicSortMode.Era;
        vm.SortDescending = true; // user toggled direction

        // Re-select Era: direction must stay true
        vm.SortModeIndex = 0; // Era

        Assert.That(vm.SortDescending, Is.True);
    }

    [Test]
    public async Task SortMode_DirectionToggleCommand_TogglesDescending()
    {
        var vm = await LoadedViewModelAsync(LithA1);

        vm.SortMode = RelicSortMode.WishlistScore; // descending by default
        Assert.That(vm.SortDescending, Is.True);

        vm.ToggleSortDirectionCommand.Execute(null);
        Assert.That(vm.SortDescending, Is.False);

        vm.ToggleSortDirectionCommand.Execute(null);
        Assert.That(vm.SortDescending, Is.True);
    }

    // ── Sort consistency between Cards and TableRows ───────────────────────

    [Test]
    public async Task Sort_AppliesConsistentlyToCardsAndTableRows()
    {
        var vm = await LoadedViewModelAsync(MesoC3, LithA1, AxiD4);

        vm.SortMode = RelicSortMode.Alphabetical;

        var cardNames = vm.Cards.Select(c => c.RelicName).ToList();
        var tableNames = vm.TableRows.Select(r => r.DisplayName).ToList();

        // Cards show full relic names; TableRows show clipped names (trailing "Relic" removed).
        // Verify the order matches by era content — Axi, Lith, Meso alphabetically.
        Assert.Multiple(() =>
        {
            Assert.That(cardNames, Is.EqualTo(new[] { "Axi D4 Relic", "Lith A1 Relic", "Meso C3 Relic" }));
            Assert.That(tableNames, Is.EqualTo(new[] { "Axi D4", "Lith A1", "Meso C3" }));
        });
    }

    // ── LoadSortSettings (startup restore) ────────────────────────────────

    [Test]
    public async Task LoadSortSettings_RestoresValuesWithoutApplyingDefaultDirection()
    {
        // WishlistScore ascending (non-default direction) should survive a startup restore.
        var vm = await LoadedViewModelAsync(LithA1, NeoE5);

        vm.LoadSortSettings(RelicSortMode.WishlistScore, descending: false);

        Assert.Multiple(() =>
        {
            Assert.That(vm.SortMode, Is.EqualTo(RelicSortMode.WishlistScore));
            Assert.That(vm.SortDescending, Is.False);
        });
    }

    // ── Defaults and non-default marker ────────────────────────────────────

    [Test]
    public void SortMode_DefaultsToEraDescending()
    {
        var vm = CreateViewModel();

        Assert.Multiple(() =>
        {
            Assert.That(vm.SortMode, Is.EqualTo(RelicSortMode.Era));
            Assert.That(vm.SortDescending, Is.True);
            Assert.That(vm.IsSortNonDefault, Is.False);
        });
    }

    [Test]
    public void IsSortNonDefault_TrueForAnyOtherSort()
    {
        var vm = CreateViewModel();

        vm.SortDescending = false;
        Assert.That(vm.IsSortNonDefault, Is.True, "Era ascending");

        vm.SortDescending = true;
        vm.SortMode = RelicSortMode.WishlistScore;
        Assert.That(vm.IsSortNonDefault, Is.True, "Wishlist score descending");
    }

    [Test]
    public void LoadSortSettings_SavedEraAscending_IsNonDefault()
    {
        var vm = CreateViewModel();

        vm.LoadSortSettings(RelicSortMode.Era, descending: false);

        Assert.That(vm.IsSortNonDefault, Is.True);
    }

    [Test]
    public void SortSummary_DescribesModeAndDirection()
    {
        var vm = CreateViewModel();
        Assert.That(vm.SortSummary, Is.EqualTo("Sorted by era, descending"));

        vm.SortMode = RelicSortMode.Alphabetical;
        Assert.That(vm.SortSummary, Is.EqualTo("Sorted alphabetically, ascending"));
    }

    [Test]
    public void SortChange_InvokesSaveHook_ButLoadDoesNot()
    {
        var vm = CreateViewModel();
        var saves = 0;
        vm.SaveSortSettingsAsync = () => { saves++; return Task.CompletedTask; };

        vm.LoadSortSettings(RelicSortMode.Alphabetical, descending: false);
        Assert.That(saves, Is.Zero);

        vm.ToggleSortDirectionCommand.Execute(null);
        Assert.That(saves, Is.EqualTo(1));
    }

    // ── Filters and Display ────────────────────────────────────────────────

    [Test]
    public async Task HideVaulted_DefaultsOff_AndHidesVaultedRelicsWhenOn()
    {
        var vaulted = new WishlistRelicEntry("Meso V1 Relic", "Meso", IsVaulted: true,
            [new WishlistRelicReward("Part", "Common", IsWishlisted: true)]);
        var vm = await LoadedViewModelAsync(LithA1, vaulted);

        Assert.Multiple(() =>
        {
            Assert.That(vm.HideVaulted, Is.False);
            Assert.That(vm.IsFilterActive, Is.False);
            Assert.That(vm.FilterSummary, Is.EqualTo("Filters: nothing hidden"));
            Assert.That(vm.Cards.Select(c => c.RelicName), Does.Contain("Meso V1 Relic"));
        });

        vm.HideVaulted = true;

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsFilterActive, Is.True);
            Assert.That(vm.FilterSummary, Is.EqualTo("Filters: hiding vaulted relics"));
            Assert.That(vm.Cards.Select(c => c.RelicName), Does.Not.Contain("Meso V1 Relic"));
            Assert.That(vm.TableRows.Select(r => r.DisplayName), Does.Not.Contain("Meso V1"));
        });
    }

    [Test]
    public void FilterChange_InvokesSaveHook_ButLoadDoesNot()
    {
        var vm = CreateViewModel();
        var saves = 0;
        vm.SaveFilterSettingsAsync = () => { saves++; return Task.CompletedTask; };

        vm.LoadFilterSettings(hideVaulted: true);
        Assert.Multiple(() =>
        {
            Assert.That(vm.HideVaulted, Is.True);
            Assert.That(saves, Is.Zero);
        });

        vm.HideVaulted = false;
        Assert.That(saves, Is.EqualTo(1));
    }

    [Test]
    public void IsDisplayNonDefault_FollowsViewModeAndShowAllRewards()
    {
        var vm = CreateViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(vm.IsDisplayNonDefault, Is.False, "Compact, all rewards off");
            Assert.That(vm.DisplaySummary, Is.EqualTo("Display: Compact view"));
        });

        vm.ShowAllRewards = true;
        Assert.Multiple(() =>
        {
            Assert.That(vm.IsDisplayNonDefault, Is.True, "all rewards on");
            Assert.That(vm.DisplaySummary, Is.EqualTo("Display: Compact view, all rewards shown"));
        });

        vm.ShowAllRewards = false;
        vm.ViewMode = RelicViewMode.Table;
        Assert.Multiple(() =>
        {
            Assert.That(vm.IsDisplayNonDefault, Is.True, "Table mode");
            Assert.That(vm.DisplaySummary, Is.EqualTo("Display: Table view"));
        });
    }

    [Test]
    public void DisplayChange_InvokesSaveHook_ButLoadDoesNot()
    {
        var vm = CreateViewModel();
        var saves = 0;
        vm.SaveDisplaySettingsAsync = () => { saves++; return Task.CompletedTask; };

        vm.LoadDisplaySettings(RelicViewMode.Verbatim, showAllRewards: true);
        Assert.Multiple(() =>
        {
            Assert.That(vm.ViewMode, Is.EqualTo(RelicViewMode.Verbatim));
            Assert.That(vm.ShowAllRewards, Is.True);
            Assert.That(saves, Is.Zero);
        });

        vm.ViewMode = RelicViewMode.Table;
        vm.ShowAllRewards = false;
        Assert.That(saves, Is.EqualTo(2));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static WishlistRelicsViewModel CreateViewModel()
    {
        return new WishlistRelicsViewModel(
            Mock.Of<IWishlistRelicsReader>(),
            Mock.Of<IWishlistStore>());
    }

    private static async Task<WishlistRelicsViewModel> LoadedViewModelAsync(params WishlistRelicEntry[] entries)
    {
        var wishlistStore = new Mock<IWishlistStore>();
        wishlistStore
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WishlistEntry(1, "Default", DateTimeOffset.UnixEpoch));

        var reader = new Mock<IWishlistRelicsReader>();
        reader
            .Setup(r => r.GetRelicsForWishlistAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var vm = new WishlistRelicsViewModel(reader.Object, wishlistStore.Object);
        await vm.ReloadAsync();
        return vm;
    }
}
