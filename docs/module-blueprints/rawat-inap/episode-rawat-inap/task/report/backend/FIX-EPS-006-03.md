# FIX-EPS-006-03 - Sumber poli dan validasi asal

Tanggal: 2026-10-12. Status: SEBAGIAN - implementasi selesai, runtime API belum tersedia.

Masukan: [rencana disetujui](../../../docs/plan-repair/plan-repair-006-admisi-transfer-igd-rawat-jalan.md), [kontrak EPS-006 1.0](../../../contracts/admission-transfer-repair-006.md). Wewenang: pengguna meminta seluruh rencana dikerjakan sampai tuntas pada 12 Oktober 2026; backend MHamzah, frontend HamzahV2. Tidak mencakup penerapan migration, commit/push atau deployment.

## Perubahan dan source

Reader server menggabungkan disposisi IGD berkode RANAP dan CliDoctorCertificate InpatientReferral milik encounter rawat jalan. Daftar/detail/core memakai query kelayakan yang sama. Surat harus Issued aktif; konsumsi diturunkan dari relasi Registrasi. EmgVisit tidak dipalsukan untuk surat poli.

Berkas utama: Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.Queries.cs; Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionTransferController.cs; Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs. Daftar source frontend relatif terhadap repository QuilvianSystemFrontendDev; backend relatif terhadap NewQuilvianSystemBackend.

## Acceptance dan bukti

AC1/2/5: source filter/projection berbeda per asal. AC3/4: guard source/patient/status di client dan server. Eksekusi query EF terhadap PostgreSQL serta pembatalan konkuren belum diuji.

Validasi bersama: backend build konfigurasi Repair006 tanpa apphost: 0 error, 270 warning pada project; QBE strict WorkingTree: PASS, 0 violation. Frontend unit terarah: 28/28 PASS; build production PASS; lint:errors PASS; browser headed HTTP mock: 6/6 PASS. Bukti browser dan lint final ada di [FIX-08](../frontend/FIX-EPS-006-08.md).

## Governance dan dampak

Backend Governance Preflight: HealthServices/InPatientManagement Inp, RegistrationManagement Reg, BillingManagement Bil ACTIVE. Arketipe transaksi/worklist; tidak membuat master/ledger baru. QBE-SVC-001, API-001, PERM-001, VAL-001, DTO-001, TXN-001, PAGE-001, LOG-001, CFG-002, CODE-004. Owner service existing, metadata permission/API cocok; transaksi dan receipt coordinator dipertahankan.

Frontend UI gate: REUSE 5, COMPOSE 1, NEW 0; BaseButton, StatusBadge, InformationAlert, DataFilter, FilterSelect, Pagination dan langkah admisi existing. CSS dan perubahan user sebelumnya dipertahankan. Default deferCreation/transfer false menjaga jalur langsung; source transfer tidak memanggil generic episode writer.

AUTOMATED TEST: NOT APPLICABLE untuk backend - repository tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md). Pengujian frontend menggunakan runner node:test dan Playwright existing, tanpa dependency baru.

## Migration, keterbatasan dan handoff

Migration 20261012041938_AddAdmissionTransferOrigin + Designer/snapshot siap; Up hanya tiga kolom nullable, dua index dan satu FK Restrict. Database update tidak dijalankan. Belum ada bukti commit/rollback/konkurensi PostgreSQL, receipt nyata atau perjalanan pasien nyata. Setelah pemilik menerapkan migration pada database uji berwenang, jalankan matriks transaksi pada rencana bagian 8 dan baca ulang source, episode, penjamin, reservation serta movement deposit. Klaim selesai administratif baru sah sesudah consent tersimpan, terpisah dari arrival/departure fisik.
