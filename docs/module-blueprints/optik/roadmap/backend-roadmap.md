# Backend Roadmap — Optik

| Field | Nilai |
|---|---|
| Roadmap ID | `OPT-RM-BE-001` |
| Revision | `3` — task `BE-OPT-054` (`OPT-API-v1.1`: penyaring Kepala Optik dan pratinjau harga) dari screen brief yang disetujui 2026-11-04; r2 persetujuan mitra tercatat (`OPT-DEC-050`..`055`) |
| Status | `approved` (Yoga Aji Pratama, 2026-11-03); r2 status persetujuan mitra; r3 `approved` (Yoga Aji Pratama, 2026-11-04) |
| Blueprint | `OPT-BP-001` r1, `approved` sisi Optik (Yoga Aji Pratama, 2026-11-03) |
| Cakupan | `MVP-0` s.d. `MVP-4` (`EPIC OPT-00` s.d. `OPT-11`). `POST-MVP` dicatat di bagian akhir |
| SHA baseline | BE `2a5094ca` (branch `yoga`), FE `b6dd6c304` (branch `YogaV2`) |
| Kontrak masukan | `OPT-API-v1`, `OPT-STATE-v1`, `OPT-VAL-v1`, `OPT-INT-v1`, `OPT-PERM-v1`, `OPT-PRD-v1` — `approved` sisi Optik 2026-11-03. Hash per berkas di [`blueprint-manifest.md`](../blueprint-manifest.md) bagian *Artifact hashes*. Delta tambahan `OPT-API-v1.1` (`OPT-DEC-067`, `075`, `076`, disahkan pemilik 2026-11-04) dirinci di `BE-OPT-054`; `contracts/api-contract.md` terkunci hash, penyegarannya dicatat di decision log bagian 11 |
| Masukan desain | `00-interview-decisions.md` r4, `02-backend-architecture.md` r1, `erd/data-dictionary.md`, `04-prd-to-mvp.md` bagian 10 dan 20, `testing/acceptance-test-matrix.md` |
| Owner | Yoga Aji Pratama |

> **Batas dokumen.**
> 1. Roadmap ini memecah desain menjadi task kecil, tetapi **bukan** izin menulis kode. Setiap task dikerjakan lewat `build-module-backend` dengan `TASK MODE: BACKEND` dan approval task yang eksplisit.
> 2. **QBE preflight** dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/`, bukan dari roadmap ini.
> 3. Migration dibuat di `BE-OPT-002` dan `BE-OPT-003`. **Penerapan** migration ke basis data mana pun butuh otorisasi terpisah per sesi. Bila ada migration lain yang tertunda di basis data tujuan, pakai skrip `--idempotent`, bukan `database update`.
> 4. Build backend memakai `-p:RunAnalyzers=False`. Sebelum build, server lokal BE/FE dimatikan. Uji yang menulis data memakai harness InMemory/SQLite di `Tests/` (berkas uji baru wajib `git add -f`). Uji di devYoga hanya dengan izin pemilik per sesi.
> 5. Task yang mengubah kode milik modul lain (Registrasi, Billing, Farmasi, Rekam Medis, Medical Fee, Master Data) memakai persetujuan pemiliknya yang tercatat di `OPT-EVD-002` (amendment pass 2026-11-03, `OPT-DEC-050`..`055`). Laporan task wajib menyebut penyetujunya.
> 6. Commit, push, dan deployment tetap wewenang pemilik.

## Legenda

| Tanda | Arti |
|:---:|---|
| ✅ | Prasyarat atau task selesai dan terbukti |
| ⛔ | Tertahan keputusan atau persetujuan di luar task |
| tanpa tanda | Siap dikerjakan setelah dependency selesai |

## Alur bisnis yang dibangun, per gelombang

Roadmap mengikuti `FLOW-OPT-MVP-001` di PRD. **Contoh:** Ibu Sari (samaran), frame Rp 450.000 +
lensa stok Rp 600.000, pemeriksaan Rp 75.000.

| Gelombang | Yang bisa dilakukan sesudah gelombang selesai | Contoh dengan Ibu Sari |
|---|---|---|
| `MVP-0` | Tabel Optik ada; unit, depo, katalog barang Optik, dan pengaturan tersedia | Frame merk A hitam 52-18-140 tampil di katalog Optik dengan stok 20 |
| `MVP-1` | Kunjungan Optik, pemeriksaan refraksi berversi, resep berversi + transposisi, hasil di Rekam Medis | Rina (RO) memeriksa Ibu Sari dan membuat resep OD −1,50 / OS −1,75 |
| `MVP-2` | Tarif pemeriksaan dan barang masuk tagihan; pesanan bisa ditagihkan dan dikirim ke pengerjaan dengan uang muka | Tagihan Rp 1.125.000; uang muka Rp 525.000 → pesanan dikirim, resep terkunci, stok −1 |
| `MVP-3` | Penyerahan, batal, tidak diambil, penjualan langsung; kunjungan tertutup saat pesanan selesai | Ibu Sari melunasi dan menerima kacamata; kunjungan ditutup |
| `MVP-4` | Garansi dan remake, jasa medis pemeriksa, empat laporan | Keluhan pusing → remake Rp 0 |
| Penyesuaian layar | Kepala Optik menemukan pengajuan batal dan pesanan lewat batas dari daftar; petugas melihat perkiraan harga sebelum menagih | Draf frame + lensa menampilkan perkiraan Rp 1.050.000 sebelum Tagihkan |

## Grafik Dependency

```text
✅BE-OPT-000 {Registry Opt} ─> ✅BE-OPT-002 ─┬─> BE-OPT-004 ─┬─> BE-OPT-005
                                          │               └─> ✅BE-OPT-010 ─> ✅BE-OPT-011 ─> ✅BE-OPT-012
BE-OPT-001 {Master Data ✅} ────┘
✅BE-OPT-003 {Registrasi ✅} ───────────> (melengkapi BE-OPT-010)
✅BE-OPT-013 {Rekam Medis ✅} <── ✅BE-OPT-012

✅BE-OPT-030 {Billing ✅} ─┬─> ✅BE-OPT-031 <── BE-OPT-011
                                      └─> ✅BE-OPT-032 <── BE-OPT-012 ─┐
✅BE-OPT-033 {Farmasi ✅} ─────────────> ✅BE-OPT-034 <┘

✅BE-OPT-034 ─> ✅BE-OPT-040 ─┬─> ✅BE-OPT-041 ─┬─> ✅BE-OPT-050 ─> ✅BE-OPT-052 ─> ✅BE-OPT-053
                          └─> ✅BE-OPT-042 │
✅BE-OPT-051 {Medical Fee ✅} <── BE-OPT-031  └──────────────────────────────> ✅BE-OPT-053

✅BE-OPT-032 + ✅BE-OPT-041 ─> ✅BE-OPT-054 {OPT-API-v1.1} ─> FE-OPT-020 (pratinjau harga), FE-OPT-021 (penyaring Kepala)
```

| Gelombang | Task | Boleh mulai setelah |
|---|---|---|
| `MVP-0` ✅ | `BE-OPT-000` ✅, `001` ✅, `002` ✅, `004` ✅, `005` ✅ | Registry `Opt` ✅; persetujuan Master Data/Farmasi ✅ (`OPT-EVD-002`) |
| `MVP-1` ✅ | `BE-OPT-003` ✅, `010` ✅, `011` ✅, `012` ✅, `013` ✅ | `MVP-0`; persetujuan Registrasi ✅ dan Rekam Medis ✅; konfirmasi isian visus (`n1`) untuk `011` |
| `MVP-2` ✅ | `BE-OPT-030` ✅, `031` ✅, `032` ✅, `033` ✅, `034` ✅ | `MVP-1`; persetujuan Billing ✅ dan Farmasi ✅. `030` dan `033` tidak bergantung task Optik dan boleh dikerjakan lebih awal |
| `MVP-3` ✅ | `BE-OPT-040` ✅, `041` ✅, `042` ✅ | `BE-OPT-034` |
| `MVP-4` ✅ | `BE-OPT-050` ✅, `051` ✅, `052` ✅, `053` ✅ | `BE-OPT-041`; persetujuan Medical Fee ✅ |
| Penyesuaian layar ✅ | `BE-OPT-054` ✅ | `BE-OPT-032` ✅, `041` ✅; screen brief ✅ (2026-11-04); persetujuan task r3. Tanpa kode modul lain |

**Yang tetap bisa berjalan tanpa persetujuan mitra:** begitu `BE-OPT-000` disetujui pemilik
registry, `BE-OPT-002`, `004`, `005`, `010`, `011`, `012` dapat dikerjakan seluruhnya di dalam
`OpticalManagement`. Hasilnya: pemeriksaan dan resep berjalan tanpa tagihan dan tanpa rujukan
bertaut, lalu disambungkan saat `030`/`003` disetujui.

## Ringkasan Task

| Task ID | Outcome | Epic | Dependency | Pemilik perubahan | Status |
|---|---|---|---|---|---|
| `BE-OPT-000` ✅ | Baris registry `Opt` / `OpticalManagement` | OPT-00 | — | Pemilik registry | Selesai 2026-11-03 — persetujuan Muhammad Hamzah, disampaikan Yoga Aji Pratama; checker QBE dengan kontrol negatif (`OPT-EVD-001`) |
| `BE-OPT-001` ✅ | `ServiceUnitType.Optical = 11` | OPT-00 | — | Master Data (✅ `OPT-DEC-053`, Muhammad Hamzah) | Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-001.md); seeder unit `SU-OPT-001` berjalan saat startup berikutnya |
| `BE-OPT-002` ✅ | 12 model `Opt*` + configuration + DbSet + migration `AddOpticalManagement` + enum + konstanta izin | OPT-00 | 000 | Optik | Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-002.md); migration `20261103050001_AddOpticalManagement` **belum diterapkan** |
| `BE-OPT-003` ✅ | `RegPatientEncounter.ReferralSourceEncounterId` + migration + `EncounterIntakeService` menerima rujukan | OPT-01 | — | Registrasi (✅ `OPT-DEC-052`, Muhammad Hamzah) | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-003.md); migration `20261104020212_AddEncounterReferralSource` **belum diterapkan** |
| `BE-OPT-004` ✅ | `OptNumberService`, `OptPractitionerLicenseReader`, `OptReferenceDataSeeder` | OPT-00 | 002 | Optik | Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-004.md); `OPT-OQ-017` ditutup `OPT-DEC-056` (SIP dan STR wajib) |
| `BE-OPT-005` ✅ | API data referensi: metadata, katalog barang + stok, profil barang, pengaturan + riwayat | OPT-11 | 002, 004 | Optik | Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-005.md) |
| `BE-OPT-010` ✅ | API kunjungan Optik: daftarkan lewat `EncounterIntakeService`, daftar, ringkasan | OPT-01 | 004 (+001 untuk data unit) | Optik | Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-010.md) |
| `BE-OPT-011` ✅ | API pemeriksaan refraksi: mulai, isi, final, revisi, batal (tanpa penerbitan tagihan) | OPT-02 | 010 | Optik | Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-011.md) |
| `BE-OPT-012` ✅ | API resep: buat, salin luar + transposisi, revisi, kunci, berlaku, opsi | OPT-03 | 011 | Optik | Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-012.md) |
| `BE-OPT-013` ✅ | `OptMedicalRecordDocumentSource` + dua nilai `ClinicalDocumentKind` **ditegakkan** + pendaftaran keutuhan RM per versi | OPT-09 | 011, 012 | Rekam Medis (✅ `OPT-DEC-050`, Yoga Aji Pratama) | Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-013.md) |
| `BE-OPT-030` ✅ | Billing: sumber `Optical`, kebijakan `OPTICAL` (menahan finalisasi; void selama belum final), `ResolveOpticalAsync` | OPT-08 | — | Billing (✅ `OPT-DEC-051`, Yasmina) | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-030.md); 4 berkas Billing (jembatan ikut disentuh), tanpa migration |
| `BE-OPT-031` ✅ | `OptBillingGateway` + tarif pemeriksaan sekali saat final | OPT-02, 08 | 011, 030 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-031.md); butuh konfigurasi `OpticalManagement:Billing:RefractionExamProcedureCode` |
| `BE-OPT-032` ✅ | Pesanan: draft, ubah, tagihkan per item, batal sebelum pengerjaan, status bayar + aturan sementara | OPT-04 | 012, 030, 031 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-032.md); hanya pesanan berpengerjaan (penjualan langsung di `042`) |
| `BE-OPT-033` ✅ | `DrugStockSourceDocumentTypes.OpticalOrder` + `OptStockGateway` | OPT-04 | 002 | Farmasi (✅ `OPT-DEC-054`, Ikbal) | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-033.md); kembali = Stok Masuk batch sama (`OPT-DEC-060`, konfirmasi Ikbal belum tercatat) |
| `BE-OPT-034` ✅ | Kirim ke pengerjaan (gerbang uang muka, kunci resep, potong stok) + tandai siap | OPT-04 | 032, 033 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-034.md) |
| `BE-OPT-040` ✅ | Penyerahan: lunas, identitas, snapshot garansi, pemicu finalisasi tagihan | OPT-06 | 034 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-040.md) |
| `BE-OPT-041` ✅ | Ajukan/putuskan batal sesudah pengerjaan, tutup tidak diambil, serah susulan | OPT-06 | 034, 040 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-041.md) |
| `BE-OPT-042` ✅ | Penjualan langsung + syarat resep lensa kontak | OPT-05 | 040 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-042.md) |
| `BE-OPT-050` ✅ | Keluhan garansi + remake tanpa tagihan | OPT-07 | 041, 010 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-050.md) |
| `BE-OPT-051` ✅ | Adapter Medical Fee menarik pemeriksaan Optik (RO dan Sp.M berhak) | OPT-09 | 031 | Medical Fee (✅ `OPT-DEC-055`, Yasmin) | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-051.md) |
| `BE-OPT-052` ✅ | API empat laporan | OPT-10 | 041, 050 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-052.md) |
| `BE-OPT-053` ✅ | Uji menyeluruh: privasi log, registri izin, alur penuh di harness | OPT-00..11 | 052 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-053.md) |
| `BE-OPT-054` ✅ | `OPT-API-v1.1`: dua penyaring + empat tanda baris di daftar pesanan; pratinjau harga pesanan `Draft` | OPT-04, OPT-06 | 032, 041 | Optik | Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-054.md) |

## Rincian Task

### `BE-OPT-000` ✅ — Prasyarat governance (bukan task kode)

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03. Baris `HealthServices` / `OpticalManagement / Optical` / `BUSINESS DOMAIN / MODULE` / `Opt` / `ACTIVE`, kepanjangan `Opt` = *Optical*, dan catatan perubahan lifecycle 2026-11-03 tertulis di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Persetujuan Muhammad Hamzah (pemilik registry), disampaikan Yoga Aji Pratama. Checker QBE `-Path` dua kali di pemanggilan terpisah: `OptRegistryProbe` ber-`[Table]` di `OpticalManagement` **tidak** terkena QBE-MOD-002, kontrol `ZzzRegistryProbe` di folder tak terdaftar **terkena** QBE-MOD-002 (QBE-CFG-001 pada keduanya wajar karena probe tanpa configuration). Kedua probe dihapus; `git status --short Areas/` bersih. Salinan canonical suite skill belum memuat baris ini (milik pemegang registry) |
| Outcome | Baris `HealthServices` / `OpticalManagement / Optik` / `BUSINESS DOMAIN / MODULE` / `Opt` (Optical) / `PLANNED`→`ACTIVE` di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Requirement/decision | `FR-OPT-001`; `OPT-DEC-036`; QBE-MOD-002/003, QBE-NAM-004 |
| Kontrak | Registry |
| Reuse | Prosedur pendaftaran registry (contoh baris `Mcu`) |
| Cakupan | Satu baris registry; tanpa kode |
| Dependency | Persetujuan pemilik registry |
| Acceptance criteria | (1) Baris tercatat dengan pemegang modul Yoga Aji Pratama. (2) Checker QBE tidak melaporkan QBE-MOD-002 untuk model `Opt*` ber-`[Table]` di folder `OpticalManagement`, dan kontrol negatif `Zzz*` di folder tak terdaftar tetap terkena |
| Verifikasi | Checker QBE dengan kontrol negatif |
| Risiko/pemilik | Tanpa baris ini seluruh model Optik `BLOCKED`. Pemilik registry |
| DoD | Baris tercatat + bukti checker |

### `BE-OPT-001` ✅ — Jenis unit layanan Optik

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-001.md). 6 uji lulus (harness SQLite `Tests/QuilvianSystemBackend.OpticalTests`); QBE bersih dengan kontrol negatif. Tambahan label `"Optik"` di `MedicalRecordCompletenessProfileService` dan pendaftaran seeder di `Program.cs` |
| Outcome | `ServiceUnitType.Optical = 11` di ujung enum; satu baris `MstServiceUnit` "Optik" disiapkan lewat seeder atau input Master Data |
| Requirement/decision | `FR-OPT-002`; `OPT-DEC-032` |
| Kontrak | `02-backend-architecture.md` bagian 4.1 |
| Reuse | Pola `Hemodialysis = 10` |
| Cakupan | `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs`; data unit |
| Dependency | `OPT-APR-006` |
| Acceptance criteria | (1) Nilai lama tidak bergeser (uji enum: `Hemodialysis` tetap 10). (2) Unit Optik dapat dipilih `EncounterIntakeService` |
| Verifikasi | Uji unit enum; kueri baca-saja unit di devYoga dengan izin |
| Risiko/pemilik | Penyaring berbasis jenis unit di modul lain. Pemilik Master Data |
| DoD | Kriteria terbukti; tanpa migration |

### `BE-OPT-002` ✅ — Skema Optik

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-002.md). 12 tabel, 34 index (30 sesuai DDL + 4 index FK otomatis EF), 28 FK `Restrict`; QBE Strict `PASS` dengan kontrol negatif. **Belum diterapkan** ke basis data mana pun |
| Outcome | 12 model, 12 configuration, 12 DbSet, `OpticalEnums.cs`, `OpticalPermissions.cs`, migration `AddOpticalManagement` |
| Requirement/decision | `FR-OPT-001`; seluruh keputusan bentuk data (`erd/data-dictionary.md`) |
| Kontrak | `erd/` r1, `OPT-PERM-v1` |
| Reuse | `IdentityModel`, pola `McuOrderConfiguration` (`Version` concurrency) |
| Cakupan | `Areas/HealthServices/OpticalManagement/{Models,Enums,Constants}/`, `Repositories/Configurations/HealthServices/OpticalManagement/`, `Repositories/ApplicationDbContext.cs`, `Migrations/` |
| Dependency | `BE-OPT-000` |
| Acceptance criteria | (1) Seluruh index unik dan FK `Restrict` sesuai DDL kamus data. (2) Unique terfilter `OptOrderCancellation` (`Status = 1`) dan `OptWarrantyClaim.RemakeOrderId`. (3) `OptPrescription.LockedByOrderId` tanpa FK fisik. (4) `Down` menghapus 12 tabel tanpa menyentuh tabel lain (`UAT-27`) |
| Verifikasi | Build `-p:RunAnalyzers=False`; checker QBE dengan kontrol negatif; perbandingan migration terhadap DDL |
| Risiko/pemilik | Migration belum diterapkan ke DB mana pun sampai ada izin. Optik |
| DoD | Kriteria 1–4 terbukti; laporan task |

### `BE-OPT-003` ✅ — Rujukan kunjungan asal

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-003.md). 7 uji baru lulus (total 129). Migration `20261104020212_AddEncounterReferralSource` dibuat, **belum diterapkan**. Rujukan bertaut tidak mewajibkan `IsReferral` dan hanya diterima di jalur penunjang (`Outpatient`) |
| Outcome | Kolom `ReferralSourceEncounterId` + migration `AddEncounterReferralSource`; `EncounterIntakeRequest` dan `EncounterIntakeService` menerima dan memvalidasi rujukan |
| Requirement/decision | `FR-OPT-011`; `OPT-DEC-037`, `039` |
| Kontrak | `OPT-INT-v1` `INT-OPT-01`, `INT-OPT-10` |
| Reuse | `EncounterIntakeService` |
| Cakupan | `RegistrationManagement/Models/RegPatientEncounter.cs`, `DTOS/EncounterIntakeDtos.cs`, `Services/EncounterIntakeService.cs`, configuration Registrasi, `Migrations/` |
| Dependency | `OPT-APR-008`, `010` |
| Acceptance criteria | (1) Rujukan ke kunjungan pasien yang sama diterima dan tersimpan. (2) Rujukan ke kunjungan pasien lain ditolak `400`. (3) Pemanggil lama (Lab, Rad, MCU) tidak berubah perilaku |
| Verifikasi | Uji harness jalur baru + regresi pemanggil lama |
| Risiko/pemilik | Service dipakai banyak modul. Pemilik Registrasi |
| DoD | Kriteria terbukti; migration belum diterapkan tanpa izin |

### `BE-OPT-004` ✅ — Nomor, pembaca izin, seeder

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-004.md). 11 uji baru lulus (total 17). Pengaman `42P01` agar startup tidak mati sebelum migration diterapkan |
| Outcome | `OptNumberService` (kunci `OPT-EXM`, `OPT-RX`, `OPT-ORD`, `OPT-CLM`), `OptPractitionerLicenseReader`, `OptReferenceDataSeeder` (pengaturan bawaan) |
| Requirement/decision | `OPT-DEC-012`, `040`, `041`; QBE-CODE-001..006 |
| Kontrak | `02-backend-architecture.md` bagian 4.2, 8 |
| Reuse | `NumberSeriesAllocator`, pola `McuOrderNumberService`; `WfpCredentialLicense`; `ApplicationUser.WorkforceProfileId` |
| Cakupan | `OpticalManagement/Services/`, `Seeders/`, `Program.cs` |
| Dependency | 002 |
| Acceptance criteria | (1) SIP berakhir 31-10-2026 → tidak berlaku pada 03-11-2026 WIB; berakhir 03-11-2026 → berlaku sampai 23.59 WIB (`OPT-AC-002`). (2) Nomor unik monotonik tanpa Count+1. (3) Seeder idempoten |
| Verifikasi | Uji unit perbandingan tanggal; uji alokasi nomor |
| Risiko/pemilik | Jebakan UTC 17 jam. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-005` ✅ — API data referensi

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-005.md). 5 endpoint; 16 uji baru lulus (total 33), termasuk `PermissionRegistryValidator` |
| Outcome | Grup `Opt Reference Data`: metadata, katalog (profil + stok depo Optik), profil barang, pengaturan + `OptSettingRevision` |
| Requirement/decision | `FR-OPT-110`, `111`; `OPT-DEC-009`, `031`, `040`, `041` |
| Kontrak | `OPT-API-v1`, `OPT-VAL-v1` bagian 6, `OPT-PERM-v1` |
| Reuse | `DrugStockService.GetAvailableQuantitiesAsync` (baca) |
| Cakupan | Controller, service, DTO Optik |
| Dependency | 002, 004 |
| Acceptance criteria | (1) `OPT-VAL-070`..`073`. (2) Ubah pengaturan tidak mengubah dokumen lama dan menulis riwayat lama→baru (`UAT-24`, `25`). (3) Profil hanya untuk barang yang ada |
| Verifikasi | Uji harness |
| Risiko/pemilik | Data kategori dan depo butuh `OPT-APR-005` untuk diisi. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-010` ✅ — Kunjungan Optik

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-010.md). 3 endpoint; 9 uji baru lulus (total 42). Petugas juga butuh izin Registrasi `PatientEncounter : Create`; `CompanyGuarantor` belum diterima Registrasi untuk kunjungan penunjang |
| Outcome | Grup `Opt Visit`: daftarkan (lewat `EncounterIntakeService`, kunci idempoten), daftar, ringkasan kunjungan |
| Requirement/decision | `FR-OPT-010`; `OPT-DEC-006`, `043` |
| Kontrak | `OPT-API-v1`, `OPT-INT-v1` `INT-OPT-01` |
| Reuse | `EncounterIntakeService` |
| Cakupan | `OptVisitController`, `OptVisitService`, DTO |
| Dependency | 004; data unit Optik (001) untuk uji berjalan |
| Acceptance criteria | (1) Satu kunjungan per kunci idempoten. (2) Optik tidak menulis `RegPatientEncounter` langsung. (3) `UAT-01` |
| Verifikasi | Uji harness; tinjau kode tidak ada `Set<RegPatientEncounter>().Add` di Optik |
| Risiko/pemilik | Rujukan bertaut menunggu 003. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-011` ✅ — Pemeriksaan refraksi

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-011.md). 7 endpoint; 34 uji baru lulus (total 76). Keputusan `OPT-DEC-056`..`059`. Dua kunci konfigurasi kode profesi RO/Sp.M wajib diisi sebelum dipakai |
| Outcome | Grup `Opt Refraction Exam` tanpa penerbitan tagihan |
| Requirement/decision | `FR-OPT-020`; `OPT-DEC-005`, `012`, `047` |
| Kontrak | `OPT-STATE-v1` bagian 1, `OPT-VAL-v1` bagian 1 dan 3 |
| Reuse | `OptPractitionerLicenseReader` |
| Cakupan | Controller, service, DTO pemeriksaan |
| Dependency | 010; konfirmasi isian visus (`PROPOSED` RCG `n1`) |
| Acceptance criteria | (1) `OPT-AC-001`, `002`. (2) Revisi sesudah final menambah revisi dengan alasan wajib. (3) Batal final ditolak `409`. (4) `UAT-04` |
| Verifikasi | Uji unit + harness |
| Risiko/pemilik | Kolom ukur bisa berubah bila `n1` dikoreksi. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-012` ✅ — Resep

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-012.md). 7 endpoint (termasuk `GET range-override` agar hak `OverrideRange` terdaftar); 38 uji baru lulus (total 114). Pengisian kunci resep dan naiknya `Version` saat kunci menjadi tugas task pesanan |
| Outcome | Grup `Opt Prescription` + `OptPrescriptionRules` |
| Requirement/decision | `FR-OPT-030`..`032`; `OPT-DEC-007`, `011`, `017`, `022`, `023`, `039`, `045`, `046` |
| Kontrak | `OPT-STATE-v1` bagian 2, `OPT-VAL-v1` bagian 2 |
| Reuse | — |
| Cakupan | Controller, service, DTO resep |
| Dependency | 011 |
| Acceptance criteria | (1) `OPT-AC-003`, `004`, `012`, `028`. (2) `OPT-VAL-010`..`023`. (3) `UAT-05`, `06` |
| Verifikasi | Uji unit aturan (tabel kasus rentang/kelipatan/transposisi) + harness |
| Risiko/pemilik | Keselamatan klinis: salah tanda silinder. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-013` ✅ — Pembaca Rekam Medis

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-03 — [laporan](../task/report/backend/BE-OPT-013.md). `OpticalRefraction = 45`, `OpticalPrescription = 46` ditegakkan; setiap versi pemeriksaan dan resep terdaftar sebagai dokumen RM tersendiri; 9 uji baru lulus (total 123) dan regresi Rekam Medis 225/225 |
| Outcome | `OptMedicalRecordDocumentSource` + dua nilai `ClinicalDocumentKind` |
| Requirement/decision | `FR-OPT-090`; `OPT-DEC-024` |
| Kontrak | `OPT-INT-v1` `INT-OPT-09` |
| Reuse | `IMedicalRecordDocumentSource` (pola `McuMedicalRecordDocumentSource`) |
| Cakupan | Service Optik; enum Rekam Medis; registrasi DI |
| Dependency | 012; `OPT-APR-007` (angka enum dan aturan `JenisYangDitegakkan`) |
| Acceptance criteria | (1) `OPT-AC-014`. (2) `UAT-20`. (3) Tidak ada kolom harga/bayar |
| Verifikasi | Uji harness pembaca |
| Risiko/pemilik | Pemilik Rekam Medis |
| DoD | Kriteria terbukti |

### `BE-OPT-030` ✅ — Perluasan Billing

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-030.md). 26 uji baru lulus (total 155), termasuk rantai fakta → folio → jembatan → item invoice. Cakupan bertambah satu berkas: `BillingClinicalChargeBridgeService` (pemetaan sumber, identitas butir, status dari `RuleSnapshot.milestone`). Kontrak producer untuk `031`/`032`: status barang dikirim di `RuleSnapshot.milestone` (`ORDERED`/`IN_WORK`/`READY`/`HANDED_OVER`/`NOT_COLLECTED`), pemeriksaan `ExamFinalized` |
| Outcome | Konstanta `Optical`/`OpticalExamCharge`/`OpticalItemCharge`; kebijakan `OPTICAL` di `BillingChargeSourceAdapter` (status `ORDERED`/`IN_WORK`/`READY` belum selesai; void normal dari `ORDERED`); `ResolveOpticalAsync` |
| Requirement/decision | `FR-OPT-080`, `081`; `OPT-DEC-028`, `029`, `042`, `043` |
| Kontrak | `OPT-INT-v1` `INT-OPT-02`, `04`, `05` |
| Reuse | `BillingSourceContract`, `BillingChargeSourceAdapter`, `BillingSourceTariffResolver`, `BillingFinalizationService` |
| Cakupan | Tiga berkas Billing (`02-backend-architecture.md` bagian 5) |
| Dependency | `OPT-APR-001`, `002`, `011`, `012` |
| Acceptance criteria | (1) Fakta sumber `Optical` diterima; sumber tak terdaftar tetap ditolak `BIL_SOURCE_INVALID`. (2) Item `IN_WORK` membuat `OrdersIncomplete` (`OPT-AC-025`, `UAT-18`). (3) Tarif barang dari `DrugId`, pemeriksaan dari tindakan. (4) Regresi sumber lain tidak berubah |
| Verifikasi | Uji kontrak Billing + regresi |
| Risiko/pemilik | Finalisasi otomatis tagihan modul lain. Pemilik Billing |
| DoD | Kriteria terbukti |

### `BE-OPT-031` ✅ — Tarif pemeriksaan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-031.md). 7 uji baru lulus (total 173) dengan rantai Billing sungguhan. Tindakan "Pemeriksaan Refraksi" dicari dari kode konfigurasi `OpticalManagement:Billing:RefractionExamProcedureCode` (belum diisi di `appsettings`); tanpa kode, final tetap jalan dan tarif tertunda sampai revisi berikutnya |
| Outcome | `OptBillingGateway`; final menerbitkan satu fakta `OpticalExamCharge` sesudah commit; `BillingFactEmittedAt` |
| Requirement/decision | `FR-OPT-021`; `OPT-DEC-042`, `047` |
| Kontrak | `OPT-INT-v1` `INT-OPT-04` |
| Reuse | `ClinicalMilestoneFactProducer` (pola `LabOrderService`) |
| Cakupan | Service Optik |
| Dependency | 011, 030 |
| Acceptance criteria | (1) `OPT-AC-024`. (2) Billing tidak menjawab → pemeriksaan tetap final, fakta `Pending` dikirim ulang |
| Verifikasi | Uji kontrak |
| Risiko/pemilik | Tagih ganda. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-032` ✅ — Pesanan: draft, tagihkan, batal awal, status bayar

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-032.md). 7 endpoint; 15 uji baru lulus (total 188) dengan rantai Billing sungguhan. `OPT-AC-005`/`019`/`027` dibuktikan pada angka `OptOrderPaymentGate`; penegakannya saat kirim ke pengerjaan ada di `034`. Bagian pasien per pesanan dari rincian kalkulasi Billing, terbayar per tagihan (aturan sementara) |
| Outcome | `POST/PUT orders`, `bill`, `cancel`, `payment-status`; `OptOrderPaymentGate` dengan aturan sementara |
| Requirement/decision | `FR-OPT-040`, `041`, `043`, `080`; `OPT-DEC-008`, `009`, `028`, `029`, `034`, `035`, `044` |
| Kontrak | `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` bagian 4 |
| Reuse | `ClinicalMilestoneFactProducer`; baca tagihan Billing |
| Cakupan | Controller, service, DTO pesanan |
| Dependency | 012, 030, 031 |
| Acceptance criteria | (1) `OPT-AC-005`, `006`, `018`, `019`, `027`. (2) Billing tidak menjawab → pesanan tetap `Draft` (`503`) |
| Verifikasi | Uji harness + kontrak |
| Risiko/pemilik | Angka terbayar per tagihan sampai `OPT-APR-003`. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-033` ✅ — Jenis dokumen stok Optik

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-033.md). 11 uji baru lulus (total 166). `OPT-OQ-015` ditutup `OPT-DEC-060` (Stok Masuk batch yang sama; dipilih Yoga Aji Pratama, konfirmasi Ikbal belum tercatat). Gateway tidak membuka transaksi; `034` membungkusnya |
| Outcome | `DrugStockSourceDocumentTypes.OpticalOrder`; `OptStockGateway` (kunci tetap per pesanan per item) |
| Requirement/decision | `OPT-DEC-004`, `019`, `038` |
| Kontrak | `OPT-INT-v1` `INT-OPT-07` |
| Reuse | `DrugStockService.IssueAsync`, `ReceiveBatchAsync` (pola `OperatingRoomInventoryDispatchService`) |
| Cakupan | Konstanta Farmasi; service Optik |
| Dependency | 002; `OPT-APR-005` |
| Acceptance criteria | (1) Potong dua kali dengan kunci sama → stok berkurang sekali (`OPT-AC-008`). (2) Jenis mutasi pengembalian sesuai keputusan Farmasi |
| Verifikasi | Uji harness |
| Risiko/pemilik | Pemilik Farmasi |
| DoD | Kriteria terbukti |

### `BE-OPT-034` ✅ — Kirim ke pengerjaan dan tandai siap

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-034.md). 2 endpoint; 10 uji baru lulus (total 198). Potong stok, kunci resep, dan status dalam satu transaksi; tagihan direvisi `IN_WORK`/`READY` sesudah commit. Pengiriman serentak oleh dua petugas bergantung pada token `Version` dan belum diuji serentak |
| Outcome | `send-to-work` (uang muka, tujuan, kunci resep, potong stok dalam satu transaksi), `mark-ready` |
| Requirement/decision | `FR-OPT-040`, `042`, `043`; `OPT-DEC-007`, `013`, `019` |
| Kontrak | `OPT-STATE-v1` bagian 3 |
| Reuse | `OptStockGateway`, `OptOrderPaymentGate` |
| Cakupan | Service pesanan |
| Dependency | 032, 033 |
| Acceptance criteria | (1) `OPT-AC-004`, `005`, `008`, `019`. (2) Stok kurang → status, kunci, dan stok tidak berubah. (3) `UAT-07`..`09` |
| Verifikasi | Uji harness |
| Risiko/pemilik | Atomicity. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-040` ✅ — Penyerahan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-040.md). 1 endpoint; 6 uji baru lulus (total 204). Lunas dibaca per tagihan kunjungan (aturan sementara). Pesanan terakhir terminal + lunas → Billing memfinalisasi dan kunjungan `Completed`. Pemicu yang sama dipakai `041` saat batal/tidak diambil |
| Outcome | `hand-over` (lunas, identitas, wakil, *fitting*, snapshot garansi), lalu pemicu finalisasi tagihan bila pesanan terakhir |
| Requirement/decision | `FR-OPT-060`, `081`; `OPT-DEC-041`, `043`, `048` |
| Kontrak | `OPT-STATE-v1`, `OPT-INT-v1` `INT-OPT-02` |
| Reuse | `TryCompleteSettledInvoiceAsync` (Billing) |
| Cakupan | Service pesanan, `OptBillingGateway` |
| Dependency | 034 |
| Acceptance criteria | (1) `OPT-AC-009`. (2) `UAT-12`, `13`. (3) Pesanan terakhir terminal dan lunas → kunjungan `Completed` |
| Verifikasi | Uji harness + kontrak |
| Risiko/pemilik | Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-041` ✅ — Batal sesudah pengerjaan dan tidak diambil

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-041.md). 3 endpoint; 7 uji baru lulus (total 211). Baris tagihan pesanan yang berakhir tanpa diserahkan dibawa ke `NOT_COLLECTED` dulu, karena adjustment kredit Billing tidak mengubah status baris dan baris `IN_WORK`/`READY` akan menahan finalisasi selamanya |
| Outcome | Pengajuan + keputusan batal (stok kembali, batal baris), tutup tidak diambil, serah susulan |
| Requirement/decision | `FR-OPT-061`, `062`; `OPT-DEC-010`, `018`, `038`, `049` |
| Kontrak | `OPT-STATE-v1`, `OPT-VAL-v1` `053`..`056` |
| Reuse | `OptStockGateway`, `EmitClinicalCancellationAsync` |
| Cakupan | Service pesanan |
| Dependency | 034, 040 |
| Acceptance criteria | (1) `OPT-AC-007`, `017`, `021`. (2) `UAT-14`, `15`. (3) `OPT-DEC-049` |
| Verifikasi | Uji harness + kontrak |
| Risiko/pemilik | Cara void vs adjustment ditetapkan Billing. Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-042` ✅ — Penjualan langsung

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-042.md). Tanpa endpoint baru (memakai `POST /orders`, `bill`, `hand-over`, `cancel`); 5 uji baru lulus (total 216). Lensa kacamata tidak dijual langsung; resep hanya wajib untuk lensa kontak |
| Outcome | Pesanan `DirectSale`: tagihkan → lunas → serahkan, stok dipotong saat diserahkan, lensa kontak wajib resep |
| Requirement/decision | `FR-OPT-050`; `OPT-DEC-020`, `046` |
| Kontrak | `OPT-STATE-v1` bagian 3 |
| Reuse | Seluruh jalur pesanan |
| Cakupan | Service pesanan |
| Dependency | 040 |
| Acceptance criteria | `OPT-AC-010`, `029`; `UAT-10`, `11` |
| Verifikasi | Uji harness |
| Risiko/pemilik | Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-050` ✅ — Garansi dan remake

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-050.md). 5 endpoint; 9 uji baru lulus (total 225). Remake memakai jalur kirim/siap/serah pesanan yang sama tanpa fakta tagihan. Tambahan atas kontrak: `PrescriptionVersionId?` pada keputusan untuk resep baru hasil pemeriksaan ulang |
| Outcome | Grup `Opt Warranty Claim`; remake `IsRemake` tanpa fakta tagihan |
| Requirement/decision | `FR-OPT-070`, `071`; `OPT-DEC-015`, `041`, `043` |
| Kontrak | `OPT-STATE-v1` bagian 4, `OPT-VAL-v1` bagian 5 |
| Reuse | `OptOrderService`, `OptVisitService` |
| Cakupan | Controller, service, DTO keluhan |
| Dependency | 041, 010 |
| Acceptance criteria | `OPT-AC-013`, `022`, `026`; `UAT-16`, `17` |
| Verifikasi | Uji unit garansi + harness |
| Risiko/pemilik | Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-051` ✅ — Jasa medis

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-051.md). 5 uji baru lulus (total 230). Sp.M dibayar sebagai `Doctor`, RO sebagai `Workforce`. Perbandingan tanggal tagihan pada kueri lama adapter diganti ke `DateTimeOffset` (makna sama) agar dapat diuji |
| Outcome | `MedicalFeeServiceSourceAdapter` menarik pemeriksaan Optik final |
| Requirement/decision | `FR-OPT-091`; `OPT-DEC-042` |
| Kontrak | `OPT-INT-v1` `INT-OPT-08` |
| Reuse | Adapter Medical Fee |
| Cakupan | Berkas Medical Fee |
| Dependency | 031; `OPT-APR-009` (kelayakan RO) |
| Acceptance criteria | Pemeriksaan final tampil sebagai layanan medis berpelaksana dan cocok dengan baris tagihan |
| Verifikasi | Uji harness Medical Fee |
| Risiko/pemilik | Pemilik Medical Fee |
| DoD | Kriteria terbukti |

### `BE-OPT-052` ✅ — Laporan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-052.md). 4 endpoint; 7 uji baru lulus (total 237). Penjualan = baris tagihan `OPTICAL` aktif (bruto, sebelum diskon/adjustment tagihan); cara bayar mengikuti tiga nilai kunjungan (Tunai/Asuransi/Penjamin Perusahaan) |
| Outcome | Empat laporan `Opt Report` |
| Requirement/decision | `FR-OPT-100`; `OPT-DEC-025` |
| Kontrak | `OPT-API-v1` |
| Reuse | Baris tagihan Billing (rekap penjualan) |
| Cakupan | Controller, service, DTO laporan |
| Dependency | 041, 050 |
| Acceptance criteria | `UAT-22`, `23`; angka penjualan sama dengan baris tagihan sumber `Optical` |
| Verifikasi | Uji harness dengan data samaran |
| Risiko/pemilik | Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-053` ✅ — Uji menyeluruh

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-053.md). 44 endpoint sama persis dengan matriks izin; E2E penuh lewat controller lulus; log tanpa kolom Sensitif. Total 239 uji Optik; regresi Kamar Operasi 142 dan Rekam Medis 225 lulus. **Seluruh roadmap backend Optik `MVP-0`..`MVP-4` selesai** |
| Outcome | Bukti tidak ada kolom sensitif di logger; seluruh endpoint terdaftar di registri izin dengan ejaan konstanta; alur penuh di harness |
| Requirement/decision | `NFR-001`..`007`; `OPT-PERM-v1` |
| Kontrak | Seluruh kontrak v1 |
| Reuse | Pola `BE-MCU-035` |
| Cakupan | `Tests/` |
| Dependency | 052 |
| Acceptance criteria | Baris E2E penuh `testing/acceptance-test-matrix.md` lulus di harness |
| Verifikasi | Laporan uji |
| Risiko/pemilik | Optik |
| DoD | Kriteria terbukti |

### `BE-OPT-054` ✅ — Penyaring Kepala Optik dan pratinjau harga (`OPT-API-v1.1`)

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [laporan](../task/report/backend/BE-OPT-054.md). 8 uji baru lulus (total Optik 247); registri izin 45 endpoint; penyaring, tanda, dan Tutup Tidak Diambil memakai satu fungsi batas; uji mutasi batas dan penyaring tertangkap |
| Outcome | (a) Kepala Optik menemukan pengajuan batal yang menunggu keputusan dan pesanan yang sudah boleh ditutup *Tidak Diambil* langsung dari daftar pesanan, dan setiap baris membawa tandanya. (b) Petugas melihat perkiraan harga per barang dan total pesanan `Draft` sebelum menagih |
| Requirement/decision | `OPT-DEC-067`, `075`, `076`; `OPT-DEC-018`, `010`; `OPT-OQ-019`; screen brief §5.8, §5.10, §8 |
| Kontrak | Delta tambahan `OPT-API-v1` → `v1.1`, pemanggil lama tidak terdampak: **(a)** `GET orders/`: `OptOrderListQuery` + `HasPendingCancellation: bool?` (ada pengajuan batal berstatus `Requested`) dan `PastNotCollectedLimit: bool?` (status `ReadyForPickup` dan sudah memenuhi syarat tutup *Tidak Diambil*); `OptOrderListItemResponse` + `HasPendingCancellation`, `IsPastNotCollectedLimit`, `IsOverdue`, `NeedsContact` (`bool`, aturan sama dengan penyaringnya). **(b)** `GET orders/{id}/price-preview` (`OptOrder : Read`) → `OptOrderPricePreviewResponse` { `OrderId`, `OrderNumber`, `Lines[]` { `ItemId`, `LineNo`, `DrugCode`, `DrugName`, `Quantity`, `UnitPrice?`, `LineAmount?`, `TariffAvailable`, `Message?` }, `EstimatedTotal?`, `AllTariffsAvailable` }. Hanya untuk pesanan `Draft` yang belum ditagihkan dan bukan remake; selain itu `409` *"Harga pesanan yang sudah ditagihkan dibaca dari tagihan."* Billing tak terjangkau → `503` |
| Reuse | (a) Satu sumber aturan batas hari dengan penolakan Tutup Tidak Diambil (`OptOrderService`, `NotCollectedCloseDays`) dan penyaring `Overdue`/`NeedsContact` yang ada; tanggal WIB. (b) Jalur yang sama dengan `OptBillingGateway.CheckItemTariffsAsync`: `InpServiceClassResolver` + `BillingSourceTariffResolver` (domain `Optical`, efek `OpticalItemCharge`) dengan klinik dan kelas kunjungan; baca-saja, tanpa menerbitkan fakta |
| Cakupan | `OptOrderDtos.cs`, `OptOrderService.cs` (atau service baca baru), `OptBillingGateway.cs` (metode pratinjau), `OptOrderController.cs`; uji di `Tests/QuilvianSystemBackend.OpticalTests/`; registri izin 44 → 45 endpoint (matriks `Be053EndToEndTests` ikut diperbarui). Tanpa migration, tanpa kode modul lain |
| Dependency | `BE-OPT-032` ✅, `041` ✅; screen brief ✅ |
| Acceptance criteria | (1) `HasPendingCancellation=true` hanya mengembalikan pesanan dengan pengajuan batal `Requested`; sesudah diputuskan pesanan itu tidak muncul lagi. (2) `PastNotCollectedLimit=true` memuat pesanan siap tepat di hari batas dan menolak sehari sebelumnya, **sama persis** dengan hari pertama Tutup Tidak Diambil diterima. (3) Empat tanda baris konsisten dengan penyaringnya pada data yang sama. (4) `GET orders/` tanpa parameter baru memberi hasil yang sama seperti sebelum task. (5) `OPT-AC-034`: pratinjau frame Rp 450.000 + lensa Rp 600.000 = `EstimatedTotal` Rp 1.050.000 dan sama dengan baris tagihan sesudah Tagihkan pada hari yang sama. (6) Barang tanpa tarif: `TariffAvailable=false` + pesan, `AllTariffsAvailable=false`, `EstimatedTotal` kosong, respons tetap `200`. (7) Pesanan sudah ditagihkan/bukan `Draft` → `409`; remake → `409`; Billing gagal → `503`; tanpa `OptOrder : Read` → `403`. (8) Pratinjau tidak menerbitkan fakta tagihan dan tidak mengubah `Version` pesanan |
| Verifikasi | Uji harness (rantai Billing `PesananUji`) untuk (1)–(8), termasuk uji batas hari dan uji mutasi pada batas; regresi seluruh `OpticalTests`; QBE checker `-Mode Strict -Path` atas berkas yang disentuh + kontrol negatif |
| Risiko/pemilik | Perkiraan bisa berbeda dari tagihan bila tarif berubah antara pratinjau dan Tagihkan, sehingga dilabeli *perkiraan*. Batas hari wajib memakai satu fungsi yang sama dengan validasi tutup agar tanda dan tombol tidak bertentangan. Optik |
| DoD | Kriteria (1)–(8) terbukti; laporan `task/report/backend/BE-OPT-054.md`; traceability dan roadmap diperbarui; penyegaran `contracts/api-contract.md` tercatat untuk pass blueprint |

## POST-MVP (di luar roadmap ini)

| Usulan | Pemilik | Dasar |
|---|---|---|
| Alokasi pembayaran per baris tagihan; cabut aturan sementara `OPT-DEC-035` | Billing (`OPT-APR-003`) | PRD bagian 8 |
| Persetujuan diskon per `ApproverRole` | Billing (`OPT-APR-004`) | PRD bagian 8 |
| Jalur kacamata BPJS | Insurance/Registrasi + Optik | `OPT-DEC-030` |
| Uji coba dan kontrol lensa kontak | Optik | `OPT-DEC-022` |
