# Nilai Aset — PRD → MVP

## 1. Identitas dokumen

| Field | Value |
|---|---|
| Produk / modul | Quilvian — Manajemen Aset & Inventaris / `nilai` |
| Status | `draft` · `AST-NLI-1.0.0` |
| Repository / SHA | BE `rizkiG` `1f54a175`, FE `RizkiV2` `e49e8a81` |
| Cakupan | Parameter nilai, buku nilai, harga perolehan, penyusutan bulanan, perubahan nilai, perolehan hibah, kiriman ke Akuntansi, rekonsiliasi, laporan nilai |

## 2. Ringkasan eksekutif

Akuntansi mendapat penyusutan bulanan yang dihitung dari data aset nyata — per kelompok dan per pusat
biaya — tanpa jurnal manual dan tanpa tercatat dua kali, serta saldo subledger aset untuk dicocokkan
dengan buku besar setiap bulan.

## 3. Masalah produk

Akuntansi sudah siap menerima kejadian (`TS-08`), punya rekonsiliasi control account (`TS-09`), dan punya
butir tutup buku "Penyusutan belum dijalankan" yang belum aktif (`TS-11`). Yang belum ada: sumber angka
penyusutannya. Aturan posting Akuntansi juga belum dapat memakai pusat biaya dari kejadian (`AST-CON-003`).

## 4. Visi produk

Parameter ditetapkan Akuntansi → aset disetujui dengan buku nilai → setiap akhir bulan penyusutan diproses
sekali → kejadian per kelompok × pusat biaya dan saldo per akun kontrol terkirim → Akuntansi menjurnal dan
merekonsiliasi.

## 5. Batas MVP

**Mulai:** tabel dasar dan parameter ada sejak `MVP-0`; aset sudah tercatat. **Akhir:** satu periode
diproses dan terkirim (bila dinyalakan); perubahan nilai empat jenis berjalan; saldo subledger terkirim;
laporan nilai tersedia.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab |
|---|---|
| Kepala Akuntansi | Parameter, kebijakan, akun kontrol, pemetaan pusat biaya; menyetujui perubahan nilai; mencoba ulang kiriman |
| Petugas Aset | Memproses penyusutan; mengajukan perubahan nilai |
| Direktur, Kepala Bagian Aset | Melihat nilai |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID | Keputusan |
|---|---|---|
| Parameter nilai | `CAP-EXT-02` | Wajib, `MVP-0` — tanpanya aset tidak dapat disetujui |
| Harga perolehan, nilai buku | `CAP-09.1`, `09.2` | Wajib |
| Riwayat perubahan nilai (tanpa revaluasi) | `CAP-09.3` | Wajib |
| Penyusutan | `CAP-09.4` | Wajib |
| Rekonsiliasi | `CAP-09.5` | Wajib |
| Perolehan hibah | `CAP-EXT-04` | Wajib bersama register |
| Laporan nilai | `CAP-11.6` | Wajib |

## 8. Kemampuan yang ditunda

| Kemampuan | ID | Alasan | Pengganti selama MVP |
|---|---|---|---|
| Revaluasi | `CAP-09.3` sebagian | Butuh kebijakan per kelompok, akun surplus revaluasi, dan laporan penilai (`AST-DEC-032`) | Revaluasi belum pernah dilakukan lewat sistem; bila terjadi, Akuntansi menjurnal manual dan Petugas Aset mencatat catatan pada aset |
| Pembalikan penurunan nilai | `AST-RCG-G18` | Tidak diputuskan | Penurunan nilai baru hanya ke bawah |

## 9. Alur bisnis target

`FLOW-AST-MVP-005` (penyusutan) dan `FLOW-AST-MVP-006` (perubahan nilai): `flowcharts/01..02`.

## 10. Epic dan functional requirement

### EPIC AST-11 — Parameter nilai · `MISSING / NEW` · `MVP-0`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-110` | Parameter bertanggal berlaku, baris pertama boleh bulan berjalan, berikutnya bulan depan | Ditetapkan 15 Juni → berlaku Juli |
| `FR-AST-111` | Kebijakan kelompok berlaku ke depan untuk seluruh aset kelompok tanpa estimasi sendiri | Laptop 48 → 60 bulan |
| `FR-AST-112` | Akun kontrol per kelompok divalidasi terhadap daftar akun Akuntansi | Kode tak dikenal ditolak |
| `FR-AST-113` | Pemetaan unit → pusat biaya; daftar unit yang belum jelas | Rawat Inap dua pusat biaya → tampil "belum jelas" |
| `FR-AST-114` | Buku nilai terbentuk saat aset disetujui/diimpor | Kursi Rp 450 rb tidak dikapitalisasi |

### EPIC AST-12 — Penyusutan dan pengiriman · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-120` | Rumus `AST-DES-009`, pembulatan rupiah, bulan terakhir menyerap sisa | Rp 1.416.667 |
| `FR-AST-121` | Sekali per periode, berurutan, satu transaksi | Mei sebelum April ditolak |
| `FR-AST-122` | Pusat biaya unit pada akhir bulan | Mutasi 15 April → ICU |
| `FR-AST-123` | Penyesuaian aset terlambat dicatat, dipulihkan, dan koreksi harga | Rp 2.700.000 |
| `FR-AST-124` | Kejadian per kelompok × pusat biaya; kotak keluar; bawaan ditahan | `Held` sampai dinyalakan |
| `FR-AST-125` | Coba ulang kiriman gagal | — |

### EPIC AST-13 — Perubahan nilai dan hibah · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-130` | Empat jenis perubahan nilai disetujui Kepala Akuntansi bukan pengaju | Lihat `02-perubahan-nilai.md` |
| `FR-AST-131` | Hanya penurunan nilai yang mengirim kejadian | Perbaikan besar tanpa kejadian |
| `FR-AST-132` | Penurunan nilai tidak boleh menembus residu | — |
| `FR-AST-133` | Hibah masuk mengirim kejadian perolehan hibah | Rp 3 jt |

### EPIC AST-14 — Rekonsiliasi dan laporan nilai · `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-AST-140` | Saldo per akun kontrol dikirim setiap proses selesai; kirim ulang membuat versi baru | — |
| `FR-AST-141` | Laporan nilai per kelompok/unit/periode dan ekspor | — |

### EPIC AST-15 — Revaluasi · `OPEN DECISION` (tidak masuk gelombang mana pun)

## 11. Model status

Buku `Aktif`/`Dihentikan`; perubahan nilai `Diajukan → Disetujui | Ditolak | Dibatalkan`; kotak keluar
`Pending`/`Held`/`Acknowledged`/`Failed`. Invariant: nilai buku ≥ residu; satu proses per periode.

## 12. Sasaran arsitektur

Dipakai ulang: jalur kejadian Akuntansi, rekonsiliasi control account, pola kotak keluar Finance,
`MstCostCenter`. Diperluas (modul Akuntansi): pusat biaya pada kejadian (`DEP-AST-01`). Baru: 10 tabel,
10 service, 7 controller.

## 13. Sasaran kemampuan API

[`contracts/api-contract.md`](contracts/api-contract.md): 2.1–2.4 `EPIC AST-11`; 2.5 `AST-11`/`AST-14`;
2.6, 2.8 `EPIC AST-12`; 2.7 `EPIC AST-13`; 2.9–2.10 `EPIC AST-14`. Semua **Rencana (belum tersedia)**.

## 14. Matriks kewenangan

[`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 1.

## 15. Batas integrasi dan billing

**MUST NOT** membuat jurnal, membaca buku besar, mengirim kejadian perolehan pembelian/perbaikan besar/
saldo awal, atau menyalakan kiriman penyusutan sebelum `DEP-AST-01`..`03` selesai.

## 16. Guardrail regulasi

Standar akuntansi aset tetap (PSAK 216): model biaya, garis lurus, perubahan estimasi prospektif,
penurunan nilai. Kebijakan akuntansi RS tetap wewenang Kepala Akuntansi; sistem tidak mengarangnya.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-NLI-001` | Proses 10.000 aset dalam satu transaksi < 2 menit |
| `NFR-NLI-002` | Tidak ada kejadian yang hilang bila Akuntansi tidak tersedia |
| `NFR-NLI-003` | Kredensial pekerja tidak pernah tercatat di log maupun `LastError` |

## 18. Skenario UAT

| ID | Kondisi awal | Langkah | Hasil | Epic |
|---|---|---|---|---|
| `UAT-NLI-01` | Belum ada parameter | Setujui aset | Ditolak; tetapkan parameter lalu berhasil | AST-11 |
| `UAT-NLI-02` | Unit dua pusat biaya | Pratinjau | Galat; setelah dipetakan, bersih | AST-11, AST-12 |
| `UAT-NLI-03` | Aset contoh | Proses April lalu Mei | Angka sesuai `01-proses-penyusutan.md` | AST-12 |
| `UAT-NLI-04` | April sudah diproses | Proses April lagi | Ditolak | AST-12 |
| `UAT-NLI-05` | Kiriman dinyalakan | Proses Mei | Jurnal penyusutan di Akuntansi per pusat biaya | AST-12 |
| `UAT-NLI-06` | Aset Rp 58 jt keliru | Ajukan koreksi, setujui | Penyesuaian Rp 2.700.000 di proses berikutnya | AST-13 |
| `UAT-NLI-07` | Pengajuan sendiri | Kepala Akuntansi menyetujui | Ditolak | AST-13 |
| `UAT-NLI-08` | Mei diproses | Buka rekonsiliasi Akuntansi | Saldo Aset tampil per akun kontrol | AST-14 |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Rumus terbukti dengan contoh berangka | `AT-NLI-01`, `05`, `06`, `08` |
| Satu proses per periode, berurutan, atomik | `AT-NLI-30`, `31`, `37` |
| Penyusutan tidak tercatat dua kali | `AT-NLI-39` + bukti `DEP-AST-03` |
| Jurnal memakai pusat biaya unit | `AT-NLI-16`, `UAT-NLI-05` |
| Rupiah tidak terkirim ke peran tanpa hak | `AT-NLI-40` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat |
|---|---|---|
| `MVP-0` | Tabel dasar + `EPIC AST-11` | Blueprint disetujui |
| `MVP-1` | Kejadian hibah (`FR-AST-133`) bersama `register` | `MVP-0` |
| `MVP-4` | `EPIC AST-12`, `AST-13` (selain hibah), `AST-14` | `MVP-3` selesai |
| `POST-MVP` | `EPIC AST-15` Revaluasi | Keputusan kebijakan revaluasi |

| Pertanyaan | Siapa | Dampak | Memblokir |
|---|---|---|:---:|
| `AST-OQ-001..004` angka parameter | Kepala Akuntansi / Direktur | Persetujuan aset dan penghapusan tidak dapat dihitung | Ya untuk go-live, tidak untuk pengerjaan |
| `AST-OQ-009` kode kejadian dan aturan posting | Kepala Akuntansi | Kejadian masuk daftar gagal Akuntansi | Ya untuk penyalaan kiriman |
| `AST-UNK-002` template jurnal berulang penyusutan | Kepala Akuntansi | Risiko dobel | Ya untuk penyalaan `AST-PENYUSUTAN` |
| `DEP-AST-01` perluasan pusat biaya Akuntansi | Rizki (Akuntansi) | Jurnal tanpa pusat biaya yang benar | Ya untuk penyalaan; tidak untuk pengerjaan Aset |
