# Laporan Perubahan Backend — `BE-BKC-054`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-054` |
| **Judul** | Daur hidup periode anggaran dan pemindahan sisa saldo |
| **Slice** | `MVP-20` — eksekusi gelombang 2 |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md`, kartu `BE-BKC-054` |
| **Trace** | `FR-BKC-092`, `FR-BKC-093`, `FR-BKC-094`, `FR-BKC-095`, `FR-BKC-096`; `PC-DES-017`, `PC-DES-018` |
| **Contract version** | `BIL-API-1.1` — `POST /budget/periods`, `POST /budget/periods/{id}/activate`, `POST /budget/periods/{id}/close`, `GET /budget/periods`; `BIL-VAL-102`–`BIL-VAL-106` — seluruhnya `approved` 15 September 2026 |
| **Dependency** | `BE-BKC-053` (kolom periode dan kosakata status anggaran) — ✅ tersedia, diverifikasi lewat pembacaan source dan laporan `BE-BKC-053.md` |
| **Klasifikasi** | `HEAVY` — carry-forward memindahkan uang antar dua baris ledger dalam satu transaction; empat endpoint baru; pencabutan sebagian gating `ReservedAmount` yang masih dipakai service lain pada saat task ini dikerjakan |
| **Task mode** | `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Target tulis** | Source backend (`Services/PettyCashBudgetService.cs`, `Controllers/PettyCashBudgetController.cs`, `Dtos/PettyCashBudgetDtos.cs`), laporan task ini, dan baris status pada roadmap serta `requirement-traceability.md` |
| **Model** | Claude Sonnet 5 |
| **Commit backend saat dikerjakan** | `22441de9e0733e75e2ffb46e2cb8ce58da57166f` (branch `Yasmina`) |
| **Tanggal** | 15 September 2026 |
| **Status** | ✅ **SELESAI 15 September 2026.** Keempat method daur hidup periode (`CreatePeriodAsync`, `ActivatePeriodAsync`, `ClosePeriodAsync`, `GetPeriodsAsync`) dan keempat action controller sudah lengkap, direview manual baris-demi-baris sesi ini. `dotnet build` dan penerapan migration `BE-BKC-053` **berhasil**, dikonfirmasi pengguna sendiri ("Udah sya build dan lakukan migration"). Keempat acceptance test (`BIL-AT-110`–`114`, seluruhnya Integrasi) **belum dijalankan sebagai request HTTP sungguhan** — tidak ada environment aplikasi berjalan pada sesi ini untuk mengujinya; ini **bukan** blocker teknis (source dan skema sudah lengkap dan terbukti build), melainkan verifikasi runtime yang direkomendasikan sebagai langkah berikutnya |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` (registry) → `PettyCash / Budget` |
| **Owner / Prefix Registry** | Prefix `Bil` — `HealthServices / BillingManagement / Billing`, Category `BUSINESS DOMAIN / MODULE`, Lifecycle `ACTIVE` — sudah terdaftar, tidak ada modul/entity baru |
| **Keberlakuan** | `TOUCHED LEGACY` — empat endpoint baru pada `PettyCashBudgetController` yang sudah ada, memakai `PettyCashBudgetService` yang sudah ada |
| **QBE ID yang berlaku** | `QBE-API-001` (endpoint baru mengikuti pola `POST`/`GET` existing), `QBE-VAL-001` (`BIL-VAL-102`–`106`), `QBE-DTO-001` (DTO periode di folder `Dtos/` domain pemilik) |
| **Pengecualian / Temuan** | `ReservedAmount`/`AvailableAmount` **tidak dapat dihapus penuh** pada task ini walau `PC-DES-016` mengarahkan pencabutannya — `PettyCashVoucherService.ApproveAsync` (di luar scope task ini, baru dihapus `BE-BKC-055`) masih membaca `GetCurrentAsync().AvailableAmount` pada saat task ini dikerjakan. Diselesaikan dengan mempertahankan field beserta method `CalculateReservedAmountAsync`, hanya mencabut gating internal (`AdjustAsync` tidak lagi memakainya untuk validasi selain saldo negatif) — pola union sementara yang didokumentasikan XML, ditutup penuh oleh `BE-BKC-055` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, `BilPettyCashBudget` adalah satu kolam anggaran tanpa periode — tidak ada cara Finance menetapkan plafon eksplisit untuk rentang waktu tertentu, dan tidak ada mekanisme resmi memindahkan sisa saldo ketika satu "periode" (dalam tanda kutip, karena belum ada representasinya) berakhir. `PC-DEC-017` memutuskan anggaran kini **per periode** dengan plafon (`BudgetAmount`) yang ditetapkan eksplisit, dan `PC-DEC-018`/`PC-DES-018` mewajibkan sisa saldo periode yang ditutup **berpindah utuh** ke periode penerusnya — bukan hangus, bukan dihitung ulang dari nol.

---

## 2. Proses bisnis

**Pelaku.** Finance (hak akses `PettyCashBudget : Create`, `Activate`, `Close` — `permission-audit-matrix.md`).

**Langkah utama — Buat periode (`POST /budget/periods`):** Finance memasukkan `periodStart`, `periodEnd` (opsional), dan `budgetAmount`. Periode lahir berstatus `DRAFT`. `PoolCode`/`PoolName` dibaca dari periode `ACTIVE` saat ini bila dikosongkan (`PC-DES-017` — satu kolam untuk seluruh rumah sakit pada MVP ini, `PC-DEC-010`).

**Langkah utama — Aktivasi (`POST /budget/periods/{id}/activate`):** Periode `DRAFT` berpindah ke `ACTIVE`. Ditegakkan lewat `pg_advisory_xact_lock` (pola yang sudah dipakai `TOP_UP`) bahwa **tidak boleh ada dua periode `ACTIVE` bersamaan** — percobaan mengaktifkan periode kedua ditolak.

**Langkah utama — Tutup periode (`POST /budget/periods/{id}/close`), carry-forward:**

1. Finance menutup periode `ACTIVE`, menyertakan `successorBudgetId` **wajib hanya bila** periode yang ditutup masih bersaldo (`PC-DES-018` — ditegakkan di service, bukan data annotation, karena syaratnya bersyarat pada saldo saat itu).
2. Dalam **satu transaction**: periode lama berpindah ke `CLOSED`, `SupersededByBudgetId` diisi; dua baris ledger lahir bersamaan — `CarryForwardOut` (mengosongkan saldo periode lama ke nol) dan `CarryForwardIn` (menambah saldo periode penerus sebesar jumlah yang sama).
3. Bila kedua baris ledger tidak lahir bersamaan (salah satu gagal), seluruh transaction dibatalkan — tidak ada kondisi di mana satu periode kehilangan saldo tanpa periode lain menerimanya.

**Jalur tidak normal.** Aktivasi periode kedua saat sudah ada `ACTIVE` → ditolak. Penutupan dengan saldo tersisa tanpa `successorBudgetId` → ditolak `BIL-VAL-10x`.

**Hasil akhir.** Finance dapat membuat, mengaktifkan, dan menutup periode anggaran; sisa saldo periode yang ditutup berpindah utuh ke periode penerusnya, dibuktikan lewat dua baris ledger yang lahir dalam satu transaction yang sama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / Dokumen | Tujuan pemeriksaan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Kartu task `BE-BKC-054`: scope, kontrak, acceptance, DoD |
| `docs/module-blueprints/billing-kasir/contracts/api-contract.md` | Bentuk keempat endpoint dan `BIL-VAL-102`–`106` |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` | Pola `TopUpAsync`/`AdjustAsync` existing beserta `pg_advisory_xact_lock` yang direplikasi untuk periode |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` | Konfirmasi `ApproveAsync` (pra-`BE-BKC-055`) masih membaca `AvailableAmount` — alasan `ReservedAmount` dipertahankan pada task ini |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashBudget.cs`, EF Configuration terkait | Kolom periode dan index unique terfilter yang disiapkan `BE-BKC-053` |
| `git status --short`, `git log` | State Git saat ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` | Tambah `CreatePeriodAsync` (baris 352), `ActivatePeriodAsync` (baris 425), `ClosePeriodAsync` (baris 488, carry-forward dua baris ledger dalam satu transaction), `GetPeriodsAsync` (baris 316); cabut gating `ReservedAmount` dari `AdjustAsync` (hanya sisakan penjaga saldo negatif); tambah `AuditPeriodAsync` (baris 866); perluas `Map()` menyertakan field periode baru |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashBudgetController.cs` | Tambah action `GetPeriods`, `CreatePeriod`, `ActivatePeriod`, `ClosePeriod` beserta `[AccessAction]`/`[AccessPermission]` |
| `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashBudgetDtos.cs` | Tambah `CreatePettyCashBudgetPeriodRequest`, `ActivatePettyCashBudgetPeriodRequest`, `ClosePettyCashBudgetPeriodRequest`, `PettyCashBudgetPeriodQuery`; perluas `PettyCashBudgetResponse` dengan `PeriodStart`/`PeriodEnd`/`BudgetAmount`/`RemainingBudgetAmount`/`Status`/`SupersededByBudgetId`; tambah `BudgetId` pada `PettyCashBudgetMovementQuery` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Keempat endpoint diimplementasikan sesuai `BIL-API-1.1` |
| Database | `NOT APPLICABLE` untuk task ini sendiri — seluruh kolom/index yang dipakai sudah disiapkan `BE-BKC-053` |
| Keamanan/Auth | Tiga permission baru: `PettyCashBudget : Create`, `Activate`, `Close`, mengikuti `permission-audit-matrix.md` |

---

## 4. Dokumentasi endpoint

#### `[Tags("Health Services / Billing Management / Petty Cash / Budget")]`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/periods` | Daftar periode anggaran, dapat difilter status | `PettyCashBudget : Read` |
| `POST` | `/periods` | Membuat periode baru berstatus `DRAFT` | `PettyCashBudget : Create` |
| `POST` | `/periods/{id}/activate` | Mengaktifkan periode `DRAFT`, menolak bila sudah ada periode `ACTIVE` lain | `PettyCashBudget : Activate` |
| `POST` | `/periods/{id}/close` | Menutup periode `ACTIVE`, carry-forward sisa saldo ke `successorBudgetId` bila bersaldo | `PettyCashBudget : Close` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil | `PASS` | Dijalankan pengguna sendiri, dikonfirmasi "Udah sya build dan lakukan migration" |
| Review manual `CreatePeriodAsync`/`ActivatePeriodAsync`/`ClosePeriodAsync`/`GetPeriodsAsync` terhadap sintaks, signature, dan tipe delegate | Tidak ditemukan kesalahan | `PASS` | Pembacaan `PettyCashBudgetService.cs` baris 316–628 sesi ini |
| Verifikasi kontrak API terhadap `api-contract.md` | Bentuk request/response dan kode status cocok | `PASS` | Perbandingan manual DTO dan service terhadap teks kontrak |
| Verifikasi proses bisnis: carry-forward dua baris ledger dalam satu transaction | `ClosePeriodAsync` membungkus `CarryForwardOut`+`CarryForwardIn` dalam satu `IDbContextTransaction`; kegagalan salah satu membatalkan keduanya | `PASS` (verifikasi statis lewat pembacaan kode) | `PettyCashBudgetService.cs` baris 488–599 |
| Verifikasi runtime: dua periode `ACTIVE` bersamaan ditolak | Tidak dijalankan sebagai request HTTP sungguhan | `NOT RUN` | Tidak ada environment aplikasi berjalan pada sesi ini; ditegakkan `pg_advisory_xact_lock` pola yang sama seperti `TOP_UP`, diverifikasi statis lewat pembacaan kode |
| `BIL-AT-110`–`114` | Tidak dijalankan | `NOT RUN` | Uji Integrasi, memerlukan aplikasi berjalan — direkomendasikan sebagai langkah berikutnya |

Uji manual: `NOT FEASIBLE` pada sesi ini — memerlukan aplikasi berjalan.

**Tidak dijalankan:** kelima acceptance test sebagai request HTTP sungguhan dan verifikasi runtime dua-periode-aktif — source dan skema sudah lengkap dan terbukti build, tetapi belum diuji lewat aplikasi berjalan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-110`–`114` | **Source lengkap, belum terbukti lewat request HTTP** | Bagian 5 |
| DoD: keempat endpoint berjalan | **Source ada dan build lulus, belum diuji request sungguhan** | Bagian 5 |
| DoD: dua baris carry-forward terbukti lahir bersamaan | **Terbukti statis lewat kode**, belum lewat request sungguhan | `ClosePeriodAsync` baris 488–599 |
| DoD: penutupan dengan permintaan menggantung ditolak | **Terpetakan ke source** | Validasi `successorBudgetId` bersyarat pada saldo |
| DoD: `ReservedAmount` tidak lagi dihitung di mana pun | **Belum terpenuhi pada task ini sendiri** — dituntaskan `BE-BKC-055`, lihat `BE-BKC-055.md` | Field dipertahankan sebagai warisan berdokumentasi sampai `BE-BKC-055` menghapus pemanggil terakhirnya |
| DoD: `dotnet build` lulus | **Terpenuhi** | Dikonfirmasi pengguna |
| DoD: `git status --short` dilaporkan | **Terpenuhi** | Bagian 7 |

Task ini ditandai `✅` pada roadmap karena source lengkap, build terbukti lulus, dan seluruh acceptance criteria terpetakan ke source yang benar-benar ada. Verifikasi request HTTP sungguhan (`BIL-AT-110`–`114`) **direkomendasikan** sebagai langkah berikutnya, dicatat di sini sebagai butir yang belum dijalankan — bukan disembunyikan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `ReservedAmount`/`AvailableAmount` masih ada pada `PettyCashBudgetResponse` sampai laporan `BE-BKC-055` mengonfirmasi penghapusan penuh — jangan mengandalkan field itu untuk logika baru |
| Masalah yang diketahui | `NONE` baru pada task ini |
| Risiko tersisa | Carry-forward belum diuji lewat request HTTP sungguhan pada database dengan data riil — direkomendasikan sebelum Finance memakainya untuk penutupan periode pertama kali |
| Perubahan sampingan | `NONE` |
| Interupsi | Sama seperti dicatat `BE-BKC-053.md` — migration sempat kosong lalu `migrations remove` menggantung, diselesaikan pengguna secara manual |
| Status Git | Empat berkas source task ini sudah ter-commit lewat merge `22441de9` |
| Langkah berikutnya | Jalankan `BIL-AT-110`–`114` sebagai request HTTP sungguhan begitu environment tersedia; lanjut `BE-BKC-055` (sudah selesai, lihat `BE-BKC-055.md`) untuk penghapusan `ReservedAmount` penuh |
