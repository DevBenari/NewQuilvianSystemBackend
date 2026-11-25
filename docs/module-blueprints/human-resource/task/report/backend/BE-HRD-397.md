# Laporan Perubahan Backend — `BE-HRD-397`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-397` |
| Judul | Otorisasi tingkat record untuk dokumen personal rahasia |
| Slice | `S-FIN-M04` / Dokumen Personal (alias `FIN-HRD-04-BE-011`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-397` |
| Trace | `HRD-DEC-053`; instruksi Menu 04 `FIN-HRD-04-BE-011` |
| Contract version | Kontrak dokumen Workforce Core existing |
| Dependency | — |
| Klasifikasi | `MEDIUM` — controller, penanda hak akses baru, middleware penyajian berkas |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Controllers/WfpDocumentController.cs`, `WfpEducationController.cs`, `WorkforceCore/WorkforceDocumentExplicitPermissions.cs` (baru), `Program.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build, QBE, dan validator registri hak akses lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` + `NEW CODE` (penanda hak akses) |
| QBE yang berlaku | QBE-PERM-001, QBE-API-001, QBE-DTO-001 |
| Aturan akses | `role-access-rules.md`: hak dari layar Akses Role, tanpa nama peran di kode |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Dokumen personal bertanda rahasia (mis. surat keterangan sehat, dokumen pajak) hanya untuk HR berwenang dan pemiliknya. Detail dan unduhan sudah memeriksa kerahasiaan, tetapi:

1. **Hak ditentukan dari nama peran di kode.** Pemeriksaan memakai `SuperAdmin`, `Admin`, `HRAdmin`, `HRManager` dan klaim teks — dilarang aturan akses, dan peran HR lain yang dibuat admin tidak dapat diberi hak ini.
2. **Daftar membocorkan tautan berkas.** `GET` daftar mengembalikan `FilePath`/`FileUrl` dokumen rahasia ke siapa pun yang punya hak baca daftar.
3. **Berkas terbuka tanpa login.** Seluruh folder unggahan disajikan statis di `/uploads/...`; siapa pun yang tahu tautannya dapat membuka dokumen dan ijazah tanpa login.

## 2. Proses bisnis

1. Admin mencentang **"View Confidential Workforce Document"** (`WorkforceDocument : ViewConfidential`) untuk peran HR yang berwenang di Pengaturan → Manajemen Role → Akses Role.
2. Daftar dokumen: dokumen rahasia tetap terlihat (jenis, nama, tanggal), tetapi nomor, penerbit, keterangan, dan tautan berkas disamarkan bagi pemanggil yang bukan pemilik dan tidak punya hak di atas.
3. Detail, unduhan, ubah, dan verifikasi dokumen rahasia: pemilik dokumen atau pemegang hak → lanjut; selain itu `403 CONFIDENTIAL_DOCUMENT_RESTRICTED`.
4. Tautan berkas (`FileUrl`) selalu menunjuk endpoint unduh `GET …/documents/{id}/file` (dan `…/educations/{id}/file` untuk ijazah). Frontend membuka tautan itu di tab baru; cookie login ikut terkirim, sehingga pemeriksaan hak tetap berjalan.
5. Permintaan langsung ke `/uploads/human-resource/workforce/**` dibalas `404`.

**Contoh.** Kepala ruang tanpa hak `ViewConfidential` membuka daftar dokumen perawat C: dokumen "Surat Keterangan Sehat" tampil tanpa nomor dan tautan; membuka detailnya → `403`. Perawat C sendiri → berhasil.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpDocumentController.cs` (semua aksi, `MapResponse`, `CanAccessConfidentialDocumentAsync`), `WfpEducationController.cs`, `Program.cs` (`UseStaticFiles` `/uploads`), `AccessExplicitPermissionAttribute.cs`, `WorkflowDelegationExplicitPermissions.cs` (pola rujukan), `AccessPermissionService.cs`, `PermissionRegistryDescriptor.cs`/`Validator.cs`, produsen dokumen lain (`SpkRkkIssuanceService`, `EmployeeDocumentIssuanceService` — menyimpan di luar `/uploads`), frontend `workforce-profile-dashboard.jsx` (`normalizeFilePreviewUrl`, `window.open`), autentikasi JWT dari cookie di `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/WorkforceDocumentExplicitPermissions.cs` (baru) | Penanda `[assembly: AccessExplicitPermission]` `WorkforceDocument : ViewConfidential` |
| `WorkforceCore/Controllers/WfpDocumentController.cs` | Hak rahasia = pemilik atau `HasAccessAsync(..., ViewConfidential)`; penyamaran daftar; `FileUrl` ke endpoint unduh; penjaga rahasia pada `PUT` dan `/verify`; cek profil tidak mensyaratkan profil aktif |
| `WorkforceCore/Controllers/WfpEducationController.cs` | `FileUrl` ke endpoint unduh |
| `Program.cs` | Middleware `404` untuk `{PublicRequestPath}/human-resource/workforce/**` sebelum `UseStaticFiles` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap. `FileUrl` dokumen dan ijazah kini path API relatif; field nomor/penerbit/keterangan/berkas dokumen rahasia `null` bagi pemanggil tanpa hak |
| Database | Tanpa migration. Baris `SysActionAccess` untuk penanda baru dibuat seeder akses saat startup (mekanisme existing) |
| Keamanan/Auth | **Perubahan perilaku:** peran HR yang dulu lolos karena namanya kini perlu dicentang hak `ViewConfidential` oleh admin. Berkas pegawai tidak lagi terbuka tanpa login |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Document

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/documents` | Daftar; dokumen rahasia disamarkan tanpa hak | `WorkforceDocument : Read` (+ `ViewConfidential` untuk isi rahasia) |
| `GET` | `…/documents/{id}` | Detail; rahasia → pemilik/`ViewConfidential` | `WorkforceDocument : Read` |
| `GET` | `…/documents/{id}/file` | Unduh berkas | `WorkforceDocument : Read` |
| `PUT` | `…/documents/{id}` | Ubah; rahasia → pemilik/`ViewConfidential` | `WorkforceDocument : Update` |
| `PATCH` | `…/documents/{id}/verify` | Verifikasi; rahasia → `ViewConfidential` | `WorkforceDocument : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 (termasuk berkas penanda baru dan `Program.cs`) | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Validator registri hak akses atas DLL hasil build (`PermissionRegistryDescriptor.BuildFromAssembly` + `PermissionRegistryValidator.Validate`, skrip `dotnet fsi` di luar repository) | `IsValid=True`, 2.635 identitas, 0 temuan; `WorkforceDocument.ViewConfidential (Read)` terdaftar | `PASS` | Keluaran skrip |
| Penelusuran: daftar oleh non-pemilik tanpa hak | Field rahasia `null` | `PASS` (penelusuran) | `MapResponse(masked)` |
| Penelusuran: `GET /uploads/human-resource/workforce/...` | `404` | `PASS` (penelusuran) | middleware `Program.cs` |
| Startup aplikasi + uji API di DB dev | Belum dijalankan | `NOT RUN` | Backend lokal pengguna sedang berjalan; wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `GetDocumentById` dan `DownloadDocumentFile` memeriksa kerahasiaan | Terpenuhi | Keduanya memanggil `CanAccessConfidentialDocumentAsync` (kini berbasis hak, bukan nama peran) |
| (2) Tidak ada URL file statis yang melewati otorisasi | Terpenuhi | `FileUrl` ke endpoint unduh; jalur statis subfolder pegawai `404`; daftar menyamarkan tautan dokumen rahasia |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Admin perlu mencentang `View Confidential Workforce Document` untuk peran HR yang berwenang; tanpa itu hanya pemilik dokumen (dan SuperAdmin) yang dapat membuka dokumen rahasia |
| Masalah yang diketahui | Berkas lama yang tersimpan di luar `human-resource/workforce/**` (bila ada) tidak terlindungi middleware ini |
| Risiko tersisa | Bukti runtime (startup + uji `403`/`404`) belum ada |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-398` |
