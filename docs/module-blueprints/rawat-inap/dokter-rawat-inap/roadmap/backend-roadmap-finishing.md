# Roadmap Backend — Dokter Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Roadmap | `dokter-rawat-inap/roadmap/backend-roadmap-finishing.md` — revision `2` |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `dokter-rawat-inap`, kontrak **`0.7.0` `approved`** 2026-10-02 lewat `RWI-DEC-221` |
| Status roadmap | **Eksekusi backend diotorisasi user 2026-10-05; BE-RWI-160–164 ✅ 5 Oktober 2026** — build terintegrasi `0 Error(s)`, migration R10/R11 diterapkan ke database development; uji API/runtime dikecualikan atas instruksi pengguna. Riwayat coding tanpa build dicatat di bawah |
| Ditulis | 2 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `02-backend-architecture.md` bagian 12 (`3dd1a36f…`), `contracts/api-contract.md` bagian 13 (`bab898df…`), `data/data-dictionary.md` bagian Finishing (`c0ced2e7…`), `testing/acceptance-test-matrix.md` bagian 15 (`e7c6da47…`), `contracts/validation-matrix.md` (`VAL-RWF-60` s.d. `68`). Hash lengkap pada `../blueprint-manifest.md` bagian 10 |
| Keputusan | `RWI-DEC-114`, `153`, `165`, `168`, `171`, `188`, `191`, `218`, `219`, `220`, `221`; gate `1.10` |
| Source SHA | Backend `bf5c6bde` |
| Deret ID | `BE-RWI-160` s.d. `BE-RWI-164` |
| Roadmap pendamping | `frontend-roadmap-finishing.md`, `requirement-traceability-finishing.md`. Roadmap `backend-roadmap-v2.md` tetap berlaku untuk task lamanya; `BE-RWI-104` di sana menjadi prasyarat `FE-RWI-172` |

**Kebijakan verifikasi backend.** Mengikuti `rules/backend/TEST_POLICY.md`: bukti task backend adalah QBE preflight/conformance, review diff/scope, `dotnet build` project aplikasi, verifikasi API/kontrak, verifikasi proses bisnis dengan contoh, dan verifikasi runtime bila lingkungan tersedia. Roadmap ini tidak memuat task automated test, dan tidak adanya automated test bukan gap.

**Pada setiap handoff ke `build-module-backend`:** QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` beserta `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Wewenang menerapkan migration ke database dan deployment tidak diberikan roadmap ini.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

```text
BE-RWI-161 ✅ ──┐
                │
BE-RWI-162 ✅ ──┴─┬─> BE-RWI-164 ✅
                  │
BE-RWI-163 ✅ ────┘

BE-RWI-160 ✅
```

Jumlah pasangan prasyarat→task: **3**, sama dengan isi kolom `Dependency`. Tidak ada prasyarat dari roadmap lain.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-160`, `BE-RWI-161`, `BE-RWI-162`, `BE-RWI-163` — boleh paralel, kecuali catatan migration di bawah |
| 2 | `BE-RWI-161`, `BE-RWI-162`, `BE-RWI-163` | `BE-RWI-164` |

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian 23.20.** `MVP-0` (`RWF-W0`): `BE-RWI-160`, `BE-RWI-163` — katalog tindakan dan perkiraan harga ikut pembukaan tombol Lab/Radiologi perawat. `MVP-1` (`RWF-W2`): `BE-RWI-161`, `BE-RWI-162`, `BE-RWI-164`.

**Catatan migration.** `BE-RWI-161` (`R10`) dan `BE-RWI-162` (`R11`) membuat migration pada satu `ApplicationDbContext` dan satu snapshot model; keduanya dikerjakan berurutan (`R10` lalu `R11`), tidak paralel di branch terpisah. `R10` boleh digabung dengan `K10` milik `keperawatan` (`BE-RWI-166`) menjadi satu migration `NutritionManagement` (`02-module-map.md` 7.4 `RWF-W2`); bila tidak digabung, keduanya tetap berurutan.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-160` | Katalog tindakan bangsal memakai penanda rawat inap; perawat melihat tindakan khusus perawat | `FR-RWF-070`; `RWI-DEC-165` (1); `AC-RWF-070`; `UAT-RWF-31` | `0.7.0` API 13.5; backend 12.5, 12.6 | `PatientProcedureController.master-options` | Parameter `careSetting` dan `audience`; bawaan perilaku lama | — | Kartu | Kartu | Master `IsAvailableForInpatient` bawaan `true` / Muhammad Hamzah | Kartu |
| `BE-RWI-161` | Pesanan gizi menyimpan status verifikasi; dokter peminta memverifikasi | `FR-RWF-031`, `037`; `RWI-DEC-171`, `188`, `191`; `INV-RWF-22`, `23` | API 13.3; backend 12.8, 12.9 (`R10`) | `NutritionOrderService` | Tiga kolom, migration `R10`, daftar dan aksi verifikasi | — | Kartu | Kartu | Modul Gizi milik Ikbal Yulianto | Kartu |
| `BE-RWI-162` | Pesanan darah menyimpan status verifikasi; dokter peminta memverifikasi | `FR-RWF-032`, `037`; `RWI-DEC-171`, `188`, `191`; `INV-RWF-22`, `23` | API 13.4; backend 12.6, 12.8, 12.9 (`R11`) | `BbkBloodOrderService` | Enum, tiga kolom, migration `R11`, daftar dan aksi verifikasi | — | Kartu | Kartu | Modul Bank Darah milik Sukma Giri Pratama | Kartu |
| `BE-RWI-163` | Status tanggungan dan perkiraan harga per pemeriksaan tersedia bagi pemesan | `FR-RWF-034`; `RWI-DEC-218`, `219`, `220` (5); `RWI-AC-335`, `337`; `UAT-RWF-36`, `37` | API 13.2 `coverage-status`; backend 12.13 | `InsuranceCoverageService`, `AccessPermissionService` | Controller dan adapter baru dengan satu endpoint baca | — | Kartu | Kartu | Hak lihat harga dua jalur lama mengikuti hak baca / Muhammad Hamzah | Kartu |
| `BE-RWI-164` | Pesanan gizi dan darah dari bangsal lewat adapter dengan pemeriksaan penugasan dokter | `FR-RWF-031`, `032`, `036`, `038`; `RWI-DEC-171`, `188`; `INV-RWF-20`, `21`, `24`; `UAT-RWF-07`, `08`, `30` | API 13.2; validasi `VAL-RWF-60` s.d. `62` | `IsDoctorAssignedAsync`, service Gizi dan Bank Darah | Tiga endpoint pesanan pada controller `BE-RWI-163` | `BE-RWI-161`, `BE-RWI-162`, `BE-RWI-163` | Kartu | Kartu | Menyentuh dua modul pemilik lain / Muhammad Hamzah | Kartu |

## Kartu task

### ✅ `BE-RWI-160` — Katalog tindakan rawat inap

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Keempat acceptance criteria terpetakan ke source (dicocokkan ulang 5 Oktober 2026). Build terintegrasi `dotnet build` `0 Error(s)`, 233 warning; tanpa perubahan skema. Uji API/runtime `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Catatan: kriteria 4 terpenuhi di tingkat API; `UAT-RWF-31` di layar poliklinik menuntut pemanggil poliklinik mengirim `careSetting=Outpatient`, belum tercakup `FE-RWI-176`. Bukti: [laporan](../task/report/backend/BE-RWI-160.md) |
| **Outcome** | Pemilih tindakan di bangsal menampilkan tindakan yang boleh dipakai di rawat inap, dan pemilih perawat ikut menampilkan tindakan khusus perawat. Pemanggil lama mendapat hasil yang sama persis |
| **Requirement/decision** | `FR-RWF-070`; `RWI-DEC-165` butir 1; `AC-RWF-070`; `UAT-RWF-31` |
| **Kontrak** | `0.7.0`: API 13.5 (`GET clinical-management/patient-procedures/master-options` + `careSetting`, `audience`); backend 12.5, 12.6 |
| **Reuse** | `PatientProcedureController.master-options` (baris `128-136`); kolom `MstProcedure.IsAvailableForInpatient` yang sudah ada |
| **Cakupan** | Enum parameter `ProcedureCatalogCareSetting` (`Outpatient` bawaan, `Inpatient`) dan `ProcedureCatalogAudience` (`Doctor` bawaan, `Nurse`) di `ClinicalManagement/Enums/`. `careSetting=Inpatient` menyaring `IsAvailableForInpatient`; `audience=Nurse` tidak memakai saringan `IsDoctorAction`. Controller ini membaca `ApplicationDbContext` langsung — utang teknis yang **tidak** dirapikan di task ini |
| **Dependency** | — |
| **Acceptance criteria** | 1. Tanpa parameter → hasil identik dengan sebelum perubahan. 2. `careSetting=Inpatient` menampilkan tindakan `IsAvailableForInpatient = true` dan menyembunyikan yang `false`. 3. `audience=Nurse` menampilkan tindakan khusus perawat. 4. Tindakan khusus rawat inap (`IsAvailableForOutpatient = false`) tampil di bangsal, tidak di poliklinik (`UAT-RWF-31`) |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API untuk tiga kombinasi (tanpa parameter, `Inpatient`+`Doctor`, `Inpatient`+`Nurse`) dengan contoh data master |
| **Risiko/pemilik** | Bawaan `IsAvailableForInpatient = true` membuat semua tindakan tampil sampai admin Master Data mengisi penanda (`02-backend-architecture.md` 12.10). Pemilik: Muhammad Hamzah; isi master oleh admin Master Data |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-160.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-161` — Verifikasi instruksi pesanan gizi (`R10`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Keenam acceptance criteria terpetakan ke source. Build terintegrasi `dotnet build` `0 Error(s)`, 233 warning; migration `20261005050735_AddNutritionAndBloodInstructionVerification` diterapkan ke database development (`Done.`, nol `Pending`). Uji API/runtime `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Bukti: [laporan](../task/report/backend/BE-RWI-161.md) |
| **Outcome** | Pesanan konsultasi gizi menyimpan status verifikasi instruksi. Dokter peminta melihat pesanan yang menunggu verifikasinya dan memverifikasinya |
| **Requirement/decision** | `FR-RWF-031`, `FR-RWF-037`; `RWI-DEC-171`, `RWI-DEC-188`, `RWI-DEC-191`; `INV-RWF-22`, `INV-RWF-23` |
| **Kontrak** | API 13.3 (`POST orders` diperluas, `GET orders/instruction-verification-worklist`, `POST orders/{id}/verify-instruction`); backend 12.5, 12.8, 12.9 (`R10`); validasi `VAL-RWF-63`, `64` |
| **Reuse** | `NutritionOrderService`, `NutritionOrderController`; enum `GziInstructionVerificationStatus` yang juga dipakai diet (`keperawatan` `0.6.0`) |
| **Cakupan** | Kolom `InstructionVerificationStatus` (`int`, bawaan `0`), `InstructionVerifiedAt`, `InstructionVerifiedByUserId` pada `GziNutritionOrder`; configuration; migration `R10`; `CreateAsync` menerima status verifikasi (pemanggil lama tetap `NotRequired`); dua endpoint dengan `NutritionOrder : VerifyInstruction` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Pesanan lama dan pesanan dari poliklinik tetap `NotRequired` dan alurnya tidak berubah (`AC-RWF-036`). 2. Daftar hanya memuat pesanan `Pending` milik dokter yang login. 3. Verifikasi oleh dokter peminta menyimpan status, waktu, dan pemverifikasi. 4. Dokter lain → 403 (`VAL-RWF-63`). 5. Sudah diverifikasi → 409 (`VAL-RWF-64`). 6. Baris registry `NutritionOrder : VerifyInstruction` lahir dari atribut endpoint |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration terhadap kamus data; verifikasi API; verifikasi proses bisnis regresi pesanan gizi poliklinik |
| **Risiko/pemilik** | Enum `GziInstructionVerificationStatus` dibuat oleh task yang dikerjakan lebih dulu antara task ini dan `BE-RWI-166`. Modul Gizi milik Ikbal Yulianto (persetujuan `RWI-DEC-191`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked mencatat status penerapan migration apa adanya; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-162` — Verifikasi instruksi pesanan darah (`R11`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Keenam acceptance criteria terpetakan ke source. Build terintegrasi `dotnet build` `0 Error(s)`, 233 warning; migration `20261005050735_AddNutritionAndBloodInstructionVerification` diterapkan ke database development (`Done.`, nol `Pending`). Uji API/runtime `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Build terintegrasi sempat menemukan dua error kompilasi di berkas task ini; sudah diperbaiki (laporan bagian 7.2). Bukti: [laporan](../task/report/backend/BE-RWI-162.md) |
| **Outcome** | Pesanan darah menyimpan status verifikasi instruksi; dokter peminta melihat dan memverifikasinya. Alur `confirm-duplicate` dan pesanan dari poliklinik serta IGD tidak berubah |
| **Requirement/decision** | `FR-RWF-032`, `FR-RWF-037`; `RWI-DEC-171`, `RWI-DEC-188`, `RWI-DEC-191`; `INV-RWF-22`, `INV-RWF-23` |
| **Kontrak** | API 13.4; backend 12.5, 12.6 (`BbkInstructionVerificationStatus`), 12.8, 12.9 (`R11`); validasi `VAL-RWF-63`, `64` |
| **Reuse** | `BbkBloodOrderService`, `BbkBloodOrderController`; kolom `RequestingDoctorId` dan `InputByUserId` yang sudah ada |
| **Cakupan** | Enum `BbkInstructionVerificationStatus` (`NotRequired = 0`, `Pending = 1`, `Verified = 2`); tiga kolom pada `BbkBloodOrder`; configuration; migration `R11`; `CreateAsync` menerima status verifikasi; dua endpoint dengan `BloodOrder : VerifyInstruction` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Pesanan darah poliklinik dan IGD tetap `NotRequired` dan alurnya tidak berubah (`AC-RWF-036`). 2. Daftar hanya memuat pesanan `Pending` milik dokter yang login. 3. Verifikasi menyimpan status, waktu, dan pemverifikasi pada pesanan di Bank Darah (`AC-RWF-035`). 4. Dokter lain → 403 (`INV-RWF-23`). 5. Sudah diverifikasi → 409. 6. `confirm-duplicate` tetap berfungsi |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis regresi pesanan darah |
| **Risiko/pemilik** | Dikerjakan sesudah migration `R10` (catatan migration). Modul Bank Darah milik Sukma Giri Pratama (persetujuan `RWI-DEC-191`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-163` — Status tanggungan dan perkiraan harga (`coverage-status`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Ketujuh acceptance criteria terpetakan ke source. Build terintegrasi `dotnet build` `0 Error(s)`, 233 warning; tanpa perubahan skema. Uji API/runtime `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Bukti: [laporan](../task/report/backend/BE-RWI-163.md) |
| **Outcome** | Layar pemesanan bangsal dapat menampilkan, per pemeriksaan, status tanggungan penjamin dan perkiraan harga sesuai penjamin dan kelas pasien — hanya kepada pemegang hak membuat pesanan jenis itu |
| **Requirement/decision** | `FR-RWF-034` (digantikan sebagian); `RWI-DEC-218`, `RWI-DEC-219`, `RWI-DEC-220` butir 5; `RWI-AC-335`, `RWI-AC-337`; `UAT-RWF-36`, `UAT-RWF-37` |
| **Kontrak** | API 13.2 (`GET episodes/{episodeId}/ancillary-orders/coverage-status`, `CoverageStatusItem`); backend 12.13; validasi `VAL-RWF-66` s.d. `68` |
| **Reuse** | `InsuranceCoverageService.ResolveProcedureAsync`; `AccessPermissionService.HasAccessAsync` |
| **Cakupan** | Membuat `InpatientAncillaryOrderController`, `InpAncillaryOrderAdapter` (method `GetCoverageStatusAsync`), dan `InpatientAncillaryOrderDtos.cs`. `Laboratory`, `Radiology`, `Procedure` diresolusi lewat resolver; `Nutrition`, `Blood` selalu `NOT_ESTIMABLE`. Harga dikirim per `ItemType` hanya bila pemanggil memegang `LabOrder : Create`, `RadOrder : Create`, `PatientProcedure : Create`, `NutritionOrder : Create`, atau `BloodOrder : Create`. Tidak membuka transaksi |
| **Dependency** | — |
| **Acceptance criteria** | 1. Perawat pemegang `LabOrder : Create` memilih "Darah Lengkap" untuk pasien BPJS kelas 2 → `AVAILABLE`, `EstimatedUnitPrice`, dan `PriceLabel` "perkiraan — tagihan final di kasir". 2. Pengguna yang hanya memegang `InpatientEpisode : Read` → `NOT_PERMITTED` dan field harga tidak ada di JSON (bukan `0`). 3. Tarif tidak ada di master → `NOT_ESTIMABLE` (`RWI-AC-335`). 4. `Nutrition` dan `Blood` → selalu `NOT_ESTIMABLE`. 5. Satu item gagal diresolusi tidak menggagalkan item lain. 6. `ItemIds` kosong → 400 (`VAL-RWF-66`). 7. Tidak ada penulisan data |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API dengan dua akun berbeda hak dan contoh berangka |
| **Risiko/pemilik** | Harga obat (`prescribing-drugs`) dan harga order tindakan (`patient-procedures`) memakai endpoint lama yang hak lihat harganya mengikuti hak baca, bukan hak membuat pesanan (12.13, sudah diketahui pemilik saat approval). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-164` — Pesanan gizi dan darah dari bangsal lewat adapter

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Kedelapan acceptance criteria terpetakan ke source. Build terintegrasi `dotnet build` `0 Error(s)`, 233 warning; memakai skema R10/R11 yang sudah diterapkan. Uji API/runtime `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Bukti: [laporan](../task/report/backend/BE-RWI-164.md) |
| **Outcome** | Dokter dan perawat bangsal memesan konsultasi gizi dan darah lewat Rawat Inap. Rawat Inap memeriksa penugasan dokter lebih dulu, menentukan dokter peminta dan status verifikasi, lalu meneruskan pesanan ke modul pemiliknya |
| **Requirement/decision** | `FR-RWF-031`, `032`, `036`, `038`; `RWI-DEC-171`, `RWI-DEC-188`; `INV-RWF-20`, `21`, `24`; `NFR-RWF-10`; `AC-RWF-032` s.d. `034`; `UAT-RWF-07`, `08`, `30`; `INT-RWF-16` |
| **Kontrak** | API 13.2 (`POST nutrition-consultations`, `POST blood-orders`, `POST blood-orders/confirm-duplicate`); validasi `VAL-RWF-60` s.d. `62`; backend 12.2, 12.5 |
| **Reuse** | `InpatientClinicalContextService.IsDoctorAssignedAsync`; `NutritionOrderService.CreateAsync` (`BE-RWI-161`); `BbkBloodOrderService.CreateAsync` (`BE-RWI-162`); controller dan adapter dari `BE-RWI-163` |
| **Cakupan** | Tiga endpoint dengan string permission modul tujuan. Dokter peminta = akun login bila yang memesan dokter; wajib dipilih bila bukan dokter. Status verifikasi `NotRequired` bila akun login dokter peminta, `Pending` bila bukan. `IdempotencyKey`. Gagal tertutup bila konteks penugasan tidak terbaca |
| **Dependency** | `BE-RWI-161`, `BE-RWI-162`, `BE-RWI-163` |
| **Acceptance criteria** | 1. Dokter memesan konsultasi gizi → tampil di `nutrition-management/orders`, peminta = akun dokter, `NotRequired` (`AC-RWF-032`, `UAT-RWF-07`). 2. Perawat memesan 2 PRC atas instruksi dokter jaga → tampil di Bank Darah `Pending`, penginput perawat (`AC-RWF-033`, `UAT-RWF-08`). 3. Perawat tanpa memilih dokter → 400 (`VAL-RWF-60`). 4. Dokter tanpa penugasan → 403 dan tidak ada pesanan di modul tujuan (`AC-RWF-034`, `UAT-RWF-30`). 5. Konteks penugasan tidak terbaca → pesanan darah ditolak (`INT-RWF-16`). 6. Pesanan mirip → alur `confirm-duplicate` Bank Darah. 7. Tidak ada baris tagihan saat pesanan dibuat (`INV-RWF-24`). 8. `IdempotencyKey` sama tidak membuat pesanan ganda |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis dengan modul Gizi dan Bank Darah sungguhan, runtime bila tersedia |
| **Risiko/pemilik** | Menyentuh modul milik Ikbal Yulianto dan Sukma Giri Pratama (disetujui `RWI-DEC-191`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

## Catatan implementasi backend — 5 Oktober 2026

User secara eksplisit meminta seluruh task dalam berkas ini dikerjakan sampai coding tuntas melalui build-module-backend, dengan larangan menjalankan dotnet build dan migration. Otorisasi ini berlaku pada backend dan dokumentasi hasilnya; bukan otorisasi frontend atau publikasi. Kelima task sudah diimplementasikan berurutan. Tanda 🟡 dipertahankan karena bukti build/database/API dan DoD penuh belum tersedia.

| Task | Status coding | Bukti |
| --- | --- | --- |
| BE-RWI-160 | Selesai; schema tidak berubah | [Laporan](../task/report/backend/BE-RWI-160.md) |
| BE-RWI-161 | Selesai; model/configuration R10 disiapkan | [Laporan](../task/report/backend/BE-RWI-161.md) |
| BE-RWI-162 | Selesai; model/configuration R11 disiapkan | [Laporan](../task/report/backend/BE-RWI-162.md) |
| BE-RWI-163 | Selesai; endpoint coverage read-only | [Laporan](../task/report/backend/BE-RWI-163.md) |
| BE-RWI-164 | Selesai; adapter Gizi/darah bangsal | [Laporan](../task/report/backend/BE-RWI-164.md) |

Validasi: QBE Strict ExplicitFiles 25 source PASS (0 VIOLATION, 0 REVIEW); parsing C# 25 berkas tanpa error; 59 pemeriksaan statis lulus, termasuk metadata 9 endpoint; review diff dan whitespace source PASS. Ini bukan hasil dotnet build atau pembuktian API/runtime.

Migration R10/R11 **belum dibuat/dijalankan**. Perubahan model berisi tiga kolom verifikasi pada GziNutritionOrder dan tiga pada BbkBloodOrder, default NotRequired=0, index status dan FK user Restrict. Snapshot dan migration 20261005033044_AddRawatInapFinishing dari pekerjaan sebelumnya tidak diubah; keberhasilan update database yang user laporkan untuk migration itu tidak mencakup perubahan model baru ini.

Hubungan task lain: GziInstructionVerificationStatus sudah dibuat oleh BE-RWI-161 untuk dipakai juga Keperawatan BE-RWI-166 (diet). Coding diet tidak termasuk roadmap ini. Gizi dan Bank Darah tetap pemilik order, transaksi dan audit. Integrasi frontend: FE-RWI-174 untuk pemesanan, FE-RWI-175 untuk verifikasi, FE-RWI-176 untuk katalog; coverage dipakai juga FE-RWI-172/173/177 dan Episode Rawat Inap FE-RWI-193. Tidak ada perubahan source frontend.

Delta internal: public route tetap sesuai kontrak, tetapi tiga controller digunakan agar metadata permission masing-masing cocok dengan InpatientEpisode, NutritionOrder dan BloodOrder. Parameter katalog nullable mempertahankan hasil caller lama di HEAD saat ini; konsumen poliklinik perlu setting Outpatient eksplisit. Worklist memakai NutritionOrderVerificationItem dan BloodOrderVerificationItem sebagai bentuk konkret OrderVerificationItem milik masing-masing modul. ClinicalNote darah mengikuti batas history 500 karakter; idempotensi memakai owner transaction/history dan deterministic order id tanpa kolom kunci tambahan. Rincian ada di laporan masing-masing; kontrak desain approved dan hash approval tidak ditulis ulang.

## Pembaruan ketersediaan migration R10/R11 — 5 Oktober 2026

Preferensi user diperjelas: file migration disertakan dalam implementasi schema supaya user cukup menjalankan `dotnet ef database update`. Pada pemeriksaan ulang, `20261005050735_AddNutritionAndBloodInstructionVerification.cs` dan Designer sudah tersedia di Migrations, beserta snapshot yang telah diperbarui. Tidak dibuat ulang oleh agent. Status "belum dibuat" pada catatan implementasi awal di atas kini merupakan riwayat; status terbaru **FILE TERSEDIA, penerapan database belum dibuktikan**.

Migration berisi enam kolom verifikasi GziNutritionOrder/BbkBloodOrder, empat index dan dua FK user Restrict; parsing sintaks tiga file serta pemeriksaan kesamaan Designer/snapshot dan scope Up/Down PASS. Dibandingkan target AddRawatInapFinishing, perubahan snapshot terbatas pada dua model order dan relasinya. Tidak menjalankan dotnet build, dotnet ef atau database update. Laporan BE-RWI-161, BE-RWI-162 dan BE-RWI-164 bagian 7.1 memuat bukti terbaru. Instruksi persiapan migration untuk task berikutnya dicatat dalam AGENTS.md.

## Pembaruan status — build, migration, dan penandaan selesai 5 Oktober 2026

Pengguna meminta kelima task ditandai selesai karena build dan migration sudah dilakukan. Build terintegrasi `dotnet build` `0 Error(s)` (233 warning, nol di berkas task ini) dan `dotnet ef database update` menerapkan R10/R11 (`20261005050735_AddNutritionAndBloodInstructionVerification`) ke database development; nol migration `Pending`. Setiap acceptance criteria dicocokkan ulang terhadap source pada HEAD `0a108994`. Uji API dan runtime `NOT RUN`, dikecualikan atas instruksi pengguna 5 Oktober 2026. Catatan bertanda 🟡 di atas dipertahankan sebagai riwayat.

| Task | Status | Laporan |
| --- | --- | --- |
| `BE-RWI-160` | ✅ — kriteria 4 terpenuhi di tingkat API; `UAT-RWF-31` poliklinik menunggu pemanggil poliklinik mengirim `careSetting=Outpatient` | [BE-RWI-160](../task/report/backend/BE-RWI-160.md) |
| `BE-RWI-161` | ✅ | [BE-RWI-161](../task/report/backend/BE-RWI-161.md) |
| `BE-RWI-162` | ✅ — dua error kompilasi diperbaiki saat build terintegrasi | [BE-RWI-162](../task/report/backend/BE-RWI-162.md) |
| `BE-RWI-163` | ✅ | [BE-RWI-163](../task/report/backend/BE-RWI-163.md) |
| `BE-RWI-164` | ✅ | [BE-RWI-164](../task/report/backend/BE-RWI-164.md) |
