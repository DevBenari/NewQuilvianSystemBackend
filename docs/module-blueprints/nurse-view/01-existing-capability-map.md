# Peta Kemampuan Existing — Nurse View

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` |
| Revision | `1` |
| Status | `draft` |
| Owner | Rizki Gunawan |
| Masukan keputusan | [`00-interview-decisions.md`](./00-interview-decisions.md) revision `1` — `NV-DEC-004` s.d. `020`, `NV-AC-001` s.d. `016` |
| Backend SHA | `a6f977a3` (branch `rizkiG`) |
| Frontend SHA | `76b0c6e4b` (branch `RizkiV2`) |
| Tanggal audit | 2026-10-28 |
| Mode | `MODULE BLUEPRINT MODE` — source read-only; hanya `docs/module-blueprints/nurse-view/**` yang ditulis |

Audit ini membaca source dan **tidak mengubah apa pun**. Pertanyaannya satu: untuk setiap isi
display Nurse View, **data dan kemampuan apa yang sudah ada, dan mana yang benar-benar belum**.

Bukti ditulis `repo/path#simbol`. Seluruh bukti backend diambil pada `a6f977a3`, frontend pada
`76b0c6e4b`.

## Batas audit

| Di dalam audit | Di luar audit |
|---|---|
| Sumber data setiap elemen kartu bed dan header (`NV-DEC-006` s.d. `020`) | Isi data di database: jumlah kamar ber-`FloorCode`, isi roster, versi instrumen yang aktif |
| Pola akun perangkat display, login, dan otorisasinya | Perilaku runtime di TV sungguhan |
| SignalR `QueueHub` dan penerbit event | Kandidat slice berikutnya: tablet pasien, panggilan pasien, pesan makan |
| Pola route frontend tanpa sidebar | Integrasi alat monitor pasien |

## Ringkasan

| Status | Jumlah |
|---|---:|
| Ready to reuse | 4 |
| Reuse with adapter | 6 |
| Extend | 1 |
| Missing | 3 |
| Conflict | 2 |
| Unknown | 0 sebagai status kemampuan; 4 butir runtime di bagian Unknown |

## Temuan paling menentukan

1. **Rumah sakit belum menjalankan shift perawat.** `RWI-DEC-100` (Muhammad Hamzah,
   2026-09-11) mencatat sistem "tidak mempunyai konsep shift perawat sama sekali". Karena itu
   `InpatientClinicalContextService` **sengaja tidak memakai roster**. Header "perawat jaga shift
   berjalan per bangsal" (`NV-DEC-012`) bergantung pada data yang kemungkinan besar kosong.
   Ini menjawab `NV-OQ-011` dengan status **Conflict**.
2. **Koreksi analisa awal.** `00-analisa-awal.md` bagian 3 menyebut daftar dokter jaga IGD
   dibaca dari roster HR. Itu keliru: `AmbilDokterJagaAktifAsync` membaca `MstDoctorSchedule`,
   yaitu jadwal dokter, bukan roster. Untuk perawat, padanannya tidak ada.
3. **Proyeksi status bed sudah lengkap dan bisa dipakai apa adanya.**
   `InpBedManagementQueryService.ProjectBatchAsync` menghasilkan enam status bed dari
   penempatan, reservasi, dan pembersihan, dengan jumlah query tetap per permintaan.
4. **Identitas penghuni di proyeksi itu terikat pengguna, bukan perangkat.** Nama dan No. RM
   hanya diisi bila pengguna punya penugasan unit di `BedManagementOptions.UnitAssignments`.
   Akun perangkat tidak punya penugasan itu, sehingga ia akan menerima bed "Terisi" **tanpa**
   penghuni. Nurse View butuh proyeksi identitas sendiri yang langsung menghasilkan inisial.
5. **Realtime saat ini hanya untuk satu kejadian.** Satu-satunya penerbit rawat inap adalah
   `InpatientBedSelectionChanged` saat bed diambil (reservasi atau penempatan). Simpan TTV,
   pulang, pembersihan, dan penunjukan perawat **tidak menerbitkan event apa pun**. Push yang
   dijanjikan `NV-DEC-014` menuntut titik terbit baru di kode milik Clinical dan Rawat Inap.
6. **Login perangkat menyentuh berkas bersama.** Pola display antrean tersebar di
   `Controllers/AuthController.cs`, `Program.cs` (policy), `Constants/AuthorizationPolicies.cs`
   (daftar otorisasi alternatif yang disetujui), dan `proxy.js` di frontend. Pola ini juga
   **bukan** alur kode pairing seperti contoh `NV-DEC-009`/`016`, melainkan login yang dibuat
   admin dan kata sandinya ditampilkan sekali.
7. **Modul belum terdaftar.** `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` tidak
   memuat Nurse View. Menurut `QBE-MOD-002`, entity baru (master perangkat) berstatus `BLOCKED`
   sampai Area, Module, dan prefix-nya didaftarkan.

## Peta kemampuan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `NV-CAP-01` | Daftar bed per lantai, dikelompokkan per bangsal (`NV-DEC-006`) | Rawat Inap | `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpFloorPlanQueryService.cs#LoadAsync` | Reuse with adapter | (1) Kamar disaring `FloorPlanRoomKey != null && FloorCode != null`; `NV-DEC-006` hanya menuntut `FloorCode`. (2) Cakupan unit dari `InpBedAccessService.UnitsAsync`, yang bersandar pada pengguna (`ActorId`), bukan perangkat. Nurse View butuh pemuatan per lantai milik perangkat | Kamar tanpa `FloorPlanRoomKey` bisa hilang dari display tanpa terlihat |
| `NV-CAP-02` | Status bed: Terisi, Dipesan, Menunggu/Dalam Pembersihan, Tersedia, Tidak Tersedia (`NV-DEC-015`) | Rawat Inap | `…/InpBedManagementQueryService.cs#ProjectBatchAsync` (`internal`), `#StatusLabels` baris 15–18 | Ready to reuse | Pemetaan label: `Available` → "Kosong", `Reserved` → "Dipesan — pasien akan datang", `WaitingCleaning`/`Cleaning` → "Dibersihkan", `Unavailable` → "Tidak tersedia" | Rendah — satu sumber status, tidak dihitung ulang |
| `NV-CAP-03` | Penghuni bed dan identitas samaran: inisial, jenis kelamin, umur (`NV-DEC-008`) | Rawat Inap + Patient Management | `…/InpBedManagementQueryService.cs` baris 150–160 (`HoldingContext` diisi hanya bila `CanReadLoadedIdentityAsync`); `…/InpBedAccessService.cs#CanReadLoadedIdentityAsync`; `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` (`FullName`, `Gender`, `BirthDate`) | Reuse with adapter | Penghuni diambil dari `BedProjectionBatch.Placements`/`Reservations` yang sudah dimuat, lalu disamarkan **di backend**. `HoldingContext` tidak boleh diteruskan ke perangkat karena memuat nama lengkap dan No. RM | **Tinggi untuk privasi** bila adapter keliru memakai `HoldingContext` |
| `NV-CAP-04` | Perawat penanggung jawab episode (`NV-DEC-012`) | Rawat Inap | `…/InPatientManagement/Models/InpNurseAssignment.cs` (`IsActive`, `EndDateTime`); `InpatientEpisodeController` `POST {id}/nurse-assignments` | Ready to reuse | Dibaca langsung: baris aktif per episode, nama dari `MstEmployee` | Rendah |
| `NV-CAP-05` | Perawat jaga shift berjalan per bangsal di header (`NV-DEC-012`, `NV-OQ-011`) | HR Scheduling | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxShiftAssignment.cs` (`OrganizationUnitId`, `ShiftDate`, `ScheduledStartAt/EndAt`, `AssignmentStatus`); `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs#OrganizationUnitId`; `…/ClinicalManagement/Services/InpatientClinicalContextService.cs` baris 1236–1240; `rawat-inap/00-interview-decisions.md#RWI-DEC-100` | **Conflict** | Secara struktur bisa dipetakan (bangsal → `OrganizationUnitId` → `TrxShiftAssignment`), tetapi `RWI-DEC-100` menyatakan shift perawat belum dijalankan. Alternatif yang sudah dipakai Rawat Inap: penempatan organisasi `WfpOrganizationAssignment`. Itu menjawab "siapa perawat bangsal ini", bukan "siapa yang jaga sekarang" | Header bisa selalu kosong, atau menampilkan perawat yang tidak sedang jaga |
| `NV-CAP-06` | TTV dan EWS terakhir per episode (`NV-DEC-007`, `011`) | Clinical Management | `…/ClinicalManagement/Models/TrxPatientVitalSign.cs` (`InpEpisodeId` baris 231, `ObservationDateTime`, `EarlyWarningScore`, `EwsRiskLevel`, `IsCritical`, `VitalSignStatus`); `…/Controllers/PatientVitalSignController.cs` baris 559–564 (EWS dihitung saat simpan lewat `PatientVitalSignCalculation`); `…/Services/InpatientVitalSignService.cs` (episode diisi server) | Ready to reuse | Ambil baris terbaru per `InpEpisodeId`, kecuali `Draft`, `Cancelled`, `EnteredInError` | Rendah |
| `NV-CAP-07` | Peringatan kritis visual (`NV-DEC-013`) | Clinical Management | `…/PatientVitalSignController.cs#GetCriticalAlerts` baris 128–190 | Reuse with adapter | Pakai **kolomnya** (`IsCritical`, `EwsRiskLevel`) pada baris TTV terakhir, **bukan endpoint-nya**: endpoint itu mengembalikan `PatientName` dan `MedicalRecordNumber` serta tidak disaring per episode atau lantai | Sedang — endpoint yang salah dipakai akan membocorkan identitas |
| `NV-CAP-08` | Interval ukur ulang TTV per tingkat EWS (`NV-DEC-010`) | Nurse View (pengaturan baru) | `…/TrxPatientVitalSign.cs` hanya memuat `EwsMonitoringRecommendation` berupa teks; `DueAt` di `PatientAssessmentController` hanya untuk pengkajian | Missing | Pengaturan baru: empat angka (`Low`/`Medium`/`High`/`Critical`) beserta penanda "disahkan". Tanpa angka sah → aman-gagal | Angka adalah kebijakan klinis (`NV-OQ-010`) |
| `NV-CAP-09` | Ikon risiko jatuh tingkat Tinggi (`NV-DEC-018`) | Clinical Management / keperawatan | `…/ClinicalManagement/Models/CliAssessmentInstrumentResponse.cs` (`BandCode`, `IsAlertBand`); `…/Enums/ClinicalInstrumentKind.cs#FallRiskScale`; `…/Seeders/ClinicalInstrumentDraftSeeder.cs` baris 264–267, 316–320, 380–384 (pita `HIGH` satu-satunya dengan `IsAlertBand = true` pada Humpty Dumpty, Morse, Ontario); `…/Models/TrxPatientAssessment.cs#InpEpisodeId` | Reuse with adapter | Ambil hasil instrumen `FallRiskScale` terakhir yang sah per episode; tinggi = `BandCode == "HIGH"`. Ada sumber lama kedua, `TrxPatientAssessment.FallRiskStatus` (`HighRisk`); jalur rawat inap memakai instrumen (`FE-RWI-128`) | Seeder bernama **Draft**: bila versi instrumen belum diaktifkan, tidak ada hasil untuk dibaca (Unknown runtime) |
| `NV-CAP-10` | Ikon "ada alergi" (`NV-DEC-019`) | Clinical Management | `…/ClinicalManagement/Models/TrxPatientAllergy.cs` (`PatientId`, `AllergyStatus`); `…/Enums/PatientAllergyStatus.cs` (`Active = 1`) | Ready to reuse | Cukup "ada atau tidak" baris `Active` per pasien; nama zat tidak dikirim | Rendah |
| `NV-CAP-11` | Ikon isolasi + tanda "Bed tidak sesuai isolasi" (`NV-DEC-020`) | Rawat Inap | `…/InPatientManagement/Models/InpEpisode.cs#RequiresIsolation` baris 105; `…/Services/InpCensusQueryService.cs#GetIsolationMismatchAsync` baris 427–447 (aturan: `RequiresIsolation != Bed.IsIsolationBed`) | Reuse with adapter | Endpoint `isolation-mismatch` berupa daftar berhalaman; untuk kartu cukup aturan yang sama dihitung atas data yang sudah dimuat. Aturannya membandingkan **bed**, bukan kamar | Rendah; jenis isolasi belum ada (`NV-OQ-013`) |
| `NV-CAP-12` | Master perangkat display per lantai (`NV-DEC-016`) | Nurse View | Pola: `Areas/Administrator/MasterData/Models/MstQueueDisplayDevice.cs` (`NurseStationClusterId` wajib), `…/Controllers/QueueDisplayDeviceController.cs#GenerateQueueDisplayDeviceLogin`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` tanpa entri Nurse View | Missing | Entity baru meniru pola display antrean. **Terhalang `QBE-MOD-002`** sampai Area/Module/prefix didaftarkan | Blocker implementasi, bukan blocker desain |
| `NV-CAP-13` | Login dan otorisasi akun perangkat (`NV-DEC-009`) | Platform / Auth | `NewQuilvianSystemBackend/Controllers/AuthController.cs#ResolveQueueDisplayLoginContextAsync` (baris 1675; klaim `is_queue_display_account`); `Program.cs` baris 1294 (policy `QueueDisplayRuntimeRead`); `Constants/AuthorizationPolicies.cs#ApprovedAlternativeAuthorization`; `QuilvianSystemFrontendDev/src/proxy.js` baris 62–71 (redirect berdasarkan cookie `isQueueDisplayAccount`) | **Conflict** | Pola yang ada mengharuskan perubahan pada empat berkas bersama di luar modul. Ketentuan kerja proyek: jangan menambah kode ke `Program.cs`; daftar otorisasi alternatif adalah keputusan governance. Pola juga bukan alur kode pairing | Butuh keputusan pemilik governance |
| `NV-CAP-14` | Pembaruan push + muat ulang 60 detik (`NV-DEC-014`) | Platform realtime + Rawat Inap + Clinical | `NewQuilvianSystemBackend/Hubs/QueueHub.cs` (`JoinQueueDisplayCluster` baris 116 sebagai pola join akun display; `JoinInpatientBedSelection` baris 52); `…/InPatientManagement/Services/InpBedRealtimeService.cs` (event tunggal `InpatientBedSelectionChanged`, hanya dari `InpBedOccupancyService`) | Extend | Perlu (1) grup per lantai di hub yang sama; (2) titik terbit baru saat TTV disimpan/dibatalkan, pasien pulang/pindah, pembersihan, dan penunjukan perawat. Titik (2) berada di kode milik modul lain | Tanpa (2), push hanya berlaku untuk reservasi/penempatan; sisanya menunggu muat ulang 60 detik |
| `NV-CAP-15` | Halaman TV tanpa sidebar, pita data basi, sambung ulang otomatis (`NV-DEC-009`, `014`, `017`) | Frontend platform | `QuilvianSystemFrontendDev/src/app/queue-display/layout.jsx`, `page.jsx`; `src/lib/hooks/queue-display/use-queue-display-screen.jsx` (timer muat ulang baris 1152, `lastUpdatedAt` baris 407, pendengar `offline` baris 1506); `src/lib/realtime/queue-realtime-client.js` (`withAutomaticReconnect` baris 217, `onreconnected`, `onclose`) | Reuse with adapter | Ditiru polanya, tidak disalin seluruhnya (layar antrean 1.999 baris). Pita basi > 2 menit belum ada; yang ada baru jam pembaruan terakhir | Rendah |
| `NV-CAP-16` | Melaporkan kamar tanpa `FloorCode` (`NV-DEC-006`) | Master Data HealthServices | `…/InpFloorPlanQueryService.cs#LoadAsync` membuang kamar tanpa `FloorCode` tanpa laporan | Missing | Daftar periksa admin kecil, atau cukup langkah operasional sebelum go-live | Rendah; bisa jadi catatan operasional, bukan kartu kode |

## Kontrak as-is yang akan dibaca

Endpoint di bawah **sudah ada**. Nurse View tidak memanggilnya dari TV; yang dipakai ulang
adalah service atau query di belakangnya, karena seluruh endpoint ini ditujukan untuk pengguna
manusia dengan `[AccessPermission]`, bukan akun perangkat.

### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/floor-plan` | Denah satu lantai: kamar, status kamar, bed enam status | `InpatientBedManagement : Read` | query `FloorPlanMonitoringQuery` (`FloorCode`) | `FloorPlanResponse` |
| `GET` | `/monitoring` | Daftar bed dan statusnya | `InpatientBedManagement : Read` | query `BedMonitoringQuery` | daftar bed operasional |

Kode `403` berarti pengguna tidak punya hak membaca Bed Management.

### Health Services / Clinical Management / Patient Vital Sign

Base URL: `api/v1/health-services/clinical-management/patient-vital-signs`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/episodes/{episodeId}` | Deret TTV satu episode, bawaan 24 jam, maksimal 7 hari | `PatientVitalSign : Read` | `from`, `to` | `List<PatientVitalSignSeriesItem>` |
| `GET` | `/critical-alerts` | Peringatan TTV abnormal/kritis | `PatientVitalSign : Read` | `patientId`, `encounterId`, `onlyNeedDoctorNotification` | `List<PatientVitalSignAlertResponse>` — **memuat nama dan No. RM** |

Kode `400` pada deret berarti rentang lebih dari 7 hari.

### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/isolation-mismatch` | Episode yang kebutuhan isolasinya tidak cocok dengan bed | `InpatientMonitoring : Read` | `IsolationMismatchQuery` | `IsolationMismatchPagedResult` |
| `GET` | `/unassigned-nurse-episodes` | Episode aktif tanpa perawat PJ | `InpatientMonitoring : Read` | `InpatientMonitoringQuery` | `UnassignedNursePagedResult` |

### Health Services / Registration Management / Queue Display Runtime

Base URL: `api/v1/health-services/registration-management/queue-display-runtime`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/current` | Identitas perangkat yang sedang login; mencatat waktu online | policy `QueueDisplayRuntimeRead` (tanpa `[AccessPermission]`) | — | `QueueDisplayRuntimeCurrentResponse` |

Ini **pola** untuk runtime perangkat Nurse View, bukan endpoint yang dipakai. Kode `404` berarti
akun yang login tidak tertaut ke perangkat display mana pun.

## Contoh alur data satu kartu

Pukul 10.05 perawat Sari menyimpan TTV pasien Budi Santoso (L, 64 th) di bed 301-A, lantai 3,
Bangsal Bedah:

1. `PatientVitalSignController` menghitung EWS = 6, `EwsRiskLevel = Medium`, dan menyimpan baris
   dengan `InpEpisodeId` episode Budi (`NV-CAP-06`).
2. **Hari ini tidak ada event yang terbit** (`NV-CAP-14`). TV lantai 3 baru melihatnya pada muat
   ulang berikutnya, paling lambat 60 detik.
3. Saat dimuat, status 301-A = `Occupied` dari `ProjectBatchAsync` (`NV-CAP-02`). Penghuninya
   diambil dari penempatan aktif lalu disamarkan menjadi "B.S. · L · 64 th" (`NV-CAP-03`).
4. Perawat PJ "Sari" dari `InpNurseAssignment` aktif (`NV-CAP-04`).
5. Ikon kuning tampil bila hasil instrumen risiko jatuh terakhir `BandCode = HIGH`
   (`NV-CAP-09`). Ikon merah "Alergi" tampil bila Budi punya alergi `Active` (`NV-CAP-10`).
6. Tanda "lewat jadwal" **tidak** tampil sampai interval `Medium` disahkan (`NV-CAP-08`).

## Conflict

| ID | Conflict | Pihak | Yang perlu diputuskan |
|---|---|---|---|
| `NV-CON-001` | Header perawat jaga (`NV-DEC-012`) bertumpu pada shift perawat, padahal `RWI-DEC-100` menyatakan shift perawat belum dijalankan | Rizki Gunawan; fakta dari Muhammad Hamzah | Sumber header: roster (bila nanti diisi), penempatan bangsal, atau tanpa header di MVP |
| `NV-CON-002` | Login perangkat menuntut perubahan `AuthController`, `Program.cs`, daftar otorisasi alternatif, dan `proxy.js`; contoh pairing di `NV-DEC-009`/`016` berbeda dari pola login yang dibuat admin | Rizki Gunawan; pemilik governance/QBE | Pola login perangkat, dan izin menyentuh berkas bersama |

## Unknown — hanya terjawab lewat lingkungan berjalan

| ID | Hal | Cara menjawab |
|---|---|---|
| `NV-UNK-001` | Apakah semua kamar rawat inap aktif punya `FloorCode` (dan `FloorPlanRoomKey`) | Cek master kamar di basis data lokal (`NV-OQ-012`) |
| `NV-UNK-002` | Apakah versi instrumen risiko jatuh sudah aktif, bukan `Draft` | Cek `CliClinicalInstrumentVersion.VersionStatus` |
| `NV-UNK-003` | Apakah `TrxShiftAssignment` memuat perawat sama sekali | Cek roster di basis data lokal |
| `NV-UNK-004` | Apakah `MstServiceUnit.OrganizationUnitId` bangsal rawat inap sudah terisi | Cek master unit; prasyarat yang sama dengan `RWI-DEC-100` |

## Pertanyaan penutup untuk `grill-me` closure pass

| ID | Pertanyaan | Terkait |
|---|---|---|
| `NV-CQ-001` | Header perawat: roster, penempatan bangsal, atau ditiadakan di MVP? | `NV-CON-001`, `NV-DEC-012` |
| `NV-CQ-002` | Pola login perangkat dan izin menyentuh berkas bersama | `NV-CON-002`, `NV-DEC-009`, `016` |
| `NV-CQ-003` | Push untuk TTV, pulang, pembersihan, dan penunjukan perawat menuntut titik terbit di kode modul lain. Diminta ke pemiliknya, atau MVP cukup muat ulang 60 detik? | `NV-CAP-14`, `NV-DEC-014` |
| `NV-CQ-004` | Area, Module, dan prefix Nurse View untuk registry | `NV-CAP-12`, `QBE-MOD-002` |
| `NV-CQ-005` | Kamar ber-`FloorCode` tetapi tanpa `FloorPlanRoomKey`: ikut tampil atau tidak? | `NV-CAP-01`, `NV-DEC-006` |

## Pemicu impact scan

Peta ini dinyatakan basi dan perlu impact scan terbatas bila salah satu berubah:

- `InpBedManagementQueryService.ProjectBatchAsync`, `InpBedAccessService`, `InpFloorPlanQueryService`;
- `TrxPatientVitalSign`, `PatientVitalSignCalculation`, `InpatientVitalSignService`;
- `CliAssessmentInstrumentResponse` atau pita instrumen `FallRiskScale`;
- `QueueHub`, `InpBedRealtimeService`, `AuthController`, policy di `Program.cs`, `proxy.js`;
- keputusan baru Rawat Inap tentang shift perawat (pengganti `RWI-DEC-100`);
- SHA backend berbeda dari `a6f977a3` atau SHA frontend berbeda dari `76b0c6e4b`.
