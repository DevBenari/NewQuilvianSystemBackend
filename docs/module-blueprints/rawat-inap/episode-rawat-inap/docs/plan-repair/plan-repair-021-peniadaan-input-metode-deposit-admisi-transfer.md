# PLAN-REPAIR-021 — Penanganan Kegagalan Admisi Transfer Akibat Validasi Metode Deposit dan Pembersihan Kontrak Usang

```yaml
plan_id: PLAN-REPAIR-EPS-021
issue: ../issue/issue-021-peniadaan-input-metode-deposit-admisi-transfer.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-15"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-15"
basis_source_backend: "bcc663df321aabbc07888385605aa196c74cc6f3 (MHamzah)"
basis_source_frontend: "60f7764a08c032e2ead66c9ea744e44e2497f6b6 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Perbaikan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-EPS-021-01` | `ISS-EPS-021-01` | Rebuild dan restart server API backend agar runtime memuat perubahan `bcc663df` tanpa validasi `DEPOSIT_METHOD_REQUIRED` | Backend Runtime | 1 | `FIX-EPS-021-01` | ✅ SELESAI | [Laporan Task Backend](../task/report/backend/FIX-EPS-021-01.md) |
| `FIX-EPS-021-02` | `ISS-EPS-021-02` | Hapus properti `depositPaymentMethodId` dari helper pembentuk payload transfer frontend | Frontend | 2 | `FIX-EPS-021-02` | ✅ SELESAI | [Laporan Task Frontend](../task/report/frontend/FIX-EPS-021-02.md) |
| `FIX-EPS-021-03` | `ISS-EPS-021-03` | Bersihkan method `getDepositMethods` yang sudah usang dari service transfer rawat inap frontend | Frontend | 3 | `FIX-EPS-021-03` | ✅ SELESAI | [Laporan Task Frontend](../task/report/frontend/FIX-EPS-021-03.md) |

**Ringkasan: 3 dari 3 perbaikan selesai (100%).**

---

## 2. Solusi Terpilih Per Temuan

### ISS-EPS-021-01 — Sinkronisasi Runtime Server Backend dengan Perubahan Domain

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Mengembalikan field input metode pembayaran deposit pada tampilan frontend | Mengelabui validasi backend yang lama | Melanggar proses bisnis rumah sakit; petugas admisi dipaksa memilih instrumen kasir |
| **B** | Mematikan proses backend lama, melakukan kompilasi bersih (`dotnet build`), dan menjalankan ulang API server | Sesuai 100% dengan prinsip domain: admisi hanya mencatat rencana deposit (`PlannedDepositAmount`), penarikan uang dilakukan di Kasir | Mengharuskan restart service backend lokal |

**Solusi Terpilih: Opsi B.**  
Secara kode sumber, commit `bcc663df` sudah menghapus kewajiban `DepositPaymentMethodId`. Solusi yang benar adalah menyelaraskan runtime lokal melalui kompilasi bersih dan me-restart server backend, bukan merusak antarmuka pengguna untuk mengakomodasi binary kedaluwarsa.

---

### ISS-EPS-021-02 — Pembersihan Properti `depositPaymentMethodId` pada Payload Frontend

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Tetap mengirim `"depositPaymentMethodId": null` pada payload JSON | Tidak mengubah pemanggil | Mempertahankan properti sampah (*dead property*) pada payload jaringan |
| **B** | Bersihkan properti `depositPaymentMethodId` dari payload kembalian `buildTransferAdmissionPayload` | Payload bersih, ringkas, dan mencerminkan kontrak bahwa admisi tidak mengurusi metode pembayaran | Tidak ada |

**Solusi Terpilih: Opsi B.**  
Hapus properti `depositPaymentMethodId` dari objek hasil kembalian `buildTransferAdmissionPayload` pada berkas `src/utils/health-services/inpatient-management/inpatient-admission-transfer-utils.js`.

---

### ISS-EPS-021-03 — Pembersihan Service Method `getDepositMethods`

Hapus fungsi `getDepositMethods` dari `inpatient-admission-transfer.service.js` karena seluruh alur admisi transfer maupun admisi umum kini mencatat uang muka murni sebagai nominal rencana, tanpa perlu mengambil opsi metode pembayaran kasir.

---

## 3. Skema Tampilan Sebelum → Sesudah

Perbaikan ini **tidak mengubah tata letak antarmuka visual** yang sudah disetujui pada langkah deposit, melainkan memulihkan keberhasilan alur pada Langkah 7 (Booking Bed).

### 3.1 Skema Langkah 7: Booking Bed pada Admisi Transfer Pasien

**SEBELUM (Kondisi Gagal Saat Ini):**
```text
+---------------------------------------------------------------------------------+
| LANGKAH 7 DARI 10   Booking Bed                                      70% Selesai|
| [✓ Deposit]   [✓ Dokter]   [✓ Pilih Bed]   (• Booking Bed)   [ Konfirmasi ] ... |
+---------------------------------------------------------------------------------+
|                                                                                 |
|  +---------------------------------------------------------------------------+  |
|  | ⊗ Gagal Memuat Permintaan Rawat Inap                                      |  |
|  |   Pilih metode penerimaan deposit.                                        |  |
|  +---------------------------------------------------------------------------+  |
|                                                                                 |
|                       [ Periksa Ulang ]   [ Kembali ke Antrean ]                |
+---------------------------------------------------------------------------------+
```

**SESUDAH (Setelah Runtime Backend Disinkronkan):**
```text
+---------------------------------------------------------------------------------+
| LANGKAH 7 DARI 10   Booking Bed                                      70% Selesai|
| [✓ Deposit]   [✓ Dokter]   [✓ Pilih Bed]   (• Booking Bed)   [ Konfirmasi ] ... |
+---------------------------------------------------------------------------------+
|                                                                                 |
|  +---------------------------------------------------------------------------+  |
|  | ℹ Memproses Pemesanan Tempat Tidur...                                     |  |
|  |   Menghubungkan episode transfer dan mengunci tempat tidur rawat inap.    |  |
|  +---------------------------------------------------------------------------+  |
|                                                                                 |
| (API POST /admission-transfers/admit mengembalikan HTTP 201 Created)            |
| (Sistem otomatis beralih mulus ke LANGKAH 8: Konfirmasi Admisi)                 |
+---------------------------------------------------------------------------------+
```

Tabel Komponen dan Status Layar:

| Wilayah | Konten / Komponen | Perilaku Saat Sukses |
| :--- | :--- | :--- |
| **Notifikasi Status** | Banner Peringatan | Banner merah galat hilang; digantikan status transisi sukses |
| **Aksi Langkah 7** | Eksekusi API `admit` | Payload dikirim tanpa `depositPaymentMethodId`; backend menyimpan `PlannedDepositAmount = 1000000` dan mengembalikan detail reservasi bed |
| **Navigasi Stepper** | InpatientAdmissionStepper | Berpindah otomatis ke Langkah 8 (Konfirmasi) untuk persetujuan pasien / wali |

---

## 4. Rincian Perbaikan

### FIX-EPS-021-01 — Rebuild dan Restart Server API Backend

| Parameter | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-EPS-021-01` |
| **Area** | Backend Runtime |
| **Jenis Perubahan** | Operasional Lingkungan / Build |
| **Berkas Terkait** | `NewQuilvianSystemBackend/bin/Debug/net9.0/QuilvianSystemBackend.dll` |
| **Radius Dampak** | Seluruh pemanggilan endpoint transfer admisi `/v1/health-services/inpatient-management/admission-transfers/admit` |
| **Bergantung Pada** | Tidak ada |

**Langkah Pengerjaan:**
1. Hentikan proses backend `dotnet run` atau IIS Express / Kestrel yang sedang berjalan.
2. Jalankan perintah kompilasi bersih:
   ```powershell
   dotnet build NewQuilvianSystemBackend/QuilvianSystemBackend.csproj -c Debug
   ```
3. Pastikan berkas `QuilvianSystemBackend.dll` terperbarui ke timestamp terbaru.
4. Jalankan kembali aplikasi server backend:
   ```powershell
   dotnet run --project NewQuilvianSystemBackend/QuilvianSystemBackend.csproj
   ```

**Acceptance Criteria:**
1. Permintaan `POST /api/v1/health-services/inpatient-management/admission-transfers/admit` dengan nominal `depositAmount: 1000000` dan tanpa `depositPaymentMethodId` menghasilkan respon **HTTP 201 Created** (bukan 422).
2. Nilai `PlannedDepositAmount` pada baris tabel `InpEpisode` terisi Rp 1.000.000.
3. Tidak ada pembuatan mutasi kasir prematur; seluruh mutasi kasir ditunda hingga pasien diproses di loket billing.

**Verifikasi:**
Kirim ulang permintaan admit melalui antarmuka web admisi rawat inap dan pastikan proses berhasil melangkah ke Langkah 8 (Konfirmasi).

---

### FIX-EPS-021-02 — Pembersihan Properti Usang pada Helper Payload Transfer Frontend

| Parameter | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-EPS-021-02` |
| **Area** | Frontend |
| **Jenis Perubahan** | Source Frontend |
| **Berkas yang Diubah** | `src/utils/health-services/inpatient-management/inpatient-admission-transfer-utils.js` |
| **Radius Dampak** | Pembentukan payload admisi transfer IGD dan rawat jalan |
| **Bergantung Pada** | `FIX-EPS-021-01` |

**Langkah Pengerjaan:**
1. Buka `src/utils/health-services/inpatient-management/inpatient-admission-transfer-utils.js`.
2. Pada fungsi `buildTransferAdmissionPayload`, hapus parameter dan properti `depositPaymentMethodId`:
   ```javascript
   // Sebelum:
   export const buildTransferAdmissionPayload = ({ source, patientId, form,
     paymentCategory, selectedPayer, depositAmount = 0, depositPaymentMethodId, bed } = {}) => {
     ...
     return {
       ...
       depositAmount, depositPaymentMethodId: depositPaymentMethodId || null,
       ...
     };
   };

   // Sesudah:
   export const buildTransferAdmissionPayload = ({ source, patientId, form,
     paymentCategory, selectedPayer, depositAmount = 0, bed } = {}) => {
     ...
     return {
       ...
       depositAmount,
       ...
     };
   };
   ```

**Acceptance Criteria:**
1. Payload yang dikirimkan ke endpoint `/admit` tidak lagi memuat properti `depositPaymentMethodId`.
2. Unit test `inpatient-transfer-repair-006.test.mjs` tetap berjalan lulus (100% pass).

---

### FIX-EPS-021-03 — Pembersihan Method `getDepositMethods` pada Service Frontend

| Parameter | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-EPS-021-03` |
| **Area** | Frontend |
| **Jenis Perubahan** | Source Frontend / Cleanup |
| **Berkas yang Diubah** | `src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js` |
| **Radius Dampak** | Modul service transfer rawat inap |
| **Bergantung Pada** | `FIX-EPS-021-02` |

**Langkah Pengerjaan:**
1. Hapus deklarasi method `getDepositMethods` pada `inpatientAdmissionTransferService`.
2. Pastikan tidak ada referensi impor atau pemanggilan method tersebut di seluruh repository frontend.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-021-01 (Kompilasi ulang dan restart backend server)
└── FIX-EPS-021-02 (Pembersihan properti payload frontend)
    └── FIX-EPS-021-03 (Pembersihan service method usang)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan Solusi | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :---: |
| **K-01** | Kebijakan properti `DepositPaymentMethodId` pada DTO backend | Hapus sepenuhnya / Pertahankan sebagai nullable `Guid?` | Pertahankan sebagai `Guid?` opsional di backend agar backward compatible, hapus dari pengiriman frontend | `FIX-EPS-021-02` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| :--- | :--- | :--- | :--- |
| `02-backend-architecture.md` | Bagian Alur Deposit Transfer | "Menerima uang muka dan metode pembayaran di admisi" | "Loket admisi hanya mencatat rencana deposit (`PlannedDepositAmount`); penyetoran dan metode pembayaran dikelola di loket Kasir/Billing." |
| `contracts/api-contract.md` | Spesifikasi `POST /admission-transfers/admit` | `depositPaymentMethodId: Guid (Wajib jika deposit > 0)` | `depositPaymentMethodId: Guid (Opsional/Deprecated - tidak diproses)` |

---

## 8. Verifikasi Menyeluruh

1. **Uji Skenario Transfer Pasien dengan Deposit:**
   - Masuk ke menu *Admisi Rawat Inap* > Tab *Admisi Transfer*.
   - Pilih satu pasien rujukan IGD (contoh: Tn. Budi, trauma tumpul abdomen).
   - Pada Langkah 1 (Deposit), masukkan nominal Rp 1.000.000. Tidak ada input metode pembayaran.
   - Lanjutkan hingga Langkah 7 (Booking Bed). Pilih bed dan klik Booking.
   - **Hasil:** Berhasil tanpa galat 422, langsung beralih ke Langkah 8 (Konfirmasi Admisi).
2. **Pemeriksaan Database:**
   - Periksa tabel `InpEpisode` untuk episode yang baru terbentuk: kolom `PlannedDepositAmount` bernilai `1000000.00`.
   - Tidak ada transaksi pembayaran fiktif di tabel `BilDepositMovement` sebelum kasir memprosesnya.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| **2026-10-15** | Rencana perbaikan dibuat untuk sinkronisasi runtime backend dan pembersihan kontrak usang deposit transfer | Antigravity AI |
