# Laporan Perubahan Frontend — `FE-BKC-FIX-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-002` (ad-hoc bug fix — bukan task roadmap bernomor `FE-BKC-0xx`, dibuat sendiri untuk menjaga jejak laporan tetap tracked) |
| Judul | Dropdown "Tarif Layanan" (form Buat Invoice Manual) tidak menampilkan hasil sebelum diketik pencarian |
| Slice | Ditemukan lewat laporan bug pengguna (screenshot) atas hasil `FE-BKC-014` — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap untuk perbaikan ini |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan kontrak API, murni konfigurasi field di sisi klien |
| Wewenang UI | Override properti field yang sudah ada (`requireSearch`) pada satu field form — bukan base component baru/extend, tidak melalui `base-component-decision-gate` |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — 1 berkas diubah, 1 baris logika (nilai literal `false` pada satu properti konfigurasi field), tidak ada perubahan kontrak API/database/keamanan |
| Task mode | `FRONTEND` — bug fix ad-hoc, otorisasi eksplisit pengguna (laporan bug langsung dengan screenshot dan deskripsi perilaku yang diharapkan) |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/health-services/billing-management/billing-invoices/create-manual/create-manual-invoice-view.jsx` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit) |
| Commit backend yang dijadikan rujukan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source selesai, lint bersih, **terverifikasi hidup** (login sungguhan, request network diperiksa langsung) |

---

## 1. Keadaan yang ditemukan di awal

Pengguna melaporkan (dengan screenshot) bahwa pada form "Buat Invoice Manual", setelah memilih
kunjungan dan Kategori Tarif ("Registration"), dropdown "Tarif Layanan" tetap kosong sampai
pengguna mengetik kata kunci pencarian — padahal kategori sudah dipilih. Permintaan eksplisit
pengguna: hasil GET harus tampil langsung sesuai kategori terpilih tanpa perlu mengetik dulu,
sambil tetap bisa dicari (search) dan digulir (scroll/load-more).

Ditelusuri: entry resource `tariffs` pada `health-service-select-resources.js` (dari sesi
sebelumnya, `FE-BKC-014`) diset `requireSearch: true, minSearchLength: 2` — pengaturan ini
sengaja dipakai untuk listing tarif UMUM (ratusan ribu baris) supaya tidak menembak query tanpa
filter. `base-editor-field.jsx` sudah punya jalur override per-field: `field.requireSearch`
diteruskan langsung ke `useSelectResource(...)` dan MENANG atas `resourceConfig.requireSearch`
(lihat `use-select-resource.jsx` baris ~503: `Boolean(optionRequireSearch ?? resourceConfig?.requireSearch ?? false)`).
Field `tariffId` pada form ini SELALU mengirim filter kategori+konteks kunjungan
(`tariffCategoryId`/`serviceUnitId`/`clinicId`/`patientClassId`, lihat `BKC-DEC-061`), jadi
listing awal di sini sudah tersaring sempit dan aman untuk ditampilkan tanpa perlu syarat
pencarian dulu.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir/staf yang membuat invoice manual lewat "Buat Invoice Manual".

**Pemicu**: kunjungan dan Kategori Tarif (opsional) sudah dipilih, kasir membuka dropdown
"Tarif Layanan".

**Langkah (sesudah perbaikan)**:

1. Kasir memilih kunjungan; filter konteks (unit layanan/klinik/kelas pasien) otomatis terpasang.
2. Kasir opsional memilih Kategori Tarif untuk mempersempit daftar.
3. Kasir membuka dropdown "Tarif Layanan" — daftar tarif yang cocok dengan filter di atas
   **langsung tampil** tanpa perlu mengetik apa pun.
4. Kasir tetap bisa mengetik kata kunci untuk mempersempit lebih lanjut (request baru terkirim
   ke server dengan parameter `search`), dan bisa menggulir daftar ke bawah untuk memuat halaman
   berikutnya (mekanisme `hasMoreOptions`/`loadMore` bawaan `FilterSelect`, tidak diubah).

**Aturan yang berlaku**: hanya field `tariffId` pada form ini yang di-override; entry resource
`tariffs` di registry bersama TIDAK diubah, sehingga konsumen lain resource yang sama (bila ada)
tetap memakai default `requireSearch: true`.

**Jalur tidak normal**: bila kombinasi kategori+konteks kunjungan memang tidak punya tarif yang
cocok, server mengembalikan `items: []` (`totalData: 0`) dan `FilterSelect` menampilkan state
kosong bawaan — ini perilaku data, bukan bug pada perbaikan ini (diverifikasi terjadi pada salah
satu kombinasi uji, lihat § 6).

**Hasil akhir**: kasir tidak lagi harus mengetik dulu untuk melihat tarif yang relevan dengan
kategori/kunjungan yang sudah dipilih.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`create-manual-invoice-view.jsx` (field config `tariffId`); `health-service-select-resources.js`
(`tariffs` entry — dibaca saja, TIDAK diubah); `use-select-resource.jsx` (logika precedence
`requireSearch`/`minSearchLength`/`disableInitialLoad` — dibaca saja); `base-editor-field.jsx`
(jalur penerusan `field.requireSearch` ke hook — dibaca saja); `filter-select.jsx` (perilaku
search-input dan scroll/`loadMore` bawaan — dibaca saja).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `create-manual-invoice-view.jsx` | Field config `tariffId` ditambah `requireSearch: false` beserta komentar penjelasan alasan override |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Perilaku "tampil tanpa search dulu" pada field `tariffId` | `REUSE` (override properti field yang sudah didukung) | `base-editor-field.jsx` sudah menyediakan jalur override `field.requireSearch` per-field yang menang atas default resource — tidak ada base component baru, tidak ada perubahan default `FilterSelect`/`useSelectResource`/resource registry bersama |

**`UI GATE`**: tidak ada — murni pengaturan nilai properti field yang sudah ada di kontrak
`base-editor-field`, tidak ada keputusan pengguna yang perlu ditunggu.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Skeleton/loading bawaan `FilterSelect` (`loadingText`) — tidak diubah |
| Kosong (kombinasi filter tanpa tarif cocok) | Teks kosong bawaan `FilterSelect` (`emptyText`) — tidak diubah, diverifikasi muncul pada § 6 |
| Gagal | Tidak diubah — jalur error `useSelectResource` yang sudah ada tetap berlaku |
| Tanpa hak akses | `NOT APPLICABLE` — tidak ada perubahan endpoint/permission |

---

## 5. Endpoint yang dikonsumsi

`GET /health-services/master-data/tariffs/options` — endpoint yang SAMA persis dengan
`FE-BKC-014`, tidak ada perubahan kontrak. Perbaikan ini hanya mengubah KAPAN request pertama
kali ditembak dari sisi klien (sekarang: begitu dropdown dibuka, bukan menunggu ≥2 karakter
diketik).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint create-manual-invoice-view.jsx` | Berhasil tanpa error | `PASS` | Keluaran perintah kosong |
| Login live, buka "Buat Invoice Manual", pilih kunjungan, buka dropdown Tarif Layanan TANPA mengetik apa pun | Request `GET .../tariffs/options?...&pageNumber=1&pageSize=25` (tanpa parameter `search`) terkirim otomatis, status 200 | `PASS` | URL dan status request ditangkap langsung dari `page.on("response")` |
| Kombinasi kunjungan + kategori "Registration" tertentu | Request sukses (200) tapi `totalData: 0, items: []` — dropdown tampil kosong sesuai state bawaan | `PASS` (perilaku data, bukan bug fix ini) | Body JSON response ditangkap dan dicatat |
| Kombinasi kunjungan + TANPA kategori (hanya filter konteks kunjungan) | Request sukses, `totalData: 18515`, 25 baris pertama tampil langsung di listbox tanpa mengetik apa pun | `PASS` | Body JSON + isi `listbox` (`innerText` 5 baris pertama) diperiksa langsung |
| Menggulir daftar (scroll) | `optionList` punya `overflow-y: auto`, `scrollHeight` (1082px) > `clientHeight` (238px) — bisa digulir, tombol "muat lebih banyak" (`hasMoreOptions`) tersedia untuk paginasi | `PASS` | `getComputedStyle`/`scrollHeight`/`clientHeight` diperiksa langsung |
| Mengetik kata kunci pencarian ("konsul") setelah dropdown terbuka | Request baru terkirim dengan parameter `search=konsul` (debounce bekerja, filter konteks kunjungan tetap terbawa) | `PASS` | URL request baru ditangkap dari `page.on("request")` |
| Console error selama pengujian | Tidak ada error baru yang berkaitan dengan perubahan ini | `PASS` | Console dipantau selama sesi Playwright |

Uji manual: `PASS`.

**Tidak dijalankan:** `npm run build`/`next build` penuh; component test (instruksi eksplisit
pengguna sepanjang sesi ini — tanpa file test).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Dropdown Tarif Layanan menampilkan hasil GET langsung sesuai kategori/konteks terpilih, tanpa perlu mengetik pencarian dulu | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| Tetap bisa dicari (search) | Terpenuhi, terverifikasi hidup — tidak diubah, request `search=` tetap terkirim | Lihat § 6 |
| Tetap bisa digulir (scroll/paginasi) | Terpenuhi, terverifikasi hidup — tidak diubah | Lihat § 6 |
| Resource `tariffs` bersama di registry tidak berubah (konsumen lain tidak terdampak) | Terpenuhi | Lihat § 3.2 — hanya 1 berkas field-level yang diubah |
| lint lulus | Terpenuhi | Lihat § 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan sampingan (TIDAK diperbaiki, di luar wewenang task ini) | Selama verifikasi ditemukan bahwa `FilterSelect` selalu merender satu baris `role="option"` tambahan di POSISI PERTAMA daftar, berisi teks placeholder field (mis. "Cari nama atau kode tarif...") dengan `aria-selected="true"`, meskipun belum ada tarif yang benar-benar dipilih. Ini adalah perilaku `FilterSelect`/`base-editor-field` yang sudah ada SEBELUM perbaikan ini (independen dari `requireSearch`) dan kemungkinan berlaku di SEMUA field select berbasis komponen ini di seluruh aplikasi, bukan spesifik field `tariffId`. Sebelumnya tidak terlihat pada field ini karena dropdown memang tidak pernah menampilkan isi apa pun tanpa mengetik dulu. Tidak termasuk dalam permintaan pengguna saat ini (yang fokus pada "tampil langsung tanpa search"), dan mengubah perilaku ini akan menyentuh komponen bersama (`FilterSelect`) sehingga butuh `base-component-decision-gate` serta keputusan pengguna terpisah bila memang dianggap perlu diperbaiki |
| Masalah yang diketahui | Kombinasi kategori+kunjungan tertentu bisa saja punya nol tarif (state kosong bawaan) — ini perilaku data as-is, bukan regresi dari perbaikan ini |
| Dependency backend | `NONE` — tidak ada perubahan endpoint/kontrak |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Interupsi | `NONE` |
| Status Git | Modified (task ini): `create-manual-invoice-view.jsx`. Berkas lain pada working tree yang sama milik `FE-BKC-014`/`015`/`016`/`FE-BKC-FIX-001` (lihat laporan masing-masing). Belum staged/commit |
| Langkah berikutnya | `SELESAI` — permintaan pengguna sudah terpenuhi. Temuan sampingan (baris pseudo-option pertama) ditindaklanjuti dan ditutup lewat task terpisah `FE-BKC-FIX-003` (`base-component-decision-gate` dilalui, root cause ternyata `BaseSelectField`/`addClearOption`, bukan `FilterSelect` itu sendiri) — lihat `task/report/frontend/FE-BKC-FIX-003.md` |
