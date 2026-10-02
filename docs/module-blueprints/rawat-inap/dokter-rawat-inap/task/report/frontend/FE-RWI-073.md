# Laporan Perubahan Frontend — `FE-RWI-073`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-073` |
| Judul | Tab Tindakan (`FE-DOK-11` pada `FE-DOK-09`) |
| Slice | Gelombang 2 — `DOK-V2-1` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) — kartu `FE-RWI-073` |
| Trace | `FR-DOK-100`, `FR-DOK-102`, `FR-DOK-103`, `FR-DOK-104`; `INV-DOK-17`; `RWI-DEC-133`, `RWI-DEC-139`, `RWI-DEC-143`, `RWI-DEC-152`; `VAL-DOK-50`, `VAL-DOK-50a`; `BE-RWI-097` [BE], `BE-RWI-098` [BE] |
| Contract version | `0.6.0` API tindakan rawat inap |
| Wewenang UI | Layar `FE-DOK-11` (Tab Tindakan) mandiri hasil pemecahan `FE-DOK-06` di dalam ruang kerja dokter `FE-DOK-09` |
| Dependency | `FE-RWI-067` ✅ selesai 17 September 2026; `BE-RWI-097` [BE] ✅ selesai 16 September 2026; `BE-RWI-098` [BE] ✅ selesai 17 September 2026 |
| Klasifikasi | `MEDIUM` — formulir pesanan tindakan rawat inap tanpa `ConsultationId`, riwayat tindakan, pembatalan berotorisasi dengan alasan wajib, dan daftar tunggu verifikasi instruksi dokter |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `QuilvianSystemFrontendDev/tests/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Google Antigravity |
| Tanggal | 17 September 2026 |
| Status | ✅ **Selesai 17 September 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-5) terbukti pada source code dan verifikasi build/test. |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum implementasi task ini:
1. **Penyatuan Resep dan Tindakan dalam Satu Tab Campuran (`FE-DOK-06`):**
   Layar awal dokter rawat inap menggabungkan peresepan obat dan tindakan medis ke dalam satu tab `PrescriptionProcedureTab`. Padahal alur kerja klinis tindakan memiliki siklus instruksi, verifikasi instruksi, dan pelaksana yang independen dari siklus farmasi.
2. **Ketergantungan Palsu pada `ConsultationId` Poliklinik Rawat Jalan (`FR-DOK-100`, `RWI-DEC-152`):**
   Formulir lama menggunakan endpoint poliklinik yang mewajibkan `ConsultationId`. Di rawat inap, tindakan menempel langsung pada episode rawat inap (`InpEpisodeId`). Melalui amandemen `0.6.0` (`BE-RWI-097`), endpoint `POST /inpatient-orders` dirilis agar pesanan tindakan rawat inap dapat dibuat tanpa `ConsultationId`.
3. **Ketiadaan Antarmuka Verifikasi Instruksi Dokter (`AC-2`, `AC-3`, `AC-5`, `FR-DOK-104`):**
   Pesanan tindakan yang diinput oleh perawat di bangsal berstatus `Pending` menunggu verifikasi dokter pemberi instruksi. Dokter tidak memiliki daftar tunggu (worklist) maupun tombol verifikasi sah pada antarmuka ruang kerja dokter.
4. **Celah Pembatalan Tindakan Tanpa Alasan Wajib (`AC-4`, `FR-DOK-102`):**
   Sistem sebelumnya belum menegakkan kewajiban mengisi alasan pembatalan (`CancelReason`) secara ketat pada kontrol antarmuka sebelum tombol pembatalan dapat ditekan.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Alur Proses Bisnis Runtut

1. **Pemesanan Tindakan oleh Dokter (`AC-1`, `FR-DOK-100`):**
   - Dokter membuka Tab **Tindakan** (`FE-DOK-11`) di Ruang Kerja Dokter Rawat Inap (`FE-DOK-09`).
   - Pada segmen **Form Tindakan**, dokter memilih tindakan dari katalog master (misal: *"Pemasangan Kateter Urin"* atau *"Nebulisasi"*), menentukan jumlah (kuantitas), menandai apakah tindakan darurat/primer, serta mengisi alasan klinis dan catatan instruksi.
   - Dokter menekan tombol **"Simpan Pesanan Tindakan"**. Sistem memanggil `POST /inpatient-orders` dengan membawa `InpEpisodeId` dan `Idempotency-Key`.
   - Karena dibuat langsung oleh dokter, status verifikasi instruksi otomatis menjadi `NotRequired` (tidak memerlukan verifikasi instruksi). Pesanan langsung muncul di Riwayat Tindakan.

2. **Pemeriksaan Riwayat Tindakan Perawatan (`AC-1`, `FR-DOK-100`):**
   - Pada segmen **Riwayat Tindakan**, dokter melihat seluruh daftar tindakan episode ini (`GET /episodes/{episodeId}`) secara urut waktu.
   - Tabel menampilkan waktu tindakan, nama & kode tindakan, indikasi klinis, status klinis (`Planned`, `Ordered`, `In Progress`, `Completed`, `Cancelled`), nama penginput, status verifikasi instruksi, nama dokter pemberi instruksi, nama pelaksana, dan status billing (`Terkirim`, `Belum terbit`, `Belum dikerjakan`).

3. **Verifikasi Pesanan Tindakan Perawat (`AC-2`, `AC-3`, `AC-5`, `FR-DOK-104`, `INV-DOK-17`):**
   - Jika perawat memesan tindakan atas instruksi lisan dokter di bangsal, pesanan tercatat dengan status instruksi `Pending` dan menunjuk dokter tersebut sebagai `InstructingDoctorId`.
   - Dokter membuka segmen **Menunggu Verifikasi** (terdapat badge jumlah pesanan yang menunggu konfirmasi).
   - Segmen ini memanggil `GET /instruction-verification-worklist`, yang oleh backend secara otomatis **hanya mengembalikan pesanan yang ditujukan kepada dokter login** (`AC-2`).
   - Dokter memeriksa rincian pesanan (nama pasien, perawat penginput, tindakan, kuantitas, catatan instruksi), lalu menekan tombol **"Verifikasi Sekarang"**.
   - Sistem memanggil `PATCH /{id}/verify-instruction`. Status instruksi berubah menjadi `Verified` (`Terverifikasi`), merekam identitas verifikator dan waktu verifikasi, **tanpa mengubah perawat penginput maupun rincian pesanan** (`AC-3`, `INV-DOK-17`).
   - Pada riwayat tindakan, jika ada pesanan pending yang instruksinya ditujukan ke dokter lain, dokter login **tidak melihat tombol verifikasi** pada baris pesanan tersebut (`AC-5`, `VAL-DOK-50`).

4. **Pembatalan Pesanan Tindakan (`AC-4`, `FR-DOK-102`):**
   - Jika suatu tindakan salah dicatat atau pasien menolak, penginput asli atau DPJP aktif dapat menekan tombol **"Batalkan"**.
   - Sistem menampilkan modal pembatalan (`ConfirmModal`) yang mewajibkan pengisian `Alasan Pembatalan`.
   - Tombol konfirmasi pembatalan **terkunci (disabled) selama alasan belum diisi** (minimal 3 karakter non-spasi) (`AC-4`).
   - Setelah alasan diisi dan tombol ditekan, sistem memanggil `PATCH /{id}/cancel` dengan membawa payload `{ cancelReason }`. Status tindakan berubah menjadi `Dibatalkan` dan tindakan tidak diteruskan ke penagihan kasir.

### 2.2 Contoh Konkret Skenario Rumah Sakit

1. **Skenario Dokter Spesialis Paru (dr. Hendra, Sp.P):**
   dr. Hendra melakukan visite pagi ke pasien Budi di Ruang Melati Bed 3. dr. Hendra melihat Budi mengalami sesak dan langsung membuka Tab **Tindakan** segmen **Form Tindakan**. dr. Hendra memilih *"Nebulisasi Combivent"* 1 kali, mencatat indikasi *"Bronkospasme akut"*, lalu menekan Simpan. Tindakan tersimpan sebagai pesanan sah rawat inap tanpa perlu mencari konsultasi rawat jalan.
2. **Skenario Verifikasi Instruksi Telepon:**
   Pukul 23.00 malam, Ns. Siti menghubungi dr. Hendra via telepon karena Budi demam tinggi. dr. Hendra menginstruksikan pemeriksaan tes darah cepat dan tindakan kompres es. Ns. Siti menginput pesanan tindakan di sistem perawat dan memilih dr. Hendra sebagai dokter pemberi instruksi (status pesanan: `Pending`).
   Pukul 07.30 pagi, dr. Hendra membuka ruang kerja Budi, melihat badge angka `1` pada segmen **Menunggu Verifikasi**, memeriksa rincian yang diinput Ns. Siti, lalu menekan **"Verifikasi Sekarang"**. Status menjadi `Terverifikasi`, nama Ns. Siti tetap tercatat sebagai penginput pesanan, dan tindakan siap dieksekusi. Bila dokter lain (misal dr. Rina, Sp.A) membuka riwayat tindakan Budi, dr. Rina tidak melihat tombol verifikasi pada pesanan dr. Hendra.

---

## 3. Bukti Pemenuhan Acceptance Criteria

| ID Kriteria | Deskripsi Kriteria | Status | Bukti Implementasi & Verifikasi |
| :--- | :--- | :---: | :--- |
| **AC-1** | Form Tindakan dan Riwayat Tindakan bekerja lewat kontrak `0.6.0`. | ✅ Terbukti | `createInpatientProcedureOrder` memanggil `POST /inpatient-orders` tanpa `ConsultationId`; `getPatientProceduresByEpisode` membaca data dari `GET /episodes/{episodeId}`. |
| **AC-2** | Daftar "menunggu verifikasi instruksi" hanya memuat pesanan yang pemberi instruksinya adalah dokter login. | ✅ Terbukti | `getInstructionVerificationWorklist` memanggil `GET /instruction-verification-worklist`; backend menyaring dari sesi klaim dokter login; panel menampilkan worklist dokter tersebut. |
| **AC-3** | Verifikasi tidak mengubah penginput maupun isi pesanan yang ditampilkan. | ✅ Terbukti | `verifyProcedureInstruction` memanggil `PATCH /{id}/verify-instruction` tanpa mengirim perubahan payload rincian pesanan/penginput; modal konfirmasi menegaskan prinsip ketidakberubahan data (`INV-DOK-17`). |
| **AC-4** | Pembatalan pesanan mewajibkan alasan sebelum tombol aktif. | ✅ Terbukti | Menggunakan `ConfirmModal` dengan `requireReason={true}`; fungsi `validateCancelReason` menolak string kosong / < 3 karakter; tombol konfirmasi tetap disabled hingga alasan diisi. |
| **AC-5** | Dokter yang bukan pemberi instruksi tidak melihat tombol verifikasi pada pesanan itu. | ✅ Terbukti | Fungsi penjaga `canVerifyInstruction({ order, loggedInDoctorId })` memeriksa `instructingDoctorId === loggedInDoctorId`; tombol "Verifikasi" pada riwayat tindakan disembunyikan jika bukan dokter pemberi instruksi. |

---

## 4. Evaluasi Base Component Decision Gate

| Kebutuhan UI | Komponen Dipilih | Lokasi | Status | Alasan / Rekomendasi |
| :--- | :--- | :--- | :---: | :--- |
| Sub-navigasi Tab Tindakan | `ClinicalSegmentedNav` | `@/components/ui/doctor-clinical-base` | `REUSE` | Digunakan untuk 3 segmen: Form Tindakan, Riwayat Tindakan, Menunggu Verifikasi. |
| Tabel Data Riwayat & Worklist | `ClinicalDataTable` | `@/components/ui/doctor-clinical-base` | `REUSE` | Digunakan untuk merender baris tindakan dengan header dekoratif dan empty state. |
| Banner Notifikasi & Galat | `ClinicalSafetyAlert` | `@/components/ui/doctor-clinical-base` | `REUSE` | Menampilkan pesan error aksi dan notifikasi sukses verifikasi/pembatalan. |
| Badge Status Tindakan & Verifikasi | `ClinicalStatusBadge`, `ClinicalAuditBadge` | `@/components/ui/doctor-clinical-base` | `REUSE` | Digunakan untuk status klinis dan status instruksi (`Pending`, `Verified`). |
| Asynchronous State Guard | `ClinicalStateBoundary` | `@/components/ui/doctor-clinical-base` | `REUSE` | Menangani loading, error, dan empty state. |
| Guard Hak Tulis Dokter | `ClinicalActionGuard` | `@/components/ui/doctor-clinical-base` | `REUSE` | Mengunci form pemesanan jika dokter tidak berwenang pada episode ini. |
| Modal Pembatalan & Konfirmasi | `ConfirmModal` | `@/components/features/base-features/confirm-modal` | `REUSE` | Menggunakan properti bawaan `requireReason={true}` untuk memvalidasi alasan pembatalan. |
| Kontrol Formulir Input | `BaseNativeSelectField`, `BaseTextField`, `BaseTextAreaField`, `BaseCheckboxField`, `BaseButton` | `@/components/features/base-features/` | `REUSE` | Kontrol terstandar sesuai design token Quilvian. |
| Tata Letak Sub-Panel | Komposisi Panel Mandiri | `tabs/procedure/` | `COMPOSE` | Merangkai seluruh base component di atas. |

> **Keputusan:** Seluruh elemen berstatus `REUSE` dan `COMPOSE`. Nol komponen berstatus `NEW`.

---

## 5. Berkas yang Diubah dan Ditambahkan

| Berkas | Status | Ringkasan Perubahan |
| :--- | :---: | :--- |
| `src/lib/services/health-services/clinical-management/patient-procedure.service.js` | Ubah | Penambahan fungsi API `createInpatientProcedureOrder`, `getInstructionVerificationWorklist`, `verifyProcedureInstruction`, dan `cancelPatientProcedure`. |
| `src/lib/constants/health-services/inpatient-management/inpatient-procedure-constants.jsx` | Baru | Konstanta segmen tindakan, status verifikasi instruksi, status tindakan, teks UI, dan pesan validasi bahasa Indonesia. |
| `src/utils/health-services/inpatient-management/inpatient-procedure-utils.jsx` | Baru | Utilitas logika `canVerifyInstruction`, `canCancelProcedure`, `validateCancelReason`, `validateInpatientProcedureOrder`, serta pemetaan status dan label. |
| `tests/unit/inpatient-procedure-utils.test.mjs` | Baru | 6 unit test untuk memverifikasi hak verifikasi instruksi, hak pembatalan, validasi alasan, dan validasi form pesanan tindakan. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx` | Baru | Custom hook pengelola state master options, riwayat episode, worklist instruksi, form input pesanan, aksi verifikasi, dan aksi pembatalan. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-form-panel.jsx` | Baru | Panel formulir pemesanan tindakan rawat inap dengan base component controls dan action guard. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-history-panel.jsx` | Baru | Panel riwayat tindakan episode dengan kolom status verifikasi, tombol pembatalan ber-modal alasan, dan tombol verifikasi jika berhak. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-verification-worklist-panel.jsx` | Baru | Panel daftar tunggu verifikasi pesanan perawat untuk dokter login dengan tombol eksekusi verifikasi. |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/inpatient-procedure-tab.jsx` | Baru | Komponen master Tab Tindakan (`FE-DOK-11`) menggantikan pemakaian tab campuran lama. |
| `src/style/health-services/inpatient-management/physician-procedure.module.css` | Baru | Styling CSS module untuk Tab Tindakan sesuai design tokens Quilvian. |
| `src/components/view/health-services/inpatient-management/physician-workspace/components/physician-workspace-tabs.jsx` | Ubah | Mengarahkan `TAB_CONTENT.procedure` ke `InpatientProcedureTab` (`FE-DOK-11`). |

---

## 6. Spesifikasi Kontrak API Bergaya Swagger

#### Health Services / Clinical Management / Patient Procedure

Base URL: `/api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/inpatient-orders` | Membuat pesanan tindakan rawat inap (tanpa `ConsultationId`) | `PatientProcedure : Create` | `CreateInpatientProcedureOrderRequest` + `Idempotency-Key` | `ApiResponse<PatientProcedureResponse>` (`201` baru / `200` idempoten) |
| `GET` | `/episodes/{episodeId}` | Mengambil daftar tindakan pada satu episode rawat inap | `PatientProcedure : Read` | Path: `episodeId` (UUID); Query: `pageNumber`, `pageSize` | `ApiResponse<PagedResult<PatientProcedureResponse>>` (`200 OK`) |
| `GET` | `/instruction-verification-worklist` | Mengambil daftar pesanan perawat yang menunggu verifikasi dokter login | `PatientProcedure : Read` | Query: `pageNumber`, `pageSize` | `ApiResponse<PagedResult<InstructionVerificationItemResponse>>` (`200 OK`, `403` jika bukan dokter) |
| `PATCH` | `/{id}/verify-instruction` | Dokter pemberi instruksi memverifikasi pesanan tindakan perawat | `PatientProcedure : Verify` | Path: `id` (UUID) | `ApiResponse<PatientProcedureResponse>` (`200 OK`, `403`, `409`) |
| `PATCH` | `/{id}/cancel` | Membatalkan pesanan tindakan (hanya penginput asli atau DPJP aktif) | `PatientProcedure : Update` | Path: `id` (UUID); Body: `{ cancelReason: string }` | `ApiResponse<object>` (`200 OK`, `400`, `403`) |

---

## 7. Bukti Pengujian dan Verifikasi

1. **Unit Tests (`tests/unit/inpatient-procedure-utils.test.mjs`):**
   - `canVerifyInstruction: hanya mengizinkan dokter login yang merupakan pemberi instruksi pada pesanan pending` — **PASS**
   - `canCancelProcedure: mengizinkan penginput asli atau DPJP aktif selama belum tertagih/selesai` — **PASS**
   - `validateCancelReason: mewajibkan minimal 3 karakter non-spasi` — **PASS**
   - `validateInpatientProcedureOrder: memvalidasi procedureId dan quantity` — **PASS**
   - `describeVerificationStatus & describeProcedureStatus: menghasilkan label dan tone yang benar` — **PASS**
   - `normalizeProcedureList dan normalizeWorklistItems: mengembalikan array terstruktur` — **PASS**
   - Total: 6 passed, 0 failed.

2. **Lint Validation (`npm run lint`):**
   - **PASS** — 0 errors, 695 warnings (seluruhnya berasal dari legacy codebase yang tidak diubah).

3. **Build Validation (`npm run build`):**
   - Menghasilkan build Next.js / Turbopack tanpa galat kompilasi.
