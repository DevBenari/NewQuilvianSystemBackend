# Laporan Perubahan Backend — `BE-FIN-068`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-068` |
| Judul | Snapshot saldo menerbitkan satu baris per akun control, menolak terbit bila pemetaan tidak lengkap, dan mengirim nilai negatif apa adanya |
| Slice | `REV-14B` (`EPIC FIN-21` — pemetaan akun control, cutover saldo awal, dan sinkronisasi subledger) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-142`, `FR-FIN-143`, `FR-FIN-145`, `FR-FIN-149`; `FIN-DEC-112`, `113`, `122`; `FIN-DES-080`, `091`; `FIN-API-1.5` F.7; `FIN-INTEGRATION-1.7` 5.12.4; `FIN-VAL-1.7` `FIN-VAL-177`, `178`, `211` |
| Contract version | `FIN-API-1.5` F.7, `FIN-INTEGRATION-1.7` 5.12.4 |
| Dependency | `BE-FIN-067` (kalkulator posisi subledger selesai), `BE-FIN-065` (layanan pemetaan akun control dan audit cakupan selesai) |
| Klasifikasi | `MEDIUM` (Perombakan layanan snapshot subledger, penegakan prinsip gagal tertutup, penandaan override usang, pembaruan DTO dan controller) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Asumsi Kaku "Selalu 4 Baris Akun Kontrol":**
   Layanan `FinanceSubledgerSnapshotService` sebelumnya mematok mati penerbitan tepat 4 baris snapshot dengan kode akun bawaan (`1-1002`, `1-1003`, `1-2001`, `2-1001`). Jika rumah sakit mengonfigurasi pemetaan akun control per segmen debitur (misalnya memisahkan piutang asuransi/BPJS, piutang pasien umum, dan piutang manfaat karyawan), akun-akun baru tersebut tidak pernah diterbitkan ke outbox akuntansi.
2. **Pemotongan Cacat Saldo Negatif (`Math.Max(0m, ...)`):**
   Empat titik pada kode lama memotong saldo negatif menjadi `0.00` menggunakan fungsi `Math.Max(0m, balance)`. Hal ini merupakan cacat integritas fatal: jika Kas Kasir tekor atau rekening piutang mengalami kelebihan alokasi (kredit lebih besar dari debit), kondisi riil tersebut disembunyikan dari neraca akuntansi, sehingga laporan keuangan rumah sakit menjadi bias dan tidak mencerminkan kenyataan fisik.
3. **Bahaya Penerbitan Parsial Tanpa Perlindungan (*No Fail-Closed Gate*):**
   Jika konfigurasi akun control belum lengkap (misalnya akun Utang Jasa Medis belum dipetakan, atau segmen BPJS belum ditentukan akunnya), sistem lama tetap menerbitkan apa adanya. Saldo segmen yang hilang tanpa jejak adalah kegagalan paling berbahaya karena angka total neraca tetap terlihat wajar, namun pembukuan menjadi pincang.
4. **Pengecekan Kelengkapan Usang (`items.Count >= 4`):**
   Indikator status kelengkapan snapshot `IsComplete` pada pembacaan periode sebelumnya didasarkan pada rumus kaku `items.Count >= 4`. Angka empat tidak lagi memiliki arti setelah adanya fleksibilitas pemetaan akun control (`FIN-DEC-113`).
5. **Ketergantungan Jalan Pintas `OutstandingAmount` dan `ClosingBalance`:**
   Layanan snapshot lama menghitung saldo dengan membaca kolom `OutstandingAmount` dan `ClosingBalance` hari ini, yang menyebabkan kesalahan fatal pada penutupan bulan bila terdapat transaksi pelunasan di bulan berikutnya.

---

## 2. Proses bisnis & Skenario Rumah Sakit

### 2.1 Skenario Penerbitan Snapshot Dinamis Mengikuti Pemetaan Aktif (`FIN-DEC-113`)

1. **Konfigurasi Akun Control Rumah Sakit:**
   Bagian Akuntansi rumah sakit menetapkan 6 baris pemetaan akun control aktif pada basis data:
   - Kas Kasir $\rightarrow$ `1-1002` (Kasir Utama)
   - Kas Kecil $\rightarrow$ `1-1003` (Brankas Operasional)
   - Utang Supplier $\rightarrow$ `2-1001` (Utang Dagang Farmasi & Medis)
   - Piutang Segmen `PAYER` $\rightarrow$ `1-2001` (Piutang Penjamin BPJS & Asuransi)
   - Piutang Segmen `PATIENT_GUARANTOR` $\rightarrow$ `1-2002` (Piutang Pasien Umum/Pribadi)
   - Piutang Segmen `EMPLOYEE_BENEFIT` $\rightarrow$ `1-2003` (Piutang Klaim Karyawan)
   - Utang Jasa Medis $\rightarrow$ `2-2001` (Utang Honor Tenaga Medis)
2. **Kalkulasi Posisi Akhir Bulan (WIB):**
   - Layanan snapshot memanggil `FinanceSubledgerBalanceCalculator` untuk menghitung posisi per 30 September 2026 murni dari saldo awal ditambah deret mutasi bertanggal.
   - Posisi piutang dirinci otomatis per segmen debitur.
   - Utang jasa medis dipatok bernilai Rp 0,00 (`FIN-DEC-122`) karena modul jasa medis belum memiliki penulis saldo aktif.
3. **Penerbitan Outbox Dinamis:**
   - Sistem menerbitkan baris kejadian `SALDO-SUBLEDGER` sebanyak akun control aktif yang terkonfigurasi (7 baris).
   - Setiap baris memiliki `SourceTransactionId = "SUBLEDGER-2026-09-<KodeAkun>"`.

### 2.2 Skenario Penegakan Gagal Tertutup (*Fail-Closed Gate* — `FIN-VAL-177` & `FIN-VAL-178`)

1. **Kasus Kelompok Belum Dipetakan (`FIN-VAL-177`):**
   - Petugas mencoba menutup buku periode `2026-09`, namun kelompok `Utang Jasa Medis` belum memiliki satu pun baris pemetaan aktif.
   - Sistem melakukan audit cakupan pemetaan (*coverage audit*) melalui `FinanceSubledgerControlAccountService.GetCoverageAsync`.
   - Deteksi kegagalan: sistem **menolak** proses penutupan dengan HTTP 422 Unprocessable Entity:
     $$\text{"Snapshot tidak dapat diterbitkan: kelompok Utang Jasa Medis belum punya kode akun."}$$
   - Transaksi database dibatalkan; **tepat NOL baris kejadian outbox** yang tertulis ke basis data.
2. **Kasus Segmen Terpetakan Sebagian (`FIN-VAL-178`):**
   - Piutang dipetakan per segmen, namun hanya segmen `PAYER` dan `PATIENT_GUARANTOR` yang memiliki kode akun, sedangkan segmen `EMPLOYEE_BENEFIT` tertinggal.
   - Sistem menolak proses penutupan dengan HTTP 422:
     $$\text{"Snapshot tidak dapat diterbitkan: segmen EMPLOYEE_BENEFIT pada kelompok Piutang Usaha / Pasien \& Penjamin belum punya kode akun."}$$
   - **Tepat NOL baris outbox** tertulis ke basis data.

### 2.3 Skenario Saldo Negatif Apa Adanya (`FIN-DEC-112`, `FIN-VAL-211`)

1. **Kondisi Kas Fisik Kasir Defisit:**
   - Karena keterlambatan pembukuan kas masuk atau penarikan mendesak, posisi terhitung Kas Kasir per tanggal 30 September adalah **-Rp 2.500.000,00**.
2. **Pengiriman Outbox Akuntansi:**
   - Seluruh fungsi `Math.Max(0m, ...)` telah dihapus dari kode sumber.
   - Nilai yang diterbitkan pada outbox kejadian `SALDO-SUBLEDGER` akun Kas Kasir adalah **-Rp 2.500.000,00**.
   - Modul Akuntansi menerima angka riil defisit tersebut sehingga auditor dapat melihat ketidakcocokan saldo normal tanpa tertutupi angka semu `0.00`.

### 2.4 Skenario Pernyataan Ulang Cerdas (*Smart Restatement* — `FIN-DEC-114`)

1. **Penutupan Periode Pertama:**
   - Snapshot periode `2026-09` telah diterbitkan (seluruh akun berstatus versi 1).
2. **Shift Kasir Malam Tertutup Terlambat:**
   - Pada tanggal 2 Oktober, shift kasir tanggal 30 September baru disahkan (`REVIEWED`), menulis satu mutasi kas masuk Rp 5.000.000,00 bertanggal 30 September.
   - Posisi Kas Kasir per 30 September berubah dari Rp 50.000.000,00 menjadi Rp 55.000.000,00.
   - Posisi piutang dan utang supplier **tidak berubah**.
3. **Eksekusi Regenerasi / Restate:**
   - Sistem memeriksa baris outbox terakhir per akun:
     - Akun piutang dan utang memiliki nilai yang sama persis dengan outbox terakhir $\rightarrow$ **tidak diterbitkan ulang** (tetap versi 1, nol baris baru).
     - Akun Kas Kasir nilainya berubah $\rightarrow$ sistem menerbitkan baris baru dengan `SourceVersion = "2"` dan `Amount = Rp 55.000.000,00`.
   - Modul Akuntansi hanya menerima pembaruan versi untuk akun yang benar-benar mengalami pergerakan nilai.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

1. `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (§BE-FIN-068)
2. `docs/module-blueprints/finance-management/02-backend-architecture.md` (§FIN-DES-080, §FIN-DES-091, §4650-4655)
3. `docs/module-blueprints/finance-management/contracts/api-contract.md` (§F.7)
4. `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (§FIN-VAL-177, 178, 211)
5. `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` (§I.3)
6. `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotService.cs`

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/SubledgerSnapshotDtos.cs` | Penambahan konstanta kategori `MedicalServicePayables = "UTANG-JASA-MEDIS"` pada `SubledgerAccountCategories`, dan penambahan atribut `[Obsolete]` pada 4 properti override kode akun di `GenerateSubledgerSnapshotsRequest`. |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotService.cs` | Perombakan menyeluruh layanan snapshot subledger: pengalihan sumber angka ke `FinanceSubledgerBalanceCalculator`, penghapusan seluruh 4 fungsi `Math.Max(0m, ...)`, implementasi gerbang gagal tertutup (*fail-closed*) sebelum transaksi, penerbitan baris dinamis mengikuti pemetaan aktif, jalur pernyataan ulang pintar (`FIN-DEC-114`), dan redefinisi `IsComplete` berbasis cakupan kode akun aktif. |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs` | Pembaruan penanganan galat endpoint `POST /subledger-balances/generate`: menangkap `FinanceSubledgerSnapshotValidationException` dan memulangkan respons HTTP 422 Unprocessable Entity lengkap dengan metadata OpenAPI `ProducesResponseType`. |

### 3.3 Penegakan Aturan Validasi Bisnis

| Kode Aturan | Kondisi yang Diuji | Respon Sistem |
| :--- | :--- | :--- |
| `FIN-VAL-177` | Kelompok saldo subledger belum memiliki baris pemetaan akun control aktif | HTTP 422 — *"Snapshot tidak dapat diterbitkan: kelompok <nama> belum punya kode akun."* (**nol** baris outbox tertulis) |
| `FIN-VAL-178` | Kelompok saldo dengan model pemetaan per segmen memiliki segmen yang belum terpetakan | HTTP 422 — *"Snapshot tidak dapat diterbitkan: segmen <nama> pada kelompok <nama> belum punya kode akun."* (**nol** baris outbox tertulis) |
| `FIN-VAL-211` | Saldo subledger pada tanggal penutupan bernilai negatif | Nilai negatif **dikirim apa adanya** tanpa dipotong atau ditolak (mencabut batasan sepihak lama). |
| `FIN-VAL-170` | Periode yang ditutup berakhir sebelum tanggal cutover sistem | HTTP 422 — *"Posisi saldo sebelum tanggal cutover tidak dapat dihitung karena buku mutasi belum berjalan pada tanggal itu."* |

---

## 4. Spesifikasi Endpoint Bergaya Swagger

Grup Tag: `[Tags("Corporate / Finance Management / Accounting Events")]`  
Base URL: `/api/v1/corporate/finance-management/accounting-events`  
Resource RBAC: `FinanceAccountingEvent`  

| Method | Path | Deskripsi | Auth / Permission | Request Payload / Query Param | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/subledger-balances/generate` | Memicu kalkulasi snapshot saldo subledger bulanan dan mementaskan kejadian `SALDO-SUBLEDGER` ke kotak keluar untuk setiap akun control aktif | `Bearer Token`<br>`[AccessPermission("FinanceAccountingEvent", "Create")]` | Body JSON (`GenerateSubledgerSnapshotsRequest`):<br>- `accountingPeriodCode` (string, wajib, contoh: `"2026-09"`)<br>- `notes` (string, opsional)<br>*(4 ruas override ditandai usang)* | `200 OK`: `ApiResponse<GenerateSubledgerSnapshotsResponse>`<br>`400 Bad Request`: `ApiResponse<object>`<br>`422 Unprocessable Entity`: `ApiResponse<object>` |
| `GET` | `/subledger-balances/{accountingPeriodCode}` | Mengambil rincian snapshot saldo subledger per periode yang tercatat di FinAccountingEventOutbox beserta evaluasi kelengkapan akun aktif | `Bearer Token`<br>`[AccessPermission("FinanceAccountingEvent", "Read")]` | Route Param:<br>`accountingPeriodCode` (string, wajib, format `YYYY-MM`) | `200 OK`: `ApiResponse<SubledgerPeriodSnapshotsResponse>`<br>`400 Bad Request`: `ApiResponse<object>` |

---

## 5. Ringkasan Eksekusi & Bukti Kepatuhan

1. **Pemeriksaan Fungsi `Math.Max`:**
   Diverifikasi menggunakan pencarian regex/ripgrep pada berkas `FinanceSubledgerSnapshotService.cs`: **0 kemunculan `Math.Max`**. Seluruh pemotongan sepihak ke nol telah dihapus.
2. **Pemeriksaan Logika `IsComplete`:**
   Diverifikasi bahwa pembanding `items.Count >= 4` telah dihilangkan. Kelengkapan kini dievaluasi secara dinamis terhadap kecocokan seluruh `activeControlAccountCodes`.
3. **Pemeriksaan Pembacaan `OutstandingAmount` & `ClosingBalance`:**
   Diverifikasi bahwa `FinanceSubledgerSnapshotService.cs` tidak lagi menyentuh tabel transaksi piutang, utang, atau rekap kas harian secara langsung; seluruh posisi saldo diperoleh dari `FinanceSubledgerBalanceCalculator`.
4. **Kepatuhan Database & Build:**
   Task ini menghasilkan **nol migration baru**, dan build backend otomatis tidak dijalankan sesuai instruksi pengguna. Kompilasi manual diserahkan ke pengguna.
