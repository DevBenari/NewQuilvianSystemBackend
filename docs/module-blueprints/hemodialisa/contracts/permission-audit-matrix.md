# Hemodialisa — Matriks Hak Akses dan Audit

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — `last_changed_in: HMD-CONTRACT-v1`, status `approved` |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| `input_revision` | `contracts/api-contract.md` r1; `data/data-dictionary.md` r1 |

**Dokumen ini tidak memuat tabel seluruh endpoint.** Pemetaan endpoint ke hak akses sudah
dipegang kolom **Hak akses** pada `contracts/api-contract.md`, dan dua hal berikut dihitung dari
sana, bukan ditulis ulang:

| Yang diturunkan | Caranya |
|---|---|
| String atribut hak akses | `[AccessPermission("<Resource>", "<Action>")]`, disalin apa adanya dari kolom Hak akses |
| Status pencatatan logger | Konvensi repository: `GET` tidak dicatat, selain `GET` dicatat |

Penyimpangan dari kedua turunan itu didaftar pada bagian 5 sebagai pengecualian bernama.

---

## 1. Cara kerja hak akses di repository ini

Hak akses ditegakkan **di server**, per endpoint, memakai atribut. Menonaktifkan tombol di layar
bukan pengaman — ia hanya kenyamanan.

| Lapisan | Bentuk | Rujukan source |
|---|---|---|
| Penanda kelas | `[AccessController]` pada controller | Pola yang dipakai seluruh controller `HealthServices` |
| Penanda aksi | `[AccessAction]` pada setiap endpoint | idem |
| Butir hak akses | `[AccessPermission("Resource", "Action")]` pada setiap endpoint | Contoh nyata: `Areas/HealthServices/RadiologyManagement/Controllers/RadModalityController.cs:36@190c91a0` |
| Layanan pemeriksa | `AccessPermissionService` | Dipakai antara lain `ClinicalNoteAddendumController.cs:40, 48@190c91a0` |

Contoh bentuk yang akan dipakai Hemodialisa:

```csharp
[Route("api/v1/health-services/hemodialysis-management/hemodialysis-sessions")]
[Tags("Health Services / Hemodialysis Management / Hemodialysis Session")]
[AccessController]
public class HmdSessionController : ControllerBase
{
    [HttpPost("{id:guid}/start")]
    [AccessAction]
    [AccessPermission("HemodialysisSession", "Start")]
    public async Task<IActionResult> StartSession(...)
}
```

---

## 2. Butir hak akses modul ini

Enam belas Resource. Seluruhnya baru dan perlu ditanam lewat seeder hak akses.

| Resource | Action yang tersedia | Menjaga apa |
|---|---|---|
| `HemodialysisOrder` | `Read`, `Create`, `Accept`, `Hold`, `Reject`, `Cancel` | Permintaan HD masuk |
| `HemodialysisEpisode` | `Read`, `Create`, `Update`, `ChangeStatus` | Program HD pasien |
| `HemodialysisEligibility` | `Read`, `Decide` | Penilaian kelayakan klinis |
| `HemodialysisVascularAccess` | `Read`, `Create`, `Update` | Status akses pembuluh darah |
| `HemodialysisSerology` | `Read`, `Create` | Rujukan dan tinjauan hasil serologi |
| `HemodialysisIsolation` | `Read`, `Decide` | Keputusan kebutuhan isolasi |
| `HemodialysisPrescription` | `Read`, `Create`, `Update`, `Activate`, `Cancel` | Instruksi cuci darah |
| `HemodialysisSchedule` | `Read`, `Create`, `Update`, `Cancel` | Jadwal dan penugasan petugas |
| `HemodialysisSession` | `Read`, `CheckIn`, `Update`, `DeclareReady`, `Hold`, `OverrideChecklist`, `Start`, `Stop`, `Complete`, `SubmitDocumentation`, `RetryBillingHandoff` | Pelaksanaan sesi |
| `HemodialysisObservation` | `Read`, `Create` | Pemantauan berkala |
| `HemodialysisMedication` | `Read`, `Administer` | Pemberian obat |
| `HemodialysisComplication` | `Read`, `Create` | Komplikasi |
| `HemodialysisRecord` | `Finalize` | Pengesahan dan penguncian catatan |
| `HemodialysisUnitReadiness` | `Read`, `Create`, `Update`, `DeclareReady`, `DeclareNotReady` | Kesiapan unit |
| `HemodialysisMachine` | `Read`, `Create`, `Update`, `ChangeStatus`, `Delete` | Mesin |
| `HemodialysisStation` | `Read`, `Create`, `Update`, `ChangeStatus`, `Delete` | Station |
| `HemodialysisSetting` | `Read`, `Update` | Pengaturan unit |
| `HemodialysisChecklistItem` | `Read`, `Create`, `Update`, `SetOverridable` | Master butir checklist |

`HemodialysisRecord : Finalize` sengaja dipisahkan dari `HemodialysisSession : Update`. Keduanya
kewenangan yang berbeda: yang satu mengisi catatan, yang lain menyatakan catatan itu sah dan
mengunci. Menggabungkannya berarti setiap orang yang boleh mengetik juga boleh mengesahkan.

---

## 3. Peta peran rumah sakit ke butir hak akses

Peran di bawah adalah peran kerja di unit HD, bukan nama peran pengguna di sistem. Pemetaannya
ke peran pengguna dikerjakan admin saat modul dipasang.

| Peran kerja | Butir hak akses yang dipegang |
|---|---|
| **Dokter atau perawat unit peminta** (bangsal, poliklinik, IGD) | `HemodialysisOrder : Create`, `: Read`, `: Cancel` |
| **Petugas administrasi HD** | `HemodialysisEpisode : Read`, `: Create`, `: Update`, `: ChangeStatus`; `HemodialysisOrder : Read` |
| **Koordinator unit HD** | `HemodialysisOrder : Read`, `: Accept`, `: Hold`; `HemodialysisSchedule : *`; `HemodialysisUnitReadiness : *`; `HemodialysisMachine : *`; `HemodialysisStation : *`; `HemodialysisSession : Read` |
| **Dokter dialisis** | `HemodialysisOrder : Reject`; `HemodialysisEligibility : Decide`; `HemodialysisPrescription : *`; `HemodialysisIsolation : Decide`; `HemodialysisVascularAccess : Update`; `HemodialysisSession : Read`, `: OverrideChecklist`; `HemodialysisRecord : Finalize` |
| **Dokter penanggung jawab pasien (DPJP)** | Sama seperti dokter dialisis, dan menjadi pemilik keputusan pada episode yang ia pegang |
| **Perawat dialisis** | `HemodialysisSession : CheckIn`, `: Update`, `: DeclareReady`, `: Hold`, `: Start`, `: Stop`, `: Complete`, `: SubmitDocumentation`; `HemodialysisObservation : *`; `HemodialysisMedication : Administer`; `HemodialysisComplication : Create`; `HemodialysisVascularAccess : Create` |
| **Tim Pencegahan dan Pengendalian Infeksi (PPI)** | `HemodialysisSerology : Read`, `: Create`; `HemodialysisIsolation : Read`, `: Decide` |
| **Rekam Medis** | `HemodialysisSession : Read`; akses koreksi lewat butir hak akses milik modul Rekam Medis |
| **Pemegang akun tata kelola klinis** | `HemodialysisChecklistItem : SetOverridable`; `HemodialysisSetting : Update` |
| **Auditor berwenang** | Seluruh butir `: Read`; tidak satu pun butir yang mengubah data |

### Butir yang sengaja dipegang sedikit orang

| Butir | Sebabnya |
|---|---|
| `HemodialysisRecord : Finalize` | Pengesahan catatan klinis final. Hanya dokter penanggung jawab sesi (`HMD-ASM-002`) |
| `HemodialysisSession : OverrideChecklist` | Melewati pengaman keselamatan sebelum tindakan. Hanya dokter |
| `HemodialysisOrder : Reject` | Menolak indikasi medis. Hanya dokter (`HMD-ASM-003`) |
| `HemodialysisChecklistItem : SetOverridable` | Menetapkan butir persiapan mana yang boleh dilewati. Ini kewenangan kebijakan klinis, bukan kewenangan operasional |
| `HemodialysisSetting : Update` | Mengubah batas rasio perawat dan menyalakan penegakan kewenangan |

---

## 4. Kewenangan yang tidak dapat dijaga mesin hak akses

Ini bagian yang paling mudah terlewat: ada aturan yang **tidak** cukup dijaga dengan
memeriksa "pengguna ini punya butir hak akses atau tidak".

| Aturan | Dijaga oleh | Yang **tidak** dijaganya | Risikonya |
|---|---|---|---|
| Hanya dokter penanggung jawab **sesi itu** yang boleh mengesahkan | Aturan bisnis pada `HmdSessionFinalizationService` | Mesin hak akses hanya tahu pengguna punya butir `HemodialysisRecord : Finalize`; ia tidak tahu dokter itu penanggung jawab sesi yang mana | Dokter A mengesahkan sesi yang ditangani dokter B. Tanggung jawab hukum jadi kabur |
| Pengesah harus berbeda dari penyelesai dokumentasi | Aturan bisnis, membandingkan `SignedByUserId` dengan `DocumentedByUserId` | Mesin hak akses tidak mengenal konsep "orang yang berbeda" | Satu orang menulis sekaligus mengesahkan. Ini persis jebakan yang membuat modul Radiologi macet |
| Pembuat permintaan hanya boleh membatalkan permintaannya sendiri | Aturan bisnis pada `HmdOrderService` | Mesin hak akses tidak tahu siapa pembuat baris itu | Permintaan orang lain dibatalkan |
| Kewenangan klinis petugas untuk menjalankan dialisis | **Belum dijaga sama sekali** | Pembacaan kewenangan dari Human Resource belum tersedia (`HMD-DEP-002`) | Perawat tanpa kewenangan dialisis menjalankan sesi. Inilah sebabnya statusnya dicatat **belum dapat diverifikasi**, bukan terverifikasi |

Baris terakhir adalah risiko terbuka yang diketahui dan dicatat, bukan yang tersembunyi.
Penutupannya menjadi gerbang go-live, bukan blocker desain.

---

## 5. Pengecualian dari turunan bawaan

| Endpoint | Penyimpangan | Sebabnya |
|---|---|---|
| `GET /hemodialysis-episodes/{id}/serology-reviews` | **Dicatat logger**, padahal `GET` biasanya tidak | Status serologi adalah informasi kesehatan yang sensitif. Siapa membacanya dan kapan perlu tertelusur |
| `GET /hemodialysis-sessions/{id}` | **Dicatat logger**, padahal `GET` biasanya tidak | Membuka ruang kerja sesi berarti membuka catatan klinis lengkap seorang pasien |
| `POST /hemodialysis-sessions/{id}/billing-handoff/retry` | Tidak mengubah data klinis apa pun, tetapi tetap dicatat | Pengulangan penyerahan menyentuh jalur keuangan |

Selain tiga baris di atas, turunan bawaan berlaku apa adanya.

---

## 6. Audit

### Lapisan pencatatan

| Lapisan | Apa yang dicatat | Rujukan |
|---|---|---|
| Kolom audit pada setiap tabel | Siapa membuat, mengubah, membatalkan, dan menghapus, beserta waktunya | Warisan `IdentityModel`, sepuluh kolom |
| Logger aplikasi | Pemanggilan endpoint selain `GET`, ditambah tiga pengecualian pada bagian 5 | Konvensi repository |
| Jejak akses rekam medis | Pembacaan dokumen klinis yang sudah final | `MrcAccessLog`, milik Rekam Medis |
| Riwayat status mesin | Setiap perpindahan status beserta alasan dan pelakunya | `HmdMachineStatusHistory` |
| Kolom pelaku pada perpindahan status | Waktu dan pengguna pada setiap titik penting sesi | Kolom pada `HmdSession` |

### Kejadian yang wajib meninggalkan jejak tahan lama

Sepuluh kejadian berikut **tidak boleh** hanya tercatat di log aplikasi yang bisa habis masa
simpannya. Jejaknya melekat pada baris data itu sendiri.

| Kejadian | Jejaknya melekat pada |
|---|---|
| Permintaan HD ditolak | `HmdOrder.DecisionReason`, pelaku, dan waktunya |
| Episode ditutup | `HmdEpisode.ClosureReason`, `ClosureNote`, pelaku, dan waktunya |
| Resep diaktifkan dan digantikan | `HmdPrescription.SupersededByPrescriptionId` beserta kolom audit |
| Butir checklist dilewati | `HmdSessionChecklist.OverrideReason`, `OverriddenByUserId`, `OverriddenAt` |
| Sesi dimulai | `HmdSession.StartedAt`, `StartedByUserId` |
| Sesi dihentikan di tengah jalan | `HmdSession.StopReason`, `StopNote` |
| Dokumentasi diselesaikan | `HmdSession.DocumentedByUserId`, `DocumentedAt` |
| Catatan disahkan dan dikunci | `HmdSession.SignedByUserId`, `SignedAt`, ditambah baris keutuhan dokumen di Rekam Medis |
| Koreksi setelah pengesahan | Baris koreksi di Rekam Medis beserta alasan, penulis, dan waktunya |
| Status mesin berubah | Satu baris `HmdMachineStatusHistory` |

Prinsipnya satu: **alasan adalah bagian dari data, bukan bagian dari log.** Alasan penolakan,
penghentian, dan pelewatan butir persiapan akan ditanya ulang bertahun-tahun kemudian, dan
saat itu log aplikasi sudah lama tidak ada.

---

## 7. Kolom sensitif dan masa simpan

Kolom bertanda **Sensitif** pada `data/data-dictionary.md` tunduk pada tiga aturan:

1. **Tidak boleh** masuk ke payload logger. Yang dicatat hanya id baris, controller, aksi, dan
   status.
2. **Tidak boleh** dipakai sebagai contoh berisi data asli di dokumen mana pun. Seluruh contoh
   pada blueprint ini memakai nama samaran.
3. Kebutuhan penyamaran pada tampilan ditinjau per layar.

### Kolom sensitif modul ini

| Tabel | Kolom | Sebabnya |
|---|---|---|
| `HmdOrder` | `ClinicalReason`, `DecisionReason` | Memuat indikasi medis dan alasan penolakan klinis |
| `HmdEligibilityAssessment` | `IndicationSummary`, `DecisionReason`, `FollowUpInstruction` | Penilaian klinis |
| `HmdVascularAccess` | `ConditionNote` | Kondisi fisik pasien |
| `HmdSerologyReview` | `ResultSummary`, `ReviewNote` | **Paling sensitif di modul ini** — memuat status Hepatitis B, Hepatitis C, dan HIV |
| `HmdIsolationDecision` | `Reason` | Menyiratkan status infeksi pasien |
| `HmdPrescription` | `ClinicalNote`, `AnticoagulantPlan` | Instruksi klinis |
| `HmdSession` | `StopNote` | Alasan klinis penghentian |
| `HmdSessionAssessment` | `Complaint`, `PatientCondition`, `AccessConditionNote` | Keluhan dan kondisi pasien |
| `HmdSessionObservation` | `Note` | Catatan klinis |
| `HmdSessionMedication` | `Note` | Catatan pemberian obat |
| `HmdSessionComplication` | `SignsAndSymptoms`, `Intervention`, `ClinicianInstruction` | Kejadian klinis |

**Status serologi memerlukan perhatian khusus.** Ia hanya boleh dibaca peran yang benar-benar
membutuhkannya untuk keputusan operasional — tim PPI, dokter, dan perawat dialisis. Ia
**tidak** ditampilkan pada daftar kerja harian maupun daftar pasien, karena kedua layar itu
dilihat banyak orang sekaligus di ruang terbuka.

### Masa simpan

Data klinis Hemodialisa mengikuti masa simpan rekam medis yang berlaku di rumah sakit. Modul ini
**tidak** menetapkan masa simpannya sendiri, dan **tidak** menghapus baris secara fisik —
penghapusan bersifat penandaan lewat kolom audit warisan.
