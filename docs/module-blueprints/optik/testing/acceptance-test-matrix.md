# Optik — Matriks Uji Penerimaan

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| Revision | `1` — `draft` |
| Masukan | `OPT-AC-001`..`029` (`00-interview-decisions.md` r4), seluruh kontrak `v1` |

Jenis test: **U** = unit, **I** = integrasi backend (harness), **K** = kontrak antar-modul,
**E** = end-to-end (UI + backend). Data pada skenario adalah samaran.

| Requirement | Skenario | Jalur | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `OPT-AC-001` | Petugas penjualan menyimpan resep | Gagal | I | `403` `OPT-VAL-001`; tidak ada baris `OptPrescription` |
| `OPT-AC-002` | SIP berakhir 31-10-2026, simpan resep 03-11-2026 WIB | Gagal | U + I | `403` `OPT-VAL-002` |
| `OPT-AC-002` | SIP berakhir 03-11-2026, simpan resep 03-11-2026 pukul 23.30 WIB | Berhasil | U | Diterima (perbandingan tanggal WIB, bukan jam UTC) |
| `OPT-AC-003` | Revisi resep belum terkunci | Berhasil | I | Versi 2 tersimpan; versi 1 tetap terbaca beserta pembuat |
| `OPT-AC-004` | Revisi resep yang dipakai pesanan `InFabrication` | Gagal | I | `409` `OPT-VAL-022` |
| `OPT-AC-005` | Bagian pasien Rp 1.050.000, terbayar Rp 500.000 → kirim | Gagal | I | `409` `OPT-VAL-046` "minimum Rp 525.000" |
| `OPT-AC-005` | Terbayar Rp 525.000 → kirim | Berhasil | I | Status `InFabrication`; resep terkunci |
| `OPT-AC-006` | Ubah persentase 50 → 30 sesudah pesanan A dibuat | Berhasil | I | Pesanan A tetap minimum 50%; pesanan B baru 30% |
| `OPT-AC-007` | Petugas penjualan batal langsung dari `InFabrication` | Gagal | I | `409` |
| `OPT-AC-007` | Kepala Optik menyetujui pengajuan batal | Berhasil | I | Status `Cancelled`; baris frame batal; stok frame +1 |
| `OPT-AC-008` | Tombol kirim ke pengerjaan ditekan dua kali (kunci sama) | Berhasil | I | Stok berkurang tepat 1; satu baris riwayat |
| `OPT-AC-008` | Dua petugas mengirim pesanan yang sama bersamaan (`ExpectedVersion` sama) | Gagal | I | Satu berhasil, satu `409` `OPT-VAL-058` |
| `OPT-AC-009` | Serahkan saat belum lunas | Gagal | I | `409` `OPT-VAL-050` |
| `OPT-AC-010` | Penjualan langsung cairan Rp 85.000 | Berhasil | E | `Draft` → tertagih → lunas → `HandedOver`; stok −1 saat serah |
| `OPT-AC-011` | Diskon 15% (batas 10%) | Gagal (tertahan) | K | Diskon berstatus menunggu persetujuan; tagihan belum berkurang |
| `OPT-AC-012` | Pesanan memakai resep berusia 13 bulan | Gagal | U + I | `409` `OPT-VAL-023` |
| `OPT-AC-013` | Keluhan ukuran hari ke-10, diperiksa, disetujui | Berhasil | I | Pesanan remake `IsRemake`, tanpa fakta tagihan; stok dipotong saat dikirim |
| `OPT-AC-014` | Rekam Medis membaca pemeriksaan dan resep | Berhasil | K | Visus, refraksi, dan resep versi terakhir; tanpa harga |
| `OPT-AC-016` | Diskon 10% | Berhasil | K | Diskon tercatat di baris tagihan frame |
| `OPT-AC-017` | Batal sesudah lensa custom dipesan (frame Rp 400.000 + lensa Rp 800.000, DP Rp 600.000) | Berhasil | I + K | Baris frame batal; baris lensa tetap Rp 800.000; sisa piutang Rp 200.000 di Billing |
| `OPT-AC-018` | Dua pesanan berframe sama di satu kunjungan | Berhasil | K | Dua baris tagihan terpisah, masing-masing tertaut item pesanannya |
| `OPT-AC-019` | Pesanan kedua dikirim saat pesanan pertama belum lunas | Gagal | I | `409` `OPT-VAL-047` |
| `OPT-AC-020` | Dokter mencari "frame" di peresepan obat | Gagal (tidak tampil) | E | Barang Optik (`IsPrescribable = false`) tidak muncul |
| `OPT-AC-021` | Batal sesudah pengerjaan, lensa stok "belum dipotong" | Berhasil | I | Lensa stok kembali ke stok, barisnya batal |
| `OPT-AC-021` | Batal sesudah pengerjaan, lensa stok "sudah dipotong" | Berhasil | I | Baris lensa tetap ditagih; stok tidak kembali |
| `OPT-AC-022` | Keluhan ukuran hari ke-31 | Gagal | U | `IsWithinWarranty = false`; persetujuan remake `409` `OPT-VAL-062` |
| `OPT-AC-022` | Lapisan mengelupas bulan ke-5 | Berhasil | U | `IsWithinWarranty = true` |
| `OPT-AC-023` | Kunjungan Optik dari rujukan Poli Mata | Berhasil | I | `ReferralSourceEncounterId` dan dokter perujuk tampil di ringkasan kunjungan |
| `OPT-AC-023` | Rujukan menunjuk kunjungan milik pasien lain | Gagal | I | `400` dari Registrasi |
| `OPT-AC-024` | Pemeriksaan final lalu direvisi dua kali | Berhasil | I + K | Tepat satu baris tarif pemeriksaan di Billing |
| `OPT-AC-025` | Pasien lunas penuh di awal, pesanan masih `InFabrication` | Berhasil | K | Tagihan **tidak** difinalisasi; kunjungan tetap terbuka (`OrdersIncomplete`) |
| `OPT-AC-025` | Pesanan terakhir `HandedOver` dan sudah lunas | Berhasil | K | Tagihan difinalisasi; kunjungan `Completed` |
| `OPT-AC-026` | Keluhan dicatat pada kunjungan pemesanan yang sama | Gagal | I | `400` `OPT-VAL-061` |
| `OPT-AC-027` | Penjamin menanggung Rp 1.000.000 dari Rp 1.200.000; terbayar Rp 90.000 | Gagal | I | `409` minimum Rp 100.000 |
| `OPT-AC-027` | Penjamin menanggung penuh | Berhasil | I | Kirim diterima tanpa uang muka |
| `OPT-AC-028` | Salin resep luar +1,00 / +0,50 × 90 | Berhasil | U | Tersimpan +1,50 / −0,50 × 180, `IsTransposed`, teks asli tersimpan |
| `OPT-AC-028` | RO mengisi SPH −22,00 | Gagal | U | `403` `OPT-VAL-004` |
| `OPT-AC-028` | Sp.M mengisi SPH −22,00 dengan alasan | Berhasil | I | Tersimpan dengan `RangeOverrideReason` |
| `OPT-AC-028` | CYL −0,75 tanpa AXIS | Gagal | U | `400` `OPT-VAL-012` |
| `OPT-AC-029` | Jual lensa kontak kosmetik tanpa resep | Gagal | I | `409` `OPT-VAL-042` |
| `OPT-DEC-048` | Serahkan tanpa mencentang identitas | Gagal | I | `400` `OPT-VAL-051` |
| `OPT-DEC-049` | `NotCollected` lalu pasien datang dan melunasi | Berhasil | I + K | Status `HandedOver`; piutang tertutup di Billing |
| `OPT-DEC-047` | Revisi visus sesudah final tanpa alasan | Gagal | U | `400` `OPT-VAL-021` |
| `OPT-DEC-025` | Laporan pesanan terlambat per 10 November | Berhasil | I | Menampilkan pesanan lewat perkiraan jadi dan yang `ReadyForPickup` > 30 hari |
| `INT-OPT-04` | Billing tidak menjawab saat simpan & tagihkan | Gagal | K | Pesanan tetap `Draft`; pesan `503`; tidak ada setengah jadi |
| `INT-OPT-07` | Stok kurang saat kirim | Gagal | I | `409` `OPT-VAL-049`; status, kunci resep, dan stok tidak berubah |
| E2E penuh | Kunjungan → pemeriksaan → resep → pesanan → uang muka → pengerjaan → siap → serah → keluhan → remake → serah | Berhasil | E | Seluruh status dan tagihan sesuai; satu tagihan per kunjungan pemesanan |
