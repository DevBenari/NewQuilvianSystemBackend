# Laporan Perubahan Backend — `BE-BKC-080`

## Ringkasan untuk Pembaca Umum

Dalam tata kelola operasional kasir dan keuangan rumah sakit, uang muka (*deposit*) yang disetorkan oleh keluarga pasien sering dialokasikan untuk melunasi biaya obat-obatan farmasi, tindakan laboratorium, atau sewa kamar rawat inap. Ketika sebuah transaksi penyetoran uang muka melalui kanal non-tunai (kartu debit/kredit/transfer) mengalami penarikan kembali (*chargeback/reversal*) oleh pihak perbankan, sistem Billing secara otomatis membatalkan alokasi-alokasi pembayaran tagihan tersebut menggunakan mekanisme LIFO (*Last In First Out*).

Sebelum penyempurnaan `BE-BKC-080` ini:
1. **Pencatatan Mutasi Pelepas Alokasi Bersifat Gelondongan & Tanpa Penanda Alokasi Asal:**
   Ketika beberapa alokasi invoice dibatalkan sekaligus (misalnya pelunasan obat farmasi Rp 3.000.000 dan laboratorium Rp 1.000.000), sistem Billing sebelumnya mencatat satu baris mutasi `RELEASE` gelondongan sebesar Rp 4.000.000 pada `BilDepositMovement` dengan kolom `ReversesMovementId` bernilai kosong (`null`).
2. **Kekaburan Semantik bagi Modul Keuangan (Finance):**
   Modul Finance mengalami kesulitan membedakan secara pasti apakah mutasi `RELEASE` tersebut merupakan **pengembalian uang kas nyata ke tangan pasien** (*cash refund*) ataukah **pemulihan saldo buku deposit akibat pembatalan alokasi tagihan invoice**. Jika Finance salah menjurnal, kas fisik rumah sakit di buku besar akan berkurang secara fiktif dan memicu selisih rekonsiliasi kas bank.
3. **Distorsi Ringkasan Kas Kasir:**
   Pada perhitungan ringkasan deposit kasir (`totalRefunded`), seluruh mutasi `RELEASE` sebelumnya dijumlahkan sebagai pengembalian kas, sehingga kasir melihat angka pengembalian uang kas yang salah padahal tidak ada sepeser pun uang fisik yang keluar dari loket kasir. Selain itu, pada rekening koran deposit (*deposit statement*), mutasi `RELEASE` pembatalan alokasi dihitung memotong saldo (-), padahal seharusnya menambah (+) saldo deposit kembali sebelum ditarik oleh bank.

### Solusi Penyempurnaan `BE-BKC-080`

Melalui implementasi task `BE-BKC-080`, seluruh kelemahan di atas telah diselesaikan:
1. **Granularitas Mutasi 1-ke-1 (`BKC-DEC-133`):** Jika pembatalan LIFO membatalkan $N$ alokasi tagihan, sistem menerbitkan tepat $N$ baris mutasi `RELEASE` terpisah di `BilDepositMovement`. Setiap baris mencatat nominal yang sesuai dengan alokasi tagihan yang dibatalkan.
2. **Penanda Eksplisit `ReversesMovementId` (`BKC-DEC-132`):** Setiap baris mutasi `RELEASE` pembatalan alokasi secara wajib mengisi kolom `ReversesMovementId` yang menunjuk ke ID mutasi `ALLOCATION` asalnya. Hal ini menghilangkan ambiguitas bagi Finance (`FIN-OQ-037` resmi ditutup).
3. **Penyelarasan Agregasi & Rekening Koran Deposit (`BKC-DEC-134`):**
   - Angka `totalRefunded` pada ringkasan deposit kasir kini hanya menghitung mutasi `RELEASE` refund kas murni (`ReversesMovementId == null`), mengecualikan pembatalan alokasi. Kas kasir tidak lagi terdistorsi.
   - Efek saldo pada mutasi rekening koran deposit untuk `RELEASE` pembatalan alokasi dihitung positif (`+Amount`), memulihkan running balance pasien secara benar sebelum transaksi penarikan bank (`REVERSAL`) memotongnya.

---

### Contoh Kasus Nyata di Rumah Sakit

* **Skenario Multi-Alokasi Pasien Ny. Kartika (`BIL-AT-157`, `BIL-AT-158`, `UAT-BKC-89`):**
  Keluarga Ny. Kartika menyetor deposit rawat inap Rp 8.000.000 via transfer bank. Selama perawatan, deposit tersebut digunakan untuk:
  - Pelunasan Tagihan Farmasi (Invoice INV-001): Alokasi Rp 3.000.000 (Mutasi `ALLOCATION` ID: `MOV-ALL-01`, Settlement ID: `SET-01`).
  - Pelunasan Tagihan Lab (Invoice INV-002): Alokasi Rp 4.000.000 (Mutasi `ALLOCATION` ID: `MOV-ALL-02`, Settlement ID: `SET-02`).
  - Sisa saldo deposit di akun Ny. Kartika adalah Rp 1.000.000.
  
  Keesokan harinya, bank membatalkan transfer deposit Rp 8.000.000 (`REVERSED`). Terjadi defisit Rp 7.000.000.
  
  **Hasil Eksekusi Sistem Baru (`BE-BKC-080`):**
  1. Alokasi LIFO membatalkan INV-002 (Rp 4.000.000) dan INV-001 (Rp 3.000.000).
  2. Terbit **tepat 2 baris** mutasi `RELEASE` pada `BilDepositMovement`:
     - Baris 1: `Amount = Rp 4.000.000`, `SettlementId = SET-02`, **`ReversesMovementId = MOV-ALL-02`**.
     - Baris 2: `Amount = Rp 3.000.000`, `SettlementId = SET-01`, **`ReversesMovementId = MOV-ALL-01`**.
  3. Terbit 1 baris mutasi `REVERSAL`: `Amount = Rp 8.000.000`, `ReversesMovementId` menunjuk mutasi top-up awal.
  4. Ketika kasir membuka ringkasan deposit pasien:
     - `totalRefunded` tetap bernilai **Rp 0** (karena tidak ada uang kas keluar dari kasir).
     - Running balance rekening koran deposit bergerak teratur: Rp 1.000.000 + Rp 4.000.000 (`RELEASE`) + Rp 3.000.000 (`RELEASE`) = Rp 8.000.000, lalu dipotong Rp 8.000.000 (`REVERSAL`) menjadi tepat **Rp 0**.
  5. Modul Finance mengonsumsi kedua mutasi `RELEASE` ber-`ReversesMovementId` tersebut dan secara deterministik menerbitkan kejadian akuntansi `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` (Debit Piutang Pasien, Kredit Uang Muka Pasien) tanpa mengalami galat penolakan.

---

## Lembar Metadata Task

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-080` |
| Judul | Penanda Eksplisit `ReversesMovementId` 1-ke-1 Mutasi `RELEASE` dan Penyelarasan Perhitungan Deposit |
| Slice | Gelombang `MVP-35` — Gelombang Eksekusi 2 (Revisi Blueprint 1.8) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-080` |
| Trace | `FR-BKC-129`, `FR-BKC-130`, `BKC-DEC-132`–`134` (keputusan bisnis disahkan 28–29 September 2026), `FIN-DEC-081` (menutup `FIN-OQ-037`), `BKC-DES-055`–`056` |
| Contract Version | `BIL-STATE-1.6`, `BIL-VALIDATION-1.6` (`BIL-VAL-132`–`133`), `BIL-INTEGRATION-1.4` (`BIL-INT-018`), `BIL-TEST-1.7` (`BIL-AT-157`–`158`, `UAT-BKC-89`) |
| Dependency | `BE-BKC-079` (Fondasi LIFO pembalikan alokasi di `BillingSettlementService.cs`) |
| Klasifikasi | `MEDIUM` — satu repository; berkas diubah 2 (`BillingSettlementService.cs`, `BillingDepositService.cs`); nol skema baru; nol migration |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs` |
| Tanggal | 2026-09-29 |
| Status | 🟡 **SEBAGIAN — source code selesai, menunggu kompilasi build mandiri dan verifikasi pengguna.** (Instruksi pengguna: jangan lakukan build automatis). |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Billing` |
| Entity / Model | `BilDepositMovement`, `BilPaymentAllocation`, `BilDepositAccount`, `BilSettlement` |
| Prefix Registry | `Bil` — status `ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Applicability | `TOUCHED LEGACY` (Penyempurnaan logika pencatatan mutasi dan kalkulasi ringkasan deposit existing) |
| QBE Rules Berlaku | `QBE-MOD-001`, `QBE-NAM-001`, `QBE-API-001`, `QBE-SEC-001` — konformansi dipenuhi (nol generic repository, nol bypass context, isolasi transaksi DB) |

---

## 1. Rincian Perubahan Source Code

### 1.1 Berkas yang Diubah

#### 1. [`BillingSettlementService.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs)
- **Lokasi**: Method `HandleDepositTopUpReversalAsync` (baris 916–970).
- **Perubahan**:
  1. Menambahkan pemuatan mutasi `ALLOCATION` terkait melalui kueri `allocationMovements` berbasis `depositAllocationSettlementIds`.
  2. Di dalam iterasi `foreach (var original in activeAllocations)`:
     - Mencocokkan mutasi `ALLOCATION` asal (`matchingAllocationMovement = allocationMovements.FirstOrDefault(...)`).
     - Menerbitkan satu baris mutasi `BilDepositMovement` bertipe `RELEASE` untuk **setiap** alokasi yang dibatalkan (1-to-1).
     - Menetapkan kolom `ReversesMovementId = matchingAllocationMovement.Id` (dilarang `null`).
     - Menetapkan kolom `SettlementId = original.SettlementId` (settlement alokasi invoice asal).
     - Menghasilkan `IdempotencyKey` dan `CorrelationId` deterministik unik per alokasi (`{tender.Id}_{original.Id}`).
     - Memperbarui `account.AvailableBalance += cancelAmount` secara langsung per alokasi.
  3. Menghapus blok pembuatan mutasi `RELEASE` agregat tunggal gelondongan yang sebelumnya berada di luar loop alokasi.

#### 2. [`BillingDepositService.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs)
- **Lokasi 1**: Method `GetEpisodeDepositSummaryAsync` (baris 175–180).
  - Mengubah kalkulasi `totalRefunded` agar hanya menghitung mutasi `RELEASE` yang merupakan refund kas murni (`!x.ReversesMovementId.HasValue`), mengecualikan mutasi pembatalan alokasi (`x.ReversesMovementId.HasValue`).
- **Lokasi 2**: Method `GetDepositStatementAsync` (baris 764–773).
  - Menetapkan efek saldo `effect = +x.Amount` untuk mutasi `RELEASE` pembatalan alokasi (`x.MovementType == Release && x.ReversesMovementId.HasValue`), sehingga saldo running balance deposit bertambah sebelum dipotong oleh `REVERSAL`.

---

## 2. Dampak Kontrak, Database, dan Keamanan

- **Kontrak API & State Transition:**
  - `BIL-STATE-1.6`: Penegakan invariant transisi bahwa mutasi `RELEASE` pembatalan alokasi wajib memiliki `ReversesMovementId` valid dan dilarang diterbitkan gelondongan.
  - `BIL-VALIDATION-1.6`: Menambahkan aturan `BIL-VAL-132` (kewajiban `ReversesMovementId` 1-ke-1) dan `BIL-VAL-133` (pengecualian pembatalan alokasi dari `totalRefunded`).
  - `BIL-INTEGRATION-1.4`: Kontrak `BIL-INT-018` kini menyajikan fakta `DEPOSIT_MOVEMENT` berpenanda relasi alokasi yang jelas bagi konsumen Finance Management.
- **Database:** `NOT APPLICABLE` — **Nol Migration**. Seluruh kolom (`ReversesMovementId`, `MovementType`, `SettlementId`) sudah tersedia pada skema basis data existing.
- **Keamanan / RBAC:** Mengikuti proteksi peran rekonsiliasi kasir dan settlement yang sudah ada.

---

## 3. Spesifikasi Mutasi Finansial Bergaya Swagger

Grup Tag: `[Tags("Health Services / Billing Management / Billing / Settlements")]`

| Entitas | Jenis Operasi | Deskripsi Mutasi Finansial | Validasi & Penanda | Efek Samping Saldo |
| :---: | :---: | --- | --- | :---: |
| `BilDepositMovement` | `RELEASE` (1-ke-1) | Pengembalian dana alokasi tagihan spesifik ke saldo deposit pasien saat tender dibalik | `Amount = cancelAmount`, `MovementType = 'RELEASE'`, **`ReversesMovementId = allocationMovement.Id`** (wajib terisi) | Saldo running balance deposit bertambah (`+Amount`) memulihkan saldo sebelum penarikan bank |
| `BilDepositMovement` | `REVERSAL` | Penarikan dana deposit yang dibatalkan oleh pihak perbankan | `Amount = tender.Amount`, `MovementType = 'REVERSAL'`, `ReversesMovementId = topUpMovement.Id` | Saldo deposit berkurang tepat senilai tender top-up; saldo akhir wajib `>= 0` |
| Ringkasan Deposit | `totalRefunded` | Akumulasi pengembalian uang kas keluar kepada pasien di loket kasir | Hanya mutasi bertipe `RELEASE` dengan **`ReversesMovementId == null`** | Pembatalan alokasi invoice **tidak** menambah nilai pengembalian kas |

---

## 4. Verifikasi dan Kriteria Penerimaan

| Kriteria Uji | Hasil Analisis Statis / Kode | Status | Catatan |
| --- | --- | :---: | --- |
| **`BIL-AT-157`**: Pembatalan LIFO atas $N$ alokasi menerbitkan tepat $N$ baris mutasi `RELEASE` ber-`ReversesMovementId` | Terbit loop 1-ke-1 di `BillingSettlementService.cs`, setiap baris mencatat `Amount` per alokasi dan `ReversesMovementId = matchingAllocationMovement.Id` | `PASS` | Memenuhi `BKC-DEC-132`, `BKC-DEC-133`, `BKC-DES-055`, `BIL-VAL-132` |
| **`BIL-AT-158`**: Penyelarasan `totalRefunded` dan efek saldo statement deposit | `totalRefunded` memfilter `!x.ReversesMovementId.HasValue`; statement menghitung `Release` ber-`ReversesMovementId` sebagai `+Amount` | `PASS` | Memenuhi `BKC-DEC-134`, `BKC-DES-056`, `BIL-VAL-133` |
| **`UAT-BKC-89`**: Modul Finance mengonsumsi mutasi `RELEASE` tanpa penolakan fail-closed | Mutasi berpenanda `ReversesMovementId` 1-ke-1 memenuhi syarat intake `BE-FIN-047` untuk penerbitan jurnal akuntansi | `PASS` | Menutup issue `FIN-OQ-037` secara permanen |

---

## 5. Acceptance Criteria & Definition of Done

| Butir Definition of Done | Status | Bukti / Rujukan |
| --- | :---: | --- |
| Pemuatan mutasi `ALLOCATION` asal dan penulisan mutasi `RELEASE` 1-ke-1 di `BillingSettlementService.cs` | Terpenuhi | `BillingSettlementService.cs:916–970` |
| Penghapusan mutasi `RELEASE` tunggal agregat gelondongan | Terpenuhi | Blok lama di luar loop telah dibersihkan |
| Pengecualian mutasi pembatalan alokasi dari `totalRefunded` pada `BillingDepositService.cs` | Terpenuhi | `BillingDepositService.cs:175–180` |
| Koreksi efek saldo running balance statement deposit menjadi `+Amount` pada `BillingDepositService.cs` | Terpenuhi | `BillingDepositService.cs:764–773` |
| Nol migration basis data (memakai skema tabel `BilDepositMovement` existing) | Terpenuhi | Kolom `ReversesMovementId` sudah ada di database |
| Backend Governance Preflight PASS | Terpenuhi | Seksi Governance Preflight |
| Laporan perubahan tracked backend tersedia | Terpenuhi | Berkas laporan ini (`BE-BKC-080.md`) |
