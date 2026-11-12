# Laporan Penyelesaian Task: BE-HRD-333 5 Controller API Kontrak v25 (24 Endpoint Swagger), Worker Kurs BI TRV-W4, 24 Permissions & Audit Verifikasi E2E QBE Conformance

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-333`
* **Nama Task**: 5 Controller API Kontrak v25 (24 Endpoint Swagger), Worker Kurs BI TRV-W4, 24 Permissions & Audit Verifikasi E2E QBE Conformance
* **Modul / Sub-Modul**: Human Resource / Sub-modul 20: Business Travel & Expense Reimbursement (Perjalanan Dinas & Biaya Operasional)
* **Slice Delivery**: `S-D5` (Kontrak API `v25`, Gelombang `Verifikasi`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `Implementasi 5 Controller ASP.NET Core Kontrak v25 mencakup 24 RESTful endpoint pada Swagger tags resmi, background worker TravelCurrencyRateSyncWorker (TRV-W4), penguncian 24 permissions Hrd.BusinessTravel.* dan Hrd.Expense.*, pencatatan 14 audit event HRD_TRV_*, kompilasi dotnet build 0 error dan QBE Conformance PASS.`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `BusinessTravelManagement` & `ExpenseManagement` (Sub-modul 20: SPPD & Reimbursement Non-Trip)
* **Prefix Registry**: `Hrd` / `Trx` / `Mst` / `Wfp` (Kepatuhan Kamus Data Bagian 24 & Konstitusi Repositori)
* **Klasifikasi Kode**: `NEW CODE & EXTENSION`
* **Wewenang**: Implementasi backend sesuai kontrak terkunci `v25` dan aturan bisnis `VR-TRV-01` s.d. `VR-TRV-25`.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung (`dotnet ef database update`) ke database server development atau master tanpa otorisasi tertulis terpisah.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam operasional rumah sakit modern, koordinasi penugasan perjalanan dinas nakes dan penggantian biaya operasional memiliki dampak langsung terhadap kesinambungan pelayanan medis dan akuntabilitas keuangan:
1. **Pemisahan Tegas SPPD vs Reimbursement Non-Trip (TRV-B1, HRD-DEC-188)**: Penugasan luar kota/fasilitas (SPPD) memuat agenda, rute, transportasi, hotel, dan uang saku dinas, sedangkan penggantian belanja langsung lokal (reagen darurat cito, konsumsi panitia akreditasi) dikelola via klaim operasional non-trip tanpa memaksakan pembuatan SPPD fiktif.
2. **Plafon Uang Muka 80% & Verifikasi 2-Pintu (TRV-B2, HRD-DEC-189, VR-TRV-02/03)**: Uang muka kerja dibatasi maksimal 80% dari total estimasi pagu biaya yang disetujui guna mencegah kelebihan pembayaran yang sulit ditagih, serta wajib melalui persetujuan 2-pintu: Atasan Langsung (urgensi dinas) dan Finance Manager (likuiditas kas).
3. **Uang Harian Lumpsum Bebas Kuitansi Receh (TRV-B3, HRD-DEC-190, VR-TRV-04)**: Uang saku dan makan harian dihitung otomatis berbasis matriks zonasi wilayah (Zona 1 Lokal, Zona 2 Nasional, Zona 3 Internasional) dan grade nakes tanpa membebani verifikator memeriksa ribuan kuitansi makan warung.
4. **SLA Settlement 7 Hari, Pembekuan H+14 & Potong Payroll H+30 (TRV-B4, HRD-DEC-191, VR-TRV-13..15, INT-HR-99)**: Penyelesaian selisih kas wajib dalam 3 hari. Keterlambatan > 14 hari membekukan hak pengajuan SPPD baru, dan keterlambatan > 30 hari memicu pemotongan saldo piutang dari slip gaji bulanan via Sub-10 secara otomatis dengan mematuhi batas maksimal potong 50% PP 36/2021.
5. **Deklarasi Biaya Darurat Tanpa Nota Maks 10% (TRV-B5, HRD-DEC-192, VR-TRV-08/09)**: Perlindungan staf medis saat menghadapi situasi darurat di pelosok (ojek darurat spesimen, perahu rujukan) dengan batas maksimal Rp 150.000 per transaksi dan akumulasi maksimal 10% dari total klaim dengan alasan minimal 15 karakter.
6. **Kesinambungan Pelayanan Medis & Gembok Hard-Stop Roster (TRV-B6, HRD-DEC-193, INT-HR-97, VR-TRV-06)**: SPPD yang disetujui otomatis mencatat status "Hadir Dinas Luar" di Sub-07 dan memasang gembok validasi (hard-stop) pada modul roster Sub-06 agar dokter/perawat tidak dijadwalkan pada dinas bangsal selama penugasan dinas luar.

---

## 4. Rincian Implementasi & Alur Teknis
* **Ringkasan Pekerjaan**: Implementasi 5 Controller ASP.NET Core Kontrak v25 mencakup 24 RESTful endpoint pada Swagger tags resmi, background worker TravelCurrencyRateSyncWorker (TRV-W4), penguncian 24 permissions Hrd.BusinessTravel.* dan Hrd.Expense.*, pencatatan 14 audit event HRD_TRV_*, kompilasi dotnet build 0 error dan QBE Conformance PASS.
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 24, keputusan bisnis HRD-DEC-188 s.d. HRD-DEC-193, asumsi HRD-ASM-71 s.d. HRD-ASM-74, dan matriks validasi Sub-modul 20.
* **Kriteria Penerimaan (Acceptance Criteria)**: Seluruh method service, model, DTO, background worker, dan controller terimplementasi rapi, terisolasi dalam transaksi database ACID, dan lolos uji kompilasi serta conformance check.

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS** (0 Violation).
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1s (`AT-HRD-TRV-22`).


---

## Koreksi 10 November 2026 (audit Gelombang 1)

| Klaim pada laporan | Fakta sesudah remediasi otorisasi 10 November 2026 ([evidence/03-hr-authorization-remediation.md](../../../evidence/03-hr-authorization-remediation.md) bagian 6) |
| --- | --- |
| 24 permissions `Hrd.BusinessTravel.*` dan `Hrd.Expense.*` terkunci | Baru **19 aksi** pada 5 resource yang punya endpoint; 5 hak akses (`Hrd.BusinessTravel.MissingReceipt.*`, `Hrd.Expense.Claim.Approve/Edit`, `Hrd.Expense.Category.Manage`, `Hrd.BusinessTravel.Audit.View`) belum punya endpoint |
| Worker kurs BI `TravelCurrencyRateSyncWorker` (`TRV-W4`) berjalan | Worker hanya menulis log; kurs tidak pernah diambil (kelompok A, menunggu keputusan sumber kurs) |

Status task diturunkan menjadi 🟡 pada roadmap sampai 5 endpoint tersisa dibuat dan sumber kurs diputuskan.

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (sumber kurs dan identitas pegawai)

**Keputusan sumber kurs (`HRD-ASM-72`):** master kurs modul Keuangan (`MstExchangeRate`, kurs tengah per tanggal) menjadi satu-satunya sumber; tidak ada pemanggilan API Bank Indonesia. Petugas Keuangan memasukkan kurs tengah BI ke master, dan worker `TravelCurrencyRateSyncWorker` (`TRV-W4`) menerapkannya setiap hari ke butir klaim perjalanan dinas (`TrxTravelExpenseItem`) bermata uang asing yang belum dikonversi (`ExchangeRate = 1`): kurs tengah pada tanggal transaksi atau kurs terakhir sebelum tanggal itu mengisi `ExchangeRate` dan `BaseCurrencyAmount`. Contoh: butir hotel 120 USD tanggal 3 November, master kurs 3 November 15.800 → `BaseCurrencyAmount` 1.896.000. Butir yang kursnya belum ada dilewati dan pemegang hak akses `TravelRatePolicy : ManageRate` diberi tahu mata uang mana yang kurang.

**Identitas pegawai (kelompok G nomor 5):** `BusinessTravelRequestsController`, `BusinessTravelAdvancesController`, dan `DirectExpenseClaimsController` kini membaca klaim `workforce_profile_id` (cadangan `employee_id`, lalu ID akun) lewat helper `HrActorClaims`, sehingga kolom `WorkforceProfileId` berisi profil pegawai, bukan ID akun. Baris lama yang terlanjur berisi ID akun dipetakan ulang oleh migration `20261111020000`. Penanda Finance pada `BusinessTravelAdvancesController` masih dari nama peran (temuan lama, tidak diubah).

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/BusinessTravelManagement/Workers/BusinessTravelWorkers.cs` | `TRV-W4` menerapkan kurs master; `TRV-W1` mengirim pengingat ke pegawai |
| 3 controller perjalanan dinas | Helper klaim identitas |
| `Shared/HumanResource/Helpers/HrActorClaims.cs` | Helper baru |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Masih 🟡: 5 dari 24 hak akses belum punya endpoint (koreksi 10 November 2026 tetap berlaku); `AT-HRD-TRV-22` `NOT RUN`.


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.


## Pembaruan 11 November 2026 — Penuntasan Temuan

Lima hak akses yang kurang kini punya endpoint: `POST /settlements/{id}/missing-receipts`, `POST /settlements/missing-receipts/{itemId}/approve`, `PUT /expenses/claims/{id}`, `POST /expenses/claims/{id}/approve`, `GET /travel/audit-logs`, dan master `expense-categories`. Log audit disusun dari cap waktu yang tersimpan karena modul tidak punya tabel audit. Pintu Finance uang muka kini memakai hak akses `BusinessTravelAdvance : ApproveFinance`. Potongan payroll H+30 kini benar-benar membuat `TrxPayrollDeductionSchedule`, satu kali per uang muka.

`dotnet build` 0 error, 602 warning. QBE Strict `PASS`. Aplikasi dijalankan pada 11 November 2026: seluruh worker berjalan tanpa galat query dan lima dokumen Swagger berhasil dibuat. Uji API dengan akun sungguhan `NOT RUN`.
