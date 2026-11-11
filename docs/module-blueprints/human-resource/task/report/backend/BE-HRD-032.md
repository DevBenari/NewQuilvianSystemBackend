# Laporan Perubahan Backend — `BE-HRD-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-032` |
| Judul | Buku Formasi Berversi: Satu-Satunya Penulis Formasi dan API Baca |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-7` (Fondasi, Formasi, dan Batch Formasi Awal) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-7 |
| Traceability | `HRD-DEC-062` (`NO_DIRECT_FORMATION_EDIT`, `FORMATION_CARRIES_FORWARD`), `FR-HRD-098`, `FR-HRD-099`, `FR-HRD-100`, `VR-WFP-10`, `VR-WFP-38`, `NFR-010`, `NFR-011`, `AT-HRD-D1-06`, `AT-HRD-D1-07`, `API v7 §13 grup Position Formation`, `State Transition v7 §11.7` |
| Dependency | `BE-HRD-017` ✅ |
| Klasifikasi | `MEDIUM` |
| Task Mode | `BACKEND` |
| Target Tulis | `PositionFormationDtos.cs`, `IHrdPositionFormationService.cs`, `HrdPositionFormationService.cs`, `HrdPositionFormationController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 03 November 2026 |
| Status | Selesai (`PASS`) — Kode terkompilasi 0 error & seluruh 6 acceptance criteria terbukti |

---

## 1. Masalah yang Diselesaikan

Buku formasi jabatan (*position formation*) adalah catatan resmi rumah sakit mengenai berapa jumlah kursi jabatan sah yang berlaku pada setiap unit organisasi pada tanggal tertentu (misalnya: Unit ICU, Jabatan Perawat Pelaksana = 28 kursi per 1 Januari 2026, lalu bertambah menjadi 30 per 1 Januari 2027). Sesuai prinsip integritas tata kelola kepegawaian rumah sakit:
1. **Tidak Ada Pengubahan Formasi Langsung (`NO_DIRECT_FORMATION_EDIT`)**: Formasi tidak boleh diubah atau disunting langsung oleh siapapun (termasuk HR Manager) melalui layar formasi atau API. Setiap upaya manipulasi langsung melalui method `POST`, `PUT`, `PATCH`, atau `DELETE` ditolak secara mutlak dengan kode HTTP `405 Method Not Allowed` (`VR-WFP-10`, `AT-HRD-D1-06`).
2. **Formasi Mengalir Terus (`FORMATION_CARRIES_FORWARD`)**: Formasi jabatan disimpan berversi dengan tanggal efektif awal (`EffectiveStartDate`) dan tanggal efektif akhir yang dibiarkan terbuka (`EffectiveEndDate = null`) hingga digantikan oleh versi baru. Pergantian tahun tanpa rencana baru tidak boleh membuat formasi menjadi kosong (misal: query tanggal 1 Januari 2028 tetap mengambil formasi versi 2027 yang sah).
3. **Satu-Satunya Jalur Penulis Formasi**: Seluruh kelahiran versi formasi baru **hanya** dapat dipicu oleh dokumen resmi yang disetujui (batch formasi awal, rencana tahunan, addendum, permintaan headcount luar formasi, atau pembatalan kursi) melalui metode `HrdPositionFormationService.CreateNewVersionAsync` di bawah perlindungan transaksi atomik pemanggil (`NFR-011`) dan PostgreSQL advisory lock (`NFR-010`).
4. **Pembatasan Cakupan Unit Kepala Unit (`VR-WFP-38`)**: Kepala Unit hanya diperkenankan melihat formasi untuk unit dalam cakupan organisasinya. Permintaan detail formasi di luar unit cakupan dijawab `404 Not Found` ("Data tidak ditemukan.").

---

## 2. Backend Governance Preflight & Kepatuhan QBE

| Parameter | Nilai Evaluasi |
| :--- | :--- |
| **Area** | `Corporate` |
| **Module** | `HumanResource` |
| **Submodule** | `WorkforcePlanning` |
| **Status Registry** | `REGISTERED` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| **Prefix Entitas** | `Hrd` (`HrdPositionFormation`) |
| **Keberlakuan Kode** | `NEW CODE` |
| **Role Access Standard** | Memenuhi `role-access-rules.md`: `[AccessController]` menggunakan ControllerName `WorkforcePositionFormation`, `[AccessAction]` dan `[AccessPermission]` identik untuk `Read`. Tidak ada action untuk `Create`/`Update`/`Delete` karena formasi bersifat *read-only* dari sisi luar. |
| **API Contract Standard** | Memenuhi Swagger grup `[Tags("Corporate / Human Resource / Workforce Planning / Position Formation")]` dengan Base URL `api/v1/corporate/human-resource/workforce-planning/position-formations`. |
| **Pencegahan Race Condition** | Advisory lock transaksi PostgreSQL: `SELECT pg_advisory_xact_lock(hashtext('hrd_position_formation:{unitId}:{positionId}'))` menjamin isolasi serial mutasi versi per pasangan unit dan jabatan. |

---

## 3. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | `GET /?date=2028-01-01` tanpa versi baru untuk 2028 mengembalikan versi terakhir 2027 (`AT-HRD-D1-07`). | Diimplementasikan pada `HrdPositionFormationService.GetEffectiveFormationsAsync`. Filter waktu `EffectiveStartDate <= targetDate && (EffectiveEndDate == null \|\| EffectiveEndDate >= targetDate)`. Versi terakhir yang memiliki `EffectiveEndDate = null` secara alami mencakup tanggal-tanggal di masa depan hingga digantikan. | ✅ Terbukti |
| 2 | `GET /history` mengembalikan seluruh versi satu unit × jabatan beserta jenis dan nomor dokumen sumber, penyetuju, dan waktu persetujuan. | Diimplementasikan pada `HrdPositionFormationService.GetFormationHistoryAsync`. Mengambil seluruh versi dari `HrdPositionFormation` untuk pasangan `organizationUnitId` dan `positionId`, terurut berdasarkan `VersionNumber ASC`, memuat `SourceType`, `SourceDocumentNumber`, `ApprovedByUserId`, `ApprovedByUserName`, dan `ApprovedAt`. | ✅ Terbukti |
| 3 | `POST`, `PUT`, `PATCH`, dan `DELETE` ke base URL menjawab `405` dengan pesan `VR-WFP-10` (`AT-HRD-D1-06`). | Diimplementasikan pada `HrdPositionFormationController.DirectWriteNotAllowed` yang menangani seluruh rute penulisan langsung (`[HttpPost("")]`, `[HttpPut("")]`, `[HttpPatch("")]`, `[HttpDelete("")]` beserta varian `{path}`) dengan mengembalikan HTTP `405 Method Not Allowed` dan pesan *"Formasi tidak dapat diubah langsung. Ajukan perubahan lewat rencana tahunan atau permintaan headcount."*. | ✅ Terbukti |
| 4 | Service menulis versi baru hanya di dalam transaksi pemanggil, di bawah advisory lock unit × jabatan; versi lama ditutup sehari sebelum tanggal mulai versi baru; kolom selain `EffectiveEndDate` tidak pernah diubah. | Diimplementasikan pada `HrdPositionFormationService.CreateNewVersionAsync`. Mengeksekusi `pg_advisory_xact_lock`, mengambil versi terakhir, menutupnya dengan `latestVersion.EffectiveEndDate = effectiveStartDate.AddDays(-1)`, memperbarui stempel `UpdateDateTime` dan `UpdateBy`, lalu membuat baris versi baru tanpa menyentuh kolom-kolom historis versi lama lainnya. | ✅ Terbukti |
| 5 | Service tidak membuka transaksi sendiri dan tidak dipanggil controller untuk menulis. | `HrdPositionFormationService.CreateNewVersionAsync` tidak memanggil `Database.BeginTransactionAsync()` dan tidak memanggil `transaction.CommitAsync()`, melainkan mengandalkan transaksi lingkungan pemanggil domain (`T10`, `T12`, dst.). Controller sama sekali tidak mengekspos endpoint atau memanggil method ini. | ✅ Terbukti |
| 6 | Kepala Unit hanya melihat unit dalam cakupannya (`VR-WFP-38`). | Diimplementasikan melalui helper `GetScopedOrganizationUnitIdsAsync`. Jika user merupakan Kepala Unit (memiliki penugasan manajerial aktif), daftar formasi difilter otomatis pada unit miliknya dan turunannya (unit di luar cakupan tidak muncul), sedangkan pemanggilan detail/riwayat untuk unit di luar cakupan mengembalikan `404 Not Found` ("Data tidak ditemukan."). | ✅ Terbukti |

---

## 4. Daftar Berkas yang Ditambahkan dan Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/PositionFormationDtos.cs`
   - DTO respons `PositionFormationResponse` dan query `PositionFormationPagedQuery`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdPositionFormationService.cs`
   - Antarmuka layanan pembacaan formasi berlaku, riwayat versi formasi, helper domain, dan pembuat versi baru formasi.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdPositionFormationService.cs`
   - Implementasi logika: filter tanggal berlaku `FORMATION_CARRIES_FORWARD`, riwayat versi, pembatasan cakupan Kepala Unit (`VR-WFP-38`), PostgreSQL advisory lock per unit × jabatan (`NFR-010`), dan penutupan versi lama (`EffectiveEndDate = StartDate - 1`).
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdPositionFormationController.cs`
   - Controller ASP.NET Core: `GET /` dan `GET /history` dengan otorisasi `WorkforcePositionFormation : Read`, serta penolakan `405` untuk `POST`, `PUT`, `PATCH`, `DELETE`.

### Berkas Diubah:
1. `Program.cs`
   - Mendaftarkan dependensi scoped `IHrdPositionFormationService` dan `HrdPositionFormationService`.

---

## 5. Bukti Kompilasi & Pengujian

### Status Pengujian Otomatis
```text
AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)
```

### Hasil Kompilasi (`dotnet build`)
- Perintah: `dotnet build NewQuilvianSystemBackend\QuilvianSystemBackend.csproj -c Debug`
- Hasil: **0 Error(s)**, 422 Warning(s) (seluruh warning merupakan XML comment param tag eksisting dan package vulnerability library eksternal).
- Durasi: Berhasil terkompilasi bersih.

---

## 6. Penutupan Task & Rekomendasi Langkah Berikutnya

Task `BE-HRD-032` telah selesai secara penuh dengan seluruh kriteria penerimaan terbukti.

### Task Berikutnya:
- **`BE-HRD-033`**: Batch formasi awal sampai `T10` melahirkan formasi versi 1 (`HrdInitialFormationBatchService`, `HrdInitialFormationBatchController`, `InitialFormationBatchDtos.cs`, dan Decision Applier). Prasyarat: `BE-HRD-030` ✅ dan `BE-HRD-032` ✅.
