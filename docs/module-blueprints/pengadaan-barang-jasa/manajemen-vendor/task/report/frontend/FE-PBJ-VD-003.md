# Laporan Perubahan Frontend — `FE-PBJ-VD-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-VD-003` |
| Judul | Dokumen legalitas |
| Slice | Gelombang 3 roadmap frontend `manajemen-vendor` revisi 1 |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | Layar `FE-PBJ-VD-04` dan tab Dokumen `FE-PBJ-VD-02`; FR-VD-020..023; `PBJ-DEC-045`, `046`, `061`; `PBJ-ASM-001`, `002`; `PBJ-FE-003`, `004` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor Document dan `GET /vendors/eligibility`, dengan delta `BE-PBJ-VD-004` D-1..D-8 dan delta INT `BE-PBJ-VD-005` (`warnings`, kode dan nama vendor) |
| Wewenang UI | `PBJ-FE-001` nama menu SOURCE, `PBJ-FE-004` standar layar daftar, `PBJ-FE-003` invariant (tombol tersembunyi bukan penjaga; server tetap memutus); rupa form dan detail dokumen `DEV_DISCRETION` (arsitektur frontend bagian 4.3) |
| Dependency | `FE-PBJ-VD-002` ✅, `BE-PBJ-VD-004` ✅, `BE-PBJ-VD-005` ✅ |
| Klasifikasi | `MEDIUM` — 1 layar baru, 1 tab baru, 3 hook, 1 service, perluasan 1 service, 1 komponen bersama modul, 1 utilitas |
| Task mode | `FRONTEND` (instruksi pengguna "lanjutkan", 18 November 2026; dilanjutkan "lanjutkan, db sudah di perbaiki", 19 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `b1b360efa` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `a6df5619` (branch `sukmagp`, berisi `BE-PBJ-VD-001`..`008`, belum di-commit) |
| Tanggal | 18 November 2026 (kode, putaran 1); 19 November 2026 (putaran 2, perbaikan tab, penutupan) |
| Status | ✅ Selesai — lint dan build `PASS`; uji layar putaran 2 16/16 `PASS`; cek API 4/4 `PASS` |

---

## 1. Keadaan yang ditemukan di awal

Butir menu "Dokumen Legalitas" sudah ada sejak `FE-PBJ-VD-001`, tetapi rutenya `/corporate/procurement/vendors/documents` masih 404. Detail Vendor (`FE-PBJ-VD-002`) baru punya tab Profil & Kategori, Rekening, dan Riwayat.

Backend sudah lengkap:

- `BE-PBJ-VD-004`: daftar, ringkasan, detail, catat, verifikasi, tolak, batal, dan riwayat dokumen. Berkas dokumen memakai endpoint lampiran bersama dengan `documentType = PBJ_VENDOR_DOCUMENT`.
- `BE-PBJ-VD-005`: kelayakan vendor per kategori.

Komponen lampiran `FE-PBJ-002` (`ProcurementAttachmentPanel`) sudah bisa dipakai untuk jenis dokumen apa pun lewat prop `documentType` dan `readOnly`.

---

## 2. Proses bisnis dari sisi pengguna

1. **Catat dokumen.** Petugas Tim Pengadaan menekan **+ Catat Dokumen**, memilih vendor dan jenis dokumen, lalu mengisi nomor, tanggal terbit, dan tanggal berlaku. Jenis yang mewajibkan tanggal berlaku ditandai "wajib tanggal berlaku" di pilihan dan di keterangan isian. Aturan tanggal (`PBJ-VAL-101`) diperiksa server, lalu pesannya tampil di form.
2. **Unggah berkas.** Setelah tersimpan, kartu dokumen langsung terbuka dengan status **Menunggu verifikasi**. Pengunggah mengunggah berkas di bagian Lampiran. Hanya pengunggah yang bisa menambah, mengganti, atau menarik berkas, dan hanya selama dokumen masih menunggu verifikasi.
3. **Verifikasi oleh petugas lain.** Petugas lain membuka dokumen dari antrean (saringan Status = Menunggu verifikasi), memeriksa berkas, lalu menekan **Verifikasi** atau **Tolak** dengan alasan minimal 10 karakter.
   - Pengunggah tidak melihat kedua tombol itu. Ia melihat keterangan "Anda pengunggah dokumen ini, jadi verifikasinya dilakukan petugas lain." (`PBJ-DEC-046`).
   - Bila tetap dikirim, server menolak dengan 403 `PBJ-VAL-103`.
4. **Batal.** Pengunggah dapat membatalkan catatan yang salah selama masih menunggu verifikasi.
5. **Keadaan berlaku.** Dokumen terverifikasi menampilkan keadaan **Berlaku**, **Segera kedaluwarsa**, **Kedaluwarsa**, atau **Digantikan**, serta jarak ke tanggal berlaku, misalnya "20 hari lagi" atau "lewat 1 hari". Ringkasan menghitung dokumen yang kedaluwarsa dan yang segera kedaluwarsa.
6. **Kelayakan per kategori.** Tab Dokumen di Detail Vendor menampilkan, untuk setiap kategori vendor, apakah vendor **Layak** atau **Tidak layak** beserta alasannya. Contoh alasan: "Dokumen wajib Izin PBF kedaluwarsa sejak 17-11-2026." Tab ini juga menampilkan peringatan yang tidak menggugurkan, misalnya "berakhir dalam 20 hari".

Contoh: Rina mencatat Izin PBF milik "Sehat Farma" yang berlaku sampai 08-12-2026 dan mengunggah PDF-nya. Selama belum diverifikasi, kategori Obat (yang mewajibkan Izin PBF) tampil **Tidak layak — belum terverifikasi**. Budi, petugas lain, memverifikasi. Kategori Obat berubah menjadi **Layak** dengan peringatan "berakhir dalam 20 hari", dan kartu Segera Kedaluwarsa bertambah satu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` bagian 2–7; `contracts/api-contract.md` §Vendor Document dan kelayakan; `testing/acceptance-test-matrix.md` `AT-VD-10`..`16`; laporan `BE-PBJ-VD-004` (delta D-1..D-8) dan `BE-PBJ-VD-005`.

Backend:

- `VendorDocumentsController.cs`, `VendorDocumentDtos.cs`, `ProcurementVendorDocumentService.cs`, `ProcurementVendorDocumentValidity.cs`;
- `VendorEligibilityDtos.cs`, `ProcurementVendorEligibilityService.cs`;
- `ProcurementAttachmentsController.cs`;
- konstanta riwayat `ProcurementDocumentHistoryActions`.

Frontend:

- `procurement-attachment-panel.jsx`, `use-procurement-attachments.jsx`;
- `purchase-request-detail-view.jsx` (pola `ConfirmModal` dengan alasan);
- `vendor-document-type-view.jsx` (pola kartu form di atas daftar);
- `use-vendor-history.jsx` (pencarian vendor di server);
- `period-closing-view.jsx` (`selectUserInfo` untuk id pengguna);
- base component `confirm-modal.jsx`, `filter-date-picker.jsx`, `base-form-control.jsx`, `data-table.jsx`.

### 3.2 Berkas yang berubah

| Berkas (`V2QuilvianSystemFrontendDev`) | Status | Isi |
| --- | --- | --- |
| `src/lib/services/corporate/procurement/vendor-document-service.js` | Baru | Metadata, ringkasan, daftar, detail, riwayat, catat, verifikasi (`Idempotency-Key`), tolak, batal |
| `src/lib/services/corporate/procurement/vendor-service.js` | Diubah | `fetchVendorEligibility({ itemCategoryId, supplierId })` |
| `src/lib/hooks/corporate/procurement/use-vendor-document-list.jsx` | Baru | Daftar berhalaman, ringkasan, dan pilihan saringan untuk `-04`. Untuk tab vendor: `supplierId` dikunci, 100 baris, tanpa ringkasan |
| `src/lib/hooks/corporate/procurement/use-vendor-document-workspace.jsx` | Baru | Form catat; dokumen terpilih beserta riwayatnya; Verifikasi/Tolak/Batal; satu `Idempotency-Key` per niat verifikasi; penanda pengunggah dari `selectUserInfo`; `PBJ-VAL-090` dan `093` → keadaan basi |
| `src/lib/hooks/corporate/procurement/use-vendor-eligibility.jsx` | Baru | Satu panggilan `GET /vendors/eligibility` per kategori vendor |
| `src/components/view/corporate/procurement/vendors/vendor-document-workspace.jsx` | Baru | Kartu form catat; kartu dokumen terpilih (rincian, `ProcurementAttachmentPanel`, riwayat); tiga `ConfirmModal` |
| `src/components/view/corporate/procurement/vendors/vendor-document-list-view.jsx` | Baru | Layar `-04` dan pembentuk kolom yang dipakai bersama tab vendor |
| `src/components/view/corporate/procurement/vendors/vendor-document-tab.jsx` | Baru | Tab Dokumen `-02`: kelayakan per kategori, daftar dokumen vendor tanpa paginasi (`pagination={false}`), ruang kerja dokumen |
| `src/components/view/corporate/procurement/vendors/vendor-detail-view.jsx` | Diubah | Tab **Dokumen** (urutan arsitektur bagian 4.2) hanya bagi pemegang `ProcurementVendorDocument : Read`. Tab tanpa izin jatuh ke tab pertama. Perubahan dokumen memuat ulang detail vendor |
| `src/app/corporate/procurement/vendors/documents/page.jsx` | Baru | Entry point + `metadata` |
| `src/app/corporate/procurement/vendors/[slug]/page.jsx` | Diubah | `?tab=documents` diterima |
| `src/components/view/corporate/procurement/shared/procurement-status-badge.jsx` | Diubah | `kind` baru `vendorDocumentStatus` dan `vendorDocumentValidity`; bawaan tidak berubah |
| `src/lib/constants/corporate/procurement/procurement-constants.jsx` | Diubah | `VENDOR_DOCUMENT_STATUS` disamakan dengan label server ("Menunggu verifikasi"; Ditolak bernada `danger`); `VENDOR_DOCUMENT_VALIDITY`; label riwayat `VERIFY` "Diverifikasi" |
| `src/utils/corporate/procurement/procurement-utils.jsx` | Diubah | Fungsi murni `formatExpiryDistance` |

Tidak ada stylesheet baru. Kelas yang dipakai (`card`, `cardHeader`, `cardTitle`, `formGrid`, `summaryList`, `flagRow`, `unitList`, `actionRow`, `filterRow`, `muted`) sudah ada di `procurement.module.css`.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: `page.jsx` → view → hook → service `InstanceAxios`. Tidak ada Redux slice baru, sama dengan modul Pengadaan lain. Id pengguna dibaca lewat selector yang sudah ada, `selectUserInfo`.
- Ruang kerja dokumen (form + kartu dokumen) dipakai bersama oleh `-04` dan tab vendor, jadi tidak ada salinan kode.
- Validasi di layar hanya untuk isian kunci (vendor dan jenis). Aturan bisnis `PBJ-VAL-101`..`104` dibaca dari server (`PBJ-FE-003`).

### Keputusan base component (gerbang keputusan)

| Elemen layar | Status | Komponen / bukti |
| --- | --- | --- |
| Judul halaman `-04` | `REUSE` | `Hero` tanpa tombol (`PBJ-FE-004`) |
| Kartu ringkasan | `REUSE` | `SummaryGrid`, 5 kartu dari `GET /vendor-documents/summary` |
| Baris filter + "+ Catat Dokumen" | `REUSE` | `DataFilter` (`actions`) + `periodFilterControls` + `FilterSelect` ×3 + `pageSizeControl` |
| Tabel dan paginasi | `REUSE` | `DataTable` + `Pagination`; tab vendor tanpa paginasi lewat prop bawaan `pagination={false}` (pola yang sama di `nursing-soap-form.jsx`, `lab-catalog-picker.jsx`) |
| Isian form | `REUSE` | `BaseSelectField` (vendor `serverSide`, jenis dokumen), `BaseTextField`, `BaseDateField` |
| Konfirmasi Verifikasi, Tolak + alasan, Batal + alasan | `REUSE` | `ConfirmModal` (`requireReason`, `maxReasonLength` 1000) |
| Lampiran dokumen | `REUSE` | `ProcurementAttachmentPanel` `FE-PBJ-002` dengan `documentType = PBJ_VENDOR_DOCUMENT` dan `readOnly` |
| Riwayat dokumen | `REUSE` | `ProcurementHistoryPanel` dengan `statusLabel` |
| Notifikasi, galat, akses ditolak | `REUSE` | `ToastStack`, `InformationAlert`, `AccessDeniedGate` |
| Lencana status verifikasi dan keadaan berlaku | `EXTEND` (komponen bersama modul) | `ProcurementStatusBadge` diberi dua `kind` baru; bawaan tetap |
| Form catat dokumen | `COMPOSE` | Kartu form di atas daftar, pola `vendor-document-type-view.jsx` (`-11`) |
| Kartu dokumen terpilih | `COMPOSE` | `dl.summaryList` + `flagRow` + `BaseButton`, pola kartu identitas `-02` |
| Kelayakan per kategori | `COMPOSE` | `ul.unitList` + `StatusBadge` + teks alasan |
| Tab Dokumen | `COMPOSE` | `BaseButton role="tab"` yang sudah ada di `-02` |

`EXTEND` menyasar komponen milik modul Pengadaan di `procurement/shared/`, bukan base component. Prop-nya opsional dan perilaku bawaannya tidak berubah.

Pilihan untuk elemen bukan `REUSE`:

1. **Lencana status dokumen**
   1. `EXTEND` `ProcurementStatusBadge` dengan `kind` baru **(rekomendasi)** — satu komponen untuk semua status Pengadaan; risiko regresi rendah.
   2. Komponen lencana dokumen terpisah — menduplikasi pemetaan label dan nada.
2. **Form catat dokumen**
   1. `COMPOSE` kartu form di atas daftar **(rekomendasi)** — sama dengan form master data vendor `-11`..`-13`; dipakai ulang di tab vendor.
   2. Rute halaman form terpisah — rute baru di luar peta menu; pengguna meninggalkan daftar.
   3. `NEW` modal form — tidak ada base component modal form; butuh keputusan pengguna; tidak dikerjakan.
3. **Rincian dokumen beserta lampiran dan aksinya**
   1. `COMPOSE` kartu di atas daftar, terbuka saat baris dibuka **(rekomendasi)** — antrean dan rincian di satu layar; tanpa rute baru.
   2. Rute detail `/vendors/documents/{id}` — peta menu bagian 3 tidak mendaftarkan layar anak `-04`; menambah rute di luar rancangan.
   3. `NEW` drawer — tidak ada base component drawer di modul Pengadaan; tidak dikerjakan.
4. **Kelayakan per kategori**
   1. `COMPOSE` daftar kategori dengan lencana dan alasan **(rekomendasi)** — alasan bisa beberapa baris; mudah dibaca.
   2. `DataTable` — kolom alasan berbaris banyak sulit dibaca di tabel.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` pada base component.

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
| --- | --- |
| Memuat | `DataTable loading` "Mengambil dokumen legalitas..." / "Mengambil dokumen vendor..."; kartu dokumen "Memuat dokumen..."; kelayakan "Menghitung kelayakan..." |
| Kosong | Daftar "Belum ada dokumen pada saringan ini."; tab vendor "Belum ada dokumen."; kelayakan tanpa kategori "Vendor belum punya kategori. Atur kategori di tab Profil & Kategori."; lampiran "Belum ada lampiran."; riwayat "Belum ada riwayat." |
| Gagal | `InformationAlert` "Dokumen legalitas gagal dimuat." / "Dokumen vendor gagal dimuat." / "Kelayakan vendor gagal dimuat." / "Dokumen gagal dimuat." + **Coba lagi** |
| Tidak ditemukan | Kartu dokumen "Dokumen tidak ditemukan." tanpa Coba lagi |
| Akses ditolak | `-04` → `AccessDeniedGate`; tab Dokumen tidak tampil tanpa `ProcurementVendorDocument : Read`, dan `?tab=documents` jatuh ke Profil & Kategori |
| Validasi server | `PBJ-VAL-101` di form catat; `PBJ-VAL-102`, `103`, `104` di kartu dokumen dan toast. Modal Tolak/Batal tetap terbuka saat alasan ditolak supaya isian tidak hilang |
| Data basi | 409 `PBJ-VAL-090` (versi berubah) atau `PBJ-VAL-093` (sudah diputus petugas lain) → peringatan "Dokumen sudah diubah orang lain." + **Muat ulang** |
| Pengiriman ganda | Tombol aksi nonaktif saat memproses; `useProcurementAction` menolak aksi tumpang tindih; satu `Idempotency-Key` dipakai ulang sampai verifikasi berhasil |
| Penjaga orang yang sama | Verifikasi/Tolak tidak tampil bagi pengunggah; Batalkan hanya bagi pengunggah; server tetap menolak (`PBJ-FE-003`) |
| Hanya baca | Tanpa `Create`, tombol "+ Catat Dokumen" tidak tampil; tanpa `Verify`, tombol Verifikasi/Tolak tidak tampil; lampiran baca saja kecuali pengunggah dokumen yang masih menunggu verifikasi |

---

## 5. Endpoint yang dikonsumsi

Base `api/v1/corporate/procurement-management`, semua lewat `InstanceAxios` dengan `AbortController`.

| Method dan path | Dipakai di | Hak akses |
| --- | --- | --- |
| `GET /vendor-documents/filters/metadata` | Saringan `-04`; pilihan jenis di form | `ProcurementVendorDocument : Read` |
| `GET /vendor-documents/summary` | Ringkasan `-04` | `ProcurementVendorDocument : Read` |
| `GET /vendor-documents` (`search`, `documentTypeId`, `verificationStatus`, `validityState`, `supplierId`, `startDate`, `endDate`, `customPeriod`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize`) | `-04`; tab vendor | `ProcurementVendorDocument : Read` |
| `GET /vendor-documents/{id}`, `GET /vendor-documents/{id}/history` | Kartu dokumen | `ProcurementVendorDocument : Read` |
| `POST /vendor-documents` | Form catat | `ProcurementVendorDocument : Create` |
| `POST /vendor-documents/{id}/verify` + `Idempotency-Key` | Verifikasi | `ProcurementVendorDocument : Verify` |
| `POST /vendor-documents/{id}/reject` | Tolak | `ProcurementVendorDocument : Verify` |
| `POST /vendor-documents/{id}/cancel` | Batal oleh pengunggah | `ProcurementVendorDocument : Create` |
| `GET/POST /attachments`, `GET /attachments/{id}/download`, `POST /attachments/{id}/deactivate` | Lampiran (`FE-PBJ-002`) | `ProcurementAttachment : Read, Upload` + hak atas dokumen induk |
| `GET /vendors/eligibility?itemCategoryId=&supplierId=` | Kelayakan per kategori | `ProcurementVendor : Read` |
| `GET /vendors?search=&pageSize=25` | Pencarian vendor di form | `ProcurementVendor : Read` |

### Delta terhadap kontrak dan rancangan

| ID | Delta | Keputusan |
| --- | --- | --- |
| F-1 | Rincian dokumen tampil sebagai kartu di atas daftar, bukan rute anak | Peta menu bagian 3 tidak punya layar anak `-04`; rupa `DEV_DISCRETION` (bagian 4.3) |
| F-2 | Kelayakan tab vendor = satu panggilan per kategori | Kontrak hanya menyediakan kelayakan per kategori; tidak ada endpoint semua kategori satu vendor |
| F-3 | 409 `PBJ-VAL-093` diperlakukan sebagai data basi, sama dengan `PBJ-VAL-090` | Kode itu muncul ketika petugas lain sudah memutus dokumen yang sedang dibuka; arsitektur bagian 6 hanya menyebut `090` |
| F-4 | Saringan Keadaan berlaku hanya mengenai dokumen terverifikasi | Perilaku server (`BE-PBJ-VD-004` D-1); dijelaskan di deskripsi baris filter |
| F-5 | Tab Dokumen disembunyikan tanpa `ProcurementVendorDocument : Read` | Arsitektur bagian 4.2 tidak mengatur; tab tanpa izin hanya akan berisi 403 |
| F-6 | Unggah berkas butuh `ProcurementAttachment : Read, Upload` selain izin dokumen | Perilaku lampiran bersama `FE-PBJ-002`. Peta peran perlu memberi Tim Pengadaan kedua izin itu. Dicatat untuk pemilik akses |
| F-7 | Lima kartu ringkasan: Menunggu Verifikasi, Terverifikasi, Ditolak, Kedaluwarsa, Segera Kedaluwarsa | Mengikuti isi `VendorDocumentSummaryCountsResponse` |

---

## 6. Verifikasi

Uji layar memakai Playwright terhadap `next dev` `localhost:3000`. Panggilan `https://localhost:7184` dialihkan ke build backend scratch `https://localhost:7185` dengan database `QuilvianNewDevSukma`.

- Putaran 1 (18 November 2026) memakai dev server milik pengguna.
- Putaran 2 (19 November 2026): dev server 3000 dan backend 7184 milik pengguna sedang mati. Karena itu `next dev` 3000 dan backend uji 7185 dinyalakan sementara untuk uji ini, lalu dimatikan lagi.

Aktor non-SuperAdmin `TEST-PBJ … FEVD003`:

| Aktor | Izin |
| --- | --- |
| `up` (Tim Pengadaan, pengunggah) | `ProcurementVendor : Read, Update` + `ProcurementVendorDocument : Read, Create, Verify` + `ProcurementAttachment : Read, Upload` |
| `ver` (Tim Pengadaan, pemeriksa) | Sama dengan `up` |
| `ro` | `ProcurementVendor : Read` + `ProcurementVendorDocument : Read` + `ProcurementAttachment : Read` |
| `nodoc` | `ProcurementVendor : Read` |

Data uji dibuat dengan bentuk yang sama di run `20261118154916` (putaran 1) dan `20261119093556` (putaran 2):

- Jenis dokumen TA (wajib tanggal berlaku) dan TB (tanpa tanggal berlaku).
- Kategori K1 dengan kewajiban TA aktif, dan K2 tanpa kewajiban.
- Supplier S1 (K1 + K2, tanpa dokumen).
- Supplier S2 (K1) dengan empat dokumen:
  - izin TA terverifikasi yang berakhir kemarin;
  - dokumen TB tanpa berkas;
  - dua dokumen TB berberkas yang menunggu verifikasi.

Skrip: `ui_fe_vd003.cjs` (uji layar) dan `fevd003_api_check.py` (cek API) di scratchpad sesi; tangkapan layar di `shots-fevd003/`.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 8 berkas dokumen vendor, diulang 19 November 2026 sesudah perbaikan tab | 0 error; 2 warning `react-hooks/set-state-in-effect` (pola muat-data yang sama dengan hook lain) | `PASS` | keluaran perintah |
| `npm run lint:errors` (seluruh repo), 18 November 2026 | exit 0 | `PASS` | `fevd003-lint.log` |
| `npx next build`, diulang 19 November 2026 sesudah perbaikan tab, berdampingan dengan `next dev` | `Compiled successfully in 100s`, exit 0; 33 rute `/corporate/procurement/**` termasuk `/vendors/documents` | `PASS` | `build-fevd003c.log` |
| Grep anti-regresi pada berkas baru: warna literal, `style=`, `px`/`fontSize`, elemen HTML mentah, axios langsung, stylesheet baru | 0 temuan | `PASS` | keluaran perintah |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — test-policy: test baru tidak wajib | `NOT RUN` | — |
| Uji layar putaran 1 (`UI-01`..`UI-16`), 18 November 2026 | 6 `PASS`, 9 `FAIL` (semua berasal dari skrip atau beruntun), 1 tidak jalan | `PARTIAL` (riwayat) | `ui-fevd003.results.run1.json` |
| Basis data dev tidak terjangkau | 18 November 2026 15:56:54 sampai 19 November 2026 pagi (`Npgsql … Failed to connect`; ping dan TCP gagal, internet normal). Putaran 2 tertahan; dua penunggu otomatis habis waktu | Riwayat | `be7185-fevd003.log` |
| Uji layar putaran 2, 19 November 2026 | 15 `PASS`, 1 `FAIL` (`UI-12`, skrip) | `PARTIAL` | `ui-fevd003.results.run2.json` |
| `UI-12` dan `UI-14` diulang sesudah perbaikan skrip dan tab | 2/2 `PASS` | `PASS` | `ui-fevd003.results.run2-ui12.json` |
| Cek API `API-01`..`04` sesudah uji layar | 4/4 `PASS` | `PASS` | `fevd003_api.results.json` |

### Temuan putaran 2

| Temuan | Penyebab | Tindakan |
| --- | --- | --- |
| `UI-12` gagal walaupun layar benar | Skrip mencari "tidak layak" di seluruh kartu kelayakan. Kalimat keterangan kartu memang memuat "Vendor tidak layak tidak dapat dipilih …" | Skrip memeriksa potongan per kategori saja |
| Tabel tab Dokumen menampilkan "Menampilkan 0 sampai 0 dari 0 data" dan tombol halaman walaupun ada baris | Cacat frontend: `DataTable` memasang paginasi secara bawaan (`pagination = true`), sedangkan tab memuat sampai 100 dokumen tanpa total halaman | `pagination={false}` di `vendor-document-tab.jsx`; `UI-12` kini juga memeriksa bahwa teks paginasi tidak tampil |

### Rincian putaran 2

| Skenario | Hasil | Bukti |
| --- | --- | --- |
| `UI-01` Menu tampil; hero tanpa tombol; 5 kartu; 9 kontrol filter satu baris (`AT-VD-63`); "+ Catat Dokumen" di baris filter | `PASS` | `rows: 1`, `heroButtons: 0` |
| `UI-02` Saringan: cari run → 4 dokumen; Status Menunggu → 3; Keadaan Kedaluwarsa + Jenis TA → 1; Atur ulang | `PASS` | 4 / 3 / 1 baris |
| `UI-03` `AT-VD-10`: Izin tanpa tanggal berlaku, lalu berlaku sebelum terbit | `PASS` | 2× 422 `PBJ-VAL-101`; pesan server tampil di form |
| `UI-04` Catat Izin berlaku 20 hari lagi + unggah PDF; pengunggah tanpa Verifikasi/Tolak (`AT-VD-12`) | `PASS` | 201; tombol `Batalkan`, `Buka vendor`, `Tutup`; unggah 200 |
| `UI-05` S1 sebelum verifikasi: K1 Tidak layak "belum terverifikasi"; K2 Layak (`AT-VD-14`) | `PASS` | Teks per kategori |
| `UI-06` Pemeriksa: Verifikasi + Tolak tampil, Batalkan tidak, lampiran baca saja; Verifikasi → Terverifikasi + Segera kedaluwarsa (`AT-VD-13`, `16`) | `PASS` | 200 dengan `Idempotency-Key` |
| `UI-07` Ringkasan Segera Kedaluwarsa bertambah; baris "20 hari lagi" (`AT-VD-16`) | `PASS` | 0 → 1 |
| `UI-08` `AT-VD-11`: verifikasi tanpa berkas | `PASS` | 422 `PBJ-VAL-102`; status tetap Menunggu verifikasi |
| `UI-09` Tolak: alasan pendek, lalu alasan sah (`AT-VD-13`) | `PASS` | 422 `PBJ-VAL-104` dengan modal tetap terbuka; lalu 200 Ditolak |
| `UI-10` `AT-VD-15`: S2 K1 Tidak layak "kedaluwarsa sejak 18-11-2026"; kepala "Ada yang kedaluwarsa" | `PASS` | Teks kelayakan dan lencana |
| `UI-11` Data basi: diputus dari tab lain, lalu Verifikasi | `PASS` | 409 `PBJ-VAL-093` → peringatan + Muat ulang → Ditolak |
| `UI-12` S1 sesudah verifikasi: K1 Layak + "berakhir dalam 20 hari (09-12-2026)"; K2 Layak; kepala "Segera kedaluwarsa"; tanpa paginasi | `PASS` (ulang) | `12-tab-after.png` |
| `UI-13` Catat dari tab (vendor terkunci) lalu Batalkan oleh pengunggah | `PASS` | 201, lalu 200 Dibatalkan |
| `UI-14` Tanpa `pageerror` sesi `up` dan `ver` | `PASS` | 0 |
| `UI-15` Baca saja: tanpa "+ Catat Dokumen"; tanpa Verifikasi/Tolak/Batalkan; lampiran bisa Unduh, tidak bisa unggah | `PASS` | tombol `Buka vendor`, `Tutup` |
| `UI-16` Tanpa `ProcurementVendorDocument`: tab Dokumen tidak ada, `?tab=documents` jatuh ke Profil; menu tidak tampil; `-04` Akses Ditolak | `PASS` | `docTab: 0`, `menu: false` |

### Cek API

| ID | Skenario | Hasil |
| --- | --- | --- |
| `API-01` | Pengunggah memverifikasi dokumen sendiri yang belum berberkas | 403 `PBJ-VAL-103`: penjaga orang yang sama diperiksa sebelum aturan berkas |
| `API-02` | Pengunggah memverifikasi dokumen sendiri yang sudah berberkas | 403 `PBJ-VAL-103` |
| `API-03` | Dokumen `API-02` dibatalkan pengunggah | 200 Dibatalkan |
| `API-04` | `AT-VD-15`: `MstSupplier.IsBlacklisted` S1 dan S2 sesudah uji | `false` untuk keduanya; izin kedaluwarsa tidak mengubah daftar hitam |

Uji manual: seluruh jalur di atas dijalankan lewat layar dengan empat aktor.

**Tidak dijalankan:** tes unit baru (opsional); `test:uat` (dilarang repository); lebar layar selain 1440 px.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-10` `PBJ-VAL-101` terbaca dari server | Terpenuhi | `UI-03` |
| `AT-VD-11` `PBJ-VAL-102` terbaca dari server | Terpenuhi | `UI-08` (putaran 1 dan 2) |
| `AT-VD-12` tombol Verifikasi tersembunyi bagi pengunggah; `PBJ-VAL-103` | Terpenuhi | `UI-04`; `API-01`, `API-02` |
| `AT-VD-13` verifikasi oleh petugas lain; tolak dengan alasan | Terpenuhi | `UI-06`, `UI-09` |
| `AT-VD-14` kategori tanpa kewajiban → layak | Terpenuhi | `UI-05`, `UI-12` |
| `AT-VD-15` izin kedaluwarsa → tidak layak dengan alasan; vendor tidak otomatis masuk daftar hitam | Terpenuhi | `UI-10`; `API-04` |
| `AT-VD-16` "Segera kedaluwarsa" + masuk ringkasan | Terpenuhi | `UI-06`, `UI-07`, `UI-12` |
| `AT-VD-63` satu baris filter `-04` | Terpenuhi | `UI-01` |
| Hak akses: baca saja dan tanpa izin dokumen | Terpenuhi | `UI-15`, `UI-16` |
| Data basi dan pengiriman ganda | Terpenuhi | `UI-11`; `Idempotency-Key` di `UI-06` |
| DoD: lint dan build | Terpenuhi | Tabel verifikasi |
| DoD: laporan ini dan status roadmap | Terpenuhi | Berkas ini; roadmap dan traceability ditandai ✅ |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status task | ✅ Selesai 19 November 2026 |
| Riwayat blocker | Basis data dev `QuilvianNewDevSukma` tidak terjangkau sejak 18 November 2026 15:56:54. Pulih 19 November 2026 pagi; data dan migration utuh |
| Data uji | Dua run di `QuilvianNewDevSukma`, masing-masing: 1 unit, 4 jabatan, 4 akun, policy, 2 jenis dokumen, 2 kategori + 1 kewajiban, 2 supplier, profil vendor + kategori, dokumen + berkas PDF uji. Run `20261118154916` punya 4 dokumen. Run `20261119093556` punya 7 dokumen, termasuk dokumen uji `UI-04`, `UI-13`, dan `API-02`. Id di `fevd003.fixture.run1.json` dan `fevd003.fixture.run2.json` scratchpad. **Sudah dibersihkan 19 November 2026** atas izin pemilik ("lanjutkan" atas tawaran pembersihan): dry run lalu apply `cleanup_vendor_fixture.py` untuk kedua run, satu transaksi. Dokumen, lampiran, kewajiban, jenis dokumen, kategori, profil, supplier, policy, pegawai, jabatan, dan unit dihapus lunak; 8 akun dinonaktifkan + lockout. Dry run ulang: tidak ada sisa. Riwayat dan berkas PDF uji (45 byte) dibiarkan |
| Risiko tersisa | Peta peran perlu memberi Tim Pengadaan `ProcurementAttachment : Read, Upload` (F-6), tanpa itu pengunggah tidak bisa mengunggah berkas. Kelayakan tab vendor = N panggilan untuk N kategori (F-2). Tab Dokumen menampilkan paling banyak 100 dokumen per vendor |
| Dependency backend | Tidak ada perubahan backend pada task ini |
| Task berikutnya | `FE-PBJ-VD-004` (dirilis bersama `BE-PBJ-VD-007`) atau `FE-PBJ-VD-005` |
