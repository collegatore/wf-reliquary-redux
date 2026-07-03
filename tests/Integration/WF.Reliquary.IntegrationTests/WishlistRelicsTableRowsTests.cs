using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistRelicsTableRowsTests
{
    private static WishlistRelicEntry LithA1 { get; } = new(
        "Lith A1 Relic", "Lith", IsVaulted: false,
        [
            new WishlistRelicReward("Forma Blueprint", "Common", IsWishlisted: true),
            new WishlistRelicReward("Revenant Prime Systems Blueprint", "Uncommon", IsWishlisted: false),
            new WishlistRelicReward("Revenant Prime Blueprint", "Rare", IsWishlisted: true),
        ]);

    private static WishlistRelicEntry MesoB2Vaulted { get; } = new(
        "Meso B2 Relic", "Meso", IsVaulted: true,
        [
            new WishlistRelicReward("Mag Prime Blueprint", "Common", IsWishlisted: true),
        ]);

    [Test]
    public async Task TableRows_AreRelicRows_WithClippedNamesAndChildren()
    {
        var vm = await LoadedViewModelAsync(LithA1);

        Assert.That(vm.TableRows, Has.Count.EqualTo(1));
        var row = vm.TableRows[0];
        Assert.Multiple(() =>
        {
            Assert.That(row, Is.InstanceOf<RelicTableRowViewModel>());
            Assert.That(row.DisplayName, Is.EqualTo("Lith A1"), "trailing 'Relic' clipped");
            Assert.That(row.ShowsActions, Is.True);
            // Only the two wishlisted rewards (Common, Rare) become child rows; the
            // non-wishlisted Uncommon reward is omitted from expansion entirely.
            Assert.That(row.Children, Is.Not.Null.And.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task RelicRow_TierMarkers_ListWishlistedTiers_InCommonUncommonRareOrder()
    {
        // Common + Rare are wishlisted (Uncommon is not) → "C, R".
        var vm = await LoadedViewModelAsync(LithA1);

        Assert.That(vm.TableRows[0].TierMarkers, Is.EqualTo("C, R"));
    }

    [Test]
    public async Task RelicRow_TierMarkers_ShowsCountWhenMoreThanOneRewardPerTier()
    {
        var entry = new WishlistRelicEntry(
            "Axi A1 Relic", "Axi", IsVaulted: false,
            [
                new WishlistRelicReward("Forma Blueprint", "Common", IsWishlisted: true),
                new WishlistRelicReward("Revenant Prime Blueprint", "Rare", IsWishlisted: true),
                new WishlistRelicReward("Revenant Prime Neuroptics Blueprint", "Rare", IsWishlisted: true),
            ]);

        var vm = await LoadedViewModelAsync(entry);

        // 1 common → "C", 2 rares → "2 R"
        Assert.That(vm.TableRows[0].TierMarkers, Is.EqualTo("C, 2 R"));
    }

    [Test]
    public async Task ChildRows_OnlyWishlistedRewards_TierOrdered_WithPrefixLetter()
    {
        var vm = await LoadedViewModelAsync(LithA1);

        var children = vm.TableRows[0].Children!;
        Assert.Multiple(() =>
        {
            // Only the wishlisted Common and Rare rewards appear, Common before Rare;
            // the non-wishlisted Uncommon reward is omitted entirely (not just dimmed).
            Assert.That(children, Has.Count.EqualTo(2));
            Assert.That(children[0].DisplayName, Is.EqualTo("C Forma"));
            Assert.That(children[1].DisplayName, Is.EqualTo("R Revenant Blueprint"));
            Assert.That(children.Select(c => c.DisplayName), Has.None.Contains("Systems"));

            // No dimming — everything shown is wishlisted.
            Assert.That(children[0].IsDimmed, Is.False);
            Assert.That(children[1].IsDimmed, Is.False);

            // Child rows carry no pictogram actions.
            Assert.That(children[0].ShowsActions, Is.False);
        });
    }

    [Test]
    public async Task Rows_FollowEraSort_WithDefaultSettings()
    {
        // Default sort is Era ascending; ViewModel sorts regardless of reader input order.
        var vm = await LoadedViewModelAsync(MesoB2Vaulted, LithA1); // Meso first in reader output
        vm.ShowVaulted = true;

        Assert.That(vm.TableRows.Select(r => r.DisplayName), Is.EqualTo(new[] { "Lith A1", "Meso B2" }));
    }

    [Test]
    public async Task ShowVaulted_FiltersVaultedRows_AndVaultedRowIsDimmed()
    {
        var vm = await LoadedViewModelAsync(LithA1, MesoB2Vaulted);

        // Default: vaulted relic hidden.
        Assert.That(vm.TableRows.Select(r => r.DisplayName), Is.EqualTo(new[] { "Lith A1" }));

        vm.ShowVaulted = true;
        var meso = vm.TableRows.Single(r => r.DisplayName == "Meso B2");
        Assert.Multiple(() =>
        {
            Assert.That(meso.IsVaulted, Is.True);
            Assert.That(meso.RowOpacity, Is.EqualTo(0.45), "vaulted rows use reduced opacity");
        });
    }

    [Test]
    public async Task ChildRows_UseFullOpacity()
    {
        // Since only wishlisted rewards become child rows, none is ever dimmed.
        var vm = await LoadedViewModelAsync(LithA1);

        Assert.That(vm.TableRows[0].Children!.Select(c => c.RowOpacity), Is.All.EqualTo(1.0));
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
