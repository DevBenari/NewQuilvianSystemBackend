# Laporan Perubahan Backend — `BE-MCU-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-001` |
| Judul | Kunjungan `MedicalCheckup` lewat `EncounterIntakeService` + bukti berjalan |
| Slice | `MVP-0` — prasyarat check-in MCU (`EPIC MCU-03`) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-001`* |
| Trace | `FR-MCU-021` (bagian kunjungan); `MCU-DEC-003`, `023`, `028`; `MCU-COORD-004`; `MCU-UNK-01`, `05`, `06`; `INT-MCU-02`, `INT-MCU-11`; `AT-MCU-RUN-01`..`03` |
| Contract version | `MCU-API-v1` bagian *Perubahan internal pada modul lain* dan `MCU-INT-v1` — keduanya `approved` 2026-10-21. Kontrak publik tidak berubah |
| Dependency | Tidak ada task MCU. Persetujuan pemilik Registrasi ✅ 2026-10-21 (`MCU-EVD-002` bagian 1) |
| Klasifikasi | `MEDIUM` — satu service milik modul lain yang dipakai Laboratorium dan Radiologi; tanpa perubahan schema, tanpa endpoint baru; ada uji tulis devYoga |
| Task mode | `BACKEND` |
| Target tulis | Backend: `Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs`, `.../DTOS/EncounterIntakeDtos.cs`, `Tests/QuilvianSystemBackend.McuTests/`, laporan ini, serta tanda status pada roadmap dan traceability MCU |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4d3e4f98` (branch `yoga`) |
| Tanggal | 2026-10-21 |
| Status | Selesai — kelima acceptance criteria terbukti |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement / Registration`, prefix `Reg` (`ACTIVE / LEGACY`). Pemakai: `MedicalCheckupManagement`, prefix `Mcu` (`ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` (service Registrasi yang sudah ada); proyek uji baru = test scope |
| QBE yang berlaku | `QBE-SVC-001` (logika di service), `QBE-VAL-001` (validasi jenis/klinik/dokter), `QBE-TXN-001` (kunjungan + sumber bayar + antrean dalam satu transaksi), `QBE-CODE-003`/`004` (nomor antrean lewat alokator berkunci + unique index yang sudah ada), `QBE-LOG-001` (log audit dengan aktor), `QBE-DTO-001` (DTO internal, bukan entity) |
| Tidak berlaku | `QBE-MOD-002/003`, `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*` — tidak ada model, configuration, maupun tabel baru |
| Checker QBE | `PASS` — 0 VIOLATION, 0 REVIEW atas dua berkas source yang berubah |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, satu-satunya jalur bagi modul lain untuk membuat kunjungan
(`EncounterIntakeService`) **selalu** membuat kunjungan `Outpatient` tanpa antrean, tanpa
skrining perawat, dan tanpa dokter. Bentuk itu cocok untuk pasien Laboratorium/Radiologi yang
datang langsung ke unit penunjang, tetapi tidak untuk peserta MCU.

**Contoh.** Budi check-in MCU pukul 07.30. Bila MCU memakai jalur lama, kunjungan Budi tercatat
sebagai rawat jalan biasa dan **tidak pernah muncul** di daftar nurse station, sehingga perawat
tidak dapat melakukan skrining awal yang diwajibkan sebelum Pemeriksaan MCU (`MCU-DEC-018`).
Menulis langsung ke tabel kunjungan dari modul MCU dilarang (`AC-45`, kunjungan milik Registrasi).

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Modul MCU (kelak `McuCheckInService`, `BE-MCU-013`) | Memanggil `RegisterAsync` dengan `EncounterType = MedicalCheckup`, klinik MCU, dokter MCU, dan kunci idempotensi `MCU-CHECKIN:<order>` |
| 2 | Registrasi | Memeriksa kunci idempotensi lebih dulu; kirim ulang langsung mengembalikan kunjungan dan antrean yang sama |
| 3 | Registrasi | Memeriksa kewenangan `PatientEncounter : Create` (tidak berubah), pasien aktif, unit layanan aktif |
| 4 | Registrasi | Memvalidasi klinik: wajib bertipe *Medical Check-Up*, aktif, tersedia untuk registrasi, dan berada di unit layanan yang dipilih. Bila klinik mewajibkan dokter, dokter wajib diisi dan aktif |
| 5 | Registrasi | Dalam satu transaksi: kunjungan `MedicalCheckup` + sumber bayar + antrean. Kebutuhan antrean, skrining, dan dokter **dibaca dari master klinik** — sama persis dengan jalur loket |
| 6 | Registrasi | Nomor antrean dari alokator loket (berkunci per tanggal-unit, cadangan nomor prioritas berlaku); kode antrean memakai huruf nama pendek klinik — klinik MCU → `M002` |
| 7 | Sistem | Sesudah commit, layar antrean diberi tahu lewat realtime; kegagalan notifikasi tidak menggagalkan pendaftaran |
| 8 | Perawat | Memanggil, memulai, dan menyelesaikan skrining di nurse station seperti pasien poli lain. Antrean tercatat `ScreeningCompletedAt`, status *Menunggu Dokter* |

**Aturan yang berlaku**

| Kondisi | Hasil |
| --- | --- |
| `EncounterType` tidak diisi (bawaan `Outpatient`) | Perilaku lama, tidak berubah: kunjungan penunjang tanpa antrean, `EncounterDate` = waktu pendaftaran |
| `Outpatient` dengan klinik atau dokter | Ditolak `422` — supaya isian tidak diabaikan diam-diam |
| `MedicalCheckup` | Mengikuti master klinik; `EncounterDate` dan `QueueDate` = tanggal operasional WIB (dibutuhkan penyaring nurse station) |
| Jenis lain (`Emergency`, `Inpatient`, `Telemedicine`, `Unknown`) | Ditolak `422` |

**Jalur tidak normal**

| Kejadian | Hasil |
| --- | --- |
| Klinik kosong, bukan tipe MCU, di unit lain, atau tidak dikenal | `EncounterIntakeValidationException` (`422`), tidak ada data tersimpan |
| Klinik mewajibkan dokter tetapi dokter kosong | Ditolak: "Dokter MCU wajib dipilih …" — tanpa ini nurse station kelak menolak menyelesaikan skrining |
| Pengguna tanpa kewenangan Registrasi | `EncounterIntakeForbiddenException` (`403`), tidak ada data |
| Dua permintaan berkunci sama tiba bersamaan | Unique index memilih pemenang; yang kalah membaca kunjungan + antrean milik pemenang |
| Tabrakan nomor kunjungan/antrean | Transaksi dibatalkan dan dialokasikan ulang, maksimal 5 percobaan, lalu `409` |
| Klinik MCU tanpa kewajiban antrean | Kunjungan tetap `MedicalCheckup` berstatus *Registered*, tanpa antrean |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EncounterIntakeService.cs`, `EncounterIntakeDtos.cs`, `PatientEncounterController.cs`
(`CreateEncounterCoreAsync`, validasi klinik/dokter, `GenerateQueueCode`, `ResolvePatientClassAsync`,
`ResolveTargetEncounterDateAsync`), `NurseStationQueueController.cs` (`start-screening`,
`finish-screening`, penyaring cluster), `OutpatientQueueNumberAllocator.cs`,
`OutpatientQueueClassificationService.cs`, `QueueRealtimeService.cs`, `MstClinic.cs`, `ClinicType.cs`,
`LabPatientRegistrationService.cs` (pemanggil lama), `Program.cs` (registrasi DI), harness uji
`PatientManagementTests`. Dokumen: roadmap, `MCU-API-v1`, `MCU-INT-v1`, `02-backend-architecture.md`
§7 dan §9, `00-interview-decisions.md`, `01-existing-capability-map.md`, matriks uji §6.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/DTOS/EncounterIntakeDtos.cs` | `EncounterIntakeRequest`: ruas `EncounterType` (bawaan `Outpatient`), `ClinicId`, `DoctorId`. `EncounterIntakeResult`: `QueueId`, `QueueCode` |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs` | `ResolveVisitProfileAsync` (validasi jenis/klinik/dokter), `BuildQueueAsync` (antrean memakai klasifikasi + alokator loket), `NotifyQueueCreatedAsync`, `FindQueueAsync` (jawaban kirim ulang memuat antrean), salinan `GenerateQueueCode` loket; konstruktor menerima `OutpatientQueueClassificationService`, `OutpatientQueueNumberAllocator`, `QueueRealtimeService?` (semuanya sudah terdaftar di DI). Log audit kini menulis nomor kunjungan, jenis, dan antrean di teks pesan, dan payload tidak lagi memakai ruas `Id` yang menimpa pelaku log |
| `Tests/QuilvianSystemBackend.McuTests/` (baru, diabaikan `.gitignore` — perlu `git add -f`) | Proyek uji xUnit + SQLite: `Infrastructure/TestDatabase.cs`, `Infrastructure/EncounterIntakeHarness.cs`, `EncounterIntakeMedicalCheckupTests.cs` (17 kasus) |
| `docs/module-blueprints/mcu/task/report/backend/BE-MCU-001.md` | Laporan ini |
| `docs/module-blueprints/mcu/roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md` | Tanda status dan tautan bukti |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint HTTP yang berubah. Kontrak internal `INT-05`/`INT-MCU-02` bertambah ruas opsional; pemanggil lama (Lab, Rad) tidak perlu diubah |
| Database | Tidak ada perubahan schema maupun migration. Data uji ditulis ke devYoga atas izin pemilik sesi ini (lihat §5) |
| Keamanan/Auth | Penjaga `HasRegistrationAuthorityAsync` (`PatientEncounter : Create`) tidak berubah dan tetap berlaku untuk jalur MCU — akun admisi MCU wajib memegang izin itu (`MCU-INT-v1`). Log audit kini mencatat pelaku yang benar |

### 3.4 Delta terhadap kontrak

| Kontrak | Isi kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| `MCU-INT-v1` `INT-MCU-02` | Request mendapat ruas *jenis kunjungan dan klinik* | Ditambah juga `DoctorId` | Klinik MCU devYoga ber-`IsDoctorRequired = true`, dan `finish-screening` menolak antrean tanpa dokter tujuan. Kontrak check-in sudah mewajibkan `mcuDoctorId`, jadi ruas ini hanya meneruskannya |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini mengubah service internal, bukan endpoint. Endpoint nurse station yang
dipakai pada uji berjalan sudah ada dan tidak diubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Berhasil, 0 error, 80 detik | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 17 lulus, 0 gagal | `PASS` | `Passed! - Failed: 0, Passed: 17` |
| Checker QBE `-Path` atas dua berkas source | 0 VIOLATION, 0 REVIEW | `PASS` | `Final result: PASS` |
| `AT-MCU-RUN-03` — kueri baca-saja devYoga | `CLN-MCU-001` "Klinik Medical Check Up", tipe 7, `IsQueueRequired`/`IsScreeningRequired`/`IsDoctorRequired` = true, unit `SU-OPD-MCU-001`; anggota cluster nurse station aktif `5cda472b…` | `PASS` | Sesi Npgsql `READ ONLY` |
| `AT-MCU-RUN-01` — `RegisterAsync` produksi terhadap devYoga | `ENC-RSMMC-00183` jenis `MedicalCheckup` (4), status *Menunggu Perawat*, satu sumber bayar Tunai, antrean `M002` di `CLN-MCU-001`, cakupan nomor = cluster `5cda472b…`; kirim ulang → kunjungan yang sama, `IsReplay = true`, jumlah kunjungan berkunci itu = 1 | `PASS` | Keluaran konsol + kueri baca-saja |
| `AT-MCU-RUN-02` — nurse station lewat API backend lokal (superadmin) | Antrean muncul di `GET nurse-station-queues`; `start-screening` 200; `finish-screening` 200; `ScreeningCompletedAt` terisi, antrean *Menunggu Dokter* (4), kunjungan *Menunggu Dokter* (5) | `PASS` | Respons API + kueri baca-saja |

Uji manual: `PASS` (lewat API, bukan layar).

**Uji yang ditulis (harness SQLite, tanpa basis data bersama):** kunjungan MCU + antrean beserta
semua ruasnya; nomor antrean berurutan (`M002`, `M004` — nomor 1/3/5/10/15 dicadangkan untuk
prioritas, sama seperti loket); kirim ulang; klinik MCU tanpa antrean; regresi `Outpatient`
(jenis, sumber, status, tanpa klinik/dokter/antrean, tanggal = waktu pendaftaran); `Outpatient`
dengan klinik/dokter ditolak; empat jenis kunjungan lain ditolak; empat bentuk klinik tidak sah;
dokter kosong dan dokter tidak dikenal; tanpa kewenangan.

**Tidak dijalankan:**
- Kewenangan nyata pada `AT-MCU-RUN-01`. Konsol uji mengganti seam `HasRegistrationAuthorityAsync`,
  karena belum ada endpoint MCU yang memanggil service ini. Kode kewenangannya tidak berubah;
  pembuktian dengan akun admisi MCU dilakukan di `BE-MCU-013`.
- Seluruh suite uji modul lain. Pemanggil lama dibuktikan lewat uji regresi `Outpatient` dan build.

**Data uji yang tertinggal di devYoga (atas izin):** pasien `UAT FECRM PASIEN IGD`
(RM 00-00-00-17), kunjungan `ENC-RSMMC-00183` (*Menunggu Dokter*, dokter dr. Bima Prasetya), antrean
`M002` tanggal 2026-10-21, kunci idempotensi `BE-MCU-001:AT-MCU-RUN-01:2026-10-21`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Pemanggilan dengan jenis `MedicalCheckup` dan klinik MCU membuat tepat satu kunjungan `MedicalCheckup` beserta antrean skrining | Terpenuhi | Uji `Mcu_membuat_tepat_satu_kunjungan_…`, `Kirim_ulang_…`; `AT-MCU-RUN-01` |
| (2) Pemanggil lama (`Outpatient`) tidak berubah perilaku | Terpenuhi | Uji `Regresi_Outpatient_…`; ruas baru opsional, nilai bawaan `Outpatient` |
| (3) `AT-MCU-RUN-03`: klinik/unit MCU ada dan `IsQueueRequired = true` | Terpenuhi | Kueri baca-saja §5 |
| (4) `AT-MCU-RUN-01`: satu kunjungan uji di devYoga | Terpenuhi | `ENC-RSMMC-00183` / `M002` |
| (5) `AT-MCU-RUN-02`: skrining selesai lewat nurse station dan statusnya terbaca dari antrean/kunjungan | Terpenuhi | `ScreeningCompletedAt` terisi; antrean dan kunjungan *Menunggu Dokter* |
| DoD: hasil RUN dicatat; `MCU-UNK-01`/`05`/`06` dapat ditutup | Terpenuhi | §5; ketiga unknown terjawab dengan bukti berjalan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Harness SQLite baru juga melepas check constraint regex PostgreSQL (` ~ `, modul Finance), yang membuat `EnsureCreated` gagal di SQLite. Harness `PatientManagementTests` belum melepasnya dan kemungkinan ikut gagal sejak constraint itu masuk — di luar scope, tidak diubah |
| Masalah yang diketahui | **Untuk `BE-MCU-013` (check-in), bukan cacat task ini:** (a) `RegisterAsync` membuka transaksinya sendiri, padahal `INT-MCU-02` memintanya berjalan **di dalam** transaksi check-in. Perlu dukungan transaksi pemanggil, termasuk menunda notifikasi realtime sampai commit luar. (b) `CompanyGuarantor` masih ditolak jalur ini, padahal `INT-MCU-02` memakai penjamin perusahaan untuk peserta korporat. (c) Kunjungan tercatat `RegistrationSource = WalkIn`/`IsWalkIn = true` dan kelas pasien *RAWAT JALAN* — perlu dipastikan untuk peserta booking dan tarif paket. Ketiganya perubahan service Registrasi lagi |
| Risiko tersisa | Sesudah skrining, kunjungan MCU berstatus *Menunggu Dokter* dan masuk daftar antrean dokter (dokter MCU). Bila dokter menyelesaikan konsultasi dari layar antrean dokter rawat jalan, status kunjungan bisa bergerak di luar alur MCU. Perlu disikapi di `BE-MCU-023`/`030` (`MCU-COORD-004`). Notifikasi realtime pada jalur balapan kunci-sama bisa terkirim dua kali — hanya kosmetik |
| Perubahan sampingan | `NONE`. Build menulis ulang keluaran `bin/obj` saja |
| Interupsi | `NONE` |
| Status Git | ` M Areas/HealthServices/RegistrationManagement/DTOS/EncounterIntakeDtos.cs`, ` M Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs`, `?? docs/module-blueprints/mcu/` (seluruh blueprint MCU belum di-commit), `!! Tests/QuilvianSystemBackend.McuTests/` (perlu `git add -f`). Perubahan lain di working tree milik pekerjaan sebelumnya dan tidak disentuh |
| Langkah berikutnya | `BE-MCU-002` (skema 21 tabel MCU). Butir (a)–(c) di atas dimasukkan ke cakupan `BE-MCU-013` atau task Registrasi tersendiri, dengan persetujuan pemilik Registrasi |
