# Impact Scan Kemampuan — Reservasi Pasien Prioritas (Amendment RP)

| Field | Nilai |
|---|---|
| Blueprint | `rawat-jalan` (umbrella `RJ-BIL-BP-001`), Amendment RP, bentuk `SINGLE` (`RJ-PRI-DEC-003`) |
| Mode | Impact scan terbatas untuk Amendment RP, 16 Oktober 2026 |
| Keputusan yang diaudit | `RJ-PRI-DEC-001` s.d. `RJ-PRI-DEC-024` dan `RJ-PRI-FE-001` s.d. `004` ([00-interview-decisions.md](00-interview-decisions.md), bagian *Amendment Pass 2026-10-16 — Reservasi Pasien Prioritas*). Terkait `crm-membership` `CRM-DEC-001` s.d. `032` |
| Backend SHA | `551e1cbb` (branch `sukmagp`) |
| Frontend SHA | `1fd858322` (branch `sukmagpV2`) |
| Capability map induk | [01-existing-capability-map.md](01-existing-capability-map.md) (BE `9b26be38`) dan [01-capability-impact-scan-daftar-pasien-rj.md](01-capability-impact-scan-daftar-pasien-rj.md) (BE `245f0464`). Keduanya `STALE` terhadap SHA di atas (`F-RP-7`). Berkas ini **menambah**, bukan menggantikan, map induk, dan hanya untuk klaster yang disentuh Amendment RP |
| Kontrak yang berlaku | Belum ada kontrak baru untuk Amendment RP. Kontrak as-is yang disentuh ada di bagian 4 |
| Sifat audit | Read-only. Tidak ada source aplikasi yang diubah. Database tidak di-query |
| Status | `CURRENT` untuk kedua SHA di atas. Menjadi `STALE` bila salah satu SHA berubah pada berkas yang dirujuk |

**Singkatan bukti.** `BE` = `NewQuilvianSystemBackend`, `FE` = `V2QuilvianSystemFrontendDev/src`. Bentuk bukti
`path:baris`. Bila SHA tidak ditulis, yang berlaku SHA pada tabel di atas.

## 1. Ringkasan untuk pembaca umum

| No | Temuan | Artinya |
|---:|---|---|
| 1 | Pembagi nomor antrean sudah satu pintu untuk kiosk dan petugas, dan sudah mengunci nomor agar tidak ganda | Aturan kelipatan 5 cukup mengganti cara memilih nomor. Tidak perlu membangun ulang |
| 2 | Layar display mengambil daftar antrean lewat **satu** penyaring di backend | Menyembunyikan antrean prioritas dari daftar dan suara cukup di satu tempat |
| 3 | **Tetapi** layar display juga menerima kabar panggilan seketika (realtime) dan langsung menampilkannya tanpa lewat penyaring itu. Kabar itu membawa nomor antrean, penanda prioritas, bahkan `PatientId` | Tanpa perbaikan, nomor prioritas akan **sekilas muncul** di display saat perawat menekan Panggil. `PatientId` di layar publik juga melanggar `RJ-DOC-DEC-057` yang sudah berlaku |
| 4 | Aturan member di antrean RJ masih memakai kueri sendiri, dan untuk booking ke tanggal mendatang menilai status pada **tanggal kunjungan** | Perlu dialihkan ke resolver CRM dan ke tanggal pendaftaran (`RJ-PRI-DEC-006`, `007`) |
| 5 | Belum ada wadah "reservasi" sama sekali, belum ada halaman publik bertoken, dan belum ada checklist skrining | Bagian ini memang baru. Polanya (nomor seri, proses terjadwal, batas laju permintaan, halaman tanpa login) sudah ada untuk ditiru |
| 6 | Pembuatan kunjungan sudah memeriksa jadwal dan kuota dokter | Saat reservasi dilengkapi, aturan `RJ-PRI-DEC-016` otomatis berlaku bila memakai jalur yang sama |
| 7 | Data status member pasien sudah ikut terkirim ke layar Pendaftaran RJ, tetapi belum ditampilkan | Cukup pekerjaan tampilan |
| 8 | Konfigurasi backend sudah memuat domain `https://quilvian-mmchospital.com` | `RJ-PRI-OQ-003` tidak mulai dari nol. Apakah domain itu dapat diakses dari internet belum bisa dibuktikan dari source |

### Contoh: apa yang terjadi hari ini bila hanya daftar display yang disaring

> Pukul 09.15 perawat Penyakit Dalam menekan **Panggil** untuk nomor 10 (pasien Gold, prioritas). Backend
> mengirim kabar realtime ke group nurse station cluster. Layar display di ruang tunggu juga anggota group itu,
> sehingga ia menerima kabar yang sama dan langsung menampilkan "Nomor 10" di bagian panggilan. Beberapa detik
> kemudian layar memuat ulang daftar dari backend. Karena daftar sudah disaring, nomor 10 hilang lagi. Hasilnya,
> nomor prioritas tetap terlihat sekilas. Ini melanggar `RJ-PRI-DEC-005`.

## 2. Batas audit

Yang diperiksa hanya kemampuan yang disentuh Amendment RP:

1. pembagi nomor antrean dan konfigurasinya;
2. penyaringan display publik, suara panggilan, dan kabar realtime ke display;
3. klasifikasi member/prioritas di antrean RJ dibanding resolver CRM;
4. wadah reservasi, nomor seri, dan proses terjadwal akhir hari;
5. pola endpoint dan halaman publik tanpa login, serta batas laju permintaan;
6. SignalR untuk daftar admisi realtime;
7. tiket kiosk dan bukti daftar petugas;
8. worklist nurse station dan Klinis Dokter;
9. pola hak akses dengan aksi khusus;
10. status member di layar Pendaftaran RJ;
11. pola tombol `wa.me`.

**Di luar batas:** aturan internal billing, IGD, rawat inap, isi master tingkat, data database, konfigurasi server
produksi yang sebenarnya, dan aksesibilitas domain dari internet.

## 3. Peta kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`path:baris@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-RP-01` | Nomor prioritas kelipatan 5 tanpa batas (`RJ-PRI-DEC-004`) | Registration | BE `Areas/HealthServices/RegistrationManagement/Services/OutpatientQueueNumberAllocator.cs:13-31` (`OutpatientQueueNumberOptions`, daftar tetap `{ 1, 3, 5, 10, 15 }`), `:63-134` (`AllocateAsync`), `:85-88` (`pg_advisory_xact_lock` per tanggal dan service unit), `:93-109` (nomor yang pernah terbit tidak dipakai ulang); `Program.cs:503`, `:507-508`; `appsettings.json:176` | Extend | Ganti daftar tetap dengan aturan "kelipatan N" (bawaan 5). Prioritas mengambil kelipatan N terkecil yang belum terpakai, tanpa jalur "nomor cadangan habis". Reguler melewati semua kelipatan N. Kunci, cakupan nomor (`ResolveScopeAsync`), dan larangan pakai ulang **dipakai apa adanya**. Kiosk dan petugas sudah lewat jalur yang sama (`F-AQ-2`) | Kunci konfigurasi `ReservedPriorityNumbers` berubah makna. Lingkungan lain yang mengisinya perlu diberi tahu. Contoh perilaku baru: Reguler 1, 2, 3, 4, 6; Prioritas 5, 10, 15 |
| `CAP-RP-02` | Antrean prioritas tidak ada di daftar display dan suara (`RJ-PRI-DEC-005`) | Registration | BE `Controllers/QueueDisplayRuntimeController.cs:21` (`[Authorize]`), `:89-170` (`summary`, `items`), `:172-226` (`called`, teks dan audio suara), `:230-252` (`BuildDisplayQueueQuery`, satu-satunya penyaring, sudah memuat `QueueAudienceMode`), `:156` (urutan prioritas di atas), `:317-337` (audio hanya untuk antrean yang lolos penyaring) | Reuse with adapter | Tambahkan syarat `IsPriorityQueue == false` di `BuildDisplayQueueQuery`. Ketiga endpoint display dan suaranya ikut tersaring. Urutan baris 156 menjadi tidak relevan | Rendah. Ruang kerja perawat dan dokter memakai controller lain, sehingga tidak terdampak |
| `CAP-RP-03` | Kabar realtime ke layar display tidak membocorkan antrean prioritas maupun identitas (`RJ-PRI-DEC-005`, `RJ-DOC-DEC-057`) | Registration | FE `lib/hooks/queue-display/use-queue-display-screen.jsx:1518-1531` (display bergabung ke group nurse station cluster lewat `syncNurseStationClusters`), `:1166-1185` (kandidat panggilan dari kabar realtime langsung digabung ke tampilan, `mergeCallIntoSnapshot`); `utils/queue-display/queue-display-call-coordinator-utils.jsx:474`. BE `Areas/HealthServices/RegistrationManagement/DTOS/QueueRealtimeDtos.cs:6-45` (payload memuat `QueueNumber`, `QueueCode`, `IsPriorityQueue`, `PatientId`, `EncounterId`); `Services/QueueRealtimeService.cs:229-275` (dikirim ke group nurse station dan dokter); `Hubs/QueueHub.cs:43-58` | Repair | Dua hal. (a) Display ikut menerima kabar yang ditujukan ke perawat, lalu menampilkannya tanpa penyaring backend. Antrean prioritas akan muncul sekilas. (b) Payload yang sama membawa `PatientId` dan `EncounterId` ke perangkat publik, padahal `RJ-DOC-DEC-057` melarangnya. Pilihan perbaikan (group khusus display dengan payload ringkas, atau penyaring di sisi display) diputuskan saat desain. Lihat `CONF-RP-01` | Sedang. (b) sudah terjadi hari ini, bukan akibat Amendment RP |
| `CAP-RP-04` | Satu aturan member di antrean RJ (`RJ-PRI-DEC-006`, `BE-CRM-022`) | Registration + CRM | BE `Services/OutpatientQueueClassificationService.cs:43-105`, khususnya `:56-69` (kueri sendiri: `JoinDate`, `ExpiredDate` boleh kosong, `IsActive`, `.Date` versi UTC); `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipStatusResolver.cs:53` (`GetTodayWib`), `:59-76` (`QueryActiveOn`), `:77` (`GetActiveAsync`); `Program.cs:501`, `:505`; `Controllers/PatientEncounterController.cs:100` (cadangan `new OutpatientQueueClassificationService(dbContext)` bila DI tidak menyuntik) | Extend | Ganti kueri `:56-69` dengan resolver. Kebijakan tingkat (`PriorityQueue`, `PriorityLevel`, `QueueAudience`, `PublicDisplayMode`) tetap dibaca seperti sekarang. Cadangan di baris 100 perlu menerima resolver | Rendah, sesuai `CRM-DECREQ-001` |
| `CAP-RP-05` | Status member dinilai pada tanggal pendaftaran untuk booking ke tanggal mendatang (`RJ-PRI-DEC-007`) | Registration | BE `Controllers/PatientEncounterController.cs:1013-1027` (`ClassifyAsync(request.PatientId, targetEncounterDate, …)` lalu `AllocateAsync`) | Extend | Sekarang tanggal yang dipakai adalah tanggal kunjungan. Klasifikasi harus memakai tanggal hari ini (WIB), sedangkan nomor tetap dialokasikan untuk tanggal kunjungan | Mengubah hasil untuk booking mingguan yang sudah ada. Contoh: booking 1 Okt untuk 20 Okt, membership habis 13 Okt. Sekarang Reguler, sesudah perubahan prioritas |
| `CAP-RP-06` | Wadah Reservasi Prioritas di luar kunjungan, beserta status dan riwayat (`RJ-PRI-DEC-009`, `021`, `022`, `023`, `024`) | Registration | Pencarian `class \w*(Appointment|Booking|Reservation)` di BE: hanya `InpBedReservation`, `InpSurgeryBookingAdapter` (rawat inap) dan cuti HR. `RegPatientEncounter` (`Models/RegPatientEncounter.cs:13-190`) adalah kunjungan. `EncounterStatus.Draft` ada di enum tetapi tidak dipakai (`Enums/EncounterStatus.cs:7-8`). Registry: `HealthServices / RegistrationManagement / Registration`, prefix `Reg`, `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:23`) | Missing | Entity, status, riwayat, dan endpoint baru. Prefix `Reg` sudah terdaftar, sehingga tidak perlu registrasi prefix baru. Desain perlu memastikan arti `LEGACY` pada baris registry itu tidak menghalangi model baru | Sedang. Ini inti amendment |
| `CAP-RP-07` | Nomor reservasi otomatis (`RJ-PRI-OQ-002`) | Platform | BE `Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs:84` (`AllocateAsync`, transaksi sendiri, nomor hangus bila gagal); contoh pemakaian `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipService.cs:1049-1055` | Ready to reuse | Cukup satu `SequenceKey` dan prefix baru, misalnya `RSV` dengan reset harian | — |
| `CAP-RP-08` | Penutupan "Tidak Datang" otomatis di akhir hari (`RJ-PRI-DEC-013`) | Registration | BE `Program.cs:1144` (`KioskEncounterClosureHostedService`), `:1149` (`PatientMembershipLifecycleHostedService`), `:129-142` dan `:1137-1140` (proses terjadwal hanya jalan pada `Runtime:Role` `Worker`/`All`); `appsettings.json:184-187` (`KioskEncounterClosure`, `ServiceDayEndHour` 21) | Ready to reuse | Pola hosted service yang idempoten dan mengejar ketertinggalan sudah ada dua. Batas waktu `RJ-PRI-DEC-013` adalah 23.59 WIB, berbeda dari penutupan kiosk (pukul 21) | Penutupan berjalan di container Worker, jadi tidak dapat mendorong kabar realtime. Daftar admisi cukup memuat ulang |
| `CAP-RP-09` | Endpoint publik tanpa login bertoken (`RJ-PRI-DEC-012`) | Registration | BE: `[AllowAnonymous]` hanya ada di `Areas/Administrator/MasterData/Controllers/KioskDeviceController.cs:1130` dan `QueueDisplayDeviceController.cs:587` (login perangkat). Pencarian `PublicToken|ShareToken|TokenHash|ConfirmationToken` di `Areas`: tidak ada pola link bertoken | Missing | Endpoint anonim pertama untuk data bisnis. Desain wajib menetapkan: kode acak disimpan sebagai hash, respons tanpa data pribadi, respons seragam untuk kode salah atau kedaluwarsa, dan audit | Tinggi bila salah rancang. Ini satu-satunya pintu sistem tanpa login yang menyentuh data pasien |
| `CAP-RP-10` | Batas laju permintaan untuk link publik (`RJ-PRI-OQ-003`) | Platform | BE `Program.cs:1276-1300` (`AddRateLimiter`, partisi per user atau IP, `FixedWindow`, 429 terbaca "coba lagi"), `:2185` (`UseRateLimiter`); `Areas/HealthServices/RegistrationManagement/Controllers/KioskPatientLookupController.cs:67-69` (`[EnableRateLimiting]`) | Reuse with adapter | Tambah satu policy baru berpartisi IP untuk endpoint publik | Di belakang proxy, IP asli harus diteruskan (header forwarded). Konfigurasi server tidak diaudit |
| `CAP-RP-11` | Halaman publik di frontend tanpa login (`RJ-PRI-FE-001`) | Frontend platform | FE `proxy.js:6-15` (`PUBLIC_PATH_PREFIXES`), `components/ui/auth-wrapper.jsx:15-22` (daftar yang sama di sisi klien); `lib/axiosInstance/InstanceAxios.jsx:1161-1173` (`canRefreshForRequest`, opsi `_skipAuthRefresh`), `:1131-1158` (`redirectToLogin`) | Reuse with adapter | Tambahkan prefix route baru di **dua** daftar publik. Panggilan API dari halaman itu memakai `_skipAuthRefresh`, agar tidak mencoba refresh sesi atau mengalihkan ke login | Rendah. Kedua daftar harus tetap sama |
| `CAP-RP-12` | Domain publik untuk link (`RJ-PRI-OQ-003`) | IT/infrastruktur | BE `appsettings.json:44-51` (`Cors:AllowedOrigins`: `https://dev.quilvian-mmchospital.com`, `https://sta.quilvian-mmchospital.com`, `https://quilvian-mmchospital.com`) | Unknown | Domain tercatat di konfigurasi. Apakah domain dan API backend dapat dibuka dari internet (bukan hanya jaringan RS) tidak bisa dibuktikan dari source. Lihat `UNK-RP-01` | Bila hanya intranet, link WA tidak bisa dibuka pasien dari HP |
| `CAP-RP-13` | Daftar Reservasi Prioritas realtime di layar admisi (`RJ-PRI-DEC-014`) | Registration | BE `Program.cs:247` (`AddSignalR`), `:2190` (`/hubs/queues`); `Hubs/QueueHub.cs:17-18` (`[Authorize]`), `:43-140` (group hanya nurse station, dokter, poli dokter, dan IGD); `Services/QueueRealtimeService.cs:55-80` (nama group). FE `lib/realtime/queue-realtime-client.js:143-248` | Extend | Tambah group untuk petugas admisi beserta event "reservasi berubah". Konfirmasi pasien masuk lewat HTTP ke container Web, yang memegang SignalR, sehingga kabar dapat didorong seketika | Rendah |
| `CAP-RP-14` | Pelengkapan reservasi menjadi kunjungan dengan validasi jadwal dan kuota (`RJ-PRI-DEC-010`, `016`) | Registration | BE `Controllers/PatientEncounterController.cs:852` (`ClinicId` boleh kosong), `:2433-2496` (validasi jadwal: ada, aktif, cocok hari, terima appointment/walk-in), `:2546-2562` (kuota pasien, appointment, walk-in), `:1013-1027` (klasifikasi dan nomor), `:1166-1167` (`QueueClassification` di respons); jalur bersama kiosk dan petugas (`F-AQ-2`) | Reuse with adapter | Pelengkapan memakai jalur pembuatan kunjungan yang sama, dengan dua penyesuaian. Klasifikasi diambil dari snapshot reservasi (`RJ-PRI-DEC-007`), kecuali membership berstatus Dibatalkan (`RJ-PRI-DEC-024`). Kunjungan juga dihubungkan ke reservasinya | Sedang. Jalur pembuatan kunjungan dipakai kiosk, petugas, IGD, dan modul lain |
| `CAP-RP-15` | Satu reservasi per pasien per tanggal dan tolak bila ada kunjungan RJ aktif (`RJ-PRI-DEC-021`) | Registration | Map induk `CAP-DP-09`; BE `Controllers/PatientEncounterController.cs:114` (saklar `BlockActiveEncounter`), `:879-895` (kunci per pasien lalu `FindActiveEncounterAsync`, `RJ-DOC-REV-BE-007`), `:1790-1792` (`OutpatientEncounterRules.FindBlockingEncounterAsync`) | Reuse with adapter | Pemeriksaan kunjungan aktif dipakai ulang. Keunikan reservasi per pasien per tanggal adalah aturan baru pada wadah `CAP-RP-06` | — |
| `CAP-RP-16` | Checklist skrining awal (`RJ-PRI-DEC-019`) | Registration | `EncounterStatus.InNurseScreening` (`Enums/EncounterStatus.cs:19-20`) milik skrining perawat dan sengaja tidak dipakai (`RJ-PRI-DEC-015`). Tidak ditemukan data skrining awal pintu masuk | Missing | Tiga isian Ya/Tidak, petugas, dan jam pada reservasi | Rendah |
| `CAP-RP-17` | Tiket kiosk dan bukti daftar petugas mengarahkan ke ruang tunggu prioritas (`RJ-PRI-DEC-017`, `018`) | Registration + Kiosk | BE `Controllers/PatientEncounterController.cs:1166-1167` dan `DTOs/PatientEncounterDtos.cs:533-543` (`QueueClassification.IsPriorityQueue` sudah ada di respons pembuatan kunjungan). FE `components/view/kiosk/registration/old-patient/kiosk-old-patient-step-ticket.jsx:282` (pola pemberitahuan di tiket, `KioskReferralTicketNotice`); `components/view/health-services/registration-management/outpatient-registration/outpatient-completed-step.jsx:95-99` (teks tetap "Arahkan pasien ke ruang tunggu poliklinik dan tunggu panggilan") | Reuse with adapter | Tiket memakai pola pemberitahuan yang sama berdasarkan `IsPriorityQueue`. Layar selesai petugas mengganti teksnya untuk pasien prioritas. Nama ruang diambil dari konfigurasi (`RJ-PRI-DEC-018`), yang belum ada | Rendah |
| `CAP-RP-18` | Penanda lokasi di worklist perawat dan dokter (`RJ-PRI-DEC-005`, `017`, `018`) | Registration | BE `DTOs/NurseStationQueueDtos.cs:60`, `DTOs/DoctorQueueDtos.cs:95` (`IsPriorityQueue`). FE `components/features/health-services/nurse-station-management/QueuePatientCard.jsx:110-112` dan `components/features/health-services/doctor-queue-features/QueuePatientCard.jsx:156-158` (lencana "Prioritas" sudah tampil) | Extend | Tambah teks lokasi "Ruang Tunggu Prioritas …" di samping lencana. Tombol Panggil tetap berfungsi: statusnya berubah, tetapi tidak muncul di display (`CAP-RP-02`, `CAP-RP-03`) | Rendah |
| `CAP-RP-19` | Suara panggilan dari ruang kerja perawat dan dokter (`RJ-PRI-DEC-005`) | Registration | BE `Controllers/NurseStationQueueController.cs:238`, `Controllers/DoctorQueueController.cs:488` (membuat berkas audio saat Panggil). FE `lib/hooks/health-services/registration-management/use-queue-voice-player.js` tidak dipakai komponen mana pun. Pemutaran hanya di `lib/hooks/queue-display/use-queue-display-screen.jsx` | Ready to reuse | Suara hanya diputar layar display. Setelah `CAP-RP-02`/`03`, antrean prioritas tidak bersuara. Pembuatan berkas audio untuk antrean prioritas boleh dilewati sebagai penghematan, bukan syarat | — |
| `CAP-RP-20` | Hak akses resource baru dengan aksi `Screen`, `Complete`, `Cancel` (`RJ-PRI-DEC-020`) | Shared Platform | BE `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientMembershipController.cs:458-465` (`[AccessAction("Cancel", …, AccessType = AccessTypes.Update)]` + `[AccessPermission("PatientMembership", "Cancel")]`); `Constants/AccessTypes.cs:3-15` (jenis akses hanya Read/Create/Update/Delete); `Areas/HealthServices/RegistrationManagement/OutpatientEncounterExplicitPermissions.cs:26` | Ready to reuse | Aksi khusus didaftarkan dengan `AccessType` Update atau Create. Admin mencentangnya di Akses Role | Operasional. Fitur tidak bisa dipakai sebelum dicentang |
| `CAP-RP-21` | Status member di layar Pendaftaran RJ (scope butir 2, `RJ-PRI-FE-004`) | Registration + CRM | FE `lib/state/slice/health-services/registration-management/outpatient-registration-slice.jsx:105-116` (`searchOutpatientPatients` memakai `searchEmergencyRegistrationPatients`), `lib/services/health-services/registration-management/emergency-registration.service.js:292`; pencarian `isMember|membership|tier` di `components/view/health-services/registration-management/outpatient-registration/**`: tidak ditampilkan. BE `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientDtos.cs:111-117` (`IsMember`, `DefaultMembershipTierName`, `ActivePatientMembershipId`); `Models/MstPatient.cs:91` (proyeksi hanya ditulis resolver) | Extend | Pekerjaan frontend saja: tampilkan tingkat dari proyeksi. Kelayakan reservasi tetap diperiksa backend lewat resolver, bukan dari proyeksi | Proyeksi diperbarui saat status berubah dan oleh proses harian pukul 00.05. Antara 00.00 dan 00.05 label bisa tertinggal. Untuk tampilan dapat diterima |
| `CAP-RP-22` | Tombol kirim lewat `wa.me` (`RJ-PRI-DEC-011`) | Frontend | FE `lib/hooks/health-services/billing-management/billing-invoices/use-dokumen-kasir.js:145` (`window.open("https://wa.me/?text=…")`), `:154` (`mailto:`) | Reuse with adapter | Pola kasir membuka WA **tanpa** nomor tujuan, sehingga petugas memilih kontak sendiri. Untuk reservasi, nomor pasien sebaiknya langsung diisi (`wa.me/62…?text=…`), sehingga perlu penormalan nomor ke format 62 | Salah normalisasi mengirim ke nomor yang salah. Ini kirim manual, jadi petugas masih melihat tujuannya |

## 4. Kontrak as-is yang disentuh

**Grup Swagger:** `Health Services / Registration Management / Queue Display Runtime`

| Method | Path | Hak akses | Perilaku sekarang |
|---|---|---|---|
| `GET` | `/current` | Policy `QueueDisplayRuntimeReadPolicy` | Profil perangkat display, termasuk `QueueAudienceMode` dan `EnableVoiceCalling` |
| `GET` | `/summary` | Policy `QueueDisplayRuntimeReadPolicy` | Ringkasan antrean dari `BuildDisplayQueueQuery`. Antrean prioritas **ikut** dihitung |
| `GET` | `/items` | Policy `QueueDisplayRuntimeReadPolicy` | Daftar antrean, prioritas diurutkan di atas (baris 156) |
| `GET` | `/called` | Policy `QueueDisplayRuntimeReadPolicy` | Antrean yang sedang dipanggil beserta teks dan audio suara |

**Grup Swagger:** `Health Services / Registration Management / Patient Encounter`
(`api/v1/health-services/registration-management/patient-encounters`)

| Method | Path | Hak akses | Perilaku sekarang |
|---|---|---|---|
| `POST` | `/admin` (petugas), `/` dan `/kiosk` (kiosk) | `PatientEncounter : Create` / policy kiosk | Satu jalur `CreateEncounterCoreAsync`: validasi jadwal dan kuota, klasifikasi pada **tanggal kunjungan**, nomor antrean, penjamin. Respons memuat `QueueClassification` |
| `PATCH` | `/{id}/check-in`, `/admin/{id}/check-in` | `PatientEncounter : Update` | Mengisi `CheckedInAt` kunjungan. Tidak berlaku untuk reservasi yang belum menjadi kunjungan |
| `GET` | `/doctor-schedule-quotas` | `PatientEncounter : Read` | Kuota terpakai per jadwal dan tanggal |

**Hub realtime:** `/hubs/queues` (`QueueHub`, wajib login)

| Method hub | Dipakai oleh | Payload yang diterima |
|---|---|---|
| `JoinNurseStationCluster`, `JoinAccessibleNurseStationClusters` | Ruang kerja perawat **dan layar display** | `QueueRealtimeEventResponse`: `QueueNumber`, `QueueCode`, `IsPriorityQueue`, `QueueAudience`, `PatientId`, `EncounterId` |
| `JoinDoctorQueue`, `JoinDoctorClinicQueue` | Klinis Dokter | Sama |

**Endpoint tanpa login yang ada:** hanya `POST …/kiosk-devices/login` dan `POST …/queue-display-devices/login`.

## 5. Conflict dan unknown

| ID | Isi | Dampak | Pemilik | Status |
|---|---|---|---|---|
| `CONF-RP-01` | Kabar realtime ke layar display tidak melewati penyaring backend, dan membawa `PatientId`/`EncounterId` ke perangkat publik (`CAP-RP-03`). Butir (b) sudah bertentangan dengan `RJ-DOC-DEC-057` sebelum Amendment RP | Tanpa perbaikan, `RJ-PRI-DEC-005` tidak terpenuhi (nomor prioritas muncul sekilas) dan pelanggaran `RJ-DOC-DEC-057` berlanjut | Sukma Giri (owner RJ) | Ditutup `RJ-PRI-DEC-025` (16 Okt 2026): diperbaiki penuh dalam Amendment RP lewat group display sendiri dengan payload ringkas |
| `UNK-RP-01` | Apakah `https://quilvian-mmchospital.com` beserta API backend-nya dapat dibuka dari internet publik (`CAP-RP-12`) | Bila tidak, link konfirmasi tidak berfungsi bagi pasien di luar jaringan RS | IT/infrastruktur | Butuh akses lingkungan. Tidak memblokir desain, memblokir go-live |
| `UNK-RP-02` | Apakah proxy produksi meneruskan IP asli klien ke backend (`CAP-RP-10`) | Batas laju per IP akan menganggap semua pasien satu IP | IT/infrastruktur | Butuh akses lingkungan. Memblokir go-live |

## 6. Koreksi dan pelengkap atas fakta `F-RP`

| Fakta | Pelengkap dari impact scan |
|---|---|
| `F-RP-1` | Tetap benar. Pembuatan kunjungan juga mengklasifikasi pada tanggal kunjungan (`CAP-RP-05`) |
| `F-RP-2` | Tetap benar. Pola `wa.me` kasir tidak mengisi nomor tujuan (`CAP-RP-22`) |
| `F-RP-3` | Tetap benar. Belum ada satu pun endpoint bisnis tanpa login (`CAP-RP-09`) |
| `F-RP-4` | Ditambah: kabar realtime ke display melewati penyaring (`CAP-RP-03`) |
| `F-RP-5` | Dipersempit: datanya sudah terkirim, hanya tampilan yang belum ada (`CAP-RP-21`) |
| `RJ-PRI-OQ-003` | Domain sudah tercatat di CORS (`CAP-RP-12`). Yang tersisa aksesibilitas internet (`UNK-RP-01`) dan IP asli (`UNK-RP-02`) |

## 7. Pemicu impact scan berikutnya

Ulangi impact scan terbatas bila salah satu berkas berikut berubah sebelum desain disetujui:
`OutpatientQueueNumberAllocator.cs`, `OutpatientQueueClassificationService.cs`, `QueueDisplayRuntimeController.cs`,
`QueueRealtimeService.cs`, `QueueRealtimeDtos.cs`, `QueueHub.cs`, `PatientEncounterController.cs` (bagian pembuatan
kunjungan), `PatientMembershipStatusResolver.cs`, `use-queue-display-screen.jsx`, `proxy.js`, `auth-wrapper.jsx`.

## 8. Rekomendasi untuk `requirement-completeness-gate`

| No | Butir | Arah |
|---:|---|---|
| 1 | `CONF-RP-01` sudah dijawab (`RJ-PRI-DEC-025`) | Masuk cakupan desain display |
| 2 | `CAP-RP-06`, `09`, `16` adalah kemampuan baru. Requirement-nya sudah lengkap di `RJ-PRI-DEC-009`, `012`, `013`, `019`, `021` s.d. `024` | Siap dinilai |
| 3 | `CAP-RP-01`, `04`, `05` mengubah perilaku yang sudah berjalan (nomor antrean, klasifikasi booking mingguan). Acceptance perlu memuat perbandingan sebelum dan sesudah | Masuk acceptance gate |
| 4 | `UNK-RP-01`/`02` dan `RJ-PRI-OQ-006` (isi master tingkat) memblokir go-live, bukan desain | Dicatat sebagai blocker `IMPLEMENTATION` |
| 5 | Tidak ada konflik dengan keputusan `crm-membership`. `CAP-RP-04` adalah `BE-CRM-022` | Tetap satu task dengan roadmap CRM, atau dipindah ke roadmap RJ. Diputuskan saat `plan-module-delivery` |
