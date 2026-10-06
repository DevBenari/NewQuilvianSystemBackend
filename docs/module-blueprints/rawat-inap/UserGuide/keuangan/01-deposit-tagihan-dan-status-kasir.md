# Deposit, Tagihan, dan Status Kasir

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan pelaku

Petugas admisi/bangsal membaca informasi administrasi keuangan dan berkoordinasi dengan kasir. Kasir menangani penerimaan, invoice, pembayaran, refund, serta izin kasir melalui modul Billing.

Prasyarat: pasien/episode/kunjungan benar, penjamin tersedia, dan hak membaca informasi keuangan sesuai tugas.

## Deposit dalam alur admisi: batas yang harus dipahami

1. Pada Pembayaran, pilih cara bayar yang benar.
2. Source layar yang diperiksa menampilkan Deposit untuk Tunai/Umum; Asuransi/Perusahaan melewati langkah itu.
3. Bila tampil, isian Nominal Diterima menerima angka rupiah. Kosong pun dapat dilanjutkan.
4. Layar menyatakan minimum kebijakan belum dapat ditampilkan.
5. **Angka pada langkah ini merupakan isian lokal. Hook Deposit tidak mengirim penerimaan uang, dan hook pembentukan episode yang diperiksa tidak memanggil top-up deposit.**
6. Jangan menyatakan kwitansi/transaksi berhasil hanya karena angka sudah diketik atau episode berhasil dibuat.
7. Untuk uang yang benar-benar diterima, koordinasikan pencatatan melalui Billing oleh petugas berwenang. Periksa bukti transaksi pada pasien/kunjungan yang benar.

Contoh: petugas mengetik Rp500.000 pada admisi Budi. Tanpa transaksi dan bukti penerimaan Billing, jumlah itu belum boleh dianggap saldo deposit tercatat. Mengubah angka lokal menjadi Rp600.000 juga tidak menambah saldo sebesar Rp100.000.

## Membaca tagihan dan izin kasir

1. Buka Detail Episode atau Keperawatan → **Tagihan Pasien** sesuai hak.
2. Periksa pasien, episode, dan kunjungan yang ditautkan.
3. Baca kartu status kasir, alasan kendala, dan waktu pembacaan yang tersedia.
4. Untuk rincian nominal/pembayaran/invoice, petugas kasir membuka modul Billing yang terkait. Kartu status operasional bangsal tidak digunakan sebagai ledger nominal.
5. Setelah kasir melakukan tindak lanjut, baca ulang status. Jangan menandai sendiri Disetujui kasir di bangsal.
6. Bila pasien akan keluar, ikuti [pencatatan kepergian](../pemulangan/01-pasien-keluar-dan-penutupan-episode.md).
7. Sebelum penutupan, baca kesiapan terbaru; izin kasir dapat berubah setelah sebelumnya disetujui.

## Arti status kasir

| Nilai | Label layar | Tindak lanjut |
| --- | --- | --- |
| PENDING | Menunggu kasir | Koordinasikan pemeriksaan/penyelesaian Billing. |
| BLOCKED | Terkendala | Baca alasan dan hubungi kasir. |
| CLEARED | Disetujui kasir | Salah satu syarat penutupan terpenuhi pada pembacaan itu. |
| REVOKED | Izin dicabut | Perlu pemeriksaan ulang kasir; persetujuan lama tidak dipakai. |
| UNREADABLE | Status kasir tidak dapat dibaca | Sistem tidak dapat memverifikasi izin saat itu. Jangan menganggap saldo nol/lunas. |

Status ini berbeda dari Draft/Admitted/Closed yang merupakan status episode. Label lama Lunas pada layar kelayakan keuangan tidak memberi wewenang mengubah izin kasir; sumber keputusan tetap Billing.

## Tarif kamar, koreksi, dan operasi

- Tarif kamar dihitung Billing berdasarkan linimasa penempatan. Petugas bangsal tidak menghitung ulang tarif sebagai transaksi kedua.
- Salah kelas/waktu penempatan menggunakan [koreksi penempatan](../bantuan/01-kendala-koreksi-dan-pertanyaan-umum.md). Invoice yang sudah final membatasi koreksi; koordinasikan kasir.
- Jika invoice Perlu Diperiksa atau tarif belum ditemukan, kasir meninjau di Billing. Jangan memasukkan biaya kamar manual tambahan sebagai cara menutupi kegagalan tarif otomatis.
- Pada admisi kamar pulih, periksa hubungan kunjungan operasi asal dan episode tujuan jika biaya belum terlihat. Riwayat terkait tidak membenarkan biaya ganda.

## Keluar ruangan dan penutupan

Pasien yang sudah benar-benar keluar tetap dapat dicatat melalui peringatan ketika izin kasir belum ada/tidak terbaca. Bed dilepas; episode masih DischargePending. Penutupan biasa tetap memerlukan izin kasir yang terbaca.

Supervisor yang berwenang mempunyai jalur penutupan tanpa izin kasir dengan alasan. Jalur ini tidak menghapus piutang/tagihan dan tidak menggantikan empat syarat penutupan lainnya.

## Hasil akhir dan rujukan

Petugas memahami status kasir yang sebenarnya dan setiap uang/invoice ditelusuri melalui transaksi Billing. Kartu yang gagal dibaca atau angka Deposit lokal tidak diperlakukan sebagai bukti pelunasan.

Bukti, API, dan batas integrasi: [BILLING](../99-sumber-dan-status-panduan.md#billing).

