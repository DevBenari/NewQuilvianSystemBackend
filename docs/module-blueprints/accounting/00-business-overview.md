# Accounting — Business Overview

| Field | Value |
| --- | --- |
| Blueprint ID | `ACC-BP-001` |
| Revision | `2` |
| Sumber | `ACC-PRD-001` revisi `0.1`, disetujui 1 September 2026 |
| Product/domain owner | Rizki |
| Implementation owner | Rizki |

## Untuk apa modul ini dibuat

Modul Accounting adalah **buku catatan keuangan resmi** rumah sakit. Semua kejadian yang
punya akibat uang — pasien membayar, obat terpakai, gaji dibayarkan, alat disusutkan —
diterjemahkan menjadi catatan akuntansi yang seimbang, bisa ditelusuri, dan bisa diaudit.

Istilah yang dipakai berulang:

| Istilah | Artinya dalam bahasa sehari-hari |
| --- | --- |
| Chart of Accounts (COA) | Daftar akun. Semacam daftar "laci" tempat setiap rupiah digolongkan, misalnya Kas, Piutang Pasien, Pendapatan Rawat Inap |
| Journal | Satu catatan transaksi akuntansi, berisi minimal dua baris yang saling menyeimbangkan |
| Debit dan Kredit | Dua sisi setiap catatan. Total kedua sisi wajib sama persis |
| Posting | Tindakan mengesahkan sebuah journal sehingga resmi masuk buku besar dan tidak bisa diubah lagi |
| General Ledger (GL) | Buku besar. Kumpulan semua journal yang sudah disahkan, dikelompokkan per akun |
| Accounting Period | Periode akuntansi, misalnya bulan September 2026. Dipakai untuk mengunci pembukuan |
| Idempotency | Jaminan bahwa satu kejadian yang sama, walau dikirim berkali-kali, hanya dicatat sekali |

**Contoh alur nyata.** Pasien Budi dirawat inap, lalu tagihannya diterbitkan Billing sebesar
Rp 10.000.000 dengan penjamin BPJS menanggung Rp 8.000.000 dan Budi menanggung Rp 2.000.000.
Billing tetap pemilik tagihan itu. Accounting menerima pemberitahuan bahwa piutang telah
diakui, lalu membuat satu journal: sisi debit Piutang BPJS Rp 8.000.000 dan Piutang Pasien
Rp 2.000.000, sisi kredit Pendapatan Rawat Inap Rp 10.000.000. Total kedua sisi sama, yaitu
Rp 10.000.000, sehingga journal boleh disahkan.

## Batas kepemilikan

Accounting **memiliki** dan berhak mengatur:

- Daftar akun (COA);
- Journal dan barisnya;
- Proses pengesahan (posting);
- Buku besar;
- Periode akuntansi dan penutupannya;
- Laporan keuangan akuntansi;
- Jejak audit akuntansi.

Accounting **tidak memiliki**, dan hanya boleh menerima akibat keuangannya:

- Faktur dan item tagihan (milik Billing);
- Siklus penagihan piutang dan pembayaran utang (milik Finance);
- Pelaksanaan pembayaran dan transaksi kasir (milik Billing/Finance);
- Pengajuan dan persetujuan anggaran (milik Budgeting);
- Stok, pembelian, dan penguasaan aset tetap (milik Inventory, Purchasing, Fixed Asset).

**Contoh perbedaannya.** Ketika BPJS menolak sebagian klaim, yang menagih ulang, menghubungi
penjamin, dan memutuskan penghapusan piutang adalah Finance. Accounting hanya mencatat
akibatnya di buku besar setelah keputusan itu sah.

Revisi 2 menambahkan 27 keputusan owner hasil wawancara 1 September 2026. Daftar lengkapnya
ada di [00-interview-decisions.md](00-interview-decisions.md); tabel di bawah hanya memuat
keputusan pondasi `ACC-DEC-001` sampai `ACC-DEC-008`.

## Catatan penting

| ID | Type | Pernyataan | Owner | Status | Bukti/approval |
| --- | --- | --- | --- | --- | --- |
| `ACC-DEC-001` | Decision | Accounting menjadi bounded context tersendiri | Rizki | `approved` | `ACC-PRD-001@0.1` §4, 1 September 2026 |
| `ACC-DEC-002` | Decision | Accounting adalah pemilik tunggal Journal, Posting, GL, Period, dan Closing. Modul lain dilarang punya buku besar tandingan | Rizki | `approved` | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-003` | Decision | Finance adalah bounded context di luar Accounting | Rizki | `approved` | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-004` | Decision | Billing dan Kasir tetap di luar Accounting | Rizki | `approved` | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-005` | Decision | Transaksi sumber tetap milik modul penerbitnya | Rizki | `approved` | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-006` | Decision | Riwayat akuntansi yang sudah disahkan bersifat permanen; koreksi hanya lewat pembalikan atau journal koreksi | Rizki | `approved` | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-007` | Decision | Accounting boleh dikembangkan lebih dulu, paralel dengan Finance | Rizki | `approved` | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-008` | Decision | Migration Accounting boleh dibuat lebih dulu | Rizki | `approved` | `ACC-PRD-001@0.1` §4 |
| `ACC-FACT-001` | Fact | Tidak ada satu pun entity atau layar akuntansi di kedua repository | — | terverifikasi | `NewQuilvianSystemBackend@aa837d7`, `QuilvianSystemFrontendDev@fc49cc7` |
| `ACC-FACT-002` | Fact | Kontrak integrasi Billing `BIL-INTEGRATION-0.4` sudah disetujui dan mengarahkan akibat keuangan ke AR/AP | Owner Billing | terverifikasi | `billing-kasir/contracts/integration-contract.md#BIL-INT-007..009@aa837d7` |
| `ACC-CONF-001` | Conflict | `ACC-PRD-001` §26 menganggap arah Billing masih terbuka, padahal kontrak Billing yang sudah disetujui sudah menguncinya ke AR/AP | Owner Billing, owner Finance, Rizki | terbuka | Lihat `ACC-DEP-003` dan `ACC-OQ-005` |
| `ACC-CONF-002` | Conflict | `ACC-PRD-001` §35 hanya menandai 8 pertanyaan sebagai pemblokir, tetapi §38 menuntut kepastian atas hal-hal yang berasal dari pertanyaan yang tidak ditandai pemblokir | Rizki | **selesai** | Diselesaikan dengan menutup seluruh 36 pertanyaan, bukan hanya 8. Lihat `ACC-DEC-036` |
| `ACC-ASSUM-001` | Assumption | `ACC-DEC-008` mengasumsikan migration Accounting aman dibuat lebih dulu. Asumsi ini tidak berlaku selama snapshot EF belum dipulihkan | Rizki | perlu ditinjau | Lihat `ACC-DEP-001` |

## Hasil akhir yang diharapkan

Modul dianggap berhasil bila satu kejadian keuangan dari modul lain dapat mengalir sampai
menjadi laporan keuangan, dengan tujuh jaminan: seimbang, tidak dobel, bisa ditelusuri sampai
transaksi asalnya, tercatat siapa yang mengerjakan, terkunci per periode, bisa dikoreksi
tanpa menghapus riwayat, dan hanya bisa diakses oleh yang berhak.
