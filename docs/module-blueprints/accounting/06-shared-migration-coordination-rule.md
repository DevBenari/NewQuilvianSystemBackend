# Shared EF Migration Coordination Rule — usulan `QBE-MIG-001`

| Field | Value |
|---|---|
| Klasifikasi artefak | **`SHARED_ENGINEERING_RULE`** |
| Status | **`PROPOSED`** — belum canonical, belum mengikat siapa pun |
| Usulan ID | `QBE-MIG-001` dan `QBE-MIG-002` |
| Rumah canonical yang dituju | `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` bagian *Aturan canonical* |
| Pemilik keputusan | Lead |
| Diusulkan oleh | Rizki (Accounting), 1 September 2026 |
| Dependency terkait | `ACC-DEP-005` |
| Berlaku untuk | Setiap modul yang berbagi `ApplicationDbContext`, bukan Accounting saja |

## Kenapa berkas ini ada di sini, bukan langsung di `docs/engineering/`

Aturan ini **bukan** milik Accounting. Ia mengikat setiap modul yang berbagi
`ApplicationDbContext` dan `ApplicationDbContextModelSnapshot` — Accounting, Finance, Billing,
Operating Room, dan seterusnya. Menaruhnya sebagai aturan lokal blueprint Accounting akan salah:
Finance tidak terikat oleh dokumen milik Accounting.

Rumahnya yang benar adalah `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`. Berkas itu
**hanya ada di branch `origin/QuilvianIntegrationBackend`**, tidak di `rizkiG`, dan pemiliknya
adalah lead. Menyalinnya ke branch ini akan menciptakan governance tandingan — persis kesalahan
yang sudah dihindari pada `ACC-DEP-002` untuk registry prefix.

Karena itu berkas ini berstatus **usulan**. Ia menyediakan teks siap pakai untuk didaftarkan
lead, dan sementara itu menjadi rujukan roadmap Accounting. Ia tidak mengikat Finance sampai
lead mengesahkannya.

## Teks aturan yang diusulkan

> | ID | Ketentuan | Kelayakan otomasi |
> |---|---|---|
> | `QBE-MIG-001` | MUST / NEW CODE: migration final yang berbagi `ApplicationDbContext` atau `ApplicationDbContextModelSnapshot` MUST diserialisasi terhadap canonical integration baseline. Dua modul MUST NOT menghasilkan migration final secara paralel dari baseline snapshot yang sama. | REVIEW_ONLY |
> | `QBE-MIG-002` | MUST / NEW CODE: sebelum `dotnet ef migrations add`, developer MUST mengambil migration predecessor, `ApplicationDbContextModelSnapshot` terbaru, dan canonical integration baseline terbaru, serta MUST mencatat SHA baseline yang menjadi sumber migration-nya. | PARTIALLY_AUTOMATABLE |

## Aturan predecessor — siapa pun boleh lebih dahulu

Aturan ini **tidak** menetapkan urutan tetap antar modul. Ia hanya melarang dua migration final
lahir dari baseline yang sama.

Modul yang migration-nya lebih dahulu valid dan masuk canonical integration baseline menjadi
**predecessor**. Modul berikutnya wajib mengikuti baseline itu.

**Bila Finance lebih dahulu:**

```
Finance Migration → review → integration baseline
        → Rizki pull/rebase → database update sesuai policy
        → Accounting Migration
```

**Bila Accounting lebih dahulu:**

```
Accounting Migration → review → integration baseline
        → Yasmin pull/rebase → database update sesuai policy
        → Finance Migration
```

Coding kedua modul tetap **boleh paralel**. Yang diserialisasi hanya migration finalnya.

### Yang dilarang

Accounting dan Finance menghasilkan migration final secara paralel dari
`ApplicationDbContextModelSnapshot` baseline yang sama. Akibatnya snapshot salah satunya menjadi
usang tanpa terlihat, dan migration berikutnya akan membawa operasi milik modul lain — persis
kerusakan yang dulu tercatat sebagai `ACC-DEP-001`.

### Yang bukan maksud aturan ini

Aturan ini **tidak** berlaku surut dan **tidak** membatalkan pekerjaan yang sudah berjalan.
Bila Finance sudah menghasilkan migration sebelum aturan ini disahkan, migration itu **tetap
sah** dan justru menjadi predecessor. Tidak ada rollback terhadap pekerjaan Yasmin hanya karena
aturan ini dibuat belakangan.

## Migration Coordination Gate

Gate ini dijalankan **sebelum** `dotnet ef migrations add`, oleh developer modul yang hendak
membuat migration. Tujuh pertanyaan, semuanya harus terjawab tertulis.

| # | Pertanyaan | Bukti yang dicatat |
|---|---|---|
| 1 | Apakah modul paralel sudah membuat migration? | Ya / Tidak, beserta cara memeriksanya |
| 2 | Bila sudah, apa nama migration-nya? | Nama berkas lengkap |
| 3 | Apakah migration itu sudah commit dan push? | SHA commit |
| 4 | Apakah sudah merge ke canonical integration baseline? | SHA baseline |
| 5 | Apakah sudah diterapkan ke shared development database? | Tanggal dan pelaksana |
| 6 | Apakah `ApplicationDbContextModelSnapshot` lokal berasal dari baseline terbaru? | Hasil pembandingan |
| 7 | SHA/commit baseline mana yang menjadi sumber migration ini? | SHA |

Gate **gagal** bila salah satu pertanyaan tidak terjawab. Migration tidak dibuat sampai gate
lulus.

> **Catatan penting.** Database pengembangan dipakai bersama satu tim. Pertanyaan 5 menyangkut
> data yang sedang dipakai orang lain, jadi jawabannya harus dikonfirmasi, bukan diasumsikan.

## Jalur adopsi

1. Lead meninjau usulan `QBE-MIG-001` dan `QBE-MIG-002`.
2. Bila disetujui, teksnya masuk ke `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` pada
   branch `origin/QuilvianIntegrationBackend`.
3. Berkas ini berubah status menjadi `ADOPTED` dan hanya menjadi penunjuk ke aturan canonical.
4. Roadmap Accounting dan roadmap Finance sama-sama merujuk ID canonical, bukan berkas ini.

Sampai langkah 2 selesai, `ACC-DEP-005` berstatus `MISSING` dan `BE-ACC-006` menjalankan gate di
atas berdasarkan usulan ini.
