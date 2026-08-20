# Farmasi — Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `PHA-BP-001` |
| Blueprint revision | `1` |
| Map status | `current-source-audit` |
| Decision input | `00-interview-decisions.md` |
| Input SHA-256 | `f3140fa77a0c87dd8d9d3141727212168741e49c051eaf929c4b794cd33f809c` |
| Backend SHA audited | `39b8b69f61b5754716175e464de1f1ef64f0400e` |
| Frontend SHA audited | `400104f2a0f3239c14c40f5905b419977a538450` |
| Audit date | 19 Agustus 2026 |
| Source mutation | Tidak ada; audit source bersifat read-only |

## Audit boundary

Audit membuktikan perilaku as-is yang relevan terhadap keputusan `PHA-DEC-003` sampai
`PHA-DEC-039`:

- identity dan owner episode: patient, encounter, consultation, doctor, service unit, clinic;
- master obat, satuan, tarif, coverage, lokasi penyimpanan, dan kebijakan stok;
- penyusunan resep reguler/racikan, finalisasi klinis, status pembayaran, dan antrean Farmasi;
- review/klarifikasi apoteker, preparation, final check, batch, dan dispensing;
- ledger stok, reservasi, FEFO, recall, transfer, stock opname, retur, dan adjustment;
- Billing/reversal/refund, pemberian obat rawat inap, resep pulang, authorization, audit,
  notifikasi, integrasi eksternal, serta test evidence.

Tidak diaudit secara mendalam: formula tarif/claim di luar konsumsi Farmasi, implementasi HR
di luar identitas aktor, dan detail modul IGD/Inpatient yang tidak memiliki consumer Farmasi.

## Journey yang ditelusuri

1. Dokter membuka antrean konsultasi, mencari katalog obat berdasarkan encounter, menyusun
   resep reguler/racikan, lalu finalisasi konsultasi.
2. Resep submitted menunggu status pembayaran/penjamin, kemudian berubah menjadi
   `ReadyForPharmacy` melalui endpoint mutation pada resep.
3. Apoteker memulai review, mengisi kriteria/klarifikasi, menyetujui, menyiapkan obat, dan
   memasukkan batch/expiry sebagai input preparation.
4. Frontend daftar Farmasi membuka detail resep dan mencoba menyediakan workflow review,
   preparation, final check, dan dispensing.
5. Stok lokasi, reservasi saat save, serah-terima, charge setelah pemberian, retur/reversal,
   recall, serta discharge medication dicari sebagai journey lintas modul.

## Capability register

| ID | Need | Owner | Evidence (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risk |
|---|---|---|---|---|---|---|
| `PHA-CAP-001` | Patient/encounter/consultation/doctor/service-unit linkage | Registration + Clinical | `BE/Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs#TrxPrescription@39b8b69`; `BE/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs#CreatePrescription@39b8b69` | Ready to reuse | Header menyalin owner episode dan snapshot penjamin dari encounter/consultation | Actor scope pada mutation dibahas terpisah di `PHA-CAP-016` |
| `PHA-CAP-002` | Master obat, satuan, klasifikasi khusus, tarif, dan coverage | Health Services Master Data + Clinical | `BE/Areas/HealthServices/MasterData/Models/MstDrug.cs#MstDrug@39b8b69`; `BE/Areas/HealthServices/ClinicalManagement/Controllers/PrescribingDrugController.cs#GetDrugs@39b8b69`; `FE/src/lib/services/health-services/clinical-management/prescribing-drug.service.js#getPrescribingDrugs@400104f` | Ready to reuse | Katalog aktif/prescribable, unit, tariff readiness, dan insurance pricing sudah dikonsumsi UI dokter | Narasi interaksi bukan clinical rule engine |
| `PHA-CAP-003` | Lokasi penyimpanan dan reorder/safety-stock policy per scope | Master Data + Pharmacy/Warehouse | `BE/Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs#MstDrugStorageLocation@39b8b69`; `BE/Areas/HealthServices/MasterData/Models/MstDrugStockPolicy.cs#MstDrugStockPolicy@39b8b69`; `FE/src/lib/hooks/health-services/master-data/drug-stock-policy/use-drug-stock-policy-editor.jsx#useMasterDataDrugStockPolicyEditor@400104f` | Reuse with adapter | CRUD BE/FE tersedia; kunci scope drug/location/unit/clinic dan validasi threshold tersedia. Hilangkan pilihan `IsAllowNegativeStock=true` dan tambahkan maker-checker/audit reason sesuai keputusan | UI dan API saat ini masih mengizinkan policy stok negatif |
| `PHA-CAP-004` | Authoritative on-hand/reserved/prescribable ledger dan atomic reservation | Pharmacy/Inventory | `BE/Areas/HealthServices/MasterData/Models/MstDrugStockPolicy.cs#MstDrugStockPolicy@39b8b69`; scoped search tidak menemukan balance/ledger/reservation transaction | Missing | Policy bukan saldo. Tidak ada on-hand, reserved, release, concurrency token, atau mutation atomik | `PHA-DEC-004`, `019`, `020`, `021` belum dapat ditegakkan |
| `PHA-CAP-005` | Batch/expiry allocation, FEFO, quarantine, recall, patient trace | Pharmacy/Inventory | `BE/Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionPreparation.cs#TrxPrescriptionPreparationItem@39b8b69`; `BE/Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs#IsQuarantineLocation@39b8b69` | Extend | Preparation menyimpan batch/expiry sebagai input bebas; tidak ada authoritative batch stock, FEFO selector, recall state, block, atau reverse trace patient | Batch yang tidak ada/expired dapat diketik tanpa validasi ledger |
| `PHA-CAP-006` | Transfer, in-transit, partial receipt/discrepancy, opname, adjustment, return/destruction | Pharmacy/Warehouse | Scoped model/configuration search pada backend SHA audited tidak menemukan transaction owner | Missing | Belum ada lifecycle atau persistence owner | Custody dan audit `PHA-DEC-030`–`032` tidak dapat dijalankan |
| `PHA-CAP-007` | Dokter menyusun resep reguler/racikan dan template | Clinical + Pharmacy | `BE/Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkspaceService.cs#PrescriptionWorkspaceService@39b8b69`; `FE/src/lib/hooks/health-services/pharmacy-management/use-doctor-prescription.js#useDoctorPrescription@400104f`; `FE/src/components/view/health-services/registration-management/doctor-queues/tabs/prescription/doctor-prescription-tab.jsx#DoctorPrescriptionTab@400104f` | Repair | Journey reachable dan autosave tersedia, tetapi create header menginisialisasi fulfillment `WaitingForPayment` sementara finalization mensyaratkan `WaitingForClinicalFinalization`; tidak ada validasi/reservasi stok | Resep baru dapat gagal difinalkan karena state awal tidak kompatibel |
| `PHA-CAP-008` | Finalisasi consultation + resep dengan warning acknowledgement | Clinical | `BE/Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs#FinalizeAsync@39b8b69`; `BE/Areas/HealthServices/PharmacyManagement/Services/PrescriptionValidationService.cs#ValidateForConsultationAsync@39b8b69` | Extend | Transaction boundary, warning acknowledgement, duplicate item, approval flag, tariff/unit check tersedia; perlu stock reservation dan clinical safety engine | Severity statis saat ini belum setara keputusan clinical governance |
| `PHA-CAP-009` | Payment/guarantee gate dan handoff ke antrean Farmasi | Billing + Pharmacy | `BE/Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkflowService.cs#CompletePaymentAsync@39b8b69`; `BE/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs#MarkPaid@39b8b69` | Conflict | State gate cocok secara konseptual, tetapi tidak ada billing transaction owner; client dengan permission `Prescription.Update` dapat menandai paid/insurance-approved/waived langsung | Status pembayaran dapat berubah tanpa bukti pembayaran/penjamin authoritative |
| `PHA-CAP-010` | Telaah apoteker, criteria version, hard stop, clarification doctor | Pharmacy + Clinical | `BE/Areas/HealthServices/PharmacyManagement/Services/PrescriptionReviewService.cs#PrescriptionReviewService@39b8b69`; `BE/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionReviewController.cs#PrescriptionReviewController@39b8b69`; `FE/src/lib/hooks/health-services/pharmacy-management/use-prescription-pharmacy-workflow.js#usePrescriptionPharmacyWorkflow@400104f` | Extend | Backend review/version/clarification ada. Frontend hook tidak dipakai component; severity dapat diubah lewat request dan controller hanya `[Authorize]` tanpa permission pharmacist/doctor terpisah | Actor/SoD dan clinical severity authority belum ditegakkan |
| `PHA-CAP-011` | Preparation dan final check | Pharmacy | `BE/Areas/HealthServices/PharmacyManagement/Services/PrescriptionPreparationService.cs#PrescriptionPreparationService@39b8b69`; `BE/Areas/HealthServices/PharmacyManagement/Services/PrescriptionFinalCheckService.cs#PrescriptionFinalCheckService@39b8b69`; `FE/src/lib/services/health-services/pharmacy-management/prescription-pharmacy-workflow.service.js#completePrescriptionFinalCheck@400104f` | Repair | Preparation endpoint ada; final-check service registered tetapi tidak memiliki controller. FE memanggil route `/prescription-final-checks/...` yang tidak tersedia. Preparation langsung menetapkan `ReadyToDispense` sebelum final check | Final check tidak menjadi gate nyata dan FE mendapat 404 |
| `PHA-CAP-012` | Physical handover/dispense, partial dispense, timestamps, stock decrement | Pharmacy/Inventory | `BE/Areas/HealthServices/PharmacyManagement/Enums/PrescriptionFulfillmentStatus.cs#PrescriptionFulfillmentStatus@39b8b69`; search atas assignment `DispensedAt`, `DispensedByUserId`, dan `FulfillmentStatus.Dispensed` tidak menemukan mutation | Missing | Enum/model field hanya deklaratif; tidak ada endpoint/service/ledger effect | Journey berhenti di preparation; serah-terima dan pengurangan stok tidak terbukti |
| `PHA-CAP-013` | Larangan substitusi oleh apoteker/sistem | Clinical + Pharmacy | `BE/Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionDrugSubstitution.cs#TrxPrescriptionDrugSubstitution@39b8b69`; search menemukan model/config/DbSet tanpa consumer/service/controller | Conflict | Dormant substitution persistence bertentangan dengan `PHA-DEC-033`; mutation resep final tetap dikunci tetapi schema intent tidak selaras | Desain berikutnya dapat tanpa sengaja mengaktifkan substitusi yang dilarang |
| `PHA-CAP-014` | Allergy/interaction/dose clinical decision support berversi | Clinical Governance + Pharmacy | `BE/Areas/HealthServices/ClinicalManagement/Models/TrxPatientAllergy.cs#TrxPatientAllergy@39b8b69`; `BE/Areas/HealthServices/MasterData/Models/MstDrug.cs#DrugInteraction@39b8b69`; scoped search tidak menemukan pemakaian allergy pada prescription validation | Missing | Patient allergy dan narasi obat adalah input yang dapat direuse, tetapi tidak ada rule set/version/severity/override/verification engine | Hard stop `PHA-DEC-037`–`039` belum ada |
| `PHA-CAP-015` | Rawat inap administration, charge-after-administration, discharge medication, admission merge | Inpatient/Nursing + Billing + Pharmacy | scoped file/model search tidak menemukan medication administration, inpatient admission, atau discharge medication owner | Missing | Encounter ada tetapi downstream clinical administration dan discharge gate tidak ada | `PHA-DEC-007`, `024`–`026` belum dapat dibuktikan |
| `PHA-CAP-016` | Fine-grained authorization, actor scope, segregation of duties, audit | Security + Pharmacy/Clinical | `BE/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs#PrescriptionController@39b8b69`; `BE/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionReviewController.cs#PrescriptionReviewController@39b8b69`; `BE/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionPreparationController.cs#PrescriptionPreparationController@39b8b69` | Repair | Core prescription CRUD memakai `AccessPermission`; review/preparation hanya authentication. Tidak ada pharmacist role, doctor ownership, dual-check, witness, atau maker-checker enforcement. Base identity fields memberi audit dasar | User terautentikasi dapat memanggil mutation review/preparation tanpa business-role proof |
| `PHA-CAP-017` | Frontend Pharmacy list/detail/action journey | Pharmacy UI | `FE/src/app/health-services/pharmacy-management/prescriptions/page.jsx#Page@400104f`; `FE/src/app/health-services/pharmacy-management/prescriptions/[consultationId]/page.jsx#PrescriptionWorkspaceRoute@400104f`; `FE/src/components/view/health-services/pharmacy-management/prescription-list/prescription-list-detail-page.jsx#PrescriptionListDetailPage@400104f` | Repair | List memiliki loading/error/retry/refresh. Detail route mengirim `consultationId`, sedangkan component meminta `prescriptionId`, sehingga request tidak dikirim. Detail hanya read-only dan workflow hook tidak terpasang | Route detail dan seluruh action Farmasi tidak reachable end-to-end |
| `PHA-CAP-018` | Billing charge/reversal/refund/internal hospital loss | Billing/Finance | `BE/Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs#BillingId@39b8b69`; scoped search tidak menemukan `TrxBilling`, invoice/payment transaction, reversal, atau refund pada Health Services | Missing | Price/coverage snapshot dapat direuse; financial posting owner belum ada | Status/angka resep bukan bukti posting finansial atau idempotency charge |
| `PHA-CAP-019` | Recall/pickup/payment notifications dan external medication integration | Notification/Integration + Pharmacy | scoped source search tidak menemukan consumer notification/recall atau medication interoperability untuk Pharmacy | Missing | Belum ada contract/event/retry/PHI policy | Recall follow-up dan external sync tidak terbukti |
| `PHA-CAP-020` | Automated verification evidence | Backend + Frontend owners | scoped test search menghasilkan `NO_BACKEND_TEST_EVIDENCE` dan `NO_FRONTEND_TEST_EVIDENCE` | Missing | Tambahkan unit/integration/contract/UI tests per lifecycle dan permission | Build/lint tidak membuktikan behavior bisnis/concurrency |

## Backend inventory dan as-is contract

### Persistence owner yang tersedia

- `TrxPrescription` memiliki unique active prescription per `ConsultationId`, snapshot encounter,
  patient, doctor, unit, penjamin, pricing total, dan tiga status terpisah: clinical,
  payment, fulfillment.
- Regular item dan compound ingredient menyimpan snapshot drug, dose, signa, quantity,
  tariff/coverage, classification high-alert/narcotic/psychotropic, dan approval flag.
- Review menyimpan version, criteria snapshot, severity, finding, recommendation, pharmacist,
  clarification, dan status.
- Preparation menyimpan theoretical/actual/waste serta batch/expiry yang dikirim client.
- Final-check dan substitution table tersedia, tetapi tidak memiliki journey lengkap.
- Drug master, unit conversion, location hierarchy, serta stock policy sudah dipetakan dan
  dimigrasikan. Tidak ada persistence balance/ledger/transfer/opname/return/recall.

### State contract aktual

Alur yang dapat dicapai dari service backend:

`Draft → Submitted/WaitingForPayment → Paid|InsuranceApproved|PaymentWaived/ReadyForPharmacy`

`ReadyForPharmacy → QueuedAtPharmacy → VerifiedByPharmacy → InPreparation → ReadyToDispense`

Keterbatasan:

- create header menulis `WaitingForPayment`, sedangkan finalization draft memerlukan
  `WaitingForClinicalFinalization`;
- tidak ada authoritative mutation untuk `PartiallyDispensed` atau `Dispensed`;
- header timestamps `PharmacyQueuedAt`, `PharmacyVerifiedAt`, `PreparationStartedAt`,
  `ReadyToDispenseAt`, dan `DispensedAt` tidak seluruhnya ditulis oleh service terkait;
- final-check yang gagal tidak memindahkan resep ke state remediasi, sedangkan yang lulus
  menetapkan state yang sudah sama dengan state sebelum final check;
- cancellation dari modul dokter ditolak setelah Farmasi mulai memproses, tetapi reversal
  setelah handover tidak tersedia.

### Provider/consumer financial contract

- Prescribing catalog menghitung hospital tariff, insurance contract price, coverage,
  patient-pay, approval, dan guarantee-letter requirement.
- Prescription menyimpan snapshot hasil perhitungan tersebut.
- Endpoint prescription dapat menandai billing generated, paid, insurance approved, atau
  waived, tetapi tidak memvalidasi transaksi Billing/Kasir eksternal.
- Tidak ada evidence posting charge, payment allocation, reversal, refund, admission account
  merge, atau hospital-loss ledger.

### Authorization dan audit aktual

- Prescription header/item/compound/workspace memakai `AccessController`, `AccessAction`, dan
  `AccessPermission`.
- Payment mutations memakai permission umum `Prescription.Update`, bukan permission khusus
  Kasir/Billing.
- Review dan preparation hanya memakai `[Authorize]`; tidak ada permission action atau validasi
  profesi/role.
- `EnsureEditableAsync` menjaga state draft/payment, tetapi tidak membuktikan bahwa actor adalah
  dokter pemilik consultation.
- Entity identity fields menyimpan create/update/delete/cancel actor/timestamp. Logger dipakai
  pada sebagian controller, tetapi business audit khusus reason/old-new/witness belum konsisten.

## Frontend inventory dan as-is contract

### Reachable

- Doctor queue memasang `DoctorPrescriptionTab` dan memakai `useDoctorPrescription` untuk
  catalogue, regular/compound editing, template, autosave, conflict reload, dan finalization
  registration.
- Master drug, category, supplier, unit conversion, location, dan stock policy memiliki App
  Router pages, Redux/hooks, form/list/detail, access-denied, loading, error, dan retry patterns.
- Pharmacy prescription list memiliki filter, paging, polling saat visible, loading/empty/error,
  dan direct detail navigation.

### Tidak reachable atau mismatch

- Route folder `[consultationId]` sebenarnya menerima `prescriptionId` dari list, tetapi page
  mengirim prop bernama `consultationId` ke component yang hanya menerima `prescriptionId`.
- Detail Pharmacy bersifat read-only; tidak merender review/preparation/final-check actions.
- `usePrescriptionPharmacyWorkflow` dan service lengkap tersedia tetapi tidak diimport oleh
  component/page mana pun.
- Service final-check mengarah ke endpoint backend yang tidak ada.
- Doctor drug availability hanya memeriksa tariff/configuration readiness. Tidak ada field atau
  presentation prescribable stock, reservation, location, batch, atau expiry.
- Stock-policy form mengekspos `Boleh Stok Negatif`, bertentangan dengan `PHA-DEC-004`.

## Confirmed conflicts dan unknowns

### Confirmed conflicts

1. `PHA-GAP-001`: initial prescription fulfillment state tidak kompatibel dengan finalization
   guard.
2. `PHA-GAP-002`: keputusan no-negative-stock bertentangan dengan BE/FE configuration yang
   mengizinkan `IsAllowNegativeStock=true`.
3. `PHA-GAP-003`: payment authority berada pada generic Prescription Update, tanpa Billing
   transaction evidence.
4. `PHA-GAP-004`: substitution persistence ada walaupun keputusan melarang substitusi.
5. `PHA-GAP-005`: frontend detail prop mismatch membuat detail Pharmacy gagal load.
6. `PHA-GAP-006`: frontend memanggil final-check endpoint yang tidak tersedia.
7. `PHA-GAP-007`: fulfillment enum/timestamps menyatakan dispense, tetapi tidak ada mutation
   yang mencapainya.
8. `PHA-GAP-008`: review/preparation tidak memiliki permission/role/SoD enforcement.

### Unknown di luar source boundary

- owner final dan SOP untuk pharmacist, Kepala Farmasi, dual-check, witness, stock adjustment,
  retur, recall, dan controlled-drug governance;
- sistem Billing/Kasir eksternal yang mungkin hidup di repository/service lain;
- source clinical knowledge base untuk allergy, interaction, dose, severity, dan versioning;
- lokasi/depo pemenuh resep yang dipilih dari encounter;
- current regulatory contract untuk narkotika/psikotropika/high-alert;
- SLA reservation, unpaid cancellation, pickup 24 jam, partial payment, dan pending guarantee;
- contract Inpatient/Nursing untuk medication administration dan discharge closure;
- production database migration state serta data seed/configuration runtime.

## Closure questions

Pertanyaan berikut bukan permintaan implementasi; jawabannya diperlukan sebelum target design:

1. Repository/service mana yang authoritative untuk Billing, Cashier, payment, refund, dan
   admission account consolidation?
2. Siapa approver formal untuk permission matrix dokter, apoteker, Kasir, Kepala Farmasi,
   petugas gudang, checker, dan witness?
3. Apakah `TrxPrescriptionDrugSubstitution` harus dipertahankan sebagai audit historis saja,
   atau sudah tidak menjadi bagian contract aktif?
4. Apa source/version clinical rule yang disetujui untuk allergy, interaction, dose, dan
   severity?
5. Bagaimana encounter menentukan satu farmasi/depo pelayanan authoritative?
6. Apa state dan owner untuk unpaid reservation expiry, partial payment, pending guarantee,
   correction, reversal, dan duplicate callback?
7. Repository/service mana yang akan memiliki medication administration rawat inap dan
   discharge medication handover?

## Verification evidence

- Backend: `dotnet build QuilvianSystemBackend.csproj --no-restore -v:q` berhasil, `0 error`.
- Frontend targeted ESLint terhadap route/component/hook/service/utils Farmasi dan prescribing
  menghasilkan `0 error`, `13 warning`; warning terutama `react-hooks/set-state-in-effect` dan
  `react-hooks/refs` pada hook Farmasi.
- Scoped search tidak menemukan test Farmasi backend maupun frontend.
- Runtime API/database/browser tidak dijalankan; status `Ready to reuse` hanya diberikan pada
  capability yang memiliki persistence/provider, authorization wiring, consumer, dan source
  validation yang relevan terhadap scope capability tersebut.

## Impact scan trigger

Decision log mencatat backend SHA lama `36d7eca7...`; audit memakai `39b8b69f...`. Scoped diff
antara kedua SHA untuk Pharmacy, Master Data, Clinical, configuration, Program, dan migrations
tidak menemukan perubahan file relevan, sehingga map ini current pada SHA audited.

Map menjadi `stale` dan wajib menjalankan impact scan terarah jika salah satu berubah:

- backend atau frontend SHA;
- decision log revision/hash;
- contract Prescription/Payment/Fulfillment status;
- Drug/StockPolicy/StorageLocation, review/preparation/final-check, Billing, inventory ledger,
  medication administration, authorization, atau route Pharmacy;
- migration/runtime dependency yang menjadi evidence capability.

