# Requirement Traceability — Billing dan Kasir

## Metadata

```yaml
blueprint_id: BIL-CASH-001
blueprint_revision: 0.4
roadmap_revision: 1
status: DRAFT_FORWARD_TEST
decision_revision: 0.2
backend_source: c99f0a51577456c91831870892870f9ae633b4c2
frontend_source: e555bf2ad6848a1d6cc097ab8c6c5f5259edb151
```

## Pemetaan requirement ke delivery

| Requirement/decision | Design/contract | Backend | Frontend | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| Satu invoice dan charge idempotent (`DEC-013`–`018`,`040`) | CTX-01, API/Integration | `BE-BKC-005`,`008` | `FE-BKC-001`,`003` | `AT-001`–`004`,`020` | Covered/planned — `FE-BKC-003` (recalculate + void) source selesai 25 Agustus 2026, menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-003-hitung-ulang-dan-pembatalan-item-invoice.md` |
| Admin fee (`DEC-001`–`006`) | CPT-007, Validation | `BE-BKC-002`,`006` | `FE-BKC-002`,`012` | `AT-009`–`011` | Covered/planned — form create/update Administration Fee Policy dimigrasi ke `BaseEditorView` 28 Agustus 2026 (konsistensi UI, bukan perubahan bisnis); lihat `task/report/frontend/fe-bkc-012-konsistensi-base-component-form-master-data.md` |
| Diskon (`DEC-007`–`012`) | CPT-008/009, Permission | `BE-BKC-003`,`007` | `FE-BKC-002`,`004`,`012` | `AT-012`,`022` | Covered/planned — `FE-BKC-004` (ajukan diskon promo/dokter + approve dokter) source selesai 25 Agustus 2026, menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-004-diskon-promo-dan-approval-dokter.md`. Form create/update Discount Policy dimigrasi ke `BaseEditorView` 28 Agustus 2026; lihat `task/report/frontend/fe-bkc-012-konsistensi-base-component-form-master-data.md` |
| Tax/room/coverage (`DEC-019`–`023`,`041`,`043`) | CPT-021–024, Integration | `BE-BKC-004`,`006` | `FE-BKC-001`,`002`,`012` | `AT-010`,`011`,`013`,`021` | Covered/planned — form create/update Tax Rule dan Room Charge Policy dimigrasi ke `BaseEditorView` 28 Agustus 2026; lihat `task/report/frontend/fe-bkc-012-konsistensi-base-component-form-master-data.md` |
| Deposit/progress (`DEC-025`–`030`) | CTX-02, Patient Funds | `BE-BKC-009`,`011` | `FE-BKC-005` | `AT-007`,`008`,`020` | Covered/planned — `FE-BKC-005` (top-up + allocation, panel deposit di halaman invoice RANAP) source selesai 25 Agustus 2026, menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-005-deposit-rawat-inap-dan-progress-allocation.md` |
| Split tender (`DEC-028`–`030`,`036`) | Settlement State/API | `BE-BKC-010`,`012` | `FE-BKC-006`,`007` | `AT-005`,`006`,`017` | Covered/planned — `FE-BKC-006` (create settlement + tender rows, panel di halaman invoice) source selesai 25 Agustus 2026, menunggu `FE-BKC-007` untuk verifikasi tender tunai dan verifikasi manual; lihat `task/report/frontend/fe-bkc-006-split-tender-dan-reconciliation-status.md` |
| Refund (`DEC-032`,`033`) | CTX-03 | `BE-BKC-013` | `FE-BKC-008` | `AT-008`,`014` | Covered/planned — `FE-BKC-008` source selesai 25 Agustus 2026, menunggu verifikasi manual. **Update 30 Agustus 2026**: `RefundableCreditId` kini bisa ditemukan lewat `GET .../invoices/{invoiceId}/refundable-credits` (backend commit `f5e2106`) — keterbatasan "tidak ada endpoint pencarian" pada laporan asli sudah closed; lihat `task/report/frontend/fe-bkc-010-accessibility-privacy-dan-regression-lintas-workspace.md` |
| Write-off/adjustment (`DEC-034`,`035`,`042`) | CTX-03, Integration | `BE-BKC-014`,`016` | `FE-BKC-008` | `AT-014`,`015`,`021` | Covered/planned — `FE-BKC-008` (ajukan/setujui/reversal, panel di halaman invoice) source selesai 25 Agustus 2026, menunggu verifikasi manual. **Update 30 Agustus 2026**: `ISSUE-FE-008` (tanpa satu pun endpoint `GET`) sudah closed sejak backend commit `f5e2106` — `BillingFinancialExceptionsController` kini punya `GET invoices/{invoiceId}` dan `GET {type}/{id}`, frontend sudah memakainya sebagai source of truth; lihat `task/report/frontend/fe-bkc-010-accessibility-privacy-dan-regression-lintas-workspace.md` |
| Shift kasir (`DEC-037`–`039`) | CTX-04 | `BE-BKC-012` | `FE-BKC-007`,`012` | `AT-016`,`017`,`022` | Covered/planned — `FE-BKC-007` (halaman baru, open/handover/close/review/reopen) source selesai 25 Agustus 2026, menunggu verifikasi manual; temuan gap `GET` by-id shift dan master data Register dicatat di laporan; lihat `task/report/frontend/fe-bkc-007-operasi-shift-kasir.md`. Form create/update Register dimigrasi ke `BaseEditorView` 28 Agustus 2026; lihat `task/report/frontend/fe-bkc-012-konsistensi-base-component-form-master-data.md` |
| Finalisasi/departure (`DEC-031`,`036`,`044`) | CTX-05, State | `BE-BKC-015` | `FE-BKC-009` | `AT-018`,`019`,`023` | Covered/planned — `FE-BKC-009` source selesai (ter-commit `2dcea2f8f`, sebelumnya belum dilaporkan), diverifikasi ulang 30 Agustus 2026 (lint/test:unit/build lulus, satu gap `isFinal`/`CLOSED` diperbaiki); menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-009-preview-dan-finalisasi-invoice.md` |
| AR/AP final (`DEC-041`–`044`) | Integration/Handoff | `BE-BKC-016` | `FE-BKC-009` | `AT-019`,`021`,`023` | Covered, external dependency — panel handoff AR/AP sudah dibangun tapi tidak bisa diuji nyata sampai `BKC-BLK-INT-001` (kontrak konsumen AR/AP) selesai dan transisi `FINAL → CLOSED` benar-benar diaktifkan backend; lihat `task/report/frontend/fe-bkc-009-preview-dan-finalisasi-invoice.md` |
| Security/privacy/concurrency | Permission/Validation | `BE-BKC-001`,`017` | `FE-BKC-010` | `AT-020`,`022`,`024` | Covered/planned — evidence matrix `BE-BKC-017` diperbarui 28 Agustus 2026 (22/24 acceptance ID `Covered`; `AT-020` butuh Postgres nyata, `AT-023` blocked `BKC-BLK-INT-001`); lihat `evidence/06-be-bkc-017-acceptance-evidence-matrix.md` dan `task/report/backend/be-bkc-017-hardening-dan-acceptance-lintas-slice.md`. `FE-BKC-010` (`AT-024`) diaudit 30 Agustus 2026 lewat pembacaan source — privacy/status/label terpenuhi; scan a11y otomatis dan critical E2E journeys masih blocked (tooling/environment); lihat `task/report/frontend/fe-bkc-010-accessibility-privacy-dan-regression-lintas-workspace.md` |
| Menu Pembayaran, Dokumen Kasir: Kwitansi per tender, Struk Pasien (`DEC-045`–`058`, amendment 27-28 Agustus 2026, di luar roadmap revisi 1 asli) | Belum ada dokumen kontrak `0.4` terpisah — desain lahir langsung dari `/grill-me` amendment, bukan `design-business-module` | `BE-BKC-017` slice (kwitansi per tender di `AddTenderAsync`, endpoint `POST .../kwitansi` lama dihapus) | `FE-BKC-011`, `FE-BKC-017` | Tidak terpetakan ke `BIL-AT-001`–`024` (acceptance test matrix ditulis sebelum amendment ini) | Source `FE-BKC-011` selesai, lint/build lulus, menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-011-dokumen-kasir-kwitansi-per-tender-dan-struk-pasien.md`. `BKC-DEC-052`–`058` masih `draft`, belum ada approval formal. **Update 3 September 2026 (`FE-BKC-017`, `BKC-DEC-063`–`064`)**: Dokumen Kasir dipindah dari modal menjadi halaman terpisah (`[slug]/pembayaran/dokumen-kasir`, tab/tender via query string) — isi/data/mekanisme PDF/share TIDAK berubah, murni wadah presentasi. `dokumen-kasir-modal.jsx` dihapus. Lint (`--quiet`) PASS 0 error; `test:unit` PASS 434/434 (tanpa regresi); `npm run build` **BLOCKED/INCONCLUSIVE** — folder `.next/standalone` terkunci oleh instance app yang sedang berjalan di lingkungan builder (`node .next/standalone/server.js`), pengguna eksplisit memilih untuk TIDAK menghentikan proses itu; bukan kegagalan dari kode task ini. Verifikasi manual ter-autentikasi belum dijalankan. Lihat `task/report/frontend/FE-BKC-017.md` |
| Master kategori billing item (`BE-BKC-002` scope asli — `MstBillingItemCategory`) | CPT-006/007 | `BE-BKC-002` (sudah ada sejak awal modul) | `FE-BKC-013` (baru) | `AT-009`–`011` (tidak langsung, digunakan admin fee/diskon/tax) | Frontend CRUD (list+create+update+activate/deactivate/delete) baru dibangun 31 Agustus 2026 — sebelumnya hanya ada slice `options` untuk dropdown picker, tidak ada halaman kelola sama sekali; lihat `task/report/frontend/fe-bkc-013-billing-item-category-crud.md`. Menunggu verifikasi manual |
| Entri manual katalog tarif + coverage per item (`BKC-DEC-059`–`062`, amendment 2 September 2026, blueprint revision `0.5 approved`) | `02-backend-architecture.md`/`03-frontend-architecture.md` § Amendment 2 Sep 2026, `contracts/` amendment `BIL-API-0.4`/`BIL-VALIDATION-0.4`/`BIL-INTEGRATION-0.4`/`BIL-PERMISSION-0.4` | `BE-BKC-018`–`021` | `FE-BKC-014`–`016` | `BIL-AT-025`–`028` | **Update 3 September 2026**: Seluruh empat task backend slice ini (`BE-BKC-018`–`021`) source selesai, dieksekusi atas otorisasi eksplisit pengguna per task individual — `BE-BKC-018` (fondasi `TariffId`+`ADHOC_CATALOG`+konteks encounter), `BE-BKC-019` (endpoint `POST catalog-charges`, `BIL-AT-025`/`026`), `BE-BKC-020` (endpoint `GET catalog-charges/coverage-preview`, `BIL-AT-027`/`028`), `BE-BKC-021` (penyempitan gating approval `RegistrationBillingCoverageAdapter`, dampak GLOBAL — pengguna dikonfirmasi eksplisit atas catatan risiko `BKC-DEC-062` sebelum implementasi dimulai). Build `BE-BKC-018` terkonfirmasi lulus; build `BE-BKC-019`/`020`/`021` **belum diverifikasi** — pengguna menjalankan manual atas permintaan sendiri (lihat `task/report/backend/{BE-BKC-018.md,BE-BKC-019.md,BE-BKC-020.md,BE-BKC-021.md}`). Notifikasi Payer/Insurance+Finance/AR yang direkomendasikan `BKC-DEC-062` sebelum deploy `BE-BKC-021` ke produksi **belum dilakukan** — lihat `BE-BKC-021.md` § 7. **`FE-BKC-014`** (dropdown katalog tarif) source selesai, diverifikasi lewat login sungguhan ke data dev nyata — menemukan bug backend pra-eksisting (`TariffController` filter scope strict-equality) yang diperbaiki lewat task ad-hoc **`BE-BKC-FIX-001`** (di luar penomoran roadmap asli); source perbaikan itu juga belum di-build/restart. **`FE-BKC-015`** (badge coverage per opsi tarif) source selesai — memperluas `FilterSelect` (prop opsional `renderOption`, disetujui pengguna) — disclaimer kondisional terverifikasi hidup, badge visual belum (terhalang `BE-BKC-FIX-001` yang sama). **`FE-BKC-016`** (subtotal Mandiri/Asuransi terpisah) source selesai, murni komposisi tampilan tanpa perubahan formula — verifikasi visual menemukan blocker BARU dan lebih luas: Menu Pembayaran untuk invoice apa pun HTTP 500 karena migration `AddTariffIdToBilInvoiceItem` (`BE-BKC-018`) belum dijalankan ke database (kolom ada di model C# yang sudah di-build, belum ada secara fisik di tabel). Ketiga task FE (`014`–`016`) ditulis tanpa file test (instruksi eksplisit pengguna). **`FE-BKC-FIX-001`** (ad-hoc, terverifikasi hidup): indikator scroll horizontal pada tabel item tagihan Menu Pembayaran, ditemukan saat investigasi laporan bug pengguna. **`FE-BKC-FIX-002`** (ad-hoc, terverifikasi hidup, laporan bug pengguna terpisah dengan screenshot): dropdown Tarif Layanan pada `FE-BKC-014` tidak menampilkan hasil sebelum diketik pencarian meski kategori/konteks kunjungan sudah terisi — diperbaiki dengan override `requireSearch: false` khusus field `tariffId` (resource `tariffs` bersama di registry tidak diubah); menemukan temuan sampingan pra-eksisting (satu baris opsi pseudo berisi teks placeholder selalu muncul di posisi pertama `FilterSelect`, independen dari perbaikan ini). **`FE-BKC-FIX-003`** (ad-hoc, terverifikasi hidup, menutup temuan sampingan `FE-BKC-FIX-002`): root cause ternyata `BaseSelectField`/`addClearOption` (base component bersama, ~481 pemakaian `type: "select"` di 111 berkas) menambahkan baris "clear ke kosong" tanpa syarat — diperbaiki lewat `base-component-decision-gate` (pengguna memilih opsi rekomendasi: sembunyikan baris clear untuk field wajib, dan untuk field opsional hanya tampil saat sudah ada nilai terpilih). **`FE-BKC-FIX-004`** (ad-hoc, terverifikasi hidup, permintaan UX langsung pengguna): Tarif Layanan/Qty kini `disabled` (REUSE `field.disabled`) selama Kategori Tarif belum dipilih; dua permintaan lain (refresh dropdown tarif saat kategori diganti, reset form setelah submit sukses) ternyata sudah terpenuhi sejak `FE-BKC-014` — diverifikasi hidup lewat alur penuh kunjungan→kategori→tarif→qty→submit→reset, bukan implementasi baru. **`FE-BKC-FIX-005`** (ad-hoc, terverifikasi hidup, laporan bug pengguna terpisah dengan screenshot): kata kunci pencarian dan hasil dropdown Tarif Layanan lama masih terbawa setelah Kategori Tarif diganti — root cause state internal `FilterSelect`/`useSelectResource` yang tidak ikut ter-reset saat filter berubah; diperbaiki lewat `base-component-decision-gate` (properti opsional baru `field.remountKey` pada `BaseEditorForm`, opt-in murni, pengguna memilih opsi remount total). **`FE-BKC-FIX-006`** (ad-hoc, terverifikasi hidup untuk kasus dilaporkan, laporan bug pengguna langsung dengan dua screenshot): badge status coverage per item Menu Pembayaran (`FE-BKC-016`) selalu "Penjamin" untuk invoice asuransi walau item tidak coverable dan Subtotal Asuransi Rp 0 — root cause badge dibaca dari cara bayar kunjungan, bukan hasil kalkulasi; diperbaiki memakai `breakdown.items[].coverable` (sudah ada di kontrak, baru dikonsumsi) + status coverage aktual invoice, lewat keputusan eksplisit pengguna. Jalur "Penjamin" sungguhan tidak bisa diverifikasi hidup (tidak ada invoice coverage aktual di dev). **`BE-BKC-FIX-002`** (ad-hoc backend, source+migration selesai, EKSEKUSI migration BELUM dilakukan — wewenang pengguna): investigasi lanjutan `FE-BKC-FIX-006` menemukan dua root cause backend yang membuat coverage asuransi tidak pernah benar-benar diterapkan ke item invoice manapun di seluruh sistem — (1) `MstTariffCategory.IsCoveredByInsuranceDefault` ter-backfill `false` untuk semua kategori akibat migration 2 September yang keliru (defaultValue beda dari model C#), (2) `RegistrationBillingCoverageAdapter.Matches()` tidak pernah bisa match rule manapun karena rujukan item (`SourceReferenceId`) diambil dari idempotency key, bukan referensi domain (TariffId/ProcedureId/DrugId/TariffCategoryId) — diperbaiki dengan migration data (`UPDATE` seluruh baris kategori) dan `BillingCoverageComponent` diberi lima field rujukan eksplisit per dimensi. Otorisasi eksplisit pengguna lewat `AskUserQuestion` (dua kali). **`FE-BKC-FIX-007`** (ad-hoc, terverifikasi hidup, permintaan UX langsung pengguna): tombol "Batal" tidak lagi `router.push` ke daftar invoice — sekarang murni mereset form di tempat (REUSE, pola sama dengan reset pasca-submit sukses). **`BE-BKC-FIX-003`**+**`FE-BKC-FIX-008`** (ad-hoc, satu task disetujui bersama lewat `AskUserQuestion`, BELUM diverifikasi hidup - menunggu rebuild): pengujian nyata atas `BE-BKC-FIX-002` menemukan waterfall coverage cuma mengembalikan TOTAL gabungan (bukan per komponen), menyebabkan badge status per item kembali salah (semua "Penjamin" begitu `IsCoveredByInsuranceDefault` benar untuk semua kategori) DAN Subtotal Mandiri/Asuransi (`FE-BKC-016`) diam-diam menggelembungkan Mandiri sebesar jumlah "Penjamin Belum Terverifikasi". Diperbaiki dengan melacak hasil waterfall PER KOMPONEN (item/admin-fee/room-charge) di backend (`BillingCoverageComponentOutcome` baru pada `BillingCoverageAdapter.cs`, field baru pada `CalculationItemResponse`/`AdministrationFeeCalculationResponse`/`RoomChargeCalculationResponse`), dikonsumsi frontend untuk badge 3-status (Tunai/Penjamin/Menunggu Verifikasi) dan split Subtotal-Pajak Mandiri-Asuransi yang EKSAK (bukan proporsional/heuristik lagi). Lihat `task/report/backend/BE-BKC-FIX-003.md` dan `task/report/frontend/FE-BKC-FIX-008.md`. **`BE-BKC-FIX-004`** (ad-hoc, dua keputusan bisnis disetujui lewat `AskUserQuestion`, BELUM diverifikasi hidup): (1) item TANPA rule asuransi sama sekali kini otomatis Mandiri, bukan unresolved (mengubah sebagian gating `BE-BKC-021`) - rule yang ADA tapi butuh approval/limit bulanan TETAP unresolved seperti sebelumnya; (2) PPN Obat/Alkes kini juga mempertimbangkan rawat jalan vs rawat inap (dibebaskan untuk rawat inap, terlepas status coverage), bukan cuma kategori `IsPharmacy` saja. Lihat `task/report/backend/BE-BKC-FIX-004.md`. **`BE-BKC-FIX-005`** (ad-hoc, laporan bug pengguna langsung dengan dua screenshot, BELUM diverifikasi hidup): `Matches()` di `RegistrationBillingCoverageAdapter` (kalkulasi Menu Pembayaran sesungguhnya) memakai gerbang tunggal `rule.ItemType == component.CoverageItemType` yang memaksa item berkategori Pharmacy/Drug/Consumable-Alkes SELALU bertag `"Drug"` — rule `ItemType="ServiceCategory"` menyasar kategori Drug/Pharmacy lewat `TariffCategoryId` (rule "Coverage Obat Rajal"/"Coverage Pharmacy" buatan pengguna) tidak pernah bisa cocok akibatnya, walau `TariffCategoryId`-nya benar diisi — beda dengan engine preview tarif (`InsuranceCoverageService.FindCoverageRuleAsync`) yang menunjukkan item "Tercover" dengan benar untuk rule yang sama. Diperbaiki dengan menyelaraskan `Matches()` ke pola OR-chain per-dimensi `InsuranceCoverageService` (literal string `ItemType` dikonfirmasi dari source, bukan tebakan) — `CoverageItemType`/`CoverageItemType()` dibiarkan ada (vestigial, tidak dipakai gating lagi) tapi tidak dibersihkan (di luar scope). Risiko dicatat: rule blanket tanpa referensi spesifik apa pun (pola lama yang didukung gerbang tunggal sebelumnya) tidak lagi cocok ke apa pun setelah perbaikan ini — tidak ditemukan bukti pola itu dipakai nyata di data yang sudah diverifikasi sesi ini. Lihat `task/report/backend/BE-BKC-FIX-005.md`. **`FE-BKC-FIX-009`** (ad-hoc, permintaan langsung pengguna, BELUM diverifikasi hidup — dev server stale): setelah `BE-BKC-FIX-005` diverifikasi (item Drug jadi "Penjamin"), pengguna mengubah rule Radiology (CoveragePercent=75/CoPaymentPercent=25) dan mempertanyakan Subtotal Mandiri yang masih ada pada item berbadge "Penjamin" — diverifikasi lewat query backend langsung, kalkulasinya BENAR (co-payment rule memang menyisakan porsi pasien meski item itu "Penjamin"). Pengguna meminta info co-payment ditampilkan di tabel item untuk semua item coverage asuransi <100% — ditambahkan baris "Co-payment pasien: Rp{nominal}" di bawah badge "Penjamin" (REUSE `styles.sectionHint`, tanpa perubahan backend — field yang dibutuhkan sudah ada sejak `BE-BKC-FIX-003`). Temuan sampingan dilaporkan (belum diperbaiki): `CalculateCoveredAmount()` menumpuk `CoveragePercent`+`CoPaymentPercent` sebagai dua pengurang independen (75%-25%=cuma 50% tertanggung, bukan 75%) — menunggu keputusan pengguna. Lihat `task/report/frontend/FE-BKC-FIX-009.md`. **`BE-BKC-FIX-006`** (ad-hoc, keputusan bisnis eksplisit pengguna, BELUM diverifikasi hidup): `CoveragePercent`/`CoPaymentPercent` dikonfirmasi pengguna SALING MELENGKAPI (jumlah 100), bukan dua pengurang independen — sebelumnya KEDUA engine coverage (`BillingCoverageAdapter.CalculateCoveredAmount()` dan `InsuranceCoverageService.ResolveTariffInternalAsync`) menumpuk keduanya (mis. Coverage 75% dipotong lagi 25% dari eligible penuh, hasil akhir cuma 50% tertanggung). Diperbaiki di KEDUA engine (CoPaymentPercent tidak lagi jadi pengurang terpisah, CoPaymentAmount/nominal tetap independen tidak berubah) + `InsuranceCoverageRuleController` kini menurunkan `CoPaymentPercent` server-side dari `CoveragePercent` (authoritative, mengabaikan nilai client) — data lama tidak perlu migrasi, cukup buka+simpan ulang rule lewat form. Dependency frontend diselesaikan lewat **`FE-BKC-FIX-010`** (ad-hoc, BELUM diverifikasi hidup): field "Persentase Co-Payment" pada form master data Insurance Coverage Rule kini read-only, otomatis mengikuti `100 - Persentase Coverage` secara live (REUSE `getFieldDisabled`/`getDisabledReason` yang sudah disediakan `BaseGroupedEditorForm`); rule lama yang datanya tidak konsisten langsung menampilkan nilai turunan yang benar begitu form dibuka. Lihat `task/report/backend/BE-BKC-FIX-006.md` dan `task/report/frontend/FE-BKC-FIX-010.md`. **`BE-BKC-FIX-007`** (ad-hoc, permintaan pengguna langsung, independen, BELUM diverifikasi hidup): kolom "Satuan" tabel item Menu Pembayaran selalu "-" karena `InvoiceItemResponse` (`GET .../invoices/{id}`) tidak punya field Unit — ditambahkan untuk kategori Drug/Pharmacy/Consumable-Alkes, diisi `MeasurementName` lewat rantai `BilInvoiceItem.TariffId → MstTariff.Drug → MstDrug.DispenseUnitMeasurement` (navigation property sudah ada, tanpa migration), digerbangi `Category.IsPharmacy`. Murni backend — frontend sudah membaca field ini. Lihat `task/report/backend/BE-BKC-FIX-007.md`. **`BE-BKC-FIX-008`**+**`FE-BKC-FIX-011`** (ad-hoc, permintaan fitur baru langsung pengguna dengan referensi visual, tiga keputusan scope dikonfirmasi lewat `AskUserQuestion`, BELUM diverifikasi hidup): halaman baru "Riwayat Pembayaran" — daftar SEMUA invoice yang sudah punya minimal satu pembayaran, lintas pasien (beda dari Running Invoice yang tidak membawa info penjamin/pembayaran). Satu baris = satu invoice; endpoint baru `GET .../invoices/payment-history` (dua-pass query: filter invoice ber-tender `SUCCEEDED`, lalu batch lookup penjamin/`ClaimMethod`/`PatientAmount`/daftar Kwitansi) mengembalikan status Lunas/Cicilan dan seluruh Kwitansi per invoice; aksi "Lihat Kwitansi" per baris membuka dropdown seluruh Kwitansi invoice itu, masing-masing REUSE penuh alur cetak Kwitansi yang sudah ada (`FE-BKC-011`/`FE-BKC-017`, tanpa komponen cetak baru). Helper `derivePaymentInstallmentSummary` (label "Angsuran N/M", status per Kwitansi dari akumulasi pembayaran) disiapkan sebagai fondasi. **Redesain penuh `kwitansi-document.jsx`** (Total Tagihan/Rincian Pembayaran/Sisa Pembayaran/nama petugas kasir/detail dokter, sesuai referensi Kwitansi pengguna) **DIMINTA TAPI SENGAJA BELUM DIKERJAKAN** — mengubah dokumen yang sudah dipakai produksi untuk setiap pembayaran di seluruh sistem, menunggu keputusan cakupan pengguna (lihat `task/report/frontend/FE-BKC-FIX-011.md` § 5). Lint (`eslint . --quiet` repo penuh) PASS 0 error; `test:unit`/`build` tidak dijalankan (instruksi eksplisit pengguna). Lihat `task/report/backend/BE-BKC-FIX-008.md` dan `task/report/frontend/FE-BKC-FIX-011.md`. Lihat `task/report/frontend/{FE-BKC-014.md,FE-BKC-015.md,FE-BKC-016.md,FE-BKC-FIX-001.md,FE-BKC-FIX-002.md,FE-BKC-FIX-003.md,FE-BKC-FIX-004.md,FE-BKC-FIX-005.md,FE-BKC-FIX-006.md,FE-BKC-FIX-007.md,FE-BKC-FIX-008.md,FE-BKC-FIX-009.md,FE-BKC-FIX-010.md,FE-BKC-FIX-011.md}` dan `task/report/backend/{BE-BKC-FIX-001.md,BE-BKC-FIX-002.md,BE-BKC-FIX-003.md,BE-BKC-FIX-004.md,BE-BKC-FIX-005.md,BE-BKC-FIX-006.md,BE-BKC-FIX-007.md,BE-BKC-FIX-008.md}` |

## Coverage acceptance test

| Test | Task utama | Jalur gagal tercakup |
| --- | --- | --- |
| `BIL-AT-001`–`004` | `BE-BKC-005`,`008`; `FE-BKC-001`,`003` | duplicate, incomplete, void invalid |
| `BIL-AT-005`–`008` | `BE-BKC-009`–`011`,`013`; `FE-BKC-005`,`006` | tender fail/pending, insufficient, excess credit |
| `BIL-AT-009`–`013` | `BE-BKC-002`–`007`; `FE-BKC-002`,`004` | duplicate fee, forbidden discount, coverage cap |
| `BIL-AT-014`–`017` | `BE-BKC-012`–`014`; `FE-BKC-007`,`008` | self-approval, reversal, variance, late settlement |
| `BIL-AT-018`–`021` | `BE-BKC-006`,`008`,`011`,`015`,`016`; `FE-BKC-003`,`005`,`009` | departure unpaid, conflict, post-final correction |
| `BIL-AT-022`–`024` | `BE-BKC-017`; `FE-BKC-010` | unauthorized, consumer down, privacy/a11y |
| `BIL-AT-025`–`028` | `BE-BKC-018`–`021`; `FE-BKC-014`–`016` | tarif nonaktif/kedaluwarsa, disparitas preview vs kalkulasi final |

## Coverage gap dan blocker

| ID | Gap | Dampak | Owner | Status/aksi |
| --- | --- | --- | --- | --- |
| `BKC-BLK-FE-001` | Root governance frontend tidak ditemukan | Semua FE write tertahan | Frontend authority | Tetapkan/restore sebelum builder. **Catatan 2 September 2026**: `QuilvianSystemFrontendDev/AGENTS.md` ditemukan dan terbaca pada sesi desain `FE-BKC-014`–`016` — kemungkinan sudah resolved, TAPI belum diverifikasi ulang formal terhadap seluruh task lama (`FE-BKC-001`–`013`). Builder tetap wajib memverifikasi saat mulai eksekusi |
| `BKC-BLK-INT-001` | Schema/transport aktual AR dan AP belum dibuktikan di consumer | `BE-BKC-016` tidak dapat final | AR/AP + Integration | Contract discovery sebelum task approval |
| `BKC-BLK-PROV-001` | Provider payment/refund sandbox dan callback contract belum dipilih | E2E `AT-006`,`013` refund provider | Treasury/Integration | Adapter bisa dibangun terhadap interface; aktivasi menunggu provider |
| `BKC-BLK-DATA-001` | Nilai seed Finance/Inpatient belum dicantumkan | Master dapat dibuat tetapi tidak boleh diaktifkan | Finance/Inpatient | Serahkan nominal/rate/rules sebelum seed aktif |

Tidak ada requirement bisnis approved yang kehilangan task. Gap di atas adalah dependency implementasi/operasional, bukan izin untuk mengarang policy. Roadmap dianggap siap dieksekusi hanya per task yang disetujui, dimulai dari `BE-BKC-001` atau task master independen setelah fondasi tersedia.

---

# Amendment 4 September 2026 — Traceability gelombang `MVP-4` sampai `MVP-12`

## Metadata

```yaml
roadmap_revision: 2
status: DRAFT_FORWARD_TEST
blueprint_revision_dibaca: 0.8 (manifest) + 0.9 (02-backend-architecture.md)
blueprint_status: draft — readiness DESIGN_DRAFT_AWAITING_APPROVAL
baseline_revision: 0.5 (approved 2 September 2026)
decision_revision: 0.2 (baseline) + BKC-DEC-059 s.d. BKC-DEC-091 (seluruhnya approved)
design_decisions: BKC-DES-001 s.d. BKC-DES-025 approved; BKC-DES-026 dan BKC-DES-027 draft
backend_source_pada_manifest: ffeb45a83a6282982214668acc57e15ac0652f04
backend_source_terverifikasi: fd4a605 (branch Yasmina, 4 September 2026) — 52 commit dan 237 berkas source di depan baseline
frontend_source_pada_manifest: 00210f9a5fb2f4f69e57b8c90c57c63c788da792
frontend_source_terverifikasi: belum diperiksa — repository frontend tidak tersedia pada sesi ini
```

## 1. Pemetaan requirement ke delivery

| Requirement/decision | Design/contract | Backend | Frontend | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| Rupiah tanggungan penjamin per baris biaya (`FR-BKC-009`–`013`, `BKC-DEC-069`) | `BKC-DES-001`–`006`, `015`–`017`; `BIL-API-0.6`, `BIL-VALIDATION-0.6` (`BIL-VAL-028`) | `BE-BKC-022` | — | `BIL-AT-029`, `030`, `049`, `050`, `052` | **Source selesai 4 September 2026, verifikasi menunggu pengguna.** `IsPerItemAllocationAvailable` + penjaga `BIL-VAL-028` ditambahkan; 3 test baru; 2 fixture basi diperbaiki (nilai assert tidak berubah). `BIL-AT-030`/`049` sudah terpenuhi sejak `BE-BKC-FIX-003`. `dotnet build`/`test` belum dijalankan — lihat `task/report/backend/BE-BKC-022.md` |
| Lembar "Invoice Asuransi" (`FR-BKC-014`–`017`, `020`; `BKC-DEC-065`–`068`) | `BKC-DES-007`–`009`; `BIL-API-0.5`, `BIL-VALIDATION-0.5` (`BIL-VAL-029`–`034`) | `BE-BKC-023` | `FE-BKC-018` | `BIL-AT-031`–`035` | **`dotnet build`/`test` dikonfirmasi lulus oleh pengguna 5 September 2026.** `BKC-GATE-03` (penilaian Security) **ditutup** 5 September 2026 (`BKC-DEC-092`) — dipakai ulang apa adanya. Lihat `task/report/backend/BE-BKC-023.md`. **Update 7 September 2026**: `FE-BKC-018` (frontend) dikerjakan ulang dari nol setelah `BKC-GAP-08` (lihat § 2 di bawah) — source kali ini terverifikasi ada, lint/test:unit lulus, `build` sengaja tidak dijalankan pelaksana (menunggu pengguna); lihat `task/report/frontend/FE-BKC-018.md` |
| Tab dan cetak A4 pada Dokumen Kasir (`FR-BKC-018`, `019`) | `03-frontend-architecture.md` § Amendment 3 September (kedua) | — | `FE-BKC-018` | Acceptance 32–39 pada arsitektur frontend | **`npm run lint:errors`/`test:unit`/`build` dikonfirmasi lulus 6 September 2026** — `BKC-GATE-03` ditutup, `BE-BKC-023` terverifikasi. Tab "Invoice Asuransi" baru (sejajar Kwitansi/Struk Pasien), satu pemanggilan data (`getInsuranceInvoiceDocument`) hanya saat tab dibuka, PDF A4 (Kwitansi/Struk Pasien tetap A5), plus perbaikan pengenalan nilai tab dari query string. `test:unit` 440/440 lulus tanpa regresi; `build` sukses (275/275 halaman statis, route dinamis baru tanpa error). Verifikasi manual ter-autentikasi (lima keadaan penjamin) masih menunggu. Lihat `task/report/frontend/FE-BKC-018.md` |
| Pencabutan empat gerbang penahan tanggungan (`FR-BKC-021`–`024`, `026`; `BKC-DEC-071`, `072`, `074`) | `BIL-VALIDATION-0.6` § Aturan yang dicabut | `BE-BKC-024` | `FE-BKC-019` | `BIL-AT-036`–`039`, `052`, `053` | **`dotnet build`/`test` dikonfirmasi lulus oleh pengguna 5 September 2026.** Sisa dua gerbang (`NeedApproval`, limit bulanan) dicabut di `ResolveAsync`; `BIL-AT-037` (baru) dan `BIL-AT-039` (sebelumnya tanpa test) ditambahkan; batas per kunjungan diverifikasi regresi via test baru. Lihat `task/report/backend/BE-BKC-024.md`. **`FE-BKC-019` (Ringkasan Pembayaran): `lint`/`test:unit`/`build` lulus 6 September 2026** — baris "Penjamin Belum Terverifikasi" diganti "Selisih Tidak Ditagihkan (kontrak penjamin)" (menjumlahkan `unresolvedCoverageAmount`+`nonBillableResidualAmount`); Subtotal/Pajak Mandiri ikut dikurangi `nonBillableResidualAmount` per komponen. Menunggu verifikasi manual; lihat `task/report/frontend/FE-BKC-019.md` |
| Anomali data penjamin (`FR-BKC-027`–`031`; `BKC-DEC-073`) | `BKC-DES-010`–`012`; `BIL-VAL-035`–`037` | `BE-BKC-025` | `FE-BKC-020` | `BIL-AT-041`–`043`, `054` | **`dotnet build`/`test` dikonfirmasi lulus oleh pengguna 5 September 2026.** `BillingCoverageAnomaly`, empat kode, `BIL-VAL-035`/`037` baru dan `036` diretarget. Lihat `task/report/backend/BE-BKC-025.md`. **`FE-BKC-020` (peringatan anomali + penanda per baris): `lint`/`test:unit`/`build` lulus 6 September 2026** — peringatan kuning (`InformationAlert variant="warning"`, satu per kalimat `anomalyMessages`) ditaruh di atas Ringkasan Pembayaran; badge baris baru "Anomali Data" (`getItemCoverageStatus`, diperiksa sebelum "Menunggu Verifikasi") dari `itemDataAnomalyAmount`/`taxDataAnomalyAmount`; nominal anomali tidak dijadikan baris subtotal. Menunggu verifikasi manual; lihat `task/report/frontend/FE-BKC-020.md` |
| Gerbang PPN rawat inap versus rawat jalan (`FR-BKC-032`–`036`; `BKC-DEC-078`, `079`) | `BKC-DES-018`, `019`; `BIL-VAL-038`, `039` | `BE-BKC-026` | — | `BIL-AT-044`–`046`, `051` | **`dotnet test` dikonfirmasi lulus oleh pengguna 5 September 2026.** Enam test baru menutupi seluruh acceptance; satu fixture test lama yang basi (`RecalculateCreatesImmutableVersionsWithTaxProvenance`) ditemukan tidak konsisten dengan gerbang `IsPharmacy` dan diperbaiki. `BKC-OQ-085` tetap terjawab (paparan nol — dikonfirmasi ulang via query: nol tagihan `RANAP` di database dev). Lihat `task/report/backend/BE-BKC-026.md` |
| Cara pembagian PPN mengikuti nasib barangnya (`FR-BKC-037`; `BKC-DEC-077`) | `BKC-DES-020` — tindakan data, bukan kode | `BE-BKC-031` | — | `BIL-AT-047`, `048`; bukti keluar butir 4 | **`DONE` — diverifikasi 5 September 2026.** Audit data langsung (query read-only, otorisasi eksplisit pengguna): tarif PPN aktif (`PPN-001`) `AllocationRule=PROPORTIONAL`, hanya satu tarif aktif pada satu waktu. Tidak ada koreksi. `UAT-17` boleh dilanjutkan. Lihat `task/report/backend/BE-BKC-031.md` |
| Ember tersendiri untuk selisih tidak dapat ditagihkan (`FR-BKC-038`, `039`; `BKC-DEC-080`) | `BKC-DES-021`, `022`; `BIL-VAL-043` | `BE-BKC-027`, `BE-BKC-028` | `FE-BKC-019` | `BIL-AT-055`–`057` | `BE-BKC-027`/`028`: **`DONE`, diverifikasi 6 September 2026.** `dotnet build`/`test` lulus; migration `20260904232421_AddWriteOffCategoryAndNonBillableResidual` **dieksekusi dan dibuktikan langsung** (tercatat di `__EFMigrationsHistory`; kolom `BilWriteOffCase.Category` default `'PATIENT_AR'` dan `BilCalculationVersion.NonBillableResidualAmount` default `0`, keduanya `NOT NULL`, ada secara fisik). `BKC-GATE-09` **ditutup**. Belum ada transaksi nyata untuk didemonstrasikan (database dev nol `BilWriteOffCase`). Bukti `task/report/backend/BE-BKC-027.md`, `BE-BKC-028.md`, `BE-BKC-032.md` |
| Penanggungan selisih lewat write-off (`FR-BKC-040`–`044`; `BKC-DEC-036`, `080`) | `BKC-DES-023`–`025`; `BIL-VAL-040`–`042`, `BIL-VAL-018`/`023` dipertegas | `BE-BKC-029` | `FE-BKC-021` | `BIL-AT-058`–`061`; `UAT-21`–`26` | **`DONE`, diverifikasi 6 September 2026.** `dotnet build`/`test` lulus (plafon+penjaga kategori pada `CreateWriteOffAsync`/`ApproveWriteOffAsync`; formula outstanding diperbaiki konsisten di 4 service). Kolom penopang sudah ada secara fisik sejak migration `BE-BKC-027` dieksekusi — tidak lagi akan gagal runtime. Belum ada transaksi nyata untuk didemonstrasikan; bukti `task/report/backend/BE-BKC-029.md`. **`FE-BKC-021` (Pengecualian Finansial): `lint`/`test:unit`/`build` lulus 6 September 2026 untuk acceptance 1–6** — sisa selisih (`nonBillableResidualRemaining`) ditampilkan dari server; pemilih kategori pada formulir write-off; kategori+nominal terisi awal dari sisa selisih; kolom Kategori pada tabel case. **Acceptance 7 (peringatan finalisasi) sengaja TIDAK dikerjakan** — diblokir `BKC-GAP-01` (desain `BKC-DEC-090` belum ditulis). Menunggu verifikasi manual; lihat `task/report/frontend/FE-BKC-021.md` |
| Perluasan perutean ke jalur `NotCovered` (`BKC-DEC-089`, menutup `BKC-OQ-093`) | `BKC-DES-026`, `027` — **approved** 5 September 2026 | `BE-BKC-030` | — | `BIL-AT-062`, `063` — ditulis pada `testing/acceptance-test-matrix.md`, `approved` | **`dotnet build`/`test` dikonfirmasi lulus oleh pengguna 5 September 2026** (cabang `NotCovered` dipindahkan ke akumulator `nonBillableResidual` yang sama dengan jalur 5; `BIL-AT-062`–`063` tercakup test baru); bukti `task/report/backend/BE-BKC-030.md` |
| Finalisasi diperingatkan, bukan diblokir (`BKC-DEC-090`, menutup `BKC-OQ-094a`) | Belum dirancang — bukan bagian revisi `0.8` maupun `0.9` | — | `FE-BKC-021` acceptance 7 | `UAT-27` | **Coverage gap desain** — lihat bagian 3 |
| Selisih non-billable di luar alur AR/AP (`BKC-DEC-091`, menutup `BKC-OQ-094b`) | Belum dirancang — bukan bagian revisi `0.8` maupun `0.9` | — | — | — | **Coverage gap desain** — lihat bagian 3 |
| Regresi dan bukti keluar lintas gelombang | Seluruh kontrak gelombang ini | `BE-BKC-032` | — | Seluruh `BIL-AT-029`–`061` | **`DONE` — ditutup 6 September 2026 atas keputusan eksplisit pengguna (fokus saat ini: `RAJAL`).** `dotnet build`/`test` lulus; `BKC-GATE-03` dan `BKC-GATE-09` keduanya tertutup, dibuktikan langsung; bukti butir #1, #6, #7 lengkap. **Butir #2, #3, #4, #5, #8 (bukti nyata `RANAP`/asuransi/write-off) sengaja ditunda** — wajib dilengkapi sebelum modul siap produksi untuk rawat inap atau write-off. Lihat `task/report/backend/BE-BKC-032.md` |

## 2. Cakupan acceptance test

| Test | Task utama | Jalur gagal yang tercakup |
| --- | --- | --- |
| `BIL-AT-029`, `030`, `049`, `050`, `052` | `BE-BKC-022` | Rincian tidak menjumlah; kunci alokasi tertukar antar baris pajak; salinan lama dibaca sebagai Rp 0 |
| `BIL-AT-031`–`035` | `BE-BKC-023`; `FE-BKC-018` | Kunjungan bukan-asuransi dijawab galat; kebocoran isi kesepakatan dan nomor kartu |
| `BIL-AT-036`–`039`, `053` | `BE-BKC-024`; `FE-BKC-019` | Nominal tertahan seperti perilaku lama; batas per kunjungan ikut tercabut |
| `BIL-AT-041`–`043`, `054` | `BE-BKC-025`; `FE-BKC-020` | Tanggungan ditolak diam-diam menjadi tagihan pasien; pembayaran terhalang peringatan |
| `BIL-AT-044`–`046`, `051` | `BE-BKC-026` | PPN rawat inap tetap dipungut; IGD ikut dibebaskan; jenis kunjungan tak dikenal menghentikan perhitungan |
| `BIL-AT-047`, `048` | `BE-BKC-031` | PPN obat yang tidak ditanggung ikut dibebankan ke asuransi |
| `BIL-AT-055`–`057` | `BE-BKC-028` | Selisih masuk nominal menggantung atau porsi pasien; mesin melahirkan kasus write-off sendiri |
| `BIL-AT-058`–`061` | `BE-BKC-029`; `FE-BKC-021` | Plafon diuji terhadap tagihan pasien; pengaju menyetujui sendiri; kategori asing diterima diam-diam |
| `BIL-AT-062`, `063` | `BE-BKC-030` | Jalur `NotCovered` masih mengisi nominal menggantung (bukan selisih write-off) |
| `UAT-21`–`27` | `BE-BKC-029`; `FE-BKC-021` | `UAT-27` sengaja memilih memperingatkan, bukan memblokir |

## 3. Coverage gap dan blocker gelombang ini

| ID | Gap | Dampak | Owner | Status/aksi |
| --- | --- | --- | --- | --- |
| ~~`BKC-GATE-01`~~ | ~~Tujuh dokumen kontrak masih `draft`~~ **DITUTUP 4 September 2026** | — | Pemilik blueprint + Product/Domain Owner | Product/Domain Owner (wewenang ganda Finance/AR) mengunci keenam dokumen kontrak. Delapan task backend dan seluruh task frontend kini bebas dari gerbang ini |
| ~~`BKC-GATE-02`~~ | ~~Bukti desain dibaca terhadap `ffeb45a8`, `HEAD` 52 commit di depannya~~ **DITUTUP 4 September 2026** | — | `/qv-trace` | Dijalankan; hasil di `01-existing-capability-map.md` § 17. Cakupan `BE-BKC-022`, `024`, `026`, `032` sudah dinilai ulang |
| ~~`BKC-GATE-03`~~ | ~~Penilaian Security atas pemakaian ulang `BillingInvoice : Read` untuk lembar bernomor polis~~ **DITUTUP 5 September 2026** — `BKC-DEC-092` (Security Owner): dipakai ulang apa adanya | Sebelumnya menahan `BE-BKC-023` dan `FE-BKC-018` — tidak lagi | Security Owner | — (tidak lagi menahan apa pun; bila kelak dipisah jadi permission tersendiri, peran harus dipetakan ulang) |
| ~~`BKC-GATE-04`~~ | ~~`BKC-OQ-085` — dampak penurunan tagihan rawat inap~~ **DITUTUP 4 September 2026** | — | Billing/Finance/AR | Pemilik konfirmasi: belum ada tagihan rawat inap, penurunan nol, kelebihan bayar nol |
| `BKC-GATE-05` | `BKC-OQ-083` — PPN untuk `MCU`, `TELEMEDICINE`, dan `OTC`. **Diturunkan**: ketiganya belum dipakai | Tidak lagi menahan `BE-BKC-026`; hanya syarat sebelum salah satunya diaktifkan | Product/Domain Owner + Finance/Tax | Lampirkan hasil `BIL-AT-051` sebagai bahan keputusan saat aktivasi |
| ~~`BKC-GATE-06`~~ | ~~`BKC-DES-026`–`027` `draft`; kontrak `BIL-API-0.8`/`BIL-TEST-0.8` belum ditulis; `BIL-AT-062`–`063` belum ada~~ **DITUTUP 5 September 2026** — ketiganya selesai | Sebelumnya menahan `BE-BKC-030` — tidak lagi | Product/Domain Owner + pemilik blueprint | — (tidak lagi menahan apa pun) |
| ~~`BKC-GATE-07`~~ | ~~Enam berkas working tree belum di-commit dan belum pernah dibangun~~ **DITUTUP 4 September 2026** | — | Pemilik repository | Keempat berkas backend sudah ter-commit di `HEAD`, working tree bersih, dan solution terbukti dibangun tanpa galat. Penahan ketiga task itu kini `BKC-GATE-02` |
| `BKC-GATE-08` | Kelengkapan `MstInsuranceProvider` dan `MstInsuranceCoverageRule` di lingkungan uji | `UAT-05`, `UAT-06`, `UAT-10` tidak dapat dijalankan dengan data bermakna | Insurance/Finance Owner | Tidak memblokir penulisan kode |
| ~~`BKC-GATE-09`~~ | ~~`BKC-OQ-092` — wewenang tulis backend/frontend, mode task, dan cabang kerja~~ **DITUTUP 6 September 2026** untuk migration `BE-BKC-027` — dieksekusi pengguna, dibuktikan langsung (migration tercatat di `__EFMigrationsHistory`, kolom baru ada secara fisik) | Sebelumnya menahan eksekusi migration `BE-BKC-027` — tidak lagi | Pengguna | — (tidak lagi menahan migration ini) |
| `BKC-GAP-01` | `BKC-DEC-090` (finalisasi diperingatkan, bukan diblokir) sudah `approved` tetapi **desainnya belum ditulis** | `FE-BKC-021` acceptance 7 dan `UAT-27` belum punya rujukan desain | Pemilik blueprint | Pass desain tersendiri; tidak memblokir gelombang ini |
| `BKC-GAP-02` | `BKC-DEC-091` (selisih non-billable di luar alur AR/AP) sudah `approved` tetapi **desainnya belum ditulis** | Perilaku penyerahan AR/AP untuk kategori baru memakai perilaku berjalan apa adanya | Pemilik blueprint + Finance/AR | Pass desain tersendiri |
| `BKC-GAP-03` | Manifest berhenti di revisi `0.8`, sementara `02-backend-architecture.md` sudah memuat revisi `0.9` | Manifest menyebut penyelarasan `BKC-OQ-093` sebagai "gap kecil tersisa", padahal desainnya sudah ditulis | Pemilik blueprint | Perbarui manifest ke revisi `0.9` beserta hash artefaknya |
| ~~`BKC-GAP-04`~~ | ~~`contracts/api-contract.md` dan `testing/acceptance-test-matrix.md` belum menyusul revisi `0.9`~~ **DITUTUP 5 September 2026** — keduanya ditulis dan `approved` | `BIL-API-0.8`, `BIL-TEST-0.8`, dan `BIL-AT-062`–`063` kini ada isinya | Pemilik blueprint | — (bagian `BKC-GATE-06`, sudah tertutup) |
| `BKC-GAP-05` | `MODULE-STATUS.md` masih menulis revisi `0.4` dan belum mencatat gelombang `MVP-4`–`MVP-12` | Pembaca status modul memperoleh gambaran yang jauh tertinggal | `/manage-module-blueprint` | Perbarui setelah roadmap revisi `2` ini ditinjau |
| `BKC-GAP-06` | Manifest mencatat `backend_commit_sha: ffeb45a8` sebagai baseline bukti, padahal `HEAD` sudah 52 commit di depannya | Seluruh `input_hashes` dan bagian "Bukti as-is" pada revisi `0.7`–`0.9` menggambarkan kode yang sudah berubah | Pemilik blueprint | Perbarui SHA dan hash sesudah `/qv-trace` dijalankan — bukan sebelumnya |
| `BKC-GAP-07` | `frontend_commit_sha` belum dapat diverifikasi pada sesi ini | Seluruh task `FE-BKC-018`–`021` disusun atas bukti frontend yang belum dikonfirmasi ulang | Frontend authority | Periksa repository frontend dan catat SHA yang benar-benar berlaku |

## 4. Pemeriksaan requirement yatim

Seluruh functional requirement `FR-BKC-009` sampai `FR-BKC-044` memiliki sedikitnya satu task.
Tidak ada requirement bisnis yang sudah `approved` dan kehilangan task.

Dua keputusan bisnis yang sudah `approved` tetapi **belum memiliki desain** — `BKC-DEC-090` dan
`BKC-DEC-091` — sengaja **tidak** dibuatkan task pada roadmap ini. Membuatkan task untuk keputusan
yang desainnya belum ada berarti pelaksana yang akan merancangnya sambil menulis kode, dan itu
persis cara keputusan bisnis terbentuk tanpa pemiliknya. Keduanya dicatat sebagai `BKC-GAP-01` dan
`BKC-GAP-02`, dan menunggu pass desain tersendiri.

---

# Amendment 7 September 2026 — Invoice Asuransi (koreksi bukti) dan Struk Pasien (cross-check keputusan)

```yaml
roadmap_revision: 3
status: DRAFT_FORWARD_TEST
manifest_revision_field: 0.8 (belum dinaikkan — lihat BKC-GAP-03, masih terbuka)
manifest_narasi_revision: 0.9 (BKC-DES-026/027 approved 5 September 2026, BKC-GATE-06 tertutup)
```

## 1. Ketidaksesuaian revisi manifest — dikonfirmasi, sudah tercatat sebelumnya

`blueprint-manifest.md` field `revision: 0.8` tidak konsisten dengan badan dokumennya sendiri, yang
menyatakan `BKC-DES-026`–`027` (revisi `0.9`) **approved** 5 September 2026 dan `BKC-GATE-06`
**tertutup** (lihat `contract_versions.api`/`testing`, `artifact_hashes_note`, dan bagian "Catatan
revisi 0.7" di dalam manifest). **Ini bukan temuan baru** — `BKC-GAP-03` pada § 3 di atas sudah
mencatatnya persis: "Manifest berhenti di revisi `0.8`, sementara `02-backend-architecture.md`
sudah memuat revisi `0.9`". Status `BKC-GAP-03` **masih terbuka**; belum ada perbaikan sejak
dicatat. Isi revisi `0.9` (perluasan perutean jalur `NotCovered`, `BE-BKC-030`) **tidak
bersinggungan** dengan Invoice Asuransi maupun Struk Pasien — keduanya rumpun kemampuan berbeda
(residual/write-off vs dokumen cetak kasir). Kesimpulan: ketidaksesuaian ini **nyata**, **sudah
tercatat**, dan **tidak memblokir** perencanaan pada dua kemampuan yang dibahas amendment ini.

## 2. `BKC-GAP-08` (baru) — Laporan tracked `FE-BKC-018` tidak dapat diverifikasi terhadap source

| Requirement/decision | Design/contract | Backend | Frontend | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| Lembar "Invoice Asuransi" — tab dan cetak (`BKC-DEC-065`–`069`, `FR-BKC-014`–`020`) | `BKC-DES-001`–`009`; `BIL-API-0.5`/`0.6`, `BIL-VALIDATION-0.5` | `BE-BKC-023` | `FE-BKC-018` | `BIL-AT-031`–`035` | **Backend: `DONE`, terverifikasi ulang langsung 7 September 2026** — endpoint `GET .../invoices/{id}/insurance-invoice-document` dikonfirmasi ada di `BillingInvoicesController.cs` baris 303. **Frontend: status diturunkan ke belum-dikerjakan** — laporan `FE-BKC-018.md` mengklaim delapan berkas selesai dan lint/test/build lulus, tetapi grep menyeluruh `QuilvianSystemFrontendDev/src` untuk `INVOICE_ASURANSI`/`insuranceInvoiceDocument`/`invoice-asuransi-document.jsx` menghasilkan **nol match**, dan `git log`/`reflog`/`stash` pada branch `QuilvianIntegrationFrontend` (checked out) maupun `yasmina` (disebut laporan) sama-sama tidak menemukan commit berisi perubahan ini. Lihat `frontend-roadmap.md` § Amendment 7 September 2026 untuk tabel bukti lengkap |

`BKC-GAP-08`: laporan tracked frontend tidak sinkron dengan source pada tingkat yang tidak bisa
dijelaskan sebagai "belum di-commit biasa" — tidak ada jejak apa pun di reflog/stash. Dampak: setiap
klaim "source selesai, belum di-commit" pada laporan task frontend modul ini **tidak boleh
dipercaya tanpa verifikasi ulang langsung ke source**, bukan hanya dibaca dari laporannya. Owner:
pemilik repository frontend (audit lebih dalam ke sandbox/sesi mana pekerjaan itu ditulis berada di
luar kewenangan `plan-module-delivery`, yang bersifat read-only terhadap source aplikasi). Status:
task `FE-BKC-018` dikerjakan ulang dari nol lewat `build-module-frontend`; task lama tidak diberi
ID baru.

**Update 7 September 2026 (sesi berbeda, lanjutan)**: `FE-BKC-018` dikerjakan ulang lewat
`build-module-frontend` sesuai keputusan di atas. Source diverifikasi ADA di disk kali ini (branch
`QuilvianIntegrationFrontend`) — bukan diklaim dari laporan lama. `eslint . --quiet` (repo penuh)
PASS 0 error, `npm run test:unit` PASS 440/440 tanpa regresi. `npm run build` sengaja TIDAK
dijalankan pelaksana (pengguna eksplisit meminta menjalankannya sendiri pada giliran ini) — bukan
diklaim PASS tanpa dijalankan seperti temuan `BKC-GAP-08` sebelumnya. Verifikasi manual
ter-autentikasi masih menunggu. `BKC-GAP-08` dapat dianggap **tertutup untuk instance task ini**
(source kali ini nyata dan terverifikasi) — polanya (jangan percaya klaim "selesai" tanpa
verifikasi source langsung) tetap berlaku untuk laporan-laporan LAIN di modul ini yang belum
diperiksa ulang. Lihat `task/report/frontend/FE-BKC-018.md` (ditulis ulang).

## 3. Struk Pasien — cross-check keputusan per elemen (referensi PDF staging)

Konteks: pemilik modul menunjukkan PDF Struk Pasien dari lingkungan staging
(`staging.quilvian-mmchospital.com`) dengan format yang jauh lebih kaya dari implementasi lokal.
**PDF itu bukan bukti requirement yang locked** — ia dari environment/build berbeda, bukan dari
`00-interview-decisions.md`. Setiap elemen dicocokkan satu per satu:

| Elemen PDF staging | Decision ID yang dicek | Kutipan/hasil cross-check | Status |
| --- | --- | --- | --- |
| Item baris obat/tindakan/racikan/biaya admin (baseline) | `BKC-DEC-058`, acceptance criteria #26 | "Struk Pasien menampilkan rincian tagihan ... yang identik dengan tabel Tagihan Pasien pada Menu Pembayaran" — **sudah diimplementasikan dan sesuai**, dikonfirmasi langsung dari `struk-pasien-document.jsx` (tabel Item/Layanan/Qty/Harga/Total) | **Covered** — tidak perlu task |
| "Subtotal Mandiri" / "Subtotal Penjamin" / "Pajak (11%)" / "Harus Dibayar" berjenjang | `BKC-DEC-058` AC#26 (lingkupnya hanya tabel item); `BKC-DEC-062`, `070`–`077` (formula subtotal, tapi untuk Ringkasan Pembayaran **Menu Pembayaran**) | Formula dan field-nya **sudah** `approved` dan sudah diekspos backend (`GET /{id}/calculation-preview`, dipakai Menu Pembayaran) — tetapi **tidak ada satu keputusan pun** yang menugaskan breakdown ini muncul di dokumen Struk Pasien. `BKC-DEC-058` secara eksplisit membatasi kewajiban konsistensi Struk Pasien pada "tabel Tagihan Pasien" saja | **BLOCKED** — gap penempatan konten, bukan gap formula. Rekomendasi: `/grill-me` satu pertanyaan tertutup; risiko implementasi rendah begitu disetujui (field sudah ada) |
| `Penjamin` (nama penjamin korporat, mis. "Asuransi Korporasi PT Sentra Niaga Abadi") berdampingan `Asuransi` (nama provider, mis. "AXA Mandiri") | `BKC-DEC-067` | `BKC-DEC-067` mengunci Company Guarantor **di luar scope untuk Invoice Asuransi** — dokumen yang **berbeda** dari Struk Pasien. Tidak ada baris keputusan mana pun yang membahas field Company Guarantor pada Struk Pasien secara spesifik | **BLOCKED** — gap requirement murni; `BKC-DEC-067` tidak bisa ditarik untuk menjawab ini (beda dokumen) |
| QR code "Scan untuk verifikasi pembayaran" | — (dicari di seluruh `00-interview-decisions.md`) | Nol kemunculan | **BLOCKED** — tidak pernah dibahas sama sekali; perlu digali dari nol (verifikasi ke sistem apa, generate oleh siapa, isi payload QR) |
| Blok tanda tangan "Kasir" / "Penerima" | — (dicari di seluruh `00-interview-decisions.md`) | Nol kemunculan untuk Struk Pasien. Satu-satunya "blok tanda tangan" yang disinggung ada pada amendment Invoice Asuransi (`BKC-DEC-069`, ditandai `DEV_DISCRETION` untuk dokumen **itu**) | **BLOCKED** — tidak pernah dibahas untuk Struk Pasien; layout tanda tangan Invoice Asuransi tidak otomatis berlaku di sini |

Task roadmap yang menampung ini: `FE-BKC-022` (`frontend-roadmap.md` § Amendment 7 September
2026), berstatus `BLOCKED` penuh — keempat elemen baru semuanya butuh keputusan yang belum ada,
dan tidak ada pekerjaan yang bisa dimulai sebagian.

`BKC-GAP-09` s.d. `BKC-GAP-12` (baru, satu per elemen blocked di atas, berurutan sesuai tabel):
Owner Product/Domain Owner; aksi: jawab lewat `/grill-me` sebelum `FE-BKC-022` dapat direncanakan
lebih lanjut. Tidak satu pun dari keempatnya boleh diputuskan sepihak oleh agent perencana maupun
builder — ini keputusan konten dokumen finansial yang dibaca pasien dan (untuk sebagian elemen)
pihak asuransi.

**Status baris di atas sudah usang — lihat § 5 di akhir berkas ini.** `BKC-GAP-09`, `10`, `12`
ditutup 7 September 2026 (`BKC-DEC-093`,`094`,`096`); `BKC-GAP-11` (QR) ditutup sebagai
keputusan "ditunda" (`BKC-DEC-095`), bukan dibiarkan menggantung. Isi di atas dipertahankan
sebagai riwayat, bukan status terkini.

---

# Amendment 7 September 2026 (kedua) — Traceability rumpun baru: Petty Cash (Voucher Kas Kecil)

```yaml
roadmap_revision: 4
status: DRAFT_FORWARD_TEST
blueprint_revision_dibaca: 1.0 (blueprint-manifest.md, status approved untuk seluruh revisi
  termasuk Petty Cash; readiness DESIGN_APPROVED, kontrak terkunci)
scope_pass_ini: HANYA rumpun Petty Cash. Baris tabel di atas (rumpun revisi 0.6-0.9) TIDAK diubah
  dan TIDAK dinilai ulang — manifest menandai bukti as-is rumpun tersebut stale terhadap dd31bc9,
  menuntut trace-existing-capabilities impact scan tersendiri sebelum re-planning, di luar
  cakupan pass ini. Manifest secara eksplisit MENGONFIRMASI Petty Cash tidak menyentuh satu pun
  file/tabel milik rumpun-rumpun tersebut dan TIDAK perlu menunggu scan itu (blueprint-manifest.md
  § "Kemandirian rumpun ini terhadap pertanyaan terbuka sebelumnya")
decision_revision: PC-DEC-001–015 (approved 7 September 2026)
design_decisions: PC-DES-001–014 (approved lewat PC-DEC-015)
backend_source: dd31bc91818566c0b53e1b68c0129f5a6cf01a2b
frontend_source: 12f9242ce62e4d80dbdb719f80bb0e7a2848474c
```

## 1. Pemetaan requirement ke delivery

| Requirement/decision | Design/contract | Backend | Frontend | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| Siklus hidup voucher lima status (`EPIC BKC-10`; `FR-BKC-045`,`047`–`053`; `PC-DEC-001`,`003`–`009`,`011`,`013`) | `PC-DES-001`,`003`,`005`–`013`; `BIL-API-0.9`, `BIL-VALIDATION-0.8`, `BIL-STATE-0.8` | 🟡 `BE-BKC-033`, `034`, ✅ `035`,`036`,`037` | `FE-BKC-023`,`024`,`025` | `BIL-AT-064`,`065`,`066`,`067`–`075`; `UAT-28`–`33`,`35`,`42` | 🟡 **SEBAGIAN — 8 September 2026, diperbarui setelah verifikasi build/test.** `BE-BKC-035`,`036`,`037` **terverifikasi ✅** (`dotnet build` LULUS, `dotnet test` 12+19+17 = 48/48 LULUS, 8 September 2026). `BE-BKC-034` juga lulus build/test (11/11) sesi yang sama tetapi tetap 🟡 tanpa laporan tracked tersendiri. `BE-BKC-033` tetap 🟡 menyangkut bukti seed database dan review Finance. Frontend: `FE-BKC-023` 🟡 SEBAGIAN (8 September 2026) — source selesai, lint/test:unit/build lulus, menunggu verifikasi manual ter-autentikasi. `FE-BKC-024` 🟡 SEBAGIAN (8 September 2026) — mengisi tombol "+ Buat Voucher"/"Input Nota" yang dikecualikan `FE-BKC-023`; lint/test/build **sengaja tidak dijalankan** sesi ini atas permintaan eksplisit pengguna (verifikasi manual oleh pengguna sendiri) — bukti hanya tinjauan kode statis. `FE-BKC-025` 🟡 SEBAGIAN (8 September 2026) — modal detail voucher (`GET /{id}`, klik dua kali pada baris) selesai ditulis; lint/test/build **sengaja tidak dijalankan** sesi ini atas permintaan eksplisit pengguna — bukti hanya tinjauan kode statis. Dengan ini `FE-PC-01`–`04` (`MVP-15`) lengkap secara source, seluruhnya menunggu verifikasi manual ter-autentikasi oleh pengguna. Bukti: [`BE-BKC-033`](../task/report/backend/BE-BKC-033.md), [`be-bkc-035`](../task/report/backend/be-bkc-035-master-data-kategori-petty-cash.md), [`be-bkc-036`](../task/report/backend/be-bkc-036-kolam-anggaran-dan-saldo-berjalan.md), [`be-bkc-037`](../task/report/backend/be-bkc-037-siklus-hidup-voucher-petty-cash.md), [`FE-BKC-023`](../task/report/frontend/FE-BKC-023.md), [`FE-BKC-024`](../task/report/frontend/FE-BKC-024.md), [`FE-BKC-025`](../task/report/frontend/FE-BKC-025.md) |
| Nomor voucher unik dan aman di bawah pemakaian bersamaan (`FR-BKC-046`; `PC-DES-008`; `CAP-30`) | `PC-DES-008` | `BE-BKC-034` | — | `BIL-AT-065`,`066` | **Direncanakan pass ini.** `EXTEND` atas mekanisme `BilNumberSeries` yang sudah `Ready to reuse` |
| Kolam anggaran, saldo berjalan, dan kedua penjaga saldo (`EPIC BKC-11`; `FR-BKC-054`–`060`; `PC-DEC-002`,`008`–`010`) | `PC-DES-004`–`006`,`011`,`014`; `BIL-VAL-047`,`048`,`054`,`055`,`057` | ✅ `BE-BKC-036`, `037` | `FE-BKC-023`,`026` | `BIL-AT-067`,`069`,`070`,`071`,`077`,`080`; `UAT-34`,`36`–`38` | ✅ **SELESAI — 8 September 2026.** `BE-BKC-036` (`GET /current`, `GET /movements`, `POST /top-ups`, `POST /adjustments`, `CalculateReservedAmountAsync`, `ApplyDisbursementAsync`) dan `BE-BKC-037` (`ApproveAsync` memanggil penjaga `BIL-VAL-047`; `DisburseAsync` memanggil `ApplyDisbursementAsync` — `ApplyDisbursementAsync` kini **punya pemanggil nyata**) selesai ditulis dan **terverifikasi**: `dotnet build` LULUS, `dotnet test` 19+17 = 36/36 LULUS, termasuk `BIL-VAL-054` (koreksi turun), `BIL-VAL-048` (pencairan saldo tidak cukup — diuji lewat koreksi saldo di luar `AdjustAsync`, lihat laporan `BE-BKC-037` § 13), dan `BIL-VAL-047` (contoh berangka persis kontrak). `FR-BKC-060` (kas kecil tidak menyentuh shift kasir) diuji lewat `BIL-AT-077` pada `BE-BKC-038` (✅, lihat baris berikutnya), bukan pada task ini. Frontend: `FE-BKC-026` 🟡 SEBAGIAN (8 September 2026) — layar Anggaran Kas Kecil (kartu saldo, Tambah Anggaran, Koreksi Saldo, riwayat pergerakan) selesai ditulis; lint/test/build **sengaja tidak dijalankan** sesi ini atas permintaan eksplisit pengguna — bukti hanya tinjauan kode statis. Bukti: [`be-bkc-036-kolam-anggaran-dan-saldo-berjalan`](../task/report/backend/be-bkc-036-kolam-anggaran-dan-saldo-berjalan.md), [`be-bkc-037-siklus-hidup-voucher-petty-cash`](../task/report/backend/be-bkc-037-siklus-hidup-voucher-petty-cash.md), [`FE-BKC-026`](../task/report/frontend/FE-BKC-026.md) |
| Data induk kategori pengeluaran (`EPIC BKC-12`; `FR-BKC-061`–`063`; `PC-DEC-012`; `CAP-31`) | `PC-DES-002` | ✅ `BE-BKC-035` | `FE-BKC-027` | `BIL-AT-076`; `UAT-39`–`41` | 🟡 **SEBAGIAN — 8 September 2026, diperbarui setelah `FE-BKC-027`.** Backend `BE-BKC-035` **tetap ✅ terverifikasi** (`dotnet build` LULUS, `dotnet test` 12/12 LULUS; nama entity `MstPettyCashCategory` mengikuti penutupan `PC-OQ-001`/`PC-DEC-014`). Frontend: `FE-BKC-027` 🟡 SEBAGIAN (8 September 2026) — bentuk penuh tujuh-berkas (list/detail/editor) selesai ditulis mengikuti `master-data-feature-standard.md`, `categoryCode` diketik Finance saat tambah dan terkunci saat ubah, sembilan thunk memetakan sembilan endpoint; lint/test/build **sengaja tidak dijalankan** sesi ini atas permintaan eksplisit pengguna — bukti hanya tinjauan kode statis, verifikasi manual belum dilakukan. Bukti: [`be-bkc-035-master-data-kategori-petty-cash`](../task/report/backend/be-bkc-035-master-data-kategori-petty-cash.md), [`FE-BKC-027`](../task/report/frontend/FE-BKC-027.md) |
| Hak akses, privasi, dan regresi kas shift kasir (seluruh `PC-DEC-001`–`013`; `PC-DES-001`–`014`) | Seluruh kontrak Petty Cash | ✅ `BE-BKC-038` | — | `BIL-AT-064`–`080` penuh | ✅ **SELESAI — 8 September 2026.** Dua cacat ditemukan dan diperbaiki: audit log yang sebelumnya tidak benar-benar tercetak (`BIL-AT-079`), dan regresi `NullReferenceException` pada replay idempotency akibat perbaikan itu. `dotnet build` LULUS; `dotnet test` 89/89 LULUS (gabungan Petty Cash + Number Series + Access Permission + Cashier Shift), termasuk konfirmasi ulang setelah kedua perbaikan. `BIL-AT-072`,`077`,`078`,`079` `Covered` baru sesi ini; `BIL-AT-071`,`080` `Partial` (concurrency/unique-index hanya dapat dibuktikan provider relational, § 6 laporan). Bukti: [`be-bkc-038-hardening-lintas-slice-petty-cash`](../task/report/backend/be-bkc-038-hardening-lintas-slice-petty-cash.md) |

## 2. Cakupan acceptance test

| Test | Task utama | Jalur gagal yang tercakup |
| --- | --- | --- |
| `BIL-AT-064` | `BE-BKC-037` | Status berpindah tidak sesuai urutan; label tampilan menyimpang dari `PC-DEC-013` |
| `BIL-AT-065`, `066` | `BE-BKC-034` | Nomor berbasis milidetik epoch; nomor kembar atau terlewat di bawah pemakaian bersamaan |
| `BIL-AT-067`, `068` | `BE-BKC-036`, `037` | Saldo berkurang saat persetujuan alih-alih saat pencairan; saldo bergerak saat nota dimasukkan |
| `BIL-AT-069`, `070` | `BE-BKC-036`, `037` | Persetujuan diuji terhadap saldo kotor, bukan sisa bebas; pencairan tidak diperiksa ulang saat itu juga |
| `BIL-AT-071` | `BE-BKC-037` | Tombol tertekan dua kali menyerahkan uang dua kali |
| `BIL-AT-072` | `BE-BKC-037` | Voucher ditolak masih dapat diubah lewat jalur mana pun |
| `BIL-AT-073` | `BE-BKC-037` | Pembatalan oleh bukan-pemohon; pembatalan setelah diputuskan; lahirnya status `CANCELLED` |
| `BIL-AT-074`, `075` | `BE-BKC-037` | Status antara yang tidak diminta; eskalasi otomatis bukti nota yang sengaja ditunda |
| `BIL-AT-076` | `BE-BKC-035` | Kategori terpakai terhapus; kategori nonaktif masih ditawarkan |
| `BIL-AT-077` | `BE-BKC-038` | Kas shift kasir ikut berkurang akibat pencairan voucher — **uji regresi paling penting** |
| `BIL-AT-078` | `BE-BKC-038` | Argumen `[AccessAction]`/`[AccessPermission]` tidak sama persis; akses tanpa butir hak lolos |
| `BIL-AT-079` | `BE-BKC-038` | Data sensitif (`RecipientName`, `Purpose`, `RejectionReason`) bocor ke log |
| `BIL-AT-080` | `BE-BKC-036` | Kolam aktif kedua lolos; koreksi saldo mengingkari komitmen yang sudah disetujui |

## 3. Coverage gap dan catatan non-blocking

| ID | Catatan | Dampak | Owner | Status/aksi |
| --- | --- | --- | --- | --- |
| `PC-OQ-003` | Baris registry `HealthServices / BillingManagement / Billing` belum ditegaskan mencakup submodule `PettyCash/` (`QBE-MOD-003`) | **Memblokir hanya penulisan file model pertama** pada `BE-BKC-033`; **tidak memblokir** roadmap, task lain, maupun approval task manapun. Dicatat sebagai dependency tingkat task pada `BE-BKC-033` (`backend-roadmap.md`), **bukan** sebagai roadmap blocker | Pemilik arsitektur backend | **Ditutup 7 September 2026** pada `BE-BKC-033`. Baris registry yang ada dinyatakan mencakup `PettyCash/`: pemiliknya sudah terdaftar (`Bil` `ACTIVE`), sehingga prosedur registry langkah 2 berlaku; dan submodule sekerabat `Cashier/` serta `Operational/` juga memuat model `Bil*` tanpa baris tersendiri. Tidak ada prefix baru. **Tidak** `BLOCKED BY QBE-MOD-002`. Sisa kerapian non-blocking: menambahkan nama folder `PettyCash` ke kolom Module/pemilik pada registry — berkas suite skill, di luar wewenang tulis task backend. Bukti: [`BE-BKC-033`](../task/report/backend/BE-BKC-033.md) |
| `PC-OQ-004` | Pengaju voucher boleh menyetujui pengajuannya sendiri — `PC-DEC-004` menetapkan satu jenjang tanpa pemeriksaan dua orang, berbeda dari write-off | Risiko administratif murni; mitigasi sekarang adalah admin **SHOULD NOT** memberikan `Create` dan `Approve` kepada Posisi yang sama | Product/Domain Owner + Kepala Kasir/Finance Operations | Terbuka, tidak memblokir. Dicatat pada `BE-BKC-037` sebagai risiko yang diketahui, **bukan** diselesaikan sepihak oleh task ini |
| `PC-OQ-005` | Prefix dan kebijakan reset nomor voucher (`PTC`, `DAILY`, 4 digit) adalah bawaan desain, belum dikonfirmasi eksplisit Finance | Nilai konfigurasi, dapat diubah tanpa menyentuh kode | Product/Domain Owner + Finance | Terbuka, tidak memblokir |
| `PC-OQ-002` | Koreksi nomor nota pada voucher `Selesai` — apakah boleh petugas yang sama atau perlu wewenang lebih tinggi | Bawaan desain: petugas yang sama, setiap koreksi tercatat pada `BilPettyCashVoucherCommand` | Kepala Kasir/Finance Operations | Terbuka, tidak memblokir |
| `BKC-GAP-13` (baru) | `blueprint-manifest.md` field `revision: 1.0` sudah dinaikkan dan konsisten dengan badan dokumennya untuk Petty Cash, **tetapi** § "Trace dan approval" pada `02-backend-architecture.md`/`03-frontend-architecture.md` masih mencantumkan baris `Status: draft` di ujung bagian Petty Cash — tertinggal dari `PC-DEC-015` yang ditulis pada amendment berikutnya di `00-interview-decisions.md` pada tanggal yang sama | Pembaca yang hanya membaca ujung `02-backend-architecture.md`/`03-frontend-architecture.md` tanpa membaca `blueprint-manifest.md` atau `00-interview-decisions.md` amendment kedua dapat keliru menyimpulkan `PC-DES-001`–`014` belum disetujui | Pemilik blueprint | Perapian baris "Status" pada kedua dokumen arsitektur adalah revisi tersendiri milik `/manage-module-blueprint`; **tidak memblokir** roadmap ini — manifest (rollup yang lebih baru) sudah eksplisit menyatakan `status: approved`/`readiness: DESIGN_APPROVED` untuk Petty Cash |

## 4. Pemeriksaan requirement yatim

Seluruh functional requirement `FR-BKC-045` sampai `FR-BKC-063` (`EPIC BKC-10`, `BKC-11`, `BKC-12`)
memiliki sedikitnya satu task backend dan, untuk kemampuan yang tampil di layar, satu task
frontend. Tidak ada requirement Petty Cash yang `approved` dan kehilangan task.

`FR-BKC-060` ("kas kecil tidak menyentuh kas shift kasir") sengaja **tidak** mendapat task
tersendiri — ia adalah invariant lintas-slice yang hanya dapat dibuktikan setelah seluruh alur
voucher dan budget berjalan, sehingga ditempatkan sebagai acceptance capstone `BE-BKC-038`
(`BIL-AT-077`), bukan sebagai requirement yang terlewat.

Tidak ada satu pun `PC-DEC-*` maupun `PC-DES-*` yang berstatus selain `approved` pada pass ini —
berbeda dari gelombang `MVP-4`–`MVP-12` yang sempat menyisakan `BKC-GAP-01`/`02` (keputusan
`approved` tanpa desain). Petty Cash tidak mewarisi kondisi itu: keputusan bisnis dan desainnya
disetujui pada sesi desain yang sama, 7 September 2026.

---

# Amendment 8 September 2026 — `FE-BKC-022` (Struk Pasien) dibuka kembali

```yaml
roadmap_revision: 5
status: DRAFT_FORWARD_TEST
pemicu: /plan-module-delivery menemukan BKC-DEC-093-096 approved 7 September 2026 tapi
  frontend-roadmap.md § 2 dan requirement-traceability.md § 3 (di atas) masih menulis BLOCKED
scope_pass_ini: HANYA FE-BKC-022/Struk Pasien. Tabel § 1 (Petty Cash) dan seluruh baris
  rumpun lain TIDAK disentuh dan TIDAK dinilai ulang pass ini
```

## 5. `FE-BKC-022` — status diperbarui dari `BLOCKED` menjadi `READY_FOR_TASK_APPROVAL`

Definisi task lengkap ada di `frontend-roadmap.md` § "Amendment 8 September 2026 —
`FE-BKC-022` dibuka kembali sebagai `READY_FOR_TASK_APPROVAL`". Ringkasan cross-check § 3 di
atas, diperbarui:

| Elemen PDF staging | Decision ID | Status (7 September) | Status (8 September, pass ini) |
| --- | --- | --- | --- |
| Item baris (baseline) | `BKC-DEC-058` | Covered | Covered — tidak berubah |
| Breakdown Subtotal/Pajak/Harus Dibayar | `BKC-DEC-093` | BLOCKED (gap keputusan) | **READY** — keputusan `approved`; kontrak `GET /{id}/calculation-preview` diverifikasi ulang langsung ke source pass ini, tidak ada field baru dibutuhkan |
| Field Penjamin | `BKC-DEC-094` | BLOCKED (gap keputusan) | **READY, dengan koreksi bukti** — keputusan `approved`, TAPI premis "berdampingan dengan field Asuransi yang sudah ada" salah (tidak ada field Asuransi hari ini; backend hanya expose SATU field payer per kunjungan, bukan dua sekaligus). Lihat `frontend-roadmap.md` § 1 amendment ini untuk detail lengkap dan kenapa ini tidak membatalkan keputusannya |
| QR verifikasi | `BKC-DEC-095` | BLOCKED (belum dibahas) | **Ditunda secara sadar** — bukan lagi gap, keputusan eksplisit "jangan dibangun sekarang". Tidak ada task |
| Blok tanda tangan | `BKC-DEC-096` | BLOCKED (gap keputusan) | **READY** — keputusan `approved`, `DEV_DISCRETION` untuk detail layout, tidak ada kontrak data yang dibutuhkan |

`BKC-GAP-09`, `10`, `12` dinyatakan **DITUTUP** (bukan sekadar dijawab — implementasinya
sudah dapat direncanakan penuh, tidak ada sisa pertanyaan turunan). `BKC-GAP-11` **DITUTUP
sebagai keputusan ditunda**, tetap tidak punya task.

**Coverage gap yang tersisa (non-blocking, dicatat apa adanya):** `04-prd-to-mvp.md` belum
punya Epic/FR/UAT untuk `BKC-DEC-093`–`096` — slice ini disusulkan ke Dokumen Kasir
(`FE-BKC-011`/`017`/`018`) yang sudah lama melewati PRD aslinya. Penulisan Epic/FR/UAT formal
adalah pekerjaan `design-business-module`, bukan `plan-module-delivery`; tidak dianggap
memblokir `FE-BKC-022` karena acceptance criteria pada task itu sendiri sudah cukup spesifik
dan dapat diuji tanpa menunggu penomoran PRD.

**Tidak ada task backend baru pada amendment ini.** Kedua kontrak yang dipakai `FE-BKC-022`
sudah live di backend saat ini (diverifikasi langsung ke source `BillingInvoiceService.cs`
dan `BillingInvoiceDtos.cs`, bukan diasumsikan dari dokumen kontrak) — `backend-roadmap.md`
sengaja tidak disentuh amendment ini.

### Update 8 September 2026 (lanjutan, sesi berbeda) — `FE-BKC-022` dieksekusi, 🟡 SEBAGIAN

`FE-BKC-022` dikerjakan lewat `build-module-frontend` pada sesi yang sama hari ini. Source
ditulis (`billing-invoice-calculation-breakdown.js` baru, `menu-pembayaran-view.jsx`,
`use-dokumen-kasir-page.js`, `struk-pasien-document.jsx` diubah), `eslint` (quiet dan full
severity) PASS 0 error/warning, unit test baru (5 skenario rumus breakdown) PASS, `next build`
exit 0. Detail penuh di [`task/report/frontend/FE-BKC-022.md`](../task/report/frontend/FE-BKC-022.md).

Status **tidak** dinaikkan ke `✅` — verifikasi manual ter-autentikasi (klik-coba nyata dengan
invoice tunai/asuransi/penjamin sungguhan, mencocokkan angka breakdown terhadap Ringkasan
Pembayaran) belum dijalankan (`NOT FEASIBLE` pada sesi ini, tidak ada kredensial). Kriteria
acceptance 1 (breakdown identik Ringkasan Pembayaran) baru terbukti lewat unit test rumus,
belum lewat pengamatan layar nyata.

Koreksi bukti `BKC-DEC-094` yang dicatat pada amendment 8 September (pertama) di atas
**terkonfirmasi** saat implementasi: `struk-pasien-document.jsx` sebelum task ini benar-benar
tidak punya field Asuransi apa pun, dan `TrxPatientEncounterGuarantor.cs`
(`NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Models/`) memang
mengunci relasi satu-ke-satu payer per kunjungan. Implementasi mengikuti bentuk yang sudah
dikoreksi (satu baris berlabel dinamis), bukan bentuk asli `BKC-DEC-094`.

> **Koreksi penamaan, 11 September 2026.** Nama `TrxPatientEncounterGuarantor` pada paragraf di
> atas sudah **tidak berlaku** di source. Entity itu telah di-rename menjadi
> `RegPatientEncounterGuarantor` mengikuti registry kepemilikan prefix. Isi kesimpulannya tetap
> benar — relasi satu-ke-satu per kunjungan memang dikunci di sana — hanya namanya yang berubah.
> Ditemukan saat wawancara rumpun Edit Tagihan & Multi-Payer dan dicatat pada
> [`00-interview-decisions.md`](../00-interview-decisions.md) § Catatan koreksi.

---

# Amendment 11 September 2026 — Rumpun Edit Tagihan & Multi-Payer Coverage

```yaml
roadmap_revision: 5
blueprint_revision: 1.1
status: READY_FOR_TASK_APPROVAL
backend_commit_sha: d295c4d59b68d223edc597c8b165b7ef4282b49f
frontend_commit_sha: 0eafa76bf397a47ceb9d44a6f69006ee25f8ba51
input_hash_00_interview_decisions: 6d74e4fbe954782895df0c89d441b4a8357dac38e20f945224778ed8440c3414
input_hash_01_capability_map: b33911fb655fb71beda296affdc2a8980f0c5f0fd05dd4d01301e30c05a866e8
approval_keputusan_bisnis: MPY-DEC-001-012, seluruhnya approved 11 September 2026
approval_keputusan_arsitektur: MPY-DES-001-017, approved lewat MPY-DEC-012
contracts: [BIL-API-1.0, BIL-STATE-0.9, BIL-VALIDATION-0.9, BIL-INTEGRATION-0.8,
  BIL-PERMISSION-0.8, BIL-TEST-1.0, BIL-CALCULATION-0.9, MPY-ENC-PAYER-001]
```

## 1. Requirement sampai bukti

| Requirement | Keputusan | Desain | Kontrak | Task backend | Task frontend | Bukti | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `FR-BKC-064` Aturan tanggungan perusahaan menentukan porsi penjamin | `MPY-DEC-008` | `MPY-DES-006` | `BIL-CALCULATION-0.9` | ✅ `BE-BKC-043`, ✅ `BE-BKC-044` | `FE-BKC-033` | `BIL-AT-100` | Selesai (backend) — master aturan tanggungan (`BE-BKC-043`) dan mesin kalkulasi `CompanyGuarantorCoverageService` (`BE-BKC-044`) selesai dan terverifikasi build |
| `FR-BKC-065` Peringatan palsu pada kunjungan berpenjamin perusahaan dihapus | `MPY-DEC-008` | `MPY-DES-007` | `BIL-CALCULATION-0.9` | ✅ `BE-BKC-044` | — | `BIL-AT-100`, `UAT-54` | Selesai (backend) — `RegistrationBillingCoverageAdapter` diperbaiki menjadi dispatcher, anomali palsu `INSURANCE_PROVIDER_MISSING` dihapus pada `BE-BKC-044` |
| `FR-BKC-066` Urun biaya diturunkan server | `MPY-DEC-008` | `MPY-DES-006` | `BIL-VALIDATION-0.9` | ✅ `BE-BKC-043` | `FE-BKC-033` | `BIL-AT-099` | Selesai (backend) — derivasi server-side urun biaya selesai diimplementasikan pada `BE-BKC-043` |
| `FR-BKC-067` Aturan bermasa berlaku dan berprioritas | `MPY-DEC-008` | `MPY-DES-006` | `BIL-VALIDATION-0.9` | ✅ `BE-BKC-043`, ✅ `BE-BKC-044` | `FE-BKC-033` | `BIL-AT-099` | Selesai (backend) — master aturan tanggungan (`BE-BKC-043`) dan mesin kalkulasi berprioritas/masa berlaku (`BE-BKC-044`) selesai dan lulus build |
| `FR-BKC-068` Penanggung kunjungan dapat diganti sebelum pembayaran | `MPY-DEC-003`, `MPY-DEC-007` | `MPY-DES-001`, `MPY-DES-004` | `BIL-API-1.0`, `MPY-ENC-PAYER-001` | ✅ `BE-BKC-045`, ✅ `BE-BKC-047` | `FE-BKC-028` | `BIL-AT-081`, `UAT-43` | Selesai (backend) — kontrak serah terima `MPY-ENC-PAYER-001` (`BE-BKC-045`) dan orkestrator ganti payer (`BE-BKC-047`) selesai |
| `FR-BKC-069` Kunjungan tetap punya tepat satu penanggung | `MPY-DEC-001` | `MPY-DES-002` | `MPY-ENC-PAYER-001` | ✅ `BE-BKC-045`, ✅ `BE-BKC-047` | — | `BIL-AT-082` | Selesai (backend) — invariant satu penanggung dikunci di `MPY-ENC-PAYER-001` (`BE-BKC-045`) dan orkestrator ganti payer (`BE-BKC-047`) selesai |
| `FR-BKC-070` Perbandingan sebelum mengganti tidak menyimpan apa pun | `MPY-DEC-003` | `MPY-DES-005`, `MPY-DES-015` | `BIL-API-1.0` | ✅ `BE-BKC-046`, ✅ `BE-BKC-047` | `FE-BKC-028` | `BIL-AT-087` | Selesai (backend) — mesin evaluasi payer kandidat tanpa efek samping (`BE-BKC-046`) dan endpoint `POST /{id}/payer-comparison-preview` (`BE-BKC-047`) selesai |
| `FR-BKC-071` Kartu tidak sah ditolak beserta sebabnya | `MPY-DEC-003` | `MPY-DES-004` | `BIL-VALIDATION-0.9`, `MPY-ENC-PAYER-001` | ✅ `BE-BKC-045`, ✅ `BE-BKC-047` | `FE-BKC-028` | `BIL-AT-083`, `BIL-AT-084`, `UAT-44` | Selesai (backend) — aturan penolakan kartu dikunci di `MPY-ENC-PAYER-001` (`BE-BKC-045`) dan gerbang validasi ganti payer (`BE-BKC-047`) selesai |
| `FR-BKC-072` Penanggung baris yang tidak lagi sah dikembalikan otomatis | `MPY-DEC-004` | `MPY-DES-009` | `BIL-STATE-0.9` | ✅ `BE-BKC-047` | `FE-BKC-028` | `BIL-AT-092`, `UAT-48` | Selesai (backend) — penanggung baris biaya yang tidak lagi sah direset otomatis menjadi `CASH`/`AUTO` pada `BE-BKC-047` |
| `FR-BKC-073` Setiap penggantian meninggalkan jejak yang tidak dapat dihapus | `MPY-DEC-005` | `MPY-DES-003` | `BIL-PERMISSION-0.8` | ✅ `BE-BKC-041`, ✅ `BE-BKC-047` | — | `BIL-AT-088` | Selesai (backend) — tabel `BilInvoicePayerChangeCommand` (`BE-BKC-041`) dan pencatatan jejak audit tak terhapus (`BE-BKC-047`) selesai |
| `FR-BKC-074` Penanggung baris dapat diubah sebelum pembayaran | `MPY-DEC-004` | `MPY-DES-008` | `BIL-API-1.0` | ✅ `BE-BKC-048` | `FE-BKC-029` | `BIL-AT-089`, `UAT-46` | Selesai (backend) — endpoint `PUT /{id}/item-payer-assignments` dan mutasi append-only selesai pada `BE-BKC-048` |
| `FR-BKC-075` Pilihan penanggung mengikuti penanggung kunjungan | `MPY-DEC-004` | `MPY-DES-008` | `BIL-VALIDATION-0.9` | ✅ `BE-BKC-048` | `FE-BKC-029` | `BIL-AT-091`, `UAT-47` | Selesai (backend) — validasi `BIL-VAL-077` dan `BIL-VAL-078` menegakkan kesesuaian penanggung dengan kunjungan pada `BE-BKC-048` |
| `FR-BKC-076` Penandaan tidak digerbang hasil tanggungan | `MPY-DEC-004` | `MPY-DES-008` | `BIL-VALIDATION-0.9` | ✅ `BE-BKC-048` | `FE-BKC-029` | `BIL-AT-090` | Selesai (backend) — penandaan tidak digerbang hasil tanggungan; baris tidak tertanggung tetap boleh ditandai dan hasilnya nol tertanggung pada `BE-BKC-048` |
| `FR-BKC-077` Angka tagihan selalu menjumlah | `MPY-DEC-004` | `MPY-DES-017` | `BIL-CALCULATION-0.9` | ✅ `BE-BKC-048` | — | `BIL-AT-089` | Selesai (backend) — mesin kalkulasi menjumlah utuh komponen tagihan nol selisih pada `BE-BKC-048` |
| `FR-BKC-078` Obat yang tidak ditebus tidak ditagihkan | `MPY-DEC-009` | `MPY-DES-010` | `BIL-API-1.0` | ✅ `BE-BKC-049` | `FE-BKC-030` | `BIL-AT-094`, `UAT-49` | Selesai (backend) — endpoint `PUT /{id}/drug-billing-disposition` dan pengeluaran obat `EXCLUDED` dari kalkulasi selesai pada `BE-BKC-049` |
| `FR-BKC-079` Jumlah pada baris obat tidak pernah berubah | `MPY-DEC-009` | `MPY-DES-010` | `BIL-VALIDATION-0.9` | ✅ `BE-BKC-049` | `FE-BKC-030` | `BIL-AT-094` | Selesai (backend) — kuantitas obat pada `BilInvoiceItem` tidak diubah sama sekali; mutasi append-only disposisi selesai pada `BE-BKC-049` |
| `FR-BKC-080` Rawat inap ditolak, IGD diterima | `MPY-DEC-009` | `MPY-DES-011` | `BIL-VALIDATION-0.9` | ✅ `BE-BKC-049` | `FE-BKC-030` | `BIL-AT-095`, `UAT-50`, `UAT-51` | Selesai (backend) — validasi `BIL-VAL-081` menolak RANAP dan menerima IGD/RAJAL pada `BE-BKC-049` |
| `FR-BKC-081` Catatan penyerahan obat tidak tersentuh | `MPY-DEC-009` | `MPY-DES-010` | `BIL-INTEGRATION-0.8` | ✅ `BE-BKC-049` | — | `BIL-AT-096` | Selesai (backend) — isolasi data Farmasi dipatuhi penuh (0 baris Farmasi disentuh) pada `BE-BKC-049` |
| `FR-BKC-082` Lembar tagihan perusahaan dapat dicetak | `MPY-DEC-006` | `MPY-DES-013` | `BIL-API-1.0` | ✅ `BE-BKC-050` | `FE-BKC-031` | `UAT-53` | Selesai (backend) — endpoint `GET /{id}/company-guarantor-invoice-document` dan service penyusunan dokumen selesai pada `BE-BKC-050` |
| `FR-BKC-083` Rute penggantian biaya tampil sebagai keterangan | `MPY-DEC-008` | `MPY-DES-014` | `BIL-API-1.0` | ✅ `BE-BKC-042`, ✅ `BE-BKC-050` | `FE-BKC-032`, `FE-BKC-031` | `UAT-53` | Selesai (backend) — master rute reimbursement (`BE-BKC-042`) dan pemuatan metadata keterangan rute pada dokumen penjamin (`BE-BKC-050`) selesai |
| `FR-BKC-084` Lembar khusus kunjungan berpenjamin perusahaan | `MPY-DEC-006` | `MPY-DES-013` | `BIL-API-1.0` | ✅ `BE-BKC-050` | `FE-BKC-031` | `UAT-53` | Selesai (backend) — penolakan terbit untuk tunai dan asuransi pribadi serta verifikasi penjamin perusahaan selesai pada `BE-BKC-050` |
| `FR-BKC-085` Ketiga koreksi tertutup sesudah pembayaran | `MPY-DEC-005` | `MPY-DES-016` | `BIL-STATE-0.9` | ✅ `BE-BKC-047`, ✅ `BE-BKC-048`, ✅ `BE-BKC-049`, ✅ `BE-BKC-051` | `FE-BKC-028` | `BIL-AT-085`, `UAT-45` | Selesai (backend) — gerbang status `OPEN` dan nol pembayaran (`BIL-VAL-059`–`060`) dikunci rapat dan terverifikasi penuh pada `BE-BKC-051` |
| `FR-BKC-086` Perubahan bersifat sekaligus atau tidak sama sekali | `MPY-DEC-005` | `MPY-DES-016` | `BIL-API-1.0` | ✅ `BE-BKC-047`, ✅ `BE-BKC-051` | — | `BIL-AT-086`, `UAT-52` | Selesai (backend) — batas transaksi serializable atomik dan rollback total saat konflik concurrency/validasi terverifikasi pada `BE-BKC-051` |

## 2. Kemampuan asal ke task

| Kemampuan | Status audit | Task yang memakainya |
| --- | --- | --- |
| `CAP-33` Kapabilitas ubah payer encounter | **Missing** | ✅ `BE-BKC-045` (kontrak serah terima selesai), ✅ `BE-BKC-047` (diimplementasikan via `EncounterPaymentSourceService`) |
| `CAP-34` Mesin tanggungan terikat satu payer | Reuse with adapter | ✅ `BE-BKC-044`, ✅ `BE-BKC-046` |
| `CAP-35` Master perusahaan dan kartu karyawan | Ready to reuse | ✅ `BE-BKC-042`, ✅ `BE-BKC-047` |
| `CAP-36` Aturan tanggungan asuransi sebagai cetakan | Ready to reuse as template | ✅ `BE-BKC-041`, ✅ `BE-BKC-043` |
| `CAP-37` Penanggung per baris pada invoice | **Missing** | ✅ `BE-BKC-041`, ✅ `BE-BKC-048`, ✅ `BE-BKC-049` |
| `CAP-38` Lembar Invoice Asuransi sebagai pola | Reuse with adapter | ✅ `BE-BKC-050`, `FE-BKC-031` |
| `CAP-39` Pendaftaran hak akses lewat atribut | Ready to reuse | ✅ `BE-BKC-042`, ✅ `BE-BKC-043`, ✅ `BE-BKC-051` |
| `CAP-40` Base component dan `BasePayerWorkspace` | Reuse with adapter | `FE-BKC-028`, `FE-BKC-034` |
| `CAP-41` Pola penomoran dokumen | Pola saja | ✅ `BE-BKC-050` (memakai ulang nomor tagihan, tanpa seri baru) |

## 3. Coverage gap

| Butir | Keadaan |
| --- | --- |
| Requirement tanpa acceptance test | **Nihil.** Kedua puluh tiga `FR-BKC-064`–`086` seluruhnya punya sekurang-kurangnya satu `BIL-AT-*` atau `UAT-*` |
| Acceptance test tanpa task pelaksana | **Nihil.** `BIL-AT-081`–`100` seluruhnya terpetakan ke sekurang-kurangnya satu task |
| Epic tanpa UAT jalur gagal | **Nihil.** Kelima epic `MUST HAVE` punya skenario berhasil **dan** gagal |
| Task tanpa jejak requirement | **Nihil**, kecuali `BE-BKC-052` yang memang task aktivasi data dan jejaknya ke `MPY-OQ-006`, bukan ke `FR` |
| Verifikasi yang belum dapat dijalankan | Klik-coba ter-autentikasi dan pengujian ujung-ke-ujung masih bergantung ketersediaan environment — keadaan yang sama seperti rumpun sebelumnya di modul ini, dan **bukan** gap yang lahir dari rumpun ini |

## 4. Pekerjaan di luar roadmap ini

| Pekerjaan | Pemilik | Dilacak di mana |
| --- | --- | --- |
| `EncounterPaymentSourceService` | `RegistrationManagement` (Muhammad Hamzah) | Roadmap modul itu sendiri. Kontrak serah terimanya `MPY-ENC-PAYER-001` pada [`encounter-payment-source-change-contract.md`](../../rawat-inap/episode-rawat-inap/contracts/encounter-payment-source-change-contract.md), disimpan di `<blueprint-root>` `rawat-inap/episode-rawat-inap` berdampingan dengan `RWI-ENC-PAYER-001` |
| Pemeriksaan kolom penanda sudah-ditagih pada catatan penyerahan obat | Backend/API bersama Pharmacy | `MPY-OQ-005`, selesai diselidiki pada langkah pertama `BE-BKC-049` (kolom `BilledAt` bersifat pasif, data Farmasi terisolasi penuh) |
| Pengisian aturan tanggungan per perusahaan penjamin | Product/Domain bersama Admin Master Data | `MPY-OQ-006`, dilacak `BE-BKC-052`. **Interim 12 September 2026:** data `[DEV PLACEHOLDER]` (bukan kontrak riil) terpasang untuk kelima perusahaan aktif agar dapat diuji; task tetap terbuka sampai diganti nilai kontrak asli dan direview Finance — lihat kolom Status `BE-BKC-052` pada `backend-roadmap.md` |
| Koordinasi urutan commit dengan pekerjaan "Payment Reminder" | Pemilik modul `billing-kasir` | `MPY-CQ-03`, dicatat pada `BE-BKC-047`, `048`, `049`, `050`, `051` |
