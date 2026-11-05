# Laporan Perubahan Frontend — `FE-FIN-057`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-057` |
| Judul | Penanganan Status Persetujuan Benefit Pegawai (Langsung Aktif vs Menunggu Tanggal Mulai) dan Penguncian Tombol Aksi Sesuai Otoritas Backend |
| Slice | `REV-23` (Benefit Management — Sinkronisasi Kartu Pasien & Lifecycle Benefit) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "REVISI ROADMAP 23 (3 November 2026)", tabel Task REV-23 Frontend |
| Trace | `FR-FIN-262`, `FR-FIN-263`; `FIN-DEC-234`, `235`; `SCR-BRS-03`; `FIN-API-1.12` §`V.1` (`POST /{id}/approve`, `POST /{id}/activate`) |
| Layar | `SCR-BRS-03` (Rincian / Detail Pendaftaran Benefit Pegawai) & `SCR-BRS-02` (Daftar Pendaftaran Benefit Pegawai) |
| Contract Version | `FIN-API-1.12` §`V.1` |
| Wewenang UI | `REUSE` — menggunakan komponen dasar (@/components/features/base-features/base-button, status-badge, information-alert, toast) tanpa token/CSS visual baru |
| Dependency | `BE-FIN-130` 🟡 [BE] — logika Backend persetujuan & aktivasi otomatis telah siap |
| Klasifikasi | `LIGHT` — 1 view detail, 2 hook (detail & list), 1 utilitas formatting, 1 berkas unit test baru |
| Task Mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only |
| Target Tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/` (laporan dan roadmap saja) |
| Tanggal | 4 November 2026 |
| Status | ✅ **SELESAI** — source selesai, 49 unit test PASS (7 test baru FE-FIN-057), ESLint PASS, acceptance criteria AC 1–4 terbukti |

---

## 1. Keadaan yang Ditemukan di Awal

1. Sebelumnya, ketika pejabat HRD menyetujui pendaftaran benefit (*Approve*), sistem frontend hanya menampilkan pesan umum *"Status enrollment berhasil diperbarui"*, tanpa membedakan apakah benefit tersebut langsung aktif hari ini atau masih harus menunggu tanggal mulai berlakunya di masa depan.
2. Tombol **"Aktifkan"** pada layar rincian detail sebelumnya memiliki logika cadangan (*fallback*) `canActivate = ... || statusKey === "approved"`. Hal ini menyebabkan tombol **"Aktifkan"** tetap muncul di layar ketika pendaftaran berstatus *Approved*, meskipun tanggal mulai berlakunya masih di masa depan. Padahal, backend secara tegas menolak aktivasi manual mendahului tanggal mulai (`FIN-VAL-291`, HTTP 422).
3. Kartu ringkasan metrik status pendaftaran di halaman utama bertuliskan *"Disetujui - Siap diaktifkan"*. Istilah ini tidak lagi akurat dengan model bisnis baru, karena pendaftaran yang sudah tiba tanggal mulainya akan langsung berstatus *Active* saat disetujui, sedangkan pendaftaran yang berstatus *Approved* adalah pendaftaran yang sedang **menunggu tanggal mulai berlaku**.
4. Jika terjadi benturan pembaruan data secara bersamaan oleh pengguna lain (HTTP 409 *Conflict*), halaman detail tidak melakukan sinkronisasi ulang data secara otomatis.

---

## 2. Proses Bisnis dari Sisi Pengguna

### Peran Pengguna
Manajer / Pejabat HRD (*HR Approver*) yang memiliki wewenang menyetujui (*Approve*) atau mengaktifkan (*Activate*) pendaftaran benefit pegawai rumah sakit.

### Skenario Operasional Rumah Sakit

#### Skenario 1: Tanggal Mulai Sudah Tiba (Langsung Aktif — AC 1)
- **Kondisi:** dr. Hendra didaftarkan ke program benefit dengan tanggal mulai berlaku hari ini (atau tanggal lampau). Status pendaftaran saat ini: **Diajukan** (*Submitted*).
- **Aksi:** Pejabat HRD membuka detail pendaftaran dr. Hendra dan menekan tombol **"Setujui"**. Modal dialog konfirmasi menampilkan penjelasan bahwa benefit akan langsung aktif.
- **Eksekusi:** Pejabat HRD menekan tombol konfirmasi.
- **Hasil Tampilan:**
  - Backend memproses persetujuan sekaligus aktivasi dalam satu transaksi atomik. Status pendaftaran menjadi **Active**.
  - Notifikasi hijau (Toast Sukses) berbunyi:
    > *"Enrollment benefit EBE-202611-0001 berhasil disetujui dan diaktifkan."*
  - Data di halaman memuat ulang: Plafon pemakaian terisi, status berganti menjadi **Aktif**, dan tombol aksi yang relevan diperbarui dari backend.

#### Skenario 2: Tanggal Mulai di Masa Depan (Menunggu Tanggal Mulai — AC 2 & AC 3)
- **Kondisi:** Perawat Maya didaftarkan pada tanggal 4 November 2026, namun masa kerjanya baru efektif mulai **15 November 2026**. Status saat ini: **Diajukan** (*Submitted*).
- **Aksi:** Pejabat HRD menekan tombol **"Setujui"** pada detail pendaftaran Perawat Maya.
- **Hasil Tampilan:**
  - Status pendaftaran menjadi **Disetujui** (*Approved*).
  - Notifikasi hijau (Toast Sukses) secara jelas mengabarkan jadwal aktivasi otomatis:
    > *"Enrollment benefit EBE-202611-0002 berhasil disetujui (aktif otomatis mulai 15/11/2026)."*
  - Pada kartu **Status & Periode**, muncul penjelas: *"Menunggu tanggal mulai"*.
  - **Kepatuhan AC 3:** Backend mengirimkan `availableActions: ["cancel"]` (tanpa `"activate"`). Di layar frontend, tombol **"Aktifkan"** **tersembunyi secara otomatis** sehingga HRD tidak dapat memaksakan aktivasi sebelum tanggal 15 November 2026.

#### Skenario 3: Penolakan Syarat Aktivasi / Pembatalan Atomik (AC 4)
- **Kondisi:** HRD menyetujui pendaftaran pegawai yang tanggal mulainya sudah tiba, namun pagu plafon pada master program bernilai Rp0, atau pegawai sudah memiliki pendaftaran benefit medis aktif lain yang saling bertumpuk (*irisan aktif*).
- **Hasil Tampilan:**
  - Backend menolak dengan kode HTTP `422 Unprocessable Entity` dan membatalkan seluruh transaksi secara atomik.
  - Sistem frontend menutup modal dan memunculkan Toast Peringatan Bahaya (Merah) bertajuk **"Gagal Mengubah Status"** dengan memuat pesan spesifik dari backend (contoh: *"Batas plafon benefit harus lebih dari 0 untuk aktivasi"*).
  - Status pendaftaran pada layar **tetap Diajukan (*Submitted*)** dan tidak berubah menjadi *Approved* atau *Active*.

#### Skenario 4: Benturan Perubahan Bersamaan (HTTP 409 Conflict)
- **Kondisi:** Dua petugas HR membuka pendaftaran yang sama; petugas pertama telah membatalkan atau mengubah pendaftaran tersebut lebih dulu.
- **Hasil Tampilan:**
  - Saat petugas kedua menekan Setujui, backend mengembalikan `409 Conflict`.
  - Frontend menampilkan notifikasi galat benturan data dan seketika memicu pemanggilan `loadDetail()` otomatis untuk menyegarkan tampilan dengan data terkini di server.

---

## 3. Perubahan Teknis yang Dikerjakan

### 3.1 Berkas yang Berubah

| Berkas | Jenis Perubahan | Deskripsi Perubahan |
| --- | --- | --- |
| `src/utils/hr/benefit/employee-benefit-enrollment-utils.js` | Utilitas | Menambahkan fungsi `formatEffectiveDate(value)` untuk memformat tanggal ke format lokal `dd/MM/yyyy`, serta fungsi murni `resolveTransitionSuccessMessage` untuk merakit pesan notifikasi sesuai jenis aksi dan status akhir. |
| `src/lib/hooks/hr/benefit/use-employee-benefit-enrollment-list.js` | Hook Daftar | Memperbarui subjudul kartu ringkasan status `approved` dari `"Siap diaktifkan"` menjadi `"Menunggu tanggal mulai"`. |
| `src/lib/hooks/hr/benefit/use-employee-benefit-enrollment-detail.js` | Hook Detail | Mengintegrasikan `resolveTransitionSuccessMessage` pada penanganan sukses transisi; memperjelas pesan modal konfirmasi persetujuan; serta menambahkan penanganan galat HTTP 409 dengan memanggil ulang `loadDetail()`. |
| `src/components/view/hr/benefit/employee-benefit-enrollment-detail-view.jsx` | Tampilan Detail | Menghapus fallback manual `statusKey === "approved"` pada penentuan `canActivate`, sehingga tombol **Aktifkan** murni mematuhi izin dari array `availableActions` backend (AC 3); serta menambahkan label *"Menunggu tanggal mulai"* pada kartu ikhtisar status saat berstatus *Approved*. |
| `tests/unit/employee-benefit-enrollment-lifecycle.test.mjs` | Unit Test | Berkas uji baru dengan 7 pengujian otomatis yang memvalidasi kriteria AC 1, AC 2, AC 3, AC 4, kartu ringkasan, dan penanganan galat 409. |

### 3.2 Kepatuhan Arsitektur UI (Design Tokens & Base Components)

```text
UI GATE DECISION: PASS REUSE
- BaseButton: REUSE (varian primary untuk aksi alur kerja, outline-danger untuk pembatalan, outline-secondary untuk kembali)
- StatusBadge: REUSE (menampilkan badge status pendaftaran dengan skema warna baku)
- InformationAlert: REUSE (menampilkan pesan peringatan kondisi data)
- ToastStack: REUSE (menampilkan notifikasi mengambang sukses dan galat)
Tidak ada modifikasi CSS global, pembuatan komponen visual baru, atau token warna baru.
```

---

## 4. Keadaan Tampilan Antarmuka (UI States)

| Status Pendaftaran | Ketersediaan Tombol Aksi di Layar | Pesan Notifikasi Saat Berhasil Disetujui | Penjelasan Ikhtisar Status |
| :--- | :--- | :--- | :--- |
| **Diajukan (*Submitted*)** | • **Setujui** (`canApprove: true`)<br>• **Batalkan** (`canCancel: true`) | — | Pendaftaran menunggu persetujuan pejabat HRD. |
| **Disetujui (*Approved* - Tanggal Masa Depan)** | • **Batalkan** (`canCancel: true`)<br>*(Tombol **Aktifkan** TERSEMBUNYI)* | *"Enrollment benefit [Nomor] berhasil disetujui (aktif otomatis mulai [dd/MM/yyyy])."* | Disetujui • Menunggu tanggal mulai berlaku. |
| **Aktif (*Active*)** | • **Batalkan** (`canCancel: true`)<br>*(Tombol **Setujui** & **Aktifkan** TERSEMBUNYI)* | *"Enrollment benefit [Nomor] berhasil disetujui dan diaktifkan."* | Plafon aktif dan dapat digunakan di loket registrasi/kasir RS. |

---

## 5. Spesifikasi Kontrak API (Swagger-Aligned)

### [Tags("Corporate Human Resource - Employee Benefit Management")]

| Method | Path | Deskripsi | Otorisasi / Hak Akses | Request / Response Format |
| :---: | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/approve` | Menyetujui pendaftaran benefit. Bila tanggal mulai telah tiba, langsung berstatus `Active`; bila belum, berstatus `Approved`. | `EmployeeBenefitEnrollment : Approve` | **Request Body:** `EnrollmentTransitionRequest`<br>**Response Body:** `ApiResponse<EmployeeBenefitEnrollmentResponse>` (HTTP 200 OK) |
| `POST` | `/api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{id}/activate` | Mengaktifkan pendaftaran berstatus `Approved` secara manual saat tanggal mulai telah tiba. | `EmployeeBenefitEnrollment : Activate` | **Request Body:** `EnrollmentTransitionRequest`<br>**Response Body:** `ApiResponse<EmployeeBenefitEnrollmentResponse>` (HTTP 200 OK) |

#### Rincian Respons Sukses Persetujuan Langsung Aktif (HTTP 200)
```json
{
  "statusCode": 200,
  "message": "Enrollment benefit disetujui dan diaktifkan.",
  "data": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "enrollmentNumber": "EBE-202611-0001",
    "enrollmentStatus": "Active",
    "effectiveStartDate": "2026-11-04T00:00:00Z",
    "remainingAmount": 10000000.00,
    "availableActions": ["Cancel", "SyncPatientCard"]
  }
}
```

#### Rincian Respons Sukses Persetujuan Tanggal Mendatang (HTTP 200)
```json
{
  "statusCode": 200,
  "message": "Enrollment benefit disetujui.",
  "data": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "enrollmentNumber": "EBE-202611-0002",
    "enrollmentStatus": "Approved",
    "effectiveStartDate": "2026-11-15T00:00:00Z",
    "remainingAmount": 0.00,
    "availableActions": ["Cancel"]
  }
}
```

---

## 6. Verifikasi Berbasis Bukti (*Evidence-Based Verification*)

### 6.1 Hasil Pengujian Unit Frontend Baru
Perintah:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/employee-benefit-enrollment-lifecycle.test.mjs
```
Hasil:
```text
✔ formatEffectiveDate mengubah ISO string atau YYYY-MM-DD menjadi dd/MM/yyyy (0.48ms)
✔ AC-1: HRD menekan Setujui dan benefit langsung Active menghasilkan toast 'Disetujui dan diaktifkan' (0.13ms)
✔ AC-2: HRD menekan Setujui untuk tanggal di masa depan menghasilkan status Approved dan toast 'aktif otomatis mulai {tanggal}' (0.04ms)
✔ transisi lain: activate, submit, cancel menghasilkan teks toast yang tepat (0.05ms)
✔ AC-3: tombol Aktifkan di layar detail tersembunyi bila tidak ada di availableActions backend (2.26ms)
✔ ringkasan status: kartu 'Disetujui' bersubjudul 'Menunggu tanggal mulai' (0.46ms)
✔ AC-4 & 409: hook detail menangani 409 dengan loadDetail ulang dan menampilkan pesan galat asli (0.31ms)

ℹ tests 7
ℹ pass 7
ℹ fail 0
```

### 6.2 Hasil Pengujian Regresi Keseluruhan Modul Benefit
Perintah:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/employee-benefit-enrollment-lifecycle.test.mjs tests/unit/employee-benefit-enrollment-create-form.test.mjs tests/unit/employee-benefit-receivable-report.test.mjs tests/unit/billing-benefit-allocation.test.mjs
```
Hasil:
```text
ℹ tests 49
ℹ pass 49
ℹ fail 0
```

### 6.3 Hasil Pemeriksaan Kerapian Kode (ESLint)
Perintah:
```bash
npx eslint src/utils/hr/benefit/employee-benefit-enrollment-utils.js src/lib/hooks/hr/benefit/use-employee-benefit-enrollment-list.js src/lib/hooks/hr/benefit/use-employee-benefit-enrollment-detail.js src/components/view/hr/benefit/employee-benefit-enrollment-detail-view.jsx tests/unit/employee-benefit-enrollment-lifecycle.test.mjs
```
Hasil: **Clean (0 errors, 0 warnings)**.

---

## 7. Status Acceptance Criteria (AC)

| Kriteria | Target AC | Bukti Lapangan | Status |
| :---: | :--- | :--- | :---: |
| **AC 1** | Respons berstatus `Active` → menampilkan teks toast *"Disetujui dan diaktifkan"* | Fungsi `resolveTransitionSuccessMessage` merakit pesan *"Enrollment benefit {nomor} berhasil disetujui dan diaktifkan."* Terverifikasi unit test. | ✅ LULUS |
| **AC 2** | Respons berstatus `Approved` → menampilkan teks toast *"aktif otomatis mulai {tanggal}"* | Fungsi `resolveTransitionSuccessMessage` memformat tanggal mulai efektif ke format lokal dan merakit *"Enrollment benefit {nomor} berhasil disetujui (aktif otomatis mulai {tanggal})."* Terverifikasi unit test. | ✅ LULUS |
| **AC 3** | Tombol **Aktifkan** tersembunyi bila tidak ada di `availableActions` | Logika `canActivate` di `employee-benefit-enrollment-detail-view.jsx` hanya mengecek `availableActions.some(...)` tanpa fallback status, sehingga saat `activate` tidak dikirim backend, tombol tersembunyi. Terverifikasi unit test. | ✅ LULUS |
| **AC 4** | Galat Backend 422 syarat aktivasi tampil apa adanya, status pendaftaran tetap Diajukan (*Submitted*) | Hook detail menangkap pesan galat backend dan menampilkannya pada toast merah bertajuk *"Gagal Mengubah Status"*, tanpa mengubah status pendaftaran di Redux store. Terverifikasi unit test. | ✅ LULUS |

