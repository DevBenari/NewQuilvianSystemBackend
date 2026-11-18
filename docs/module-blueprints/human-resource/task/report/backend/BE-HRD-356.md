# Laporan Perubahan Backend — `BE-HRD-356`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-356` |
| Judul | Kejadian gangguan mesin dan rujukan pada koreksi (`POST-MVP`) |
| Slice | Amandemen `A52` — `EPIC HRD-118`, gelombang `POST-MVP` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B88 |
| Trace | `FR-PK-028`, `FR-PK-029`; `HRD-DEC-202` (uji a–d), `HRD-ASM-77`; `INV-A52-07`; `02-backend-architecture.md` 35.3.3, 35.4.1–35.4.3; `api-contract.md` "Attendance Device Incident", "Self Services / Attendance Correction" (**Diubah**), koreksi atas nama (**Diubah**); `state-transition-matrix.md` 30.5; `validation-matrix.md` `VR-PK-20` s.d. `VR-PK-22`; `permission-audit-matrix.md` 30.1, 30.2, 30.5; `AT-HRD-PK-35`, `AT-HRD-PK-36` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-343` ✅; master mesin absen `BE-HRD-346` ✅ |
| Klasifikasi | `HEAVY` — skor 9: satu repository (0), sekitar 25 berkas diperiksa (2), 3 berkas source baru + 5 diubah (2), perubahan alur koreksi yang sudah dipakai (2), hak akses baru (1), perbaikan cacat lama yang ditemukan (1), tanpa schema (0), ditambah 1 untuk dampak pada persetujuan koreksi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services/AttendanceDeviceIncidentService.cs,Controllers/AttendanceDeviceIncidentController.cs,DTOs/AttendanceDeviceIncidentDtos.cs}` (baru); `Services/AttendanceCorrectionService.cs`, `Services/AttendanceCorrectionOnBehalfService.cs`, `DTOs/AttendanceCorrectionDtos.cs`, `Areas/SelfServices/HumanResource/Controllers/AttendanceCorrectionSelfServiceController.cs`, `Program.cs`; proyek uji HR (`DeviceIncidentTests.cs` baru, `A52PermissionRegistryTests.cs`, `AcceptanceTestIndex.md`); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`) + perubahan `BE-HRD-357`, `353`, `354`, `355` yang belum di-commit; keempat berkas koreksi bersih sebelum task ini |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — proyek uji HR 237/237 (termasuk kontrol negatif per putaran); `AT-HRD-PK-36`, `VR-PK-20` s.d. `22`, `PUT` koreksi dengan rujukan, dan 401/403 lulus lewat API devYoga; `AT-HRD-PK-35` bagian "diterima untuk diputuskan" lolos gerbang lampiran tetapi berhenti di workflow karena devYoga belum punya definisi `ATTENDANCE_CORRECTION` (bagian 5, 7) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` dan `SelfServices` (endpoint pilihan pada koreksi mandiri) |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); entity `HrdAttendanceDeviceIncident` dan kolom `HrdAttendanceCorrectionRequest.DeviceIncidentId` sudah ada (`BE-HRD-334`), tidak ada entity baru |
| Keberlakuan | `NEW CODE` (service, controller, DTO kejadian gangguan) dan `TOUCHED LEGACY` (layanan, DTO, dan controller koreksi presensi) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-LOG-001`, `QBE-TXN-001` |
| Tidak berlaku | `QBE-CODE-*` (tanpa nomor dokumen baru), `QBE-DB-*` (tanpa schema) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Konsep kejadian gangguan belum ada di kode (`HRD-TF-065`); tabel dan kolom rujukan sudah ada tanpa API | Mesin mati harus dibuktikan per orang dengan lampiran; HRD tidak punya riwayat gangguan per mesin |
| **Cacat lama ditemukan saat uji:** `PUT` koreksi presensi selalu gagal "Attendance correction gagal diperbarui karena terjadi konflik data" | Detail baru ditambahkan ke koleksi koreksi yang sudah dilacak dengan Id terisi, sehingga EF menganggapnya baris lama dan menjalankan UPDATE 0 baris (`DbUpdateConcurrencyException`). Kontrak task ini mewajibkan `PUT` menerima `DeviceIncidentId`, jadi cacat ini diperbaiki |

---

## 2. Proses bisnis

**Pencatatan (`HRD-DEC-202` butir 1–2, 6; state 30.5).** Petugas IT atau Admin HRD mencatat satu kejadian: mesin absen terdaftar,
jam mulai, jam selesai, dan uraian. Jam selesai harus sesudah jam mulai (`VR-PK-20`); mesin harus terdaftar (`404`). Mencatat kejadian
**tidak pernah** membuat scan (`INV-A52-07`). Kejadian yang salah catat dibatalkan dengan alasan wajib; yang sudah dibatalkan tidak dapat
dibatalkan lagi (`409`). Daftar dapat disaring per mesin dan tanggal (riwayat gangguan per mesin) dan menampilkan jumlah koreksi yang
merujuk tiap kejadian.

**Rujukan pada koreksi (butir 3–5, `HRD-ASM-77`).** Formulir koreksi mandiri mendapat pilihan kejadian `Active` yang bersinggungan dengan
tanggal kerja itu (`device-incident-options`). Koreksi mandiri (buat/ubah) dan koreksi atas nama menerima `DeviceIncidentId` opsional;
kejadian dibatalkan ditolak `422` (`VR-PK-21`), tidak dikenal `404`. Saat koreksi **diajukan**:

| Kondisi | Hasil |
| --- | --- |
| Merujuk kejadian, semua jam masuk/pulang yang diajukan di dalam rentang gangguan | Lampiran tidak wajib — termasuk bila alasan pengajuan biasanya mewajibkan lampiran |
| Merujuk kejadian, ada jam di luar rentang (atau tidak ada jam sama sekali), tanpa lampiran | `422` "Jam di luar rentang gangguan wajib disertai lampiran" (`VR-PK-22`) |
| Merujuk kejadian, di luar rentang, dengan lampiran | Lanjut sebagai koreksi biasa |
| Kejadian yang dirujuk dibatalkan sesudah draf dibuat | `422` "Kejadian gangguan ini sudah dibatalkan" |
| Tanpa rujukan | Aturan lama (alasan pengajuan yang mewajibkan lampiran → `400`) |

Koreksi tetap diputuskan atasan lewat workflow seperti koreksi lain (butir 5); satu kejadian dapat dirujuk banyak koreksi (uji (d)).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-202`, `HRD-ASM-77`; kontrak API tiga bagian terkait; `state-transition-matrix.md` 30.5; `validation-matrix.md` `VR-PK-20`–`22`; `permission-audit-matrix.md` 30.1, 30.2, 30.5; `data-dictionary.md`; `acceptance-test-matrix.md` `AT-HRD-PK-35`, `-36`
- Model `HrdAttendanceDeviceIncident` + konfigurasi (CHECK `EndAt > StartAt`), `HrdAttendanceCorrectionRequest` (`DeviceIncidentId`), `HrdAttendanceCorrectionDetail` + konfigurasi (indeks unik tersaring), `MstAttendanceDevice`, `MstRequestReason`, `HrdAttendanceSetting` (`RequireAttachmentForCorrection`), `TrxWorkflowInstance`
- `AttendanceCorrectionService` (buat, ubah, ajukan, `AddDetails`, `ValidateRequestContentAsync`, `ValidateCorrectionEligibility`, `ResolveWorkflowCodeAsync`), `AttendanceCorrectionOnBehalfService`, `AttendanceCorrectionFieldCatalog`, `AttendanceCorrectionSelfServiceController`, `AttendanceCorrectionController`, `AttendanceDeviceService` (hapus mesin yang punya kejadian)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `AttendanceManagement/Services/AttendanceDeviceIncidentService.cs` | **Baru.** Daftar (mesin, tanggal WIB, status, paginasi; jumlah koreksi perujuk), catat, batal, pilihan untuk formulir koreksi |
| `AttendanceManagement/Controllers/AttendanceDeviceIncidentController.cs` | **Baru.** `GET /`, `POST /`, `POST /{id}/cancel`; `AttendanceDeviceIncident : Read/Create/Cancel`; audit `AttendanceDeviceIncident.Create/Cancel` |
| `AttendanceManagement/DTOs/AttendanceDeviceIncidentDtos.cs` | **Baru.** Request catat/batal, query, `DeviceIncidentResponse`, `DeviceIncidentOptionResponse` |
| `AttendanceManagement/Services/AttendanceCorrectionService.cs` | `DeviceIncidentId` pada buat dan ubah (`ValidateDeviceIncidentReferenceAsync`); gerbang lampiran saat ajukan dipindah ke method publik `ValidateSubmissionEvidenceAsync` (perilaku tanpa rujukan sama persis) dengan aturan rujukan; `AreRequestedTimesWithin`; `DeviceIncidentId` di detail respons; **perbaikan `AddDetails`**: detail baru didaftarkan eksplisit sebagai `Added`. CRLF dipertahankan |
| `AttendanceManagement/Services/AttendanceCorrectionOnBehalfService.cs` | Validasi dan simpan `DeviceIncidentId` |
| `AttendanceManagement/DTOs/AttendanceCorrectionDtos.cs` | `DeviceIncidentId` pada request buat, atas nama, ubah, dan respons detail |
| `SelfServices/HumanResource/Controllers/AttendanceCorrectionSelfServiceController.cs` | `GET /device-incident-options?attendanceDate=` (`MyAttendanceCorrection : Read`); dependensi `AttendanceDeviceIncidentService` |
| `Program.cs` | Satu baris `AddScoped<AttendanceDeviceIncidentService>()` (CRLF) |
| `Tests/.../DeviceIncidentTests.cs` | **Baru.** 4 uji |
| `Tests/.../A52PermissionRegistryTests.cs` | Controller kejadian gangguan masuk validator registri dan uji 403; tiga resource/aksi masuk daftar |
| `Tests/.../AcceptanceTestIndex.md` | `AT-HRD-PK-35`, `-36` dipetakan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint kejadian gangguan dan satu endpoint pilihan sesuai kontrak `v26`. Pilihan memakai `DeviceIncidentOptionResponse` (`Value`, `Label`, mesin, jam) — bentuk `OptionResponse` diperluas. Koreksi menerima `DeviceIncidentId` opsional; respons detail menambahkan `DeviceIncidentId` |
| Database | Tanpa migration; menulis `HrdAttendanceDeviceIncident`; mengisi kolom `DeviceIncidentId` yang sudah ada |
| Keamanan/Auth | Resource baru `AttendanceDeviceIncident : Read/Create/Cancel` (`Cancel` bertipe akses `Update`), terdaftar saat startup, belum diberikan ke jabatan mana pun. Pilihan kejadian memakai izin koreksi mandiri yang sudah ada |
| Perilaku koreksi lama | Tanpa rujukan: tidak berubah. `PUT` koreksi kini berfungsi (sebelumnya selalu `409`) |

### 3.4 Delta dan keputusan desain

| Butir | Implementasi | Alasan |
| --- | --- | --- |
| `VR-PK-22` vs `HRD-ASM-77` | Di luar rentang tanpa lampiran **selalu** `422` bila koreksi merujuk kejadian | `VR-PK-22` merinci aturan untuk "koreksi dengan gangguan" tanpa syarat setelan; `AT-HRD-PK-35` mengharapkan `422` tanpa menyebut setelan. Bila setelan tidak mewajibkan lampiran, `HRD-ASM-77` ("ikut setelan") bisa berarti lolos — **perlu konfirmasi pemilik modul** |
| `RequireAttachmentForCorrection` pada setelan kehadiran | Tidak ditegakkan (tetap seperti sebelumnya) | Temuan: setelan ini belum dipakai layanan koreksi mana pun; yang berlaku hanya `MstRequestReason.IsAttachmentRequired`. Menegakkannya mengubah perilaku semua koreksi dan di luar cakupan task ini |
| Koreksi tanpa jam (mis. status) yang merujuk kejadian | Tidak dibebaskan dari lampiran | `HRD-ASM-77` berbicara tentang jam; tanpa jam tidak ada yang bisa dicocokkan dengan rentang |
| Batas rentang | Inklusif (06.30 dan 09.00 termasuk) | Jam tepat di batas masih saat gangguan |
| Kejadian dibatalkan sesudah dirujuk | Draf yang merujuknya tidak dapat diajukan (`422`); koreksi yang sudah diajukan tidak diubah | State 30.5 menolak rujukan ke kejadian `Cancelled` |
| Validasi saat buat/ubah tidak memeriksa jam vs rentang | Jam dinilai saat **ajukan** | Lampiran hanya dapat diunggah sesudah draf ada; aturan lampiran yang sudah ada juga dinilai saat ajukan |
| Perbaikan `AddDetails` | `_dbContext.Set<HrdAttendanceCorrectionDetail>().Add(detail)` di samping penambahan ke koleksi | Perbaikan minimal; berlaku untuk buat (induk baru) dan ubah (induk dilacak). Ditemukan karena kontrak `PUT` + `DeviceIncidentId` |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/attendance/device-incidents` — Tag `Corporate / Human Resource / Attendance Management / Attendance Device Incident`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | `AttendanceDeviceIncident : Read` | Query `attendanceDeviceId`, `fromDate`, `toDate` (WIB), `status`, `pageNumber`, `pageSize` (maks. 100) | `PagedResult<DeviceIncidentResponse>` | `200`, `400`, `401`, `403` |
| `POST` | `/` | `AttendanceDeviceIncident : Create` | `CreateDeviceIncidentRequest` (`AttendanceDeviceId`, `StartAt`, `EndAt`, `Description`) | `DeviceIncidentResponse` | `200`, `400` (`VR-PK-20`), `401`, `403`, `404` (mesin) |
| `POST` | `/{id}/cancel` | `AttendanceDeviceIncident : Cancel` | `{ cancelReason }` | `DeviceIncidentResponse` | `200`, `400`, `401`, `403`, `404`, `409` |

Base URL: `api/v1/self-services/human-resource/attendance-corrections`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/device-incident-options` | `MyAttendanceCorrection : Read` | Query `attendanceDate` | `List<DeviceIncidentOptionResponse>` | `200`, `400`, `401`, `403` |
| `POST` | `/`, `PUT` `/{id}` | `MyAttendanceCorrection : Create` / `: Update` | + `deviceIncidentId` opsional | Tetap | + `404`/`422` (`VR-PK-21`) |
| `POST` | `/{id}/submit` | `MyAttendanceCorrection : Submit` | Tetap | Tetap | + `422` (`VR-PK-21`, `VR-PK-22`) |

`POST api/v1/corporate/human-resource/attendance/correction-requests/on-behalf` menerima `deviceIncidentId` dengan cara yang sama.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat; tidak ada peringatan dari berkas baru/diubah | `PASS` | Keluaran build |
| Proyek uji HR (`dotnet test`, SQLite di memori) | 237/237 lulus (227 sebelumnya + 4 uji gangguan mesin + 3 resource + 3 endpoint di uji registri) | `PASS` | Keluaran `dotnet test` |
| — `AT-HRD-PK-36`, state 30.5, `VR-PK-20` | Jumlah log mesin sama sebelum/sesudah catat dan batal; data kejadian + pencatat; jam tanpa zona = WIB; jam sama/terbalik `400` pesan kontrak; uraian kosong `400`; mesin tak terdaftar `404`; batal tanpa alasan `400`, batal sah, ulang `409`, tak dikenal `404` | `PASS` | `AT_HRD_PK_36_…` |
| — Riwayat per mesin dan pilihan | Saring mesin, tanggal (kejadian lewat tengah malam ikut tanggal berikutnya), status; tanggal terbalik `400`; pilihan hanya `Active` pada tanggal itu dengan label mesin, jam WIB, uraian | `PASS` | `Daftar_riwayat_per_mesin_dan_pilihan_koreksi` |
| — `AT-HRD-PK-35`, uji (b)(d), `VR-PK-21`/`22` | 06.50–09.00 tanpa lampiran lolos gerbang; 09.30 tanpa lampiran `422` pesan kontrak; 2 koreksi merujuk satu kejadian; dengan lampiran lolos; koreksi status saja `422`; kejadian dibatalkan → draf perujuk `422`, buat baru `422`, tak dikenal `404`; ubah: rujukan diganti dan tersimpan, kejadian dibatalkan `422` | `PASS` | `AT_HRD_PK_35_…` |
| — Tanpa rujukan | Aturan lama tetap (`400` bila alasan mewajibkan lampiran; lolos bila tidak); rujukan dalam rentang membebaskan walau alasan mewajibkan | `PASS` | `Tanpa_rujukan_aturan_lampiran_lama_tetap` |
| — Cacat `PUT` koreksi | Sebelum perbaikan, `UpdateAsync` gagal juga **tanpa** rujukan kejadian; diagnosa sementara (dihapus sesudahnya): `DbUpdateConcurrencyException` "expected to affect 1 row(s), but actually affected 0" | `PASS` (temuan) | Keluaran uji diagnosa |
| — Lapis hak akses | Controller lolos `PermissionRegistryValidator`; tiga resource/aksi terdaftar; tiga endpoint menolak pegawai tanpa kebijakan Akses Role | `PASS` | `A52PermissionRegistryTests` |
| Kontrol negatif per putaran: A (`VR-PK-20` + filter status pilihan), B (pembebasan lampiran selalu), C (`VR-PK-21` dimatikan), D (perbaikan `AddDetails` dikembalikan) | A: gagal `AT_HRD_PK_36` dan `Daftar_…`; B, C, D: masing-masing gagal `AT_HRD_PK_35`. Kode dipulihkan (dibandingkan byte demi byte) setiap putaran | `PASS` | `mut356.sh` dan keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2457 identitas kanonik"; tiga aksi `AttendanceDeviceIncident` tersimpan | `PASS` | `be356.log`; `at-log.jsonl` |
| API devYoga (17 November 2026) — tanpa sesi / gilang tanpa izin | `401` / daftar dan catat `403` | `PASS` | `at-356.mjs`, `at-log.jsonl` (entri "356 …") |
| — `VR-PK-20` / mesin tak terdaftar | `400` pesan kontrak / `404` | `PASS` | `at-log.jsonl` |
| — Catat kejadian `UJI-A52-GDB-01` 06.30–09.00 | `200`, `Active`, pencatat dari sesi; riwayat per mesin memuatnya | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-36` | Jumlah baris `HrdAttendanceRawLog` 73 → 73 (juga sesudah batal) | `PASS` | `at-log.jsonl` |
| — Izin sementara `MyAttendanceCorrection` (6 aksi) pada jabatan gilang; pilihan formulir | "UJI-A52-GDB-01 Mesin uji Gedung B (UJI-A52) — 17-11-2026 06.30–09.00 — …"; sesudah kejadian dibatalkan: 0 pilihan | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-35` bagian 2 | Draf 09.30 merujuk kejadian → ajukan `422` "Jam di luar rentang gangguan wajib disertai lampiran" | `PASS` | `at-log.jsonl` |
| — `PUT` koreksi (cacat yang diperbaiki) | Ubah ke 06.50 → `200` "Attendance correction berhasil diperbarui."; detail memuat rujukan dan jam baru | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-35` bagian 1 | Ajukan 06.50 tanpa lampiran **lolos gerbang lampiran**, lalu `404` "Workflow definition aktif dengan kode ATTENDANCE_CORRECTION tidak ditemukan untuk scope pemohon."; koreksi tetap `Draft`, tidak ada instance workflow | `PARTIAL` — data master workflow koreksi belum ada di devYoga | `at-log.jsonl` |
| — Batal kejadian | Tanpa alasan `400`; dengan alasan `200` | `PASS` | `at-log.jsonl` |
| — `VR-PK-21` atas nama (putaran 2, tanpa draf aktif) | Merujuk kejadian dibatalkan `422` "Kejadian gangguan ini sudah dibatalkan"; tak dikenal `404`; tidak ada koreksi tercipta | `PASS` | `at-356b.mjs`, `at-log.jsonl` (entri "356b …") |
| — Audit | 4 baris `[AUD]` untuk 4 perubahan kejadian berhasil | `PASS` | `be356.log` |
| — Pemulihan | Izin dikirim ulang (32 butir) dan dibaca identik; koreksi uji dan detailnya serta tiga kejadian uji dihapus lunak; tidak ada instance workflow | `PASS` | `at-log.jsonl`; salinan `at-356-policy-backup.json` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Mencatat kejadian tidak menambah log mesin (`AT-HRD-PK-36`, uji (a)) | Terpenuhi | Harness dan devYoga |
| Koreksi merujuk kejadian tanpa lampiran (uji (b)) | Terpenuhi di harness; devYoga lolos gerbang lampiran, pengajuan penuh terhalang workflow master | Bagian 5 |
| Koreksi tetap menunggu keputusan atasan (uji (c)) | Tidak diubah — jalur workflow koreksi yang sudah ada | Kode |
| Satu kejadian dirujuk banyak koreksi (uji (d)) | Terpenuhi | Harness |
| `VR-PK-20` s.d. `VR-PK-22` | Terpenuhi | Harness dan devYoga |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) **devYoga belum punya definisi workflow `ATTENDANCE_CORRECTION`**, sehingga koreksi presensi apa pun tidak dapat diajukan di devYoga (bukan akibat task ini). (2) Tiga aksi `AttendanceDeviceIncident` belum diberikan ke jabatan mana pun (Petugas IT, Admin HRD). (3) Setelan `RequireAttachmentForCorrection` belum ditegakkan untuk koreksi biasa — keputusan pemilik modul diperlukan, termasuk tafsir `VR-PK-22` vs `HRD-ASM-77` (bagian 3.4). (4) **Cacat lama `PUT` koreksi diperbaiki** di task ini; layar koreksi yang sebelumnya gagal menyimpan perubahan draf kini berfungsi |
| Risiko tersisa | Indeks unik tersaring `(AttendanceCorrectionRequestId, FieldName)` pada detail koreksi: `PUT` di PostgreSQL terbukti berhasil (field sama dihapus lunak lalu ditambah); harness SQLite melepas indeks unik tersaring sehingga urutan perintah hanya terbukti di devYoga |
| Perubahan sampingan | `NONE`. Berkas uji diagnosa sementara sudah dihapus; data uji devYoga dihapus lunak; izin dipulihkan |
| Interupsi | `NONE` |
| Status Git | 3 berkas source baru + 5 diubah; 1 uji baru + 2 berkas uji diubah; laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-357`, `353`, `354`, `355` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-358` (verifikasi `POST-MVP` `A52`) — semua dependency kini ✅. Frontend: `FE-HRD-320` |
