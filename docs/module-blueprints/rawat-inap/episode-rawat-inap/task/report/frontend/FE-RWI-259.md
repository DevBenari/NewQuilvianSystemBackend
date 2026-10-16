# Laporan Perubahan Frontend — `FE-RWI-259`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Penjamin turunan, ringkasan final, dan pemulihan bed |
| Kontrak | `RWI-BAYI-022-2.0.0` |
| Tanggal | 16 Oktober 2026 |
| Status | **Selesai source; verifikasi live tertunda** |

## Hasil

Payload encounter bayi mengirim referensi dan versi sumber penjamin ibu; field payer manual
dibuat kosong. Tidak ada input nominal deposit maupun penggabungan invoice. Identitas lengkap,
DPJP, bed, penjamin turunan, dan informasi bahwa tidak ada deposit tambahan baru tampil pada
langkah konfirmasi. Recovery service-unit existing membersihkan bed dan memuat ulang daftar
tanpa membuang data ibu, bayi, atau DPJP.

## Bukti dan verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Targeted unit admisi bayi | 9/9 lulus; tiga skenario `FE-RWI-259` lulus |
| Targeted ESLint `--quiet` | Lulus |
| Next.js production build | Lulus, 498 halaman |
| Full unit suite | 2.908/2.936 lulus; 28 failure berada di CRM, menu, keperawatan, operasi, dan fitur lain; tidak ada failure admisi bayi |
| Live browser/API | Belum dijalankan karena migration backend belum diterapkan |

Dependency alokasi deposit ibu tetap milik Billing/Kasir/AR. Tombol pembatalan tidak diubah.
