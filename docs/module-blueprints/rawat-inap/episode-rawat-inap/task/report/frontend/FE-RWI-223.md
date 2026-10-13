# Laporan Perubahan Frontend — `FE-RWI-223`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-223` |
| Judul | Langkah 1 Pasien Baru Jenis Kunjungan (Umum & Rujukan Teks Ringkas) dan Langkah 2 Filter Tiga Kategori Pasien |
| Slice | Slice 2 (UI Pendaftaran Admisi Pasien Baru & Dokter); Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) — kartu `FE-RWI-223` |
| Trace | `FR-RI-180`, `FR-RI-181`; `RWI-DEC-269`, `RWI-DEC-270`; `RWI-AC-390`, `RWI-AC-391`; Validation `VAL-ADM-02` |
| Contract version | `0.11.0` **`approved`** (10 Oktober 2026) |
| Dependency | `FE-RWI-222` ✅ |
| Klasifikasi | `MEDIUM` — Komponen Jenis Kunjungan baru, form intake teks rujukan faskes luar tanpa upload fisik, filter 3 opsi kategori pasien |
| Task mode | `FRONTEND` |
| Target tulis | `inpatient-admission-flow-constants.jsx`, `use-inpatient-admission-flow.jsx`, `inpatient-admission-visit-type-step.jsx`, `inpatient-admission-view.jsx` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Seluruh kriteria penerimaan `RWI-AC-390` dan `RWI-AC-391` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Berdasarkan revisi Tim Analisis Bisnis (Mba Ilma) pada pendaftaran admisi pasien baru:
1. **Langkah 1 Jenis Kunjungan (`RWI-DEC-269`, `RWI-AC-390`):**
   - Menambahkan langkah awal berupa seleksi jenis kunjungan: "Umum" dan "Rujukan".
   - Jika "Umum" dipilih, alur langsung lanjut ke langkah 2 (Tipe Pasien).
   - Jika "Rujukan" dipilih, antarmuka memunculkan formulir isian ringkas 5 atribut (Nomor Rujukan, Tanggal/Jam Rujukan, Fasilitas Kesehatan Perujuk, Nama Dokter Perujuk, dan Diagnosa Rujukan) tanpa tombol/fitur unggah berkas fisik.
2. **Langkah 2 Filter Tiga Kategori Pasien (`RWI-DEC-270`, `RWI-AC-391`):**
   - Opsi kategori pasien pada langkah Tipe Pasien disaring secara ketat menjadi tepat 3 pilihan: "Pasien Umum", "Bayi Baru Lahir", dan "Pegawai RS". Kategori redundan (Ibu, Anak, Korporat) tidak lagi dirender.

---

## 2. Rincian Perubahan Source Code

### 2.1 `inpatient-admission-flow-constants.jsx`
- Memperbarui daftar langkah pendaftaran pasien baru `INPATIENT_ADMISSION_NEW_PATIENT_STEPS` menjadi 12 langkah dengan memasukkan `visit-type` pada langkah 1 dan menggeser langkah berikutnya secara runtut.
- Menambahkan konstanta `INPATIENT_VISIT_TYPE` (`GENERAL`, `REFERRAL`) dan `INPATIENT_VISIT_TYPE_OPTIONS`.
- Menyaring `INPATIENT_PATIENT_TYPE_OPTIONS` menjadi tepat 3 opsi: `GENERAL` ("Pasien Umum"), `NEWBORN` ("Bayi Baru Lahir"), dan `EMPLOYEE` ("Pegawai RS").

### 2.2 `use-inpatient-admission-flow.jsx`
- Menambahkan state `visitType` dan `referralData` (menyimpan 5 atribut rujukan: `referralNumber`, `referralDateTime`, `referralFacilityName`, `referralDoctorName`, `referralDiagnosis`).
- Mengekspos state dan pembaruannya (`setVisitType`, `setReferralData`) ke objek kembalian hook alur admisi.

### 2.3 `inpatient-admission-visit-type-step.jsx` (Komponen Baru)
- Menampilkan pilihan radio card antara "Kunjungan Umum" dan "Kunjungan Rujukan".
- Jika "Kunjungan Umum", tombol "Lanjut ke Tipe Pasien" langsung aktif untuk melangkah ke langkah berikutnya.
- Jika "Kunjungan Rujukan", merender form intake 5 isian tekstual tanpa input file/upload. Memvalidasi bahwa kelima kolom wajib diisi (`VAL-ADM-02`) sebelum pengguna dapat melanjutkan.

### 2.4 `inpatient-admission-view.jsx`
- Menambahkan penanganan perenderan `stepSlug === "visit-type"` yang menghubungkan ke komponen `InpatientAdmissionVisitTypeStep`.

---

## 3. Bukti Verifikasi dan Acceptance Criteria

| ID Kriteria | Kriteria Penerimaan | Status | Bukti Implementasi & Hasil Pengujian |
|---|---|:---:|---|
| `RWI-AC-390` | Langkah 1 Pasien Baru menyediakan opsi Umum & Rujukan teks ringkas tanpa upload file | ✅ LULUS | Komponen `InpatientAdmissionVisitTypeStep` merender pilihan Umum dan Rujukan. Opsi rujukan memunculkan form 5 atribut teks tanpa elemen `<input type="file">`. Validasi kelengkapan 5 atribut berfungsi sebelum `flow.goNext()`. |
| `RWI-AC-391` | Langkah 2 Pasien Baru menyaring kategori menjadi tepat 3 opsi: Pasien Umum, Bayi Baru Lahir, Pegawai RS | ✅ LULUS | `INPATIENT_PATIENT_TYPE_OPTIONS` memuat tepat 3 elemen. Layar `patient-type` hanya merender kartu Pasien Umum, Bayi Baru Lahir, dan Pegawai RS. Opsi Ibu, Anak, dan Korporat telah dihapus. |

---

## 4. Handoff ke Task Berikutnya
- Task frontend `FE-RWI-223` telah selesai penuh.
- Siap dilanjutkan ke task **`FE-RWI-224`**:
  - Langkah 1 Pasien Lama Split Screen (Form pencarian No RM/NIK di kolom kanan, kartu identitas pasien di kolom kiri).
  - Langkah 2 Pasien Lama: Jenis Kunjungan & Kategori Pasien.
