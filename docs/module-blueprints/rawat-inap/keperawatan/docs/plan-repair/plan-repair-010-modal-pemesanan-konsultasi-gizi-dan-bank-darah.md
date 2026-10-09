# PLAN-REPAIR-010 — Implementasi Modal Formulir Pemesanan Konsultasi Gizi dan Bank Darah pada Penunjang Medis

```yaml
plan_id: PLAN-REPAIR-KEP-010
issue: ../issue/issue-010-modal-pemesanan-konsultasi-gizi-dan-bank-darah.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pengguna (Perintah Langsung: Konsultasi Gizi dan bank darah gunakan tampilan modal untuk menambahkan (create))"
tanggal_keputusan: "2026-10-06"
basis_source_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
basis_source_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-KEP-010-01` | `ISS-KEP-010-01` | Pembuatan komponen `NutritionOrderModal` (size="lg") dan pengalihan pembuatan asuhan gizi dari form inline ke modal pop-up | Frontend | 1 | `FE-KEP-15` | ✅ SELESAI | `nutrition-order-modal.jsx`, `nursing-ancillary-section.jsx`, unit test 4/4 PASS |
| `FIX-KEP-010-02` | `ISS-KEP-010-02` | Pembuatan komponen `BloodBankOrderModal` (size="lg") dan pengalihan pemesanan darah dari form inline ke modal pop-up | Frontend | 1 | `FE-KEP-15` | ✅ SELESAI | `blood-bank-order-modal.jsx`, `nursing-ancillary-section.jsx`, unit test 4/4 PASS |
| `FIX-KEP-010-03` | `ISS-KEP-010-T1` | Integrasi pemanggilan modal gizi dan darah pada riwayat pesanan ruang kerja dokter | Frontend | 2 | `FE-RWI-145` | ✅ SELESAI | `supporting-service-tab.jsx`, unit test 24/24 PASS |

**Ringkasan: 3 dari 3 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-KEP-010-01 — Formulir Konsultasi Gizi Memadati Halaman

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Buat komponen modal mandiri `NutritionOrderModal` (`react-bootstrap/Modal` size `lg`) yang dipanggil melalui tombol `+ Buat Pesanan Baru` pada header tabel riwayat `SupportingHistorySection`. | Layar utama bersih menyajikan riwayat pesanan dan status diet; form isian tampil lapang dan fokus saat dibutuhkan. | Memerlukan pembuatan berkas komponen modal baru. |
| B | Buat akordeon (*collapsible accordion*) untuk melipat form inline. | Tidak membuat modal baru. | Tetap memakan ruang vertikal dan tidak konsisten dengan pola Hemodialisa. |

**Solusi terpilih: Opsi A.**
Konsisten 100% dengan pola modal hemodialisa (`HemodialysisOrderModal`) yang sudah baku dan disukai pengguna.

---

### ISS-KEP-010-02 — Formulir Bank Darah Menutupi Panel Pemantauan Transfusi

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Buat komponen modal `BloodBankOrderModal` (`react-bootstrap/Modal` size `lg`) lengkap dengan repeater komponen darah, estimasi coverage, dan modal konfirmasi duplikasi `ConfirmModal`. | Panel pemantauan reaksi transfusi (`NursingTransfusionMonitoringPanel`) langsung terlihat di bawah tabel riwayat darah tanpa terdorong ke bawah. | Memerlukan koordinasi state duplikasi di dalam modal. |
| B | Pindahkan form darah ke tab terpisah. | Memisahkan form dari tabel. | Menambah kompleksitas hirarki navigasi sub-tab. |

**Solusi terpilih: Opsi A.**
Memberikan pengalaman pemesanan darah yang aman, cepat, dan tidak mengganggu alur pemantauan tanda vital transfusi perawat.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Tampilan Sebelum (Formulir Inline Memadati Layar)

```text
[ Tab: Konsultasi Gizi ]
+-----------------------------------------------------------------------------------------+
| [🥗] Formulir Konsultasi Asuhan Gizi & Diet Pasien           [⚡ Terhubung Instalasi Gizi] |
| Jenis Permintaan: [ Konsultasi Asuhan Gizi Klinis ▾]  Prioritas: [⏳ Rutin] [⚡ Urgent]  |
| Dokter Peminta:   [ dr. Rendy Pangalila          ]   Penginput: [ Ns. Siti Aminah     ]  |
| Diagnosa Pasien:  [ E11.9: DM Tipe 2 Tanpa Komplikasi ] Riwayat Alergi: [ Tidak ada  ]  |
| Alasan Rujukan & Rencana Asuhan Nutrisi: *                                               |
| [ Tuliskan indikasi rujukan gizi, target kebutuhan kalori, pembatasan diet...        ]  |
|                                                     [ Batal ]  [ Kirim Permintaan Gizi ]|
+-----------------------------------------------------------------------------------------+
| Riwayat Pesanan Konsultasi Gizi                                                         |
| [ Tabel Riwayat terdorong jauh ke bawah halaman... ]                                    |
+-----------------------------------------------------------------------------------------+
```

### Tampilan Sesudah (Bersih dengan Aksi Modal Pop-up)

```text
[ Tab: Konsultasi Gizi ]
+-----------------------------------------------------------------------------------------+
| Konsultasi Gizi (3 pesanan)                                     [ + Buat Pesanan Baru ] |
| Daftar permintaan asuhan gizi dan diet pasien selama perawatan rawat inap.             |
+-----------------------------------------------------------------------------------------+
| No. Permintaan | Waktu Permintaan | Jenis Asuhan | Status Verifikasi | Dokter / Dietisien |
| GZ-2026-00012  | 06/10/2026 09:30 | Diet Rendah Garam | Terverifikasi | dr. Rendy / Gz. Ani|
| GZ-2026-00008  | 04/10/2026 14:15 | Konseling DM      | Selesai       | dr. Rendy / Gz. Ani|
+-----------------------------------------------------------------------------------------+

Saat menekan [ + Buat Pesanan Baru ], muncul modal pop-up elegan:
+-----------------------------------------------------------------------------------------+
| [🥗] Formulir Permintaan Konsultasi Asuhan Gizi                                     [×] |
| Pemesanan asuhan gizi dan diet pasien ke Instalasi Gizi dengan konteks rawat inap.      |
+-----------------------------------------------------------------------------------------+
| [ Ringkasan Pasien Rawat Inap & DPJP Terkunci ]                                         |
| Status Prioritas: [  o] Rutin                                                           |
| Jenis Permintaan: [ Konsultasi Asuhan Gizi Klinis ▾] Dokter: [ dr. Rendy Pangalila   ▾] |
| Diagnosa SOAP:    [ E11.9: DM Tipe 2            ]   Alergi:  [ Tidak ada alergi      ]  |
| Pilihan Cepat: [+ Diet Rendah Garam] [+ Diet DM] [+ Diet Rendah Protein] [+ Malnutrisi] |
| Alasan Rujukan: *                                                                       |
| [ Evaluasi dan penyusunan diet rendah garam untuk manajemen hipertensi...             ] |
+-----------------------------------------------------------------------------------------+
|                                                      [   Batal   ]  [ Kirim Permintaan ]|
+-----------------------------------------------------------------------------------------+
```

---

## 4. Rincian Perbaikan

### FIX-KEP-010-01 — Pembuatan NutritionOrderModal dan Refaktor Sub-tab Konsultasi Gizi

- **Berkas Baru:** `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/nutrition-order-modal.jsx`
- **Berkas Diubah:** `src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`
- **Hasil:** Form inline dihapus; header `SupportingHistorySection` kini menampilkan tombol `+ Buat Pesanan Baru` yang membuka modal form konsultasi gizi lengkap dengan preset pilihan cepat.

### FIX-KEP-010-02 — Pembuatan BloodBankOrderModal dan Refaktor Sub-tab Bank Darah

- **Berkas Baru:** `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/blood-bank-order-modal.jsx`
- **Berkas Diubah:** `src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`
- **Hasil:** Form inline dihapus; tabel riwayat darah dan panel pemantauan transfusi darah langsung tampil di halaman utama, sementara pembuatan pesanan darah dibuka dalam modal pop-up berfitur lengkap (repeater komponen, estimasi harga, konfirmasi duplikasi).

### FIX-KEP-010-03 — Penyelarasan Pemesanan Penunjang pada Ruang Kerja Dokter

- **Berkas Diubah:** `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx`
- **Hasil:** Dokter yang meninjau riwayat gizi atau darah dapat membuka modal pop-up langsung melalui tombol `+ Buat Pesanan Baru`, tanpa harus beralih sub-tab.

---

## 5. Verifikasi dan Pengujian

1. **Unit Test Spesifik Modal (`inpatient-ancillary-order-modals.test.mjs`):**
   - 4 skenario uji lulus 100% (integritas berkas, struktur modal gizi, struktur modal darah, pengikatan modal di nursing section).
2. **Unit Test Regresi Modul Penunjang:**
   - `inpatient-supporting-service-modernisasi.test.mjs`: 7/7 PASS.
   - `inpatient-nursing-procedure-and-ancillary.test.mjs`: 6/6 PASS.
   - `hemodialysis-inpatient-order.test.mjs`: 7/7 PASS.
   - Total pengujian: 24/24 PASS (0 failure).
3. **Pemeriksaan Linter:**
   - 0 error, 0 warning pada seluruh berkas baru dan modifikasi.

---

## 6. Riwayat

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-06 | Rencana perbaikan dibuat dan diimplementasikan secara tuntas; NutritionOrderModal & BloodBankOrderModal terintegrasi di perawat dan dokter. Status: SELESAI. | Antigravity Builder |
