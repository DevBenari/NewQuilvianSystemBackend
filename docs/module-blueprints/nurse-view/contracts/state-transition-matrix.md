# State Transition Matrix — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.0` · status `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-10-28 |
| Traceability | `NV-DEC-016`, `022`; `NV-AC-016`, `018` |

Hanya **perangkat display** yang punya status sendiri. Status bed, episode, dan TTV milik modul
pemiliknya dan hanya dibaca.

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
