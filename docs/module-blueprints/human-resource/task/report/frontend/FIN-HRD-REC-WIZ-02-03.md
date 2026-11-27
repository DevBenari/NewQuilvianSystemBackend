# Laporan Perubahan Frontend — `FIN-HRD-REC-WIZ-02` & `FIN-HRD-REC-WIZ-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Instruksi | `FIN-HRD-REC-WIZ-02` (Entry & Step 1–3) dan `FIN-HRD-REC-WIZ-03` (Step 4–5, Integrasi & Recovery) |
| Judul | Wizard Full-Page Pendaftaran Pelamar 5 Langkah di Submenu Pelamar |
| Submenu target | Sumber Daya Manusia → Siklus Kepegawaian → Rekrutmen & Seleksi → **Pelamar** (`/hr/recruitment/candidates`, menambah `/hr/recruitment/candidates/create`) |
| Blueprint & trace | `S-D2` Rekrutmen & Seleksi; `REG-WIZ-01` s.d. `REG-WIZ-10`; rujukan existing `FE-HRD-048`, `FE-HRD-049`, `FE-HRD-057`, `MSL-15` tahap 2 |
| Baseline HEAD | `f122c1d641f2cf47e3597762e6956fabdae37eaa` (branch `HamzahV2`, belum di-commit) |
| Dependency BE | `FIN-HRD-REC-WIZ-01` ✅ ([laporan BE](file:///C:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/task/report/backend/FIN-HRD-REC-WIZ-01.md)) |
| Klasifikasi | Frontend Multi-Step Feature + Service Reuse + Unit Test + CSS Module Tokens |
| Target tulis | `QuilvianSystemFrontendDev/{src,tests}/**`, `test-with-agy/**`, laporan ini |
| Tanggal | 25 November 2026 |
| Status | ✅ **IMPLEMENTED** — seluruh gerbang statis (`check-css-usage.mjs`, `eslint --max-warnings=0`, `npm run lint:errors`, unit test wizard `6/6`, Tailwind scan `0`, `npm run build` `935/935`) dan `12/12` skenario Chromium UAT lulus (`6 LIVE_BE` + `6 ROUTE_INJECTED` untuk lowongan `Published` & fault injection karena DB dev `0` lowongan) |

---

## 1. Keadaan Awal & Scope

Sebelumnya, aksi utama **Daftarkan Pelamar** di `/hr/recruitment/candidates` membuka dialog modal tunggal tanpa pemisahan langkah, tanpa pemeriksaan NIK *exact*, tanpa pengecekan `rowVersion` atau recovery koneksi, dan tanpa keterhubungan dengan kebutuhan dokumen klinis (`STR`) dari lowongan `Published`.

Sesuai lingkup terarah yang disetujui:
- Sidebar navigasi **tidak diubah** (tetap 8 submenu Rekrutmen & Seleksi).
- Halaman daftar `/hr/recruitment/candidates` dan detail `/hr/recruitment/candidates/[id]` dipertahankan penuh; tombol utama membuka halaman wizard baru `/hr/recruitment/candidates/create` (`CANDIDATE` permission-gated), dilengkapi tombol **Wizard** per baris serta **Lanjutkan di Wizard** / **Daftarkan ke Lowongan** dari halaman detail (memakai `?candidateId=`).
- Parameter URL tidak pernah membawa PII (`NIK`, `email`, atau nomor telepon). Zero `localStorage`/`sessionStorage` untuk draft PII.
- Perbaikan tambahan pada detail existing (`candidate-detail-screening-view.jsx`): kalkulasi STR kedaluwarsa yang semula memakai perbandingan jam berjalan (`new Date(row.validUntil) < new Date()`) diperbaiki memakai pembanding tanggal kalender `isDateOnlyExpired`.

---

## 2. Alur Bisnis Pengguna 5 Langkah

1. **Langkah 1 — Lowongan & Jalur**: Petugas memilih *Melamar ke Lowongan Aktif* (`Published` yang belum lewat batas tutup) atau *Daftar Kandidat Tanpa Lowongan*. Bila memilih lowongan, kartu ringkasan menampilkan nomor lowongan, jabatan, unit, RS, kuota, batas tutup, dan penanda posisi klinis/non-klinis read-only. Lowongan yang sudah dilamar kandidat terseleksi ditandai nonaktif agar tidak terajukan ganda.
2. **Langkah 2 — Identitas Pelamar & Cek NIK**: Petugas menginput NIK tepat 16 digit (hanya angka). Lookup `GET /candidates?searchNik=` disaring **exact match** dengan `AbortController` + sequence token (anti-stale response). Bila ditemukan, petugas meninjau kartu ringkasan serta lencana *Rehire (Mantan Pegawai)* bila ada dan menekan **Gunakan Kandidat Existing** untuk memuat detail lengkap tanpa `POST` duplikat; bila baru, petugas mengisi nama lengkap KTP, jenis kelamin, tempat lahir, dan tanggal lahir (<= hari ini).
3. **Langkah 3 — Kontak & Checkpoint Persisten**: Petugas mengisi telepon, email, dan alamat domisili. Menekan **Simpan Profil & Lanjut** mengirim `POST /candidates` tepat sekali (status `Registered`), menaruh `?candidateId=<uuid>` di URL untuk resume, dan membuka Step 4. Pada kandidat existing, `PUT /candidates/{id}` hanya dikirim bila ada diff biodata/kontak dan petugas berwenang (`Candidate:Update`), serta menangani `409 RowVersion` dengan tombol muat snapshot terbaru server.
4. **Langkah 4 — Dokumen Pendukung**: Menyajikan dua bagian jelas: tabel **Arsip Dokumen Pelamar di Server** (urut terbaru, badge *Belum Diverifikasi*/*Terverifikasi*, tombol *Unduh* ber-izin) dan daftar baris unggah baru (*KTP*, *CV*, *Ijazah*, plus slot wajib peringatan *STR* bila posisi klinis, dan **+ Tambah Dokumen Lainnya**). Berkas fisik diunggah lewat `HrAttachmentField` -> `key`, lalu disimpan via `POST /candidates/{id}/documents` dengan `applicationId: null`. Jika metadata gagal disimpan sementara upload fisik sudah berhasil, baris menyimpan `key` tersebut dan menyediakan tombol **Coba Lagi Simpan Metadata** tanpa mengunggah fisik ulang.
5. **Langkah 5 — Review & Konfirmasi Akhir**: Menampilkan ringkasan jalur/lowongan, identitas dengan **NIK tersamar** (tombol *Tampilkan NIK* opsional), kontak, berkas, status kepatuhan STR klinis (*Belum Ada* / *Kedaluwarsa* / *Multi-STR campuran* — bersifat peringatan selaras `VR-REC-07/08` tanpa auto-reject semu atau auto-pass), tautan *Ubah* ke langkah terkait tanpa menghapus dokumen, kolom `notes` (maks 500 karakter), dan kotak centang pernyataan konfirmasi. Mode lowongan memanggil `POST /candidates/{id}/apply` disertai timeout recovery; mode tanpa lowongan selesai dengan status `Registered` (`0` panggilan `/apply`).

---

## 3. Perubahan Kode & Gerbang Komponen

### 3.1 Berkas Frontend yang Dibuat & Diubah (`QuilvianSystemFrontendDev`)

| Berkas | Status | Peran |
| --- | --- | --- |
| `src/app/hr/recruitment/candidates/create/page.jsx` | Baru | Route Next.js App Router `/hr/recruitment/candidates/create` terbungkus `<Suspense>` |
| `src/components/view/hr/recruitment/candidate-registration-wizard-view.jsx` | Baru | Shell wizard full-page (`Hero`, RBAC `InformationAlert`, `ConfirmModal` exit guard, `ToastStack`, grid 2 kolom) |
| `src/components/view/hr/recruitment/wizard/candidate-wizard-stepper.jsx` | Baru | `CandidateWizardStepper` (ARIA `aria-current="step"`, tombol langkah tersegmentasi) & `CandidateWizardSummarySidebar` |
| `src/components/view/hr/recruitment/wizard/candidate-wizard-step-panels.jsx` | Baru | Panel UI Step 1 s.d. Step 5 (`Step1VacancyPanel` .. `Step5ReviewSubmitPanel`) & `WizardCompletedPanel` |
| `src/lib/hooks/hr/recruitment/use-candidate-registration-wizard.js` | Baru | State mesin wizard 5 langkah, `useHrRemoteData`, NIK `AbortController`, checkpoint Step 3, upload retry Step 4, STR compliance & submit recovery |
| `src/utils/hr/recruitment/candidate-registration-wizard-utils.js` | Baru | Validator murni per step, exact NIK matcher/masker, `isDateOnlyExpired`, `assessClinicalStrCompliance`, checklist builder, sanitizer error |
| `src/style/hr/recruitment/candidate-wizard.module.css` | Baru | CSS Module berbasis token `--region-*` & responsif breakpoint `<= 1100px`, `<= 640px` (tanpa Tailwind) |
| `src/lib/constants/hr/recruitment/recruitment-constants.js` | Diubah | Menambah helper rute `candidateCreate`, `candidateCreateWithVacancy`, `candidateResume` pada `RECRUITMENT_ROUTES` |
| `src/components/view/hr/recruitment/candidates-directory-view.jsx` | Diubah | Mengganti modal pendaftaran menjadi tombol navigasi wizard & menambah tombol *Wizard* per baris |
| `src/components/view/hr/recruitment/candidate-detail-screening-view.jsx` | Diubah | Menambah aksi *Lanjutkan di Wizard* pada `Hero` & memperbaiki kalkulasi `isDateOnlyExpired` STR |
| `tests/unit/candidate-registration-wizard-utils.test.mjs` | Baru | `6` unit test helper murni wizard (NIK, lowongan, validasi step, STR, & submit guard) |

### 3.2 Base Component Decision Gate

| Elemen Layar | Kandidat Base / Reuse | Keputusan | Alasan |
| --- | --- | --- | --- |
| Page container, Hero, ToastStack | `Hero`, `ToastStack`, `dataPage` + `--region-*` | `REUSE` | Konsisten dengan `candidates-directory-view.jsx` & seluruh HRD |
| Pilihan jalur Step 1 | `BaseCheckboxCard` (`variant="compact"`) | `REUSE` | Kartu pilihan radio/checkbox aksesibel bawaan dengan badge & deskripsi |
| Pemilih lowongan Published | `BaseSelectField` (searchable, server options) | `REUSE` | Mendukung pencarian judul/nomor/jabatan/unit & status opsi `disabled` |
| Input identitas, kontak, dokumen | `BaseInputField`, `BaseNativeSelectField`, `BaseDateField`, `BaseTextAreaField`, `BaseCheckboxField` | `REUSE` | Field error, `min`/`max` kalender, & aksesibilitas bawaan `base-form-control` |
| Unggah & unduh dokumen | `HrAttachmentField` (membungkus `BaseFileUpload`) | `REUSE` | Unggah multipart langsung (`<=10 MB`) & unduh ber-izin `RecruitmentService` |
| Tabel arsip dokumen Step 4 | `DataTable` (`sortLatestFirst={false}`) | `REUSE` | Menampilkan kolom verifikasi, nomor, masa berlaku, & unduh |
| Kartu section, grid ringkasan, badge, alert | `HrPanel`, `HrInfoGrid`, `HrStatusBadge`, `StatusBadge`, `InformationAlert`, `BaseButton`, `ConfirmModal` | `REUSE` | Seluruh lencana status, peringatan, tombol loading, & modal konfirmasi keluar |
| Stepper 5 langkah & sidebar ringkasan | `CandidateWizardStepper` & `CandidateWizardSummarySidebar` | `COMPOSE` | Menggubah token `--region-*` dalam `candidate-wizard.module.css` (`portal-credentialing-wizard` memakai alur domain kredensial) |

`UI GATE: PASS — 13 kelompok elemen REUSE, 1 komposisi terstruktur COMPOSE, 0 komponen ad-hoc pengganti base component`

### 3.3 Ringkasan Matriks State Layar per Step

| Step / State | Kondisi Tampil | Penanganan Galat / Batas |
| --- | --- | --- |
| Memuat (`vacanciesLoading`, `resumeLoading`) | Placeholder memuat opsi lowongan atau banner memuat `?candidateId=` | Tidak menampilkan angka/baris palsu sebelum respons selesai |
| Kosong (`Published` = 0) | `InformationAlert` info jujur + panduan jalur Tanpa Lowongan | Tidak men-seed lowongan dummy di UI |
| Dilarang (`!canCreateCandidate && !canUpdateCandidate && !canUploadDocument`) | `InformationAlert` warning blokir akses penuh pada wizard | Tombol kembali ke Direktori Pelamar; zero bocoran data kandidat |
| Galat jaringan / konflik `409` / tutup `422` / retry parsial | Banner error per baris/aksi + `Muat Data Terbaru dari Server` (`RowVersion`) + `Coba Lagi Simpan Metadata` (`key` tetap sama) + refetch otomatis saat timeout `/apply` | Tidak mengirim `POST` profil atau upload fisik ganda |

---

## 4. Endpoint Backend yang Dikonsumsi (`RecruitmentService`)

### `[Tags("Corporate / Human Resource / Recruitment Management / Job Vacancy")]`
Base path Axios: `/v1/corporate/human-resource/recruitment/vacancies`

| Method | Path | Pemakaian Wizard | Hak Akses | Response |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Muat opsi lowongan `Published` Step 1 | `JobVacancy : Read` | `PagedResult<JobVacancyListItemResponse>` |
| `GET` | `/{id}` | Muat detail jabatan/unit/lokasi/persyaratan Step 1 & verifikasi sebelum submit | `JobVacancy : Read` | `JobVacancyResponse` |

### `[Tags("Corporate / Human Resource / Recruitment Management / Candidate")]`
Base path Axios: `/v1/corporate/human-resource/recruitment/candidates`

| Method | Path | Pemakaian Wizard | Hak Akses | Response |
| --- | --- | --- | --- | --- |
| `GET` | `/?searchNik=...` | Lookup NIK 16 digit exact Step 2 | `Candidate : Read` | `PagedResult<CandidateListItemResponse>` |
| `GET` | `/{id}` | Hydrate resume `?candidateId=`, konfirmasi existing, refetch konflik `409` & timeout | `Candidate : Read` | `CandidateResponse` |
| `POST` | `/` | Checkpoint persisten Step 3 kandidat baru | `Candidate : Create` | `CandidateResponse` |
| `PUT` | `/{id}` | Simpan perubahan biodata/kontak kandidat existing bila ada diff | `Candidate : Update` | `CandidateResponse` |
| `POST` | `/documents/attachments` | Upload berkas fisik melalui `HrAttachmentField` Step 4 | `Candidate : UploadDocument` | `HrStoredAttachment` (`201`) |
| `POST` | `/{id}/documents` | Simpan metadata berkas (`applicationId: null`) + retry Step 4 | `Candidate : UploadDocument` | `CandidateDocumentResponse` |
| `GET` | `/documents/attachments/download` | Unduh berkas dari arsip dokumen Step 4 | `Candidate : Read` | `Blob` file |
| `POST` | `/{id}/apply` | Ajukan lamaran pada Step 5 mode lowongan aktif | `Candidate : Update` | `CandidateApplicationResponse` |

---

## 5. Hasil Verifikasi Pasca-Edit Terakhir

| Pemeriksaan / Perintah | Hasil Aktual | Status |
| --- | --- | --- |
| `node ".../check-css-usage.mjs"` (6 berkas `.jsx` wizard & integrasi) | `OK — 6 berkas diperiksa, 0 temuan` | `PASS` |
| `npx eslint --max-warnings=0` (seluruh 10 berkas JS/JSX/MJS baru & diubah) | `0 error`, `0 warning`, exit `0` | `PASS` |
| `npm run lint:errors` (`eslint . --quiet` seluruh repo FE) | Exit `0` | `PASS` |
| Pemindaian kelas Tailwind utility pada 6 `.jsx` berubah | `Tailwind hits: 0` | `PASS` |
| Unit test helper wizard (`candidate-registration-wizard-utils.test.mjs`) | `6/6 PASS`, `0 fail`, `144.09 ms` | `PASS` |
| Unit test penuh repo (`npm run test:unit`) | `5546 tests: 5397 pass, 149 fail` — **0 gagal di modul rekrutmen** (`149` fail seluruhnya pre-existing pada `workforce-salary-assignment.test.mjs`, `payroll-*`, `inpatient-*`, dll.) | `PASS (Scoped)` |
| Build produksi (`npm run build` + `postbuild` standalone) | `✓ Compiled successfully in 78s`; `✓ Generating static pages (935/935)`; route `○ /hr/recruitment/candidates/create` terbangun; standalone siap | `PASS` |
| Uji live browser Chromium (`wiz-1125-uat-browser.mjs`) | `12/12` skenario otomasi lulus (`0` HTTP 5xx, `0` pageError) — lihat rincian jujur per UAT di laporan pengujian live | `PASS` |

---

## 6. Tangkapan Layar Bukti UI (`test-with-agy/screenshots/wiz-1125/`)

| Tahap / Kasus | Berkas Screenshot | Bukti yang Terkonfirmasi |
| --- | --- | --- |
| Step 1 Entry Full-Page | `01-step1-wizard-entry.png` | Hero Pelamar, 5-step stepper penuh tanpa elipsis, sidebar tetap 8 menu rekrutmen |
| Step 2 Identitas & Cek NIK | `02-step2-identity.png` | Lookup NIK non-existing, form biodata, kalender date picker, ringkasan sidebar |
| Step 2 Rehire Existing NIK | `07-step2-rehire-match.png` | Kartu match `CAN-20261125-0003` dengan badge *Riwayat Rehire (Mantan Pegawai)* & opsi reuse |
| Step 3 Kontak & Checkpoint | `03-step3-contact.png` | Form telepon/email/domisili & info checkpoint persisten `Registered` |
| Step 4 Dokumen & Arsip | `04-step4-documents.png` | KTP tersimpan (`Tersimpan`), preview nama file, `Berlaku Sejak/Sampai`, toast sukses |
| Step 4 Partial Retry (Klinis) | `14-step4-partial-retry.png` | `2` tersimpan (`KTP`, `Ijazah`) & `1` baris gagal (`CV`) terisolasi untuk retry tanpa re-upload |
| Step 5 Review (NIK Tersamar) | `05-step5-review.png` | NIK `999721••••••0001` tersamar, tombol *Ubah Identitas*/*Kontak* & konfirmasi |
| Sukses Tanpa Lowongan (`LIVE_BE`) | `06-step5-success-nolow.png` | `CAN-20261125-0008` tercatat `Terdaftar` tanpa lamaran palsu (`0x /apply`) |
| Step 1 & Sukses Lowongan Klinis (`ROUTE_INJECTED`) | `13-step1-clinical-vacancy.png`, `15-clinical-apply-success.png` | Info jabatan/unit/kuota klinis & pengiriman `APP-CLIN-211816` setelah multi-STR |
| Sukses Lowongan Non-Klinis (`ROUTE_INJECTED`) | `12-nonclinical-apply-success.png` | `APP-NON-211816` terkirim tanpa memaksakan STR |
| Pemulihan Timeout Submit (`ROUTE_INJECTED`) | `16-timeout-recovery-success.png` | Refetch mendeteksi `APP-RECOVER-211816` sudah tercatat di server tanpa submit ulang |
| Responsif 390px & Detail/RBAC | `08-wizard-mobile-390.png`, `09-candidate-detail-integration.png`, `10-rbac-recruiter-directory.png`, `11-rbac-asesor-denied.png` | Mobile `overflow=-15px`, tombol detail *Lanjutkan di Wizard*, RBAC Recruiter (`0` tombol buat) & Asesor terblokir |

---

## 7. Catatan Penutup

- **Perubahan di luar scope**: `NONE` (perubahan pre-existing pada `tests/unit/self-service-side-menu.test.mjs` dipertahankan utuh).
- **Batasan lingkungan pengujian**: Karena DB dev memiliki `0` lowongan `Published`, skenario lowongan aktif (`UAT-02`, `UAT-03`, `UAT-07`, `UAT-11` s.d. `UAT-14`, `UAT-16` s.d. `UAT-18`, `UAT-25`, `UAT-26`, `UAT-27`) memakai injeksi respons terukur pada `page.route` terhadap backend `7194` (upload berkas fisik & simpan metadata ke database tetap berjalan nyata ke `7194`).
- **Memory sync**: `MEMORY_PENDING_SYNC`.
