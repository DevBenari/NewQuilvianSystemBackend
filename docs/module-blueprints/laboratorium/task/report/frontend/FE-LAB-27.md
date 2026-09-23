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
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — ketiga AC terbangun, 10 uji unit baru lulus, lint 0 error, build hijau, **kesembilan route terkompilasi**. **Belum diklik di peramban** |

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
| `AC-144` layar pemetaan menyediakan usulan dari `GET /suggestions` yang **wajib dikonfirmasi manusia** | **Terbangun; aturannya terbukti** | **Nol tombol "terapkan semua"**; tiap baris punya tombol `Konfirmasi` sendiri yang **mati** selama golongan belum dipilih; kata kunci pencocok ditampilkan; 4 uji |
| `AC-145` layar keberlakuan menampilkan penanda **wajib** per pasangan parameter-kategori | **Terbangun; aturannya terbukti** | Kolom `Wajib` per baris, tersimpan per pasangan; 6 uji |
| DoD — kedua layar berjalan | **Terbangun** | 9 route terkompilasi |
| DoD — `AC-143`..`AC-145` terbukti | ⚠ **Dua dari tiga baru terbukti pada lapis aturan** | Belum diklik di peramban |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan lint baru |
| Masalah yang diketahui | **Ketiga layar belum diklik di peramban.** Dua bagian non-standar — keberlakuan dan penggolongan — memakai markup tabel polos tanpa CSS Module khusus; tata letaknya pada layar sempit **belum dilihat** dan kemungkinan perlu penyesuaian |
| Risiko tersisa | **Sedang, dan terpusat pada `AC-144`.** Godaan menambahkan "terapkan semua" akan muncul begitu kepala instalasi menghadapi puluhan baris — dan itu menghapus satu-satunya pemeriksaan manusia yang menjaga penggolongan. Ketiadaannya **disengaja** dan dicatat pada komentar berkasnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 37 berkas tersentuh, seluruhnya dalam cakupan. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** Klik ketiga layar terhadap backend berisi data. **2.** Isi penggolongan bagi 6 pemeriksaan yang tersisa — itu **menutup penahan `FE-LAB-28`**. **3.** Tinjau tata letak kedua bagian non-standar pada layar sempit |
