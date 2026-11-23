# Inventarisasi Fisik — Requirement Traceability

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12 |
| Blueprint / contract | `AST-BP-001` rev 1 · `AST-INV-1.0.0` `approved` 2026-11-12 (`AST-DEC-051`) |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |

Bukti backend = `V-BE`; tidak adanya automated test bukan gap (`rules/backend/TEST_POLICY.md`).

| Requirement | Decision / aturan | Desain | Task BE | Task FE | Verifikasi | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `CAP-07.1` jadwal | `AST-DEC-018`; `AST-RULE-030` | state §1 | `BE-AST-020` ✅, `021` ✅ | `FE-AST-015` ✅ | `AT-INV-01`, `31` | `BE-AST-020`: 6 tabel opname, unik parsial sesi aktif per unit dan satu ketua per sesi, migration `AddAssetPhysicalInventory` diterapkan dan diperiksa di DB — [`BE-AST-020.md`](../task/report/backend/BE-AST-020.md). `BE-AST-021`: `AT-INV-01` (jadwal, tepat satu ketua), `AT-INV-31` (409 `AST-VAL-INV-007`), validasi `001..005`, `020`, `023`, `024`; runtime 45/45 — [`BE-AST-021.md`](../task/report/backend/BE-AST-021.md). `FE-AST-015`: jadwalkan/ubah/mulai/batal dari layar, menu grup Inventaris; Playwright 16/16 — [`FE-AST-015.md`](../task/report/frontend/FE-AST-015.md) | ✅ 2026-11-23 — `UAT-INV-02` milik tim UAT |
| Penahanan unit saat opname | `AST-RULE-023`; `AST-DES-008`, `031` | `INT-INV-04` | `BE-AST-021` ✅ | — | `AT-INV-03`, `30`; `UAT-INV-02`, `03` | `AT-INV-30`: mulai ditolak 409 `AST-VAL-INV-006` menyebut nomor mutasi/pinjaman penahan; `AT-INV-03`: mutasi, pinjaman (`AST-VAL-MOV-006`) dan penyerahan (`AST-VAL-REG-026`) ditolak selama sesi `Berjalan`; snapshot 44 aset termasuk yang dipinjam keluar — [`BE-AST-021.md`](../task/report/backend/BE-AST-021.md) | ✅ 2026-11-23 — `UAT-INV-02`, `03` milik tim UAT |
| `CAP-07.2` pemeriksaan fisik | `AST-DEC-010`, `018`; `AST-FE-007` (menggantikan `003`); `AST-DES-027`, `029` | flowchart 01 | `BE-AST-022` ✅ | `FE-AST-016` 🟡 | `AT-INV-04..06`, `13`, `32`, `33`, `38`; `UAT-INV-06` | `BE-AST-022`: `AT-INV-05`, `06`, `32`, `33` lulus, temuan wajib foto, runtime 37/37 — [`BE-AST-022.md`](../task/report/backend/BE-AST-022.md). `FE-AST-016`: `AT-INV-13` (kamera tiruan membaca QR), `AT-INV-38` (kamera ditolak → ketik nomor), temuan, kirim ulang; Playwright 16/16 — [`FE-AST-016.md`](../task/report/frontend/FE-AST-016.md) | 🟡 2026-11-23 — menunggu `npm run build` ulang untuk perbaikan pemindai; `UAT-INV-06` milik tim UAT |
| `CAP-07.3` pencocokan | `AST-RULE-031`; `AST-DES-028` | state §2 | `BE-AST-022` ✅ | `FE-AST-017` | `AT-INV-04` | `BE-AST-022`: `AT-INV-04` enam kategori terhitung (Sesuai 2, Sesuai Sedang Dipinjam 1, Tidak Ditemukan 39, Beda Lokasi 2, Beda Kondisi 1, Tidak Terdaftar 1), aset unit lain → Beda Lokasi (`AST-DES-028`) — [`BE-AST-022.md`](../task/report/backend/BE-AST-022.md) | 🟡 2026-11-23 — backend ✅; layar pencocokan `FE-AST-017` belum |
| `CAP-07.4` selisih | `AST-RULE-032`; `AST-RCG-G10`, `G11` | state §2 | `BE-AST-022`, `023` | `FE-AST-017` | `AT-INV-34` | — | belum |
| `CAP-07.5` verifikasi | `AST-RCG-G22`; `AST-ASM-003` | state §1 | `BE-AST-023` | `FE-AST-017` | `AT-INV-07`, `35`; `UAT-INV-04` | — | belum |
| `CAP-07.6` berita acara | `AST-DEC-046` | API 2.1 `/document` | `BE-AST-023` | `FE-AST-017` | `AT-INV-12` | — | belum |
| Tindak lanjut selisih | `AST-DEC-019`; `AST-DES-007`, `030` | flowchart 02 | `BE-AST-023` | `FE-AST-017` | `AT-INV-07..10`, `36`; `UAT-INV-05` | — | belum |
| `CAP-01.5` inventaris tertunda | `AST-DEC-031` | API 2.3 | `BE-AST-024` | `FE-AST-018` | `AT-INV-11` | — | belum |
| `CAP-11.4` laporan inventaris | — | API 2.4 | `BE-AST-024` | `FE-AST-018` | `AT-INV-11` | — | belum |

**Gap requirement → bukti:** tidak ada.
