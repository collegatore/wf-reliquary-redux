WITH wishlisted_relic_ids AS (
    SELECT DISTINCT rr.relic_id
    FROM wishlist_items wi
    JOIN reward_items ri ON ri.normalized_name = wi.part_normalized_name
    JOIN relic_rewards rr ON rr.reward_item_id = ri.id
    WHERE wi.wishlist_id = @wishlist_id
)
SELECT
    r.display_name AS DisplayName,
    r.era AS Era,
    r.is_vaulted AS IsVaulted,
    ri.name AS RewardName,
    rr.reward_tier AS RewardTier,
    CASE WHEN wi.part_normalized_name IS NOT NULL THEN 1 ELSE 0 END AS IsWishlisted
FROM relics r
JOIN wishlisted_relic_ids wri ON wri.relic_id = r.id
JOIN relic_rewards rr ON rr.relic_id = r.id
JOIN reward_items ri ON ri.id = rr.reward_item_id
LEFT JOIN wishlist_items wi
    ON wi.part_normalized_name = ri.normalized_name
    AND wi.wishlist_id = @wishlist_id
ORDER BY
    CASE r.era WHEN 'Lith' THEN 1 WHEN 'Meso' THEN 2 WHEN 'Neo' THEN 3 WHEN 'Axi' THEN 4 END,
    r.display_name,
    CASE rr.reward_tier WHEN 'Common' THEN 1 WHEN 'Uncommon' THEN 2 WHEN 'Rare' THEN 3 ELSE 4 END,
    rr.source_order
