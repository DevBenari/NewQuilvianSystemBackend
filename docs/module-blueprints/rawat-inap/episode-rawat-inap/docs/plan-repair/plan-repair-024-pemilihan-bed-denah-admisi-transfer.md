# PLAN-REPAIR-024 — Penerapan Komponen Denah pada Pemilihan Tempat Tidur Admisi Transfer

```yaml
plan_id: PLAN-REPAIR-EPS-024
issue: ../issue/issue-024-pemilihan-bed-denah-admisi-transfer.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-17"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-17"
tanggal_selesai: "2026-10-17"
basis_source_backend: "c2bc563ed9332f08fa95b9fe35786fea7a647279 (MHamzah)"
basis_source_frontend: "15ae73984ad26a408e3038566d7e1d1533fe10c3 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Perbaikan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| `FIX-EPS-024-01` | `ISS-EPS-024-01` | Mengaktifkan `selectionMode === "floor-plan"` dan merender `FloorPlanSelection` pada Langkah 5 alur transfer | FE | 1 | — | ✅ SELESAI | [inpatient-admission-bed-step.jsx](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx#L128-L156), [floor-plan-transfer-admission.test.mjs](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/tests/unit/floor-plan-transfer-admission.test.mjs) |
| `FIX-EPS-024-02` | `ISS-EPS-024-02` | Menyesuaikan adapter `useInpatientAdmissionBed` agar memetakan status `isSelectable` denah dari `available-beds` saat `episodeId` belum ada (alur transfer) | FE | 1 | — | ✅ SELESAI | [use-inpatient-admission-bed.jsx](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx#L118-L230), [floor-plan-transfer-admission.test.mjs](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/tests/unit/floor-plan-transfer-admission.test.mjs) |

**Ringkasan: 2 dari 2 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-EPS-024-01 & ISS-EPS-024-02 — Pemilihan Denah pada Jalur Admisi Transfer

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | **Frontend Adapter Pattern (Overlay Ketersediaan):** Jalur transfer memuat denah per lantai melalui `bedManagementService.getFloorPlan(floorCode)` yang tidak mewajibkan `episodeId`. Hook menyuntikkan status kelayakan (`isSelectable: true` dan outline hijau) untuk tempat tidur yang terdapat pada `availableBeds` yang sudah diloloskan server. Pilihan bed diteruskan ke Langkah 6. | 1. Tidak mengubah backend C# sama sekali.<br>2. Transaksi atomik transfer `POST /admission-transfers/admit` tetap aman 100%.<br>3. UI denah langsung identik dengan alur admisi biasa.<br>4. Radius dampak sempit dan aman. | Memerlukan penyatuan fungsi adapter data denah di hook `useInpatientAdmissionBed`. |
| **B** | **Backend Contract Extension:** Mengubah DTO `FloorPlanSelectionQuery` di backend agar `EpisodeId` bersifat opsional dan menambahkan `Purpose = "TransferAdmission"` dengan parameter `TransferDispositionId`. | Backend menghitung kelayakan secara native di endpoint denah. | Memerlukan perubahan backend C#, registri controller, otorisasi, migration/DTO baru, dan deploy lintas repository yang memakan waktu lebih lama. |
| **C** | **Bootstrap Episode Draft Sementara:** Membuat episode rawat inap Draft sementara di Langkah 4 alur transfer, lalu menghapusnya atau mengikatnya saat submit. | Memakai endpoint denah `Purpose=Admission` tanpa ubahan. | **Ditolak keras**: melanggar prinsip atomik disposisi IGD (`ISS-EPS-006-01`), rentan menghasilkan episode yatim/duplikat bila alur dibatalkan. |

**Solusi Terpilih: Opsi A.**

1. Menjaga prinsip tata kelola Quilvian: *"Ikuti kode yang sudah ada, jangan mengubah arsitektur transaksi yang sudah berjalan stabil"*.
2. Memenuhi 100% ekspektasi pengguna: layar Langkah 5 beralih dari dropdown teks biasa menjadi komponen denah visual interaktif (`FloorPlanSelection`) lengkap dengan tab lantai (Lantai 2, 3, 4, 5, 8, 9), zoom/pan, dan seleksi bed di kamar.
3. Alur hilir Langkah 6 (Booking Bed) dan Langkah 7 (Konfirmasi) sudah siap menerima objek bed dari denah tanpa perlu penulisan ulang.

**Kenapa bukan opsi lain:**
- Opsi B membutuhkan koordinasi task backend terpisah (`BE-RWI`) yang menahan perbaikan frontend yang sebenarnya dapat diselesaikan secara elegan di adapter client.
- Opsi C berisiko tinggi terhadap integritas data klinis dan billing rawat inap (potensi episode fiktif).

---

## 3. Skema Tampilan Sebelum → Sesudah

### Tampilan SEBELUM Perbaikan (Kondisi Saat Ini)

```text
+----------------------------------------------------------------------------------------------------+
|  Admisi Rawat Inap                                                                                 |
|  Daftarkan pasien, tentukan penjamin dan DPJP, lalu pesan tempat tidurnya.                        |
|                                                                                                    |
|  LANGKAH 5 DARI 9  Pilih Bed                                                        56% Selesai    |
|  (✓ Antrean Transfer) (✓ Verifikasi) (✓ Pembayaran) (✓ Dokter) (• Pilih Bed) ( Booking ) ...       |
|                                                                                                    |
|  LANGKAH PILIH BED                                                                                 |
|  Pilih Tempat Tidur                                                                                |
|  Pilih satu tempat tidur yang dapat dipakai pasien ini, lalu lanjut ke pemesanan.                 |
|                                                                                                    |
|  [ℹ Kelayakan penempatan ditentukan server: Daftar tempat tidur datang apa adanya dari server...]  |
|                                                                                                    |
|  [ℹ Pilihan bed admisi transfer: Alur transfer dari IGD/Poli membuat episode dan reservasi...]     |
|                                                                                                    |
|  Tempat Tidur *                                                                                    |
|  [ Pilih bed yang tersedia                                                                      ▾] |
|                                                                                                    |
|  [ Batal Pendaftaran ]                                              [ Kembali ] [ Lanjut ke Booking]|
+----------------------------------------------------------------------------------------------------+
```

### Tampilan SESUDAH Perbaikan (Target Implementasi)

```text
+----------------------------------------------------------------------------------------------------+
|  Admisi Rawat Inap                                                                                 |
|  Daftarkan pasien, tentukan penjamin dan DPJP, lalu pesan tempat tidurnya.                        |
|                                                                                                    |
|  LANGKAH 5 DARI 9  Pilih Bed                                                        56% Selesai    |
|  (✓ Antrean Transfer) (✓ Verifikasi) (✓ Pembayaran) (✓ Dokter) (• Pilih Bed) ( Booking ) ...       |
|                                                                                                    |
|  LANGKAH PILIH BED                                                                                 |
|  Pilih Tempat Tidur                                                                                |
|  Pilih satu tempat tidur yang dapat dipakai pasien ini, lalu lanjut ke pemesanan.                 |
|                                                                                                    |
|  [ℹ Kelayakan penempatan ditentukan server: Daftar tempat tidur datang apa adanya dari server...]  |
|                                                                                                    |
|  (Bila bed sudah dipilih):                                                                         |
|  [✓ Bed terpilih: Kelas 2 329 · bed 329-C                                                        ] |
|                                                                                                    |
|  +-- [Tab Lantai: Lt 2 | Lt 3 (Aktif) | Lt 4 | Lt 5 | Lt 8 | Lt 9] ------------------------------+ |
|  |                                                                                                | |
|  |   [ Denah Grafis SVG Interaktif Lantai 3 ]                                                      | |
|  |   +-------------------+  +-------------------+  +-------------------+                          | |
|  |   | Kamar 328         |  | Kamar 329 (Hijau) |  | Kamar 330         |                          | |
|  |   | [328-A] [328-B]   |  | [329-A] [329-C ✓] |  | [Penuh]           |                          | |
|  |   +-------------------+  +-------------------+  +-------------------+                          | |
|  |                                                                                                | |
|  |   Keterangan Warna: [■ Tersedia] [■ Terisi] [■ Dipesan] [■ Pembersihan] [■ Khusus ODC]         | |
|  +------------------------------------------------------------------------------------------------+ |
|                                                                                                    |
|  [ Batal Pendaftaran ]                                              [ Kembali ] [ Lanjut ke Booking]|
+----------------------------------------------------------------------------------------------------+
```

### Tabel Wilayah Tampilan SESUDAH

| Wilayah | Isi & Komponen | Sumber Data | Perilaku Interaktif |
| :--- | :--- | :--- | :--- |
| **Heading & Stepper** | Judul langkah, nomor langkah 5 dari 9, stepper alur transfer. | State alur `flow` | Menampilkan progres admisi transfer secara konsisten. |
| **Banner Informasi** | Status kelayakan server & konfirmasi bed terpilih (`InformationAlert`). | State `selectedBed` & `selectedBedLocation` | Muncul hijau saat bed telah diklik pada pop-up denah. |
| **Tab Pemilihan Lantai** | Tab tombol daftar lantai rumah sakit (Lantai 2, 3, 4, 5, 8, 9). | `FloorPlanSelection` | Mengganti visualisasi lantai aktif tanpa memuat ulang seluruh halaman. |
| **Peta Denah SVG Interaktif** | Vektor denah lantai, tata letak kamar, pintu, lorong, dan kotak ranjang. | `FloorPlanDrawing` & data denah bersama | Dapat di-zoom, digeser (*pan*), dan setiap kamar dapat diklik untuk membuka pop-up. Kamar dengan bed layak diberi outline hijau terang. |
| **Pop-up Kamar (Mode Pilih)** | Pop-up berisi daftar bed di kamar tersebut: kode bed, status, dan tombol *"Pilih bed ini"*. | `FloorPlanRoomPopup` | Tombol *"Pilih bed ini"* aktif untuk bed yang statusnya `isSelectable === true`. Mengklik tombol menetapkan `selectedBed` dan menutup pop-up. |
| **Tombol Aksi Bawah** | `Batal Pendaftaran` (kiri), `Kembali` dan `Lanjut ke Pemesanan` (kanan). | `BaseButton` | Tombol `Lanjut ke Pemesanan` aktif hanya setelah ada bed yang dipilih. |

---

## 4. Rincian Perbaikan

### FIX-EPS-024-01 — Aktifkan Komponen Denah pada Langkah 5 Alur Transfer

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-024-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`inpatient-admission-bed-step.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx` |
| **Radius dampak** | Layar Admisi Rawat Inap (`InpatientAdmissionView`) alur transfer. Alur admisi reguler dan bayi tidak terganggu karena sudah memakai mode `floor-plan`. |
| **Bergantung pada** | `FIX-EPS-024-02` |

**Langkah Implementasi:**
1. Hapus percabangan usang yang menampilkan blok `<section aria-label="Pilihan bed admisi transfer">` berisi `BaseSelectField`.
2. Pastikan `FloorPlanSelection` menjadi komponen utama yang dirender untuk seluruh alur admisi (reguler, bayi, maupun transfer) saat `selectionMode === "floor-plan"`.
3. Hubungkan callback `onSelectBed`, `selectedBedId`, dan `clearSelectedBed` ke state controller bersama.

**Acceptance Criteria:**
1. Pada Langkah 5 alur transfer, komponen `BaseSelectField` (dropdown "Pilih bed yang tersedia") tidak lagi dirender.
2. Komponen `FloorPlanSelection` tampil utuh di layar dengan kontrol tab lantai (Lantai 2, 3, 4, 5, 8, 9) dan peta SVG interaktif.
3. Saat pengguna mengklik salah satu kamar di denah, pop-up rincian kamar terbuka dengan daftar tempat tidur.
4. Saat pengguna mengklik *"Pilih bed ini"*, banner hijau *"Bed terpilih: [Nama Ruang] · bed [Label]"* muncul dan tombol *"Lanjut ke Pemesanan"* menjadi aktif.

---

### FIX-EPS-024-02 — Penyesuaian Adapter Denah untuk Alur Transfer Tanpa Episode Awal

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-024-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`use-inpatient-admission-bed.jsx`) |
| **Berkas yang diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` |
| **Radius dampak** | Controller pemilihan bed admisi rawat inap |
| **Bergantung pada** | Tidak ada |

**Langkah Implementasi:**
1. Perbarui pendeteksian mode denah:
   ```javascript
   // Sebelumnya:
   const usesAdmissionFloorPlan = Boolean(admission && !transfer);

   // Menjadi:
   const usesAdmissionFloorPlan = Boolean(admission || transfer);
   ```
2. Sesuaikan konfigurasi hook `useFloorPlan`:
   - Jika `episodeId` tersedia (alur reguler yang sudah menyiapkan episode Draft), panggil `useFloorPlan({ purpose: "Admission", episodeId })` seperti biasa.
   - Jika `transfer` aktif dan `episodeId` belum ada (alur transfer), aktifkan pembacaan denah dasar (tanpa parameter `purpose/episodeId` yang memicu penolakan backend) atau gunakan denah monitoring bersama.
3. Pada pembuatan snapshot `floorPlanBoard`:
   Lakukan pencocokan (*reconciliation*) antara daftar kamar/bed denah dengan `legacyBoard.availableBeds`. Bed yang terdaftar di `availableBeds` secara otomatis disuntikkan properti `isSelectable = true` dan `eligibility = { isEligible: true }`, sehingga outline hijau kamar dan tombol *"Pilih bed ini"* menyala untuk bed yang valid menurut server.
4. Pastikan `prepareBooking()` untuk alur transfer tetap meneruskan eksekusi ke Langkah 6 tanpa mencoba membuat episode Draft prematur.

**Acceptance Criteria:**
1. Pemanggilan denah pada alur transfer tidak menghasilkan galat HTTP 400 atau 422 di konsol/network browser.
2. Tempat tidur yang tidak tersedia (terisi, dipesan, perbaikan) tetap ditandai sesuai statusnya dan tidak dapat dipilih.
3. Tempat tidur yang lolos filter server (`board.availableBeds`) dapat diklik dan terpilih sebagai `selectedBed`.
4. Berpindah ke Langkah 6 (Booking Bed) membawa data lengkap bed (`bedId`, `bedName`, `patientClassId`, `operationalVersion`) dan tombol *"Pesan Tempat Tidur"* sukses mengeksekusi `admitFromTransfer`.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-024-02 (Adapter kelayakan denah transfer di use-inpatient-admission-bed.jsx)
└── FIX-EPS-024-01 (Penerapan FloorPlanSelection di inpatient-admission-bed-step.jsx)
    └── Verifikasi Alur End-to-End & Unit Testing
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan | Dampak | Menahan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Penggunaan denah visual pada alur transfer admisi | Setuju menerapkan Opsi A (Frontend Adapter Pattern) | Menstandarkan UX pemilihan bed ke peta denah SVG visual interaktif | `FIX-EPS-024-01`, `FIX-EPS-024-02` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Kalimat Sekarang | Menjadi |
| :--- | :--- | :--- | :--- |
| `05-skema-tampilan.md` | Langkah 5 (Pilih Bed Alur Transfer) | *"Pilihan bed admisi transfer menggunakan dropdown BaseSelectField"* | *"Seluruh alur admisi (Pasien Baru, Pasien Lama, Bayi, dan Transfer IGD/Poli) menggunakan komponen visual FloorPlanSelection"* |
| `03-frontend-architecture.md` | Bagian Pemilihan Tempat Tidur | *"Jalur Transfer mempertahankan board existing sampai FE-RWI-249"* | *"Jalur Transfer mengintegrasikan FloorPlanSelection melalui ketersediaan bed server"* |

---

## 8. Verifikasi Menyeluruh

1. **Uji Linting & Unit Test Frontend:**
   ```bash
   npm run lint:errors
   npm run test:unit -- tests/unit/inpatient-admission-bed-step.test.mjs tests/unit/inpatient-admission-transfer.test.mjs
   ```
2. **Uji Skenario Manual (Live Browser):**
   - Buka menu **Admisi Rawat Inap** -> Pilih tab **Transfer IGD/Poliklinik**.
   - Pilih salah satu pasien rujukan dari antrean transfer.
   - Verifikasi data pasien, penjamin/pembayaran, dan data DPJP/dokter.
   - Masuk ke **Langkah 5 (Pilih Bed)**:
     - Pastikan denah visual muncul dengan tab Lantai 2, 3, 4, 5, 8, 9.
     - Klik kamar yang memiliki bed kosong -> pop-up kamar terbuka.
     - Klik *"Pilih bed ini"* pada salah satu bed tersedia.
     - Pastikan banner hijau bed terpilih muncul.
   - Klik *"Lanjut ke Pemesanan"* -> Masuk ke **Langkah 6 (Booking Bed)**.
   - Klik *"Pesan Tempat Tidur"* -> Pastikan request `POST /admission-transfers/admit` berhasil (HTTP 201 Created).
   - Pastikan alur berlanjut ke Konfirmasi dan Cetak Persetujuan dengan nomor episode resmi.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-17 | Pembuatan rencana perbaikan awal untuk integrasi denah pada admisi transfer IGD/Poli | Antigravity AI (`diagnose-module-issue`) |
| 2026-10-17 | Rencana perbaikan disetujui pemilik sistem untuk dikerjakan (Opsi A) | Pemilik Sistem (User) |
| 2026-10-17 | Implementasi adapter ketersediaan denah transfer & penambahan unit test `floor-plan-transfer-admission.test.mjs` selesai diverifikasi (Status: SELESAI) | Antigravity AI (`build-module-frontend`) |
