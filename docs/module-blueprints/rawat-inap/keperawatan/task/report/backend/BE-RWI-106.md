# Laporan Perubahan Backend — `BE-RWI-106`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-106` |
| Judul | Lima perbaikan keselamatan pengkajian (migration K0) |
| Slice | Gelombang 1 — `KEP-V2-0` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-106` |
| Trace | `FR-KEP-038`; `RLN3-CAP-17`, `18`, `21`, `22`, `23`, `29`; `RLN3-CON-02` s.d. `04`; api-contract 0.5.0 bagian 7.1 |
| Contract version | `0.5.0` — roadmap `APPROVED` lewat `RWI-DEC-150` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — satu controller legacy, satu berkas DTO, nol perubahan bentuk data |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement` (controller dan DTO pengkajian); dokumentasi blueprint `keperawatan` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`, upstream `origin/MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — keenam kriteria terpetakan; `dotnet build` lolos (0 error, 212 warning); verifikasi kontrak API runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY` |
| Keberlakuan | `TOUCHED LEGACY` — `PatientAssessmentController` (legacy `Trx*`, controller memakai `ApplicationDbContext`); tidak ditulis ulang, hanya diperluas |
| QBE relevan | `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001` |
| Hak akses baru | Tidak ada. Endpoint baru memakai `PatientAssessment : Read` |
| Database | Nol perubahan (K0 = kode saja) |

## 1. Masalah yang diperbaiki

Impact scan `RLN-PH-03` menemukan lima cacat. Yang paling berbahaya: layar keperawatan mengirim
`consciousnessStatus || 1`, `appetiteStatus || 1`, dan `functionalStatus || 1`. Angka `1` di backend berarti
Compos Mentis, nafsu makan normal, dan mandiri — sehingga isian yang **tidak pernah dikaji** tersimpan
sebagai **pemeriksaan dengan hasil baik**. Pada saat yang sama backend menerima angka enum apa pun:
pilihan "Ketergantungan Berat" tersimpan sebagai angka `4` yang tidak dikenal, dan penyuntingan yang
berangkat dari baris daftar dapat mengosongkan belasan isian yang tidak ikut tampil di daftar.

## 2. Proses bisnis

1. Layar ruang kerja keperawatan membaca **daftar nilai sah** dari `GET /patient-assessments/filters/metadata`.
   Setiap enum membawa angka, kode, dan label Indonesia; nilai `0` selalu berarti **belum dikaji**.
2. Ns. Siti membuat pengkajian. Angka enum di luar daftar — misalnya status fungsional `4` — ditolak `400`
   dengan kalimat "Nilai status fungsional (4) tidak dikenal. Nilai yang sah: 0 = Unknown, 1 = Independent,
   2 = NeedPartialAssistance, 3 = FullyDependent."
3. Isian yang tidak dikirim tetap bernilai `0` (belum dikaji). Tidak ada satu pun jalur backend yang
   mengubah `0` menjadi nilai normal.
4. Ns. Siti membuka daftar pengkajian Budi (`GET /episodes/{episodeId}`) — **berpaginasi**, paling banyak 100
   baris, dan kini dapat disaring `assessmentStatus`.
5. Sebelum menyunting, layar membaca detail (`GET /{id}`) dan menerima `UpdateDateTime`. Kiriman `PUT /{id}`
   wajib membawa nilai itu sebagai `ExpectedUpdateDate`.
6. **Jalur tidak normal:**
   - Tanpa `ExpectedUpdateDate` pada pengkajian keperawatan rawat inap → `400` kode `DETAIL_NOT_LOADED`.
   - Ns. Rina menyimpan pukul 09.02; kiriman Ns. Siti pukul 09.05 masih membawa jejak 08.55 → `409` kode
     `STALE_ASSESSMENT`, perubahan Rina tidak tertimpa.
   - Perawat unit lain → `403` kode `NURSE_UNIT_NOT_ASSIGNED`; akun tanpa pegawai → `403` kode
     `NURSE_NOT_LINKED_TO_EMPLOYEE`; bukan penulis konsep → `403` kode `NOT_DOCUMENT_AUTHOR`. Sesi habis tetap
     `401`, dan butir hak akses yang tidak dicentang tetap `403` dari `AccessPermissionFilter` tanpa kode —
     layar dapat membedakan "minta admin" dari "minta kepala ruangan".
7. Poliklinik, medical check-up, IGD, dan kajian medis tidak berubah: `ExpectedUpdateDate` boleh tidak dikirim.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientAssessmentController.cs`, `PatientAssessmentDtos.cs`, seluruh enum pengkajian,
`Filters/AccessPermissionFilter.cs`, `Responses/ApiResponse.cs`, `01-existing-capability-map.md` bagian 17
(`RLN3-CAP-17` s.d. `29`), api-contract 0.5.0 bagian 7.1.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | `GET filters/metadata`; validasi enum pada buat dan ubah; saringan `assessmentStatus` pada daftar per episode; penjaga `ExpectedUpdateDate`; kode alasan `403`/`409`/`400` pada isian `errors.code`; `UpdateDateTime` pada response |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs` | `UpdateDateTime` pada response; `ExpectedUpdateDate` pada request ubah; `PatientAssessmentMetadataResponse` dan `PatientAssessmentEnumOptionResponse` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: satu endpoint baca, satu query `assessmentStatus`, satu isian response, satu isian request. **Perubahan perilaku:** `PUT /{id}` pengkajian keperawatan rawat inap kini menuntut `ExpectedUpdateDate` |
| Database | `NOT APPLICABLE` — nol migration |
| Keamanan/Auth | Tidak ada butir hak akses baru; isian `errors.code` hanya memuat kode alasan, bukan data pasien |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Daftar nilai sah seluruh enum pengkajian beserta label; `NotAssessedValue = 0` | `PatientAssessment : Read` |
| `GET` | `/episodes/{episodeId}` | Daftar berpaginasi; saringan baru `assessmentStatus` | `PatientAssessment : Read` |
| `PUT` | `/{id}` | Wajib `ExpectedUpdateDate` untuk pengkajian keperawatan rawat inap | `PatientAssessment : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–6 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| Review diff dan scope | Hanya dua berkas di atas; line ending mengikuti `.gitattributes` | `PASS` | `git diff --stat` sesi ini |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Verifikasi kontrak API runtime (Swagger) | Tidak dijalankan | `NOT RUN` | Build lolos; belum dijalankan pada putaran ini |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi tidak dibangun).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Enum sama persis frontend–backend, daftar sahnya satu sumber | Terpenuhi (sisi backend) | `GetMetadata` + `TemukanEnumTidakSah`/`PeriksaEnum` pada buat dan ubah. Penyelarasan layar milik task frontend `FE-RWI` gelombang `KEP-V2-0` |
| 2. Daftar per episode berpaginasi | Terpenuhi | `GetByEpisode` — `NormalizePaging` (maks 100), `Skip/Take`, saringan `assessmentStatus` |
| 3. Isian belum dikaji tersimpan sebagai belum dikaji | Terpenuhi (sisi backend) | Seluruh enum request berbawaan `Unknown = 0`; metadata `NotAssessedValue = 0`; tidak ada konversi `0` → normal. Pengiriman `|| 1` di layar diperbaiki task frontend |
| 4. Penyuntingan menolak kiriman tanpa pembacaan detail | Terpenuhi | `PeriksaBacaDetailSebelumSunting` — `400 DETAIL_NOT_LOADED`, `409 STALE_ASSESSMENT` |
| 5. `401` dan `403` dibedakan | Terpenuhi | `AccessPermissionFilter` (401/403 tanpa kode) + `errors.code` pada 403 kewenangan data |
| 6. Regresi: pengkajian lama terbaca apa adanya | Terpenuhi (statis) | Seluruh perubahan aditif; tidak ada kolom maupun mapping yang dihapus |
| DoD: laporan menyebut kelima perbaikan | Terpenuhi | Bagian 2 dan tabel ini |
| DoD: `dotnet build` | `dotnet build` lolos | `PASS` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kontrak 7.1 menamai query `status`/`page`; source memakai `assessmentStatus`/`pageNumber` mengikuti endpoint yang sudah ada — delta kontrak dicatat. Standar endpoint transaksi memakai `POST /{id}/<aksi>`, sedangkan grup ini sudah memakai `PATCH /{id}/complete` sejak sebelum task; tidak diubah |
| Masalah yang diketahui | Layar yang belum diperbarui dan menyunting tanpa `ExpectedUpdateDate` akan menerima `400` — disengaja, mencegah isian terhapus |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu interupsi pengguna saat penelusuran; dilanjutkan dari kondisi terverifikasi, nol penyuntingan ganda |
| Status Git | `M` pada dua berkas source di atas; laporan ini `??` |
| Langkah berikutnya | `BE-RWI-107` dan `BE-RWI-114`; frontend menyelaraskan enum dan `ExpectedUpdateDate` |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |
