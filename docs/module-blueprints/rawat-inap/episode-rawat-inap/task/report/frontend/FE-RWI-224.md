# Laporan Perubahan Frontend — `FE-RWI-224`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-224` |
| Judul | Langkah 1 Pasien Lama Split Screen (Pencarian & Verifikasi Kanan-Kiri) dan Langkah 2 Jenis Kunjungan & Kategori |
| Slice | Slice 2 (UI Pendaftaran Admisi Pasien Baru & Dokter); Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) — kartu `FE-RWI-224` |
| Trace | `FR-RI-182`; `RWI-DEC-271`; `RWI-AC-392` |
| Contract version | `0.11.0` **`approved`** (10 Oktober 2026) |
| Dependency | `FE-RWI-223` ✅ |
| Klasifikasi | `MEDIUM` — Layout split screen responsif desktop/tablet, pemaduan alur pencarian & verifikasi dalam 1 layar, langkah 2 jenis kunjungan dan kategori |
| Task mode | `FRONTEND` |
| Target tulis | `inpatient-admission-flow-constants.jsx`, `inpatient-admission.module.css`, `inpatient-admission-existing-patient-step.jsx`, `inpatient-admission-visit-category-step.jsx`, `inpatient-admission-view.jsx` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Kriteria penerimaan `RWI-AC-392` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Berdasarkan revisi Tim Analisis Bisnis (Mba Ilma) pada admisi pasien lama:
1. **Langkah 1 Pasien Lama Split Screen (`RWI-DEC-271`, `RWI-AC-392`):**
   - Menyatukan formulir pencarian dan verifikasi data identitas pasien lama ke dalam 1 tampilan layar split (kanan-kiri) yang terpadu.
   - Kolom kanan: panel pencarian pasien berdasarkan No. RM atau NIK beserta daftar hasil pencarian.
   - Kolom kiri: kartu verifikasi identitas pasien terpilih. Ketika pasien di panel kanan diklik, data identitas langsung mengisi kartu verifikasi di panel kiri tanpa berpindah langkah atau me-reload halaman.
   - Terdapat tombol "Ganti Pasien" untuk mereset pilihan dan melakukan pencarian ulang.
2. **Langkah 2 Pasien Lama (`visit-category`):**
   - Menjadi langkah terpadu untuk menentukan Jenis Kunjungan (Umum / Rujukan dengan form 5 isian tekstual tanpa upload fisik) dan Kategori Pasien (3 opsi: Pasien Umum, Bayi Baru Lahir, Pegawai RS), sebelum berlanjut ke langkah Pembayaran.

---

## 2. Rincian Perubahan Source Code

### 2.1 `inpatient-admission-flow-constants.jsx`
- Memperbarui `INPATIENT_ADMISSION_EXISTING_PATIENT_STEPS` menjadi 10 langkah terpadu:
  1. `existing-patient` (Pasien Lama - Split Screen)
  2. `visit-category` (Jenis Kunjungan & Kategori)
  3. `payment` (Pembayaran)
  4. `deposit` (Deposit)
  5. `doctor` (Dokter)
  6. `bed-selection` (Pilih Bed)
  7. `bed-booking` (Booking Bed)
  8. `confirmation` (Konfirmasi)
  9. `consent-form` (Form Persetujuan)
  10. `consent-print` (Cetak Persetujuan)

### 2.2 `inpatient-admission.module.css`
- Menambahkan kelas CSS layout responsif:
  - `.splitLayout`: grid 2 kolom dengan breakpoint tablet/mobile responsif.
  - `.splitColumn`: flex column untuk susunan kartu dan kontrol pencarian.
  - `.emptyVerificationCard`: placeholder informatif di panel kiri sebelum pasien dipilih.

### 2.3 `inpatient-admission-existing-patient-step.jsx`
- Mengimplementasikan komponen `InpatientAdmissionExistingPatientSplitStep`:
  - Panel kiri merender `BasePatientVerificationCard` saat pasien terpilih dan kartu panduan saat belum ada pilihan.
  - Panel kanan menyediakan pencarian No. RM/NIK dan kartu daftar pasien yang reaktif.
  - Tombol "Lanjut ke Jenis Kunjungan & Kategori" aktif hanya ketika pasien telah terpilih.

### 2.4 `inpatient-admission-visit-category-step.jsx` (Komponen Baru)
- Menggabungkan pilihan Jenis Kunjungan (Umum & Rujukan dengan 5 atribut teks) dan pilihan Kategori Pasien (Pasien Umum, Bayi Baru Lahir, Pegawai RS).

### 2.5 `inpatient-admission-view.jsx`
- Menghubungkan rendering langkah `existing-patient` ke `InpatientAdmissionExistingPatientSplitStep` dan langkah `visit-category` ke `InpatientAdmissionVisitCategoryStep`.

---

## 3. Bukti Verifikasi dan Acceptance Criteria

| ID Kriteria | Kriteria Penerimaan | Status | Bukti Implementasi & Hasil Pengujian |
|---|---|:---:|---|
| `RWI-AC-392` | Form pencarian dan verifikasi data identitas pasien lama menyatu dalam 1 layar split screen kanan-kiri | ✅ LULUS | Komponen `InpatientAdmissionExistingPatientSplitStep` merender form pencarian di kolom kanan dan kartu identitas di kolom kiri. Mengklik kartu hasil pencarian langsung mengupdate panel kiri secara reaktif. Tombol Ganti Pasien membersihkan pilihan, dan tombol Lanjut membawa ke Langkah 2. |

---

## 4. Handoff ke Task Berikutnya
- Task frontend `FE-RWI-224` telah selesai penuh.
- Siap dilanjutkan ke task **`FE-RWI-225`**:
  - Langkah Mandiri (*Dedicated Step*) Formulir Persetujuan Rawat Inap & Kanvas Tanda Tangan Digital interaktif (`react-signature-canvas`).
  - Penegakan invariant Bayi Baru Lahir wajib Orang Tua / Wali sah.
