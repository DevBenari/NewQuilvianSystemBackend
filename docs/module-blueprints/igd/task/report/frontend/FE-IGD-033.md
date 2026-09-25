# `FE-IGD-033` — Tombol Simpan Pemeriksaan berhenti aktif sesudah penilaian tersimpan

| Field | Nilai |
| --- | --- |
| Task | `FE-IGD-033` |
| Gelombang | R3.10 — 17 September 2026 |
| Status | ✅ **SELESAI — 17 September 2026.** Lint `PASS`, unit test **866/866**, `npm run build` lulus, dan **uji lewat layar oleh pemilik LULUS** — kelima kriteria. Tanpa UAT |
| Pemicu | Pertanyaan pemilik pada uji layar 17 September 2026: *"apakah tidak perlu klik pemeriksaan lagi ketika sudah simpan pemeriksaan dan muncul field dokter?"* |
| Frontend | branch `RizkiV2`, di atas `f05c323e5` |
| Pasangan wajib | **`BE-IGD-047`** — lihat bagian 2. Keduanya harus dirilis bersama |
| Kontrak | Nol perubahan. Nol endpoint baru, nol backend |

---

## 1. Apa yang terjadi pada pemilik

Alur yang dijalankan:

1. Mengisi penilaian triage → menekan **Simpan Pemeriksaan** → tersimpan, `Penilaian ke-1`
2. Bagian **pilih dokter pemeriksa** muncul — memang dirancang muncul hanya sesudah
   penilaian tersimpan
3. Memilih dokter, lalu menekan **Simpan Pemeriksaan** lagi → `409`

Langkah 3 adalah kesimpulan yang wajar: ada isian baru, dan tombol simpan masih menyala. Yang
keliru bukan pemiliknya, melainkan layarnya.

Penetapan dokter punya tombolnya sendiri, **Tetapkan Dokter**, di dalam bagian itu. Tombol
**Simpan Pemeriksaan** sudah selesai tugasnya sejak langkah 1 — tetapi tetap aktif, tetap
bertulisan sama, dan tetap mengirim permintaan **pembuatan penilaian baru**.

## 2. Mengapa perbaikan ini wajib menyertai `BE-IGD-047`

Sebelum `BE-IGD-047`, penekanan kedua menghasilkan `409`. Membingungkan, tetapi **tidak ada
data salah yang tersimpan**.

Sesudah `BE-IGD-047`, nomor urut dihitung server dengan benar — sehingga penekanan kedua
**berhasil**, dan diam-diam menambahkan `Penilaian ke-2` ke riwayat klinis pasien. Perawat
tidak diberi tahu apa pun; ia mengira baru saja menyimpan pilihan dokternya.

**Memperbaiki backend saja mengubah galat yang berisik menjadi catatan klinis ganda yang
senyap.** Karena itu kedua task ini satu paket, dan `BE-IGD-047` tidak boleh dirilis sendirian.

## 3. Yang diubah

Satu berkas, `emergency-triage-form-view.jsx`:

| # | Perubahan |
| ---: | --- |
| 1 | Tombol **Simpan Pemeriksaan** dinonaktifkan begitu `savedTriage` terisi |
| 2 | Tulisannya berubah menjadi **Pemeriksaan Tersimpan**, sehingga keadaannya terbaca tanpa menebak |
| 3 | Pesan berhasil menunjuk langkah berikutnya: menetapkan dokter, dan menyatakan penilaian tidak perlu disimpan lagi |

Nol komponen bersama diubah, nol komponen baru dibuat, nol perubahan CSS global.

## 4. Yang **tidak** dikerjakan, dan alasannya

| Hal | Alasan |
| --- | --- |
| Menyatukan penetapan dokter ke dalam satu tombol simpan | Mengubah alur kerja yang sudah diputuskan — dokter memang sengaja ditentukan **sesudah** penilaian tersimpan, karena pilihannya bergantung pada hasil penilaian. Itu keputusan produk, bukan perbaikan cacat |
| Menyembunyikan tombol sama sekali | Tombol yang hilang membuat perawat mengira layarnya rusak. Dinonaktifkan beserta tulisan yang berubah lebih terbaca |
| Penjaga backend terhadap penilaian ganda pada satu kunjungan | **Lubang yang tetap terbuka.** Layar kini tidak lagi memancingnya, tetapi API masih menerima penilaian kedua dari pemanggil mana pun. Penjaga sesungguhnya harus di backend, dan belum ada requirement maupun keputusan yang memintanya — sama persis dengan lubang "pengkajian ganda oleh dua perawat" pada `FE-IGD-031` |

## 5. Validasi

| Jenis | Hasil |
| --- | --- |
| `npm run lint:errors` | ✅ exit 0 |
| Unit test | ✅ **866 lulus, 0 gagal** |
| `npm run build` | ✅ **LULUS 17 September 2026** |
| Uji lewat layar | ✅ **LULUS 17 September 2026** — tombol nonaktif bertulisan "Pemeriksaan Tersimpan"; **Tetapkan Dokter** menetapkan dr. Rendy Pangalila tanpa menyentuh penilaian; riwayat memuat tepat satu penilaian |

## 6. Uji layar yang membuktikan perbaikan ini

Dijalankan **sesudah** `BE-IGD-047` lolos build dan uji API.

| # | Langkah | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | Simpan penilaian triage pada pasien baru | Tersimpan. Tombol berubah menjadi **Pemeriksaan Tersimpan** dan **tidak dapat ditekan** |
| 2 | Pilih dokter, tekan **Tetapkan Dokter** | Dokter tertetapkan tanpa menyentuh penilaian |
| 3 | Buka riwayat triage pasien itu | **Tepat satu** penilaian — bukan dua |
