# Laporan Perubahan Backend — `BE-IGD-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-006` |
| Judul | Sistem menandai sendiri pasien yang terlambat ditangani |
| Slice | S2 — Pasien menunggu terlalu lama tertandai |
| Trace | `IGD-DEC-027`, `IGD-GAP-007`; integration contract bagian "Proses terjadwal di dalam aplikasi" |
| Dependency | `BE-IGD-002` dan `BE-IGD-005` — keduanya sudah dikerjakan |
| Commit backend | `21c609f2853574532f74dd2b1489b8d2e502abd1` |
| Tanggal | 18 Agustus 2026 |
| **Status** | **Belum selesai — belum pernah dikompilasi maupun dijalankan** |

---

## 1. Masalah yang diperbaiki

`BE-IGD-005` menyediakan tempat menyimpan penanda keterlambatan, tetapi tidak ada yang
mengisinya. Tanpa task ini, keterlambatan hanya terlihat bila ada orang yang kebetulan membuka
layar dan menghitung sendiri.

Pemantau ini berjalan sebagai proses latar di dalam aplikasi yang sama. Ia memeriksa berkala
penilaian yang batas waktu responsnya sudah lewat sementara pasiennya belum ditangani, lalu
menandainya. Keterlambatan karena itu tercatat walaupun tidak ada yang menonton layar.

## 2. Proses bisnis

### 2.1 Pelaku

Tidak ada pelaku manusia. Seluruh penulisan dilakukan proses latar.

### 2.2 Aturan yang ditegakkan

| No | Aturan | Cara ditegakkan |
| --- | --- | --- |
| 1 | Target yang belum dikonfigurasi tidak pernah dianggap terlambat | Saringan `ResponseDueAt != null`, menyambung `BE-IGD-002` |
| 2 | Penandaan tidak berganda | Saringan `!IsSlaBreached`; baris bertanda tidak pernah masuk kandidat lagi |
| 3 | Penilaian batal tidak pernah berlaku | Saringan `TriageStatus != Cancelled` |
| 4 | Pasien yang sudah ditangani tidak ditandai | `visit.TreatmentStartedAt == null` |
| 5 | Kegagalan tidak memblokir pelayanan | Proses latar terpisah dengan `catch` menyeluruh |
| 6 | Data klinis tidak berubah | Hanya `IsSlaBreached` dan `SlaBreachedAt` yang ditulis |

### 2.3 Mengapa `TreatmentStartedAt`, bukan status kunjungan

Status kunjungan dapat bergerak maju-mundur antara `InTreatment` dan `UnderObservation`.
`TreatmentStartedAt` diisi sekali memakai `??=` saat penanganan pertama dimulai dan tidak pernah
tertimpa, sehingga merupakan jawaban paling langsung atas "apakah pasien ini sudah ditangani".

## 3. File yang diubah

| File | Perubahan |
| --- | --- |
| `Services/EmergencyTriageSlaMonitorOptions.cs` | **Baru** — `Enabled`, `PollIntervalSeconds`, `BatchSize` |
| `Services/EmergencyTriageSlaMonitorHostedService.cs` | **Baru** — `BackgroundService` mengikuti pola HR |
| `Services/EmergencyTriageService.cs` | `MarkSlaBreachesAsync` |
| `Program.cs` | `Configure<EmergencyTriageSlaMonitorOptions>` + `AddHostedService` |

Frekuensi tidak ditanam di kode. Nilai bawaan 60 detik dipilih karena target respons dihitung
dalam satuan menit, dan dapat diubah lewat seksi konfigurasi
`HealthServices:EmergencyTriageSlaMonitor` tanpa menyentuh kode.

`BatchSize` membatasi ukuran satu transaksi supaya lonjakan pasien tidak menghasilkan satu
penulisan raksasa; sisanya diproses pada pemindaian berikutnya.

## 4. Verifikasi

**Belum ada satu pun verifikasi berjalan.** Build tidak dijalankan atas permintaan owner, dan
solution tidak memiliki test project sehingga `AT-IGD-020` sampai `AT-IGD-023` tidak punya
tempat untuk ditulis.

| Kriteria | Status |
| --- | --- |
| 1. Penilaian lewat batas dan belum ditangani ditandai | Ada di kode — **belum terbukti** |
| 2. `ResponseDueAt` kosong tidak pernah ditandai | Ada di kode — **belum terbukti** |
| 3. Satu menit sebelum batas tidak ditandai | Ada di kode (`<= now`) — **belum terbukti** |
| 4. Pemindaian dua kali tidak menggeser `SlaBreachedAt` | Ada di kode — **belum terbukti** |
| 5. Kegagalan tidak menghalangi pelayanan | Ada di kode — **belum terbukti** |
| 6. Kolom klinis tidak berubah | Ada di kode — **belum terbukti** |

## 5. Penyimpangan dari scope roadmap

Roadmap menuliskan scope hanya berkas hosted service dan satu baris `Program.cs`. Kenyataannya
ada tiga tambahan:

1. **Berkas options terpisah** — konsekuensi langsung dari perintah roadmap sendiri agar
   frekuensi dapat dikonfigurasi dan tidak ditanam di kode.
2. **`MarkSlaBreachesAsync` diletakkan di `EmergencyTriageService`**, bukan di dalam hosted
   service. Ini justru mengikuti arahan Reuse: kelima hosted service HR semuanya mendelegasikan
   pekerjaan ke service ber-scope, bukan mengerjakannya sendiri.
3. **Dua baris di `Program.cs`**, bukan satu, karena `Configure` dan `AddHostedService` adalah
   dua panggilan terpisah.

Ketiganya menambah, tidak mengubah arah desain.

## 6. Keputusan yang perlu disahkan owner

| No | Keputusan | Alasan diambil | Yang diminta |
| ---: | --- | --- | --- |
| 1 | Penilaian `Superseded` **tetap** dapat ditandai breach | Penantian sebelum retriage benar-benar terjadi; menghapusnya berarti memalsukan riwayat mutu | Penegasan Product/Domain |
| 2 | Penilaian `Cancelled` dikecualikan | Penilaian batal tidak pernah berlaku, sejalan dengan aturan retriage `BE-IGD-004` | Penegasan Product/Domain |
| 3 | Kunjungan `Cancelled` dikecualikan | Pasien sudah tidak menunggu; menandainya sekarang akan mencatat `SlaBreachedAt` yang salah | Penegasan Product/Domain |
| 4 | Kolom audit tidak disentuh | Menimpa `UpdateBy` dengan pelaku sistem menghapus jejak siapa terakhir mengubah penilaian klinis | Penegasan Backend/API |
| 5 | Frekuensi bawaan 60 detik | Belum ditetapkan siapa pun; roadmap meminta nilai wajar yang dapat dikonfigurasi | Penetapan Product/Domain |

## 7. Risiko tersisa

| No | Risiko | Penanganan |
| ---: | --- | --- |
| 1 | ~~Kolom `IsSlaBreached` belum ada di database~~ | **Tertutup** — migration `BE-IGD-005` sudah diterapkan dan kedua kolom terverifikasi di database lokal maupun `QuilvianNewDevTim01` |
| 2 | Pemantau menyala sebelum data master terisi | `BE-IGD-003` belum tuntas; `Enabled=false` dapat dipakai sampai siap |
| 3 | Beban basis data bila interval diturunkan drastis | Batas bawah 10 detik dipaksa di kode |
