# PLAN-REPAIR-009 — Restrukturisasi Alur Discharge Dokter Rawat Inap: Kondisional Otomatis Resume ODC/Ranap, Integrasi Lengkap Keputusan Pulang, dan Penataan Ulang Status Episode Full-Width (col-md-12)

```yaml
plan_id: PLAN-REPAIR-DOK-009
issue: ../issue/issue-009-alur-discharge-dan-kondisional-resume-medis.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-22"
diputuskan_oleh: "Pemilik Kebutuhan (User)"
tanggal_keputusan: "2026-10-22"
basis_source_backend: "41c6451e (MHamzah)"
basis_source_frontend: "dc92a799a (HamzahV2)"
ditulis_dengan: diagnose-module-issue
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Perbaikan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| `FIX-DOK-009-01` | `ISS-DOK-009-01` | Deteksi otomatis kondisional Resume ODC vs Rawat Inap tanpa sakelar tab manual | FE | 2 | — | ✅ SELESAI | `inpatient-resume-tab.jsx` |
| `FIX-DOK-009-02` | `ISS-DOK-009-02`, `ISS-DOK-009-T1` | Integrasi alur pemulangan lengkap (Alur Discharge, Tahap 1 Keputusan Pulang, Tahap 2 Resume & E-Sign, Status Kasir) ke Ruang Kerja Dokter | FE | 1 | — | ✅ SELESAI | `inpatient-resume-tab.jsx` & `inpatient-discharge-view.jsx` |
| `FIX-DOK-009-03` | `ISS-DOK-009-03` | Restrukturisasi layout Ringkasan Operasional / Status Episode menjadi Full Width `col-md-12` pada Ruang Kerja Episode & Dokter | FE | 2 | — | ✅ SELESAI | `inpatient-discharge-view.module.css` |

**Ringkasan: 3 dari 3 perbaikan selesai (100% Terimplementasi dan Lulus Linter).**

---

## 2. Solusi Terpilih per Temuan

### ISS-DOK-009-01 — Opsi Manual Resume ODC vs Rawat Inap Membebani Dokter

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | **Deteksi Otomatis Berbasis Metadata Episode (`isOdcEpisode`)** | Dokter tidak perlu memilih tab secara manual; formulir yang tampil dijamin tepat 100% sesuai unit/kelas pasien | Perlu utilitas pencocokan nama unit/kamar yang andal |
| **B** | Tetap mempertahankan switch 3 segmen manual (Ranap / ODC / Riwayat) | Tidak mengubah arsitektur navigasi | Rawan salah klik oleh dokter, membingungkan saat pasien ODC membuka ranap |
| **C** | Membuat menu terpisah di sidebar kiri untuk "Dokter - Rawat ODC" | Pemisahan modul sangat tegas | Menambah kerumitan navigasi sidebar rumah sakit |

**Solusi Terpilih: Opsi A.**
1. Sistem mengevaluasi `patientClassName`, `roomName`, atau `serviceUnitName`. Jika mengandung string `"ODC"` atau flag `isOdc === true`, sistem langsung merender form Resume ODC. Jika bangsal reguler, langsung form Rawat Inap.
2. Sub-navigasi disederhanakan menjadi 2 segmen kontekstual: **Formulir Resume** dan **Riwayat Revisi**.

---

### ISS-DOK-009-02 — Tab Resume Dokter Tidak Memiliki Akses Keputusan Pulang

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | **Integrasikan Komponen Alur Discharge Lengkap ke Tab Dokter** | Dokter menyelesaikan seluruh alur (Keputusan Pulang $\rightarrow$ Resume $\rightarrow$ E-Sign) di satu tab tanpa keluar menu | Memerlukan penyatuan hook `useInpatientDischarge` ke ruang kerja dokter |
| **B** | Menyediakan tombol tautan/redirect yang mengarahkan dokter ke halaman episode admisi | Sederhana dalam implementasi kode | Dokter tetap terpaksa keluar dari ruang kerja dokter; melanggar pemisahan hak akses dokter vs admisi |
| **C** | Menghapus aturan `GUARD-INP-03` (mengizinkan resume diisi tanpa keputusan pulang) | Form tidak terkunci | **Melanggar medikolegal klinis** — ringkasan pulang tidak boleh sah jika pasien belum diputuskan pulang |

**Solusi Terpilih: Opsi A.**
1. Di ruang kerja dokter, tab *Resume Medis* mengorkestrasi alur pemulangan lengkap:
   - Stepper **Alur Discharge** (Keputusan Pulang $\rightarrow$ Resume Pulang $\rightarrow$ Tanda Tangan DPJP $\rightarrow$ Final).
   - **Tahap 1: Keputusan Pulang**: Jika belum diputuskan pulang, dokter dapat memilih cara pulang dan mengeklik *"Simpan Keputusan Pulang"* langsung di tab ini.
   - **Tahap 2: Resume Pulang**: Form terbuka otomatis segera setelah Keputusan Pulang tersimpan.
   - **Status Kasir & Riwayat Versi**: Pemantauan izin kasir real-time dan histori revisi.
2. Halaman `/episodes/[id]/discharge` dipertahankan untuk staf admisi sebagai view operasional pemulangan.

---

### ISS-DOK-009-03 — Layout Sidebar Ringkasan Operasional Mempersempit Form dan Tidak `col-md-12`

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | **Ubah Layout Menjadi Single-Column Full Width (`col-md-12`) dengan Banner Horizontal** | Form medis memiliki ruang baca lebar maksimal (100%); seluruh informasi operasional tetap terbaca jelas | Memerlukan penyesuaian CSS grid dan styling card status |
| **B** | Tetap 2 kolom tapi memperkecil sidebar menjadi 15% | Perubahan minimal | Teks di sidebar terpotong dan form tetap tidak maksimal |
| **C** | Menyembunyikan card Ringkasan Operasional ke dalam modal popup | Halaman sangat bersih | Status tempat tidur dan catatan operasional tidak terpantau secara sekilas |

**Solusi Terpilih: Opsi A.**
Card *STATUS EPISODE - Ringkasan Operasional* diposisikan membentang penuh (`col-md-12` / `col-12`) tepat di bawah Card Alur Discharge, menyajikan indikator status tempat tidur, DPJP, cara pulang, dan langkah selanjutnya dalam tata letak horizontal yang rapi.

---

## 3. Skema Tampilan: Sebelum $\rightarrow$ Sesudah

### Tampilan SEBELUM Perbaikan (Ruang Kerja Dokter & Episode)

```text
+----------------------------------------------------------------------------------------------------+
| Dokter - Rawat Inap  [Total Pasien 5] [Dirawat 5] [Discharge Pending 0]                           |
+----------------------------------------------------------------------------------------------------+
| [ FARHAN ALAMSYAH ]  | [ SOAP ] [ CPPT ] [ Hasil Skrining ] [ Resep ] [ Tindakan ] [ Resume Medis* ]|
| RM: 00-00-00-37      |-----------------------------------------------------------------------------|
| ODC 321A - Bed 321-A | [ (•) Resume Rawat Inap ]   [ ( ) Resume ODC ]   [ ( ) Riwayat Revisi ]      |
|                      |-----------------------------------------------------------------------------|
|                      | ⚠ Pengisian Resume Ditahan                                                  |
|                      | Pengisian dan penandatanganan resume hanya berhak dilakukan oleh DPJP aktif |
|                      |                                                                             |
|                      | 1. Diagnosis Pasien (Wajib)                                                 |
|                      | [ Tidak ada catatan                                            ] (DISABLED) |
|                      | [ Tidak ada catatan                                            ] (DISABLED) |
|                      |                                                                             |
|                      | (Dokter terpaksa keluar menu mencari layar Admisi untuk Keputusan Pulang)    |
+----------------------------------------------------------------------------------------------------+
```

---

### Tampilan SESUDAH Perbaikan (Rekomendasi Terbaik — Full Width `col-md-12`)

```text
+----------------------------------------------------------------------------------------------------+
| Dokter - Rawat Inap  [Total Pasien 5] [Dirawat 5] [Discharge Pending 0]                           |
+----------------------------------------------------------------------------------------------------+
| [ FARHAN ALAMSYAH ]  | [ SOAP ] [ CPPT ] [ Hasil Skrining ] [ Resep ] [ Tindakan ] [ Resume Medis* ]|
| RM: 00-00-00-37      |-----------------------------------------------------------------------------|
| ODC 321A - Bed 321-A | Sub-Tab: [ (•) Formulir Resume Medis (ODC) ]   [ ( ) Riwayat Revisi (0) ]   |
| (Otomatis ODC)       |-----------------------------------------------------------------------------|
|                      |                                                                             |
|                      | 1. ALUR DISCHARGE: Progres Keputusan dan Resume (col-md-12)                 |
|                      | +-------------------------------------------------------------------------+ |
|                      | | (1) Keputusan Pulang     (2) Resume Pulang    (3) Tanda Tangan    (4)   | |
|                      | |     [SAAT INI]               [TERKUNCI]           [TERKUNCI]     FINAL  | |
|                      | +-------------------------------------------------------------------------+ |
|                      |                                                                             |
|                      | 2. STATUS EPISODE: Ringkasan Operasional (col-md-12 Banner)                 |
|                      | +-------------------------------------------------------------------------+ |
|                      | | Status: [ ADMITTED ]  •  Bed: ODC 321A (Aktif)  •  DPJP: dr. Rendy      | |
|                      | | Cara Pulang: Belum Diputuskan  •  Resume: Belum Disusun                  | |
|                      | | ➔ Langkah Berikutnya: Tetapkan cara pulang untuk membuka resume medis.   | |
|                      | +-------------------------------------------------------------------------+ |
|                      |                                                                             |
|                      | 3. TAHAP 1: Keputusan Pemulangan Pasien (col-md-12)                         |
|                      | +-------------------------------------------------------------------------+ |
|                      | | Cara Pulang: (•) Sembuh/Perbaikan  ( ) Rujuk  ( ) PAPS  ( ) Meninggal     | |
|                      | | Alasan Pulang:                                                          | |
|                      | | [ Keadaan klinis membaik, afebris 24 jam, tanda vital dalam batas normal] | |
|                      | | [ Simpan Keputusan Pulang ]                                             | |
|                      | +-------------------------------------------------------------------------+ |
|                      |                                                                             |
|                      | 4. TAHAP 2: Formulir Resume Medis (col-md-12 — Otomatis Aktif saat Selesai)|
|                      | +-------------------------------------------------------------------------+ |
|                      | | 💡 Usulan Klinis: [ Isi dari Data Klinis (SOAP, Resep, Tindakan) ]      | |
|                      | | 1. Diagnosis Utama & Sekunder                                           | |
|                      | |    [ ICD-10 Diagnosis Utama .......................................... ]| |
|                      | | 2. Ringkasan Perjalanan Penyakit & Anamnesis                            | |
|                      | |    [ Catatan perkembangan klinis pasien selama di bangsal ............ ]| |
|                      | | 3. Tindakan Medis & Pemeriksaan Penunjang Laboratorium/Radiologi         | |
|                      | | 4. Terapi Obat Pulang & Anjuran Kontrol Poliklinik                      | |
|                      | | [ Simpan Draf Resume ]            [ Tanda Tangani Resume (E-Sign) ]     | |
|                      | +-------------------------------------------------------------------------+ |
|                      |                                                                             |
|                      | 5. STATUS KASIR & AUDIT KLINIS (col-md-12)                                  |
|                      | +-------------------------------------------------------------------------+ |
|                      | | Izin Pulang Kasir: [ Menunggu Penilaian Kasir ]  [ Perbarui Status ]     | |
|                      | | Riwayat Versi: Belum ada versi final yang ditandatangani.                | |
|                      | +-------------------------------------------------------------------------+ |
+----------------------------------------------------------------------------------------------------+
```

---

### Tabel Wilayah Antarmuka (Sesudah Perbaikan)

| No | Wilayah | Isi & Elemen | Sumber Data / Komponen | Perilaku Responsif |
| :-: | :--- | :--- | :--- | :--- |
| **1** | **Sub-Navigasi Tab** | Segmented nav: Formulir Resume dan Riwayat Revisi | `ClinicalSegmentedNav` | Otomatis deteksi ODC vs Ranap reguler |
| **2** | **Alur Discharge** | Stepper horizontal progres 4 tahap | `WorkflowIndicator` | `col-12` full width, badge status dinamis |
| **3** | **Ringkasan Operasional** | Status episode, bed, DPJP, cara pulang, dan langkah berikutnya | `SidePanel` $\rightarrow$ `DischargeOperationalBanner` | `col-md-12` full width, card horizontal elegan |
| **4** | **Tahap 1: Keputusan Pulang** | Radio pilihan cara pulang, textarea alasan, tombol simpan | `DecisionSection` | `col-12` full width, otomatis terkunci hijau saat tersimpan |
| **5** | **Tahap 2: Resume Medis** | 8 bagian form klinis terstruktur, tombol prefill usulan data klinis, draf, dan e-sign | `ResumeSection` / `ResumeFormPanel` | `col-12` full width, terbuka otomatis saat Tahap 1 selesai |
| **6** | **Status Kasir & Audit** | Kartu status billing kelayakan kasir dan riwayat versi | `BillingSummaryCard` & `RevisionHistory` | `col-12` full width di bagian bawah alur |

---

### Tabel Tombol dan Aksi (Sesudah Perbaikan)

| Label Tombol | Jenis / Varian | Kapan Aktif | Aksi yang Terjadi saat Ditekan |
| :--- | :--- | :--- | :--- |
| **Simpan Keputusan Pulang** | `Primary` | Saat DPJP telah memilih cara pulang & alasan tidak kosong | Membuka modal konfirmasi $\rightarrow$ panggil API `/decide` $\rightarrow$ status episode berubah `DischargePending` $\rightarrow$ Tahap 2 terbuka seketika |
| **Isi dari Data Klinis** | `Secondary` | Saat form resume aktif & belum ditandatangani | Memanggil API `/prefill` $\rightarrow$ menyalin diagnosis SOAP, tindakan medis, dan resep farmasi ke kolom resume |
| **Simpan Draf Resume** | `Secondary` | Saat form resume aktif & ada perubahan isian | Memanggil API `PUT /summary` $\rightarrow$ menyimpan draf tanpa memvalidasi kelengkapan final |
| **Tanda Tangani Resume (E-Sign)** | `Primary` | Saat diagnosis utama terisi & DPJP berhak | Membuka modal konfirmasi E-Sign $\rightarrow$ panggil API `PATCH /sign` $\rightarrow$ resume terkunci permanen |
| **Perbarui Status Kasir** | `Outline` | Selalu aktif setelah keputusan pulang terbit | Memanggil API billing clearance untuk mengecek pelunasan di loket kasir secara real-time |

---

## 4. Rincian Perbaikan

### FIX-DOK-009-01 — Kondisional Otomatis Resume ODC vs Rawat Inap
* **Berkas yang Diubah**: `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/resume/inpatient-resume-tab.jsx`
* **Perubahan**:
  - Hapus array statis `INPATIENT_RESUME_SEGMENTS` yang menampilkan pilihan manual ODC vs Ranap.
  - Implementasikan fungsi selektor `resolveEpisodeCareMode(episode, placement)`:
    - Jika `isOdc` $\rightarrow$ Tampilkan komponen Resume ODC.
    - Jika `isRanap` $\rightarrow$ Tampilkan komponen Resume Rawat Inap.
  - Sub-navigasi hanya menampilkan tab **Formulir Resume** dan **Riwayat Revisi**.

---

### FIX-DOK-009-02 — Integrasi Lengkap Alur Pemulangan ke Ruang Kerja Dokter
* **Berkas yang Diubah**:
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/resume/inpatient-resume-tab.jsx`
  - `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-resume-tab.jsx`
* **Perubahan**:
  - Tambahkan state dan action handler pemulangan (`handleDecideDischarge`, modal konfirmasi keputusan pulang).
  - Susun urutan kartu: `WorkflowIndicator` $\rightarrow$ `DischargeOperationalBanner` $\rightarrow$ `DecisionSection` $\rightarrow$ `ResumeSection` $\rightarrow$ `BillingSummaryCard` $\rightarrow$ `RevisionHistory`.
  - Pasien dengan status `Admitted` tidak lagi melihat banner buntu, melainkan form penetapan cara pulang aktif.

---

### FIX-DOK-009-03 — Restrukturisasi Layout Status Episode Menjadi `col-md-12`
* **Berkas yang Diubah**:
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx`
  - `QuilvianSystemFrontendDev/src/style/health-services/inpatient-management/inpatient-discharge-view.module.css`
  - `QuilvianSystemFrontendDev/src/style/health-services/inpatient-management/physician-resume.module.css`
* **Perubahan**:
  - Ubah `.workspaceGrid` dari 2 kolom (`grid-template-columns: 3fr 1.12fr`) menjadi layout single column full-width (`grid-template-columns: 1fr`).
  - Restrukturisasi `SidePanel` menjadi `DischargeOperationalBanner` berbasis card horizontal `col-md-12`.

---

## 5. Urutan Pengerjaan & Dependency Graph

```text
FIX-DOK-009-01 (Kondisional Otomatis ODC vs Ranap)
  │
  ▼
FIX-DOK-009-03 (Restrukturisasi Layout col-md-12 pada Komponen Discharge)
  │
  ▼
FIX-DOK-009-02 (Pemasangan Alur Discharge Terpadu ke Ruang Kerja Dokter)
```

---

## 6. Verifikasi Menyeluruh (End-to-End Test Scenario)

1. **Uji Kasus Pasien ODC (Farhan Alamsyah — Bed ODC 321A)**:
   - Buka ruang kerja dokter, pilih Farhan Alamsyah, klik tab *Resume Medis*.
   - Pastikan tidak ada tombol switch manual "Resume Rawat Inap". Sistem langsung menampilkan form resume kontekstual ODC.
   - Stepper Alur Discharge tampil full width `col-md-12`.
   - Dokter memilih cara pulang *Sembuh/Perbaikan Klinis*, isi alasan, klik *Simpan Keputusan Pulang*.
   - Episode berpindah ke `DischargePending`, form resume terbuka aktif seketika.
   - Klik *Isi dari Data Klinis*, pastikan diagnosis SOAP, tindakan, dan resep terisi otomatis.
   - Klik *Tanda Tangani Resume (E-Sign)*, pastikan resume berhasil disahkan dan terkunci final.
2. **Uji Kasus Ruang Kerja Admisi (`/episodes/[id]/discharge`)**:
   - Buka menu admisi episode Farhan Alamsyah.
   - Pastikan layout kini membentang penuh `col-md-12` tanpa sidebar sempit yang menjepit formulir.
   - Status tertera *Resume Final* dan *Menunggu Kasir*.

---

## 7. Riwayat Dokumen

| Tanggal | Versi | Pengubah | Keterangan |
| :--- | :---: | :--- | :--- |
| **2026-10-22** | `1.0.0` | Antigravity AI (`diagnose-module-issue`) | Penyusunan dokumen rencana perbaikan awal untuk persetujuan pemilik |
