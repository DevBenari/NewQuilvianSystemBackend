# Pergerakan Aset — PRD → MVP

## 1. Identitas dokumen

| Field | Value |
|---|---|
| Produk / modul | Quilvian — Manajemen Aset & Inventaris / `pergerakan` |
| Status | `draft` · `AST-MOV-1.0.0` |
| Repository / SHA | BE `rizkiG` `1f54a175`, FE `RizkiV2` `e49e8a81` |
| Cakupan | Mutasi antar unit, persetujuan, serah terima, peminjaman, pengembalian, perpanjangan, dashboard 1.4, laporan mutasi |

## 2. Ringkasan eksekutif

Setiap perpindahan aset antar unit — tetap maupun sementara — tercatat dengan persetujuan Kepala Unit
dan Bagian Aset, dan baru berlaku saat unit penerima mengonfirmasi. Pinjaman yang lewat waktu terlihat
tanpa harus ditanyakan satu per satu.

## 3. Masalah produk

Tidak ada kemampuan mutasi atau peminjaman di Quilvian. Yang tersedia: register aset beserta penempatan
dan kunci proses (`register`), serta penugasan Kepala Unit dari HR (`TS-04`).

## 4. Visi produk

1. Unit asal atau Bagian Aset mengajukan → Kepala Unit asal dan Bagian Aset menyetujui → unit tujuan
   menerima → penempatan dan pusat biaya berpindah.
2. Unit peminjam meminta → pemilik menyetujui → pinjaman berjalan dengan tanggal kembali → kembali dan
   kondisinya diperiksa.

## 5. Batas MVP

**Mulai:** `register` `MVP-1` selesai (aset dapat ditempatkan). **Akhir:** satu mutasi dan satu pinjaman
dapat berjalan dari pengajuan sampai selesai; Terlambat tampil di dashboard; laporan mutasi tersedia.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab |
|---|---|
| Petugas Aset | Mengajukan mutasi dan pinjaman atas nama unit |
| Kepala Unit asal/pemilik | Menyetujui pelepasan; mengonfirmasi pengembalian |
| Kepala Unit tujuan/peminjam | Menerima mutasi; mengambil dan mengembalikan pinjaman |
| Kepala Bagian Aset | Menyetujui mutasi tahap 2 |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID | Keputusan |
|---|---|---|
| Pengajuan, persetujuan, serah terima mutasi | `CAP-05.1`–`05.3` | Wajib; tanpa ini aset berpindah tanpa jejak |
| Peminjaman, pengembalian | `CAP-05.4`, `05.5` | Wajib; pinjaman menjadi mutasi diam-diam tanpa ini |
| Mutasi berjalan di dashboard | `CAP-01.4` | Wajib |
| Laporan mutasi | `CAP-11.2` | Wajib |

## 8. Kemampuan yang ditunda

| Kemampuan | ID | Alasan | Pengganti |
|---|---|---|---|
| Penahanan saat opname | `AST-RULE-023` | Sesi opname baru ada di `MVP-3` (`AST-DES-026`) | Selama `MVP-2`, Bagian Aset tidak menjadwalkan opname; aturan aktif bersama `inventarisasi` |
| Notifikasi email/WhatsApp ke penyetuju | `AST-RCG-G21` | Tidak diputuskan untuk MVP | Antrean "menunggu Anda" di layar dan kartu dashboard |

## 9. Alur bisnis target

`FLOW-AST-MVP-002` (mutasi) dan `FLOW-AST-MVP-003` (peminjaman): `flowcharts/01-mutasi.md`, `02-peminjaman.md`.

## 10. Epic dan functional requirement

### EPIC AST-08 — Mutasi antar unit · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-070` | Mutasi hanya untuk aset dari satu unit asal, ke unit berbeda | IGD+ICU dalam satu mutasi ditolak |
| `FR-AST-071` | Dua persetujuan; tahap 1 otomatis bila pengaju Kepala Unit asal | Kepala IGD mengajukan → langsung ke Bagian Aset |
| `FR-AST-072` | Penempatan berpindah saat diterima; unit tujuan mengisi ruangan dan penanggung jawab | Lihat contoh `01-mutasi.md` |
| `FR-AST-073` | Kondisi berbeda saat diterima membuka laporan kondisi | — |
| `FR-AST-074` | Mutasi selesai tidak dapat dibatalkan; koreksi = mutasi baru beserta rujukan | — |
| `FR-AST-075` | Aset terkunci selama mutasi | Aset dalam mutasi tidak dapat dipinjam |

### EPIC AST-09 — Peminjaman · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-080` | Pinjaman wajib rencana tanggal kembali | — |
| `FR-AST-081` | Terlambat dihitung saat dibaca | Pinjam 1–3 Juni → Terlambat 4 Juni |
| `FR-AST-082` | Perpanjangan dengan persetujuan pemilik; satu terbuka per pinjaman | — |
| `FR-AST-083` | Pengembalian dikonfirmasi pemilik dengan kondisi per aset | Kabel putus → laporan kondisi |
| `FR-AST-084` | Pinjaman tidak mengubah unit pemilik aset | Daftar aset IGD tetap memuat pompa yang dipinjam ICU, bertanda "dipinjam ICU" |

## 11. Model status

Mutasi `Diajukan → DisetujuiUnitAsal → DisetujuiBagianAset → Diserahterimakan` (+ `Ditolak`, `Dibatalkan`);
pinjaman `Diajukan → Disetujui → Dipinjam → MenungguKonfirmasiKembali → Dikembalikan` (+ `Ditolak`,
`Dibatalkan`); Terlambat turunan. Rincian: `contracts/state-transition-matrix.md`.

## 12. Sasaran arsitektur

Dipakai ulang: kunci proses, penempatan, laporan kondisi, penyelesai aktor, linimasa (`register`). Baru:
5 tabel, 4 service, 3 controller.

## 13. Sasaran kemampuan API

[`contracts/api-contract.md`](contracts/api-contract.md) 2.1 (`EPIC AST-08`), 2.2 (`EPIC AST-09`), 2.3–2.4
(keduanya). Semua **Rencana (belum tersedia)**.

## 14. Matriks kewenangan

[`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 1.

## 15. Batas integrasi dan billing

**MUST NOT** mengubah `AstAsset` langsung, mengirim kejadian ke Akuntansi, atau meminjamkan ke pihak luar.

## 16. Guardrail regulasi

Tidak ada data pasien. Berita acara serah terima memuat nama dan jabatan, bukan nomor identitas pegawai.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-MOV-001` | Serah terima seluruh butir dalam satu transaksi |
| `NFR-MOV-002` | Konkurensi `RowVersion` pada setiap langkah |
| `NFR-MOV-003` | Hari Terlambat dihitung menurut tanggal WIB |

## 18. Skenario UAT

| ID | Kondisi awal | Langkah | Hasil | Epic |
|---|---|---|---|---|
| `UAT-MOV-01` | Monitor di IGD | Mutasi ke ICU sampai diterima | Monitor di ICU-2, berita acara tercetak | AST-08 |
| `UAT-MOV-02` | Mutasi menunggu Bagian Aset | Kepala ICU mencoba menyetujui | Tombol tidak tampil; panggilan langsung ditolak | AST-08 |
| `UAT-MOV-03` | Mutasi menunggu diterima | ICU menolak terima | Mutasi `Ditolak`, monitor tetap di IGD | AST-08 |
| `UAT-MOV-04` | Pompa IGD | ICU pinjam 1–3 Juni, belum kembali 4 Juni | Tampil Terlambat | AST-09 |
| `UAT-MOV-05` | Pinjaman berjalan | Kembali rusak | Laporan kondisi terbuka | AST-09 |
| `UAT-MOV-06` | Pinjaman `Dipinjam` | Coba batalkan | Ditolak; harus dikembalikan | AST-09 |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Mutasi berjalan penuh dengan dua persetujuan | `UAT-MOV-01`, `AT-MOV-01` |
| Penempatan tidak pernah berubah sebelum diterima | `AT-MOV-01`, `AT-MOV-29` |
| Larangan menyetujui sendiri | `AT-MOV-26` |
| Terlambat terlihat | `UAT-MOV-04` |
| Pengembalian rusak membuka laporan kondisi | `UAT-MOV-05` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat |
|---|---|---|
| `MVP-2` | `EPIC AST-08`, `AST-09` | `register` `MVP-1` selesai |
| `MVP-3` (bersama `inventarisasi`) | Pemasangan `AST-VAL-MOV-006` | `inventarisasi` dibangun |

| Pertanyaan | Siapa | Dampak | Memblokir |
|---|---|---|:---:|
| `AST-UNK-004` penugasan manajerial Kepala Unit sudah diisi HR? | Rizki + HR | Persetujuan tahap 1 dan penerimaan tidak dapat dilakukan | Ya untuk go-live, tidak untuk pengerjaan |
