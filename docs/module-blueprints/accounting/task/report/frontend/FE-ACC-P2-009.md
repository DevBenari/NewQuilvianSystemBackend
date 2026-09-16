# Laporan Perubahan Frontend — `FE-ACC-P2-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-009` |
| Judul | Layar master Jenis Kejadian |
| Slice | `P2-0b` — Wave A, batch 14 September 2026 |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-009` (revisi 4) |
| Trace | `FR-P2-007` (prasyarat); `ACC-DEC-045`; `03-frontend-architecture.md` bagian 9 butir 3 dan bagian 10 butir 17 |
| Contract version | `ACC-API-0.10` grup Event Type (kontrak masih `Rencana (belum tersedia)`); `ACC-PERMISSION-0.5` `EventType : Read/Create/Update`. Bentuk payload diambil dari **source** `BE-ACC-P2-017` |
| Wewenang UI | Kartu roadmap: daftar, tambah, ubah, nonaktifkan; tombol dimatikan bagi yang tidak berhak. Warna, jarak, urutan kolom `DEV_DISCRETION` |
| Dependency | `BE-ACC-P2-017` 🟡 — source dan 6 dari 6 acceptance-nya lengkap, build backend 0 error; satu-satunya sisanya uji panggil `GET` saat backend berjalan. Migration `BE-ACC-P2-016` ✅ sudah diterapkan Rizki 14 Sep 2026 |
| Klasifikasi | `MEDIUM` — skor 6: repository 0 (laporan saja di backend), berkas diperiksa 2, berkas diubah 2 (12 berkas), logika 1, kontrak API 1, database 0, keamanan 1, UI 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan dan tautan bukti di repository backend |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `f6b1498fe` (branch `RizkiV2`), perubahan belum di-commit. Ter-commit Rizki di `a09ba4b13` (14 Sep 2026 13.11 WIB) bersama `FE-ACC-P2-010` dan `014` |
| Commit backend yang dijadikan rujukan | `b3ab542e` + working tree `BE-ACC-P2-017` (branch `rizkiG`); source itu kini ter-commit di `da1a4b6d` |
| Tanggal | 14 September 2026 |
| Status | **✅ SELESAI 14 September 2026** — 7 dari 7 acceptance terpetakan ke source; ESLint `0 error, 0 warning`; `npm run build` oleh owner `✓ Compiled successfully`. Ketiga butir DoD kartu terpenuhi. `IMPLEMENTATION COMPLETE`, `READY FOR UAT` — UAT dan uji peramban belum dijalankan. **Catatan dependency:** `BE-ACC-P2-017` masih 🟡 hanya karena uji panggil runtime backend; kontrak dan source yang dikonsumsi layar ini sudah lengkap — lihat bagian 8. **Riwayat:** 🟡 SEBAGIAN pada hari yang sama, hanya menunggu build owner |

---

## 1. Keadaan yang ditemukan di awal

Belum ada layar, slice, route, maupun butir menu Jenis Kejadian di frontend. Backend `BE-ACC-P2-017`
sudah menulis `EventTypeController` dengan tujuh endpoint, tetapi tabel `AccEventType` belum ada di
database sampai migration `BE-ACC-P2-016` diterapkan — sebelum itu setiap panggilan menjawab `500`.

Isi datanya pun belum ditetapkan: daftar jenis kejadian yang diterbitkan Finance menunggu
`DEC-ACC-P2-002`.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Alur normal

| Langkah | Pelaku | Yang terjadi di layar |
| ---: | --- | --- |
| 1 | Administrator akuntansi | Membuka **Akuntansi › Master Data › Jenis Kejadian** |
| 2 | Layar | Menampilkan daftar berhalaman: Kode, Nama, Modul Asal, **Aturan Posting Aktif**, Status |
| 3 | Administrator | Menekan **+ Tambah Jenis Kejadian**, mengisi `PENGAKUAN-PIUTANG`, "Pengakuan piutang pasien", `Finance`, menekan Simpan |
| 4 | Layar | Toast "PENGAKUAN-PIUTANG berhasil ditambahkan dan berstatus aktif.", kembali ke daftar |
| 5 | Administrator | Menekan **Perbarui** (atau klik dua kali baris) → layar ubah; **Kode terkunci**, nama dan modul asal dapat diubah |
| 6 | Administrator | Menekan **Nonaktifkan** → modal konfirmasi → daftar dimuat ulang |

### 2.2 Contoh — jenis yang masih dipakai

`PENGAKUAN-PIUTANG` punya **1** aturan posting aktif. Kolom Aturan Posting Aktif menampilkan `1`.
Saat Nonaktifkan ditekan, modal sudah memperingatkan **sebelum** dikonfirmasi:

> "Jenis PENGAKUAN-PIUTANG masih dipakai 1 aturan posting aktif, sehingga penonaktifannya akan
> ditolak. Nonaktifkan aturan posting itu lebih dahulu."

Bila tetap dikonfirmasi, backend menolak `409` dan pesannya tampil apa adanya sebagai toast merah
"Gagal Menonaktifkan". Modal tetap terbuka.

### 2.3 Jalur tidak normal

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Tanpa `EventType : Create` | Tombol Tambah mati + keterangan biru "Anda tidak memiliki hak menambah jenis kejadian." |
| Tanpa `EventType : Update` | Tombol Perbarui, Nonaktifkan, Aktifkan mati; klik dua kali tidak membuka layar ubah; keterangan menjelaskannya |
| Layar ubah dibuka tanpa hak | Tombol Perbarui Jenis Kejadian mati + keterangan di bawah hero |
| Kode kembar | Toast merah berisi pesan backend `409` "Kode jenis kejadian … sudah dipakai." |
| Tautan ubah kedaluwarsa (token sesi hilang) | "Tautan jenis kejadian tidak valid. Buka ulang dari daftar Jenis Kejadian." |
| Tanpa `EventType : Read` | `AccessDeniedGate` |
| Migration `016` belum diterapkan | Pesan galat `500` dari backend di atas tabel — dikenali dari kalimat "relation … does not exist" di log backend. **Sejak 14 Sep 2026 tidak berlaku lagi di `QuilvianNewDevRizki`**; tetap mungkin pada database yang belum menerapkan `20260914044507` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; kartu `FE-ACC-P2-009`; `EventTypeController.cs`, `AccEventTypeService.cs`,
`EventTypeDtos.cs` (`rizkiG`); layar Jenis Jurnal (`journal-type-view.jsx`, `use-journal-type.jsx`,
`use-journal-type-editor.jsx`, `journal-type-form-view.jsx`, route-nya,
`accounting-journal-type-slice.jsx`); `master-data-resource-slice-factory.jsx`; layar Daftar Akun
(`chart-of-account-view.jsx`, `use-chart-of-account.jsx` — pola aktif/nonaktif + `ConfirmModal`);
`use-recurring-journal.jsx` dan `use-recurring-journal-editor.jsx` (token route privat,
`usePermission`); `base-editor-view.jsx`, `base-editor-form.jsx`, `information-alert.jsx`,
`data-table.jsx`, `confirm-modal.jsx`; `master-data-inpatient-setting-view.jsx` (preseden
`formProps.renderActions`); `store.jsx`; `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/accounting/event-type/event-type-constants.jsx` | **Baru.** `EVENT_TYPE_CONFIG`, endpoint, pasangan hak, pilihan filter, salinan teks, field form (kode terkunci saat ubah) |
| `src/lib/state/slice/corporate/accounting/accounting-event-type-slice.jsx` | **Baru.** Factory dengan `activateConfig`/`deactivateConfig`; tujuh thunk diekspor sesuai tujuh endpoint |
| `src/lib/hooks/corporate/accounting/event-type/use-event-type.jsx` | **Baru.** Daftar, filter, paginasi, dua `usePermission`, konfirmasi aktif/nonaktif, token route |
| `src/lib/hooks/corporate/accounting/event-type/use-event-type-editor.jsx` | **Baru.** Tambah dan ubah; `GET /{id}`; validasi panjang dari DTO; payload persis DTO |
| `src/components/view/corporate/accounting/event-type/event-type-view.jsx` | **Baru.** Layar daftar |
| `src/components/view/corporate/accounting/event-type/form/event-type-form-view.jsx` | **Baru.** Layar tambah/ubah di atas `BaseEditorView` |
| `src/app/corporate/accounting/event-types/page.jsx`, `event-type-client.jsx`, `create/page.jsx`, `[slug]/update/page.jsx` | **Baru.** Route tipis; `[slug]` menolak token kosong, > 180 karakter, dan token cadangan |
| `src/lib/state/store.jsx` | Registrasi `accountingEventType` |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir "Jenis Kejadian" tingkat 3 di Akuntansi › Master Data |

### 3.3 Kepatuhan arsitektur frontend

Alur view → hook → slice → `InstanceAxios` lewat factory yang sudah ada. Nol komponen baru, nol CSS
Module baru, nol perubahan `globals.css`, nol `style={{ }}`.

**Selisih terhadap `master-data-feature-standard.md` — dilaporkan, disengaja.** Standar itu
(rujukan `hr/master-data/job-level`) menuntut sembilan thunk, halaman rincian `BaseDetailView`,
`/summary`, `/filters/metadata`, filter tanggal, dan berkas `utils` terpisah. Layar ini mengikuti
**pola modul Accounting** (Jenis Jurnal, Daftar Akun, Jurnal Berulang) karena:

1. kartu roadmap menetapkan Reuse `master-data-resource-slice-factory.jsx` dan layar Jenis Jurnal
   sebagai pola terdekat;
2. backend **tidak menyediakan** `/summary`, `/filters/metadata`, `PATCH /status`, maupun `DELETE`
   (delta `BE-ACC-P2-017`), sehingga thunk-nya akan menjawab `404`;
3. sebelas layar Accounting yang sudah ada memakai bentuk yang sama — layar kedua belas yang berbeda
   akan tampil dan berperilaku lain di modul yang sama.

Satu hal standar yang **diikuti** walaupun Jenis Jurnal tidak: `Id` tidak muncul di bilah alamat —
token route privat, pola Jurnal Berulang.

#### Gerbang keputusan base component

`UI GATE: 10 elemen — REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `journal-type-view.jsx` | REUSE | Apa adanya |
| Keterangan hak yang tidak dimiliki | `InformationAlert` `variant="info"` | `period-closing-view.jsx` | REUSE | Tampil hanya bila ada hak yang hilang |
| Pencarian dan filter status/jumlah baris | `DataFilter`, `FilterSelect` | `journal-type-view.jsx` | REUSE | Apa adanya |
| Tombol Tambah mati + alasan | `BaseButton` `disabled` + `title` | `recurring-journal-view.jsx` | REUSE | Di `DataFilter.actions` |
| Tabel berhalaman | `DataTable` + `RegionPagination` | `journal-type-view.jsx` | REUSE | Apa adanya |
| Status | `StatusBadge` | `journal-type-view.jsx` | REUSE | Aktif/Nonaktif |
| Tombol Perbarui per baris | `BaseButton` | `journal-type-view.jsx` | REUSE | Kolom "Ubah" |
| Tombol Nonaktifkan/Aktifkan per baris | `BaseButton` | `chart-of-account-view.jsx` | REUSE | Kolom "Keaktifan" terpisah — dua kolom satu tombol, tanpa CSS tata letak baru |
| Konfirmasi dan notifikasi | `ConfirmModal`, `ToastStack` | `chart-of-account-view.jsx` | REUSE | Apa adanya |
| Form tambah/ubah dengan tombol Simpan yang dapat dimatikan | `BaseEditorView` + `formProps.renderActions` | `master-data-inpatient-setting-view.jsx` baris 57 | REUSE | Render prop yang sudah ada; `BaseEditorForm` tidak diubah |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `DataTable` "Mengambil jenis kejadian..."; form ubah memuat rincian |
| Kosong | "Jenis kejadian belum ada." — "Daftar jenis kejadian yang diterbitkan Finance belum ditetapkan (DEC-ACC-P2-002). Tambahkan jenis bila sudah disepakati." |
| Gagal | Pesan backend di atas tabel; pada form, `InformationAlert` merah |
| Tanpa hak akses | Baca: `AccessDeniedGate`. Tambah/ubah: tombol mati + keterangan biru |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Accounting / Master Data / Event Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/event-types` | Daftar; query `isActive`, `search`, `pageNumber`, `pageSize` | `EventType : Read` |
| `GET` | `/v1/corporate/accounting/event-types/{id}` | Mengisi form ubah | `EventType : Read` |
| `GET` | `/v1/corporate/accounting/event-types/options` | Thunk diekspor untuk form Aturan Posting (`FE-ACC-P2-010`) | `EventType : Read` |
| `POST` | `/v1/corporate/accounting/event-types` | Tambah — `{ eventTypeCode, eventTypeName, sourceModule }` | `EventType : Create` |
| `PUT` | `/v1/corporate/accounting/event-types/{id}` | Ubah — `{ eventTypeName, sourceModule }`, **tanpa** kode | `EventType : Update` |
| `PATCH` | `/v1/corporate/accounting/event-types/{id}/deactivate` | Nonaktifkan, tanpa badan | `EventType : Update` |
| `PATCH` | `/v1/corporate/accounting/event-types/{id}/activate` | Aktifkan kembali, tanpa badan (delta kontrak `BE-ACC-P2-017`) | `EventType : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada seluruh berkas baru + `store.jsx` + `menu-items.jsx` | Exit `0`, keluaran kosong — **0 error, 0 warning** | `PASS` | Terminal agent, 14 Sep 2026 |
| Grep anti-regresi pada berkas baru | Nol `<button` mentah, `<table`, `style={{`, `fw-`/`fs-`, hex, `rgb` | `PASS` | `Select-String` |
| **Kolom layar lawan DTO backend** | 5 kolom tabel ↔ `EventTypeListResponse` (`eventTypeCode`, `eventTypeName`, `sourceModule`, `activePostingRuleCount`, `isActive`) — cocok; 3 isian form ↔ `CreateEventTypeRequest`; 2 isian ↔ `UpdateEventTypeRequest` — cocok; batas panjang 50/200/50 sama dengan `[MaxLength]` | `PASS` | Bagian 5 |
| Ejaan hak | `EventType : Create/Update` = `ControllerName` + argumen `[AccessAction]` | `PASS` | `EventTypeController.cs` |
| Pemanggilan endpoint sungguhan — **riwayat, pagi 14 Sep 2026** | Tidak dapat — tabel belum ada sampai `016` | `NOT RUN` | — |
| Pemanggilan endpoint sungguhan — sesi penutupan, 14 Sep 2026 13.17 WIB | Tabel sudah ada (`016` ✅), tetapi backend tidak berjalan: port 5107 tertutup. Bukan butir Verifikasi maupun DoD kartu | `NOT RUN` | [`BE-ACC-P2-017`](../backend/BE-ACC-P2-017.md) bagian 5 |
| `npm run build` — **riwayat, sebelum build owner** | Dijalankan owner — belum | `NOT RUN` | DoD kartu |
| `npm run build` oleh Rizki, 14 Sep 2026, `RizkiV2` HEAD `a09ba4b13` | `▲ Next.js 16.2.12 (Turbopack)`; `✓ Compiled successfully in 34.7s`; `✓ Finished TypeScript in 333ms`; `✓ Generating static pages using 15 workers (326/326)`; postbuild `prepare-standalone` berhasil. Rute terdaftar: `/corporate/accounting/event-types` (`○`), `/event-types/create` (`○`), `/event-types/[slug]/update` (`ƒ`). Nol error | `PASS` | Keluaran terminal Rizki |
| `npx eslint` ulang pada **26 berkas JSX commit `a09ba4b13`** — sesi penutupan, agent | Exit `0`, keluaran kosong — **0 error, 0 warning**; memuat seluruh berkas JSX task ini, `store.jsx`, dan `menu-items.jsx` | `PASS` | Terminal agent, 14 Sep 2026 |
| Grep anti-regresi ulang atas diff commit | JSX ditambahkan: nol `style={{`, nol `<button` mentah. Dua `className="fs-4"` pada ikon butir menu Jenis Kejadian dan Aturan Posting di `menu-items.jsx` — **dipertahankan**: pola ikon yang sama dipakai 192 kali di berkas itu, bukan override typography layar | `PASS` dengan catatan | `git show a09ba4b13 -U0` disaring |
| Isi commit | `a09ba4b13` memuat tepat 28 berkas ketiga laporan (`014`, `009`, `010`), tanpa berkas lain; `globals.css` tidak termasuk | `PASS` | `git show --stat a09ba4b13` |

`AUTOMATED TEST: SKIPPED (opsional) — ACC-DEC-081.`

Uji manual: `NOT FEASIBLE` bagi agent — tanpa peramban. Alasan kedua semula, tabel belum ada, gugur
sejak migration `016` diterapkan; yang tersisa bagi siapa pun yang menguji adalah backend berjalan
dan hak `EventType : *` diberikan lewat Akses Role. Skenario bagian 2 diserahkan ke tim UAT.

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Daftar memakai `GET /event-types` dengan tabel berhalaman dan penyaring | **Terpenuhi** | `getEventTypeList(requestParams)`; `DataTable` + paginasi; filter status, cari, jumlah baris |
| 2 | Form tambah dan ubah mengirim payload sesuai DTO backend | **Terpenuhi** | `handleSubmit` — create `{ eventTypeCode, eventTypeName, sourceModule }`, update tanpa kode |
| 3 | Nonaktifkan meminta konfirmasi, dan penolakan `409` tampil apa adanya | **Terpenuhi** | `ConfirmModal` + `submitConfirm` menampilkan `error.message` |
| 4 | Tombol Tambah, Ubah, dan Nonaktifkan dimatikan bagi yang tidak berhak | **Terpenuhi** | `createBlockedReason`, `updateBlockedReason`; tombol Simpan form ikut mati |
| 5 | Keadaan memuat, gagal, dan kosong ditangani | **Terpenuhi** | Bagian 4 |
| 6 | Butir menu terdaftar | **Terpenuhi** | `menu-items.jsx` — `corporateAccountingEventType` |
| 7 | `globals.css` tidak disentuh, tanpa `style={{ }}` | **Terpenuhi** | `git status`, grep |

| Butir DoD | Hasil |
| --- | --- |
| Lint hijau | **Ya** — diulang atas isi commit, 0 error, 0 warning |
| Laporan task tertulis | **Ya** — berkas ini |
| Build dijalankan owner | **Ya** — 14 Sep 2026, `✓ Compiled successfully in 34.7s`, 326/326 halaman statis. Riwayat: **belum** sebelum build |
| Uji manual / UAT | Bukan butir DoD kartu. Belum dijalankan — diserahkan ke tim UAT; **bukan** `UAT PASS` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Isi datanya menunggu `DEC-ACC-P2-002`, jadi layar akan menampilkan keadaan kosong sampai jenis kejadian ditambahkan. Hak `EventType : *` belum diberikan ke peran mana pun — non-SuperAdmin melihat semua tombol mati. **Tertutup 14 Sep 2026:** galat `500` karena tabel belum ada, di `QuilvianNewDevRizki` |
| Dependency backend | `BE-ACC-P2-016` ✅ diterapkan Rizki 14 Sep 2026. `BE-ACC-P2-017` 🟡 — **dicatat, tidak menahan task ini:** kartu `FE-ACC-P2-009` meminta verifikasi lint, build owner, dan pemeriksaan kolom layar lawan DTO, ketiganya terpenuhi; kontrak `EventTypeController` yang dikonsumsi sudah terkunci di source dan 6 dari 6 acceptance `017` terpenuhi. Sisa `017` hanya uji panggil runtime. **Bila uji panggil itu kelak menemukan bentuk respons berbeda dari `EventTypeDtos.cs`, task ini wajib diturunkan kembali ke 🟡** |
| Perubahan sampingan | `NONE` |
| Interupsi | Penutupan status sesudah build owner dikerjakan sesi 14 September 2026 siang; source tidak disunting ulang |
| Status Git | Saat dikerjakan: ` M store.jsx`, ` M menu-items.jsx`, dan lima jalur `??` di atas. Sekarang: ter-commit Rizki di `a09ba4b13`; working tree `RizkiV2` bersih saat diperiksa sesudah build. **Agent nol commit, push, stage, atau merge** |
| Langkah berikutnya | ~~Owner: `npm run build`; migration `016`~~ **selesai 14 Sep 2026**. Owner: beri hak `EventType : Read/Create/Update` lewat Akses Role. Backend: uji panggil `BE-ACC-P2-017`. Tim UAT: skenario bagian 2 |
