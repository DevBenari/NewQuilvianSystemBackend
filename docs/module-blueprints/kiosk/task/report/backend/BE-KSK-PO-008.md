# Laporan Perubahan Backend — `BE-KSK-PO-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-008` |
| Judul | Penutupan Jalur Lain dan Tidak Datang |
| Slice | `EPIC KSK-PO-01` — siklus hidup pendaftaran (`FR-KSK-PO-003`, `004`), gelombang teknis 5, rilis `MVP-0` (dirilis bersama `MVP-1`) |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-008` |
| Trace | `KSK-DEC-037`, `045`; `GATE-PO-04`; `KSK-DSN-PO-015`; PO-S.1 baris keempat dan kelima; PO-I.3 |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | `BE-KSK-PO-005` ✅ |
| Klasifikasi | `HIGH` — mengubah transaksi pembuatan kunjungan Rawat Jalan (kunci, hitungan kuota, penutupan) dan menambah layanan latar harian |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Controllers,Services}`, `Program.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `93f0fa1b` (`sukmagp`) |
| Tanggal | 2026-11-20 |
| Status | 🟡 Sebagian — Jalur Lain terbukti di runtime; Tidak Datang terbukti di harness dan layanan harian terbukti berjalan saat aplikasi menyala, tetapi belum ada pendaftaran bertanggal lewat di runtime sampai 2026-11-22 |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` (`OnlineRegistrationService.Closure.cs`, `OnlineRegistrationClosureHostedService`) dan `TOUCHED LEGACY` (`PatientEncounterController.CreateEncounterCoreAsync` dan `ValidateDoctorScheduleQuotaAsync`) |
| QBE yang berlaku | `QBE-SVC-001` (logika penutupan di service; controller hanya memanggil hook), `QBE-LOG-001` (log tanpa kontak dan NIK) |
| Hardcode role | Tidak ada |

## 1. Masalah yang diperbaiki

Dua jenis pendaftaran online akan menggantung selamanya dan terus memakai kuota appointment:

- pendaftaran pasien yang akhirnya didaftarkan lewat kiosk Pasien Lama/Baru atau loket pada tanggal yang sama;
- pendaftaran yang tidak pernah di-check-in sampai tanggalnya lewat.

Task ini menutup keduanya secara otomatis (`KSK-DEC-045`, `037`).

## 2. Proses bisnis

1. **Jalur Lain.** Saat kunjungan Rawat Jalan berpoli dibuat untuk seorang pasien, sistem mencari pendaftaran online Menunggu Check-in milik pasien itu pada tanggal kunjungan yang sama:
   - pasien lama dikenali dari `PatientId`;
   - pasien baru yang belum check-in dikenali dari NIK yang sama persis dengan NIK master pasien.

   Pendaftaran itu langsung berstatus **Didaftarkan lewat Jalur Lain**, ditautkan ke kunjungan, kodenya mati, dan kuotanya lepas. Semua terjadi dalam transaksi pembuatan kunjungan yang sama.
2. **Tanpa kuota ganda.** Ketika kunjungan itu diperiksa kuotanya, pendaftaran online milik pasien itu sendiri tidak ikut dihitung, karena akan ditutup di transaksi yang sama. Contoh: jadwal berkuota 1 sudah "penuh" oleh pendaftaran online Ibu Rina. Petugas tetap bisa mendaftarkan Ibu Rina ke jadwal itu, sedangkan pasien lain ditolak "Kuota pasien pada jadwal dokter sudah penuh."
3. **Tidak Datang.** Setiap hari pukul 00.05 WIB, dan sekali saat aplikasi menyala, pendaftaran Menunggu Check-in yang tanggal kunjungannya sudah lewat berstatus **Tidak Datang**. Kodenya mati, kuotanya lepas, dan riwayatnya tercatat. Menjalankannya ulang tidak menambah riwayat, dan hari yang terlewat saat server mati ikut diproses.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OutpatientPriorityReservationService` (`CloseForEncounterAsync`, `CloseExpiredAsync`), `OutpatientPriorityReservationClosureHostedService`, `PatientMembershipLifecycleHostedService.DelayUntilNextRun`, `PatientEncounterController` (kunci pasien, kunci kuota, hook Reservasi Prioritas, `ValidateDoctorScheduleQuotaAsync`), `DoctorScheduleQuotaService.GetUsagesAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Services/OnlineRegistrationService.Closure.cs` | **Baru** (partial): `BelongsToPatient` (aturan pencocokan tunggal), `LockPatientForEncounterAsync`, `CountClosableOnScheduleAsync`, `CloseForEncounterAsync`, `CloseExpiredAsync` (batch 200) |
| `.../Services/OnlineRegistrationClosureHostedService.cs` | **Baru**: berjalan saat start lalu setiap `Closure:RunAtLocalTime` WIB; dapat dimatikan lewat `Closure:Enabled`; tidak bergantung pada saklar halaman publik |
| `.../Controllers/PatientEncounterController.cs` | Untuk kunjungan Rawat Jalan berpoli: (1) kunci pendaftaran online pasien sesudah kunci kunjungan aktif dan sebelum kunci kuota; (2) hitungan kuota dikurangi pendaftaran online pasien itu sendiri pada jadwal dan tanggal yang sama; (3) `CloseForEncounterAsync` di samping hook Reservasi Prioritas |
| `Program.cs` | `AddHostedService<OnlineRegistrationClosureHostedService>()` |

**Delta terhadap desain:**

| Desain | Source | Alasan |
| --- | --- | --- |
| Penutupan Jalur Lain di samping hook Reservasi Prioritas | Ditambah pengecualian kuota pasien sendiri dan kunci online sebelum kunci kuota | Tanpa pengecualian, pasien yang memegang slot terakhir lewat online ditolak saat didaftarkan petugas (kuota ganda yang dilarang `KSK-DEC-045`). Tanpa urutan kunci itu, kunjungan dan pengiriman online pasien yang sama bisa saling menunggu (kunjungan: kuota → online; pengiriman: online → kuota) |
| Pencocokan pasien baru "lewat NIK master" | NIK sama persis dengan `MstPatient.IdentityNumber`, hanya untuk pendaftaran pasien baru yang belum punya `PatientId` | Menjaga risiko kartu "menutup pendaftaran pasien lain bila pencocokan NIK salah"; dokumen identitas lain tidak dipakai |
| — | Bila (secara teori) ada lebih dari satu pendaftaran tertutup, hanya satu yang ditautkan `EncounterId` | Kolom `EncounterId` unik; aturan satu-per-tanggal membuat kasus ini tidak terjadi dalam praktik |
| — | Riwayat Jalur Lain bersumber `Sistem` dengan pelaku petugas/akun kiosk yang membuat kunjungan | Sama dengan Reservasi Prioritas; menjawab "siapa yang memicu" |
| — | `CloseExpiredAsync` memakai transaksi hanya pada database relasional | Supaya logika yang sama dapat diuji di harness in-memory; perilaku di PostgreSQL tidak berubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. `POST patient-encounters` dan `POST patient-encounters/kiosk` kini menutup pendaftaran online pasien yang sama (perubahan perilaku yang sudah tercatat pada `KSK-CONTRACT-v2`) |
| Database | Menulis status, `EncounterId`, `ClosedAt`, versi, dan riwayat. Tanpa migration |
| Keamanan | Pencocokan NIK sama persis untuk mencegah salah tutup; log tidak memuat kontak maupun NIK |
| Konkurensi | Urutan kunci di semua jalur: kunci kunjungan aktif → kunci pendaftaran online pasien → kunci kuota |

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Konfigurasi:

| Kunci | Bawaan | Arti |
| --- | --- | --- |
| `HealthServices:Registration:OnlineRegistration:Closure:Enabled` | `true` | Menyalakan penutupan harian Tidak Datang |
| `HealthServices:Registration:OnlineRegistration:Closure:RunAtLocalTime` | `00:05` | Jam WIB penutupan harian; nilai tidak sah kembali ke `00:05` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | Build succeeded, 0 error | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 10 berkas (diff `BE-KSK-PO-010` + task ini), VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | PASS: fallback 68 tetap, endpoint telanjang baru 0, identitas wajib 24/24 | `PASS` | Keluaran verifier |
| Harness `harness-po008` (DI asli, EF in-memory, jam palsu 2026-11-25 09:00 WIB) | 11/11 PASS. Tidak Datang: kemarin dan 2 hari lalu → status 4, satu riwayat `NO_SHOW` masing-masing (sumber Sistem, tanpa pelaku), token dihapus; hari ini, besok, dan Dibatalkan tidak tersentuh; kuota tanggal itu kembali 0; dijalankan ulang → 0 ditutup dan riwayat tetap 2; hosted service menutup hari yang tertinggal begitu dinyalakan; `RunAtLocalTime` sah/tidak sah. Pencocokan: `PatientId` dan NIK master sama persis tertutup Jalur Lain (satu tertaut `EncounterId`, riwayat membawa `EncounterId` dan pelaku); NIK sama tanggal lain, NIK lain, pasien lain, pasien baru yang sudah punya `PatientId`, dan NIK kosong tidak tersentuh; pengecualian kuota menghitung 2 milik pasien | `PASS` | Harness scratchpad |
| Runtime 7185: hosted service saat start | Log "Penutupan pendaftaran online selesai: 0 pendaftaran ditutup sebagai Tidak Datang." (belum ada tanggal lewat) | `PASS` | `be7185-po008.log` |
| J1–J3 AC 12 pasien lama | p1 mendaftar online di jadwal S4 berkuota 1 (penuh oleh pendaftaran itu), lalu didaftarkan petugas ke S4 → `200`; pendaftaran status 3 "Didaftarkan lewat Jalur Lain", nomor kunjungan tertaut, riwayat `REGISTERED_ELSEWHERE` (Sistem, pelaku petugas); kuota S4 tetap 1 (kunjungan), bukan 2; kode ditolak kiosk `404` seragam | `PASS` | `po008_test.py` |
| J4 kontras | Pasien lain ke S4 → `400` "Kuota pasien pada jadwal dokter sudah penuh." | `PASS` | `po008_test.py` |
| J5 AC 12 pasien baru | Pendaftaran online pasien baru dengan NIK pasien master `RJPR UJI`, lalu pasien master itu didaftarkan petugas → status 3 | `PASS` | `po008_test.py` |
| J6 | Pendaftaran pasien lain pada jadwal dan tanggal yang sama (`PO-20261120-0002`) tidak berubah (status dan versi) | `PASS` | `po008_test.py` |
| Sesudah kunjungan uji dibatalkan | Kedua pendaftaran tetap status 3 (PO-S.2) | `PASS` | Pembersihan |
| Log | Kontak uji tidak ada di log; satu-satunya `ERR` adalah scheduler absensi HR yang tidak terkait | `PASS` | `be7185-po008.log` |
| AC 11 runtime dengan pendaftaran bertanggal lewat | Belum dapat dijalankan: pendaftaran online paling cepat H+1, sehingga data uji pertama yang lewat tanggal adalah milik 2026-11-21 | `NOT RUN` | Jalankan aplikasi pada/atau sesudah 2026-11-22, lalu periksa status pendaftaran 21 Nov yang tidak di-check-in |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 11: pendaftaran kemarin belum check-in → status 4, riwayat `NO_SHOW`, kuota kembali | **Sebagian** | Harness H1, H1c; runtime menunggu 2026-11-22 |
| AC 11: dijalankan dua kali → riwayat tidak bertambah | Terpenuhi lewat harness | H2 |
| AC 11: server mati dua hari → kedua hari diproses saat menyala | Terpenuhi lewat harness; layanan berjalan saat start di runtime | H1, H3; log runtime |
| AC 12: pasien lama dengan pendaftaran hari itu lalu didaftarkan lewat jalur lain → status 3, `EncounterId` terisi, kode ditolak kiosk | Terpenuhi | J1–J3 (jalur petugas; jalur kiosk Pasien Lama memakai transaksi yang sama `CreateEncounterCoreAsync`) |
| AC 12: pasien baru online lalu didaftarkan di loket dengan NIK sama → status 3 | Terpenuhi | J5 |
| DoD: menjalankan ulang tidak menambah riwayat | Terpenuhi lewat harness | H2 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Yang belum | Runtime Tidak Datang dengan data nyata. Pada 2026-11-22 sesudah 00.05 WIB (atau saat backend uji dinyalakan hari itu), pendaftaran uji 21 Nov yang tidak di-check-in harus berstatus 4. Sesudah terbukti, task ini dapat ditandai ✅ |
| Risiko tersisa | Pasien baru yang NIK-nya berbeda dari NIK master (salah ketik online) tidak tertutup Jalur Lain; pendaftarannya akan menjadi Tidak Datang pada malam harinya dan kuotanya tetap terpakai sampai saat itu |
| Data uji | Jadwal S2 dan S4 dinonaktifkan; dua kunjungan uji tanggal 23 Nov dibatalkan; dua pendaftaran uji tanggal 23 Nov berstatus 3 |
| Perubahan sampingan | `NONE` di repository. Harness, skrip uji, dan log di scratchpad |
| Status Git | Milik task ini: 2 berkas baru (`OnlineRegistrationService.Closure.cs`, `OnlineRegistrationClosureHostedService.cs`) dan perubahan pada `PatientEncounterController.cs` serta `Program.cs`; ditambah perubahan `BE-KSK-PO-010` yang belum di-commit. Tidak ada commit |
| Langkah berikutnya | 2026-11-21: runtime `200` `BE-KSK-PO-006`, lalu `BE-KSK-PO-007` (check-in menjadi kunjungan, memakai hook penutupan di sini sebagai cabang "kunjungan dari check-in"). 2026-11-22: runtime Tidak Datang untuk task ini |
