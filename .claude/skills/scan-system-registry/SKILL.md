---
name: scan-system-registry
description: Pindai seluruh sistem Quilvian menjadi registry keadaan nyata sebelum modul apa pun dibahas. Gunakan untuk mendata area, modul, entity, tingkat kesiapan, kepemilikan data, kavling nama, grup endpoint, dan zona konflik antar modul lintas backend dan frontend. Wajib dijalankan sebelum /grill-me. Jangan gunakan untuk audit satu modul secara mendalam, mengusulkan entity baru, atau mengubah kode.
---

# Scan System Registry

Petakan keadaan sistem apa adanya sebelum siapa pun mengambil keputusan. Laporkan fakta, jangan
mengusulkan apa pun.

## Effort dan model minimum

| Mode | Minimum effort | Model Claude minimum | Model Claude disarankan | Model GPT setara |
| --- | --- | --- | --- | --- |
| `full` | `high` | Claude Sonnet 5 | Claude Opus 5 | GPT-5, reasoning `high` |
| `refresh` | `medium` | Claude Sonnet 5 | Claude Opus 5 | GPT-5, reasoning `medium` |

Alasan: pemindaian penuh menyisir ratusan entity di dua repository dan harus membedakan entity
yang benar-benar siap dari entity yang baru berupa kelas. Effort rendah menghasilkan registry
yang tampak lengkap padahal hanya membaca nama berkas.

Jika sesi berjalan di bawah batas ini, beritahu pengguna sebelum mulai dan minta konfirmasi.

## Aturan yang mengikat skill ini

| Aturan | Isi |
| --- | --- |
| [aturan pra-scan](../../rules/rule-prascan/aturan-prascan-modul.md) | Gerbang wajib, batas kewenangan, dan status kesegaran |
| [format registry](../../rules/rule-prascan/format-registry-sistem.md) | Bentuk baku tujuh berkas keluaran dan legenda status `L0`–`L4` |
| [aturan output dokumentasi](../../rules/rule-output/aturan-output-dokumentasi.md) | Bahasa Indonesia, mudah dipahami, detail bercontoh, proses bisnis jelas, endpoint bergaya Swagger |

Baca ketiganya sebelum menulis berkas apa pun. Format registry adalah kontrak keluaran; jangan
mengarang struktur sendiri.

## Batas kewenangan

Skill ini **read-only** terhadap source aplikasi. Larangan yang tidak boleh dilanggar:

1. Jangan mengubah source, migration, konfigurasi, atau berkas apa pun di luar
   `docs/system-registry/`.
2. Jangan menjalankan perintah git selain `status`, `log`, `diff`, `show`, `blame`, dan
   `rev-parse`.
3. Jangan mengusulkan entity baru, prioritas, sprint, atau urutan implementasi. Registry hanya
   memuat keadaan sekarang.
4. Jangan menebak pemilik modul. Yang tidak jelas ditulis `Belum ditentukan` dan masuk zona
   konflik.
5. Jangan menyimpulkan migration sudah diterapkan ke database hanya karena berkasnya ada.

## Tentukan mode

| Mode | Kapan dipakai | Cakupan |
| --- | --- | --- |
| `full` | Registry belum pernah dibuat, scan penuh terakhir lebih dari 30 hari, atau ada folder area/modul baru | Seluruh sistem |
| `refresh` | Registry sudah ada, hanya SHA yang berubah | Hanya berkas yang berubah pada `git diff --name-only <sha-registry>..HEAD` |
| `focus <area>` | Pengguna hanya ingin satu area diperbarui | Satu area, manifest tetap ditandai sebagian |

Jika mode tidak disebutkan, tentukan sendiri dari keadaan manifest lalu jelaskan pilihannya
dalam satu kalimat sebelum mulai.

## Siapkan pemindaian

1. Temukan Git root `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev`. Jangan
   menganggap direktori kerja saat ini selalu salah satunya.
2. Catat commit SHA kedua repository dengan `git rev-parse --short HEAD` **sebelum** menyisir.
   SHA ini yang dipakai pada seluruh bukti.
3. Baca `docs/system-registry/registry-manifest.md` bila ada, untuk menentukan mode.
4. Nyatakan batas yang tidak diperiksa, misalnya database runtime, service eksternal, atau
   environment produksi.

## Sisir backend

Kerjakan berurutan agar hasilnya dapat diperiksa ulang:

1. **Daftar `DbSet`.** Baca `Repositories/ApplicationDbContext.cs`. Setiap `public DbSet<T>`
   menjadi satu baris kandidat entity. Ini menentukan kolom Model.
2. **Configuration.** Cari `IEntityTypeConfiguration<T>` di `Repositories/`. Cocokkan per
   entity, bukan per jumlah berkas. Selisih jumlah wajib dijelaskan.
3. **Migration.** Telusuri `Migrations/` untuk mencari migration yang benar-benar membuat atau
   mengubah tabel entity tersebut. Nama berkas migration saja tidak cukup; periksa isinya.
4. **Controller dan service.** Cari pemakaian entity di `Areas/**/Controllers/` dan
   `Areas/**/Services/`. Catat nilai `[Route(...)]` dan `[Tags(...)]`.
5. **Enum dan konstanta.** Catat enum yang bermakna sama tetapi didefinisikan di dua area.
6. **Struktur folder.** Catat penyimpangan penamaan folder, misalnya `Controller` tunggal atau
   `DTOS` huruf besar.

Gunakan pencarian teks lebih dulu untuk menemukan kandidat, lalu baca berkas yang relevan.
Jangan menilai hanya dari hasil pencocokan nama.

## Sisir frontend

Untuk menentukan kolom Consumer:

1. Cari service atau API client yang memanggil base URL backend.
2. Cari route dan menu yang benar-benar dapat dicapai pengguna.
3. Bedakan pemakaian nyata dari kode mati, mock, atau dummy. Kode mati bukan consumer.

Entity yang tidak dipanggil frontend belum tentu salah; banyak entity memang dipakai antar
modul backend. Catat pemakaian antar modul backend sebagai consumer yang sah.

## Tentukan tingkat kesiapan

Berikan tepat satu tingkat per entity sesuai legenda pada format registry:

| Tingkat | Diberikan bila |
| --- | --- |
| `L1 Terdaftar` | Model dan `DbSet` ada |
| `L2 Berskema` | `L1` ditambah configuration dan migration yang terbukti |
| `L3 Berlayanan` | `L2` ditambah controller atau service yang memakainya |
| `L4 Terpakai` | `L3` ditambah consumer nyata di frontend atau modul backend lain |
| `⚠ Bermasalah` | Ada lapisan yang melompat, misalnya ada controller tanpa migration |

Tingkat tidak boleh dinaikkan karena "seharusnya ada". Bila sebuah lapisan tidak dapat
diperiksa, tulis `?` pada kolomnya dan turunkan tingkatnya, bukan menaikkannya.

## Tentukan kepemilikan data

1. Tetapkan modul pemilik dari lokasi entity dan modul yang benar-benar menulis ke sana.
2. Untuk data yang dipakai lintas modul, isi berkas kepemilikan data bersama beserta daftar
   nama yang dilarang dibuat ulang.
3. Entity yang ditulis lebih dari satu modul tanpa kesepakatan masuk zona konflik `KF-3`.

Kepemilikan data adalah alat utama pencegah konflik. Kerjakan bagian ini dengan teliti,
walaupun ia berbentuk tabel pendek.

## Kumpulkan zona konflik

Cari tujuh jenis konflik pada format registry: nama kembar, duplikasi konsep, entity tanpa
pemilik, skema tidak lengkap, alamat endpoint bentrok, enum ganda, dan prefix tidak sesuai.

Untuk setiap temuan, tulis risiko nyatanya bagi pengguna atau data, bukan istilah teknis.

> **Contoh benar:** "Dua modul menghitung penjamin dengan cara berbeda, sehingga tagihan bisa
> berbeda untuk pasien yang sama."
>
> **Contoh belum memenuhi aturan:** "Terjadi inkonsistensi data penjamin."

Temuan lama tidak boleh dihapus. Temuan yang sudah ditutup tetap tinggal beserta bukti
penutupnya.

## Tulis keluaran

Tulis tujuh berkas ke `NewQuilvianSystemBackend/docs/system-registry/` sesuai format registry:

```text
registry-manifest.md
01-peta-area-dan-modul.md
02-entity-terdaftar.md
03-kepemilikan-data-bersama.md
04-kavling-nama-dan-endpoint.md
05-zona-konflik.md
06-indeks-entity.md
```

Berkas yang tidak relevan tetap dibuat, berisi satu baris alasan. Jangan menghapus berkas tanpa
jejak dan jangan membuat berkas kosong tanpa keterangan.

Pada mode `refresh`, perbarui hanya bagian yang berubah, lalu perbarui SHA dan tanggal pada
manifest. Tandai bagian yang tidak diperiksa ulang agar pembaca tahu batas kesegarannya.

## Sajikan ringkasan kepada pengguna

Setelah berkas tertulis, tampilkan ringkasan singkat di layar:

1. jumlah area, modul, entity, dan sebaran tingkat kesiapan;
2. lima zona konflik paling berisiko beserta akibat nyatanya;
3. daftar data bersama yang paling sering salah diduplikasi;
4. bagian yang tidak dapat diperiksa beserta alasannya.

Jangan menampilkan seluruh isi registry di layar. Registry dibaca dari berkasnya.

## Tawarkan skill berikutnya

Setelah registry tuntas, **selalu** tawarkan langkah berikutnya secara eksplisit, lengkap
dengan alasan singkatnya:

| Kondisi setelah pemindaian | Skill yang ditawarkan |
| --- | --- |
| Registry `SEGAR` dan pengguna sudah punya modul yang ingin dibahas | `/grill-me` Scope Pass, dibuka dengan Kartu Konteks Pra-Wawancara |
| Ada zona konflik yang memblokir lebih dari satu modul | `/grill-me` khusus untuk menutup konflik tersebut lebih dulu |
| Registry sudah ada dan modul sedang berjalan | `/trace-existing-capabilities` untuk memperdalam modul tertentu |
| Pemindaian tidak lengkap karena batas akses | Sebutkan batasnya, jangan tawarkan langkah maju seolah registry sudah utuh |

Tawarkan, jangan jalankan sendiri. Tunggu persetujuan pengguna sebelum berpindah skill.
