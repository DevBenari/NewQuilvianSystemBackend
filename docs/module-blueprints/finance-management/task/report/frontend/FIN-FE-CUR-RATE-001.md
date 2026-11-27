# Laporan Perubahan Frontend — `FIN-FE-CUR-RATE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIN-FE-CUR-RATE-001` |
| Judul | `/finance/master-data/currency`: kolom Kurs ke IDR, Update Terakhir, dan Aksi (Edit, Nonaktifkan/Aktifkan, Hapus); Desimal dihapus dari tabel, form, dan detail |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap (27 Nov 2026). Tidak ada baris roadmap, jadi **tidak ada tanda status yang dipasang pada roadmap** |
| Trace | `FIN-DEC-018`, `FR-FIN-003`, `FR-FIN-004`, `FIN-BE-CUR-RATE-001` |
| Contract version | Mengonsumsi kontrak aditif `FIN-BE-CUR-RATE-001` (belum dibuild/diterapkan) |
| Wewenang UI | Susunan tabel dan aksi diminta pemilik Finance pada task; komponen mengikuti base component yang sudah ada |
| Dependency | `FIN-BE-CUR-RATE-001` harus diterapkan **lebih dulu**. Terhadap backend lama: kolom Kurs tampil "Belum ada kurs", Update Terakhir tampil "-", dan Hapus ditolak karena `DELETE` belum ada (lihat bagian 8) |
| Task mode | `FRONTEND` (+ laporan ini di repository backend) |
| Target tulis | `QuilvianSystemFrontendDev` (source) |
| Branch / SHA saat dikerjakan | `yasmina` @ `533b21dc6a6f6c630a79071fc75dbf2b825371eb` |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-27 |
| Status | 🟡 **SEBAGIAN** — source selesai, ESLint dan pemeriksa CSS bersih; `npm run build` dan dev server **TIDAK DIJALANKAN** (dilarang), layar **belum pernah dibuka** (`VISUAL CHECK: NOT FEASIBLE`) |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Temuan |
| --- | --- |
| Tabel | Kolom: Kode, Nama, Simbol, **Desimal**, Mata Uang Dasar, Status. Tanpa kurs, tanpa nama pengguna, tanpa aksi per baris |
| Form & detail | Menampilkan "Jumlah Desimal" (isian wajib 0–6) |
| Hook detail | Sudah punya `handleStatusChange`, tetapi view detail tidak memasang tombolnya; status hanya bisa diubah lewat saklar di form Perbarui |
| `supportsDelete` | `false`; tidak ada thunk hapus |
| Setelah kurs dicatat | Hanya baris riwayat yang ditambahkan; detail mata uang tidak dimuat ulang |

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas Finance membuka **Finance → Data Master → Mata Uang & Kurs**.
2. Tabel menampilkan `1 USD = Rp15.825,00` beserta "Berlaku 25 Nov 2026 · Kurs tengah"; IDR menampilkan `1 IDR = Rp1`; mata uang tanpa kurs berlaku menampilkan "Belum ada kurs".
3. Kolom **Update Terakhir** menampilkan dua baris: nama pengguna dan tanggal-jam (WIB).
4. Kolom **Aksi** (menu tiga titik) menampilkan hanya aksi yang diizinkan Akses Role:

| Aksi | Syarat tampil | Perilaku |
| --- | --- | --- |
| Edit | `Currency : Update` | Membuka halaman Perbarui yang sudah ada |
| Nonaktifkan / Aktifkan | `Currency : Update` | Dialog konfirmasi → `PATCH /status` → tabel dan ringkasan dimuat ulang |
| Hapus | `Currency : Delete` | Dialog konfirmasi → `DELETE`; bila ditolak backend, alasannya tampil **di dalam dialog** |

5. Tombol dinonaktifkan **beserta alasannya** bila: mata uang dasar/IDR (Nonaktifkan dan Hapus), atau sudah punya riwayat kurs (Hapus). Ini petunjuk tampilan; backend tetap penegak akhir.
6. Pencatatan kurs tetap di halaman detail (form "Catat Kurs"); setelah tersimpan, detail dan riwayat dimuat ulang.

**Jalur tidak normal**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Backend menolak Hapus (dipakai/punya kurs) | Dialog tetap terbuka dengan pesan backend, mis. "Mata uang USD tidak dapat dihapus karena sudah memiliki 12 riwayat kurs. Nonaktifkan saja bila tidak dipakai lagi." |
| Tanpa hak Update maupun Delete | Kolom Aksi tidak ditampilkan |
| Pengguna pelaku tidak ditemukan | "Pengguna tidak tersedia" dengan waktu tetap tampil |
| Kurs kosong atau 0 | "Belum ada kurs" (tidak pernah Rp0) |
| Jaringan gagal | Pesan standar Bahasa Indonesia, bukan teks mentah Axios |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah (diverifikasi `git status`)

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/master-data/currency/currency-constants.jsx` | Hapus field & kolom & baris detail Desimal. Kolom baru `rateToIdr`, `lastUpdated`, `actions`. `CURRENCY_PERMISSION`, `CURRENCY_TEXTS`, `CURRENCY_RATE_REFERENCE_CODE`. `supportsDelete: true` |
| `src/utils/finance/master-data/currency/currency-utils.jsx` | `describeCurrentRate`, `describeLastUpdated`, `getCurrencyDeactivateBlockReason`, `getCurrencyDeleteBlockReason`; baris detail format `rateToIdr`/`lastUpdated`; `formatDateTime` dalam WIB; `formatDateOnly` tidak bergeser sehari untuk tanggal tanpa jam |
| `src/lib/state/slice/finance/master-data/master-data-currency-slice.jsx` | Thunk `deleteCurrency`; status menggabungkan respons penuh; `getCurrencyDetail` mode `silent`; pesan mentah Axios tidak lagi ditampilkan |
| `src/lib/hooks/finance/master-data/currency/use-master-data-currency.jsx` | Izin `Currency : Update/Delete`, aksi Edit, konfirmasi status, konfirmasi hapus, toast, muat ulang daftar + ringkasan |
| `src/lib/hooks/finance/master-data/currency/use-master-data-currency-detail.jsx` | Setelah kurs dicatat: muat ulang detail (senyap) dan riwayat kurs |
| `src/components/view/finance/master-data/currency/master-data-currency-view.jsx` | Kolom baru, `RowActionMenu`, dua `ConfirmModal`, `ToastStack`, `InformationAlert` |
| `tests/unit/finance-currency-rate-column.test.mjs` | **Baru** — 16 tes (fungsi murni dan kontrak yang dibaca dari source BE) |

Tidak diubah: `currency-detail-view.jsx`, `currency-form-view.jsx`, `use-master-data-currency-editor.jsx`, route `src/app/finance/master-data/currency/**`. Form dan detail otomatis tidak lagi menampilkan Desimal karena dirender dari konfigurasi.

### 3.2 Kepatuhan arsitektur

Alur `app → view → hook → slice → InstanceAxios` dipertahankan; view tidak memanggil Axios; tidak ada CSS Module baru (tata letak memakai utilitas Bootstrap yang terbukti ada dan `administrator-region-table.module.css`); standar master data (`<FEATURE>_CONFIG`, sembilan thunk + satu thunk hapus) diikuti.

### 3.3 Gerbang tampilan

**Modul referensi visual:** `src/components/view/payroll/policy/payroll-policy-list-view.jsx` (aksi baris + konfirmasi) dan `src/components/view/corporate/asset-management/movement/asset-loan-table-columns.jsx` (`RowActionMenu`).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header halaman | `Hero` | `REUSE` | Tidak berubah |
| Ringkasan | `SummaryGrid` | `REUSE` | Tidak berubah |
| Filter & pencarian | `DataFilter`, `FilterSelect`, `FilterDatePicker` | `REUSE` | Tidak berubah |
| Tabel & paginasi | `DataTable`, `Pagination` | `REUSE` | Tidak berubah |
| Sel dua baris (kurs, update terakhir) | `tableStyles.primaryCell/supportingCell` + utilitas Bootstrap `d-flex flex-column` | `COMPOSE` | Dirangkai di view; tanpa komponen baru |
| Menu aksi baris | `RowActionMenu` | `REUSE` | Mendukung `disabled` + alasan tampil; lebar tabel tidak membengkak |
| Konfirmasi | `ConfirmModal` | `REUSE` | Pola `payroll-policy-list-view` |
| Pesan galat dalam dialog | `InformationAlert` | `REUSE` | — |
| Notifikasi hasil | `ToastStack` | `REUSE` | — |
| Badge status | `StatusBadge` | `REUSE` | Tidak berubah |

`UI GATE: PASS — 10 elemen REUSE 9, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0`

> **Alternatif yang dipertimbangkan untuk menu aksi.** (A) `RowActionMenu` — **dipilih**: alasan tombol nonaktif terbaca, tabel tetap ramping. (B) Tiga `BaseButton` sejajar seperti tabel kebijakan Payroll: aksi langsung terlihat tetapi menambah lebar kolom dan alasan nonaktif hanya lewat tooltip.

Keluaran pemeriksa, apa adanya:

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs <view daftar> <view detail> <view form>
OK — 3 berkas diperiksa, 0 temuan.
$ grep -nE "<button|className=\"btn|<table|fw-|fs-|!important|fixed inset-0|<(input|select|textarea)|UUID|GUID" master-data-currency-view.jsx
(kosong)
$ grep -nE "<pola Tailwind bagian G.7>" master-data-currency-view.jsx
(kosong)
```

Satu temuan pertama dari pemeriksa (`"missing"` terbaca sebagai nama class) sudah diperbaiki sebelum hasil di atas.

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/finance/master-data/currency` | — | `VISUAL CHECK: NOT FEASIBLE` — dev server dan build dilarang pada task ini |
| `/finance/master-data/currency/{id}` dan `/update` | — | `VISUAL CHECK: NOT FEASIBLE` — alasan sama |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel `DataTable` ("Mengambil data mata uang...") |
| Kosong | "Data mata uang tidak ditemukan." |
| Gagal | Kotak galat di atas tabel; galat aksi tampil di dalam dialog |
| Tanpa hak akses daftar | `AccessDeniedGate` (tidak berubah) |
| Tanpa hak aksi | Aksi disembunyikan; kolom Aksi hilang bila tidak ada hak Update maupun Delete |
| Aksi berjalan | Tombol dialog berputar dan terkunci (`ConfirmModal loading`) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Master Data / Currency

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/master-data/currencies` | Daftar + Kurs ke IDR + Update Terakhir | `Currency : Read` |
| `PATCH` | `…/{id}/status` | Nonaktifkan / Aktifkan | `Currency : Update` |
| `DELETE` | `…/{id}` | Hapus | `Currency : Delete` |
| `GET` | `…/{id}` | Detail (dimuat ulang senyap setelah kurs dicatat) | `Currency : Read` |
| `GET`/`POST` | `…/{id}/exchange-rates` | Riwayat dan pencatatan kurs | `Currency : Read` / `Update` |

Alamat dicocokkan dengan `[Route]` controller backend oleh tes (bukan dengan service FE).

---

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` pada 6 berkas source + tes | 0 error; 1 peringatan `set-state-in-effect` pada `use-master-data-currency-detail.jsx:107` — baris itu **sudah ada sebelum perubahan** dan tidak saya ubah | `PASS` / `EXISTING` |
| `check-css-usage.mjs` (3 view) | `OK — 3 berkas diperiksa, 0 temuan.` | `PASS` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/finance-currency-rate-column.test.mjs` | 16 tes, 16 lulus, 0 gagal | `PASS` |
| `npm run build`, dev server | **Tidak dijalankan** — dilarang | `NOT RUN` |
| Buka layar, klik filter/aksi/dialog, responsif, screenshot | Tidak dilakukan | `NOT RUN` / `MANUAL TEST: NOT FEASIBLE` |

`AUTOMATED TEST: node --test tests/unit/finance-currency-rate-column.test.mjs — PASS (16/16)`

---

## 7. Acceptance criteria

Matriks AC-01…AC-35 ada pada laporan akhir task. Ringkas: semua butir tampilan dan kontrak terbukti statis; butir yang butuh data/browser/build (AC-26 ukuran query nyata, AC-31 responsif, AC-32 kesejajaran visual) berstatus `NOT RUN`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dependency backend | Tanpa `FIN-BE-CUR-RATE-001` terapan: kolom Kurs "Belum ada kurs" untuk semua mata uang asing, Update Terakhir "-", dan aksi Hapus gagal dengan pesan generik |
| Perilaku lama yang dipertahankan | `DataTable` memakai bawaan `sortLatestFirst`, sehingga baris tiap halaman diurutkan ulang menurut tanggal dibuat (terbaru dulu) dan mengabaikan urutan nama dari backend. Itu perilaku lama modul ini; tidak diubah karena bukan bagian permintaan |
| Filter periode/tanggal | Tetap tampil tetapi tidak dikirim ke backend (`unsupportedFilterKeys`), sesuai standar master data |
| Hapus pada halaman detail | Tidak ditambahkan — permintaan hanya pada tabel daftar |
| Input kurs | Form "Catat Kurs" mengonversi teks dengan `Number()` dan mengganti koma menjadi titik; teks berformat ribuan seperti `15.825` ditafsirkan sebagai 15,825. Perilaku lama, dilaporkan sebagai risiko input |
| Perubahan sampingan | `NONE` |
| Status Git | 6 berkas source `M` + 1 tes `??` pada repository frontend; tidak ada stage/commit/push |
| Langkah berikutnya | Terapkan backend, `npm run build`, buka layar dengan data (USD ber-kurs, kurs masa depan, tanpa kurs, mata uang dasar), simpan screenshot |
