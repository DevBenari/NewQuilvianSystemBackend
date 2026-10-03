# Laporan Perubahan Frontend — `FE-FIN-028`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-028` |
| Judul | Penyetuju cutover dapat mencatat, menyetujui, dan mengunci saldo awal setiap kelompok — Layar Saldo Awal Cutover |
| Slice | `REV-14B` — `EPIC FIN-21` (pemetaan akun control, cutover saldo awal, dan sinkronisasi subledger) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian REV-14B |
| Trace | `FR-FIN-146`..`148`; `FIN-DEC-128`; `FIN-DES-088`; `03-frontend-architecture.md` 19.2 nomor 8 |
| Contract version | `FIN-API-1.5` F.1; `FIN-STATE-1.6` F.1; `FIN-PERM-1.7` G.1, G.5; `FIN-VAL-1.7` `180`..`185` |
| Wewenang UI | Frontend Owner — **sebagian tertahan** `FIN-OQ-079` (penempatan butir menu). Yang **mengikat** (invariant): baris `LOCKED` tampil dinonaktifkan, bukan disembunyikan; layar tidak boleh menjanjikan pemisahan penyiap dan penyetuju dijamin sistem; setujui dan kunci dinonaktifkan sejak permintaan dikirim; piutang dan utang menampilkan nol beserta alasannya. Yang `DEV_DISCRETION`: bentuk kartu, urutan dan lebar kolom, warna, ikon |
| Dependency | `BE-FIN-066` ✅ (5 endpoint saldo awal pada `FinanceSubledgerSetupController`; laporan BE menyatakan `dotnet build` tidak dijalankan otomatis) |
| Klasifikasi | `MEDIUM` — 1 rute, 1 view + 3 komponen turunan, 1 hook, 1 slice, 1 berkas konstanta, 1 berkas utilitas, 1 CSS Module, 1 unit test, 2 perubahan kecil pada berkas existing |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only; repository backend ditulis **hanya** untuk laporan ini dan bukti pada roadmap |
| Branch | `yasmina` (upstream `origin/yasmina`), sama dengan `FE-FIN-027`. Prompt tidak memuat penetapan branch eksplisit; kesesuaian dengan penetapan pemegang modul **belum dikonfirmasi** |
| Tanggal | 3 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak. `npm run lint:errors`, `npm run test:unit`, dan `npm run build` **NOT RUN** — standing instruction pengguna: build/lint/test dijalankan manual oleh pengguna. Verifikasi manual UI **NOT FEASIBLE** dari sesi ini (§7) |

---

## 1. Masalah yang Diperbaiki

Backend `BE-FIN-066` sudah menyediakan siklus hidup saldo awal cutover `DRAFT` → `APPROVED` → `LOCKED`, tetapi petugas tidak punya layar untuk menjalankannya:

1. **Tidak ada layar pencatatan.** Lima kelompok saldo (`KAS-KASIR`, `KAS-KECIL`, `PIUTANG`, `UTANG-SUPPLIER`, `UTANG-JASA-MEDIS`) harus dicatat dengan alasan dan rujukan dokumen Accounting, dan belum ada antarmukanya.
2. **Setujui dan kunci tidak dapat ditarik.** Tanpa penjagaan di layar, klik ganda atau konfirmasi yang kurang jelas dapat mengunci nilai yang salah secara permanen. Penguncian `KAS-KASIR` juga menerbitkan mutasi kas `SALDO-AWAL` yang tidak dapat dibatalkan.
3. **Risiko anggapan pemisahan tugas.** Mesin hak akses tidak mencegah satu orang memegang `Create` dan `Approve` sekaligus (`FIN-PERM-1.7` G.5). Layar yang diam tentang hal ini dapat dibaca sebagai jaminan.
4. **Kelompok piutang dan utang membingungkan.** Nilainya wajib nol (`FIN-VAL-181`) karena rinciannya datang dari migrasi tagihan lama; kolom kosong atau angka yang dapat diisi akan menyesatkan.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Pelaku:** penyiap cutover (hak `FinanceSubledgerSetup` `Create`/`Update`) dan penyetuju cutover (hak `Approve`). Satu orang **dapat** memegang keduanya — lihat §9.

1. Petugas membuka `/finance/subledger-setup/opening-balances`. Layar memanggil `GET /opening-balances` dan menampilkan **tepat lima kartu**, satu per kelompok, termasuk kelompok yang belum dicatat.
2. **Catat:** pada kartu "Belum dicatat", petugas menekan *Catat Saldo Awal*, mengisi nominal, tanggal cutover, rujukan dokumen Accounting, dan alasan (keduanya wajib), lalu menyimpan sebagai `DRAFT`. Untuk kelompok piutang dan utang, kolom nominal dikunci pada `0`.
3. **Ubah:** selama `DRAFT`, petugas dapat mengoreksi isian (`PUT`, membawa `RowVersion`).
4. **Setujui:** penyetuju menekan *Setujui* → dialog konfirmasi menyatakan persetujuan tidak dapat ditarik → `POST /{id}/approve`.
5. **Kunci:** pada `APPROVED`, *Kunci* → dialog konfirmasi menyatakan penguncian permanen (dan, untuk `KAS-KASIR` bernominal > 0, penerbitan satu mutasi kas `SALDO-AWAL`) → `POST /{id}/lock`.
6. Pada `APPROVED` dan `LOCKED`, tombol *Ubah* tetap tampil tetapi **nonaktif**, disertai keterangan mengapa.
7. Bila baris diubah orang lain di sela-sela (`409` benturan `RowVersion`), dialog ditutup, daftar dimuat ulang, dan banner menjelaskan bahwa baris sudah diubah orang lain beserta tombol *Muat Ulang Data*.

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

| No | Elemen | Keputusan | Komponen sumber |
|:--:|---|:--:|---|
| 1 | Header halaman | `REUSE` | `base-features/hero` |
| 2 | Banner pemisahan penyiap/penyetuju, tautan, sukses, galat, memuat | `REUSE` | `base-features/information-alert` |
| 3 | Kartu ringkasan jumlah per status | `REUSE` | `base-features/summary-grid` |
| 4 | Lencana status | `REUSE` | `base-features/status-badge` |
| 5 | Tombol aksi | `REUSE` | `base-features/base-button` |
| 6 | Dialog konfirmasi setujui/kunci | `REUSE` | `base-features/confirm-modal` |
| 7 | Field form (teks, area teks, tanggal, select) | `REUSE` | `base-features/base-form-control` |
| 8 | Penutup akses | `REUSE` | `base-features/access-denied-gate` |
| 9 | Kartu satu kelompok | `NEW (view-lokal)` | `opening-balance-group-card.jsx` di bawah `components/view/finance/subledger-setup/opening-balances/`. Bukan base component baru: spesifik domain dan hanya dipakai satu layar. Dipilih kartu, bukan `DataTable`, karena hanya lima baris tetap berurutan dan `DataTable` mengurutkan menurut tanggal buat |

**Hasil UI Gate:** 8 `REUSE`, 1 komponen view-lokal. Tidak ada base component, wrapper, atau abstraksi generik baru. Styling lewat satu CSS Module (`src/style/corporate/finance/subledger-setup/opening-balances-view.module.css`) memakai token `--base-*` yang diwarisi dari `.dataPage`; kelas yang dipakai di dalam modal memakai token global saja karena modal dirender lewat portal di luar `.dataPage`.

---

## 4. Cara Mencapai Layar Selama Menu Tertahan (`FIN-OQ-079`)

Butir menu **tidak** dipasang (bukan `DEV_DISCRETION`). Layar dicapai lewat:

1. **Rute langsung:** `/finance/subledger-setup/opening-balances`
2. **Tautan dari layar terkait:** banner info baru pada layar Pemetaan Akun Control (`/finance/subledger-setup/control-accounts`) — *Buka Saldo Awal Cutover →*
3. **Tautan balik:** banner info pada layar saldo awal — *Buka Pemetaan Akun Control →*

---

## 5. Endpoint yang Dikonsumsi dan Pemetaan Kontrak

Base URL: `/api/v1/corporate/finance-management/subledger-setup`

| Method | Path | Hak akses | Dipanggil lewat | Catatan |
| :-- | :-- | :-- | :-- | :-- |
| `GET` | `/opening-balances` | `Read` | `fetchOpeningBalances` | Tidak berpaging; `ApiResponse<List<OpeningBalanceResponse>>`. `signal` diteruskan; dibatalkan saat layar ditutup |
| `POST` | `/opening-balances` | `Create` | `createOpeningBalance` | Body: `balanceGroup`, `amount`, `cutoverDate`, `reason`, `accountingReferenceDocument` |
| `PUT` | `/opening-balances/{id}` | `Update` | `updateOpeningBalance` | Body: `amount`, `cutoverDate`, `reason`, `accountingReferenceDocument`, `rowVersion` |
| `POST` | `/opening-balances/{id}/approve` | `Approve` | `approveOpeningBalance` | Body: `rowVersion` |
| `POST` | `/opening-balances/{id}/lock` | `Approve` | `lockOpeningBalance` | Body: `rowVersion` |

### Delta antara kontrak, backend saat ini, dan layar

Tidak ada payload yang ditebak; semua di bawah ini dibaca dari `SubledgerOpeningBalanceDtos.cs` dan `FinanceOpeningBalanceService.cs`. Backend tidak diubah.

| # | Temuan | Perlakuan di layar |
|:--:|---|---|
| 1 | `ApproveOpeningBalanceRequest.Notes` dan `LockOpeningBalanceRequest.Notes` diterima backend tetapi **tidak disimpan** | ✅ **Ditutup 3 Oktober 2026** (keputusan pemilik): `Notes` dihapus dari kedua DTO backend dan dari kontrak. Layar tidak pernah mengirimnya |
| 2 | `OpeningBalanceResponse.ApprovedBy` hanya `Guid`, tanpa nama | ✅ **Ditutup 3 Oktober 2026** (keputusan pemilik): backend menambah `ApprovedByName` (`DisplayName ?? UserName ?? Email ?? UserCode`, pola yang sama dengan `PettyCashBudgetService`); layar menampilkan *Disetujui oleh* beserta waktunya. Bila `ApprovedByName` `null`, layar menulis "Nama penyetuju tidak tersedia" |
| 3 | Kontrak `FIN-STATE-1.6` F.1 menyatakan mengunci `KAS-KASIR` menerbitkan satu mutasi kas; kode hanya menerbitkannya bila `Amount > 0` | ✅ **Source ditutup 3 Oktober 2026** (keputusan pemilik: "selalu terbit"). Karena `FIN-VAL-168` dan constraint DB `CK_FinCashMovement_Amount` menolak nominal 0, dibuat **pengecualian khusus `SALDO-AWAL`** (nol boleh, negatif tetap ditolak). Dialog kunci kini menyebut mutasi untuk setiap penguncian `KAS-KASIR`, termasuk bernilai nol. ⚠️ **Constraint DB baru berlaku setelah migration `RelaxFinCashMovementAmountForZeroOpeningBalance` dijalankan oleh pengguna** (berkasnya sudah ditulis) — lihat [laporan BE-FIN-084](../backend/BE-FIN-084.md) |
| 4 | `Approve` pada baris `APPROVED` dan `Lock` pada baris `LOCKED` dijawab `200` tanpa memeriksa `RowVersion` (idempoten) | Layar tetap mencegah permintaan ganda; idempotensi backend hanya jaring pengaman |
| 5 | `FIN-VAL-184` (tanggal cutover melewati hari ini WIB) hanya diperiksa backend saat **kunci** | Layar menonaktifkan *Kunci* dan konfirmasinya bila tanggal cutover lebih besar dari hari ini (kalender WIB), dengan keterangan; backend tetap penentu akhir |
| 6 | `FIN-VAL-180` (`409`): kelompok sudah punya baris aktif | Form catat hanya menawarkan kelompok yang belum dicatat |
| 7 | `api-contract.md` F.1 masih memberi label **"Rencana (belum tersedia)"** pada endpoint yang sudah ada | ✅ **Ditutup 3 Oktober 2026:** lima endpoint saldo awal kini **Tersedia** (`BE-FIN-066`); lima endpoint pemetaan akun control pada tabel yang sama ikut dikoreksi (`BE-FIN-065`). Bentuk body saldo awal didokumentasikan |

---

## 6. Berkas yang Diubah dan Dibuat

### Baru (`QuilvianSystemFrontendDev`)

| Berkas | Isi |
| --- | --- |
| `src/app/finance/subledger-setup/opening-balances/page.jsx` | Rute tipis: metadata dan `Suspense` |
| `src/components/view/finance/subledger-setup/opening-balances/opening-balances-view.jsx` | View: akses, banner, ringkasan, lima kartu, orkestrasi modal |
| `.../opening-balances/opening-balance-group-card.jsx` | Kartu satu kelompok beserta kendali per status |
| `.../opening-balances/opening-balance-form-modal.jsx` | Modal catat dan ubah |
| `.../opening-balances/opening-balance-confirm-modal.jsx` | Konfirmasi setujui dan kunci |
| `src/lib/hooks/finance/subledger-setup/use-opening-balances.jsx` | Hook: hak akses, modal, penjaga klik ganda, pemulihan benturan |
| `src/lib/state/slice/finance/subledger-setup/finance-opening-balance-slice.jsx` | Slice dan lima thunk |
| `src/lib/constants/finance/subledger-setup/opening-balance-constants.jsx` | Endpoint, urutan kelompok, status, pesan baku |
| `src/utils/finance/subledger-setup/opening-balance-utils.jsx` | Fungsi murni: normalisasi, turunan kendali, validasi, galat, format |
| `src/style/corporate/finance/subledger-setup/opening-balances-view.module.css` | CSS Module |
| `tests/unit/opening-balances-utils.test.mjs` | 22 unit test untuk utilitas (ditulis, **belum dijalankan**) |

### Diubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/store.jsx` | Impor dan registrasi reducer `financeOpeningBalance` (+2 baris) |
| `src/components/view/finance/subledger-setup/control-accounts/subledger-control-accounts-view.jsx` | Impor `next/link`, satu banner info bertautan ke layar saldo awal; perbaikan temuan `FE-FIN-027` (`actions`, `AccessDeniedGate`, `variant`) |
| `.../control-accounts/control-account-table-columns.jsx` dan tiga berkas di `.../control-accounts/modals/` | `variant="outline"` → `"secondary"` (1 baris per berkas) |

Tidak ada perubahan pada repository backend selain berkas laporan ini dan bukti pada roadmap.

---

## 7. Verifikasi dan Validasi

| Pemeriksaan | Perintah | Status | Keterangan |
| --- | --- | :--: | --- |
| Lint | `npm run lint:errors` | `NOT RUN` | Standing instruction pengguna (build/lint/test manual). **Belum terverifikasi — menunggu pengguna** |
| Unit test | `npm run test:unit` atau `node --import ./tests/helpers/register.mjs --test tests/unit/opening-balances-utils.test.mjs` | `NOT RUN` | Alasan sama. 22 test ditulis; hasilnya belum diketahui |
| Build | `npm run build` | `NOT RUN` | Alasan sama; juga hindari bentrok `.next/standalone` yang terkunci aplikasi berjalan |
| Resolusi import | skrip pemeriksa berkas (bukan lint/build) | `PASS` | 12 berkas yang dibuat/diubah; 0 import `@/` atau relatif yang tidak menunjuk ke berkas yang ada |
| `git status --short` | — | `PASS` | Lihat §10 |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Alasan konkret: sesi ini tidak menjalankan server dev (instruksi build/test manual), tidak memegang sesi login, dan tidak memverifikasi bahwa backend + database berjalan dengan `BE-FIN-066` ter-deploy. Hasil di bawah ini **belum pernah diamati**; daftar ini untuk dijalankan pengguna.

| # | Skenario | Yang harus teramati |
|:--:|---|---|
| 1 | Buka layar tanpa baris apa pun | Lima kartu "Belum dicatat"; piutang/utang menampilkan `Rp 0,00` beserta alasan wajib nol |
| 2 | Catat `KAS-KASIR` Rp 50.000.000 tanpa alasan atau tanpa rujukan | Ditolak di form dengan pesan wajib diisi; tidak ada request terkirim |
| 3 | Catat `PIUTANG` | Kolom nominal terkunci `0`; tersimpan sebagai `DRAFT` |
| 4 | Catat `KAS-KECIL` dengan nominal negatif | Ditolak di form (`FIN-VAL-185`) |
| 5 | Ubah baris `DRAFT`, simpan | Nilai baru tampil; `RowVersion` berganti |
| 6 | Klik *Setujui* → konfirmasi, klik cepat dua kali pada *Ya, Setujui* | Hanya satu request; tombol nonaktif sejak klik pertama; baris menjadi `APPROVED` |
| 7 | Baris `APPROVED` dan `LOCKED` | *Ubah* tampil nonaktif dengan keterangan, bukan hilang |
| 8 | *Kunci* baris dengan tanggal cutover di masa depan | Tombol nonaktif dengan keterangan |
| 9 | *Kunci* `KAS-KASIR` bernominal > 0 | Dialog menyebut mutasi `SALDO-AWAL`; sesudahnya muncul satu mutasi `SALDO-AWAL` di Buku Mutasi Kas (`FE-FIN-026`) |
| 10 | Dua sesi: sesi A mengubah baris, sesi B menyetujui dari data lama | Sesi B mendapat banner "sudah diubah oleh orang lain" dan daftar termuat ulang |
| 11 | Pengguna dengan `Read` saja | Tidak ada tombol catat/ubah/setujui/kunci pada baris `DRAFT`/belum dicatat; baris `LOCKED` tetap menampilkan *Ubah* nonaktif |
| 12 | Pengguna tanpa `Read` | Layar akses ditolak |
| 13 | Backend mati | Pesan gagal dimuat dan tombol *Coba Lagi*; **tidak** ada lima kartu "Belum dicatat" |

---

## 8. Pemenuhan Acceptance Criteria

Status "Terpenuhi (source)" berarti terpenuhi menurut pembacaan kode; **belum teramati saat dijalankan**.

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | 1 layar lima kelompok beserta alur `DRAFT`→`APPROVED`→`LOCKED` | Terpenuhi (source); belum terverifikasi runtime | `buildOpeningBalanceRows` selalu menghasilkan lima baris; kendali per status di `deriveRowControls`; unit test urutan dan kendali |
| 2 | Kolom alasan dan rujukan dokumen Accounting **wajib** | Terpenuhi (source) | `validateOpeningBalanceForm` (`FIN-VAL-182`); field `required`; unit test |
| 3 | Setujui dan kunci **dinonaktifkan sejak permintaan dikirim**, tidak dapat ditarik | Terpenuhi (source) | `ConfirmModal` `loading` mengunci tombol; `inFlightRef` di hook menahan klik pada tick yang sama; `pendingAction` per baris menonaktifkan kendali; dialog menyatakan tidak dapat ditarik |
| 4 | Baris `LOCKED` menampilkan kendali ubah **dinonaktifkan**, bukan disembunyikan | Terpenuhi (source) | `showEdit` selalu `true` untuk baris yang ada; `editDisabled` untuk non-`DRAFT`; unit test, termasuk pengguna tanpa hak ubah |
| 5 | Piutang dan utang menampilkan nol beserta alasannya, bukan kolom kosong | Terpenuhi (source) | Kartu menampilkan `Rp 0,00` dan teks wajib nol untuk tiga kelompok migrasi, baik sudah dicatat maupun belum; `FIN-VAL-181` dicegah di form |
| 6 | Layar **tidak** menjanjikan pemisahan penyiap dan penyetuju dijamin sistem | Terpenuhi (source) | Banner permanen `SEPARATION_DISCLAIMER` di atas daftar; tidak ada teks lain yang menyiratkan pemisahan |
| 7 | Benturan `RowVersion` ditampilkan sebagai "sudah diubah orang lain" dan meminta muat ulang | Terpenuhi (source) | `isStaleConflict`, `recoverFromStale`, banner dengan *Muat Ulang Data*; unit test |
| 8 | Loading, empty, error, retry, unauthorized ditangani | Terpenuhi (source) | Memuat; gagal + *Coba Lagi* tanpa menampilkan kartu palsu; `AccessDeniedGate` untuk tanpa `Read`/`401`/`403` |
| 9 | Menu: sebagian tertahan `FIN-OQ-079` | Terpenuhi | §4 |
| 10 | `npm run lint` dan `npm run build` PASS | **Belum terpenuhi — NOT RUN** | §7 |

---

## 9. Risiko dan Hal yang MUST Disampaikan Saat Menyerahkan Modul

1. **`FIN-PERM-1.7` G.5 — pemisahan penyiap dan penyetuju tidak dijaga mesin.** Satu orang dapat memegang `Create` dan `Approve` sekaligus dan melewati maker-checker. Pencegahannya hanya **pemberian hak oleh admin**. Layar menyatakan ini terang-terangan dan **MUST** disebutkan ulang saat modul diserahkan.
2. **Penguncian tidak dapat dibatalkan**, termasuk mutasi kas `SALDO-AWAL` yang diterbitkannya. Itu sebabnya konfirmasi eksplisit dipasang.
3. **Validasi sisi klien** (`FIN-VAL-181`, `182`, `185`, dan penahan kunci untuk `FIN-VAL-184`) menduplikasi aturan backend demi umpan balik cepat. Bila aturan backend berubah, utilitas harus disesuaikan; backend tetap penentu akhir.
4. **Delta kontrak #3** ditutup di source, tetapi penguncian `KAS-KASIR` bernominal 0 **akan gagal di database** sampai migration constraint dijalankan (`BE-FIN-084`).
5. **Kata "Draf"** dipakai untuk status `DRAFT` pada label tampilan; kode status tetap `DRAFT`.

### Temuan pada `FE-FIN-027` — ✅ diperbaiki 3 Oktober 2026 atas permintaan pemilik

Terlihat saat mempelajari `FE-FIN-027` sebagai pola, diperiksa terhadap komponen sumbernya, lalu diperbaiki:

- `Hero` diberi `action`, padahal `Hero` hanya membaca `actions` → diganti `actions`, sehingga tombol *+ Tambah Pemetaan Akun* dapat tampil.
- `AccessDeniedGate` diberi `message` dan `returnUrl`, padahal komponen hanya membaca `error`/`actionError`/`children` → diganti `error={{ statusCode: 403, message }}`. Tautan kembali (`returnUrl`) dibuang karena komponen tidak mendukungnya.
- `BaseButton variant="outline"` (4 berkas: kolom tabel dan tiga modal) → `variant="secondary"`; varian `outline` memang tidak ada.

Perbaikan ini **belum terverifikasi runtime** (lint/build NOT RUN); khususnya, tampilnya tombol Tambah dan pesan akses ditolak perlu diamati manual.

---

## 10. Status Git (frontend)

Branch `yasmina`, tidak ada stage, commit, push, pull, merge, rebase, atau deploy.

```text
 M src/components/view/finance/subledger-setup/control-accounts/subledger-control-accounts-view.jsx
 M src/lib/state/store.jsx
?? src/app/finance/subledger-setup/opening-balances/
?? src/components/view/finance/subledger-setup/opening-balances/
?? src/lib/constants/finance/subledger-setup/opening-balance-constants.jsx
?? src/lib/hooks/finance/subledger-setup/use-opening-balances.jsx
?? src/lib/state/slice/finance/subledger-setup/finance-opening-balance-slice.jsx
?? src/style/corporate/finance/
?? src/utils/finance/subledger-setup/opening-balance-utils.jsx
?? tests/unit/opening-balances-utils.test.mjs
```

Sebelum task ini, working tree frontend bersih.

---

## 11. Langkah Berikutnya

Jalankan manual di `QuilvianSystemFrontendDev`: `npm run lint:errors`, lalu `npm run test:unit`, lalu `npm run build`, dan laporkan hasilnya. Setelah ketiganya PASS dan skenario §7 teramati, task dapat ditandai ✅ pada roadmap.
