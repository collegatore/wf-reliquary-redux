WITH wishlisted_relic_ids AS (
    SELECT DISTINCT rr.relic_id
    FROM wishlist_items wi
    JOIN reward_items ri ON ri.normalized_name = wi.part_normalized_name
    JOIN relic_rewards rr ON rr.reward_item_id = ri.id
    WHERE wi.wishlist_id = @wishlist_id
),
missions_with_wishlist AS (
    SELECT DISTINCT mr.mission_source_id
    FROM mission_rewards_merged mr
    WHERE mr.relic_id IN (SELECT relic_id FROM wishlisted_relic_ids)
)
-- Rotation missions: one row per rotation × wishlisted relic (LEFT JOIN keeps zero-chance rotations)
SELECT
    ms.id               AS MissionSourceId,
    ms.display_label    AS MissionDisplayLabel,
    ms.source_order     AS MissionSourceOrder,
    ms.mission_type     AS MissionType,
    ms.node             AS MissionNode,
    ms.region           AS MissionRegion,
    rot.id              AS RotationId,
    rot.display_label   AS RotationDisplayLabel,
    rot.source_order    AS RotationSourceOrder,
    rel.display_name    AS RelicName,
    mr.chance_percent   AS RelicChancePercent,
    mr.source_order     AS RewardSourceOrder
FROM missions_with_wishlist mww
JOIN mission_sources ms ON ms.id = mww.mission_source_id
JOIN reward_rotations rot ON rot.mission_source_id = ms.id
LEFT JOIN mission_rewards_merged mr
    ON mr.mission_source_id = ms.id
    AND mr.rotation_id = rot.id
    AND mr.relic_id IN (SELECT relic_id FROM wishlisted_relic_ids)
LEFT JOIN relics rel ON rel.id = mr.relic_id

UNION ALL

-- Non-rotation missions: one row per wishlisted relic in the flat pool
SELECT
    ms.id               AS MissionSourceId,
    ms.display_label    AS MissionDisplayLabel,
    ms.source_order     AS MissionSourceOrder,
    ms.mission_type     AS MissionType,
    ms.node             AS MissionNode,
    ms.region           AS MissionRegion,
    NULL                AS RotationId,
    NULL                AS RotationDisplayLabel,
    NULL                AS RotationSourceOrder,
    rel.display_name    AS RelicName,
    mr.chance_percent   AS RelicChancePercent,
    mr.source_order     AS RewardSourceOrder
FROM missions_with_wishlist mww
JOIN mission_sources ms ON ms.id = mww.mission_source_id
JOIN mission_rewards_merged mr
    ON mr.mission_source_id = ms.id
    AND mr.relic_id IN (SELECT relic_id FROM wishlisted_relic_ids)
    AND mr.rotation_id IS NULL
JOIN relics rel ON rel.id = mr.relic_id
WHERE NOT EXISTS (
    SELECT 1 FROM reward_rotations rr WHERE rr.mission_source_id = ms.id
)

ORDER BY MissionSourceOrder, RotationSourceOrder, RewardSourceOrder
