# Laporan Perubahan Backend — Perbaikan Register Masalah SDM (`MSL-01`, `02`, `04`–`13`)

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `MSL-01`, `MSL-02`, `MSL-04`, `MSL-05`, `MSL-06`, `MSL-07`, `MSL-08`, `MSL-09`, `MSL-10`, `MSL-11`, `MSL-12`, `MSL-13` |
| Judul | Perbaikan masalah backend P0/P1 dari register masalah modul SDM |
| Roadmap | [register-masalah-hr.md](../../../roadmap/register-masalah-hr.md) bagian 3 dan 4 |
| Trace | `HRD-DEC-127`, `HRD-DEC-184`, `FR-PAY-25`, `VR-PAY-22`, `VR-PAY-24`, `DED-B8`, `VR-PRF-04`, laporan `FE-HRD-R32`, `R33`, `R34`, `R35`, `R40`, `R43`, `R45`, `R46` |
| Wewenang | Permintaan eksplisit pemilik pada 18 November 2026: "sekarang perbaiki seluruh masalah nya" |
| Klasifikasi | `HEAVY` — 12 masalah, 40 berkas, menyentuh hak akses, data gaji, dan konfigurasi runtime |
| Task mode | `BACKEND` (source saja; tanpa migration, tanpa eksekusi database, tanpa git) |
| Target tulis | `NewQuilvianSystemBackend` (source) dan laporan ini |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `90e36dee` (branch `MHamzah`), perubahan belum di-commit |
| Tanggal | Rabu, 18 November 2026 |
| Status | 🟡 Source selesai untuk 11 masalah (termasuk `MSL-10` di putaran 2, migration ditulis tetapi belum dijalankan) dan lolos build + QBE; 1 masalah ditutup sesuai rancangan (`MSL-08`); `MSL-14` dan tahap 1 `MSL-15` dikerjakan di putaran 2. Uji runtime di browser belum dijalankan (butuh izin menyalakan backend di DB dev bersama) |

---

## 1. Ringkasan untuk pembaca umum

| No. | Masalah | Hasil | Contoh di rumah sakit setelah perbaikan |
|---|---|---|---|
| `MSL-01` | Penerbitan surat kuasa Plh selalu galat 500 | ✅ Diperbaiki | Kepala Ruang ICU menunjuk Ketua Tim sebagai Plh tiga hari; surat kuasa terbit dan Plh langsung melihat kotak masuk persetujuan |
| `MSL-02` | Penilaian Kinerja Saya selalu galat dan bisa membuka lembar orang lain | ✅ Diperbaiki | Perawat membuka layarnya dan melihat lembar draf berisi indikator KPI; mengganti ID di alamat tidak lagi membuka lembar rekan |
| `MSL-04` | Slip gaji dicocokkan dengan ID akun, sehingga pegawai tidak melihat slipnya | ✅ Diperbaiki | Ns. Mira melihat slip November dan PIN tanggal lahirnya diterima |
| `MSL-05` | Daftar Plh memuat semua unit | ✅ Diperbaiki | Kepala Ruang Bedah hanya melihat surat kuasa yang ia berikan atau terima |
| `MSL-06` | Siapa pun bisa mengisi hasil proctoring FPPE | ✅ Diperbaiki | Hanya dokter proctor yang ditunjuk komite yang bisa menilai kasus |
| `MSL-07` | Restitusi: pegawai tidak bisa melihat riwayat klaimnya; anti persetujuan diri tidak jalan; pengaturan tanpa audit; sinkron piutang mengarang data | ✅ Diperbaiki | Pegawai melihat riwayat klaim kacamatanya; atasan yang mengajukan klaim tidak bisa menyetujui klaimnya sendiri |
| `MSL-08` | Presensi mandiri tanpa `AccessPermission` | ⚪ Ditutup — sesuai rancangan | Semua pegawai tetap bisa absen; lihat bagian 3.8 |
| `MSL-09` | PDF slip gaji tidak bersandi | ✅ Diperbaiki | PDF terkunci dengan tanggal lahir DDMMYYYY, misalnya 03061999 |
| `MSL-10` | Pengaturan lembur hanya di memori server | ⛔ Tertahan | Butuh tabel baru; pembuatan migration menunggu izin terpisah |
| `MSL-11` | Galat 500 meneruskan pesan teknis berbahasa Inggris | ✅ Diperbaiki | Pengguna membaca "Data tidak dapat disimpan karena data yang sama sudah tercatat" |
| `MSL-12` | Settlement SPPD tanpa nomor bukti dan tanpa pemeriksaan status | ✅ Diperbaiki | Settlement yang lebih bayar tidak bisa ditutup tanpa nomor bukti setor balik |
| `MSL-13` | Worker latar tetap jalan di runtime `Web` | ✅ Diperbaiki untuk 35 worker SDM | Di Blue-Green, worker SDM hanya berjalan di container `Worker`, tidak tiga kali |

---

## 2. Backend Governance Preflight

| Field | Nilai |
|---|---|
| Area / Module | `Corporate/HumanResource` (WorkflowManagement, PerformanceManagement, PayrollManagement, BusinessTravelManagement), `Corporate/PayrollManagement` (service), `Middlewares`, `Shared/HumanResource`, `Program.cs` |
| Keberlakuan | `TOUCHED LEGACY` untuk seluruh berkas lama; `NEW CODE` untuk `HrSafeError.cs` dan `WorkflowDelegationExplicitPermissions.cs` (keduanya tanpa model persisted) |
| Entity/tabel baru | Tidak ada — `QBE-MOD-002`/`QBE-MOD-003` tidak berlaku |
| QBE yang dicek | `Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` pada 16 berkas utama: semuanya `Final result: PASS`, 0 temuan |
| Hak akses | Tidak ada nama peran di kode. Lingkup "semua unit" memakai penanda `AccessExplicitPermission` yang diberikan admin lewat Akses Role |

---

## 3. Perubahan per masalah

### 3.1 `MSL-01` — Penerbitan surat kuasa Plh

**Penyebab sebenarnya ada tiga**, bukan satu seperti dugaan di register:
1. `CK_TrxApprovalDelegation_WorkflowScope` menolak karena `AppliesToAllWorkflows`, `WorkflowDefinitionId`, dan `ScopeDefinitionJson` kosong.
2. `DelegatorUserId`/`DelegateUserId` diisi ID profil tenaga kerja, padahal kolomnya ber-FK ke `AspNetUsers`. Setelah butir 1 diperbaiki, simpan akan gagal di FK ini.
3. Plh satu hari (tanggal mulai = selesai) melanggar `CK_TrxApprovalDelegation_EffectivePeriod` (akhir harus lebih besar dari awal).

**Perbaikan** di `ApprovalDelegationV2Service.CreateDelegationAsync`:
- Akun pemberi dan penerima dicari di `AspNetUsers` lewat `EmployeeId`, lalu `WorkforceProfileId`. Bila Plh belum punya akun login, ditolak dengan pesan: "… belum memiliki akun login, sehingga belum bisa ditunjuk sebagai Plh."
- Cakupan `ALL` → `AppliesToAllWorkflows = true`. Cakupan per jenis (misalnya `LEAVE,OVERTIME`) → `ScopeDefinitionJson = {"requestTypes":["LEAVE","OVERTIME"]}`.
- Tanggal dinormalkan ke UTC; bila akhir ≤ awal, akhir digeser ke 23.59.59 hari yang sama.
- `DelegatorWorkforceProfileId`/`DelegateWorkforceProfileId` diisi (null bila profil kosong).
- Blok `catch (Exception)` yang meneruskan pesan EF dihapus; galat teknis kini ditangani middleware (`MSL-11`).

### 3.2 `MSL-05` — Daftar dan pencabutan delegasi per pemilik

- `GET /api/v1/hrd/workflow/delegations` menyaring surat kuasa tempat pegawai login menjadi pemberi **atau** penerima kuasa.
- Penanda hak baru `WorkflowDelegation : ViewAllDelegations` ("View And Revoke All Delegations") di `WorkflowDelegationExplicitPermissions.cs`. Pemegangnya melihat dan mencabut surat kuasa seluruh unit. Contoh: staf SDM yang mengurus Plh lintas unit diberi hak ini lewat Akses Role.
- `PUT {id}/revoke` hanya untuk pemberi kuasa atau pemegang hak di atas; surat kuasa yang sudah tidak aktif ditolak; `RevokedByUserId` kini terisi. Pencabutan oleh orang lain dijawab `403`.
- Ukuran halaman dibatasi 1–100.

### 3.3 `MSL-02` — Penilaian kinerja

- Semua endpoint `PerformanceAppraisalsController` kini memakai `workforce_profile_id` dari token. Parameter query `workforceProfileId`, `managerId`, `reviewerId`, dan `staffId` **dihapus**.
- Siklus ditentukan server bila tidak dikirim: `ACTIVE_ASSESSMENT`, lalu `CALIBRATION`, lalu `GOAL_SETTING`. Tanpa siklus terbuka, `404` dengan pesan "Belum ada siklus penilaian kinerja yang sedang berjalan…".
- `GET my-appraisal` **tidak lagi menulis**. Bila lembar belum ada, server mengembalikan draf belum tersimpan (`id` = GUID kosong) berisi semua indikator KPI aktif.
- `POST self-submit` membuat lembar, lengkap dengan atasan langsung (dari `WfpManagerAssignment` aktif) dan atasan kedua (atasan dari atasan). Tanpa atasan tercatat, ditolak dengan pesan yang meminta pegawai menghubungi SDM.
- `GET subordinates` hanya memuat lembar milik bawahan langsung atau bawahan lini kedua.
- `manager-assess`, `second-line-review`, dan `acknowledge` memeriksa bahwa pemanggil adalah atasan langsung, atasan kedua, atau pemilik lembar; selain itu `403`.
- Nama pegawai kini dari profil, menggantikan teks "Pegawai (xxxxxxxx)" dan unit tetap "Instalasi Rawat Inap".

### 3.4 `MSL-04` — Slip gaji

- Controller memakai kunci pegawai dari token (`workforce_profile_id`, lalu `employee_id`), bukan ID akun. Akun tanpa data pegawai dijawab `403` dengan pesan jelas.
- Service memetakan satu kunci menjadi pasangan (`WorkforceProfileId`, `MstEmployee.Id`), sehingga slip yang hanya mencatat salah satunya tetap cocok.
- Rincian dan unduhan kini juga menolak slip yang belum dirilis (`IsEmployeeVisible`/status) dan slip milik orang lain; keduanya dijawab "Dokumen slip gaji tidak ditemukan." (`404`), bukan `401`, supaya frontend tidak mengira sesi PIN habis.

### 3.5 `MSL-06` — Proctoring FPPE

`VerifyCaseLogAsync` menerima ID proctor dari token. Penilaian ditolak `403` bila pemanggil bukan `AssignedProctorWorkforceId` penugasan itu, atau bila `proctorWorkforceId` di isian berbeda dari akun login. Kasus yang sudah diverifikasi tidak bisa dinilai ulang (`400`). `ProctorWorkforceId` di DTO kini opsional.

### 3.6 `MSL-07` — Restitusi

| Butir | Perubahan |
|---|---|
| Lihat klaim sendiri | Endpoint baru `GET claims/mine` dan `GET claims/mine/{id}` dengan hak `RestitutionClaim : ViewOwn` |
| Kuota | `GET claims/quota` tanpa `employeeId` menghitung kuota pegawai login. Kuota pegawai lain hanya untuk pemegang `RestitutionClaim : Read` |
| Pengajuan | `POST claims` selalu atas nama pegawai login; `employeeId` lain ditolak `403` |
| Anti persetujuan diri (`DED-B8`) | Keempat keputusan (atasan, HR, final, tolak) menolak `403` bila `claim.EmployeeId` = `employee_id` pemutus. Pemeriksaan lama membandingkan dengan ID akun sehingga tidak pernah cocok |
| Pengaturan plafon | Endpoint baru `PUT settings/{id}`. Nilai sebelum dan sesudah dicatat ke log audit platform (`LoggerService.AuditAsync`, level `AUD`) beserta pelakunya; `POST` upsert juga dicatat |
| Sinkron piutang Finance | Cabang lama yang **mengarang 10 pinjaman Rp6.000.000** untuk 10 pegawai pertama dihapus. Bila payload kosong, sumbernya kini `FinReceivableInstallmentPlan` berstatus `DISETUJUI` atas piutang `EMPLOYEE_BENEFIT` yang masih terutang |

Asumsi yang perlu dikonfirmasi pemilik: kategori potongan untuk piutang manfaat karyawan memakai `KasbonPinjamanRS` (prioritas 2), karena daftar kategori yang ada hanya `GantiRugiAsetKasir`, `KasbonPinjamanRS`, `CicilanKoperasi`, `IuranPaguyuban`.

### 3.7 `MSL-09` — PDF slip gaji bersandi

- Generator PDF manual (tanpa tabel xref yang sah, nama RS tertulis tetap, potongan BPJS diperkirakan 60% dari total) diganti dokumen QuestPDF dengan watermark "RAHASIA – nama – NIK".
- PDF dikunci AES-256 lewat `QuestPDF.Fluent.DocumentOperation.Encrypt` (qpdf bawaan paket QuestPDF 2026.7.1 yang sudah terpasang; tidak ada paket baru). Sandi pembuka = tanggal lahir **DDMMYYYY**; sandi pemilik acak per dokumen; cetak boleh, salin teks tidak.
- Tanpa tanggal lahir, PDF tidak dibuat dan dijawab `409`: "PDF slip gaji belum dapat dibuat karena tanggal lahir Anda belum tercatat…".

Asumsi: format sandi DDMMYYYY mengikuti petunjuk layar lama dan `FR-PAY-25` ("kombinasi tanggal lahir"), sengaja berbeda dari PIN layar (`ddMMyy`).

### 3.8 `MSL-08` — Ditutup sesuai rancangan

`AttendanceSelfServiceController` sengaja ditandai `IsSystemOnly = true` dan `VisibleInRoleAccess = false`, dengan pengamanan berbasis kepemilikan data. Di `AccessPermissionService.HasAccessAsync`, aksi system-only **hanya diloloskan untuk superadmin**. Memasang `[AccessPermission("MyAttendance", "Read")]` sesuai rekomendasi lama akan membuat **seluruh pegawai tidak bisa absen**, dan tidak bisa diperbaiki dari layar Akses Role karena hak itu tidak tampil. Karena semua pegawai memang wajib absen, rancangan sekarang dipertahankan. Butir menu **Presensi Saya** tetap terbuka untuk semua pegawai; akun tanpa data pegawai melihat pesan dari layar.

### 3.9 `MSL-10` — Tertahan izin migration

Parameter lembur `CtoValidityDays`, `MonthlyCutoffDay`, dan `PostClaimCitoGraceDays` tidak punya kolom di tabel mana pun (`MstOvertimePolicy` hanya menampung sebagian). Menyimpannya permanen butuh tabel baru `HrdOvertimeSetting` beserta migration. `CLAUDE.md` mewajibkan izin terpisah untuk **pembuatan** migration, jadi belum dikerjakan.

### 3.10 `MSL-11` — Pesan galat aman

- Helper baru `Shared/HumanResource/Helpers/HrSafeError.cs`: galat dianggap "bisnis" bila jenisnya galat aturan (`InvalidOperationException`, `ArgumentException` selain `ArgumentNullException`, `KeyNotFoundException`, `UnauthorizedAccessException`, `ValidationException`) **dan** dilempar kode aplikasi sendiri, atau jenis exception-nya didefinisikan di aplikasi. `DbUpdateException` tidak pernah dianggap bisnis.
- 120 blok `catch (Exception ex)` di 32 controller SDM yang meneruskan `ex.Message` diberi filter `when (HrSafeError.IsDomain(ex))`. Galat bisnis tetap dijawab seperti sebelumnya; galat teknis diteruskan ke middleware.
- `GlobalExceptionMiddleware` mencatat detail ke log, lalu menjawab:

| Penyebab | Status | Pesan |
|---|---|---|
| SQLSTATE `23505` (data ganda) | `409` | "Data tidak dapat disimpan karena data yang sama sudah tercatat. Periksa kembali isian Anda." |
| SQLSTATE `23503` (rujukan tidak ada) | `409` | "…merujuk data lain yang tidak ada atau sudah dihapus. Muat ulang halaman lalu pilih kembali datanya." |
| SQLSTATE `23xxx` lain (check, not null) | `409` | "…tidak memenuhi aturan penyimpanan…" |
| Bentrok simpan bersamaan (`DbUpdateConcurrencyException`, `40001`, `40P01`) | `409` | "Data ini baru saja diubah oleh pengguna lain…" |
| `DbUpdateException` lain | `500` | "Data belum dapat disimpan karena gangguan pada basis data…" |
| Lainnya | `500` | "Terjadi kesalahan pada server." |

### 3.11 `MSL-12` — Settlement perjalanan dinas

- `verify` hanya untuk status `Submitted` atau `Verified`.
- `close` hanya untuk status `Verified`. Nomor bukti wajib bila ada lebih bayar (setor balik pegawai) atau kurang bayar (bayar RS). Settlement seimbang boleh tanpa nomor.
- `CloseSettlementDto.PaymentOrRefundRef` kini opsional. Sebelumnya `[Required]` justru **menolak** settlement seimbang, karena frontend mengirim string kosong.

### 3.12 `MSL-13` — Worker latar

Komentar `Program.cs` menyatakan produksi Blue-Green menjalankan BLUE dan GREEN sebagai `Web`, plus satu container `Worker`. Akibatnya 43 worker di luar gerbang berjalan tiga kali di produksi.
- **35 worker SDM** kini didaftarkan dengan `if (runBackgroundJobs)`.
- `MedicalRecordCompletenessQueueWorker` **harus tetap** di proses Web, karena memproses antrean memori yang diisi request HTTP.
- **7 worker milik modul lain** tidak diubah, karena bukan milik modul SDM: `MedicalRecordCompletenessRecheckHostedService`, `CliSurgicalSiteSurveillanceWorker`, `CliTransfusionReactionNoticeWorker`, `CliEquipmentUsageWorker`, `BilInvoiceSyncWorker`, `PrescriptionReservationExpiryWorker`, `ClinicalFactDispatchWorker`. Ketujuhnya penjadwal berkala dan perlu digerbangi oleh pemilik modulnya.

---

## 4. Dokumentasi endpoint yang berubah

#### Corporate - HRD Workflow Delegations

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/hrd/workflow/delegations` | Daftar surat kuasa milik pegawai login; semua unit bila punya hak lingkup | `WorkflowDelegation : ViewDelegationList` (+ `ViewAllDelegations` untuk semua unit) |
| `POST` | `/api/v1/hrd/workflow/delegations` | Terbitkan surat kuasa Plh. Body: `{ delegateEmployeeId, startDate, endDate, delegatedRequestTypes, delegationReason }` → `201` | `WorkflowDelegation : CreateDelegation` |
| `PUT` | `/api/v1/hrd/workflow/delegations/{id}/revoke` | Cabut surat kuasa (pemberi kuasa atau pemegang `ViewAllDelegations`) → `200`/`403`/`404` | `WorkflowDelegation : RevokeDelegation` |

#### Corporate - HRD Performance Appraisals & Multi-Level Review

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/hrd/performance/appraisals/my-appraisal?cycleId=` | Lembar sendiri (draf belum tersimpan bila belum ada) → `200`/`403`/`404` | `PerformanceAppraisal : ViewOwn` |
| `POST` | `.../appraisals/self-submit` | Kirim penilaian diri; membuat lembar bila belum ada | `PerformanceAppraisal : SelfAssess` |
| `GET` | `.../appraisals/subordinates?cycleId=` | Lembar bawahan langsung/lini kedua | `PerformanceAppraisal : ViewSubordinates` |
| `POST` | `.../appraisals/{id}/manager-assess` | Penilaian atasan langsung | `PerformanceAppraisal : ManagerAssess` |
| `POST` | `.../appraisals/{id}/second-line-review` | Telaah atasan kedua | `PerformanceAppraisal : SecondLineReview` |
| `POST` | `.../appraisals/{id}/acknowledge` | Tanggapan pegawai pemilik | `PerformanceAppraisal : Acknowledge` |

#### HR Payroll - Self-Service Payslips

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/payroll/runs/self-service/my-payslips` | Slip yang sudah dirilis milik pegawai login | `PayrollRunModern : GetMyPayslips` |
| `POST` | `.../self-service/verify-pin` | Verifikasi PIN 6 digit (`ddMMyy`) | `PayrollRunModern : VerifyPayslipPin` |
| `GET` | `.../self-service/payslips/{id}/detail` | Rincian (header `X-Payroll-PIN`) → `200`/`401` sesi PIN/`404` | `PayrollRunModern : GetPayslipDetail` |
| `GET` | `.../self-service/payslips/{id}/download-pdf` | PDF AES-256 bersandi DDMMYYYY → `200`/`401`/`404`/`409` | `PayrollRunModern : DownloadPayslipPdf` |

#### Corporate - HRD FPPE Incident & Focused Evaluation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/hrd/clinical-performance/fppe/case-logs/{logId}/verify` | Penilaian proctor yang ditunjuk → `200`/`400`/`403`/`404` | `FppeFocusedEvaluation : VerifyCaseLog` |

#### HR Deductions - Restitution Claims

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/hr-deductions/claims/mine` | **Baru.** Riwayat klaim sendiri | `RestitutionClaim : ViewOwn` |
| `GET` | `/api/hr-deductions/claims/mine/{id}` | **Baru.** Detail klaim sendiri | `RestitutionClaim : ViewOwn` |
| `GET` | `/api/hr-deductions/claims/quota?benefitCategory=&employeeId=` | `employeeId` kini opsional | `RestitutionClaim : ViewOwn` (+ `Read` untuk pegawai lain) |
| `POST` | `/api/hr-deductions/claims` | Klaim atas nama sendiri | `RestitutionClaim : Submit` |
| `POST` | `/api/hr-deductions/claims/{id}/approve-supervisor`, `review-hr`, `approve-final`, `reject` | Kini `403` bila memutuskan klaim sendiri | Masing-masing seperti sebelumnya |

#### HR Deductions - Settings

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/hr-deductions/settings/{id}` | **Baru.** Ubah satu kebijakan; tercatat di log audit | `RestitutionSetting : Manage` |

#### Corporate - HRD Business Travel Settlements

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `.../settlements/{id}/verify` | Hanya `Submitted`/`Verified` | `BusinessTravelSettlement : Verify` |
| `POST` | `.../settlements/{id}/close` | Hanya `Verified`; nomor bukti wajib bila ada selisih | `BusinessTravelSettlement : Close` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` (build 1–3) | 0 error; 605 → 604 warning | `PASS` | Keluaran perintah |
| Build 4 (setelah `MSL-09`) | 4 error `CS0535` di `HrdEmploymentContractService` — pekerjaan paralel pengguna (kontrak & masa kerja) yang belum selesai, bukan berkas laporan ini | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| Build 5 (setelah pekerjaan paralel tersambung) | 0 error, 12 warning (inkremental) | `PASS` | Keluaran perintah |
| QBE Strict `-Path` pada 16 berkas | Semua `PASS`, 0 temuan | `PASS` | Keluaran `Invoke-QbeConformanceCheck.ps1` |
| Pemeriksaan DB baca-saja | 34/58 akun punya `EmployeeId`; akun `mira.safitri` cocok `EmployeeId` dan `WorkforceProfileId`; `HrdPerformanceCycle` 0 baris; `MstOvertimePolicy` 0 baris | `PASS` | `admisi-1026-db.py` (SELECT saja) |
| Uji runtime dan Swagger | Belum dijalankan. Menyalakan backend menulis registri hak akses ke DB dev bersama saat start, sehingga butuh izin pemilik | `NOT RUN` | — |
| Automated test | `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)` | — | — |

---

## 6. Status migration dan database

Tidak ada migration dibuat atau dijalankan. Tidak ada DML/DDL. Penanda hak `WorkflowDelegation : ViewAllDelegations` baru masuk registri `SysActionAccess` saat backend dinyalakan (seeder reflektif yang sudah ada).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Endpoint penilaian kinerja kini butuh klaim `workforce_profile_id`; akun tanpa profil dijawab `403`. Layar lama yang masih mengirim `?workforceProfileId=` tidak lagi berpengaruh. Penghapusan `catch (Exception)` membuat galat teknis tercatat di log server dan pengguna menerima pesan umum |
| Asumsi | Sandi PDF DDMMYYYY; kategori potongan `KasbonPinjamanRS` untuk piutang manfaat; Plh wajib punya akun login |
| Dependency | `MSL-10` menunggu izin migration; uji runtime menunggu izin menyalakan backend |
| Task berikutnya | Uji live alur Plh, slip gaji, penilaian kinerja, restitusi; laporkan 7 worker non-SDM ke pemilik modulnya |

---

## 8. Putaran 2 — 18 November 2026 sore

**Wewenang.** Pemilik menjawab "oke ikuti semua rekomendasi, lanjut kerjakan". Artinya: (1) boleh menyalakan backend untuk uji live, dengan uji baca plus satu penulisan yang dipulihkan; (2) migration `MSL-10` boleh dibuat tetapi tidak dijalankan; (3) `MSL-14`–`MSL-27` dikerjakan dengan opsi A di semua keputusan, mulai dari `DEC-FER-05` dan `DEC-FER-02`; (4) galat build Menu 08 diperbaiki.

### 8.1 `MSL-10` — Pengaturan lembur di tabel

| Berkas | Isi |
|---|---|
| `Areas/Corporate/HumanResource/OvertimeManagement/Models/HrdOvertimeSetting.cs` (baru) | Satu baris aktif per rumah sakit (`HospitalCode`, bawaan `RSMMC`) berisi 8 parameter lembur |
| `Repositories/Configurations/Corporate/HumanResource/OvertimeManagement/HrdOvertimeSettingConfiguration.cs` (baru) | Konfigurasi tabel, nilai bawaan, indeks unik `HospitalCode` |
| `Migrations/20261118100000_AddHrdOvertimeSetting.cs` (baru, **belum dijalankan**) | SQL idempoten `IF NOT EXISTS`, constraint tanggal tutup buku 1–28 dan batas positif, baris awal dengan nilai bawaan lama |
| `OvertimeSettingService.cs` | Membaca/menyimpan tabel; setiap ubah dicatat `LoggerService.AuditAsync` dengan nilai sebelum dan sesudah. Bila tabel belum ada (SQLSTATE `42P01`), baca memakai nilai bawaan (`isPersisted: false`) dan simpan dijawab `409` dengan pesan meminta admin menjalankan pembaruan basis data |
| `OvertimeSettingController.cs` | `PUT` menjawab `400` untuk isian di luar batas dan `409` bila tabel belum ada |
| `Program.cs` | Lifetime `Singleton` → `Scoped` (kini memakai `ApplicationDbContext`) |

Batas isian (sama di server dan layar): batas harian 1–24 jam; batas mingguan 1–168 jam dan tidak lebih kecil dari batas harian; pembulatan 1–60 menit; durasi minimum 1–240 menit; CTO 1–365 hari; tanggal tutup buku 1–28; toleransi presensi 1–240 menit; tenggat klaim cito 1–14 hari.

### 8.2 `MSL-14` — Daftar lintas pegawai (`DEC-FER-05` opsi A)

Tiga layanan kueri baca-saja dan satu helper bersama (`Shared/HumanResource/Helpers/HrCrossEmployeeLookup.cs`: paging 1–100, pencarian ILIKE yang aman). Kolom klinis/rahasia tidak ikut dikirim.

#### Corporate - HRD Work Restriction & Injury BPJS JKK

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/api/v1/hrd/occupational-health/restrictions?search&status&activeOnly&pageNumber&pageSize` | Pembatasan kerja seluruh pegawai, tanpa dasar klinis | `WorkRestrictionAndInjury : Read` |
| `GET` | `/api/v1/hrd/occupational-health/injuries?search&status&pageNumber&pageSize` | Kecelakaan kerja seluruh pegawai, tanpa catatan klinis; batas lapor BPJS = kejadian + 48 jam | `WorkRestrictionAndInjury : Read` |

#### Corporate - HRD Occupational Health & Medical Privacy

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/api/v1/hrd/occupational-health/records?search&status&pageNumber&pageSize` | Ringkasan kelaikan kerja seluruh pegawai (tanpa `ClinicalBasisRestricted`) | `OccupationalHealthRecord : ViewSummary` |

#### Corporate - HRD Staff Vaccination & Immunity Tracking

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/api/v1/hrd/occupational-health/vaccinations?search&status&pageNumber&pageSize` | Vaksinasi seluruh pegawai | `StaffVaccination : Read` |

#### Corporate - HRD Disciplinary (Hubungan Industrial)

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/api/v1/hrd/disciplinary/sanctions?search&status&level&activeOnly&pageNumber&pageSize` | SP seluruh pegawai (tanpa uraian pelanggaran) | `EmployeeDisciplinary : ViewSanctions` |
| `GET` | `/api/v1/hrd/disciplinary/suspensions?search&status&activeOnly&pageNumber&pageSize` | Skorsing seluruh pegawai | `EmployeeDisciplinary : ManageSuspension` |

#### Corporate - HRD Performance Calibration & Dispute Resolution / PIP & Career

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/api/v1/corporate/hrd/performance/disputes?search&status&pageNumber&pageSize` | Sanggahan seluruh pegawai | `PerformanceCalibration : ManageDispute` |
| `GET` | `/api/v1/corporate/hrd/performance/clinical-ladder/applications?search&status&pageNumber&pageSize` | Pengajuan jenjang klinis seluruh pegawai | `PerformancePipAndCareer : ManageClinicalCareer` |

Contoh: Kepala K3RS membuka **Pembatasan Tugas Medis** dan langsung melihat 12 perawat yang masih dibatasi shift malam, tanpa memilih pegawai satu per satu.

### 8.3 `MSL-15` — Unggah lampiran (`DEC-FER-02` opsi A), tahap 1

- `Shared/HumanResource/Helpers/HrAttachmentStorage.cs` (baru). Berkas disimpan di `App_Data/human-resource-attachments` (di luar `wwwroot`, jadi tidak bisa dibuka langsung). Kunci berkasnya `{kategori}/{tahun}/{bulan}/{idAkunPengunggah}/{guid}{ekstensi}`. Format yang diterima PDF, JPG, PNG, WEBP, Word, Excel; maksimal 10 MB (bisa diatur lewat `HumanResource:Attachment:*`).
- Endpoint unggah/unduh ditempatkan di controller pemilik fitur dan memakai hak yang sudah ada, supaya admin tidak perlu memberi hak baru ke semua pegawai. Unduhan diizinkan untuk pengunggah atau pemegang hak baca/putus fitur itu; selain itu dijawab `404`.

| Fitur (`[Tags]`) | Unggah | Unduh |
|---|---|---|
| HR Deductions - Restitution Claims | `POST /api/hr-deductions/claims/attachments?category=restitution-receipt` atau `restitution-medical-resume` (`RestitutionClaim : Submit`) | `GET .../claims/attachments/download?key=` (`ViewOwn`; selain pengunggah butuh `Read`/`ApproveSupervisor`/`ReviewHr`/`ApproveFinal`) |
| Corporate - HRD Business Travel Settlements | `POST /api/v1/hrd/travel/settlements/attachments` (`BusinessTravelSettlement : Submit`) | `GET .../attachments/download?key=` (`Read`; selain pengunggah butuh `Verify`/`Close`) |
| Corporate - HRD Performance Appraisals & Multi-Level Review | `POST /api/v1/corporate/hrd/performance/appraisals/evidence` (`SelfAssess`) | `GET .../evidence/download?key=` (`ViewOwn`; selain pengunggah butuh `ManagerAssess`/`SecondLineReview`/`ViewSubordinates`) |
| Corporate - HRD Performance Calibration & Dispute Resolution | `POST /api/v1/corporate/hrd/performance/disputes/evidence` (`SubmitDispute`) | `GET .../disputes/evidence/download?key=` (`SubmitDispute`; selain pengunggah butuh `ManageDispute`) |

Belum dikerjakan di `MSL-15`: lampiran COB benefit, dokumen kandidat dan MCU rekrutmen, surat sakit (cuti sudah punya unggah sendiri), bukti koreksi presensi atas nama, logbook lembur, spreadsheet batch kontrak awal (butuh endpoint impor), dan nota override rekredensial (VR-OPP-19).

Temuan keamanan, tidak diubah karena di luar SDM: `WorkflowFileStorageService` menyimpan lampiran workflow di `wwwroot/uploads/human-resource/workflow`. Bila static files aktif, berkas itu bisa dibuka tanpa login oleh siapa saja yang tahu namanya.

### 8.4 Verifikasi putaran 2

| Pemeriksaan | Hasil | Klasifikasi |
|---|---|---|
| `dotnet build` ke folder sementara | Kode putaran ini lolos kompilasi. Build berakhir gagal karena 1 galat `CS1061` di `LifecycleManagement/Services/HrdEmploymentContractService.cs:369` (Menu 08, sedang dikerjakan pengguna) | `EXISTING / ENVIRONMENT ISSUE` |
| Build sebelumnya | Sekali `OutOfMemoryException` Roslyn saat memori mesin penuh; lulus setelah memori longgar | `ENVIRONMENT ISSUE` |
| QBE Strict `-Path` 20 berkas | Semua `PASS` | `PASS` |
| Migration `20261118100000` | Ditulis, **tidak dijalankan** | — |
| Uji live | Bagian pegawai non-admin lulus (lihat laporan FE). Bagian akun Mira (Plh, slip gaji, restitusi) tertahan karena DB dev `160.22.250.77` tidak bisa dijangkau (koneksi timeout) | `NOT RUN` |
| Automated test | `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)` | — |
