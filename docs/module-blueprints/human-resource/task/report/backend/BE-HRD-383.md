# Laporan Perubahan Backend — `BE-HRD-383`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-383` |
| Judul | Surat Tugas alur berjenjang: ajukan Kepala Unit, verifikasi HRD, pengesahan pejabat, terbit HRD, selesai atau batal |
| Slice | `S-FIN-ADM` / Surat Tugas (alias `ST-IMP (alur)`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.1, kartu `BE-HRD-383` |
| Trace | `02_ALUR_BISNIS_DAN_KEPUTUSAN.md` 3.2, 4.2, keputusan terkunci 1–2; baseline `BE-HRD-109`, `BE-HRD-194` |
| Contract version | Kontrak Administrasi Kepegawaian (`docs/update/administrasi-kepegawaian`); endpoint baru dicatat sebagai delta |
| Dependency | `BE-HRD-382` ✅ |
| Klasifikasi | `HEAVY` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `AssignmentLetterService`, `AssignmentLetterController`, DTO, penanda hak akses, `Program.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a7c37c56` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpetakan ke source; build, QBE, registri lulus; Migration `20261120093000_AddHrdAssignmentLetter` ditulis, sudah diterapkan 21 November 2026 ke DB dev `QuilvianNewDevHamzah` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `HrServiceManagement` |
| Keberlakuan | `NEW CODE` (Surat Tugas, registri) + `TOUCHED LEGACY` (Surat Keterangan) |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

`OfficialDutyRequestController` hanya punya `approve`/`reject` satu tahap untuk dinas luar presensi; tidak ada verifikasi HRD, pengesahan terpisah, penerbitan bernomor, maupun status pelaksanaan.

## 2. Proses bisnis

1. **Draf** (Kepala Unit, `Create`) → **Diajukan** (`Submit`, hanya pembuat).
2. **Verifikasi HRD** (`Verify`, bukan pembuat/pengaju): lulus → **Menunggu Pengesahan**; minta revisi (catatan wajib) → **Perlu Revisi** (pembuat memperbaiki lalu mengajukan ulang); tolak (alasan wajib) → **Ditolak**.
3. **Pengesahan** (`Endorse`, bukan pembuat/pengaju/verifikator): sah → **Disahkan** (nama & jabatan pengesah dibekukan sebagai penandatangan); tolak (alasan wajib).
4. **Penerbitan** (`Issue`) hanya dari **Disahkan**: nomor `ST-RSMMC-NNNNNN` dari `NumberSeriesAllocator`, naskah + QR (`BE-HRD-384`), penugasan presensi per peserta.
5. **Dilaksanakan** (`Report`, pembuat/peserta, sejak tanggal mulai) → **Selesai** dengan laporan hasil (wajib) dan rujukan bukti.
6. **Batal** (`Cancel`, alasan wajib) dari status sebelum terbit, atau sesudah terbit tetapi sebelum tanggal mulai — penugasan presensi ditandai `Cancelled` dan hari presensi terkait diproses ulang.
7. Benturan jadwal (shift, Surat Tugas lain aktif, dinas luar lain) ditampilkan sebagai peringatan per peserta, tidak memblokir.
8. Setiap aksi dikunci per surat (transaksi + baca ulang); transisi tidak sah `409`, aktor salah `403`, validasi `422`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OfficialDutyRequestService.cs`, `TrxShiftAssignment`, `HrdAttendanceDaily`, `HrdUnitScopeService.cs`, dokumen alur bisnis Administrasi Kepegawaian.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/HrServiceManagement/Services/AssignmentLetterService.cs` | State machine, cakupan baca, aktor terpisah, benturan jadwal, penerbitan, pelaksanaan, pembatalan |
| `Areas/Corporate/HumanResource/HrServiceManagement/Controllers/AssignmentLetterController.cs` | 12 endpoint; pemetaan `403`/`404`/`409`/`422` |
| `Areas/Corporate/HumanResource/HrServiceManagement/DTOs/AssignmentLetterDtos.cs` | DTO + status + aksi tersedia |
| `Areas/Corporate/HumanResource/HrServiceManagement/AssignmentLetterExplicitPermissions.cs` | Penanda `ManageAllUnits` |
| `Program.cs` | Registrasi service |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `api/v1/corporate/human-resource/assignment-letters` (12 aksi) |
| Database | Bergantung migration `20261120093000` |
| Keamanan/Auth | 9 aksi baru di Akses Role (`Read`, `Create`, `Update`, `Submit`, `Verify`, `Endorse`, `Issue`, `Report`, `Cancel`) + penanda `ManageAllUnits` |

## 4. Dokumentasi endpoint

#### HR Services - Assignment Letters (Surat Tugas)

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/assignment-letters` | Daftar sesuai cakupan (filter status/tanggal/cari) | `AssignmentLetter : Read` |
| `GET` | `/api/v1/corporate/human-resource/assignment-letters/{id}` | Detail + peserta + peringatan benturan + jejak aktor + aksi tersedia | `AssignmentLetter : Read` |
| `GET` | `/api/v1/corporate/human-resource/assignment-letters/{id}/document` | Naskah terbit (HTML) | `AssignmentLetter : Read` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters` | Susun draf | `AssignmentLetter : Create` |
| `PUT` | `/api/v1/corporate/human-resource/assignment-letters/{id}` | Ubah draf / revisi | `AssignmentLetter : Update` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters/{id}/submit` | Ajukan ke HRD | `AssignmentLetter : Submit` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters/{id}/verify` | Verifikasi HRD (`Approve`/`RequestRevision`/`Reject`) | `AssignmentLetter : Verify` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters/{id}/endorse` | Pengesahan pejabat (`Endorse`/`Reject`) | `AssignmentLetter : Endorse` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters/{id}/issue` | Terbitkan bernomor + naskah + QR | `AssignmentLetter : Issue` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters/{id}/start-execution` | Mulai pelaksanaan | `AssignmentLetter : Report` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters/{id}/complete` | Selesai + laporan hasil | `AssignmentLetter : Report` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters/{id}/cancel` | Batal berjustifikasi | `AssignmentLetter : Cancel` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error; himpunan warning (tanpa nomor baris) sama dengan sebelum perubahan — tambahan 6 instans berasal dari commit pengguna `a7c37c56` pada `HrdInitialPlacementBatchService.cs`, bukan dari perubahan ini | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (18 berkas slice Administrasi Kepegawaian) | 18 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: verifikator = pengaju | `403` | `PASS` | `VerifyAsync` |
| Penelusuran: terbit sebelum disahkan | `409` | `PASS` | `IssueAsync` |
| Penelusuran: minta revisi tanpa catatan | `422` | `PASS` | `RequireText` |
| Validator registri hak akses | `IsValid=True` (2.660 identitas) | `PASS` | skrip `dotnet fsi` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Pengaju, verifikator, pengesah, penerbit tercatat terpisah; revisi/tolak wajib alasan | Terpenuhi | kolom aktor per tahap + larangan aktor sama + `RequireText` |
| (2) Tidak ada penerbitan/nomor sebelum disahkan | Terpenuhi | nomor dialokasikan hanya di `IssueAsync` dari status Disahkan |
| (3) Kepala Unit hanya memilih bawahan; benturan jadwal sebagai peringatan | Terpenuhi | cakupan unit + `ComputeConflictsAsync` |
| (4) Status pelaksanaan dan bukti hasil tersimpan; transisi tidak sah `409` | Terpenuhi | `StartExecutionAsync`/`CompleteAsync`; `HrServiceConflictException` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Asumsi | Pengesahan memakai hak `AssignmentLetter : Endorse` (matriks kewenangan dikonfigurasi admin di Akses Role); pembatalan sesudah terbit hanya sebelum tanggal mulai |
| Dampak frontend | Layar Surat Tugas perlu pindah dari `official-duties` (satu pegawai) ke `assignment-letters` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |


## Pembaruan 21 November 2026 — Status Migration

Migration `20261120093000_AddHrdAssignmentLetter` sudah diterapkan ke database pengembangan `QuilvianNewDevHamzah`: pemilik menjalankan `dotnet ef database update --no-build` ("No migrations were applied. The database is already up to date.") dan `dotnet ef migrations list --no-build` terhadap `QuilvianNewDevHamzah` tidak menunjukkan satu pun migration `Pending`. Pernyataan "migration belum dijalankan" dan "wajib dijalankan sebelum dipakai" pada bagian di atas sudah tidak berlaku untuk DB dev tersebut; database lain (staging/production) tetap wajib menjalankannya sebelum fitur dipakai. Status task tetap ✅. Uji API di DB dev tetap `NOT RUN`.
