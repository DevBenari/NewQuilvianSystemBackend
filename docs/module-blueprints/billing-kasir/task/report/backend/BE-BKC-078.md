# Laporan Perubahan Backend — `BE-BKC-078`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-078` |
| Judul | Status `PERLU_TINDAK_LANJUT` pada Review Variance & Endpoint Penyelesaian Tindak Lanjut Shift Kasir |
| Slice | Gelombang `MVP-34` — Shift Kasir: Blocking Selisih Kas dan Status Tindak Lanjut (`docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `Gelombang MVP-34`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-078` |
| Trace | `BKC-DEC-124`, `BKC-DEC-125`, `BKC-DEC-126` (keputusan bisnis approved 25 September 2026 oleh Yasmin); `BP-005`, `RULE-011` dokumen `Shift Kasir (3).md` |
| Contract version | `BIL-API-1.6` (draft), `BIL-STATE-1.5` (draft), `BIL-VALIDATION-1.5` (draft), `BIL-PERMISSION-1.3` (draft) |
| Dependency | `BE-BKC-077` |
| Klasifikasi | `LIGHT` — satu repository; berkas diperiksa 4; berkas diubah 4; penambahan DTO, endpoint baru, dan logika transisi status; nol perubahan skema database; nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShiftCommand.cs`, `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Cashier/Dtos/CashierShiftDtos.cs`, `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs`, `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Cashier/Controllers/CashierShiftsController.cs` |
| Tanggal | 2026-09-25 |
| Status | 🟡 **SEBAGIAN — source code selesai, menunggu kompilasi build mandiri dan verifikasi manual pengguna.** Penambahan field `Outcome` pada `ReviewVarianceRequest`, transisi status `PERLU_TINDAK_LANJUT` pada `ReviewVarianceAsync`, DTO `ResolveShiftFollowUpRequest`, method `ResolveFollowUpAsync`, audit command `RESOLVE_FOLLOW_UP`, dan controller endpoint `POST {id}/resolve-follow-up` telah terpasang lengkap. Kompilasi `dotnet build` diserahkan kepada pengguna sesuai instruksi. |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Cashier` |
| Entity / Model | `BilCashierShift`, `BilCashVarianceReview`, `BilCashierShiftCommand` |
| Prefix Registry | `Bil` — status `ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Applicability | `TOUCHED LEGACY` (ekstensi endpoint dan state transition pada service transaksi existing) |
| QBE Rules Berlaku | `QBE-MOD-001`, `QBE-NAM-001`, `QBE-API-001`, `QBE-SEC-001` — konformansi dipenuhi (nol generic repository, nol bypass context, idempotency transaction lock, audit command persisten) |

---

## 1. Masalah dan Kebutuhan Bisnis

Sebelum perubahan ini:
1. Ketika Supervisor/Kepala Kasir meninjau shift berselisih kas (`CLOSED_WITH_VARIANCE`) melalui `POST /variance-reviews`, sistem secara sepihak langsung memindahkan status shift ke `REVIEWED` terlepas dari apakah selisih tersebut sudah tuntas dijelaskan atau masih memerlukan investigasi/tindak lanjut fisik.
2. Tidak ada status peralihan formal untuk shift yang masih butuh penelusuran bukti fisik struk atau audit berkas transaksi kasir (`BP-005`, `RULE-011`).
3. Akibatnya, supervisor tidak memiliki mekanisme bertahap untuk mencatat temuan sementara tanpa langsung membebaskan kasir/register dari pengawasan ketat.
4. Ketika status `PERLU_TINDAK_LANJUT` diperkenalkan (per `BKC-DEC-124`), dibutuhkan endpoint aksi susulan yang definitif untuk menyelesaikan investigasi tersebut menjadi `REVIEWED` setelah bukti diverifikasi (`BKC-DEC-125`).

Sesuai `BKC-DEC-124`, `BKC-DEC-125`, dan `BKC-DEC-126`:
- Modal review selisih kas supervisor kini mendukung 2 opsi hasil: `"VERIFIED"` (`REVIEWED`) dan `"NEEDS_FOLLOW_UP"` (`PERLU_TINDAK_LANJUT`).
- Shift berstatus `PERLU_TINDAK_LANJUT` tetap memblokir pembukaan shift baru pada kasir atau register tersebut sesuai validasi `BE-BKC-077`.
- Kepala Kasir/Supervisor dapat memanggil endpoint `POST {id}/resolve-follow-up` dengan menyertakan `VerificationNote` wajib (pesan validasi: `"Catatan verifikasi wajib diisi."`) untuk memindahkan status shift ke `REVIEWED`.

---

## 2. Proses Bisnis yang Diterapkan

### 2.1 Alur Review Variance dengan Pilihan Hasil (Outcome)
1. Supervisor/Kepala Kasir membuka modal review selisih kas untuk shift berstatus `CLOSED_WITH_VARIANCE`.
2. Supervisor memasukkan resolusi, alasan, serta memilih hasil review:
   - Jika `Outcome == "NEEDS_FOLLOW_UP"`, shift dipindahkan ke status `PERLU_TINDAK_LANJUT`. Kasir dan register tetap terblokir dari pembukaan shift baru per `BE-BKC-077`.
   - Jika `Outcome == "VERIFIED"` (atau kosong/default), shift dipindahkan ke status `REVIEWED`. Blokir pembukaan shift baru terlepas.
3. Transaksi dicatat pada `BilCashVarianceReview` dan diaudit melalui `BilCashierShiftCommand` tipe `REVIEW_VARIANCE`.

### 2.2 Alur Penyelesaian Tindak Lanjut (Resolve Follow-Up)
1. Kepala Kasir/Supervisor membuka riwayat shift atau modal tindak lanjut untuk shift berstatus `PERLU_TINDAK_LANJUT`.
2. Pengguna mengisi catatan verifikasi hasil audit (`VerificationNote`) dan menekan tombol simpan/selesaikan.
3. Permintaan dikirim ke `POST /api/v1/health-services/billing-management/cashier/shifts/{id}/resolve-follow-up` dengan header `Idempotency-Key`.
4. Server mengambil lock advisory `BIL_CASHIER_SHIFT_COMMAND_{idempotencyKey}` dan `BIL_CASHIER_SHIFT_{id}`.
5. Server memvalidasi:
   - Request DTO valid (`VerificationNote` tidak boleh kosong, jika kosong melempar `"Catatan verifikasi wajib diisi."`).
   - `ExpectedRowVersion` cocok dengan versi terkini shift.
   - Status shift **wajib** `PERLU_TINDAK_LANJUT`. Jika bukan, tolak dengan HTTP 422: *"Hanya shift berstatus PERLU_TINDAK_LANJUT yang dapat diselesaikan."*.
6. Server mencatat riwayat verifikasi baru di `BilCashVarianceReview` (merekam jejak penyelesaian lengkap dengan timestamp dan ID reviewer).
7. Server memperbarui status shift menjadi `REVIEWED`, memperbarui `RowVersion`, dan menyentuh waktu modifikasi (`UpdateDateTime`).
8. Server mencatat audit command `CashierShiftCommandTypes.ResolveFollowUp` (`"RESOLVE_FOLLOW_UP"`).
9. Server mengembalikan respon `ApiResponse<CashVarianceResponse>` dengan status shift yang sudah terbarui ke `REVIEWED`.
10. Blokir pembukaan shift baru pada kasir atau register terkait secara otomatis terangkat karena status shift sudah menjadi `REVIEWED`.

---

## 3. Rincian Perubahan Source Code

### 3.1 Berkas yang Diperiksa
- `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShiftCommand.cs`
- `Areas/HealthServices/BillingManagement/Cashier/Dtos/CashierShiftDtos.cs`
- `Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs`
- `Areas/HealthServices/BillingManagement/Cashier/Controllers/CashierShiftsController.cs`

### 3.2 Berkas yang Diubah

| Berkas | Lokasi | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShiftCommand.cs` | Baris 45 | Menambahkan konstanta `public const string ResolveFollowUp = "RESOLVE_FOLLOW_UP";` pada `CashierShiftCommandTypes`. |
| `Areas/HealthServices/BillingManagement/Cashier/Dtos/CashierShiftDtos.cs` | Baris 66 & 71-78 | Menambahkan properti `public string? Outcome { get; set; }` pada `ReviewVarianceRequest`. Menambahkan kelas DTO `ResolveShiftFollowUpRequest` dengan field `ExpectedRowVersion`, `VerificationNote` (`[Required(ErrorMessage = "Catatan verifikasi wajib diisi."), MaxLength(500)]`), `CorrelationId`, dan `CausationId`. |
| `Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs` | Baris 577, 620-623 | Pada `ReviewVarianceAsync`: menyertakan `Outcome` dalam perhitungan `payloadHash`; mengevaluasi percabangan outcome: jika `"NEEDS_FOLLOW_UP"` maka status menjadi `CashierShiftStatuses.PerluTindakLanjut`, selain itu menjadi `CashierShiftStatuses.Reviewed`. |
| `Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs` | Baris 678-804 | Mengimplementasikan method baru `ResolveFollowUpAsync`: mengunci idempotensi dan shift, memvalidasi status `PERLU_TINDAK_LANJUT`, mencatat `BilCashVarianceReview`, mengubah status shift menjadi `REVIEWED`, mencatat `BilCashierShiftCommand` dengan tipe `RESOLVE_FOLLOW_UP`, dan mengembalikan `CashVarianceResponse`. |
| `Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs` | Baris 1120-1133 | Menambahkan private helper `ValidateResolveFollowUp` dengan validasi `ValidateText(request.VerificationNote, "Catatan verifikasi")`. |
| `Areas/HealthServices/BillingManagement/Cashier/Controllers/CashierShiftsController.cs` | Baris 212-237 | Menambahkan endpoint `POST {id:guid}/resolve-follow-up` dengan atribut `[AccessAction("ResolveFollowUp", "Resolve Cashier Shift Follow Up", AccessType = AccessTypes.Update, SortOrder = 11)]`, `[AccessPermission("CashierShift", "Review")]`, dan respon status 200 OK. |

### 3.3 Dampak Kontrak, Database, dan Keamanan
- **Kontrak API:**
  - `POST /shifts/{id}/variance-reviews`: Menerima properti opsional `Outcome` (`"NEEDS_FOLLOW_UP"` atau `"VERIFIED"`). Respon `ApiResponse<CashVarianceResponse>` mengembalikan status shift `PERLU_TINDAK_LANJUT` atau `REVIEWED`.
  - `POST /shifts/{id}/resolve-follow-up`: Endpoint baru untuk aksi penyelesaian investigasi susulan, menerima `ResolveShiftFollowUpRequest` dan mengembalikan `ApiResponse<CashVarianceResponse>` dengan status shift `REVIEWED`.
- **Database:** `NOT APPLICABLE` — **Nol Migration**. Nilai status baru disimpan pada kolom string `BilCashierShift.Status` existing (`VARCHAR(30)`). Command dicatat pada `BilCashierShiftCommand` existing.
- **Keamanan / RBAC:** Dilindungi oleh hak akses `[AccessPermission("CashierShift", "Review")]` dan terdaftar pada matriks role access dengan `[AccessAction("ResolveFollowUp", ...)]`.

---

## 4. Spesifikasi API Bergaya Swagger

Grup Tag: `[Tags("Health Services / Billing Management / Cashier / Shifts")]`

| Method | Path | Deskripsi | Otorisasi & Hak Akses | Request Body / Header | Status & Response Body |
| :---: | --- | --- | --- | --- | :---: |
| `POST` | `/api/v1/health-services/billing-management/cashier/shifts/{id:guid}/variance-reviews` | Meninjau selisih kas penutupan shift dengan pilihan hasil verifikasi selesai atau perlu tindak lanjut | `[Authorize]`, `[AccessPermission("CashierShift", "Review")]` | Header: `Idempotency-Key`<br/>Body: `ReviewVarianceRequest` (`ExpectedRowVersion`, `Resolution`, `Reason`, `Outcome`, `CorrelationId`, `CausationId`) | `201 Created`: `ApiResponse<CashVarianceResponse>` (`Shift.Status`: `REVIEWED` atau `PERLU_TINDAK_LANJUT`)<br/>`422 Unprocessable`: `ApiResponse<object>` |
| `POST` | `/api/v1/health-services/billing-management/cashier/shifts/{id:guid}/resolve-follow-up` | Menyelesaikan investigasi tindak lanjut shift selisih kas dan memindahkan status definitif ke `REVIEWED` | `[Authorize]`, `[AccessAction("ResolveFollowUp", ...)]`, `[AccessPermission("CashierShift", "Review")]` | Header: `Idempotency-Key`<br/>Body: `ResolveShiftFollowUpRequest` (`ExpectedRowVersion`, `VerificationNote`, `CorrelationId`, `CausationId`) | `200 OK`: `ApiResponse<CashVarianceResponse>` (`Shift.Status`: `REVIEWED`)<br/>`422 Unprocessable`: `ApiResponse<object>` (*"Catatan verifikasi wajib diisi."*) |

---

## 5. Verifikasi dan Kriteria Penerimaan

| Skenario Pengujian | Hasil Analisis Statis / Kode | Klasifikasi | Catatan |
| --- | --- | :---: | --- |
| Review variance dengan `Outcome = "NEEDS_FOLLOW_UP"` | Shift beralih status ke `PERLU_TINDAK_LANJUT` | `PASS` | Sesuai `BKC-DEC-124` |
| Review variance dengan `Outcome = "VERIFIED"` atau kosong | Shift beralih status ke `REVIEWED` | `PASS` | Sesuai alur verifikasi normal |
| Eksekusi `resolve-follow-up` pada shift bukan `PERLU_TINDAK_LANJUT` | Ditolak dengan HTTP 422: *"Hanya shift berstatus PERLU_TINDAK_LANJUT yang dapat diselesaikan."* | `PASS` | Sesuai `BIL-STATE-1.5` |
| Eksekusi `resolve-follow-up` dengan `VerificationNote` kosong | Ditolak anotasi model & validator: *"Catatan verifikasi wajib diisi."* | `PASS` | Sesuai `BKC-DEC-126` |
| Eksekusi `resolve-follow-up` yang valid | Status shift beralih ke `REVIEWED`, lock shift dilepas, riwayat review & audit command tercatat | `PASS` | Sesuai `BKC-DEC-125` |
| Idempotensi `resolve-follow-up` | Header `Idempotency-Key` sama mengembalikan replay tanpa duplikasi mutasi | `PASS` | Menggunakan replay cache `ReplayAsync` |
| Kompilasi `dotnet build` | Ditangguhkan ke pengguna | `PENDING_USER_BUILD` | Sesuai instruksi pengguna agar build dijalankan mandiri |

---

## 6. Acceptance Criteria & Definition of Done

| Butir Definition of Done | Status | Bukti / Rujukan |
| --- | :---: | --- |
| Nilai status baru `CashierShiftStatuses.PerluTindakLanjut` terdaftar | Terpenuhi | `BilCashierShift.cs:33` |
| DTO `ReviewVarianceRequest.Outcome` dan `ResolveShiftFollowUpRequest` terpasang | Terpenuhi | `CashierShiftDtos.cs:66,71-78` |
| Logika transisi status di `ReviewVarianceAsync` mendukung `PERLU_TINDAK_LANJUT` | Terpenuhi | `CashierShiftService.cs:620-623` |
| Method `ResolveFollowUpAsync` memvalidasi status awal dan catatan verifikasi | Terpenuhi | `CashierShiftService.cs:678-804` |
| Endpoint `POST {id}/resolve-follow-up` terdaftar dengan `[AccessAction]` & `[AccessPermission]` | Terpenuhi | `CashierShiftsController.cs:212-237` |
| Audit command `RESOLVE_FOLLOW_UP` tercatat persisten pada `BilCashierShiftCommand` | Terpenuhi | `BilCashierShiftCommand.cs:45`, `CashierShiftService.cs:774` |
| Nol migration basis data | Terpenuhi | Menggunakan kolom string existing `BilCashierShift.Status` |
| Laporan tracked tersedia | Terpenuhi | Berkas laporan ini |
| Kompilasi mandiri pengguna | Menunggu build pengguna | Instruksi eksplisit pengguna |
