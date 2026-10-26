# Laporan Perubahan Backend — `BE-FIN-126`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-126` |
| Judul | Purchasing Invoice yang belum disetujui dapat dibatalkan dengan jejak pembatal dan waktu, tanpa menyentuh utang |
| Slice | `REV-22` — Finance AP mengikuti BLM, `Slice B` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22" |
| Trace | `AP-UAT-19`; `FIN-DEC-045`; `state-transition-matrix.md` §B.4 |
| Contract version | `state-transition-matrix.md` §B.4 (approved) sudah mengizinkan `DRAFT`/`PENDING_APPROVAL` → `CANCELLED`. `api-contract.md` §B.4 dan `permission-audit-matrix.md` belum mendaftarkan endpoint dan aksi `Cancel`; dicatat sebagai delta pada roadmap `REV-22` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 5: repository 0; berkas diperiksa 0; berkas diubah 0 (2 source); logika bisnis 1; kontrak API 2 (endpoint di luar kontrak terdaftar); database 1 (persistence existing); keamanan/auth 1 (aksi akses baru); UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`: `Areas/Corporate/FinanceManagement/Purchasing/`, `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `48011b09` (branch `Yasmina`), dengan perubahan `BE-FIN-120`..`125` dan pekerjaan Billing milik pengguna yang belum di-commit |
| Tanggal | 2026-10-26 |
| Status | 🟡 **SEBAGIAN** — seluruh kriteria terpetakan ke source; `dotnet build` **NOT RUN** |

---

## 1. Masalah yang diperbaiki

Status `CANCELLED` sudah ada pada Purchasing Invoice (konstanta dan check constraint), dan matriks transisi mengizinkannya dari `DRAFT` dan `PENDING_APPROVAL`, tetapi tidak ada
endpoint untuk mencapainya. Invoice yang salah susun hanya bisa ditolak (`REJECTED`, oleh penyetuju) atau dibiarkan menggantung di daftar. Petugas yang menyadari kesalahannya
sebelum disetujui tidak punya jalur sah untuk menarik invoice itu.

---

## 2. Proses bisnis

**Pelaku:** pengguna dengan hak `Cancel` pada Purchasing Invoice.

**Alur:**

1. Pengguna memanggil pembatalan atas invoice berstatus `DRAFT` atau `PENDING_APPROVAL`, dengan versi data terakhir (`ExpectedRowVersion`).
2. Sistem memeriksa versi, lalu status. Invoice menjadi `CANCELLED`; pembatal dan waktu pembatalan tercatat, dan audit aktor ditulis.
3. Tidak ada utang dan tidak ada kejadian Accounting yang dibuat atau diubah, karena memang belum ada. Tukar Faktur sumber tetap `RECEIVED`.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Invoice `APPROVED` (utang sudah terbentuk) | Ditolak `422`; koreksi lewat retur atau penyesuaian utang |
| Invoice `REJECTED` atau sudah `CANCELLED` | Ditolak `422` |
| `ExpectedRowVersion` usang | Ditolak `409` |
| Pengguna tanpa hak `Cancel` | Ditolak `403` |
| Invoice tidak ditemukan | `404` |

**Catatan penting:** satu Tukar Faktur hanya boleh punya satu invoice (indeks unik). Setelah invoice dibatalkan, Tukar Faktur tetap terikat pada invoice itu. Untuk menyusun invoice baru,
Tukar Faktur dibatalkan lebih dulu (masih diperbolehkan karena berstatus `RECEIVED`) lalu dicatat ulang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Purchasing/Services/FinancePurchasingInvoiceService.cs` dan `Controllers/FinancePurchasingInvoicesController.cs`
- `Purchasing/Models/FinPurchasingInvoice.cs` (konstanta status) dan `Repositories/Configurations/.../FinPurchasingInvoiceConfiguration.cs` (check constraint status)
- Pola `CancelAsync` pada `FinanceInvoiceExchangeService` dan `FinanceSupplierReturnService`
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §B.4

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Purchasing/Services/FinancePurchasingInvoiceService.cs` | `CancelAsync` baru |
| `Purchasing/Controllers/FinancePurchasingInvoicesController.cs` | Aksi `POST /{id}/cancel` dengan `[AccessAction("Cancel", ...)]` dan `[AccessPermission("FinancePurchasingInvoice", "Cancel")]`; komentar header yang menyatakan cancel "tidak dibangun" diperbarui |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | `BE-FIN-126` pada amendment `REV-22` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru di luar daftar kontrak (transisinya sudah ada di matriks status). Tidak ada endpoint existing yang berubah |
| Database | `NOT APPLICABLE` — nol migration; status `CANCELLED` sudah diizinkan check constraint `CK_FinPurchasingInvoice_Status` |
| Keamanan/Auth | Aksi akses baru `FinancePurchasingInvoice : Cancel` (tipe `Update`). Muncul otomatis di layar Akses Role karena registry diturunkan dari `[AccessPermission]`; **awalnya tidak diberikan kepada siapa pun selain SuperAdmin** sampai admin mencentangnya. Tidak ada pemeriksaan nama role di kode |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing / Purchasing Invoice

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/finance-management/purchasing/purchasing-invoices/{id}/cancel` | Membatalkan invoice `DRAFT` atau `PENDING_APPROVAL`; body `{ "expectedRowVersion": "<guid>" }` | `FinancePurchasingInvoice : Cancel` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: build dijalankan mandiri |
| QBE preflight | `TOUCHED LEGACY`; `QBE-API-001`, `QBE-PERM-001`, `QBE-DEL-001`, `QBE-LOG-001` terpenuhi | `PASS` | Penelusuran source |
| Pencocokan `[AccessAction]` dan `[AccessPermission]` | Nama aksi `Cancel` sama di kedua atribut; `AccessType.Update` | `PASS` | `FinancePurchasingInvoicesController.cs` |
| Registry hak akses | Diturunkan dari `[AccessPermission]` (Phase A0), sehingga `Cancel` muncul di layar Akses Role | `PASS` | `PermissionRegistryDescriptor.cs` |
| Penelusuran jalur normal dan tidak normal | Urutan: versi, status, simpan, audit; semua status selain `DRAFT`/`PENDING_APPROVAL` ditolak | `PASS` | `CancelAsync` |
| Pemetaan exception | `KeyNotFound` 404, `PurchasingConflict` 409, `PurchasingValidation` 422 sudah ditangani `Failure` | `PASS` | Controller |

`AUTOMATED TEST: NOT APPLICABLE — task ini tidak disertai test otomatis; persetujuan pemilik atas test hanya mencakup BE-FIN-120 dan BE-FIN-121 (rules/backend/TEST_POLICY.md).`

Uji manual: `NOT FEASIBLE` — server backend tidak dijalankan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `DRAFT` dibatalkan: `CANCELLED`, pembatal dan waktu tercatat | Terpetakan ke source; belum dibuktikan build | `CancelAsync` |
| 2. `PENDING_APPROVAL` dibatalkan: sama | Terpetakan ke source; belum dibuktikan build | `CancelAsync` |
| 3. `APPROVED`, `REJECTED`, `CANCELLED` ditolak `422` | Terpetakan ke source; belum dibuktikan build | Pemeriksaan status |
| 4. Versi usang ditolak `409` | Terpetakan ke source; belum dibuktikan build | `EnsureCurrent`, `DbUpdateConcurrencyException` |
| 5. Tukar Faktur tetap `RECEIVED`; tanpa utang dan tanpa kejadian Accounting | Terpetakan ke source; belum dibuktikan build | `CancelAsync` tidak menyentuh keduanya |
| 6. Tanpa hak `Cancel` ditolak `403` | Terpetakan ke source; belum dibuktikan build | `[AccessPermission]` |
| DoD: preflight selesai | Terpenuhi | Metadata dan bagian 5 |
| DoD: `dotnet build` berhasil | **Belum terpenuhi** | `NOT RUN` |
| DoD: laporan tracked ada | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pembatalan tidak dibatasi pada pengaju; siapa pun yang diberi hak `Cancel` dapat membatalkan. Sama dengan pembatalan Tukar Faktur dan Retur |
| Masalah yang diketahui | Tukar Faktur tetap terikat pada invoice yang dibatalkan (indeks unik), sehingga penyusunan ulang membutuhkan pembatalan dan pencatatan ulang Tukar Faktur. Mengubahnya menyentuh indeks unik dan membutuhkan keputusan |
| Risiko tersisa | Rendah. Build belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `git status --short`; berkas task ini tercantum pada bagian 3.2 |
| Langkah berikutnya | Pemilik menjalankan `dotnet build`; admin mencentang hak `Cancel` pada layar Akses Role bagi peran yang berwenang; lalu `BE-FIN-126` dinaikkan ke ✅ |
