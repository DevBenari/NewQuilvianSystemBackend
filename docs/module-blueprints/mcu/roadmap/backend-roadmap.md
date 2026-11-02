# Backend Roadmap — MCU (Medical Check-Up)

| Field | Nilai |
|---|---|
| Roadmap ID | `MCU-RM-BE-001` |
| Revision | `1` |
| Status | `approved` (Yoga Aji Pratama, 2026-10-21) |
| Blueprint | `MCU-BP-001` r1, `approved` (Yoga Aji Pratama, 2026-10-21) |
| Cakupan | `MVP-0` s.d. `MVP-3` (`EPIC MCU-01` s.d. `MCU-06`). `EPIC MCU-07` s.d. `MCU-11` di luar roadmap ini (bagian terakhir) |
| SHA baseline | BE `4d3e4f98` (branch `yoga`), FE `7fb1e1d3e` (branch `YogaV2`) |
| Kontrak masukan | `MCU-API-v1`, `MCU-STATE-v1`, `MCU-VAL-v1`, `MCU-INT-v1`, `MCU-PERM-v1` — seluruhnya `approved` 2026-10-21. Hash per berkas di [`blueprint-manifest.md`](../blueprint-manifest.md) bagian *Artifact hashes* |
| Masukan desain | `00-interview-decisions.md` r5, `02-backend-architecture.md` r1, `erd/data-dictionary.md`, `04-prd-to-mvp.md` §10 dan §20, `testing/acceptance-test-matrix.md`, `evidence/2026-10-21-persetujuan-dan-pembuktian.md` (`MCU-EVD-002`) |
| Owner | Yoga Aji Pratama |

> **Batas dokumen.**
> 1. Roadmap ini memecah desain menjadi task kecil, tetapi **bukan** izin menulis kode. Setiap task baru boleh dikerjakan lewat `build-module-backend` dengan `TASK MODE: BACKEND` dan approval task yang eksplisit.
> 2. **QBE preflight** dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/`, bukan dari roadmap ini.
> 3. Migration dibuat di `BE-MCU-002` dan `BE-MCU-003`. **Penerapan** migration ke basis data mana pun butuh otorisasi terpisah per sesi; bila ada migration lain yang tertunda di basis data tujuan, pakai skrip idempotent, bukan `database update`.
> 4. Build backend memakai `-p:RunAnalyzers=False`. Uji yang menulis data memakai harness InMemory/SQLite di `Tests/` (berkas uji baru wajib `git add -f`). Uji berjalan di devYoga hanya dengan izin pemilik per sesi.
> 5. Task yang mengubah service milik modul lain (Registrasi, Lab, Rad, Billing) mencantumkan pemiliknya. Persetujuan Registrasi, Lab, Rad, dan Billing (Yasmina) sudah tercatat (`MCU-EVD-002` bagian 1 dan 4).
> 6. Commit, push, dan deployment tetap wewenang pemilik.

## Legenda

| Tanda | Arti |
|:---:|---|
| ✅ | Prasyarat atau task selesai dan terbukti |
| ⛔ | Tertahan keputusan atau persetujuan di luar task |
| tanpa tanda | Siap dikerjakan setelah dependency selesai |

## Alur bisnis yang dibangun, per gelombang

Roadmap mengikuti satu perjalanan peserta (`FLOW-MCU-MVP-001` di PRD). **Contoh:** Budi, peserta
perorangan, Paket Basic Rp 350.000 (Darah Lengkap, Gula Darah Puasa, Thorax, Pemeriksaan Fisik).

| Gelombang | Yang bisa dilakukan pengguna sesudah gelombang selesai | Contoh dengan Budi |
|---|---|---|
| `MVP-0` | Admin MCU menyusun paket, harga perusahaan, jenis Pemeriksaan MCU, kategori kelayakan, kuota harian, dan dokter jaga | Paket Basic tersedia; 27 Okt buka dengan kuota 30, dokter jaga dr. Rina |
| `MVP-1` | Admisi membooking, menjadwal ulang, membatalkan, dan meng-check-in peserta; peserta yang tidak datang otomatis *Tidak Hadir* | 23 Okt Budi dibooking (1/30). 27 Okt check-in → kunjungan MCU dan satu baris tagihan Rp 350.000 |
| `MVP-2` | Pemeriksaan berjalan: pesanan Lab/Rad bertanda paket tanpa tagihan ganda, isi Pemeriksaan MCU, tambah/batal komponen, batal penuh | Budi lunas → pesanan Lab & Rad lahir; tagihan tetap Rp 350.000 sesudah spesimen diterima |
| `MVP-3` | Dokter menyusun dan menyetujui resume, petugas memverifikasi dan menyerahkan hasil | Resume *Fit dengan catatan* disetujui dr. Rina, diverifikasi Dewi, diserahkan lewat WhatsApp |

## Grafik Dependency

```text
✅{Registry Mcu} ─> BE-MCU-002 ─┬─> BE-MCU-003 ─> BE-MCU-020
                              ├─> BE-MCU-004 ─────────────┐
                              ├─> BE-MCU-005 ─┬─> 006     │
                              │               └─> 007 ────┤
                              └─> BE-MCU-010 ─────────────┴─> BE-MCU-011 ─┬─> BE-MCU-012
BE-MCU-001 (Registrasi, tanpa dependency) ─────────────────────────────────┴─> BE-MCU-013 ─> BE-MCU-014

BE-MCU-013 + BE-MCU-020 ─> BE-MCU-021 ─┬─> BE-MCU-022 ─> BE-MCU-023
                                      ├─> BE-MCU-025
                                      └─> BE-MCU-024 <── BE-MCU-026 (Billing)

BE-MCU-022 + BE-MCU-023 ─> BE-MCU-030 ─┬─> BE-MCU-031 ──────────┐
                                      ├─> BE-MCU-032 (+ 020)   │
                                      └─> BE-MCU-033 ──────────┴─> BE-MCU-034 ─> BE-MCU-035
```

| Gelombang | Task | Boleh mulai setelah |
|---|---|---|
| `MVP-0` | `BE-MCU-001`, `BE-MCU-002`, `BE-MCU-003`, `BE-MCU-004`, `BE-MCU-005`, `BE-MCU-006`, `BE-MCU-007` | Registry `Mcu` ✅ (2026-10-21), blueprint approved ✅, persetujuan Lab/Rad ✅ dan Registrasi ✅ |
| `MVP-1` | `BE-MCU-010`, `BE-MCU-011`, `BE-MCU-012`, `BE-MCU-013`, `BE-MCU-014` | `MVP-0` selesai |
| `MVP-2` | `BE-MCU-020`, `BE-MCU-021`, `BE-MCU-022`, `BE-MCU-023`, `BE-MCU-024`, `BE-MCU-025`, `BE-MCU-026` | `MVP-1` selesai. `BE-MCU-026` (Billing) tidak bergantung pada task MCU mana pun dan boleh dikerjakan lebih awal |
| `MVP-3` | `BE-MCU-030`, `BE-MCU-031`, `BE-MCU-032`, `BE-MCU-033`, `BE-MCU-034`, `BE-MCU-035` | `BE-MCU-021`..`023` selesai (resume butuh komponen dan isian) |

`BE-MCU-001` sengaja ditempatkan paling awal walau sisi pemakainya baru di `MVP-1`: keputusan
`MCU-DEC-028` mewajibkan bukti berjalan pembuatan kunjungan MCU dan skrining perawatnya
(`MCU-UNK-01`, `MCU-UNK-05`) dikumpulkan pada **task backend pertama**. Task ini tidak bergantung pada
tabel MCU, sehingga dapat berjalan sejajar dengan `BE-MCU-002`.

## Ringkasan Task

| Task ID | Outcome | Epic | Dependency | Pemilik perubahan | Status |
|---|---|---|---|---|---|
| `BE-MCU-000` ✅ | Baris registry `Mcu` / `ACTIVE` | MCU-01 | — | Pemilik registry | Selesai 2026-10-21 |
| `BE-MCU-001` ✅ | Kunjungan `MedicalCheckup` berantrean skrining lewat `EncounterIntakeService` + bukti berjalan | MCU-03 | — | Registrasi (disetujui) | Selesai 2026-10-21 — [laporan](../task/report/backend/BE-MCU-001.md) |
| `BE-MCU-002` ✅ | Skema 21 tabel MCU + migration 1 | MCU-01 | 000 | MCU | Selesai 2026-10-21 — [laporan](../task/report/backend/BE-MCU-002.md); migration belum diterapkan |
| `BE-MCU-003` ✅ | Kolom `CoveringMcuOrderId` pada `LabOrder`/`RadOrder` + migration 2 | MCU-01 | 002 | Lab, Rad (disetujui) | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-003.md); migration belum diterapkan |
| `BE-MCU-004` ✅ | Seri nomor order/impor + seeder master usulan | MCU-01 | 002 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-004.md); master usulan belum diratifikasi |
| `BE-MCU-005` ✅ | API paket, komponen, persyaratan, harga perusahaan + konstanta izin MCU | MCU-01 | 002 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-005.md) |
| `BE-MCU-006` ✅ | API jenis Pemeriksaan MCU dan kategori kelayakan | MCU-01 | 002, 005 (konstanta izin) | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-006.md) |
| `BE-MCU-007` ✅ | API jadwal harian, kuota, pelampauan, dokter jaga + pakai/lepas kuota | MCU-01 | 002, 005 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-007.md) |
| `BE-MCU-010` ✅ | Daftar Pasien MCU, ringkasan, detail, riwayat, metadata penyaring | MCU-02 | 002 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-010.md) |
| `BE-MCU-011` ✅ | Booking, jadwal ulang, batal sebelum datang | MCU-02 | 004, 005, 007, 010 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-011.md) |
| `BE-MCU-012` ✅ | Tutup hari: *Terjadwal* → *Tidak Hadir* | MCU-02 | 011 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-012.md) |
| `BE-MCU-013` ✅ | Check-in admisi dalam satu transaksi | MCU-03 | 001, 011 | MCU (+ Registrasi dan Billing ikut transaksi pemanggil — keputusan 2026-10-22) | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-013.md) |
| `BE-MCU-014` ✅ | *Walk-in* admisi | MCU-03 | 013 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-014.md) |
| `BE-MCU-020` ✅ | Lab/Rad: pesanan bertanda paket, dokter MCU penanggung jawab, tagihan Lab/Rad ditahan | MCU-04 | 003 | Lab, Rad (disetujui) | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-020.md) |
| `BE-MCU-021` ✅ | Mulai pemeriksaan: gerbang lunas/terjamin + pesanan Lab/Rad bertanda | MCU-04 | 013, 020 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-021.md) |
| `BE-MCU-022` ✅ | Komponen: daftar, sinkron status, tambah di luar paket, jadwal ulang komponen | MCU-04 | 021 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-022.md) |
| `BE-MCU-023` ✅ | Isi Pemeriksaan MCU + kunci skrining | MCU-04 | 022, 001 | MCU | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-023.md) |
| `BE-MCU-024` ✅ | Batal komponen + refund sebesar nilai pengembalian | MCU-04 | 021, 026 | MCU (+ Lab: cabut permintaan — keputusan 2026-10-23) | Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-024.md); uji devYoga menunggu migration |
| `BE-MCU-025` ✅ | Batal penuh sebelum komponen dimulai | MCU-04 | 021 | MCU (+ Billing dan Registrasi ikut transaksi pemanggil — keputusan 2026-10-22) | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-025.md) |
| `BE-MCU-026` ✅ | Perluasan refund Billing: nominal sebagian, akumulasi per baris, tolak melebihi nilai baris | MCU-04 | — (persetujuan Yasmina ✅) | Billing | Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-026.md); migration belum diterapkan |
| `BE-MCU-030` ✅ | Resume: draf, approve, kembalikan, verifikasi | MCU-05 | 022, 023 | MCU | Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-030.md) |
| `BE-MCU-031` ✅ | Buka ulang resume, versi, revisi nilai isian | MCU-05 | 030 | MCU | Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-031.md) |
| `BE-MCU-032` ✅ | Ganti dokter MCU + pindah tanggung jawab pesanan terbuka | MCU-05 | 020, 030 | MCU (+ Lab/Rad) | Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-032.md) |
| `BE-MCU-033` ✅ | Dokumen hasil lengkap dan ringkasan kelayakan | MCU-06 | 030 | MCU | Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-033.md) |
| `BE-MCU-034` ✅ | Persetujuan pelepasan, catat penyerahan, pemberitahuan koreksi | MCU-06 | 031, 033 | MCU | Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-034.md) |
| `BE-MCU-035` ✅ | Uji menyeluruh privasi log dan registri izin MCU | MCU-06 | 034 | MCU | Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-035.md) |

## Rincian Task

### `BE-MCU-000` ✅ — Prasyarat governance (bukan task kode)

| Field | Isi |
|---|---|
| Outcome | Baris `HealthServices` / `MedicalCheckupManagement / Medical Check-Up` / `BUSINESS DOMAIN / MODULE` / `Mcu` / `ACTIVE` di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Dasar | `MCU-GOV-001`, `FR-MCU-001`, QBE-MOD-002/003 |
| Status | ✅ Selesai 2026-10-21. Persetujuan Muhammad Hamzah, disampaikan Yoga Aji Pratama. Checker QBE terbukti tidak melaporkan QBE-MOD-002 untuk model `Mcu*` ber-`[Table]`, sedangkan kontrol di folder tak terdaftar terkena (`MCU-EVD-002` bagian 1) |

### `BE-MCU-001` ✅ — Kunjungan `MedicalCheckup` lewat `EncounterIntakeService` + bukti berjalan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-21 — [laporan](../task/report/backend/BE-MCU-001.md). 17 uji harness lulus; `AT-MCU-RUN-01`..`03` `PASS` di devYoga (`ENC-RSMMC-00183`, antrean `M002`). Delta: request juga membawa `DoctorId`. Tiga butir terbuka untuk `BE-MCU-013`: transaksi pemanggil, `CompanyGuarantor`, sumber registrasi/kelas pasien |
| Outcome | `EncounterIntakeService.RegisterAsync` dapat membuat kunjungan `EncounterType = MedicalCheckup` di klinik MCU, dengan antrean dan skrining perawat ditentukan master klinik (`IsQueueRequired`/`IsScreeningRequired`), bukan selalu `Outpatient` tanpa antrean |
| Requirement/decision | `FR-MCU-021` (bagian kunjungan); `MCU-DEC-003`, `023`, `028`; `MCU-COORD-004`; `MCU-UNK-01`, `05`, `06` |
| Kontrak | `MCU-API-v1` bagian *Perubahan internal* (Registrasi — kontrak publik tidak berubah); `MCU-INT-v1` |
| Reuse | `EncounterIntakeService`, `RegPatientEncounter`, antrean `RegQueue`, nurse station `start-screening`/`finish-screening` |
| Cakupan | `Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs`, `.../DTOS/EncounterIntakeDtos.cs`; uji di `Tests/`. Tidak menyentuh tabel MCU |
| Dependency | — (persetujuan Registrasi ✅ 2026-10-21) |
| Acceptance criteria | (1) Pemanggilan dengan jenis `MedicalCheckup` dan klinik MCU membuat tepat satu kunjungan `MedicalCheckup` beserta antrean skrining. (2) Pemanggil lama (`Outpatient`) tidak berubah perilaku. (3) `AT-MCU-RUN-03`: kueri baca-saja membuktikan klinik/unit MCU ada dan `IsQueueRequired = true` — bila tidak ada, task berhenti dan melapor (master milik Master Data). (4) `AT-MCU-RUN-01`: satu kunjungan uji di devYoga. (5) `AT-MCU-RUN-02`: perawat menyelesaikan skrining lewat nurse station dan status *skrining selesai* dapat dibaca dari antrean/kunjungan |
| Verifikasi | Uji integrasi harness (jalur baru + regresi jalur `Outpatient`); kueri baca-saja Npgsql; satu penulisan devYoga **dengan izin pemilik per sesi** (`MCU-DEC-028`) |
| Risiko/pemilik | Mengubah service yang dipakai modul lain (rawat jalan, kiosk). Pemilik: Registrasi. **Contoh risiko:** perubahan default jenis kunjungan membuat kunjungan poli dari kiosk ikut berantrean skrining — dicegah oleh uji regresi (2) |
| DoD | Kelima kriteria terbukti; hasil RUN dicatat di laporan task; `MCU-UNK-01`/`05`/`06` dapat ditutup |

### `BE-MCU-002` ✅ — Skema 21 tabel MCU

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-21 — [laporan](../task/report/backend/BE-MCU-002.md). Migration `20261021073245_AddMedicalCheckupManagement`: 21 tabel, 19 index unik sesuai DDL, 66 FK `Restrict`; QBE `PASS` dengan kontrol negatif. **Belum diterapkan** ke basis data mana pun |
| Outcome | 21 model `Mcu*`, enum `MedicalCheckupEnums`, 21 configuration, 21 `DbSet` jamak, dan migration `AddMedicalCheckupManagement` — termasuk `McuDailySchedule.DutyMcuDoctorId` (`MCU-DEC-042`) |
| Requirement/decision | `FR-MCU-001`; `MCU-DEC-003`, `007`, `012`, `015`, `020`, `025`, `031`..`042` |
| Kontrak | `erd/data-dictionary.md` (kolom, index, FK, DDL); `MCU-STATE-v1` (nilai enum status) |
| Reuse | `BaseModel`, pola configuration Lab/Rad, `HasConversion<int>()` untuk enum |
| Cakupan | `Areas/HealthServices/MedicalCheckupManagement/Models/`, `/Enums/`, `Repositories/Configurations/HealthServices/MedicalCheckupManagement/`, `ApplicationDbContext.cs`, satu migration |
| Dependency | `BE-MCU-000` ✅ |
| Acceptance criteria | (1) Skrip SQL migration offline cocok dengan DDL kamus data — nama tabel, kolom wajib/boleh kosong, unique filtered index (`WHERE "IsDelete" = false`), FK `Restrict`. (2) Tidak ada kolom `SortOrder` (isian memakai `FieldNumber`). (3) Checker QBE `PASS` untuk seluruh berkas model/configuration baru. (4) `Down()` menghapus 21 tabel dan tidak menyentuh tabel lain |
| Verifikasi | `dotnet ef migrations script <sebelumnya> AddMedicalCheckupManagement --no-build` (offline); checker QBE `-Path`; build `-p:RunAnalyzers=False` |
| Risiko/pemilik | Migration besar. Penerapan ke basis data butuh izin terpisah. Pemilik: Yoga |
| DoD | Kriteria terbukti; skrip SQL terlampir di laporan; migration belum diterapkan kecuali diizinkan |

### `BE-MCU-003` ✅ — Kolom `CoveringMcuOrderId` pada pesanan Lab dan Rad

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-003.md). Migration `20261022021727_AddCoveringMcuOrderToLabRadOrders`: dua kolom `uuid NULL` tanpa nilai bawaan, dua index, dua FK `Restrict` — SQL offline sama persis dengan DDL kamus; uji MCU 24 dan Radiologi 106 lulus; QBE `PASS` dengan kontrol negatif. **Belum diterapkan** ke basis data mana pun |
| Outcome | `LabOrder` dan `RadOrder` memiliki kolom `CoveringMcuOrderId` (boleh kosong, index, FK `Restrict` ke `McuOrder`) beserta migration `AddCoveringMcuOrderToLabRadOrders`. **Belum** ada perubahan perilaku |
| Requirement/decision | `MCU-DEC-005`, `022`; `MCU-COORD-002` ✅ |
| Kontrak | `02-backend-architecture.md` §5.4, §7, §8 urutan 2 |
| Reuse | Model dan configuration Lab/Rad yang ada |
| Cakupan | `LabOrder.cs`, `LabOrderConfiguration.cs`, `RadOrder.cs`, `RadOrderConfiguration.cs`, satu migration |
| Dependency | `BE-MCU-002` (FK ke `McuOrder`) |
| Acceptance criteria | (1) Seluruh baris lama bernilai `NULL` dan tidak ditulis ulang. (2) Tidak ada endpoint Lab/Rad yang menerima kolom ini dari luar. (3) Uji Lab/Rad yang ada tetap lulus |
| Verifikasi | Skrip SQL offline; uji regresi Lab/Rad (`Tests/QuilvianSystemBackend.RadiologyTests` dan uji Lab yang ada) |
| Risiko/pemilik | Menyentuh tabel modul lain. Pemilik: Lab dan Rad (disetujui 2026-10-21) |
| DoD | Kriteria terbukti; migration belum diterapkan kecuali diizinkan |

### `BE-MCU-004` ✅ — Seri nomor dan master usulan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-004.md). `McuOrderNumberService` (deret `MCU-ORDER`/`MCU-IMPORT` lewat `NumberSeriesAllocator`) dan `McuReferenceDataSeeder` (4 kategori, 2 jenis, 6 isian — seluruhnya bertanda usulan); uji MCU 33 lulus; QBE `PASS` dengan kontrol negatif. Seeder baru jalan di basis data yang sudah menerima migration `BE-MCU-002` |
| Outcome | Seri `MCU-ORDER` (format `MCU-RSMMC-000001`, tanpa reset) dan `MCU-IMPORT` lewat `NumberSeriesAllocator`; `McuOrderNumberService`; `McuReferenceDataSeeder` idempotent yang mengisi kategori kelayakan usulan (*Fit*, *Fit dengan catatan*, *Unfit sementara*, *Unfit*) dan jenis Pemeriksaan MCU usulan (*Pemeriksaan Fisik*, *Tanda Vital & Antropometri*) |
| Requirement/decision | `02-backend-architecture.md` §9; `MCU-OQ-002`, `MCU-OQ-003` (nilai **usulan**) |
| Kontrak | `erd/data-dictionary.md`; QBE-CODE-001..006 |
| Reuse | `NumberSeriesAllocator` (modul Platform `Num`), pola seeder Lab |
| Cakupan | `MCU/Services/McuOrderNumberService.cs`, `MCU/Seeders/McuReferenceDataSeeder.cs`, registrasi seeder |
| Dependency | `BE-MCU-002` |
| Acceptance criteria | (1) Dua order berurutan memperoleh `MCU-RSMMC-000001` lalu `000002`. (2) Seeder dijalankan dua kali tidak menggandakan data dan tidak menimpa isian yang sudah diubah admin. (3) Setiap master usulan bertanda belum diratifikasi pada keterangannya |
| Verifikasi | Uji unit nomor; uji seeder dua kali di harness |
| Risiko/pemilik | Nilai master klinis belum diratifikasi dokter MCU — **tidak boleh go-live** sebelum `MCU-OQ-002`/`003` terjawab. Pemilik: Yoga + dokter MCU |
| DoD | Kriteria terbukti |

### `BE-MCU-005` ✅ — API paket MCU

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-005.md). 9 endpoint grup `Mcu Package`, `McuPackageService`, `MedicalCheckupPermissions` (8 resource); `AT-MCU-01`..`03` dan `VAL-MCU-01`..`08` terbukti di harness; `PermissionRegistryValidator` tanpa temuan; uji MCU 63 lulus. Swagger berjalan belum dibuka (startup menunggu migration MCU) |
| Outcome | Grup Swagger `Health Services / Medical Checkup Management / Mcu Package` lengkap (9 endpoint), `McuPackageService`, dan `Constants/MedicalCheckupPermissions.cs` untuk seluruh resource MCU |
| Requirement/decision | `FR-MCU-002`, `FR-MCU-003`; `MCU-DEC-007`, `015`, `020`, `025`; `MCU-DEC-041` |
| Kontrak | `MCU-API-v1` grup Mcu Package; `VAL-MCU-01`..`08`; `MCU-PERM-v1` (`McuPackage : Read/Create/Update`) |
| Reuse | `MstTariff` (`IsPackageTariff`), `MstProcedure`, `MstRadModality`, `MstCompanyGuarantor`, `PagedResult<T>`, `FilterMetadataResponse` |
| Cakupan | Model sudah ada dari 002; service, DTO, controller, registrasi DI |
| Dependency | `BE-MCU-002` |
| Acceptance criteria | `AT-MCU-01`, `AT-MCU-02`, `AT-MCU-03` lulus. **Contoh:** Paket Basic Rp 350.000 dengan nilai pengembalian DL Rp 100.000 + GDP Rp 40.000 + Thorax Rp 250.000 = Rp 390.000 → `400` *"Total nilai pengembalian Rp 390.000 melebihi harga paket Rp 350.000."*; paket tidak tersimpan. `GET options?companyGuarantorId&date` mengembalikan harga yang berlaku pada tanggal itu. Controller tanpa `<summary>` XML; `filters/metadata` dideklarasikan tepat sesudah konstruktor |
| Verifikasi | Uji unit service; `PermissionRegistryValidator` lulus |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti; Swagger menampilkan grup dengan nama persis |

### `BE-MCU-006` ✅ — API data referensi

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-006.md). 7 endpoint grup `Mcu Reference Data`, `McuReferenceDataService`; ketiga kriteria terbukti di harness; `PermissionRegistryValidator` tanpa temuan; uji MCU 79 lulus |
| Outcome | Grup `Health Services / Medical Checkup Management / Mcu Reference Data`: jenis Pemeriksaan MCU beserta isian ber-nomor butir, dan kategori kelayakan |
| Requirement/decision | `FR-MCU-004`; `MCU-DEC-007`; `MCU-DEC-041` |
| Kontrak | `MCU-API-v1` grup Mcu Reference Data; `VAL-MCU-09`; `MCU-PERM-v1` (`McuReferenceData`) |
| Reuse | Pola master Lab |
| Cakupan | `McuReferenceDataService`, DTO, controller |
| Dependency | `BE-MCU-002`, `BE-MCU-005` (konstanta izin) |
| Acceptance criteria | (1) Isian `FieldType = Option` tanpa daftar pilihan → `400` *"Isian pilihan wajib berisi daftar pilihan."* (2) Isian ber-`FieldNumber` tampil urut nomor butir. (3) Kategori kelayakan dapat dinonaktifkan tanpa menghapus |
| Verifikasi | Uji unit |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-007` ✅ — API jadwal, kuota, pelampauan, dokter jaga

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-007.md). 3 endpoint grup `Mcu Daily Schedule`; `McuScheduleService.ReserveAsync`/`ReleaseAsync` ikut transaksi pemanggil, dijaga `Version` + coba ulang; keempat kriteria dan `AT-MCU-04`..`06` (tingkat service) terbukti di harness; uji MCU 95 lulus. Penguncian baris PostgreSQL belum dibuktikan |
| Outcome | Grup `Health Services / Medical Checkup Management / Mcu Daily Schedule`; `McuScheduleService` dengan metode **pakai kuota** dan **lepas kuota** yang ikut transaksi pemanggil dan dijaga token konkurensi `Version` |
| Requirement/decision | `FR-MCU-005`, `FR-MCU-006`; `MCU-DEC-012`, `029`, `037`, `042`; `MCU-DEC-041` |
| Kontrak | `MCU-API-v1` grup Mcu Daily Schedule; `VAL-MCU-10`..`15`; `MCU-PERM-v1` (`McuSchedule : Read/Update/ApproveQuotaOverride`) |
| Reuse | `MstDoctor` untuk dokter jaga |
| Cakupan | Service, DTO, controller |
| Dependency | `BE-MCU-002`, `BE-MCU-005` |
| Acceptance criteria | (1) Kuota baru di bawah terpakai → `409` *"Sudah ada 26 peserta terjadwal; kuota tidak boleh di bawah 26."* (2) Pelampauan oleh kepala unit beralasan tercatat; pengguna tanpa `ApproveQuotaOverride` → `403` (`AT-MCU-06` bagian izin). (3) Dua pemakaian kuota bersamaan atas sisa terakhir → tepat satu berhasil (`AT-MCU-05`, diuji di sini pada tingkat service). (4) `PUT` rentang tanggal menyimpan `dutyMcuDoctorId`; `GET` mengembalikan nama dokter jaga |
| Verifikasi | Uji unit + uji konkurensi dua `DbContext` di harness. Atomik sungguhan milik PostgreSQL — dicatat apa adanya bila InMemory tidak bertransaksi |
| Risiko/pemilik | Kuota adalah titik balapan utama modul. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-010` ✅ — Daftar Pasien MCU dan detail order

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-010.md). 5 endpoint baca grup `Mcu Order` lewat `McuOrderQueryService`; keempat kriteria terbukti di harness berbaris master induk; uji MCU 112 lulus |
| Outcome | Grup `Mcu Order` bagian baca: `GET filters/metadata`, `GET summary`, `GET /`, `GET {id}`, `GET {id}/history` lewat `McuOrderQueryService` (`AsNoTracking`) |
| Requirement/decision | `FR-MCU-037` (tampilan *n dari m komponen tuntas*); `MCU-DEC-031`; `MCU-FE-003` (NIK tidak utuh di daftar) |
| Kontrak | `MCU-API-v1` grup Mcu Order (GET); `MCU-PERM-v1` (`McuOrder : Read`) |
| Reuse | `PagedResult<T>`, `FilterMetadataResponse` |
| Cakupan | Query service, DTO, sebagian `McuOrderController` |
| Dependency | `BE-MCU-002` |
| Acceptance criteria | (1) Penyaring tanggal/status/sumber/perusahaan/dokter bekerja. (2) Daftar menampilkan NIK dimasking (mis. `3201********0001`). (3) Order berkomponen 4 dengan 3 tuntas tampil *3 dari 4*. (4) `summary?date=2026-10-27` mengembalikan jumlah per sembilan status |
| Verifikasi | Uji integrasi harness (dengan baris master induk agar join tidak menghilangkan data) |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-011` ✅ — Booking, jadwal ulang, batal sebelum datang

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-011.md). `POST bookings`, `PATCH {id}/reschedule`, `PATCH {id}/cancel` (sebelum datang) lewat `McuBookingService`; `AT-MCU-04`, `05`, `08`, `09`, `10`, `14` lulus di harness; log terbukti tanpa NIK/nama/alasan; uji MCU 128 lulus. Batal penuh sesudah check-in tetap `BE-MCU-025` |
| Outcome | `POST bookings`, `PATCH {id}/reschedule`, `PATCH {id}/cancel` (jalur sebelum datang) lewat `McuBookingService` |
| Requirement/decision | `FR-MCU-010`..`014`; `MCU-DEC-003`, `012`, `014`, `033`, `038`; `MCU-DEC-041` |
| Kontrak | `MCU-API-v1` grup Mcu Order; `VAL-MCU-10`, `11`, `15`..`18`, `20`, `21`, `23`, `46`; `MCU-STATE-v1` |
| Reuse | `McuScheduleService` (007), `McuOrderNumberService` (004), `MstPatient` (baca) |
| Cakupan | Service, DTO, controller |
| Dependency | `BE-MCU-004`, `005`, `007`, `010` |
| Acceptance criteria | `AT-MCU-04`, `AT-MCU-05` (jalur API), `AT-MCU-08`, `AT-MCU-09`, `AT-MCU-10`, `AT-MCU-14` lulus. **Contoh:** booking Budi (identitas sementara) → order *Terjadwal*, `PatientId` kosong, kuota 1/30, nol kunjungan, nol baris tagihan, nol pasien baru. Booking kedua Budi pada 27 Okt → `409` menyebut `MCU-RSMMC-000001` |
| Verifikasi | Uji integrasi harness |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti; log tidak memuat NIK |

### `BE-MCU-012` ✅ — Tutup hari: *Tidak Hadir*

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-012.md). `McuNoShowService` + `McuNoShowHostedService` (saat start + tiap 23.59 WIB; saklar `MedicalCheckup:NoShowEnabled`, jam `MedicalCheckup:NoShowCutoffLocalTime`); `AT-MCU-13`, idempoten, dan saklar terbukti dengan jam tiruan; uji MCU 137 lulus |
| Outcome | `McuNoShowHostedService` mengubah order *Terjadwal* yang tanggalnya lewat menjadi *Tidak Hadir* dan melepas kuota; jam tutup dari `MedicalCheckup:NoShowCutoffLocalTime` (bawaan 23.59 WIB) |
| Requirement/decision | `FR-MCU-015`; `MCU-DEC-014`, `030` |
| Kontrak | `MCU-STATE-v1`; `02-backend-architecture.md` §13 |
| Reuse | Pola hosted service CRM (`PatientMembershipLifecycleHostedService`) termasuk saklar `Enabled` |
| Cakupan | Hosted service, konfigurasi, registrasi DI |
| Dependency | `BE-MCU-011` |
| Acceptance criteria | `AT-MCU-13`. **Contoh:** pukul 23.59 WIB 27 Okt order Budi → *Tidak Hadir*, kuota 29/30, riwayat mencatat perubahan oleh sistem. Dijalankan dua kali tidak mengubah apa pun lagi. Ada saklar konfigurasi untuk mematikannya saat uji modul lain |
| Verifikasi | Uji dengan jam tiruan; uji idempoten |
| Risiko/pemilik | Hosted service menulis ke DB bersama saat aplikasi mulai — wajib bisa dimatikan. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-013` ✅ — Check-in admisi

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-013.md). Grup `Mcu Check In` (3 endpoint) lewat `McuCheckInService` dalam satu transaksi; `EncounterIntakeService` dan `BillingInvoiceService.UpsertChargeAsync` kini ikut transaksi pemanggil, dan intake menerima penjamin perusahaan khusus kunjungan MCU (keputusan Yoga 2026-10-22; persetujuan tertulis pemilik Billing/Registrasi belum tercatat). `AT-MCU-15`..`24` lulus di harness dengan Billing sungguhan; kegagalan Billing sesudah kunjungan terbukti membatalkan seluruh check-in |
| Outcome | Grup `Mcu Check In`: `GET patient-matches`, `GET check-in/preview`, `POST check-in` lewat `McuCheckInService`, **dalam satu transaksi**: kunci jadwal → tautkan pasien → kunjungan (`BE-MCU-001`) → baris paket (`AddCatalogChargeAsync`) → dokter MCU + penandaan persyaratan → order *Hadir* + riwayat |
| Requirement/decision | `FR-MCU-020`..`024`; `MCU-DEC-003`, `013`, `020`, `025`, `026`, `032`, `033`, `039`, `042` (dokter jaga sebagai isian bawaan); `MCU-COORD-009` |
| Kontrak | `MCU-API-v1` grup Mcu Check In; `VAL-MCU-24`..`30`; `MCU-STATE-v1`; `MCU-INT-v1` |
| Reuse | `EncounterIntakeService` (001), `BillingInvoiceService.AddCatalogChargeAsync`, `MstTariff`, Patient Management `POST patients` (pasien baru dibuat lebih dulu oleh pemanggil) |
| Cakupan | Service, DTO, controller |
| Dependency | `BE-MCU-001`, `BE-MCU-011` |
| Acceptance criteria | `AT-MCU-16`..`AT-MCU-24` lulus. **Contoh:** check-in Budi → order yang **sama** *Hadir*, satu kunjungan `MedicalCheckup`, satu baris Rp 350.000, tanpa biaya administrasi. Siti (PT Contoh) 27 Okt → Rp 300.000; 5 Jan → Rp 350.000. Tarif dinonaktifkan sebelum check-in → `409`, order tetap *Terjadwal*, nol kunjungan. `preview` mengisi dokter jaga sebagai pilihan bawaan |
| Verifikasi | Uji integrasi harness lintas modul; kegagalan disimulasikan dengan interceptor simpan |
| Risiko/pemilik | Transaksi lintas tiga modul. Pemilik: Yoga; Billing dipakai apa adanya |
| DoD | Kriteria terbukti; kirim ulang check-in tidak menggandakan apa pun |

### `BE-MCU-014` ✅ — *Walk-in* admisi

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-014.md). `POST orders/walk-ins`: order + kuota hari ini + check-in `BE-MCU-013` dalam satu transaksi; kedua kriteria terbukti di harness dengan Registrasi dan Billing sungguhan |
| Outcome | `POST orders/walk-ins`: order langsung *Hadir* memakai kuota hari itu, lewat jalur check-in yang sama |
| Requirement/decision | `FR-MCU-025`; `MCU-DEC-017`, `037` |
| Kontrak | `MCU-API-v1` grup Mcu Order; `VAL-MCU-10`, `11`, `20`, `25`..`29` |
| Reuse | `McuCheckInService` (013), `McuScheduleService` |
| Cakupan | Service, DTO, controller |
| Dependency | `BE-MCU-013` |
| Acceptance criteria | (1) Walk-in pada hari berkuota penuh → `409` menyebut kuota; sesudah pelampauan kepala unit → berhasil. (2) Satu kunjungan dan satu baris paket, sama seperti check-in |
| Verifikasi | Uji integrasi harness |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-020` ✅ — Lab/Rad: pesanan bertanda paket dan penahanan tagihan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-020.md). Jalur internal `CreateForMcuAsync` Lab/Rad (bertanda, dokter MCU, ikut transaksi pemanggil); pesanan bertanda nol fakta tagih di spesimen layak, mutu citra, dan Form Hasil; pesanan tanpa tanda tetap menagih; uji mutasi membuktikan penjaga |
| Outcome | Jalur internal `LabOrderService`/`RadOrderService` untuk membuat pesanan ber-`CoveringMcuOrderId` dengan `InstructingDoctorId` = dokter MCU; `LabSpecimenService` dan `RadStudyService` **tidak** menerbitkan `LaboratoryCharge`/`RadiologyCharge` untuk pesanan bertanda; detail pesanan Lab/Rad menampilkan ruas baca *dicakup paket MCU* |
| Requirement/decision | `FR-MCU-031` (sisi Lab/Rad), `FR-MCU-032`; `MCU-DEC-022`, `032`; `MCU-COORD-002` ✅, `MCU-COORD-008` ✅ |
| Kontrak | `MCU-API-v1` *Perubahan internal* (Lab, Rad — hanya penambahan ruas baca); `MCU-INT-v1` `INT-MCU-05`, `06` |
| Reuse | `LabOrderService`, `LabSpecimenService`, `RadOrderService`, `RadStudyService` |
| Cakupan | Service Lab/Rad dan DTO detail pesanan; uji regresi |
| Dependency | `BE-MCU-003` |
| Acceptance criteria | `AT-MCU-28` pada tingkat Lab/Rad: spesimen pesanan bertanda diterima dan Thorax diakuisisi → nol fakta tagih. Pesanan **tanpa** tanda tetap menerbitkan fakta tagih seperti sekarang (regresi) |
| Verifikasi | Uji integrasi harness Lab dan Rad |
| Risiko/pemilik | Tagihan ganda Rp 760.000 untuk paket Rp 350.000 bila penahanan bocor. Pemilik: Lab dan Rad |
| DoD | Kriteria terbukti; uji Lab/Rad yang ada tetap lulus |

### `BE-MCU-021` ✅ — Mulai pemeriksaan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-021.md). `POST orders/{id}/start-examination`: gerbang `VAL-MCU-34` lewat `LabPaymentClearanceRules`, pesanan Lab per disiplin + Rad per komponen bertanda paket atas nama dokter MCU, komponen dan order berpindah — satu transaksi; `AT-MCU-26`, `27`, `28` terbukti |
| Outcome | `POST orders/{id}/start-examination`: cek lunas (perorangan) atau terjamin (perusahaan) lewat `LabPaymentClearanceRules`; buat pesanan Lab per disiplin dan pesanan Rad per komponen bertanda paket; komponen *Dipesan*; order *Dalam Pemeriksaan* — satu transaksi |
| Requirement/decision | `FR-MCU-030`, `FR-MCU-031`; `MCU-DEC-004`, `024`, `032` |
| Kontrak | `MCU-API-v1` grup Mcu Examination; `VAL-MCU-34`, `46`; `MCU-STATE-v1`; `MCU-PERM-v1` (`McuExamination : Start`) |
| Reuse | `LabPaymentClearanceRules`, jalur pesanan bertanda (020) |
| Cakupan | `McuExaminationService` (bagian mulai), controller |
| Dependency | `BE-MCU-013`, `BE-MCU-020` |
| Acceptance criteria | `AT-MCU-26`, `AT-MCU-27`, `AT-MCU-28` (ujung ke ujung). **Contoh:** Budi belum bayar → `409` *"Tagihan paket belum lunas (sisa Rp 350.000)."* dan nol pesanan. Sesudah lunas → satu pesanan Lab (DL, GDP) dan satu pesanan Rad (Thorax), keduanya bertanda dan atas nama dr. Rina; tagihan tetap Rp 350.000 dan lunas sesudah spesimen diterima |
| Verifikasi | Uji integrasi harness |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-022` ✅ — Komponen: daftar, sinkron, tambah, jadwal ulang

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-022.md). Empat endpoint komponen; tambah di luar paket hanya dokter MCU yang ditetapkan (pesanan Lab/Rad tanpa tanda); sinkron menyelesaikan komponen dari Lab/Rad dan memindah order ke *Menunggu Resume*; jadwal ulang tanpa kuota. `AT-MCU-29` (sisi MCU/Lab) dan `AT-MCU-07` terbukti; nominal tagihan Billing tidak diuji |
| Outcome | `GET/POST components`, `POST components/refresh`, `PATCH components/{componentId}/reschedule` |
| Requirement/decision | `FR-MCU-033`, `FR-MCU-037`, `FR-MCU-023` (komponen dijadwal ulang); `MCU-DEC-013`, `029`, `036` |
| Kontrak | `MCU-API-v1` grup Mcu Examination; `MCU-STATE-v1` bagian komponen; `MCU-PERM-v1` (`McuExamination : Read/AddComponent/Update`) |
| Reuse | Status `LabExamination`/`RadReport`, status antrean skrining (001) |
| Cakupan | `McuExaminationService` (komponen), controller |
| Dependency | `BE-MCU-021` |
| Acceptance criteria | `AT-MCU-29`, `AT-MCU-07`. **Contoh:** dr. Rina menambah Kolesterol Total → komponen *di luar paket*, pesanan Lab **tanpa** tanda, tagihan bertambah Rp 75.000 saat spesimen diterima, kelengkapan *4 dari 5*. GDP dijadwal ulang ke Selasa yang penuh → berhasil, kuota Selasa tidak berubah |
| Verifikasi | Uji integrasi harness |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-023` ✅ — Isi Pemeriksaan MCU

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-023.md). `GET/PUT components/{componentId}/examination-result`: kunci skrining dari `RegQueue.ScreeningCompletedAt`, isian wajib saat diselesaikan, `6,4`/`6.4` → `6.4`, kunci sesudah approve, revisi nilai sesudah buka ulang; komponen terakhir tuntas → *Menunggu Resume*. `AT-MCU-33`, `34`, `35` terbukti |
| Outcome | `GET/PUT components/{componentId}/examination-result` dengan kunci skrining, isian wajib saat diselesaikan, angka desimal titik, dan kunci sesudah approve |
| Requirement/decision | `FR-MCU-036`; `MCU-DEC-007`, `018`, `035`; `MCU-DEC-041` |
| Kontrak | `VAL-MCU-36`, `42`, `43`, `44`; `MCU-PERM-v1` (`McuExamination : Update`) |
| Reuse | Status skrining (001) |
| Cakupan | `McuExaminationService` (hasil), controller |
| Dependency | `BE-MCU-022`, `BE-MCU-001` |
| Acceptance criteria | `AT-MCU-33` (termasuk bukti devYoga `MCU-UNK-05` bila belum tertutup di 001), `AT-MCU-34`, `AT-MCU-35`. **Contoh:** isian `6.4` tersimpan `6.4`, bukan `64`; EKG *Selesai* tanpa lampiran |
| Verifikasi | Uji unit + integrasi |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-024` ✅ — Batal komponen dengan refund

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-024.md). `AT-MCU-30`, `31`, `37` terbukti di harness integrasi (Lab, Rad, Billing produksi). Lab ditambah `CancelOrderedProcedureAsync` untuk permintaan yang belum masuk wadah (keputusan pemilik Lab 2026-10-23); komponen Lab terakhir pada pesanan yang belum diproses membatalkan pesanan Lab utuh. Refund per komponen berkorelasi komponen. **Uji devYoga belum dijalankan** (migration `AddCoveringMcuOrderToLabRadOrders` + `AddBillingRefundCaseItem` tertunda). Temuan: batal penuh sesudah batal komponen ditolak Billing (*melebihi sisa*) — butuh perubahan `INT-MCU-09` |
| Outcome | `PATCH components/{componentId}/cancel` oleh dokter MCU yang ditetapkan: Lab dibatalkan per pemeriksaan lewat `LabExaminationService.CancelAsync`, Rad per pesanan, lalu refund sebesar `RefundAmountSnapshot` bila sudah dibayar |
| Requirement/decision | `FR-MCU-034`; `MCU-DEC-015`, `043`; `MCU-AC-009`, `MCU-AC-031`; `ARC-TECH-01` ✅ |
| Kontrak | `MCU-STATE-v1` (batalkan komponen); `VAL-MCU-38`, `40`; `MCU-INT-v1` `INT-MCU-08`, `INT-MCU-09` |
| Reuse | `LabExaminationService.CancelAsync` (terbukti `MCU-EVD-002` 2.1), batal pesanan Rad, refund Billing **sesudah** `BE-MCU-026` |
| Cakupan | `McuExaminationService` (batal komponen), controller |
| Dependency | `BE-MCU-021`, `BE-MCU-026` |
| Acceptance criteria | `AT-MCU-30`, `AT-MCU-31`, `AT-MCU-37`. **Contoh:** Thorax dibatalkan (kehamilan) → komponen *Dibatalkan*, pesanan Rad batal, refund tercatat Rp 150.000. Audiometri dibatalkan kemudian → refund Rp 50.000; total ≤ Rp 350.000. Pemeriksaan Lab yang sudah tervalidasi → `409` *"Pemeriksaan Lab sudah divalidasi dan tidak dapat dibatalkan."* (penjaga MCU sendiri, karena Lab hanya menolak yang sudah dirilis) |
| Verifikasi | Uji integrasi + satu uji devYoga refund dengan izin |
| Risiko/pemilik | Wajib menunggu 026 selesai; tanpa itu refund tercatat Rp 350.000 untuk pembatalan Rp 150.000. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-025` ✅ — Batal penuh

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-025.md). `PATCH orders/{id}/cancel` untuk order *Hadir*/*Dalam Pemeriksaan*: pesanan Lab/Rad batal, refund utuh (dibayar) atau void baris (belum dibayar), kunjungan + antrean batal, order *Dibatalkan* — satu transaksi; `BillingRefundService.CreateAsync`/`BillingInvoiceService.VoidItemAsync` dan jalur baru `EncounterIntakeService.CancelMedicalCheckupAsync` ikut transaksi pemanggil; kuota tidak dikembalikan (keputusan user 2026-10-22). `AT-MCU-32` terbukti |
| Outcome | `PATCH orders/{id}/cancel` jalur sesudah check-in: hanya bila belum ada komponen dimulai; pesanan turunan dibatalkan, baris paket dikembalikan **utuh** (refund bila sudah dibayar, pembatalan baris bila belum), kunjungan dibatalkan, order *Dibatalkan* — satu transaksi |
| Requirement/decision | `FR-MCU-035`; `MCU-DEC-021`; `MCU-AC-014` |
| Kontrak | `VAL-MCU-41`; `MCU-STATE-v1` 2.3 (definisi *dimulai*); `MCU-INT-v1` `INT-MCU-09` (refund utuh) |
| Reuse | `BillingRefundService.CreateAsync` apa adanya — refund utuh satu baris **sudah** didukung (`MCU-EVD-002` 2.2); `BillingInvoiceService.VoidItemAsync` untuk baris yang belum dibayar (peserta perusahaan); batal pesanan Lab/Rad; pembatalan kunjungan Registrasi |
| Cakupan | `McuExaminationService` (batal penuh), `McuOrderController` |
| Dependency | `BE-MCU-021` |
| Acceptance criteria | `AT-MCU-32`. **Contoh:** Budi membayar 07.10, batal 07.20 sebelum sampel diambil → *Dibatalkan*, refund Rp 350.000, kunjungan batal. Bila darah sudah diambil 07.15 → `409` *"Sudah ada pemeriksaan yang dimulai. Gunakan pembatalan per komponen."* |
| Verifikasi | Uji integrasi harness |
| Risiko/pemilik | Cara membatalkan kunjungan dan memakai `VoidItemAsync` dipastikan terhadap source saat eksekusi. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-026` ✅ — Perluasan refund Billing

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-22 — [laporan](../task/report/backend/BE-MCU-026.md). Tabel baru `BilRefundCaseItem` (rincian refund per baris; migration `20261022081149_AddBillingRefundCaseItem` dibuat, **belum diterapkan**); `CreateAsync` memakai `RequestedAmount` ≤ sisa baris, nominal sebagian hanya satu baris, beberapa baris = sisa penuh; satu case aktif per invoice kini hanya untuk deposito; `refundable-items` menampilkan sisa per baris. Ketiga uji `MCU-EVD-002` 2.2 berbalik; regresi kasir lulus. Bentuk dipilih user 2026-10-22; tinjauan Yasmina tertunda |
| Outcome | `BillingRefundService.CreateAsync` memakai `RequestedAmount` (≤ sisa yang dapat direfund pada baris terpilih); akumulasi refund tercatat per baris; beberapa case berurutan atas satu baris diperbolehkan selama total ≤ nilai baris; `refundable-items` menampilkan sisa per baris. Celah refund ganda tertutup untuk **semua** pemakai Billing |
| Requirement/decision | `MCU-DEC-043`; `MCU-COORD-003`; `MCU-AC-031`; `MCU-EVD-002` 2.2 |
| Kontrak | `MCU-API-v1` *Perubahan internal* baris Billing (perilaku `RequestedAmount` berubah, bentuk request tetap) |
| Reuse | `BilRefundCase`, `BilRefundLine`, alokasi proporsional ke tender |
| Cakupan | Service refund Billing, kemungkinan satu kolom/tabel akumulasi refund per baris — bentuk ditetapkan pemilik Billing |
| Dependency | — (persetujuan Yasmina, pemilik `billing-kasir`, ✅ 2026-10-21) |
| Acceptance criteria | Ketiga uji `MCU-EVD-002` 2.2 berbalik: (1) permintaan Rp 150.000 tercatat Rp 150.000; (2) komponen kedua Rp 50.000 pada baris yang sama diterima sesudah case pertama; (3) total diajukan atas baris Rp 350.000 tidak pernah melebihi Rp 350.000 — permintaan Rp 200.000 sesudah Rp 150.000 + Rp 50.000 ditolak. Refund utuh tanpa `RequestedAmount` tetap berperilaku sama (regresi kasir) |
| Verifikasi | Uji harness `MCU-EVD-002` dijadikan uji permanen di `Tests/`, ditambah uji regresi layar refund kasir |
| Risiko/pemilik | Mengubah perilaku layar refund kasir yang sudah dipakai; bentuk akumulasi per baris dan perlakuan peserta perusahaan ditetapkan pemilik Billing saat eksekusi. Pemilik: Yasmina (`billing-kasir`) |
| DoD | Kriteria terbukti; laporan task ditinjau pemilik Billing |

### `BE-MCU-030` ✅ — Resume: draf, approve, kembalikan, verifikasi

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-030.md). Lima endpoint grup Mcu Resume lewat `McuResumeService`; `AT-MCU-38`, `39` (dua bagian pertama), `40` terbukti di harness integrasi; uji mutasi 5/5 tertangkap. Verifikasi juga menolak bila hasil Lab/Rad komponen yang selesai tidak lagi *Completed* di modulnya. Tanpa migration |
| Outcome | Grup `Mcu Resume`: `GET /`, `PUT current`, `POST current/approve`, `POST current/return`, `POST current/verify` lewat `McuResumeService` |
| Requirement/decision | `FR-MCU-040`..`043`; `MCU-DEC-008`, `009`, `018`, `032`, `036`; `MCU-DEC-041` |
| Kontrak | `VAL-MCU-35`..`40`; `MCU-STATE-v1`; `MCU-PERM-v1` (`McuResume`) |
| Reuse | Penautan akun ke `MstDoctor` seperti `LabOrderService.VerifyInstructionAsync` |
| Cakupan | Service, DTO, controller |
| Dependency | `BE-MCU-022`, `BE-MCU-023` |
| Acceptance criteria | `AT-MCU-38`, `AT-MCU-39` (dua bagian pertama), `AT-MCU-40`. **Contoh:** Thorax belum diekspertise → approve `409` menyebut Thorax. dr. Hadi (bukan dokter yang ditetapkan) → `403`. dr. Rina approve lalu mencoba memverifikasi → `403`; Dewi memverifikasi tanpa mengubah isi → *Terverifikasi* |
| Verifikasi | Uji integrasi harness |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-031` ✅ — Buka ulang dan versi resume

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-031.md). `POST reopen` lewat `McuResumeService.ReopenAsync`; `AT-MCU-43` (tanpa alur penyerahan) terbukti di harness integrasi, penanda penyerahan versi lama sudah diisi (diuji dengan data semai). Revisi nilai kini mencatat versi resume yang **disetujui** dengan nilai lama (perbaikan `BE-MCU-023`). `AT-MCU-16` pada kriteria adalah uji check-in — kemungkinan salah rujuk. Tanpa migration |
| Outcome | `POST resumes/reopen`: versi lama *Dikoreksi* tetap terbaca, versi baru *Draf*, isian Pemeriksaan MCU terbuka dengan revisi nilai tercatat, penyerahan versi lama ditandai perlu pemberitahuan |
| Requirement/decision | `FR-MCU-044`; `MCU-DEC-011`; `MCU-AC-013` |
| Kontrak | `MCU-STATE-v1`; `VAL-MCU-40`, `44` |
| Reuse | — |
| Cakupan | `McuResumeService`, `McuExaminationService` (revisi nilai) |
| Dependency | `BE-MCU-030` (penanda penyerahan diisi bila `BE-MCU-034` sudah ada; uji penuh `AT-MCU-43` dijalankan ulang di 034) |
| Acceptance criteria | `AT-MCU-43` tanpa bagian penyerahan; `AT-MCU-16` versi ulang. Hanya dokter yang ditetapkan, beralasan |
| Verifikasi | Uji integrasi |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-032` ✅ — Ganti dokter MCU

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-032.md). `PATCH mcu-doctor` lewat `McuCheckInService.ReassignDoctorAsync`; `AT-MCU-39` bagian ketiga terbukti di harness integrasi. Jalur *reuse* Lab/Rad ternyata tidak ada — dibuat `ReassignMcuDoctorAsync` (`internal`) di `LabOrderService` dan `RadOrderService` sesuai `INT-MCU-06`; hanya pesanan paket yang masih terbuka yang pindah. Tanpa migration |
| Outcome | `PATCH orders/{id}/mcu-doctor` oleh kepala unit beralasan; riwayat dokter bertambah; pesanan Lab/Rad yang masih terbuka berpindah `InstructingDoctorId` ke dokter pengganti |
| Requirement/decision | `FR-MCU-045`; `MCU-DEC-040`; `MCU-COORD-008` ✅ |
| Kontrak | `MCU-API-v1` grup Mcu Check In; `VAL-MCU-45`; `MCU-PERM-v1` (`McuOrder : ReassignDoctor`) |
| Reuse | Jalur ganti dokter penanggung jawab pesanan terbuka di Lab/Rad (020) |
| Cakupan | `McuCheckInService`, controller |
| Dependency | `BE-MCU-020`, `BE-MCU-030` |
| Acceptance criteria | `AT-MCU-39` bagian ketiga: sesudah penggantian, dr. Hadi berhasil approve dan dr. Rina `403`. Order *Diserahkan* → `409` *"Dokter MCU tidak dapat diganti pada order yang sudah selesai."* |
| Verifikasi | Uji integrasi |
| Risiko/pemilik | Pemilik: Yoga; sisi pesanan Lab/Rad |
| DoD | Kriteria terbukti |

### `BE-MCU-033` ✅ — Dokumen hasil dan ringkasan kelayakan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-033.md). `GET result-document?variant=full|fitness-summary` lewat `McuDeliveryService`; `AT-MCU-42` bagian ringkasan terbukti (refleksi daftar putih 12 ruas + skenario Siti tanpa *HBsAg*). Hasil lengkap memuat hasil Lab dirilis, laporan Rad dirilis, isian Pemeriksaan MCU; syarat resume berlaku *Disetujui Dokter*/*Terverifikasi*. Tanpa migration. Diuji di HEAD `cee59643` |
| Outcome | `GET result-document?variant=full|fitness-summary` disusun server; DTO ringkasan **tidak punya** ruas nilai maupun diagnosis |
| Requirement/decision | `FR-MCU-050`; `MCU-DEC-010`; `MCU-FE-003` |
| Kontrak | `MCU-API-v1` grup Mcu Delivery; `MCU-PERM-v1` (`McuDelivery : Read`) |
| Reuse | Hasil Lab/Rad yang sudah dirilis |
| Cakupan | `McuDeliveryService` (susun data cetak), controller |
| Dependency | `BE-MCU-030` |
| Acceptance criteria | `AT-MCU-42` bagian ringkasan: refleksi atas tipe DTO ringkasan menunjukkan nol ruas nilai/diagnosis. **Contoh:** Siti *Unfit sementara* → ringkasan hanya *Unfit sementara — disarankan tidak bertugas di bagian pengolahan makanan sampai pemeriksaan ulang*; kata *HBsAg reaktif* tidak ada |
| Verifikasi | Uji unit + refleksi DTO |
| Risiko/pemilik | Bentuk cetakan final menunggu brief layar; data saja di sini. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-034` ✅ — Persetujuan pelepasan dan penyerahan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-034.md). Enam endpoint penyerahan/persetujuan di `McuDeliveryService`; `AT-MCU-41`, `42`, `43` (penuh), `44` terbukti di harness integrasi; uji mutasi 5/5 tertangkap. Bentuk bukti persetujuan final (`MCU-OQ-003`) tetap menahan go-live penyerahan ke perusahaan. Tanpa migration |
| Outcome | `GET/POST deliveries`, `PATCH deliveries/{deliveryId}/correction-notice`, `GET/POST release-consents`, `PATCH release-consents/{consentId}/revoke`; penyerahan pertama versi terverifikasi → *Diserahkan* |
| Requirement/decision | `FR-MCU-051`..`053`; `MCU-DEC-010`, `011`, `019`, `027` |
| Kontrak | `VAL-MCU-31`..`33`; `MCU-STATE-v1`; `MCU-PERM-v1` (`McuDelivery : Create/ManageConsent`) |
| Reuse | — (pengiriman otomatis ditunda, `MCU-COORD-005`) |
| Cakupan | `McuDeliveryService`, controller |
| Dependency | `BE-MCU-031`, `BE-MCU-033` |
| Acceptance criteria | `AT-MCU-41`, `AT-MCU-42`, `AT-MCU-43` (penuh), `AT-MCU-44`. **Contoh:** isi lengkap ke PT Contoh tanpa persetujuan Siti → `409`; ringkasan → tersimpan dengan penerima, isi, saluran, tujuan, versi, petugas, waktu |
| Verifikasi | Uji integrasi |
| Risiko/pemilik | Bentuk bukti persetujuan (`MCU-OQ-003`) menahan go-live, bukan task ini. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `BE-MCU-035` ✅ — Privasi log dan registri izin

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/backend/BE-MCU-035.md). `AT-MCU-45` lulus: 22 endpoint ubah data peserta lewat controller sungguhan, penangkap log merekam pesan + scope semua logger (termasuk mitra) — nol NIK, nilai isian, isi resume, tujuan penyerahan, maupun alasan tindakan. `PermissionRegistryValidator` nol temuan MCU; 28 kunci MVP `MCU-PERM-v1` terdeklarasi persis; 3 kunci `POST-MVP` (impor, rekomendasi, jasa medis) belum punya endpoint. Tanpa perubahan aplikasi; uji dihapus sesudah lulus (instruksi pemilik) |
| Outcome | Satu uji menyeluruh atas semua tindakan ubah MCU: log aplikasi tidak memuat NIK, nilai isian, isi resume, maupun tujuan penyerahan; `PermissionRegistryValidator` lulus dengan seluruh kunci `MCU-PERM-v1` |
| Requirement/decision | `NFR-006`; `MCU-DEC-010`; `AT-MCU-45` |
| Kontrak | `MCU-PERM-v1`; `02-backend-architecture.md` §11 |
| Reuse | `LoggerService` (catatan: `AuditAsync` hanya menulis teks pesan) |
| Cakupan | Uji di `Tests/`; perbaikan teks log bila ditemukan kebocoran |
| Dependency | `BE-MCU-034` |
| Acceptance criteria | `AT-MCU-45` lulus; validator registri izin lulus |
| Verifikasi | Penangkap log pada harness |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

## Prasyarat operasional sebelum UAT (bukan task kode)

| Hal | Isi | Pemilik |
|---|---|---|
| Penerapan migration ke devYoga | Migration `BE-MCU-002` dan `003` diterapkan dengan skrip, atas izin per sesi | Yoga |
| Kebijakan akses | Lima peran MCU dipetakan ke departemen/posisi lewat layar Role Access (`MCU-GAP-02`); POST Role Access menimpa seluruh set — baca dulu, kirim ulang utuh | Yoga |
| Tarif paket | Baris `MstTariff` ber-`IsPackageTariff` untuk paket uji, ditandai CONTOH | Pemilik tarif |
| Klinik MCU | Hasil `AT-MCU-RUN-03` di `BE-MCU-001` | Master Data |

## Di luar Roadmap Ini

| Epic | Penyebab | Yang terjadi selama MVP |
|---|---|---|
| `EPIC MCU-07` Impor rombongan | `POST-MVP` (PRD §8) | Admisi membooking rombongan satu per satu |
| `EPIC MCU-08` Kiosk | `POST-MVP`; perubahan modul kiosk `MCU-COORD-006`. Dokter jaga sudah diputuskan (`MCU-DEC-042`) dan kolomnya ikut dibuat di `BE-MCU-002` | Peserta check-in di admisi |
| `EPIC MCU-09` Rekomendasi RJ/RI/IGD | `POST-MVP`; layar di tiga modul lain | Dokter menyampaikan rekomendasi; admisi membooking |
| `EPIC MCU-10` Sumber jasa medis | `MCU-COORD-010` | Nilai jasa tersimpan per komponen; Keuangan menghitung manual |
| `EPIC MCU-11` Laporan | `DEC-MCU-005` | Daftar Pasien MCU dengan penyaring |
| Pengiriman WhatsApp/email otomatis | `MCU-COORD-005` | Penyerahan dicatat manual (`MCU-DEC-027`) |
