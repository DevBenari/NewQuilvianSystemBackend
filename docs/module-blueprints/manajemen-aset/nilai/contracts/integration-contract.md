# Nilai Aset — Kontrak Integrasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-NLI-1.0.0` — `draft` |
| Owner | Rizki (Aset + Akuntansi) · `approved_by`: — · `approved_at`: — |
| Traceability | `AST-DEC-015`, `036`, `038..042`, `047`; `ACC-DEC-045..049`, `061`, `066`; `TS-08`, `TS-09`, `TS-13` |

## 1. Ringkasan arah data

| Arah | Mitra | Isi |
|---|---|---|
| Aset → Akuntansi | `POST api/v1/corporate/accounting/accounting-events` | Enam jenis kejadian (bagian 3) |
| Aset → Akuntansi | Kejadian `SaldoSubledger` | Saldo per akun kontrol per periode (bagian 4) |
| Aset ← HR | `MstCostCenter`, `MstOrganizationUnit` | Baca saja |
| Aset ← Akuntansi | Daftar akun (kode, aktif) | Baca saja, untuk validasi `AST-VAL-NLI-006` |

Aset **tidak pernah** membaca jurnal atau saldo buku besar; Akuntansi **tidak pernah** membaca tabel Aset
(`ACC-DEC-061`).

## 2. Mekanisme pengiriman (`AST-DES-034`)

| Unsur | Ketentuan |
|---|---|
| Penulisan | `AssetAccountingEventService` menulis `AstAccountingOutbox` **di dalam transaksi pemicu** (proses penyusutan, persetujuan, penghapusan final) — kejadian tidak pernah hilang karena gangguan jaringan |
| Pengiriman | `AssetAccountingDispatchWorker` (`BackgroundService`), satu baris satu percobaan satu transaksi; kegagalan satu baris tidak menghentikan siklus |
| Bawaan | `AssetManagement:AccountingDispatch:Enabled = false`. Kode di luar `EnabledEventCodes` tetap `Held` |
| Kredensial | Dari konfigurasi; tidak ditanam di source dan tidak dicetak |
| Idempotensi | `SourceTransactionId` + `SourceVersion` unik. Contoh penyusutan: `DEPR:{RunId}:{KodeKelompok}:{KodePusatBiaya}` versi `1` |
| Percobaan | `MaxAttempts` (bawaan 5) lalu `Failed`; coba ulang manual oleh Kepala Akuntansi |
| Sisi Akuntansi | Tanpa pemetaan akun → daftar gagal (`ACC-DEC-046`); periode tertutup → periode terbuka berikutnya (`ACC-DEC-047`); coba ulang terjadwal 3 kali (`ACC-DEC-049`) |

## 3. Jenis kejadian (kode usulan `AST-DES-012`; final `AST-OQ-009`)

`SourceModule = AssetManagement`. Seluruh nominal rupiah 2 desimal.

| Kode | Dipicu | Satuan kejadian | `Amount` | Komponen (`ComponentCode` → nominal) | Pusat biaya |
|---|---|---|---|---|---|
| `AST-PENYUSUTAN` | Proses penyusutan | Satu per (kelompok, pusat biaya, periode) | Jumlah penyusutan + penyesuaian | `<KODE KELOMPOK>` → jumlah yang sama | **Ya**, dari kejadian (`AST-DEC-047`) |
| `AST-PENURUNAN-NILAI` | Persetujuan penurunan nilai | Satu per pengajuan | Nilai penurunan | `<KODE KELOMPOK>` | Ya, unit aset saat disetujui |
| `AST-PEROLEHAN-HIBAH` | Aset hibah disetujui | Satu per aset | Harga perolehan | `<KODE KELOMPOK>` | Tidak |
| `AST-PENGHAPUSAN` | Usulan penghapusan final | Satu per butir aset | Harga perolehan tercatat | `PEROLEHAN_<KODE>`, `AKUMULASI_<KODE>`, `PENURUNAN_<KODE>`, `HASIL_PELEPASAN`, `LABA_PELEPASAN` **atau** `RUGI_PELEPASAN` | Ya, unit terakhir |
| `AST-PEMULIHAN` | Pemulihan disetujui | Satu per aset | Sama dengan kejadian penghapusan yang dipulihkan | Komponen sama dengan penghapusan aslinya; aturan posting kebalikannya | Ya |
| `AST-SALDO-SUBLEDGER` | Proses penyusutan selesai, atau kirim ulang | Satu per (akun kontrol, periode) | Saldo | — (`SubledgerBalance`: `AccountingPeriodCode` `YYYY-MM`, `ControlAccountCode`, saldo) | Tidak |

**Contoh `AST-PENYUSUTAN`.** Proses April 2026: kelompok `ALKMED` pusat biaya ICU berisi monitor
Rp 1.416.667 dan dua pompa Rp 500.000 → satu kejadian `Amount` Rp 2.416.667, komponen `ALKMED` Rp 2.416.667,
pusat biaya ICU. Aturan posting Akuntansi: `ALKMED` debit Beban Penyusutan Alat Medis (pusat biaya kosong
→ diisi ICU dari kejadian), `ALKMED` kredit Akumulasi Penyusutan Alat Medis.

**Contoh `AST-PENGHAPUSAN`.** Ambulans `AST-KEND-000004` dijual Rp 35.000.000; biaya tercatat
Rp 400.000.000, akumulasi Rp 380.000.000, penurunan 0 → nilai buku Rp 20.000.000, laba Rp 15.000.000.
Komponen: `PEROLEHAN_KEND` 400.000.000; `AKUMULASI_KEND` 380.000.000; `HASIL_PELEPASAN` 35.000.000;
`LABA_PELEPASAN` 15.000.000. Debit = 380 jt + 35 jt = 415 jt; kredit = 400 jt + 15 jt = 415 jt. Uang
penjualan sendiri diterima dan dijurnal Finance; akun "hasil pelepasan" menjadi akun penampung yang
dipertemukan dengan penerimaan itu (`AST-OQ-009`).

## 4. Saldo subledger (`AST-DES-035`)

Setiap proses penyusutan selesai, `AssetSubledgerSnapshotService` menjumlahkan per kelompok lalu per kode
akun kontrol `AstSubledgerAccountSetup`:

| Akun kontrol | Saldo |
|---|---|
| `CostControlAccountCode` | Σ `CapitalizedCost` buku `Aktif` dan dikapitalisasi |
| `AccumulatedDepreciationControlAccountCode` | Σ `AccumulatedDepreciation` |
| `AccumulatedImpairmentControlAccountCode` | Σ `AccumulatedImpairment` |

Dua kelompok boleh menunjuk akun kontrol yang sama; saldonya dijumlahkan. Kirim ulang membuat
`SourceVersion` baru. Perbandingan dengan buku besar dikerjakan rekonsiliasi control account Akuntansi
(`ACC-DEC-066`).

## 5. Ketergantungan pada modul lain

| ID | Kebutuhan | Pemilik | Status | Menahan |
|---|---|---|---|---|
| `DEP-AST-01` | Kejadian keuangan membawa pusat biaya; baris aturan posting tanpa pusat biaya memakai pusat biaya kejadian (`AST-DEC-047`) | Akuntansi (Rizki) | Belum ada; task lintas modul | Penyalaan `AST-PENYUSUTAN`, `AST-PENURUNAN-NILAI`, `AST-PENGHAPUSAN`, `AST-PEMULIHAN` |
| `DEP-AST-02` | Enam jenis kejadian terdaftar (`AccEventType`, `SourceModule = AssetManagement`) beserta aturan posting | Akuntansi (Rizki) | Data, belum ada (`AST-OQ-009`) | Penyalaan per kode |
| `DEP-AST-03` | Template jurnal berulang penyusutan (bila ada) dinonaktifkan mulai bulan yang sama (`AST-DEC-038`) | Akuntansi (Rizki) | `AST-UNK-002` | Penyalaan `AST-PENYUSUTAN` |
| `DEP-AST-04` | Akun pengguna layanan untuk pekerja pengirim dengan izin `AccountingEvent : Receive` | Platform/Akuntansi | Pola Finance | Penyalaan pekerja |

Tanpa keempatnya modul Aset tetap berjalan: kejadian menumpuk sebagai `Held`/`Pending` dan terkirim
setelah dinyalakan, tanpa ada yang hilang.
