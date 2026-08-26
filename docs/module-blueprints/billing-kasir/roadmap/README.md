# Roadmap Delivery — Billing dan Kasir

Blueprint `BIL-CASH-001 revision 0.4` telah disetujui pada 20 Agustus 2026. Roadmap revision `1` ini berstatus `DRAFT_FORWARD_TEST`: urutan dan task sudah dapat ditinjau, tetapi **belum memberi wewenang menulis source**. Setiap builder hanya boleh menjalankan satu task yang kemudian disetujui secara eksplisit.

## Fase

| Phase | Outcome | Backend | Frontend | Dependency | Status |
| --- | --- | --- | --- | --- | --- |
| `BKC-PH-001` | Fondasi dapat diuji | `BE-BKC-001` | — | Blueprint approved | `READY_FOR_TASK_APPROVAL` |
| `BKC-PH-002` | Policy finansial dapat dikelola | `BE-BKC-002`–`004` | `FE-BKC-002` | PH-001 | `PLANNED` |
| `BKC-PH-003` | Charge menjadi running invoice yang benar | `BE-BKC-005`–`008` | `FE-BKC-001`,`003`,`004` | PH-001/002 | `PLANNED` |
| `BKC-PH-004` | Deposit dan split payment berjalan | `BE-BKC-009`–`011` | `FE-BKC-005`,`006` | PH-003 | `PLANNED` |
| `BKC-PH-005` | Shift dan exception finansial terkontrol | `BE-BKC-012`–`014` | `FE-BKC-007`,`008` | PH-004 | `PLANNED` |
| `BKC-PH-006` | Finalisasi menghasilkan AR/AP idempotent | `BE-BKC-015`,`016` | `FE-BKC-009` | PH-003–005 | `PLANNED` |
| `BKC-PH-007` | Bukti lintas-slice dan hardening lengkap | `BE-BKC-017` | `FE-BKC-010` | Semua slice | `PLANNED` |

## Aturan eksekusi

1. Task backend dan frontend tetap terpisah.
2. Migration **boleh digenerasikan** hanya bila disebut dalam scope task backend yang disetujui. Migration tidak boleh dijalankan ke database tanpa otorisasi terpisah.
3. Setiap backend task menjalankan QBE preflight dari `AGENTS.md`, engineering contract, registry prefix, dan aturan `.codex` yang berlaku saat eksekusi.
4. Frontend task menunggu governance frontend tersedia; pilihan visual yang tidak mengubah kontrak tetap `DEV_DISCRETION`.
5. Task berstatus `DONE` hanya setelah bukti acceptance yang ditetapkan benar-benar tersedia.

Dokumen: [backend](./backend-roadmap.md), [frontend](./frontend-roadmap.md), dan [traceability](./requirement-traceability.md).

