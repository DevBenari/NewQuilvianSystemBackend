# Laporan Perubahan Backend — `BE-HRD-355`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-355` |
| Judul | Tugas belajar (`POST-MVP`) |
| Slice | Amandemen `A52` — `EPIC HRD-117`, gelombang `POST-MVP` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B88 |
| Trace | `FR-PK-026`, `FR-PK-027`; `HRD-DEC-201` (uji a–d); `02-backend-architecture.md` 35.4.1–35.4.3, 35.9.4 butir 1; `api-contract.md` "Study Assignment"; `state-transition-matrix.md` 30.4; `validation-matrix.md` `VR-PK-18`, `VR-PK-19`, `VR-PK-02` (`B8`); `permission-audit-matrix.md` 30.1, 30.2, 30.5; `integration-contract.md` `INT-HR-102`; `AT-HRD-PK-12` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-343` ✅; `B8` sudah ada sejak `BE-HRD-337` ✅ |
| Klasifikasi | `HEAVY` — skor 8: satu repository (0), sekitar 20 berkas diperiksa (2), 3 berkas source baru + 2 diubah (1), logika inti pemrosesan kehadiran (3), pekerjaan latar (1), hak akses baru (1), tanpa schema (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services/StudyAssignmentService.cs,Controllers/StudyAssignmentController.cs,DTOs/StudyAssignmentDtos.cs}`, `Services/AttendanceProcessingService.cs`, `Program.cs`; proyek uji HR (`StudyAssignmentTests.cs` baru, `A52PermissionRegistryTests.cs`, `AcceptanceTestIndex.md`); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`) + perubahan `BE-HRD-357`, `353`, `354` yang belum di-commit |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — proyek uji HR 227/227 (termasuk kontrol negatif per putaran); `AT-HRD-PK-12` dengan data dari API baru, `VR-PK-18`, `VR-PK-19`, status harian "Tugas Belajar" lewat job hitung ulang, dan 403 lulus lewat API devYoga |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); entity `HrdStudyAssignment` sudah ada (`BE-HRD-334`), tidak ada entity baru |
| Keberlakuan | `NEW CODE` (service, controller, DTO) dan `TOUCHED LEGACY` (`AttendanceProcessingService`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-LOG-001`, `QBE-TXN-001` |
| Tidak berlaku | `QBE-CODE-*` (tanpa nomor dokumen), `QBE-DB-*` (tanpa schema) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Tabel `HrdStudyAssignment` dan pemeriksaan `B8` sudah ada, tetapi tanpa API pencatatan | Tugas belajar hanya bisa disisip langsung ke basis data |
| Pemrosesan harian tidak mengenal tugas belajar | Pegawai tugas belajar tercatat "Tidak Ada Scan" lalu Alpa saat periode ditutup, atau rekapnya galat karena tidak punya jadwal |

---

## 2. Proses bisnis

**Pencatatan (`HRD-DEC-201` butir 1–2, 5).** Admin HRD mencatat nomor SK, lembaga, program studi, tanggal mulai, dan tanggal selesai.
Tanggal selesai tidak boleh sebelum tanggal mulai (`VR-PK-19`, satu hari sah). Tugas belajar `Active` tidak boleh tumpang tindih
dengan tugas belajar `Active` lain pegawai yang sama (`VR-PK-18`, `422`). Mengubah data (kembali lebih cepat, diperpanjang) dan
membatalkan wajib alasan (`400` "Alasan perubahan wajib diisi"). Alasan terakhir, pengubah, dan waktunya tampil di respons. Tugas
belajar `Cancelled` tidak dapat diubah atau dibatalkan lagi (`409`).

**Roster (`B8`, butir 3).** Pemeriksaan `B8` dari `BE-HRD-337` membaca tabel yang sama, sehingga tugas belajar yang dicatat lewat API
langsung memblokir penjadwalan dan Ganti Petugas.

**Status harian (35.9.4 butir 1, butir 4).** Pada tanggal dalam masa tugas belajar `Active`, rekap harian berstatus "Tugas Belajar"
(`StudyAssignment`), mendahului semua status lain. Rekap ini bukan Alpa, tanpa menit terlambat atau pulang cepat, tanpa pengecualian
jadwal maupun scan hilang, dan tetap `Processed` walau tidak punya jadwal (karena `B8` mencegah penjadwalan). Saldo cuti tidak disentuh.
Penutupan periode hanya mengubah "Tidak Ada Scan"/"Belum Lengkap" menjadi Alpa, sehingga "Tugas Belajar" tidak terdampak.

**Hitung ulang (`INT-HR-102`).** Catat, ubah, dan batal menjadwalkan hitung ulang rekap harian yang sudah ada dan belum terkunci pada
rentang terdampak (satu job per rekap, `ATT-STUDY-<id>-<rekap>-v<versi>`). Untuk perubahan, rentangnya gabungan rentang lama dan baru,
sehingga hari yang keluar dari masa studi kembali dihitung biasa. Tanggal tanpa rekap tidak diproses di sini. Jumlah rekap yang
dijadwalkan dikembalikan di `ReprocessScheduled`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-201`; kontrak API "Study Assignment"; `state-transition-matrix.md` 30.4; `validation-matrix.md` `VR-PK-18`, `-19`; `permission-audit-matrix.md` 30.1, 30.2, 30.5; `data-dictionary.md` `HrdStudyAssignment`; `acceptance-test-matrix.md` `AT-HRD-PK-12`; traceability baris coverage gap "status harian tugas belajar tanpa ID uji"
- Model `HrdStudyAssignment` + konfigurasi (CHECK `EndDate >= StartDate`), `AttendanceValueConstants.StudyAssignmentStatus`, `RosterValidationService` (`B8`), `MyScheduleSelfService` (pembacaan tugas belajar), `AttendanceProcessingService`, `AttendanceSchedulerService.EnqueueAsync`, laporan `BE-HRD-337`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/StudyAssignmentService.cs` | **Baru.** Daftar (filter pegawai, `activeOn`, status, paginasi), detail, catat, ubah, batal; `VR-PK-18`, `VR-PK-19`; hitung ulang rekap terdampak |
| `Areas/Corporate/HumanResource/AttendanceManagement/Controllers/StudyAssignmentController.cs` | **Baru.** `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `POST /{id}/cancel`; `StudyAssignment : Read/Create/Update/Cancel`; audit `StudyAssignment.Create/Update/Cancel` |
| `Areas/Corporate/HumanResource/AttendanceManagement/DTOs/StudyAssignmentDtos.cs` | **Baru.** Request catat/ubah (`ChangeReason`)/batal (`CancelReason`), query, `StudyAssignmentResponse` (pencatat, pengubah, `ReprocessScheduled`) |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceProcessingService.cs` | Membaca tugas belajar `Active` per pegawai/tanggal; `ApplyStudyAssignment` (status, nol menit, bukan Alpa); tanpa pengecualian selain scan di luar jendela; `Processed` walau jadwal tidak terselesaikan; penanda "Tugas belajar" di pesan rekap. CRLF dipertahankan |
| `Program.cs` | Satu baris `AddScoped<StudyAssignmentService>()` (CRLF) |
| `Tests/QuilvianSystemBackend.HumanResourceTests/StudyAssignmentTests.cs` | **Baru.** 4 uji |
| `Tests/QuilvianSystemBackend.HumanResourceTests/A52PermissionRegistryTests.cs` | Controller tugas belajar masuk validator registri dan uji 403; empat resource/aksi masuk daftar |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AcceptanceTestIndex.md` | `AT-HRD-PK-12` menyebut bukti API `BE-HRD-355`; uji pendukung |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint sesuai kontrak `v26`. `CancelStudyAssignmentRequest` berisi `CancelReason`; respons menambah `ReprocessScheduled` |
| Database | Tanpa migration; menulis `HrdStudyAssignment` dan job `HrdAttendanceSchedulerJob` |
| Keamanan/Auth | Resource baru `StudyAssignment : Read/Create/Update/Cancel` (aksi `Cancel` bertipe akses `Update`, pola repository), terdaftar saat startup, belum diberikan ke jabatan mana pun. Matriks 30.2: hanya Admin HRD |
| Payroll | Hari tugas belajar tidak membawa pengecualian yang memblokir payroll dan bukan Alpa |

### 3.4 Delta dan keputusan desain

| Butir | Implementasi | Alasan |
| --- | --- | --- |
| Status "Tugas Belajar" pada hari libur/off dalam masa studi | Tetap "Tugas Belajar" | 35.9.4 menaruh tugas belajar di urutan pertama, sebelum hari libur dan off; pegawai roster tidak punya jadwal selama studi (`B8`), sehingga tanpa urutan ini harinya berstatus galat |
| Pegawai tugas belajar yang ternyata scan/datang | Tetap "Tugas Belajar", menit terlambat 0 | Urutan 35.9.4; keputusan menyebut pegawai dibebaskan dari pekerjaan |
| Konkurensi | Tanpa token konkurensi | `HrdStudyAssignment` tidak punya `RowVersion` di kamus data; satu-satunya pengguna adalah Admin HRD. Ubah/batal yang bersamaan: yang terakhir menang, keduanya tercatat di log audit |
| Riwayat perubahan | Hanya alasan terakhir di tabel; setiap catat/ubah/batal tercatat di log audit (pelaku, waktu, tanggal) | Kamus data hanya menyediakan `LastChangeReason` |
| Perubahan yang menyentuh periode ditutup | Tidak dilarang; rekap terkunci tidak dihitung ulang | Kontrak tidak memuat larangan; data periode ditutup tetap final |
| Uji status harian | `StudyAssignmentTests.Status_harian_tugas_belajar_dan_hitung_ulang` | Menutup coverage gap traceability "status harian tugas belajar tanpa ID uji tersendiri"; matriks uji perlu ID baru pada penyegaran blueprint |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/attendance/study-assignments` — Tag `Corporate / Human Resource / Attendance Management / Study Assignment`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | `StudyAssignment : Read` | Query `workforceProfileId`, `activeOn`, `status`, `pageNumber`, `pageSize` (maks. 100) | `PagedResult<StudyAssignmentResponse>` | `200`, `401`, `403` |
| `GET` | `/{id}` | `StudyAssignment : Read` | — | `StudyAssignmentResponse` | `200`, `401`, `403`, `404` |
| `POST` | `/` | `StudyAssignment : Create` | `CreateStudyAssignmentRequest` | `StudyAssignmentResponse` | `200`, `400` (`VR-PK-19`, data SK), `401`, `403`, `404`, `422` (`VR-PK-18`) |
| `PUT` | `/{id}` | `StudyAssignment : Update` | `UpdateStudyAssignmentRequest` (+ `ChangeReason` wajib) | `StudyAssignmentResponse` | `200`, `400`, `401`, `403`, `404`, `409` (dibatalkan), `422` |
| `POST` | `/{id}/cancel` | `StudyAssignment : Cancel` | `CancelStudyAssignmentRequest` (`CancelReason` wajib) | `StudyAssignmentResponse` | `200`, `400`, `401`, `403`, `404`, `409` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat; tidak ada peringatan dari berkas baru/diubah | `PASS` | Keluaran build |
| Proyek uji HR (`dotnet test`, SQLite di memori) | 227/227 lulus (214 sebelumnya + 4 uji tugas belajar + 4 resource + 5 endpoint di uji registri) | `PASS` | Keluaran `dotnet test` |
| — Catat dan validasi | Data SK, pencatat "Admin HRD"; `VR-PK-19` `400` pesan kontrak, satu hari sah; SK kosong `400`; pegawai tak dikenal `404`; `VR-PK-18` `422` pesan kontrak; bersebelahan sah; sesudah dibatalkan sah | `PASS` | `Catat_validasi_dan_tumpang_tindih` |
| — Ubah/batal (`HRD-DEC-201` uji (d)) | Tanpa alasan (kosong/spasi) `400`; menabrak tugas belajar lain `422`; tanggal terbalik `400`; ubah sah → alasan, pengubah, waktu; batal tanpa alasan `400`; batal sah; batal/ubah sesudah dibatalkan `409`; tidak dikenal `404` | `PASS` | `Ubah_dan_batal_wajib_alasan` |
| — Daftar | Filter `activeOn` (yang dibatalkan tidak ikut), status, pegawai; paginasi; detail | `PASS` | `Daftar_dan_filter` |
| — Status harian (`FR-PK-027`, uji (b)(c)) dan `INT-HR-102` | Dewi dijadwalkan 4 Feb, tidak datang → "Tidak Ada Scan"; SK dicatat → 1 job `ATT-STUDY-…`; diproses → `StudyAssignment`, bukan Alpa, tanpa pengecualian, `Processed`, layak payroll; tanggal tanpa shift → `StudyAssignment`, bukan galat; scan 07.50 → tetap `StudyAssignment`, 0 menit; tabel saldo cuti kosong; dipendekkan → 2 job, 4 Feb kembali dihitung biasa (`IncompletePunch`, 50 menit); dibatalkan di luar rekap → 0 job; rekap terkunci → 0 job | `PASS` | `Status_harian_tugas_belajar_dan_hitung_ulang` |
| — Lapis hak akses | Controller lolos `PermissionRegistryValidator`; empat resource/aksi terdaftar; lima endpoint menolak pegawai tanpa kebijakan Akses Role | `PASS` | `A52PermissionRegistryTests` |
| Kontrol negatif per putaran: A (alasan ubah + pemicu hitung ulang), B (cek tumpang tindih + status galat tanpa jadwal), C (status Tugas Belajar) | A: gagal `Ubah_…` dan `Status_…`; B: gagal `Catat_…`, `Ubah_…`, `Status_…`; C: gagal `Status_…` saja. Kode dipulihkan (dibandingkan byte demi byte) setiap putaran | `PASS` | `mut355.sh` dan keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2454 identitas kanonik"; empat aksi `StudyAssignment` tersimpan | `PASS` | `be355.log`; `at-log.jsonl` |
| API devYoga (17 November 2026) — tanpa sesi | `401` | `PASS` | `at-355.mjs`, `at-log.jsonl` (entri "355 …") |
| — `gilang.mahendra` (bukan Admin HRD) | Daftar dan catat `403` | `PASS` | `at-log.jsonl` |
| — `VR-PK-19` / `VR-PK-18` | `400` "Tanggal selesai harus sesudah tanggal mulai" / `422` terhadap tugas belajar Putri Handayani 1 Agt – 31 Des 2027 yang sudah ada | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-12` dengan data dari API baru | Tugas belajar Arif Maulana 1–10 Agt 2027 dicatat lewat API (filter `activeOn` 3 Agt memuatnya); Ganti Petugas Indra → Arif pada shift Pagi 3 Agt 2027 → `422` "VR-PK-02 (B8): Arif Maulana sedang tugas belajar 1–10 Agustus 2027."; shift dan riwayatnya tidak berubah | `PASS` | `at-log.jsonl` |
| — Status harian gilang hari ini | Sebelum: `NotYetPresent`; catat SK 17 Nov → `ReprocessScheduled` 1, job `ATJ-20261117-000003` `Completed` → `StudyAssignment`, `Processed`, 0 pengecualian, pesan "Tugas belajar" | `PASS` | `at-log.jsonl` |
| — Ubah dan batal | Tanpa alasan `400`; dengan alasan → selesai 18 Nov, alasan, pengubah "SuperAdmin"; batal tanpa alasan `400`; batal → `Cancelled`; batal ulang `409`; job `ATJ-20261117-000004` `Completed` → rekap kembali `NotYetPresent` | `PASS` | `at-log.jsonl` |
| — Audit | 5 baris `[AUD]` untuk 5 perubahan berhasil (2 catat, 1 ubah, 2 batal termasuk pembersihan) | `PASS` | `be355.log` |
| — Pemulihan | Dua tugas belajar uji (`UJI-A52-355`) `Cancelled` dan dihapus lunak; rekap gilang akhir `NotYetPresent`; tidak ada izin yang diubah | `PASS` | `at-log.jsonl` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tumpang tindih ditolak (`VR-PK-18`) | Terpenuhi | Harness dan devYoga |
| Ubah/batal wajib alasan (`VR-PK-19`, `HRD-DEC-201` uji (d)) | Terpenuhi | Harness dan devYoga |
| Saldo cuti tidak berkurang (uji (c)) | Terpenuhi | Harness (tabel saldo cuti kosong); pemrosesan tidak menyentuh cuti |
| `AT-HRD-PK-12` dengan data nyata | Terpenuhi | devYoga: data dari API baru, `B8` menolak |
| Status harian "Tugas Belajar" (uji (b)) | Terpenuhi | Harness dan devYoga |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) Empat aksi `StudyAssignment` belum diberikan ke jabatan mana pun; Admin HRD perlu diberi lewat Role Access. (2) Tugas belajar Putri Handayani 1 Agt – 31 Des 2027 di devYoga masih ada dari `BE-HRD-337` (data uji lama, tidak disentuh). (3) Matriks uji perlu ID untuk status harian tugas belajar pada penyegaran blueprint |
| Risiko tersisa | Tanpa token konkurensi; perubahan lewat jendela studi panjang yang sudah berjalan menjadwalkan satu job per rekap yang ada |
| Perubahan sampingan | `NONE`. Data uji devYoga sudah dibatalkan dan dihapus lunak |
| Interupsi | `NONE` |
| Status Git | 3 berkas source baru + 2 diubah; 1 uji baru + 2 berkas uji diubah; laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-357`, `353`, `354` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-356` (gangguan mesin); sesudahnya `BE-HRD-358`. Frontend: `FE-HRD-319` |
