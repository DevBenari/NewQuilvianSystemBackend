# Laporan Perubahan Frontend — `FE-HRD-R39`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R39` |
| Judul | Migrasi Rekrutmen dan perbaikan isian UUID |
| Slice | `S-D2` (Rekrutmen & Hiring), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R39`; task asal `FE-HRD-044`–`056` |
| Trace | API Contract v8 bagian 14; HRD-DEC-073 s.d. HRD-DEC-080; VR-REC-02, 03, 05, 11 |
| Contract version | API Contract v8 + source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Migrasi base component; isian UUID diganti pilihan bernama |
| Dependency | `FE-HRD-R14` — 🟡 |
| Klasifikasi | `HEAVY` — 13 berkas, 32 pelanggaran, 13 isian UUID |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — 13 layar bermigrasi, 13 isian UUID menjadi pilihan bernama, formulir kembali berfungsi; unggah dokumen kandidat dan sertifikat MCU tertahan backend |

---

## 1. Keadaan yang ditemukan di awal

- **Semua formulir buat Rekrutmen tidak bisa dipakai**: isian memakai `BaseFormControl` (hanya pembungkus label) seolah-olah input, sehingga kotak isian tidak pernah dirender.
- 13 isian meminta petugas mengetik UUID (permintaan headcount, unit, jabatan, permintaan rekrutmen, lamaran, grade gaji, penawaran, kandidat, lowongan asal/target, kursi headcount).
- Nilai rekaan: `hospitalSiteId` dan `contractTypeId` tetap `0000…0001`, batas grade gaji 8.000.000, tanggal lahir kandidat 1995-01-01, tempat lahir/alamat "-".
- Evaluasi HR/klinis mengirim ID kandidat sebagai ID evaluasi dan rekomendasi 0/1/2 (server 1/2/3) — selalu gagal.
- Jawaban kandidat "menerima" dikirim dengan kode 4 (= Ditolak Penyetuju); hasil MCU "Laik" dikirim 0 (enum mulai 1).
- Unggah dokumen kandidat mengirim path contoh `/uploads/str-sample.pdf`, ukuran tetap, dan kode jenis 3 (Ijazah, bukan STR).
- Peta status memakai angka mulai 0 dan kunci `StatusBadge` "neutral" yang tidak sah; tab pelamar menyaring nilai "PASSED/FAILED" yang tidak pernah dikirim server.

## 2. Proses bisnis dari sisi pengguna

1. **Permintaan rekrutmen**: HR memilih permintaan headcount yang siap direkrut (cari nomor/jabatan); rumah sakit, unit, dan jabatan terisi otomatis. Draf diajukan (server langsung menyetujui) atau dibatalkan dengan alasan.
2. **Lowongan**: dibuat dari permintaan yang disetujui (judul/kuota terisi dari permintaan), ditandai klinis bila wajib STR, lalu ditayangkan atau ditutup. Rincian lowongan menampilkan pelamar per status seleksi.
3. **Kandidat**: HR mendaftarkan kandidat (NIK 16 digit, tanggal lahir wajib); mantan pegawai dikenali sebagai rehire. Di rincian kandidat, HR mendaftarkan ke lowongan tayang, memulai dan menilai **seleksi berkas** (verifikasi STR).
4. **Evaluasi**: per lamaran, evaluator (pengguna yang login) membuka sesi Wawancara HR atau Asesmen Klinis, lalu mengisi skor 0–100 dan rekomendasi. Pada asesmen klinis, "Tidak direkomendasikan" adalah veto mutlak (VR-REC-11).
5. **Penawaran**: pilih kandidat → lamaran, grade gaji (rentang dari master), jenis kontrak, gaji, tanggal masuk, batas jawaban. Di atas batas grade masuk jalur pengecualian. Penyetuju menyetujui/menolak per tahap, lalu penawaran bersyarat dikirim dan jawaban kandidat dicatat.
6. **MCU**: rujukan diterbitkan untuk penawaran bersyarat yang diterima; hasil laik/laik dengan batasan/tidak laik dicatat dokter pemeriksa.
7. **Talent pool**: kandidat berperingkat disimpan dengan masa retensi, lalu dapat di-fast-track ke lowongan tayang (membuat draf penawaran).
8. **Serah terima**: penawaran final diterima dibuatkan serah terima (opsional dikaitkan kursi headcount), lalu diserahkan ke onboarding.
9. Jalur tidak normal: tanpa hak tampil peringatan; galat server tampil; tombol simpan nonaktif sampai isian wajib valid.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Sepuluh controller Rekrutmen (bagian 5), seluruh DTO dan enum `RecruitmentManagement`, layanan `JobRequisitionService`, `JobOfferService`, `JobOfferApprovalService`, `CandidateEvaluationService`, `McuClearanceService`, `HiringHandoffService`, DTO headcount dan grade gaji.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/recruitment/recruitment-constants.js` | BARU: rute, peta status per `*Name`, opsi enum angka, 6 resource pilihan |
| 13 view `components/view/hr/recruitment/*.jsx` | Ditulis ulang (evaluasi HR/klinis menjadi pembungkus tipis) |
| `components/features/hr/recruitment/CandidateEvaluationWorkspace.jsx` | BARU: alur jadwal → penilaian |
| `components/features/hr/recruitment/JobOfferFormModal.jsx` | BARU: penawaran dengan kandidat → lamaran |
| `components/features/hr/shared/hr-resource-field.jsx` | Prop opsional `filters` (kompatibel mundur) untuk membatasi pilihan per status |
| `src/lib/state/store.jsx` | Slice `recruitment` dicabut |
| `recruitment-slice.js`, `recruitment.module.css`, `recruitment-display-utils.js` | Dihapus |
| `menu-items.jsx` | `requiredPermission` tujuh menu Rekrutmen |

Pemetaan 13 isian UUID: permintaan rekrutmen (headcount siap direkrut → rumah sakit/unit/jabatan otomatis, 3 isian), lowongan (permintaan disetujui), penawaran (kandidat → lamaran; grade gaji), MCU (penawaran bersyarat diterima), talent pool (kandidat, lowongan asal, lowongan target tayang, grade gaji), serah terima (penawaran final diterima; kursi dari permintaan headcount).

### 3.3 Kepatuhan arsitektur frontend

`page → view → service`; Redux slice dicabut dan diganti `useHrRemoteData`. Evaluator diambil dari `state.auth.userInfo`.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, ringkasan, filter, tabel | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `DataTable` | `REUSE` | — |
| Isian referensi bernama | `HrResourceField` (`ResourceFilterSelect`) | `REUSE` | Menggantikan 13 isian UUID |
| Dialog formulir | `ConfirmModal` + `BaseFormControl` | `COMPOSE` | — |
| Ruang kerja evaluasi | `HrPanel` + `DataTable` + `ConfirmModal` | `COMPOSE` | Dipakai dua layar |
| Status | `HrStatusBadge` / `StatusBadge` | `REUSE` | — |

`UI GATE: PASS — 5 elemen REUSE, 2 COMPOSE, 0 NEW`

```text
$ node .../check-css-usage.mjs <15 berkas JSX R39>
OK — 1 berkas diperiksa, 0 temuan. (×15)
$ grep Tailwind <berkas R39>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/panel |
| Kosong | "Belum ada permintaan rekrutmen." / "Kandidat belum melamar lowongan." / "Belum ada pelamar pada tab ini." |
| Gagal | Pesan server di `InformationAlert` |
| Tanpa hak akses | "Anda belum punya hak untuk melihat …"; tombol aksi disembunyikan |

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Recruitment Management / Job Requisition

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`/`POST` | `/v1/corporate/human-resource/recruitment/requisitions` | Daftar / buat | `JobRequisition : Read` / `Create` |
| `GET` | `.../requisitions/{id}` | Rincian | `JobRequisition : Read` |
| `POST` | `.../requisitions/{id}/submit` | Ajukan (`rowVersion`) | `JobRequisition : Submit` |
| `POST` | `.../requisitions/{id}/cancel` | Batalkan | `JobRequisition : Cancel` |

#### Corporate / Human Resource / Recruitment Management / Job Vacancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`/`POST` | `/v1/corporate/human-resource/recruitment/vacancies` | Daftar / buat | `JobVacancy : Read` / `Create` |
| `GET` | `.../vacancies/{id}` | Rincian | `JobVacancy : Read` |
| `POST` | `.../vacancies/{id}/publish`, `.../close` | Tayang / tutup | `JobVacancy : Publish` / `Close` |

#### Corporate / Human Resource / Recruitment Management / Candidate

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`/`POST` | `/v1/corporate/human-resource/recruitment/candidates` | Daftar / daftarkan | `Candidate : Read` / `Create` |
| `GET` | `.../candidates/{id}` | Rincian, lamaran, dokumen | `Candidate : Read` |
| `POST` | `.../candidates/{id}/apply` | Daftarkan ke lowongan | `Candidate : Update` |

#### Corporate / Human Resource / Recruitment Management / Candidate Screening

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/recruitment/screenings` | Hasil seleksi / pelamar lowongan | `CandidateScreening : Read` |
| `POST` | `.../screenings`, `.../screenings/evaluate` | Mulai / nilai seleksi | `CandidateScreening : Evaluate` |

#### Corporate / Human Resource / Recruitment Management / Candidate Evaluation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/recruitment/evaluations` | Sesi per lamaran | `CandidateEvaluation : Read` |
| `POST` | `.../evaluations/schedule` | Buka sesi | `CandidateEvaluation : Evaluate` |
| `POST` | `.../evaluations/{id}/submit-hr` | Nilai wawancara HR | `CandidateEvaluation : Evaluate` |
| `POST` | `.../evaluations/{id}/submit-clinical` | Nilai asesmen klinis | `CandidateEvaluation : EvaluateClinical` |

#### Corporate / Human Resource / Recruitment Management / Job Offer

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`/`POST` | `/v1/corporate/human-resource/recruitment/offers` | Daftar / buat | `JobOffer : Read` / `Create` |
| `GET` | `.../offers/{id}` | Rincian | `JobOffer : Read` |
| `POST` | `.../offers/{id}/send-conditional`, `.../candidate-respond` | Kirim / jawaban kandidat | `JobOffer : Update` |

#### Corporate / Human Resource / Recruitment Management / Job Offer Approval

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/recruitment/offers/{offerId}/approvals` | Riwayat persetujuan | `JobOfferApproval : Read` |
| `POST` | `.../approvals/approve`, `.../approvals/reject` | Putusan penyetuju | `JobOfferApproval : Approve` |

#### Corporate / Human Resource / Recruitment Management / MCU Clearance

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/recruitment/mcu-clearance` | Daftar rujukan | `OccupationalHealthClearance : Read` |
| `POST` | `.../mcu-clearance/register` | Terbitkan rujukan | `OccupationalHealthClearance : Create` |
| `POST` | `.../mcu-clearance/{id}/record-result` | Catat hasil | `OccupationalHealthClearance : Evaluate` |

#### Corporate / Human Resource / Recruitment Management / Talent Pool

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/recruitment/talent-pool` | Daftar | `TalentPool : Read` |
| `POST` | `.../talent-pool/enroll` | Tambah | `TalentPool : Create` |
| `POST` | `.../talent-pool/{id}/fast-track-promote` | Fast-track | `TalentPool : Promote` |

#### Corporate / Human Resource / Recruitment Management / Hiring Handoff

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/recruitment/handoffs` | Daftar | `HiringHandoff : Read` |
| `POST` | `.../handoffs/generate`, `.../{id}/confirm-transfer` | Buat / serahkan | `HiringHandoff : Execute` |

Pendukung: `workforce-planning/headcount-requests` (daftar dan rincian kursi), `master-data/salary-grades/options`, `master-data/contract-types/options`.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 13 view + 2 komponen + konstanta + `hr-resource-field` + store + menu | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 15 JSX (termasuk aturan `UUID-DIKETIK`) | 0 temuan | `PASS` | 3.4 |
| `npm run build` BUILD-2 | 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` | Terpenuhi | 3.2, 3.4 |
| Petugas memilih nama, tidak mengetik UUID (contoh: isian Pelamar dapat dicari) | Terpenuhi | 13 isian → pilihan; pemeriksa 0 temuan `UUID-DIKETIK` |
| Unggah dokumen kandidat dan sertifikat MCU | Belum terpenuhi | Server hanya menerima `FilePath` tanpa endpoint unggah; unggah dilepas dari UI |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Submit permintaan rekrutmen langsung menyetujui (tanpa tahap persetujuan) |
| Masalah yang diketahui | Tidak ada endpoint daftar lamaran (dipilih via kandidat); daftar seleksi tanpa filter `applicationId` (dicari via nama lalu disaring); `CandidateScreeningListItem` tanpa `CandidateId`; evaluator selalu pengguna login |
| Dependency backend | Endpoint unggah berkas rekrutmen; tahap persetujuan permintaan; filter lamaran pada seleksi |
| Perubahan sampingan | `HrResourceField` mendapat prop `filters` opsional; slice, CSS, dan util lama dihapus |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Endpoint unggah berkas; uji browser alur rekrutmen end-to-end |
