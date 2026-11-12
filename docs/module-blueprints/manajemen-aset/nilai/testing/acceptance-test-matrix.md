# Nilai Aset — Matriks Uji Penerimaan

| Field | Value |
|---|---|
| `last_changed_in` | `AST-NLI-1.0.0` — `draft` |

`DepreciationCalculator` adalah fungsi murni dan diverifikasi dengan contoh berangka berikut (skenario
`UNIT` = pemeriksaan logika; automated test backend tidak diwajibkan, `rules/backend/TEST_POLICY.md`).
Seluruh skenario lain adalah skenario verifikasi manual/runtime.

## 1. Jalur berhasil

| ID | Requirement | Skenario | Jenis | Bukti |
|---|---|---|---|---|
| `AT-NLI-01` | `AST-AC-009` | Monitor Rp 85 jt, 60 bulan, siap pakai 20 Mar 2026 | UNIT | April = Rp 1.416.667; bulan ke-60 menutup akumulasi tepat Rp 85.000.000 |
| `AT-NLI-02` | `AST-RULE-041` | Aset dihapus 10 Agu 2028 | INTEG | Proses Agustus tidak memuat aset itu; Juli memuat |
| `AT-NLI-03` | `AST-RULE-046` | Mutasi IGD → ICU 15 April | INTEG | Baris April berpusat biaya ICU |
| `AT-NLI-04` | `AST-DES-010` | Disetujui 3 Mei, siap pakai 10 Feb | INTEG | Proses Mei: penyesuaian Maret, April + Mei |
| `AT-NLI-05` | `AST-RULE-047` | Koreksi Rp 58 jt → Rp 85 jt sesudah 6 bulan | UNIT + INTEG | Penyesuaian Rp 2.700.000 |
| `AT-NLI-06` | `AST-RCG-G17` | Umur 60 → 72 sesudah 24 bulan, nilai buku Rp 51 jt | UNIT | Rp 1.062.500 |
| `AT-NLI-07` | `AST-DES-033` | Kebijakan laptop 48 → 60 bulan berlaku Juli | INTEG | Juni tetap 48; Juli dihitung ulang dari sisa |
| `AT-NLI-08` | `AST-RCG-G19` | Saldo awal: Rp 60 jt, akumulasi Rp 24 jt, sudah 24 dari 60 bulan | UNIT | Rp 1.000.000 per bulan |
| `AT-NLI-09` | `AST-DEC-039` | Proses April | INTEG | Satu kejadian per (kelompok, pusat biaya); komponen = kode kelompok |
| `AT-NLI-10` | `AST-DES-035` | Saldo subledger April | INTEG | Satu kejadian per akun kontrol; jumlah = Σ buku |
| `AT-NLI-11` | `AST-DES-034` | Pengiriman dimatikan | INTEG | Baris `Held`; `AttemptCount` tetap 0 |
| `AT-NLI-12` | `AST-DES-034` | Dinyalakan, Akuntansi menjawab 201 | INTEG | `Acknowledged`, `AccountingEventId` terisi |
| `AT-NLI-13` | `AST-DEC-040` | Hibah disetujui | INTEG | Satu kejadian `AST-PEROLEHAN-HIBAH` |
| `AT-NLI-14` | `AST-DEC-041` | Perbaikan besar disetujui | INTEG | Buku naik; **nol** kejadian |
| `AT-NLI-15` | Penurunan nilai | Rp 12 jt disetujui | INTEG | Nilai buku turun; satu kejadian `AST-PENURUNAN-NILAI` |
| `AT-NLI-16` | `DEP-AST-01` (bersama Akuntansi) | Kejadian berpusat biaya ICU dengan aturan posting tanpa pusat biaya | INTEG (lintas modul) | Baris jurnal beban memakai pusat biaya ICU |

## 2. Jalur gagal

| ID | Requirement | Skenario | Jenis | Bukti |
|---|---|---|---|---|
| `AT-NLI-30` | `AST-VAL-NLI-011` | Proses April dua kali | INTEG | 409; satu proses |
| `AT-NLI-31` | `AST-VAL-NLI-012` | Proses Mei sebelum April | INTEG | 409 |
| `AT-NLI-32` | `AST-VAL-NLI-014` | Unit dengan dua pusat biaya tanpa pemetaan | INTEG | Galat pratinjau; proses 409 |
| `AT-NLI-33` | `AST-VAL-NLI-008` | Setujui aset sebelum parameter ada | INTEG | 409 di `register` |
| `AT-NLI-34` | `AST-VAL-NLI-002` | Tetapkan parameter berlaku bulan ini (bukan baris pertama) | INTEG | 400 |
| `AT-NLI-35` | `AST-VAL-NLI-024` | Penurunan nilai melebihi nilai buku | INTEG | 400 |
| `AT-NLI-36` | `AST-VAL-NLI-026` | Kepala Akuntansi menyetujui pengajuannya sendiri | INTEG | 403 |
| `AT-NLI-37` | Transaksi proses | Kegagalan di tengah proses 1.200 aset | INTEG | Nol baris, nol entri, nol kotak keluar |
| `AT-NLI-38` | `AST-DES-034` | Akuntansi menjawab 422 lima kali | INTEG | `Failed`; coba ulang mengembalikan ke `Pending` |
| `AT-NLI-39` | Idempotensi | Pekerja mengirim baris yang sama dua kali | INTEG | Akuntansi menyimpan satu kejadian |
| `AT-NLI-40` | `AST-DEC-025` | Kepala Unit memanggil `/valuation/books` | INTEG | 403 |
| `AT-NLI-41` | `CK_AstAssetBook_NotBelowResidual` | Penyusutan yang akan menembus residu | UNIT | Bulan terakhir dibatasi tepat di residu |
