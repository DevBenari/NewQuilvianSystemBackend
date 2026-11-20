# Laporan Perubahan Backend — `BE-KSK-PO-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-006` |
| Judul | Verifikasi check-in kiosk |
| Slice | `EPIC KSK-PO-03` — check-in kiosk (`FR-KSK-PO-020`), gelombang teknis 5, rilis `MVP-2` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-006` |
| Trace | `KSK-DEC-033`, `040`; `KSK-DSN-PO-008`; `KSK-DSN-006`; `KSK-VAL-PO-040` s.d. `045` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | `BE-KSK-PO-005` ✅ |
| Klasifikasi | `MEDIUM` — endpoint kiosk baru yang menerbitkan token sekali pakai, tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Controllers,DTOS,Enums,Services}`, `Program.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a86001e2` (`sukmagp`) |
| Tanggal | 2026-11-20 |
| Status | 🟡 Sebagian — seluruh jalur penolakan terbukti di runtime; jalur sukses (`200`, token, paket) terbukti di harness dengan jam palsu. Runtime `200` menunggu hari kunjungan data uji, 2026-11-21 |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` (controller, service, DTO, enum) dan `TOUCHED LEGACY` (`OnlineRegistrationPublicService.MaskNumber`: `private` → `internal`, isi tetap) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), `QBE-API-001`/`QBE-DTO-001` (route `api/v1/...`, `ApiResponse<T>`), `QBE-LOG-001` (log tanpa data pribadi), `QBE-PERM-001` (endpoint perangkat memakai policy `KioskRead` yang terdaftar di `AuthorizationPolicies.ApprovedAlternativeAuthorization`, tanpa `[AccessAction]`, sesuai `KSK-DSN-006`) |
| Hardcode role | Tidak ada di kode baru. Policy `KioskRead` yang dipakai ulang memang memeriksa role (`Program.cs`), itu milik platform kiosk yang sudah ada dan bukan bagian task ini |

## 1. Masalah yang diperbaiki

Pendaftaran online sudah bisa disimpan (`BE-KSK-PO-005`), tetapi kiosk belum punya cara menemukannya pada hari kunjungan. Task ini membuat langkah pertama check-in: kiosk mengirim kode (atau NIK/No. RM untuk Lupa Kode) beserta tanggal lahir, lalu backend memeriksa dan menerbitkan token check-in berlaku 15 menit beserta paket data untuk membuat kunjungan.

## 2. Proses bisnis

1. Pasien memilih "Check-in Pasien Online" di kiosk, lalu memasukkan kode (contoh `K7QM-4TRZ`) dan tanggal lahir. Bila lupa kode, pasien memasukkan NIK atau No. RM dan tanggal lahir.
2. Backend mencari pendaftaran berstatus Menunggu Check-in:
   - Mode kode: kode dinormalkan (huruf besar, tanpa tanda hubung), di-hash SHA-256, lalu dicari. Tanggal lahir dicocokkan dengan master pasien untuk pasien lama, atau dengan isian online untuk pasien baru.
   - Mode Lupa Kode: pasien lama dikenali dengan aturan yang sama dengan halaman publik (NIK/No. RM + tanggal lahir, pasien gabungan dan non-aktif ditolak). Pasien baru dikenali dari NIK dan tanggal lahir yang diisi online.
3. Bila tidak cocok, sudah berstatus akhir, atau tanggal kunjungannya sudah lewat, jawabannya selalu `404` "Data tidak sesuai atau pendaftaran sudah tidak berlaku. Periksa kembali atau hubungi petugas."
4. Bila cocok tetapi tanggal kunjungan belum hari ini (WIB), jawabannya `409` "Check-in hanya dapat dilakukan pada 21 Nov 2026." Tanggal hanya disebut sesudah kode/identitas **dan** tanggal lahir terbukti.
5. Bila hari ini, backend menerbitkan token acak 256 bit, menyimpan hash-nya beserta batas berlaku (15 menit), lalu mengembalikan paket check-in: data kunjungan dari jadwal (unit, poli, dokter, ruang, jam), penjamin, rujukan, status consent, nama tersamar. Untuk pasien baru, paket juga memuat NIK online (pembanding hasil pindai KTP/KIA) dan kontak online. Kode pendaftaran tidak pernah ada di paket.
6. Status pendaftaran tidak berubah. Kunjungan baru dibuat pada langkah berikutnya (`BE-KSK-PO-007`) memakai token itu.

Contoh: Ibu Rina datang Rabu 25 Nov pukul 09.00, memasukkan `K7QM-4TRZ` dan 2 April 1985. Kiosk menerima paket "S*** R*****, Poli Paru, dr. Sari, 08.00–12.00, Asuransi ****7788" dan token yang berlaku sampai 09.15. Bila ia datang Selasa, kiosk menampilkan "Check-in hanya dapat dilakukan pada 25 Nov 2026."

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`KioskPatientLookupController.cs` dan `KioskAdmissionTicketController.cs` (pola kontroler kiosk), `AuthorizationPolicies.cs` dan policy `KioskRead` di `Program.cs`, `KioskPatientLookupService.cs` (rantai gabung, kelayakan), `OnlineRegistrationPublicService` (pencocokan, hash kode), `PatientMembershipStatusResolver` (pola `TimeProvider`), `AppDateTimeHelper`, payload kunjungan kiosk di frontend (`kiosk-new-patient-submit.helpers.jsx`, `kiosk-referral-utils.js`) untuk menentukan isi paket, laporan `BE-KSK-002` dan `BE-KSK-003` (cara uji akun kiosk).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Controllers/OnlineRegistrationCheckInController.cs` | **Baru.** `[Authorize]`, `[AccessController]` (`OnlineRegistrationCheckIn`, pola kontroler kiosk), `POST check-in/verify` dengan `[Authorize(Policy = KioskRead)]`, batas laju `OnlineRegistrationCheckIn`, `Cache-Control: no-store` |
| `.../Services/OnlineRegistrationCheckInService.cs` | **Baru.** `VerifyAsync`, `ComputeTokenHash` (dipakai `BE-KSK-PO-007`), pencarian kode dan Lupa Kode, paket check-in. Tanggal "hari ini" dari `TimeProvider` zona Asia/Jakarta |
| `.../DTOS/OnlineRegistrationCheckInDtos.cs` | **Baru.** `OnlineRegistrationCheckInVerifyRequest`, `OnlineRegistrationCheckInPackage` beserta bagian `visit`, `payer`, `newInsurance`, `referral` |
| `.../Enums/OnlineRegistrationCheckInMode.cs` | **Baru.** `Code = 1`, `Identity = 2` |
| `.../Services/OnlineRegistrationPublicService.cs` | `MaskNumber`: `private` → `internal`, isi tetap |
| `Program.cs` | `AddScoped<OnlineRegistrationCheckInService>`; policy `OnlineRegistrationCheckIn` per akun kiosk (`CheckInPermitPerMinute`, bawaan 10/menit) dan cabang `OnRejected` dengan pesan `KSK-VAL-PO-044` tanpa mencatat akun/IP |

**Delta terhadap desain (aditif, perlu diketahui pemilik):**

| Desain | Source | Alasan |
| --- | --- | --- |
| Paket: "kunjungan, penjamin, rujukan" | Bagian bersarang `visit`, `payer` (`newInsurance`), `referral`; tambahan `registrationNumber`, `serviceUnitName`, `roomId`, `sessionName`, `payerName`, `maskedNumber`, `documentCount` | Kiosk menyusun permintaan `POST patient-encounters/kiosk` dari paket ini, dan payload kunjungan kiosk yang ada memakai `roomId`. Kartu baru dikirim utuh karena kiosk membuatnya lewat `POST patient-insurances/kiosk` |
| — | `contactWhatsAppNumber`, `contactEmail` hanya untuk pasien baru | `KSK-DEC-048`: master pasien baru dibuat dari hasil pindai, sedangkan No. WA/email diambil dari isian online. Untuk pasien lama selalu `null` |
| Kontroler tanpa atribut akses selain policy | `[AccessController]` pada kelas, tanpa `[AccessAction]` | Pola `KioskPatientLookupController` dan `KioskAdmissionTicketController`. Verifier tetap PASS dan himpunan fallback tetap 68 |
| — | Tanggal kunjungan yang sudah lewat dijawab `404` | Pendaftaran itu menunggu ditutup Tidak Datang (`KSK-DEC-037`); menyebut tanggal lampau tidak membantu pasien |
| — | Verifikasi tidak bergantung pada `OnlineRegistration:Enabled` | Saklar itu mematikan halaman publik; pendaftaran yang sudah tersimpan tetap harus dapat di-check-in |
| — | Verifikasi tidak mengubah `RowVersion` dan tidak menulis riwayat | Tidak ada perubahan status; perubahan petugas yang sedang berjalan tidak ikut gagal |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: grup *Online Registration Check-In*, satu endpoint |
| Database | Hanya mengisi `CheckInTokenHash` dan `CheckInTokenExpiresAt` (kolom dari `BE-KSK-PO-001`). Tanpa migration |
| Keamanan/Auth | Tanpa login → `401`. Penebakan kode ditahan oleh tanggal lahir, hari kunjungan, dan batas 10 permintaan per menit per akun kiosk. Token hanya disimpan sebagai hash. Log hanya memuat id pendaftaran dan kode aturan |

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Online Registration Check-In

Base URL: `api/v1/health-services/registration-management/online-registrations/kiosk`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/check-in/verify` | Memeriksa kode atau identitas + tanggal lahir, lalu menerbitkan token check-in | `KioskRead`, `OnlineRegistrationCheckIn` (10/menit/akun) | `{ "mode": 1, "code": "K7QM-4TRZ", "birthDate": "1985-04-02" }` atau `{ "mode": 2, "identityType": 1, "identityValue": "3174…", "birthDate": "1985-04-02" }` | `ApiResponse<OnlineRegistrationCheckInPackage>` |

Contoh respons `200` (data contoh, dipersingkat):

```json
{
  "onlineRegistrationId": "…",
  "registrationNumber": "PO-20261120-0001",
  "checkInToken": "q3kz…43 karakter…",
  "expiresAt": "2026-11-25T02:15:00Z",
  "patientMode": 1,
  "patientId": "…",
  "maskedName": "S*** R*****",
  "identityNumberForScan": null,
  "visit": { "visitDate": "2026-11-25", "serviceUnitId": "…", "clinicId": "…", "clinicName": "Poli Paru", "doctorId": "…", "doctorName": "dr. Sari", "doctorScheduleId": "…", "roomId": "…", "startTime": "08:00", "endTime": "12:00" },
  "payer": { "patientType": 2, "paymentType": 2, "patientInsuranceId": "…", "payerName": "Asuransi Contoh", "maskedNumber": "****7788", "newInsurance": null },
  "referral": null,
  "hasConsent": false
}
```

Kode status: `200` ditemukan; `400` isian tidak sah (`KSK-VAL-PO-040`, `043`); `401` tanpa login; `403` akun bukan kiosk; `404` tidak cocok atau tidak berlaku (pesan seragam); `409` belum hari kunjungan (pesan menyebut tanggal); `429` lebih dari 10 permintaan per menit dari akun kiosk yang sama.

## 5. Verifikasi

Akun uji: seed SuperAdmin dari `appsettings.Development.json` (kredensial tidak dicetak), yang lolos policy `KioskRead`, sama dengan `BE-KSK-003`. Sandi akun perangkat kiosk tidak tersedia di sesi ini. Data runtime: enam pendaftaran uji `PO-20261120-0001` s.d. `0006` dari `BE-KSK-PO-005` (tanggal kunjungan 2026-11-21). Karena pendaftaran online paling cepat H+1, belum ada pendaftaran bertanggal hari ini; jalur sukses diuji lewat harness.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | Build succeeded, 0 error | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | PASS: fallback 68 cocok persis, endpoint telanjang baru 0, identitas wajib 24/24 | `PASS` | Keluaran verifier |
| Harness `harness-po006` (DI asli, EF in-memory, jam palsu 2026-11-25 09:00 WIB) | 26/26: kode pasien lama/baru → token 43 karakter base64url, hash = SHA-256 token, berlaku tepat 15 menit, status dan `RowVersion` tetap; verifikasi ulang menimpa token; paket pasien lama tanpa kode/NIK/kontak, kartu tersamar `****7788`; paket pasien baru dengan `identityNumberForScan`, kontak, kartu baru utuh, rujukan + 2 berkas, consent; Lupa Kode NIK pasien baru, No. RM bertanda hubung, NIK master pasien lama; 404 identik untuk tanggal lahir salah, kode tak ada, status Dibatalkan, tanggal kemarin, NIK salah tanggal lahir, NIK tanpa pendaftaran; 409 "…pada 26 Nov 2026." untuk kode dan Lupa Kode; sebelum hari H dengan tanggal lahir salah tetap 404; pendaftaran yang ditolak tidak mendapat token; 6 validasi 400 | `PASS` | Harness scratchpad (2 butir sempat gagal karena nomor pendaftaran uji memuat kodenya sendiri; harness diperbaiki, bukan source) |
| R1 tanpa login | `401` | `PASS` | `po006_test.py today` |
| R2 kode tak ada | `404`, `Cache-Control: no-store` | `PASS` | `po006_test.py today` |
| R3 validasi | Kode 7 karakter, memuat `O`, kosong → `400` `KSK-VAL-PO-040`; NIK 15 digit → `400` `KSK-VAL-PO-043`; tanpa mode → `400` | `PASS` | `po006_test.py today` |
| R4 AC 8 kode benar + tanggal lahir benar, sebelum hari H | 5 pendaftaran pasien baru: `409` "Check-in hanya dapat dilakukan pada 21 Nov 2026." | `PASS` | `po006_test.py today` |
| R5 kode pasien lama (huruf kecil) + tanggal lahir master | `409` menyebut tanggal; dengan tanggal lahir salah `404` seragam; kode tak ada `404` identik | `PASS` | `po006_test.py today` |
| R6 AC 10 Lupa Kode | NIK pasien lama, No. RM pasien lama, NIK 5 pasien baru → `409` menyebut tanggal; NIK yang tidak pernah tersimpan → `404`; tanggal lahir salah → `404` | `PASS` | `po006_test.py today` |
| RL1–RL2 batas laju (backend dijalankan ulang dengan batas bawaan) | Permintaan 1–10 `404`, ke-11 `429` "Terlalu banyak percobaan. Silakan coba lagi sebentar."; tanpa login tetap `401` | `PASS` | `po006_test.py rate` |
| L1 log dua run | 21 nilai sensitif (kode, NIK, No. RM, tanggal lahir, kontak): 0 temuan; satu-satunya `ERR` adalah scheduler absensi HR yang tidak terkait | `PASS` | Pemeriksaan log |
| D1–D4 runtime `200` pada hari kunjungan | Belum dapat dijalankan: tanggal kunjungan data uji 2026-11-21 | `NOT RUN` | Skrip siap: `po006_test.py dday` |
| `403` akun non-kiosk | Tidak diuji ulang; policy `KioskRead` yang sama sudah terbukti di `BE-KSK-002` | `NOT RUN` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 8: sebelum hari H ditolak dengan tanggal; tanggal lahir salah ditolak pesan umum | Terpenuhi | R4, R5, R5b, harness H5a, H6 |
| AC 10: Lupa Kode NIK/No. RM + tanggal lahir menemukan pendaftaran hari ini | **Sebagian** | Pencarian terbukti di runtime (R6, `409` karena belum hari H); hasil `200` pada hari H baru di harness (H4a–H4c) |
| Tambahan (1): kode berstatus akhir → `404` identik | Terpenuhi lewat harness | H5c. Runtime belum punya pendaftaran berstatus akhir; status itu baru dapat dibuat lewat `BE-KSK-PO-008`/`010` |
| Tambahan (2): belum hari H → `409` menyebut tanggal | Terpenuhi | R4, R5, R6 |
| Tambahan (3): paket tanpa kode; `identityNumberForScan` hanya pasien baru | Terpenuhi lewat harness | H1c, H3. Runtime menunggu D2–D3 |
| DoD: verifier otorisasi PASS (`KSK-DSN-006`) | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Yang belum | Runtime `200` (token tersimpan dan paket) memakai pendaftaran uji `PO-20261120-*` pada **2026-11-21** dengan `po006_test.py dday`. Sesudah PASS, task ini dapat ditandai ✅ |
| Peringatan | Batas laju dihitung per akun. Bila beberapa kiosk berbagi satu akun, kuota 10/menit ikut terbagi |
| Risiko tersisa | Kode 8 karakter dari 31 simbol (sekitar 8,5 × 10¹¹ kemungkinan) masih ditopang tanggal lahir dan batas laju; tebakan hanya mungkin dari kiosk yang login |
| Perubahan sampingan | `NONE` di repository. Harness, skrip uji, dan log berada di scratchpad |
| Status Git | Milik task ini: 4 berkas baru (controller, service, DTO, enum) dan perubahan pada `OnlineRegistrationPublicService.cs` serta `Program.cs`; ditambah perubahan task sebelumnya yang belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-KSK-PO-007` (check-in menjadi kunjungan) memakai `ComputeTokenHash`; atau `BE-KSK-PO-008`/`009` yang boleh paralel |
