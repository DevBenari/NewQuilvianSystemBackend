# Laporan Perubahan Frontend — `FE-BKC-FIX-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-004` (ad-hoc enhancement — bukan task roadmap bernomor `FE-BKC-0xx`, dibuat sendiri untuk menjaga jejak laporan tetap tracked) |
| Judul | Field Tarif Layanan/Qty digerbangi Kategori Tarif; verifikasi refresh dropdown dan reset form pada form Buat Invoice Manual |
| Slice | Permintaan langsung pengguna atas UX form `FE-BKC-014` — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap untuk perbaikan ini |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan kontrak API, murni perilaku klien |
| Wewenang UI | `REUSE` murni — properti `field.disabled` yang sudah didukung `base-editor-field.jsx`, bukan base component baru/extend |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — 1 berkas diubah (2 baris `disabled: !form.categoryId` + 1 teks deskripsi), 2 dari 3 permintaan ternyata sudah terpenuhi oleh source yang ada (verifikasi saja, tanpa perubahan kode) |
| Task mode | `FRONTEND` — enhancement ad-hoc, permintaan eksplisit pengguna |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/health-services/billing-management/billing-invoices/create-manual/create-manual-invoice-view.jsx` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit) |
| Commit backend yang dijadikan rujukan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source selesai, lint bersih, **terverifikasi hidup** (login sungguhan, alur penuh: pilih kunjungan → kategori → tarif → qty → submit → sukses → form ter-reset) |

---

## 1. Keadaan yang ditemukan di awal

Pengguna meminta tiga perilaku pada form "Buat Invoice Manual":

1. Field "Tarif Layanan" dan "Qty" harus `disabled` selama "Kategori Tarif" belum dipilih.
2. Saat "Kategori Tarif" diganti, isi dropdown "Tarif Layanan" harus ikut refresh.
3. Setelah invoice berhasil disimpan, field-field form harus dikosongkan/direfresh.

Ditelusuri satu per satu:

- **(1)** Belum ada — sebelumnya `categoryId` murni opsional secara struktural (`description: "Opsional - hanya menyaring..."`), field lain tetap selalu bisa diisi terlepas dari status kategori.
- **(2)** SUDAH ADA di infrastruktur `useSelectResource` (`use-select-resource.jsx`): `paramsKey` yang memicu effect fetch sudah mencakup `safeFiltersKey` (turunan dari `filters.tariffCategoryId`), effect ini retrigger independen dari status buka/tutup dropdown, dan `setState` mengganti `options` sepenuhnya (bukan menambah) saat `append: false` (fetch halaman pertama akibat filter berubah). Tidak butuh perubahan kode, hanya verifikasi hidup.
- **(3)** SUDAH ADA di `handleSubmit` (`use-create-manual-invoice.js` baris ~198): `setForm(buildEmptyForm())` dipanggil setelah `addCatalogCharge` sukses — bahkan salinan teks `previewProps`/`ConfirmModal` yang sudah ada sebelumnya SUDAH mengklaim "form dikosongkan". Tidak butuh perubahan kode, hanya verifikasi hidup.

Jadi hanya (1) yang butuh implementasi baru.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir/staf yang membuat invoice manual lewat "Buat Invoice Manual".

**Langkah (sesudah perbaikan)**:

1. Kasir memilih kunjungan. Field "Kategori Tarif" aktif seperti biasa (selalu opsional secara
   struktural — tidak dikirim ke server, tidak divalidasi wajib saat submit).
2. Selama "Kategori Tarif" belum dipilih, field "Tarif Layanan" dan "Qty" tampil **disabled**
   (tidak bisa diklik/diketik) — mencegah kasir membuka dropdown tarif tanpa filter kategori
   (berpotensi puluhan ribu baris tanpa penyaringan).
3. Begitu kasir memilih Kategori Tarif, kedua field itu langsung aktif.
4. Bila kasir GANTI Kategori Tarif ke kategori lain, isi dropdown Tarif Layanan otomatis
   ter-refresh sesuai kategori baru (tarif yang sebelumnya terpilih ikut dikosongkan — perilaku
   pre-existing dari `handleChange`, dipertahankan apa adanya).
5. Setelah kasir mengisi Tarif Layanan + Qty dan submit berhasil, SELURUH field (termasuk
   Kategori Tarif, Tarif Layanan, Qty, Harga) kembali ke keadaan awal kosong — dan karena
   Kategori Tarif ikut kosong lagi, Tarif Layanan/Qty otomatis kembali `disabled` sampai kasir
   memilih kategori untuk invoice berikutnya.

**Aturan yang berlaku**: `disabled: !form.categoryId` pada field `tariffId` dan `quantity`,
dievaluasi ulang setiap render dari state form yang sama (pola yang sama dengan `filters: {...}`
pada field `tariffId` yang sudah reaktif terhadap `form.categoryId`).

**Jalur tidak normal**: `NOT APPLICABLE`.

**Hasil akhir**: kasir dituntun mengikuti urutan Kunjungan → Kategori → Tarif/Qty, tidak bisa lagi
"melompat" ke Tarif Layanan sebelum menyaring lewat kategori; dan form selalu bersih untuk invoice
berikutnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`create-manual-invoice-view.jsx` (field config); `use-create-manual-invoice.js` (`handleSubmit`,
`handleChange` — dibaca saja, TIDAK diubah, sudah benar); `use-select-resource.jsx` (mekanisme
`paramsKey`/`fetchPage`/`setState` untuk refresh saat filter berubah — dibaca saja, TIDAK diubah,
sudah benar); `base-editor-form.jsx` (mekanisme `dependsOn`/`dependencyMissing` bawaan — dibaca,
DIPUTUSKAN TIDAK DIPAKAI, lihat § 3.3).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `create-manual-invoice-view.jsx` | Field `tariffId` dan `quantity` ditambah `disabled: !form.categoryId`; deskripsi field `categoryId` diperbarui dari "Opsional - hanya menyaring..." menjadi kalimat yang juga menyebut field lain baru aktif setelah kategori dipilih |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Gerbang disabled pada `tariffId`/`quantity` | `REUSE` | `field.disabled` sudah dibaca langsung oleh `base-editor-field.jsx` (`isDisabled = Boolean(disabled \|\| field.disabled \|\| field.readOnly)`) — tidak ada base component baru/extend |

**`UI GATE`**: tidak ada — murni pengaturan nilai properti field yang sudah ada di kontrak
`base-editor-field`.

**Catatan desain — kenapa bukan `dependsOn` bawaan**: `base-editor-form.jsx` sudah punya
mekanisme `field.dependsOn` yang otomatis men-disable field bila field lain kosong DAN field ini
sendiri belum berisi (`hasOwnValue = isFilled(form[field.name])`). Mekanisme itu TIDAK dipakai
untuk `quantity` karena `buildEmptyForm()` mengisi `quantity` dengan default `"1"` sejak awal
(bukan string kosong) — akibatnya `hasOwnValue` SELALU `true` untuk `quantity`, dan `dependsOn`
tidak akan pernah menganggapnya "missing", sehingga field itu tidak akan pernah ter-disable lewat
jalur itu. `field.disabled` eksplisit dipakai sebagai gantinya, konsisten untuk kedua field
(`tariffId` yang defaultnya memang kosong, dan `quantity` yang defaultnya `"1"`).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Kategori belum dipilih | Tarif Layanan dan Qty tampil disabled (tidak bisa diklik/diketik) |
| Kategori sudah dipilih | Tarif Layanan dan Qty aktif |
| Kategori diganti | Dropdown Tarif Layanan refresh ke hasil kategori baru; Tarif Layanan+Harga ikut dikosongkan (pre-existing) |
| Submit sukses | Seluruh field form kembali kosong; Tarif Layanan/Qty kembali disabled (karena Kategori ikut kosong) |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada perubahan endpoint. `GET .../tariffs/options` (existing, `FE-BKC-014`)
tetap dipakai, hanya waktu triggernya yang diverifikasi ulang (sudah otomatis refetch saat filter
kategori berubah).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint create-manual-invoice-view.jsx` | Berhasil tanpa error | `PASS` | Keluaran perintah kosong |
| Kunjungan dipilih, Kategori Tarif BELUM dipilih | Tombol Tarif Layanan dan input Qty `disabled` (dikonfirmasi lewat `isDisabled()`); klik paksa pada tombol Tarif Layanan yang disabled TIDAK membuka dropdown (`listbox` count = 0) | `PASS` | Diperiksa langsung lewat Playwright |
| Kategori Tarif dipilih ("Registration") | Tarif Layanan dan Qty langsung `disabled=false` | `PASS` | Diperiksa langsung |
| Kategori diganti dari "Consultation" (0 hasil tarif untuk kunjungan uji) ke "Laboratory" (25 hasil) | Request `GET .../tariffs/options?tariffCategoryId=...` baru terkirim dengan `tariffCategoryId` kategori baru; jumlah opsi di dropdown berubah dari 0 ke 25 sesuai kategori aktif | `PASS` | URL request dan jumlah opsi ditangkap langsung |
| Alur penuh: pilih kunjungan → kategori "Laboratory" → tarif → Qty=2 → Simpan Invoice | Harga otomatis terisi (462.000) dari tarif dipilih; submit sukses, dialog "Invoice Berhasil Dibuat" tampil, tidak ada error | `PASS` | Dialog sukses terdeteksi, tidak ada alert gagal |
| Setelah dialog sukses ditutup ("Tetap di Sini") | Kategori Tarif kembali ke placeholder "Semua kategori"; Tarif Layanan kembali ke placeholder DAN `disabled=true`; Qty kembali ke `"1"` DAN `disabled=true`; Harga kembali kosong | `PASS` | Nilai dan status `disabled` tiap field diperiksa langsung setelah reset |
| Console error selama pengujian | Hanya noise pra-eksisting tidak terkait (CSP foto profil, dsb.) — tidak ada error baru dari perubahan ini | `PASS` | Console dipantau selama sesi Playwright |

Uji manual: `PASS`.

**Tidak dijalankan:** `npm run build`/`next build` penuh; component test (instruksi eksplisit
pengguna sepanjang sesi ini — tanpa file test).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tarif Layanan dan Qty disabled selama Kategori Tarif belum dipilih | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| Dropdown Tarif Layanan refresh saat Kategori Tarif diganti | Terpenuhi (pre-existing), terverifikasi hidup dengan kategori berbeda sungguhan | Lihat § 6 |
| Form dikosongkan/direfresh setelah invoice berhasil disimpan | Terpenuhi (pre-existing), terverifikasi hidup lewat alur submit penuh | Lihat § 6 |
| lint lulus | Terpenuhi | Lihat § 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Cakupan perubahan sesungguhnya | Dari 3 permintaan, hanya permintaan (1) yang butuh source berubah. Permintaan (2) dan (3) SUDAH terpenuhi sejak `FE-BKC-014`/desain awal form — laporan ini menjadi bukti verifikasi hidup formal untuk keduanya, bukan implementasi baru |
| Masalah yang diketahui | `NONE` baru. Kombinasi kategori+kunjungan tertentu masih bisa punya nol tarif (perilaku data as-is, sudah dicatat di `FE-BKC-FIX-002`) — sekarang lebih terlihat karena kategori wajib dipilih dulu untuk menguji Tarif Layanan |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Interupsi | `NONE` |
| Status Git | Modified (task ini): `create-manual-invoice-view.jsx`. Berkas lain pada working tree yang sama milik task-task sebelumnya (lihat laporan masing-masing). Belum staged/commit |
| Langkah berikutnya | `SELESAI` — ketiga permintaan pengguna terpenuhi dan terverifikasi hidup |
