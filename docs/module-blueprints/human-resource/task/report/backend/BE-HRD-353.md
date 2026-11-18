# Laporan Perubahan Backend — `BE-HRD-353`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-353` |
| Judul | Dispensasi: pengajuan, catat atas nama, keputusan (`POST-MVP`) |
| Slice | Amandemen `A52` — `EPIC HRD-116`, gelombang `POST-MVP` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B88 |
| Trace | `FR-PK-023`, `FR-PK-024`; `HRD-DEC-200`, `HRD-ASM-76`; `02-backend-architecture.md` 35.3.3, 35.4.2–35.4.3, 35.9 (klik ganda); `api-contract.md` "Attendance Dispensation" dan "Self Services / Attendance Dispensation"; `state-transition-matrix.md` 30.3; `validation-matrix.md` `VR-PK-13` s.d. `VR-PK-17`; `permission-audit-matrix.md` 30.1, 30.2, 30.5; `integration-contract.md` `INT-HR-105`; `AT-HRD-PK-33`, `AT-HRD-PK-34` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-343` ✅ |
| Klasifikasi | `HEAVY` — skor 9: satu repository (0), sekitar 25 berkas diperiksa (2), 4 berkas source baru + `Program.cs` (1), alur status dengan konkurensi (2), hak akses, cakupan, dan penyetuju dari server (2), data sensitif alasan (1), pemberitahuan lintas modul (1), tanpa schema (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services/AttendanceDispensationService.cs,Controllers/AttendanceDispensationController.cs,DTOs/AttendanceDispensationDtos.cs}`, `Areas/SelfServices/HumanResource/Controllers/AttendanceDispensationSelfServiceController.cs`, `Program.cs` (satu registrasi DI); proyek uji HR (`AttendanceDispensationTests.cs` baru, `A52PermissionRegistryTests.cs`, `AcceptanceTestIndex.md`); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`) + perubahan `BE-HRD-357` yang belum di-commit |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — proyek uji HR 209/209 (termasuk kontrol negatif); `AT-HRD-PK-33`, `AT-HRD-PK-34`, `VR-PK-13`, `-14`, `-16`, `-17`, alur atas nama → setuju → pemberitahuan lulus lewat API devYoga dengan superadmin dan akun non-superadmin |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` (dispensasi korporat) dan `SelfServices` (layanan mandiri) |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); entity `HrdAttendanceDispensation` sudah ada (`BE-HRD-334`), tidak ada entity baru |
| Keberlakuan | `NEW CODE` (service, dua controller, DTO) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-LOG-001` (audit tanpa alasan), `QBE-TXN-001` (satu `SaveChanges` per transisi, token konkurensi) |
| Tidak berlaku | `QBE-CODE-*` (tidak ada nomor dokumen), `QBE-DB-*` (tanpa schema), `QBE-NAM-003` (bukan migrasi legacy) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Konsep dispensasi belum ada di kode (`HRD-TF-065`); tabel `HrdAttendanceDispensation` sudah ada tanpa API | Pegawai yang terlambat karena tugas (mis. mengantar rujukan) tidak punya jalur pembebasan menit terlambat |
| Pola izin pulang cepat menerima ID penyetuju dari klien (`EarlyLeavePermissionService.cs:350`) | Tidak ditiru: penyetuju dispensasi ditetapkan server |

---

## 2. Proses bisnis

**Pengajuan (`HRD-ASM-76` a).** Pegawai mengajukan untuk dirinya lewat layanan mandiri (pegawai diambil dari sesi), atau Kepala Unit
mencatat atas nama pegawai unit yang ia kelola. Isinya tanggal kerja, jenis (`LateArrival` datang terlambat / `EarlyDeparture`
pulang lebih awal), jam mulai–selesai, dan alasan wajib. Dispensasi bukan untuk tidak masuk sehari penuh (`HRD-DEC-200` butir 5).

**Penyetuju ditetapkan server (`HRD-ASM-76` c, `VR-PK-16`, `VR-PK-17`).** Penyetuju = atasan langsung pegawai dari `WfpManagerAssignment`
yang aktif pada tanggal itu dan boleh menyetujui (`CanApproveRequests`), diutamakan atasan utama bertipe `Direct`. Permintaan tidak
punya kolom penyetuju; kolom yang disisipkan klien diabaikan. Pegawai tidak pernah menjadi penyetujunya sendiri, jadi dispensasi Kepala
Unit diputuskan atasannya. Tanpa atasan langsung → `422` "Atasan langsung Anda belum ditetapkan. Hubungi HRD".

**Batas waktu (`VR-PK-14`, `VR-PK-15`).** Tanggal paling lama `MaxBackdatedCorrectionDays` hari ke belakang (setelan kehadiran, saat ini 3)
→ `422`. Tanggal dalam periode kehadiran `Closed` (periode tanpa unit atau periode unit pegawai) → `403` "Periode kehadiran Februari 2027
sudah ditutup", untuk pengajuan maupun setiap tindakan sesudahnya.

**Status (state 30.3).** `Submitted` → `Approved`/`Rejected` (hanya penyetuju; tolak wajib alasan) atau `Cancelled` (hanya pengaju).
Status akhir tidak dapat diubah (`409`). Setiap perubahan mengganti `RowVersion`; token basi → `409`, termasuk klik ganda.

**Pemberitahuan (`INT-HR-105`).** Keputusan dikirim ke pengaju sebagai lonceng dalam aplikasi. Kegagalan pemberitahuan tidak
membatalkan keputusan. Log mesin tidak disentuh (`INV-A52-07`); pengurangan menit terlambat adalah `BE-HRD-354`.

**Contoh (uji harness, `HRD-DEC-200`).** Budi mengajukan dispensasi 3 Februari 07.00–07.40 "mengantar rujukan pasien"; server menetapkan
Sari (Kepala Unit ICU) sebagai penyetuju; Sari menyetujui; Budi menerima "Dispensasi Anda disetujui". Sari mengajukan untuk dirinya →
penyetuju Dodi (Kepala Bidang); Sari mencoba menyetujui → `403`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-200`, `HRD-ASM-76`; `02-backend-architecture.md` 35.3.3, 35.4.1–35.4.3, 35.9; kontrak API dua bagian dispensasi; `state-transition-matrix.md` 30.3; `validation-matrix.md` `VR-PK-13`–`17`; `permission-audit-matrix.md` 30.1, 30.2, 30.5; `integration-contract.md` `INT-HR-102`, `INT-HR-105`; `data-dictionary.md` `HrdAttendanceDispensation`; `acceptance-test-matrix.md` `AT-HRD-PK-32`–`34`
- Model `HrdAttendanceDispensation` + konfigurasi EF (CHECK `EndAt > StartAt`, `RowVersion` token konkurensi), `WfpManagerAssignment` (CHECK `CK_WfpManagerAssignment_NotSelf`), `HrdAttendanceSetting`, `HrdAttendancePeriod`, `HrdNotification`
- `AttendanceSettingService` (`GetEffectiveMaxBackdatedCorrectionDaysAsync`), `AttendanceCorrectionOnBehalfService` (pola atas nama dan periode ditutup), `OnCallCallService` (pola transisi + `RowVersion`), `ShiftReplacementService` (pola pemberitahuan), `UnifiedNotificationGatewayService`/`HrdNotificationService`, `HrdUnitScopeService`, `MyScheduleSelfServiceController`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceDispensationService.cs` | **Baru.** Ajukan (mandiri/atas nama), setujui, tolak, batalkan, daftar korporat, detail, daftar milik sendiri, metadata; penyetuju dari `WfpManagerAssignment`; unit pegawai dari penempatan yang berlaku pada tanggal itu; periode ditutup; `RowVersion`; pemberitahuan ke pengaju |
| `Areas/Corporate/HumanResource/AttendanceManagement/Controllers/AttendanceDispensationController.cs` | **Baru.** `GET /filters/metadata`, `GET /`, `GET /{id}`, `POST /`, `POST /{id}/approve`, `POST /{id}/reject`; `AttendanceDispensation : Read/Create/Approve`; audit `AttendanceDispensation.Create/Approve/Reject` |
| `Areas/SelfServices/HumanResource/Controllers/AttendanceDispensationSelfServiceController.cs` | **Baru.** `GET /`, `POST /`, `POST /{id}/cancel`; `MyAttendanceDispensation : Read/Create/Cancel`; audit `MyAttendanceDispensation.Create/Cancel` |
| `Areas/Corporate/HumanResource/AttendanceManagement/DTOs/AttendanceDispensationDtos.cs` | **Baru.** `CreateMyDispensationRequest`, `CreateDispensationRequest` (tanpa kolom penyetuju), `DispensationDecisionRequest`, `RejectDispensationRequest`, query, `DispensationResponse` (+ `CanDecide`, `CanCancel`), metadata |
| `Program.cs` | Satu baris `AddScoped<AttendanceDispensationService>()` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AttendanceDispensationTests.cs` | **Baru.** 8 uji |
| `Tests/QuilvianSystemBackend.HumanResourceTests/A52PermissionRegistryTests.cs` | Dua controller dispensasi masuk validator registri dan uji 403; enam resource/aksi masuk daftar |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AcceptanceTestIndex.md` | `AT-HRD-PK-33`, `-34` dipetakan ke uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sembilan endpoint sesuai kontrak `v26`; respons `DispensationResponse` (nama pegawai, unit, penyetuju, label, `RowVersion`, `CanDecide`, `CanCancel`) |
| Database | Tanpa migration; menulis ke tabel `HrdAttendanceDispensation` yang sudah ada |
| Keamanan/Auth | Resource baru `AttendanceDispensation : Read/Create/Approve` (modul `HUMAN_RESOURCE_ATTENDANCE`) dan `MyAttendanceDispensation : Read/Create/Cancel` (modul `HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE`), terdaftar saat startup, belum diberikan ke jabatan mana pun. Aksi `Approve` bertipe akses `Update` (repository tidak punya `AccessTypes.Approve`; pola sama dengan aksi `Cancel` lain) |
| Privasi | `Reason` (sensitif) hanya di respons untuk pembaca berwenang; tidak masuk log audit maupun teks pemberitahuan |

### 3.4 Delta dan keputusan desain

| Butir | Implementasi | Alasan |
| --- | --- | --- |
| Pesan `VR-PK-16` saat dicatat atas nama | "Atasan langsung pegawai ini belum ditetapkan. Hubungi HRD" | Pesan kontrak ditujukan ke pegawai ("Anda"); Kepala Unit yang mencatat bukan pegawai itu |
| Rentang jam | Jam mulai pada tanggal kerja atau sehari sesudahnya (shift malam), rentang ≤ 24 jam; selain itu `400` | Dispensasi untuk "satu tanggal dan rentang jam" (`HRD-DEC-200` butir 1); kontrak tidak merinci |
| Dispensasi bertumpuk | `409` bila rentang bertumpuk dengan dispensasi `Submitted`/`Approved` pegawai yang sama pada tanggal itu. **Dipersempit di `BE-HRD-354`** menjadi hanya dispensasi sejenis | Mencegah menit dihapus ganda di `BE-HRD-354`; pemrosesan memisahkan jenis |
| Jam tanpa zona | Dibaca sebagai WIB, disimpan UTC | Sama dengan `AppDateTimeHelper.ToUtc` |
| Unit dispensasi | Unit penempatan pegawai (utama didahulukan) yang berlaku pada tanggal dispensasi | Tabel tidak menyimpan unit; desain 35.9 membaca lewat `(WorkforceProfileId, AttendanceDate)` |
| Pembatalan dispensasi yang dicatat Kepala Unit | Pegawai tidak dapat membatalkannya (`403`); penyetuju dapat menolak | State 30.3: "Batalkan — Pengaju"; endpoint batal hanya ada di layanan mandiri |
| Penerima pemberitahuan | Pengaju; dilewati bila pengaju sendiri yang memutuskan (Kepala Unit mencatat atas nama lalu memutuskan) | `INT-HR-105` menyebut "pengaju". Akibatnya pegawai yang dicatatkan Kepala Unit tidak menerima lonceng — **perlu keputusan pemilik modul** bila pegawai juga harus diberi tahu |
| Daftar korporat | Cakupan baca (Kepala Unit/Bidang, `ReadAllUnits`) **ditambah** dispensasi yang penyetujunya pengguna ini di unit mana pun; `awaitingMyDecision=true` hanya yang `Submitted` untuk pengguna ini | Kontrak: "pada unit dalam cakupan baca, atau yang menunggu keputusan pengguna" |
| Validasi wajib kosong (`Reason`, `RejectionReason`) | Ditangkap validasi model ASP.NET (`400` bentuk `ProblemDetails`) sebelum service; spasi saja ditangkap service (`400` pesan Indonesia) | Pola `[Required]` DTO yang berlaku di repository |
| Pemicu hitung ulang sesudah disetujui (`INT-HR-102`) | Tidak dikerjakan | Cakupan `BE-HRD-354` |

---

## 4. Dokumentasi endpoint

Base URL korporat: `api/v1/corporate/human-resource/attendance/dispensations` — Tag `Corporate / Human Resource / Attendance Management / Attendance Dispensation`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | `AttendanceDispensation : Read` | — | Status, jenis, ukuran halaman, batas mundur | `200` |
| `GET` | `/` | `AttendanceDispensation : Read` | Query `organizationUnitId`, `fromDate`, `toDate` (default bulan berjalan), `status`, `awaitingMyDecision`, `pageNumber`, `pageSize` | `PagedResult<DispensationResponse>` | `200`, `400`, `401`, `403` |
| `GET` | `/{id}` | `AttendanceDispensation : Read` | — | `DispensationResponse` | `200`, `401`, `403`, `404` |
| `POST` | `/` | `AttendanceDispensation : Create` | `CreateDispensationRequest` | `DispensationResponse` | `200`, `400`, `401`, `403` (bukan unit kelola, `VR-PK-15`), `404`, `409`, `422` (`VR-PK-14`, `VR-PK-16`) |
| `POST` | `/{id}/approve` | `AttendanceDispensation : Approve` | `{ rowVersion }` | `DispensationResponse` | `200`, `401`, `403` (`VR-PK-17`, `VR-PK-15`), `404`, `409` |
| `POST` | `/{id}/reject` | `AttendanceDispensation : Approve` | `{ rowVersion, rejectionReason }` | `DispensationResponse` | `200`, `400`, `401`, `403`, `404`, `409` |

Base URL layanan mandiri: `api/v1/self-services/human-resource/attendance-dispensations` — Tag `Self Services / Human Resource / Attendance Dispensation`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | `MyAttendanceDispensation : Read` | Query `fromDate`, `toDate`, `pageNumber`, `pageSize` | `PagedResult<DispensationResponse>` | `200`, `401`, `403` |
| `POST` | `/` | `MyAttendanceDispensation : Create` | `CreateMyDispensationRequest` | `DispensationResponse` | `200`, `400`, `401`, `403`, `409`, `422` |
| `POST` | `/{id}/cancel` | `MyAttendanceDispensation : Cancel` | `{ rowVersion }` | `DispensationResponse` | `200`, `401`, `403`, `404`, `409` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat; tidak ada peringatan dari berkas baru | `PASS` | Keluaran build |
| Proyek uji HR (`dotnet test`, SQLite di memori) | 209/209 lulus (186 sebelumnya + 8 uji dispensasi + 6 resource + 9 endpoint di uji registri) | `PASS` | Keluaran `dotnet test` |
| — Contoh `HRD-DEC-200` | Penyetuju Sari dari server; DTO tanpa kolom penyetuju; disetujui; `RowVersion` berganti; Budi menerima lonceng; log mesin kosong | `PASS` | `Pengajuan_sendiri_penyetuju_dari_server_lalu_disetujui` |
| — `AT-HRD-PK-33` / `VR-PK-17` | Dispensasi Sari → penyetuju Dodi; Sari, Budi, Hani, Admin HRD menyetujui/menolak → `403` "Anda tidak berwenang memutuskan dispensasi ini", status tetap; Dodi menyetujui; pegawai tanpa atasan → `422` | `PASS` | `AT_HRD_PK_33_…` |
| — `AT-HRD-PK-34` / `VR-PK-14` | 5 dan 4 hari ke belakang `422` "…sampai 3 hari ke belakang"; 3 hari sah; setelan diubah ke 5 → 5 hari sah; metadata ikut | `PASS` | `AT_HRD_PK_34_…` |
| — `VR-PK-13`, alasan, jenis, rentang, `VR-PK-16`, bertumpuk | `400` (jam selesai ≤ mulai, alasan spasi, `FullDay`, jam di luar tanggal); `422` atasan tanpa hak setuju; shift malam 05.00–07.00 hari berikutnya sah; jam tanpa zona = WIB; bertumpuk `409` | `PASS` | `Validasi_pengajuan` |
| — `VR-PK-15` | Periode ICU ditutup → pengajuan `403` "Periode kehadiran Februari 2027 sudah ditutup", keputusan dan pembatalan yang tertunda `403`; HCU tidak terdampak | `PASS` | `Periode_ditutup_…` |
| — Catat atas nama | Sari → Budi sah (`IsOnBehalf`, penyetuju Sari); Sari → HCU, Dodi (Kepala Bidang) → ICU, Budi (tanpa jabatan) → `403`; pegawai tanpa atasan `422` pesan atas nama; pegawai tak dikenal `404`; Budi tidak dapat membatalkan `403`; Sari menolak dengan alasan, tanpa lonceng ke dirinya | `PASS` | `Catat_atas_nama_…` |
| — Transisi dan konkurensi | Setujui ulang token lama `409`; tolak sesudah disetujui `409`; batal sesudah disetujui `409`; tolak tanpa alasan `400`; batal milik orang lain `404`; batal sah → setujui `409`; pusat pemberitahuan mati → keputusan tetap tersimpan | `PASS` | `Transisi_dan_konkurensi` |
| — Daftar dan detail | Sari: ICU; Dodi: semua unit bidang; `awaitingMyDecision` per penyetuju; Admin HRD (`ReadAllUnits`) semua; Hani: HCU; Budi: kosong; filter unit di luar cakupan `403`; detail lintas unit `403`, penyetuju boleh; daftar mandiri hanya milik sendiri | `PASS` | `Daftar_dan_detail_mengikuti_cakupan` |
| — Lapis hak akses | Kedua controller lolos `PermissionRegistryValidator`; enam resource/aksi terdaftar; sembilan endpoint menolak pegawai tanpa kebijakan Akses Role | `PASS` | `A52PermissionRegistryTests` |
| Kontrol negatif: (1) pemeriksaan penyetuju, (2) batas mundur, (3) cakupan atas nama, (4) status transisi, (5) periode ditutup saat pengajuan — dimatikan | Lima uji yang menjaganya gagal; kode dipulihkan (dibandingkan byte demi byte), seluruh proyek 209/209 | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2450 identitas kanonik"; enam resource/aksi tersimpan di `SysActionAccess` | `PASS` | `be353.log` di scratchpad sesi; `at-log.jsonl` |
| API devYoga (17 November 2026) — tanpa sesi | `401` (korporat dan mandiri) | `PASS` | `at-353.mjs`, `at-log.jsonl` (entri "353 …") |
| — `gilang.mahendra` tanpa izin | Ajukan dan daftar korporat `403` | `PASS` | `at-log.jsonl` |
| — Superadmin (tanpa profil pegawai) mengajukan untuk dirinya | `403` "Akun Anda belum terhubung dengan profil pegawai." | `PASS` | `at-log.jsonl` |
| — Data sementara (pola izin sementara + data uji yang disetujui pengguna pada `BE-HRD-351`/`357`; baris atasan langsung adalah jenis data uji baru untuk task ini): izin dispensasi pada jabatan gilang; atasan langsung `UJI-A52-353` Raka → Gilang dan Gilang → Raka | Tersimpan | — | `at-log.jsonl` |
| — `VR-PK-16` sebelum atasan dibuat | `422` "Atasan langsung Anda belum ditetapkan. Hubungi HRD" | `PASS` | `at-log.jsonl` |
| — Superadmin mencatat atas nama Raka, klien menyisipkan `approverWorkforceProfileId` nol | `200`, `IsOnBehalf`, penyetuju **Gilang** (server), unit ICU | `PASS` | `at-log.jsonl` |
| — Gilang: daftar menunggu keputusan → setujui → setujui ulang token lama | Muncul (`canDecide=true`) → `200 Approved` → `409` | `PASS` | `at-log.jsonl` |
| — Pemberitahuan ke pengaju | `HrdNotification` untuk superadmin: "Dispensasi Raka Firmansyah disetujui — Datang terlambat 17-11-2026 07:00–07:40: disetujui." (kueri pertama memakai `DedupKey` tanpa akhiran `|userId` sehingga kosong; dikoreksi) | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-33`: gilang mengajukan untuk dirinya (penyetuju Raka) lalu menyetujui sendiri | `403` "Anda tidak berwenang memutuskan dispensasi ini" | `PASS` | `at-log.jsonl` |
| — Bertumpuk / batalkan / daftar milik sendiri | `409` / `200 Cancelled` / satu baris "Dibatalkan" | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-34` (5 hari, batas 3), `VR-PK-13`, tanpa alasan, `FullDay`, atas nama tanpa izin `Create` | `422` pesan kontrak, `400`, `400`, `400`, `403` | `PASS` | `at-log.jsonl` |
| — Baris tersimpan dan log mesin | `Approved`, `IsOnBehalf`, penyetuju dan pemutus Gilang; tidak ada log mesin baru Raka | `PASS` | `at-log.jsonl` |
| — Audit | 4 baris `[AUD]` untuk 4 perubahan berhasil; teks alasan tidak muncul di log | `PASS` | `be353.log` |
| — Pemulihan | Izin dikirim ulang (32 butir) dan dibaca identik; dua atasan sementara `IsActive=false`, `IsDelete=true`; dua dispensasi uji ditandai terhapus; gilang kembali `403` | `PASS` | `at-log.jsonl`; salinan `at-353-policy-backup.json` |
| `VR-PK-15` lewat API | Tidak dijalankan: satu-satunya periode `Closed` di devYoga (1–7 Okt) sudah di luar batas mundur 3 hari, sehingga `VR-PK-14` lebih dulu menolak; terbukti di harness | `NOT RUN` | — |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada ID penyetuju dari klien | Terpenuhi — DTO tanpa kolom penyetuju; kolom yang disisipkan klien diabaikan (devYoga) | Bagian 5 |
| Penyetuju ≠ pegawai (`AT-HRD-PK-33`) | Terpenuhi | Harness dan devYoga |
| Batas mundur (`AT-HRD-PK-34`) | Terpenuhi | Harness dan devYoga |
| `VR-PK-13` s.d. `VR-PK-17` | Terpenuhi (`VR-PK-15` di harness) | Bagian 5 |
| Risiko: atasan langsung belum ditetapkan untuk banyak pegawai | Ditangani dengan pesan `VR-PK-16`; devYoga memang belum punya data atasan langsung untuk akun uji | Bagian 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) Enam aksi baru belum diberikan ke jabatan mana pun: pegawai perlu `MyAttendanceDispensation : Read/Create/Cancel`; Kepala Unit `AttendanceDispensation : Read/Create/Approve`; Kepala Bidang `Read/Approve`. (2) **devYoga belum punya data `WfpManagerAssignment` untuk akun uji**; tanpa data atasan langsung, setiap pengajuan ditolak `VR-PK-16`. (3) Pegawai yang dispensasinya dicatat Kepala Unit tidak menerima lonceng keputusan (kontrak hanya menyebut pengaju) — perlu keputusan pemilik modul. (4) Menit terlambat belum berkurang sampai `BE-HRD-354` |
| Risiko tersisa | Daftar korporat memuat dispensasi dalam rentang tanggal lalu menyaring cakupan di memori; ukurannya sebanding dengan jumlah dispensasi pada rentang itu (default satu bulan) |
| Perubahan sampingan | `NONE`. Izin, atasan sementara, dan dispensasi uji di devYoga sudah dipulihkan/ditandai terhapus |
| Interupsi | `NONE` |
| Status Git | 4 berkas source baru + `Program.cs`; 1 uji baru + 2 berkas uji diubah; laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-357` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-354` (dispensasi pada pemrosesan harian, `AT-HRD-PK-32`), `BE-HRD-355`, `BE-HRD-356`. Frontend: `FE-HRD-318` (butuh juga `BE-HRD-354`) |
