# Laporan Perubahan Backend — `BE-FIN-067`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-067` |
| Judul | Finance dapat menyatakan posisi setiap kelompok saldo pada tanggal mana pun sejak cutover |
| Slice | `REV-14B` (`EPIC FIN-21` — pemetaan akun control, cutover saldo awal, dan sinkronisasi subledger) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-134`, `FR-FIN-135`, `FR-FIN-139`; `FIN-DEC-114`, `125`; `FIN-DES-081`; `FIN-API-1.5` F.6; `FIN-VAL-1.7` `FIN-VAL-170` |
| Contract version | `FIN-API-1.5` F.6, `FIN-VAL-1.7` `FIN-VAL-170` |
| Dependency | `BE-FIN-066` (saldo awal cutover terkunci), `BE-FIN-062` (buku mutasi kas tersambung), `BE-FIN-060` (buku mutasi piutang & utang) |
| Klasifikasi | `MEDIUM` (1 DTO berkas baru, 1 service kalkulator baru, 2 endpoint pada controller yang sudah ada, 1 registrasi DI) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Ketiadaan Layanan Kalkulasi Posisi Saldo Historis Berbasis Jejak:**
   Sistem Finance tidak memiliki cara untuk menghitung posisi saldo subledger pada suatu tanggal tertentu di masa lalu (*point-in-time balance*). Saldo yang disajikan ke pihak manajemen maupun modul akuntansi hanya berupa saldo berjalan saat ini (*current balance*).
2. **Cacat Arsitektur Jalan Pintas (`OutstandingAmount` dan `ClosingBalance`):**
   Pada implementasi awal, snapshot saldo subledger membaca kolom `OutstandingAmount` pada piutang/utang dan `ClosingBalance` pada rekap kas harian. Nilai-nilai tersebut merupakan posisi saldo **sekarang**, bukan posisi saldo pada tanggal yang diminta. Akibatnya, transaksi pelunasan yang terjadi di periode berikutnya (misalnya tanggal 1 Oktober) secara keliru mengurangi saldo penutupan periode sebelumnya (30 September).
3. **Pencampuran Peran Rekap Kas Harian Operasional:**
   Rekap kas harian (`FinDailyCashSnapshot`) sebelumnya diposisikan sebagai sumber kebenaran saldo buku besar, padahal kasir memerlukan laporan operasional yang fleksibel dan dapat ditutup walau masih ada shift kasir yang sedang ditelaah. Akibat pergeseran paradigma `FIN-DES-081`, kas kasir dihitung murni dari buku mutasi kas bertanggal, sedangkan rekap harian diturunkan menjadi laporan operasional pembanding.
4. **Kebutuhan Permukaan Baca Selisih Kas Operasional (`FIN-DEC-125`):**
   Karena posisi kas dan rekap kas harian kini dihitung dari dua sumbu yang berbeda, keduanya dapat berselisih secara sah (misalnya karena setoran tertunda atau penyesuaian kasir). Selisih ini **wajib ditampilkan secara transparan** berdampingan beserta rincian mutasi kas yang menjelaskannya, bukan disembunyikan atau dipaksa bernilai nol.
5. **Ketiadaan Validasi Batas Tanggal Cutover (`FIN-VAL-170`):**
   Buku mutasi subledger tidak diisi mundur sebelum tanggal *cutover*. Permintaan saldo untuk tanggal sebelum *cutover* berisiko memulangkan angka yang tampak sah namun secara faktual keliru.

---

## 2. Proses bisnis & Skenario Rumah Sakit

### 2.1 Skenario Kasus Uji Dua Pembayaran Lintas Periode (Uji Kritis `FIN-DEC-114`)

1. **Kondisi Awal Piutang Pasien BPJS:**
   - Pada tanggal 25 September 2026, terbit tagihan klaim BPJS Kesehatan senilai Rp 100.000.000,00 (`PENGAKUAN`, `BalanceAfter = Rp 100.000.000,00`).
2. **Pembayaran Pertama (Dalam Periode September):**
   - Pada tanggal 29 September 2026, BPJS mencairkan klaim tahap I sebesar Rp 40.000.000,00 (`ALOKASI-PENERIMAAN`, `Amount = -Rp 40.000.000,00`, `BalanceAfter = Rp 60.000.000,00`).
3. **Pembayaran Kedua (Lintas Periode — Awal Oktober):**
   - Pada tanggal 2 Oktober 2026, BPJS mencairkan sisa klaim tahap II sebesar Rp 60.000.000,00 (`ALOKASI-PENERIMAAN`, `Amount = -Rp 60.000.000,00`, `BalanceAfter = Rp 0,00`).
   - Nilai kolom `OutstandingAmount` pada tabel `FinReceivable` saat ini adalah **Rp 0,00**.
4. **Hasil Kalkulasi Posisi Per 30 September 2026:**
   - Petugas Akuntansi meminta posisi piutang per tanggal `2026-09-30` (`GET /subledger-balances/position?asOfDate=2026-09-30`).
   - Kalkulator membaca seluruh mutasi piutang dengan `BusinessDate <= 2026-09-30`.
   - Mutasi tanggal 2 Oktober **diabaikan** dari perhitungan periode September.
   - Hasil posisi terhitung piutang per 30 September:
     $$\text{Posisi} = \text{Rp 100.000.000,00} - \text{Rp 40.000.000,00} = \mathbf{Rp\ 60.000.000,00}$$
   - Terbukti **nol pembacaan `OutstandingAmount`**, sehingga saldo piutang September tidak terdistorsi menjadi Rp 0,00.

### 2.2 Skenario Selisih Rekapitulasi Kas Harian Terhadap Posisi Kas Terhitung (`FIN-DEC-125`)

1. **Kejadian Kas Kasir:**
   - Saldo awal cutover Kas Kasir: Rp 50.000.000,00.
   - Total penerimaan kasir shift final selama bulan September: Rp 120.000.000,00.
   - Setoran bank yang telah dibukukan (*POSTED*): Rp 100.000.000,00.
   - Posisi Kas Kasir terhitung pada `2026-09-30`:
     $$\text{Posisi Kas Terhitung} = \text{Rp 50.000.000,00} + \text{Rp 120.000.000,00} - \text{Rp 100.000.000,00} = \mathbf{Rp\ 70.000.000,00}$$
2. **Kondisi Rekap Kas Harian Kasir:**
   - Pada lembar rekap harian terakhir per `2026-09-30`, kasir mencatat `ClosingBalance` fisik sebesar Rp 68.500.000,00 karena terdapat selisih kas fisik Rp 1.500.000,00 yang masih dalam proses telaah (*review*).
3. **Penyajian Selisih:**
   - Melalui endpoint `GET /subledger-balances/2026-09/variance`, sistem mengembalikan:
     - `CalculatedCashPosition`: Rp 70.000.000,00
     - `DailyCashClosingBalance`: Rp 68.500.000,00
     - `VarianceAmount`: -Rp 1.500.000,00
     - `HasVariance`: `true`
     - `ExplainingMovements`: Daftar seluruh baris mutasi kas pada bulan September beserta nomor shift dan metode pembayarannya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

1. `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (§BE-FIN-067)
2. `docs/module-blueprints/finance-management/02-backend-architecture.md` (§FIN-DES-081, §L.8)
3. `docs/module-blueprints/finance-management/contracts/api-contract.md` (§F.6)
4. `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (§FIN-VAL-170)
5. `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` (§I.1, §I.2)
6. `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs`

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Dtos/SubledgerPositionDtos.cs` | **Dibuat.** DTO representasi kalkulasi posisi subledger (`SubledgerPositionResponse`, `SubledgerGroupPositionItem`, `SubledgerSegmentPositionItem`) dan selisih kas harian (`CashVarianceResponse`, `CashMovementExplainingItem`). |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerBalanceCalculator.cs` | **Dibuat.** Layanan komputasi posisi subledger murni berbasis buku mutasi dan saldo awal cutover, penegakan `FIN-VAL-170`, kalkulasi segmen piutang dan utang jasa medis, serta analisis selisih kas operasional periode. |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs` | **Diperbarui.** Injeksi dependensi `FinanceSubledgerBalanceCalculator` dan penambahan 2 endpoint baca: `GET /subledger-balances/position` dan `GET /subledger-balances/{accountingPeriodCode}/variance` dengan otorisasi `FinanceAccountingEvent : Read`. |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | **Diperbarui.** Pendaftaran dependensi `FinanceSubledgerBalanceCalculator` sebagai layanan berlingkup (`AddScoped`). |

### 3.3 Penegakan Aturan Validasi Bisnis

| Kode Aturan | Kondisi yang Diuji | Respon Sistem |
| :--- | :--- | :--- |
| `FIN-VAL-170` | Permintaan posisi saldo untuk tanggal yang lebih awal daripada tanggal cutover (`asOfDate < CutoverDate` atau `periodEndDate < CutoverDate`) | HTTP 422 — *"Posisi saldo sebelum tanggal cutover tidak dapat dihitung karena buku mutasi belum berjalan pada tanggal itu."* |
| `FIN-VAL-Format` | Format kode periode akuntansi tidak sesuai pola `YYYY-MM` | HTTP 400 — *"Kode periode harus berbentuk YYYY-MM, contoh: 2026-09."* |

---

## 4. Spesifikasi Endpoint Bergaya Swagger

Grup Tag: `[Tags("Corporate / Finance Management / Accounting Events")]`  
Base URL: `/api/v1/corporate/finance-management/accounting-events`  
Resource RBAC: `FinanceAccountingEvent`  

| Method | Path | Deskripsi | Auth / Permission | Request Payload / Query Param | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/subledger-balances/position` | Menghitung posisi seluruh kelompok saldo subledger dan rincian segmennya pada tanggal tertentu sejak cutover murni dari saldo awal + buku mutasi | `Bearer Token`<br>`[AccessPermission("FinanceAccountingEvent", "Read")]` | Query Param:<br>`asOfDate` (`DateOnly`, format `YYYY-MM-DD`, Wajib) | `200 OK`: `ApiResponse<SubledgerPositionResponse>`<br>`400 Bad Request`: `ApiResponse<object>`<br>`422 Unprocessable Entity`: `ApiResponse<object>` |
| `GET` | `/subledger-balances/{accountingPeriodCode}/variance` | Mengambil perbandingan selisih rekapitulasi kas harian operasional terhadap posisi kas terhitung periode tertentu beserta mutasi kas penjelas | `Bearer Token`<br>`[AccessPermission("FinanceAccountingEvent", "Read")]` | Route Param:<br>`accountingPeriodCode` (`string`, format `YYYY-MM`, Wajib) | `200 OK`: `ApiResponse<CashVarianceResponse>`<br>`400 Bad Request`: `ApiResponse<object>`<br>`422 Unprocessable Entity`: `ApiResponse<object>` |

---

## 5. Ringkasan Eksekusi & Status

1. **Pemisahan Sumber Kebenaran Posisi Saldo:**
   `FinanceSubledgerBalanceCalculator` berhasil mengimplementasikan kalkulasi murni dari saldo awal cutover (`FinOpeningBalance`) ditambah deret mutasi bertanggal (`FinCashMovement`, `FinReceivableMovement`, `FinSupplierPayableMovement`).
2. **Kepatuhan Larangan Cacat:**
   Dipastikan **nol pembacaan kolom `OutstandingAmount`** pada piutang/utang dan **nol pembacaan `ClosingBalance`** sebagai penentu angka posisi subledger.
3. **Kepatuhan Database & Build:**
   Task ini bersifat murni baca (*read-only*), menghasilkan **nol migration baru**, dan build backend otomatis tidak dijalankan sesuai instruksi pengguna. Kompilasi manual diserahkan ke pengguna.
