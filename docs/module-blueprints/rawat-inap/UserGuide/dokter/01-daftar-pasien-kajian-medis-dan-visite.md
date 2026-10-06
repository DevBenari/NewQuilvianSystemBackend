# Daftar Pasien, Kajian Medis, dan Visite Dokter

Versi 1.0 · 6 Oktober 2026.

## Tujuan, pelaku, dan prasyarat

Dokter membuka pasien yang menjadi tanggung jawabnya, mengisi kajian medis, dan mencatat visite. Pemicu adalah awal perawatan, pemeriksaan ulang, atau kunjungan dokter.

Prasyarat: akun terhubung identitas dokter, penugasan pada episode sah, dan data pasien terbaca. Kajian medis adalah dokumen dokter; pengkajian perawat ditampilkan sebagai referensi tersendiri.

## Membuka pasien

1. Buka **Dokter Rawat Inap** di `/health-services/inpatient-management/doctor-inpatient`.
2. Cari pasien pada daftar yang tersedia; gunakan penyaring yang diperlukan.
3. Pilih pasien dan periksa RM, nomor episode, unit/bed, alergi, diagnosis kerja, serta DPJP aktif.
4. Jika pasien tidak muncul, periksa penugasan dan penyaring sebelum meminta akses tambahan.
5. Buka tab **Kajian Pasien**. Tab **Hasil Skrining** menyediakan rujukan pengukuran/pemantauan perawat; membaca hasil ini tidak membuat kajian medis baru.

## Membuat kajian medis

1. Periksa riwayat; gunakan **Kajian Baru** untuk kejadian yang memang memerlukan dokumen baru.
2. Pilih Kajian Medis Awal atau Kajian Medis Ulang sesuai pemeriksaan.
3. Isi formulir. Bagian wajib pada source mencakup keluhan utama, riwayat penyakit sekarang, pemeriksaan fisik, diagnosis kerja, dan rencana terapi.
4. Isi bagian tambahan yang diperlukan dan periksa seluruh hasil.
5. Simpan draf untuk pekerjaan yang belum selesai.
6. Gunakan aksi penyelesaian saat pemeriksaan telah lengkap; baca validasi/tanda tangan yang diminta.
7. Periksa status dokumen pada riwayat. Selesainya penyimpanan draf bukan penyelesaian kajian.
8. Koreksi dokumen final melalui addendum/jalur yang disediakan.

## Mencatat visite

1. Buka tab **Visit** lalu pilih **Catat Visite**.
2. Isi **Waktu Visite** sebagai waktu kedatangan ke pasien.
3. Pilih peran sesuai penugasan: DPJP, Konsulen, atau Dokter Jaga.
4. Isi catatan; tautkan dokumen Catatan Perkembangan, Catatan Terpadu, atau Tindakan bila diperlukan dan tersedia.
5. Simpan dan periksa Riwayat Visite.
6. Jika kejadian salah/duplikat, gunakan Batalkan dengan alasan sesuai hak; riwayat tetap tersimpan.

## Status dan contoh

| Catatan | Keadaan | Makna |
| --- | --- | --- |
| Kajian | Draf/Sedang diisi | Masih perlu dilanjutkan sesuai hak penulis. |
| Kajian | Selesai | Penyelesaian berhasil menurut server. |
| Kajian | Tidak Ditandatangani | Dokumen terkunci tanpa tanda tangan; ikuti pesan dan jalur yang tersedia. |
| Visite | Tercatat | Kejadian visite tersimpan. |
| Visite | Dibatalkan | Kejadian tetap ada beserta pembatalannya. |

Dokter fiktif dr. Andi datang pukul 09.00 dan memasukkan data pukul 09.20. Waktu Visite adalah 09.00. Menulis SOAP pukul 09.20 tidak otomatis mencatat visite 09.00.

Peringatan visite yang berdekatan membantu meninjau kemungkinan duplikasi; dua kunjungan nyata pada hari yang sama tidak otomatis dilarang.

## Kendala dan hasil akhir

- Kajian ditolak karena bagian kosong: isi bagian yang disebut, bukan menyalin teks tanpa pemeriksaan.
- Dokter tidak dikenali/penugasan tidak sah: periksa profil dan penugasan.
- Data skrining gagal dimuat: bedakan kegagalan membaca dari tidak adanya hasil.
- Episode Closed: dokumentasi rutin hanya baca; jangan membuka episode baru untuk mengubah catatan lama.

Hasil akhir: kajian medis dan kejadian visite dapat dibaca ulang dengan penulis/waktu yang benar. Lanjutkan [SOAP dan CPPT](02-soap-cppt-dan-verifikasi.md).

Bukti: [DOKTER](../99-sumber-dan-status-panduan.md#dokter).

