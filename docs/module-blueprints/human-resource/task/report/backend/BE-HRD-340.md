# Laporan Perubahan Backend — `BE-HRD-340`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-340` |
| Judul | Tambah shift tanpa turun ke `Draft`, hapus shift terbit, endpoint Riwayat Revisi |
| Slice | Amandemen `A52` atas `S-B4` Penjadwalan — `EPIC HRD-110`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B85 |
| Trace | `FR-PK-005`, `FR-PK-006`; `HRD-DEC-195`; `INV-A52-02`, `INV-A52-03`; `02-backend-architecture.md` 35.9.3; `api-contract.md` 32 Roster Period; `permission-audit-matrix.md` 30.1; `VR-PK-04`, `-06`, `-07`, `-27`; `AT-HRD-PK-05`, `-06`, `-08` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-338` 🟡 (`ShiftRevisionRecorder` selesai), `BE-HRD-336` 🟡 (`HrdUnitScopeService` selesai); sisa keduanya hanya uji API tertentu, tidak memengaruhi task ini |
| Klasifikasi | `HEAVY` — skor 8: satu repository (0), sekitar 16 berkas diperiksa (1), 4 berkas source (1), transaksi + konkurensi (2), perubahan perilaku dan dua endpoint baru (2), tanpa schema (0), hak akses baru (1), tanpa UI (0), ditambah 1 untuk perubahan string hak akses yang ada |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/SchedulingManagement/{Services,Controllers,DTOs,Constants}/`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `400a9678` (branch `yoga`) + perubahan `BE-HRD-339` dan migration `BE-HRD-FIX-001` yang belum di-commit |
| Tanggal | 13 November 2026 |
| Status | ✅ Selesai — harness 59/59 (termasuk kontrol negatif) dan `AT-HRD-PK-05`, `-06`, `-08` lulus lewat API devYoga |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` untuk `RosterPeriodService`, `RosterPeriodController`, `RosterSchedulingDtos`, `SchedulingRequestValueConstants`; `NEW CODE` untuk dua endpoint dan tiga metode service baru |
| QBE yang berlaku | `QBE-NAM-001`, `QBE-SVC-001` (aturan, transaksi, dan cakupan di service; controller hanya meneruskan hasil) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Tambah shift pada roster terbit menurunkan status ke `Draft` (`RosterPeriodService.cs:391` lama) tanpa menaikkan versi | Roster yang sedang berjalan tidak lagi terbit. Terbit ulang lalu gagal 500 (`IX_TrxRosterPublication_RosterPeriodId_VersionNumber`) karena publikasi kedua memakai versi yang sama — terbukti di devYoga pada `ROST-202611-0003` |
| Shift tambahan tidak diperiksa sama sekali | Pegawai tanpa penempatan, sedang tugas belajar, atau bertabrakan jadwal tetap bisa ditambahkan ke roster terbit (melanggar `INV-A52-03`) |
| Shift yang tidak dikenal dilewati diam-diam | Permintaan "berhasil" tetapi shift tidak tersimpan |
| Tidak ada cara menghapus shift dari roster terbit, tidak ada Riwayat Revisi yang dapat dibaca | `HRD-DEC-195` butir 3 tidak dapat dipenuhi di layar |
| `POST /{id}/assignments` memakai hak akses `RosterAssignment : Create`, resource yang tidak terdaftar di registry | Selain superadmin tidak seorang pun dapat diberi hak menambah shift (`AccessPermissionService` tidak menemukan `SysControllerAccess` bernama `RosterAssignment`) |

---

## 2. Proses bisnis

**Tambah shift pada roster terbit.** Roster ICU Februari terbit versi 1. Sari menambahkan Tono pada
Pagi 20 Februari dengan alasan "Kekurangan tenaga".

| Langkah | Ditolak bila | Kode |
| --- | --- | --- |
| Cakupan unit, periode terkunci | Di luar cakupan kelola; roster `Closed` | 403 |
| Alasan (`VR-PK-07`) | Kategori atau catatan kosong; kategori lebih dari 50 karakter | 400 |
| Isi permintaan | Tanpa shift; tanggal di luar periode; shift saling bertabrakan dalam satu permintaan | 400 |
| Master shift | Shift tidak dikenal atau tidak aktif | 404 |
| Pemeriksaan penuh (`INV-A52-03`) | Ada blokir `B1`–`B8` (B6 gagal tertutup) | 422 + `errors` berkode |
| Alasan peringatan (`VR-PK-04`) | Ada kode `W1`–`W7` tanpa alasan di `WarningReasons` | 422 + `errors` |

Disimpan dalam satu transaksi: shift baru berstatus `Published` (masuk penugasan pegawai yang sudah
ada di periode itu bila ada), satu baris riwayat `AddShift` per shift, lalu commit. Status dan versi
roster tidak berubah. Roster draf tetap memakai perilaku lama (tanpa alasan, tanpa riwayat).

**Hapus shift dari roster terbit.** Sari menghapus shift tadi dengan alasan "Ruang ditutup". Shift
tidak dihapus fisik: ditandai `IsCancel`, nonaktif, berstatus `Cancelled`, lewat pembaruan bersyarat
(409 bila shift baru saja diganti petugas atau dihapus orang lain). Satu baris riwayat `RemoveShift`
menyimpan pegawai, shift, dan jam sebelumnya. Hitungan penugasan dikurangi. Roster tetap terbit.
Hapus hanya berlaku untuk roster terbit (lainnya 409).

**Riwayat Revisi.** `GET /{id}/revisions` membaca riwayat satu periode, terbaru di atas, dengan
filter tanggal shift, pegawai (sebelum atau sesudah), dan jenis perubahan. Mematuhi cakupan baca
(403 di luar cakupan). Tidak ada endpoint ubah atau hapus.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `api-contract.md` 32 Roster Period; `permission-audit-matrix.md` 30.1; `02-backend-architecture.md` 35.4.2, 35.9.3; `validation-matrix.md` `VR-PK-04`–`07`, `-27`; `integration-contract.md` `INT-HR-101`; `acceptance-test-matrix.md` `AT-HRD-PK-05`, `-06`, `-08`
- `RosterPeriodService.cs`, `RosterPeriodController.cs`, `RosterSchedulingDtos.cs`, `SchedulingRequestValueConstants.cs`, `SchedulingExplicitPermissions.cs`
- `ShiftRevisionRecorder.cs`, `RosterValidationService.cs`, `HrdShiftAssignmentRevision.cs`
- `AccessPermissionService.cs` (pencocokan resource ke `SysControllerAccess`), `AccessPermissionAttribute.cs`, `AccessTypes.cs`
- `AttendanceScheduleResolverService.cs:196-207` (resolver kehadiran mengabaikan shift `IsCancel`/nonaktif dan memeriksa `AssignmentStatus`)
- Frontend `src/lib/services/hr/roster-scheduling-service.js`: `addRosterAssignment` didefinisikan tetapi tidak dipanggil layar mana pun, sehingga tidak ada konsumen yang bergantung pada perilaku `Draft` lama (risiko kartu)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/RosterPeriodService.cs` | `AddAssignmentAsync` mengalihkan roster terbit ke `AddShiftsToPublishedAsync` (baru). Metode baru `RemovePublishedShiftAsync`, `GetRevisionsAsync`, `BuildRevisionResponsesAsync`. Konstruktor menerima `ShiftRevisionRecorder` (sudah terdaftar di DI) |
| `Areas/Corporate/HumanResource/SchedulingManagement/Controllers/RosterPeriodController.cs` | `AddAssignment` memakai `RosterPeriod : Create` (sebelumnya `RosterAssignment : Create`). Endpoint baru `POST {id}/shifts/{shiftAssignmentId}/remove` (`RosterPeriod : RemoveShift`, `AccessTypes.Delete`) dan `GET {id}/revisions` (`RosterPeriod : Read`). `errors` diteruskan pada respons gagal |
| `Areas/Corporate/HumanResource/SchedulingManagement/DTOs/RosterSchedulingDtos.cs` | `CreateRosterAssignmentRequest`: `ReasonCategory` (≤50), `ReasonNote` (≤1000), `WarningReasons`. `RosterAssignmentResponse`: `RevisionIds`, `Warnings`, `InfoMessages` (aditif). Baru: `RemovePublishedShiftRequest`, `ShiftRevisionResponse` |
| `Areas/Corporate/HumanResource/SchedulingManagement/Constants/SchedulingRequestValueConstants.cs` | `ShiftRevisionChangeType.Label`: Ganti Petugas, Koreksi Mundur, Tambah Shift, Hapus Shift |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `v26`. Kolom `ShiftRevisionResponse` tidak dirinci kontrak; diturunkan dari entitas riwayat dan `HRD-DEC-217` butir 2, ditambah nama dan label untuk tampilan. Tambahan di luar tabel kontrak (aditif): `RevisionIds`, `Warnings`, `InfoMessages` pada respons tambah shift; 409 untuk hapus pada roster bukan terbit atau shift yang berubah. Tabel kontrak masih menandai `remove` dan `revisions` "Rencana (belum tersedia)" — artefak terkunci hash, dicatat untuk penyegaran pass blueprint |
| Database | Tanpa migration. Menulis `HrdShiftAssignmentRevision`; hapus shift mengubah `TrxShiftAssignment` (`IsCancel`, `IsActive`, `AssignmentStatus`, `CancelBy`, `CancelDateTime`) |
| Keamanan/Auth | Aksi baru `RosterPeriod : RemoveShift` terdaftar lewat `[AccessAction]` dan dapat diberikan lewat Akses Role. `POST /{id}/assignments` pindah ke `RosterPeriod : Create` sesuai matriks 30.1 — pemegang hak `RosterPeriod : Create` kini juga dapat menambah shift. Catatan alasan tidak ditulis ke log |

---

## 4. Dokumentasi endpoint

### HR Scheduling - Roster Period

Base URL: `api/v1/corporate/human-resource/scheduling-management/roster-periods`

| Method | Path | Hak akses | Request | Response | Kode status |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/assignments` | `RosterPeriod : Create` | `CreateRosterAssignmentRequest` | `RosterAssignmentResponse` | 201, 400, 403, 404, 422 |
| `POST` | `/{id}/shifts/{shiftAssignmentId}/remove` | `RosterPeriod : RemoveShift` | `RemovePublishedShiftRequest` | `ShiftRevisionResponse` | 200, 400, 403, 404, 409 |
| `GET` | `/{id}/revisions` | `RosterPeriod : Read` | Query `fromDate`, `toDate`, `workforceProfileId`, `changeType`, `pageNumber`, `pageSize` (maks. 100) | `PagedResult<ShiftRevisionResponse>` | 200, 400, 403, 404 |

Contoh tambah shift pada roster terbit:

```json
{
  "workforceProfileId": "…Tono…", "positionId": "…",
  "shifts": [ { "shiftDate": "2027-02-20", "shiftId": "…Pagi…" } ],
  "reasonCategory": "Kekurangan tenaga", "reasonNote": "Pasien ICU bertambah",
  "warningReasons": [ { "warningCode": "W6", "reasonCategory": "Kekurangan tenaga" } ]
}
```

Contoh hapus shift:

```json
{ "reasonCategory": "Ruang ditutup", "reasonNote": "Bed ICU 3 renovasi" }
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (59 uji: 45 sebelumnya + 14 baru) | 59/59 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\scopetest` |
| — `AT-HRD-PK-05`: roster tetap `Published` versi 3; satu riwayat `AddShift` (sebelum kosong, sesudah Tono/Pagi 07.00–14.00 WIB); shift `Published`; pegawai yang sudah ada memakai penugasan yang sama | Sesuai | `PASS` | Dua uji |
| — Tanpa alasan (dua kasus) → 400; `B3` dan `B1` → 422 berkode tanpa perubahan; dua shift bertabrakan dalam satu permintaan → 400; roster draf tetap perilaku lama | Sesuai | `PASS` | Lima uji |
| — `AT-HRD-PK-06`: tanpa alasan 400; dengan alasan → `IsCancel`, nonaktif, `Cancelled`, riwayat `RemoveShift` dengan nilai lama, hitungan penugasan berkurang, roster tetap `Published` versi 3; hapus ulang 404 | Sesuai | `PASS` | Satu uji |
| — Hapus shift yang baru diganti petugas (disela interceptor) → 409 tanpa riwayat; roster draf → 409; shift periode lain → 404 | Sesuai | `PASS` | Tiga uji |
| — Riwayat: terbaru di atas, label, nama; filter jenis, pegawai, tanggal; tanggal terbalik 400; pengguna di luar cakupan baca 403 | Sesuai | `PASS` | Satu uji |
| — `AT-HRD-PK-08`: tidak ada `PUT`/`PATCH`/`DELETE`/`POST` pada rute riwayat di controller Penjadwalan; string hak akses sesuai matriks 30.1 | Sesuai | `PASS` | Dua uji (refleksi) |
| Kontrol negatif: jalur roster terbit dimatikan sementara, backend dibangun ulang | Tepat 7 uji jalur terbit yang gagal. Kode dipulihkan dari cadangan (dibandingkan byte demi byte, CRLF terjaga), dibangun ulang, 59/59 lagi | `PASS` | Keluaran `dotnet test` |
| API devYoga (13 November 2026, roster `ROST-202611-0002`, terbit versi 1): `AT-HRD-PK-05` | Tanpa alasan 400; Dewi 4 Agustus → 422 `B8/BLOCKING`; dengan alasan → 201, shift `2027-02-20/Published`, `revisionIds` terisi; roster tetap `Published` versi 1; riwayat `AddShift` (sebelum kosong, Tono/Pagi 00.00Z–07.00Z) | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-06` | Tanpa alasan 400; dengan alasan 200 (`Hapus Shift`, Raka Firmansyah, Pagi (UJI-A52), oleh SuperAdmin); shift `IsCancel`, nonaktif, `Cancelled`; roster tetap `Published` versi 1; hapus ulang 404 | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-08` | `PUT`/`PATCH`/`DELETE` `revisions/{revId}` → 404; `PUT`/`DELETE`/`POST` `revisions` → 405 | `PASS` | `at-log.jsonl` |
| — `GET /{id}/revisions` | 200, 6 baris (4 Ganti Petugas dari `BE-HRD-338`, 1 Tambah Shift, 1 Hapus Shift), teratas Hapus Shift; filter `AddShift` → 1; tanggal terbalik → 400; periode tidak ada → 404 | `PASS` | `at-log.jsonl` |
| Log backend selama uji | Tanpa galat selain `AttendanceScheduler:SystemActorUserId` yang sudah ada | `PASS` | `be.log` sesi |
| Cakupan baca 403 dengan akun non-superadmin lewat API | — | `NOT RUN` | Butuh akun non-superadmin bersandi (sama dengan `AT-HRD-PK-09`/`-10`); terbukti di harness |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Status tetap `Published` | Terpenuhi | Harness dan API devYoga (tambah dan hapus) |
| Hapus tanpa alasan 400 | Terpenuhi | Harness dan API devYoga |
| Riwayat tidak punya endpoint ubah/hapus | Terpenuhi | Harness (refleksi) dan API devYoga (404/405) |
| Daftar riwayat mematuhi cakupan baca | Terpenuhi di harness | API butuh akun non-superadmin |
| `AT-HRD-PK-05`, `-06`, `-08` | Terpenuhi | Bagian 5 |
| DoD umum: build lulus, tanpa angka tertanam, catatan alasan tidak masuk log | Terpenuhi | Batas halaman 100 adalah batas teknis paginasi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru |
| Masalah yang diketahui | (1) **Terbit pada roster yang sudah `Published` masih gagal 500** (versi publikasi kembar): `PublishRosterPeriodAsync` tidak memeriksa status sudah terbit. Jalur yang biasa memicunya (tambah shift → `Draft` → terbit ulang) sudah tertutup oleh task ini, tetapi `populate-staff`, `copy-previous`, dan `apply-pattern` masih menurunkan roster terbit ke `Draft`, dan terbit ulang sesudahnya tetap 500. Belum ada task; perlu keputusan pemilik roadmap. (2) Tiga endpoint itu juga masih memakai `RosterAssignment : Create` yang tidak terdaftar (hanya superadmin). (3) **`INT-HR-101` untuk tambah, hapus, dan Ganti Petugas belum dijalankan** — kontrak integrasi menyebut keempat perubahan roster terbit memicu hitung ulang kehadiran, tetapi kartu `BE-HRD-338`/`340` tidak memuatnya; baru koreksi mundur (`BE-HRD-339`) yang memicunya. Perlu task tambahan. (4) Roster `ROST-202611-0003` di devYoga masih tertahan `Draft` dari sebelum perbaikan |
| Risiko tersisa | (1) Dua permintaan tambah shift serentak untuk pegawai dan jam yang sama dapat sama-sama lolos pemeriksaan (tidak ada pembaruan bersyarat untuk baris baru). (2) Shift yang dihapus tidak memicu hitung ulang kehadiran (butir 3 di atas). (3) Pemegang `RosterPeriod : Create` kini dapat menambah shift; sebelumnya tidak ada non-superadmin yang dapat |
| Perubahan sampingan | `NONE`. Satu `sed -i` gagal cocok sempat mengubah akhir baris `RosterPeriodService.cs` menjadi LF saat kontrol negatif; berkas dipulihkan dari cadangan dan dibandingkan byte demi byte |
| Interupsi | `NONE` |
| Status Git | 4 berkas source task ini (diubah) di atas perubahan `BE-HRD-339`; laporan ini; 2 dokumen roadmap/traceability. Tidak ada stage atau commit |
| Langkah berikutnya | `BE-HRD-341` — pemberitahuan dampak penempatan sementara ke Kepala Unit asal. Usulan task tambahan untuk Masalah 1–3 |

Data uji devYoga baru (bertanda `UJI-A52`): satu shift Tono 20 Februari 2027 di `ROST-202611-0002`,
kini berstatus batal, beserta dua baris riwayat (`AddShift`, `RemoveShift`).
