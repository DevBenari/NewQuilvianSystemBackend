# Admisi Pasien Lama

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan prasyarat

Petugas admisi menyiapkan rawat inap bagi pasien yang sudah terdaftar. Siapkan nomor RM atau identitas pencarian, dokumen penjamin, kelas perawatan, unit tujuan, dan DPJP. Pastikan pasien tidak sedang dikerjakan melalui episode Draft yang seharusnya dilanjutkan.

## Langkah penggunaan

1. Buka Admisi Rawat Inap di `/health-services/inpatient-management/admissions`.
2. Pilih **Pendaftaran Pasien Lama**.
3. Cari dengan nomor RM atau NIK melalui pilihan pencarian yang tersedia. Pemindaian kartu digunakan bila fitur tersebut tersedia pada perangkat.
4. Pilih hasil yang benar. Pada **Informasi Pasien Lama**, cocokkan nama, nomor RM, dan identitas pendukung. Jangan memilih hanya berdasarkan kemiripan nama.
5. Buka **Tipe Pasien** dan pilih kategori yang sesuai. Bila Bayi Baru Lahir, pilih episode ibu aktif yang benar.
6. Pada **Pembayaran**, periksa cara bayar, penjamin, dan kelas. Kartu penjamin lama tetap perlu diperiksa kesesuaiannya.
7. Bila **Deposit** tampil, isi nominal sesuai kebutuhan. Baca [batas pencatatan deposit](../keuangan/01-deposit-tagihan-dan-status-kasir.md); angka di langkah ini belum menjadi bukti transaksi.
8. Pada **Dokter**, pilih unit, DPJP, catatan, dan kebutuhan isolasi. Simpan dan periksa nomor episode Draft.
9. Pada **Pilih Bed** dan **Booking Bed**, pilih tempat yang layak, lakukan pemesanan, lalu periksa masa berlakunya.
10. Pada **Konfirmasi**, cocokkan seluruh ringkasan dan lanjutkan ke **Cetak Persetujuan Pasien Ranap**.
11. Serahkan episode kepada petugas bangsal untuk penempatan saat pasien tiba.

Urutan source pasien lama: Pasien Lama → Informasi Pasien Lama → Tipe Pasien → Pembayaran → Deposit bila berlaku → Dokter → Pilih Bed → Booking Bed → Konfirmasi → Cetak Persetujuan. Jalur ini tidak mempunyai langkah Kartu Pasien.

## Pemeriksaan penting

| Bagian | Yang diperiksa | Contoh |
| --- | --- | --- |
| Hasil pencarian | RM dan identitas benar | Dua pasien bernama Budi dibedakan dengan RM CONTOH-001 dan CONTOH-002. |
| Episode yang sudah ada | Draft dilanjutkan, bukan dibuat ulang | Draft dari pagi ditemukan; gunakan Lanjutkan Admisi. |
| Penjamin | Kartu/perusahaan dan kelas sesuai | Polis lama tidak otomatis berarti boleh digunakan untuk kunjungan baru. |
| Bayi dan ibu | Hubungan episode ibu benar | Nomor episode ibu diperiksa sebelum lanjut. |

## Status dan hasil akhir

| Sebelum | Tindakan | Sesudah |
| --- | --- | --- |
| Pasien terdaftar, belum ada episode untuk admisi ini | Simpan langkah Dokter | Episode Draft terbentuk. |
| Draft | Pesan bed/konfirmasi/cetak | Tetap Draft. |
| Draft | Petugas mencatat penempatan pasien yang telah tiba | Admitted. |

Contoh: Budi sudah memiliki RM CONTOH-001. Petugas menggunakan RM tersebut untuk episode perawatan baru pada 6 Oktober. Sistem tidak memerlukan pendaftaran identitas pasien baru untuk menghasilkan episode baru.

## Kendala dan pembatalan

- **Pasien tidak ditemukan:** periksa ejaan/nomor/kriteria pencarian sebelum beralih ke pasien baru.
- **Identitas berbeda:** hentikan pemilihan; tindak lanjuti koreksi melalui pengelolaan data pasien yang berwenang.
- **Pasien memiliki permintaan kamar pulih yang masih aktif:** buka [jalur kamar pulih](03-pasien-dari-kamar-pulih.md), sehingga permintaan dan episode terhubung.
- **DPJP/penjamin salah setelah episode terbentuk:** jangan menganggap tombol Kembali membatalkan data. Baca [pelanjutan dan pembatalan](04-lanjutkan-atau-batalkan-admisi.md).
- **Kartu pasien hilang:** pencetakan ulang menggunakan layanan kartu pasien yang tersedia; jangan mendaftarkan identitas baru.

Hasil akhir: pasien lama ditautkan pada episode Draft yang benar, dengan penjamin, unit, DPJP, dan pemesanan yang dapat ditelusuri.

Lanjutkan ke [penempatan bed](../perawatan/01-tempat-tidur-transfer-dan-penanggung-jawab.md). Bukti: [ADMISI](../99-sumber-dan-status-panduan.md#admisi).

