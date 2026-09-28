# Engineering Decision Log

## DEC-BUILD-001
Date:
2026-09-04

Decision:
Exclude EF generated migration code from Web compilation.

Reason:
Cold build exceeded 40 minutes.

Impact:
Build reduced to 18 seconds.

Owner:
Backend Team

Status:
DICABUT pada hari yang sama. Catatan "Impact" di atas hanya mengukur waktu build dan
tidak menangkap akibatnya. Atribut [Migration("...")] berada di dalam berkas Designer,
sehingga mengeluarkan Designer dari kompilasi membuat EF tidak lagi mengenali migration:
`migrations list` hanya melihat 7 dari 131 migration, dan `migrations add` menghasilkan
migration berisi 570 CreateTable — seluruh skema dari nol. Lihat DEC-BUILD-002 untuk
pendekatan yang mempertahankan penghematannya tanpa akibat itu.


## DEC-BUILD-002
Date:
2026-09-21

Decision:
Arsipkan berkas Designer migration yang tidak lagi dipakai ke Migrations\History\ dan
pindahkan atribut [DbContext] serta [Migration] miliknya ke Migrations\MigrationMetadata.g.cs
yang tetap dikompilasi.

Reason:
Build gagal dengan System.OutOfMemoryException di dalam Roslyn LocalRewriter setelah
jumlah migration mencapai 204. Sebabnya bukan kode aplikasi: seluruh source aplikasi
berukuran 26,7 MB, sedangkan Migrations\ berukuran 554 MB / 13,7 juta baris. Tiap
Designer memuat BuildTargetModel, yaitu snapshot penuh seluruh skema, sekitar 4,5 MB
per berkas, diulang 204 kali. Roslyn melakukan lowering atas method-method raksasa itu
secara paralel sampai csc menembus 20 GB lalu mati.

-m:1 tidak menolong karena yang dibatasinya adalah paralelisme MSBuild, bukan
paralelisme internal compiler.

Impact:
Volume migration yang dikompilasi 554 MB -> 31,9 MB. Build Debug 364 detik lalu OOM
-> 58 detik, 0 error. Peringatan tetap 222, seluruhnya dari kode aplikasi; tidak ada
satu pun yang berasal dari berkas migration.

Build yang sama juga memunculkan satu error yang selama ini tertutup OOM: Program.cs
masih memanggil LabDummyDataSeeder, yang kelasnya dicabut 2026-09-17. Pemanggil mati
itu dihapus; kunci "Seeders:RunLabDummySeed" memang sudah tidak ada di appsettings mana
pun, sehingga blok tersebut tidak pernah berjalan.

Berbeda dari DEC-BUILD-001, atribut yang dibutuhkan EF TIDAK ikut hilang. Diverifikasi
dengan `dotnet ef migrations list --no-build --no-connect`: seluruh 220 migration tetap
terlihat (199 dari MigrationMetadata.g.cs + 5 Designer yang dipertahankan + 16 atribut
yang memang sudah ditulis tangan di berkas utama), tanpa ID ganda.

Designer milik migration bermuatan data
(InsertData/UpdateData/DeleteData) dan dua migration terbaru tetap dikompilasi utuh
karena TargetModel-nya masih dipakai. ApplicationDbContextModelSnapshot.cs selalu
dikompilasi.

Migration baru hasil `dotnet ef migrations add` lahir di Migrations\ dan otomatis ikut
dikompilasi, jadi tidak ada risiko ia tertinggal diam-diam. Pemangkasan lanjutan
dilakukan dengan tooling\migrations\Update-MigrationHistory.ps1. Target MSBuild
ValidateMigrationHistoryCoverage menggagalkan build bila arsip dan metadata berselisih.

Untuk mengompilasi seluruh Designer kembali: dotnet build -p:FullMigrationMetadata=true

Verification:
1. `dotnet ef migrations list --no-build --no-connect` -> 220 migration terlihat, tanpa ID ganda.
2. `dotnet ef migrations script --idempotent --no-build` -> exit 0; 4.012 KB, 78.022 baris,
   681 CREATE TABLE, 277 INSERT INTO. Ini membuktikan pembuatan SQL tetap benar untuk
   seluruh 220 migration, termasuk 199 yang Designer-nya diarsipkan. Diperiksa isinya:
   CREATE TABLE public."LabSpecimenType" lengkap dengan tipe kolomnya (berasal dari
   migration yang DIARSIPKAN), dan INSERT seed MstAdministrationFeePolicy dengan literal
   bertipe benar (TIMESTAMPTZ/FALSE/0.0) -- inilah alasan 3 migration bermuatan data
   tidak ikut diarsipkan.
3. `dotnet ef database update --no-build` terhadap QuilvianNewDevHamzah -> 18 migration
   pending diterapkan, 40 detik. Diverifikasi LANGSUNG ke katalog Postgres, bukan dari
   jawaban "Done." belaka: __EFMigrationsHistory 203 -> 221 baris, tabel LabSpecimenType
   ada dan terisi, LabOrder.OrderNumber ada + NOT NULL + 0 baris NULL, kolom
   MstTaxRule.TaxableCategory hilang. Sesudahnya: 0 pending, 0 pending model changes.

Known limitation:
`dotnet ef migrations remove` hanya aman satu kali berturut-turut. Perintah itu menulis
ulang snapshot dari TargetModel migration SEBELUMNYA; dengan 2 Designer terbaru yang
disimpan, remove kedua akan membacanya dari Designer yang diarsipkan dan menghasilkan
snapshot KOSONG tanpa peringatan. Menaikkan jumlah Designer yang disimpan menambah
N-1 kali remove yang aman.

Catatan terpisah: __EFMigrationsHistory memuat satu baris yatim,
20260610151122_addColumnMstKioskDevice, yang migration-nya tidak ada lagi di source.
EF mengabaikannya, tetapi ia menjelaskan selisih 203 baris riwayat vs 202 yang dikenali.

Owner:
Backend Team