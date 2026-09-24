# Laporan Perubahan Backend — `BE-IGD-046`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-046` |
| Judul | Validasi dan proyeksi tanda vital pada detail observasi |
| Slice | `IGD-S04` · `EPIC IGD-09` (pemantauan observasi) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.9, kartu `BE-IGD-046` |
| Trace | `IGD-DEC-122`, `IGD-DEC-123`, `IGD-DEC-124`, `IGD-DEC-125`, `IGD-DEC-126`, `IGD-DEC-056`, `IGD-DEC-057`, `IGD-DEC-110`; bukti `IGD-EV-124`…`130`; [audit Observasi V1–V2](../../../evidence/2026-09-15-audit-observasi-v1-v2.md) temuan `J-1`, `J-2`, `J-3`; api-contract bagian 7; validation-matrix bagian 9 |
| Contract version | API `0.6.0` dan validation `0.6.0` (16 September 2026, status berkas `draft`). Aturan yang dipakai task ini `approved` lewat `IGD-DEC-122`…`126` (Rizki Gunawan, 16 September 2026) |
| Dependency | `IGD-DEC-122` ✅, `IGD-DEC-126` ✅, kontrak `0.6.0` ✅. Tidak menunggu task lain |
| Klasifikasi | `MEDIUM` — tiga berkas pada satu modul: satu service existing (pemeriksaan baru), satu controller existing (empat action disentuh), satu berkas DTO (proyeksi aditif). Tanpa entity, konfigurasi EF, migration, endpoint baru, maupun registrasi service baru |
| Task mode | `BACKEND` — frontend `QuilvianSystemFrontendDev` (`RizkiV2`) hanya dibaca untuk memastikan pemanggil saat ini tidak rusak |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `EmergencyObservationService.cs`, `EmergencyObservationDetailController.cs`, `EmergencyObservationDetailDtos.cs`; laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | Dikerjakan di atas `27351517` (branch `rizkiG`). Perubahan **belum** di-commit; ditinggalkan di working tree untuk review owner |
| Tanggal | 16 September 2026 |
| Status | **Implementation complete.** **Build = Verified** — dijalankan Rizki 16 September 2026: *Build succeeded with 207 warning(s) in 223,7s*, **nol error**. **Runtime verified: belum.** Bukan UAT |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `EmergencyInstallationManagement` / Emergency |
| Submodule | — (capability berada langsung di bawah module) |
| Registry | Prefix `Emg`, kategori BUSINESS DOMAIN / MODULE, lifecycle `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `EmergencyInstallationManagement`) |
| Keberlakuan | `TOUCHED LEGACY` — controller existing yang memakai `ApplicationDbContext` langsung. Pola itu **tidak** diperluas: pemeriksaan bisnis baru ditaruh di `EmergencyObservationService`, dan tidak ada abstraksi repository baru |
| QBE yang berlaku | `QBE-VAL-001` (invarian bisnis divalidasi sebelum persistence), `QBE-API-001` (envelope `ApiResponse`, kode status dan konvensi route existing dipertahankan), `QBE-DTO-001` (entity EF tidak diekspos; proyeksi memakai response DTO), `QBE-SVC-001` (aturan domain pindah ke Module Service yang sudah terdaftar), `QBE-PERM-001` (metadata `AccessController`/`AccessAction`/`AccessPermission` tidak berubah) |
| QBE yang dicatat, tidak ditangani | `QBE-SVC-001` sebagian — controller masih membaca `ApplicationDbContext` langsung untuk kueri daftar/detail (utang legacy; tidak diperluas, tidak di-refactor). `QBE-LOG-001` — log `Create`/`Update` belum menyertakan aktor pada payload log (legacy, di luar lingkup). `QBE-NAM-001` — `TrxPatientVitalSign` dan `TrxPatientIntegratedProgressNote` memakai prefix `Trx` legacy milik `ClinicalManagement`; hanya dibaca, tidak dinormalkan |
| Wewenang database | **Tidak diminta dan tidak dipakai.** Nol migration, nol perintah basis data |

---

## 1. Masalah yang diperbaiki

Layar Observasi IGD mencatat pemantauan berkala pasien: waktu, keadaan klinis, tindakan, respons
pasien, cairan masuk dan keluar, serta catatan. Satu baris pemantauan **boleh menautkan** satu
tanda vital yang sudah tercatat (`PatientVitalSignId`) dan satu catatan perkembangan
(`ProgressNoteId`). Empat masalah nyata ditemukan pada audit Observasi V1–V2:

| Kode | Keadaan sebelum | Akibat nyata |
| --- | --- | --- |
| `J-1` | Backend hanya memeriksa apakah baris tanda vital **ada**: `Any(x => x.Id == patientVitalSignId && !x.IsDelete)` | Tanda vital **pasien lain** dapat ditautkan ke pemantauan pasien ini. Angka tekanan darah, nadi, dan GCS milik orang lain akan tampil di riwayat pemantauan pasien ini, lalu ikut dibaca dokter saat mengambil keputusan. Ini soal keselamatan pasien sekaligus privasi |
| `J-1` (lanjutan) | Encounter tidak pernah dibandingkan | Tanda vital dari **kunjungan lain** pasien yang sama — misalnya kunjungan rawat jalan tiga bulan lalu — dapat ditautkan ke episode IGD hari ini, sehingga grafik pemantauan bercampur antar-episode |
| `J-3` | `RecordedByUserId` diambil dari **badan permintaan** bila tidak kosong | Pelaku pencatat dapat dipalsukan: pemanggil cukup mengirim GUID perawat lain, dan rekam medis akan menuliskan nama orang itu. Bila badan permintaan tidak mengirim field tersebut sama sekali, `PUT` justru **mengosongkan** pencatat menjadi GUID nol |
| `J-2` | Detail pemantauan dapat dibuat pada periode berstatus `Completed` atau `Cancelled` | Pemantauan "sesudah selesai" tercatat tanpa penanda apa pun, seolah-olah terjadi sebelum periode ditutup. Kesimpulan observasi yang sudah ditulis menjadi tidak konsisten dengan isi pemantauannya |

Masalah kelima bersifat kinerja dan tampilan: response hanya membawa **GUID** tanda vital, bukan
angkanya, dan tidak membawa nama pencatat sama sekali. Layar riwayat yang ingin menampilkan
tekanan darah dan nama perawat terpaksa memanggil endpoint tanda vital **satu kali per baris**
(pola N+1) dan mengambil daftar pengguna hanya untuk menerjemahkan satu GUID.

*Contoh konkret masalah `J-1`.* Perawat membuka periode observasi Tn. A di IGD. Karena backend
tidak memeriksa pemiliknya, permintaan yang menautkan tanda vital Ny. B tetap diterima `200`.
Riwayat pemantauan Tn. A kemudian menampilkan tekanan darah 80/50 milik Ny. B. Dokter jaga
membaca angka itu sebagai perburukan Tn. A.

---

## 2. Proses bisnis

**Tujuan.** Menjaga agar pemantauan observasi hanya menunjuk data klinis milik pasien dan episode
kunjungan yang sedang dibuka, mencatat pelaku yang sebenarnya, menolak pemantauan pada periode
yang sudah ditutup, dan menampilkan angkanya tanpa permintaan tambahan per baris.

**Pelaku.** Perawat IGD (pencatat), dokter jaga (pembaca), dan sistem sebagai penjaga aturan.

**Pemicu.** Perawat menekan *Catat Pemantauan* pada tab Observasi.

**Langkah normal.**

1. Perawat membuka periode observasi pasien. Periode berstatus `Active` (atau `Escalated` bila
   pasien sempat memburuk).
2. Perawat mencatat tanda vital baru lewat kemampuan `PatientVitalSign` milik `ClinicalManagement`
   — alur bawaan — atau memilih tanda vital yang sudah tercatat pada kunjungan yang sama.
3. Perawat mengisi keadaan klinis, tindakan, respons pasien, jumlah cairan, dan catatan, lalu
   menyimpan.
4. Backend memeriksa berurutan: periode ada → periode belum ditutup → tanda vital dan catatan
   perkembangan memang milik pasien dan kunjungan ini dan masih berlaku.
5. Baris pemantauan disimpan. Pelaku pencatat diambil dari pengguna yang sedang login, bukan dari
   badan permintaan.
6. Balasan langsung membawa angka tanda vital dan nama pencatat, sehingga riwayat pemantauan
   bertambah lengkap tanpa permintaan lanjutan.

**Jalur tidak normal.**

| Keadaan | Jawaban sistem | Yang dilakukan petugas |
| --- | --- | --- |
| Periode observasi tidak ditemukan | `400` "Periode observasi tidak ditemukan." | Memuat ulang daftar periode |
| Periode sudah `Completed` atau `Cancelled` | `409` "Periode observasi ini sudah ditutup, pemantauan baru tidak dapat ditambahkan. Buka periode observasi baru bila pasien masih perlu dipantau." | Membuka periode observasi baru |
| Tanda vital yang dipilih tidak ada | `400` "Tanda vital yang dipilih tidak ditemukan. Pilih tanda vital lain atau catat tanda vital baru." | Memilih ulang atau mencatat tanda vital baru |
| Tanda vital milik pasien lain | `400` "Tanda vital yang dipilih bukan milik pasien pada kunjungan ini." | Memilih tanda vital pasien yang benar |
| Tanda vital dari kunjungan lain | `400` "Tanda vital yang dipilih berasal dari kunjungan lain. Pilih tanda vital dari kunjungan IGD yang sedang dibuka." | Memilih tanda vital episode IGD ini |
| Tanda vital sudah dibatalkan, dihapus, atau nonaktif | `400` "Tanda vital yang dipilih sudah tidak berlaku. Pilih tanda vital lain atau catat tanda vital baru." | Mencatat tanda vital baru |
| Catatan perkembangan di luar lingkup | `400` "Catatan perkembangan yang dipilih bukan milik kunjungan pasien ini." | Memilih catatan perkembangan kunjungan ini |

**Status yang dihasilkan.** Tidak ada status baru. Status periode observasi (`Active`,
`Completed`, `Escalated`, `Cancelled`) tidak berubah artinya, dan alur
`PATCH .../observation-status` milik `BE-IGD-040` tidak disentuh.

**Urutan pemeriksaan bersifat mengikat** (validation `0.6.0` bagian 9.1). Bila periode sudah
ditutup **dan** tanda vitalnya milik pasien lain, yang dijawab lebih dulu adalah `409` periode
tertutup — karena mengganti tanda vitalnya pun tidak membuat permintaan itu diterima. Tidak ada
data yang berubah sebelum seluruh pemeriksaan lulus; tidak ada penyimpanan yang lalu dibatalkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Dokumen blueprint:** `MODULE-STATUS.md`; `00-interview-decisions.md` (`IGD-DEC-122`…`126`,
`IGD-DEC-056`, `IGD-DEC-057`, `IGD-DEC-110`); `evidence/2026-09-15-audit-observasi-v1-v2.md`;
`02-backend-architecture.md` bagian 12; `contracts/api-contract.md` bagian 7;
`contracts/validation-matrix.md` bagian 9; `roadmap/backend-roadmap.md` bagian R3.9;
`roadmap/requirement-traceability.md` bagian R3.5.

**Tata kelola:** `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`,
`API_RULES.md`, `DATABASE_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`.

**Source backend:** `EmergencyObservationDetailController.cs`; `EmergencyObservationService.cs`;
`EmergencyObservationDetailDtos.cs`; `EmgObservationDetail.cs`; `EmgObservation.cs`;
`EmgVisit.cs`; `EmergencyObservationStatus.cs`; `EmgObservationDetailConfiguration.cs`;
`TrxPatientVitalSign.cs`; `PatientVitalSignStatus.cs`; `PatientVitalSignController.cs` (arti
"masih berlaku"); `TrxPatientIntegratedProgressNote.cs`;
`PatientIntegratedProgressNoteController.cs` (arti "masih berlaku"); `ApplicationUser.cs`;
`EmergencyDepartureService.cs` dan `EmergencyDepartureController.cs` (pola `Hasil` dan
`Failure`); `EmergencyObservationController.cs` (`BE-IGD-040`, tidak disentuh); `Program.cs`
(memastikan `EmergencyObservationService` sudah terdaftar); `QuilvianSystemBackend.csproj`.

**Source frontend (baca saja):** `emergency-assessment-slice.jsx` dan
`emergency-assessment-observation-tab.jsx` pada branch `RizkiV2` — memastikan pemanggil saat ini
tidak mengirim `recordedByUserId` maupun `patientVitalSignId`, dan tidak mencocokkan teks pesan
penolakan.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyObservationService.cs` | +177 / −1. Tujuh konstanta pesan penolakan kontrak `0.6.0` bagian 9; record `HasilPemeriksaanPemantauan` (kode status + pesan) mengikuti pola `Hasil` pada `EmergencyDepartureService`; method `ValidateDetailScopeAsync` yang memeriksa keberadaan periode, status periode, lingkup tanda vital, dan lingkup catatan perkembangan dalam urutan yang dikunci kontrak |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyObservationDetailController.cs` | +113 / −42. `EmergencyObservationService` disuntikkan (sudah terdaftar di `Program.cs`, **tanpa** registrasi baru); `Create` dan `Update` memanggil pemeriksaan lingkup sebelum menyentuh data; `RecordedByUserId` pada `Create` selalu dari token; `Update` tidak lagi menulis `RecordedByUserId` dari badan permintaan; `ValidateRequestAsync` lama dan pemetaan `ToResponse` diganti expression proyeksi `ProyeksiResponse` beserta helper `BacaResponseAsync` dan `Failure`; daftar dan detail memakai proyeksi yang sama |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyObservationDetailDtos.cs` | +56 / −0. `EmergencyObservationDetailResponse` bertambah `VitalSign` dan `RecordedByName`; kelas baru `EmergencyObservationDetailVitalSignResponse` (20 field, seluruhnya kolom nyata `TrxPatientVitalSign`); `CreateEmergencyObservationDetailRequest.RecordedByUserId` diberi keterangan **usang dan diabaikan** |

**Nol** perubahan pada entity, konfigurasi EF, `Program.cs`, `Migrations/`, snapshot, enum, dan
endpoint.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif pada response, mengetat pada penerimaan.** Bentuk request tidak bertambah dan tidak berkurang; `recordedByUserId` tetap diterima tetapi diabaikan. Response bertambah `vitalSign` (objek, `null` bila tidak ada tautan) dan `recordedByName` (string, `null` bila pengguna tidak ditemukan). Dua penolakan baru ditegakkan: `400` lingkup tautan dan `409` periode tertutup. Tidak ada endpoint, field, enum, atau envelope yang dihapus maupun diganti nama |
| Database | **NOT APPLICABLE** — nol migration, nol kolom baru, nol perubahan entity/konfigurasi, nol perintah basis data. Relasi `PatientVitalSign`, `ProgressNote`, dan `RecordedByUser` beserta indexnya sudah ada sejak `EmgObservationDetailConfiguration` dibuat dan dipakai apa adanya |
| Keamanan/Auth | **Menguat.** Pelaku pencatat tidak lagi dapat ditentukan pemanggil; data klinis pasien lain tidak lagi dapat ditautkan ke rekam observasi pasien ini. Metadata hak akses (`EmergencyObservationDetail : Read/Create/Update/Delete`) **tidak berubah**, dan tidak ada pemeriksaan role yang di-hardcode |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Observation Detail

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observation-details`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pemantauan satu periode observasi, kini lengkap dengan angka tanda vital dan nama pencatat | `EmergencyObservationDetail : Read` |
| `GET` | `/{id}` | Satu baris pemantauan beserta angka tanda vital dan nama pencatat | `EmergencyObservationDetail : Read` |
| `POST` | `/` | Mencatat satu putaran pemantauan; menolak periode tertutup dan tautan di luar lingkup | `EmergencyObservationDetail : Create` |
| `PUT` | `/{id}` | Mengubah pemantauan; tautan tetap diperiksa lingkupnya, pencatat asli dipertahankan | `EmergencyObservationDetail : Update` |
| `DELETE` | `/{id}` | Menandai pemantauan terhapus (soft delete) — **tidak berubah** | `EmergencyObservationDetail : Delete` |

Kode status: `200`, `400`, `403`, `404` (khusus `/{id}`), dan `409`.

---

## 5. Verifikasi

### 5.1 Perintah

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false` | **Berhasil** — *Build succeeded with 207 warning(s) in 223,7s*, nol error | `PASS` | Dijalankan **Rizki, 16 September 2026**; keluaran terminal owner |
| Asal 207 warning build | Tiga warning yang terlihat pada keluaran (`CS1573` parameter `cancellationToken`, `CS1587` letak komentar XML) berada di `Seeders/AccessMenuSeeder.cs` dan `Areas/HealthServices/RegistrationManagement/.../RegistrationController.cs` — **bukan** berkas task ini. Keluaran penuh belum disaring per berkas | `EXISTING / ENVIRONMENT ISSUE` | Keluaran terminal owner; perintah penyaring ada pada 5.2 |
| Automated test backend | Proyek test backend sudah dihapus 11 September 2026 | `NOT RUN` | `IGD-DEC-110` butir (b); tidak ada proyek test yang dapat dijalankan |
| Perintah basis data | Tidak dijalankan sama sekali | `NOT RUN` | Task tidak memberi wewenang database; nol migration |
| Pemeriksaan keseimbangan sintaks (kurung dan tanda kutip) pada tiga berkas yang berubah | Seimbang pada ketiganya | `PASS` | Pemeriksaan source lokal; **bukan** pengganti compiler |
| Pemeriksaan tumpang tindih nama tipe antar-namespace yang di-`using` baru | Nol tumpang tindih antara `ClinicalManagement.Enums`/`Models` dan `EmergencyInstallationManagement.Enums`/`Models` | `PASS` | Perbandingan daftar berkas kedua namespace |
| Pemeriksaan sisa pemanggil kode yang dihapus (`ValidateRequestAsync`, `ToResponse`) | Nol sisa di seluruh repository | `PASS` | Pencarian source `EmergencyObservationDetailResponse` dan kedua nama method |

Uji manual: `REQUIRED` — dijalankan pemilik sesudah build berhasil. Contoh permintaan dan
balasannya ada pada 5.3.

**Tidak dijalankan, beserta alasannya:** `dotnet build` (alur kerja owner — agent tidak menjalankan
build backend); automated test (proyek test tidak ada); perintah basis data (tidak diberi wewenang
dan tidak dibutuhkan); uji lewat layar (frontend `FE-IGD-028` belum dikerjakan).

### 5.2 Perintah build dan hasilnya

```bash
cd NewQuilvianSystemBackend
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

Dijalankan **Rizki pada 16 September 2026**. Hasil: `Build succeeded with 207 warning(s) in 223,7s`
— **nol error**. Tanpa `dotnet ef migrations add` dan tanpa `dotnet ef database update`; task ini
tidak mengubah schema.

Jumlah 207 warning adalah keadaan baseline repository (`GenerateDocumentationFile` menyala,
`NoWarn` hanya melepas `CS1591`), bukan hasil task ini. Untuk memastikan nol warning berasal dari
tiga berkas yang berubah, keluarannya dapat disaring:

```powershell
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false |
  Select-String "EmergencyObservationDetailController|EmergencyObservationService|EmergencyObservationDetailDtos"
```

Keluaran kosong berarti ketiga berkas tidak menyumbang satu pun warning.

### 5.3 Contoh uji API manual (`IGD-DEC-110`)

Seluruh contoh di bawah **belum dijalankan** (`NOT FEASIBLE` bagi agent: membutuhkan aplikasi
berjalan, token, dan data). Nilainya ditulis supaya pemilik dapat menjalankannya apa adanya.

Base URL contoh: `POST api/v1/health-services/emergency-installation-management/emergency-observation-details`

**Contoh 1 — tanda vital pasien dan kunjungan yang sama (kriteria 1).**

```json
{
  "emergencyObservationId": "5c1f…  (periode Active milik Tn. A)",
  "patientVitalSignId": "9ab3…  (tanda vital Tn. A, encounter IGD yang sama, status Recorded)",
  "recordedAt": "2026-09-16T10:30:00Z",
  "clinicalConditionSummary": "Nyeri dada berkurang, akral hangat.",
  "urineOutputMl": 150
}
```

Harapan: `200`. Balasannya membawa `patientVitalSignId`, objek `vitalSign` berisi angka, dan
`recordedByName`.

**Contoh 2 — tanda vital pasien lain (kriteria 2).** Sama seperti contoh 1, tetapi
`patientVitalSignId` milik Ny. B. Harapan: `400` dengan pesan *"Tanda vital yang dipilih bukan
milik pasien pada kunjungan ini."*

**Contoh 3 — tanda vital dari encounter lain (kriteria 3).** `patientVitalSignId` milik Tn. A
tetapi dari encounter rawat jalan sebelumnya. Harapan: `400` dengan pesan *"Tanda vital yang
dipilih berasal dari kunjungan lain. Pilih tanda vital dari kunjungan IGD yang sedang dibuka."*

**Contoh 4 — tanda vital tidak berlaku (kriteria 4).** `patientVitalSignId` milik Tn. A pada
encounter yang sama, tetapi sudah dibatalkan lewat `PATCH patient-vital-signs/{id}/cancel`.
Harapan: `400` dengan pesan *"Tanda vital yang dipilih sudah tidak berlaku. Pilih tanda vital
lain atau catat tanda vital baru."*

**Contoh 5 — `recordedByUserId` palsu diabaikan (kriteria 5).** Login sebagai `UserA`, kirim
badan permintaan berisi `"recordedByUserId": "<GUID UserB>"`. Harapan: `200` — **tidak** ditolak —
dan `recordedByUserId` pada balasan berisi `UserA`, `recordedByName` berisi nama `UserA`.

**Contoh 6 — periode tertutup (kriteria 6).** Periode diselesaikan lebih dulu lewat
`PATCH .../emergency-observations/{id}/observation-status` dengan `observationStatus = 2`
(`Completed`), lalu `POST` pemantauan baru. Harapan: `409` dengan pesan *"Periode observasi ini
sudah ditutup, pemantauan baru tidak dapat ditambahkan. Buka periode observasi baru bila pasien
masih perlu dipantau."* Ulangi dengan `observationStatus = 4` (`Cancelled`) — harapannya sama.
Ulangi dengan `Escalated` (`3`) — harapannya `200`, perilaku `Escalated` tidak berubah.

**Contoh 7 — proyeksi pada daftar (kriteria 7).**
`GET .../emergency-observation-details?emergencyObservationId=<id periode>&sortBy=recordedAt`.
Harapan: setiap baris membawa `vitalSign` dan `recordedByName`, dan pada log Serilog terlihat
**satu** perintah `SELECT` untuk halaman itu — bukan satu perintah tambahan per baris.

*Potongan balasan yang diharapkan:*

```json
{
  "data": {
    "items": [
      {
        "id": "…",
        "emergencyObservationId": "…",
        "patientVitalSignId": "…",
        "vitalSign": {
          "id": "…",
          "observationDateTime": "2026-09-16T10:28:00Z",
          "bloodPressureSystolic": 128,
          "bloodPressureDiastolic": 82,
          "pulseRate": 96,
          "respiratoryRate": 20,
          "temperature": 37.2,
          "oxygenSaturation": 97,
          "gcsEye": 4, "gcsVerbal": 5, "gcsMotor": 6, "gcsTotal": 15,
          "consciousnessStatus": 1,
          "isUsingOxygen": true,
          "oxygenSupportType": 1,
          "oxygenFlowRate": 3,
          "oxygenSupportNote": null,
          "vitalSignStatus": 1,
          "isAbnormal": false,
          "isCritical": false
        },
        "recordedAt": "2026-09-16T10:30:00Z",
        "recordedByUserId": "…",
        "recordedByName": "Ns. Ani Rahmawati",
        "clinicalConditionSummary": "Nyeri dada berkurang, akral hangat.",
        "urineOutputMl": 150
      }
    ]
  }
}
```

> **Delta tercatat terhadap contoh pada api-contract bagian 7.2.** Nilai enum dikirim sebagai
> **angka**, bukan nama (`"consciousnessStatus": 1`, bukan `"ComposMentis"`). Backend ini tidak
> memasang `JsonStringEnumConverter` pada `Program.cs`, dan `PatientVitalSignResponse` milik
> `ClinicalManagement` — sumber angka yang sama — juga mengirim enum sebagai angka. Mengubahnya
> hanya untuk satu endpoint akan menciptakan konvensi tandingan. Contoh JSON di dokumen kontrak
> bersifat ilustratif; bentuk yang berlaku adalah bentuk backend saat ini.

**Contoh 8 — nama pencatat (kriteria 8).** `GET .../{id}` pada baris hasil contoh 1. Harapan:
`recordedByName` berisi nama, bukan GUID; bila penggunanya tidak ditemukan, nilainya `null`.

**Contoh 9 — baris lama tanpa tautan (kriteria 9).** `GET .../{id}` pada baris pemantauan yang
dibuat sebelum task ini. Harapan: `200`, `patientVitalSignId` `null`, `vitalSign` `null`, dan
seluruh kolom lama tetap terbaca.

**Contoh 10 — pemantauan tanpa tanda vital tetap sah (validation aturan 11).** `POST` tanpa
`patientVitalSignId` dan tanpa `progressNoteId` pada periode `Active`. Harapan: `200`.

### 5.4 Pemetaan pemeriksaan ke source

| Pemeriksaan | Tempat di source |
| --- | --- |
| Periode ada (aturan 1) | `EmergencyObservationService.ValidateDetailScopeAsync` — penjaga `Guid.Empty` dan kueri `EmgObservation` |
| Periode belum ditutup (aturan 2 dan 3) | idem — cabang `tolakPeriodeTertutup` dengan pola `is Completed or Cancelled`; `Active` dan `Escalated` tidak masuk cabang |
| Tanda vital ada / satu pasien / satu encounter / masih berlaku (aturan 4–7) | idem — blok `patientVitalSignId` |
| Catatan perkembangan satu lingkup (aturan 8) | idem — blok `progressNoteId` |
| Pelaku dari token (aturan 9) | `EmergencyObservationDetailController.Create` — `RecordedByUserId = actorUserId` |
| Kosong bukan nol (aturan 10) | Tidak berubah — kelima kolom cairan tetap `decimal?` dan disalin apa adanya |
| Tautan opsional (aturan 11) dan baris lama terbaca (aturan 12) | Penjaga `HasValue` pada service; `ProyeksiResponse` mengirim `vitalSign` `null` ketika relasinya kosong |
| Urutan pemeriksaan (bagian 9.1) | Urutan pernyataan di dalam `ValidateDetailScopeAsync`; seluruhnya selesai sebelum `Add`/`SaveChangesAsync` dipanggil |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `POST` dengan `patientVitalSignId` milik pasien **dan** encounter yang sama → `200`, tautan tersimpan | Terpenuhi (source) | `ValidateDetailScopeAsync` meloloskan tautan yang cocok; `Create` menyimpan `PatientVitalSignId` apa adanya. Uji runtime contoh 1 belum dijalankan |
| 2. Tanda vital milik pasien lain → `400` beserta pesannya | Terpenuhi (source) | `PesanTandaVitalBedaPasien`, dibandingkan lewat `EmgObservation → EmgVisit.PatientId` |
| 3. Tanda vital dari encounter lain → `400` beserta pesannya | Terpenuhi (source) | `PesanTandaVitalBedaKunjungan`, dibandingkan lewat `EmgVisit.EncounterId` |
| 4. Tanda vital dibatalkan/dihapus/nonaktif → `400` beserta pesannya | Terpenuhi (source) | Penyaring `!IsDelete` pada kueri, lalu `IsActive` dan `VitalSignStatus is Cancelled or EnteredInError` |
| 5. `recordedByUserId` pemanggil diabaikan, permintaan **tidak** ditolak | Terpenuhi (source) | `Create` memakai `actorUserId` tanpa syarat; tidak ada penolakan yang dikaitkan dengan field itu; keterangan usang pada DTO |
| 6. `POST` pada `Completed`/`Cancelled` → `409`; `Active` dan `Escalated` tetap diterima | Terpenuhi (source) | Cabang `tolakPeriodeTertutup: true` hanya dipanggil dari `Create`; dua status lain tidak masuk pola |
| 7. `GET /` dan `GET /{id}` membawa `vitalSign` dalam **satu** kueri | Terpenuhi (source) | `ProyeksiResponse` dipakai keduanya; proyeksi lewat navigation menghasilkan `LEFT JOIN`, bukan kueri per baris. Pembuktian lewat log SQL menunggu runtime |
| 8. Response membawa `recordedByName`; kosong bila pengguna tidak ditemukan, tanpa GUID | Terpenuhi (source) | `RecordedByName` pada proyeksi; `null` ketika relasi `RecordedByUser` kosong |
| 9. Baris lama tanpa `patientVitalSignId` tetap terbaca, `vitalSign` `null` | Terpenuhi (source) | Cabang `x.PatientVitalSign == null ? null : …` pada proyeksi; tidak ada pengisian mundur |
| 10. Nol migration dan nol perubahan kolom | Terpenuhi | `git status --short` hanya memuat tiga berkas source; `Migrations/` dan snapshot tidak tersentuh |
| 11. Alur `PATCH .../observation-status` beserta `CompletionSummary` (`BE-IGD-040`) tidak berubah | Terpenuhi | `EmergencyObservationController.cs` tidak ikut berubah; method `CanTransition` pada service tidak disentuh |
| 12. `progressNoteId` mengikuti aturan lingkup yang sama | Terpenuhi (source) | Blok `progressNoteId` pada `ValidateDetailScopeAsync` dengan pesan aturan 8 |

**Definition of Done gelombang R3.9**

| Butir | Status |
| --- | --- |
| Acceptance criteria terpetakan ke source | Terpenuhi — tabel di atas dan bagian 5.4 |
| Contoh request/response uji API manual per kriteria (`IGD-DEC-110`) | Terpenuhi sebagai dokumen (bagian 5.3); **eksekusinya belum** — `NOT FEASIBLE` bagi agent |
| Perintah build diberikan kepada Rizki | Terpenuhi — bagian 5.2 |
| Laporan tracked `task/report/backend/BE-IGD-046.md` | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi — `backend-roadmap.md` R3.9 dan `requirement-traceability.md` R3.5 |
| QBE preflight diselesaikan | Terpenuhi — tabel Backend Governance Preflight |
| Tanpa UAT PASS | Terpenuhi — status UAT tidak ditulis lulus di mana pun |
| Butir 10 DoD (`IGD-DEC-122`…`126` disetujui Clinical Governance dan Nursing authority) | **Belum terpenuhi** — keputusan `approved` oleh Product/Domain Owner IGD; persetujuan Clinical Governance dan Nursing authority belum ada. Dicatat terbuka sesuai kartu roadmap |

---

## 7. Catatan penutup

### 7.1 Keputusan implementasi yang perlu diketahui pemilik

| Hal | Yang dipilih dan alasannya |
| --- | --- |
| Pesan "periode tidak ditemukan" | Teks lama `"EmergencyObservationId wajib diisi."` dan `"EmergencyObservationId tidak ditemukan."` diganti menjadi `"Periode observasi tidak ditemukan."` sesuai validation `0.6.0` bagian 9 aturan 1. Frontend saat ini menampilkan pesan backend apa adanya dan tidak mencocokkan teksnya, sehingga penggantian ini aman |
| `PUT` dan periode tertutup | Penolakan `409` **tidak** dinyalakan pada `PUT`. Aturan 2 menolak *"pemantauan baru"*, dan kartu `BE-IGD-046` menyebut `POST`. Memperluasnya ke `PUT` berarti menambah larangan yang tidak diperintahkan kontrak |
| `PUT` dan pencatat | `PUT` tidak lagi menulis `RecordedByUserId` dari badan permintaan, dan **tidak** menggantinya dengan pengguna yang mengubah. Pencatat asli dipertahankan; pelaku perubahan tetap tercatat pada `UpdateBy`. Ini pilihan paling sedikit mengubah arti data, sekaligus menutup pemalsuan. Perbaikan `PUT` menjadi tambah-saja (`IGD-DEC-080`, temuan `J-4`) tetap **di luar lingkup** |
| Lingkup catatan perkembangan | Aturan 8 memberi **satu** pesan untuk seluruh kegagalan lingkup, termasuk "tidak ditemukan". Pesan itu dipakai apa adanya; tidak ada teks baru yang dikarang |
| Arti "masih berlaku" | Mengikuti arti yang sudah dipakai domain pemiliknya: tanda vital = `IsActive` dan bukan `Cancelled`/`EnteredInError` (`PatientVitalSignController`); catatan perkembangan = `IsActive` dan bukan `IsCancel` (`PatientIntegratedProgressNoteController`) |
| Nama pencatat | Memakai urutan yang sudah dipakai backend lain: `DisplayName`, `UserName`, `Email`, `UserCode`. Tidak ada konvensi nama baru |

### 7.2 Penutup baku

| Hal | Isi |
| --- | --- |
| Peringatan | Build owner menghasilkan **207 warning dengan nol error** pada seluruh project. Tiga warning yang terlihat pada keluaran berada di berkas milik modul lain; penyaringan per berkas untuk ketiga berkas task ini belum dijalankan — perintahnya pada 5.2. Dua `using` pada controller — `ClinicalManagement.Models` dan `QuilvianSystemBackend.Models` — kini tidak terpakai setelah validasi lama dipindah ke service; keduanya sengaja **tidak** dihapus supaya diff tetap sempit dan tidak ada risiko resolusi tipe yang tidak dapat diuji agent. Silakan dibersihkan pemilik bila build bersih |
| Masalah yang diketahui | **`RUNTIME / DOMAIN GAP — provisional patient vital sign`**: `EmgVisit.PatientId` boleh kosong untuk pasien tanpa identitas, sedangkan `TrxPatientVitalSign.PatientId` wajib. Akibatnya kunjungan yang `PatientId`-nya masih kosong **selalu** menolak penautan tanda vital dengan pesan aturan 5. Itu bacaan harfiah kontrak dan **bukan** perilaku yang dikarang, tetapi artinya pasien provisional belum dapat memakai pemantauan bertanda vital. Tidak diperbaiki di sini (temuan `J-5`, `IGD-EV-130`; butuh keputusan, bukan tebakan). Kedua, ketika `EmgVisit.EncounterId` **dan** `TrxPatientVitalSign.EncounterId` sama-sama kosong, pemeriksaan encounter lolos karena keduanya dianggap sama; pasiennya tetap wajib sama, sehingga tidak ada kebocoran antar-pasien, tetapi tautan antar-episode masih mungkin pada data tanpa encounter |
| Risiko tersisa | **Build bersih, runtime belum.** Yang terbukti adalah kode ini mengompilasi tanpa error; klaim perilaku pada laporan ini tetap berupa pemetaan source sampai contoh 5.3 dijalankan. Penolakan baru menyentuh endpoint yang sudah dipakai layar: pemantauan yang saat ini disimpan **tanpa** `patientVitalSignId` tidak terpengaruh, tetapi pemanggil yang terbiasa menulis pemantauan pada periode yang sudah ditutup akan mulai menerima `409`. Bentuk response bertambah dua field; penambahan field bersifat aditif dan tidak merusak pembaca lama |
| Perubahan sampingan | `NONE` — tiga berkas source yang berubah seluruhnya milik task ini. Perubahan pada `docs/module-blueprints/igd/**` yang sudah ada di working tree sebelum task ini dimulai (pass desain 15–16 September 2026) tidak disentuh, kecuali roadmap, traceability, dan laporan ini yang memang bagian dari task |
| Interupsi | `NONE` |
| Status Git | Lihat 7.3. Tidak ada `stage`, `commit`, `push`, `merge`, maupun perpindahan branch. Branch tetap `rizkiG` |
| Langkah berikutnya | 1) ~~Build~~ — **selesai 16 September 2026, nol error**. 2) Jalankan uji API manual 5.3 contoh 1–10 untuk membuktikan perilakunya saat berjalan. 3) `FE-IGD-028` sudah boleh dimulai; verifikasi runtime dapat berjalan berdampingan. 4) `IGD-OQ-089` (bentuk terstruktur alat jalan napas), `IGD-OQ-090` (entri susulan), dan verifikasi `J-5` tetap terbuka dan tidak menahan `FE-IGD-028` |

### 7.3 `git status --short` di akhir pekerjaan

```text
 M Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyObservationDetailController.cs
 M Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyObservationDetailDtos.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyObservationService.cs
 M docs/module-blueprints/igd/00-interview-decisions.md
 M docs/module-blueprints/igd/02-backend-architecture.md
 M docs/module-blueprints/igd/03-frontend-architecture.md
 M docs/module-blueprints/igd/MODULE-STATUS.md
 M docs/module-blueprints/igd/blueprint-manifest.md
 M docs/module-blueprints/igd/contracts/api-contract.md
 M docs/module-blueprints/igd/contracts/validation-matrix.md
 M docs/module-blueprints/igd/evidence/2026-09-15-audit-observasi-v1-v2.md
 M docs/module-blueprints/igd/roadmap/backend-roadmap.md
 M docs/module-blueprints/igd/roadmap/frontend-roadmap.md
 M docs/module-blueprints/igd/roadmap/requirement-traceability.md
?? docs/module-blueprints/igd/task/report/backend/BE-IGD-046.md
```

Sebelas berkas `docs/module-blueprints/igd/**` sudah berubah **sebelum** task ini dimulai (pass
desain Observasi 15–16 September 2026, milik pemilik). Yang berasal dari task ini adalah tiga
berkas `Areas/**`, laporan ini, serta baris status pada `backend-roadmap.md` dan
`requirement-traceability.md`.

---

## 8. Status akhir

| Pertanyaan | Jawaban |
| --- | --- |
| `BE-IGD-046` | **IMPLEMENTATION COMPLETE** — seluruh 12 acceptance criteria terpetakan ke source; nol migration |
| Build | **Verified** — Rizki, 16 September 2026: *Build succeeded with 207 warning(s)*, nol error |
| Runtime verified | **Belum** — contoh 5.3 belum dijalankan |
| UAT | **Tidak dinyatakan lulus**; status UAT milik tim terpisah |
| `BACKEND READY FOR FE-IGD-028` | **YES** — implementasi lengkap dan build bersih (16 September 2026). Kontrak `0.6.0` bagian 7 sudah tersedia untuk layar. Catatan: perilaku runtime belum dibuktikan; uji API manual 5.3 tetap disarankan berjalan berdampingan dengan pekerjaan layar |
