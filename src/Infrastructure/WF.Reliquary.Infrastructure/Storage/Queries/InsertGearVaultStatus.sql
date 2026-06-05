WITH gear_names AS (
    SELECT DISTINCT
        TRIM(SUBSTR(name, 1, INSTR(name, ' Prime ') + 5)) AS gear_name
    FROM reward_items
    WHERE item_kind = 'PrimePart'
),
obtainable_gears AS (
    SELECT DISTINCT
        TRIM(SUBSTR(ri.name, 1, INSTR(ri.name, ' Prime ') + 5)) AS gear_name
    FROM reward_items ri
    JOIN relic_rewards rr ON rr.reward_item_id = ri.id
    JOIN mission_rewards mr ON mr.relic_id = rr.relic_id
    WHERE ri.item_kind = 'PrimePart'
)
INSERT INTO gear_vault_status (gear_name, is_vaulted)
SELECT
    g.gear_name,
    CASE WHEN o.gear_name IS NULL THEN 1 ELSE 0 END
FROM gear_names g
LEFT JOIN obtainable_gears o ON o.gear_name = g.gear_name
