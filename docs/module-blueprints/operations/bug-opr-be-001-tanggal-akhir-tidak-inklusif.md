# `BUG-OPR-BE-001` — Tanggal akhir laporan Operasi membuang seluruh data hari itu

Ditemukan 2 Oktober 2026 saat penambahan uji `BE-OPR-010`. **Belum diperbaiki**; task yang
menemukannya berfokus verifikasi.

| Hal | Isi |
|---|---|
| Prioritas usulan | **P1** — angkanya terbaca masuk akal tetapi keliru, dan keputusan penjadwalan diambil di atasnya |
| Pemilik | modul Operasi |
| Berkas | `Areas/HealthServices/OperatingRoomManagement/Services/OperatingRoomReportService.cs` |
| Terjaga uji | `Tests/QuilvianSystemBackend.OperatingRoomTests/ReportTests.cs` → `Tanggal_akhir_tanpa_jam_membuang_seluruh_data_hari_itu` |

## Masalah

Ketiga laporan Operasi memakai nilai `To` apa adanya:

```csharp
if (request.To.HasValue)
    query = query.Where(x => x.RequestedAt <= request.To.Value.ToUniversalTime());
```

| Baris | Laporan | Kolom |
|---|---|---|
| 21 | kasus operasi | `RequestedAt` |
| 81 | pemakaian ruang | rentang wajib |
| 152 | material/implant | `OccurredAt` |

Penyaring pada layar mengirim **tanggal tanpa jam**. Memperlakukannya apa adanya berarti pukul
00:00, sehingga laporan yang diminta "sampai hari ini" justru membuang seluruh kasus hari ini.

## Akibatnya

Laporan tidak melempar galat dan tidak terlihat rusak — ia mengembalikan angka yang lebih kecil
dan masuk akal. Pemakaian ruang hari terakhir hilang, jumlah kasus terbaca kurang, dan jejak
implant yang dicatat hari itu tidak muncul pada laporan traceability.

Cacat yang sama pernah terjadi pada laporan Gizi dan sudah ditutup di sana lewat penolong
`ToInclusive`, beserta catatannya:

> *"Penyaring layar mengirim tanggal tanpa jam. Memperlakukannya apa adanya berarti pukul 00:00,
> sehingga laporan yang diminta 'sampai hari ini' justru membuang seluruh data hari ini — persis
> yang terjadi saat penelusuran layar dan membuat ringkasan menyebut satu order padahal ada dua."*

Laporan Operasi belum punya padanannya.

## Acceptance ketika nanti diperbaiki

1. **Tanggal akhir tanpa jam mencakup seluruh harinya.** `To = 2 Oktober` memuat kasus pukul
   23:59 pada 2 Oktober.
2. **Tanggal akhir yang menyebut jam dipakai apa adanya.** `To = 2 Oktober 10:00` tidak memuat
   kasus pukul 10:01 — pemeriksaan jam dilakukan **sebelum** konversi ke UTC, karena tanggal lokal
   pukul 00:00 berubah menjadi 17:00 UTC dan tidak lagi dikenali sebagai "tanggal tanpa jam".
3. **Berlaku pada ketiga laporan**, bukan hanya laporan kasus.

Penolong `ToInclusive` pada `NutritionReportService` dapat dipakai sebagai acuan bentuknya.

## Gap terkait yang bukan bug

**Rentang terbalik tidak ditolak.** `From` lebih besar daripada `To` tidak divalidasi; laporan
mengembalikan hasil kosong. Itu tidak merusak data dan tidak menyesatkan sejauh pembacanya tahu
apa yang dimintanya, tetapi juga tidak memberi tahu bahwa penyaringnya mustahil. Perilakunya
dijaga uji `Rentang_terbalik_mengembalikan_kosong_bukan_melempar`; apakah ia perlu ditolak adalah
keputusan pemilik modul, bukan diputuskan dari task pengujian.

## Catatan

Uji yang menjaganya **sengaja dinamai sesuai perilaku sebenarnya**. Ia lulus hari ini dan akan
gagal begitu bug ini diperbaiki — kegagalan itulah penanda bahwa perbaikannya benar-benar
mengubah perilaku, dan ujinya harus diperbarui bersama perbaikan tersebut.
