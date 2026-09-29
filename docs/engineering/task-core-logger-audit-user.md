# Task Core/shared: kolom pengguna pada baris audit terisi id yang salah

Dicatat 29 September 2026. Pemilik: Core/shared platform, **bukan** modul Operasi.

Ditemukan saat menyusun uji audit `BE-OPR-011`. Sengaja tidak diperbaiki dari task Operasi:
`LoggerService` dipakai seluruh modul, dan mengubah cara ia menyusun baris log akan mengubah isi
log setiap modul sekaligus.

## Gejalanya

Baris audit modul Operasi memuat `UserId` yang berisi **id kasus operasi**, bukan id pengguna
yang melakukan tindakan.

## Sebabnya

`Services/Logging/LoggerService.cs`, pada `WriteAsync`:

```csharp
userId = GetValueFromData(data, "UserId", "Id") ?? userId;
```

Nilai diambil dari `UserId` lebih dulu, lalu dari `Id`. Data audit yang dikirim service Operasi
tidak memuat `UserId`; ia memuat `Id` — yaitu id kasus — dan `ActorUserId`. Akibatnya `Id` yang
terpakai.

Nama `Id` terlalu umum untuk dianggap menunjuk pengguna. Modul mana pun yang mengirim `Id` entitas
di dalam data auditnya akan mengalami hal yang sama, jadi ini bukan persoalan satu modul.

Dua hal lain yang ikut terlihat dan sebaiknya ditimbang bersamaan:

- Isi `data` tidak ikut tercetak pada baris log. Nomor kasus, hasil, dan revisi yang dikirim
  service tidak pernah muncul, sehingga baris audit tidak dapat ditelusuri ke objeknya.
- Klaim `ActorUserId` yang sudah disediakan pemanggil tidak pernah dibaca.

## Akibat sekarang

Pertanggungjawaban pelaku pada modul Operasi terbaca dari `OprStatusHistory.CreateBy`, bukan dari
log. Itu memang sumber yang lebih kuat, tetapi log audit menjadi menyesatkan bila dibaca sendiri:
kolomnya bernama pengguna dan isinya bukan pengguna.

Uji `AuditPrivacyTests` di `Tests/QuilvianSystemBackend.OperatingRoomTests` sengaja **tidak**
menuntut nama pelaku muncul di log, supaya ia tidak diam-diam mengesahkan keadaan ini. Ketika
task ini selesai, uji itu perlu diperketat.

## Usul perbaikan

1. Baca `ActorUserId` sebelum `Id` pada daftar nama yang dicari, atau hapus `Id` dari daftar itu
   karena terlalu umum.
2. Cetak isi `data` pada baris log, atau sekurangnya sertakan `CorrelationId`.
3. Periksa modul lain yang mengirim `Id` di dalam data auditnya, karena kemungkinan besar mereka
   terkena hal yang sama tanpa disadari.
