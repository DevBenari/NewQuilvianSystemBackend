# Laporan Perubahan Frontend — `FE-KSK-PO-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-005` |
| Judul | Layar Check-in: kode/Lupa Kode, ringkasan, pasien lama, tiket |
| Slice | `EPIC KSK-PO-02` — check-in di kiosk (`FR-KSK-PO-050`), rilis `MVP-2` |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-005` |
| Trace | `KSK-DEC-033`, `036`, `040`, `046`; desain `FE-KSK-PO-02` dan `FE-KSK-01` di [03-frontend-architecture.md](../../../03-frontend-architecture.md) |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`), grup *Online Registration Check-In* `POST /kiosk/check-in/verify`, `POST patient-encounters/kiosk` (diperluas); validation matrix `KSK-VAL-PO-040` s.d. `048`, `053` s.d. `055` |
| Wewenang UI | Route `/kiosk/registration/online-check-in` dan kartu beranda dari desain `FE-KSK-PO-02`/`FE-KSK-01`. Susunan langkah (Verifikasi, Ringkasan, Cetak Antrean) dan letak tombol `DEV_DISCRETION`, mengikuti Pendaftaran Pasien Lama kiosk |
| Dependency | `BE-KSK-PO-007` ✅ 2026-11-25 (dimulai saat masih 🟡 atas persetujuan pemilik 2026-11-23) |
| Klasifikasi | `HIGH` — layar publik di kiosk yang membuat kunjungan dan nomor antrean |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: route, view, hook, service, konstanta, util, style, kartu beranda kiosk; laporan dan baris status di blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `7bba5cd1c` (`sukmagpV2`) + rework `FE-KSK-PO-002` (format No. RM) yang belum di-commit |
| Commit backend yang dijadikan rujukan | `356bf463` (`sukmagp`); kode check-in di `a30d9f68` |
| Tanggal | 2026-11-25 |
| Status | ✅ Selesai — seluruh layar, jalur gagal, dan jalur sukses nyata (kode dan Lupa Kode sampai tiket) terbukti di browser pada 2026-11-25 |

## 1. Keadaan yang ditemukan di awal

- Kartu "Check-in Pasien Online" di beranda kiosk sudah ada dengan `enabledType: "appointment"`, tetapi belum punya route. Saat disentuh, kartu menampilkan notice "Fitur akan dilanjutkan pada tahap berikutnya.".
- Backend sudah menyediakan verifikasi (`BE-KSK-PO-006`) dan check-in menjadi kunjungan (`BE-KSK-PO-007`). Keduanya 🟡 karena jalur `200` hanya bisa dijalankan pada hari kunjungan data uji.
- Layar Pendaftaran Pasien Lama kiosk menampilkan tiket di layar, tetapi tidak mencetak tiket antrean poli. Cetak thermal baru dipakai oleh menu Cetak Antrean (antrean admisi).

## 2. Proses bisnis dari sisi pengguna

1. Pasien menyentuh kartu **Check-in Pasien Online** di beranda kiosk. Kartu hanya aktif pada perangkat yang mengizinkan appointment.
2. Pada langkah **Verifikasi**, pasien memilih salah satu cara:
   - **Saya punya kode**: isi kode dari WhatsApp atau email, misalnya `K7QM-4TRZ`, lalu tanggal lahir pasien.
   - **Lupa Kode**: pilih NIK atau No. Rekam Medis, isi nomornya, lalu tanggal lahir.
3. Pasien menekan **Periksa**. Contoh hasil:
   - Kode atau tanggal lahir salah: "Data tidak sesuai atau pendaftaran sudah tidak berlaku. Periksa kembali atau hubungi petugas."
   - Datang sehari lebih awal: "Check-in hanya dapat dilakukan pada 26 Nov 2026."
   - Cocok pada hari kunjungan: layar pindah ke **Ringkasan**.
4. Ringkasan menampilkan nama tersamar (misalnya `Si** Ra****`), nomor pendaftaran, tanggal dan jam praktik, poli, dokter, jenis pasien, penjamin, serta faskes perujuk dan jumlah surat rujukan. NIK, No. RM, kode, dan nomor kartu tidak tampil.
5. Pasien menekan **Check-in Sekarang**. Bila pendaftaran membawa kartu asuransi baru, kiosk lebih dulu memakai kartu yang sama bila sudah tercatat pada pasien, atau membuatnya. Sesudah itu kiosk membuat kunjungan dengan `onlineRegistrationId` dan token check-in.
6. Langkah **Cetak Antrean** menampilkan nomor antrean di layar dan mencetak tiket thermal 80 mm. Bila tiket tidak keluar, pasien menekan **Cetak Ulang** di header, atau mengikuti arahan "ambil tiket di menu Cetak Antrean".
7. Layar kembali ke beranda sesudah 10 detik, saat pasien menekan Selesai, atau saat 120 detik tanpa sentuhan. Semua isian dan paket check-in dibuang.

Keadaan khusus:

- **Jadwal berubah atau kuota penuh pada hari H** (`409` `KSK-VAL-PO-053`/`054`), atau **pendaftaran berubah bersamaan** (`055`): pesan backend tampil, tombol Check-in Sekarang nonaktif, dan footer mengarahkan ke loket pendaftaran.
- **Token habis** (`KSK-VAL-PO-046`), di layar sebelum dikirim maupun ditolak backend: layar kembali ke Verifikasi dengan pesan "Waktu check-in habis. Silakan ulangi dari awal.".
- **Pasien baru** (`patientMode` 2): ringkasan tampil, tetapi Check-in Sekarang nonaktif dengan catatan bahwa pasien baru perlu pindai KTP/KIA. Langkah pindai dikerjakan `FE-KSK-PO-006`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`kiosk-home-view.jsx`; seluruh `old-patient/**` kiosk (view, step ticket, step type, step review); `use-kiosk-old-patient-registration.jsx`; `kiosk-old-patient-registration.service.js` (`createPatientInsuranceFromKiosk`, `fetchPatientPaymentProfile`); `use-kiosk-admission-ticket.jsx`, `kiosk-admission-ticket-view.jsx`, `kiosk-admission-ticket.module.css`, `thermal-ticket-print.utils.js`; `use-kiosk-inactivity-timeout.jsx`; `login-slice.jsx` (konteks perangkat kiosk); `FilterSelect`, `FilterDatePicker`. Backend: `OnlineRegistrationCheckInDtos.cs`, `OnlineRegistrationCheckInService.Encounter.cs` (aturan `048`, `ReferralMatches`, `NormalizeRequest`), `PatientEncounterDtos.cs`, `PatientInsuranceController` (`POST /kiosk`, pindai kartu opsional).

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/app/kiosk/registration/online-check-in/page.jsx` | Baru | Route kiosk |
| `src/components/view/kiosk/registration/online-check-in/kiosk-online-check-in-view.jsx` | Baru | Kerangka layar, bar langkah, pesan, footer aksi, tiket layar, tiket thermal, peringatan batas waktu |
| `.../online-check-in/kiosk-online-check-in-step-verify.jsx` | Baru | Pilihan cara, isian kode atau NIK/No. RM, tanggal lahir |
| `.../online-check-in/kiosk-online-check-in-step-summary.jsx` | Baru | Ringkasan paket check-in |
| `.../online-check-in/kiosk-online-check-in-thermal-ticket.jsx` | Baru | Tiket antrean poli 80 mm untuk dicetak |
| `src/lib/hooks/kiosk/registration/use-kiosk-online-check-in.js` | Baru | Alur, akses perangkat, penjaga ketukan ganda, token, cetak, pembersihan sesi |
| `src/lib/services/kiosk/registration/kiosk-online-check-in.service.js` | Baru | Verifikasi, kunjungan, kartu asuransi baru (`InstanceAxios`) |
| `src/lib/constants/kiosk/registration/kiosk-online-check-in.constants.js` | Baru | Route, endpoint, enum cermin, langkah, header, pesan |
| `src/utils/kiosk/registration/kiosk-online-check-in.utils.js` | Baru | Format dan validasi kode/identitas/tanggal lahir, payload verifikasi dan kunjungan, label ringkasan, nomor antrean, pencocokan kartu |
| `src/style/kiosk/registration/kiosk-online-check-in.module.css` | Baru | Tata letak isian, tombol header, tiket thermal tersembunyi di layar, aturan cetak; token saja |
| `src/components/view/kiosk/kiosk-home-view.jsx` | Diubah | `KIOSK_ROUTES.onlineCheckIn` dan `route` pada kartu Check-in Pasien Online |

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Penerapan |
| --- | --- |
| `page.jsx` tipis | Hanya merender view |
| View tidak memanggil HTTP | View → hook → service |
| HTTP | `InstanceAxios`, supaya `use-kiosk-inactivity-timeout` menahan hitungan selama request berjalan. Kartu asuransi memakai fungsi service pasien lama yang sudah dipakai kiosk |
| Akses | Akun perangkat kiosk (`isKioskUser`) dan `kioskCanAppointment` dari konteks perangkat; tanpa peran hardcode. Backend tetap menegakkan `KioskRead` |
| Privasi | Nama tersamar dari backend; NIK pembanding pasien baru, kode, token, dan identitas tidak tampil maupun tercetak ke console. Tiket thermal tanpa nama dan No. RM |
| Pesan | Pesan `400`/`404`/`409`/`429` dari backend tampil apa adanya (sudah Bahasa Indonesia sesuai validation matrix); gagal jaringan memakai pesan kiosk sendiri |

### 3.4 Gerbang keputusan base component

| Elemen | Kandidat | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Kerangka layar (header, bar langkah, kartu isi, footer aksi) | Kelas `kiosk-old-patient-view.module.css` | Pendaftaran Pasien Lama kiosk | REUSE | Kelas yang sama |
| Pilihan cara check-in | Kartu Jenis Pasien kiosk (`oldPatientTypeChoiceCard`) | `kiosk-old-patient-step-type.jsx` | REUSE | Dua kartu |
| Isian kode dan nomor identitas | Isian kiosk `.fieldGroup input` | Step pasien lama | REUSE | — |
| Jenis identitas | `FilterSelect` | Base component | REUSE | — |
| Tanggal lahir | `FilterDatePicker` | Base component, dipakai langkah rujukan kiosk | REUSE | — |
| Ringkasan | Kelas Review Data Pasien kiosk | `kiosk-old-patient-step-review.jsx` | REUSE | Profil + bagian detail |
| Tiket layar | `KioskOldPatientStepTicket` | Langkah tiket pasien lama | REUSE | Tanpa diubah; nama diisi nama tersamar |
| Tiket thermal | Kelas tiket `kiosk-admission-ticket.module.css` + `printThermalTicket` + `ADMISSION_TICKET_PAPER` | Cetak Antrean | COMPOSE | Lihat pilihan di bawah |
| Peringatan batas waktu | `KioskInactivityWarning` + `use-kiosk-inactivity-timeout` | Seluruh layar kiosk | REUSE | — |

Pilihan untuk tiket thermal (satu-satunya elemen bukan `REUSE`):

1. **Rangkai komponen tiket kecil di layar ini dengan kelas dan util cetak Cetak Antrean (direkomendasikan, dipilih).** Hasil cetak seragam dengan tiket admisi dan Cetak Antrean tidak tersentuh. Biaya kecil: satu komponen 50 baris.
2. Pindahkan `ThermalTicket` Cetak Antrean ke komponen bersama. Tiket memang dipakai dua layar, tetapi refactor menyentuh layar Cetak Antrean yang sudah lolos UAT, jadi risiko regresinya lebih besar.
3. Cetak tiket layar apa adanya dengan `window.print()`. Tidak perlu komponen baru, tetapi tiket layar dirancang untuk monitor, sehingga hasil di kertas 80 mm terpotong.

`UI GATE: PASS` — tidak ada `NEW` maupun `EXTEND` pada base component.

## 4. State yang ditangani di layar

| Keadaan | Penanganan |
| --- | --- |
| Memeriksa sesi | "Memuat halaman check-in pasien online..." |
| Bukan akun kiosk / belum login | Dialihkan (pola layar kiosk lain) |
| Perangkat tanpa appointment | Pesan "Layanan ini belum aktif pada perangkat kiosk ini.", tanpa isian |
| Validasi layar | Kode bukan 8 karakter dari himpunan kode (`040`), NIK bukan 16 digit, No. RM bukan angka, tanggal lahir kosong atau di masa depan; tanpa request |
| Memeriksa / memproses | Tombol "Memeriksa..." / "Memproses check-in..." nonaktif; ketukan ganda diabaikan (`045`) |
| `404`, `409` (belum hari H), `429` | Pesan backend; tetap di Verifikasi |
| `409` jadwal/kuota/tabrakan, `404` saat check-in | Pesan backend; Check-in Sekarang nonaktif; arahan ke loket |
| Token habis (`046`) | Kembali ke Verifikasi dengan pesan |
| Gagal jaringan | "Koneksi ke server terputus. Periksa jaringan kiosk lalu coba lagi." |
| Kartu asuransi baru gagal disiapkan | Pesan backend kartu, atau "Kartu asuransi dari pendaftaran online gagal disiapkan. Silakan ke loket pendaftaran." |
| Printer tidak mengeluarkan tiket | Tiket tetap di layar; Cetak Ulang di header; arahan menu Cetak Antrean |
| Tanpa sentuhan 120 detik | Peringatan 5 detik, lalu beranda dan sesi dibuang |

## 5. Endpoint yang dikonsumsi

| Endpoint | Kegunaan | Bentuk |
| --- | --- | --- |
| `POST /api/v1/health-services/registration-management/online-registrations/kiosk/check-in/verify` | Verifikasi kode atau identitas + tanggal lahir | Body `{ mode: 1, code, birthDate }` atau `{ mode: 2, identityType, identityValue, birthDate }`; `200` → `OnlineRegistrationCheckInPackage` |
| `GET /api/v1/health-services/patient-management/master-data/patient-insurances/options?patientId=` | Mencari kartu asuransi baru yang sudah tercatat (coba ulang tidak menggandakan) | Fungsi kiosk yang ada `fetchPatientPaymentProfile` |
| `POST /api/v1/health-services/patient-management/master-data/patient-insurances` | Membuat kartu asuransi baru dari pendaftaran online bila belum ada | Fungsi kiosk yang ada `createPatientInsuranceFromKiosk`; aksi backend sama dengan `/kiosk`, policy `KioskRead`, pindai kartu opsional |
| `POST /api/v1/health-services/registration-management/patient-encounters/kiosk` | Membuat kunjungan check-in | `PatientEncounterCreateRequest` dari paket: `patientId`, `serviceUnitId`, `clinicId`, `roomId`, `doctorId`, `doctorScheduleId`, `visitDate`, `encounterType` 1, `visitType` 1, `paymentType`, `patientInsuranceId`, `patientCompanyGuarantorId`, `isReferral` + `referralInstitutionId`, `isAppointment: true`, `isWalkIn: false`, `isFromKiosk: true`, `onlineRegistrationId`, `onlineRegistrationCheckInToken` |

Delta terhadap desain: desain menyebut `POST patient-insurances/kiosk` untuk kartu baru. Layar ini memakai fungsi kiosk pasien lama yang memanggil `POST patient-insurances`. Keduanya adalah aksi backend yang sama (`[HttpPost]` dan `[HttpPost("kiosk")]` pada satu method, policy `KioskRead`), jadi perilaku tidak berbeda.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint berkas task | 0 error; 1 warning lama `<img>` di `kiosk-home-view.jsx` | `PASS` | Keluaran eslint |
| `npm run lint:errors` | Exit 0 (`eslint . --quiet`) | `PASS` | `scratchpad/fe-po005/lint-errors.log` |
| `npm run build` | Exit 0; 856/856 halaman, `○ /kiosk/registration/online-check-in` | `PASS` | `scratchpad/fe-po005/build.log` |
| Grep anti-regresi | Tanpa warna literal, tanpa `console`; satu `style` inline berisi variabel ukuran kertas (pola Cetak Antrean) | `PASS` | Grep |
| Uji browser 2026-11-25, dev server 3000, request 7184 dialihkan ke backend uji 7185, login SuperAdmin + konteks perangkat kiosk pada respons `/auth/me` | 26/26 PASS (rincian di bawah) | `PASS` | `scratchpad/fe-po005/browser-today.cjs`, `browser-today.log`, tangkapan layar `k-*.png` |
| Jalur sukses nyata 2026-11-25 (Data uji `PO-20261125-0001` s.d. `0009` semula bertanggal 2026-11-26; dengan izin pemilik (2026-11-25, AskUserQuestion) tanggal kunjungannya dipindah ke 2026-11-25 lewat satu `UPDATE` 9 baris pada `RegOnlineRegistration` (tanggal, jadwal padanan yang dibuat lewat API, `RowVersion` baru) supaya token hari H dapat terbit hari itu (`po025_shift.py`)) | 10/10 PASS: S1 kode `PO-20261125-0006` → ringkasan (nomor, 25 November 2026, nama tersamar) → Check-in Sekarang → tiket antrean `P005` di layar dan tiket thermal sama, cetak otomatis 1×, payload memakai `onlineRegistrationId` + token 43 karakter, pendaftaran status 2 dengan nomor kunjungan, Selesai → beranda; S2 Lupa Kode NIK `PO-20261125-0007` → hal yang sama, antrean `P010` | `PASS` | `scratchpad/fe-po005/browser-success.cjs`, `browser-success.log`, `s-R7-*.png`, `s-R8-*.png` |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — repository tidak memakai test runner untuk layar ini | — | `test-policy.md` |

Rincian uji browser (`[tiruan]` = respons backend tiruan lewat `route.fulfill`, karena jalur sukses baru tersedia 26 Nov):

| Kode | Skenario | Hasil |
| --- | --- | --- |
| K1 | Perangkat tanpa appointment: kartu beranda nonaktif (AC tambahan 1) | PASS |
| K2 | URL langsung tanpa appointment: pesan perangkat, tanpa isian | PASS |
| K3 | Perangkat dengan appointment: kartu beranda membuka route | PASS |
| K4 | Bar langkah Verifikasi > Ringkasan > Cetak Antrean; dua pilihan cara | PASS |
| K5, K26 | 1920×1080 dan potret 1080×1920 tanpa gulir mendatar | PASS |
| K6 | Periksa kosong: pesan `040` dan tanggal lahir, tanpa request | PASS |
| K7 | `k7qm4trz` tampil `K7QM-4TRZ` | PASS |
| K8 | Kode berisi `I`, `L`, `0`, `1` ditolak di layar (`040`), tanpa request | PASS |
| K9 | Kode format sah tetapi tidak ada: `404` pesan seragam dari backend | PASS |
| K10, K11 | Kode `PO-20261125-0006` + tanggal lahir benar: `409` "Check-in hanya dapat dilakukan pada 26 Nov 2026."; ketukan ganda = 1 request (AC 8, `045`) | PASS |
| K12, K13 | Lupa Kode: isian identitas; No. RM terformat `00-00-00-01` | PASS |
| K14 | Lupa Kode NIK + tanggal lahir pasien `PO-20261125-0007`: `409` dengan tanggal (AC 10) | PASS |
| K15 | NIK, tanggal lahir, kode tidak tampil sebagai teks layar | PASS |
| K16 | [tiruan] Ringkasan lengkap dengan nama tersamar | PASS |
| K17 | [tiruan] `409` `KSK-VAL-PO-053`: pesan, tombol nonaktif, arahan loket (AC 13 tampilan) | PASS |
| K18 | [tiruan] `400` `046`: kembali ke Verifikasi dengan pesan | PASS |
| K19 | [tiruan] Payload kunjungan sesuai paket dan kontrak; ketukan ganda = 1 request | PASS |
| K20–K22 | [tiruan] Tiket layar + arahan printer (AC tambahan 2); tiket thermal tanpa nama/No. RM; cetak otomatis 1×; Cetak Ulang mencetak lagi | PASS |
| K23 | Selesai kembali ke beranda | PASS |
| K24 | [tiruan] Paket pasien baru: catatan pindai KTP/KIA, Check-in nonaktif, NIK pembanding tidak tampil | PASS |
| K25 | Jam palsu 116 + 6 detik: peringatan lalu beranda | PASS |

Temuan saat uji yang langsung diperbaiki: baris arahan printer di atas tiket layar mendorong tombol Selesai ke y=1074–1124 pada layar 1920×1080 (terpotong). Arahan dipindah ke deskripsi header dan tombol Cetak Ulang ke samping Kembali ke Home; tombol Selesai kini di y=982–1032.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 7 (tampilan): check-in kode + tanggal lahir, tiket | Terpenuhi | Nyata S1; tiruan K16, K19–K22 |
| AC 8 (tampilan): sebelum hari H, data salah | Terpenuhi | K9, K10 |
| AC 10 (tampilan): Lupa Kode | Terpenuhi | Nyata K14 (`409`), S2 (`200` sampai tiket) |
| AC 13 (tampilan): jadwal tidak berlaku → loket | Terpenuhi | Layar K17 (respons tiruan dengan pesan yang sama persis); `409` `053` nyata di backend `BE-KSK-PO-007` C11 |
| Tambahan 1: kartu beranda hanya aktif dengan `kioskCanAppointment` | Terpenuhi | K1, K2, K3 |
| Tambahan 2: printer gagal → tiket di layar + arahan Cetak Antrean | Terpenuhi | K20–K22 |
| DoD: tabel keputusan base component dan `UI GATE` | Terpenuhi | Bagian 3.4 |
| Uji di layar kiosk | Terpenuhi di ukuran kiosk 1920×1080 dan 1080×1920 (browser), belum di perangkat fisik | K5, K26 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Yang belum | Cetak di printer thermal kiosk fisik (di luar jangkauan uji browser) |
| Catatan uji nyata | Kunjungan aktif lain milik pasien menolak check-in dengan pesan backend "Pasien masih memiliki kunjungan aktif …"; layar menampilkannya di Ringkasan dan pasien diarahkan ke petugas. Kunjungan uji yang menghalangi dibatalkan lewat API |
| Risiko | (1) Browser tidak tahu kertas benar-benar keluar; tiket selalu tampil di layar dan Cetak Ulang tersedia. (2) Menu Cetak Antrean menerbitkan nomor antrean admisi, bukan mencetak ulang tiket poli; arahan mengikuti desain yang disetujui, tetapi pasien yang tiketnya tidak keluar mungkin tetap perlu dibantu petugas. Perlu dikonfirmasi pemilik. (3) Pasien baru diarahkan ke loket sampai `FE-KSK-PO-006` |
| Perubahan sampingan | `NONE`. Dev server 3000 dinyalakan agent untuk uji dan dihentikan sesudahnya |
| Status Git | Berkas baru dan perubahan `kiosk-home-view.jsx` belum di-commit. Tidak ada commit, push, migration, atau eksekusi database |
| Langkah berikutnya | `FE-KSK-PO-006` (check-in pasien baru) |

## 9. Perubahan oleh `FE-KSK-PO-006` (2026-11-25)

Layar ini dipakai juga untuk check-in pasien baru. Sejak [`FE-KSK-PO-006`](FE-KSK-PO-006.md), paket pasien baru tidak lagi diarahkan ke loket: Ringkasan menampilkan tombol **Lanjut Pindai KTP/KIA**, lalu langkah **Pindai Kartu** muncul di antara Ringkasan dan Cetak Antrean. Skenario K24 di bagian 6 menggambarkan perilaku sementara sebelum `FE-KSK-PO-006`.

Regresi alur pasien lama sesudah perubahan itu: 11/11 PASS dengan respons tiruan (K16–K23, K25, serta K24b–K24c untuk perilaku pasien baru yang baru) — `scratchpad/fe-po005/regression-mock.cjs`, `regression-mock.log`.

