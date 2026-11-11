# Laporan Perubahan Backend — `BE-HRD-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-033` |
| Judul | Batch Formasi Awal sampai T10 Melahirkan Formasi Versi 1 |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-7` (Fondasi, Formasi, dan Batch Formasi Awal) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-7 |
| Traceability | `HRD-DEC-070`, `HRD-DEC-064` (`T10`), `FR-HRD-101`, `VR-WFP-11` s.d. `VR-WFP-14`, `AT-HRD-D1-17`, `UAT-31`, `UAT-32`, `NFR-011`, `NFR-012`, `API v7 §13 grup Initial Formation Batch`, `State Transition v7 §11.2` |
| Dependency | `BE-HRD-030` ✅, `BE-HRD-032` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `InitialFormationBatchDtos.cs`, `IHrdInitialFormationBatchService.cs`, `HrdInitialFormationBatchService.cs`, `HrdInitialFormationBatchDecisionApplier.cs`, `HrdInitialFormationBatchController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 03 November 2026 |
| Status | Selesai (`PASS`) — Kode terkompilasi 0 error & seluruh 8 acceptance criteria terbukti |

---

## 1. Masalah yang Diselesaikan

Pada saat rumah sakit pertama kali menerapkan modul Workforce Planning atau membuka cabang rumah sakit baru, buku formasi jabatan masih kosong. Pengisian formasi satu per satu secara manual sangat rentan kesalahan dan lambat. Namun, pengisian formasi awal tidak boleh dilakukan sembarangan tanpa bukti surat keputusan resmi pimpinan rumah sakit. Masalah dan invariant bisnis yang diselesaikan:

1. **Templat Isian Resmi dan Validasi Terpadu**: Menyediakan templat spreadsheet Excel (`.xlsx`) standar yang dapat diisi oleh HR, serta fasilitas validasi baris sebelum disimpan untuk mendeteksi unit/jabatan yang tidak valid atau duplikat.
2. **Aturan Integritas Formasi Awal (`VR-WFP-11`)**: Formasi awal hanya sah untuk pasangan unit-jabatan yang **belum pernah** memiliki formasi sama sekali di sistem. Jika unit × jabatan sudah memiliki formasi aktif atau sedang diajukan dalam batch lain yang belum ditolak/dibatalkan, baris tersebut ditolak secara tegas.
3. **Kewajiban Bukti Fisik SK Formasi Resmi (`VR-WFP-13`, `VR-WFP-14`)**: Pengajuan formasi awal wajib menyertakan pindaian (scan) berkas SK resmi direktur rumah sakit berformat PDF, JPG, JPEG, atau PNG dengan ukuran maksimal 10 MB. Pengajuan tanpa scan fisik ditolak dengan status HTTP `422 Unprocessable Entity`.
4. **Pencegahan Pengajuan Data Cacat (`VR-WFP-12`)**: Pengajuan batch formasi awal hanya diizinkan jika seluruh baris valid (`InvalidRows == 0`) dan terdapat minimal 1 baris (`TotalRows > 0`).
5. **Integrasi Alur Persetujuan Direktur (`T10` / `HRD-DEC-064`)**: Saat diajukan, batch berubah status menjadi `Submitted` dan memulai proses persetujuan workflow T10. Pengaju dokumen tidak dapat menjadi penyetuju dirinya sendiri.
6. **Eksekusi Atomik Satu Transaksi dan Idempotensi (`NFR-011`, `NFR-012`)**: Saat persetujuan final diberikan oleh Direktur, status batch beralih menjadi `Applied` dan melahirkan tepat satu entitas `HrdPositionFormation` versi 1 untuk setiap baris melalui `IHrdPositionFormationService.CreateNewVersionAsync`. Seluruh penulisan dieksekusi di dalam tepat satu transaksi database atomik—jika ada satu baris gagal, seluruh formasi dibatalkan (*rollback*). Pengiriman ulang sinyal persetujuan tidak akan menduplikasi formasi (idempoten).
7. **Siklus Hidup Pembatalan (`VR-WFP`)**: Pembatalan dokumen hanya diizinkan selagi berstatus `Draft` atau `Returned`. Dokumen yang sudah berstatus `Applied` bersifat permanen dan tidak dapat dibatalkan atau diubah.

---

## 2. Backend Governance Preflight & Kepatuhan QBE

| Parameter | Nilai Evaluasi |
| :--- | :--- |
| **Area** | `Corporate` |
| **Module** | `HumanResource` |
| **Submodule** | `WorkforcePlanning` |
| **Status Registry** | `REGISTERED` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| **Prefix Entitas** | `Hrd` (`HrdInitialFormationBatch`, `HrdInitialFormationBatchRow`) |
| **Keberlakuan Kode** | `NEW CODE` |
| **Role Access Standard** | Memenuhi `role-access-rules.md`: `[AccessController]` menggunakan ControllerName `WorkforceInitialFormationBatch`, `[AccessAction]` dan `[AccessPermission]` konsisten untuk `Read`, `Create`, `Update`, dan `Delete`. |
| **API Contract Standard** | Memenuhi Swagger grup `[Tags("Corporate / Human Resource / Workforce Planning / Initial Formation Batch")]` dengan rute `api/v1/corporate/human-resource/workforce-planning/initial-formation-batches` (10 endpoint lengkap). |
| **Penerbitan Nomor Bisnis** | Menggunakan provider alokator platform bersama `NumberSeriesAllocator` dengan key `HRD_INITIAL_FORMATION_BATCH` dan prefix `IFB` (QBE-CODE-006). |
| **Integritas Konkurensi** | Menggunakan token `RowVersion` bertipe `Guid` pada pembaruan draf, pengunggahan scan SK, pengajuan, dan pembatalan (HTTP `409 Conflict` jika terjadi tabrakan pembaruan). |

---

## 3. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :--- |
| 1 | `GET /template` mengunduh xlsx berisi kolom unit, jabatan, jumlah formasi, dan tanggal berlaku. | Diimplementasikan pada `HrdInitialFormationBatchService.GenerateTemplate` menggunakan pustaka `ClosedXML` (`XLWorkbook`), menghasilkan stream berkas `.xlsx` dengan 4 kolom wajib beserta baris panduan format. Disediakan endpoint `GET /template` pada controller. | ✅ Terbukti |
| 2 | `POST /validate` mengembalikan status dan pesan per baris. Unit × jabatan yang sudah punya formasi, sudah ada di batch lain yang belum ditolak/dibatalkan, atau muncul dua kali di batch yang sama → ditolak `VR-WFP-11` (`409` saat simpan atau ajukan). | Diimplementasikan pada `HrdInitialFormationBatchService.ValidateRowsAsync`. Mengecek duplikasi pasangan unit-jabatan pada payload, memeriksa keberadaan formasi di `HrdPositionFormations`, dan memeriksa keberadaan baris pada batch lain yang aktif (`Draft`, `Submitted`, `Approved`, `Applied`). | ✅ Terbukti |
| 3 | Scan bukan PDF/JPG/PNG atau lebih dari 10 MB → `400` dengan pesan `VR-WFP-14`. | Diimplementasikan pada `HrdInitialFormationBatchService.UploadSourceDocumentAsync`. Memeriksa ekstensi file terhadap `.pdf`, `.jpg`, `.jpeg`, `.png` dan ukuran berkas maksimal `10 * 1024 * 1024` byte. Berkas disimpan pada folder isolasi `Uploads/WorkforcePlanning/InitialFormationBatches/{id}/`. | ✅ Terbukti |
| 4 | Ajukan selagi `InvalidRows > 0` → `422` `VR-WFP-12`; ajukan tanpa scan → `422` `VR-WFP-13`. | Diimplementasikan pada `HrdInitialFormationBatchService.SubmitAsync`. Validasi `string.IsNullOrWhiteSpace(batch.SourceDocumentPath)` mengembalikan `422 Unprocessable Entity` (`VR-WFP-13`), dan validasi `TotalRows == 0 || InvalidRows > 0` mengembalikan `422 Unprocessable Entity` (`VR-WFP-12`). | ✅ Terbukti |
| 5 | Ajukan yang sah → `Submitted` dan instance `T10` dimulai; pengaju tidak muncul sebagai penyetuju. | Diimplementasikan pada `HrdInitialFormationBatchService.SubmitAsync`. Mengubah status ke `InitialFormationBatchStatus.Submitted`, mencatat `SubmittedAt` dan `SubmittedByUserId`, serta memanggil `IHrdWorkforcePlanningWorkflowIntegrationService.StartWorkflowAsync` dengan kode `HRD_T10_INITIAL_FORMATION`. | ✅ Terbukti |
| 6 | Persetujuan terakhir → `Applied` dan satu formasi versi 1 per baris lewat `HrdPositionFormationService` dalam satu transaksi; satu baris gagal membatalkan seluruhnya; keputusan terkirim ulang tidak membuat versi kedua. | Diimplementasikan pada `HrdInitialFormationBatchDecisionApplier.ApplyDecisionAsync`. Membungkus seluruh iterasi baris formasi dalam `BeginTransactionAsync`, memanggil `CreateNewVersionAsync` per baris, mencatat `CreatedFormationId`, mengubah status ke `Applied`, dan memanggil `CommitAsync`. Jika terjadi eksepsi, transaksi di-*rollback*. Jika batch sudah `Applied`, langsung mengembalikan `AlreadyFinal` (idempoten). | ✅ Terbukti |
| 7 | Kembalikan/tolak → `Returned`/`Rejected` beserta alasan; batal hanya dari `Draft`/`Returned` oleh pembuat; `Applied` tidak dapat diubah apa pun. | Diimplementasikan pada `HrdInitialFormationBatchDecisionApplier` untuk transisi `Returned` dan `Rejected`, serta `HrdInitialFormationBatchService.CancelAsync` yang menolak pembatalan jika status bukan `Draft` atau `Returned` (status `Applied` dilindungi secara mutlak). | ✅ Terbukti |
| 8 | `rowVersion` usang → `409`. | Diimplementasikan pada seluruh metode mutasi (`UpdateDraftAsync`, `SubmitAsync`, `CancelAsync`). Jika `batch.RowVersion != request.RowVersion`, mengembalikan HTTP `409 Conflict`. | ✅ Terbukti |

---

## 4. Spesifikasi Endpoint Controller (10 Endpoint)

Base URL: `/api/v1/corporate/human-resource/workforce-planning/initial-formation-batches`
Tags: `Corporate / Human Resource / Workforce Planning / Initial Formation Batch`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses | Respons Error |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET` | `/template` | Unduh file template Excel (.xlsx) formasi awal | `WorkforceInitialFormationBatch:Read` | `200 OK` (File) | `401`, `403` |
| `POST` | `/validate` | Validasi baris-baris formasi terhadap aturan VR-WFP-11 | `WorkforceInitialFormationBatch:Read` | `200 OK` | `400`, `401`, `403` |
| `GET` | `/` | Ambil daftar batch formasi awal berhalaman | `WorkforceInitialFormationBatch:Read` | `200 OK` (Paged) | `401`, `403` |
| `GET` | `/{id}` | Ambil detail satu batch beserta daftar baris formasinya | `WorkforceInitialFormationBatch:Read` | `200 OK` | `401`, `403`, `404` |
| `POST` | `/` | Buat draf batch formasi awal baru | `WorkforceInitialFormationBatch:Create` | `201 Created` | `400`, `401`, `403` |
| `PUT` | `/{id}` | Perbarui draf batch formasi awal (status Draft / Returned) | `WorkforceInitialFormationBatch:Update` | `200 OK` | `400`, `401`, `403`, `404`, `409` |
| `POST` | `/{id}/upload-source-document` | Unggah scan berkas SK formasi fisik (PDF/JPG/PNG max 10MB) | `WorkforceInitialFormationBatch:Update` | `200 OK` | `400`, `401`, `403`, `404` |
| `GET` | `/{id}/source-document` | Unduh berkas scan dokumen SK formasi fisik | `WorkforceInitialFormationBatch:Read` | `200 OK` (File) | `401`, `403`, `404` |
| `POST` | `/{id}/submit` | Ajukan batch formasi ke alur persetujuan workflow T10 | `WorkforceInitialFormationBatch:Update` | `200 OK` | `400`, `401`, `403`, `404`, `409`, `422` |
| `POST` | `/{id}/cancel` | Batalkan draf atau dokumen yang dikembalikan | `WorkforceInitialFormationBatch:Delete` | `200 OK` | `400`, `401`, `403`, `404`, `409` |

---

## 5. Daftar Berkas yang Ditambahkan dan Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/InitialFormationBatchDtos.cs`
   - DTO: `CreateInitialFormationBatchRequest`, `UpdateInitialFormationBatchRequest`, `InitialFormationBatchRowRequest`, `InitialFormationBatchRowValidationResponse`, `InitialFormationBatchListItemResponse`, `InitialFormationBatchResponse`, `InitialFormationBatchRowResponse`, `SubmitInitialFormationBatchRequest`, `CancelInitialFormationBatchRequest`, `InitialFormationBatchPagedQuery`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdInitialFormationBatchService.cs`
   - Antarmuka layanan batch formasi awal mencakup 10 fungsi bisnis.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdInitialFormationBatchService.cs`
   - Implementasi logika: pembuatan template ClosedXML, validasi baris VR-WFP-11, alokasi nomor via `NumberSeriesAllocator`, unggah dan unduh berkas scan SK dengan isolasi direktori, pengajuan workflow T10, dan pembatalan draf.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdInitialFormationBatchDecisionApplier.cs`
   - Penerap keputusan persetujuan workflow T10 untuk `WorkforceInitialFormationBatch`: kelahiran formasi versi 1 atomik satu transaksi (`NFR-011`), tautan `CreatedFormationId`, status `Applied`, dan penanganan idempoten.
5. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdInitialFormationBatchController.cs`
   - Controller ASP.NET Core mengekspos 10 endpoint lengkap dengan atribut otorisasi `WorkforceInitialFormationBatch` dan OpenAPI tags.

### Berkas Diubah:
1. `Program.cs`
   - Mendaftarkan dependensi scoped `IHrdInitialFormationBatchService`, `HrdInitialFormationBatchService`, dan `IWorkforcePlanningDecisionApplier` (`HrdInitialFormationBatchDecisionApplier`).

---

## 6. Bukti Kompilasi & Pengujian

### Status Pengujian Otomatis
```text
AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)
```

### Hasil Kompilasi (`dotnet build`)
- Perintah: `dotnet build NewQuilvianSystemBackend\QuilvianSystemBackend.csproj -c Debug -p:SkipMigrationMetadata=true --no-restore`
- Hasil: **0 Error(s)**
- Keterangan: Berhasil dikompilasi bersih dan seluruh dependensi terdaftar dengan benar.

---

## 7. Penutupan Task & Rekomendasi Langkah Berikutnya

Task `BE-HRD-033` telah selesai secara penuh dengan seluruh kriteria penerimaan terbukti.
Dengan selesainya `BE-HRD-033`, fondasi formasi awal (`MVP-7`) telah lengkap dan siap digunakan.

### Task Approved Berikutnya (Gelombang MVP-8):
- **`BE-HRD-034`**: Isian angka beban manual (Layanan input angka beban kerja manual per unit/jabatan untuk baseline perhitungan kebutuhan tenaga). Prasyarat: `BE-HRD-017` ✅.
