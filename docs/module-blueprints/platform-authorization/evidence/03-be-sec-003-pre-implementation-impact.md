# BE-SEC-003 — Pre-Implementation Impact Report

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `SEC-BP-001` |
| Task ID | `BE-SEC-003` |
| Judul | Technical Permission Granularity Hardening — pilot Dokter Rawat Jalan |
| Klasifikasi | `HEAVY` |
| Status | `PRE-IMPLEMENTATION` — belum ada satu baris source pun yang diubah |
| Dokumen sumber | `evidence/01-be-sec-002-audit-architecture.md`, `evidence/02-be-sec-002-decision-closure.md` |
| Backend SHA | `e1d112142510baa86ccd89977bc7189c89ed012b` |
| Frontend SHA | `2b9e3b074f8a3839857e123515353dd2f3233ac3` |
| Sumber data dampak | Database **development**, query read-only, dijalankan 2 September 2026 |
| Wewenang | `N-5` disetujui pemilik sistem: read-only query pada database development |
| Tanggal | 2 September 2026 |

---

## 1. Ringkasan

Tujuh identitas technical permission yang terlalu kasar dipecah menjadi 28 identitas. Kemampuan
efektif setiap Departemen × Posisi **tidak berubah sama sekali**.

| Ukuran | Nilai aktual dari database development |
| --- | ---: |
| Identitas yang dipecah | **7** |
| Identitas baru | **28** |
| Baris `SysAccessPolicy` yang diperluas | **9** |
| Baris `SysAccessPolicy` sesudah perluasan | **40** |
| Selisih bersih baris policy | **+31** |
| Departemen × Posisi terdampak | **3** dari 11 |
| Pengguna aktif terdampak | **6** dari 39 |
| Kemampuan yang hilang | **0** |
| Kemampuan baru yang diberikan | **0** |

Satu perubahan **bukan** pemecahan dan karena itu tidak berlaku parity: endpoint audio panggilan
antrean. Hari ini hanya dilindungi `[Authorize]`, sehingga **setiap akun yang login** dapat
mengunduhnya. Setelah `BE-SEC-003` ia menuntut izin. Ini penyempitan yang disengaja, dan dampaknya
dihitung terpisah di bagian 8.

---

## 2. Metodologi dan pengaman query

Seluruh angka pada dokumen ini diambil langsung dari database development, bukan dari perkiraan.

| Pengaman | Cara |
| --- | --- |
| Transaksi read-only di sisi PostgreSQL | Setiap sesi membuka transaksi lalu menjalankan `SET TRANSACTION READ ONLY`. Server sendiri yang menolak penulisan, bukan hanya niat baik alat |
| Penyaring kata kunci | Alat menolak SQL yang memuat kata kunci mutasi sebelum dikirim ke server |
| Transaksi selalu dibatalkan | Setiap sesi diakhiri `ROLLBACK`, tidak pernah `COMMIT` |
| Rahasia tidak pernah dicetak | Connection string dibaca dari `appsettings.Development.json` dan tidak pernah ditampilkan maupun dicatat |
| Alat di luar repository | Alat query dibuat di direktori scratchpad, **bukan** di dalam repository backend. Tidak ada berkas repository yang bertambah |

Sepuluh query dijalankan: introspeksi skema, inventarisasi identitas, hitungan policy per identitas,
rincian Departemen × Posisi, metrik baseline, inventaris policy pilot, rekapitulasi 11 pasangan,
inventaris permission antrean, verifikasi baris registry inert, dan hitungan pengguna terdampak.

---

## 3. Baseline database development

| Metrik | Nilai | Catatan |
| --- | ---: | --- |
| `SysAccessPolicy` total | **498** | Cocok dengan laporan `BE-SEC-001` |
| `SysAccessPolicy` efektif (`IsAllowed` ∧ `IsActive` ∧ ¬`IsDelete`) | **469** | `BE-SEC-001` melaporkan 492 sebelum dedupe; 23 baris kembar ditutup pada langkah *safe rebind*, sehingga 492 − 23 = 469. Konsisten |
| Departemen × Posisi dengan izin efektif | **11** | Cocok dengan `BE-SEC-001` |
| `SysActionAccess` aktif | **1.076** | Cocok dengan log validator `KeyCount=1076` |
| `SysControllerAccess` aktif | **289** | |
| Penempatan organisasi sah saat ini | **49** | `IsActive` ∧ ¬`IsDelete` ∧ ¬`IsCancel` ∧ dalam masa berlaku ∧ user aktif |
| Pengguna berbeda dengan penempatan sah | **39** | |

### 3.1 Sebelas Departemen × Posisi beserta jumlah penggunanya

| Departemen | Posisi | Baris policy efektif | Pengguna aktif |
| --- | --- | ---: | ---: |
| Finance | Manajer Finance | 69 | 2 |
| Human Resource | Manajer HR | 71 | 13 |
| Keperawatan | Bidan | 4 | 1 |
| Keperawatan | Kepala Keperawatan | 8 | 1 |
| Keperawatan | Kepala Ruangan | 7 | 0 |
| Keperawatan | Perawat IGD | 7 | 2 |
| Keperawatan | Perawat Rawat Inap | 16 | 4 |
| Keperawatan | Perawat Rawat Jalan | 13 | 4 |
| Medis | Dokter IGD | 9 | 0 |
| Medis | Dokter Spesialis | 5 | 4 |
| **Medis** | **Dokter Umum** | **260** | **2** |

Perhatikan `Medis / Dokter Umum`: **260 dari 469** baris izin efektif — 55% seluruh hak sistem —
dipegang satu pasangan yang hanya berisi 2 pengguna. Ini bukan temuan `BE-SEC-003`, tetapi konteks
yang menjelaskan mengapa lapisan Business Permission dibutuhkan.

Jumlah pengguna pada tabel di atas berjumlah 33. Enam pengguna lain memiliki penempatan sah pada
pasangan Departemen × Posisi yang **tidak punya izin sama sekali**, sehingga total 39.

---

## 4. Hasil query — identitas yang akan dipecah

Ketujuh identitas ada, aktif, tidak terhapus, dan bukan `IsSystemOnly`.

| Resource | Action | `ActionAccessId` | `ControllerAccessId` | `AccessType` | Aktif |
| --- | --- | --- | --- | --- | --- |
| `DoctorConsultation` | `Update` | `977393b0-6c09-4e09-aa27-16ef941f1894` | `20de6551-100f-4153-ae3e-46c067cd96a8` | `Update` | ya |
| `DoctorQueue` | `Update` | `aa971275-c84f-442a-86c3-115bdca58420` | `116698d8-5bb2-4460-9397-8719a764beff` | `Update` | ya |
| `PatientAssessment` | `Update` | `db30defd-8830-483e-a7ad-ea015e6b1c44` | `33bd1c45-fe85-41ba-8909-c665be916222` | `Update` | ya |
| `PatientDiagnosis` | `Update` | `2664936c-345d-4cd0-8c4b-0aa6fadd5ee3` | `ac2ed507-351d-4070-86bb-e33be15533c7` | `Update` | ya |
| `PatientProcedure` | `Create` | `bff2a276-a322-4c2b-965f-4cc0160b5391` | `2ac5294d-d3cf-4d35-adb9-62d59a948001` | `Create` | ya |
| `PatientProcedure` | `Update` | `f1a98b1f-d94d-4d5a-8ace-13633315d2a4` | `2ac5294d-d3cf-4d35-adb9-62d59a948001` | `Update` | ya |
| `PatientVitalSign` | `Update` | `e1d265d2-33b3-42be-b0f3-4ed379bc987f` | `70b6ee07-de05-4777-8dce-e97d418593c1` | `Update` | ya |

### 4.1 Hitungan policy per identitas

| Resource | Action | Total baris | Efektif | Nonaktif | Terhapus | Tidak diizinkan | Dibatalkan | Dept × Posisi |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `DoctorConsultation` | `Update` | 1 | 1 | 0 | 0 | 0 | 0 | 1 |
| `DoctorQueue` | `Update` | **3** | **3** | 0 | 0 | 0 | 0 | **3** |
| `PatientAssessment` | `Update` | 1 | 1 | 0 | 0 | 0 | 0 | 1 |
| `PatientDiagnosis` | `Update` | 1 | 1 | 0 | 0 | 0 | 0 | 1 |
| `PatientProcedure` | `Create` | 1 | 1 | 0 | 0 | 0 | 0 | 1 |
| `PatientProcedure` | `Update` | 1 | 1 | 0 | 0 | 0 | 0 | 1 |
| `PatientVitalSign` | `Update` | 1 | 1 | 0 | 0 | 0 | 0 | 1 |
| **Total** | | **9** | **9** | **0** | **0** | **0** | **0** | |

Seluruh 9 baris berstatus efektif. Tidak ada baris nonaktif, terhapus, dibatalkan, maupun
`IsAllowed = false` yang perlu diperlakukan khusus. Ini menyederhanakan migrasi secara berarti.

---

## 5. Technical Permission Split Matrix

Kolom **Business operation** diturunkan dari perilaku endpoint yang benar-benar ada di source,
bukan dari nama methodnya.

### 5.1 `DoctorQueue.Update` → 6 identitas

| Endpoint yang dilindungi | Business operation | Identitas baru | Sensitif |
| --- | --- | --- | --- |
| `POST /doctor-queues/{id}/call` | Memanggil pasien ke ruang dokter | `DoctorQueue.Call` | tidak |
| `POST /doctor-queues/{id}/start-consultation` | Memulai konsultasi | `DoctorQueue.StartConsultation` | tidak |
| `POST /doctor-queues/{id}/finish-consultation` | Menutup episode pelayanan; memicu proses hilir | `DoctorQueue.FinishConsultation` | **ya** |
| `POST /doctor-queues/{id}/skip` | Melewati pasien yang tidak muncul | `DoctorQueue.Skip` | tidak |
| `POST /doctor-queues/{id}/no-show` | Menandai pasien tidak hadir | `DoctorQueue.NoShow` | tidak |
| `POST /doctor-queues/{id}/requeue` | Mengembalikan pasien ke antrean | `DoctorQueue.Requeue` | tidak |

### 5.2 `DoctorConsultation.Update` → 4 identitas

| Endpoint | Business operation | Identitas baru | Sensitif |
| --- | --- | --- | --- |
| `PUT /doctor-consultations/{id}` | Menyunting header konsultasi | `DoctorConsultation.Edit` | tidak |
| `PATCH /doctor-consultations/{id}/soap` | Menulis dan menyimpan otomatis SOAP | `DoctorConsultation.WriteSoap` | **ya** |
| `PATCH /doctor-consultations/{id}/complete` | **Transisi workflow** — memvalidasi dan menyelesaikan konsultasi | `DoctorConsultation.Complete` | **ya** |
| `PATCH /doctor-consultations/{id}/cancel` | Membatalkan konsultasi | `DoctorConsultation.Cancel` | **ya** |

Sesuai `D-ARCH-7`: `Complete` adalah transisi workflow, bukan CRUD update. Ia dipisah dari
`WriteSoap` walaupun keduanya hari ini memakai identitas yang sama.

### 5.3 `PatientProcedure.Update` → 5 identitas

| Endpoint | Business operation | Identitas baru | Sensitif |
| --- | --- | --- | --- |
| `PUT /patient-procedures/{id}` | Menyunting tindakan | `PatientProcedure.Edit` | tidak |
| `PATCH /patient-procedures/{id}/remove-draft` | Menghapus pilihan tindakan dari konsultasi draft | `PatientProcedure.RemoveDraft` | tidak |
| `PATCH /patient-procedures/{id}/approve` | **Menyetujui** tindakan; gerbang sebelum pelaksanaan | `PatientProcedure.Approve` | **ya** |
| `PATCH /patient-procedures/{id}/execute` | **Melaksanakan** tindakan; status menjadi `Completed` | `PatientProcedure.Execute` | **ya** |
| `PATCH /patient-procedures/{id}/cancel` | Membatalkan tindakan; **ditolak bila billing sudah terbit** | `PatientProcedure.Cancel` | **ya** |

Bukti dari source bahwa ketiganya kewenangan berbeda:

```csharp
// ExecuteProcedure — approve adalah gerbang sebelum execute
if (entity.IsNeedApproval && !entity.IsApproved) { return BadRequest(...); }

// CancelProcedure — cancel berkonsekuensi finansial
if (entity.IsBillingGenerated) {
    return BadRequest(... "Tindakan yang sudah masuk billing tidak dapat dibatalkan dari modul klinis.");
}
```

### 5.4 `PatientProcedure.Create` → 2 identitas

| Endpoint | Business operation | Identitas baru | Sensitif |
| --- | --- | --- | --- |
| `POST /patient-procedures/select` | Memilih tindakan ke dalam konsultasi draft | `PatientProcedure.Select` | tidak |
| `POST /patient-procedures` | Membuat tindakan penuh | `PatientProcedure.Create` | tidak |

Frontend pilot hanya memakai `/select`. Pemisahan ini mencegah pemberian kemampuan yang tidak
dipakai.

### 5.5 `PatientVitalSign.Update` → 4 identitas

| Endpoint | Business operation | Identitas baru | Sensitif |
| --- | --- | --- | --- |
| `PUT /patient-vital-signs/{id}` | Mengubah catatan tanda vital | `PatientVitalSign.Edit` | tidak |
| `PATCH /patient-vital-signs/{id}/verify` | **Verifikasi** catatan orang lain — kendali mutu | `PatientVitalSign.Verify` | **ya** |
| `PATCH /patient-vital-signs/{id}/notify-doctor` | Menandai dokter sudah diberi tahu | `PatientVitalSign.NotifyDoctor` | tidak |
| `PATCH /patient-vital-signs/{id}/cancel` | Membatalkan catatan | `PatientVitalSign.Cancel` | **ya** |

### 5.6 `PatientAssessment.Update` → 3 identitas

| Endpoint | Business operation | Identitas baru | Sensitif |
| --- | --- | --- | --- |
| `PUT /patient-assessments/{id}` | Mengubah pengkajian | `PatientAssessment.Edit` | tidak |
| `PATCH /patient-assessments/{id}/complete` | Menyelesaikan dokumen pengkajian | `PatientAssessment.Complete` | tidak |
| `PATCH /patient-assessments/{id}/cancel` | Membatalkan pengkajian | `PatientAssessment.Cancel` | **ya** |

### 5.7 `PatientDiagnosis.Update` → 4 identitas

| Endpoint | Business operation | Identitas baru | Sensitif |
| --- | --- | --- | --- |
| `PUT /patient-diagnoses/{id}` | Mengubah diagnosis | `PatientDiagnosis.Edit` | tidak |
| `PATCH /patient-diagnoses/{id}/set-primary` | Menandai diagnosis utama | `PatientDiagnosis.SetPrimary` | **ya** |
| `PATCH /patient-diagnoses/{id}/resolve` | Menyatakan diagnosis sudah teratasi | `PatientDiagnosis.Resolve` | **ya** |
| `PATCH /patient-diagnoses/{id}/cancel` | Membatalkan diagnosis | `PatientDiagnosis.Cancel` | **ya** |

### 5.8 Rekapitulasi

| Identitas lama | Endpoint | Identitas baru | Sensitif di dalamnya |
| --- | ---: | ---: | ---: |
| `DoctorQueue.Update` | 6 | 6 | 1 |
| `DoctorConsultation.Update` | 4 | 4 | 3 |
| `PatientProcedure.Update` | 5 | 5 | 3 |
| `PatientProcedure.Create` | 2 | 2 | 0 |
| `PatientVitalSign.Update` | 4 | 4 | 2 |
| `PatientAssessment.Update` | 3 | 3 | 1 |
| `PatientDiagnosis.Update` | 4 | 4 | 3 |
| **Total** | **28** | **28** | **13** |

Jumlah endpoint sama dengan jumlah identitas baru: **pemetaan satu-ke-satu**. Tidak ada endpoint
yang kehilangan perlindungan, dan tidak ada identitas baru yang tidak menunjuk endpoint nyata.

`AccessType` setiap identitas baru **tidak berubah** dari identitas lamanya, karena `AccessType`
adalah metadata kolom layar Akses Role, bukan identitas — aturan kanonik `BE-SEC-001` nomor 3 dan 4.

---

## 6. Department × Position Impact Matrix

Data aktual dari database development.

| Resource | Action | Departemen | Posisi | Baris policy | Pengguna aktif |
| --- | --- | --- | --- | ---: | ---: |
| `DoctorConsultation` | `Update` | Medis | Dokter Umum | 1 | 2 |
| `DoctorQueue` | `Update` | Medis | **Dokter IGD** | 1 | **0** |
| `DoctorQueue` | `Update` | Medis | **Dokter Spesialis** | 1 | **4** |
| `DoctorQueue` | `Update` | Medis | Dokter Umum | 1 | 2 |
| `PatientAssessment` | `Update` | Medis | Dokter Umum | 1 | 2 |
| `PatientDiagnosis` | `Update` | Medis | Dokter Umum | 1 | 2 |
| `PatientProcedure` | `Create` | Medis | Dokter Umum | 1 | 2 |
| `PatientProcedure` | `Update` | Medis | Dokter Umum | 1 | 2 |
| `PatientVitalSign` | `Update` | Medis | Dokter Umum | 1 | 2 |

`DepartmentId` `Medis` = `676f2aa7-8089-466b-b8a9-73adf5599626`.
`PositionId`: Dokter Umum = `cd1cd442-f971-a117-19c1-ae8809230138`;
Dokter Spesialis = `527a6805-8073-dbd8-92cc-fd4ae0b5acd7`;
Dokter IGD = `ae5bb7af-9e65-63ed-c22b-57212203e592`.

### 6.1 Tiga pasangan terdampak

| Departemen × Posisi | Identitas lama yang dipegang | Identitas baru yang akan diterima | Pengguna |
| --- | ---: | ---: | ---: |
| Medis × Dokter Umum | 7 | **28** | 2 |
| Medis × Dokter Spesialis | 1 (`DoctorQueue.Update`) | **6** | 4 |
| Medis × Dokter IGD | 1 (`DoctorQueue.Update`) | **6** | 0 |
| **Total baris** | **9** | **40** | |

Delapan pasangan lain — Finance, Human Resource, dan seluruh Keperawatan — **tidak memegang satu
pun** identitas yang dipecah, sehingga tidak terdampak sama sekali.

---

## 7. User Impact

| Ukuran | Nilai |
| --- | ---: |
| Pengguna aktif berbeda yang terdampak pemecahan | **6** |
| — lewat Medis × Dokter Umum | 2 |
| — lewat Medis × Dokter Spesialis | 4 |
| — lewat Medis × Dokter IGD | 0 |
| Pengguna aktif berbeda di seluruh sistem | 39 |
| Persentase terdampak | **15%** |
| Pengguna yang kehilangan kemampuan | **0** |
| Pengguna yang memperoleh kemampuan baru | **0** |

Angka 6 adalah hitungan **berbeda**: seandainya satu orang menempati Dokter Umum sekaligus Dokter
Spesialis, ia hanya dihitung sekali. Query memakai `COUNT(DISTINCT "UserId")` atas penempatan yang
sah saat ini.

---

## 8. Queue Audio — rekomendasi implementasi otorisasi

### 8.1 Audit semantik OR pada ASP.NET Core

Pemilik sistem meminta audit implementasi sebelum coding, dengan peringatan tegas: *"Jangan sekadar
memasang dua `[Authorize]` attribute jika runtime-nya menghasilkan AND semantics."*

**Peringatan itu benar.** Hasil audit:

| Mekanisme | Semantik | Bukti |
| --- | --- | --- |
| Dua `[Authorize(Policy = ...)]` pada satu action | **AND** — seluruhnya harus lulus | Perilaku baku ASP.NET Core: setiap policy dievaluasi terpisah dan semuanya harus berhasil |
| `[Authorize(Policy = X)]` + `[AccessPermission(...)]` | **AND** | `AccessPermissionAttribute` adalah `TypeFilterAttribute` yang menjalankan `AccessPermissionFilter : IAsyncAuthorizationFilter`. Filter itu menetapkan `context.Result = 403` saat gagal, memotong pipeline tanpa peduli hasil policy lain |
| Dua `[AccessPermission(...)]` pada satu action | **Tidak mungkin dikompilasi** | `Attributes/AccessPermissionAttribute.cs` memakai `[AttributeUsage(AttributeTargets.Method)]` **tanpa** `AllowMultiple = true` |
| Beberapa requirement di dalam satu policy | **AND** | Perilaku baku `AuthorizationPolicyBuilder` |
| Beberapa handler untuk **satu** requirement | **OR** | Requirement dianggap lulus bila **ada** handler yang memanggil `context.Succeed(requirement)` |
| Satu `RequireAssertion` dengan `\|\|` di dalamnya | **OR** | Satu requirement, satu assertion |

Jadi tiga cara sah menghasilkan OR: satu `RequireAssertion`, satu requirement dengan dua handler,
atau satu filter yang melakukan OR di dalamnya.

### 8.2 Rekomendasi

> **Satu atribut filter baru yang melakukan OR di dalamnya, memakai kembali policy dan service yang
> sudah ada.**

Bentuk konseptualnya:

```csharp
// Attributes/QueueVoicePlaybackAttribute.cs  — mengikuti pola AccessPermissionAttribute
[AttributeUsage(AttributeTargets.Method)]
public class QueueVoicePlaybackAttribute : TypeFilterAttribute
{
    public QueueVoicePlaybackAttribute() : base(typeof(QueueVoicePlaybackFilter)) { }
}

// Filters/QueueVoicePlaybackFilter.cs — mengikuti pola AccessPermissionFilter
public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
{
    if (!authenticated) { context.Result = 401; return; }

    // Jalur perangkat display: memakai kembali definisi policy yang sudah ada di Program.cs
    var byDevice = await _authorizationService.AuthorizeAsync(user, "QueueDisplayRuntimeRead");
    if (byDevice.Succeeded) return;

    // Jalur petugas: memakai kembali AccessPermissionService yang sudah ada
    if (await _accessPermissionService.HasAccessAsync(user, "QueueVoice", "PlayAudio")) return;

    context.Result = 403;
}
```

Mengapa bentuk ini yang dipilih:

| Kriteria | Penilaian |
| --- | --- |
| Semantik | **OR sejati.** Satu filter, satu keputusan |
| Mengikuti pola repository | Ya — persis pola `AccessPermissionAttribute` → `AccessPermissionFilter` yang sudah ada |
| Duplikasi logika claim | **Nol.** Definisi `QueueDisplayRuntimeRead` di `Program.cs:571` dipakai kembali lewat `IAuthorizationService`, tidak disalin |
| Perubahan pada `AccessPermissionService` | **Nol** |
| Perubahan pada `AccessPermissionFilter` | **Nol** |
| Perubahan pada `PermissionRegistryDescriptor` | **Nol** — lihat 8.3 |
| Arsitektur baru | Tidak ada. Satu atribut dan satu filter, keduanya mengikuti pola terdekat |

### 8.3 Bagaimana `QueueVoice.PlayAudio` terdaftar tanpa mengubah descriptor

Ini bagian yang paling mudah salah, dan audit source menunjukkan jalannya sudah tersedia.

`PermissionRegistryDescriptor.BuildCore` memperlakukan endpoint ber-`[AccessAction]` **tanpa**
`[AccessPermission]` sebagai jalur kompatibilitas: ia tetap didaftarkan memakai **nama controller**
sebagai resource dan argumen pertama `[AccessAction]` sebagai nama aksi.

Kedua endpoint audio hari ini memakai `[AccessAction("Read", ...)]`, sehingga terdaftar sebagai
`QueueVoice.Read`. Dengan mengubah argumen pertamanya menjadi `"PlayAudio"`:

```csharp
[AccessAction("PlayAudio", "Play Queue Voice Audio",
    Description = "Memutar audio panggilan antrean",
    AccessType = AccessTypes.Read, SortOrder = 1)]
```

identitas `QueueVoice.PlayAudio` otomatis terdaftar dan **muncul di layar Akses Role** sehingga
admin dapat memberikannya. `AccessType` tetap `Read`, memenuhi aturan empat kolom.

Tidak ada `[AccessPermission]` yang ditambahkan pada kedua endpoint itu — justru itulah yang
menghindari AND.

### 8.4 Dampak pada test terkunci

`CanonicalSecurityContractTests.CompatibilityFallbackMatchesApprovedLegacySetExactly` mengunci
**himpunan persis** 69 endpoint warisan. Dua endpoint audio ada di dalamnya, pada kelompok
*"Audio panggilan antrean | 2 | `[Authorize]` saja"*.

Sesudah perubahan, keduanya **tetap** di himpunan fallback (masih tanpa `[AccessPermission]`),
tetapi nama aksinya berubah dari `Read` menjadi `PlayAudio`, dan keterangan kelompoknya berubah
dari *"`[Authorize]` saja"* menjadi *"`QueueVoice.PlayAudio` OR `QueueDisplayRuntimeRead`"*.

Jumlahnya tetap **69**. Yang wajib diperbarui secara sadar adalah isi himpunan dan komentar
klasifikasinya, bukan jumlahnya. Koreksi atas perkiraan sebelumnya di `evidence/02` bagian I.5 yang
menduga 69 → 67: dugaan itu keliru karena mengasumsikan endpoint akan memakai `[AccessPermission]`.

### 8.5 Dampak penyempitan — inilah yang perlu keputusan

Hari ini kedua endpoint hanya `[Authorize]`. Artinya **seluruh 39 pengguna aktif** — dan setiap
akun lain yang dapat login, termasuk akun kiosk — dapat mengunduh berkas audio bila mengetahui
`dateKey` dan `fileName`. Berkas itu memuat nama pasien yang diumumkan.

Sesudah perubahan, hanya yang memegang `QueueVoice.PlayAudio` atau memenuhi
`QueueDisplayRuntimeRead` yang dapat memutarnya.

**Siapa yang memegang izin `QueueVoice` hari ini:**

| Departemen × Posisi | `QueueVoice.*` | Pengguna aktif |
| --- | --- | ---: |
| Medis × Dokter Umum | `Read`, `Create`, `Update` | 2 |
| Medis × Dokter IGD | `Read`, `Create`, `Update` | 0 |
| **Sembilan pasangan lain** | **tidak ada** | **31** |

Bila `QueueVoice.PlayAudio` hanya diberikan kepada yang sudah memegang `QueueVoice`, maka **37 dari
39 pengguna kehilangan kemampuan memutar audio** — termasuk seluruh perawat dan Dokter Spesialis
yang justru memanggil pasien setiap hari.

**Siapa yang benar-benar memanggil pasien** — diturunkan dari izin antrean yang mereka pegang:

| Departemen × Posisi | Izin antrean yang dipegang | Pengguna | Punya `QueueVoice`? |
| --- | --- | ---: | --- |
| Medis × Dokter Umum | `DoctorQueue.*`, `NurseStationQueue.*`, `QueueVoice.*` | 2 | **ya** |
| Medis × Dokter IGD | `DoctorQueue.*`, `QueueVoice.*` | 0 | **ya** |
| Medis × Dokter Spesialis | `DoctorQueue.*` | 4 | tidak |
| Keperawatan × Perawat Rawat Jalan | `NurseStationQueue.*`, `Queue.*` | 4 | tidak |
| Keperawatan × Perawat Rawat Inap | `NurseStationQueue.*` | 4 | tidak |
| Keperawatan × Perawat IGD | `NurseStationQueue.*` | 2 | tidak |
| Keperawatan × Kepala Keperawatan | `NurseStationQueue.*` | 1 | tidak |
| Keperawatan × Kepala Ruangan | `NurseStationQueue.*` | 0 | tidak |
| **Total** | | **17** | |

> **Daftar penerima `QueueVoice.PlayAudio`: delapan pasangan di atas, mencakup 17 pengguna aktif.**
> **Status: `APPROVED` pemilik sistem 2 September 2026** — lihat bagian 14.1.
>
> Akibatnya 22 pengguna lain — Manajer Finance (2), Manajer HR (13), Bidan (1), dan 6 pengguna pada
> pasangan tanpa izin — kehilangan kemampuan mengunduh rekaman nama pasien. **Itu memang tujuan
> perubahan ini**, bukan efek samping.

Karena ini penyempitan, ia **tidak tunduk pada jaminan parity** di bagian 10. Ia dijalankan sebagai
langkah 9 yang terpisah dan tercatat, di luar verifikasi parity langkah 6.

### 8.6 Pemisahan actor sesuai `D-ARCH-8`

| Actor | Kemampuan | Jalur otorisasi | Butuh Departemen + Posisi? |
| --- | --- | --- | --- |
| Dokter | Memanggil pasien | `DoctorQueue.Call` | ya |
| Perawat | Memanggil pasien | Identitas antrean perawat — di luar scope pilot | ya |
| Dokter dan perawat | Memutar berkas audio | `QueueVoice.PlayAudio` | ya |
| Perangkat display antrean | Memutar berkas audio | Policy `QueueDisplayRuntimeRead` | **tidak** |

Perangkat display tetap tidak membutuhkan Departemen + Posisi, sesuai instruksi.

---

## 9. Berkas yang Diperkirakan Berubah

### 9.1 Backend — source

| Berkas | Perubahan | Sifat |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs` | 6 `[AccessPermission]` diganti nama aksinya | Ubah |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | 4 `[AccessPermission]` | Ubah |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | 7 `[AccessPermission]` (5 `Update` + 2 `Create`) | Ubah |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` | 4 `[AccessPermission]` | Ubah |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | 3 `[AccessPermission]` | Ubah |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientDiagnosisController.cs` | 4 `[AccessPermission]` | Ubah |
| `Areas/HealthServices/RegistrationManagement/Controllers/QueueVoiceController.cs` | 2 `[AccessAction]` menjadi `PlayAudio`; 2 `[QueueVoicePlayback]` ditambahkan | Ubah |
| `Attributes/QueueVoicePlaybackAttribute.cs` | Atribut OR | **Baru** |
| `Filters/QueueVoicePlaybackFilter.cs` | Filter OR | **Baru** |

**Tidak disentuh, dan ini disengaja:** `Services/Security/AccessPermissionService.cs`,
`Services/Security/PermissionRegistryDescriptor.cs`, `Services/Security/PermissionRegistryValidator.cs`,
`Seeders/AccessMenuSeeder.cs`, `Filters/AccessPermissionFilter.cs`, `Program.cs`, seluruh model,
seluruh konfigurasi EF, dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.

> Registry prefix **tidak** diperbarui pada `BE-SEC-003`, sesuai instruksi pemilik sistem: task ini
> tidak membuat satu pun model persisted `Sec*`. Pembaruan registry adalah langkah pertama
> `BE-SEC-004`.

### 9.2 Backend — test

| Berkas | Perubahan |
| --- | --- |
| `QuilvianSystemBackend.Tests/Security/PermissionRegistryInvariantTests.cs` | Jumlah kunci registry bertambah 21 bersih (28 baru − 7 lama yang ditutup); invarian identitas ganda diperiksa ulang |
| `QuilvianSystemBackend.Tests/Security/CanonicalSecurityContractTests.cs` | Himpunan 69 endpoint fallback diperbarui: 2 baris audio berganti nama aksi dan klasifikasi |
| `QuilvianSystemBackend.Tests/Security/StaleRegistryAuthorizationTests.cs` | Ditambah kasus: identitas lama yang ditutup berhenti mengotorisasi |
| `QuilvianSystemBackend.Tests/Security/PermissionSplitParityTests.cs` | **Baru** — pembuktian parity dan pemetaan satu-ke-satu |
| `QuilvianSystemBackend.Tests/Security/QueueVoicePlaybackAuthorizationTests.cs` | **Baru** — pembuktian semantik OR |

### 9.3 Skrip migrasi data

| Berkas | Isi |
| --- | --- |
| `tooling/` atau direktori skrip yang disetujui — **bukan** `Migrations/` | Skrip perluasan `SysAccessPolicy`, mode laporan dan mode tulis, beserta skrip baliknya |

Ditempatkan di luar `Migrations/` dengan sengaja: ini **bukan** perubahan skema EF. Menaruhnya
sebagai EF migration akan membuat `has-pending-model-changes` dan riwayat migration menjadi
menyesatkan.

### 9.4 Frontend

**Tidak ada.** Frontend tidak pernah membaca nama technical permission (`evidence/01` bagian C.5),
sehingga pemecahan ini tidak terlihat olehnya.

### 9.5 Dokumentasi

| Berkas | Perubahan |
| --- | --- |
| `docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-003.md` | **Baru** — laporan tracked sesudah implementasi dan validasi |
| `roadmap/backend-roadmap.md` | Status `BE-SEC-003` diperbarui |
| `roadmap/requirement-traceability.md` | Bukti `SEC-REQ-013`, `SEC-REQ-014`, `SEC-REQ-021` |

---

## 10. Rancangan Migrasi Data

### 10.1 Bahaya urutan yang harus dipahami lebih dulu

`AccessMenuSeeder` melakukan rekonsiliasi generik: **baris registry yang tidak lagi dideklarasikan
source akan ditutup otomatis** saat aplikasi menyala (`CloseRowsAbsentFromSourceAsync`, dibuat
`BE-SEC-001`).

Akibatnya, begitu source yang sudah dipecah dijalankan:

1. 28 identitas baru terdaftar;
2. 7 identitas lama **otomatis ditutup**;
3. 9 baris `SysAccessPolicy` yang menunjuknya **seketika menjadi tidak berlaku**;
4. 6 pengguna kehilangan kemampuan sampai perluasan dijalankan.

> **Menjalankan skrip perluasan bukan langkah opsional yang bisa menyusul keesokan harinya.** Ia
> harus berada dalam jendela pemeliharaan yang sama dengan penyalaan aplikasi.

### 10.2 Urutan yang diusulkan

| # | Langkah | Menulis? | Pemeriksaan |
| ---: | --- | --- | --- |
| 0 | Snapshot `SysAccessPolicy`, `SysActionAccess`, `SysControllerAccess` | tidak | Jumlah baris dicatat |
| 1 | Jalankan skrip perluasan dalam **mode laporan** terhadap database saat ini | tidak | Hasilnya harus sama persis dengan bagian 5, 6, dan 7 dokumen ini |
| 2 | Tinjauan manusia atas laporan langkah 1 | — | Enam syarat bagian 10.3 terpenuhi |
| 3 | Mulai jendela pemeliharaan | — | |
| 4 | Deploy source; aplikasi menyala; seeder mendaftarkan 28 identitas dan menutup 7 identitas lama | **ya** | Log validator: `Permission registry valid` |
| 5 | Jalankan skrip perluasan dalam **mode tulis** | **ya** | 9 baris → 40 baris |
| 6 | Verifikasi parity | tidak | Selisih nol di kedua arah, per Departemen × Posisi |
| 7 | Smoke test akun non-SuperAdmin | tidak | Kemampuan sebelum = sesudah |
| 8 | Tutup jendela pemeliharaan | — | |
| 9 | **Terpisah dan tercatat**: pemberian `QueueVoice.PlayAudio` kepada delapan pasangan | **ya** | Bukan bagian parity; lihat bagian 8.5 |

Langkah 9 sengaja dipisah karena ia satu-satunya langkah yang **mengubah** kemampuan. Menggabungkannya
dengan langkah 5 akan membuat verifikasi parity mustahil dibaca.

### 10.3 Enam syarat parity — pemetaan ke bukti

Pemilik sistem menetapkan enam syarat. Berikut cara masing-masing dibuktikan:

| # | Syarat | Cara dibuktikan |
| ---: | --- | --- |
| 1 | Permission baru berasal dari endpoint yang memang dilindungi permission lama | Refleksi assembly: daftar endpoint per identitas lama diambil dari `PermissionRegistryDescriptor`, bukan dari penalaran. Bagian 5 adalah hasilnya |
| 2 | Mempertahankan *exact historical endpoint capability set* | Test parity: himpunan endpoint per Departemen × Posisi dibandingkan sebelum dan sesudah |
| 3 | Tidak ada capability baru yang sebelumnya tidak tercakup | Jumlah endpoint = jumlah identitas baru = 28. Pemetaan satu-ke-satu, tidak ada identitas tanpa endpoint |
| 4 | Departemen + Posisi tidak berubah | Skrip menyalin `DepartmentId` dan `PositionId` apa adanya; penyaring `WHERE` menyertakan keduanya |
| 5 | Tidak ada Departemen + Posisi baru | Hitungan pasangan berbeda sebelum dan sesudah harus tetap **11** |
| 6 | Tidak ada perluasan satu-ke-banyak berdasarkan tebakan bisnis | Perluasan diturunkan dari daftar endpoint hasil refleksi. Tidak ada aturan bisnis, tidak ada pencocokan nama, tidak ada heuristik |

### 10.4 Hasil yang diharapkan

| Ukuran | Sebelum | Sesudah | Selisih |
| --- | ---: | ---: | ---: |
| `SysAccessPolicy` total | 498 | **529** | +31 |
| `SysAccessPolicy` efektif | 469 | **500** | +31 |
| Departemen × Posisi dengan izin efektif | 11 | **11** | **0** |
| `SysActionAccess` aktif | 1.076 | **1.097** | +21 |
| Endpoint terjangkau per Departemen × Posisi | — | — | **0** |

Baris policy bertambah, tetapi **kemampuan tidak**. Satu izin lama yang membuka 5 endpoint kini
menjadi 5 izin yang masing-masing membuka 1 endpoint. Jumlah endpoint yang dapat dijangkau identik.

Rincian perluasan:

| Identitas lama | Baris sekarang | × identitas baru | Baris sesudah |
| --- | ---: | ---: | ---: |
| `DoctorQueue.Update` | 3 | 6 | 18 |
| `DoctorConsultation.Update` | 1 | 4 | 4 |
| `PatientProcedure.Update` | 1 | 5 | 5 |
| `PatientProcedure.Create` | 1 | 2 | 2 |
| `PatientVitalSign.Update` | 1 | 4 | 4 |
| `PatientAssessment.Update` | 1 | 3 | 3 |
| `PatientDiagnosis.Update` | 1 | 4 | 4 |
| **Total** | **9** | | **40** |

---

## 11. Legacy Parity Matrix

Ini bentuk yang wajib dihasilkan skrip pada langkah 1 dan diverifikasi pada langkah 6. Isinya
diturunkan dari data nyata database development.

### 11.1 Medis × Dokter Umum — 2 pengguna

| Identitas lama | Endpoint terjangkau sebelum | Identitas baru yang diterima | Endpoint terjangkau sesudah | Parity |
| --- | ---: | --- | ---: | --- |
| `DoctorQueue.Update` | 6 | `Call`, `StartConsultation`, `FinishConsultation`, `Skip`, `NoShow`, `Requeue` | 6 | **sama** |
| `DoctorConsultation.Update` | 4 | `Edit`, `WriteSoap`, `Complete`, `Cancel` | 4 | **sama** |
| `PatientProcedure.Update` | 5 | `Edit`, `RemoveDraft`, `Approve`, `Execute`, `Cancel` | 5 | **sama** |
| `PatientProcedure.Create` | 2 | `Select`, `Create` | 2 | **sama** |
| `PatientVitalSign.Update` | 4 | `Edit`, `Verify`, `NotifyDoctor`, `Cancel` | 4 | **sama** |
| `PatientAssessment.Update` | 3 | `Edit`, `Complete`, `Cancel` | 3 | **sama** |
| `PatientDiagnosis.Update` | 4 | `Edit`, `SetPrimary`, `Resolve`, `Cancel` | 4 | **sama** |
| **Total** | **28** | **28 identitas** | **28** | **sama** |

### 11.2 Medis × Dokter Spesialis — 4 pengguna

| Identitas lama | Endpoint sebelum | Identitas baru | Endpoint sesudah | Parity |
| --- | ---: | --- | ---: | --- |
| `DoctorQueue.Update` | 6 | `Call`, `StartConsultation`, `FinishConsultation`, `Skip`, `NoShow`, `Requeue` | 6 | **sama** |

### 11.3 Medis × Dokter IGD — 0 pengguna

| Identitas lama | Endpoint sebelum | Identitas baru | Endpoint sesudah | Parity |
| --- | ---: | --- | ---: | --- |
| `DoctorQueue.Update` | 6 | 6 identitas yang sama seperti di atas | 6 | **sama** |

### 11.4 Delapan pasangan lain

Tidak memegang satu pun identitas yang dipecah. Sebelum dan sesudah: **tidak ada perubahan**.

### 11.5 Ini pelestarian, bukan pemberian baru

Perlu dinyatakan tegas, karena angkanya mudah disalahbaca:

> Medis × Dokter Umum menerima `PatientProcedure.Approve` dan `PatientProcedure.Execute` **bukan
> karena diberi hak baru**, melainkan karena kedua endpoint itu **memang sudah dapat mereka akses
> hari ini** lewat `PatientProcedure.Update`. Baris policy bertambah dari 1 menjadi 5; kemampuannya
> tetap 5 endpoint yang sama.

Penyempitan — misalnya mencabut `Approve` dari dokter — adalah tindakan pemilik sistem **sesudah**
`BE-SEC-003` selesai, tercatat sendiri, dan dapat dikembalikan. Ia sengaja **tidak** diselipkan ke
dalam migrasi ini, sesuai instruksi.

---

## 12. Rancangan Rollback

### 12.1 Batas rollback

`BE-SEC-003` mandiri. Tidak ada task lain yang bergantung padanya saat ia dijalankan, dan frontend
tidak terpengaruh sama sekali.

### 12.2 Prosedur

| # | Langkah | Isi |
| ---: | --- | --- |
| 1 | Kembalikan source | Balikkan 9 berkas backend ke identitas lama; cabut atribut dan filter baru |
| 2 | Nyalakan ulang aplikasi | Seeder mendaftarkan ulang 7 identitas lama dan menutup 28 identitas baru |
| 3 | Jalankan skrip balik | Menciutkan 40 baris `SysAccessPolicy` menjadi 9, memakai `DepartmentId` dan `PositionId` yang sama |
| 4 | Verifikasi | Total baris kembali **498**; efektif **469**; pasangan Departemen × Posisi tetap **11** |
| 5 | Bila skrip balik gagal | Pulihkan `SysAccessPolicy` dari snapshot langkah 0 |

### 12.3 Titik tanpa jalan kembali

**Tidak ada.** Tidak ada kolom dihapus, tidak ada tipe berubah, tidak ada baris di-hard-delete.
Penutupan identitas bersifat penandaan (`IsActive = false`), sehingga dapat dibuka kembali.

### 12.4 Bila langkah 9 (pemberian `QueueVoice.PlayAudio`) sudah dijalankan

Rollback-nya terpisah dan sepele: nonaktifkan baris policy `QueueVoice.PlayAudio` yang baru dibuat.
Karena endpoint audio kembali ke `[Authorize]` saja setelah source dibalikkan, tidak ada yang
kehilangan akses.

---

## 13. Rencana Test Keamanan

### 13.1 Parity — inti pembuktian task ini

| Test | Yang dibuktikan |
| --- | --- |
| `EveryNewIdentityMapsToExactlyOneEndpoint` | 28 identitas ↔ 28 endpoint, satu-ke-satu |
| `EveryNewIdentityDerivesFromRetiredIdentityEndpointSet` | Setiap identitas baru berasal dari endpoint yang memang dijaga identitas lama. Syarat parity 1 dan 3 |
| `EffectiveEndpointSetIsIdenticalBeforeAndAfterPerDepartmentPosition` | Syarat parity 2. Selisih nol di kedua arah |
| `DepartmentAndPositionAreNeverAlteredByExpansion` | Syarat parity 4 |
| `DistinctDepartmentPositionCountStaysEleven` | Syarat parity 5 |
| `ExpansionUsesReflectionNotBusinessHeuristics` | Syarat parity 6 — sumber perluasan adalah `PermissionRegistryDescriptor` |

### 13.2 Invarian registry

| Test | Yang dibuktikan |
| --- | --- |
| `EveryProtectedEndpointIsRegisterableInRoleAccess` | Invarian `BE-SEC-001` tetap berlaku |
| `AuthorizationIdentityAlwaysComesFromAccessPermission` | Kontrak kanonik tidak dilanggar |
| `AccessTypeStaysWithinFourColumns` | Seluruh identitas baru tetap `Read`/`Create`/`Update`/`Delete` |
| `RetiredIdentitiesAreClosedNotHardDeleted` | Penutupan bersifat penandaan |
| `StaleRegistryAuthorizationTests` | Identitas lama yang ditutup berhenti mengotorisasi |
| `ReconcileNeverCreatesAccessPolicy` | **Tetap hijau** — perluasan dilakukan skrip berwenang, bukan seeder |

### 13.3 Semantik OR audio antrean

| Test | Yang dibuktikan |
| --- | --- |
| `DisplayDeviceCanPlayAudioWithoutTechnicalPermission` | Jalur perangkat lolos tanpa Departemen + Posisi |
| `StaffWithPlayAudioPermissionCanPlayAudio` | Jalur petugas lolos |
| `StaffWithoutPlayAudioPermissionIsDenied` | Yang tidak berhak ditolak `403` |
| `AuthorizationIsOrNotAnd` | Memenuhi **salah satu** jalur sudah cukup — pembuktian eksplisit atas peringatan pemilik sistem |
| `UnauthenticatedRequestIsRejected` | `401`, bukan `403` |
| `QueueVoicePlayAudioAppearsInRoleAccessRegistry` | Admin benar-benar dapat memberikannya |

### 13.4 Regresi

| Test | Yang dibuktikan |
| --- | --- |
| Tiga test SuperAdmin existing | Perilaku SuperAdmin tidak berubah |
| 12 test kontrak terkunci (`opr-permission-v1`, Billing) | Tidak terdampak |
| `CompatibilityFallbackMatchesApprovedLegacySetExactly` | Himpunan 69 endpoint diperbarui secara sadar, bukan longgar |
| Seluruh suite `dotnet test` | 856 test baseline tetap lulus, ditambah test baru |

### 13.5 Verifikasi manual pada database development

Mengikuti pola smoke test `BE-SEC-001`, dijalankan dengan akun nyata non-SuperAdmin:

1. Dokter Umum dapat memanggil, memulai, dan menyelesaikan konsultasi seperti sebelumnya.
2. Dokter Umum masih dapat menghapus pilihan tindakan dari draft.
3. Dokter Umum masih dapat menyetujui dan melaksanakan tindakan — **parity, bukan hak baru**.
4. Dokter Spesialis masih dapat memakai keenam aksi antrean.
5. Perawat Rawat Jalan tidak memperoleh satu pun kemampuan baru.
6. Manajer Finance tetap tidak dapat menyentuh endpoint klinis mana pun.
7. Sesudah langkah 9: perawat dapat memutar audio; Manajer Finance tidak.

---

## 14. Keputusan Owner — Status Akhir

### 14.1 `O-1` · Otorisasi audio antrean — **APPROVED**

Diputuskan pemilik sistem 2 September 2026.

| Field | Isi |
| --- | --- |
| **Status** | **`APPROVED`** |
| Mekanisme | Dual authorization dengan semantik **OR**: `QueueVoice.PlayAudio` **ATAU** `QueueDisplayRuntimeRead` |
| Actor manusia | Dokter dan perawat → `QueueVoice.PlayAudio` |
| Actor perangkat | Perangkat display antrean → policy `QueueDisplayRuntimeRead` yang sudah ada |
| Larangan 1 | `AllowAnonymous` **tidak boleh** dipakai |
| Larangan 2 | `QueueDisplayRuntimeRead` **tidak boleh** dijadikan permission bagi user dokter maupun perawat |
| Larangan 3 | Implementasi OR wajib memakai **satu** mekanisme otorisasi yang memang menghasilkan OR. Dua atribut yang menghasilkan AND dilarang |

Implementasi yang memenuhi ketiga larangan sudah diaudit dan diuraikan pada bagian 8.2 dan 8.3:
satu `QueueVoicePlaybackAttribute` → `QueueVoicePlaybackFilter` yang melakukan OR di dalam dirinya,
memakai kembali policy `QueueDisplayRuntimeRead` lewat `IAuthorizationService` dan
`AccessPermissionService` yang sudah ada.

### 14.1.1 Penerjemahan "Dokter dan Perawat" menjadi Departemen × Posisi

Keputusan menyebut actor sebagai peran bisnis. Penerjemahannya ke pasangan Departemen × Posisi
diturunkan dari bukti: **pasangan yang benar-benar memegang izin antrean**, yaitu yang memang
memanggil pasien dan karena itu memutar audio.

| # | Departemen | Posisi | Izin antrean yang dipegang | Pengguna aktif | Kelompok actor |
| ---: | --- | --- | --- | ---: | --- |
| 1 | Medis | Dokter Umum | `DoctorQueue.*`, `NurseStationQueue.*`, `QueueVoice.*` | 2 | Dokter |
| 2 | Medis | Dokter Spesialis | `DoctorQueue.*` | 4 | Dokter |
| 3 | Medis | Dokter IGD | `DoctorQueue.*`, `QueueVoice.*` | 0 | Dokter |
| 4 | Keperawatan | Perawat Rawat Jalan | `NurseStationQueue.*`, `Queue.*` | 4 | Perawat |
| 5 | Keperawatan | Perawat Rawat Inap | `NurseStationQueue.*` | 4 | Perawat |
| 6 | Keperawatan | Perawat IGD | `NurseStationQueue.*` | 2 | Perawat |
| 7 | Keperawatan | Kepala Keperawatan | `NurseStationQueue.*` | 1 | Perawat |
| 8 | Keperawatan | Kepala Ruangan | `NurseStationQueue.*` | 0 | Perawat |
| | | | **Total** | **17** | |

**Satu pasangan sengaja dikecualikan, dengan alasan yang dapat diperiksa:**

| Departemen × Posisi | Pengguna | Alasan dikecualikan |
| --- | ---: | --- |
| Keperawatan × Bidan | 1 | **Tidak memegang satu pun izin antrean.** Ia tidak memanggil pasien, sehingga tidak memutar audio. Memasukkannya berarti memberi kemampuan yang tidak ia pakai |

Bila pemilik sistem menganggap Bidan termasuk actor perawat yang memanggil pasien, penambahannya
adalah satu baris dan tidak mengubah rancangan apa pun. Keputusan itu **tidak menahan**
`BE-SEC-003`: pemberian izin adalah langkah 9 yang terpisah dan tercatat.

### 14.1.2 Dampak yang diterima

| Ukuran | Nilai |
| --- | ---: |
| Pengguna yang **tetap** dapat memutar audio | **17** |
| Pengguna yang **kehilangan** kemampuan itu | **22** |
| — Manajer HR | 13 |
| — Manajer Finance | 2 |
| — Bidan | 1 |
| — pengguna pada pasangan tanpa izin apa pun | 6 |
| Perangkat display antrean | tidak terpengaruh — lewat `QueueDisplayRuntimeRead` |

Ke-22 pengguna itu hari ini dapat mengunduh rekaman nama pasien hanya karena mereka bisa login.
Menghentikannya adalah tujuan perubahan, bukan efek samping.

### 14.2 Wewenang operasional yang dibutuhkan

**O-2 · Wewenang menjalankan migrasi data pada database development**

Pemilik sistem sudah memberi `CONDITIONALLY APPROVED`: migrasi boleh dijalankan **apabila dry-run
membuktikan enam syarat parity**.

Dokumen ini adalah dry-run **berbasis source dan data**, bukan dry-run eksekusi skrip. Skripnya
belum ada, karena menulisnya adalah implementasi. Urutannya:

1. `BE-SEC-003` dimulai;
2. skrip perluasan ditulis dalam mode laporan;
3. laporannya dijalankan dan dibandingkan dengan bagian 5, 6, 7, dan 11 dokumen ini;
4. bila cocok, enam syarat terpenuhi dan penulisan boleh dilakukan;
5. bila tidak cocok, **berhenti** dan laporkan selisihnya.

Tidak ada wewenang tambahan yang perlu diminta bila alurnya persis seperti di atas.

### 14.3 Yang tidak menahan `BE-SEC-003`

| Butir | Mengapa tidak menahan |
| --- | --- |
| Siapa boleh `approve` tindakan | Pemecahan mempertahankan hak apa adanya. Penyempitan adalah tindakan terpisah sesudahnya |
| Siapa boleh `execute` tindakan | Sama |
| Siapa menandatangani surat dokter | `MedicalCertificate` dikeluarkan dari scope |
| Registry prefix `Sec` | Sudah disetujui; diperbarui pada `BE-SEC-004`, bukan task ini |
| Frontend authority | Frontend tidak terpengaruh pemecahan ini |
| Nama akhir entity `Sec*` | Baru relevan pada `BE-SEC-004` |

---

## 15. Rekomendasi

> ## `READY_TO_IMPLEMENT_BE_SEC_003`
>
> **Tanpa syarat.** `O-1` sudah dijawab pemilik sistem 2 September 2026, dan `O-2` sudah
> `CONDITIONALLY APPROVED` dengan alur yang cocok dengan rancangan bagian 10.

### 15.1 Alasan

| Kriteria kesiapan | Status |
| --- | --- |
| Scope terkunci dan terbatas pada pilot | **Ya** — 6 controller, 7 identitas, tanpa pemecahan platform-wide |
| Identitas baru ditetapkan dari semantik endpoint nyata | **Ya** — bagian 5, seluruhnya berbukti source |
| Dampak diukur dari data nyata, bukan perkiraan | **Ya** — bagian 3, 4, 6, 7 dari database development |
| Legacy parity dapat dibuktikan | **Ya** — bagian 11, satu-ke-satu, 28 ↔ 28 |
| Enam syarat parity punya cara pembuktian | **Ya** — bagian 10.3 |
| Rancangan migrasi memperhitungkan perilaku seeder | **Ya** — bagian 10.1, termasuk bahaya urutan |
| Rollback dirancang dan tanpa titik tanpa kembali | **Ya** — bagian 12 |
| Rencana test lengkap | **Ya** — bagian 13 |
| Berkas yang berubah terdaftar | **Ya** — bagian 9 |
| Semantik OR diaudit sebelum coding | **Ya** — bagian 8.1, sesuai permintaan pemilik sistem |
| Otorisasi audio diputuskan owner | **Ya** — `O-1` `APPROVED`, bagian 14.1 |
| Keputusan owner yang menahan | **Nol** |

### 15.2 Yang berubah dari rekomendasi sebelumnya

`evidence/02` bagian P menyimpulkan `OWNER_DECISION_REQUIRED` karena dua penahan: wewenang database
dan keputusan audio.

| Penahan | Status akhir |
| --- | --- |
| Wewenang query database development | **Hilang** — disetujui, query sudah dijalankan, hasilnya di dokumen ini |
| Keputusan otorisasi audio antrean | **Hilang** — `O-1` `APPROVED` 2 September 2026 |

Keduanya tertutup, sehingga status naik menjadi `READY_TO_IMPLEMENT_BE_SEC_003` tanpa syarat.

### 15.3 Tiga larangan yang mengikat implementasi

Dicatat di sini agar tidak hilang saat `BE-SEC-003` dikerjakan:

1. **Jangan** memakai `AllowAnonymous` pada endpoint audio.
2. **Jangan** menjadikan `QueueDisplayRuntimeRead` sebagai permission bagi user dokter atau perawat.
   Policy itu tetap milik perangkat, dan tetap tidak membutuhkan Departemen + Posisi.
3. **Jangan** memakai dua atribut otorisasi yang runtime-nya menghasilkan AND. Wajib satu mekanisme
   yang memang menghasilkan OR — bagian 8.1 dan 8.2.

### 15.4 Ringkasan angka

| Ukuran | Nilai |
| --- | ---: |
| Identitas dipecah | 7 → 28 |
| Baris `SysAccessPolicy` | 9 → 40 |
| Departemen × Posisi terdampak | 3 dari 11 |
| Pengguna terdampak pemecahan | 6 dari 39 |
| Kemampuan hilang | **0** |
| Kemampuan baru | **0** |
| Berkas source backend berubah | 9 |
| Berkas source frontend berubah | **0** |
| Migration EF baru | **0** |
| Pengguna terdampak penyempitan audio | 22 dari 39 — **disengaja**, menunggu O-1 |

---

## 16. Koreksi terhadap `evidence/02`

Satu perkiraan pada dokumen closure ternyata keliru setelah diperiksa terhadap source:

| Lokasi | Perkiraan sebelumnya | Yang benar |
| --- | --- | --- |
| `evidence/02` bagian I.5 dan B.3 (BP-02) | Himpunan fallback warisan berubah **69 → 67** karena dua endpoint audio memperoleh identitas | Jumlahnya tetap **69**. Kedua endpoint audio **tetap** berada di himpunan fallback karena tetap tanpa `[AccessPermission]`; yang berubah hanya nama aksinya (`Read` → `PlayAudio`) dan klasifikasi perlindungannya |

Sebabnya: perkiraan awal mengasumsikan endpoint audio akan memakai `[AccessPermission]`. Audit
semantik OR pada bagian 8 membuktikan justru sebaliknya — memakai `[AccessPermission]` di sana akan
menghasilkan AND dan mematahkan jalur perangkat display.

`CompatibilityFallbackMatchesApprovedLegacySetExactly` tetap wajib diperbarui secara sadar, tetapi
yang berubah adalah **isi** himpunan, bukan **jumlahnya**.

---

## Pernyataan Penutup

| Batasan | Status |
| --- | --- |
| Perubahan controller | **Tidak ada** |
| Perubahan attribute | **Tidak ada** |
| Entity baru | **Tidak ada** |
| Migration dibuat | **Tidak ada** |
| Penulisan database | **Tidak ada** — seluruh query berjalan di dalam transaksi `READ ONLY` yang selalu di-`ROLLBACK` |
| Migrasi policy | **Tidak ada** |
| Perubahan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **Tidak ada** — dijadwalkan pada `BE-SEC-004` |
| `git commit` | **Tidak ada** |
| `git push` | **Tidak ada** |
| Working tree frontend | Bersih, tidak disentuh |

Alat query dibuat di direktori scratchpad dan **tidak** menjadi bagian repository.

`BE-SEC-003` berstatus `READY_TO_IMPLEMENT`. Seluruh keputusan pemilik sistem yang dibutuhkan sudah
ditutup; yang tersisa adalah pemberian wewenang eksekusi task itu sendiri.
