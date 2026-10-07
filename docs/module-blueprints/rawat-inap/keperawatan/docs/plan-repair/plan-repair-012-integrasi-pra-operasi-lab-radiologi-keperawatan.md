# PLAN-REPAIR-012 — Integrasi Catatan Pra-Operasi Bangsal dan Modal Pemesanan Delegatif Laboratorium & Radiologi pada Modul Keperawatan

```yaml
plan_id: PLAN-REPAIR-KEP-012
issue: ../issue/issue-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-07"
diputuskan_oleh: "Pemilik Produk"
tanggal_keputusan: "2026-10-07"
basis_source_backend: "671191eb (MHamzah)"
basis_source_frontend: "1f889d67c (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| `FIX-KEP-012-01` | `ISS-KEP-012-01`, `ISS-KEP-012-T1` | Konsol status kasus bedah dan integrasi drawer Catatan Pra-Operasi bangsal pada sub-tab Catatan Pra-Operasi | FE | 1 | — | ✅ SELESAI | [`../../task/report/frontend/FIX-KEP-012-01.md`](../../task/report/frontend/FIX-KEP-012-01.md) |
| `FIX-KEP-012-02` | `ISS-KEP-012-02` | Modal formulir pemesanan Laboratorium khusus perawat berdelegasi DPJP (`NursingLabOrderModal`) | FE | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-KEP-012-03` | `ISS-KEP-012-02` | Modal formulir pemesanan Radiologi khusus perawat berdelegasi DPJP (`NursingRadiologyOrderModal`) | FE | 1 | — | BELUM DIKERJAKAN | — |

**Ringkasan: 1 dari 3 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-KEP-012-01 — Sub-tab Catatan Pra-Operasi Menampilkan Stub "Integrasi belum tersedia"

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Ganti komponen stub di sub-tab `pre-op` dengan panel yang memuat daftar kasus operasi pasien pada episode ini, lengkap dengan status persiapan dan tombol aksi langsung untuk membuka `WardPreOpDrawer`. Jika belum ada jadwal operasi, sediakan tombol pintas menuju *Pemesanan Ruang Bedah*. | Perawat dapat melihat status dan mengisi checklist pra-operasi langsung dari menu Catatan Keperawatan tanpa harus pindah menu; menjaga keterikatan data `caseId` yang valid. | Membutuhkan pemanggilan servis kasus operasi rawat inap pada tab tersebut. |
| B | Hapus sub-tab `pre-op` dari menu Catatan Keperawatan dan arahkan perawat seluruhnya ke Menu 7 (*Ruangan Bedah*). | Struktur kode lebih ringkas tanpa duplikasi titik akses. | Membingungkan perawat yang mencari checklist asuhan di menu catatan keperawatan; mengubah arsitektur menu V2 yang sudah disepakati (`FE-RWI-180`). |
| C | Alihkan rute URL secara otomatis (*redirect*) dari `?tab=pre-op` langsung ke `?section=surgery-booking`. | Tidak perlu membuat komponen tampilan baru. | Menimbulkan efek layar melompat (*jarring UX*) yang membingungkan perawat; berpindah menu secara tiba-tiba tanpa persetujuan pengguna. |

**Solusi terpilih: Opsi A.**
1. Memenuhi prinsip kemudahan kerja perawat: checklist pra-operasi adalah bagian dari asuhan bangsal, sehingga perawat dapat mengaksesnya dari sub-tab Catatan Pra-Operasi.
2. Memakai ulang komponen `WardPreOpDrawer` yang sudah stabil di modul Bedah Sentral tanpa menulis ulang logika bisnis atau model backend baru.

*Kenapa bukan opsi lain:* Opsi B merusak kelengkapan 7 sub-tab catatan asuhan yang sudah dibakukan. Opsi C menyebabkan perpindahan menu otomatis yang membingungkan alur kerja klinis.

---

### ISS-KEP-012-02 — Tombol Pemesanan Laboratorium dan Radiologi Dinonaktifkan (`disabled`)

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Bangun dua modal formulir terpisah (`NursingLabOrderModal` dan `NursingRadiologyOrderModal`) yang mewajibkan pemilihan Dokter Pemberi Instruksi (`InstructingDoctorId`), lalu hubungkan tombol `+ Pesan Laboratorium` dan `+ Pesan Radiologi`. | Sangat aman secara tata kelola klinis; sesuai penuh dengan kontrak backend `BE-RWI-104`; konsisten dengan modal Gizi, Darah, dan HD yang sudah ada di sistem. | Perlu membangun dua komponen modal baru di frontend. |
| B | Buka tombol pemesanan tanpa mewajibkan pemilihan dokter pemberi instruksi (mengirim pesanan atas nama akun perawat saja). | Implementasi frontend sangat cepat. | **Ditolak keras oleh backend**: Server akan melempar galat `400 / 403` karena akun perawat tanpa tautan DPJP wajib mengisi instruksi; melanggar aturan tata kelola klinis rumah sakit. |
| C | Tampilkan pesan dialog pop-up yang menginstruksikan perawat untuk meminta dokter membuka akun dokter guna melakukan pemesanan. | Nol perubahan logika transaksi. | Tidak menyelesaikan masalah di lapangan ketika DPJP memberikan instruksi lisan atau telepon; membebani dokter untuk login berulang kali. |

**Solusi terpilih: Opsi A.**
1. Mengikuti pola yang telah terbukti sukses pada Konsultasi Gizi (`NutritionOrderModal`) dan Bank Darah (`BloodBankOrderModal`).
2. Sepenuhnya selaras dengan migrasi database dan controller backend `BE-RWI-104` yang telah diterapkan pada branch `MHamzah`.

*Kenapa bukan opsi lain:* Opsi B pasti gagal saat validasi backend dan melanggar hukum rekam medis. Opsi C mempertahankan inefisiensi alur kerja bangsal rawat inap.

---

## 3. Skema Tampilan Sebelum → Sesudah

### A. Sub-tab Catatan Pra-Operasi (Catatan Keperawatan)

#### SEBELUM
```text
+--------------------------------------------------------------------------------------------------+
| Catatan Keperawatan > Catatan Pra-Operasi                                                        |
+--------------------------------------------------------------------------------------------------+
|                                                                                                  |
| [ ⚠ Integrasi belum tersedia ]                                                                   |
|                                                                                                  |
| Catatan Pra-Operasi                                                                              |
| Catatan pra-operasi dijadwalkan pada rilis mendatang.                                            |
| Integrasi belum tersedia dan tidak ada permintaan jaringan.                                     |
|                                                                                                  |
+--------------------------------------------------------------------------------------------------+
```

#### SESUDAH
```text
+--------------------------------------------------------------------------------------------------+
| Catatan Keperawatan > Catatan Pra-Operasi                                                        |
+--------------------------------------------------------------------------------------------------+
| Informasi: Persiapan pra-operasi bangsal terhubung dengan jadwal kamar operasi aktif pasien.     |
|                                                                                                  |
| Daftar Jadwal Operasi Pasien Ini:                                                                |
| +----------------------------------------------------------------------------------------------+ |
| | No. Kasus       | Tindakan & DPJP Bedah        | Jadwal Operasi    | Status      | Aksi      | |
| |-----------------+------------------------------+-------------------+-------------+-----------| |
| | OPR-2609-0012   | Apendektomi Terbuka          | 08 Okt 2026 09:00 | Terjadwal   | [ Buka    | |
| |                 | DPJP: dr. Budi Santoso, Sp.B | Kamar OK-02       | Pra-Op: Draf|   Pra-Op ]| |
| +----------------------------------------------------------------------------------------------+ |
|                                                                                                  |
| * Jika pasien belum memiliki jadwal operasi:                                                     |
| [ Belum ada pesanan kamar bedah untuk episode ini. ]                                             |
| [ Buat Pemesanan Ruang Bedah di Menu Ruangan Bedah → ]                                           |
+--------------------------------------------------------------------------------------------------+
```

##### Spesifikasi Tampilan Sesudah:
- **Wilayah**:
  - Panel Ringkasan Pra-Bedah: menampilkan daftar kasus operasi aktif pasien yang diambil via `inpatientSurgeryBookingService.getCasesByEncounter(encounterId)`.
  - Empty State: jika belum ada pesanan OK, menampilkan banner edukatif dan tombol pintas menuju Menu 7 (*Ruangan Bedah*).
- **Tombol**:
  - `[ Buka Pra-Op ]`: memicu dibukanya komponen laci `WardPreOpDrawer` dengan `caseId` kasus terkait.
  - `[ Buat Pemesanan Ruang Bedah → ]`: memindahkan navigasi kerja perawat langsung ke `?section=surgery-booking`.
- **Keadaan (*States*)**:
  - *Loading*: animasi skeleton tabel kasus operasi.
  - *No Cases*: panduan ramah dengan tombol rujukan ke pemesanan kamar bedah.
  - *No Permission*: banner informatif jika akun perawat belum memiliki permission `OperatingRoomWardPreOp:Send`.

---

### B. Sub-tab Laboratorium & Radiologi (Penunjang Medis)

#### SEBELUM
```text
+--------------------------------------------------------------------------------------------------+
| Penunjang Medis — Laboratorium                                                                   |
+--------------------------------------------------------------------------------------------------+
| [ (×) + Pesan Laboratorium ]  Pemesanan dari sisi perawat sedang disiapkan.                     |
|                               Hasil pemeriksaan tetap dapat dibaca di bawah.                     |
|                                                                                                  |
| Tabel Pesanan dan Hasil:                                                                         |
| (Daftar hasil pemeriksaan ditampilkan di sini...)                                                |
+--------------------------------------------------------------------------------------------------+
```

#### SESUDAH
```text
+--------------------------------------------------------------------------------------------------+
| Penunjang Medis — Laboratorium                                                                   |
+--------------------------------------------------------------------------------------------------+
| [ + Pesan Laboratorium ]  (Tombol aktif berwarna primer)                                         |
|                                                                                                  |
| Tabel Pesanan dan Hasil:                                                                         |
| +----------------------------------------------------------------------------------------------+ |
| | No. Order    | Pemeriksaan     | Prioritas | Verifikasi DPJP       | Status Hasil | Aksi     | |
| |--------------+-----------------+-----------+-----------------------+--------------+----------| |
| | LAB-2610-045 | Darah Lengkap   | Cito      | [⏳ Menunggu DPJP]    | Diproses     | [Detail] | |
| |              | dr. Hendra (PJ) |           | (Ns. Mira Safitri)    |              |          | |
| +----------------------------------------------------------------------------------------------+ |
|                                                                                                  |
| Modal Input Pesanan (Pop-up saat tombol ditekan):                                                |
| +----------------------------------------------------------------------------------------------+ |
| | Pemesanan Pemeriksaan Laboratorium (Atas Instruksi DPJP)                                     | |
| |                                                                                              | |
| | Dokter Pemberi Instruksi*: [ ▾ dr. Hendra Sp.PD (DPJP Pasien) ]                              | |
| | Panel / Pemeriksaan*:      [ ▾ Darah Lengkap (Hematologi Rutin) ]                            | |
| | Prioritas*:                (•) Rutin   ( ) Cito                                              | |
| | Indikasi Klinis*:          [ Evaluasi Hb post transfusi                                    ] | |
| | Catatan Tambahan:          [ Pasien puasa sejak jam 22.00                                  ] | |
| |                                                                                              | |
| | ⚠ Pesanan ini dibuat atas delegasi DPJP dan akan diverifikasi pada worklist DPJP.            | |
| |                                                     [ Batal ] [ Kirim Pesanan Laboratorium ] | |
| +----------------------------------------------------------------------------------------------+ |
+--------------------------------------------------------------------------------------------------+
```

---

## 4. Rincian Perbaikan

### FIX-KEP-012-01 — Konsol Kasus Bedah dan Pemicu Laci Pra-Operasi pada Catatan Keperawatan

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-012-01`, `ISS-KEP-012-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`nursing-narrative-tab.jsx` & panel baru `nursing-ward-pre-op-panel.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/tabs/nursing-narrative-tab.jsx`<br/>`src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-notes/panels/nursing-ward-pre-op-panel.jsx` (baru) |
| **Radius dampak** | Menu Catatan Keperawatan (sub-tab `pre-op`) dan integrasi modul Kamar Bedah |
| **Bergantung pada** | Tidak ada |

**Langkah:**
1. Buat komponen baru `nursing-ward-pre-op-panel.jsx` yang memanfaatkan `inpatientSurgeryBookingService.getCasesByEncounter` untuk mengambil daftar operasi pasien.
2. Integrasikan `WardPreOpDrawer` ke dalam panel tersebut untuk membuka form checklist pra-operasi saat tombol baris ditekan.
3. Ganti case `"pre-op"` pada `nursing-narrative-tab.jsx` agar merender `<NursingWardPreOpPanel />` menggantikan `<NursingUnavailableSection />`.
4. Tambahkan tombol tautan navigasi ke Menu 7 (*Ruangan Bedah*) jika pasien belum memiliki jadwal operasi.

**Acceptance criteria:**
1. Ketika sub-tab `pre-op` dibuka pada pasien dengan jadwal bedah aktif (misal `OPR-2609-0012`), tabel jadwal operasi pasien tampil lengkap dengan status pra-operasi.
2. Mengklik tombol `[ Buka Pra-Op ]` berhasil membuka laci `WardPreOpDrawer` dan memuat checklist pra-bedah tanpa galat HTTP 404/500.
3. Pada pasien tanpa jadwal bedah, layar menyajikan pesan informatif dan tombol navigasi yang mengarahkan perawat ke Menu Ruangan Bedah.

**Verifikasi:**
Jalankan tes unit frontend: `npm run test:unit tests/unit/ward-pre-op.test.mjs` dan verifikasi rendering browser.

**Risiko:**
Akun perawat tanpa hak akses `OperatingRoomCase:Read` dapat melihat empty-state. Ditangani dengan pesan penjelas hak akses yang santun.

---

### FIX-KEP-012-02 — Modal Pemesanan Laboratorium Khusus Perawat (`NursingLabOrderModal`)

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-012-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`nursing-ancillary-section.jsx` & modal baru `nursing-lab-order-modal.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`<br/>`src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/modals/nursing-lab-order-modal.jsx` (baru) |
| **Radius dampak** | Sub-tab Laboratorium pada Ruang Kerja Keperawatan rawat inap |
| **Bergantung pada** | Backend endpoint `POST /lab-orders` (`BE-RWI-104`) |

**Langkah:**
1. Buat modal pop-up `nursing-lab-order-modal.jsx` menggunakan `BaseModal` Quilvian.
2. Muat daftar dokter penanggung jawab / visite yang bertugas atas pasien dari data episode (`assignedDoctors` / `dpjpDoctorId`).
3. Sediakan input dropdown pemilihan panel pemeriksaan lab, radio selector prioritas (*Rutin / Cito*), dan input teks indikasi klinis.
4. Pasang payload dengan `inpatientEpisodeId` dan `instructingDoctorId` ke endpoint `POST /api/v1/health-services/laboratory-management/lab-orders`.
5. Aktifkan tombol `+ Pesan Laboratorium` pada `nursing-ancillary-section.jsx` untuk memicu modal tersebut jika `canWrite = true`.
6. Tambahkan kolom *Verifikasi Instruksi* pada tabel `nursing-ancillary-order-table.jsx`.

**Acceptance criteria:**
1. Tombol `+ Pesan Laboratorium` aktif dan membuka jendela modal pemesanan.
2. Pemesanan gagal simpan di sisi client (menampilkan validasi inline) bila dokter pemberi instruksi belum dipilih.
3. Pesanan yang berhasil dikirim tercatat dengan status verifikasi `Pending` dan muncul di tabel riwayat laboratorium.

**Verifikasi:**
Uji integrasi browser: buka sub-tab Lab, buat pesanan darah lengkap cito dengan memilih dr. DPJP, verifikasi respons `200/201 Created` dari backend.

**Risiko:**
Penolakan HTTP 403 bila dokter yang dipilih tidak terdaftar bertugas atas episode pasien. Ditangani dengan hanya menampilkan dokter yang sedang aktif ditugaskan pada episode pasien.

---

### FIX-KEP-012-03 — Modal Pemesanan Radiologi Khusus Perawat (`NursingRadiologyOrderModal`)

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-012-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`nursing-ancillary-section.jsx` & modal baru `nursing-radiology-order-modal.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx`<br/>`src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/modals/nursing-radiology-order-modal.jsx` (baru) |
| **Radius dampak** | Sub-tab Radiologi pada Ruang Kerja Keperawatan rawat inap |
| **Bergantung pada** | Backend endpoint `POST /rad-orders` (`BE-RWI-104`) |

**Langkah:**
1. Buat modal pop-up `nursing-radiology-order-modal.jsx`.
2. Sediakan pilihan modalitas radiologi (X-Ray, USG, CT-Scan), pemeriksaan spesifik, prioritas, serta pemilihan wajib Dokter Pemberi Instruksi.
3. Hubungkan form ke endpoint `POST /api/v1/health-services/radiology-management/rad-orders` dengan menyertakan `instructingDoctorId`.
4. Aktifkan tombol `+ Pesan Radiologi` pada `nursing-ancillary-section.jsx`.

**Acceptance criteria:**
1. Tombol `+ Pesan Radiologi` aktif dan membuka modal formulir radiologi.
2. Pesanan tersimpan ke server dengan nomor order radiologi dan status verifikasi `Pending`.
3. Hasil pesanan langsung tampil pada tabel riwayat penunjang radiologi.

**Verifikasi:**
Simulasikan pemesanan Foto Thorax PA atas instruksi DPJP via browser testing.

**Risiko:**
Pemeriksaan radiologi tertentu membutuhkan persiapan khusus (puasa/alergi zat kontras). Sediakan catatan persiapan klinis pada modal.

---

## 5. Urutan Pengerjaan

```text
FIX-KEP-012-01 (Konsol Kasus Bedah & Drawer Pra-Op di Catatan Keperawatan) [✅ SELESAI]
│
├── FIX-KEP-012-02 (Modal Pemesanan Laboratorium Khusus Perawat via DPJP) [BELUM DIKERJAKAN]
│
└── FIX-KEP-012-03 (Modal Pemesanan Radiologi Khusus Perawat via DPJP) [BELUM DIKERJAKAN]
```

*Catatan: Ketiga perbaikan bersifat independen secara teknis komponen, namun disarankan dikerjakan berurutan dimulai dari Pra-Operasi (karena drawer pendukungnya sudah siap), diikuti Laboratorium dan Radiologi.*

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Rekomendasi | Status Keputusan |
| :---: | --- | --- | :---: |
| K-01 | Konfirmasi penempatan akses Pra-Operasi di sub-tab `pre-op` Catatan Keperawatan | Setujui Opsi A (panel konsol kasus bedah + laci pra-operasi inline) | **DISETUJUI (Opsi A)** oleh Pemilik Produk (2026-10-07) |
| K-02 | Verifikasi delegasi pesanan Lab/Rad rawat inap | Setujui Opsi A (pesanan langsung diteruskan ke unit penunjang dengan penanda verifikasi pending oleh DPJP) | **DISETUJUI (Opsi A)** oleh Pemilik Produk (2026-10-07) |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| :--- | :--- | :--- | :--- |
| `03-frontend-architecture.md` (Keperawatan) | Bagian 10 (Penunjang & Catatan Keperawatan) | "Kontrol pesan Lab/Rad ditampilkan tetapi disabled (AC-6)" | "Kontrol pesan Lab/Rad aktif menggunakan modal pemesanan delegatif perawat dengan input wajib Dokter Pemberi Instruksi (`BE-RWI-104`)" |
| `skema-tampilan-keperawatan-rawat-inap.md` | Sub-tab Pra-Operasi | Komponen stub unavailable | Konsol status kasus operasi aktif dan pemicu `WardPreOpDrawer` |

---

## 8. Verifikasi Menyeluruh

Setelah ketiga perbaikan selesai diimplementasikan:
1. **Verifikasi Pra-Operasi**:
   - Buka pasien dengan episode `Admitted` yang memiliki jadwal operasi.
   - Buka menu *Catatan Keperawatan* -> sub-tab *Catatan Pra-Operasi*.
   - Pastikan informasi jadwal operasi tampil, klik `Buka Pra-Op`, isi checklist tanda vital, penandaan area operasi, dan simpan draft.
   - Buka menu *Ruangan Bedah*, pastikan data yang diisi di Catatan Keperawatan tersinkronisasi sempurna pada kasus yang sama.
2. **Verifikasi Laboratorium**:
   - Buka menu *Penunjang Medis* -> sub-tab *Laboratorium*.
   - Klik `+ Pesan Laboratorium`.
   - Pilih DPJP yang bertugas, pilih tes Darah Lengkap, kirim pesanan.
   - Pastikan nomor pesanan muncul di tabel riwayat dengan badge `Menunggu Verifikasi DPJP`.
3. **Verifikasi Radiologi**:
   - Buka sub-tab *Radiologi*, klik `+ Pesan Radiologi`.
   - Pilih DPJP, pilih pemeriksaan Rontgen Thorax, kirim pesanan.
   - Pastikan pesanan tercatat di riwayat penunjang radiologi.
4. **Validasi Sintaks & Build**:
   - Jalankan `npm run lint` pada repository frontend.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-07 | Rencana perbaikan dibuat, 3 item perbaikan terdaftar, status MENUNGGU_PERSETUJUAN | `diagnose-module-issue` |
| 2026-10-07 | Rencana perbaikan DISETUJUI penuh oleh pemilik produk (K-01 Opsi A, K-02 Opsi A) | Pemilik Produk |
