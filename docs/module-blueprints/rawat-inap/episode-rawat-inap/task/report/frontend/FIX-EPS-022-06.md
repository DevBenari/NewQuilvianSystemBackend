# Laporan Perubahan Frontend — FIX-EPS-022-06

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Pemulihan ramah ketika unit bed menjadi tidak valid |
| Pasangan backend | [FIX-EPS-022-06](../backend/FIX-EPS-022-06.md) |
| Tanggal selesai source | 2026-10-16 |
| Status | SELESAI SOURCE; race condition API nyata belum disimulasikan |

## Hasil

Pada kegagalan pembentukan encounter yang menyatakan service unit tidak valid/tidak aktif/tidak tersedia, controller bed tidak lagi meneruskan pesan teknis mentah. Pilihan bed invalid dibersihkan, daftar bed dimuat ulang, seluruh input dokter/pembayaran dipertahankan, dan pengguna melihat pesan untuk memilih bed lain. Tidak ada submit ulang otomatis.

## Catatan kontrak

Backend saat ini masih mengirim pesan, belum error code terstruktur. Pencocokan teks dipersempit pada pesan kontrak existing dan dicatat sebagai residual. Filter backend yang baru mencegah kasus normal; recovery frontend menangani data yang berubah setelah daftar dimuat.

## Acceptance

- Tetap pada langkah pemilihan bed.
- Hanya pilihan bed yang dilepas.
- Daftar bed di-refresh.
- Form lain tidak di-reset.
- Pesan ramah menggantikan pesan teknis.

