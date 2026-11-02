# Validation Matrix — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.0` · status `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
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
| `VAL-NV-16` | Perangkat nonaktif tidak boleh login | Login | akun perangkat `Inactive` (terkunci) | Pesan login terkunci standar | `401` |

**Contoh `VAL-NV-11`.** Admin mengetik interval `Critical` = 0 karena mengira "terus-menerus"
berarti nol. Sistem menolak. Admin memasukkan 15 menit sesuai SK Komite.
