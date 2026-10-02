# Laporan Perubahan Backend — `BE-FIN-066`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-066` |
| Judul | Saldo awal cutover dapat dicatat, disetujui, dan dikunci satu kali |
| Slice | `REV-14B` (`EPIC FIN-21` — pemetaan akun control, cutover saldo awal, dan sinkronisasi subledger) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-146`..`148`; `FIN-DEC-128`; `FIN-DES-088`; `FIN-API-1.5` F.1; `FIN-STATE-1.6` F.1; `FIN-VAL-1.7` `FIN-VAL-180`..`185`; `FIN-PERM-1.7` G.1-G.3 |
| Contract version | `FIN-API-1.5` F.1, `FIN-STATE-1.6` F.1 |
| Dependency | `BE-FIN-064` (skema tabel `FinOpeningBalance` berdiri), `BE-FIN-062` (buku mutasi kas) |
| Klasifikasi | `MEDIUM` (1 DTO, 1 service, 5 endpoint pada controller yang sudah ada, 1 registrasi DI) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Tidak Ada Layanan Pencatatan Saldo Awal Cutover:**
   Saat rumah sakit beralih ke Quilvian V2 (*go-live*), sistem Finance tidak memiliki alur operasional di tingkat backend untuk mencatat, memvalidasi, menyetujui, dan mengunci posisi saldo awal per tanggal *cutover*.
2. **Bahaya Modifikasi Saldo Awal Berjalan:**
   Jika saldo awal dapat diubah sewaktu-waktu oleh staf operasional, seluruh kalkulasi posisi subledger, laporan aging piutang, rekonsiliasi utang supplier, dan snapshot neraca bulanan ke Accounting akan mengalami distorsi parah. Diperlukan penegakan mesin status satu arah (`DRAFT` $\rightarrow$ `APPROVED` $\rightarrow$ `LOCKED`) dengan token konkurensi (`RowVersion`) dan perlindungan permanen pada status `LOCKED`.
3. **Risiko Penghitungan Ganda Saldo Awal Tagihan Lama:**
   Pada kelompok saldo piutang dan utang, saldo awal harus bersumber murni dari rincian dokumen individual (faktur klaim asuransi pasien atau faktur supplier) melalui proses impor berkas migrasi batch (`FIN-DES-089`). Jika saldo awal pada tabel header ini diizinkan bernilai nominal gelondongan, saldo akan terhitung dua kali lipat.
4. **Keterputusan dengan Buku Mutasi Kas:**
   Penguncian saldo awal kas kasir sebelumnya belum terintegrasi ke buku mutasi kas (`FinCashMovement`), sehingga kasir tidak dapat melacak awal mula saldo fisik kas pada tanggal *cutover*.

---

## 2. Proses bisnis & Skenario Rumah Sakit

### 2.1 Skenario Operasional Rumah Sakit

1. **Kas Kasir & Kas Kecil:**
   - Rumah sakit menetapkan tanggal cutover sistem baru per `2026-10-01`.
   - Berdasarkan Berita Acara Rekonsiliasi Kas Fisik (*Cash Count*) bersama auditor internal, kas fisik di brankas kasir utama berjumlah Rp 50.000.000,00 dan kas kecil operasional berjumlah Rp 10.000.000,00.
   - Staf keuangan menginput nominal tersebut ke dalam sistem dengan mencantumkan nomor dokumen BAP dan alasan yang jelas.
2. **Piutang Pasien & Utang Supplier:**
   - Piutang pasien BPJS/asuransi dan utang supplier farmasi berjumlah miliaran rupiah yang terdiri dari ribuan lembar tagihan berjalan.
   - Sesuai aturan akuntansi rumah sakit, angka saldo awal pada tabel `FinOpeningBalance` **wajib bernilai 0.00** beserta catatan alasan *"Rincian saldo awal datang dari migrasi batch tagihan individual"*. Rincian faktur tersebut nantinya akan diimpor melalui modul migrasi tagihan lama (`EPIC FIN-24`).
3. **Persetujuan dan Penguncian:**
   - Manajer Keuangan / Kepala Bagian Akuntansi memeriksa kesesuaian angka terhadap kertas kerja akuntansi, lalu memberikan persetujuan (`APPROVED`).
   - Setelah diverifikasi final, Manajer mengunci saldo awal (`LOCKED`).
   - Sistem secara otomatis mencatat tepat satu transaksi mutasi kas masuk (`SALDO-AWAL`, arah `IN`) pada buku mutasi kas bertanggal `2026-10-01` untuk kelompok `KAS-KASIR`.

### 2.2 Siklus Hidup Status Saldo Awal

```text
[Belum Ada] ──► POST /opening-balances ──► [DRAFT]
                                             │
                       PUT /opening-balances │ (Koreksi data DRAFT)
                                             ▼
                                      [DRAFT Valid]
                                             │
                      POST /{id}/approve     │ (Persetujuan Manajer Keuangan)
                                             ▼
                                        [APPROVED]
                                             │
                      POST /{id}/lock        │ (Penguncian Permanen + Terbit Mutasi Kas)
                                             ▼
                                         [LOCKED] (Permanen, Tidak Dapat Diubah)
```

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

1. `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`
2. `docs/module-blueprints/finance-management/02-backend-architecture.md` (§FIN-DES-088)
3. `docs/module-blueprints/finance-management/contracts/api-contract.md` (§F.1)
4. `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` (§F.1)
5. `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (§F.3, FIN-VAL-180..185)
6. `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinOpeningBalance.cs`
7. `Areas/Corporate/FinanceManagement/CashManagement/Models/FinCashMovement.cs`
8. `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs`

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/SubledgerOpeningBalanceDtos.cs` | DTO representasi saldo awal (`OpeningBalanceResponse`), pembuatan (`CreateOpeningBalanceRequest`), pembaruan (`UpdateOpeningBalanceRequest`), persetujuan (`ApproveOpeningBalanceRequest`), dan penguncian (`LockOpeningBalanceRequest`) dengan token konkurensi `RowVersion`. |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningBalanceService.cs` | Layanan logika bisnis untuk pencatatan, koreksi, persetujuan, dan penguncian permanen saldo awal, penegakan aturan validasi `FIN-VAL-180`..`185`, serta orkestrasi mutasi kas otomatis pada kelompok `KAS-KASIR`. |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceSubledgerSetupController.cs` | Penambahan 5 endpoint operasional REST API untuk saldo awal subledger di bawah base URL `/subledger-setup` lengkap dengan integrasi hak akses RBAC. |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Pendaftaran dependensi `FinanceOpeningBalanceService` ke service collection ASP.NET Core (`AddScoped`). |

### 3.3 Penegakan Aturan Validasi Bisnis

| Kode Aturan | Kondisi yang Diuji | Respon Sistem |
| :--- | :--- | :--- |
| `FIN-VAL-180` | Kelompok saldo yang sama sudah memiliki baris aktif | HTTP 409 — *"Saldo awal untuk kelompok ini sudah pernah dicatat."* |
| `FIN-VAL-181` | Kelompok `PIUTANG`, `UTANG-SUPPLIER`, atau `UTANG-JASA-MEDIS` bernilai selain 0.00 | HTTP 422 — *"Saldo awal kelompok ini harus nol karena rinciannya datang dari migrasi tagihan lama."* |
| `FIN-VAL-182` | Ruas `Reason` atau `AccountingReferenceDocument` kosong/spasi | HTTP 422 — *"Alasan dan rujukan dokumen saldo awal wajib diisi."* |
| `FIN-VAL-183` | Mengubah baris yang sudah berstatus `APPROVED` atau `LOCKED` | HTTP 409 — *"Saldo awal yang sudah disetujui tidak dapat diubah."* |
| `FIN-VAL-184` | Tanggal cutover melebihi hari ini dalam kalender bisnis WIB saat dikunci | HTTP 422 — *"Tanggal cutover tidak boleh melewati hari ini."* |
| `FIN-VAL-185` | Saldo awal kas bernilai negatif (`Amount < 0`) | HTTP 422 — *"Saldo awal kas tidak boleh negatif."* |

---

## 4. Dokumentasi Endpoint API

Tag Grup: `[Tags("Corporate / Finance Management / Subledger Setup")]`  
Base URL: `/api/v1/corporate/finance-management/subledger-setup`

| Method | Path | Deskripsi | Hak Akses | Request Body | Response Body | Status HTTP |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/opening-balances` | Mengambil seluruh daftar saldo awal cutover aktif per kelompok saldo | `FinanceSubledgerSetup : Read` | — | `ApiResponse<List<OpeningBalanceResponse>>` | `200 OK` |
| `POST` | `/opening-balances` | Mencatat saldo awal satu kelompok saldo dalam status awal `DRAFT` | `FinanceSubledgerSetup : Create` | `CreateOpeningBalanceRequest` (JSON) | `ApiResponse<OpeningBalanceResponse>` | `201 Created`<br>`400 Bad Request`<br>`409 Conflict`<br>`422 Unprocessable` |
| `PUT` | `/opening-balances/{id}` | Mengoreksi nominal, tanggal cutover, alasan, atau dokumen rujukan saldo awal yang masih `DRAFT` | `FinanceSubledgerSetup : Update` | `UpdateOpeningBalanceRequest` (JSON) | `ApiResponse<OpeningBalanceResponse>` | `200 OK`<br>`400 Bad Request`<br>`404 Not Found`<br>`409 Conflict`<br>`422 Unprocessable` |
| `POST` | `/opening-balances/{id}/approve` | Menyetujui saldo awal cutover (`DRAFT` $\rightarrow$ `APPROVED`) oleh pejabat berwenang | `FinanceSubledgerSetup : Approve` | `ApproveOpeningBalanceRequest` (JSON) | `ApiResponse<OpeningBalanceResponse>` | `200 OK`<br>`404 Not Found`<br>`409 Conflict`<br>`422 Unprocessable` |
| `POST` | `/opening-balances/{id}/lock` | Mengunci permanen saldo awal (`APPROVED` $\rightarrow$ `LOCKED`); menerbitkan mutasi kas masuk untuk kas kasir | `FinanceSubledgerSetup : Approve` | `LockOpeningBalanceRequest` (JSON) | `ApiResponse<OpeningBalanceResponse>` | `200 OK`<br>`404 Not Found`<br>`409 Conflict`<br>`422 Unprocessable` |

---

## 5. Verifikasi dan Bukti Kepatuhan

1. **Integritas Konkurensi & Status:**
   - Kolom `RowVersion` divalidasi pada setiap mutasi status dan pembaruan data untuk mencegah modifikasi tumpang-tindih.
   - Baris berstatus `LOCKED` diproteksi secara absolut di tingkat service: tidak ada jalur yang mengizinkan pembaruan maupun penarikan status dari `LOCKED`.
2. **Penerbitan Mutasi Kas Otomatis:**
   - Penguncian kelompok `KAS-KASIR` dibuktikan secara transaksional memanggil `FinanceSubledgerMovementService.RecordCashMovementAsync` dengan jenis mutasi `SALDO-AWAL`, arah `IN`, dan rujukan `OPENING_BALANCE` bertanggal `CutoverDate`.
3. **Kepatuhan Batasan Task:**
   - **Nol migrasi baru:** Task ini menggunakan skema tabel `FinOpeningBalance` yang telah dibuat pada task `BE-FIN-064`.
   - **Nol build otomatis:** Tidak ada perintah `dotnet build` otomatis yang dijalankan sesuai instruksi pengguna.
