# Pengadaan Barang & Jasa — Ringkasan Bisnis

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 1 |
| Tanggal | 12 November 2026 |
| Sumber kebutuhan | Dokumen pengguna "BRD & PRD — Modul Pengadaan Barang & Jasa" versi 1.0 (draft, 12 November 2026) |
| Status dokumen sumber | DRAFT, belum disahkan pemilik proses bisnis |

> **Cara membaca dokumen ini.** Setiap pernyataan diberi jenis:
> - **SOURCE**: berasal langsung dari daftar menu pengguna (11 menu, 61 submenu).
> - **PROPOSAL**: usulan dari BRD/PRD (alur, aturan, field, hak akses, API, fase). Belum menjadi keputusan.
> - **DECISION**: sudah diputuskan pemilik dan tercatat.
> - **FACT**: terbukti dari repository atau blueprint lain, dengan rujukan.
> - **OPEN**: pertanyaan yang menunggu keputusan pemilik.
>
> Salinan utuh dokumen BRD & PRD belum disimpan di repository. Disarankan menaruhnya di
> `docs/module-blueprints/pengadaan-barang-jasa/evidence/00-brd-prd-v1.0.md` sebagai bukti masukan.

---

## 1. Tujuan modul

Modul ini mengelola pengadaan dari awal sampai akhir: kebutuhan unit dicatat, diajukan, diperiksa,
disiapkan, vendor dipilih, pelaksanaannya dipantau, hasilnya diperiksa, vendornya dinilai, lalu
semuanya bisa dilaporkan dan diaudit. Semua tahap harus bisa ditelusuri.

**Contoh alur sederhana.** Unit Rawat Inap butuh 20 kursi roda. Kepala unit mengajukan permintaan.
Tim Pengadaan memeriksa stok, ternyata tersedia 5. Sisanya, 15 kursi roda, disiapkan
spesifikasinya. Tiga vendor diminta penawaran, lalu satu vendor ditetapkan dan disetujui pejabat
berwenang. PO dibuat di sistem BLM. Barang datang dalam dua kali kirim, diperiksa, dan dibuatkan
BAST. Terakhir, kinerja vendor dinilai.

## 2. Keputusan yang sudah diambil

| ID | Jenis | Pernyataan | Pemilik | Status | Bukti |
| --- | --- | --- | --- | --- | --- |
| `PBJ-DEC-001` | DECISION | **Batas modul: pilihan A — Pengadaan sebagai modul hulu.** Modul ini memiliki: rencana kebutuhan, permintaan pengadaan (PR), persiapan, permintaan dan evaluasi penawaran, penetapan vendor, monitoring, evaluasi vendor, serta persetujuan dan laporan pengadaan. **Purchase Order (PO) dan Tanda Terima Barang (GR) TIDAK dimiliki modul ini**: keduanya tetap bersumber dari BLM dan disimpan oleh Finance Management (`FIN-DEC-045`, `FIN-DEC-261`, `FIN-DEC-265`). Modul ini menyerahkan hasil penetapan vendor ke proses PO dan hanya **membaca** status PO/GR untuk monitoring. | Pengguna sesi (pemilik kebutuhan) | `approved` | Jawaban "saya setuju A", 12 November 2026 |
| `PBJ-DEC-002` | DECISION | Modul ini dicatat sebagai blueprint baru `PBJ-BP-001`, tidak menggantikan blueprint lain. | Pengguna sesi | `approved` | Idem |

## 3. Fakta dari repository dan blueprint lain

| ID | Jenis | Pernyataan | Bukti |
| --- | --- | --- | --- |
| `PBJ-FACT-001` | FACT | Finance Management sudah memiliki submodule `Purchasing` (prefix `Fin`, `ACTIVE`): PO, GR, Tukar Faktur, Faktur Pembelian, Retur. | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 21 @`100c011d`; `finance-management/00-interview-decisions.md` `FIN-DEC-045` |
| `PBJ-FACT-002` | FACT | BLM adalah sumber utama PO dan GR. Quilvian tidak boleh membuat, mengubah, atau menyetujui PO/GR BLM. Integrasinya masih menunggu Tim BLM (`FIN-OQ-141` `OPEN`). | `finance-management/00-interview-decisions.md` `FIN-DEC-261`, `FIN-DEC-265`, `FIN-OQ-141` |
| `PBJ-FACT-003` | FACT | Master supplier yang dipakai Finance adalah `MstSupplier` milik Administrator. | `Areas/Administrator/MasterData/Models/MstSupplier.cs` @`100c011d`; `FIN-DEC-014` |
| `PBJ-FACT-004` | FACT | Approval PO dan Faktur Pembelian di Finance memakai dua jenjang dengan ambang Rp 50.000.000. | `FIN-DEC-050`, `FIN-DEC-052` |
| `PBJ-FACT-005` | FACT | Tidak ditemukan modul gudang/inventori umum di `Areas/`. Hanya ada stok di modul layanan seperti Farmasi. Kemampuannya belum diaudit. | Daftar folder `Areas/` @`100c011d` |

## 4. Pemetaan 61 submenu terhadap keputusan A

Kolom "Pemilik usulan" adalah **PROPOSAL** turunan `PBJ-DEC-001`. Baris bertanda **OPEN** butuh keputusan.

| Menu (SOURCE) | Submenu (SOURCE) | Pemilik usulan | Catatan |
| --- | --- | --- | --- |
| 1. Dashboard Pengadaan | Ringkasan Pengadaan; Pengajuan Menunggu Persetujuan; Pengadaan Berjalan; Pengadaan Tertunda | Pengadaan | — |
| | Realisasi Anggaran | Pengadaan, membaca data Finance | **OPEN** `PBJ-OQ-009` |
| 2. Rencana Pengadaan | 5 submenu | Pengadaan | Pemilik pagu anggaran: **OPEN** `PBJ-OQ-009` |
| 3. Permintaan Pengadaan | Pengajuan; Verifikasi Kebutuhan; Permintaan Mendesak; Riwayat Pengajuan | Pengadaan | — |
| | Pemeriksaan Stock | Pengadaan, membaca stok dari sumber lain | Sumber stok **OPEN** `PBJ-OQ-008` |
| 4. Persiapan Pengadaan | 5 submenu | Pengadaan | — |
| 5. Data Penyedia/Vendor | Daftar vendor; Kategori; Dokumen Legalitas; Rekening; Status; Riwayat | **OPEN** `PBJ-OQ-010` | Memperluas `MstSupplier` atau tabel pendamping milik Pengadaan |
| 6. Penawaran & Pemilihan Penyedia | 6 submenu | Pengadaan | — |
| 7. Pesanan & Kontrak | Purchase Order (PO) | BLM / Finance — **hanya dibaca** | `PBJ-DEC-001` |
| | SPK; Kontrak; Termin Pembayaran; Perubahan Kontrak; Arsip Kontrak | **OPEN** `PBJ-OQ-011` | Termin bersinggungan dengan utang Finance |
| 8. Monitoring Pelaksanaan | 6 submenu | Pengadaan | Membaca status PO/GR BLM; bergantung pada `FIN-OQ-141` |
| 9. Serah Terima Hasil | Serah Terima Barang | BLM / Finance (GR) — **hanya dibaca** | `PBJ-DEC-001` |
| | Pemeriksaan Barang/Jasa; Serah Terima Pengerjaan; BAST; Ketidaksesuaian; Penyelesaian | **OPEN** `PBJ-OQ-012` | Hubungan BAST dan pemeriksaan dengan GR BLM |
| 10. Evaluasi Penyedia | 5 submenu | Pengadaan | — |
| 11. Persetujuan & Laporan | Antrean; Validasi; Persetujuan Paket & Kontrak; Laporan Pembelian; Laporan Audit | Pengadaan | — |
| | Laporan Realisasi | Pengadaan, membaca data Finance | **OPEN** `PBJ-OQ-009` |

## 5. Ketergantungan awal

| ID | Kemampuan | Jenis | Status | Dibutuhkan untuk |
| --- | --- | --- | --- | --- |
| `PBJ-DEP-001` | Master supplier `MstSupplier` | `MODULE_FOUNDATION` | `UNKNOWN` (belum diaudit) | Menu 5, 6, 10 |
| `PBJ-DEP-002` | PO/GR Finance + integrasi BLM | `INTEGRATION` | `UNKNOWN`; integrasi BLM tertahan `FIN-OQ-141` | Menu 7, 8, 9 (baca) |
| `PBJ-DEP-003` | Sumber data stok / master barang | `INTEGRATION` | `UNKNOWN` | Menu 3 (Pemeriksaan Stock) |
| `PBJ-DEP-004` | Mesin approval, nomor dokumen (`NumberSeriesManagement`), unggah berkas, audit log, hak akses | `MODULE_FOUNDATION` | `UNKNOWN` | Semua menu |
| `PBJ-DEP-005` | Data pagu/komitmen/realisasi anggaran | `INTEGRATION` | `UNKNOWN` | Menu 1, 2, 11 |

## 6. Pertanyaan terbuka untuk pemilik proses

> **Pembaruan 12 November 2026:** sebagian besar pertanyaan di bawah sudah dijawab pada wawancara
> (`PBJ-DEC-004`..`PBJ-DEC-018`). Status terkini ada di `00-interview-decisions.md` bagian 5. Tabel ini
> disimpan sebagai daftar asal.

| ID | Pertanyaan | Asal | Prioritas |
| --- | --- | --- | --- |
| `PBJ-OQ-001` | Bentuk blueprint: `SINGLE` atau `COMPOSITE`? | Aturan bentuk blueprint | Tinggi |
| `PBJ-OQ-002` | Prefix entity (`Prc` diusulkan) dan Area (`Corporate/ProcurementManagement` diusulkan) | `QBE-MOD-002/003` | Tinggi, sebelum implementasi |
| `PBJ-OQ-003` | Status hukum rumah sakit (swasta, BLU/BLUD, lainnya) dan SOP pengadaan yang berlaku | BRD A7.1, B15 | Kritis |
| `PBJ-OQ-004` | Matriks otorisasi: nominal, jenis, unit, urgensi, sumber dana. Apakah ambang Rp 50 juta Finance juga dipakai? | BRD A7.2, B15 | Kritis |
| `PBJ-OQ-005` | Metode pengadaan dan batas nilainya (penunjukan langsung, pembandingan penawaran, dll.) | BRD A7.3 | Tinggi |
| `PBJ-OQ-006` | Apakah rencana tahunan wajib menjadi rujukan setiap PR? Bagaimana pengadaan di luar rencana? | BRD A7.4 | Tinggi |
| `PBJ-OQ-007` | Pemisahan barang stok, aset, nonstok, dan jasa; siapa pemilik nomor dokumen | BRD A7.5 | Tinggi |
| `PBJ-OQ-008` | Sumber "stok tersedia" (BLM, Farmasi, gudang lain), serta perlakuan stok parsial dan reservasi | BRD A7.6, B15 | Kritis |
| `PBJ-OQ-009` | Definisi pagu, committed, dan realisasi; siapa pemilik data anggaran | BRD A7.7, B15 | Kritis |
| `PBJ-OQ-010` | Data vendor: perluas `MstSupplier` atau tabel pendamping milik Pengadaan? Siapa pemilik rekening vendor? | BRD A7.9 + `PBJ-FACT-003` | Tinggi |
| `PBJ-OQ-011` | SPK, Kontrak, Termin, dan Addendum milik siapa? Bagaimana hubungannya dengan PO BLM dan utang Finance? | `PBJ-DEC-001` | Tinggi |
| `PBJ-OQ-012` | Pemeriksaan, BAST, dan NCR: dicatat di Pengadaan lalu dikaitkan ke GR BLM, atau di tempat lain? | `PBJ-DEC-001`, BRD A7.10 | Tinggi |
| `PBJ-OQ-013` | Kebijakan pengadaan mendesak dan persetujuan setelah kejadian | BRD A7.8 | Sedang |
| `PBJ-OQ-014` | Bagaimana hasil penetapan vendor diserahkan ke BLM untuk dibuatkan PO? Integrasi baru atau manual? | `PBJ-DEC-001`, `FIN-OQ-141` | Tinggi |
| `PBJ-OQ-015` | SLA, kalender kerja, retensi dokumen, notifikasi | BRD A7.12 | Sedang |
| `PBJ-OQ-016` | Urutan rilis MVP (fase F0–F4 BRD masih usulan) | BRD B12, B15 | Sedang |
