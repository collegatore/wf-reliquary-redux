UPDATE relics
SET is_vaulted = 1
WHERE id NOT IN (
    SELECT DISTINCT relic_id
    FROM mission_rewards
    WHERE relic_id IS NOT NULL
)
