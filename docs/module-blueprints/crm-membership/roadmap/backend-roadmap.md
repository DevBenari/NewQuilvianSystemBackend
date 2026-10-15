# Backend Roadmap — CRM Membership

| Field | Nilai |
|---|---|
| Roadmap ID | `CRM-RM-BE-001` |
| Revision | `1` |
| Status | `approved` (Yoga Aji Pratama, 2026-10-13) |
| Blueprint | `crm-membership` r1, `approved` (Yoga Aji Pratama, 2026-10-13, `CRM-DEC-030`) |
| Cakupan | **Terbatas** `MVP-0` s.d. `MVP-2` (`EPIC CRM-01` s.d. `CRM-07`). `EPIC CRM-08`/`09` tidak direncanakan (`DEC-CRM-001`) |
| SHA baseline | BE `1aecf642` (branch `yoga`), FE `f1e5e273a` |
| Kontrak masukan | API `v2`, state/validasi/integrasi/izin `v1`, semua `approved`. Hash per berkas di `blueprint-manifest.md#artifact_hashes` |
| Masukan desain | `02-backend-architecture.md`, `erd/data-dictionary.md`, `04-prd-to-mvp.md` §10, §20 |
| Owner | Yoga Aji Pratama |

> **Batas dokumen.**
> 1. Roadmap ini memecah desain menjadi task kecil, tetapi **bukan** izin menulis kode. Setiap task baru boleh dikerjakan lewat `build-module-backend` dengan `TASK MODE: BACKEND` dan approval task yang eksplisit.
> 2. **QBE preflight** dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/`, bukan dari roadmap ini.
> 3. Migration dibuat di task `BE-CRM-001`/`002`. **Penerapan** migration ke database mana pun butuh otorisasi terpisah per sesi.
> 4. Build backend memakai `-p:RunAnalyzers=False`. Uji yang menulis data memakai harness InMemory. Uji konkurensi memakai Npgsql lokal dengan izin.
> 5. Commit, push, dan deployment tetap wewenang pemilik.

## Legenda

| Tanda | Arti |
|:---:|---|
| ✅ | Prasyarat atau task selesai dan terbukti |
| 🟡 | Source dan migration ada, bukti berbasis database belum dijalankan |
| ⛔ | Prasyarat berupa keputusan atau gerbang di luar task belum terpenuhi |
| tanpa tanda | Siap dikerjakan setelah dependency selesai |

## Grafik Dependency

```text
✅{Registry} ─> BE-CRM-001 ✅ ─┬─> BE-CRM-002 ✅ ─┐
                           ├─> BE-CRM-003 ─┤
                           ├─> BE-CRM-010 ─┼─> BE-CRM-011 ─┬─> BE-CRM-012 ✅ ─┐
                           └─> BE-CRM-023  │               ├─> BE-CRM-013 ✅ ─┼─> BE-CRM-015 🟡 (rilis bersama FE-CRM-001)
                                           └─> BE-CRM-014 ✅│               └─> BE-CRM-020 ✅
                                               BE-CRM-021 ✅ <┘ (butuh 010)
⛔{AG-03 owner rawat-jalan} ─> BE-CRM-022 (butuh 010)
```

| Gelombang | Task | Boleh mulai setelah |
|---|---|---|
| `MVP-0` | `BE-CRM-001`, `BE-CRM-002`, `BE-CRM-003`, `BE-CRM-023` | Registry folder disetujui ✅ (2026-10-13) |
| `MVP-1` | `BE-CRM-010`, `BE-CRM-011`, `BE-CRM-012`, `BE-CRM-013`, `BE-CRM-014`, `BE-CRM-015` | `MVP-0` selesai |
| `MVP-2` | `BE-CRM-020`, `BE-CRM-021`, `BE-CRM-022` ⛔ | `MVP-1` selesai |

## Ringkasan Task

| Task ID | Outcome | Epic | Dependency | Status |
|---|---|---|---|---|
| `BE-CRM-001` ✅ | Model, enum, konfigurasi, dan migration skema membership | CRM-01 | `BE-CRM-000` ✅ | Selesai, diterapkan ke devYoga — [laporan](../task/report/backend/BE-CRM-001.md) |
| `BE-CRM-002` ✅ | Data lama menjadi Perlu Verifikasi + index unik per status | CRM-01 | 001 | Selesai, diterapkan ke devYoga — [laporan](../task/report/backend/BE-CRM-002.md) |
| `BE-CRM-003` ✅ | Nominal iuran di master tingkat | CRM-01 | 001 | Selesai — [laporan](../task/report/backend/BE-CRM-003.md) |
| `BE-CRM-023` ✅ | Respons daftar/detail/ringkasan memuat kolom dan status baru | CRM-01 | 001 | Selesai — [laporan](../task/report/backend/BE-CRM-023.md) |
| `BE-CRM-010` ✅ | Satu aturan member aktif + penulis proyeksi | CRM-05 | 001 | Selesai — [laporan](../task/report/backend/BE-CRM-010.md) |
| `BE-CRM-011` 🟡 | Daftar membership baru lewat service | CRM-02 | 002, 003, 010 | Selesai, `AT-12` paralel di PostgreSQL belum — [laporan](../task/report/backend/BE-CRM-011.md) |
| `BE-CRM-012` ✅ | Perpanjangan dan upgrade | CRM-02 | 011 | Selesai — [laporan](../task/report/backend/BE-CRM-012.md) |
| `BE-CRM-013` ✅ | Batal, verifikasi data lama, catatan, riwayat | CRM-04 | 011 | Selesai — [laporan](../task/report/backend/BE-CRM-013.md) |
| `BE-CRM-014` ✅ | Lookup pasien + status aktif pasien | CRM-02, CRM-05 | 010 | Selesai — [laporan](../task/report/backend/BE-CRM-014.md) |
| `BE-CRM-015` 🟡 | Penutupan rute kiosk, pensiun endpoint lama, `IsMember` tidak lagi diterima | CRM-06, CRM-05 | 011, 013; rilis bersama `FE-CRM-001` | Backend selesai, menunggu `FE-CRM-001` untuk rilis — [laporan](../task/report/backend/BE-CRM-015.md) |
| `BE-CRM-020` ✅ | Proses terjadwal harian | CRM-03 | 011, 012 | Selesai — [laporan](../task/report/backend/BE-CRM-020.md) |
| `BE-CRM-021` ✅ | Kiosk hanya membaca membership Aktif | CRM-05 | 010 | Selesai — [laporan](../task/report/backend/BE-CRM-021.md) |
| `BE-CRM-022` | Antrean RJ memakai resolver | CRM-05 | 010, ⛔ `AG-03` | ⛔ |

## Rincian Task

### `BE-CRM-000` — Prasyarat governance (bukan task kode)

| Field | Isi |
|---|---|
| Outcome | Baris registry `HealthServices / PatientManagement / Membership` dengan prefix `Pat`, lifecycle `ACTIVE`, tercatat di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Dasar | `QBE-MOD-003`, `QBE-NAM-002`; `02-backend-architecture.md` §7 urutan 0 |
| Pemilik | Pemilik registry (persetujuan), Yoga Aji Pratama (pengaju) |
| Status | ✅ Selesai 2026-10-13. Baris `HealthServices / PatientManagement / Membership / Patient Membership` (`Pat`, `ACTIVE`) ditambahkan beserta catatan lifecycle. Persetujuan Muhammad Hamzah (pemilik registry), disampaikan Yoga Aji Pratama. QBE checker `ReportOnly` atas berkas registry: PASS |

### `BE-CRM-001` ✅ — Skema membership

Selesai 2026-10-13 di `761bfe9d`: migration `20261013051652_ExtendPatientMembershipLifecycle`. Laporan: [`task/report/backend/BE-CRM-001.md`](../task/report/backend/BE-CRM-001.md). Diterapkan ke `QuilvianNewDevYoga` 2026-10-13.

| Field | Isi |
|---|---|
| Outcome | `MstPatientMembership` memuat 10 kolom baru, `MembershipStatus` punya 3 nilai baru dengan default `PendingPayment`, `MstMembershipTier.AnnualFeeAmount` ada, `PatMembershipStatusHistory` + konfigurasi terdaftar di `ApplicationDbContext`, dan migration `ExtendPatientMembershipLifecycle` dibuat |
| Requirement/decision | `FR-CRM-001`, `FR-CRM-002`; `CRM-DEC-004`, `012`, `013`, `017`, `021`, `023`, `024` |
| Kontrak | `erd/data-dictionary.md` r1, `contracts/state-transition-matrix.md` v1 |
| Reuse | `MstPatientMembership`, `MstPatientMembershipConfiguration`, pola `IdentityModel` |
| Cakupan | Model, 3 enum baru, konfigurasi baru dan diperbarui, satu migration. **Tanpa** index unik baru, tanpa perubahan controller |
| Dependency | `BE-CRM-000` ✅ |
| Acceptance criteria | (1) Kolom, tipe, dan bawaan persis kamus data. (2) FK self dan FK riwayat `Restrict`. (3) `Down` migration membalik semua perubahan. (4) Build `-p:RunAnalyzers=False` lulus |
| Verifikasi | Build, `dotnet ef migrations script` untuk migration ini dibandingkan DDL kamus data, dan `migrations list` menunjukkan satu migration baru tertunda |
| Risiko/pemilik | Merge lain membawa migration bersamaan, sehingga snapshot bentrok. Pemilik: implementer + Yoga |
| DoD | Kriteria 1–4 terbukti dan laporan task tertulis |

### `BE-CRM-002` ✅ — Data lama dan index unik

Migration `20261013052442_AddPatientMembershipStatusUniqueIndexes` dibuat 2026-10-13. SQL data ditaruh di migration ini (bukan di migration `BE-CRM-001`), sebelum index. Laporan: [`task/report/backend/BE-CRM-002.md`](../task/report/backend/BE-CRM-002.md). Diterapkan ke `QuilvianNewDevYoga` 2026-10-13 setelah dua uji kering; kriteria 1–3 terbukti.

| Field | Isi |
|---|---|
| Outcome | Saat migration diterapkan, semua membership lama yang tidak dihapus menjadi `NeedsVerification` (`LegacyImport`), proyeksi pasiennya kosong, satu riwayat `LEGACY_FLAGGED` per baris, lalu tiga index unik bersyarat + unik `FeeChargeReferenceId` menggantikan index `IsPrimary` |
| Requirement/decision | `FR-CRM-003`, `FR-CRM-004`; `CRM-DEC-021`, `INV-01` s.d. `INV-03` |
| Kontrak | `02-backend-architecture.md` §7 langkah 2–3, `erd/patient-membership.md` |
| Reuse | — |
| Cakupan | SQL idempoten dalam migration `ExtendPatientMembershipLifecycle` + migration `AddPatientMembershipStatusUniqueIndexes` |
| Dependency | `BE-CRM-001` |
| Acceptance criteria | (1) Menjalankan migration dua kali di DB uji tidak menggandakan riwayat. (2) Setelah migration, `INSERT` kedua berstatus 7 untuk pasien sama ditolak. (3) `Down` memulihkan status lama |
| Verifikasi | `AT-27`, `UAT-01`, `UAT-02` di DB lokal (dengan izin) |
| Risiko/pemilik | Jumlah baris lama belum diketahui (`CRM-CAP-015`). Pemilik: Yoga |
| DoD | Kriteria 1–3 terbukti dengan bukti kueri |

### `BE-CRM-003` ✅ — Iuran di master tingkat

Selesai 2026-10-13. Laporan: [`task/report/backend/BE-CRM-003.md`](../task/report/backend/BE-CRM-003.md).

| Field | Isi |
|---|---|
| Outcome | `MembershipTierController` menerima dan mengembalikan `AnnualFeeAmount` (≥ 0). Pilihan tingkat untuk pendaftaran hanya Silver/Gold/Platinum/Executive |
| Requirement/decision | `CRM-DEC-004`, `006`; `VAL-02`, `VAL-03` |
| Kontrak | `contracts/api-contract.md` v2 bagian Membership Tier |
| Reuse | `MembershipTierController`, `MembershipTierDtos` |
| Cakupan | DTO + validasi + metadata form tingkat |
| Dependency | `BE-CRM-001` |
| Acceptance criteria | Nilai negatif 400. Nilai 2.400.000 tersimpan dan terbaca kembali |
| Verifikasi | Uji endpoint lokal |
| Risiko/pemilik | Menyentuh master milik Administrator. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-CRM-023` ✅ — Respons baca diperluas

Selesai 2026-10-13. Laporan: [`task/report/backend/BE-CRM-023.md`](../task/report/backend/BE-CRM-023.md).

| Field | Isi |
|---|---|
| Outcome | `GET /admin`, `/admin/{id}`, `/admin/summary` memuat `RegistrationType`, `PeriodStartDate`, `PaymentDueAt`, `FeeAmountSnapshot`, `CancellationCategory`, `RowVersion`, hitungan per status baru, dan filter status |
| Requirement/decision | `CRM-FE-005`; `03-frontend-architecture.md` §3 |
| Kontrak | API `v2` (endpoint lama yang dipertahankan) |
| Reuse | Endpoint baca lama (legacy ratchet, tidak ditulis ulang) |
| Cakupan | DTO response + mapping + filter |
| Dependency | `BE-CRM-001` |
| Acceptance criteria | Field baru terisi. Filter `membershipStatus=7` hanya mengembalikan Menunggu Pembayaran |
| Verifikasi | Uji endpoint lokal |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `BE-CRM-010` ✅ — Resolver member aktif

Selesai 2026-10-13. Laporan: [`task/report/backend/BE-CRM-010.md`](../task/report/backend/BE-CRM-010.md).

| Field | Isi |
|---|---|
| Outcome | `PatientMembershipStatusResolver` menjawab membership Aktif pasien pada tanggal WIB tertentu, dan menulis proyeksi `MstPatient` |
| Requirement/decision | `FR-CRM-040`, `FR-CRM-041` (sisi penulis); J1; `CRM-DEC-005`; `INV-11` |
| Kontrak | `contracts/integration-contract.md` `INT-01` s.d. `INT-03` |
| Reuse | Logika tanggal `OutpatientQueueClassificationService.cs:55-104` sebagai rujukan |
| Cakupan | Service baru + registrasi DI + konfigurasi `PatientMembership:TimeZone` |
| Dependency | `BE-CRM-001` |
| Acceptance criteria | Pukul 23.30 WIB di hari terakhir: Aktif. 00.01 WIB keesokannya: Reguler. Status Menunggu/Terjadwal/Perlu Verifikasi: Reguler |
| Verifikasi | `AT-03`, `AT-09` (unit, InMemory) |
| Risiko/pemilik | Jebakan UTC-WIB. Pemilik: implementer |
| DoD | Kriteria terbukti |

### `BE-CRM-011` 🟡 — Daftar membership baru

Selesai 2026-10-13 kecuali uji paralel `AT-12` di PostgreSQL (index unik sudah terbukti di `BE-CRM-002`). Laporan: [`task/report/backend/BE-CRM-011.md`](../task/report/backend/BE-CRM-011.md).

| Field | Isi |
|---|---|
| Outcome | `POST /admin` dengan `RegisterPatientMembershipRequest` membuat Menunggu Pembayaran dengan nomor dari `NumberSeriesAllocator`, `FeeAmountSnapshot`, `PaymentDueAt`, riwayat `REGISTERED`, di bawah kunci per pasien |
| Requirement/decision | `FR-CRM-010`, `FR-CRM-011`; `CRM-DEC-003`, `004`, `006`, `012`, `023`; `VAL-01` s.d. `VAL-06` |
| Kontrak | API `v2`, validasi `v1`, izin `v1` (`PatientMembership : Create`) |
| Reuse | `NumberSeriesAllocator`, pola kunci `pg_advisory_xact_lock` dari `OutpatientQueueNumberAllocator` |
| Cakupan | `PatientMembershipService` (perintah daftar), DTO baru, ubah aksi `POST /admin`, deret nomor (seed bila allocator membutuhkannya), konfigurasi `PaymentDueDays` |
| Dependency | `BE-CRM-002`, `BE-CRM-003`, `BE-CRM-010` |
| Acceptance criteria | `AT-01`, `AT-02`, `AT-11`, `AT-12`, `AT-20`, `AT-26` lulus |
| Verifikasi | Unit InMemory + uji konkurensi Npgsql lokal (`AT-12`) |
| Risiko/pemilik | Format nomor belum final (`CRM-GAP-012`): pakai contoh `MBR-YYMM-nnnnnn` sebagai konfigurasi. Pemilik: Yoga |
| DoD | Kriteria terbukti dan laporan task tertulis |

### `BE-CRM-012` ✅ — Perpanjangan dan upgrade

Selesai 2026-10-13 di `83ef27bd`. Laporan: [`task/report/backend/BE-CRM-012.md`](../task/report/backend/BE-CRM-012.md).

| Field | Isi |
|---|---|
| Outcome | `POST /admin/{id}/renewals` dan `/upgrades` membuat Menunggu Pembayaran (`Renewal`/`Upgrade`) dengan rujukan membership sebelumnya |
| Requirement/decision | `FR-CRM-012`, `FR-CRM-013`; `CRM-DEC-012`, `024`, `029`; `VAL-05`, `VAL-07` s.d. `VAL-09` |
| Kontrak | API `v2`, state `v1` |
| Reuse | `PatientMembershipService` dari 011 |
| Cakupan | Dua perintah service + dua endpoint + konfigurasi `RenewalWindowDays` |
| Dependency | `BE-CRM-011` |
| Acceptance criteria | `AT-14`, `AT-29`, `AT-30` lulus. Perpanjangan 11 hari sebelum habis tercatat Menunggu (`Renewal`) |
| Verifikasi | Unit InMemory |
| Risiko/pemilik | Penyelesaian (lunas ke Aktif/Terjadwal) **tidak** di sini, karena milik `EPIC CRM-08` |
| DoD | Kriteria terbukti |

### `BE-CRM-013` ✅ — Batal, verifikasi, catatan, riwayat

Selesai 2026-10-13. Refund/pembatalan tagihan dicatat sebagai permintaan di riwayat sampai `DEC-CRM-001`. Laporan: [`task/report/backend/BE-CRM-013.md`](../task/report/backend/BE-CRM-013.md).

| Field | Isi |
|---|---|
| Outcome | `POST /admin/{id}/cancel`, `/verify`, `PATCH /admin/{id}/notes`, `GET /admin/{id}/status-histories` berjalan sesuai matriks status. Pembatalan Aktif ikut membatalkan Terjadwal. Permintaan refund dan pembatalan tagihan **dicatat** di riwayat sebagai menunggu S2, tanpa memanggil billing |
| Requirement/decision | `FR-CRM-030` s.d. `FR-CRM-035`; `CRM-DEC-013`, `021`, `022`, `025`, `029`; `VAL-10` s.d. `VAL-17` |
| Kontrak | API `v2`, state `v1`, izin `v1` (`Cancel`, `Verify`, `Update`, `Read`) |
| Reuse | `PatientMembershipService` |
| Cakupan | Empat perintah/kueri + endpoint + `[AccessAction]` untuk `Cancel`/`Verify` |
| Dependency | `BE-CRM-011` |
| Acceptance criteria | `AT-15`, `AT-16`, `AT-28`, `AT-35` lulus. `InputError` pada membership yang belum punya data pemakaian manfaat ditolak fail-closed (`INT-07` menunggu S2) |
| Verifikasi | Unit InMemory + uji endpoint |
| Risiko/pemilik | Refund salah input belum bisa dibuktikan sampai S2. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-CRM-014` ✅ — Lookup dan status aktif

Selesai 2026-10-13. Laporan: [`task/report/backend/BE-CRM-014.md`](../task/report/backend/BE-CRM-014.md).

| Field | Isi |
|---|---|
| Outcome | `GET /admin/patient-lookup` (No. RM/NIK/No. Telp, ≥ 3 karakter, semua hasil + status member) dan `GET /admin/patients/{patientId}/active` |
| Requirement/decision | `FR-CRM-014`; `CRM-DEC-010`; `VAL-22` |
| Kontrak | API `v2` |
| Reuse | Pola pencarian `PatientController.cs:1987-1992`, resolver |
| Cakupan | Dua endpoint baca + DTO |
| Dependency | `BE-CRM-010` |
| Acceptance criteria | `AT-18` lulus. NIK/No. Telp tidak masuk log |
| Verifikasi | Uji endpoint |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `BE-CRM-015` 🟡 — Penutupan dan pensiun endpoint

Backend selesai 2026-10-13. **Belum boleh dirilis tanpa `FE-CRM-001`.** Juga memperbaiki akar member palsu (`IsMember` sentinel). `AT-19` dan `UAT-11` belum terbukti. Laporan: [`task/report/backend/BE-CRM-015.md`](../task/report/backend/BE-CRM-015.md).

| Field | Isi |
|---|---|
| Outcome | `POST /` dan `POST /kiosk` dihapus. `PUT`, `PATCH status`, dan `DELETE` mengembalikan 410 dengan petunjuk pengganti. `PatientController` mengabaikan `IsMember`. Default `MstPatient.IsMember = false` |
| Requirement/decision | `FR-CRM-041`, `FR-CRM-050` s.d. `FR-CRM-052`; `CRM-DEC-018`, `022` |
| Kontrak | API `v2` (dampak kompatibilitas breaking) |
| Reuse | — |
| Cakupan | `PatientMembershipController`, `PatientController`, `MstPatient` |
| Dependency | `BE-CRM-011`, `BE-CRM-013`. **Rilis bersama `FE-CRM-001`**, agar halaman lama tidak putus |
| Acceptance criteria | `AT-19`, `AT-22`, `AT-23`, `AT-25` lulus. `UAT-11` lulus |
| Verifikasi | Uji endpoint dengan akun kiosk dan akun admisi |
| Risiko/pemilik | Klien lain yang belum diketahui memanggil endpoint lama. Mitigasi: grep FE sebelum merge. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-CRM-020` ✅ — Proses terjadwal

Selesai 2026-10-13. Bawaan menyala (`PatientMembership:Lifecycle:Enabled`). Laporan: [`task/report/backend/BE-CRM-020.md`](../task/report/backend/BE-CRM-020.md).

| Field | Isi |
|---|---|
| Outcome | `PatientMembershipLifecycleHostedService` berjalan saat start dan pukul 00.05 WIB: kedaluwarsa, mulai Terjadwal, batal tidak dibayar. Idempoten dan mengejar ketertinggalan |
| Requirement/decision | `FR-CRM-020` s.d. `FR-CRM-023`; `CRM-DEC-005`, `014`, `024` |
| Kontrak | State `v1`, integrasi `INT-08` |
| Reuse | Pola `KioskEncounterClosureHostedService` |
| Cakupan | Hosted service + perintah sistem di service + registrasi `Program.cs` |
| Dependency | `BE-CRM-011`, `BE-CRM-012` |
| Acceptance criteria | `AT-06`, `AT-08` lulus. Menjalankan dua kali tidak menambah riwayat. Data Terjadwal buatan (disisipkan uji) menjadi Aktif tepat pada hari H |
| Verifikasi | Unit InMemory dengan jam tiruan |
| Risiko/pemilik | Startup backend membebani DB. Proses dibatasi per batch. Pemilik: implementer |
| DoD | Kriteria terbukti |

### `BE-CRM-021` ✅ — Kiosk hanya membaca Aktif

Selesai 2026-10-13, termasuk titik baca ketiga `KioskScanSessionController`. Laporan: [`task/report/backend/BE-CRM-021.md`](../task/report/backend/BE-CRM-021.md).

| Field | Isi |
|---|---|
| Outcome | `GET /options` (kiosk) dan `KioskPatientLookupService` (nomor member) hanya mengembalikan membership berstatus Aktif dalam periode |
| Requirement/decision | `FR-CRM-043`; `CRM-CAP-012` |
| Kontrak | Integrasi `INT-03` |
| Reuse | Resolver |
| Cakupan | Dua titik baca |
| Dependency | `BE-CRM-010` |
| Acceptance criteria | Nomor member kedaluwarsa tidak ditemukan kiosk. Yang Aktif ditemukan (`AT-23`) |
| Verifikasi | Uji endpoint akun kiosk |
| Risiko/pemilik | Menyentuh service kiosk (Registration). Pemilik: Yoga, dengan pemberitahuan ke owner kiosk |
| DoD | Kriteria terbukti |

### `BE-CRM-022` ⛔ — Antrean RJ memakai resolver

| Field | Isi |
|---|---|
| Outcome | `OutpatientQueueClassificationService` memakai resolver, bukan kueri sendiri |
| Requirement/decision | `FR-CRM-042`; `AG-03`; `CRM-CAP-016` |
| Kontrak | Integrasi `INT-02` |
| Dependency | `BE-CRM-010`, ⛔ persetujuan owner rawat-jalan |
| Acceptance criteria | `AT-36` lulus. Klasifikasi antrean untuk pasien Aktif tidak berubah dibanding perilaku sekarang |
| Verifikasi | Uji klasifikasi sebelum dan sesudah |
| Risiko/pemilik | Menyentuh modul rawat-jalan. Pemilik: owner rawat-jalan |
| DoD | Kriteria terbukti dan persetujuan tercatat |
| Pekerjaan yang tetap jalan tanpa ini | Semua task lain. Antrean RJ tetap memakai aturannya sendiri yang sudah memeriksa status dan tanggal |

## Di luar Roadmap Ini

| Epic | Penyebab | Yang terjadi selama MVP |
|---|---|---|
| `EPIC CRM-08` (iuran, `ConfirmFeePaidAsync`, refund, pembatalan tagihan) | `DEC-CRM-001` | Membership baru berhenti di Menunggu Pembayaran dan batal otomatis setelah 7 hari. Member lama aktif lewat verifikasi |
| `EPIC CRM-09` (diskon di billing, snapshot IGD/rawat inap) | `DEC-CRM-001` pertanyaan 5, `AG-01` | Belum ada diskon member pada tagihan |
