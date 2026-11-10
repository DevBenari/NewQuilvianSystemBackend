# Rehab Medik — Permission dan Audit Matrix

| Field | Nilai |
|---|---|
| Blueprint | `RHB-BP-001` r1 |
| `last_changed_in` | `v1` — status `draft` |
| Owner | Sukma Giri Pratama — belum disetujui |
| Traceability | RHB-INV-02, 08; RHB-DEC-002, 005, 011, 016; PRD §7 |

Pemetaan endpoint ke hak akses **hanya** ada di kolom *Hak akses* pada `api-contract.md`.
String atributnya diturunkan langsung: `[AccessPermission("<Resource>", "<Action>")]`.
Pencatatan logger mengikuti konvensi: `GET` tidak dicatat, selain `GET` dicatat.

**Pengecualian logger:** tidak ada. `GET /rehab-lookups/*` tetap tidak dicatat walaupun
membaca data pasien. Jejak akses berkas pasien sudah ditangani `MrcAccessLog` milik Rekam Medis
ketika dokumen dibuka lewat berkas RM.

## 1. Cara kerja hak akses di repository ini

| Unsur | Rujukan source |
|---|---|
| `[AccessController(moduleCode, moduleName, displayName, AreaName, ControllerName)]` di kelas | Pola `Areas/HealthServices/HemodialysisManagement/Controllers/HmdChecklistItemController.cs:32` |
| `[AccessAction("<Action>", "<label>", AccessType = AccessTypes.Read/Create/Update/Delete)]` + `[AccessPermission("<Resource>", "<Action>")]` di setiap endpoint | `PatientEncounterController.cs:525-526` |
| Konstanta resource/action di satu berkas, dipakai di ketiga tempat | `HemodialysisManagement/Constants/HemodialysisPermissions.cs` → rehab: `RehabilitationPermissions.cs` |
| Satu Resource = satu controller (agar terdaftar di `AccessMenuSeeder`) | Keterangan `HemodialysisPermissions` |
| Pemberian hak: admin lewat Pengaturan → Manajemen Role → Akses Role | `role-access-rules.md` |

`ModuleCode = "HEALTH_SERVICE_REHABILITATION_MANAGEMENT"`,
`ModuleName = "Health Service Rehabilitation Management"`, `AreaName = "HealthServices"`.

## 2. Daftar Resource dan Action

| Resource | Action → `AccessType` |
|---|---|
| `RehabRequest` | `Read`→Read, `Create`→Create, `Update`→Update, `Return`→Update, `Disposition`→Update, `Cancel`→Update |
| `RehabLookup` | `Read`→Read |
| `RehabEpisode` | `Read`→Read |
| `RehabMedicalAssessment` | `Read`, `Create`, `Update`, `Sign`→Update |
| `RehabDisciplineAssessment` | `Read`, `Create`, `Update`, `Sign`→Update |
| `RehabGoal` | `Read`, `Create`, `Update` |
| `RehabProgram` | `Read`, `Create`, `Update`, `Activate`→Update, `Stop`→Update |
| `RehabEducation` | `Read`, `Create`, `Update`, `Sign`→Update |
| `RehabSession` | `Read`, `Create`, `Update`, `Sign`→Update |
| `RehabBillingHandoff` | `Read`, `Retry`→Update |
| `RehabProgressReview` | `Read`, `Create`, `Update`, `Sign`→Update |
| `RehabDischargeSummary` | `Read`, `Update`, `Sign`→Update |
| `RehabConfiguration` | `Read`, `Create`, `Update`, `Delete` |

## 3. Peta peran rumah sakit ke butir hak akses (usulan untuk admin)

Peta ini adalah **saran konfigurasi**, bukan kode. Tidak ada nama peran di source.

| Peran | Butir yang diberikan |
|---|---|
| Petugas admisi rehab | `RehabRequest : Read/Create/Update/Return/Cancel`, `RehabLookup : Read`, `RehabEpisode : Read`, `RehabSession : Read` |
| Dokter Sp.KFR | Semua butir `Read`; `RehabRequest : Disposition`; `RehabMedicalAssessment : *`; `RehabGoal : *`; `RehabEducation : *`; `RehabProgressReview : *`; `RehabDischargeSummary : *`; `RehabProgram : Stop` |
| Fisioterapis, terapis okupasi, terapis wicara | Semua butir `Read`; `RehabDisciplineAssessment : *`; `RehabGoal : *`; `RehabProgram : Create/Update/Activate/Stop`; `RehabEducation : *`; `RehabSession : *`; `RehabProgressReview : Create/Update/Sign` |
| Petugas billing | `RehabSession : Read`, `RehabBillingHandoff : Read/Retry` |
| Petugas rekam medis / auditor | Semua butir `Read`; addendum lewat `ClinicalNoteAddendum` milik RM |
| Admin konfigurasi rehab | `RehabConfiguration : *`; `ClinicalInstrumentConfiguration : Read/Update` (milik Cli) |
| Komite medik | `ClinicalInstrumentConfiguration : Approve` (milik Cli) |

## 4. Kewenangan yang tidak dapat dijaga mesin hak akses

| Penjaga di service | Yang dijaga | Yang **tidak** dijaga | Risiko |
|---|---|---|---|
| Penulis = pemilik dokumen (`AuthorUserId`, keutuhan RM) | Hanya penulis yang mengubah/menandatangani draft | Siapa yang diberi butir `Create` | Admin memberi butir ke peran yang salah. Mitigasi: peta §3 dan review akses berkala. |
| Profesi pengguna = `RhbDiscipline.ProfessionId` | Terapis okupasi tidak bisa membuat program/sesi fisioterapi | Kompetensi/kredensial individual (SIP/STR) | Kredensial di luar scope (HR). Rehab hanya memeriksa profesi. |
| Asesmen medis final sebelum program aktif | RHB-DEC-005 | Bahwa yang menandatangani benar Sp.KFR. Itu dijaga lewat pemberian butir `RehabMedicalAssessment : Sign`, bukan lewat spesialisasi teks `MstDoctor.SpecialistName`. | Butir diberikan ke dokter non-Sp.KFR. Mitigasi: konfigurasi akses. |
| Kunjungan milik pasien yang sama (lookup + simpan) | Anti-IDOR antar pasien | — | — |
| `RhbMedicalRecordDocumentSource.ReadDetailAsync` mengembalikan `null` untuk pasien lain | Anti-IDOR pada berkas RM | — | — |

## 5. Audit

| Lapisan | Yang dicatat |
|---|---|
| Logger project (Create/Update/aksi) | `EntityId`, controller, action, status hasil, aktor. **Tanpa** isi klinis. |
| `RhbStatusHistory` | Setiap perpindahan status permintaan, episode, program, sesi: dari, ke, aksi, alasan, aktor, waktu |
| `MrcClinicalDocumentIntegrity` | Penulis, tanda tangan (waktu, perangkat, IP dari server), kunci, pembatalan |
| `MrcClinicalNoteAddendum` | Koreksi beserta alasan, penulis, urutan |
| `IdentityModel` | Create/Update/Cancel/Delete by + waktu |
| `RhbSession.BillingHandoff*` | Status, waktu, galat handoff (tanpa data klinis) |

Kejadian yang **wajib** meninggalkan jejak tahan lama: disposisi permintaan (terutama `reject`),
aktivasi/penghentian program, sesi `StoppedEarly`/`Escalated`, penutupan episode, retry billing.

## 6. Kolom sensitif dan masa simpan

Kolom bertanda **S** pada `data/data-dictionary.md` tidak boleh masuk payload log, pesan galat,
`BillingHandoffError`, maupun daftar/worklist. Masa simpan mengikuti kebijakan rekam medis
enterprise (Permenkes 24/2022). Rehab tidak menghapus fisik data klinis.
