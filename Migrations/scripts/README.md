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
| `verify-operating-room-schema.sql` | — | Pemeriksaan skema setelah migration diterapkan; hanya membaca |
| `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix.sql` | `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix` | Menormalkan empat tabel Rekam Medis dari `Trx*` ke prefix registry `Mrc*`, beserta primary key, 15 index, dan 6 foreign key-nya. **Seluruhnya `RENAME`** — tidak ada `DROP`, `DELETE`, maupun `TRUNCATE`, sehingga isi tabel tetap utuh |
| `verify-medical-record-mrc-rename.sql` | — | Pemeriksaan setelah rename Rekam Medis diterapkan; hanya membaca |
| `20260904030116_SplitLabSpecimenIntoExamination.sql` | `20260904030116_SplitLabSpecimenIntoExamination` | Melepas `ProcedureId` dan lima kolom salinan tarif dari `LabSpecimen` beserta index dan foreign key-nya, setelah keenamnya pindah ke `LabExamination`. **Diawali penjaga**: bila tabel `LabSpecimen` masih memuat baris, skrip berhenti dengan pesan `LAB-OPEN-012` dan tidak satu kolom pun dihapus |
| `20260904035620_AddLabExaminationIdToLabTransitionHistory.sql` | `20260904035620_AddLabExaminationIdToLabTransitionHistory` | Menambah kolom `LabExaminationId` beserta index dan foreign key-nya pada `LabTransitionHistory`, supaya riwayat dapat menunjuk pemeriksaan yang berpindah dan bukan hanya wadahnya. Aditif; tidak ada kolom yang dihapus maupun diubah |
| `20260904065309_AddLabDisciplineAndReferralMasterData.sql` | `20260904065309_AddLabDisciplineAndReferralMasterData` | Menambah kolom `LabDiscipline` pada `MstProcedure` beserta index bersyaratnya, dan membuat dua tabel data induk perujuk `MstReferralInstitution` serta `MstReferralDoctor`. Aditif; tidak ada kolom maupun tabel yang dihapus |
| `20260904072427_AddReferralPointerToPatientEncounter.sql` | `20260904072427_AddReferralPointerToPatientEncounter` | Menambah `ReferralInstitutionId` dan `ReferralDoctorId` pada `TrxPatientEncounter` beserta foreign key dan index bersyaratnya. Aditif dan boleh kosong, sehingga kunjungan yang sudah ada tidak terpengaruh |
| `rollback-20260904030116_SplitLabSpecimenIntoExamination.sql` | `20260904030116_SplitLabSpecimenIntoExamination` | Langkah mundurnya: keenam kolom, index, dan foreign key-nya dikembalikan. **Hanya aman pada tabel kosong** — `ProcedureId` dikembalikan sebagai kolom wajib berisi GUID nol, sehingga foreign key-nya gagal terbentuk bila sudah ada baris wadah |

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
