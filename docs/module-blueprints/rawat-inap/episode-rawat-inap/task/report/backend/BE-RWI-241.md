# Laporan Perubahan Backend — `BE-RWI-241`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-241` |
| Judul | Otomasi Pemicu Pembuatan Serah Terima Klinis saat Admisi Transfer |
| Slice | Pendaftaran admisi transfer IGD/Rajal otomatis menerbitkan dokumen serah terima klinis berstatus `PendingSend` lengkap dengan snapshot TTV dan obat dari unit asal |
| Roadmap | [backend-roadmap-transfer-pasien-masuk.md](../../roadmap/backend-roadmap-transfer-pasien-masuk.md) — kartu `BE-RWI-241` |
| Trace | FR-TRF-001; TRF-AC-001, TRF-AC-002; RWI-DEC-051; Kontrak 0.14.0 Transfer Handover 2.1; Flowchart 19 |
| Contract version | `0.14.0` |
| Dependency | Tidak ada (Gelombang 1) |
| Klasifikasi | `MEDIUM` — Modifikasi entitas `CliTransferHandover` (nullable placements), konfigurasi EF Core, pembuatan migrasi C# & SQL script, injeksi service antar modul (`InpAdmissionTransferService` memanggil `CliTransferHandoverService`) |
| Task mode | `BACKEND` — Permintaan pengerjaan tuntas seluruh task oleh pengguna |
| Target tulis | `CliTransferHandover.cs`, `CliTransferHandoverConfiguration.cs`, `InpAdmissionTransferService.cs`, `InpAdmissionTransferService.BedManagement.cs`, `CliTransferHandoverService.cs`, file migrasi `20261022130000_MakeTransferHandoverPlacementsNullable.cs` dan SQL `20261022130000_MakeTransferHandoverPlacementsNullable.sql` |
| Tanggal | 22 Oktober 2026 |
| Status | ✅ Selesai — Implementasi kode sumber, penanganan idempotensi, toleransi error (non-blocking), dan berkas migrasi database telah disiapkan |

---

## 1. Masalah yang Diselesaikan

Sebelumnya, serah terima klinis (`CliTransferHandover`) hanya dirancang untuk perpindahan internal antar-bed dalam rawat inap dengan kolom `FromPlacementId` dan `ToPlacementId` yang bersifat wajib (`Guid` non-nullable dengan foreign key kaku ke `InpBedPlacement`). Pada saat pasien baru ditransfer masuk dari Instalasi Gawat Darurat (IGD) atau Poliklinik Rawat Jalan (Rajal), pasien belum memiliki rekam penempatan fisik (`InpBedPlacement`), melainkan baru sebatas alokasi reservasi bed di rawat inap. Akibatnya:
1. Tidak ada otomasi pembuatan dokumen serah terima klinis saat petugas admisi memproses transfer masuk pasien (`POST /admission-transfers/admit`).
2. Perawat IGD/Rajal harus membuat serah terima secara manual secara terpisah, atau data TTV dan pengobatan asal tidak tersambung ke episode rawat inap.
3. Struktur database menolak penyimpanan dokumen serah terima tanpa placement ID.

> **Skenario Sebelum:** Pasien dari IGD selesai diproses admisi transfer masuk rawat inap, tetapi bangsal rawat inap tidak menerima draf serah terima transfer. Data tanda-tanda vital IGD dan terapi obat harus disalin ulang secara manual oleh perawat.

> **Skenario Sesudah:** Begitu admisi transfer disetujui (`ExecuteTransferAdmissionAsync`), sistem secara otomatis menerbitkan satu rekam `CliTransferHandover` baru dengan status `PendingSend` (`NotSent`), mengaitkannya ke episode rawat inap (`InpEpisodeId`), serta menyalin snapshot tanda-tanda vital terakhir dan riwayat obat dari encounter IGD/Rajal asal.

---

## 2. Alur Proses Bisnis & Aturan

| Unsur | Penjelasan |
| --- | --- |
| **Tujuan** | Menjamin kesinambungan informasi asuhan klinis (clinical handover) dari IGD/Rajal ke rawat inap secara otomatis tanpa entri ulang data dasar |
| **Pelaku** | Petugas Admisi / Kasir PPRI (pemicu admisi transfer), Perawat Unit Asal (IGD/Rajal), Perawat Bangsal Penerima |
| **Pemicu** | Eksekusi `POST /api/v1/health-services/inpatient-management/admission-transfers/admit` |

### Langkah dan Aturan Bisnis:
1. **Relaksasi Foreign Key Penempatan (`FromPlacementId` & `ToPlacementId` Nullable):**
   - Kolom `FromPlacementId` dan `ToPlacementId` diubah menjadi nullable (`Guid?`).
   - Indeks unik penempatan (`UX_CliTransferHandover_ToPlacement`) diberi klausa parsial `WHERE "ToPlacementId" IS NOT NULL` sehingga mendukung transfer masuk dari luar rawat inap.
2. **Inisiasi Idempoten Dokumen Serah Terima:**
   - Method `CliTransferHandoverService.CreateForAdmissionTransferAsync` memeriksa apakah sudah ada `CliTransferHandover` aktif untuk episode rawat inap terkait. Jika sudah ada, sistem mengembalikan record yang ada tanpa membuat duplikasi.
   - Status awal dokumen diset ke `PendingSend` (kode internal `NotSent`).
3. **Penyalinan Snapshot Klinis Kunjungan Asal:**
   - Sistem menyalin TTV terbaru (tekanan darah, nadi, laju napas, suhu tubuh, SpO2, skor nyeri), alat medis terpasang, dan daftar terapi obat dari `sourceEncounterId` (rekam medis IGD/Rajal).
4. **Isolasi Kegagalan (Non-Blocking / Fail-Safe):**
   - Proses pembuatan handover dilingkupi blok `try-catch` terisolasi di `InpAdmissionTransferService.BedManagement.cs`. Jika data klinis asal tidak lengkap atau terjadi kendala pada modul klinis, log peringatan (`ILogger.LogWarning`) dicatat dan proses admisi rawat inap **tetap berhasil** (tidak membatalkan transaksi episode).

---

## 3. Rincian Perubahan Kode

### 3.1 Berkas yang Diubah / Dibuat

| Berkas | Jenis Perubahan | Deskripsi |
| --- | --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/CliTransferHandover.cs` | Modifikasi | Mengubah `FromPlacementId` dan `ToPlacementId` menjadi `Guid?` (nullable) |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliTransferHandoverConfiguration.cs` | Modifikasi | Mengonfigurasi `IsRequired(false)` dan menambahkan filter index unik `WHERE "ToPlacementId" IS NOT NULL` |
| `Areas/HealthServices/ClinicalManagement/Services/CliTransferHandoverService.cs` | Modifikasi | Menambahkan method `CreateForAdmissionTransferAsync` dan memperluas `BuildSnapshotAsync` agar mendukung pencatatan TTV dari kunjungan luar |
| `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs` | Modifikasi | Injeksi dependensi `CliTransferHandoverService` pada constructor |
| `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.BedManagement.cs` | Modifikasi | Memanggil `_transferHandoverService.CreateForAdmissionTransferAsync` sesudah reservasi bed dan pembentukan episode berhasil dibuat |
| `Migrations/20261022130000_MakeTransferHandoverPlacementsNullable.cs` | Baru | Berkas migrasi EF Core untuk mengubah kolom skema dan indeks |
| `Migrations/sql/20261022130000_MakeTransferHandoverPlacementsNullable.sql` | Baru | Berkas SQL manual DDL PostgreSQL yang siap dijalankan mandiri |

---

## 4. Dokumentasi Endpoint Terkait

#### Health Services / Inpatient Management / Admission Transfer
Base URL: `api/v1/health-services/inpatient-management/admission-transfers`

| Method | Path | Deskripsi | Otorisasi | Request Body | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/admit` | Memproses admisi transfer IGD/Rajal ke rawat inap sekaligus menginisiasi dokumen serah terima `PendingSend` | `InpatientAdmissionTransfer : Create` | `ExecuteTransferAdmissionRequest` | `TransferAdmissionResponse` |

---

## 5. Verifikasi Kriteria Penerimaan (Acceptance Criteria)

| Kode AC | Kriteria Penerimaan | Bukti Implementasi pada Kode | Status |
| :--- | :--- | :--- | :---: |
| `TRF-AC-001` | `POST /admission-transfers/admit` otomatis menghasilkan `CliTransferHandover` dengan status `PendingSend` | `InpAdmissionTransferService.BedManagement.cs`: pemanggilan `_transferHandoverService.CreateForAdmissionTransferAsync(episode.Id, request.SourceEncounterId, ...)` | ✅ Terbukti |
| `TRF-AC-002` | Snapshot klinis handover memuat TTV, alat, dan riwayat obat terakhir dari kunjungan asal | `CliTransferHandoverService.cs`: method `BuildSnapshotAsync` mengambil TTV terakhir dari rekam medis encounter asal dan mengisi `TransferHandoverSnapshot` | ✅ Terbukti |
| Fail-Safe | Kegagalan pembuatan handover tidak membatalkan admisi rawat inap | `InpAdmissionTransferService.BedManagement.cs`: blok penanganan exception mandiri dengan pencatatan log peringatan | ✅ Terbukti |
| Idempotensi | Pemanggilan berulang tidak menduplikasi handover | `CliTransferHandoverService.cs`: pengecekan record existing berdasarkan `InpEpisodeId` | ✅ Terbukti |
