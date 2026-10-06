# Panduan Penggunaan Rawat Inap Quilvian

Versi 1.0 · 6 Oktober 2026 · Blueprint RWI-BP-001.

Panduan ini membantu petugas menyelesaikan pekerjaan dari admisi sampai penutupan episode. **Episode** adalah satu rangkaian perawatan rawat inap; pasien yang sama dapat memiliki beberapa episode pada waktu berbeda. Selalu cocokkan nomor rekam medis, nama pasien, dan episode sebelum bekerja.

## Mulai di sini

1. Baca [persiapan dan hak akses](01-persiapan-dan-hak-akses.md).
2. Buka [alur lengkap rawat inap](00-alur-lengkap-rawat-inap.md).
3. Pilih panduan sesuai pekerjaan pada tabel di bawah.
4. Ikuti tautan proses berikutnya pada akhir setiap panduan.

## Daftar isi berdasarkan pekerjaan

| Kelompok | Panduan |
| --- | --- |
| Pengantar | [Alur lengkap](00-alur-lengkap-rawat-inap.md) · [Persiapan dan hak akses](01-persiapan-dan-hak-akses.md) |
| Admisi | [Pasien baru](admisi/01-pasien-baru.md) · [Pasien lama](admisi/02-pasien-lama.md) · [Kamar pulih](admisi/03-pasien-dari-kamar-pulih.md) · [Lanjutkan atau batalkan](admisi/04-lanjutkan-atau-batalkan-admisi.md) |
| Pengelolaan bangsal | [Tempat tidur, transfer, dan penanggung jawab](perawatan/01-tempat-tidur-transfer-dan-penanggung-jawab.md) |
| Keperawatan | [Pengkajian dan rencana pulang](keperawatan/01-pengkajian-dan-perencanaan-pulang.md) · [Asuhan dan catatan harian](keperawatan/02-rencana-asuhan-dan-catatan-harian.md) · [Obat dan alat](keperawatan/03-pemberian-obat-dan-pemakaian-alat.md) · [Pemantauan khusus](keperawatan/04-pemantauan-khusus.md) |
| Dokter | [Pasien, kajian, dan visite](dokter/01-daftar-pasien-kajian-medis-dan-visite.md) · [SOAP, CPPT, dan verifikasi](dokter/02-soap-cppt-dan-verifikasi.md) · [Resep dan tindakan](dokter/03-resep-obat-dan-tindakan.md) · [Resume dan instruksi pulang](dokter/04-resume-medis-dan-instruksi-pulang.md) |
| Layanan terkait | [Order dan hasil penunjang](layanan-terkait/01-order-dan-hasil-penunjang.md) · [Operasi dan serah terima](layanan-terkait/02-operasi-dan-serah-terima-pascaoperasi.md) |
| Keuangan | [Deposit, tagihan, dan status kasir](keuangan/01-deposit-tagihan-dan-status-kasir.md) |
| Pemulangan | [Pasien keluar dan penutupan episode](pemulangan/01-pasien-keluar-dan-penutupan-episode.md) |
| Operasional | [Dashboard, sensus, dan daftar pantau](operasional/01-dashboard-sensus-dan-daftar-pantau.md) · [Master data dan pengaturan](operasional/02-master-data-dan-pengaturan.md) |
| Bantuan | [Kendala, koreksi, dan pertanyaan umum](bantuan/01-kendala-koreksi-dan-pertanyaan-umum.md) |
| Pemeliharaan panduan | [Sumber, API pendukung, dan status verifikasi](99-sumber-dan-status-panduan.md) |

## Jalur baca menurut peran

| Peran | Urutan yang disarankan |
| --- | --- |
| Petugas admisi | Persiapan → pasien baru/lama/kamar pulih → penempatan bed → keuangan → penutupan |
| Perawat | Penugasan pasien → pengkajian → asuhan → obat/alat → penunjang → kepergian pasien |
| Dokter | Daftar pasien → kajian → SOAP/CPPT → resep/tindakan → penunjang → resume/keputusan pulang |
| Kepala ruangan | Penempatan/transfer → penugasan → daftar pantau → koreksi sesuai kewenangan |
| Kasir | Deposit/tagihan → status izin kasir → daftar pasien keluar sebelum izin kasir |
| Administrator | Persiapan hak akses → master data/pengaturan → kendala dan sumber bukti |

Nama peran menjelaskan pekerjaan. Hak menjalankan tindakan tetap ditentukan akun, penugasan pada episode, dan pemeriksaan server. Tombol yang terlihat belum tentu dapat dijalankan pada semua pasien.

## Cara membaca contoh dan status

Semua nama pasien, nomor rekam medis berawalan CONTOH, nama petugas, kamar, waktu, dan nilai transaksi di panduan merupakan data fiktif. Contoh menjelaskan pencatatan aplikasi; pilihan terapi mengikuti instruksi klinis yang sah.

| Istilah | Makna |
| --- | --- |
| Draft | Admisi sedang disiapkan; belum ada penempatan pasien yang mengaktifkan perawatan. |
| Admitted | Pasien telah ditempatkan dan episode berjalan. |
| DischargePending | DPJP telah membuat keputusan pulang; pekerjaan pemulangan belum seluruhnya selesai. |
| Closed | Episode sudah ditutup. |
| Cancelled | Admisi dibatalkan; episode itu tidak dapat dilanjutkan. |
| Simpan draf | Data tersimpan tetapi belum tentu selesai, final, atau ditandatangani. |
| Addendum | Catatan tambahan/koreksi dengan jejak penulis dan alasan; catatan awal tetap dapat ditelusuri. |

## Dasar dan batas verifikasi

Langkah disusun dari source backend/frontend dan kontrak yang diperiksa pada 6 Oktober 2026. Pemeriksaan source bukan bukti bahwa semua fitur telah aktif pada lingkungan rumah sakit. Pengujian langsung antarmuka, akun peran, printer, dan transaksi pada lingkungan target belum dijalankan dalam pekerjaan dokumentasi ini.

Batas yang berpengaruh pada penggunaan dijelaskan pada bab terkait, termasuk deposit yang masih berupa isian lokal, Resume ODC yang belum terhubung, dan perbedaan ketersediaan Rehabilitasi Medik antara layar dokter dan perawat. Lihat [status rinci](99-sumber-dan-status-panduan.md).

