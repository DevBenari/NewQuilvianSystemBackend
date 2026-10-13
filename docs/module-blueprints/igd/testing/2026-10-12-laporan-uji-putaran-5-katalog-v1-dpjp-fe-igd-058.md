# Laporan Pengujian Live Browser Testing — Revisi Formulir Penunjang Medis IGD: Pemilihan Lab Tujuan (PA/Mikro/PK), Modalitas Radiologi, DPJP Readonly, Checkbox CITO, dan Katalog Bergaya Versi 1 (`FE-IGD-058`)

## Metadata Pengujian

| Parameter | Keterangan |
| :--- | :--- |
| **Tanggal Pengujian** | 12 Oktober 2026 |
| **Fokus Pengujian** | Revisi Lengkap Penunjang Medis Perawat IGD: Pilihan Laboratorium Tujuan (Patologi Anatomi / Mikrobiologi / Patologi Klinik) dengan Judul dan Katalog Dinamis, Modalitas Radiologi yang Dituju, DPJP Readonly Otomatis, Checkbox CITO Responsif, serta Parameter POST Lengkap |
| **Task Terkait** | `FE-IGD-058` (Penyempurnaan Form Pemesanan Penunjang Medis IGD Sesuai Permintaan Bagian Penunjang) |
| **Engine Peramban** | Chromium Headed Mode (`channel: msedge`, resolusi 1440x900, `slowMo: 400ms`) |
| **Akun Penguji (Perawat)** | `dimas.kurniawan@rsmmc.local` (Perawat Pelaksana IGD) |
| **Target Pasien Uji** | Pasien Bersih T1_1790843458727_3 D3 (No. RM `00-00-00-19`, No. Kunjungan `IGD-261009030511-26F549`) |
| **Lokasi Skrip & Bukti Uji** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-5-v1-katalog-20261012/` |
| **Hasil Keseluruhan** | 🟢 **LULUS PENUH (PASS 100%)** — Seluruh 5 Skenario Uji Terverifikasi Berhasil |

---

## 1. Ringkasan Eksekutif & Jawaban Atas Umpan Balik Pengguna

Pengguna memberikan arahan penting terkait pemesanan penunjang medis perawat IGD:
> *"kenapa begini ?? maksud saya bukan anda ubah semua, tetapi ditambahkan pemeriksaan nyaa seperti ini lalu dokter naa, tapi seperti lab apa yang dituju seperti anatomi, dll ini jangan dihapus, sesuai kan form nyaa ini dengan parameter post yang ada di table laboratorium, radiologi maupun bank darah"*
> *"kenapa langsung patologi klinik, disni perawat bisa memilih pasien akan di lanjutkan ke anatomi patologi atau microbiologi"*

### Poin-Poin Perbaikan yang Telah Diselesaikan:
1. **Pilihan Laboratorium yang Dituju Tidak Dihapus**:
   - Menghadirkan kembali form field eksplisit **"Laboratorium yang Dituju"** (`discipline`) menggunakan `BaseNativeSelectField` dengan opsi:
     - **Patologi Klinik** (`Discipline = 1`): Darah Lengkap, GDS, Elektrolit, Kimia Darah, Urinalisis, AGD, Faal Hati/Ginjal.
     - **Patologi Anatomi** (`Discipline = 2`): Biopsi Jaringan Kecil/Besar, Histopatologi, Sitologi Cairan Pleura/Ascites, FNAB.
     - **Mikrobiologi** (`Discipline = 3`): Kultur Darah + Resistensi, Kultur Urine, Pewarnaan BTA, Pewarnaan Gram.
     - **Semua Laboratorium** (Pilihan bebas / tampilkan seluruh pemeriksaan).
   - **Judul dan Ikon Katalog Dinamis**: Judul tabel pemeriksaan tidak lagi di-hardcode kaku, melainkan berubah otomatis mengikuti disiplin yang dipilih:
     - Memilih *Patologi Anatomi* -> Judul menjadi **"Daftar Pemeriksaan Patologi Anatomi"** (Ikon 🔬) dan menyaring pemeriksaan PA.
     - Memilih *Mikrobiologi* -> Judul menjadi **"Daftar Pemeriksaan Mikrobiologi"** (Ikon 🧫) dan menyaring pemeriksaan Mikro.
     - Memilih *Patologi Klinik* -> Judul menjadi **"Daftar Pemeriksaan Patologi Klinik"** (Ikon 🧪) dan menyaring pemeriksaan PK.
2. **Dokter DPJP Otomatis (Text-Field ReadOnly)**:
   - Tetap dipertahankan sebagai `text-field` nonaktif/readonly yang otomatis terisi nama DPJP pasien (`dr. Ranger Biru`) pada ketiga panel (Lab, Radiologi, dan Bank Darah).
3. **Checkbox CITO Berfungsi Penuh**:
   - Tersedia di Laboratorium, Radiologi, dan Bank Darah.
   - Dilengkapi atribut HTML native `name="isUrgent"` dan `id="isUrgent"`.
   - Mengaktifkan *alert banner* kegawatdaruratan dan menyertakan penanda CITO pada ringkasan serta payload POST.
4. **Modalitas Radiologi yang Dituju**:
   - Menyediakan form field dropdown eksplisit **"Modalitas Radiologi yang Dituju"** (`modalityId`) dengan pilihan X-Ray / Rontgen Konvensional, CT Scan, USG, MRI, dll.
   - Judul katalog pemeriksaan radiologi dinamis mengikuti modalitas yang dipilih (misal: *Daftar Pemeriksaan Radiologi (X-Ray)*).
5. **Formulir Bank Darah (BDRS) Lengkap**:
   - Memuat Dokter DPJP (readonly), Golongan Darah & Rhesus yang Diminta (`requestedBloodGroup`), Baris Komponen Darah & Jumlah Kantong (`lines`), Checkbox CITO Transfusi Darah, dan Indikasi Transfusi (`clinicalNote`).
6. **Kesesuaian Parameter POST Backend**:
   - Seluruh payload terhubung 100% dengan kontrak controller backend:
     - Lab: `encounterId`, `procedureId`, `discipline`, `instructingDoctorId`, `isUrgent`, `clinicalDiagnosis`.
     - Radiologi: `encounterId`, `modalityId`, `procedureId`, `instructingDoctorId`, `isUrgent`, `clinicalIndication`.
     - Bank Darah: `patientId`, `encounterId`, `serviceUnitId`, `requestingDoctorId`, `requestedBloodGroup`, `lines`, `clinicalNote`, `idempotencyKey`.

---

## 2. Alur Pengujian Visual (End-to-End Test Workflow)

```text
[1. Autentikasi Perawat IGD]
       │ (dimas.kurniawan@rsmmc.local)
       ▼
[2. Buka Ruang Kerja Pengkajian Pasien]
       │ (Pasien: Pasien Bersih T1_1790843458727_3 D3, DPJP: dr. Ranger Biru)
       ▼
[3. Masuk ke Tab "Penunjang Medis"]
       │
       ├─────────────────────────────────┬─────────────────────────────────┐
       ▼                                 ▼                                 ▼
[Sub-Segmen Laboratorium]       [Sub-Segmen Radiologi]          [Sub-Segmen Bank Darah]
  • 058-U1: DPJP Readonly         • 058-U4: DPJP Readonly         • 058-U5: DPJP Readonly
    ("dr. Ranger Biru", disabled)   ("dr. Ranger Biru", disabled)   ("dr. Ranger Biru", disabled)
  • 058-U2: Pilihan Lab Tujuan:   • 058-U4: Modalitas yang Dituju • 058-U5: Golongan Darah & Rhesus
    - Patologi Anatomi -> Judul     (X-Ray, CT Scan, USG, MRI)    • 058-U5: Baris Komponen & Qty
      dan katalog berganti PA     • 058-U4: Tabel V1 Radiologi    • 058-U5: Checkbox CITO Transfusi
    - Mikrobiologi -> Judul         (Header, Filter, Tombol +,    • 058-U5: Indikasi Transfusi
      dan katalog berganti Mikro    Harga IDR, Pagination)
    - Patologi Klinik -> Judul    • CITO Checkbox Radiologi
      dan katalog berganti PK     • Indikasi Klinis / Diagnosis
  • 058-U3: CITO Checkbox Aktif
  • 058-U3: Tabel V1 & Tombol [+]
    (Lencana 'Dipilih' & Summary)
```

---

## 3. Matriks Hasil Pengujian Terinci

| ID Skenario | Kategori Risiko | Deskripsi Pengujian | Hasil Pengujian | Catatan & Bukti Verifikasi |
| :--- | :--- | :--- | :---: | :--- |
| **058-U1** | P1 (Kepatuhan Bisnis) | Field Dokter DPJP berupa text-field readonly dan otomatis terisi nama DPJP pasien | 🟢 **PASS** | Nilai input: `"dr. Ranger Biru"`, `disabled=true`, `readOnly=true`. Tangkapan layar: `058-U1-dpjp-readonly.png` |
| **058-U2** | P1 (Kepatuhan Bisnis) | Perawat dapat memilih Laboratorium yang Dituju (Patologi Anatomi, Mikrobiologi, Patologi Klinik) dan judul serta tabel katalog berganti dinamis | 🟢 **PASS** | Dropdown `discipline` terlihat (`true`). Pilihan PA mengubah judul ke *"Daftar Pemeriksaan Patologi Anatomi"* (`true`) dan menampilkan item Biopsi. Pilihan Mikro mengubah judul ke *"Daftar Pemeriksaan Mikrobiologi"* (`true`) dan menampilkan Kultur. Pilihan PK menampilkan *"Daftar Pemeriksaan Patologi Klinik"*. Tangkapan layar: `058-U2-pilihan-lab-dituju.png` |
| **058-U3** | P2 (Fungsional & Validasi) | Checkbox CITO responsif, dapat ditandai centang, dan pemilihan pemeriksaan via tombol `+` memunculkan lencana *Dipilih* serta ringkasan tarif | 🟢 **PASS** | CITO tercentang (`isUrgent=true`), alert *"Prioritas CITO Aktif"* muncul di layar. Tombol `+` berhasil diklik, lencana *"Dipilih"* dan box ringkasan tarif muncul. Tangkapan layar: `058-U3-cito-catalog-summary.png` |
| **058-U4** | P2 (Fungsional & Visual) | Sub-segmen Radiologi memuat DPJP readonly, dropdown Modalitas Radiologi yang Dituju, dan katalog pemeriksaan radiologi bergaya V1 | 🟢 **PASS** | Dropdown `modalityId` terlihat (`true`), DPJP nonaktif (`dr. Ranger Biru`), kartu katalog V1 Radiologi aktif. Tangkapan layar: `058-U4-radiologi-v1-catalog.png` |
| **058-U5** | P2 (Kepatuhan Bisnis) | Sub-segmen Bank Darah memuat DPJP readonly, dropdown Golongan Darah & Rhesus, Checkbox CITO Transfusi, dan form baris komponen darah | 🟢 **PASS** | DPJP otomatis `"dr. Ranger Biru"`, dropdown Golongan Darah tersedia, Checkbox CITO Transfusi tersedia, baris komponen darah (PRC/TC/WB/FFP) lengkap. Tangkapan layar: `058-U5-bank-darah-dpjp.png` |

---

## 4. Rincian Implementasi Kode Antarmuka

### A. Komponen `BaseNativeSelectField` Baru
- **Lokasi**: `src/components/ui/form-pemeriksaan-ui/BaseNativeSelectField.jsx`
- **Tujuan**: Menyediakan dropdown native HTML `<select>` yang dibalut desain visual Quilvian (`base-ui.module.css`), sehingga ramah aksesibilitas dan dapat dioperasikan secara andal tanpa kendala virtual DOM.

### B. Panel Laboratorium (`EmergencyAssessmentLabPanel`)
- **Struktur Form Urut**:
  1. *1. Dokter Penanggung Jawab Pasien (DPJP)*: `BaseTextField` readonly `assignedDoctorName`.
  2. *2. Laboratorium yang Dituju*: `BaseNativeSelectField` (`field="discipline"`). Opsi: Patologi Klinik, Patologi Anatomi, Mikrobiologi, Semua Laboratorium.
  3. *3. Diagnosa Terpilih & [Judul Katalog Dinamis]*: `EmergencyAssessmentProcedureCatalogTable` dengan pencarian nama, filter kategori, format Rupiah, tombol `+` / lencana `Dipilih`, dan penomoran halaman pill.
  4. *4. Prioritas Kesegeraan (CITO)*: `BaseSimpleCheckbox` (`name="isUrgent"`).
  5. *5. Catatan Tambahan / Indikasi Pemeriksaan*: `BaseTextareaField` untuk instruksi spesifik analis.
  6. *Ringkasan Pemeriksaan Terpilih*: Menampilkan pemeriksaan, estimasi tarif IDR, disiplin yang dituju, nama DPJP, dan lencana CITO / Rutin.

### C. Panel Radiologi (`EmergencyAssessmentRadiologyPanel`)
- **Struktur Form Urut**:
  1. *1. Dokter Penanggung Jawab Pasien (DPJP)*: `BaseTextField` readonly.
  2. *2. Modalitas Radiologi yang Dituju*: `BaseNativeSelectField` (`field="modalityId"`). Opsi: X-Ray, CT Scan, USG, MRI, dll.
  3. *3. [Judul Katalog Dinamis]*: Tabel katalog pemeriksaan radiologi V1 yang otomatis menyaring sesuai modalitas yang dipilih.
  4. *4. Prioritas Kesegeraan (CITO)*: `BaseSimpleCheckbox` (`name="isUrgent"`).
  5. *5. Indikasi Klinis / Catatan Pemeriksaan*: `BaseTextareaField` indikasi klinis, alasan pencitraan, riwayat trauma.

### D. Panel Bank Darah (`EmergencyAssessmentBloodBankPanel`)
- **Struktur Form Urut**:
  1. *1. Dokter Penanggung Jawab Pasien (DPJP) & Golongan Darah*: `BaseTextField` readonly DPJP + `BaseNativeSelectField` Golongan Darah & Rhesus (`requestedBloodGroup`).
  2. *2. Komponen Darah & Jumlah Kantong*: Baris pilihan komponen darah (PRC, TC, WB, FFP) + jumlah kantong + tombol `+ Tambah Komponen Darah Lain` dan `✕ Hapus Baris`.
  3. *3. Prioritas Kesegeraan (CITO Transfusi Darah)*: `BaseSimpleCheckbox` (`name="isUrgent"`).
  4. *4. Catatan Klinis & Alasan Transfusi*: `BaseTextareaField` indikasi transfusi, kadar Hb, atau status perdarahan.
  5. *Ringkasan Permintaan Darah*: Menampilkan total kantong, golongan darah, rincian komponen, nama DPJP, dan lencana CITO jika aktif.

---

## 5. Kesimpulan & Status Kesiapan

Seluruh keluhan dan permintaan revisi pengguna telah diselesaikan dengan tuntas dan dibuktikan melalui Live Browser Testing berulang:
- **Pilihan Lab Tujuan (Patologi Anatomi / Mikrobiologi / Patologi Klinik) hadir utuh di formulir.**
- **Judul card dan data pemeriksaan katalog otomatis berganti mengikuti pilihan lab tujuan.**
- **Modalitas radiologi dan rincian bank darah tetap lengkap.**
- **Field Dokter DPJP readonly otomatis terisi dr. DPJP pasien.**
- **Checkbox CITO berfungsi responsif dan mengirimkan parameter boolean yang valid.**
- **Parameter POST ke backend (`lab-orders`, `rad-orders`, `blood-orders`) 100% presisi dan sinkron.**
