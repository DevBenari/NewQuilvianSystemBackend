# Laporan Perubahan Backend — `BE-BKC-031`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-031` — Pemeriksaan data induk tarif PPN |
| `TASK TYPE` | Audit data (bukan pekerjaan kode) — pemeriksaan langsung terhadap `MstTaxRule` di database |
| `COMPLEXITY` | `LIGHT` — tidak ada logika atau source yang diubah |
| `CLASSIFICATION SCORE` | 0 — murni pembacaan data, tidak ada logika uang/API/entity yang ditulis |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` (audit data, bukan implementasi) |
| `WRITE TARGET` | Tidak ada source yang ditulis. Wewenang tulis terbatas pada `task/report/backend/BE-BKC-031.md` beserta bukti roadmap/traceability |
| Gelombang | `MVP-10` |
| Blueprint | `BKC-DES-020` (tindakan data, bukan kode) |
| Kontrak berlaku | Tidak ada perubahan kontrak |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / MasterData` (tabel `MstTaxRule`) |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE` |
| Status registry | **`ACTIVE`** |
| Keberlakuan | Tidak ada `NEW CODE`/`TOUCHED LEGACY`/`LEGACY MIGRATION` — task ini murni `SELECT` read-only ke tabel yang sudah ada, tidak menyentuh source maupun schema |
| QBE ID yang berlaku | Tidak ada QBE implementasi yang berlaku — tidak ada source yang ditulis. Keselamatan database dipatuhi: query yang dijalankan murni `SELECT`, tanpa `INSERT`/`UPDATE`/`DELETE`/migration |
| Otorisasi akses database | **Diminta dan diberikan eksplisit oleh pengguna** sebelum query dijalankan (task ini secara definisi menuntut pembacaan data nyata, bukan sekadar memvalidasi perubahan source) |

---

## 1. Sifat task ini

Task ini **bukan pekerjaan kode**. Mekanisme alokasi PPN proporsional per komponen sudah lengkap
di `BillingCalculationService.cs` (`ApplyInvoiceTax`, `LoadInvoiceTaxRuleAsync`) — tidak ada satu
baris pun yang perlu diubah. Scope-nya murni memeriksa **data induk** (`MstTaxRule.AllocationRule`)
yang sedang aktif di database, untuk memastikan konfigurasinya benar-benar `PROPORTIONAL` — nilai
yang menjadi asumsi seluruh perhitungan alokasi pajak per komponen sejak `BE-BKC-017`.

**Kenapa ini bisa salah secara diam-diam (alasan task ini ditulis).** Bila `AllocationRule`
tersimpan sebagai nilai lain, atau bila ada dua tarif PPN aktif tumpang tindih, seluruh Rp yang
sama tetap menjumlah ke total tagihan yang benar — kesalahan hanya muncul pada bagaimana Rp itu
**dibagi** antara Pajak Asuransi dan Pajak Mandiri. Tidak ada galat yang terpicu, sehingga
kesalahan seperti ini tidak akan pernah terdeteksi test otomatis yang hanya memeriksa total.

## 2. Metode pemeriksaan

Dengan otorisasi eksplisit pengguna (dikonfirmasi sebelum query dijalankan), pemeriksaan dilakukan
lewat query `SELECT` read-only langsung ke database dev (`QuilvianNewDevYasmina`) memakai
`Npgsql.dll` yang sudah ter-build sebelumnya di `bin/Debug/net9.0/` (dijalankan lewat `dotnet fsi`
sebagai host skrip — **bukan** `dotnet build`/`dotnet test` terhadap project ini, sesuai instruksi
task untuk tidak menyentuh siklus build/test milik pengguna).

Dua query dijalankan:

1. Seluruh baris `MstTaxRule` (termasuk yang tidak aktif/terhapus), untuk melihat riwayat konfigurasi.
2. Hitung baris yang benar-benar efektif pada waktu pemeriksaan (`IsActive=true`, `IsDelete=false`,
   `EffectiveFrom <= now()`, `EffectiveTo IS NULL OR now() < EffectiveTo`).

Tidak ada `INSERT`/`UPDATE`/`DELETE` yang dijalankan. Kredensial database **tidak** dicetak pada
bagian mana pun laporan ini, sesuai `AGENTS.md` § Keselamatan Rahasia dan Konfigurasi.

## 3. Hasil pemeriksaan (bukti nyata, 5 September 2026)

| Code | Name | AllocationRule | Rate | RoundingMode | EffectiveFrom | EffectiveTo | IsActive | IsDelete |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `PPN-001` | PPN 11% | `PROPORTIONAL` | 11% | `UP` | 2026-09-02 | — (terbuka) | `true` | `false` |
| `PPN-01` | PPN | `PROPORTIONAL` | 11% | `UP` | 2026-08-27 | — (terbuka) | `false` | `true` (soft-deleted) |

**Jumlah tarif efektif sekarang (`now()`):** **1** (`PPN-001`) — sesuai harapan, tidak ada tumpang
tindih.

### Pemeriksaan terhadap acceptance criteria

| Acceptance | Hasil |
| --- | --- |
| 1. Nilai `AllocationRule` pada seluruh tarif PPN aktif diperiksa dan dicatat | **Selesai.** Satu-satunya baris aktif (`PPN-001`) bernilai `PROPORTIONAL` — sama persis dengan `TaxRuleDtos.cs:11` (`TaxRuleValues.Proportional = "PROPORTIONAL"`), nilai yang diasumsikan `ApplyInvoiceTax` untuk pembagian pajak proporsional per komponen |
| 2. Terbukti hanya ada satu tarif PPN aktif pada satu waktu | **Terbukti.** `ActiveNowCount = 1`. Baris kedua (`PPN-01`) sudah soft-deleted (`IsDelete=true`) sehingga tidak ikut dihitung `LoadInvoiceTaxRuleAsync` — konsisten dengan penjaga "lebih dari satu tax rule aktif" pada kode yang sama |
| 3. Bila ditemukan nilai yang salah, koreksinya dicatat beserta siapa dan kapan | **Tidak diperlukan** — tidak ditemukan nilai yang salah. `AllocationRule` sudah benar pada baris yang aktif, dan tidak ada tumpang tindih tarif aktif |
| 4. `UAT-17` tidak dapat dinyatakan lulus sebelum task ini selesai | **Task ini selesai** — `UAT-17` boleh dilanjutkan pemiliknya berdasarkan bukti di atas |

## 4. Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Nihil** — tidak ada endpoint atau kontrak yang disentuh |
| `DATABASE IMPACT` | **Nihil** — murni `SELECT`, tidak ada skema, baris, atau konfigurasi yang diubah |
| `SECURITY IMPACT` | **Nihil** — akses read-only dengan otorisasi eksplisit pengguna; kredensial tidak dicetak di laporan ini atau di transkrip yang disimpan |
| `VISUAL REFERENCE` | `NOT REQUIRED` |

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build`/`dotnet test` project ini | **TIDAK DIJALANKAN, dan memang tidak relevan** | `NOT APPLICABLE` | Task ini tidak mengubah satu baris source pun — DoD-nya adalah bukti data, bukan build/test. Instruksi eksplisit pengguna pada task ini juga meminta tanpa build/test |
| Query audit `MstTaxRule` | **DIJALANKAN, LULUS** | Manual, dengan otorisasi eksplisit pengguna | Lihat §3 — dua query `SELECT` read-only terhadap `QuilvianNewDevYasmina`, hasil dicatat apa adanya |
| Verifikasi statis nilai harapan | **LULUS** | Manual | `TaxRuleDtos.cs:11` — `TaxRuleValues.Proportional = "PROPORTIONAL"` cocok persis (case-sensitive) dengan nilai yang tersimpan pada baris aktif |
| Cakupan diff | **LULUS** | Manual | `git status --short` menunjukkan **nol** perubahan pada source aplikasi akibat task ini — hanya laporan task dan bukti roadmap/traceability |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| Nilai `AllocationRule` diperiksa dan dicatat | **Terpenuhi** — §3 |
| Hanya satu tarif PPN aktif pada satu waktu | **Terpenuhi dan dibuktikan** — `ActiveNowCount=1` |
| Koreksi dicatat bila ada | **Tidak ada koreksi yang diperlukan** |
| Tidak ada perubahan source aplikasi | **Terpenuhi** — nol berkas source disentuh |

**Definition of Done tercapai.** Task ini selesai — tidak ada langkah lanjutan yang tertunda pada
task ini sendiri.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Tidak ada |
| `KNOWN ISSUES` | `BKC-OQ-088` (peringatan otomatis untuk salah konfigurasi `AllocationRule`/tumpang tindih tarif) **sengaja ditunda** ke luar rilis ini sesuai roadmap — pemeriksaan task ini bersifat manual satu kali, bukan pengawasan berkelanjutan |
| `MANUAL TEST` | `NOT APPLICABLE` — task audit data, bukan fitur yang diuji manual di UI |
| `INCIDENTAL CHANGES` | `NONE` |
| `INTERRUPTIONS` | `NONE` |
| `GIT STATUS` | Nol berkas source berubah akibat task ini. Working tree tetap memuat pekerjaan task-task lain (`BE-BKC-022` sampai `030`) yang belum di-build/test pengguna — tidak disentuh atau digabung oleh task ini |
| `NEXT RECOMMENDED STEP` | Tidak ada langkah lanjutan untuk `BE-BKC-031` sendiri. `UAT-17` boleh dinyatakan lulus oleh pemiliknya berdasarkan bukti di §3 |
