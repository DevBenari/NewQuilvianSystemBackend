# Audit Source untuk Desain Encounter-First, Pasien Tanpa Identitas, dan Kelayakan Dokter Jaga

| Field | Nilai |
| --- | --- |
| Tanggal | 22 September 2026 |
| Jenis | Evidence audit source **baca-saja**. **Nol source aplikasi diubah**, nol migration, nol kueri basis data dijalankan agent |
| Pemicu | Tinjauan desain pemilik sesudah gelombang R3.12 tuntas; desainnya disetujui **prinsip** di percakapan 22 September 2026 lalu ditulis ke repo pada pass blueprint yang sama hari itu |
| Backend | `NewQuilvianSystemBackend` branch `rizkiG` `0d13f3a8` |
| Frontend | `QuilvianSystemFrontendDev` branch `RizkiV2` `c941012ac` (sinkron dengan origin) |
| Keputusan yang bersandar pada berkas ini | `IGD-DEC-139`, `IGD-DEC-140`, `IGD-DEC-141`; penutupan `IGD-OQ-093` |
| Task yang bersandar pada berkas ini | `BE-IGD-051`…`056`, `FE-IGD-035`…`037` (roadmap backend R3.13, frontend R3.12) |

Setiap temuan di bawah ditulis dengan **lokasi source** supaya bisa diperiksa ulang tanpa
bertanya kepada agent. Nomor baris berlaku pada SHA di atas.

---

## 1. `IGD-EV-140` — pilihan dokter IGD hari ini adalah seluruh master dokter; roster IGD belum ada

**Gap baru — dicatat sebagai bukti, bukan sebagai cacat task lama.** `FE-IGD-027` **tetap ✅**:
acceptance-nya tentang riwayat penugasan dokter, dan seluruhnya terpenuhi. Pembatasan "hanya
dokter yang sedang jaga" adalah kebutuhan **baru** yang lahir sesudahnya. Pemilik menegaskan ini
dua kali pada 22 September 2026 — jangan turunkan `FE-IGD-027`.

| # | Temuan | Lokasi |
| ---: | --- | --- |
| 1 | Daftar pilihan dokter pada layar triase diambil dari `GET /v1/corporate/human-resource/master-data/doctors/options` dengan `isActive: true` — artinya **semua** dokter aktif rumah sakit, termasuk dokter poliklinik yang sedang praktik di gedung lain | `src/lib/state/slice/health-services/emergency-installation-management/emergency-management-triage-slice.jsx` baris 553–554 dan 576 |
| 2 | `MstDoctorSchedule` sebenarnya **kaya**: `ScheduleType` (`WeeklyRecurring`/`SpecificDate`/`Temporary`), `PracticeDate`, `StartTime`/`EndTime`, `IsOvernight`, `SessionName`, `ScheduleStatus` (`Draft`/`Active`/`Suspended`/`Closed`), `EffectiveStartDate`/`EffectiveEndDate`, `IsSubstituteSchedule`, `SubstituteDoctorId` | `Areas/HealthServices/MasterData/Models/MstDoctorSchedule.cs` baris 22–76 |
| 3 | Tetapi tabel itu **ber-DNA poliklinik**: `ClinicId` wajib (`Guid`, bukan `Guid?`), ditambah tujuh ruas kuota dan kanal pendaftaran (`MaxPatientQuota`, `MaxAppointmentQuota`, `MaxWalkInQuota`, `EstimatedServiceMinutes`, `IsAllowWalkIn`, `IsAllowAppointment`, `IsAllowKioskRegistration`) | `MstDoctorSchedule.cs` baris 31 dan 52–64 |
| 4 | Cuti dokter tidak dapat dibaca langsung: `MstDoctor.WorkforceProfileId` (`Guid`) dan `WfpLeaveRequest.EmployeeId` (`Guid?`) berasal dari **domain identitas yang berbeda** — jebakan yang sama dengan `BE-IGD-039` (`DepartmentId` lawan `OrganizationUnitId`). `LeaveRequestStatus` bertipe **string** bebas dengan bawaan `"Draft"`, bukan enum | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs` baris 18; `Areas/Corporate/HumanResource/LeaveManagement/Models/WfpLeaveRequest.cs` baris 24 dan 89 |
| 5 | **Belum diketahui** apakah basis data dev memuat jadwal untuk unit IGD sama sekali. Agent dilarang membaca basis data; kueri E1–E3 (bagian 7) menunggu pemilik | — |

**Artinya.** Dari source saja, rumusan "dokter yang sedang jaga IGD" **belum dapat dihitung**. Tabel
jadwalnya ada, tetapi (a) bentuknya dirancang untuk sesi praktik poliklinik, dan (b) belum terbukti
berisi. Karena itu `BE-IGD-056` ditahan sampai E1–E3 dijawab.

*Contoh.* dr. Andi terdaftar di master dan aktif, tetapi hanya praktik Selasa pagi di Poli Penyakit
Dalam. Hari ini, Jumat pukul 23.00, namanya tetap muncul di pilihan dokter IGD dan tetap bisa
ditetapkan sebagai Dokter Penanggung Jawab IGD pasien yang baru ditriage.

---

## 2. `IGD-EV-141` — waktu tiba: validasinya dikomentari, dan nilai kosong jatuh ke jam browser

**Temuan dicatat, tidak diperbaiki.** Penyelesaiannya bergantung pada keputusan W1 (`IGD-OQ-094`).

| # | Temuan | Lokasi |
| ---: | --- | --- |
| 1 | `ArrivalDateTime` **sudah** dikumpulkan di layar pendaftaran IGD, dalam dua isian: `arrivalDate` dan `arrivalTime` | `src/components/view/health-services/registration-management/emergency-registration/emergency-visit-step.jsx` baris 83–90 dan 337–347 |
| 2 | Pemeriksaan *"Tanggal dan jam kedatangan belum valid."* di `handleNext` **dikomentari** — jadi langkah itu boleh dilewati dengan tanggal/jam kosong atau rusak | `emergency-visit-step.jsx` baris 201–204 |
| 3 | Saat dikirim, nilai kosong diganti `new Date().toISOString()` lewat `toIsoDateOrNow` — yaitu **jam komputer petugas**, bukan jam server, dan **bukan** waktu pasien tiba | `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` baris 158–159 (definisi) dan 1161 (pemakaian `arrivalDateTime`) |
| 4 | Backend punya jaring kedua: `ArrivalDateTime = request.ArrivalDateTime == default ? now : request.ArrivalDateTime` — tetapi karena frontend tidak pernah mengirim nilai kosong, cabang itu praktis tidak tercapai | `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs` baris 297 |

> **Dikoreksi 22 September 2026 (malam), impact scan suplemen 3.2 bagian S3.2.5.** Paragraf di bawah keliru soal SLA:
> `ResponseDueAt` dihitung dari `EmgTriage.StartedAt`, bukan dari waktu tiba, dan tidak ada perhitungan *door-to-triage*
> di kode. Pemakai waktu tiba yang sebenarnya: filter/urutan daftar kunjungan, penjaga waktu penugasan dokter,
> tampilan pengkajian, fallback `BE-IGD-048`. Paragraf asli dibiarkan sebagai riwayat.

**Kenapa ini penting.** Waktu tiba adalah titik nol *door-to-triage* (`IGD-DEC-127`). Bila isiannya
kosong, sistem diam-diam mencatat "pasien tiba tepat saat tombol simpan ditekan" menurut jam
komputer pendaftaran — yang bisa meleset menit atau jam bila jam komputernya salah. Angka SLA
triage lalu tampak lebih baik dari kenyataan.

*Contoh.* Pasien tiba 20.05, antre di loket, didaftarkan 20.40. Petugas tidak mengisi jam
kedatangan. Tercatat tiba **20.40** (jam komputer loket), sehingga menunggu 35 menit sebelum
didaftarkan hilang dari catatan.

**Batas yang ditegaskan pemilik.** `ArrivalDateTime` **tidak boleh** disamakan dengan
`RegPatientEncounter.RegisteredAt`. Keduanya dua peristiwa berbeda: tiba di pintu lawan
tercatat di loket. Usulan W1 — waktu tiba diisi perawat triage saat **Mulai Triage**, diisi awal
dengan **waktu server** dan boleh dikoreksi — masih menunggu keputusan pemilik (`IGD-OQ-094`).

**Temuan sampingan, di luar lingkup IGD.** Fungsi yang sama dipakai untuk tanggal lahir:
`birthDate: toIsoDateOrNow(values.birthDate)` (`emergency-registration.utils.js` baris 892). Tanggal
lahir kosong pada pendaftaran pasien baru lewat layar IGD akan tersimpan sebagai **hari ini**.
Belum diperiksa apakah backend pasien menolak atau menyimpannya. Dicatat untuk pemilik layar
pendaftaran pasien; tidak diberi task di pass ini.

---

## 3. `IGD-EV-142` — modul IGD tidak pernah menutup encounter, dan tanda "encounter sudah berakhir" tidak seragam

| # | Temuan | Lokasi |
| ---: | --- | --- |
| 1 | `EncounterStatus` punya tiga status akhir: `Completed = 9`, `Cancelled = 10`, `NoShow = 11` | `Areas/HealthServices/RegistrationManagement/Enums/EncounterStatus.cs` baris 35–42 |
| 2 | Seluruh folder `EmergencyInstallationManagement` menyebut `EncounterStatus` **nol kali**. Kunjungan IGD yang selesai tidak menutup encounter-nya; encounter `Emergency` **tidak pernah ditutup siapa pun** kecuali petugas memanggil endpoint Registrasi secara manual | `grep -rn EncounterStatus Areas/HealthServices/EmergencyInstallationManagement` → kosong |
| 3 | Kunjungan IGD mencapai status akhir lewat **dua** jalur saja: `PATCH /emergency-visits/{id}/complete` → `Completed` (penyimpanan baris 562), dan `PATCH /emergency-visits/{id}/visit-status` dengan target `Cancelled` (penyimpanan baris 496). `PATCH visit-status` menolak target `Completed` (baris 474) | `EmergencyVisitController.cs` baris 460–562 |
| 4 | Jalur ketiga yang **bukan** status: `DELETE /emergency-visits/{id}` menghapus lunak kunjungan (`IsDelete = true`) dan membiarkan encounter-nya | `EmergencyVisitController.cs` baris 595–612 |
| 5 | Registrasi sendiri memakai **empat** tanda berakhir yang tidak selalu ditulis bersamaan: `PATCH /patient-encounters/{id}/cancel` mengisi `IsCancel`, `CancelledAt`, `CancelReason` (dan membatalkan antrean) **tanpa** mengubah `EncounterStatus`; `PATCH /patient-encounters/{id}/status` ke `Completed` mengubah `EncounterStatus` **tanpa** mengisi `CompletedAt`; pemeriksaan "sudah batal atau selesai" di endpoint status membaca `IsCancel`/`CancelledAt`/`CompletedAt`, **bukan** `EncounterStatus` | `PatientEncounterController.cs` baris 926–935 (status) dan 1046–1085 (cancel) |
| 6 | `PATCH /patient-encounters/{id}/status` ke `Completed` ikut **mengunci catatan klinis yang belum ditandatangani** (`RM-DEC-003`) lewat `ClinicalDocumentIntegrityService.LockOpenDocumentsForEncounterAsync`. Service itu **tidak menyimpan sendiri** — ikut `SaveChanges` pemanggil — dan sudah terdaftar di DI | `PatientEncounterController.cs` baris 948–957; `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` baris 41 dan 325; `Program.cs` baris 392 |
| 7 | Billing menganggap `Completed` **dapat ditagih** dan `Cancelled`/`NoShow` **tidak** | `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs` baris 1205–1227; `BillingDepositService.cs` baris 264 |
| 8 | Jalan keluar manual yang sudah ada: `PATCH /patient-encounters/{id}/status` (`PatientEncounter : Update`) | `PatientEncounterController.cs` baris 906–913 |

**Artinya untuk desain.** Rumus "encounter sudah berakhir" yang hanya membaca `EncounterStatus`
akan salah membaca encounter yang dibatalkan lewat `PATCH …/cancel`. Rumus yang hanya membaca
`CompletedAt` akan salah membaca encounter yang ditutup lewat `PATCH …/status`. Karena itu
`IGD-DEC-139` memakai **keempat** tanda sekaligus. Dan karena IGD tidak pernah menutup encounter,
setiap encounter `Emergency` lama kemungkinan besar masih "terbuka" — sebab `BE-IGD-053` baru boleh
menyala sesudah `BE-IGD-052`.

---

## 4. `IGD-EV-143` — hanya satu pintu yang membuat encounter `Emergency`; pasien tanpa identitas tidak bisa punya encounter

| # | Temuan | Lokasi |
| ---: | --- | --- |
| 1 | Pembuatan `RegPatientEncounter` ada di tiga tempat produksi. `PatientEncounterController.CreateEncounterCoreAsync` menulis `EncounterType = request.EncounterType` — **satu-satunya** yang bisa menghasilkan `Emergency`. `EncounterIntakeService` menulis `Outpatient` tetap; `InpEpisodeService` menulis `Inpatient` tetap | `PatientEncounterController.cs` baris 427, 564, 591; `Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs` baris 317–326; `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` baris 1116–1125 |
| 2 | Satu pembuat lain adalah seeder demo kamar operasi, yang tidak mengisi `EncounterType` sehingga jatuh ke bawaan `Outpatient` | `Areas/HealthServices/OperatingRoomManagement/Seeders/OperatingRoomDemoSeeder.cs` baris 388; `RegPatientEncounter.cs` baris 83 |
| 3 | Jadi penjaga episode ganda yang dipasang pada **satu pintu itu** menutup seluruh jalur `Emergency` tanpa menyentuh rawat jalan maupun rawat inap | — |
| 4 | Status awal encounter bergantung pada `IsQueueRequired` klinik/unit: `Queued` bila wajib antre, `Registered` bila tidak. **Belum diperiksa** apakah unit IGD wajib antre dan apakah baris `TrxQueue` ikut dibuat — `IGD-OQ-097` | `PatientEncounterController.cs` baris 544 dan 595–597 |
| 5 | `RegPatientEncounter.PatientId` bertipe `Guid` **non-nullable** — pasien tanpa identitas tidak dapat punya encounter | `RegPatientEncounter.cs` baris 26 |
| 6 | Sebaliknya `EmgVisit.PatientId` bertipe `Guid?`, dengan `IsUnknownPatient` dan `TemporaryPatientAlias`; penjaga `AllowUnknownPatient` pada pengaturan IGD | `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgVisit.cs` baris 23, 48, 51; `Services/EmergencyVisitService.cs` baris 145 |
| 7 | `EmgVisit.EncounterId` bertipe `Guid?` dengan **unique index tanpa filter** — satu encounter paling banyak punya satu kunjungan, **termasuk kunjungan yang sudah dihapus lunak**. PostgreSQL mengizinkan banyak `NULL`, jadi pasien tanpa identitas tidak terganggu | `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgVisitConfiguration.cs` baris 31 |
| 8 | Aturan "episode aktif" hari ini hanya membaca kunjungan: `PatientId` sama, `IsDelete = false`, `VisitStatus` bukan `Completed` dan bukan `Cancelled` | `EmergencyVisitService.cs` baris 343–364 (`CariEpisodeAktifAsync`) |

**Akibat butir 7 yang perlu diingat.** Encounter yang kunjungannya pernah dihapus lunak **tidak
dapat** diberi kunjungan baru — insert kedua akan bentrok dengan unique index. Kelas K4 (bagian 6)
karena itu tidak bisa "dikembalikan ke antrean" oleh `BE-IGD-055`.

---

## 5. `IGD-EV-144` — dua aksi yang sudah ada menuntut kunjungan sudah lahir

| # | Temuan | Lokasi |
| ---: | --- | --- |
| 1 | Aksi **Tangani Segera** (`FE-IGD-030` ✅, `IGD-DEC-128`) memanggil `PATCH /emergency-visits/{id}/visit-status` — butuh `id` kunjungan | `emergency-management-triage-slice.jsx` baris 176–192 |
| 2 | Daftar pasien triase hari ini membaca `GET /emergency-visits` dengan filter `visitStatus` — hanya kunjungan; encounter tanpa kunjungan **tidak tampil** | `emergency-management-triage-slice.jsx` baris 125 dan 137–160 |
| 3 | Penugasan dokter IGD mewajibkan kunjungan: `EmgDoctorAssignment.EmergencyVisitId` bertipe `Guid` wajib | `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgDoctorAssignment.cs` baris 49 |
| 4 | Satu-satunya tempat alasan tersimpan pada penugasan adalah `AssignmentReason` (`string?`); tidak ada ruas penanda override | `EmgDoctorAssignment.cs` baris 85 |

**Artinya untuk desain.** Pada alur encounter-first, pasien di daftar **Menunggu Triage** belum punya
kunjungan, sehingga **Tangani Segera** dan **Tetapkan Dokter** tidak dapat dijalankan pada barisnya
sampai kunjungan lahir. Untuk Tetapkan Dokter ini wajar (dokter ditetapkan sesudah triage). Untuk
Tangani Segera ini **bertabrakan** dengan `IGD-DEC-128` — pasien gawat tidak boleh menunggu tombol
Mulai Triage. Dicatat sebagai `IGD-OQ-100`.

---

## 6. Kelas encounter `Emergency` yang belum berakhir — K1 sampai K4

> **Catatan kejujuran.** Keempat kelas ini dirancang di percakapan 22 September 2026, tetapi rumusan
> kata demi katanya **tidak tersimpan** di repo maupun di catatan agent. Tabel di bawah adalah
> **rekonstruksi agent dari fakta source bagian 3 dan 4**. Pemilik mengonfirmasi atau mengoreksinya
> **sebelum** kueri D dipakai sebagai dasar `BE-IGD-052`. Bila pemilik sudah memegang versi
> percakapan, versi itu yang berlaku dan tabel ini diganti.

"Belum berakhir" memakai rumus `IGD-DEC-139`: `IsDelete = false`, `IsCancel = false`,
`CancelledAt` kosong, `CompletedAt` kosong, dan `EncounterStatus` bukan 9, 10, atau 11.

| Kelas | Keadaan kunjungannya | Bukti yang tersedia | Arti | Boleh ditutup `BE-IGD-052`? |
| --- | --- | --- | --- | --- |
| **K1** | Ada kunjungan (tidak dihapus) berstatus `Completed` atau `Cancelled` | Status akhir kunjungan; `VisitCompletedAt` untuk `Completed` | Encounter tertinggal terbuka karena IGD tidak pernah menutupnya (`IGD-EV-142` butir 2) | **Ya** — buktinya deterministik |
| **K2** | Ada kunjungan (tidak dihapus) yang belum berakhir | — | Episode memang sedang berjalan, **atau** kunjungan lama yang tidak pernah diselesaikan | **Tidak** — bukan urusan rekonsiliasi encounter; kunjungannya yang harus diselesaikan lewat jalurnya |
| **K3** | Tidak ada kunjungan sama sekali | Hanya encounter-nya | Encounter dari pendaftaran ganda yang ditolak `409` (`IGD-DEC-138`), pendaftaran yang ditinggal, atau pasien yang pergi sebelum ditriage | **Tidak otomatis** — butuh audit referensi per baris (kueri B pada kartu `BE-IGD-050`); encounter yang sudah dipakai billing, antrean, atau catatan klinis **bukan** yatim |
| **K4** | Satu-satunya kunjungannya dihapus lunak | Baris kunjungan terhapus | Kunjungan dihapus lewat `DELETE`, encounter tertinggal | **Tidak otomatis** — hapus lunak bukan status klinis; tidak bisa diberi kunjungan baru (`IGD-EV-143` butir 7) |

Sesudah `IGD-DEC-139` berlaku, K3 yang **baru** lahir tidak lagi berarti cacat: encounter tanpa
kunjungan adalah pasien yang **sedang menunggu triage**. K3 yang bermasalah adalah yang menunggu
**tanpa akhir** — pertanyaan `IGD-OQ-096`.

---

## 7. Kueri baca-saja yang menunggu pemilik

> **Catatan yang sama berlaku.** Kueri D dan E1–E3 diberikan di percakapan 22 September 2026.
> Teks di bawah **disusun ulang** agent dari ringkasannya, supaya tidak hilang bila percakapan
> ditutup. Bila pemilik menjalankan versi percakapan, versi itu yang dicatat hasilnya. Agent
> **tidak** menjalankan satu pun.

### 7.1 Kueri D — jumlah encounter `Emergency` yang belum berakhir, per kelas

```sql
-- D. Klasifikasi encounter Emergency (EncounterType = 2) yang BELUM berakhir. Baca-saja.
WITH enc AS (
  SELECT e."Id"
  FROM public."RegPatientEncounter" e
  WHERE e."EncounterType" = 2
    AND NOT e."IsDelete"
    AND NOT e."IsCancel"
    AND e."CancelledAt" IS NULL
    AND e."CompletedAt" IS NULL
    AND e."EncounterStatus" NOT IN (9, 10, 11)
)
SELECT
  CASE
    WHEN EXISTS (SELECT 1 FROM public."EmgVisit" v
                 WHERE v."EncounterId" = enc."Id" AND NOT v."IsDelete"
                   AND v."VisitStatus" IN (8, 9))            THEN 'K1'
    WHEN EXISTS (SELECT 1 FROM public."EmgVisit" v
                 WHERE v."EncounterId" = enc."Id" AND NOT v."IsDelete") THEN 'K2'
    WHEN EXISTS (SELECT 1 FROM public."EmgVisit" v
                 WHERE v."EncounterId" = enc."Id")            THEN 'K4'
    ELSE 'K3'
  END AS kelas,
  COUNT(*) AS jumlah
FROM enc
GROUP BY 1
ORDER BY 1;
```

`VisitStatus` 8 = `Cancelled`, 9 = `Completed`. Karena unique index `EncounterId` tanpa filter,
satu encounter paling banyak punya satu kunjungan, sehingga keempat cabang saling lepas.

### 7.2 Kueri E1–E3 — apakah jadwal dokter IGD ada

```sql
-- E1. Unit layanan yang dipakai kunjungan IGD, dan jumlah jadwal dokter pada unit itu.
SELECT v."ServiceUnitId",
       COUNT(*) AS kunjungan,
       (SELECT COUNT(*) FROM public."MstDoctorSchedule" s
        WHERE s."ServiceUnitId" = v."ServiceUnitId" AND NOT s."IsDelete") AS jadwal
FROM public."EmgVisit" v
WHERE NOT v."IsDelete"
GROUP BY v."ServiceUnitId";

-- E2. Jadwal pada unit IGD menurut jenis, status, dan penanda lintas tengah malam.
--     ScheduleType: 1 WeeklyRecurring, 2 SpecificDate, 3 Temporary.
--     ScheduleStatus: 1 Draft, 2 Active, 3 Suspended, 4 Closed. Keduanya disimpan sebagai int.
SELECT s."ScheduleType", s."ScheduleStatus", s."IsOvernight", COUNT(*) AS jumlah
FROM public."MstDoctorSchedule" s
WHERE NOT s."IsDelete" AND s."IsActive"
  AND s."ServiceUnitId" IN (SELECT DISTINCT "ServiceUnitId" FROM public."EmgVisit" WHERE NOT "IsDelete")
GROUP BY 1, 2, 3
ORDER BY 1, 2, 3;

-- E3. Klinik yang terpasang pada jadwal unit IGD (ClinicId wajib) dan jumlah dokter berbeda.
SELECT s."ClinicId", COUNT(*) AS jadwal, COUNT(DISTINCT s."DoctorId") AS dokter
FROM public."MstDoctorSchedule" s
WHERE NOT s."IsDelete"
  AND s."ServiceUnitId" IN (SELECT DISTINCT "ServiceUnitId" FROM public."EmgVisit" WHERE NOT "IsDelete")
GROUP BY s."ClinicId";
```

**Cara membaca hasilnya.** E1 = 0 jadwal → roster IGD **tidak ada** di tabel ini; kriteria kelayakan
harus bersumber lain atau jadwal IGD diisi dulu oleh pemiliknya. E2 menunjukkan apakah shift malam
(`IsOvernight`) dan jadwal khusus tanggal (`SpecificDate`) memang dipakai. E3 menunjukkan klinik
tiruan apa yang dipasang untuk memenuhi `ClinicId` wajib — tanda bahwa tabel poliklinik "dipaksa"
dipakai untuk IGD.

### 7.3 Tempat mencatat hasil

| Kueri | Hasil | Dijalankan oleh / tanggal |
| --- | --- | --- |
| D | *belum dilaporkan* | — |
| E1 | *belum dilaporkan* | — |
| E2 | *belum dilaporkan* | — |
| E3 | *belum dilaporkan* | — |
| A/B (`BE-IGD-050` acceptance 8) | *belum dilaporkan* — masih terbuka sejak 21 September 2026 | — |

Yang diminta adalah **angka**, bukan pernyataan "berhasil" — kueri ini kueri hitung.
