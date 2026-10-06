# Dashboard, Sensus, Daftar Pantau, dan Laporan

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan pelaku

Kepala ruangan, admisi, kasir, dan petugas berwenang membaca kondisi layanan serta pekerjaan yang perlu ditindaklanjuti. Pemeriksaan dilakukan berkala dan saat ada informasi pasien/bed yang tidak sesuai.

Prasyarat: hak membaca halaman terkait dan sumber data berhasil dimuat.

## Membaca dashboard dan sensus

1. Buka Rawat Inap atau `/health-services/inpatient-management`.
2. Periksa kartu ringkasan dan tanggal/penyaring yang sedang digunakan.
3. Buka Sensus Rawat Inap (`/health-services/inpatient-management/census`).
4. Pilih unit, kelas, dan penyaring yang diperlukan.
5. Baca daftar pasien serta lokasi/DPJP/statusnya. Cocokkan episode sebelum membuka detail.
6. Bila angka tampak berbeda dari papan bed, bandingkan penyaring dan waktu pembacaan terlebih dahulu.

Pemesanan Draft, penempatan pasien aktif, pasien sudah keluar tetapi episode belum ditutup, dan episode Closed merupakan kelompok berbeda. Jangan menyamakan semua jumlah sebagai pasien yang sedang menempati bed.

## Membaca daftar pantau

Buka `/health-services/inpatient-management/monitoring`. Pilih kelompok, lalu buka detail pasien untuk menindaklanjuti pekerjaan sebenarnya.

| Kelompok | Tindak lanjut |
| --- | --- |
| Penutupan Tertunda | Periksa lima syarat penutupan dan pekerjaan yang masih kurang. |
| Ditutup Tanpa Izin Kasir | Kasir/supervisor meninjau tindak lanjut keuangan. |
| Pulang Sebelum Izin Kasir | Periksa kejadian keluar dan koordinasikan kasir; tidak sama dengan episode ditutup tanpa izin. |
| Belum Ada Penanggung Jawab | Periksa penugasan perawat. |
| Penempatan Tidak Sesuai Isolasi | Periksa atribut isolasi dan lokasi; gunakan perubahan yang benar. |
| Serah Terima Pasca Operasi Tertunda | Periksa daftar kiriman operasi dan penerimaan bangsal. |
| Permintaan Admisi Tertunda | Tindak lanjuti permintaan kamar pulih melalui jalur admisinya. |
| Kekurangan deposit/pesanan tertagih belum dilaksanakan | Periksa data dan status sumber; informasi kegagalan pembacaan bukan nilai nol. |

Daftar pantau adalah sarana tindak lanjut. Membuka daftar tidak otomatis memperbaiki data atau menahan seluruh tindakan pasien.

## Selisih tempat tidur dan laporan transfer

1. Buka Laporan Selisih Tempat Tidur di `/health-services/inpatient-management/bed-drift` bila angka/status bed tidak sesuai.
2. Bandingkan lokasi episode dengan status master bed dan riwayat kejadian. Jangan langsung mengedit master bed untuk menutupi ketidaksesuaian.
3. Untuk riwayat perpindahan, buka Laporan Rawat Inap (`/health-services/inpatient-management/reports`) dan pilih laporan transfer.
4. Tentukan rentang serta penyaring, lalu tinjau daftar.
5. Gunakan ekspor bila akun mempunyai hak ExportRoomTransfer. Hak membaca laporan tidak selalu mengizinkan ekspor.
6. Tangani berkas hasil sesuai tata kelola data pasien rumah sakit.

## Status, contoh, dan kendala

Halaman baca tidak mengubah status episode. Perubahan dilakukan melalui pekerjaan pemiliknya: penugasan, penempatan, verifikasi, pemulangan, atau Billing.

Contoh: Budi keluar pukul 12.00, tetapi episode baru ditutup pukul 13.00. Pada 12.30 bed telah kosong sementara daftar penutupan tertunda masih memuat Budi. Dua informasi itu dapat sama-sama benar.

Jika data gagal dimuat, periksa hak dan sumber yang gagal. Jika laporan kosong, periksa penyaring sebelum menyimpulkan tidak ada kejadian. Ambang terlambat mengikuti pengaturan; angka pada contoh bukan target rumah sakit.

## Hasil akhir dan rujukan

Petugas mengetahui episode dan pekerjaan yang perlu ditindaklanjuti, dengan penyaring/waktu pembacaan yang jelas. Lanjutkan ke proses pemilik melalui tautan detail pasien.

Bukti dan API: [OPERASIONAL](../99-sumber-dan-status-panduan.md#operasional).

