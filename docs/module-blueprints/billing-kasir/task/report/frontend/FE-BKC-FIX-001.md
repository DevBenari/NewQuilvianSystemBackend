# Laporan Perubahan Frontend — `FE-BKC-FIX-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-001` (ad-hoc bug fix — bukan task roadmap bernomor `FE-BKC-0xx`, dibuat sendiri untuk menjaga jejak laporan tetap tracked) |
| Judul | Indikator scroll horizontal pada tabel item tagihan pasien (Menu Pembayaran) |
| Slice | Ditemukan saat verifikasi manual `FE-BKC-016` — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap untuk perbaikan ini |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan kontrak API |
| Wewenang UI | Tambah indikator visual (fade) murni dekoratif — tidak ada base component baru/extend |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — skor 1 (repo 0, berkas diperiksa 0, berkas diubah 0 [2 berkas], logika bisnis 1/Sedang — murni CSS+markup dekoratif, kontrak API 0, database 0, keamanan 0, UI/workflow 0) |
| Task mode | `FRONTEND` — bug fix ad-hoc, otorisasi eksplisit pengguna ("Boleh lakukan perbaikkan nya juga") |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx`, `src/style/health-services/billing-management/menu-pembayaran.module.css` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit) |
| Commit backend yang dijadikan rujukan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source selesai, lint bersih, **terverifikasi hidup** (login sungguhan) di lebar viewport sempit (800px, tempat masalahnya direproduksi) dan lebar (1440px, memastikan tidak ada regresi) |

---

## 1. Keadaan yang ditemukan di awal

Pengguna melaporkan tabel item tagihan pasien di Menu Pembayaran terlihat "berantakan" pada kolom
Qty dan Harga Satuan. Ditelusuri: pada state/lebar layar TERTENTU, DOM dan CSS tabel itu sendiri
terbukti benar (diverifikasi lewat pembacaan `outerHTML` langsung dan klik-coba di beberapa lebar
viewport 1024–1440px — kolom sejajar dengan benar). Root cause spesifik dari screenshot pengguna
tidak berhasil direproduksi ulang (kemungkinan bundle Next.js yang belum di-refresh saat itu).

Namun proses investigasi menemukan masalah NYATA yang berbeda: tabel (`baseStyles.tableWrapper`
punya `overflow-x: auto`, `dataTable` punya `min-width: max(840px, 100%)`) di bawah ~840px lebar
viewport butuh scroll horizontal, tapi tidak ada indikator visual apa pun bahwa kolom "Harga (Rp)"
masih ada di sebelah kanan — kolom itu bisa terpotong diam-diam. Pengguna menyetujui perbaikan ini
dikerjakan.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir, di layar Menu Pembayaran mana pun yang punya tabel item tagihan.

**Pemicu**: lebar jendela browser/layar kasir lebih sempit dari yang dibutuhkan tabel untuk
menampilkan semua kolom tanpa scroll (di bawah ~840px area konten).

**Langkah**:

1. Kasir membuka Menu Pembayaran pada layar sempit.
2. Tabel item tagihan menampilkan kolom sebanyak muat, dengan **fade tipis di tepi kanan** yang
   menandakan masih ada kolom lain (Harga Satuan/Qty/Harga (Rp), tergantung seberapa sempit) yang
   perlu digulir untuk dilihat.
3. Kasir menggulir tabel ke kanan (gesture standar horizontal scroll) untuk melihat kolom yang
   tersembunyi.

**Aturan yang berlaku**: fade selalu tampil (tidak otomatis hilang saat sudah di ujung scroll) —
trade-off yang disengaja demi kesederhanaan dan menghindari benturan dengan `overflow-y` milik
tabel yang sama (lihat komentar kode).

**Jalur tidak normal**: `NOT APPLICABLE` — murni penanda visual, tidak ada interaksi baru.

**Hasil akhir**: kasir tidak lagi salah kira tabel "kehabisan" kolom ketika sebenarnya perlu
digulir.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`src/components/view/.../menu-pembayaran/menu-pembayaran-view.jsx` (blok tabel item);
`src/style/health-services/billing-management/menu-pembayaran.module.css` (`.itemTableScroll`);
`src/style/components/features/base-features/base-data-components.module.css`
(`.tableWrapper`/`.dataTable` — dibaca saja, TIDAK diubah).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `menu-pembayaran-view.jsx` | Wrapper tabel item dibungkus `<div className={styles.itemTableScrollOuter}>` tambahan; satu `<div className={styles.itemTableScrollFade} aria-hidden="true" />` ditambahkan setelah tabel |
| `menu-pembayaran.module.css` | Kelas baru `.itemTableScrollOuter` (`position: relative`, murni pembungkus posisi) dan `.itemTableScrollFade` (gradient tipis 28px di tepi kanan, `pointer-events: none`) |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Indikator fade tepi kanan | `REUSE` (CSS lokal) | Murni `div` dekoratif + CSS gradient pada modul CSS feature-scoped yang sudah ada (`menu-pembayaran.module.css`) — TIDAK menyentuh `base-data-components.module.css` (shared) sama sekali, tidak ada base component baru/extend |

**`UI GATE`**: tidak ada — seluruhnya CSS lokal, tidak ada keputusan pengguna yang perlu ditunggu.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` |
| Kosong | `NOT APPLICABLE` |
| Gagal | `NOT APPLICABLE` |
| Tanpa hak akses | `NOT APPLICABLE` |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada permintaan API baru.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` (2 berkas berubah) | Berhasil tanpa error | `PASS` | Keluaran perintah kosong |
| Buka Menu Pembayaran pada lebar 800px (kondisi bermasalah) | Fade tepi kanan tampil jelas; kolom "Harga (Rp)" yang terpotong kini punya penanda visual | `PASS` | Screenshot zoom pada area tabel — fade terlihat jelas di tepi kanan |
| Buka Menu Pembayaran pada lebar 1440px (regresi — tidak butuh scroll) | Fade tetap tampil tipis tapi tidak mengganggu; tidak ada elemen rusak/terpotong lain | `PASS` | Screenshot penuh halaman |
| Console error saat kedua pengujian | Tidak ada error baru yang berkaitan dengan perubahan ini (error lain yang muncul — `Performance.measure`, `404` foto profil, `422` — sudah ada sebelum perubahan ini, tidak berkaitan dengan tabel) | `PASS` | Daftar console error difilter |

Uji manual: `PASS`.

**Tidak dijalankan:** `npm run build`/`next build` penuh; component test (instruksi eksplisit
pengguna sepanjang sesi ini — tanpa file test).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kolom yang terpotong scroll horizontal punya penanda visual | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| Tidak ada regresi pada lebar viewport yang sudah cukup lebar | Terpenuhi, terverifikasi hidup | Lihat § 6 |
| lint lulus | Terpenuhi | Lihat § 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Perbaikan ini menjawab masalah "scroll horizontal tanpa penanda" yang DITEMUKAN saat investigasi, bukan reproduksi persis dari screenshot awal pengguna (kolom tergeser). Root cause screenshot awal **tidak berhasil dipastikan** — kemungkinan bundle lama sebelum hard-refresh. Bila masalah "kolom tergeser" itu masih muncul setelah perbaikan ini, itu masalah terpisah yang butuh laporan/screenshot baru untuk ditelusuri ulang |
| Masalah yang diketahui | Fade selalu tampil (tidak otomatis hilang saat tabel sudah di-scroll penuh ke kanan) — trade-off desain yang disengaja, dijelaskan di komentar kode |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Interupsi | `NONE` — task ini murni tindak lanjut dari laporan bug pengguna pada sesi yang sama |
| Status Git | Modified (task ini): `menu-pembayaran-view.jsx`, `menu-pembayaran.module.css`. Berkas lain pada working tree yang sama milik `FE-BKC-014`/`015`/`016` (lihat laporan masing-masing). Belum staged/commit |
| Langkah berikutnya | Bila laporan awal "kolom tergeser" muncul lagi setelah hard-refresh browser, kirim screenshot baru beserta lebar jendela browser saat itu untuk ditelusuri sebagai temuan terpisah |
