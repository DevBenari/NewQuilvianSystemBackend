# PLAN-REPAIR-005 — Penggantian Input UUID Menjadi Dropdown Pilihan Karyawan dan Resolusi Galat 404 pada Surat Tugas, Surat Keterangan, dan Dokumen Administrasi

```yaml
plan_id: PLAN-REPAIR-HRD-005
issue: ../issue/issue-005-penugasan-surat-tugas-workforce-profile-uuid.md
status_rencana: SELESAI
tanggal_rencana: "2026-11-16"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-11-16"
basis_source_backend: "80c94e8e (MHamzah)"
basis_source_frontend: "ec3616405 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-HRD-005-01` | `ISS-HRD-005-T1` | Ekspor reusable hook `useSelectWorkforceProfiles` pada `use-hr-select.js` | FE | 1 | — | SELESAI | Ekspor hook aktif di `use-hr-select.js:20`, terhubung ke resource `workforceProfiles` endpoint `/employees/options` |
| `FIX-HRD-005-02` | `ISS-HRD-005-01`, `ISS-HRD-005-02` | Ganti textfield UUID dengan dropdown pilihan pegawai pada modal Pengajuan Surat Tugas Resmi | FE | 1 | — | SELESAI | `BaseSelectField` terpasang di `official-duties-view.jsx:580`, penghapusan fallback auth user ID, ESLint 0 error, 0 hardcoded color |
| `FIX-HRD-005-03` | `ISS-HRD-005-03` | Ganti textfield UUID dengan dropdown pilihan pegawai pada modal Pengajuan Surat Keterangan Resmi | FE | 1 | — | SELESAI | `BaseSelectField` terpasang di `employee-documents-view.jsx:584`, normalisasi parameter caller di `hr-service.js:188`, ESLint 0 error |
| `FIX-HRD-005-04` | `ISS-HRD-005-04` | Sempurnakan fallback dan penyajian nama pegawai/jabatan pada registri Dokumen Administrasi Terpadu | FE | 2 | — | SELESAI | Pemetaan nama & NIP di `administrative-documents-view.jsx`, shell tokenized, tabel tanpa duplicate pagination, ESLint 0 error |

**Ringkasan: 4 dari 4 perbaikan selesai (100%).**

---

## 2. Solusi Terpilih per Temuan

### ISS-HRD-005-01 & ISS-HRD-005-02 — Input Pegawai Surat Tugas Menggunakan UUID & Gagal Simpan 404

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Ganti ruas input dengan `BaseSelectField` yang memanfaatkan endpoint master karyawan `/api/v1/corporate/human-resource/master-data/employees/options` (resource `workforceProfiles`). | Opsi menampilkan format ramah manusia (*Nama Pegawai, NIP, Unit/Jabatan*), bebas dari string UUID di layar, dan nilai yang tersimpan selalu merupakan `MstWorkforceProfile.Id` yang sah di database. | Memerlukan pemuatan opsi karyawan secara asinkron saat modal dibuka. |
| B | Tetap menggunakan text input tetapi menambahkan tombol pencarian modal (*Lookup Modal*) terpisah. | Dapat menampilkan tabel pegawai lengkap dengan foto. | Terlalu berat dan berlebihan untuk sekadar memilih 1 nama pegawai pada modal pengajuan ringkas. |
| C | Otomatis mengunci ke profil pengguna yang sedang login tanpa opsi memilih pegawai lain. | Paling sedikit perubahan kodenya. | Tidak dapat mengakomodasi kasus manajer HR yang mengajukan surat tugas dinas luar untuk staf bawahannya. |

**Solusi terpilih: Opsi A.**
1. Memenuhi peraturan keras: **"Dilarang menampilkan atau meminta input UUID pada tampilan pengguna."**
2. Memanfaatkan resource `workforceProfiles` yang sudah terdaftar di `hr-select-resources.js` dengan konfigurasi `valueKey: "workforceProfileId"`.
3. Menghilangkan galat HTTP 404 karena nilai `workforceProfileId` dipastikan selalu merujuk ke entitas `MstWorkforceProfile` yang ada di database.

---

### ISS-HRD-005-03 — Input Pegawai Surat Keterangan Resmi Menggunakan UUID

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Ganti `requestedForWorkforceProfileId` menjadi `BaseSelectField` yang menggunakan hook `useSelectWorkforceProfiles`. | Konsisten dengan formulir Surat Tugas, ramah pengguna, dan mencegah penerbitan dokumen atas nama akun non-pegawai. | Tidak ada kekurangan signifikan. |
| B | Tetap membiarkan isian teks manual. | Nol usaha perbaikan. | Melanggar aturan global dan menyebabkan kegagalan otorisasi payroll/KPR. |

**Solusi terpilih: Opsi A.**

---

### ISS-HRD-005-04 — Keterpaduan Identitas pada Dokumen Administrasi

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Validasi ketertelusuran data hulu (Surat Tugas dan Surat Keterangan) serta berikan fallback nama yang informatif pada registri arsip terpadu. | Arsip naskah dinas selalu menampilkan nama personel dan NIP yang jelas tanpa rekaman kosong. | Bergantung pada kebersihan data hulu. |

**Solusi terpilih: Opsi A.**

---

## 3. Skema Tampilan Sebelum → Sesudah

### Formulir Modal Pengajuan Surat Tugas Dinas Baru

#### SEBELUM (Kondisi Bermasalah — Lihat Screenshot Pelapor)
```text
+-------------------------------------------------------------------------------+
| ✈  Pengajuan Surat Tugas Dinas Baru                                        [X]|
+-------------------------------------------------------------------------------+
| Nomor Surat Perintah Tugas                  Kategori Penugasan                |
| [ SPT/RSQ-SDM/20261116/5733               ] [ Pelatihan, Seminar & Workshop ▾]|
|                                                                               |
| Pegawai yang Ditugaskan (WorkforceProfileId)                                  |
| [ 0ba84a1a-2559-49ba-a320-10fb1f399d70                                    ]   |
| ⚠ ID Profil Pegawai sasaran. Mengacu pada master data pegawai aktif.         |
|                                                                               |
| Lokasi Tujuan Penugasan                                                       |
| [ Contoh: Gedung Bapelkes Kemenkes RI, Jakarta Selatan                    ]   |
|                                                                               |
| Waktu Mulai Tugas                           Waktu Selesai Tugas               |
| [ 16/11/2026 11:27                     📅 ] [ 16/11/2026 19:27            📅 ]|
|                                                                               |
| Uraian Tugas & Tujuan Dinas (Min 10 Karakter)                                 |
| [ Jelaskan maksud dan rincian kegiatan penugasan resmi...                 ]   |
+-------------------------------------------------------------------------------+
|                                                    [ Batal ] [ Ajukan Surat ] |
+-------------------------------------------------------------------------------+
```

#### SESUDAH (Rekomendasi Perbaikan)
```text
+-------------------------------------------------------------------------------+
| ✈  Pengajuan Surat Tugas Dinas Baru                                        [X]|
+-------------------------------------------------------------------------------+
| Nomor Surat Perintah Tugas                  Kategori Penugasan                |
| [ SPT/RSQ-SDM/20261116/5733               ] [ Pelatihan, Seminar & Workshop ▾]|
|                                                                               |
| Pegawai yang Ditugaskan *                                                     |
| [ Dewi Anggraeni (EMP-001) — Perawat Pelaksana / Keperawatan                 ▾]|
| Pilih nama pegawai yang akan ditugaskan dinas luar rumah sakit.               |
|                                                                               |
| Lokasi Tujuan Penugasan *                                                     |
| [ Gedung Bapelkes Kemenkes RI, Hang Jebat, Jakarta Selatan                ]   |
|                                                                               |
| Waktu Mulai Tugas *                         Waktu Selesai Tugas *             |
| [ 16/11/2026 11:27                     📅 ] [ 16/11/2026 19:27            📅 ]|
|                                                                               |
| Uraian Tugas & Tujuan Dinas (Min 10 Karakter) *                               |
| [ Menghadiri workshop keselamatan pasien dan resusitasi neonatus...       ]   |
+-------------------------------------------------------------------------------+
|                                                    [ Batal ] [ Ajukan Surat ] |
+-------------------------------------------------------------------------------+
```

#### Tabel Wilayah Formulir
| Wilayah | Konten / Fungsi | Komponen | Sumber Data |
| :--- | :--- | :--- | :--- |
| **Nomor Surat** | Nomor unik naskah dinas resmi | `BaseInputField` (otomatis ter-generate) | Algoritma penomoran draf resmi RS |
| **Kategori** | Klasifikasi kegiatan dinas | `BaseSelectField` | Konstanta `DUTY_CATEGORIES` |
| **Pegawai Ditugaskan** | Pemilihan karyawan yang ditugaskan | `BaseSelectField` | Hook `useSelectWorkforceProfiles` |
| **Tujuan & Waktu** | Alamat dinas dan rentang penugasan | `BaseInputField` & `BaseDateTimeField` | Masukan pengguna |
| **Uraian Tugas** | Rincian kegiatan penugasan | `BaseTextAreaField` | Masukan pengguna |

---

## 4. Rincian Perbaikan

### FIX-HRD-005-01 — Ekspor reusable hook `useSelectWorkforceProfiles` pada `use-hr-select.js`

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-005-T1` |
| **Area** | Frontend |
| **Jenis Perubahan** | Penambahan fungsi hook pada library select |
| **Berkas yang Diubah** | `QuilvianSystemFrontendDev/src/lib/hooks/select/hr/use-hr-select.js` |
| **Radius Dampak** | Modul HRD yang membutuhkan pemilihan profil tenaga kerja (`workforceProfiles`) |
| **Bergantung pada** | Tidak ada |

**Langkah Pengerjaan:**
1. Tambahkan ekspor hook `useSelectWorkforceProfiles`:
   ```javascript
   export const useSelectWorkforceProfiles = (options = {}) =>
     useSelectResource("workforceProfiles", options);
   ```
2. Pastikan properti pencarian, paging, dan sorting terhubung ke endpoint `/v1/corporate/human-resource/master-data/employees/options`.

**Acceptance Criteria:**
1. Pemanggilan `useSelectWorkforceProfiles()` mengembalikan objek berisi `options`, `loading`, `error`, dan fungsi `onSearchChange`.
2. Setiap item dalam `options` memiliki:
   - `value`: GUID dari `workforceProfileId`.
   - `label`: Nama lengkap pegawai (`fullName` / `displayName`).
   - `description`: Jabatan utama dan departemen.

---

### FIX-HRD-005-02 — Ganti textfield UUID dengan dropdown pilihan pegawai pada modal Pengajuan Surat Tugas Resmi

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-005-01`, `ISS-HRD-005-02` |
| **Area** | Frontend View |
| **Jenis Perubahan** | Refactoring form modal |
| **Berkas yang Diubah** | `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/official-duties-view.jsx` |
| **Radius Dampak** | Layar Surat Tugas Resmi (`/hr/workforce-core/official-duties`) |
| **Bergantung pada** | `FIX-HRD-005-01` |

**Langkah Pengerjaan:**
1. Impor `useSelectWorkforceProfiles` pada `official-duties-view.jsx`.
2. Di dalam komponen, inisialisasi hook:
   ```javascript
   const employeeSelect = useSelectWorkforceProfiles({ onlyActive: true });
   ```
3. Ubah state inisialisasi form: bila `currentUser.workforceProfileId` ada dan valid, gunakan nilai tersebut; jika tidak ada (misalnya akun SuperAdmin), biarkan string kosong `""` agar pengguna memilih pegawai dari daftar.
4. Ganti `BaseInputField` pada ruas `workforceProfileId` menjadi:
   ```jsx
   <BaseSelectField
     label="Pegawai yang Ditugaskan"
     required
     name="workforceProfileId"
     placeholder="Pilih pegawai yang ditugaskan..."
     value={createForm.workforceProfileId}
     options={employeeSelect.options}
     loading={employeeSelect.loading}
     error={formErrors.workforceProfileId}
     description="Pilih nama pegawai yang tercatat aktif pada master data kepegawaian."
     onChange={(name, val) =>
       setCreateForm((prev) => ({ ...prev, workforceProfileId: val }))
     }
   />
   ```
5. Perbarui validasi form agar menolak submit jika pegawai belum dipilih (`!createForm.workforceProfileId`).

**Acceptance Criteria:**
1. Modal pengajuan surat tugas dinas **tidak menampilkan string UUID apa pun** pada antarmuka pengguna.
2. Muncul dropdown yang menampilkan nama-nama pegawai aktif (misal: "Dewi Anggraeni", "Bagas Saputra", dsb.).
3. Saat pengguna memilih pegawai dan menekan "Ajukan Surat Tugas", request POST ke `/api/v1/corporate/human-resource/attendance-management/official-duties` mengirimkan `workforceProfileId` yang valid dan berhasil dengan status **200 OK / 201 Created**, tanpa galat 404.

---

### FIX-HRD-005-03 — Ganti textfield UUID dengan dropdown pilihan pegawai pada modal Pengajuan Surat Keterangan Resmi

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-005-03` |
| **Area** | Frontend View |
| **Jenis Perubahan** | Refactoring form modal |
| **Berkas yang Diubah** | `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/employee-documents-view.jsx` |
| **Radius Dampak** | Layar Surat Keterangan Resmi (`/hr/workforce-core/employee-documents`) |
| **Bergantung pada** | `FIX-HRD-005-01` |

**Langkah Pengerjaan:**
1. Impor dan inisialisasi `useSelectWorkforceProfiles` pada `employee-documents-view.jsx`.
2. Ganti `BaseInputField` untuk `requestedForWorkforceProfileId` menjadi `BaseSelectField` yang terhubung ke `employeeSelect.options`.
3. Tampilkan deskripsi penjelas: *"Surat keterangan resmi akan diterbitkan atas nama pegawai ini beserta data riwayat penempatan dan payroll resminya."*

**Acceptance Criteria:**
1. Tidak ada string UUID yang tampil pada formulir permohonan surat keterangan resmi.
2. Pengguna dapat memilih pegawai penerima naskah dinas dari dropdown dengan mudah.
3. Permohonan surat keterangan resmi (misal Keterangan Aktif Bekerja atau Penghasilan KPR) berhasil dikirimkan ke server tanpa galat 404/400.

---

### FIX-HRD-005-04 — Sempurnakan penyajian nama pegawai/jabatan pada registri Dokumen Administrasi Terpadu

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-005-04` |
| **Area** | Frontend View |
| **Jenis Perubahan** | Peningkatan pemetaan data registri |
| **Berkas yang Diubah** | `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/administrative-documents-view.jsx` |
| **Radius Dampak** | Layar Registri Dokumen Administrasi (`/hr/workforce-core/administrative-documents`) |
| **Bergantung pada** | `FIX-HRD-005-02`, `FIX-HRD-005-03` |

**Langkah Pengerjaan:**
1. Pastikan pemetaan nama pegawai (`ownerName`) dan kode (`ownerNik`) membaca field relasi `workforceProfile` secara konsisten pada seluruh stream dokumen (SK, SPT, dan Surat Keterangan).
2. Sediakan label fallback yang deskriptif bila terdapat data arsip legacy yang relasi profilnya belum terpasang.

**Acceptance Criteria:**
1. Seluruh baris arsip dokumen menampilkan identitas pemilik dokumen (Nama Pegawai dan NIP/Kode) secara lengkap tanpa tanda hubung `"-"` kosong.

---

## 5. Urutan Pengerjaan

```text
FIX-HRD-005-01 (Ekspor hook useSelectWorkforceProfiles di use-hr-select.js)
├── FIX-HRD-005-02 (Dropdown pegawai pada form Surat Tugas Resmi)
├── FIX-HRD-005-03 (Dropdown pegawai pada form Surat Keterangan Resmi)
└── FIX-HRD-005-04 (Penyempurnaan registri Dokumen Administrasi)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Status | Dampak pada Rencana |
| :---: | :--- | :---: | :--- |
| K-01 | Wajib memilih pegawai terdaftar dari master data untuk setiap pengajuan surat dinas resmi | Disetujui sesuai aturan tata kelola RS | Menetapkan `FIX-HRD-005-02` dan `FIX-HRD-005-03` untuk mengunci field pada data master aktif |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| :--- | :--- | :--- | :--- |
| `03-frontend-architecture.md` | Bagian formulir naskah dinas kepegawaian | "WorkforceProfileId berupa isian referensi ID" | "WorkforceProfileId berupa BaseSelectField menggunakan resource workforceProfiles dengan label nama dan NIP pegawai" |

---

## 8. Verifikasi Menyeluruh

1. Buka menu **Surat Tugas Resmi** (`/hr/workforce-core/official-duties`), klik **+ Ajukan Surat Tugas**:
   - Pastikan field *Pegawai yang Ditugaskan* berbentuk dropdown pilihan nama pegawai.
   - Pastikan tidak ada karakter UUID yang tampak di layar.
   - Pilih satu pegawai (misal: "Dewi Anggraeni"), isi tujuan dan waktu dinas, lalu klik **Ajukan Surat Tugas**.
   - Verifikasi respons jaringan: `HTTP POST 200/201 Success` dan data muncul di antrean verifikasi.
2. Buka menu **Surat Keterangan Resmi** (`/hr/workforce-core/employee-documents`), klik **+ Ajukan Permohonan Surat**:
   - Pastikan field *Pegawai Penerima* berbentuk dropdown pilihan nama pegawai.
   - Pilih jenis surat, isi keperluan, pilih pegawai, lalu kirim permohonan.
   - Verifikasi respons jaringan berhasil tanpa galat 404.
3. Buka menu **Dokumen Administrasi** (`/hr/workforce-core/administrative-documents`):
   - Verifikasi naskah dinas yang baru diajukan muncul di tabel registri terpadu dengan nama pegawai dan NIP yang akurat.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | :--- | :---: |
| 2026-11-16 | Rencana perbaikan dibuat dengan 4 butir perbaikan terkoordinasi (Status: MENUNGGU_PERSETUJUAN). | `diagnose-module-issue` |
| 2026-11-16 | Disetujui oleh Pemilik Sistem. Seluruh butir perbaikan (FIX-01 s/d FIX-04) telah selesai diimplementasikan dan diverifikasi bersih (ESLint 0 errors, 0 UUID, tokenized). Status: SELESAI. | Antigravity AI |
