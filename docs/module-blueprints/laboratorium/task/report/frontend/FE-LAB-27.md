# Laporan Perubahan Frontend — `FE-LAB-27`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-27` |
| Judul | Dua layar data induk Patologi Anatomi |
| Slice | `S4c` — gelombang `MVP-6b1` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6e.5 |
| Trace | `FR-13.11`; `LAB-DEC-086`, `LAB-DEC-087`; `INV-39`; `AC-143`, `AC-144`, `AC-145`; `VAL-99`, `VAL-100`, `VAL-101` |
| Contract version | `LAB-API-v1` **`r25`** bagian 20.3 + **`r32`** bagian 27 — keduanya `approved` |
| Wewenang UI | Dua fitur data induk baru di `master-data/` + satu layar penggolongan. Nol layar Laboratorium lain disentuh |
| Dependency | `BE-LAB-50` ✅, **`BE-LAB-66`** ✅ — dan **izin jabatan Kepala Instalasi sudah diberikan** 2026-09-23 |
| Klasifikasi | `HEAVY` — 3 acceptance criteria, 2 fitur data induk penuh + 2 bagian non-standar |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `6ea61bcad` (branch `YogaV2`) |
| Tanggal | 2026-09-23 |
| Status | ✅ **`SELESAI`** (naik 2026-10-02) — ketiga AC terbukti **di peramban** dengan akun Kepala Instalasi asli terhadap backend lokal dan PostgreSQL dev: layar 15/15, ulang menu aksi 2/2, tata letak 390 px 3/3 sesudah satu perbaikan. Lihat 8. *(Semula 2026-09-23: ⚠ — ketiga AC terbangun, 10 uji unit baru lulus, lint 0 error, build hijau, kesembilan route terkompilasi; belum diklik di peramban.)* |

---

## 1. Masalah yang diperbaiki

`BE-LAB-50` mendirikan keempat tabel data induk Patologi Anatomi 2026-09-18, dan `BE-LAB-66`
melengkapi permukaannya 2026-09-23. **Nol satu pun layar memanggilnya.**

Akibatnya berantai: tanpa parameter dan golongan yang dapat dikelola, keberlakuan nol dapat
disusun; tanpa keberlakuan, formulir laporan Patologi Anatomi **kosong bagi setiap pesanan**;
dan tanpa penggolongan jenis pemeriksaan, pesanan nol punya golongan sama sekali (`INV-39`).
`FE-LAB-28` menunggu persis rantai ini.

---

## 2. Proses bisnis

**Pelaku.** Kepala instalasi laboratorium (`DR-LAB-003` untuk penggolongan).

**Rantai yang membentuk formulir laporan:**

```text
Parameter (ruas isian)  ──┐
                          ├─→ Keberlakuan per golongan ──┐
Golongan (kategori PA)  ──┘                              ├─→ Formulir laporan
                                                         │
Jenis pemeriksaan katalog ─→ Penggolongan ─→ Golongan ───┘
```

**Langkah berurutan:**

1. Kepala instalasi mendaftarkan **parameter** — ruas isian seperti `MAKROSKOPIK`.
2. Mendaftarkan **golongan** — kategori seperti `HISTO`.
3. Membuka satu golongan, lalu menyusun **keberlakuan**: ruas mana yang berlaku, dan mana
   yang **wajib**.
4. Membuka layar **penggolongan**, memeriksa usulan satu per satu, lalu mengonfirmasinya.

**Jalur tidak normalnya, dan ini yang paling penting.** Golongan tanpa satu pun ruas
menghasilkan formulir **kosong sama sekali**, dan `VAL-100` baru menyebutnya ketika patolog
sudah membuka layar hasil. Layar keberlakuan karena itu menyatakannya lebih awal, dan kolom
`Jumlah Ruas` membuatnya terlihat langsung dari daftar.

Contoh berangka. Katalog memuat 10 jenis pemeriksaan PA; 4 sudah digolongkan. Layar
penggolongan berbunyi *"4 dari 10 jenis pemeriksaan sudah digolongkan. **6 belum.**"* — angka
yang datang dari `unmappedProcedure` milik `BE-LAB-66`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/master-data-feature-standard.md`; kontrak `r25` dan
`r32`; `03-frontend-architecture.md` bagian 13.2; fitur `lab-organisms` hasil `FE-LAB-24`
sebagai bentuk induk; `LabPathologyMasterDataDtos.cs` sebagai bukti bentuk permintaan;
`use-select-resource.jsx` dan registry pemilihnya.

### 3.2 Berkas yang berubah

| Kelompok | Jumlah | Isi |
| --- | ---: | --- |
| Fitur `lab-pathology-parameters` | **13** | 4 route + client, 3 tampilan, 1 konstanta, 3 hook, 1 utils |
| Slice parameter | 1 | `master-data-lab-pathology-parameter-slice.jsx` |
| Fitur `lab-pathology-categories` | **13** | Bentuk yang sama |
| Slice golongan | 1 | `master-data-lab-pathology-category-slice.jsx` |
| Keberlakuan ruas | 1 | `lab-pathology-category-parameters-section.jsx` |
| Penggolongan pemeriksaan | 2 | 1 tampilan + 1 route |
| Service bersama | 1 | `lab-pathology-mapping.service.js` — 7 fungsi |
| Aturan murni | 1 | `lab-pathology-mapping-rules.js` — 6 fungsi |
| Registrasi | 3 | `store.jsx` (2 slice), `menu-items.jsx` (3 entri), registry pemilih (2 resource) |
| Uji | 1 | `lab-pathology-mapping-fe27-rules.test.mjs` — 10 uji |

**Total 37 berkas.** Nol berkas di luar `master-data/` dan `laboratory-management/` disentuh;
nol komponen base diubah.

### 3.3 Empat keputusan pelaksanaan

**1. Fitur diturunkan dari `lab-organisms`, bukan ditulis ulang.** `master-data-feature-standard`
menetapkan bentuknya, dan `FE-LAB-24` sudah membuktikan bentuk itu berjalan. Menyalin lalu
mengadaptasi membuat ketiga layar berperilaku **sama persis** dengan layar data induk lain —
yang justru dituntut standar. Seluruh teks domain ditulis ulang; nol kalimat organisme
tertinggal.

**2. Nol thunk `/options`, mengikuti penyimpangan yang sudah dicatat `FE-LAB-24`.** Kedua
alamat didaftarkan di registry pemilih bersama. Menuliskan alamat yang sama dua kali persis
yang dilarang komentar di sana.

**3. Keberlakuan dirender DI BAWAH kartu detail, bukan di dalamnya.** `BaseDetailView` komponen
tertutup yang dipakai seluruh modul; menyisipkan bagian ke dalamnya berarti mengubah komponen
base demi satu layar.

**4. Parameter nonaktif yang sudah terlanjur berlaku tetap ditampilkan.** Mencabutnya diam-diam
akan mengubah bentuk formulir laporan tanpa satu pun manusia memutuskannya. Ia ditandai
`Nonaktif` supaya kepala instalasi melihat keadaannya dan memilih sendiri.

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Pathology Parameter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata`, `/summary`, `/`, `/options`, `/{id}` | Bentuk layar, ringkasan, daftar, pilihan, detail | `LabPathologyParameter : Read` |
| `POST` | `/` | Menambah ruas isian | `LabPathologyParameter : Create` |
| `PUT` | `/{id}` | Mengubah label dan urutan | `LabPathologyParameter : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan | `LabPathologyParameter : Update` |

#### Health Services / Laboratory Management / Lab Pathology Category

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata`, `/summary`, `/`, `/options`, `/{id}` | Sama, ditambah `parameterCount` | `LabPathologyCategory : Read` |
| `GET` | `/{id}/parameters` | Keberlakuan ruas satu golongan | `LabPathologyCategory : Read` |
| `PUT` | `/{id}/parameters` | Menyusun ulang keberlakuan — **penggantian utuh** | `LabPathologyCategory : Update` |
| `POST`/`PUT`/`PATCH` | `/`, `/{id}`, `/{id}/status` | Tambah, ubah, status | `Create`/`Update` |

#### Health Services / Laboratory Management / Lab Procedure Pathology Category

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/`, `/summary`, `/suggestions` | Daftar tersimpan, angka tersisa, usulan | `LabPathologyCategory : Read` |
| `POST` | `/` | Menyimpan satu penggolongan **yang sudah dikonfirmasi manusia** | `LabPathologyCategory : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | **0 error** | `PASS` | Keluaran perintah |
| Uji unit berkas ini | **10/10 lulus** | `PASS` | Keluaran perintah |
| Seluruh suite unit | **1659 lulus, 7 gagal** | `PASS` | Naik dari 1633 baseline; gagal **tetap 7** |
| Ketujuh kegagalan diperiksa satu per satu | **Nol terkait** | `PASS` | 5 `FE-RWI`, 1 bank darah, 1 menuntut menu `/corporate/accounting/reconciliation` yang memang belum ada |
| `npm run build` | **Hijau** | `PASS` | `✓ Compiled successfully` |
| Route terkompilasi | **9 route** | `PASS` | 4 parameter, 4 golongan, 1 penggolongan |

Uji manual: **`NOT FEASIBLE`** — sesi ini nol punya alat kendali peramban.

**Endpoint-nya sendiri sudah terbukti berjalan** pada sesi yang sama lewat `BE-LAB-66`:
kesebelasnya dijawab `200` bagi akun Kepala Instalasi, dan ringkasan penggolongan
mengembalikan `10 / 4 / 6` yang cocok dengan kenyataan tercatat.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-143` **nol tombol Hapus** | ✅ **Terpenuhi** | Kedua konfigurasi menyetel `isDeletable: false`; halaman detail merender **dua** aksi saja; backend pun nol punya `DELETE` |
| `AC-144` layar pemetaan menyediakan usulan dari `GET /suggestions` yang **wajib dikonfirmasi manusia** | ✅ **Terpenuhi** (2026-10-02, di peramban) | **Nol tombol "terapkan semua"**; tiap baris punya tombol `Konfirmasi` sendiri yang **mati** selama golongan belum dipilih; kata kunci pencocok ditampilkan; 4 uji; layar A8–A11 di 8 |
| `AC-145` layar keberlakuan menampilkan penanda **wajib** per pasangan parameter-kategori | ✅ **Terpenuhi** (2026-10-02, di peramban) | Kolom `Wajib` per baris, tersimpan per pasangan; 6 uji; layar A6–A7 di 8 |
| DoD — kedua layar berjalan | ✅ | 9 route terkompilasi; ketiga layar dibuka dengan akun asli (8) |
| DoD — `AC-143`..`AC-145` terbukti | ✅ **Ketiganya terbukti di peramban** | 8 — sebelumnya ⚠ dua dari tiga baru terbukti pada lapis aturan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan lint baru |
| Masalah yang diketahui | ~~Ketiga layar belum diklik di peramban; tata letak kedua bagian non-standar pada layar sempit belum dilihat.~~ **Tertutup 2026-10-02** — diklik dengan akun asli, dan dugaan tata letak sempit **terbukti**: kolom `Wajib` dan tombol `Konfirmasi` terdorong keluar layar pada 390 px. Diperbaiki (8.2) |
| Risiko tersisa | **Sedang, dan terpusat pada `AC-144`.** Godaan menambahkan "terapkan semua" akan muncul begitu kepala instalasi menghadapi puluhan baris — dan itu menghapus satu-satunya pemeriksaan manusia yang menjaga penggolongan. Ketiadaannya **disengaja** dan dicatat pada komentar berkasnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 37 berkas tersentuh, seluruhnya dalam cakupan. **Nol operasi Git dijalankan** |
| Langkah berikutnya | ~~**1.** Klik ketiga layar terhadap backend berisi data.~~ (selesai 2026-10-02) **2.** Isi penggolongan bagi pemeriksaan yang tersisa — **pekerjaan data kepala instalasi**, bukan kode. ~~**3.** Tinjau tata letak kedua bagian non-standar pada layar sempit.~~ (selesai dan diperbaiki 2026-10-02) |

---

## 8. Verifikasi susulan 2026-10-02 — di peramban dengan akun asli

**Lingkungan.** Backend lokal (`dotnet run`, `Development`) terhadap PostgreSQL dev bersama; `next dev`
port 3000 dari working tree `YogaV2`; Chromium lewat Playwright; login **lewat formulir** dengan akun
**dr. Bima (Kepala Instalasi)**. Penjaga tulis aktif: setiap permintaan non-`GET` dicegat dan dijawab
tiruan, sehingga **nol tulis ke database** pada task ini — yang diuji adalah apa yang layar kirim.

### 8.1 Hasil

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| A1 | Daftar parameter termuat dari backend | `PASS` | 15 baris |
| A2, A2', A2'' | `AC-143` menu aksi baris parameter dan golongan: nol `Hapus` | `PASS` | Menu `["Perbarui","Nonaktifkan"]` pada keduanya. Percobaan pertama A2 lolos dengan menu kosong karena pemilih uji salah; diulang dengan pemilih `.dropdown-menu.show .dropdown-item` (menu `RowActionMenu` diportal ke `body`) |
| A3 | `AC-143` detail parameter: aksi hanya `Kembali` dan `Perbarui` | `PASS` | Nol `Hapus`, nol `Aktifkan/Nonaktifkan` di halaman detail |
| A4 | Tambah parameter: kosong ditolak di layar tanpa kirim; isian sah → `POST` | `PASS` | Badan `{parameterCode, parameterName, sortOrder}` (dicegat) |
| A5 | Daftar golongan menampilkan kolom `Jumlah Ruas` | `PASS` | Kepala tabel |
| A6 | `AC-145` detail golongan: bagian *Keberlakuan Ruas Isian* dengan penanda `Wajib` per pasangan | `PASS` | 30 kotak; ringkasan "3 ruas berlaku, 3 di antaranya wajib" |
| A7 | `AC-145` ubah `Wajib` lalu simpan → `PUT …/parameters` membawa penanda per pasangan | `PASS` | Badan `{items:[{labPathologyParameterId, isRequired}]}` (dicegat) |
| A8 | Layar penggolongan termuat dengan data asli | `PASS` | Ringkasan + daftar tersimpan |
| A9 | `AC-144` usulan tanpa golongan: `Konfirmasi` mati; teks "golongkan manual"; nol "terapkan semua" | `PASS` | `nonaktif=true`, tombol massal 0 |
| A10 | `AC-144` golongan dipilih manusia → `Konfirmasi` aktif → `POST` satu baris | `PASS` | Opsi 4 golongan; badan `{procedureId, labPathologyCategoryId}` (dicegat) |
| A11 | `AC-144` kata kunci pencocok tampil apa adanya | `PASS` | — |
| A12 ×3 | 390 px: penggolongan, daftar golongan, detail + keberlakuan tanpa gulir horizontal halaman | `PASS` | `scrollWidth 375 ≤ clientWidth 390` |
| A13 | Nol tulis ke backend | `PASS` | Tiga tulis seluruhnya tiruan |
| B1 | 390 px keberlakuan: kolom `Wajib` dapat dicapai | **`FAIL` → `PASS`** | Lihat 8.2 |
| B2 | 390 px penggolongan: tombol `Konfirmasi` terlihat utuh | **`FAIL` → `PASS`** | Lihat 8.2; sesudah perbaikan tombol di `x=50`, lebar 89 px dalam layar 390 px |
| B3 | 390 px penggolongan: daftar pilihan golongan terbuka dan terlihat | `PASS` | — |

### 8.2 Satu cacat yang ditemukan uji, dan perbaikannya

**Gejala (390 px).** Halaman tidak bergulir menyamping — A12 lolos — tetapi kolom `Wajib` keberlakuan
(`x=609..676`) dan kolom `Aksi` penggolongan (`x=731..796`) **berada di luar layar tanpa wadah gulir**:
tidak dapat dicapai sama sekali. Sebabnya: kedua tabel memakai `.dataTable` (lebar minimum
`max(840px, 100%)`) **tanpa** `.tableWrapper` yang menyediakan `overflow-x: auto`, sehingga lebihnya
dipotong oleh panel.

| Berkas frontend | Perubahan |
| --- | --- |
| `master-data/lab-pathology-categories/detail/lab-pathology-category-parameters-section.jsx` | Tabel keberlakuan dibungkus `baseStyles.tableWrapper` — kolom `Wajib` dicapai lewat gulir tabel |
| `master-data/lab-pathology-categories/mapping/lab-pathology-procedure-mapping-view.jsx` | Tabel tersimpan dibungkus `tableWrapper`; tabel usulan diberi kelas `suggestionTable` dan `data-label` per sel (`Kode`, `Jenis Pemeriksaan`, `Kata Kunci Cocok`, `Golongan`, `Aksi`) |
| `src/style/…/lab-pathology-categories/lab-pathology-mapping.module.css` | `.panel { container-type: inline-size }` + `@container (max-width: 880px)`: baris usulan **ditumpuk** (kepala tabel disembunyikan, tiap sel menampilkan labelnya) — pemilih golongan dan tombol `Konfirmasi` berada dalam layar tanpa gulir |

Penumpukan dipilih untuk tabel usulan — bukan gulir — karena pekerjaannya per baris: memilih golongan
lalu menekan `Konfirmasi`. Menggulir menyamping di antara keduanya pada layar sempit rawan salah baris.

### 8.3 Validasi akhir (seluruh perbaikan sesi 2026-10-02)

| Perintah | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **2303 lulus, 6 gagal** dari 2309 — keenamnya kegagalan baseline yang sama sebelum sesi (Hemodialisa ×4, Bank Darah M0, petty cash). Nol uji Laboratorium gagal |
| `npm run lint:errors` | **0 error** |
| `npm run build` | **Hijau** (server BE/FE dimatikan lebih dulu) |

**Status Git:** perbaikan 8.2 belum ter-commit di frontend. **Nol operasi Git dijalankan.**
