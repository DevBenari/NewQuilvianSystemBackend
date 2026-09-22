# Hemodialisa — Peta Kemampuan Existing

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Revision peta | `2` |
| Status | `draft` |
| Masukan | `00-interview-decisions.md` revision 9 |
| Decision yang berlaku | `HMD-DEC-001` sampai `HMD-DEC-004`, `HMD-DEC-006` sampai `HMD-DEC-011` |
| Backend SHA | `190c91a0` — branch `MHamzah`. Audit asli pada `69b256ca`; impact scan 18 September 2026 menyatakan seluruh temuan backend masih berlaku |
| Frontend SHA | `a38683142` — branch `HamzahV2`. Audit asli pada `8143874d8`; impact scan 18 September 2026 memperbarui empat temuan frontend |
| Tanggal audit | 18 September 2026 |
| Task mode | `MODULE BLUEPRINT MODE` — read-only terhadap source aplikasi |

> **Cara membaca dokumen ini.**
> Dokumen ini menjawab satu pertanyaan saja: **apa yang sudah ada di sistem hari ini.**
> Ia tidak merancang apa pun, tidak mengusulkan tabel baru, dan bukan izin menulis kode.
>
> Setiap kemampuan diberi tepat satu status:
>
> | Status | Artinya |
> |---|---|
> | `READY TO REUSE` | Sudah ada, sudah dipakai modul lain, dan cocok dipakai apa adanya |
> | `REUSE WITH ADAPTER` | Datanya sudah ada, tetapi cara membacanya belum sesuai kebutuhan Hemodialisa |
> | `EXTEND` | Mekanismenya sudah ada dan tinggal ditambah agar mengenali Hemodialisa |
> | `REPAIR` | Sudah ada tetapi rusak atau tidak konsisten |
> | `MISSING` | Belum ada sama sekali |
> | `CONFLICT` | Dua sumber saling bertentangan |
> | `UNKNOWN` | Belum dapat dipastikan tanpa akses atau keputusan manusia |

---

## Batas Audit

Audit ini mencakup **26 kemampuan Phase 1** yang dikunci `HMD-DEC-001` dan `HMD-DEC-008`: 25
Feature `MUST HAVE` ditambah `HMD-CAP-001` (permintaan HD masuk).

Kemampuan Phase 2 dan Phase 3 **tidak diaudit**, sesuai `HMD-DEC-002`. Beberapa fakta yang
kebetulan ditemukan dan menyangkut fase itu sudah dicatat di decision log sebagai
`HMD-FACT-013` sampai `HMD-FACT-018`, dan tidak diulang di sini.

Penelusuran dilakukan per klaster kemampuan, bukan dengan menyisir seluruh repository:
Identity/Master Owner, Episode/Transaction Owner, Actor/Workforce, Location/Resource,
Workflow/Status, Documentation/Record, Order/Result, Financial, Authorization/Audit, dan
External Integration.

---

## Ringkasan Hasil

| Status | Jumlah | Kemampuan |
|---|---:|---|
| `READY TO REUSE` | 5 | `FEAT-001`, `FEAT-006`, `FEAT-011`, `FEAT-033`, dan pola kerangka layar klinis frontend |
| `REUSE WITH ADAPTER` | 3 | `FEAT-005` (baca hasil Lab), `FEAT-029` (baca stok Farmasi), `FEAT-030` (kompetensi staf) |
| `EXTEND` | 3 | `FEAT-019`, `FEAT-036`, dan `ServiceUnitType` |
| `MISSING` | 15 | Seluruh inti Hemodialisa |
| `CONFLICT` | 0 | — |
| `UNKNOWN` | 0 | — |

**Kesimpulan besarnya:** tidak ada duplikasi yang tersembunyi. Tidak ada satu pun tabel atau
endpoint Hemodialisa yang ternyata sudah pernah dibuat orang lain. Tetapi juga tidak benar
bahwa semuanya harus dibangun dari nol — lima kemampuan bisa dipakai apa adanya, dan tiga
lainnya hanya perlu ditambah agar mengenali Hemodialisa.

Yang paling berharga dari audit ini bukan daftar `MISSING`-nya, melainkan **tiga pola yang
sudah terbukti jalan** dan sebaiknya disalin bentuknya, bukan dirancang ulang. Lihat bagian
*Pola yang Wajib Diikuti*.

---

## Kontrak Bukti Kemampuan

### Klaster 1 — Identitas pasien dan konteks kunjungan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-01` | `FEAT-001` — verifikasi identitas pasien | Patient Management | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs@69b256ca` | `READY TO REUSE` | Tidak ada | Rendah |
| `CAP-02` | `FEAT-001` — konteks kunjungan | Registration Management | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs@69b256ca` | `READY TO REUSE` | Tidak ada. `EncounterType` sudah memuat Outpatient/Emergency/Inpatient sehingga HD tidak perlu jenis baru | Rendah |
| `CAP-03` | Konteks rawat inap opsional | Inpatient Management | `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs@69b256ca` | `READY TO REUSE` | Tidak ada. Pola `InpEpisodeId` nullable sudah dipakai `LabOrder` dan `RadOrder` | Rendah |

### Klaster 2 — Dokumentasi klinis

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-04` | `FEAT-006` — persetujuan tindakan | Clinical Management | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientConsent.cs@69b256ca` | `READY TO REUSE` | Tidak ada. Hemodialisa **tidak** membuat `HmdConsent` | Rendah |
| `CAP-05` | `FEAT-011`, `FEAT-017` — tanda vital pra dan pasca HD | Clinical Management | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs@69b256ca` | `READY TO REUSE` | Tidak ada untuk vital klinis. Parameter mesin (QB, QD, TMP, UF, VP, AP) **tidak** dipaksakan masuk ke sini | Rendah |
| `CAP-06` | Assessment generik | Clinical Management | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs@69b256ca` | `READY TO REUSE` | Dipakai bila field-nya cocok; bila tidak, data khusus sesi disimpan sendiri | Rendah |
| `CAP-07` | `FEAT-019` — finalisasi dan penguncian catatan | Medical Record Management | `Models/MrcClinicalDocumentIntegrity.cs`; `Services/ClinicalDocumentIntegrityService.cs:104, 200, 242, 340, 426@69b256ca` | `EXTEND` | **Dua langkah, bukan satu.** Lihat *Temuan Kritis 1* | **Tinggi bila hanya satu langkah dikerjakan** |
| `CAP-08` | `FEAT-036` — koreksi lewat *addendum* | Medical Record Management | `Models/MrcClinicalNoteAddendum.cs`; `Services/ClinicalNoteAddendumService.cs`; `Controllers/ClinicalNoteAddendumController.cs:179, 192@69b256ca` | `EXTEND` | Sama seperti `CAP-07`; endpoint-nya generik per `documentKind` | Sedang |
| `CAP-09` | Jejak akses rekam medis | Medical Record Management | `Models/MrcAccessLog.cs`; `Services/MedicalRecordAccessAuditService.cs@69b256ca` | `READY TO REUSE` | Tidak ada | Rendah |

### Klaster 3 — Tindakan dan penagihan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-10` | `FEAT-013`, `FEAT-033` — tindakan yang dapat ditagih | Clinical Management | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs:29-30, 240-256@69b256ca` | `READY TO REUSE` | Tidak ada. Dua kolom dokter dipakai sesuai `HMD-DEC-009` | Rendah |
| `CAP-11` | `FEAT-033` — jalur serah terima ke Billing | Billing Management | `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs@69b256ca` | `READY TO REUSE` | Tidak ada. `Procedure` sudah terdaftar sebagai sumber yang sah. Hemodialisa **tidak** perlu sumber baru | Rendah |
| `CAP-12` | Master tindakan HD | Master Data | `Areas/HealthServices/MasterData/Models/MstProcedure.cs@69b256ca` | `READY TO REUSE` | Tidak ada. Tindakan HD didaftarkan sebagai baris data induk, bukan tabel baru | Rendah |

### Klaster 4 — Sumber daya unit

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-13` | `FEAT-027` — registry mesin HD dan status laiknya | — | Tidak ditemukan. `Areas/HealthServices/MasterData/Models/` tidak memuat satu pun master alat medis. `MstRadModality` adalah master **jenis** alat radiologi dan hanya punya `IsActive`, bukan status operasional per unit | `MISSING` | Bangun baru. Bentuk terdekat yang sudah ada adalah `MstBed` — lihat *Pola yang Wajib Diikuti* | Sedang |
| `CAP-14` | Station/kursi HD | — | Tidak ditemukan | `MISSING` | Bangun baru, mengikuti `MstBed` | Sedang |
| `CAP-15` | `FEAT-012`, `FEAT-028` — kesiapan unit dan pengolahan air | — | Tidak ditemukan | `MISSING` | Bangun baru | Sedang |
| `CAP-16` | `FEAT-026` — keputusan isolasi | — | Tidak ada tabel keputusan isolasi. Yang ada hanya penanda statis `MstBed.IsIsolationBed:35@69b256ca` | `MISSING` | Bangun baru. Penanda statis tidak cukup karena isolasi HD bergantung status serologi yang berubah | Sedang |
| `CAP-17` | Ruang dan unit layanan | Master Data | `MstRoom.cs`, `MstServiceUnit.cs@69b256ca` | `READY TO REUSE` | Tidak ada | Rendah |
| `CAP-18` | `ServiceUnitType.Hemodialysis` | Master Data | `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs:3-16@69b256ca` — berisi 0–9 lalu `Other = 99`; nilai `10` kosong | `EXTEND` | Tambahkan satu nilai enum. Tidak menabrak nilai lain | Rendah |

### Klaster 5 — Staf dan kompetensi

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-19` | Identitas dokter dan petugas | Human Resource | `MstDoctor.cs`, `MstEmployee.cs`, `MstWorkforceProfile.cs@69b256ca` | `READY TO REUSE` | Tidak ada. Hemodialisa **tidak** menyalin nama, STR, atau SIP | Rendah |
| `CAP-20` | `FEAT-030` — kompetensi dan kewenangan klinis | Human Resource / Credentialing | `Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpClinicalPrivilege.cs:12-56@69b256ca` — memuat `PrivilegeCode`, `PrivilegeName`, `ProcedureGroup`, `ProcedureName`, `PracticeLocation` | `REUSE WITH ADAPTER` | **Datanya kaya, cara membacanya belum ada.** Lihat *Temuan Kritis 3* | Sedang |
| `CAP-21` | Jadwal dinas petugas | Human Resource / Scheduling | `TrxShiftAssignment.cs`, `WfpWorkScheduleAssignment.cs`, `TrxRosterAssignment.cs@69b256ca` | `REUSE WITH ADAPTER` | Berguna untuk memeriksa petugas benar-benar bertugas pada shift itu, tetapi bukan penjadwalan pasien | Sedang |

### Klaster 6 — Pesanan, hasil, dan obat

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-22` | `FEAT-005` — baca hasil serologi | Laboratory | `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs`; `Controllers/LabExaminationController.cs:42, 56@69b256ca` | `REUSE WITH ADAPTER` | **Hasil hanya bisa dibaca per pesanan atau per spesimen.** Lihat *Temuan Kritis 2* | **Tinggi** |
| `CAP-23` | `FEAT-015` — pemakaian obat diteruskan ke Farmasi | Pharmacy | `PhmDrugUsage` pada `Areas/HealthServices/PharmacyManagement/@69b256ca` | `READY TO REUSE` | Hemodialisa mencatat fakta pemberian klinis, Farmasi tetap memiliki stok | Rendah |
| `CAP-24` | `FEAT-029` — kesiapan obat dan BMHP | Pharmacy | Sumber stok dimiliki Farmasi | `REUSE WITH ADAPTER` | Hemodialisa hanya menampilkan status kesiapan; kontrak bacanya perlu disepakati dengan Farmasi | Sedang |

### Klaster 7 — Inti Hemodialisa yang belum ada

Seluruh baris berikut berstatus `MISSING`. Pencarian `hemodial|dialisa|dialysis` pada seluruh
berkas `.cs` backend menghasilkan **nol** berkas (`HMD-FACT-007`).

| ID | Kebutuhan | Status | Catatan |
|---|---|---|---|
| `CAP-25` | `FEAT-003` — episode/program HD | `MISSING` | Tidak boleh memakai `InpEpisode` |
| `CAP-26` | `FEAT-004` — penilaian kelayakan klinis | `MISSING` | — |
| `CAP-27` | `FEAT-005` — status akses vaskular | `MISSING` | Sisi Hemodialisa-nya baru; hasil Lab-nya lihat `CAP-22` |
| `CAP-28` | `FEAT-007`, `FEAT-008` — resep HD dan revisinya | `MISSING` | — |
| `CAP-29` | `FEAT-009` — penjadwalan sesi | `MISSING` | Tidak ada penjadwalan sumber daya pasien di mana pun |
| `CAP-30` | `FEAT-010` — checklist Pra-HD | `MISSING` | — |
| `CAP-31` | `FEAT-013` — sesi HD | `MISSING` | — |
| `CAP-32` | `FEAT-014` — pemantauan berkala dan parameter mesin | `MISSING` | — |
| `CAP-33` | `FEAT-015` — catatan pemberian obat pada sesi | `MISSING` | Sisi klinisnya baru; stoknya lihat `CAP-23` |
| `CAP-34` | `FEAT-016` — komplikasi | `MISSING` | — |
| `CAP-35` | `FEAT-018` — disposisi pasien | `MISSING` | — |
| `CAP-36` | `HMD-CAP-001` — permintaan HD masuk | `MISSING` | Bentuknya menyalin `LabOrder`/`RadOrder` |

### Klaster 8 — Frontend

| ID | Kebutuhan | Bukti | Status | Gap/adapter |
|---|---|---|---|---|
| `CAP-37` | Kerangka layar klinis | `src/components/ui/clinical-workspace/`: `ClinicalPageHeader.jsx`, `ClinicalWorkspaceShell.jsx`, `ClinicalSectionNav.jsx`, `ClinicalStateBoundary.jsx@8143874d8`. `ClinicalStateBoundary` dipakai 37 berkas | `READY TO REUSE` | Klaim PRD terbukti benar. Keempatnya nyata dan sudah dipakai luas |
| `CAP-38` | Struktur modul klinis frontend | `src/app/health-services/radiology-management/` (4 route), `src/lib/services/.../radiology-management/` (6 service), `src/lib/hooks/.../radiology-management/` (aturan transisi, aturan query, hook form dan antrean), `src/lib/constants/...@8143874d8` | `READY TO REUSE` (sebagai pola) | Preseden lengkap yang tinggal disalin bentuknya |
| `CAP-39` | Butir menu Hemodialisa | `src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:9` memesan kunci `menuHemodialisa`, tetapi belum ada butir menu, route, atau layar | `MISSING` | Kunci namanya sudah dipesan; isinya belum ada |
| `CAP-40` | Titik masuk permintaan dari Rawat Inap | `.../inpatient-supporting-service-constants.jsx:70-79` — kartu Hemodialisa berlabel "Integrasi belum tersedia", `isAvailable: false`, `routeKey` tidak pernah dipakai navigasi | `MISSING` | Layar penampungnya sudah ada dan sengaja dikosongkan (`RWI-DEC-108`). Tinggal disambungkan bila `HMD-CAP-001` jadi |

---

## Temuan Kritis

Tiga temuan berikut mengubah perkiraan pekerjaan, bukan sekadar melengkapi daftar.

### Temuan Kritis 1 — Menambah nomor dokumen saja tidak membuat catatan HD terkunci

PRD menulis targetnya sebagai satu baris: `ClinicalDocumentKind.HemodialysisSession = 14`.

Kenyataannya **ada dua tempat**, dan yang kedua justru yang menentukan.

`ClinicalDocumentIntegrityService` menyimpan daftar tertutup berisi jenis dokumen yang
benar-benar ditegakkan aturan keutuhannya:

```csharp
private static readonly HashSet<ClinicalDocumentKind> JenisYangDitegakkan =
[
    ClinicalDocumentKind.ProgressNote,
    ClinicalDocumentKind.Consultation,
    ClinicalDocumentKind.Assessment,
    ClinicalDocumentKind.Procedure
];
```

*(`Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs:81-93@69b256ca`)*

Keterangan di atasnya menyatakan pembatasan itu disengaja: *"Sembilan jenis lain sudah punya
nomor pada `ClinicalDocumentKind`, tetapi belum ditegakkan."*

**Contoh akibatnya bila langkah kedua terlewat:** nomor 14 ditambahkan, sesi HD difinalisasi,
layar menampilkan "Finalized" — tetapi `DitegakkanUntuk` mengembalikan `false`, sehingga
dokumen tidak pernah terdaftar di daftar keutuhan, `EnsureMutableAsync` tidak pernah menolak
perubahan, dan catatan sesi yang seharusnya terkunci tetap bisa disunting. Acceptance criteria
`HMD-AC-008` akan gagal tanpa ada pesan error apa pun yang menjelaskan sebabnya.

Keterangan kode itu juga menuntut satu hal lagi: jenis yang **belum** ditegakkan wajib
dinyatakan terbuka di layar (`RM-FE-009`), bukan didiamkan.

### Temuan Kritis 2 — Hasil laboratorium tidak bisa dibaca per pasien

Hemodialisa butuh menjawab: *"berapa hasil HBsAg terakhir pasien ini, dan kapan?"* — untuk
`FEAT-005` dan untuk keputusan isolasi `FEAT-026`.

Laboratorium hanya menyediakan dua jalan baca:

| Method | Path | Kegunaan |
|---|---|---|
| `GET` | `/by-order/{labOrderId}` | Hasil untuk satu pesanan tertentu |
| `GET` | `/by-specimen/{specimenId}` | Hasil untuk satu spesimen tertentu |

*(`Areas/HealthServices/LaboratoryManagement/Controllers/LabExaminationController.cs:42, 56@69b256ca`)*

Keduanya mengharuskan pemanggil **sudah tahu** id pesanan atau spesimennya. Hemodialisa tidak
tahu — yang diketahuinya hanya pasiennya.

**Ini dependency lintas modul, bukan pekerjaan Hemodialisa.** Menyalin hasil Lab ke tabel
Hemodialisa dilarang tegas oleh PRD dan oleh kepemilikan data. Jalan yang sah adalah meminta
Laboratorium menyediakan pembacaan per pasien per jenis pemeriksaan.

Ini risiko tertinggi pada peta ini karena jadwalnya tidak dipegang Hemodialisa.

### Temuan Kritis 3 — Data kompetensi ada, cara bertanyanya belum

`WfpClinicalPrivilege` menyimpan persis yang dibutuhkan: `PrivilegeCode`, `PrivilegeName`,
`ProcedureGroup`, `ProcedureName`, `PracticeLocation`, ditambah seluruh siklus pengajuan,
persetujuan, penangguhan, dan pencabutan.

Tetapi seluruh berkas yang membacanya berada **di dalam** `CredentialingManagement` sendiri.
Tidak ada satu pun modul klinis yang bertanya *"apakah perawat ini punya kewenangan dialisis
yang masih berlaku hari ini?"*.

Hemodialisa akan menjadi modul klinis pertama yang menanyakannya. Bentuk pertanyaannya perlu
disepakati dengan pemilik Human Resource, bukan ditebak.

---

## Pola yang Wajib Diikuti

Tiga pola berikut sudah terbukti berjalan di repository ini. Merancang bentuk baru untuk
ketiganya akan melanggar aturan "ikuti kode yang sudah ada" pada `AGENTS.md`.

### Pola 1 — Permintaan layanan penunjang

`LabOrder` dan `RadOrder` memakai bentuk yang sama persis:

| Unsur | Nilai |
|---|---|
| Konteks kunjungan | `EncounterId` wajib |
| Konteks rawat inap | `InpEpisodeId` boleh kosong |
| Status awal | `Requested` |
| Pola penahanan | `StatusBeforeHold` |

Siklus hidup `RadOrderStatus` selengkapnya: `Draft`, `Requested`, `Accepted`, `Scheduled`,
`InProgress`, `Completed`, `OnHold`, `CancelRequested`, `Cancelled`, `Rejected`
*(`Areas/HealthServices/RadiologyManagement/Enums/RadiologyEnums.cs:12-43@69b256ca`)*.

Perhatikan `Rejected` dan `OnHold` — keduanya persis yang ditanyakan `HMD-OQ-007`. Jawabannya
tidak perlu menciptakan kosakata status baru.

### Pola 2 — Status dan okupansi sumber daya

`MstBed` adalah bentuk terdekat untuk station dan mesin HD:

| Unsur `MstBed` | Padanan Hemodialisa |
|---|---|
| `BedStatus`: `Available`, `Occupied`, `Reserved`, `Cleaning`, `Maintenance`, `Blocked`, `Inactive` | Status mesin: `Ready`, `Blocked`, `Maintenance`, `NotEligible` |
| `IsIsolationBed` | Kebutuhan isolasi Hepatitis B |
| `IsReservable` | Mesin yang boleh dijadwalkan |

*(`Areas/HealthServices/MasterData/Models/MstBed.cs:9-41`; `Enums/BedStatus.cs:3-13@69b256ca`)*

Layanannya pun sudah ada: `InpBedOccupancyService` menyediakan `SearchAvailableBedsAsync`,
`GetBedBoardAsync`, `ReserveBedAsync`, `CancelReservationAsync`, `ExpireDueReservationsAsync`,
`PlacePatientAsync`, dan `TransferAsync`
*(`Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:79-1136@69b256ca`)*.

**Satu hal yang belum ada padanannya:** `MstBed` tidak menyimpan riwayat perubahan status.
Kebutuhan `FEAT-027` — "riwayat status harus disimpan" — memang baru.

### Pola 3 — Pencegahan tabrakan lewat index basis data

Rawat Inap mencegah duplikasi dengan unique index, bukan hanya dengan pemeriksaan di service:

```csharp
builder.HasIndex(x => new { x.EpisodeId, x.SequenceNumber }).IsUnique();
builder.HasIndex(x => x.EncounterId).IsUnique();
```

*(`Repositories/Configurations/HealthServices/InPatientManagement/InpBedPlacementConfiguration.cs:28`;
`InpEpisodeConfiguration.cs:26@69b256ca`)*

Acceptance criteria `HMD-AC-001` sampai `HMD-AC-004` — episode ganda dan tabrakan jadwal —
sebaiknya dijaga cara yang sama, bukan hanya dengan validasi di service.

---

## Kontrak As-Is yang Akan Dikonsumsi Hemodialisa

Endpoint berikut **sudah ada** dan akan dipakai apa adanya. Ditulis seperti tampilan Swagger
agar dapat dicocokkan langsung.

### Health Services / Medical Record Management / Clinical Document Integrity

Base URL: `api/v1/health-services/medical-record-management/clinical-document-integrities`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/by-document/{documentKind}/{documentId}` | Melihat status keutuhan satu dokumen klinis | `ClinicalDocumentIntegrity : Read` | Path | `ApiResponse<ClinicalDocumentIntegrityResponse>` |
| `POST` | `/by-document/{documentKind}/{documentId}/sign` | Menandatangani dan mengunci dokumen | `ClinicalDocumentIntegrity : Update` | Path + body | `ApiResponse<ClinicalDocumentIntegrityResponse>` |
| `GET` | `/by-encounter/{encounterId}` | Seluruh dokumen pada satu kunjungan | `ClinicalDocumentIntegrity : Read` | Path | `ApiResponse<PagedResult<...>>` |
| `GET` | `/my-unsigned` | Dokumen milik pengguna yang belum ditandatangani | `ClinicalDocumentIntegrity : Read` | Query | `ApiResponse<PagedResult<UnsignedDocumentResponse>>` |

*(`Areas/HealthServices/MedicalRecordManagement/Controllers/ClinicalDocumentIntegrityController.cs:29-323@69b256ca`)*

Arti kode status bagi pengguna: **200** dokumen ditemukan atau berhasil dikunci; **403**
pengguna tidak berwenang menandatangani dokumen ini; **404** dokumen belum terdaftar pada
daftar keutuhan — pada kasus Hemodialisa, inilah yang muncul bila *Temuan Kritis 1* terlewat;
**409** dokumen sudah terkunci sehingga tidak dapat diubah lagi.

Karena `{documentKind}` adalah bagian dari path, Hemodialisa **tidak perlu endpoint baru**.
Begitu `HemodialysisSession` dikenali di dua tempat, keempat endpoint ini langsung berlaku.

### Health Services / Medical Record Management / Clinical Note Addendum

Base URL: `api/v1/health-services/medical-record-management/clinical-note-addendums`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/by-document/{documentKind}/{documentId}` | Daftar koreksi pada satu dokumen | `ClinicalNoteAddendum : Read` | Path | `ApiResponse<PagedResult<...>>` |
| `GET` | `/authority/{documentKind}/{documentId}` | Memeriksa apakah pengguna berwenang mengoreksi | `ClinicalNoteAddendum : Read` | Path | `ApiResponse<...>` |
| `POST` | `/by-document/{documentKind}/{documentId}` | Membuat koreksi sebagai penulis asli | `ClinicalNoteAddendum : Create` | Path + body | `ApiResponse<...>` |
| `POST` | `/by-document/{documentKind}/{documentId}/as-substitute` | Membuat koreksi sebagai pengganti penulis | `ClinicalNoteAddendum : CreateAsSubstitute` | Path + body | `ApiResponse<...>` |

*(`Areas/HealthServices/MedicalRecordManagement/Controllers/ClinicalNoteAddendumController.cs:22-197@69b256ca`)*

Sama seperti di atas, seluruhnya generik per `documentKind`.

### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Request | Response |
|---|---|---|---|---|
| `GET` | `/by-order/{labOrderId}` | Hasil pemeriksaan satu pesanan | Path | `ApiResponse<...>` |
| `GET` | `/by-specimen/{specimenId}` | Hasil pemeriksaan satu spesimen | Path | `ApiResponse<...>` |

*(`Areas/HealthServices/LaboratoryManagement/Controllers/LabExaminationController.cs:21-56@69b256ca`)*

**Tidak tersedia, dan inilah gap-nya:** pembacaan per pasien, dan pembacaan "hasil terakhir
untuk jenis pemeriksaan tertentu". Lihat *Temuan Kritis 2*.

### Konvensi yang berlaku untuk seluruh controller Hemodialisa nanti

Contoh nyata dari Radiologi:

```csharp
[Route("api/v1/health-services/radiology-management/rad-orders")]
[Tags("Health Services / Radiology Management / Rad Order")]
...
[AccessPermission("RadOrder", "Read")]
```

*(`Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs:14-37@69b256ca`)*

Hak akses ditegakkan per endpoint dengan `[AccessPermission(Resource, Action)]`. Ini yang
memenuhi acceptance criteria `HMD-AC-012`.

---

## Conflict dan Unknown

| Hal | Keadaan |
|---|---|
| Conflict | **Tidak ada.** Dua conflict yang sudah tercatat di decision log — `HMD-CONF-001` dan `HMD-CONF-002` — tidak berubah oleh audit ini. Yang pertama sudah ditutup `HMD-DEC-008`; yang kedua di luar Phase 1 |
| Unknown | **Tidak ada** pada batas audit ini |

Dua hal berikut **bukan** Unknown, melainkan dependency yang pemiliknya sudah jelas dan
jadwalnya belum: pembacaan hasil Lab per pasien (`CAP-22`, milik Laboratorium) dan pembacaan
kewenangan klinis (`CAP-20`, milik Human Resource).

---

## Impact Scan 18 September 2026

Dijalankan karena kedua SHA berubah setelah audit asli, sebelum `design-business-module` boleh
menulis berkas apa pun.

| Repository | Dari | Ke | Perubahan |
|---|---|---|---|
| Backend | `69b256ca` | `190c91a0` | **1 commit, 9 berkas, seluruhnya di `docs/`.** Tidak ada satu pun source `.cs` berubah |
| Frontend | `8143874d8` | `a38683142` | **98 commit, 564 berkas.** Sebagian besar Accounting, Finance, Petty Cash, Billing, Radiologi, dan *nursing workspace* Rawat Inap |

### Hasil per temuan

| Temuan | Keadaan setelah scan |
|---|---|
| Seluruh temuan backend `CAP-01` sampai `CAP-36` | **Tetap berlaku.** Perubahan backend hanya dokumen |
| *Temuan Kritis 1, 2, 3* | **Tetap berlaku.** Ketiganya bersandar pada source backend yang tidak berubah |
| *Pola 1, 2, 3* | **Tetap berlaku** |
| `CAP-37` kerangka layar klinis | **Diperbarui — bertambah baik.** Lihat di bawah |
| `CAP-38` struktur modul klinis frontend | **Tetap berlaku.** Radiologi tetap 4 route dan 6 service; isinya bertambah kaya, bentuknya tidak berubah |
| `CAP-39` kunci menu `menuHemodialisa` | **Tetap berlaku.** Berkasnya tidak tersentuh 98 commit itu |
| `CAP-40` titik masuk dari Rawat Inap | **Diperbarui — sekarang ada dua, bukan satu.** Lihat di bawah |

### Temuan baru 1 — Kerangka layar klinis bertambah tujuh komponen yang langsung dipakai Hemodialisa

Pada audit asli hanya empat komponen yang diperiksa. Folder `src/components/ui/clinical-workspace/`
sekarang memuat 21 komponen, dan tujuh di antaranya menjawab kebutuhan Hemodialisa secara langsung:

| Komponen | Kebutuhan Hemodialisa yang dijawab |
|---|---|
| `ClinicalAddendumList.jsx`, `ClinicalAddendumItem.jsx` | `FEAT-036` koreksi setelah final |
| `ClinicalRevisionHistory.jsx`, `ClinicalRevisionItem.jsx` | Riwayat revisi resep dan catatan |
| `ClinicalSafetyAlert.jsx` | `FEAT-016` komplikasi dan eskalasi |
| `ClinicalValidationSummary.jsx` | `FEAT-019` penolakan finalisasi karena isian belum lengkap |
| `ClinicalCompletionBar.jsx` | `FEAT-010` kemajuan pengisian checklist Pra-HD |
| `ClinicalTimeline.jsx`, `ClinicalTimelineItem.jsx` | `FEAT-014` riwayat pemantauan berkala |
| `PatientContextHeader.jsx` | Kepala konteks pasien pada seluruh layar kerja |

**Akibatnya bagi desain:** ketujuhnya dipakai ulang, **tidak** dibuat baru. Status `CAP-37`
tetap `READY TO REUSE`, cakupannya yang meluas.

### Temuan baru 2 — Titik masuk permintaan HD dari Rawat Inap sekarang ada dua

Pada `8143874d8` hanya layar dokter yang memuat kartu Hemodialisa. Pada `a38683142`, layar
**perawat** juga memuatnya:

| Layar | Berkas | Keadaan |
|---|---|---|
| *Physician workspace* — layanan penunjang | `.../physician-workspace/tabs/supporting-service/` | Kartu "Hemodialisa", `isAvailable: false` |
| *Nursing workspace* — penunjang (baru) | `.../nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx:28, 35, 50-53` | Kunci `dialysis` dipetakan ke `SUPPORTING_SERVICE_KEY.HEMODIALYSIS`, masuk `UNAVAILABLE_SERVICE_KEYS`, keterangan *"Pelayanan hemodialisis rutin dan cito rawat inap. Integrasi modul ini belum tersedia di rilis ini."* |

**Akibatnya bagi desain:** `HMD-DEC-008` — entity permintaan HD masuk — menjadi **lebih kuat
dasarnya**, karena sekarang dua layar menunggunya, bukan satu. Sekaligus muncul satu hal yang
perlu diputuskan saat merancang: **perawat bangsal, bukan hanya dokter, tampaknya juga membuat
permintaan penunjang.** Dimensi Aktor pada slice `S1` berstatus `PROPOSED` pada `HMD-RCG-001`,
dan temuan ini memberinya bukti — tetapi siapa yang sah membuat permintaan HD tetap keputusan
pemilik, bukan kesimpulan dari susunan layar.

### Verdict

**Desain boleh berjalan.** Tidak ada temuan yang membatalkan keputusan mana pun. Dua temuan di
atas memperkaya `CAP-37` dan `CAP-40`, dan tidak satu pun mengubah status kesiapan
`HMD-RCG-001`.

---

## Pemicu Impact Scan

Peta ini dihitung pada backend `69b256ca` dan frontend `8143874d8`. Tandai peta ini **stale**
lalu jalankan impact scan terbatas bila salah satu terjadi:

| Pemicu | Yang harus diperiksa ulang |
|---|---|
| SHA backend berubah | `CAP-07`, `CAP-08`, `CAP-11`, `CAP-18`, `CAP-22` |
| SHA frontend berubah | `CAP-37`, `CAP-38`, `CAP-39`, `CAP-40` |
| `JenisYangDitegakkan` berubah | `CAP-07`, `CAP-08`, *Temuan Kritis 1* |
| `BillingSourceContract` berubah | `CAP-11` |
| Laboratorium menambah endpoint baca | `CAP-22`, *Temuan Kritis 2* |
| `MstBed` atau `BedStatus` berubah | *Pola 2* |
| Baris registry `Hmd` ditambahkan | Catat pada decision log bahwa *Tindakan Lanjutan* nomor 1–4 sudah selesai |

---

## Pertanyaan Penutup

Audit ini tidak melahirkan pertanyaan baru untuk pemilik bisnis. Empat pertanyaan yang sudah
terbuka di `00-interview-decisions.md` tetap terbuka, dan audit ini justru mempersempit satu
di antaranya.

| ID | Pertanyaan | Apa yang berubah setelah audit |
|---|---|---|
| `HMD-OQ-000` | Siapa badan tata kelola klinis | Digeser menjadi gerbang `GO-LIVE` oleh `HMD-DEC-010`, bukan lagi blocker `DESIGN` |
| `HMD-OQ-003` | Item Pra-HD mana yang boleh di-*override* | Tidak berubah |
| `HMD-OQ-004` | Siapa berwenang memfinalisasi sesi | Sedikit menyempit: mekanisme penandatanganan sudah ada dan sudah punya pemeriksaan kewenangan sendiri, jadi pertanyaannya murni siapa orangnya, bukan bagaimana caranya |
| `HMD-OQ-007` | Siapa boleh menolak atau menahan permintaan HD | **Menyempit.** Kosakata statusnya tidak perlu diputuskan lagi — `Rejected`, `OnHold`, dan `CancelRequested` sudah ada pada pola `RadOrderStatus`. Yang tersisa hanya siapa orangnya dan apa alasan sah menolak |

Dua dependency lintas modul perlu disepakati dengan pemiliknya, dan keduanya bukan pertanyaan
bisnis melainkan koordinasi jadwal:

| ID | Dependency | Pemilik |
|---|---|---|
| `HMD-DEP-001` | Pembacaan hasil laboratorium per pasien per jenis pemeriksaan | Laboratorium |
| `HMD-DEP-002` | Pembacaan kewenangan klinis yang masih berlaku untuk seorang petugas | Human Resource / Credentialing |

---

## Langkah Berikutnya

`HMD-DEC-010` menggeser seluruh pertanyaan tata kelola klinis dari blocker `DESIGN` menjadi
gerbang `GO-LIVE`. Karena itu urutannya berubah:

1. **Jalankan `design-business-module`** dengan *Temuan Kritis 1–3* dan *Pola 1–3* sebagai
   masukan wajib.
2. Sampaikan `HMD-DEP-001` dan `HMD-DEP-002` ke pemilik Laboratorium dan Human Resource.
3. Kerjakan lima *Tindakan Lanjutan* registry pada `00-interview-decisions.md`.
4. Ajukan penunjukan badan klinis, lalu tutup `HMD-OQ-003`, `HMD-OQ-004`, dan `HMD-OQ-007`
   sebelum go-live.
