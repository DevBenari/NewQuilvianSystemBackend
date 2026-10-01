# Laporan Perubahan Backend — `BE-RJE-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-005` |
| Judul | Perubahan setelah invoice final |
| Slice | `MVP-1` — `EPIC RJE-02` Jembatan folio ke invoice |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-005` |
| Trace | `FR-RJE-015`; `RJ-BIL-DEC-004`; `RJ-E2E-DEC-010`, `022`; `02-backend-architecture.md` V2.7.5; `contracts/validation-matrix.md` V2 (`ADJUSTMENT_REJECTED`) |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-003` ✅ |
| Klasifikasi | `MEDIUM` — satu service diperluas dengan jalur keuangan pasca-final dan penghitungan kumulatif |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `def2d63e` (`sukmagp`), working tree belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — keempat acceptance criteria terbukti; satu cacat ditemukan runtime dan diperbaiki (bagian 5.1) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Registry | `Bil` `ACTIVE` |
| Keberlakuan | Perluasan service `NEW CODE` dari `BE-RJE-003` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001` (adjustment memakai transaksi dan lock milik `CreateAdjustmentAsync`), `QBE-AUD-001` (audit adjustment milik service yang ada + audit jembatan) |
| Tidak berlaku | Model, configuration, endpoint, permission — tidak disentuh |
| Wewenang | `RJ-E2E-DEC-022` — source dan runtime ke `QuilvianNewDevSukma`, termasuk mengubah status invoice uji |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-RJE-003`, pelayanan yang datang **setelah** tagihan final berhenti di antrean
`INVOICE_NOT_OPEN`, karena invoice final tidak boleh disunting. Padahal kejadian ini lumrah: hasil
Lab diterima setelah pasien membayar, atau jumlah tindakan dikoreksi.

**Contoh:** tagihan kunjungan sudah `FINAL`, lalu study Radiologi *Film Xray 8x10* (Rp198.000) baru
diterima. Sesudah task ini, sistem membuat adjustment `DEBIT` Rp198.000 berstatus `SUBMITTED` yang
menunggu persetujuan Billing. Item tagihan tidak disunting.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Perubahan pasca-final tetap tercatat, tetapi selalu melewati persetujuan manusia (`RJ-BIL-DEC-004`) |
| Pelaku | Sistem (jembatan); penyetuju Billing (menyetujui adjustment lewat alur yang sudah ada) |
| Pemicu | Efek folio diteruskan ketika invoice kunjungan berstatus `FINAL`, `CLOSED`, atau `SETTLED_BY_WRITE_OFF` |
| Langkah | 1. Jembatan menetapkan harga seperti biasa. 2. Bila adjustment dengan kunci (fakta, versi) yang sama sudah ada → dipakai, selesai. 3. Nilai efektif = total item aktif + adjustment dari **revisi sebelumnya** fakta yang sama. 4. Selisih = nilai baru − nilai efektif. 5. Selisih nol → `Synced` `NO_FINANCIAL_CHANGE`. 6. Selisih ≠ 0 → `CreateAdjustmentAsync` (`DEBIT` bila naik, `CREDIT` bila turun), kunci dan `CorrelationId` deterministik |
| Aturan | Item invoice final tidak pernah disunting. Satu (fakta, versi) paling banyak satu adjustment |
| Perubahan status | Efek `Pending` → `Synced` (`ADJUSTMENT_SUBMITTED` / `NO_FINANCIAL_CHANGE`) atau `ReconciliationRequired` (`ADJUSTMENT_REJECTED`) atau `Failed` (konflik sementara, dicoba ulang) |
| Jalur tidak normal | Invoice `CLOSED`/`SETTLED_BY_WRITE_OFF` → Billing menolak adjustment → antrean `ADJUSTMENT_REJECTED`. Invoice berubah bersamaan (RowVersion) → `Failed`, dicoba ulang dengan kunci sama |
| Hasil akhir | Tagihan final tetap utuh; perubahan menunggu persetujuan di daftar adjustment Billing |

**Contoh kumulatif (R3):** tindakan Rp1.246.000 tercatat qty 1 di invoice final. Revisi v2 menaikkan
qty menjadi 2 → `DEBIT` Rp1.246.000. Revisi v3 kembali ke qty 1 → nilai efektif = 1.246.000 +
1.246.000 = 2.492.000, nilai baru 1.246.000 → `CREDIT` Rp1.246.000. Totalnya kembali tepat.

### 2.1 Preflight wajib — perilaku `CreateAdjustmentAsync`

| Pertanyaan | Jawaban dari source | Dampak pada jembatan |
| --- | --- | --- |
| Menerima invoice `FINAL`? | Ya | Adjustment diajukan |
| Menerima `CLOSED` / `SETTLED_BY_WRITE_OFF`? | **Tidak** — "Invoice sudah closed atau settled by write-off dan tidak dapat menerima adjustment/write-off baru." (`EnsureLedgerMutableInvoice`) | `ReconciliationRequired` `ADJUSTMENT_REJECTED` — sesuai desain V2.7.5 |
| Idempotency | Replay dicocokkan lewat `IdempotencyKey` **dan** hash (`InvoiceId`, arah, nominal, alasan, `CorrelationId`, `CausationId`) | Seluruh isian dibuat deterministik; revisi yang sudah beradjustment dipakai ulang tanpa mengajukan (lihat cacat R4) |
| `CorrelationId` | Wajib unik di seluruh adjustment | Memakai kunci deterministik (fakta, versi) |
| `ExpectedInvoiceRowVersion` | Wajib cocok | Dibaca saat perencanaan; konflik → `Failed` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingFinancialExceptionService.cs` (`CreateAdjustmentAsync`, `EnsureLedgerMutableInvoice`,
`ValidateCreateAdjustmentRequest`, `ComputeAdjustmentPayloadHash`), `BillingAdjustmentDtos.cs`,
`BilAdjustment.cs`, `BillingClinicalChargeBridgeService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | Penolakan `INVOICE_NOT_OPEN` diganti jalur adjustment: `BuildAdjustmentPlanAsync` (pemakaian ulang adjustment berkunci sama, nilai efektif kumulatif dari revisi sebelumnya, selisih, `CreateAdjustmentRequest` deterministik), `AdjustAsync` (panggilan `CreateAdjustmentAsync` pada scope sendiri, pemetaan penolakan ke `ADJUSTMENT_REJECTED`, konflik ke `Failed`); `SyncPlan`/`SyncOutcome` membawa adjustment; `InvoiceAdjustmentId` dicatat pada efek; kode `ADJUSTMENT_SUBMITTED`, `ADJUSTMENT_REJECTED`, `NO_FINANCIAL_CHANGE` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tanpa endpoint baru |
| Database | Tanpa skema. Penulisan baru ke `BilAdjustment` (lewat service yang ada) dan `BilProcessingEffect.InvoiceAdjustmentId` |
| Keamanan/Auth | Tidak ada pengurangan/penambahan tagihan pasca-final tanpa persetujuan; `RequestedBy` = aktor klinis fakta, sehingga pemisahan pengaju–penyetuju milik Billing tetap berlaku |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Build pertama `… -o <scratchpad>/out-rje005` | `0 Error(s)`, `230 Warning(s)` | `PASS` | Dipakai R1–R5 |
| Build sesudah perbaikan `… --no-incremental -o <scratchpad>/out-rje005b` | `0 Error(s)`, `230 Warning(s)`, 2 menit 14 detik; nol warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R5 | Seluruhnya `PASS` setelah perbaikan (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**, sesi `superadmin`. Finalisasi
invoice lewat API menuntut alur pembayaran penuh, sehingga status invoice uji
`6810a4d2-605c-48f0-9c29-34b1a6cb80b1` diubah lewat SQL (`FINAL`, lalu `CLOSED` untuk R5). Sumber
Radiologi sintetis bertanda `TEST-RJE005`; revisi tindakan memakai fakta versi 2 dan 3 yang disisipkan
untuk tindakan nyata dari `BE-RJE-003`. Folio, jembatan, dan `CreateAdjustmentAsync` berjalan sungguhan.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Tagihan baru (Radiologi) pada invoice `FINAL` | Efek `Synced` `ADJUSTMENT_SUBMITTED`; adjustment `DEBIT` Rp198.000 `SUBMITTED` dengan alasan terbaca; item invoice tetap 4 | 1 | `PASS` |
| R2 | Revisi Radiologi v2 bernilai sama | Efek `Synced` `NO_FINANCIAL_CHANGE`; nol adjustment baru | 2 | `PASS` |
| R3 | Revisi tindakan v2 qty 2, lalu v3 qty 1 | v2 → `DEBIT` Rp1.246.000; v3 → `CREDIT` Rp1.246.000 (kumulatif benar) | 1 | `PASS` |
| R4 (run 1) | Efek v2 dikembalikan ke `Pending`, diproses ulang | Nol adjustment baru, **tetapi** efek jatuh ke `Failed` `TRANSIENT_FAILURE` | 4 | `NEW ERROR` → diperbaiki |
| R4 (run 2, sesudah perbaikan) | Efek v2 (yang `Failed`) dan v3 diproses ulang | Keduanya `Synced` `ADJUSTMENT_SUBMITTED` memakai adjustment yang ada; nol adjustment baru; total tetap 3 | 4 | `PASS` |
| R5 | Tagihan baru pada invoice `CLOSED` | Efek `ReconciliationRequired` `ADJUSTMENT_REJECTED`, pesan "Invoice sudah closed atau settled by write-off dan tidak dapat menerima adjustment/write-off baru." | 3 | `PASS` |

**Cacat yang ditemukan dan diperbaiki (R4).** Saat efek v2 diproses ulang, selisihnya dihitung ulang
dengan ikut memasukkan adjustment **v3** yang terjadi belakangan. Nominalnya berubah (Rp2.492.000),
kunci sama tetapi isi berbeda, sehingga Billing menolak sebagai konflik dan jembatan mencatatnya
sebagai gangguan sementara. Tidak ada adjustment ganda, tetapi efek akan dicoba ulang sampai habis
lalu masuk antrean dengan sebab yang salah. Perbaikan: (a) nilai efektif hanya memperhitungkan revisi
dengan versi **lebih kecil**; (b) adjustment berkunci sama yang sudah ada langsung dipakai. Efek v2
yang sempat `Failed` pulih menjadi `Synced` pada run 2 (jumlah percobaan 1 tercatat apa adanya).

**Keberlakuan R1–R3 dan R5 terhadap build terakhir.** Keempatnya dijalankan pada build sebelum
perbaikan. Perbaikan hanya berlaku ketika adjustment berkunci sama sudah ada (tidak ada pada R1–R3,
R5) atau ketika ada revisi lebih tinggi yang sudah beradjustment (tidak ada saat R1–R3 dijalankan).
Hasilnya tetap berlaku untuk build terakhir.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

**Tidak dijalankan:** persetujuan adjustment (`ApproveAdjustmentAsync`) — milik alur Billing yang sudah
ada dan di luar task; konflik RowVersion bersamaan (butuh dua penulis bersamaan, dibuktikan saat
`BE-RJE-010`).

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Invoice uji `6810a4d2-…` | Status **`CLOSED`** (diubah uji lewat SQL; semula `OPEN`), 4 item tidak berubah |
| Adjustment | 3 baris `SUBMITTED` pada invoice itu: `DEBIT` Rp198.000, `DEBIT` Rp1.246.000, `CREDIT` Rp1.246.000 |
| Sumber sintetis | 2 `RadOrder`/`RadStudy` baru `TEST-RJE005-*` dan faktanya; fakta tindakan versi 2 dan 3 |

**Dampak ke pengujian berikutnya:** invoice uji sudah `CLOSED`; skenario yang membutuhkan invoice `OPEN`
perlu kunjungan lain. Adjustment `SUBMITTED` dapat dipakai menguji layar persetujuan Billing.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tagih baru pada invoice `FINAL` → adjustment `DEBIT` `SUBMITTED` sebesar harga | Terpenuhi | R1, R3 |
| 2. Selisih nol → `Synced` `NO_FINANCIAL_CHANGE`, tanpa adjustment | Terpenuhi | R2 |
| 3. Penolakan service → `ReconciliationRequired` `ADJUSTMENT_REJECTED` | Terpenuhi | R5 |
| 4. Kirim ulang tidak membuat adjustment kedua | Terpenuhi (sesudah perbaikan) | R4 run 2 |
| Preflight `CreateAdjustmentAsync` tercatat | Terpenuhi | Bagian 2.1 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar log aplikasi yang sudah ada |
| Masalah yang diketahui | `CREDIT` pada obat tahap 2 (`BE-RJE-008`) dan pembatalan (`BE-RJE-009`) akan memakai jalur ini; cara uang dikembalikan tetap `RJ-E2E-OQ-003` |
| Risiko tersisa | Konflik isi yang benar-benar permanen tetap dicatat `Failed` dan baru masuk antrean setelah batas percobaan — dapat dibedakan lebih tajam saat `BE-RJE-010` |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | `NONE` |
| Status Git | 1 berkas source `M` dan dokumen blueprint. Belum di-stage atau di-commit |
| Langkah berikutnya | `BE-RJE-008` (obat dua tahap) dan `BE-RJE-009` (pembatalan) kini terbuka; juga `BE-RJE-007`, `010`, `014` |
