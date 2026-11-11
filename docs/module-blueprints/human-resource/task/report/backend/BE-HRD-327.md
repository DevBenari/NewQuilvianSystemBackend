# Laporan Penyelesaian Task: BE-HRD-327 Layanan Formulir Deklarasi Biaya Darurat Tanpa Kuitansi (MissingReceiptDeclarationService - TRV-B5, HRD-DEC-192, VR-TRV-08)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-327`
* **Nama Task**: Layanan Formulir Deklarasi Biaya Darurat Tanpa Kuitansi (MissingReceiptDeclarationService - TRV-B5, HRD-DEC-192, VR-TRV-08)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 20: Business Travel & Expense Reimbursement (Perjalanan Dinas & Biaya Operasional)
* **Slice Delivery**: `S-D5` (Kontrak API `v25`, Gelombang `MVP-65`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `Implementasi MissingReceiptDeclarationService yang mengawal deklarasi pengeluaran darurat lapangan tanpa nota (maksimal Rp 150.000 per transaksi dan total <= 10% dari total klaim, VR-TRV-08) dengan kewajiban alasan kronologi minimal 15 karakter (VR-TRV-09).`

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
* **Ringkasan Pekerjaan**: Implementasi MissingReceiptDeclarationService yang mengawal deklarasi pengeluaran darurat lapangan tanpa nota (maksimal Rp 150.000 per transaksi dan total <= 10% dari total klaim, VR-TRV-08) dengan kewajiban alasan kronologi minimal 15 karakter (VR-TRV-09).
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 24, keputusan bisnis HRD-DEC-188 s.d. HRD-DEC-193, asumsi HRD-ASM-71 s.d. HRD-ASM-74, dan matriks validasi Sub-modul 20.
* **Kriteria Penerimaan (Acceptance Criteria)**: Seluruh method service, model, DTO, background worker, dan controller terimplementasi rapi, terisolasi dalam transaksi database ACID, dan lolos uji kompilasi serta conformance check.

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS** (0 Violation).
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1s (`AT-HRD-TRV-12, AT-HRD-TRV-13`).
