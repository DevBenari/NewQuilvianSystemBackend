# Uji Modul Operasi

Bukti otomatis untuk acceptance criteria `BE-OPR-003` sampai `BE-OPR-011`.

## Menjalankan

```
dotnet test Tests\QuilvianSystemBackend.OperatingRoomTests\QuilvianSystemBackend.OperatingRoomTests.csproj -p:SkipMigrationMetadata=true
```

`-p:SkipMigrationMetadata=true` wajib. Tanpa itu build ikut mengompilasi seluruh berkas
`Migrations\**\*.Designer.cs` dan snapshot modelnya — sekitar 6,7 juta baris — dan waktunya
berubah dari sekitar 40 detik menjadi sekitar satu jam. Properti dengan nama sama di dalam
`.csproj` ini tidak cukup, karena properti proyek tidak menurun ke proyek yang direferensikan;
hanya `-p:` pada baris perintah yang berlaku global.

## Basis data

Setiap uji membuat basis data SQLite baru di dalam memori dan membentuk tabelnya dari
konfigurasi EF Core yang sama dengan yang dipakai aplikasi. Tidak ada uji yang menyentuh basis
data mana pun yang tercatat di `appsettings`, dan tidak ada berkas yang tertinggal di disk.

Data dasarnya dibuat dengan memanggil `OperatingRoomDemoSeeder` — seeder milik modul Operasi
sendiri. Akibatnya uji tidak pernah mengarang bentuk data yang berbeda dari yang dipakai
sungguhan, dan seeder itu ikut teruji.

## Isi

| Berkas | Yang dibuktikan |
|---|---|
| `PermissionMatrixTests` | Setiap endpoint Operasi menuntut login dan izin atas sumber daya Operasi; endpoint yang mengubah data tidak cukup dengan izin baca |
| `StateTransitionTests` | Setiap status dicapai lewat service sungguhan; transisi terlarang ditolak; `Completed` menunggu catatan final, recovery, dan serah terima |
| `ConcurrencyAndIdempotencyTests` | `OPR012` menolak versi basi; pengulangan kunci tidak menggandakan; `OPR013` menolak kunci yang dipakai untuk isi berbeda |
| `MaterialSerialTests` | `OPR014` beserta batasnya |
| `AuditPrivacyTests` | Riwayat status menyimpan pelakunya; jejak audit tidak memuat identitas pasien maupun isi klinis |
| `IntegrationOutboxTests` | Sisi Operasi dari penyerahan ke Inventory/Farmasi |
| `Infrastructure/` | Basis data uji, penyusun service, dan langkah alur yang dipakai bersama |

## Yang sengaja tidak diuji di sini

**Penolakan `403` per peran.** Jawabannya bergantung pada peran dan pemetaan izin milik
lingkungan. Menuliskannya di dalam uji berarti mengarang keadaan lalu menyatakannya terbukti.
Yang diuji adalah kontrak izin yang terpasang pada endpoint.

**Kontrak consumer Billing dan Inventory.** Belum disahkan pemiliknya. `IntegrationOutboxTests`
berhenti di batas modul: ia membuktikan rencana penyerahan tercatat sekali, bukan bentuk pesan
yang diterima modul lain. `BE-OPR-009` tetap Blocked.
