# FIX-EPS-021-03 - Pembersihan service method getDepositMethods yang usang

Tanggal: 2026-10-15. Status: SELESAI - dead code removal.

Masukan: [rencana disetujui](../../../docs/plan-repair/plan-repair-021-peniadaan-input-metode-deposit-admisi-transfer.md), [laporan issue](../../../docs/issue/issue-021-peniadaan-input-metode-deposit-admisi-transfer.md). Wewenang: persetujuan perbaikan oleh Pemilik Sistem (User) pada 15 Oktober 2026; branch frontend HamzahV2.

## Perubahan dan source

Sebelumnya, `inpatientAdmissionTransferService` memuat method `getDepositMethods` yang memanggil endpoint `/v1/health-services/billing-management/master-data/payment-methods/options`. Karena proses bisnis rumah sakit menetapkan bahwa pemilihan metode pembayaran adalah wewenang loket Kasir/Billing dan bukan tanggung jawab loket Admisi (admisi hanya menetapkan `PlannedDepositAmount`), maka pemanggilan method tersebut di alur transfer tidak lagi relevan dan menjadi *dead code*.

Tindakan perbaikan:
1. Memperbarui `src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js`:
   - Menghapus method `getDepositMethods(config = {})`.
2. Melakukan audit audit referensi di seluruh codebase frontend untuk memastikan tidak ada komponen atau hook yang memanggil method yang dihapus tersebut. Hasil: 0 referensi tersisa.

Berkas utama:
- `QuilvianSystemFrontendDev/src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js`

## Acceptance dan bukti

| Kriteria Penerimaan | Status | Bukti Aktual |
| :--- | :---: | :--- |
| Method `getDepositMethods` dihapus dari service frontend | PASS | Deklarasi fungsi di `inpatient-admission-transfer.service.js` telah dibersihkan sepenuhnya |
| Tidak ada panggilan sisa / dangling reference di codebase | PASS | Pencarian teks pada seluruh codebase frontend menghasilkan 0 referensi pemanggil `getDepositMethods` |
| Seluruh unit test suite admisi dan deposit ranap tetap lulus | PASS | `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-deposit.test.mjs tests/unit/inpatient-admission-transfer.test.mjs`: 13 tests PASS, 0 fail. Durasi: 1.98s |

## Governance dan dampak

- Frontend Service Layer: Mengurangi ketergantungan lintas modul yang tidak perlu antara Admisi Ranap dan Master Data Metode Pembayaran Billing.
- Efisiensi: Menghilangkan potensi request HTTP yang tidak diperlukan saat proses admisi berlangsung.
