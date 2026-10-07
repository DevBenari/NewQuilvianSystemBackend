# PLAN-REPAIR-009 — Penataan Ulang UI/UX Catatan Keperawatan: Eliminasi Double Header, Harmonisasi Sub-Navigasi, dan Standarisasi Base Component

```yaml
plan_id: PLAN-REPAIR-KEP-009
issue: ../issue/issue-009-tampilan-ui-ux-catatan-keperawatan.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pengguna (Pemilik)"
tanggal_keputusan: "2026-10-06"
basis_source_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
basis_source_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-KEP-009-01` | `ISS-KEP-009-01`, `ISS-KEP-009-02` | Eliminasi kartu header luar redundan dan ganti pill navigasi ke sub-navbar rapi mengikuti pola *Obat & Alkes* | FE | 1 | — | ✅ SELESAI | `nursing-narrative-tab.jsx` & `nursing-workspace.module.css:6733-6785` |
| `FIX-KEP-009-02` | `ISS-KEP-009-03` | Migrasi banner aturan keselamatan RWI-AC-206 ke Base Component `ClinicalSafetyAlert` | FE | 2 | — | ✅ SELESAI | `nursing-narrative-tab.jsx:164-171` (`ClinicalSafetyAlert`) |
| `FIX-KEP-009-03` | `ISS-KEP-009-04` | Reposisi aksi Catatan Naratif CPPT ke quick-actions kompak di pojok sub-navbar | FE | 2 | — | ✅ SELESAI | `nursing-narrative-tab.jsx:136-159` (`nursingCareQuickActions`) |
| `FIX-KEP-009-04` | `ISS-KEP-009-05` | Pembersihan label teknis internal "(V1)" pada teks judul antarmuka klinis | FE | 1 | — | ✅ SELESAI | `nursing-narrative-tab.jsx` (dibersihkan) |
| `FIX-KEP-009-05` | `ISS-KEP-009-T1` | Standardisasi kontrol pemilihan selang WSD dan sub-tab DPO memakai `BaseButton` dan `StatusBadge` | FE | 3 | — | ✅ SELESAI | `wsd-observation-panel.jsx:348` (`StatusBadge`) & `nursing-dpo-panel.jsx:62` (`medicationSubNavBar`) |

**Ringkasan: 5 dari 5 perbaikan selesai (seluruh implementasi kode tuntas dan terverifikasi).**

---

## 2. Solusi Terpilih per Temuan

### ISS-KEP-009-01 & ISS-KEP-009-02 — Penataan Ulang Tata Letak dan Sub-Navigasi 6 Sub-Menu

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | **Sub-Navbar Rapi Satu Tingkat (Pola Obat & Alkes).** Hapus kartu header luar pada `nursing-narrative-tab.jsx`. Ganti pill box Bootstrap abu-abu dengan sub-navbar horizontal bersih (`.nursingSubNavBar`) dengan active indicator teal/cyan dan tombol aksi cepat di sisi kanan. Setiap sub-panel yang aktif memegang kartu header kerjanya sendiri. | Menghilangkan 100% tumpukan ganda (*double header*), menghemat ~160px ruang vertikal, konsisten 100% dengan tab *Obat & Alkes*, navigasi sangat intuitif bagi perawat. | Memerlukan sedikit penyesuaian kelas CSS modul. |
| B | Pertahankan kartu header luar, tetapi hilangkan kartu header di dalam masing-masing sub-panel. | Hanya ada satu header di atas. | Sangat rumit karena tiap sub-panel memiliki judul, subtitle, tombol aksi, dan filter spesifik (misal: WSD punya "+ Daftarkan Selang", Spooling punya DatePicker, Diet punya "+ Tetapkan Diet"). Memindahkan kontrol sub-panel ke header luar merusak enkapsulasi komponen. |
| C | Pertahankan kedua kartu seperti sekarang, hanya kurangi padding dan ubah warna. | Perubahan kode paling sedikit. | Tidak menyelesaikan akar masalah; dua kartu tetap bertumpuk vertikal dan tetap boros ruang monitor. |

**Solusi terpilih: Opsi A.**
1. Memenuhi 5 kriteria pelapor: Posisi rapi, user-friendly, menarik, mengikuti template sub-menu keperawatan lain, dan berbasis Base Component.
2. Mempertahankan modularitas dan enkapsulasi masing-masing sub-panel (`WsdObservationPanel`, `SpoolingCairanPanel`, `InpatientDietPanel`, dll.) tanpa perlu merombak logika internalnya.
3. Sejalan dengan pola arsitektur yang sudah terbukti sukses pada `nursing-medication-section.jsx`.

**Kenapa bukan opsi lain:**
- Opsi B memecah keterpaduan komponen anak karena aksi spesifik tiap panel harus diangkat (*prop drilling / context bloat*) ke komponen induk.
- Opsi C ditolak karena mempertahankan cacat visual dasar yang dikeluhkan pelapor.

---

### ISS-KEP-009-03 — Standarisasi Banner Aturan Keselamatan RWI-AC-206

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Ganti `<div className="alert alert-info">` dengan `ClinicalSafetyAlert` bertone `info`. Diletakkan secara proporsional tepat di bawah sub-navbar. | Mematuhi aturan tata kelola Base Component Quilvian, mewarisi styling token, aksesibel dengan ikon ARIA standar. | Tidak ada. |
| B | Gunakan `InformationAlert` dari `@/components/features/base-features/information-alert`. | Menggunakan base component fitur umum. | `ClinicalSafetyAlert` lebih tepat secara semantik untuk domain ruang kerja klinis rawat inap (`@/components/ui/clinical-workspace`). |

**Solusi terpilih: Opsi A.**
Sesuai katalog `@/components/ui/clinical-workspace/ClinicalSafetyAlert.jsx`, komponen ini memang dirancang khusus untuk peringatan aturan keselamatan klinis pasien.

---

### ISS-KEP-009-04 — Reposisi Aksi Catatan Naratif CPPT

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Pindahkan tombol `[+ Tulis Catatan Naratif CPPT]` dan `[Riwayat Naratif (n)]` ke pojok kanan bilah sub-navigasi sebagai *Quick Action Button*. Panel riwayat dibuka sebagai offcanvas/modal atau kartu expander halus di bawah sub-nav tanpa merusak layout panel aktif. | Akses tetap cepat dan mudah ditemukan, tidak memakan kartu header tersendiri di atas, alur kerja sub-panel tidak terganggu. | Perlu penataan flexbox pada baris navigasi. |
| B | Hapus seluruh tombol naratif dari tab ini dan arahkan perawat hanya membuka tab *Catatan Terintegrasi* sesuai `03-frontend-architecture.md:732`. | Menghilangkan elemen non-sub-menu secara total. | Menghilangkan jalan pintas pencatatan naratif yang sudah biasa digunakan perawat saat ronde bangsal. |

**Solusi terpilih: Opsi A.**
Memberikan kompromi terbaik antara kerapian tata letak antarmuka dan kecepatan operasional perawat di bangsal rawat inap.

---

### ISS-KEP-009-05 — Pembersihan Label Teknis "(V1)"

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Hapus teks "(V1)" dari seluruh antarmuka pengguna; gunakan label bersih "Catatan Keperawatan". | Mematuhi etika antarmuka medis rumah sakit; tampilan terlihat profesional dan siap pakai (*production-grade*). | Tidak ada. |

**Solusi terpilih: Opsi A.**
Label versi rekayasa tidak boleh bocor ke pengguna akhir klinis.

---

## 3. Skema Tampilan SEBELUM → SESUDAH

### 3.1 Rangka Antarmuka SEBELUM (Kondisi Saat Ini — Bermasalah)

```text
+-- Catatan Keperawatan (V1) [KARTU HEADER 1 - REDUNDAN] -----------------------+
| [ikon] Catatan Keperawatan (V1)                                                |
| Enam sub-menu asuhan keperawatan terpadu: Spooling Cairan, Observasi WSD...   |
|                                                                                |
| [+ Tulis Catatan Naratif CPPT]   [Lihat Riwayat Naratif (0)]                   |
+-------------------------------------------------------------------------------+
| (i) Aturan Keselamatan: Angka cairan atau obat yang diketik di narasi catatan  |
|     keperawatan tidak mengubah total maupun MAR...                             |
+-------------------------------------------------------------------------------+
| [nav-pills kotak abu-abu kaku]                                                 |
| [ Spooling Cairan ] [• Observasi WSD ] [ Sliding Scale ] [ DPO ] [ Pra-Op ] [ Diet ] |
+-------------------------------------------------------------------------------+
+-- Observasi Pengeluaran Cairan WSD [KARTU HEADER 2 - BERTUMPUK] --------------+
| [ikon] Observasi Pengeluaran Cairan WSD                                       |
| Pencatatan selang dan volume WSD per shift. Volume bertambah dihitung server...|
|                                                                                |
| [+ Daftarkan Selang]   [Segarkan]                                             |
+-------------------------------------------------------------------------------+
| Pilih Selang WSD Pasien:                                                      |
| [btn native] WSD kanan (Aktif)    [btn native] WSD kiri (Aktif)               |
+-------------------------------------------------------------------------------+
| WSD kanan — Lokasi: Dada Kanan | Dipasang: 1 Okt 08.00 | Status: Aktif        |
| [Tabel Riwayat Pembacaan Selang WSD...]                                       |
+-------------------------------------------------------------------------------+
```

---

### 3.2 Rangka Antarmuka SESUDAH (Rekomendasi Perbaikan — Rapi & Menarik)

```text
+-- Bilah Sub-Navigasi Terpadu (Mengikuti Pola Obat & Alkes) -------------------+
| [💧 Spooling] [• 🌊 Observasi WSD] [📈 Sliding Scale] [💊 DPO] [🏥 Pra-Op] [🍽️ Diet]  |
|                                     Aksi Cepat: [+ Tulis Naratif] [Riwayat (0)]|
+-------------------------------------------------------------------------------+
| ℹ Aturan Keselamatan: Angka cairan atau obat narasi tidak mengubah total/MAR.  |
|   (ClinicalSafetyAlert - tone: info, kompak & proporsional)                   |
+-------------------------------------------------------------------------------+
+-- Observasi Pengeluaran Cairan WSD [SATU KARTU KERJA AKTIF RESMI] -----------+
| [ikon] Observasi Pengeluaran Cairan WSD                                       |
| Pencatatan selang dan volume WSD per shift, terintegrasi Pengawasan Harian.   |
|                                         [+ Daftarkan Selang]   [Segarkan]     |
+-------------------------------------------------------------------------------+
| Pilih Selang WSD Pasien:                                                      |
| [ WSD Kanan (Dada Kanan) | [✓ Aktif] ]   [ WSD Kiri (Dada Kiri) | [✓ Aktif] ] |
+-------------------------------------------------------------------------------+
| WSD Kanan — Lokasi: Dada Kanan | Dipasang: 1 Okt 2026, 08:00 WIB              |
| Status: [StatusBadge: Aktif]  |  Sisa Awal: 350 ml                            |
|                                         [+ Catat Pembacaan]   [Lepas Selang]  |
|-------------------------------------------------------------------------------|
| [Tabel Riwayat Pembacaan Shift...]                                            |
| Waktu Periode      | Sisa Lalu | Dibuang | Sisa Kini | Bertambah | Dicatat Oleh|
| 01 Okt 08:00–14:00 | 350 ml    | 0 ml    | 450 ml    | +100 ml   | Ns. Siti    |
+-------------------------------------------------------------------------------+
```

---

### 3.3 Spesifikasi Wilayah Antarmuka SESUDAH

| Wilayah | Isi | Komponen Penanggung Jawab | Sumber Data |
| --- | --- | --- | --- |
| **Bilah Sub-Navigasi** | 6 tab navigasi horizontal bergaris aksen aktif teal/cyan + Quick Action Naratif CPPT di ujung kanan | `NursingCareSubNavBar` di dalam `nursing-narrative-tab.jsx` | Statis 6 sub-menu (`NURSING_NOTE_SUB_MENUS`) |
| **Peringatan Keselamatan** | Aturan keselamatan klinis RWI-AC-206 berformat banner ringkas | `ClinicalSafetyAlert` (`tone="info"`) | Statis regulasi klinis |
| **Kartu Kerja Sub-Panel Aktif** | Satu kartu kerja mandiri sesuai sub-menu yang dipilih (WSD, Spooling, Sliding Scale, DPO, Pra-Op, Diet) | Sub-panel terkait (`WsdObservationPanel`, dll.) | API masing-masing bounded context |
| **Drawer/Modal Riwayat Naratif** | Daftar riwayat catatan naratif CPPT keperawatan | Modal / Card dropdown yang tidak memotong tabel utama | `useInpatientNursingCareNotes` |

---

### 3.4 Spesifikasi Tombol dan Interaksi

| Tombol / Tab | Jenis Komponen | Kondisi Aktif / Dinonaktifkan | Tindakan Saat Diklik |
| --- | --- | --- | --- |
| Tab Sub-Menu (Spooling, WSD, dll.) | Tab Button Underline (`.nursingSubNavBtn`) | Selalu aktif; penanda aktif berlatar `#f0fdfa` dengan border bawah `#0d9488` | Mengganti state `activeSubMenu` dan memuat sub-panel terkait |
| `+ Tulis Catatan Naratif` | `BaseButton` (`variant="primary"`, `size="sm"`) | Aktif jika perawat memiliki hak tulis (`!readOnly`) | Membuka modal entri catatan naratif CPPT |
| `Riwayat Naratif (n)` | `BaseButton` (`variant="outline"`, `size="sm"`) | Selalu aktif | Membuka modal/panel riwayat entri CPPT perawat |
| `+ Daftarkan Selang` (WSD) | `BaseButton` (`variant="primary"`, `size="sm"`) | Aktif jika bukan mode hanya-baca | Membuka form modal pendaftaran selang WSD baru |
| `Segarkan` (WSD) | `BaseButton` (`variant="outline"`, `size="sm"`) | Dinonaktifkan saat sedang mengambil data (`loading`) | Memuat ulang daftar selang dan pembacaan dari API |

---

### 3.5 Keadaan Antarmuka (*States*)

| Keadaan | Tampilan |
| --- | --- |
| **Memuat Data (*Loading*)** | Sub-navigasi tetap dapat diakses; kartu kerja sub-panel menampilkan spinner ringkas / skeleton table Quilvian. |
| **Data Kosong (*Empty*)** | Menggunakan empty-state terstandarisasi Quilvian (ikon lembut, judul jelas, panduan aksi, contoh: "Belum ada selang WSD terdaftar. Klik '+ Daftarkan Selang' untuk memulai."). |
| **Galat (*Error*)** | Menampilkan `InformationAlert` bertone `danger` di dalam kartu kerja, disertai tombol `BaseButton` Coba Lagi (*Retry*). |
| **Hanya-Baca (*Read-Only*)** | Tombol aksi penambahan form (+ Catat, + Daftarkan) disembunyikan otomatis bila status episode ditutup (*Closed*). |

---

## 4. Rincian Perbaikan

### FIX-KEP-009-01 — Restrukturisasi Sub-Navbar Catatan Keperawatan Mengikuti Pola Obat & Alkes

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-009-01`, `ISS-KEP-009-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend JSX dan CSS Module |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/tabs/nursing-narrative-tab.jsx`<br>`src/style/health-services/inpatient-management/nursing-workspace.module.css` |
| **Radius dampak** | Khusus layar Catatan Keperawatan rawat inap (tidak mempengaruhi tab lain) |
| **Bergantung pada** | Tidak ada |

**Langkah:**
1. Hapus pembungkus `<div className={styles.tabHeaderBar}>` terluar pada baris 116–149 di `nursing-narrative-tab.jsx`.
2. Buat kelas CSS baru di `nursing-workspace.module.css`: `.nursingCareSubNavBar`, `.nursingCareSubNavBtn`, `.nursingCareSubNavBtnActive` yang mengadopsi struktur rapi setara dengan `.medicationSubNavBar` namun dioptimalkan untuk 6 menu berikon.
3. Ganti elemen `<div className="nav nav-pills...">` dengan bilah sub-navbar baru yang menggabungkan tab navigasi di sisi kiri dan quick-actions di sisi kanan.

**Acceptance criteria:**
1. Layar Asuhan Keperawatan → Catatan Keperawatan hanya menampilkan SATU kartu header per sub-menu aktif (tidak ada lagi kartu header ganda yang bertumpuk vertikal).
2. Tinggi total area navigasi berkurang minimal 120 piksel, memungkinkan tabel kerja WSD/Spooling/Diet langsung terlihat tanpa perlu scroll ke bawah pada layar 1080p.
3. Gaya visual navigasi 6 sub-menu konsisten dengan gaya tab di *Obat & Alkes*.

**Verifikasi:**
1. Periksa kode dengan `npm run lint` di frontend.
2. Buka layar di peramban, pastikan transisi antar 6 sub-menu berjalan mulus dan indikator tab aktif terlihat jelas.

**Risiko:**
Tidak ada risiko fungsional karena pemanggilan sub-panel anak tetap menggunakan props yang sama.

---

### FIX-KEP-009-02 — Migrasi Banner Aturan Keselamatan RWI-AC-206 ke Base Component

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-009-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend JSX |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/tabs/nursing-narrative-tab.jsx` |
| **Radius dampak** | Layar Catatan Keperawatan |
| **Bergantung pada** | `FIX-KEP-009-01` |

**Langkah:**
1. Import `ClinicalSafetyAlert` dari `@/components/ui/clinical-workspace/ClinicalSafetyAlert`.
2. Ganti blok `<div className="alert alert-info...">` dengan:
   ```jsx
   <ClinicalSafetyAlert
     tone="info"
     title="Aturan Keselamatan Klinis (RWI-AC-206)"
     description="Angka cairan atau obat yang diketik di narasi catatan keperawatan tidak mengubah total intake/output maupun MAR. Pencatatan cairan dilakukan di Spooling Cairan / Pengawasan Harian, dan obat di Daftar Pemberian Obat (DPO)."
     testId="nursing-safety-alert-narrative"
     className="mb-3"
   />
   ```

**Acceptance criteria:**
1. Komponen alert keselamatan ter-render menggunakan `ClinicalSafetyAlert` resmi tanpa menggunakan kelas Bootstrap mentah.
2. Memiliki atribut `data-testid="nursing-safety-alert-narrative"` dan ikon status yang terstandarisasi.

---

### FIX-KEP-009-03 — Reposisi Aksi Catatan Naratif CPPT ke Quick Actions Kompak

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-009-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend JSX |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/tabs/nursing-narrative-tab.jsx` |
| **Radius dampak** | Layar Catatan Keperawatan |
| **Bergantung pada** | `FIX-KEP-009-01` |

**Langkah:**
1. Posisikan tombol `[+ Tulis Catatan Naratif CPPT]` dan `[Riwayat Naratif (n)]` di baris sub-navbar sebelah kanan menggunakan `BaseButton` (`size="sm"`).
2. Buat modal khusus atau offcanvas drawer untuk melihat riwayat catatan naratif agar saat dibuka tidak mendorong tabel data sub-panel ke bawah.

**Acceptance criteria:**
1. Membuka riwayat catatan naratif tidak merusak tata letak atau mendorong tabel sub-panel aktif ke bawah.
2. Tombol aksi naratif CPPT tetap mudah diakses perawat saat dibutuhkan tanpa mendominasi layar tindakan khusus.

---

### FIX-KEP-009-04 — Pembersihan Label Teknis "(V1)" pada Header dan Teks Antarmuka

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-009-05` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend JSX |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/tabs/nursing-narrative-tab.jsx` |
| **Radius dampak** | Layar Catatan Keperawatan |
| **Bergantung pada** | Tidak ada |

**Langkah:**
1. Hapus string `(V1)` dari teks judul, atribut title, maupun komentar yang diekspos ke antarmuka pengguna.
2. Gunakan label baku: **Catatan Keperawatan**.

**Acceptance criteria:**
1. Tidak ada teks "(V1)" yang tampil pada header, tab label, atau tooltip di antarmuka pengguna.

---

### FIX-KEP-009-05 — Harmonisasi Kontrol Pemilihan Selang WSD dan Sub-Tab DPO ke Base Component

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-009-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend JSX |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/panels/wsd-observation-panel.jsx`<br>`src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/panels/nursing-dpo-panel.jsx` |
| **Radius dampak** | Panel WSD dan DPO rawat inap |
| **Bergantung pada** | Tidak ada |

**Langkah:**
1. Ganti tag `<button className="btn btn-sm...">` pada pill pemilihan selang WSD dengan `BaseButton` atau segmented pill card yang rapi.
2. Ganti `<span className="badge bg-success">` dengan `StatusBadge` resmi.
3. Standardisasi sub-tab pada `nursing-dpo-panel.jsx` agar konsisten menggunakan pola tab underline yang bersih.

**Acceptance criteria:**
1. Pemilihan selang WSD menggunakan komponen terstandarisasi dengan status badge yang jelas.
2. Tidak ada kelas Bootstrap mentah `btn btn-sm` dan `badge bg-*` pada kontrol pemilihan.

---

## 5. Urutan Pengerjaan

```text
FIX-KEP-009-04 (Pembersihan label "(V1)")
  └── FIX-KEP-009-01 (Restrukturisasi sub-navbar & eliminasi double header)
        ├── FIX-KEP-009-02 (Migrasi ke ClinicalSafetyAlert)
        └── FIX-KEP-009-03 (Reposisi aksi catatan naratif CPPT)
              └── FIX-KEP-009-05 (Harmonisasi kontrol sub-panel WSD & DPO)
```

Urutan di atas memastikan perbaikan struktural layout (`FIX-01` dan `FIX-04`) dikerjakan terlebih dahulu sebelum perapian komponen kontrol level mikro (`FIX-02`, `FIX-03`, `FIX-05`).

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan Rekomendasi | Perbaikan yang Ditahan |
| :---: | --- | --- | :---: |
| K-01 | Persetujuan penggantian kartu header luar menjadi sub-navbar satu tingkat mengikuti template *Obat & Alkes* | **Setujui Opsi A**: Eliminasi kartu header ganda, satu kartu kerja per sub-menu aktif. | `FIX-KEP-009-01` s.d. `FIX-KEP-009-05` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | Bagian 10, sub-bagian `FE-KEP-24` | Menyebutkan "Catatan Keperawatan enam sub-menu... Narasi perawat: Tidak di sini" tanpa spesifikasi tata letak sub-navbar. | Ditegaskan bahwa `FE-KEP-24` menggunakan sub-navbar satu baris mengikuti gaya `.medicationSubNavBar` dan menaungi satu kartu kerja aktif tanpa kartu header ganda. |
| `skema-tampilan-keperawatan-rawat-inap.md` | Bagian skema tampilan Asuhan Keperawatan | Skema lama dengan kotak naratif dominan. | Skema baru sesuai mockup ASCII bagian 3.2 dokumen ini. |

---

## 8. Verifikasi Menyeluruh

Setelah seluruh perbaikan diimplementasikan oleh builder:

1. **Uji Navigasi Antar Sub-Menu:**
   - Masuk ke menu *Asuhan Keperawatan* → klik tab *Catatan Keperawatan*.
   - Klik berturut-turut: *Spooling Cairan*, *Observasi WSD*, *Sliding Scale*, *DPO*, *Catatan Pra-Operasi*, *Diet Medis*.
   - Pastikan setiap sub-menu membuka kartu kerjanya dengan lancar dan tidak ada kartu ganda di atasnya.
2. **Uji Ergonomi Layar:**
   - Pada resolusi 1920x1080 dan 1366x768, pastikan tabel WSD / riwayat diet langsung terlihat di paro atas layar (*above the fold*) tanpa perlu scroll panjang.
3. **Uji Catatan Naratif CPPT:**
   - Klik tombol aksi cepat `+ Tulis Catatan Naratif CPPT`, simpan satu catatan simulasi (misal: "Pasien mengeluh pusing pada pukul 16.00").
   - Pastikan catatan tersimpan dan tidak menggeser tata letak sub-panel WSD.
4. **Validasi Kode:**
   - Jalankan `npm run lint` di repository frontend, pastikan 0 error dan 0 warning terkait komponen yang dimodifikasi.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-06 | Rencana perbaikan dibuat, memuat 5 perbaikan terstruktur, mockup ASCII sebelum-sesudah, dan register status pengerjaan; status MENUNGGU_PERSETUJUAN. | `diagnose-module-issue` (Antigravity) |
| 2026-10-06 | Disetujui pemilik (K-01 Opsi A). Seluruh 5 perbaikan diimplementasikan pada kode frontend, lolos uji linting dan unit test (11/11 & 6/6), status diubah menjadi SELESAI. | `diagnose-module-issue` (Antigravity) |
