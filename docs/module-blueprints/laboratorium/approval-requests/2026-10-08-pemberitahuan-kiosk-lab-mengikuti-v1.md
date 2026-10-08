# `LAB-REQ-018` — Pemberitahuan perubahan bagian Laboratorium pada kiosk

| Field | Nilai |
| --- | --- |
| Request ID | `LAB-REQ-018` |
| Tanggal | 2026-10-08 |
| Diajukan oleh | Yoga Aji Pratama (`yogaaji452@gmail.com`), pemilik modul Laboratorium |
| Ditujukan kepada | Andry Zain, pemilik `registration-management` |
| Sifat | **Pemberitahuan**, bukan permintaan persetujuan. Perubahan berada di dalam bagian Laboratorium pada kiosk yang disetujui lewat `LAB-REQ-006` (2026-09-15) |
| Status | `menunggu dibaca` |
| Rujukan | `LAB-DEC-213` (decision log revision `91`, putaran 26); `LAB-EVD-013`; `LAB-DEC-051`..`LAB-DEC-054` |

## 1. Ringkasnya

Pasien lama yang memilih **Laboratorium** di kiosk kini menjalani alur yang sama dengan kiosk versi 1. Kiosk
tidak lagi bertanya *"Apakah Anda membawa surat dari dokter?"*. Sebagai gantinya kiosk memeriksa sendiri apakah
pasien sudah punya pesanan laboratorium yang masih berjalan.

## 2. Alur baru

| Keadaan pasien | Yang dilihat pasien | Yang tercatat |
|---|---|---|
| Sudah punya pesanan Lab dari dokter | Layar *Konfirmasi Kehadiran*: tanggal, asal poli, dokter, daftar pemeriksaan → tombol *Konfirmasi Kehadiran* | Hanya sesi kiosk bertujuan Laboratorium, penanda *membawa permintaan dokter* = ya. **Nol** kunjungan baru |
| Belum punya pesanan Lab | Jenis Kunjungan (Umum/Rujukan) → Pembayaran → Konfirmasi → Cetak Antrean | Kunjungan dibentuk lewat route kiosk Registrasi yang sudah ada (`POST /patient-encounters/kiosk`) ke unit Laboratorium (`SU-LAB-001`), **tanpa** dokter; sesi kiosk ditandai terpakai |

**Contoh:** pasien Tunai tanpa pesanan Lab memilih Laboratorium → Kunjungan Umum → Tunai → *Konfirmasi &
Daftarkan* → tiket antrean `Q002` untuk unit Laboratorium Klinik.

## 3. Yang **tidak** berubah di sisi Registrasi

- Route, validasi, dan kepemilikan pembentukan kunjungan tetap milik Registrasi. Kode Laboratorium tetap tidak
  menulis kunjungan maupun pasien (`AC-45`).
- Alur kiosk Poliklinik tidak berubah.
- Penutupan otomatis kunjungan kiosk yang tidak dilanjutkan tetap berjalan seperti `LAB-DEC-054`/`LAB-DEC-059`.

## 4. Dua hal yang mungkin perlu ditinjau Registrasi

Keduanya **di luar wewenang Laboratorium** dan tidak menahan apa pun:

1. Kunjungan kiosk ke unit Laboratorium terbentuk berstatus *Menunggu Dokter*, padahal unit ini tidak memakai
   dokter. Kiosk Laboratorium menampilkannya sebagai *Menunggu Panggilan Laboratorium*.
2. Nomor antrean unit Laboratorium berprefiks `Q` (contoh `Q001`). Bila Registrasi punya prefiks khusus
   Laboratorium, kiosk akan mengikutinya tanpa perubahan.

## 5. Cara menanggapi

Cukup balas bila ada keberatan. Tanpa keberatan, pemberitahuan ini dianggap sudah dibaca.
