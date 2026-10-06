# Laporan Perubahan Frontend — `FE-FIN-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-032` |
| Judul | Penyetuju cutover dapat mengunduh templat, mengunggah tagihan lama, memperbaiki baris yang salah, lalu menyerahkannya untuk disetujui — Layar Batch Migrasi Tagihan Lama |
| Slice | `REV-14E` — `EPIC FIN-24`, migrasi tagihan lama dua format |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian REV-14D dan REV-14E (frontend) |
| Trace | `FR-FIN-167`..`170`, `FR-FIN-178`, `FR-FIN-181`..`183`; `FIN-DEC-129`, `136`, `140`; `FIN-DES-089`, `090`, `093`; `03-frontend-architecture.md` 19.2 nomor 9 dan 20.2 |
| Contract version | `FIN-API-1.6` F.2 (`contract_status: approved 2026-10-02`); `FIN-STATE-1.6` F.2; `FIN-VAL-1.7` `FIN-VAL-186`..`196`; `FIN-VAL-1.8` `FIN-VAL-224`..`226`; `FIN-PERM-1.7` G.5 |
| Wewenang UI | Frontend Owner — **sebagian tertahan** `FIN-OQ-079` (menu). **Mengikat:** unduh templat menuntut jenis **dan** format; pemilih unggah **tidak** meminta format; galat baris selalu membawa nomor baris dan dapat disalin/diunduh; penolakan rekonsiliasi menampilkan kedua angka berdampingan; asal format batch ditampilkan; pilihan XLSX pada unduh templat nonaktif sampai `BE-FIN-083`. `DEV_DISCRETION`: tata letak, warna |
| Dependency | `BE-FIN-082` 🟡 (daftar, rincian, nyatakan, setujui, tolak), `BE-FIN-081` 🟡 (unggah, validasi), `BE-FIN-080` 🟡 (templat CSV). Seluruhnya source lengkap; pengguna melaporkan `dotnet build` sukses setelah task itu ditulis. **`BE-FIN-083` (pembaca XLSX) belum ada**, tertahan `FIN-OQ-081`. Penerapan migration `AddFinanceOpeningItemMigration` **tidak dikonfirmasi** dalam sesi ini |
| Klasifikasi | `HIGH` — 1 rute, 1 view + 4 komponen turunan, 1 hook, 1 slice, 1 service, 1 berkas konstanta, 1 berkas utilitas, 1 CSS Module, 1 unit test, 2 perubahan kecil pada berkas existing; unggah berkas, unduh berkas, dan dua tindakan yang tidak dapat ditarik |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only (tidak diubah); laporan dan bukti roadmap ditulis di repository backend |
| Branch | `yasmina` (upstream `origin/yasmina`); penetapan eksplisit pemegang modul belum dikonfirmasi, sama seperti task sebelumnya |
| Tanggal | 3 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak dan keputusan pemilik 3 Oktober 2026 (unggah ulang). Backend `BE-FIN-085` ikut berubah dan **belum dikompilasi**. `npm run lint:errors`, `npm run test:unit`, `npm run build` **NOT RUN** (standing instruction: dijalankan manual oleh pengguna). Verifikasi manual UI **NOT FEASIBLE** dari sesi ini (§8). Jalur XLSX tidak dapat dibuktikan end-to-end karena `BE-FIN-083` belum ada |

---

## 1. Masalah yang Diperbaiki

Backend `BE-FIN-079`..`082` sudah menyediakan alur migrasi tagihan lama (templat, unggah, validasi per baris, rekonsiliasi, setujui/tolak), tetapi petugas tidak punya layar untuk menjalankannya:

1. **Tidak ada jalan membuat batch.** Piutang dan utang yang masih berjalan (ribuan lembar) hanya dapat masuk lewat batch, karena saldo awal cutover kelompok itu wajib nol (`FIN-VAL-181`).
2. **Galat per baris harus bisa ditindaklanjuti.** Berkas diperbaiki di luar sistem; petugas butuh nomor baris dan alasan, dalam bentuk yang dapat disalin atau diunduh.
3. **Dua format, dua tempat berbeda.** Unduh templat **meminta** format; unggah **tidak** (backend menentukannya dari berkas, `FIN-DES-093`). Pemilih format pada unggah membuka jalan memaksa pembaca yang salah.
4. **XLSX belum dapat dipakai.** Pembaca XLSX di backend belum dibangun; menampilkan pilihannya lebih dulu menghasilkan unduhan dan unggahan yang gagal.
5. **Persetujuan tidak dapat ditarik** dan melahirkan item beserta mutasi pembuka, lalu mengunci batch. Penolakan rekonsiliasi harus menampilkan **kedua angka**, bukan sekadar "tidak cocok".
6. **Angka total di backend menyesatkan bila dibaca mentah:** `TotalOutstandingAmount` bernilai 0 sampai validasi dan hanya menjumlah baris yang lolos (§5 delta 5).

---

## 2. Proses Bisnis dari Sisi Pengguna

**Pelaku:** penyiap (hak `FinanceOpeningItemBatch` `Create`/`Update`) dan penyetuju (`Approve`). Satu orang **dapat** memegang keduanya — layar menyatakan ini (§9).

1. Petugas membuka `/finance/subledger-setup/opening-item-batches`.
2. **Unduh templat:** memilih jenis item (Piutang / Utang Supplier) **dan** format. Tombol *Unduh Templat* nonaktif sampai keduanya terpilih. XLSX terlihat tetapi nonaktif disertai keterangan.
3. **Unggah:** memilih jenis item, lalu satu berkas (CSV; XLSX diterima pemilih tetapi akan ditolak backend sampai tersedia). Format **tidak** ditanyakan. Berhasil → batch berstatus *Draf* terbuka pada panel rincian.
4. **Validasi:** *Validasi Batch* memeriksa setiap baris. Tanpa galat → *Tervalidasi*. Dengan galat → tetap *Draf* dan daftar galat tampil dengan **nomor baris**; petugas dapat menyalin atau mengunduhnya (CSV), memperbaiki berkas di luar sistem, lalu mengunggah ulang sebagai batch baru dan memvalidasinya.
5. **Nyatakan saldo awal Accounting:** nominal dan rujukan dokumen (keduanya wajib). Status batch tidak berubah.
6. **Rekonsiliasi:** panel menampilkan *Total sisa tagihan (dari berkas)* dan *Saldo awal Accounting (dinyatakan)* berdampingan beserta selisih.
7. **Setujui:** dialog menampilkan kedua angka, menyatakan persetujuan tidak dapat ditarik dan mengunci batch. Backend menolak `422` bila total tidak sama; dialog tetap terbuka dengan kedua angka dan pesan backend.
8. **Tolak:** dialog dengan alasan wajib; tombol nonaktif sampai alasan terisi. Batch ditolak bersifat final.

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

| No | Elemen | Keputusan | Sumber |
|:--:|---|:--:|---|
| 1 | Header, banner, peringatan | `REUSE` | `base-features/hero`, `information-alert` |
| 2 | Saringan dan tabel daftar berpaging | `REUSE` | `base-features/data-filter`, `filter-select`, `data-table`, `pagination/pagination` |
| 3 | Lencana status | `REUSE` | `base-features/status-badge` |
| 4 | Pemilih jenis item, input nominal dan rujukan | `REUSE` | `base-features/base-form-control` |
| 5 | Dialog setujui dan tolak (tolak: alasan wajib bawaan) | `REUSE` | `base-features/confirm-modal` |
| 6 | Penutup akses, tombol, radio format | `REUSE` | `access-denied-gate`, `base-button`, `Form.Check` (react-bootstrap) |
| 7 | Kartu unduh templat, kartu unggah, panel rincian, kolom tabel | `NEW (view-lokal)` | `opening-item-batches/` — spesifik domain, satu layar |
| 8 | Unduh templat dan simpan berkas ke perangkat | `NEW (service)` | `lib/services/finance/subledger-setup/opening-item-batch-service.jsx` — Blob bukan nilai serializable sehingga tidak melewati Redux |

**Hasil UI Gate:** 6 kelompok `REUSE`, 2 view-lokal/service. Tidak ada base component, wrapper, atau abstraksi generik baru. Satu CSS Module (`.../opening-item-batches-view.module.css`): kelas halaman memakai token `--base-*` dari `.dataPage`; kelas di dalam modal memakai token global karena modal dirender lewat portal.

---

## 4. Cara Mencapai Layar Selama Menu Tertahan (`FIN-OQ-079`)

Butir menu **tidak** dipasang (bukan `DEV_DISCRETION`). Layar dicapai lewat:

1. **URL langsung:** `/finance/subledger-setup/opening-item-batches`
2. **Tautan dari layar terkait:** banner info baru pada layar *Saldo Awal Cutover* (`/finance/subledger-setup/opening-balances`) — *Buka Batch Migrasi Tagihan Lama →*
3. **Tautan balik:** banner info pada layar batch — *Buka Saldo Awal Cutover →*

---

## 5. Endpoint yang Dikonsumsi dan Pemetaan Kontrak

Base URL: `/api/v1/corporate/finance-management/opening-item-batches`

| Method | Path | Hak akses | Dipanggil lewat | Catatan |
| :-- | :-- | :-- | :-- | :-- |
| `GET` | `/` | `Read` | `fetchOpeningItemBatches` | Berpaging; saringan `itemKind`, `status`; `signal` diteruskan |
| `GET` | `/{id}` | `Read` | `fetchOpeningItemBatchDetail` | Memuat hasil validasi per baris terakhir |
| `GET` | `/template?itemKind=&format=` | `Read` | `downloadOpeningItemBatchTemplate` (service) | `responseType: blob`; galat JSON dibaca dari Blob |
| `POST` | `/` | `Create` | `uploadOpeningItemBatch` | `multipart/form-data`: `file`, `itemKind`. **Tanpa ruas format** |
| `POST` | `/{id}/reupload` | `Update` | `reuploadOpeningItemBatch` | `multipart/form-data`: `file`, `expectedRowVersion`. **Endpoint baru `BE-FIN-085`**; hanya batch `DRAFT` |
| `POST` | `/{id}/validate` | `Update` | `validateOpeningItemBatch` | Tanpa `RowVersion` |
| `POST` | `/{id}/declare-accounting-opening` | `Update` | `declareOpeningItemBatchAccountingOpening` | `expectedRowVersion`, nominal, rujukan |
| `POST` | `/{id}/approve` | `Approve` | `approveOpeningItemBatch` | `expectedRowVersion` |
| `POST` | `/{id}/reject` | `Approve` | `rejectOpeningItemBatch` | `expectedRowVersion`, `rejectionReason` |

### Delta antara kontrak, backend saat ini, dan layar

Dibaca dari `FinanceOpeningItemBatchesController.cs`, `OpeningItemBatchDtos.cs`, dan `FinanceOpeningItemBatchService.cs`. Backend tidak diubah.

| # | Temuan | Perlakuan di layar |
|:--:|---|---|
| 1 | `GET /template?format=XLSX` dijawab **`503`** (bukan `400`): nilainya sah, sistem belum siap | Pilihan XLSX dinonaktifkan di layar (`OPENING_ITEM_BATCH_XLSX_READER_AVAILABLE = false`, satu-satunya tempat keadaan ini dinyatakan). Bila tetap sampai backend, pesan `503` diteruskan apa adanya |
| 2 | `POST /` dengan berkas XLSX dijawab `503` ("Berkas XLSX belum dapat diproses…") | Pemilih tetap menerima CSV **dan** XLSX (kontrak); keterangan di atas pemilih menyatakan XLSX belum dapat diproses; pesan backend ditampilkan bila tetap dikirim |
| 3 | Unggah tidak menjalankan validasi baris; batch lahir `DRAFT` dengan `TotalItemCount = 0` dan `TotalOutstandingAmount = 0` | Validasi adalah tombol tersendiri. Layar tidak menjalankannya otomatis |
| 4 | Unggah dijawab `200` (kontrak tidak menyebut kode sukses) | Tidak berdampak; keberhasilan dibaca dari `requestStatus` |
| 5 | `TotalOutstandingAmount` **hanya menjumlah baris yang lolos** dan bernilai 0 sebelum validasi | Layar **tidak** membandingkan angka itu dengan Accounting kecuali seluruh baris sudah divalidasi dan lolos. Sebelum validasi tertulis *Belum divalidasi*; dengan baris bergalat tertulis bahwa angka baru mencakup baris yang lolos. Pada daftar, *Belum ada baris lolos* menggantikan `Rp 0` untuk Draf tanpa item |
| 6 | Pesan `422` rekonsiliasi memformat angka tanpa desimal (`N0`), sehingga selisih di bawah Rp 1 tidak terlihat; redaksinya juga berbeda dari `FIN-VAL-192` pada kontrak | ✅ **Diperbaiki di backend 3 Oktober 2026 (`BE-FIN-085`, belum dikompilasi):** dua desimal, budaya `id-ID`, dan kalimat penutup *"Batch tidak dapat disetujui."*. Layar tetap menampilkan **kedua angka dari data batch** berdampingan dan tidak mengurai angka dari teks pesan. Redaksi `FIN-VAL-192` pada `validation-matrix.md` belum diselaraskan (menunggu wewenang kontrak) |
| 7 | `approve` memvalidasi ulang berkas fisik; bila kini bergalat, batch dikembalikan ke `DRAFT` dan dijawab `409` | Setiap `409` menutup dialog dan memuat ulang rincian serta daftar |
| 8 | `RowVersion` berputar pada validasi dan setiap tindakan tulis | Layar selalu memakai `rowVersion` rincian terkini untuk deklarasi/setujui/tolak, bukan salinan lama |
| 9 | Tolak memakai hak `Approve` (bukan `Update`) | Tombol tolak mengikuti hak `Approve`, dan tersedia dari `DRAFT` maupun `VALIDATED` |
| 10 | `ApprovedBy` hanya `Guid` | Layar menampilkan waktu persetujuan/penguncian saja |
| 11 | Hasil validasi per baris disimpan sebagai JSON utuh dan dikirim tanpa paging | Layar merender maksimal 200 galat; daftar lengkap tersedia lewat salin/unduh |
| 12 | `api-contract.md` F.2 masih berlabel **"Rencana (belum tersedia)"** pada kedelapan endpoint | Tidak diubah pada task ini (artefak kontrak di luar wewenang tulis task ini); dilaporkan |
| 13 | Batch hanya dapat diunggah setelah saldo awal cutover ditetapkan (`422`) | Banner dan tautan ke *Saldo Awal Cutover*; pesan backend ditampilkan bila tetap terjadi |
| 14 | `state-transition-matrix.md` F.2 menyebut tindakan **"Mengunggah ulang berkas"** pada batch `DRAFT`, tetapi controller tidak punya endpoint untuk itu | ✅ **Diputuskan pemilik 3 Oktober 2026: tambah endpoint.** Backend: `POST /{id}/reupload` (`BE-FIN-085`, belum dikompilasi). Layar: bagian *Unggah Ulang Berkas* pada panel rincian, hanya untuk batch `DRAFT` dan hak `Update`; memakai jenis item milik batch, tanpa pemilih format; hasil validasi lama dikosongkan di layar sampai batch divalidasi lagi. `api-contract.md` F.2 **belum** memuat endpoint ini (menunggu wewenang kontrak) |

---

## 6. Privasi

| Hal | Perlakuan |
| --- | --- |
| Kolom *Pengenal* (nama debitur/kode supplier — Sensitif) | Tampil **hanya** pada tabel galat layar ini, bagi pemegang hak `Read` |
| Daftar galat yang disalin dan diunduh | Memuat **nomor baris, kode galat, dan alasan saja**. Kolom pengenal sengaja dibuang; nomor baris cukup untuk menemukan baris pada berkas asal. Keterangan ini ditulis di layar |
| Berkas CSV galat | Sel yang diawali `=`, `+`, `-`, `@`, tab, atau CR dinetralkan (diawali `'`) agar tidak ditafsirkan sebagai rumus saat dibuka di spreadsheet; seluruh sel dikutip |
| Isi berkas migrasi | Tidak dicatat ke log klien dan tidak dikirim ke pihak ketiga; hanya diteruskan ke backend |

---

## 7. Berkas yang Diubah dan Dibuat

### Baru (`QuilvianSystemFrontendDev`)

| Berkas | Isi |
| --- | --- |
| `src/app/finance/subledger-setup/opening-item-batches/page.jsx` | Rute tipis dengan metadata dan `Suspense` |
| `src/components/view/finance/subledger-setup/opening-item-batches/opening-item-batches-view.jsx` | View: akses, banner, kartu unduh/unggah, daftar, panel rincian, dialog setujui/tolak |
| `.../opening-item-batches/opening-item-batch-template-card.jsx` | Unduh templat: jenis + format, XLSX nonaktif |
| `.../opening-item-batches/opening-item-batch-upload-card.jsx` | Satu pemilih berkas CSV/XLSX, tanpa pemilih format |
| `.../opening-item-batches/opening-item-batch-detail-panel.jsx` | Rincian, rekonsiliasi, nyatakan saldo awal, galat per baris, aksi |
| `.../opening-item-batches/opening-item-batch-table-columns.jsx` | Kolom tabel (dengan `key`, sesuai API `DataTable`) |
| `src/lib/hooks/finance/subledger-setup/use-opening-item-batches.jsx` | Hook: izin, daftar, rincian, templat, unggah, aksi, penjaga klik ganda |
| `src/lib/state/slice/finance/subledger-setup/finance-opening-item-batch-slice.jsx` | Slice dan delapan thunk (termasuk unggah ulang) |
| `src/lib/services/finance/subledger-setup/opening-item-batch-service.jsx` | Unduh templat (Blob), simpan berkas ke perangkat |
| `src/lib/constants/finance/subledger-setup/opening-item-batch-constants.jsx` | Endpoint, jenis, status, pesan, penanda pembaca XLSX |
| `src/utils/finance/subledger-setup/opening-item-batch-utils.jsx` | Fungsi murni: normalisasi, templat, unggah, galat baris, CSV aman, rekonsiliasi, kendali status, payload |
| `src/style/corporate/finance/subledger-setup/opening-item-batches-view.module.css` | CSS Module |
| `tests/unit/opening-item-batch-utils.test.mjs` | 26 unit test (ditulis, **belum dijalankan**) |

### Diubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/store.jsx` | Impor dan registrasi reducer `financeOpeningItemBatch` (+2 baris) |
| `src/components/view/finance/subledger-setup/opening-balances/opening-balances-view.jsx` | Satu banner info bertautan ke layar batch (+7 baris) |

Tidak ada perubahan pada repository backend selain berkas laporan ini dan bukti roadmap.

---

## 8. Verifikasi dan Validasi

| Pemeriksaan | Perintah | Status | Keterangan |
| --- | --- | :--: | --- |
| Lint | `npm run lint:errors` | `NOT RUN` | Standing instruction pengguna. **Menunggu pengguna**. Risiko khusus: hook mengatur state di dalam `useEffect` (pola yang sama dengan `FE-FIN-028`/`031`); bila aturan `react-hooks` yang dipakai repository melarangnya, lint akan menandainya |
| Unit test | `npm run test:unit` atau `node --import ./tests/helpers/register.mjs --test tests/unit/opening-item-batch-utils.test.mjs` | `NOT RUN` | 26 test ditulis; hasil belum diketahui |
| Build | `npm run build` | `NOT RUN` | Alasan sama |
| Resolusi import dan named export | skrip pemeriksa berkas | `PASS` | 14 berkas; 0 masalah |
| `git status --short` | — | `PASS` | §11 |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Alasan konkret: sesi ini tidak menjalankan server dev, tidak memegang sesi login, tidak memverifikasi backend + migration `AddFinanceOpeningItemMigration` terapan, dan tidak memiliki berkas migrasi uji. Belum satu pun perilaku di bawah ini pernah teramati.

| # | Skenario | Yang harus teramati |
|:--:|---|---|
| 1 | Buka layar tanpa memilih apa pun pada kartu templat | Tombol *Unduh Templat* nonaktif dengan keterangan; pilih hanya jenis → tetap nonaktif; pilih hanya format → tetap nonaktif; keduanya → aktif |
| 2 | Pilihan format XLSX | Terlihat, **nonaktif**, dengan keterangan bahwa pembaca XLSX belum tersedia |
| 3 | Unduh templat CSV piutang dan utang supplier | Dua berkas berbeda terunduh dengan nama dari backend (`opening-item-receivable.csv`, `opening-item-supplier-payable.csv`) |
| 4 | Pemilih unggah | **Tidak ada** pemilih format; menerima `.csv` dan `.xlsx`; keterangan menyatakan format ditentukan sistem |
| 5 | Unggah tanpa memilih jenis item atau berkas, atau berkas `.pdf` | Galat di layar; tidak ada permintaan terkirim |
| 6 | Unggah CSV sah (saldo awal cutover sudah dicatat) | Batch *Draf* terbuka pada panel rincian; asal berkas *CSV*; total *Belum divalidasi* |
| 7 | Unggah sebelum saldo awal cutover dicatat | Pesan backend (`422`) tampil; batch tidak terbentuk |
| 8 | Unggah berkas XLSX | Pesan `503` backend tampil; tidak ada batch |
| 9 | Validasi berkas dengan baris salah | Batch tetap *Draf*; tabel galat memuat **nomor baris**, kode, alasan; hitungan lolos/bergalat; total berlabel baru mencakup baris lolos |
| 10 | *Salin Daftar Galat* dan *Unduh Daftar Galat (CSV)* | Teks/CSV berisi nomor baris, kode, alasan — **tanpa** nama debitur/kode supplier; sel berawalan `=` terbaca sebagai teks di spreadsheet |
| 11 | Validasi berkas tanpa galat | Batch *Tervalidasi*; total sisa tampil |
| 12 | Nyatakan saldo awal Accounting: nominal kosong/0 atau rujukan kosong | Galat di layar; tidak ada permintaan terkirim |
| 13 | Nyatakan nominal **berbeda** dari total, lalu *Setujui Batch* | Panel menampilkan kedua angka dan selisih; dialog menampilkan kedua angka; backend menolak `422`; dialog tetap terbuka dengan pesan backend dan kedua angka |
| 14 | Nyatakan nominal **sama**, setujui, klik cepat dua kali | Hanya satu permintaan; batch menjadi *Terkunci*; tidak ada aksi tersisa |
| 15 | Tolak: alasan kosong | Tombol konfirmasi nonaktif; alasan terisi → batch *Ditolak*, final |
| 16 | Dua sesi: ubah batch di sesi A, lalu setujui dari data lama di sesi B | Sesi B menerima pesan batch sudah diubah; rincian dimuat ulang |
| 17 | Pengguna hanya `Read` | Tidak ada kartu unggah, tombol validasi/nyatakan/unggah ulang/setujui/tolak |
| 18 | Pengguna `Create`+`Update` tanpa `Approve` | Validasi dan nyatakan tersedia; setujui dan tolak **tidak** |
| 19 | Pengguna tanpa `Read` | Penutup akses |
| 20 | Saringan jenis item dan status, atur ulang, ganti halaman | Daftar mengikuti saringan; kembali ke halaman 1 saat saringan berubah |
| 21 | Batch *Draf* bergalat → *Unggah Ulang Berkas* dengan berkas perbaikan | Batch yang sama tetap (nomor batch sama); nama berkas dan total diperbarui; tabel galat kosong dan total *Belum divalidasi*; saldo awal Accounting yang sudah dinyatakan tetap; setelah divalidasi hasilnya mengikuti berkas baru |
| 22 | *Unggah Ulang Berkas* pada batch *Tervalidasi*, *Disetujui*, *Terkunci*, *Ditolak* | Bagian itu **tidak** tampil; bila dipanggil langsung, backend menjawab `409` |
| 23 | Berpindah batch setelah memilih berkas pengganti | Pilihan berkas dikosongkan |
| 24 | Pesan galat penolakan rekonsiliasi (`422`) | Angka pada pesan backend memuat dua desimal; kedua angka pada dialog tetap sama dengan data batch |

---

## 9. Pemenuhan Acceptance Criteria

Status "Terpenuhi (source)" = terpenuhi menurut pembacaan kode; **belum teramati saat dijalankan**.

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | Unduh templat menuntut **jenis dan format** dipilih keduanya; tombol nonaktif sampai itu | Terpenuhi (source) | `canDownloadTemplate`; kartu templat; unit test |
| 2 | Pemilih unggah **tidak** meminta format; pemilih format pada unggah tidak dibuat | Terpenuhi (source) | Kartu unggah hanya memuat jenis item dan berkas; thunk tidak mengirim format; unit test |
| 3 | Satu pemilih unggah menerima CSV **dan** XLSX | Terpenuhi (source) | `accept=".csv,.xlsx,…"`; `validateUploadSelection`. XLSX ditolak backend sampai `BE-FIN-083` (delta 2) |
| 4 | Galat baris **selalu** menampilkan nomor barisnya dan dapat disalin atau diunduh | Terpenuhi (source) | Tabel galat; `buildErrorListText`, `buildErrorListCsv`; unit test |
| 5 | Penolakan rekonsiliasi (`422`) menampilkan **kedua angka berdampingan**, bukan hanya "tidak cocok" | Terpenuhi (source) | Panel rekonsiliasi dan dialog setujui; delta 6 |
| 6 | Kolom menyatakan saldo awal Accounting beserta rujukan dokumen (wajib) | Terpenuhi (source) | Form nyatakan; `validateDeclareForm`; unit test |
| 7 | Aksi setujui dan tolak; tolak mewajibkan alasan | Terpenuhi (source) | Dialog; `ConfirmModal requireReason`; kendali per status |
| 8 | Asal format batch ditampilkan | Terpenuhi (source) | Kolom *Asal Berkas* dan rincian dari `sourceFormat` backend |
| 9 | Pilihan XLSX pada unduh templat **dinonaktifkan** sampai `BE-FIN-083` selesai | Terpenuhi (source) | `OPENING_ITEM_BATCH_XLSX_READER_AVAILABLE = false`; unit test |
| 10 | Laporan menyebut keadaan pilihan XLSX dan cara mencapai layar | Terpenuhi | §4, §5 delta 1–2 |
| 11 | Verifikasi manual jalur galat baris dan jalur selisih rekonsiliasi | **NOT FEASIBLE** | §8 skenario 9, 13 |
| 12 | `npm run lint` dan `npm run build` PASS | **Belum terpenuhi — NOT RUN** | §8 |

---

## 10. Risiko

1. **Pemisahan penyiap dan penyetuju tidak dijaga mesin** (`FIN-PERM-1.7` G.5): satu orang dapat memegang `Create`/`Update` **dan** `Approve`. Layar menyatakan ini lewat banner yang sama dengan layar saldo awal; `MUST` disampaikan saat menyerahkan modul.
2. **Persetujuan dan penolakan tidak dapat ditarik.** Persetujuan melahirkan item beserta mutasi pembuka di buku piutang/utang. Pengamannya: dialog konfirmasi, kedua angka, penjaga klik ganda, dan `RowVersion`.
3. **Jalur XLSX tidak dapat dibuktikan** sampai `BE-FIN-083`. Saat ia selesai, `OPENING_ITEM_BATCH_XLSX_READER_AVAILABLE` perlu dibalik **dan** skenario unduh/unggah XLSX diamati; membaliknya tanpa pembuktian adalah risiko.
4. **Salin ke papan klip** bergantung pada izin peramban; bila ditolak layar menyarankan unduh.
5. **Unggah ulang bergantung pada `BE-FIN-085` yang belum dikompilasi.** Memakai bagian *Unggah Ulang Berkas* sebelum backend itu dibangun dan diterapkan menghasilkan `404`. Backend memutar `RowVersion` sehingga dua sesi yang mengunggah ulang bersamaan akan ditolak `409` salah satunya.
5b. **Jendela kegagalan pada penyimpanan berkas** (`BE-FIN-085` §4): bila penggantian berkas fisik gagal setelah basis data tersimpan, metadata dan berkas tidak sinkron; validasi berikutnya akan membaca berkas yang salah.
6. **Ukuran berkas besar:** layar tidak membatasi ukuran di klien (kontrak tidak menyebut batasnya); kegagalan datang dari backend.
7. **Migration `AddFinanceOpeningItemMigration` belum dikonfirmasi diterapkan.** Bila tabel tidak ada, daftar gagal dan layar menampilkan galat biasa dengan *Coba Lagi*.

### Temuan di luar cakupan (`UNRELATED EXISTING ISSUE`, tidak diperbaiki)

- `subledger-control-accounts-view.jsx` (`FE-FIN-027`) memberi `DataFilter` prop `searchValue`, sedangkan komponen itu membaca `searchKeyword`; kolom pencarian pada layar itu kemungkinan tidak terkendali.
- `BE-FIN-079`..`082` masih berstatus 🟡 pada roadmap backend sementara pengguna melaporkan build sukses; belum diperbarui. Bukan wewenang task frontend.

---

## 11. Status Git (frontend)

Branch `yasmina`; tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Working tree juga memuat pekerjaan `FE-FIN-027`..`031` yang belum di-commit (tidak diulang di bawah).

```text
 M src/lib/state/store.jsx
 M src/components/view/finance/subledger-setup/opening-balances/opening-balances-view.jsx
?? src/app/finance/subledger-setup/opening-item-batches/
?? src/components/view/finance/subledger-setup/opening-item-batches/
?? src/lib/constants/finance/subledger-setup/opening-item-batch-constants.jsx
?? src/lib/hooks/finance/subledger-setup/use-opening-item-batches.jsx
?? src/lib/services/finance/
?? src/lib/state/slice/finance/subledger-setup/finance-opening-item-batch-slice.jsx
?? src/style/corporate/finance/subledger-setup/
?? src/utils/finance/subledger-setup/opening-item-batch-utils.jsx
?? tests/unit/opening-item-batch-utils.test.mjs
```

---

## 12. Langkah Berikutnya

Jalankan manual: `npm run lint:errors`, `npm run test:unit`, `npm run build`, lalu amati skenario §8 — terutama nomor 9 (galat baris), 10 (isi salinan/CSV), 13 (selisih rekonsiliasi), 14 (klik ganda), dan 1–2 (templat). Setelah itu task dapat ditandai ✅ untuk jalur CSV; jalur XLSX menunggu `BE-FIN-083`.
