# Peta Kemampuan — Finishing Accounting (ACC-FIN-001..029)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `ACC-BP-001` |
| Dokumen | Peta kemampuan as-is untuk lingkup finishing. Read-only terhadap source |
| Skill | `trace-existing-capabilities` |
| Tanggal audit | 24 November 2026 |
| Masukan | MASTER PRD `ACC-FIN-PRD-001` (24 November 2026), ditempel Rizki di sesi chat; decision log Accounting revisi 18 (`ACC-DEC-001`..`140`); decision log Aset (`AST-DEC-001`..`051`) |
| Status modul sebelum audit | Development ✅ selesai sesuai lingkup yang disetujui (80 kartu, 30 September 2026); verdict audit kesiapan 29 September 2026 `NOT_READY` (kesiapan produksi, bukan kelengkapan development) |
| Backend SHA | `1f353c455585830168b2b1392e3cb2447608a031` (branch `rizkiG`) |
| Frontend SHA | `22894248d10a9971ea52b6bbddb83cac0748b7d4` (branch `RizkiV2`) |
| Engineering Skills SHA | `13ad293e5b68f73de22f4edf19eae413c4cf2081` (branch `main`) |
| SHA rujukan PRD | Backend `424d3f07` dan frontend `73691166` — **keduanya terbukti leluhur `HEAD`**, dan nol berkas Accounting yang di-commit berubah sejak itu |
| Kontrak yang berlaku | `ACC-API-0.9`, `ACC-STATE`, `ACC-VALIDATION`, `ACC-PERMISSION`, `ACC-INTEGRATION`, `ACC-XMOD-0.7` |
| Status dokumen | `DRAFT` — bahan untuk `grill-me` dan penyusunan roadmap finishing. **Bukan** arsitektur target dan **bukan** izin implementasi |

## 1. Mengapa dokumen ini ada dan apa batasnya

PRD finishing meminta 29 kebutuhan dikerjakan langsung. Aturan repository (`CLAUDE.md` di backend
dan frontend) mewajibkan setiap perubahan source lewat skill `build-module-backend` atau
`build-module-frontend`, dan hanya untuk task yang sudah disetujui di roadmap. Roadmap itu lahir dari
rangkaian `grill-me` → `trace-existing-capabilities` → `requirement-completeness-gate` →
`design-business-module` → `plan-module-delivery`.

Kebutuhan ACC-FIN belum punya roadmap. Karena itu langkah pertama yang sah adalah audit ini. Ia
menjawab satu pertanyaan untuk setiap kebutuhan: **apa yang sudah ada, dan apa yang benar-benar
kurang**, supaya kita tidak membangun ulang sesuatu yang sudah berjalan.

**Batas audit:**

- Backend: `Areas/Corporate/AccountingManagement` (77 berkas, ±17.000 baris), konfigurasi EF-nya di
  `Repositories/Configurations/Corporate/AccountingManagement`, serta titik sentuh di
  `AssetManagement`, `FinanceManagement`, `ProcurementManagement/Planning`, `PayrollManagement`,
  `HealthServices/MedicalFeeManagement`, dan `HealthServices/PharmacyManagement`.
- Frontend: `src/app/corporate/accounting` (31 rute), `src/components/view/corporate/accounting`,
  slice Redux, hook, dan konstanta Accounting.
- ClickUp **tidak dibaca**. Sesi ini tidak punya konektor ClickUp, jadi ID task ClickUp dikutip
  dari PRD apa adanya. Deskripsi dan subtask ClickUp belum diverifikasi (lihat bagian 6, `U-01`).
- Database tidak disentuh. Tidak ada build, test, atau migration yang dijalankan.

**Perubahan lokal yang ditemukan dan sengaja tidak disentuh:**

| Repo | Berkas | Milik |
| --- | --- | --- |
| Backend | 6 berkas termodifikasi + 6 berkas baru di `AssetManagement/Valuation` dan dokumen `manajemen-aset` | Pekerjaan `BE-AST-027` yang sedang ditahan Rizki |
| Frontend | `posting-rule-form-view.jsx`, `use-posting-rule-editor.jsx`, `posting-rule-form-view.module.css` (+847/−171 baris) | Perubahan layar aturan posting yang belum di-commit |

## 2. Arti status

Setiap kebutuhan diberi tepat satu status:

| Status | Arti dalam bahasa sederhana | Contoh |
| --- | --- | --- |
| Ready to reuse | Sudah ada dan sudah memenuhi kebutuhannya | Pencegahan kejadian ganda sudah dijaga database |
| Reuse with adapter | Sudah ada di modul lain; Accounting cukup menyambung | Penyusutan aset dihitung modul Aset, Accounting menerima jurnalnya |
| Extend | Fondasinya ada, perlu ditambah sebagian | Buku besar sudah ada, tetapi belum bisa disaring per unit biaya |
| Repair | Ada tetapi salah | — (tidak ditemukan pada audit ini) |
| Missing | Belum ada sama sekali | Laporan laba rugi |
| Conflict | Ada dua kemungkinan pemilik atau dua sumber data, dan harus diputuskan dulu | Anggaran sudah ada di Pengadaan, PRD meminta anggaran baru di Accounting |
| Unknown | Belum dapat dipastikan tanpa akses atau keputusan orang | Apakah rumah sakit wajib lapor SAKTI |

## 3. Ringkasan

| Status | Jumlah | Kebutuhan |
| --- | ---: | --- |
| Ready to reuse | 2 | 007, 022 |
| Reuse with adapter | 3 | 023, 024, 026 |
| Extend | 8 | 001, 004, 005, 006, 008, 027, 028, 029 |
| Missing | 6 | 002, 003, 015, 016, 018, 020 |
| Conflict | 6 | 009, 010, 011, 013, 014, 017 |
| Unknown | 4 | 012, 019, 021, 025 |

**Tiga temuan terbesar:**

1. **Fixed Asset (Wave 6) sudah dirancang dan sedang dibangun di modul Aset**, milik Rizki sendiri:
   `BE-AST-027` (penyusutan, 🟡 belum di-commit) sampai `BE-AST-037` (laporan penghapusan). Membangun
   ulang di Accounting akan menjadi duplikat. Bagian Accounting cukup menerima kejadiannya, dan
   mekanismenya sudah ada.
2. **Jasa dokter (Wave 3) mengalir ke Payroll, bukan ke Finance.** Serah terima Medical Fee
   (`MdfFinanceHandoff`) sudah ada, tetapi pembacanya `PayrollMedicalFeeIntakeService`. Tabel utang jasa
   medis milik Finance (`FinMedicalServicePayable`) tidak punya penulis, dan pembayaran
   `MEDICAL_SERVICE` ditolak di `FinancePaymentService`. Ini urusan kepemilikan antara Medical Fee,
   Finance, dan Payroll, bukan bug Accounting.
3. **Anggaran (Wave 4) sudah ada di Pengadaan.** `PrcBudget` menyimpan anggaran per tahun ×
   departemen × kategori barang × sumber dana, lengkap dengan versi, persetujuan lewat mesin workflow,
   dan pencatatan komitmen (`PrcBudgetCommitment`). Anggaran per akun × unit biaya di Accounting akan
   menjadi sumber anggaran kedua bila tidak diputuskan hubungannya.

## 4. Peta kemampuan

Format bukti: `path#simbol`. Seluruh path backend relatif terhadap root `NewQuilvianSystemBackend`;
path frontend relatif terhadap `QuilvianSystemFrontendDev/src`.

### Wave 1 — Master Data & Accounting Control

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| ACC-FIN-001 | Penyempurnaan bagan akun (COA) | Accounting | `AccChartOfAccountService#PeriksaIndukAsync` (lingkaran langsung dan tidak langsung), `#PeriksaTingkatTerhadapInduk` (tingkat 1–5), `#UpdateAsync` (induk ber-anak ditolak menerima transaksi, kode terkunci sesudah dipakai jurnal Posted), `#KodeKembar` (kode unik per badan hukum), `AccChartOfAccount#NormalBalance` (disimpan terpisah dari jenis akun), `#IsControlAccount`; FE `chart-of-accounts/*` | Extend | **Kurang dua:** (a) jenis akun anak tidak diperiksa terhadap induknya; (b) nilai jenis akun dan saldo normal `0` masih diterima (dicatat MODULE-STATUS sebagai catatan impor, bukan kartu, atas arahan 30 September). **Sudah benar:** jenis akun dan saldo normal memang tidak dapat diubah lewat `UpdateAsync` | Rendah. Butir (a) adalah aturan bisnis baru |
| ACC-FIN-002 | Kunci/buka grup COA per periode | Accounting | Tidak ada entity, endpoint, atau layar. Kunci yang ada hanya kunci periode (`AccAccountingPeriod` + syarat 9 di `AccJournalService#PeriksaSembilanSyaratAsync`) | Missing | Butuh tabel baru (kunci + riwayat). **Titik pasang penjaganya sudah tunggal:** semua jalur — manual, berulang (`AccRecurringJournalService` baris 673), tutup tahun (`AccYearEndClosingService` baris 244), kejadian (`AccAccountingEventService` baris 606–611), pembalikan — bermuara di `PeriksaSembilanSyaratAsync` | Sedang. Perlu definisi "grup" dan penanganan dua petugas yang mengunci dan mengesahkan bersamaan |
| ACC-FIN-003 | Cost/Profit Center dan alokasi biaya | HR Master Data (pusat biaya), Accounting (alokasi) | `AccJournalLine#CostCenterId`; `AccJournalService` baris 1204–1280 (wajib untuk akun beban, unit biaya harus ada); `MstCostCenter` (tanpa penanda pusat laba) | Missing | Pusat biaya di jurnal **sudah ada**. Yang kurang: konsep pusat laba (kolom di master milik HR atau pemetaan di Accounting), aturan alokasi, simulasi, jurnal alokasi, laporan per unit | Tinggi. Dasar alokasi wajib diputuskan pemilik proses |
| ACC-FIN-004 | Fondasi approval bertingkat | Accounting | `AccJournalService#ApproveAsync` (satu tingkat, larangan menyetujui jurnal sendiri `403`), `#RejectAsync` (alasan wajib), `#SubmitAsync` (ajukan ulang dari `Rejected`), `#PostAsync` (terpisah dari persetujuan), `AccJournalApproval` (riwayat). Kandidat reuse: `HumanResource/WorkflowManagement#TrxWorkflowInstance` + `MstWorkflowDefinition` (dipakai `PrcBudget#WorkflowInstanceId`), dan pola jenjang nominal `FinanceApprovalAuthorizationService` | Extend | Kurang: matriks per jenis transaksi dan nominal, jenjang berurutan, potret aturan saat diajukan. Perlu dipilih: memperluas `AccJournalApproval` atau menumpang mesin workflow HR | Tinggi. Menyentuh alur jurnal yang sudah berjalan |

### Wave 2 — Manual Jurnal & Laporan Keuangan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| ACC-FIN-005 | Penyempurnaan buku besar | Accounting | `AccGeneralLedgerService#GetMovementsAsync` (hanya jurnal Posted, saldo berjalan tetap benar lintas halaman, saring badan hukum/akun/tanggal), `#GetAccountBalanceAsync`; FE `general-ledger-view.jsx` | Extend | Kurang: (a) baris mutasi tidak membawa `JournalId`, jadi rincian jurnal tidak dapat dibuka langsung; (b) saldo awal dihitung tetapi **tidak dikirim** di respons; (c) saringan unit biaya; (d) rujukan transaksi sumber (asal kejadian); (e) ekspor CSV. Semua dapat ditambah tanpa tabel baru dan tanpa mematahkan kontrak | Rendah |
| ACC-FIN-006 | Penyempurnaan jurnal dan penyesuaian | Accounting | `JournalController` (CRUD, ajukan, setujui, tolak, sahkan, balik), `#ReverseAsync` (`FullReversal` → JB, `Adjustment` → JP), `RecurringJournalController` (template); FE `journals/*` (cari dan reset ada di daftar), `recurring-journals/*` | Extend | Kurang: impor `.xlsx`/`.csv` beserta pratinjau dan galat per baris, unduh format template, aksi "Simpan sebagai Jurnal & Template" sekaligus. **"Pilih Pasien" bertentangan** dengan `ACC-DEC-056` (kotak masuk tidak menyimpan pengenal pasien) — lihat `C-06` | Sedang. Impor wajib lewat validasi jurnal yang sama |
| ACC-FIN-007 | Pengendalian kegagalan dan duplikasi jurnal | Accounting | Indeks unik `AccAccountingEvent (SourceModule, SourceTransactionId, EventTypeCode, SourceVersion)`; `AccAccountingEventAttempt` (riwayat percobaan); `AccountingEventController` `retry`/`ignore`; indeks unik `AccRecurringJournalRun (TemplateId, AccountingPeriodId)` tanpa saringan hapus; `IX_AccJournal_ReversalOfJournalId` (satu pembalik per jurnal) | Ready to reuse | Sisa kecil, bukan gap: dua petugas yang menekan **Sahkan** pada jurnal yang sama di detik yang sama sama-sama lolos pemeriksaan status karena tidak ada kunci baris. Hasilnya tetap **satu** jurnal Posted (tidak ada saldo ganda), hanya riwayatnya tercatat dua kali | Rendah |
| ACC-FIN-008 | Laporan keuangan | Accounting | Neraca saldo: `AccGeneralLedgerService#GetTrialBalanceAsync`, FE `trial-balance`. Laba rugi, neraca, rasio, target pendapatan, laporan pajak: tidak ada | Extend | **Laba rugi dan neraca dapat dihitung dari data yang ada**, karena klasifikasinya turun dari `AccountType` (Aset/Liabilitas/Ekuitas → neraca, Pendapatan/Beban → laba rugi) dan hierarkinya dari `ParentAccountId`. **Rasio likuiditas** butuh pembeda aset lancar/tidak lancar yang **tidak ada** di COA. **Target pendapatan** bergantung pada Wave 4. **Laporan pajak**: data pajak ada di Finance (`FinPaymentDeduction`, `FinReceiptDeduction`) dan Payroll (`PayrollTaxCalculationService`), bukan di Accounting | Sedang. Perlakuan laba berjalan di neraca harus selaras dengan tutup tahun yang ada |

### Wave 3 — A/P Dokter

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| ACC-FIN-009 | Integrasi utang jasa dokter | Medical Fee (hitung), Finance/Payroll (bayar), Accounting (jurnal) | `MedicalFeeManagement/FeeCalculation#MdfFinanceHandoff` (ada; berisi penerima, periode, nilai bruto, status, `RowVersion`); dibaca `PayrollManagement/Services/PayrollMedicalFeeIntakeService`; `FinanceManagement/Payable/Models/FinMedicalServicePayable` **tanpa penulis** | Conflict | Jasa dokter hari ini mengalir Medical Fee → **Payroll**. PRD menulis Finance pemilik pembayaran. Roadmap Finance (`BE-FIN-021`) masih menulis serah terima Medical Fee "belum ada" — catatan itu **sudah basi** | Tinggi. Risiko pengakuan ganda bila Payroll dan Finance sama-sama mencatat |
| ACC-FIN-010 | Pembayaran `MEDICAL_SERVICE` | Finance | `FinancePaymentService` baris 123 dan 293: `"Jenis utang MEDICAL_SERVICE belum didukung pada task ini."` | Conflict | **Akar masalahnya:** penolakan ini disengaja, karena belum ada baris `FinMedicalServicePayable` yang dapat dilunasi. Membuang validasinya tanpa penulis utang hanya memindahkan galatnya. Tergantung keputusan `C-01` | Tinggi. Di luar wewenang Accounting |
| ACC-FIN-011 | Pajak dan penyesuaian jasa dokter | Payroll/Finance | `PayrollManagement/Services/PayrollTaxCalculationService`, `PayrollTaxOfficialReferenceProvider` (PPh 21 dengan rujukan resmi); `MedicalFeeAdjustmentsController` (penyesuaian di sisi Medical Fee); `FinPayableAdjustment` (ada, tetapi tidak pernah dibuat untuk `MEDICAL_SERVICE`) | Conflict | Mesin pajak sudah ada di Payroll. Mesin yang dipakai bergantung pada jalur pembayaran `C-01` | Tinggi. Tarif pajak tidak boleh dikarang |
| ACC-FIN-012 | Laporan fee dokter, fee KSO, rekonsiliasi bagi hasil | Medical Fee | KSO hanya ditemukan sebagai jenis kepemilikan aset (`MstAssetOwnershipType`, `AstUtilization`), bukan sebagai bagi hasil jasa medis | Unknown | Arti "KSO" pada konteks jasa dokter belum jelas | Sedang |

### Wave 4 — Anggaran

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| ACC-FIN-013 | Anggaran per COA dan unit biaya | ? | `ProcurementManagement/Planning/Models/PrcBudget` (tahun × departemen × kategori barang × sumber dana, `VersionNo`, `PreviousVersionId`, `IsCurrentVersion`, `WorkflowInstanceId`, `RowVersion`), `ProcurementBudgetService` | Conflict | Dimensi anggaran Pengadaan **berbeda** dari yang diminta PRD (akun COA × unit biaya × bulan). Harus diputuskan: satu anggaran dengan dua dimensi, atau dua anggaran yang saling terpetakan | Tinggi. Dua sumber anggaran yang tidak terpetakan menghasilkan realisasi yang tidak dapat direkonsiliasi |
| ACC-FIN-014 | Perencanaan CAPEX | ? | `PrcNeedPlan`, `PrcPackagePlan`, `ProcurementPlanQuotaService` (rencana kebutuhan pengadaan) | Conflict | Rencana kebutuhan Pengadaan kemungkinan sudah memuat sebagian CAPEX. Tumpang tindih belum dipetakan | Sedang |
| ACC-FIN-015 | Perencanaan OPEX | ? | Tidak ada | Missing | Bergantung pada keputusan `C-02` | Sedang |
| ACC-FIN-016 | Konsolidasi anggaran | ? | `ProcurementPlanConsolidationService` (konsolidasi rencana pengadaan saja) | Missing | Laporan laba rugi anggaran, ringkasan CAPEX, klaster pendapatan: tidak ada | Sedang |
| ACC-FIN-017 | Realisasi dan pengendalian anggaran | ? | Komitmen: `PrcBudgetCommitment` (`Amount`, `IsActive`, `ReleasedAt` — pelepasan komitmen sudah ada); realisasi: baris jurnal Posted per akun beban dan unit biaya (`AccJournalLine#CostCenterId`) | Conflict | Dua bahan sudah ada, sambungannya belum. Kebijakan lewat anggaran (peringatan atau blokir) belum diputuskan | Tinggi |

### Wave 5 — Inventory Accounting

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| ACC-FIN-018 | Valuasi persediaan dan HPP | Farmasi/Logistik | `PhmDrugBatch` hanya menyimpan nomor batch, kedaluwarsa, pemasok — **tanpa harga perolehan**. Harga yang ada di Farmasi adalah harga jual resep (`PhmPrescriptionItem#UnitPrice`) | Missing | Accounting tidak dapat menilai persediaan karena sumber operasionalnya tidak menyimpan nilai. Metode (FIFO atau rata-rata tertimbang) juga belum diputuskan | Tinggi. Accounting dilarang menjadi sumber kedua kuantitas stok |
| ACC-FIN-019 | Integrasi stok opname dan penyesuaian stok | Farmasi/Logistik | `DrugStockController`, `DrugStockEnums`, `StockTransferService` (Farmasi); opname di `AssetManagement/PhysicalInventory` adalah opname **aset tetap**, bukan persediaan | Unknown | Proses opname persediaan dan selisih nilainya belum dipetakan sampai ujung | Sedang |
| ACC-FIN-020 | Jurnal persediaan otomatis | Farmasi/Logistik → Accounting | Mekanisme kejadian siap (`AccAccountingEvent`, `AccPostingRule`); **nol** kode kejadian persediaan di katalog `ACC-XMOD-0.7` | Missing | Butuh kode kejadian dan pengirim dari Farmasi/Logistik. Sisi Accounting tidak perlu kode baru — cukup data jenis kejadian dan aturan posting | Sedang |
| ACC-FIN-021 | Rekonsiliasi persediaan; interkoneksi SAKTI | Farmasi/Logistik + Accounting | Rekonsiliasi control account siap: `AccSubledgerBalance`, `ReconciliationController` `subledger-comparison`. SAKTI: nol jejak di source maupun dokumen | Unknown | Mekanisme rekonsiliasi dapat dipakai ulang begitu Farmasi mengirim saldo subledger. SAKTI hanya berlaku bagi satuan kerja pemerintah; status kelembagaan rumah sakit belum diketahui | Sedang |

### Wave 6 — Fixed Asset

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| ACC-FIN-022 | Register aset, cetak label, ubah lokasi | Aset | Sub-modul `register` (`BE-AST-007` sampai `BE-AST-009`: pencatatan, label QR, penempatan), `pergerakan` (`BE-AST-016`..`019`: mutasi antar unit) | Ready to reuse | Accounting cukup menautkan; tidak membuat master aset | Rendah |
| ACC-FIN-023 | Proses penyusutan | Aset → Accounting | `BE-AST-027` 🟡 (dikode, di-build, belum diuji, belum di-commit): sekali per periode per pusat biaya, garis lurus (`AST-DEC-013`); `BE-AST-029` pengirim kejadian (bawaan mati); `BE-AST-032` ⛔ penyalaan per kode | Reuse with adapter | Adapter Accounting = data jenis kejadian + aturan posting untuk kode penyusutan. Nol kode Accounting baru | Sedang. Bergantung penyelesaian roadmap Aset |
| ACC-FIN-024 | Penghapusan aset | Aset → Accounting | Sub-modul `penghapusan` `BE-AST-033`..`037` (usulan, persetujuan, pemindahtanganan, berita acara, pemulihan, riwayat) | Reuse with adapter | Sama dengan 023 | Sedang |
| ACC-FIN-025 | Revaluasi aset | Aset | `AST-DEC-016` (empat jenis perubahan nilai, termasuk revaluasi); **`AST-DEC-032`: revaluasi ditunda ke gelombang lanjutan** (Rizki, 12 November 2026) | Unknown | Sudah diputuskan ditunda. Dicatat agar tidak dibuka ulang dari jalur Accounting | Rendah |
| ACC-FIN-026 | Laporan dan rekonsiliasi aset tetap | Aset + Accounting | `BE-AST-030` (saldo subledger per akun kontrol → `AccSubledgerBalance`), `BE-AST-031` (nilai buku, laporan nilai), `BE-AST-037` (laporan penghapusan); `AstAccountingOutbox` (status tertahan dibedakan dari terkirim) | Reuse with adapter | Rekonsiliasi Accounting yang ada langsung menampilkan selisih begitu saldo subledger Aset masuk | Rendah |

### Wave 7 — Approval & Control

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| ACC-FIN-027 | Pemantauan pengajuan | Accounting | Per fitur sudah ada: daftar jurnal tersaring status `PendingApproval`, `periods/{id}/closing-history`, kotak masuk kejadian dengan ringkasan (`accounting-events/summary`) | Extend | Belum ada satu layar terpusat. Dapat dibangun read-only tanpa tabel baru, tetapi isinya bergantung pada jenis pengajuan yang ditetapkan di 004 | Rendah |
| ACC-FIN-028 | Integrasi approval bertingkat ke semua proses | Accounting | Approval yang ada: jurnal (satu tingkat) dan tutup bulan (`AccPeriodClosingApproval`, peran `Accounting Director` `ACC-DEC-055`) | Extend | Bergantung pada 004. Jasa dokter memakai approval Finance (PRD sendiri menyatakan demikian) | Sedang |
| ACC-FIN-029 | Audit integrasi akuntansi | Accounting | `AccAccountingEvent#JournalId` (kejadian → jurnal), `AccAccountingEventAttempt` (alasan gagal, riwayat ulang), `AccPostingRule` (aturan), `subledger-comparison` (selisih saldo); FE `accounting-events/*`, `reconciliation` | Extend | Kurang: penelusuran satu layar dari transaksi sumber sampai buku besar, ekspor hasil audit. **Deteksi kejadian yang tidak pernah dikirim** butuh hitungan dari modul sumber — tidak dapat disimpulkan dari kotak masuk Accounting saja | Sedang |

## 5. Kontrak as-is yang relevan

Endpoint yang **sudah ada** dan akan disentuh atau dipakai ulang oleh finishing. Seluruhnya di bawah
prefix `api/v1/corporate/accounting`.

**Swagger: `Corporate / Accounting / General Ledger`**

| Method | Path | Hak akses | Isi | Catatan finishing |
| --- | --- | --- | --- | --- |
| GET | `/general-ledger/movements` | `GeneralLedger : Read` | Mutasi satu akun: tanggal, nomor jurnal, nomor baris, keterangan, debit, kredit, saldo berjalan | 005: belum membawa `JournalId`, saldo awal, unit biaya, asal sumber |
| GET | `/general-ledger/trial-balance` | `GeneralLedger : Read` | Neraca saldo satu periode: saldo awal, debit, kredit, saldo akhir per akun, `IsBalanced` | 008: dasar laba rugi dan neraca |
| GET | `/general-ledger/account-balance/{accountId}` | `GeneralLedger : Read` | Saldo satu akun pada satu periode | — |

**Swagger: `Corporate / Accounting / Journal`**

| Method | Path | Isi | Catatan finishing |
| --- | --- | --- | --- |
| POST | `/journals` | Buat draft | 006: tujuan impor |
| POST | `/journals/{id}/submit`, `/approve`, `/reject`, `/post`, `/reverse` | Alur status | 002: penjaga kunci grup masuk di sini lewat `PeriksaSembilanSyaratAsync`; 004: jenjang persetujuan |

**Swagger: `Corporate / Accounting / Accounting Events` dan `Reconciliation`**

| Method | Path | Isi | Catatan finishing |
| --- | --- | --- | --- |
| POST | `/accounting-events` | Terima kejadian dari modul sumber, idempoten | 020, 023, 024: jalur masuk tanpa endpoint baru |
| POST | `/accounting-events/{id}/retry`, PATCH `/{id}/ignore` | Ulang atau abaikan | 029 |
| GET | `/reconciliation/subledger-comparison` | Bandingkan saldo GL dengan saldo subledger | 021, 026 |

**Contoh alur yang sudah berjalan hari ini (dipakai 023):** modul Aset memproses penyusutan November
2026 untuk pusat biaya Radiologi senilai Rp 12.500.000. Kotak keluarnya mengirim satu kejadian dengan
`SourceTransactionId = DEP:{badan hukum}:202611:{kode}:{pusat biaya}`. Bila kejadian yang sama
terkirim dua kali — misalnya karena jaringan putus lalu dicoba ulang — kiriman kedua ditolak indeks
unik `(SourceModule, SourceTransactionId, EventTypeCode, SourceVersion)`. Jurnal penyusutan tetap
satu.

## 6. Daftar conflict dan unknown

| ID | Pertanyaan | Kebutuhan | Yang harus memutuskan | Mengapa tidak dapat diputuskan agent |
| --- | --- | --- | --- | --- |
| `C-01` | Jasa dokter dibayar lewat **Payroll** (jalur yang sudah ada) atau lewat **utang Finance** (`FinMedicalServicePayable`)? Atau dipisah: dokter pegawai lewat Payroll, dokter mitra lewat Finance? | 009, 010, 011, 012 | Owner Medical Fee, Yasmin (Finance), owner Payroll, Rizki | Menentukan pemilik uang dan mesin pajak |
| `C-02` — **tertutup arahnya 25 Nov 2026 → `ACC-DEC-159`; ratifikasi Procurement `OQ-159-1` terbuka** | Anggaran Accounting (COA × unit biaya × bulan) **menggantikan**, **melengkapi**, atau **dipetakan ke** `PrcBudget` Pengadaan? | 013–017 | Rizki + owner Pengadaan | Dua sumber anggaran tanpa peta membuat realisasi tidak dapat direkonsiliasi |
| `C-03` — **tertutup 25 Nov 2026 → `ACC-DEC-165`** | "Grup COA" itu apa: akun induk tingkat tertentu (misalnya tingkat 1–2), atau pengelompokan bebas di luar hierarki? | 001, 002 | Rizki / pemilik proses akuntansi | Menentukan bentuk tabel |
| `C-04` | Approval bertingkat memperluas `AccJournalApproval` atau menumpang mesin workflow HR (`TrxWorkflowInstance`) yang sudah dipakai Pengadaan? | 004, 028 | Rizki (+ owner Workflow bila menumpang) | Pilihan arsitektur dengan dampak lintas modul |
| `C-05` — **tertutup 25 Nov 2026 → `ACC-DEC-166`** | Bolehkah jenis akun anak berbeda dari induknya? | 001 | Pemilik proses akuntansi | Aturan bisnis baru; bagan akun sah dari ekspor `gl_coa` belum diperiksa terhadap aturan ini |
| `C-06` | Subtask "Pilih Pasien" pada jurnal manual bertentangan dengan `ACC-DEC-056` (tidak menyimpan pengenal pasien). Tetap dilarang, atau dibuka untuk jurnal manual saja? | 006 | Rizki | Mengubah keputusan yang sudah disetujui |
| `U-01` | Isi deskripsi dan subtask ClickUp 46 task belum dibaca — sesi ini tanpa konektor ClickUp | Semua | Rizki (ekspor atau tempel isinya) | Tidak ada akses |
| `U-02` | Arti "KSO" pada laporan jasa dokter | 012 | Owner Medical Fee | Tidak ada jejak di source |
| `U-03` | Apakah rumah sakit sasaran wajib SAKTI (satuan kerja pemerintah)? | 021 | Pemilik proses akuntansi | Bergantung status kelembagaan |
| `U-04` | Proses opname persediaan Farmasi/Logistik dan apakah harga perolehan akan disimpan per batch | 018, 019, 020 | Owner Farmasi, owner Logistik | Di luar wewenang Accounting |
| `U-05` — **tertutup 25 Nov 2026 → `ACC-DEC-161`, `167`** | Pembeda aset lancar/tidak lancar untuk rasio likuiditas, dan daftar rasio yang diminta | 008 | Pemilik proses akuntansi | Tidak ada di COA |
| `U-06` | Over budget: peringatan saja atau memblokir transaksi? | 017 | Pemilik proses akuntansi | Kebijakan |

**Catatan 25 November 2026.** `U-07` (sumber data pajak; tercatat di decision log, bukan tabel di atas)
tertutup arahnya → `ACC-DEC-160`, dengan `OQ-160-1` terbuka. Peta ini **tidak** diaudit ulang; fakta
Master COA untuk ACC-FIN-001 diambil ulang dari source `0f67c5c2` dan DB dev 25 November 2026 (decision
log pass 25 November). Rencana ACC-FIN-001: `roadmap/*-finishing.md` revisi 2.

## 7. Pemisahan fakta, inferensi, dan rekomendasi

**Fakta** — terbukti dari source pada SHA di atas: seluruh isi kolom *Bukti* pada bagian 4, serta
temuan bahwa seluruh jalur pembentukan jurnal melewati `PeriksaSembilanSyaratAsync`.

**Inferensi** — masuk akal, belum dibuktikan dengan menjalankan sistem:

- Laba rugi dan neraca dapat dihitung tanpa tabel baru. Dasarnya: neraca saldo sudah menghitung saldo
  per akun dari jurnal Posted, dan `AccountType` sudah membedakan kelompok laporan.
- Catatan "Medical Fee belum ada" di roadmap Finance sudah basi. Dasarnya: `MdfFinanceHandoff` beserta
  controller dan service-nya ada di source.

**Rekomendasi** — untuk dibahas di `grill-me`, bukan keputusan:

| Urutan | Isi | Alasan |
| --- | --- | --- |
| 1 | Mulai dari **ACC-FIN-005 dan ACC-FIN-008 (laba rugi + neraca)** | Nol tabel baru, nol keputusan lintas modul, hasilnya langsung terlihat, dan menjadi dasar 017 dan 026 |
| 2 | Lanjut **ACC-FIN-001, 002, 004** setelah `C-03`, `C-04`, `C-05` dijawab | Wave 1 butuh tabel baru dan aturan bisnis baru |
| 3 | Kirim surat lintas modul untuk `C-01` dan `C-02` lebih awal | Jawabannya paling lama datang |
| 4 | Tutup Wave 6 sebagai "dipenuhi roadmap Aset" | Mencegah duplikasi pekerjaan Rizki sendiri |
| 5 | Tandai Wave 5 sebagai `BLOCKED DEPENDENCY` sampai `U-04` terjawab | Accounting tidak boleh menjadi sumber kedua stok |

## 8. Pemicu impact scan

Peta ini harus ditandai **stale** dan dipindai ulang secara terbatas bila salah satu terjadi:

- SHA backend atau frontend di atas berubah pada path Accounting, Finance Payable, Medical Fee
  FeeCalculation, Procurement Planning, atau Payroll Medical Fee.
- `BE-AST-027` sampai `BE-AST-037` berubah status.
- Finance menulis penulis `FinMedicalServicePayable` atau mencabut penolakan `MEDICAL_SERVICE`.
- Farmasi atau Logistik menambah harga perolehan per batch.
- Isi ClickUp dibaca dan berbeda dari ringkasan PRD.
