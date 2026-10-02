# Laporan Perubahan Backend — `BE-RWI-127`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-127` |
| Judul | Fondasi Outbox & Occupancy Tracking |
| Slice | Gelombang INT-BE-1 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md`](../../roadmap/backend-roadmap.md) — kartu `BE-RWI-127` |
| Trace | `FR-INT-017`; `RWI-DEC-161`, `RWI-AC-241`; Data Dictionary §2 & §3 |
| Contract version | `1.0.0` |
| Dependency | Tidak ada (Fondasi awal) |
| Klasifikasi | `HIGH` — Skema basis data outbox integrasi dan kolom tracking tempat tidur |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/`, `Repositories/`, `Migrations/` |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — Model, konfigurasi EF Core, DbSet ApplicationDbContext, dan file migrasi telah terpasang lengkap. |

---

## 1. Masalah yang Diselesaikan
Sebelumnya sistem rawat inap tidak memiliki mekanisme penyimpanan event integrasi asinkron yang andal (*Transactional Outbox*) untuk berkomunikasi dengan modul Billing/Kasir, sehingga berisiko terjadi inkonsistensi data jika pengiriman event jaringan gagal (*dual-write problem*). Selain itu, entitas penempatan tempat tidur (`InpBedPlacement`) belum memiliki penanda kepergian fisik (`PhysicallyLeftAt`), pelacakan versi mutasi (`Version`), riwayat alasan perubahan (`ChangeReason`), dan status *superseded* (`IsSuperseded`) untuk audit jejak rekam penagihan sewa kamar.

---

## 2. Proses Bisnis & Perubahan yang Dikerjakan

1. **Enum `OutboxStatus`**: Dibuat di `Areas/HealthServices/InPatientManagement/Enums/OutboxStatus.cs` dengan status `Pending (0)`, `Processing (1)`, `Published (2)`, `Failed (3)`, dan `DeadLetter (4)`.
2. **Enum `BillingClearanceStatus`**: Dibuat di `Areas/HealthServices/InPatientManagement/Enums/BillingClearanceStatus.cs` dengan status `None (0)`, `Pending (1)`, `Cleared (2)`, `Revoked (3)`, dan `Overridden (4)`.
3. **Model Entitas `InpIntegrationOutbox`**: Dibuat di `Areas/HealthServices/InPatientManagement/Models/InpIntegrationOutbox.cs` mewarisi `IdentityModel`, menyimpan data transaksi outbox (`IdempotencyKey`, `SourceDomain`, `SourceType`, `SourceDetailId`, `EventType`, `PayloadJson`, `Status`, `RetryCount`, `NextRetryAtUtc`, `PublishedAtUtc`, `LastError`, `CreatedAtUtc`).
4. **Pembaruan Entitas `InpBedPlacement`**: Menambahkan kolom `PhysicallyLeftAt`, `Version` (bawaan 1), `ChangeReason`, `IsSuperseded` (bawaan false), dan `SupersededAtUtc`.
5. **Pembaruan Entitas `InpEpisode`**: Menambahkan kolom tracking kelayakan kasir `ClearanceStatus`, `ClearanceRevokedReason`, `IsSupervisorOverridden`, `SupervisorOverrideReason`, `SupervisorOverriddenByUserId`, dan `SupervisorOverriddenAtUtc`.
6. **Konfigurasi EF Core**:
   - Dibuat `Repositories/Configurations/HealthServices/InPatientManagement/InpIntegrationOutboxConfiguration.cs` dengan unique index pada `IdempotencyKey` dan filtered index pada `Status` (`IX_InpIntegrationOutbox_Status_Pending`).
   - Diperbarui `InpBedPlacementConfiguration.cs` dengan pemetaan kolom baru dan index pada `IsSuperseded`.
7. **Pendaftaran `DbSet`**: Ditambahkan `public DbSet<InpIntegrationOutbox> InpIntegrationOutboxes { get; set; }` pada `Repositories/ApplicationDbContext.cs`.
8. **File Migrasi EF Core**: Dibuat migrasi `Migrations/20260917000000_AddInpatientBillingIntegrationOutbox.cs` dengan operasi `Up()` dan `Down()` zero-downtime yang aman.

---

## 3. Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Enums/OutboxStatus.cs` | Baru — Enum status antrean outbox |
| `Areas/HealthServices/InPatientManagement/Enums/BillingClearanceStatus.cs` | Baru — Enum status kelayakan kasir |
| `Areas/HealthServices/InPatientManagement/Models/InpIntegrationOutbox.cs` | Baru — Entitas tabel antrean outbox integrasi |
| `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs` | Diperbarui — Kolom tracking hunian & versi |
| `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` | Diperbarui — Kolom clearance & supervisor override |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpIntegrationOutboxConfiguration.cs` | Baru — Konfigurasi tabel & indeks outbox |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpBedPlacementConfiguration.cs` | Diperbarui — Konfigurasi kolom baru penempatan |
| `Repositories/ApplicationDbContext.cs` | Diperbarui — Penambahan DbSet InpIntegrationOutboxes |
| `Migrations/20260917000000_AddInpatientBillingIntegrationOutbox.cs` | Baru — Script migrasi database Up & Down |

---

## 4. Verifikasi dan Kepatuhan Kriteria Penerimaan

- **AC-1 (Migrasi Skema):** Tabel `InpIntegrationOutboxes` dan penambahan kolom pada `InpBedPlacement` dan `InpEpisode` terdefinisi dengan tipe data presisi dan default aman (nol penguncian tabel).
- **AC-2 (Index Unik Idempotensi):** Index unik `UQ_InpIntegrationOutbox_IdempotencyKey` dan filtered index status pending terdaftar pada konfigurasi EF Core.
- **AC-3 (Rollback Bersih):** Method `Down()` pada file migrasi mengembalikan skema awal secara bersih.
- **Catatan Eksekusi:** Sesuai instruksi pengguna, `dotnet build` dan penerapan migrasi database dilakukan mandiri oleh pengguna setelah seluruh kode terpasang.
