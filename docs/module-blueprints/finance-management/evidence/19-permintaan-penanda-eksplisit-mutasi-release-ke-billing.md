# Permintaan untuk Billing — Penanda Eksplisit pada Mutasi Deposit `RELEASE`

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Owner modul Billing / Kasir (`billing-kasir`, `BIL-CASH-001`) |
| Tanggal | 29 September 2026 |
| Sifat | **Kesepakatan tercapai (APPROVED).** Owner Billing resmi menyetujui Opsi A via `BKC-DEC-132`..`134` pada 29 September 2026 |
| Dasar temuan | Pemeriksaan source read-only pada commit `7811c048` (`BillingSettlementService.cs:949` dan `BillingDepositService.cs:176`) |
| Dasar keputusan | `FIN-DEC-081` (Finance) dan `BKC-DEC-132`..`134` (Billing Kasir) |
| Dicatat sebagai | `FIN-OQ-037` pada decision log Finance (**CLOSED** 29 September 2026) |

Berkas ini berdiri sendiri. Anda tidak perlu membuka dokumen blueprint Finance untuk membacanya.

---

## 1. Ringkasan satu paragraf

Terima kasih atas penyelesaian cepat `evidence/17` melalui keputusan `BKC-DEC-128`..`131`. Mekanisme pembatalan alokasi tagihan LIFO saat pembalikan tender top-up kini berjalan dengan tepat di source `7811c048`. Di sisi Finance, kami telah mengesahkan pemetaannya (`FIN-DEC-080` / `FIN-DES-064`): mutasi `RELEASE` yang Anda tulis kami petakan sebagai kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` (Debit Piutang, Kredit Uang Muka Pasien — tagihan pasien terbuka kembali dan saldo deposit pulih tanpa ada kas yang keluar). Namun, pada pemeriksaan source kami menemukan bahwa nama `RELEASE` masih berpotensi menimbulkan ambiguitas semantik di masa mendatang, dan kolom `ReversesMovementId` pada mutasi `RELEASE` saat ini belum diisi. Bagian 4 mengajukan dua opsi solusi yang sangat ringan, dengan rekomendasi Opsi A yang **nol perubahan skema database**.

---

## 2. Apa yang kami temukan di source `7811c048`

Kami menelusuri penulisan dan pembacaan `BilDepositMovement` untuk memastikan intake Finance tidak salah membaca data Anda:

| Berkas dan baris | Kode yang berjalan | Kondisi saat ini |
|---|---|---|
| `BillingSettlementService.cs:949` | Penulisan mutasi `RELEASE` saat alokasi tagihan dibatalkan secara LIFO | Kolom `ReversesMovementId` dibiarkan **kosong (`null`)**, dan hanya `SettlementId` yang terisi |
| `BillingDepositService.cs:176` | Perhitungan total uang yang dikembalikan | `totalRefunded` menjumlahkan seluruh mutasi `RELEASE` (`MovementType == BilDepositMovement.MovementType.Release`) |

Dua hal ini menunjukkan bahwa di sisi Billing sendiri, arti mutasi `RELEASE` masih berada di persimpangan antara **"pembatalan alokasi pemakaian deposit"** dan **"pengembalian uang kas kepada pasien (refund)"**.

---

## 3. Mengapa penanda eksplisit penting bagi kedua modul

1. **Bagi Modul Billing:**
   Karena `BillingDepositService.cs:176` menjumlahkan seluruh mutasi `RELEASE` ke dalam `totalRefunded`, pembatalan alokasi tagihan kini ikut terhitung sebagai "dana yang dikembalikan ke pasien". Padahal uangnya masih utuh berada di akun deposit pasien.
2. **Bagi Modul Finance:**
   Lawan jurnal akuntansi ditentukan oleh **pergerakan uang riil**, bukan nama label:
   - Pembatalan alokasi: Debit Piutang, Kredit Uang Muka Pasien (nol kas bergerak).
   - Pengembalian kas ke pasien (refund): Debit Uang Muka Pasien, Kredit Kas (uang fisik/bank keluar).
   Bila kelak tim Billing menambahkan jalur refund deposit tunai dan sama-sama memakai `RELEASE` tanpa penanda, Finance berisiko salah menjurnal kas keluar untuk pembatalan alokasi, atau salah membuka piutang untuk pengembalian kas nyata.

---

## 4. Dua pilihan yang kami ajukan

Keduanya menyelesaikan persoalan ini secara tuntas. Pilihan ada sepenuhnya pada kewenangan tim Billing:

### Opsi A (Direkomendasikan) — Isi kolom `ReversesMovementId` pada mutasi `RELEASE`

Saat `BillingSettlementService.cs:949` menulis mutasi `RELEASE`, isi kolom `ReversesMovementId` dengan ID mutasi `ALLOCATION` yang dibatalkan.

| Aspek | Penjelasan |
|---|---|
| Perubahan skema database | **Nol.** Kolom `ReversesMovementId` **sudah ada** di tabel `BilDepositMovement` |
| Semantik bisnis | Sempurna. Sama persis dengan mutasi `REVERSAL` yang mengisi `ReversesMovementId` menunjuk `TOP_UP` asalnya |
| Dampak bagi Finance | Finance dapat langsung memvalidasi `ReversesMovementId` tanpa bergantung semata-mata pada korelasi `SettlementId` |
| Dampak bagi Billing | Perubahan sangat kecil hanya pada satu baris inisialisasi mutasi di `BillingSettlementService.cs` |

### Opsi B — Tambahkan nilai baru pada enum `MovementType`

Membuat nilai enum baru, misalnya `ALLOCATION_CANCELLED` atau `ALLOCATION_REVERSAL`.

| Aspek | Penjelasan |
|---|---|
| Semantik bisnis | Paling bersih dan eksplisit secara konsep |
| Dampak bagi Billing | Menyentuh definisi enum di model, check constraint di database, dan memerlukan migrasi skema serta penyesuaian perhitungan `totalRefunded` di `BillingDepositService` |
| Dampak bagi Finance | Finance cukup menambahkan case enum baru pada intake service |

---

## 5. Yang kami lakukan di sisi Finance selama menunggu

Kami **tidak menahan pembangunan Finance**. Sesuai keputusan arsitektur `FIN-DES-065` dan keputusan bisnis `FIN-DEC-081`, Finance menerapkan aturan intake yang aman dan *fail-closed*:

1. **Jalur Normal (Berpasangan):** Bila mutasi `RELEASE` memiliki mutasi `REVERSAL` dengan `SettlementId` yang sama, Finance memprosesnya dengan yakin sebagai pembatalan alokasi LIFO dan menerbitkan kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`.
2. **Jalur Mitigasi (*Fail-Closed*):** Bila di kemudian hari muncul mutasi `RELEASE` yang **tidak berpasangan** dengan `REVERSAL` pada `SettlementId` yang sama, Finance **tidak akan menebak** lawan jurnalnya. Baris intake tersebut ditandai berstatus **`ERROR`** (nol kejadian akuntansi diterbitkan) dan diarahkan untuk verifikasi manual melalui rujukan `FIN-OQ-037`.

Dengan mekanisme ini, sistem terlindungi dari mutasi liar tanpa menghambat alur kerja yang sudah disepakati saat ini.

---

## 6. Yang kami butuhkan

Kami memohon konfirmasi dari Owner Billing apakah **Opsi A** (mengisi `ReversesMovementId`) disetujui untuk diterapkan pada task Billing berikutnya, ataukah tim Billing lebih memilih **Opsi B**.

Konfirmasi ini tidak menahan task Finance saat ini (`BE-FIN-025`/`BE-FIN-047`), melainkan akan menjadi penyempurnaan permanen integritas data antar-modul kita.

---

## 7. Tanggapan Resmi Owner Billing (Disahkan 29 September 2026)

Melalui sesi `/grill-me` 29 September 2026, Owner Billing Kasir telah meratifikasi keputusan resmi sebagai berikut:

1. **Adopsi Opsi A (`BKC-DEC-132`):** Billing menyetujui penerapan **Opsi A** (mengisi kolom existing `ReversesMovementId` pada mutasi `RELEASE` dengan ID mutasi `ALLOCATION` yang dibatalkan) tanpa perubahan skema database.
2. **Granularitas 1-ke-1 (`BKC-DEC-133`):** Bila pembatalan LIFO membatalkan lebih dari satu alokasi tagihan, Billing mencatat 1 baris mutasi `RELEASE` untuk setiap alokasi yang dibatalkan (1-to-1) agar audit trail dan nilai per tagihan/settlement terpetakan sempurna.
3. **Penyelarasan `BillingDepositService` (`BKC-DEC-134`):**
   - Perhitungan `totalRefunded` pada ringkasan deposit mengecualikan mutasi `RELEASE` yang memiliki `ReversesMovementId != null` (hanya menghitung refund kas nyata).
   - Efek saldo (`BalanceEffect`) pada mutasi rekening untuk `RELEASE` pembatalan alokasi dihitung sebagai penambahan (`+Amount`), memulihkan saldo deposit sebelum mutasi `REVERSAL` menariknya (`-Amount`).

Dengan persetujuan ini, `FIN-OQ-037` resmi berstatus **CLOSED**.

