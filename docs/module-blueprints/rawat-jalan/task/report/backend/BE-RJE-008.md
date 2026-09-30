# Laporan Perubahan Backend — `BE-RJE-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-008` |
| Judul | Obat dua tahap |
| Slice | `MVP-2` — `EPIC RJE-04` Obat dua tahap |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-008` |
| Trace | `FR-RJE-030`, `031`, `032`; `AC-RJ-007`; `RJ-E2E-DEC-005`, `006`, `025`; `02-backend-architecture.md` V2.7.2–V2.7.5 |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-005` ✅ |
| Klasifikasi | `HIGH` — menyentuh gerbang finalisasi invoice, penyerahan obat (stok), dan dua modul (Billing, Pharmacy) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `eeb18c57` (`sukmagp`), working tree belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — kelima acceptance criteria terbukti lewat alur utuh dokter → kasir → farmasi. Ada dua koreksi audit dan satu perbaikan bug Farmasi (bagian 1) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing`; titik sentuh `PharmacyManagement` (finalisasi resep, penyerahan obat, pembuatan resep) |
| Registry | `Bil` `ACTIVE`; `Phm` `ACTIVE / LEGACY` |
| Keberlakuan | Perluasan service `NEW CODE` (`BE-RJE-003`); `TOUCHED LEGACY` pada `PrescriptionDispensingService`, `PrescriptionController`, `BillingChargeSourceAdapter` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001` (fakta tahap 2 diterbitkan **setelah** penyerahan tersimpan), `QBE-AUD-001` |
| Tidak berlaku | Model, configuration, migration, endpoint baru, permission — tidak disentuh |
| Wewenang | `RJ-E2E-DEC-025` — source dan runtime ke `QuilvianNewDevSukma`, termasuk data uji (resep, stok, shift kasir, status invoice uji) |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, obat Rawat Jalan terkunci melingkar. Farmasi baru menyerahkan obat setelah
tagihannya lunas, sedangkan Billing hanya mau menagih obat yang sudah diserahkan.

**Contoh setelah task ini:** dr. X meresepkan Panadol sirup × 4 (Rp64.504) dan Acetin × 4
(Rp142.080).

1. Begitu konsultasi selesai, invoice memuat satu item resep `PRESCRIBED` Rp826.336.
2. Pasien membayar di kasir. Invoice menjadi final dan tertutup, lalu clearance farmasi terbit.
3. Farmasi menelaah, menyiapkan, dan menyerahkan obat.
4. Acetin hanya diserahkan 3, jadi tagihan menyesuaikan menjadi Rp684.256.

**Tiga hal di source ternyata berbeda dari asumsi blueprint.** Ketiganya diputuskan di sini karena
aturan targetnya sudah disetujui; tidak ada kebijakan baru yang dikarang.

| No | Temuan | Akibat bila dibiarkan | Tindakan |
| ---: | --- | --- | --- |
| 1 | Audit V2.7.3 menyatakan `IsOrderComplete` tidak dipakai kode lain. Ternyata `BillingFinalizationService.AreAllOrdersCompleteAsync` memakainya, dan `PRESCRIBED` terbaca sebagai order belum selesai | Invoice berisi resep tahap 1 tidak pernah bisa difinalkan, dan clearance farmasi baru terbit setelah invoice final dan lunas. Deadlock tetap ada | Adapter menganggap `PHARMACY`/`PRESCRIBED` sudah lengkap untuk finalisasi. `PRESCRIBED` tetap boleh di-void normal. Ini menegakkan invariant V2.7.3 yang sudah disetujui: "item `PRESCRIBED` tidak menahan finalisasi" |
| 2 | `POST /prescriptions` membuat resep draft dengan `FulfillmentStatus = WaitingForPayment` sejak commit `f194f978`. Model, validator finalisasi, `PrescriptionWorkflowService`, dan rekonsiliasi obat semuanya memakai `WaitingForClinicalFinalization` | Setiap konsultasi yang memuat resep baru ditolak dengan `INVALID_PRESCRIPTION_FULFILLMENT_STATUS` dan tidak pernah bisa diselesaikan. Ini jalur yang dipakai layar dokter | Status awal diperbaiki menjadi `WaitingForClinicalFinalization`. Resep lama yang sudah terlanjur salah di data dev **tidak** dimigrasi (bagian 7) |
| 3 | Pelunasan penuh langsung memfinalkan invoice sekaligus menutupnya (`CLOSED`). Clearance farmasi hanya terbit pada `CLOSED`, dan invoice `CLOSED` menolak adjustment | Dalam alur normal, invoice pasti sudah `CLOSED` ketika obat diserahkan. Penyerahan sebagian **tidak pernah** otomatis menjadi `CREDIT`, dan selalu berakhir di antrean `ADJUSTMENT_REJECTED` | Mengikuti jalur cadangan yang sudah tertulis di V2.7.5. Tidak diubah di task ini; lihat risiko di bagian 7 |

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Obat ditagih saat diresepkan supaya bisa dilunasi, lalu tagihannya menyesuaikan jumlah yang benar-benar diserahkan |
| Pelaku | Dokter (menyelesaikan konsultasi), kasir (menerima pembayaran), farmasi (telaah, penyiapan, cek akhir, serah) |
| Tahap 1 | Konsultasi selesai → fakta resep `milestone = "ClinicalFinalization"` → satu item `PHARMACY` per resep, `PRESCRIBED`, harga Σ(jumlah diresepkan × tarif) untuk item yang tidak dihentikan |
| Tahap 2 | Farmasi menyerahkan obat → revisi fakta yang sama `milestone = "Dispensed"` beserta jumlah kumulatif yang diserahkan per item → item menjadi `DISPENSED` dengan harga Σ(jumlah diserahkan × tarif) |
| Tarif per obat | (1) `PhmPrescriptionItem.TariffId` bila masih berlaku; (2) `MstTariff.DrugId` = obat, klinik dan kelas pasien paling spesifik |
| Keadaan invoice di tahap 2 | `OPEN` → item diperbarui. `FINAL` → adjustment `CREDIT`/`DEBIT` sebesar selisih, menunggu persetujuan. `CLOSED` → ditolak Billing, masuk antrean `ADJUSTMENT_REJECTED` |
| Jalur tidak normal | Satu obat tanpa tarif → **seluruh** resep ke antrean `TARIFF_NOT_FOUND`, tidak ada tagihan sebagian. Resep berisi racikan → antrean `SOURCE_REJECTED` (lihat bagian 7). Serah ditekan ulang → tidak ada fakta baru. Resep dari jalur lama yang tidak pernah punya fakta tahap 1 → tahap 2 tidak menerbitkan apa pun |
| Hasil akhir | Item resep yang nilainya sama dengan obat yang diserahkan, atau adjustment/antrean dengan sebab terbaca |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PrescriptionDispensingService.cs`, `PrescriptionWorkflowService.cs`, `PrescriptionValidationService.cs`,
`PrescriptionFinancialClearanceService.cs`, `PrescriptionController.cs`, model
`PhmPrescription`/`Item`/`Compound`/`CompoundItem`, `ClinicalMilestoneFactProducer.cs` (penentuan versi
dan fingerprint), `BillingInvoiceService.UpsertChargeAsync`, `BillingChargeSourceAdapter.cs`,
`BillingFinalizationService.cs`, `BillingInvoiceClosureService.cs`, `BillingSettlementService.cs`,
`BilConsumerHandoffService.PublishForClearanceChangeAsync`, `BillingFinancialExceptionService.EnsureLedgerMutableInvoice`,
`CashierShiftService.cs`, serta controller telaah, penyiapan, cek akhir, penyerahan, dan stok farmasi.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | `ResolvePharmacyAsync` dan `FindDrugTariffAsync`; request membawa `DispensedQuantities` opsional; `Resolved` menerima `TariffId` kosong (satu item per resep tidak menunjuk satu tarif) |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | `PHARMACY` masuk gerbang domain; identitas = ID resep (sama dengan yang dibaca clearance); `TryReadPrescriptionStage` membaca `RuleSnapshot` → `PRESCRIBED`/`DISPENSED` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` | `IsOrderComplete`: `PHARMACY`/`PRESCRIBED` dianggap lengkap untuk finalisasi (temuan 1) |
| `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs` | Konstanta `PrescriptionMilestoneClinicalFinalization` dan `PrescriptionMilestoneDispensed` |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` | Fakta resep tahap 1 membawa `RuleSnapshot.milestone = "ClinicalFinalization"` |
| `Areas/HealthServices/PharmacyManagement/Services/PrescriptionDispensingService.cs` | `EmitDispensedChargeAsync` setelah penyerahan tersimpan: hanya merevisi fakta tahap 1 yang ada; `RuleSnapshot` hanya berisi id item dan jumlah (`SEC-RJ-005`); kegagalannya tidak membatalkan penyerahan |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` | Status awal resep draft `WaitingForClinicalFinalization` (temuan 2) |
| `docs/…/00-interview-decisions.md` | `RJ-E2E-DEC-025` (wewenang `BE-RJE-008`, `009`, `010`, `014`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request/response tidak berubah. **Perilaku berubah:** `POST /prescriptions` kini mengembalikan resep dengan `fulfillmentStatus = 1` (sebelumnya `2`) |
| Database | Tanpa skema. Revisi fakta `Prescription` pada `CliClinicalMilestoneFact` (versi 2, dst.) |
| Keamanan/Auth | Tidak ada endpoint atau permission baru. Fakta tanpa instruksi obat maupun harga |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah bentuk. Perubahan nilai awal
`fulfillmentStatus` pada `POST /api/v1/health-services/pharmacy-management/prescriptions` tercatat di
bagian 3.3.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje008c` | `0 Error(s)`, `230 Warning(s)`, 1 menit 48 detik; nol warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Findings: none`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R9 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**, sesi `superadmin`. Seluruh
langkah memakai endpoint sungguhan: resep dan item resep dokter, diagnosis, finalisasi konsultasi,
hitung ulang invoice, settlement dan tender tunai, baca resep oleh farmasi, telaah, penyiapan, cek
akhir, saldo stok pembuka, penyiapan dan penyerahan obat.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Resep A (Panadol × 4, Acetin × 4) — konsultasi diselesaikan | `200`; fakta v1 `{"milestone":"ClinicalFinalization"}`; item `PHARMACY` `PRESCRIBED` qty 1 **Rp826.336** = 4 × 64.504 + 4 × 142.080; invoice `OPEN` | 1 | `PASS` |
| R2 | Farmasi mencoba menelaah resep A sebelum lunas | `400` "Status resep belum siap untuk proses telaah farmasi."; telaah, penyiapan, dan cek akhir tertolak berurutan | 3 | `PASS` |
| R3 | Kasir: pratinjau final resep A sebelum bayar | `allOrdersComplete = true` walau item `PRESCRIBED` (temuan 1 terbukti tertutup); satu-satunya penghalang "belum lunas" | 2 | `PASS` |
| R4 | Kasir membayar tunai Rp826.336 → farmasi membaca resep → telaah → penyiapan → cek akhir | Tender `201`; invoice `CLOSED`; clearance `CLEARED`/`PAID`/`INVOICE_SETTLED`; resep `QueuedAtPharmacy`, `Paid`; telaah `200`, penyiapan `200`, cek akhir "Resep siap diserahkan" | 2 | `PASS` |
| R5 | A — invoice disiapkan `OPEN` (SQL); serah Panadol 4 + Acetin 3 | Resep `PartiallyDispensed`; fakta v2 `Dispensed` dengan jumlah per item; item **yang sama** naik ke versi 2 `DISPENSED` **Rp684.256**; tanpa adjustment | 4 (`OPEN`) | `PASS` |
| R6 | A — penyerahan yang sama dikirim ulang | `200` tanpa potong stok; fakta tetap 2 | — | `PASS` |
| R7 | E (Panadol × 1, Acetin × 2) — lunas, invoice disiapkan `FINAL` (SQL); serah Panadol 1 + Acetin 1 | Adjustment **`CREDIT` Rp142.080 `SUBMITTED`** ("nilai 348664 menjadi 206584"); item tetap `PRESCRIBED`; efek `Synced`/`ADJUSTMENT_SUBMITTED` | 4 (`FINAL`) | `PASS` |
| R8 | B (Amoxsan × 5) — lunas, invoice `CLOSED` apa adanya; serah 4 | Efek v2 `ReconciliationRequired` `ADJUSTMENT_REJECTED` ("Invoice sudah closed …"); item tetap Rp164.100; tanpa adjustment (temuan 3) | — | `PASS` |
| R9 | C (Panadol × 2, Acetin × 2) — seluruh tarif Acetin dinonaktifkan sementara, lalu konsultasi diselesaikan | Konsultasi `200`; efek `ReconciliationRequired` `TARIFF_NOT_FOUND` "Tarif obat ACETIN … Seluruh resep ditahan"; **nol** item — Panadol tidak ditagih sendiri. 19 tarif dipulihkan | 5 | `PASS` |
| R10 | D — resep lama berisi racikan | `SOURCE_REJECTED` "Resep memuat racikan yang belum dapat ditagih otomatis"; nol item | — | `PASS` |
| R11 | Resep baru E dibuat lewat `POST /prescriptions` setelah perbaikan | Status awal `(Draft, WaitingForClinicalFinalization, NotBilled)`; konsultasi E selesai `200` | — | `PASS` |

**Percobaan yang tidak dipakai sebagai bukti.**
- Run pertama ditolak `INVALID_PRESCRIPTION_FULFILLMENT_STATUS` untuk resep lama maupun resep baru
  yang dibuat lewat API. Dari situ temuan 2 ditemukan.
- Pembayaran pertama ditolak "Buka shift kasir sebelum menerima uang tunai"; shift uji dibuka lewat
  `POST /cashier/shifts/open`.
- Settlement kedua dijawab `409` karena settlement draft dari percobaan pertama masih aktif; skrip
  memakai settlement itu.
- Pemanggilan `POST /finalizations/invoices/{id}` sesudah bayar dijawab `409`, karena pelunasan
  sudah memfinalkan dan menutup invoice (temuan 3).

**Kondisi yang disiapkan lewat SQL, dan alasannya.**
- R5: `OPEN`. Dalam alur normal, farmasi baru boleh menyerahkan obat setelah invoice `CLOSED`,
  jadi keadaan `OPEN` saat penyerahan tidak dapat dicapai lewat API.
- R7: `FINAL`. Sama sebabnya dengan R5.
- R9: tarif Acetin dinonaktifkan. Pembuatan item resep sendiri menolak obat tanpa tarif, jadi
  kondisi "tarif berakhir antara resep ditulis dan konsultasi diselesaikan" disimulasikan.
- R10 dan resep A/B/C: status resep lama yang salah dibetulkan ke
  `WaitingForClinicalFinalization`.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Konsultasi | 5 selesai (A `8e5dc761…`, B `7185ab1c…`, C `2fc79214…`, D `283af202…`, E `5a155b03…`) dengan diagnosis `TEST-RJE008` |
| Resep | A `26b76ee9…`, B `40f106bf…`, E `be856c51…` `PartiallyDispensed`; C `bb641c80…` dan D `aeabc117…` `WaitingForPayment` dengan efek di antrean |
| Invoice | A `CLOSED` (dikembalikan setelah R5; item kini Rp684.256 sementara pembayaran Rp826.336 — ketidakseimbangan buatan uji); B `CLOSED`; E **`FINAL`** dengan adjustment `CREDIT` `SUBMITTED` |
| Stok | Saldo pembuka `TEST-RJE008-*` di depo `test`: Panadol, Acetin, Amoxsan masing-masing 10 dikurangi yang diserahkan |
| Kasir | Satu shift kasir superadmin **masih terbuka** di register `6fae18a6…` |
| Master | 19 tarif Acetin kembali aktif |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Resep difinalkan → item `PRESCRIBED` sebesar Σ jumlah × tarif | Terpenuhi | R1 |
| 2. Invoice dilunasi → clearance `CLEARED`; telaah dan serah berjalan (`UAT-07`) | Terpenuhi | R3, R4, R5 |
| 3. Belum lunas → farmasi ditolak (`UAT-09`) | Terpenuhi | R2 |
| 4. Serah sebagian pada invoice `FINAL` → adjustment `CREDIT`; pada `OPEN` → item `DISPENSED` lebih kecil (`UAT-08`) | Terpenuhi, dengan keadaan invoice disiapkan lewat SQL | R5, R7; keadaan nyata `CLOSED` di R8 |
| 5. Satu obat tanpa tarif → seluruh resep di antrean | Terpenuhi | R9 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar log aplikasi yang sudah ada |
| Risiko tersisa — `CLOSED` | Dalam alur normal, invoice sudah `CLOSED` ketika obat diserahkan, sehingga **setiap** penyerahan sebagian berakhir di antrean `ADJUSTMENT_REJECTED` dan uang kembaliannya ditangani manual oleh Billing (`RJ-E2E-OQ-003`). Bila pemilik ingin penyerahan sebagian menjadi `CREDIT` otomatis, perlu keputusan baru: misalnya clearance farmasi terbit pada `FINAL` + lunas, atau adjustment kredit boleh pada `CLOSED` dan membuka kembali ke `FINAL`. Ini keputusan kebijakan keuangan, bukan urusan task ini |
| Risiko tersisa — racikan | Resep berisi racikan tidak pernah ditagih otomatis dan selalu masuk antrean. Belum ada aturan harga per bahan maupun jalur penyerahan racikan. Perlu keputusan pemilik sebelum production |
| Risiko tersisa — data lama | 10 resep draft lama di data dev masih berstatus salah (`WaitingForPayment`); konsultasinya tidak dapat diselesaikan sampai statusnya dibetulkan. Tidak dimigrasi karena di luar wewenang database |
| Titik sentuh Farmasi | Perbaikan status awal resep mengubah perilaku modul Farmasi. Sign-off formal Farmasi tetap `OPEN` sesuai roadmap |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | Dua kali aplikasi uji dimatikan untuk build ulang (sesudah temuan 1 dan 2) |
| Status Git | 7 berkas source dan dokumen blueprint `M`, laporan ini baru. Belum di-stage atau di-commit |
| Langkah berikutnya | `BE-RJE-009` (pembatalan dan koreksi) |
