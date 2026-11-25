# Laporan Live Testing — Submenu Pelamar (Wizard Pendaftaran 5 Langkah)

| Field | Nilai |
| --- | --- |
| Menu | Sumber Daya Manusia → Siklus Kepegawaian → Rekrutmen & Seleksi → **Pelamar** (`/hr/recruitment/candidates`, `/hr/recruitment/candidates/create`, `/hr/recruitment/candidates/[id]`) |
| Task | `FIN-HRD-REC-WIZ-04` (memvalidasi `FIN-HRD-REC-WIZ-01..03`) |
| Waktu uji (WIB) | 25 November 2026, ~17:09 – 17:41 WIB |
| Lingkungan FE | Next.js dev server lokal (`http://localhost:3000`, terkonfirmasi pula build produksi `npm run build` 935 halaman sukses) |
| Lingkungan BE & DB | Build backend terbaru terisolasi `https://localhost:7194` (`QuilvianSystemBackend.dll` hasil kompilasi setelah perubahan `FIN-HRD-REC-WIZ-01`) + proxy Playwright; Database dev `QuilvianNewDevHamzah` (tercatat **0 Headcount Request, 0 Job Requisition, 0 Job Vacancy** pada sesi uji) |
| Peramban | Google Chrome (`Chromium 158`) mode visual headed, `slowMo: 300 ms`, `1440x900` & `390x844` |
| Peran / Akun uji (tanpa kredensial) | (1) `SuperAdmin` (`superadmin@admin.com`) untuk akses penuh kandidat/dokumen/lowongan; (2) `HR Recruiter` (`uat.pkb.recruiter@rsmmc.local`) pemegang `Candidate:Read`, `JobVacancy:Read`, `CandidateScreening:Read/Evaluate` tanpa hak `Create/Update/UploadDocument`; (3) `HR Asesor` (`uat.pkb.asesor@rsmmc.local`) pemegang `CandidateEvaluation:Read/Clinical` tanpa hak `Candidate:*` |
| Skrip bukti | `test-with-agy/scripts/wiz-1125-be-api-test.mjs` (`6/6 PASS`) dan `test-with-agy/scripts/wiz-1125-uat-browser.mjs` (`12/12` grup browser `PASS`, `0` HTTP 5xx, `0` pageError) |

---

## 1. Ringkasan Eksekutif & Batasan Jujur Lingkungan

Seluruh 30 kasus uji (`UAT-01` s.d. `UAT-30`) dijalankan dan dievaluasi secara transparan dengan klasifikasi sumber bukti berikut:
1. **Bukti Backend & Database Nyata (`LIVE_BE` & `BE_API_LIVE`)**:
   - Registrasi profil kandidat mode Tanpa Lowongan (`CAN-20261125-0008`), kode dari `NumberSeriesAllocator` (`CAN-20261125-0001`, `CAN-20261125-0003`, `CAN-20261125-0008`), deteksi Rehire `MstEmployee` (`999911••••••0006` -> `RSMMC-20260105006`), resume `?candidateId=`, unggah berkas multipart fisik + simpan metadata (`IsVerified=false`), penolakan upload/ekstensi/tanggal invalid, proteksi balapan paralel NIK (`200`/`409`), konflik `RowVersion` (`409`), idempotensi upload metadata, pengecekan `0` PII pada storage browser, tampilan mobile `390px`, navigasi keyboard, serta kontrol akses RBAC API + UI.
2. **Simulasi Terukur via `page.route` (`ROUTE_INJECTED`) atas `0` Lowongan DB Dev**:
   - Karena DB dev `QuilvianNewDevHamzah` **tidak memiliki data master Headcount Request / Job Requisition / Job Vacancy `Published`** dan instruksi melarang seeding master tanpa otorisasi pemilik, daftar dan detail lowongan `Published` (klinis `VAC-UAT-CLIN-01` & non-klinis `VAC-UAT-NON-02`) serta fault HTTP `409`/`422`/`500`/timeout diinjeksikan lewat `page.route` sambil tetap menyalurkan unggah dokumen & profil pelamar secara nyata ke backend `7194`.
   - Konsekuensi jujur: pengajuan lamaran (`POST /candidates/{id}/apply`) persisten ke tabel `HrdCandidateApplications` nyata dan penciptaan evaluasi `CandidateScreening` baru di DB dev ditandai **terblokir data master (`BLOCKED_DEV_DB_NO_VACANCY`)** pada lapisan database end-to-end, meskipun logika UI + validasi FE + penanganan API `ApplyVacancyAsync` di source & kompilasi telah terverifikasi.

---

## 2. Alur Proses Bisnis yang Diuji

Contoh skenario RS MMC: Petugas rekrutmen SDM (`SuperAdmin`) membuka menu **Pelamar** dan memilih **Daftarkan Pelamar** untuk:
1. Mencatat pelamar baru tanpa lowongan aktif (*Siti Sintetis 211816*, NIK sintetis `999721••••••0001`): melakukan lookup NIK -> mengisi biodata & kontak -> menekan checkpoint Step 3 (`POST /candidates` -> `CAN-20261125-0008`) -> mengunggah KTP di Step 4 -> memverifikasi NIK tersamar & selesai tanpa membuat lamaran palsu -> memverifikasi refresh `?candidateId=` memuat ulang profil & arsip dokumen dari server.
2. Mengecek NIK mantan pegawai `MstEmployee` (`999911••••••0006`): sistem menampilkan indikasi **Riwayat Rehire (`RSMMC-20260105006`)** untuk *CAN-20261125-0003*, memuat profil tanpa `POST` duplikat, dan memulihkan konflik `RowVersion` (`409`).
3. Menyimulasikan pendaftaran ke lowongan **Non-Klinis** (`VAC-UAT-NON-02`, STR tidak dipaksakan) dan **Klinis** (`VAC-UAT-CLIN-01`, peringatan missing STR `VR-REC-07`, kegagalan parsial CV sementara Ijazah berhasil disimpan & retry CV tanpa unggah file fisik ulang, peringatan STR kedaluwarsa `VR-REC-08` serta multi-STR campuran, lalu navigasi aman ke `/hr/recruitment/screenings`).

---

## 3. Matriks Hasil Uji Lengkap (`UAT-01` s.d. `UAT-30`)

| ID | Prio | Skenario | Klasifikasi | Hasil Aktual | Bukti |
| --- | --- | --- | --- | --- | --- |
| `UAT-01` | P0 | Klik *Daftarkan Pelamar*, buka wizard full-page tanpa modal & tanpa ubah 8 menu sidebar | `PASS [LIVE_BE]` | Terbuka di `/hr/recruitment/candidates/create`, `modalCount=0`, sidebar intact | `01-step1-wizard-entry.png` |
| `UAT-02` | P0 | Lowongan Non-Klinis aktif -> isi Step 1–5 -> review & apply | `PASS [ROUTE_INJECTED]` *(DB apply nyata `BLOCKED`: 0 lowongan DB dev)* | Non-klinis `VAC-UAT-NON-02` lolos tanpa memaksa STR; `/apply` mengirim `vacancyId` + `notes`, sukses `APP-NON-211816` | `12-nonclinical-apply-success.png` |
| `UAT-03` | P0 | Lowongan Klinis + STR valid & dokumen pendukung -> apply sukses | `PASS [ROUTE_INJECTED]` *(DB apply nyata `BLOCKED`: 0 lowongan DB dev)* | Kartu klinis tampil; dokumen & STR valid terunggah nyata ke BE `7194`; `/apply` hasilkan `APP-CLIN-211816` tanpa auto-lulus screening | `13-step1-clinical-vacancy.png`, `15-clinical-apply-success.png` |
| `UAT-04` | P0 | Mode Tanpa Lowongan: checkpoint profil Step 3, upload KTP Step 4, selesai Step 5 | `PASS [LIVE_BE]` | `CAN-20261125-0008` + KTP (`IsVerified=false`) tersimpan nyata di DB; `0` panggilan `/apply` | `06-step5-success-nolow.png` |
| `UAT-05` | P0 | Refresh / resume melalui `?candidateId=` memuat profil + arsip dokumen tanpa register baru | `PASS [LIVE_BE]` | Reload memuat `CAN-20261125-0008` & `1` arsip dokumen server; lowongan dipilih ulang di Step 1 | `browser-uat-results.json` |
| `UAT-06` | P0 | Lookup exact NIK pelamar existing -> reuse tanpa `POST` duplikat | `PASS [LIVE_BE]` | `999911••••••0006` memuat `CAN-20261125-0003`; `POST /candidates` tidak bertambah (`0`) | `07-step2-rehire-match.png` |
| `UAT-07` | P0 | Kirim lamaran ganda pada candidate–vacancy yang sama | `PASS [ROUTE_INJECTED]` + kode BE `409` terverifikasi | UI menahan pilihan ganda via badge/validasi Step 5; respons `409` terdeteksi dan menayangkan nomor lamaran eksisting `APP-RECOVER-211816` | `browser-uat-results.json` |
| `UAT-08` | P0 | Double click tombol checkpoint Step 3 & submit akhir | `PASS [LIVE_BE]` | Klik ganda sinkron pada `wizard-step3-save-next` tetap mengirim tepat `1` `POST /candidates` (`savingProfileLockRef` + tombol `loading`) | `wiz-1125-uat-browser.mjs` |
| `UAT-09` | P0 | Concurrency paralel 2 sesi (NIK kembar & apply kembar) | `PASS [BE_API_LIVE]` (NIK) / `BLOCKED` (apply DB nyata) | 2 request paralel `POST /candidates` NIK identik -> `1x 200` (`CAN-20261125-0001`) + `1x 409`, `0` HTTP 500 (`BE-API-02`). Apply paralel DB nyata menunggu data vacancy `Published` | `be-api-results.json` (`BE-API-02`) |
| `UAT-10` | P0 | Akun terbatas (`Recruiter` tanpa Create/Update/Upload; `Asesor` tanpa Read) | `PASS [LIVE_BE]` + `PASS [BE_API_LIVE]` | Recruiter tidak melihat CTA buat (`0` tombol) & write ditolak BE `403/403/403`; Asesor diblokir pesan izin & GET/Download `403` | `10-rbac-recruiter-directory.png`, `11-rbac-asesor-denied.png`, `BE-API-06` |
| `UAT-11` | P0 | Lowongan tutup di tengah pengisian wizard -> profil/dokumen aman & pilih ulang | `PASS [ROUTE_INJECTED]` | Injeksi `422` penutupan menampilkan alert jelas tanpa menghapus profil/dokumen `CAN-20261125-0008` | `browser-uat-results.json` |
| `UAT-12` | P0 | Timeout jaringan saat submit lamaran -> refetch deteksi status server sebelum retry | `PASS [ROUTE_INJECTED]` | Timeout aborted memicu refetch otomatis `GET /candidates/{id}`, menemukan `APP-RECOVER-211816` & menampilkan sukses terkonfirmasi tanpa ganda | `16-timeout-recovery-success.png` |
| `UAT-13` | P0 | Upload fisik sukses, simpan metadata gagal -> retry pakai `key` sama & idempoten | `PASS [LIVE_BE]` (`BE-API-05`) + `PASS [ROUTE_INJECTED]` | Retry CV setelah metadata `500` memakai `key` lampiran awal (`0` re-upload fisik), BE mengembalikan 1 baris idempoten | `14-step4-partial-retry.png`, `BE-API-05` |
| `UAT-14` | P0 | Gagal upload salah satu berkas di antara banyak berkas | `PASS [ROUTE_INJECTED]` | Kegagalan baris CV tidak membatalkan Ijazah (`2` tersimpan di server, hanya CV yang butuh klik *Coba Lagi*) | `14-step4-partial-retry.png` |
| `UAT-15` | P0 | Coba manipulasi `filePath` palsu & akses berkas oleh role tanpa izin | `PASS [BE_API_LIVE]` | Path `/uploads/fake-path.pdf` ditolak `400`; download/upload oleh Asesor & upload oleh Recruiter ditolak `403` | `be-api-results.json` (`BE-API-05`, `06`) |
| `UAT-16` | P1 | Lowongan klinis tanpa STR & STR kedaluwarsa | `PASS [ROUTE_INJECTED]` | `VR-REC-07` warning tampil saat STR kosong; mengunggah STR berakhir `2023-06-14` memunculkan peringatan `kedaluwarsa` | `14-step4-partial-retry.png`, `browser-uat-results.json` |
| `UAT-17` | P1 | Kandidat non-klinis tanpa STR | `PASS [ROUTE_INJECTED]` | Slot STR tidak dipaksakan di Step 4 & pengajuan lamaran sukses normal | `12-nonclinical-apply-success.png` |
| `UAT-18` | P1 | Kandidat punya 2 STR (lama kedaluwarsa + baru aktif) | `PASS [ROUTE_INJECTED]` + catatan `PENDING_OWNER_DECISION` | UI menampilkan warning `2 dokumen STR` & menandai pembaruan aktif sebagai acuan terbaru; BE `EvaluateScreeningAsync` mengurutkan `CreateDateTime DESC` | `browser-uat-results.json` |
| `UAT-19` | P1 | NIK terkait `MstEmployee` tampilkan status Rehire | `PASS [LIVE_BE]` + `PASS [BE_API_LIVE]` | `999911••••••0006` tampil lencana Rehire & `RSMMC-20260105006` pada kartu lookup & detail tanpa menyimpulkan kelulusan | `07-step2-rehire-match.png`, `BE-API-03` |
| `UAT-20` | P1 | Konflik `RowVersion` edit dua sesi pada kandidat existing (`PUT 409`) | `PASS [LIVE_BE]` + `PASS [BE_API_LIVE]` | Backend menolak stale `rowVersion` dengan `409`; wizard memunculkan tombol *Muat Data Terbaru dari Server* | `BE-API-04`, `browser-uat-results.json` |
| `UAT-21` | P1 | Validasi inline NIK, nama kosong, jenis kelamin, tanggal lahir, & BE `400` | `PASS [LIVE_BE]` + `PASS [BE_API_LIVE]` | FE menahan Step 2 -> Step 3; BE menolak NIK pendek & `BirthDate` masa depan dengan `400` | `BE-API-01`, `browser-uat-results.json` |
| `UAT-22` | P1 | Penolakan file `.txt`, file `> 10 MB`, & kalender `ValidFrom > ValidUntil` | `PASS [LIVE_BE]` + `PASS [BE_API_LIVE]` | FE tolak `.txt` ("Jenis berkas harus..."), tolak `10.6 MB` ("Ukuran berkas melebihi 10 MB"), & blokir tanggal 2024 sebelum `Berlaku Sejak` 2025; BE tolak tanggal terbalik `400` | `04-step4-documents.png`, `BE-API-05` |
| `UAT-23` | P1 | Kembali dari Step 5 -> Step 2 lalu balik ke Step 5 tanpa kehilangan dokumen | `PASS [LIVE_BE]` | Data sesi tetap utuh dan dokumen KTP tersimpan tetap ada saat kembali ke Step 5 | `05-step5-review.png` |
| `UAT-24` | P1 | Navigasi keluar saat form kotor memicu `ConfirmModal` guard | `PASS [LIVE_BE]` | Dialog *Tinggalkan Halaman Pendaftaran* muncul dan *Tetap di Halaman* menahan navigasi keluar | `browser-uat-results.json` |
| `UAT-25` | P1 | Kandidat yang sama melamar lowongan kedua yang berbeda | `PASS [ROUTE_INJECTED]` | Lowongan pertama berstatus sudah dilamar; kandidat memilih lowongan kedua `VAC-UAT-CLIN-01` dan menerbitkan `APP-SECOND-211816` | `browser-uat-results.json` |
| `UAT-26` | P1 | Klik *Lanjut ke Seleksi Administrasi* setelah lamaran berhasil | `PASS [ROUTE_INJECTED]` *(data DB screening nyata `BLOCKED`: 0 lowongan DB dev)* | Tombol muncul untuk `SuperAdmin` (`CandidateScreening:Read`) dan menavigasi benar ke `/hr/recruitment/screenings` | `15-clinical-apply-success.png` |
| `UAT-27` | P2 | Error GET lowongan `500` -> tombol retry memulihkan ke empty state jujur | `PASS [ROUTE_INJECTED]` + `PASS [LIVE_BE]` | Alert `500` tampil -> klik *Muat Ulang Daftar Lowongan* pulih ke empty state "Belum ada lowongan kerja berstatus Published" | `browser-uat-results.json` |
| `UAT-28` | P2 | Responsif `390x844`, fokus `Tab` keyboard, & ARIA stepper | `PASS [LIVE_BE]` | Mobile `390px` horizontal overflow `-15px` (0 overflow); `Tab` memfokus `BUTTON`; sidebar & ringkasan tersusun vertikal | `08-wizard-mobile-390.png` |
| `UAT-29` | P2 | Regresi Direktori, Detail Pelamar, Lowongan, & Seleksi | `PASS [LIVE_BE]` | Halaman Detail tampilkan tombol *Lanjutkan di Wizard*; halaman Lowongan & Seleksi termuat normal | `09-candidate-detail-integration.png` |
| `UAT-30` | P2 | Inspeksi `localStorage` & `sessionStorage` pasca-reload | `PASS [LIVE_BE]` | Zero NIK, zero email, dan zero file draft pada `localStorage`/`sessionStorage` | `browser-uat-results.json` |

---

## 4. Temuan & Perbaikan Selama Pengujian

| Kode | Asal | Masalah Ditemukan Saat Uji | Perbaikan yang Diterapkan & Verifikasi Retest |
| --- | --- | --- | --- |
| `FIX-UAT-01` | UI Stepper (`1440px`) | Label langkah `"Informasi Kontak"` dan `"Unggah Dokumen"` terpotong elipsis (`fail-uat-24.png`) karena grid 5 kolom sempit. | Dipadatkan menjadi `"Kontak & Profil"` & `"Dokumen"` pada `WIZARD_STEPS`; retest seluruh screenshot menampilkan 5 langkah penuh tanpa pemotongan teks. |
| `FIX-UAT-02` | Selektor & Upload Chrome 158 (Skrip Uji) | (a) `page.locator("main")` dan input `BaseCheckboxCard` (`pointer-events: none`) tidak cocok DOM nyata; (b) `node fetch` manual di `setupApiProxy` serta path CDP `setInputFiles` mengirim `0` byte ke `POST /documents/attachments` (`400 Pilih berkas`). | Beralih ke `.administrator-region-page` / `label[for=...]`, `route.fetch()`, dan payload memori `{ name, mimeType, buffer }` pada skrip uji — retest menghasilkan `12/12 PASS` browser & `201 Created` berkas nyata ke `7194`. |

---

## 5. Penutup & Prasyarat Uji Lanjutan Penuh Tanpa Mock

1. Untuk menutup sisa celah `BLOCKED` pada DB dev (`UAT-02`, `UAT-03`, `UAT-07`, `UAT-09` apply paralel, `UAT-26` screening dari DB nyata), perlu 1 **Lowongan Klinis `Published`** dan 1 **Lowongan Non-Klinis `Published`** yang dibuat melalui alur resmi *Permintaan Requisition → Lowongan* atas izin pemilik data dev.
2. Daemon sementara backend port `7194` yang dipakai pada pengujian terisolasi telah dihentikan bersih setelah pengujian selesai (`0` proses latar tersisa).
