# MASTER INSTRUKSI AGENT — RECRUITMENT JOURNEY QUILVIAN V2

| Metadata | Ketentuan |
|---|---|
| Tanggal baseline | 25 November 2026 |
| Modul | Sumber Daya Manusia (HRD) |
| Lokasi | Siklus Kepegawaian → Rekrutmen & Seleksi → Pelamar → Detail Pelamar |
| Menu bisnis / slice | Menu 03 — Rekrutmen & Seleksi; `S-D2` |
| Feature | Recruitment Journey, tahap 2 setelah Multi-Step Pendaftaran Pelamar |
| Keputusan | Planning enam tahap disetujui pengguna melalui percakapan 25 November 2026; rekonsiliasi canonical bila diperlukan |
| Status awal | `PARTIAL / REPAIR / ADD`: kemampuan transaksi existing ditemukan, Journey agregat diusulkan; belum ada UAT live pada audit ini |
| Mandat | Backend dahulu → frontend → integrasi → pengujian live browser dan retest |

**PERINTAH UTAMA:** Implementasikan Recruitment Journey yang benar-benar bekerja, berpusat pada satu lamaran (`ApplicationId`), dengan data dan tindakan nyata dari backend. Jangan berhenti pada analisis, mockup, atau rancangan. Pertahankan struktur sidebar, transaksi existing, Registration Wizard yang telah dibuat, serta batas kewenangan setiap petugas. Jangan membuat database workflow paralel.

## 1. Role

Anda adalah Senior Fullstack Engineer Quilvian V2 (ASP.NET Core/EF Core dan Next.js), Business Process Analyst SDM rumah sakit, UI/UX Engineer dan QA/UAT Engineer. Kerjakan source code di workspace lokal yang diizinkan; jaga keselamatan proses penerimaan tenaga klinis, integritas formasi, privasi kandidat, dan konsistensi transaksi.

### 1.1 Repository dan izin fase

| Peran | Repository | Branch wajib | Snapshot HEAD |
|---|---|---|---|
| Frontend | `DevBenari/QuilvianSystemFrontendDev` | `HamzahV2` | `362eaa2185e1511ef3723bba481cdea77d841ed5` |
| Backend | `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | `ba6671d01ecc87e826ef01065cc7d8a9e36141ce` |
| Governance | `DevBenari/QuilvianEngineeringSkills` | `main` | `62b7ead4595161ec8f9bb7fc2dd96b3a7b29f2a5` |

FE commit 20:32:15 WIB, BE 19:41:17 WIB, 25 November 2026. SHA adalah bukti baseline, bukan perintah reset/checkout. Verifikasi branch dan working tree terbaru sebelum bekerja. Bila HEAD berubah, lakukan impact review singkat terhadap file terkait; jangan menimpa perubahan developer lain.

- `MODULE BLUEPRINT MODE`: hanya jika sinkronisasi keputusan/kontrak/task diperlukan dan diizinkan; tulis terbatas pada blueprint HRD, source read-only.
- `BACKEND MODE`: tulis source BE `MHamzah` pada domain rekrutmen, kontrak agregasi journey, validasi state/permission dan integrasi handoff yang relevan. FE/Skills read-only.
- `FRONTEND MODE`: tulis source FE `HamzahV2` pada halaman detail kandidat, hook, service, constants, style, navigasi kontekstual dan tes rekrutmen yang relevan. BE/Skills read-only.
- `CROSS-REPO MODE` hanya jika otorisasi mode tersebut tersedia; jika tidak, kerjakan fase terpisah dan validasi integrasi tanpa melanggar batas tulis.
- Jangan mengubah repository Engineering Skills. Jangan otomatis commit, push, pull, merge, rebase, reset, deploy, membuat/menjalankan migration, atau menjalankan SQL/akses database langsung. Masing-masing membutuhkan izin terpisah. Mutasi UAT hanya melalui aplikasi/API pada lingkungan pengujian yang diizinkan; tidak menggunakan data atau akun produksi.

## 2. Task

### 2.1 Hasil bisnis dan ruang lingkup

HRD dapat membuka satu kandidat, memilih satu lamaran, mengetahui tahap aktif, hambatan, pekerjaan berikutnya, petugas yang bertanggung jawab dan melakukan aksi sesuai permission hingga kasus onboarding benar-benar tersedia. Setiap tahap dapat dilihat sebagai ringkasan riwayat, tanpa harus menyelesaikan semua proses dalam satu sesi. Journey bukan wizard pengisian linear, dan tidak ada tombol "Selanjutnya" yang mengubah status bisnis secara lokal.

```text
Sumber Daya Manusia
└── Siklus Kepegawaian
    └── Rekrutmen & Seleksi
        ├── Permintaan Requisition                 (existing)
        ├── Lowongan                              (existing)
        ├── Pelamar                               (existing)
        │   ├── Daftar Pelamar
        │   ├── Multi-Step Pendaftaran Pelamar
        │   └── Detail Pelamar
        │       └── Recruitment Journey (FITUR, bukan item sidebar)
        ├── Seleksi Administrasi                  (existing)
        ├── Tes/Wawancara                         (existing)
        ├── Surat Penawaran (Offer)                (existing)
        ├── Pemantauan MCU Pra-Kerja               (existing)
        └── Talent Pool Cadangan                   (existing)

Penerimaan & Onboarding → Onboarding / Kasus Onboarding (existing)
```

Batas: satu feature lintas alur pada Menu 03, bukan mandat refactor atau finishing semua fitur rekrutmen. Lima subbagian target registry di bawah dipetakan agar tidak ada dependency terputus; perubahan hanya yang diperlukan Journey. Pertahankan semua delapan submenu navigasi terbaru. Onboarding Wizard penuh, portal pelamar eksternal, perubahan kebijakan HRD, desain ulang master data, payroll, dan pemindahan menu tidak termasuk.

### 2.2 Keputusan yang sudah dikunci

ID `RJ-DEC-*` adalah referensi lokal planning, bukan nomor keputusan canonical yang sudah terdaftar. Jangan mengalokasikan nomor `HRD-DEC` secara sepihak.

| ID | Keputusan |
|---|---|
| RJ-DEC-01 | Journey menjadi bagian Detail Pelamar; tidak ada submenu sidebar tambahan. |
| RJ-DEC-02 | Enam tahap: Pendaftaran, Seleksi Administrasi, Tes & Wawancara, Surat Penawaran, MCU Pra-Kerja, Serah Terima. |
| RJ-DEC-03 | Satu journey per `ApplicationId`; kandidat dengan banyak lamaran tidak berbagi status/aksi antar lamaran. |
| RJ-DEC-04 | Stepper horizontal, konten tahap aktif, side section tindakan/hambatan. Riwayat hanya dari evidence backend. |
| RJ-DEC-05 | Transisi dan kelulusan ditentukan backend. Tahap lain dapat dilihat sebagai riwayat/ringkasan tanpa membuka aksi yang belum sah. |
| RJ-DEC-06 | Reuse halaman kerja, service, DTO, approval, upload, dan entity existing; tambah agregasi baca yang terarah. Tidak membuat duplikasi transaksi. |
| RJ-DEC-07 | Otorisasi per tindakan, scope objek, masking medis dan kompensasi. HRD tidak dapat membypass asesor, penyetuju atau petugas MCU. |
| RJ-DEC-08 | Pendaftaran existing tetap dapat dilanjutkan; handoff hanya dianggap berhasil sampai kasus onboarding terkonfirmasi. |
| RJ-DEC-09 | Tangani jalur ditolak, dibatalkan, offer kedaluwarsa, talent pool dan status yang belum bisa dipastikan; bukan selalu enam langkah sukses. |
| RJ-DEC-10 | Reuse database existing, tanpa migration baru kecuali terbukti perlu dan ada izin terpisah. |

Canonical yang wajib dipertahankan: `HRD-DEC-073` (requisition-driven), `HRD-DEC-076` (penapisan klinis), `HRD-DEC-077` (jalur penilai independen dan veto), `HRD-DEC-078` (conditional offer diterima sebelum MCU dan privasi), `HRD-DEC-079` (tier approval), `HRD-DEC-080` (talent pool), `HRD-DEC-241` dan `HRD-ASM-98` (scope asesor), `HRD-DEC-244` (daftar seleksi per lamaran, bukan per kandidat), `HRD-ASM-96` (UserInterview pada jalur klinis/teknis). Baca versi canonical terbaru beserta supersession sebelum merubah state/contract. Approval chat ini tidak mengesahkan kebijakan hukum/klinis baru.

### Submenu 1 - Lowongan

- Tujuan: menghubungkan lamaran dengan lowongan, jabatan, unit, kategori klinis/nonklinis, persyaratan dan formasi yang benar; pelaku recruiter berizin.
- Reuse route `/hr/recruitment/vacancies` dan `/hr/recruitment/vacancies/[id]`, `RecruitmentService.getVacancy`, `JobVacancyController`, `JobVacancyDtos.cs`. Nama fungsi FE aktual diperiksa saat preflight, jangan menambah service duplikat bila sudah ada.
- Input adalah `VacancyId` yang berasal dari lamaran sah, bukan field bebas yang bisa diubah dalam Journey. Ambil unit/jabatan melalui relasi requisition/lowongan. Tampilkan judul, nomor, kategori, status lowongan dan tanggal batas bila tersedia. Jangan menebak field yang tidak ada di DTO.
- Backend: periksa integritas relasi application → vacancy → requisition, soft delete dan scope RS; lowongan sudah ditutup tidak menghapus histori lamaran. Pendaftaran baru tetap mengikuti validasi Published dan tanggal tutup; tindakan lanjutan lamaran existing mengikuti kontrak, bukan otomatis ditolak karena lowongan ditutup.
- FE: tombol lihat lowongan bila berizin; shortcut pendaftaran dari lowongan existing dipertahankan. Jangan mengubah alur requisition/kuota di luar guard yang dibutuhkan.
- Acceptance/UAT: dua lamaran kandidat pada lowongan berbeda menampilkan identitas lowongan yang benar (RJ-UAT-04/05); link lowongan tidak membocorkan data yang tidak berizin (RJ-UAT-02); pendaftaran existing tetap berjalan (RJ-UAT-07).
- Status sumber: `IMPLEMENTED_NOT_VERIFIED` untuk lowongan existing; perubahan konteks Journey `ADD`.

### Submenu 2 - Pelamar

- Tujuan: satu identitas kandidat, banyak lamaran; HRD membuka detail dan memilih lamaran tanpa membuat ulang profil.
- Reuse `/hr/recruitment/candidates`, `/hr/recruitment/candidates/create`, `/hr/recruitment/candidates/[id]`, `CandidateRegistrationWizardView`, `CandidateDetailScreeningView`, `CandidateController`, `CandidateService`, `CandidateDtos`, serta route helper `RECRUITMENT_ROUTES.candidateResume`.
- Input: `candidateId` dari route; `applicationId` dari pilihan yang sah/URL yang tervalidasi. Identitas kandidat dipakai bersama; stage, evaluasi, offer, MCU, handoff selalu per application. Jika belum ada lamaran, tampilkan empty state + Lanjutkan Pendaftaran hanya bagi pengguna berizin. Jangan membuat application sementara secara otomatis.
- Backend: implementasikan GET agregat Journey (kontrak §3.3); data berasal dari entity existing. Pastikan application benar-benar milik kandidat yang sedang ditampilkan dan berada dalam scope akses. Jangan mencampur nama yang sama, ID offer berbeda atau dokumen dari aplikasi lain. Identitas kandidat dan dokumen umum dapat ditampilkan menurut aturan akses, dokumen spesifik lamaran harus terikat benar.
- FE: tambahkan pemilih lamaran dengan nomor, lowongan, tanggal/status. Jika satu lamaran, dapat otomatis dipilih; jika banyak dan belum dipilih, gunakan pemilihan eksplisit; bila deep-link valid, buka lamaran terkait. Jangan memilih lamaran berdasarkan nama atau sekadar baris pertama tanpa konteks. Simpan konteks pilihan melalui route query non-sensitif; jangan taruh NIK, nominal atau hasil medis di URL/storage browser.
- FE: pertahankan profil, daftar lamaran dan dokumen existing melalui komposisi detail/tabs yang konsisten. Journey menjadi panel utama untuk lamaran terpilih; gunakan button/action dari stage terpilih, bukan semua form menjulur ke bawah. Setelah aksi atau kembali dari halaman kerja, refetch data server untuk application yang sama. Kelola request race/abort saat berpindah lamaran.
- Acceptance/UAT: tidak ada duplikasi kandidat/lamaran; pendaftaran yang sudah dibuat dapat diteruskan tanpa reset; switch A/B tidak mencampurkan status; refresh mempertahankan konteks (RJ-UAT-03–08, 26–28).
- Status: wizard route terlihat pada baseline terbaru (`IMPLEMENTED_NOT_VERIFIED`); Journey `ADD`, navigasi kontekstual `REPAIR`.

### Submenu 3 - Seleksi Administrasi

- Tujuan: recruiter memeriksa dokumen dan memutuskan penapisan per lamaran, bukan per kandidat. Input `ApplicationId`, data kandidat, kategori lowongan, dokumen yang berlaku, bukti dan catatan sesuai kontrak.
- Reuse `/hr/recruitment/screenings`, `CandidateDetailScreeningView`, `CandidateScreeningEvaluationFields`, `CandidateScreeningController`, `CandidateScreeningService`, DTO screening. Gunakan fungsi existing Mulai Seleksi dan Nilai Berkas, bukan form duplikat atau set status frontend.
- Backend: `CandidateScreening.Read` untuk pembacaan dan permission mutasi existing sesuai controller; pastikan pelaku dan relasi application benar. Pada penolakan administrasi, StageStatus `REJECTED_ADMINISTRATION` harus dikenali meski CurrentStage masih Screening. Lolos hanya berdasarkan hasil screening persisten.
- STR klinis: source terbaru mengurutkan dokumen terbaru lebih dulu; uji relevansi dokumen yang terpilih terhadap lamaran/profesi dan aturan `HRD-DEC-076`. Jangan menggugurkan atau meluluskan berdasarkan file upload semata. Jaga jalur pemeriksaan manual surat proses perpanjangan yang approved; jangan mengarang kebijakan STR/SIP. Bila implementasi canonical berbeda, perbaiki guard yang sudah disetujui atau tandai bagian ambigu BLOCKED.
- FE: tampilkan daftar/kelengkapan dokumen, jenis klinis, status penapisan, waktu dan catatan yang boleh dibaca. Sediakan tindakan Mulai Seleksi/Nilai Berkas bila tersedia dari backend, atau link ke layar kerja existing dengan konteks application. Pertahankan daftar penapisan semua lamaran dan filter existing sesuai `HRD-DEC-244`.
- Acceptance/UAT: pending, passed, rejected, STR hilang/kedaluwarsa, nonklinis, beberapa lamaran dan izin baca/tindak terbukti; gagal screening mengunci downstream (RJ-UAT-09–12, 17).
- Status: `IMPLEMENTED_NOT_VERIFIED`; integrasi status dan scope data `REPAIR/ADD`.

### Submenu 4 - Tes/ Wawancara

- Tujuan: melihat pekerjaan wawancara HR dan asesmen klinis/teknis yang benar-benar ditugaskan, berikut jadwal, hasil, rekomendasi, dan hambatan.
- Reuse `/hr/recruitment/evaluations`, `/hr/recruitment/evaluations/hr/[id]`, `/hr/recruitment/evaluations/clinical/[id]`, `CandidateHrEvaluationView`, `CandidateClinicalExamView`, `CandidateEvaluationController`, `CandidateEvaluationService` dan DTO. Route existing memakai konteks kandidat; tambah query atau kontrak navigasi application yang tervalidasi tanpa mengganti arti `[id]` diam-diam.
- Backend: wajib memastikan screening lulus sebelum evaluasi; evaluasi klinis menggunakan dua jalur independen yang harus selesai sesuai `HRD-DEC-077`, termasuk `UserInterview` sesuai `HRD-ASM-96`. Jalur nonklinis tidak memaksa STR/asesmen klinis; pertahankan penilaian teknis jika memang dipersyaratkan canonical. Jangan mengasumsikan urutan HR dan klinis wajib serial; kedua jalur dapat berjalan independen sesuai kontrak.
- Audit `CheckAndAdvanceApplicationStageAsync`: baris evaluasi yang baru dijadwalkan tidak berarti evaluasi selesai. Periksa indikator penyelesaian/pengiriman hasil yang benar, rekomendasi valid, persyaratan dan veto. Jangan menganggap `Considered` otomatis memenuhi syarat bila canonical belum menetapkannya. Perkuat transisi dan guard penawaran berdasarkan hasil sah, bukan semata tidak ada status gagal.
- Security: asesor hanya melihat/mengirim nilai untuk evaluasi yang ditugaskan kepadanya kecuali hak baca menyeluruh yang sah; `ViewAll` tidak otomatis memberi hak mengirim nilai pihak lain. Clinical veto `NotRecommended` memblokir offer; HR tidak memiliki aksi override. Validasi langsung di API, bukan hanya hide button.
- FE: tampilkan dua subpanel ringkas sesuai kebutuhan jabatan, jadwal, status penilai (sesuai akses), rekomendasi, dan tombol jadwalkan/isi/lihat jika berhak. Jika link membuka halaman evaluasi, pilih application/evaluation ID yang benar dan kembali ke Journey setelah mutasi.
- Acceptance/UAT: jadwal saja tidak meluluskan; klinis wajib memenuhi dua jalur yang diwajibkan, penolakan/veto memblokir offer, asesor tak berwenang ditolak, dan A/B tidak tercampur (RJ-UAT-13–18).
- Status: `IMPLEMENTED_NOT_VERIFIED`; guard dan deep link `REPAIR`.

### Submenu 5 - Onboarding

- Pemetaan navigasi terbaru: `Penerimaan & Onboarding → Onboarding` (`/hr/recruitment/handoffs`) dan `Kasus Onboarding` (`/hr/lifecycle/onboarding-cases`, detail `[id]`). Registry lama menempatkan Onboarding di Menu 03; jangan pindah kembali sidebar. Ruang lingkup hanya serah terima dan tautan kasus, bukan Onboarding Wizard penuh.
- Tujuan: menyerahkan kandidat yang memenuhi prasyarat offer/MCU dan formasi tanpa membuat identitas pegawai kedua. Aktor HRD dengan `HiringHandoff:Execute`; pembacaan `HiringHandoff:Read`, detail kasus mengikuti permission lifecycle sendiri.
- Reuse `HiringHandoffController`, `HiringHandoffService.GenerateHandoffAsync`, `ConfirmTransferAsync`, `HrdOnboardingCaseService.CreateFromHandoffAsync` serta service/route existing. Input `OfferId` milik application yang dipilih; output handoff ID, workforce profile reference dan case ID bila benar-benar ada.
- Backend: verifikasi offer diterima, clearance yang sah dan pembatasan kerja bila berlaku, formasi tersedia sesuai existing, tidak ada duplikasi handoff/profil/kuota. Source saat ini dapat menyimpan handoff sebelum pembuatan kasus dan mencatat kegagalan kasus di notes. Perbaiki pemulihan/atomicity sejauh pattern governance mengizinkan; jangan menggandakan profil atau memakan kursi lagi saat retry. Gunakan state/link yang sudah persisten, bukan flag sukses lokal. Jika transaksi gabungan tidak dapat dijamin, gunakan mekanisme recovery idempotent yang sah dan status parsial eksplisit; jangan menambah schema tanpa izin.
- FE: tampilkan ringkasan prasyarat, tombol serah terima/konfirmasi sesuai backend, status handoff dan kesiapan kasus. Jika kasus belum tersedia, beri status "Serah terima tercatat — kasus onboarding belum tersedia" dan aksi pemulihan sah atau instruksi petugas berwenang. Tombol Lanjutkan ke Onboarding hanya jika case ID terverifikasi, scope izin benar, route detail valid. Status Hired bukan siap kerja atau izin klinis.
- Acceptance/UAT: conditional offer belum diterima/MCU belum memenuhi syarat ditolak; handoff valid membuat tepat satu relasi; kegagalan kasus bukan sukses palsu; retry aman, tautan ke kasus benar; pegawai belum aktif kerja sebelum lifecycle existing mengesahkan (RJ-UAT-21–25, 29).
- Status: `IMPLEMENTED_NOT_VERIFIED`; pemulihan/konfirmasi integrasi `REPAIR`.

### 2.3 Capability existing tambahan yang wajib terhubung

| Submenu navigasi | Peran dalam Journey | Scope |
|---|---|---|
| Permintaan Requisition | Sumber formasi/headcount | Tampilkan relasi yang relevan, jangan ubah kebijakan approval/headcount. |
| Surat Penawaran (Offer) | Tahap 04, draft–approval–kirim–respons | Reuse offer dan approval existing, perkuat guard dan scope. |
| Pemantauan MCU Pra-Kerja | Tahap 05, kelayakan kerja | Reuse layanan MCU, perbaiki guard penerimaan conditional offer dan privacy. |
| Talent Pool Cadangan | Jalur alternatif kandidat yang memenuhi syarat namun tidak terpilih/kuota | Tampilkan status/catatan yang terbukti; tautkan aksi existing bila sah, jangan membuat tahap wajib ketujuh. |

**Offer:** pakai `JobOfferService`, `JobOfferApprovalController`, `JobOfferController`. Pastikan screening dan evaluasi yang diwajibkan benar-benar selesai dan memenuhi syarat, tidak ada clinical veto, tidak ada offer aktif duplikat, serta guard kuota/formasi sesuai kontrak. Approval tier standar dan eskalasi mengikuti `HRD-DEC-079`; jangan approve dari UI dengan role tidak sah. Draft, pending approval, approved, sent conditional, accepted, declined, revoked, expired dan final harus dibedakan. `Approved` bukan bukti kandidat menerima tawaran. Jangan expose gaji/tunjangan kepada user tanpa izin yang berlaku. Tautkan detail offer berdasarkan offer ID milik application, bukan hasil search nama.

**MCU:** `McuClearanceService.RegisterMcuAsync` pada snapshot menerima `ConditionalAccepted` atau `SentConditional`. Ini bertentangan dengan canonical `HRD-DEC-078` yang mensyaratkan persetujuan kandidat terlebih dahulu. Perbaiki guard server agar tawaran yang baru dikirim tidak cukup. Verifikasi juga jalur `CandidateRespondAsync` yang pada snapshot menerima status `Approved`; jangan menganggap penawaran belum dikirim sebagai respons kandidat yang sah tanpa bukti/kontrak. Uji langsung melalui API, bukan sekadar tombol disabled.

`RecordResultAsync` snapshot menaikkan offer ke `FinalAccepted` untuk Fit/FitWithRestrictions; jangan menampilkan fakta bahwa kandidat menerima offer final apabila hanya ada hasil MCU tanpa bukti respons. Keputusan canonical memang mengatur penawaran definitif setelah FIT dan konfirmasi penyesuaian kerja untuk FitWithRestrictions; rekonsiliasi bukti dan status UI dengan state actual. Canonical memisahkan TEMPORARILY_UNFIT dan PERMANENTLY_UNFIT, sementara enum FE/source yang dibaca menggunakan `Pending`, `Fit`, `FitWithRestrictions`, `Unfit`. Jangan mengarang mapping Unfit menjadi sementara/permanen; pertahankan alur existing yang sah dan catat mismatch `NEEDS_SYNC`. Jika perubahan enum/persistence/kebijakan butuh approval atau migration, tahan hanya jalur terdampak; tetap selesaikan agregasi dan UI yang aman. FitWithRestrictions tidak otomatis siap handoff tanpa konfirmasi penyesuaian kerja menurut canonical. Bila bukti konfirmasi belum tersedia, tandai blocker, jangan menciptakan persetujuan dummy.

**Talent pool:** status alternatif, bukan tanda gagal/sukses global. Gunakan aturan `HRD-DEC-080` dan service existing; tampilkan retensi/expired/fast-track hanya jika data dan izin mendukung. Jangan mempromosikan kandidat otomatis dari Journey atau melewati penawaran dan MCU. Untuk beberapa offer historis, pilih offer relevan secara deterministik sesuai relasi/status; jika ada konflik beberapa record aktif, tampilkan hambatan dan minta koreksi sesuai workflow, bukan memilih sembarang baris.

### 2.4 Prinsip data dan alur bisnis

```text
Candidate (1 identitas)
   ├── Application A → Vacancy A → Screening A → Evaluations A
   │                              → Offer A → MCU A → Handoff A → Case A
   └── Application B → Vacancy B → Screening B → Evaluations B
                                  → Offer B → MCU B → Handoff B → Case B
```

- `ApplicationId` menjadi kunci journey. Candidate-level identity tidak digunakan sebagai bukti status lamaran. Jika kandidat status Hired/Rejected/TalentPool global berbeda dari application tertentu, tampilkan terpisah dan prioritaskan evidence lamaran untuk journey.
- Enam tahap adalah kelompok visual, bukan enum baru. Stage Pendaftaran selesai ketika application persisten tercatat; kandidat hanya Registered tanpa application tidak memiliki journey lamaran. Status lokal stepper bukan sumber keputusan.
- Tes HR dan klinis/teknis dapat paralel; penawaran/approval dan MCU memiliki substatus; Talent Pool dan terminal outcome adalah cabang. Bukan workflow bebas diklik untuk maju.
- Tidak ada manipulasi `CurrentStage`, `StageStatus`, `CandidateStatus`, approval, clearance atau status handoff dari frontend tanpa endpoint bisnis yang sah. GET journey murni baca, tidak auto-heal, tidak memicu approval, tidak menulis state.
- Gunakan transaksi, audit aktor dan waktu, concurrency/idempotency sesuai governance. Validasi kondisi saat mutasi ulang di server; optimistic UI tidak boleh mendahului keberhasilan. Jika schema perlu diubah, jangan migration tanpa izin.

## 3. Context

### 3.1 Evidence baseline yang telah dibaca

Prefix FE: [QuilvianSystemFrontendDev @ 362eaa2](https://github.com/DevBenari/QuilvianSystemFrontendDev/tree/362eaa2185e1511ef3723bba481cdea77d841ed5).
Prefix BE: [NewQuilvianSystemBackend @ ba6671d](https://github.com/DevBenari/NewQuilvianSystemBackend/tree/ba6671d01ecc87e826ef01065cc7d8a9e36141ce).
Governance: [QuilvianEngineeringSkills @ 62b7ead](https://github.com/DevBenari/QuilvianEngineeringSkills/tree/62b7ead4595161ec8f9bb7fc2dd96b3a7b29f2a5).

| Evidence FE (path relatif) | Temuan |
|---|---|
| `src/app/hr/recruitment/candidates/create/page.jsx` | Memanggil `CandidateRegistrationWizardView`; bukan bukti UAT lulus. |
| `src/components/view/hr/recruitment/candidate-detail-screening-view.jsx` | Detail kandidat mempunyai tabel lamaran, dokumen, screening dan link evaluasi. Screening diambil berdasarkan nama dengan page size 50, lalu difilter menurut application ID. Potensi data tidak ditemukan karena paging. Link evaluasi menggunakan kandidat, perlu konteks application. |
| `src/lib/constants/hr/recruitment/recruitment-constants.js` | Route helper create/resume/detail, screening/evaluasi/offer/MCU/handoff; map status existing. |
| `src/lib/services/hr/recruitment-service.js` | Pemanggilan API transaksi rekrutmen yang dapat digunakan ulang. |
| `src/app/hr/recruitment/evaluations/hr/[id]/page.jsx`, `clinical/[id]/page.jsx` | Route memanggil view wawancara HR dan klinis existing. |

Prefix backend rekrutmen: `Areas/Corporate/HumanResource/RecruitmentManagement/`.

| Evidence BE | Temuan / tindak lanjut |
|---|---|
| `Enums/ApplicationStage.cs` | Screening, InterviewHr, ClinicalAssessment, Offering, MedicalCheckup, Handoff, TalentPool, Rejected, Withdrawn. Bukan enam tahap UI literal. |
| `DTOs/CandidateDtos.cs` | CandidateResponse memuat applications dan documents; per application ada VacancyId, CurrentStage, StageStatus; dokumen dapat memiliki ApplicationId. |
| `DTOs/CandidateScreeningDtos.cs`, `CandidateEvaluationDtos.cs`, `JobOfferDtos.cs`, `McuClearanceDtos.cs`, `HiringHandoffDtos.cs` | Relasi ApplicationId / OfferId; tidak semua query list dapat langsung difilter per application; hindari N+1/search nama. |
| `Services/CandidateScreeningService.cs` sekitar 185–285 | Dokumen diurutkan terbaru; rejected bisa hanya mengubah StageStatus; passed ke InterviewHr. |
| `Services/CandidateEvaluationService.cs` sekitar 193–410 | Penjadwalan, submit HR/clinical, scope penilai, veto dan check advance. Uji apakah baris terjadwal bisa dianggap selesai. |
| `Services/JobOfferService.cs` sekitar 115–196, 315–430 | Create memeriksa gagal/veto dan duplicate offer; belum terlihat guard positif kelulusan seluruh evaluasi pada excerpt. Send memerlukan Approved; respons menerima SentConditional, FinalOfferSent atau Approved. |
| `Services/McuClearanceService.cs` sekitar 160–299 | Register menerima ConditionalAccepted/SentConditional; hasil Unfit revoke, Fit/FitWithRestrictions menaikkan status offer. |
| `Services/HiringHandoffService.cs` sekitar 123–303 | Generate menyimpan handoff/profile/formasi dan mencoba create onboarding case; kegagalan kasus bisa dicatat setelah save; perlu recovery yang aman. |
| `Controllers/CandidateEvaluationController.cs`, `JobOfferController.cs`, `McuClearanceController.cs`, `HiringHandoffController.cs` | Permission per aksi berbeda; agregasi tidak boleh menjadi jalan pintas bypass. |

Referensi keputusan: `docs/module-blueprints/human-resource/00-interview-decisions.md` rev 44 (baseline BE; §33.5/sekitar 4839–4907 dan §57.8/sekitar 9960 dst), `flowcharts/rekrutmen-dan-seleksi.md` (S-D2, berstatus draft tetapi merujuk keputusan approved), `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`, serta contract S-D2 versi aktif. Baca task/report yang relevan, jangan menganggap status roadmap sebagai bukti runtime. Referensi penulisan instruksi Menu 06/12 telah dibaca; bukan sumber keputusan rekrutmen.

Semua temuan adalah bukti pembacaan source dan keputusan; status runtime `NOT VERIFIED`. Nama endpoint/field baru di bawah adalah target implementasi, bukan klaim telah tersedia.

### 3.2 Task register FIN (usulan ID lokal, rekonsiliasi dengan roadmap resmi)

| ID | Prioritas | Fase dan hasil |
|---|---|---|
| FIN-HRD-REC-JRN-01 | P0 | Backend — agregasi read-only per application, relasi, state projection, permission dan error contract. |
| FIN-HRD-REC-JRN-02 | P0 | Backend — validasi transisi screening/evaluation/offer/MCU, veto, status akhir, akses penilai, guard privasi. |
| FIN-HRD-REC-JRN-03 | P0 | Backend — integrasi handoff/case, recovery/idempotency, tidak ada false success atau duplikasi profil/formasi. |
| FIN-HRD-REC-JRN-04 | P1 | Frontend — detail kandidat, selector lamaran, stepper, enam panel, side section, responsive/base components. |
| FIN-HRD-REC-JRN-05 | P1 | Frontend — reuse aksi existing dan deep link kontekstual, re-fetch, resume, terminal/alternatif, error/permission. |
| FIN-HRD-REC-JRN-06 | P0–P2 | QA — API/contract test, regression, UAT live browser lintas peran, perbaikan dan retest, pelaporan. |

Dependency: 01/02 terlebih dahulu; 03 sebelum aksi handoff dianggap siap; 04–05 konsumsi kontrak backend yang stabil; 06 setelah fungsi tersedia. Sinkronisasi canonical/task-ID hanya dalam wewenang resmi, jangan membuat ID duplikat. Label FIN ini tidak otomatis berarti task terdaftar atau DONE.

### 3.3 Kontrak agregasi backend (usulan yang disetujui untuk implementasi)

```http
GET /api/v1/corporate/human-resource/recruitment/applications/{applicationId}/journey
```

Implementasikan controller/DTO/service/query dengan arsitektur canonical repository (bukan DbContext langsung dalam controller untuk kode baru). Gunakan response wrapper existing, auth dan scope rumah sakit/organisasi yang berlaku. `Candidate:Read` dapat menjadi prasyarat membaca identitas; tetapkan resource/aksi endpoint menurut registry resmi dan kontrak keamanan, bukan membuat permission fiktif. Read kandidat tidak otomatis memberi read offer/MCU/evaluasi. Mask field atau section sesuai izin/assignment; validasi scope server-side untuk setiap resource. Jika permission tidak ada, jangan memperluas hak secara otomatis. Gunakan 401/403/404 dan error mapping sesuai kontrak; jangan memberi sinyal yang membocorkan keberadaan data terlarang.

**Bentuk logis respons target** (nama/tipe final mengikuti convention DTO; bukan payload API existing):

| Bagian | Isi minimum |
|---|---|
| `applicationId`, `candidateId`, `applicationNumber` | Identitas lamaran persisten, ownership valid. |
| `candidateSummary` | Nama/kode/identitas ringkas sesuai izin; minimalkan PII. |
| `vacancySummary` | VacancyId, title, nomor, kategori, unit/jabatan bila tersedia dari relasi. |
| `currentStage`, `stageStatus`, `candidateStatus` | State raw dari server; status kandidat dan lamaran terpisah. |
| `journeyOutcome` | Active / Rejected / Withdrawn / TalentPool / HandoffPending / HandoffCompleted / Unknown, sebagai projection; bukan enum DB baru. Sesuaikan nama final. |
| `stages[]` | Enam item berurutan tetap: key, label, status tampilan, alasan/status detail, record IDs, tanggal/aktor bila diizinkan, data ringkas sesuai tahap. |
| `allowedActions[]` | Kode aksi, tahap, reference ID yang benar, allowed/denied dan alasan yang aman. Data ini membantu UI, bukan pengganti authorization server. |
| `blockers[]`, `nextAction` | Hambatan nyata/prasyarat yang tidak terpenuhi; role/tindakan berikutnya berdasarkan evidence; jangan bocorkan data restricted. |
| `history[]` | Event/record yang sungguh tersedia dan timestamp valid, tanpa event buatan. Jika histori lengkap belum tersedia, jelaskan keterbatasan. |
| `handoffReference`, `onboardingCaseReference` | Handoff ID dan case ID hanya bila ada/berizin; status readiness yang nyata. |
| `dataCompleteness` | Indikator section terlarang, belum tersedia, konflik, atau gagal dibaca; tidak mengubah error menjadi "belum dimulai". |

Stage status visual: `COMPLETED`, `IN_PROGRESS`, `WAITING`, `BLOCKED`, `REJECTED`, `NOT_APPLICABLE`, `RESTRICTED`, `UNKNOWN` (nama final boleh diselaraskan). Seluruhnya derived/projection, tidak disimpan sebagai state baru. Bedakan waiting karena belum dikerjakan, blocked karena prasyarat gagal, restricted karena izin, dan unknown karena data error/konflik. Tahap klinis yang tidak diwajibkan tidak ditandai pending. Progress bila ditampilkan = jumlah tahap relevan yang selesai berdasarkan server / jumlah tahap relevan; jangan menghitung langkah yang user klik atau menyamakan persentase dengan kelulusan. Bila evidence tidak lengkap, tampilkan status alih-alih persentase menipu.

**Pemetaan state minimal yang harus diuji:**

| Tahap | Evidence penyelesaian / status penting | Tindakan berikutnya |
|---|---|---|
| Pendaftaran | Application benar-benar ada; status kandidat saja tidak cukup. | Resume wizard bila belum ada lamaran/berkas. |
| Seleksi Administrasi | Screening Passed atau hasil sah; `REJECTED_ADMINISTRATION` terminal untuk jalur itu walau CurrentStage belum berubah. | Mulai/Nilai Berkas jika allowed; jangan bypass penolakan. |
| Tes & Wawancara | Hasil evaluasi yang benar-benar disubmit dan memenuhi seluruh syarat; `ASSESSMENT_FAILED`/veto memblokir. | Schedule/submit/read sesuai evaluator. |
| Penawaran | Riwayat approval/offer dan respons kandidat sah, bukan draft/Approved/SentConditional semata. | Buat/lihat/approve/kirim/catat respons sesuai izin. |
| MCU | Clearance aktual, konfirmasi pembatasan kerja bila wajib; Unfit/konflik harus teridentifikasi. | Rujuk/catat hasil oleh petugas sah. |
| Serah Terima | Handoff sah + kasus onboarding benar-benar tersedia untuk penyelesaian journey. Handoff tanpa case adalah partial/pending. | Generate/confirm/recovery sesuai kontrak; buka kasus jika berizin. |

Jangan membuat asumsi status masa lalu dari urutan angka enum; baca evidence setiap domain. Prioritaskan terminal outcome dan veto/rejection; status raw yang bertentangan harus terlihat sebagai konflik yang perlu diperbaiki, bukan badge sukses. Cabang TalentPool, offer declined/expired/revoked dan withdrawn mengikuti canonical; tidak menampilkan tombol lanjut jika aksi tidak sah. Jika suatu status belum dipetakan, fail closed untuk aksi mutasi dan laporkan unknown.

Agregasi harus query terarah berdasarkan application/offer ID dan scope, projection field minimum, paginated/bounded history bila diperlukan, cancellation token, tanpa N+1 query per baris atau mengambil semua kandidat untuk filter nama. Baca model/relasi EF dan indeks yang sudah ada; jangan menambahkan indeks/migration tanpa izin. Gunakan mekanisme izin/row access existing dan uji potensi IDOR, termasuk applicationId tebakan. GET tidak menyimpan, tidak mengubah status atau membuat event.

### 3.4 API existing yang harus dipakai ulang

Base: `/api/v1/corporate/human-resource/recruitment`. Path relatif tabel berikut adalah endpoint yang ditemukan pada service/controller; Agent cocokkan DTO, izin dan status aktual sebelum memanggil. Jangan buat endpoint mutasi generik "advance stage".

| Method + path | Fungsi dan batas izin |
|---|---|
| GET `/candidates/{id}` | Candidate Read; profil dan daftar lamaran. |
| GET `/vacancies/{id}` | JobVacancy Read sesuai scope. |
| GET `/screenings`, GET `/screenings/{id}` | CandidateScreening Read. |
| POST `/screenings`, POST `/screenings/evaluate` | Permission screening existing; initiate/evaluate dengan ApplicationId. |
| GET `/evaluations`, GET `/evaluations/{id}` | CandidateEvaluation Read, scope asesor/ViewAll. |
| POST `/evaluations/schedule` | CandidateEvaluation Evaluate. |
| POST `/evaluations/{id}/submit-hr` | CandidateEvaluation Evaluate, asesor/aktor sah. |
| POST `/evaluations/{id}/submit-clinical` | CandidateEvaluation EvaluateClinical, asesor/aktor sah. |
| GET `/offers`, GET `/offers/{id}` | JobOffer Read, masking kompensasi. |
| POST `/offers`, PUT `/offers/{id}` | JobOffer Create/Update; guard dan concurrency. |
| GET `/offers/{id}/approvals`, POST `/offers/{id}/approvals/approve`, POST `/offers/{id}/approvals/reject` | Permission approval existing dan assigned approver; jangan bypass engine. |
| POST `/offers/{id}/send-conditional`, POST `/offers/{id}/candidate-respond` | JobOffer Update, validasi status. |
| GET `/mcu-clearance`, GET `/mcu-clearance/{id}` | OccupationalHealthClearance Read, field-level masking. |
| POST `/mcu-clearance/register` | OccupationalHealthClearance Create. |
| POST `/mcu-clearance/{id}/record-result` | OccupationalHealthClearance Evaluate; petugas sah. |
| GET `/talent-pool`, GET `/talent-pool/{id}` | TalentPool Read; aksi enroll/fast-track hanya jika izin dan prasyarat benar. |
| GET `/handoffs`, GET `/handoffs/{id}` | HiringHandoff Read. |
| POST `/handoffs/generate`, POST `/handoffs/{id}/confirm-transfer` | HiringHandoff Execute, validasi offer dan clearance. |

Lifecycle case: GET `/api/v1/corporate/human-resource/onboarding-cases/{id}` sesuai contract existing dan permission `WorkforceOnboardingCase:Read`. Jangan mengambil detail kasus lewat endpoint baru yang melewati hak akses. Semua submit melalui endpoint pemilik transaksi; jika agregasi memakai helper domain existing, jangan memanggil endpoint via HTTP internal yang berisiko bypass scope.

### 3.5 Keamanan dan konsistensi

- Least privilege: bedakan boleh membaca ringkasan perjalanan, boleh membaca detail section, boleh melakukan aksi. Jangan mengirim field tersembunyi ke browser lalu hanya disembunyikan CSS.
- MCU: hanya kesimpulan kelayakan dan informasi fungsional yang sah untuk HRD/atasan; diagnosis, lab, doctor notes/sertifikat rahasia hanya petugas berhak sesuai canonical. Jangan expose di response, history, tooltip, toast, export, log, screenshot atau URL. Gunakan masking service existing; jangan reimplementasikan kebijakan baru.
- Kompensasi: nominal offer/gaji/tunjangan dan approval sensitif mengikuti permission existing yang terbukti; tidak tersedia bila kontrak izin belum jelas.
- Audit: aktor dan timestamp dari sesi/record; jangan percaya ID aktor dari client. Pakai concurrency token bila DTO menyediakan. Saat conflict, refetch dan minta user meninjau ulang; jangan retry mutation buta.
- Handoff: uji retry setelah respons hilang, partial failure, dan concurrent requests. Gunakan relasi/constraint existing dan transaksi/recovery yang sah. Jika diperlukan perubahan schema, status `BLOCKED_SCHEMA` dengan alasan dan bukti; jangan membuat data palsu untuk mengklaim selesai.

## 4. Reasoning / Execution Workflow

### 4.1 Preflight dan governance

1. Verifikasi branch, SHA, working tree, dan mode/target tulis. Baca `AGENTS.md`, `CLAUDE.md` FE/BE, `agents/rules/GLOBAL_RULES.md` dan `agents/rules/README.md` di Engineering Skills. Governance canonical wajib, tidak bergantung pada pemakaian skill. `CLAUDE.md` terbaru menyatakan runtime skill opsional secara umum; instruksi ini **secara eksplisit meminta** registered runtime `quilvian-engineering-skills:build-module-backend` untuk fase BE dan `quilvian-engineering-skills:build-module-frontend` untuk fase FE. Panggil sesuai aturan runtime nyata; jangan klaim invokasi palsu. Jika skill yang diminta tidak tersedia, laporkan `BLOCKED — Quilvian runtime skill unavailable` pada fase terdampak dan minta jalur sah, tidak melewati governance. Jika canonical rule sendiri tak terbaca, `BLOCKED — canonical governance unavailable`.
2. Baca `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dan rules backend relevan (`TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `DATABASE_RULES`, `REVIEW_RULES`, `TEST_POLICY`, `CROSS_REPO_RULES`). Terapkan QBE/legacy ratchet; jangan meniru counter/arsitektur legacy pada kode baru.
3. FE baca aturan `frontend-architecture`, `base-component-catalog`, `base-component-decision-gate`, `page-composition-patterns`, `design-tokens`, `ui-consistency-checklist`, `test-policy`; baca aturan output/report. Jangan pakai salinan rules lama di repo aplikasi.
4. Cocokkan canonical decision/flowchart/contract/roadmap dengan requirement ini. Bila perlu rekonsiliasi dalam `MODULE BLUEPRINT MODE` dengan target sah, catat perubahan sempit dan traceability. Jangan ulang wawancara untuk 10 keputusan Journey yang sudah dikunci. Konflik medis/approval yang belum terjawab hanya memblokir jalur terdampak.
5. Catat baseline test yang ada dan izin environment. Setelah preflight sah, implementasikan source langsung sesuai FIN task. Jangan berhenti di audit.

### 4.2 Backend-first — FIN-HRD-REC-JRN-01 sampai 03

- Implementasikan DTO projection, service/query, controller GET journey, permission mapping dan tests. Temukan relasi existing di `HrdCandidate`, `HrdCandidateApplication`, `HrdJobVacancy`, `HrdCandidateScreening`, `HrdCandidateEvaluation`, `HrdJobOffer`, `HrdJobOfferApproval`, `HrdPreEmploymentMedicalCheck`, `HrdTalentPool`, `HrdCandidateHiringHandoff` dan entity lifecycle terkait. Verifikasi path/symbol aktual; model ini bukan mandat tabel baru. Gunakan read-only projection untuk agregasi.
- Buat fungsi mapping status dan eligibility terpusat dalam domain yang sesuai, reuse policy existing sejauh mungkin agar Journey dan mutasi tidak punya dua aturan yang saling bertentangan. Dukung status terminal, permission masking, unknown dan partial. Jangan buat workflow engine kedua.
- Perbaiki guard yang telah dijabarkan: screening sebelum evaluasi, hasil submit nyata sebelum offer, veto, approval sebelum kirim, persetujuan kandidat sebelum MCU, penanganan pembatasan kerja/terminal, handoff idempotent dan recovery. Uji API direct terhadap aksi yang dipanggil di luar urutan. Hanya ubah aturan yang telah didukung keputusan canonical; tandai konflik kontrak dan jalur yang belum disetujui.
- Buktikan GET tidak mengubah state dan tidak mengambil data lintas scope. Periksa query footprint, consistency response, actor permissions, response wrapper, 400/401/403/404/409/422 dan error handling sesuai kontrak. Data sensitif tidak muncul di log.
- Lakukan build dan tes backend sesuai governance setelah perubahan siap; gunakan targeted test dan environment yang sah. Jangan menjalankan migration, akses DB langsung atau build yang dilarang oleh aturan aktif tanpa izin. Jika backend belum siap untuk satu aksi, FE harus menampilkan blocker yang jujur, bukan simulate success.

### 4.3 Frontend — FIN-HRD-REC-JRN-04 sampai 05

Pertahankan route `/hr/recruitment/candidates/[id]`. Usulan deep-link: `/hr/recruitment/candidates/{candidateId}?applicationId={applicationId}&stage={stageKey}`. Validasi keduanya; `stage` hanya panel yang dibaca, tidak memajukan state. Jika route convention existing berbeda, gunakan helper resmi dan dokumentasikan. Halaman daftar kandidat, wizard dan seluruh antrean kerja tetap berfungsi.

**Pola visual desktop (wireframe instruksi, bukan mock data produksi):**

```text
┌─────────────────────────────────────────────────────────────────────────┐
│ Hero existing: Detail Pelamar                               [Kembali]  │
│ Rekrutmen & Seleksi / Pelamar                                           │
├─────────────────────────────────────────────────────────────────────────┤
│ Ringkasan kandidat: Nama · Kode kandidat · Status kandidat              │
│ Pilih lamaran [Nomor lamaran – Lowongan – Status                 v]     │
│ Unit/Jabatan · Jenis lowongan · Status LAMARAN (bukan status kandidat)   │
├─────────────────────────────────────────────────────────────────────────┤
│  1 Pendaftaran — 2 Administrasi — 3 Tes/Wawancara — 4 Penawaran         │
│                  — 5 MCU — 6 Serah Terima                             │
│  Progress dari BE + status hambatan/terminal jika ada                    │
├─────────────────────────────────────────────┬───────────────────────────┤
│ KONTEN TAHAP DIPILIH                         │ SIDE SECTION              │
│ Judul tahap + status + ringkasan             │ Tahap aktif sebenarnya     │
│ Field data / tabel ringkas yang relevan      │ Prasyarat / alasan blokir  │
│ Bukti/hasil sesuai izin                     │ Petugas/role berwenang     │
│                                             │ Langkah berikutnya         │
│                                             │ [Aksi sah untuk tahap]     │
│                                             │ [Lihat detail existing]    │
├─────────────────────────────────────────────┴───────────────────────────┤
│ Riwayat proses / Dokumen / Daftar lamaran (tabs/section ringkas existing)│
└─────────────────────────────────────────────────────────────────────────┘
```

Gunakan header, warna, spacing, radius, typography, status badge dan form existing. Jangan membuat halaman baru yang mengubah keseluruhan identitas visual. Desktop: konten utama sekitar 2/3 lebar dan side section sekitar 1/3, sesuaikan dengan layout tokens dan area sidebar actual. Tablet/mobile: side section turun setelah ringkasan/konten yang relevan, stepper wrap atau scroll horizontal yang aksesibel; tidak ada overflow halaman. Aksi tidak ditaruh sebagai deretan tombol panjang di header. Fokus pada satu CTA utama yang sah, aksi sekunder sesuai kebutuhan. Hindari card bertumpuk panjang, tabel raksasa tanpa kebutuhan, dan modal yang memenuhi layar untuk workflow multi-hari.

**Detail panel per tahapan:**

| Step | Konten tengah | Side section / aksi kontekstual |
|---|---|---|
| 01 Pendaftaran | Ringkasan lowongan, identitas/kontak yang diizinkan, nomor/tanggal lamaran, daftar dokumen dan kelengkapan. | Lanjutkan Pendaftaran jika perlu; lihat dokumen/detail. Jangan daftar ulang jika application sudah ada. |
| 02 Seleksi Administrasi | Hasil screening, dokumen relevan, status STR jika klinis, catatan dan waktu evaluasi yang sah. | Mulai Seleksi / Nilai Berkas / Lihat hasil sesuai allowedActions. Jika rejected, tampilkan alasan yang aman dan blokir downstream. |
| 03 Tes & Wawancara | Dua subpanel ringkas HR dan klinis/teknis bila berlaku; jadwal, asesor yang boleh diketahui, status submit dan rekomendasi. | Jadwalkan, isi evaluasi sendiri, lihat hasil, link ke halaman existing; jika bukan asesor, tampilkan menunggu petugas. |
| 04 Surat Penawaran | Nomor/status offer, approval stages, respons kandidat, ringkasan kontrak; nominal hanya jika berizin. | Buat draft, buka penawaran, approve/reject sesuai assignment, kirim/rekam respons sesuai state; jangan duplikasi engine approval. |
| 05 MCU Pra-Kerja | Rujukan, tanggal, status kelayakan, hambatan/konfirmasi pembatasan yang sah; detail medis restricted. | Buat Rujukan / Catat Hasil hanya jika diizinkan; jika role HR umum, lihat status ringkas. |
| 06 Serah Terima | Ringkasan prasyarat, status handoff, referensi profil dan kasus onboarding bila berizin. | Serah Terima / Konfirmasi / Pemulihan sah; Lanjutkan ke Onboarding hanya bila case ada. |

**Interaksi:**

- Stepper dapat dibaca/dipilih untuk meninjau tahapan; tahap aktif server ditandai berbeda dari tahap yang user lihat. Status tidak semata warna, gunakan label dan icon/teks yang aksesibel. Klik tahap blocked hanya membuka alasan, tidak membuka action ilegal. Jika role tidak berhak baca section, tampilkan informasi akses terbatas yang tidak membocorkan konten.
- Loading skeleton/loader yang sesuai, empty kandidat tanpa lamaran, partial/restricted section, error API yang eksplisit dengan Retry, not found dan conflict. Jangan isi demo/fallback sukses ketika server error. Unknown status tidak berarti belum dimulai. Jangan reset input/selection yang tidak terkait saat mutasi gagal.
- Aksi gunakan `RecruitmentService`/view existing. Boleh reuse modal untuk aksi kecil; gunakan layar existing untuk form besar. Passing application/offer/evaluation ID harus benar; validasi kembali saat route dibuka. Jika kembali dari layar kerja, refetch journey yang sesuai. Handle double-click, pending, expired session, 403/409/422, stale cache dan jaringan putus. Jangan auto-retry POST tanpa rekonsiliasi.
- Preserve history/tabs dokumen dan fungsi upload/apply/resume existing. Jangan mencabut akses user yang hanya memiliki permission tertentu untuk kerja daftar lama. Jangan mengubah `menu-items.jsx` kecuali ada bug nyata yang secara khusus disetujui; planning ini tidak memberi izin restrukturisasi sidebar.
- Pisahkan route tipis, view komposisi, hook controller, service, constants, style CSS Module dan test mengikuti arsitektur existing; nama path baru adalah DEV_DISCRETION sesuai governance. Tambah method GET journey ke service rekrutmen yang ada, bukan instance axios baru. Jangan mengganti keseluruhan Redux/HTTP layer.

**Base component decision gate (pemetaan awal, wajib verifikasi props dan katalog terbaru):**

| Elemen | Kandidat reuse / ketentuan |
|---|---|
| Header | `Hero` yang sudah dipakai detail kandidat. |
| Tombol | `BaseButton` dengan variant resmi, loading/disabled; jangan membuat tombol global baru. |
| Pilih lamaran | Base form/select existing, opsi memakai application ID dan label bermakna; gunakan HrResourceField bila sesuai kontrak. |
| Stepper | Cari komponen step/wizard yang benar-benar tersedia; review Registration Wizard, payroll/credentialing wizard dan WorkflowStepperCard sebagai referensi pola. WorkflowStepperCard existing berupa tabel, jangan menganggap sudah ada base horizontal stepper. Reuse bila cocok, jika tidak komposisikan primitive/base component sesuai decision gate; tidak membuat sistem styling paralel. |
| Ringkasan dan stage content | `HrPanel`, `HrInfoGrid` atau pattern detail/base component yang canonical. |
| Status | `StatusBadge`/`HrStatusBadge` map raw backend; label terminal, unknown, restricted jelas. |
| Daftar dokumen/riwayat | `DataTable` dan field/row semantics existing, tanpa komponen tabel custom. |
| Form dan konfirmasi | `BaseInputField`, `BaseSelectField`, `BaseTextAreaField`, `ConfirmModal`, helper upload existing; verify export/props. |
| Error/empty/toast | `InformationAlert`, `ToastStack`, loading/empty/access gate existing. |
| Layout | Token warna/spacing/typography dan CSS Modules; gunakan breakpoint resmi, jangan hardcode theme baru. |

Catat keputusan REUSE/COMPOSE/NEW per elemen beserta evidence path. Jika butuh komponen baru lintas modul, ikuti gate approval; tidak membuat duplikasi hanya karena lebih cepat. Uji keyboard focus, tab order, label, aria-current untuk step aktif dan status tidak mengandalkan warna saja.

### 4.4 UAT live browser dan API — FIN-HRD-REC-JRN-06

Gunakan browser nyata/headed pada environment test yang diizinkan dengan backend dan database development yang memang tersedia. Jangan menggunakan fixture produksi atau real PII/medical records. Siapkan lewat UI/API sah: kandidat sintetis klinis dengan dua lamaran (A, B), kandidat nonklinis, dan kandidat jalur terminal/cadangan bila diperlukan; gunakan lowongan/formasi test yang valid. Tidak memasukkan data palsu ke production. Bila setup/akun/permission tidak ada, tandai kasus `BLOCKED` atau `NOT RUN`, jangan membuat bukti sukses.

Akun/peran minimal (role aktual mengikuti permission matrix): recruiter/HRD, asesor yang ditugaskan dan asesor lain, penyetuju offer, petugas MCU/K3RS, HRD pembaca terbatas, pelaku handoff. Jika satu akun dev memiliki semua hak, itu tidak membuktikan negative role tests.

Urutan: Critical/Fatal → Business Compliance → Functional & Validation → Visual/UI → UI/UX. Setiap kasus: prasyarat, langkah aktual, expected, actual, PASS/FAIL/NOT RUN/BLOCKED, actor/role, HTTP/status/data yang telah disamarkan, screenshot/bukti bila tersedia, bug dan retest. Tabel ini adalah rencana, bukan hasil tes.

| ID | Prioritas | Skenario / langkah inti | Expected result |
|---|---|---|---|
| RJ-UAT-01 | P0 | Login tanpa autentikasi lalu buka Journey/GET langsung. | Ditolak sesuai 401; tidak ada data kandidat. |
| RJ-UAT-02 | P0 | User tanpa akses kandidat/objek mencoba application ID lain/deep link. | 403/404 sesuai kontrak; tidak ada IDOR atau kebocoran. |
| RJ-UAT-03 | P0 | Buka kandidat A dengan application milik kandidat berbeda via URL. | Konteks tidak valid ditolak; tidak tampil atau memutasi lamaran lain. |
| RJ-UAT-04 | P0 | Kandidat memiliki lamaran A dan B dengan status berbeda. Pilih A lalu B. | Lowongan, screening, evaluasi, offer dan aksi tidak tercampur. |
| RJ-UAT-05 | P0 | Banyak kandidat bernama sama / data melewati 50 baris; buka aplikasi tertentu. | Agregasi mengambil ID tepat, bukan hasil search nama/paging yang hilang. |
| RJ-UAT-06 | P1 | Kandidat Registered tanpa lamaran. | Empty state dan Resume bila berizin; tidak dibuat application otomatis. |
| RJ-UAT-07 | P1 | Buat lamaran melalui wizard existing dan buka Journey. | Nomor lamaran dan data riil tampil, tidak mendaftar ulang. |
| RJ-UAT-08 | P1 | Ganti lamaran cepat saat request belum selesai, lalu refresh. | Tidak ada stale response A tampil pada B; konteks pilihan pulih. |
| RJ-UAT-09 | P1 | Mulai seleksi, isi hasil, refresh dan buka daftar penapisan existing. | Status per lamaran konsisten antar halaman dan persisten. |
| RJ-UAT-10 | P0 | Kandidat klinis dengan STR hilang/kedaluwarsa melalui workflow sah. | Penolakan sesuai aturan existing; tidak ada CTA offer; uji direct API. |
| RJ-UAT-11 | P1 | Kandidat nonklinis tanpa STR. | STR tidak diwajibkan; hasil sesuai screening existing. |
| RJ-UAT-12 | P0 | Kandidat dengan beberapa STR/dokumen lamaran berbeda dan data relevan/expired. | Pemilihan sesuai aturan yang sah; tidak mengambil berkas application lain secara salah. |
| RJ-UAT-13 | P0 | Jadwalkan HR/klinikal; belum kirim hasil. | Tahap belum dianggap lulus, offer tidak dapat dibuat prematur. |
| RJ-UAT-14 | P0 | Asesor bukan pemilik evaluasi mencoba membaca/mengirim nilai. | Akses sesuai scope; submit pihak lain ditolak termasuk direct API. |
| RJ-UAT-15 | P0 | Klinis: hanya HR lulus, asesmen wajib belum selesai. | Belum siap offer; kedua jalur diperlukan sesuai kontrak. |
| RJ-UAT-16 | P0 | Klinis: asesor NotRecommended/veto; HR mencoba override/offer. | Veto persist, offer ditolak, tidak ada override HR. |
| RJ-UAT-17 | P0 | Direct API jadwalkan/offer ketika screening belum lulus atau ditolak. | Server menolak; UI menjelaskan blocker, tidak hanya menyembunyikan tombol. |
| RJ-UAT-18 | P1 | Jalur evaluasi sah klinis/nonklinis (dan teknis bila berlaku) disubmit. | Status siap offer hanya jika semua persyaratan terpenuhi. |
| RJ-UAT-19 | P0 | Offer menunggu approval atau bukan assigned approver; coba send/approve. | Penolakan server; tier approval existing tetap berlaku. |
| RJ-UAT-20 | P0 | User tanpa hak nominal membuka Journey, inspect network/history. | Tidak ada nominal/sensitive field di payload/DOM/log; user berhak tetap dapat melihat sesuai kontrak. |
| RJ-UAT-21 | P0 | Offer Approved/SentConditional tetapi kandidat belum menerima; register MCU/direct API. | MCU ditolak sesuai `HRD-DEC-078`; respons yang belum sah tidak membuka MCU. |
| RJ-UAT-22 | P0 | ConditionalAccepted dan rujuk MCU; user HR biasa membaca, petugas MCU mencatat hasil. | Hanya ringkasan laik kerja untuk HR; data medis restricted; hak evaluasi dipatuhi. |
| RJ-UAT-23 | P0 | MCU Unfit/terminal, atau status tidak dikenal; coba handoff. | Handoff diblokir sesuai canonical/kontrak, tidak ada status sukses palsu. |
| RJ-UAT-24 | P0 | FitWithRestrictions tanpa bukti konfirmasi penyesuaian kerja; kemudian jalur yang sah bila tersedia. | Tidak boleh handoff tanpa syarat; blocker bila kontrak belum mendukung, bukti konfirmasi nyata bila tersedia. |
| RJ-UAT-25 | P0 | Offer dan MCU sah, generate/confirm handoff lalu buka kasus. | Satu handoff/profil/relasi kursi dan case benar; bukan aktif kerja otomatis. |
| RJ-UAT-26 | P0 | Simulasikan kegagalan pembuatan case setelah handoff pada test terkendali, lalu retry sah. | Partial/pending, tidak ada tombol sukses onboarding, recovery tidak duplikasi profil/kursi/case. |
| RJ-UAT-27 | P0 | Double submit, dua tab dan timeout setelah mutasi offer/handoff; refetch. | Tidak duplikasi; konflik/hasil dipulihkan dengan state server. |
| RJ-UAT-28 | P1 | API gagal/timeout/403/409/422 pada tahap yang dibuka. | Pesan sesuai kondisi, retry aman, tidak fallback demo atau state selesai. |
| RJ-UAT-29 | P1 | Offer ditolak/revoked/expired, talent pool/withdrawn bila jalur tersedia. | Outcome benar, tidak memaksa step berikut; jalur cadangan existing tetap berizin. |
| RJ-UAT-30 | P1 | Klik tahap lampau/mendatang, buka halaman kerja existing dan kembali. | Hanya panel berubah; ID application benar; status server ter-refresh; tidak auto advance. |
| RJ-UAT-31 | P1 | Role hanya baca Journey atau izin sebagian; coba tindakan langsung. | Section restricted jelas, allowedActions sesuai; mutasi langsung yang tidak sah ditolak. |
| RJ-UAT-32 | P1 | GET journey berulang, bandingkan audit/state tanpa mutasi. | Read-only, tidak menciptakan state/history baru. |
| RJ-UAT-33 | P1 | Kasus onboarding ada tetapi user tidak berhak membukanya. | Tidak bocor case detail; tombol tidak menawarkan akses yang tidak sah. |
| RJ-UAT-34 | P2 | Desktop, tablet, mobile; teks panjang, zoom, tema existing. | Tidak overflow; stepper terbaca; side section dan CTA proporsional; desain konsisten. |
| RJ-UAT-35 | P2 | Navigasi keyboard, fokus, screen reader label, loading dan feedback. | Aksesibel; tahap/status dapat dipahami tanpa warna; tombol jelas. |
| RJ-UAT-36 | P1 | Regresi menu Lowongan, Pelamar, Seleksi, Evaluasi, Offer, MCU, Talent Pool, Onboarding dan wizard sebelumnya. | Route/sidebar tidak berubah, antrean kerja existing berfungsi, transaksi tidak ganda. |

Tambahan kontrak/backend automated tests: projection semua raw enum/status penting, state bertentangan, pending vs completed, restricted payload, multi-application, invalid GUID, direct API guards, concurrency/retry, GET no write, failure handoff, role assignment, DTO serialization dan existing regression. Bila integration environment tidak tersedia, jalankan unit/contract test yang dapat dijalankan dan tandai gap; jangan menyamakan unit test dengan UAT.

### 4.5 Definition of Done

- FIN 01–03: endpoint agregat nyata, scope dan permission benar, state projection akurat, guard mutasi yang sudah disetujui ditegakkan server-side, handoff/case tidak false success, no new schema tanpa izin. Semua area yang membutuhkan keputusan tambahan diberi blocker tepat.
- FIN 04–05: Detail Pelamar tetap pada route existing; selector per lamaran, stepper enam tahap, konten/side section, status dan aksi sesuai data real, link/resume/context/refetch benar; UI base component decision gate dan design tokens dipenuhi.
- FIN 06: build/lint/QBE/test policy sesuai aturan aktual, hasil test dan UAT dicatat dengan bukti; semua P0/P1 yang diwajibkan lulus sebelum klaim siap. Perbaiki bug dalam scope dan retest. Tidak mengklaim 100% atau production-ready jika ada test belum berjalan, blocker, atau risiko kritis.
- Rekonsiliasi task/report resmi `docs/module-blueprints/human-resource/task/report/{backend|frontend}/` dan roadmap/traceability sesuai mode dan izin. Jangan mengedit blueprint root di luar mode yang sah. Gunakan ID resmi setelah preflight, FIN lokal sebagai referensi bila belum terdaftar. UAT satu laporan menu/feature sesuai governance.

## 5. Stop conditions

Berhenti hanya pada fase/area terdampak jika branch/workspace salah, canonical governance tidak terbaca, registered runtime skill yang diminta tidak tersedia, permission akses/target tulis tidak sah, keputusan material belum ada, lingkungan uji tidak diizinkan, atau schema/operasi dilarang tanpa otorisasi. Laporkan status spesifik, source, dampak, dan tindakan yang diperlukan; lanjutkan area independen yang sah.

Jangan berhenti karena audit selesai, ada halaman existing, kode berhasil dikompilasi, atau ditemukan bug yang dapat diperbaiki dalam scope. Jangan mengulang wawancara keputusan yang telah disetujui. Jangan mengganti kontrak klinis/approval yang ambigu dengan asumsi baru. Jangan menciptakan fake success, dummy API, data produksi tiruan, maupun direct DB mutation. Jangan otomatis commit/push/pull/merge/rebase/reset/deploy/migration/SQL.

## 6. Output

Agent wajib mengembalikan ringkasan implementasi yang dapat diverifikasi (bukan hanya pernyataan "selesai"):

1. Status enam task FIN, task ID resmi bila terdaftar, branch/SHA baseline dan source akhir, working tree/diff/file aktual FE dan BE.
2. Matriks lima Submenu target registry (Lowongan, Pelamar, Seleksi Administrasi, Tes/ Wawancara, Onboarding) serta integrasi Offer, MCU, Talent Pool/Requisition; bedakan reuse, add, repair, partial, blocked dan NOT VERIFIED.
3. Kontrak endpoint agregasi final, permission matrix, status mapping dan bukti validasi API; sebutkan field yang di-mask, alasan blocker, dan metode menjaga isolation per application.
4. Bukti UI aktual: screenshot desktop/mobile untuk keenam panel, pemilihan dua lamaran, terminal/blocked, role restricted, dan navigasi ke onboarding bila tersedia. Tanpa data sensitif.
5. Hasil build, lint, unit/integration/security/QBE/contract, seluruh RJ-UAT-01–36 beserta actual result, bug dan retest. Laporkan NOT RUN/BLOCKED dengan alasan; tidak membuat screenshot atau hasil tes palsu.
6. Laporan task sesuai template repository serta rekonsiliasi roadmap/traceability bila diizinkan; tidak melakukan commit/push. Status migrasi/DB/deploy secara eksplisit.
7. Daftar sisa keputusan (jika ada) dengan dampak spesifik, khususnya mismatch MCU dan recovery handoff; next action terdekat. Jangan mengubah semua kebijakan HRD dalam feature ini.

### Checkpoint handoff (untuk sinkronisasi jika diizinkan)

- 25 Nov 2026: Registration Wizard terlihat pada source `362eaa2`, belum UAT live oleh pemeriksaan ChatGPT. Recruitment Journey planning telah disetujui; enam tahap per ApplicationId, agregasi read-only, reuse, privasi, no sidebar/schema baru tanpa izin.
- FE `362eaa2185e1511ef3723bba481cdea77d841ed5`; BE `ba6671d01ecc87e826ef01065cc7d8a9e36141ce`; Skills `62b7ead4595161ec8f9bb7fc2dd96b3a7b29f2a5`.
- `memory.md` pada root blueprint HRD belum ditemukan (404 saat pembacaan); `MEMORY_NOT_INITIALIZED / MEMORY_PENDING_SYNC`. Jangan klaim telah tersimpan di repo. Jika izin MODULE BLUEPRINT MODE tersedia, sinkronkan checkpoint dengan history yang benar. Usulan lokasi instruksi: `docs/module-blueprints/human-resource/docs/update/rekrutmen-dan-seleksi/INSTRUKSI_AGENT_HRD_RECRUITMENT_JOURNEY.md`.
- Risiko prioritas: scope per lamaran, evaluasi prematur, conditional offer sebelum MCU, privasi, FitWithRestrictions/Unfit mismatch, handoff tanpa case. Next: jalankan FIN 01–03 → FIN 04–05 → FIN 06. Onboarding Wizard adalah pekerjaan berikutnya, bukan scope instruksi ini.

**PERINTAH FINAL AGENT: Setelah preflight dan mode sah, MULAI LANGSUNG IMPLEMENTASI SOURCE CODE Recruitment Journey. Backend terlebih dahulu, kemudian frontend dan UAT live browser. Jangan berhenti pada audit atau mockup.**
