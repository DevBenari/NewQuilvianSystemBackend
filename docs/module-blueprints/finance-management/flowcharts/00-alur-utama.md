# Alur utama Finance Management — penunjuk, belum lengkap

| Field | Nilai |
|---|---|
| Status | **`BELUM LENGKAP`** — berkas ini dibuat 5 Oktober 2026 pada revisi 17 untuk menutup berkas canonical yang hilang, **bukan** untuk menyatakan alur utama modul sudah tergambar |
| Mengapa belum lengkap | Modul ini dirancang sebelum aturan `flowcharts/00-alur-utama.md` berlaku. Yang ada di folder ini adalah alur **per proses** yang lahir bersama amandemen masing-masing. Menggambar alur pokok Finance ujung ke ujung menuntut satu pass desain tingkat modul — bukan efek samping amandemen satu kemampuan |
| Yang **MUST NOT** dilakukan dengan berkas ini | Menganggapnya sebagai alur utama modul, atau menggambar alur pokok Finance dari terkaan hanya agar berkas ini terlihat terisi |
| Siapa yang menutupnya | Pass `design-business-module` tingkat modul, atas permintaan pemilik |

## Alur per proses yang SUDAH tergambar

| Proses | Berkas | Slice / rumpun | Revisi |
|---|---|---|---|
| Klaim penjamin | [klaim-penjamin.md](klaim-penjamin.md) | Piutang penjamin | 13 |
| Piutang sewa non-pasien | [piutang-sewa-non-pasien.md](piutang-sewa-non-pasien.md) | Piutang sewa | 13 |
| Migrasi tagihan lama | [migrasi-tagihan-lama.md](migrasi-tagihan-lama.md) | Cutover dan subledger | 15 |
| Bukti pembayaran langsung | [bukti-pembayaran-langsung.md](bukti-pembayaran-langsung.md) | Pembayaran langsung berkontrol | 15 |
| Perjanjian cicilan piutang karyawan | [perjanjian-cicilan-piutang-karyawan.md](perjanjian-cicilan-piutang-karyawan.md) | `S2a`, `S2b` | 17 |
| Pelunasan internal porsi benefit | [pelunasan-internal-porsi-benefit.md](pelunasan-internal-porsi-benefit.md) | `S7a` | 17 |
| Integrasi piutang manfaat karyawan end-to-end | [01-alur-piutang-manfaat-karyawan-end-to-end.md](01-alur-piutang-manfaat-karyawan-end-to-end.md) | `S1`, `S3`, `S4b`, `S5`, `S6` | 18 |

## Rumpun yang BELUM punya flowchart

Daftar ini adalah daftar utang, bukan daftar rencana. Tidak ada satu pun yang dijadwalkan pada amandemen
ini.

| Rumpun kemampuan | Keterangan |
|---|---|
| Penerimaan piutang dan alokasinya | Jalur pokok piutang — penerimaan uang, alokasi ke tagihan, dan pembalikannya |
| Utang supplier dan pembayarannya | Termasuk Purchasing/AP dan potongan retur |
| Kas dan bank beserta rekap hariannya | Termasuk buku mutasi kas |
| Deposit pasien | Penempatan, pemakaian, dan pengembaliannya |
| Petty cash | Pengajuan, realisasi, dan pertanggungjawabannya |
| Penghapusan buku dan penyesuaian piutang | Pola pengaju–penyetuju `FIN-DEC-012` |
| Snapshot saldo subledger dan penutupan periode | Termasuk pemetaan akun control |

## Apa yang dapat dipakai sementara ini

Sampai alur pokok modul digambar, urutan langkah per proses terbaca dari tiga tempat: berkas pada tabel
pertama di atas, matriks perpindahan status pada `contracts/state-transition-matrix.md`, dan skenario UAT
pada `04-prd-to-mvp.md`. Relasi antar entity terbaca dari class diagram pada
`02-backend-architecture.md`, dan kontrak kolomnya dari `data/data-dictionary.md`.
