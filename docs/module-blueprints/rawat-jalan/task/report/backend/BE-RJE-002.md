# Laporan Perubahan Backend — `BE-RJE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-002` |
| Judul | Kontrak adapter `BIL-INTEGRATION-1.3` |
| Slice | `MVP-0` — `EPIC RJE-01` Fondasi data dan kontrak |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-002` |
| Trace | `FR-RJE-003`; `RJ-E2E-DEC-001`, `005`, `016`, `018`; `02-backend-architecture.md` V2.7.3; `contracts/integration-contract.md` V2-2 |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-001` ✅ (28 September 2026) |
| Klasifikasi | `LIGHT` — 5 berkas service/DTO/konstanta, tanpa model, tanpa migration, tanpa endpoint baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `0f0f5a4b` (`sukmagp`), working tree belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — kelima acceptance criteria terbukti (bagian 6) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` (Billing, Operational) dan `ClinicalManagement` |
| Registry | `Bil` `ACTIVE`; `Cli` `ACTIVE / LEGACY` |
| Keberlakuan | Perubahan pada service dan DTO yang sudah patuh konvensi; tidak ada entity baru |
| QBE yang berlaku | `QBE-VAL-001` (validasi kontrak), `QBE-API-001` (bentuk respons lama dipertahankan), `QBE-DTO-001` (field DTO opsional, entity tidak diekspos) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-MOD-*` — tanpa model; `QBE-PERM-001` — tanpa endpoint baru |
| Wewenang | `RJ-E2E-DEC-018` — source dan runtime ke `QuilvianNewDevSukma`; tanpa migration (tidak diperlukan), commit, atau deployment |

---

## 1. Masalah yang diperbaiki

Billing hanya menerima obat berstatus `DISPENSED` (sudah diserahkan), sementara farmasi baru boleh
menyerahkan obat setelah lunas. Akibatnya resep Rawat Jalan terkunci. Billing juga tidak mengenal
jasa konsultasi, dan folio tidak dapat membedakan revisi pembatalan klinis dari revisi biasa karena
keduanya membawa jenis efek yang sama.

**Contoh:** resep Paracetamol Tn. A (samaran) tidak bisa masuk tagihan sebelum diserahkan, dan
tidak bisa diserahkan sebelum dibayar. Sesudah task ini, kontrak `1.3` menerima resep tahap 1
(`PRESCRIBED`), sehingga jembatan (`BE-RJE-008`) dapat menagihnya lebih dulu.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Kontrak Billing siap menerima obat tahap 1 dan jasa konsultasi, dan setiap efek folio tahu apakah ia pembatalan |
| Pelaku | Sistem (producer klinis, folio, adapter Billing) |
| Pemicu | Fakta klinis dikirim ke folio; item dikirim ke invoice |
| Aturan | `PRESCRIBED` dan `CONSULTATION` **hanya** sah pada kontrak `1.3`. Pemanggil kontrak `0.4`/`1.2` tetap terikat aturan lama, sehingga perilakunya tidak berubah diam-diam. Penanda pembatalan tidak ikut sidik jari request |
| Status | `PHARMACY`: tagih `PRESCRIBED`/`DISPENSED`, void normal dari `PRESCRIBED`, status batal `CANCELLED`. `CONSULTATION`: tagih `COMPLETED`, tanpa void normal |
| Jalur tidak normal | Kontrak tidak dikenal → `422` dengan daftar tiga versi. `CONSULTATION` dengan kontrak lama → `422` "SourceDomain belum didukung". Status konsultasi selain `COMPLETED` → `422` |
| Hasil akhir | Fondasi kontrak untuk `BE-RJE-003`, `007`, `008`, `009` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingChargeSourceAdapter.cs`, `BillingInvoiceService.cs` (urutan validasi `UpsertChargeAsync`,
pemanggil internal yang memakai kontrak `0.4`), `BillingInvoicesController.cs` (`FromSource`),
`BillingSourceContract.cs`, `BillingOperationalDtos.cs`, `BillingFolioService.cs`
(`NormalizeRequest`, `BuildFingerprint`, `CreateProcessingEffect`), `BillingFolioController.cs`
(endpoint internal), `ClinicalMilestoneFactProducer.cs` (`DispatchAsync`),
`PatientProcedureController.cs` (alur eksekusi dan pembatalan untuk validasi).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` | Konstanta `IntegrationContractVersion13`; kebijakan `PHARMACY` baru dan domain `CONSULTATION`; aturan keras "PHARMACY hanya `DISPENSED`" diganti aturan per versi kontrak; `CONSULTATION` hanya dengan `1.3`; validasi versi kontrak disatukan di `IsSupportedContractVersion` untuk jalur upsert dan void |
| `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs` | Konteks `Consultation` / efek `ConsultationCharge` masuk gerbang kontrak folio |
| `Areas/HealthServices/BillingManagement/Operational/DTOs/BillingOperationalDtos.cs` | `RecognizeBillingMilestoneRequest.IsClinicalCancellation` (opsional, bawaan `false`) |
| `Areas/HealthServices/BillingManagement/Operational/Services/BillingFolioService.cs` | Penanda dibawa lewat `NormalizeRequest` dan disimpan ke `BilProcessingEffect.IsClinicalCancellation`; **tidak** ikut `BuildFingerprint` |
| `Areas/HealthServices/ClinicalManagement/Services/ClinicalMilestoneFactProducer.cs` | Mengisi penanda dari `MilestoneKind == ClinicalCancellation` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif. `POST /folios/internal/milestones/recognize` menerima field opsional `isClinicalCancellation`. `POST /billing/invoices/from-source` menerima `BIL-INTEGRATION-1.3`; pesan versi tidak sah kini menyebut tiga versi |
| Database | Tanpa migration. Kolom `IsClinicalCancellation` sudah dibuat `BE-RJE-001` |
| Keamanan/Auth | `NOT APPLICABLE` — atribut hak akses tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/from-source` | Mencatat item dari sistem sumber; kini menerima kontrak `BIL-INTEGRATION-1.3` beserta `PHARMACY`/`PRESCRIBED` dan `CONSULTATION`/`COMPLETED` | `BillingInvoice : Create` (tidak berubah) |

#### Health Services / Billing Management / Billing Folio

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/internal/milestones/recognize` | Pengakuan milestone internal; field opsional `isClinicalCancellation` baru | `BillingMilestone : RecognizeInternal` (tidak berubah) |

Kode status tidak berubah: `422` untuk kontrak/status tidak sah pada invoice; `400` untuk isian
milestone tidak lengkap; `409` untuk revisi yang sama dengan isi berbeda.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false --no-incremental -o <scratchpad>/out-rje002` | `0 Error(s)`, `230 Warning(s)`, 2 menit 5 detik; **nol** warning dari berkas task | `PASS` | Sama dengan baseline `BE-RJE-001` |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Tanpa perubahan model |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 5 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R13 | 16/16 `PASS` (bagian 5.1), satu kegagalan skrip uji diperbaiki dan diulang | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi hasil build dijalankan dari scratchpad pada `http://localhost:5219` terhadap
**`QuilvianNewDevSukma`**, sesi `superadmin` dari login seed (kredensial dibaca dari konfigurasi
tanpa dicetak; cookie dikirim sebagai header). Skenario `from-source` memakai `CategoryId` fiktif
**dengan sengaja**: adapter memvalidasi paling awal, sehingga permintaan yang lolos adapter berhenti
di "Kategori tarif tidak ditemukan" **tanpa menulis apa pun**. Pesan itu membuktikan adapter sudah
menerima, dan jumlah invoice/item dibandingkan sebelum dan sesudah.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; tanpa cookie `401` | — | `PASS` |
| R1 | `PHARMACY`/`PRESCRIBED` kontrak `1.2` | `422` "Jumlah obat yang diserahkan belum final." | 1 | `PASS` |
| R2 | `PHARMACY`/`PRESCRIBED` kontrak `1.3` | Lolos adapter → `422` "Kategori tarif tidak ditemukan atau tidak aktif." | 2 | `PASS` |
| R3 | `CONSULTATION`/`COMPLETED` kontrak `1.3` | Lolos adapter → `422` kategori | 3 | `PASS` |
| R4 | `CONSULTATION`/`COMPLETED` kontrak `1.2` | `422` "SourceDomain belum didukung oleh kontrak charge Billing." | 3 | `PASS` |
| R5 | `CONSULTATION`/`ACCEPTED` kontrak `1.3` | `422` "Source belum mencapai status billable yang disetujui." | 3 | `PASS` |
| R6 | `PHARMACY`/`DISPENSED` kontrak `1.2` (perilaku lama) | Lolos adapter → `422` kategori | 5 | `PASS` |
| R7 | `PROCEDURE`/`PERFORMED` kontrak `0.4` (perilaku lama) | Lolos adapter → `422` kategori | 5 | `PASS` |
| R8 | Kontrak `BIL-INTEGRATION-9.9` | `422` "ContractVersion harus BIL-INTEGRATION-0.4, BIL-INTEGRATION-1.2, atau BIL-INTEGRATION-1.3." | — | `PASS` |
| R8b | Jumlah invoice/item sebelum → sesudah R1–R8 | `(0, 0)` → `(0, 0)` | — | `PASS` |
| R9 | Endpoint internal: `InternalTest` v1 lalu v2 `isClinicalCancellation = true` (quantity dan unit kosong, seperti pembatalan sungguhan) | Keduanya `200`; efek di DB: v1 `false`, v2 `true` | 4 | `PASS` |
| R10 | Kirim ulang v1 **tanpa** field penanda; kirim ulang v2 dengan penanda `false` | Keduanya `200` replay; efek di DB tidak berubah | 4 | `PASS` |
| R11 | `Consultation`/`ConsultationCharge` v1 lewat endpoint internal | `200`; efek `Consultation` tercatat | 3 | `PASS` |
| R11b | `Consultation` dengan efek `ProcedureCharge` | `400` "EffectType belum diizinkan untuk SourceContext tersebut." | 3 | `PASS` |
| R12 | Alur sungguhan tindakan: buat → approve → execute | Masing-masing `200`; `billingHandoff = Emitted`; fakta v1 `ChargeEligibility` `Dispatched` | 4 | `PASS` |
| R13 | Tindakan yang sama dibatalkan | `200`, `Emitted`; fakta v2 `ClinicalCancellation` `Dispatched`; **efek folio v2 `IsClinicalCancellation = true`**, v1 `false` | 4 | `PASS` |

**Kegagalan skrip uji yang diperbaiki.** Percobaan pertama R9 v2 mengirim `quantity` kosong dengan
`unit` terisi dan ditolak `400` "Quantity dan Unit wajib tersedia bersama-sama" — aturan lama folio
yang benar. Diulang dengan keduanya kosong, sesuai bentuk pembatalan sungguhan.

**Catatan AC 4.** Kartu menyebut fakta pembatalan **Lab**. Database dev pemilik tidak memiliki order
Lab sama sekali, sehingga jalur producer dibuktikan lewat pembatalan **tindakan** (R12–R13). Producer,
folio, dan penanda yang dilalui sama persis; perbedaannya hanya pemanggil producer.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

**Tidak dijalankan:** jalur `403` dengan aktor tanpa hak akses — atribut hak akses tidak berubah dan
akun lain tidak tersedia.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Kunjungan uji `e3338713-5814-418f-9b37-ab258111ea53` | Satu `BilFolio` baru berisi efek `InternalTest` v1/v2 (fakta `109870af-…`) dan `Consultation` v1 (fakta `a5182e31-…`), idempotency key berawalan `TEST-RJE002-` |
| Kunjungan `f91ab8da-3854-4022-9ebb-c69c2d901871`, konsultasi `c21d7825-…` | Satu tindakan uji `34b7819e-f9c8-44e3-a86b-39424c8b1ff4` berstatus dibatalkan (catatan `TEST-RJE002`), dua fakta klinis, satu folio dan dua efek |
| Invoice dan item invoice | Tidak berubah (`0` / `0`) |

**Dampak ke pengujian berikutnya:** efek uji di atas bernilai `InvoiceSyncStatus = 0`
(`NotApplicable`, bawaan kolom) karena jembatan belum ada. Saat `BE-RJE-003` dikerjakan, efek
*Procedure* pada kunjungan `f91ab8da-…` dapat dipakai sebagai data uji, atau diabaikan karena bukan
baris `Pending`. Data dibiarkan dan dicatat, sama dengan perlakuan data uji Bank Darah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `PHARMACY`/`PRESCRIBED` dengan kontrak `1.2` tetap ditolak "Jumlah obat yang diserahkan belum final." | Terpenuhi | R1 |
| 2. Dengan `1.3` diterima | Terpenuhi | R2 |
| 3. `CONSULTATION`/`COMPLETED` dengan `1.3` diterima; dengan status lain ditolak | Terpenuhi | R3, R5; ditambah R4 (kontrak lama ditolak), R11, R11b (gerbang folio) |
| 4. Fakta pembatalan menghasilkan efek folio `IsClinicalCancellation = true` (kontrak `1.0.2`) | Terpenuhi | R9, R10, R12, R13 |
| 5. Perilaku domain lain tidak berubah | Terpenuhi | R6, R7, R8b; pemanggil internal tetap kontrak `0.4` |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Log aplikasi: peringatan aturan keselamatan radiologi (fail-closed, perilaku yang sudah ada) dan HTTPS redirect port — tidak terkait task |
| Masalah yang diketahui | `IsOrderComplete` kini bernilai `false` untuk item `PHARMACY` `PRESCRIBED`. Method itu tidak dipakai kode mana pun selain adapter (audit `063d38b`), sehingga tidak menahan finalisasi |
| Risiko tersisa | Pemanggil lain yang kelak memakai kontrak `1.3` harus mengikuti aturan tahap 1; baru jembatan (`BE-RJE-003`/`008`) yang dirancang memakainya |
| Perubahan sampingan | `NONE` di repository. Data uji di database: bagian 5.2 |
| Interupsi | `NONE` |
| Status Git | 5 berkas source `M` dan dokumen blueprint (laporan ini, roadmap, traceability, decision log). Belum di-stage atau di-commit |
| Langkah berikutnya | `BE-RJE-003` (jembatan folio → invoice) — prasyaratnya kini terpenuhi dan menahan enam task lain |
