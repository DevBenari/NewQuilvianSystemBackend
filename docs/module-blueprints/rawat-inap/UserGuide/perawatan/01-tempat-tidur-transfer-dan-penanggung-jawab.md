# Tempat Tidur, Transfer, dan Penanggung Jawab

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan prasyarat

Petugas admisi/bangsal yang berwenang mencatat lokasi pasien yang sebenarnya dan menjaga penugasan klinis. Proses dipicu oleh kedatangan pasien, perpindahan ruangan, pergantian DPJP, atau perubahan perawat penanggung jawab.

Siapkan episode yang benar, bed tujuan, alasan perubahan, dan waktu kejadian. Kebutuhan isolasi, unit, serta kelas harus sesuai hasil pemeriksaan server.

## Menempatkan pasien yang baru tiba

1. Buka Papan Tempat Tidur (`/health-services/inpatient-management/bed-board`) atau Detail Episode dari daftar episode.
2. Cocokkan RM, nomor episode Draft, unit, kelas, DPJP, dan kebutuhan isolasi.
3. Pastikan pasien benar-benar tiba. Pemesanan bed saja belum menyatakan pasien dirawat di bed tersebut.
4. Buka aksi penempatan/konfirmasi masuk yang tersedia pada layar.
5. Pilih episode dan bed yang benar. Jika ada reservasi, periksa pemilik serta masa berlakunya.
6. Tinjau ringkasan, kemudian simpan satu kali.
7. Muat ulang detail. Pastikan status episode Admitted, lokasi aktif benar, dan bed terisi.
8. Jika pasien berasal dari IGD, penuhi pencatatan kedatangan/serah terima pada proses IGD yang diperlukan server sebelum penempatan.

## Transfer bed atau ruangan

1. Buka Detail Episode atau Keperawatan → Transfer Pasien → Form Transfer.
2. Periksa lokasi aktif dan pastikan episode masih Admitted.
3. Pilih bed tujuan yang layak dan isi alasan, misalnya “Pasien dipindahkan sesuai kebutuhan isolasi”.
4. Baca dampak: penempatan lama berakhir, penempatan baru dibuka, dan bed lama dibebaskan.
5. Konfirmasi dan periksa History Transfer/riwayat penempatan. Pastikan hanya satu lokasi yang aktif.
6. Lakukan dokumentasi serah terima klinis melalui pekerjaan yang disediakan. Riwayat perpindahan bed tidak menggantikan komunikasi dan dokumentasi klinis.

## Penugasan dokter dan perawat

| Kebutuhan | Langkah |
| --- | --- |
| Mengalihkan DPJP | Buka penugasan dokter pada Detail Episode, pilih dokter tujuan, isi alasan, dan konfirmasi sesuai kewenangan. Periksa DPJP aktif sesudah simpan. |
| Menambah konsulen/dokter jaga | Gunakan penugasan dokter pendukung, pilih peran yang tepat, lalu periksa riwayatnya. |
| Mengakhiri penugasan pendukung | Pilih penugasan yang benar dan aksi akhir; beri alasan bila diminta. |
| Menetapkan perawat penanggung jawab | Buka penugasan perawat, pilih pegawai yang benar, lalu simpan dan periksa penugasan aktif. |

Riwayat penugasan tetap dapat ditelusuri. Mengetik nama petugas dalam catatan tidak membuat penugasan aktif.

## Status dan contoh

| Dari | Tindakan | Ke/hasil |
| --- | --- | --- |
| Draft | Penempatan pertama | Admitted; bed Occupied. |
| Reservasi Active | Dipakai untuk penempatan | Consumed. |
| Admitted | Transfer | Tetap Admitted; penempatan lama berakhir dan tujuan aktif. |
| DischargePending | Transfer | Ditolak; periksa proses pemulangan/koreksi. |
| Closed/Cancelled | Penempatan baru | Ditolak; bukan jalur membuka ulang perawatan. |

Contoh: Budi di bed CONTOH-A pada 09.00 dipindahkan ke CONTOH-B pada 14.00. Riwayat menunjukkan A berakhir dan B aktif; petugas tidak membuat episode kedua untuk perpindahan ini.

## Validasi, koreksi, dan hasil akhir

- Alasan transfer wajib; batas isian mengikuti layar. Kontrak request transfer membatasi alasan hingga 500 karakter.
- Bed yang terisi/tidak sesuai isolasi dapat ditolak. Muat ulang, jangan memaksa dengan mengubah atribut pasien yang sebenarnya.
- Pesan perubahan oleh petugas lain berarti data perlu dimuat ulang sebelum memilih tindakan.
- **Transfer** mencatat perpindahan nyata. **Koreksi penempatan** membetulkan catatan yang salah; jangan membuat transfer fiktif untuk memperbaiki waktu/kelas lama. Lihat [panduan koreksi](../bantuan/01-kendala-koreksi-dan-pertanyaan-umum.md).
- Pergantian penanggung jawab tidak mengubah status episode.

Hasil akhir: lokasi aktif, riwayat perpindahan, dan penugasan sesuai keadaan nyata. Lanjutkan [pengkajian](../keperawatan/01-pengkajian-dan-perencanaan-pulang.md) atau [kajian dokter](../dokter/01-daftar-pasien-kajian-medis-dan-visite.md).

Bukti dan API: [EPISODE](../99-sumber-dan-status-panduan.md#episode).

