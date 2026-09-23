# BE-SEC-003 — Current HEAD Revalidation

> **Mode:** revalidasi baca-saja. Tidak ada perubahan source, tidak ada migration, tidak ada
> penulisan database, tidak ada commit, tidak ada push. Satu-satunya berkas yang ditulis adalah
> laporan ini.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Revalidation evidence** — bukan laporan task |
| Task yang direvalidasi | `BE-SEC-003` — Technical Permission Granularity Hardening |
| Baseline yang dibandingkan | `evidence/03`, `evidence/04`, `roadmap/backend-roadmap.md` (2 September 2026) |
| Tanggal revalidasi | 11 September 2026 |
| Rekomendasi | **`PLANNING_UPDATE_REQUIRED`** — dengan dua `OWNER_DECISION_REQUIRED` di dalamnya |

---

## A. Current HEAD identity

| Repository | Branch | HEAD | Commit | Working tree |
|---|---|---|---|---|
| `NewQuilvianSystemBackendAndryZain` | `AndryZain` | `8131913380063aee7f2ebe2bda54e888d88b2b02` | `Fix EF migration discovery for payment method migration` | **clean** |
| `QuilvianSystemFrontendDev` | `AgentCodexFrontend` | `2b9e3b074f8a3839857e123515353dd2f3233ac3` | `update attendance self service` | **clean** |

HEAD backend **cocok persis** dengan yang diharapkan pemilik sistem pada instruksi revalidasi.

**Frontend tidak bergerak sama sekali.** SHA frontend hari ini identik dengan SHA yang dicatat
`blueprint-manifest.md` sebagai *Frontend SHA saat audit `BE-SEC-002`*. Seluruh perubahan yang
dibahas laporan ini murni backend.

---

## B. Previous BE-SEC-003 baseline

| Field | Nilai |
|---|---|
| Backend SHA saat audit `BE-SEC-002` | `e1d112142510baa86ccd89977bc7189c89ed012b` |
| Status task pada roadmap | `READY FOR IMPLEMENTATION` — planning selesai, keputusan owner tertutup |
| Rancangan | 7 identitas kasar dipecah menjadi 28 identitas |
| Database yang direncanakan | `SysActionAccess` aktif 1.076 → 1.097 (28 baru, 7 ditutup); `SysAccessPolicy` fisik 498 → 537 (39 dibuat, nol dihapus); efektif 469 → 500, lalu 508 setelah langkah audio |
| Skema | Tidak ada perubahan skema, tidak ada EF migration |

Jarak dari baseline ke HEAD: **325 commit**.

---

## C. Changes since previous planning yang relevan dengan authorization

Empat perubahan menyentuh jalur otorisasi. Satu di antaranya mengubah status `BE-SEC-003` secara
mendasar.

### C.1 Temuan utama — pemecahan identitas **sudah dikerjakan** sebagai `BE-SEC-003A`

Commit `85fcc3fd` — *"WIP: save AndryZain local progress before integration sync"*, devbenari,
**4 September 2026** — berisi pemecahan technical permission itu sendiri:

```
 .../Controllers/DoctorConsultationController.cs    |   8 +-
 .../Controllers/PatientAssessmentController.cs     |   8 +-
 .../Controllers/PatientDiagnosisController.cs      |  12 +-
 .../Controllers/PatientProcedureController.cs      |  24 +-
 .../Controllers/PatientVitalSignController.cs      |  12 +-
 .../Controllers/DoctorQueueController.cs           |  24 +-
 .../Security/PermissionSplitPreparationTests.cs    | 429 +++++++++++++++++++++
```

Ini **bukan** pekerjaan tim lain yang kebetulan menabrak `BE-SEC-003`. Ini pekerjaan `BE-SEC-003`
sendiri, dikerjakan dua hari setelah planning ditulis, lalu masuk ke aliran integrasi lewat commit
WIP. Berkas test yang menyertainya menamai dirinya secara eksplisit:

> *Invarian pemecahan identitas technical permission — `BE-SEC-003A`.*
> *Fase A ini **hanya menyiapkan identitas dan pemetaan endpoint**. Ia sengaja **tidak**
> memindahkan satu pun hak: perluasan `SysAccessPolicy` adalah fase terpisah yang berjalan di
> luar seeder.*
> — `Tests/QuilvianSystemBackend.UnitTests.InMemory/Security/PermissionSplitPreparationTests.cs`

Artinya `BE-SEC-003` **sudah terpecah menjadi dua fase di source**, sementara roadmap masih
memperlakukannya sebagai satu task tunggal:

| Fase | Isi | Status di HEAD |
|---|---|---|
| **Fase A** | Pemecahan identitas di source + test invarian | **SELESAI** — ada di HEAD |
| **Fase B** | Perluasan `SysAccessPolicy` ke *exact historical capability set* | **BELUM DIKERJAKAN** |
| **Fase C** | Penyempitan audio antrean (`QueueVoice.PlayAudio`, otorisasi OR) | **BELUM DIKERJAKAN** |

### C.2 Saklar mematikan otorisasi untuk pengembangan — **baru**

`Services/Security/AccessPermissionService.cs` menerima konstruktor baru dan sebuah saklar:

```csharp
_authorizationDisabled =
    !configuration.GetValue("Security:Authorization:Enabled", true) &&
    !environment.IsProduction();
```

`HasAccessAsync` mengembalikan `true` tanpa syarat bila saklar aktif. Autentikasi tetap wajib.

Penilaian: **aman untuk produksi, berbahaya untuk pembuktian.** Saklar diabaikan di produksi,
default-nya `true` di `appsettings.json`, dan `appsettings.Development.json` tidak menimpanya —
jadi tidak ada regresi A0 di sini. Tetapi setiap verifikasi manual `BE-SEC-003` yang dijalankan di
lingkungan dengan `Security:Authorization:Enabled=false` akan **lulus secara palsu**: semua endpoint
mengizinkan semua orang. Lihat bagian T.

### C.3 `GetEffectivePermissionsAsync` — jalur otorisasi kedua, **baru**

Metode baru beserta endpoint `AuthController.cs:597` menerbitkan daftar pasangan
`resource : action` yang boleh dipakai pengguna, untuk dipakai frontend menyembunyikan tombol.
Dokumentasinya sendiri menyatakan kontrak yang mengikat:

> *"**Jawabannya wajib sama persis dengan `HasAccessAsync`.** Keduanya membaca tabel yang sama
> dengan penyaring yang sama; kalau salah satu berubah, yang lain wajib ikut."*

**Kontrak itu sudah dilanggar sejak lahir.** Lihat bagian D.6.

### C.4 `PatientAssessment.Amend` — identitas baru **di luar** himpunan Fase A

Endpoint `POST /patient-assessments/{id}/addendums` dengan
`[AccessPermission("PatientAssessment", "Amend")]` **tidak ada** pada commit Fase A `85fcc3fd`
(terverifikasi: nol kemunculan string `Amend` di berkas tersebut) dan **tidak ada** pada
`evidence/04`. Ia ditambahkan tim lain sesudahnya.

---

## D. A0 integrity verification

Enam invarian yang diminta, diperiksa satu per satu terhadap source HEAD.

| # | Invarian A0 | Status | Bukti |
|---|---|---|---|
| 1 | `[AccessPermission(resource, action)]` sebagai canonical technical permission identity | **UTUH** | `Attributes/AccessPermissionAttribute.cs` tidak berubah; `PermissionRegistryDescriptor` tetap menurunkan identitas dari argumen `[AccessPermission]`; test `AuthorizationIdentityAlwaysComesFromAccessPermission` masih ada |
| 2 | `AccessMenuSeeder` tidak membuat `SysAccessPolicy` | **UTUH** | Nol operasi tulis ke `SysAccessPolicies` di `Seeders/AccessMenuSeeder.cs`; dua kemunculan string hanyalah komentar. Seeder hanya mengelola `SysApplicationModule`, `SysControllerAccess`, `SysActionAccess` |
| 3 | Effective authorization memakai valid organization assignment | **UTUH** | `HasAccessAsync` join `ApplicationUserOrganizations` × `SysAccessPolicies` pada `DepartmentId` + `PositionId`, dengan jendela `EffectiveStartDate`/`EffectiveEndDate` |
| 4 | `IsPrimary` bukan authorization eligibility filter | **UTUH** | Tidak dipakai di `HasAccessAsync`; komentar di `AccessPermissionService.cs:156` menyatakan pengecualiannya disengaja. `OrganizationAuthorizationProjectionService.IsAssignmentValid` juga tidak memakainya |
| 5 | `AssignmentType` bukan authorization eligibility filter | **UTUH** | Tidak muncul di predikat kelayakan mana pun; hanya dipakai sebagai teks deskripsi proyeksi (`:305`) |
| 6 | Stale/deleted/cancelled assignment tidak memberikan access | **UTUH pada jalur penjaga; BOCOR pada jalur daftar** | Lihat D.6 |

**Tidak ada regression A0 pada jalur penegakan.** Tidak ada satu pun dari enam invarian yang rusak
karena merge tim lain. Kondisi STOP yang diminta instruksi **tidak terpenuhi**, sehingga revalidasi
dilanjutkan.

### D.6 Divergensi `IsCancel` pada `GetEffectivePermissionsAsync`

Predikat kelayakan penempatan berbeda antara dua jalur yang dokumentasinya mewajibkan identik:

| Penyaring | `HasAccessAsync` (penjaga) | `GetEffectivePermissionsAsync` (daftar) |
|---|:---:|:---:|
| `IsActive` | ✅ | ✅ |
| `!IsDelete` | ✅ | ✅ |
| jendela `EffectiveStartDate` / `EffectiveEndDate` | ✅ | ✅ |
| **`!IsCancel`** | ✅ | ❌ **hilang** |

Akibatnya, pengguna yang penempatannya **dibatalkan** akan melihat kemampuan itu tercantum pada
daftar kewenangan efektifnya, lalu ditolak `403` saat menekan tombolnya. Ini persis kegagalan yang
dokumentasi metode tersebut janjikan tidak akan terjadi.

**Klasifikasi:** bukan privilege escalation — penjaga tetap menolak, dan tidak ada akses nyata yang
diberikan. Ini **cacat integritas kontrak** pada permukaan baru.

**Cakupan test:** nol. Tidak ada satu pun kemunculan `IsCancel` di
`AccessPermissionEnforcementTests.cs`. Test parity yang ada hanya membandingkan tiga pasangan pada
penempatan yang sehat, sehingga divergensi ini tidak mungkin tertangkap.

**Kaitan dengan `BE-SEC-003`:** langsung dan wajib. Fase B memperbanyak identitas dari 7 menjadi 22+.
Setiap identitas baru akan dibaca **kedua** jalur. Memperbaiki `HasAccessAsync` saja sementara
`GetEffectivePermissionsAsync` menyimpang akan melipatgandakan cacat ini sebanyak identitas baru.

---

## E. Current endpoint inventory

Diambil langsung dari atribut pada source HEAD, bukan dari diff.

| Controller | Route dasar (`api/v1/…`) | Endpoint | Endpoint baru sejak baseline |
|---|---|---:|---:|
| `PatientProcedureController` | `health-services/clinical-management/patient-procedures` | 13 | 1 |
| `DoctorConsultationController` | `health-services/clinical-management/doctor-consultations` | 11 | 1 |
| `PatientAssessmentController` | `health-services/clinical-management/patient-assessments` | 14 | 6 |
| `PatientDiagnosisController` | `health-services/clinical-management/patient-diagnoses` | 10 | 0 |
| `PatientVitalSignController` | `health-services/clinical-management/patient-vital-signs` | 13 | 0 |
| `DoctorQueueController` | `health-services/registration-management/doctor-queues` | 10 | 0 |
| `QueueVoiceController` | `health-services/registration-management/queue-voice` | 5 | 0 |

Endpoint baru sejak baseline:

| Endpoint | Permission | Catatan |
|---|---|---|
| `GET /patient-procedures/episodes/{episodeId}` | `PatientProcedure.Read` | Tindakan satu perawatan rawat inap |
| `GET /doctor-consultations/episodes/{episodeId}/soap-timeline` | `DoctorConsultation.Read` | — |
| `GET /patient-assessments/episodes/{episodeId}` | `PatientAssessment.Read` | — |
| `POST /patient-assessments/{id}/addendums` | **`PatientAssessment.Amend`** | **Identitas baru** — lihat C.4 |
| `GET /patient-assessments/{id}/addendums` | `PatientAssessment.Read` | — |
| `GET /patient-assessments/episodes/{episodeId}/timeline` | `PatientAssessment.Read` | — |
| `GET /patient-assessments/episodes/{episodeId}/due-status` | `PatientAssessment.Read` | — |
| `GET /patient-assessments/monitoring/initial-assessment-compliance` | `PatientAssessment.Read` | — |

Seluruh endpoint baru kecuali satu memakai identitas `Read` yang sudah ada, sehingga tidak menambah
beban migrasi. Satu-satunya pengecualian adalah `Amend`.

**Integritas descriptor:** pemeriksaan otomatis atas ketujuh controller menemukan **nol
ketidakcocokan** antara argumen kedua `[AccessPermission]` dan argumen pertama `[AccessAction]` yang
menyertainya. Setiap identitas yang ditegakkan runtime benar-benar terdaftar ke registry, sehingga
tidak ada endpoint yang gagal-tertutup karena kunci registry yang hilang.

---

## F. Current technical permission inventory

Himpunan ini dikunci oleh `PermissionSplitPreparationTests` dan terverifikasi cocok dengan source.

### F.1 22 identitas hasil pemecahan — **sudah ada di HEAD**

| Resource | Action |
|---|---|
| `PatientProcedure` | `Select`, `Edit`, `Approve`, `Execute`, `RemoveDraft`, `Cancel` |
| `DoctorConsultation` | `Complete`, `Cancel` |
| `DoctorQueue` | `Call`, `StartConsultation`, `FinishConsultation`, `Skip`, `NoShow`, `Requeue` |
| `PatientVitalSign` | `Verify`, `NotifyDoctor`, `Cancel` |
| `PatientAssessment` | `Complete`, `Cancel` |
| `PatientDiagnosis` | `SetPrimary`, `Resolve`, `Cancel` |

### F.2 5 identitas lama yang **bertahan**

`DoctorConsultation.Update`, `PatientVitalSign.Update`, `PatientAssessment.Update`,
`PatientDiagnosis.Update`, `PatientProcedure.Create`.

Keputusan desain Fase A yang penting dan berbeda dari `evidence/04`: alih-alih mengganti nama
`Update` menjadi `Edit` di semua controller, Fase A **membiarkan `Update` menjaga endpoint `PUT`**.
Konsekuensinya baris `SysAccessPolicy` yang menunjuk kelima identitas ini **tidak pernah menjadi
yatim**, dan kehilangan hak yang harus dipulihkan Fase B menyusut drastis.

### F.3 2 identitas lama yang **pensiun**

`PatientProcedure.Update` dan `DoctorQueue.Update`. Hanya kedua inilah yang berhenti dideklarasikan
source, sehingga hanya kedua inilah yang akan ditutup `AccessMenuSeeder`.

### F.4 Identitas di luar himpunan terkunci

`PatientAssessment.Amend` — terdaftar, ditegakkan, **belum pernah diberikan kepada siapa pun**.

---

## G. Current split matrix

Klasifikasi setiap kandidat menurut kategori yang diminta instruksi.

| Kandidat | Kategori | Keterangan |
|---|---|---|
| `PatientProcedure` | **F** — sudah dipecah | 6 identitas baru; `Update` pensiun, diganti `Edit`; `Create` menyempit maknanya. Ditambah **B** (satu endpoint `Read` baru) |
| `DoctorConsultation` | **F** sebagian | `Complete` dan `Cancel` dipecah. `WriteSoap` **tidak** dipecah — menyimpang dari `D-ARCH-7`. Ditambah **B** (satu endpoint `Read` baru) |
| `DoctorQueue` | **F** — sudah dipecah | Keenam identitas dipecah penuh; `Update` pensiun. Tidak ada endpoint baru |
| `PatientAssessment` | **F** + **B** + **G** | `Complete`, `Cancel` dipecah. Enam endpoint baru, salah satunya membawa identitas baru `Amend` yang belum diberikan |
| `PatientDiagnosis` | **F** — sudah dipecah | `SetPrimary`, `Resolve`, `Cancel` dipecah. Tidak ada endpoint baru |
| `PatientVitalSign` | **F** — sudah dipecah | `Verify`, `NotifyDoctor`, `Cancel` dipecah. Tidak ada endpoint baru |
| Audio antrean | **A** — *unchanged* | `QueueVoiceController` identik dengan baseline. Belum dikerjakan |

**Tidak ada kategori C** (endpoint dihapus) dan **tidak ada kategori E** (semantik endpoint berubah)
pada keenam controller. Tidak ada pemecahan tambahan yang diusulkan hanya karena nama endpoint
terlihat berbeda.

### G.1 Kandidat coarse di luar scope — `NurseStationQueue`

`NurseStationQueueController` sudah ada sejak baseline dan **tetap kasar**: lima endpoint transisi
antrean (`call`, `start-screening`, `finish-screening`, `skip`, `no-show`) berbagi satu
`NurseStationQueue.Update`. Polanya identik dengan `DoctorQueue` sebelum dipecah.

Ini **kategori G** terhadap scope keseluruhan, tetapi **bukan** temuan baru dan **bukan** bagian
pilot Dokter Rawat Jalan. Dicatat di sini agar tidak hilang; **tidak** diusulkan masuk
`BE-SEC-003`. Memperluas scope pilot sekarang justru mengaburkan pembuktian parity-nya.

---

## H. Current PatientProcedure matrix

| Endpoint | Baseline `e1d1121` | **Current HEAD** | Status vs `evidence/04` |
|---|---|---|---|
| `GET` (6 varian baca) | `Read` | `Read` | Tidak berubah |
| `POST /select` | `Create` | **`Select`** | ✅ sesuai rencana |
| `POST /` | `Create` | `Create` | ✅ bertahan, sesuai `C.4` |
| `PUT /{id}` | `Update` | **`Edit`** | ✅ sesuai rencana |
| `PATCH /{id}/approve` | `Update` | **`Approve`** | ✅ sesuai rencana |
| `PATCH /{id}/execute` | `Update` | **`Execute`** | ✅ sesuai rencana |
| `PATCH /{id}/remove-draft` | `Update` | **`RemoveDraft`** | ✅ sesuai rencana |
| `PATCH /{id}/cancel` | `Update` | **`Cancel`** | ✅ sesuai rencana |

**Ketujuh aksi yang diminta revalidasi — `Select`, `Create`, `Edit`, `Approve`, `Execute`,
`RemoveDraft`, `Cancel` — seluruhnya sudah terpisah di source HEAD.** `D-ARCH-6` terpenuhi:
`Approve`, `Execute`, dan `Cancel` masing-masing berdiri sendiri.

Jejak konsumen (frontend `patient-procedure.service.js`) membuktikan ketiga jalur yang dipakai layar
dokter setiap hari kini bergantung pada identitas baru:
`POST /select` → `Select`; `PATCH /{id}/remove-draft` → `RemoveDraft`; `PUT /{id}` → `Edit`.

---

## I. Current DoctorConsultation matrix

| Endpoint | Baseline | **Current HEAD** | Rencana `evidence/04` | Status |
|---|---|---|---|---|
| `GET` (5 varian baca) | `Read` | `Read` | `Read` | Tidak berubah |
| `POST /` | `Create` | `Create` | `Create` | Tidak berubah |
| `PUT /{id}` | `Update` | `Update` | `Edit` | ⚠️ **menyimpang** |
| `PATCH /{id}/soap` | `Update` | `Update` | **`WriteSoap`** | ⚠️ **menyimpang** |
| `PATCH /{id}/complete` | `Update` | **`Complete`** | `Complete` | ✅ sesuai |
| `PATCH /{id}/cancel` | `Update` | **`Cancel`** | `Cancel` | ✅ sesuai |

Dua dari empat identitas yang direncanakan sudah ada. Dua sisanya tidak.

**`PUT` tetap `Update` adalah penyimpangan yang tidak berbahaya** — `Update` kini praktis berarti
`Edit`, dan mempertahankan namanya justru menyelamatkan baris policy lama. Penggantian nama menjadi
`Edit` murni kosmetik dan berbiaya migrasi.

**`WriteSoap` tidak dipecah adalah penyimpangan yang berarti.** `D-ARCH-7` menyatakan `WriteSoap`
dan `Complete` adalah capability berbeda. Di HEAD, `Complete` sudah terpisah tetapi penulisan SOAP
masih berbagi izin dengan penyuntingan header konsultasi. Konsekuensinya: admin tidak dapat memberi
"boleh menyunting header konsultasi" tanpa sekaligus memberi "boleh menulis isi rekam medis".

Ini **`OWNER_DECISION_REQUIRED` #1**. Lihat bagian U.

---

## J. Current DoctorQueue matrix

| Endpoint | Baseline | **Current HEAD** | Status |
|---|---|---|---|
| `GET` (4 varian baca) | `Read` | `Read` | Tidak berubah |
| `POST /{id}/call` | `Update` | **`Call`** | ✅ |
| `POST /{id}/start-consultation` | `Update` | **`StartConsultation`** | ✅ |
| `POST /{id}/finish-consultation` | `Update` | **`FinishConsultation`** | ✅ |
| `POST /{id}/skip` | `Update` | **`Skip`** | ✅ |
| `POST /{id}/no-show` | `Update` | **`NoShow`** | ✅ |
| `POST /{id}/requeue` | `Update` | **`Requeue`** | ✅ |

**Keenam aksi yang diminta revalidasi sudah terpisah penuh.** Tidak ada endpoint baru. Tidak ada
endpoint yang hilang. `DoctorQueue.Update` pensiun.

Ini controller dengan dampak terbesar: baseline mencatat **3 baris policy** untuk
`DoctorQueue.Update` (Medis × Dokter Umum, Dokter Spesialis, Dokter IGD) yang kini menjadi yatim,
dan **6 pengguna** yang bergantung padanya.

---

## K. Current PatientAssessment matrix

| Endpoint | Baseline | **Current HEAD** | Rencana | Status |
|---|---|---|---|---|
| `GET` (8 varian baca) | `Read` | `Read` | `Read` | 5 endpoint baca baru |
| `POST /` | `Create` | `Create` | `Create` | Tidak berubah |
| `PUT /{id}` | `Update` | `Update` | `Edit` | ⚠️ kosmetik |
| `PATCH /{id}/complete` | `Update` | **`Complete`** | `Complete` | ✅ |
| `PATCH /{id}/cancel` | `Update` | **`Cancel`** | `Cancel` | ✅ |
| `POST /{id}/addendums` | *(tidak ada)* | **`Amend`** | *(tidak direncanakan)* | 🆕 **baru** |

Pemecahan yang direncanakan sudah selesai secara substansi. Yang baru adalah `Amend`: kemampuan
menambahkan adendum pada dokumen pengkajian yang sudah final. Secara semantik ini memang capability
tersendiri dan **layak** berdiri sendiri — pemisahannya benar.

Yang belum ada adalah keputusan siapa yang boleh memakainya. Sampai ada baris `SysAccessPolicy`,
`Amend` ditolak untuk semua orang kecuali SuperAdmin. Ini **`OWNER_DECISION_REQUIRED` #2**.

---

## L. Current PatientDiagnosis matrix

| Endpoint | Baseline | **Current HEAD** | Rencana | Status |
|---|---|---|---|---|
| `GET` (5 varian baca) | `Read` | `Read` | `Read` | Tidak berubah |
| `POST /` | `Create` | `Create` | `Create` | Tidak berubah |
| `PUT /{id}` | `Update` | `Update` | `Edit` | ⚠️ kosmetik |
| `PATCH /{id}/set-primary` | `Update` | **`SetPrimary`** | `SetPrimary` | ✅ |
| `PATCH /{id}/resolve` | `Update` | **`Resolve`** | `Resolve` | ✅ |
| `PATCH /{id}/cancel` | `Update` | **`Cancel`** | `Cancel` | ✅ |

Ketiga identitas sensitif sudah terpisah. Tidak ada endpoint baru, tidak ada yang hilang.

---

## M. Current PatientVitalSign matrix

| Endpoint | Baseline | **Current HEAD** | Rencana | Status |
|---|---|---|---|---|
| `GET` (7 varian baca) | `Read` | `Read` | `Read` | Tidak berubah |
| `POST /` | `Create` | `Create` | `Create` | Tidak berubah |
| `PUT /{id}` | `Update` | `Update` | `Edit` | ⚠️ kosmetik |
| `PATCH /{id}/verify` | `Update` | **`Verify`** | `Verify` | ✅ |
| `PATCH /{id}/notify-doctor` | `Update` | **`NotifyDoctor`** | `NotifyDoctor` | ✅ |
| `PATCH /{id}/cancel` | `Update` | **`Cancel`** | `Cancel` | ✅ |
| `DELETE /{id}` | `Delete` | `Delete` | `Delete` | Tidak berubah |

Ketiga identitas yang direncanakan sudah terpisah. `Verify` — kendali mutu atas catatan petugas
lain — kini berdiri sendiri sebagaimana dituntut.

---

## N. Queue audio current state

**`QueueVoiceController.cs` identik dengan baseline `e1d1121`.** Tidak ada perubahan sama sekali.

| Endpoint | Otorisasi di HEAD | Identitas registry | Target `D-ARCH-8` |
|---|---|---|---|
| `GET /queue-voice/audio/{dateKey}/{fileName}` | `[Authorize]` tingkat kelas saja | `QueueVoice.Read` lewat fallback | `QueueVoice.PlayAudio` **OR** `QueueDisplayRuntimeRead` |
| `GET /queue-voice/download/{dateKey}/{fileName}` | `[Authorize]` tingkat kelas saja | `QueueVoice.Read` lewat fallback | sama |

Keduanya membawa `[AccessAction("Read", …)]` tetapi **tanpa** `[AccessPermission]`. Jalur
`PermissionRegistryDescriptor.BuildCore` mendaftarkannya memakai nama controller yang
mendeklarasikan, sehingga identitasnya `QueueVoice.Read` — persis seperti yang dicatat
`evidence/04` bagian C.8. Keduanya juga masuk `snapshot.UnenforcedActions`, yang dikunci test
`CompatibilityFallbackMatchesApprovedLegacySetExactly`.

**Keadaan nyata hari ini: setiap akun yang berhasil login dapat memutar dan mengunduh rekaman nama
pasien** bila mengetahui `dateKey` dan `fileName`. Ini persis kondisi yang `D-ARCH-8` dan `O-1`
diputuskan untuk memperbaikinya. Tidak ada regresi; tidak ada kemajuan.

**Target semantik tidak berubah dan tetap valid:** `QueueVoice.PlayAudio` **OR**
`QueueDisplayRuntimeRead`, satu mekanisme yang benar-benar menghasilkan OR, `AllowAnonymous`
ditolak, `QueueDisplayRuntimeRead` tidak boleh menjadi permission user dokter/perawat.

`QueueDisplayRuntimeController` juga tidak berubah: keempat endpoint-nya memakai
`[Authorize(Policy = QueueDisplayRuntimeReadPolicy)]`, terpisah dari matriks Akses Role.

**Catatan implementasi untuk nanti:** `QueueVoice.Read` **tetap hidup** setelah langkah audio,
karena `GET /queue-voice/profiles` masih mendeklarasikannya lewat `[AccessPermission]`. Jadi langkah
audio menambah satu identitas tanpa memensiunkan satu pun.

Frontend sudah siap menerima penyempitan ini — `queue-voice.service.js:196` sudah memiliki penanganan
pesan *"Akun ini belum memiliki izin untuk mengakses audio panggilan."* untuk respons `403`.

---

## O. Current database impact

### O.1 Keterbatasan yang harus dinyatakan jujur

**Pengukuran langsung ke database development tidak dapat dilakukan pada sesi ini.** Tidak ada klien
`psql` di mesin, dan upaya menjalankan query baca lewat assembly `Npgsql` yang tersedia di keluaran
build **ditolak oleh pembatas izin harness**. Tidak ada upaya menyiasati penolakan itu.

Akibatnya seluruh angka di bawah adalah **turunan deterministik dari source HEAD ditambah baseline
`evidence/03`**, bukan hasil pengukuran. Angka-angka ini **wajib diukur ulang** sebelum Fase B
dijalankan. Lihat bagian R.

### O.2 Perubahan skema

**Nol.** Audit terhadap 151 berkas migration yang berubah sejak baseline membuktikan **tidak satu
pun** EF migration menyentuh `SysAccessPolicy`, `SysActionAccess`, `SysControllerAccess`,
`SysApplicationModule`, atau `AspNetUserOrganization`. Satu-satunya kemunculan nama tabel tersebut
ada di `ApplicationDbContextModelSnapshot.cs` — wajar, karena snapshot memuat seluruh entity.

Kesimpulan `evidence/04` bahwa `BE-SEC-003` **tidak memerlukan EF schema migration** tetap berlaku
di HEAD.

### O.3 Keadaan data yang diperkirakan hari ini

`AccessMenuSeeder` berjalan pada **setiap** startup (`Program.cs:1188`). Rekonsiliasinya:

1. Mendaftarkan 22 identitas Fase A + `Amend` sebagai baris `SysActionAccess` baru.
2. Menutup baris yang tidak lagi dideklarasikan source — yaitu `PatientProcedure.Update` dan
   `DoctorQueue.Update` — dengan `IsActive=false`, `IsDelete=true`, tanpa hard delete.
3. **Tidak menyentuh `SysAccessPolicy` sama sekali**, sesuai rancangan: *"memindahkannya ke
   kemampuan baru sama saja memberi hak tanpa keputusan admin."*

`HasAccessAsync` menyaring `!x.IsDelete` pada `SysActionAccess`. Maka pada setiap lingkungan yang
sudah menjalankan aplikasi di HEAD ini:

| Akibat | Rincian |
|---|---|
| **Baris policy menjadi yatim** | Policy yang menunjuk `PatientProcedure.Update` (1 baris baseline) dan `DoctorQueue.Update` (3 baris baseline) tidak lagi mengotorisasi apa pun |
| **Identitas baru tanpa hak** | 22 identitas Fase A + `Amend` terdaftar dengan **nol** baris `SysAccessPolicy` |
| **Kemampuan yang bertahan** | Kelima identitas `F.2` beserta seluruh `Read`, `Create`, dan `Delete` tetap berfungsi normal |

**Ini berarti kehilangan hak yang Fase B dirancang untuk mencegahnya kemungkinan besar sudah
terjadi**, karena Fase A masuk tanpa Fase B mengiringinya.

Dampak yang dapat ditelusuri sampai ke layar, memakai jejak konsumen frontend:

| Aksi dokter di layar | Endpoint | Identitas | Diperkirakan |
|---|---|---|---|
| Memilih tindakan | `POST /patient-procedures/select` | `Select` | **ditolak** |
| Menghapus pilihan tindakan | `PATCH /patient-procedures/{id}/remove-draft` | `RemoveDraft` | **ditolak** |
| Menyunting tindakan | `PUT /patient-procedures/{id}` | `Edit` | **ditolak** |
| Menyetujui / melaksanakan / membatalkan tindakan | `approve` / `execute` / `cancel` | `Approve` / `Execute` / `Cancel` | **ditolak** |
| Memanggil pasien | `POST /doctor-queues/{id}/call` | `Call` | **ditolak** |
| Mulai / selesai konsultasi | `start-consultation` / `finish-consultation` | `StartConsultation` / `FinishConsultation` | **ditolak** |
| Skip / requeue | `skip` / `requeue` | `Skip` / `Requeue` | **ditolak** |
| Menyelesaikan pengkajian | `PATCH /patient-assessments/{id}/complete` | `Complete` | **ditolak** |

**Bukti pendukung bahwa Fase B memang belum pernah dijalankan:** direktori `Migrations/scripts/`
berisi lima skrip `grant-*-access.sql` milik tim lain (Operating Room, Farmasi, Stock Request,
Prescription Dispensing) — **tidak satu pun** untuk identitas pilot Dokter Rawat Jalan. Tidak ada
artefak tertelusur yang memberikan hak atas 22 identitas tersebut.

**Status temuan ini: `HIGHLY LIKELY`, bukan `CONFIRMED`.** Ia mengikuti secara deterministik dari
source dan perilaku seeder, tetapi belum diukur pada database mana pun. Verifikasi adalah langkah
pertama bagian R.

> **DIKOREKSI OLEH PENGUKURAN — 11 September 2026. Kehilangan hak BELUM terjadi.**
>
> Pengukuran aktual membalik kesimpulan di atas, dan hasilnya kabar baik:
>
> - Seluruh identitas hasil pemecahan berstatus **`TIDAK_TERDAFTAR`** — belum pernah dibuat.
> - `PatientProcedure.Update` dan `DoctorQueue.Update` masih **`IsActive=true`, `IsDelete=false`**.
>
> Artinya `AccessMenuSeeder` **belum pernah dijalankan terhadap database ini pada HEAD**. Registry
> masih pada keadaan sebelum Fase A, sehingga policy lama masih hidup dan masih menjaga akses.
>
> Kehilangan hak karena itu **PENDING, bukan REALIZED**: ia akan terjadi pada detik aplikasi HEAD
> pertama kali start terhadap database ini — bukan karena seeder menutup identitas lama, melainkan
> karena endpoint di HEAD sudah menuntut identitas baru yang belum ada di registry.
>
> Konsekuensi untuk perencanaan: Fase B adalah **pencegahan**, bukan pemulihan — dan urutan
> penerapannya menjadi jauh lebih ketat, karena jendela kerusakannya dibuka oleh startup aplikasi
> itu sendiri. Lihat `evidence/07`.

---

## P. Differences from old impact report

| Hal | `evidence/03` + `evidence/04` (2 Sep) | Current HEAD (11 Sep) | Selisih |
|---|---|---|---|
| Status pemecahan source | Belum dikerjakan | **Fase A selesai** | Pekerjaan utama sudah masuk |
| Identitas lama yang pensiun | **7** | **2** | −5, karena `Update` dipertahankan di 4 controller |
| Identitas baru hasil pemecahan | 28 (termasuk `PlayAudio`) | 22 + `Amend`; `PlayAudio` belum | Komposisi berbeda |
| Baris policy menjadi yatim | **8** | **4** | −4 |
| Baris policy yang harus dibuat Fase B | **39** (31 pemecahan + 8 audio) | **42** (34 pemecahan + 8 audio) | Dihitung ulang |
| Perubahan bersih policy efektif | +31 | **+30** | −1, karena `WriteSoap` tidak dipecah |
| `SysActionAccess` aktif | 1.076 → 1.097 (+21) | net **+21** hari ini; **+22** setelah langkah audio | `QueueVoice.Read` bertahan |
| Basis pengukuran | Query langsung 2 Sep | **Tidak dapat diukur** | Seluruh angka absolut kedaluwarsa |
| EF schema migration | Nol | **Nol** — dikonfirmasi ulang | Tidak berubah |

### P.1 Aritmetika Fase B yang dihitung ulang

Memakai jumlah baris policy baseline `evidence/03` sebagai pengali — **angka pengali ini sendiri
belum diverifikasi ulang**:

| Identitas sumber | Baris baseline | Identitas baru | Baris dibuat | Baris dinonaktifkan |
|---|---:|---:|---:|---:|
| `PatientProcedure.Update` *(pensiun)* | 1 | 5 | 5 | 1 |
| `PatientProcedure.Create` *(bertahan)* | 1 | 1 (`Select`) | 1 | 0 |
| `DoctorQueue.Update` *(pensiun)* | 3 | 6 | 18 | 3 |
| `DoctorConsultation.Update` *(bertahan)* | 1 | 2 | 2 | 0 |
| `PatientVitalSign.Update` *(bertahan)* | 1 | 3 | 3 | 0 |
| `PatientAssessment.Update` *(bertahan)* | 1 | 2 | 2 | 0 |
| `PatientDiagnosis.Update` *(bertahan)* | 1 | 3 | 3 | 0 |
| **Subtotal pemecahan** | **9** | | **34** | **4** |
| Audio `QueueVoice.PlayAudio` (`O-1`) | 0 | 1 | 8 | 0 |
| `PatientAssessment.Amend` *(keputusan owner)* | 0 | 1 | 0 atau 1 | 0 |
| **Total** | | | **42** *(+1)* | **4** |

### P.2 Mengapa angka absolut lama tidak boleh dipakai lagi

> **DIKOREKSI OLEH PENGUKURAN — 11 September 2026.** Dugaan di bawah **salah**. Pengukuran aktual
> pada `QuilvianNewDevAndryZain` menunjukkan angkanya **tidak bergerak sama sekali**: total
> `SysAccessPolicy` **498**, efektif **469**, pasangan Departemen × Posisi **11**, `SysActionAccess`
> aktif **1.076** — identik dengan baseline 2 September.
>
> Sebabnya sekarang jelas: skrip `grant-*-access.sql` milik tim lain adalah berkas di repository,
> **bukan** sesuatu yang berjalan sendiri, dan aplikasi HEAD belum pernah start terhadap database
> ini. Database development masih membeku pada keadaan 2 September.
>
> Yang tetap berlaku dari paragraf di bawah hanyalah prinsipnya — angka wajib diukur, bukan
> diasumsikan. Kali ini pengukurannya kebetulan mengonfirmasi angka lama.

Baseline `SysAccessPolicy` total **498** / efektif **469** dan `SysActionAccess` aktif **1.076**
diukur 2 September. Sejak itu 325 commit masuk, termasuk lima skrip `grant-*-access.sql` milik tim
lain, serta modul-modul baru (Radiologi, Nutrisi, Accounting, Petty Cash, Lab, Operating Room).

---

## Q. Risk introduced by team changes

| # | Risiko | Tingkat | Dampak |
|---|---|---|---|
| Q1 | **Fase A masuk produksi-jalur tanpa Fase B** | **TINGGI** | Kehilangan hak yang tidak disengaja pada alur harian Dokter Rawat Jalan. Justru hasil yang `evidence/04` susun seluruh strategi migrasinya untuk mencegah |
| Q2 | **Pemecahan masuk lewat commit berlabel `WIP`** | **TINGGI** | Perubahan otorisasi paling sensitif dalam rangkaian ini masuk aliran integrasi tanpa laporan task tertelusur, tanpa tinjauan, dan tanpa jendela penerapan. Roadmap masih menyatakan task `READY FOR IMPLEMENTATION`, padahal separuhnya sudah berjalan |
| Q3 | **Divergensi `IsCancel`** (D.6) | **SEDANG** | Daftar kewenangan lebih longgar daripada penjaganya; tombol tampil lalu ditolak `403`. Akan berlipat seiring bertambahnya identitas |
| Q4 | **Saklar `Security:Authorization:Enabled`** (C.2) | **SEDANG** | Verifikasi Fase B dapat lulus secara palsu bila dijalankan di lingkungan yang mematikannya. Tidak berbahaya di produksi |
| Q5 | **`PatientAssessment.Amend` tanpa pemilik** | **RENDAH** | Fitur adendum praktis mati untuk non-SuperAdmin sampai ada keputusan pemberian hak |
| Q6 | **Angka impact kedaluwarsa** (P.2) | **SEDANG** | Skrip migrasi yang memakai angka lama sebagai target akan salah menilai keberhasilannya |
| Q7 | **`NurseStationQueue` tetap kasar** (G.1) | **RENDAH** | Di luar scope pilot; dicatat agar tidak hilang, bukan untuk dikerjakan sekarang |
| Q8 | **Gerbang test keamanan tidak dapat dijalankan** (T.1b) | **TINGGI** | `BillingDepositServiceTests.cs` tidak dapat dikompilasi sejak commit `58c61a5b` (10 Sep), sehingga seluruh test keamanan di proyek yang sama ikut mati. Acceptance criteria nomor 7, 8, dan 10 tidak dapat dipenuhi. **Pemblokir Fase B** |

Yang **tidak** terjadi, dan layak dinyatakan eksplisit:

- Tidak ada regresi pada keenam invarian A0.
- Tidak ada EF migration yang menyentuh tabel otorisasi.
- Tidak ada `AllowAnonymous` yang muncul di endpoint audio.
- Tidak ada `QueueDisplayRuntimeRead` yang bocor menjadi permission user.
- Tidak ada endpoint yang kehilangan `[AccessPermission]`-nya.
- Tidak ada ketidakcocokan descriptor yang membuat endpoint gagal-tertutup.
- Frontend tidak bergerak sama sekali.

---

## R. Updated migration strategy

**`BE-SEC-003` tetap tidak memerlukan EF schema migration.** Dikonfirmasi ulang di O.2.

Bila kelak ditemukan kebutuhan perubahan skema dari pekerjaan lain, perubahan itu **dipisahkan** dan
**tidak** dimasukkan ke `BE-SEC-003` — sesuai instruksi pemilik sistem.

Strategi yang diperbarui, karena Fase A tidak lagi perlu dikerjakan:

| Langkah | Isi | Catatan |
|---|---|---|
| **R0** | **Ukur ulang keadaan sebenarnya** dengan query baca-saja | Prasyarat mutlak. Hitung ulang `SysAccessPolicy` total/efektif, `SysActionAccess` aktif, pasangan Departemen × Posisi, status `IsDelete` pada `PatientProcedure.Update` dan `DoctorQueue.Update`, serta jumlah baris untuk kelima identitas yang bertahan. `Migrations/scripts/diagnose-user-access.sql` dapat dipakai ulang |
| **R1** | Konfirmasi apakah kehilangan hak O.3 benar-benar terjadi | Menentukan apakah Fase B adalah *pemulihan* atau *pencegahan* |
| **R2** | Jalankan Fase B sebagai skrip `grant-*.sql` tertelusur | Ikuti pola `Migrations/scripts/grant-*-access.sql` yang sudah mapan. **Di luar** `AccessMenuSeeder`, sesuai larangan implementasi nomor 7 |
| **R3** | Mode laporan lebih dulu, lalu mode tulis | Larangan nomor 8 tetap: tidak ada pre-seeding identitas sebelum deploy |
| **R4** | Langkah audio sebagai perubahan terpisah | Fase C tetap memerlukan perubahan source (`[AccessAction]` `Read` → `PlayAudio` + mekanisme OR), jadi ia adalah langkah source + data, bukan data saja |

**Yang berubah dari strategi lama:** urutan "deploy source lalu perluas policy" tidak lagi berlaku
untuk Fase B, karena source-nya sudah ter-deploy. Fase B kini murni operasi data yang dapat
dijalankan kapan saja, dan **semakin cepat semakin baik** bila R1 memastikan hak memang hilang.

---

## S. Updated deployment strategy

| Fase | Jenis | Jendela penerapan | Rollback |
|---|---|---|---|
| **A** — pemecahan identitas | Source | **Sudah terjadi** | Balikkan `85fcc3fd`; seeder mendaftarkan ulang `PatientProcedure.Update` dan `DoctorQueue.Update`; policy lama hidup kembali karena tidak pernah dihapus |
| **B** — perluasan policy | **Data saja** | Kapan saja; tidak perlu restart | Nonaktifkan 34 baris baru; aktifkan kembali 4 baris yatim |
| **C** — penyempitan audio | Source + data | Perlu jendela + restart | Balikkan source; nonaktifkan 8 baris policy |

Fase B **tidak lagi memerlukan koordinasi jendela penerapan**, karena tidak ada perubahan source
yang menyertainya. Ini penyederhanaan nyata dibanding rencana lama.

Rollback Fase A **masih tersedia penuh dan tanpa titik tanpa kembali**: seeder menutup baris registry
dengan lifecycle, bukan hard delete, sehingga baris `SysAccessPolicy` lama tetap utuh dan menunjuk
`ActionAccessId` yang sama. Ini properti desain A0 yang terbukti berguna persis pada situasi ini.

---

## T. Updated test requirements

### T.1 Keadaan test di HEAD

Berkas test keamanan yang ada:

| Berkas | Isi |
|---|---|
| `PermissionSplitPreparationTests.cs` | Invarian Fase A — 22 identitas, 5 bertahan, 2 pensiun, pemetaan endpoint |
| `CanonicalSecurityContractTests.cs` | Identitas selalu dari `[AccessPermission]`; allowlist fallback dikunci sebagai himpunan |
| `PermissionRegistryInvariantTests.cs` | Invarian registry `BE-SEC-001` |
| `OrganizationAuthorizationProjectionTests.cs` | Proyeksi penempatan organisasi |
| `StaleRegistryAuthorizationTests.cs` | **Baru, 10 Sep** — registry yang sudah ditutup tidak boleh tetap mengotorisasi meski policy lama masih menunjuknya |
| `AccessPermissionEnforcementTests.cs` | Penegakan + parity `GetEffectivePermissionsAsync` |

`StaleRegistryAuthorizationTests` menutup lubang yang berhubungan langsung dengan O.3: ia membuktikan
baris policy yatim **tidak** memberi akses. Keberadaannya memperkuat, bukan melemahkan, kesimpulan
bahwa kemampuan yang bergantung pada identitas pensiun kini tertolak.

### T.1b Gerbang test keamanan **tidak dapat dijalankan** di HEAD

`dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory --filter "FullyQualifiedName~Security"`
dijalankan pada sesi ini. Hasilnya: **proyek gagal dikompilasi; tidak satu pun test berjalan.**
Tidak ada baris `Passed!` maupun `Failed!` pada keluaran — eksekusi berhenti di tahap build.

> **Dikoreksi oleh [`06-be-sec-003-readiness-correction.md`](06-be-sec-003-readiness-correction.md)
> bagian D.** Angka di bawah diambil dari keluaran build yang **terpotong**. Pembangunan penuh
> proyek test menunjukkan **14 galat pada dua berkas milik dua modul** — `BillingDepositServiceTests.cs`
> (11 galat) dan `PatientEncounterCompanyGuarantorTests.cs` (3 galat). Blocker-nya lebih luas
> daripada yang dilaporkan di sini.

Galat kompilasi yang terlihat pada keluaran terpotong — seluruhnya `CS0117` pada satu berkas:

```
Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingDepositServiceTests.cs
  error CS0117: 'BilCalculationVersion' does not contain a definition for 'IsActive'
  error CS0117: 'BilDepositAccount'    does not contain a definition for 'IsActive'
  error CS0117: 'BilDepositMovement'   does not contain a definition for 'IsActive'
  error CS0117: 'BilInvoice'           does not contain a definition for 'IsActive'
```

Penyebabnya **bukan** pekerjaan otorisasi. Entity `Bil*` kini turun dari `IdentityModel` yang tidak
memiliki `IsActive`, sementara berkas test masih menyetel properti itu. Sentuhan terakhir pada berkas
tersebut adalah commit `58c61a5b` — *"Change table with perfix Trx (Patient Encounter and
Perscription)"*, **10 September 2026**, sehari sebelum HEAD.

**Dampaknya terhadap `BE-SEC-003` langsung dan berat.** Keenam berkas test keamanan pada T.1 berada
di **proyek yang sama**. Selama `BillingDepositServiceTests.cs` tidak dapat dikompilasi, seluruh
invarian berikut tidak dapat dibuktikan:

- `PermissionSplitPreparationTests` — invarian Fase A
- `CanonicalSecurityContractTests` — termasuk `CompatibilityFallbackMatchesApprovedLegacySetExactly`
- `PermissionRegistryInvariantTests` — termasuk `ReconcileNeverCreatesAccessPolicy`
- `StaleRegistryAuthorizationTests`
- `AccessPermissionEnforcementTests`
- `OrganizationAuthorizationProjectionTests`

Acceptance criteria `BE-SEC-003` nomor **7**, **8**, dan **10** menuntut test-test ini hijau.
Ketiganya **tidak dapat dipenuhi di HEAD** sampai berkas Billing tersebut diperbaiki.

**Perbaikannya milik tim Billing, bukan `BE-SEC-003`.** Sesuai instruksi pemilik sistem, perubahan
itu **dipisahkan** dan tidak dimasukkan ke `BE-SEC-003` — tetapi ia menjadi **dependency pemblokir**
bagi Fase B, karena tanpa gerbang test yang jalan, Fase B tidak punya alat bukti.

### T.2 Yang harus ditambahkan

| # | Test | Alasan |
|---|---|---|
| T-1 | **Parity `IsCancel`** — penempatan yang dibatalkan tidak boleh muncul di `GetEffectivePermissionsAsync` | Menutup D.6. Wajib, dan sebaiknya mendahului Fase B |
| T-2 | **Parity menyeluruh** — untuk himpunan pasangan yang representatif, `Contains(set, r, a)` harus sama dengan `HasAccessAsync(r, a)`, termasuk pada penempatan yang dibatalkan, kedaluwarsa, dan nonaktif | Test yang ada hanya mencakup tiga pasangan pada penempatan sehat |
| T-3 | **Bukti parity per Departemen × Posisi** sesudah Fase B — himpunan endpoint terjangkau identik sebelum dan sesudah | Acceptance criteria nomor 2 dan 4, dihitung ulang terhadap angka R0 |
| T-4 | **Idempotensi skrip Fase B** — dijalankan dua kali menghasilkan keadaan yang sama | Acceptance criteria nomor 5 |
| T-5 | **`PatientAssessment.Amend` masuk himpunan terkunci** | Agar identitas di luar Fase A tidak lagi menyelinap tanpa keputusan |
| T-6 | **Otorisasi audio terbukti OR**, bukan AND | Fase C; `D-ARCH-8` |
| T-7 | **`CompatibilityFallbackMatchesApprovedLegacySetExactly` diperbarui secara sadar** saat Fase C memindahkan dua endpoint audio keluar dari fallback | Test membandingkan himpunan, sehingga akan gagal bila allowlist tidak ikut diperbarui |

### T.3 Syarat lingkungan verifikasi

Setiap verifikasi manual **wajib** memastikan `Security:Authorization:Enabled` bernilai `true` di
lingkungan pengujian. Bila `false`, `HasAccessAsync` mengembalikan `true` tanpa syarat dan seluruh
pembuktian parity menjadi tidak bermakna. Ini syarat baru yang tidak ada di `evidence/04`.

---

## U. Recommendation

# `PLANNING_UPDATE_REQUIRED`

Planning `BE-SEC-003` **tidak lagi menggambarkan keadaan sebenarnya** dan tidak boleh dieksekusi apa
adanya. Sebagian besar pekerjaannya sudah selesai; mengeksekusi rencana lama akan mengulang Fase A
yang sudah ada, memakai angka database yang kedaluwarsa, dan memakai split matrix yang berbeda dari
yang benar-benar diimplementasikan.

**Jangan memaksakan implementasi berdasarkan `evidence/04`.**

### U.1 Yang harus diperbarui sebelum implementasi dilanjutkan

| # | Pembaruan |
|---|---|
| 1 | Catat `BE-SEC-003` sebagai task **berfase**: Fase A `COMPLETED` (commit `85fcc3fd`, 4 Sep), Fase B `NOT STARTED`, Fase C `NOT STARTED` |
| 2 | Ganti split matrix `evidence/04` bagian C dengan matriks H–M laporan ini — 22 identitas, 5 bertahan, 2 pensiun |
| 3 | Ganti seluruh aritmetika database dengan P.1, dan tandai angka absolut lama sebagai kedaluwarsa |
| 4 | Tambahkan R0 sebagai prasyarat mutlak Fase B |
| 5 | Tambahkan T-1 dan T-2 ke test plan |
| 6 | Tambahkan syarat lingkungan T.3 |
| 7 | Buat laporan task tertelusur untuk Fase A secara retroaktif — perubahan otorisasi sebesar ini tidak boleh hanya berjejak commit `WIP` |

### U.2 `OWNER_DECISION_REQUIRED` — dua keputusan yang menahan

**Keputusan #1 — `DoctorConsultation.WriteSoap`.**
`D-ARCH-7` menyatakan `WriteSoap` dan `Complete` adalah capability berbeda. Fase A memisahkan
`Complete` tetapi membiarkan `PATCH /soap` berbagi `Update` dengan `PUT /{id}`. Pilihannya:

- **(a)** Pecah `WriteSoap` sekarang sebagai bagian Fase B — menegakkan `D-ARCH-7` sepenuhnya,
  menambah 1 identitas dan 1 baris policy.
- **(b)** Terima keadaan sekarang, catat sebagai penyimpangan sadar dari `D-ARCH-7`, dan tunda ke
  fase Business Permission.

Rekomendasi teknis: **(a)**. Biayanya kecil — satu atribut, satu baris policy — dan ia satu-satunya
tuntutan `D-ARCH-7` yang belum terpenuhi. Menundanya berarti "boleh menyunting header konsultasi"
terus menyeret "boleh menulis isi rekam medis".

**Keputusan #2 — penerima `PatientAssessment.Amend`.**
Identitas ini ditambahkan tim lain di luar himpunan Fase A dan belum diberikan kepada siapa pun.
Diperlukan penetapan Departemen × Posisi penerimanya, setara `O-1` untuk audio. Sampai diputuskan,
fitur adendum pengkajian mati untuk seluruh pengguna non-SuperAdmin.

### U.3 Yang tidak berubah dan tidak perlu diputuskan ulang

- Keenam invarian A0 tetap utuh — **tidak ada regression, tidak ada STOP**.
- `D-ARCH-6` (Procedure) **terpenuhi penuh** di HEAD.
- `D-ARCH-8`, `D-ARCH-10`, dan `O-1` (audio) tetap valid tanpa perubahan; source-nya belum tersentuh.
- `D-ARCH-9` (Medical Certificate) tetap `BROKEN_DEPENDENCY`, di luar scope.
- Tidak ada EF schema migration untuk `BE-SEC-003`.
- Scope pilot tetap enam controller; `NurseStationQueue` **tidak** ditarik masuk.

### U.4 Urutan tindakan yang disarankan

1. **R0** — ukur ulang database dengan query baca-saja *(butuh izin akses database yang pada sesi ini ditolak)*.
2. **R1** — pastikan apakah kehilangan hak O.3 nyata; bila ya, perlakukan Fase B sebagai **pemulihan berprioritas**.
3. **Pulihkan gerbang test** — perbaiki `BillingDepositServiceTests.cs` sebagai task Billing terpisah (T.1b). Tanpa ini Fase B tidak dapat dibuktikan.
4. Tutup dua keputusan owner di U.2.
5. Perbarui planning sesuai U.1.
6. Baru jalankan Fase B, lalu Fase C.

Langkah 1–3 saling bebas dan dapat berjalan paralel. Langkah 3 bukan pekerjaan `BE-SEC-003` dan
tidak boleh diselipkan ke dalamnya, tetapi ia **memblokir** langkah 6.

---

## Lampiran — batas revalidasi ini

Dinyatakan jujur, agar tidak ada yang diperlakukan lebih pasti daripada buktinya:

| Hal | Status |
|---|---|
| Inventaris endpoint, identitas, dan descriptor | **Terverifikasi** dari source HEAD |
| Perbandingan terhadap baseline `e1d1121` | **Terverifikasi** lewat pembacaan source pada kedua SHA |
| Enam invarian A0 | **Terverifikasi** dari source |
| Ketiadaan EF migration pada tabel otorisasi | **Terverifikasi** atas 151 berkas migration |
| Ketiadaan skrip grant untuk identitas pilot | **Terverifikasi** atas `Migrations/scripts/` |
| Keadaan baris `SysAccessPolicy` dan `SysActionAccess` sebenarnya | **TIDAK terverifikasi** — akses database ditolak harness |
| Kehilangan hak pada O.3 | **`HIGHLY LIKELY`**, diturunkan deterministik dari source + perilaku seeder; **belum diukur** |
| Gerbang test keamanan tidak dapat dijalankan | **Terverifikasi** — 8 galat `CS0117`, nol test berjalan (T.1b) |

Tidak ada source yang diubah. Tidak ada migration yang dibuat. Tidak ada database yang ditulis.
Tidak ada commit. Tidak ada push.
