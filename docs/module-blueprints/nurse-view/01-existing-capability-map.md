# Peta Kemampuan Existing — Nurse View

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` |
| Revision | `2` — impact scan 2026-11-05 untuk Amendment revision 5 (tablet bed dan panggilan pasien); lihat [Impact scan revision 2](#impact-scan-revision-2--tablet-bed-dan-panggilan-pasien). Sebelumnya `1` — audit 2026-10-28 |
| Status | `draft` |
| Owner | Rizki Gunawan |
| Masukan keputusan | [`00-interview-decisions.md`](./00-interview-decisions.md) revision `1` — `NV-DEC-004` s.d. `020`, `NV-AC-001` s.d. `016`; revision `5` (draft) — `NV-DEC-034` s.d. `049`, `NV-AC-028` s.d. `041` untuk impact scan |
| Backend SHA | `a6f977a3` (revision 1); `0c6eab34` (impact scan revision 2) — branch `rizkiG` |
| Frontend SHA | `76b0c6e4b` (revision 1); `91a11b08f` (impact scan revision 2) — branch `RizkiV2` |
| Tanggal audit | 2026-10-28; impact scan 2026-11-05 |
| Mode | `MODULE BLUEPRINT MODE` — source read-only; hanya `docs/module-blueprints/nurse-view/**` yang ditulis |

Audit ini membaca source dan **tidak mengubah apa pun**. Pertanyaannya satu: untuk setiap isi
display Nurse View, **data dan kemampuan apa yang sudah ada, dan mana yang benar-benar belum**.

Bukti ditulis `repo/path#simbol`. Seluruh bukti backend diambil pada `a6f977a3`, frontend pada
`76b0c6e4b`.

> **Impact scan revision 2 (2026-11-05).** Bagian revision 1 di bawah tetap berlaku untuk papan
> pemantauan dan tidak disunting. Kemampuan untuk tablet bed dan panggilan pasien, beserta
> status baris lama yang berubah karena 14 task MVP, ada di
> [Impact scan revision 2](#impact-scan-revision-2--tablet-bed-dan-panggilan-pasien). Bukti
> impact scan diambil pada backend `0c6eab34` dan frontend `91a11b08f`.

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

## Impact scan revision 2 — tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| Tanggal | 2026-11-05 |
| Mode | Impact scan terbatas, source read-only; hanya `docs/module-blueprints/nurse-view/**` yang ditulis |
| Pemicu | SHA berubah (backend 80 commit, frontend 62 commit sejak revision 1) dan Amendment revision 5 (`NV-DEC-034`–`049`) |
| Backend SHA | `0c6eab34` (branch `rizkiG`) |
| Frontend SHA | `91a11b08f` (branch `RizkiV2`) |
| Kontrak yang berlaku | `0.1.2` — belum ada kontrak untuk panggilan pasien |

Bukti di bagian ini ditulis `repo/path#simbol` pada kedua SHA di atas. `BE` = `NewQuilvianSystemBackend`,
`FE` = `QuilvianSystemFrontendDev`.

### Batas audit

| Di dalam audit | Di luar audit |
|---|---|
| Akun perangkat per bed: pola login, klaim, policy, dan halaman tanpa sidebar | Hardware tablet, mode kiosk browser, dan jaringan Wi-Fi kamar |
| Pembacaan penempatan Rawat Inap untuk mengunci tablet dan mendeteksi pindah atau pulang | Aturan internal transfer dan pemulangan milik Rawat Inap |
| Grup realtime `QueueHub` untuk TV, konsol, dan tablet | Bel kabel dan integrasinya (`NV-DEC-037`) |
| Izin baru untuk Konsol Panggilan lewat Akses Role | Isi data di database bersama |
| Pola penyimpanan: satu panggilan aktif per bed, klaim bersamaan, kirim ulang | Laporan waktu tanggap (`NV-DEC-049`, slice berikutnya) |
| Bunyi di browser untuk layar staf | Perilaku runtime di perangkat sungguhan |

### Perubahan sejak revision 1 pada berkas pemicu

| Berkas | Yang berubah | Dampak ke peta revision 1 |
|---|---|---|
| `BE/Controllers/AuthController.cs`, `BE/Program.cs`, `BE/Constants/AuthorizationPolicies.cs`, `BE/Hubs/QueueHub.cs` | Penambahan Nurse View sendiri (14 task MVP) dan penambahan modul lain dari integration (antrean admisi, optik, kelengkapan rekam medis) | Tidak mengubah pola yang dipakai Nurse View. Pola login display antrean tetap sama |
| `FE/src/proxy.js`, `FE/src/lib/hooks/auth/use-base-login.jsx`, `FE/src/lib/realtime/queue-realtime-client.js` | Penambahan Nurse View dan antrean admisi | Sama |
| `BE/…/InPatientManagement/Services/InpAdmissionTransferService.cs`, `InpDischargeService.Closure.cs` | Penolakan transfer IGD → rawat inap (`BE-IGD-077`) dan kait kelengkapan rekam medis | Tidak menyentuh pengakhiran penempatan bed |
| `InpBedManagementQueryService`, `InpBedAccessService`, `InpFloorPlanQueryService`, `TrxPatientVitalSign`, `InpatientVitalSignService`, `InpBedRealtimeService` | Tidak berubah | — |

**Status baris revision 1 sesudah 14 task MVP:**

| ID | Status revision 1 | Keadaan sekarang | Bukti |
|---|---|---|---|
| `NV-CAP-05` | Conflict | Ditutup `NV-DEC-021`/`033`; header membaca roster dengan cadangan unit periode dan profesi pegawai | `BE/Areas/HealthServices/NurseViewManagement/Services/NvwFloorBoardQueryService.cs` baris 170 (`MapCpptProfessionType`) |
| `NV-CAP-08` | Missing | Sudah ada | `BE/Areas/HealthServices/NurseViewManagement/Models/NvwVitalSignInterval.cs` |
| `NV-CAP-12` | Missing | Sudah ada | `BE/Areas/HealthServices/NurseViewManagement/Models/NvwDisplayDevice.cs`; registry `Nvw` `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 45) |
| `NV-CAP-13` | Conflict | Ditutup `NV-DEC-030`; sudah diterapkan | `BE/Controllers/AuthController.cs#ResolveNurseViewLoginContextAsync` baris 1882; klaim baris 2062–2070 |
| `NV-CAP-14` | Extend | Push TTV sudah ada (`NV-DEC-026`) | `BE/Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` baris 592, 732, 879 memanggil `NvwRealtimeService.NotifyVitalSignChangedAsync` |

### Ringkasan

| Status | Jumlah |
|---|---:|
| Ready to reuse | 5 |
| Reuse with adapter | 4 |
| Extend | 1 |
| Missing | 2 |
| Conflict | 1 |
| Unknown | 0 sebagai status kemampuan; 2 butir runtime di bagian Unknown |

### Temuan paling menentukan

1. **Akhir penempatan bed tercatat jelas, tetapi tidak ada kabar ke modul lain.** Saat pasien
   pindah atau pulang, Rawat Inap mengisi `EndDateTime` dan `EndReason` pada `InpBedPlacement`.
   Tidak ada event di dalam proses yang bisa didengar Nurse View. Satu-satunya siaran,
   `InpatientBedSelectionChanged`, ditujukan ke layar pengguna dan memuat nama pasien
   (`NV-DEC-026`). Jadi Nurse View harus **membaca** akhir penempatan sendiri untuk menutup
   panggilan (`NV-DEC-046`), tanpa mengubah kode Rawat Inap.
2. **Akun perangkat TV belum membedakan jenis perangkat.** `ResolveActiveDeviceAsync`, policy
   `NurseViewDisplayRead`, dan `JoinNurseViewFloor` hanya memeriksa "akun ini perangkat Nurse
   View yang aktif". Bila tablet bed disimpan di tabel perangkat yang sama tanpa pembeda, tablet
   pasien bisa membaca **seluruh papan lantai** dan bergabung ke grup lantai. Itu membocorkan
   inisial pasien lain ke tablet di samping bed.
3. **Endpoint untuk perangkat wajib memakai policy bernama.** Daftar endpoint tanpa penegakan
   izin sengaja kosong (`PermissionRegistryDescriptor.KnownUnenforcedBusinessEndpoints`). Endpoint
   tablet tidak bisa memakai `[AccessPermission]`, karena akun perangkat tidak punya peran, jadi
   ia butuh policy bernama di `AuthorizationPolicies` dan `Program.cs`. Keduanya berkas bersama,
   sehingga ini memerlukan restu lead (`NV-OQ-028`).
4. **Izin konsol bisa memakai mekanisme yang ada, dengan satu syarat.** Baris izin di Akses Role
   dibuat otomatis dari atribut `[AccessPermission]` dan `[AccessAction]` pada endpoint. Izin
   yang hanya diperiksa di hub, tanpa endpoint REST, tidak akan pernah punya baris untuk
   dicentang admin.
5. **Ada sisa prototipe panggilan pasien di frontend.** `nurseCallContext.jsx` dibuat Oktober 2025,
   menyimpan panggilan di `localStorage` browser, dan sudah tidak dipakai halaman mana pun.
   Prototipe ini tidak boleh dipakai ulang, karena datanya hanya ada di satu browser.

### Peta kemampuan amendment

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `NV-CAP-17` | Bed → kamar → lantai untuk tablet dan strip TV (`NV-DEC-035`) | Master Data HealthServices | `BE/Areas/HealthServices/MasterData/Models/MstBed.cs#RoomId`; `MstRoom.FloorCode`; pola join di `BE/Areas/HealthServices/NurseViewManagement/Services/NvwRealtimeService.cs#NotifyVitalSignChangedAsync` baris 43–56 | Ready to reuse | Lantai sebuah bed dibaca dari kamarnya, sama seperti papan | Rendah; kamar tanpa `FloorCode` tetap tidak tampil (`NV-UNK-001`) |
| `NV-CAP-18` | Penghuni aktif satu bed, untuk mengunci tablet atau menampilkan pasiennya (`NV-DEC-046`) | Rawat Inap | `BE/Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs` (`BedId`, `EndDateTime` baris 40, `IsActive`); `BE/Repositories/Configurations/HealthServices/InPatientManagement/InpBedPlacementConfiguration.cs` baris 34–37 (`IX_InpBedPlacement_BedId_Active`, `INV-INP-02`: satu bed paling banyak satu penempatan aktif) | Ready to reuse | Baca penempatan dengan `BedId` tablet dan `EndDateTime IS NULL`. Tidak ada → tablet "Bed belum berpenghuni" | Rendah |
| `NV-CAP-19` | Deteksi pasien pindah atau pulang untuk menutup panggilan (`NV-DEC-046`) | Rawat Inap | `BE/…/InPatientManagement/Enums/InpBedPlacementEndReason.cs` (`Transfer`, `EpisodeClosed`, `AdmissionCancelled`, `PatientDeparted`, `CorrectedEntry`); pengakhiran di `BE/…/InPatientManagement/Services/InpBedOccupancyService.cs#TransferCoreAsync` (baris 1020), `#ReleaseActivePlacementAsync` (baris 1850), `InpEpisodeService.cs#ReleaseBedHoldsAsync` (baris 1169); `InpBedRealtimeService.cs` hanya menyiarkan ke klien SignalR | Reuse with adapter | Panggilan menyimpan penempatan saat dibuat. Panggilan aktif ditutup bila penempatan itu sudah berakhir; alasannya dipetakan dari `EndReason`. Pemeriksaan dilakukan Nurse View sendiri (saat dibaca dan/atau berkala), tanpa titik terbit baru di kode Rawat Inap. Pola pekerja berkala sudah ada (`BackgroundService`, misalnya `AttendanceSchedulerHostedService`) | Sedang: (1) ada jeda sampai pemeriksaan berikutnya; (2) alasan untuk `AdmissionCancelled` dan `CorrectedEntry` belum diputuskan (`NV-CQ-006`); (3) transfer yang dicatat sebelum pasien benar-benar berpindah (`NV-UNK-006`) |
| `NV-CAP-20` | Akun perangkat tablet bed: daftar, buat login, atur ulang, nonaktifkan (`NV-DEC-035`) | Nurse View + Platform/Auth | `BE/Areas/HealthServices/NurseViewManagement/Services/NvwDisplayDeviceService.cs#CreateLoginAsync` (baris 235), `#ResetLoginAsync` (307), `#DeactivateAsync` (368); `NvwDeviceContextService.cs#ResolveLoginContextAsync` (baris 33), `#ResolveActiveDeviceAsync` (56, tanpa cek jenis perangkat); `BE/Program.cs` baris 1473–1481 (policy `NurseViewDisplayRead` hanya mensyaratkan klaim `nurse_view_device_id` terisi); `BE/Hubs/QueueHub.cs#JoinNurseViewFloor` (baris 388) | Reuse with adapter | Alur login perangkat ditiru (kata sandi tampil sekali, sesi berstempel, nonaktif langsung ditolak). Perangkat tablet **wajib** terikat ke `BedId` dan dibedakan dari TV, supaya `ResolveActiveDeviceAsync`, `NurseViewDisplayRead`, dan `JoinNurseViewFloor` tidak menerimanya. Endpoint tablet butuh policy bernama sendiri | **Tinggi untuk privasi** bila tablet lolos sebagai TV (temuan no. 2). Menyentuh berkas bersama → `NV-OQ-028` |
| `NV-CAP-21` | Halaman tablet tanpa sidebar dan pengalihan setelah login | Frontend platform | `FE/src/components/providers/root-layout-shell.jsx` baris 18–20, `FE/src/components/ui/auth-wrapper.jsx` baris 21–23, `FE/src/components/providers/session-activity-manager.jsx` baris 30–32 (daftar prefix `/kiosk`, `/queue-display`, `/nurse-view-display`); `FE/src/proxy.js` baris 3–5; `FE/src/lib/hooks/auth/use-base-login.jsx` | Reuse with adapter | Satu route tablet ditambahkan ke kelima berkas, satu entri per berkas, persis pola `NV-DEC-032` | Berkas bersama → `NV-OQ-028` |
| `NV-CAP-22` | Izin Konsol Panggilan lewat Akses Role (`NV-DEC-043`) | Platform/Auth + Nurse View | `BE/Attributes/AccessPermissionAttribute.cs`; `BE/Seeders/AccessMenuSeeder.cs` baris 39 (`PermissionRegistryDescriptor.Build`); `BE/Services/Security/PermissionRegistryValidator.cs` (gerbang integritas saat startup, `Program.cs` baris 2355); `BE/Areas/HealthServices/NurseViewManagement/Constants/NurseViewPermissions.cs` | Ready to reuse | Resource baru ditambahkan di `NurseViewPermissions` dan dipasang pada endpoint konsol; baris Akses Role terbit sendiri. Pemeriksaan di hub memakai resource yang sama | Izin yang hanya dicek di hub tanpa endpoint REST tidak punya baris (temuan no. 4). Menyunting Akses Role menimpa seluruh policy peran; ini catatan operasional |
| `NV-CAP-23` | Grup realtime untuk strip TV, Konsol Panggilan, dan tablet bed | Platform realtime + Nurse View | `BE/…/NurseViewManagement/Services/NvwRealtimeService.cs` (grup `nurse-view-floor:{lantai}`, event `NurseViewBoardChanged` berisi `EventType`, `BedId`, `FloorCode`, `OccurredAtUtc` tanpa identitas); `BE/Hubs/QueueHub.cs#JoinAdmissionQueueBoard` baris 116 (pola join berizin: `RejectQueueDisplayDeviceAccount` + `HasAccessAsync`); `FE/src/lib/realtime/queue-realtime-client.js#joinNurseViewFloor` baris 578; `FE/src/lib/hooks/nurse-view-display/use-nurse-view-floor-board.jsx#scheduleRealtimeLoad` | Extend | (1) Strip TV: grup lantai yang ada bisa dipakai dengan jenis event baru, tetapi TV saat ini memuat ulang **seluruh papan** untuk setiap event, paling sering sekali per 5 detik (`NV-FCT-002`); strip butuh penanganan sendiri. (2) Konsol: join baru berizin untuk pengguna manusia; `JoinNurseViewFloor` menolak akun non-perangkat. (3) Tablet: grup per bed belum ada | `QueueHub` berkas bersama → `NV-OQ-028`. Payload ke TV dan tablet tidak boleh memuat identitas (`NV-DEC-044`) |
| `NV-CAP-24` | Data panggilan: satu panggilan aktif per bed, klaim bersamaan, kirim ulang tanpa dobel (`NV-DEC-038`–`041`, `NV-AC-039`) | Nurse View (data baru) | Tidak ada entity panggilan (`NV-FCT-003`). Pola yang tersedia: indeks unik berfilter `IX_InpBedPlacement_BedId_Active` (`HasFilter("\"EndDateTime\" IS NULL")`); token konkurensi `Version` di `BE/Repositories/Configurations/HealthServices/NurseViewManagement/NvwDisplayDeviceConfiguration.cs` baris 26; header `Idempotency-Key` di `BE/Areas/Corporate/FinanceManagement/PatientRefund/Controllers/FinancePatientRefundsController.cs` baris 106 dengan indeks unik `IdempotencyKey` | Missing | Entity baru di prefix `Nvw` yang sudah `ACTIVE` (registry baris 45), jadi `QBE-MOD-002` tidak menghalangi. Nama `NurseCall` tidak boleh dipakai (`NV-FCT-004`) | Rendah bila ketiga pola dipakai: tanpa indeks berfilter bisa muncul dua panggilan aktif per bed; tanpa token konkurensi dua klaim bisa sama-sama diterima |
| `NV-CAP-25` | Jejak klaim, ambil alih, lepas, dan nama pengklaim (`NV-DEC-040`, `049`) | Nurse View (data baru) + Platform | `BE/Models/ApplicationUser.cs` (`DisplayName` baris 12, `EmployeeId` baris 18) | Missing | Tabel jejak baru. Nama pengklaim dibaca dari pengguna yang login, bukan diketik | Rendah |
| `NV-CAP-26` | Inisial, jenis kelamin, dan umur di Konsol Panggilan (`NV-DEC-044`) | Nurse View | `BE/Areas/HealthServices/NurseViewManagement/Helpers/NvwPatientMasking.cs` (`Initials`, `GenderCode`, `AgeLabel`) | Ready to reuse | Helper yang sama dengan kartu bed, dijalankan di backend | Rendah |
| `NV-CAP-27` | Bunyi Konsol Panggilan: per panggilan baru, nada Darurat, pengingat, tunda (`NV-DEC-045`) | Frontend | `FE/src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-list.jsx#playArrivalSound` (bunyi saat event realtime di layar staf, `/audio/queue-dingdong.mp3`); `FE/src/lib/hooks/queue-display/use-queue-display-screen.jsx` baris 530–559, 771 (deteksi autoplay diblokir dan pesan "Klik tombol suara satu kali") | Reuse with adapter | Cara memutar bunyi dan menangani autoplay diblokir ditiru. Nada Darurat, pengulangan pengingat, dan tunda bunyi belum ada | Sedang: browser bisa menolak bunyi sebelum ada klik; konsol perlu tombol "Aktifkan suara" seperti layar antrean |
| `NV-CAP-28` | Halaman Konsol Panggilan berlogin, dengan menu berizin | Frontend | `FE/src/utils/menu-sidebar/menu-items.jsx` (pola `requiredPermission` dipakai banyak menu); `FE/src/lib/hooks/health-services/registration-management/admission-queues/use-admission-queue-realtime.jsx` baris 39 (join grup berizin dari layar staf) | Ready to reuse | Menu baru dengan `requiredPermission` ke resource konsol | Rendah. Catatan: dua menu Nurse View yang ada (baris 90–98) belum memakai `requiredPermission`; di luar scope amendment |
| `NV-CAP-29` | Sisa prototipe panggilan pasien di frontend | Frontend (Rizki Gunawan) | `FE/src/lib/helpers/nurseCallContext.jsx` (161 baris; `NurseCallProvider`, `localStorage` kunci `nurseCalls`, `AudioContext`); commit `2c5836d13`, `cedbb656f`, `a626a8569` (Oktober 2025). Halaman `src/app/Display-Nurse-View/**` dan hook `useNurseCall.jsx` dari commit yang sama sudah dihapus; tidak ada `import` yang tersisa | **Conflict** | Tidak boleh dipakai ulang: data panggilan hanya tersimpan di satu browser, tanpa backend dan tanpa izin. Namanya juga memakai `NurseCall` (`NV-FCT-004`) | Developer bisa keliru memakainya sebagai dasar; keputusan hapus atau biarkan → `NV-CQ-007` |

### Kontrak as-is yang terkait

Endpoint di bawah **sudah ada**. Endpoint panggilan pasien belum ada; bentuknya ditetapkan
`design-business-module`.

#### Health Services / Nurse View Management / Display Device

Base URL: `api/v1/health-services/nurse-view-management/display-devices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar perangkat display | `NurseViewDisplayDevice : Read` | query saringan dan halaman | `PagedResult<NvwDisplayDeviceResponse>` |
| `GET` | `/floors` | Pilihan lantai | `NurseViewDisplayDevice : Read` | — | `List<NvwFloorOptionResponse>` |
| `GET` | `/{id}` | Rincian perangkat | `NurseViewDisplayDevice : Read` | — | `NvwDisplayDeviceDetailResponse` |
| `POST` | `/` | Daftarkan perangkat | `NurseViewDisplayDevice : Create` | kode, nama, lantai | `NvwDisplayDeviceDetailResponse` |
| `PUT` | `/{id}` | Ubah perangkat | `NurseViewDisplayDevice : Update` | nama, lokasi, keterangan | `NvwDisplayDeviceDetailResponse` |
| `POST` | `/{id}/login` | Buat login; kata sandi tampil sekali | `NurseViewDisplayDevice : Update` | — | `NvwDisplayDeviceLoginResponse` |
| `POST` | `/{id}/login/reset` | Atur ulang login | `NurseViewDisplayDevice : Update` | — | `NvwDisplayDeviceLoginResponse` |
| `PATCH` | `/{id}/deactivate` | Nonaktifkan perangkat | `NurseViewDisplayDevice : Update` | alasan | `NvwDisplayDeviceDetailResponse` |

Ini pola untuk pengelolaan tablet bed. Bedanya, tablet terikat ke bed, bukan ke lantai.

#### Health Services / Nurse View Management / Display Runtime

Base URL: `api/v1/health-services/nurse-view-management/display-runtime`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/current` | Identitas TV yang sedang login | policy `NurseViewDisplayRead` | — | `NvwDisplayRuntimeCurrentResponse` |
| `GET` | `/board` | Papan satu lantai | policy `NurseViewDisplayRead` | — | papan per bangsal |

Kode `403` berarti akun bukan perangkat display aktif, atau sesinya sudah diganti login baru.
**Tablet bed tidak boleh lolos dari kedua endpoint ini** (temuan no. 2).

### Contoh alur panggilan dengan kemampuan yang ada

Pukul 10.00 pasien bed 331-C, lantai 3, menekan Darurat:

1. Tablet 331-C masuk dengan akun perangkatnya. Polanya meniru `NV-CAP-20`, tetapi perangkatnya
   terikat ke bed 331-C, dan policy-nya terpisah dari TV.
2. Backend memastikan bed 331-C punya penempatan aktif (`NV-CAP-18`). Hasilnya ada, jadi
   panggilan dicatat bersama penempatan tersebut. Indeks berfilter menjamin bed itu tidak punya
   dua panggilan aktif (`NV-CAP-24`).
3. Lantai 3 dibaca dari kamar 331 (`NV-CAP-17`). Sinyal tanpa identitas dikirim ke grup lantai 3
   dan grup konsol lantai 3 (`NV-CAP-23`).
4. Konsol di nurse station berbunyi dengan nada Darurat (`NV-CAP-27`) dan menampilkan
   "331-C · R.A. · P · 72 th · DARURAT" (`NV-CAP-26`).
5. Sari dan Dewi menekan "Saya tangani" hampir bersamaan. Token konkurensi hanya menerima yang
   pertama (`NV-CAP-24`), dan jejak mencatat nama Sari dari akun login-nya (`NV-CAP-25`).
6. Pukul 10.05 pasien dipindah ke 402-B sebelum Sari tiba. Penempatan 331-C berakhir dengan
   `EndReason = Transfer`. Pada pemeriksaan berikutnya, Nurse View menutup panggilan dengan alasan
   "pasien pindah" (`NV-CAP-19`).

### Conflict

| ID | Conflict | Pihak | Yang perlu diputuskan |
|---|---|---|---|
| `NV-CON-003` | Prototipe `nurseCallContext.jsx` memakai nama `NurseCall` dan penyimpanan `localStorage`, bertentangan dengan `NV-DEC-039`–`049` (data panggilan di server, satu sumber untuk TV, konsol, dan tablet) | Rizki Gunawan (pembuat dan pemilik modul) | Dihapus dalam task frontend amendment, atau dibiarkan dengan catatan larangan pakai (`NV-CQ-007`). **Ditutup 2026-11-05 — `NV-DEC-051`: dihapus di task frontend amendment** |

### Unknown — hanya terjawab lewat lingkungan berjalan

| ID | Hal | Cara menjawab |
|---|---|---|
| `NV-UNK-006` | Apakah petugas mencatat pindah bed tepat saat pasien benar-benar berpindah. Bila dicatat lebih awal, tablet lama terkunci selagi pasien masih di sana | Tanya kepala ruang tentang praktik pencatatan transfer; selama ini bel kabel tetap tersedia (`NV-DEC-037`) |
| `NV-UNK-007` | Kemampuan tablet: browser yang dipakai, layar selalu menyala, mode kiosk, dan izin bunyi | Uji pada satu tablet contoh sebelum pengadaan |

### Pertanyaan penutup untuk `grill-me` closure pass

| ID | Pertanyaan | Terkait |
|---|---|---|
| `NV-CQ-006` | Penempatan juga bisa berakhir karena `AdmissionCancelled` (penempatan dibatalkan) dan `CorrectedEntry` (koreksi salah catat). Alasan penutupan apa yang dicatat untuk keduanya, dan apakah koreksi salah catat juga menutup panggilan? | `NV-CAP-19`, `NV-DEC-046`. **Ditutup 2026-11-05 — `NV-DEC-050`: keduanya tidak menutup panggilan otomatis** |
| `NV-CQ-007` | Prototipe `nurseCallContext.jsx` dihapus dalam task frontend amendment atau dibiarkan? | `NV-CAP-29`, `NV-CON-003`. **Ditutup 2026-11-05 — `NV-DEC-051`: dihapus** |
| `NV-CQ-008` | Berapa jeda terlama yang diterima antara pasien pindah atau pulang dan panggilannya tertutup? Contoh: sama dengan muat ulang papan, paling lambat 60 detik | `NV-CAP-19`, `NV-DEC-046`. **Ditutup 2026-11-05 — `NV-DEC-052`: paling lambat 60 detik** |

### Pemicu impact scan berikutnya

Selain pemicu revision 1, bagian ini basi bila salah satu berubah:

- `InpBedPlacement` (`EndDateTime`, `EndReason`), `InpBedPlacementEndReason`, atau indeks
  `IX_InpBedPlacement_BedId_Active`;
- `InpBedOccupancyService#TransferCoreAsync`, `#ReleaseActivePlacementAsync`, atau
  `InpEpisodeService#ReleaseBedHoldsAsync`;
- `NvwDeviceContextService`, policy `NurseViewDisplayRead`, atau `QueueHub#JoinNurseViewFloor`;
- `AccessMenuSeeder`, `PermissionRegistryDescriptor`, atau `PermissionRegistryValidator`;
- SHA backend berbeda dari `0c6eab34` atau SHA frontend berbeda dari `91a11b08f`.
