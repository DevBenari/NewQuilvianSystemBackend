# Master Data dan Pengaturan Rawat Inap

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan pelaku

Administrator/pemilik pengaturan yang berwenang menyiapkan data pendukung dan parameter operasional. Proses dipicu oleh pembukaan unit/kamar/bed, perubahan kebijakan yang disetujui, atau pilihan data yang tidak tersedia.

Prasyarat: hak master/pengaturan, nilai kebijakan yang telah disetujui pemiliknya, serta pemahaman dampak pada pasien aktif. Panduan ini tidak menetapkan kebijakan klinis atau tarif baru.

## Persiapan data pendukung

| Data | Yang diperiksa |
| --- | --- |
| Unit rawat inap | Aktif, tipe/unit sesuai, konfigurasi antrean sesuai rawat inap. |
| Kelas, kamar, bed | Nama/kode, hubungan unit/kelas, status aktif, atribut yang diperlukan penempatan. |
| Dokter/perawat | Profil petugas dan pilihan penugasan terbaca. |
| Penjamin | Kategori bayar, data kartu/perusahaan, dan hubungan pasien tersedia. |
| Instrumen klinis | Versi dan policy yang disetujui dapat dipilih untuk konteks pasien. |
| Tindakan/obat/alat | Katalog tersedia; data ditangani modul pemiliknya. |
| Tarif Billing | Tarif dan lingkupnya disiapkan pada Billing, bukan dihitung ulang melalui pengaturan bangsal. |

## Mengubah Pengaturan Rawat Inap

1. Buka `/health-services/inpatient-management/settings`.
2. Baca nilai yang sedang berlaku dan satuan setiap parameter.
3. Pilih parameter yang memang mendapat persetujuan perubahan.
4. Isi nilai baru sesuai satuan dan batas yang ditampilkan.
5. Tinjau ringkasan/peringatan dampak sebelum simpan.
6. Simpan satu kali, muat ulang, dan periksa nilai yang kembali dari server.
7. Koordinasikan perubahan yang memengaruhi cara kerja/tenggat dengan petugas terkait.

Contoh rumpun parameter adalah masa pemesanan bed, kedaluwarsa Draft, ambang daftar pantau, dan penomoran episode. Nilai serta batas tepatnya mengikuti form/validasi versi yang terpasang.

**Contoh satuan:** 30 menit berbeda dari 30 jam. Jika kebijakan yang disetujui menyatakan 30 menit, jangan mengisi 30 pada parameter berunit jam.

## Mengelola butir administrasi penutupan

1. Buka daftar butir administrasi di `/health-services/inpatient-management/clearance-items`.
2. Periksa butir existing sebelum membuat nama yang sama.
3. Buka Buat atau Perbarui sesuai kebutuhan/hak.
4. Isi nama, urutan, lingkup/ketentuan wajib, dan isian lain yang tersedia.
5. Simpan dan periksa hasil pada daftar.
6. Periksa penggunaan pada checklist episode melalui petugas yang berwenang.

Membuat/mengubah definisi butir tidak berarti butir pasien sudah diselesaikan. Penandaan pekerjaan nyata dilakukan pada checklist episode.

## Status, validasi, dan jalur tidak normal

| Perubahan | Hasil |
| --- | --- |
| Simpan pengaturan sah | Nilai pengaturan diperbarui; status episode tidak otomatis berubah oleh aksi simpan pengguna. |
| Perubahan master | Data aktif/pilihan mengikuti modul pemilik dan penggunaan datanya. |
| Definisi butir baru | Butir tersedia sesuai lingkup; belum merupakan penandaan selesai pasien. |
| Nilai di luar batas | Ditolak; baca satuan/rentang dan perbaiki. |

- Data yang sedang dipakai dapat mempunyai pembatasan perubahan/penghapusan. Baca penolakan server.
- Pilihan bed kosong tidak otomatis berarti perlu membuat bed baru; periksa status/unit/kelas dan pemesanan.
- Instrumen belum disetujui tidak diaktifkan melalui catatan pasien.
- Perubahan masa kedaluwarsa tidak dijadikan alasan menghapus jejak Draft; pembatalan/pelanjutan memakai alur episode.
- Bila akun tidak mempunyai hak Update, minta pemilik pengaturan yang berwenang; jangan memakai akses langsung database.

## Hasil akhir dan rujukan

Master dan pengaturan sesuai keputusan yang disetujui, dapat dibaca kembali, serta dipahami petugas pengguna.

Lanjutkan [persiapan dan hak akses](../01-persiapan-dan-hak-akses.md). Bukti dan API: [ADMINISTRASI](../99-sumber-dan-status-panduan.md#administrasi).

