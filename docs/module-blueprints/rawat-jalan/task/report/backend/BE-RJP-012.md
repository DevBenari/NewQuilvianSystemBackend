# Laporan Perubahan Backend — `BE-RJP-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-012` |
| Judul | Endpoint publik link konfirmasi |
| Slice | `MVP-2`, `EPIC RP-06` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-012` |
| Trace | `FR-RP-050`, `051`, `053`; `RJ-PRI-DEC-008`, `012`, `028`; `VAL-RP-07` s.d. `09`; `NFR-RP-007`; `INV-RP-05` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` — `api-contract.md` *Priority Reservation Arrival*; `permission-audit-matrix.md` RP-3, RP-4 |
| Dependency | `BE-RJP-007` ✅, `BE-RJP-010` ✅ |
| Klasifikasi | `HIGH` — skor 9: diperiksa 9–20 (1), diubah 5 (1), logika sedang (1), kontrak baru (2), keamanan tinggi (3: endpoint tanpa login, rahasia di path), workflow (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `PriorityReservationArrivalController.cs` (baru), `OutpatientPriorityReservationService.cs`, `OutpatientPriorityReservationDtos.cs`, `QueueRealtimeService.cs`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-19 |
| Status | ✅ Selesai — runtime 12/12 `PASS`. **Go-live tetap menunggu `RJ-PRI-OQ-003`** (IT/infrastruktur) dan tinjauan keamanan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `Program.cs` dan `QueueRealtimeService` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001` |
| Role access | `[AllowAnonymous]` di tingkat kelas, **tanpa** `[AccessController]`, agar tidak muncul di layar Akses Role (desain RP.6). Dijaga kode acak 256 bit dan policy `PriorityReservationArrivalPublic` |

---

## 1. Masalah yang diselesaikan

Pasien belum bisa memberi tahu rumah sakit bahwa ia sudah tiba (PDF 2C). Sesudah task ini, link di pesan WA membuka
keadaan reservasi, dan tombol "Saya sudah tiba" mengubah reservasi menjadi Sudah Tiba sekaligus memberi kabar ke layar
admisi.

## 2. Proses bisnis

| Keadaan reservasi | `GET /{code}` | `POST /{code}/confirm` |
| --- | --- | --- |
| Menunggu Kedatangan, sebelum hari kunjungan | `200 NOT_YET_OPEN` "Konfirmasi kedatangan baru bisa dilakukan pada {tanggal}." | Sama, tanpa perubahan |
| Menunggu Kedatangan, hari kunjungan | `200 READY` | Menjadi Sudah Tiba (sumber `PatientLink`), riwayat `ARRIVED`, kabar `PriorityReservationChanged`; jawab `200 CONFIRMED` |
| Sudah Tiba atau Menjadi Kunjungan | `200 CONFIRMED` dengan jam konfirmasi pertama | Sama, tanpa perubahan |
| Dibatalkan, Tidak Datang, Didaftarkan lewat Jalur Lain, hari kunjungan sudah lewat, kode salah, kode sudah diganti | `404` "Link tidak berlaku. Silakan hubungi petugas pendaftaran." | Sama |
| Lebih dari 20 permintaan per menit dari satu IP | `429` "Terlalu banyak percobaan. Coba lagi sebentar lagi." | Sama |

Contoh dari uji: link dibuka pada hari kunjungan pukul 10.23 WIB, pasien menekan "Saya sudah tiba", dan jawabannya
"Kedatangan Anda sudah tercatat pukul 10.23 WIB. Silakan menunggu di Ruang Tunggu Prioritas; petugas akan menghampiri
Anda." Saat link dibuka lagi, jam yang tampil tetap 10.23.

Respons tidak memuat nama, No. RM, NIK, tingkat member, poli, dokter, maupun ID internal. Status Menjadi Kunjungan
dijawab `CONFIRMED` karena kontrak hanya mencantumkan Dibatalkan, Tidak Datang, dan Jalur Lain sebagai `404`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/PriorityReservationArrivalController.cs` | Baru | `GET /{code}`, `POST /{code}/confirm`, `Cache-Control: no-store`, `RedactRequestPath` |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationService.cs` | Diperbarui | `GetArrivalStateAsync`, `ConfirmArrivalByCodeAsync`, pencarian lewat hash, validasi bentuk kode (43 karakter base64url), jam konfirmasi dibulatkan ke mikrodetik |
| `Areas/HealthServices/RegistrationManagement/DTOS/OutpatientPriorityReservationDtos.cs` | Diperbarui | `PriorityReservationArrivalPublicResponse` |
| `Areas/HealthServices/RegistrationManagement/Services/QueueRealtimeService.cs` | Diperbarui | Galat kirim kabar reservasi dicatat lewat Serilog langsung, bukan `LoggerService` |
| `Program.cs` | Diperbarui | Policy `PriorityReservationArrivalPublic` (partisi IP, `ArrivalLinkPermitPerMinute`, bawaan 20); cabang `OnRejected` khusus policy ini; middleware penyamar path sesudah `UseSerilogRequestLogging` |

### 3.2 Perlindungan kode di log (`INV-RP-05`, RP-4)

Kode link ada di path URL sesuai kontrak. Tanpa penanganan khusus, kode itu masuk ke tiga tempat: log permintaan
Serilog (`RequestPath`, `Path`, `DisplayMessage`), setiap entri `LoggerService` (yang menyertakan path), dan scope log
hosting yang menempel pada semua log MEL selama permintaan. Penanganannya:

| Celah | Penanganan |
| --- | --- |
| Log permintaan Serilog | Middleware mengganti `Request.Path` menjadi `/…/priority-reservation-arrivals/{code}[/confirm]` sesudah pipeline selesai dan sebelum log ditulis |
| `LoggerService` dan scope hosting | Controller publik **tidak menulis log**. Jejak konfirmasi cukup riwayat `ARRIVED` sumber `PatientLink` |
| Penolakan batas laju | Dicatat lewat Serilog langsung tanpa path dan tanpa IP |
| Galat kirim kabar realtime | Dicatat lewat Serilog langsung (tidak membawa scope path) |

Delta terhadap RP-4: "Konfirmasi lewat link — dicatat di logger tanpa kode dan tanpa IP" tidak dibuat sebagai entri
`LoggerService`, karena setiap entri `LoggerService` selama permintaan publik pasti membawa path berkode. Jejak tahan
lamanya tetap ada di tabel riwayat. "Kode salah — dihitung saja" tidak dibuat sebagai penghitung tersendiri; jumlahnya
dapat dibaca dari log permintaan tersamar dengan status `404`.

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Priority Reservation Arrival

Base URL: `api/v1/health-services/registration-management/priority-reservation-arrivals`

Tanpa login (`[AllowAnonymous]`), dibatasi `PriorityReservationArrivalPublic` (20 permintaan per menit per IP).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{code}` | Keadaan link | Publik (kode acak) | — | `ApiResponse<PriorityReservationArrivalPublicResponse>` | Tersedia |
| `POST` | `/{code}/confirm` | Pasien menekan "Saya sudah tiba" | Publik (kode acak) | — | `ApiResponse<PriorityReservationArrivalPublicResponse>` | Tersedia |

Contoh respons:

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Kedatangan Anda sudah tercatat pukul 10.23 WIB. Silakan menunggu di Ruang Tunggu Prioritas; petugas akan menghampiri Anda.",
  "data": {
    "reservationNumber": "RSV-20261019-0042",
    "visitDate": "2026-10-19T00:00:00",
    "state": "CONFIRMED",
    "confirmedAt": "2026-10-19T03:23:57.359967Z",
    "message": "Kedatangan Anda sudah tercatat pukul 10.23 WIB. …"
  }
}
```

Kode status: `200`; `404` link tidak berlaku (isi sama untuk semua sebab); `429` batas laju.

## 5. Verifikasi

Data samaran: pasien `RJPR UJI LNK-A`..`F`, membership Gold disisipkan SQL data uji. Reservasi F diubah SQL menjadi
Tidak Datang (data uji). Semua permintaan publik memakai klien tanpa cookie login. Kabar realtime didengar akun admin
lewat `sr_board.cjs`.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build, `has-pending-model-changes`, QBE Strict (5 berkas) | 0 error; bersih; VIOLATION 0 | `PASS` |
| `R-RP-11` a | Reservasi besok: `GET` dan `confirm` `NOT_YET_OPEN` "…pada 20 Oktober 2026."; status tidak berubah | `PASS` |
| Cache | `Cache-Control: no-store` | `PASS` |
| `R-RP-11` b / `UAT-RP-14` | Hari H: `READY`, lalu `confirm` `CONFIRMED`; DB Sudah Tiba, sumber `PatientLink`, satu `ARRIVED` tanpa pelaku | `PASS` |
| `R-RP-11` c | Dibuka dan ditekan lagi: `CONFIRMED` dengan jam pertama yang sama persis; `ARRIVED` tetap satu | `PASS` |
| `R-RP-13` | Kunci respons hanya `reservationNumber`, `visitDate`, `state`, `confirmedAt`, `message`; tanpa nama, No. RM, kode tingkat, ID pasien, ID reservasi | `PASS` |
| `R-RP-15` (link) | Sesudah petugas mengembalikan ke Belum Tiba, link yang sama `READY` lagi dan dapat dikonfirmasi | `PASS` |
| `R-RP-23` / `UAT-RP-15` | Sesudah kirim ulang: link lama `404` "Link tidak berlaku…", link baru `READY` | `PASS` |
| `R-RP-12` | Dibatalkan, Tidak Datang, kode acak, kode tak berformat, dan `confirm` pada reservasi Dibatalkan: kelimanya `404` dengan isi sama (tanpa menghitung `timestamp`) | `PASS` |
| Status Dibatalkan | `confirm` tidak mengubah apa pun | `PASS` |
| `UAT-RP-14` realtime | Board menerima satu `PriorityReservationChanged` `ARRIVED`, status 2 | `PASS` |
| `R-RP-25` | Sesudah jeda 62 detik, 25 permintaan beruntun: 20 pertama `404`, ke-21 s.d. 25 `429` "Terlalu banyak percobaan. Coba lagi sebentar lagi." | `PASS` |
| Log | 7 kode yang dipakai uji tidak ada di log berkas maupun konsol; 120 baris memakai path tersamar `/{code}` | `PASS` |

Skrip: `rt_rjp012.py` di scratchpad sesi. Total 12/12 `PASS`.

**Percobaan yang tidak dipakai sebagai bukti.** Run pertama 10/12: (1) jam konfirmasi di respons pertama berpresisi
7 digit sedangkan baca ulang dari DB 6 digit — diperbaiki di produk dengan membulatkan ke mikrodetik sebelum simpan;
(2) skrip membandingkan body termasuk `timestamp` — diperbaiki di skrip.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`. Regresi batas laju Cek No. RM kiosk tidak diuji ulang; cabang lamanya
tidak berubah selain pemeriksaan nama policy di depannya.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-11` | Terpenuhi | a, b, c |
| `R-RP-12` | Terpenuhi | Uji 404 seragam |
| `R-RP-13` | Terpenuhi | Kunci respons |
| `R-RP-25` | Terpenuhi | Uji batas laju |
| `UAT-RP-14` | Terpenuhi sisi backend (status + kabar realtime); bunyi layar di `FE-RJP-005` | Uji |
| `UAT-RP-15` | Terpenuhi | Uji kirim ulang |
| Sisa `R-RP-15` dan `R-RP-23` dari `BE-RJP-007` | Terpenuhi | Uji link |
| Go-live | **Belum** — menunggu `RJ-PRI-OQ-003` dan tinjauan keamanan | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu-satunya endpoint bisnis tanpa login. Sebelum go-live: (1) jawab `RJ-PRI-OQ-003` (domain publik, reverse proxy, `ForwardedHeaders` agar batas laju memakai IP asli pasien, bukan IP proxy); (2) setel `PublicArrivalBaseUrl` per lingkungan; (3) tinjauan keamanan |
| Masalah yang diketahui | Bila terjadi galat tak tertangani selama permintaan publik, `GlobalExceptionMiddleware` mencatat galat itu beserta scope path, sehingga kode dapat ikut tercatat. Risiko rendah; perlu ditinjau bersama tinjauan keamanan |
| Risiko tersisa | Pesan WA yang diteruskan memungkinkan orang lain menekan "Saya sudah tiba" (diterima di `RJ-PRI-DEC-012`); petugas dapat mengembalikan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | Roadmap frontend `FE-RJP-003` dst. |
