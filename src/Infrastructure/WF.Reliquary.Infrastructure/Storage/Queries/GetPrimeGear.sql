SELECT
    gvs.gear_name AS GearName,
    ri.name AS ItemName,
    gvs.is_vaulted AS IsVaulted
FROM gear_vault_status gvs
JOIN reward_items ri
    ON ri.item_kind = 'PrimePart'
    AND TRIM(SUBSTR(ri.name, 1, INSTR(ri.name, ' Prime ') + 5)) = gvs.gear_name
ORDER BY gvs.gear_name, ri.name
