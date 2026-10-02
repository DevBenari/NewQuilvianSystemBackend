# BE-SEC-003 — Implementation Plan

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `SEC-BP-001` |
| Task ID | `BE-SEC-003` |
| Judul | Technical Permission Granularity Hardening — pilot Dokter Rawat Jalan |
| Klasifikasi | `HEAVY` |
| Mode | `IMPLEMENTATION PLANNING ONLY` — tanpa coding, migration, database write, commit, maupun push |
| Backend SHA | `4d6722d00be66b0a59382daac3e3f1b86dc778b5` (branch `AndryZain`, working tree bersih) |
| Baseline dampak | `evidence/03-be-sec-003-pre-implementation-impact.md` |
| Impact scan atas perubahan SHA | `4d6722d` hanya menyentuh 6 berkas dokumen blueprint. **Nol** perubahan pada `Areas/`, `Controllers/`, `Models/`, `Services/`, `Filters/`, `Attributes/`, `Migrations/`, `Seeders/`, `Repositories/`, `Program.cs`, dan `docs/engineering/`. Seluruh angka dampak pada `evidence/03` **tetap berlaku** |
| Tanggal | 2 September 2026 |

---

## A. Scope `BE-SEC-003`

### A.1 Yang dikerjakan

| # | Pekerjaan | Ukuran |
| ---: | --- | --- |
| 1 | Memecah identitas technical permission yang terlalu kasar pada pilot | 7 identitas → 28 identitas |
| 2 | Memberi identitas dan otorisasi OR pada dua endpoint audio antrean | 2 endpoint |
| 3 | Memperluas `SysAccessPolicy` agar kemampuan setiap Departemen × Posisi tidak berubah | 9 baris terdampak → 40 baris efektif |
| 4 | Memberikan `QueueVoice.PlayAudio` kepada delapan Departemen × Posisi | keputusan `O-1`, langkah terpisah |
| 5 | Memperbarui test yang terkunci dan menambah test baru | 3 berkas diubah, 2 berkas baru |

### A.2 Yang **tidak** dikerjakan

| Di luar scope | Alasan |
| --- | --- |
| Seluruh Health Services, seluruh controller | Pilot terbatas Dokter → Rawat Jalan |
| Entity Business Permission (`SecBusinessFeature`, `SecBusinessPermission`, …) | `BE-SEC-004` dan sesudahnya |
| Entity Access Profile | `BE-SEC-006` |
| Resolver dua sumber | `BE-SEC-007`, `BE-SEC-008` |
| `/api/access/me` | `BE-SEC-010` |
| Otorisasi frontend | `FE-SEC-001` … `FE-SEC-003` |
| `MedicalCertificate` — termasuk `MedicalCertificate.Update` yang memuat 7 endpoint | `BROKEN_DEPENDENCY`; task terpisah setelah route frontend diperbaiki |
| `PrescriptionTemplate.Create`, `Prescription.Update`, `Drug.Read`, `PatientIntegratedProgressNote.Update` | Kasar, tetapi tidak memblokir satu pun Business Permission pilot |
| Pembaruan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Task ini tidak membuat model persisted `Sec*`; registry adalah langkah pertama `BE-SEC-004` |
| Penyempitan hak siapa pun | Kecuali langkah 9 audio yang sudah diputuskan `O-1` |
| Perubahan frontend | Frontend tidak pernah membaca nama technical permission |

### A.3 Identitas yang dipecah

| # | Identitas lama | Endpoint | Identitas baru | Perlakuan registry |
| ---: | --- | ---: | ---: | --- |
| 1 | `DoctorQueue.Update` | 6 | 6 | **Ditutup** seeder |
| 2 | `DoctorConsultation.Update` | 4 | 4 | **Ditutup** seeder |
| 3 | `PatientProcedure.Update` | 5 | 5 | **Ditutup** seeder |
| 4 | `PatientProcedure.Create` | 2 | 2 | **Bertahan, menyempit** — lihat C.4 |
| 5 | `PatientVitalSign.Update` | 4 | 4 | **Ditutup** seeder |
| 6 | `PatientAssessment.Update` | 3 | 3 | **Ditutup** seeder |
| 7 | `PatientDiagnosis.Update` | 4 | 4 | **Ditutup** seeder |
| 8 | *(tanpa identitas)* audio antrean | 2 | 1 identitas baru | **Ditambahkan** |

---

## B. Berkas yang Diperkirakan Berubah

### B.1 Controller — hanya nama aksi pada atribut

| Berkas | Perubahan | Jumlah atribut |
| --- | --- | ---: |
| `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs` | `[AccessPermission]` pada 6 action | 6 |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | `[AccessPermission]` pada 4 action | 4 |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | `[AccessPermission]` pada 6 action (5 `Update` + `SelectProcedure`) | 6 |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` | `[AccessPermission]` pada 4 action | 4 |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | `[AccessPermission]` pada 3 action | 3 |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientDiagnosisController.cs` | `[AccessPermission]` pada 4 action | 4 |
| `Areas/HealthServices/RegistrationManagement/Controllers/QueueVoiceController.cs` | `[AccessAction]` pada 2 endpoint audio menjadi `PlayAudio`; `[QueueVoicePlayback]` ditambahkan | 4 |

**`[AccessAction]` menyertainya.** Setiap `[AccessPermission]` yang berganti nama aksi wajib disertai
`[AccessAction]` dengan nama tampil dan deskripsi yang sesuai, karena `PermissionRegistryDescriptor`
hanya mendaftarkan kunci ketika `[AccessAction]` ada pada method yang sama. `AccessType` **tidak**
berubah.

### B.2 Infrastruktur otorisasi — dua berkas baru

| Berkas | Isi |
| --- | --- |
| `Attributes/QueueVoicePlaybackAttribute.cs` | `TypeFilterAttribute` tanpa argumen, mengikuti pola `AccessPermissionAttribute` |
| `Filters/QueueVoicePlaybackFilter.cs` | `IAsyncAuthorizationFilter` yang melakukan OR: `IAuthorizationService.AuthorizeAsync(user, "QueueDisplayRuntimeRead")` **atau** `AccessPermissionService.HasAccessAsync(user, "QueueVoice", "PlayAudio")` |

### B.3 Migrasi data

| Berkas | Isi |
| --- | --- |
| `Services/Security/PermissionSplitExpansionService.cs` | **Baru.** Mode laporan dan mode tulis; idempoten; menghasilkan matriks parity per Departemen × Posisi |
| `Program.cs` | Registrasi service + satu langkah startup bersyarat setelah `AccessMenuSeeder`, mengikuti pola `Seeders:Run*` yang sudah ada |
| `appsettings.json`, `appsettings.Development.json` | Satu key baru `Seeders:RunPermissionSplitExpansion` (nama key saja; nilainya `false` secara default) |

> **Bukan EF migration.** Tidak ada perubahan skema. Menaruhnya di `Migrations/` akan membuat
> `has-pending-model-changes` dan riwayat migration menyesatkan.

### B.4 Test

| Berkas | Sifat |
| --- | --- |
| `QuilvianSystemBackend.Tests/Security/PermissionRegistryInvariantTests.cs` | Ubah — jumlah kunci registry |
| `QuilvianSystemBackend.Tests/Security/CanonicalSecurityContractTests.cs` | Ubah — isi himpunan 69 endpoint fallback (jumlah tetap 69) |
| `QuilvianSystemBackend.Tests/Security/StaleRegistryAuthorizationTests.cs` | Ubah — kasus identitas lama yang ditutup |
| `QuilvianSystemBackend.Tests/Security/PermissionSplitParityTests.cs` | **Baru** |
| `QuilvianSystemBackend.Tests/Security/QueueVoicePlaybackAuthorizationTests.cs` | **Baru** |

### B.5 Dokumentasi

| Berkas | Sifat |
| --- | --- |
| `docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-003.md` | **Baru** — setelah implementasi dan validasi |
| `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md` | Ubah — status dan bukti |

### B.6 Yang secara tegas TIDAK disentuh

`Services/Security/AccessPermissionService.cs` · `Services/Security/PermissionRegistryDescriptor.cs` ·
`Services/Security/PermissionRegistryValidator.cs` · `Seeders/AccessMenuSeeder.cs` ·
`Filters/AccessPermissionFilter.cs` · `Attributes/AccessPermissionAttribute.cs` · seluruh model ·
seluruh konfigurasi EF · `Migrations/` · `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` ·
seluruh repository frontend.

---

## C. Permission Split Matrix

Kolom **Existing SysAccessPolicy**, **Department**, **Position**, dan **User** diambil dari query
read-only database development pada `evidence/03`.

---

### C.1 `PatientProcedure.Update` → 5 identitas

| Field | Isi |
| --- | --- |
| **Old Resource** | `PatientProcedure` |
| **Old Action** | `Update` |
| `ActionAccessId` | `f1a98b1f-d94d-4d5a-8ace-13633315d2a4` |
| `ControllerAccessId` | `2ac5294d-d3cf-4d35-adb9-62d59a948001` |
| **Current behavior** | Satu izin membuka **5 endpoint** dengan makna bisnis yang jelas berbeda: menyunting, menghapus draft, **menyetujui**, **melaksanakan**, dan **membatalkan** tindakan |
| **Existing SysAccessPolicy** | 1 baris, efektif, tidak nonaktif/terhapus/dibatalkan |
| **Department impact** | Medis (`676f2aa7-8089-466b-b8a9-73adf5599626`) |
| **Position impact** | Dokter Umum (`cd1cd442-f971-a117-19c1-ae8809230138`) |
| **User impact** | 2 pengguna aktif |
| **Migration mapping** | 1 baris → **5 baris**. Old ditutup seeder; policy lama dinonaktifkan; 5 policy baru dibuat dengan `DepartmentId` dan `PositionId` identik |
| **Rollback** | Ciutkan 5 baris menjadi 1; aktifkan kembali policy lama; seeder mendaftarkan ulang `Update` saat source dibalikkan |

| Endpoint | Current | **New Resource** | **New Action** | Reason | Sensitivity |
| --- | --- | --- | --- | --- | --- |
| `PUT /patient-procedures/{id}` | `PatientProcedure.Update` | `PatientProcedure` | `Edit` | Penyuntingan biasa | Non-sensitif |
| `PATCH /patient-procedures/{id}/remove-draft` | `PatientProcedure.Update` | `PatientProcedure` | `RemoveDraft` | Menghapus pilihan dari konsultasi draft; inilah yang dipakai dokter rawat jalan | Non-sensitif |
| `PATCH /patient-procedures/{id}/approve` | `PatientProcedure.Update` | `PatientProcedure` | `Approve` | **Gerbang klinis.** `ExecuteProcedure` menolak bila `IsNeedApproval && !IsApproved` — persetujuan adalah syarat pelaksanaan, sehingga tidak mungkin dimaksudkan sebagai izin yang sama | **Sensitif** |
| `PATCH /patient-procedures/{id}/execute` | `PatientProcedure.Update` | `PatientProcedure` | `Execute` | Menetapkan `ProcedureStatus = Completed`, `IsExecuted`, `ExecutedByUserId`. Pelaksanaan tindakan medis | **Sensitif** |
| `PATCH /patient-procedures/{id}/cancel` | `PatientProcedure.Update` | `PatientProcedure` | `Cancel` | **Dampak finansial.** Ditolak bila `IsBillingGenerated`; menulis `CancelledByUserId` dan `CancelReason` | **Sensitif** |

---

### C.2 `DoctorConsultation.Update` → 4 identitas

| Field | Isi |
| --- | --- |
| **Old Resource** | `DoctorConsultation` |
| **Old Action** | `Update` |
| `ActionAccessId` | `977393b0-6c09-4e09-aa27-16ef941f1894` |
| `ControllerAccessId` | `20de6551-100f-4153-ae3e-46c067cd96a8` |
| **Current behavior** | Satu izin membuka 4 endpoint, termasuk menulis SOAP **dan** menyelesaikan konsultasi — dua kemampuan yang `D-ARCH-7` nyatakan berbeda |
| **Existing SysAccessPolicy** | 1 baris, efektif |
| **Department impact** | Medis |
| **Position impact** | Dokter Umum |
| **User impact** | 2 pengguna aktif |
| **Migration mapping** | 1 baris → **4 baris** |
| **Rollback** | Ciutkan 4 baris menjadi 1 |

| Endpoint | Current | **New Resource** | **New Action** | Reason | Sensitivity |
| --- | --- | --- | --- | --- | --- |
| `PUT /doctor-consultations/{id}` | `DoctorConsultation.Update` | `DoctorConsultation` | `Edit` | Penyuntingan header konsultasi | Non-sensitif |
| `PATCH /doctor-consultations/{id}/soap` | `DoctorConsultation.Update` | `DoctorConsultation` | `WriteSoap` | Penulisan klinis dengan autosave — pekerjaan harian dokter | **Sensitif** — isi rekam medis |
| `PATCH /doctor-consultations/{id}/complete` | `DoctorConsultation.Update` | `DoctorConsultation` | `Complete` | **Transisi workflow**, bukan CRUD update. Memvalidasi seluruh tab lalu menutup konsultasi dan memicu proses hilir | **Sensitif** |
| `PATCH /doctor-consultations/{id}/cancel` | `DoctorConsultation.Update` | `DoctorConsultation` | `Cancel` | Membatalkan konsultasi berjalan | **Sensitif** |

Bukti `D-ARCH-7` dari source: `UpdateSoap` (`DoctorConsultationController.cs:503`) dan
`CompleteConsultation` (`:596`) sama-sama `[AccessPermission("DoctorConsultation", "Update")]`.

---

### C.3 `DoctorQueue.Update` → 6 identitas

| Field | Isi |
| --- | --- |
| **Old Resource** | `DoctorQueue` |
| **Old Action** | `Update` |
| `ActionAccessId` | `aa971275-c84f-442a-86c3-115bdca58420` |
| `ControllerAccessId` | `116698d8-5bb2-4460-9397-8719a764beff` |
| **Current behavior** | Satu izin membuka 6 endpoint yang menyatukan alur antrean dengan lifecycle konsultasi. Akibatnya empat Business Permission pilot — panggil, alur antrean, mulai, dan selesai — **tidak dapat dibedakan** |
| **Existing SysAccessPolicy** | **3 baris**, seluruhnya efektif |
| **Department impact** | Medis (ketiganya) |
| **Position impact** | Dokter Umum; Dokter Spesialis (`527a6805-8073-dbd8-92cc-fd4ae0b5acd7`); Dokter IGD (`ae5bb7af-9e65-63ed-c22b-57212203e592`) |
| **User impact** | Dokter Umum 2; Dokter Spesialis 4; Dokter IGD 0. **Total 6 pengguna** |
| **Migration mapping** | 3 baris → **18 baris** (3 × 6), masing-masing mempertahankan pasangan Departemen × Posisi-nya |
| **Rollback** | Ciutkan 18 baris menjadi 3 |

| Endpoint | Current | **New Resource** | **New Action** | Reason | Sensitivity |
| --- | --- | --- | --- | --- | --- |
| `POST /doctor-queues/{id}/call` | `DoctorQueue.Update` | `DoctorQueue` | `Call` | Memanggil pasien; memicu audio antrean | Non-sensitif |
| `POST /doctor-queues/{id}/start-consultation` | `DoctorQueue.Update` | `DoctorQueue` | `StartConsultation` | Membuka ruang kerja konsultasi | Non-sensitif |
| `POST /doctor-queues/{id}/finish-consultation` | `DoctorQueue.Update` | `DoctorQueue` | `FinishConsultation` | Menutup episode pelayanan; memicu farmasi dan kasir | **Sensitif** |
| `POST /doctor-queues/{id}/skip` | `DoctorQueue.Update` | `DoctorQueue` | `Skip` | Melewati pasien yang tidak muncul | Non-sensitif |
| `POST /doctor-queues/{id}/no-show` | `DoctorQueue.Update` | `DoctorQueue` | `NoShow` | Menandai pasien tidak hadir | Non-sensitif |
| `POST /doctor-queues/{id}/requeue` | `DoctorQueue.Update` | `DoctorQueue` | `Requeue` | Mengembalikan pasien ke antrean | Non-sensitif |

---

### C.4 `PatientProcedure.Create` → 2 identitas — **KASUS KHUSUS**

> **Ini satu-satunya pemecahan yang identitas lamanya BERTAHAN, bukan ditutup.** Melewatkan
> perbedaan ini menyebabkan kehilangan hak yang senyap.

| Field | Isi |
| --- | --- |
| **Old Resource** | `PatientProcedure` |
| **Old Action** | `Create` |
| `ActionAccessId` | `bff2a276-a322-4c2b-965f-4cc0160b5391` |
| **Current behavior** | Satu izin membuka 2 endpoint: memilih tindakan ke draft (`POST /select`) dan membuat tindakan penuh (`POST /`) |
| **Existing SysAccessPolicy** | 1 baris, efektif |
| **Department impact** | Medis |
| **Position impact** | Dokter Umum |
| **User impact** | 2 pengguna aktif |
| **Perlakuan registry** | `POST /` **tetap** bernama `Create`, sehingga kunci `PatientProcedure.Create` masih dideklarasikan source dan **tidak ditutup** seeder. Yang terjadi adalah **penyempitan makna**: dari 2 endpoint menjadi 1 |
| **Migration mapping** | Policy lama **dipertahankan aktif** (kini hanya membuka `POST /`) **ditambah** 1 policy baru untuk `Select`. Total 1 → **2 baris** |
| **Bahaya bila diperlakukan seperti yang lain** | Bila skrip menonaktifkan policy `Create` lalu membuat `Select` + `Create` baru, hasilnya tetap benar. Tetapi bila skrip hanya mencari identitas yang **ditutup** seeder, `PatientProcedure.Create` tidak akan ditemukan, `Select` tidak pernah dibuat, dan **dokter kehilangan kemampuan memilih tindakan** — justru fitur yang dipakai setiap hari di layar |
| **Rollback** | Nonaktifkan policy `Select`; policy `Create` tidak pernah disentuh |

| Endpoint | Current | **New Resource** | **New Action** | Reason | Sensitivity |
| --- | --- | --- | --- | --- | --- |
| `POST /patient-procedures/select` | `PatientProcedure.Create` | `PatientProcedure` | `Select` | Memilih tindakan ke konsultasi draft. **Satu-satunya jalur yang dipakai frontend pilot** (`use-doctor-procedure.js` → `selectPatientProcedure`) | Non-sensitif |
| `POST /patient-procedures` | `PatientProcedure.Create` | `PatientProcedure` | `Create` *(tidak berubah)* | Pembuatan tindakan penuh; tidak dipakai frontend pilot | Non-sensitif |

---

### C.5 `PatientVitalSign.Update` → 4 identitas

| Field | Isi |
| --- | --- |
| **Old Resource / Action** | `PatientVitalSign` / `Update` · `ActionAccessId` `e1d265d2-33b3-42be-b0f3-4ed379bc987f` |
| **Current behavior** | Satu izin membuka 4 endpoint, termasuk **verifikasi catatan orang lain** |
| **Existing SysAccessPolicy** | 1 baris, efektif |
| **Department / Position / User** | Medis / Dokter Umum / 2 pengguna |
| **Migration mapping** | 1 baris → **4 baris** |
| **Rollback** | Ciutkan 4 menjadi 1 |

| Endpoint | Current | **New Action** | Reason | Sensitivity |
| --- | --- | --- | --- | --- |
| `PUT /patient-vital-signs/{id}` | `Update` | `Edit` | Mengubah catatan tanda vital | Non-sensitif |
| `PATCH /patient-vital-signs/{id}/verify` | `Update` | `Verify` | Kendali mutu atas catatan petugas lain | **Sensitif** |
| `PATCH /patient-vital-signs/{id}/notify-doctor` | `Update` | `NotifyDoctor` | Menandai dokter sudah diberi tahu | Non-sensitif |
| `PATCH /patient-vital-signs/{id}/cancel` | `Update` | `Cancel` | Membatalkan catatan tanda vital | **Sensitif** |

---

### C.6 `PatientAssessment.Update` → 3 identitas

| Field | Isi |
| --- | --- |
| **Old Resource / Action** | `PatientAssessment` / `Update` · `ActionAccessId` `db30defd-8830-483e-a7ad-ea015e6b1c44` |
| **Current behavior** | Satu izin membuka 3 endpoint, termasuk pembatalan dokumen pengkajian |
| **Existing SysAccessPolicy** | 1 baris, efektif |
| **Department / Position / User** | Medis / Dokter Umum / 2 pengguna |
| **Migration mapping** | 1 baris → **3 baris** |
| **Rollback** | Ciutkan 3 menjadi 1 |

| Endpoint | Current | **New Action** | Reason | Sensitivity |
| --- | --- | --- | --- | --- |
| `PUT /patient-assessments/{id}` | `Update` | `Edit` | Mengubah pengkajian | Non-sensitif |
| `PATCH /patient-assessments/{id}/complete` | `Update` | `Complete` | Menyelesaikan dokumen pengkajian tanpa mengubah status antrean | Non-sensitif |
| `PATCH /patient-assessments/{id}/cancel` | `Update` | `Cancel` | Membatalkan pengkajian | **Sensitif** |

---

### C.7 `PatientDiagnosis.Update` → 4 identitas

| Field | Isi |
| --- | --- |
| **Old Resource / Action** | `PatientDiagnosis` / `Update` · `ActionAccessId` `2664936c-345d-4cd0-8c4b-0aa6fadd5ee3` |
| **Current behavior** | Satu izin membuka 4 endpoint, termasuk `resolve` yang tidak ada di layar dokter rawat jalan |
| **Existing SysAccessPolicy** | 1 baris, efektif |
| **Department / Position / User** | Medis / Dokter Umum / 2 pengguna |
| **Migration mapping** | 1 baris → **4 baris** |
| **Rollback** | Ciutkan 4 menjadi 1 |

| Endpoint | Current | **New Action** | Reason | Sensitivity |
| --- | --- | --- | --- | --- |
| `PUT /patient-diagnoses/{id}` | `Update` | `Edit` | Mengubah diagnosis | Non-sensitif |
| `PATCH /patient-diagnoses/{id}/set-primary` | `Update` | `SetPrimary` | Menandai diagnosis utama; memengaruhi penagihan dan pelaporan | **Sensitif** |
| `PATCH /patient-diagnoses/{id}/resolve` | `Update` | `Resolve` | Pernyataan klinis bahwa diagnosis sudah teratasi | **Sensitif** |
| `PATCH /patient-diagnoses/{id}/cancel` | `Update` | `Cancel` | Membatalkan diagnosis | **Sensitif** |

---

### C.8 Audio antrean — penambahan identitas, bukan pemecahan

| Field | Isi |
| --- | --- |
| **Old Resource / Action** | *(tidak ada `[AccessPermission]`)*. Kedua endpoint terdaftar lewat jalur fallback sebagai `QueueVoice.Read` dari argumen pertama `[AccessAction]` |
| **Current behavior** | Hanya `[Authorize]`. **Setiap akun yang login** dapat mengunduh rekaman nama pasien bila mengetahui `dateKey` dan `fileName` |
| **New Resource / Action** | `QueueVoice` / `PlayAudio` |
| **Otorisasi** | `QueueVoice.PlayAudio` **ATAU** policy `QueueDisplayRuntimeRead` — **OR**, satu mekanisme |
| **Existing SysAccessPolicy** | **Nol** baris menunjuk kemampuan ini, karena identitasnya belum pernah ada |
| **Department / Position impact** | Delapan pasangan penerima menurut `O-1`: Medis × {Dokter Umum, Dokter Spesialis, Dokter IGD}; Keperawatan × {Perawat Rawat Jalan, Perawat Rawat Inap, Perawat IGD, Kepala Keperawatan, Kepala Ruangan} |
| **User impact** | **17 pengguna tetap** dapat memutar audio; **22 pengguna kehilangan** kemampuan itu |
| **Migration mapping** | 0 baris → **8 baris** policy baru. **Bukan** pelestarian; ini penyempitan yang diputuskan `O-1` |
| **Rollback** | Nonaktifkan 8 baris policy; setelah source dibalikkan endpoint kembali `[Authorize]` saja sehingga tidak ada yang kehilangan akses |

| Endpoint | Current | New | Reason | Sensitivity |
| --- | --- | --- | --- | --- |
| `GET /queue-voice/audio/{dateKey}/{fileName}` | `[Authorize]` saja | `QueueVoice.PlayAudio` OR `QueueDisplayRuntimeRead` | Berkas memuat nama pasien yang diumumkan | **Sensitif** |
| `GET /queue-voice/download/{dateKey}/{fileName}` | `[Authorize]` saja | sama | sama | **Sensitif** |

**Cara identitas terdaftar tanpa mengubah descriptor.** Argumen pertama `[AccessAction]` diubah dari
`"Read"` menjadi `"PlayAudio"`. Jalur fallback `PermissionRegistryDescriptor.BuildCore` — untuk
endpoint ber-`[AccessAction]` tanpa `[AccessPermission]` — mendaftarkannya sebagai
`QueueVoice.PlayAudio` memakai nama controller. `AccessType` tetap `Read`. **Tidak ada**
`[AccessPermission]` yang ditambahkan; justru itulah yang mencegah semantik AND.

### C.9 Rekapitulasi

| Identitas lama | Baris policy | × identitas baru | Baris sesudah | Perlakuan |
| --- | ---: | ---: | ---: | --- |
| `DoctorQueue.Update` | 3 | 6 | 18 | Ditutup + diganti |
| `DoctorConsultation.Update` | 1 | 4 | 4 | Ditutup + diganti |
| `PatientProcedure.Update` | 1 | 5 | 5 | Ditutup + diganti |
| `PatientVitalSign.Update` | 1 | 4 | 4 | Ditutup + diganti |
| `PatientAssessment.Update` | 1 | 3 | 3 | Ditutup + diganti |
| `PatientDiagnosis.Update` | 1 | 4 | 4 | Ditutup + diganti |
| **Subtotal ditutup** | **8** | | **38** | |
| `PatientProcedure.Create` | 1 | 2 | 2 | **Bertahan + ditambah 1** |
| **Subtotal pemecahan** | **9** | | **40** | |
| `QueueVoice.PlayAudio` | 0 | 1 | 8 | **Penambahan (`O-1`)** |
| **Total** | **9** | | **48** | |

---

## D. Strategi Migrasi

### D.1 Dua jenis perubahan yang wajib dipisahkan

| | **Pemecahan** (langkah 1–8) | **Penyempitan audio** (langkah 9) |
| --- | --- | --- |
| Sifat | Pelestarian hak | Pengurangan hak |
| Jaminan | `BEFORE = AFTER` per Departemen × Posisi | Tidak berlaku — memang menyempit |
| Baris policy | 9 → 40 | 0 → 8 |
| Verifikasi | Parity otomatis, selisih nol dua arah | Daftar penerima cocok dengan `O-1` |
| Kegagalan berarti | Migrasi berhenti | Daftar penerima salah |

Menggabungkan keduanya membuat verifikasi parity mustahil dibaca, karena setiap selisih menjadi
ambigu: bug atau kesengajaan.

### D.2 Aturan pemetaan

| Aturan | Isi |
| --- | --- |
| Sumber pemetaan | **Refleksi assembly** lewat `PermissionRegistryDescriptor`. Daftar endpoint per identitas lama diambil dari atribut, bukan dari penalaran bisnis |
| Dilarang | Memberi identitas baru yang endpoint-nya **tidak** dijaga identitas lama |
| Dilarang | Perluasan satu-ke-banyak berdasarkan tebakan, kemiripan nama, atau aturan bisnis |
| Wajib | `DepartmentId` dan `PositionId` disalin apa adanya; penyaring `WHERE` menyertakan keduanya |
| Wajib | Idempoten — dijalankan dua kali tidak menghasilkan duplikat |
| Wajib | Mode laporan tersedia dan dijalankan lebih dulu |

### D.3 Aritmetika yang diharapkan

| Ukuran | Sebelum | Sesudah | Selisih |
| --- | ---: | ---: | ---: |
| Baris `SysAccessPolicy` fisik | 498 | **537** | +39 dibuat, **0 dihapus** |
| Baris `SysAccessPolicy` efektif | 469 | **500** | +39 dibuat, −8 dinonaktifkan |
| Baris efektif setelah langkah 9 audio | 500 | **508** | +8 |
| Departemen × Posisi dengan izin efektif | 11 | **11** | **0** |
| `SysActionAccess` aktif | 1.076 | **1.097** | +28 baru, −7 ditutup |
| Endpoint terjangkau per Departemen × Posisi (langkah 1–8) | — | — | **0** |

> **Koreksi terhadap `evidence/03` bagian 10.4.** Dokumen itu menyebut baris `SysAccessPolicy` total
> `498 → 529 (+31)`. Angka itu mengasumsikan 9 baris lama **diganti**. Setelah audit `AccessMenuSeeder`,
> perlakuan yang benar adalah **soft**: 8 baris dinonaktifkan tanpa dihapus dan 39 baris baru dibuat,
> sehingga total fisik menjadi **537**. Angka **efektif** `469 → 500 (+31)` tetap benar dan tidak
> berubah. Jaminan parity tidak terpengaruh.

### D.4 Perlakuan policy lama

| Pilihan | Keputusan |
| --- | --- |
| Hard delete | **Tidak.** Melanggar pola lifecycle repository dan menghapus sejarah |
| Dibiarkan aktif | **Tidak.** Baris akan tampak aktif padahal menunjuk registry yang sudah ditutup — persis kebingungan yang `BE-SEC-001` selesaikan |
| **Dinonaktifkan dengan jejak audit** | **Ya.** `IsActive = false`, `UpdateDateTime`, `UpdateBy` diisi. Baris tetap ada dan dapat diaktifkan kembali saat rollback |

---

## E. Strategi Lifecycle `AccessMenuSeeder`

### E.1 Hasil audit — perilaku sebenarnya

Dibaca dari `Seeders/AccessMenuSeeder.cs` dan `Program.cs` pada SHA `4d6722d`.

| Fakta | Bukti |
| --- | --- |
| Seeder berjalan pada **setiap** kali aplikasi menyala, tanpa syarat | `Program.cs:976` — `RunStartupSeederAsync("AccessMenuSeeder", …)` |
| Kegagalan seeder **menghentikan** startup | `RunStartupSeederAsync` (`Program.cs:828-834`) tidak menangkap exception |
| Urutan dalam satu run | modul → `SaveChanges` → controller → `SaveChanges` → **action dibuat/diperbarui** → `SaveChanges` → **baris absen ditutup** → `SaveChanges` → normalisasi → `SaveChanges` |
| **Identitas baru dibuat SEBELUM identitas lama ditutup** | `ReconcileAsync`: blok upsert action selesai dan disimpan, baru `CloseRowsAbsentFromSourceAsync` dipanggil |
| Kunci pencocokan action | `BuildActionKey(ControllerAccessId, ActionName)` = `"{controllerId:N}:{actionName}"` |
| Kriteria penutupan | Setiap baris dengan `IsActive \|\| !IsDelete` yang kuncinya **tidak** ada di snapshot source → `IsActive=false`, `IsDelete=true`, `VisibleInRoleAccess=false` |
| Seeder **tidak pernah** menyentuh `SysAccessPolicy` | Komentar eksplisit pada `CloseRowsAbsentFromSourceAsync` + test `ReconcileNeverCreatesAccessPolicy` |
| Seeder **tidak** dibungkus satu transaksi | Lima `SaveChangesAsync` terpisah |
| Gerbang validator berjalan **setelah** seeder | `Program.cs:988-998`; melempar di luar Production |
| Seluruh langkah startup selesai **sebelum** `app.Run()` | Traffic belum diterima selama seeding |

### E.2 Di mana window kehilangan hak sebenarnya berada

Window **bukan** di dalam seeder, karena identitas baru sudah ada sebelum identitas lama ditutup.
Window ada di antara **selesainya seeder** dan **selesainya perluasan policy**:

```
t0  seeder membuat 28 identitas baru      → belum ada policy yang menunjuknya
t1  seeder menutup 7 identitas lama       → 8 policy menjadi inert
                                             ← DI SINI 6 pengguna kehilangan hak
t2  perluasan policy dijalankan            → parity pulih
t3  gerbang validator
t4  app.Run() — traffic mulai diterima
```

**Karena t0 sampai t3 seluruhnya terjadi sebelum aplikasi menerima request, window tersebut tidak
pernah terlihat pengguna — asalkan perluasan berjalan di dalam proses startup yang sama.**

### E.3 Dua jebakan yang harus dihindari

**Jebakan 1 — pre-seeding identitas baru sebelum deploy.** Terlihat menarik: buat 28 baris
`SysActionAccess` lebih dulu, lalu deploy. **Tidak aman.** Selama source lama masih berjalan,
identitas baru itu **absen dari snapshot source**, sehingga setiap restart — deploy ulang, restart
pod, autoscale — akan menutupnya lewat `CloseRowsAbsentFromSourceAsync`.

**Jebakan 2 — deployment dengan tumpang tindih instance.** Bila instance lama masih melayani traffic
saat instance baru menyala, seeder instance baru menutup identitas lama **di database yang sama**.
Instance lama seketika mulai menolak 6 pengguna dengan `403`, dan akan terus begitu sampai perluasan
selesai. Lebih buruk lagi: restart berikutnya pada instance lama akan menutup identitas **baru**.

> **Karena itu deployment wajib berpola hentikan-dulu-baru-nyalakan, bukan rolling update yang
> tumpang tindih.** Bila topologi produksi tidak memungkinkannya, itu keputusan operasional yang
> harus diangkat sebelum penerapan ke lingkungan selain development — lihat bagian I.

### E.4 Rancangan langkah perluasan

| Aspek | Keputusan |
| --- | --- |
| Tempat | Langkah startup **terpisah**, dipanggil setelah `AccessMenuSeeder` dan sebelum gerbang validator |
| Mengapa bukan di dalam seeder | `ReconcileNeverCreatesAccessPolicy` memanggil `AccessMenuSeeder.ReconcileAsync` langsung dan menuntut `SysAccessPolicies.Count == 0`. Menaruh perluasan di dalam seeder **akan mematahkan test itu** — dan test itu memang penjaga aturan "registry tidak pernah memberi hak" |
| Mengapa test tetap hijau | Perluasan berada di komponen lain; `ReconcileAsync` tetap tidak membuat policy |
| Gerbang | Config flag `Seeders:RunPermissionSplitExpansion`, default `false`. Mengikuti pola `Seeders:RunPrescriptionReviewCriterionSeed`, `RunEmergencyMasterDataSeed`, `RunInpatientMasterDataSeed`, `RunIcdSeed` yang sudah ada |
| Idempotensi | Melewati pasangan Departemen × Posisi yang sudah punya policy untuk identitas baru |
| Mode laporan | Wajib tersedia; menghasilkan matriks parity tanpa menulis |
| Kegagalan | Melempar, sehingga startup berhenti — lebih baik aplikasi tidak menyala daripada menyala dengan hak yang tidak lengkap |
| Sesudah berhasil | Flag dikembalikan ke `false` pada deployment berikutnya |

Bentuk konseptual pada `Program.cs`, disisipkan setelah baris 976:

```csharp
await RunStartupSeederAsync("AccessMenuSeeder", () => AccessMenuSeeder.SeedAsync(app.Services));

// BE-SEC-003 — perluasan policy setelah pemecahan identitas.
// Berjalan sekali, dijaga config flag, dan WAJIB berada di antara seeder dan gerbang validator
// supaya tidak ada window "identitas lama ditutup, identitas baru belum diberikan".
if (builder.Configuration.GetValue<bool>("Seeders:RunPermissionSplitExpansion"))
{
    await RunStartupSeederAsync(
        "PermissionSplitExpansion",
        () => PermissionSplitExpansionService.ExpandAsync(app.Services));
}

// Gerbang integritas permission (Phase A0) — tidak berubah.
```

---

## F. Deployment Order

### F.1 Prasyarat sebelum jendela penerapan

| # | Langkah | Menulis? | Gerbang |
| ---: | --- | --- | --- |
| 1 | Implementasi source selesai; `dotnet build` sukses | tidak | 0 error |
| 2 | Seluruh test lulus, termasuk 5 berkas test keamanan | tidak | 0 gagal |
| 3 | `dotnet ef migrations has-pending-model-changes` | tidak | Tidak ada perubahan tertunda |
| 4 | Snapshot `SysAccessPolicy`, `SysActionAccess`, `SysControllerAccess` | tidak | Jumlah baris dicatat: 498 / 1.076 aktif / 289 aktif |
| 5 | Perluasan dijalankan **mode laporan** terhadap database sasaran | **tidak** | Hasil harus sama persis dengan matriks bagian C dan `evidence/03` |
| 6 | Tinjauan manusia atas laporan langkah 5 | — | Enam syarat parity terpenuhi |

Bila langkah 5 menghasilkan angka yang berbeda dari matriks — misalnya `DoctorQueue.Update` ternyata
bukan 3 baris lagi — **berhenti**. Selisih berarti database sudah berubah sejak audit, dan
rencananya harus dihitung ulang, bukan dipaksakan.

### F.2 Jendela penerapan

| # | Langkah | Menulis? | Verifikasi |
| ---: | --- | --- | --- |
| 7 | Set `Seeders:RunPermissionSplitExpansion = true` | konfigurasi | — |
| 8 | **Hentikan seluruh instance lama** | — | Nol instance melayani traffic. Wajib, lihat E.3 jebakan 2 |
| 9 | Deploy dan nyalakan instance baru | — | — |
| 10 | `AccessMenuSeeder` berjalan: 28 identitas dibuat, 7 ditutup | **ya** | Log `[StartupSeed] AccessMenuSeeder completed` |
| 11 | `PermissionSplitExpansion` berjalan: 8 policy dinonaktifkan, 39 dibuat | **ya** | Log jumlah per identitas |
| 12 | Gerbang validator | tidak | Log `Permission registry valid. KeyCount=1097` |
| 13 | `app.Run()` — traffic diterima | — | **Window nol**: langkah 10–12 selesai sebelum request pertama |
| 14 | Verifikasi parity | tidak | Selisih nol dua arah, per Departemen × Posisi |
| 15 | Smoke test akun non-SuperAdmin | tidak | Bagian H.5 |

### F.3 Langkah penyempitan audio — terpisah dan tercatat

| # | Langkah | Menulis? | Verifikasi |
| ---: | --- | --- | --- |
| 16 | Berikan `QueueVoice.PlayAudio` kepada delapan pasangan `O-1` | **ya** | 8 baris policy baru |
| 17 | Verifikasi | tidak | 17 pengguna dapat memutar audio; 22 tidak; perangkat display tidak terpengaruh |

Dipisahkan dari langkah 11 dengan sengaja: ini satu-satunya langkah yang **mengubah** kemampuan,
sehingga verifikasi parity langkah 14 tetap dapat dibaca sebagai "harus nol".

### F.4 Setelah penerapan

| # | Langkah |
| ---: | --- |
| 18 | Set `Seeders:RunPermissionSplitExpansion = false` |
| 19 | Tulis laporan tracked `task/report/backend/BE-SEC-003.md` |
| 20 | Perbarui roadmap dan requirement traceability |

### F.5 Penerapan ke lingkungan lain

Development lebih dulu, satu lingkungan pada satu waktu. Untuk setiap lingkungan berikutnya, ulangi
**langkah 4 sampai 17** — termasuk mode laporan dan tinjauan manusia. Angka pada dokumen ini berasal
dari database development dan **tidak boleh** diasumsikan berlaku di lingkungan lain.

---

## G. Rollback Plan

### G.1 Batas rollback

`BE-SEC-003` mandiri. Tidak ada task lain yang bergantung padanya saat dijalankan, dan frontend
tidak terpengaruh sama sekali.

### G.2 Prosedur

| # | Langkah | Isi |
| ---: | --- | --- |
| 1 | Set `Seeders:RunPermissionSplitExpansion = false` | Mencegah perluasan berjalan lagi |
| 2 | Hentikan seluruh instance | Hindari tumpang tindih, sama seperti saat maju |
| 3 | Kembalikan source ke `4d6722d` | 9 berkas backend; cabut 2 berkas atribut/filter baru |
| 4 | Nyalakan aplikasi | Seeder mendaftarkan ulang 7 identitas lama dan menutup 28 identitas baru |
| 5 | Jalankan skrip balik | Aktifkan kembali 8 policy lama; nonaktifkan 39 policy baru; nonaktifkan 8 policy audio |
| 6 | Verifikasi | `SysAccessPolicy` efektif kembali **469**; pasangan Departemen × Posisi tetap **11**; endpoint terjangkau per pasangan sama seperti sebelum penerapan |
| 7 | Bila langkah 5 gagal | Pulihkan `SysAccessPolicy` dari snapshot langkah 4 pada F.1 |

### G.3 Sifat rollback per komponen

| Komponen | Rollback | Catatan |
| --- | --- | --- |
| Nama identitas pada controller | Kembalikan source | Seeder menyesuaikan registry otomatis |
| 28 identitas baru di registry | Ditutup otomatis seeder | Soft: `IsActive=false`, `IsDelete=true` |
| 7 identitas lama | Dibuka kembali otomatis seeder | Karena kembali dideklarasikan source |
| 39 policy baru | Skrip balik menonaktifkan | Tidak dihapus |
| 8 policy lama | Skrip balik mengaktifkan kembali | Baris aslinya tidak pernah dihapus |
| `PatientProcedure.Create` | **Tidak pernah disentuh** | Kasus khusus C.4; hanya `Select` yang dinonaktifkan |
| 8 policy audio | Skrip balik menonaktifkan | Endpoint kembali `[Authorize]` saja, tidak ada yang kehilangan akses |
| Atribut dan filter baru | Dicabut bersama source | Tidak ada jejak database |

### G.4 Titik tanpa jalan kembali

**Tidak ada.** Tidak ada kolom dihapus, tidak ada tipe berubah, tidak ada baris di-hard-delete, dan
tidak ada EF migration. Seluruh perubahan bersifat penandaan yang dapat dibalik.

### G.5 Risiko rollback yang harus disadari

| Risiko | Mitigasi |
| --- | --- |
| Rollback dilakukan setelah admin memberi hak baru lewat layar Akses Role | Skrip balik hanya menonaktifkan baris yang **dibuatnya sendiri**, dikenali dari `CreateBy` dan rentang `CreateDateTime`. Pemberian manual admin tidak disentuh, tetapi menjadi inert karena identitasnya ditutup — dilaporkan, tidak dihapus |
| Rollback sebagian: source dibalikkan tanpa menjalankan skrip balik | 39 policy baru menjadi inert dan 8 policy lama masih nonaktif → **6 pengguna kehilangan hak**. Langkah 5 wajib, bukan opsional |
| Snapshot tidak diambil | Langkah 4 pada F.1 adalah gerbang; tanpa snapshot penerapan tidak boleh dimulai |

---

## H. Test Plan

Delapan tuntutan pemilik sistem dipetakan ke test yang konkret.

### H.1 Pemetaan tuntutan ke test

| # | Tuntutan owner | Test | Berkas |
| ---: | --- | --- | --- |
| 1 | Pengguna existing tetap punya endpoint access yang sama setelah migrasi | `EffectiveEndpointSetIsIdenticalBeforeAndAfterPerDepartmentPosition` | `PermissionSplitParityTests` |
| 2 | `Approve` tidak memberikan `Execute` | `ApproveDoesNotGrantExecute` | `PermissionSplitParityTests` |
| 3 | `Execute` tidak memberikan `Approve` | `ExecuteDoesNotGrantApprove` | `PermissionSplitParityTests` |
| 4 | `Complete` tidak sama dengan SOAP write | `CompleteIsNotWriteSoap` | `PermissionSplitParityTests` |
| 5 | Permission lama setelah penutupan tidak mengizinkan akses | `RetiredIdentityNoLongerAuthorizes` | `StaleRegistryAuthorizationTests` |
| 6 | Registry permission baru valid | `EveryProtectedEndpointIsRegisterableInRoleAccess`, `AccessTypeStaysWithinFourColumns`, `NoDuplicateResourceAcrossModules` | `PermissionRegistryInvariantTests` |
| 7 | Tidak ada privilege broadening | `NoDepartmentPositionGainsEndpointAccess` | `PermissionSplitParityTests` |
| 8 | Tidak ada privilege loss yang tidak diharapkan | `NoDepartmentPositionLosesEndpointAccess` | `PermissionSplitParityTests` |

### H.2 Test parity — inti pembuktian

| Test | Yang dibuktikan |
| --- | --- |
| `EveryNewIdentityMapsToExactlyOneEndpoint` | 28 identitas ↔ 28 endpoint, satu-ke-satu |
| `EveryNewIdentityDerivesFromRetiredIdentityEndpointSet` | Setiap identitas baru berasal dari endpoint yang memang dijaga identitas lama. Syarat parity 1 dan 3 |
| `NoDepartmentPositionLosesEndpointAccess` | Selisih negatif nol |
| `NoDepartmentPositionGainsEndpointAccess` | Selisih positif nol, di luar langkah 9 audio |
| `DepartmentAndPositionAreNeverAlteredByExpansion` | Syarat parity 4 |
| `DistinctDepartmentPositionCountStaysEleven` | Syarat parity 5 |
| `ExpansionSourceIsReflectionNotHeuristics` | Syarat parity 6 |
| `ExpansionIsIdempotent` | Dijalankan dua kali tidak menghasilkan duplikat |
| **`PatientProcedureCreateRetainsPolicyAndGainsSelect`** | **Kasus khusus C.4.** Policy `Create` tetap aktif dan `Select` ditambahkan. Tanpa test ini, kehilangan hak memilih tindakan lolos tanpa terdeteksi |

### H.3 Test pemisahan kewenangan

| Test | Yang dibuktikan |
| --- | --- |
| `ApproveDoesNotGrantExecute` | Departemen × Posisi dengan `PatientProcedure.Approve` saja ditolak pada `PATCH /execute` |
| `ExecuteDoesNotGrantApprove` | Kebalikannya |
| `CancelIsSeparateFromEdit` | `PatientProcedure.Cancel` tidak diberikan oleh `Edit` |
| `CompleteIsNotWriteSoap` | Departemen × Posisi dengan `DoctorConsultation.WriteSoap` saja ditolak pada `PATCH /complete` |
| `WriteSoapIsNotComplete` | Kebalikannya |
| `FinishConsultationIsSeparateFromSkip` | `DoctorQueue.FinishConsultation` tidak diberikan oleh `Skip` |
| `VerifyIsSeparateFromEditOnVitalSign` | `PatientVitalSign.Verify` tidak diberikan oleh `Edit` |

### H.4 Test registry dan invarian `BE-SEC-001`

| Test | Yang dibuktikan |
| --- | --- |
| `RetiredIdentityNoLongerAuthorizes` | Policy yang menunjuk identitas tertutup menghasilkan `403` |
| `ReconcileNeverCreatesAccessPolicy` | **Tetap hijau** — perluasan berada di luar seeder |
| `AuthorizationIdentityAlwaysComesFromAccessPermission` | Kontrak kanonik tidak dilanggar |
| `CompatibilityFallbackMatchesApprovedLegacySetExactly` | Himpunan tetap **69**; isi diperbarui secara sadar untuk dua endpoint audio |
| `SeederIdentityMatchesRuntimeIdentity` | Kunci yang didaftarkan sama dengan yang dicari runtime |
| Tiga test SuperAdmin existing | Perilaku SuperAdmin tidak berubah |
| 12 test kontrak terkunci (`opr-permission-v1`, Billing) | Tidak terdampak |

### H.5 Test otorisasi OR audio

| Test | Yang dibuktikan |
| --- | --- |
| `AuthorizationIsOrNotAnd` | Memenuhi **salah satu** jalur sudah cukup — pembuktian eksplisit atas peringatan pemilik sistem |
| `DisplayDeviceCanPlayAudioWithoutTechnicalPermission` | Perangkat lolos lewat `QueueDisplayRuntimeRead` tanpa Departemen + Posisi |
| `StaffWithPlayAudioPermissionCanPlayAudio` | Dokter dan perawat lolos lewat `QueueVoice.PlayAudio` |
| `StaffWithoutPlayAudioPermissionIsDenied` | Manajer Finance ditolak `403` |
| `UnauthenticatedRequestIsRejected` | `401`, bukan `403` |
| `QueueVoicePlayAudioAppearsInRoleAccessRegistry` | Admin benar-benar dapat memberikannya |
| `DisplayRuntimePolicyIsNotGrantedAsUserPermission` | `QueueDisplayRuntimeRead` tidak menjadi permission dokter/perawat — larangan `O-1` |

### H.6 Verifikasi manual pada database development

Akun nyata non-SuperAdmin, mengikuti pola smoke test `BE-SEC-001`:

| # | Skenario | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | Dokter Umum memanggil, memulai, menyelesaikan konsultasi | Berhasil, sama seperti sebelum migrasi |
| 2 | Dokter Umum memilih tindakan lalu menghapusnya dari draft | Berhasil — pembuktian kasus khusus C.4 |
| 3 | Dokter Umum menyetujui dan melaksanakan tindakan | Berhasil — **parity, bukan hak baru** |
| 4 | Dokter Umum menulis SOAP dan menyelesaikan konsultasi | Keduanya berhasil |
| 5 | Dokter Spesialis memakai keenam aksi antrean | Berhasil |
| 6 | Perawat Rawat Jalan mencoba endpoint klinis dokter | Ditolak `403`, sama seperti sebelumnya |
| 7 | Manajer Finance mencoba endpoint klinis | Ditolak `403` |
| 8 | Setelah langkah 16: perawat memutar audio panggilan | Berhasil |
| 9 | Setelah langkah 16: Manajer Finance mengunduh audio | Ditolak `403` |
| 10 | Perangkat display antrean memutar audio | Berhasil, tanpa Departemen + Posisi |

### H.7 Batasan test yang harus dinyatakan jujur

| Batasan | Alasan |
| --- | --- |
| Provider `InMemory` tidak menegakkan unique index | Sama seperti `BE-SEC-001`; verifikasi index dilakukan langsung di database |
| Test parity berbasis data sintetis | Parity terhadap **data nyata** hanya dapat dibuktikan lewat mode laporan pada database sasaran, bukan lewat unit test |
| Semantik OR pada filter | Diuji lewat test integrasi filter, bukan lewat pemanggilan HTTP penuh, kecuali repository sudah punya pola test integrasi HTTP |

---

## I. Owner Decision yang Masih Tersisa

### I.1 Satu keputusan operasional

**P-1 · Topologi deployment untuk lingkungan selain development**

| Field | Isi |
| --- | --- |
| Mengapa muncul | Audit E.3 jebakan 2: bila instance lama masih melayani traffic saat instance baru menyala, seeder instance baru menutup identitas lama di database yang sama, dan instance lama seketika menolak 6 pengguna |
| Yang dibutuhkan | Konfirmasi bahwa penerapan memakai pola **hentikan-dulu-baru-nyalakan**, bukan rolling update yang tumpang tindih |
| Bukti keadaan sekarang | Tidak ditemukan setting replica pada konfigurasi deployment di repository pada SHA ini. Topologi produksi sebenarnya tidak dapat disimpulkan dari source |
| Menahan? | **Tidak menahan development.** Menahan penerapan ke lingkungan lain |
| Bila tidak memungkinkan | Window kehilangan hak selama beberapa detik untuk 6 pengguna. Perlu dijadwalkan pada jam sepi dan disetujui terpisah |

### I.2 Satu catatan yang tidak menahan

**Keperawatan × Bidan dan `QueueVoice.PlayAudio`.** Bidan (1 pengguna) dikecualikan dari delapan
pasangan penerima karena tidak memegang satu pun izin antrean. Bila pemilik sistem menganggapnya
actor pemanggil pasien, penambahannya satu baris pada langkah 16 dan tidak mengubah rancangan apa
pun.

### I.3 Wewenang yang sudah ada dan tidak perlu diminta lagi

| Butir | Status |
| --- | --- |
| Query read-only database development | `APPROVED` — sudah dijalankan |
| Migrasi data development | `CONDITIONALLY APPROVED` — enam syarat parity dibuktikan lewat langkah 5 dan 6 pada F.1 |
| Otorisasi audio antrean | `APPROVED` — `O-1` |
| Pemecahan `PatientProcedure` dan `DoctorConsultation` | `APPROVED` — `D-ARCH-6`, `D-ARCH-7` |
| Prefix `Sec` | `APPROVED`, tetapi **tidak dipakai** task ini |

### I.4 Yang tetap di luar scope dan tidak perlu diputuskan sekarang

Siapa boleh `approve`/`execute` tindakan (pemecahan mempertahankan hak apa adanya; penyempitan
adalah tindakan terpisah sesudahnya) · siapa menandatangani surat dokter · frontend authority ·
nama akhir entity `Sec*` · definisi pegawai aktif untuk Self Service.

---

## J. Database Impact Summary

Ringkasan terkonsolidasi seluruh dampak database `BE-SEC-003`. Sumber angka: query read-only
database **development**, 2 September 2026 (`evidence/03`).

### J.1 Perubahan skema

| Aspek | Nilai |
| --- | --- |
| Tabel baru | **Nol** |
| Kolom baru | **Nol** |
| Kolom dihapus atau berubah tipe | **Nol** |
| Index berubah | **Nol** |
| EF migration | **Nol** |
| `dotnet ef migrations has-pending-model-changes` | Harus tetap bersih |

> `BE-SEC-003` **tidak menyentuh skema sama sekali.** Yang berubah hanya isi tiga tabel yang sudah
> ada.

### J.2 Perubahan data per tabel

| Tabel | Sebelum | Sesudah | Operasi |
| --- | ---: | ---: | --- |
| `SysActionAccess` — baris aktif | 1.076 | **1.097** | 28 baris baru dibuat seeder; 7 baris lama ditutup (`IsActive=false`, `IsDelete=true`) |
| `SysActionAccess` — baris fisik | — | +28 | Tidak ada hard delete |
| `SysControllerAccess` — baris aktif | 289 | **289** | Tidak berubah; seluruh resource sudah ada |
| `SysApplicationModule` | — | — | Tidak berubah |
| `SysAccessPolicy` — baris fisik | 498 | **537** | 39 baris dibuat; **nol** dihapus |
| `SysAccessPolicy` — baris efektif (langkah 1–8) | 469 | **500** | 39 dibuat, 8 dinonaktifkan |
| `SysAccessPolicy` — baris efektif (setelah langkah 16 audio) | 500 | **508** | 8 baris `QueueVoice.PlayAudio` |
| `AspNetUserOrganization` | — | — | **Tidak disentuh** |
| Departemen × Posisi dengan izin efektif | 11 | **11** | **Tidak berubah** |

### J.3 Dampak pada pengguna

| Kelompok | Jumlah | Dampak |
| --- | ---: | --- |
| Pengguna aktif berbeda di seluruh sistem | 39 | — |
| Terdampak pemecahan identitas | **6** | Kemampuan **identik** sebelum dan sesudah |
| — Medis × Dokter Umum | 2 | 7 identitas lama → 28 identitas baru |
| — Medis × Dokter Spesialis | 4 | 1 identitas lama → 6 identitas baru |
| — Medis × Dokter IGD | 0 | 1 identitas lama → 6 identitas baru |
| Kehilangan kemampuan karena pemecahan | **0** | Dijamin parity |
| Memperoleh kemampuan baru karena pemecahan | **0** | Dijamin parity |
| Tetap dapat memutar audio antrean | **17** | Delapan pasangan penerima `O-1` |
| **Kehilangan** kemampuan memutar audio | **22** | Penyempitan yang **disengaja** menurut `O-1`, bukan bagian parity |

### J.4 Operasi tulis yang dijalankan

| # | Operasi | Pelaku | Kapan |
| ---: | --- | --- | --- |
| 1 | 28 baris `SysActionAccess` dibuat | `AccessMenuSeeder` | Startup, otomatis |
| 2 | 7 baris `SysActionAccess` ditutup | `AccessMenuSeeder` | Startup, otomatis |
| 3 | 8 baris `SysAccessPolicy` dinonaktifkan | `PermissionSplitExpansionService` | Startup, di balik config flag |
| 4 | 39 baris `SysAccessPolicy` dibuat | `PermissionSplitExpansionService` | Startup, di balik config flag |
| 5 | 8 baris `SysAccessPolicy` audio dibuat | Langkah terpisah tercatat | Setelah verifikasi parity |

Operasi 1 dan 2 sudah berjalan hari ini pada setiap startup — bukan hal baru. Operasi 3 sampai 5
adalah yang menuntut wewenang migrasi data.

### J.5 Lingkungan

| Lingkungan | Status |
| --- | --- |
| Development | Angka pada dokumen ini berasal dari sini; wewenang migrasi `CONDITIONALLY APPROVED` |
| Lingkungan lain | **Belum dihitung.** Angka development **tidak boleh** diasumsikan berlaku. Ulangi langkah 4–17 pada bagian F untuk setiap lingkungan |

---

## K. Known Limitations

Dicatat apa adanya supaya tidak ditemukan sebagai kejutan saat implementasi.

### K.1 Batasan cakupan yang disengaja

| # | Batasan | Alasan |
| ---: | --- | --- |
| 1 | Hanya pilot Dokter → Rawat Jalan | Instruksi scope. Identitas kasar di modul lain tetap ada |
| 2 | `MedicalCertificate.Update` — **7 endpoint** termasuk `issue`, `verify`, `approve`, `reject`, `revoke` — tidak dipecah | `BROKEN_DEPENDENCY`; menunggu perbaikan route frontend. **Ini identitas terkasar di seluruh audit dan tetap terbuka setelah `BE-SEC-003`** |
| 3 | `PrescriptionTemplate.Create` (3 endpoint), `Prescription.Update` (2), `Drug.Read` (6), `PatientIntegratedProgressNote.Update` (2) tidak dipecah | Tidak memblokir Business Permission pilot mana pun |
| 4 | Keluasan baca lintas pasien (`PatientVitalSign.Read` 7 endpoint termasuk `critical-alerts`) tidak disempitkan | Persoalan **data-scope**, bukan capability; lapisan terpisah menurut `D-ARCH-7` |
| 5 | Penyempitan hak — misalnya mencabut `Approve` dari dokter — tidak dilakukan | Pemecahan hanya melestarikan. Penyempitan adalah tindakan owner terpisah sesudahnya |

### K.2 Batasan teknis yang harus diterima

| # | Batasan | Akibat | Mitigasi |
| ---: | --- | --- | --- |
| 1 | `AccessMenuSeeder` **tidak** dibungkus satu transaksi — lima `SaveChangesAsync` terpisah | Crash di tengah seeding dapat meninggalkan registry setengah jadi | Startup gagal menghentikan aplikasi; jalankan ulang. Snapshot tersedia |
| 2 | Perluasan policy berjalan sebagai langkah startup | Aplikasi menulis data hak akses saat menyala | Dijaga config flag yang default `false`, idempoten, dan dimatikan kembali setelah penerapan |
| 3 | Provider `InMemory` tidak menegakkan unique index | Perilaku index tidak teruji lewat unit test | Sama seperti `BE-SEC-001`: verifikasi langsung di database |
| 4 | Parity terhadap **data nyata** tidak dapat dibuktikan unit test | Unit test hanya membuktikan logika | Mode laporan wajib dijalankan pada database sasaran sebelum menulis |
| 5 | Semantik OR diuji di tingkat filter, bukan HTTP penuh | Bergantung pola test yang ada di repository | Bila repository belum punya test integrasi HTTP, buktikan lewat test filter dan smoke test manual |
| 6 | Deployment tumpang tindih akan menimbulkan window `403` bagi 6 pengguna | Instance lama menolak begitu instance baru menutup identitas lama | Deployment wajib hentikan-dulu-baru-nyalakan; lihat `P-1` |
| 7 | Pre-seeding identitas baru sebelum deploy **tidak aman** | Restart pada source lama menutupnya kembali | Jangan dilakukan; perluasan hanya di startup yang sama |

### K.3 Batasan bukti

| # | Batasan | Catatan |
| ---: | --- | --- |
| 1 | Seluruh angka berasal dari database **development** | Lingkungan lain belum diukur |
| 2 | Topologi deployment produksi tidak dapat disimpulkan dari source | Tidak ditemukan setting replica pada SHA ini; menjadi `P-1` |
| 3 | Jumlah pengguna adalah keadaan **2 September 2026** | Penempatan organisasi berubah; hitung ulang saat penerapan |
| 4 | `evidence/03` bagian 10.4 memuat angka total baris policy yang sudah dikoreksi di sini | `498 → 537`, bukan `529`. Angka efektif `469 → 500` tetap benar |

### K.4 Yang tetap terbuka setelah `BE-SEC-003` selesai

| Butir | Menjadi tanggung jawab |
| --- | --- |
| Business Permission belum ada | `BE-SEC-004`, `BE-SEC-005` |
| Access Profile belum ada | `BE-SEC-006` |
| Frontend masih tanpa otorisasi | `FE-SEC-001` … `FE-SEC-003` |
| `MedicalCertificate` masih kasar dan route-nya masih salah | Task terpisah |
| Data-scope belum ditegakkan | Lapisan terpisah |
| 17 policy inert warisan `BE-SEC-001` | Masih fail closed |

---

## L. Rekomendasi

> ## `READY_FOR_IMPLEMENTATION`
>
> untuk lingkungan **development**.
>
> Penerapan ke lingkungan lain menunggu **P-1**.

### L.1 Dasar

| Kriteria | Status |
| --- | --- |
| Scope terkunci dan terbatas pada pilot | **Ya** — 7 controller, 7 identitas, tanpa pemecahan platform-wide |
| Baseline dampak masih berlaku pada SHA saat ini | **Ya** — impact scan membuktikan `4d6722d` hanya menyentuh dokumen |
| Setiap identitas baru berbukti dari semantik endpoint | **Ya** — bagian C |
| Seluruh kolom matriks terisi dengan data nyata | **Ya** — termasuk Departemen, Posisi, dan jumlah pengguna |
| Lifecycle seeder diaudit sampai urutan `SaveChanges` | **Ya** — bagian E.1 |
| Window kehilangan hak dapat dihilangkan | **Ya** — bagian E.2, perluasan di dalam startup yang sama |
| Dua jebakan lifecycle teridentifikasi | **Ya** — bagian E.3 |
| Kasus khusus `PatientProcedure.Create` teridentifikasi | **Ya** — bagian C.4, dengan test khusus |
| Deployment order lengkap | **Ya** — bagian F, 20 langkah |
| Rollback tanpa titik tanpa kembali | **Ya** — bagian G |
| Delapan tuntutan test terpetakan | **Ya** — bagian H.1 |
| Keputusan owner yang menahan development | **Nol** |

### L.2 Temuan terpenting dari sesi perencanaan ini

| # | Temuan | Akibat bila terlewat |
| ---: | --- | --- |
| 1 | **`PatientProcedure.Create` bertahan, tidak ditutup**, karena `POST /` tetap bernama `Create`. Yang terjadi penyempitan makna dari 2 endpoint menjadi 1 | Skrip yang hanya memproses identitas **tertutup** tidak akan pernah membuat `PatientProcedure.Select`. Dokter kehilangan kemampuan memilih tindakan — fitur yang dipakai setiap hari — tanpa satu pun alarm berbunyi |
| 2 | **Seeder membuat identitas baru sebelum menutup yang lama** dalam satu run | Menentukan bahwa window dapat dihilangkan sepenuhnya dengan menaruh perluasan di startup yang sama |
| 3 | **Pre-seeding sebelum deploy tidak aman** | Setiap restart pada source lama akan menutup identitas yang sudah disiapkan |
| 4 | **Deployment tumpang tindih tidak aman** | Instance lama menolak 6 pengguna begitu instance baru menyala |
| 5 | Perluasan **tidak boleh** berada di dalam `AccessMenuSeeder` | Akan mematahkan `ReconcileNeverCreatesAccessPolicy`, penjaga aturan "registry tidak pernah memberi hak" |
| 6 | Total baris policy fisik `498 → 537`, bukan `529` | Koreksi terhadap `evidence/03` bagian 10.4; angka efektif `469 → 500` tetap benar |

### L.3 Langkah berikutnya

1. Konfirmasi **P-1** bila penerapan tidak berhenti di development.
2. Beri wewenang eksekusi `BE-SEC-003` lewat `quilvian-engineering-skills:build-module-backend`.
3. Implementasi mengikuti bagian B, C, dan E; penerapan mengikuti bagian F.

---

## Pernyataan Penutup

| Batasan | Status |
| --- | --- |
| Perubahan controller | **Tidak ada** |
| Perubahan attribute atau filter | **Tidak ada** |
| Entity baru | **Tidak ada** |
| Migration dibuat | **Tidak ada** |
| Penulisan database | **Tidak ada** |
| Migrasi policy | **Tidak ada** |
| `git commit` / `git push` | **Tidak ada** |
| Working tree frontend | Bersih, tidak disentuh |

`BE-SEC-003` berstatus `READY_FOR_IMPLEMENTATION` untuk development, menunggu wewenang eksekusi.
