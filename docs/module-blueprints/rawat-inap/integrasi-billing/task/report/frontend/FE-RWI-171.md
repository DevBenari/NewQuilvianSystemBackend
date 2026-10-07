# Laporan Perubahan Frontend — `FE-RWI-171`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-171` |
| Judul | Antrean "perlu diperiksa" kasir |
| Slice | `MVP-1` / `MVP-2` — gelombang eksekusi 1 roadmap frontend Finishing |
| Roadmap | [`frontend-roadmap-finishing.md`](../../../roadmap/frontend-roadmap-finishing.md) — kartu `FE-RWI-171` |
| Trace | `FR-RWF-012`, `FR-RWF-015`, `FR-RWF-017`; `RWI-DEC-192`; `INV-RWF-08`; `AC-RWF-018` |
| Contract version | `integrasi-billing` `1.1.0` `approved` 2 Oktober 2026 (`RWI-DEC-221`); frontend 6.3 `FE-INT-05`; API 3.9 (`review-queue`, `review-resolution`, kode `BIL-REV-001`, `BIL-FIN-020`, `BIL-FIN-021`) |
| Wewenang UI | Letak daftar di halaman invoice kasir yang sudah ada `DEV_DISCRETION`; kontrak tidak menambah butir menu |
| Dependency | `BE-RWI-155` [BE] 🟡 — endpoint antrean dan penyelesaian tersedia, contoh layanan nyata belum UAT |
| Klasifikasi | `MEDIUM` — skor 6: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang ada (1), hak akses terkait (1), satu halaman (1) |
| Task mode | `FRONTEND` — backend strict read-only. Layar milik modul Billing (pemilik Yasmina); perubahan dibatasi pada bagian baru dan satu tanda |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan, roadmap, traceability `integrasi-billing` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Mulai di `8740efa02`; implementasi ikut commit pengguna `f6eca4ef7`, lalu merge pengguna `eb0a0a790` |
| Commit backend yang dijadikan rujukan | `10d3e7ee` |
| Tanggal | 6 Oktober 2026 |
| Status | ✅ **SELESAI.** Kelima acceptance criteria terpetakan ke source. Butir DoD uji manual oleh kasir **dikecualikan atas keputusan pengguna 6 Oktober 2026** |

**Otorisasi eksekusi.** Roadmap berstatus `DRAFT`; pengguna memerintahkan seluruh task dikerjakan
sampai selesai pada 5 Oktober 2026.

---

## 1. Keadaan yang ditemukan di awal

- Backend sudah menandai invoice "perlu diperiksa" ketika biaya kamar manual dan hitungan tarif kamar
  otomatis sama-sama aktif, dan menolak finalisasinya dengan `BIL-FIN-020`. Layar kasir belum punya
  daftar maupun tombol untuk menyelesaikan pemeriksaan, sehingga invoice bertanda tidak dapat
  difinalkan dari mana pun.
- **Celah kontrak.** Frontend 6.3 menyebut tanda pada invoice dibaca dari `GET billing/invoices/{id}`,
  tetapi `InvoiceDetailResponse` tidak membawa `RequiresReview` maupun `ReviewReasonCode`. Tanda di
  layar invoice karena itu dibaca dari `review-queue`, yang dijaga hak akses yang sama
  (`BillingInvoice : Read`).
- Panel finalisasi di Menu Pembayaran sudah menampilkan `BlockingReasons` dari pratinjau dan meneruskan
  pesan 422 server lewat notifikasi, sehingga `BIL-FIN-020` dan `BIL-FIN-021` sudah terbaca tanpa
  perubahan.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Invoice yang biaya kamarnya berisiko tertagih dua kali diperiksa kasir sebelum difinalkan.

**Pelaku.** Kasir. Membaca antrean: `BillingInvoice : Read`. Menyelesaikan pemeriksaan:
`BillingInvoice : Update`.

**Langkah.**

1. Kasir membuka **Billing → Invoice** (Running Invoice). Bagian **Invoice Perlu Diperiksa** tampil
   di bawah judul halaman.
2. Setiap baris memuat nomor invoice, pasien dan nomor rekam medis, alasan
   ("Perlu diperiksa: Biaya kamar manual dan otomatis"), dan sejak kapan ditandai. Saringan Jenis
   Layanan tersedia.
3. Kasir membuka Menu Pembayaran invoice itu. Peringatan kuning
   "Perlu diperiksa: biaya kamar manual dan otomatis" tampil di atas tagihan.
4. Kasir membatalkan baris biaya kamar yang dobel lewat aksi batal baris yang sudah ada.
5. Kembali ke daftar, kasir menekan **Selesai diperiksa**, menulis catatan pemeriksaan, lalu
   mengonfirmasi.
6. Invoice hilang dari daftar dan dapat difinalkan.

**Contoh.** Invoice INV-2026-0001 memuat biaya kamar manual 3 Oktober dan hitungan otomatis untuk
periode yang sama. Kasir membatalkan baris manual, lalu menyelesaikan pemeriksaan dengan catatan
"Baris biaya kamar manual 3 Okt dibatalkan, hitungan otomatis dipakai."

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Biaya kamar dobel masih aktif (422 `BIL-REV-001`) | Pesan server tampil di dialog; dialog tetap terbuka dan tanda tetap terpasang |
| Invoice berubah sejak dibaca (409) | Notifikasi "Data sudah berubah"; antrean dimuat ulang |
| Catatan kosong atau hanya tanda baca | Ditolak di layar sebelum dikirim |
| Finalisasi invoice bertanda | Pratinjau dan notifikasi menampilkan "Invoice ini perlu diperiksa sebelum difinalkan." (`BIL-FIN-020`) apa adanya |
| Tanpa `BillingInvoice : Update` | Kolom aksi berisi "-"; tombol tidak dirender |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingInvoicesController.cs` (`review-queue`, `review-resolution`, `GET {id}`),
`BillingInvoiceService.cs` (`ResolveReviewAsync`, `BillingInvoiceReviewException`),
`BillingInvoiceDtos.cs`, `BilInvoice.cs`, `BillingFinalizationService.cs`,
`BillingFinalizationDtos.cs`, controller finalisasi; frontend `billing-invoices-view.jsx`,
`use-billing-invoices.js`, `billing-invoice-slice.jsx`, `billing-invoice-constants.js`,
`use-billing-finalization.js`, `billing-finalization-slice.jsx`, `billing-finalization-panel.jsx`,
`menu-pembayaran-view.jsx`, `patient-billing-summary.service.js`, `confirm-modal.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/billing-management/billing-invoice-review.service.js` | **Baru.** `getInvoiceReviewQueue`, `resolveInvoiceReview` lewat `InstanceAxios` |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-review-constants.js` | **Baru.** Endpoint, label alasan, kode, saringan bawaan, batas catatan, salinan teks. Diletakkan di folder hook mengikuti konvensi modul Billing (`billing-invoice-constants.js`) |
| `src/utils/health-services/billing-management/billing-invoice-review-utils.js` | **Baru.** Normalisasi antrean tanpa rupiah, teks tanda, payload `ResolveInvoiceReviewRequest`, query saringan |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-invoice-review-queue.js` | **Baru.** Hook antrean dan penyelesaian (`BillingInvoice : Update` bentuk ketat); hook tanda untuk Menu Pembayaran |
| `src/components/view/health-services/billing-management/billing-invoices/billing-invoice-review-queue-section.jsx` | **Baru.** Bagian daftar, saringan, tabel, dan dialog catatan |
| `src/components/view/health-services/billing-management/billing-invoices/billing-invoices-view.jsx` | Memasang bagian antrean di bawah judul halaman |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` | Peringatan tanda "Perlu diperiksa" |
| `tests/unit/inpatient-integration-billing-finishing.test.mjs` | Tiga tes `FE-RWI-171` |

### 3.3 Kepatuhan arsitektur frontend

Operasi antrean bersifat lokal pada halaman, sehingga memakai service dan hook, bukan slice Redux
baru. Tabel memakai `DataTable` dengan kelas sel tabel semantik; tidak ada kolom rupiah.

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Daftar perlu diperiksa | `DataFilter` + `FilterSelect` + `DataTable` + `InformationAlert` | `billing-invoices-view.jsx` | COMPOSE | Bagian baru di halaman yang sama |
| Selesai diperiksa dengan catatan | `ConfirmModal requireReason` | dipakai pembatalan admisi | REUSE | — |
| Tanda pada invoice | `InformationAlert` | katalog | REUSE | — |

`UI GATE: 3 elemen — REUSE 2, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil invoice yang perlu diperiksa..." |
| Kosong | "Tidak ada invoice yang perlu diperiksa." |
| Gagal | Pesan server atau "Antrean invoice perlu diperiksa gagal dimuat." beserta tombol **Coba lagi** |
| Tanpa hak akses | Tanpa `BillingInvoice : Read`, penolakan 403 tampil sebagai pesan galat di bagian antrean (halaman Running Invoice sendiri dijaga `AccessDeniedGate`); tanpa `Update`, tombol tidak dirender |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/review-queue` | Antrean dan tanda pada Menu Pembayaran; query `ServiceType`, paginasi | `BillingInvoice : Read` |
| `POST` | `/{id}/review-resolution` | Menyelesaikan pemeriksaan dengan `Note` dan `RowVersion` | `BillingInvoice : Update` |

#### Health Services / Billing Management / Billing / Finalizations

Base URL: `api/v1/health-services/billing-management/billing/finalizations`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/invoices/{invoiceId}` | Jalur finalisasi yang sudah ada; penolakan `BIL-FIN-020`/`021` tampil apa adanya | `BillingFinalization : Create` |

Kode status: `422` `BIL-REV-001` biaya kamar manual dan otomatis masih sama-sama aktif; `409` invoice
berubah; `422` `BIL-FIN-020` finalisasi ditolak karena perlu diperiksa; `422` `BIL-FIN-021` masih ada
baris tarif belum ada.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run test:unit` sesudah implementasi | 2174 tes, 2167 lulus, 7 gagal — seluruhnya garis dasar di luar task | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` tujuh berkas tes task | 113 dari 113 lulus | `PASS` | Termasuk "antrean dibaca tanpa rupiah", "dijaga butir BillingInvoice : Update" |
| ESLint berkas task | 0 error; satu warning `set-state-in-effect` pada hook tanda sudah diperbaiki | `PASS` | Keluaran perintah |
| `npm run build` final | `✓ Compiled successfully in 64s`, exit 0 | `PASS` | Keluaran perintah |
| Uji manual oleh pengguna kasir | Tidak dijalankan | `NOT RUN` | Dikecualikan |

`AUTOMATED TEST: npm run test:unit — PASS untuk tes task (7 kegagalan tersisa = garis dasar di luar task); tujuh berkas tes task 113/113 PASS`

`MANUAL TEST: NOT FEASIBLE — butuh akun kasir dan invoice bertanda di lingkungan uji; tidak tersedia di sesi ini. Dikecualikan atas keputusan pengguna 6 Oktober 2026.`

Uji manual: `NOT FEASIBLE`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Invoice bertanda muncul di daftar; kosong → "Tidak ada invoice yang perlu diperiksa." | Terpenuhi | `billing-invoice-review-queue-section.jsx`; salinan teks di konstanta |
| 2. Tombol hanya bagi `BillingInvoice : Update` | Terpenuhi | `canResolve = permissionLoaded && updateAllowed` |
| 3. Biaya kamar dobel masih aktif → pesan `BIL-REV-001`, tanda tetap | Terpenuhi | `confirmResolve` menampilkan pesan server di dialog tanpa memuat ulang antrean |
| 4. Sesudah selesai diperiksa, invoice hilang dari daftar | Terpenuhi | Sukses memuat ulang antrean dari server |
| 5. Finalisasi invoice bertanda → pesan `BIL-FIN-020` tampil apa adanya | Terpenuhi | Jalur finalisasi yang sudah ada di Menu Pembayaran (`billing-finalization-panel.jsx`, `use-billing-finalization.js`) |
| DoD: lint dan build lulus; laporan tracked | Terpenuhi | Bagian 6 |
| DoD: uji manual oleh kasir | Dikecualikan | Keputusan pengguna 6 Oktober 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tanda di Menu Pembayaran membaca satu halaman antrean (100 baris). Antrean yang lebih panjang dari itu akan membuat tanda sebagian invoice tidak tampil, walau finalisasinya tetap ditolak server |
| Masalah yang diketahui | `InvoiceDetailResponse` belum membawa `RequiresReview`/`ReviewReasonCode`; sebaiknya ditambahkan backend supaya tanda dibaca langsung dari detail invoice sesuai kontrak |
| Dependency backend | `BE-RWI-155` 🟡. Gerbang rilis "Layar kasir dirilis bersama `BE-RWI-155`" tetap berlaku |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna meng-commit dan me-merge branch di tengah pekerjaan |
| Status Git | Implementasi di commit pengguna `f6eca4ef7`; tidak ada perubahan berkas task sesudahnya selain tes gabungan |
| Langkah berikutnya | Uji oleh kasir pada invoice uji yang memuat biaya kamar manual dan otomatis |
