# Laporan Perubahan Frontend — `FE-FIN-056`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-056` |
| Judul | Modal Pendaftaran Benefit Pegawai Baru Mengajukan Langsung Berstatus Diajukan (*Submitted*) dalam Satu Operasi Atomik |
| Slice | `REV-23` (Benefit Management — Sinkronisasi Kartu Pasien & Lifecycle Benefit) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "REVISI ROADMAP 23 (3 November 2026)", tabel Task REV-23 Frontend |
| Trace | `FR-FIN-261`; `FIN-DEC-243`; `SCR-BRS-01`; `FIN-API-1.12` §`V.1` (`POST /submit`) |
| Layar | `SCR-BRS-01` (Modal Pendaftaran Benefit Pegawai Baru) |
| Contract Version | `FIN-API-1.12` §`V.1` (`POST api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/submit`) |
| Wewenang UI | `REUSE` — menggunakan komponen dasar (@/components/features/base-features/base-form-control, BaseButton, InformationAlert) tanpa token/CSS visual baru |
| Dependency | `BE-FIN-129` 🟡 [BE] — endpoint Backend atomik `POST /submit` telah siap |
| Klasifikasi | `LIGHT` — 1 view modal, 1 hook, 1 Redux slice, 1 service Axios, 1 berkas unit test |
| Task Mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only |
| Target Tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/` (laporan dan roadmap saja) |
| Tanggal | 4 November 2026 |
| Status | ✅ **SELESAI** — source selesai, 13 unit test PASS, ESLint lolos, acceptance criteria AC 1–4 terbukti |

---

## 1. Keadaan yang Ditemukan di Awal

1. Sebelumnya, layar pendaftaran benefit (`SCR-BRS-01`) memiliki tombol berlabel **"Simpan Draft"** dengan indikator pemuatan **"Menyimpan..."**, yang memanggil endpoint `POST /employee-benefit-enrollments` untuk menciptakan pendaftaran berstatus **Draft**.
2. Sesuai keputusan bisnis `FIN-DEC-243` dan kebutuhan `FR-FIN-261`, petugas HR yang mendaftarkan karyawan atau dokter ke program benefit rumah sakit tidak memerlukan tahap draft yang menggantung. Petugas bermaksud mendaftarkan pegawai agar langsung masuk ke antrean persetujuan pimpinan HRD (*Submitted* / Diajukan).
3. Memanggil `create` lalu disusul pemanggilan `submit` secara beruntun (*chaining*) rentan terhadap kegagalan jaringan di tengah jalan, yang dapat meninggalkan data menggantung berstatus *Draft* tanpa sepengetahuan pengguna.
4. Oleh karena itu, backend pada task `BE-FIN-129` telah menyediakan endpoint tunggal yang atomik: `POST /submit`. Frontend ditugaskan mengintegrasikan antarmuka modal pendaftaran agar langsung memanggil endpoint ini dalam satu klik tunggal yang aman dari klik ganda.

---

## 2. Proses Bisnis dari Sisi Pengguna

### Peran Pengguna
Petugas Administrasi SDM / HR (*Human Resources Officer*) dengan hak akses pendaftaran benefit pegawai.

### Skenario Operasional Rumah Sakit
1. **Pemicu (Trigger):**
   Seorang dokter spesialis atau staf medis baru (misalnya dr. Hendra, Sp.A) telah menyelesaikan orientasi kerja. Petugas HR membuka menu **Manajemen Benefit Pegawai** (`/corporate/human-resource/benefit-management/employee-benefit-enrollments`) dan menekan tombol **"Tambah Pendaftaran"**.
2. **Pengisian Formulir di Layar Modal (`SCR-BRS-01`):**
   - Petugas HR mengetikkan nama, NIP, atau NIK dokter (minimal 3 karakter) pada kolom pencarian pegawai.
   - Sistem menampilkan opsi profil tenaga kerja aktif (*Workforce Profile*). Petugas memilih nama yang bersangkutan.
   - Petugas memilih program benefit yang aktif (contoh: *Plafon Rawat Inap & Jalan Kelas 1*).
   - Petugas menetapkan tanggal pendaftaran dan tanggal mulai berlaku (*Effective Start Date*), serta mengisi catatan bila diperlukan.
3. **Pengajuan Langsung (*One-Click Submission*):**
   - Petugas menekan tombol berlabel **"Ajukan Benefit"**.
   - Sistem seketika mengunci tombol dan menampilkan teks pemuatan **"Mengajukan..."** (`loadingLabel="Mengajukan..."`), mencegah petugas menekan tombol dua kali (*double submit protection* via `submitInFlightRef.current`).
4. **Hasil Akhir yang Sukses:**
   - Permintaan dikirim langsung ke endpoint `POST /submit`.
   - Backend memproses validasi kelayakan dan langsung menerbitkan nomor pendaftaran dengan status **Diajukan** (*Submitted*).
   - Modal tertutup otomatis, dan muncul notifikasi hijau (Toast Sukses):
     > *"Pengajuan benefit [Nomor Pendaftaran] berhasil diajukan dan menunggu persetujuan HRD."*
   - Daftar tabel dan kartu ringkasan status di latar belakang otomatis dimuat ulang (`loadData()`), menampilkan pendaftaran baru tersebut pada kolom status **Diajukan**.
5. **Penanganan Kasus Gagal / Pengecualian:**
   - **Karyawan sudah terdaftar aktif:** Backend mengembalikan kode HTTP `422 Unprocessable Entity` atau `409 Conflict`.
   - Sistem frontend menampilkan pesan galat asli dari backend apa adanya di banner peringatan modal (`InformationAlert`) dan Toast Gagal (**"Gagal Mengajukan"**), tanpa menggantinya dengan kalimat umum, sehingga petugas memahami persis kendala data yang terjadi.

---

## 3. Perubahan Teknis yang Dikerjakan

### 3.1 Berkas yang Berubah

| Berkas | Jenis Perubahan | Deskripsi Perubahan |
| --- | --- | --- |
| `src/lib/services/hr/employee-benefit-enrollment-service.js` | Service | Menambahkan fungsi `submitNewEnrollment(payload)` yang mengeksekusi `POST ${BASE_URL}/submit` menggunakan `InstanceAxios`. |
| `src/lib/state/slice/hr/benefit/employee-benefit-enrollment-slice.js` | Redux Slice | Menambahkan `createAsyncThunk` `submitNewEnrollment` dan mendaftarkannya ke dalam array `actionThunks` builder Redux agar `actionLoading` dan `actionError` terkelola konsisten. |
| `src/lib/hooks/hr/benefit/use-employee-benefit-enrollment-list.js` | Custom Hook | Mengimpor `submitNewEnrollment`, mengganti dispatch `createEnrollment` menjadi `submitNewEnrollment` di dalam `handleSubmitCreate`, memperbarui teks toast sukses ("Pengajuan benefit ... berhasil diajukan dan menunggu persetujuan HRD"), serta menetapkan judul toast gagal "Gagal Mengajukan". |
| `src/components/view/hr/benefit/employee-benefit-enrollment-create-modal.jsx` | UI Component | Mengubah tombol aksi footer menjadi berlabel **"Ajukan Benefit"** dengan `loadingLabel="Mengajukan..."`, serta memperbarui dokumentasi JSDoc komponen. |
| `tests/unit/employee-benefit-enrollment-create-form.test.mjs` | Unit Test | Memperbarui verifikasi label tombol modal, menambahkan assertion `loadingLabel="Mengajukan..."`, pengujian dispatch thunk `submitNewEnrollment`, dan pengecekan keberadaan endpoint di service/slice. |

### 3.2 Kepatuhan Arsitektur dan Komponen UI

```text
UI GATE DECISION: PASS REUSE
- BaseButton: REUSE (varian primary untuk "Ajukan Benefit", secondary untuk "Batal")
- BaseSelectField: REUSE (pemilih pegawai dan program benefit)
- BaseDateField: REUSE (tanggal pendaftaran dan rentang berlaku)
- BaseTextAreaField: REUSE (catatan pendaftaran)
- InformationAlert: REUSE (banner pesan galat validasi backend)
Tidak ada penambahan kelas CSS, inline style hardcoded, atau token warna baru.
```

---

## 4. Keadaan Tampilan Antarmuka (UI States)

| State | Tampilan Antarmuka | Indikator / Pesan |
| --- | --- | --- |
| **Form Siap Diisi** | Seluruh field aktif, tombol "Ajukan Benefit" berwarna biru primer | Tombol siap diklik, input tervalidasi di sisi klien |
| **Sedang Mengajukan** | Tombol primer nonaktif (*disabled*), ikon spinner berputar | Label tombol berubah menjadi **"Mengajukan..."**; klik kedua diabaikan oleh `submitInFlightRef.current` |
| **Pengajuan Berhasil** | Modal tertutup, formulir direset ke keadaan kosong | Toast hijau: *"Pengajuan benefit [Nomor] berhasil diajukan dan menunggu persetujuan HRD."* Tabel memuat data baru berstatus Diajukan. |
| **Validasi Gagal (Klien)** | Field yang bermasalah bergaris merah dengan pesan bantuan di bawahnya | Peringatan di bawah input (misal: "Penerima benefit wajib dipilih") |
| **Galat Backend (422/409)** | Modal tetap terbuka, field terkait disorot, banner merah di atas formulir | `InformationAlert` varian danger menampilkan pesan persis dari backend; Toast merah: *"Gagal Mengajukan"* |

---

## 5. Spesifikasi Kontrak API (Swagger-Aligned)

### [Tags("Corporate Human Resource - Employee Benefit Management")]

| Method | Path | Deskripsi | Otorisasi / Hak Akses | Request / Response Format |
| :---: | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/submit` | Mendaftarkan pegawai baru ke program benefit dan langsung menetapkan status ke `Submitted` (Diajukan) secara atomik | `EmployeeBenefitEnrollment : Create` | **Request Body:** `CreateEmployeeBenefitEnrollmentRequest`<br>**Response Body:** `EmployeeBenefitEnrollmentResponse` (HTTP 201 Created) |

#### Rincian Payload Permintaan (`CreateEmployeeBenefitEnrollmentRequest`)
```json
{
  "workforceProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "benefitPlanId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "enrollmentDate": "2026-11-04",
  "effectiveStartDate": "2026-11-05",
  "effectiveEndDate": null,
  "notes": "Pendaftaran dokter spesialis anak baru."
}
```

#### Rincian Respons Sukses (`EmployeeBenefitEnrollmentResponse` - HTTP 201)
```json
{
  "statusCode": 201,
  "message": "Pendaftaran benefit berhasil diajukan.",
  "data": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "enrollmentNumber": "EBE-202611-0001",
    "enrollmentStatus": "Submitted",
    "workforceProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "benefitPlanId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "enrollmentDate": "2026-11-04T00:00:00Z",
    "effectiveStartDate": "2026-11-05T00:00:00Z",
    "effectiveEndDate": null,
    "notes": "Pendaftaran dokter spesialis anak baru."
  }
}
```

---

## 6. Verifikasi Berbasis Bukti (*Evidence-Based Verification*)

### 6.1 Hasil Pengujian Unit Frontend
Perintah:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/employee-benefit-enrollment-create-form.test.mjs
```
Hasil:
```text
✔ payload sesuai CreateEmployeeBenefitEnrollmentRequest: tanggal tanpa zona, akhir kosong menjadi null (0.58ms)
✔ form lengkap lolos validasi (0.53ms)
✔ validasi: penerima dan program wajib berupa Guid dari backend, bukan NIP/NIK/nama (0.06ms)
✔ validasi tanggal: wajib, nyata, dan akhir tidak boleh lebih awal dari mulai (0.05ms)
✔ validasi catatan: maksimal 500 karakter (0.04ms)
✔ tanggal bawaan memakai tanggal lokal, bukan tanggal UTC (0.04ms)
✔ error backend dipetakan ke field yang tepat (0.13ms)
✔ pesan error tidak membocorkan stack trace atau detail server (0.10ms)
✔ opsi penerima memakai workforceProfileId backend, NIK tersamar, dan label jenis (karyawan/dokter) (0.16ms)
✔ source: modal tidak lagi memakai ConfirmModal 430px dan grid tanggal tidak bisa melebar (2.29ms)
✔ source: pemilih pegawai server-side bermenu portal, program dari master, tanpa data hardcode (0.17ms)
✔ source: hook mencegah double submit dan memanggil submitNewEnrollment langsung Diajukan (0.10ms)
✔ source: service dan slice menyediakan submitNewEnrollment atomik (FE-FIN-056, FIN-DEC-243) (0.16ms)

ℹ tests 13
ℹ pass 13
ℹ fail 0
```

### 6.2 Uji Regresi Unit Terkait
Perintah:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/employee-benefit-enrollment-create-form.test.mjs tests/unit/employee-benefit-receivable-report.test.mjs tests/unit/billing-benefit-allocation.test.mjs
```
Hasil:
```text
ℹ tests 42
ℹ pass 42
ℹ fail 0
```

---

## 7. Status Acceptance Criteria (AC)

| Kriteria | Target AC | Bukti Lapangan | Status |
| :---: | :--- | :--- | :---: |
| **AC 1** | Satu permintaan per klik (`submitInFlightRef.current` aktif mencegah klik ganda) | Terpasang `submitInFlightRef.current = true` sebelum dispatch dan reset di `finally`. Terverifikasi unit test. | ✅ LULUS |
| **AC 2** | Tombol bertuliskan **"Ajukan Benefit"** dan loading **"Mengajukan..."** (bukan "Simpan Draft") | `employee-benefit-enrollment-create-modal.jsx` baris 269–276 menggunakan teks `Ajukan Benefit` dan `loadingLabel="Mengajukan..."`. Terverifikasi unit test. | ✅ LULUS |
| **AC 3** | Daftar dimuat ulang (`loadData()`) dan menampilkan status **Diajukan** (*Submitted*); toast memuat pesan sukses resmi | Hook `use-employee-benefit-enrollment-list.js` memanggil `loadData()`, `closeCreate()`, dan toast: *"Pengajuan benefit {enrollmentNumber} berhasil diajukan dan menunggu persetujuan HRD."* | ✅ LULUS |
| **AC 4** | Galat Backend (422, 409) tampil apa adanya tanpa diganti teks umum; judul toast "Gagal Mengajukan" | Error diekstrak melalui `resolveEnrollmentErrorMessage(payload)` dan dipetakan ke form field via `mapEnrollmentErrorToFields`. Toast menggunakan title: `"Gagal Mengajukan"`. | ✅ LULUS |

