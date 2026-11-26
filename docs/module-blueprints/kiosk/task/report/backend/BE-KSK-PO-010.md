# Laporan Perubahan Backend — `BE-KSK-PO-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-010` |
| Judul | Ubah jadwal, ubah kontak, batal |
| Slice | `EPIC KSK-PO-04` — layar petugas (`FR-KSK-PO-031`, `032`), gelombang teknis 6, rilis `MVP-3` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-010` |
| Trace | `KSK-DEC-051`, `052`; `KSK-PO-AS-003`; `KSK-DSN-PO-016`; `GATE-PO-16`; `KSK-VAL-PO-060` s.d. `072`; PO-S.1 baris enam s.d. delapan, PO-S.2 |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | `BE-KSK-PO-009` (source sudah ada; task ini sekaligus menutup kriteria yang tertunda di sana) |
| Klasifikasi | `HIGH` — perintah petugas yang memindah kuota antar jadwal dan menutup pendaftaran, dengan kontrol konkurensi |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Controllers,DTOS,Services}` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `93f0fa1b` (`sukmagp`, berisi `BE-KSK-PO-001` s.d. `009`) |
| Tanggal | 2026-11-20 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di runtime |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` (`OnlineRegistrationService.Commands.cs`, DTO perintah, tiga endpoint) dan perubahan kode baru task sebelumnya (`OnlineRegistrationPublicService`: helper dibuka `internal` dengan overload berparameter, perilaku pengiriman tetap) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), `QBE-API-001`/`QBE-DTO-001`, `QBE-LOG-001` (log tanpa kontak), `QBE-PERM-001` (`[AccessAction]` `Update`/`Cancel` + `[AccessPermission("OnlineRegistration", ...)]`, argumen sama persis) |
| Arketipe endpoint | Transaksi ber-lifecycle: perpindahan status lewat `POST /{id}/cancel`; suntingan atribut lewat `PUT /{id}/schedule` dan `PUT /{id}/contact` sesuai kontrak yang disetujui (standar umum menyarankan `PATCH`; kontrak `v2` menetapkan `PUT`, sehingga kontrak yang dipakai). Tanpa `DELETE` |
| Hardcode role | Tidak ada |

## 1. Masalah yang diperbaiki

Petugas sudah dapat melihat pendaftaran online (`BE-KSK-PO-009`), tetapi belum dapat melayani pasien yang meminta pindah tanggal/dokter, mengganti No. WA/email, atau membatalkan, dan belum dapat membersihkan pendaftaran mencurigakan (`KSK-DEC-051`, `052`).

## 2. Proses bisnis

1. **Ubah jadwal.** Petugas memilih tanggal baru (besok sampai 30 hari ke depan) dan jadwal dokter di poli yang sama. Sistem memeriksa jadwal masih mengizinkan appointment dan kiosk, kuota appointment masih ada, dan pasien belum punya pendaftaran atau kunjungan lain pada tanggal baru. Kuota jadwal lama langsung lepas, kuota jadwal baru terpakai. Kode pendaftaran tetap sama. Contoh: Ibu Rina menelepon minta pindah dari Sabtu 21 Nov ke Senin 23 Nov dengan dokter lain di Poli Paru. Kuota Sabtu bertambah satu, kuota Senin berkurang satu, dan bila ia mencoba check-in hari Sabtu, kiosk menjawab "Check-in hanya dapat dilakukan pada 23 Nov 2026."
2. **Ubah kontak.** Petugas mengganti No. WA atau email dengan aturan yang sama dengan halaman online. Kode tidak dikirim ulang; pasien yang lupa kode memakai Lupa Kode di kiosk (`KSK-PO-AS-003`).
3. **Batal.** Petugas mengisi alasan (5–500 karakter). Status menjadi Dibatalkan, kode langsung ditolak di kiosk, dan kuota appointment kembali. Pembatalan tidak dapat dibatalkan ulang.
4. Ketiga perintah hanya untuk status Menunggu Check-in dan wajib mengirim versi data terakhir. Bila data sudah diubah petugas lain, jawabannya "Data sudah berubah. Muat ulang lalu coba lagi."
5. Setiap perintah tercatat di riwayat beserta petugas, waktu, dan nilai lama/baru (tanggal, jadwal, kontak, atau alasan).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OutpatientPriorityReservationService` (`RescheduleAsync`, `ExecuteStaffCommandAsync`, pola `RowVersion`), `OutpatientPriorityReservationController` (respons perintah, `LoggerService`), `DoctorScheduleQuotaService`, `OutpatientEncounterRules`, `OnlineRegistrationPublicService` (aturan pengiriman yang dipakai ulang), `DoctorScheduleController` dan DTO-nya (data uji jadwal).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Services/OnlineRegistrationService.Commands.cs` | **Baru** (partial). `ChangeScheduleAsync`, `ChangeContactAsync`, `CancelAsync`, pemeriksaan bersama (ada, versi cocok, Menunggu Check-in), riwayat, simpan dengan penanganan bentrok versi |
| `.../Services/OnlineRegistrationService.cs` | Kelas `partial`; konstruktor menerima `OnlineRegistrationPublicService`, `DoctorScheduleQuotaService`, `IConfiguration`, `ILogger` |
| `.../Controllers/OnlineRegistrationController.cs` | `PUT {id}/schedule`, `PUT {id}/contact` (`Update`), `POST {id}/cancel` (`Cancel`); log perintah lewat `LoggerService` tanpa kontak maupun alasan |
| `.../DTOS/OnlineRegistrationDtos.cs` | `OnlineRegistrationScheduleChangeRequest`, `OnlineRegistrationContactChangeRequest`, `OnlineRegistrationCancelRequest` |
| `.../Services/OnlineRegistrationPublicService.Submit.cs` | Dipakai ulang tanpa mengubah perilaku pengiriman: `ParseContact`, `LockOnlinePatientAsync(patientId, nik)`, `HasOpenRegistrationAsync(..., excludeRegistrationId)`, `HasActiveOutpatientEncounterOnAsync` menjadi `internal` (versi lama tetap sebagai pembungkus) |
| `.../Services/OnlineRegistrationPublicService.cs` | `LoadOfferedSchedulesAsync`: `private` → `internal` |

**Delta terhadap kontrak/desain:**

| Kontrak | Source | Alasan |
| --- | --- | --- |
| Kontak sama dengan sekarang | `400` "Tidak ada perubahan kontak." | Pasangan `KSK-VAL-PO-066` untuk kontak; mencegah riwayat kosong |
| `KSK-VAL-PO-063` jadwal tidak ditemukan | `409` dengan pesan `KSK-VAL-PO-021` | Sama dengan jadwal yang tidak lagi berlaku |
| Batal | Juga mengisi `IsCancel`, `CancelBy`, `CancelDateTime` (`IdentityModel`) dan menghapus token check-in | Konsisten dengan kolom audit bawaan; token lama tidak boleh dipakai |
| Ubah jadwal | Token check-in yang terlanjur terbit dihapus | Token terikat tanggal lama |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: tiga endpoint perintah sesuai `KSK-CONTRACT-v2` |
| Database | Menulis `RegOnlineRegistration` (jadwal, kontak, status, versi) dan `RegOnlineRegistrationHistory`. Tanpa migration. Aksi `Update` dan `Cancel` dibuat seeder saat aplikasi menyala |
| Keamanan/Auth | `Update` dan `Cancel` terpisah dari `Read`. Log tidak memuat kontak lama/baru maupun alasan; nilai itu hanya di riwayat yang dibaca petugas ber-hak |
| Konkurensi | Versi data (`expectedRowVersion`) dicek sebelum dan saat simpan. Ubah jadwal mengambil kunci pasien lalu kunci kuota jadwal baru, urutan yang sama dengan pengiriman pendaftaran dan pembuatan kunjungan |

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Online Registration

Base URL: `api/v1/health-services/registration-management/online-registrations`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}/schedule` | Ubah tanggal dan/atau jadwal dokter di poli yang sama | `OnlineRegistration : Update` | `{ "visitDate": "2026-11-23", "doctorScheduleId": "…", "expectedRowVersion": "…" }` | `ApiResponse<OnlineRegistrationDetailResponse>` |
| `PUT` | `/{id}/contact` | Ubah No. WA atau email | `OnlineRegistration : Update` | `{ "contact": "0812 1111 2222", "expectedRowVersion": "…" }` | `ApiResponse<OnlineRegistrationDetailResponse>` |
| `POST` | `/{id}/cancel` | Batal dengan alasan | `OnlineRegistration : Cancel` | `{ "reason": "Pendaftaran ganda", "expectedRowVersion": "…" }` | `ApiResponse<OnlineRegistrationDetailResponse>` |

Kode status: `200` berhasil (detail terbaru); `400` isian tidak sah (`KSK-VAL-PO-061`, `062`, `066`, `067`, `070`); `401` tanpa login; `403` tanpa hak; `404` pendaftaran tidak ditemukan; `409` bukan Menunggu Check-in (`060`/`071`), jadwal tidak tersedia (`063`), kuota penuh (`064`), bentrok tanggal (`065`), versi data lama (`072`).

## 5. Verifikasi

Backend dari build saat ini di `https://localhost:7185` terhadap `QuilvianNewDevSukma`. Akun: fixture `TEST-PO009` dari `BE-KSK-PO-009`; policy jabatan `reader` diperbarui menjadi `Read` + `Update` + `Cancel`, jabatan `noaccess` menjadi `Read` saja, keduanya lewat `POST role-access/policies`. Data: pendaftaran `PO-20261120-0001` s.d. `0006` (2026-11-21) dan jadwal uji baru `TEST-PO010` S2, S3 (Poli Paru, dokter lain) serta SX (poli lain) untuk Senin 2026-11-23, semuanya lewat API.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | Build succeeded, 0 error | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 6 berkas (diff sesudah commit `93f0fa1b`), VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | PASS: metadata gap 0, fallback 68 tetap, endpoint telanjang baru 0, identitas wajib 24/24 | `PASS` | Keluaran verifier |
| A1 Akses Role sesudah seeder | Resource "Online Registration" punya aksi `Read`, `Update`, `Cancel` | `PASS` | `po010_test.py` |
| U1–U2 AC 19 pindah ke tanggal lain + dokter lain di poli yang sama | `200`, status tetap 1, versi baru, riwayat `SCHEDULE_CHANGED` (tanggal dan label jadwal lama/baru, pelaku "TEST-PO009 reader"); `doctor-schedule-quotas` jadwal lama 5→4 dan jadwal baru 0→1; kode lama di kiosk menjawab "Check-in hanya dapat dilakukan pada 23 Nov 2026." | `PASS` | `po010_test.py` |
| U3 versi lama | `409` `KSK-VAL-PO-072` | `PASS` | `po010_test.py` |
| U4–U6, U9 validasi | Tanggal dan jadwal sama `400` `066`; poli lain `400` `062`; hari ini dan H+31 `400` `061`; jadwal nonaktif/tanpa kiosk `409` `063` | `PASS` | `po010_test.py` |
| U7 AC 19 jadwal baru penuh | Slot terakhir `200`; berikutnya `409` `064`, pendaftaran tidak berubah (versi, tanggal, riwayat tetap) | `PASS` | `po010_test.py` |
| U8 `KSK-VAL-PO-065` | Pasien lama mendaftar online untuk 23 Nov (`200`), lalu pendaftarannya yang 21 Nov dipindah ke 23 Nov → `409` | `PASS` | `po010_test.py` |
| C1–C4 AC 19 ubah kontak | Email → `0812 1111 2222`: `200`, kontak `081211112222`, riwayat `CONTACT_CHANGED` lama/baru, status email tetap (tidak dikirim ulang); No. WA salah dan kosong `400` (`067`); kontak sama `400`; versi lama `409` | `PASS` | `po010_test.py` |
| X1–X4 AC 20 batal | `200`, status 5 "Dibatalkan", alasan, `closedAt`, riwayat `CANCELLED` dengan alasan, flag aksi `false`; kuota 1→0; kode ditolak kiosk `404` seragam; batal ulang, ubah jadwal, ubah kontak pada status Dibatalkan `409` (`071`/`060`); alasan 3 karakter dan kosong `400` (`070`); pendaftaran uji 23 Nov dibatalkan, kuota kembali 2→1 | `PASS` | `po010_test.py` |
| Z1–Z4 otorisasi | Akun hanya `Read`: ubah jadwal, ubah kontak, batal → `403`, detail `200`, data tidak berubah; tanpa login `401`; id tak dikenal `404` | `PASS` | `po010_test.py` |
| L1 log | 26 nilai sensitif (kontak lama/baru, NIK, kode) tidak ada di log; satu-satunya `ERR` adalah scheduler absensi HR yang tidak terkait | `PASS` | `be7185-po010.log` |
| `KSK-VAL-PO-065` varian kunjungan aktif | Tidak diuji terpisah; memakai helper yang sama dengan `KSK-VAL-PO-024` yang terbukti di runtime `BE-KSK-PO-005` (T5ah) | `NOT RUN` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

Total runtime 33/33 PASS. Run pertama gagal di U1 karena tanggal tujuan uji jatuh hari Minggu dan jadwal uji yang dibuat dengan `practiceDay = 0` tersimpan sebagai Senin; tiga jadwal itu dinonaktifkan dan uji dijalankan ulang dengan Senin 2026-11-23.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 19: pindah tanggal + dokter lain di poli yang sama; status tetap 1; kuota lama +1, baru −1; riwayat lama/baru; kode tetap | Terpenuhi | U1, U2 |
| AC 19: jadwal baru penuh → `409`, pendaftaran tidak berubah | Terpenuhi | U7 |
| `KSK-DSN-PO-016`: jadwal poli lain → `400` `062` | Terpenuhi | U5 |
| AC 19: ubah kontak → riwayat `CONTACT_CHANGED`; email tidak dikirim ulang | Terpenuhi | C1 |
| AC 20: batal dengan alasan → status 5; kode ditolak kiosk; kuota kembali | Terpenuhi | X1, X1b, X2 |
| AC 20: batal tanpa alasan → `400`; batal pada status akhir → `409` | Terpenuhi | X3, X4 |
| `GATE-PO-16`: versi lama → `409` `072` | Terpenuhi | U3, C4 |
| Verifikasi: kuota sebelum/sesudah lewat `doctor-schedule-quotas`; kode lama di kiosk sesudah pindah | Terpenuhi | U1b, U2 |
| DoD: log tanpa kontak | Terpenuhi | L1 |
| `BE-KSK-PO-009` tambahan (1): aksi `Read`, `Update`, `Cancel` di Akses Role | Terpenuhi | A1 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di luar task | Jadwal dokter yang dibuat lewat `POST doctor-schedules` dengan `practiceDay = 0` (Minggu) terbaca kembali sebagai `1` (Senin). Penyebabnya belum ditelusuri; milik modul Master Data jadwal dokter |
| Data uji | Jadwal `TEST-PO010` S2, S3, SX, tiga jadwal Minggu yang salah, dan jadwal `TEST-PO005` FULL sudah dinonaktifkan. Jadwal `TEST-PO005` A **sengaja diaktifkan kembali** untuk uji check-in hari H pada 2026-11-21. Status pendaftaran uji: `0001`, `0005`, `0006` Menunggu Check-in 21 Nov; `0002`, `0003` Menunggu Check-in 23 Nov; `0004` dan pendaftaran pasien lama 23 Nov Dibatalkan. Fixture `TEST-PO009` masih dipakai; bersihkan sesudah modul selesai diuji |
| Risiko tersisa | Pembatalan oleh petugas bersifat final. Hak `Cancel` sebaiknya hanya untuk jabatan admisi senior |
| Perubahan sampingan | `NONE` di repository. Skrip uji dan log di scratchpad |
| Status Git | Milik task ini: 1 berkas baru (`OnlineRegistrationService.Commands.cs`) dan perubahan pada `OnlineRegistrationController.cs`, `OnlineRegistrationDtos.cs`, `OnlineRegistrationService.cs`, `OnlineRegistrationPublicService.cs`, `OnlineRegistrationPublicService.Submit.cs`. Tidak ada commit |
| Langkah berikutnya | `BE-KSK-PO-008` (penutupan Jalur Lain dan Tidak Datang). Pada 2026-11-21: runtime `200` untuk `BE-KSK-PO-006`, lalu `BE-KSK-PO-007`. Frontend `FE-KSK-PO-007`/`008` kini memiliki backend lengkap |
