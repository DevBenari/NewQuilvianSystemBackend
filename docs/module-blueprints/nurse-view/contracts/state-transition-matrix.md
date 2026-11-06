# State Transition Matrix — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.0` · status `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-10-28 |
| Traceability | `NV-DEC-016`, `022`; `NV-AC-016`, `018` |

Hanya **perangkat display** yang punya status sendiri. Status bed, episode, dan TTV milik modul
pemiliknya dan hanya dibaca.

> **Revision 2 (`0.2.0` `approved`, `NV-DEC-055`) — 2026-11-05.** Status tablet bed dan panggilan pasien ada di
> [Revision 2](#revision-2--tablet-bed-dan-panggilan-pasien). Bagian 1–3 tetap `0.1.0`.

## 1. `NvwDisplayDevice.DeviceStatus`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Daftarkan perangkat | `PendingLogin` | Admin IT (`NurseViewDisplayDevice : Create`) | Nama dan lantai terisi; lantai dikenal | `400` / `422` |
| `PendingLogin` | Buat login | `Active` | Admin IT (`: Update`) | `Version` cocok | `409` bila versi beda |
| `Active` | Atur ulang login | `Active` | Admin IT (`: Update`) | `Version` cocok. Kata sandi lama dan sesi lama tidak berlaku | `409` |
| `Active` | Nonaktifkan | `Inactive` | Admin IT (`: Update`) | Alasan 5–500 karakter | `400` |
| `PendingLogin` | Nonaktifkan | `Inactive` | Admin IT (`: Update`) | Alasan terisi | `400` |
| `Inactive` | Atur ulang login (= aktifkan kembali) | `Active` | Admin IT (`: Update`) | `Version` cocok; akun dibuka kuncinya dengan kata sandi baru | `409` |
| `PendingLogin`/`Active`/`Inactive` | Ubah nama/lantai/lokasi | sama | Admin IT (`: Update`) | Lantai dikenal; `Version` cocok | `422` / `409` |

### Transisi yang tidak sah

| Dari status | Tindakan | Hasil |
|---|---|---|
| `Active` atau `Inactive` | Buat login | `409` — login sudah ada; pakai atur ulang login |
| `PendingLogin` | Atur ulang login | `409` — belum ada login; pakai buat login |
| `Inactive` | Nonaktifkan | `409` — sudah nonaktif |
| Mana pun | Ubah `DeviceCode` | Tidak tersedia; kode tetap selamanya |
| Mana pun | Hapus perangkat | Tidak tersedia di MVP; pakai nonaktifkan agar jejak tetap ada |

**Contoh.** TV `NVW-000002` dicuri pukul 21.00. Kepala ruang melapor ke IT. Admin IT
menonaktifkan perangkat dengan alasan "TV hilang, laporan satpam no. 45". Status `Active` →
`Inactive`, akun login terkunci. Seminggu kemudian TV pengganti dipasang; admin menekan atur
ulang login pada perangkat yang sama, mendapat kata sandi baru, dan status kembali `Active`.

## 2. Status runtime perangkat (diamati, bukan diubah orang)

| Keadaan | Syarat | Tampil di daftar admin |
|---|---|---|
| Online | `LastOnlineAt` ≤ 3 menit lalu | "Online" |
| Offline | `LastOnlineAt` > 3 menit lalu atau kosong | "Offline sejak HH:MM" |

## 3. Keadaan papan di TV (frontend)

| Keadaan | Masuk bila | Keluar bila |
|---|---|---|
| `Memuat` | Pertama kali dibuka | Papan pertama diterima |
| `Tampil` | Papan diterima | Koneksi putus > 120 detik |
| `DataBasi` | Tidak ada papan baru selama > 120 detik | Papan baru diterima → `Tampil` |
| `Ditolak` | Server menjawab `401`/`403` | Login ulang → `Memuat` |

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.2.0` · status `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-11-05 |
| Traceability | `NV-DEC-035`, `038`–`041`, `046`, `050`, `052`–`054`; `NV-AC-029`–`045` |

Nama status di bawah dipakai persis sama di `flowcharts/04`–`06`.

### 4. `NvwBedTablet.DeviceStatus`

Sama dengan perangkat display (§1), dengan satu tambahan: pemindahan bed.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Daftarkan tablet | `PendingLogin` | Admin IT (`NurseViewBedTablet : Create`) | Nama terisi; bed aktif di kamar ber-lantai; bed belum punya tablet yang belum nonaktif (`AD-01`) | `400` / `422` / `409` |
| `PendingLogin` | Buat login | `Active` | Admin IT (`: Update`) | `Version` cocok | `409` |
| `Active` | Atur ulang login | `Active` | Admin IT (`: Update`) | `Version` cocok; sandi dan sesi lama gugur | `409` |
| `PendingLogin`/`Active` | Nonaktifkan | `Inactive` | Admin IT (`: Update`) | Alasan 5–500 karakter | `400` |
| `Inactive` | Atur ulang login (aktifkan kembali) | `Active` | Admin IT (`: Update`) | `Version` cocok; bed belum dipakai tablet lain | `409` |
| Mana pun | Pindah ke bed lain | sama | Admin IT (`: Update`) | Bed lama tanpa panggilan aktif (`AD-04`); bed baru belum punya tablet | `409` |

#### Transisi yang tidak sah

| Dari status | Tindakan | Hasil |
|---|---|---|
| `Active`/`Inactive` | Buat login | `409` `VAL-NV-22` |
| `PendingLogin` | Atur ulang login | `409` `VAL-NV-23` |
| `Inactive` | Nonaktifkan | `409` `VAL-NV-25` |
| Mana pun | Ubah `DeviceCode` | Tidak tersedia |
| Mana pun | Hapus tablet | Tidak tersedia; pakai nonaktifkan |

### 5. `NvwPatientCall.CallStatus`

| Nilai | Label | Arti |
|---|---|---|
| `Waiting` | Menunggu | Pasien sudah memanggil, belum ada perawat yang mengklaim |
| `Claimed` | Diklaim | Seorang perawat menyatakan menangani; TV menampilkan "‹nama› menuju" |
| `Handled` | Ditangani | Selesai oleh perawat: "Perawat hadir" di tablet (dikonfirmasi di bed) atau "Tutup panggilan" di konsol (tidak dikonfirmasi di bed) |
| `Cancelled` | Dibatalkan | Pasien membatalkan panggilan biasa yang masih menunggu |
| `ClosedBySystem` | Ditutup sistem | Pasien pindah atau pulang |

`Handled`, `Cancelled`, dan `ClosedBySystem` adalah status akhir. Prioritas (`Normal`/`Emergency`)
bukan status, tetapi tunduk pada aturan "hanya naik".

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Tekan "Panggil perawat" atau "Darurat" | `Waiting` | Tablet bed aktif | Bed punya penempatan aktif; bed tanpa panggilan aktif | `409` `VAL-NV-28` |
| `Waiting`/`Claimed` | Tekan lagi (tombol apa pun) | sama | Tablet bed | — | — ; `PressCount` naik, jam awal tetap |
| `Waiting`/`Claimed`, prioritas `Normal` | Tekan "Darurat" | sama, prioritas `Emergency` | Tablet bed | — | — ; `EscalatedAt` terisi |
| `Waiting` | "Saya tangani" | `Claimed` | Pemegang `NurseViewPatientCall : Update` | `Version` cocok | `409` `VAL-NV-38` |
| `Claimed` | "Ambil alih" | `Claimed` (pengklaim berganti) | Pemegang izin selain pengklaim | `Version` cocok | `409` `VAL-NV-34` |
| `Claimed` | "Lepas" | `Waiting` (jam awal tetap) | Pengklaim saat ini | `Version` cocok | `403` `VAL-NV-35` |
| `Waiting`/`Claimed` | "Perawat hadir" ditahan 3 detik | `Handled` (`IsBedsideConfirmed = true`) | Tablet bed tempat panggilan dibuat; tetap bisa walau bed kini kosong (`NV-DEC-050`) | Ada panggilan aktif | `404` `VAL-NV-32` |
| `Waiting`/`Claimed` | "Tutup panggilan" + alasan | `Handled` (`IsBedsideConfirmed = false`) | Pemegang izin | Alasan sah; "Lainnya" + keterangan 5–250 | `400` `VAL-NV-36` |
| `Waiting`, prioritas `Normal` | "Batal" | `Cancelled` | Tablet bed | — | `409` `VAL-NV-31` |
| `Waiting`/`Claimed` | Penempatan berakhir `Transfer` | `ClosedBySystem` ("pasien pindah") | Sistem, paling lambat 60 detik | — | — |
| `Waiting`/`Claimed` | Penempatan berakhir `EpisodeClosed`/`PatientDeparted` | `ClosedBySystem` ("pasien pulang") | Sistem, paling lambat 60 detik | — | — |

#### Transisi yang tidak sah

| Dari status | Tindakan | Hasil |
|---|---|---|
| `Claimed` | "Saya tangani" oleh perawat lain | `409` `VAL-NV-33` — gunakan Ambil alih |
| `Waiting` | "Ambil alih" atau "Lepas" | `409` `VAL-NV-34` |
| `Claimed` | "Lepas" oleh bukan pengklaim | `403` `VAL-NV-35` |
| `Claimed`, atau prioritas `Emergency` | "Batal" | `409` `VAL-NV-31` |
| Prioritas `Emergency` | Turun menjadi `Normal` | Tidak tersedia (`G-13`) |
| `Handled`/`Cancelled`/`ClosedBySystem` | Tindakan apa pun | `409` `VAL-NV-37`; panggilan selesai tidak dibuka ulang (`G-12`) |
| `Waiting`/`Claimed` | Penempatan berakhir `AdmissionCancelled`/`CorrectedEntry` | **Tidak** ada perubahan; tetap menunggu perawat (`NV-DEC-050`) |
| Mana pun | Klaim berakhir sendiri karena waktu | Tidak ada (`NV-DEC-040`) |

**Contoh lengkap.** 305-A menekan "Panggil perawat" 10.00.00 (`Waiting`), lalu "Darurat"
10.00.50 (tetap `Waiting`, kini `Emergency`). Sari mengklaim 10.01.00 (`Claimed`). Dewi
mengambil alih 10.04.10 (`Claimed`, pengklaim Dewi). Dewi menahan "Perawat hadir" 10.05.00
(`Handled`, dikonfirmasi di bed). Bila sesudahnya pasien menekan "Batal", ditolak `VAL-NV-37`;
bila pasien butuh lagi, ia menekan tombol dan lahir panggilan baru.

### 6. Keadaan layar tablet bed (frontend)

| Keadaan | Masuk bila | Yang tampil |
|---|---|---|
| `Memuat` | Pertama dibuka | Kerangka layar |
| `BedKosong` | `IsBedOccupied = false` dan tanpa panggilan aktif | "Bed belum berpenghuni"; tombol panggil tidak ada |
| `Siap` | Bed berpenghuni, tanpa panggilan aktif | Tombol "Panggil perawat" dan "Darurat" |
| `Menunggu` | Panggilan `Waiting` | Status, lama menunggu; "Batal" hanya bila `CanCancel`; "Darurat" bila `CanEscalate`; area "Perawat hadir" |
| `Diklaim` | Panggilan `Claimed` | "‹nama perawat› menuju"; area "Perawat hadir" |
| `GagalKirim` | Permintaan tekan belum mendapat jawaban | "Panggilan belum terkirim — tekan bel di dinding"; kirim ulang otomatis dengan kunci yang sama |
| `Terputus` | Koneksi realtime dan muat ulang gagal | Pita "Koneksi terputus — gunakan bel di dinding" |
| `Ditolak` | `401`/`403` | Layar login |

### 7. Keadaan Konsol Panggilan (frontend)

| Keadaan | Masuk bila | Yang tampil |
|---|---|---|
| `PilihLantai` | Belum ada lantai tersimpan di PC | Pilihan lantai |
| `Memuat` | Lantai dipilih | Kerangka daftar |
| `Kosong` | Tidak ada panggilan aktif | "Tidak ada panggilan aktif di Lantai ‹n›." |
| `Berisi` | Ada panggilan aktif | Daftar berurutan; bunyi mengikuti `NV-DEC-045` |
| `SuaraDitahanBrowser` | Browser menolak memutar bunyi | Tombol "Aktifkan suara" (`G-14`) |
| `PengingatDitunda` | "Matikan bunyi" ditekan | "Pengingat ditunda sampai HH:MM"; panggilan baru tetap berbunyi |
| `DataBasi` | Tidak ada daftar baru > 120 detik | Pita "Data terakhir HH:MM — koneksi terputus" |
