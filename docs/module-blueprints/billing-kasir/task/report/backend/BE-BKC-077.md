# Laporan Perubahan Backend — `BE-BKC-077`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-077` |
| Judul | Blocking Pembukaan Shift Baru untuk Selisih Kas Belum Direview |
| Slice | Gelombang `MVP-34` — Shift Kasir: Blocking Selisih Kas dan Status Tindak Lanjut (`docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `Gelombang MVP-34`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-077` |
| Trace | `BKC-DEC-123`, `BKC-DEC-126` (keputusan bisnis approved 25 September 2026 oleh Yasmin); `RULE-012`, `BP-007` dokumen `Shift Kasir (3).md` |
| Contract version | `BIL-API-1.6` (draft), `BIL-VALIDATION-1.5` (draft) |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — satu repository; berkas diperiksa 2; berkas diubah 2; logika query validasi tambahan pra-insert shift; nol perubahan schema database; nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs`, `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs` |
| Tanggal | 2026-09-25 |
| Status | 🟡 **SEBAGIAN — source code selesai, menunggu kompilasi build mandiri dan verifikasi manual pengguna.** Penegakan query penjaga pada `OpenAsync` dan penambahan konstanta status `PerluTindakLanjut` telah terpasang persis sesuai spesifikasi roadmap. Kompilasi `dotnet build` diserahkan kepada pengguna sesuai instruksi. |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Cashier` |
| Entity / Model | `BilCashierShift` |
| Prefix Registry | `Bil` — status `ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Applicability | `TOUCHED LEGACY` (ekstensi logika pada service transaksi existing) |
| QBE Rules Berlaku | `QBE-MOD-001`, `QBE-NAM-001`, `QBE-API-001`, `QBE-SEC-001` — konformansi dipenuhi (nol generic repository, nol bypass context) |

---

## 1. Masalah yang Diperbaiki

Sebelum perubahan ini, method `CashierShiftService.OpenAsync` (baris 67-71) hanya memeriksa apakah kasir atau register memiliki shift berstatus aktif (`OPEN` atau `REOPENED` via `ActiveShifts()`). 
Apabila seorang kasir atau loket menutup shift sebelumnya dengan status `CLOSED_WITH_VARIANCE` (memiliki selisih kas fisik vs sistem) dan selisih tersebut **belum direview** oleh Supervisor/Kepala Kasir, sistem mengizinkan pembukaan shift baru secara bebas. 

Kondisi ini bertentangan dengan prinsip kontrol pertanggungjawaban kas rumah sakit (`RULE-012` dan `BP-007` pada dokumen `Shift Kasir (3).md`) serta keputusan bisnis `BKC-DEC-123`:
> "Tolak pembukaan shift baru bila kasir ATAU register yang sama memiliki shift berstatus `CLOSED_WITH_VARIANCE` atau `PERLU_TINDAK_LANJUT` yang belum berstatus `REVIEWED`."

---

## 2. Proses Bisnis yang Diterapkan

1. Kasir login dan memilih loket (register) lalu mengirimkan permintaan pembukaan shift (`POST /shifts/open`).
2. Server mengambil advisory lock transaksi `BIL_CASHIER_{actorUserId}` dan `BIL_REGISTER_{RegisterId}` untuk mencegah race condition.
3. Server memeriksa apakah kasir atau register masih memiliki shift aktif (`OPEN` / `REOPENED`). Jika ada, tolak dengan pesan: *"Kasir atau register masih memiliki shift aktif."* (HTTP 409 Conflict).
4. **Validasi Baru (`BKC-DEC-123`):** Server memeriksa apakah terdapat baris `BilCashierShift` aktif (`!IsDelete`) milik `CashierId == actorUserId` ATAU `RegisterId == request.RegisterId` yang berstatus `CLOSED_WITH_VARIANCE` atau `PERLU_TINDAK_LANJUT`.
5. Jika ditemukan, sistem melempar `CashierShiftConflictException` dengan pesan:
   **`"Kasir atau register masih memiliki shift yang menunggu review selisih kas."`**
   Pesan ini ditangkap oleh handler `Failure` pada `CashierShiftsController` dan dikembalikan ke klien sebagai HTTP 409 Conflict.
6. Jika tidak ditemukan (seluruh shift sebelumnya telah `REVIEWED`, atau `CLOSED` seimbang tanpa selisih, atau `HANDED_OVER`), sistem melanjutkan pembuatan baris shift baru dengan status `OPEN`.

---

## 3. Rincian Perubahan Source Code

### 3.1 Berkas yang Diperiksa
- `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs`
- `Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs`
- `Areas/HealthServices/BillingManagement/Cashier/Controllers/CashierShiftsController.cs`
- `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md`

### 3.2 Berkas yang Diubah

| Berkas | Lokasi | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs` | Baris 33 | Menambahkan konstanta status baru: `public const string PerluTindakLanjut = "PERLU_TINDAK_LANJUT";` pada kelas `CashierShiftStatuses`. |
| `Areas/HealthServices/BillingManagement/Cashier/Services/CashierShiftService.cs` | Baris 73-80 | Menambahkan query validasi pra-buka shift di dalam `OpenAsync`: memeriksa baris `BilCashierShifts` untuk kasir atau register dengan status `ClosedWithVariance` atau `PerluTindakLanjut`, dan melempar `CashierShiftConflictException("Kasir atau register masih memiliki shift yang menunggu review selisih kas.")`. |

### 3.3 Dampak Kontrak, Database, dan Keamanan
- **Kontrak API:** Penambahan respon error fungsional HTTP 409 Conflict dengan pesan standar penolakan selisih kas. Skema DTO `OpenShiftRequest` dan `CashierShiftResponse` tidak mengalami perubahan.
- **Database:** `NOT APPLICABLE` — **Nol Migration**. Kolom `BilCashierShift.Status` bertipe `VARCHAR(30)` sehingga mendukung nilai konstanta baru tanpa perubahan skema fisik.
- **Keamanan / RBAC:** Tidak berubah — endpoint `Open` tetap diproteksi atribut `[AccessPermission("CashierShift", "Create")]`.

---

## 4. Dokumentasi Endpoint

Grup Tag: `[Tags("Health Services / Billing Management / Cashier / Shifts")]`

| Method | Path | Deskripsi | Otorisasi | Request / Header | Response & Status |
| :---: | --- | --- | --- | --- | :---: |
| `POST` | `/api/v1/health-services/billing-management/cashier/shifts/open` | Buka shift kasir baru dengan validasi blokir selisih kas | `[Authorize]`, `[AccessPermission("CashierShift", "Create")]` | Header: `Idempotency-Key`<br/>Body: `OpenShiftRequest` (`RegisterId`, `OpeningCash`, `CorrelationId`, `CausationId`) | `201 Created`: `ApiResponse<CashierShiftResponse>`<br/>`409 Conflict`: `ApiResponse<object>` (*"Kasir atau register masih memiliki shift yang menunggu review selisih kas."*) |

---

## 5. Verifikasi dan Kriteria Penerimaan

| Skenario Pengujian | Hasil Analisis Statis / Kode | Klasifikasi | Catatan |
| --- | --- | :---: | --- |
| Kasir memiliki shift `CLOSED_WITH_VARIANCE` yang belum direview | Ditolak melempar `CashierShiftConflictException` dengan pesan tepat | `PASS` | Sesuai `BKC-DEC-123` |
| Register memiliki shift `CLOSED_WITH_VARIANCE` yang belum direview | Ditolak melempar `CashierShiftConflictException` | `PASS` | Sesuai `BKC-DEC-123` |
| Kasir/register memiliki shift `PERLU_TINDAK_LANJUT` | Ditolak melempar `CashierShiftConflictException` | `PASS` | Sesuai `BKC-DEC-124` & `RULE-012` |
| Kasir/register dengan riwayat telah `REVIEWED` atau `CLOSED` seimbang | Lolos validasi, shift baru dibuat | `PASS` | Sesuai `RULE-013` |
| Pesan respon HTTP 409 | `"Kasir atau register masih memiliki shift yang menunggu review selisih kas."` | `PASS` | Ditangani otomatis oleh `Failure` method di controller |
| Kompilasi `dotnet build` | Ditangguhkan ke pengguna | `PENDING_USER_BUILD` | Sesuai instruksi pengguna agar build dijalankan mandiri |

---

## 6. Acceptance Criteria & Definition of Done

| Butir | Status | Bukti / Rujukan |
| --- | :---: | --- |
| Pembukaan shift ditolak jika kasir memiliki shift berselisih belum direview | Terpenuhi (source) | `CashierShiftService.cs:73-80` |
| Pembukaan shift ditolak jika register memiliki shift berselisih belum direview | Terpenuhi (source) | `CashierShiftService.cs:73-80` |
| Pembukaan shift ditolak jika berstatus `PERLU_TINDAK_LANJUT` | Terpenuhi (source) | `CashierShiftService.cs:73-80` |
| Pembukaan shift diizinkan bila shift sebelumnya sudah `REVIEWED` atau seimbang | Terpenuhi (source) | `CashierShiftService.cs:73-80` |
| Pesan error persis sesuai `BKC-DEC-123` | Terpenuhi | `"Kasir atau register masih memiliki shift yang menunggu review selisih kas."` |
| Nol migration basis data | Terpenuhi | Nol perubahan berkas EF Core / Migration |
| Laporan tracked tersedia | Terpenuhi | Berkas laporan ini |
| Kompilasi dan verifikasi mandiri | Menunggu build pengguna | Instruksi eksplisit pengguna |
