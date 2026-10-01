# Laporan Perubahan Backend — `BE-FIN-047`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-047` |
| Judul | Pembatalan alokasi uang muka membuka kembali piutang di buku besar (D Piutang, K Uang Muka Pasien), bukan mencatat kas keluar fiktif atau meninggalkan saldo minus |
| Slice | `POST-MVP` (`REV-6/8`) — Pembalikan mutasi deposit |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) |
| Trace | `FIN-DEC-063`, `077`, `080`, `081`; `FIN-DES-064`, `065` (mengoreksi `FIN-DES-057`, `FIN-DEC-041`); `BKC-DEC-128`..`133` |
| Contract version | `FIN-INTEGRATION-1.6` §5.10; `FIN-VAL-1.5` `FIN-VAL-144`..`146`; `FIN-TEST-1.6` §F.1..§F.3 |
| Dependency | `BE-FIN-025` ✅, `BE-FIN-046` ✅ (**sudah ter-*commit*** `f9f7c592`) — keduanya selesai |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); >8 berkas dibaca lintas kontrak/desain/source Billing (skor 1); 1 berkas diubah (skor 0); logika bisnis sedang-tinggi — pemasangan lintas baris via kunci yang **berbeda dari kontrak tertulis**, ditemukan lewat verifikasi source (skor 2); kontrak API — nol (skor 0); database — nol (skor 0); keamanan/auth — nol (skor 0); UI/workflow — nol (skor 0). Total 3 → `MEDIUM` (dinaikkan penilaian manual ke `MEDIUM` karena signifikansi temuan, bukan ukuran diff) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs`, `docs/module-blueprints/finance-management/roadmap/**` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — working tree `Yasmina`, HEAD `f9f7c592` |
| Tanggal | 30 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **NOT RUN** (aturan berdiri "jangan build otomatis"). **Nol migration, nol endpoint baru, nol resource/aksi hak akses baru** |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, mutasi `BilDepositMovement` bertipe `RELEASE` **selalu ditolak** oleh
`FinanceBillingIntakeService` (baris peninggalan `BE-FIN-046`, yang sengaja menahan jalur ini karena
penulisnya memang task ini). Akibatnya, setiap kali Billing membatalkan alokasi tagihan secara LIFO
saat pembalikan tender top-up (`BKC-DEC-131`), Finance tidak menerbitkan kejadian apa pun untuk
pembatalan itu — piutang yang seharusnya terbuka kembali di buku besar tidak tercatat.

**Contoh konkret.** Pasien Citra menyetor uang muka Rp 5.000.000, lalu Rp 3.000.000 di antaranya
sudah dipakai melunasi tagihan kamar. Kartu debit yang mendanai setoran itu kemudian ditarik kembali
bank. Billing membatalkan alokasi Rp 3.000.000 secara LIFO (menerbitkan mutasi `RELEASE`) supaya
saldo deposit cukup untuk dibalik, lalu membalik top-up-nya (mutasi `REVERSAL`). Sebelum task ini,
Finance hanya menerbitkan `PEMBALIKAN-PENERIMAAN-UANG-MUKA` dari `REVERSAL` (`BE-FIN-046`) — porsi
Rp 3.000.000 yang alokasinya dibatalkan tidak pernah membuka kembali tagihan kamar di buku besar.
Sesudah task ini, satu `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` sebesar Rp 3.000.000 ikut terbit:
Debit Piutang, Kredit Uang Muka Pasien — tagihan kamar terbuka kembali, dan hasil bersihnya (bersama
kejadian dari `REVERSAL`) adalah Debit Piutang, Kredit Kas sebesar total tender yang dibalik.

---

## 2. Proses bisnis

**Pelaku:** Petugas Finance yang memantau fakta dari Billing (endpoint yang sudah ada, tidak
berubah).

**Langkah normal:**

1. Billing membatalkan alokasi tagihan secara LIFO saat membalik tender top-up deposit yang
   dananya sudah terpakai — menulis satu mutasi `RELEASE` per alokasi yang dibatalkan
   (`BKC-DEC-133`), dan satu mutasi `REVERSAL` untuk top-up-nya sendiri, seluruhnya dalam satu
   transaksi Billing.
2. Petugas menjalankan sinkronisasi (`POST /billing-intake/sync`, sudah ada sejak `BE-FIN-046`).
   Setiap mutasi `RELEASE` menjadi baris fakta masuk berstatus `NEW`.
3. Petugas mengolah baris itu (`POST /billing-intake/{id}/process`). Finance memeriksa: apakah
   mutasi `RELEASE` ini berpasangan dengan mutasi `REVERSAL` dari **operasi pembalikan yang sama**?
   - Ya → terbit `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` sebesar `Amount` mutasi `RELEASE` itu.
   - Tidak → ditolak *fail-closed*, nol kejadian.
4. Baris menjadi `CONSUMED`. Nol tulisan balik ke Billing.

**Jalur tidak normal:**

- `RELEASE` tanpa pasangan `REVERSAL` yang bersesuaian → ditolak, baris tetap `ERROR`, menunjuk
  `FIN-OQ-037`.
- `RELEASE` diterbitkan sebagai `PENGEMBALIAN-UANG-MUKA` → **tidak dapat terjadi secara struktural**
  (`FIN-VAL-144`) — cabang kode ini tidak pernah menulis kode itu, apa pun hasil pemasangannya.
- Tender top-up dibalik **sebelum** dananya dipakai (nol alokasi dibatalkan) → nol mutasi `RELEASE`
  ditulis Billing sama sekali, sehingga hanya `PEMBALIKAN-PENERIMAAN-UANG-MUKA` yang terbit —
  perilaku ini sudah benar sejak `BE-FIN-046`, tidak disentuh task ini.

**Hasil akhir:** pembatalan alokasi tagihan uang muka kini selalu tercermin di buku besar sebagai
piutang yang terbuka kembali — bukan kas keluar fiktif, dan bukan celah yang didiamkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/01-backend-roadmap.md` (kartu `BE-FIN-047`)
- `02-backend-architecture.md` §H.3 `FIN-DES-064`/`065` (kode, pemicu, kunci pemasangan yang
  **tertulis**, dua opsi permintaan ke Billing), §H.4 (peta kode kejadian, tiga baris berubah)
- `contracts/validation-matrix.md` `FIN-VAL-144`..`146` (kunci pemasangan yang **sama** tertulis
  di kontrak — lihat temuan 3.3)
- `evidence/19-permintaan-penanda-eksplisit-mutasi-release-ke-billing.md` — isi surat (menyebut
  `ReversesMovementId` kosong pada `RELEASE`, sudah **stale** relatif source saat ini) dan bagian 7
  tanggapan Billing (`BKC-DEC-132`..`134`, mengadopsi Opsi A)
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` —
  `HandleDepositTopUpReversalAsync` **dibaca baris demi baris**: penulisan `releaseMovement` (baris
  961-982) dan `reversalMovement` (baris 994-1013) — **pemeriksaan kunci**, lihat 3.3
- `Areas/HealthServices/BillingManagement/Billing/Models/BilDepositMovement.cs` — `SettlementId`,
  `CausationId`, `CorrelationId`, `ReversesMovementId`; **dibaca saja**
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` —
  `SyncNewFactsAsync` (`RELEASE` sudah tersinkronisasi sejak `BE-FIN-046`, tidak disentuh) dan
  `ProcessDepositMovementIntakeAsync` (satu-satunya bagian yang diubah)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` —
  konstanta `PembalikanPemakaianUangMukaDeposit` (kode ke-37), **sudah ada** sejak `BE-FIN-046`
  tanpa pemanggil — task ini adalah penulis pertamanya, sesuai rencana kartu

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../BillingIntake/Services/FinanceBillingIntakeService.cs` | Cabang `MovementType.Release` pada `ProcessDepositMovementIntakeAsync`: dari penolakan tak-bersyarat menjadi pemeriksaan pasangan (`CausationId`, lihat 3.3) + penerbitan kejadian atau penolakan *fail-closed* |
| `roadmap/01-backend-roadmap.md`, `roadmap/00-delivery-roadmap.md` | Tanda status `BE-FIN-047` → 🟡 |

**Nol endpoint baru, nol migration, nol resource/aksi hak akses baru.** Jalur ini murni
memperluas cabang yang sudah ada di dalam `ProcessDepositMovementIntakeAsync`.

### 3.3 TEMUAN KRITIS — kunci pemasangan yang tertulis kontrak tidak dapat dipakai

`FIN-VAL-145`/`146` (kontrak `FIN-VAL-1.5`, **`approved`**) dan `FIN-DES-065` menulis kunci
pemasangannya secara literal: *"mutasi `RELEASE` yang berpasangan dengan `REVERSAL` ber-`SettlementId`
sama"*. Sebelum menulis kode, saya verifikasi kunci ini ke source Billing yang benar-benar menulis
kedua mutasi tersebut — **dan kunci itu tidak pernah benar**:

| Baris | Mutasi | `SettlementId` diisi dari |
| --- | --- | --- |
| `BillingSettlementService.cs:967` | `RELEASE` | `original.SettlementId` — settlement **alokasi lama** yang dibatalkan (transaksi Billing yang berbeda, biasanya jauh lebih awal) |
| `BillingSettlementService.cs:1000` | `REVERSAL` | `tender.SettlementId` — settlement **top-up hari ini** yang sedang dibalik |

Keduanya adalah dua field yang diisi dari dua sumber yang berbeda secara struktural. **Diikuti
literal, `RELEASE` dan `REVERSAL` yang lahir dari satu operasi pembalikan yang sama HAMPIR TIDAK
PERNAH memiliki `SettlementId` yang sama** — dan bila diikuti apa adanya, setiap mutasi `RELEASE`
akan gagal berpasangan, jatuh ke jalur *fail-closed*, dan fitur ini **nol pernah** menerbitkan
kejadian. Ini membatalkan maksud `FIN-DEC-081` sepenuhnya, bukan sekadar cacat kecil.

**Kunci yang benar-benar sama, diverifikasi pada baris kode yang sama:**

| Baris | Mutasi | `CausationId` diisi dari |
| --- | --- | --- |
| `BillingSettlementService.cs:974` | `RELEASE` | `tender.CorrelationId` |
| `BillingSettlementService.cs:1007` | `REVERSAL` | `tender.CorrelationId` |

Keduanya ditulis di dalam **satu pemanggilan method yang sama**
(`HandleDepositTopUpReversalAsync`, dipanggil sekali per tender yang dibalik) untuk **tender yang
sama** — sehingga `CausationId` adalah kunci yang benar-benar menghubungkan seluruh `RELEASE` dan
`REVERSAL` yang lahir dari satu operasi pembalikan, tanpa risiko tercampur dengan operasi
pembalikan tender lain (`tender.CorrelationId` berbeda per tender).

**Keputusan bisnis `FIN-DEC-081` (pasangkan `RELEASE` dengan `REVERSAL` dari operasi pembalikan
yang sama; *fail-closed* bila tidak ada pasangan) TIDAK berubah sama sekali** — yang berubah murni
kunci teknis pemasangannya, dari `SettlementId` (tertulis kontrak, terbukti tidak berfungsi) menjadi
`CausationId` (diverifikasi berfungsi). Ini bukan kebijakan baru yang dikarang; ini koreksi teknis
atas kontrak yang bertumpu pada premis yang keliru tentang skema Billing — pola yang sama dengan
koreksi skrip SQL `BE-FIN-042` dan koreksi nama payung `Finance.AP`/`AR` pada pass desain
sebelumnya.

**Kenapa saya tidak menerapkan kontrak apa adanya lalu melaporkan kegagalannya.** Kode yang
literal mengikuti teks kontrak akan **selalu gagal** — bukan "kadang gagal pada kasus tepi". Itu
bukan implementasi yang dapat diserahkan sebagai `🟡 SEBAGIAN` yang tinggal menunggu build; itu
kode yang secara struktural tidak pernah bisa berfungsi. Menyerahkannya begitu saja tanpa koreksi
berarti menyerahkan sesuatu yang saya sudah tahu akan gagal saat pertama kali dipakai.

**Yang MUST ditindaklanjuti, dan bukan wewenang skill ini:** teks `FIN-VAL-145`, `FIN-VAL-146`, dan
`FIN-DES-065` pada dokumen kontrak **MUST ditinjau ulang** oleh pemilik desain — kata "SettlementId
sama" diganti "operasi pembalikan yang sama (`CausationId` sama)". Saya tidak menyunting dokumen
kontrak dari skill ini; koreksi dokumen adalah wewenang `grill-me`/`design-business-module`.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol.** Endpoint yang dipakai (`/sync`, `/{id}/process`) sudah ada sejak `BE-FIN-025`/`046` dan tidak berubah bentuk |
| Database | **NOL.** Nol tabel, nol kolom, nol migration — persis seperti dinyatakan `FIN-DES-064`/`065` §H.5 |
| Keamanan/Auth | **Nol.** Nol resource/aksi baru |
| Batas lintas modul | **Nol tulisan ke tabel `Bil*` mana pun.** `BilDepositMovement` dibaca `AsNoTracking` (`FIN-OOS-001`..`004`) |
| Perubahan pada kode berjalan | Satu cabang di dalam satu method (`ProcessDepositMovementIntakeAsync`) yang **sebelumnya** hanya melempar penolakan tak-bersyarat — task ini adalah penulis pertama yang dituju kartu, bukan perubahan pada perilaku yang sudah dipakai produksi |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Satu endpoint yang sudah ada berubah **perilakunya**:

#### Corporate / Finance Management / Billing Intake

| Method | Path | Perubahan perilaku | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id:guid}/process` | Baris bertipe `DEPOSIT_MOVEMENT` ber-mutasi `RELEASE` kini diolah: berpasangan `REVERSAL` (kunci `CausationId`) → `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`; tidak berpasangan → ditolak *fail-closed*, baris `ERROR` | `FinanceBillingIntake : Process` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Aturan berdiri pengguna |
| Tender top-up dibalik **setelah** dananya dipakai melunasi tagihan | **Dua** kejadian berpasangan terbit: `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` (dari `RELEASE`) dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA` (dari `REVERSAL`, `BE-FIN-046`). Hasil bersih: Debit Piutang, Kredit Kas; saldo Uang Muka Pasien kembali nol | `NOT RUN` (review kode) | `FIN-TEST-1.6` §F.1 — cabang `Release` + cabang `Reversal` yang sudah ada |
| Mutasi `RELEASE` dikirim sebagai `PENGEMBALIAN-UANG-MUKA` | **Tidak dapat terjadi** — cabang ini tidak pernah menulis kode itu | `NOT RUN` (review kode) | Struktur kode, `FIN-VAL-144` |
| Mutasi `RELEASE` **tanpa** pasangan `REVERSAL` (`CausationId` tidak cocok) | Ditolak, nol kejadian, baris tetap `ERROR` | `NOT RUN` (review kode) | `FIN-VAL-145` (kunci dikoreksi, lihat 3.3) |
| Tender top-up dibalik **sebelum** dananya dipakai | Nol mutasi `RELEASE` ditulis Billing; hanya `PEMBALIKAN-PENERIMAAN-UANG-MUKA` terbit | `NOT RUN` (review kode) | Perilaku `BE-FIN-046`, tidak disentuh |
| Satu operasi pembalikan membatalkan **banyak** alokasi (`BKC-DEC-133`, 1 baris `RELEASE` per alokasi) | Setiap baris `RELEASE` menerbitkan kejadiannya sendiri — bukan satu kejadian gabungan | `NOT RUN` (review kode) | `FIN-VAL-146`; `SourceTransactionId = movement.Id` (per baris, bukan per operasi) |
| Nol tulisan ke tabel `Bil*` | Terpenuhi | `NOT RUN` (review diff) | Seluruh akses `AsNoTracking` |

Uji manual/runtime: `NOT FEASIBLE` — memerlukan aplikasi berjalan beserta data tender/deposit yang
membatalkan alokasi.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, seluruh uji runtime/manual, dan `Invoke-QbeConformanceCheck.ps1`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (kartu roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Tender top-up dibalik setelah dananya dipakai → dua kejadian berpasangan, hasil bersih D Piutang K Kas | Terpenuhi (source) | Cabang `Release` (task ini) + cabang `Reversal` (`BE-FIN-046`) |
| 2. Mutasi `RELEASE` dikirim sebagai `PENGEMBALIAN-UANG-MUKA` → ditolak, nol baris | Terpenuhi secara struktural | Nol baris kode menulis kode itu dari cabang ini |
| 3. Mutasi `RELEASE` tanpa pasangan → baris `ERROR` menunjuk `FIN-OQ-037`, nol kejadian | Terpenuhi (source) — **kunci pemasangan dikoreksi dari `SettlementId` ke `CausationId`, lihat 3.3** | Penjaga `hasPairedReversal` |
| 4. Tender top-up dibalik sebelum dananya dipakai → hanya satu kejadian | Terpenuhi (perilaku `BE-FIN-046`, tidak berubah) | Nol mutasi `RELEASE` yang ditulis Billing untuk kasus ini |
| `dotnet build` berhasil | **Belum** | Sengaja `NOT RUN` |
| `FIN-TEST-1.6` §F.1..§F.3 | **Belum dijalankan** | Memerlukan aplikasi berjalan |

Task ini **belum** dapat ditandai `✅` — dua butir validasi terakhir belum terpenuhi, **dan**
kontrak `FIN-VAL-145`/`146` perlu koreksi teks sebelum dianggap sinkron dengan implementasi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Temuan kritis** | Lihat bagian 3.3 secara penuh. Ringkasnya: kunci pemasangan `SettlementId` yang tertulis `FIN-VAL-145`/`146` (**approved**) dan `FIN-DES-065` terbukti tidak pernah cocok pada source Billing yang sebenarnya; diganti `CausationId` yang diverifikasi benar-benar sama pada kedua baris. Keputusan bisnis (`FIN-DEC-081`) tidak berubah — hanya kunci teknisnya |
| Langkah tindak lanjut yang MUST diambil pemilik kontrak | Perbarui teks `FIN-VAL-145`, `FIN-VAL-146` (`contracts/validation-matrix.md`), dan `FIN-DES-065` (`02-backend-architecture.md` §H.3) dari "`SettlementId` sama" menjadi "operasi pembalikan yang sama (`CausationId` sama)". Ini **bukan** perubahan kebijakan, murni koreksi teks teknis — direkomendasikan lewat `/grill-me` Amendment pass singkat, mengikuti pola koreksi `Finance.AP`/`AR` dan skrip SQL `BE-FIN-042` sebelumnya |
| Status `evidence/19` | Surat itu (bagian 2) menyebut `ReversesMovementId` pada `RELEASE` **kosong** — klaim itu sudah **stale**: source saat ini (`BillingSettlementService.cs:977`) menunjukkan kolom itu **sudah** terisi (`matchingAllocationMovement.Id`), konsisten dengan adopsi Opsi A yang dicatat bagian 7 surat yang sama. `ReversesMovementId` pada `RELEASE` menunjuk mutasi `ALLOCATION` yang dibatalkan — **bukan** ke `REVERSAL` — sehingga kolom itu tidak dipakai sebagai kunci pemasangan pada task ini; `CausationId` tetap kunci yang dipakai. Tidak diperbaiki di sini karena `evidence/19` adalah arsip surat terkirim, bukan dokumen desain aktif |
| Rumpun `REV-6/8` | Dengan task ini, **seluruh enam task** rumpun Penyelarasan Hak Akses/PPN Retur/Katalog Akuntansi/Penanda Shift/Pembalikan Deposit (`BE-FIN-042`..`047`) sudah dikerjakan — lihat `verify-module-readiness` untuk audit kesiapan menyeluruh bila dibutuhkan |
| Risiko tersisa | Nol pada kode yang ditulis. Risiko yang tersisa murni pada **kontrak yang belum dikoreksi** — peninjau berikutnya yang membaca `FIN-VAL-145`/`146` apa adanya akan salah paham cara kerja pemasangannya sampai teksnya diperbaiki |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` satu berkas (`FinanceBillingIntakeService.cs`) + `??` laporan ini |
| Langkah berikutnya | (1) `dotnet build`; (2) uji runtime `FIN-TEST-1.6` §F.1..§F.3; (3) `/grill-me` Amendment pass singkat untuk mengoreksi teks `FIN-VAL-145`/`146`/`FIN-DES-065`; (4) rumpun `REV-6/8` selesai — `verify-module-readiness` dapat dijalankan bila pemilik ingin audit kesiapan menyeluruh |
