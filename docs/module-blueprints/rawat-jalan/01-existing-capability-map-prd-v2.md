# Peta Kemampuan Existing — Rawat Jalan sampai Billing V2

| Field | Nilai |
| --- | --- |
| Masukan yang diaudit | `PRD-RJ-BIL-V2-001` (`PRD-Rawat-Jalan.md`, status *Draft for Owner Review*, belum di-commit) |
| Blueprint | `RJ-BIL-BP-001` (umbrella Rawat Jalan) — revisi manifest terakhir `25` |
| Skill | `trace-existing-capabilities` (read-only terhadap source aplikasi) |
| Tanggal audit | `2026-09-28` |
| Backend | `NewQuilvianSystemBackend` cabang `sukmagp` — `063d38bc306bb6b46bdf088513fa6cdcc80399d8` |
| Frontend | `V2QuilvianSystemFrontendDev` cabang `sukmagpV2` — `83b8b72744d4afaaedb3d2af9dd0b83fb272f9d6` |
| Kontrak yang berlaku di source | `BIL-INTEGRATION-0.4` dan `BIL-INTEGRATION-1.2` (`ContractBillingChargeSourceAdapter`) |
| Status peta | `CURRENT` untuk kedua SHA di atas. Menjadi `STALE` bila salah satu SHA berubah |

> **Peta ini menggantikan status Billing pada [MODULE-STATUS.md](MODULE-STATUS.md) sebagai dasar
> PRD V2**, tetapi **tidak** menggantikan penilaian task `RJ-BIL-*` lama. Peta lama
> [01-existing-capability-map.md](01-existing-capability-map.md) tetap disimpan sebagai riwayat.

---

## 1. Ringkasan untuk pemilik

PRD V2 benar pada arah besarnya: Billing canonical (`BilInvoice`/`BilInvoiceItem`) memang sudah ada,
dan Rawat Jalan tidak perlu membangun Billing kedua. Namun audit menemukan **empat hal yang
membuat PRD belum bisa dibekukan apa adanya**:

| No | Temuan | Dampak bagi pasien/kasir | Status |
| ---: | --- | --- | --- |
| 1 | **Tidak ada satu pun jalur dari fakta klinis ke invoice.** Semua fakta klinis (tindakan, Lab, Radiologi, resep) hanya masuk ke `BilFolio`. Kasir membaca `BilInvoice`. | Pasien selesai diperiksa, tetapi di layar kasir tagihannya kosong. Kasir terpaksa input ulang manual — persis yang ingin dicegah PRD. | `CONFLICT` |
| 2 | **Obat terkunci melingkar (*deadlock*).** Farmasi baru boleh menelaah dan menyerahkan obat bila resep sudah `CLEARED` (lunas) di Billing. Tetapi Billing hanya menerima item obat berstatus `DISPENSED` (sudah diserahkan). | Obat tidak bisa diserahkan karena belum dibayar, dan tidak bisa dibayar karena belum diserahkan. | `CONFLICT` |
| 3 | **Harga masih boleh datang dari pemanggil.** Endpoint `POST from-source` menyalin `UnitPrice` dan `DoctorShare` dari isian yang dikirim, bukan dari katalog tarif. | Siapa pun yang memegang `BillingInvoice : Create` dapat mencatat harga sembarang lewat API. | `CONFLICT` |
| 4 | **Biaya administrasi Rawat Jalan sudah dihitung Billing** dari master `MstAdministrationFeePolicy` (`ServiceType = RAJAL`). Usulan PRD menambah sumber `REGISTRATION` akan menagih dua kali. | Pasien kena biaya admin ganda. | Usulan PRD perlu dikoreksi |

Selain itu, dua asumsi PRD tentang kondisi existing **tidak sesuai source**:

- Tab **Laboratorium** dan **Radiologi** di workspace dokter Rawat Jalan belum ada. Tab
  `Penunjang Medis` masih berupa layar *sedang disiapkan* (`WorkInProgressTab`). Dokter Rawat Jalan
  belum bisa memesan Lab/Radiologi dari layar konsultasi.
- **Tidak ada project automated test** yang ter-commit di backend pada SHA ini. Laporan lama yang
  menyebut `22` test lulus merujuk berkas yang tidak ada di cabang ini. Karena itu **tidak ada satu
  pun kemampuan yang boleh diberi status `Ready to reuse`** pada peta ini.

---

## 2. Batas audit

| Masuk audit | Tidak masuk audit |
| --- | --- |
| Jalur Encounter → konsultasi → fakta klinis → Billing sesuai PRD §9–§22 | Pembayaran kasir, tender, settlement, refund, AR/AP (di luar scope PRD) |
| `ContractBillingChargeSourceAdapter`, `BillingInvoiceService.UpsertChargeAsync`, `BillingFolioService`, `ClinicalMilestoneFactProducer` | Kebenaran rumus kalkulasi, diskon, dan coverage di dalam `BillingCalculationService` |
| Gerbang financial clearance farmasi | Rawat inap, IGD, hemodialisis, bank darah (hanya dicatat bila memakai jalur yang sama) |
| Workspace dokter Rawat Jalan di frontend | Menjalankan aplikasi, database, atau test (tidak ada wewenang runtime) |

Metode: pencarian terarah dengan `rg`, lalu membaca implementasi yang relevan. Tidak ada source
aplikasi yang diubah.

---

## 3. Proses bisnis as-is (yang benar-benar terjadi hari ini)

**Tujuan:** pelayanan pasien Rawat Jalan tercatat dan bisa ditagih.

**Pelaku:** perawat, dokter, Lab, Radiologi, Farmasi, kasir.

**Langkah as-is (berdasarkan source):**

1. Perawat menyelesaikan skrining. Kunjungan pindah ke `WaitingForDoctor` — atau langsung
   `Completed` bila kunjungan tidak butuh dokter (`NurseStationQueueController.cs:329`).
2. Dokter mengisi SOAP, resep, dan tindakan di workspace `doctor-queues`.
3. Tindakan yang dikonfirmasi menerbitkan fakta ke `CliClinicalMilestoneFact`
   (`PatientProcedureController.cs:1324`).
4. Dokter menekan **Selesai Konsultasi**. `ConsultationFinalizationService` mengunci konsultasi,
   lalu **setelah commit** menerbitkan fakta tagihan untuk setiap resep yang difinalkan
   (`ConsultationFinalizationService.cs:217`). **Tidak ada fakta untuk biaya konsultasi.**
5. Fakta dikirim ke `BillingFolioService` dan menghasilkan `BilFolio`/`BilChargeLine`.
6. **Berhenti di sini.** Tidak ada kode yang meneruskan `BilChargeLine` ke `BilInvoice`.
7. Kasir membuka Menu Pembayaran yang membaca `BilInvoice`. Tagihan hanya terisi bila ada yang
   memanggil `POST from-source`, `POST catalog-charges`, atau `POST other-charges`.
8. Farmasi ingin menelaah resep → ditolak karena resep belum `CLEARED`
   (`PrescriptionFinancialClearanceService.cs:88`).

**Contoh konkret:** Pasien samaran *Tn. A* diperiksa dr. B, mendapat tindakan *Nebulizer* dan resep
*Paracetamol 10 tablet*. Setelah dokter menekan Selesai Konsultasi, `BilFolio` kunjungan Tn. A berisi
dua baris (Nebulizer dan resep). Di Menu Pembayaran, invoice Tn. A **tidak ada** — `GET
encounters/{encounterId}/charge-summary` menjawab `404 "Belum ada invoice untuk kunjungan ini."`.
Resep Paracetamol tidak bisa ditelaah farmasi karena belum lunas, dan tidak bisa dilunasi karena
belum ada di invoice.

---

## 4. Peta kemampuan

Bukti ditulis `path:baris` relatif terhadap repository. `BE` = backend `063d38b`, `FE` = frontend
`83b8b72`.

| ID | Kebutuhan PRD | Pemilik | Bukti | Status | Gap / adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `CAP-V2-01` | FR-001 `EncounterId` sebagai akar korelasi | Billing | BE `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInvoiceConfiguration.cs:26` (`HasIndex(EncounterId).IsUnique()`); BE `BillingInvoiceService.cs` lock `BIL_ENCOUNTER_{id}` pada `UpsertChargeAsync` | Reuse with adapter | Mekanisme siap; belum ada pemanggil dari sisi klinis | Rendah |
| `CAP-V2-02` | FR-002 invoice berjalan terbentuk otomatis | Billing | BE `BillingInvoiceService.cs:1457` — `UpsertChargeAsync` mencari invoice per encounter lalu membuatnya bila belum ada | Reuse with adapter | Butuh adapter Clinical Fact → `UpsertChargeAsync` (lihat `CAP-V2-13`) | Tinggi bila adapter tidak dibuat |
| `CAP-V2-03` | FR-003, `SEC-RJ-002` harga dari tarif server | Billing | BE `BillingInvoiceDtos.cs:145` `UnitPrice`, `:152` `DoctorShare` pada `UpsertChargeRequest`; BE `BillingInvoiceService.cs:1898-1899` menyalin keduanya apa adanya. Pembanding: `catalog-charges` sudah menetapkan harga dari `MstTariff` (`BillingInvoicesController.cs:233`) | Conflict | Adapter harus menetapkan harga dari tarif; `from-source` publik tetap menerima harga dari pemanggil | **Tinggi** — manipulasi harga lewat API |
| `CAP-V2-04` | FR-004 Tindakan → invoice | Clinical → Billing | BE `BillingChargeSourceAdapter.cs:26-29` (`PROCEDURE`); BE `PatientProcedureController.cs:1324` menerbitkan fakta, tujuan folio | Reuse with adapter | Status kontrak cocok; jalur kirim belum ke invoice | Sedang |
| `CAP-V2-05` | FR-005 Laboratorium → invoice | Laboratory → Billing | BE `BillingChargeSourceAdapter.cs:30-33`; BE `LabSpecimenService.cs:675`, `:1086` | Reuse with adapter | Sama dengan `CAP-V2-04` | Sedang |
| `CAP-V2-06` | FR-006 Radiologi → invoice | Radiology → Billing | BE `BillingChargeSourceAdapter.cs:34-37`; BE `RadStudyService.cs:537` | Reuse with adapter | Sama dengan `CAP-V2-04` | Sedang |
| `CAP-V2-07` | FR-007 obat ditagih saat `DISPENSED` | Pharmacy → Billing | BE `BillingChargeSourceAdapter.cs:39`, `:87-88` hanya menerima `DISPENSED`; BE `PrescriptionDispensingService.cs:158`, `:287`, `:626` mensyaratkan clearance; BE `PrescriptionFinancialClearanceService.cs:74-88` hanya `CLEARED` yang lolos; BE `BilConsumerHandoffService.cs:301-305` clearance dibentuk dari item `PHARMACY` di invoice; BE `ConsultationFinalizationService.cs:210-212` menagih resep saat finalisasi (`RJ-BIL-DEC-002`) | Conflict | Tiga aturan saling bertentangan: PRD (`DISPENSED`), keputusan lama `RJ-BIL-DEC-002` (saat finalisasi), dan gerbang farmasi (lunas sebelum serah) | **Kritis** — alur obat Rawat Jalan terkunci |
| `CAP-V2-08` | FR-008 biaya konsultasi punya sumber resmi | Clinical → Billing | Tidak ada domain `CONSULTATION` di `BillingChargeSourceAdapter.cs:23-74` maupun `BillingSourceContract.cs:21-56`. Master tarif **sudah ada**: BE `MstTariff.cs:37` `IsConsultationFee`, `MstTariffCategory.cs:23`, `MstPatientClass.cs:51` `DefaultConsultationFee`, `DoctorServiceRuleDtos.cs:52` `TariffId` | Missing | Domain sumber belum ada; data tarif bisa dipakai ulang | Tinggi — jasa dokter tidak tertagih |
| `CAP-V2-09` | FR-009 biaya registrasi/admin berbasis konfigurasi | Billing | BE `AdministrationFeePolicyDtos.cs:7` `RAJAL`; BE `AdministrationFeeCalculationService.cs:43`; BE `BillingCalculationService.cs:194`, `:541` | Reuse with adapter | Sudah berbasis policy di dalam kalkulasi. Usulan sumber `REGISTRATION` **berpotensi menagih ganda**. Penerapan pada invoice Rawat Jalan belum diverifikasi dengan test | Sedang |
| `CAP-V2-10` | §10 finalisasi konsultasi satu jalur canonical | Clinical | BE `ConsultationFinalizationService.cs` (commit `:205-206`, emisi `:217`, respons `BillingHandoffIssues` `:236`); FE `doctor-consultation.service.js:89-93` `completeDoctorConsultation` | Extend | Tidak menerbitkan fakta konsultasi; FE tidak membaca `BillingHandoffIssues` sehingga dokter tidak tahu bila penyerahan gagal | Sedang |
| `CAP-V2-11` | §11 Rawat Jalan berhenti di `ConsultationCompleted`, tidak boleh langsung `Completed` | Registration | BE `NurseStationQueueController.cs:327-329` set `Completed` bila tidak perlu dokter; BE `PatientEncounterController.cs:944-953` mengizinkan lompat status ke `Completed` tanpa validasi transisi (`RM-CAP-019`) | Conflict | Butuh keputusan: kunjungan tanpa dokter dan endpoint ubah status bebas | Sedang — kunjungan bisa tertutup sebelum ditagih |
| `CAP-V2-12` | §12 ledger fakta durable, idempotent, berversi | Clinical Integration | BE `ClinicalMilestoneFactProducer.cs:80-222` (`Pending`, `SuppressedNoPriorCharge`), `:252-298` aturan replay, `:382-441` hasil dispatch; model `CliClinicalMilestoneFact.cs:14` | Reuse with adapter | Target dispatch masih `BillingFolioService` (`:44`, `:346`) | Sedang |
| `CAP-V2-13` | §13 satu jalur Folio ↔ Invoice (P0) | Billing | Tidak ada referensi `BilInvoice`/`BillingInvoiceService` di `Operational/`, `ClinicalManagement/`, `PharmacyManagement/`, `LaboratoryManagement/`, `RadiologyManagement/`. Folio dibaca `PatientBillingSummaryService.cs:95-106` (rawat inap) | Conflict | Dua sumber kebenaran finansial sudah hidup berdampingan | **Kritis** |
| `CAP-V2-14` | §15 idempotency per sumber dan versi | Billing | BE `BillingInvoiceService.cs` — cek `BilChargeReceipt` per `IdempotencyKey`, tolak versi lebih lama, versi sama beda isi → `409` (`:1540-1543`) | Reuse with adapter | Adapter harus menurunkan `IdempotencyKey` stabil dari identitas fakta | Rendah |
| `CAP-V2-15` | §16 koreksi dan pembatalan | Billing + Clinical | Kasus A: `ClinicalMilestoneFactProducer.cs:140-142`. Kasus B: `BillingChargeSourceAdapter.cs:104-106` hanya boleh void dari `CONFIRMED`/`ACCEPTED`; `PHARMACY` dan `CONSUMABLE` tidak punya status void (`:39-40`) | Extend | Pelayanan yang sudah `PERFORMED`/`COMPLETED` tidak bisa di-void; harus lewat adjustment Billing. Jalurnya belum dikontrakkan untuk sumber klinis | Sedang |
| `CAP-V2-16` | §16 Kasus C `OutcomeUnknown` direkonsiliasi | Clinical Integration | Penanda `OutcomeUnknown` ada (`ClinicalMilestoneFactProducer.cs:414`, `:431`); replay identitas sama (`:252`). **Tidak ditemukan** `BackgroundService` yang memproses ulang | Extend | Tanpa pekerja rekonsiliasi, fakta `OutcomeUnknown` hanya diproses ulang bila aksi klinis diulang | Sedang |
| `CAP-V2-17` | §17–18 endpoint baca Ringkasan Billing | Billing | BE `BillingInvoicesController.cs:202-206` `GET encounters/{encounterId}/charge-summary`; DTO `BillingInvoiceDtos.cs:346-360`, `:364-378` | Reuse with adapter | Belum ada: status coverage, waktu pembaruan terakhir, status per pelayanan. Menjawab `404` saat invoice belum ada. Hak akses `BillingInvoice : Read` terlalu luas (lihat §6) | Sedang |
| `CAP-V2-18` | §8, §19 panel Ringkasan Billing di workspace dokter | Frontend | FE `doctor-queue.constants.js:64-110` — tidak ada tab billing; tidak ada konsumen `charge-summary` di `src/`. Kandidat reuse: FE `patient-billing-summary.service.js`, `billing-summary-card.module.css` (rawat inap) | Missing | Panel baru; base component tersedia | Rendah |
| `CAP-V2-19` | §8 tab Laboratorium dan Radiologi di workspace dokter | Frontend | FE `doctor-queue.constants.js:94-96` `supportingExam` → `WorkInProgressTab` (`doctor-queue-view.jsx:196-200`). `rad-order.service.js` hanya dipakai rawat inap | Missing | PRD menganggap sudah ada | Tinggi — FR-005/006 tidak punya pintu masuk di Rawat Jalan |
| `CAP-V2-20` | `SEC-RJ-001` otorisasi server | Billing | BE `BillingInvoicesController.cs:13` `[Authorize]`, setiap action memakai `[AccessPermission]` | Reuse with adapter | Butuh permission baca terbatas untuk dokter | Rendah |
| `CAP-V2-21` | `SEC-RJ-004` teks aman dari XSS | Frontend | FE tidak ditemukan `dangerouslySetInnerHTML` di `registration-management`, `billing-management`, `doctor-queue-features` | Reuse with adapter | Pertahankan pada panel baru | Rendah |
| `CAP-V2-22` | `SEC-RJ-006` audit minimum | Billing + Clinical | BE `BillingInvoiceService.cs` `AuditAsync("BillingInvoice.CreateCharge"/"UpsertCharge")` pada `UpsertChargeAsync` | Unknown | Kelengkapan field (`SourceVersion`, `CorrelationId`, `Outcome`) belum diperiksa | Rendah |
| `CAP-V2-23` | §20 base component frontend | Frontend | FE `src/components/features/base-features/` — `hero.jsx`, `data-table.jsx`, `data-filter.jsx`, `status-badge.jsx`, `base-button.jsx`, `access-denied-gate.jsx`, `toast-stack.jsx`, `filter-select.jsx`, `filter-date-picker.jsx` | Reuse with adapter | Semua ada; perilaku belum diuji ulang | Rendah |
| `CAP-V2-24` | §14 `CONSUMABLE` / `USED` | Belum ada pemilik | Hanya kebijakan di `BillingChargeSourceAdapter.cs:40`; tidak ada producer | Missing | PRD menulis `REUSE` — tidak sesuai | Rendah (PRD: "bila tersedia") |
| `CAP-V2-25` | DoD 17–20 uji otorisasi dan E2E | Semua | `git ls-files` backend `063d38b` tidak memuat project test apa pun | Missing | Harness test harus dibangun atau dipulihkan | Tinggi — tidak ada bukti verifikasi |

Jumlah: `0` Ready to reuse · `13` Reuse with adapter · `2` Extend · `0` Repair · `5` Missing ·
`4` Conflict · `1` Unknown (`25` baris).

---

## 5. Kontrak API as-is yang relevan

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/encounters/{encounterId}/charge-summary` | Rekap tagihan satu kunjungan per kategori, angka dari mesin kalkulasi yang sama dengan Menu Pembayaran | `BillingInvoice : Read` | path `encounterId` | `EncounterChargeSummaryResponse` |
| `GET` | `/{id}` | Detail invoice lengkap beserta item | `BillingInvoice : Read` | path `id` | `InvoiceDetailResponse` |
| `GET` | `/{id}/calculation-preview` | Pratinjau hasil hitung invoice | `BillingInvoice : Read` | path `id` | `CalculationResponse` |
| `POST` | `/from-source` | Mencatat atau memperbarui item dari sistem sumber (**harga ikut dikirim pemanggil**) | `BillingInvoice : Create` | header `Idempotency-Key`, body `UpsertChargeRequest` | `InvoiceDetailResponse` |
| `POST` | `/catalog-charges` | Menambah item dari katalog tarif; harga ditetapkan server | `BillingInvoice : Create` | body berisi `TariffId` dan kuantitas | `InvoiceDetailResponse` |
| `POST` | `/{id}/items/{itemId}/void` | Membatalkan satu item | `BillingInvoice : Update` | body `VoidInvoiceItemRequest` | `InvoiceDetailResponse` |

Arti kode status untuk pengguna:

- `200` — data berhasil diambil atau disimpan.
- `403` — pengguna tidak punya hak akses untuk tindakan ini.
- `404` pada `charge-summary` — **belum ada invoice** untuk kunjungan ini. Untuk Rawat Jalan ini
  keadaan normal di awal kunjungan, bukan galat.
- `409` — versi sumber yang sama dikirim dengan isi berbeda, atau versi pembatalan tidak lebih baru.
- `422` — isian tidak memenuhi aturan Billing, misalnya obat belum `DISPENSED`.

### Health Services / Billing Management / Billing Folio

Base URL: `api/v1/health-services/billing-management/folios`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/by-encounter/{encounterId}` | Membaca folio (buku catatan fakta tagihan) satu kunjungan | `BillingFolio : Read` | path `encounterId` | `BillingFolioDetailResponse` |
| `GET` | `/{folioId}` | Membaca satu folio | `BillingFolio : Read` | path `folioId` | `BillingFolioDetailResponse` |
| `POST` | `/internal/milestones/recognize` | Pintu internal pengakuan milestone | `BillingMilestone : RecognizeInternal` | `RecognizeBillingMilestoneRequest` | `RecognizeBillingMilestoneResponse` |

### Tabel status sumber yang diterima Billing hari ini

| `SourceDomain` | Status yang bisa ditagih | Boleh di-void dari | Status pembatalan |
| --- | --- | --- | --- |
| `PROCEDURE`, `LABORATORY`, `RADIOLOGY` | `CONFIRMED`, `ACCEPTED`, `COMPLETED`, `PERFORMED` | `CONFIRMED`, `ACCEPTED` | `CANCELLED`, `VOIDED` |
| `PHARMACY` | `DISPENSED` | — | — |
| `CONSUMABLE` | `USED` | — | — |
| `CONSULTATION` | **tidak ada** | — | — |

**Contoh:** Lab *Darah Lengkap* versi `1` status `ACCEPTED` masuk invoice. Lab dibatalkan, versi `2`
status `CANCELLED` → item di-void, riwayat tetap ada. Tetapi bila Lab sudah versi `2` status
`COMPLETED`, pembatalan versi `3` **ditolak** dengan pesan *"Item tidak dapat dibatalkan karena
pelayanan atau pembayaran sudah diproses."*

---

## 6. Ketidakcocokan frontend/backend dan catatan hak akses

1. **Hak akses Ringkasan Billing terlalu luas.** `charge-summary` memakai `BillingInvoice : Read`.
   Butir yang sama membuka daftar semua invoice, riwayat pembayaran, dan `cashier-overview`.
   Memberikannya ke dokter berarti dokter bisa membaca tagihan seluruh pasien lewat API. Rawat inap
   sudah memecahkan masalah yang sama dengan butir tersendiri `PatientBillingSummary : Read`
   (`PatientBillingSummaryController.cs:23-26`), tetapi endpoint itu membaca **folio per episode**,
   bukan invoice.
2. **Respons finalisasi diabaikan frontend.** Backend mengembalikan `BillingHandoffIssues`, frontend
   tidak membacanya.
3. **Layar folio `RJ-BIL-FE-001`/`FE-002` tidak ditemukan** di frontend `83b8b72`. Yang tersisa
   hanya kartu ringkasan tagihan rawat inap.

---

## 7. Daftar `CONFLICT` dan `UNKNOWN`

| ID | Isi | Butuh keputusan dari |
| --- | --- | --- |
| `CAP-V2-03` | `from-source` menerima harga dari pemanggil | Billing/Finance + Security |
| `CAP-V2-07` | Titik tagih obat: finalisasi resep vs `DISPENSED` vs gerbang lunas-sebelum-serah | Farmasi + Billing/Finance |
| `CAP-V2-11` | Kunjungan bisa `Completed` tanpa melewati Billing | Registration + Billing |
| `CAP-V2-13` | Folio dan Invoice sebagai dua kebenaran finansial | Billing (pemilik arsitektur) |
| `CAP-V2-22` | Kelengkapan field audit | Diperiksa saat desain |

---

## 8. Pemicu impact scan

Peta ini harus ditandai `STALE` dan dipindai ulang terbatas bila salah satu berkas berikut berubah:
`BillingChargeSourceAdapter.cs`, `BillingInvoiceService.cs` (`UpsertChargeAsync`, `ApplySource`,
`GetChargeSummaryByEncounterAsync`), `ClinicalMilestoneFactProducer.cs`, `BillingFolioService.cs`,
`ConsultationFinalizationService.cs`, `PrescriptionFinancialClearanceService.cs`,
`BilConsumerHandoffService.cs`, `NurseStationQueueController.cs`, `doctor-queue.constants.js`,
atau `PRD-Rawat-Jalan.md` sendiri.

---

## 9. Fakta, inferensi, rekomendasi

- **Fakta:** seluruh isi §4–§6 beserta buktinya.
- **Inferensi:** alur obat Rawat Jalan saat ini tidak dapat diselesaikan dari UI (§3 langkah 8),
  disimpulkan dari gerbang clearance tanpa pembentuk item `PHARMACY`. Belum dibuktikan dengan
  menjalankan aplikasi.
- **Rekomendasi (bukan keputusan):** bekukan jawaban atas empat `CONFLICT` sebelum PRD naik ke
  desain. Pertanyaannya diajukan melalui sesi `grill-me`.
