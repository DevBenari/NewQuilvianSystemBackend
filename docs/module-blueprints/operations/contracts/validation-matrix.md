# Validation Matrix — Modul Operasi

Contract `opr-validation-v1`; status `approved`; approved by pemilik kebutuhan pada 2026-08-21.

| Kode | Aturan | Berlaku pada | Kondisi | Pesan pengguna |
|---|---|---|---|---|
| `OPR001` | Tindakan utama wajib | Create case | Tidak ada procedure utama | “Pilih satu tindakan utama.” |
| `OPR002` | Tindakan tidak boleh diproses dua kasus aktif | Create/update case | Procedure sudah terkait kasus aktif | “Tindakan sudah diproses pada kasus operasi lain.” |
| `OPR003` | Jadwal tidak boleh bentrok | Schedule | Ruang/tim overlap termasuk buffer | “Ruang atau anggota tim sudah memiliki jadwal pada waktu tersebut.” |
| `OPR004` | Tim minimum wajib | Schedule | Peran minimum belum lengkap | “Lengkapi dokter bedah, dokter anestesi, perawat instrumen, dan perawat sirkuler.” |
| `OPR005` | Tenaga harus aktif/berwenang | Schedule/start | Resolver tenaga menolak | “Anggota tim tidak aktif atau tidak memiliki kewenangan yang sesuai.” |
| `OPR006` | Consent/checklist valid | Ready | Belum lengkap dan bukan bypass darurat | “Persiapan pasien belum lengkap.” |
| `OPR007` | Bypass darurat beralasan | Ready | Alasan/penanggung jawab kosong | “Lengkapi alasan dan penanggung jawab jalur darurat.” |
| `OPR008` | Quantity positif | Material usage | Quantity ≤ 0 | “Jumlah pemakaian harus lebih dari nol.” |
| `OPR009` | Serial implant wajib | Material usage | Implant tanpa serial/batch | “Lengkapi batch atau nomor serial implant.” |
| `OPR010` | Final record immutable | Update execution | Record sudah final | “Catatan final hanya dapat diperbaiki melalui addendum.” |
| `OPR011` | Handover harus diterima | Complete | Penerima/waktu belum ada | “Serah terima pasien belum diterima unit tujuan.” |
| `OPR012` | Version wajib cocok | Semua command | Data sudah berubah | “Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.” |
| `OPR013` | Idempotency wajib | Integrasi/command material | Key kosong/duplikat tidak cocok | “Permintaan tidak dapat diverifikasi sebagai permintaan yang sama.” |

Semua timestamp disimpan UTC dan ditampilkan sesuai zona waktu fasilitas. Data klinis tidak boleh masuk custom logger.
