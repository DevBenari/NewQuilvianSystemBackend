# Laporan Perubahan Frontend — `FE-FIN-037`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-037` |
| Judul | Tujuh layar cutover dan master ambang dapat ditemukan dari menu, bukan hanya dari alamat langsung |
| Slice | `REV-16F1` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian `task_range_frontend: FE-FIN-033..FE-FIN-037` |
| Trace | `FR-FIN-191`; `FIN-DEC-148`, `FIN-DEC-160`; `03-frontend-architecture.md` §21.2, §21.6 |
| Contract version | **Nol kontrak API.** `FIN-PERM-1.8` untuk `resource`/`action` tiap butir |
| Reuse | Pola grup `subMenu`/`subItems` `menu-items.jsx` yang sudah ada — persis bentuk `corporateAccountingMasterData` dan `corporateFinanceMasterData` yang sudah ada di berkas yang sama |
| Dependency | **Nol dependency backend** — satu-satunya task pada rentang `FE-FIN-033..037` yang boleh mulai tanpa menunggu apa pun (`REV-16F1`, "Boleh mulai sekarang") |
| Wewenang UI | **Bukan** `DEV_DISCRETION` untuk nama submenu, label butir, urutan ketiga butir cutover, dan penempatan Ambang di Master Data (`FIN-DEC-148`, `160`, approved product/UI brief). `DEV_DISCRETION` untuk ikon dan posisi submenu baru di antara submenu Finance lainnya (lihat §3) |
| Klasifikasi | `LIGHT` — 1 berkas diubah (nol berkas baru), nol berkas test ditambah (test existing yang menyentuh berkas ini sudah cukup, lihat §7) |
| Task mode | `FRONTEND`. Target tulis `QuilvianSystemFrontendDev`; laporan ini ditulis di repository backend karena di sanalah `docs/module-blueprints/` tinggal |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `66d90570d` (branch `yasmina`) |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `npm run lint:errors`, `npm run test:unit`, `npm run build` seluruhnya PASS (dijalankan sesi ini). `npm run dev` **TIDAK DIJALANKAN** — satu-satunya sisa adalah pemeriksaan menu manual sebagai pemegang hak dan yang tidak berhak (§7) |

---

## 0. Dependency Backend — Nol, Task Ini Tidak Tertahan Apa Pun

Berbeda dari `FE-FIN-033`..`036`, roadmap menyatakan eksplisit **"Nol dependency backend"** untuk
task ini — keempat route yang diberi butir menu (`control-accounts`, `opening-balances`,
`opening-item-batches`, `direct-payment-threshold`) **sudah ada dan berfungsi** sejak
`FE-FIN-027`/`028`/`031`/`032`, hanya belum dapat dicapai lewat menu. Task ini murni menyambungkan
navigasi ke layar yang sudah hidup, tidak membaca kontrak baru apa pun.

---

## 1. Masalah yang Diperbaiki

Empat layar (tiga di bawah submenu baru, satu di submenu yang sudah ada) **sudah dibangun dan
berfungsi**, tetapi hanya dapat dicapai lewat alamat langsung atau tautan silang —
diverifikasi langsung: `grep` atas `direct-payment-threshold`, `subledger-setup`,
`opening-balances`, `opening-item-batches`, `control-accounts` di `menu-items.jsx` **sebelum**
task ini menghasilkan **nol** kecocokan. Petugas tidak punya jalan menemukan layar-layar ini
dari sidebar sama sekali — inilah `FIN-OQ-079`, sudah `CLOSED` oleh `FIN-DEC-148` secara keputusan,
tetapi source-nya belum menerapkannya.

---

## 2. Proses Bisnis dari Sisi Pengguna

1. Grup **Keuangan** kini punya submenu **baru** "Cutover & Subledger", berisi tiga butir
   berurutan: **Pemetaan Akun Control** → **Saldo Awal Cutover** → **Batch Migrasi Tagihan
   Lama**. Urutan ini **bukan selera** — mengikuti urutan kerja petugas yang sesungguhnya
   (`FIN-DEC-160`): batch migrasi tidak dapat diunggah sebelum saldo awal cutover dicatat, dan
   snapshot subledger ditolak bila pemetaan akun belum lengkap.
2. Submenu **Master Data** yang sudah ada kini punya butir tambahan di akhir: **Ambang
   Pembayaran Langsung** — bukan submenu baru, karena ia memang master data (nilai tunggal
   yang diubah, bukan daftar transaksi).
3. Setiap butir baru membawa `requiredPermission` yang benar, sehingga **tersembunyi** bagi
   pemegang hak yang tidak punya `Read` pada resource terkait — mengikuti mesin filter menu
   yang sudah ada (`filter-menu-items-by-permission.jsx`, tidak disentuh task ini).
4. **Dua layar sengaja TIDAK diberi butir menu** (dikonfirmasi tetap sesuai `03-frontend-architecture.md`
   §21.2, tidak ada perubahan): Buku Mutasi Kas (dicapai dari layar Kas & Bank/Setoran & Kas
   Harian) dan Riwayat Mutasi Piutang/Utang Supplier (dicapai dari rincian piutang/utang) —
   keduanya permukaan baca milik layar induknya, bukan layar berdiri sendiri.
5. Menu **Transaksi A/R** dan **Transaksi A/P** **tidak tersentuh sama sekali** — diverifikasi
   lewat `git diff` (lihat §5): kedua hunk perubahan berada tepat **sebelum** blok Transaksi A/R
   (di dalam Master Data) dan tepat **setelah** blok Transaksi A/P (sebelum Petty Cash) — nol
   baris di antara `key: "financeTransactionAr"` dan `key: "financeTransactionAp"` diubah.

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

**UI GATE**: nol komponen baru, nol pola baru. Struktur `{ label, key, icon, pathname,
requiredPermission }` dan pengelompokan `subMenu`/`subItems` adalah **REUSE** murni dari pola
yang sudah berulang puluhan kali di berkas yang sama (termasuk `corporateAccountingMasterData`
dan `corporateFinanceMasterData` yang sudah ada tepat di sampingnya).

Dua keputusan yang **bukan** `DEV_DISCRETION`, diterapkan apa adanya sesuai `FIN-DEC-148`/`160`:
nama submenu "Cutover & Subledger", label dan urutan ketiga butirnya, serta penempatan "Ambang
Pembayaran Langsung" di submenu Master Data yang sudah ada.

Satu keputusan `DEV_DISCRETION` eksplisit yang saya ambil, dicatat untuk transparansi:
**posisi submenu baru di antara submenu Finance lainnya.** `FIN-DEC-148`/`160` menetapkan ia
"berdiri di samping" Transaksi A/R dan A/P (bukan DI DALAM keduanya) dan urutan INTERNAL ketiga
butirnya — tetapi tidak menetapkan slot pastinya di antara Master Data/Petty Cash/Cash
Monitoring. Saya menempatkannya **tepat setelah Transaksi A/P, sebelum Petty Cash** — secara
literal "di samping" kedua menu itu (bersebelahan langsung dengan keduanya dalam urutan daftar),
bukan posisi lain yang juga sah secara kontrak. Ikon kelima butir (`RiFlowChart`, `RiSafeLine`,
`RiHistoryLine`, `RiFileList3Line`, `RiSettingsLine`) murni `DEV_DISCRETION` (§21.6) — dipilih
mengikuti kecocokan semantik dan konsistensi dengan ikon sejenis yang sudah ada di berkas yang
sama (`RiSafeLine` dipakai juga untuk "Rekonsiliasi Control Account" di grup Akuntansi — konsep
yang sama).

---

## 4. Endpoint yang Dikonsumsi dan Pemetaan Kontrak

**Nol endpoint baru, nol route baru.** Keempat route yang diberi butir menu **sudah ada** sejak
task sebelumnya; task ini hanya menambah jalan masuk navigasi:

| Butir menu | Route | `requiredPermission` |
| --- | --- | --- |
| Pemetaan Akun Control | `/finance/subledger-setup/control-accounts` | `FinanceSubledgerSetup:Read` |
| Saldo Awal Cutover | `/finance/subledger-setup/opening-balances` | `FinanceSubledgerSetup:Read` |
| Batch Migrasi Tagihan Lama | `/finance/subledger-setup/opening-item-batches` | `FinanceOpeningItemBatch:Read` |
| Ambang Pembayaran Langsung | `/finance/master-data/direct-payment-threshold` | `MstDirectPaymentThreshold:Read` |

Keempat `resource` diverifikasi langsung dari `permissionResource` pada konstanta layar masing-masing
(`subledger-control-account-constants.jsx`, `opening-balance-constants.jsx`,
`opening-item-batch-constants.jsx`, `direct-payment-threshold-constants.jsx`) — bukan ditebak.

---

## 5. Berkas yang Diubah

Nol berkas baru. Satu berkas **diubah**:

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Dua hunk aditif: (1) satu butir "Ambang Pembayaran Langsung" ditambahkan di akhir `subItems` milik `corporateFinanceMasterData`; (2) satu submenu baru `corporateFinanceCutoverSubledger` (3 butir) disisipkan antara penutup `financeTransactionAp` dan pembuka `corporateFinancePettyCashGroup`. Nol baris di dalam `financeTransactionAr`/`financeTransactionAp` tersentuh — dikonfirmasi lewat `git diff` yang hanya menunjukkan dua hunk aditif tanpa penghapusan satu baris pun |

---

## 6. Dampak Database dan Backend

`NOT APPLICABLE`. `FRONTEND MODE` murni. Nol file backend dibaca atau diubah — task ini tidak
memerlukan referensi backend sama sekali (nol kontrak API baru).

---

## 7. Verifikasi dan Validasi

| Pemeriksaan | Hasil | Keterangan |
| --- | :--: | --- |
| `npm run lint:errors` | `PASS` | Nol error |
| `npm run test:unit` | `PASS` (tanpa regresi) | Jalankan penuh, dibandingkan **nama demi nama** dengan baseline (`git stash` lalu jalankan ulang): **persis himpunan 23 nama test gagal yang sama** (49 instance total) pada kedua kondisi — hanya durasi (ms) yang berbeda. `petty-cash-finance-separation.test.mjs` ("CASE 1 & CASE 2") **termasuk** di antaranya, tetapi dikonfirmasi gagal dengan alasan yang sama sekali tidak terkait (label `"Finance"` vs ekspektasi test `"Keuangan"` — ketidaksesuaian pra-ada sejak sebelum task ini, dibuktikan `git stash`) |
| `npm run build` | `PASS` | Exit code `0`; nol "Failed to compile" |
| `npm run dev` | **TIDAK DIJALANKAN** | Instruksi eksplisit pengguna |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

**Alasan konkret**: memeriksa menu sungguhan sebagai pemegang hak vs yang tidak berhak menuntut
server dev berjalan dan sesi login dengan peran berbeda — di luar cakupan sesi ini sesuai
instruksi eksplisit pengguna. Cakupan yang **tercakup** tanpa server: struktur statis diverifikasi
lewat pembacaan `git diff` penuh (nol baris A/R atau A/P tersentuh), lewat uji otomatis existing
yang membaca berkas ini (`petty-cash-finance-separation.test.mjs` lainnya seperti `CASE 15`
tetap PASS), dan lewat pencocokan manual keempat `resource` permission terhadap konstanta
layar sungguhan (§4).

`AUTOMATED TEST: node --test "tests/unit/*.test.mjs" — PASS (nol regresi baru, dibuktikan diff nama test gagal persis sama dengan baseline)`.

---

## 8. Pemenuhan Acceptance Criteria

| Kriteria (roadmap `02-frontend-roadmap.md` baris `FE-FIN-037`) | Status | Bukti |
| --- | :--: | --- |
| Ketiga butir cutover muncul berurutan mengikuti urutan kerja petugas | **Terpenuhi** | Urutan literal dalam array `subItems`: control-accounts → opening-balances → opening-item-batches |
| Butir ambang muncul paling akhir pada Master Data | **Terpenuhi** | Ditambahkan sebagai elemen terakhir `subItems` milik `corporateFinanceMasterData` |
| Setiap butir membawa `permissionKey` yang benar sehingga tersembunyi bagi yang tidak berhak | **Terpenuhi struktural** (statis; **NOT FEASIBLE** verifikasi sebagai pengguna sungguhan — §7) | §4 — keempat `resource` dicocokkan ke konstanta layar sungguhan |
| Submenu Transaksi A/R dan Transaksi A/P **MUST NOT** berubah sama sekali | **Terpenuhi, dibuktikan** | `git diff` — nol baris di dalam kedua blok itu tersentuh |
| Buku Mutasi Kas dan Riwayat Mutasi Piutang/Utang Supplier **MUST NOT** diberi butir menu | **Terpenuhi, tidak disentuh** | Nol penambahan untuk kedua layar itu — dicapai dari layar induknya seperti sebelum task ini |
| `npm run lint:errors`, `npm run test:unit`, `npm run build` | **PASS, dijalankan sesi ini** | §7 |
| Periksa menu secara manual sebagai pemegang hak dan sebagai yang tidak berhak | **NOT FEASIBLE** — menuntut server dan sesi login | §7 |

**DoD roadmap**: **belum terpenuhi sepenuhnya** — acceptance criteria source-level terpenuhi dan
dibuktikan (termasuk pembuktian non-regresi A/R/A/P lewat diff, bukan hanya klaim);
`lint`/`test:unit`/`build` PASS dijalankan sesi ini; satu-satunya sisa adalah pemeriksaan visual
sungguhan dengan dua peran (berhak/tidak berhak) yang menuntut server berjalan.

---

## 9. Risiko dan Hal yang MUST Disampaikan

1. **Risiko utama yang diperingatkan roadmap** (`menu-items.jsx` 2.129+ baris milik seluruh
   modul) **ditangani dengan sengaja minimal**: dua hunk aditif murni, tidak ada baris yang
   dipindah, digeser, atau diformat ulang di luar yang diperlukan. Dibuktikan lewat `git diff`
   penuh pada §5 — bukan hanya diklaim.
2. **Posisi submenu baru adalah keputusan `DEV_DISCRETION` yang dicatat eksplisit** (§3) — bila
   pemilik menghendaki slot berbeda di antara submenu Finance (misalnya di awal, sebelum Master
   Data), ini perubahan posisi semata tanpa menyentuh isi butir, aman dipindahkan kapan pun.
3. **Nol perubahan pada mesin filter menu** (`filter-menu-items-by-permission.jsx`) — butir baru
   mengikuti mekanisme penyaringan permission yang sudah ada apa adanya, tidak ada logika baru
   yang berisiko salah menyaring butir lain.
4. **`petty-cash-finance-separation.test.mjs` tetap mengandung satu kegagalan pra-ada** yang
   tidak terkait task ini (label "Finance" vs "Keuangan") — **MUST** tidak disalahpahami sebagai
   regresi dari task ini; dibuktikan gagal identik sebelum task ini disentuh.

---

## 10. Status Git (frontend)

```text
 M src/utils/menu-sidebar/menu-items.jsx
```

(Berkas `FE-FIN-033`/`034`/`035`/`036` yang masih tercatat berubah pada `git status` keseluruhan
bukan bagian task ini — lihat laporan masing-masing.) Tidak ada stage, commit, push, pull, merge,
atau rebase.

---

## 11. Langkah Berikutnya

1. Pemilik memeriksa menu secara manual dengan server dev berjalan: sebagai pemegang keempat
   hak (`FinanceSubledgerSetup:Read`, `FinanceOpeningItemBatch:Read`,
   `MstDirectPaymentThreshold:Read`) dan sebagai pengguna yang tidak memegangnya — memastikan
   butir tampil/tersembunyi sesuai hak.
2. Konfirmasi opsional posisi submenu "Cutover & Subledger" di antara submenu Finance lainnya
   (§3) bila pemilik menghendaki slot berbeda dari "setelah Transaksi A/P, sebelum Petty Cash".
3. Sesudah langkah 1 dilaporkan, `FE-FIN-037` dapat ditandai sepenuhnya selesai pada roadmap.
   Ini menutup **seluruh** rentang task frontend `FE-FIN-033..037` pada epik `REV-16`.
