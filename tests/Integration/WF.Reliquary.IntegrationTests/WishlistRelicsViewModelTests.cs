using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistRelicsViewModelTests
{
    [Test]
    public void ViewMode_DefaultsToCompact()
    {
        var vm = CreateViewModel();

        Assert.Multiple(() =>
        {
            Assert.That(vm.ViewMode, Is.EqualTo(RelicViewMode.Compact));
            Assert.That(vm.IsViewModeCompact, Is.True);
            Assert.That(vm.IsViewModeVerbatim, Is.False);
            Assert.That(vm.IsViewModeTable, Is.False);
        });
    }

    [Test]
    public void IsViewModeTable_TracksViewMode()
    {
        // The "Show all rewards" checkbox binds its visibility to !IsViewModeTable,
        // so this flag is what hides the checkbox while Table mode is active.
        var vm = CreateViewModel();

        Assert.That(vm.IsViewModeTable, Is.False, "hidden only in Table mode");

        vm.ViewMode = RelicViewMode.Table;
        Assert.That(vm.IsViewModeTable, Is.True);

        vm.ViewMode = RelicViewMode.Compact;
        Assert.That(vm.IsViewModeTable, Is.False);
    }

    [Test]
    public void ShowAllRewards_CheckedState_SurvivesTableRoundTrip()
    {
        var vm = CreateViewModel();
        vm.ShowAllRewards = true;

        // Switch to Table mode (checkbox hidden) and back to a card mode.
        vm.ViewMode = RelicViewMode.Table;
        vm.ViewMode = RelicViewMode.Compact;

        Assert.That(vm.ShowAllRewards, Is.True, "checked state must be retained while hidden");
    }

    private static WishlistRelicsViewModel CreateViewModel()
    {
        return new WishlistRelicsViewModel(
            Mock.Of<IWishlistRelicsReader>(),
            Mock.Of<IWishlistStore>());
    }
}
