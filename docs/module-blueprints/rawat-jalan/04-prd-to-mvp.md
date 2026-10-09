# PRD → MVP — Rawat Jalan sampai Tagihan (V2)

## 1. Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian Hospital Information System |
| Modul | Rawat Jalan (umbrella `RJ-BIL-BP-001`) — amendment V2 |
| Kode modul dokumen | `RJE` |
| Status | `approved` — Sukma Giri, 2026-09-28 (`RJ-E2E-DEC-015`) |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.0` |
| Repository target | `NewQuilvianSystemBackend` (`sukmagp`), `V2QuilvianSystemFrontendDev` (`sukmagpV2`) |
| Baseline SHA | backend `063d38bc306bb6b46bdf088513fa6cdcc80399d8`; frontend `83b8b72744d4afaaedb3d2af9dd0b83fb272f9d6` |
| Masukan | `PRD-RJ-BIL-V2-001`; `00-interview-decisions.md` revisi `18`; `01-existing-capability-map-prd-v2.md`; `02`/`03` bagian V2; `contracts/*` bagian V2; `data/data-dictionary.md` |
| Domain architecture | revisi `1`, `DOMAIN_ARCHITECTURE_PARTIAL`; slice ini di core internal yang siap independen |
| Cakupan satu baris | Setiap pelayanan Rawat Jalan berbiaya masuk otomatis ke satu tagihan kunjungan berharga katalog, dan dokter dapat melihat ringkasannya tanpa bisa mengubah angka |

## 2. Ringkasan eksekutif

Hari ini pasien Rawat Jalan selesai diperiksa, tetapi tagihannya kosong di layar kasir: pelayanan
tercatat di buku folio Billing dan tidak pernah sampai ke invoice. Kasir terpaksa mengetik ulang,
harga bisa diketik bebas, jasa konsultasi tidak tertagih, dan resep terkunci karena farmasi
menunggu lunas sementara obat baru boleh ditagih setelah diserahkan.

MVP ini menyambungkan pelayanan ke invoice lewat satu jembatan di Billing, memakai harga dari
katalog tarif, menagih jasa konsultasi, memecah tagihan obat menjadi dua tahap agar farmasi bisa
bekerja, dan menyediakan antrean rekonsiliasi bagi petugas Billing. Dokter mendapat tab Ringkasan
Billing baca saja.

## 3. Masalah produk

| Kondisi sekarang | Bukti (backend `063d38b`) | Akibat |
|---|---|---|
| Fakta klinis hanya masuk folio | `ClinicalMilestoneFactProducer.cs:44`, `:346`; tidak ada referensi invoice di modul klinis/folio | Invoice kosong; kasir input ulang |
| Harga dari pemanggil | `BillingInvoiceService.cs:1898-1899` | Harga dapat dimanipulasi |
| Tidak ada domain konsultasi | `BillingChargeSourceAdapter.cs:23-74` | Jasa dokter tidak tertagih |
| Obat hanya `DISPENSED`, farmasi menunggu lunas | `BillingChargeSourceAdapter.cs:87-88`; `PrescriptionFinancialClearanceService.cs:74-88` | Alur obat terkunci |
| Kunjungan tanpa dokter langsung `Completed` | `NurseStationQueueController.cs:327-329` | Kunjungan tertutup sebelum ditagih |
| Tidak ada kirim ulang otomatis | Tidak ada pekerja untuk `OutcomeUnknown` | Tagihan tertinggal tanpa ada yang tahu |

Yang **sudah ada** dan dipakai ulang: invoice per kunjungan dengan unique index, idempotency
`BilChargeReceipt`, mesin kalkulasi, adjustment dengan persetujuan, katalog tarif lengkap dengan
penanda konsultasi, clearance farmasi, dan pola pekerja latar.

## 4. Visi produk

1. Pasien terdaftar dan membuka kunjungan Rawat Jalan.
2. Perawat skrining; dokter memeriksa, mengerjakan tindakan, memesan pemeriksaan, meresepkan.
3. Setiap pelayanan yang terjadi menerbitkan fakta pelayanan.
4. Fakta tercatat di buku folio.
5. Jembatan Billing mencari harga di katalog tarif dan menulis item ke tagihan kunjungan.
6. Kasir menerima pembayaran dari tagihan yang sudah lengkap.
7. Farmasi menyerahkan obat; jumlah aktual menyesuaikan tagihan.
8. Kegagalan di mana pun dicoba ulang dengan identitas yang sama, lalu masuk antrean manusia.

## 5. Batas MVP

**Titik mulai:**

1. Kunjungan bertipe `Outpatient` sudah terdaftar.
2. Pelayanan terjadi melalui jalur yang sudah ada: eksekusi tindakan, penerimaan spesimen Lab,
   penerimaan mutu study Radiologi, finalisasi konsultasi canonical, dispensing resep.

**Titik akhir:**

1. Setiap pelayanan berbiaya menjadi item invoice berharga katalog, atau tercatat di antrean
   rekonsiliasi dengan sebabnya.
2. Obat dapat dibayar sebelum diserahkan dan disesuaikan setelah diserahkan.
3. Kunjungan berhenti di `ConsultationCompleted` atau `Billing`.
4. Dokter dapat membaca Ringkasan Billing kunjungan yang sedang ia tangani.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP |
|---|---|
| Perawat poli | Menyelesaikan skrining; kunjungan tanpa dokter diserahkan ke Billing |
| Dokter | Menyelesaikan konsultasi; membaca Ringkasan Billing dan pemberitahuan penyerahan |
| Laboratorium / Radiologi | Menjalankan pemeriksaan seperti biasa — tanpa langkah tambahan |
| Farmasi | Menelaah dan menyerahkan obat setelah lunas — tanpa langkah tambahan |
| Kasir | Menerima pembayaran dari tagihan yang terisi otomatis |
| Petugas Billing | Menangani antrean rekonsiliasi |
| Admin Billing | Mengatur kebijakan kirim ulang |
| Admin tarif | Melengkapi tarif yang hilang (data master yang sudah ada) |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Invoice terbentuk otomatis per kunjungan | `CAP-V2-01`, `CAP-V2-02` | Wajib; tanpa ini kasir input ulang |
| Jembatan folio → invoice | `CAP-V2-13`, `CAP-V2-12`, `CAP-V2-14` | Wajib; inti seluruh MVP |
| Harga dari katalog tarif | `CAP-V2-03` | Wajib; tanpa ini harga dapat dimanipulasi |
| Tindakan, Lab, Radiologi masuk invoice | `CAP-V2-04`, `05`, `06` | Wajib; pelayanan inti |
| Jasa konsultasi | `CAP-V2-08` | Wajib; pendapatan inti Rawat Jalan |
| Obat dua tahap | `CAP-V2-07` | Wajib; tanpa ini alur obat terkunci |
| Biaya admin tetap dari policy `RAJAL` | `CAP-V2-09` | Wajib dipastikan (tanpa pekerjaan baru selain verifikasi) |
| Pembatalan dan koreksi | `CAP-V2-15` | Wajib; tanpa ini salah pasien tidak dapat dikoreksi aman |
| Kirim ulang otomatis + antrean rekonsiliasi | `CAP-V2-16` | Wajib; tanpa ini kegagalan tidak terlihat |
| Kunjungan tanpa dokter tidak `Completed` | `CAP-V2-11` | Wajib; tanpa ini kunjungan tertutup sebelum ditagih |
| Ringkasan Billing dokter | `CAP-V2-17`, `CAP-V2-18` | Wajib (PRD §8, DoD 13) |
| Pemberitahuan penyerahan saat Selesai Konsultasi | `CAP-V2-10` | Wajib; tanpa ini dokter tidak tahu penyerahan gagal |

## 8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| `CONSUMABLE` / `USED` | `CAP-V2-24` | Producer belum ada di modul mana pun; butuh keputusan siapa pencatat pemakaian (`RJ-E2E-DEC-011`) | Kasir memakai `catalog-charges` yang harganya dari tarif |
| Pemesanan Lab/Radiologi dari workspace dokter | `CAP-V2-19` | Tetap `CONDITIONAL` (`RJ-DOC-DEC-002`); pintu masuk order ada di modul Lab/Radiologi | Order lewat modul Lab/Radiologi yang sudah ada; faktanya tetap tertagih otomatis |
| Layar master kebijakan kirim ulang | `RJ-E2E-DEC-009` | Perubahan jarang; endpoint terjaga hak akses dan ter-audit sudah cukup | Admin Billing memakai endpoint `PUT` |
| Pengamanan `PATCH` status kunjungan ke `Completed` | `CAP-V2-11` | Menunggu `RJ-E2E-DEC-004` (siapa menutup kunjungan) | Frontend Rawat Jalan tidak memanggilnya |
| Penerusan fakta rawat inap, IGD, Bank Darah, hemodialisis | — | Milik modul lain; di luar scope | Alur mereka tidak berubah |
| Ringkasan rawat inap pindah dari folio ke invoice | — | Di luar scope (`RJ-E2E-DEC-003`) | Tetap membaca folio |
| Pembagian jasa medis (`DoctorShare`) | — | Milik modul medical-fee | `DoctorShare = 0`, sama dengan `catalog-charges` |
| Dokumen reimbursement asuransi perusahaan | `RJ-BIL-DEC-016` | Kebutuhan tanpa task dan kontrak | Proses manual di luar sistem |

## 9. Alur bisnis target

`FLOW-RJE-MVP-001` — Pelayanan Rawat Jalan sampai tagihan lengkap:

1. Perawat menyelesaikan skrining. Bila tidak butuh dokter, kunjungan menjadi `Billing`.
2. Dokter mengerjakan tindakan → fakta tindakan → folio → item `PROCEDURE`.
3. Lab menerima spesimen → item `LABORATORY`; Radiologi menerima mutu study → item `RADIOLOGY`.
4. Dokter menekan Selesai Konsultasi → item `CONSULTATION` dan item `PHARMACY` `PRESCRIBED`;
   kunjungan `ConsultationCompleted`; dokter melihat pemberitahuan bila ada masalah penyerahan.
5. Kasir menerima pembayaran; clearance resep menjadi `CLEARED`.
6. Farmasi menelaah dan menyerahkan obat → item `PHARMACY` `DISPENSED` atau penyesuaian.
7. Kegagalan dicoba ulang otomatis; yang tetap gagal ditangani petugas Billing di antrean.

Rincian dan jalur gagal: [flowcharts/](flowcharts/00-alur-utama.md).

## 10. Epic dan functional requirement

### `EPIC RJE-01` — Fondasi data dan kontrak — disposisi `EXTEND`

Tujuan: kolom, master, dan kontrak siap sebelum jembatan berjalan.

> **FR-RJE-001 — Kolom sinkron dan rekonsiliasi**
>
> `BilChargeLine` dan `CliClinicalMilestoneFact` memiliki kolom sesuai
> [data-dictionary.md](data/data-dictionary.md) bagian 2.
>
> **Contoh:** setelah migration, `SELECT "InvoiceSyncStatus" FROM "BilChargeLine"` mengembalikan
> `0` atau `4` untuk seluruh baris lama, tanpa `NULL`.

> **FR-RJE-002 — Kebijakan kirim ulang**
>
> `MstBillingSyncPolicy` berisi `FACT_DISPATCH` dan `INVOICE_SYNC` (5 / 60 / 3600, aktif).
> Tanpa baris aktif, tidak ada kirim ulang otomatis.
>
> **Contoh:** admin menonaktifkan `INVOICE_SYNC`; kegagalan berikutnya langsung
> `ReconciliationRequired` `SYNC_POLICY_INACTIVE`.

> **FR-RJE-003 — Kontrak adapter 1.3**
>
> `PHARMACY` menerima `PRESCRIBED` hanya dengan `BIL-INTEGRATION-1.3`; `CONSULTATION` menerima
> `COMPLETED`.
>
> **Contoh:** pemanggil lama mengirim `PHARMACY`/`PRESCRIBED` dengan kontrak `1.2` → tetap ditolak
> "Jumlah obat yang diserahkan belum final."

> **FR-RJE-004 — Baris lama**
>
> Baris folio lama Rawat Jalan dari lima konteks → `ReconciliationRequired` `LEGACY_PRE_BRIDGE`;
> nol item invoice baru.
>
> **Contoh:** 25 dari 40 baris lama masuk antrean; 15 `NotApplicable`.

### `EPIC RJE-02` — Jembatan folio ke invoice — disposisi `MISSING / NEW`

> **FR-RJE-010 — Kelayakan.** Hanya kunjungan `Outpatient` dan lima konteks yang diteruskan;
> pengulangan Radiologi `InternalHospitalError` dan tindakan gratis `NotApplicable`.
> **Contoh:** tindakan pada kunjungan rawat inap → `NotApplicable` `NOT_OUTPATIENT`; invoice rawat
> inap tidak berubah.

> **FR-RJE-011 — Identitas stabil.** Kunci idempotency = Guid deterministik dari
> `MilestoneFactId` + versi. **Contoh:** jembatan dan pekerja memproses baris yang sama
> bersamaan → satu item; percobaan kedua replay.

> **FR-RJE-012 — Harga dari katalog.** Harga dari `MstTariff` berlaku pada tanggal pelayanan
> (`02` bagian `V2.7.4`); harga snapshot klinis diabaikan. **Contoh:** snapshot `unitPrice`
> Rp99.999, tarif Nebulizer Rp75.000 → item Rp75.000.

> **FR-RJE-013 — Tanpa tarif, tanpa tagihan Rp0.** **Contoh:** Radiologi *Thorax PA* tanpa tarif →
> antrean `TARIFF_NOT_FOUND`, bukan item Rp0.

> **FR-RJE-014 — `from-source` menolak domain klinis Rawat Jalan.** **Contoh:** kasir mengirim
> `PROCEDURE` Rp1 lewat API → `422 RJE-VAL-010`; `ADHOC` tetap diterima.

> **FR-RJE-015 — Perubahan pasca-final lewat adjustment.** **Contoh:** Lab diterima setelah
> invoice `FINAL` → adjustment `DEBIT` Rp85.000 `SUBMITTED`.

### `EPIC RJE-03` — Jasa konsultasi — disposisi `EXTEND`

> **FR-RJE-020 — Fakta konsultasi.** Finalisasi canonical menerbitkan tepat satu fakta
> `Consultation` setelah commit. **Contoh:** tombol Selesai ditekan dua kali → satu item
> `CONSULTATION`.

> **FR-RJE-021 — Urutan tarif konsultasi** (`RJ-E2E-DEC-012`). **Contoh:** rule dr. B Rp150.000 ada
> → Rp150.000; tanpa rule, tarif klinik + kelas Rp120.000 → Rp120.000; tidak ada keduanya →
> antrean.

### `EPIC RJE-04` — Obat dua tahap — disposisi `EXTEND`

> **FR-RJE-030 — Tahap 1.** Resep difinalkan → item `PHARMACY` `PRESCRIBED`, harga
> Σ jumlah × tarif. **Contoh:** 10 × Rp1.500 + 10 × Rp2.000 = Rp35.000.

> **FR-RJE-031 — Tahap 2.** Dispensing menerbitkan versi baru dengan jumlah diserahkan.
> **Contoh:** 8 Vitamin C → Rp31.000 (invoice `OPEN`) atau adjustment `CREDIT` Rp4.000 (invoice
> `FINAL`).

> **FR-RJE-032 — Deadlock hilang.** Invoice berisi item `PRESCRIBED` dapat dilunasi dan clearance
> resep menjadi `CLEARED`. **Contoh:** Tn. A membayar Rp35.000 → farmasi dapat mulai telaah.

### `EPIC RJE-05` — Pembatalan dan koreksi — disposisi `EXTEND`

> **FR-RJE-040 — Void bila masih boleh.** Pembatalan atas item `ACCEPTED`/`PRESCRIBED` pada
> invoice `OPEN` → item `VOIDED`. **Contoh:** Lab `ACCEPTED` dibatalkan → item batal, riwayat ada.

> **FR-RJE-041 — Adjustment bila sudah dikerjakan.** **Contoh:** Nebulizer `PERFORMED` salah pasien →
> adjustment `CREDIT` Rp75.000 `SUBMITTED`; item tetap `ACTIVE`.

> **FR-RJE-042 — Tanpa pembatalan palsu.** **Contoh:** Lab batal sebelum diterima → tidak ada
> void maupun adjustment.

### `EPIC RJE-06` — Keandalan dan rekonsiliasi — disposisi `MISSING / NEW`

> **FR-RJE-050 — Kirim ulang fakta.** `ClinicalFactDispatchWorker` memakai identitas sama.
> **Contoh:** fakta `OutcomeUnknown` dikirim ulang 60 detik kemudian dengan `IdempotencyKey` yang
> sama.

> **FR-RJE-051 — Kirim ulang invoice.** `BilInvoiceSyncWorker` dengan jadwal 60/120/240/480 detik
> lalu antrean. **Contoh:** gagal ke-5 → `RETRY_EXHAUSTED`.

> **FR-RJE-052 — API antrean** (daftar, kirim ulang, selesaikan manual). **Contoh:** selesaikan
> manual tanpa alasan → `422 RJE-VAL-022`.

> **FR-RJE-053 — Layar antrean `FE-RJE-03`** terjangkau dari menu *Billing dan Kasir*.
> **Contoh:** petugas Billing tanpa `Update` melihat daftar tanpa tombol aksi.

> **FR-RJE-054 — API kebijakan kirim ulang.** **Contoh:** `MaxAttemptCount = 25` → `422 RJE-VAL-030`.

### `EPIC RJE-07` — Ringkasan Billing — disposisi `MISSING / NEW`

> **FR-RJE-060 — Endpoint ringkasan** dengan butir `EncounterBillingSummary : Read`, tanpa harga
> per item, `200 NO_INVOICE` saat belum ada invoice. **Contoh:** dr. B memanggil
> `GET /billing/invoices/{id}` → `403`.

> **FR-RJE-061 — Tab `FE-RJE-01`.** Angka tampil apa adanya. **Contoh:** total di tab = total
> `calculation-preview` invoice, sampai rupiah terakhir.

> **FR-RJE-062 — Pemberitahuan `FE-RJE-02`.** **Contoh:** Billing mati saat Selesai Konsultasi →
> konsultasi selesai, dokter melihat "akan dicoba ulang otomatis".

### `EPIC RJE-08` — Status kunjungan tanpa dokter — disposisi `EXTEND`

> **FR-RJE-070** — Skrining selesai tanpa dokter → kunjungan `Billing`, `CompletedAt` kosong.
> **Contoh:** pasien kontrol tekanan darah di poli tanpa dokter → status *Dalam proses billing*.

Tidak ada epic berstatus `OPEN DECISION`.

## 11. Model status yang diusulkan

| Keadaan | Nilai | Invariant utama |
|---|---|---|
| Sinkron baris folio | `NotApplicable`, `Pending`, `Synced`, `Failed`, `ReconciliationRequired`, `Resolved` | Setiap baris layak berakhir `Synced`, `ReconciliationRequired`, atau `Resolved`; `Resolved` final |
| Penyerahan fakta | `Pending`, `Dispatched`, `Rejected`, `OutcomeUnknown`, `SuppressedNoPriorCharge` (tidak berubah) | Kirim ulang tidak pernah memakai kunci baru |
| Item `PHARMACY` | `PRESCRIBED` → `DISPENSED` | Versi tidak mundur |
| Kunjungan | berhenti di `ConsultationCompleted` / `Billing` | Rawat Jalan tidak menulis `Completed` |

Rincian: [contracts/state-transition-matrix.md](contracts/state-transition-matrix.md) bagian V2.

## 12. Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| `BilInvoice`/`BilInvoiceItem`, `UpsertChargeAsync`, `VoidItemAsync`, `CreateAdjustmentAsync`, `BillingCalculationService`, `MstTariff`, `MstDoctorServiceRule`, `MstAdministrationFeePolicy`, clearance farmasi, pola pekerja latar | `CliClinicalMilestoneFact`, `BilChargeLine`, `ClinicalMilestoneFactProducer`, `BillingFolioService`, `ConsultationFinalizationService`, `PrescriptionDispensingService`, `ContractBillingChargeSourceAdapter`, `NurseStationQueueController`, `from-source` | `BillingClinicalChargeBridgeService`, `BillingSourceTariffResolver`, dua pekerja latar, `MstBillingSyncPolicy`, tiga controller baru, dua layar/bagian frontend |

Rincian: [02-backend-architecture.md](02-backend-architecture.md) bagian V2.

## 13. Sasaran kemampuan API

### Health Services / Billing Management / Encounter Billing Summary

Base URL: `api/v1/health-services/billing-management/encounter-billing-summaries`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/{encounterId}` | Ringkasan tagihan satu kunjungan | `EncounterBillingSummary : Read` | path | `ApiResponse<EncounterBillingSummaryResponse>` | `EPIC RJE-07` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Billing / Charge Reconciliations

Base URL: `api/v1/health-services/billing-management/billing/charge-reconciliations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar antrean | `BillingChargeReconciliation : Read` | `ChargeReconciliationQuery` | `ApiResponse<PagedResult<ChargeReconciliationItemResponse>>` | `EPIC RJE-06` | **Rencana (belum tersedia)** |
| `POST` | `/{itemType}/{id}/retry` | Kirim ulang | `BillingChargeReconciliation : Update` | path | `ApiResponse<ChargeReconciliationItemResponse>` | `EPIC RJE-06` | **Rencana (belum tersedia)** |
| `POST` | `/{itemType}/{id}/resolve` | Selesaikan manual | `BillingChargeReconciliation : Update` | `ResolveChargeReconciliationRequest` | `ApiResponse<ChargeReconciliationItemResponse>` | `EPIC RJE-06` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Master Data / Billing Sync Policy

Base URL: `api/v1/health-services/billing-management/master-data/billing-sync-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Baca kebijakan | `BillingSyncPolicy : Read` | — | `ApiResponse<List<BillingSyncPolicyResponse>>` | `EPIC RJE-06` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah kebijakan | `BillingSyncPolicy : Update` | `UpdateBillingSyncPolicyRequest` | `ApiResponse<BillingSyncPolicyResponse>` | `EPIC RJE-06` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/from-source` | Mencatat item dari sumber; kini menolak domain klinis Rawat Jalan | `BillingInvoice : Create` | `UpsertChargeRequest` | `ApiResponse<InvoiceDetailResponse>` | `EPIC RJE-02` | Sudah ada — diubah |

## 14. Matriks kewenangan

| Peran | Butir hak akses |
|---|---|
| Dokter | `EncounterBillingSummary : Read` |
| Petugas Billing | `EncounterBillingSummary : Read`, `BillingInvoice : Read`, `BillingChargeReconciliation : Read`, `BillingChargeReconciliation : Update` |
| Admin Billing | seluruh butir di atas + `BillingSyncPolicy : Read`, `BillingSyncPolicy : Update` |

String atribut: `[AccessPermission("<Resource>", "<Action>")]`. Sumber:
[contracts/permission-audit-matrix.md](contracts/permission-audit-matrix.md) bagian V2.

## 15. Batas integrasi dan billing

Modul Rawat Jalan **MUST NOT**: menentukan harga, subtotal, pajak, diskon, bagian penjamin atau
pasien, status bayar, settlement, maupun metode bayar; memanggil endpoint tulis Billing dari
browser; membuat invoice, payer, atau status finansial sendiri; menulis `Completed` pada kunjungan.
Billing **MUST NOT** membuat item Rp0 karena tarif hilang, dan **MUST NOT** memakai harga dari
snapshot klinis.

## 16. Guardrail regulasi

| Kewajiban | Penerapan di MVP |
|---|---|
| Kerahasiaan rekam medis | Fakta ke Billing tanpa SOAP, diagnosis, anamnesis, hasil pemeriksaan, aturan pakai obat; ringkasan dokter tanpa harga per item; log tanpa nama pasien atau isi klinis |
| Keutuhan rekam medis | Konsultasi `Completed` tidak dibuka ulang (`RJ-DOC-DEC-003`); pembatalan klinis menjadi versi baru, tidak menghapus |
| Jejak audit finansial | Setiap sinkron, void, adjustment, dan keputusan manual tercatat dengan pelaku dan waktu |
| Pengurangan tagihan | Selalu lewat adjustment dengan persetujuan manusia (`RJ-BIL-DEC-004`) |

Sign-off formal Farmasi, Finance, dan Security/Privacy (`RJ-E2E-OQ-005`) menahan aktivasi
production, bukan pengembangan.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan | Contoh |
|---|---|---|
| `NFR-001` | Atomicity per mata rantai; klinis tidak pernah batal karena Billing | Billing mati → tindakan tetap `Completed` |
| `NFR-002` | Concurrency: satu invoice per kunjungan, satu item per (sumber, versi) | Dua baris satu kunjungan diproses bersamaan → satu invoice |
| `NFR-003` | Idempotency: kunci deterministik di setiap mata rantai | Klik ganda → satu item |
| `NFR-004` | Otorisasi server-side di setiap endpoint | Panggilan manual tanpa butir → `403` |
| `NFR-005` | Audit sesuai `SEC-RJ-006` | — |
| `NFR-006` | Waktu: harga dipilih menurut `OccurredAt`, bukan waktu sinkron | Tarif naik 1 Oktober; pelayanan 30 September disinkron 2 Oktober → tarif lama |
| `NFR-007` | Kirim ulang terbatas dan terjadwal dari master | 5 percobaan, maks. jeda 1 jam |
| `NFR-008` | Validasi input server-side (UUID, jumlah, status, versi, teks, filter, pencarian maks. 100 karakter) | Pencarian 500 karakter → `422` |
| `NFR-009` | Teks tampil sebagai teks biasa | `<script>` tampil apa adanya |

## 18. Skenario UAT

Data samaran. Setiap epic `MUST HAVE` punya skenario berhasil dan gagal.

| UAT | Epic | Kondisi awal | Langkah | Hasil yang diharapkan |
|---|---|---|---|---|
| `UAT-01` | RJE-02, 03, 04 | Tn. A kunjungan Rawat Jalan kelas Umum, tarif lengkap | Nebulizer dikerjakan, resep dibuat, Selesai Konsultasi | Invoice berisi konsultasi, Nebulizer, resep `PRESCRIBED`; harga = katalog |
| `UAT-02` | RJE-02 | Tarif *Thorax PA* belum ada | Study Radiologi diterima | Tidak ada item; antrean `TARIFF_NOT_FOUND` |
| `UAT-03` | RJE-02 | — | Kirim `from-source` `PROCEDURE` Rp1 lewat Postman | `422 RJE-VAL-010` |
| `UAT-04` | RJE-02 | Invoice Tn. A sudah `FINAL` | Lab baru diterima | Adjustment `DEBIT` `SUBMITTED` |
| `UAT-05` | RJE-03 | dr. B punya rule tarif Rp150.000 | Selesai Konsultasi dua kali | Satu item `CONSULTATION` Rp150.000 |
| `UAT-06` | RJE-03 | Poli Gigi tanpa tarif konsultasi | Selesai Konsultasi | Konsultasi selesai; antrean `TARIFF_NOT_FOUND`; tidak ada item Rp0 |
| `UAT-07` | RJE-04 | Resep Rp35.000 `PRESCRIBED` | Kasir menerima Rp35.000; farmasi menelaah | Farmasi dapat mulai telaah dan menyerahkan obat |
| `UAT-08` | RJE-04 | Sama; invoice `FINAL` | Farmasi menyerahkan 8 dari 10 Vitamin C | Adjustment `CREDIT` Rp4.000 `SUBMITTED` |
| `UAT-09` | RJE-04 | Resep belum dibayar | Farmasi mencoba menyerahkan | Ditolak: pembayaran belum beres |
| `UAT-10` | RJE-05 | Lab `ACCEPTED`, invoice `OPEN` | Lab dibatalkan | Item `VOIDED`; riwayat ada |
| `UAT-11` | RJE-05 | Nebulizer `PERFORMED` | Dibatalkan karena salah pasien | Adjustment `CREDIT` `SUBMITTED`; item tetap `ACTIVE` |
| `UAT-12` | RJE-05 | Lab belum diterima | Lab dibatalkan | Tidak ada void maupun adjustment |
| `UAT-13` | RJE-06 | Database invoice diputus sementara | Tindakan dikerjakan, lalu koneksi pulih | Item muncul sekali setelah kirim ulang otomatis |
| `UAT-14` | RJE-06 | Kebijakan `INVOICE_SYNC` nonaktif | Sinkron gagal | Langsung antrean `SYNC_POLICY_INACTIVE` |
| `UAT-15` | RJE-06 | Item antrean `TARIFF_NOT_FOUND` | Tarif dilengkapi; petugas menekan Kirim Ulang | Item `Synced`, muncul di invoice |
| `UAT-16` | RJE-06 | Item antrean | Selesaikan manual tanpa alasan | `422`; item tetap di antrean |
| `UAT-17` | RJE-07 | Kunjungan baru tanpa pelayanan | Dokter membuka tab Ringkasan Billing | Kalimat "Belum ada tagihan…", bukan galat |
| `UAT-18` | RJE-07 | dr. B hanya punya `EncounterBillingSummary : Read` | Panggil `GET /billing/invoices/{id}` | `403`; tombol Buka Detail Billing tidak tampil |
| `UAT-19` | RJE-07 | Billing mati | Dokter Selesai Konsultasi | Konsultasi selesai; pemberitahuan "akan dicoba ulang otomatis" |
| `UAT-20` | RJE-08 | Kunjungan tanpa dokter | Perawat menyelesaikan skrining | Kunjungan `Billing`; tidak `Completed` |
| `UAT-21` | RJE-08 | Kunjungan butuh dokter | Perawat menyelesaikan skrining | Kunjungan `WaitingForDoctor` (tidak berubah) |
| `UAT-22` | RJE-01 | Database dengan baris folio lama | Migration dijalankan | Baris lama Rawat Jalan di antrean `LEGACY_PRE_BRIDGE`; nol item invoice baru |
| `UAT-23` | RJE-01 | Kontrak lama `1.2` | Kirim `PHARMACY`/`PRESCRIBED` | Ditolak seperti sebelumnya |
| `UAT-24` | RJE-02, 07 | Ny. D (samaran) kunjungan Rawat Jalan dengan satu asuransi | Sama dengan `UAT-01` | Invoice terbentuk otomatis; bagian penjamin dan pasien dihitung Billing; angka di tab Ringkasan Billing sama dengan `calculation-preview` |

UAT dijalankan sebagai validasi runtime lewat HTTP sungguhan terhadap `QuilvianNewDevSukma`
(pola Bank Darah, `02` bagian `V2.13`), ditambah pengamatan layar untuk UAT frontend.

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Pasien dapat berjalan sampai `ConsultationCompleted` dengan finalisasi canonical | `UAT-01`, `UAT-05` |
| Jasa konsultasi punya kontrak sumber dan tertagih | `UAT-05`, `UAT-06` |
| Tindakan, Lab, Radiologi masuk invoice | `UAT-01`, `UAT-02`, `UAT-04` |
| Obat dua tahap berjalan dan deadlock hilang | `UAT-07`, `UAT-08`, `UAT-09` |
| Satu kunjungan satu invoice; tanpa item ganda karena retry | `UAT-05`, `UAT-13`; acceptance matrix V2-1 |
| Harga dari tarif server-side | `UAT-01`, `UAT-03` |
| Biaya admin hanya dari policy `RAJAL` | Acceptance matrix V2-2 baris `RJ-E2E-DEC-002` |
| Rawat Jalan tanpa mutasi finansial | `UAT-03`, `UAT-18` |
| Pembatalan/koreksi memakai versi sumber | `UAT-10`, `UAT-11`, `UAT-12` |
| `OutcomeUnknown` punya jalur rekonsiliasi | `UAT-13`..`UAT-16` |
| Otorisasi `401`/`403` terbukti | `UAT-18`; acceptance matrix V2-5 |
| Ringkasan Billing dapat dibaca dokter | `UAT-17`, `UAT-19` |
| Kunjungan tanpa dokter tidak `Completed` | `UAT-20`, `UAT-21` |
| Master kebijakan kirim ulang terisi | Rencana data master awal (`02` bagian `V2.11`) |
| Layar antrean terjangkau dari menu | Peta butir menu (`03` bagian `V2.3`) |
| Build, model EF, QBE lulus pada setiap task | Laporan task bagian verifikasi (pola `BE-BD-022`) |
| E2E tunai, asuransi, dan retry/klik ganda lulus | `UAT-01` (tunai), `UAT-24` (asuransi), `UAT-05`, `UAT-13` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` | `EPIC RJE-01` | Blueprint dan kontrak V2 disetujui |
| `MVP-1` | `EPIC RJE-02` (tindakan, Lab, Radiologi), `EPIC RJE-08` | `MVP-0` selesai |
| `MVP-2` | `EPIC RJE-03`, `EPIC RJE-04` | `MVP-1` selesai |
| `MVP-3` | `EPIC RJE-05`, `EPIC RJE-06` (backend + layar antrean) | `MVP-2` selesai |
| `MVP-4` | `EPIC RJE-07` | Endpoint ringkasan dari `MVP-3` tersedia |
| `POST-MVP` | Seluruh bagian 8 | Di luar rilis pertama |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| `RJ-E2E-OQ-003` — kelebihan bayar obat dikembalikan tunai atau jadi kredit? | Billing/Finance | Adjustment `CREDIT` tetap terbentuk; cara uang kembali ditangani manual | Tidak |
| `RJ-E2E-OQ-004` / `RJ-E2E-DEC-004` — siapa menutup kunjungan ke `Completed`? | Registration + Billing | Kunjungan berhenti di `ConsultationCompleted`/`Billing` | Tidak |
| `RJ-E2E-OQ-005` — sign-off Farmasi, Finance, Security/Privacy | Pemilik masing-masing | Menahan aktivasi production | Tidak (pengembangan) |
| Apakah `CreateAdjustmentAsync` menerima invoice `CLOSED`? | Diverifikasi di preflight task `EPIC RJE-02`/`05` | Bila tidak, baris masuk antrean `ADJUSTMENT_REJECTED` — sudah dirancang | Tidak |

Tidak ada pertanyaan yang memblokir. Setelah pemilik menyetujui blueprint dan kontrak V2, dokumen
ini boleh diteruskan ke `plan-module-delivery`.


---

# Amendment DP — Daftar Pasien Rawat Jalan (revisi `28`, `draft`)

Bagian ini **menambah** batas rilis untuk satu fitur; isi `04-prd-to-mvp.md` di atas tidak berubah.
Semua entity, endpoint, permission, dan status di bawah sudah ada di `02-backend-architecture.md`
*Amendment DP*, `contracts/` *Amendment DP*, dan `flowcharts/daftar-pasien-rawat-jalan.md`.

## DP-1. Masalah dan tujuan

Petugas menolak pendaftaran pasien karena kunjungan lama masih aktif, tetapi tidak ada layar untuk
menutupnya. Di DB dev ada 159 kunjungan aktif sejak 24 Jun 2026. Tujuan: setiap kunjungan yang
memblokir pendaftaran dapat ditemukan dan ditutup petugas yang berwenang dari frontend.

## DP-2. Batas MVP

| Titik | Isi |
|---|---|
| Mulai | Pengguna membuka menu "Daftar Pasien Rawat Jalan" |
| Akhir | Kunjungan menggantung dibatalkan dan pasien berhasil didaftarkan ulang (`AT-DP-24`) |

## DP-3. Kemampuan `MUST HAVE`

| Kemampuan | Asal | Disposisi |
|---|---|---|
| Daftar kunjungan RJ berklinik bercakupan | `CAP-DP-01`, `02`, `03`, `04` | `EXTEND` (endpoint baru di atas data yang ada) |
| Summary per kelompok + menggantung | `CAP-DP-02` | `MISSING / NEW` |
| Hak `ReadAll`, `Read`, `Cancel` | `CAP-DP-05` | `MISSING / NEW` (pola ada) |
| Batal dengan guard status + konsultasi | `CAP-DP-06`, `08` | `EXTEND` |
| Pemblokir pendaftaran hanya RJ berklinik status < 7 | `CAP-DP-09` | `EXTEND` |
| Layar dari base component + butir menu | `CAP-DP-11`, `12`, `13` | `EXISTING / REUSE` |

## DP-4. Yang ditunda

| Kemampuan | Alasan | Pengganti selama MVP |
|---|---|---|
| Menutup kunjungan ke `Completed` | Milik Registration + Billing (`RJ-E2E-OQ-004`) | Status 7-8 tidak lagi memblokir |
| Pengetatan `PATCH /patient-encounters/{id}/cancel` | `RJ-DOC-OQ-008` | Frontend tidak memakainya |
| Pembatalan massal data lama | `RJ-DOC-OQ-011` | Batal satu per satu lewat kartu Menggantung |
| Refactor antrean memakai `ClinicalActorScopeService` | Di luar scope | Logika ganda dicatat sebagai utang teknis |

## DP-5. Epic dan functional requirement

| Epic | FR | Bunyi | Disposisi |
|---|---|---|---|
| `EP-DP-1` Backend daftar | `FR-DP-01` | Server mengembalikan hanya kunjungan RJ berklinik dalam cakupan pengguna | `EXTEND` |
| | `FR-DP-02` | Summary lima kelompok, `hanging` lintas tanggal | `MISSING / NEW` |
| | `FR-DP-03` | Metadata cakupan dan opsi filter | `MISSING / NEW` |
| | `FR-DP-04` | Butir `OutpatientEncounter : Read/Cancel/ReadAll` terdaftar | `MISSING / NEW` |
| `EP-DP-2` Backend batal & pemblokir | `FR-DP-05` | Batal status 0-5 dan 6 tanpa konsultasi aktif, dengan alasan, antrean ikut batal | `EXTEND` |
| | `FR-DP-06` | Pemblokir pendaftaran memakai `BlocksRegistration` | `EXTEND` |
| `EP-DP-3` Frontend | `FR-DP-07` | Layar `FE-RJDP-01` sesuai skema DP-FE.3 | `MISSING / NEW` |
| | `FR-DP-08` | Butir menu di bawah Skrining Pasien | `MISSING / NEW` |
| | `FR-DP-09` | Modal batal dengan tombol bersyarat | `MISSING / NEW` |

## DP-6. Skenario UAT

| Epic | Jalur berhasil | Jalur gagal |
|---|---|---|
| `EP-DP-1` | `AT-DP-01`, `03`, `05` | `AT-DP-02`, `04`, `06` |
| `EP-DP-2` | `AT-DP-07`, `08`, `15`, `17` | `AT-DP-09`..`14`, `16` |
| `EP-DP-3` | `AT-DP-23`, `24` | `AT-DP-22` |

## DP-7. Definition of Done

| No | Butir | Bukti |
|---:|---|---|
| 1 | Build Release backend `0 Error`, QBE Strict `PASS` pada berkas yang disentuh | Laporan task BE |
| 2 | `AT-DP-01`..`21` lulus runtime terhadap `QuilvianNewDevSukma` | Log runtime di laporan task |
| 3 | Endpoint lama dan akses rekam medis tidak berubah (`AT-DP-18`, `19`) | Log runtime |
| 4 | Butir hak akses muncul di layar Akses Role | Tangkapan layar / query read-only |
| 5 | Layar dan menu FE lulus `AT-DP-22`..`24`; build FE lulus | Laporan task FE |
| 6 | ENC-RSMMC-00146 dapat ditutup dan pasiennya didaftarkan ulang | `AT-DP-24` |

## DP-8. Gelombang pengiriman

| Gelombang | Isi |
|---|---|
| `MVP-0` | `EP-DP-1` + `EP-DP-2` (backend; dapat dirilis sendiri — pemblokir lebih longgar langsung membantu pendaftaran) |
| `MVP-1` | `EP-DP-3` (frontend; bergantung `MVP-0`) |
| `POST-MVP` | Butir DP-4 |

## DP-9. Pertanyaan terbuka sebelum development lock

| ID | Isi | Memblokir |
|---|---|---|
| — | Approval desain dan kontrak `RJ-DOC-ENCLIST-001@1.0.0` oleh pemilik | Ya — `plan-module-delivery` |
| `R-DP-1` | Panggilan dokter bersamaan dengan pembatalan | Tidak — risiko sisa |

---

# Amendment KT — Konsultasi Tertunda di Klinis Dokter (revisi `29`, `draft`)

Diturunkan dari `02-backend-architecture.md` *Amendment KT*, `03-frontend-architecture.md`
*Amendment KT*, `contracts/` *Amendment KT* (`RJ-DOC-PENDCONS-001@1.0.0`), dan
`flowcharts/konsultasi-tertunda.md`. Tidak ada entity baru.

## KT-1. Masalah dan tujuan

Saat pemblokir pendaftaran dihidupkan (`BlockActiveEncounter = true`), pasien dengan konsultasi
dokter yang tertinggal dari hari sebelumnya tidak dapat didaftarkan. Daftar Pasien Rawat Jalan
menyuruh petugas menghubungi dokter, tetapi Klinis Dokter hanya menampilkan antrean hari ini,
sehingga dokter tidak dapat membuka konsultasi itu. Tujuan: dokter dapat menemukan, membuka, lalu
menyelesaikan atau membatalkan konsultasinya yang tertunda dari Klinis Dokter.

## KT-2. Batas MVP

| Titik | Isi |
|---|---|
| Mulai | Dokter membuka Klinis Dokter dan punya konsultasi dari hari sebelumnya yang belum selesai |
| Akhir | Konsultasi itu selesai (kunjungan menjadi status 7), atau dibatalkan sehingga petugas dapat membatalkan kunjungannya di Daftar Pasien Rawat Jalan |

## KT-3. Kemampuan `MUST HAVE`

| ID | Kemampuan | Asal |
|---|---|---|
| `KT-CAP-01` | Daftar Konsultasi tertunda per dokter, lintas tanggal | `RJ-DOC-DEC-029`, `030`; `F-KT-1`, `F-KT-2` |
| `KT-CAP-02` | Membuka konsultasi tertunda di workspace yang sama | `RJ-DOC-DEC-031`; `F-KT-3` |
| `KT-CAP-03` | Simpan (finalisasi) konsultasi tertunda dengan konfirmasi tambahan | `RJ-DOC-DEC-031`, `RJ-DOC-FE-011`; `F-KT-4` |
| `KT-CAP-04` | Batalkan konsultasi tertunda dengan alasan | `RJ-DOC-DEC-031`; `F-KT-5` |
| `KT-CAP-05` | Petunjuk Daftar Pasien Rawat Jalan menunjuk tempat yang benar | `RJ-DOC-FE-012` |

## KT-4. Yang ditunda

| Hal | Alasan | Pengganti selama MVP |
|---|---|---|
| Tombol Batalkan konsultasi untuk antrean hari ini (`RJ-DOC-OQ-014`) | Di luar scope amendment | Konsultasi hari ini diselesaikan lewat Simpan |
| Kunjungan status 6 yang antreannya bukan `InConsultation` (`RJ-DOC-OQ-012`) | Menunggu hitungan data | Bila ada, ditangani manual oleh pemilik |
| Resep draf ikut batal saat konsultasi dibatalkan (`RJ-DOC-OQ-015`) | Perilaku lama, di luar scope | Farmasi tidak menerima resep draf karena resep draf tidak pernah diteruskan |

## KT-5. Epic dan functional requirement

**Epic KT-E1 — Konsultasi tertunda** (`MUST HAVE`)

| FR | Requirement | Disposisi |
|---|---|---|
| `KT-FR-01` | Backend mengembalikan antrean milik dokter yang memenuhi syarat `02` KT.3.1, urut tanggal paling lama | `MISSING / NEW` |
| `KT-FR-02` | Setiap baris membawa `draftPrescriptionCount`, `procedureCount`, `pendingDays`, `canCancelConsultation` | `MISSING / NEW` |
| `KT-FR-03` | Panel kiri menampilkan bagian Konsultasi tertunda beserta jumlahnya; tidak tampil bila kosong | `MISSING / NEW` |
| `KT-FR-04` | Item tertunda dapat dibuka di workspace | `EXTEND` (`useDoctorConsultationWorkspace`) |
| `KT-FR-05` | Banner tanggal tampil selama item lampau terbuka | `MISSING / NEW` |
| `KT-FR-06` | Modal Simpan meminta centang konfirmasi bila item lampau memuat resep draf atau tindakan; hitungan dibaca ulang saat modal dibuka | `EXTEND` (`FinalizeConsultationModal`) |
| `KT-FR-07` | Simpan memakai `finish-consultation` yang ada tanpa perubahan aturan | `EXISTING / REUSE` |
| `KT-FR-08` | Batalkan konsultasi dengan alasan wajib maks 250, lewat `PATCH /doctor-consultations/{id}/cancel` | `EXTEND` (endpoint ada, UI baru) |
| `KT-FR-09` | Pesan petunjuk/penolakan di Daftar Pasien Rawat Jalan memakai bunyi baru | `EXTEND` |

## KT-6. Skenario UAT

| ID | Skenario | Hasil yang diharapkan |
|---|---|---|
| `UAT-KT-01` | dr. Arif membuka Klinis Dokter pada 5 Okt; ENC-RSMMC-00172 (30 Sep) masih Sedang Konsultasi | Konsultasi tertunda (1) berisi IKBAL, tertunda 5 hari |
| `UAT-KT-02` | dr. Arif membuka item itu, melengkapi SOAP, menekan Simpan; ada 1 resep draf | Modal meminta centang "1 resep akan diteruskan ke farmasi"; sesudah dicentang dan disimpan, item hilang; kunjungan status 7; IKBAL dapat didaftarkan dengan `BlockActiveEncounter = true` |
| `UAT-KT-03` | dr. Arif membatalkan konsultasi tertunda dengan alasan "Pasien pulang sebelum diperiksa" | Item hilang; pesan meminta petugas membatalkan kunjungan; petugas berhasil membatalkannya di Daftar Pasien Rawat Jalan |
| `UAT-KT-04` (gagal) | dr. Budi membuka Klinis Dokter | Konsultasi tertunda milik dr. Arif tidak tampil |
| `UAT-KT-05` (gagal) | dr. Arif membatalkan tanpa alasan | Tombol tidak dapat ditekan / server `400` |
| `UAT-KT-06` (gagal) | Pengguna bukan penulis konsultasi mencoba Simpan atau Batalkan | Ditolak backend dengan pesan penjaga penulis |
| `UAT-KT-07` (gagal) | Daftar tertunda gagal dimuat | Antrean hari ini tetap tampil; bagian tertunda menampilkan pesan dan Coba lagi |

## KT-7. Definition of Done

| Butir | Bukti |
|---|---|
| `GET /doctor-queues/pending-consultations` sesuai kontrak `RJ-DOC-PENDCONS-001@1.0.0` | Uji runtime HTTP `AT-KT-01`..`08` |
| Antrean hari ini dan endpoint lain `DoctorQueueController` tidak berubah perilakunya | `AT-KT-09` |
| Bunyi pesan Daftar Pasien Rawat Jalan berubah, kondisi tidak | `AT-KT-10` |
| Hitungan `RJ-DOC-OQ-012` di DB uji dilaporkan | Laporan task backend |
| Layar memenuhi `UAT-KT-01`..`07` | Laporan task frontend beserta bukti runtime |
| `dotnet build` 0 error, QBE Strict lulus pada berkas yang disentuh; `npm run lint`/build frontend lulus | Laporan task |

## KT-8. Gelombang pengiriman

| Gelombang | Isi |
|---|---|
| `MVP-0` | Backend: endpoint `pending-consultations`, DTO, bunyi pesan (`KT-FR-01`, `02`, `09`) |
| `MVP-1` | Frontend: bagian tertunda, buka item, banner, konfirmasi Simpan, Batalkan konsultasi (`KT-FR-03`..`08`) |
| `POST-MVP` | `RJ-DOC-OQ-014`, `RJ-DOC-OQ-015`, penanganan hasil `RJ-DOC-OQ-012` bila perlu |

## KT-9. Pertanyaan terbuka sebelum development lock

| ID | Isi | Memblokir |
|---|---|---|
| `RJ-DOC-OQ-012` | Jumlah kunjungan status 6 dengan konsultasi aktif yang antreannya bukan `InConsultation` | Tidak — dihitung di task backend |
| `RJ-DOC-OQ-014` | Tombol Batalkan konsultasi juga untuk antrean hari ini | Tidak — `POST-MVP` |
| `RJ-DOC-OQ-015` | Resep draf saat konsultasi dibatalkan | Tidak — perilaku lama |

Tidak ada pertanyaan yang memblokir.

# Amendment MT — Menu Konsultasi Tertunda (revisi `30`, `draft`)

Diturunkan dari `03-frontend-architecture.md` *Amendment MT*, `02-backend-architecture.md`
*Amendment MT* (tanpa perubahan backend), dan `flowcharts/konsultasi-tertunda.md` bagian
*Amendment MT*. Tidak ada entity, endpoint, atau butir hak akses baru.

## MT-1. Masalah dan tujuan

Sesudah *Amendment KT*, konsultasi tertunda tampil sebagai tab di panel kiri Klinis Dokter.
Panel itu sempit, sehingga dokter sulit membandingkan banyak konsultasi tertunda sekaligus (contoh
pada gambar pemilik: Tertunda (8)). Tujuan: dokter mengelola konsultasi tertunda dari halaman
daftar tersendiri, seperti petugas mengelola kunjungan di Daftar Pasien Rawat Jalan. Klinis Dokter
dipakai untuk meninjau isi konsultasi sebelum disimpan.

## MT-2. Batas MVP

| Titik | Isi |
|---|---|
| Mulai | Dokter membuka menu Dokter → Rawat Jalan → Konsultasi Tertunda |
| Akhir | Setiap baris sudah disimpan (kunjungan status 7) atau dibatalkan, dan dokter kembali ke daftar |

## MT-3. Kemampuan `MUST HAVE`

| ID | Kemampuan | Asal |
|---|---|---|
| `MT-CAP-01` | Menu Dokter → Rawat Jalan menjadi grup Klinis Dokter dan Konsultasi Tertunda | `RJ-DOC-FE-014` |
| `MT-CAP-02` | Halaman daftar Konsultasi Tertunda dengan pencarian dan pagination | `RJ-DOC-FE-014`, `RJ-DOC-FE-016`; `KT-CAP-01` |
| `MT-CAP-03` | Aksi baris Batalkan Konsultasi dengan alasan wajib | `RJ-DOC-DEC-047`; `KT-CAP-04` |
| `MT-CAP-04` | Aksi baris Simpan Konsultasi: membuka Klinis Dokter untuk ditinjau | `RJ-DOC-DEC-046`; `KT-CAP-02`, `KT-CAP-03` |
| `MT-CAP-05` | Kembali ke daftar sesudah Simpan/Batalkan berhasil di Klinis Dokter | `RJ-DOC-DEC-048` |
| `MT-CAP-06` | Tab Tertunda dihapus dari Klinis Dokter; pengingat jumlah tertunda | `RJ-DOC-FE-014`, `RJ-DOC-FE-015` |

## MT-4. Yang ditunda

| Hal | Alasan | Pengganti selama MVP |
|---|---|---|
| Filter Dokter untuk pengguna jalur super admin (`RJ-DOC-OQ-016`) | Butuh sinyal backend "boleh melihat semua dokter" untuk endpoint ini; amendment ini frontend-only | Kolom Dokter di tabel |
| `RJ-DOC-OQ-014`, `RJ-DOC-OQ-015` | Tetap seperti *Amendment KT* | Tetap seperti *Amendment KT* |

## MT-5. Epic dan functional requirement

**Epic MT-E1 — Menu Konsultasi Tertunda** (`MUST HAVE`)

| FR | Requirement | Disposisi |
|---|---|---|
| `MT-FR-01` | Sidebar Dokter → Rawat Jalan berisi Klinis Dokter (rute lama) dan Konsultasi Tertunda (rute baru) | `EXTEND` (`menu-items.jsx`) |
| `MT-FR-02` | Halaman Konsultasi Tertunda menampilkan tabel dari `pending-consultations` dengan kolom MT-FE.3, pencarian, jumlah baris, pagination, keadaan kosong/gagal/`403` | `MISSING / NEW` |
| `MT-FR-03` | Aksi Batalkan Konsultasi di baris: modal beralasan wajib 1–250, endpoint batal existing, baris hilang dan pesan petugas tampil sesudah sukses; tidak tampil tanpa `canCancelConsultation` | `EXTEND` (`useDoctorPendingConsultations`) |
| `MT-FR-04` | Aksi Simpan Konsultasi membuka Klinis Dokter dengan parameter konsultasi; Klinis Dokter memuat dan membuka item itu tanpa membuka modal Simpan | `EXTEND` (`doctor-queue-view`, `useDoctorConsultationWorkspace`) |
| `MT-FR-05` | Konsultasi yang tidak lagi tertunda menghasilkan pesan "tidak ditemukan" dengan tautan kembali | `MISSING / NEW` |
| `MT-FR-06` | Sesudah Simpan atau Batalkan berhasil pada item dari daftar, dokter diarahkan ke daftar dengan pesan sukses; parameter URL dibersihkan sesudah item terbuka | `MISSING / NEW` |
| `MT-FR-07` | Tombol Batalkan Konsultasi di workspace untuk item dari daftar | `EXISTING / REUSE` (dari `RJ-DOC-REV-FE-012`) |
| `MT-FR-08` | Panel kiri Klinis Dokter tanpa tab; pengingat "Ada {n} konsultasi tertunda" bila `n > 0` | `EXTEND` |
| `MT-FR-09` | Banner kunjungan lampau dan konfirmasi modal Simpan tetap berlaku | `EXISTING / REUSE` |

## MT-6. Skenario UAT

| ID | Skenario | Hasil yang diharapkan |
|---|---|---|
| `UAT-MT-01` | dr. Arif membuka sidebar Dokter → Rawat Jalan | Dua butir: Klinis Dokter dan Konsultasi Tertunda. Klinis Dokter berperilaku seperti sebelumnya untuk antrean hari ini, tanpa tab |
| `UAT-MT-02` | dr. Arif punya 8 konsultasi tertunda dan membuka Klinis Dokter | Pengingat "Ada 8 konsultasi tertunda…" tampil; tautannya membuka Konsultasi Tertunda |
| `UAT-MT-03` | dr. Arif membuka Konsultasi Tertunda dan mencari "IKBAL" | Satu baris IKBAL, 30 Sep 2026, tertunda 7 hari, resep draf 1 / tindakan 2 |
| `UAT-MT-04` | dr. Arif memilih Batalkan Konsultasi pada AGNES dengan alasan "Pasien pulang sebelum diperiksa" | Baris AGNES hilang; pesan meminta petugas membatalkan kunjungannya; petugas berhasil membatalkannya di Daftar Pasien Rawat Jalan |
| `UAT-MT-05` | dr. Arif memilih Simpan Konsultasi pada IKBAL | Klinis Dokter terbuka dengan IKBAL terpilih, banner tertunda tampil, modal Simpan tidak terbuka. Sesudah meninjau dan menekan Selesaikan serta centang konfirmasi, dokter kembali ke Konsultasi Tertunda dengan pesan sukses dan baris IKBAL hilang |
| `UAT-MT-06` | Dari Klinis Dokter (dibuka lewat Simpan), dr. Arif menekan Batalkan Konsultasi | Konsultasi batal; kembali ke Konsultasi Tertunda dengan pesan petugas |
| `UAT-MT-07` (gagal) | Batalkan tanpa alasan | Tombol konfirmasi tidak dapat ditekan |
| `UAT-MT-08` (gagal) | Pengguna tanpa `DoctorConsultation : Cancel` | Aksi Batalkan tidak tampil di daftar maupun workspace |
| `UAT-MT-09` (gagal) | Baris sudah disimpan di tab lain, lalu Simpan Konsultasi ditekan di tab ini | Klinis Dokter menampilkan "Konsultasi tertunda tidak ditemukan atau sudah diselesaikan." dengan tautan kembali |
| `UAT-MT-10` (gagal) | Finalisasi ditolak validasi | Tetap di Klinis Dokter dengan pesan validasi existing; tidak diarahkan ke daftar |
| `UAT-MT-11` (gagal) | dr. Budi membuka Konsultasi Tertunda | Konsultasi milik dr. Arif tidak tampil |
| `UAT-MT-12` (gagal) | Muat ulang Klinis Dokter sesudah konsultasi dari daftar disimpan | Konsultasi itu tidak terbuka ulang; parameter URL sudah dibersihkan |

## MT-7. Definition of Done

| Butir | Bukti |
|---|---|
| `MT-FR-01`..`09` terpenuhi | Laporan task `RJ-DOC-REV-FE-015` |
| `UAT-MT-01`..`12` lulus | Uji layar Playwright terhadap FE dev + backend dev (pola `runtime-ui-test-setup`), dicatat per skenario |
| `npm run lint` pada berkas yang disentuh dan `next build` lulus | Keluaran perintah di laporan task |
| Tidak ada perubahan backend | `git diff` backend kosong untuk task ini |
| Antrean hari ini tidak berubah perilaku | `UAT-MT-01` dan regresi `UAT-KT-02` lewat jalur baru |

## MT-8. Gelombang pengiriman

| Gelombang | Isi |
|---|---|
| `MVP-0` | `RJ-DOC-REV-FE-015` — seluruh `MT-FR-01`..`09` dalam satu task frontend (satu vertical slice; tanpa dependency backend) |
| `POST-MVP` | `RJ-DOC-OQ-016` filter Dokter |

## MT-9. Pertanyaan terbuka

| ID | Pertanyaan | Memblokir? |
|---|---|---|
| `RJ-DOC-OQ-016` | Perlukah filter Dokter untuk pengguna jalur super admin? Bila ya, backend perlu sinyal cakupan pada `pending-consultations` | Tidak — `POST-MVP` |

# Amendment PM-B — PRD → MVP Pendaftaran Rujukan (revisi `31`, `approved` 2026-10-08)

## PM-1. Batas MVP

Mulai: petugas atau pasien Kiosk memilih Jenis Kunjungan *Rujukan*, atau petugas menambah penjamin
asuransi baru dengan scan. Selesai: kunjungan rujukan ke poli atau Laboratorium terbentuk dengan
rincian dan surat tersimpan privat, rujukan yang belum lengkap terlihat dan dapat dilengkapi, dan
penjamin baru yang tidak cocok dengan kartunya ditolak.

## PM-2. Kemampuan

| Kemampuan | Prioritas | Asal | Disposisi |
|---|---|---|---|
| Master Institusi & Dokter Perujuk + tanda mitra | MUST | `F-PM-5`, `F-PM-11` | `EXTEND` |
| Rincian rujukan + surat privat + koreksi | MUST | `F-PM-5`, `F-PM-13` | `MISSING / NEW` |
| Step Data Rujukan (petugas), unit tujuan poli/Lab, popup jadwal | MUST | `RJ-DOC-REV-FE-013/017` | `EXTEND` |
| Rujukan belum lengkap di Daftar Kunjungan RJ | MUST | `RJ-DOC-REV-FE-0xx` Daftar Pasien RJ | `EXTEND` |
| Step Data Rujukan Kiosk + scan surat + popup jadwal | MUST | `F-PM-10` | `EXTEND` |
| Pencocokan scan kartu asuransi | MUST (backend + FE siap; aktif saat agent siap) | `RJ-DOC-REV-FE-014` | `EXTEND` |
| OCR kartu asuransi di agent | — | `RJ-DOC-OQ-PM-01` | `OPEN DECISION` (di luar repository) |
| Rujukan Radiologi | Ditunda | `RJ-DOC-OQ-PM-02` | `OPEN DECISION` — pengganti: opsi nonaktif "belum tersedia" |
| Data uji 3+ dokter per poli | MUST (uji) | `RJ-DOC-DEC-079` | Data, bukan kode |

## PM-3. Epic dan functional requirement

| Epic | FR | Uraian | Disposisi |
|---|---|---|---|
| E-PM-1 Master perujuk | FR-PM-01 | CRUD standar Institusi Perujuk dengan `IsPartner`; badge Mitra | `EXTEND` |
| | FR-PM-02 | CRUD standar Dokter Perujuk per institusi | `EXTEND` |
| | FR-PM-03 | `options` + `kiosk/options` membawa `isPartner` | `EXTEND` |
| E-PM-2 Rincian rujukan | FR-PM-04 | Create kunjungan admin/Kiosk menerima rujukan, satu transaksi | `EXTEND` |
| | FR-PM-05 | `GET/PUT …/referral` dengan hitung `IsComplete`, penguncian status, unit tujuan tetap, `RowVersion`, revisi | `NEW` |
| | FR-PM-06 | Berkas privat: unggah (admin/Kiosk), unduh berizin, hapus lunak | `NEW` |
| | FR-PM-07 | `referralStatus` di list kunjungan RJ + filter | `EXTEND` |
| E-PM-3 Layar petugas | FR-PM-08 | Step Data Rujukan, bar step dinamis, Data Kunjungan tanpa poli untuk rujukan | `EXTEND` |
| | FR-PM-09 | Unit tujuan poli/Lab (Lab lewat registrasi lab + `PUT`), Radiologi nonaktif | `EXTEND` |
| | FR-PM-10 | Popup jadwal praktik poli | `NEW` (komponen view) |
| | FR-PM-11 | Badge + filter + form Lengkapi/Koreksi di Daftar Kunjungan RJ | `EXTEND` |
| E-PM-4 Kiosk | FR-PM-12 | Step Data Rujukan Kiosk + scan surat + unggah sesudah kunjungan | `EXTEND` |
| | FR-PM-13 | Popup jadwal di Layanan & Dokter | `EXTEND` |
| E-PM-5 Scan asuransi | FR-PM-14 | Validasi `cardScan` di backend (admin + Kiosk) | `EXTEND` |
| | FR-PM-15 | Pencocokan di layar + alert "Data tidak match", aktif bila agent mengembalikan nilai | `EXTEND` |

## PM-4. Skenario UAT

| Epic | Berhasil | Gagal |
|---|---|---|
| E-PM-1 | Tambah *Klinik Sehat Sentosa* bermitra; muncul badge Mitra dan di pilihan pendaftaran | Kode ganda ditolak; hapus institusi yang sudah dipakai ditolak |
| E-PM-2 | Kunjungan rujukan lengkap tersimpan; `GET` menampilkan surat; koreksi alasan tercatat di riwayat | Koreksi sesudah konsultasi dimulai ditolak; ubah unit tujuan ditolak; `/uploads/...referral...` tidak dapat dibuka |
| E-PM-3 | `RJ-AC-PM-04`..`06`, `08`; rujukan Lab hari ini terbentuk | Radiologi nonaktif (`RJ-AC-PM-07`); unggah gagal → rujukan belum lengkap dan coba lagi berhasil |
| E-PM-4 | `RJ-AC-PM-09`; scan surat 2 halaman tersimpan | Scanner gagal → tiket tetap tercetak dengan pesan |
| E-PM-5 | `RJ-AC-PM-01`, `03` (dengan respons agent tiruan) | `RJ-AC-PM-02` |

## PM-5. Definition of Done

| Butir | Bukti |
|---|---|
| Migration PM-B diterapkan ke `QuilvianNewDevSukma` tanpa perubahan model tertunda | Log EF |
| `RJ-AC-PM-01`..`10` lulus (runtime HTTP + uji layar) | Laporan task |
| Berkas surat tidak dapat diakses lewat URL publik | Runtime HTTP |
| Pendaftaran Umum, IGD, dan Kiosk Umum tidak berubah | Uji layar regresi |
| Data uji `PMTEST` tersedia (1 poli ≥ 4 dokter) | Query baca-saja |
| Roadmap dan traceability diperbarui | Berkas roadmap |

## PM-6. Gelombang pengiriman

| Gelombang | Isi |
|---|---|
| `MVP-0` | Backend: migration + master perujuk (E-PM-1), rincian rujukan + berkas + list (E-PM-2), validasi `cardScan` (FR-PM-14); data uji |
| `MVP-1` | Frontend: master perujuk, step Data Rujukan petugas + popup jadwal, Daftar Kunjungan RJ, pencocokan scan di layar |
| `MVP-2` | Kiosk: step Data Rujukan + scan surat + popup jadwal |
| `POST-MVP` | Rujukan Radiologi (`RJ-DOC-OQ-PM-02`); aktivasi pencocokan nyata saat agent siap (`RJ-DOC-OQ-PM-01`) |

## PM-7. Pertanyaan terbuka

| ID | Pertanyaan | Memblokir |
|---|---|---|
| `RJ-DOC-OQ-PM-01` | Kontrak OCR kartu asuransi di agent | Tidak memblokir MVP; pencocokan nyata menunggu |
| `RJ-DOC-OQ-PM-02` | Registrasi Radiologi | `POST-MVP` |
