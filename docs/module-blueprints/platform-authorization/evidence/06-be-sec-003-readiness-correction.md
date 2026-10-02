# BE-SEC-003 — Readiness Correction (pra-Fase B)

> **Mode:** koreksi persiapan. **Tidak ada** policy expansion, **tidak ada** database write,
> **tidak ada** EF migration, **tidak ada** commit/push. Perubahan source terbatas pada deklarasi
> technical permission dan satu perbaikan paritas otorisasi.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Correction evidence** — bukan laporan task |
| Task | `BE-SEC-003` — Technical Permission Granularity Hardening |
| Laporan task tracked | [`../task/report/backend/BE-SEC-003.md`](../task/report/backend/BE-SEC-003.md) |
| Bukti sebelumnya | [`05-be-sec-003-current-head-revalidation.md`](05-be-sec-003-current-head-revalidation.md) |
| HEAD saat koreksi | `8131913380063aee7f2ebe2bda54e888d88b2b02` (`AndryZain`) |
| Tanggal | 11 September 2026 |
| Fase | A `COMPLETED` · **A′ (koreksi ini) `COMPLETED`** · B `NOT STARTED` · C `NOT STARTED` |

---

## A. `DoctorConsultation.WriteSoap` — koreksi diterapkan

**Keputusan pemilik sistem: `APPROVED`.** Menutup satu-satunya tuntutan `D-ARCH-7` yang belum
terpenuhi pada revalidasi `evidence/05` bagian I.

### A.1 Kontrak sesudah koreksi

| Endpoint | Sebelum | **Sesudah** | Sensitivitas |
|---|---|---|---|
| `PUT /doctor-consultations/{id}` | `Update` | `Update` *(tetap)* | Non-sensitif |
| `PATCH /doctor-consultations/{id}/soap` | `Update` | **`WriteSoap`** | **Sensitif** — isi rekam medis |
| `PATCH /doctor-consultations/{id}/complete` | `Complete` | `Complete` *(tetap)* | **Sensitif** |
| `PATCH /doctor-consultations/{id}/cancel` | `Cancel` | `Cancel` *(tetap)* | **Sensitif** |

`DoctorConsultation.Update` kini hanya menjaga `PUT /{id}` — penyuntingan header konsultasi.
Admin akhirnya dapat memberikan "boleh menyunting header" tanpa ikut memberikan "boleh menulis
isi rekam medis".

### A.2 Yang diubah

Dua baris atribut pada satu method. **Body method tidak disentuh**, sehingga tidak ada perubahan
business logic:

```diff
-[AccessAction("Update", "Autosave Doctor Consultation SOAP", …, AccessType = AccessTypes.Update, SortOrder = 4)]
-[AccessPermission("DoctorConsultation", "Update")]
+[AccessAction("WriteSoap", "Autosave Doctor Consultation SOAP", …, AccessType = AccessTypes.Update, SortOrder = 4)]
+[AccessPermission("DoctorConsultation", "WriteSoap")]
 public async Task<IActionResult> UpdateSoap(Guid id, [FromBody] UpdateDoctorConsultationSoapRequest request)
```

**Kedua atribut diubah bersamaan**, sesuai kontrak penamaan `rules/backend/role-access-rules.md`
bagian 3: argumen ke-2 `[AccessPermission]` wajib sama persis dengan argumen ke-1 `[AccessAction]`
pada method yang sama. Mengubah salah satunya saja menghasilkan **403 permanen yang tidak dapat
diperbaiki dari layar Akses Role**, karena baris untuk dicentang tidak pernah dibuat.

`AccessType` tetap `AccessTypes.Update` — salah satu dari empat nilai yang sah. `VisibleInRoleAccess`
tetap `true`, `IsSystemOnly` tetap `false`. Verifikasi otomatis atas seluruh controller pilot:
**nol ketidakcocokan descriptor**.

### A.3 Dampak terhadap himpunan identitas

| | Sebelum koreksi | **Sesudah** |
|---|---:|---:|
| Identitas baru hasil pemecahan | 22 | **23** |
| Identitas lama yang bertahan | 5 | 5 |
| Identitas lama yang pensiun | 2 | 2 |

`DoctorConsultation.Update` **tetap** identitas yang bertahan — `PUT /{id}` masih
mendeklarasikannya — sehingga baris `SysAccessPolicy` lama **tidak menjadi yatim** dan tidak ada
kehilangan hak tambahan akibat koreksi ini. Yang bertambah hanyalah satu identitas baru yang belum
diberikan kepada siapa pun.

---

## B. Perbaikan paritas `IsCancel`

Menutup temuan `evidence/05` bagian D.6.

### B.1 Cacatnya

`GetEffectivePermissionsAsync` — jalur yang menerbitkan daftar kewenangan untuk frontend —
kehilangan penyaring `!organization.IsCancel` yang sudah ada di `HasAccessAsync` sejak Phase A0.
Akibatnya penempatan organisasi yang **dibatalkan** tetap menyumbang kemampuan ke daftar, lalu
ditolak `403` oleh penjaganya. Dokumentasi metode itu sendiri mewajibkan keduanya identik.

### B.2 Perbaikannya

```diff
 where organization.UserId == user.Id
       && organization.IsActive
       && !organization.IsDelete
+      && !organization.IsCancel
       && (!organization.EffectiveStartDate.HasValue ||
           organization.EffectiveStartDate.Value <= now)
       && (!organization.EffectiveEndDate.HasValue ||
           organization.EffectiveEndDate.Value >= now)
```

Satu baris. Predikat kelayakan penempatan pada kedua jalur kini identik:

| Penyaring | `HasAccessAsync` | `GetEffectivePermissionsAsync` |
|---|:---:|:---:|
| `IsActive` | ✅ | ✅ |
| `!IsDelete` | ✅ | ✅ |
| `!IsCancel` | ✅ | ✅ **(diperbaiki)** |
| `EffectiveStartDate` | ✅ | ✅ |
| `EffectiveEndDate` | ✅ | ✅ |

Tidak ada semantics lain yang diubah. Cabang SuperAdmin, penyaring `system-only`, dan penyaring
policy dibiarkan apa adanya.

### B.3 Regression test

Ditambahkan `GetEffectivePermissions_ExcludesCancelledOrganizationAssignment` pada
`Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/AccessPermissionEnforcementTests.cs`.
Test menegaskan **kedua** sisi sekaligus pada satu penempatan yang dibatalkan:

```csharp
// Penjaga menolak penempatan yang dibatalkan.
Assert.False(await service.HasAccessAsync(principal, BillingWriteOffController, ApproveAction));

// Daftar wajib sepakat dengan penjaganya, bukan lebih longgar.
Assert.False(Contains(set, BillingWriteOffController, ApproveAction));
```

Helper `AssignUserToOrganizationAsync` diberi parameter opsional `isCancel = false`. Seluruh 21
pemanggil lama memakai argumen posisional empat pertama atau argumen bernama, sehingga tidak ada
yang rusak oleh penambahan parameter.

---

## C. `PatientAssessment.Amend` — authority analysis

**Tidak ada grant yang diberikan. Tidak ada `SysAccessPolicy` yang dibuat. Default tetap
`FAIL CLOSED`.** Analisis ini read-only.

Hasil: **Opsi A — source cukup untuk menurunkan authority model berbasis bukti.**

### C.1 Siapa author sebuah assessment

Dicatat pada `MrcClinicalDocumentIntegrity.AuthorUserId` saat pengkajian difinalkan
(`PatientAssessmentController.cs:942`):

```csharp
var authorUserId = entity.AssessmentByUserId ?? entity.CreateBy;
if (authorUserId == Guid.Empty) authorUserId = actorUserId;

await _integrityService.RegisterSignedAsync(
    ClinicalDocumentKind.Assessment, entity.Id, entity.PatientId, entity.EncounterId, authorUserId, …);
```

Author ditetapkan **sistem**, bukan dikirim klien. `ClinicalDocumentKind.Assessment` termasuk
`JenisYangDitegakkan`, sehingga pengkajian benar-benar tunduk aturan keutuhan.

### C.2 Siapa yang dapat edit

| Keadaan dokumen | Jalur | Penjaga |
|---|---|---|
| Belum final | `PUT /patient-assessments/{id}` | `PatientAssessment.Update` **+** `_integrityService.EnsureMutableAsync` (`:717`) |
| Sudah terkunci | `POST /patient-assessments/{id}/addendums` | `PatientAssessment.Amend` **+** `RM-DEC-004` |
| Dibatalkan | — | Ditolak: *"Catatan ini sudah dibatalkan dan tidak dapat dikoreksi."* |

Dokumen yang sudah terkunci **tidak dapat disunting langsung sama sekali**. Addendum adalah
satu-satunya jalur, dan isi dokumen induk tidak pernah tersentuh.

### C.3 Original author rule — **tersedia dan sudah ditegakkan**

`ClinicalNoteAddendumService.ResolveAuthorityAsync`, didokumentasikan sebagai `RM-DEC-004`,
bertingkat empat:

| Tingkat | Syarat | Hasil |
|---:|---|---|
| 1 | Pengguna adalah **penulis asli** (`AuthorUserId == actorUserId`) | **Boleh** |
| 2 | Akun penulis asli **nonaktif** *dan* pengguna berwenang sebagai pengganti | Boleh |
| 3 | Ada **penetapan berhalangan** yang masih berlaku *dan* pengguna berwenang sebagai pengganti | Boleh |
| 4 | Selain itu | **Ditolak** |

Ditambah tiga pra-pemeriksaan: belum terdaftar → *"belum final, perbaiki langsung"*; `Draft` →
*"belum terkunci"*; `Cancelled` → *"tidak dapat dikoreksi"*.

**Yang menentukan hasil analisis ini:** `PatientAssessmentController` memanggil service dengan
`actorHasSubstituteAuthority: false` — **nilai tetap, bukan turunan**. Tingkat 2 dan 3 karena itu
**permanen tertutup** pada endpoint ini. Lewat `/patient-assessments/{id}/addendums`, **hanya
penulis asli** yang dapat menambahkan koreksi.

### C.4 `ClinicalDocumentIntegrity` dan `InpatientDocumentCorrectionAuthority`

Keduanya relevan, dan keduanya sudah punya aturan:

| Komponen | Perannya |
|---|---|
| `ClinicalDocumentIntegrityService` | Pemilik kunci dokumen dan catatan `AuthorUserId`. Menentukan apakah dokumen masih boleh disunting |
| `ClinicalNoteAddendumService` | Pemilik `RM-DEC-004` — aturan siapa boleh menambah koreksi |
| `InpatientDocumentCorrectionAuthorityService` | Lapisan tambahan bagi **pengganti** pada konteks rawat inap; sumber kebenarannya penugasan dokter berperiode. Dipanggil `ClinicalNoteAddendumController`, **tidak** dipanggil `PatientAssessmentController` karena jalur pengganti memang tertutup di sana |

Jalur pengganti untuk pengkajian **tetap ada**, tetapi bukan di endpoint ini:

```
POST /clinical-note-addendums/by-document/Assessment/{id}/as-substitute
     → [AccessPermission("ClinicalNoteAddendum", "CreateAsSubstitute")]
     → + InpatientDocumentCorrectionAuthorityService.EnsureMaySubstituteAsync
```

### C.5 Doctor/Nurse distinction — **tidak dipakai sebagai penentu kewenangan, dan itu benar**

Kewenangan pengganti dinyatakan sebagai **technical permission tersendiri**
(`ClinicalNoteAddendum.Create` vs `ClinicalNoteAddendum.CreateAsSubstitute`) — dua endpoint, dua
permission — bukan sebagai pemeriksaan peran di dalam kode. Ini sesuai
`rules/backend/role-access-rules.md` bagian 5 dan 6: kewenangan yang melekat pada data (identitas
penulis) tetap diperiksa backend, sementara "boleh memakai fitur ini" diserahkan ke matriks Akses
Role.

Tidak ditemukan `IsInRole`, daftar nama peran, nama departemen, atau `UserType` pada jalur
otorisasi addendum.

### C.6 Assessment type — **ada sebagai data, sengaja TIDAK membedakan authority**

`PatientAssessmentType` memang membedakan dua profesi:

| Nilai | Profesi |
|---|---|
| `Initial`, `Reassessment`, `DailyReassessment`, `DischargePlanning` | Keperawatan |
| `MedicalInitial`, `MedicalReassessment` | Medis (DPJP/dokter) |

Tetapi source menetapkan secara eksplisit bahwa pembedaan itu **bukan urusan hak akses**:

> *"Pembedaan antara kajian medis dan pengkajian keperawatan karena itu dijaga aturan bisnis lewat
> nilai enum ini, **bukan oleh mesin hak akses** yang hanya melihat satu sumber daya."*
> — `Enums/PatientAssessmentType.cs`

**Konsekuensi: `PatientAssessment.Amend` tetap SATU identitas.** Memecahnya per jenis pengkajian
akan melawan keputusan desain yang sudah tertulis, dan `evidence/05` bagian G sudah menetapkan
bahwa pemecahan hanya sah bila semantik bisnisnya memang berbeda — di sini tidak.

### C.7 Authority model — kesimpulan

```
PatientAssessment.Amend
  = "boleh menambahkan koreksi pada pengkajian yang SAYA tulis sendiri"

Gerbang 1 — permission  : PatientAssessment.Amend      (matriks Akses Role, keputusan admin)
Gerbang 2 — data        : RM-DEC-004 tingkat 1         (sudah ditegakkan, tidak dapat dilewati)

Non-penulis → ditolak 403 oleh Gerbang 2, berapa pun luas Gerbang 1 diberikan.
```

**Implikasi praktis untuk Fase B:** `Amend` adalah *feature gate*, bukan *authority gate*.
Memberikannya luas **tidak dapat** membuat siapa pun mengoreksi pengkajian orang lain. Risiko
privilege broadening-nya nol.

**Usulan cakupan grant — untuk diputuskan pemilik sistem, belum dijalankan:** pasangan Departemen ×
Posisi yang **sudah memegang `PatientAssessment.Complete`**. Alasannya deduktif: hanya orang yang
dapat memfinalkan pengkajian yang dapat menjadi penulisnya, sehingga di luar himpunan itu `Amend`
tidak menambah kemampuan apa pun.

**Keputusan terpisah yang tidak ditarik ke `BE-SEC-003`:** apakah `ClinicalNoteAddendum.CreateAsSubstitute`
diberikan untuk konteks pengkajian. Itu permission milik modul Rekam Medis, endpoint berbeda, dan
keputusan berbeda.

---

## D. Test blocker — dua berkas, dua modul

**Tidak ada perubahan source Billing maupun Patient Encounter pada task ini.** Laporan saja,
sesuai instruksi.

> **Koreksi terhadap `evidence/05` bagian T.1b.** Revalidasi sebelumnya melaporkan *"8 galat,
> seluruhnya pada satu berkas"*. Angka itu diambil dari keluaran yang terpotong. Pembangunan
> penuh proyek test pada sesi ini menunjukkan **14 galat pada dua berkas milik dua modul
> berbeda**. Blocker-nya lebih luas daripada yang dilaporkan, bukan lebih sempit.

| Field | Isi |
|---|---|
| **Gejala** | `Tests/QuilvianSystemBackend.UnitTests.InMemory` **gagal dikompilasi** (`Build FAILED`); nol test berjalan |
| **Jumlah galat** | **14**, pada **dua** berkas |
| **Berkas 1** | `Tests/.../BillingManagement/BillingDepositServiceTests.cs` — 11 galat `CS0117`, baris 596, 610, 622, 633, 716, 730, 742, 753, 764, 776, 787 |
| **Berkas 2** | `Tests/.../HealthServices/RegistrationManagement/PatientEncounterCompanyGuarantorTests.cs` — 3 galat, baris 562, 575, 577 |
| **Modul pemilik 1** | Billing — `Areas/HealthServices/BillingManagement/Billing/`, prefix `Bil` |
| **Modul pemilik 2** | Registration Management — `Areas/HealthServices/RegistrationManagement/`, Patient Encounter |

### D.1 Galat persis

**Berkas 1 — Billing** (11 galat, seluruhnya `CS0117` atas properti `IsActive`):

```
BillingDepositServiceTests.cs(596,13): error CS0117: 'BilDepositAccount'     does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(610,13): error CS0117: 'BilDepositMovement'    does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(622,13): error CS0117: 'BilInvoice'            does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(633,13): error CS0117: 'BilCalculationVersion' does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(716,13): error CS0117: 'BilDepositAccount'     does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(730,13): error CS0117: 'BilDepositMovement'    does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(742,13): error CS0117: 'BilDepositMovement'    does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(753,13): error CS0117: 'BilDepositMovement'    does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(764,13): error CS0117: 'BilDepositMovement'    does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(776,13): error CS0117: 'BilInvoice'            does not contain a definition for 'IsActive'
BillingDepositServiceTests.cs(787,13): error CS0117: 'BilCalculationVersion' does not contain a definition for 'IsActive'
```

**Berkas 2 — Patient Encounter** (3 galat, tiga jenis berbeda):

```
PatientEncounterCompanyGuarantorTests.cs(562,9):  error CS8410: 'PatientEncounterTestWorld': type used in an
                                                   asynchronous using statement must implement 'System.IAsyncDisposable'
PatientEncounterCompanyGuarantorTests.cs(575,45): error CS1061: 'PatientEncounterController' does not contain a
                                                   definition for 'GetEncounterById'
PatientEncounterCompanyGuarantorTests.cs(577,50): error CS0117: 'PatientEncounterTestWorld' does not contain a
                                                   definition for 'Payload'
```

### D.2 Root cause

Keempat entity turun dari `IdentityModel`:

```csharp
public class BilDepositMovement : IdentityModel      // juga BilDepositAccount, BilInvoice, BilCalculationVersion
```

`Models/IdentityModel.cs` menyediakan `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`,
`DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete` — **tidak
ada `IsActive`**. Keempat entity juga tidak mendeklarasikannya sendiri. Lifecycle-nya memakai
`IsDelete`/`IsCancel`, bukan `IsActive`.

Berkas modelnya **tidak berubah sejak 24 Agustus 2026**. Yang bergerak adalah berkas test-nya:
ditambahkan `0e805dea` (3 Sep), diisi `1b067dee` dan `bc2db9eb` (9 Sep), disentuh terakhir
`58c61a5b` (10 Sep). Jadi test-nya ditulis terhadap bentuk entity yang tidak pernah ada pada
branch ini — pola khas divergensi merge.

**Berkas 2 — Patient Encounter.** Akar masalahnya sejenis tetapi bukan soal `IsActive`: test
memanggil `PatientEncounterController.GetEncounterById` yang tidak ada pada controller versi
branch ini, serta memakai helper `PatientEncounterTestWorld` yang tidak memiliki `DisposeAsync`
maupun `Payload`. Berkas ini ditulis `bc2db9eb` (9 Sep) dan disentuh `58c61a5b` (10 Sep) — **dua
commit yang sama** dengan berkas Billing.

Kedua blocker karena itu berasal dari satu peristiwa integrasi yang sama, dan keduanya adalah
divergensi antara test dan source yang menyertainya — bukan cacat pada modul otorisasi.

### D.3 Dampak terhadap `BE-SEC-003`

Berat, dan bukan karena kesalahan pekerjaan otorisasi. Seluruh test keamanan berada di **proyek
yang sama**, sehingga ikut mati:

`PermissionSplitPreparationTests` · `CanonicalSecurityContractTests` ·
`PermissionRegistryInvariantTests` · `StaleRegistryAuthorizationTests` ·
`AccessPermissionEnforcementTests` · `OrganizationAuthorizationProjectionTests`

Acceptance criteria `BE-SEC-003` nomor **7**, **8**, dan **10** menuntut test-test itu hijau.
Ketiganya tidak dapat dipenuhi sampai **kedua** berkas test itu diperbaiki.

### D.4 Dua task terpisah yang disarankan

Keduanya **di luar `BE-SEC-003`** dan tidak boleh diselipkan ke dalamnya.

**Task 1 — Billing**

| Field | Usulan |
|---|---|
| Judul | Perbaiki `BillingDepositServiceTests.cs` agar sesuai bentuk entity `Bil*` |
| Pemilik | Tim Billing |
| Klasifikasi | `LIGHT` — hanya berkas test |
| Isi | Hapus penyetelan `IsActive` pada keempat entity, atau tambahkan kolom `IsActive` pada entity bila lifecycle-nya memang menuntutnya (keputusan Billing, bukan keputusan `BE-SEC-003`) |
| Berkas terdampak | `Tests/.../BillingManagement/BillingDepositServiceTests.cs` — bila opsi kedua dipilih, ditambah keempat model `Bil*` beserta EF migration-nya |

**Task 2 — Patient Encounter**

| Field | Usulan |
|---|---|
| Judul | Perbaiki `PatientEncounterCompanyGuarantorTests.cs` agar sesuai kontrak controller dan helper saat ini |
| Pemilik | Tim Registration Management |
| Klasifikasi | `LIGHT` — hanya berkas test dan helper-nya |
| Isi | Sesuaikan pemanggilan ke nama aksi controller yang benar-benar ada; lengkapi `PatientEncounterTestWorld` dengan `DisposeAsync` dan `Payload`, atau ubah test agar tidak memerlukannya |
| Berkas terdampak | `Tests/.../HealthServices/RegistrationManagement/PatientEncounterCompanyGuarantorTests.cs` beserta helper `PatientEncounterTestWorld` |

**Larangan berlaku untuk keduanya:** jangan menyelesaikannya dengan menghapus, meng-comment, atau
mengecualikan berkas test dari proyek. Itu menyembunyikan divergensi, bukan menutupnya.

---

## E. SQL read-only untuk validasi dampak database

Ditulis di [`Migrations/scripts/diagnose-be-sec-003-permission-state.sql`](../../../../Migrations/scripts/diagnose-be-sec-003-permission-state.sql),
mengikuti pola `diagnose-user-access.sql` yang sudah ada.

**Hanya `SELECT`.** Tidak ada `INSERT`, `UPDATE`, `DELETE`, DDL, atau `\copy`. Tidak memuat
connection string maupun kredensial apa pun.

| Bagian | Isi |
|---|---|
| **A** | `SysAccessPolicy` total (fisik) |
| **B** | `SysAccessPolicy` efektif (`IsAllowed ∧ IsActive ∧ ¬IsDelete`) + jumlah pasangan Departemen × Posisi |
| **C** | `SysActionAccess` aktif |
| **D** | Status dua identitas pensiun — `PatientProcedure.Update`, `DoctorQueue.Update` — beserta jumlah policy efektif yang menggantung padanya |
| **E** | Inventaris lengkap 29 identitas pilot: `ControllerAccessId`, `ActionAccessId`, resource, action, `DepartmentId`/`DepartmentName`, `PositionId`/`PositionName`, `IsAllowed`, `IsActive`, `IsDelete`, dan jumlah pengguna terdampak |
| **F** | Ringkasan per identitas: `TIDAK_TERDAFTAR` / `REGISTRY_DITUTUP` / `KOSONG` / `ADA` |
| **G** | Pengguna aktif per pasangan Departemen × Posisi yang terdampak |

Daftar identitas pada bagian E dan F **sudah memuat `DoctorConsultation.WriteSoap`** hasil koreksi
bagian A, serta `PatientAssessment.Amend`.

Bagian F adalah jawaban langsung atas pertanyaan terbuka `evidence/05` bagian O: identitas dengan
status `KOSONG` berarti kemampuannya terdaftar tetapi belum diberikan kepada siapa pun — tepat
yang harus dipulihkan Fase B.

---

## F. Keadaan fase `BE-SEC-003` sesudah koreksi

| Fase | Isi | Status |
|---|---|---|
| **A** | Pemecahan 22 identitas di source + test invarian (commit `85fcc3fd`, 4 Sep) | **`COMPLETED`** |
| **A′** | Koreksi persiapan: `WriteSoap` (identitas ke-23), paritas `IsCancel`, regression test, SQL diagnostik | **`COMPLETED`** — dokumen ini |
| **B** | Perluasan `SysAccessPolicy` ke *exact historical capability set* | **`NOT STARTED`** |
| **C** | Penyempitan audio antrean — `QueueVoice.PlayAudio` **OR** `QueueDisplayRuntimeRead` | **`NOT STARTED`** |

`BE-SEC-003` secara keseluruhan **belum selesai** dan tidak ditandai selesai.

### F.1 Aritmetika Fase B yang diperbarui

Memperbarui `evidence/05` bagian P.1 dengan tambahan `WriteSoap`. Pengali diambil dari baseline
`evidence/03` yang **masih harus diukur ulang** lewat SQL bagian E.

| Identitas sumber | Baris baseline | Identitas baru | Baris dibuat | Baris dinonaktifkan |
|---|---:|---:|---:|---:|
| `PatientProcedure.Update` *(pensiun)* | 1 | 5 | 5 | 1 |
| `PatientProcedure.Create` *(bertahan)* | 1 | 1 (`Select`) | 1 | 0 |
| `DoctorQueue.Update` *(pensiun)* | 3 | 6 | 18 | 3 |
| `DoctorConsultation.Update` *(bertahan)* | 1 | **3** (`WriteSoap`, `Complete`, `Cancel`) | **3** | 0 |
| `PatientVitalSign.Update` *(bertahan)* | 1 | 3 | 3 | 0 |
| `PatientAssessment.Update` *(bertahan)* | 1 | 2 | 2 | 0 |
| `PatientDiagnosis.Update` *(bertahan)* | 1 | 3 | 3 | 0 |
| **Subtotal pemecahan** | **9** | | **35** | **4** |
| Audio `QueueVoice.PlayAudio` (`O-1`, Fase C) | 0 | 1 | 8 | 0 |
| `PatientAssessment.Amend` *(keputusan owner)* | 0 | 1 | 0 atau 1 | 0 |
| **Total** | | | **43** *(+1)* | **4** |

Perubahan dari `evidence/05`: 34 → **35** baris dibuat pada subtotal pemecahan, karena `WriteSoap`
menambah satu baris pada pasangan Departemen × Posisi yang memegang `DoctorConsultation.Update`.
Perubahan bersih policy efektif menjadi **+31**.

---

## G. Blocker yang tersisa

| # | Blocker | Pemilik | Menahan |
|---|---|---|---|
| 1 | `BillingDepositServiceTests.cs` (11 galat) dan `PatientEncounterCompanyGuarantorTests.cs` (3 galat) tidak dapat dikompilasi | Tim Billing dan tim Registration Management | Seluruh gerbang test keamanan; acceptance criteria 7, 8, 10 |
| 2 | Keadaan database belum terukur | Pemilik sistem | Seluruh aritmetika Fase B. SQL sudah siap di bagian E |
| 3 | Cakupan grant `PatientAssessment.Amend` | Pemilik sistem | Hanya butir `Amend`; tidak menahan sisa Fase B |

Ketiganya saling bebas dan dapat diselesaikan paralel.

---

## H. Langkah berikutnya untuk Fase B

1. Jalankan `Migrations/scripts/diagnose-be-sec-003-permission-state.sql` pada database
   development. Baca bagian **F** lebih dulu: identitas berstatus `KOSONG` adalah kemampuan yang
   hari ini ditolak untuk semua orang.
2. Bandingkan bagian **D** dengan harapan: `PatientProcedure.Update` dan `DoctorQueue.Update`
   seharusnya `IsActive = false`, `IsDelete = true`. Bila masih aktif, aplikasi belum pernah start
   di HEAD ini dan kehilangan haknya belum terjadi — Fase B menjadi **pencegahan**, bukan
   pemulihan.
3. Minta tim Billing dan tim Registration Management memperbaiki kedua blocker `D.4` agar gerbang test hidup kembali.
4. Tutup keputusan cakupan `Amend` (bagian C.7).
5. Susun skrip `grant-*.sql` Fase B memakai angka hasil langkah 1 — **di luar** `AccessMenuSeeder`,
   mode laporan lebih dulu, lalu mode tulis.
6. Fase C (audio) menyusul sebagai perubahan source + data terpisah.

**Pastikan `Security:Authorization:Enabled` bernilai `true`** di lingkungan verifikasi. Bila
`false`, `HasAccessAsync` mengembalikan `true` tanpa syarat dan seluruh pembuktian Fase B menjadi
tidak bermakna.

---

## Lampiran — batas koreksi ini

| Hal | Status |
|---|---|
| Koreksi `WriteSoap` | **Diterapkan**, terverifikasi dari source |
| Paritas `IsCancel` | **Diterapkan**, terverifikasi dari source |
| `dotnet build` proyek utama | **Berhasil** — `QuilvianSystemBackend.dll` dibangun ulang pukul 10:46, sesudah seluruh suntingan source; nol galat |
| Kompilasi berkas yang diubah task ini | **Bersih** — nol galat pada `DoctorConsultationController.cs`, `AccessPermissionService.cs`, `AccessPermissionEnforcementTests.cs`, `PermissionSplitPreparationTests.cs` |
| Regression test `IsCancel` | **Ditulis dan terkompilasi**; eksekusinya terhalang dua blocker test di luar modul ini |
| Authority model `Amend` | **Diturunkan dari source**, tidak ada grant yang diberikan |
| Keadaan database | **Belum terukur** — SQL disiapkan, eksekusi milik pemilik sistem |
| Fase B | **Belum dijalankan** |
| Fase C (audio) | **Belum dijalankan**; `QueueVoiceController` tidak disentuh |

Tidak ada policy migration. Tidak ada database write. Tidak ada EF migration. Tidak ada commit.
Tidak ada push.
