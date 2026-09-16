# Radiologi — Frontend Architecture

| Field | Value |
|---|---|
| Contract version | `RAD-ARCH-FE-001` |
| Revision | `2` |
| Status | `approved` |
| Frontend SHA | `f66ed1885` |
| Backend SHA | `0e2eb105` |
| Input | `RAD-ARCH-BE-001`, `RAD-API-001`, `RAD-STATE-001`, `RAD-PERM-001` |
| Slice | `S15`, melayani `S1`, `S2`, `S3`, `S4`, `S6`, `S7`, `S9`, `S10`, **`S12`**, `S13`, `S14` |

> **Keadaan awal.** Frontend Radiologi **belum ada sama sekali** (`RAD-CAP-029`). Tidak ada satu
> pun halaman, service, atau state yang memanggil `rad-orders`, `rad-studies`, maupun
> `radiology-management`. Seluruhnya dibangun dari nol.

---

## 1. Hierarki Kewenangan UI

Dokumen ini **tidak** menetapkan warna, tata letak, urutan menu, atau pilihan antara tab, modal,
dan drawer. Penetapan itu mengikuti urutan wewenang berikut:

```text
keamanan / privasi / invariant
  -> brief produk atau UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

| Tingkat | Keadaan pada modul ini |
|---|---|
| Keamanan, privasi, invariant | **Mengikat.** Ditetapkan dokumen ini pada bagian 5 |
| Brief produk atau UI yang disetujui | **Tidak ada.** Belum pernah dibuat untuk Radiologi |
| Konvensi project | **Mengikat.** Modul Laboratorium menjadi pola terdekat |
| `DEV_DISCRETION` | Seluruh sisanya |

**Karena tidak ada brief UI yang disetujui, sebagian besar keputusan tampilan berstatus
`DEV_DISCRETION` dengan syarat mengikuti pola Laboratorium.** Agent tidak menetapkan selera
tampilan.

---

## 2. Pola yang Ditiru

Modul Laboratorium sudah lengkap dan terbukti berjalan. Strukturnya ditiru, bukan ditemukan
ulang.

| Aspek | Pola Laboratorium yang ditiru | Lokasi |
|---|---|---|
| Route halaman | Satu folder per kelompok layar | `src/app/health-services/laboratory-management/` |
| Service API | Satu berkas per kelompok endpoint | `src/lib/services/health-services/laboratory-management/` |
| Redux slice | Satu slice per kelompok data | `src/lib/state/slice/health-services/laboratory-management/` |
| Konstanta | Satu folder per modul | `src/lib/constants/health-services/laboratory-management/` |
| Hook | Satu folder per modul | `src/lib/hooks/health-services/laboratory-management/` |
| Component tampilan | Satu folder per modul | `src/components/view/health-services/laboratory-management/` |
| Panel keadaan | Panel memuat, kosong, gagal | `src/components/features/health-services/laboratory-management/laboratory-state-panel/` |

Struktur yang diusulkan untuk Radiologi mengikuti pola yang sama dengan nama
`radiology-management`. **Nama folder dan berkas persisnya adalah `DEV_DISCRETION`** selama
mengikuti pola di atas.

---

## 3. Kebutuhan Layar

Layar dikelompokkan menurut siapa yang memakainya, bukan menurut tabel.

### 3.1 Untuk dokter pengirim

| Layar | Kegunaan | Data yang dikonsumsi | Slice |
|---|---|---|---|
| Buat pesanan radiologi | Memesan pemeriksaan untuk pasien yang sedang ditangani | `POST /rad-orders`; daftar alat dan prosedur | `S1` |
| Daftar pesanan saya | Melihat pesanan yang pernah dibuat beserta statusnya | `GET /rad-orders` | `S1` |
| Lihat hasil bacaan | Membaca kesimpulan radiolog | `GET /rad-reports/by-encounter/{id}` | `S14` |
| Riwayat versi hasil | Melihat versi lama bila ada koreksi | `GET /rad-reports/{id}/versions` | `S10` |

### 3.2 Untuk petugas pendaftaran radiologi

| Layar | Kegunaan | Data yang dikonsumsi | Slice |
|---|---|---|---|
| Antrian pesanan masuk | Menerima, menjadwalkan, menolak pesanan | `GET /rad-orders`; endpoint transisi | `S1` |
| Rincian pesanan | Melihat satu pesanan beserta study-nya | `GET /rad-orders/{id}`, `GET /rad-studies/by-order/{id}` | `S1`, `S2` |

### 3.3 Untuk radiografer

| Layar | Kegunaan | Data yang dikonsumsi | Slice |
|---|---|---|---|
| **Daftar kerja per alat** | Melihat pekerjaan hari itu pada alat tempat ia bertugas | `GET /rad-orders/worklist?modalityId=...` | `S12` |
| Verifikasi pasien | Memastikan identitas sebelum pemeriksaan | `POST /rad-studies/{id}/verify-patient` | `S2` |
| Isian gerbang keselamatan | Menjawab butir wajib sebelum foto | `POST /rad-studies/{id}/safety-checks`, `POST /{id}/clear-safety` | `S3` |
| Pengambilan citra | Mulai, selesai, atau hentikan acquisition | `POST /{id}/start-acquisition` dan seterusnya | `S2` |
| Penilaian mutu citra | Menyatakan citra layak atau harus diulang | `POST /{id}/decide-quality` | `S6` |
| Pencatatan bahan terpakai | Mencatat kontras, film, BHP | `POST /{id}/consumptions` | `S7` |

### 3.4 Untuk dokter radiolog

| Layar | Kegunaan | Data yang dikonsumsi | Slice |
|---|---|---|---|
| Daftar bacaan menunggu | Melihat study layak yang belum dibaca | `GET /rad-reports?status=Pending` | `S9` |
| Tulis dan ubah draf bacaan | Menulis temuan, kesimpulan, saran | `POST /by-study/{id}/draft`, `PUT /{id}/draft` | `S9` |
| Sahkan dan rilis bacaan | Mengesahkan lalu merilis | `POST /{id}/validate`, `POST /{id}/release` | `S9` |
| Koreksi bacaan yang sudah dirilis | Menulis amandemen berversi | `POST /{id}/amendments` | `S10` |

### 3.5 Untuk admin Radiologi dan penanggung jawab klinis

| Layar | Kegunaan | Data yang dikonsumsi | Slice |
|---|---|---|---|
| Kelola alat pencitraan | Menambah, mengubah, menonaktifkan alat | CRUD `master-data/rad-modalities` | `S13` |
| Kelola butir keselamatan | Menambah, mengubah, menonaktifkan butir | CRUD `master-data/rad-safety-requirements` | `S4` |
| Kelola aturan keselamatan | Menyusun draf, mengajukan, mengesahkan, menolak | `master-data/rad-safety-rules` beserta transisinya | `S4` |
| **Papan kesiapan alat** | Melihat alat mana yang belum punya aturan aktif | `GET /master-data/rad-safety-rules/coverage` | `S4` |

---

## 4. Aksi per Peran

| Aksi | Dokter pengirim | Petugas pendaftaran | Radiografer | Radiolog | Admin Radiologi | Penanggung jawab klinis |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| Buat pesanan | Ya | — | — | — | — | — |
| Terima, jadwalkan, tolak pesanan | — | Ya | — | — | — | — |
| Batalkan pesanan langsung | Hanya sampai `Requested` | Ya | — | — | — | — |
| Verifikasi pasien | — | — | Ya | — | — | — |
| Jawab butir keselamatan | — | — | Ya | — | — | — |
| Ambil citra | — | — | Ya | — | — | — |
| Nilai mutu citra | — | — | Ya | Ya | — | — |
| Catat bahan terpakai | — | — | Ya | — | — | — |
| Tulis draf bacaan | — | — | Ya | Ya | — | — |
| **Sahkan bacaan** | — | — | **Tidak** | **Ya** | — | — |
| Rilis bacaan | — | — | — | Ya | — | — |
| Lihat hasil bacaan | Ya | Ya | Ya | Ya | — | — |
| Kelola alat dan butir | — | — | — | — | Ya | — |
| Susun draf aturan keselamatan | — | — | — | — | Ya | — |
| **Sahkan aturan keselamatan** | — | — | — | — | **Tidak** | **Ya** |

Peran di atas adalah **sebutan bisnis**. Pemetaannya ke peran Quilvian masih `DEC-RAD-004`.

---

## 5. Yang Mengikat dan Tidak Boleh Diserahkan ke Selera

Berikut menyangkut keselamatan, privasi, atau invariant. **Bukan** `DEV_DISCRETION`.

| No | Ketentuan | Alasan |
|---:|---|---|
| 1 | Tombol Sahkan **wajib disembunyikan atau dinonaktifkan** ketika pengguna adalah penulis draf yang bukan radiolog | Mencegah pengguna mencoba tindakan yang pasti ditolak `403`. Backend tetap memeriksa; frontend hanya membantu |
| 2 | Bacaan yang **belum dirilis tidak boleh tampil** di layar dokter pengirim | Draf belum sah dan dapat berubah. Menampilkannya berisiko dipakai mengambil keputusan pengobatan |
| 3 | Versi bacaan yang tampil **wajib versi berlaku**, bukan versi tersimpan di cache | `RAD-DEC-006`. Koreksi harus langsung terlihat |
| 4 | Bila modul Radiologi gagal dihubungi, layar **wajib menampilkan pesan gangguan**, bukan daftar kosong | Daftar kosong terbaca "pasien tidak punya pemeriksaan" — kesimpulan salah yang berbahaya |
| 5 | Layar aturan keselamatan **wajib menampilkan peringatan** untuk setiap alat tanpa aturan aktif | Tanpa peringatan, ketiadaan aturan baru diketahui saat pasien sudah di depan alat |
| 6 | Isi bacaan **tidak boleh** disimpan di penyimpanan browser | Data medis. Cukup di memori selama halaman terbuka |
| 7 | Penanda "pemeriksaan ini pengulangan" **wajib terlihat** di daftar study | Petugas perlu tahu pasien sudah pernah disinari sebelumnya |
| 8 | Alasan koreksi **wajib ditampilkan** bersama versi bacaan | Pembaca harus tahu mengapa hasilnya berubah |
| 9 | Penanda cito **wajib terlihat** tanpa membuka rincian, dan pesanan cito **wajib di urutan atas** daftar kerja | Cito yang tenggelam di tengah daftar sama saja dengan tidak ditandai |

---

## 6. Yang Berstatus `DEV_DISCRETION`

Seluruh butir berikut diserahkan kepada developer, dengan syarat mengikuti pola Laboratorium
dan design system project.

| Butir | Catatan |
|---|---|
| Nama dan urutan menu sidebar | Kunci `menuRadiologi` sudah tersedia di sidebar |
| Route persisnya | Ikuti pola `src/app/health-services/laboratory-management/` |
| Tab, modal, atau drawer untuk isian | Ikuti pola layar terdekat |
| Tata letak formulir bacaan | — |
| Warna, ikon, tipografi | Ikuti design token project |
| Penempatan tombol aksi | — |
| Bentuk tabel dan kolom yang ditampilkan | — |
| Cara menampilkan riwayat versi | Boleh berupa daftar, boleh berupa perbandingan berdampingan |

**Agent tidak menetapkan satu pun butir di atas.** Menetapkannya tanpa brief yang disetujui
melanggar hierarki kewenangan pada bagian 1.

---

## 7. Penanganan Keadaan Layar

Setiap layar wajib menangani lima keadaan, mengikuti pola panel keadaan Laboratorium.

| Keadaan | Yang ditampilkan | Catatan khusus Radiologi |
|---|---|---|
| Sedang memuat | Indikator memuat | — |
| Berhasil, ada data | Datanya | — |
| Berhasil, kosong | "Belum ada data" beserta ajakan tindakan | **Kecuali** untuk hasil bacaan — lihat baris berikut |
| Gagal dihubungi | Pesan gangguan beserta tombol coba lagi | **Wajib** untuk hasil bacaan; dilarang menampilkan keadaan kosong |
| Ditolak hak akses | "Anda tidak punya hak akses untuk melihat ini" | — |

### Perbedaan kosong dan gagal pada hasil bacaan

Ini kekhususan modul Radiologi yang paling mudah salah diterapkan.

| Keadaan | Yang benar ditampilkan |
|---|---|
| Permintaan berhasil, tidak ada bacaan | "Belum ada hasil bacaan untuk kunjungan ini." |
| Permintaan **gagal** | "Hasil radiologi sedang tidak dapat ditampilkan. Coba lagi beberapa saat." |

Menyamakan keduanya berarti dokter dapat menyimpulkan pasien tidak punya pemeriksaan padahal
sebenarnya sistemnya sedang bermasalah.

---

## 8. Cache dan Pembaruan Data

| Data | Boleh disimpan sementara? | Kapan wajib diambil ulang |
|---|:---:|---|
| Daftar alat pencitraan | Ya | Setelah data induk diubah |
| Daftar butir keselamatan | Ya | Setelah data induk diubah |
| Daftar pesanan | Ya, singkat | Setelah setiap perubahan status |
| Study dan jawaban keselamatan | **Tidak** | Selalu ambil segar |
| **Hasil bacaan** | **Tidak** | **Selalu ambil segar** — `RAD-DEC-006` |

> **Mengapa hasil bacaan tidak boleh disimpan sementara.** Koreksi berversi dapat terjadi kapan
> saja. Data tersimpan yang tidak diperbarui akan menampilkan versi yang sudah diralat, dan
> itulah persis bahaya yang dicegah `RAD-DEC-006`.

---

## 9. Pencegahan Kiriman Ganda

| Layar | Risiko | Penanganan |
|---|---|---|
| Buat pesanan | Tombol Simpan ditekan dua kali sehingga pesanan tercatat dobel | Tombol dinonaktifkan selama permintaan berjalan |
| Sahkan bacaan | Dua radiolog mengesahkan draf yang sama bersamaan | Backend menolak yang kedua dengan `409`; layar menampilkan "Data ini baru saja diubah petugas lain. Muat ulang halaman lalu ulangi tindakan Anda." |
| Nilai mutu citra | Penilaian ganda menerbitkan fakta tagih dobel | Backend sudah menjaga lewat penanda; layar cukup menonaktifkan tombol |

---

## 10. Aksesibilitas dan Tampilan Responsif

| Aspek | Ketentuan |
|---|---|
| Isian gerbang keselamatan | Wajib dapat dioperasikan dengan papan ketik. Radiografer sering memakai sarung tangan |
| Ukuran sasaran sentuh | Ikuti design token project. Layar radiologi sering diakses lewat tablet di samping alat |
| Kontras teks | Ikuti design token project |
| Perilaku responsif | Ikuti pola halaman Laboratorium |

---

## 11. Ketergantungan Test

| Yang diuji | Jenis | Catatan |
|---|---|---|
| Tombol Sahkan tersembunyi bagi penulis bukan-radiolog | Unit component | Membuktikan butir 1 bagian 5 |
| Hasil bacaan gagal dimuat menampilkan pesan gangguan, bukan daftar kosong | Unit component | Membuktikan butir 4 bagian 5 |
| Peringatan alat tanpa aturan aktif tampil | Unit component | Membuktikan butir 5 bagian 5 |
| Alur dokter memesan sampai membaca hasil | End-to-end | Dijalankan hanya bila environment mendukung |

Kewajiban test otomatis mengikuti `rules/frontend/test-policy.md`.

---

## 12. Yang Sengaja Tidak Dibuat

| Yang ditolak | Alasan |
|---|---|
| Layar temuan kritis dan kotak pemberitahuannya | `S11` tertahan `DEC-RAD-002` |
| Daftar pantau keterlambatan pesanan cito | Ditunda `RAD-DEC-013`; lihat `RAD-OPEN-009` |
| Layar penugasan petugas ke pemeriksaan | Ditolak `RAD-DEC-012` — daftar kerja dikelompokkan per alat, bukan per orang |
| Layar pelewatan gerbang keselamatan darurat | `S5` tertahan `DEC-RAD-001` |
| Penampil citra | PACS dan DICOM di luar scope |
| Base component baru | Pakai ulang yang sudah ada; larangan `AGENTS.md` frontend |
| Hook atau view generik lintas modul | Larangan `AGENTS.md` frontend |
