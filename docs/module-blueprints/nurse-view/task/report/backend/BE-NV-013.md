# Laporan Perubahan Backend — `BE-NV-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-013` |
| Judul | Penutupan otomatis saat pasien pindah atau pulang |
| Slice | `MVP-7` — `EPIC NV-06` (gelombang backend 5, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-013` |
| Trace | `FR-NV-043`, `044` (kejadian sistem); `NV-DEC-046`, `050`, `052`, `057`; `AD-05`; `NV-AC-038`, `042`; `UAT-19`; `NFR-012`; state §5 baris `ClosedBySystem`; integration-contract R2 §5, §8; kamus data R2.3 (pemetaan `EndReason`); `TLM-NV-002` bagian 8 |
| Contract version | `0.2.1` — `approved` (Rizki Gunawan, 2026-11-09, `NV-DEC-057`): state §5, kamus data R2.3, permission-audit R2 §9. Dikerjakan mula-mula di atas `0.2.0` (`NV-DEC-055`) |
| Dependency | `BE-NV-011` ✅ (9 Nov 2026) |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 3 (0), logika sedang — pemetaan alasan, satu panggilan satu transaksi, putaran latar (1), tanpa kontrak API (0), persistence yang sudah ada (1), keamanan tidak tersentuh (0), satu workflow terbatas — siklus panggilan (1) |
| Task mode | `BACKEND` — prompt lanjutan Rizki 9 Nov 2026 ("Mulai dengan BE-NV-013 kecuali Rizki memilih lain"), target tulis `NewQuilvianSystemBackend` branch `rizkiG` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/Services/**` dan `Enums/NvwPatientCallCloseReason.cs`, `Program.cs` (berkas bersama, restu `NV-DEC-056`, baris tabel "Berkas bersama per task"), laporan ini, baris status roadmap, traceability, dan `TLM-NV-002`. Sesudah `NV-DEC-057`: amandemen blueprint atas keputusan owner — `00-interview-decisions.md`, state §5, kamus data R2.3, `02-backend-architecture.md`, permission-audit R2 §9, acceptance-test-matrix, kartu `BE-NV-013` (Cakupan, AC-3, Kontrak), `blueprint-manifest.md` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `589fb513` (branch `rizkiG`); perubahan `BE-NV-011` sudah di-stage Rizki, perubahan `BE-NV-012` belum di-stage, keduanya belum di-commit |
| Build saat diuji | DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` 9 Nov 2026 11.43, lebih baru dari source terakhir (11.02); memuat `NvwPatientCallReconciliationService`, `NvwPatientCallReconciliationHostedService`, `ResolveDepartureAsync`, `CorrectedToOtherBed`, `AdmissionCancelled`. Backend dijalankan Rizki; "Permission registry valid" 11.48.32 WIB, pemeriksaan berkala start 11.48.35 WIB |
| Tanggal | 2026-11-09 |
| Status | ✅ **SELESAI 9 November 2026** — AC 5/5 terpetakan ke source (AC-3 dalam bunyi `NV-DEC-057`). AC-1 terbukti runtime: uji HTTP **9/9 `PASS`**, jeda penempatan berakhir → panggilan tertutup **1,47 detik**. AC-4 dan AC-5 terbukti lewat penelusuran source dan review diff seperti yang diminta kartu. Bukti runtime AC-2 dikecualikan atas keputusan Rizki 9 Nov 2026; AC-3 belum terbukti runtime, dicatat sesuai kartu karena koreksi dan batal admisi tidak dapat dipulihkan. QBE Strict `PASS` (3 berkas). Build `PASS` menurut pernyataan Rizki. Riwayat: 🟡 SEBAGIAN 9 Nov 2026 pagi sebelum build dan uji |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-NV-011` dan `BE-NV-012`, panggilan hanya selesai bila pasien membatalkannya, perawat
menahan "Perawat hadir", atau perawat menutupnya dari konsol. Bila pasien dipindah atau pulang
sebelum perawat datang, panggilannya menggantung di strip TV dan konsol, padahal bed itu sudah
kosong.

**Contoh.** Pasien 331-C memanggil pukul 10.00.00, lalu dipindah ke 402-B pukul 10.05.00. Tanpa
task ini, panggilan 331-C tetap tampil "Menunggu" sampai ada perawat yang menutupnya dengan tangan.

---

## 2. Proses bisnis

**Tujuan:** panggilan aktif dari bed yang ditinggalkan pasien karena pindah atau pulang tertutup
sendiri paling lambat 60 detik sesudah Rawat Inap mencatatnya (`NV-DEC-052`). Nurse View hanya
membaca penempatan Rawat Inap; tidak ada kode Rawat Inap yang diubah.

**Langkah normal:**

1. Setiap 30 detik pemeriksaan berkala membuka scope baru dan mencari panggilan `Waiting` atau
   `Claimed` yang penempatannya — penempatan yang dicatat saat panggilan dibuat — sudah berakhir.
2. Bila penempatan itu sudah dikoreksi, pemeriksaan mengikuti baris penggantinya
   (`SupersededByCorrectionId`) sampai baris terkini (`NV-DEC-057`), lalu menilai baris itu:

   | Baris penempatan terkini | Panggilan aktif | Alasan tercatat |
   | --- | --- | --- |
   | Berakhir `Transfer` | Ditutup sistem | `PatientTransferred` — "pasien pindah" |
   | Berakhir `EpisodeClosed` atau `PatientDeparted` | Ditutup sistem | `PatientDischarged` — "pasien pulang" |
   | Berakhir `AdmissionCancelled` (admisi dibatalkan, bed dilepas) | Ditutup sistem | `AdmissionCancelled` (nilai baru `8`) — "admisi dibatalkan" |
   | Berada di **bed lain** (koreksi salah pilih bed) | Ditutup sistem | `PatientTransferred` — "pasien pindah" |
   | Di bed yang sama dan masih aktif (koreksi kelas atau jam) | **Tidak** diubah; dinilai ulang tiap putaran | — |

   **Contoh rantai.** 331-C memanggil 10.00. Pukul 10.02 kelasnya dikoreksi: penempatan lama
   berakhir `CorrectedEntry` dan baris pengganti tetap di 331-C, sehingga panggilan tetap aktif.
   Pukul 10.05 pasien dipindah ke 402-B: baris pengganti berakhir `Transfer`, dan paling lambat
   10.06 panggilan 331-C tertutup "pasien pindah".

3. Setiap panggilan diproses dalam satu transaksi: status `ClosedBySystem`, alasan, jam tutup,
   token versi naik, dan satu jejak `ClosedBySystem` dengan `ActorKind = System`.
4. Sesudah tertutup, tablet bed itu tidak lagi menampilkan panggilan. Karena bed tidak punya
   penempatan aktif, `GET /current` mengembalikan `IsBedOccupied = false` ("Bed belum
   berpenghuni"; perilaku `BE-NV-010`/`011` yang sudah ada).

**Contoh (AC-1).** 331-C memanggil pukul 10.00.00. Pukul 10.05.00 pasien dipindah ke 402-B.
Putaran berikutnya — paling lambat sekitar 10.05.30, karena satu putaran tiap 30 detik — menutup
panggilan 331-C dengan alasan "pasien pindah". Panggilan tidak ikut ke 402-B, sehingga lantai 4
tidak menerima apa pun dari kejadian itu.

**Jalur tidak normal:**

| Kejadian | Jawaban sistem |
| --- | --- |
| Perawat mengklaim atau menutup panggilan yang sama pada detik yang sama | Token versi menolak salah satunya. Bila pemeriksaan yang kalah, panggilan itu dilewati dan diperiksa ulang 30 detik kemudian; bila sudah tidak aktif, tidak disentuh lagi |
| Satu panggilan gagal disimpan karena galat lain | Galat dicatat log `Error` dengan Id panggilan dan kode bed; panggilan berikutnya tetap diproses (AC-4) |
| Seluruh putaran gagal, misalnya database tidak terjangkau | Galat dicatat log `Error`; host tetap hidup dan putaran berikutnya berjalan 30 detik kemudian (AC-4, `NFR-012`) |
| Penempatan dikoreksi di bed yang sama | Panggilan dibiarkan aktif; tablet tetap bisa dipakai karena baris pengganti masih aktif (AC-3, `NV-DEC-050`, `057`) |
| Rantai koreksi lebih dari 10 langkah, atau baris penempatan tidak ditemukan | Panggilan dibiarkan aktif dan dinilai ulang putaran berikutnya; perawat tetap bisa menutupnya |
| Backend mati saat pasien dipindah | Putaran pertama berjalan begitu host hidup, sehingga panggilan yang tertinggal langsung tertutup |
| Backend berjalan dengan `Runtime:Role = Web` | Pemeriksaan tidak berjalan di proses itu; ia berjalan di proses `Worker` (pola semua hosted service lain) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`; suite `rules/backend/` (`TASK_RULES`, `TASK_CLASSIFICATION`,
  `REVIEW_RULES`, `REPORT_TEMPLATE`, `TEST_POLICY`); `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
  registry baris 45 (`Nvw` `ACTIVE`)
- Blueprint: kartu `BE-NV-013`; state §5; integration-contract R2 §5, §8; kamus data R2.2
  (`ClosedByUserId`, `ActorUserId`, `PreviousClaimerUserId`) dan R2.3; `02-backend-architecture.md`
  R2.1, R2.4, R2.10 (`AD-05`); permission-audit R2 §9; `00-interview-decisions.md`
  `NV-DEC-046`, `050`, `052`; `TLM-NV-002` beserta jawaban Muhammad Hamzah (bagian 8)
- Source Rawat Inap untuk mencocokkan jawaban Hamzah: `InpEpisodeService.CancelAdmissionAsync`
  dan `ReleaseBedHoldsAsync` (episode `Admitted` boleh dibatalkan supervisor/kepala ruang;
  penempatan berakhir `AdmissionCancelled` dan bed dilepas), `ApplyPlacementCorrectionCoreAsync`
  (koreksi penempatan aktif: baris lama `CorrectedEntry`; koreksi penempatan yang sudah berakhir:
  baris lama mempertahankan `EndReason`; keduanya menautkan `SupersededByCorrectionId`; bed
  pengganti boleh sama atau berbeda)
- Source Nurse View: `NvwPatientCall`, `NvwPatientCallEvent`, enum `NvwPatientCallStatus`,
  `NvwPatientCallCloseReason`, `NvwPatientCallEventType`, `NvwPatientCallActorKind`,
  `NvwPatientCallConfiguration` (`Version` sebagai concurrency token, FK `InpBedPlacementId`),
  `NvwPatientCallService` (pola satu perintah satu `SaveChanges`), `NvwPatientCallQueryService`
  (`IsActiveStatus`, `GetForTabletAsync`), `NvwRealtimeService` (pola `ILogger`)
- Source pembanding: `AttendanceSchedulerHostedService`, `EmergencyTriageSlaMonitorHostedService`,
  blok `runBackgroundJobs` di `Program.cs`, konfigurasi Serilog
- Source Rawat Inap (baca saja): `InpBedPlacement`, `InpBedPlacementEndReason`,
  `InpBedOccupancyService` (`TransferAsync`, `ReleaseActivePlacementAsync`,
  `ReleaseBedStatusCopyAsync`), `InpBedMutationCoordinator.InvalidateAsync`,
  `InpBedReadinessService`, `InpBedAccessService.WorkflowAssignment`,
  `InpatientBedOccupancyController`, `InpatientBedManagementController`
- DB dev (SELECT read-only): keadaan bed uji, bed kosong siap per lantai, unit layanan lantai 3–4,
  episode `DischargePending`, riwayat `EndReason`, panggilan aktif, tablet

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Services/NvwPatientCallReconciliationService.cs` | **Baru.** `CloseCallsForDepartedPatientsAsync`: satu query kandidat (panggilan aktif yang penempatan awalnya sudah berakhir), lalu `ResolveDepartureAsync` mengikuti `SupersededByCorrectionId` paling banyak 10 langkah dan memutuskan alasan sesuai tabel §2. Kandidat yang harus ditutup dibaca ulang dan ditutup dalam satu `SaveChanges` sendiri. `DbUpdateConcurrencyException` → dilewati, dicoba putaran berikutnya. Galat lain → log `Error`, lanjut ke kandidat berikutnya. `ChangeTracker.Clear()` sesudah setiap kandidat. Mengembalikan ringkasan kandidat/ditutup/dilewati/gagal |
| `…/Services/NvwPatientCallReconciliationHostedService.cs` | **Baru.** `BackgroundService`, putaran pertama saat host hidup lalu tiap 30 detik; scope baru tiap putaran; galat putaran → log `Error`, putaran berikutnya tetap berjalan; berhenti rapi saat host dimatikan |
| `…/Enums/NvwPatientCallCloseReason.cs` | +1 nilai `AdmissionCancelled = 8` (`NV-DEC-057`). Nilai lama tidak berubah; konsol tetap hanya menerima alasan 2–4 |
| `Program.cs` | +2 baris, tanpa perubahan lain (diff di bawah) |

Diff `Program.cs` (berkas bersama, `NV-DEC-056`):

```diff
@@ -1029,6 +1029,7 @@
     builder.Services.AddScoped<NvwPatientCallService>();
+    builder.Services.AddScoped<NvwPatientCallReconciliationService>();
@@ -1353,6 +1354,7 @@
         builder.Services.AddHostedService<EmergencyTriageSlaMonitorHostedService>();
+        builder.Services.AddHostedService<NvwPatientCallReconciliationHostedService>();
```

Nol baris komentar baru; keempat berkas CRLF tanpa BOM. Tanpa berkas di
`Areas/HealthServices/InPatientManagement/` (AC-5).

**Keputusan developer di luar teks kartu.**

| No | Keputusan | Alasan |
| ---: | --- | --- |
| 1 | `ClosedAt` = jam penutupan oleh pemeriksaan, bukan jam berakhirnya penempatan | Jeda pindah → tutup harus bisa diukur (DoD); jam penempatan tetap terbaca di `InpBedPlacement` |
| 2 | `ClosedByUserId`, `ActorUserId`, `UpdateBy`, `CreateBy` jejak = kosong (`Guid.Empty`/`null`); `ClaimedByUserId` dibiarkan sebagai riwayat; `PreviousClaimerUserId` tidak diisi | Kamus data R2.2: "kosong untuk sistem"; `PreviousClaimerUserId` hanya untuk `TakenOver` dan `Released` |
| 3 | Penempatan yang dihapus lunak (`IsDelete`) tidak memicu penutupan | Selaras dengan definisi penempatan aktif di tablet. Source Rawat Inap tidak pernah menghapus lunak penempatan, jadi saringan ini tidak mengubah perilaku hari ini |
| 4 | Interval 30 detik ditulis tetap di kode, tanpa kunci konfigurasi | `AD-05` menetapkan 30 detik; kunci baru berarti mengubah `appsettings` di luar target tulis |
| 5 | `AddHostedService` di dalam blok `runBackgroundJobs`; `AddScoped` di blok Nurse View | Pola seluruh hosted service: pada Blue-Green hanya kontainer `Worker` yang menjalankannya. Bila ada dua proses `All`, token versi mencegah satu panggilan ditutup dua kali |
| 6 | Log ringkasan putaran hanya ditulis bila ada kandidat | Tanpa itu log bertambah dua baris per menit tanpa isi |
| 7 | Tanpa sinyal realtime | Pengirim sinyal dan pemanggilnya milik `BE-NV-014` (kartu `BE-NV-014`, grafik R2.1 baris `NvwPatientCallReconciliationService --> NvwRealtimeService`). Sampai itu, konsol memuat ulang tiap 15 detik dan strip TV tiap 60 detik |
| 8 | `SupersededByCorrectionId` diikuti setiap kali terisi, apa pun `EndReason` baris lamanya | Koreksi atas penempatan yang sudah berakhir mempertahankan `EndReason` lama (misalnya `Transfer`), sehingga baris penggantilah yang memuat bed dan jam yang benar |
| 9 | Batas 10 langkah rantai; melewatinya berarti panggilan dibiarkan aktif | Mencegah putaran tak berujung bila data berputar; perawat tetap bisa menutup |
| 10 | Log memuat dasar keputusan (`Transfer`, `EpisodeClosed`, `PatientDeparted`, `AdmissionCancelled`, atau `CorrectedToOtherBed`), Id penempatan penentu, dan jumlah langkah koreksi | Jejak alasan bisa dibaca tanpa membuka tabel Rawat Inap |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru atau berubah. `GET /current` tablet dan `GET /active` konsol otomatis berhenti menampilkan panggilan yang ditutup karena keduanya hanya membaca status `Waiting`/`Claimed` |
| Database | Tanpa migration, tanpa perubahan skema: `CloseReason` kolom `integer` tanpa check constraint, sehingga nilai `8` tidak butuh migration. Menulis `NvwPatientCall` dan `NvwPatientCallEvent`; membaca `InpBedPlacement` (termasuk `SupersededByCorrectionId`) dan `MstBed`. Satu query kandidat tiap 30 detik memakai index berfilter `IX_NvwPatientCall_BedId_Active` dan kunci utama `InpBedPlacement` |
| Keamanan/Auth | Tidak ada endpoint, tidak ada izin baru. Log hanya memuat Id panggilan, kode bed, lantai, status, alasan, Id penempatan, `EndReason`, dan dua jam; **tanpa** nama, No. RM, atau Id episode pasien |
| Rawat Inap | Tidak ada perubahan kode (`NV-DEC-052`). Ketergantungan baca `EndDateTime`/`EndReason` dikabarkan lewat `TLM-NV-002` |

---

## 4. Dokumentasi endpoint

Task ini tidak menambah atau mengubah endpoint. Perilakunya berjalan di latar dan terlihat lewat
endpoint `BE-NV-011` dan `BE-NV-012` yang sudah ada:

| Method | Path | Yang berubah sesudah penutupan otomatis |
| --- | --- | --- |
| `GET` | `api/v1/health-services/nurse-view-management/bed-tablet-runtime/current` | `ActiveCall` kosong, `IsBedOccupied = false` |
| `GET` | `api/v1/health-services/nurse-view-management/patient-calls/active?floorCode=…` | Panggilan tidak lagi ada di daftar lantai |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <2 berkas baru>,Program.cs` (array path) | Files evaluated **3** · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-09 |
| QBE ulang sesudah perapian dua berkas baru | Files evaluated **2** · VIOLATION 0 · REVIEW 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-09 |
| QBE ulang sesudah `NV-DEC-057` (service, hosted service, enum) | Files evaluated **3** · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-09 |
| Review diff/scope (AC-5) | Dua berkas baru di `NurseViewManagement/Services/`, enum +1 nilai, `Program.cs` +2/−0; nol berkas `InPatientManagement/` | `PASS` | `git status --short`, `git diff` |
| Nol komentar; akhiran baris | Nol `//`/`/*` pada kedua berkas baru; keempat berkas CRLF, tanpa BOM, nol baris LF tunggal | `PASS` | `grep -c`, hitungan byte Node |
| Penelusuran source aturan `NV-DEC-057` | `ResolveDepartureAsync`: baris hilang atau dihapus lunak → biarkan; `SupersededByCorrectionId` terisi → ikuti; bed berbeda → `PatientTransferred`; masih aktif → biarkan; selain itu menurut `EndReason` (`AdmissionCancelled` → nilai `8`). Dicocokkan dengan cara `ApplyPlacementCorrectionCoreAsync` membentuk baris pengganti | `PASS` | §2, §3.1 |
| Penelusuran source AC-4 | Galat per panggilan ditangkap di dalam `foreach` (kecuali pembatalan host); galat putaran ditangkap di hosted service, lalu `Task.Delay` tetap dijalankan | `PASS` | §6 |
| `dotnet build` | Dijalankan Rizki 9 Nov 2026, berhasil (pernyataan Rizki); backend dijalankan Rizki, rute tanpa login → `401` | `PASS` menurut pernyataan Rizki; jumlah warning tidak dinyatakan | DLL 11.43 memuat kelima simbol task ini (pemeriksaan byte agent) |
| Uji HTTP no. 1–9 — 9 Nov 11.51 WIB (`uji-tutup-otomatis.mjs`) | **9/9 `PASS`** — rincian §5.3 | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-013-20261109/hasil-2026-11-09T04-51-59-115Z/hasil.json`, `percobaan.json`, `policy-asli-sari.json` |
| Percobaan pertama 11.49 WIB | Berhenti sebelum menulis apa pun: pasien 326-C wajib ruang isolasi (`ISOLATION_REQUIRED`), sehingga tidak ada bed tujuan layak | `EXISTING / ENVIRONMENT ISSUE` (data uji, bukan source) | `hasil-2026-11-09T04-49-29-915Z/`; pemeriksaan `available-beds?includeIneligible=true` (`cek-bed-tujuan.mjs`, `cek-bed-tujuan-semua.mjs`) |
| Pemeriksaan DB sesudah uji (SELECT read-only) | Lihat §5.3 | `PASS` | Kueri agent 9 Nov 11.54 WIB |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan agent:** `dotnet build`, `dotnet run`, `dotnet ef` — milik Rizki. Tanpa migration.

### 5.1 Fakta DB dev untuk uji (SELECT read-only, 9 Nov 2026)

| Hal | Temuan |
| --- | --- |
| Bed uji berpenghuni | 326-C, 325-A, 325-B (lantai 3) dan 420-A (lantai 4): episode `Admitted`, belum meninggalkan ruangan, unit layanan "Rawat Inap", **tanpa folio tagihan**, `BedStatus` Occupied, kesiapan `Unverified` |
| Bed kosong siap | 60 di lantai 3, 42 di lantai 4 (`Available` + kesiapan `Ready`); contoh lantai 4: 401-A, 402-A |
| Unit layanan | Lantai 3 dan 4 memakai satu `ServiceUnitId` yang sama, sehingga transfer antarlantai **tidak** menerbitkan dokumen serah terima klinis (`CliTransferHandoverService` hanya dipanggil bila unit berbeda) |
| Episode `DischargePending` | Tidak ada |
| Riwayat akhir penempatan | Baru satu baris di seluruh DB, `EndReason = Transfer` (riwayat pasien 420-A) |
| Panggilan aktif / tablet | 0 panggilan aktif; 20 tablet, semuanya nonaktif |

### 5.2 Penelusuran pemulihan data Rawat Inap dan keputusan uji

AC-1 dan AC-2 hanya bisa dibuktikan bila sebuah penempatan benar-benar berakhir. Itu data milik
tim Muhammad Hamzah, jadi jalur pemulihannya ditelusuri lebih dulu tanpa menulis apa pun.

| Jalur | Bisa dipulihkan lewat API? | Sisa permanen |
| --- | --- | --- |
| **Pindah bed** (`POST inpatient-management/bed-occupancies/placements/transfer`) | **Ya.** Berbeda dengan koreksi (`BE-NV-011` §5), transfer memanggil `InvalidateAsync` untuk bed asal, sehingga bed asal masuk `WaitingCleaning`. Dari situ `POST bed-management/beds/{id}/cleaning-attempts` → `PATCH cleaning-attempts/{id}/complete` → `POST beds/{id}/readiness-verifications` (`IsReady = true`) membawanya ke `Ready`/`Available`. Penugasan unit memakai fallback SuperAdmin; `BedManagement:EnableWorkflow` menyala. Sesudah itu pasien bisa dipindah balik, lalu bed tujuan dibersihkan dengan cara yang sama | Dua baris penempatan berakhir `Transfer` beserta alasan transfer, dua kejadian outbox `BED_OCCUPIED` ke Billing (pasien belum punya folio), dua upaya pembersihan, tanda terima operasi bed. Tidak ada yang bisa dihapus lewat API |
| **Pulang** (`EpisodeClosed`/`PatientDeparted`) | **Tidak.** Episode tertutup dan pasien meninggalkan bed; tidak ada jalur membuka ulang episode | Episode pasien uji selesai permanen |
| **Koreksi** (`CorrectedEntry`) | **Tidak** — temuan `BE-NV-011` §5 (bed asal tertahan `Cleaning`/`Unverified`) | — |
| **Batal penempatan** (`AdmissionCancelled`) | **Tidak**; episode dibatalkan | Episode pasien uji batal permanen |

**Keputusan Rizki 9 November 2026.** (1) AC-1 diuji dengan pindah pulang-balik lewat API seperti
rencana di bawah; langkah Rawat Inap memakai SuperAdmin sebagai persiapan dan pemulihan. (2) AC-2
dikecualikan dari bukti runtime; cukup terpetakan ke source. (3) Celah rantai koreksi mula-mula
dicatat saja; keputusan ini **diganti** sesudah jawaban Hamzah (butir berikut).

**Jawaban Muhammad Hamzah dan `NV-DEC-057` (9 November 2026).** Jawaban lengkap tercatat di
`TLM-NV-002` bagian 8. Yang mengubah task ini: `AdmissionCancelled` berarti pasien batal dirawat
dan bed dilepas; koreksi salah pilih bed berarti pasien dipindah secara fisik; koreksi di bed
yang sama jarang tetapi sebaiknya ditambal. Rizki memutuskan `NV-DEC-057`: ketiganya diikuti,
dengan alasan baru "admisi dibatalkan". Hamzah juga merestui pasien 326-C untuk uji pulang-balik
dan menyatakan Billing aman bila bed tujuan sekelas, sehingga skrip uji hanya memilih bed
`SameGrade`.

**Rencana uji AC-1.** Pakai pasien 326-C dan satu bed sekelas di lantai 4 (dipilih skrip dari
`available-beds`). Saat dijalankan, pasien yang dipindah diganti ke 325-A dan 326-C menjadi
pembanding (lihat di bawah tabel), sehingga peran kedua bed pada tabel ini tertukar.
Langkah Rawat Inap dijalankan lewat SuperAdmin sebagai persiapan dan pemulihan; aksi Nurse View
memakai tablet uji dan akun Sari.

| No | Langkah | Hasil yang diharapkan | AC |
| ---: | --- | --- | --- |
| 1 | Tablet uji di 326-C dan 325-A (persiapan, SuperAdmin) | Aktif | — |
| 2 | 326-C memanggil biasa; Sari mengklaim. 325-A memanggil Darurat | Dua panggilan aktif: `Claimed` dan `Waiting` | — |
| 3 | Pindahkan pasien 326-C ke 401-A; catat jam | Penempatan 326-C `EndReason = Transfer` | — |
| 4 | Pantau lewat SELECT tiap 5 detik sampai panggilan 326-C berubah | `ClosedBySystem`, `CloseReason = PatientTransferred`; `ClosedAt − EndDateTime ≤ 60 detik`; satu jejak `ClosedBySystem`, `ActorKind = System`, tanpa `ActorUserId` | AC-1 |
| 5 | `GET /current` tablet 326-C; `GET /active` lantai 3 dan 4 | `IsBedOccupied = false`, tanpa panggilan; lantai 3 hanya 325-A; lantai 4 tanpa panggilan dari 401-A | AC-1 |
| 6 | Panggilan 325-A | Tetap `Waiting`, versi tidak berubah | AC-4 (pemilahan) |
| 7 | Log backend | Satu baris "menutup otomatis" untuk 326-C; tanpa identitas pasien | — |
| 8 | Pemulihan: bersihkan 326-C, pindahkan balik 401-A → 326-C, bersihkan 401-A; nonaktifkan tablet; pulihkan policy Sari | Pasien kembali di 326-C; 401-A `Available`/`Ready` | — |

**AC-2 (pulang)** tidak bisa dipulihkan, sehingga bukti runtime-nya dikecualikan (keputusan
butir 2). Jalurnya sama dengan transfer; yang berbeda hanya pemetaan alasannya. Catatan untuk UAT:
saat mengukur jeda pulang, pembanding yang tepat adalah `InpBedPlacement.UpdateDateTime` (jam
pencatatan), karena `EndDateTime` pulang bisa diisi jam berangkat yang lebih awal dari jam
pencatatannya.

**AC-3** tidak diuji runtime: koreksi tidak bisa dipulihkan (cacat yang dikonfirmasi Hamzah,
`TLM-NV-002` bagian 8 no. 6–7) dan batal admisi bersifat permanen. Kartu memintanya dicatat
"belum terbukti" dalam keadaan itu.

**Pasien uji diganti ke 325-A (keputusan Rizki 9 November 2026).** Percobaan pertama berhenti
sebelum menulis apa pun. Pasien 326-C wajib ruang isolasi: semua bed lantai 4 ditolak
`ISOLATION_REQUIRED`, dan dua bed isolasi yang tersisa (316-A, 317-A) berkelas `Unknown`, yang
ditolak transfer (`CLASS_ORDER_UNVERIFIED`). Pasien 325-A punya 20 bed sekelas di lantai 4. Rizki
memilih 325-A sebagai pasien yang dipindah dan 326-C sebagai pembanding, dan ia sendiri yang
mengabari Hamzah (restu Hamzah semula untuk 326-C).

### 5.3 Hasil uji HTTP — 9 November 2026

Verifikasi developer oleh agent, bukan UAT. Putusan diambil dari JSON mentah, log backend, dan
SELECT read-only. Tablet uji: `NVT-000021` (325-A) dan `NVT-000022` (326-C). Bed tujuan: 420-B
(lantai 4, `SameGrade`, dipilih dari `available-beds`).

| No | Terukur | AC |
| ---: | --- | --- |
| 1 | 325-A memanggil biasa lalu diklaim Sari (`Claimed`); 326-C memanggil Darurat (`Waiting`) | — |
| 2 | Transfer 325-A → 420-B `200`, kategori 3; penempatan 325-A berakhir `Transfer` pukul 11.52.03,977 WIB | — |
| 3 | Panggilan 325-A `ClosedBySystem` pukul 11.52.05,452 WIB: **jeda 1,47 detik**; alasan 6 (`PatientTransferred`); `ClosedByUserId` kosong; pengklaim Sari tetap tercatat; versi 1 → 2; jejak `Pressed`(tablet) → `Claimed`(perawat) → `ClosedBySystem`(`System`, tanpa `ActorUserId`, `CreateBy` kosong, alasan 6) | AC-1 |
| 4 | `GET /current` tablet 325-A → `isBedOccupied` false, tanpa panggilan aktif; tekan baru → `409` `VAL-NV-28` | AC-1 |
| 5 | Konsol lantai 3 hanya memuat 326-C; lantai 4 kosong; bed 420-B tanpa panggilan apa pun | AC-1 |
| 6 | Panggilan 326-C (penempatan masih aktif) tetap `Waiting`, versi tidak berubah, tanpa jejak `ClosedBySystem` | AC-4 (pemilahan) |
| 7 | 35 detik kemudian, sesudah satu putaran lagi: 325-A tetap satu jejak `ClosedBySystem` dan versi tetap; 326-C tidak tersentuh | Idempotensi |
| 8 | Log: tepat satu baris "menutup otomatis" untuk 325-A (`BD-RSMMC-00029`, lantai 3, `Claimed` → `ClosedBySystem`, `PatientTransferred`, dasar `Transfer`) dan satu ringkasan putaran (1 kandidat, 1 ditutup, 0 dilewati, 0 gagal); nol galat pemeriksaan; nol sandi, identitas kedua pasien, dan token sepanjang hari | — |
| 9 | Pemulihan: 325-A dibersihkan dan diverifikasi siap, pasien dipindah balik 420-B → 325-A (`200`, kategori 3), 420-B dibersihkan dan diverifikasi siap; panggilan 326-C ditutup lewat "Perawat hadir"; kedua tablet nonaktif; policy Perawat Rawat Inap 20 → 20 identik | — |

**Catatan jeda.** Jeda 1,47 detik terjadi karena putaran pemeriksaan kebetulan berjalan sesaat
sesudah transfer. Batas atasnya menurut desain adalah satu interval (30 detik) ditambah lama satu
putaran, tetap di bawah 60 detik `NV-DEC-052`.

**Pemeriksaan DB sesudah uji (SELECT read-only, 11.54 WIB).**

| Hal | Keadaan |
| --- | --- |
| Penempatan episode 325-A | Urutan 1 di 325-A berakhir `Transfer` 11.52.03; urutan 2 di 420-B 11.52.03–11.52.45 (42 detik, kategori 3) berakhir `Transfer`; urutan 3 di 325-A aktif |
| Bed | 325-A `Occupied`/`Unverified` dan 420-B `Available`/`Ready` — sama dengan sebelum uji; 326-C tidak berubah |
| Outbox Rawat Inap | 2 kejadian `BED_OCCUPIED` (`ROOM_STAY`) di jendela uji. Menurut Hamzah, singgah < 1 menit di kelas yang sama tidak membuat tagihan ganda |
| Upaya pembersihan | 2 (325-A dan 420-B), keduanya terverifikasi |
| Sisa Nurse View | 0 panggilan aktif di lantai 3–4; 0 tablet aktif; panggilan uji tersimpan sebagai riwayat |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 pindah → paling lambat 60 detik `ClosedBySystem` "pasien pindah"; tablet `IsBedOccupied = false`; lantai 4 tidak menerima panggilan | ✅ Terbukti 9 Nov 2026 | `ResolveCloseReason` (`Transfer` → `PatientTransferred`); putaran 30 detik; uji no. 3–5 (jeda 1,47 detik) |
| AC-2 pulang (`EpisodeClosed`/`PatientDeparted`) → "pasien pulang" | ✅ Terpetakan ke source; bukti runtime **dikecualikan atas keputusan Rizki 9 November 2026** (pemulangan tidak dapat dipulihkan) | `ResolveCloseReason` (`EpisodeClosed`, `PatientDeparted` → `PatientDischarged`); jalur simpan sama dengan AC-1 |
| AC-3, bunyi `NV-DEC-057`: koreksi di bed yang sama → tetap aktif lalu mengikuti pengganti; koreksi ke bed lain → "pasien pindah"; `AdmissionCancelled` → "admisi dibatalkan" | 🟡 Terpetakan ke source; runtime belum terbukti (kartu: dicatat bila koreksi tidak bisa dibuat; batal admisi permanen) | `ResolveDepartureAsync` (rantai `SupersededByCorrectionId`, pemeriksaan bed, baris aktif dibiarkan); `ResolveCloseReason` (`AdmissionCancelled` → nilai `8`) |
| AC-4 satu panggilan gagal tidak menahan yang lain; galat satu putaran tidak menghentikan host | ✅ Terbukti lewat penelusuran source (cara yang diminta kartu) | `try`/`catch` per kandidat di `CloseCallsForDepartedPatientsAsync`; `try`/`catch` per putaran di `ExecuteAsync` dengan `Task.Delay` sesudahnya; hanya pembatalan host yang menghentikan loop |
| AC-5 diff tanpa berkas `Areas/HealthServices/InPatientManagement/` | ✅ Terbukti | Review diff §5 |
| DoD — AC-1–5 terpetakan ke source | Terpenuhi | Tabel ini |
| DoD — build Rizki lulus | Terpenuhi | Pernyataan Rizki; DLL 11.43 memuat kode task ini |
| DoD — catatan jeda pindah → tutup tersimpan | Terpenuhi | §5.3 (1,47 detik) dan `hasil.json` `catatan.jeda` |
| DoD — laporan dirujuk dari `TLM-NV-002` | Terpenuhi | Baris 2026-11-09 di `TLM-NV-002` §7 |

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (orkestrasi di Module Service), `QBE-LOG-001`
(jejak `ClosedBySystem` dengan aktor `System` + log teks), `QBE-TXN-001` (panggilan dan jejaknya
dalam satu `SaveChanges`), `QBE-ENUM-001` (enum milik modul; enum Rawat Inap hanya dibaca),
`QBE-AUD-001` (jejak database terpisah dari log aplikasi), `QBE-MOD-001`. Tidak berlaku:
`QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*` selain penamaan kelas `Nvw*`, `QBE-API-001`,
`QBE-PERM-001`, `QBE-DTO-001`, `QBE-CODE-*`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DB-*`,
`QBE-DEL-001`.

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix
`Nvw` `ACTIVE` (registry repository baris 45; salinan suite skill belum memuat baris ini, seperti
yang sudah dicatat registry) · applicability `NEW CODE`, ditambah `TOUCHED LEGACY` untuk dua baris
`Program.cs` · branch `rizkiG`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sinyal realtime belum dikirim (`BE-NV-014`). Sesudah build, pemeriksaan langsung berjalan di DB dev bersama setiap kali backend hidup dengan `Runtime:Role` `All` atau `Worker` |
| Masalah yang diketahui | (1) Celah rantai koreksi (panggilan menunjuk penempatan yang sudah dikoreksi di bed yang sama, lalu pasien dipindah) **sudah ditambal** sesuai `NV-DEC-057`: pemeriksaan mengikuti `SupersededByCorrectionId`. Panggilan dengan pengganti aktif di bed yang sama dinilai ulang tiap putaran; jumlahnya kecil karena koreksi semacam itu sangat jarang (Hamzah). (2) `NV-UNK-006`: Hamzah menegaskan transfer dicatat saat pasien benar-benar pindah; rencana pindah memakai reservasi. Pengamatan lapangan `G-16` tetap milik kepala ruang. (3) Bunyi AC-5 `FE-NV-011` ("bed kini kosong → area perawat tetap ada selama panggilan aktif") tetap benar untuk jeda ≤ 60 detik sebelum pemeriksaan menutup panggilan |
| Risiko tersisa | Rawat Inap mengganti arti `EndReason` atau cara mengisi `EndDateTime` → perilaku penutupan berubah diam-diam. `TLM-NV-002` sudah disampaikan dan dijawab 9 Nov 2026; Hamzah menyatakan nilai 1–5 dan semantik `EndDateTime` stabil |
| Perubahan sampingan | Kode: `NONE`. Data di DB dev bersama akibat uji: tablet `NVT-000021`, `000022` dibuat lalu dinonaktifkan; 2 panggilan selesai beserta jejaknya tersimpan sebagai riwayat; policy Perawat Rawat Inap diubah sementara lalu dipulihkan identik. Data Rawat Inap (direstui Rizki dan Hamzah, §5.2): 2 baris penempatan `Transfer` pada episode 325-A, 2 outbox `BED_OCCUPIED`, 2 upaya pembersihan; pasien dan kedua bed kembali ke keadaan semula |
| Interupsi | Percobaan pertama berhenti sebelum menulis apa pun karena pasien 326-C wajib isolasi; pasien uji diganti ke 325-A atas keputusan Rizki |
| Status Git | `??` `NvwPatientCallReconciliationService.cs`, `NvwPatientCallReconciliationHostedService.cs`; ` M` `NvwPatientCallCloseReason.cs`; `MM` `Program.cs` (dua baris task ini di atas baris `BE-NV-011` yang sudah di-stage); laporan ini; amandemen blueprint `NV-DEC-057` (lihat Target tulis); baris status roadmap, traceability, dan `TLM-NV-002`. Perubahan `BE-NV-011`/`BE-NV-012` tetap seperti sebelum task ini |
| Langkah berikutnya | `BE-NV-014` (sinyal realtime dan grup hub) kini boleh dimulai karena `BE-NV-012` dan `BE-NV-013` ✅; ia menambahkan sinyal ke pemeriksaan ini juga. `BE-NV-015` (strip TV) boleh paralel. Rizki mengabari Hamzah bahwa uji memakai pasien 325-A |
