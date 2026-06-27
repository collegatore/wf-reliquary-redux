## Context

This change introduces no new user-visible capabilities and modifies no existing
behavioral requirements. All work is internal maintenance:

- Upgrading a vulnerable transitive dependency (`SQLitePCLRaw.lib.e_sqlite3` 2.1.10 → 2.1.11)
- Fixing two nullable compiler warnings (CS8601, CS8602)

No spec files are created or modified as part of this change.
