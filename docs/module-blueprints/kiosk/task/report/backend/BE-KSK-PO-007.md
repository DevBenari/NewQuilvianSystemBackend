# Laporan Perubahan Backend — `BE-KSK-PO-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-007` |
| Judul | Check-in menjadi kunjungan |
| Slice | `EPIC KSK-PO-03` — check-in kiosk (`FR-KSK-PO-021`), gelombang teknis 6, rilis `MVP-2` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-007` |
| Trace | `KSK-DEC-034`, `036`, `039`, `046`, `048`; `INV-PO-02`, `06`; `KSK-DSN-PO-008`, `010`, `014`; `KSK-VAL-PO-046` s.d. `055`; PO-S.1 baris ketiga |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | `BE-KSK-PO-006` ✅ 2026-11-25 |
| Klasifikasi | `HIGH` — mengubah jalur bersama pembuatan kunjungan (`CreateEncounterCoreAsync`), tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Controllers,DTOS,Services}` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `c91db4b5` (`sukmagp`) |
| Tanggal | 2026-11-20; runtime hari H 2026-11-25 |
| Status | ✅ Selesai — penolakan dan regresi tiga kanal terbukti di runtime 2026-11-20; jalur sukses pasien lama dan pasien baru, rujukan + berkas, consent, `052`, `053`, `054`, `055`, dan kuota tepat penuh oleh pendaftaran sendiri terbukti di runtime hari H 2026-11-25 |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, prefix `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE`: `OnlineRegistrationCheckInService.Encounter.cs` (partial baru). `TOUCHED LEGACY`: `PatientEncounterController` (hook check-in di `CreateEncounterCoreAsync`; tiga kalimat kuota dijadikan konstanta tanpa mengubah isinya), `PatientEncounterDtos.cs` (dua field opsional), `OnlineRegistrationCheckInService.cs` (`partial` dan ringkasan) |
| QBE yang berlaku | `QBE-SVC-001` (logika check-in di service; controller hanya memanggil hook, sama dengan pola Reservasi Prioritas), `QBE-API-001`/`QBE-DTO-001` (kontrak `ApiResponse<T>` tidak berubah, field DTO aditif), `QBE-LOG-001` (log hanya id pendaftaran, id kunjungan, dan kode aturan), `QBE-PERM-001` (tidak ada endpoint baru; `POST /kiosk` tetap `KioskRead`, `POST /admin` tetap `PatientEncounter : Create`) |
| Prefix | Tidak ada entity baru. Entity yang ditulis: `RegOnlineRegistration`, `RegOnlineRegistrationHistory`, `RegOnlineRegistrationReferralDocument`, `RegPatientGeneralConsent`, `RegEncounterReferralDocument` — semua prefix `Reg` milik modul ini |
| Hardcode role | Tidak ada. Kanal ditentukan dari route (`logScope`), bukan dari role |

## 1. Masalah yang diperbaiki

Sesudah `BE-KSK-PO-006`, kiosk bisa memverifikasi kode dan menerima token check-in, tetapi belum bisa mengubah pendaftaran online menjadi kunjungan. Task ini menyambungkan token itu ke jalur kunjungan yang sudah ada (`POST patient-encounters/kiosk`). Kunjungan terbentuk sebagai appointment dengan sumber Pendaftaran Online. Pendaftaran berpindah ke Menjadi Kunjungan dalam transaksi yang sama, dan consent serta berkas rujukan yang diisi online ikut terbawa.

## 2. Proses bisnis

1. Pasien memasukkan kode dan tanggal lahir di kiosk. Backend menerbitkan token 15 menit dan paket check-in (`BE-KSK-PO-006`).
2. Bila pasien baru, kiosk lebih dulu memindai KTP/KIA. Kiosk lalu membuat master pasien lewat endpoint kiosk Pasien Baru yang sudah ada, atau memakai master yang NIK-nya sama. Kartu asuransi baru dibuat dengan cara yang sama.
3. Kiosk memanggil `POST patient-encounters/kiosk` dengan isi paket ditambah `onlineRegistrationId` dan `onlineRegistrationCheckInToken`.
4. **Sebelum transaksi**, `PrepareCheckInAsync` memeriksa:
   - token cocok dan belum kedaluwarsa;
   - status masih Menunggu Check-in;
   - tanggal kunjungan hari ini (WIB);
   - poli, jadwal, dokter, unit, jenis kunjungan Rawat Jalan, dan `IsAppointment` sama dengan pendaftaran;
   - pasien sama: pasien lama lewat `PatientId`, pasien baru lewat NIK master;
   - jenis pembayaran dan kartu sama;
   - faskes perujuk sama.

   Bila semuanya lolos, backend sendiri yang mengisi tanggal kunjungan, `IsAppointment = true`, `IsWalkIn = false`, `IsFromKiosk = true`, `RegistrationSource = 7`, dan rincian rujukan dari pendaftaran. Isian kiosk untuk field-field ini diabaikan.
5. Validasi jalur kunjungan yang biasa tetap berjalan: jadwal aktif, hari praktik, izin appointment dan kiosk, serta kuota. Bila jadwal batal atau berubah, kiosk menerima `409` "Jadwal dokter berubah, silakan ke loket pendaftaran.". Bila kuota penuh, `409` "Kuota dokter sudah penuh, silakan ke loket pendaftaran.".
6. **Di dalam transaksi**, sesudah kunjungan dan rincian rujukannya terbentuk, `CompleteCheckInInTransactionAsync` menjalankan langkah berikut:
   - memeriksa ulang pendaftaran;
   - mengubah status menjadi Menjadi Kunjungan (2);
   - menautkan kunjungan dan mengisi `CheckedInAt`;
   - mengisi `PatientId` untuk pasien baru;
   - menghapus hash token;
   - menyalin consent tampungan ke `RegPatientGeneralConsent` dengan waktu tanda tangan aslinya;
   - membuat baris `RegEncounterReferralDocument` yang menunjuk berkas yang sama, tanpa menyalin berkas;
   - menulis riwayat `CHECKED_IN` dengan sumber Kiosk.
7. Bila pendaftaran berubah di antara pemeriksaan dan penyimpanan, kunjungan dibatalkan utuh dan kiosk menerima `409` "Pendaftaran sudah berubah. Silakan hubungi petugas.". Contoh perubahan: petugas membatalkan, memindah jadwal, atau mengubah kontak, atau pasien memverifikasi ulang di kiosk lain.

**Contoh.** Pendaftaran `PO-20261122-0003` untuk kunjungan 25 Nov 2026: pasien baru Budi, Poli Paru dr. Sari, Asuransi Contoh dengan polis `POL-BARU-01`, rujukan Puskesmas Uji dengan 2 halaman surat, consent ditandatangani 22 Nov 2026 10:15 WIB. Pada 25 Nov, kiosk memindai KTP Budi dan membuat master pasien baru serta kartunya, lalu mengirim permintaan kunjungan. Hasilnya:

- kunjungan Rawat Jalan appointment dengan sumber 7;
- pendaftaran berstatus Menjadi Kunjungan dengan nomor kunjungan tertaut;
- consent tercatat pada kunjungan itu dengan waktu tanda tangan 22 Nov 10:15 WIB;
- dua dokumen rujukan kunjungan yang menunjuk berkas yang sama.

Bila kiosk mencoba mengganti dokter atau kartu, permintaan ditolak `400` dan pendaftaran tetap Menunggu Check-in.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientEncounterController.cs` (kanal, hook Reservasi Prioritas, transaksi, kunci, kuota, catch), `PatientEncounterDtos.cs`, `OnlineRegistrationCheckInService.cs`, `OnlineRegistrationService.Closure.cs` (`CountClosableOnScheduleAsync`, `CloseForEncounterAsync`), `OnlineRegistrationPublicService.Submit.cs` (`INV-PO-03`), `EncounterReferralService.cs` (`ValidateForCreateAsync`, `AttachToNewEncounterAsync`, `ApplyCompletion`), `ReferralDocumentStorageService.cs` (hapus berkas bersifat lunak), model `RegOnlineRegistration*`, `RegPatientGeneralConsent`, `RegEncounterReferralDocument`, `MstPatientInsurance`, kontrak `api-contract.md`, `validation-matrix.md`, `state-transition-matrix.md`, `acceptance-test-matrix.md`, `02-backend-architecture.md` PO.4.6.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` | `PatientEncounterCreateRequest` + `OnlineRegistrationId` (`Guid?`) dan `OnlineRegistrationCheckInToken` (`string?`, `[MaxLength(64)]`) |
| `Areas/HealthServices/RegistrationManagement/Services/OnlineRegistrationCheckInService.cs` | Kelas menjadi `partial`; ringkasan menunjuk berkas baru |
| `Areas/HealthServices/RegistrationManagement/Services/OnlineRegistrationCheckInService.Encounter.cs` (baru) | `PrepareCheckInAsync`, `CompleteCheckInInTransactionAsync`, `OnlineRegistrationCheckInPreparation`, `OnlineRegistrationCheckInConflictException`, tujuh pesan `KSK-VAL-PO-046` s.d. `055`; pencocokan token waktu-konstan (`CryptographicOperations.FixedTimeEquals`) |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | (1) Field baru dari `/admin` → `400` `047`. (2) `PrepareCheckInAsync` sesudah pemeriksaan kanal, sebelum tanggal dan validasi. (3) `RegistrationSource = 7` tanpa check-in → `400`. (4) Pada check-in, kegagalan jadwal/kuota jalur kunjungan → `409` `053`/`054`, termasuk cek ulang kuota di dalam transaksi. (5) `CloseForEncounterAsync` Jalur Lain dilewati untuk check-in. (6) `CompleteCheckInInTransactionAsync` sesudah kunjungan dan rujukan ditambahkan. (7) `catch` konflik check-in dan `DbUpdateConcurrencyException` → `409` `055` dengan rollback. Tiga kalimat kuota dijadikan konstanta (isi sama) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: dua field opsional pada `PatientEncounterCreateRequest`. Respons `PatientEncounterCreateResponse` tidak berubah. Permintaan tanpa field baru berperilaku sama (terbukti G1–G3) |
| Database | Tanpa migration. Menulis kolom yang sudah ada dari `BE-KSK-PO-001` (`Status`, `EncounterId`, `CheckedInAt`, `PatientId`, `CheckInToken*`, `EncounterReferralDocumentId`) serta baris `RegOnlineRegistrationHistory`, `RegPatientGeneralConsent`, `RegEncounterReferralDocument` |
| Berkas | Tidak ada salinan. Dokumen kunjungan menunjuk `StoragePath` yang sama. Hapus dokumen rujukan kunjungan bersifat lunak (`IsDelete`), jadi berkas fisik tidak hilang bagi pendaftaran online |
| Keamanan/Auth | Token dibandingkan sebagai hash secara waktu-konstan. Id yang tidak dikenal dijawab sama dengan token salah (`046`) supaya keberadaan id tidak bocor. Status akhir baru disebut (`055`) bila id sah. Tidak ada endpoint baru; policy tetap |
| Log | `Rule` + `RegistrationId` saat ditolak; `RegistrationId` + `EncounterId` saat sukses. Tanpa token, kode, NIK, No. RM, atau kontak (diperiksa L1) |

### 3.4 Delta terhadap kontrak dan roadmap

| No. | Kontrak/roadmap | Implementasi | Alasan |
| ---: | --- | --- | --- |
| 1 | `api-contract.md` menyebut `POST ''` sebagai route **petugas** yang menolak `onlineRegistrationId` | Source: `POST ''` dan `POST /kiosk` sama-sama kanal kiosk (`KioskRead`, `CreateEncounterForKiosk`); petugas = `POST /admin`. Penolakan `047` dipasang di `/admin` | Source adalah bukti as-is. Penolakan mengikuti kanal, bukan nama path |
| 2 | Roadmap: "status Menjadi Kunjungan **sebelum** validasi kuota kunjungan" | Status tetap 1 saat kuota dihitung. Pendaftaran itu dikecualikan dari hitungan lewat `CountClosableOnScheduleAsync` (`BE-KSK-PO-008`). Status diubah di transaksi yang sama sesudah kunjungan dan rujukan terbentuk | Consent butuh `PatientEncounterId` dan dokumen butuh `EncounterReferralId`. Hasilnya sama: slot pendaftaran tidak dihitung dua kali (harness Q1–Q3) |
| 3 | `validation-matrix.md` tidak memuat penolakan sumber 7 | `RegistrationSource = 7` tanpa check-in → `400` "Sumber registrasi Pendaftaran Online hanya untuk check-in pendaftaran online di kiosk." | `KSK-DSN-PO-014`: sumber 7 hanya lahir dari check-in. Sebelum Amendment PO nilai 7 memang ditolak validasi enum, jadi kunjungan biasa tidak berubah perilaku |
| 4 | `046` "token kosong, salah, kedaluwarsa" | Id tidak dikenal dan tanggal kunjungan bukan hari ini (lewat tengah malam) juga `400` `046`. Status bukan 1 → `409` `055` | Tidak membocorkan keberadaan id; token memang hanya terbit untuk hari H |
| 5 | Token maks. 64 | Token 65 karakter ditolak validasi model framework (`400`, bentuk `ProblemDetails` tanpa `message`) | Sama dengan atribut DTO lain di controller ini |

### 3.5 Keputusan pemilik yang masih terbuka

| Hal | Fakta | Usulan |
| --- | --- | --- |
| Faskes perujuk bukan mitra | Halaman online menerima faskes aktif apa pun. Jalur kunjungan mewajibkan faskes mitra yang berlaku pada tanggal kunjungan (`DEC-FRJ-001`, `RJ-VAL-PM-17`). Check-in pendaftaran dengan faskes non-mitra akan ditolak `400` `RJ-VAL-PM-17` di kiosk | Dipertahankan fail-closed (validasi RJ tidak dilonggarkan). Pemilik memutuskan salah satu: (a) halaman online hanya menampilkan faskes mitra, atau (b) check-in rujukan non-mitra diarahkan ke loket dengan pesan khusus |

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Patient Encounter (diperluas)

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/kiosk` (dan alias `''`) | Membuat kunjungan. Bila `onlineRegistrationId` + `onlineRegistrationCheckInToken` diisi, kunjungan menjadi check-in pendaftaran online | `KioskRead` | `PatientEncounterCreateRequest` + `onlineRegistrationId` (`Guid?`) + `onlineRegistrationCheckInToken` (`string?`, maks. 64) | `ApiResponse<PatientEncounterCreateResponse>` (tidak berubah) |
| `POST` | `/admin` | Sama seperti sebelumnya. Field check-in ditolak `400` | `PatientEncounter : Create` | Tidak berubah | Tidak berubah |

Contoh permintaan check-in pasien lama (data contoh):

```json
{
  "patientId": "6f1c…",
  "serviceUnitId": "a2d4…",
  "clinicId": "f068…",
  "doctorId": "9b7e…",
  "doctorScheduleId": "db0b…",
  "roomId": "51aa…",
  "visitDate": "2026-11-21",
  "encounterType": 1,
  "visitType": 1,
  "registrationSource": 2,
  "paymentType": 2,
  "patientInsuranceId": "c3d9…",
  "isFromKiosk": true,
  "isWalkIn": false,
  "isAppointment": true,
  "isReferral": false,
  "onlineRegistrationId": "0e8a…",
  "onlineRegistrationCheckInToken": "q3kz…43 karakter…"
}
```

Backend tetap menyimpan `registrationSource = 7`, `isWalkIn = false`, dan tanggal dari pendaftaran, berapa pun nilai yang dikirim kiosk.

| Kode | Arti pada check-in | Pesan |
| --- | --- | --- |
| `200` | Kunjungan terbentuk, pendaftaran Menjadi Kunjungan | — |
| `400` `KSK-VAL-PO-046` | Token kosong, salah, kedaluwarsa; id tidak dikenal; bukan hari kunjungan | "Waktu check-in habis. Silakan ulangi dari awal." |
| `400` `KSK-VAL-PO-047` | Field check-in dikirim ke `/admin` | "Pendaftaran online hanya dapat di-check-in di kiosk." |
| `400` `KSK-VAL-PO-048`/`049` | Poli, jadwal, dokter, unit, jenis kunjungan, `IsAppointment`, pembayaran, kartu, rujukan, atau `PatientId` pasien lama berbeda | "Data check-in tidak sesuai dengan pendaftaran online. Silakan ulangi." |
| `400` `KSK-VAL-PO-052` | NIK master pasien yang dikirim berbeda dari NIK online | "Data kartu identitas tidak sesuai dengan pendaftaran online. Silakan ke loket pendaftaran." |
| `409` `KSK-VAL-PO-053` | Jadwal batal, berubah, tidak berlaku hari ini, atau tidak menerima appointment/kiosk | "Jadwal dokter berubah, silakan ke loket pendaftaran." |
| `409` `KSK-VAL-PO-054` | Kuota penuh oleh kunjungan lain | "Kuota dokter sudah penuh, silakan ke loket pendaftaran." |
| `409` `KSK-VAL-PO-055` | Pendaftaran sudah bukan Menunggu Check-in, atau berubah selama check-in | "Pendaftaran sudah berubah. Silakan hubungi petugas." |

Validasi lain jalur kunjungan (pasien tidak aktif, kunjungan aktif, kartu tidak berlaku, faskes non-mitra) tetap `400` dengan pesan lamanya.

## 5. Verifikasi

Akun uji: seed SuperAdmin dari `appsettings.Development.json` (kredensial tidak dicetak), yang lolos `KioskRead` dan `PatientEncounter : Create`. Backend uji Release di `https://localhost:7185` terhadap `QuilvianNewDevSukma`. Data dibuat hanya lewat API. Backend user di 7184 tidak disentuh.

Pendaftaran online paling cepat H+1, dan token hanya terbit pada hari kunjungan. Karena itu, pada 2026-11-20 belum ada token sah di runtime. Jalur sukses dan tabrakan diuji lewat harness dengan DI asli, EF in-memory, dan jam palsu.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | Build succeeded, 0 error; 0 warning baru di empat berkas yang disentuh | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 4 berkas, VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Keluaran QBE |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | PASS: metadata gap 0, fallback 68 cocok persis, endpoint telanjang baru 0, identitas wajib 24/24 | `PASS` | Keluaran verifier |
| Harness `harness-po007` P1–P5 (`046`) | Token kosong, salah, id tidak dikenal, kedaluwarsa (lewat 1 detik), tanggal kunjungan besok → `400` pesan sama | `PASS` | Harness, jam palsu 2026-11-25 09:00 WIB |
| Harness P6, T5 (`055`) | Status Dibatalkan, dan token yang sudah dipakai (status 2) → `409` | `PASS` | Harness |
| Harness P7–P17, P19–P23 (`048`) | `IsAppointment = false`, IGD, jadwal/poli/dokter/unit lain, ganti Tunai, kartu lain, tambah/hapus rujukan, faskes lain, rujukan internal, penjamin turunan bayi, kartu baru polis beda, kartu pasien lain dengan polis sama → `400` | `PASS` | Harness |
| Harness P13 (`049`), P18 (`052`) | `PatientId` pasien lain pada pasien lama → `400` `048`; NIK master berbeda pada pasien baru → `400` `052` | `PASS` | Harness |
| Harness P24 | Penolakan tidak mengubah status, hash token, dan `RowVersion` | `PASS` | Harness |
| Harness P25–P26 (`KSK-DSN-PO-014`) | Normalisasi: tanggal pendaftaran, `IsAppointment`, bukan walk-in, kiosk, sumber 7, dokter dari jadwal; kartu baru cocok walau polis beda huruf/spasi; nomor dan rincian rujukan dari pendaftaran menimpa isian kiosk; tanggal surat 20 Nov → `2026-11-19T17:00Z` (awal hari WIB) | `PASS` | Harness |
| Harness Q1–Q3 (tambahan 1) | Jadwal kuota total 1 / appointment 1 terisi pendaftaran itu sendiri: pemiliknya tidak penuh, pasien lain penuh; pasien baru dikenali lewat NIK master | `PASS` | Harness (rumus `ValidateDoctorScheduleQuotaAsync`) |
| Harness C1 (AC 9 pasien lama) | Status 2, `EncounterId`, `CheckedInAt`, hash dan kedaluwarsa token kosong, `RowVersion` baru; riwayat `CHECKED_IN` 1→2 sumber Kiosk dengan pelaku dan `EncounterId`; tanpa consent baru | `PASS` | Harness |
| Harness C2 (AC 9, 14, `KSK-DSN-PO-010`) | Pasien baru: `PatientId` diisi master; consent dengan `SignedAt` asli `2026-11-22T03:15Z` (bukan waktu check-in), penanda tangan, 12 klausul, tanda tangan; 2 `RegEncounterReferralDocument` dengan `StoragePath`, nama, tipe, ukuran, urutan sama; berkas yang sudah dihapus tidak ikut; `EncounterReferralDocumentId` terisi; `CaptureSource = 3`; rujukan lengkap | `PASS` | Harness |
| Harness T1–T4 (`055`, tambahan 2) | Verifikasi ulang di kiosk lain, pembatalan petugas, pindah jadwal sesudah pemeriksaan → `OnlineRegistrationCheckInConflictException`; perubahan kontak di antara muat dan simpan → `DbUpdateConcurrencyException`; status tetap 1, tanpa kunjungan, tanpa riwayat `CHECKED_IN` | `PASS` | Harness. Total 44/44 |
| R1–R1c (`047`) | `POST /admin` dengan id + token, id saja, token saja → `400` | `PASS` | `po007_test.py today` |
| R2–R2e (`046`) | `POST /kiosk` pada pendaftaran nyata `PO-20261120-0001` dengan token salah, tanpa token, id tidak dikenal, token tanpa id → `400` pesan sama; token 65 karakter → `400` | `PASS` | `po007_test.py today` |
| R3–R3b | Sumber 7 tanpa check-in di kiosk dan petugas → `400` | `PASS` | `po007_test.py today` |
| R4 (tambahan 2) | Sesudah delapan penolakan: status 1, `RowVersion` sama, tanpa kunjungan, riwayat tetap 1 | `PASS` | `po007_test.py today` |
| G1–G3 (tambahan 3, DoD regresi) | Tanpa field baru, 2026-11-20: RJ petugas `200` sumber 1 walk-in; kiosk `200` sumber 2 `IsFromKiosk`; IGD `200` jenis 2 | `PASS` | `po007_test.py today` (3 kunjungan uji di `po007_ledger.json`) |
| L1 log | 0 `ERR`/`FTL`; 21 nilai sensitif (kode, NIK, No. RM, tanggal lahir, kontak) dan token uji: 0 temuan; penolakan tercatat sebagai `Rule` + `RegistrationId` | `PASS` | Pemeriksaan log |
| C1–C6 runtime hari H pasien lama (Data uji `PO-20261125-0001` s.d. `0009` semula bertanggal 2026-11-26; dengan izin pemilik (2026-11-25, AskUserQuestion) tanggal kunjungannya dipindah ke 2026-11-25 lewat satu `UPDATE` 9 baris pada `RegOnlineRegistration` (tanggal, jadwal padanan yang dibuat lewat API, `RowVersion` baru) supaya token hari H dapat terbit hari itu (`po025_shift.py`)) | 6/6 PASS: C1 token lama sesudah verifikasi ulang `400` `046`; C2 poli diganti `400` `048`, status tetap 1; C3 check-in `200`, kunjungan sumber 7, appointment, kiosk, bukan walk-in; C4 pendaftaran status 2, nomor kunjungan, `checkedInAt`, riwayat `CHECKED_IN` sumber Kiosk 1→2; C5 token sama dipakai lagi `409` `055`; C6 kode diverifikasi lagi `404` | `PASS` | `po025_dday.py`, `po025-dday.log` |
| C7–C10, C13–C15 runtime hari H pasien baru | 7/7 PASS: C7 master dibuat lewat `POST patients/kiosk` dengan NIK pendaftaran; C8 check-in `200`, pendaftaran status 2, No. RM terisi; C9 kunjungan `isReferral`, sumber 7; C13 rujukan kunjungan 2 dokumen, `CaptureSource` 3; C14 general consent kunjungan ada, `SignedAt` sama dengan waktu tanda tangan online; C15 kedua berkas (PDF, PNG) terbuka dari kunjungan; C10 master NIK berbeda `400` `052`, status tetap 1 | `PASS` | `po025-dday.log` |
| C11, C12, C16 runtime hari H jadwal dan kuota | 3/3 PASS: C11 jadwal dinonaktifkan sesudah verifikasi `409` `053`, status tetap 1; C12 kuota jadwal 2/2 diturunkan ke 1 (terisi pendaftaran lain) `409` `054`, status tetap 1; C16 kuota dikembalikan 2/2 (terisi pendaftaran itu sendiri dan satu lainnya) → check-in `200`, kuota terpakai tetap 2 | `PASS` | `po025-dday.log`. Dua kunjungan regresi uji 20 Nov (`ENC-RSMMC-00399`, `00400`) dibatalkan lewat API karena memblokir pasien uji (aturan kunjungan aktif) |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 7 (sisi backend): kunjungan `IsAppointment`, sumber 7, status 2; kode sekali pakai | Terpenuhi | Harness C1, P25, T5; runtime C3–C6 |
| AC 9: pasien baru dengan master baru atau NIK lama | Terpenuhi | Harness C2, P26, Q3; runtime C7–C8 (master baru dari endpoint kiosk Pasien Baru) |
| AC 13: jadwal tidak berlaku → `409` `053` | Terpenuhi | Runtime C11 |
| AC 14: consent melekat dengan `SignedAt` asli | Terpenuhi | Harness C2b; runtime C14 |
| AC 17 (sisi backend): NIK master berbeda → `400` `052` | Terpenuhi | Harness P18; runtime C10 |
| `KSK-VAL-PO-046` s.d. `049`, `052`, `055` | Terpenuhi (harness + runtime sebagian) | P1–P24, T1–T5; R1–R2e |
| `KSK-VAL-PO-050`, `051` | Di luar backend | Layar kiosk (`FE-KSK-PO-006`) |
| `KSK-VAL-PO-053`, `054` | Terpenuhi | Runtime C11, C12 |
| Tambahan (1): kuota tepat penuh oleh pendaftaran itu sendiri | Terpenuhi | Harness Q1–Q3; runtime C16 |
| Tambahan (2): kegagalan di transaksi membatalkan kunjungan, status tetap 1 | Terpenuhi | Harness T1–T4; runtime R4, C2, C10–C12 (status tetap 1 sesudah penolakan) |
| Tambahan (3): kunjungan biasa tanpa field baru sama seperti sebelumnya | Terpenuhi | G1–G3, R3–R3b |
| DoD: regresi RJ/kiosk/IGD satu kunjungan per kanal | Terpenuhi | G1–G3 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Yang belum | Tidak ada. Runtime hari H terbukti 2026-11-25 tanpa perubahan source |
| Catatan runtime | Aturan kunjungan aktif berlaku pada check-in: dua pasien uji sempat ditolak `400` "Pasien masih memiliki kunjungan aktif …" karena kunjungan regresi 20 Nov; perilaku ini benar dan ditampilkan apa adanya di kiosk |
| Keputusan pemilik | Faskes perujuk non-mitra (bagian 3.5) |
| Risiko tersisa | Jalur bersama kunjungan bertambah satu cabang. Cabang itu hanya aktif bila field check-in diisi, dan regresi tiga kanal PASS |
| Data uji | 3 kunjungan regresi 2026-11-20 (2 dibatalkan 2026-11-25), kunjungan check-in 2026-11-25 dan 2 master pasien uji di `po025_dday_ledger.json`, jadwal uji `TEST-PO025`/`TEST-PO026`. Dibersihkan bersama data uji modul |
| Perubahan sampingan | `NONE` di repository. Harness, skrip uji, dan log ada di scratchpad |
| Migration/DB | Tidak ada migration. Satu `UPDATE` data uji 9 baris atas izin eksplisit pemilik 2026-11-25 (tanggal kunjungan); tulis lain lewat API |
| Status Git | Source task ini sudah di-commit pemilik (`a30d9f68`). Pembaruan 2026-11-25 hanya laporan, roadmap, traceability, dan `MODULE-STATUS.md`. Agent tidak membuat commit |
| Langkah berikutnya | `FE-KSK-PO-006` (check-in pasien baru di kiosk) memakai kontrak ini |
