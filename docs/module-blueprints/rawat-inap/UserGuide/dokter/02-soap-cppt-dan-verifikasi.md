# SOAP, CPPT, dan Verifikasi DPJP

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan prasyarat

Dokter mencatat perkembangan pasien serta memverifikasi catatan lintas profesi yang menjadi kewenangannya. **SOAP** berisi Subjective, Objective, Assessment, Plan. **CPPT** berarti Catatan Perkembangan Pasien Terintegrasi.

Prasyarat: pasien/episode benar, penugasan dokter sah, dan hak menulis/verifikasi sesuai. Hanya DPJP aktif yang memenuhi pemeriksaan kewenangan verifikasi CPPT pada episode tersebut.

## Menulis SOAP

1. Buka pasien pada Dokter Rawat Inap → **SOAP**.
2. Periksa Form SOAP dan Riwayat SOAP. Buka dokumen yang masih draf ketika melanjutkan pekerjaan yang sama.
3. Isi Subjective, Objective, Assessment, Plan, serta data lain yang diminta formulir.
4. Pilih diagnosis melalui pilihan yang tersedia; periksa kesesuaian istilah/kode, jangan hanya memasukkan kode tanpa memahami pilihannya.
5. Simpan draf jika masih dikerjakan.
6. Tinjau validasi akhir, kemudian gunakan aksi penyelesaian/finalisasi sesuai kewenangan.
7. Periksa bahwa dokumen final tercatat pada riwayat. Koreksi final menggunakan addendum yang tersedia.
8. Buka Catatan Dokter bila ingin membaca catatan terkait; tetap periksa jenis dokumennya.

## Membaca dan memverifikasi CPPT

1. Buka tab **CPPT** atau daftar **Perlu Diperiksa** bila tersedia bagi akun.
2. Pilih penyaring tanggal, profesi, jenis catatan, atau status verifikasi sesuai kebutuhan.
3. Baca identitas pasien, waktu, penulis, isi, serta status catatan.
4. Jika perlu verifikasi, pastikan akun merupakan DPJP aktif. Hak menu saja tidak cukup.
5. Gunakan aksi **Verifikasi** pada catatan yang benar setelah pemeriksaan.
6. Muat ulang dan periksa penanda/verifikator/waktu verifikasi.
7. Bila isi perlu dibetulkan, gunakan proses koreksi yang tersedia atau tindak lanjuti dengan penulis; jangan menimpa catatan profesi lain.

Verifikasi CPPT dan verifikasi instruksi tindakan/penunjang merupakan pekerjaan berbeda. Keduanya tidak boleh dianggap selesai oleh satu tombol yang sama.

## Status dan aturan

| Sebelum | Aksi | Sesudah/hasil |
| --- | --- | --- |
| SOAP Draf/Sedang ditulis | Simpan | Tetap dokumen yang dapat dilanjutkan sesuai hak. |
| SOAP lengkap | Finalisasi | Final/Completed menurut server. |
| SOAP Final | Addendum | Tetap final dengan catatan tambahan dan jejak koreksi. |
| CPPT belum diverifikasi | Verifikasi oleh DPJP aktif | Penanda verifikasi dan pelaku tercatat. |
| CPPT belum diverifikasi | Verifikasi oleh dokter yang bukan DPJP aktif | Ditolak oleh pemeriksaan kewenangan. |

Dokumen Tidak Ditandatangani tidak sama dengan Final yang ditandatangani. Baca penanda keutuhan dokumen dan pesan layar.

## Contoh dan kendala

Perawat Wati menulis CPPT Budi pada 10.00. dr. Andi, DPJP aktif, memeriksanya pada 10.30 dan melakukan verifikasi. dr. Rina yang hanya menjadi dokter jaga dapat memiliki hak membaca, tetapi hubungan penugasannya tidak otomatis mengizinkan verifikasi sebagai DPJP.

Jika simpan/verifikasi mengalami gangguan koneksi, buka kembali riwayat sebelum mengulang. Jika catatan sudah final dan ditemukan kekeliruan, tambah koreksi beralasan sesuai hak penulis. Jika waktu/penulis tidak cocok, periksa dokumen dan episode sebelum bertindak.

## Hasil akhir dan rujukan

SOAP menunjukkan perkembangan yang terdokumentasi; CPPT menunjukkan siapa yang menulis dan siapa yang memverifikasi. Dokumen tidak diperlakukan sebagai selesai hanya karena formulir telah ditutup.

Lanjutkan [resep/tindakan](03-resep-obat-dan-tindakan.md) atau [resume](04-resume-medis-dan-instruksi-pulang.md). Bukti dan API: [CATATAN DOKTER](../99-sumber-dan-status-panduan.md#catatan-dokter).

