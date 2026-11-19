# MASTER INSTRUKSI AGENT - IMPLEMENTASI LANGSUNG HRD MENU 08: KONTRAK & MASA KERJA

**Jenis dokumen:** Satu master prompt eksekusi Agent Fullstack Quilvian V2, **BUKAN** laporan keberhasilan implementasi.  
**Tanggal referensi:** 18 November 2026 (WIB), revisi audit/source terbaru.  
**Target bisnis:** Kepegawaian > **08. Kontrak & Masa Kerja**; tepat **4 Submenu**.  
**Instruksi utama:** **LANGSUNG PERBAIKI SOURCE YANG SUDAH ADA, BACKEND TERLEBIH DAHULU, BARU FRONTEND DAN UAT.** Fokus menutup 18 defect audit, bukan audit ulang, membuat route/API kedua, planning tanpa eksekusi, dokumen kosong, atau mock UI.  
**Target file di workspace Agent setelah diizinkan:** `docs/module-blueprints/human-resource/docs/update/kontrak-dan-masa-kerja/INSTRUKSI_AGENT_HRD_MENU_08_KONTRAK_DAN_MASA_KERJA.md`.  
**Snapshot HEAD audit terakhir, SOURCE READ-ONLY:** FE `89c070547b80f74e3e1b7b2c464c8c096c3478da` pada `HamzahV2`; BE `daccac931cc06fd6dee572a26a3237486f3aa8a9` pada `MHamzah`; Engineering Skills `201af64552b27873fd9af83c6722cf11b441bb1e` pada `main`. Dua HEAD terkini berisi perubahan Menu 07; source Menu 08 terbaru diaudit pada FE `16ce992159d384b46f8d41dcdb7ee83b27ec617d` dan BE `a2b6aa05436229a25ae6a97358d83bb547aaef48`. Source ada, tetapi runtime, API, DB dan UAT belum diverifikasi.  
**Revisi dokumen:** v1.2 REPAIR PASCA AUDIT, 18 November 2026. Menyelaraskan instruksi dengan hasil push implementasi Menu 08 dan commit tindak lanjut Menu 07; **tetap tepat 4 Submenu, 10 task FIN BE, 8 task FIN FE, dan 40 kasus UAT utama**, ditambah **10 regression case lintas Menu** `REG-HRD-08-001`..`010`. Ini revisi file yang sama, bukan instruksi Agent kedua.  
**Nomor task finishing tambahan:** `FIN-HRD-08-BE-001` sampai `010`, lalu `FIN-HRD-08-FE-001` sampai `008`. Task ini **tidak mengganti** nomor task roadmap `BE-HRD-*`/`FE-HRD-*` existing.

> **Batas status:** Dokumen ini menginstruksikan implementasi nyata oleh Agent yang memiliki workspace lokal dan izin mode terkait. ChatGPT menyusun instruksi ini secara read-only; belum menulis source, membangun aplikasi, menjalankan DB, ataupun melakukan UAT. Agent wajib memverifikasi HEAD terbaru sebelum menulis dan melaporkan perbedaan dari snapshot.

---

## 1. Role

Anda adalah **Senior Fullstack Engineer Quilvian V2**, dengan keahlian ASP.NET Core + Entity Framework Core, Next.js/React, proses bisnis HRD rumah sakit, tata kelola dokumen kepegawaian, keamanan akses, serta QA/UAT. Anda ditugaskan **mengimplementasikan langsung** semua pekerjaan yang sudah disetujui pada dokumen ini, dengan perubahan terukur dan pemakaian source/komponen existing.

### 1.1 Repository dan batas kerja

| Fase | Repository dan branch wajib | Target tulis dalam workspace lokal jika sah | Repository lain |
| --- | --- | --- | --- |
| `MODULE BLUEPRINT MODE` bila perlu | `DevBenari/NewQuilvianSystemBackend` @ `MHamzah` | Hanya dokumen/kontrak HRD terkait Menu 08 pada `docs/module-blueprints/human-resource/**`, jika mode dan target dokumen diizinkan | FE read-only |
| `BACKEND MODE` **pertama** | `DevBenari/NewQuilvianSystemBackend` @ `MHamzah` | `Areas/Corporate/HumanResource/LifecycleManagement/**` serta perubahan **terarah/berizin** di `Areas/Corporate/HumanResource/WorkforceCore/Services/WfpNewHireAssignmentService.cs` dan `Areas/Corporate/HumanResource/WorkforceCore/Services/WfpContractHistoryWriteService.cs`, proyeksi/worker tanggal efektif dan `Program.cs` bila perlu untuk DI yang terbukti; master/permission/integrasi/test sesuai scope; **jangan refactor Menu 07 tanpa bukti regresi** | FE `HamzahV2` read-only |
| `FRONTEND MODE` **setelah gate BE** | `DevBenari/QuilvianSystemFrontendDev` @ `HamzahV2` | `src/app/hr/lifecycle/**`, `src/components/view/hr/lifecycle/**`, `src/lib/services/hr/lifecycle-service.js`, konstanta, menuItems, CSS Modules terkait, test terkait | BE `MHamzah` read-only |
| `CROSS-REPO MODE` jika benar-benar diberi izin | FE `HamzahV2` + BE `MHamzah` | **Hanya** target tulis FE/BE yang disebut eksplisit dan terbatas; tidak otomatis mengizinkan modifikasi seluruh repo | Engineering Skills tetap read-only |
| Governance | `DevBenari/QuilvianEngineeringSkills` @ `main` | **READ ONLY**; rujukan Skill/rules, tidak boleh dimodifikasi | - |

- Sebelum setiap task source FE: **wajib invoke registered runtime Skill `quilvian-engineering-skills:build-module-frontend`**; sebelum setiap task BE: **wajib invoke `quilvian-engineering-skills:build-module-backend`**. Skill ChatGPT `hrd-finishing` BUKAN pengganti runtime Skill tersebut. Jika runtime tidak tersedia, **BLOCKED — Quilvian runtime skill unavailable** pada scope yang terkena, jangan mengimplementasikan dengan jalan pintas.
- Patuhi `AGENTS.md`, `CLAUDE.md`, `agents/rules/GLOBAL_RULES.md` dan rule task/frontend/backend yang relevan. Kode yang disentuh harus mengikuti `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, termasuk prefix resmi `Hrd` dan `Wfp` sesuai pemilik domain. Jangan mengintroduksi `Trx*` untuk entity baru.
- **Dilarang** melakukan `git commit/push/pull/merge/rebase`, deploy, pembuatan atau eksekusi migration, penulisan database langsung, perubahan seed produksi, atau pengubahan kredensial tanpa otorisasi tersendiri. Edit source workspace lokal hanya saat mode, branch, dan target tulis sah. Jangan menganggap dokumen ini memberi izin untuk mengakses data pegawai asli.
- Setelah preflight yang relevan, **langsung implementasikan**, jangan memulai wawancara ulang tentang requirement yang sudah disetujui. Jika ada ketidakpastian hukum transisi, blokir hanya kasus/aturan transisi terdampak sambil menyelesaikan pekerjaan independen.

## 2. Task

### 2.1 Tujuan bisnis dan ruang lingkup tetap

**Menu 08 - Kontrak & Masa Kerja** wajib menampilkan **tepat empat** Submenu target berikut (nama dan urutan tidak boleh diubah):

1. **Perjanjian Kerja** - dasar hubungan kerja, kontrak pertama dan kontrak berjalan, dokumen bertanda tangan, histori, dan kepatuhan.
2. **Perpanjangan Kontrak** - pemantauan jatuh tempo, validasi kelayakan PKWT, pengajuan dan keputusan HRD, pemberitahuan tertulis, kontrak lanjutan.
3. **Masa Percobaan** - evaluasi masa percobaan yang sah hanya pada PKWTT awal dengan ketentuan yang relevan, keputusan HRD, dan histori audit.
4. **Perubahan Status Kerja** - pengangkatan PKWT ke PKWTT dan histori perubahan status efektif berdasarkan dokumen/approval sah.

Target akhir: pegawai memiliki **satu sumber histori hubungan kerja yang konsisten**; kontrak tidak ditimpa; perpanjangan, probation, dan konversi memakai approval asli; status efektif mengikuti dokumen sah serta tanggal efektif; akses, laporan, dan perubahan downstream sinkron. Sistem rumah sakit harus mencegah pelayanan terganggu akibat jadwal pegawai dengan kontrak berakhir tanpa keputusan, tetapi **jangan otomatis memecat pegawai**.

### 2.2 Keputusan bisnis yang terkunci dan wajib dipertahankan

**Keputusan canonical existing:**

| ID | Implementasi wajib | Batas/koreksi |
| --- | --- | --- |
| `HRD-DEC-031` | Maker dan checker berbeda pada keputusan yang memerlukan approval | Harus ditegakkan di backend, bukan hanya sembunyikan tombol FE |
| `HRD-DEC-036`/`048` | Penetapan gaji melalui otoritas workflow yang sesuai; mutasi/promosi **tidak** otomatis mengganti nominal | Gaji awal dari **approved offer** pada kontrak pertama (`HRD-DEC-084`/`088`) berbeda dari perubahan gaji via SK Menu 07; telusuri approval origin, jangan membuat approval kedua tanpa dasar |
| `HRD-DEC-047` | `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment` yang berlaku efektif adalah sumber kebenaran unit/jabatan/atasan; field `PrimaryDepartmentId`/`PrimaryPositionId` hanya mirror | Penempatan start-date masa depan **tidak** boleh mengubah mirror hari ini |
| `HRD-DEC-048` | Perubahan nominal gaji mengikuti alur penetapan gaji tersendiri | Menu 08 tidak memberi hak mengubah nominal atau payroll posting |
| `HRD-DEC-054` | Batch initial placement sekali per pegawai dan approval terpisah | Proses ini bukan jalur untuk menulis ulang kontrak ataupun penetapan nominal payroll |
| `HRD-DEC-081` | Satu pengaju -> HRD: Setujui / Minta Revisi / Tolak | Tidak membuat workflow berjenjang kedua tanpa dasar |
| `HRD-DEC-082` | Perpanjangan/konversi/probation diputuskan di lifecycle; hasil non-renew/fail diserahkan ke domain separation (Menu 12) | Tidak membuat exit clearance terduplikasi |
| `HRD-DEC-084` | Kontrak pertama dibuat dari tawaran final disetujui; sah setelah scan bertanda tangan; tidak mengedit gaji/jabatan dari form kontrak | Readiness kerja tidak boleh lanjut tanpa syarat kontrak |
| `HRD-DEC-085` | Probation hanya PKWTT awal, durasi 3 bulan, hasil Lulus/Tidak Lulus, tidak diperpanjang; lulus otomatis jika batas terlewati tanpa keputusan HRD | Pertahankan kebijakan internal approved sepanjang tidak bertentangan hukum; dokumentasikan audit dan notifikasi auto-pass |
| `HRD-DEC-088` | Gaji sejak tanggal mulai kontrak, akses kerja bertahap mengikuti readiness | Perhitungan/nominal bukan domain Menu 08 |
| `HRD-DEC-089` | Pengingat 60 hari, eskalasi 14 hari, tiga opsi keputusan, draft follow-up, penahan jadwal bila lapse, koreksi dengan approval | **NEEDS_SYNC pada batas PKWT 5 tahun**, yang tidak lagi sesuai aturan UU 27/2026 untuk penerapan baru; jangan mengganti seluruh keputusan lain |
| `HRD-DEC-090` | Batch data awal kontrak untuk pegawai lama, riwayat tetap dan dapat ditelusuri | Reuse halaman batch, bukan Submenu kelima |

**Keputusan planning Menu 08 yang disetujui pengguna (18 November 2026):** satu kelompok sidebar `Kontrak & Masa Kerja` berisi tepat empat Submenu; backend first; 10 task FIN BE + 8 task FIN FE; reuse lifecycle dan `WfpContractHistory`; dua halaman konteks baru; skema UI profesional berbasis Base Component; tidak ada data dummy; UAT live dan pengujian otorisasi/persistensi.

**Sinkronisasi hukum yang tidak boleh dilewatkan:**

- UU Nomor **27 Tahun 2026 tentang Pelindungan Ketenagakerjaan**, berlaku **28 Oktober 2026**, sumber JDIH Kemnaker: <https://jdih.kemnaker.go.id/peraturan/detail/3126/undang-undang-nomor-27-tahun-2026> dan JDIH BPK: <https://peraturan.bpk.go.id/Details/359755/uu-no-27-tahun-2026-pelindungan-ketenagakerjaan>.
- PKWT berdasar jangka waktu/pekerjaan tertentu menurut Pasal 82: **jangka awal paling lama 2 tahun; maksimal 2 kali perpanjangan, masing-masing paling lama 1 tahun**. Rencana perpanjangan wajib diberitahukan **secara tertulis paling lambat 7 hari sebelum kontrak berakhir**. **Bukan** aturan legacy 5 tahun/60 bulan untuk kontrak yang tunduk pada rezim baru.
- PKWT **tidak boleh** mengatur masa percobaan (Pasal 86). PKWTT **dapat** mensyaratkan probation paling lama 3 bulan jika dicantumkan secara sah (Pasal 92). Pertahankan batasan internal `HRD-DEC-085` untuk PKWTT awal; jangan membuat probation baru saat konversi PKWT -> PKWTT.
- PKWT harus tertulis dalam bahasa Indonesia dan huruf latin (Pasal 83); validasi jenis/sifat pekerjaan yang memenuhi syarat PKWT diperlukan (Pasal 87). Penafsiran kontrak sebelum tanggal berlaku, aturan transisi, dan peraturan pelaksanaan wajib ditelaah oleh Legal/HRD berwenang. **Jangan retroaktif mengonversi, membatalkan, atau mengubah kontrak historis** lewat skrip otomatis.
- Pastikan kompensasi PKWT/pengakhiran melalui domain Finance/Payroll berwenang; jangan mengimplementasikan rumus keuangan sendiri dari dugaan.
- **Konflik dengan `HRD-DEC-089` historis harus disinkronkan secara terdokumentasi dalam `MODULE BLUEPRINT MODE` yang sah**: pertahankan riwayat keputusan, beri status `NEEDS_SYNC` lalu rujukan aturan terbaru dan alasan superseding untuk bagian 5 tahun saja; jangan diam-diam mengubah blueprint di BACKEND MODE.

### 2.2A PATCH AUDIT MUTAKHIR - status implementasi dan 18 defect terarah (SOURCE VERIFIED, RUNTIME NOT VERIFIED)

**PENTING:** Bagian 2.2A ini menggantikan seluruh deskripsi 'belum ada menu/worklist/policy' pada snapshot awal v1.1. Setelah push 18 November 2026, **keempat Submenu Menu 08, route baru, policy service PKWT, summary API, dan sebagian guard backend MEMANG ADA**. Jangan membuat ulang file/endpoint yang sama. Tugas Agent adalah **memperbaiki implementasi existing** sampai acceptance terpenuhi. Hasil audit code tidak sama dengan UAT live. Saat membuka workspace, ulangi inspeksi hanya pada file yang berubah sejak SHA pin, bukan mengulang analisis bisnis dari awal. Semua requirement yang telah disepakati tetap terkunci.

**Audit repository:**
- FE `DevBenari/QuilvianSystemFrontendDev` @ `HamzahV2`, HEAD `89c070547b80f74e3e1b7b2c464c8c096c3478da`; perubahan terakhir dari `16ce992...` mengenai Menu 07, bukan source utama Menu 08.
- BE `DevBenari/NewQuilvianSystemBackend` @ `MHamzah`, HEAD `daccac931cc06fd6dee572a26a3237486f3aa8a9`; perubahan terakhir dari `a2b6aa...` mengenai Menu 07, bukan service lifecycle Menu 08.
- Rules `DevBenari/QuilvianEngineeringSkills` @ `main`, HEAD `201af64552b27873fd9af83c6722cf11b441bb1e`.
- Laporan Agent yang HARUS dikoreksi sesuai hasil retest: `docs/module-blueprints/human-resource/docs/update/kontrak-dan-masa-kerja/LAPORAN_EKSEKUSI_HRD_MENU_08_KONTRAK_DAN_MASA_KERJA.md`. Laporan menyebut `VERIFIED WORKING`, `40 UAT + 10 regresi PASS`, tetapi belum menyertakan bukti browser/DB per kasus yang dapat diperiksa. Jangan salin klaim PASS tersebut.
- Dokumen instruksi yang sudah dipush: `docs/module-blueprints/human-resource/docs/update/kontrak-dan-masa-kerja/INSTRUKSI_AGENT_HRD_MENU_08_KONTRAK_DAN_MASA_KERJA.md`. **Perbarui file ini, bukan membuat prompt paralel**.

| Audit ID | Severity | File/simbol aktual | Cacat dan konsekuensi | Task pemilik |
| --- | --- | --- | --- | --- |
| AUD-08-01 | P0 | `WorkforceCore/Services/WfpContractHistoryWriteService.cs` `ActivateContractAsync`; `LifecycleManagement/Services/HrdEmploymentLifecycleSchedulerService.cs` | Writer mengisi `ContractStatus="Active"`, `IsCurrent=true` langsung saat tanda tangan; scheduler mencari `ContractStatusSigned` (`"Signed"`) dan `!IsCurrent`. Kontrak masa depan/expiry tidak sinkron | BE-002/007/008/010 |
| AUD-08-02 | P0 | `HrdEmploymentLifecycleWorkflowIntegrationService.HandleWorkflowCompletedAsync` dan `HrdEmploymentContractService.ApplyDecisionAsync` | Callback T16 mengubah Approved/Rejected langsung, tanpa guard maker-checker dan tanpa membuat draft follow-up | BE-005/007/010 |
| AUD-08-03 | P0 | `HrdEmploymentContractService.CreateFollowUpDraftAsync`; `WfpContractHistoryWriteService.CreateRenewalContractAsync` | HistoryType `Conversion` ditimpa menjadi `Renewal`; jejak status PKWT ke PKWTT tidak akurat | BE-003/007/010 |
| AUD-08-04 | P0 | `HrdEmploymentContractService.UpdateDecisionAsync` | Endpoint PUT membolehkan perubahan `DecisionType`, tanggal dan durasi pada `Submitted` tanpa memanggil ulang policy validator `SubmitDecisionAsync` | BE-001/003/005/010 |
| AUD-08-05 | P0 | `HrdFixedTermPolicyService.CalculateMonthsBetween` | Selisih bulan kalender mengabaikan sisa hari; periode >24 bulan dapat dihitung tepat 24 bulan. Risiko PKWT melampaui batas | BE-001/003/010 |
| AUD-08-06 | P0 | `HrdEmploymentContractService.ApplyDecisionAsync`, `SubmitDecisionAsync` | Status, follow-up dan handoff belum ditutup atomik; guard retry Approved+follow-up tidak mencakup NotRenew; race dua request berpotensi membuat side effect ganda | BE-003/005/007/010 |
| AUD-08-07 | P0 | `RecordEmployeeNotifiedAsync`, `RecordEmployeeNotifiedRequest` | `EvidenceNotes` opsional, tidak ada bukti dokumen wajib. `ON_TIME` dihitung dari tanggal manual, tidak membuktikan pengiriman tertulis | BE-004/010 |
| AUD-08-08 | P0 | `HrdEmploymentContractController.CheckViewAmountPermission` endpoint `/salary-detail` | API yang dilaporkan memberikan detail gaji nyatanya hanya mengembalikan `ApiResponse<bool>(true)`; kontrak endpoint/dokumentasi tidak sesuai | BE-002/009/010 |
| AUD-08-09 | P0 | `RecordSignedAsync` dan `WfpContractHistoryWriteService.ActivateContractAsync` | Preflight dilakukan sesudah upload scan, `offer` nullable dapat melewati validasi initial; `ContractStatus Draft` dicek di luar transaksi dan writer tanpa guard ulang, idempotensi dua request belum terbukti | BE-002/006/009/010 |
| AUD-08-10 | P1 (P0 bila akses lintas site) | `GetPrintDataAsync`, `GetByIdAsync`, `DownloadScanAsync`, decision detail | ViewAmount mask sudah ada, tetapi akses Read belum membuktikan site/unit/owner scoping; NIK penuh bisa terekspos ke role yang hanya punya Read | BE-002/009/010 |
| AUD-08-11 | P1 | `GetContractSummaryAsync`, `GetRenewalSummaryAsync`, `GetConversionSummaryAsync` | Summary kontrak expiring dihitung H-30, FE menulis H-60; `hospitalSiteId` diterima tapi tidak diterapkan pada query | BE-008/009/010 |
| AUD-08-12 | P0 integrasi | FE `employment-contracts-list-view.jsx`; BE `EmploymentContractSummaryResponse` | FE `activeCount`,`awaitingSignatureCount`,`reviewNeededCount` tidak sama dengan BE `activeContractsCount`,`pendingSignedCount`,`needsReviewCount` | BE-009/FE-002/FE-008 |
| AUD-08-13 | P0 integrasi | FE `contract-renewals-list-view.jsx`; BE `ContractRenewalSummaryResponse` | FE `due60DaysCount`,`due14DaysCount`,`due7DaysNoticeCount`,`overdueCount`; BE `reminderH60Count`,`escalationH14Count`,`noticeDueH7Count`,`pendingDecisionCount`. Jangan memetakan dua metrik berbeda sebagai sinonim | BE-008/009/FE-004/FE-008 |
| AUD-08-14 | P0 integrasi | FE `employment-status-changes-view.jsx`; BE `EmploymentStatusChangeSummaryResponse` | FE `approvedCount`,`effectiveCount`,`rejectedCount`; BE hanya `pendingSignedCount`,`scheduledOrEffectiveCount`, tanpa rejected. Perlu definisi state + DTO/adapter yang benar | BE-007/008/009/FE-006/FE-008 |
| AUD-08-15 | P0 integrasi | Dua FE worklist baru; BE `ContractDecisionResponse` | FE menggunakan `employeeFullName`, `employeeWorkforceCode`, `employmentContractId`, `noticeCompliance`; BE menggunakan `fullName`, `workforceCode`, `contractHistoryId`, `notificationComplianceStatus`. Link dapat jadi `/undefined` | BE-009/FE-004/FE-006/FE-008 |
| AUD-08-16 | P1 integrasi | FE `contract-renewals-list-view.jsx`; BE `ContractDecisionResponse` | FE memakai `renewalSequence || 1`, tetapi DTO keputusan tidak menyediakan `renewalSequence`. Jangan tampilkan urutan palsu | BE-003/009/FE-004/FE-008 |
| AUD-08-17 | P1 | `HrdEmploymentLifecycleSchedulerService.ExecuteDailyTickAsync`, hosted service | `DateTime.UtcNow.Date` dipakai sebagai hari operasional RS Indonesia; multi-instance idempotensi dan transisi current belum terbukti. Worker initial hires dapat terlewati karena AUD-08-01 | BE-007/008/009/010 |
| AUD-08-18 | P0 evidence | Report Agent dan `task/report/{backend,frontend}` | Klaim seluruh 50 kasus PASS tanpa artefak hasil per ID; `MEMORY_SYNCED` dilaporkan namun `docs/module-blueprints/human-resource/memory.md` tidak ditemukan pada HEAD yang diaudit | BE-010/FE-008; reporting |

**Patch existing yang SUDAH BENAR sebagian, WAJIB DIPERTAHANKAN:** 4 submenu dan 2 route baru, `HrdFixedTermPolicyService`, `HrdProbationService` guard `RequiresEndDate`, validasi grade/struktur gaji, penghapusan fallback `DepartmentId=organizationUnitId`, masking BasicSalary di print API, cleanup attachment saat gagal, maker-checker pada salah satu service, dan guard future-dated mirror pada `WfpNewHireAssignmentService`. Jangan revert manfaat tersebut. Perbaiki jalur bypass/celahnya saja.

### 2.2B Invariant bisnis yang harus dipenuhi sebelum Agent menandai DONE

1. **Status hubungan kerja tanggal efektif, bukan tombol.** `Draft` = belum bertanda tangan; `Signed` = dokumen telah ditandatangani tetapi belum efektif; `Active` = masa kerja mulai dan kontrak current; `Completed`/`Ended`/`LapsedWithoutDecision` mengikuti keputusan HRD/Legal serta histori. Pada tanggal mulai mendatang, signed contract tidak mematikan current contract sebelumnya. Jika canonical mensyaratkan state berbeda, pertahankan label yang disetujui dengan perilaku ekuivalen dan dokumentasikan migrasi status lama secara non-destruktif; jangan ubah DB langsung.
2. **Satu active/current yang sah untuk setiap pegawai sesuai domain.** Permintaan tanda tangan, workflow callback, scheduler dan retry tidak boleh menghasilkan dua current kontrak, dua follow-up, dua probation awal atau dua salary assignment yang bertentangan. Mekanisme wajib database-robust, bukan `if(status != Draft)` sebelum transaksi atau lock memori saja.
3. **Approval workflow nyata.** Pembuat keputusan tidak menyetujui sendiri, callback tidak melewati service orkestrasi, setiap perubahan status memiliki aktor, timestamp, bukti dan konsekuensi follow-up. Jika instance T16 ternyata hanya GUID placeholder, benahi integrasi menggunakan workflow engine canonical atau blok acceptance hingga integrasi riil terverifikasi. Hindari dependency injection circular; gunakan orkestrasi yang kompatibel.
4. **Masa kerja/konversi tidak mengubah Payroll/Position/Department lebih awal.** Penempatan awal dari approved offer boleh membentuk assignment bertanggal masa depan, tetapi mirror saat efektif. Konversi PKWT ke PKWTT bukan mutasi unit atau kenaikan gaji otomatis. `HistoryType=Conversion` harus persist apa adanya.
5. **Tidak ada kepatuhan palsu.** Pemberitahuan H-7 memerlukan evidence tertulis yang dapat diperiksa, bukan hanya tanggal/catatan manual. Kalkulator PKWT menghitung durasi berdasarkan boundary tanggal yang tepat dan versi kebijakan hukum; transisi historis perlu pemeriksaan legal bila data/aturan tidak pasti.
6. **FE-BE sama kontraknya.** Setiap field summary, worklist, url, status dan permission harus berasal dari DTO aktual atau adapter eksplisit dengan semantik identik. Tidak boleh `undefined`, metrik nol palsu, fallback status `Ke-1`, atau toast sukses bila API error.
7. **Privasi RS dijaga dari API.** ViewAmount, scope legalEntity/site/unit/employee, NIK dan file scan ditentukan di server. Menyembunyikan tampilan frontend saja bukan authorization.
8. **Bukti verifikasi harus nyata.** `dotnet build`/ESLint adalah gate kompilasi dan lint, BUKAN UAT, integrasi HTTP, DB persistence atau browser test. Semua kasus tanpa eksekusi diberi `NOT RUN/BLOCKED`.

**Sumber hukum resmi yang boleh dirujuk dalam penilaian:** JDIH Kementerian Ketenagakerjaan, <https://jdih.kemnaker.go.id/peraturan/detail/3126/undang-undang-nomor-27-tahun-2026> (UU No. 27/2026, 28 Oktober 2026). Verifikasi pasal, ketentuan peralihan, kategori pekerjaan dan aturan turunan pada dokumen resmi + keputusan Legal RS sebelum mengubah data historis; bukan mengarang tafsir legal baru. Baca keputusan HRD-DEC-089 bersama legal policy hasil push. Uji aturan 24 bulan + 1 hari secara kalender, bukan sekadar label Pasal.

### 2.3 Detail implementasi per Submenu

### Submenu 1 - Perjanjian Kerja

**Tujuan dan aktor:** HRD mengelola kontrak pekerja rumah sakit (perawat, tenaga farmasi, tenaga nonmedis, dsb. sesuai cakupan hubungan kerja), atasan/unit melihat ruang lingkup yang diizinkan, bagian Legal/pejabat rumah sakit memeriksa aspek formal, pegawai menerima dan menandatangani. HRD tidak menentukan kewenangan klinis dari kontrak semata.

**Input dan sumber:** `WorkforceProfileId`, tawaran final yang sudah disetujui, `MstContractType`, `EmploymentTypeId`, nomor kontrak resmi, `StartDate/EndDate`, referensi jabatan/unit, tanggal tanda tangan, scan PDF bertanda tangan, status validasi hukum, dan histori. Jangan meminta pegawai mengetik ulang identitas yang ada di Menu 04 atau nominal dari tawaran yang sudah disetujui.

**Alur wajib:**

1. Rekrutmen/offer approved -> sistem membuat satu draft kontrak pertama melalui `HrdEmploymentContractService.CreateDraftFromOfferAsync` dan `WfpContractHistory`; idempotent terhadap event onboarding berulang.
2. Server menentukan klasifikasi PKWT/PKWTT dari master kontrak terkontrol, memeriksa tanggal, sifat pekerjaan dan rule effective date, serta menolak master tidak konsisten. Tidak boleh menganggap `RequiresEndDate == true` selalu cukup sebagai satu-satunya bukti kepatuhan hukum.
3. HRD melihat/mengecek draf, mencetak data yang benar, melaksanakan tanda tangan pihak terkait, lalu mengunggah scan melalui `POST /employment-contracts/{id}/record-signed`.
4. Setelah scan sah dan tanggal valid, status kontrak dan histori menjadi authoritative; onboarding/penempatan hanya berlanjut sesuai syarat readiness. Tampilkan kontrak aktif, histori termasuk renewal/konversi/koreksi, audit penandatanganan, dokumen dan alasan perubahan.
5. Koreksi kontrak yang sudah signed harus melalui `HrdContractCorrection`/workflow T17 dan menyimpan versi sebelumnya; batch historis melalui `HrdInitialContractBatch`, dengan penanda konteks data awal, tidak memalsukan kontrak baru.

**Source existing wajib direuse:** FE `src/app/hr/lifecycle/employment-contracts/page.jsx`, `src/app/hr/lifecycle/employment-contracts/[id]/page.jsx`, `src/components/view/hr/lifecycle/employment-contracts-list-view.jsx`, `employment-contract-detail-view.jsx`; `src/lib/services/hr/lifecycle-service.js`. BE `HrdEmploymentContractController`, `HrdEmploymentContractService`, `HrdNumberSeriesService`, `WfpContractHistory`, `MstContractType`, `HrdContractCorrection`, kontrak DTO/attachment service. Semua ada pada repository snapshot; uji runtime **NOT VERIFIED**.

**Repair wajib:**

- `GetPrintDataAsync` mengembalikan `BasicSalary` dari offer dan `IsSalaryMasked = false` meskipun route `print-data` hanya memeriksa `WorkforceEmploymentContract.Read`. P0: lakukan pemeriksaan permission nominal yang benar di **backend**; `BasicSalary = null` atau masked sesuai kontrak ketika user tak berizin; jangan mengirim nominal tersembunyi dalam payload. Audit juga endpoint download PDF/dokumen dan metadata sensitif.
- Sumber fallback print berisi nama pemberi kerja `Rumah Sakit Quilvian`, `EmployeeNationalId` dari `ProfileCode`, dan tipe kontrak fallback `PKWT`: cegah data cetak dokumen hukum berisi data salah/placeholder; gunakan master institusi/identitas terverifikasi dan error yang jelas ketika data wajib tidak tersedia.
- Hindari menganggap `GET print-data` sebagai kontrak final legal yang otomatis sah; tetap perlu perjanjian dan tanda tangan valid.
- Pastikan `StartDate <= EndDate` hanya bila memiliki end date, wajib end date untuk PKWT menurut klasifikasi; PKWTT tidak dipaksa mengisi tanggal akhir PKWT; nomor dokumen unik; tidak ada dua kontrak definitif aktif pada satu pegawai dengan periode bertabrakan.
- Untuk data awal lama, jangan mewajibkan penandatanganan ulang historis secara tidak benar; berikan status kelengkapan/dokumen tertunda yang faktual.

**Target FE:** daftar dengan SummaryGrid, filter jenis/unit/status/akhir kontrak, tabel status dengan nomor dokumen dan H-nya; detail dengan tab `Informasi`, `Dokumen`, `Riwayat`, `Keputusan`, `Audit`; tombol `Lihat`, `Cetak`, `Catat Tanda Tangan`, `Ajukan Koreksi` hanya bila role+state diizinkan. Tunjukkan masked salary jika tidak berizin; tidak ada data dummy.

**Dampak integrasi Menu 07 pada Submenu ini:** `RecordSignedAsync` memanggil `WfpNewHireAssignmentService`; validasi salary grade/structure, department/position FK, tanggal efektif dan rollback harus selesai sebelum mengakui kontrak Signed. Jangan memperlakukan perubahan gaji akibat SK Menu 07 sebagai bagian signing kontrak pertama.

**UAT / acceptance:** kontrak pertama dari approved offer, signed tanpa PDF -> 4xx, signed sah -> persist reload, dokumen tidak bocor, dua kali signed ditolak/idempotent sesuai kontrak; dokumen print tidak memakai placeholder palsu; histori tetap utuh dan status current tunggal setelah pergantian. Tambahan wajib `REG-HRD-08-001`..`007` untuk kontrak pertama/penempatan/gaji. Lihat `AT-HRD-08-001` s.d. `AT-HRD-08-010`.

### Submenu 2 - Perpanjangan Kontrak

**Tujuan dan aktor:** kepala unit/atasan langsung mengajukan kelanjutan kontrak; HRD lain menyetujui/minta revisi/menolak; pegawai menerima pemberitahuan dan menandatangani kontrak lanjutan; Legal dapat memeriksa ketentuan masa berlaku tanpa membuat persetujuan bertingkat baru.

**Sumber dan data:** kontrak signed/current dan riwayat `PreviousContractHistoryId`, `RenewalSequence`, periode, jenis pekerjaan, `DecisionType.Extend`, jumlah perpanjangan sebelumnya, total masa, target start/end, alasan, nomor keputusan, workflow T16, bukti pemberitahuan tertulis/tanggal kirim, kontrak follow-up.

**Alur wajib:**

1. Scheduler/worklist membaca kontrak menjelang akhir, memicu pengingat internal H-60 dan eskalasi H-14 sesuai `HrdEmploymentLifecycleSetting` (bisa dikonfigurasi); H-7 adalah **tenggat hukum untuk pemberitahuan tertulis perpanjangan** bila memang akan diperpanjang, bukan pengingat internal pengganti.
2. Atasan/HRD membuka `/hr/lifecycle/contract-renewals`, melihat sisa dan riwayat berdasarkan legal policy version, bukan lagi angka 60 bulan. Server mengembalikan alasan `canExtend`, `maxExtensionMonths`, `renewalCount`, ruleVersion/legal basis, `requiredNoticeDate` dan alasan penolakan.
3. Pengaju memilih `Extend`, mengisi tanggal awal dan akhir, alasan; server memvalidasi tanggal kontinu tanpa overlap/gap tidak sah, jumlah perpanjangan maksimal, durasi tiap perpanjangan, sifat pekerjaan, bukti notifikasi, status kontrak dan rowVersion. `ExtensionMonths` wajib konsisten dengan tanggal, hindari aritmetika bulan parsial dengan mengurangi angka `Month` saja.
4. `HRD-DEC-081`: satu pengaju, HRD checker berbeda memutuskan; approved -> `CreateFollowUpDraftAsync` persisten satu kali, tersambung ke kontrak asal; **bukan** langsung mengganti status current sebelum kontrak lanjutan ditandatangani dan efektif.
5. HRD mencatat bukti pemberitahuan **tertulis** kepada pegawai, dengan tanggal/jam, metode, penerima, dokumen dan identitas aktor. Log audit server harus memisahkan `NotifiedAt` dari waktu input. Jika lewat H-7, tandai pelanggaran dan eskalasi Legal/HRD; jangan otomatis memundurkan tanggal/menandai patuh.
6. Kontrak lanjutan ditandatangani -> aktif pada tanggal efektif -> kontrak sebelumnya selesai pada batas relevan; tidak boleh dua current. Jika tidak diperpanjang, gunakan `NotRenew` existing -> handoff Menu 12, bukan membuat clearance baru.

**Source existing wajib direuse:** BE `HrdEmploymentContractService.FixedTermCheckAsync` (baris sekitar 432), `SubmitDecisionAsync` (sekitar 491), `RecordEmployeeNotifiedAsync` (sekitar 676), `ApplyDecisionAsync` (sekitar 704), `CreateFollowUpDraftAsync` (sekitar 768); `HrdContractDecision`, `HrdEmploymentLifecycleSchedulerService`, `HrdEmploymentLifecycleSetting` dan `HrdEmploymentLifecycleWorkflowIntegrationService`. FE `employment-contract-detail-view.jsx` (dialog keputusan), `LifecycleService.submitContractDecision`, `getFixedTermCheck` dan konstanta decision. Existing tidak membuktikan halaman daftar perpanjangan dedicated.

**Repair wajib P0:**

- `FixedTermCheckAsync` memakai `const int maxMonths = 60; // 5 tahun`; `SubmitDecisionAsync` membandingkan >60; ganti kedua jalur dengan **satu service rule resmi**, berversi dan berlaku berdasarkan kontrak, termasuk 2 tahun awal + 2x 1 tahun untuk peristiwa baru yang tunduk UU 27/2026. Aturan transisi historis butuh Legal review, jangan eksekusi mass update.
- `RecordEmployeeNotifiedAsync` menyimpan `EmployeeNotifiedOn` tanpa bukti tertulis/validasi status/tenggat yang cukup; tambah kontrak evidensi yang aman; jangan menyamakan input tanggal dengan bukti legal.
- `ApplyDecisionAsync` dan `CreateFollowUpDraftAsync` harus atomik dan idempotent, aman callback workflow ganda; nomor kontrak `parent-R{seq}` tidak boleh bentrok/truncation collision; tidak boleh menghasilkan draft ganda.
- Backend GET existing `/{id:guid}/decisions/{decisionId:guid}` bukan endpoint daftar keputusan. Untuk halaman worklist dedicated, tambah API agregasi/filter/paging jika memang belum ada; beri route **PROPOSED** yang mengikuti API rules, jangan mengarang endpoint existing.
- Status late/expired tanpa keputusan harus tersambung dengan jadwal kerja; belum signed -> tidak dianggap perpanjangan sah. Jangan menutup kontrak secara otomatis menjadi hubungan kerja berakhir final jika memerlukan keputusan legal/HRD.

**Target FE:** dashboard kontrak jatuh tempo, filter `Unit/Status/Interval/Approval`, tabel dengan `Perpanjangan Ke-`, `Tgl Akhir`, `Sisa Durasi`, `Bukti Notifikasi`, `Status Keputusan`; detail perbandingan kontrak sebelum/sesudah; wizard ringan form perpanjangan; panel hasil kelayakan server, due date pemberitahuan, lampiran, timeline workflow. Jangan menyembunyikan status pelanggaran H-7.

**UAT / acceptance:** kontrak awal 2 tahun diperpanjang sesuai rule; perpanjangan ketiga ditolak; perpanjangan masing-masing di atas satu tahun ditolak; periode bersinggungan atau request tidak konsisten ditolak; pemberitahuan dan bukti tertulis ada dan diverifikasi; approval ganda tidak menggandakan kontrak; signed future effective tidak aktif terlalu dini. Lihat `AT-HRD-08-011` s.d. `AT-HRD-08-020`.

### Submenu 3 - Masa Percobaan

**Tujuan dan aktor:** atasan langsung menilai pegawai PKWTT baru, HRD berbeda dari pengaju mengesahkan, sistem mengingatkan dua minggu sebelumnya dan menutup kasus sesuai `HRD-DEC-085`. Komite klinis tetap pemilik kewenangan klinis, bukan menjadi penilai probation default yang membebani alur rumah sakit.

**Input dan sumber:** kontrak PKWTT pertama yang sah mencantumkan syarat masa percobaan, identitas atasan dari penempatan efektif, `HrdProbation`, `HrdProbationAssessment`, periode start/end 3 bulan, hasil `Pass/Fail`, alasan, lampiran wajib untuk `Fail`, rowVersion, status workflow T15.

**Alur wajib:** kontrak PKWTT pertama signed dan probation memang tercantum sah -> buat satu probation dari tanggal mulai yang benar -> atasan mendapat reminder H-14 -> menilai Lulus/Tidak Lulus -> HRD Setujui/Minta Revisi/Tolak -> jika Lulus, tutup probation dan sinkronkan profil sesuai kontrak; jika Tidak Lulus disetujui, handoff kepada Menu 12, tanpa exit flow kedua; jika tanggal akhir lewat tanpa keputusan sah, sistem `PassedAutomatically` dengan audit, notifikasi HRD/atasan, dan tutup penilaian terbuka.

**Source existing wajib direuse:** BE `HrdProbationService.CreateForContractAsync`, `SubmitAssessmentAsync`, `ApplyAssessmentDecisionAsync`, `HrdProbationController`, `HrdEmploymentLifecycleSchedulerService`, `HrdProbation`/`HrdProbationAssessment`. FE `/hr/lifecycle/probations`, `probations-list-view.jsx`, `probation-detail-view.jsx`, service dan constants. EXISTING SOURCE, runtime belum diverifikasi.

**Repair wajib P0:**

- `CreateForContractAsync` hanya mengandalkan `MstContractType.IsProbationApplicable` + `HistoryType == Initial`; tegakkan pengecualian tegas PKWT di backend bahkan jika master salah, dan pastikan probation PKWTT **memang disyaratkan pada perjanjian/surat pengangkatan**, bukan otomatis pada setiap PKWTT. Kebijakan `HRD-DEC-085` untuk PKWTT awal berlaku selama syarat tercantum sah; jika master tak menyimpan bukti syarat, gunakan dokumen authoritative atau usulkan perluasan kontrak via mode yang sah.
- Saat `CreateDraftFromOfferAsync`, `ProbationEndDate` berdasarkan `DefaultDurationMonths ?? 3` harus diselaraskan dengan aturan 3 bulan dan bukti persyaratan; jangan membuat tanggal probation untuk PKWT.
- Jangan buat probation baru untuk renewal atau PKWT -> PKWTT; `WfpContractHistory.HistoryType` dan chain harus menjadi guard.
- Periksa scheduler `PassedAutomatically` untuk deadline berdasarkan kalender RS/site, audit aktor sistem, notifikasi, idempotensi/retry dan race dengan HRD yang sedang memutuskan. Jangan menganggap status otomatis setara bukti UAT sampai job sungguh dijalankan.
- Lulus Tidak Lulus memerlukan lampiran sesuai kebijakan; jangan izinkan approval yang sudah ditutup otomatis pada tanggal berikutnya; kasus gagal tidak menghapus rekam kerja.

**Target FE:** list summary empat indikator (berjalan/perlu evaluasi/menunggu HRD/selesai), filter unit/atasan/status/due; detail dengan pegawai, kontrak, timeline 3 bulan, hasil Lulus/Tidak Lulus, alasan, file bukti (wajib jika Tidak Lulus), status persetujuan dan histori; **tanpa** tombol perpanjang probation. Jangan tampilkan form untuk pegawai PKWT.

**Dampak integrasi Menu 07:** probation dibuat setelah initial placement dan salary pada jalur kontrak pertama. Kegagalan validasi salary/department wajib me-rollback probation, Signed state dan checklist onboarding secara menyeluruh, bukan menyisakan `HrdProbation.Running` orphan. Proses konversi/perpanjangan dan mutasi SK tidak memicu probation baru. Status probation diturunkan dari kontrak yang benar-benar sah.

**UAT / acceptance:** PKWT tidak dapat membuat probation walaupun master salah; PKWTT first contract dengan syarat sah mendapat probation tepat 3 bulan; PKWTT tanpa klausul sah tidak mendapat probation; conversion tidak membuat probation; fail tanpa bukti ditolak; maker checker; auto-pass on expiry bisa diaudit dan status persist. Tambahan `REG-HRD-08-002`,`006`,`007`. Lihat `AT-HRD-08-021` s.d. `AT-HRD-08-028`.

### Submenu 4 - Perubahan Status Kerja

**Tujuan dan aktor:** atasan mengajukan pengangkatan status PKWT ke PKWTT, HRD menyetujui berdasarkan masa kerja, evaluasi dan legal eligibility, pihak rumah sakit/pegawai menandatangani kontrak baru; profil kepegawaian berubah **pada tanggal efektif**. Ini bukan proses mutasi jabatan dan bukan menu pemberhentian.

**Sumber dan input:** `WfpContractHistory` yang current, status dan jenis kontrak, `ContractDecisionType.ConvertToPermanent` (enum value `2`), `HrdContractDecision`, `FollowUpContractHistoryId`, dokumen/nomor kontrak lanjutan, tanggal mulai PKWTT, alasan, persetujuan, audit.

**Alur wajib:** usulan dari kontrak PKWT -> validasi status dan periode/riwayat -> satu HRD checker menyetujui -> draft PKWTT baru dibuat terhubung riwayat -> HRD mencatat dokumen bertanda tangan -> scheduler mengaktifkan sesuai tanggal efektif -> `EmploymentType`/kontrak efektif dan proyeksi profil disinkronkan atomik -> publikasi event downstream melalui kontrak terverifikasi tanpa mengubah gaji otomatis.

**Source existing wajib direuse:** `HrdEmploymentContractService.ApplyDecisionAsync`, `CreateFollowUpDraftAsync`, enum `ContractDecisionType.ConvertToPermanent`, `WfpContractHistory.PreviousContractHistoryId` dan `HistoryType = Conversion`, `HrdEmploymentLifecycleSchedulerService`, `WfpEmploymentRecord`/sync bila sumber membuktikan. FE sebelumnya mengajukan `decisionType:2` dari detail kontrak; target dedicated halaman baru `/hr/lifecycle/employment-status-changes` hanya konteks UI.

**Repair wajib P0:**

- `CreateFollowUpDraftAsync` mengambil `targetContractTypeId = parent.ContractTypeId` lalu mencari type tanpa end date, tetapi **jika tidak ketemu** bisa tetap memakai PKWT. Ubah ke fail-closed dengan pesan master PKWTT tidak tersedia/tidak valid. Jenis kontrak PKWTT harus eksplisit, aktif, sesuai master, dan tidak memiliki tanggal akhir PKWT.
- Jangan mengganti `MstWorkforceProfile` atau `IsCurrent` pada tanggal approval saja. Bedakan `Approved` -> `FollowUpDraft` -> `Signed` -> `Scheduled`/`Effective`. Validasi benturan tanggal, satu current per site/pegawai sesuai pola existing, status previous, audit, race concurrency.
- Jika kontrak PKWT tidak memenuhi syarat hukum, berikan alur kasus **rekonsiliasi kepatuhan** dan referensi Legal/HRD, bukan tombol manual yang berpura-pura membuat semua pelanggaran sah; jangan memigrasikan riwayat historis otomatis.
- Tidak ada probation baru saat conversion; tidak ada perubahan gaji langsung; tidak mengubah jabatan/unit melalui Menu 08.

**Target FE:** daftar pegawai dengan riwayat status, filter, detail sebelum -> sesudah; form tanggal efektif + alasan, validasi dan status approval; panel dokumen PKWTT lanjutan; timeline status per tanggal; tag `Menunggu TTD`/`Dijadwalkan`/`Berlaku` dari server, serta tautan ke profil pegawai.

**UAT / acceptance:** master PKWTT kosong membuat API gagal jelas tanpa mutasi; conversion approved tidak langsung efektif jika tanggal kemudian; setelah signed dan worker efektif status/riwayat benar; tidak ada probation/penyesuaian gaji otomatis; refresh/restart server tetap konsisten. Lihat `AT-HRD-08-029` s.d. `AT-HRD-08-036`.

### 2.4 Urutan task BACKEND-FIRST - 10 task FIN

**Instruksi:** Jalankan task `FIN-HRD-08-BE-001` sampai `FIN-HRD-08-BE-010` berurutan sesuai dependency. Untuk setiap task: invoke `build-module-backend`, baca implementasi terkait dan aturan kode, ubah source nyata, tambahkan/ubah tes scoped, jalankan pemeriksaan yang diizinkan, perbaiki regresi, buat laporan task. Jangan menyamakan hasil `dotnet build` dengan UAT.

#### FIN-HRD-08-BE-001 - Legal policy version dan sinkronisasi keputusan (P0)

**Target:** `HrdEmploymentContractService.FixedTermCheckAsync`, `SubmitDecisionAsync`, master contract, DTO; blueprint `HRD-DEC-089` hanya dalam mode dokumen sah.

- Buat **service tunggal rule PKWT berversi** atau perluas policy existing dengan `EffectiveFrom`, identitas aturan/sumber, `InitialMaxMonths`, `MaxRenewalCount`, `MaxMonthsPerRenewal`, `NoticeLeadDays`, serta klasifikasi historis/transisi. Tidak perlu tabel baru jika repository punya provider konfigurasi yang cukup; kalau perlu schema baru buat rencana migration dan tunggu izin terpisah.
- Implementasikan rule baru untuk peristiwa kontrak yang tunduk UU 27/2026; tolak perpanjangan ketiga dan perpanjangan >12 bulan; cegah masa awal >24 bulan serta sifat pekerjaan non-PKWT. **Jangan** hardcode regulasi 60 bulan sebagai aturan baru.
- Sediakan deterministic result yang menjelaskan ruleVersion, legal basis, batas per perpanjangan, jumlah yang telah dipakai, cause blocked dan warning; cocok untuk form FE. Kasus law-transition historis yang memerlukan pendapat hukum tidak boleh diberi hasil `eligible` palsu.
- Dokumentasikan perbedaan `HRD-DEC-089` existing vs regulasi terbaru, rujuk keputusan user yang menyetujui planning Menu 08 dan sumber resmi. Sinkronisasi canonical hanya lewat dokumen berizin; jika tidak berizin beri status `NEEDS_SYNC` di report, tetapi implementasikan guard non-destruktif yang jelas untuk kasus baru sesuai kontrak yang sah.
- **Accept:** unit test awal 24 bulan, perpanjangan pertama/kedua masing-masing <=12 bulan, perpanjangan ketiga dan awal 25 bulan ditolak; old records tidak dimutasi.

#### FIN-HRD-08-BE-002 - Perjanjian kerja dan keamanan print (P0)

**Target:** `HrdEmploymentContractController/Service` khususnya `RecordSignedAsync` dan `GetPrintDataAsync`, `EmploymentContractDtos`, `WfpContractHistory`, `HrdInitialContractBatch`/attachment, permission terkait; integrasi terarah ke `WorkforceCore/Services/WfpNewHireAssignmentService.cs` dan master offer/grade/site dengan izin lintas domain yang benar.

- Pertahankan draft dari approved offer (`HRD-DEC-084`); validasi PKWT vs PKWTT, dokumen wajib, nomor unik, kontrak current tunggal, jenis pekerjaan, tanggal, penandatanganan, audit.
- **Integrasi baru Menu 07 (P0):** sebelum `RecordSignedAsync` melakukan aktivasi, pastikan site/organization unit/department/position valid dan konsisten, grade+structure gaji aktif serta kompatibel dengan **offer yang memang disetujui**. Jangan fallback `DepartmentId=OrganizationUnitId`, jangan fallback `MstSalaryStructure` grade tidak cocok. Gagal validasi wajib menjadi error jelas, dengan rollback end-to-end yang dapat diuji.
- `WfpNewHireAssignmentService.AssignNewHirePlacementAndSalaryAsync` harus menghindari duplikasi assignment; jangan membuat salary assignment awal jika already present sah. Jangan menandai `ApprovedByUserId` seolah petugas pencatat tanda tangan menyetujui tawaran; simpan jejak sumber approval offer. PDF yang sudah di-upload sebelum transaksi tetapi DB rollback memerlukan cleanup aman atau rekonsiliasi file orphan.
- Saat kontrak first-signed future-dated, assignment boleh Scheduled/as-of tetapi mirror profil dan status penempatan current harus **tetap mengikuti tanggal efektif** (`HRD-DEC-047`), bukan waktu user klik `Catat Tanda Tangan`.
- Perbaiki `GetPrintDataAsync`: **pemeriksaan izin melihat nominal di backend**; jangan mengembalikan BasicSalary jika role tidak berwenang. Pakai permission amount canonical dari source/role registry yang terbukti, bukan nama action karangan tanpa registrasi. Uji scope site/unit, PDF download, dan response API.
- Hapus fallback hukum palsu `Rumah Sakit Quilvian`, `EmployeeNationalId = ProfileCode`, dan default jenis PKWT. Bila data wajib kurang, tampilkan `dataIncomplete` dan larang menerbitkan dokumen final sampai diperbaiki dari master; jangan melakukan insert data dummy.
- Pastikan PDF attachment tervalidasi tipe ukuran/otorisasi/privasi, metadata/signature timestamp disimpan, akses cetak sesuai kewenangan.
- **Accept:** tanpa signed file tetap Draft; print tanpa ViewAmount tidak memperoleh nominal; dokumen menampilkan identitas institusi terverifikasi; batch historis tidak dianggap kontrak baru.

#### FIN-HRD-08-BE-003 - Kalkulator perpanjangan dan chain histori (P0)

- Gabungkan perhitungan `FixedTermCheckAsync` dan pengajuan `SubmitDecisionAsync` ke validator bersama dari BE-001. Tidak boleh FE dapat `canExtend=true` sementara POST justru menyetujui periode terlarang.
- Rekonstruksi `PreviousContractHistoryId` dan `RenewalSequence` dari histori authoritative; validasi start/end, sela/periode, sifat pekerjaan, total/legal regime, dan status penandatanganan.
- Cegah parallel submit/duplication dan collision nomor follow-up `parent-Rn` bila >50 chars/truncation. Gunakan mekanisme sequence resmi, uniqueness dan optimistic concurrency.
- Tidak mengubah kontrak lama dengan update tanggal; create next record, previous tetap immutable secara historis; koreksi approved tersimpan per versi.
- **Accept:** pemrosesan dua user simultan -> maksimal satu open decision/follow-up sah; semua validasi lewat POST tetap menolak bypass FE.

#### FIN-HRD-08-BE-004 - Bukti pemberitahuan tertulis dan tenggat (P0)

- Perluas `RecordEmployeeNotifiedAsync`/DTO/attachment atau service notifikasi existing dengan bukti surat/artefak, identitas pegawai/penerima, kanal, tanggal kirim, diinput oleh, timestamp penerimaan/tercatat; validasi jenis keputusan (perpanjangan vs non-renew), status, deadline, kemampuan mengaudit.
- Integrasikan pengingat H-60/H-14 existing dan tenggat tertulis H-7 menurut UU; tag `ON_TIME`, `LATE`, `MISSING`, `NOT_APPLICABLE`, dan alasan. Jangan mengisi `employeeNotifiedOn` otomatis tanpa evidence.
- Jika perlu entity persisted baru, patuhi QBE prefix/registry/configuration, bukan modifikasi DB langsung. Kegagalan file/notif tidak boleh menghasilkan false success.
- **Accept:** tanpa bukti tertulis untuk perpanjangan tidak dilaporkan `compliant`; lewat tenggat ditandai dan diekskalasi, bukan dibuat tanggal palsu.

#### FIN-HRD-08-BE-005 - Workflow keputusan, maker-checker, atomicity (P0)

- Kunci state: `Submitted`, `Returned`, `Rejected`, `Approved`, `Withdrawn` dan pemrosesan ulang sesuai workflow T16 existing; tolak tindakan tidak sah, termasuk keputusan oleh pembuat sendiri. Kepala unit hanya lingkup pegawai/unit yang berhak.
- Perbaiki `ApplyDecisionAsync`: approved + draft follow-up + hubungan nomor + event harus konsisten transactional (atau outbox/idempotency bila ada side effect lintas aggregate). Jika callback workflow datang dua kali, tidak boleh muncul dua follow-up. Jika gagal, rollback atau recover terkontrol; tidak cukup memutasi status lokal.
- `UpdateDecisionAsync` hanya update pada state/aktor yang dibolehkan; `WithdrawDecisionAsync` hanya state yang ditetapkan, log alasan/audit. Jangan membuat approval kedua untuk kontrak pertama yang sudah disetujui di offer.
- Jika belum ada endpoint worklist GET terfilter keputusan, tambah endpoint dengan pagination, role scoping, status, jenis dan tanggal; tandai API baru pada laporan, perbarui FE adapter setelah BE siap.
- **Accept:** maker==checker 403/409; callback retry tidak menggandakan kontrak; workflow status dan dokumen konsisten.

#### FIN-HRD-08-BE-006 - Probation PKWTT dan approval (P0)

- Validasi masa percobaan `HrdProbationService.CreateForContractAsync`, `CreateDraftFromOfferAsync`, master type dan syarat tertulis yang disepakati; PKWT dilarang walau `IsProbationApplicable=true`, PKWTT baru hanya jika klausul legal ada, masa percobaan maksimal tiga bulan tanpa extension.
- Auto-case hanya first PKWTT initial, bukan renewal/conversion. Tolak fail tanpa bukti file; simpan status/aktor/waktu; auto-pass saat melewati akhir sesuai `HRD-DEC-085`, namun pemberitahuan dan audit wajib nyata.
- **Regresi kontrak pertama pasca-push Menu 07:** ketika service penempatan/gaji melempar error validasi, `HrdProbation` tidak boleh sudah committed atau terpisah dari kontrak Draft. Uji transaksi yang benar-benar menjalankan `RecordSignedAsync` -> `_newHireAssignmentService` -> `_probationService`; periksa status onboarding, attachment storage dan histori jika exception pada setiap tahap. Gagal setelah upload PDF tidak boleh menghasilkan sukses palsu/attachment orphan tanpa penanganan.
- Pastikan concurrency antara callback HRD dan scheduler tidak menimpa keputusan final, status `PassedAutomatically` tak bisa disetujui ulang, dan tidak ada kehilangan histori.
- **Accept:** PKWT/follow-up conversion tidak memiliki probation, PKWTT eligible 3 bulan; rerun job idempotent; failed->offboarding melalui adapter sah.

#### FIN-HRD-08-BE-007 - Konversi PKWT ke PKWTT (P0)

- `CreateFollowUpDraftAsync` harus fail-closed bila `MstContractType` PKWTT yang aktif/valid tidak ada; jangan fallback memakai `parent.ContractTypeId`, jangan buat `Conversion` dengan end date PKWT.
- Terapkan state approved -> follow-up draft -> signed -> effective, sync `WfpContractHistory`, employment record/profil dan status hubungan kerja saat efektif; pakai optimistic concurrency dan transaksi.
- Simpan `PreviousContractHistoryId`, status sebelum/sesudah, nomor keputusan, tanggal dan aktor. Jangan memicu probation baru, penempatan baru, atau perubahan gaji langsung.
- **Accept:** master PKWTT tidak ada -> error terstruktur dan DB tak berubah; scheduled conversion tidak mengubah status lebih cepat; signed future sesuai tanggal baru benar-benar efektif.

#### FIN-HRD-08-BE-008 - Scheduler, reminder, audit dan masa kerja (P1/P0 keamanan data)

- Audit `HrdEmploymentLifecycleSchedulerHostedService` (check per jam) dan `HrdEmploymentLifecycleSchedulerService.ExecuteDailyTickAsync`; pastikan service diregistrasi dan berjalan, bukan hanya class file. Perhatikan timezone rumah sakit `Asia/Jakarta`/site, penyimpanan UTC, edge tanggal 31/leap year, retry, job locking multi-instance dan rowVersion.
- Status `Signed`, `Active`, `Completed`, `Ended`, `LapsedWithoutDecision`, `Cancelled` harus memiliki semantik konsisten dengan `IsCurrent`. Jangan menghitung pegawai berakhir hubungan kerja hanya karena tidak ada keputusan (perlu Legal/HRD). Jangan mengaktifkan draft belum signed.
- Pengingat H-60/H-14 dan H-7 dibuat server-side, dideduplikasi, disimpan sebagai evidence dan dipresentasikan di worklist. Perhitungan masa kerja dari histori sah, bukan selisih sekali `now - start` jika ada jeda/riwayat.
- **Regresi mirror future-dated Menu 07:** saat kontrak ditandatangani sebelum `StartDate`, `WfpOrganizationAssignment`/`WfpPositionAssignment` menyimpan tanggal yang akan berlaku; `MstWorkforceProfile.PrimaryDepartmentId`/`PrimaryPositionId` **jangan diubah lebih cepat**. Verifikasi worker `PersonnelActionOrderScheduledApplyWorker` yang diregistrasi di `Program.cs` mengurus SK saja; **jangan menganggap otomatis juga mengurus initial hires**. Gunakan/read as-of resolver yang tepat dan implementasikan proyeksi mirror saat efektif dengan retry-idempotensi; uji lintas pergantian hari WIB, beberapa instance, serta current prior assignment.
- **Accept:** worker dijalankan dua kali menghasilkan state yang sama; satu kontrak current; tiada aktivasi sebelum tanggal; lapsed menahan scheduling sesuai kontrak lintas domain yang ada.

#### FIN-HRD-08-BE-009 - Integrasi dan perlindungan data lintas domain (P0)

- Domain terkait: Rekrutmen approved offer, Menu 04 Data Induk Pegawai, Menu 06 SK/Koreksi, Menu 07 Penempatan, Scheduling, Payroll, Identity, Menu 12 separation. Buat adapter/event yang terbukti diperlukan; jangan menciptakan tabel/endpoint duplikat.
- **P0 regresi sumber data Menu 07:** pada `WorkforceCore/Services/WfpNewHireAssignmentService.cs`, validasi `MstOrganizationUnit.DepartmentId`, `MstPosition.DepartmentId`, site/unit/position, relasi `MstSalaryGrade` + `MstSalaryStructure` (aktif, grade tepat), currency dan nilai awal sesuai approved offer. `actualDeptId != Guid.Empty ? actualDeptId : organizationUnitId` adalah fallback yang **harus dibuang** karena ID unit bukan ID department. Jangan memilih struktur gaji aktif tanpa mencocokkan grade.
- **Pembatasan gaji:** kontrak pertama dari approved offer boleh memicu initial salary assignment sesuai baseline `HRD-DEC-084/088` yang sah; mutasi/promosi, batch initial placement, perpanjangan dan konversi **tidak** mengubah salary otomatis (`HRD-DEC-036/048/054`). Jejak `ApprovedByUserId` nominal harus merujuk otoritas approval sebenarnya, bukan pencatat scan. Pastikan no double write ketika event/worker retry.
- **Pembatasan penempatan:** mirror `MstWorkforceProfile` hanya sinkron dari assignment yang efektif hari ini, bukan penempatan masa depan. Rekomendasi perubahan source Menu 07 hanya file/metode yang menjadi dependency kontrak Menu 08; jangan menghapus mekanisme SK terjadwal/menu lain yang baru dipush.
- Berikan akses minimal: HRD checker dapat memutuskan dalam scope, kepala unit hanya mengusulkan pegawai yang berwenang, pegawai ESS hanya datanya sendiri jika kontrak mengizinkan, Finance/Payroll menerima referensi status/kompensasi tanpa membuka seluruh PDF/NIK/nominal ke role lain.
- Audit `GetPagedAsync/GetByIdAsync/GetPrintDataAsync/DownloadScanAsync` untuk IDOR lintas site dan privacy. Masking gaji backend + tidak mencetak NIK penuh kecuali hak dan kebutuhan sah.
- Jadwal sesudah lapsed tanpa kontrak baru harus tertahan oleh kontrak integration asli; jika upstream planning belum siap, laporkan BLOCKED pada integrasi saja dan jangan beri label PASS.
- **Accept:** role lintas unit tidak bisa GET ID pegawai yang bukan scope; perubahan status tidak langsung mengubah payroll/penempatan; handoff non-renew/fail diterima Menu 12 tanpa duplikasi.

#### FIN-HRD-08-BE-010 - Backend validation gate dan regression tests (P0)

- Tambahkan unit/integration/negative test untuk kontrak awal/renewal/probation/conversion/dokumen/role/worker. Jalankan command yang diperbolehkan rules repo dan lingkungan; tidak mengaktifkan migration/DB produksi.
- Pemeriksaan meliputi null/invalid master, 400/403/404/409, optimistic concurrency, duplicate callback, duplicate submit, rollback, kalender/tanggal efektif, lembaga/site, docs evidence, auth+amount masking, transisi dari legacy.
- **Wajib jalankan `REG-HRD-08-001`..`010`** di samping 40 UAT asli: (1) rollback missing salary structure; (2) salary grade vs structure mismatch; (3) invalid department FK; (4) future-dated placement mirror; (5) same-day effective; (6) rollback probation/onboarding/attachment; (7) retry & no duplicate assignment; (8) no salary change from SK/renewal/conversion; (9) coexist Menu 07/08 sidebar; (10) approved offer salary provenance dan print masking. Test lapisan service/controller DB **paling tidak** untuk P0 bila test environment sah.
- Bekukan DTO/API version setelah source/test lolos gate BE agar FE tidak menebak payload. Laporan jalur `/task/report/backend/FIN-HRD-08-BE-010.md` atau tempat sesuai rule; status `PASS/FAIL/NOT RUN` dengan command yang benar-benar dieksekusi.
- **Gate ke FE:** API non-mock dapat menerima/mengembalikan data dari DB test yang diotorisasi, seluruh P0 tidak mempunyai defect open, dan daftar endpoint/enum/permission dipastikan; jika DB test tidak diotorisasi, kerjakan FE yang aman namun beri integration/UAT `BLOCKED`, bukan menyatakan lulus.

### 2.4A Patch wajib per task FIN-HRD-08-BE, tindak lanjut audit 18 November 2026

**Urutan implementasi praktis:** selesaikan state/workflow/transaction sebelum menetapkan DTO final; selesaikan policy & notification kemudian integrasi FE; kerjakan bukan hanya memberi rekomendasi. Task numbering tetap BE-001..010, penambahan di bawah ini adalah acceptance delta task yang sama, bukan task baru.

| Task lama | File yang harus diperiksa dan DIUBAH bila perlu | Patch wajib dan acceptance yang dapat dites |
| --- | --- | --- |
| `FIN-HRD-08-BE-001` | `HrdFixedTermPolicyService.cs`, `IHrdFixedTermPolicyService.cs`, `EmploymentContractDtos.cs` | Gunakan kalkulasi tanggal inclusive/exclusive yang presisi; uji start `2027-01-01`, end maksimum legal dan `+1 hari`; samakan GET eligibility, POST dan PUT; versi aturan dan legal transition dikembalikan jelas; jangan auto-rewrite legacy |
| `FIN-HRD-08-BE-002` | `WfpContractHistoryWriteService.ActivateContractAsync`, `HrdEmploymentContractService.RecordSignedAsync/GetPrintDataAsync`, `HrdEmploymentContractController`, attachment service | Pusatkan state Signed->Active berdasar effective date, satu current, preflight approval offer, auth scoped dan salary detail yang benar; retry aman, tidak menampilkan `ex.Message` internal ke klien; scan orphan cleanup; `GetPrintDataAsync` mask amount dan NIK sesuai izin |
| `FIN-HRD-08-BE-003` | `SubmitDecisionAsync`, `UpdateDecisionAsync`, `CreateFollowUpDraftAsync`, `HrdFixedTermPolicyService` | Gunakan validator bersama pada POST+PUT; guard jenis keputusan/kontrak, legal period, status, actor; cegah open decision ganda dengan concurrency DB yang sah; hitung renewalSequence authoritative, collision-safe number; jangan membuat convert sebagai renewal |
| `FIN-HRD-08-BE-004` | `RecordEmployeeNotifiedAsync`, `RecordEmployeeNotifiedRequest`, attachment/notification persistence | Evidence artefak tertulis wajib untuk label `ON_TIME`; penerima, kanal, waktu pengiriman, waktu penerimaan/tercatat, actor, immutable metadata serta access control; tolak future/backdated yang tidak berlandasan proof; jika schema perlu diubah, berhenti hanya pada migration gate, siapkan kode kompatibel/plan dan laporkan blokir |
| `FIN-HRD-08-BE-005` | `HrdEmploymentLifecycleWorkflowIntegrationService.HandleWorkflowCompletedAsync`, `HrdEmploymentContractService.ApplyDecisionAsync`, workflow adapter | Satu jalur domain untuk callback dan approval, maker-checker, state guard, transaksi status+follow-up/handoff, idempotency juga untuk `NotRenew` dan reject, audit; jangan mengubah hanya `DecisionStatus` dengan `SaveChangesAsync`; tes callback dua kali/dua worker dan rollback |
| `FIN-HRD-08-BE-006` | `HrdProbationService`, `HrdEmploymentContractService.RecordSignedAsync`, scheduler | Pertahankan PKWT prohibition, fail evidence, maker-checker; perbaiki awal probation hanya dari initial PKWTT sah dan clause; test rollback bersama PDF+contract+placement; callback T15 harus melewati guard yang sama, tidak auto-pass setelah approved fail; uji tanggal akhir |
| `FIN-HRD-08-BE-007` | `CreateFollowUpDraftAsync`, `WfpContractHistoryWriteService.CreateRenewalContractAsync`, `ActivateContractAsync`, scheduler | Buat persist method yang tidak menimpa `HistoryType=Conversion`; sumber type PKWTT aktif wajib, `EndDate=null`, `ProbationEndDate=null`, link prev/next, tipe status berubah saat efektif setelah scan; current lama tetap sampai successor aktif; tidak ubah salary |
| `FIN-HRD-08-BE-008` | `HrdEmploymentLifecycleSchedulerService`, hosted service, summary service | Jadwal kalender operasional RS berzonasi site/WIB, bukan UTC date mentah; cek both Signed/Active sesuai state machine authoritative; lakukan activation/expires/lapsed sesuai keputusan dan legal hold; sync mirror bila penempatan efektif; idempotency multi-instance; angka H-60/H-14/H-7 dari real DB (hitung overdue & pending beda metrik) |
| `FIN-HRD-08-BE-009` | `HrdEmploymentContractController`, query read/summary, `EmploymentContractDtos`, `WfpNewHireAssignmentService`, `WfpContractHistoryWriteService` | Tambahkan scope resource/site/unit/employee pada Read/print/download/decisions, ViewAmount; jangan bocorkan NIK; `salary-detail` harus sesuai contract jika endpoint dipertahankan; `hospitalSiteId` benar-benar menyaring. Hasil JSON cocok dengan FE, tidak memanipulasi gaji/perubahan unit tanpa approval |
| `FIN-HRD-08-BE-010` | Test project/repository patterns; `task/report/backend`, report master | Jalankan scoped unit+integration tests, build; uji P0 raw HTTP/SQL UAT berizin, dual request/callback, rollback file, prepared data; catat exact command/log exit code/role/DB sebelum-sesudah; UAT `NOT RUN` bila browser atau DB UAT tidak tersedia |

**Panduan implementasi khusus P0 (tidak boleh dipotong):**

1. **Kontrak bertanggal efektif masa depan:** `RecordSignedAsync` harus menyimpan scan dan menandai `Signed` tanpa `IsCurrent=true`/tanpa mengganti current lama bila `StartDate` lebih dari hari operasional. Saat mulai berlaku, worker dapat mengubah ke `Active` dan mematikan previous current secara transactional. Untuk start yang sudah efektif, kontrak boleh langsung `Active` setelah validasi/tanda tangan, tetapi worker expired harus juga memproses Active. Jangan meninggalkan `Active` future yang tak diproses. Perbaiki `SyncFromContractAsync` agar employment record tidak diubah prematur. Jika data lama sudah Active future, siapkan backward-compatible predicate/repair plan, jangan rewrite DB diam-diam.
2. **Callback T16/T15:** `StartContractDecisionWorkflowAsync` sekarang hanya menyimpan GUID dan Submitted. Periksa apakah adapter `WorkflowManagement` benar-benar memiliki instance persetujuan. Gunakan satu layanan domain untuk memvalidasi maker-checker serta menjalankan side effects; jangan memperbaiki T16 dengan memanggil service yang membuat constructor dependency circular. Pilih integrasi resmi, pertahankan idempotency key `WorkflowInstanceId/DecisionId`, serta baca final state sebelum perubahan. Untuk T15 juga jangan mem-bypass guard probation yang baru.
3. **Konversi bukan Renewal:** jangan memanggil `CreateRenewalContractAsync` untuk konversi jika method tersebut menulis `HistoryType="Renewal"`. Pilih writer domain yang mempertahankan `Conversion`, audit `PreviousContractHistoryId`, pastikan numbering tidak bentrok, simpan `SourceDocumentType` dan referensi keputusan bila skema tersedia.
4. **Keputusan update:** endpoint PUT tidak boleh mengubah decision yang sedang/ sudah diputus kecuali state resmi memungkinkan. Semua input tanggal/durasi/kategori disanitasi dan revalidate dengan policy yang sama seperti POST. Tambahkan negative test PATCH/PUT yang mengubah 12 menjadi 25 bulan, atau `Extend` menjadi `ConvertToPermanent` tanpa legal eligibility.
5. **Kepatuhan H-7:** satu catatan `EvidenceNotes` tanpa bukti tidak boleh menjadi status `ON_TIME` yang dipercaya; sediakan evidence file / immutable reference + metadata as-of dan waktu server, with access scoped. Jika tanggal pengiriman tidak dapat dibuktikan, status `MISSING/UNVERIFIED` sesuai kontrak legal; jangan menerbitkan kepatuhan palsu. Tidak menolak otomatis hak pekerja berdasarkan sekadar keterlambatan administratif tanpa keputusan Legal.
6. **Isolasi data:** `Read` tidak otomatis berarti semua pegawai di seluruh RS; implementasikan scoping yang sudah ada, bukan invent permission baru tanpa registry. NIK masked pada list/print untuk role yang tidak sah; nominal gaji tidak boleh pernah keluar dari API `GetById`/worklist/print ke aktor tanpa ViewAmount.
7. **Efek persetujuan atomik:** status Approved, draft follow-up, no duplicate, separation handoff harus konsisten. Simpan DB dalam transaksi jika hanya database; untuk storage/workflow/external side effects gunakan pola outbox atau idempotent compensation, bukan klaim semuanya atomik hanya karena ada satu `SaveChangesAsync`.
8. **Non-renewal dan masa percobaan gagal:** `HandoffLifecycleSeparationAsync` ke Menu 12 tidak boleh membuat dua separation record ketika callback diulang. Error downstream tidak boleh menghasilkan status Approved tanpa handoff yang sah; status `PENDING_RETRY/FAILED` hanya jika contract existing mendukung, jangan menciptakan enum diam-diam.
9. **Numerik/performance:** `ContractDecisionQuery.PageSize` dibatasi secara server; summary dibatasi site, paging stabil, string/date format enum sesuai serializer (C# biasanya nilai angka). Cek indeks/unicity dan migration gate sebelum menambahkan constraint.

**Gate keluar BACKEND MODE:** inventaris seluruh endpoint dan JSON nyata (camelCase), key summary dan paged response, role/permission, state transition, full test matrix. Kirim kontrak tersebut ke fase FE. Jangan hanya tulis "BE done, 0 compiler errors".

### 2.5 Urutan task FRONTEND SETELAH BACKEND - 8 task FIN

**Wajib pakai registered Skill `quilvian-engineering-skills:build-module-frontend` per task, semua Base Component dan CSS Modules sesuai rule. Tidak membuat UI sukses palsu, endpoint dummy, data statis yang tampak riil, atau ID UUID mentah.**

#### FIN-HRD-08-FE-001 - MenuItems dan navigasi (P0)

- Ubah `src/utils/menu-sidebar/menu-items.jsx` agar pada kelompok HRD ada menu induk **`Kontrak & Masa Kerja`** dengan **empat** child persis: `Perjanjian Kerja`, `Perpanjangan Kontrak`, `Masa Percobaan`, `Perubahan Status Kerja`.
- **HEAD FE pasca-push sudah memiliki grup Menu 07 `Penempatan, Mutasi & Promosi` beserta 5 child.** Jangan menimpa/memindahkan/menyalin ulang Menu 07. Tambahkan Menu 08 sebagai grup **berbeda**, gunakan key/route unik, cek lokasi sidebar relatif terhadap `Onboarding & Kontrak` dan `Perencanaan Tenaga Kerja`, validasi permission resolver dan active-route. Baca ulang current HEAD sebelum edit; rebase perubahan secara semantik dalam worktree, **tanpa melakukan `git rebase`**.
- Reuse route `/hr/lifecycle/employment-contracts` untuk Perjanjian Kerja dan `/hr/lifecycle/probations` untuk Masa Percobaan; **tambah context page** `/hr/lifecycle/contract-renewals` dan `/hr/lifecycle/employment-status-changes` hanya setelah kontrak API teruji. Tambahkan `LIFECYCLE_ROUTES` dan permission mapping, unique keys, active sidebar state.
- Fitur Onboarding/Template/Pendampingan Klinis/Permintaan Akses/Batch/Pengaturan/Worklist tetap dapat diakses di grup sesuai owner bisnisnya (jangan hapus/reparent membabi buta). Hindari dua item sidebar dengan pathname sama; tautan lama/bookmark tetap hidup (alias/link/redirect bila diperlukan).
- Uji role dan deep links; sidebar tidak boleh menampilkan menu berizin semu.

#### FIN-HRD-08-FE-002 - Perjanjian Kerja list (P1)

- Reuse `employment-contracts-list-view.jsx`; ubah Hero judul `Perjanjian Kerja`; tambah SummaryGrid dari API asli: Aktif, Menunggu TTD, Akan Berakhir, Perlu Pemeriksaan (jangan hitung page size sebagai total keseluruhan; API summary jika perlu).
- `DataFilter`: search pegawai/no kontrak, PKWT/PKWTT, unit, status, expiring interval; `DataTable` kolom pegawai/kode, nomor, jenis, unit, start/end, H-remaining, status, aksi rincian. Server pagination/sort, reset filter tanpa bug, loading/error/empty.
- Tidak boleh fallback ke daftar contoh saat API 404/500; tampilan berisi pesan gagal dan retry.

#### FIN-HRD-08-FE-003 - Detail kontrak, dokumen, salary masking, histori (P0)

- Reuse `employment-contract-detail-view.jsx` dan `LifecycleService`; detail `Informasi`, `Dokumen`, `Riwayat`, `Keputusan`, `Audit`, print-data sesuai akses; Base form/ConfirmModal untuk upload signed, koreksi, print.
- Perbaiki label lama `Batas total kontrak menurut PP 35/2021` agar sesuai legal policy version yang benar dari backend, bukan string statis salah; tidak lagi default `maxExtensionMonths || 60`.
- Hanya tampilkan nominal jika API mengembalikan akses sah. Tombol `Catat Tanda Tangan` hanya Draft+permission, `Koreksi` sesuai state. Tampilkan status `Belum Ada Dokumen`, `Butuh TTD`, `Perlu Verifikasi` dengan penjelasan.
- Cegah double submit, tampilkan error backend, cegah dokumen bocor pada preview/download/print, gunakan safe filename.

#### FIN-HRD-08-FE-004 - Perpanjangan Kontrak worklist dan form (P0)

- Tambahkan page/view baru untuk `/hr/lifecycle/contract-renewals`, tetap memanggil service/API keputusan existing yang telah diperluas; tidak membuat Redux/local-only approval.
- SummaryGrid pengingat H-60, eskalasi H-14, tenggat pemberitahuan H-7 **berdasarkan respons server**; filter Unit, status, due, decision. Tabel histori/rangkaian kontrak dan aksi `Ajukan Perpanjangan`, `Lihat Kelayakan`, `Lihat Dokumen`.
- Form berisi before/after dates, renewal count, max extension menurut rule version, sebab tidak eligible, alasan, bukti pemberitahuan tertulis, status approval. Hasil `LATE` harus jelas; tidak boleh merender `eligible` ketika API gagal.
- Form pemanjangan bersifat halaman/form terstruktur, bukan modal tunggal yang terlalu panjang pada tablet.

#### FIN-HRD-08-FE-005 - Masa Percobaan (P0)

- Reuse `probations-list-view.jsx`, `probation-detail-view.jsx` dan service; rework summary, filter, timeline tiga bulan, hasil Lulus/Tidak Lulus, alasan, lampiran jika gagal, approval workflow dan audit.
- Tidak menampilkan tombol `Perpanjang Masa Percobaan` ataupun form probation untuk PKWT. Gunakan kontrak/server eligibility, bukan hanya label dari UI.
- Hasil auto-pass, fail, pending, returned harus jelas; pengajuan ditutup tak boleh bisa submit; disabled/loading/err harus sesuai backend.

#### FIN-HRD-08-FE-006 - Perubahan Status Kerja (P0)

- Tambahkan context page `/hr/lifecycle/employment-status-changes` (bukan tabel status kerja baru). Daftar kasus konversi dari `HrdContractDecision` tipe `ConvertToPermanent`, filter dan history; detail before PKWT -> after PKWTT + tanggal efektif.
- Sediakan submit decision melalui API existing, banner approval, link draft PKWTT, catat signed, timeline activation; jangan set `status=PKWTT` pada client saat baru dibuat/approved.
- Jika master PKWTT unavailable, perlihatkan error server dan jangan sembunyikan sebagai success; user tidak dapat langsung mengubah profil atau nominal gaji dari halaman ini.

#### FIN-HRD-08-FE-007 - UI consistency, responsive, permissions (P1)

- Implementasikan layout `Hero -> SummaryGrid -> DataFilter -> InformationAlert -> DataTable -> ToastStack` untuk list mengikuti `rules/frontend/page-composition-patterns.md`.
- Gunakan `BaseButton`, `BaseFormControl`, `BaseGroupedEditorView` atau pola editor legal sesuai repository, `FilterSelect`, `ResourceFilterSelect`, `StatusBadge`, `ConfirmModal`, `AccessDeniedGate`, `ToastStack`. Import per file dari `src/components/features/base-features/` tanpa mengarang barrel export.
- Gunakan CSS Modules dan design tokens, bukan random hex/inline fixed widths dan bukan Tailwind class yang tidak terpasang. State loading, empty, error, access denied; responsive tablet/desktop, label/form, keyboard, focus, screen reader minimal.
- UI default tidak menampilkan nomor identitas sensitif penuh atau nominal tanpa role. Cegah perbedaan label `Status Perpanjangan`, `Status Kontrak`, `Status Hubungan Kerja`.

#### FIN-HRD-08-FE-008 - FE test, live browser UAT, retest dan laporan (P0)

- Jalankan lint dan build/targeted frontend test sesuai rules jika diizinkan; tes permission, navigasi, form, respons API, double click, error upload, refresh persistence, tablet viewport. Gunakan browser headed dengan environment dan akun UAT sah bila tersedia; **screenshot tanpa PII**.
- Semua failed case in-scope harus diperbaiki dan diuji ulang, bukan hanya dilaporkan. Integrasi UAT dengan backend yang belum tersedia ditandai `BLOCKED/NOT RUN`, bukan `PASS`.
- Jalankan UAT `AT-HRD-08-001`..`040` dan regression overlay `REG-HRD-08-001`..`010`, termasuk role HRD/atasan, refresh+login ulang, eksekusi worker, kontrak signed future-date serta perbandingan sidebar Menu 07 vs 08. Jangan mengklaim regression P0 PASS dari lint/build FE saja.
- Rekonsiliasi status per Submenu dan task report di lokasi resmi sesuai wewenang.

### 2.5A Patch frontend yang WAJIB diterapkan terhadap hasil audit (task FIN sama)

**Semua halaman di bawah SUDAH ADA. Jangan membuat page baru pengganti, jangan mengubah URL, jangan membuat data dummy.** Utamakan kontrak respons server terbaru hasil task BE-009; contoh pemetaan berikut berdasarkan DTO yang dibaca saat audit dan mungkin diperbaiki BE; setelah BE selesai gunakan bentuk final yang terbukti dari HTTP.

| Field/fungsi | Frontend saat audit (BUG) | BE DTO aktual saat audit (SOURCE) | Tindakan FE |
| --- | --- | --- | --- |
| Summary Perjanjian Kerja | `activeCount` | `activeContractsCount` | Ubah mapper langsung/adapter teruji; jangan angka `0` karena `undefined` |
| Summary Perjanjian Kerja | `awaitingSignatureCount` | `pendingSignedCount` | Selaraskan dan test data >0 |
| Summary Perjanjian Kerja | `reviewNeededCount` | `needsReviewCount` | Selaraskan |
| Summary kontrak expiring | judul H-60 | BE query `AddDays(30)` | BE ubah ke 60 sesuai keputusan/label, lalu FE recheck |
| Summary Perpanjangan | `due60DaysCount` | `reminderH60Count` | Selaraskan |
| Summary Perpanjangan | `due14DaysCount` | `escalationH14Count` | Selaraskan |
| Summary Perpanjangan | `due7DaysNoticeCount` | `noticeDueH7Count` | Selaraskan |
| Summary Perpanjangan | `overdueCount` | TIDAK ADA; ada `pendingDecisionCount` | Jangan menganggap pending=overdue. Minta BE menambah `overdueCount` riil atau ganti kartu menjadi "Menunggu Keputusan" dengan field pending |
| Summary Konversi | `approvedCount`, `effectiveCount`, `rejectedCount` | `pendingSignedCount`, `scheduledOrEffectiveCount`; tidak ada rejected | Perbaiki perincian state BE agar `ApprovedMenungguTTD`, `SignedMenungguEfektif`, `Efektif`, `Ditolak` berbeda, atau sesuaikan UI sesuai metrik asli tanpa mengarang hasil |
| Worklist pegawai | `employeeFullName` | `fullName` | Gunakan `fullName` |
| Worklist kode pegawai | `employeeWorkforceCode` | `workforceCode` | Gunakan `workforceCode` |
| Deep link rincian | `employmentContractId` | `contractHistoryId` | Gunakan ID ini dan guard falsy sebelum navigasi |
| Badge H-7 | `noticeCompliance` | `notificationComplianceStatus` | Gunakan nilai server yang benar; `MISSING` tidak boleh jadi `ON_TIME` |
| Urutan perpanjangan | `renewalSequence || 1` | `ContractDecisionResponse` belum punya field | Tambah DTO/mapper BE berdasarkan histori; jika tidak dapat, render "Belum tersedia" jujur, jangan angka satu palsu |
| Print kontrak | `isSalaryMasked` | `isSalaryMasked` disediakan | Pertahankan; response API tidak boleh mengandung `basicSalary` bila izin tidak ada |

**Tugas per ID:**

- **`FIN-HRD-08-FE-001`**: verifikasi grup sidebar `Kontrak & Masa Kerja` sudah berisi **tepat 4 submenu**, Menu 07 tetap 5 child. Jangan menambahkan grup kedua. Cek permission filter yang sekarang mendukung multiple `requiredPermission` dan state route aktif. Reuse `src/utils/menu-sidebar/menu-items.jsx` dan `src/lib/constants/hr/lifecycle/lifecycle-constants.js`.
- **`FIN-HRD-08-FE-002`**: perbaiki `employment-contracts-list-view.jsx` sesuai summary contract DTO dan H-60 asli, list filter/reload/error, no silent default. Dengan database test count >= 1, SummaryGrid wajib menampilkan nilai yang sama dengan GET `/summary`.
- **`FIN-HRD-08-FE-003`**: `employment-contract-detail-view.jsx`: label legal policy dinamis (tanpa fallback legal basis yang menipu), batas durasi term yang sama dengan BE, tombol sign disabled saat status tidak sah, error preview/file persisten, `isSalaryMasked` dan identitas sensitif aman; GET salary-detail sesuaikan kontrak final (jangan memakai `true` sebagai nominal).
- **`FIN-HRD-08-FE-004`**: perbaiki `contract-renewals-list-view.jsx` dan service adapter: summary DTO, person fields, notice badge, row link, renewal sequence, filter/search/pagination. Implementasikan form bukti tertulis jika backend menerima file; jangan tombol "Sudah diberitahukan" yang hanya mengirim tanggal tanpa evidence. Tidak mengganti `decisionType=1`.
- **`FIN-HRD-08-FE-005`**: `probations-list-view.jsx`/`probation-detail-view.jsx`: pertahankan summary yang cocok DTO, status `PassedAutomatically`, wajib evidence file Fail, keadaan pending/returned/closed, output API error 400/403/409, role UI sejalan server. Pastikan kalender expiry dan maker-checker tidak tampak bisa dibypass.
- **`FIN-HRD-08-FE-006`**: perbaiki `employment-status-changes-view.jsx` yang memakai field/summary salah; tampilkan `Diajukan`, `Disetujui menunggu TTD`, `Ditandatangani menunggu efektif`, `Berlaku efektif`, `Ditolak/Ditarik` hanya bila state dapat diperoleh sah dari BE; jangan gabungkan `Signed future` dengan efektif. Semua link kontrak memakai `contractHistoryId` valid dan histori `Conversion` terbukti.
- **`FIN-HRD-08-FE-007`**: pakai Base Components existing (Hero, SummaryGrid, DataFilter, DataTable, FilterSelect, BaseButton, StatusBadge, InformationAlert, ToastStack, ConfirmModal, dsb. sesuai pattern), CSS Modules/design tokens, responsive desktop+tablet, focus/accessibility, error/empty/loading, no mock data. Jangan mengganti seluruh desain tanpa kebutuhan perbaikan.
- **`FIN-HRD-08-FE-008`**: jalankan targeted test untuk semua mapper; periksa DOM, click table detail, permission menu, API 401/403/409/500, tab form, print mask, viewport 1366px/1024px/768px, screenshots tanpa NIK/nominal. UAT browser headed hanya jika kredensial dan environment sah.

**Skema UI setelah perbaikan (wajib pertahankan layout existing, ini target fungsi data):**

```text
MENU 08 > 1. PERJANJIAN KERJA
[Hero: Perjanjian Kerja]                 [Aksi berizin: Tambah/Refresh]
[Kontrak Aktif (API)] [Menunggu TTD (API)] [Akan Berakhir H-60 (API)] [Perlu Pemeriksaan (API)]
[Filter No/Nama] [Status] [Jenis] [Unit/Site] [Reset]
| Nomor | Pegawai | PKWT/PKWTT | Tanggal Mulai/Akhir | Status | Rincian |
DETAIL: identitas terproteksi | dokumen scan | status signed/active | jejak tindakan |
CETAK: tanpa ViewAmount => nominal masked; NIK masked sesuai policy
```

```text
MENU 08 > 2. PERPANJANGAN KONTRAK
[Hero: Perpanjangan Kontrak]
[H-60] [H-14] [H-7] [Menunggu Keputusan atau Overdue REAL dari API]
[Filter status usulan] [Filter pegawai/no dokumen] [Periode]
| Nomor Usulan | Pegawai | Kontrak Asal | Perpanjangan Ke | H-7 | Status | Rincian |
DETAIL/FORM: eligibility ruleVersion -> before/after tanggal -> alasan ->
             attachment bukti tertulis -> submit -> approval status server
NO DATA = empty state, API ERROR = error state, bukan baris contoh
```

```text
MENU 08 > 3. MASA PERCOBAAN
[Hero: Masa Percobaan]
[Berjalan (API)] [Jatuh Tempo H-14 (API)] [Menunggu Penilaian (API)] [Selesai (API)]
[Filter pegawai] [Unit] [Status] [Jatuh tempo]
| Pegawai | Kontrak PKWTT awal | Mulai | Akhir | Status | Penilaian | Rincian |
DETAIL: ajukan lulus/tidak lulus; Fail wajib upload; reviewer != pengaju;
        PassedAutomatically locked + audit; tidak boleh untuk PKWT/konversi
```

```text
MENU 08 > 4. PERUBAHAN STATUS KERJA
[Hero: Perubahan Status Kerja]
[Diajukan (API)] [Approved menunggu TTD (API)] [Signed future (API)] [Aktif (API)]
[Filter pegawai] [Tanggal efektif] [Status approval]
| Nomor | Pegawai | PKWT -> PKWTT | Tgl Efektif | TTD | Status | Rincian |
DETAIL: link kontrak asal / follow-up; timeline Submitted -> Approved ->
        Draft PKWTT -> Signed -> Active; tidak ada perubahan gaji otomatis
```

**Frontend acceptance wajib:** nonzero API counts tampil persis, tidak ada `undefined` pada nama/ID, semua tombol rinci menuju GUID benar, badge notifikasi sesuai server, state conversion tidak lebih cepat dari tanggal efektif, tidak ada console error, hasil refresh tetap konsisten.

### 2.6 MenuItems target dan route matrix

**Kondisi setelah push terbaru:** FE `menu-items.jsx` **SUDAH memiliki** grup `Kontrak & Masa Kerja` (4 child) dan grup `Penempatan, Mutasi & Promosi` (5 child). Route `/hr/lifecycle/employment-contracts`, `/hr/lifecycle/contract-renewals`, `/hr/lifecycle/probations`, dan `/hr/lifecycle/employment-status-changes` **SUDAH ADA**. **REPAIR** permission, field mapping, deep link dan konsistensi route - jangan membuat menu baru kedua. Grup pendukung `Onboarding & Kontrak` tetap sesuai owner domain; pertahankan deep link historis dan jangan duplikasi pathname.

**Target navigasi:**

```text
Sumber Daya Manusia
  ...
  Penempatan, Mutasi & Promosi
  Kontrak & Masa Kerja
    1. Perjanjian Kerja             /hr/lifecycle/employment-contracts         [EXISTING ROUTE]
    2. Perpanjangan Kontrak         /hr/lifecycle/contract-renewals            [PROPOSED NEW CONTEXT]
    3. Masa Percobaan               /hr/lifecycle/probations                   [EXISTING ROUTE]
    4. Perubahan Status Kerja       /hr/lifecycle/employment-status-changes    [PROPOSED NEW CONTEXT]
  Kompetensi & Kredensial
  ...
```

| Item | Komponen/view utama | Sumber backend | Permission |
| --- | --- | --- | --- |
| Perjanjian Kerja | `employment-contracts-list-view.jsx`, detail existing | `HrdEmploymentContractController` | `WorkforceEmploymentContract.Read` existing; aksi spesifik |
| Perpanjangan Kontrak | view **baru** dengan reuse details/form | `HrdContractDecision` T16 + API worklist **PROPOSED** | Existing `SubmitDecision`, `RecordNotification` + read/list scope yang sah |
| Masa Percobaan | `probations-list-view.jsx`, detail existing | `HrdProbationController` | `WorkforceProbation.Read/Submit` dan workflow HRD |
| Perubahan Status Kerja | view **baru** dengan reuse kontrak | `HrdContractDecision` `ConvertToPermanent` | Existing keputusan dan akses kontrak; mapping aksi sesuai policy |

`requiredPermission` pada menu harus cocok dengan action/controller actual, dicek di server. Jangan menciptakan resource permission baru tanpa registrasi resmi.

### 2.7 Skema tampilan frontend WAJIB - 4 halaman bisnis

**Gunakan desain profesional konsisten, dashboard responsive, struktur kartu/warna/typography mengikuti Base Component Quilvian. Skema adalah kontrak tampilan; bukan permintaan untuk memasang placeholder sebagai data produksi.**

#### UI-08.1 - Perjanjian Kerja

```text
+-------------------------------------------------------------------------------------------+
| HERO: Perjanjian Kerja                            [Akses Batch Data Awal] [Help]           |
| Sumber data kontrak resmi, dokumen, riwayat hubungan kerja                                 |
+--------------------+--------------------+--------------------+---------------------------+
| Kontrak Aktif [API]| Menunggu TTD [API]  | Akan Berakhir [API]| Perlu Periksa [API]        |
+--------------------+--------------------+--------------------+---------------------------+
| DATAFILTER: [Cari Nama/Nomor] [PKWT/PKWTT] [Unit] [Status] [Akhir] [Reset]                  |
+-------------------------------------------------------------------------------------------+
| Alert: hanya tampil jika error/policy reminder                                           |
| TABEL: No Kontrak | Pegawai | Unit | Jenis | Mulai | Berakhir | Sisa Hari | Status | Aksi   |
|        [data server paged; bila kosong: empty state dan bukan 3 pegawai contoh]             |
+-------------------------------------------------------------------------------------------+
| Detail (halaman): [Informasi] [Dokumen] [Riwayat] [Keputusan] [Audit]                        |
| Pegawai / PKWT-PKWTT / Dasar pekerjaan / Periode / Pejabat / TTD / Berkas                   |
| [Lihat PDF] [Cetak Data Sah] [Catat TTD] [Ajukan Koreksi]  (sesuai izin)                     |
| Nominal sensitif: masked jika tidak punya izin                                            |
+-------------------------------------------------------------------------------------------+
```

**Interaksi:** pencarian debounced, filter disimpan di URL jika pola existing mendukung; klik baris menuju detail; upload scan dengan status kemajuan, error jelas, retry; update summary setelah sukses; `ToastStack` untuk action server. Ukuran tablet: summary 2 kolom, tabel scroll horizontal atau kolom prioritas; form stacked 1 kolom.

#### UI-08.2 - Perpanjangan Kontrak

```text
+-------------------------------------------------------------------------------------------+
| HERO: Perpanjangan Kontrak                                  [Buka Pengajuan]               |
+---------------------------+----------------------------+----------------------------------+
| Pengingat H-60 [API]      | Eskalasi H-14 [API]        | Tenggat H-7 [API legal status]   |
+---------------------------+----------------------------+----------------------------------+
| FILTER: [Cari] [Unit] [Harian/Jatuh Tempo] [Approval] [Jenis Keputusan]                     |
+-------------------------------------------------------------------------------------------+
| TABEL: Pegawai | Kontrak Lama | Akhir | Renewal Ke | Eligible | Notifikasi | Approval | Aksi |
+-------------------------------------------------------------------------------------------+
| FORM/DETAIL PENGAJUAN                                                                    |
| Identitas/Unit [read-only]         Kontrak asal [Nomor | PKWT | Tgl awal-akhir]            |
| Perpanjangan ke-[n]                Batas sesuai Rule Version [API]                         |
| Mulai baru [Date]                  Akhir baru [Date]           Jumlah bulan [computed]     |
| Alasan [Textarea]                  Bukti surat perpanjangan [Upload / Riwayat Kirim]       |
| Status: memenuhi / tidak memenuhi / butuh Legal review                                    |
| [Simpan sebagai Draf bila API mendukung] [Ajukan] [Batal]                                 |
| Timeline: Submitted -> Returned/Rejected/Approved -> Draft Lanjutan -> Signed -> Effective |
+-------------------------------------------------------------------------------------------+
```

**Interaksi:** detail dari daftar, lihat before/after tanggal, action hanya sesuai permission; submit disabled jika legal validation gagal; jangan auto-tambah `extensionMonths` berdasarkan value hardcode; tampilkan status `LATE/MISSING` dan siapa yang bertanggung jawab. Jika backend tidak punya state draft untuk keputusan, jangan menampilkan tombol simpan draft palsu.

#### UI-08.3 - Masa Percobaan

```text
+-------------------------------------------------------------------------------------------+
| HERO: Masa Percobaan                                            [Lihat Ketentuan]          |
+---------------------+-----------------------+--------------------+------------------------+
| Berjalan [API]       | Due <=14 hari [API]   | Menunggu HRD [API] | Selesai [API]          |
+---------------------+-----------------------+--------------------+------------------------+
| FILTER: [Cari] [Unit] [Atasan] [Status] [Periode] [Hanya Mendekati Akhir]                  |
+-------------------------------------------------------------------------------------------+
| TABEL: Pegawai | PKWTT | Atasan | Mulai | Akhir 3 Bulan | Sisa Hari | Status | Aksi         |
+-------------------------------------------------------------------------------------------+
| DETAIL: identitas & nomor kontrak | Penilai atasan | Timeline | Audit                      |
| 1. Hasil: ( ) Lulus  ( ) Tidak Lulus                                                    |
| 2. Alasan Evaluasi [textarea]                                                             |
| 3. Lampiran [Upload] wajib jika Tidak Lulus                                              |
| 4. Status workflow & keputusan HRD                                                        |
| [Ajukan] [Tarik jika boleh] [Lihat Riwayat]                                               |
| Tidak ada tombol memperpanjang masa percobaan; tidak tampil untuk PKWT                     |
+-------------------------------------------------------------------------------------------+
```

**Interaksi:** mencegah self-approval, menolak upload hilang untuk Fail, refresh setelah status berubah, warning sebelum akhir 3 bulan; jika `PassedAutomatically`, hanya tampil riwayat dan alasan, tidak bisa submit keputusan tertunda.

#### UI-08.4 - Perubahan Status Kerja

```text
+-------------------------------------------------------------------------------------------+
| HERO: Perubahan Status Kerja                                  [Ajukan Pengangkatan Tetap]   |
+------------------------+------------------------+------------------------------------------+
| Diajukan [API]         | Menunggu TTD [API]     | Dijadwalkan/Efektif [API]                 |
+------------------------+------------------------+------------------------------------------+
| FILTER: [Cari Pegawai] [Unit] [Status Keputusan] [Tanggal Efektif]                         |
+-------------------------------------------------------------------------------------------+
| TABEL: Pegawai | Status Lama | Status Baru | Tgl Efektif | Keputusan | Dokumen | Aksi       |
+-------------------------------------------------------------------------------------------+
| DETAIL:                                                                               |
| +-----------------------------------+    +-----------------------------------------+  |
| | SEBELUM: PKWT                      | -> | SESUDAH: PKWTT                           |  |
| | Kontrak lama, periode, jabatan     |    | Draft kontrak baru, mulai efektif        |  |
| +-----------------------------------+    +-----------------------------------------+  |
| Dasar keputusan [Alasan]         Nomor keputusan [API]          Status TTD [API]            |
| Approval timeline [Submitted/Returned/Approved] -> Signed -> Effective                     |
| [Ajukan] [Lihat Draft PKWTT] [Catat TTD] [Lihat Riwayat] (berdasarkan role+state)           |
+-------------------------------------------------------------------------------------------+
```

**Interaksi:** sebelum approval hanya pengajuan, setelah approval tapi belum signed status belum efektif; setelah signed+tanggal mulai baru status efektif. Tampilkan error validasi bila master PKWTT tidak tersedia. `Unit/Jabatan/Gaji` jangan dapat diubah di sini.

### 2.8 Kontrak API target, state dan integrasi

**Endpoint EXISTING dari controller yang telah dibaca** (base prefix `api/v1/corporate/human-resource`):

| Method | Path | Resource/Action existing | Pemakaian |
| --- | --- | --- | --- |
| GET | `/employment-contracts` | `WorkforceEmploymentContract.Read` | Daftar kontrak |
| GET | `/employment-contracts/{id}` | `WorkforceEmploymentContract.Read` | Detail kontrak |
| GET | `/employment-contracts/{id}/print-data` | `WorkforceEmploymentContract.Read` | Print data; **tambahkan check izin nominal** |
| POST | `/employment-contracts/{id}/record-signed` | `WorkforceEmploymentContract.MarkSigned` | TTD + PDF |
| GET | `/employment-contracts/{id}/download-scan` | `WorkforceEmploymentContract.Read` | Dokumen bertanda tangan; evaluasi akses objek |
| GET | `/employment-contracts/{id}/fixed-term-check` | `WorkforceEmploymentContract.Read` | Kelayakan PKWT; perbaiki rule |
| POST | `/employment-contracts/{id}/decisions` | `WorkforceEmploymentContract.SubmitDecision` | Extend / ConvertToPermanent / NotRenew |
| GET | `/employment-contracts/{id}/decisions/{decisionId}` | `WorkforceEmploymentContract.Read` | Detail keputusan |
| PUT | `/employment-contracts/{id}/decisions/{decisionId}` | `WorkforceEmploymentContract.SubmitDecision` | Update usulan yang diizinkan |
| DELETE | `/employment-contracts/{id}/decisions/{decisionId}` | `WorkforceEmploymentContract.SubmitDecision` | Withdraw yang diizinkan |
| POST | `/employment-contracts/{id}/decisions/{decisionId}/notify` | `WorkforceEmploymentContract.RecordNotification` | Rekam pemberitahuan; perlu evidence |
| POST | `/employment-contracts/{id}/corrections` | `WorkforceEmploymentContract.RequestCorrection` | Koreksi versi kontrak |
| POST | `/employment-contracts/{id}/document` | `WorkforceEmploymentContract.UploadDocument` | Upload kontrak lama |
| GET | `/lifecycle-management/probations` atau `/probations` | `WorkforceProbation.Read` | List probation |
| GET | `/lifecycle-management/probations/{id}` atau `/probations/{id}` | `WorkforceProbation.Read` | Detail probation |
| POST | `/lifecycle-management/probations/{id}/assessments` atau `/probations/{id}/assessments` | `WorkforceProbation.Submit` | Ajukan penilaian |

**STATUS AUDIT 18 NOV 2026:** GET decision worklist (`/employment-contracts/decisions`), GET `summary`, `renewal-summary`, `conversion-summary` dan GET `probations/summary` **SUDAH ADA** di BE, tetapi harus diperbaiki per-field dan scope. API evidence notifikasi tertulis immutable beserta berkas, salary detail sesungguhnya, dan `eligibleActions`/history timeline hanya ditambah bila tidak ada layanan existing yang mencukupi dan kontrak final menyetujuinya. Sesuaikan tabel endpoint source aktual; jangan menggandakan endpoint. Reuse controller/service existing sebelum membuat controller baru.

**State machine ringkas (server authoritative):**

```text
Contract:   Draft -> Signed -> (Effective/Current according to StartDate) -> Completed/Ended
                 |                                               |
                 +-> Decision Submitted -> Returned/Rejected/Approved
                                             Approved Extend/Convert -> FollowUp Draft
                                                        -> Signed -> Scheduled -> Effective
                                             Approved NotRenew -> Menu 12 Handoff
                 +-> LapsedWithoutDecision (past EndDate without valid follow-up)
Probation:  Running -> AwaitingDecision -> (Passed | Failed | Returned -> Running)
                 \-> PassedAutomatically after expiry without approved decision
```

Nama state di atas adalah konsep UI; gunakan enum/status backend aktual (mis. `ContractStatusSigned`, `IsCurrent`), **jangan memaksa enum baru** jika proyeksi dari existing sudah cukup. Simpan history perubahan aktor dan waktu yang tidak dapat dipalsukan client.

**Cross-module boundary:** Menu 03 menawarkan employee offer -> Menu 08 kontrak; Menu 04 identitas dan histori; Menu 05/07 unit, jabatan, atasan efektif; Menu 06 dokumen/SK; Menu 09 kredensial tidak otomatis dari kontrak; Payroll hitung nilai/kompensasi; Scheduling tahan jadwal setelah lapsed yang tidak sah; Menu 12 mengeksekusi offboarding; Identity mengelola akses. Jangan duplikasikan master/tabel tiap menu.

---

## 3. Context

### 3.1 Snapshot repository dan sumber bukti yang terbaca

| Repository | Branch | HEAD yang dibaca pada 18 Nov 2026 | Commit relevan implementasi Menu 08 | Hasil |
| --- | --- | --- | --- | --- |
| FE `DevBenari/QuilvianSystemFrontendDev` | `HamzahV2` | `89c070547b80f74e3e1b7b2c464c8c096c3478da` | `16ce992159d384b46f8d41dcdb7ee83b27ec617d` | 4 submenu + 2 context view BARU benar-benar ada, DTO mismatch dan UAT tidak terbukti |
| BE `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | `daccac931cc06fd6dee572a26a3237486f3aa8a9` | `a2b6aa05436229a25ae6a97358d83bb547aaef48` | Lifecycle Service/Policy/Summary telah dibuat/diubah, defect P0 masih ada |
| Rules `DevBenari/QuilvianEngineeringSkills` | `main` | `201af64552b27873fd9af83c6722cf11b441bb1e` | sama | Sumber governance dan runtime Skills, ChatGPT hanya read-only |

Dua commit HEAD yang lebih baru setelah implementasi Menu 08 berkaitan dengan perbaikan Menu 07. Audit Menu 08 berlaku untuk source Menu 08 yang tidak berubah setelah SHA implementasi di atas. **Agent wajib cek SHA workspace dan perubahan worktree sebelum edit tanpa git pull/fetch/checkout otomatis.** Bila source ternyata berubah, gunakan file aktual sebagai sumber, re-evaluate hanya dampak delta, lalu eksekusi repair yang masih relevan. Jangan overwrite perubahan agent lain.

**Governance wajib sebelum menulis code:** FE dan BE `AGENTS.md`, `CLAUDE.md`; rules `QuilvianEngineeringSkills/agents/rules/GLOBAL_RULES.md`, `backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `API_RULES.md`, `DATABASE_RULES.md`, `CROSS_REPO_RULES.md`, `REVIEW_RULES.md`, `TEST_POLICY.md`; `frontend/frontend-architecture.md`, `base-component-catalog.md`, `base-component-decision-gate.md`, `page-composition-patterns.md`, `design-tokens.md`, `ui-consistency-checklist.md`, `test-policy.md`; `testing/`, `rule-output/`. BE wajib `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Registered runtime skills WAJIB dipanggil sebelum mengubah source, bukan dibaca manual sebagai pengganti.

**Sumber canonical:** `docs/module-blueprints/human-resource/00-interview-decisions.md` (`HRD-DEC-031`, `036`, `047`, `048`, `054`, `081`..`090`), `roadmap/requirement-traceability.md`, frontend/backend roadmap, lifecycle contracts/flowcharts; report Menu 08 yang sudah dipush tidak dapat diperlakukan sebagai hasil test tanpa evidence. Jika canonical policy historis stale, tandai `NEEDS_SYNC` dan lakukan perubahan dokumen hanya dalam mode/izin yang sah.

**Source utama BE (path repo):**
- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/HrdEmploymentContractController.cs`, `HrdProbationController.cs`;
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentContractService.cs`, `HrdFixedTermPolicyService.cs`, `HrdEmploymentLifecycleSchedulerService.cs`, `HrdEmploymentLifecycleSchedulerHostedService.cs`, `HrdEmploymentLifecycleWorkflowIntegrationService.cs`, `HrdProbationService.cs`, `HrdLifecycleAttachmentService.cs`, `HrdNumberSeriesService.cs`;
- `Areas/Corporate/HumanResource/LifecycleManagement/DTOs/EmploymentContractDtos.cs`, `HrdProbationDtos.cs`;
- `Areas/Corporate/HumanResource/WorkforceCore/Services/WfpContractHistoryWriteService.cs`, `WfpNewHireAssignmentService.cs`, `WorkforceCoreCrossEmployeeQueryService.cs` bila dependency terbukti;
- `Areas/Corporate/HumanResource/LifecycleManagement/Constants/EmploymentLifecycleValueConstants.cs`, domain models Wfp/Hrd dan `Program.cs` registrasi DI/worker.

**Source utama FE (path repo):**
- `src/utils/menu-sidebar/menu-items.jsx`, `src/lib/constants/hr/lifecycle/lifecycle-constants.js`, `src/lib/services/hr/lifecycle-service.js`;
- `src/components/view/hr/lifecycle/employment-contracts-list-view.jsx`, `employment-contract-detail-view.jsx`, `probations-list-view.jsx`, `probation-detail-view.jsx`, `contract-renewals-list-view.jsx`, `employment-status-changes-view.jsx`;
- `src/app/hr/lifecycle/contract-renewals/page.jsx`, `src/app/hr/lifecycle/employment-status-changes/page.jsx`, route detail/list existing;
- `src/components/features/base-features/`, `src/lib/hooks/hr/use-hr-remote-data` untuk reuse.

### 3.2 Matriks source vs gap (hasil audit terakhir, BUKAN hasil UAT)

| Submenu | Yang sudah ada dan harus REUSE | Masalah belum tertutup | Status |
| --- | --- | --- | --- |
| Perjanjian Kerja | Sidebar; list/detail FE; BE `GetPrintDataAsync`, `RecordSignedAsync`, `HrdFixedTermPolicyService`, grade/department preflight | Writer `Active/IsCurrent` sebelum efektif, signed race, salary detail API boolean, summary field mismatch, data scope/identitas | `REPAIR - SOURCE SEEN, UAT NOT VERIFIED` |
| Perpanjangan Kontrak | FE route/worklist; BE decision GET, summary, policy checker | PUT tanpa validasi ulang, callback workflow bypass, date math, evidence H-7 opsional, FE summary/row mismatch | `REPAIR - SOURCE SEEN, UAT NOT VERIFIED` |
| Masa Percobaan | FE route/list/detail; BE PKWT guard, Fail evidence, summary/assessment | T15 callback bypass service assessment, edge auto-pass vs callback, integrasi rollback dan live UAT belum terbukti | `PARTIAL/REPAIR - SOURCE SEEN, UAT NOT VERIFIED` |
| Perubahan Status Kerja | FE route/worklist; BE ConvertToPermanent, fail-closed master PKWTT, scheduler | Writer menimpa `Conversion` jadi `Renewal`, status efektif tidak konsisten, callback Approved tanpa draft, FE summary/row mismatch | `REPAIR - SOURCE SEEN, UAT NOT VERIFIED` |

**Defect yang SUDAH DITUTUP oleh implementasi sebelumnya dan harus dijaga:** fallback FK department salah telah dihapus; grade salary structure dicocokkan; guard probation PKWT dan mask BasicSalary tersedia; grup Menu 08 dan dua halaman konteks sudah ada. Jangan mengklaim masalah tersebut masih belum dikerjakan tanpa cek source. Tindakan baru adalah penyempurnaan jalur yang masih cacat.

**Referensi bukti code yang diperiksa** (cocokkan lagi di branch):
- BE `.../WorkforceCore/Services/WfpContractHistoryWriteService.cs:142-190` (`Active`/`IsCurrent`), BE `.../LifecycleManagement/Services/HrdEmploymentLifecycleSchedulerService.cs:43-80` (`Signed`/current), BE `.../LifecycleManagement/Services/HrdEmploymentLifecycleWorkflowIntegrationService.cs:202-212` (callback T16), BE `.../LifecycleManagement/Services/HrdEmploymentContractService.cs:620-645` (PUT update tanpa policy), `:747-824` (ApplyDecision), `:824-885` (Conversion overwritten via writer), `:1320-1390` (Summary), serta BE `.../LifecycleManagement/DTOs/EmploymentContractDtos.cs` (DTO).
- FE `.../lifecycle/employment-contracts-list-view.jsx`, `contract-renewals-list-view.jsx`, `employment-status-changes-view.jsx` untuk field mismatch yang dijabarkan pada 2.5A.

### 3.3 Aturan sumber hukum dan konflik keputusan

- Sumber resmi yang dicek untuk UU 27/2026: JDIH Kementerian Ketenagakerjaan, JDIH BPK; tanggal berlaku 28 Oktober 2026. Pasal 82 (PKWT durasi/perpanjangan + notice), 83 (tertulis Indonesia), 86 (larangan probation PKWT), 87 (kriteria pekerjaan PKWT), 92 (probation PKWTT). **Semua aturan yang diterapkan dan tanggal efektif harus disertai sumber dalam code comment policy/test dan blueprint**.
- Keputusan `HRD-DEC-089` disetujui pada 4 November 2026 tetapi memuat batas 5 tahun dari PP 35/2021, yang tidak sesuai UU 27/2026 untuk perjanjian yang tunduk aturan baru. Ini **konflik dokumen yang perlu direkonsiliasi**, bukan izin untuk retroaktif memperbaiki semua historical records.
- Jangan berasumsi PP lama seluruhnya tetap atau seluruhnya tidak berlaku; minta pemeriksaan Legal atas aturan pelaksana dan ketentuan peralihan. Jika satu aturan transisi tidak pasti, kembalikan `LEGAL_REVIEW_REQUIRED` dan cegah false eligibility untuk kasus tersebut.
- Jangan menganggap semua jabatan rumah sakit dapat memakai PKWT; validasi sifat pekerjaan berdasarkan pasal relevan, approved HRD/Legal dan kategori pekerjaan. Jika tidak ada data karakter pekerjaan authoritative, lakukan guard + workflow review, bukan hardcode valid pada semua unit.

---

## 4. Reasoning / Execution Workflow

Istilah `Reasoning` pada judul ini berarti **urutan eksekusi teknis dan verifikasi**, bukan permintaan untuk membeberkan chain-of-thought privat.

### 4.1 Preflight wajib, singkat dan terarah

1. Verifikasi `git branch --show-current`, `git rev-parse HEAD`, upstream/worktree masing-masing FE dan BE, serta branch `main` Engineering Skills, read-only. Jika HEAD bergerak sejak snapshot, lakukan `git diff` read-only untuk scope Menu 08 dan adaptasi, jangan overwrite kerja Agent lain.
2. Baca `AGENTS.md`, `CLAUDE.md`, global rules dan registered runtime Skill `build-module-backend` / `build-module-frontend`; pastikan runtime tersedia. Baca backend QBE contract/registry dan FE base component/page pattern sebelum menyentuh source.
3. Baca source target/controller/service/DTO/EF/configuration/test terdekat dan `HRD-DEC-081`, `082`, `084`, `085`, `089`, `090`; cek mode/izin tulis masing-masing repositori. **Preflight ini bukan task audit terpisah yang boleh menggantikan implementasi**.
4. Periksa apakah `docs/module-blueprints/human-resource/memory.md` tersedia pada HEAD; bila belum, catat `MEMORY_NOT_INITIALIZED`/`MEMORY_PENDING_SYNC`, jangan membuat klaim telah sync di GitHub. Semua data sensitif harus tetap di luar laporan.
5. Bandingkan **dua parent commit** FE `f095e2e0` -> `f7a1f091` dan BE `47d6545c` -> `fcdd9f4d`, baca `RecordSignedAsync`, `WfpNewHireAssignmentService`, `PersonnelActionOrderScheduledApplyWorker`, `menu-items.jsx`. Pastikan tidak ada push baru yang mengubah file target sejak snapshot; bila ada, lakukan impact review terarah read-only, jangan overwrite kerja orang lain.
6. Hasil preflight ringkas: scope file target, dependency, conflict law+blueprint, integrasi push Menu 07, urutan task siap kerja, mode sah. **Lanjut langsung BE-001**, bukan audit ulang tanpa coding.

### 4.2 Fase dokumen bila konflik canonical memerlukan perubahan

Jika mendapat `MODULE BLUEPRINT MODE` yang secara eksplisit mengizinkan path dokumentasi HRD: sinkronkan kontrak `HRD-DEC-089` dan traceability untuk bagian aturan 5 tahun yang superseded oleh UU 27/2026; pertahankan jejak keputusan serta label `NEEDS_SYNC`/status persetujuan legal transisi. Jangan menulis source pada mode dokumen. Jika akses dokumen tidak diberikan, buat patch ringkas dalam laporan dan lanjut pekerjaan backend yang independen. Jangan menunda semua task karena satu historical transition belum diputus.

### 4.3 Fase BACKEND MODE - langsung kerjakan task 001 sampai 010

- Panggil registered skill `quilvian-engineering-skills:build-module-backend` untuk setiap task; implementasikan source, tes targeted, laporan resmi.
- Urutan dependency utama: BE-001 legal service -> **BE-002 kontrak/dokumen + kontrak signing dengan WfpNewHire** -> BE-003 renewal validator -> BE-004 notice -> BE-005 workflow -> **BE-006 rollback probation** -> BE-007 conversion -> **BE-008 mirror tanggal efektif + worker awareness** -> **BE-009 cross-module FK/salary/approval provenance** -> **BE-010 regression gate**. Semua tetap 10 task; jika dependency teknis mengharuskan urutan fix BE-009 lebih awal, lakukan dalam iterasi backend terarah dan catat deviasi dalam laporan.
- Jika migration/schema diperlukan, **jangan** create/run migration tanpa izin spesifik. Siapkan schema plan/patch secara sah, kerjakan bagian yang tidak membutuhkan migration, tandai bagian DB `BLOCKED_MIGRATION_AUTHORIZATION` beserta impact. Jangan mengubah database produksi atau meniru sukses dari in-memory.
- Sebelum memulai FE, bekukan response contract, enum numerical, status dan permission lewat source/test actual. Jangan biarkan frontend menebak API baru.

### 4.4 Fase FRONTEND MODE - langsung kerjakan task 001 sampai 008

- Panggil registered skill `quilvian-engineering-skills:build-module-frontend` tiap task. Jangan menulis di BE pada fase FE; jika API mismatch, kembali ke BACKEND MODE yang diotorisasi atau lakukan CROSS-REPO hanya bila diberi target tulis kedua repositori secara eksplisit.
- Rute `contract-renewals` dan `employment-status-changes` hanya context page; jangan menambah database tambahan per route. UI menampilkan data server authoritative; semua loading/error/empty/access-denied harus jelas.
- Wajib implementasi dari skema UI-08.1 s.d. UI-08.4 dan layout base components. **Pertahankan lima entry Menu 07 yang sudah ter-push; tambah empat entry Menu 08 dan tetap hidupkan route legacy.** Uji coexist sidebar dengan role berbeda sebelum menandai FE-001 selesai. Gunakan `StatusBadge` nilai `status` valid (`active`,`inactive`,`pending`,`warning`,`info`,`danger`) dengan `label` untuk teks bisnis; jangan gunakan status tekstual Indonesia sebagai CSS key.
- Tidak boleh membuat tombol mutasi FE yang hanya mengubah local state; setelah action server, invalidate/refetch cache dan tampilkan state persist setelah refresh.

### 4.5 Validasi dan UAT live - minimal 40 skenario

**Aturan:** jalankan test dalam environment test yang sah menggunakan akun/role UAT; browser **headed** bila tersedia, simpan bukti screenshot/HTTP tanpa nama asli/NIK/nominal, lakukan retest setelah fix. Gunakan `PASS/FAIL/BLOCKED/NOT RUN`, bukan mencentang dari inspeksi file. Pengujian P0 harus lulus atau disebut blocker produksi.

| UAT ID | Submenu | Skenario wajib | Severity |
| --- | --- | --- | --- |
| AT-HRD-08-001 | Perjanjian Kerja | Draft pertama dibuat hanya dari offer approved, satu kali meski event retry | P0 |
| AT-HRD-08-002 | Perjanjian Kerja | PKWT non-eligible/salah periode ditolak backend | P0 |
| AT-HRD-08-003 | Perjanjian Kerja | Tanda tangan tanpa PDF valid ditolak | P0 |
| AT-HRD-08-004 | Perjanjian Kerja | PDF signed sah persisten setelah reload dan login ulang | P0 |
| AT-HRD-08-005 | Perjanjian Kerja | Double submit signed dan dua HRD simultan tidak menghasilkan duplikasi | P0 |
| AT-HRD-08-006 | Perjanjian Kerja | Print-data tidak bocorkan BasicSalary untuk role tanpa nominal | P0 |
| AT-HRD-08-007 | Perjanjian Kerja | Cetak dokumen tak berisi fallback `Rumah Sakit Quilvian`/identitas salah | P0 |
| AT-HRD-08-008 | Perjanjian Kerja | Kontrak sebelumnya tetap ada setelah koreksi approved | P0 |
| AT-HRD-08-009 | Perjanjian Kerja | Batch data awal tidak memalsukan TTD/masa percobaan baru | P1 |
| AT-HRD-08-010 | Perjanjian Kerja | List filter, empty/error, pagination dan detail konsisten server | P1 |
| AT-HRD-08-011 | Perpanjangan Kontrak | Kontrak baru 24 bulan diperlakukan sesuai rule baru | P0 |
| AT-HRD-08-012 | Perpanjangan Kontrak | Perpanjangan pertama <=12 bulan diperiksa legal | P0 |
| AT-HRD-08-013 | Perpanjangan Kontrak | Perpanjangan kedua <=12 bulan diperiksa legal | P0 |
| AT-HRD-08-014 | Perpanjangan Kontrak | Perpanjangan ketiga ditolak via API langsung | P0 |
| AT-HRD-08-015 | Perpanjangan Kontrak | Extension >12 bulan / endDate tidak konsisten ditolak | P0 |
| AT-HRD-08-016 | Perpanjangan Kontrak | Nomor follow-up dan riwayat unique, no overlap saat concurrency | P0 |
| AT-HRD-08-017 | Perpanjangan Kontrak | Pemberitahuan tertulis H-7 dinilai, bukti hilang/terlambat tidak jadi compliant | P0 |
| AT-HRD-08-018 | Perpanjangan Kontrak | Maker==checker ditolak backend; role lain 403 | P0 |
| AT-HRD-08-019 | Perpanjangan Kontrak | Disetujui tetapi belum TTD -> kontrak lama masih authoritative | P0 |
| AT-HRD-08-020 | Perpanjangan Kontrak | NotRenew diserahkan ke Menu 12 sekali saja, bukan clearance kedua | P0 |
| AT-HRD-08-021 | Masa Percobaan | PKWT ditolak walau master mengizinkan probation | P0 |
| AT-HRD-08-022 | Masa Percobaan | PKWTT awal dengan syarat tertulis sah membuat probation 3 bulan | P0 |
| AT-HRD-08-023 | Masa Percobaan | PKWTT tanpa syarat tertulis yang sah tidak dibuat probation | P0 |
| AT-HRD-08-024 | Masa Percobaan | Kontrak conversion tidak membuat probation | P0 |
| AT-HRD-08-025 | Masa Percobaan | Fail tanpa evidence ditolak, Lulus memerlukan alasan sesuai aturan | P0 |
| AT-HRD-08-026 | Masa Percobaan | Pengaju bukan penyetuju, returned/rejected bisa ditindaklanjuti hanya pada waktunya | P0 |
| AT-HRD-08-027 | Masa Percobaan | Auto-pass akhir periode + audit + notifikasi, rerun idempotent | P0 |
| AT-HRD-08-028 | Masa Percobaan | Approval fail vs scheduler bersamaan tidak menghasilkan dua keputusan final | P0 |
| AT-HRD-08-029 | Perubahan Status Kerja | Submit conversion hanya dari PKWT sah/current | P0 |
| AT-HRD-08-030 | Perubahan Status Kerja | Master PKWTT hilang -> fail-closed tanpa data berubah | P0 |
| AT-HRD-08-031 | Perubahan Status Kerja | Approved -> draft bukan langsung PKWTT current | P0 |
| AT-HRD-08-032 | Perubahan Status Kerja | Signed future-date -> baru efektif pada hari mulai | P0 |
| AT-HRD-08-033 | Perubahan Status Kerja | Kontrak sebelum/sesudah dan status historis tak terhapus | P0 |
| AT-HRD-08-034 | Perubahan Status Kerja | PKWT->PKWTT tanpa probation dan tanpa gaji otomatis | P0 |
| AT-HRD-08-035 | Perubahan Status Kerja | Dua HRD simultaneous/retry tidak membuat conversion ganda | P0 |
| AT-HRD-08-036 | Perubahan Status Kerja | UI before/after, PDF signed, status effective/refetch benar | P1 |
| AT-HRD-08-037 | Lintas Submenu | Sidebar tepat empat child, old route tetap hidup, role menu benar | P1 |
| AT-HRD-08-038 | Lintas Submenu | IDOR lintas unit/site dan download/print uang sensitif ditolak | P0 |
| AT-HRD-08-039 | Lintas Submenu | Scheduler tanggal Indonesia, multiple instances, lapsed -> scheduling hold | P0 |
| AT-HRD-08-040 | Lintas Submenu | Browser desktop/tablet, keyboard/focus, API fail state, empty state, retest | P1 |

### 4.5A Regression test khusus dependency push Menu 07 — **10 kasus tambahan, tanpa mengubah 40 UAT utama**

**Semua kasus adalah perintah uji untuk Agent, bukan klaim telah PASS.** Gunakan data uji anonim, DB test terotorisasi, dan bukti respons/status. Pemilik perbaikan utama berada dalam task FIN yang disebutkan (tidak membuat nomor task 011/009 baru).

| Regression ID | Task induk | Prasyarat, stimulus dan assertion yang wajib diuji | Prioritas |
| --- | --- | --- | --- |
| `REG-HRD-08-001` | BE-002/009/010 | Kontrak pertama dari approved offer dengan `MstSalaryStructure` **tidak ada**; `record-signed` gagal terstruktur, **tanpa** Signed/IsCurrent, assignment, probation, atau onboarding completed; periksa compensating cleanup PDF | P0 |
| `REG-HRD-08-002` | BE-002/006/009/010 | SalaryGrade sah A tetapi fallback salary structure aktif milik grade B; API **menolak**, tidak mencatat WfpSalaryAssignment salah dan tidak menjalankan probation | P0 |
| `REG-HRD-08-003` | BE-002/009/010 | `MstOrganizationUnit.DepartmentId` dan rujukan department posisi tidak valid/hilang; **jangan** memasukkan `OrganizationUnitId` sebagai `DepartmentId`; dapat 4xx yang dapat ditangani dan no partial FK | P0 |
| `REG-HRD-08-004` | BE-008/009/010 | TTD kontrak tanggal D untuk tanggal masuk D+14; assignment historis tercatat future, **mirror PrimaryDepartmentId/PrimaryPositionId dan tampilan unit saat ini tidak berubah sebelum D+14** | P0 |
| `REG-HRD-08-005` | BE-008/009/010 | Pada D+14 atau kontrak signed same-day, as-of reader/worker mengaktifkan penempatan yang benar sekali, sync mirror yang tepat, timezone WIB, rerun idempotent | P0 |
| `REG-HRD-08-006` | BE-002/006/010 | Simulasi kegagalan setelah validasi assignment tetapi sebelum `CreateForContractAsync`/completion onboarding; rollback DB Signed/assignment/probation/checklist dan verifikasi berkas orphan dikelola | P0 |
| `REG-HRD-08-007` | BE-002/006/009/010 | Dua user/dua request `record-signed` bersamaan dan retry pada kontrak awal; tepat satu kontrak current, satu primary assignment tiap jenis, satu probation sah, satu salary awal (jika disetujui) | P0 |
| `REG-HRD-08-008` | BE-007/008/009/010 | SK mutasi/promosi, renewal, conversion dan initial placement batch tidak langsung mengganti salary; worker `PersonnelActionOrderScheduledApplyWorker` **tidak mengaktifkan kontrak draft / initial hire secara salah** | P0 |
| `REG-HRD-08-009` | FE-001/007/008 | Sidebar Menu 07 lima child **tetap** aksesibel, Menu 08 tepat empat child, unique pathname/keys, role permission benar, route lama `/hr/lifecycle/employment-contracts` dan `/probations` hidup | P1 |
| `REG-HRD-08-010` | BE-002/009/010 + FE-003 | `WfpSalaryAssignment` awal terlacak ke approved offer, grade dan struktur cocok, nilai tidak berubah melalui SK; user tanpa hak nominal tidak menerima `BasicSalary` di API cetak/detail/download | P0 |

**Evidence gate:** tuliskan SQL query **read-only** dan redacted result yang diizinkan, API request/response anonim, audit event, screenshot headed untuk REG-009 bila lingkungan tersedia. Jika tidak ada DB UAT atau browser, tandai `NOT RUN/BLOCKED`; jangan menganggap source inspection lulus. Uji ulang existing `AT-HRD-08-001`, `003`, `004`, `021`, `022`, `027`, `037`, `038`, `039` sesudah patch karena berbagi jalur signing/onboarding/worker/sidebar.

### 4.5B AUDIT REPAIR TEST - wajib ulang setelah perubahan P0, dan perbarui bukti 40+10 asli

**Jangan menghapus `AT-HRD-08-001..040` atau `REG-HRD-08-001..010`.** Peta uji tambahan berikut adalah bukti khusus defect audit; bisa dihubungkan ke UAT/regresi resmi. Tidak membuat nomor FIN baru. Setiap kasus harus berisi mode tes, actor, data anonim, input, expected status HTTP, perubahan row persisten sebelum/sesudah, dan log yang memang dihasilkan.

| Audit ID | Tes wajib dan syarat PASS faktual |
| --- | --- |
| AUD-08-01 | Tanda tangan kontrak future-dated -> status Signed, IsCurrent=false; current lama tak berubah; jalankan tick tanggal efektif -> tepat satu Active/current, profile/record ikut efektif. Jalankan ulang tick -> tidak ada duplikasi |
| AUD-08-02 | Callback T16 maker==checker -> ditolak; checker sah approve -> Approved + satu follow-up, bukan hanya boolean; callback dua kali tidak menambah kontrak |
| AUD-08-03 | Konversi PKWT->PKWTT -> `HistoryType=Conversion` persisten setelah reload, previous link benar, `EndDate=null`, `ProbationEndDate=null` |
| AUD-08-04 | POST dan PUT masa PKWT yang melampaui batas/kategori salah -> 400/409, DB tidak berubah. Uji update keputusan yang Submitted dan state final secara terpisah |
| AUD-08-05 | 24 bulan maksimum + 1 hari, tanggal 31, leap year, rentang 12 bulan + 1 hari -> ditolak jika melampaui batas yang sah; boundary legal tepat diterima |
| AUD-08-06 | Dua request submit concurrent dan 2 callback approve concurrent -> satu open decision, satu follow-up, satu nominal/assignment, tidak ada duplicate number |
| AUD-08-07 | Notifikasi tanpa lampiran/sumber bukti => tidak `ON_TIME`; dengan bukti sah status berdasar waktu yang tervalidasi; backdate fraud ditolak/ditandai review |
| AUD-08-08 | GET `salary-detail` dengan ViewAmount+scope -> response detail terotorisasi atau kontrak resmi dinyatakan berbeda; tanpa permission -> 403; tidak pernah hanya `true` saat mengklaim nominal |
| AUD-08-09 | Dua `record-signed` simultan + salary configuration missing -> satu hasil sah maksimal; failed path rollback contract/placement/probation/onboarding dan orphan file cleanup; tidak ada informasi internal `ex.Message` di response |
| AUD-08-10 | Cross-site/employee IDOR GET list, detail, scan, print, decision, amount: role tanpa scope -> 403/404 (menurut API contract); NIK/nominal tidak bocor dalam JSON/Network |
| AUD-08-11 | Summary `hospitalSiteId` A vs B membatasi hasil; H-60 menghitung rentang benar termasuk border; H-30 tidak diberi label H-60 tanpa data |
| AUD-08-12 | GET summary nonzero -> angka SummaryGrid Perjanjian Kerja identik, reload sama, tidak menghasilkan `undefined` |
| AUD-08-13 | GET renewal summary nonzero -> angka H-60/H-14/H-7 sesuai JSON, overdue tidak dihitung dari pending; status filter terjaga |
| AUD-08-14 | Conversion Submitted/Approved Draft/Signed future/Active/Rejected -> setiap kartu sesuai data dan label; tidak ada angka statis/rekaan |
| AUD-08-15 | Worklist renewal/conversion pegawai nama+kode benar, badge notifikasi benar, klik Rincian navigasi ke `/employment-contracts/{contractHistoryId}` valid (tidak `undefined`) |
| AUD-08-16 | Baris perpanjangan ke-2 tampil `Ke-2` dari riwayat/DTO sah, tidak `Ke-1` hasil fallback; jika belum ada field nyatakan unknown, jangan palsu |
| AUD-08-17 | Pergantian hari 00:00 WIB, dua instance worker paralel, restart worker dan jadwal future: tidak memutasi sebelum waktu, tidak menggandakan record, status konsisten |
| AUD-08-18 | Report `VERIFIED WORKING` hanya setelah test runner/API/DB/browser headed benar dijalankan; screenshot terautentikasi redacted dan report per test; jika tidak ada environment wajib `NOT RUN` |

**Negative & UI retest tambahan:** refresh browser setelah write; login HRD maker/checker berbeda; akses dengan role tanpa ViewAmount; failure jaringan 500, file terlalu besar/tidak valid, duplicate submit, tooltip error dan focus keyboard; tablet portrait 768px, tablet landscape 1024px, desktop 1366px. Jika tidak ada akun UAT atau DB UAT, lakukan semua tes unit yang dapat dijalankan, tandai sisanya `BLOCKED/NOT RUN`, jangan mengubah DB produksi.

**Bukti per case harus ada:** test ID, SHA, timestamp WIB, environment, command+exit, actor role anonim, request dan response status (redacted), SQL read-only sebelum/sesudah jika izin, dan screenshot/headed log untuk UI. Jangan gunakan screenshot palsu, payload dummy produksi, atau kalimat "manual test PASS" tanpa bukti. Validasi `dotnet build`, `npm run lint:errors`, targeted unit, Next build sesuai policy secara nyata. Pastikan laporan menjelaskan warning yang masih ada dan mengapa tidak berdampak/masih pending.

### 4.6 Definition of Done dan laporan evidence

**DoD mutlak:**

1. Empat Submenu sesuai nama exact, navigasi sah dan akses tersaring. Dua route baru membuka tampilan nyata, bukan placeholder.
2. Semua jalur BE memakai DB persisten dan validation authoritative, approval maker-checker, versioned legal policy, audit aktor, status efektif dan dokumen.
3. `FixedTermCheckAsync` dan submit memakai satu validator sesuai hukum terbaru untuk kasus yang jelas tercakup; konflik transisi historical diungkap jujur dan tidak dimigrasi tanpa approval.
4. Dokumen print tidak memunculkan nominal bagi user tak berizin, fallback identitas salah, maupun teks kontrak valid palsu.
5. Payroll, penempatan, penjadwalan, kredensial, offboarding tetap bounded; tidak ada perubahan gaji/status klinis otomatis yang dilarang. **Regresi sumber `WfpNewHireAssignmentService` diselesaikan:** tidak ada department FK dari ID unit, grade–structure mismatch, mirror future-dated premature, atau assignment/PKWTT/probation parsial akibat error signing.
6. FE hanya memakai API real dan komponen dasar; tidak ada mock data production, false success, tombol pending tanpa backend.
7. Targeted tests, backend permission tests, frontend lint/build, browser headed UAT (jika diotorisasi), hasil PASS/FAIL/NOT RUN per kasus, bug retest dengan evidence.
8. Laporan task official `task/report/backend/FIN-HRD-08-BE-xxx.md` dan `task/report/frontend/FIN-HRD-08-FE-xxx.md` atau path setara sesuai rule; rekonsiliasi roadmap/traceability hanya dalam mode tulis dokumen sah.
9. Status paling akhir wajib satu di antara `VERIFIED WORKING`, `IMPLEMENTED NOT VERIFIED`, `PARTIAL`, `REPAIR`, `BLOCKED`, `NOT VERIFIED` per Submenu dengan alasan dan bukti. Tidak boleh menyatakan 100%/READY FOR PRODUCTION jika P0/QA/UAT masih terbuka.
10. Checkpoint `memory.md` (jika berizin) dengan SHA terbaru, keputusan hukum dan next action; jika belum tersimpan `MEMORY_PENDING_SYNC`, jangan klaim GitHub berubah.

---

## 5. Stop conditions

**JANGAN BERHENTI** hanya karena audit selesai, API ditemukan, satu layar selesai, hasil lint/build hijau, atau ada defect yang bisa diperbaiki. **SELESAIKAN SOURCE CODE** sesuai izin, dokumentasikan bukti dan perbaiki regresi dalam scope.

**STOP hanya pada area yang terkena** apabila: branch/upstream salah atau worktree konflik; registered runtime engineering skill tidak tersedia; mode/target tulis tidak diotorisasi; operasi migration/DB/commit/deploy memerlukan izin terpisah; sumber hukum transisi kasus historis belum mendapat keputusan Legal; kontrak lintas modul/permission tidak sah; lingkungan UAT/data sensitif tidak mendapat izin. **Lanjutkan task independen**, laporkan blocker dengan file/function, alasan, dampak, gate yang diperlukan, dan next action. Jangan memalsukan hasil dengan data in-memory/mock.

**Larangan operasional eksplisit:** tidak `git commit`, `git push`, `git pull`, `git merge`, `git rebase`, membuat/jalankan migration, mengubah database/seed produksi, deploy, membuka secrets, atau mengubah rule engineering. Jangan menimpa perubahan developer lain. Bila perintah build/test dilarang environment, catat `NOT RUN` dengan sebab.

## 6. Output

Setelah implementasi, Agent wajib menghasilkan **laporan hasil eksekusi** (berbeda dengan master instruksi ini) yang menunjukkan:

1. **Ringkasan konkret** per task `FIN-HRD-08-BE-001`..`010` dan `FIN-HRD-08-FE-001`..`008`: `DONE/PARTIAL/BLOCKED`, file apa yang diedit, symbol/line, apakah DB/endpoint real, dan bukti test.
2. **Tabel 4 Submenu** `Perjanjian Kerja`, `Perpanjangan Kontrak`, `Masa Percobaan`, `Perubahan Status Kerja`: fitur, status, API live, UI route, acceptance pending.
3. **Perubahan kontrak**: list endpoint existing yang dipakai, endpoint baru aktual (jika dibuat), HTTP method/status, DTO fields, state transitions, permissions, legal policy version, bukti transaksi/idempotency dan eksekusi worker.
4. **MenuItems dan UI**: route baru/alias, schema implemented vs target UI-08.1-08.4, screenshot anonymized, loading/error/empty/auth/responsive.
5. **Testing**: command + timestamp + output ringkas `PASS/FAIL/NOT RUN`, **40 UAT utama + 10 regression `REG-HRD-08-001`..`010`** dengan evidence link/screenshot atau alasan blocker, bukti retest setelah fix. Wajib sertakan rollback signing, FK departemen, grade–structure, future-date mirror dan coexist Menu 07/08.
6. **Governance**: Skill runtime yang dipanggil per task, QBE/frontend base gate, targeted docs/reports yang benar-benar ditulis, git diff/status, HEAD FE/BE/Skills saat akhir, tanpa klaim commit/push tanpa izin.
7. **Isu tersisa**: legal transitional decisions, mismatch law/backend, migration authorization, test DB/runtime blockers, next action <=3 item, memory sync `MEMORY_SYNCED/MEMORY_PENDING_SYNC` sesuai fakta.
8. **Revisi laporan resmi pasca audit:** PERBARUI `docs/module-blueprints/human-resource/docs/update/kontrak-dan-masa-kerja/LAPORAN_EKSEKUSI_HRD_MENU_08_KONTRAK_DAN_MASA_KERJA.md` pada workspace berizin, bukan hanya append kata DONE. Untuk setiap `AUD-08-01..18`, jelaskan source path, jalur perbaikan, respons aktual, bukti retest dan status `PASS/FAIL/BLOCKED/NOT RUN`. Turunkan status laporan dari `VERIFIED WORKING` bila masih ada P0 atau bukti browser/DB kurang. Sinkronkan ringkasan `task/report/backend/FIN-HRD-08-BE-KONTRAK-DAN-MASA-KERJA.md` serta frontend analog atau per-task report sesuai aturan output. Teks `MEMORY_SYNCED` hanya boleh dipakai setelah file memory persisten ditemukan dan diverifikasi. Jangan mengganti SHA laporan dengan SHA baru sebelum melihat status Git aktual.

**Template ringkas yang wajib diisi Agent saat menyelesaikan pekerjaan:**

```text
MENU 08 / KONTRAK & MASA KERJA - LAPORAN EKSEKUSI
FE branch/SHA: ...
BE branch/SHA: ...
Skills SHA: ...
Backend FIN: 001 ... 010 [status + file bukti]
Frontend FIN: 001 ... 008 [status + route/file bukti]
Submenu: [empat baris dan state]
Tabel API/DB: [persistance, permission, test]
UU 27/2026 policy: [rule version + legal transition status]
Validation: build/lint/unit/integration/security/UAT [PASS/FAIL/NOT RUN]
Regression: [REG-HRD-08-001..010 status PASS/FAIL/NOT RUN, row state, bukti rollback, link 2 commit Menu 07]
Remaining blockers: [gate dan owner]
Git status: [read-only evidence]
Memory sync: [status benar]
```

**PERINTAH FINAL KEPADA AGENT:** Pekerjaan Menu 08 sudah memiliki source; JANGAN membuat ulang Menu/halaman atau berhenti di review. Mulai dari preflight sah, kemudian **IMPLEMENTASIKAN SEKARANG PERBAIKAN 18 DEFECT AUDIT** di dalam seluruh pekerjaan `FIN-HRD-08-BE-001` sampai `FIN-HRD-08-BE-010` di backend pada branch dan mode yang diizinkan. Setelah kontrak API/backend siap, **IMPLEMENTASIKAN** `FIN-HRD-08-FE-001` sampai `FIN-HRD-08-FE-008` pada frontend, lalu lakukan test, UAT, retest, dan laporan evidence. Jangan berakhir hanya dengan membaca kode atau menulis dokumen. Jangan bertanya kembali mengenai planning yang sudah disetujui.
