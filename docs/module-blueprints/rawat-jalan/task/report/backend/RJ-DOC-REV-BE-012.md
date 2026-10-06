# Laporan Perubahan Backend — `RJ-DOC-REV-BE-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-012` |
| Judul | Endpoint konsultasi tertunda dan bunyi petunjuk |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `13.1` |
| Trace | `RJ-DOC-DEC-029`, `030`, `031`, `RJ-DOC-FE-012`; menjawab `RJ-DOC-OQ-012` |
| Kontrak | `RJ-DOC-PENDCONS-001@1.0.0` (`approved`, `RJ-DOC-DEC-032`) |
| Desain | `02-backend-architecture.md` *Amendment KT* (KT.3.1–KT.3.5, KT.5, KT.7, KT.10) |
| Wewenang | `RJ-DOC-DEC-033`; uji runtime penuh, satu UPDATE `QueueDate` per antrean uji, dan pencopotan sementara role SuperAdmin akun uji, masing-masing disetujui pemilik pada sesi 5 Okt 2026 |
| Task mode | `BACKEND` |
| Klasifikasi | `MEDIUM` (1 repo, ±14 berkas diperiksa, 3 berkas diubah, kontrak API aditif, tanpa schema) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ SELESAI |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (prefix `Reg`, registry `ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — `DoctorQueueController.cs`, `DoctorQueueDtos.cs`, `OutpatientEncounterListService.cs` |
| Branch | `sukmagp` (sama dengan task `RJ-DOC-REV-*` sebelumnya). Tanpa switch, commit, atau push |
| QBE yang berlaku | Conformance Strict pada berkas yang disentuh; `QBE-PERM-001` (`[AccessAction]` + `[AccessPermission]` serasi), `QBE-DTO-001` (DTO, bukan entity), `QBE-PAGE-001` (paging mapan). `QBE-SVC-001` tidak dipaksakan: query di controller mengikuti pola `DoctorQueueController` yang ada (utang teknis, `02` KT.6/KT.13) |
| Hak akses | Tidak ada butir baru. `GET pending-consultations` memakai `DoctorQueue : Read`. Tidak ada hardcode role baru; jalur `IsCurrentUserSuperAdminAsync` existing dipakai ulang apa adanya |
| Migration / DB schema | Tidak ada |

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs` | (1) Endpoint `GET pending-consultations` (`doctorId`, `queueId`, `search`, `pageNumber`, `pageSize`), urut `QueueDate` lalu `QueueNumber` menaik. (2) `BuildPendingConsultationQuery` dengan syarat KT.3.1, memakai `WhereOutpatientClinicEncounter`. (3) `BuildQueueDisplayQuery` — rantai `Include` yang sebelumnya ada di `BuildQueueBaseQuery`, kini dipakai berdua. (4) `ApplySearchFilter` dipisah dari `ApplyStandardFilter` tanpa perubahan isi. (5) `MapResponsesAsync<TResponse>` / `MapResponse<TResponse>` generik; overload lama tetap mengembalikan `DoctorQueueResponse`. (6) Hitungan `draftPrescriptionCount` (`PrescriptionStatus.Draft`), `procedureCount`, `pendingDays`, dan penanda `canCancelConsultation` lewat `AccessPermissionService.HasAccessAsync(User, "DoctorConsultation", "Cancel")`. (7) `AccessPermissionService` disuntik lewat konstruktor (sudah terdaftar `Program.cs:369`) |
| `Areas/HealthServices/RegistrationManagement/DTOS/DoctorQueueDtos.cs` | `DoctorPendingConsultationResponse : DoctorQueueResponse` dengan empat field KT.3.3 |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterListService.cs` | Bunyi baru `RJDP-VAL-005` di `GetCancelBlockedReason` (KT.3.4). Kondisi tidak berubah |

Diff: 3 berkas, `+217 / -12`.

## 3. Validasi

| Perintah / bukti | Hasil | Status |
| --- | --- | --- |
| `dotnet build -c Release --no-incremental` sebelum perubahan (baseline) | `0 Error(s)`, `244 Warning(s)` | — |
| `dotnet build -c Release --no-incremental` sesudah perubahan | `0 Error(s)`, `244 Warning(s)` — tanpa warning baru | `PASS` |
| QBE Strict × 3 berkas, sebelum dan sesudah | Setiap berkas `VIOLATION 0`, `REVIEW 0`, `PASS` | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build --configuration Release` | "No changes have been made to the model since the last migration." | `PASS` |
| `dotnet build` Debug | Kompilasi berhasil; salin `bin/Debug/.../QuilvianSystemBackend.exe` gagal (`MSB3021`) karena dikunci server dev pemilik (PID 12160, port 7184). Bukan galat kode | Catatan |
| `AUTOMATED TEST` | `NOT APPLICABLE` — pola Bank Darah, tanpa project test | — |

### Uji runtime

Lingkungan: build Release dari scratchpad, `Development`, `Runtime__Role=Web`, `http://localhost:5199`,
DB `QuilvianNewDevSukma`. Run kedua memakai `HealthServices__Registration__BlockActiveEncounter=true`.
Login SuperAdmin seed dan akun uji `uji.rjdp.dokter` / `uji.rjdp.tanpacakupan`. Kredensial tidak
dicetak. Pembanding dihitung dengan SQL baca-saja. Log: `rt_be012_*.log` (scratchpad).

| ID | AT | Skenario | Hasil | Status |
| --- | --- | --- | --- | --- |
| R0–R1 | — | SuperAdmin, semua dokter | `200`, `totalData 9` = SQL 9; urut tanggal menaik | `PASS` |
| R2–R6 | — | ENC-RSMMC-00172 (dr. Arif, 30 Sep): field, `pendingDays 5`; `doctorId` dr. Arif → 2; `queueId` → 1 baris; `search` → 1; paging `pageSize 2` halaman 2 → 2 baris, `totalPage 5` | Sesuai | `PASS` |
| R8 | — | `GET /doctor-queues` hari ini tidak memuat antrean lampau | `totalData 0` | `PASS` |
| R9b | `AT-KT-06` | Hitungan resep draf dan tindakan untuk 9 konsultasi tertunda nyata dibanding SQL | `9/9` cocok (contoh 00117: 1 resep, 1 tindakan) | `PASS` |
| R10 | `AT-KT-03` | Data nyata yang wajib dikecualikan: 3 antrean lampau `WaitingForDoctor`, 15 kunjungan lampau status 7, 1 antrean `InConsultation` dengan konsultasi batal saja | Tidak satu pun muncul | `PASS` |
| C1–C5 | `AT-KT-09` | `GET /doctor-queues` (2 varian), `/summary` (2 varian), `/call-lock` di app baru (5199) dibanding server lama pemilik (7184), DB sama | Kelima respons identik | `PASS` |
| S1 | `AT-KT-02` | Kunjungan uji A (`ENC-RSMMC-00200`) Sedang Konsultasi hari ini | Tidak di daftar tertunda; ada di antrean hari ini | `PASS` |
| S2–S3 | `AT-KT-01` | Sesudah `QueueDate` A dimundurkan ke kemarin | Muncul bagi dokter uji, `pendingDays 1` | `PASS` |
| S4 | `AT-KT-04` | Dokter uji (role SuperAdmin dicopot) | Hanya `ENC-RSMMC-00200`; 00172 milik dr. Arif tidak terlihat | `PASS` |
| S5 | `AT-KT-04` | Dokter uji mengirim `doctorId` dr. Arif | `403`, sama dengan `GET /doctor-queues` lama | `PASS` |
| S6 | `AT-KT-05` | Akun tanpa data dokter | `403` | `PASS` |
| S7 | — | Tanpa login | `401` | `PASS` |
| S8 | — | SuperAdmin sesudah data uji | `totalData 10` | `PASS` |
| S9 | `AT-KT-10` | Batal kunjungan A (status 6, konsultasi aktif) | `400` dengan bunyi baru `RJDP-VAL-005` | `PASS` |
| S10 | `AT-KT-10` | Petunjuk baris di `GET /outpatient-encounters` | `cancelBlockedReason` = bunyi baru | `PASS` |
| T0 | — | Saklar blokir aktif: daftar pasien uji lagi | `400` menyebut ENC-RSMMC-00200 | `PASS` |
| T1–T3 | `AT-KT-07` | Dokter uji mengisi SOAP + diagnosis utama, lalu `finish-consultation` | `200`; antrean 10, kunjungan 7, konsultasi 2; baris hilang | `PASS` |
| T4 | `AT-KT-07` | Daftar pasien uji lagi dengan saklar aktif | `200` (`ENC-RSMMC-00201`) | `PASS` |
| U1 | `AT-KT-01` | Kunjungan B dimundurkan dua hari | Muncul, `pendingDays 2` | `PASS` |
| U2–U4 | `AT-KT-08` | Dokter uji membatalkan konsultasi B | `200`; antrean dan kunjungan tetap 6; baris hilang | `PASS` |
| U5 | `AT-KT-08` | SuperAdmin membatalkan kunjungan B lewat `PATCH /outpatient-encounters/{id}/cancel` | `200` | `PASS` |

Ringkasan: seluruh skenario `PASS`. Run pertama S4/S5 `FAIL` karena akun `uji.rjdp.dokter` ternyata
memegang role SuperAdmin, sehingga diperlakukan sebagai super admin (endpoint lama
`GET /doctor-queues` berperilaku sama). Dengan izin pemilik, baris role itu dicopot sementara lewat
SQL, lalu S4/S5 diulang dan `PASS`. Satu UPDATE pertama untuk B ditolak unique index
`IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_QueueCode` (kode `B001` sudah dipakai A di tanggal
kemarin) dan tidak mengubah apa pun; B lalu dimundurkan dua hari.

### Jawaban pemeriksaan wajib

| Butir | Hasil |
| --- | --- |
| `RJ-DOC-OQ-012` — kunjungan status 6 dengan konsultasi aktif yang antreannya bukan `InConsultation` | **0** di `QuilvianNewDevSukma` (5 Okt 2026). Tidak ada kunjungan yang lolos dari daftar |
| KT.10 — jabatan dokter uji memegang `DoctorConsultation : Cancel` | **Ya** — `canCancelConsultation true` untuk akun tanpa role SuperAdmin |

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| `AT-KT-01` muncul bagi dokter pemilik | Terbukti | S2, U1 |
| `AT-KT-02` antrean hari ini tidak muncul | Terbukti | S1 |
| `AT-KT-03` tiga kasus pengecualian | Terbukti | R10 |
| `AT-KT-04` cakupan dokter | Terbukti | S4, S5 |
| `AT-KT-05` tanpa data dokter → `403` | Terbukti | S6 |
| `AT-KT-06` hitungan dan `queueId` | Terbukti | R4, R9b |
| `AT-KT-07` Simpan, kunjungan 7, pasien dapat daftar lagi | Terbukti | T1–T4 |
| `AT-KT-08` Batal konsultasi lalu batal kunjungan | Terbukti | U2–U5 |
| `AT-KT-09` endpoint lama tidak berubah | Terbukti | C1–C5 |
| `AT-KT-10` bunyi baru `RJDP-VAL-005` | Terbukti | S9, S10 |

## 5. Delta terhadap kontrak

| Butir | Kontrak | Perilaku | Penanganan |
| --- | --- | --- | --- |
| `doctorId` dokter lain oleh dokter biasa | `api-contract.md` *Amendment KT*: "dokter lain diabaikan, tidak melebarkan hasil" | `403` (`Forbid()`), persis seperti `GET /doctor-queues`, karena `ResolveAllowedDoctorIdAsync` mengembalikan `null` | Tidak melebarkan hasil dan sesuai `02` KT.3.2 ("cakupan sama persis"). Bunyi kontrak perlu dikoreksi pemilik pada revisi berikutnya; kode tidak diubah |

## 6. Data uji dan keadaan sesudah uji

| Data | Keadaan |
| --- | --- |
| `ENC-RSMMC-00200` (pasien uji `KSKTEST-RM-07`, dokter uji, `QueueDate` dimundurkan ke 4 Okt) | Konsultasi selesai, kunjungan status 7. Diagnosis R10 dan SOAP bertanda `UJI-RJKT-BE012`. Tidak lagi memblokir pendaftaran |
| `ENC-RSMMC-00201` (`QueueDate` dimundurkan ke 3 Okt) | Konsultasi `Cancelled`, kunjungan `IsCancel = true` |
| Data nyata (termasuk ENC-RSMMC-00172) | Hanya dibaca; tidak berubah |
| Dokter uji dan pegawai `UJI-RJDP Tanpa Cakupan` | Dipakai uji layar `FE-012`, lalu dinonaktifkan dan bypass lokasi dimatikan (5 Okt 2026); login ditolak `401` |
| Role SuperAdmin akun `uji.rjdp.dokter` | Dikembalikan sesudah `FE-012` (baris `AspNetUserRoles` yang sama); akun kembali memegang SuperAdmin + Supervisor |

Rincian pemulihan ada di laporan [RJ-DOC-REV-FE-012](../frontend/RJ-DOC-REV-FE-012.md) §5.

## 7. Risiko tersisa

1. **Akun uji dokter memegang role SuperAdmin** sebelum task ini. Akibatnya uji cakupan dokter
   pada task lain yang memakai akun ini (termasuk `RJ-DOC-REV-BE-009`) berjalan sebagai super admin.
   Pemilik perlu memutuskan apakah role itu memang dimaksudkan.
2. **Endpoint geolocation bypass** (`PATCH …/user-account/geolocation-bypass`) gagal `500` bila
   `geolocationBypassUntil` dikirim tanpa zona waktu (`Kind=Unspecified` ke `timestamptz`). Bug lama,
   di luar scope; dicatat untuk task pemiliknya.
3. **Server dev pemilik di port 7184 perlu di-restart** untuk memuat endpoint baru.
4. `RJ-DOC-OQ-014` dan `RJ-DOC-OQ-015` tetap terbuka sesuai roadmap.

## 8. Task berikutnya

`RJ-DOC-REV-FE-012` — dependency `[BE] RJ-DOC-REV-BE-012` terpenuhi.
