# Laporan Perubahan Backend — `BE-HRD-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-023` |
| Judul | Aggregate Entitas & Konfigurasi EF Core untuk SK Kepegawaian, Penempatan Batch, dan Approval Assignment |
| Slice | `S-A1` (Workforce Core & Employee 360) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 10 |
| Trace | `FR-HRD-013`, `FR-HRD-017`, `FR-HRD-018`, `HRD-DEC-046`, `HRD-DEC-048`, `HRD-DEC-050` |
| Dependency | — (Task fondasi Gelombang 10) |
| Klasifikasi | `HEAVY` |
| Task mode | `BACKEND` |
| Target tulis | Models, Configurations, ApplicationDbContext, Migrations, Laporan Task |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang Diselesaikan

Sebelumnya, tidak ada entitas terpadu untuk Surat Keputusan (SK) Kepegawaian di sistem. Mutasi, promosi, dan rotasi dilakukan secara terfragmentasi tanpa nomor SK, tanpa bukti pindaian dokumen fisik yang ditandatangani, dan tanpa jejak audit pengesahan. Selain itu, 55 pegawai baseline rumah sakit memiliki data penempatan organisasi namun tidak memiliki penempatan jabatan, atasan, atau gaji. Entitas penempatan `WfpOrganizationAssignment`, `WfpPositionAssignment`, dan `WfpManagerAssignment` juga belum memiliki foreign key ke SK pengesahan serta jejak persetujuan (`ApprovedByUserId`, `ApprovedAt`).

---

## 2. Proses Bisnis Rumah Sakit

* **Tujuan**: Membangun fondasi persistensi relasional untuk alur SK Kepegawaian (Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara), relasi batch penempatan awal, dan jejak pengesahan mutasi.
* **Pelaku**: Tim Tata Usaha Kepegawaian, Direktur SDM, Administrator Database.
* **Pemicu**: Kebutuhan tata kelola kepegawaian rumah sakit yang patuh regulasi akreditasi dan audit tata kelola SDM rumah sakit.
* **Hasil Akhir**: Skema database siap menampung dokumen SK, berkas fisik pindaian SK bertanda tangan, batch penempatan 55 pegawai, dan relasi riwayat penempatan yang mengacu ke SK pengesahan.

---

## 3. Rincian Perubahan yang Dikerjakan

### 3.1 Berkas Baru
1. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Models/HrdPersonnelActionOrder.cs` — Aggregate root SK Kepegawaian terpadu.
2. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Models/HrdPersonnelActionOrderAttachment.cs` — Entitas lampiran berkas pindaian SK fisik bertanda tangan.
3. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Models/HrdInitialPlacementBatch.cs` — Aggregate batch dan baris penempatan awal untuk 55 pegawai baseline.
4. `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/WorkforceCore/HrdPersonnelActionOrderConfiguration.cs` — Konfigurasi EF Core untuk SK Kepegawaian.
5. `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/WorkforceCore/HrdPersonnelActionOrderAttachmentConfiguration.cs` — Konfigurasi EF Core untuk lampiran SK.
6. `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/WorkforceCore/HrdInitialPlacementBatchConfiguration.cs` — Konfigurasi EF Core untuk batch penempatan awal.
7. `NewQuilvianSystemBackend/Migrations/20261102085741_AddPersonnelActionOrderAndAssignmentApprovalFields.cs` & `Designer.cs` — Berkas migrasi database EF Core.

### 3.2 Berkas yang Diperbarui
1. `WfpOrganizationAssignment.cs` & `Configuration.cs` — Menambahkan kolom `PersonnelActionOrderId`, `PersonnelActionOrder`, `ApprovedByUserId`, `ApprovedAt`.
2. `WfpPositionAssignment.cs` & `Configuration.cs` — Menambahkan kolom `PersonnelActionOrderId`, `PersonnelActionOrder`, `ApprovedByUserId`, `ApprovedAt`.
3. `WfpManagerAssignment.cs` & `Configuration.cs` — Menambahkan kolom `PersonnelActionOrderId`, `PersonnelActionOrder`, `ApprovedByUserId`, `ApprovedAt`.
4. `WfpSalaryAssignment.cs` & `Configuration.cs` — Menambahkan kolom `PersonnelActionOrderId`, `PersonnelActionOrder`.
5. `TrxEmployeeProfileChangeRequest.cs` & `Configuration.cs` — Menambahkan kolom `IsSubmittedOnBehalf`, `SubmittedByWorkforceProfileId`, `OnBehalfReason`, `OnBehalfNotifiedAt`, `EvidenceFilePath`, `EvidenceFileName`.
6. `Repositories/ApplicationDbContext.cs` — Mendaftarkan DbSet `HrdPersonnelActionOrders`, `HrdPersonnelActionOrderAttachments`, `HrdInitialPlacementBatches`, dan `HrdInitialPlacementBatchRows`.

---

## 4. Verifikasi dan Kepatuhan

* Konformasi nama tabel dan schema: Semua entitas baru berada pada schema `public` dengan relasi onDelete `Restrict` untuk keselamatan integritas data referensial rumah sakit.
* Migrasi EF Core telah disiapkan secara lengkap untuk dijalankan oleh pengembang melalui terminal.
