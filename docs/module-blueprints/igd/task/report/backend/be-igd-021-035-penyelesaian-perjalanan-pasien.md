# BE-IGD-021â€“035 â€” Penyelesaian Perjalanan Pasien IGD

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-021` sampai `BE-IGD-035` |
| Dasar keputusan | `IGD-DEC-106`â€“`109` dan kontrak MVP-1â€“MVP-6 approved |
| Commit dasar | `300922c` |
| Tanggal | 26 Agustus 2026 |
| Status | Selesai di working tree; belum di-commit dan belum diterapkan ke database |

## Hasil per kelompok

| Task | Hasil |
| --- | --- |
| `BE-IGD-021`â€“`025` | Penjaga transisi kunjungan, penutupan melalui closure gate, encounter `Emergency`, dan episode ganda beserta override beralasan selesai |
| `BE-IGD-026`â€“`028` | Pengkajian dan konsultasi IGD dapat dibentuk dari encounter tanpa antrean; jalur rawat jalan tetap dijaga |
| `BE-IGD-029`â€“`030` | Rantai resep serta tabel diagnosis, tindakan, tanda vital, dan CPPT dibuktikan pada konteks IGD |
| `BE-IGD-031` | Transfer diganti menjadi Departure; route lama dihapus dan migration memakai rename |
| `BE-IGD-032` | Status tunggal dipecah menjadi `PhysicalStatus` dan `HandoverStatus`; closure gate hanya membaca status fisik sesuai `IGD-DEC-106` |
| `BE-IGD-033`â€“`034` | Riwayat kejadian append-only, waktu sebenarnya, amend, dan reverse dengan persetujuan orang kedua selesai |
| `BE-IGD-035` | Sikap pesanan, penerimaan per pesanan, penolakan dan sikap pengganti, serta pembentukan pesanan internal selesai |

## Migration

Migration `20260826090500_ImplementIgdFullPatientJourney` ditulis terarah karena scaffold otomatis membawa drift snapshot global dari modul lain. Migration mencakup nullable queue, encounter IGD, episode ganda, rename transfer, dua status, event, order item, constraint/index parsial, backfill, dan jalur `Down`.

Migration tidak pernah dijalankan ke database. Validasi source dilakukan dengan build dan generasi script idempotent ke `obj/igd-migration-validation.sql`.

## Verifikasi

```text
dotnet build QuilvianSystemBackend.csproj --configuration Release --no-restore
=> 0 Error(s), 136 Warning(s)

dotnet test --filter FullyQualifiedName~EmergencyInstallationManagement
=> 234 passed, 0 failed

dotnet test QuilvianSystemBackend.Tests.csproj
=> 750 passed, 2 failed, total 752
```

Dua kegagalan full suite tetap berada pada `InPatientManagement`:

- `InpStatusHistoryAndMonitoringTests.Kriteria1Dan4_RiwayatTerbacaUrutDanTetapTerbacaSetelahEpisodeDitutup`
- `InpCorrectionAndNewbornTests.Kriteria2Dan3_StatusTetapClosedTempatTidurTidakKembaliDanLamaDirawatTidakBertambah`

## Batas operasi

- Tidak ada `database update`.
- Tidak ada commit, push, deploy, atau perubahan data bersama.
- Snapshot global tidak diregenerasi dari scaffold yang tercemar drift modul lain; migration terarah menjadi artefak penerapan IGD.

---

## Koreksi migration — empat kolom penempatan nyaris hilang permanen

Ditemukan saat peninjauan migration `20260826090500_ImplementIgdFullPatientJourney`, setelah
seluruh task-nya dinyatakan selesai.

`Up` membuang `FromRoomId`, `ToRoomId`, `FromBedId`, dan `ToBedId` **tanpa syarat**.
`02-backend-architecture.md` bagian 6.2 melarang tepat hal itu:

> **Langkah 6.** Empat kolom tempat tidur dan ruangan dihapus. Bila sudah ada baris yang
> mengisinya, nilainya **hilang permanen** kecuali diarsipkan lebih dulu. Jumlah baris
> terdampak belum diketahui — `IGD-UNK-03`. Langkah 6 **tidak boleh dijalankan** sebelum angka
> itu diketahui dan keputusan pengarsipannya diambil.

`Down` memang menambahkan kembali keempat kolom, tetapi hanya **bentuknya** — datanya sudah
tidak ada. Cara mundur yang memulihkan kolom kosong bukan cara mundur.

`IGD-UNK-03` masih belum terjawab: menghitungnya butuh kueri ke basis data bersama satu tim,
dan otorisasinya belum diberikan. Menunggu jawaban itu akan menahan seluruh `MVP-4`.

**Perbaikannya menghapus ketergantungan pada jawaban itu.** `Up` kini mengarsipkan lebih dulu
ke `TrxEmergencyDepartureLegacyPlacement`, baru membuang kolomnya:

| Bila `IGD-UNK-03` ternyata | Akibatnya |
| --- | --- |
| Nol baris terisi | Tabel arsip kosong. Tidak merugikan siapa pun |
| Ada baris terisi | Riwayat penempatan utuh, dan `Down` benar-benar memulihkannya |

`Down` menyalin nilainya kembali dari tabel arsip, dan **tidak** menghapus tabel itu — ia satu-
satunya salinan riwayat tersebut, sehingga membuangnya saat mundur membuat pemulihan hanya
dapat dilakukan sekali.

Tabel arsip sengaja **di luar model EF**. Ia artefak penyelamatan data, bukan bagian domain;
memasukkannya ke model berarti mengundang modul lain membacanya seolah data yang hidup.

**Pelajaran:** peringatan cara mundur pada blueprint hanya berguna bila dibaca saat migration
ditulis, bukan saat ditinjau. Migration ini sudah lolos build dan test dengan cacatnya utuh —
karena tidak ada satu pun test yang dapat menangkap kehilangan data yang baru terjadi saat
`database update` dijalankan di basis data yang ada isinya.

---

## `IGD-DEC-106` — empat syaratnya, dan mana yang butuh kode

Gerbang penutupan sudah membaca `PhysicalStatus` saja. Tetapi keputusan itu mengikat **empat**
syarat, dan tiga di antaranya sempat terlewat saat task-nya dinyatakan selesai.

| Syarat | Keadaan | Diwujudkan oleh |
| --- | --- | --- |
| Gerbang membaca fisik saja | Sudah | `EmergencyDispositionService.ValidateVisitClosureAsync` |
| (a) keadaan dokumen tersimpan sebagai fakta saat penutupan | Sudah, **tanpa kolom baru** | `TrxEmergencyDepartureEvent` |
| (b) rantai dokumen tetap dapat ditindaklanjuti setelah penutupan | Sudah | Aksi terima/tolak tidak memeriksa status kunjungan |
| (c) dokumen belum final muncul pada daftar pantau | Sudah | `GET /emergency-departures?handoverStatus=Pending` |
| (d) balasan penutupan menyebut dokumen yang menggantung | **Ditambahkan** | `EmergencyVisitController.Complete` |

**Syarat (a) sengaja tidak diberi kolom baru.** Godaannya adalah menambahkan
`ClosedWithPendingHandover` pada `TrxEmergencyVisit`. Itu akan menyimpan jawaban yang sudah
dapat dihitung: tabel kejadian mencatat setiap perpindahan status dokumen beserta waktunya, dan
`VisitCompletedAt` mencatat kapan kunjungan ditutup — sehingga "dokumen mana yang menggantung
saat penutupan" dapat direkonstruksi tepat. Menambah kolom turunan berarti membuat dua sumber
kebenaran untuk satu fakta, dan yang satu pasti akan menyimpang dari yang lain.

**Syarat (d) butuh kode, dan tidak boleh diselesaikan dengan menahan penutupan.** Menahan dan
mendiamkan sama-sama salah. Yang benar: menutup, lalu menyebut nomor kepergiannya pada pesan
balasan, beserta keterangan bahwa dokumennya masih dapat diterima atau ditolak sesudahnya.

### Sembilan test baru — `EmergencyClosureHandoverTests`

Termasuk satu yang menguji **alasan ① keputusan itu secara langsung**: setelah kunjungan
ditutup dengan dokumen masih `Pending`, `CariEpisodeAktifAsync` untuk pasien yang sama
mengembalikan `null` — artinya pasien boleh mendaftar lagi tanpa jalan keluar beralasan.

Test itu ada supaya hubungan sebab-akibatnya tidak hilang. Seseorang yang kelak menganggap
validation §6 aturan 3 "terlalu longgar" dan memperketatnya akan melihat test ini gagal, dan
gagalnya akan menyebutkan akibat sebenarnya: pasien tertahan di depan pintu IGD karena tanda
tangan yang terlupa di unit lain.

Juga tercakup satu perubahan perilaku nyata: kepergian ber-`PhysicalStatus` `Cancelled` kini
**tuntas**. Gerbang lama memperlakukan `Cancelled` sebagai belum tuntas, sehingga satu
perpindahan yang dibatalkan menahan kunjungan selamanya.

### Verifikasi akhir

```
dotnet build QuilvianSystemBackend.sln --configuration Release   => 0 Error(s)
dotnet test QuilvianSystemBackend.Tests.csproj                   => 759 lulus, 2 gagal, total 761
```

Dua yang gagal tetap milik `InPatientManagement`, tidak tersentuh pekerjaan ini.
