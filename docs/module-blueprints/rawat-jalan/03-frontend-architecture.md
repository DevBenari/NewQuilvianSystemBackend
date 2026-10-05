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
