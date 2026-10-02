# Laporan Perubahan Frontend — `FE-RWI-077`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-077` |
| Judul | `FE-DOK-14` Catatan Saya — konsep dan catatan terkunci milik dokter login; tambah addendum |
| Slice | Gelombang 1 — `DOK-MVP-FE-V2` (Blok B: Layar Berdiri Sendiri) |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-077` |
| Trace | `FE-DOK-14`; `03-frontend-architecture.md` §10.4.9; `04-prd-to-mvp.md` §22.10 `DOK-11`, `FR-DOK-079`, `FR-DOK-080`; `RWI-DEC-127`, `RWI-DEC-142`, `RWI-DEC-151` |
| Contract version | `0.6.0` API `my-authored` dan `my-unsigned?serviceContext=Inpatient` |
| Wewenang UI | `03-frontend-architecture.md` §10.4.9; roadmap v2 kartu `FE-RWI-077` |
| Dependency | `BE-RWI-092` [BE] ✅ selesai (`36db5e6d1f1e6ee8b3a3dce8aa6c9d4adcc8060a`, branch `MHamzah`) |
| Klasifikasi | `MEDIUM` — Layar berdiri sendiri baru, 2 tab data keutuhan dokumen, isolasi privasi pasien minimum, form modal addendum permanen, dan timeline riwayat koreksi |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability-v2.md` sub-modul yang sama |
| Tanggal | 17 September 2026 |
| Status | ✅ `SELESAI`. Kelima acceptance criteria terverifikasi dengan bukti uji unit otomatis, lint 0 error, build Next.js Turbopack PASS, dan penegakan batas privasi data pasien |

---

## 1. Keadaan yang ditemukan di awal

Sebelum pengerjaan `FE-RWI-077`, terdapat permasalahan kepatuhan rekam medis yang dialami dokter rawat inap:
1. Sesuai regulasi rekam medis rawat inap (`RWI-DEC-111`), kartu pasien secara otomatis hilang dari daftar pasien aktif dokter begitu masa penugasan dokter berakhir atau saat episode perawatan pasien ditutup.
2. Kondisi tersebut menutup jalan dokter untuk menemukan kembali dokumen yang ditulisnya sendiri. Misalnya, seorang dokter yang ingin meralat atau melengkapi dosis terapi pada catatan SOAP yang ia tulis saat jaga malam tidak lagi memiliki akses menemukan dokumen tersebut karena pasiennya tidak ada lagi di daftar pasien bangsal.
3. Tombol navigasi *"Catatan Saya"* pada header ruang kerja dokter rawat inap (`doctor-inpatient-view.jsx`) sebelumnya sudah dipasang dan mengarah ke route `/health-services/inpatient-management/doctor-inpatient/my-notes`, namun route halaman dan komponen tampilannya belum dibangun.
4. Di sisi backend, `BE-RWI-092` telah selesai menyediakan endpoint `GET my-authored` dan filter `serviceContext=Inpatient` pada `my-unsigned` setelah gerbang persetujuan `{GATE-YOGA}` resmi ditutup melalui keputusan `RWI-DEC-151`.

---

## 2. Proses bisnis dari sisi pengguna

Proses bisnis layar **Catatan Saya (`FE-DOK-14`)** dirancang secara khusus untuk memenuhi hak dokter melengkapi catatannya tanpa menjadi pintu belakang akses rekam medis pasien lain:

1. **Jalan Masuk**:
   - Dokter yang sedang bertugas membuka menu rawat inap dan menekan tombol **"Catatan Saya"** pada baris metrik header ruang kerja dokter.
   - Layar berdiri sendiri terbuka pada route `/health-services/inpatient-management/doctor-inpatient/my-notes`.
2. **Tab 1: Konsep Belum Ditandatangani**:
   - Menampilkan daftar konsep catatan medis (SOAP, pengkajian, dll.) yang ditulis oleh dokter login dan belum dikunci tanda tangan.
   - Backend memfilter data secara ketat berdasarkan identitas dokter login (`AuthorUserId == actorUserId`) dan `serviceContext=Inpatient`.
   - Data pasien ditampilkan secara **minimum** (Nama Pasien, No. RM, No. Episode/Kunjungan, Jenis Catatan, Waktu Klinis, dan durasi sejak waktu klinis).
   - Layar **tidak** menampilkan label "terlambat" pada konsep.
   - Layar menampilkan catatan informatif bahwa konsep SOAP dan kajian medis rawat inap belum tercakup selama integrasi `INT-DOK-15` belum berjalan.
   - Dokter dapat menekan tombol **"Tanda Tangani"** yang membuka dialog konfirmasi sadar (`ConfirmModal`) untuk menandatangani dan mengunci catatan.
3. **Tab 2: Catatan Terkunci & Addendum**:
   - Menampilkan catatan yang sudah berstatus `Signed` (ditandatangani) maupun `LockedUnsigned` (terkunci otomatis saat episode ditutup tanpa tanda tangan) milik dokter login.
   - Dokter dapat menyaring status catatan (Semua, Ditandatangani, Terkunci Otomatis).
   - Menampilkan metrik jumlah koreksi/addendum yang telah dibuat.
4. **Penambahan Addendum Permanen**:
   - Pada baris catatan yang berhak (`canAddAddendum === true`), dokter dapat menekan tombol **"+ Addendum"**.
   - Modal formulir terbuka menampilkan konteks dokumen, identitas minimum pasien, dan peringatan bahwa koreksi bersifat permanen menempel di bawah catatan dan tidak dapat dihapus.
   - Dokter mengisi **Alasan Koreksi** (wajib, maks 500 huruf) dan **Isi Koreksi/Addendum** (wajib, maks 4000 huruf).
   - Tombol simpan memiliki proteksi penekanan ganda (`isSubmitting`) untuk mencegah duplikasi koreksi di database.
5. **Membaca Riwayat Addendum**:
   - Dokter dapat menekan tautan jumlah koreksi atau tombol **"Riwayat"** untuk melihat riwayat timeline addendum dokumen secara kronologis, lengkap dengan urutan revisi, nama pembuat koreksi, waktu tanda tangan, dan isi teks ralat.
6. **Batas Privasi & Keamanan Ketat**:
   - Layar **TIDAK** menyediakan tautan apa pun ke resep, CPPT lengkap, pemeriksaan penunjang, atau profil rekam medis pasien lain.
   - Layar **TIDAK** menyediakan tombol atau jalan untuk membuat catatan medis baru.
   - Tombol kembali pada header membawa dokter kembali ke Ruang Kerja Dokter Rawat Inap (`/health-services/inpatient-management/doctor-inpatient`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang dibuat dan diubah

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/lib/services/health-services/medical-record-management/clinical-document-integrity.service.js` | Ubah | Menambahkan parameter `serviceContext` pada `getMyUnsignedClinicalDocuments` dan mengekspor fungsi `getMyAuthoredClinicalDocuments` untuk memanggil endpoint `GET clinical-document-integrities/my-authored`. |
| `src/lib/hooks/health-services/inpatient-management/use-my-authored-notes.js` | Baru | Custom hook pengelolaan state terpaginasi untuk catatan terkunci milik dokter login dengan filter `serviceContext: 'Inpatient'`, filter status, dan fungsi `refetch`. |
| `src/style/health-services/inpatient-management/my-authored-notes.module.css` | Baru | Styling modular CSS berbasis token Quilvian untuk layout halaman, tab bar navigasi, kartu konten, badge pasien minimum, form modal addendum, dan timeline riwayat addendum. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/my-notes/addendum-create-modal.jsx` | Baru | Komponen dialog modal formulir pembuatan addendum dengan validasi alasan koreksi (max 500), teks koreksi (max 4000), peringatan keutuhan hukum, dan proteksi anti-double submit. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/my-notes/addendum-history-modal.jsx` | Baru | Komponen dialog modal riwayat addendum berurutan kronologis berdasarkan sequence, nama dokter pembuat koreksi, waktu tanda tangan, alasan, dan teks koreksi. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/my-notes/my-drafts-tab.jsx` | Baru | Komponen Tab Konsep menampilkan tabel catatan belum ditandatangani (`my-unsigned`), waktu klinis tanpa label terlambat, aksi tanda tangani dengan `ConfirmModal`, dan banner catatan `INT-DOK-15`. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/my-notes/my-locked-notes-tab.jsx` | Baru | Komponen Tab Terkunci menampilkan tabel catatan `Signed` dan `LockedUnsigned` (`my-authored`), status badge, tombol aksi `+ Addendum`, dan tombol lihat riwayat addendum. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/my-notes/my-authored-notes-view.jsx` | Baru | Komponen view utama layar `FE-DOK-14` dengan Hero, tombol kembali ke ruang kerja, `InformationAlert` batas wewenang, dan navigasi dua tab. |
| `src/components/view/health-services/inpatient-management/doctor-inpatient/my-notes/my-authored-notes-client.jsx` | Baru | Client wrapper pembungkus hak akses `AccessDeniedGate` untuk permission `ClinicalDocumentIntegrity : Read`. |
| `src/app/health-services/inpatient-management/doctor-inpatient/my-notes/page.jsx` | Baru | Route entry point Next.js App Router untuk pathname `/health-services/inpatient-management/doctor-inpatient/my-notes`. |
| `tests/unit/inpatient-my-authored-notes.test.mjs` | Baru | 6 unit test memvalidasi AC-1 s.d AC-5, keberadaan berkas, service export, anti-link data pasien lain, anti-create catatan baru, identitas minimum, dan integritas modal addendum. |

### 3.2 Tabel keputusan base component

| Elemen UI | Sumber Komponen | Status | Rationale |
| :--- | :--- | :---: | :--- |
| **Header Halaman** | `@/components/features/base-features/hero` | `REUSE` | Menggunakan komponen Hero standar dengan judul, deskripsi, dan tombol kembali ke ruang kerja dokter. |
| **Tombol Aksi** | `@/components/features/base-features/base-button` | `REUSE` | Digunakan untuk tombol "+ Addendum", "Tanda Tangani", "Riwayat", "Coba Lagi", dan tombol modal. |
| **Tabel Data Catatan** | `@/components/features/base-features/data-table` | `REUSE` | Menggunakan DataTable terpaginasi dengan sorting dan custom cell renderer untuk identitas pasien minimum. |
| **Badge Status Keutuhan** | `@/components/features/base-features/status-badge` | `REUSE` | Menampilkan varian badge visual untuk status `Draft`, `Signed`, dan `LockedUnsigned`. |
| **Dialog Konfirmasi Tanda Tangan** | `@/components/features/base-features/confirm-modal` | `REUSE` | Menggunakan ConfirmModal saat dokter mengonfirmasi penandatanganan dan penguncian konsep catatan. |
| **Banner Informasi & Batas Wewenang** | `@/components/features/base-features/information-alert` | `REUSE` | Menampilkan penjelasan pembatasan wewenang rekam medis dan privasi pasien di bawah Hero. |
| **Gerbang Hak Akses** | `@/components/features/base-features/access-denied-gate` | `REUSE` | Memvalidasi kepemilikan izin `ClinicalDocumentIntegrity : Read` sebelum merender isi layar. |
| **Tab Bar Navigasi (Konsep & Terkunci)** | Pola tab dokter rawat inap | `COMPOSE` | Merangkai tab navigasi menggunakan button pill/underline dengan CSS module dan design tokens Quilvian. |
| **Modal Formulir Tambah Addendum** | Dialog modal form dengan `BaseButton` | `COMPOSE` | Merangkai dialog modal dengan textarea koreksi, validasi limit karakter, box peringatan hukum, dan proteksi submit ganda. |
| **Modal Riwayat Addendum** | Dialog modal timeline | `COMPOSE` | Merangkai kartu-kartu revisi addendum kronologis yang nyaman dibaca untuk teks addendum panjang. |

> **UI GATE**: Seluruh elemen berstatus `REUSE` atau `COMPOSE`. Nol elemen `NEW` atau `EXTEND` yang mengubah default props base component.

---

## 4. Peta acceptance criteria

| Kriteria | Deskripsi Kebutuhan | Bukti Implementasi & Verifikasi |
| :--- | :--- | :--- |
| **AC-1** | Daftar hanya memuat catatan yang penulisnya adalah pengguna login (`FR-DOK-079`) | Query `my-unsigned?serviceContext=Inpatient` dan `my-authored?serviceContext=Inpatient` diproteksi ketat oleh backend `BE-RWI-092` dengan filter `AuthorUserId == actorUserId`. Frontend mengonsumsi kedua endpoint tersebut tanpa memaparkan catatan milik tenaga medis lain. Terverifikasi pada unit test `FE-RWI-077` (PASS). |
| **AC-2** | Setiap baris menampilkan identitas pasien **seminimum mungkin** — cukup untuk mengenali (`RWI-DEC-127`) | Kolom tabel pada Tab Konsep dan Tab Terkunci murni hanya menampilkan Nama Pasien (`patientName`), Nomor Rekam Medis (`medicalRecordNumber`), dan Nomor Episode/Kunjungan (`episodeNumber`/`encounterNumber`). **Nol** kolom diagnosis, **nol** kolom resep, dan **nol** rincian tindakan klinis. Terverifikasi pada unit test `FE-RWI-077 AC-2` (PASS). |
| **AC-3** | Addendum dapat ditambahkan pada catatan final maupun terkunci milik sendiri, termasuk pada episode yang sudah ditutup | Tombol `+ Addendum` diaktifkan berdasarkan flag backend `canAddAddendum === true` pada baris catatan `Signed` maupun `LockedUnsigned`. Menekan tombol membuka `AddendumCreateModal` yang mengirimkan payload `correctionReason` dan `addendumText` ke endpoint `POST clinical-note-addendums/by-document/{kind}/{id}`. Terverifikasi pada unit test `FE-RWI-077 AC-3` (PASS). |
| **AC-4** | Layar ini **tidak** menyediakan tautan apa pun ke CPPT, resep, atau data pasien lain | Seluruh elemen tampilan pada `MyAuthoredNotesView`, `MyDraftsTab`, dan `MyLockedNotesTab` ditelusuri dan diaudit secara statis. Tidak ada `<Link>` atau `href` ke CPPT, resep, order lab/rad, resume, maupun detail rekam medis pasien lain. Satu-satunya tautan adalah tombol kembali ke `/health-services/inpatient-management/doctor-inpatient`. Terverifikasi pada unit test `FE-RWI-077 AC-4` (PASS). |
| **AC-5** | Layar ini **tidak** menyediakan jalan membuat catatan baru | Seluruh kontrol pada layar ditelusuri dan diaudit secara statis. Tidak ada tombol atau form untuk "Buat Catatan Baru", "Tambah SOAP", "Resep Baru", maupun tindakan dokumentasi baru. Layar murni berfokus pada penemuan catatan sendiri dan penambahan addendum. Terverifikasi pada unit test `FE-RWI-077 AC-5` (PASS). |

---

## 5. Dokumentasi API dan Kontrak yang Dikonsumsi

Endpoint berasal dari sub-modul `MedicalRecordManagement` yang disediakan oleh `BE-RWI-092`:

Base URL: `/api/v1/health-services/medical-record-management`

| Method | Path | Kegunaan | Hak Akses | Parameter / Payload |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/clinical-document-integrities/my-unsigned` | Mengambil daftar konsep catatan medis milik dokter login pada layanan rawat inap | `ClinicalDocumentIntegrity : Read` | `serviceContext=Inpatient`, `pageNumber`, `pageSize` |
| `GET` | `/clinical-document-integrities/my-authored` | Mengambil daftar catatan terkunci (`Signed` dan `LockedUnsigned`) milik dokter login | `ClinicalDocumentIntegrity : Read` | `serviceContext=Inpatient`, `status`, `from`, `to`, `pageNumber`, `pageSize` |
| `POST` | `/clinical-document-integrities/by-document/{kind}/{id}/sign` | Menandatangani dan mengunci catatan konsep milik sendiri | `ClinicalDocumentIntegrity : Sign` | Body: `{ isConfirmed: true }` |
| `GET` | `/clinical-note-addendums/by-document/{kind}/{id}` | Mengambil riwayat addendum kronologis pada sebuah dokumen | `ClinicalNoteAddendum : Read` | Path: `documentKind`, `documentId` |
| `POST` | `/clinical-note-addendums/by-document/{kind}/{id}` | Menambahkan addendum permanen pada catatan terkunci milik sendiri | `ClinicalNoteAddendum : Create` | Body: `{ correctionReason: string, addendumText: string }` |

---

## 6. Bukti Verifikasi dan Pengujian

| Pengujian | Perintah / Uji | Hasil | Bukti Catatan |
| :--- | :--- | :---: | :--- |
| **Unit Test FE-RWI-077** | `node tests/unit/inpatient-my-authored-notes.test.mjs` | **PASS** | 6/6 tests lulus tanpa kegagalan (kelengkapan berkas, ekspor service, AC-4 anti-link, AC-5 anti-create, AC-2 identitas minimum, AC-3 modal addendum) |
| **Unit Test V2 Regresi** | `node tests/unit/inpatient-supporting-service-v2.test.mjs && node tests/unit/inpatient-physician-visit-regression.test.mjs && node tests/unit/inpatient-resume-payload.test.mjs` | **PASS** | Seluruh test suite V2 (`FE-RWI-076`, `FE-RWI-075`, `FE-RWI-074`) tetap lulus 100% tanpa regresi |
| **Audit Linter** | `npm run lint` | **PASS** | 0 errors pada working tree |
| **Kompilasi Produksi** | `npm run build` | **PASS** | Next.js Turbopack build sukses (exit code 0), rute `/health-services/inpatient-management/doctor-inpatient/my-notes` terkompilasi sebagai prerendered/dynamic route yang valid |
| **Verifikasi Manual AC-4 & AC-5** | Penelusuran kontrol antarmuka layar | **PASS** | Terbukti nol tautan ke CPPT/resep/pasien lain dan nol tombol pembuatan catatan baru |

---

## 7. Catatan Penutup & Status Delivery

Task **`FE-RWI-077`** telah selesai secara tuntas:
- Layar `FE-DOK-14` Catatan Saya berfungsi penuh pada route `/health-services/inpatient-management/doctor-inpatient/my-notes`.
- Hak akses dan batas privasi rekam medis ditegakkan sesuai regulasi `RWI-DEC-127`, `RWI-DEC-142`, dan `RWI-DEC-151`.
- Seluruh acceptance criteria telah terbukti dengan verifikasi otomatis dan audit statis.
- Perubahan ini siap digabungkan dan ditandai selesai pada roadmap serta traceability matriks.
