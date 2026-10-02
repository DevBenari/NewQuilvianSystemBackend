# Bukti — Uji langkah mundur migration `AddEmergencyDoctorAssignment` (`BE-IGD-044` acceptance 5)

| Field | Nilai |
| --- | --- |
| Tanggal | 21 September 2026 |
| Task | `BE-IGD-044` acceptance 5 — *"`Down()` dijalankan di basis data terpisah dan hasilnya dicatat"* |
| Migration yang diuji | `20260917072515_AddEmergencyDoctorAssignment` — `Down()` bawaan EF berupa `DropTable("EmgDoctorAssignment")` |
| Wewenang | Pemilik (Rizki) memerintahkan uji ini pada 21 September 2026 ("uji Down migration asli"), dengan larangan **tidak menginstal apa pun** |
| Source | Backend `267b56a0` (branch `rizkiG`), DLL hasil build pemilik 21 September 2026 |
| Klasifikasi bukti | Uji migration pada **salinan basis data terpisah**. Bukan UAT, bukan uji HTTP, bukan uji layar |

## 1. Apa yang diuji, dengan bahasa sederhana

Bila migration ini suatu hari harus dibatalkan, tabel `EmgDoctorAssignment` harus **hilang seluruhnya
dan tidak ada hal lain yang ikut rusak**. Bila migration dijalankan lagi sesudahnya, tabel itu harus
kembali **persis sama** — kolom, index, dan foreign key-nya.

*Contoh.* Pemilik membatalkan fitur riwayat dokter di sebuah rumah sakit. Setelah rollback, tabel
kunjungan IGD, dokter, dan pengguna harus tetap utuh; hanya tabel riwayat dokter yang lenyap.

## 2. Alat dan batas

| Butir | Isi |
| --- | --- |
| Basis data | Kontainer PostgreSQL 16.15 lokal dari image `postgres:16` yang **sudah ada** di mesin (`--pull never`). Nol unduhan, nol instalasi. Dev (PostgreSQL 15.15) tidak ditulis; selisih versi mayor 16 lawan 15 dicatat sebagai keterbatasan |
| Cara menerapkan | `dotnet ef database update --connection <lokal> --no-build`, memakai DLL hasil build pemilik |
| Pengukuran | Skrip SQL baca-saja berisi sidik jari (`md5`) seluruh kolom, index, dan constraint **selain** tabel yang diuji, ditambah sidik jari tabel itu sendiri |
| Pembersihan | Kontainer dan volumenya dihapus (`docker rm -f -v`), Docker Desktop dihentikan kembali seperti keadaan awal |

## 3. Hasil

Rantai penuh **189 migration** diterapkan ke basis data kosong tanpa galat (`Done.`, exit `0`, 628
tabel `public`).

| Pengukuran | B0 — sebelum (head `20260921032943`) | B1 — sesudah `Down` ke `20260915074405` | B2 — sesudah `Up` kembali ke head |
| --- | --- | --- | --- |
| Jumlah tabel `public` | 628 | **627** | 628 |
| Tabel `EmgDoctorAssignment` ada | ya | **tidak** | ya |
| Baris `__EFMigrationsHistory` | 189 | **187** | 189 |
| Migration terakhir | `20260921032943_BackfillEmergencyDoctorAssignment` | `20260915074405_RevisiTablePettyCash` | `20260921032943_BackfillEmergencyDoctorAssignment` |
| FK dari tabel lain yang menunjuk ke tabel ini | 0 | 0 | 0 |
| Sidik jari kolom tabel lain | `c68ab6ba…` | **sama** | **sama** |
| Sidik jari index tabel lain | `0009400d…` | **sama** | **sama** |
| Sidik jari constraint tabel lain | `5c37a58e…` | **sama** | **sama** |
| Sidik jari definisi `EmgDoctorAssignment` | `b5cc176b…` | (tabel tidak ada) | **`b5cc176b…` — identik dengan B0** |

Perbandingan berkas B0 dan B2 dengan `diff` menghasilkan **identik**.

Yang dibuktikan:

1. `Down` dari head melewati **dua** migration berurutan — `Down()` `BE-IGD-048` lalu `Down()` asli
   `AddEmergencyDoctorAssignment` — dan selesai tanpa galat.
2. Selisihnya **tepat satu tabel** (628 → 627). Kolom, index, dan constraint semua tabel lain tidak
   berubah satu pun.
3. Tidak ada foreign key dari tabel lain ke `EmgDoctorAssignment`, jadi `DropTable` tidak dapat
   menjatuhkan objek modul lain.
4. `Up` kembali membuat tabel dengan definisi **identik** (kolom, tipe, nullability, default, enam
   index termasuk unique bersyarat `IX_EmgDoctorAssignment_EmergencyVisitId_Active`, dan ketiga FK).

## 4. Yang **tidak** dicakup

| Hal | Keterangan |
| --- | --- |
| Tabel berisi data saat `Down` | Tabel pada uji ini kosong (basis data baru). `DropTable` memang menghapus isi tabel — itu sifat rollback yang disengaja. Perlindungan baris legacy yang sudah dipakai ada pada `Down()` `BE-IGD-048` dan sudah terbukti pada `BE-IGD-048` bagian 5.1 (guard A dan C) |
| Versi mesin | Uji memakai PostgreSQL 16.15; dev memakai 15.15. `DropTable`/`CreateTable` standar, tetapi selisih itu dicatat |
| Migration `20260826090500` | Bukan bagian uji ini. Milik `BE-IGD-026` dan `BE-IGD-031`; berada di belakang sekitar 75 migration modul lain, sehingga butuh rancangan uji terpisah |

## 5. Dampak status

`BE-IGD-044` acceptance 5 **terpenuhi**. Bersama acceptance 1–3 (source), acceptance 4 (dikerjakan
`BE-IGD-048`, terbukti pada salinan berkandidat), dan acceptance 6 (snapshot +102 baris tanpa
penghapusan), seluruh enam acceptance terpenuhi. **UAT belum dan tidak diklaim.**
