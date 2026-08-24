# Billing dan Kasir — Existing Capability Map

| Field | Nilai |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Capability-map revision | `0.2` |
| Status | `source-audited`; belum menyatakan siap implementasi atau siap produksi |
| Tanggal audit | 20 Agustus 2026 (`Asia/Jakarta`) |
| Business input | [`00-interview-decisions.md`](./00-interview-decisions.md), approved decision revision `0.2` |
| Supplemental evidence | [`05-servicebilling-attachment-evidence.md`](./evidence/05-servicebilling-attachment-evidence.md), ZIP SHA-256 `2b948721cee4154eaecaf9ac57d7621fb34cb7b61fb31a5fd6dff04df7ad218d` |
| Backend snapshot | `e6f6ecba1537783ea2eb379ac12cc97790707303` (current branch `Yasmina`) |
| Frontend snapshot | `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151` |
| Audit method | Pembacaan statis source, konfigurasi EF, migration, route, DI, state/service frontend, dan test inventory |
| Write boundary | Hanya dokumen blueprint ini; source aplikasi tidak diubah |

## 1. Boundary audit

Audit mencakup capability yang diperlukan untuk alur Billing dan Kasir yang telah disetujui:

- identitas encounter, penjamin, tarif, coverage, dan sumber item pelayanan;
- invoice, billing item, administrasi, diskon, deposit, pembayaran, refund, finalisasi, AR, dan AP dokter;
- integrasi tindakan, resep, laboratorium, dan radiologi;
- kasir, split tender, rekonsiliasi QRIS, shift, selisih kas, authorization, dan audit trail;
- consumer frontend, route yang dapat dijangkau, state/service, error/loading behavior, dan bukti pengujian.

Tidak termasuk dalam audit ini: perancangan target schema/API, implementasi, eksekusi migration,
perubahan database, penentuan owner baru, dan verifikasi runtime terhadap environment eksternal.
Istilah status pada dokumen ini hanya memakai: `Ready to reuse`, `Reuse with adapter`, `Extend`,
`Repair`, `Missing`, `Conflict`, dan `Unknown`.

## 2. Impact scan sejak interview decisions

Dokumen keputusan sebelumnya merekam backend SHA
`a4a71584104a738502042188623fbc36971995e0`. Audit ini memakai SHA backend yang lebih baru,
sehingga impact scan dilakukan untuk rentang tersebut.

Fakta perubahan relevan:

- capability `LaboratoryManagement/LabOrder` ditambahkan, termasuk model, DTO, controller,
  service, konfigurasi EF, migration, registrasi DI, dan `DbSet`;
- `ApplicationDbContext.cs` dan `Program.cs` berubah untuk capability Lab Order;
- tidak ditemukan penambahan aggregate invoice, billing item transaksi, deposit, payment,
  refund, cashier shift, AR penjamin, atau integrasi pembentuk Billing.

Konsekuensi: pernyataan lama bahwa Lab Order sama sekali belum ada sudah kedaluwarsa. Yang ada
sekarang adalah fondasi order minimal; lifecycle pemeriksaan dan integrasi Billing tetap belum ada.

Impact scan tambahan dari `e6f6ecba1537783ea2eb379ac12cc97790707303` ke
`f63572a962e1e21ff71105ab0122814e269355e0` hanya menemukan perubahan housekeeping/engineering
dan penambahan folder IGD pada project. Tidak ada aggregate, route, DI, migration, atau consumer
transaksi Billing/Kasir baru. Karena itu seluruh kesimpulan current-V2 pada peta ini tetap berlaku.
Current branch kembali berada pada SHA `e6f6ecba1537783ea2eb379ac12cc97790707303`; inverse impact
review memakai comparison yang sama dan tidak mengubah capability classification Billing/Kasir.

## 3. Kesimpulan eksekutif

Source saat ini belum memiliki modul transaksi Billing dan Kasir yang dapat dilengkapi secara
inkremental. Yang tersedia adalah dua master Billing, sumber episode/penjamin, mesin pricing dan
coverage, serta beberapa producer klinis yang menyimpan marker Billing tanpa consumer Billing
otoritatif.

Tidak ada jalur end-to-end yang membuktikan:

`order pelayanan -> billing item idempotent -> invoice encounter -> deposit/progress payment ->`
`split tender -> finalisasi -> AR penjamin + AP dokter -> shift kasir`.

Karena itu, master dan sumber klinis dapat dipakai sebagai input/adaptor, tetapi aggregate transaksi
inti harus dibangun. Marker Billing pada tindakan dan resep perlu diperbaiki agar tidak lagi dapat
menyatakan “billing generated” tanpa record Billing yang otoritatif.

## 4. Capability evidence map

| Capability | Status | Evidence as-is | Gap terhadap keputusan approved |
| --- | --- | --- | --- |
| Encounter sebagai identitas episode | `Reuse with adapter` | `TrxPatientEncounter` memiliki `EncounterNumber`, pasien, unit layanan, klinik, dokter, dan metode pembayaran; `ApplicationDbContext.cs:547` | Belum ada relasi/invariant satu invoice per encounter dan belum ada kontrak transfer rajal ke ranap |
| Snapshot penjamin encounter | `Reuse with adapter` | `TrxPatientEncounterGuarantor` menyimpan payment method, patient insurance, provider, dan snapshot; `ApplicationDbContext.cs:548` | Belum menjadi snapshot finansial invoice dan belum membentuk AR |
| Resolusi tarif dan coverage | `Reuse with adapter` | `InsuranceCoverageService.cs:63-256` menghitung contract tariff, coverage, co-payment, dan patient pay | Belum ada repricing invoice, versioned calculation, locking, atau snapshot final |
| Master metode pembayaran | `Extend` | CRUD authorized di `PaymentMethodController.cs:20-516`; model memiliki flag Billing/refund dan integration code; `ApplicationDbContext.cs:530` | Belum dikonsumsi transaksi tender, split payment, gateway, atau reconciliation |
| Master kategori billing item | `Extend` | CRUD authorized di `BillingItemCategoryController.cs:20-581`; flag procedure/lab/radiology/pharmacy/admin/deposit/refund/discount; `ApplicationDbContext.cs:531` | Kategori tidak membentuk item atau aturan lifecycle finansial |
| Aggregate invoice satu encounter | `Missing` | Tidak ditemukan entity, `DbSet`, service, atau controller invoice Billing transaksi | Seluruh state `OPEN`, progress payment, final settlement, closed, dan reopen belum tersedia |
| Billing item dan idempotensi sumber | `Missing` | Tidak ditemukan ledger/item transaksi dengan source type + source ID unik | Belum menjamin satu pelayanan masuk tepat satu kali atau menyimpan histori void/koreksi |
| Tindakan pasien: snapshot harga/coverage | `Reuse with adapter` | `TrxPatientProcedure.cs:36-198`; create memakai `ResolveProcedureAsync` dan transaction di `PatientProcedureController.cs:434-553` | Snapshot berada di producer, belum menjadi snapshot Billing yang dapat direkonsiliasi |
| Tindakan pasien -> Billing | `Repair` | Ada `BillingItemId`, `IsBillingGenerated`, `BillingGeneratedAt`, tetapi create selalu `false`; tidak ditemukan writer menjadi `true` | Marker tidak terhubung ke record Billing; cancel hanya bergantung pada marker sehingga guard dapat salah |
| Resep -> Billing | `Repair` | `TrxPrescription` memiliki `BillingId` dan nilai coverage; `MarkBillingGeneratedAsync` hanya mengisi ID opsional/status/timestamp (`PrescriptionWorkflowService.cs:60-78`) | Endpoint dapat menandai Billing tanpa membuat/memvalidasi invoice atau billing item otoritatif |
| Fondasi Lab Order | `Extend` | `LabOrder` hanya memiliki `EncounterId` dan `ProcedureId`; CRUD read/create/cancel authorized; `ApplicationDbContext.cs:583` | Perlu duplicate guard, lifecycle performed/result, cancel reason/actor confirmation, dan contract klinis |
| Lab Order -> Billing | `Missing` | Tidak ada pricing, coverage, billing marker, event/outbox, atau pemanggilan Billing pada `LabOrderService` | OTC lab tidak dapat dibuktikan lunas sebelum pemeriksaan dan pembatalan tidak memicu refund |
| Radiology Order -> Billing | `Missing` | Ditemukan master flag dan UI/utility radiologi, tetapi tidak ditemukan aggregate order radiologi backend yang setara Lab Order | Tidak ada sumber transaksi stabil untuk Billing, lifecycle performed, atau konfirmasi pembatalan |
| Aturan biaya administrasi | `Missing` | Hanya flag `IsAdministrationFee` pada `MstTariff`, `MstTariffCategory`, dan `MstBillingItemCategory` | Belum ada nominal/rule effective-dated: sekali pasien/hari, sekali admission, dan replacement rajal -> ranap |
| Master promo/discount otomatis | `Missing` | Hanya flag kategori dan diskon kontrak asuransi; tidak ada master promo Billing approved Finance | Belum ada scope item/total, periode efektif, patient-only allocation, atau non-discountable admin fee |
| Diskon dokter dan approval | `Conflict` | FE legacy memakai role substring dan komentar approval 3 layer (`diskonApprovalGuard.js:20-67`); route notifikasi menunjuk `/kasir/...` yang tidak ada di App Router | Approved flow adalah dokter memutuskan, kasir input, approval dokter satu layer; Finance hanya exception |
| Diskon ad-hoc | `Missing` | Tidak ditemukan request/approval Finance atau adjustment ledger | Belum mendukung approval ad-hoc dan audit sebelum efektif |
| Deposit/top-up/progress payment ranap | `Missing` | Tidak ditemukan wallet/ledger deposit, allocation, top-up, release, atau refund | Dana belum dapat ditahan unallocated dan dialokasikan sebagian tanpa mengunci invoice |
| Payment, split tender, dan outstanding | `Missing` | Tidak ditemukan payment attempt/tender/allocation entity atau endpoint | Cash sukses + QRIS gagal tidak dapat dipertahankan sebagai payment parsial yang auditable |
| Gateway QRIS dan reconciliation | `Missing` | Master hanya mempunyai integration metadata; tidak ditemukan request, callback, idempotency, atau reconciliation | Tidak ada status attempt, retry terhadap sisa, atau perlindungan duplicate callback |
| Refund dan reversal | `Missing` | Tidak ditemukan refund request, Finance authorization, payment reversal, atau refundable-credit ledger | Pembatalan OTC/rajal sesudah bayar belum dapat menghasilkan proses Finance yang terlacak |
| Write-off pasien | `Missing` | Tidak ditemukan AR pasien/write-off workflow pada scope Health Services | Belum mendukung pengajuan Billing/AR, approval Finance, parsial/penuh, dan audit non-payment |
| AR penjamin | `Missing` | Encounter menyimpan penjamin, tetapi tidak ditemukan receivable yang lahir saat finalisasi invoice | Porsi penjamin, excess, claim rejection tetap AR RS, dan status lunas pasien belum terpisah |
| AP dokter data shell | `Reuse with adapter` | `TrxMedicalServiceFeeCalculation` dan `TrxMedicalServiceFeePayment`; `ApplicationDbContext.cs:389-390` | Hanya model payroll; tidak ada controller/service producer dan tidak ada source link ke invoice/item final |
| AP dokter readiness policy | `Missing` | Model punya state calculation/payment generik, tanpa event invoice final atau settlement gate | Belum memisahkan AP “lahir” dan “siap dibayar” sesuai policy pemilik AP |
| Shift kasir dan selisih kas | `Missing` | Tidak ditemukan shift/register/opening balance/physical count/variance workflow | Saldo awal/akhir, cash sistem, kas fisik, investigasi, dan pelaporan kepala kasir belum ada |
| Audit master Billing | `Reuse with adapter` | Kedua controller master memakai permission attributes dan `LoggerService` | Pola dapat dipakai, tetapi perlu audit transaksi immutable dan actor/reason yang lebih spesifik |
| Workspace frontend Billing/Kasir | `Missing` | Tidak ada route pada `src/app` dengan nama/path Billing, Kasir, atau Cashier; tidak ada service/slice transaksi | Worklist, invoice detail, split checkout, deposit, refund, close/reopen, dan shift belum tersedia |
| Pilihan payer/metode saat registrasi | `Reuse with adapter` | Emergency registration menampilkan `paymentMethodId` dan payer (`payment-method-step.jsx:93-174`) | Ini sumber konteks encounter, bukan penerimaan uang atau tender kasir |
| Ringkasan harga resep | `Reuse with adapter` | Komponen menampilkan total, covered, patient pay (`prescription-billing-summary.jsx:10-48`) | Presentational summary producer; bukan invoice atau checkout dan menyatakan recalculation per autosave |
| Test otomatis Billing/Kasir | `Missing` | Backend tidak memiliki test project relevan; FE hanya test auth/base/route smoke tanpa skenario Billing/Kasir | Tidak ada bukti invariant, concurrency, idempotency, split failure, finalisasi, AR/AP, atau shift |

## 5. Kontrak backend as-is

### 5.1 BillingManagement hanya master data

`BillingManagement/MasterData` menyediakan dua resource:

1. `api/v1/health-services/billing-management/master-data/payment-methods`;
2. `api/v1/health-services/billing-management/master-data/billing-item-categories`.

Keduanya memiliki list, filter metadata, summary, option list, detail, create, update,
activate/deactivate, dan soft delete. Controller memakai `[Authorize]`, `AccessController`,
`AccessAction`, `AccessPermission`, validasi duplicate, serta `LoggerService`. Ini adalah pola teknis
yang bisa dipertahankan, bukan bukti bahwa transaksi Billing sudah ada.

`MstPaymentMethod` memodelkan ketersediaan untuk billing/refund serta metadata integrasi. Nominal
atau persentase administration fee pada metode pembayaran, jika dipakai, bermakna biaya tender;
ia tidak memenuhi aturan biaya administrasi pasien yang bergantung pada jenis kunjungan dan hari.

`MstBillingItemCategory` adalah taksonomi. Flag `IsAdministrationFee`, `IsDiscount`, `IsDeposit`,
dan `IsRefund` tidak memiliki ledger atau behavior transaksi di belakangnya.

### 5.2 Encounter dan penjamin

`TrxPatientEncounter` adalah kandidat owner identitas episode. `TrxPatientEncounterGuarantor`
menjadi record payment source/penjamin per encounter dan menyimpan referensi serta snapshot yang
relevan. Keduanya tetap berada di Registration Management; Billing sebaiknya mereferensikan
identitas ini dan mengambil snapshot finansialnya, bukan mengambil alih ownership registrasi.

Belum ada bukti database invariant yang menghubungkan encounter ke tepat satu invoice. Transfer
rajal ke ranap juga belum memiliki kontrak finansial untuk mengganti biaya administrasi.

### 5.3 Pricing dan coverage

`InsuranceCoverageService` menerima konteks encounter/item, memilih tarif rumah sakit dan kontrak
asuransi sesuai effective period/scope, lalu menghitung covered amount serta patient pay. Nilai uang
dibulatkan dua desimal dengan midpoint away from zero (`InsuranceCoverageService.cs:683`).

Capability ini layak menjadi dependency perhitungan, tetapi Billing tetap membutuhkan calculation
version, input snapshot, reason perubahan, dan final snapshot. Memanggil ulang service saja tidak
mencukupi kebutuhan histori atau locking.

### 5.4 Producer tindakan

Create tindakan menyelesaikan coverage dan menyimpan hasil dalam transaction yang sama, lalu
menetapkan `IsBillingGenerated = false`. Tidak ditemukan consumer yang membuat billing item dan
mengubah marker menjadi benar secara atomik.

Endpoint cancel menolak ketika `IsBillingGenerated` benar (`PatientProcedureController.cs:1042`),
tetapi tidak membuktikan order belum performed. Karena marker tidak pernah diselesaikan oleh Billing,
guard ini dapat mengizinkan pembatalan pada kondisi klinis yang seharusnya tidak boleh. Ini adalah
defect integrasi, bukan sekadar capability baru.

### 5.5 Producer resep

Prescription menyimpan total, covered amount, patient pay, payment/fulfillment status, `BillingId`,
dan timestamp. Action “mark billing generated” mengubah status menjadi menunggu pembayaran serta
menyimpan ID opsional. Tidak ada foreign-key/lookup yang membuktikan ID tersebut adalah invoice
otoritatif dan tidak ada billing item yang dibuat secara atomik.

### 5.6 Producer laboratorium dan radiologi

Lab Order saat ini memvalidasi encounter dan procedure lab aktif, kemudian menyimpan pasangan
`EncounterId`/`ProcedureId`. Cancel hanya mengisi common cancellation fields. Konfigurasi EF memiliki
index pencarian, tetapi tidak ditemukan unique constraint untuk mencegah duplicate order yang sama.

Tidak ada state ordered/collected/performed/resulted, harga, coverage, pelaksana konfirmasi,
cancel reason, atau integrasi Billing. Untuk radiologi, hanya ditemukan klasifikasi procedure/tariff,
halaman/utility legacy, dan presentation artifacts; tidak ditemukan backend order transaction.

### 5.7 AP dokter yang ada

`TrxMedicalServiceFeeCalculation` memodelkan periode jasa medis, gross service, fee, deduction, tax,
net fee, approval, dan posting payroll. `TrxMedicalServiceFeePayment` memodelkan penjadwalan/status
pembayaran. Hanya model, konfigurasi, migration, dan `DbSet` yang ditemukan; tidak ditemukan
controller atau service operasional untuk calculation tersebut.

Tidak ada stable reference ke invoice final, billing item, procedure, atau share dokter. Karena itu
struktur ini bisa menjadi downstream adapter candidate, tetapi belum boleh dianggap AP dokter yang
siap dipakai.

## 6. Kontrak frontend as-is

- Emergency registration memilih metode pembayaran sistem dan payer untuk encounter. UI memiliki
  opsi, validation, loading/error payer, dan langkah verifikasi; ia tidak menerima uang.
- Doctor procedure UI memanggil endpoint tindakan, me-refresh daftar, dan menangani duplicate `409`.
  Keberhasilan di UI hanya berarti tindakan tersimpan, bukan billing item terbentuk.
- Prescription workspace menampilkan summary dari data resep. Tidak terdapat navigasi ke invoice
  atau sumber pembayaran.
- `DiskonApprovalNotifBell` menavigasi ke route `/kasir/diskon-approval/...`, tetapi audit `src/app`
  tidak menemukan route tersebut. Guard approval berbasis substring nama role/posisi dan memberi
  fallback luas kepada non-tenaga-medis; ini bukan authorization contract server-side.
- Prefix `/kasir` hanya dikenali oleh handler sidebar. Pengenalan prefix tidak membuat halaman,
  service, Redux state, atau API transaksi tersedia.
- Tidak ditemukan UI payment method master yang mengonsumsi controller BillingManagement baru.

## 7. Trace journey end-to-end

| Journey approved | Trace aktual | Hasil |
| --- | --- | --- |
| Tindakan dibuat dan otomatis masuk invoice sekali | Tindakan + snapshot tersimpan; marker Billing tetap false; tidak ada invoice/item | Putus setelah producer |
| Resep menjadi tagihan dan menunggu pembayaran | Resep dapat ditandai “billing generated” tanpa record Billing | Status semu; integrity tidak terbukti |
| OTC lab/radiologi lunas sebelum performed | Lab order minimal ada; radiology order dan payment gate tidak ada | Tidak tersedia |
| Deposit ranap ditahan, top-up, lalu progress payment | Tidak ada deposit/allocation ledger | Tidak tersedia |
| Split cash + QRIS; QRIS gagal, cash tetap posted | Tidak ada tender/payment attempt/reconciliation | Tidak tersedia |
| Final invoice menghasilkan AR penjamin dan AP dokter | Tidak ada finalization/orchestration; AP hanya data shell payroll | Tidak tersedia |
| Shift kasir direkonsiliasi dengan kas fisik | Tidak ada shift/register/variance | Tidak tersedia |

## 8. Mismatch dan conflict FE–BE

1. FE memiliki notifikasi/guard diskon dokter dan route string `/kasir`, tetapi route page dan API
   transaksi Billing/Kasir yang menjadi owner tidak ditemukan.
2. Producer resep dapat mengumumkan Billing sudah dibuat, sementara backend tidak memiliki invoice
   atau item yang dapat menjadi authoritative target.
3. Producer tindakan mengekspos marker Billing, tetapi tidak ada writer sukses; cancel guard menjadi
   tidak andal.
4. Ringkasan resep menyebut backend menghitung ulang pada autosave, sedangkan approved Billing
   memerlukan histori repricing dan snapshot final, bukan hanya nilai terbaru pada producer.
5. Guard diskon FE menyebut approval RS/voucher tiga layer dan fallback role, sedangkan keputusan
   approved membedakan promo otomatis, diskon dokter satu layer, dan Finance exception.
6. Master payment method siap secara backend, tetapi tidak ditemukan consumer admin maupun kasir
   pada frontend.

## 9. Bukti legacy dari lampiran ServiceBilling

Lampiran adalah potongan service generasi `QuilvianSystemBackendDev`, bukan source current-V2 dan
bukan bukti runtime. Klasifikasi lengkap tersedia di
[`05-servicebilling-attachment-evidence.md`](./evidence/05-servicebilling-attachment-evidence.md).

| Capability legacy | Status terhadap target | Evidence/gap utama |
| --- | --- | --- |
| Generate/reuse nomor invoice melalui `MainKasir` | `Reuse with adapter` | Menunjukkan intent satu invoice kunjungan, tetapi tanggal invoice mengikuti tanggal bayar dan tidak ada invariant current-V2 |
| Pembentukan/update billing lab | `Repair` | Ada upaya sinkron quantity dan reuse, tetapi identity digabung per pemeriksaan lintas booking serta tidak ada uniqueness database |
| Perhitungan coverage primary/excess | `Conflict` | Excess dianggap covered saat primary gagal tanpa validasi kontrak excess atau allocation parsial |
| Bulk `StatusBilling = true` | `Conflict` | Menyamakan paid dengan status item dan tidak memisahkan tender, patient settlement, close, serta AR penjamin |
| Payment detail/cicilan/deposit read model | `Reuse with adapter` | Memberi bukti istilah/reference legacy, tetapi tidak menyediakan ledger, invariant, authorization, atau write contract current-V2 |
| Estimasi kamar rawat inap | `Unknown` | Memakai `ceil(total days)` dan minimum satu hari; policy hari/cutoff/transfer belum disetujui |
| PPN dan pembulatan | `Unknown` | Ada kalkulasi legacy, tetapi applicability, basis, penanggung, rate, effective date, dan rounding belum menjadi policy approved |
| Auto write-off setelah 90 hari | `Conflict` | Bertentangan dengan maker Billing/AR dan approver Finance yang sudah disetujui |
| Daily cashier report | `Repair` | Menjumlah cash, noncash, dan AR penjamin sebagai pendapatan tanpa shift/register reconciliation |
| AR/AP finalization | `Missing` | Hanya terdapat read flag AR; tidak ada service pembentukan AR, AP, controller, DI, migration, atau test |

Attachment tidak mengubah fakta bahwa current-V2 belum memiliki transaksi Billing/Kasir end-to-end.

## 10. Fact, inference, dan recommendation

### Fact

- Hanya master payment method dan billing item category yang berada di BillingManagement.
- Tidak ada entity/`DbSet` transaksi invoice, billing item, deposit, tender, refund, AR, atau shift.
- Tindakan dan resep memiliki marker/referensi Billing tanpa aggregate Billing yang ditemukan.
- Lab Order baru tersedia sebagai pasangan encounter-procedure dan cancellation flag.
- Frontend tidak memiliki App Router page Billing/Kasir pada snapshot yang diaudit.
- Tidak ada test otomatis Billing/Kasir yang relevan pada inventory repository.

### Inference

- Folder bernama BillingManagement belum mewakili capability transaksi Billing.
- Marker Billing pada producer kemungkinan merupakan seam integrasi yang belum selesai, bukan
  kontrak yang aman untuk dilanjutkan apa adanya.
- `TrxMedicalServiceFeeCalculation` kemungkinan dimaksudkan sebagai downstream payroll/AP, tetapi
  readiness dan ownership aktual tidak dapat dibuktikan hanya dari model.
- Artefak diskon `/kasir` kemungkinan berasal dari implementasi legacy/parsial yang tidak lagi
  route-complete pada frontend saat ini.

### Recommendation

- Pertahankan Encounter sebagai episode reference dan pricing/coverage service sebagai dependency,
  dengan adapter serta snapshot boundary yang eksplisit.
- Bangun aggregate invoice, item ledger idempotent, deposit/allocation ledger, payment/tender, dan
  finalization orchestration sebagai capability inti; jangan menjadikan marker producer sebagai
  source of truth.
- Perbaiki tindakan dan resep melalui contract atomik/idempotent terhadap Billing, termasuk
  reconciliation untuk record lama yang statusnya tidak konsisten.
- Definisikan port/event finalisasi untuk AR dan AP; adaptasikan data shell jasa medis hanya setelah
  owner AP menyetujui input, state, dan settlement gate.
- Ganti authorization diskon berbasis role-string di frontend dengan permission server-side yang
  mengikuti jenis diskon dan approval approved.
- Rencanakan vertical slice pertama yang membuktikan satu producer -> satu billing item -> satu
  invoice encounter sebelum checkout, refund, AR/AP, dan shift dikembangkan.

Recommendation di atas bukan keputusan arsitektur target dan harus melewati requirement gate serta
hospital-domain architecture/design blueprint sebelum implementation planning.

## 11. Unknown dan closure questions

| Unknown | Mengapa belum tertutup | Evidence/owner yang diperlukan |
| --- | --- | --- |
| Stable source identity per producer | Procedure dan prescription punya ID, tetapi granularity charge/line belum disepakati teknis | Contract producer dan aturan uniqueness per source line |
| Concurrency satu invoice per encounter | Tidak ada aggregate atau database constraint | Target persistence invariant dan retry/idempotency policy |
| Data legacy dengan marker Billing tidak konsisten | Tidak ada reconciliation report/migration yang ditemukan | Profil data database read-only dan keputusan remediation |
| Contract transfer rajal -> ranap | Encounter ownership ada, hubungan transfer finansial belum diaudit | Evidence Registration/Inpatient dan domain architecture |
| Endpoint/route diskon legacy | FE mereferensikan path yang tidak ada pada App Router; backend owner tidak ditemukan pada audit terarah | Konfirmasi apakah source/history lain masih authoritative |
| AP doctor downstream contract | Model payroll ada tanpa service/producer | Owner AP: input line, timing “born”, readiness gate, reversal |
| Payment gateway provider contract | Hanya integration code pada master | Provider/API, callback security, idempotency, reconciliation SLA |
| Runtime registration/migration state | Source dan migration ada, environment tidak dieksekusi | Deployment/runtime verification pada fase readiness |
| Tax/PPN (`BKC-DEC-041`) | Lampiran menghitung PPN, tetapi tidak membuktikan policy RS | Finance/Tax owner menetapkan applicability, basis, bearer, rate/effective date, dan rounding |
| Primary vs excess allocation (`BKC-DEC-042`) | Legacy fallback excess tidak memvalidasi kontrak dan limit | Payer/Insurance + Finance owner menetapkan priority, partial coverage, limit, dan residual pasien |
| Room-charge calculation (`BKC-DEC-043`) | Legacy memakai durasi dibulatkan ke atas dan minimum satu hari | Inpatient/Finance owner menetapkan trigger, unit hari/cutoff, transfer/class change, leave, dan correction |
| Invoice/due-date/aging origin (`BKC-DEC-044`) | Legacy memakai payment date sebagai invoice date dan auto-aging 90 hari | Finance/AR owner menetapkan invoice date, due date, aging origin, dan perbedaan patient/payer |

Unknown tersebut tidak membatalkan keputusan bisnis yang sudah approved, tetapi memblokir klaim
bahwa capability terkait siap dipakai atau siap direncanakan sampai kontrak teknis/domainnya ditutup.

## 12. Verification evidence dan limitasi

Audit melakukan:

- inventory `DbSet`, model, controller, service, konfigurasi EF, migration, dan DI;
- pencarian writer/reader marker Billing pada tindakan dan resep;
- pencarian capability invoice, payment, deposit, refund, AR, AP, shift, radiologi, dan gateway;
- inventory route, service, state, component, dan test frontend;
- impact scan backend sejak SHA pada interview decisions.
- inspeksi statis 13 service/interface pada lampiran ZIP sebagai evidence legacy terpisah.

Audit tidak menjalankan build, unit test, integration test, migration, atau aplikasi. Alasannya bukan
karena kegagalan build, melainkan karena tugas ini adalah audit source read-only dan tidak ditemukan
test Billing/Kasir yang dapat memberi evidence tambahan. Oleh sebab itu, DI wiring, authorization,
schema deployed, data quality, dan behavior runtime tetap belum terverifikasi.

## 13. Staleness dan impact-scan trigger

Capability map ini harus dianggap stale dan diaudit ulang bila salah satu kondisi berikut terjadi:

- backend HEAD berbeda dari `e6f6ecba1537783ea2eb379ac12cc97790707303` pada area Billing,
  Registration, Clinical, Pharmacy, Laboratory, Radiology, Finance/AR, Payroll/AP, auth, EF, atau DI;
- frontend HEAD berbeda dari `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151` pada route Kasir,
  procedure, prescription, registration payer, discount approval, state/service, atau tests;
- approved decisions revision/hash berubah;
- ditemukan repository/service legacy lain yang masih menjadi runtime authority;
- runtime database menunjukkan table, trigger, job, atau integration consumer yang tidak direpresentasikan source.

Impact scan berikutnya harus mencatat SHA lama/baru, file yang berubah, capability yang terdampak,
dan apakah status pada tabel evidence map tetap valid.

## 14. Handoff

Hasil audit revision `0.2` menegaskan bahwa desain tidak boleh berangkat dari asumsi “transaksi
Billing sudah ada”. Bukti legacy membantu menemukan pola dan konflik, tetapi tidak menjadi current
capability. Handoff ke requirement gate wajib membawa approved decision revision `0.2`, termasuk
`BKC-DEC-031`–`BKC-DEC-044`, tanpa mengadopsi policy legacy sebagai target.
