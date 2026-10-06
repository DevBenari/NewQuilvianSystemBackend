# Order dan Hasil Penunjang Medis

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan prasyarat

Dokter/perawat berwenang memesan layanan penunjang, menindaklanjuti instruksi, dan membaca hasil. Pemicu adalah instruksi pemeriksaan atau pelayanan yang dibutuhkan pasien.

Siapkan episode, penugasan, jenis pemeriksaan, prioritas/waktu, serta instruksi dokter yang benar bila pesanan dibuat perawat. Pilihan dan layanan yang aktif mengikuti modul pemiliknya.

## Membuka pekerjaan

- Dokter: pasien pada Dokter Rawat Inap → **Penunjang Medis**.
- Perawat: Keperawatan → **Penunjang Medis**, kemudian pilih Radiologi, Laboratorium, Rehab Medik, Konsultasi Gizi, Hemodialisa, atau Bank Darah.

| Layanan | Yang diperiksa |
| --- | --- |
| Laboratorium | Jenis/panel pemeriksaan, prioritas, instruksi, status pesanan, dan hasil. |
| Radiologi | Pemeriksaan, informasi yang diminta formulir, status, dan hasil/laporan. |
| Konsultasi Gizi/diet | Permintaan yang benar dan status verifikasi instruksi; pesanan konsultasi tidak sama dengan diet pasien. |
| Hemodialisa | Permintaan dan kaitannya dengan pelayanan yang ditampilkan. |
| Bank Darah | Jenis kebutuhan darah/komponen dan instruksi; pemantauan transfusi dilakukan per kantong. |
| Rehab Medik | Periksa ketersediaan panel pada profesi yang digunakan. Panel perawat pada source masih menyatakan integrasi belum tersedia. Jangan menganggap ketersediaannya sama dengan layar dokter. |

## Membuat dan menindaklanjuti pesanan

1. Periksa nama, RM, episode, dan alergi.
2. Pilih layanan dan buka formulir pesanan.
3. Pilih pemeriksaan/layanan dari katalog yang tersedia, lalu isi rincian wajib.
4. Jika perawat membuat pesanan atas instruksi, isi informasi dokter/instruksi sesuai formulir.
5. Simpan satu kali, lalu periksa pesanan dalam riwayat dan nomor/status yang dikembalikan.
6. Tindak lanjuti pesanan yang Menunggu Verifikasi melalui dokter berwenang. Ada hak dan proses terpisah untuk verifikasi Tindakan, Lab, Radiologi, Gizi, Bank Darah, serta Diet.
7. Setelah pelayanan diproses modul penunjang, buka riwayat/hasil pada pasien yang benar.
8. Periksa waktu, status finalisasi, dan rincian hasil. “Order tersimpan” tidak berarti “hasil tersedia”.
9. Bila hasil gagal dimuat, muat ulang sumber tersebut; jangan mencatatnya sebagai hasil kosong/normal.

## Pemantauan transfusi oleh perawat

1. Buka Bank Darah pada Penunjang Medis dan pilih pesanan/kantong yang benar.
2. Pastikan kantong dapat dipilih menurut status yang diberikan modul Bank Darah.
3. Gunakan aksi mulai pemantauan dan catat data/waktu sesuai formulir.
4. Tambahkan observasi yang benar-benar dilakukan pada rangkaian kantong tersebut.
5. Jika terjadi reaksi, isi pencatatan reaksi dan tindak lanjut yang tersedia; komunikasikan sesuai prosedur klinis yang berlaku.
6. Setelah proses selesai, gunakan aksi penyelesaian pemantauan; jangan menandai Completed sebelum selesai.
7. Periksa riwayat kantong. Kantong lain memerlukan rangkaian sendiri.

## Status, contoh, dan kendala

| Peristiwa | Makna |
| --- | --- |
| Pesanan dibuat | Permintaan tersedia; pelayanan belum tentu dimulai. |
| Instruksi menunggu verifikasi | Perlu tindak lanjut dokter; bukan izin mengabaikan pemeriksaan instruksi. |
| Hasil belum tersedia | Periksa status pemrosesan, bukan membuat hasil sendiri. |
| Pemantauan transfusi Completed | Rangkaian pemantauan kantong itu selesai; bukan semua kantong pasien. |
| Pesanan dibatalkan/ditolak | Baca alasan dan jangan mengeksekusi pesanan lama. |

Contoh: Budi mempunyai dua kantong. Observasi kantong pertama pukul 11.00 dicatat pada kantong pertama; hasilnya tidak disalin menjadi observasi kantong kedua.

Pesanan duplikat dapat terjadi bila pengguna mengulang setelah koneksi putus. Periksa riwayat sebelum mengirim lagi. Perubahan/penolakan mengikuti modul pemilik dan hak akun; pasien tidak perlu dibuat ulang untuk memesan layanan kedua.

## Hasil akhir dan rujukan

Pesanan, verifikasi instruksi, hasil, dan rangkaian pemantauan dapat ditelusuri pada episode yang benar. Lanjutkan [catatan dokter](../dokter/02-soap-cppt-dan-verifikasi.md) atau [pemantauan keperawatan](../keperawatan/04-pemantauan-khusus.md).

Bukti dan API: [PENUNJANG](../99-sumber-dan-status-panduan.md#penunjang).

