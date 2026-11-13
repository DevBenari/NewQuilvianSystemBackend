# Laporan Perubahan Backend — `BE-PBJ-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-004` |
| Judul | Lampiran |
| Slice | `MVP-0` — fondasi (`EPIC PBJ-03`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-004` |
| Trace | `PBJ-DEC-023`; FR-PBJ-020..021; `PBJ-VAL-080`..`083`; `AT-PP-53`..`56` |
| Contract version | `PBJ-PP-API` 1.1 (bagian Attachment tidak berubah dari 1.0), `PBJ-PP-PERM` 1.2 |
| Dependency | `BE-PBJ-002` 🟡 |
| Klasifikasi | `MEDIUM` (logika 1, kontrak API 1, database 2, keamanan 2) |
| Task mode | `BACKEND` (diberikan pemilik 12 November 2026, bersama `BE-PBJ-003`) |
| Target tulis | `Areas/Corporate/ProcurementManagement/Common/**`, konfigurasi EF, `ApplicationDbContext.cs`, extension DI, migration `AddProcurementAttachment`; basis data dev tidak disentuh |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `100c011d` (belum di-commit) |
| Tanggal | 12 November 2026 |
| Status | 🟡 **SEBAGIAN.** Kode dan migration ada; build dan QBE `PASS`. Belum: uji runtime `AT-PP-53`..`56`. Selain migration yang belum diterapkan, lampiran baru dapat dipakai setelah task dokumen pertama (`BE-PBJ-006`) mendaftarkan penyedia akses `PBJ_PR`; sampai itu semua jenis dokumen dijawab 404 (*fail-closed*, sesuai catatan kartu) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Common` |
| Registry | `Corporate / Procurement | ProcurementManagement / Common / … | Prc | ACTIVE` (`PBJ-DEC-035`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs` |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Prc`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001` (tarik = `IsCancel`, tidak hapus fisik), `QBE-LOG-001`, `QBE-AUD-001`, `QBE-TXN-001` (berkas fisik dibuang bila simpan DB gagal) |
| Pengecualian pola | `IProcurementDocumentAccessProvider` adalah interface titik perluasan (bukan service biasa), dengan alasan yang sama seperti `IWorkflowReferenceLifecycleHandler` |
| Hasil checker | QBE `PASS` (bersama `BE-PBJ-003`, 41 berkas, 0 temuan) |

## 1. Masalah yang diperbaiki

Dokumen Pengadaan butuh bukti pendukung (foto kondisi, KAK, penawaran, BAST), tetapi belum ada tempat menyimpan
berkas yang aman. Lampiran workflow HR menyimpan berkas di bawah `wwwroot` dan hanya menempel ke instance
approval, sehingga tidak cocok untuk legalitas vendor atau foto NCR (`PBJ-CAP-004`).

## 2. Proses bisnis

1. Pengguna mengunggah berkas ke sebuah dokumen (jenis + ID).
2. Server memeriksa hak atas dokumen induk:
   - tidak ada atau di luar cakupan → 404;
   - hanya boleh baca (mis. dokumen final) → 409 `PBJ-VAL-083`.
3. Server memeriksa berkas:
   - ekstensi harus ada di daftar izin (`PBJ-VAL-080`);
   - isi berkas harus cocok dengan ekstensinya (`PBJ-VAL-081`);
   - ukuran ≤ batas, bawaan 10 MB (`PBJ-VAL-082`).
4. Berkas disimpan dengan nama acak di luar `wwwroot`, beserta SHA-256-nya. Satu baris riwayat `ATTACH`
   ditulis dalam transaksi yang sama.
5. Berkas pengganti membuat versi baru, dan versi lama tetap ada.
6. Unduhan hanya lewat endpoint yang memeriksa hak atas dokumen induk, dengan
   `Content-Disposition: attachment`.

**Contoh penolakan:**
- `penawaran.pdf` yang ternyata berisi HTML → 422 "Isi berkas tidak sesuai dengan jenisnya."
- `virus.exe` yang diganti nama menjadi `foto.jpg` → 422 dengan pesan yang sama, karena berkas tidak diawali
  `FF D8 FF`.
- DOCX palsu (ZIP tanpa `word/document.xml`) → 422.

**Jalur tidak normal:**
- Bila basis data gagal menyimpan, berkas fisik dibuang supaya tidak ada berkas yatim.
- Lampiran ditarik (`deactivate`) lewat penanda `IsCancel`, tidak dihapus fisik.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WorkflowFileStorageService.cs` (pola penyimpanan, daftar ekstensi), `WorkflowAttachmentController.cs`
(`[Consumes]`, `[RequestSizeLimit]`, `PhysicalFile`), DTO lampiran workflow, `.gitignore`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Common/Models/PrcAttachment.cs` + `PrcAttachmentConfiguration.cs` | **Baru.** Index unik `(AttachmentKey, VersionNo)` dan `StoredFileName` |
| `Common/Services/IProcurementDocumentAccessProvider.cs` | **Baru.** Tingkat hak `None`/`Read`/`Edit` per jenis dokumen |
| `Common/Services/ProcurementAttachmentService.cs` | **Baru.** Daftar, unggah (validasi + versi + riwayat), tarik, unduh; konfigurasi `Procurement:Attachment:{AllowedExtensions,MaximumFileSizeBytes,StorageRoot}` dengan nilai bawaan |
| `Common/DTOs/ProcurementAttachmentDtos.cs` | **Baru** |
| `Common/Controllers/ProcurementAttachmentsController.cs` | **Baru.** 4 endpoint |
| `ProcurementManagementServiceCollectionExtensions.cs`, `ApplicationDbContext.cs` | +1 service, +1 `DbSet` |
| `Migrations/20261112150000_AddProcurementAttachment.cs` + `.Designer.cs`; snapshot | **Baru** / diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `PBJ-PP-API` bagian Attachment, tanpa delta. Ditambahkan field opsional `replacesAttachmentId` pada unggah untuk membuat versi baru (FR-PBJ-021) |
| Database | 1 tabel baru, tanpa FK. Migration **dibuat, tidak diterapkan** |
| Keamanan/Auth | Berkas di luar `wwwroot` (`App_Data/procurement-attachments` di bawah content root, dapat diatur). Akses berlapis: izin `ProcurementAttachment : Read/Upload` **dan** hak atas dokumen induk. Daftar izin konfigurasi hanya menerima ekstensi yang punya pemeriksa tanda tangan, sehingga konfigurasi tidak bisa membuka `.html`/`.exe` |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Common / Attachment

Base URL: `api/v1/corporate/procurement-management/attachments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/?documentType=&documentId=&includeHistory=` | Lampiran sebuah dokumen | `ProcurementAttachment : Read` + hak baca dokumen |
| `POST` | `/` (multipart) | Unggah; isi `replacesAttachmentId` untuk versi baru | `ProcurementAttachment : Upload` + hak ubah dokumen |
| `GET` | `/{id}/download` | Unduh berkas | `ProcurementAttachment : Read` + hak baca dokumen |
| `POST` | `/{id}/deactivate` | Tarik lampiran | `ProcurementAttachment : Upload` + hak ubah dokumen |

**Kode status:** 404 dokumen/lampiran tidak ada atau di luar cakupan; 409 dokumen final atau lampiran bukan versi
berlaku; 422 berkas ditolak.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (bersama `BE-PBJ-003`) | 0 error, 608 warning (= baseline), 0 dari Pengadaan | `PASS` | Log build |
| EF `migrations script` | `CREATE TABLE public."PrcAttachment"` + 4 index (2 unik) | `PASS` | Keluaran skrip |
| Konsistensi snapshot | Tidak ada selisih untuk `PrcAttachment` | `PASS` | Log pemeriksaan |
| QBE | 0 temuan | `PASS` | Laporan checker |
| Pemeriksa tanda tangan (PDF `%PDF`, PNG 8 byte, JPEG `FF D8 FF`, DOCX/XLSX ZIP + `[Content_Types].xml` + bagian utama) | Ada di `MatchesSignature` | `PASS` (statis) | Review source |
| `AT-PP-53`..`56` | Tidak dijalankan | `NOT RUN` | Migration belum diterapkan; belum ada penyedia akses dokumen (pertama: `BE-PBJ-006`) |

- **Uji manual:** `NOT FEASIBLE` saat ini.
- **AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Status migration dan basis data

`20261112150000_AddProcurementAttachment` dibuat, **tidak diterapkan**. Urutan: setelah `AddProcurementCatalog`.

## 7. Risiko yang tersisa

| Risiko | Penanganan |
| --- | --- |
| Folder `App_Data/` **tidak** ada di `.gitignore`, sehingga berkas unggahan di mesin developer bisa ikut ter-commit | Perlu satu baris `.gitignore` (`App_Data/`) atau akar penyimpanan di luar repository via `Procurement:Attachment:StorageRoot`. Di luar cakupan task ini; **perlu keputusan pemilik** |
| Antivirus tidak dijalankan pada berkas | BRD B6 menyebut "bila tersedia"; belum ada layanan antivirus di repository |
| Seluruh lampiran tertolak 404 sampai penyedia akses dokumen pertama terdaftar | Disengaja (fail-closed); `BE-PBJ-006` mendaftarkan `PBJ_PR` |

## 8. Task berikutnya

`BE-PBJ-006` mendaftarkan `IProcurementDocumentAccessProvider` untuk `PBJ_PR`, dan saat itu `AT-PP-53`..`56`
dapat diuji.
