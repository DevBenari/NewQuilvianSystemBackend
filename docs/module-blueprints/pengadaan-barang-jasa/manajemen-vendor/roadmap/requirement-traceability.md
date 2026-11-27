# Manajemen Vendor — Traceability Requirement

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · `manajemen-vendor` (`approved`, `PBJ-DEC-062`) |
| Revisi roadmap | `1` · `draft` · 17 November 2026 |
| Kontrak | set `1.0` (`approved`) |
| SHA | BE `c6b86a6e`, FE `059b6c612` |
| Hash masukan | `../blueprint-manifest.md` |

Kolom **Bukti** merujuk `testing/acceptance-test-matrix.md` (`AT-VD-*`) dan `04-prd-to-mvp.md` (`UAT-VD-*`).

**Status task (17 November 2026):**

- `BE-PBJ-VD-001` ✅ — `AT-VD-60`, `61` (backend), `62`, `64` terbukti; laporan [BE-PBJ-VD-001](../task/report/backend/BE-PBJ-VD-001.md).
- `BE-PBJ-VD-002` ✅ — tiga master × sembilan endpoint, uji runtime 49/49 `PASS`; laporan [BE-PBJ-VD-002](../task/report/backend/BE-PBJ-VD-002.md).
- `BE-PBJ-VD-003` ✅ — daftar, detail, kategori, rekening tersamarkan, riwayat; uji runtime 25/25 `PASS`; laporan [BE-PBJ-VD-003](../task/report/backend/BE-PBJ-VD-003.md).
- `BE-PBJ-VD-004` ✅ — dokumen legalitas, verifikasi oleh petugas lain, keadaan berlaku; uji runtime 33/33 `PASS`; laporan [BE-PBJ-VD-004](../task/report/backend/BE-PBJ-VD-004.md).
- `BE-PBJ-VD-006` ✅ — batasi dan cabut Dibatasi, berakhir sendiri tanpa job; uji runtime 19/19 `PASS`; laporan [BE-PBJ-VD-006](../task/report/backend/BE-PBJ-VD-006.md).
- `BE-PBJ-VD-005` ✅ — kelayakan vendor per kategori; uji runtime 14/14 `PASS`; delta bentuk respons (INT perlu 1.1); laporan [BE-PBJ-VD-005](../task/report/backend/BE-PBJ-VD-005.md).
- `BE-PBJ-VD-008` ✅ — penilaian berkala, finalisasi oleh orang lain, revisi menggantikan; uji runtime 32/32 `PASS`; laporan [BE-PBJ-VD-008](../task/report/backend/BE-PBJ-VD-008.md).
- `BE-PBJ-VD-007` ✅ — usulan blokir/buka blokir lewat `Wfl`, Master Data terkunci; uji runtime 28/28 `PASS`; laporan [BE-PBJ-VD-007](../task/report/backend/BE-PBJ-VD-007.md).
- Seluruh task backend (8/8) selesai.
- `FE-PBJ-VD-001` ✅ — menu vendor + layar Pengaturan `-11`..`-13`; uji layar 24/24 `PASS`; laporan [FE-PBJ-VD-001](../task/report/frontend/FE-PBJ-VD-001.md).
- `FE-PBJ-VD-002` ✅ — daftar, detail, kategori, rekening, riwayat vendor; uji layar 15/15 `PASS`; laporan [FE-PBJ-VD-002](../task/report/frontend/FE-PBJ-VD-002.md). `BE-PBJ-VD-003` diperbarui (rekening di daftar, 10/10 `PASS`; [laporan](../task/report/backend/BE-PBJ-VD-003.md)).
- `FE-PBJ-VD-003` ✅ — `-04` dan tab Dokumen `-02` dengan kelayakan per kategori; lint dan build `PASS`; uji layar putaran 2 16/16 `PASS`, cek API 4/4 `PASS` (19 November 2026); laporan [FE-PBJ-VD-003](../task/report/frontend/FE-PBJ-VD-003.md).
- `FE-PBJ-VD-004` ✅ — `-06`, `-07`, tab Status `-02`, Antrean Persetujuan jenis Usulan Status Vendor, Master Data Supplier blokir baca saja; uji layar 16/16 `PASS`, cek API/DB 5/5 `PASS` (19 November 2026); `BE-PBJ-VD-003` diperbarui (D-10 `NON_ACTIVE`, 8/8 `PASS`); laporan [FE-PBJ-VD-004](../task/report/frontend/FE-PBJ-VD-004.md).
- `FE-PBJ-VD-005` ✅ — `-09`, `-10`, tab Penilaian `-02`; uji layar 16/16 `PASS`, cek API/DB 5/5 `PASS` (19 November 2026); laporan [FE-PBJ-VD-005](../task/report/frontend/FE-PBJ-VD-005.md). Seluruh 13 task roadmap revisi 1 selesai.

## 1. Functional requirement → task

| Requirement | Keputusan | Desain / kontrak | Task BE | Task FE | Bukti |
| --- | --- | --- | --- | --- | --- |
| FR-VD-001 jenis dokumen | `PBJ-ASM-001` | API §Master Data | `BE-PBJ-VD-001` ✅, `002` ✅ | `FE-PBJ-VD-001` ✅ | layar `-11` `UI-02`..`08`, `12`, `20` ✅ ([FE-PBJ-VD-001](../task/report/frontend/FE-PBJ-VD-001.md)); `AT-VD-64` ✅ (seed 5 jenis, [laporan](../task/report/backend/BE-PBJ-VD-001.md)); API jenis dokumen `DT-01`..`11` ✅ ([BE-PBJ-VD-002](../task/report/backend/BE-PBJ-VD-002.md)); `UAT-VD-01` |
| FR-VD-002 dokumen wajib bertahap | `PBJ-DEC-061` | API §Category Document Requirement | `BE-PBJ-VD-001` ✅, `002` ✅ | `FE-PBJ-VD-001` ✅ | layar `-12` `UI-09`..`11`, `21` ✅ ([FE-PBJ-VD-001](../task/report/frontend/FE-PBJ-VD-001.md)); `AT-VD-14`, `64` ✅ (kewajiban 0 baris); aktivasi bertahap `RQ-02`..`16` ✅ ([BE-PBJ-VD-002](../task/report/backend/BE-PBJ-VD-002.md)) |
| FR-VD-003 kriteria berbobot | `PBJ-DEC-050` | API §Evaluation Criterion; VAL-113 | `BE-PBJ-VD-002` ✅ | `FE-PBJ-VD-001` ✅ | layar `-13` total bobot per kategori `UI-13`..`17`, `23` ✅ ([FE-PBJ-VD-001](../task/report/frontend/FE-PBJ-VD-001.md)); `CR-01`..`14` ✅ (total bobot per kategori, `PBJ-VAL-113`; [BE-PBJ-VD-002](../task/report/backend/BE-PBJ-VD-002.md)); `AT-VD-30`; `UAT-VD-02` |
| FR-VD-010 daftar dan detail vendor | `PBJ-DEC-010`, `053` | API §Vendor | `BE-PBJ-VD-003` ✅ | `FE-PBJ-VD-002` ✅ | `AT-VD-01` ✅, `04` ✅ (API; [BE-PBJ-VD-003](../task/report/backend/BE-PBJ-VD-003.md); layar `UI-04`, `05`, `15`, [FE-PBJ-VD-002](../task/report/frontend/FE-PBJ-VD-002.md)) |
| FR-VD-011 kategori vendor | `PBJ-DEC-052` | API `PUT categories`; VAL-100 | `BE-PBJ-VD-003` ✅ | `FE-PBJ-VD-002` ✅ | `AT-VD-01` ✅, `02` ✅ ([BE-PBJ-VD-003](../task/report/backend/BE-PBJ-VD-003.md)); atur kategori tanpa muat ulang `UI-05`, data basi `UI-08`, `-03` `UI-09` ✅ ([FE-PBJ-VD-002](../task/report/frontend/FE-PBJ-VD-002.md)); `UAT-VD-03` |
| FR-VD-012 rekening tersamarkan | `PBJ-DEC-015` | PERM §6 | `BE-PBJ-VD-003` ✅ | `FE-PBJ-VD-002` ✅ | `AT-VD-03` ✅ ([BE-PBJ-VD-003](../task/report/backend/BE-PBJ-VD-003.md); daftar `R-01`..`R-10`; layar `UI-07`, `UI-10`, [FE-PBJ-VD-002](../task/report/frontend/FE-PBJ-VD-002.md)); `UAT-VD-04` |
| FR-VD-020 catat dokumen | `PBJ-DEC-045` | API §Vendor Document; VAL-101 | `BE-PBJ-VD-004` ✅ | `FE-PBJ-VD-003` ✅ | `AT-VD-10` ✅ ([BE-PBJ-VD-004](../task/report/backend/BE-PBJ-VD-004.md); layar `UI-03`, [FE-PBJ-VD-003](../task/report/frontend/FE-PBJ-VD-003.md)) |
| FR-VD-021 verifikasi orang lain | `PBJ-DEC-046` | STATE §2; VAL-102..104 | `BE-PBJ-VD-004` ✅ | `FE-PBJ-VD-003` ✅ | `AT-VD-11`..`13` ✅ (API; [BE-PBJ-VD-004](../task/report/backend/BE-PBJ-VD-004.md)); layar `AT-VD-11` `UI-08`, `AT-VD-12` `UI-04` + API `PBJ-VAL-103` `API-01`/`02`, `AT-VD-13` `UI-06`, `UI-09` ✅ ([FE-PBJ-VD-003](../task/report/frontend/FE-PBJ-VD-003.md)); `UAT-VD-05` |
| FR-VD-022 keadaan berlaku | `PBJ-ASM-002` | STATE §2 | `BE-PBJ-VD-004` ✅ | `FE-PBJ-VD-003` ✅ | `AT-VD-16` ✅ (API; [BE-PBJ-VD-004](../task/report/backend/BE-PBJ-VD-004.md); layar `UI-06`, `UI-07`, [FE-PBJ-VD-003](../task/report/frontend/FE-PBJ-VD-003.md)) |
| FR-VD-023 kelayakan per kategori | `PBJ-DEC-045`, `052`, `054`, `061` | INT §2 | `BE-PBJ-VD-005` ✅ | `FE-PBJ-VD-003` ✅ | `AT-VD-14` ✅, `15` ✅, `17` ✅, `27` ✅ ([BE-PBJ-VD-005](../task/report/backend/BE-PBJ-VD-005.md)); layar `AT-VD-14` `UI-05`, `UI-12`, `AT-VD-15` `UI-10` + `API-04` ([FE-PBJ-VD-003](../task/report/frontend/FE-PBJ-VD-003.md)); `UAT-VD-06` |
| FR-VD-030 Dibatasi | `PBJ-DEC-047` | STATE §1; VAL-105 | `BE-PBJ-VD-006` ✅ | `FE-PBJ-VD-004` ✅ | `AT-VD-20` ✅, `21` ✅ (API; [BE-PBJ-VD-006](../task/report/backend/BE-PBJ-VD-006.md)); layar `AT-VD-20` `UI-02` ([FE-PBJ-VD-004](../task/report/frontend/FE-PBJ-VD-004.md)); `UAT-VD-07` |
| FR-VD-031 usulan blokir | `PBJ-DEC-021`; `PBJ-ASM-003` | STATE §3; VAL-106, 107, 091 | `BE-PBJ-VD-007` ✅ | `FE-PBJ-VD-004` ✅ | `AT-VD-22`..`25` ✅, `28` ✅, `29` ✅ ([BE-PBJ-VD-007](../task/report/backend/BE-PBJ-VD-007.md)); layar `AT-VD-22` `UI-06`, `23` `UI-07`, `24` `UI-10`/`UI-11`, `25` `UI-09` + `API-01`, `29` `UI-12` + `API-03` ([FE-PBJ-VD-004](../task/report/frontend/FE-PBJ-VD-004.md)); `UAT-VD-08` |
| FR-VD-032 kunci Master Data | `PBJ-DEC-060` | API §Supplier | `BE-PBJ-VD-007` ✅ | `FE-PBJ-VD-004` ✅ | `AT-VD-26` ✅ (API; [BE-PBJ-VD-007](../task/report/backend/BE-PBJ-VD-007.md)); layar Master Data baca saja `UI-14` + `API-04` ([FE-PBJ-VD-004](../task/report/frontend/FE-PBJ-VD-004.md)); `UAT-VD-09` |
| FR-VD-040 penilaian per vendor × kategori × periode | `PBJ-DEC-059`, `050` | STATE §4; VAL-108, 109 | `BE-PBJ-VD-008` ✅ | `FE-PBJ-VD-005` ✅ | `AT-VD-30`..`32` ✅ ([BE-PBJ-VD-008](../task/report/backend/BE-PBJ-VD-008.md)); layar `AT-VD-30` `UI-02`, `31` `UI-04`, `32` `UI-05` + `API-03` ([FE-PBJ-VD-005](../task/report/frontend/FE-PBJ-VD-005.md)); `UAT-VD-11` |
| FR-VD-041 draf → final → revisi | `PBJ-DEC-049` | STATE §4; VAL-110..112 | `BE-PBJ-VD-008` ✅ | `FE-PBJ-VD-005` ✅ | `AT-VD-33`..`35` ✅ ([BE-PBJ-VD-008](../task/report/backend/BE-PBJ-VD-008.md)); layar `AT-VD-33` `UI-03`/`UI-07` + `API-01`/`02`, `34` `UI-06`, `35` `UI-08`/`UI-09` + `API-04` ([FE-PBJ-VD-005](../task/report/frontend/FE-PBJ-VD-005.md)); `UAT-VD-10` |
| FR-VD-042 rekomendasi hanya saran | `PBJ-DEC-048` | STATE §4 | `BE-PBJ-VD-008` ✅ | `FE-PBJ-VD-005` ✅ | `AT-VD-36` ✅ ([BE-PBJ-VD-008](../task/report/backend/BE-PBJ-VD-008.md)); layar `UI-11` + `API-05` ([FE-PBJ-VD-005](../task/report/frontend/FE-PBJ-VD-005.md)) |
| FR-VD-050 riwayat vendor | `PBJ-ASM-004`, `PBJ-DEC-060` | PERM §5 | `BE-PBJ-VD-003` ✅ (+ setiap task aksi) | `FE-PBJ-VD-002` ✅ | `AT-VD-01` ✅ (`V-17`, [BE-PBJ-VD-003](../task/report/backend/BE-PBJ-VD-003.md)), `13`, `24`; `UAT-VD-12` |
| Standar layar daftar | `PBJ-FE-004` | FE §4.6 | — | semua task FE | `AT-VD-63` ✅ untuk `-01`, `-03`, `-05`, `-08` (`UI-02`, `09`, `10`, `11`, [FE-PBJ-VD-002](../task/report/frontend/FE-PBJ-VD-002.md)) dan `-11`..`-13` (`UI-24`, [FE-PBJ-VD-001](../task/report/frontend/FE-PBJ-VD-001.md)) dan `-04` (`UI-01`, [FE-PBJ-VD-003](../task/report/frontend/FE-PBJ-VD-003.md)) dan `-06` (`UI-01`, `UI-13`, [FE-PBJ-VD-004](../task/report/frontend/FE-PBJ-VD-004.md)) dan `-09` (`UI-01`, [FE-PBJ-VD-005](../task/report/frontend/FE-PBJ-VD-005.md)) |
| Registry dan konformitas | `PBJ-DEC-063` | Arsitektur §5 | `BE-PBJ-VD-001` ✅ | — | `AT-VD-60` ✅ (QBE Strict `PASS`) |
| Build dan skema | — | Kamus data | semua task BE | semua task FE | `AT-VD-61`, `62` — ✅ untuk `BE-PBJ-VD-001` (9 tabel cocok kamus data); `AT-VD-61` frontend ✅ untuk `FE-PBJ-VD-001` ([FE-PBJ-VD-001](../task/report/frontend/FE-PBJ-VD-001.md)) |

## 2. Keputusan → task

| Keputusan | Task |
| --- | --- |
| `PBJ-DEC-010`, `053` | `BE-PBJ-VD-003` ✅, `FE-PBJ-VD-002` ✅ |
| `PBJ-DEC-015` | `BE-PBJ-VD-003` ✅, `FE-PBJ-VD-002` ✅ |
| `PBJ-DEC-021`, `PBJ-ASM-003` | `BE-PBJ-VD-007` ✅, `FE-PBJ-VD-004` ✅ |
| `PBJ-DEC-044` | `BE-PBJ-VD-008` ✅ (`SourceType = MANUAL`); `S-V7` tidak masuk roadmap ini |
| `PBJ-DEC-045`, `046`, `PBJ-ASM-001`, `002` | `BE-PBJ-VD-004` ✅, `005` ✅, `FE-PBJ-VD-003` ✅ |
| `PBJ-DEC-047` | `BE-PBJ-VD-006` ✅, `FE-PBJ-VD-004` ✅ |
| `PBJ-DEC-048`, `049`, `050`, `059` | `BE-PBJ-VD-008` ✅, `FE-PBJ-VD-005` ✅ |
| `PBJ-DEC-051` | Bundel peran (`PERM` §3) — semua task; uji aktor di `FE-PBJ-VD-002` |
| `PBJ-DEC-052`, `054` | `BE-PBJ-VD-003` ✅, `005` ✅ |
| `PBJ-DEC-060` | `BE-PBJ-VD-007` ✅, `FE-PBJ-VD-004` ✅ |
| `PBJ-DEC-061` | `BE-PBJ-VD-001` ✅, `002` ✅, `005` ✅ |
| `PBJ-DEC-063` | `BE-PBJ-VD-001` ✅ |

## 3. Gap traceability

| Gap | Keterangan | Tindakan |
| --- | --- | --- |
| `PBJ-OQ-033` | Nama submenu menu 10 | `FE-PBJ-VD-001` memakai label sementara; ganti setelah nama SOURCE tersedia |
| `PBJ-OQ-019`, `PBJ-OQ-034` | Nilai awal dari SOP | Seeder memakai nilai awal; uji approval memakai matriks uji dev |
| `S-V7` | Evaluasi per kontrak | Di luar roadmap ini; menunggu `kontrak-pelaksanaan` |
| Requirement tanpa task | Tidak ada | — |
