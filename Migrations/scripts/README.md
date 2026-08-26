# Skrip SQL Migration

Folder ini menyimpan skrip SQL yang dihasilkan dari migration Entity Framework, untuk
lingkungan yang menerapkan perubahan skema **secara manual** dan tidak menjalankan
`dotnet ef database update` langsung ke database.

Skrip di sini adalah hasil turunan, bukan sumber kebenaran. Sumber kebenaran tetap berkas
migration di `Migrations/`. Jika migration berubah, skripnya wajib dibuat ulang.

## Daftar skrip

| Berkas | Migration asal | Isi |
| --- | --- | --- |
| `20260821060256_AddOperatingRoomFoundation.sql` | `20260821060256_AddOperatingRoomFoundation` | Membuat 13 tabel modul Operasi beserta relasi dan index |

## Cara menjalankan

```bash
psql -h <host> -U <user> -d <database> -f 20260821060256_AddOperatingRoomFoundation.sql
```

Isi berkas juga dapat ditempel langsung ke pgAdmin atau DBeaver.

## Sifat skrip

Seluruh skrip dibuat dengan opsi `--idempotent`, sehingga aman dijalankan lebih dari satu kali.

**Contoh:** skrip `AddOperatingRoomFoundation` membungkus setiap perintah di dalam pemeriksaan
`IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation')`.
Pada eksekusi pertama, 13 tabel dibuat dan satu baris ditambahkan ke `__EFMigrationsHistory`.
Pada eksekusi kedua, pemeriksaan tersebut bernilai salah sehingga tidak ada satu pun perintah
yang dijalankan. Database tidak berubah dan tidak ada pesan kesalahan.

Seluruh perintah juga dibungkus satu transaksi (`START TRANSACTION` sampai `COMMIT`). Bila ada
satu perintah gagal, seluruh perubahan dibatalkan sehingga database tidak tertinggal setengah
jadi.

## Cara membuat ulang

Ganti `<migration-sebelumnya>` dan `<migration-tujuan>` sesuai kebutuhan:

```bash
dotnet ef migrations script <migration-sebelumnya> <migration-tujuan> \
  --idempotent --no-build \
  -o Migrations/scripts/<migration-tujuan>.sql
```

Contoh yang dipakai untuk berkas pada tabel di atas:

```bash
dotnet ef migrations script 20260818084734_AddTriageSlaBreachMarker \
  20260821060256_AddOperatingRoomFoundation \
  --idempotent --no-build \
  -o Migrations/scripts/20260821060256_AddOperatingRoomFoundation.sql
```

Rentang sengaja dibatasi dari migration sebelumnya ke migration tujuan, bukan dari awal
riwayat, supaya skrip tidak memuat migration lama yang sudah diterapkan.

## Yang perlu diperiksa setelah menjalankan

Pengujian otomatis modul Operasi berjalan di atas database dalam memori. Tiga hal berikut
**belum terbukti** di PostgreSQL sungguhan dan sebaiknya dicoba sekali secara nyata:

| Yang perlu dibuktikan | Alasan | Cara memeriksa |
| --- | --- | --- |
| Filtered unique index pada `OprSchedule` | Database dalam memori tidak menegakkan index bersyarat | Coba buat dua jadwal aktif untuk satu kasus; yang kedua harus ditolak |
| Kolom `jsonb` pada checklist dan recovery | Database dalam memori menyimpannya sebagai teks biasa | Simpan satu checklist, lalu baca kembali lewat `GET` persiapan |
| Concurrency token `Version` | Perilaku benturan versi berbeda antar provider | Ubah satu kasus dari dua sesi; yang kedua harus dijawab `OPR012` |
