# Pasien Keluar dan Penutupan Episode

Versi 1.0 · 6 Oktober 2026.

## Tujuan, pelaku, dan prasyarat

DPJP membuat keputusan pulang; perawat/admisi berwenang mencatat kepergian; petugas dengan hak Close menutup episode; kasir memutuskan izin keuangan. Pemicu adalah keputusan klinis dan kejadian pasien meninggalkan ruangan.

Siapkan episode yang benar, resume, checklist administrasi, status kasir, serta waktu kepergian bila pasien telah keluar.

## 1. Keputusan pulang dan resume

1. DPJP aktif membuka Keputusan Pulang dan Resume dari Detail Episode atau ruang kerja dokter.
2. Pilih cara pulang yang sesuai dan simpan keputusan melalui aksi yang tersedia.
3. Pastikan status menjadi DischargePending.
4. Lengkapi resume dan tanda tangan DPJP melalui [panduan resume](../dokter/04-resume-medis-dan-instruksi-pulang.md).
5. Perawat/admisi menyiapkan butir administrasi dan berkoordinasi dengan kasir.

## 2. Mencatat kepergian fisik

1. Saat pasien benar-benar meninggalkan ruangan, buka aksi **Catat pasien meninggalkan ruangan** pada Detail Episode.
2. Periksa identitas dan status DischargePending.
3. Isi waktu nyata. Waktu tidak boleh di masa depan atau sebelum keputusan pulang; waktu juga tidak boleh mendahului penempatan/pemakaian alat yang terkait.
4. Baca status kasir. Jika belum memberi izin/tidak terbaca, baca dialog peringatan sebelum menyatakan pengakuan dan melanjutkan pencatatan.
5. Konfirmasi satu kali setelah memeriksa kejadian.
6. Pastikan waktu kepergian tersimpan, bed dilepas, dan episode masih DischargePending.
7. Jika tindak lanjut penutupan alat gagal, ikuti peringatan. Jangan mengulangi kepergian yang sudah tersimpan.

Pencatatan ini tidak dapat dibatalkan lewat aksi rutin. Jangan menggunakannya ketika pasien masih menunggu atau hanya berencana pulang.

## 3. Penutupan episode

Buka halaman Penutupan Episode, `/health-services/inpatient-management/episodes/{id}/closure`. Baca kesiapan terbaru dari server.

| Syarat | Yang harus dipastikan | Dapat ditembus supervisor? |
| --- | --- | --- |
| Keputusan pulang | Episode DischargePending karena keputusan DPJP sudah ada. | Tidak. |
| Resume ditandatangani | Resume pulang memiliki tanda tangan yang sah. | Tidak. |
| Administrasi lengkap | Semua butir wajib yang menghalangi penutupan sudah ditandai. | Tidak. |
| Kasir memberi izin | Status terbaca dan CLEARED. | Ya, hanya jalur CloseOverride yang sah. |
| Keadaan bed jelas | Masih ada penempatan aktif yang sah atau kepergian telah dicatat. | Tidak. |

1. Tinjau setiap syarat dan daftar pekerjaan yang belum selesai.
2. Tandai butir administrasi hanya setelah pekerjaan nyata selesai; isi catatan bila diminta.
3. Baca ringkasan akibat penutupan, termasuk dokumen/pesanan yang belum selesai.
4. Jika semua syarat terpenuhi, gunakan aksi penutupan, baca dialog, lalu konfirmasi satu kali.
5. Periksa status Closed dan riwayatnya.
6. Jika data berubah setelah dibaca, muat ulang. Penutupan memakai versi data; jangan mengulang konfirmasi dengan ringkasan lama.

## Penutupan tanpa izin kasir

Supervisor dengan hak CloseOverride dapat memilih jalur tersebut bila satu-satunya syarat yang dapat dilewati adalah keuangan. Isi alasan yang jelas. Contoh: “Penutupan administratif disetujui supervisor; penyelesaian kasir ditindaklanjuti pada catatan terkait”.

Bila resume belum ditandatangani atau syarat lain belum terpenuhi, jalur ini tetap ditolak. Episode ditandai Ditutup tanpa izin kasir dan masuk daftar pantau. Tanda tersebut tidak menyatakan tagihan lunas.

## Status dan contoh waktu

| Waktu contoh | Kejadian | Status/akibat |
| --- | --- | --- |
| 10.00 | DPJP membuat keputusan pulang | DischargePending. |
| 12.00 | Pasien nyata keluar dan dicatat | Bed tersedia; tetap DischargePending. |
| 12.30 | Resume/administrasi/kasir diperiksa | Masih DischargePending bila belum lengkap. |
| 13.00 | Penutupan berhasil | Closed. |

Catatan kepergian pukul 09.00 tidak sah bila keputusan pulang baru pukul 10.00. Koneksi yang putus setelah simpan perlu ditindaklanjuti dengan membaca ulang detail, bukan membuat catatan kepergian kedua.

## Kendala dan hasil akhir

INP-DEP-001 meminta pengakuan peringatan izin kasir sebelum kepergian disimpan. INP-CLS-012 menolak alasan override kosong/tidak bermakna. Pesan syarat belum terpenuhi menunjukkan pekerjaan yang masih harus diselesaikan.

Setelah Closed, penugasan aktif berakhir dan dokumentasi rutin menjadi hanya baca. Proses penutupan dapat membatalkan pesanan yang belum dilaksanakan atau mengunci dokumen belum ditandatangani; periksa riwayat dan akibat yang ditampilkan.

Hasil akhir: episode Closed dengan jejak pelaku, waktu, dokumen, administrasi, dan status kasir. Untuk kesalahan sesudahnya, baca [koreksi](../bantuan/01-kendala-koreksi-dan-pertanyaan-umum.md).

Bukti dan API: [PEMULANGAN](../99-sumber-dan-status-panduan.md#pemulangan).

