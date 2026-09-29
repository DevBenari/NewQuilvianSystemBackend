# Laporan Perubahan Frontend — `FE-ACC-P2-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-016` |
| Judul | Layar Rekonsiliasi per periode: saldo subledger, selisih, dan penghalang keempat |
| Slice | Wave D — pasangan `BE-ACC-P2-014` |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-016` (revisi 7, `APPROVED` Rizki 28 September 2026) |
| Trace | `FR-P2-040`, `042`, `043`; `ACC-DEC-066`, `071`, `076`, `107`..`114`; `03-frontend-architecture.md` 11.3 dan 11.6 (revision 7, approved `GATE-DESAIN-0928`) |
| Contract version | `ACC-API-0.13` — `GET /reconciliation/subledger-comparison`, butir `SUBLEDGER_RECONCILIATION` pada `GET /periods/{id}/closing-checklist`; `GET /periods` yang sudah ada |
| Wewenang UI | Isi dan sumber data dikunci 11.6; `DEV_DISCRETION` dipakai untuk periode bawaan (tidak ada, kecuali dari tautan), pemilih periode (`FilterSelect` seperti Neraca Saldo), rupa penanda, dan cara membawa periode (`?periode=YYYY-MM`) |
| Dependency | `BE-ACC-P2-014` 🟡 (source ditulis, menunggu build); `FE-ACC-P2-017` 🟡 |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa > 20 (2), diubah > 8 (2), logika sedang (1), memakai kontrak baru yang sudah approved (1), satu layar + satu tautan (1) |
| Task mode | `FRONTEND` (Langkah D, berpasangan atas perintah Rizki 28 September 2026) |
| Target tulis | Layar Rekonsiliasi (route, view, hook, slice, constants, utils, CSS module milik layar), satu entri tautan Daftar Periksa beserta pembawa periodenya, dua berkas unit test; laporan ini dan baris status di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `a6d269077` (branch `RizkiV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `485d8b8b` + source `BE-ACC-P2-014` di working tree |
| Tanggal | 29 September 2026 |
| Status | **🟡 SEBAGIAN — 29 September 2026.** 12 dari 12 acceptance terpetakan ke source; eslint 11 berkas 0/0; test terkait 53/53; `test:unit` 1788/1797 — 9 gagal di luar task. `npm run build` Rizki 29 September 2026 berhasil (Compiled 53 detik, 411/411 halaman, postbuild standalone siap). **Belum:** uji layar bersama `BE-ACC-P2-014` (bagian 6.2) |

## 1. Keadaan yang ditemukan di awal

Layar `FE-ACC-P2-008` hanya membaca `GET /reconciliation/gl-balances` per **tanggal**. Kolom Saldo
Subledger dan Selisih selalu "Belum tersedia". Daftar Periksa Penutupan memetakan tautan Lihat hanya
untuk dua kode jurnal.

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka Rekonsiliasi Control Account (butir menu dari `FE-ACC-P2-017`) dan memilih periode.
2. Layar memanggil `subledger-comparison` untuk periode itu dan menampilkan spanduk keadaan:
   **belum berlaku** (biru), **bersih** (hijau), atau **belum bersih** (kuning) beserta rincian dari
   backend.
3. Tabel memuat setiap control account: saldo buku besar, saldo subledger beserta tanggal cut-off,
   versi, dan nomor pesannya, selisih, keadaan, dan apakah akun itu menahan penutupan.
4. Nomor pesan saldo dapat diklik untuk membuka Rincian Kejadian (bila berhak `AccountingEvent : Read`).
5. Dari Daftar Periksa Penutupan, butir **Rekonsiliasi saldo subledger** yang menahan membawa tautan
   Lihat ke layar ini **untuk periode yang sama** (`?periode=2026-09`).

**Contoh.** September 2026: Kas Kecil tampil "Belum diterima" dengan selisih "-", bukan Rp 0; Utang
Supplier Rp 30.000.000 lawan Rp 29.999.500 tampil selisih Rp 500 berwarna merah dan berpenanda
"Menahan penutupan".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| Kelima berkas layar Rekonsiliasi, `page.jsx`, CSS module-nya, `tests/unit/accounting-reconciliation.test.mjs` | Titik tolak |
| `use-trial-balance.jsx`, `trial-balance-view.jsx` | Pola pemilih periode (`FilterSelect`, daftar dari slice periode, reset saat badan hukum berganti) |
| `accounting-period-slice.jsx` | `getAccountingPeriodList`, selector |
| `use-accounting-event-inbox.jsx`, `accounting-event-constants.jsx`, `private-route-token-utils.js` | Membuka Rincian Kejadian lewat token rute privat |
| `period-closing-view.jsx`, `use-period-closing.jsx`, `period-closing-utils.jsx`, `period-closing-constants.jsx`, `tests/unit/accounting-period-closing-link.test.mjs` | Tautan Lihat Daftar Periksa |
| `journals/page.jsx`, `use-journal.jsx` | Pola `useSearchParams` di balik `Suspense` |
| Base component `information-alert.jsx`, `status-badge.jsx`, `base-button.jsx`, `filter-select.jsx` | Varian yang tersedia |
| Backend `ControlAccountReconciliationDtos.cs`, `AccPeriodClosingService.cs` | Bentuk respons yang dikonsumsi |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../reconciliation/control-account-reconciliation-constants.jsx` | + izin `AccountingEvent : Read`, kunci query `periode`, label "Belum diterima"/"-"/"Tidak wajib", `SUBLEDGER_RECONCILIATION_STATE`, `SUBLEDGER_ITEM_STATUS`, badge "Menahan penutupan"; konfigurasi layar per periode; kolom tabel baru; alasan kosong `noPeriodAvailable`, `noPeriod`, `periodNotFound` |
| `src/utils/.../reconciliation/control-account-reconciliation-utils.jsx` | + `normalizeSubledgerComparisonReport/Row`, `resolveSubledgerItemStatus`, `buildSubledgerComparisonQuery`, `resolveSubledgerStateNotice`, `readReconciliationPeriodFromQuery`, `buildReconciliationPeriodHref`, `buildReconciliationPeriodOptions`. Fungsi `gl-balances` lama **dipertahankan** — endpoint-nya tetap sah dan dijaga fixture |
| `src/lib/state/slice/corporate/accounting/accounting-reconciliation-slice.jsx` | + thunk `getSubledgerComparison` memakai state `report` yang sama (penjaga `requestId`, buang saat gagal/tutup) |
| `src/lib/hooks/.../reconciliation/use-control-account-reconciliation.jsx` | Ditulis ulang: pemilih periode, periode awal dari `?periode=`, reset periode saat badan hukum berganti, panggilan `subledger-comparison` (tidak lagi `gl-balances`), `openEvent`, `stateNotice` |
| `src/components/view/.../reconciliation/control-account-reconciliation-view.jsx` | Ditulis ulang: spanduk keadaan, kartu meta per periode, ringkasan tujuh angka, `FilterSelect` periode, kolom Subledger/Selisih/Keadaan/Penutupan |
| `src/style/corporate/accounting/control-account-reconciliation-view.module.css` | + `.amountStack`, `.amountDetail`, `.eventLink`, `.muted` — token saja |
| `src/app/corporate/accounting/reconciliation/page.jsx` | Dibungkus `Suspense` (syarat `useSearchParams`) |
| `src/lib/constants/.../accounting-period/period-closing-constants.jsx` | + entri `SUBLEDGER_RECONCILIATION` di `CHECKLIST_ITEM_LINK` |
| `src/utils/.../accounting-period/period-closing-utils.jsx` | Entri yang membawa kode periode memakai `buildReconciliationPeriodHref` |
| `src/lib/hooks/.../accounting-period/use-period-closing.jsx` | Tautan berizin disembunyikan bila `AccountingReconciliation : Read` diketahui tidak ada |
| `src/components/view/.../closing/period-closing-view.jsx` | Tautan tampil hanya bila alamatnya ada: `const href = checklistHrefByCode[item.code] \|\| "";` |
| `tests/unit/accounting-reconciliation.test.mjs` | Asersi kolom dan label disesuaikan; + 4 test sisi subledger (22 → 26) |
| `tests/unit/accounting-period-closing-link.test.mjs` | Asersi ekspresi `href` disesuaikan; + 1 test tautan rekonsiliasi |

Total +769/−255 pada 14 berkas (termasuk `menu-items.jsx` milik `FE-ACC-P2-017`).

### 3.3 Kepatuhan arsitektur frontend

| Elemen | Keputusan | Dasar |
|---|---|---|
| Pemilih periode | `REUSE` `FilterSelect` | Sama dengan Neraca Saldo |
| Spanduk keadaan | `REUSE` `InformationAlert` varian `info`/`success`/`warning` | — |
| Tabel | `REUSE` `DataTable` | — |
| Keadaan per akun, penanda menahan | `REUSE` `StatusBadge` (`active`, `inactive`, `pending`, `warning`) | — |
| Ringkasan | `REUSE` `SummaryGrid` | — |
| Tautan nomor pesan | `REUSE` `BaseButton` varian `ghost` | — |
| Baris rincian cut-off/versi | `COMPOSE` — kelas baru di CSS module milik layar, token saja | Tidak menyentuh `globals.css` |

`UI GATE`: REUSE 6, COMPOSE 1, NEW 0. API hanya lewat thunk Redux + `InstanceAxios`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Menghitung rekonsiliasi saldo subledger..." dan kerangka ringkasan |
| Badan hukum belum dipilih | "Badan hukum belum dipilih." |
| Belum ada periode | "Belum ada periode akuntansi." |
| Periode belum dipilih | "Periode belum dipilih." |
| Kode periode di alamat tidak ada | "Periode tidak ditemukan pada badan hukum ini." |
| Tanpa control account | "Belum ada akun yang ditandai sebagai control account." |
| Gagal | Pesan backend untuk `400`/`409`; kalimat layar untuk jaringan, `404`, `5xx`; tombol Muat Ulang |
| Tanpa hak akses | `AccessDeniedGate` — "Anda tidak memiliki hak melihat rekonsiliasi control account." |

## 5. Endpoint yang dikonsumsi

#### Corporate / Accounting / Reconciliation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/reconciliation/subledger-comparison?accountingPeriodId=` | Seluruh isi layar | `AccountingReconciliation : Read` |

#### Corporate - Accounting - Accounting Period

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/periods?legalEntityId=&pageSize=100` | Pilihan periode | `AccountingPeriod : Read` |

## 6. Verifikasi

### 6.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 11 berkas source | Keluar `0`, nol pesan | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` empat berkas test rekonsiliasi/penutupan | 53/53 | `PASS` | Keluaran perintah |
| `npm run test:unit` | 1797 test, 1788 lulus, 9 gagal: `accounting-trial-balance` (2), `inpatient-physician-entry` (5), `menu-permission-filter` M0 bank darah (1), `petty-cash-finance-separation` (1) — seluruhnya di luar layar ini dan sudah gagal pada baseline `FE-ACC-P2-015` (10 gagal; test rekonsiliasi yang dulu gagal kini lulus) | `UNRELATED EXISTING ISSUE` | Keluaran perintah |
| Grep `style={{`, warna literal, `//` baru | Nol | `PASS` | — |
| `npm run build` (Rizki) | `npm run build` Rizki 29 September 2026 berhasil (Compiled 53 detik, 411/411 halaman, postbuild standalone siap); route `/corporate/accounting/reconciliation` terbangun statis | `PASS` | Keluaran build yang ditempel Rizki |
| Uji layar | Menunggu Rizki | `NOT RUN` | Bagian 6.2 |
| `AUTOMATED TEST` | 5 test baru ditulis (opsional) | `PASS` | — |

### 6.2 Skenario uji layar untuk Rizki

Jalankan sesudah `npm run build` (atau `npm run dev`) dan sesudah build backend `BE-ACC-P2-014`.
Urutannya berselang-seling dengan skenario Swagger S1–S14 di laporan
[`BE-ACC-P2-014`](../backend/BE-ACC-P2-014.md) bagian 5.2.

| # | Langkah | Hasil yang diharapkan | Membuktikan |
|---:|---|---|---|
| L1 | Menu Accounting → **Rekonsiliasi Control Account** | Layar terbuka; butir ada sesudah Neraca Saldo | `FE-ACC-P2-017` |
| L2 | Tanpa memilih periode | Tabel kosong "Periode belum dipilih."; tidak ada isian tanggal | (1) |
| L3 | Pilih **Agustus 2026** | Spanduk biru "Rekonsiliasi belum berlaku untuk periode ini." + "…berlaku mulai periode 2026-09."; kolom Penutupan tidak ada "Menahan penutupan" | (5) |
| L4 | Pilih **September 2026** (sesudah Swagger S1) | Spanduk kuning berisi `stateMessage` S1; akun tanpa saldo "Belum diterima" / "-", **bukan** Rp 0; penanda "Menahan penutupan" pada akun wajib; angka ringkasan sama dengan S1 | (2), (3), (6) |
| L5 | Sesudah S6 (cut-off 15 September) → Muat Ulang | Akun A: nilai beserta "Cut-off 15 September 2026 · versi …", selisih "-", keadaan "Cut-off bukan akhir periode" | (4) |
| L6 | Sesudah S7 (+Rp 500) → Muat Ulang | Akun A: selisih **−Rp 500** merah, keadaan "Berselisih" | (2) |
| L7 | Klik nomor pesan saldo akun A | Rincian Kejadian pesan itu terbuka | (7) |
| L8 | Sesudah S10 → Muat Ulang | Spanduk hijau "Rekonsiliasi bersih."; nol penanda menahan | (5) |
| L9 | Periode Akuntansi → September 2026 → Daftar Periksa, **sebelum** S10 | Butir keempat "Rekonsiliasi saldo subledger" bertanda bermasalah dengan pesan rincian dan tombol **Lihat**; klik → Rekonsiliasi terbuka langsung pada September 2026 | (8) |
| L10 | Daftar Periksa Agustus 2026 | Butir keempat "belum dapat diperiksa" beserta alasannya, tanpa angka, tanpa Lihat | (8) |
| L11 | Tekan Ajukan pada September sebelum S10 (bila penghalang lain nol) | Pesan `409` rekonsiliasi tampil apa adanya | (9) |
| L12 | Ganti badan hukum (bila ada lebih dari satu) | Periode terpilih kosong kembali, tabel dibuang | (1), (10) |
| L13 | Pengguna tanpa `AccountingReconciliation : Read` | Layar "Anda tidak memiliki hak…"; di Daftar Periksa tombol Lihat butir keempat tidak tampil | (8) |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Pemilih periode menggantikan isian tanggal; data dari `GET /periods` | Terpenuhi di source | `FilterSelect` + `getAccountingPeriodList`; `FilterDatePicker` dihapus |
| (2) Tabel dan spanduk hanya dari `subledger-comparison`; selisih dari backend | Terpenuhi di source | Hook memanggil `getSubledgerComparison` saja; `difference` dibaca dari `Difference` |
| (3) `BelumDiterima` → "Belum diterima" / "-", tidak pernah Rp 0 | Terpenuhi di source + test | Sel `subledger`/`difference` saat `null`; test "belum diterima bukan nol" |
| (4) Cut-off bukan akhir periode → nilai + tanggal cut-off, selisih "-" | Terpenuhi di source | `renderSubledgerDetail`; selisih `null` dari backend |
| (5) Spanduk tiga keadaan, `StateMessage` apa adanya, periode titik mulai | Terpenuhi di source + test | `resolveSubledgerStateNotice` |
| (6) Penanda menahan; akun tidak wajib tanpa penanda | Terpenuhi di source | Sel `blocking` |
| (7) Nomor pesan membuka Rincian Kejadian bila berhak | Terpenuhi di source | `openEvent` + `canOpenEvent` |
| (8) Butir keempat terender tanpa kode khusus; Lihat ke periode sama; tersembunyi tanpa hak | Terpenuhi di source + test | Entri `CHECKLIST_ITEM_LINK`, `buildReconciliationPeriodHref`, filter izin di `use-period-closing.jsx` |
| (9) Pesan `409` tampil apa adanya | Terpenuhi — perilaku layar yang ada | `FE-ACC-P2-002` menampilkan pesan backend |
| (10) Tanpa cache, tanpa tombol pengecualian | Terpenuhi di source | `resetReconciliationState` saat tutup, `clearReconciliation` saat ganti badan hukum/periode |
| (11) Keadaan kosong dan gagal sesuai 11.6 | Terpenuhi di source | Bagian 4 |
| (12) Nol `globals.css`, nol `//` baru | Terpenuhi | Grep bagian 6.1 |

| Butir DoD | Keadaan |
| --- | --- |
| Lint hijau | ✅ |
| Build owner berhasil | ✅ 29 September 2026 |
| Uji layar tercatat | **Belum** |
| Laporan task tertulis | ✅ |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status Git frontend | ` M` 14 berkas di bagian 3.2 (termasuk `menu-items.jsx` milik `FE-ACC-P2-017`) |
| Selama pengerjaan | Seluruh komentar lama di berkas konstanta rekonsiliasi terhapus di disk di luar pengerjaan agent; isinya selain komentar sama — diterima apa adanya |
| Risiko | Thunk dan normalizer `gl-balances` tidak lagi dipakai layar ini tetapi dipertahankan; menghapusnya butuh keputusan terpisah. `404` dari `subledger-comparison` ditampilkan sebagai "Layanan … belum tersedia pada server ini" — benar bila backend belum di-build ulang |
| Temuan di luar cakupan | `CHECKLIST_ITEM_LINK` belum memetakan `FAILED_EVENTS`/`HELD_EVENTS` ke Kotak Masuk (coverage gap roadmap revisi 7) |
| Task berikutnya | Build backend + frontend, lalu uji sekali: Swagger S1–S14 berselang-seling dengan L1–L13 |
