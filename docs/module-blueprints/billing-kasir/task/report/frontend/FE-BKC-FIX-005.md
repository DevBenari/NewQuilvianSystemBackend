# Laporan Perubahan Frontend — `FE-BKC-FIX-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-005` (ad-hoc bug fix — bukan task roadmap bernomor `FE-BKC-0xx`, dibuat sendiri untuk menjaga jejak laporan tetap tracked) |
| Judul | Kata kunci pencarian dan hasil tarif lama tetap menempel di dropdown "Tarif Layanan" setelah Kategori Tarif diganti |
| Slice | Laporan bug pengguna terpisah (screenshot) atas hasil `FE-BKC-FIX-004` — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap untuk perbaikan ini |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan kontrak API, murni perilaku klien |
| Wewenang UI | **EXTEND base component bersama** (`BaseEditorForm`, dipakai di seluruh form editor aplikasi) — properti opsional baru `field.remountKey`, opt-in murni, tidak mengubah default untuk consumer yang tidak memakainya. Melalui `base-component-decision-gate`, disetujui eksplisit pengguna lewat `AskUserQuestion` (opsi "Reset penuh via remount (Rekomendasi)") sebelum implementasi dimulai |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — 2 berkas diubah, perubahan kecil (satu properti key opsional di `BaseEditorForm`, satu field config di halaman) |
| Task mode | `FRONTEND` — bug fix ad-hoc, otorisasi eksplisit pengguna + keputusan gate base component |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/features/base-features/base-editor-form.jsx`, `src/components/view/health-services/billing-management/billing-invoices/create-manual/create-manual-invoice-view.jsx` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit) |
| Commit backend yang dijadikan rujukan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source selesai, lint bersih, **terverifikasi hidup** (login sungguhan): kata kunci pencarian dan hasil lama terbukti benar-benar bersih setelah Kategori Tarif diganti |

---

## 1. Keadaan yang ditemukan di awal

Pengguna melaporkan (dengan screenshot): setelah mengetik pencarian pada dropdown "Tarif Layanan"
lalu MENGGANTI "Kategori Tarif", hasil pencarian lama masih terbawa — screenshot menunjukkan kotak
pencarian dan satu baris hasil tarif dari kategori sebelumnya masih tampil padahal kategori sudah
berbeda.

Ditelusuri: `handleChange` (`use-create-manual-invoice.js`) HANYA mengosongkan NILAI yang terpilih
(`tariffId`, `unitPrice`) saat kategori berganti — TIDAK menyentuh kata kunci pencarian yang sudah
diketik pengguna. Kata kunci itu tersimpan sebagai state INTERNAL di dua tempat berbeda yang tidak
bisa direset dari luar lewat prop biasa:

1. `searchKeyword` — state lokal di dalam `FilterSelect` (teks yang tampil di kotak pencarian).
2. `internalSearch` — state di dalam hook `useSelectResource` yang dipanggil oleh
   `BaseEditorField` (menentukan parameter `search=` yang dikirim ke server).

Keduanya melekat pada INSTANCE KOMPONEN yang sama selama field `tariffId` tidak pernah
di-unmount/mount-ulang — dan karena `BaseEditorForm` (`base-editor-form.jsx`) memberi key tetap
`field.name` ke setiap `<BaseEditorField>`, field yang sama selalu memakai instance (dan state
internal) yang sama, berapa pun kali kategori diganti.

Karena state yang perlu direset ada di DUA lapisan berbeda dan tidak ada API eksternal untuk
mengosongkannya satu per satu, opsi yang dipilih (lewat `base-component-decision-gate`) adalah
remount total: field `tariffId` di-mount ulang dari nol setiap kali `categoryId` berubah, sehingga
kedua state internal itu ikut ter-reset bersih sekaligus.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir/staf yang membuat invoice manual lewat "Buat Invoice Manual".

**Pemicu**: sudah mengetik kata kunci di kotak pencarian dropdown "Tarif Layanan", lalu mengganti
"Kategori Tarif" ke kategori lain.

**Langkah (sesudah perbaikan)**:

1. Kasir memilih Kategori Tarif A, membuka dropdown Tarif Layanan, mengetik kata kunci pencarian
   (mis. "A1 GA"), melihat hasil yang tersaring.
2. Kasir mengganti Kategori Tarif ke B.
3. Kasir membuka lagi dropdown Tarif Layanan — kotak pencarian sudah KOSONG (bukan kata kunci
   lama), dan daftar yang tampil adalah tarif kategori B TANPA tersaring kata kunci lama sama
   sekali (bukan sekadar "terlihat benar" tapi diam-diam masih tersaring — datanya benar-benar
   berbeda/lengkap).

**Aturan yang berlaku**: `field.remountKey` opsional pada `BaseEditorForm` — bila diisi, key React
untuk field itu menjadi `"${field.name}:${field.remountKey}"`; berubahnya nilai `remountKey`
memaksa React meng-unmount lalu mount ulang `BaseEditorField` (dan semua state internal di
dalamnya) dari nol. Field `tariffId` memasang `remountKey: form.categoryId`.

**Jalur tidak normal**: `NOT APPLICABLE`.

**Hasil akhir**: kata kunci pencarian dan hasil tarif tidak lagi "bocor" lintas kategori.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`base-editor-form.jsx` (loop render field, key assignment); `filter-select.jsx`
(`searchKeyword` — state lokal, dikonfirmasi TIDAK punya API eksternal untuk direset, dibaca saja,
TIDAK diubah); `use-select-resource.jsx` (`internalSearch`/`setSearch`/`reset` — dikonfirmasi
`reset()` sudah ada tapi tidak pernah dipanggil otomatis saat filter berubah, dan tidak ada channel
bagi consumer field untuk memanggilnya dari luar `BaseEditorField`; dibaca saja, TIDAK diubah);
`base-editor-field.jsx` (pemanggil `useSelectResource` — dibaca saja, TIDAK diubah);
`create-manual-invoice-view.jsx` (field config `tariffId`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `base-editor-form.jsx` | Key untuk `<BaseEditorField>` per field diubah dari selalu `field.name` menjadi `field.remountKey !== undefined ? \`${field.name}:${field.remountKey}\` : field.name` — default identik untuk field yang tidak menyetel `remountKey` |
| `create-manual-invoice-view.jsx` | Field `tariffId` ditambah `remountKey: form.categoryId` |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Key remount per field pada `BaseEditorForm` | `EXTEND` (properti baru, opt-in) | Menambah kapabilitas baru (`field.remountKey`) pada base component bersama yang dipakai di seluruh form editor aplikasi — wajib melalui `base-component-decision-gate` walau perilaku default field lain tidak berubah sama sekali |

**`UI GATE`**: **DILALUI** — disajikan 2 pilihan bernomor ke pengguna lewat `AskUserQuestion`
(reset penuh via remount [direkomendasikan] / reset data saja tanpa remount, kotak pencarian tetap
terisi). Pengguna memilih opsi yang direkomendasikan sebelum satu baris kode pun diubah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Kategori diganti, dropdown belum pernah dibuka lagi | `NOT APPLICABLE` — tidak ada yang terlihat berubah sampai dropdown dibuka |
| Kategori diganti, dropdown dibuka kembali | Kotak pencarian kosong; daftar opsi hasil fetch baru untuk kategori aktif, tanpa filter kata kunci lama |

---

## 5. Endpoint yang dikonsumsi

`GET .../tariffs/options` — endpoint yang sama, tidak ada perubahan kontrak. Perbaikan ini hanya
memastikan parameter `search=` yang dikirim benar-benar kosong lagi setelah kategori berganti
(sebelumnya berpotensi masih membawa kata kunci lama pada permintaan berikutnya bila pengguna
sempat mengetik sebelum mengganti kategori).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint base-editor-form.jsx create-manual-invoice-view.jsx` | Berhasil tanpa error | `PASS` | Keluaran perintah kosong |
| Kategori "Laboratory", ketik pencarian "A1 GA" | 2 hasil tersaring benar ("A1 GAMBARAN DARAH TEPI...") | `PASS` | Jumlah dan isi opsi diperiksa langsung |
| Ganti kategori ke "Radiology" (kategori BERBEDA, sama-sama punya data untuk kunjungan uji), buka lagi dropdown Tarif Layanan | Kotak pencarian KOSONG (bukan "A1 GA" lagi); 25 hasil tampil, daftar dimulai dari "ABDOMEN 2 POSISI" dst. — daftar PENUH kategori Radiology, TIDAK lagi tersaring kata kunci "A1 GA" dari kategori sebelumnya | `PASS` | `inputValue()` kotak pencarian dan isi 3 opsi pertama diperiksa langsung |
| Regresi: field lain yang tidak memasang `remountKey` (mis. `encounterId`, `categoryId` sendiri) | Tidak diverifikasi ulang secara eksplisit pada task ini, TAPI perubahan `base-editor-form.jsx` murni kondisional (`field.remountKey !== undefined`) — untuk field yang tidak menyetelnya, ekspresi key identik dengan sebelumnya (`field.name`), sehingga tidak ada perubahan perilaku yang mungkin terjadi | `PASS` (by construction) | Lihat § 3.2 — satu baris kondisional, cabang default tidak berubah |
| Console error selama pengujian | Hanya noise pra-eksisting tidak terkait (CSP foto profil, dsb.) — tidak ada error baru dari perubahan ini | `PASS` | Console dipantau selama sesi Playwright pada pengujian-pengujian sebelumnya di form yang sama pada task ini |

Uji manual: `PASS`.

**Tidak dijalankan:** `npm run build`/`next build` penuh; regresi otomatis pada seluruh consumer
`BaseEditorForm` lain (dijamin aman by construction, lihat baris regresi di atas, bukan lewat
pengujian langsung tiap consumer); component test (instruksi eksplisit pengguna sepanjang sesi ini
— tanpa file test).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kotak pencarian Tarif Layanan kosong lagi setelah Kategori Tarif diganti | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| Hasil dropdown setelah kategori diganti TIDAK lagi tersaring kata kunci lama | Terpenuhi, terverifikasi hidup dengan dua kategori berbeda yang sama-sama punya data | Lihat § 6 |
| Perubahan default base component melalui `base-component-decision-gate` dengan persetujuan eksplisit pengguna | Terpenuhi | Lihat § 3.3 |
| Field lain yang tidak memasang `remountKey` tidak berubah perilakunya | Terpenuhi by construction | Lihat § 6 |
| lint lulus | Terpenuhi | Lihat § 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Trade-off yang disadari | Remount mereset SELURUH state lokal field itu (termasuk posisi scroll dropdown, fokus, dsb.) — dinilai wajar karena pengguna memang sedang mengganti konteks (kategori) secara sadar, field yang di-remount pun sebelumnya sempat `disabled` (lihat `FE-BKC-FIX-004`) sehingga tidak ada state penting yang "hilang tanpa sadar" |
| Masalah yang diketahui | `NONE` baru |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Interupsi | `NONE` |
| Status Git | Modified (task ini): `base-editor-form.jsx`, `create-manual-invoice-view.jsx`. Berkas lain pada working tree yang sama milik task-task sebelumnya (lihat laporan masing-masing). Belum staged/commit |
| Langkah berikutnya | `SELESAI` — permintaan pengguna terpenuhi dan terverifikasi hidup dengan skenario dua kategori berbeda yang sama-sama punya data (bukan hanya kategori kosong) |
