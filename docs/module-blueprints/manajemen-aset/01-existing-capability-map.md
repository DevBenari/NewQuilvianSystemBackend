# Manajemen Aset & Inventaris — Peta Kemampuan Existing

| Field | Value |
|---|---|
| Blueprint ID | `AST-BP-001` |
| Revision | `1` |
| Map revision | `1.0` |
| Status | `CURRENT` terhadap kedua SHA di bawah |
| Masukan | `00-interview-decisions.md` decision revision `1` (`AST-DEC-001..032`) |
| Backend SHA | `1f54a17551ac49a1d12f3186ce982c1e8107c91e` (branch `rizkiG`) |
| Frontend SHA | `e49e8a8158c652f4405588c8a787f9355baea690` (branch `RizkiV2`) |
| Diaudit | 2026-11-12, `AUDIT` read-only; hanya berkas ini yang ditulis (`MODULE BLUEPRINT MODE`) |

Penulisan bukti: `BE:` = `NewQuilvianSystemBackend`, `FE:` = `QuilvianSystemFrontendDev`, keduanya pada SHA di atas.

---

## 1. Ringkasan dalam lima kalimat

1. **Modul aset belum ada sama sekali**: nol model, nol endpoint, nol halaman. Karena itu seluruh 62
   kemampuan berstatus `Missing`, kecuali empat yang punya catatan khusus (bagian 4).
2. **Hampir semua yang dibutuhkan di sekelilingnya sudah ada**: unit organisasi, pegawai, rantai
   pengguna → unit, Number Series, jalur kejadian Akuntansi, rekonsiliasi control account, pustaka
   Excel/QR/PDF di backend, dan komponen dasar di frontend.
3. **Satu konflik nyata**: format nomor inventaris yang disetujui (`AST/ALK-MED/2026/000123`) tidak
   bisa dibuat oleh alokator Number Series bersama, yang menghasilkan `{awalan}-{urut}` dan melarang
   deret baru direset tahunan (`AST-CON-002`).
4. **Dua hal belum bisa dipastikan dari source code**: master lokasi mana yang dipakai untuk ruangan
   non-klinis (`AST-UNK-001`), dan apakah Akuntansi sudah menjurnal penyusutan lewat jurnal berulang
   di database (`AST-UNK-002`, kelanjutan `AST-CON-001`).
5. **Persetujuan dibangun di dalam modul sendiri**, mengikuti pola maker-checker Finance, bukan
   memakai mesin workflow HR yang belum pernah dipakai modul di luar HR.

---

## 2. Batas audit

| Diperiksa | Tidak diperiksa |
|---|---|
| Model, konfigurasi, controller, service, dan konstanta di `Areas/Corporate/{HumanResource, FinanceManagement, AccountingManagement}`, `Areas/HealthServices/MasterData`, `Areas/Platform/NumberSeriesManagement`, `Attributes/`, `Constants/`, `Models/IdentityModel.cs`, `Controllers/AuthController.cs`, `QuilvianSystemBackend.csproj` | Isi database (master unit, ruangan, penugasan manajerial, template jurnal berulang yang sudah dibuat pengguna) |
| `FE: package.json`, `src/utils/menu-sidebar/menu-items.jsx`, `src/components/features/base-features/`, pemakaian pustaka QR/kamera | Perilaku runtime, test otomatis, lingkungan staging |
| Decision log Akuntansi (`accounting/00-interview-decisions.md`) untuk aturan penerimaan kejadian | Seluruh isi blueprint modul lain |

Klaster yang diaudit: Identity/Master Owner, Actor/Workforce, Location/Resource, Workflow/Status,
Documentation/Record, Financial, Authorization/Audit. Klaster Order/Result dan External Integration
tidak relevan untuk modul ini, kecuali GR Finance yang bisa datang dari sistem luar (BLM).

---

## 3. Titik sentuh — kemampuan existing yang dipakai modul aset

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `TS-01` | Daftar unit (IGD, ICU, Keuangan, Gudang, …) sebagai pemilik/penempatan aset | HR MasterData (`Mst`) | `BE: Areas/Corporate/HumanResource/MasterData/Organization/Models/MstOrganizationUnit.cs#MstOrganizationUnit` (`UnitCode`, `UnitName`, `ParentOrganizationUnitId`, `LegalEntityId`, `HospitalSiteId`, `CostCenters`, `WorkLocations`, `IsActive`) | Reuse with adapter | Aset hanya **membaca**; butuh query/lookup dari sisi Aset | Unit klinis juga punya `MstServiceUnit` dengan `OrganizationUnitId` opsional — dua daftar unit bisa tidak sama (`AST-UNK-003`) |
| `TS-02` | Ruangan tempat aset berada, termasuk ruangan non-klinis (kantor, gudang, dapur) | Belum jelas | `BE: Areas/HealthServices/MasterData/Models/MstRoom.cs#MstRoom` wajib `ServiceUnitId`, `RoomType` hanya jenis klinis (`RoomType.cs`: Outpatient…Operating, `Other=99`). `BE: .../Organization/Models/MstWorkLocation.cs#MstWorkLocation` memuat `BuildingName`, `FloorName`, `RoomName` sebagai teks bebas | Unknown | Tiga kemungkinan sumber: `MstRoom`, `MstWorkLocation`, atau master lokasi milik Aset | Memilih tanpa melihat isi data bisa membuat ruangan non-klinis tidak bisa dipilih (`AST-UNK-001`) |
| `TS-03` | Pegawai penanggung jawab aset | HR (`Mst`/`Wfp`) | `BE: .../Workforce/Models/MstEmployee.cs#MstEmployee` (`EmployeeNumber`, `FullName`, `PrimaryDepartmentId`); `MstWorkforceProfile.cs` (`Employee`, `UserAccount`, `OrganizationAssignments`) | Reuse with adapter | Lookup pegawai aktif dari sisi Aset | — |
| `TS-04` | Menentukan "unit saya" dan "Kepala Unit" dari pengguna yang masuk | HR WorkforceCore (`Wfp`) | `BE: Models/ApplicationUser.cs#WorkforceProfileId`; `BE: Areas/Corporate/HumanResource/WorkforceCore/Models/WfpOrganizationAssignment.cs` (`OrganizationUnitId`, `IsPrimary`, `IsManagerialAssignment`, `EffectiveStartDate/EndDate`, `IsActive`) | Reuse with adapter | Aset butuh service baca: pengguna → profil → penugasan aktif → unit; penugasan manajerial → Kepala Unit | Bila penugasan manajerial belum diisi HR, tidak ada yang terbaca sebagai Kepala Unit (`AST-UNK-004`) |
| `TS-05` | Unit pengguna dari token login | Platform Auth | `BE: Controllers/AuthController.cs` baris 2071–2087: klaim `department_id`, `position_id`, **tanpa** unit organisasi | Missing | Jangan mengandalkan klaim; pakai `TS-04` | Temuan di luar cakupan: `HrdClinicalPreceptorshipService.GetUserUnitId` membaca klaim `OrganizationUnitId` yang tidak pernah diterbitkan (bagian 7) |
| `TS-06` | Nomor inventaris unik dan tidak bentrok | Platform (`Num`) | `BE: Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs#AllocateAsync`; `NumberAllocationRequest.cs` (`SequenceKey` milik modul, `Prefix` maks 15 karakter, `ResetPolicy` deret baru `NEVER` per `DEC-PLT-004`); format hasil `{prefix}-{sequence}` (baris 108–113) | Conflict | Format putusan `AST-RULE-003` memakai `/`, menyisipkan tahun perolehan, dan menyiratkan urut per tahun | `AST-CON-002`; memakai cara lain melanggar `QBE-CODE-006` (alokator adalah satu-satunya cara sah) |
| `TS-07` | Menautkan PO/GR Finance sebagai bukti perolehan | Finance Purchasing (`Fin`) | `BE: Areas/Corporate/FinanceManagement/Purchasing/Models/FinGoodsReceipt.cs` (`GRNumber`, `PurchaseOrderId`, `ReceivedDate`, `Status`, `SourceSystem`); controller `[Tags("Corporate / Finance Management / Purchasing / Goods Receipt")]`, `GET` dengan `FinanceGoodsReceipt : Read` | Reuse with adapter | Aset menyimpan `Id` dan nomor GR sebagai rujukan; pencarian GR lewat endpoint Finance. Tidak ada penanda "barang modal" pada GR; harga ada di item PO | Pengguna Aset butuh izin `FinanceGoodsReceipt : Read`, atau Aset menyediakan lookup baca sendiri |
| `TS-08` | Mengirim kejadian keuangan ke Akuntansi | Akuntansi (`Acc`) | `BE: .../AccountingManagement/AccountingEvent/Controllers/AccountingEventController.cs` `POST api/v1/corporate/accounting/accounting-events` (`AccountingEvent : Receive`); `AccAccountingEvent.cs` (`EventTypeCode`, `SourceModule`, `SourceTransactionId`, `SourceVersion`, `AccountingDate`, `DocumentDate`, `Amount`, `Components`); pengirim existing: `BE: .../FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` + `FinanceAccountingDispatchWorker.cs` (outbox, HTTP, bawaan mati, coba ulang) | Reuse with adapter | Aset butuh outbox dan worker pengirim sendiri, mengikuti pola Finance; jenis kejadian Aset didaftarkan di `AccEventType` (`SourceModule`) dan aturan posting di `AccPostingRule` | Aturan penerimaan sudah diputuskan Akuntansi: tanpa pemetaan → daftar gagal (`ACC-DEC-046`); periode tertutup → periode terbuka berikutnya (`ACC-DEC-047`); coba ulang 3 kali (`ACC-DEC-049`) |
| `TS-09` | Rekonsiliasi saldo aset dengan buku besar | Akuntansi (`Acc`) | `BE: .../Reconciliation/Models/AccSubledgerBalance.cs` (`AccountingPeriodId`, `ChartOfAccountId`, `Balance`, `AsOfDate`, `AccountingEventId`); `EventTypeKind.SaldoSubledger = 2`; `AccControlAccountReconciliationService.cs` (sisi buku besar, `ACC-DEC-066`); pengirim saldo Finance `FinanceSubledgerSnapshotService.cs` | Extend | Aset mengirim saldo subledger per akun kontrol (harga perolehan, akumulasi penyusutan) sebagai kejadian berjenis saldo; Akuntansi membandingkannya | `ACC-DEC-061` melarang Akuntansi membaca tabel modul lain, sehingga saldo **harus** dikirim Aset, bukan dibaca Akuntansi |
| `TS-10` | Mencegah penyusutan tercatat dua kali | Akuntansi (`Acc`) | `BE: .../RecurringJournal/Models/AccRecurringJournalTemplate.cs` baris 11 menyebut penyusutan bulanan sebagai contoh; pencarian `penyusutan` di seeder dan `SeedData/` nol hasil | Unknown | Template jurnal berulang dibuat pengguna di database; source tidak bisa menjawab apakah template penyusutan sudah ada | `AST-UNK-002` (kelanjutan `AST-CON-001`) |
| `TS-11` | Butir daftar periksa tutup buku "Penyusutan belum dijalankan" | Akuntansi (`Acc`) | `BE: .../AccountingPeriod/Services/AccPeriodClosingService.cs` baris 83 (`DEPRECIATION_NOT_RUN`) dan 183–188 (`ButirBelumTersedia`, `menahan: false`) | Extend | Butir itu bisa diisi dari status penyusutan bulanan Aset. Perubahannya berada di modul Akuntansi, bukan Aset | Bukan prasyarat MVP Aset |
| `TS-12` | Badan hukum pemilik aset | HR MasterData | `MstLegalEntity` dirujuk `MstOrganizationUnit.LegalEntityId` dan `AccAccountingEvent.LegalEntityId` (wajib) | Reuse with adapter | Setiap kejadian ke Akuntansi wajib membawa `LegalEntityId`, sehingga aset harus tahu badan hukum pemiliknya | Bila RS punya lebih dari satu badan hukum/site, data aset harus memisahkannya (`AST-UNK-005`) |
| `TS-13` | Pusat biaya pembebanan penyusutan | HR MasterData | `MstCostCenter.cs` (`CostCenterCode`, `AccountingCode`, `OrganizationUnitId`) | Unknown | Belum diputuskan apakah beban penyusutan dipisah per unit/pusat biaya | `AST-UNK-006` |
| `TS-14` | Persetujuan berjenjang dan larangan menyetujui sendiri | Finance (pola) / HR Workflow (`Wfl`) | Pola Finance: `FinPayment.cs` check constraint `CK_FinPayment_MakerChecker`; `FinanceApprovalAuthorizationService.cs` (jenjang nominal); `FinancePaymentService.cs` baris 437–441. Mesin HR: `TrxWorkflowInstance.cs` (berpusat pada `RequestedByWorkforceProfileId`), tanpa konsumen di luar `Areas/Corporate/HumanResource` | Missing | Persetujuan Aset dibangun di dalam modul mengikuti pola Finance | Memakai mesin HR berarti menjadi konsumen pertama di luar HR tanpa bukti kesesuaian |
| `TS-15` | Lampiran: foto kondisi, berita acara, dokumen kepemilikan | Per modul | Pola Finance `BE: .../Collection/Services/FinanceTransactionProofService.cs` (ekstensi dan ukuran dari konfigurasi, *fail-closed* bila belum diatur: 503/413/400, akar `uploads`); tidak ada service penyimpanan bersama | Missing | Aset membuat service lampiran sendiri mengikuti pola tersebut | — |
| `TS-16` | Impor Excel data awal | Finance (pola) | `BE: QuilvianSystemBackend.csproj` `ClosedXML 0.105.1`; `FinanceOpeningItemBatchService.cs` (unggah → validasi per baris → persetujuan; batch `VALIDATED` hanya bila **nol** baris salah, baris 340–341; batas baris per berkas) | Missing | Impor aset dibangun mengikuti pola batch Finance | Pola Finance menolak seluruh batch bila ada satu baris salah; ini berbeda dari kesan opsi wawancara (`AST-OQ-014`) |
| `TS-17` | Membuat QR dan mencetak label/berita acara | Bersama | `BE: csproj` `QRCoder 1.8.0`, `QuestPDF 2026.7.1` (dipakai `PatientController.cs`, laporan Finance); `FE: package.json` `qrcode`, `qrcode.react`, `react-qr-code`, `react-to-print`, `@react-pdf/renderer`; contoh cetak `FE: src/components/view/health-services/inpatient-management/admission-workspace/sections/identity-labels/wristband-print.jsx` | Reuse with adapter | Templat label dan berita acara Aset dibuat baru dengan pustaka yang ada | — |
| `TS-18` | Memindai QR lewat kamera HP/tablet | Belum ada | `FE: package.json` `react-qr-scanner ^1.0.0-alpha.11` dan `react-webcam` terpasang tetapi **tidak dipakai** di `src/`; akses kamera (`getUserMedia`) hanya untuk foto (`UploadPhotoField.jsx`, `identity-image-capture.jsx`) | Extend | Perlu komponen pemindai QR; pustaka terpasang masih versi alpha | Pemilihan pustaka adalah keputusan teknis desain; akses kamera butuh HTTPS |
| `TS-19` | Hak akses per endpoint dan hak lihat rupiah | Platform Auth | `BE: Attributes/AccessControllerAttribute.cs`, `AccessActionAttribute.cs`, `AccessPermissionAttribute.cs`, `AccessExplicitPermissionAttribute.cs`; `Constants/AccessTypes.cs`; pola `ViewAmount` + endpoint `/amounts`: `BE: Areas/Corporate/PayrollManagement/Controllers/PayrollCompensationsController.cs` baris 42, 68–69 | Ready to reuse | — | `[AccessAction]` tanpa `[AccessPermission]` gagal pada verifier CI |
| `TS-20` | Jejak siapa membuat/mengubah | Bersama | `BE: Models/IdentityModel.cs` (`CreateBy/DateTime`, `UpdateBy`, `DeleteBy`, `CancelBy`, `IsDelete`, `IsCancel`) | Ready to reuse | Riwayat status dan riwayat lokasi tetap butuh tabel milik Aset | — |
| `TS-21` | Pekerjaan terjadwal (penyusutan bulanan, penanda Terlambat, kirim kejadian) | Bersama (pola) | `BE: AccRecurringJournalSchedulerHostedService.cs`, `FinanceAccountingDispatchWorker.cs` (`BackgroundService`, bawaan mati lewat konfigurasi) | Reuse with adapter | Worker Aset dibuat mengikuti pola itu | — |
| `TS-22` | Grup menu sidebar | Frontend | `FE: src/utils/menu-sidebar/menu-items.jsx` grup tingkat atas `Akuntansi` (`key: corporateAccounting`, baris 1746) dan `Finance` (baris 1845) | Extend | Tambah grup "Manajemen Aset" dengan 11 butir | Letaknya `AST-OQ-015` |
| `TS-23` | Komponen dasar layar | Frontend | `FE: src/components/features/base-features/` — `data-table.jsx`, `data-filter.jsx`, `pagination.jsx`, `status-badge.jsx`, `summary-grid.jsx`, `row-action-menu.jsx`, `confirm-modal.jsx`, `access-denied-gate.jsx`, `base-detail-view.jsx`, `base-editor-form.jsx`, `base-letterhead.jsx`, `resource-filter-select.jsx` | Ready to reuse | — | — |
| `TS-24` | Grafik dashboard | Frontend | `FE: package.json` `apexcharts`, `chart.js`, `recharts`; contoh beranda `FE: src/app/corporate/accounting/page.jsx` | Ready to reuse | — | — |
| `TS-25` | Aset yang dipegang pegawai saat keluar | HR Lifecycle | `BE: .../LifecycleManagement/Models/TrxAssetReturn.cs` (`AssetCode`, `AssetName` teks bebas); dipakai hanya lewat `TrxExitClearance.AssetReturns` dan `ApplicationDbContext`; tidak ada controller/service lain yang memakainya | Unknown | Kelak dapat ditautkan ke register (`AST-OQ-012`) | Dua sumber "aset dipegang siapa" bila tidak ditautkan |

---

## 4. Status 62 kemampuan modul

Karena modul aset belum ada, setiap kemampuan `Missing` kecuali yang disebut khusus. Kolom
"Bergantung" menunjuk titik sentuh bagian 3 yang harus dipakai, bukan dibuat ulang.

### 4.1 `register` (43 kemampuan)

| ID | Kemampuan | Status | Bergantung |
|---|---|---|---|
| `CAP-01.1`, `01.2`, `01.3`, `01.6` | Dashboard: ringkasan, per unit, kondisi, aset rusak | Missing | `TS-01`, `TS-04`, `TS-23`, `TS-24` |
| `CAP-02.1`–`02.4`, `02.6` | Master kategori, kelompok, kode inventaris, jenis kepemilikan, klasifikasi medis/non-medis | Missing | `TS-19`, `TS-23` |
| `CAP-02.5` | Master lokasi | **Unknown** | `TS-01`, `TS-02` — `AST-UNK-001` |
| `CAP-03.1`, `03.2` | Pencatatan aset baru, data perolehan | Missing | `TS-07`, `TS-12` |
| `CAP-03.3` | Nomor inventaris | **Conflict** | `TS-06` — `AST-CON-002` |
| `CAP-03.4` | Label QR | Missing (pustaka siap) | `TS-17` |
| `CAP-03.5`, `08.2` | Dokumen kepemilikan | Missing | `TS-15` |
| `CAP-03.6`, `11.1` | Daftar inventaris, daftar per unit | Missing | `TS-04`, `TS-19` |
| `CAP-04.1`–`04.4` | Penempatan, penanggung jawab, berita acara penyerahan, riwayat lokasi | Missing | `TS-01`, `TS-02`, `TS-03`, `TS-17` |
| `CAP-06.1`–`06.6`, `11.3` | Pemeriksaan kondisi, status, riwayat status, laporan kondisi | Missing | `TS-15`, `TS-20` |
| `CAP-08.1`, `08.3`–`08.5` | Permohonan pemanfaatan, pengamanan, aset menganggur, kewenangan | Missing | `TS-14`, `TS-15` |
| `CAP-11.7` | History & Audit | Missing | `TS-20` |
| Impor data awal (`AST-DEC-023`) | Missing | `TS-16` |

### 4.2 `pergerakan` (7 kemampuan)

| ID | Kemampuan | Status | Bergantung |
|---|---|---|---|
| `CAP-01.4`, `05.1`–`05.5`, `11.2` | Mutasi, persetujuan, serah terima, peminjaman, pengembalian, laporan | Missing | `TS-04` (Kepala Unit), `TS-14`, `TS-17` (berita acara), `TS-21` (penanda Terlambat) |

### 4.3 `inventarisasi` (8 kemampuan)

| ID | Kemampuan | Status | Bergantung |
|---|---|---|---|
| `CAP-01.5`, `07.1`, `07.3`–`07.6`, `11.4` | Jadwal, pencocokan, selisih, verifikasi, berita acara, laporan | Missing | `TS-14`, `TS-17` |
| `CAP-07.2` | Pemeriksaan fisik dengan pindai QR | Missing | `TS-18` (Extend) |

### 4.4 `nilai` (6 kemampuan)

| ID | Kemampuan | Status | Bergantung |
|---|---|---|---|
| `CAP-09.1`–`09.3`, `11.6` | Harga perolehan, nilai buku, riwayat perubahan nilai, laporan nilai | Missing | `TS-14` |
| `CAP-09.4` | Data penyusutan | Missing; **tertahan** `AST-UNK-002` | `TS-08`, `TS-10`, `TS-13`, `TS-21` |
| `CAP-09.5` | Rekonsiliasi dengan Akuntansi | **Extend** | `TS-09` |

### 4.5 `penghapusan` (7 kemampuan)

| ID | Kemampuan | Status | Bergantung |
|---|---|---|---|
| `CAP-01.6`* , `10.1`–`10.6`, `11.5` | Usulan, kelayakan, persetujuan, pemindahtanganan, berita acara, riwayat, laporan | Missing | `TS-08`, `TS-14`, `TS-15`, `TS-17` |

\* `CAP-01.6` (kartu Aset Rusak) dimiliki `register`; `penghapusan` hanya memakai angkanya sebagai
sumber usulan.

---

## 5. Kontrak as-is yang akan dipakai

Hanya endpoint yang **sudah ada** dan akan dikonsumsi modul aset. Endpoint modul aset sendiri dirancang
pada `contracts/api-contract.md` tiap sub-modul.

### Corporate - Accounting - Accounting Event

Base URL: `api/v1/corporate/accounting/accounting-events`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/` | Menerima satu kejadian keuangan dari modul lain | `AccountingEvent : Receive` | Kejadian: `EventTypeCode`, `SourceModule`, `SourceTransactionId`, `SourceVersion`, `AccountingDate`, `DocumentDate`, `Amount`, `Components[]` | `AccountingEventReceiptDto` |
| `GET` | `/` | Daftar kejadian dan statusnya | `AccountingEvent : Read` | query daftar | `PagedResult<AccountingEventListDto>` |
| `GET` | `/{id}` | Rincian satu kejadian | `AccountingEvent : Read` | - | `AccountingEventDetailDto` |
| `POST` | `/{id}/retry` | Mencoba ulang kejadian gagal | `AccountingEvent : Retry` | - | `AccountingEventDetailDto` |

Arti bagi pengguna: kejadian penyusutan untuk kelompok yang belum dipetakan ke akun **tidak** menjadi
jurnal dan masuk daftar gagal di Akuntansi (`ACC-DEC-046`), sehingga laporan nilai aset dan buku besar
bisa berbeda sampai pemetaannya dilengkapi.

### Corporate / Finance Management / Purchasing / Goods Receipt

Base URL: `api/v1/corporate/finance-management/purchasing/goods-receipts`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Mencari GR untuk ditautkan ke aset | `FinanceGoodsReceipt : Read` | query daftar | daftar GR |
| `GET` | `/{id}` | Rincian GR (nomor, tanggal terima, item) | `FinanceGoodsReceipt : Read` | - | rincian GR |

Kode 403 di sini berarti petugas aset belum diberi izin membaca GR Finance; tautan GR tetap opsional
(`AST-RULE-005`), sehingga pencatatan aset tidak terhenti.

### Number Series (in-process, tanpa endpoint)

`NumberSeriesAllocator.AllocateAsync(NumberAllocationRequest)` — dipanggil langsung oleh service
modul. Nomor yang terbit lalu batal dipakai **hangus** dan tidak diisi ulang (`INV-PLT-002`).
Contoh: alokasi ke-123 untuk aset yang validasinya gagal → aset berikutnya mendapat ke-124.

---

## 6. Conflict dan Unknown

> **Sesudah closure pass (decision revision `2`):** `AST-CON-002` ditutup `AST-DEC-033`;
> `AST-CON-001` ditutup `AST-DEC-038`; `AST-UNK-001` dan `003` ditutup `AST-DEC-034`;
> `AST-UNK-005` ditutup `AST-DEC-035`; `AST-UNK-006` ditutup `AST-DEC-036`. Yang tetap terbuka:
> `AST-UNK-002` dan `AST-UNK-004` (keduanya data di database, menahan go-live, bukan desain).
> Status kemampuan pada bagian 3–4 adalah status bukti saat audit dan sengaja tidak diubah.

| ID | Jenis | Uraian | Bukti | Owner | Menahan |
|---|---|---|---|---|---|
| `AST-CON-002` | Conflict | Format nomor `AST/ALK-MED/2026/000123` (pemisah `/`, tahun perolehan, urut per kelompok per tahun) tidak dapat dihasilkan alokator bersama: hasilnya `{prefix}-{urut}`, awalan maks 15 karakter, deret baru `NEVER`. Contoh yang bisa dibuat: `AST-ALKMED-000123` (deret per kelompok, tidak pernah direset) | `TS-06` | Rizki (+ pemilik platform bila format harus diubah) | Desain `register` (`CAP-03.3`) |
| `AST-CON-001` | Conflict | Kemungkinan penyusutan tercatat dua kali (jurnal berulang + kejadian Aset) | `TS-10`; decision log | Rizki (Akuntansi) | Desain `nilai` |
| `AST-UNK-001` | Unknown | Sumber ruangan untuk lokasi aset: `MstRoom` (klinis saja), `MstWorkLocation` (teks bebas), atau master lokasi milik Aset | `TS-02` | Rizki | Desain `register` (`CAP-02.5`) |
| `AST-UNK-002` | Unknown | Apakah sudah ada template jurnal berulang penyusutan di database | `TS-10` | Rizki (Akuntansi) | Implementasi `nilai` |
| `AST-UNK-003` | Unknown | `MstServiceUnit` dan `MstOrganizationUnit` adalah dua daftar unit; unit klinis belum tentu tertaut | `TS-01` | Rizki | Desain `register` |
| `AST-UNK-004` | Unknown | Apakah penugasan manajerial (`IsManagerialAssignment`) sudah diisi HR untuk setiap Kepala Unit | `TS-04` | Rizki + HR | Implementasi `pergerakan` (persetujuan unit asal) |
| `AST-UNK-005` | Unknown | Jumlah badan hukum/site RS: satu atau lebih | `TS-12` | Rizki | Desain `register`, `nilai` |
| `AST-UNK-006` | Unknown | Apakah beban penyusutan dibebankan per unit/pusat biaya | `TS-13` | Rizki (Akuntansi) | Desain `nilai` |

---

## 7. Temuan di luar cakupan (dilaporkan, tidak diperbaiki)

| ID | Temuan | Bukti | Pemilik |
|---|---|---|---|
| `AST-OOS-F01` | `HrdClinicalPreceptorshipService.GetUserUnitId` membaca klaim `OrganizationUnitId`/`UnitId`/`unit_id`, padahal `AuthController` tidak pernah menerbitkannya. Penyaringan per unit di layar preseptorship klinis kemungkinan selalu kosong | `BE: Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdClinicalPreceptorshipService.cs` baris 48–54; `BE: Controllers/AuthController.cs` baris 2071–2087 | Pemilik HR |

---

## 8. Pertanyaan penutup untuk pemilik (closure pass)

1. `AST-CON-002` — format nomor inventaris yang sesuai alokator bersama.
2. `AST-UNK-001` dan `AST-UNK-003` — sumber unit dan ruangan untuk lokasi aset.
3. `AST-UNK-005` — satu atau lebih badan hukum/site.
4. `AST-UNK-006` — pembebanan penyusutan per unit/pusat biaya.
5. `AST-OQ-014` — impor data awal semua-atau-tidak-sama-sekali (pola Finance) atau sebagian.
6. `AST-CON-001`/`AST-UNK-002` — perlakuan jurnal berulang penyusutan yang mungkin sudah ada.

## 9. Pemicu impact scan

Peta ini harus ditandai `STALE` dan dipindai ulang secara terbatas bila salah satu berubah sesudah
SHA di atas: `Areas/Platform/NumberSeriesManagement/**`, `Areas/Corporate/AccountingManagement/{AccountingEvent,MasterData/EventType,MasterData/PostingRule,Reconciliation,RecurringJournal}/**`,
`Areas/Corporate/FinanceManagement/{Purchasing,AccountingIntegration}/**`,
`Areas/Corporate/HumanResource/{MasterData/Organization,WorkforceCore}/**`, `Areas/HealthServices/MasterData/Models/{MstRoom,MstServiceUnit}.cs`,
`Attributes/**`, `Controllers/AuthController.cs`, `FE: src/utils/menu-sidebar/menu-items.jsx`,
`FE: src/components/features/base-features/**`, atau `FE: package.json`.
