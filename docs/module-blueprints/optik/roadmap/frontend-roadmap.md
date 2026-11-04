# Frontend Roadmap — Optik

| Field | Nilai |
|---|---|
| Roadmap ID | `OPT-RM-FE-001` |
| Revision | `1` |
| Status | `approved` (Yoga Aji Pratama, 2026-11-03) |
| Blueprint | `OPT-BP-001` r1, `approved` sisi Optik (Yoga Aji Pratama, 2026-11-03) |
| Cakupan | `MVP-0` s.d. `MVP-4` — kebutuhan layar `FE-OPT-N01`..`N15` di `03-frontend-architecture.md` |
| SHA baseline | FE `b6dd6c304` (branch `YogaV2`), BE `2a5094ca` (branch `yoga`) |
| Kontrak masukan | `OPT-API-v1`, `OPT-PERM-v1`, `OPT-VAL-v1`, `OPT-STATE-v1` — `approved` sisi Optik, ber-hash di [`blueprint-manifest.md`](../blueprint-manifest.md). **Kerja paralel dengan backend diizinkan hanya untuk lapisan data dan aturan bersama**, karena kontraknya approved dan terkunci hash |
| Masukan desain | `03-frontend-architecture.md` r1, `04-prd-to-mvp.md` bagian 18 (UAT) |
| Owner | Yoga Aji Pratama |

> **Batas dokumen.**
> 1. Bukan izin menulis kode. Setiap task dikerjakan lewat `build-module-frontend` dengan approval task eksplisit.
> 2. **Menu, route, tab/modal, dan tata letak belum ditetapkan.** Semua task layar tertahan `FE-OPT-000` (screen brief, `OPT-FE-001`/`002`). Warna, ikon, dan jarak adalah `DEV_DISCRETION` (`OPT-FE-004`).
> 3. Isian ukur desimal **tidak** memakai `type: "number"` mentah; pakai pola `DECIMAL_FIELD_PROPS`/`parseDecimal` (`OPT-FE-003`).
> 4. Uji unit: `node --import ./tests/helpers/register.mjs --test tests/unit/`. E2E memakai pola spec API-tiruan standalone dengan output ke scratchpad; predikat cegatan wajib memuat `/v1/`.
> 5. Folder mengikuti pola modul yang ada: `src/lib/services/health-services/optical-management/`, `src/lib/state/slice/health-services/optical-management/`, `src/lib/constants/health-services/optical-management/`, `src/utils/health-services/optical-management/`, `src/components/view/health-services/optical-management/`. Route final mengikuti screen brief.

## Grafik Dependency

```text
FE-OPT-001 (aturan bersama, mandiri)
FE-OPT-002 (data klinis) ─┐                ⛔FE-OPT-000 {screen brief}
FE-OPT-003 (data pesanan) ┘                       │
                                                  ▼
FE-OPT-000 + 001 + 002 + BE-OPT-005 ─> FE-OPT-031 (katalog & pengaturan)
FE-OPT-000 + 002 + BE-OPT-010 ─> FE-OPT-010 (menu + kunjungan) ─┬─> FE-OPT-011 (pemeriksaan, + BE-OPT-011)
                                                                └─> FE-OPT-012 (resep, + BE-OPT-012)
FE-OPT-012 + 003 + BE-OPT-032/034 ─> FE-OPT-020 (pesanan) ─> FE-OPT-021 (serah/batal) ─┬─> FE-OPT-022 (penjualan langsung)
                                                                                    └─> FE-OPT-030 (garansi, + BE-OPT-050)
FE-OPT-021 + BE-OPT-052 ─> FE-OPT-032 (laporan) ─> FE-OPT-033 (E2E)
```

## Ringkasan Task

| Task ID | Outcome | Layar | Dependency | Status |
|---|---|---|---|---|
| `FE-OPT-000` ⛔ | Screen brief Optik disetujui: menu, daftar layar, route, pembagian daftar/detail, tab/modal | Semua | Pemilik (`OPT-FE-001`, `002`) | Tertahan — belum disusun |
| `FE-OPT-001` ✅ | Aturan bersama teruji unit: input desimal bertanda minus, pratinjau transposisi, cermin aturan rentang/kelipatan | — | — | Selesai 2026-11-03 — [laporan](../task/report/frontend/FE-OPT-001.md); belum di-commit. Format rupiah/tanggal WIB dipindah ke `FE-OPT-003` (dipakai bersama status bayar) |
| `FE-OPT-002` | Lapisan data klinis: service Axios + slice Redux untuk kunjungan, pemeriksaan, resep, metadata | — | Kontrak v1 | Direncanakan |
| `FE-OPT-003` | Lapisan data pesanan: pesanan, status bayar, keluhan, katalog, pengaturan, laporan, diskon Billing; format rupiah dan tanggal WIB (dipindah dari `FE-OPT-001`) | — | Kontrak v1 | Direncanakan |
| `FE-OPT-010` ⛔ | Menu Optik + daftar kunjungan + pendaftaran + ringkasan kunjungan | `N01`..`N03` | 000, 002, `BE-OPT-010` | Tertahan 000 |
| `FE-OPT-011` ⛔ | Form pemeriksaan refraksi + riwayat revisi | `N04`, `N06` | 010, 001, `BE-OPT-011` | Tertahan 000 |
| `FE-OPT-012` ⛔ | Form resep + salin resep luar + riwayat versi | `N05`, `N06` | 010, 001, `BE-OPT-012` | Tertahan 000 |
| `FE-OPT-020` ⛔ | Buat pesanan, katalog, tagihkan, ajukan diskon, status bayar, kirim ke pengerjaan | `N07`, `N08` | 012, 003, `BE-OPT-032`, `034` | Tertahan 000 (+ BE `030`/`033`) |
| `FE-OPT-021` ⛔ | Daftar pesanan, tandai siap, serahkan, ajukan/putuskan batal, tutup tidak diambil | `N09`..`N11` | 020, `BE-OPT-040`, `041` | Tertahan 000 |
| `FE-OPT-022` ⛔ | Penjualan langsung | `N07` (mode langsung) | 021, `BE-OPT-042` | Tertahan 000 |
| `FE-OPT-030` ⛔ | Keluhan garansi: catat, periksa ulang, putuskan | `N12` | 021, `BE-OPT-050` | Tertahan 000 |
| `FE-OPT-031` ⛔ | Katalog/profil barang dan pengaturan (Admin Optik) | `N13`, `N14` | 000, 001, 003, `BE-OPT-005` | Tertahan 000 |
| `FE-OPT-032` ⛔ | Empat laporan | `N15` | 021, `BE-OPT-052` | Tertahan 000 |
| `FE-OPT-033` | E2E per UAT `UAT-01`..`26` | Semua | 032 | Direncanakan |

## Rincian Task

### `FE-OPT-000` ⛔ — Screen brief

| Field | Isi |
|---|---|
| Outcome | `05-screen-brief.md` disetujui: menu (letak di `menu-items.jsx`, `requiredPermission`), daftar layar, route, daftar/detail, tab/modal, isi kolom daftar |
| Requirement/decision | `OPT-DEC-026`; `OPT-FE-001`, `002` |
| Kontrak | `03-frontend-architecture.md` bagian 2 |
| Dependency | Keputusan pemilik |
| Acceptance criteria | Brief berstatus `approved` dengan ruang `DEV_DISCRETION` yang tertulis |
| Risiko/pemilik | Tanpa brief, layar tidak boleh dibangun. Yoga Aji Pratama |
| DoD | Brief approved |

### `FE-OPT-001` ✅ — Aturan bersama

| Field | Isi |
|---|---|
| Outcome | Utilitas: parse/format desimal bertanda minus dengan kelipatan 0,25; pratinjau transposisi (+1,00 / +0,50 × 90 → +1,50 / −0,50 × 180); cermin `OPT-VAL-010`..`015`, `018`; label status dari metadata |
| Requirement/decision | `OPT-FE-003`; `OPT-DEC-045` |
| Kontrak | `OPT-VAL-v1` |
| Reuse | `DECIMAL_FIELD_PROPS`, `parseDecimal` |
| Dependency | — |
| Acceptance criteria | Uji unit: −1,50 tetap −1,50; −1,30 ditandai salah; transposisi sesuai contoh; AXIS wajib bila CYL ≠ 0 |
| Verifikasi | `node --import ./tests/helpers/register.mjs --test tests/unit/` |
| Risiko/pemilik | Aturan di layar hanya cermin; backend tetap penentu. Optik |
| DoD | Uji unit lulus |

### `FE-OPT-002` / `FE-OPT-003` — Lapisan data

| Field | Isi |
|---|---|
| Outcome | Service Axios per grup `[Tags]` dan slice Redux; header `Idempotency-Key` per niat aksi; `ExpectedVersion` dikirim; galat `400/403/409/503` dipetakan ke pesan backend |
| Requirement/decision | `OPT-API-v1`; `03-frontend-architecture.md` bagian 4 |
| Kontrak | `OPT-API-v1` (hash terkunci) |
| Dependency | — (paralel dengan backend) |
| Acceptance criteria | Uji unit service/slice dengan API tiruan: klik ganda memakai kunci yang sama; `409` memicu muat ulang |
| Verifikasi | Uji unit |
| Risiko/pemilik | Perubahan kontrak menaikkan versi dan memicu penyesuaian. Optik |
| DoD | Uji unit lulus |

### `FE-OPT-010`..`032` ⛔ — Layar

Setiap task layar memakai bentuk yang sama:

| Field | Isi |
|---|---|
| Outcome | Layar sesuai kebutuhan `FE-OPT-Nxx` dan screen brief |
| Requirement/decision | Lihat `requirement-traceability.md` |
| Kontrak | `OPT-API-v1`, `OPT-STATE-v1` (tombol per status), `OPT-PERM-v1` (`requiredPermission`) |
| Dependency | `FE-OPT-000` + task data + task backend yang disebut di ringkasan |
| Acceptance criteria | Aturan bagian 3 `03-frontend-architecture.md` terpenuhi; UAT terkait lulus di E2E API-tiruan |
| Verifikasi | Uji unit komponen bila ada logika; E2E standalone |
| Risiko/pemilik | Layar tidak boleh menghitung uang atau status sendiri. Optik |
| DoD | UAT terkait lulus; tidak ada data sensitif di `console` |

### `FE-OPT-033` — E2E

| Field | Isi |
|---|---|
| Outcome | Spec E2E untuk `UAT-01`..`26` (jalur berhasil dan gagal) |
| Dependency | 032 |
| Acceptance criteria | Seluruh UAT yang punya layar lulus; UAT backend-saja dirujuk ke laporan BE |
| Verifikasi | Laporan E2E; output di scratchpad |
| DoD | Laporan lengkap |
