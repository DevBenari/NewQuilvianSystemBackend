# Penghapusan & Pelepasan Aset — PRD → MVP

## 1. Identitas dokumen

| Field | Value |
|---|---|
| Produk / modul | Quilvian — Manajemen Aset & Inventaris / `penghapusan` |
| Status | `draft` · `AST-HPS-1.0.0` |
| Repository / SHA | BE `rizkiG` `1f54a175`, FE `RizkiV2` `e49e8a81` |
| Cakupan | Usulan, pemeriksaan kelayakan, persetujuan berjenjang, pemindahtanganan, berita acara, riwayat aset dihapus, Pemulihan Aset, laporan penghapusan |

## 2. Ringkasan eksekutif

Aset hanya keluar dari register lewat usulan yang diperiksa tim dan disetujui sesuai batas nilai, dengan
laba/rugi pelepasan yang tercatat dan sampai ke Akuntansi. Kesalahan dapat dipulihkan tanpa menghapus jejak.

## 3. Masalah produk

Belum ada kemampuan penghapusan. Register, kunci proses, buku nilai, dan kotak keluar Akuntansi tersedia
dari sub-modul sebelumnya.

## 4. Visi produk

Usul → periksa → setujui (Direktur bila total di atas batas) → lepas → final → laporan; pemulihan bila keliru.

## 5. Batas MVP

**Mulai:** `nilai` `MVP-4` selesai (nilai buku dan penyusutan berjalan). **Akhir:** keempat metode pelepasan
dan alasan Hilang dapat diselesaikan sampai `Final`; pemulihan berjalan; laporan penghapusan tersedia.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab |
|---|---|
| Petugas Aset / Kepala Unit | Mengusulkan |
| Tim Pemeriksa | Menilai kelayakan |
| Kepala Bagian Aset | Menyetujui, finalisasi, memutuskan pemulihan |
| Direktur | Menyetujui usulan di atas batas |
| Petugas Aset | Mencatat pelepasan, mengajukan pemulihan |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID | Keputusan |
|---|---|---|
| Usulan, kelayakan, persetujuan | `CAP-10.1`–`10.3` | Wajib |
| Pemindahtanganan, berita acara | `CAP-10.4`, `10.5` | Wajib |
| Riwayat aset dihapus | `CAP-10.6` | Wajib |
| Laporan penghapusan | `CAP-11.5` | Wajib |
| Pemulihan Aset | `CAP-EXT-03` | Wajib; satu-satunya jalan koreksi penghapusan (`AST-DEC-028`) |

## 8. Kemampuan yang ditunda

| Kemampuan | Alasan | Pengganti |
|---|---|---|
| Integrasi penerimaan uang hasil penjualan ke Finance | Finance menerima uang lewat alurnya sendiri; tidak diputuskan otomatisasinya | Nomor penerimaan Finance dicatat pada butir; akun penampung hasil pelepasan dipertemukan Akuntansi (`AST-OQ-009`) |
| Lelang daring | Di luar cakupan | Lelang di luar sistem; hasilnya dicatat sebagai penjualan |

## 9. Alur bisnis target

`FLOW-AST-MVP-007` (penghapusan) dan `FLOW-AST-MVP-008` (pemulihan): `flowcharts/01..02`.

## 10. Epic dan functional requirement

### EPIC AST-16 — Penghapusan dan pelepasan · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-150` | Alasan harus sesuai kondisi aset | Rusak Berat untuk aset Aktif ditolak |
| `FR-AST-151` | Tim memeriksa setiap aset; Tidak Layak dikeluarkan | 2 dari 10 dikeluarkan |
| `FR-AST-152` | Persetujuan berjenjang menurut total nilai buku yang dibekukan | 10 × Rp 6 jt = Rp 60 jt → Direktur |
| `FR-AST-153` | Penyetuju bukan pengusul/pemeriksa | — |
| `FR-AST-154` | Empat metode pelepasan dengan aturan nilai | Penjualan wajib > 0; hibah = 0 |
| `FR-AST-155` | Alasan Hilang melewati pemindahtanganan | Laptop hilang → langsung final |
| `FR-AST-156` | Final mensyaratkan penyusutan bulan lalu diproses | Final 10 Agustus butuh proses Juli |
| `FR-AST-157` | Final: aset `Dihapus`, buku dihentikan, laba/rugi, kejadian, berita acara | Tabel contoh `01-usulan-sampai-final.md` |
| `FR-AST-158` | Pembatalan hanya sebelum pemindahtanganan | — |

### EPIC AST-17 — Pemulihan Aset · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-170` | Pemulihan hanya dari butir `Final`; bukti pembatalan untuk aset terjual/dihibahkan/ditukar | — |
| `FR-AST-171` | Disetujui Kepala Bagian Aset bukan pengaju | — |
| `FR-AST-172` | Aset kembali `Terdaftar`/`Aktif`/`BelumDitempatkan`; buku aktif; kejadian balik; penyusutan terlewat dikejar | Contoh laptop `02-pemulihan-aset.md` |

## 11. Model status

`Diusulkan → DalamPemeriksaan → MenungguPersetujuan → (MenungguPersetujuanDirektur) → Disetujui →
(Dipindahtangankan) → Final`, + `Ditolak`, `Dibatalkan`; pemulihan `Diajukan → Disetujui | Ditolak`.

## 12. Sasaran arsitektur

Dipakai ulang: kunci proses, register, linimasa, buku nilai, parameter, kotak keluar. Baru: 4 tabel, 4
service, 3 controller.

## 13. Sasaran kemampuan API

[`contracts/api-contract.md`](contracts/api-contract.md): 2.1, 2.3, 2.4 `EPIC AST-16`; 2.2 `EPIC AST-17`.
Semua **Rencana (belum tersedia)**.

## 14. Matriks kewenangan

[`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 1.

## 15. Batas integrasi dan billing

**MUST NOT** menerima uang, membuat jurnal, menghapus baris aset dari database, atau membuka ulang usulan final.

## 16. Guardrail regulasi

Kebijakan internal badan hukum pemilik RS dan standar akuntansi aset tetap (`AST-DEC-003`). Bila RS
kelak memerlukan persetujuan yayasan/direksi PT di atas batas tertentu, itu tingkat persetujuan baru
(saat ini tidak diputuskan).

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-HPS-001` | Finalisasi seluruh butir satu transaksi |
| `NFR-HPS-002` | Angka final dibekukan dan tidak berubah oleh proses penyusutan sesudahnya |

## 18. Skenario UAT

| ID | Kondisi awal | Langkah | Hasil | Epic |
|---|---|---|---|---|
| `UAT-HPS-01` | Ambulans usang | Usul, periksa, setujui, jual Rp 35 jt, final | Laba Rp 15 jt; aset dihapus; berita acara | AST-16 |
| `UAT-HPS-02` | 10 kursi roda | Usulkan, semua Layak | Wajib Direktur | AST-16 |
| `UAT-HPS-03` | Laptop Hilang | Usulkan, setujui, final | Tanpa pemindahtanganan; rugi kehilangan | AST-16 |
| `UAT-HPS-04` | Monitor Aktif | Usulkan alasan Rusak Berat | Ditolak | AST-16 |
| `UAT-HPS-05` | Juli belum diproses | Finalisasi 10 Agustus | Ditolak | AST-16 |
| `UAT-HPS-06` | Laptop dihapus | Ditemukan, ajukan dan setujui pemulihan | Aset kembali; harus diserahkan ulang | AST-17 |
| `UAT-HPS-07` | Ambulans terjual | Ajukan pemulihan tanpa berita acara pembatalan | Ditolak | AST-17 |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Satu aset dapat dihapus dengan setiap metode | `AT-HPS-04`, `UAT-HPS-01` |
| Batas Direktur dari total terbukti | `AT-HPS-02`, `AT-HPS-03` |
| Final atomik dan angka beku | `AT-HPS-42` |
| Penyusutan berhenti mulai bulan final | `AT-HPS-07` |
| Pemulihan berjalan dan membalik kejadian | `AT-HPS-08` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat |
|---|---|---|
| `MVP-5` | `EPIC AST-16`, `AST-17` | `MVP-4` selesai |

| Pertanyaan | Siapa | Dampak | Memblokir |
|---|---|---|:---:|
| `AST-OQ-003` batas nilai buku Direktur | Rizki/Direktur | Tingkat penyetuju tidak dapat dihitung | Ya untuk go-live |
| `AST-OQ-009` aturan posting penghapusan per metode, akun penampung hasil pelepasan | Kepala Akuntansi | Kejadian masuk daftar gagal Akuntansi | Ya untuk penyalaan kiriman |
