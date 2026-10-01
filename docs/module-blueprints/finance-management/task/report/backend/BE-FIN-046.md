# Laporan Perubahan Backend — `BE-FIN-046`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-046` |
| Judul | Uang muka pasien yang ternyata tidak jadi diterima tidak tertinggal sebagai saldo palsu |
| Slice | `POST-MVP` (`REV-6/8`) — Pembalikan mutasi deposit |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) |
| Trace | `FIN-DEC-040`, `044`, **`077`**; `FIN-DES-035`, `FIN-DES-057` (dikoreksi `FIN-DES-064`); `BKC-DEC-128`..`131` |
| Contract version | `FIN-INTEGRATION-1.6` §5.10; `FIN-VAL-1.5` `FIN-VAL-142`; `FIN-TEST-1.6` §D.6 |
| Dependency | `BE-FIN-025` ✅ — selesai, gerbang terbuka |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); >8 berkas dibaca lintas kontrak/desain/source Billing (skor 1); 2 berkas diubah (skor 0); logika bisnis sedang — tiga cabang jenis mutasi + pendeteksi lintas tabel (skor 2); kontrak API — **nol** endpoint baru (skor 0); database — **nol** (skor 0); keamanan/auth — nol (skor 0); UI/workflow — nol (skor 0). Total 3 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/{BillingIntake,AccountingIntegration}/**`, `docs/module-blueprints/finance-management/roadmap/**` (tanda status) |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Belum di-commit — working tree `Yasmina`, HEAD `7811c048` |
| Tanggal | 29 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **NOT RUN** (aturan berdiri "jangan build otomatis"). **Nol migration**, **nol endpoint baru**, **nol tulisan ke tabel `Bil*`** |

---

## 1. Masalah yang diperbaiki

Ketika tender yang mendanai top-up deposit pasien dibalik — misalnya kartu ditarik kembali oleh
penerbitnya — uang itu sebenarnya **tidak pernah jadi diterima** rumah sakit. Sebelum task ini,
Finance tidak mengonsumsi mutasi pembalik deposit sama sekali: jalur sinkronisasi hanya mengambil
mutasi `ALLOCATION` dan `RELEASE`. Akibatnya kejadian `PEMBALIKAN-PENERIMAAN-UANG-MUKA` tidak pernah
terbit untuk kasus ini, dan buku besar terus mencatat uang muka yang tidak ada uangnya.

**Contoh konkret.** Pasien Budi menyetor uang muka Rp 3.000.000 dengan kartu debit. Dua hari
kemudian bank menarik kembali transaksinya. Billing membalik tender itu dan menulis mutasi pembalik
atas top-up-nya (`BKC-DEC-131`). Sebelum task ini, Finance tidak melihat fakta itu — saldo uang muka
Budi di buku besar tetap Rp 3.000.000 padahal uangnya sudah tidak ada. Sesudah task ini, satu
kejadian `PEMBALIKAN-PENERIMAAN-UANG-MUKA` sebesar Rp 3.000.000 terbit.

Selain itu dibangun **jaring pengaman** (`FIN-VAL-142`): bila suatu saat Billing membalik tender
top-up **tanpa** menulis mutasi pembaliknya, lubang itu dibuat terlihat sebagai baris berstatus
`ERROR` di layar pantauan — bukan didiamkan sebagai selisih yang baru ketahuan saat rekonsiliasi.

---

## 2. Proses bisnis

**Pelaku:** Petugas Finance yang memantau fakta dari Billing.

**Langkah normal:**

1. Billing membalik tender top-up deposit; dalam transaksi yang sama ia menulis mutasi deposit
   pembalik yang menunjuk top-up asalnya (`ReversesMovementId`, `BKC-DEC-132`).
2. Petugas menjalankan sinkronisasi (`POST /billing-intake/sync`). Mutasi `REVERSAL` kini ikut
   terambil dan menjadi baris fakta masuk berstatus `NEW`.
3. Petugas mengolah baris itu (`POST /billing-intake/{id}/process`). Finance memeriksa mutasi apa
   yang dibalikkannya:
   - Membalik `TOP_UP` → terbit `PEMBALIKAN-PENERIMAAN-UANG-MUKA` sebesar nilai mutasi.
   - Membalik yang lain → **ditolak**, nol kejadian (lihat jalur tidak normal).
4. Baris fakta menjadi `CONSUMED`. Nol tulisan balik ke Billing.

**Jalur tidak normal — seluruhnya *fail-closed*, nol kejadian diterbitkan:**

- Mutasi `REVERSAL` tanpa `ReversesMovementId` → ditolak. Tanpa penanda eksplisit, arah jurnalnya
  tidak dapat ditentukan, dan Finance **tidak menebak**.
- Mutasi `REVERSAL` atas `ALLOCATION` → ditolak dengan pesan yang menyebut `BE-FIN-047` sebagai
  penulisnya. Barisnya tetap terlihat dan dapat diolah ulang begitu task itu selesai.
- Mutasi `TOP_UP` → ditolak. Ia bukan fakta yang diolah jalur ini.
- Tender top-up dibalik **tanpa** mutasi pembalik → baris `ERROR` menunjuk `FIN-OQ-034`, nol
  kejadian (`FIN-VAL-142`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/01-backend-roadmap.md` (kartu `BE-FIN-046`)
- `02-backend-architecture.md` §E.4 `FIN-DES-057` (kode, pemicu sasaran, pendeteksi), §E.1 temuan C,
  §H `FIN-DES-064` (koreksi pemicu `RELEASE`)
- `contracts/validation-matrix.md` `FIN-VAL-142`; `contracts/integration-contract.md` §5.10
- `Areas/HealthServices/BillingManagement/Billing/Models/BilDepositMovement.cs` — empat `MovementType`,
  kolom `ReversesMovementId`, `SettlementId`; **dibaca saja**
- `Areas/HealthServices/BillingManagement/Billing/Models/BilSettlement.cs` — `Purpose`,
  `BillingSettlementPurposes.DepositTopUp`; **dibaca saja**
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` baris 567-582
  dan `BilConsumerHandoffService.PublishForTenderAsync` — **pemeriksaan kunci**, lihat 3.3
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` —
  `SyncNewFactsAsync` dan `ProcessDepositMovementIntakeAsync` (`BE-FIN-025`)
- `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` —
  `CreateReversalReceiptAsync` (`BE-FIN-024`), untuk memastikan tidak ada kejadian ganda

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Konstanta kode ke-37 `PembalikanPemakaianUangMukaDeposit` — **sengaja tanpa penulis**, sesuai kartu task; penulisnya `BE-FIN-047` |
| `.../BillingIntake/Services/FinanceBillingIntakeService.cs` | (a) Sinkronisasi mengambil mutasi `REVERSAL`; (b) pendeteksi `FIN-VAL-142` menulis baris `ERROR` untuk top-up yatim; (c) `ProcessDepositMovementIntakeAsync` menangani `REVERSAL` beserta tiga penjaga *fail-closed* |
| `roadmap/01-backend-roadmap.md`, `roadmap/00-delivery-roadmap.md` | Tanda status `BE-FIN-046` → 🟡 |

**Nol endpoint baru.** Jalur ini memakai `POST /billing-intake/sync` dan
`POST /billing-intake/{id}/process` yang sudah ada.

### 3.3 Pemeriksaan yang menentukan: apakah ada risiko kejadian ganda?

`PEMBALIKAN-PENERIMAAN-UANG-MUKA` sudah punya satu penulis lain — `FinanceReceiptService.CreateReversalReceiptAsync`
(`BE-FIN-024`), yang menerbitkannya saat tender dibalik dan penerimaan aslinya berstatus tagihan
`OPEN`. Bila tender top-up deposit **juga** melewati jalur itu, satu peristiwa ekonomi akan terbit
dua kali dengan `SourceTransactionId` berbeda — dan unique index kotak keluar **tidak** akan
menangkapnya.

Ini **diverifikasi ke source, bukan diasumsikan**:

| Bukti | Isi |
| --- | --- |
| `BillingSettlementService` baris 570 | `PublishForTenderAsync` dipanggil untuk setiap tender yang mencapai `SUCCEEDED` atau `REVERSED` — **tanpa** penyaring `Purpose` |
| `BilConsumerHandoffService.PublishForTenderAsync` baris 61-65 | Mengembalikan `null` bila `!settlement.InvoiceId.HasValue`, dengan komentar eksplisit: "Settlement tanpa InvoiceId (misalnya DepositTopUp) tidak memiliki invoice penagihan" |
| `BilSettlement` top-up deposit | Memakai `DepositAccountId`, `InvoiceId` kosong |

**Kesimpulan: tender top-up deposit TIDAK PERNAH menghasilkan `BilCollectionHandoff`**, sehingga
jalur `FinReceipt` tidak pernah menyentuh kasus ini. Nol risiko kejadian ganda — dan pemisahannya
struktural, bukan kebetulan.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol endpoint baru dan nol endpoint berubah bentuk.** Perilaku dua endpoint yang sudah ada diperluas: `/sync` kini juga mengambil mutasi `REVERSAL` dan menulis baris `ERROR` pendeteksi; `/{id}/process` kini mengenali `REVERSAL` |
| Database | **NOL.** Nol tabel, nol kolom, nol migration. Jenis handoff yang dipakai (`DEPOSIT_MOVEMENT`) **sudah ada** — nol perubahan check constraint |
| Keamanan/Auth | **Nol.** Nol resource/aksi baru; atribut controller tidak disentuh |
| Batas lintas modul | **Nol tulisan ke tabel `Bil*` mana pun.** `BilTender`, `BilSettlement`, dan `BilDepositMovement` seluruhnya dibaca `AsNoTracking` (`FIN-OOS-001`..`004`) |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Dua endpoint yang sudah ada berubah **perilakunya**:

#### Corporate / Finance Management / Billing Intake

| Method | Path | Perubahan perilaku | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/sync` | Mutasi deposit `REVERSAL` ikut diambil; pendeteksi `FIN-VAL-142` menulis baris `ERROR` untuk top-up yang tender-nya sudah dibalik tetapi mutasi pembaliknya tidak ada | `FinanceBillingIntake : Sync` |
| `POST` | `/{id:guid}/process` | Baris bertipe `DEPOSIT_MOVEMENT` ber-mutasi `REVERSAL` kini diolah: membalik `TOP_UP` → `PEMBALIKAN-PENERIMAAN-UANG-MUKA`; selain itu ditolak *fail-closed* | `FinanceBillingIntake : Process` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Aturan berdiri pengguna |
| `REVERSAL` atas `TOP_UP` | Satu `PEMBALIKAN-PENERIMAAN-UANG-MUKA` sebesar nilai mutasi | `NOT RUN` (review kode) | Cabang `Reversal` |
| `REVERSAL` tanpa `ReversesMovementId` | Ditolak, nol kejadian, baris menjadi `ERROR` | `NOT RUN` (review kode) | Penjaga pertama |
| `REVERSAL` atas `ALLOCATION` | Ditolak dengan pesan menyebut `BE-FIN-047`, nol kejadian | `NOT RUN` (review kode) | Penjaga kedua |
| Baris bertipe `TOP_UP` diolah | Ditolak, tetap `ERROR` | `NOT RUN` (review kode) | Penjaga ketiga |
| Tender top-up dibalik **tanpa** mutasi pembalik | Baris intake `ERROR` menunjuk `FIN-OQ-034`, nol kejadian | `NOT RUN` (review kode) | Pendeteksi `FIN-VAL-142` |
| Tender top-up dibalik **dengan** mutasi pembalik (keadaan hari ini, `BKC-DEC-131`) | Nol baris `ERROR` — pendeteksi diam | `NOT RUN` (review kode) | `reversedMovementIds.Contains` |
| Sinkronisasi dijalankan dua kali | Nol baris ganda | `NOT RUN` (review kode) | Dedup `IdempotencyKey` yang sudah ada |
| Nol kejadian ganda dengan jalur penerimaan | Terpenuhi — terbukti struktural | `NOT RUN` (review source Billing) | Lihat bagian 3.3 |
| Nol tulisan ke tabel `Bil*` | Terpenuhi | `NOT RUN` (review diff) | Seluruh akses `AsNoTracking`, nol `Add`/`Update` |

Uji manual/runtime: `NOT FEASIBLE` — memerlukan aplikasi berjalan beserta data tender/deposit.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, seluruh uji runtime/manual, dan `Invoke-QbeConformanceCheck.ps1`.
Perbandingan `RowVersion`/`UpdateDateTime` tabel `Bil*` sebelum-sesudah (diminta kartu) **belum**
dijalankan karena menuntut runtime; sebagai gantinya dibuktikan lewat review diff bahwa nol operasi
tulis menyentuh entity `Bil*`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (kartu roadmap) | Status | Bukti |
| --- | --- | --- |
| Pembalikan top-up yang dananya belum terpakai → `PEMBALIKAN-PENERIMAAN-UANG-MUKA` terbit | Terpenuhi (source) | Cabang `Reversal` atas `TOP_UP` |
| Tender top-up dibalik tanpa mutasi pembalik → baris `ERROR` menunjuk `FIN-OQ-034`, nol kejadian, nol tulisan ke `Bil*` | Terpenuhi (source) | Pendeteksi `FIN-VAL-142` |
| Pemeriksaan hanya **membaca** `BilTender` dan `BilDepositMovement` | Terpenuhi | Seluruh query `AsNoTracking` |
| Konstanta `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` ditambahkan **tanpa penulis** | Terpenuhi | Konstanta ada; nol pemanggil — penulisnya `BE-FIN-047` |
| `dotnet build` berhasil | **Belum** | Sengaja `NOT RUN` |
| `FIN-TEST-1.6` §D.6 (dua baris terakhir) | **Belum dijalankan** | Memerlukan aplikasi berjalan |

Task ini **belum** dapat ditandai `✅` — dua butir validasi terakhir belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Sifat pendeteksi `FIN-VAL-142` | Sesudah `FIN-DEC-077` dan `BKC-DEC-128`..`131`, ia **jaring pengaman**, bukan jalur utama — Billing sudah menulis mutasi pembaliknya sejak `BE-BKC-079`. Pada data hari ini pendeteksi ini **seharusnya tidak pernah berbunyi**. Ia tetap dibangun karena ia yang membuat lubang terlihat bila kelak jalur Billing berubah lagi. Bila ia berbunyi, itu sinyal regresi di sisi Billing — bukan cacat Finance |
| Keputusan bentuk yang perlu ditinjau | Kontrak menuntut "baris intake `ERROR`", sementara tidak ada jenis handoff untuk tender. Barisnya karena itu ditulis bertipe `DEPOSIT_MOVEMENT` menunjuk **mutasi `TOP_UP`** yang seharusnya sudah punya pembalik — jenis handoff yang sudah ada, sehingga **nol migration**. Konsekuensinya baris bertipe `TOP_UP` kini bisa ada di tabel fakta masuk; karena itu ditambahkan penjaga ketiga yang menolaknya bila diolah, supaya ia tidak pernah diam-diam menjadi `CONSUMED` tanpa kejadian |
| Penjaga fail-closed yang disengaja | Tiga cabang penolakan (`ReversesMovementId` kosong, `REVERSAL` atas non-`TOP_UP`, dan `TOP_UP`) seluruhnya membiarkan barisnya `ERROR` dan **dapat diolah ulang**. Ini disengaja: `REVERSAL` atas `ALLOCATION` akan menjadi sah begitu `BE-FIN-047` selesai, dan barisnya sudah menunggu di sana — bukan hilang karena terlanjur ditandai selesai |
| Batas yang dihormati | Konstanta kode ke-37 ditambahkan **tanpa satu pun pemanggil**. Menulisnya dari jalur mana pun sebelum `BE-FIN-047` akan mendahului pemicu yang baru disahkan `FIN-DEC-080` (mutasi `RELEASE` berpasangan `REVERSAL`), bukan pemicu lama `FIN-DES-057` yang sudah terbukti tidak pernah ada |
| Risiko tersisa | `SyncNewFactsAsync` kini memuat seluruh `BilDepositMovement` yang belum dihapus ke memori untuk keperluan pendeteksi, bukan hanya tiga jenis yang disinkron. Pada data besar ini menaikkan beban baca sinkronisasi. Dicatat sebagai keterbatasan; jalur ini dipicu manual dan belum berjadwal, sehingga belum menjadi masalah nyata — bila kelak dijadwalkan, penyaringan berbasis periode layak ditambahkan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` dua berkas (konstanta katalog, service intake) + `??` laporan ini |
| Langkah berikutnya | (1) `dotnet build`; (2) uji runtime `FIN-TEST-1.6` §D.6; (3) `BE-FIN-047` — satu-satunya task tersisa pada rumpun ini, dan konstanta yang dibutuhkannya sudah tersedia dari task ini |
