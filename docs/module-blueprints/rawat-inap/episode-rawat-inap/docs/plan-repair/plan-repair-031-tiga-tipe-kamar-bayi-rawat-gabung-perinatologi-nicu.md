# PLAN-REPAIR-031 — Rencana Penerapan Tiga Kartu Pilihan Perawatan Bayi (Rawat Gabung, Perinatologi, NICU), Konfigurasi Boks Kamar VIP Lantai 3, Pembersihan Data Uji Coba, dan Perencanaan Unit NICU

```yaml
plan_id: PLAN-REPAIR-EPS-031
issue: ../issue/issue-031-tiga-tipe-kamar-bayi-rawat-gabung-perinatologi-nicu.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-21"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-21"
basis_source_backend: "6406fc76 (MHamzah)"
basis_source_frontend: "381ec25fa (HamzahV2)"
target_database: "QuilvianNewDevHamzah"
```

Dokumen ini memuat rencana perbaikan menyeluruh untuk mendukung 3 jenis perawatan bayi baru lahir (*Rawat Gabung*, *Ruang Perinatologi*, dan *Ruang NICU*) pada Langkah 3 formulir admisi bayi (`entry=newborn`), merinci langkah pembersihan data penempatan dan reservasi uji coba sebelumnya (*testing state reset*), mengonfigurasi 1 boks bayi pada sebagian kamar ibu privat (`VIP 301` sampai `VIP 305` Lantai 3), serta mendaftarkan 1 kamar percontohan `Ruang NICU` di Lantai 2 dengan 2 tempat tidur inkubator intensif di master data database `QuilvianNewDevHamzah`.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- | :---: |
| `FIX-EPS-031-01` | `ISS-EPS-031-01` | Redesain Langkah 3 menjadi 3 Kartu Pilihan Langsung (*Rawat Gabung*, *Ruang Perinatologi*, *Ruang NICU*) dan peniadaan centang intensif tersembunyi | FE | 1 | `FIX-EPS-031` | ✅ SELESAI | [`FIX-EPS-031.md`](../../task/report/frontend/FIX-EPS-031.md) |
| `FIX-EPS-031-02` | `ISS-EPS-031-04` | Pembersihan data penempatan dan reservasi uji coba sebelumnya (*safe state reset*) pada database `QuilvianNewDevHamzah` | DB / Ops | 1 | `FIX-EPS-031` | ✅ SELESAI | [`FIX-EPS-031.md`](../../task/report/frontend/FIX-EPS-031.md) |
| `FIX-EPS-031-03` | `ISS-EPS-031-02` | Pendaftaran 1 tempat tidur boks bayi (`BOKS`) dan penyesuaian kapasitas menjadi 2 pada kamar `VIP 301` s.d. `VIP 305` Lantai 3 | DB / Master | 1 | `FIX-EPS-031` | ✅ SELESAI | [`FIX-EPS-031.md`](../../task/report/frontend/FIX-EPS-031.md) |
| `FIX-EPS-031-04` | `ISS-EPS-031-03` | Pendaftaran kamar fisik percontohan `Ruang NICU` (`NICU-1`) di Lantai 2 dan 2 tempat tidur inkubator intensif (`NICU-1-A`, `NICU-1-B`) | DB / Master | 1 | `FIX-EPS-031` | ✅ SELESAI | [`FIX-EPS-031.md`](../../task/report/frontend/FIX-EPS-031.md) |

**Ringkasan: 4 dari 4 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-EPS-031-01 — Redesain Pilihan Perawatan Langkah 3 Menjadi 3 Kartu Langsung

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | **3 Kartu Pilihan Langsung yang Eksplisit**: Menyajikan 3 kartu bertingkat mandiri (*Rawat Gabung*, *Perinatologi*, *NICU*). Pemilihan kartu secara otomatis memetakan `careMode` dan `requiresIntensiveCare` di balik layar tanpa kotak centang terpisah. | Menghilangkan salah klik centang intensif oleh petugas (*error prevention by design*), alur intuitif dan jelas bagi petugas admisi dan perawat, tetap kompatibel 100% dengan skema data backend (`InpEpisode`). | Memerlukan pembaruan komponen `NewbornCareStep` dan adaptor konstanta pilihan perawatan. |
| **B** | Mempertahankan 2 kartu radio (`RoomingIn`, `Neonatal`) dan mewajibkan centang pilihan dropdown sekunder (*Sub-tipe Neonatal: Reguler / NICU*). | Opsi backend tetap 1-ke-1. | Memerlukan 2 kali klik dari petugas admisi, berpotensi terlewat, UI terasa berbelit-belit. |
| **C** | Membiarkan antarmuka apa adanya dan hanya menambahkan teks bantuan peringatan tebal berwarna merah di samping kotak centang. | Tidak perlu mengubah tata letak kartu. | **Ditolak tegas**: Tidak menyelesaikan akar masalah kebingungan petugas di lapangan; petugas tetap rentan salah centang. |

**Solusi Terpilih: Opsi A.**  
Opsi ini disetujui secara langsung oleh pemilik sistem pada 21 Oktober 2026. Desain 3 kartu langsung memberikan kejelasan visual seketika bagi staf rumah sakit, membedakan secara tegas antara observasi perinatologi biasa dan perawatan intensif NICU.

---

### ISS-EPS-031-02 — Penambahan Boks Bayi Khusus pada Sebagian Kamar VIP Lantai 3

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | **Pendaftaran Boks Bayi Selektif (Kamar VIP 301 s.d. 305)**: Mendaftarkan 1 bed fisik tambahan dengan kode `BD-RSMMC-BOKS-301` s.d. `305` (`IsForNewborn = true`, `BedNumber = 'BOKS'`) dan menaikkan kapasitas kamar menjadi 2. Kamar lain di Lantai 3 tetap murni dewasa. | Sesuai arahan tegas pemilik sistem: *"sebagian kamar saja yang mendukung support bayi tersebut"*, kamar privat VIP 301–305 memiliki ruang fisik yang cukup untuk boks bayi tanpa mengorbankan kenyamanan ibu. | Pasien rawat gabung di luar kamar VIP 301–305 harus diarahkan ke kamar yang memiliki boks atau dipindahkan (*transfer*). |
| **B** | Menambahkan boks bayi di seluruh 29 kamar di Lantai 3. | Fleksibilitas penempatan rawat gabung maksimal. | **Ditolak pemilik sistem**: Kamar kelas II dan kelas I bertingkat bersama tidak seluruhnya layak menampung boks bayi tambahan secara operasional dan tata ruang. |
| **C** | Mengubah ranjang ibu (`BedNumber = 'A'`) agar bertanda `IsForNewborn = true`. | Tidak perlu menambah baris data tempat tidur baru di database. | **Pelanggaran klinis & invariant fatal**: Ranjang dewasa tidak boleh dipakai bayi; sistem penagihan (*billing*) dan keselamatan ranjang bayi akan kacau. |

**Solusi Terpilih: Opsi A.**  
Memenuhi batasan operasional rumah sakit dengan menambah tempat tidur boks bayi resmi pada kamar privat yang disetujui.

---

### ISS-EPS-031-03 — Pendaftaran Kamar Percontohan & Inkubator NICU di Lantai 2

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | **Kamar NICU Percontohan Mandiri di Lantai 2**: Mendaftarkan 1 kamar baru `Ruang NICU` (`RM-RSMMC-NICU01`, nomor `NICU-1`, kelas `NICU` `CLS-149`, `IsIntensiveCare = true`) dengan 2 tempat tidur inkubator (`BD-RSMMC-NICU-01`, `BD-RSMMC-NICU-02`) berstatus `IsIntensiveCareBed = true` dan `IsForNewborn = true`. | Menggunakan master kelas `NICU` (`CLS-149`) yang sudah ada di database, memisahkan secara jelas antara unit Perinatologi non-intensif (`BAYI-1`, `BAYI-2`) dan unit intensif NICU, mendukung validasi backend `NEWBORN_INTENSIVE_REQUIRED`. | Memerlukan entri master data kamar dan tempat tidur baru. |
| **B** | Mengubah salah satu kamar perinatologi yang ada (`BAYI-2`) menjadi kamar NICU dengan mengubah kelasnya. | Tidak menambah jumlah kamar fisik baru. | Mengurangi kapasitas kamar perinatologi reguler yang sudah ada; mengacaukan tata nama kamar `BAYI-2`. |
| **C** | Hanya mengubah 2 dari 4 bed di kamar `BAYI-1` menjadi `IsIntensiveCareBed = true`. | Kamar fisik tetap sama. | Menyalahi tata kelola rumah sakit: ruang intensif isolasi/kritis memerlukan standar lingkungan dan kelas ruangan tersendiri yang berbeda dengan perinatologi umum. |

**Solusi Terpilih: Opsi A.**  
Memberikan pemisahan bounded context dan unit layanan yang bersih antara observasi neonatal dan perawatan intensif gawat darurat neonatus.

---

### ISS-EPS-031-04 — Pembersihan Data Penempatan & Reservasi Uji Coba (Safe State Reset)

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | **Pembersihan Terkendali Berbasis Planning Gate**: Menutup seluruh penempatan aktif sisa uji coba (`EndDateTime = NOW()`), membatalkan reservasi sementara yang menggantung, menyelesaikan siklus kebersihan, dan mengembalikan `BedStatus` di `MstBed` menjadi `1` (`Available`). | Database `QuilvianNewDevHamzah` kembali bersih, seluruh kamar Lantai 2 dan Lantai 3 siap pakai untuk penempatan bayi baru, riwayat data lama tetap tersimpan secara aman tanpa `DELETE` brutal. | Memerlukan verifikasi bahwa tidak ada pengguna lain yang sedang menguji transaksi bersamaan pada database sandbox. |
| **B** | Menghapus langsung seluruh tabel transaksi menggunakan `TRUNCATE TABLE` atau `DELETE FROM`. | Cepat dan instan. | **Sangat berisiko**: Melanggar referensi foreign key ke tabel episode, tagihan, dan pasien; berpotensi merusak integritas basis data. |
| **C** | Membiarkan tempat tidur yang terisi dan hanya memakai kamar/bed yang masih kosong. | Tidak menyentuh data transaksi yang ada. | Kamar favorit pengujian seperti `VIP 304` dan `ICU-A` tetap terkunci permanen, mengurangi opsi kamar yang dapat didemonstrasikan. |

**Solusi Terpilih: Opsi A.**  
Sesuai arahan pengguna: *"sebelum itu anda clear dulu data yang sudah penempatan kelas kamar, tapi dimasukkan ke dalam planning dulu ya"*.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Keadaan Sebelum Perbaikan (Langkah 3 Form Admisi Bayi)

```text
+---------------------------------------------------------------------------------------------------------+
| LANGKAH 3 DARI 7   Perawatan dan DPJP Bayi                                                42% Selesai   |
| [✓] Pilih Ibu Persalinan  [✓] Identitas Bayi  [•] Perawatan & DPJP  [ ] Penempatan  [ ] Persetujuan     |
+---------------------------------------------------------------------------------------------------------+
| ADMISI BAYI                                                                                             |
| Perawatan dan DPJP Bayi                                                                                 |
| Gunakan keputusan tenaga klinis. DPJP bayi dipilih tersendiri.                                          |
|                                                                                                         |
| JENIS PERAWATAN BAYI:                                                                                   |
| ( ) Rawat gabung dengan ibu       (•) Neonatal terpisah                                                 |
|                                                                                                         |
| Sumber keputusan perawatan: *                                                                           |
| [ Dokter Obgyn / Ruang VK Persalinan                                                                  ] |
|                                                                                                         |
| [x] Membutuhkan perawatan neonatal intensif sesuai keputusan klinis                                     |
|     Centang hanya jika bayi membutuhkan fasilitas intensif (NICU). Jangan dicentang jika bayi dirawat    |
|     di ruang Perinatologi reguler (seperti kamar BAYI-1 atau BAYI-2).                                   |
|                                                                                                         |
| DPJP BAYI: *                                                                                            |
| Dokter Penanggung Jawab Pelayanan: [ dr. Hendra, Sp.A                                               [v] |
|                                                                                                         |
|                                                                     [ Kembali ] [ Lanjut ke Penempatan ]|
+---------------------------------------------------------------------------------------------------------+
```

*Masalah:* Petugas bingung membedakan "Neonatal terpisah reguler" dan "NICU", sehingga sering keliru mencentang checkbox intensif.

---

### Keadaan Sesudah Perbaikan (Langkah 3 dengan 3 Pilihan Kartu Langsung)

```text
+---------------------------------------------------------------------------------------------------------+
| LANGKAH 3 DARI 7   Perawatan dan DPJP Bayi                                                42% Selesai   |
| [✓] Pilih Ibu Persalinan  [✓] Identitas Bayi  [•] Perawatan & DPJP  [ ] Penempatan  [ ] Persetujuan     |
+---------------------------------------------------------------------------------------------------------+
| ADMISI BAYI                                                                                             |
| Perawatan dan DPJP Bayi                                                                                 |
| Tentukan rencana penempatan awal bayi baru lahir berdasarkan arahan dan keputusan tim klinis.           |
|                                                                                                         |
| PILIH JENIS PERAWATAN BAYI: *                                                                           |
| +-----------------------------+ +-----------------------------+ +-----------------------------+         |
| | (•) KARTU 1: RAWAT GABUNG   | | ( ) KARTU 2: RUANG PERINATO | | ( ) KARTU 3: RUANG NICU     |         |
| |     (Rooming-In)            | |     (Neonatal Observasi)    | |     (Neonatal Intensif)     |         |
| |                             | |                             | |                             |         |
| | Bayi dirawat bersama ibu di | | Bayi dirawat terpisah di    | | Perawatan intensif neonatal |         |
| | kamar rawat inap ibu        | | ruang observasi bayi        | | dengan inkubator intensif   |         |
| | (kamar VIP dengan fasilitas | | reguler (Kamar Perinatologi | | untuk bayi kondisi gawat /  |         |
| | boks bayi terpasang).       | | BAYI-1 & BAYI-2).           | | darurat klinis tinggi.      |         |
| |                             | |                             | |                             |         |
| | [ Fasilitas Boks Kamar Ibu ]| | [ Kamar Perinatologi Lt. 2 ]| | [ Unit NICU Khusus Lt. 2 ]  |         |
| +-----------------------------+ +-----------------------------+ +-----------------------------+         |
|                                                                                                         |
| Sumber keputusan perawatan: *                                                                           |
| [ Rekomendasi Dokter Spesialis Anak & Obgyn R. Bersalin                                               ] |
|                                                                                                         |
| Episode Perawatan Ibu Terkait:                                                                          |
| [ Ny. Siti Aminah · Rekam Medis: RM-2026-091 · Kamar VIP 301 (Bed A) · Status: AKTIF DIRAWAT       ] |
| (i) Kamar ibu mendukung fasilitas boks bayi rawat gabung (VIP 301).                                    |
|                                                                                                         |
| DPJP BAYI (DOKTER SPESIALIS ANAK): *                                                                    |
| Dokter Penanggung Jawab Pelayanan: [ dr. Hendra, Sp.A                                               [v] |
| Status Kredensial: Dokter Spesialis Anak Aktif (SIP: 446.1/092/2026)                                    |
|                                                                                                         |
| [ ] Pasien membutuhkan ruang isolasi infeksi neonatus (opsional sesuai indikasi medis)                  |
|                                                                                                         |
|                                                                     [ Kembali ] [ Lanjut ke Penempatan ]|
+---------------------------------------------------------------------------------------------------------+
```

---

### Tabel Wilayah Layar Sesudah Perbaikan

| Wilayah Layar | Isi dan Fungsi | Sumber Data | Komponen Pengisi |
| :--- | :--- | :--- | :--- |
| **Header Alur & Langkah** | Menampilkan judul langkah, nomor urutan pendaftaran (Langkah 3 dari 7), dan bilah progres persentase pengerjaan. | Progres alur admisi | `Heading`, `BaseProgressBar` |
| **Grid 3 Kartu Pilihan Perawatan** | Tiga kartu interaktif yang dapat dipilih dengan 1 kali klik: Rawat Gabung (`RoomingIn`), Ruang Perinatologi (`Perinatology`), atau Ruang NICU (`NICU`). Menampilkan ikon, judul, deskripsi alur, dan badge fasilitas. | State `newborn.draft.careMode` & `requiresIntensiveCare` | `SegmentedCardGroup` / Custom Card Selector |
| **Sumber Keputusan Klinis** | Kolom teks wajib diisi nama dokter/unit penentu keputusan perawatan. | State `newborn.draft.careSource` | `BaseInputField` |
| **Informasi Kamar Ibu (Khusus Rawat Gabung)** | Tampil saat Kartu 1 dipilih: ringkasan nama ibu, nomor rekam medis, kamar ibu saat ini, dan evaluasi apakah kamar ibu mendukung boks bayi rawat gabung. | Evaluasi `selectedCandidate` | `ResourceFilterSelect`, `InformationAlert` |
| **Pilihan DPJP Bayi** | Dropdown pemilihan Dokter Spesialis Anak yang bertanggung jawab atas asuhan medis neonatus. | API dokter spesialis anak | `InpatientAdmissionDoctorStep` |
| **Pilihan Isolasi Neonatus** | Opsi ortogonal kebutuhan isolasi bila bayi memiliki penyakit infeksi menular. | State `newborn.draft.requiresIsolation` | `BaseCheckboxField` |
| **Tombol Navigasi** | Tombol Kembali ke Langkah 2 dan Lanjut ke Langkah 4 Penempatan. | Logika validasi langkah | `BaseButton` (`primary` & `secondary`) |

---

### Tabel Tombol Layar Sesudah Perbaikan

| Label Tombol | Jenis | Kapan Aktif | Yang Terjadi Saat Diklik |
| :--- | :--- | :--- | :--- |
| **Pilih Kartu 1 (Rawat Gabung)** | `card-selection` | Selalu aktif (kecuali draf terkunci). | Mengatur `careMode = "RoomingIn"` dan `requiresIntensiveCare = false`. Menampilkan rincian kamar ibu. |
| **Pilih Kartu 2 (Ruang Perinatologi)** | `card-selection` | Selalu aktif (kecuali draf terkunci). | Mengatur `careMode = "Neonatal"` dan `requiresIntensiveCare = false`. Menyembunyikan rincian kamar ibu. |
| **Pilih Kartu 3 (Ruang NICU)** | `card-selection` | Selalu aktif (kecuali draf terkunci). | Mengatur `careMode = "Neonatal"` dan `requiresIntensiveCare = true`. Menyembunyikan rincian kamar ibu. |
| **Lanjut ke Penempatan** | `primary` | Aktif bila: kartu jenis perawatan dipilih, sumber keputusan terisi, dan DPJP bayi telah dipilih. | Memvalidasi seluruh masukan Langkah 3 dan melangkah ke Langkah 4 Penempatan Tempat Tidur. |
| **Kembali** | `secondary` | Selalu aktif. | Kembali ke Langkah 2 Identitas Bayi. |

---

### Tabel Keadaan Layar (*States*)

| Keadaan | Ciri di Layar | Perilaku Komponen |
| :--- | :--- | :--- |
| **Inisial (Belum Memilih)** | 3 kartu tampil netral, tombol Lanjut belum aktif. | Membimbing petugas untuk memilih salah satu dari 3 mode perawatan. |
| **Kartu 1 Terpilih (Ibu di Kamar VIP 301-305)** | Kartu 1 bergaris tepi biru/hijau aktif. Muncul info kamar ibu aktif dan mendukung boks bayi. | Tombol Lanjut aktif bila DPJP dan sumber keputusan terisi. |
| **Kartu 1 Terpilih (Kamar Ibu Closed / Pulang)** | Muncul peringatan peringatan kuning/merah: *"Kamar ibu berstatus closed atau tidak aktif"*. | Tombol Lanjut dinonaktifkan, menyarankan memilih Kartu 2 atau 3. |
| **Kartu 2 Terpilih (Perinatologi)** | Kartu 2 bergaris tepi aktif. Di Langkah 4 nantinya kamar `BAYI-1` dan `BAYI-2` (Lantai 2) akan dapat diklik. | Menyiapkan konteks penempatan kamar perinatologi non-intensif. |
| **Kartu 3 Terpilih (NICU)** | Kartu 3 bergaris tepi aktif dengan badge merah *"Intensif Neonatal"*. Di Langkah 4 nantinya kamar `Ruang NICU` akan dapat diklik. | Menyiapkan konteks penempatan tempat tidur inkubator intensif. |

---

## 4. Rincian Perbaikan

### FIX-EPS-031-01 — Redesain UI Langkah 3 Menjadi 3 Kartu Pilihan Langsung

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-031-01` |
| **Area** | Frontend |
| **Jenis Perubahan** | Source frontend (`inpatient-newborn-admission-constants.js`, `inpatient-newborn-admission-steps.jsx`) |
| **Berkas yang Diubah** | `QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js`<br>`QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` |
| **Radius Dampak** | Formulir admisi bayi Langkah 3 dan sinkronisasi payload admisi bayi |
| **Bergantung pada** | Tidak ada |

**Langkah Pengerjaan:**
1. Di `inpatient-newborn-admission-constants.js`:
   - Definisikan 3 opsi eksplisit pada `NEWBORN_CARE_CARD_OPTIONS`:
     ```javascript
     export const NEWBORN_CARE_CARD_OPTIONS = Object.freeze([
       {
         key: "RoomingIn",
         title: "Rawat Gabung dengan Ibu",
         subtitle: "Rooming-In",
         description: "Bayi dirawat bersama ibu di kamar rawat inap ibu (kamar dengan fasilitas boks bayi).",
         badge: "Kamar Ibu (Lt. 3)",
         careMode: "RoomingIn",
         requiresIntensiveCare: false,
       },
       {
         key: "Perinatology",
         title: "Ruang Perinatologi",
         subtitle: "Neonatal Observasi / Reguler",
         description: "Bayi dirawat terpisah di kamar observasi bayi baru lahir non-intensif (Kamar BAYI-1 & BAYI-2).",
         badge: "Kamar Bayi (Lt. 2)",
         careMode: "Neonatal",
         requiresIntensiveCare: false,
       },
       {
         key: "NICU",
         title: "Ruang NICU",
         subtitle: "Neonatal Intensive Care Unit",
         description: "Perawatan intensif neonatal untuk bayi dengan kondisi gawat / darurat medis tinggi.",
         badge: "Unit Intensif (Lt. 2)",
         careMode: "Neonatal",
         requiresIntensiveCare: true,
       },
     ]);
     ```
2. Di `inpatient-newborn-admission-steps.jsx` pada komponen `NewbornCareStep`:
   - Ganti komponen `<Choices>` 2 radio dan checkbox `<BaseCheckboxField name="requiresIntensiveCare">` dengan visual 3 kartu langsung.
   - Sambungkan event klik kartu agar memperbarui state draf sekaligus:
     - `newborn.change("careMode", option.careMode)`
     - `newborn.change("requiresIntensiveCare", option.requiresIntensiveCare)`
   - Tentukan kartu yang aktif melalui kalkulasi:
     ```javascript
     const activeCareKey = newborn.draft.careMode === "RoomingIn"
       ? "RoomingIn"
       : newborn.draft.careMode === "Neonatal"
         ? (newborn.draft.requiresIntensiveCare ? "NICU" : "Perinatology")
         : "";
     ```
3. Perbarui unit test `tests/unit/inpatient-newborn-admission-steps.test.mjs` untuk menguji interaksi 3 kartu dan sinkronisasi datanya.

**Acceptance Criteria:**
- Langkah 3 menampilkan 3 kartu pilihan jelas tanpa ada kotak centang `requiresIntensiveCare` terpisah yang membingungkan.
- Mengklik Kartu 1 menghasilkan `careMode = "RoomingIn"` dan `requiresIntensiveCare = false`.
- Mengklik Kartu 2 menghasilkan `careMode = "Neonatal"` dan `requiresIntensiveCare = false`.
- Mengklik Kartu 3 menghasilkan `careMode = "Neonatal"` dan `requiresIntensiveCare = true`.

---

### FIX-EPS-031-02 — Planning Gate Pembersihan Data Penempatan & Reservasi Uji Coba

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-031-04` |
| **Area** | Database / Operasional Sandbox |
| **Target Database** | `QuilvianNewDevHamzah` (PostgreSQL `160.22.250.77:5432`) |
| **Jenis Perubahan** | SQL DML Update (Non-destructive safe state reset) |
| **Radius Dampak** | Mengembalikan status seluruh bed di Lantai 2 dan Lantai 3 menjadi `Available` |
| **Bergantung pada** | Persetujuan eksplisit pemilik sistem |

**Langkah Pengerjaan & Script SQL:**
Jalankan query DML berikut secara atomik di dalam satu blok transaksi:

```sql
-- ============================================================================
-- PLANNING GATE: Pembersihan Transaksi Uji Coba Penempatan dan Reservasi Bed
-- Target: Database QuilvianNewDevHamzah
-- Tujuan: Mengembalikan seluruh tempat tidur uji ke status Tersedia (Available)
-- ============================================================================

BEGIN TRANSACTION;

-- 1. Tutup seluruh penempatan uji coba yang masih aktif (EndDateTime IS NULL)
UPDATE public."InpBedPlacement"
SET 
    "EndDateTime" = NOW(),
    "EndReason" = 1, -- Selesai Normal / Discharged
    "UpdateDateTime" = NOW(),
    "UpdateByName" = 'Reset Testing State'
WHERE "EndDateTime" IS NULL 
  AND "IsDelete" = FALSE;

-- 2. Batalkan seluruh reservasi tempat tidur yang masih berstatus Aktif / Pending
UPDATE public."InpBedReservation"
SET 
    "ReservationStatus" = 3, -- Cancelled
    "ReleasedAt" = NOW(),
    "CancellationReason" = 'Pembersihan data reservasi uji coba admisi',
    "IsActive" = FALSE,
    "UpdateDateTime" = NOW(),
    "UpdateByName" = 'Reset Testing State'
WHERE "ReservationStatus" IN (1, 2) -- Active, Pending
  AND "IsDelete" = FALSE;

-- 3. Selesaikan seluruh siklus pembersihan tempat tidur yang menggantung
UPDATE public."InpBedCleaningAttempt"
SET 
    "State" = 3, -- Completed
    "CompletedAtUtc" = NOW(),
    "VerifiedAtUtc" = NOW(),
    "UpdateDateTime" = NOW(),
    "UpdateByName" = 'Reset Testing State'
WHERE "State" != 3
  AND "IsDelete" = FALSE;

UPDATE public."InpBedReadinessCycle"
SET 
    "Status" = 3, -- Completed
    "CompletedAtUtc" = NOW(),
    "UpdateDateTime" = NOW(),
    "UpdateByName" = 'Reset Testing State'
WHERE "Status" != 3
  AND "IsDelete" = FALSE;

-- 4. Kembalikan status fisik tempat tidur di tabel MstBed menjadi Available (1)
UPDATE public."MstBed"
SET 
    "BedStatus" = 1, -- Available
    "UpdateDateTime" = NOW(),
    "UpdateByName" = 'Reset Testing State'
WHERE "BedStatus" != 1
  AND "IsDelete" = FALSE;

COMMIT;
```

**Acceptance Criteria:**
- Seluruh 8 tempat tidur yang sebelumnya terkunci (`ICU-A`, `304-A`, `319-A`, `320-A`, `321-D`, `325-A`, `326-A`, `327-A`) kembali berstatus `Available`.
- Endpoint `GET /bed-management/floor-plan?floorCode=2` dan `floorCode=3` menampilkan seluruh bed dengan status `Available`.

---

### FIX-EPS-031-03 — Planning Gate Penambahan Boks Bayi pada Kamar VIP 301 s.d. 305 Lantai 3

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-031-02` |
| **Area** | Database / Master Data |
| **Target Database** | `QuilvianNewDevHamzah` (PostgreSQL `160.22.250.77:5432`) |
| **Jenis Perubahan** | SQL DML Insert (`MstBed`) dan SQL DML Update (`MstRoom`) |
| **Radius Dampak** | Kamar `VIP 301` s.d. `VIP 305` di Lantai 3 |
| **Bergantung pada** | Persetujuan eksplisit pemilik sistem |

**Langkah Pengerjaan & Script SQL:**
Jalankan query DML berikut untuk mendaftarkan 1 boks bayi per kamar VIP (total 5 boks) dan menaikkan kapasitas kamar menjadi 2:

```sql
-- ============================================================================
-- PLANNING GATE: Pendaftaran Boks Bayi Rawat Gabung di Kamar VIP 301 s.d. 305
-- Target: Database QuilvianNewDevHamzah
-- Tujuan: Mendukung opsi Rawat Gabung (Kartu 1) dengan tempat tidur boks bayi resmi
-- ============================================================================

BEGIN TRANSACTION;

-- 1. Naikkan kapasitas kamar VIP 301 s.d. 305 dari 1 menjadi 2 (1 ranjang ibu + 1 boks bayi)
UPDATE public."MstRoom"
SET 
    "Capacity" = 2,
    "UpdateDateTime" = NOW(),
    "UpdateByName" = 'Konfigurasi Boks Bayi Rawat Gabung'
WHERE "Id" IN (
    '445b9093-00bd-4e88-a890-c06018aed6ea', -- VIP 301
    'ac98907f-3764-4612-86f4-b5ce66b217f6', -- VIP 302
    '400341f5-a9ea-4cd0-a4b5-abd485e44835', -- VIP 303
    'd3f765f6-7547-4c1d-819c-339e7a54aadc', -- VIP 304
    '3e57deb2-5ef7-4ffc-94c2-0cbe6ee72a0f'  -- VIP 305
);

-- 2. Tambahkan 1 Boks Bayi di masing-masing kamar VIP 301 s.d. 305
-- Kamar VIP 301
INSERT INTO public."MstBed" (
    "Id", "RoomId", "BedCode", "BedName", "BedNumber", "BedStatus",
    "IsForNewborn", "IsIsolationBed", "IsIntensiveCareBed", "IsOdcBed",
    "IsReservable", "SortOrder", "Description", "IsActive", "IsDelete",
    "CreateDateTime", "CreateByName"
) VALUES (
    gen_random_uuid(), '445b9093-00bd-4e88-a890-c06018aed6ea', 'BD-RSMMC-BOKS-301',
    '301-BOKS', 'BOKS', 1, TRUE, FALSE, FALSE, FALSE, TRUE, 2,
    'Boks Bayi Rawat Gabung Kamar VIP 301', TRUE, FALSE, NOW(), 'Setup Boks Bayi'
) ON CONFLICT ("BedCode") DO NOTHING;

-- Kamar VIP 302
INSERT INTO public."MstBed" (
    "Id", "RoomId", "BedCode", "BedName", "BedNumber", "BedStatus",
    "IsForNewborn", "IsIsolationBed", "IsIntensiveCareBed", "IsOdcBed",
    "IsReservable", "SortOrder", "Description", "IsActive", "IsDelete",
    "CreateDateTime", "CreateByName"
) VALUES (
    gen_random_uuid(), 'ac98907f-3764-4612-86f4-b5ce66b217f6', 'BD-RSMMC-BOKS-302',
    '302-BOKS', 'BOKS', 1, TRUE, FALSE, FALSE, FALSE, TRUE, 2,
    'Boks Bayi Rawat Gabung Kamar VIP 302', TRUE, FALSE, NOW(), 'Setup Boks Bayi'
) ON CONFLICT ("BedCode") DO NOTHING;

-- Kamar VIP 303
INSERT INTO public."MstBed" (
    "Id", "RoomId", "BedCode", "BedName", "BedNumber", "BedStatus",
    "IsForNewborn", "IsIsolationBed", "IsIntensiveCareBed", "IsOdcBed",
    "IsReservable", "SortOrder", "Description", "IsActive", "IsDelete",
    "CreateDateTime", "CreateByName"
) VALUES (
    gen_random_uuid(), '400341f5-a9ea-4cd0-a4b5-abd485e44835', 'BD-RSMMC-BOKS-303',
    '303-BOKS', 'BOKS', 1, TRUE, FALSE, FALSE, FALSE, TRUE, 2,
    'Boks Bayi Rawat Gabung Kamar VIP 303', TRUE, FALSE, NOW(), 'Setup Boks Bayi'
) ON CONFLICT ("BedCode") DO NOTHING;

-- Kamar VIP 304
INSERT INTO public."MstBed" (
    "Id", "RoomId", "BedCode", "BedName", "BedNumber", "BedStatus",
    "IsForNewborn", "IsIsolationBed", "IsIntensiveCareBed", "IsOdcBed",
    "IsReservable", "SortOrder", "Description", "IsActive", "IsDelete",
    "CreateDateTime", "CreateByName"
) VALUES (
    gen_random_uuid(), 'd3f765f6-7547-4c1d-819c-339e7a54aadc', 'BD-RSMMC-BOKS-304',
    '304-BOKS', 'BOKS', 1, TRUE, FALSE, FALSE, FALSE, TRUE, 2,
    'Boks Bayi Rawat Gabung Kamar VIP 304', TRUE, FALSE, NOW(), 'Setup Boks Bayi'
) ON CONFLICT ("BedCode") DO NOTHING;

-- Kamar VIP 305
INSERT INTO public."MstBed" (
    "Id", "RoomId", "BedCode", "BedName", "BedNumber", "BedStatus",
    "IsForNewborn", "IsIsolationBed", "IsIntensiveCareBed", "IsOdcBed",
    "IsReservable", "SortOrder", "Description", "IsActive", "IsDelete",
    "CreateDateTime", "CreateByName"
) VALUES (
    gen_random_uuid(), '3e57deb2-5ef7-4ffc-94c2-0cbe6ee72a0f', 'BD-RSMMC-BOKS-305',
    '305-BOKS', 'BOKS', 1, TRUE, FALSE, FALSE, FALSE, TRUE, 2,
    'Boks Bayi Rawat Gabung Kamar VIP 305', TRUE, FALSE, NOW(), 'Setup Boks Bayi'
) ON CONFLICT ("BedCode") DO NOTHING;

COMMIT;
```

**Acceptance Criteria:**
- Kamar VIP 301 s.d. 305 memiliki 2 tempat tidur di database: Bed `A` (dewasa) dan Bed `BOKS` (`IsForNewborn = true`).
- Endpoint `GET /bed-management/floor-plan?floorCode=3` menampilkan boks bayi berstatus `Available` di kamar VIP 301 s.d. 305.
- Bayi dengan mode rawat gabung di kamar VIP 301–305 lolos validasi `InpNewbornAdmissionRules` tanpa kesalahan `NEWBORN_BED_REQUIRED`.

---

### FIX-EPS-031-04 — Planning Gate Pendaftaran Unit & Bed Percontohan NICU di Lantai 2

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-031-03` |
| **Area** | Database / Master Data |
| **Target Database** | `QuilvianNewDevHamzah` (PostgreSQL `160.22.250.77:5432`) |
| **Jenis Perubahan** | SQL DML Insert (`MstRoom` & `MstBed`) |
| **Radius Dampak** | Master Data Ruangan dan Tempat Tidur Lantai 2 |
| **Bergantung pada** | Persetujuan eksplisit pemilik sistem |

**Langkah Pengerjaan & Script SQL:**
Jalankan query DML berikut untuk mendaftarkan 1 kamar fisik NICU dan 2 inkubator intensif di Lantai 2:

```sql
-- ============================================================================
-- PLANNING GATE: Pendaftaran Kamar Fisik & Inkubator Intensif Ruang NICU (Lantai 2)
-- Target: Database QuilvianNewDevHamzah
-- Tujuan: Mendukung opsi Ruang NICU (Kartu 3) dengan kelas CLS-149 & IsIntensiveCare
-- ============================================================================

BEGIN TRANSACTION;

-- 1. Daftarkan Kamar Fisik Percontohan "Ruang NICU" di Lantai 2
INSERT INTO public."MstRoom" (
    "Id", "ServiceUnitId", "PatientClassId", "RoomCode", "RoomName",
    "RoomType", "RoomNumber", "LocationName", "FloorName", "FloorCode",
    "WingDirection", "FloorPlanRoomKey", "Capacity", "IsForNewborn",
    "IsIsolationRoom", "IsIntensiveCare", "IsOdcRoom", "IsAvailableForAdmission",
    "SortOrder", "Description", "IsActive", "IsDelete", "CreateDateTime", "CreateByName"
) VALUES (
    '8c946f01-5e72-4d56-a19e-e317bfd690a1',
    'dea54fab-052c-4c87-8fb6-b9e916c50b2d', -- ServiceUnit: Ruang Bayi (SU-BABY-001)
    '97ff6d1f-fd0d-4a4a-bb3c-ca23add7a0d5', -- PatientClass: NICU (CLS-149)
    'RM-RSMMC-NICU01',
    'Ruang NICU',
    2, -- Inpatient Room
    'NICU-1',
    'Lantai 2 · Barat',
    'Lantai 2',
    '2',
    7, -- Barat
    'lantai-2/nicu-1',
    2,
    TRUE,  -- IsForNewborn
    FALSE, -- IsIsolationRoom
    TRUE,  -- IsIntensiveCare
    FALSE,
    TRUE,  -- IsAvailableForAdmission
    5,
    'Kamar Perawatan Intensif Neonatal (NICU) Lantai 2',
    TRUE,
    FALSE,
    NOW(),
    'Setup Unit NICU'
) ON CONFLICT ("RoomCode") DO NOTHING;

-- 2. Daftarkan 2 Tempat Tidur Inkubator Intensif di Ruang NICU
INSERT INTO public."MstBed" (
    "Id", "RoomId", "BedCode", "BedName", "BedNumber", "BedStatus",
    "IsForNewborn", "IsIsolationBed", "IsIntensiveCareBed", "IsOdcBed",
    "IsReservable", "SortOrder", "Description", "IsActive", "IsDelete",
    "CreateDateTime", "CreateByName"
) VALUES (
    '9b837e12-4d61-4e89-8a21-f428c9e78111',
    '8c946f01-5e72-4d56-a19e-e317bfd690a1',
    'BD-RSMMC-NICU-01',
    'NICU-1-A',
    'A',
    1, -- Available
    TRUE, -- IsForNewborn
    FALSE,
    TRUE, -- IsIntensiveCareBed
    FALSE,
    TRUE,
    1,
    'Inkubator Intensif Neonatal NICU 1-A',
    TRUE,
    FALSE,
    NOW(),
    'Setup Unit NICU'
), (
    '9b837e12-4d61-4e89-8a21-f428c9e78222',
    '8c946f01-5e72-4d56-a19e-e317bfd690a1',
    'BD-RSMMC-NICU-02',
    'NICU-1-B',
    'B',
    1, -- Available
    TRUE, -- IsForNewborn
    FALSE,
    TRUE, -- IsIntensiveCareBed
    FALSE,
    TRUE,
    2,
    'Inkubator Intensif Neonatal NICU 1-B',
    TRUE,
    FALSE,
    NOW(),
    'Setup Unit NICU'
) ON CONFLICT ("BedCode") DO NOTHING;

COMMIT;
```

**Acceptance Criteria:**
- Kamar `Ruang NICU` (`NICU-1`) terdaftar dengan kelas `NICU` (`CLS-149`) dan `IsIntensiveCare = true`.
- Dua tempat tidur inkubator `NICU-1-A` dan `NICU-1-B` bertanda `IsForNewborn = true` dan `IsIntensiveCareBed = true` berstatus `Available`.
- Bayi dengan mode NICU (Kartu 3) lolos validasi `InpNewbornAdmissionRules` dan tempat tidur NICU dapat dipilih di Langkah 4.

---

## 5. Urutan Pengerjaan & Dependency Graph

```text
FIX-EPS-031-02 (Pembersihan DB Uji Coba — Safe State Reset)
├── FIX-EPS-031-03 (Pendaftaran Boks Bayi di Kamar VIP 301 s.d. 305 Lantai 3)
└── FIX-EPS-031-04 (Pendaftaran Kamar & Inkubator NICU di Lantai 2)
    └── FIX-EPS-031-01 (Redesain UI Langkah 3: 3 Kartu Pilihan Langsung)
```

1. **Fase 1 (Database Reset & Master Data)**: Jalankan query `FIX-EPS-031-02`, `FIX-EPS-031-03`, dan `FIX-EPS-031-04` pada database `QuilvianNewDevHamzah` setelah persetujuan pemilik diperoleh.
2. **Fase 2 (Frontend UI Redesign)**: Implementasikan pembaruan komponen `NewbornCareStep` pada `QuilvianSystemFrontendDev` (`FIX-EPS-031-01`).
3. **Fase 3 (Verifikasi End-to-End)**: Uji alur ketiga jenis kartu admisi bayi di peramban secara langsung.

---

## 6. Keputusan Pemilik yang Dibutuhkan / Disetujui

| No | Keputusan | Pilihan yang Tersedia | Status / Keputusan | Catatan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Model tampilan jenis perawatan bayi pada Langkah 3 | **1**: 3 kartu pilihan langsung.<br>**2**: 2 kartu + checkbox intensif. | **DISETUJUI: Pilihan 1** (3 Kartu Pilihan Langsung). | Disetujui pemilik sistem pada wawancara 21 Oktober 2026. |
| **K-02** | Lingkup kamar rawat gabung di Lantai 3 | **1**: Sebagian kamar privat (VIP 301 s.d. 305).<br>**2**: Seluruh 29 kamar di Lantai 3. | **DISETUJUI: Pilihan 1** (VIP 301 s.d. 305). | Mengikuti arahan pemilik: *"sebagian kamar saja yang mendukung support bayi"*. |
| **K-03** | Lokasi kamar percontohan NICU | **1**: Lantai 2 satu sayap dengan Perinatologi.<br>**2**: Lantai 3 bersama rawat inap. | **DISETUJUI: Pilihan 1** (Lantai 2). | Berada satu lantai dengan Ruang Bayi (Perinatologi) agar efisien secara operasional medis. |
| **K-04** | Eksekusi SQL pembersihan data penempatan uji coba | **1**: Setujui eksekusi script SQL di Bagian 4.<br>**2**: Tunda pembersihan. | ⏳ **MENUNGGU PERSETUJUAN EKSPLISIT** | Sesuai aturan `manage-database-operations`, eksekusi SQL menanti persetujuan pemilik atas dokumen ini. |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian Terkait | Kondisi Sebelumnya | Kondisi Menjadi |
| :--- | :--- | :--- | :--- |
| `01-business-process.md` | Alur Admisi Bayi Baru Lahir | Menggambarkan pilihan 2 mode dasar (Rawat Gabung vs Neonatal) dengan syarat intensif terpisah. | Menegaskan 3 alur penempatan klinis eksplisit: Rawat Gabung di Kamar VIP Ibu, Perinatologi Reguler, dan NICU Intensif. |
| `03-frontend-architecture.md` | Komponen Admisi Bayi (`NewbornCareStep`) | Komponen menggunakan Choices 2 radio dan 1 checkbox intensif. | Komponen menggunakan 3 kartu interaktif langsung dengan evaluasi otomatis properti payload. |
| `05-skema-tampilan.md` | Langkah 3 Formulir Admisi Bayi | Skema radio button lama. | Skema 3 kartu bertingkat modern sesuai standar kartu token Quilvian. |

---

## 8. Verifikasi Menyeluruh (Skenario E2E)

Pengujian komprehensif akan dilakukan setelah script SQL dieksekusi dan kode frontend diperbarui:

1. **Skenario 1 — Admisi Bayi Kartu 1: Rawat Gabung dengan Ibu:**
   - Pilih ibu persalinan yang dirawat di kamar `VIP 301`.
   - Di Langkah 3, klik **Kartu 1 (Rawat Gabung)**. Info kamar `VIP 301` muncul.
   - Di Langkah 4, denah otomatis membuka Lantai 3 menyorot kamar `VIP 301`.
   - Boks bayi `301-BOKS` dapat dipilih. Alur berlanjut mulus hingga aktivasi admisi.

2. **Skenario 2 — Admisi Bayi Kartu 2: Ruang Perinatologi (Observasi Reguler):**
   - Di Langkah 3, klik **Kartu 2 (Ruang Perinatologi)**.
   - Di Langkah 4, denah otomatis membuka Lantai 2. Kamar `BAYI-1` dan `BAYI-2` berwarna hijau dan dapat diklik.
   - Pilih salah satu dari 8 boks bayi perinatologi (`BAYI-1-A` s.d. `BAYI-2-D`). Penempatan sukses tanpa validasi intensif.

3. **Skenario 3 — Admisi Bayi Kartu 3: Ruang NICU (Perawatan Intensif Kritis):**
   - Di Langkah 3, klik **Kartu 3 (Ruang NICU)**.
   - Di Langkah 4, denah membuka Lantai 2. Kamar `Ruang NICU` (`NICU-1`) berwarna hijau interaktif dan dapat diklik.
   - Pilih salah satu inkubator `NICU-1-A` atau `NICU-1-B`. Validasi klinis `IsIntensiveCareBed = true` terpenuhi dengan sempurna.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-21 | Rencana perbaikan dibuat lengkap dengan 4 butir tindakan terstruktur, skema tampilan 3 kartu ASCII, script SQL pembersihan database, pendaftaran boks VIP 301-305, dan pendaftaran unit NICU. Menunggu persetujuan eksekusi pemilik sistem. | `diagnose-module-issue` |
| 2026-10-22 | Rencana disetujui Pemilik Sistem. Eksekusi database QuilvianNewDevHamzah selesai dan terverifikasi, implementasi frontend 3 kartu selesai dan 13/13 pengujian lulus. Status perbaikan ditandai SELESAI. | Tim Rekayasa Antigravity |
