# Kiosk — Impact Scan Kemampuan: Pendaftaran Pasien Online dan Check-in Kiosk (Amendment PO)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` |
| Revisi blueprint | `1` (`approved`, 30 Sep 2026). Amendment PO belum menaikkan revisi; revisi baru ditetapkan saat desain |
| Status dokumen | `draft` — hasil audit read-only, bukan keputusan |
| Masukan | [00-interview-decisions.md](00-interview-decisions.md) bagian 13, keputusan `KSK-DEC-026` s.d. `046`. Addendum §8 menambah `KSK-DEC-047` s.d. `053` dan `KSK-PO-AS-002`, `003` |
| SHA backend | `5e777833` (`sukmagp`, 19 Nov 2026) |
| SHA frontend | `5f18e9f04` (`sukmagpV2`, 19 Nov 2026 — commit `feat(kiosk)` beranda kiosk) |
| Peta sebelumnya | [01-existing-capability-map.md](01-existing-capability-map.md) r1 (kiosk) dan `rawat-jalan/01-capability-impact-scan-reservasi-prioritas.md` pada `551e1cbb`. Keduanya **stale** untuk area di bawah |
| Mode | Impact scan terbatas. Tidak ada source yang diubah |

## 1. Tujuan dan batas audit

**Tujuan:** memastikan kemampuan apa saja yang sudah ada dan dapat dipakai ulang untuk Amendment PO, sebelum desain mengusulkan tabel, endpoint, atau layar baru. Hasilnya mencegah dua hal: membangun ulang yang sudah ada, dan menganggap siap sesuatu yang belum ada.

**Yang diaudit** hanya area yang disentuh keputusan Amendment PO:

1. Wadah data dan status pendaftaran online (`KSK-DEC-030`, `037`, `045`).
2. Akses tanpa login, batas laju, captcha, dan CORS (`KSK-DEC-038`, `044`).
3. Pengenalan pasien lama, pasien baru, dan pembuatan No. RM (`KSK-DEC-031`, `032`, `039`).
4. Jadwal dokter, kuota appointment, dan pembuatan kunjungan RJ (`KSK-DEC-030`, `036`, `046`).
5. General consent, penjamin, dan rujukan (`KSK-DEC-034`, `035`).
6. Kode, pengiriman email/WA, check-in, dan Lupa Kode (`KSK-DEC-033`, `040`, `042`, `043`).
7. Layar frontend publik dan kiosk.

**Di luar audit:** billing, antrean display, nurse station, dan modul lain di luar titik sentuh di atas.

## 2. Ringkasan

Dari 37 kemampuan (29 pada audit awal, ditambah 8 pada addendum §8):

| Status | Audit awal | Addendum §8 | Jumlah | Arti singkat |
| --- | ---: | ---: | ---: | --- |
| Ready to reuse | 5 | 2 | 7 | Dapat dipakai apa adanya |
| Reuse with adapter | 11 | 4 | 15 | Inti sudah ada; perlu penyambung atau kontrak baru di atasnya |
| Extend | 6 | 2 | 8 | Perlu menambah perilaku pada kode yang sudah ada |
| Missing | 6 | 0 | 6 | Belum ada sama sekali |
| Unknown | 1 | 0 | 1 | Perlu jawaban di luar source code |
| Conflict | 0 | 0 | 0 | — |

Kesimpulan utama:

- **Pola Reservasi Prioritas hampir seluruhnya dapat ditiru.** Pola itu sudah punya endpoint tanpa login, hash kode, batas laju per IP, penutupan Tidak Datang yang mengejar hari tertinggal, dan penutupan otomatis saat pasien didaftarkan lewat jalur lain. Yang berbeda adalah panjang kodenya: kode Reservasi Prioritas tidak dapat diketik di kiosk (`CAP-PO-07`).
- **Consent dan rujukan di backend wajib terikat ke kunjungan.** Kunjungan baru ada saat check-in (`KSK-DEC-030`), jadi keduanya harus ditampung dulu di pendaftaran online, lalu ditulis saat check-in (`CAP-PO-18`, `CAP-PO-20`).
- **Semua endpoint pilihan yang dibutuhkan halaman publik masih memerlukan login** sebagai akun kiosk atau petugas. Endpoint itu meliputi poli, jadwal dokter, kuota, kartu asuransi, faskes perujuk, dan diagnosa (`CAP-PO-09`).
- **Email, captcha, dan panggilan HTTP keluar belum ada sama sekali di backend.** CSP frontend juga belum mengizinkan Cloudflare (`CAP-PO-04`, `CAP-PO-05`).

## 3. Peta kemampuan

Bukti ditulis sebagai `repo/path#symbol` pada SHA di atas. `BE` = `NewQuilvianSystemBackend`, `FE` = `QuilvianSystemFrontendDev`.

### 3.1 Data dan status

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-PO-01` | Wadah pendaftaran online beserta status Menunggu Check-in, Menjadi Kunjungan, Tidak Datang, Didaftarkan lewat Jalur Lain, dan riwayatnya | Registration (`Reg`) | Tidak ada. Pola terdekat `BE/Areas/HealthServices/RegistrationManagement/Models/RegPriorityReservation.cs`, `RegPriorityReservationHistory.cs`, `Enums/PriorityReservationStatus.cs` (status `Converted`, `NoShow`, `RegisteredElsewhere` sudah ada artinya) | Missing | Entity, riwayat, enum status, konfigurasi EF, dan migration baru dengan prefix `Reg`. Wadah ini juga menampung data yang belum boleh masuk master: data diri pasien baru, kontak WA/email, pilihan penjamin, rujukan, dan consent | Sedang. Inti amendment |
| `CAP-PO-12` | Data diri pasien baru yang belum menjadi master pasien | Registration | Tidak ada. Master pasien hanya dibuat lewat `BE/Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs#Create` (`POST .../patients` dan `/kiosk`) | Missing | Bagian dari `CAP-PO-01` (`KSK-DEC-032`) | Rendah |
| `CAP-PO-08` | Nomor pendaftaran yang mudah dibaca, bila desain membutuhkannya | Registration | `BE/.../Services/OutpatientPriorityReservationService.cs#ReservationNumberPrefix` memakai `NumberSeriesAllocator` (`RSV-…`) | Ready to reuse | Hanya bila desain menambah nomor selain kode. Tidak diminta keputusan | Rendah |

### 3.2 Akses publik dan perlindungan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-PO-02` | Endpoint bisnis tanpa login | Registration | `BE/.../Controllers/PriorityReservationArrivalController.cs:28-36`: `[AllowAnonymous]`, sengaja tanpa `[AccessController]`, `MarkNoStore()`, pesan seragam untuk kode salah, path disamarkan di log (`RedactRequestPath`) | Reuse with adapter | Controller publik baru dengan pola yang sama. Satu-satunya preseden endpoint bisnis anonim di RJ | Sedang. Menambah permukaan serangan dari internet |
| `CAP-PO-03` | Batas laju per alamat IP | Platform | `BE/Program.cs:2016-2075` (`AddRateLimiter`, policy `PriorityReservationArrivalPublic` jendela tetap per IP); `Program.cs:2234-2243` `ForwardedHeaders` dengan `KnownProxies` dikosongkan; `app.UseRateLimiter()` `Program.cs:3157` | Reuse with adapter | Dua policy baru: jumlah pendaftaran per jam dan percobaan pencocokan data per jam (`KSK-DEC-038`). Jendela saat ini per menit | Rendah, kecuali proxy tidak meneruskan IP asli (`UNK-PO-01`) |
| `CAP-PO-04` | Captcha Cloudflare Turnstile | Platform | Tidak ada `captcha`/`turnstile` di BE dan FE. BE tidak memakai `AddHttpClient`, `IHttpClientFactory`, maupun `new HttpClient(` di mana pun. CSP FE `next.config.js` hanya mengizinkan `frame-src 'self' blob:` dan sumber skrip terbatas | Missing | BE: verifikasi token ke `challenges.cloudflare.com/turnstile/v0/siteverify` lewat HTTP keluar (fasilitas bawaan ASP.NET Core, tanpa paket baru). FE: widget Turnstile serta tambahan `script-src` dan `frame-src` untuk `https://challenges.cloudflare.com`. Kunci di konfigurasi (`KSK-PO-OQ-016`) | Sedang. HTTP keluar pertama di backend; perlu batas waktu dan perilaku saat Cloudflare tidak terjangkau |
| `CAP-PO-26` | Satu pendaftaran terbuka per pasien per tanggal; ditolak bila sudah ada kunjungan RJ aktif; batas 30 hari ke depan | Registration | `BE/appsettings.json:174-186` `Registration:BlockActiveEncounter = true`, `PriorityReservation:MaxDaysAhead = 30`; aturan satu reservasi terbuka di `OutpatientPriorityReservationService#CreateAsync` (`RJ-PRI-DEC-021`) | Reuse with adapter | Bagian konfigurasi baru untuk pendaftaran online dengan pola yang sama | Rendah |
| `CAP-PO-27` | Halaman dan API dapat dibuka dari internet | IT/infrastruktur | `BE/appsettings.json:45-53` `Cors:AllowedOrigins` memuat `https://quilvian-mmchospital.com`, `dev.`, dan `sta.`; `Program.cs:325-342` policy CORS dengan kredensial | Unknown | Sama dengan `RJ-PRI-OQ-003`: apakah domain dan API benar-benar terbuka ke internet, dan apakah proxy meneruskan IP asli | Tinggi untuk go-live, tidak memblokir desain |

### 3.3 Pasien

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-PO-11` | Pencocokan pasien lama dengan NIK atau No. RM ditambah tanggal lahir, tanpa daftar hasil, dan nama tersamar | Registration | `BE/.../Controllers/KioskPatientLookupController.cs#Lookup`: `POST .../kiosk-patient-lookups`, hanya `KioskRead`, mencari lewat No. KTP, No. HP, kartu asuransi, atau member, **tanpa** tanggal lahir, dan mengembalikan kartu pasien tidak tersamar. Perlakuan pasien digabung dan pasien non-aktif ada di layanan itu (`KSK-INV-009`, `KSK-AC-014`, `015`). Penyamar nama ada di `BE/.../Controllers/QueueDisplayRuntimeController.cs:538#MaskPatientName` (`private static`) | Reuse with adapter | Kontrak publik baru: masukan No. RM/NIK + tanggal lahir, keluaran cocok/tidak + nama tersamar. Logika normalisasi, penggabungan, dan status non-aktif dipakai ulang. `MaskPatientName` dipindah ke helper bersama | Sedang. Pesan "tidak cocok" wajib seragam agar tidak membocorkan bagian yang salah |
| `CAP-PO-13` | Master pasien dan No. RM dibuat saat check-in; NIK yang sudah ada memakai No. RM lama | Patient Management | `BE/.../PatientManagement/MasterData/Controllers/PatientController.cs:494-581`: `POST .../patients/kiosk` (`KioskRead`) memvalidasi lalu mengalokasikan kode pasien dan No. RM lewat `_identifierAllocationService.AllocateAsync()`. NIK yang sudah terdaftar dijawab **`409 Conflict`** | Reuse with adapter | Jalur check-in mengubah hasil `409` menjadi "pakai pasien yang ada" tanpa menimpa master (`KSK-DEC-039`). Pembuatan pasien dan kunjungan perlu satu transaksi atau kompensasi | Sedang. Dua langkah tulis (pasien lalu kunjungan) |
| `CAP-PO-14` | Pindai KTP di kiosk | Kiosk | `FE/src/utils/kiosk/registration/kiosk-ktp-scanner-utils.jsx`; `FE/src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-scan-identity.jsx` | Ready to reuse | — | Rendah |

### 3.4 Jadwal, kuota, dan kunjungan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-PO-09` | Pilihan poli, jadwal dokter, kuota, kartu asuransi, faskes perujuk, dan diagnosa untuk halaman tanpa login | Master Data, Registration | Semua memerlukan login: `ClinicController` `GET .../clinics/kiosk/options` (`KioskRead`); `DoctorScheduleController` `GET .../doctor-schedules/kiosk/options` (`KioskRead`); `PatientEncounterController:448` `GET .../patient-encounters/doctor-schedule-quotas` (`PatientEncounter : Read`); `PatientInsuranceController:320` `GET .../patient-insurances/kiosk/options` (`KioskRead`); opsi rujukan kiosk (`FE/src/lib/services/kiosk/registration/kiosk-referral.service.js`) | Extend | Endpoint baca publik yang sempit: hanya field yang dibutuhkan halaman, tanpa data pasien. Kartu asuransi hanya milik pasien yang sudah cocok di `CAP-PO-11`, dengan nomor tersamar (`KSK-PO-FE-002`) | Sedang. Membuka data master ke internet |
| `CAP-PO-10` | Kuota appointment terpakai sejak daftar online dan dilepas saat ditutup | Registration | `BE/.../Services/DoctorScheduleQuotaService.cs:38-80#GetUsageAsync` hanya menghitung **kunjungan** (`IsAppointment`/`IsWalkIn`) pada `QueueDate`; `IsFullForAppointment` `:35-36` | Extend | Hitungan memakai juga pendaftaran online yang masih Menunggu Check-in. Saat check-in, kunjungan baru tidak boleh menghitung dua kali | Tinggi. Layanan ini dipakai kiosk, petugas, dan layar kuota |
| `CAP-PO-15` | Kunjungan RJ dan nomor antrean terbit saat check-in, dengan validasi jadwal | Registration | `BE/.../Controllers/PatientEncounterController.cs:812-830#CreateEncounterForKiosk` → `CreateEncounterCoreAsync` (dipakai bersama petugas): memeriksa `IsAllowAppointment`/`IsAllowWalkIn`, hari dan masa berlaku jadwal, serta kuota (`:2840-2930`). Enum `EncounterRegistrationSource` punya `Appointment = 3` | Reuse with adapter | Kunjungan ditautkan ke pendaftaran online. Sumber pendaftaran memakai nilai yang ada atau nilai baru, diputuskan saat desain. Validasi jadwal ulang otomatis memenuhi `KSK-DEC-046` | Sedang. Jalur bersama kiosk, petugas, dan IGD |
| `CAP-PO-16` | Pendaftaran online ditutup otomatis bila kunjungan RJ dibuat lewat jalur lain pada tanggal yang sama | Registration | `BE/.../Controllers/PatientEncounterController.cs:1194` memanggil `OutpatientPriorityReservationService#CloseForEncounterAsync` (`:1140-1180`) di dalam pembuatan kunjungan | Extend | Tambah penutupan pendaftaran online pada titik yang sama, termasuk pelepasan kuota (`KSK-DEC-045`) | Sedang. Jalur bersama |
| `CAP-PO-17` | Penutupan Tidak Datang di akhir hari, mengejar hari tertinggal, idempoten | Registration | `BE/.../Services/OutpatientPriorityReservationClosureHostedService.cs` (WIB, `Closure:RunAtLocalTime` bawaan `00:05`, mengejar ketertinggalan) dan `OutpatientPriorityReservationService#CloseExpiredAsync` `:1350-1390` | Reuse with adapter | Layanan latar baru dengan pola yang sama, atau perluasan layanan yang ada, diputuskan saat desain | Rendah |
| `CAP-PO-28` | Endpoint check-in hanya untuk akun perangkat kiosk | Kiosk | Policy `AuthorizationPolicies.KioskRead` (`BE/Program.cs:1908`), dipakai `KioskPatientLookupController` dan `CreateEncounterForKiosk` | Ready to reuse | — | Rendah |

### 3.5 Consent, penjamin, dan rujukan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-PO-18` | General consent ditandatangani online, lalu tercatat atas nama pasien saat check-in | Registration | `BE/.../Models/RegPatientGeneralConsent.cs:22-64` mewajibkan `PatientId` **dan** `PatientEncounterId`. Disimpan lewat `PUT .../patient-encounters/{encounterId}/general-consent` (`PatientEncounter : Create`, petugas saja). Menu RJ hanya mewajibkan consent untuk pasien baru: `FE/src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-registration.js:214` (`requiresConsent = patientMode === NEW`). Komponen `outpatient-general-consent-step.jsx` murni (tanpa hook/service) | Extend | Isian consent dan tanda tangan ditampung di pendaftaran online, lalu ditulis ke `RegPatientGeneralConsent` saat check-in, sesudah pasien dan kunjungan ada | Sedang. Gambar tanda tangan base64 menambah ukuran data publik |
| `CAP-PO-19` | Penjamin: memilih kartu asuransi atau penjamin milik pasien, atau mengisi kartu baru | Patient Management | `BE/.../PatientInsuranceController.cs:222,320,470`: daftar, opsi, dan `POST .../patient-insurances/kiosk` (`KioskRead`). FE `outpatient-payer-step.jsx` murni | Reuse with adapter | Kartu milik pasien dibaca lewat kontrak publik tersamar (`CAP-PO-09`). Kartu baru ditampung di pendaftaran online dan dibuat saat check-in, karena pasien baru belum punya master | Sedang |
| `CAP-PO-20` | Data rujukan dan unggah surat rujukan paling besar 5 MB | Registration | `BE/.../Models/RegEncounterReferral.cs:25-100` wajib `PatientEncounterId`; `Services/ReferralDocumentStorageService.cs:35,99` batas bawaan **5 MB** (`FileStorage:MaxReferralDocumentSizeMb = 5`), menerima PDF dan JPG; unggah lewat `POST .../patient-encounters/{encounterId}/referral/documents` (petugas) dan `/kiosk/{encounterId}/...` (`KioskRead`). FE `outpatient-referral-step.jsx` memakai `useSelectResource` dan `fetchInternalReferralLetters` (petugas) | Extend | Data rujukan dan berkas ditampung sebelum kunjungan ada, lalu dipindah ke `RegEncounterReferral` dan dokumennya saat check-in. Batas 5 MB sudah sama dengan `KSK-DEC-035`. Opsi faskes dan diagnosa perlu versi publik | Sedang. Unggah berkas tanpa login |

### 3.6 Kode, pengiriman, dan check-in

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-PO-07` | Kode acak, disimpan sebagai hash, sekali pakai, dan dapat diketik pasien di kiosk | Registration | `BE/.../Services/OutpatientPriorityReservationService.cs:1461-1465`: hash `SHA256` dan kode `RandomNumberGenerator` 256 bit dalam base64url | Reuse with adapter | Teknik hash dipakai ulang. Kode 256 bit (43 karakter) tidak dapat diketik di kiosk, sehingga perlu kode pendek dari huruf/angka yang tidak mirip satu sama lain. Keamanannya ditopang tanggal lahir, hari kunjungan, dan batas laju (`KSK-DEC-033`, `038`) | Sedang. Panjang dan himpunan karakter ditetapkan saat desain |
| `CAP-PO-05` | Kode dikirim otomatis lewat email SMTP | Platform | Tidak ada `SmtpClient`, `MailKit`, `IEmailSender`, atau sejenisnya di BE (`F-PO-8`) | Missing | Pengirim email baru memakai fasilitas bawaan .NET, tanpa paket baru. Kegagalan kirim tidak membatalkan pendaftaran (`KSK-DEC-042`). Akun SMTP dari IT (`KSK-PO-OQ-015`) | Sedang. Kebocoran sandi SMTP bila salah taruh; wajib di konfigurasi rahasia |
| `CAP-PO-06` | Tombol "Kirim kode ke WhatsApp saya" | Frontend | `FE/src/utils/health-services/registration-management/priority-reservation/priority-reservation.utils.js:190-207#buildWhatsAppUrl` (normalisasi `08…` → `62…`, membentuk `https://wa.me/…?text=`) | Ready to reuse | — | Rendah |
| `CAP-PO-24` | Layar Check-in Pasien Online dan Lupa Kode di kiosk | Kiosk | Kartu "Check-in Pasien Online" di `FE/src/components/view/kiosk/kiosk-home-view.jsx` masih memakai notice bawaan; tidak ada route `/kiosk/...check-in` | Missing | Layar baru: isian kode + tanggal lahir, jalur Lupa Kode, pindai KTP untuk pasien baru, lalu cetak tiket. Batas waktu tanpa sentuhan memakai `use-kiosk-inactivity-timeout` yang ada | Rendah |
| `CAP-PO-25` | Cetak tiket antrean sesudah check-in | Kiosk | `FE/src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-ticket.jsx`; `FE/src/utils/kiosk/admission-ticket/thermal-ticket-print.utils.js` | Ready to reuse | — | Rendah |

### 3.7 Frontend halaman publik

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-PO-21` | Route frontend yang terbuka tanpa login, tanpa navbar dan sidebar | Frontend | `/priority-arrival` terdaftar di tiga tempat: `FE/src/proxy.js` (`PUBLIC_PATH_PREFIXES`), `FE/src/components/providers/root-layout-shell.jsx:27`, `FE/src/components/ui/auth-wrapper.jsx:15-27`. Halaman: `FE/src/app/priority-arrival/[code]/page.jsx` dan `layout.jsx` | Reuse with adapter | Route publik baru didaftarkan di ketiga daftar itu | Rendah. Lupa satu daftar menyebabkan redirect ke login |
| `CAP-PO-22` | Langkah menu Pendaftaran Pasien RJ dipakai di halaman publik | Frontend | Komponen di `FE/src/components/view/health-services/registration-management/outpatient-registration/`: `outpatient-visit-type-step`, `outpatient-payer-step`, `outpatient-general-consent-step`, `outpatient-schedule-card`, `outpatient-practice-schedule-modal`, `outpatient-completed-step` murni (0 hook/service). `outpatient-visit-step` memakai `use-outpatient-schedule-quotas` (endpoint petugas). `outpatient-referral-step` memakai service petugas. `outpatient-verification-step` memakai satu hook. Halaman dan hook `use-outpatient-registration.js` terhubung ke Redux dan endpoint petugas. Urutan langkah dari `outpatient-registration.constants.js:57-155` | Reuse with adapter | Hook dan service publik baru yang memanggil endpoint publik (`CAP-PO-09`), lalu memberi data ke komponen murni yang sama. Langkah yang terikat endpoint petugas perlu varian props, bukan salinan komponen | Sedang. Perubahan pada komponen bersama berisiko ke menu petugas |
| `CAP-PO-23` | Langkah pengenalan pasien publik sebagai pengganti Cari / Input Pasien | Frontend | Menu RJ memakai `PatientSelectionStep` IGD (`FE/src/components/view/health-services/registration-management/emergency-registration/patient-selection-step.jsx`), yang menampilkan daftar hasil pencarian | Missing | Langkah baru: pilihan pasien lama (No. RM/NIK + tanggal lahir) atau pasien baru (formulir data diri), ditambah kolom No. WA/email (`KSK-DEC-031`, `043`). Formulir data diri pasien baru dapat meniru isian kiosk Pasien Baru | Rendah |
| `CAP-PO-29` | Widget captcha dan CSP | Frontend | `FE/next.config.js` (`script-src`, `connect-src`, `frame-src 'self' blob:`) | Extend | Tambahkan `https://challenges.cloudflare.com` pada `script-src` dan `frame-src`, hanya bila Turnstile dipakai | Rendah. Salah konfigurasi membuat widget kosong |

## 4. Kontrak as-is yang menjadi titik sentuh

Hanya endpoint yang ada pada SHA di atas. Endpoint baru belum ada dan tidak ditulis di sini.

### Health Services / Registration Management / Priority Reservation Arrival

| Method | Path | Akses | Kegunaan bagi Amendment PO |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/priority-reservation-arrivals/{code}` | Tanpa login, batas laju per IP | Pola acuan endpoint publik: `404` seragam untuk kode salah, `429` saat batas laju terlampaui |
| `POST` | `/api/v1/health-services/registration-management/priority-reservation-arrivals/{code}/confirm` | Tanpa login, batas laju per IP | Pola acuan aksi publik sekali pakai |

### Health Services / Registration Management / Kiosk Patient Lookup

| Method | Path | Akses | Kegunaan bagi Amendment PO |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/kiosk-patient-lookups` | `KioskRead`, batas laju per perangkat | Logika pencocokan dipakai ulang. Kontraknya tidak cocok untuk publik karena tanpa tanggal lahir dan mengembalikan kartu pasien utuh |

### Health Services / Registration Management / Patient Encounter

| Method | Path | Akses | Kegunaan bagi Amendment PO |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/kiosk` | `KioskRead` | Jalur pembuatan kunjungan saat check-in |
| `GET` | `/api/v1/health-services/registration-management/patient-encounters/doctor-schedule-quotas` | `PatientEncounter : Read` | Sisa kuota. Belum dapat dibuka tanpa login |
| `PUT` | `/api/v1/health-services/registration-management/patient-encounters/{encounterId}/general-consent` | `PatientEncounter : Create` | Penyimpanan consent. Butuh kunjungan |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/kiosk/{encounterId}/referral/documents` | `KioskRead` | Unggah surat rujukan. Butuh kunjungan |

### Health Services / Patient Management / Master Data / Patient

| Method | Path | Akses | Kegunaan bagi Amendment PO |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/patient-management/master-data/patients/kiosk` | `KioskRead` | Pembuatan master pasien dan No. RM saat check-in. NIK yang sudah ada dijawab `409` |

### Health Services / Patient Management / Master Data / Patient Insurance

| Method | Path | Akses | Kegunaan bagi Amendment PO |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/patient-management/master-data/patient-insurances/kiosk/options` | `KioskRead` | Kartu asuransi pasien. Belum dapat dibuka tanpa login |
| `POST` | `/api/v1/health-services/patient-management/master-data/patient-insurances/kiosk` | `KioskRead` | Pembuatan kartu baru saat check-in |

### Health Services / Master Data / Clinic dan Health Services / Master Data / Doctor Schedule

| Method | Path | Akses | Kegunaan bagi Amendment PO |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/master-data/clinics/kiosk/options` | `KioskRead` | Pilihan poli. Belum dapat dibuka tanpa login |
| `GET` | `/api/v1/health-services/master-data/doctor-schedules/kiosk/options` | `KioskRead` | Pilihan jadwal dokter. Belum dapat dibuka tanpa login |

Arti kode status bagi pasien: `400` isian tidak sah; `404` kode atau data tidak berlaku (pesannya seragam); `409` data bentrok, misalnya NIK sudah terdaftar; `429` terlalu banyak percobaan, coba lagi nanti.

## 5. Unknown dan koreksi fakta

| ID | Jenis | Isi | Owner | Memblokir |
| --- | --- | --- | --- | --- |
| `UNK-PO-01` | Unknown | Apakah domain halaman dan API terbuka ke internet, dan apakah proxy meneruskan IP asli. Tanpa IP asli, batas laju per IP menghitung semua pasien sebagai satu IP (`CAP-PO-03`, `CAP-PO-27`). Sama dengan `RJ-PRI-OQ-003` dan `KSK-PO-OQ-010` | IT/infrastruktur | `IMPLEMENTATION` (go-live) |
| `COR-PO-01` | Koreksi fakta | `F-PO-5` pada decision log menyebut consent disimpan per pasien. Source menunjukkan consent wajib terikat ke pasien **dan** kunjungan (`PatientEncounterId`). Keputusan `KSK-DEC-034` tetap berlaku: consent ditampung lalu ditulis saat check-in, ketika keduanya sudah ada | — | Tidak |
| `COR-PO-02` | Koreksi fakta | `F-PO-7` menyebut perubahan beranda kiosk belum di-commit. Perubahan itu kini ada di commit FE `5f18e9f04` | — | Tidak |

Tidak ada `Conflict`.

## 6. Pertanyaan penutup untuk desain

Pertanyaan di bawah tidak memerlukan keputusan bisnis baru. Semuanya dijawab di tahap desain dengan bukti di atas.

1. Kuota (`CAP-PO-10`): hitungan pendaftaran online digabung ke `DoctorScheduleQuotaService` atau dihitung terpisah lalu dijumlah? Bagaimana memastikan konversi saat check-in tidak menghitung dua kali?
2. Jalur bersama (`CAP-PO-15`, `CAP-PO-16`): penautan kunjungan ke pendaftaran online dan penutupan otomatis dipasang di `CreateEncounterCoreAsync` tanpa mengubah perilaku jalur petugas, kiosk, dan IGD.
3. Kode (`CAP-PO-07`): panjang, himpunan karakter, dan cara menyimpan hash untuk kode pendek.
4. Penampungan (`CAP-PO-18`, `CAP-PO-19`, `CAP-PO-20`): bentuk penyimpanan consent, kartu asuransi baru, data rujukan, dan berkas sebelum kunjungan ada, serta pemindahannya dalam satu transaksi saat check-in.
5. Endpoint publik (`CAP-PO-09`): field minimum yang dibuka untuk poli, jadwal, kuota, kartu asuransi tersamar, faskes, dan diagnosa.
6. Turnstile (`CAP-PO-04`): perilaku bila Cloudflare tidak terjangkau saat verifikasi, yaitu menolak (fail-closed) atau meloloskan.

## 7. Pemicu impact scan berikutnya

Ulangi impact scan terbatas bila salah satu berubah sebelum desain disetujui:

- `PatientEncounterController#CreateEncounterCoreAsync`, `DoctorScheduleQuotaService`, atau `OutpatientPriorityReservationService`.
- `PatientController#Create` (`/kiosk`) atau alokasi No. RM.
- `RegPatientGeneralConsent`, `RegEncounterReferral`, atau `ReferralDocumentStorageService`.
- Daftar route publik di `proxy.js`, `root-layout-shell.jsx`, atau `auth-wrapper.jsx`, serta CSP di `next.config.js`.
- HEAD backend berganti dari `5e777833` atau HEAD frontend dari `5f18e9f04` pada berkas di atas.

## 8. Addendum 19 Nov 2026 — layar petugas dan keputusan `KSK-DEC-047` s.d. `053`

**Alasan addendum.** Closure pass singkat sesudah penilaian kelengkapan `KSK-RCG-002` menambah satu butir scope: layar petugas Pendaftaran Online (`KSK-DEC-050` s.d. `052`). Closure pass itu juga menetapkan tiga aturan baru: tanggal H+1 s.d. H+30 (`KSK-DEC-047`), NIK hasil pindai wajib sama (`KSK-DEC-048`), dan check-in hanya dengan KTP atau KIA (`KSK-DEC-053`). Audit awal di atas belum mencakup hal-hal ini.

**SHA yang diaudit:** sama dengan audit awal, backend `5e777833` dan frontend `5f18e9f04`. Tidak ada source yang diubah.

**Preseden utama:** layar admisi Reservasi Prioritas, yang sudah punya daftar, ringkasan per status, ganti tanggal, pembatalan dengan alasan, riwayat, dan hak akses sendiri.

### 8.1 Peta kemampuan tambahan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-PO-30` | Hak akses layar petugas dengan aksi Lihat, Ubah, dan Batalkan (`KSK-PO-AS-002`) | Platform | Resource lahir dari atribut controller: `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]` (`BE/.../Controllers/OutpatientPriorityReservationController.cs:31-39`, `:66`, `:301`). Seeder mendaftarkannya saat aplikasi dinyalakan (`BE/Program.cs:2833` `AccessMenuSeeder`, aturan di `BE/Services/Security/PermissionRegistryDescriptor.cs:12-36`). Frontend memeriksa izin lewat `usePermission` (`FE/src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-detail.jsx:63-68`) dan menu memakai `requiredPermission` (`FE/src/utils/menu-sidebar/menu-items.jsx:3558-3568`) | Ready to reuse | Resource baru cukup dideklarasikan di controller baru. Registry izin tidak memerlukan migration | Rendah |
| `CAP-PO-31` | Daftar per tanggal kunjungan dan status, ringkasan per status, serta pencarian kode, NIK/No. RM, atau nama (`KSK-DEC-050`) | Registration | `OutpatientPriorityReservationService#GetPagedAsync` (`BE/.../Services/OutpatientPriorityReservationService.cs:180-260`): rentang `VisitDateFrom`/`VisitDateTo`, `Status`, `Search` minimal 3 karakter, pencocokan sebagian (`ILike`) pada nomor reservasi, nama, dan No. RM, 25 baris per halaman. Ringkasan lewat `GET .../summary`. NIK pasien tersimpan sebagai teks biasa di `MstPatient.IdentityNumber` (`BE/.../PatientManagement/MasterData/Models/MstPatient.cs:55`) | Reuse with adapter | Tiga penyesuaian. Pertama, pencarian NIK belum ada di pola RP. Kedua, nama dan NIK pasien baru ada di data pendaftaran online, bukan di master. Ketiga, kode hanya tersimpan sebagai hash (`CAP-PO-07`), sehingga pencarian kode harus mencocokkan hash secara persis, tidak bisa sebagian | Rendah |
| `CAP-PO-32` | Ubah tanggal, jadwal dokter, dan No. WA/email, dengan pertukaran kuota dan riwayat nilai lama/baru (`KSK-DEC-051`, `KSK-PO-AS-003`) | Registration | `OutpatientPriorityReservationService#RescheduleAsync` (`:748-800`) hanya mengganti **tanggal**, hanya sebelum hari kunjungan, dan **menerbitkan kode baru** (`RJ-PRI-DEC-034`, `036`). Riwayat RP hanya menyimpan `FromVisitDate`/`ToVisitDate` (`BE/.../Models/RegPriorityReservationHistory.cs:54-57`). Ganti tanggal kunjungan yang sudah ada juga tersedia di `OutpatientEncounterScheduleService` (`:250-265`), dengan kunci per pasien | Extend | Perilaku baru: ganti jadwal dokter dan kontak, lepas kuota lama lalu pakai kuota baru dalam satu transaksi, kode **tetap** (berbeda dari RP), dan riwayat menyimpan nilai lama/baru untuk tanggal, jadwal, dan kontak | Sedang |
| `CAP-PO-33` | Pembatalan oleh petugas dengan alasan wajib, menjadi status Dibatalkan (`KSK-DEC-052`) | Registration | `POST .../outpatient-priority-reservations/{id}/cancel` (`OutpatientPriorityReservationController.cs:295-310`, `[AccessPermission("OutpatientPriorityReservation", "Cancel")]`, body `PriorityReservationReasonRequest`) dan `CancelAsync` (`:888`). Token konkurensi `RowVersion` (`RegPriorityReservation.cs:91`) dikirim frontend sebagai `expectedRowVersion` | Reuse with adapter | Pola dipakai untuk wadah baru (`CAP-PO-01`), ditambah pelepasan kuota | Rendah |
| `CAP-PO-34` | Kuota tidak terpakai ganda saat dua permintaan berebut slot terakhir (AC 3, AC 19) | Registration | Pemeriksaan kuota di `PatientEncounterController#ValidateDoctorScheduleQuotaAsync` (`:2900-2925`) membaca `GetUsageAsync` **tanpa kunci per jadwal**. Kunci yang ada hanya per pasien: `REG_ENCOUNTER_ACTIVE_{PatientId}` (`:1093`), kunci pasien RP (`OutpatientPriorityReservationService.cs:1577`), dan ganti jadwal kunjungan (`OutpatientEncounterScheduleService.cs:259`). Pola `pg_advisory_xact_lock` sudah dipakai di tujuh tempat | Extend | **Fakta:** dua pasien berbeda dapat lolos pemeriksaan kuota yang sama secara bersamaan. **Inferensi:** risiko kelebihan kuota naik karena Amendment PO menambah penulis kuota baru, yaitu halaman publik, edit petugas, dan check-in. Kunci per jadwal dan tanggal dibutuhkan pada jalur pendaftaran online. Pemasangannya di jalur petugas, kiosk, dan IGD diputuskan saat desain | Sedang. Menyentuh jalur bersama |
| `CAP-PO-35` | Layar frontend petugas: daftar, detail, dan modal aksi (`KSK-PO-FE-005`) | Frontend | Route `FE/src/app/health-services/registration-management/priority-reservations/page.jsx` dan `[id]/page.jsx`. View `FE/src/components/view/health-services/registration-management/priority-reservations/` (list client/view, kolom tabel, `detail/priority-reservation-detail-view.jsx`, `detail/priority-reservation-action-modals.jsx`). Base component yang dipakai: `Hero`, `SummaryCards`, `DataFilter`, `FilterDatePicker`, `FilterSelect`, `DataTable`, `Pagination`, `AccessDeniedGate`, `ToastStack`, dan `InformationAlert`. State lewat Redux slice `FE/src/lib/state/slice/health-services/registration-management/priority-reservation-slice`, service `FE/src/lib/services/health-services/registration-management/priority-reservation.service.js` (`InstanceAxios`) | Reuse with adapter | Pola dan base component dipakai ulang dengan slice, service, hook, dan kolom baru. Letak menu tetap keputusan desain (`KSK-PO-FE-005`) | Rendah |
| `CAP-PO-36` | Kunjungan hasil check-in tercatat sebagai appointment pada tanggal hari itu (`KSK-DEC-047`) | Registration | `CreateEncounterCoreAsync` hanya menolak tanggal sebelum tanggal operasional (`PatientEncounterController.cs:2280-2283`). Untuk appointment, jalur itu memeriksa `IsAllowAppointment` (`:2848`) dan kuota appointment (`:2914`), tanpa melarang tanggal hari ini. Kiosk Pasien Baru saat ini mengirim `isAppointment: false` (`FE/src/lib/helpers/kiosk/registration/kiosk-new-patient-submit.helpers.jsx:691`). Aturan H+1 di frontend dapat memakai `isAppointmentDate` (`FE/src/utils/health-services/registration-management/outpatient-registration/outpatient-registration.utils.js:241-244`) | Ready to reuse | Check-in mengirim `isAppointment: true`. Aturan H+1 s.d. H+30 tetap divalidasi backend pendaftaran online | Rendah |
| `CAP-PO-37` | NIK yang dibandingkan saat check-in pasien baru benar-benar berasal dari kartu KTP/KIA (`KSK-DEC-048`, `053`) | Kiosk | OCR dijalankan agen pemindai Plustek lokal di mesin kiosk (`FE/src/lib/plustek-scanner-agent/plustek-scanner-agent.js:5` `http://127.0.0.1:9100`, `getOcrPayload` `:169`). Backend tidak pernah menerima hasil OCR mentah. Langkah pindai menampilkan **semua** isian OCR sebagai kolom yang dapat diubah pasien, termasuk NIK (`FE/src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-scan-identity.jsx:275-315`; daftar isian dari `getScanOcrEditFieldsByType`, `kiosk-new-patient-registration.helpers.jsx:2601`; KIA memuat "NIK Anak" `:2535`) | Reuse with adapter | Layar check-in memakai varian langkah pindai yang mengunci NIK pada hasil OCR. Bila NIK tidak terbaca, pasien memindai ulang atau ke loket. Pilihan SIM disembunyikan. Backend membandingkan NIK kiriman kiosk dengan NIK online | Sedang. Kepercayaan bahwa NIK berasal dari kartu bertumpu pada perangkat kiosk yang sudah login (`KioskRead`), bukan pada pemeriksaan backend |

### 8.2 Kontrak as-is tambahan

#### Health Services / Registration Management / Outpatient Priority Reservation (pola acuan)

| Method | Path | Akses | Kegunaan bagi Amendment PO |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/outpatient-priority-reservations` | `OutpatientPriorityReservation : Read` | Pola daftar dengan rentang tanggal, status, pencarian, dan halaman |
| `GET` | `/api/v1/health-services/registration-management/outpatient-priority-reservations/summary` | `OutpatientPriorityReservation : Read` | Pola ringkasan jumlah per status |
| `GET` | `/api/v1/health-services/registration-management/outpatient-priority-reservations/{id}` | `OutpatientPriorityReservation : Read` | Pola detail beserta riwayat |
| `POST` | `/api/v1/health-services/registration-management/outpatient-priority-reservations/{id}/reschedule` | `OutpatientPriorityReservation : Create` | Pola ganti tanggal. Berbeda dari Amendment PO karena menerbitkan kode baru |
| `POST` | `/api/v1/health-services/registration-management/outpatient-priority-reservations/{id}/cancel` | `OutpatientPriorityReservation : Cancel` | Pola pembatalan dengan alasan wajib |

Arti kode status bagi petugas: `400` isian tidak sah, misalnya alasan kosong; `404` pendaftaran tidak ditemukan; `409` status sudah berubah atau data diubah petugas lain (`RowVersion`); `403` tidak punya hak akses.

### 8.3 Pertanyaan penutup tambahan untuk desain

Pertanyaan di bawah tidak memerlukan keputusan bisnis baru.

7. Kunci kuota (`CAP-PO-34`): kunci per jadwal dan tanggal dipasang di pendaftaran online, edit petugas, dan check-in. Perlu diputuskan apakah jalur petugas, kiosk, dan IGD ikut dikunci, dengan menimbang risiko regresi jalur bersama.
8. Pencarian (`CAP-PO-31`): pencarian kode memakai hash yang cocok persis. Letak NIK dan nama pasien baru di wadah pendaftaran online harus dapat dicari.
9. Riwayat (`CAP-PO-32`): bentuk penyimpanan nilai lama/baru untuk tanggal, jadwal dokter, dan kontak.
10. Layar check-in pasien baru (`CAP-PO-37`): varian langkah pindai dengan NIK terkunci, tanpa mengubah perilaku kiosk Pasien Baru biasa.

### 8.4 Pemicu impact scan berikutnya (tambahan)

- `OutpatientPriorityReservationController`, `OutpatientPriorityReservationService#GetPagedAsync`, `RescheduleAsync`, atau `CancelAsync`.
- `PermissionRegistryDescriptor` atau `AccessMenuSeeder`.
- `ValidateDoctorScheduleQuotaAsync` atau penambahan kunci per jadwal di jalur bersama.
- `kiosk-new-patient-step-scan-identity.jsx`, `getScanOcrEditFieldsByType`, atau agen pemindai Plustek.

Tidak ada `Unknown` maupun `Conflict` baru.
