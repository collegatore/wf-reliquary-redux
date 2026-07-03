using NUnit.Framework;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistRelicCardViewModelCompactTests
{
    [Test]
    public void Rewards_IsFlatList_NoTierGrouping()
    {
        // Compact mode renders WishlistRelicCardViewModel.Rewards directly (no tier
        // label rows) — this is the flat, tier-ordered list the Compact card template
        // binds to, distinct from the grouped TierGroups Verbatim mode uses.
        var card = new WishlistRelicCardViewModel(
            "Axi A15 Relic",
            "Axi",
            isVaulted: false,
            rewards:
            [
                new WishlistRelicRewardViewModel("Revenant Prime Blueprint", "Rare", isWishlisted: true, showAllRewards: true),
                new WishlistRelicRewardViewModel("Forma Blueprint", "Common", isWishlisted: false, showAllRewards: true),
            ]);

        Assert.That(card.Rewards, Has.Count.EqualTo(2));
        Assert.That(card.Rewards[0].Tier, Is.EqualTo("Common"), "ordered Common before Rare");
        Assert.That(card.Rewards[1].Tier, Is.EqualTo("Rare"));
    }

    [Test]
    public void CompactRelicName_IsClipped()
    {
        var card = new WishlistRelicCardViewModel(
            "Axi A15 Relic",
            "Axi",
            isVaulted: false,
            rewards: []);

        Assert.That(card.CompactRelicName, Is.EqualTo("Axi A15"));
    }

    [Test]
    public void Reward_TierLetter_IsPresent_ForEachTier()
    {
        var common = new WishlistRelicRewardViewModel("Forma Blueprint", "Common", isWishlisted: true, showAllRewards: false);
        var uncommon = new WishlistRelicRewardViewModel("Revenant Prime Systems Blueprint", "Uncommon", isWishlisted: true, showAllRewards: false);
        var rare = new WishlistRelicRewardViewModel("Revenant Prime Blueprint", "Rare", isWishlisted: true, showAllRewards: false);

        Assert.Multiple(() =>
        {
            Assert.That(common.TierLetter, Is.EqualTo("C"));
            Assert.That(uncommon.TierLetter, Is.EqualTo("U"));
            Assert.That(rare.TierLetter, Is.EqualTo("R"));
        });
    }

    [Test]
    public void ShowAllRewards_Dimming_AppliesToNonWishlistedRewards()
    {
        var wishlisted = new WishlistRelicRewardViewModel("Forma Blueprint", "Common", isWishlisted: true, showAllRewards: true);
        var notWishlisted = new WishlistRelicRewardViewModel("Revenant Prime Blueprint", "Rare", isWishlisted: false, showAllRewards: true);

        Assert.Multiple(() =>
        {
            Assert.That(wishlisted.IsDimmed, Is.False, "wishlisted rewards render at full strength");
            Assert.That(notWishlisted.IsDimmed, Is.True, "non-wishlisted rewards render dimmed");
        });
    }
}
