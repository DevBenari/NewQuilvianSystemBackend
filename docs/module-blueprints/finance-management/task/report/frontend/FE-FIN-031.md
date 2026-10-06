# Laporan Perubahan Frontend — `FE-FIN-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-031` |
| Judul | Pejabat berwenang dapat melihat dan mengubah ambang pembayaran langsung, dan setiap perubahan membawa alasannya — Layar Master Ambang Pembayaran Langsung |
| Slice | `REV-14D` — `EPIC FIN-23`, pembayaran langsung berkontrol beserta bukti |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian REV-14D dan REV-14E (frontend) |
| Trace | `FR-FIN-163`..`165`; `FIN-DEC-134`; `FIN-DES-086`; `03-frontend-architecture.md` 19.2 nomor 10 |
| Contract version | `FIN-API-1.6` F.4 (`contract_status: approved 2026-10-02`); `FIN-PERM-1.8` H.1; `FIN-PERM-1.7` G.5; `FIN-VAL-1.8` `FIN-VAL-197`, `205`, `206` |
| Wewenang UI | Frontend Owner — **sebagian tertahan** `FIN-OQ-080` (apakah angka ambang ditampilkan kepada staf AR/AP belum diputuskan) dan `FIN-OQ-079` (menu). **Mengikat:** alasan kosong dicegah di layar; `404` dinyatakan sebagai seluruh pembayaran langsung sedang ditolak; layar tidak menjanjikan jenjang persetujuan. `DEV_DISCRETION`: tata letak kartu dan form, warna |
| Dependency | `BE-FIN-076` 🟡 (dua endpoint ambang; source lengkap; pengguna melaporkan `dotnet build` sukses setelah task ini ditulis). Penerapan migration `AddFinanceTransactionProofAndDirectPaymentThreshold` (tabel `MstDirectPaymentThreshold`) **tidak dikonfirmasi** dalam sesi ini |
| Klasifikasi | `MEDIUM` — 1 rute baru, 1 view, 1 hook, 1 slice, 1 berkas konstanta, 1 berkas utilitas, 1 CSS Module, 1 unit test, 1 registrasi store |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only (tidak diubah); laporan dan bukti roadmap ditulis di repository backend |
| Branch | `yasmina` (upstream `origin/yasmina`); penetapan eksplisit pemegang modul belum dikonfirmasi, sama seperti task sebelumnya |
| Tanggal | 3 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak. `npm run lint:errors`, `npm run test:unit`, `npm run build` **NOT RUN** (standing instruction: dijalankan manual oleh pengguna). Verifikasi manual UI **NOT FEASIBLE** dari sesi ini (§7). Keputusan `FIN-OQ-080` masih terbuka dan layar menahan diri (§6) |

---

## 1. Masalah yang Diperbaiki

`BE-FIN-076` menyediakan jalur baca dan ubah ambang pembayaran langsung, tetapi pejabat berwenang tidak punya layar untuk menetapkannya:

1. **Tanpa ambang, seluruh pembayaran langsung ditolak** (fail-closed, `FIN-DES-086`). Selama tidak ada layar, `PUT` hanya dapat dipanggil lewat alat teknis, sehingga jalur pembayaran langsung (`FE-FIN-030`) tidak pernah dapat dipakai.
2. **Alasan perubahan wajib** (`FIN-DEC-134`). Backend menolak alasan kosong dengan `422`, tetapi pengguna seharusnya tidak perlu sampai ke penolakan itu.
3. **Keadaan "belum ditetapkan" harus terbaca sebagai keadaan nyata.** Kolom kosong atau galat teknis akan menyembunyikan fakta bahwa pembayaran langsung sedang ditolak seluruhnya.
4. **Risiko anggapan adanya jenjang persetujuan.** Mesin hak akses tidak mengenal jenjang atas perubahan ambang (`FIN-PERM-1.7` G.5): pejabat berwenang dapat menaikkan ambang lalu membayar di bawahnya. Layar yang diam tentang ini dapat dibaca sebagai jaminan adanya pengawasan.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Pelaku:** pejabat berwenang ambang (hak `MstDirectPaymentThreshold` `Read` **dan** `Update`).

1. Pejabat membuka `/finance/master-data/direct-payment-threshold`. Layar memuat `GET /master-data/direct-payment-threshold`.
2. **Ambang sudah ada:** kartu *Ambang yang berlaku* menampilkan nilai, alasan perubahan terakhir, dan waktu perubahan terakhir. Form *Ubah ambang* memuat nilai saat ini; kolom alasan selalu kosong.
3. **Ambang belum pernah ditetapkan (`404`):** banner merah *"Ambang belum ditetapkan — seluruh pembayaran langsung sedang ditolak"* beserta penjelasannya; form berjudul *Tetapkan ambang pertama*.
4. Pejabat mengisi nilai dan alasan. Alasan kosong atau nilai tidak sah **dicegah di layar** — tidak ada permintaan terkirim.
5. *Ubah Ambang* membuka dialog konfirmasi: ambang sekarang → ambang baru, alasan, dan pernyataan bahwa perubahan berlaku langsung tanpa jenjang persetujuan lain.
6. Konfirmasi → `PUT`. Berhasil: kartu nilai diperbarui dari respons, banner sukses, kolom alasan dikosongkan. Gagal: pesan backend (`FIN-VAL-205`/`206`) muncul di dialog.
7. Pengguna yang hanya punya `Read` **tidak** melihat layar ini (§6).

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

| No | Elemen | Keputusan | Sumber |
|:--:|---|:--:|---|
| 1 | Header halaman | `REUSE` | `base-features/hero` |
| 2 | Banner pembatasan, tanpa jenjang persetujuan, sukses, galat, memuat, belum ditetapkan | `REUSE` | `base-features/information-alert` |
| 3 | Input nilai dan area alasan | `REUSE` | `base-features/base-form-control` (`BaseTextField`, `BaseTextAreaField`) |
| 4 | Tombol | `REUSE` | `base-features/base-button` |
| 5 | Dialog konfirmasi | `REUSE` | `base-features/confirm-modal` |
| 6 | Penutup akses | `REUSE` | `base-features/access-denied-gate` |
| 7 | Pembungkus halaman | `REUSE` | `base-data-components.module.css` (`dataPage`, `dataShell`) |

**Hasil UI Gate:** 7 `REUSE`, 0 komponen baru. Tidak ada base component, wrapper, atau abstraksi generik baru. Satu CSS Module (`src/style/corporate/finance/master-data/direct-payment-threshold-view.module.css`): kelas halaman memakai token `--base-*` dari `.dataPage`; kelas di dalam modal memakai token global karena modal dirender lewat portal.

---

## 4. Cara Mencapai Layar Selama Menu Tertahan (`FIN-OQ-079`)

Butir menu **tidak** dipasang (bukan `DEV_DISCRETION`). Layar dicapai lewat **URL langsung**: `/finance/master-data/direct-payment-threshold`. Tidak ada tautan dari layar lain: layar pembayaran langsung (`FE-FIN-030`) sengaja tidak menyebut angka ambang selama `FIN-OQ-080` terbuka, dan menautkannya ke layar ini hanya bermanfaat bagi pemegang hak ubah.

---

## 5. Endpoint yang Dikonsumsi dan Pemetaan Kontrak

Base URL: `/api/v1/corporate/finance-management/master-data/direct-payment-threshold`

| Method | Path | Hak akses | Dipanggil lewat | Catatan |
| :-- | :-- | :-- | :-- | :-- |
| `GET` | `/` | `MstDirectPaymentThreshold : Read` | `fetchDirectPaymentThreshold` | `signal` diteruskan; `404` ber-pesan domain = belum ditetapkan |
| `PUT` | `/` | `MstDirectPaymentThreshold : Update` | `updateDirectPaymentThreshold` | Body: `amount`, `changeReason`, `effectiveFrom` |

### Delta antara kontrak, backend saat ini, dan layar

Dibaca dari `DirectPaymentThresholdDtos.cs`, `DirectPaymentThresholdService.cs`, dan controller. Backend tidak diubah.

| # | Temuan | Perlakuan di layar |
|:--:|---|---|
| 1 | `EffectiveFrom` diterima dan disimpan, tetapi jalur pembayaran langsung (`FinanceReceivableService`, `FinanceSupplierPayableService`) **tidak membacanya**: ambang yang tersimpan berlaku seketika | Layar **tidak** menanyakan atau menampilkan tanggal berlaku (agar tidak menjanjikan perubahan terjadwal); mengirim tanggal bisnis hari ini (WIB) sebagai tanggal perubahan, sesuai perilaku sebenarnya. **Perlu keputusan pemilik:** apakah `EffectiveFrom` memang akan ditegakkan |
| 2 | `EffectiveFrom` tidak divalidasi; bila tidak dikirim, tersimpan `0001-01-01` | Layar selalu mengirimnya |
| 3 | `LastChangedBy` hanya `Guid`, tanpa nama | Layar menampilkan **waktu** perubahan terakhir saja, bukan siapa. Sama seperti `ApprovedBy` pada saldo awal; dapat dicarikan nama bila pemilik menginginkannya |
| 4 | Master ini **tidak punya `RowVersion`**: dua pejabat yang mengubah bersamaan saling menimpa tanpa peringatan | Tidak ada deteksi stale di layar; yang tersedia hanya waktu perubahan terakhir pada kartu. Dicatat sebagai risiko (§9) |
| 5 | `404` pada `GET` juga akan muncul bila rute belum terpasang di backend | Layar hanya menyatakan "belum ditetapkan" bila `404` **membawa pesan domainnya** (*"...belum ditetapkan..."*); `404` lain diperlakukan sebagai galat biasa dengan tombol *Coba Lagi* |
| 6 | Backend mengizinkan `GET` bagi pemegang `Read` saja | Layar lebih ketat dari backend: butuh `Read` **dan** `Update` (`FIN-OQ-080`). Pembatasan ini hanya di layar; backend tidak menutup angkanya bagi pemegang `Read` |
| 7 | Perubahan dicatat `LoggerService.AuditAsync` beserta `ChangeReason` dan pelaku (dibaca dari kode) | Layar menyatakan alasan "tercatat pada log beserta pelakunya" |
| 8 | `api-contract.md` F.4 masih berlabel **"Rencana (belum tersedia)"** pada kedua endpoint | Tidak diubah pada task ini (artefak kontrak di luar wewenang tulis task ini); dilaporkan |

---

## 6. Wewenang UI yang Dipakai dan Cara Layar Menahan Diri (`FIN-OQ-080`)

`FIN-OQ-080` (apakah **angka** ambang ditampilkan kepada staf AR/AP) belum dijawab pemilik. Cara layar menahan diri:

1. Layar hanya terbuka bagi pengguna dengan `MstDirectPaymentThreshold` `Read` **dan** `Update`. Selain itu, yang tampil adalah penutup akses beserta alasan.
2. Layar pembayaran langsung (`FE-FIN-030`) tetap tidak menyebut angkanya; tidak ada perubahan pada layar itu.
3. Layar ini menyatakan sendiri di bagian atas bahwa ia dibatasi untuk pemegang hak ubah dan bahwa keputusan penayangan angka kepada staf lain belum ada.

**Invariant yang dipenuhi:** alasan kosong dicegah di layar; `404` berbunyi seluruh pembayaran langsung sedang ditolak (keadaan nyata, bukan kolom kosong); layar menyatakan **tanpa jenjang persetujuan** dan tidak memuat satu pun kata yang menyiratkan persetujuan (unit test menjaga ini).

---

## 7. Berkas yang Diubah dan Dibuat

### Baru (`QuilvianSystemFrontendDev`)

| Berkas | Isi |
| --- | --- |
| `src/app/finance/master-data/direct-payment-threshold/page.jsx` dan `direct-payment-threshold-client.jsx` | Rute tipis (pola `currency`) |
| `src/components/view/finance/master-data/direct-payment-threshold/direct-payment-threshold-view.jsx` | View: akses, banner, kartu nilai, form, dialog konfirmasi |
| `src/lib/hooks/finance/master-data/direct-payment-threshold/use-direct-payment-threshold.jsx` | Hook: hak akses, form, validasi, penjaga klik ganda, konfirmasi |
| `src/lib/state/slice/finance/master-data/master-data-direct-payment-threshold-slice.jsx` | Slice dan dua thunk |
| `src/lib/constants/finance/master-data/direct-payment-threshold/direct-payment-threshold-constants.jsx` | Endpoint, hak akses, pesan baku |
| `src/utils/finance/master-data/direct-payment-threshold/direct-payment-threshold-utils.jsx` | Fungsi murni: normalisasi, parsing, validasi, payload, klasifikasi galat, format |
| `src/style/corporate/finance/master-data/direct-payment-threshold-view.module.css` | CSS Module |
| `tests/unit/direct-payment-threshold-utils.test.mjs` | 12 unit test (ditulis, **belum dijalankan**) |

### Diubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/store.jsx` | Impor dan registrasi reducer `financeDirectPaymentThreshold` (+2 baris) |

Tidak ada perubahan pada repository backend selain berkas laporan ini dan bukti roadmap.

---

## 8. Verifikasi dan Validasi

| Pemeriksaan | Perintah | Status | Keterangan |
| --- | --- | :--: | --- |
| Lint | `npm run lint:errors` | `NOT RUN` | Standing instruction pengguna. **Menunggu pengguna** |
| Unit test | `npm run test:unit` atau `node --import ./tests/helpers/register.mjs --test tests/unit/direct-payment-threshold-utils.test.mjs` | `NOT RUN` | 12 test ditulis; hasil belum diketahui |
| Build | `npm run build` | `NOT RUN` | Alasan sama |
| Resolusi import dan named export | skrip pemeriksa berkas | `PASS` | 9 berkas; 0 masalah |
| `git status --short` | — | `PASS` | §10 |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Alasan konkret: sesi ini tidak menjalankan server dev, tidak memegang sesi login, dan tidak memverifikasi bahwa backend berjalan dengan migration `MstDirectPaymentThreshold` terapan. Belum satu pun perilaku di bawah ini pernah teramati.

| # | Skenario | Yang harus teramati |
|:--:|---|---|
| 1 | Pengguna dengan `Read` saja | Penutup akses beserta alasan; **tidak ada** permintaan `GET` terkirim |
| 2 | Pengguna dengan `Read` + `Update`, **belum ada baris ambang (`404`)** | Banner merah "seluruh pembayaran langsung sedang ditolak"; tidak ada kartu nilai; form berjudul *Tetapkan ambang pertama* |
| 3 | Isi alasan kosong atau hanya spasi, klik *Tetapkan/Ubah* | Galat di kolom alasan; **tidak ada** permintaan terkirim; dialog tidak terbuka |
| 4 | Isi nilai `0`, `-5`, `1.250,50`, `abc` | Galat di kolom nilai; tidak ada permintaan terkirim |
| 5 | Tetapkan ambang pertama dengan nilai dan alasan sah | Dialog menyebut "Ambang yang ditetapkan"; konfirmasi → banner sukses "berhasil ditetapkan"; banner merah hilang; kartu nilai muncul |
| 6 | Ubah ambang yang sudah ada | Dialog menampilkan nilai sekarang → baru dan alasan; setelah konfirmasi kartu menampilkan nilai baru, alasan terakhir, dan waktu; kolom alasan kosong kembali |
| 7 | Klik cepat dua kali pada *Ya, Ubah Ambang* | Hanya satu `PUT`; tombol nonaktif selama permintaan |
| 8 | Teks dialog dan banner | Menyatakan tanpa jenjang persetujuan; **tidak** menyiratkan persetujuan |
| 9 | `PUT` ditolak backend (mis. `422`) | Pesan backend tampil di dalam dialog; dialog tetap terbuka |
| 10 | Backend mengembalikan `404` tanpa pesan domain (rute tidak ada) | **Bukan** banner "ditolak"; galat biasa dengan *Coba Lagi* |
| 11 | Zona waktu | "Terakhir diubah" tampil dalam WIB |
| 12 | Layar pembayaran langsung (`FE-FIN-030`) setelah ambang ditetapkan | Pembayaran di bawah ambang diterima, di atas ambang ditolak `422`; layar **tidak** menyebut angka ambang |

---

## 9. Pemenuhan Acceptance Criteria

Status "Terpenuhi (source)" = terpenuhi menurut pembacaan kode; **belum teramati saat dijalankan**.

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | 1 layar master ambang: nilai aktif, alasan perubahan terakhir, form ubah | Terpenuhi (source) | Kartu *Ambang yang berlaku*, form *Ubah ambang* |
| 2 | `ChangeReason` **wajib**; kosong dicegah **di layar**, bukan diserahkan ke backend | Terpenuhi (source) | `validateThresholdForm`; hook tidak mengirim bila tidak sah; unit test |
| 3 | Keadaan "belum pernah ditetapkan" (`404`) berbunyi **seluruh pembayaran langsung sedang ditolak** — bukan kolom kosong | Terpenuhi (source) | Banner `NOT_SET_*`; hanya bila `404` membawa pesan domain; unit test |
| 4 | Layar **MUST NOT** menjanjikan jenjang persetujuan atas perubahan ambang | Terpenuhi (source) | Banner dan dialog menyatakan tanpa jenjang; unit test menjaga redaksi |
| 5 | Keputusan `FIN-OQ-080` terbuka dan cara layar menahan diri disebut di laporan | Terpenuhi | §6 |
| 6 | Cara mencapai layar selama menu tertahan disebut | Terpenuhi | §4 |
| 7 | Verifikasi manual jalur `404` dan alasan kosong | **NOT FEASIBLE** | §8 skenario 2 dan 3 |
| 8 | `npm run lint` dan `npm run build` PASS | **Belum terpenuhi — NOT RUN** | §8 |

---

## 10. Risiko

1. **Tanpa `RowVersion`, perubahan bersamaan saling menimpa tanpa peringatan** (delta 4). Karena tidak ada jenjang persetujuan dan perubahan berlaku seketika, dampaknya langsung ke seluruh pembayaran langsung. Usulan di luar task ini: `RowVersion` pada `MstDirectPaymentThreshold`.
2. **`FIN-PERM-1.7` G.5 tetap berlaku:** pejabat berwenang dapat menaikkan ambang lalu membayar di bawahnya. Pengamannya hanya alasan wajib dan log. Layar menyatakan ini; `MUST` disampaikan saat menyerahkan modul.
3. **`EffectiveFrom` tersimpan tetapi tidak ditegakkan** (delta 1). Bila pemilik berniat perubahan terjadwal, perilaku backend belum sesuai.
4. **Pembatasan `FIN-OQ-080` hanya di layar.** Siapa pun dengan `Read` tetap dapat memanggil `GET` langsung dan melihat angkanya; menutupnya adalah keputusan backend/hak akses, bukan layar.
5. **Migration belum dikonfirmasi diterapkan.** Bila tabel tidak ada, `GET` gagal dan layar menampilkan galat biasa dengan *Coba Lagi*, bukan banner "belum ditetapkan".
6. **Nilai ambang awal (`FIN-OQ-074`) belum diputuskan pemilik.** Layar tidak mengisi nilai bawaan.

### Temuan di luar cakupan (`UNRELATED EXISTING ISSUE`, tidak diperbaiki)

- `BE-FIN-076` melaporkan status 🟡 dengan `dotnet build` NOT RUN pada roadmap; pengguna kemudian melaporkan build sukses, tetapi laporan itu belum diperbarui. Bukan wewenang task frontend.

---

## 11. Status Git (frontend)

Branch `yasmina`; tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Working tree juga memuat pekerjaan `FE-FIN-027`..`029` yang belum di-commit (tidak diulang di bawah).

```text
 M src/lib/state/store.jsx
?? src/app/finance/master-data/direct-payment-threshold/
?? src/components/view/finance/master-data/direct-payment-threshold/
?? src/lib/constants/finance/master-data/direct-payment-threshold/
?? src/lib/hooks/finance/master-data/direct-payment-threshold/
?? src/lib/state/slice/finance/master-data/master-data-direct-payment-threshold-slice.jsx
?? src/style/corporate/finance/master-data/
?? src/utils/finance/master-data/direct-payment-threshold/
?? tests/unit/direct-payment-threshold-utils.test.mjs
```

`store.jsx` juga memuat registrasi reducer `FE-FIN-028`.

---

## 12. Langkah Berikutnya

Jalankan manual: `npm run lint:errors`, `npm run test:unit`, `npm run build`, lalu amati skenario §8 — terutama nomor 2, 3, dan 7. Pemilik perlu memutuskan: apakah `EffectiveFrom` ditegakkan, apakah `RowVersion` ditambahkan, dan `FIN-OQ-080`. Setelah itu task dapat ditandai ✅.
