# Laporan Perubahan Backend — `BE-HRD-381`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-381` |
| Judul | Surat Keterangan: pegawai penerima, kewenangan pengaju, dan identitas RS dari pengaturan |
| Slice | `S-FIN-ADM` / Surat Keterangan (alias `SKT-IMP (BUG-HRD-ADM-01)`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.1, kartu `BE-HRD-381` |
| Trace | `BUG-HRD-ADM-01`; baseline `BE-HRD-200`, `201`, `202`, `205` |
| Contract version | Kontrak Administrasi Kepegawaian (`docs/update/administrasi-kepegawaian`); endpoint baru dicatat sebagai delta |
| Dependency | — |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `EmployeeDocumentIssuanceService`, kontrak service, `EmployeeDocumentIssuanceController`, DTO verifikasi publik, `HrLetterheadResolver.cs` (baru), `EmployeeDocumentIssuanceExplicitPermissions.cs` (baru); laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a7c37c56` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build, QBE, registri lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `HrServiceManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Kriteria (1) sudah terpenuhi (pegawai tujuan dipakai untuk data, slip gaji, nama berkas). Belum: server tidak memeriksa kewenangan pengaju atas pegawai tujuan (Kepala Unit bisa memohon untuk pegawai unit lain); `IssuingAuthority`, kop template bawaan, kota ("Jakarta"), alamat verifikasi (`simrs.quilvian.co.id`), pesan verifikasi publik, dan nilai bawaan DTO masih memuat nama "Rumah Sakit Quilvian"; unduhan dan aksi C&B/terbit memakai nama peran tertanam (`IsInRole`, `[Authorize(Roles=…)]`).

## 2. Proses bisnis

1. Pengajuan (`POST {DOC}/request`): identitas pengaju dari token. Boleh untuk diri sendiri; untuk pegawai lain hanya bila pegawai itu ditempatkan (hari ini) di unit yang dipimpin pengaju beserta turunannya, atau pengaju memegang penanda baru `EmployeeDocumentIssuance : AccessAllEmployees`. Selain itu `403`, tanpa data tersimpan.
2. Penerbitan: nama, alamat, kontak, dan kota RS diambil dari Master Lokasi Rumah Sakit (lokasi utama aktif). Bila belum diatur, penerbitan ditolak dengan pesan jelas — tidak ada identitas karangan. `IssuingAuthority` arsip = nama RS dari pengaturan.
3. Alamat QR: `HumanResource:DocumentVerificationBaseUrl` bila diatur, selain itu endpoint API publik `/api/v1/public/hr-documents/verify/{hash}` di bawah `FileStorage:PublicBaseUrl`.
4. Verifikasi publik: nama penerbit dari arsip dokumen saat terbit; pesan tanpa nama RS tertanam.
5. Unduhan "semua pegawai" dan aksi C&B/terbit/cabut kini hanya bergantung pada hak Akses Role (nama peran tertanam dihapus).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EmployeeDocumentIssuanceService.cs`, `EmployeeDocumentIssuanceController.cs`, `HrPublicDocumentVerificationController.cs`, `MstHospitalSite`, `appsettings.json`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs` | `CanActForEmployeeAsync`; identitas RS dari pengaturan; token `[NAMA_RS]`/`[ALAMAT_RS]`/`[KONTAK_RS]`/`[KOTA_RS]`; URL verifikasi dari konfigurasi; pesan verifikasi tanpa nama tertanam |
| `Areas/Corporate/HumanResource/HrServiceManagement/Services/HrLetterheadResolver.cs` | Resolver kop + URL verifikasi bersama (dipakai juga Surat Tugas) |
| `Areas/Corporate/HumanResource/HrServiceManagement/EmployeeDocumentIssuanceExplicitPermissions.cs` | Penanda `AccessAllEmployees` (baru) |
| `Areas/Corporate/HumanResource/HrServiceManagement/Services/IEmployeeDocumentIssuanceService.cs` | Parameter `ClaimsPrincipal? user` opsional |
| `Areas/Corporate/HumanResource/HrServiceManagement/Controllers/EmployeeDocumentIssuanceController.cs` | `403` pengaju tanpa kewenangan; `IsHrStaff()` diganti penanda; atribut `Roles` dihapus pada cb-approve, authorize-issue, revoke |
| `Areas/Corporate/HumanResource/HrServiceManagement/DTOs/EmployeeDocumentIssuanceDtos.cs` | Nilai bawaan `IssuedByHospital` kosong |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; pengajuan di luar kewenangan → `403`; penerbitan tanpa Master Lokasi RS → `400` berpesan |
| Database | Tanpa migration |
| Keamanan/Auth | Penanda baru `Manage Employee Letters For All Employees`; aksi yang dulu dibatasi nama peran kini mengikuti centang Akses Role |

## 4. Dokumentasi endpoint

#### HR Services - Document Issuance & Verification

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/hrd/services/documents/request?workforceProfileId=` | Ajukan surat (cek kewenangan) | `EmployeeDocumentIssuance : Request` (+ `AccessAllEmployees`) |
| `POST` | `/api/v1/corporate/hrd/services/documents/{id}/authorize-issue` | Terbitkan (kop dari pengaturan) | `EmployeeDocumentIssuance : Issue` |
| `GET` | `/api/v1/corporate/hrd/services/documents/{id}/download` | Unduh | `EmployeeDocumentIssuance : Download` (+ `AccessAllEmployees`) |
| `GET` | `/api/v1/public/hr-documents/verify/{hash}` | Verifikasi publik | anonim |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error; himpunan warning (tanpa nomor baris) sama dengan sebelum perubahan — tambahan 6 instans berasal dari commit pengguna `a7c37c56` pada `HrdInitialPlacementBatchService.cs`, bukan dari perubahan ini | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (18 berkas slice Administrasi Kepegawaian) | 18 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: Kepala Unit memohon untuk pegawai unit lain | `403` | `PASS` | `CanActForEmployeeAsync` |
| Penelusuran: grep "Quilvian Utama"/"RS Quilvian" pada HrServiceManagement | Tidak ada | `PASS` | grep 20 November 2026 |
| Validator registri hak akses | `IsValid=True` (2.660 identitas) | `PASS` | skrip `dotnet fsi` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Data pegawai, slip gaji, nama berkas memakai pegawai tujuan | Terpenuhi | `targetProfile` (audit 19 Nov) |
| (2) Pengaju tanpa kewenangan ditolak `403` | Terpenuhi | cek kewenangan di service + pemetaan controller |
| (3) Tidak ada nama RS tertanam | Terpenuhi | kop/penerbit/pesan dari Master Lokasi RS; DTO tanpa nilai bawaan |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Prasyarat data | Master Lokasi Rumah Sakit (lokasi utama) harus berisi nama, alamat, kontak, kota |
| Peringatan untuk admin | Centang `Manage Employee Letters For All Employees` untuk staf HRD; periksa peran yang perlu `ApproveDraft`/`Issue`/`Revoke` karena pembatasan nama peran dihapus |
| Temuan di luar lingkup | `HrServiceCatalogController` dan `HrServiceTicketController` masih memakai `[Authorize(Roles=…)]` — dicatat, tidak diubah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
