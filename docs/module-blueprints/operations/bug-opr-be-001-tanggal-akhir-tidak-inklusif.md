# `BUG-OPR-BE-001` — Tanggal akhir laporan Operasi membuang seluruh data hari itu

Ditemukan 2 Oktober 2026 saat penambahan uji `BE-OPR-010`. **Selesai** 2 Oktober 2026.

| Hal | Isi |
|---|---|
| Status | ✅ **Selesai** — diperbaiki beserta 10 uji regresi |
| Prioritas | **P1** — angkanya terbaca masuk akal tetapi keliru, dan keputusan penjadwalan diambil di atasnya |
| Pemilik | modul Operasi |
| Berkas | `Areas/HealthServices/OperatingRoomManagement/Services/OperatingRoomReportService.cs` |
| Terjaga uji | `Tests/QuilvianSystemBackend.OperatingRoomTests/ReportTests.cs` → `Tanggal_akhir_tanpa_jam_mencakup_seluruh_hari_itu` dan 9 uji lainnya |

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

## Perbaikan

Satu penolong bersama, `NormalkanRentang`, dipakai **ketiga** laporan supaya perilakunya tidak
berbeda antar layar. Di dalamnya `AkhirHariBila` memperluas tanggal akhir ke detik terakhir harinya
**hanya** bila yang dikirim memang tanggal tanpa jam.

Jam diperiksa **sebelum** konversi ke UTC. Memeriksanya sesudah konversi keliru: tanggal lokal
pukul 00:00 berubah menjadi 17:00 UTC, sehingga tidak pernah dikenali sebagai "tanggal tanpa jam"
dan laporan tetap membuang data hari terakhir.

Timestamp yang memang menyebut jam dihormati apa adanya — pengguna yang meminta "sampai pukul
10:00" tidak sedang meminta sampai tengah malam.

## Rentang terbalik kini ditolak

`From` yang melewati `To` melempar `ArgumentException`, dan ketiga endpoint laporan memetakannya
menjadi **400 Bad Request** mengikuti konvensi yang sudah dipakai `GetUtilization`. Daftar kosong
yang dulu dikembalikan berbohong: ia menyatakan laporannya memang tidak punya data, padahal
penyaringnya yang mustahil.

Pemeriksaannya dilakukan **sesudah** normalisasi, sehingga `From` dan `To` pada tanggal yang sama
tetap sah — satu hari penuh bukan rentang nol. Pemeriksaan lama pada `GetUtilizationAsync`
(`to <= from`) ikut diseragamkan menjadi `akhir < awal`, karena yang dilarang adalah rentang
terbalik, bukan rentang yang kebetulan berujung sama.

## Uji regresi

`Tests/QuilvianSystemBackend.OperatingRoomTests/ReportTests.cs`, 10 uji:

| Uji | Yang dijaga |
|---|---|
| `Tanggal_akhir_tanpa_jam_mencakup_seluruh_hari_itu` | acceptance 1 |
| `Kasus_hari_berikutnya_tidak_ikut_walau_tanggal_akhir_diperluas` | acceptance 2 — perluasan berhenti pada detik terakhir |
| `Timestamp_akhir_yang_menyebut_jam_dihormati_apa_adanya` | acceptance 3 |
| `Tanggal_akhir_tanpa_jam_juga_inklusif_pada_laporan_material` | acceptance 4 — konsistensi antar laporan |
| `Rentang_terbalik_ditolak` + dua uji laporan lain | keputusan 2 pada ketiga laporan |
| `Tanggal_awal_sama_dengan_tanggal_akhir_tetap_sah` | `From == To` valid |
| `Penyaring_lain_tetap_bekerja_sesudah_normalisasi_tanggal` | normalisasi tidak menelan penyaring lain |