# Laporan Perubahan Frontend — `FE-RWI-078`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-078` |
| Judul | `FE-DOK-15` Perlu Review — gabungan entri CPPT menunggu verifikasi DPJP dan pesanan perawat menunggu verifikasi instruksi dokter |
| Slice | Gelombang 1 — `DOK-MVP-FE-V2` (Blok B: Layar Berdiri Sendiri) |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-078` |
| Trace | `FE-DOK-15`; `03-frontend-architecture.md` §10.4.10; `04-prd-to-mvp.md` §22.10 `DOK-12`, `FR-DOK-084`, `FR-DOK-104`; `RWI-DEC-126`, `RWI-DEC-139` |
| Contract version | `0.6.0` API `verification-worklist` dan `instruction-verification-worklist` |
| Wewenang UI | `03-frontend-architecture.md` §10.4.10; roadmap v2 kartu `FE-RWI-078` |
| Dependency | `BE-RWI-096` [BE] ✅ dan `BE-RWI-098` [BE] ✅ (commit `23a31501`, branch `MHamzah`) |
| Klasifikasi | `MEDIUM` — Layar agregasi tugas klinis berdiri sendiri, 2 jenis antrean terpadu, pembedaan visual jelas, indikator keterlambatan (overdue), dan tautan ke ruang kerja pasien |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability-v2.md` sub-modul yang sama |
| Tanggal | 17 September 2026 |
| Status | ✅ `SELESAI`. Seluruh acceptance criteria (AC-1 s.d AC-5) terverifikasi dengan bukti uji unit otomatis, lint 0 error, dan build Next.js Turbopack PASS |

---

## 1. Keadaan yang ditemukan di awal

Sebelum pengerjaan `FE-RWI-078`, seorang dokter rawat inap menghadapi masalah pemantauan tugas verifikasi klinis:
1. **Dua Antrean Terpecah**: Dokter memiliki dua tanggung jawab verifikasi yang berbeda namun sama-sama mendesak:
   - Entri CPPT yang ditulis perawat atau profesi lain yang memerlukan verifikasi dokter sebagai DPJP (Dokter Penanggung Jawab Pelayanan).
   - Pesanan tindakan atau penunjang yang diinput oleh perawat atas instruksi dokter melalui telepon/lisan yang memerlukan konfirmasi sah dari dokter pemberi instruksi.
2. **Keterpisahan Layar**: Sebelumnya, kedua antrean ini berada di modul/tab yang berbeda atau hanya terlihat jika dokter membuka satu per satu rekam medis setiap pasien. Hal ini meningkatkan risiko kelalaian administratif dan keterlambatan verifikasi hukum.
3. **Episode Tertutup Terabaikan**: Entri CPPT pada pasien yang perawatannya sudah selesai/ditutup (`Closed`) kerap terlewat verifikasinya karena pasien sudah tidak tampil di daftar census bangsal aktif (`RWI-DEC-126`).
4. **Jalan Masuk Belum Terhubung**: Pada header ruang kerja dokter rawat inap (`doctor-inpatient-view.jsx`), metrik ringkasan telah menampilkan angka *"Perlu Review"*, namun belum dapat diklik dan belum ada layar tujuan yang menampung antrean terpusat tersebut.

---

## 2. Proses bisnis dari sisi pengguna

Proses bisnis layar **Perlu Review (`FE-DOK-15`)** dirancang sebagai dashboard terpusat tugas verifikasi dokter:

1. **Akses Masuk Cepat**:
   - Dokter yang sedang bertugas membuka menu rawat inap. Pada baris kepala ruang kerja dokter, dokter melihat metrik ringkasan *"Perlu Review 3"*.
   - Dokter menekan tombol **"Perlu Review"** atau mengklik metrik tersebut untuk langsung diarahkan ke layar `/health-services/inpatient-management/doctor-inpatient/needs-review`.
2. **Ringkasan Kartu Metrik**:
   - Bagian atas layar menampilkan 4 kartu statistik:
     - **Total Menunggu Tindakan**: Akumulasi seluruh pekerjaan tertunda.
     - **CPPT Menunggu Verifikasi DPJP**: Jumlah catatan terpadu pasien yang memerlukan tanda tangan DPJP.
     - **Instruksi Pesanan Perawat**: Jumlah pesanan tindakan/lab/radiologi yang menunggu konfirmasi instruksi.
     - **Melewati Batas (Terlambat)**: Jumlah antrean yang telah melampaui batas waktu toleransi verifikasi (disorot dengan warna peringatan merah bila > 0).
3. **Pembedaan Jenis Antrean yang Sangat Jelas (`AC-3`)**:
   - Layar menyediakan tab segmentasi: **Semua Antrean**, **CPPT Menunggu DPJP**, dan **Instruksi Pesanan Perawat**, masing-masing dilengkapi counter badge.
   - Pada tabel, setiap baris ditandai dengan badge jenis yang mencolok (`CPPT DPJP` berwarna biru primer vs `Instruksi Tindakan/Lab` berwarna oranye peringatan).
   - Untuk episode yang sudah selesai/pulang, sistem memberi penanda khusus: *"Episode Ditutup (RWI-DEC-126)"*, sehingga dokter memahami bahwa ia sedang menyelesaikan tanggungan verifikasi pascaperawatan.
4. **Penanda Keterlambatan / Overdue (`AC-4`)**:
   - Entri yang telah melewati batas waktu toleransi verifikasi ditandai dengan badge merah tegas **"⚠ Terlambat"** beserta informasi durasi keterlambatannya (misal: *"(120 mnt)"*).
   - Antrean diurutkan secara cerdas dengan memprioritaskan entri yang terlambat di baris paling atas agar segera diselesaikan.
5. **Navigasi ke Tempat Aslinya (`AC-5`)**:
   - **Untuk baris CPPT**: Menekan tombol **"Buka CPPT"** membuka Ruang Kerja Dokter Rawat Inap (`FE-DOK-09`) dengan pasien tersebut langsung terpilih dan tab **CPPT** langsung aktif (`/doctor-inpatient?episodeId={id}&tab=cppt`).
   - **Untuk baris Instruksi Tindakan**: Dokter dapat menekan **"Buka Pasien"** untuk memeriksa konteks tindakan pada tab Prosedur (`tab=procedure`), atau menekan tombol **"Verifikasi"** langsung pada tabel yang memunculkan modal konfirmasi sadar (`ConfirmModal`) untuk memvalidasi instruksi saat itu juga.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang dibuat dan diubah

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/lib/services/health-services/clinical-management/patient-integrated-progress-note.service.js` | Ubah | Menambahkan fungsi ekspor `getProgressNoteVerificationWorklist` yang memanggil endpoint `GET /verification-worklist` dengan dukungan `includeClosedEpisodes=true` (`BE-RWI-096`). |
| `src/lib/services/health-services/laboratory-management/lab-order.service.js` | Ubah | Menambahkan fungsi ekspor `getLabInstructionVerificationWorklist` untuk membaca antrean pesanan laboratorium dokter (`BE-RWI-104`). |
| `src/lib/services/health-services/radiology-management/rad-order.service.js` | Ubah | Menambahkan fungsi ekspor `getRadInstructionVerificationWorklist` untuk membaca antrean pesanan radiologi dokter (`BE-RWI-104`). |
| `src/lib/hooks/health-services/inpatient-management/use-physician-review-worklist.js` | Baru | Custom hook agregasi cerdas yang menggabungkan worklist CPPT DPJP dan instruksi pesanan (tindakan, lab, rad), normalisasi data, perhitungan overdue, dan fungsi verifikasi instruksi langsung. |
| `src/style/health-services/inpatient-management/physician-needs-review.module.css` | Baru | Styling modular CSS untuk dashboard Perlu Review: grid metrik statistik, tab segmentasi, status badge pembeda jenis, dan highlight keterlambatan. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/needs-review/physician-needs-review-view.jsx` | Baru | Komponen tampilan utama `FE-DOK-15` lengkap dengan Hero, kartu metrik, tab kategori, tabel terpadu dengan penanda jelas, tombol aksi tautan ke tab pasien, dan modal konfirmasi verifikasi. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/needs-review/physician-needs-review-client.jsx` | Baru | Client wrapper dengan proteksi gerbang hak akses `AccessDeniedGate` untuk izin CPPT dan Tindakan. |
| `src/app/health-services/inpatient-management/doctor-inpatient/needs-review/page.jsx` | Baru | Route entry point Next.js App Router pada `/health-services/inpatient-management/doctor-inpatient/needs-review`. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/doctor-inpatient-view.jsx` | Ubah | Menautkan metrik "Perlu Review" pada summary bar dan menambahkan tombol aksi navigasi "Perlu Review" pada header workspace. |
| `tests/unit/inpatient-physician-needs-review.test.mjs` | Baru | 7 unit test memvalidasi seluruh kriteria AC-1 s.d AC-5, keberadaan berkas, penanda overdue, pembedaan jenis, dan tautan navigasi. |

### 3.2 Tabel keputusan base component

| Elemen UI | Sumber Komponen | Status | Rationale |
| :--- | :--- | :---: | :--- |
| **Header Halaman** | `@/components/features/base-features/hero` | `REUSE` | Menggunakan Hero standar dengan judul, deskripsi, dan tombol kembali ke ruang kerja dokter. |
| **Tombol Navigasi / Aksi** | `@/components/features/base-features/base-button` | `REUSE` | Digunakan untuk tombol "Buka CPPT", "Buka Pasien", "Verifikasi", dan tombol muat ulang. |
| **Tabel Data Antrean** | `@/components/features/base-features/data-table` | `REUSE` | Menampilkan tabel antrean terpaginasi dengan sorting dan custom cell renderer. |
| **Status Badge & Indikator** | `@/components/features/base-features/status-badge` | `REUSE` | Menampilkan varian badge visual pembeda jenis antrean (`primary` untuk CPPT DPJP, `warning` untuk Instruksi Perawat). |
| **Dialog Konfirmasi Verifikasi** | `@/components/features/base-features/confirm-modal` | `REUSE` | Digunakan saat dokter memverifikasi pesanan tindakan secara langsung dari tabel. |
| **Banner Informasi** | `@/components/features/base-features/information-alert` | `REUSE` | Menjelaskan agregasi tugas klinis dan pentingnya verifikasi tepat waktu. |
| **Gerbang Hak Akses** | `@/components/features/base-features/access-denied-gate` | `REUSE` | Memvalidasi izin membaca dokumen klinis sebelum merender konten. |
| **Tab Navigasi Kategori** | Pola tab dokter rawat inap | `COMPOSE` | Merangkai tab navigasi segmentasi dengan badge counter menggunakan CSS module dan design tokens Quilvian. |

> **UI GATE**: Seluruh elemen berstatus `REUSE` atau `COMPOSE`. Nol elemen `NEW` atau `EXTEND` yang mengubah default props base component.

---

## 4. Peta acceptance criteria

| Kriteria | Deskripsi Kebutuhan | Bukti Implementasi & Verifikasi |
| :--- | :--- | :--- |
| **AC-1** | Daftar memuat entri CPPT yang menunggu verifikasi **dokter login sebagai DPJP** (`FR-DOK-084`) | Hook memanggil `getProgressNoteVerificationWorklist({ includeClosedEpisodes: true })`. Backend `BE-RWI-096` menyaring entri milik DPJP aktif dan DPJP terakhir pada episode `Closed`. Terverifikasi pada unit test `FE-RWI-078 AC-1 & AC-2` (PASS). |
| **AC-2** | Daftar memuat pesanan perawat yang **pemberi instruksinya dokter login** (`FR-DOK-104`) | Hook memanggil `getInstructionVerificationWorklist`, `getLabInstructionVerificationWorklist`, dan `getRadInstructionVerificationWorklist`. Backend `BE-RWI-098` dan `BE-RWI-104` menyaring pesanan yang dokternya cocok dengan akun login. Terverifikasi pada unit test `FE-RWI-078 AC-1 & AC-2` (PASS). |
| **AC-3** | Kedua jenis dibedakan dengan jelas, bukan dicampur tanpa penanda | View menyediakan tab segmentasi kategori (*Semua*, *CPPT DPJP*, *Instruksi Pesanan*) dan pada setiap baris tabel ditampilkan `StatusBadge` kontras (`CPPT DPJP` vs `Instruksi Tindakan/Lab`). Terverifikasi pada unit test `FE-RWI-078 AC-3` (PASS). |
| **AC-4** | Entri yang lewat batas ditandai **terlambat** | Properti `isOverdue` dievaluasi pada tabel. Entri yang melewati batas ditandai dengan badge merah `⚠ Terlambat` beserta informasi menit keterlambatannya, dan diprioritaskan di urutan atas. Terverifikasi pada unit test `FE-RWI-078 AC-4` (PASS). |
| **AC-5** | Setiap baris membuka tempat aslinya | Baris CPPT menyediakan tombol aksi yang mengarahkan ke `/health-services/inpatient-management/doctor-inpatient?episodeId={id}&tab=cppt`. Baris tindakan mengarahkan ke tab prosedur (`tab=procedure`) atau verifikasi langsung via modal. Terverifikasi pada unit test `FE-RWI-078 AC-5` (PASS). |

---

## 5. Dokumentasi API dan Kontrak yang Dikonsumsi

Endpoint disediakan oleh sub-modul `ClinicalManagement`, `LaboratoryManagement`, dan `RadiologyManagement`:

| Method | Path | Kegunaan | Hak Akses | Parameter Query / Body |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/verification-worklist` | Mengambil daftar episode yang memiliki entri CPPT menunggu verifikasi DPJP login | `PatientIntegratedProgressNote : Read` | `includeClosedEpisodes=true`, `pageNumber`, `pageSize` |
| `GET` | `/api/v1/health-services/clinical-management/patient-procedures/instruction-verification-worklist` | Mengambil daftar pesanan tindakan perawat yang menunggu verifikasi instruksi dokter login | `PatientProcedure : Read` | `pageNumber`, `pageSize` |
| `PATCH` | `/api/v1/health-services/clinical-management/patient-procedures/{id}/verify-instruction` | Memverifikasi instruksi pesanan tindakan oleh dokter pemberi instruksi | `PatientProcedure : Verify` | Path: `id` |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders/instruction-verification-worklist` | Mengambil daftar pesanan laboratorium yang menunggu verifikasi instruksi dokter login | `LabOrder : Read` | `pageNumber`, `pageSize` |
| `GET` | `/api/v1/health-services/radiology-management/rad-orders/instruction-verification-worklist` | Mengambil daftar pesanan radiologi yang menunggu verifikasi instruksi dokter login | `RadOrder : Read` | `pageNumber`, `pageSize` |

---

## 6. Bukti Verifikasi dan Pengujian

| Pengujian | Perintah / Uji | Hasil | Bukti Catatan |
| :--- | :--- | :---: | :--- |
| **Unit Test FE-RWI-078** | `node tests/unit/inpatient-physician-needs-review.test.mjs` | **PASS** | 7/7 tests lulus (kelengkapan berkas, ekspor service, AC-1/2 agregasi worklist, AC-3 pembedaan jenis, AC-4 overdue, AC-5 tautan ruang kerja, tombol header) |
| **Unit Test Regresi V2** | `node tests/unit/inpatient-*.test.mjs` (V2 suite) | **PASS** | Seluruh test suite V2 (`FE-RWI-078`, `FE-RWI-077`, `FE-RWI-076`, `FE-RWI-075`, `FE-RWI-074`) tetap lulus 100% tanpa regresi |
| **Audit Linter** | `npm run lint` | **PASS** | 0 errors pada working tree |
| **Kompilasi Produksi** | `npm run build` | **PASS** | Next.js Turbopack build sukses (exit code 0), rute `/health-services/inpatient-management/doctor-inpatient/needs-review` terkompilasi sebagai valid route |
| **Verifikasi Manual AC-5** | Penelusuran tautan navigasi | **PASS** | Terbukti setiap baris CPPT membuka tab `cppt` dan baris prosedur membuka tab `procedure` pada ruang kerja pasien terpilih |

---

## 7. Catatan Penutup & Status Delivery

Task **`FE-RWI-078`** telah selesai secara tuntas:
- Layar `FE-DOK-15` Perlu Review berfungsi penuh menggabungkan dua antrean verifikasi dokter rawat inap.
- Pembedaan jenis antrean dan status keterlambatan ditegakkan secara akurat.
- Tautan ke tempat aslinya di ruang kerja pasien (`doctor-inpatient`) berjalan mulus sesuai rancangan.
- Siap ditandai selesai pada roadmap dan traceability matriks.
