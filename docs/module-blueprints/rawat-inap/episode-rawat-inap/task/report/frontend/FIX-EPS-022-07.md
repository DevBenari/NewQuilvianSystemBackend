# Laporan Perubahan Frontend — FIX-EPS-022-07

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Pembatalan sadar-lifecycle pada admisi bayi |
| Tanggal selesai source | 2026-10-16 |
| Status | SELESAI SOURCE |

## Hasil

Footer langkah bayi memakai handler dan modal yang telah tersedia pada admisi umum. Sebelum episode terbentuk, tombol kiri adalah **Batal Pendaftaran** dan hanya membersihkan state/session lokal. Setelah episode Draft atau parameter resume tersedia, tombol berubah menjadi **Batalkan Admisi**, mewajibkan alasan, memanggil endpoint cancel, dan memakai cleanup/reservation release existing. Langkah Dokumen Bayi tidak menampilkan reset pendaftaran.

## Matriks hasil

| Keadaan | Aksi |
| --- | --- |
| Konteks, Identitas, Perawatan, Pembayaran, Deposit sebelum episode | Batal Pendaftaran |
| Penempatan sebelum Draft dibuat | Batal Pendaftaran |
| Penempatan setelah Draft, Persetujuan Wali, Tinjau/Aktifkan | Batalkan Admisi |
| Resume Draft pada langkah yang dapat dibuka | Batalkan Admisi |
| Dokumen setelah aktivasi | Tidak ada reset lokal |

Komponen deposit, dokter, consent, dan helper `Actions` menerima callback opsional; default konsumen lama tidak berubah.

## Validasi

Unit test source memastikan kedua label tersedia, deposit khusus telah dihapus, dan ringkasan akhir tetap berada pada review. Modal cancel existing tetap menjaga alasan wajib, loading, error, serta pembebasan reservasi.

