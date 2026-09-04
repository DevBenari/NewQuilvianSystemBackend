# Laporan Perubahan Frontend — `FE-BKC-FIX-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-003` (ad-hoc bug fix — bukan task roadmap bernomor `FE-BKC-0xx`, dibuat sendiri untuk menjaga jejak laporan tetap tracked) |
| Judul | Baris opsi "clear ke kosong" pada `BaseSelectField`/`FilterSelect` tampil tanpa syarat, termasuk saat belum ada pilihan nyata atau pada field wajib |
| Slice | Tindak lanjut temuan sampingan yang dilaporkan di `task/report/frontend/FE-BKC-FIX-002.md` § 8 — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap untuk perbaikan ini |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan kontrak API, murni perilaku klien |
| Wewenang UI | **EXTEND perilaku default base component bersama** (`BaseSelectField`/`addClearOption`, dipakai ~481 field `type: "select"` di 111 berkas seluruh aplikasi) — melalui `base-component-decision-gate`, disetujui eksplisit pengguna lewat `AskUserQuestion` (opsi "Sembunyikan bila required ATAU belum ada nilai (Rekomendasi)") sebelum implementasi dimulai |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT`-`MEDIUM` — 1 berkas diubah, perubahan logika kecil (satu flag kondisional), TAPI blast radius besar (base component bersama dipakai luas) sehingga verifikasi regresi mencakup lebih dari satu consumer |
| Task mode | `FRONTEND` — bug fix ad-hoc, otorisasi eksplisit pengguna ("boleh lakukan perbaikkan" + keputusan gate base component) |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/features/base-features/base-form-control.jsx` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit) |
| Commit backend yang dijadikan rujukan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source selesai, lint bersih, **terverifikasi hidup** (login sungguhan) pada field opsional (`categoryId`) dan field wajib (`tariffId`), baik sebelum maupun sesudah nilai dipilih |

---

## 1. Keadaan yang ditemukan di awal

Saat verifikasi `FE-BKC-FIX-002`, ditemukan bahwa `FilterSelect` selalu merender satu baris
`role="option"` tambahan di POSISI PERTAMA setiap listbox, berlabel teks placeholder field (mis.
"Cari nama atau kode tarif...", "Semua kategori"), dengan `aria-selected="true"` saat belum ada
seleksi nyata. Root cause ditelusuri sampai ke `BaseSelectField`
(`base-form-control.jsx`): fungsi `addClearOption` (`base-ui-utils.jsx`) SENGAJA menambahkan baris
"kosongkan pilihan" berisi `{value: "", label: placeholder}` di posisi pertama — TAPI dipanggil
**tanpa syarat**, untuk SEMUA field select (~481 pemakaian `type: "select"` di 111 berkas), baik
field itu wajib (`required`) maupun belum ada nilai yang benar-benar dipilih. Akibatnya baris
"clear" ini selalu ada dan `aria-selected="true"` bahkan saat state field masih kosong murni
(`value === ""`), karena `value=""` cocok dengan baris clear yang juga `value=""`.

Karena ini perilaku DEFAULT dari base component bersama dengan blast radius besar, perbaikan
ditahan dulu dan disajikan sebagai pilihan bernomor ke pengguna (`base-component-decision-gate`)
sebelum implementasi dimulai. Pengguna memilih opsi yang direkomendasikan: sembunyikan baris clear
bila field wajib ATAU belum ada nilai terpilih.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: siapa pun yang mengisi form dengan field select `BaseSelectField` di seluruh
aplikasi (jauh lebih luas dari sekadar form Buat Invoice Manual).

**Pemicu**: membuka dropdown select apa pun.

**Langkah (sesudah perbaikan)**:

1. Field WAJIB (`field.required === true`, mis. `tariffId`): dropdown TIDAK PERNAH menampilkan
   baris "kosongkan pilihan", baik sebelum maupun sesudah pengguna memilih sesuatu — mengosongkan
   field wajib ke `""` selalu tidak valid, jadi affordance itu memang tidak seharusnya ada.
2. Field OPSIONAL BELUM ada nilai (mis. `categoryId` sebelum dipilih): dropdown langsung
   menampilkan opsi data nyata saja, tanpa baris "clear" di depan yang membingungkan (tidak ada
   apa pun untuk dikosongkan).
3. Field OPSIONAL SUDAH ada nilai (mis. `categoryId` setelah memilih "Registration"): dropdown
   menampilkan opsi yang sedang terpilih (dari cache internal `FilterSelect`, bukan bagian
   perbaikan ini) diikuti baris "Semua kategori" (label placeholder, `value=""`, TIDAK
   `aria-selected`) yang bisa diklik untuk mengosongkan kembali pilihan — affordance ini sekarang
   hanya muncul persis saat dibutuhkan.

**Aturan yang berlaku**: `shouldShowClearOption = !field.required && isFilled(value)`.

**Jalur tidak normal**: `NOT APPLICABLE` — murni penyaringan kondisional atas baris opsi yang
sudah ada.

**Hasil akhir**: dropdown select mana pun di aplikasi tidak lagi menampilkan baris opsi hantu yang
tampak "terpilih" padahal pengguna belum memilih apa pun, dan field wajib tidak lagi menawarkan
cara mengosongkan diri ke state tidak valid.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`base-form-control.jsx` (`BaseSelectField`); `base-ui-utils.jsx` (`addClearOption`,
`isFilled` — dibaca saja, TIDAK diubah); `filter-select.jsx` (mekanisme `selectedOptionCache`
internal yang membuat opsi yang baru dipilih tetap tampil di posisi atas — pre-existing, TIDAK
diubah, dikonfirmasi bukan bagian dari bug ini); `base-editor-field.jsx`/`base-editor-form.jsx`
(alur `selectedOption` per field — dibaca saja); grep `type: "select"` di seluruh
`QuilvianSystemFrontendDev/src` (481 pemakaian, 111 berkas) untuk menilai blast radius sebelum
mengubah default.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `base-form-control.jsx` | `BaseSelectField`: tambah `isFilled` ke import dari `base-ui-utils`; tambah `shouldShowClearOption = !field.required && isFilled(value)`; `selectOptions` hanya memanggil `addClearOption(...)` bila `shouldShowClearOption` true, selain itu memakai `normalizedOptions` langsung tanpa baris clear |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Baris "clear ke kosong" pada `BaseSelectField` | `EXTEND` (mengubah kondisi tampil default) | Mengubah PERILAKU DEFAULT komponen bersama yang dipakai ~481 field di 111 berkas — wajib melalui `base-component-decision-gate`, TIDAK boleh diimplementasikan langsung |

**`UI GATE`**: **DILALUI** — disajikan 3 pilihan bernomor ke pengguna lewat `AskUserQuestion`
(sembunyikan bila required ATAU belum ada nilai [direkomendasikan] / sembunyikan hanya bila
required / jangan sentuh base component sama sekali). Pengguna memilih opsi yang direkomendasikan
sebelum satu baris kode pun diubah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Field wajib, belum ada nilai | Opsi data nyata saja, tanpa baris clear |
| Field wajib, sudah ada nilai | Opsi data nyata saja (termasuk yang terpilih di posisi atas lewat cache internal `FilterSelect`), tetap tanpa baris clear |
| Field opsional, belum ada nilai | Opsi data nyata saja, tanpa baris clear |
| Field opsional, sudah ada nilai | Opsi terpilih di atas (cache internal), lalu baris "kosongkan pilihan" berlabel placeholder, lalu sisa opsi |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada perubahan endpoint, murni perilaku render opsi di klien.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint base-form-control.jsx` | Berhasil tanpa error | `PASS` | Keluaran perintah kosong |
| Field opsional `categoryId`, belum ada nilai, buka dropdown | 11 opsi data nyata, tidak ada baris clear, tidak ada `aria-selected="true"` yang tidak semestinya | `PASS` | `innerText`/`aria-selected` opsi pertama diperiksa langsung |
| Field opsional `categoryId`, pilih "Registration", buka lagi dropdown | 12 opsi: "Registration" (terpilih, dari cache internal `FilterSelect`, pre-existing) di posisi pertama, "Semua kategori" (baris clear, TIDAK terpilih) di posisi kedua, lalu sisa opsi | `PASS` | 4 opsi pertama di-dump berikut teks dan `aria-selected` masing-masing |
| Field wajib `tariffId`, belum ada nilai, buka dropdown (konteks dengan hasil nyata) | 25 opsi data nyata (halaman pertama), tidak ada baris clear | `PASS` | Jumlah dan isi opsi pertama diperiksa langsung |
| Field wajib `tariffId`, pilih satu tarif nyata, buka lagi dropdown | Tetap 25 opsi, urutan sama, tidak ada baris clear yang muncul meski sudah ada nilai — sesuai aturan "field wajib tidak pernah menawarkan clear" | `PASS` | 3 opsi pertama di-dump, dibandingkan dengan hasil sebelum memilih |
| Console error selama pengujian | Hanya noise pra-eksisting tidak terkait (CSP foto profil, 404) — tidak ada error baru dari perubahan ini | `PASS` | Console dipantau selama sesi Playwright |

Uji manual: `PASS`.

**Tidak dijalankan:** `npm run build`/`next build` penuh; regresi otomatis lintas SEMUA 111 berkas
pemakai (481 pemakaian) — diverifikasi manual hanya pada 2 field representatif (satu wajib, satu
opsional) di form yang sama; component test (instruksi eksplisit pengguna sepanjang sesi ini —
tanpa file test).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Field wajib tidak pernah menampilkan baris "clear ke kosong" | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| Field opsional hanya menampilkan baris clear saat ada nilai nyata untuk dikosongkan | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| Perubahan default base component melalui `base-component-decision-gate` dengan persetujuan eksplisit pengguna | Terpenuhi | Lihat § 3.3 |
| lint lulus | Terpenuhi | Lihat § 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Menutup temuan | Ini menutup temuan sampingan yang dicatat di `task/report/frontend/FE-BKC-FIX-002.md` § 8 ("Temuan sampingan... menunggu keputusan pengguna") |
| Cakupan verifikasi vs blast radius | Perubahan ini secara teknis memengaruhi SEMUA ~481 pemakaian `type: "select"` di 111 berkas (lewat satu titik bersama `BaseSelectField`), tapi verifikasi hidup hanya dilakukan pada 2 field representatif dalam satu form (`categoryId` opsional, `tariffId` wajib). Logikanya murni deklaratif (`!field.required && isFilled(value)`, tidak bergantung konteks field/resource tertentu) sehingga risiko regresi dinilai rendah, TAPI belum ada bukti langsung untuk 109 berkas lain yang juga memakai `BaseSelectField` |
| Masalah yang diketahui | `NONE` baru — mekanisme `selectedOptionCache` internal `FilterSelect` (opsi yang baru dipilih tampil di posisi atas) tetap seperti semula, tidak disentuh, dan dikonfirmasi bukan sumber bug ini |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Interupsi | `NONE` |
| Status Git | Modified (task ini): `base-form-control.jsx`. Berkas lain pada working tree yang sama milik task-task sebelumnya (lihat laporan masing-masing). Belum staged/commit |
| Langkah berikutnya | Bila ditemukan consumer `BaseSelectField` lain yang berperilaku tidak seperti diharapkan setelah perubahan ini (mis. bergantung diam-diam pada baris clear yang selalu ada), laporkan sebagai temuan baru dengan detail field/halaman spesifik |
