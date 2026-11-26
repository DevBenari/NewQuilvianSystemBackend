# Laporan Perubahan Frontend — `FE-PBJ-VD-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-VD-004` |
| Judul | Status vendor dan usulan |
| Slice | Gelombang 3 roadmap frontend `manajemen-vendor` revisi 1 |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | Layar `FE-PBJ-VD-06`, `FE-PBJ-VD-07`, tab Status `FE-PBJ-VD-02`, layar Master Data Supplier; FR-VD-030, 031, 032; `PBJ-DEC-021`, `047`, `060`; `PBJ-ASM-003`; `PBJ-FE-003`, `004` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor Status Request, restriction/lift, §Supplier. Delta yang dipakai: `BE-PBJ-VD-006` D-1..D-5, `BE-PBJ-VD-007` D-1..D-7, dan delta baru `BE-PBJ-VD-003` D-10 (`vendorStatus=NON_ACTIVE`) |
| Wewenang UI | `PBJ-FE-001` nama menu SOURCE; `PBJ-FE-004` standar layar daftar; `PBJ-FE-003` invariant (tombol tersembunyi bukan penjaga; server tetap memutus); rupa `-06` dua tab dan form `-07` `DEV_DISCRETION` (arsitektur frontend bagian 4.4) |
| Dependency | `FE-PBJ-VD-002` ✅, `BE-PBJ-VD-006` ✅, `BE-PBJ-VD-007` ✅; pengerjaan ulang `BE-PBJ-VD-003` (D-10) ✅ 19 November 2026 atas keputusan pemilik |
| Klasifikasi | `MEDIUM` — 2 layar baru, 1 tab baru, 2 hook baru, 1 service baru, perluasan 2 hook dan 2 service, 3 berkas konfigurasi Master Data Supplier |
| Task mode | `FRONTEND` (instruksi pengguna "lanjutkan", 19 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `b1b360efa` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `a6df5619` (branch `sukmagp`, berisi `BE-PBJ-VD-001`..`008` dan D-10, belum di-commit) |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — lint dan build `PASS`; uji layar 16/16 `PASS`; cek API/DB 5/5 `PASS` |

---

## 1. Keadaan yang ditemukan di awal

Menu "Status" (`/corporate/procurement/vendors/status`) sudah ada sejak `FE-PBJ-VD-001`, tetapi rutenya masih 404. Detail Vendor belum punya tab Status, jadi Batasi/Cabut dan usulan blokir belum bisa dikerjakan dari layar.

Backend sudah lengkap:

- `BE-PBJ-VD-006`: `POST /vendors/{id}/restriction` dan `/restriction/lift`.
- `BE-PBJ-VD-007`: usulan blokir/buka blokir (`/vendor-status-requests`), linimasa `GET /{id}/approval`, dan Master Data Supplier yang mengabaikan `isBlacklisted`.

Ada tiga temuan awal:

1. `GET /vendors` hanya menerima satu nilai `vendorStatus`. Daftar "vendor non-Aktif" di `-06` tidak bisa dimuat dalam satu panggilan dengan paginasi yang benar. Pemilik memilih "Perluas backend", lalu `BE-PBJ-VD-003` dikerjakan ulang menjadi delta D-10 `vendorStatus=NON_ACTIVE`.
2. Form Master Data Supplier memakai field `isBlacklistedSupplier`, sedangkan backend memakai `isBlacklisted`. Centang Blacklist di layar itu tidak pernah terbaca maupun tersimpan, bahkan sebelum `PBJ-DEC-060`.
3. Kotak masuk Pengadaan (`FE-PBJ-005`) belum mengenal jenis dokumen `PBJ_VENDOR_STATUS`, sehingga penyetuju tidak bisa menyaring usulan status vendor di sana.

---

## 2. Proses bisnis dari sisi pengguna

1. **Memantau.** Manajer Pengadaan membuka **Status** (`-06`). Tab **Vendor Tidak Aktif** langsung memuat vendor Dibatasi, Diblokir, dan Nonaktif di Master Data (saringan bawaan "Semua selain Aktif"). Tab **Usulan** memuat usulan blokir/buka blokir beserta statusnya.
2. **Membatasi.** Dari Detail Vendor → tab **Status**, Manajer menekan **Batasi**, mengisi alasan dan, bila perlu, tanggal berakhir. Vendor tampil **Dibatasi** dan tetap bisa dipilih dengan catatan. **Cabut pembatasan** meminta alasan minimal 10 karakter.
3. **Mengusulkan blokir.** Dari tab Status, **Usulkan blokir** membuka `-07` dengan vendor terkunci dan jenis Blokir. Manajer menulis alasan, bila perlu memilih penilaian final sebagai bukti, lalu **Simpan draf**. Bila masih ada usulan terbuka untuk vendor itu, server menolak (`PBJ-VAL-107`), dan tombol Usulkan disembunyikan disertai keterangan.
4. **Mengajukan.** **Ajukan** mengirim usulan ke alur persetujuan dan memberi nomor `UBV`. Bila alur persetujuan belum dikonfigurasi, pesan server (`PBJ-VAL-091`) tampil di modal dan usulan tetap Draf.
5. **Memutus.** Penyetuju membuka **Antrean Persetujuan** Pengadaan, memilih jenis **Usulan Status Vendor**, lalu **Setujui**. Vendor menjadi **Diblokir**. Pengusul tidak mendapat tugas atas usulannya sendiri.
6. **Menarik.** Selama menunggu persetujuan, pengusul dapat **Tarik** dengan alasan; usulan menjadi Dibatalkan.
7. **Master Data.** Di layar Master Data Supplier, centang **Diblokir (Pengadaan)** hanya bisa dibaca. Keterangannya menunjuk ke Pengadaan → Status, dan nilainya tidak ikut dikirim saat menyimpan.

Contoh: Rina (Manajer) membatasi "Alkes Maju" sampai 20-11-2026 karena terlambat kirim. Seminggu kemudian dia mencabutnya, lalu mengusulkan blokir karena "Tiga NCR kritis dalam enam bulan terakhir." Pak Budi (Direktur) menyetujui `UBV-000012` dari Antrean Persetujuan. Alkes Maju menjadi Diblokir, dan admin Master Data melihat centang Diblokir tercentang tetapi tidak bisa mengubahnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` bagian 2–7; `contracts/api-contract.md` §Vendor Status Request, restriction, §Supplier; `testing/acceptance-test-matrix.md` `AT-VD-20`..`29`; laporan `BE-PBJ-VD-006`, `BE-PBJ-VD-007`.

Backend:

- `VendorStatusDtos.cs`, `VendorStatusRequestsController.cs`, `ProcurementVendorStatusService.cs`;
- `VendorDtos.cs`, `VendorsController.cs`, `ProcurementVendorProfileService.cs`;
- `SupplierDtos.cs`, `SupplierController.cs` (aturan blokir dan pajak).

Frontend:

- `purchase-request-detail-view.jsx` (pola detail + hero + `ConfirmModal` alasan);
- `procurement-document-tabs.jsx`, `procurement-approval-panel.jsx`, `procurement-approval-service.js`;
- `procurement-approval-inbox-view.jsx` (jenis dokumen kotak masuk);
- `vendor-detail-view.jsx`, `use-vendor-detail.jsx`, `vendor-list-view.jsx`, `use-vendor-list.jsx`;
- Master Data Supplier: `administrator-supplier-form-view.jsx`, `supplier-editor-config.jsx`, `supplier-grouped-form.config.jsx`, `supplier-editor-utils.jsx`, `supplier-detail-utils.jsx`, `base-grouped-editor-field.jsx`.

### 3.2 Berkas yang berubah

| Berkas (`V2QuilvianSystemFrontendDev`) | Status | Isi |
| --- | --- | --- |
| `src/lib/services/corporate/procurement/vendor-status-request-service.js` | Baru | Metadata, ringkasan, daftar, detail, riwayat, buat, ubah, ajukan (`Idempotency-Key`), batal/tarik, sinkron ulang |
| `src/lib/services/corporate/procurement/vendor-service.js` | Diubah | `restrictVendor` (`Idempotency-Key`), `liftVendorRestriction`, `fetchFinalVendorEvaluations` (bukti usulan) |
| `src/lib/services/corporate/procurement/procurement-approval-service.js` | Diubah | Linimasa persetujuan untuk `PBJ_VENDOR_STATUS` |
| `src/lib/hooks/corporate/procurement/use-vendor-status-request-list.jsx` | Baru | Daftar usulan `-06` (ringkasan, pilihan saringan) atau usulan satu vendor untuk tab Status (dikunci `supplierId`, tanpa ringkasan) |
| `src/lib/hooks/corporate/procurement/use-vendor-status-request-detail.jsx` | Baru | Form/detail `-07`: buat, simpan draf, ajukan (menyimpan perubahan dulu bila ada), tarik/batal, sinkron; penanda pengusul dari `selectUserInfo`; `PBJ-VAL-090`/`093` → data basi; bukti penilaian hanya dimuat saat isian terbuka |
| `src/lib/hooks/corporate/procurement/use-vendor-detail.jsx` | Diubah | Form Batasi, aksi Batasi (satu `Idempotency-Key` per niat) dan Cabut; respons dipakai sebagai detail terbaru |
| `src/lib/hooks/corporate/procurement/use-vendor-list.jsx` | Diubah | `defaultFilters` opsional (saringan awal sekaligus nilai Atur ulang) |
| `src/components/view/corporate/procurement/vendors/vendor-status-view.jsx` | Baru | Layar `-06`: tab Vendor Tidak Aktif (3 kartu, saringan Status bawaan "Semua selain Aktif") dan tab Usulan (6 kartu, "+ Usulan") |
| `src/components/view/corporate/procurement/vendors/vendor-status-request-detail-view.jsx` | Baru | Layar `-07`: hero Ajukan/Tarik/Batalkan/Sinkronkan, isian atau ringkasan usulan, tab Lampiran/Persetujuan/Riwayat |
| `src/components/view/corporate/procurement/vendors/vendor-status-tab.jsx` | Baru | Tab Status `-02`: status beserta alasan, Batasi/Cabut, Usulkan blokir/buka blokir, daftar usulan vendor |
| `src/components/view/corporate/procurement/vendors/vendor-detail-view.jsx` | Diubah | Tab **Status** (urutan arsitektur bagian 4.2); galat Batasi/Cabut tampil di form dan modal tab Status |
| `src/app/corporate/procurement/vendors/status/page.jsx` | Baru | Entry `-06` (`?tab=requests`) |
| `src/app/corporate/procurement/vendors/status/new/page.jsx` | Baru | Entry `-07` usulan baru (`?supplierId=&requestType=` dari tab Status) |
| `src/app/corporate/procurement/vendors/status/[slug]/page.jsx` | Baru | Entry `-07` detail; token bukan GUID → 404 |
| `src/app/corporate/procurement/vendors/[slug]/page.jsx` | Diubah | `?tab=status` diterima |
| `src/components/view/corporate/procurement/shared/procurement-status-badge.jsx` | Diubah | `kind` baru `vendorStatusRequest`; bawaan tidak berubah |
| `src/lib/constants/corporate/procurement/procurement-constants.jsx` | Diubah | `VENDOR_STATUS_REQUEST_TYPE`; jenis kotak masuk `PBJ_VENDOR_STATUS` "Usulan Status Vendor" dengan rute `-07` |
| `src/lib/constants/administrator/master-data/supplier/supplier-editor-config.jsx` | Diubah | Field `isBlacklistedSupplier` → `isBlacklisted` "Diblokir (Pengadaan)", `submit: false`, keterangan baca saja |
| `src/lib/constants/administrator/master-data/supplier/supplier-grouped-form.config.jsx` | Diubah | `isBlacklisted` selalu nonaktif dengan alasan "Baca saja. Blokir dan buka blokir diajukan lewat Pengadaan → Data Penyedia/Vendor → Status, lalu berlaku setelah disetujui." |
| `src/utils/administrator/master-data/supplier/supplier-detail-utils.jsx` | Diubah | Detail supplier: "Diblokir (Pengadaan)" dan "Alasan Blokir" dari `isBlacklisted`/`blacklistReason` |

Tidak ada stylesheet baru. Kelas yang dipakai (`card`, `cardHeader`, `cardTitle`, `formGrid`, `summaryList`, `flagRow`, `actionRow`, `filterRow`, `tabs`, `muted`) sudah ada di `procurement.module.css`.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: `page.jsx` → view → hook → service `InstanceAxios`. Tidak ada Redux slice baru, sama dengan modul Pengadaan lain.
- Tab Persetujuan, Lampiran, dan Riwayat `-07` memakai `ProcurementDocumentTabs` yang sama dengan PR dan rencana. Tidak ada salinan panel.
- Validasi di layar hanya untuk isian kunci (vendor dan jenis usulan). `PBJ-VAL-091`, `093`, `104`..`107` dibaca dari server (`PBJ-FE-003`).
- Perubahan Master Data Supplier hanya pada konfigurasi field yang sudah ada. `BaseGroupedEditorView` dan perilakunya tidak diubah.

### Keputusan base component (gerbang keputusan)

| Elemen layar | Status | Komponen / bukti |
| --- | --- | --- |
| Judul `-06` | `REUSE` | `Hero` tanpa tombol (`PBJ-FE-004`) |
| Judul `-07` dengan aksi | `REUSE` | `Hero` + `actions`, pola `purchase-request-detail-view.jsx` |
| Kartu ringkasan | `REUSE` | `SummaryGrid` (3 kartu vendor, 6 kartu usulan) |
| Baris filter + "+ Usulan" | `REUSE` | `DataFilter` (`actions`) + `periodFilterControls` + `FilterSelect` + `pageSizeControl` |
| Tabel | `REUSE` | `DataTable` + `Pagination`; tab Status tanpa paginasi (`pagination={false}`) |
| Isian | `REUSE` | `BaseSelectField` (vendor `serverSide`, jenis, bukti), `BaseTextAreaField`, `BaseDateField` |
| Konfirmasi Ajukan/Tarik/Batalkan/Cabut | `REUSE` | `ConfirmModal` (`requireReason`, `children` untuk pesan server di dalam modal) |
| Lampiran, Persetujuan, Riwayat `-07` | `REUSE` | `ProcurementDocumentTabs` `FE-PBJ-002`/`005` |
| Notifikasi, galat, akses ditolak | `REUSE` | `ToastStack`, `InformationAlert`, `AccessDeniedGate` |
| Centang Diblokir Master Data | `REUSE` | Field konfigurasi + `getFieldDisabled`/`getDisabledReason` bawaan `BaseGroupedEditorView` |
| Lencana status usulan | `EXTEND` (komponen bersama modul) | `ProcurementStatusBadge` diberi `kind` `vendorStatusRequest`; bawaan tetap |
| Dua daftar `-06` | `COMPOSE` | `BaseButton role="tab"` yang sudah ada di `-02` |
| Kartu Status + form Batasi | `COMPOSE` | `dl.summaryList` + `BaseButton` + kartu form, pola kartu `-02` dan form `-04` |

`EXTEND` menyasar komponen milik modul Pengadaan di `procurement/shared/`, bukan base component. Prop-nya opsional dan perilaku bawaannya tidak berubah.

Pilihan untuk elemen bukan `REUSE`:

1. **Lencana status usulan**
   1. `EXTEND` `ProcurementStatusBadge` dengan `kind` baru **(rekomendasi)** — satu komponen untuk semua status Pengadaan; risiko regresi rendah.
   2. Lencana terpisah — menduplikasi pemetaan label dan nada.
2. **Dua daftar `-06`**
   1. `COMPOSE` dua tab **(rekomendasi)** — satu layar, saringan masing-masing tidak bercampur; pola tab `-02`.
   2. Dua bagian bertumpuk — dua baris filter di satu halaman, melanggar kesan "satu baris filter per daftar" `PBJ-FE-004`.
3. **Kartu Status dan form Batasi**
   1. `COMPOSE` kartu di tab Status **(rekomendasi)** — alasan dan status terlihat saat bertindak; tanggal berakhir memakai `BaseDateField` biasa.
   2. `ConfirmModal` dengan tanggal di dalamnya — pemilih tanggal di dalam modal berisiko tumpang tindih lapisan.
   3. Tombol di hero `-02` sesuai skema bagian 4.2 — hero sudah memuat "Atur kategori"; aksi status lebih jelas di dekat alasannya. Dicatat sebagai delta F-2.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` pada base component.

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
| --- | --- |
| Memuat | `DataTable loading` "Mengambil vendor..." / "Mengambil usulan..." / "Mengambil usulan vendor..."; `-07` "Memuat usulan..." |
| Kosong | `-06` "Tidak ada vendor pada saringan ini." / "Belum ada usulan pada saringan ini."; tab Status "Belum ada usulan."; bukti "Belum ada penilaian final untuk vendor ini."; Persetujuan "Belum masuk persetujuan." |
| Gagal | `InformationAlert` "Daftar vendor gagal dimuat." / "Usulan gagal dimuat." / "Usulan vendor gagal dimuat." + **Coba lagi**; `-07` galat + Coba lagi |
| Tidak ditemukan | `-07` "Usulan tidak ditemukan." tanpa Coba lagi; token bukan GUID → 404 |
| Akses ditolak | `-06` tab Usulan tidak tampil tanpa `ProcurementVendorStatusRequest : Read`; `-07` baru tanpa `Create` → Akses Ditolak; 403 daftar → `AccessDeniedGate` |
| Validasi server | `PBJ-VAL-105` di form Batasi; `PBJ-VAL-104` di modal Cabut dan Tarik (modal tetap terbuka); alasan wajib, `PBJ-VAL-107` di `-07`; `PBJ-VAL-091` di modal Ajukan |
| Data basi | 409 `PBJ-VAL-090`/`093` → "sudah diubah orang lain" + **Muat ulang** (detail vendor dan `-07`) |
| Pengiriman ganda | Tombol nonaktif saat memproses; satu `Idempotency-Key` per niat Batasi dan per niat Ajukan, dipakai ulang sampai berhasil |
| Penjaga orang yang sama | Ubah/Ajukan/Tarik hanya untuk pengusul; pengusul tidak mendapat tugas persetujuan atas usulannya; server tetap menolak (`PBJ-FE-003`) |
| Hanya baca | Tanpa `Restrict`/`Create`: kartu Status tanpa tombol; `-06` tanpa "+ Usulan"; `-07` hanya **Kembali** |

---

## 5. Endpoint yang dikonsumsi

Base `api/v1/corporate/procurement-management`, semua lewat `InstanceAxios` dengan `AbortController`.

| Method dan path | Dipakai di | Hak akses |
| --- | --- | --- |
| `GET /vendors?vendorStatus=NON_ACTIVE` (delta D-10), `GET /vendors/summary`, `GET /vendors/filters/metadata` | `-06` tab Vendor Tidak Aktif | `ProcurementVendor : Read` |
| `POST /vendors/{id}/restriction` + `Idempotency-Key`, `POST /vendors/{id}/restriction/lift` | Tab Status `-02` | `ProcurementVendor : Restrict` |
| `GET /vendor-status-requests/filters/metadata`, `/summary`, `GET /vendor-status-requests` | `-06` tab Usulan; tab Status (`supplierId`) | `ProcurementVendorStatusRequest : Read` |
| `GET /vendor-status-requests/{id}`, `/{id}/history`, `/{id}/approval` | `-07` | `ProcurementVendorStatusRequest : Read` |
| `POST /vendor-status-requests`, `PUT /{id}`, `POST /{id}/submit` + `Idempotency-Key`, `POST /{id}/workflow/synchronize` | `-07` | `ProcurementVendorStatusRequest : Create` (ubah/ajukan hanya pengusul) |
| `POST /vendor-status-requests/{id}/cancel` | `-07` Batalkan/Tarik | `ProcurementVendorStatusRequest : Cancel` |
| `GET /vendor-evaluations?supplierId=&status=FINAL` | Pilihan bukti `-07` | `ProcurementVendorEvaluation : Read` |
| `GET/POST /attachments` (`PBJ_VENDOR_STATUS`) | Tab Lampiran `-07` | `ProcurementAttachment : Read, Upload` + hak atas usulan |
| `GET /v1/corporate/human-resource/approval-inbox?referenceType=PBJ_VENDOR_STATUS`, `POST …/{assignmentId}/approve` | Antrean Persetujuan Pengadaan (penyetuju) | `ApprovalInbox : Read, Approve` |
| `GET`/`PUT api/v1/administrator/master-data/suppliers/{id}` | Master Data Supplier | `Supplier : Read, Update` (tidak berubah) |

### Delta terhadap kontrak dan rancangan

| ID | Delta | Keputusan |
| --- | --- | --- |
| F-1 | Daftar vendor non-Aktif memakai `vendorStatus=NON_ACTIVE` | Delta backend D-10 (`BE-PBJ-VD-003` pengerjaan ulang 19 November 2026) atas keputusan pemilik "Perluas backend"; usulan `PBJ-VD-API` 1.1 |
| F-2 | Batasi/Cabut dan Usulkan blokir ada di kartu tab Status, bukan di hero `-02` | Skema bagian 4.2 mengunci isi dan sumber data, sedangkan letak tombol `DEV_DISCRETION`. Aksi diletakkan dekat alasan dan status |
| F-3 | `-07` baru memakai rute `/vendors/status/new` (pola `requests/new`) | Peta menu bagian 3 menyebut `-07` layar anak `-06` dan tombol di `-02`; rute anak tidak menambah butir menu |
| F-4 | Jenis kotak masuk `PBJ_VENDOR_STATUS` "Usulan Status Vendor" ditambahkan ke Antrean Persetujuan Pengadaan | Kontrak: "keputusan dikerjakan penyetuju di kotak masuk `Wfl` yang sudah ada". Tanpa jenis ini, penyetuju harus memakai Kotak Masuk Terpadu |
| F-5 | Ajukan menyimpan perubahan alasan/bukti lebih dulu bila ada | Supaya isi yang diajukan sama dengan yang terlihat |
| F-6 | Tab Lampiran `-07` butuh `ProcurementAttachment : Read` | Penyedia akses `PBJ_VENDOR_STATUS` (`BE-PBJ-VD-007` D-6). Penyetuju dan pembaca usulan perlu izin itu untuk melihat bukti. Dicatat untuk peta peran |
| F-7 | Field `isBlacklistedSupplier` Master Data diganti `isBlacklisted` | Nama lama tidak cocok dengan backend, jadi centang lama tidak pernah berfungsi. Kini terbaca dengan benar dan tidak dikirim |

---

## 6. Verifikasi

Uji layar memakai Playwright terhadap `next dev` `localhost:3000`. Panggilan `https://localhost:7184` dialihkan ke build backend scratch `https://localhost:7185` (berisi D-10) dengan database `QuilvianNewDevSukma`. Dev server 3000 dan backend 7184 milik pengguna sedang mati, jadi keduanya dinyalakan sementara untuk uji ini lalu dimatikan lagi.

Aktor non-SuperAdmin `TEST-PBJ … FEVD004`:

| Aktor | Izin |
| --- | --- |
| `mgr` (Manajer Pengadaan) | `ProcurementVendor : Read, Update, Restrict` + `ProcurementVendorStatusRequest : Read, Create, Cancel` + `ProcurementAttachment : Read, Upload` + `ProcurementVendorEvaluation : Read` + `ApprovalInbox : Read, Approve, RequestRevision` |
| `dir` (penyetuju uji) | `ProcurementVendor : Read` + `ProcurementVendorStatusRequest : Read` + `ProcurementAttachment : Read` + `ApprovalInbox : Read, Approve, Reject, Return, RequestRevision` |
| `staff` (baca saja) | `ProcurementVendor : Read` + `ProcurementVendorStatusRequest : Read` + `ProcurementAttachment : Read` |
| `md` (admin Master Data) | `Supplier : Read, Update` |

Data uji run `20261119103432`: supplier SA "Alkes Maju" dan SB "Medika Prima" Aktif, SC "Lab Sentosa" Nonaktif di Master Data. Ketiganya kena pajak 11%; lihat temuan T-2.

**Konfigurasi persetujuan uji** (izin pemilik 19 November 2026, "Ya, pasang lalu cabut"):

- Skrip `vnd_wfl_fevd004.py` memasang satu baris matriks penyetuju = akun `dir` dan satu alasan tolak uji. Keduanya ditandai "DATA UJI PBJ FEVD004 (bukan nilai SOP)".
- Definisi `PBJ-VND-BLOKIR`/`BUKA` diaktifkan sesudah `UI-07`.
- Runner mencabutnya di blok `finally`. Hasil akhir: kedua definisi kembali `Draft`, tidak aktif, 0 baris matriks hidup.

Skrip: `ui_fe_vd004.cjs`, `fevd004_api_check.py`, `run_fevd004.py` di scratchpad sesi; tangkapan layar di `shots-fevd004/`.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 20 berkas yang berubah | 0 error; 2 warning `react-hooks/set-state-in-effect` (satu di hook baru, satu sudah ada di `use-vendor-detail.jsx`; pola muat-data yang sama dengan hook lain) | `PASS` | `fevd004-eslint.log` |
| `npm run lint:errors` (seluruh repo) | exit 0 | `PASS` | `fevd004-lint.log` |
| `npx next build`, berdampingan dengan `next dev` | `Compiled successfully in 88s`, exit 0; 36 rute `/corporate/procurement/**` termasuk `/vendors/status`, `/vendors/status/new`, `/vendors/status/[slug]` | `PASS` | `build-fevd004.log` |
| Grep anti-regresi pada berkas baru: warna literal, `style=`, `px`/`fontSize`, elemen HTML mentah, axios langsung, stylesheet baru | 0 temuan | `PASS` | keluaran perintah |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — test-policy: test baru tidak wajib | `NOT RUN` | — |
| Uji layar putaran 1 (run `20261119102105`) | Tidak jalan: Turbopack dev server panik ("Resource path … coming-soon.module.scss needs to be on project filesystem"), login tidak bisa dimuat. Dev server dimulai ulang dan `.next/dev` dihapus | Riwayat | `fe-dev-fevd004.log` |
| Uji layar putaran 2 (run sama) | 13/16 `PASS`; 3 `FAIL`: dua dari skrip (huruf kapital lencana, sisa usulan dari putaran 1) dan satu temuan T-2 (simpan Master Data 400 karena pajak) | `PARTIAL` (riwayat) | `ui-fevd004.results.run2.json` |
| Uji layar putaran 3, fixture baru `20261119103432` | 16/16 | `PASS` | `ui-fevd004.results.run3.json` |
| Cek API/DB `API-01`..`05` putaran 3 | 5/5 | `PASS` | `fevd004_api.results.json` |

### Temuan selama uji

| ID | Temuan | Tindakan |
| --- | --- | --- |
| T-1 | `-07` dalam mode baca tetap memanggil daftar penilaian untuk pilihan bukti. Penyetuju tanpa `ProcurementVendorEvaluation : Read` mendapat 403 di jaringan | Diperbaiki: penilaian hanya dimuat saat isian terbuka |
| T-2 | **Di luar task.** Form Master Data Supplier selalu mengirim `taxPercent` (0 bila kosong), sedangkan backend menolak `taxPercent` apa pun untuk supplier non-pajak ("Tax percent hanya boleh diisi jika supplier taxable."). Akibatnya supplier non-pajak tidak bisa disimpan dari layar Master Data, terlepas dari perubahan blokir | Tidak diubah tanpa wewenang. Uji memakai supplier kena pajak. Perlu task Master Data Supplier terpisah |

### Rincian putaran 3

| Skenario | Hasil | Bukti |
| --- | --- | --- |
| `UI-01` `-06`: menu Status; hero tanpa tombol; tab Vendor Tidak Aktif + Usulan; 3 kartu; 8 kontrol filter satu baris (`AT-VD-63`); bawaan "Semua selain Aktif" mengirim `vendorStatus=NON_ACTIVE`; cari run → hanya SC Nonaktif | `PASS` | `01-status-vendors.png` |
| `UI-02` `AT-VD-20`: Batasi tanpa alasan → 422 `PBJ-VAL-105` tampil di form; alasan + berlaku sampai 20-11-2026 → 200 Dibatasi dengan `Idempotency-Key`; Cabut tampil, Batasi hilang; riwayat vendor "Dibatasi" | `PASS` | `02-restricted.png` |
| `UI-03` `-06`: cari run → SA Dibatasi + SC Nonaktif; Status Dibatasi (`vendorStatus=RESTRICTED`) → hanya SA; Atur ulang → "Semua selain Aktif" | `PASS` | 2 / 1 baris |
| `UI-04` Cabut: alasan pendek → 422 `PBJ-VAL-104`, modal tetap terbuka; alasan sah → Aktif | `PASS` | — |
| `UI-05` Usulkan blokir dari tab Status → `-07` baru (vendor terkunci, jenis Blokir); tanpa alasan → 422 "Alasan usulan wajib diisi."; dengan alasan → 201 Draf, pindah ke rute detail; tombol Kembali/Simpan draf/Ajukan/Batalkan; Persetujuan "Belum masuk persetujuan." | `PASS` | `05-draft.png` |
| `UI-06` `AT-VD-22`: "+ Usulan" di `-06` → usulan kedua untuk SA → 409 `PBJ-VAL-107` tampil, tetap di `/new`; tab Status SA menampilkan info usulan terbuka tanpa tombol Usulkan | `PASS` | `06-val107.png` |
| `UI-07` `AT-VD-23`: Ajukan saat definisi belum aktif → 409 `PBJ-VAL-091` "Alur persetujuan status vendor belum dikonfigurasi. Hubungi administrator." di modal; tetap Draf | `PASS` | — |
| `UI-08` Ajukan sesudah definisi aktif → 200 Menunggu Persetujuan, `UBV-000012`, `Idempotency-Key`; tombol Tarik + Sinkronkan status; tab Persetujuan berisi langkah "Penyetuju status vendor" | `PASS` | `08-submitted.png` |
| `UI-09` `AT-VD-25`: pengusul membuka Antrean Persetujuan jenis Usulan Status Vendor → tidak ada tugas yang bisa ia setujui untuk usulannya | `PASS` | — |
| `UI-10` `AT-VD-24`: penyetuju menyaring "Usulan Status Vendor", **Setujui** → 200; **Lihat** membuka `-07`: Disetujui, "Vendor Sedang Diblokir", hanya tombol Kembali | `PASS` | `10-approved.png` |
| `UI-11` SA Diblokir: tab Status "Ya — alasan usulan", tanpa Batasi, tombol "Usulkan buka blokir"; riwayat "Vendor diblokir"; `-06` baris SA Diblokir | `PASS` | `11-blocked.png` |
| `UI-12` `AT-VD-29`: usulan SB diajukan lalu **Tarik**; alasan pendek → 422 `PBJ-VAL-104` modal tetap terbuka; alasan sah → Dibatalkan, riwayat "Dibatalkan" | `PASS` | `12-withdrawn.png` |
| `UI-13` `-06` tab Usulan: 6 kartu; filter satu baris dengan "+ Usulan"; cari run → 2 usulan; Status Dibatalkan (`status=CANCELLED`) → 1 (SB) | `PASS` | `13-requests.png` |
| `UI-14` `PBJ-DEC-060` (`AT-VD-26`): Master Data Supplier ubah SA → "Diblokir (Pengadaan)" tercentang dan nonaktif, keterangan baca saja; **Perbarui Supplier** → 200 tanpa `isBlacklisted`/`blacklistReason` di payload | `PASS` | `14-master-data.png` |
| `UI-15` Baca saja: tab Status tanpa tombol, daftar usulan tampil; `-06` Usulan tanpa "+ Usulan"; `-07` hanya Kembali | `PASS` | — |
| `UI-16` Tanpa `pageerror` di empat sesi | `PASS` | 0 |

### Cek API/DB

| ID | Skenario | Hasil |
| --- | --- | --- |
| `API-01` | `AT-VD-25`: pengusul memanggil `approve` atas tugas penyetuju untuk usulannya sendiri | 404 "Approval assignment tidak ditemukan pada inbox user login."; usulan tetap `PENDING_APPROVAL`, SB tidak diblokir; usulan uji lalu ditarik |
| `API-02` | `AT-VD-24`: `MstSupplier` SA | `IsBlacklisted = true`, `BlacklistReason` = alasan usulan, `IsPreferredSupplier = false`; riwayat vendor `VENDOR_BLOCK` tepat 1 |
| `API-03` | `AT-VD-29`: usulan SB yang ditarik | Status `CANCELLED`; instance `Wfl` `Withdrawn` |
| `API-04` | `AT-VD-26`: admin Master Data mengirim `PUT isBlacklisted=false` | 200; `IsBlacklisted` tetap `true` |
| `API-05` | D-10 dengan vendor Diblokir: `vendorStatus=NON_ACTIVE` + cari run | SA `BLOCKED` + SC `INACTIVE`, tanpa SB `ACTIVE` |

Uji manual: seluruh jalur di atas dijalankan lewat layar dengan empat aktor; lebar layar 1440 px.

**Tidak dijalankan:** tes unit baru (opsional); `test:uat` (dilarang repository); lebar layar selain 1440 px; Tolak dan Minta revisi oleh penyetuju (keputusan `Wfl` milik `FE-PBJ-005` dan Kotak Masuk Terpadu, sudah diuji API di `BE-PBJ-VD-007`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-20` Batasi tanpa alasan → `PBJ-VAL-105`; lalu Dibatasi | Terpenuhi | `UI-02` |
| `AT-VD-22` usulan blokir kedua → `PBJ-VAL-107` | Terpenuhi | `UI-06` |
| `AT-VD-23` ajukan saat definisi tidak aktif → `PBJ-VAL-091`, tetap Draf | Terpenuhi | `UI-07` |
| `AT-VD-24` usulan disetujui penyetuju lain → vendor Diblokir, satu riwayat | Terpenuhi | `UI-10`, `UI-11`; `API-02` |
| `AT-VD-25` pengusul tidak dapat menyetujui sendiri | Terpenuhi | `UI-09`; `API-01` |
| `AT-VD-26` Master Data tidak dapat mengubah status blokir | Terpenuhi | `UI-14`; `API-04` |
| `AT-VD-29` pengusul menarik usulan → Dibatalkan, `Wfl` Withdrawn | Terpenuhi | `UI-12`; `API-03` |
| `AT-VD-63` satu baris filter `-06` (dua daftar) | Terpenuhi | `UI-01`, `UI-13` |
| Hak akses: baca saja | Terpenuhi | `UI-15` |
| Pengiriman ganda dan data basi | Terpenuhi untuk pengiriman ganda | `Idempotency-Key` di `UI-02`, `UI-08`; data basi memakai pola `-02`/PR yang sama dan tidak diuji ulang di task ini |
| DoD: lint dan build | Terpenuhi | Tabel verifikasi |
| DoD: laporan ini dan status roadmap | Terpenuhi | Berkas ini; roadmap dan traceability ditandai ✅ |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status task | ✅ Selesai 19 November 2026 |
| Dependency backend | Pengerjaan ulang `BE-PBJ-VD-003` (D-10 `vendorStatus=NON_ACTIVE`) atas keputusan pemilik; laporan [BE-PBJ-VD-003](../backend/BE-PBJ-VD-003.md) bagian "Pembaruan 19 November 2026" |
| Data uji | `QuilvianNewDevSukma`. Run `20261119102105` (putaran 1–2) dan `20261119103432` (putaran 3), masing-masing berisi 1 unit, 4 jabatan, 4 akun, policy, dan 3 supplier, ditambah profil vendor, usulan, riwayat, dan instance `Wfl` uji. Ditambah fixture D-10 `20261119100610` (3 supplier). Id di `fevd004.fixture.run1.json`, `fevd004.fixture.run3.json`, dan `vd003r2.fixture.json` scratchpad. **Sudah dibersihkan 19 November 2026** atas izin pemilik ("lanjutkan" atas tawaran pembersihan): dry run lalu apply `cleanup_vendor_fixture.py` untuk ketiga run dalam satu transaksi. Dihapus lunak: 7 usulan status, profil vendor, 9 supplier, policy, pegawai, jabatan, dan unit. 8 akun dinonaktifkan + lockout. Dry run ulang tidak menemukan sisa. Riwayat dan instance `Wfl` uji dibiarkan sebagai jejak. Konfigurasi persetujuan uji **sudah dicabut**; definisi `PBJ-VND-*` tetap `Draft` |
| Risiko tersisa | Temuan T-2: simpan supplier non-pajak dari Master Data gagal (di luar task). Peta peran perlu memberi penyetuju dan pembaca usulan `ProcurementAttachment : Read` (F-6). Definisi `PBJ-VND-BLOKIR`/`BUKA` tetap `Draft` sampai SOP mengisi penyetuju (`PBJ-VAL-091` di produksi) |
| Task berikutnya | `FE-PBJ-VD-005` (penilaian berkala) |
