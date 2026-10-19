# Uji Layar Radiologi dengan Data dan Akun Sungguhan — `RAD-EVD-002`

| Field | Isi |
| --- | --- |
| Tanggal | 2026-10-15 |
| Instruksi | Pemilik modul Yoga Aji Pratama: "lanjut langkah berikutnya yang kamu rekomendasikan" — menutup batas verifikasi `FE-RAD-09`, `FE-RAD-10`, `FE-RAD-14`, `FE-RAD-16`, `FE-RAD-17`, `FE-RAD-18` |
| Lingkungan | Build standalone FE (`127.0.0.1:3710`, HEAD `934c63b31` + perubahan di bawah) → backend lokal (`localhost:7184`, HEAD `da3f3eb9` + perubahan di bawah) → devYoga |
| Akun | **Bayu Ramadhan** (Radiografer) dan **dr. Arya** (Kepala Instalasi Radiologi) — izin asli dari `SysAccessPolicies`, tidak dipalsukan. Tulis di luar modul Radiologi diblokir skrip uji |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |

---

## 1. Alur klinis penuh lewat layar

Pesanan uji `RAD-ORD-261014090252-A89DD1` (CR, kunjungan uji `UAT FECRM PASIEN IGD`).

| # | Langkah | Pelaku | Hasil |
| --- | --- | --- | --- |
| 1 | Rencanakan Study Baru → `-002` | Bayu | 200 |
| 2 | Identitas Sudah Sesuai; butir *Skrining kehamilan* = Tidak berlaku; Nyatakan Lolos Keselamatan | Bayu | **Awalnya 400** — cacat 3.1; sesudah perbaikan 200, *Lolos Keselamatan* |
| 3 | Mulai → Selesai Mengambil Citra | Bayu | 200 |
| 4 | Catat Bahan Terpakai: Film 35x43 cm × 2 lembar | Bayu | 200 (`itemType: "Film"`) |
| 5 | Citra Tidak Layak → **Tandai Perlu Diulang** | Bayu | 200, status *Perlu Diulang* — sisa `FE-RAD-09` |
| 6 | Ulangi Pemeriksaan (Kondisi pasien) → `-003` | Bayu | **Awalnya diam tanpa kiriman** — cacat 3.2; sesudah perbaikan 200 |
| 7 | `-003`: keselamatan, Mulai, **Hentikan di Tengah Jalan** (Kerusakan alat) | Bayu | Cacat 3.2 yang sama; sesudah perbaikan 200, status *Dihentikan* |
| 8 | Ulangi Pemeriksaan (Kesalahan internal) → `-004` → keselamatan → ambil citra → **Citra Layak Dibaca** | Bayu | 200, *Mutu Diterima* (fakta tagih terbit ke Billing — perilaku rancangan) |
| 9 | Daftar Hasil Bacaan: bacaan `Pending` lahir otomatis, keadaan *Menunggu Draf* | Bayu | Tampil — sisa `FE-RAD-10`; penghitung halaman salah (cacat 3.3) |
| 10 | Setup → Template Hasil: tambah `THX-NORMAL` | dr. Arya | 200 |
| 11 | Draf bacaan: Peran *Radiografer*, Saran diketik sendiri, pakai template → Temuan + Kesimpulan terisi, Saran utuh; Simpan Draf | Bayu | 200 |
| 12 | Sahkan Bacaan → Rilis ke Dokter Pengirim | dr. Arya | 200, 200 — penulis ≠ pengesah (`RAD-DEC-003`) |

## 2. Layar sisi pasien

Pesanan uji kedua `RAD-ORD-261015083223-9FC3F1` dibuat superadmin lewat API (kunjungan uji yang sama, UJI-RAD).

| Skenario | Hasil |
| --- | --- |
| Detail Pasien: tarif UJI-RAD "Belum diatur"; **Ubah Pemeriksaan** → *THORAX PA + LATERAL* (katalog v1) + alasan | **Awalnya pilihan kosong** — cacat 3.4; sesudah perbaikan 200, tarif tampil 599.000 |
| Cetak Film dinyalakan, bertahan setelah muat ulang | Lulus |
| Beranda, Daftar Pasien RJ, Rincian Pasien (599.000), Print Hasil pesanan pertama (bacaan rilis), Riwayat, Statistik Pemeriksaan, Tarif Tindakan, Daftar Bacaan | **10/10** |
| Tarif Tindakan 482 pemeriksaan: berhalaman, pencarian, penyaring "belum diatur" (32) | **3/3** sesudah perbaikan cacat 3.5 |
| Layar terhadap API tiruan (regresi) | **87/87** |

## 3. Cacat yang ditemukan dan diperbaiki

Kelimanya lolos dari uji API tiruan karena tiruan menerima apa pun yang dikirim layar, atau karena datanya terlalu sedikit.

| # | Cacat | Akibat | Perbaikan |
| --- | --- | --- | --- |
| 3.1 | Layar mengirim enum sebagai nama (`"NotApplicable"`), backend hanya menerima angka | `POST /safety-checks` selalu 400 — **gerbang keselamatan tak pernah dapat dilewati dari layar**; juga `abortCause`, `repeatCause`, `itemType`, `authorRole` | Backend: `[JsonConverter(typeof(JsonStringEnumConverter))]` pada enam ruas request; angka tetap diterima. Uji `RadRequestEnumJsonTests` (3) |
| 3.2 | Modal *Ulangi Pemeriksaan* dan *Hentikan di Tengah Jalan* menyimpan isian sebagai `repeatCause`/`abortCause`, pembangun payload hanya membaca `cause`/`reason` | Klik diam tanpa kiriman dan tanpa pesan — paparan yang sudah terjadi tak dapat dicatat | FE `rad-acquisition-rules.js`: kedua bentuk diterima; uji unit bentuk modal |
| 3.3 | Slice daftar bacaan membaca `totalRows`, backend mengirim `totalData`; teks "endpointnya belum ada" usang sejak `BE-RAD-16` | "0 sampai 0 dari 0 data · 0 bacaan" walau ada baris; keterangan menyesatkan | FE slice membaca `totalData`; teks diganti keterangan *Menunggu Draf* |
| 3.4 | Pilihan Ubah Pemeriksaan dari master `procedures/options` (`Procedure : Read`) | Radiografer 403 → pilihan kosong | Backend `GET rad-orders/procedure-options` (`RadOrder : Update`, pola `BE-RAD-20`); FE resource `radProcedures`. Uji `PilihanPemeriksaan_HanyaRadiologiAktif_DanBisaDicari` |
| 3.5 | Tarif Tindakan tanpa paginasi/pencarian | 482 baris sekaligus, halaman ±31.000 px | Paginasi 25, pencarian, penyaring "belum diatur" |

Penyeragaman istilah: template memakai "Kesimpulan" (sebelumnya "Kesan"), sama dengan form draf.

## 4. Temuan lingkungan (bukan Radiologi)

- Merge `QuilvianIntegrationBackend` membawa *check constraint* Finance berbasis regex PostgreSQL (`~`). Harness SQLite uji
  Radiologi kini melepasnya (`ModelUjiSqlite`); tanpa itu 71 uji gagal di pembentukan skema.
- Merge FE membawa dependensi `@fontsource/barlow-semi-condensed` dan `react-zoom-pan-pinch`; `npm install` sesuai lockfile
  diperlukan sebelum build lokal.

## 5. Verifikasi akhir

| Bukti | Hasil |
| --- | --- |
| `dotnet test` RadiologyTests | **75/75** |
| Uji unit FE Radiologi | **218/218**; seluruh suite 2.948/2.975 — 27 gagal, nol di Radiologi |
| ESLint berkas yang berubah | 0 galat |
| `npm run build` | Lulus |

Data uji yang tertinggal di devYoga: study `-002` (Perlu Diulang), `-003` (Dihentikan), `-004` (Mutu Diterima, bacaan dirilis
versi 1) pada pesanan pertama; pesanan kedua `RAD-ORD-261015083223-9FC3F1` (THORAX PA + LATERAL, Cetak Film); template
`THX-NORMAL`; satu fakta tagih Radiologi untuk kunjungan uji.
