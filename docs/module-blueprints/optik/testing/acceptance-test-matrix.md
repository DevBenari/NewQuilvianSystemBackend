# Optik — Matriks Uji Penerimaan

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| Revision | `5` — `approved`, Yoga Aji Pratama 2026-11-06 |
| Masukan | `OPT-AC-001`..`042` (`00-interview-decisions.md` r13), `OPT-API-v1.4`, `OPT-VAL-v1.3`, `OPT-INT-v1.3`, `OPT-STATE-v1.1`, `OPT-PERM-v1.4` |

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
| `OPT-AC-011` | Tagihkan frame bertarif Rp 450.000 | Berhasil | K | Baris tagihan Rp 450.000 mengikuti master tarif |
| ~~`OPT-AC-011`~~ | ~~Diskon 15% (batas 10%)~~ | Dikeluarkan dari MVP | — | Kalimat kedua pindah ke slice `OPT-APR-004` (`OPT-DEC-079`); tidak diuji pada rilis ini |
| `OPT-AC-012` | Pesanan memakai resep berusia 13 bulan | Gagal | U + I | `409` `OPT-VAL-023` |
| `OPT-AC-013` | Keluhan ukuran hari ke-10, diperiksa, disetujui | Berhasil | I | Pesanan remake `IsRemake`, tanpa fakta tagihan; stok dipotong saat dikirim |
| `OPT-AC-014` | Rekam Medis membaca pemeriksaan dan resep | Berhasil | K | Visus, refraksi, dan resep versi terakhir; tanpa harga |
| `OPT-AC-016` | Diskon Optik 10% pada frame Rp 450.000 | Berhasil | K | Diskon Rp 45.000 tercatat langsung di baris tagihan frame. Bagian 15% pindah ke slice `OPT-APR-004` (`OPT-DEC-079`) |
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
| `OPT-AC-027` | Penjamin menanggung Rp 1.000.000 dari Rp 1.200.000; terbayar Rp 90.000 | Gagal | I + E | `409` minimum Rp 100.000. Wajib lulus dengan penjamin sungguhan sebelum rilis (`OPT-DEC-078`) |
| `OPT-AC-027` | Penjamin menanggung penuh | Berhasil | I | Kirim diterima tanpa uang muka |
| `OPT-AC-028` | Salin resep luar +1,00 / +0,50 × 90 | Berhasil | U | Tersimpan +1,50 / −0,50 × 180, `IsTransposed`, teks asli tersimpan |
| `OPT-AC-028` | RO mengisi SPH −22,00 | Gagal | U | `403` `OPT-VAL-004` |
| `OPT-AC-028` | Sp.M mengisi SPH −22,00 dengan alasan | Berhasil | I | Tersimpan dengan `RangeOverrideReason` |
| `OPT-AC-028` | CYL −0,75 tanpa AXIS | Gagal | U | `400` `OPT-VAL-012` |
| `OPT-AC-029` | Jual lensa kontak kosmetik tanpa resep | Gagal | I | `409` `OPT-VAL-042` |
| `OPT-AC-030` | Pengguna tanpa `OptReport : Read` membuka alamat laporan | Gagal | E | Menu tidak tampil; halaman akses ditolak |
| `OPT-AC-031` | Penyaring Menunggu Persetujuan Batal | Berhasil | I + E | Tepat pesanan dengan pengajuan batal belum diputuskan |
| `OPT-AC-031` | Penyaring Lewat Batas Ambil: siap 5 Agustus dan 6 Agustus 2026, hari ini 3 November, batas 90 hari | Berhasil | I | Hanya pesanan 5 Agustus yang tampil |
| `OPT-AC-032` | Catat Keluhan dari pesanan `Diserahkan` | Berhasil | E | Kunjungan baru dan keluhan tertaut, tanpa memilih ulang pasien atau pesanan |
| `OPT-AC-033` | Buat Resep dari pemeriksaan final, ADD diubah sebelum simpan | Berhasil | E | Form terisi koreksi subjektif dan PD; nilai tersimpan = nilai di form |
| `OPT-AC-034` | Pratinjau draf frame Rp 450.000 + lensa Rp 600.000 | Berhasil | I | Total Rp 1.050.000, sama dengan baris tagihan sesudah Tagihkan pada hari yang sama |
| `OPT-AC-034` | Pratinjau dengan barang tanpa tarif | Gagal (ditandai) | I | Barang ditandai "tarif belum tersedia"; pratinjau tidak ditolak; `AllTariffsAvailable = false` |
| `OPT-AC-035` | Kirim ulang `POST prescriptions` berkunci sama sesudah resep tersimpan | Berhasil | I | `200`, resep pertama, `IsReplay = true`, bukan `409` "sudah ada"; tetap satu baris |
| `OPT-AC-035` | Kirim ulang `POST prescriptions/external` berkunci sama | Berhasil | I | Satu baris `OptPrescription` |
| `OPT-AC-035` | Kirim ulang `POST orders` berkunci sama | Berhasil | I | Satu baris `OptOrder`, satu baris riwayat status |
| `OPT-AC-035` | Kirim ulang `POST warranty-claims` berkunci sama (KLM-0001) | Berhasil | I | Satu keluhan pada pesanan itu |
| `OPT-AC-035` | Salah satu dari empat `POST` tanpa header atau berisi Guid nol | Gagal | I | `400` `OPT-INVALID-REQUEST`; tidak ada baris tersimpan |
| `OPT-AC-035` | Dua kiriman serentak berkunci sama | Berhasil | I (PostgreSQL) | Satu baris; kedua jawaban menunjuk data yang sama; tidak ada galat bagi pengguna |
| `OPT-AC-035` | Pesanan remake dibuat dua kali lewat keluhan berbeda | Berhasil | I | Kunci remake kosong tidak saling bentrok pada indeks unik |
| `OPT-AC-036` | Kunjungan unit Optik dengan kartu PT Contoh (samaran) yang berlaku | Berhasil | I + E | Kunjungan tersimpan dengan penjamin itu |
| `OPT-AC-036` | Kunjungan Laboratorium dengan kartu yang sama | Gagal | I | "Penjamin perusahaan hanya dapat dipilih lewat pendaftaran loket." |
| `OPT-AC-036` | Kunjungan unit Optik dengan kartu berakhir 31-10-2026, pelayanan 05-11-2026 | Gagal | I | `400` `OPT-REG-REJECTED` dengan pesan masa berlaku; tidak ada kunjungan |
| `OPT-AC-036` | Kunjungan unit Optik dengan penjamin benefit karyawan internal | Gagal | I | `400` "…hanya dapat dipilih lewat pendaftaran loket." |
| `OPT-AC-037` | Modal Ajukan Diskon saat Diskon Optik 10% dan promo 50% unit lain aktif | Gagal (tidak tampil) | E | Hanya Diskon Optik 10% di pilihan |
| `OPT-AC-037` | Pilih Diskon Optik 10% pada frame Rp 450.000 | Berhasil | K + E | Tagihan langsung berkurang Rp 45.000, tanpa persetujuan |
| `OPT-AC-038` | Daftarkan frame ber-`IsPrescribable = false` | Berhasil | E | Frame tampil di katalog dan bisa dipilih di form pesanan |
| `OPT-AC-038` | Cari barang ber-`IsPrescribable = true` di Daftarkan Barang | Gagal (tidak tampil) | E | Barang tidak ada di pilihan |
| `OPT-AC-038` | Modal Daftarkan Barang dibuka | Berhasil | E | Keterangan penimpaan profil tampil; barang berprofil tetap bisa disimpan |
| `OPT-AC-039` | 3 pemeriksaan final tanpa `BillingFactEmittedAt`, tagihan terbuka; dua putaran pekerja latar | Berhasil | I + K | 3 baris tarif, masing-masing satu; putaran kedua tidak menambah apa pun; penanda terisi; pelaku fakta `FinalizedBy` |
| `OPT-AC-039` | Pemeriksaan yang baru difinalkan 1 menit lalu | Berhasil | I | Tidak dipilih selama masih dalam `GraceMinutes` |
| `OPT-AC-039` | 51 pemeriksaan, 50 pertama terus gagal, `BatchSize` 50 | Berhasil | I | Pemeriksaan ke-51 tetap terkirim pada putaran berikutnya |
| `OPT-AC-039` | Tarif berlaku sesudah tanggal pemeriksaan | Gagal (rekonsiliasi) | K | Fakta tercatat, penanda terisi, masuk rekonsiliasi Billing, tidak dikirim ulang lagi |
| `OPT-AC-040` | Tindakan belum diatur, 3 pemeriksaan tertinggal | Gagal (menunggu) | I | Tidak ada fakta, penanda tetap kosong, satu peringatan per putaran, peringatan layar tetap tampil |
| `OPT-AC-040` | Tindakan diatur sesudah putaran pertama | Berhasil | I | Putaran berikutnya mengirim ketiganya |
| `OPT-AC-041` | Tarif Rp 75.000 terkirim sesudah tagihan final | Berhasil | K | Adjustment Rp 75.000 menunggu persetujuan; total tagihan final tidak berubah |
| `OPT-AC-041` | Kunjungan pemeriksaan sudah tertutup dan belum punya tagihan | Berhasil atau rekonsiliasi | K | Perilaku Billing dicatat di laporan task; tarif tidak hilang diam-diam dan tidak ada galat tak tertangani |
| `OPT-AC-042` | Petugas penjualan, bukan superadmin, mendaftarkan kunjungan dengan Kartu Penjamin | Berhasil | E | Pilihan pasien, kartu, dan kunjungan asal terbuka tanpa `403`; kunjungan tersimpan |
| `OPT-AC-042` | RO membuka detail pesanan yang sudah ditagihkan | Gagal (rincian ditolak) | E | `GET invoices/{id}` `403`; pesan server tampil di bagian harga; Status Bayar tetap tampil |
| `OPT-DEC-048` | Serahkan tanpa mencentang identitas | Gagal | I | `400` `OPT-VAL-051` |
| `OPT-DEC-049` | `NotCollected` lalu pasien datang dan melunasi | Berhasil | I + K | Status `HandedOver`; piutang tertutup di Billing |
| `OPT-DEC-047` | Revisi visus sesudah final tanpa alasan | Gagal | U | `400` `OPT-VAL-021` |
| `OPT-DEC-025` | Laporan pesanan terlambat per 10 November | Berhasil | I | Menampilkan pesanan lewat perkiraan jadi dan yang `ReadyForPickup` > 30 hari |
| `INT-OPT-04` | Billing tidak menjawab saat simpan & tagihkan | Gagal | K | Pesanan tetap `Draft`; pesan `503`; tidak ada setengah jadi |
| `INT-OPT-07` | Stok kurang saat kirim | Gagal | I | `409` `OPT-VAL-049`; status, kunci resep, dan stok tidak berubah |
| E2E penuh | Kunjungan → pemeriksaan → resep → pesanan → uang muka → pengerjaan → siap → serah → keluhan → remake → serah | Berhasil | E | Seluruh status dan tagihan sesuai; satu tagihan per kunjungan pemesanan |
