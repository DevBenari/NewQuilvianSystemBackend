# Laporan Perubahan Backend — `BE-RJE-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-004` |
| Judul | Pengaman `from-source` |
| Slice | `MVP-1` — `EPIC RJE-02` Jembatan folio ke invoice |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-004` |
| Trace | `FR-RJE-014`; `RJ-E2E-DEC-006`, `020`; `02-backend-architecture.md` V2.7.6; `contracts/validation-matrix.md` V2 (`RJE-VAL-010`); `contracts/api-contract.md` V2 |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | — (gelombang 1) |
| Klasifikasi | `LIGHT` — 1 method service baru, 1 pengecualian baru, 1 action controller disentuh |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `dfb696b5` (`sukmagp`), working tree belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — ketiga acceptance criteria terbukti (bagian 6) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Registry | `Bil` `ACTIVE` |
| Keberlakuan | Perluasan service dan controller yang sudah ada |
| QBE yang berlaku | `QBE-SVC-001` (pemeriksaan di service; controller tidak menyentuh `DbContext`), `QBE-VAL-001`, `QBE-API-001` (bentuk `ApiResponse` dan kode status yang sudah ada) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-MOD-*`; `QBE-PERM-001` — atribut hak akses tidak berubah |
| Wewenang | `RJ-E2E-DEC-020` — source dan runtime ke `QuilvianNewDevSukma` |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-RJE-003`, pelayanan Rawat Jalan sudah masuk invoice otomatis dengan harga katalog. Tetapi
endpoint `POST /billing/invoices/from-source` masih menerima harga dari pemanggil. Siapa pun yang
memegang `BillingInvoice : Create` dapat mencatat tindakan Rawat Jalan seharga Rp1 lewat API.

**Contoh:** sebelum task ini, `from-source` dengan `PROCEDURE`, `UnitPrice = 1` untuk kunjungan Rawat
Jalan diterima dan membentuk item. Sesudahnya, permintaan itu ditolak `422 RJE-VAL-010`.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Pelayanan klinis Rawat Jalan hanya dapat masuk invoice lewat jembatan berharga katalog |
| Pelaku | Pemanggil `from-source` (kasir lewat layar biaya lain-lain, integrasi internal) |
| Pemicu | `POST /billing/invoices/from-source` |
| Aturan | Ditolak bila `SourceDomain` (tanpa membedakan huruf besar/kecil) salah satu dari `PROCEDURE`, `LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSULTATION` **dan** kunjungan bertipe Rawat Jalan. Domain lain dan kunjungan lain tidak berubah |
| Jalur tidak normal | Kunjungan tidak ditemukan → tidak ditolak di pengaman; diteruskan dan dijawab `404` oleh logika yang sudah ada |
| Hasil akhir | Satu-satunya jalur tagihan klinis Rawat Jalan adalah jembatan (`UpsertChargeAsync` dipanggil langsung oleh jembatan, tidak melewati pengaman) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingInvoicesController.cs` (`FromSource`), `BillingInvoiceService.cs` (`UpsertChargeAsync`,
kelas pengecualian), `BillingClinicalChargeBridgeService.cs` (pemanggil `UpsertChargeAsync`),
`Responses/ApiResponse.cs`, konsumen frontend `billing-invoice-slice.jsx:607` (domain `ADHOC`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs` | Method `UpsertManualChargeAsync` (pengaman lalu `UpsertChargeAsync`); pengecualian `BillingManualClinicalSourceException` berkode `RJE-VAL-010`. `UpsertChargeAsync` **tidak diubah** |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs` | `FromSource` memanggil `UpsertManualChargeAsync` dan memetakan pengecualian baru ke `422` dengan `errors.code = "RJE-VAL-010"` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /from-source` menambah satu jawaban `422` (`RJE-VAL-010`) untuk domain klinis Rawat Jalan — sesuai kontrak V2. Bentuk sukses tidak berubah |
| Database | `NOT APPLICABLE` — tanpa perubahan skema |
| Keamanan/Auth | Menutup jalur harga bebas untuk pelayanan klinis Rawat Jalan. Atribut hak akses tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/from-source` | Mencatat item dari sistem sumber; kini menolak domain klinis untuk kunjungan Rawat Jalan | `BillingInvoice : Create` |

Kode status: `200` item tercatat; `404` kunjungan tidak ditemukan; `409` isi berbeda untuk kunci/versi
yang sama; `422` isian tidak sah, **termasuk** `RJE-VAL-010` "Pelayanan Rawat Jalan ditagihkan otomatis
dari pelayanan klinis dan tidak dapat dicatat lewat jalur ini."

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false --no-incremental -o <scratchpad>/out-rje004` | `0 Error(s)`, `230 Warning(s)`, 2 menit 16 detik; nol warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R4 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi hasil build pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**, sesi `superadmin`
dari login seed. Jumlah seluruh item invoice dihitung sebelum dan sesudah setiap permintaan.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | `PROCEDURE` Rp1, kunjungan Rawat Jalan, kategori nyata | `422` pesan `RJE-VAL-010`, `errors.code = "RJE-VAL-010"`; item total 4 → 4 | 1 | `PASS` |
| R1b–R1e | `LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSULTATION`, kunjungan Rawat Jalan | Masing-masing `422 RJE-VAL-010`; item total tetap 4 | 1 | `PASS` |
| R1f | `procedure` (huruf kecil) | `422 RJE-VAL-010` | 1 | `PASS` |
| R2 | `ADHOC` Rp15.000, kunjungan Rawat Jalan, kategori nyata | `200` "Charge berhasil dicatat"; item total 4 → 5; item `ADHOC` Rp15.000 tersimpan | 2 | `PASS` |
| R3 | `EMERGENCY` pada kunjungan IGD (kategori fiktif, berhenti sesudah pengaman) | `422` "Kategori tarif tidak ditemukan" — lolos pengaman | 3 | `PASS` |
| R3b | `PROCEDURE` pada kunjungan **IGD** (kategori fiktif) | `422` kategori — lolos pengaman karena bukan Rawat Jalan | 3 | `PASS` |
| R3c | `ROOM_STAY` pada kunjungan Rawat Jalan (kategori fiktif) | `422` kategori — lolos pengaman | 3 | `PASS` |
| R4 | Jembatan tidak terhalang: tindakan dieksekusi lewat API | Efek `Synced`; item `PROCEDURE` Rp3.848.000 = tarif katalog | 1 | `PASS` |

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

**Tidak dijalankan:** `403` dengan aktor tanpa `BillingInvoice : Create` — atribut tidak berubah dan akun
lain tidak tersedia.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Invoice `6810a4d2-…` (kunjungan uji) | Bertambah satu item `ADHOC` Rp15.000 (deskripsi `TEST-RJE004`) |
| Tindakan uji | Satu tindakan dikerjakan (catatan `TEST-RJE003`, dibuat skrip bersama) dengan invoice baru `8638ba4f-…` berisi satu item `PROCEDURE` |
| Permintaan yang ditolak | Tidak meninggalkan data apa pun |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `PROCEDURE` Rp1 untuk kunjungan Rawat Jalan → `422 RJE-VAL-010`, nol item baru | Terpenuhi | R1, R1b–R1f |
| 2. `ADHOC` tetap diterima | Terpenuhi | R2 |
| 3. `EMERGENCY` dan `ROOM_STAY` tidak berubah | Terpenuhi | R3, R3c; ditambah R3b (domain klinis pada kunjungan non-Rawat Jalan) |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar peringatan log aplikasi yang sudah ada |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Integrasi internal lain yang kelak ingin menagih domain klinis Rawat Jalan harus lewat jembatan, bukan `from-source` — memang disengaja |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | `NONE` |
| Status Git | 2 berkas source `M` dan dokumen blueprint. Belum di-stage atau di-commit |
| Langkah berikutnya | `BE-RJE-006` (gelombang 1), lalu gelombang 4: `BE-RJE-005`, `007`, `010`, `014` |
