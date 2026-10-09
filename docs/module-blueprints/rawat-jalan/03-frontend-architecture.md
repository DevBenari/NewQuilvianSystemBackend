# Arsitektur Frontend — Rawat Jalan Billing

> **Revisi `27` (28 September 2026):** amendment **V2 — Rawat Jalan ke invoice canonical** ada di bagian akhir berkas ini dan **berlaku** untuk jalur fakta klinis → tagihan. Isi di atasnya adalah riwayat desain `RJ-BIL`.

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revision `11` |
| Status | `draft` |
| Backend contract evidence | Working tree Billing Operational; source commit `9b26be3...` |
| Frontend source evidence | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| Domain architecture | revision `1`, core internal/manual independen dari partial external adapter |

Dokumen ini menentukan kontrak perilaku frontend. Ia tidak mengunci route, layout, warna,
sidebar, tab, atau komponen visual yang belum diberi UI authority.

> **Dokumen ini adalah arsitektur layar Billing.** Ia `DOWNSTREAM` dan **bukan** Definition of
> Done developer Dokter / Rawat Jalan.
>
> Workspace dokter — antrean, konsultasi, anamnesis, vital, diagnosis, SOAP/CPPT, tab resep, tab
> tindakan, surat keterangan, autosave, dan tombol `Selesai Konsultasi` — **tidak dirancang di
> sini**. Kontrak perilakunya ada pada
> [roadmap/doctor-consultation-roadmap.md](roadmap/doctor-consultation-roadmap.md).
>
> Satu aturan yang berlaku untuk kedua scope dan tidak boleh dilanggar layar mana pun: **frontend
> tidak pernah menjadi sumber kebenaran finansial**. Layar klinis membaca status finansial sebagai
> `REFERENCE` saja, dan layar Billing membaca angka kanonik dari Billing — keduanya tidak
> menetapkannya.

## 1. Kebutuhan fungsional

| Kemampuan | Pengguna | Data yang dikonsumsi | Aksi utama |
|---|---|---|---|
| Melihat folio berdasarkan encounter | Billing, cashier, authorized clinical read | Folio status, charge line, component, processing outcome | Buka detail, refresh, lihat alasan review |
| Menampilkan milestone processing | Billing/integration operator | Outcome, replay, version conflict, error code, correlation | Retry terkontrol, buka reconciliation case |
| Menampilkan allocation | Billing/payer/cashier | Payer allocation, patient responsibility, residual | Lihat versi, jangan overwrite histori |
| Mengajukan financial action | Billing maker | Charge state, approval policy, reason, amount | Submit request; tidak langsung mengubah state |
| Memproses approval | Checker berwenang | Request, maker, impact, policy version | Approve/reject/return; self-approval ditolak |
| Melihat projection di Pharmacy | Pharmacy | Status/version/reference dari Billing/Payer | Baca saja; urgent exception melalui workflow |

## 2. Data dan status frontend

Frontend wajib membedakan:

- clinical order/fulfillment;
- milestone processing (`Received`, `InProgress`, `OutcomeUnknown`, `PendingReconciliation`);
- charge calculation (`PendingFinancialReview`, `Recognized`, `Superseded`, `Voided`, `Reversed`);
- allocation/patient responsibility;
- payment/claim settlement;
- financial action approval.

`OutcomeUnknown` ditampilkan sebagai “hasil pemrosesan belum dapat dipastikan”, bukan gagal dan
bukan berhasil. `PendingFinancialReview` ditampilkan sebagai “menunggu tinjauan finansial”.

## 3. State handling

| State UI | Perilaku |
|---|---|
| Loading | Tampilkan indikator tanpa menghapus data terakhir yang masih relevan |
| Empty | Jelaskan folio belum terbentuk; jangan membuat folio dari frontend |
| Error 400 | Tampilkan koreksi input |
| Error 401/403 | Tampilkan akses tidak tersedia; jangan menyembunyikan sebagai empty |
| Error 404 | Tampilkan encounter/folio tidak ditemukan |
| Error 409 | Tampilkan konflik versi/outcome dan tombol rekonsiliasi/reload terkontrol |
| Timeout/network | Pertahankan request identity; jangan auto-submit dengan idempotency key baru |
| Stale response | Tolak response versi lebih lama menimpa state yang lebih baru |

## 4. Aksi per peran dan permission

| Peran/capability | Boleh | Tidak boleh |
|---|---|---|
| Billing reader | Baca folio dan histori | Mengubah charge atau approval |
| Billing maker | Ajukan allocation/action | Menyetujui request sendiri |
| Billing checker | Approve/reject/return | Mengubah clinical fact |
| Payer operator | Catat decision/manual claim | Menetapkan Paid langsung pada Pharmacy |
| Cashier | Collection/receipt/refund execution setelah approval | Mengubah order klinis |
| Pharmacy | Clinical fulfillment dan baca projection | Menulis canonical financial status |
| Clinical user | Mengirim clinical fact/correction request | Void/refund/waiver financial |

## 5. Duplicate submit, cache, dan invalidation

1. Tombol submit milestone/financial action dinonaktifkan setelah request diterima.
2. Client memakai idempotency key yang stabil untuk satu operasi dan tidak menggantinya saat
   timeout sebelum status query selesai.
3. Query folio di-invalidate setelah response canonical berhasil, replay, atau version conflict.
4. Response versi lama tidak boleh menimpa allocation/projection versi baru.
5. Refresh adalah operasi read-only dan tidak mengulang mutation.

## 6. Privacy, accessibility, dan responsive behavior

Data klinis sensitif tidak masuk custom logger dan hanya ditampilkan sesuai permission. UI harus
memakai label manusia, status text, reason, dan waktu; tidak menjadikan UUID sebagai satu-satunya
informasi. Kontras, keyboard navigation, focus/error announcement, dan tampilan layar kecil
menjadi persyaratan engineering standar. Detail visual final tetap `DEV_DISCRETION` sampai UI
authority ditetapkan.

## 7. UI yang sengaja belum dikunci

Route final, lokasi menu, susunan sidebar, bentuk modal/drawer, warna status, dan component
library tidak ditentukan oleh dokumen ini. Implementer frontend harus mengusulkan opsi dan
menunggu authority UI yang sah. Adapter payer eksternal tidak boleh memiliki tombol aktivasi
produksi sebelum `RJ-BIL-DEP-009` selesai.


---

# Amendment V2 — Ringkasan Billing dan Antrean Rekonsiliasi (revisi blueprint `27`)

| Field | Nilai |
|---|---|
| Status | `approved` — Sukma Giri, 2026-09-28 |
| Kontrak | `RJ-E2E-CONTRACT-001@1.0.0` |
| Frontend SHA | `83b8b72744d4afaaedb3d2af9dd0b83fb272f9d6` (`sukmagpV2`) |
| Keputusan | `RJ-E2E-DEC-008`, `009`; `RJ-E2E-FE-001`..`004` |
| Stack | Next.js App Router, JavaScript/JSX, Redux, Axios, base component Quilvian |

> Bagian di atas garis adalah desain layar folio `RJ-BIL` revisi `11`. Layar folio `RJ-BIL-FE-001`/`002`
> **tidak ditemukan** di frontend `83b8b72` (capability map V2 bagian `6`). Amendment ini tidak
> menghidupkannya kembali.

## V2.1 Hierarki kewenangan

```text
security/privacy/invariant   → tanpa harga per item, tanpa aksi finansial, akses dijaga server
  → approved product/UI brief → PRD §8, §17, §19; RJ-E2E-FE-001, RJ-E2E-FE-002
  → project convention        → base component Quilvian, usePermission, AccessDeniedGate
  → DEV_DISCRETION            → RJ-E2E-FE-003, RJ-E2E-FE-004
```

## V2.2 Kebutuhan layar

| ID | Layar | Pengguna | Jalan masuk | Status |
|---|---|---|---|---|
| `FE-RJE-01` | Tab **Ringkasan Billing** di workspace konsultasi dokter | Dokter, perawat poli | Layar anak dari `doctor-queues` (tab baru) | Baru |
| `FE-RJE-02` | Pemberitahuan masalah penyerahan tagihan saat Selesai Konsultasi | Dokter | Layar anak dari alur Selesai Konsultasi yang sudah ada | Diperbarui |
| `FE-RJE-03` | **Antrean Rekonsiliasi Tagihan Klinis** | Petugas Billing | Butir menu baru di grup *Billing dan Kasir* | Baru |

Master kebijakan kirim ulang **tidak** mendapat layar pada MVP; admin mengubahnya lewat endpoint
yang terjaga hak akses dan ter-audit. Layarnya `POST-MVP`.

## V2.3 Peta butir menu

```text
Billing dan Kasir                                   <- grup yang sudah ada (menu-items.jsx)
├── Invoice & Billing Kasir                         (sudah ada)
├── ...                                             (sudah ada)
└── Rekonsiliasi Tagihan Klinis                     -> /health-services/billing-management/billing/charge-reconciliations   [Baru]

Registration / Doctor Queues                        (sudah ada)
└── /health-services/registration-management/doctor-queues
    └── tab "Ringkasan Billing"                     [Baru — layar anak, tanpa butir menu]
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Rekonsiliasi Tagihan Klinis | anak grup | Billing dan Kasir | `/health-services/billing-management/billing/charge-reconciliations` | `FE-RJE-03` | `BillingChargeReconciliation : Read` | Baru |
| — (tab, bukan menu) | — | Layar `doctor-queues` | — | `FE-RJE-01` | `EncounterBillingSummary : Read` | Baru |
| — (bagian alur finalisasi) | — | Modal Selesai Konsultasi yang ada | — | `FE-RJE-02` | tidak ada butir baru | Diperbarui |

Berkas yang disunting saat implementasi: `src/utils/menu-sidebar/menu-items.jsx` (butir `FE-RJE-03`)
dan `src/lib/constants/health-services/registration-management/doctor-queue/doctor-queue.constants.js`
(entri `DOCTOR_QUEUE_TABS` baru). Nama butir dan urutan `DEV_DISCRETION`.

## V2.4 Skema fitur

### `FE-RJE-01` — Tab Ringkasan Billing

```text
+- Ringkasan Billing --------------------------------------------- FE-RJE-01 -+
| Status Tagihan [OPEN]   No. INV-2026-000812   Diperbarui 10.42  [Muat ulang] |
| Penjamin: Tunai                                                              |
+------------------------------------------------------------------------------+
| Total Pelayanan 4 | Total Tagihan Rp360.000 | Ditanggung Penjamin Rp0 |      |
| Tanggungan Pasien Rp360.000                                                  |
| (!) 1 pelayanan sedang diproses ke tagihan · 0 perlu rekonsiliasi            |
+------------------------------------------------------------------------------+
| Pelayanan              | Jml | Status Pelayanan | Status Tagihan             |
| Konsultasi Sp.PD       |  1  | Selesai          | Tercatat                   |
| Nebulizer              |  1  | Dikerjakan       | Tercatat                   |
| Resep R/0912           |  1  | Diresepkan       | Tercatat                   |
| Darah Lengkap          |  1  | Diterima Lab     | Menunggu                   |
+------------------------------------------------------------------------------+
|                                                    [Buka Detail Billing]     |
+------------------------------------------------------------------------------+
| memuat  -> kerangka; data lama yang masih sah tetap tampil saat muat ulang   |
| kosong  -> "Belum ada tagihan untuk kunjungan ini. Tagihan terbentuk otomatis |
|             setelah pelayanan pertama tercatat."                             |
| gagal   -> "Ringkasan tagihan gagal dimuat."                   [Coba lagi]   |
| 403     -> tab tidak ditampilkan                                             |
+------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | Status tagihan, nomor invoice, penjamin, waktu diperbarui | `GET /encounter-billing-summaries/{encounterId}` → `BillingStatus`, `InvoiceNumber`, `PaymentSourceLabel`, `LastUpdatedAt` | `EncounterBillingSummary : Read` | `NO_INVOICE` → kalimat kosong di atas (bukan galat) |
| Angka | Total pelayanan, total tagihan, bagian penjamin, bagian pasien | `ServiceCount`, `GrossAmount`, `PayerAmount`, `PatientAmount` — **tampil apa adanya, tanpa dijumlah ulang** (`AC-RJ-009`) | sama | `NO_INVOICE` → angka tidak ditampilkan |
| Pemberitahuan sinkron | Jumlah pelayanan yang masih diproses dan yang perlu rekonsiliasi | `PendingSyncCount`, `ReconciliationCount` | sama | Keduanya 0 → wilayah disembunyikan |
| Daftar pelayanan | Nama, jumlah, status pelayanan, status tagihan | `Services[]` (**tanpa harga**) | sama | Kosong → "Belum ada pelayanan tercatat." |
| Tombol Muat ulang | Membaca ulang ringkasan | endpoint yang sama | sama | Gagal → pesan gagal + Coba lagi; data sebelumnya tetap tampil |
| Tombol Buka Detail Billing | Pindah ke detail invoice di modul Billing | route `.../billing/invoices/{token}/detail-billing` | `BillingInvoice : Read` | Tanpa butir → tombol **disembunyikan**; server tetap menolak `403` bila dipanggil langsung |

Label `BillingState`: `RECORDED` → *Tercatat*, `PENDING` → *Menunggu*, `RECONCILIATION` → *Perlu
dicek Billing*, `NOT_BILLED` → *Tidak ditagihkan*. **Tidak ada** tombol Bayar, Diskon, Void,
Finalisasi, maupun Settlement (PRD §19).

### `FE-RJE-02` — Pemberitahuan penyerahan tagihan

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Pemberitahuan setelah Selesai Konsultasi | Daftar masalah penyerahan, mis. "Resep R/0912: penyerahan ke tagihan akan dicoba ulang otomatis." | `BillingHandoffIssues` pada respons `PATCH /doctor-consultations/{id}/complete` (sudah ada, belum dibaca frontend) | tidak ada butir baru | Daftar kosong → tidak ada pemberitahuan |

Konsultasi yang sudah selesai **tidak boleh** tampak gagal karena pemberitahuan ini. Bentuknya
(toast, panel, atau baris di modal) `DEV_DISCRETION` (`RJ-E2E-FE-004`).

### `FE-RJE-03` — Antrean Rekonsiliasi Tagihan Klinis

```text
+- Rekonsiliasi Tagihan Klinis ----------------------------------- FE-RJE-03 -+
| [cari no. kunjungan / invoice] [Status v] [Jenis pelayanan v] [Sebab v] [Tgl]|
+------------------------------------------------------------------------------+
| Kunjungan | Pelayanan | Versi | Sebab            | Percobaan | Sejak | Aksi  |
| RJ-...812 | PHARMACY  |  2    | Tarif belum ada  | 0         | 09.10 | [...] |
+------------------------------------------------------------------------------+
| memuat -> kerangka baris                                                     |
| kosong -> "Tidak ada tagihan yang menunggu penanganan."   [Atur ulang]       |
| gagal  -> "Daftar gagal dimuat."                          [Coba lagi]        |
+- Halaman 1 dari n ----------------------------- [< Sebelumnya] [Berikutnya >]+

Aksi per baris: [Kirim Ulang]  [Selesaikan Manual -> jenis + alasan min. 10 karakter]
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Saringan | Status, jenis pelayanan, sebab, tanggal, pencarian (maks. 100 karakter) | query `ChargeReconciliationQuery` | `BillingChargeReconciliation : Read` | — |
| Tabel | Kunjungan, pelayanan, versi, sebab (terbaca), percobaan, sejak kapan, nomor invoice | `GET /charge-reconciliations` | sama | Kalimat kosong / gagal di atas |
| Kirim Ulang | Mengirim ulang item | `POST /{itemType}/{id}/retry` | `BillingChargeReconciliation : Update` | `409` → "Item sedang diproses sistem. Muat ulang beberapa saat lagi."; `422` → pesan dari server |
| Selesaikan Manual | Jenis penyelesaian + alasan | `POST /{itemType}/{id}/resolve` | sama | `422` alasan kurang → pesan di bawah isian; peringatan "Jangan menulis diagnosis atau isi klinis." |

Tombol aksi disembunyikan bagi pengguna tanpa `BillingChargeReconciliation : Update`. Tombol
dinonaktifkan selama permintaan berjalan (klik ganda). Kirim ulang **tidak** membuat identitas baru
di sisi mana pun.

## V2.5 Aksi per peran

Diturunkan dari [contracts/permission-audit-matrix.md](contracts/permission-audit-matrix.md) bagian
V2-3.

| Peran | `FE-RJE-01` | `FE-RJE-02` | `FE-RJE-03` |
|---|---|---|---|
| Dokter | Lihat ringkasan | Lihat pemberitahuan | Tidak terlihat |
| Petugas Billing | Lihat ringkasan + Buka Detail Billing | — | Lihat, Kirim Ulang, Selesaikan Manual |
| Kasir | Lihat bila diberi butir ringkasan | — | Lihat saja bila diberi `Read` |

## V2.6 Penanganan keadaan

| Keadaan | `FE-RJE-01` | `FE-RJE-03` |
|---|---|---|
| Memuat | Kerangka; saat muat ulang, data lama yang sah tetap tampil (PRD §22) | Kerangka baris (`DataTable`) |
| Kosong | `NO_INVOICE` = keadaan normal | Kalimat kosong + Atur ulang |
| `401` | Alur sesi berakhir yang sudah ada | sama |
| `403` | Tab tidak ditampilkan | `AccessDeniedGate` |
| `404` | "Kunjungan tidak ditemukan." | Item hilang → muat ulang daftar |
| `409` | — | "Item sedang diproses sistem…" / sudah diselesaikan; muat ulang. **Tidak** mengirim otomatis dengan kunci baru |
| `422` | Pesan kalkulasi dari Billing | Pesan validasi dari server |
| Data basi | Tombol Muat ulang; tidak ada polling otomatis | Muat ulang setelah setiap aksi |
| Pengiriman ganda | — (baca saja) | Tombol nonaktif selama permintaan berjalan |
| Timeout jaringan | Pesan gagal + Coba lagi | Pesan gagal; status item dibaca ulang sebelum mencoba lagi |

Seluruh teks pelayanan dan pesan dari server ditampilkan sebagai teks biasa; `dangerouslySetInnerHTML`
dilarang (`SEC-RJ-004`).

## V2.7 Berkas frontend yang terlibat

| Berkas | Status |
|---|---|
| `src/lib/services/health-services/billing-management/encounter-billing-summary.service.js` | Baru (pola `patient-billing-summary.service.js`) |
| `src/lib/services/health-services/billing-management/charge-reconciliation.service.js` | Baru |
| `src/components/view/health-services/registration-management/doctor-queues/tabs/billing-summary/doctor-billing-summary-tab.jsx` | Baru |
| `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | Diperbarui (render tab baru) |
| `src/lib/constants/health-services/registration-management/doctor-queue/doctor-queue.constants.js` | Diperbarui |
| `src/components/features/health-services/doctor-queue-features/FinalizeConsultationModal.jsx` atau hook finalisasi | Diperbarui (`FE-RJE-02`) |
| `src/app/health-services/billing-management/billing/charge-reconciliations/page.jsx` + view | Baru |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui |

Kandidat reuse: `nursing-billing-section.jsx` (ringkasan rawat inap) sebagai rujukan pola tampilan,
`DataTable`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `StatusBadge`, `BaseButton`,
`AccessDeniedGate`, `ToastStack`. Keputusan reuse atau buat baru per elemen menjadi bagian task
frontend.

## V2.8 Kewenangan UI

| Decision ID | Area | Status |
|---|---|---|
| `RJ-E2E-FE-001` | Ringkasan Billing sebagai tab di workspace dokter | `approved` |
| `RJ-E2E-FE-002` | Isi minimum panel = PRD §17, angka dari API | `approved` |
| `RJ-E2E-FE-003` | Tata letak, urutan field, gaya | `DEV_DISCRETION` |
| `RJ-E2E-FE-004` | Bentuk pemberitahuan penyerahan | `DEV_DISCRETION` |
| — | Tata letak `FE-RJE-03`, nama dan urutan butir menu | `DEV_DISCRETION` di dalam skema V2.4 |


---

# Amendment DP — Daftar Pasien Rawat Jalan (revisi `28`, `draft`)

Masukan: `RJ-DOC-DEC-011`..`023`, `RJ-DOC-FE-005`..`009`; backend [02-backend-architecture.md](02-backend-architecture.md)
*Amendment DP*; kontrak `RJ-DOC-ENCLIST-001@1.0.0` (`draft`). Snapshot FE `b7e9b7fd4`.

## DP-FE.1 Kebutuhan layar

| ID layar | Layar | Pengguna | Tujuan |
|---|---|---|---|
| `FE-RJDP-01` | Daftar Pasien Rawat Jalan | Dokter, perawat poli, petugas pendaftaran, Super Admin | Melihat kunjungan RJ dalam cakupan, statusnya, dan membatalkan kunjungan yang menggantung |

Pembatalan memakai modal konfirmasi di layar yang sama (`RJ-DOC-FE-008`); tidak ada layar anak.

## DP-FE.2 Peta butir menu

```text
Rawat Jalan                               <- tingkat 0 (sudah ada), field `subMenu`
├── Skrining Pasien                       -> /health-services/registration-management/nurse-station-queue   (sudah ada)
└── Daftar Pasien Rawat Jalan             -> /health-services/registration-management/outpatient-encounters  (Baru)
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses (`requiredPermission`) | Status |
|---|:---:|---|---|---|---|---|
| Daftar Pasien Rawat Jalan | 1 | Rawat Jalan (`healthServicesRegistrationManagement`) | `/health-services/registration-management/outpatient-encounters` | `FE-RJDP-01` | `{ resource: "OutpatientEncounter", action: "Read" }` | Baru |

Disisipkan tepat setelah "Skrining Pasien" di `src/utils/menu-sidebar/menu-items.jsx`
(`RJ-DOC-FE-005`). Entri lama "Daftar Kunjungan" yang dikomentari (`patient-encounters`) tidak
dihidupkan; route itu tidak ada. Pendaftaran butir menu menjadi acceptance criteria task FE
layar ini.

## DP-FE.3 Skema fitur `FE-RJDP-01`

```text
+- Hero: Daftar Pasien Rawat Jalan -------------------------------------------------+
| cakupan: "Pasien Anda" / "Klinik cluster Anda" / "Semua klinik"                    |
+-----------------------------------------------------------------------------------+
| [Menunggu] [Sedang Konsultasi] [Siap Ditagih] [Batal/Tidak Hadir] [Menggantung!]  |  <- summary card, klik = filter
+-----------------------------------------------------------------------------------+
| (Hari ini | Aktif semua tanggal)  [Tanggal] [Status v] [Klinik v] [Dokter v*]      |  <- base filter; *hanya ReadAll
| [cari no. RM / nama / no. kunjungan]                                 [Atur ulang]  |
+-----------------------------------------------------------------------------------+
| No. Kunjungan | Tanggal | Pasien / No. RM | Klinik | Dokter | Penjamin | Status | |
| ENC-...00146  | 30 Jul  | ...             | ...    | ...    | Tunai    | chip   |[⋮]|  <- aksi: Batalkan
+-----------------------------------------------------------------------------------+
| memuat -> kerangka baris                                                          |
| kosong -> "Tidak ada kunjungan pada saringan ini."              [Atur ulang]      |
| gagal  -> "Daftar kunjungan gagal dimuat."                      [Coba lagi]       |
| 403    -> "Akun Anda belum terhubung ke data dokter atau cluster perawat. ..."    |
+- Halaman 1 dari n ---------------------------------- [< Sebelumnya] [Berikutnya >]+

Modal Batalkan Kunjungan:
  ENC-RSMMC-00146 · <nama pasien> · Poli ... · status Menunggu Perawat
  Alasan pembatalan* [______________________] 0/250
  [Kembali]  [Batalkan Kunjungan]
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Hero | Judul dan label cakupan | `GET /outpatient-encounters/filters/metadata` (`scope`) | `OutpatientEncounter : Read` | `403` → seluruh layar diganti pesan cakupan (`AccessDeniedGate`/`InformationAlert`) |
| Summary card | Jumlah per kelompok: Menunggu (0-5), Sedang Konsultasi (6), Siap Ditagih (7-8), Batal/Tidak Hadir, **Menggantung** (aktif, status < 7, tanggal sebelum hari ini) | `GET /outpatient-encounters/summary` (saringan yang sama, tanpa saringan status) | `Read` | Gagal → kartu menampilkan "–" dan tabel tetap dimuat |
| Filter | Mode Hari ini / Aktif semua tanggal; tanggal; status; klinik (opsi dari metadata); dokter (hanya bila `scope.canReadAll`); pencarian | `filters/metadata` | `Read` | Opsi gagal dimuat → filter tetap dapat dipakai tanpa daftar opsi |
| Tabel | Kolom pada `OutpatientEncounterListItem` | `GET /outpatient-encounters` | `Read` | Lihat skema |
| Aksi Batalkan | Menu baris; tampil hanya bila `item.canCancel = true` **dan** `usePermission("OutpatientEncounter", "Cancel")` | — | `OutpatientEncounter : Cancel` | Baris status 6 dengan konsultasi aktif menampilkan `item.cancelBlockedReason` sebagai keterangan, bukan tombol |
| Modal batal | Ringkasan kunjungan, alasan wajib 1-250 karakter | `PATCH /outpatient-encounters/{id}/cancel` | `Cancel` | `400`/`404` → pesan dari server di modal, modal tetap terbuka; berhasil → toast, tabel dan summary dimuat ulang |

Default saat layar dibuka: mode **Hari ini** (`RJ-DOC-FE-007`). Klik kartu **Menggantung** →
mode Aktif semua tanggal + saringan `hangingOnly=true`.

## DP-FE.4 Aksi per peran

| Peran | Lihat | Saring dokter | Batalkan |
|---|---|---|---|
| Dokter (`Read`) | Pasien sendiri | Tidak | Hanya bila diberi `Cancel` |
| Perawat poli (`Read`, `Cancel`) | Klinik clusternya | Tidak | Ya, status yang diizinkan |
| Petugas pendaftaran (`Read`, `ReadAll`, `Cancel`) | Semua | Ya | Ya |

## DP-FE.5 Penanganan keadaan

| Keadaan | Perilaku |
|---|---|
| Kirim ganda | Tombol "Batalkan Kunjungan" nonaktif selama permintaan berjalan |
| Data basi | Server menolak `400` "Kunjungan sudah dibatalkan." atau "sedang dalam konsultasi" → pesan di modal, lalu tabel dimuat ulang saat modal ditutup |
| Ganti saringan | Halaman kembali ke 1; summary ikut dimuat ulang |
| Sesi habis | Pola `InstanceAxios` yang sudah ada |
| Privasi | Nama pasien dan no. RM tidak ditulis ke `console` maupun `localStorage` |

## DP-FE.6 Berkas frontend yang terlibat

Mengikuti pola layar registrasi yang ada (hook + service, bukan Redux slice baru) dan pola
pemakaian base component di `administrator-bank-view.jsx`.

| Berkas | Status |
|---|---|
| `src/app/health-services/registration-management/outpatient-encounters/page.jsx` | Baru |
| `src/components/view/health-services/registration-management/outpatient-encounters/outpatient-encounter-list-client.jsx` | Baru |
| `src/components/view/health-services/registration-management/outpatient-encounters/outpatient-encounter-list-view.jsx` | Baru |
| `src/components/view/health-services/registration-management/outpatient-encounters/outpatient-encounter-table-columns.jsx` | Baru |
| `src/components/view/health-services/registration-management/outpatient-encounters/cancel-outpatient-encounter-modal.jsx` | Baru |
| `src/lib/hooks/health-services/registration-management/outpatient-encounters/use-outpatient-encounter-list.jsx` | Baru |
| `src/lib/services/health-services/registration-management/outpatient-encounter.service.js` | Baru |
| `src/lib/constants/health-services/registration-management/outpatient-encounter-constants.js` | Baru — label status dan kelompok summary |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui — satu butir menu |
| Base component `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `RowActionMenu`, `ConfirmModal`, `AccessDeniedGate`, `ToastStack`, `FilterDatePicker`, `FilterSelect`, `Pagination` | Sudah ada, dipakai ulang tanpa diubah |

## DP-FE.7 Kewenangan UI

| Decision ID | Area | Status |
|---|---|---|
| `RJ-DOC-FE-005` | Letak menu di bawah Skrining Pasien | `approved` |
| `RJ-DOC-FE-006` | Hero, summary card, base filter, base table dari base component | `approved` |
| `RJ-DOC-FE-007` | Default hari ini + Aktif semua tanggal + kartu Menggantung | `approved` |
| `RJ-DOC-FE-008` | Modal batal, alasan wajib, tombol bersyarat | `approved` |
| `RJ-DOC-FE-009` | Kolom, urutan, isi filter, gaya | `DEV_DISCRETION` di dalam skema DP-FE.3 |

---

# Amendment KT — Konsultasi Tertunda di Klinis Dokter (revisi `29`, `draft`)

Keputusan `RJ-DOC-DEC-028`..`031`, `RJ-DOC-FE-010`..`012`; backend `02-backend-architecture.md`
*Amendment KT*; kontrak `RJ-DOC-PENDCONS-001@1.0.0` (`draft`). Snapshot FE `d232feb2b`.

## KT-FE.1 Kebutuhan layar

Tidak ada layar baru. Perubahan berada di layar yang sudah ada:

| ID | Layar | Perubahan |
|---|---|---|
| `FE-RJKT-01` | Klinis Dokter — Rawat Jalan (`/health-services/registration-management/doctor-queues`) | Bagian Konsultasi tertunda di panel kiri; banner kunjungan lampau; konfirmasi tambahan di modal Simpan; tombol dan modal Batalkan konsultasi untuk konsultasi tertunda |
| `FE-RJDP-01` | Daftar Pasien Rawat Jalan | Tidak ada perubahan kode. Petunjuk baris berasal dari backend (`02` KT.3.4) |

## KT-FE.2 Peta butir menu

Tidak ada butir menu baru. `FE-RJKT-01` tetap dijangkau lewat butir yang sudah ada:

| Butir | Tingkat | Induk | Route | Layar | Hak akses |
|---|---|---|---|---|---|
| Rawat Jalan (`healthServicesDoctorQueueOutpatient`) | 2 | Dokter | `/health-services/registration-management/doctor-queues` | `FE-RJKT-01` | `DoctorQueue : Read` |

## KT-FE.3 Skema fitur `FE-RJKT-01`

```text
┌─ Klinis Dokter ─────────────────────────────── [ringkasan antrean hari ini] ─┐
├──────────────────────────┬────────────────────────────────────────────────────┤
│ Pasien Dokter            │ (A) ⚠ Kunjungan tanggal 30 Sep 2026 — tertunda     │
│ 0 pasien hari ini        │     5 hari. Pasien mungkin sudah pulang.           │
│ [kartu antrean hari ini] │ [tab klinis seperti biasa]                         │
│                          │                                                    │
│ (B) Konsultasi tertunda 1│                                                    │
│ ┌──────────────────────┐ │                                                    │
│ │ IKBAL YULIYANTO      │ │                                                    │
│ │ 00-00-00-15 · Poli   │ │                                                    │
│ │ Anak · 30 Sep 2026   │ │                                                    │
│ │ 5 hari   [Buka]      │ │ (C) [Batalkan konsultasi]   [Simpan konsultasi]    │
│ └──────────────────────┘ │                                                    │
└──────────────────────────┴────────────────────────────────────────────────────┘
(D) Modal Simpan: tambahan kotak centang konfirmasi bila kunjungan lampau dan ada resep/tindakan
(E) Modal Batalkan konsultasi: alasan wajib, maks 250 karakter
```

| Wilayah | Isi | Sumber data | Hak akses | Keadaan kosong / gagal |
|---|---|---|---|---|
| (B) Konsultasi tertunda | Judul dan jumlah; kartu berisi nama, no. RM, poli, tanggal antrean, lama tertunda (`pendingDays`), tombol Buka. Urut paling lama di atas | `GET /doctor-queues/pending-consultations` | `DoctorQueue : Read` | Bila kosong, seluruh bagian (B) **tidak ditampilkan**. Bila gagal: "Konsultasi tertunda gagal dimuat." beserta tombol Coba lagi; antrean hari ini tetap tampil |
| (A) Banner | "Kunjungan tanggal {tanggal antrean} — tertunda {n} hari. Pasien mungkin sudah pulang. Pastikan resep dan tindakan masih diperlukan sebelum menyimpan." | Item terpilih (`queueDate`, `pendingDays`) | — | Hanya tampil bila `queueDate` sebelum hari ini |
| (C) Batalkan konsultasi | Tombol, hanya untuk item dari (B) | `PATCH /doctor-consultations/{consultationId}/cancel` | Tombol tampil bila field `canCancelConsultation` bernilai `true` (pola `canCancel` Daftar Pasien Rawat Jalan); server tetap menolak `403` | Gagal: pesan server ditampilkan di modal (E) |
| (D) Modal Simpan | Bila item lampau **dan** `draftPrescriptionCount + procedureCount > 0`: kotak centang "Saya memahami kunjungan ini tanggal {tanggal}. {x} resep akan diteruskan ke farmasi dan {y} tindakan akan ditagihkan." Tombol Simpan nonaktif sampai dicentang. Hitungan dibaca ulang saat modal dibuka | `GET /doctor-queues/pending-consultations?queueId={id}` | Sama dengan Simpan hari ini | Bila baca ulang gagal: kotak centang tetap wajib, kalimat tanpa angka: "Resep dan tindakan yang ada akan diteruskan." |
| (E) Modal Batalkan | Nama pasien, tanggal, kolom alasan (wajib, maks 250), tombol Batalkan konsultasi | — | — | Sesudah berhasil: pesan "Konsultasi dibatalkan. Minta petugas membatalkan kunjungan {no. kunjungan} di Daftar Pasien Rawat Jalan." |

## KT-FE.4 Aksi per peran

| Peran | Lihat (B) | Buka | Simpan | Batalkan konsultasi |
|---|:---:|:---:|:---:|:---:|
| Dokter penulis konsultasi | Ya, miliknya | Ya | Ya | Ya |
| Dokter lain | Tidak | — | — | — |
| Pengguna jalur super admin existing | Ya, semua dokter (perilaku `GET /doctor-queues`) | Ya | Ditolak backend bila bukan penulis | Ditolak backend bila bukan penulis |

## KT-FE.5 Penanganan keadaan

| Keadaan | Perilaku |
|---|---|
| Memuat | (B) menampilkan keadaan memuat sendiri; antrean hari ini tidak menunggu (B) |
| Item aktif | `activeItem` dicari dari antrean hari ini **dan** daftar (B). Sekarang hanya dari antrean hari ini (`useDoctorConsultationWorkspace.js:72-77`), sehingga item (B) tidak dapat dibuka tanpa perubahan ini |
| Sesudah Simpan atau Batalkan | Muat ulang (B) dan ringkasan; tutup workspace bila item hilang dari (B) |
| Event realtime antrean | Event yang sudah memicu muat ulang antrean hari ini juga memuat ulang (B), dengan debounce yang sama |
| Klik ganda | Tombol Simpan dan Batalkan nonaktif selama permintaan berjalan |
| Data berubah di tab lain | `409` dari finalisasi ditampilkan seperti sekarang; (B) dimuat ulang |
| Tombol hari ini | Tombol Panggil, Lewati, Tidak Hadir **tidak** tampil pada kartu (B); kartu (B) hanya punya Buka |

## KT-FE.6 Berkas frontend yang terlibat

| Berkas | Status | Perubahan |
|---|---|---|
| `src/lib/services/health-services/registration-management/doctor-queue.service.js` | Diperbarui | `getDoctorPendingConsultations(params)` |
| `src/lib/hooks/health-services/registration-management/doctor-queue/use-doctor-queue.js` | Diperbarui | State, muat, dan muat ulang daftar tertunda; ikut event realtime |
| `src/lib/hooks/health-services/registration-management/doctor-queue/useDoctorConsultationWorkspace.js` | Diperbarui | `activeItem` dari gabungan dua daftar; alur Batalkan konsultasi; baca ulang hitungan saat modal Simpan dibuka |
| `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | Diperbarui | Wilayah (A), (B), (C), (E) |
| `src/components/features/health-services/doctor-queue-features/FinalizeConsultationModal.jsx` | Diperbarui | Wilayah (D) |
| `src/lib/services/health-services/clinical-management/doctor-consultation.service.js` | Sudah ada | `cancelDoctorConsultation` dipakai pertama kali |
| Komponen kartu tertunda dan modal batal | Baru atau pakai ulang | `DEV_DISCRETION`, wajib base component Quilvian (`RJ-DOC-FE-006`) |

## KT-FE.7 Kewenangan UI

| Hal | Wewenang |
|---|---|
| Letak (B) di panel kiri, terpisah dari antrean hari ini, dengan jumlah | `RJ-DOC-FE-010` (`approved`) |
| Bunyi banner (A) dan syarat konfirmasi (D) | `RJ-DOC-FE-011` (`approved`) |
| Tab atau bagian terpisah untuk (B), gaya kartu, ikon, warna | `DEV_DISCRETION` |
| Letak tombol (C) di workspace | `DEV_DISCRETION`, tidak boleh berdempetan dengan tombol Simpan tanpa jarak yang jelas |

# Amendment MT — Menu Konsultasi Tertunda (revisi `30`, `draft`)

Keputusan `RJ-DOC-DEC-045`..`049`, `RJ-DOC-FE-014`..`016` (decision log *Amendment Pass
2026-10-07*). Menggantikan `RJ-DOC-FE-010` dan `RJ-DOC-FE-013` pada *Amendment KT* di atas.
Snapshot FE `9acc42027` (`sukmagpV2`). Kontrak `RJ-DOC-PENDCONS-001@1.0.0` dipakai **apa adanya**;
tidak ada endpoint baru dan tidak ada perubahan backend (`02` *Amendment MT*).

**Ringkasnya:** daftar konsultasi tertunda pindah dari panel kiri Klinis Dokter ke halaman
tersendiri. Halaman itu berpola Daftar Pasien Rawat Jalan. Klinis Dokter hanya dipakai untuk
meninjau dan menyimpan satu konsultasi tertunda yang dibuka dari halaman itu.

## MT-FE.1 Kebutuhan layar

| ID | Layar | Status | Perubahan |
|---|---|---|---|
| `FE-RJMT-01` | **Konsultasi Tertunda** (rute baru, lihat MT-FE.2) | **Baru** | Tabel konsultasi tertunda, pencarian, jumlah baris, aksi baris *Simpan Konsultasi* dan *Batalkan Konsultasi* |
| `FE-RJKT-01` | Klinis Dokter — Rawat Jalan (`/health-services/registration-management/doctor-queues`) | Diperbarui | Tab *Hari ini/Tertunda* dihapus; pengingat jumlah tertunda (`RJ-DOC-FE-015`); menerima satu konsultasi tertunda lewat parameter URL dan membukanya langsung; kembali ke `FE-RJMT-01` sesudah Simpan/Batalkan berhasil |
| `FE-RJDP-01` | Daftar Pasien Rawat Jalan | Tidak berubah | Petunjuk baris dari backend tetap benar (`F-MT-5`) |

## MT-FE.2 Peta butir menu

| Butir | Key | Tingkat | Induk | Route | Layar | Hak akses |
|---|---|---|---|---|---|---|
| Rawat Jalan | `healthServicesDoctorQueueOutpatient` | 2 | Dokter | — (**berubah menjadi grup**, tidak lagi bertautan) | — | — |
| Klinis Dokter | `healthServicesDoctorQueueOutpatientClinical` | 3 | Dokter → Rawat Jalan | `/health-services/registration-management/doctor-queues` (rute lama) | `FE-RJKT-01` | `DoctorQueue : Read` |
| Konsultasi Tertunda | `healthServicesDoctorQueueOutpatientPending` | 3 | Dokter → Rawat Jalan | `/health-services/registration-management/doctor-pending-consultations` | `FE-RJMT-01` | `DoctorQueue : Read` |

Route `FE-RJMT-01` dibuat **sejajar**, bukan anak `doctor-queues/...`, supaya pencocokan butir
aktif di sidebar tidak menyalakan Klinis Dokter dan Konsultasi Tertunda bersamaan. Nama route dan
key: `DEV_DISCRETION` (`RJ-DOC-FE-016`). Butir `Rawat Inap` di bawah Dokter tidak berubah.

## MT-FE.3 Skema fitur `FE-RJMT-01` — Konsultasi Tertunda

```text
┌─ Hero: Rawat Jalan · Konsultasi Tertunda ───────────────────────────────────────┐
│ "Konsultasi dokter dari hari sebelumnya yang belum disimpan atau dibatalkan."   │
├─ (A) DataFilter ────────────────────────────────────────────────────────────────┤
│ [Cari no. kunjungan, no. RM, nama pasien...]          [Jumlah baris ▾] [Reset]│
├─ (B) InformationAlert sukses / galat ───────────────────────────────────────────┤
├─ (C) DataTable ─────────────────────────────────────────────────────────────────┤
│ No │ Tanggal │ Tertunda │ No. Kunjungan │ Pasien/No. RM │ Klinik │ Dokter │      │
│    │         │          │               │               │        │        │ Resep│
│    │         │          │               │               │        │        │ draf/│
│    │         │          │               │               │        │        │ Tind.│ Aksi ⋮
│ 1  │30 Sep 26│ 7 hari   │ENC-RSMMC-00172│IKBAL Y. / 15  │ Anak   │dr. Arif│ 1 / 2│ (D)
├─ Pagination ────────────────────────────────────────────────────────────────────┤
(D) RowActionMenu: [Simpan Konsultasi] [Batalkan Konsultasi]**
(E) ConfirmModal Batalkan Konsultasi: ringkasan + alasan wajib (maks 250)
 ** hanya bila canCancelConsultation = true
```

| Wilayah | Isi | Sumber data | Hak akses | Keadaan kosong / gagal |
|---|---|---|---|---|
| (A) Filter | Pencarian (debounce, sama dengan Daftar Pasien Rawat Jalan; mencakup kode antrean, pasien, no. RM, no. kunjungan, poli, ruang) dan jumlah baris (10/25/50). Reset mengembalikan bawaan. **Tanpa filter Dokter** — lihat MT-FE.9 | `search`, `pageSize` pada `GET /doctor-queues/pending-consultations` | `DoctorQueue : Read` | — |
| (B) Pesan | Sukses sesudah batal atau sesudah kembali dari Simpan; galat muat | Hasil aksi / query `?result=` dari Klinis Dokter | — | — |
| (C) Tabel | Kolom: No, Tanggal (`queueDate`), Tertunda (`pendingDays` hari), No. Kunjungan, Pasien / No. RM, Klinik, Dokter, Resep draf / Tindakan (`draftPrescriptionCount` / `procedureCount`), Aksi. Urut paling lama di atas (urutan server) | `GET /doctor-queues/pending-consultations?pageNumber&pageSize&search&doctorId` | `DoctorQueue : Read` | Kosong: "Tidak ada konsultasi tertunda. Semua konsultasi hari sebelumnya sudah disimpan atau dibatalkan." Gagal: pesan server atau "Konsultasi tertunda gagal dimuat." dengan tombol Coba lagi |
| (D) *Simpan Konsultasi* | Membuka Klinis Dokter dengan konsultasi itu (MT-FE.4) | — | `DoctorQueue : Read`. Server tetap menegakkan penjaga penulis saat finalisasi | — |
| (D) *Batalkan Konsultasi* | Membuka (E) | — | Tampil bila `canCancelConsultation = true` (`DoctorConsultation : Cancel`). Server tetap menolak `403` | — |
| (E) Modal batal | "Konsultasi {nama} tanggal {tanggal} akan dibatalkan. Sesudahnya, petugas membatalkan kunjungannya di Daftar Pasien Rawat Jalan." Alasan wajib, 1–250 karakter. Tombol nonaktif selama permintaan berjalan | `PATCH /doctor-consultations/{consultationId}/cancel` dengan `{ cancelReason }` | Sama dengan (D) | Gagal: pesan server di dalam modal; modal tetap terbuka. Sukses: baris hilang, tabel dimuat ulang, pesan "Konsultasi dibatalkan. Minta petugas membatalkan kunjungan {no. kunjungan} di Daftar Pasien Rawat Jalan." |

## MT-FE.4 Perubahan `FE-RJKT-01` — Klinis Dokter

```text
┌─ Klinis Dokter ─────────────────────────────── [ringkasan antrean hari ini] ─┐
├──────────────────────────┬────────────────────────────────────────────────────┤
│ (F) ⓘ Ada 8 konsultasi   │ (G) ⚠ Kunjungan tanggal 30 Sep 2026 — tertunda     │
│   tertunda. [Buka]       │     7 hari. Pasien mungkin sudah pulang.           │
│ Pasien Dokter            │ [tab klinis seperti biasa]                         │
│ 1 pasien hari ini        │                                                    │
│ [kartu antrean hari ini] │ (H) [Batalkan Konsultasi]   [Selesaikan/Simpan]    │
└──────────────────────────┴────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Keadaan kosong / gagal |
|---|---|---|---|
| Panel kiri | Kembali seperti sebelum `RJ-DOC-REV-FE-012`: judul "Pasien Dokter" dan antrean hari ini, **tanpa** `ClinicalTabNav` | `GET /doctor-queues` (tidak berubah) | Tidak berubah |
| (F) Pengingat | "Ada {n} konsultasi tertunda dari hari sebelumnya." dengan tautan ke `FE-RJMT-01`. Letak: `DEV_DISCRETION` (`RJ-DOC-FE-015`) | `totalData` dari `GET /doctor-queues/pending-consultations?pageSize=1`, dimuat ulang mengikuti muat ulang antrean hari ini (debounce yang sama dengan *Amendment KT*) | `n = 0` atau gagal: (F) **tidak tampil**, tanpa pesan galat |
| Parameter URL | `?queueId={id}`, nama parameter `DEV_DISCRETION`. Klinis Dokter memuat satu baris lewat `GET /doctor-queues/pending-consultations?queueId={id}&pageSize=1`, menambahkannya ke daftar item workspace, lalu membukanya dengan jalur `handleStart` existing (antrean `InConsultation` langsung membuka panel tanpa memanggil start) | `RJ-DOC-PENDCONS-001` | Bila kosong (sudah disimpan/dibatalkan, bukan milik dokter, atau id tidak valid): pesan "Konsultasi tertunda tidak ditemukan atau sudah diselesaikan." dengan tautan kembali ke `FE-RJMT-01`; workspace tetap kosong. Gagal jaringan: pesan server dan tautan yang sama |
| (G) Banner | Tidak berubah (`RJ-DOC-FE-011` a) | Item terpilih | — |
| Modal Simpan | Tidak berubah (`RJ-DOC-FE-011` b); **tidak** terbuka otomatis (`RJ-DOC-DEC-046`) | — | — |
| (H) Batalkan Konsultasi | Hanya untuk item yang dibuka lewat parameter URL dan `canCancelConsultation = true` (`RJ-DOC-DEC-047`). Modal yang sama dengan (E) | `PATCH /doctor-consultations/{id}/cancel` | Gagal: pesan di modal |
| Sesudah sukses | Simpan atau Batalkan berhasil pada item dari parameter URL: arahkan ke `FE-RJMT-01` dengan penanda hasil (mis. `?result=saved` / `?result=cancelled&encounter={no}`) agar (B) menampilkan pesan (`RJ-DOC-DEC-048`). Simpan pada antrean hari ini tetap di Klinis Dokter seperti sekarang | — | Simpan gagal: tetap di Klinis Dokter dengan galat finalisasi existing |

Parameter URL dibersihkan dari alamat sesudah item terbuka (`router.replace`), supaya muat ulang
halaman tidak membuka ulang konsultasi yang sudah disimpan dan tombol Kembali browser tidak
mengulang aksi.

## MT-FE.5 Aksi per peran

| Peran | Lihat `FE-RJMT-01` | Filter Dokter | Simpan Konsultasi | Batalkan Konsultasi |
|---|:---:|:---:|:---:|:---:|
| Dokter penulis konsultasi | Ya, miliknya | Tidak | Ya | Ya, bila punya `DoctorConsultation : Cancel` |
| Dokter lain | Tidak melihat baris dokter lain | Tidak | — | — |
| Pengguna jalur super admin existing | Ya, semua dokter; kolom Dokter membedakannya | Tidak (MT-FE.9) | Tombol tampil; ditolak backend bila bukan penulis | Tampil bila punya izin; ditolak backend bila bukan penulis |
| Tanpa data dokter dan bukan super admin | `403` → `AccessDeniedGate` | — | — | — |

## MT-FE.6 Penanganan keadaan

| Keadaan | Perilaku |
|---|---|
| Memuat | `DataTable` menampilkan "Mengambil data konsultasi tertunda..." |
| Respons lama datang belakangan | Dibuang; hanya permintaan terbaru yang mengisi state (pola `useDoctorPendingConsultations`) |
| Klik ganda Batalkan | Tombol modal nonaktif selama permintaan berjalan |
| Baris sudah diproses di tab lain | Batal: pesan server di modal, tabel dimuat ulang. Simpan: Klinis Dokter menampilkan pesan "tidak ditemukan" (MT-FE.4) |
| `403` daftar | `AccessDeniedGate`, sama dengan Daftar Pasien Rawat Jalan |
| Layar sempit | `DataTable` bergulir horizontal seperti Daftar Pasien Rawat Jalan |

## MT-FE.7 Berkas frontend yang terlibat

| Berkas | Status | Perubahan |
|---|---|---|
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui | Butir Dokter → Rawat Jalan menjadi grup dua butir (MT-FE.2) |
| `src/app/health-services/registration-management/doctor-pending-consultations/page.jsx` | Baru | Halaman `FE-RJMT-01`, `metadata.title = "Konsultasi Tertunda"` |
| `src/components/view/health-services/registration-management/doctor-pending-consultations/*` | Baru | Client (`Suspense`), view, dan kolom tabel — pola `outpatient-encounters/*` |
| `src/lib/hooks/health-services/registration-management/doctor-queue/useDoctorPendingConsultations.js` | Diperbarui | Mendukung filter (`search`, `doctorId`), pagination, dan mode hitung saja (`pageSize=1`) untuk pengingat (F); logika batal tetap di sini |
| `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | Diperbarui | Hapus tab dan daftar tertunda di panel kiri; tambah (F); buka item dari parameter URL; tombol (H); arahkan ke `FE-RJMT-01` sesudah sukses |
| `src/lib/services/health-services/registration-management/doctor-queue.service.js` | Sudah ada | `getDoctorPendingConsultations` dipakai ulang |
| `src/lib/services/health-services/clinical-management/doctor-consultation.service.js` | Sudah ada | `cancelDoctorConsultation` dipakai ulang |
| `src/utils/.../doctor-queue/doctor-pending-consultation-utils.js` | Sudah ada | Dipakai ulang; tambahan formatter kolom bila perlu |
| `src/components/features/health-services/doctor-queue-features/QueuePatientCard.jsx` | Diperbarui bila perlu | Cabang `pendingMode` tidak lagi dipakai; dihapus bila tidak ada pemakai lain |
| Base component | Sudah ada | `Hero`, `DataFilter`, `FilterSelect`, `DataTable`, `RowActionMenu`, `ConfirmModal`, `InformationAlert`, `AccessDeniedGate`, `Pagination` |

## MT-FE.8 Kewenangan UI

| Hal | Wewenang |
|---|---|
| Susunan menu Dokter → Rawat Jalan → Klinis Dokter / Konsultasi Tertunda | `RJ-DOC-FE-014` (`approved`) |
| Pengingat jumlah tertunda di Klinis Dokter | `RJ-DOC-FE-015` (`approved`); letak `DEV_DISCRETION` |
| Simpan = tinjau di Klinis Dokter, tanpa modal otomatis | `RJ-DOC-DEC-046` (`approved`) |
| Batalkan di daftar dan di workspace | `RJ-DOC-DEC-047` (`approved`) |
| Kembali ke daftar sesudah sukses | `RJ-DOC-DEC-048` (`approved`) |
| Kolom, filter, route, key menu, nama parameter URL, ikon | `DEV_DISCRETION` (`RJ-DOC-FE-016`) dalam pola Daftar Pasien Rawat Jalan |

## MT-FE.9 Yang sengaja tidak dibuat

| Hal | Alasan | Pengganti |
|---|---|---|
| Filter Dokter untuk pengguna jalur super admin | Frontend tidak punya sinyal "boleh melihat semua dokter" untuk endpoint ini. Metadata Daftar Pasien Rawat Jalan (`scope.canReadAll`, `doctorOptions`) dijaga `OutpatientEncounter : Read` yang belum tentu dimiliki dokter. Menambah sinyal itu berarti mengubah backend, di luar amendment frontend-only ini | Kolom Dokter di tabel. Bila dibutuhkan, diputuskan terpisah sebagai `RJ-DOC-OQ-016` (`POST-MVP`) |
| Summary card | `RJ-DOC-FE-016` | Jumlah baris di judul tabel/pagination |
| Tombol Panggil/Lewati/Tidak Hadir | Konsultasi tertunda sudah `InConsultation` | — |

# Amendment PM-B — Frontend Pendaftaran Rujukan (revisi `31`, `approved` 2026-10-08)

Keputusan `RJ-DOC-DEC-068`..`082`, `KSK-DEC-025`. Kontrak `RJ-DOC-REFERRAL-001@1.0.0` (`draft`).
Hierarki wewenang UI: keamanan/privasi → keputusan pemilik di atas → konvensi proyek (base
component, token, pola master data `hr/master-data/job-level`) → `DEV_DISCRETION`.

## PM-FE.1 Peta butir menu

| Butir menu | Tingkat | Induk | Route | Layar | Hak akses |
|---|---|---|---|---|---|
| Institusi Perujuk | 3 | Pelayanan Kesehatan → Master Data | `/health-services/master-data/referral-institutions` (+ `create`, `[slug]`, `[slug]/update`) | Master Institusi Perujuk | `ReferralInstitution : Read` |
| Dokter Perujuk | 3 | Pelayanan Kesehatan → Master Data | `/health-services/master-data/referral-doctors` (+ `create`, `[slug]`, `[slug]/update`) | Master Dokter Perujuk | `ReferralDoctor : Read` |
| — (layar anak) | — | Pendaftaran Pasien Rawat Jalan | sama | Step *Data Rujukan* | `PatientEncounter : Create` |
| — (layar anak) | — | Daftar Kunjungan RJ (`/health-services/registration-management/outpatient-encounters`) | sama | Form *Lengkapi / Koreksi Rujukan* (aksi baris) | `PatientEncounter : Update` |
| — (layar anak) | — | Kiosk Pasien Lama / Baru | sama | Step *Data Rujukan* Kiosk | Akun perangkat Kiosk |

Letak butir di dalam grup Master Data mengikuti urutan alfabet grup yang ada (`DEV_DISCRETION`).

## PM-FE.2 Pendaftaran Rawat Jalan (petugas) — alur step

| Jenis Kunjungan | Step |
|---|---|
| Umum | Cari/Input Pasien → Data Kunjungan (tanggal, poli, jadwal dokter, jenis, keluhan) → Pembayaran → General Consent → Verifikasi → Selesai (**tidak berubah**) |
| Rujukan | Cari/Input Pasien → Data Kunjungan (tanggal, jenis, keluhan; **tanpa** poli/dokter) → **Data Rujukan** → Pembayaran → General Consent → Verifikasi → Selesai |

Bar step berganti saat Jenis Kunjungan diubah. Mengubah Rujukan → Umum mengosongkan isian rujukan
sesudah konfirmasi (`ConfirmModal`).

### Skema step *Data Rujukan*

```text
┌ LANGKAH 3 · Data Rujukan ───────────────────────────────────── [Kembali] ┐
│ A. Identitas Rujukan                                                      │
│  [No. Rujukan *]                 [Tanggal & Jam Rujukan *  08 Okt 09:10]  │
│  [Fasilitas Perujuk * (cari)  ▾] [Dokter Perujuk (cari) ▾]                │
│  ⓘ Fasilitas Perujuk Bermitra dengan Rumah Sakit   (bila mitra)           │
│ B. Tujuan & Alasan Rujukan                                                │
│  [Unit Tujuan * (cari) ▾  Poli… / Laboratorium / Radiologi (nonaktif)]    │
│  (poli) [Jadwal Dokter * (cari) ▾]   [Lihat Jadwal Praktik] (bila kosong) │
│  (lab)  [Unit Laboratorium * ▾]                                           │
│  [Diagnosa ICD-10 * (cari) ▾]  [Catatan diagnosa]                         │
│  [Alasan Rujukan * (teks)]                                                │
│ C. Dokumen                                                                │
│  [Unggah Surat Rujukan *]  surat.jpg 412 KB [Hapus]                       │
│                                              [Lanjut ke Pembayaran]       │
└───────────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Kosong / gagal |
|---|---|---|---|
| Fasilitas perujuk | Pilihan dapat dicari; alert mitra `EmergencyInlineAlert tone="info"` | `GET /referral-institutions/options` (`isPartner`) | "Fasilitas tidak ditemukan" / "Gagal memuat fasilitas perujuk. Coba lagi" |
| Dokter perujuk | Disaring institusi | `GET /referral-doctors/options?referralInstitutionId=` | "Belum ada dokter perujuk untuk fasilitas ini" |
| Unit tujuan | Seluruh poli (bukan hanya yang buka) + Laboratorium + Radiologi nonaktif | `GET /clinics/admin/options`, `GET /service-units/options` (lab) | — |
| Jadwal dokter | Sama dengan `RJ-DOC-REV-FE-017` | `GET /doctor-schedules/admin` (sudah dimuat) | Poli tanpa jadwal pada tanggal itu: alert + tombol *Lihat Jadwal Praktik* |
| Popup jadwal | Jadwal praktik seluruh hari poli itu: dokter, hari, jam, sesi, ruang | Data jadwal yang sama, tanpa saring tanggal | "Poli ini belum punya jadwal dokter aktif" |
| Diagnosa | Pilihan ICD-10 dapat dicari (server-side) | `GET /diagnoses/options` | "Diagnosa tidak ditemukan" |
| Dokumen | Unggah 1–10 berkas, pratinjau nama/ukuran | Disimpan lokal di form sampai submit | Pesan `RJ-VAL-PM-12`/`13` |

Submit (Verifikasi): poli → `POST /patient-encounters/admin` dengan blok `referral`, lalu
`POST …/referral/documents`. Lab → `POST /lab-patient-registrations/external-referral` →
`PUT …/referral` → unggah. Kegagalan sesudah kunjungan terbentuk tidak membatalkan kunjungan:
layar Selesai menampilkan "Rincian/surat rujukan belum tersimpan" + *Coba lagi*, dan kunjungan
tampil "Rujukan belum lengkap".

### Scan kartu asuransi (Pembayaran → *Daftarkan Penjamin Baru*)

Perluasan `RJ-DOC-REV-FE-014`. Bila agent mengembalikan nama asuransi dan No. polis, layar
mencocokkannya (aturan sama dengan backend). Tidak cocok → `EmergencyInlineAlert tone="error"`
"Data tidak match", tombol Simpan nonaktif, *Scan Ulang* tersedia. Cocok → `cardScan` ikut
dikirim. Agent belum mendukung → perilaku lama (`RJ-DOC-DEC-070`). Penjamin tersimpan tidak
menampilkan scan wajib.

## PM-FE.3 Daftar Kunjungan RJ — Rujukan belum lengkap

| Wilayah | Isi | Sumber | Hak akses |
|---|---|---|---|
| Kolom/badge | `StatusBadge` "Rujukan belum lengkap" (warning) bila `referralStatus = Incomplete` | `GET /outpatient-encounters` | `PatientEncounter : Read` |
| Filter | Status rujukan: Semua / Lengkap / Belum lengkap | Query `referralStatus` | — |
| Aksi baris | *Lengkapi Rujukan* / *Koreksi Rujukan* (bila rujukan, tidak terkunci) | `GET/PUT …/referral`, berkas | `PatientEncounter : Update` |
| Form | Wilayah A, B (unit tujuan **baca saja**), C; unduh surat lewat endpoint berizin | idem | Terkunci → baca saja + "Konsultasi dokter sudah dimulai" |

Bentuk form (modal, drawer, atau halaman anak) mengikuti pola aksi baris yang sudah dipakai layar
itu (`DEV_DISCRETION`).

## PM-FE.4 Kiosk (Pasien Lama dan Baru)

Jenis Kunjungan tetap step sendiri (`KSK-DEC-025`). Untuk **Pasien Rujukan**, step baru
*Data Rujukan* disisipkan sesudah Pembayaran dan sebelum *Layanan & Dokter*. Di *Layanan & Dokter*
poli yang dipilih menjadi unit tujuan.

| Isian Kiosk | Bentuk |
|---|---|
| No. rujukan | Input sentuh |
| Tanggal rujukan | Default hari ini/jam sekarang, dapat diubah |
| Fasilitas perujuk | Pilihan dapat dicari (`kiosk/options`), alert mitra |
| Dokter perujuk | Opsional |
| Surat rujukan | Tombol *Scan Surat Rujukan* lewat scanner Kiosk (`/scanner/scan`), pratinjau halaman, scan ulang |
| Diagnosa, alasan | **Tidak ditampilkan** (`RJ-DOC-DEC-076`) |

Di *Layanan & Dokter*, poli tanpa dokter praktik menampilkan tombol *Lihat Jadwal Praktik* yang
membuka popup jadwal poli itu (`GET /doctor-schedules/kiosk/options`). Sesudah kunjungan dibuat,
halaman hasil scan diunggah lewat jalur Kiosk. Gagal unggah tidak menahan tiket antrean; tiket
menampilkan "Tunjukkan surat rujukan ke petugas". Bar step Kiosk Rujukan bertambah satu step.
Pasien yang memilih Laboratorium di Tujuan Layanan tetap lewat alur Laboratorium yang ada.

## PM-FE.5 Master Institusi dan Dokter Perujuk

Mengikuti standar master data generasi `hr/master-data/job-level` (satu `<FEATURE>_CONFIG`,
sembilan thunk Redux). Institusi: Kode, Nama, Alamat, Telepon, **Bermitra** (switch), Status.
Kolom list menampilkan badge "Mitra". Dokter: Institusi (pilihan), Nama Dokter, Status.

## PM-FE.6 Penanganan keadaan

| Keadaan | Perilaku |
|---|---|
| Submit ganda | Tombol submit terkunci selama request; jalur Lab memakai `IdempotencyKey` |
| Data basi saat koreksi | `409 RJ-VAL-PM-11` → "Data rujukan sudah diubah. Muat ulang." + tombol muat ulang |
| Tanpa hak | Aksi baris tidak tampil; route master → `AccessDeniedGate` |
| Privasi | Diagnosa, alasan, dan berkas tidak di-cache di `localStorage`; URL berkas tidak disimpan |
| Aksesibilitas | Alert mitra dan "Data tidak match" memakai `role="alert"`; unggah dapat dioperasikan dengan keyboard |

## PM-FE.7 Yang sengaja tidak dibuat

| Dipertimbangkan | Alasan |
|---|---|
| Registrasi Radiologi | `RJ-DOC-OQ-PM-02` |
| Diagnosa/alasan di Kiosk | `RJ-DOC-DEC-076` |
| Unit tujuan Laboratorium di Kiosk | `RJ-DOC-DEC-082` — alur Kiosk Laboratorium yang ada |
| Pindah Jenis Kunjungan Kiosk ke Layanan & Dokter | `KSK-DEC-025` |
