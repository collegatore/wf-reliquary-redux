## 1. Parser Fix

- [x] 1.1 Add `isRelicReward` parameter to `AddRewardItem`; pass `true` from the relic rewards call site and `false` from the mission rewards call site
- [x] 1.2 Update `ItemKind` to accept the relic reward context flag and return `'Other'` for prime-named items when `isRelicReward` is `false`
- [x] 1.3 In `AddRewardItem`, when an item already exists as `'Other'` and is now encountered with `isRelicReward = true` and would be `'PrimePart'`, promote its `ItemKind` in the dictionary

## 2. Tests

- [x] 2.1 Add parser test: a prime-named item that appears only as a mission reward is classified `'Other'`
- [x] 2.2 Add parser test: a prime-named item that appears in both mission rewards and relic rewards is classified `'PrimePart'` regardless of encounter order
- [x] 2.3 Add parser test: a prime-named item that appears only as a relic reward is still classified `'PrimePart'`
