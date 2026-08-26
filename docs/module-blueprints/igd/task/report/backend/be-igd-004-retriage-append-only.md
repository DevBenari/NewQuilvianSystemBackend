# Laporan Perubahan Backend — `BE-IGD-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-004` |
| Judul | Perawat dapat menilai ulang pasien tanpa merusak riwayat |
| Slice | S1 — Perawat dapat menilai ulang pasien |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `IGD-DEC-004`, `IGD-DEC-048`, api contract `POST /{id}/retriage`, state matrix bagian 2, validation matrix bagian 2 |
| Contract version | API `0.2.0` — endpoint yang sudah direncanakan, kini tersedia |
| Dependency | `BE-IGD-001` selesai; `BE-IGD-003` seeder siap tetapi belum dijalankan |
| Commit backend saat dikerjakan | `9b040b2` |
| Tanggal | 14 Agustus 2026 |
| Status | Selesai di kode, belum diuji berjalan |

---

## 1. Masalah yang diperbaiki

Kondisi pasien IGD berubah cepat. Pasien yang tadinya dinilai Hijau bisa memburuk dalam dua
puluh menit, dan sebaliknya pasien Merah bisa membaik setelah ditangani.

Sebelum perubahan ini, satu-satunya cara mencatat perubahan itu adalah **mengubah penilaian
lama**. Akibatnya penilaian awal hilang, dan tidak ada yang bisa membuktikan bagaimana kondisi
pasien saat pertama datang.

> **Contoh:** pasien tiba pukul 20.00, dinilai level 4 karena hanya mengeluh nyeri perut.
> Pukul 20.25 tekanan darahnya turun drastis dan perawat menilai ulang menjadi level 2.
>
> Dengan cara lama, penilaian level 4 ditimpa menjadi level 2. Bila keesokan harinya keluarga
> mempertanyakan mengapa pasien menunggu 25 menit, catatan sistem menunjukkan pasien "selalu"
> level 2 — sehingga rumah sakit justru terlihat menelantarkan pasien gawat, padahal
> kenyataannya kondisi pasien memang berubah di tengah jalan.

Setelah perubahan ini, kedua penilaian tersimpan berdampingan dan urutannya terbaca jelas.

---

## 2. Proses bisnis

### 2.1 Tujuan

Perubahan kondisi pasien tercatat sebagai penilaian baru, sementara penilaian sebelumnya tetap
utuh sebagai riwayat yang dapat diaudit.

### 2.2 Pelaku

| Pelaku | Kewenangan |
| --- | --- |
| Perawat IGD | Menilai ulang pasien; memerlukan hak `EmergencyTriage : Update` |
| Dokter IGD | Memakai hasil penilaian terakhir sebagai dasar penanganan |
| Auditor dan komite mutu | Menelusuri urutan penilaian dari awal sampai akhir |

### 2.3 Pemicu

Kondisi pasien berubah, atau pasien sudah menunggu lama sehingga perlu dinilai ulang.

### 2.4 Prasyarat

| Prasyarat | Alasan |
| --- | --- |
| Penilaian sebelumnya berstatus `Completed` | Hanya penilaian yang sudah selesai yang punya kesimpulan untuk digantikan |
| Kunjungan IGD masih aktif | Kunjungan yang sudah ditutup tidak dinilai ulang |
| Level triase baru terdaftar dan aktif | Level diambil dari master, tidak boleh diketik bebas |

### 2.5 Langkah utama

1. Perawat membuka penilaian pasien yang sudah selesai.
2. Perawat menekan "Nilai ulang", lalu mengisi level baru dan ringkasan pemeriksaan.
3. Sistem memeriksa seluruh syarat pada bagian 2.6.
4. Sistem membuat **baris penilaian baru** dengan nomor urut berikutnya, ditandai sebagai
   penilaian ulang, dan menunjuk penilaian lama sebagai pendahulunya.
5. Sistem mengubah status penilaian lama menjadi `Superseded`, yang berarti "sudah digantikan".
   Isi klinisnya tidak disentuh sama sekali.
6. Keduanya disimpan dalam satu tarikan, sehingga tidak mungkin yang satu tersimpan tanpa yang
   lain.
7. Perawat melanjutkan mengisi penilaian baru sampai selesai memakai alur status yang sudah ada.

### 2.6 Aturan bisnis

**Aturan A — Hanya penilaian selesai yang dapat dinilai ulang.**

| Status penilaian lama | Hasil | Pesan |
| --- | --- | --- |
| `Completed` | Diterima | — |
| `Cancelled` | Ditolak (409) | "Penilaian triage yang sudah dibatalkan tidak dapat dinilai ulang." |
| `Draft` | Ditolak (409) | "Hanya penilaian yang sudah selesai yang dapat dinilai ulang." |
| `InProgress` | Ditolak (409) | "Hanya penilaian yang sudah selesai yang dapat dinilai ulang." |
| `Superseded` | Ditolak (409) | "Hanya penilaian yang sudah selesai yang dapat dinilai ulang." |

Penilaian `Cancelled` sengaja diberi pesan tersendiri karena alasannya memang berbeda:
penilaian yang dibatalkan tidak pernah berlaku, sehingga tidak ada yang bisa digantikan.

**Aturan B — Penilaian lama tidak pernah ditimpa.**

Yang berubah pada baris lama hanya dua hal: statusnya menjadi `Superseded`, dan jejak audit
"diubah oleh siapa dan kapan". Level, waktu, ringkasan ABCDE, target waktu, dan seluruh kolom
klinis lainnya tidak disentuh satu pun.

> **Contoh:** penilaian pukul 20.00 tercatat level 4 dengan ringkasan sirkulasi "nadi 88, tekanan
> darah 120/80". Setelah dinilai ulang pukul 20.25, baris itu tetap berisi level 4 dan angka
> yang sama persis. Yang berubah hanya statusnya menjadi "sudah digantikan".

**Aturan C — Nomor urut selalu bertambah.**

Penilaian baru memperoleh nomor urut satu lebih besar dari nomor tertinggi pada kunjungan itu.

> **Contoh:** kunjungan sudah punya penilaian nomor 1 dan 2. Penilaian ulang berikutnya
> memperoleh nomor 3. Urutan inilah yang dibaca auditor untuk melihat perjalanan kondisi
> pasien.

**Aturan D — Target waktu mengikuti level baru, dan boleh kosong.**

Target waktu disalin dari level yang baru dipilih. Bila level itu belum punya target — misalnya
level 3 yang menunggu SOP MMC — maka batas waktu responsnya dikosongkan, bukan dianggap nol
menit. Ini melanjutkan aturan yang ditetapkan `BE-IGD-002`.

> **Contoh:** pasien dinilai ulang menjadi level 2 pukul 20.25. Level 2 belum punya target,
> sehingga batas waktu respons penilaian baru dikosongkan. Bila kelak SOP menetapkan level 2
> adalah 10 menit, penilaian **baru setelah itu** akan memakai 10 menit; penilaian pukul 20.25
> tetap kosong karena targetnya dibekukan saat penilaian dibuat.

**Aturan E — Menekan tombol dua kali tetap menghasilkan satu baris.**

Penekanan pertama mengubah penilaian lama menjadi `Superseded`. Penekanan kedua menemukan
statusnya sudah bukan `Completed`, sehingga ditolak oleh Aturan A. Perlindungan ini tidak
memerlukan mekanisme tambahan apa pun — aturan statusnya sendiri yang menutup celah.

Untuk dua permintaan yang tiba **benar-benar bersamaan**, index unik `(EmergencyVisitId,
Sequence)` yang sudah ada menolak yang kedua, dan pengguna menerima pesan "Penilaian ulang
gagal disimpan karena data sedang diubah pihak lain."

### 2.7 Perubahan status

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `Completed` | Nilai ulang | `Superseded` | Perawat dengan hak `EmergencyTriage : Update` | Ada penilaian baru yang menunjuk baris ini |
| — | Penilaian ulang dibuat | `Draft` | Perawat yang sama | Level triase baru terdaftar dan aktif |

### 2.8 Jalur tidak normal

| Kejadian | Yang terjadi | Yang dilihat perawat |
| --- | --- | --- |
| Penilaian tidak ditemukan | Ditolak (404) | "Data triage IGD tidak ditemukan." |
| Penilaian sudah dibatalkan | Ditolak (409) | "Penilaian triage yang sudah dibatalkan tidak dapat dinilai ulang." |
| Penilaian belum selesai | Ditolak (409) | "Hanya penilaian yang sudah selesai yang dapat dinilai ulang." |
| Kunjungan sudah ditutup | Ditolak (409) | "Kunjungan IGD sudah ditutup, sehingga tidak dapat dinilai ulang." |
| Level triase tidak dipilih | Ditolak (400) | "TriageLevelId wajib diisi." |
| Level triase tidak aktif | Ditolak (400) | "TriageLevelId tidak ditemukan atau tidak aktif." |
| Tanpa hak akses | Ditolak (403) | "Anda tidak memiliki hak akses untuk tindakan ini." |
| Dua perawat menilai ulang bersamaan | Hanya satu berhasil | "Penilaian ulang gagal disimpan karena data sedang diubah pihak lain. Muat ulang halaman lalu coba lagi." |

### 2.9 Hasil akhir

Kunjungan memiliki rangkaian penilaian bernomor urut. Satu penilaian berstatus aktif, sisanya
berstatus `Superseded` dan tetap terbaca lengkap.

---

## 3. Keputusan desain yang perlu diketahui pemilik

**Penilaian ulang dibuat berstatus `Draft`, bukan langsung `Completed`.**

Alasannya: state matrix bagian 2 menetapkan setiap penilaian dimulai dari `Draft`, lalu
`InProgress`, lalu `Completed`. Membuat penilaian ulang langsung `Completed` berarti melewati
dua syarat yang sudah disepakati, yaitu "ringkasan ABCDE minimal satu" dan "level triase wajib
terisi sebelum penilaian diselesaikan".

Akibat yang perlu disadari: sesaat setelah penilaian ulang dibuat, kunjungan tidak punya
penilaian berstatus `Completed`, karena yang lama sudah `Superseded` dan yang baru masih
`Draft`. Jendela ini berlangsung selama perawat mengisi penilaian barunya.

Bila pemilik menghendaki penilaian ulang langsung selesai dalam satu langkah, itu keputusan
tersendiri yang mengubah state matrix, dan perlu ditetapkan lebih dulu — bukan diputuskan
diam-diam saat menulis kode.

---

## 4. File yang diubah

| File | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyTriageDtos.cs` | **Baru** — `RetriageEmergencyTriageRequest` |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyTriageService.cs` | **Baru** — `RetriageAsync` beserta kelas hasil `RetriageOutcome` |
| `Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyTriageController.cs` | **Baru** — aksi `POST /{id}/retriage` |
| `docs/module-blueprints/igd/contracts/api-contract.md` | Status endpoint retriage berubah dari "Rencana" menjadi "Sudah ada" |

### 4.1 Mengapa aturannya diletakkan di service

Seluruh pemeriksaan penolakan berada di `EmergencyTriageService.RetriageAsync`, bukan di
controller. Controller hanya menerjemahkan hasilnya menjadi kode HTTP.

Ini disengaja: bila kelak ada proses lain yang perlu menilai ulang pasien — misalnya proses
latar belakang atau integrasi — proses itu memanggil service yang sama dan otomatis tunduk pada
aturan yang sama. Aturan yang ditulis di controller akan terlewati oleh pemanggil semacam itu.

### 4.2 Yang tidak dikirim pemanggil

Enam hal berikut ditetapkan server dan **tidak** dapat dipaksakan lewat permintaan: nomor urut,
penanda penilaian ulang, penunjuk penilaian sebelumnya, sistem triase, target waktu, dan batas
waktu respons. Kunjungan juga tidak dikirim — diturunkan dari penilaian yang dinilai ulang,
sehingga mustahil penilaian ulang nyasar ke kunjungan pasien lain.

### 4.3 Keutuhan data saat gagal

Perubahan status penilaian lama dan pembuatan penilaian baru disimpan lewat **satu** perintah
simpan, sehingga keduanya berada dalam satu transaksi basis data. Bila penyimpanan gagal,
keduanya batal bersama. Tidak mungkin ada penilaian yang berstatus "sudah digantikan" tanpa ada
penggantinya.

---

## 5. Dokumentasi endpoint

### Health Services / Emergency Installation Management / Emergency Triage

Base URL: `api/v1/health-services/emergency-installation-management/emergency-triages`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/retriage` | Menilai ulang pasien; penilaian lama ditandai sudah digantikan | `EmergencyTriage : Update` | Path `id` + body `RetriageEmergencyTriageRequest` | `EmergencyTriageResponse` penilaian yang baru |

Isi `RetriageEmergencyTriageRequest`:

| Field | Wajib | Keterangan |
| --- | :---: | --- |
| `triageLevelId` | **Ya** | Level triase hasil penilaian ulang |
| `patientVitalSignId` | Tidak | Tanda vital yang mendasari penilaian ulang |
| `startedAt` | Tidak | Waktu penilaian ulang dimulai; bila kosong memakai waktu server |
| `triageReason` | Tidak | Alasan klinis penilaian ulang |
| `airwaySummary` sampai `exposureSummary` | Tidak | Ringkasan pemeriksaan ABCDE |
| `redFlagSummary` | Tidak | Tanda bahaya |
| `notes` | Tidak | Catatan tambahan |

Contoh permintaan:

```json
{
  "triageLevelId": "3f2a...-contoh",
  "triageReason": "Tekanan darah turun, pasien tampak pucat",
  "circulationSummary": "Nadi 118, tekanan darah 85/50",
  "redFlagSummary": "Tanda syok"
}
```

Contoh balasan berhasil:

```json
{
  "statusCode": 200,
  "success": true,
  "message": "Penilaian ulang triage IGD berhasil dibuat.",
  "data": {
    "sequence": 3,
    "isRetriage": true,
    "previousTriageId": "8c11...-contoh",
    "triageStatus": "Draft",
    "maxWaitingMinutesSnapshot": null,
    "responseDueAt": null
  }
}
```

Contoh balasan ditolak:

```json
{
  "statusCode": 409,
  "success": false,
  "message": "Penilaian triage yang sudah dibatalkan tidak dapat dinilai ulang.",
  "data": null
}
```

Seluruh data pada contoh adalah data samaran.

### Kode status dan artinya

| Kode | Arti teknis | Arti bagi perawat |
| --- | --- | --- |
| `200` | Berhasil | Penilaian ulang tersimpan dan siap dilengkapi |
| `400` | Permintaan tidak valid | Level triase belum dipilih atau tidak aktif |
| `401` | Belum masuk | Sesi habis; perlu masuk ulang |
| `403` | Tidak berwenang | Tidak punya hak menilai ulang |
| `404` | Tidak ditemukan | Penilaian yang dibuka sudah dihapus atau tidak pernah ada |
| `409` | Bentrok | Penilaian tidak dalam keadaan yang boleh dinilai ulang, atau sedang diubah pihak lain |

---

## 6. Verifikasi

### 6.1 Yang sudah dijalankan

| Pemeriksaan | Cara | Hasil |
| --- | --- | --- |
| Build project | `dotnet build` | **Lulus** — 0 galat, 125 peringatan, jumlahnya sama persis dengan sebelum perubahan |
| Aturan transisi konsisten dengan state matrix | Perbandingan `CanTransition` dengan state matrix bagian 2 baris demi baris | Lulus — `Completed` hanya boleh ke `Superseded`; `Cancelled` dan `Superseded` tidak boleh ke mana pun |
| Pesan penolakan sama persis dengan validation matrix | Perbandingan kata demi kata untuk dua pesan yang ditentukan kontrak | Lulus |
| Index unik yang dipakai memang sudah ada | `TrxEmergencyTriageConfiguration.cs` baris 27 | Lulus — `(EmergencyVisitId, Sequence)` unik, tidak perlu migration |
| Kolom yang dipakai memang sudah ada | `TrxEmergencyTriage.cs`: `Sequence`, `IsRetriage`, `PreviousTriageId` | Lulus — tidak ada kolom baru, tidak ada migration |
| Baris lama tidak tersentuh selain status | Pembacaan ulang `RetriageAsync`; hanya tiga field baris lama yang ditulis, yaitu status, waktu ubah, dan pengubah | Lulus |

Perlu ditegaskan untuk pemeriksaan terakhir: tidak ada satu pun kolom klinis baris lama yang
muncul di sisi kiri tanda sama dengan. Itulah bukti sifat append-only pada tingkat kode.

### 6.2 Acceptance criteria

| No | Kriteria | Status | Dasar |
| ---: | --- | --- | --- |
| 1 | Baris baru dengan `Sequence` berikutnya, `IsRetriage` benar, `PreviousTriageId` menunjuk baris lama | **Terpenuhi di kode, belum diuji berjalan** | Ketiganya ditetapkan server, tidak dapat dikirim pemanggil |
| 2 | Baris lama menjadi `Superseded` dan isinya tidak berubah | **Terpenuhi di kode, belum diuji berjalan** | Hanya status dan jejak audit yang ditulis |
| 3 | Retriage atas `Cancelled` ditolak 409 dengan pesan yang ditentukan | **Terpenuhi di kode, belum diuji berjalan** | Pemeriksaan pertama, sebelum pemeriksaan status lain |
| 4 | Retriage atas penilaian belum `Completed` ditolak 409 | **Terpenuhi di kode, belum diuji berjalan** | Pemeriksaan kedua |
| 5 | Tanpa hak `EmergencyTriage : Update` ditolak 403 | **Terpenuhi di kode, belum diuji berjalan** | `[AccessPermission("EmergencyTriage", "Update")]`, pola yang sama dengan aksi lain |
| 6 | Menekan tombol dua kali hanya menghasilkan satu baris | **Terpenuhi di kode, belum diuji berjalan** | Penekanan kedua tertolak Aturan A karena status sudah `Superseded` |

### 6.3 Yang belum dijalankan

| Yang belum diuji | Alasan | Cara menutupnya |
| --- | --- | --- |
| Enam integration test untuk keenam kriteria | Repository belum punya project test. Ini blocker yang sama sejak `BE-IGD-001` dan belum diputuskan | Perlu keputusan pemilik tentang project test |
| Test yang membandingkan baris lama kolom demi kolom | Alasan yang sama | Sama seperti di atas |
| Percobaan langsung lewat Swagger | Belum ada basis data lokal, dan master IGD belum terisi karena seeder `BE-IGD-003` belum dijalankan | Siapkan basis data lokal, jalankan seeder, lalu coba alur lengkap |

Tanpa data master, endpoint ini belum dapat dicoba sama sekali, karena tidak ada level triase
yang bisa dipilih sebagai hasil penilaian ulang.

---

## 7. Risiko tersisa

| No | Risiko | Akibat nyata bila diabaikan |
| ---: | --- | --- |
| 1 | **`CG-07` belum ditutup.** Endpoint `PUT /emergency-triages/{id}` yang sudah ada masih dapat menimpa penilaian mana pun, termasuk yang sudah `Superseded` | Riwayat yang dijaga endpoint retriage tetap dapat dirusak lewat pintu lain. Roadmap sudah menyatakan task ini **tidak** menyelesaikannya |
| 2 | Belum ada test otomatis | Sifat append-only dapat rusak tanpa ketahuan pada perubahan berikutnya |
| 3 | Penilaian ulang berstatus `Draft` | Bila perawat tidak melanjutkan sampai selesai, kunjungan tidak punya penilaian aktif yang selesai. Lihat bagian 3 |
| 4 | Master IGD belum terisi | Endpoint belum dapat dipakai siapa pun |

Risiko nomor 1 adalah yang paling penting untuk diketahui pemilik: sifat "tidak boleh ditimpa"
baru berlaku pada jalur retriage, belum pada seluruh modul.

---

## 8. Bukti penelusuran

| Klaim | Bukti |
| --- | --- |
| Kolom `Sequence`, `IsRetriage`, `PreviousTriageId` sudah tersedia | `NewQuilvianSystemBackend` + `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs` baris 23, 25, 27 + `9b040b2` |
| Index unik nomor urut sudah tersedia | `Repositories/Configurations/HealthService/EmergencyInstallationManagement/TrxEmergencyTriageConfiguration.cs` baris 27 + `9b040b2` |
| Transisi `Completed` ke `Superseded` memang sah | `docs/module-blueprints/igd/contracts/state-transition-matrix.md` bagian 2 |
| Dua pesan penolakan ditentukan kontrak | `docs/module-blueprints/igd/contracts/validation-matrix.md` bagian 2 |
| Endpoint sebelumnya berstatus rencana | `docs/module-blueprints/igd/contracts/api-contract.md` baris 50 sebelum diubah |
| `PUT` masih dapat menimpa baris historis | `Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyTriageController.cs` aksi `Update`, tidak memeriksa status sebelum menimpa + `9b040b2` |
