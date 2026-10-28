# Laporan Perubahan Frontend — `FE-FIN-055`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-055` |
| Judul | Petugas Finance membatalkan Purchasing Invoice yang belum disetujui dari layar detail, dengan konfirmasi, hak akses, dan jalur menyusun ulang yang jelas |
| Slice | `REV-22` (Finance AP mengikuti BLM, sisi frontend) — paket `FE-AP-04` pada dokumen konsolidasi audit |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22", tabel Task REV-22 Frontend |
| Trace | `AP-UAT-19`; `FIN-DEC-045`; `AUD-AP-08` (jalur menyusun ulang setelah batal: pilihan 1, jalur existing dijelaskan di UI); `state-transition-matrix.md` §B.4 |
| Contract version | `POST /purchasing-invoices/{id}/cancel` dan hak `FinancePurchasingInvoice : Cancel` — delta kontrak `BE-FIN-126` (belum diterbitkan sebagai revisi kontrak) |
| Wewenang UI | `DEV_DISCRETION` untuk penempatan tombol; mengikat untuk aturan status yang boleh dibatalkan, konfirmasi, dan teks jalur menyusun ulang |
| Dependency | `BE-FIN-126` 🟡 [BE] — source ada, belum di-build; hak `Cancel` baru belum diberikan ke role mana pun selain SuperAdmin |
| Klasifikasi | `LIGHT` — 1 hook detail, 1 view detail, 1 berkas konstanta, 1 berkas unit test (bersama task lain) |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/` (laporan dan roadmap saja) |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `7257982bb` (branch `yasmina`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `cfe22fe8` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | 🟡 **SEBAGIAN** — source selesai, lint dan unit test lulus; build dan uji manual di browser belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

1. Backend (`BE-FIN-126`) menyediakan pembatalan Purchasing Invoice berstatus `DRAFT` atau `PENDING_APPROVAL` dengan `ExpectedRowVersion`, hak `FinancePurchasingInvoice : Cancel`, dan jejak pembatal.
2. Layar detail hanya punya Perbarui, Ajukan, Setujui, Tolak, dan Catat Retur. **Tidak ada cara membatalkan invoice dari layar**; tombol Batal yang ada hanya menutup dialog, bukan membatalkan transaksi.
3. Invoice dibatalkan masih menyisakan keterikatan: Tukar Faktur sumbernya tetap `RECEIVED` tetapi tidak dapat dipakai menyusun invoice baru (satu Tukar Faktur satu invoice, indeks unik).
   Tanpa penjelasan, petugas tidak tahu langkah selanjutnya (`AUD-AP-08`).
4. **Cacat laten pada hasil aksi.** Respons ajukan/setujui/tolak/batalkan adalah `PurchasingInvoiceResponse` **tanpa `items`**, tetapi hook mengganti seluruh detail dengan respons itu. Akibatnya,
   tabel **Rincian Item** menjadi kosong setelah aksi apa pun sampai halaman dimuat ulang.
5. Tombol **Kembali** pada detail menuju Daftar Tukar Faktur karena "Daftar Purchasing Invoice belum ada" — keterangan yang sudah tidak benar sejak `BE-FIN-122`.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: staf Finance (hak `FinancePurchasingInvoice : Cancel`).

1. Staf membuka detail Purchasing Invoice berstatus **Draf** atau **Menunggu Persetujuan**. Tombol **Batalkan** tampil (tidak tampil pada Disetujui, Ditolak, atau Dibatalkan).
2. Staf tanpa hak `Cancel` melihat tombol nonaktif disertai keterangan "Anda tidak memiliki hak membatalkan Purchasing Invoice." Keputusan akhir tetap di backend (`403`).
3. Staf menekan **Batalkan**; dialog **Batalkan Purchasing Invoice?** menjelaskan: invoice tidak dapat diajukan atau diubah lagi; Tukar Faktur sumber tetap terkunci pada invoice ini;
   untuk menyusun ulang, batalkan Tukar Faktur tersebut lalu catat Tukar Faktur baru.
4. Staf menekan **Ya, Batalkan**. Permintaan membawa `expectedRowVersion`. Berhasil → notifikasi "Purchasing Invoice berhasil dibatalkan.", status berubah **Dibatalkan**, rincian item tetap tampil.
5. Halaman invoice dibatalkan menampilkan pemberitahuan tetap (tidak dapat ditutup) tentang jalur menyusun ulang, dan tombol **Buka Tukar Faktur Sumber** menuju Tukar Faktur yang bersangkutan.
   Riwayat tidak hilang: invoice dibatalkan tetap dapat dibuka dari daftar.
6. **Jalur tidak normal:**
   - Data diubah pihak lain (`ExpectedRowVersion` usang, `409`) → pesan backend tampil, dialog ditutup, detail dimuat ulang sehingga tombol dan status sesuai kondisi terbaru.
   - Status sudah berubah (mis. sudah disetujui orang lain) → backend `422`, pesan tampil di notifikasi.
   - Hak `Cancel` belum diberikan → backend `403`, pesan tampil di notifikasi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/frontend/*`; `use-purchasing-invoice-detail.jsx`, `purchasing-invoice-detail-view.jsx`, `purchasing-invoice-constants.jsx`, `use-permission.jsx`, pola `usePermission` pada
`year-end-closing-view.jsx` dan `accounting-configuration-view.jsx`, `information-alert.jsx`, `base-button.jsx`;
backend `FinancePurchasingInvoicesController.cs` (`Cancel`, `Map`/`MapDetail`), `FinancePurchasingInvoiceService.cs` (`CancelAsync`), `FinanceInvoiceExchangeService.cs` (`CancelAsync`), dokumen konsolidasi audit `AUD-AP-08`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/finance/purchasing/use-purchasing-invoice-detail.jsx` | Aksi `cancel`; `canCancel` (status guard), `cancelBlockedReason` (hak `Cancel` lewat `usePermission`), `isCancelled`; hasil aksi **digabung** ke detail (`{ ...previous, ...updated }`) sehingga `items` tidak hilang; `409` memuat ulang detail; `goBack` menuju Daftar Invoice Supplier; `goOpenInvoiceExchange` |
| `src/components/view/finance/purchasing/purchasing-invoice/detail/purchasing-invoice-detail-view.jsx` | Tombol **Batalkan** (nonaktif + keterangan bila tanpa hak), salinan dialog konfirmasi `cancel` (`ConfirmModal`), `InformationAlert` tetap untuk invoice dibatalkan, tombol **Buka Tukar Faktur Sumber** |
| `src/lib/constants/finance/purchasing/purchasing-invoice-constants.jsx` | `PURCHASING_INVOICE_CANCELLABLE_STATUSES` (DRAFT, PENDING_APPROVAL), `PURCHASING_INVOICE_PERMISSION_RESOURCE`, `PURCHASING_INVOICE_ROUTE_BASE` |
| `tests/unit/finance-purchasing-ap-blm.test.mjs` (**baru**) | 1 test untuk task ini (hanya DRAFT dan PENDING_APPROVAL yang boleh dibatalkan) |

### 3.3 Kepatuhan arsitektur frontend

**UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0** (`BaseButton` varian `warning`, `ConfirmModal` varian `danger`, `InformationAlert` varian `info` tidak dapat ditutup).

Pola tombol nonaktif disertai keterangan mengikuti layar Accounting (`year-end-closing-view.jsx`, `usePermission`); konfirmasi destruktif memakai `ConfirmModal` (varian `danger`, tanpa alasan karena kontrak backend
tidak menerima alasan); pemberitahuan memakai `InformationAlert`. Tidak ada komponen, CSS, atau token baru. Jalur menyusun ulang mengikuti **pilihan 1** `AUD-AP-08` (pertahankan jalur existing dan jelaskan di UI);
FE tidak mencoba memakai ulang Tukar Faktur yang sama karena akan tertahan indeks unik.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Spinner "Memuat detail Purchasing Invoice..." |
| Kosong | "Purchasing Invoice tidak ditemukan." |
| Gagal | Notifikasi galat dengan pesan backend; pada `409` detail dimuat ulang otomatis |
| Tanpa hak akses | Tombol Batalkan nonaktif dengan keterangan; `403` dari backend tampil sebagai notifikasi |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Purchasing / Purchasing Invoice

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/purchasing-invoices/{id}` | Memuat detail dan status | `FinancePurchasingInvoice : Read` |
| `POST` | `/v1/corporate/finance-management/purchasing/purchasing-invoices/{id}/cancel` | Membatalkan invoice DRAFT / PENDING_APPROVAL; body `{ expectedRowVersion }` | `FinancePurchasingInvoice : Cancel` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 28 berkas gabungan `REV-22` frontend | 0 error; 8 warning pola lama | `PASS` / `EXISTING WARNING` | `FE-FIN-052` bagian 6 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/finance-purchasing-ap-blm.test.mjs` | 16 test, 16 lulus (1 milik task ini) | `PASS` | Keluaran perintah |
| `npm run test:unit` dan suite penuh | Glob npm gagal pada Node 20/Windows; suite penuh 42 gagal pada 28 berkas tidak terkait | `EXISTING / ENVIRONMENT ISSUE`; `UNRELATED EXISTING ISSUE` | `FE-FIN-052` bagian 6 |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi pengguna |
| Uji manual (batalkan Draf, batalkan Menunggu Persetujuan, stale `409`, tanpa hak `Cancel`, tabel item tetap ada setelah aksi) | Tidak dijalankan | `NOT FEASIBLE` | Dev server/browser tidak dinyalakan tanpa diminta; hak `Cancel` baru belum diberikan |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** `npm run build`, `npm run dev`, Playwright.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tombol Batalkan hanya pada DRAFT dan PENDING_APPROVAL | Terpenuhi (source) | `canCancel`; test "hanya DRAFT dan PENDING_APPROVAL yang boleh dibatalkan" |
| 2. Tanpa hak `Cancel` tombol nonaktif dengan keterangan | Terpenuhi (source) | `cancelBlockedReason` |
| 3. Konfirmasi menjelaskan Tukar Faktur tetap terkunci dan jalur menyusun ulang | Terpenuhi (source) | `CONFIRM_COPY.cancel`, `CANCELLED_NOTICE` |
| 4. Permintaan membawa `ExpectedRowVersion`; versi usang ditolak dan detail dimuat ulang | Terpenuhi (source) | `runAction` (body dan cabang `409`); penolakan sendiri oleh backend |
| 5. Invoice dibatalkan tetap dapat dibuka, rincian item tidak hilang setelah aksi, ada tombol menuju Tukar Faktur sumber | Terpenuhi (source) | `setPurchasingInvoice((previous) => ({ ...previous, ...updated }))`; `isCancelled` |
| DoD: lint PASS, laporan tracked | Terpenuhi untuk error lint (0) | Bagian 6 |
| DoD: build PASS, uji manual runtime | **Belum terpenuhi** | `NOT RUN` / `NOT FEASIBLE` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Hak `FinancePurchasingInvoice : Cancel` baru awalnya hanya dimiliki SuperAdmin; berikan lewat Akses Role sebelum uji |
| Masalah yang diketahui | (1) Invoice berstatus `REJECTED` tidak dapat dibatalkan (backend hanya mengizinkan DRAFT/PENDING_APPROVAL) dan tidak dapat diubah, sehingga Tukar Faktur-nya juga terkunci; ini keputusan backend, dilaporkan tanpa diubah. (2) Menyusun ulang tetap mengharuskan membatalkan Tukar Faktur lalu mencatat yang baru (`AUD-AP-08`, jalur existing) |
| Dependency backend | `BE-FIN-126` 🟡 (belum di-build); keputusan jalur menyusun ulang yang lebih ringan (`AUD-AP-08` pilihan 2) menunggu pemilik dan memerlukan perubahan model backend |
| Perubahan sampingan | Perbaikan `items` hilang setelah aksi (temuan 4) berlaku untuk ajukan/setujui/tolak juga, bukan hanya batalkan |
| Interupsi | `NONE` |
| Status Git | Lihat `FE-FIN-054` bagian 8 |
| Langkah berikutnya | Berikan hak `Cancel`, `npm run build`, lalu uji manual pada invoice Draf dan Menunggu Persetujuan, termasuk dua tab untuk memicu `409` |
