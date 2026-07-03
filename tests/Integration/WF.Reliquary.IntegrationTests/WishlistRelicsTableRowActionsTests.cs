using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistRelicsTableRowActionsTests
{
    private static WishlistRelicEntry LithA1 { get; } = new(
        "Lith A1 Relic", "Lith", IsVaulted: false,
        [
            new WishlistRelicReward("Forma Blueprint", "Common", IsWishlisted: true),
            new WishlistRelicReward("Revenant Prime Blueprint", "Rare", IsWishlisted: false),
        ]);

    private static WishlistRelicEntry MesoB2 { get; } = new(
        "Meso B2 Relic", "Meso", IsVaulted: false,
        [
            new WishlistRelicReward("Mag Prime Blueprint", "Common", IsWishlisted: true),
        ]);

    [Test]
    public async Task ShowRewardsCommand_OpensModal_WithAllRewardsCardForThatRelic()
    {
        var vm = await LoadedViewModelAsync(LithA1, MesoB2);
        WishlistRelicCardViewModel? opened = null;
        vm.OpenRewardsModal = card =>
        {
            opened = card;
            return Task.CompletedTask;
        };

        var mesoRow = vm.TableRows.Single(r => r.DisplayName == "Meso B2");
        mesoRow.ShowRewardsCommand!.Execute(null);

        Assert.That(opened, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(opened!.RelicName, Is.EqualTo("Meso B2 Relic"), "opens the modal for the row's own relic, not another");
            Assert.That(opened.Rewards, Has.Count.EqualTo(1), "all-rewards card, not filtered by wishlist status");
        });
    }

    [Test]
    public async Task ShowRewardsCommand_ForDifferentRows_OpensDifferentRelics()
    {
        var vm = await LoadedViewModelAsync(LithA1, MesoB2);
        var seen = new List<string>();
        vm.OpenRewardsModal = card =>
        {
            seen.Add(card.RelicName);
            return Task.CompletedTask;
        };

        vm.TableRows[0].ShowRewardsCommand!.Execute(null);
        vm.TableRows[1].ShowRewardsCommand!.Execute(null);

        Assert.That(seen, Is.EqualTo(new[] { "Lith A1 Relic", "Meso B2 Relic" }));
    }

    [Test]
    public async Task ComposeRecruitingMessageCommand_OpensRecruitingMessageComposer_ForThatRelic()
    {
        var vm = await LoadedViewModelAsync(LithA1, MesoB2);
        string? composedFor = null;
        vm.OpenRecruitingMessageCompose = relicName =>
        {
            composedFor = relicName;
            return Task.CompletedTask;
        };

        var mesoRow = vm.TableRows.Single(r => r.DisplayName == "Meso B2");
        mesoRow.ComposeRecruitingMessageCommand!.Execute(null);

        Assert.That(composedFor, Is.EqualTo("Meso B2 Relic"));
    }

    [Test]
    public async Task InvokingRowActions_LeavesTableRowsUnchanged()
    {
        var vm = await LoadedViewModelAsync(LithA1, MesoB2);
        vm.OpenRewardsModal = _ => Task.CompletedTask;
        vm.OpenRecruitingMessageCompose = _ => Task.CompletedTask;

        var rowsBefore = vm.TableRows.ToList();

        vm.TableRows[0].ShowRewardsCommand!.Execute(null);
        vm.TableRows[0].ComposeRecruitingMessageCommand!.Execute(null);

        Assert.That(vm.TableRows, Is.EqualTo(rowsBefore), "opening/closing a modal must not rebuild or reorder table rows");
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
