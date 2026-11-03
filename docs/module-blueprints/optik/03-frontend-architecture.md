# Optik — Arsitektur Frontend (Kontrak Fungsional)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| Dokumen | `OPT-FE-ARCH` revision `1` — `approved` (Yoga Aji Pratama, 2026-11-03) |
| Owner UI | Yoga Aji Pratama (`OPT-DEC-026`) |
| Masukan | `00-interview-decisions.md` r4 bagian 6 (`OPT-FE-001`..`005`), `contracts/api-contract.md` `OPT-API-v1` |
| Repository | `QuilvianSystemFrontendDev` (Next.js App Router, JavaScript/JSX, Redux, Axios) |
| Acuan pola | Modul MCU v2: route `src/app/health-services/medical-checkup-management/`, view `src/components/view/health-services/medical-checkup-management/`, service `src/lib/services/health-services/medical-checkup-management/` |

> **Batas wewenang.** Dokumen ini menetapkan *apa* yang harus bisa dilakukan setiap layar.
> Menu, nama menu, daftar layar final, route, serta tab/modal/drawer **belum** ditetapkan. Semuanya
> diusulkan di screen brief dan disetujui pemilik (`OPT-FE-001`, `002`). Detail visual dalam
> konvensi proyek berstatus `DEV_DISCRETION` (`OPT-FE-004`).

## 1. Hierarki wewenang

```text
keamanan/privasi/invariant (izin, status, data sensitif)
  -> screen brief Optik yang disetujui (belum ada)
  -> konvensi proyek (komponen dasar, pola MCU/Lab)
  -> DEV_DISCRETION
```

## 2. Kebutuhan layar (usulan isi, bukan tata letak)

| ID | Kebutuhan | Pelaku | Data dikonsumsi | Aksi |
| --- | --- | --- | --- | --- |
| `FE-OPT-N01` | Daftar kunjungan Optik hari ini / terbuka | Admisi, RO, Sp.M, petugas penjualan | `GET visits/` | Daftarkan kunjungan, buka kunjungan |
| `FE-OPT-N02` | Pendaftaran kunjungan Optik (datang sendiri, rujukan Poli Mata, keluhan) | Admisi, petugas penjualan | Pencarian pasien (Patient Management), penjamin, `POST visits/` | Simpan |
| `FE-OPT-N03` | Ringkasan kunjungan | Semua peran Optik | `GET visits/{encounterId}` | Mulai pemeriksaan, buat/salin resep, buat pesanan, catat keluhan |
| `FE-OPT-N04` | Form pemeriksaan refraksi | RO, Sp.M | `refraction-exams` | Isi, simpan final, revisi (dengan alasan), batal draft |
| `FE-OPT-N05` | Form resep kacamata/lensa kontak + salin resep luar | RO, Sp.M | `prescriptions` | Simpan, revisi, lewat rentang (Sp.M) |
| `FE-OPT-N06` | Riwayat versi resep dan pemeriksaan | RO, Sp.M, petugas penjualan (baca) | detail beserta versi | Bandingkan versi |
| `FE-OPT-N07` | Pesanan: pilih resep berlaku, pilih barang dari katalog, simpan & tagihkan | Petugas penjualan | `prescriptions/options`, `reference-data/item-catalog`, `orders` | Simpan draft, tagihkan, ajukan diskon (endpoint Billing) |
| `FE-OPT-N08` | Status bayar dan kirim ke pengerjaan | Petugas penjualan | `orders/{id}/payment-status` | Kirim (tujuan + perkiraan jadi) |
| `FE-OPT-N09` | Daftar pesanan: filter status, "perlu dihubungi", "terlambat" | Petugas penjualan, Kepala Optik | `GET orders/` | Tandai siap, serahkan, ajukan batal |
| `FE-OPT-N10` | Penyerahan | Petugas penjualan | `orders/{id}` | Centang identitas, isi wakil, catatan *fitting*, serahkan |
| `FE-OPT-N11` | Persetujuan batal dan tutup tidak diambil | Kepala Optik | pengajuan batal | Setujui/tolak, tutup |
| `FE-OPT-N12` | Keluhan garansi | Petugas, RO/Sp.M, Kepala Optik | `warranty-claims` | Catat, periksa ulang, putuskan |
| `FE-OPT-N13` | Katalog dan profil barang Optik | Admin Optik | `reference-data/item-catalog`, `item-profiles` | Ubah profil |
| `FE-OPT-N14` | Pengaturan Optik | Admin Optik | `reference-data/settings` | Ubah (berlaku ke depan) |
| `FE-OPT-N15` | Empat laporan | Kepala Optik, Admin Optik | `reports/*` | Saring periode |

## 3. Aturan tampilan yang mengikat (bukan selera)

| Aturan | Dasar |
| --- | --- |
| Tombol aksi hanya tampil/aktif untuk peran dan status yang sah (`state-transition-matrix.md`). Backend tetap penjaga utama | `OPT-FE-005` |
| Isian ukur (SPH, CYL, ADD, PD, BC, DIA) memakai pola input desimal proyek, **bukan** `type: "number"` yang membuang desimal (contoh −1,50 jangan menjadi −150). Tanda minus harus bisa diketik | `OPT-FE-003`; jebakan `BaseTextField` |
| Saat menyalin resep luar bernotasi plus, layar menampilkan hasil transposisi dan teks aslinya sebelum disimpan | `OPT-DEC-045` |
| Isian di luar rentang ditandai; untuk Sp.M muncul isian alasan, untuk RO tombol simpan ditolak | `OPT-DEC-045` |
| Pesanan menampilkan versi resep yang dipakai dan tanda "terkunci" | `OPT-DEC-007` |
| Status bayar menampilkan bagian pasien, minimum uang muka, terbayar, dan sisa, dalam rupiah dari backend (bukan dihitung ulang di layar) | `OPT-DEC-028`, `044` |
| Harga dan diskon dibaca dari Billing; layar Optik tidak menyimpan harga | `OPT-DEC-021`, `027` |
| Penyerahan tidak bisa dikirim tanpa centang identitas | `OPT-DEC-048` |
| Label status dalam Bahasa Indonesia dari `reference-data/metadata`, tidak di-hardcode | Konvensi |
| Data sensitif (ukuran, keluhan, temuan) tidak ditulis ke `console` atau log klien | Kamus data |

## 4. Penanganan state

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka memuat per bagian; tombol aksi nonaktif |
| Kosong | Pesan bermakna, misalnya "Belum ada pemeriksaan pada kunjungan ini" beserta aksi berikutnya yang sah |
| Galat `400` | Pesan backend ditampilkan di dekat isian terkait (`validation-matrix.md`) |
| Galat `403` | Pesan izin/STR-SIP; tidak dicoba ulang otomatis |
| Galat `409` | Pesan backend, lalu muat ulang data (status/versi sudah berubah) |
| Galat `503` | "Modul pendukung sedang tidak dapat dihubungi"; tombol coba lagi |
| Data basi | Setiap aksi mengirim `ExpectedVersion`; sesudah aksi, detail dimuat ulang |
| Klik ganda | Satu `Idempotency-Key` per niat aksi; tombol nonaktif selama permintaan berjalan |
| Cache | Daftar pesanan dan status bayar dimuat ulang sesudah aksi yang mengubah status; status bayar tidak di-cache lama karena berubah oleh kasir |

## 5. Matriks wewenang UI

| ID | Area | Owner | Status | Allowed range |
| --- | --- | --- | --- | --- |
| `OPT-FE-001` | Menu, nama menu, daftar layar | Yoga Aji Pratama | `draft` — menunggu screen brief | Pola paritas Lab/Radiologi/MCU; menu sidebar didaftarkan di `src/utils/menu-sidebar/menu-items.jsx` dengan `requiredPermission` |
| `OPT-FE-002` | Route, pembagian daftar/detail, tab/modal/drawer | Yoga Aji Pratama | `draft` | Usulan route dasar `src/app/health-services/optical-management/…` (mengikuti pola MCU); final di screen brief |
| `OPT-FE-003` | Isian resep | Yoga Aji Pratama | `approved` (isi) | Tata letak `DEV_DISCRETION` |
| `OPT-FE-004` | Warna, jarak, komponen dasar, urutan kolom | Developer | `DEV_DISCRETION` | Konvensi proyek |
| `OPT-FE-005` | Tombol aksi status | Yoga Aji Pratama | `approved` (aturan) | Bagian 3 |

## 6. Sisa lama yang tidak dipakai

| Sisa | Keadaan | Penanganan |
| --- | --- | --- |
| `src/utils/config.jsx` item "Pendaftaran Pasien Optik" | Berkas tidak diimpor di mana pun (`OPT-CAP-29`) | Tidak dipakai; perapian menjadi task tersendiri bila disetujui |
| Kunci `menuOptik` di `left-sidebar-menu-handle.jsx` | Sisa | Dibiarkan sampai menu baru dibuat |
| `route-guard-link.js` `/Optik` untuk Dokter/Perawat | Aturan peran lama | Tidak dipakai menu baru (izin memakai `requiredPermission`) |
| Layar Optik v1 | Data contoh tertulis di kode (`OPT-CAP-30`) | Acuan tampilan saja |

## 7. Ketergantungan test

| Test | Butuh |
| --- | --- |
| Unit FE (rumus tampilan, transposisi pratinjau) | `node --import ./tests/helpers/register.mjs --test tests/unit/` |
| E2E | Akun RO, Sp.M, petugas penjualan, Kepala Optik dengan izin per departemen-jabatan; unit dan depo Optik; barang bertarif; STR/SIP aktif |
