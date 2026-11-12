# Inventarisasi Fisik — PRD → MVP

## 1. Identitas dokumen

| Field | Value |
|---|---|
| Produk / modul | Quilvian — Manajemen Aset & Inventaris / `inventarisasi` |
| Status | `draft` · `AST-INV-1.0.0` |
| Repository / SHA | BE `rizkiG` `1f54a175`, FE `RizkiV2` `e49e8a81` |
| Cakupan | Jadwal opname, pemindaian QR, pencocokan, selisih, verifikasi, berita acara, tindak lanjut, dashboard 1.5, laporan inventaris |

## 2. Ringkasan eksekutif

Bagian Aset dapat membuktikan, minimal setahun sekali per unit, bahwa catatan aset sesuai kenyataan —
dengan pemindaian QR di lokasi, berita acara yang disetujui, dan tindak lanjut selisih yang tidak hilang.

## 3. Masalah produk

Belum ada kemampuan opname. Label QR dan register tersedia dari `register`; pemindaian QR belum ada di
frontend (`TS-18`).

## 4. Visi produk

Jadwal → mulai (snapshot, unit ditahan) → pindai → cocokkan → verifikasi → usulan tindak lanjut diproses
lewat mutasi, penyerahan, laporan kondisi, atau draft aset.

## 5. Batas MVP

**Mulai:** `register` dan `pergerakan` selesai; aset sudah berlabel. **Akhir:** satu unit dapat diopname
sampai `Diverifikasi`, seluruh usulan dapat diproses atau diabaikan, dan penahanan mutasi/pinjaman/penyerahan
selama opname berlaku.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab |
|---|---|
| Petugas Aset | Menjadwalkan, memproses usulan |
| Tim Inventarisasi (ketua dan anggota) | Memulai, memindai, menutup, memberi catatan, mengajukan |
| Kepala Bagian Aset | Memverifikasi, membatalkan, mengabaikan usulan |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID | Keputusan |
|---|---|---|
| Jadwal, pemeriksaan, pencocokan, selisih, verifikasi, berita acara | `CAP-07.1`–`07.6` | Wajib |
| Inventaris tertunda | `CAP-01.5` | Wajib |
| Laporan inventaris | `CAP-11.4` | Wajib |
| Penahanan unit selama opname | `AST-RULE-023` | Wajib; dipasang ke `pergerakan` dan `register` |

## 8. Kemampuan yang ditunda

| Kemampuan | Alasan | Pengganti |
|---|---|---|
| Pemindaian luring (tanpa sinyal) | Tidak diputuskan; butuh penyimpanan lokal dan sinkronisasi | Pindaian gagal kirim tetap terlihat dengan tombol kirim ulang; ruangan tanpa sinyal dicatat lewat ketik nomor setelah tersambung |
| Pemindai barcode/genggam khusus | Label memakai QR (`AST-DEC-010`) | Kamera HP/tablet |

## 9. Alur bisnis target

`FLOW-AST-MVP-004`: `flowcharts/00-alur-utama.md`, `01-pemeriksaan-dan-pencocokan.md`, `02-tindak-lanjut-selisih.md`.

## 10. Epic dan functional requirement

### EPIC AST-10 — Inventarisasi fisik · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-090` | Satu sesi aktif per unit; sesi tidak dapat dimulai bila ada proses berjalan di unit | Mutasi `AST-MUT-000045` belum diterima → mulai ditolak |
| `FR-AST-091` | Snapshot dibekukan saat mulai | Aset yang ditempatkan sesudahnya tidak masuk daftar diharapkan |
| `FR-AST-092` | Pindai lewat kamera atau ketik nomor; pindai ulang menggantikan | — |
| `FR-AST-093` | Enam kategori hasil dihitung otomatis | Contoh 312 aset pada `01-pemeriksaan-dan-pencocokan.md` |
| `FR-AST-094` | Selisih wajib diberi catatan sebelum diajukan | — |
| `FR-AST-095` | Verifikator Kepala Bagian Aset bukan anggota tim | — |
| `FR-AST-096` | Verifikasi membuat usulan; data aset tidak berubah langsung | 7 usulan, nol perubahan aset |
| `FR-AST-097` | Usulan diproses menjadi mutasi, penyerahan, laporan kondisi, atau draft aset; atau diabaikan dengan alasan | — |
| `FR-AST-098` | Penahanan mutasi, pinjaman, dan penyerahan selama sesi `Berjalan`..`MenungguVerifikasi` | — |
| `FR-AST-099` | Berita acara memuat ringkasan kategori dan nama/waktu verifikator (`AST-DEC-046`) | — |

## 11. Model status

`Dijadwalkan → Berjalan → Pencocokan → MenungguVerifikasi → Diverifikasi` (+ `Dibatalkan`); usulan
`Usulan → Diproses | Diabaikan`. Rincian: `contracts/state-transition-matrix.md`.

## 12. Sasaran arsitektur

Dipakai ulang: register, penempatan, kondisi, mutasi, lampiran. Baru: 6 tabel, 7 service, 3 controller,
komponen pemindai QR.

## 13. Sasaran kemampuan API

[`contracts/api-contract.md`](contracts/api-contract.md) 2.1–2.4, seluruhnya `EPIC AST-10`, **Rencana (belum tersedia)**.

## 14. Matriks kewenangan

[`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 1.

## 15. Batas integrasi dan billing

**MUST NOT** mengubah lokasi atau kondisi aset langsung, dan tidak mengirim kejadian ke Akuntansi.

## 16. Guardrail regulasi

Tidak ada data pasien. Foto ruangan klinis tidak boleh memuat pasien; petunjuk ini ditulis pada layar
pemindaian.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-INV-001` | Simpan pindaian < 1 detik pada jaringan RS |
| `NFR-INV-002` | Tutup pemindaian 5.000 aset < 30 detik |
| `NFR-INV-003` | Layar pemindaian dapat dipakai di HP lebar 360 px |

## 18. Skenario UAT

| ID | Kondisi awal | Langkah | Hasil | Epic |
|---|---|---|---|---|
| `UAT-INV-01` | IGD berlabel | Opname penuh sampai verifikasi | Berita acara tercetak; usulan sesuai selisih | AST-10 |
| `UAT-INV-02` | Mutasi IGD belum diterima | Mulai opname IGD | Ditolak dengan nomor mutasi | AST-10 |
| `UAT-INV-03` | Opname IGD berjalan | Ajukan mutasi aset IGD | Ditolak | AST-10 |
| `UAT-INV-04` | Selisih tanpa catatan | Ajukan verifikasi | Ditolak | AST-10 |
| `UAT-INV-05` | Verifikasi selesai | Proses usulan beda unit | Mutasi koreksi terbentuk | AST-10 |
| `UAT-INV-06` | HP tanpa izin kamera | Pindai | Pesan; ketik nomor berhasil | AST-10 |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Satu unit dapat diopname sampai `Diverifikasi` | `UAT-INV-01` |
| Penahanan unit berlaku dua arah | `UAT-INV-02`, `UAT-INV-03` |
| Data aset tidak berubah tanpa alur pemiliknya | `AT-INV-07` |
| Seluruh usulan dapat diproses atau diabaikan | `AT-INV-08..10` |
| Pemindaian kamera bekerja di HP | `AT-INV-13`, `UAT-INV-06` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat |
|---|---|---|
| `MVP-3` | `EPIC AST-10` termasuk pemasangan gerbang opname ke `pergerakan` dan `register` | `MVP-2` selesai |

Tidak ada pertanyaan terbuka yang memblokir sub-modul ini.
