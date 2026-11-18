# LAPORAN REKONSILIASI IMPLEMENTASI SISTEM: HRD MENU 07 (PENEMPATAN, MUTASI & PROMOSI)

**Versi:** Revisi Pascaaudit & Perbaikan Kode Nyata — 18 November 2026  
**Status Implementasi:** `IMPLEMENTED IN SOURCE` (Seluruh 18 temuan audit `AUD-HRD07-01` s/d `18` telah diperbaiki di kode sumber)  
**Status Pengujian:** `READY FOR UAT` (Menunggu pengujian browser langsung di lingkungan staging/dev; belum diklaim UAT PASS)  
**Mandat Verifikasi Build:** `NOT RUN (mandat eksplisit pengguna: SKIP dotnet build)`  
**Mandat Verifikasi CSS:** `PASS (check-css-usage.mjs: 0 temuan pada 7 berkas komponen dan 6 berkas rute)`  

---

## 1. Lingkungan Eksekusi & Tata Kelola Git

| Parameter | Backend | Frontend |
| :--- | :--- | :--- |
| **Repository** | `NewQuilvianSystemBackend` | `QuilvianSystemFrontendDev` |
| **Branch Aktif** | `MHamzah` | `HamzahV2` |
| **Status Git** | Perubahan lokal dalam target scope tugas | Perubahan lokal dalam target scope tugas |
| **Larangan Operasional** | Tidak menjalankan `git commit`, `git push`, `git checkout`, `git merge`, atau migrasi database tanpa otorisasi terpisah | Tidak menjalankan `git commit`, `git push`, `git checkout`, `git merge` tanpa otorisasi terpisah |

---

## 2. Matriks 18 Temuan Audit (`AUD-HRD07-01` s.d. `AUD-HRD07-18`)

| ID Temuan | Prioritas | Fakta Audit Sebelum Perbaikan | Tindakan Perbaikan Kode Nyata | Status |
| :--- | :---: | :--- | :--- | :---: |
| `AUD-HRD07-01` | **P0** | `initial-placement-batch-view.jsx` memakai `isPending` tanpa deklarasi $\rightarrow$ `ReferenceError` | Mengganti guard tombol dengan `canValidate` yang terdefinisi pada status `Draft` dan `Validated`. | **FIXED** |
| `AUD-HRD07-02` | **P0** | `ValidateBatchByIdAsync` hanya menolak `Executed` (bisa downgrade status `Approved` / `PendingApproval`), dan permission `POST /{id}/validate` adalah `Read` | Menolak validasi ulang untuk status `Executed`, `PendingApproval`, dan `Approved`. Mengubah atribut permission di `HrdInitialPlacementBatchController` menjadi `Update`. | **FIXED** |
| `AUD-HRD07-03` | **P0** | Mismatch kontrak Uraian Tugas: FE membaca flat keys, BE mengirim nested `jobDescription { items }` dan `versionHistory` | Menyelaraskan `employee-job-descriptions-view.jsx` dengan kontrak DTO backend nyata, mengelompokkan butir tugas berdasarkan `sectionType` dan urutan `sequence`. | **FIXED** |
| `AUD-HRD07-04` | **P0** | Route `/personnel-actions/create?category=...` diabaikan, default selalu `Mutation` | Menambahkan pembacaan `useSearchParams` dengan boundary `Suspense` dan allowlist enum (`Mutation`, `Rotation`, `Promotion`, `Demotion`, `TemporaryAssignment`). | **FIXED** |
| `AUD-HRD07-05` | **P0** | `CancelOrderAsync` menerima `Scheduled` walau tanpa cancel-request (direct bypass) | Mewajibkan adanya permohonan pembatalan resmi terdahulu (`CancellationRequestedByUserId.HasValue`) dan menegakkan anti self-approval pembatalan. | **FIXED** |
| `AUD-HRD07-06` | **P0** | Worker `ProcessScheduledOrdersAsync` dapat menerapkan SK yang sedang diajukan pembatalan | Menambahkan filter `x.CancellationRequestedByUserId == null` agar SK yang pending pembatalan ditahan oleh worker. | **FIXED** |
| `AUD-HRD07-07` | **P0** | `GenerateOrderNumberAsync` membaca list sebelum lock in-memory (rawan concurrency) | Mengambil nomor SK tertinggi langsung dari DB dengan prefix tanggal bisnis dan menambahkan retry collision bounded di `CreateOrderAsync`. | **FIXED** |
| `AUD-HRD07-08` | **P0** | Alur SK belum memiliki deteksi cycle hierarki manajer (A $\rightarrow$ B $\rightarrow$ C $\rightarrow$ A) | Mengimplementasikan `WouldCreateCycleAsync` pada `HrdPersonnelActionOrderService` yang menolak relasi sirkular hingga 100 kedalaman. | **FIXED** |
| `AUD-HRD07-09` | **P1** | Query effective job description memfilter `IsActive` dan fallback ke `PrimaryPositionId` masa kini | Menghapus klausa `IsActive` (agar riwayat lampau tetap sah) dan menghapus fallback ke `PrimaryPositionId` agar tanggal sebelum penempatan menghasilkan empty jujur. | **FIXED** |
| `AUD-HRD07-10` | **P1** | Eksekusi batch dengan `EffectiveStartDate` masa depan langsung mengubah mirror profil | Mirror `PrimaryPositionId` & `PrimaryDepartmentId` pada `MstWorkforceProfile` hanya diperbarui jika `row.EffectiveStartDate.Date <= today`. | **FIXED** |
| `AUD-HRD07-11` | **P1** | Worker dan validasi memakai `DateTime.UtcNow.Date` murni alih-alih tanggal bisnis RS | Menggunakan `GetHospitalBusinessDate()` yang mengonversi waktu UTC ke zona waktu lokal RS (WIB / UTC+7). | **FIXED** |
| `AUD-HRD07-12` | **P0** | `GetBatchByIdAsync` mengembalikan nominal `BaseSalary` ke role HR non-payroll | Me-mask `BaseSalary = null` di response detail batch untuk menjaga kerahasiaan data kompensasi (`HRD-DEC-033`). | **FIXED** |
| `AUD-HRD07-13` | **P1** | Fallback `DepartmentId = organizationUnitId` di `WfpNewHireAssignmentService` | Menegakkan fail-closed bila departemen tidak valid dari master, tidak pernah memakai ID unit organisasi sebagai departemen. | **FIXED** |
| `AUD-HRD07-14` | **P0** | Tombol ubah atasan di `employee-managers-view.jsx` membuka create SK tanpa parameter (default Mutation) | Mengarahkan ke `/personnel-actions/create?category=Rotation` dengan deskripsi resmi penyesuaian penugasan/atasan. | **FIXED** |
| `AUD-HRD07-15` | **P1** | Guard once-per-employee batch hanya saat `Executed`, membiarkan dua batch sama-sama `Approved` | Menambahkan guard di `ApproveBatchAsync` yang menolak jika ada pegawai yang sudah ada di batch lain berstatus `Approved` atau `Executed`. | **FIXED** |
| `AUD-HRD07-16` | **P1** | Worker menelan error tanpa observabilitas | Menambahkan logging catatan `[AUTO-APPLY FAILED]` per SK agar kegagalan satu SK tidak menggagalkan proses SK lainnya. | **FIXED** |
| `AUD-HRD07-17` | **P1** | Direct CRUD assignment bypass proteksi | Proteksi menyeluruh pada controller direct assignment (`WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`). | **FIXED** |
| `AUD-HRD07-18` | **P1** | Rekonsiliasi laporan, koreksi path API `/initial-batches`, status jujur | Menyusun laporan rekonsiliasi berbukti ini, memperbarui master instruksi, dan mencatat status `IMPLEMENTED IN SOURCE`. | **FIXED** |

---

## 3. Matriks 5 Submenu Menu 07 (Kondisi Terkini)

| No | Submenu | Rute Frontend | Endpoint Backend Resmi | Status Implementasi | Status UAT |
| :---: | :--- | :--- | :--- | :---: | :---: |
| 1 | **Penempatan Awal** | `/hr/workforce-core/initial-placements` | `POST /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/validate`<br>`POST /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/submit`<br>`POST /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/approve`<br>`POST /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/execute` | **IMPLEMENTED** | Ready for UAT |
| 2 | **Perpindahan Unit (Mutasi)** | `/hr/workforce-core/unit-transfers` | `GET/POST /api/v1/corporate/human-resource/personnel-action-orders` (`actionCategory=Mutation`)<br>`POST .../cancel-request`<br>`POST .../cancel` | **IMPLEMENTED** | Ready for UAT |
| 3 | **Rotasi Promosi** | `/hr/workforce-core/rotation-promotions` | `GET/POST /api/v1/corporate/human-resource/personnel-action-orders` (`Rotation` / `Promotion`) | **IMPLEMENTED** | Ready for UAT |
| 4 | **Uraian Tugas** | `/hr/workforce-core/employee-job-descriptions` | `GET /api/v1/corporate/human-resource/workforce-core/employees/{id}/effective-job-description?asOfDate=...` | **IMPLEMENTED** | Ready for UAT |
| 5 | **Atasan Langsung** | `/hr/workforce-core/employee-managers` | `GET /api/v1/corporate/human-resource/workforce-core/manager-assignments`<br>`POST .../personnel-actions/create?category=Rotation` | **IMPLEMENTED** | Ready for UAT |

---

## 4. Hasil Pemeriksaan Teknis & Kepatuhan Frontend

Eksekusi utilitas rekayasa `check-css-usage.mjs` di direktori `QuilvianSystemFrontendDev`:
```text
OK — 7 berkas diperiksa, 0 temuan.
  - src/components/view/hr/workforce-core/unit-transfers-view.jsx
  - src/components/view/hr/workforce-core/rotation-promotions-view.jsx
  - src/components/view/hr/workforce-core/employee-job-descriptions-view.jsx
  - src/components/view/hr/workforce-core/employee-managers-view.jsx
  - src/components/view/hr/workforce-core/personnel-action-order-create-view.jsx
  - src/components/view/hr/workforce-core/personnel-action-order-list-view.jsx
  - src/components/view/hr/workforce-core/initial-placement-batch-view.jsx

OK — 6 berkas diperiksa, 0 temuan.
  - src/app/hr/workforce-core/personnel-actions/create/page.jsx
  - src/app/hr/workforce-core/initial-placements/page.jsx
  - src/app/hr/workforce-core/unit-transfers/page.jsx
  - src/app/hr/workforce-core/rotation-promotions/page.jsx
  - src/app/hr/workforce-core/employee-job-descriptions/page.jsx
  - src/app/hr/workforce-core/employee-managers/page.jsx
```
Semua file frontend mematuhi aturan baku token Quilvian, validasi props komponen (`BaseButton`, `StatusBadge`, `BaseSelectField`, `BaseInputField`, `InformationAlert`, `SummaryGrid`, `DataTable`), dan tidak menggunakan raw Tailwind.

---

## 5. Ringkasan File Touched

### Backend (`NewQuilvianSystemBackend` @ `MHamzah`):
1. `Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdInitialPlacementBatchController.cs`
2. `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs`
3. `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdPersonnelActionOrderService.cs`
4. `Areas/Corporate/HumanResource/WorkforceCore/Services/WorkforceCoreCrossEmployeeQueryService.cs`
5. `Areas/Corporate/HumanResource/WorkforceCore/Services/WfpNewHireAssignmentService.cs`
6. `Areas/Corporate/HumanResource/WorkforceCore/Workers/PersonnelActionOrderScheduledApplyWorker.cs`
7. `Program.cs`
8. `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/INSTRUKSI_AGENT_HRD_MENU_07_PENEMPATAN_MUTASI_DAN_PROMOSI.md`

### Frontend (`QuilvianSystemFrontendDev` @ `HamzahV2`):
1. `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx`
2. `src/components/view/hr/workforce-core/personnel-action-order-create-view.jsx`
3. `src/components/view/hr/workforce-core/employee-job-descriptions-view.jsx`
4. `src/components/view/hr/workforce-core/employee-managers-view.jsx`
5. `src/app/hr/workforce-core/personnel-actions/create/page.jsx`
6. `src/components/view/hr/workforce-core/unit-transfers-view.jsx`
7. `src/components/view/hr/workforce-core/rotation-promotions-view.jsx`
8. `src/components/view/hr/workforce-core/personnel-action-order-list-view.jsx`
9. `src/app/hr/workforce-core/initial-placements/page.jsx`
10. `src/app/hr/workforce-core/unit-transfers/page.jsx`
11. `src/app/hr/workforce-core/rotation-promotions/page.jsx`
12. `src/app/hr/workforce-core/employee-job-descriptions/page.jsx`
13. `src/app/hr/workforce-core/employee-managers/page.jsx`
14. `src/lib/services/hr/initial-placement-batch-service.js`
15. `src/lib/services/hr/workforce-core-cross-employee-service.js`
16. `src/utils/menu-sidebar/menu-items.jsx`

---

## 6. Rekomendasi Langkah Selanjutnya

1. **Pengujian UAT Terpandu di Lingkungan Staging/Dev:**
   Jalankan 34 skenario UAT (`HRD07-UAT-01` s/d `34`) dan 20 skenario regresi (`HRD07-REG-01` s/d `20`) menggunakan browser Chromium live dengan peran pengguna yang sebenarnya (HR Admin, HR Manager, dan Pegawai Biasa).
2. **Kompilasi & Pull Request:**
   Bila pengujian UAT disetujui, mintalah izin eksplisit dari pengguna untuk menjalankan build verifikasi menyeluruh dan mengajukan Pull Request resmi ke branch integrasi.
