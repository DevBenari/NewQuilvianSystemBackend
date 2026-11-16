# MASTER PROMPT — IMPLEMENTASI LANGSUNG HRD MENU 12: PEMBERHENTIAN & PENSIUN

**Jenis dokumen:** Instruksi kerja Agent Developer Fullstack Quilvian V2  
**Status:** Instruksi eksekusi; BUKAN hasil implementasi atau laporan UAT  
**Task mode yang diberikan:** `CROSS-REPO MODE` — izin mengubah source frontend dan backend secara terbatas pada enam Submenu di bawah; izin menulis laporan pekerjaan pada folder blueprint terkait. Tidak termasuk izin commit, push, deployment produksi, atau operasi database langsung.  
**Target bisnis:** Modul HRD / Kepegawaian → **Menu 12 — Pemberhentian & Pensiun**  
**Tanggal penyusunan:** 16 November 2026  
**Bahasa kerja/laporan:** Bahasa Indonesia  
**Prinsip utama:** **Langsung implementasikan task yang telah didefinisikan, bukan mengulang audit atau berhenti pada dokumentasi.**

---

## 1. Role

Anda adalah **Senior Fullstack Developer Quilvian V2 + Engineer Integrasi HRD + Pelaksana UAT Live Browser**. Anda bekerja pada aplikasi rumah sakit yang telah memiliki banyak fitur aktif. Pekerjaan Anda adalah **mengubah source code yang relevan** agar enam Submenu Pemberhentian & Pensiun benar-benar dapat digunakan, terhubung ke API dan persistence yang benar, memenuhi aturan bisnis, mengikuti UI/UX resmi Quilvian, serta dapat dibuktikan lewat pengujian browser.

Wewenang yang diberikan untuk pekerjaan ini:

- **Frontend target tulis:** `DevBenari/QuilvianSystemFrontendDev`, branch **`HamzahV2`**.
- **Backend target tulis:** `DevBenari/NewQuilvianSystemBackend`, branch **`MHamzah`**.
- **Dokumen laporan:** folder `docs/module-blueprints/human-resource/` di workspace backend, mengikuti aturan lokasi laporan suite Engineering Skills.
- **Repository Engineering Skills:** `DevBenari/QuilvianEngineeringSkills`, branch **`main`**, sebagai sumber **canonical read-only**. Jangan mengubah aturan/skill untuk menghindari kewajiban implementasi.

**Jangan mengambil posisi sebagai auditor yang hanya menulis rekomendasi.** Inspeksi source secukupnya hanya untuk menentukan file dan kontrak yang perlu diperbaiki; setelah itu **langsung tulis kode, hubungkan API, perbaiki bug, jalankan validasi yang diizinkan governance, lakukan UAT live, perbaiki kegagalan, lalu uji ulang.**

---

## 2. Task

### 2.1 Hasil akhir yang wajib dihasilkan

Bangun satu **Menu tersendiri** bernama persis **`Pemberhentian & Pensiun`** pada navigasi HRD yang memuat **enam Submenu** dengan label berikut (jangan rename sepihak):

1. **Pengunduran Diri**
2. **Pensiun**
3. **Akhir Kontrak**
4. **Pemberhentian**
5. **Serah Terima**
6. **Clearense**

Label `Clearense` dipertahankan sesuai keputusan pengguna; istilah bisnisnya *exit clearance*. Jika hendak mengubah ejaan UI menjadi `Clearance`, minta keputusan tersendiri; **jangan mengubah label tanpa izin**.

**Usulan navigasi baru** (cek benturan existing sebelum membuat):

| Submenu | Route baru yang direkomendasikan | Keterkaitan existing |
|---|---|---|
| Pengunduran Diri | `/hr/separation/resignations` | Self-service resign dan HR workflow existing |
| Pensiun | `/hr/separation/retirements` | `TrxRetirement` / lifecycle management |
| Akhir Kontrak | `/hr/separation/contract-expiry` | `/hr/lifecycle/employment-contracts` dan keputusan kontrak |
| Pemberhentian | `/hr/separation/terminations` | `/hrd/relations/separations` dan kalkulasi pesangon |
| Serah Terima | `/hr/separation/handovers` | Offboarding checklist dan tugas serah terima |
| Clearense | `/hr/separation/exit-clearance` | `/hr/exit-clearance` dan `/hrd/relations/exit-clearance` |

Route di atas **target desain, bukan klaim route existing**. Periksa route aktual, buat route tipis atau redirect/alias aman jika perlu. Jangan mematahkan bookmark, permission, atau navigasi existing. Pertahankan **`Hubungan Industrial & Terminasi`**, **`Onboarding & Kontrak`**, `Pengajuan PHK & Pesangon PP 35/2021`, dan `Exit Clearance Karyawan` apabila masih mempunyai konsumen: boleh menunjuk ke komponen/data yang sama, **bukan membuat transaksi ganda**. Sidebar tetap memakai struktur dan mekanisme izin navigasi existing.

### 2.2 Submenu 1 — Pengunduran Diri

**Aktor:** Pegawai (ESS), Kepala Unit/atasan, HRD, pejabat berwenang sesuai workflow.

**Alur bisnis yang HARUS diimplementasikan:**

1. Pegawai mengajukan melalui ESS; HRD dapat melihat daftar permohonan dan detail sesuai kewenangan. Pilih data pegawai dari sesi atau daftar resource, bukan ketik UUID.
2. Data minimal: pegawai, unit/jabatan, tanggal pengajuan, tanggal hari kerja terakhir yang diusulkan, alasan, bukti/lampiran bila diwajibkan, rencana serah terima, dan catatan.
3. Simpan draf, sunting draf, kirim, tarik/batalkan sesuai keadaan yang sah; setiap transisi diterapkan dan diverifikasi oleh backend.
4. Kepala Unit/HRD menindaklanjuti melalui workflow aktual, dengan riwayat actor, waktu dan keputusan. Jangan mengarang aktor persetujuan yang belum dikunci kebijakan.
5. Setelah disetujui, jalankan handoff ke **satu kasus separation** dan proses offboarding terkait secara **idempoten** (tidak membentuk separation/checklist ganda ketika diklik dua kali).
6. Status pegawai **tidak otomatis Nonaktif saat pengajuan**; perubahan status mengikuti keputusan sah dan tanggal efektif.
7. Pegawai/HRD dapat melihat timeline, alasan penolakan/perbaikan, dan hasil pengajuan secara konsisten setelah refresh.

**UI:** halaman list, tombol pengajuan, form bertahap/terkelompok, detail & timeline, status, lampiran, catatan keputusan, dan tindakan kontekstual berdasarkan `AvailableActions`/permission backend.

**Acceptance criteria:** satu pengajuan menghasilkan satu nomor dan histori benar; pengajuan ditolak jika tanggal/role/status tidak valid; approval tidak dapat dilakukan oleh pihak yang tidak berwenang atau oleh pegawai sendiri bila aturan melarang; handoff tidak menggandakan berkas; data tetap ada setelah refresh.

### 2.3 Submenu 2 — Pensiun

**Aktor:** HRD, Kepala Unit, pejabat keputusan, Finance/Payroll.

**Alur bisnis:**

1. Tampilkan pegawai mendekati pensiun berdasarkan tanggal lahir, masa kerja, dan **pengaturan kebijakan rumah sakit yang terverifikasi**; jangan hardcode satu usia pensiun untuk semua jenis pegawai.
2. HRD memverifikasi identitas, jenis hubungan kerja, masa kerja, tanggal pensiun rencana, tanggal efektif, jenis pensiun, catatan, dan dokumen pendukung.
3. Kepala Unit menyiapkan pengganti dan proses serah terima; pejabat berwenang memberikan keputusan sesuai matriks otorisasi yang disetujui.
4. Setelah keputusan sah, buat atau hubungkan satu `EmployeeSeparation` bertipe pensiun dan berkas offboarding; hindari menduplikasi proses jika sudah ada.
5. Hubungkan penyelesaian administratif ke Payroll dan pengaturan akses, tanpa menjadikan HRD pengganti pemilik perhitungan manfaat pensiun.

**UI:** daftar calon pensiun (filter rentang waktu, unit, status), form detail pensiun, timeline keputusan, ringkasan tindak lanjut, tindakan proses offboarding.

**Acceptance criteria:** tanggal/kelayakan jelas, tidak menerapkan usia pensiun tidak disetujui, tidak mempensiunkan pegawai secara prematur, dan status keputusan/riwayat sesuai backend.

### 2.4 Submenu 3 — Akhir Kontrak

**Aktor:** HRD, Kepala Unit, pejabat berwenang, pegawai.

**Alur bisnis:**

1. Ambil kontrak pegawai nyata dari modul existing `Kontrak Kerja`, termasuk tanggal akhir kontrak, status tanda tangan, dan riwayat kontrak.
2. Tampilkan daftar yang akan habis/berakhir dan rekomendasi Kepala Unit; berikan keputusan **perpanjang, tidak perpanjang, atau tindak lanjut lain yang benar-benar didukung kontrak**.
3. Validasi status kontrak, tanggal, keputusan sebelumnya, dan potensi duplikasi. Gunakan keputusan kontrak existing; jangan membuat database kontrak terpisah.
4. Bila diperpanjang, arahkan ke pembuatan/kelanjutan kontrak yang sah; bila benar-benar berakhir, lakukan handoff ke separation dan offboarding secara terkendali.
5. Catat pemberitahuan pegawai dan dokumen keputusan. **Jangan menyamakan kontrak berakhir dengan PHK disiplin**, dan jangan mengasumsikan rumus kewajiban PKWT tanpa dasar resmi yang dikonfigurasi.

**UI:** daftar kontrak mendekati habis, filter masa berlaku, panel ringkasan kontrak, form keputusan, detail dan histori, tautan ke kontrak sumber.

**Acceptance criteria:** keputusan tidak menggandakan kontrak, kontrak yang masih aktif tidak langsung dinonaktifkan secara salah, tanggal diperiksa, dan route lama `Kontrak Kerja` tetap berfungsi.

### 2.5 Submenu 4 — Pemberhentian

**Aktor:** HRD, Kepala Unit, pihak hubungan industrial/Legal bila relevan, pejabat berwenang, Finance.

**Alur bisnis:**

1. HRD membuat/mengelola usulan pemberhentian dengan identitas pegawai, dasar/alasan yang dipilih dari master resmi, kronologi, tanggal usulan/tanggal efektif, dan dokumen pendukung.
2. Periksa proses keberatan/perselisihan atau legal review yang diwajibkan; pengakhiran tidak otomatis sah hanya karena usulan dibuat.
3. Transisi persetujuan, keputusan, penolakan, penangguhan dan tanggal efektif **ditentukan oleh backend**; cegah self-approval, bypass URL, dan double submit.
4. Gunakan engine pesangon/kompensasi existing bila memang berlaku untuk jenis kasus yang dipilih, termasuk tahap verifikasi Finance; **tidak ada formula pesangon hardcoded di frontend**.
5. Setelah putusan sah dan efektif, buat/hubungkan offboarding, Serah Terima, Clearense, pengendalian akses, dan perubahan status kepegawaian dengan jejak audit.

**UI:** worklist kasus, form pemberhentian, detail riwayat, panel legal review, rincian pesangon dari server, keputusan, bukti/lampiran.

**Acceptance criteria:** pemegang izin yang salah tidak dapat memutuskan, data kasus tidak dapat tertukar, kompensasi dan kewajiban menggunakan data authoritative server, dan tidak ada pemutusan otomatis saat masih sengketa.

### 2.6 Submenu 5 — Serah Terima

**Aktor:** pegawai yang keluar, pegawai penerima, Kepala Unit, HRD.

**Alur bisnis:**

1. Saat separation disahkan/ditindaklanjuti, bentuk daftar pekerjaan serah terima dari template offboarding existing jika tersedia.
2. Item contoh: pekerjaan berjalan, jadwal/penugasan unit, dokumen administratif, tanggung jawab sarana/inventaris, dan penugasan pengganti. **Jangan menyalin data klinis pasien secara mentah ke HRD**; hanya rujukan atau konfirmasi serah terima sesuai kebijakan akses klinis.
3. Pegawai menyerahkan; penerima memeriksa; Kepala Unit memverifikasi. Simpan bukti, siapa yang bertindak, waktu, dan catatan jika ditolak/dikembalikan.
4. Status per item dan total progres harus berasal dari persistence backend; tidak cukup tombol yang mengubah state React lokal.
5. Item belum selesai tidak boleh ditandai selesai paksa oleh user tanpa izin dan alasan yang sah.

**UI:** daftar kasus serah terima, detail checklist, progres, penanggung jawab, tanggal target, unggah/lihat lampiran, tindakan serahkan/terima/verifikasi.

**Acceptance criteria:** setiap langkah terlacak dan tahan refresh, lampiran terikat item benar, satu item tidak dapat disetujui orang tidak berwenang, pelaporan progres akurat.

### 2.7 Submenu 6 — Clearense

**Aktor:** Kepala Unit, GA/Aset, unit terkait (termasuk alat medis jika ada), Finance, IT, HRD.

**Alur bisnis:**

1. Ambil kasus exit clearance terkait satu separation dari proses canonical yang dipilih; periksa dua implementasi backend existing sebelum memutuskan adapter. Jangan membuat dua clearance untuk kejadian yang sama.
2. Verifikasi kewajiban tiap unit: pekerjaan/serah terima, aset, alat/peralatan terkait, keuangan, hak akses, dan administrasi HRD; komposisi final mengikuti aturan RS dan proses existing.
3. Status item minimum: belum ditinjau, disetujui, terdapat kewajiban/ditolak, dengan catatan, bukti dan sign-off aktor unit yang benar.
4. **Keamanan:** identitas penyetuju berasal dari user login/server, **bukan dipercaya dari `approverUserId` yang dikirim browser**. Pastikan user hanya dapat menandatangani unitnya.
5. Sinkronkan status bebas tanggungan Finance dari sumber data yang berwenang. Bedakan status clearance administratif dari **hak normatif pengupahan/kompensasi**; jangan membuat kebijakan otomatis menahan upah yang secara hukum harus dibayarkan.
6. Selesaikan clearance hanya jika syarat yang benar-benar wajib terpenuhi; simpan audit dan tangani pembatalan/koreksi sesuai permission.
7. Pencabutan akses IT harus ditangani pada waktu yang sesuai keputusan dan kebijakan keamanan, bukan otomatis saat pengajuan baru dibuat.

**UI:** daftar clearance, ringkasan status lintas unit, detail matriks dengan badge, panel Finance, tombol verifikasi per unit yang diizinkan, bukti, catatan, alasan tidak selesai, riwayat penyelesaian.

**Acceptance criteria:** sign-off tidak bisa dipalsukan, gate Finance konsisten, tidak ada fake success, seluruh perubahan tersimpan dan dapat dilihat setelah refresh, status akhir tidak salah memicu perubahan hubungan kerja.

### 2.8 Integrasi lintas enam Submenu

- Gunakan **satu aggregate/sumber status akhir hubungan kerja** yang dapat ditelusuri; identitas `WorkforceProfileId`, `EmployeeId`, `EmployeeSeparationId`, dan `SeparationNumber` harus benar dan tidak tertukar.
- Satu pegawai dapat memiliki **riwayat** beberapa kontrak/peristiwa kerja, tetapi cegah dua proses aktif yang saling bertentangan untuk peristiwa keluar yang sama.
- Setiap aksi backend memiliki audit aktor, waktu, perubahan status dan alasan yang relevan; gunakan validasi status server-side.
- Implementasi HRD bekerja sama dengan master pegawai, kontrak, workflow, payroll/Finance, aset, User Management, dan jadwal kerja; **jangan mengubah domain lain secara diam-diam**. Jika butuh perubahan domain lain, laporkan kebutuhan scope dan lanjutkan pekerjaan independen.
- Jangan membuat data dummy, hardcoded daftar pegawai, fake success, atau fallback ke simulasi pada mode operasional.
- Jangan menyimpulkan bahwa tabel/endpoint sudah terpasang di database hanya dari adanya class atau route di source.

---

## 3. Context

### 3.1 Repository dan snapshot sumber bukti

| Fungsi | Repository | Branch wajib | HEAD SHA pada penyusunan dokumen |
|---|---|---|---|
| Frontend | `DevBenari/QuilvianSystemFrontendDev` | `HamzahV2` | `f5a0717bda0bb5bdcd68a3acc9fdde5b86074ead` |
| Backend | `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | `80c94e8ee8aea569fcb3dc0090921314d348f05f` |
| Skills/rules | `DevBenari/QuilvianEngineeringSkills` | `main` | `201af64552b27873fd9af83c6722cf11b441bb1e` |

SHA di atas adalah snapshot **saat instruksi dibuat**. Agent wajib membaca keadaan working tree dan HEAD lokal aktual lebih dahulu, mencatat jika berubah, lalu melakukan impact review hanya terhadap file terkait. **Jangan berpindah branch secara diam-diam, menghapus perubahan user, atau melakukan `git reset --hard`.**

### 3.2 Bukti source yang telah diperiksa sebagai titik awal

**Frontend:**

- `src/utils/menu-sidebar/menu-items.jsx`: sudah ada `Hubungan Industrial & Terminasi` dengan `Pengajuan PHK & Pesangon PP 35/2021` → `/hrd/relations/separations`, `Exit Clearance Karyawan` → `/hr/exit-clearance`; `Onboarding & Kontrak` memiliki `Kontrak Kerja` → `/hr/lifecycle/employment-contracts`. **Label Menu `Pemberhentian & Pensiun` belum ditemukan pada snapshot yang ditinjau.**
- `src/components/view/hrd/relations/relations-separations-view.jsx`: list pemisahan, pengajuan, kalkulasi pesangon, otorisasi Finance, via `EmployeeRelationService`.
- `src/components/view/hrd/relations/relations-exit-clearance-view.jsx`: matriks clearance multi-departemen, sign-off, finalisasi, status payroll lock.
- `src/components/view/hr/exit-clearance/exit-clearance-list-view.jsx` dan `exit-clearance-detail-view.jsx`: UI clearance HR dengan lifecycle API.
- `src/lib/services/hr/employee-relation-service.js`: API `/v1/hrd/separations` dan `/v1/hrd/exit-clearance`.
- `src/lib/services/hr/exit-clearance-service.js`: API lifecycle exit-clearance via `EXIT_CLEARANCE_CONFIG.endpoint`.
- `src/app/hr/lifecycle/employment-contracts/page.jsx`: entry point daftar kontrak.
- `AGENTS.md`: konstitusi frontend dan kewenangan `CROSS-REPO MODE`.

**Backend:**

- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/ResignationController.cs`: route `api/v1/corporate/human-resource/lifecycle-management/resignation-requests`, API list/detail, workflow, `handoff`.
- `Areas/SelfServices/HumanResource/Controllers/ResignationSelfServiceController.cs`: route `api/v1/self-services/human-resource/resignation-requests`, termasuk create/update/submit/cancel.
- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/HrdEmploymentContractController.cs`: route `api/v1/corporate/human-resource/employment-contracts`, keputusan kontrak, notifikasi pegawai, dokumen.
- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/ExitClearanceController.cs`: route `api/v1/corporate/human-resource/lifecycle-management/exit-clearances`.
- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Controllers/EmployeeSeparationController.cs`: route `api/v1/hrd/separations`, perhitungan dan persetujuan pesangon.
- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Controllers/MultiDeptExitClearanceController.cs`: route `api/v1/hrd/exit-clearance`.
- Model dengan pemetaan EF yang diperiksa: `TrxResignationRequest`, `TrxRetirement`, `HrdContractDecision`, `TrxTermination`, `TrxEmployeeSeparation`, `TrxExitClearance`, `WfpOffboardingChecklist`, `WfpOffboardingTask`, `HrdExitClearanceProcess`, `HrdExitClearanceChecklist`. **Adanya model dan mapping bukan bukti tabel sudah diterapkan ke database runtime.**
- `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`: aturan backend terpasang.

**Temuan teknis yang harus langsung ditangani dalam task relevan:**

1. **Ketidaksesuaian metadata akses resign**: pada `ResignationController.cs`, action `Synchronize` dan `Handoff` menggunakan `[AccessAction("Synchronize"/"Handoff")]` tetapi `[AccessPermission("ResignationRequest", "Update")]`. Cocokkan dengan aturan registry `AccessAction`/`AccessPermission` dan perbaiki secara aman, termasuk dampak seed/akses yang sudah diberikan. Jangan mengubah izin menjadi terbuka.
2. **Risiko pemalsuan identitas sign-off**: `EmployeeRelationManagement/Services/ExitClearanceService.cs` mengisi `ApprovedByUserId` dari `request.ApproverUserId`, walaupun controller mengenali aktor. Perbaiki agar penyetuju dan cakupan unit berasal dari autentikasi/otorisasi backend dan audit yang sah.
3. **Dua jalur exit clearance**: `LifecycleManagement/ExitClearanceController.cs` dan `EmployeeRelationManagement/MultiDeptExitClearanceController.cs`/service mengelola proses terkait. Tentukan satu sumber kebenaran/adapter tanpa membuat dua hasil berbeda.
4. **Gerbang payroll akhir**: service EmployeeRelation mengubah `IsFinalPayrollUnlocked` setelah seluruh checklist selesai. Validasi makna status ini terhadap kebijakan HRD/Finance dan hak yang wajib dibayarkan; jangan secara tidak sah menahan upah atau pembayaran normatif.
5. **Satu proses resign ke offboarding**: `ResignationLifecycleHandoffService.cs` membuat `TrxEmployeeSeparation` beserta offboarding/clearance. Pastikan idempotency, nomor dokumen aman, workflow dan identitas pegawai benar, serta tidak membentuk dua agregat paralel.

Temuan adalah bukti source **as-is** dan kebutuhan perbaikan, bukan hasil pengujian runtime.

### 3.3 Governance Quilvian Engineering Skills yang wajib dibaca dan ditaati

**Jangan sekadar menyebut nama skill dalam laporan. Baca isinya lalu ikuti.** Sumber: `https://github.com/DevBenari/QuilvianEngineeringSkills/tree/main`.

Baca paling awal:

- FE dan BE masing-masing: `AGENTS.md` pada branch kerja aktual.
- Suite canonical: `agents/rules/GLOBAL_RULES.md`, `agents/rules/README.md`.
- Skill implementasi: `Claude/.claude/skills/build-module-backend/SKILL.md` dan `Claude/.claude/skills/build-module-frontend/SKILL.md` (atau distribusi terpasang vendor yang setara).
- UI tambahan: `Antigravity/.agents/skills/refine-menu-ui/SKILL.md` jika tersedia pada runtime yang digunakan. Aturan canonical frontend tetap berlaku untuk semua vendor.
- UAT: `Claude/.claude/skills/live-browser-testing/SKILL.md` dan `agents/rules/testing/RULE_LIVE_BROWSER_TESTING.md`.

**Frontend rules wajib:**

- `agents/rules/frontend/frontend-architecture.md`
- `agents/rules/frontend/frontend-project-profile.md`
- `agents/rules/frontend/base-component-catalog.md`
- `agents/rules/frontend/base-component-decision-gate.md`
- `agents/rules/frontend/page-composition-patterns.md`
- `agents/rules/frontend/design-tokens.md`
- `agents/rules/frontend/ui-consistency-checklist.md`
- `agents/rules/frontend/test-policy.md`
- `agents/rules/frontend/REPORT_TEMPLATE.md`

**Backend rules wajib yang relevan:**

- `agents/rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md`
- `agents/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- `agents/rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `CROSS_REPO_RULES.md`
- `agents/rules/backend/API_RULES.md`, `DATABASE_RULES.md`, `role-access-rules.md`
- `agents/rules/backend/transaction-endpoint-standard.md`, `REVIEW_RULES.md`, `TEST_POLICY.md`, `REPORT_TEMPLATE.md`

**Dokumentasi & DB:**

- `agents/rules/rule-output/aturan-output-dokumentasi.md`, `lokasi-laporan-task.md`, dan aturan status roadmap.
- Jika ada interaksi DB langsung, baca `Antigravity/.agents/skills/manage-database-operations/SKILL.md` atau governance yang berlaku untuk target DB. **Bedakan `QuilvianNewDevHamzah` (pribadi) dari `QuilvianNewDevMaster` (shared).**

Penting: **`AGENTS.md` pada target repo dan governance canonical lebih tinggi daripada contoh legacy.** Entity baru tidak boleh meniru prefix `Trx*` lama; ikuti ownership/prefix yang disetujui. Untuk `NEW CODE` backend: `IdentityModel`, EF configuration, controller → service → persistence, DTO terpisah, permission metadata tepat, audit, unique numbering durabel, dan status yang aman. Jangan refactor seluruh legacy hanya untuk merapikannya.

### 3.4 Kebijakan dan informasi runtime yang belum diberikan

Nilai di bawah adalah **placeholder keputusan yang belum terbukti**, bukan fakta atau izin tersirat:

- `[usia/jenis pensiun menurut kebijakan RS dan jenis kepegawaian]`
- `[aktor/urutan persetujuan untuk resign, pensiun, akhir kontrak, dan pemberhentian]`
- `[ketentuan notice period dan pengecualian yang berlaku]`
- `[format dan penandatangan surat keputusan/berita acara]`
- `[ketentuan perhitungan pesangon, kompensasi PKWT, manfaat pensiun sesuai kasus]`
- `[kapan pengakhiran memengaruhi jadwal, akses login, email, dan izin klinis]`
- `[unit clearance dan item wajib per tipe pegawai]`
- `[URL aplikasi runtime yang sudah berjalan, atau izin menjalankan dev server lokal]`
- `[task ID roadmap aktif, revision blueprint/kontrak to-be yang sudah disetujui, bila diwajibkan governance dan telah tersedia]`
- `[akun UAT berhak akses sesuai peran; jangan cantumkan password ke dokumen]`
- `[database pengembangan yang sah untuk menyimpan data uji melalui API resmi]`

Jangan menjadikan placeholder alasan untuk menghentikan seluruh pekerjaan. **Kerjakan semua bagian yang dapat diselesaikan dari kontrak existing**, lalu ajukan hanya keputusan yang benar-benar memblokir bagian tertentu. Jangan mengarang kebijakan ketenagakerjaan rumah sakit.

### 3.5 Lokasi dokumen dan bukti

- Folder kerja instruksi/suplemen yang direkomendasikan (di repository backend): `docs/module-blueprints/human-resource/docs/update/pemberhentian-pensiun/`. Di komputer pemilik proyek: `C:\Users\Admin\Documents\Quilvian\Qulvian\NewQuilvianSystemBackend\docs\module-blueprints\human-resource\docs\update\pemberhentian-pensiun\`. **Folder ini adalah lokasi tujuan yang diusulkan; periksa apakah sudah ada sebelum membuat.**
- Laporan task resmi harus mengikuti aturan lokasi canonical `<blueprint-root>/task/report/backend/<TASK-ID>.md` dan `<blueprint-root>/task/report/frontend/<TASK-ID>.md`, berdampingan dengan roadmap pemilik task; jangan membuat sistem laporan tandingan.
- **Satu Menu = satu laporan live browser UAT** di `docs/module-blueprints/<modul>/<sub-modul>/testing/test-by-agy/` sesuai lokasi blueprint yang ditetapkan governance.
- Skrip Playwright dan screenshots **hanya** di frontend `test-with-agy/scripts/` dan `test-with-agy/screenshots/<nama-menu>/`; jangan simpan di `src/` atau di root source.

---

## 4. Reasoning

### 4.1 Urutan eksekusi WAJIB — pekerjaan dimulai dari implementasi

**Jangan membuat enam dokumen audit baru sebagai pengganti kode. Jangan berhenti setelah membuat rencana.**

**Langkah A — Preflight sangat singkat:** pastikan branch/working tree, source relevan, `AGENTS.md`, canonical governance, kontrak/API, serta task ID/revision yang memang diwajibkan dan sudah disetujui. Jangan menciptakan nomor approval atau menganggap kontrak draft sudah sah. Pertahankan perubahan lokal milik user. Pemetaan file secukupnya, lalu lanjut menulis kode pada pekerjaan yang sudah berwenang.

**Langkah B — Backend lebih dulu untuk kontrak/transisi:** gunakan controller, DTO, service, persistence, dan permission yang sudah ada. Perbaiki temuan akses/sign-off/idempotency/clearance canonical. Lengkapi endpoint yang benar-benar diperlukan untuk alur enam Submenu. Jangan membuat entity baru jika existing dapat di-extend aman. Bila entity baru diperlukan, lakukan governance prefix dan minta wewenang schema tersendiri; lanjutkan task lain.

**Langkah C — Frontend:** tambahkan menu tersendiri dan enam halaman, mengutamakan reuse route, hook, service dan base components. Sambungkan seluruh aksi ke API nyata. Gunakan service/slice yang sesuai lalu hook untuk orchestrasi; jangan memanggil Axios dari `page.jsx`/view atau menggunakan client HTTP baru. State loading/success/error/403/404/409/422 harus jelas.

**Langkah D — Validasi dan UAT:** jalankan validasi yang diwajibkan `AGENTS.md` dan rules, termasuk pemeriksaan statis, UI checker, test relevan, serta build pada lingkungan pengembangan yang **diizinkan untuk validasi**; catat perintah/hasil sebenarnya. **Jangan menganggap build berhasil sebagai bukti UI selesai.** Jalankan Chromium **headed** pada aplikasi lokal/staging yang aman dan dapat diakses, lalu gunakan klik, ketik, submit, screenshot, dan verifikasi network/API.

**Langkah E — Perbaikan:** ketika menemukan FAIL, perbaiki kode/payload/UI/permission yang berada dalam scope lalu **ulangi test yang gagal serta regression test**. Jangan mengganti hasil FAIL dengan dokumen rekomendasi.

**Langkah F — Handoff:** setelah implementasi dan pengujian yang benar-benar dilakukan, catat diff, build/test, screenshot, bukti persistence, dampak role, blocker asli, dan laporan yang sesuai governance.

### 4.2 Arsitektur frontend yang harus dipatuhi

```text
URL / Menu HRD
  → src/app/**/page.jsx (entry point tipis)
  → src/components/view/hr/** (susunan UI)
  → src/lib/hooks/hr/** (state/actions/filter/pagination)
  → src/lib/services/hr/** ATAU src/lib/state/**
  → src/lib/axiosInstance/InstanceAxios.jsx
  → Backend API resmi
```

**Dilarang:** Tailwind CSS, komponen `<button>`/`<table>`/search/dialog mentah jika base tersedia; style `#HEX`/`rgba()` literal baru; class CSS yang tidak terdefinisi; input UUID manual; request langsung dalam view; fake success; dan membuat skema state global/HTTP client paralel.

**Base Component resmi yang wajib diutamakan:**

| Elemen UI | Pilihan awal wajib | Decision Gate awal |
|---|---|---|
| Header halaman | `Hero` | `REUSE` |
| Summary angka | `SummaryGrid` | `REUSE` |
| Filter/search | `DataFilter`, `FilterSelect`, `FilterDatePicker`, `ResourceFilterSelect` | `REUSE` / `COMPOSE` |
| Tabel & pagination | `DataTable`, `Pagination` | `REUSE` |
| Tombol aksi | `BaseButton` | `REUSE` |
| Status | `StatusBadge` | `REUSE` |
| Error/denied | `InformationAlert`, `AccessDeniedGate` | `REUSE` / `COMPOSE` |
| Notifikasi | `ToastStack` | `REUSE` |
| Konfirmasi / alasan | `ConfirmModal` | `REUSE` |
| Detail | `BaseDetailView` | `REUSE` / `COMPOSE` |
| Form tambah/edit | `BaseEditorView` atau `BaseGroupedEditorView`, keluarga `BaseFormControl` | `REUSE` / `COMPOSE` |

Setiap keputusan di atas **harus dikonfirmasi pada source komponen dan props yang berlaku**, bukan menyalin nama dari tabel ini. Sebelum menulis JSX/CSS, jalankan **Base Component Decision Gate** (`REUSE → EXTEND → COMPOSE → WRAP → NEW`). Untuk status `NEW` atau `EXTEND` yang mengubah perilaku default, minta keputusan pengguna pada komponen tersebut tetapi **lanjutkan pekerjaan independen**. Jangan membuat Base Component pengganti hanya untuk mempercepat.

### 4.3 Skema tampilan halaman — wajib diimplementasikan

**Kerangka halaman list** (urutannya mengikuti `page-composition-patterns.md`):

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ HERO: Pengunduran Diri / Pensiun / ...       [Tombol aksi berizin]       │
├──────────────────────────────────────────────────────────────────────────┤
│ SUMMARY GRID: Total | Draft | Menunggu | Selesai | Butuh Tindakan        │
├──────────────────────────────────────────────────────────────────────────┤
│ DATA FILTER: [Cari pegawai] [Tanggal] [Unit] [Status] [Reset]           │
├──────────────────────────────────────────────────────────────────────────┤
│ INFORMATION ALERT: pesan penting/galat jaringan jika ada                │
├──────────────────────────────────────────────────────────────────────────┤
│ DATA TABLE: No | Nomor | Pegawai | Unit | Tanggal | Status | Aksi       │
│            ... baris hasil nyata dari API                               │
│            [Paginasi]                                                   │
├──────────────────────────────────────────────────────────────────────────┤
│ TOAST STACK: hasil tindakan sebenarnya                                  │
└──────────────────────────────────────────────────────────────────────────┘
```

Kartu angka dan kolom disesuaikan dengan makna masing-masing Submenu. Jangan memaksakan field `Total Pesangon` ke semua jenis akhir hubungan kerja. Jangan membocorkan detail finansial kepada pegawai yang tidak berwenang.

**Kerangka detail proses:**

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ BASE DETAIL VIEW: Kembali | Nomor Proses | Pegawai | Status | Aksi      │
├──────────────────────────────────────────────────────────────────────────┤
│ Identitas & masa kerja          │ Ringkasan aturan/tanggal efektif       │
│ Alasan / dokumen / lampiran     │ Tindakan yang diizinkan backend         │
├──────────────────────────────────────────────────────────────────────────┤
│ Timeline: Draf → Diajukan → Diverifikasi → Diputuskan → Offboarding      │
├──────────────────────────────────────────────────────────────────────────┤
│ [Serah Terima] [Clearense] [Riwayat keputusan] [Dokumen resmi]           │
└──────────────────────────────────────────────────────────────────────────┘
```

**Kerangka editor/form:** gunakan `BaseGroupedEditorView` untuk form kompleks; kelompokkan **Data Pegawai**, **Dasar/Alasan**, **Tanggal**, **Lampiran**, **Persetujuan/Rencana Tindak Lanjut**. Tampilkan field wajib, validasi per field, tanggal tidak konsisten, batas ukuran/format lampiran, status menyimpan, dan tombol Batal/Simpan/Kirim yang benar-benar terhubung API.

**Layout Serah Terima:** panel identitas, progres, daftar item dengan owner/penerima/atasan, tenggat, catatan, lampiran, dan riwayat. **Layout Clearense:** matriks unit dengan satu baris per kewajiban, badge status, nilai tanggungan jika berwenang, aksi sign-off per unit, detail verifikator & waktu, alert jika tertunda. Gunakan base `DataTable` dan komposisi panel yang sudah ada sebelum membuat komponen baru.

**Responsive:** periksa desktop 1440px, tablet 768px, dan ponsel 390px. Tidak ada overflow yang memutus tombol aksi, filter tetap mudah dipakai, tabel tetap memiliki scroll horizontal terkontrol, modal memiliki tinggi/scroll aman, dan label form tidak terpotong.

**Wajib mengikuti:** Poppins, CSS variables `--color-*`, `--font-size-*`, `--space-*`, `--radius-*`, `--shadow-*` dari `globals.css`; gunakan CSS Module domain hanya bila styling base tidak mencukupi. Jalankan `agents/rules/frontend/tools/check-css-usage.mjs` dari suite pada berkas JSX yang diubah dan dokumentasikan hasil aktual; target **0 temuan**.

### 4.4 Kontrak backend, workflow dan keamanan

1. Periksa endpoint controller aktual, request/response DTO, HTTP verb, nilai status, metadata `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, dan error response; **backend authoritative**, frontend harus menyesuaikan.
2. Otorisasi diset melalui **Akses Role**, bukan hardcode nama jabatan/departemen. Pastikan pasangan action dan permission sesuai exact name. Uji dengan akun bukan SuperAdmin agar 403 tidak tersamarkan.
3. Gunakan service untuk aturan bisnis, DB context/persistence melalui service sesuai QBE. Semua mutasi tervalidasi pada server: kepemilikan, state transition, timestamp efektif, idempotency, validasi tanggal, dan audit.
4. Reuse `HrdContractDecision`, `TrxEmployeeSeparation`, `TrxResignationRequest`, offboarding, dan clearance existing; **untuk entity baru** gunakan prefix dari registry yang benar, bukan `Trx*` legacy, dan `IEntityTypeConfiguration<T>`.
5. Pembangkitan nomor bisnis unik wajib aman di database dan mengikuti contract number-series yang berlaku. Jangan memakai `Count+1` atau `Max+1` tanpa proteksi.
6. Pastikan action konfirmasi keuangan dan release payroll tidak membuka akses/menahan pembayaran secara keliru. Tampilkan data dari Finance server, bukan angka ilustrasi.
7. Jika ada dua controller/aggregate yang memiliki tujuan serupa, buat **pemetaan owner dan adapter** yang beralasan, bukan menghapus satu secara langsung. Perbaiki jalur yang jelas tanpa merusak konsumen existing.
8. Perubahan integrasi ke modul lain hanya setelah dapat dibuktikan perlu dan dalam wewenang. Jangan membuat endpoint/field baru hanya berdasarkan asumsi UI.

### 4.5 Test case UAT live browser — wajib dijalankan setelah implementasi

**Engine & interaksi:** Chromium **headed mode** (`headless: false`) dan `slowMo` sekitar 400–550 ms sesuai skill. Agent sendiri login, klik, mengisi form, memilih resource, submit, merekam respons network, mengambil screenshot, dan verifikasi perubahan. **Jangan mengganti UAT dengan screenshot statis atau kode Playwright yang tidak pernah dijalankan.**

**Gunakan data uji non-produksi**, akun sesuai izin, dan lingkungan yang sah. Penyimpanan record UAT dilakukan melalui **UI/API resmi**; query database langsung untuk pembuktian maksimal `SELECT` yang diizinkan. Hindari menghapus/mengubah pegawai sungguhan atau menyebarkan data rahasia dalam screenshot.

| ID | P | Submenu | Tindakan yang diuji | Hasil yang harus terbukti |
|---|---|---|---|---|
| UAT-01 | P0 | Semua | Buka enam route dari sidebar | Halaman termuat, menu aktif, tidak blank/crash/404 |
| UAT-02 | P0 | Semua | Fetch daftar/filter dengan backend live | Data API tampil atau empty state yang jujur; tidak dummy |
| UAT-03 | P0 | Pengunduran Diri | Buat draf dari akun pegawai test | Nomor/ID benar, tersimpan, refresh tetap ada |
| UAT-04 | P0 | Pengunduran Diri | Submit → verifikasi → putusan sesuai workflow | Status & timeline benar, identitas aktor benar |
| UAT-05 | P0 | Pengunduran Diri | Handoff dua kali / dua tab | Tidak ada separation/clearance ganda |
| UAT-06 | P0 | Pensiun | Tetapkan kasus pensiun test sah | Keputusan dan relasi separation akurat |
| UAT-07 | P0 | Akhir Kontrak | Keputusan tidak diperpanjang | Riwayat kontrak & offboarding konsisten |
| UAT-08 | P0 | Akhir Kontrak | Keputusan diperpanjang | Tidak membuat termination/offboarding salah |
| UAT-09 | P0 | Pemberhentian | Proses kasus dengan keputusan sah | Tidak auto-final sebelum proses terpenuhi |
| UAT-10 | P0 | Serah Terima | Serahkan → terima → verifikasi item | Semua aksi persistence, progres berubah tepat |
| UAT-11 | P0 | Clearense | Sign-off lintas unit yang sah | Status/item/aktor/tanggal benar setelah refresh |
| UAT-12 | P0 | Clearense | Finalisasi ketika semua syarat sah | Status final tepat, tidak duplikasi proses |
| UAT-13 | P1 | Semua | Coba akses/action tanpa permission | 403/penolakan; menu/tombol tidak memberi hak backend |
| UAT-14 | P1 | Pengunduran Diri | Ajukan resign lalu periksa status pegawai | Pegawai tidak otomatis Nonaktif saat baru mengajukan |
| UAT-15 | P1 | Pensiun | Pegawai belum memenuhi aturan pensiun | Tidak dipensiunkan otomatis karena usia hardcoded |
| UAT-16 | P1 | Pemberhentian | Proses perkara yang belum sah/sengketa | Finalisasi ditolak, riwayat tidak hilang |
| UAT-17 | P1 | Clearense | Sign-off oleh akun unit berbeda | Ditolak backend; actor tidak bisa dipalsukan di request |
| UAT-18 | P1 | Clearense | Ada piutang/kewajiban yang belum selesai | Status jelas, keputusan sesuai kebijakan & hukum |
| UAT-19 | P1 | Semua | Cek nomor/id pegawai dua kasus berbeda | Tidak ada kebocoran atau pertukaran data |
| UAT-20 | P2 | Semua form | Kosongkan field wajib, data tidak logis | Pesan validasi Indonesia, tanpa fake success |
| UAT-21 | P2 | Semua form | Tanggal akhir lebih awal dari tanggal awal | Ditolak sesuai aturan masing-masing |
| UAT-22 | P2 | Semua form | Spam tombol Simpan/Kirim atau double click | Backend idempotent/tidak membuat transaksi ganda |
| UAT-23 | P2 | Semua | Uji filter gabungan, reset, pagination, refresh | Hasil dan total konsisten dengan API |
| UAT-24 | P3 | Semua | Teks sangat panjang, karakter khusus, script injection | Validasi/sanitasi aman, tidak XSS |
| UAT-25 | P3 | Semua | Session habis, 401/403/409/422/500 | Kesalahan ditampilkan tepat, tidak ada toast berhasil |
| UAT-26 | P3 | Semua | Buka record selesai lalu coba aksi lama | Transisi tidak sah ditolak server |
| UAT-27 | P4 | Semua | Screenshot 1440/768/390 | Layout rapi, modal dan tabel dapat dipakai |
| UAT-28 | P4 | Semua | Cek komponen/tokens/warna/status | Sesuai Base Component, tidak Tailwind/hardcoded CSS |
| UAT-29 | P5 | Semua | Keyboard/tab/focus dan label aksesibilitas | Aksi dapat diakses, error dan loading jelas |
| UAT-30 | P5 | Semua | Alur lengkap Kepala Unit–HRD–Finance–IT | Mudah dipahami, jumlah langkah masuk akal |

**Setiap hasil wajib mencatat** waktu, URL, role/akun uji yang telah disamarkan, aksi, payload aman yang relevan, HTTP status, status PASS/FAIL/BLOCKED, screenshot, dan evidence persistence (bila tersedia). Bila test FAIL yang ada dalam scope, **langsung perbaiki dan ulangi**. Bila fatal, hentikan hanya alur fatal tersebut, simpan bukti, lanjutkan subfitur independen yang aman.

### 4.6 Verifikasi terukur dan regresi

- Backend: ikuti `AGENTS.md` dan `rules/backend/TEST_POLICY.md` untuk QBE preflight, review diff, kontrak API, validasi proses, dan **validasi build aplikasi** pada lingkungan dev yang diizinkan. **Jangan membuat project xUnit/NUnit/MSTest baru** tanpa diminta; kebijakan backend tidak mewajibkan project test otomatis tambahan.
- Frontend: jalankan pemeriksaan UI token/CSS dan lint, tes yang relevan menurut `rules/frontend/test-policy.md`, serta validasi visual per halaman yang berubah. Jangan memperkenalkan Jest karena project menggunakan `node:test`.
- Sebelum menjalankan dev server/validasi runtime, cek proses yang sudah berjalan, port, target database, environment, dan wewenang operasional; **jangan mematikan server orang lain**. Nyalakan aplikasi di lingkungan pengembangan **hanya bila diizinkan oleh `AGENTS.md`/user/lingkungan eksekusi**, atau gunakan runtime localhost/staging yang sudah tersedia. Jangan mengklaim UAT PASS jika browser tidak pernah dibuka.
- Pembuatan migration, penerapan migration ke database, direct `INSERT/UPDATE/DELETE/DDL`, release/deploy, commit/push adalah **aksi terpisah** yang memerlukan otorisasi sesuai governance. Perubahan source kode yang diperbolehkan **tidak** memberi izin otomatis untuk aksi ini.
- Static PASS bukan Live PASS. Screenshot tanpa interaksi bukan functional PASS. Menu muncul bukan bukti create/approval/persistence. Build PASS bukan Production Ready.

---

## 5. Stop conditions

### 5.1 Kapan pekerjaan **TIDAK BOLEH BERHENTI**

Jangan berhenti hanya karena:

- Audit source telah selesai.
- Blueprint, rencana, atau enam file Markdown telah ditulis.
- Sebagian route/API existing telah ditemukan.
- Ada satu Submenu yang menunggu keputusan bisnis; **lanjutkan lima lainnya yang independen**.
- Ada bug pada UAT; **perbaiki dan ulangi** selama dalam scope dan wewenang.
- UI terlihat sudah bagus, tetapi API/persistence atau akses backend belum benar.

### 5.2 Kapan **HARUS BERHENTI hanya pada bagian terdampak**

Hentikan aksi tertentu, laporkan blocker yang spesifik, dan minta keputusan pengguna jika:

1. `AGENTS.md` atau governance canonical/Engineering Skills tidak terbaca (`BLOCKED — canonical governance unavailable`). Jangan mengarang penggantinya.
2. Branch/source/workspace yang akan diubah bukan `HamzahV2` atau `MHamzah`, atau izin tulis ke salah satu target tidak jelas.
3. Membutuhkan **migration** baru atau eksekusi pada DB; periksa ownership, dampak data, SQL/rollback dan peroleh izin terpisah. Jangan membuat atau menjalankan migration otomatis.
4. Membutuhkan perubahan aturan HRD yang belum dikunci: usia pensiun, notice period, siapa pejabat yang memutus, alasan hukum PHK, format dokumen yang menimbulkan kewajiban, aturan hak akhir, atau pencabutan akses.
5. Membutuhkan Base Component benar-benar `NEW`, atau `EXTEND` yang mengubah default bagi modul lain. Ajukan opsi dan rekomendasi; lanjutkan bagian independen.
6. Memerlukan perubahan data pegawai nyata, hak akses sistem produksi, sistem Finance produksi, deployment, atau operasi destruktif.
7. Tidak ada browser headed/akun UAT/URL server yang memungkinkan pengujian; dokumentasikan **UAT BLOCKED** dengan penyebab yang dapat diverifikasi, **bukan** klaim PASS. Tetap selesaikan pekerjaan kode yang tidak bergantung pada runtime.

**Larangan keamanan:** jangan commit/push/merge/deploy secara otomatis; jangan menampilkan password/token pribadi di laporan; jangan menyimpan data pasien sensitif pada dokumen HRD; jangan membuat success palsu atau hasil test fiktif; jangan menghapus perubahan user yang tidak terkait; jangan memperluas source di luar Menu 12 tanpa wewenang.

### 5.3 Kriteria selesai / Definition of Done

**Task Menu 12 baru boleh diklaim `SELESAI` apabila:**

- Satu Menu `Pemberhentian & Pensiun` dan keenam Submenu benar-benar tampil pada sidebar sesuai permission.
- Keenam route/halaman memiliki UI baku Quilvian dan benar-benar berfungsi sesuai bisnis yang dapat diberi wewenang.
- Semua tombol utama terhubung backend, tanpa data simulasi/fake success dan tanpa duplikasi transaksi.
- Permission backend, actor sign-off, role access, idempotency, status pegawai, kontrak, serta clearance/Finance aman.
- Validasi source/visual yang diwajibkan governance dijalankan dan bukti hasilnya ada; semua kegagalan prioritas tinggi telah diperbaiki.
- UAT **live Chromium headed** untuk P0/P1 terkait lulus, tindakan terkait P2/P3/P4/P5 tercatat; screenshot, request/response aman, dan persistence dapat ditelusuri.
- Laporan tracked task, satu laporan UAT Menu 12, source diff, dan status roadmap diperbarui menurut aturan output yang berlaku.

Jika sebagian kondisi belum terpenuhi, berikan status `PARTIAL/BLOCKED` atau `🟡/⛔` sesuai governance **dengan bukti dan rencana perbaikan konkret**. **Dilarang mengarang persentase 70%/95%, melabeli “SIAP OPERASIONAL”, atau mengaku UAT berhasil tanpa pengujian aktual.**

---

## 6. Output

### 6.1 Artefak utama yang WAJIB diserahkan Agent

1. **Perubahan source code sebenarnya** pada frontend `HamzahV2` dan backend `MHamzah` sesuai scope serta `git diff --stat` dan daftar file yang berubah; bedakan implementasi baru, reuse, refactor kecil, dan bug fix.
2. **Menu & halaman siap dipakai** untuk enam Submenu, beserta route final, screenshot, dan penjelasan fungsi utama per halaman.
3. **Matriks API FE ↔ BE** berdasarkan controller/DTO aktual: `[Tags(...)]`, HTTP verb, URL, request/response kunci, permission, dan status transaksi; cantumkan perbaikan kontrak/role yang dilakukan.
4. **Bukti rule compliance**: Base Component Decision Gate per elemen (`REUSE`, `EXTEND`, `COMPOSE`, `WRAP`, `NEW`), CSS checker, pemakaian tokens, laporan lint/build/test yang benar-benar dieksekusi, dan bukti QBE backend.
5. **Laporan UAT live browser tunggal untuk Menu 12**: 30 test case sebagai baseline (tambahkan hanya kasus relevan yang benar-benar perlu), hasil PASS/FAIL/BLOCKED, screenshot, waktu/URL/aktor uji, network/API dan re-test setelah bug fix.
6. **Dokumen handoff operasional singkat**: cara membuka menu, hak akses yang perlu diatur oleh admin melalui `Akses Role`, dependensi ke Payroll/IT/GA, batasan dan risiko yang tersisa.
7. **Task tracked reports dan status roadmap** menurut `agents/rules/rule-output/lokasi-laporan-task.md` dan aturan status canonical. Jangan menggantikan laporan resmi itu dengan dokumen analisis umum.

### 6.2 Format jawaban akhir Agent

Laporan ke pengguna **WAJIB** dimulai dengan hasil implementasi yang nyata, bukan klaim audit:

```text
IMPLEMENTASI HRD MENU 12 — PEMBERHENTIAN & PENSIUN

Target FE : DevBenari/QuilvianSystemFrontendDev @ HamzahV2
Target BE : DevBenari/NewQuilvianSystemBackend @ MHamzah
Mode      : CROSS-REPO (source FE+BE, dokumentasi terbatas)

1. HASIL SOURCE CODE
   - [file/path] : [perubahan nyata]
   - [file/path] : [perubahan nyata]
   - Diff: [ringkasan git diff]

2. HASIL PER SUBMENU
   Pengunduran Diri : IMPLEMENTED / PARTIAL / BLOCKED — [bukti]
   Pensiun         : IMPLEMENTED / PARTIAL / BLOCKED — [bukti]
   Akhir Kontrak   : IMPLEMENTED / PARTIAL / BLOCKED — [bukti]
   Pemberhentian   : IMPLEMENTED / PARTIAL / BLOCKED — [bukti]
   Serah Terima    : IMPLEMENTED / PARTIAL / BLOCKED — [bukti]
   Clearense       : IMPLEMENTED / PARTIAL / BLOCKED — [bukti]

3. VALIDASI ENGINEERING SKILLS
   UI GATE / CSS CHECKER / QBE / LINT / BUILD / TEST
   [Perintah nyata, hasil nyata, warning/error sebenarnya]

4. UAT CHROMIUM HEADED
   Dijalankan: [Ya/Tidak; URL dan lingkungan]
   PASS: [n] | FAIL: [n] | BLOCKED: [n]
   Screenshot/laporan: [path yang benar-benar ada]
   Bug diperbaiki + retest: [bukti]

5. STATUS AKHIR
   SELESAI / PARTIAL / BLOCKED, alasan, dependensi,
   keputusan owner yang benar-benar masih diperlukan.
```

**Perintah akhir:** Setelah membaca instruksi ini dan governance canonical yang dapat diakses, **MULAI LANGSUNG MENGERJAKAN SOURCE CODE** Menu 12. Jangan hanya membuat proposal, menulis audit, atau menyatakan pekerjaan selesai pada tingkat dokumen. Jalankan implementasi sampai kriteria selesai yang berada dalam wewenang tercapai, lalu lakukan live-browser UAT dan perbaiki hasil FAIL yang masuk scope. Jika terblokir keputusan tertentu, berikan blocker tersebut secara spesifik **tanpa menghentikan pekerjaan lain yang independen**.
