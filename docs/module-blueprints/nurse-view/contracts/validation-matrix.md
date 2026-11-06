# Validation Matrix — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.1` · status `approved` — Rizki Gunawan, 2026-11-02 (`NV-DEC-031`). Sebelumnya `0.1.0`, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-10-28 |

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `VAL-NV-01` | Nama perangkat wajib | Buat/ubah perangkat | kosong atau > 100 karakter | "Nama perangkat wajib diisi, paling banyak 100 karakter." | `400` |
| `VAL-NV-02` | Lantai wajib dan dikenal | Buat/ubah perangkat | kosong, > 20 karakter, atau tidak ada kamar aktif dengan lantai itu | "Lantai tidak dikenal. Pilih lantai yang terdaftar pada data kamar." | `422` |
| `VAL-NV-03` | Lokasi dan keterangan dibatasi | Buat/ubah perangkat | lokasi > 150 atau keterangan > 500 | "Isian terlalu panjang." | `400` |
| `VAL-NV-04` | Versi harus terbaru | Ubah, buat login, atur ulang login, nonaktifkan | `Version` beda dengan data | "Data perangkat sudah diubah orang lain. Muat ulang lalu coba lagi." | `409` |
| `VAL-NV-05` | Login tidak boleh dibuat dua kali | Buat login | status bukan `PendingLogin` | "Perangkat ini sudah punya login. Gunakan Atur Ulang Login." | `409` |
| `VAL-NV-06` | Atur ulang hanya bila login ada | Atur ulang login | status `PendingLogin` | "Perangkat belum punya login. Gunakan Buat Login." | `409` |
| `VAL-NV-07` | Alasan nonaktif wajib | Nonaktifkan | kosong atau < 5 / > 500 karakter | "Alasan menonaktifkan wajib diisi (5–500 karakter)." | `400` |
| `VAL-NV-08` | Tidak bisa nonaktif dua kali | Nonaktifkan | status `Inactive` | "Perangkat sudah nonaktif." | `409` |
| `VAL-NV-09` | Tingkat EWS sah | Ganti interval | `Unknown` atau di luar 1–4 | "Tingkat risiko EWS tidak dikenal." | `400` |
| `VAL-NV-10` | Satu tingkat sekali | Ganti interval | tingkat yang sama muncul dua kali | "Setiap tingkat risiko hanya boleh diisi satu kali." | `400` |
| `VAL-NV-11` | Menit dalam batas | Ganti interval | < 1 atau > 1440 | "Interval harus antara 1 dan 1440 menit (24 jam)." | `400` |
| `VAL-NV-12` | Rujukan pengesahan wajib | Ganti interval | kosong atau > 200 karakter | "Nomor surat/notulen pengesahan Komite Keperawatan wajib diisi." | `400` |
| `VAL-NV-13` | Tanggal pengesahan tidak di masa depan | Ganti interval | `RatifiedOn` > hari ini (jam operasional) | "Tanggal pengesahan tidak boleh melewati hari ini." | `400` |
| `VAL-NV-14` | Hanya akun perangkat yang boleh membaca papan | Runtime | akun bukan `LoginUserId` perangkat mana pun | "Akun ini bukan akun display Nurse View." | `403` |
| `VAL-NV-15` | Perangkat nonaktif tidak boleh membaca papan | Runtime, hub | status `Inactive` | "Perangkat display dinonaktifkan. Hubungi IT." | `403` |
| `VAL-NV-16` | Perangkat nonaktif tidak boleh login | Login, perpanjangan sesi | akun perangkat `Inactive` (terkunci) | Kata sandi benar: "Perangkat display dinonaktifkan. Hubungi IT." Kata sandi salah: pesan login terkunci standar (`NV-DEC-031`) | `401` |

**Contoh `VAL-NV-11`.** Admin mengetik interval `Critical` = 0 karena mengira "terus-menerus"
berarti nol. Sistem menolak. Admin memasukkan 15 menit sesuai SK Komite.

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.2.0` · status `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-11-05 |
| Traceability | `NV-DEC-035`, `037`–`048`, `050`, `053`, `054`; `AD-01`–`04` |

Baris `VAL-NV-01`–`16` di atas tidak berubah.

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `VAL-NV-17` | Nama tablet wajib | Buat/ubah tablet | kosong atau > 100 karakter | "Nama tablet wajib diisi, paling banyak 100 karakter." | `400` |
| `VAL-NV-18` | Bed wajib dan sah | Buat/ubah tablet | kosong, tidak aktif, atau kamarnya tidak punya lantai | "Bed tidak dikenal atau kamarnya belum punya lantai. Pilih bed dari daftar." | `422` |
| `VAL-NV-19` | Satu bed satu tablet (`AD-01`) | Buat tablet, pindah bed, aktifkan kembali | bed sudah punya tablet yang belum nonaktif | "Bed ini sudah punya tablet. Nonaktifkan tablet lama lebih dulu." | `409` |
| `VAL-NV-20` | Tidak pindah bed saat ada panggilan (`AD-04`) | Ubah bed tablet | bed lama punya panggilan aktif | "Bed ini masih punya panggilan aktif. Selesaikan panggilannya dulu." | `409` |
| `VAL-NV-21` | Versi tablet harus terbaru | Ubah, login, nonaktifkan tablet | `Version` beda | "Data tablet sudah diubah orang lain. Muat ulang lalu coba lagi." | `409` |
| `VAL-NV-22` | Login tablet tidak dibuat dua kali | Buat login tablet | status bukan `PendingLogin` | "Tablet ini sudah punya login. Gunakan Atur Ulang Login." | `409` |
| `VAL-NV-23` | Atur ulang hanya bila login ada | Atur ulang login tablet | status `PendingLogin` | "Tablet belum punya login. Gunakan Buat Login." | `409` |
| `VAL-NV-24` | Alasan nonaktif tablet wajib | Nonaktifkan tablet | kosong atau < 5 / > 500 karakter | "Alasan menonaktifkan wajib diisi (5–500 karakter)." | `400` |
| `VAL-NV-25` | Tidak nonaktif dua kali | Nonaktifkan tablet | status `Inactive` | "Tablet sudah nonaktif." | `409` |
| `VAL-NV-26` | Hanya akun tablet aktif | Runtime tablet, hub tablet | akun bukan tablet, tablet `Inactive`, atau sesi lama | "Tablet ini tidak dapat dipakai. Hubungi IT." (sesi lama: pesan `NV-SESSION-STALE`) | `403` |
| `VAL-NV-27` | Tablet nonaktif tidak boleh login (`AD-02`) | Login | akun tablet `Inactive` | Sandi benar: "Tablet bed dinonaktifkan. Hubungi IT." Sandi salah: pesan login terkunci standar | `401` |
| `VAL-NV-28` | Panggilan baru hanya dari bed berpenghuni | Tekan panggil | bed tanpa penempatan aktif | "Bed ini belum berpenghuni. Bila butuh bantuan, tekan bel di dinding." | `409` |
| `VAL-NV-29` | Kunci permintaan wajib | Seluruh `POST` runtime tablet | header `Idempotency-Key` kosong atau bukan UUID | "Permintaan tidak lengkap. Coba tekan lagi." | `400` |
| `VAL-NV-30` | Prioritas sah | Tekan panggil | bukan `1` atau `2` | "Jenis panggilan tidak dikenal." | `400` |
| `VAL-NV-31` | Batal hanya untuk panggilan biasa yang menunggu (`NV-DEC-054`) | Batal | prioritas Darurat, atau status `Claimed` | "Panggilan ini tidak bisa dibatalkan. Perawat sedang menuju." | `409` |
| `VAL-NV-32` | Konfirmasi butuh panggilan aktif | Batal, perawat hadir | bed tidak punya panggilan aktif | "Tidak ada panggilan aktif di bed ini." | `404` |
| `VAL-NV-33` | Klaim hanya panggilan yang menunggu | Saya tangani | status `Claimed` | "Panggilan sudah diambil ‹nama›. Gunakan Ambil alih bila perlu." | `409` |
| `VAL-NV-34` | Ambil alih hanya panggilan yang diklaim orang lain | Ambil alih | status `Waiting`, atau pengklaimnya diri sendiri | "Panggilan ini tidak sedang diklaim perawat lain." | `409` |
| `VAL-NV-35` | Lepas hanya oleh pengklaim | Lepas | pengguna bukan pengklaim | "Hanya perawat yang mengklaim yang bisa melepas panggilan ini." | `403` |
| `VAL-NV-36` | Alasan tutup wajib (`NV-DEC-053`) | Tutup panggilan | alasan kosong atau bukan 2–4; alasan `Lainnya` dengan keterangan < 5 / > 250 karakter | "Pilih alasan menutup panggilan." / "Keterangan wajib diisi untuk alasan Lainnya (5–250 karakter)." | `400` |
| `VAL-NV-37` | Panggilan selesai tidak bisa diubah | Seluruh tindakan panggilan | status akhir | "Panggilan ini sudah selesai." | `409` |
| `VAL-NV-38` | Versi panggilan harus terbaru | Klaim, ambil alih, lepas, tutup, batal, hadir | `Version` beda | "Panggilan sudah berubah. Daftar dimuat ulang." | `409` |
| `VAL-NV-39` | Lantai konsol dikenal | Daftar konsol, hub konsol | lantai kosong atau tidak ada kamar aktif dengan lantai itu | "Lantai tidak dikenal. Pilih lantai dari daftar." | `422` |
| `VAL-NV-40` | Ambang sah | Ganti ambang | kuning di luar 1–240; merah ≤ kuning atau > 480; hanya salah satu diisi; rujukan kosong; tanggal di masa depan | "Ambang kuning 1–240 menit dan ambang merah harus lebih besar, paling banyak 480 menit." / pesan rujukan dan tanggal sama dengan `VAL-NV-12`, `13` | `400` |
| `VAL-NV-41` | Konsol hanya untuk pengguna berizin | Hub `JoinNurseViewCallConsole` | akun perangkat, atau tanpa `NurseViewPatientCall : Read` | "Anda tidak memiliki akses ke konsol panggilan." | `HubException` |

**Contoh `VAL-NV-31`.** Pasien 318-B memanggil biasa pukul 10.00.20, lalu Dewi mengklaimnya pukul
10.00.50. Pukul 10.01.00 keluarga pasien mencoba membatalkan. Tombol "Batal" sudah tidak tampil;
permintaan yang tetap dikirim lewat aplikasi lain ditolak `409`, dan Dewi tetap datang.

**Contoh `VAL-NV-36`.** Sari menutup 305-A dari konsol dengan alasan "Lainnya" tanpa keterangan.
Sistem menolak. Sari menulis "Pasien dipindah ke ruang tindakan sementara" lalu menyimpan.
