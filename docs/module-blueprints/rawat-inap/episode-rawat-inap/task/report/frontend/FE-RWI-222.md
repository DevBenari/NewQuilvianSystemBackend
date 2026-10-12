# Laporan Perubahan Frontend — `FE-RWI-222`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-222` |
| Judul | Input Masking No HP Kontak Darurat Max 13 Digit dan Penyembunyian Dropdown Unit Tujuan Rawat Inap |
| Slice | Slice 2 (UI Pendaftaran Admisi Pasien Baru & Dokter); Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) — kartu `FE-RWI-222` |
| Trace | `FR-RI-179`, `FR-RI-183`; `RWI-DEC-267`, `RWI-DEC-268`; `RWI-AC-388`, `RWI-AC-389`; Validation `VAL-ADM-01`, `VAL-ADM-02` |
| Contract version | `0.11.0` **`approved`** (10 Oktober 2026) |
| Dependency | `BE-RWI-204` ✅ |
| Klasifikasi | `MEDIUM` — Masking regex validasi kontak darurat, penyembunyian seleksi UI unit layanan dengan preservasi state |
| Task mode | `FRONTEND` |
| Target tulis | `inpatient-admission-registration-step.jsx`, `inpatient-admission-patient-utils.jsx`, `inpatient-admission-doctor-step.jsx`, `use-inpatient-admission-doctor.jsx`, `inpatient-admission-encounter-utils.jsx` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Seluruh kriteria penerimaan `RWI-AC-388` dan `RWI-AC-389` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Berdasarkan revisi Tim Analisis Bisnis (Mba Ilma) pada admisi pendaftaran rawat inap:
1. **No. HP Kontak Darurat max 13 digit (`RWI-DEC-267`, `RWI-AC-388`):** Nomor telepon darurat penanggung jawab / keluarga pasien dibatasi maksimal 13 karakter numerik dan hanya menerima angka, guna mencegah kesalahan pengiriman SMS/WA notifikasi rumah sakit.
2. **Penyembunyian Unit Tujuan di Step Dokter (`RWI-DEC-268`, `RWI-AC-389`):** Dropdown pilihan unit layanan rawat inap tidak perlu ditampilkan pada langkah pemilihan DPJP agar petugas admisi tidak bingung. Backend otomatis mengikat unit layanan rawat inap default (`ServiceUnitType.Inpatient`) dan hook frontend mengikat nilai unit default secara otomatis di latar belakang.

---

## 2. Rincian Perubahan Source Code

### 2.1 `inpatient-admission-registration-step.jsx`
- Memperbarui komponen `EmergencyTextField` untuk `emergencyContactPhoneNumber`:
  - Mengubah atribut `maxLength` dari 30 menjadi 13.
  - Menambahkan aturan validasi React Hook Form: `pattern: /^[0-9]{8,13}$/` dan `maxLength: 13`.
  - Placeholder diperjelas: `"Contoh: 081234567890 (Maks. 13 digit)"`.

### 2.2 `inpatient-admission-patient-utils.jsx`
- Pada fungsi `buildInpatientEmergencyContactPayload`:
  - Menambahkan sanitasi string `cleanPhone = toSafeString(values.emergencyContactPhoneNumber).replace(/\D/g, "").slice(0, 13)`.
  - Memastikan payload kontak darurat yang dikirim ke backend selalu berupa angka bersih dengan panjang maksimal 13 digit.

### 2.3 `inpatient-admission-doctor-step.jsx`
- Menyembunyikan elemen dropdown Unit Tujuan dari antarmuka pengguna (`style={{ display: "none" }}` dan `aria-hidden="true"`), sehingga form tetap memegang state kontrol secara reaktif namun tidak merender elemen visual di layar.

### 2.4 `use-inpatient-admission-doctor.jsx` & `inpatient-admission-encounter-utils.jsx`
- Pada `use-inpatient-admission-doctor.jsx`: otomatis menetapkan `form.serviceUnitId` ke unit rawat inap pertama yang tersedia dari hasil query opsi bila form belum memiliki nilai unit.
- Pada `validateInpatientDoctorStep`: menghapus pemblokiran validasi lokal pada `serviceUnitId` karena backend `BE-RWI-204` secara otomatis mengikat unit rawat inap yang sah.

---

## 3. Bukti Verifikasi dan Acceptance Criteria

| ID Kriteria | Kriteria Penerimaan | Status | Bukti Implementasi & Hasil Pengujian |
|---|---|:---:|---|
| `RWI-AC-388` | Input No. HP Kontak Darurat membatasi panjang maksimal 13 karakter numerik dan menolak karakter non-angka | ✅ LULUS | `EmergencyTextField` mengunci `maxLength={13}`, validasi regex `/^[0-9]{8,13}$/`, dan payload builder membersihkan karakter non-digit serta memotong string pada 13 digit. |
| `RWI-AC-389` | Unit Tujuan di Step Dokter disembunyikan dari UI, namun tetap mengirimkan unit rawat inap ke backend | ✅ LULUS | Seksi `destinationSection` disembunyikan dengan `display: none`; `useInpatientAdmissionDoctor` mengikat `serviceUnitId` secara otomatis dan backend mengamankan pengikatan `ServiceUnitType.Inpatient`. |

---

## 4. Handoff ke Task Berikutnya
- Task frontend `FE-RWI-222` telah selesai penuh.
- Siap dilanjutkan ke task **`FE-RWI-223`**:
  - Langkah 1 Pasien Baru: Jenis Kunjungan (Umum & Rujukan Teks Ringkas 5 isian).
  - Langkah 2 Pasien Baru: Filter 3 Kategori Pasien (Pasien Umum, Bayi Baru Lahir, Pegawai RS).
