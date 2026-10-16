# FIX-EPS-021-02 - Pembersihan properti depositPaymentMethodId pada payload transfer frontend

Tanggal: 2026-10-15. Status: SELESAI - payload helper & unit test.

Masukan: [rencana disetujui](../../../docs/plan-repair/plan-repair-021-peniadaan-input-metode-deposit-admisi-transfer.md), [laporan issue](../../../docs/issue/issue-021-peniadaan-input-metode-deposit-admisi-transfer.md). Wewenang: persetujuan perbaikan oleh Pemilik Sistem (User) pada 15 Oktober 2026; branch frontend HamzahV2.

## Perubahan dan source

Sebelum perbaikan, helper `buildTransferAdmissionPayload` di frontend masih menerima parameter `depositPaymentMethodId` dan menyertakannya di payload kembalian:
`depositPaymentMethodId: depositPaymentMethodId || null`

Meskipun field metode pembayaran di antarmuka admisi sudah ditiadakan (karena admisi hanya mencatat nominal rencana deposit bagi loket kasir), penyertaan field null tersebut menimbulkan ketidakselarasan dengan kontrak antarmuka baru dan mempertahankan properti usang (*dead property*).

Tindakan perbaikan:
1. Memperbarui `src/utils/health-services/inpatient-management/inpatient-admission-transfer-utils.js`:
   - Menghapus parameter `depositPaymentMethodId` dari fungsi `buildTransferAdmissionPayload`.
   - Menghapus properti `depositPaymentMethodId` dari objek payload yang dihasilkan (hanya menyisakan `depositAmount`).
2. Memperbarui unit test `tests/unit/inpatient-transfer-repair-006.test.mjs`:
   - Menyelaraskan asersi uji agar memastikan `payload.depositPaymentMethodId` bernilai `undefined`.

Berkas utama:
- `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-admission-transfer-utils.js`
- `QuilvianSystemFrontendDev/tests/unit/inpatient-transfer-repair-006.test.mjs`

## Acceptance dan bukti

| Kriteria Penerimaan | Status | Bukti Aktual |
| :--- | :---: | :--- |
| Payload admisi transfer tidak lagi memuat `depositPaymentMethodId` | PASS | Objek kembalian `buildTransferAdmissionPayload` hanya memuat `depositAmount` |
| Seluruh unit test suite transfer admisi ranap berhasil (100% pass) | PASS | `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-transfer-repair-006.test.mjs`: 7 tests PASS, 0 fail. Durasi: 184ms |
| Asersi uji deposit positif tanpa metode pembayaran lulus | PASS | `✔ deposit positif menerima nominal dan versi bed sah tanpa mewajibkan metode penerimaan (0.2367ms)` |

## Governance dan dampak

- Frontend Governance: Menggunakan helper dan utility standar Quilvian.
- Tidak ada perubahan layout atau komponen UI visual; hanya pembersihan struktur data transfer payload pada Langkah 7 (Booking Bed).
- Payload transfer menjadi bersih dan konsisten dengan DTO backend terkini.
