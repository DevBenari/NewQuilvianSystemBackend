# Billing dan Kasir — Existing Capability Map

| Field | Nilai |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Capability-map revision | `0.3` — bertambah section 17 (impact scan 4 September 2026); section 1–16 tidak diubah |
| Status | `source-audited`; belum menyatakan siap implementasi atau siap produksi. **Section 1–16 `STALE` terhadap `HEAD` `fd4a605`** — lihat section 17 |
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

## 15. Impact scan — 27 Agustus 2026

| Field | Nilai |
| --- | --- |
| Trigger | Permintaan `/trace-existing-capabilities` setelah amendment `/grill-me` (`BKC-DEC-045`–`056`, Menu Pembayaran + Dokumen Kasir/Kwitansi) |
| Backend SHA dibandingkan | `e6f6ecba1537783ea2eb379ac12cc97790707303` (audit revision `0.2`) → `b3c8363f0b1d81cdfe3ad76c031a1fba0c94b8c5` (HEAD saat ini) + working tree belum commit |
| Commit di antara kedua SHA | 90 commit, di luar sesi ini (kemungkinan branch/sesi lain pada `Yasmina`) |
| Working tree belum commit | Seluruh implementasi Menu Pembayaran + Dokumen Kasir/Kwitansi sesi ini (`git status --short`, 35 file) — lihat `blueprint-manifest.md` untuk daftar task terkait |
| Status peta ini | **STALE untuk section 4 (evidence map)** — sebagian besar baris `Missing` pada bagian itu sudah punya implementasi source (lihat 15.1). Section 1–3 dan 5–14 (batas scope, kontrak as-is legacy yang TIDAK disentuh) masih valid berdasarkan bukti di bawah. |

### 15.1 Baris `Missing` pada section 4 yang sudah punya implementasi source

Bukti: `find`/`grep` atas `Areas/HealthServices/BillingManagement` menemukan model, service, dan
controller berikut yang tidak ada pada audit `0.2`. Ini BUKAN hasil kerja sesi ini (sesi ini hanya
menambah Menu Pembayaran, Kwitansi, dan Activate/Delete master data) — sudah ada di source sebelum
sesi ini dimulai, kemungkinan dari sesi/branch lain yang belum tercermin di kolom "Status" section 4.

| Baris section 4 (revision `0.2`) | Bukti source yang ditemukan sekarang |
| --- | --- |
| Aggregate invoice satu encounter (`Missing`) | `Billing/Models/BilInvoice.cs`, `Billing/Controllers/BillingInvoicesController.cs`, `Billing/Services/BillingInvoiceService.cs` |
| Billing item dan idempotensi sumber (`Missing`) | `Billing/Services/BillingChargeSourceAdapter.cs` (`IBillingChargeSourceAdapter`, `UpsertChargeAsync` per `SourceDomain`+`SourceDetailId`) |
| Payment, split tender, dan outstanding (`Missing`) | `Billing/Models/BilSettlement.cs`, `BilTender.cs`, `Billing/Services/BillingSettlementService.cs`, `Billing/Controllers/BillingSettlementsController.cs` |
| Deposit/top-up/progress payment ranap (`Missing`) | `Billing/Models/BilDepositAccount.cs`, `BilDepositMovement.cs`, `Billing/Services/BillingDepositService.cs`, `Billing/Controllers/BillingPatientFundsController.cs` |
| Refund dan reversal (`Missing`) | `Billing/Models/BilRefundCase.cs`, `BilRefundLine.cs`, `BilRefundableCredit.cs`, `Billing/Services/BillingRefundService.cs` |
| Write-off pasien (`Missing`) | `Billing/Models/BilWriteOffCase.cs`, `Billing/Dtos/BillingWriteOffDtos.cs` (service/controller belum diverifikasi eksplisit pada scan ini) |
| AP dokter readiness policy / AR penjamin (`Missing`) | `Billing/Models/BilHandoffAdjustment.cs`, `Billing/Services/BillingArApHandoffService.cs`, `BillingFinancialExceptionService.cs` |
| Shift kasir dan selisih kas (`Missing`) | `Cashier/Models/BilCashierShift.cs`, `BilCashierShiftHandover.cs`, `BilCashVarianceReview.cs`, `Cashier/Services/CashierShiftService.cs`, `Cashier/Controllers/CashierShiftsController.cs` |
| Workspace frontend Billing/Kasir (`Missing`) | Route `src/app/health-services/billing-management/**` sudah ada (invoice list/detail, Menu Pembayaran) — belum di-commit menurut `MODULE-STATUS.md` |

Cross-check: temuan ini konsisten dengan rekonsiliasi yang SUDAH tercatat di
[`MODULE-STATUS.md`](./MODULE-STATUS.md) tanggal 25 Agustus 2026 (`ISSUE-FE-003`). Dokumen itu
sudah lebih akurat mengenai status implementasi transaksi Billing dibanding section 4 pada file
ini — **rujuk `MODULE-STATUS.md` sebagai sumber current-state, bukan section 4 di atas**, sampai
section 4 diperbarui formal ke revision baru.

### 15.2 Temuan baru — belum tercatat di `MODULE-STATUS.md` manapun (perlu keputusan/tindak lanjut)

**A. Fakta klinis (`ClinicalMilestoneFactProducer`) dan charge-source-adapter Billing TIDAK terhubung.**

- Commit di luar sesi ini (bagian dari 90 commit di atas) menambahkan
  `Areas/HealthServices/ClinicalBillingIntegration/` (`TrxClinicalMilestoneFact`,
  `ClinicalMilestoneFactProducer`) di bawah blueprint **`rawat-jalan`** (dokumen
  `docs/module-blueprints/rawat-jalan/execution-evidence-RJ-BIL-BE-002.md`,
  `RJ-BIL-CONFLICT-001-source-audit.md`). `PatientProcedureController.ExecuteProcedure`/
  `CancelProcedure` dan `PrescriptionWorkflowService` sudah diubah untuk MENYERAHKAN fakta klinis
  lewat producer ini alih-alih menyatakan sendiri `IsBillingGenerated`/`PaymentStatus` (perbaikan
  tepat atas baris "Tindakan pasien -> Billing" dan "Resep -> Billing" yang ditandai `Repair` pada
  section 4).
- `Billing/Services/BillingChargeSourceAdapter.cs` sudah punya `SourceDomain` yang cocok untuk
  menerima fakta itu: `PROCEDURE`, `LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSUMABLE` (plus
  `ADHOC` yang ditambah sesi ini).
- **Tidak ditemukan satu pun kode yang membaca `TrxClinicalMilestoneFact` dan memanggil
  `UpsertChargeAsync`/endpoint `from-source` untuk domain `PROCEDURE`/`LABORATORY`/`RADIOLOGY`/
  `PHARMACY`/`CONSUMABLE`** (`grep -rl "TrxClinicalMilestoneFact"` hanya menunjuk ke folder
  `ClinicalBillingIntegration` sendiri; `grep` pemanggil `UpsertChargeAsync`/`from-source` hanya
  menunjuk ke Billing sendiri dan FE ad-hoc sesi ini). Produser fakta klinis dan konsumer Billing
  sama-sama sudah dibangun, tetapi oleh dua alur kerja yang tampaknya tidak saling mengetahui.
- **Dampak**: jalur `order pelayanan -> billing item idempotent` yang dinyatakan `Missing`
  end-to-end pada audit `0.2` (lihat bagian 3) **masih belum terbukti end-to-end hari ini** —
  gap-nya berpindah dari "belum ada produser maupun konsumer" menjadi "produser dan konsumer ada,
  belum disambungkan". Ini TIDAK sama dengan `BKC-BLK-INT-001` (yang soal kontrak konsumer AR/AP di
  hilir, bukan soal intake fakta klinis di hulu) — belum tercatat sebagai dependency/blocker di
  `MODULE-STATUS.md` mana pun yang ditemukan pada scan ini.
- **Perlu keputusan**: siapa pemilik pekerjaan menyambungkan keduanya (billing-kasir konsumsi
  `TrxClinicalMilestoneFact`, atau rawat-jalan memanggil Billing langsung) — pertanyaan lintas
  modul, tidak diputuskan pada scan ini.

**B. Konflik granularitas nomor Kwitansi (`BKC-DEC-054`) vs bukti legacy `KasirQuilvian1`.**

- `KasirQuilvian1/BeKasir/MainKasirDetail.cs` menyimpan `NoKwitansi` pada level **detail
  pembayaran per angsuran** (`MainKasirDetailId`, `AngsuranKe`, `NominalPembayaran`,
  `MetodePembayaranId` — satu baris per tender), bukan pada level header (`MainKasirId`/invoice).
  `MainKasirController.cs` memanggil `_noKwitansiService.GenerateNoKwitansiAsync(...)` di dalam
  loop per detail dengan komentar eksplisit "kwitansi unik per baris" (baris 437, 940-941, 1225) dan
  "kwitansi sekarang per baris" (baris 1013).
- `BKC-DEC-054` (hasil `/grill-me` sesi ini) memutuskan SATU nomor Kwitansi per **invoice**,
  dialokasikan sekali dan dipakai ulang untuk semua reprint — sudah diimplementasikan di
  `BilInvoice.KwitansiNumber` + `BillingInvoiceService.GetOrAllocateKwitansiNumberAsync`.
- Rekomendasi yang saya berikan saat `/grill-me` (dipilih user sebagai opsi "Direkomendasikan")
  DIBUAT TANPA memeriksa pola legacy ini terlebih dahulu. Kode `GenerateNoKwitansiAsync` yang
  sebelumnya di-paste user sebagai referensi eksplisit ("saya menggunakan kode itu") berasal dari
  file yang SAMA (`MainKasirController.cs`) yang justru memanggilnya per baris/tender, bukan sekali
  per invoice.
- **Perlu keputusan pemilik produk**: apakah granularitas per-invoice (sudah dibangun) tetap
  dipertahankan, atau diganti ke per-tender (`BilTender` belum punya field nomor Kwitansi — akan
  perlu kolom baru + alokasi per `AddTenderAsync`, bukan sekadar perubahan kecil) agar konsisten
  dengan pola legacy dan kebutuhan audit "satu bukti bayar per transaksi tunai/transfer yang
  benar-benar terjadi". Tidak diubah pada trace ini — hanya dicatat sebagai conflict.

**C. Scope "Struk Pasien" pada Dokumen Kasir kemungkinan salah ditempatkan sebagai "milik modul lain".**

- `BKC-DEC-052` (sesi ini) memutuskan hanya Kwitansi yang menjadi tanggung jawab billing-kasir;
  Struk Pasien dkk. hanya placeholder tab yang menaut ke modul lain.
- Bukti legacy: `KasirQuilvian1/FE kasir view/detail-pembayaran-sukses/Struk-Pasien.jsx` dan
  `pembayaran-sukses.jsx` menunjukkan Struk Pasien historisnya JUSTRU dibangun dan dimiliki oleh
  Kasir/Billing sendiri — data diambil dari `fetchBillingByKunjunganId` (slice Billing legacy),
  berisi rincian obat/tindakan/racikan/biaya admin (identik dengan data yang sudah tampil di tabel
  "Tagihan Pasien" pada Menu Pembayaran sesi ini), dan tombol WhatsApp/Email/Cetak pada layar itu
  justru terpasang di komponen Struk Pasien ini, bukan di komponen Kwitansi terpisah.
- Free-text jawaban user saat `/grill-me` ("Dokumen kasir terdiri dari SPT, Claim Letter, LML,
  LMA, Resep Obat, dan Bukti Pembayaran") tidak menyebut "Struk Pasien" secara eksplisit — tab itu
  hanya muncul dari screenshot referensi UI, sehingga belum tentu user bermaksud memasukkannya ke
  daftar "bukan tanggung jawab billing-kasir".
- **Perlu keputusan pemilik produk**: apakah Struk Pasien (rincian tagihan tercetak) sebenarnya
  IN-SCOPE billing-kasir — dan bisa dibangun dengan reuse data yang sudah ada di Menu Pembayaran —
  alih-alih tetap sebagai placeholder "milik modul lain". Tidak diubah pada trace ini.

### 15.3 Verifikasi non-duplikasi (tidak ada tindak lanjut diperlukan)

`InvoicePatientSummaryResponse` (baru, sesi ini, `Billing/Dtos/BillingInvoiceDtos.cs`) vs
`PatientSummaryResponse` (`PatientManagement/MasterData/Controllers/PatientController.cs`) —
DIPERIKSA, bukan duplikasi. `PatientSummaryResponse` adalah agregat dashboard (total/active/
inactive/newborn/member/deceased/merged count pasien), bukan detail per-invoice/encounter. Nama
mirip hanya kebetulan; tidak ada capability yang seharusnya di-reuse yang terlewat.

### 15.4 Rekomendasi tindak lanjut

1. Temuan 15.2.A (integrasi fakta klinis <-> charge intake) berdampak LEBIH LUAS dari sekadar
   billing-kasir — melibatkan blueprint `rawat-jalan`. Sebaiknya dibawa ke pemilik kedua modul
   sebagai keputusan lintas modul, bukan diputuskan sepihak oleh salah satu sesi build.
2. Temuan 15.2.B dan 15.2.C berdampak langsung pada implementasi Kwitansi/Dokumen Kasir yang baru
   selesai dibangun sesi ini — sebaiknya diklarifikasi ke Product/Domain Owner SEBELUM revision
   `BKC-DEC-052`/`054` dinaikkan statusnya dari `draft` ke `approved`, karena keduanya berpotensi
   mengubah desain yang sudah diimplementasikan.
3. Section 4 pada file ini (revision `0.2`) sebaiknya di-refresh formal ke revision baru bila
   kapasitas tersedia — `MODULE-STATUS.md` sudah menjadi sumber current-state yang lebih akurat
   untuk sementara, tetapi bukan pengganti permanen capability map ini.

## 16. Impact scan — 2 September 2026 (entri manual katalog tarif + coverage per item)

| Field | Nilai |
| --- | --- |
| Trigger | `/trace-existing-capabilities` setelah amendment `/grill-me` (`BKC-DEC-059`–`062`, form "Buat Invoice Manual (Testing)" berbasis `MstTariff` + coverage per item) |
| Backend SHA diaudit | `17b9c0e21e32b41a8dfd6dbde31462d52717646b` (branch `Yasmina`) |
| Frontend SHA diaudit | `60febdcdbb39de6cebc2d825906bce949f3b5af3` (branch `yasmina`) |
| Batas scan | Terbatas pada touch point `BKC-DEC-059`–`062`: ingestion charge ADHOC, master data `MstTariff`/`MstTariffCategory`, mesin coverage (`BillingManagement` dan `ClinicalManagement`), `MstTaxRule`, dan konsumer frontend form testing. BUKAN audit ulang menyeluruh section 1-15. |
| Status peta ini | Section 1-15 tetap berlaku apa adanya (di luar batas scan ini). Section baru ini menambah, tidak menggantikan. |

### 16.1 Tabel bukti kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| CAP-01 | Kategori Biaya dari `MstTariffCategory` pada form testing | Billing FE | `QuilvianSystemFrontendDev/src/lib/hooks/.../use-create-manual-invoice.js#L78` (`getTariffCategoryOptions`)@60febdc | **Ready to reuse** | Tidak ada — sudah jalan | - |
| CAP-02 | Dropdown item searchable dari `MstTariff`, difilter kategori+konteks encounter (`BKC-DEC-061`) | Billing BE+FE | `NewQuilvianSystemBackend/Areas/HealthServices/MasterData/Controllers/TariffController.cs#GetTariffOptions@17b9c0e` (query params `tariffCategoryId`, `serviceUnitId`, `clinicId`, `patientClassId`, `search`, plus `NormalPrice`+nama scope pada response); `QuilvianSystemFrontendDev/.../master-data-tariff-slice.jsx#getTariffOptions@60febdc` | **Ready to reuse** (backend+FE data layer) / **Missing** (komposisi UI dropdown-searchable di form testing itu sendiri — belum ada, tapi pola serverSide-searchable sudah dipakai field `encounterId` pada form yang sama) | UI baru menyusun `BaseSelectField` serverSide memakai thunk yang sudah ada; tidak perlu endpoint baru | Rendah |
| CAP-03 | Harga otomatis dari `MstTariff.NormalPrice`, tidak bisa diinput manual (`A.3`) | Billing BE | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs#UpsertChargeRequest.UnitPrice@17b9c0e` — `UnitPrice` saat ini SELALU dipercaya dari client untuk seluruh pemanggil publik `POST from-source` (dipakai form testing) | **Missing** | Perlu endpoint/parameter baru yang menolak `UnitPrice` dari client untuk kasus tarif-katalog, dan mengambil `NormalPrice` dari `MstTariff` sisi server sebelum memanggil pipeline `UpsertChargeAsync` yang sudah ada (idempotensi/locking/invoice-upsert tetap dipakai ulang) | Sedang — harus dipastikan pemanggil server-to-server lain (adapter klinis) tidak terdampak |
| CAP-04 | Endpoint preview coverage per item (`BKC-DEC-060`) | Billing BE (baru) + ClinicalManagement (reuse) | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs#ResolveTariffAsync@17b9c0e` — method generik `(encounterId, tariffId, quantity, serviceDate)` SUDAH ADA, tapi **tidak pernah dipanggil dari controller manapun** (hanya `ResolveDrugAsync`/`ResolveProcedureAsync` yang dipakai, selalu di dalam alur commit resep/tindakan, bukan sebagai preview read-only berdiri sendiri) | **Reuse with adapter** | Bungkus `ResolveTariffAsync` dengan endpoint `GET` tipis baru (read-only, tanpa side effect) — TIDAK perlu menulis ulang logika matching rule dari nol | Rendah — method sudah teruji lewat jalur resep/tindakan produksi |
| CAP-05 | `IsNeedApproval`/`IsNeedGuaranteeLetter` tidak menggagalkan penghitungan coverage (`BKC-DEC-062`) | Billing BE + ClinicalManagement | `InsuranceCoverageService.cs#ResolveTariffInternalAsync@17b9c0e` baris ~228-289: `CoveredAmount`/`PatientPayAmount` **SUDAH dihitung penuh terlepas dari** `IsNeedApproval`/`IsNeedGuaranteeLetter`/limit bulanan — kedua flag itu HANYA jadi informasi/warning pada hasil, bukan gating. Sebaliknya, `RegistrationBillingCoverageAdapter.cs#ResolveAsync@17b9c0e` (dipakai kalkulasi invoice resmi Menu Pembayaran) MEMANG menggeser ke "unresolved" saat flag itu true. | **Conflict** (lihat 16.2.A) | `BKC-DEC-062` sebaiknya diarahkan untuk MENYELARASKAN `RegistrationBillingCoverageAdapter` ke pola yang sudah ada di `InsuranceCoverageService`, bukan menulis perilaku baru dari nol | Lihat 16.2.A |
| CAP-06 | Field service unit/klinik/kelas pasien pada encounter untuk disambiguasi `MstTariff` (`BKC-DEC-061`) | Billing BE | `TrxPatientEncounter.cs@17b9c0e` PUNYA `ServiceUnitId`(Guid, wajib), `ClinicId`(Guid?), `PatientClassId`(Guid?) — tapi `ActiveEncounterOptionResponse@17b9c0e` (dipakai picker "Pasien/Kunjungan" pada form testing) TIDAK mengekspos ketiganya, hanya `GuarantorName` (string) | **Extend** | Tambah 3 field Guid ke `ActiveEncounterOptionResponse` + mapping-nya; data sumber sudah ada, tidak perlu join/kolom baru | Rendah |
| CAP-07 | Nilai `MstTaxRule.AllocationRule` aktif saat ini (untuk memverifikasi apakah "pajak hanya ke Subtotal Mandiri" sudah tercapai lewat config) | Finance/Tax Owner | `MstTaxRule.cs@17b9c0e` tidak memiliki `HasData`/seed di migration manapun (`grep AllocationRule` pada `Migrations/*.cs` hanya schema, bukan `INSERT`) — dikelola murni lewat `TaxRuleService` CRUD, nilai aktual ada di data runtime, bukan source | **Unknown** | Perlu query data (lewat halaman Master Data > Tax Rule di FE, atau akses DB langsung oleh yang berwenang) — di luar wewenang baca statis trace ini | Menentukan apakah `BKC-DEC-062`/tax perlu kerja tambahan atau cukup verifikasi config |
| CAP-08 | Endpoint ingestion ADHOC existing, untuk menilai reuse vs endpoint baru (`BKC-DEC-059`) | Billing BE | `POST other-charges@BillingInvoicesController.cs` (dipakai "Tambah Biaya Lain-lain", kategori di-hardcode ke "Biaya Lain-Lain", TIDAK menerima `CategoryId` dari client) TERPISAH dari `POST from-source@BillingInvoicesController.cs` (dipakai form testing langsung, MENERIMA `CategoryId` bebas) — keduanya sama-sama berujung ke `BillingInvoiceService.UpsertChargeAsync` dengan `SourceDomain="ADHOC"` yang SAMA | **Ready to reuse** (pemisahan endpoint publik yang diminta `BKC-DEC-059` SUDAH ADA secara alami) | Cukup tambah validasi/lookup tarif pada jalur `from-source` (atau endpoint sibling baru); TIDAK perlu SourceDomain baru untuk pemisahan dari "Tambah Biaya Lain-lain" — keduanya sudah terpisah di layer HTTP meski sama-sama `SourceDomain=ADHOC` di database | Rendah, tapi lihat 16.2.B soal keterlacakan `SourceDomain` yang sama |
| CAP-09 | Field kalkulasi untuk split Subtotal Mandiri/Asuransi di Menu Pembayaran (`BKC-DEC-062`) | Billing BE+FE | `BilCalculationVersion.PatientAmount`/`PrimaryAmount`/`ExcessAmount@erd/01-billing-account-charge.md` SUDAH dikembalikan `CalculationResponse` dan SUDAH dikonsumsi `menu-pembayaran-view.jsx@60febdc` (`harusDibayar`, `subtotalAsuransi`) — hanya belum ditampilkan sebagai dua baris subtotal sejajar (saat ini satu total dikurangi baris penjamin) | **Ready to reuse** (data); **Missing** (komposisi tampilan dua baris sejajar) | Murni perubahan JSX/komposisi tampilan di `menu-pembayaran-view.jsx`, tidak perlu field response backend baru | Rendah |

### 16.2 Temuan yang perlu keputusan/tindak lanjut

**A. CONFLICT — dua mesin coverage independen bisa saling tidak sepakat untuk tarif+pasien yang sama.**

- `RegistrationBillingCoverageAdapter` (`BillingManagement/Billing/Services/BillingCoverageAdapter.cs`, dipakai `BillingCalculationService` pada SETIAP kalkulasi invoice resmi) hanya mencocokkan `MstInsuranceCoverageRule`, dan menggeser ke "unresolved" bila rule butuh approval/surat jaminan/py sudah kena limit bulanan.
- `InsuranceCoverageService.ResolveTariffAsync/ResolveDrugAsync/ResolveProcedureAsync` (`ClinicalManagement/Services/InsuranceCoverageService.cs`, dipakai `PatientProcedureController`, `PrescribingDrugController`, `PrescriptionItemController`, `PrescriptionCompoundItemController` saat commit tindakan/resep) mensyaratkan **positive-list `MstInsuranceTariff`** lebih dulu ("tidak ada insurance tariff berarti tidak dicover") SEBELUM mengecek `MstInsuranceCoverageRule` — gate yang SAMA SEKALI TIDAK ADA pada `RegistrationBillingCoverageAdapter`. Dan seperti dicatat CAP-05, flag approval/surat jaminan diperlakukan sebagai informasi, bukan gating.
- **Akibat konkret**: untuk tarif yang sama, pasien yang sama, kedua mesin ini bisa menghasilkan kesimpulan coverage yang BERBEDA — satu bisa bilang "tercover" (lewat `MstInsuranceCoverageRule` saja) sementara yang lain bilang "tidak tercover" (karena tidak ada baris `MstInsuranceTariff`), atau sebaliknya untuk kasus butuh-approval.
- Ini BUKAN sekadar soal `BKC-DEC-062` — ini gap arsitektur yang sudah ada SEBELUM permintaan sesi ini, baru terlihat karena `BKC-DEC-060` mengharuskan billing-kasir memanggil salah satu dari keduanya untuk preview coverage.
- **Perlu keputusan `/design-business-module`**: apakah endpoint preview baru (`BKC-DEC-060`) memanggil `InsuranceCoverageService.ResolveTariffAsync` (konsisten dengan alur klinis produksi, tapi BEDA dari mesin yang dipakai Menu Pembayaran hari ini), memanggil `RegistrationBillingCoverageAdapter` (konsisten dengan Menu Pembayaran, tapi mengabaikan gate `MstInsuranceTariff` yang sudah dipakai alur klinis), atau menyelaraskan keduanya jadi SATU mesin (paling konsisten jangka panjang, effort paling besar, dan menyentuh modul `ClinicalManagement`+kemungkinan `insurance-management` sekaligus — bukan keputusan sepihak billing-kasir).
- Konsekuensi ke `BKC-DEC-062`: keputusan "abaikan gating approval, ikuti `CoverageStatus=Covered`" yang dijawab user PERSIS MENIRU perilaku `InsuranceCoverageService` yang sudah ada — memperkuat rekomendasi memilih opsi "selaraskan ke `InsuranceCoverageService`" di atas, tapi TETAP perlu keputusan eksplisit karena menyentuh gate `MstInsuranceTariff` yang belum pernah dibahas user sama sekali.

**B. Ketertelusuran — item dari form testing baru dan "Tambah Biaya Lain-lain" akan sama-sama `SourceDomain=ADHOC`.**

- Karena keduanya menghasilkan `BilInvoiceItem.SourceDomain="ADHOC"` yang identik (lihat CAP-08), laporan/audit yang hanya memfilter `SourceDomain` tidak bisa membedakan item yang harganya "terverifikasi tarif resmi" vs "bebas diisi kasir" setelah `BKC-DEC-059` berjalan.
- **Perlu keputusan `/design-business-module`**: apakah perlu penanda tambahan (mis. konvensi pada `SourceDetailId`, atau kolom baru) supaya kedua jenis ADHOC ini bisa dibedakan di laporan/audit — terutama karena `BKC-DEC-047` menjadikan audit log sebagai satu-satunya kompensasi kontrol untuk jalur bebas, dan sekarang ada jalur ADHOC kedua yang TIDAK butuh kompensasi yang sama.

### 16.3 Rekomendasi tindak lanjut

1. Bawa temuan 16.2.A ke `/design-business-module` sebagai keputusan arsitektur eksplisit — dampaknya lebih besar dari cakupan `BKC-DEC-062` semula, karena menyentuh konsistensi coverage lintas `ClinicalManagement`/`BillingManagement`, bukan cuma form testing.
2. Verifikasi CAP-07 (`MstTaxRule.AllocationRule` aktif) lewat halaman Master Data Tax Rule sebelum desain final memutuskan apakah `BKC-DEC-062`/pajak butuh kerja tambahan.
3. CAP-01, CAP-02 (data layer), CAP-04, dan CAP-08 sudah **Ready to reuse**/**Reuse with adapter** — desain final sebaiknya eksplisit menandai ini supaya implementasi tidak membangun ulang endpoint yang sudah ada.

---

## 17. Impact scan — 4 September 2026 (baseline `ffeb45a8` → `HEAD` `fd4a605`)

| Field | Nilai |
| --- | --- |
| Trigger | Perubahan SHA. Manifest blueprint mencatat baseline bukti `ffeb45a8`; `HEAD` ternyata sudah jauh di depannya. Dipicu oleh penyusunan roadmap revisi `2` (`BE-BKC-022`–`032`, `FE-BKC-018`–`021`) yang seluruhnya disusun di atas baseline lama |
| Backend SHA diaudit | `fd4a605` (branch `Yasmina`) — **52 commit dan 237 berkas source non-dokumentasi** di depan `ffeb45a8` |
| Frontend SHA diaudit | **Tidak dapat diaudit** — repository frontend tidak tersedia pada mesin ini. Seluruh baris frontend di bawah berstatus `Unknown` |
| Batas scan | Terbatas pada capability dan kontrak as-is yang **terdampak perubahan SHA** untuk scope Billing: mesin tanggungan (`BillingCoverageAdapter`), mesin kalkulasi (`BillingCalculationService`), DTO kalkulasi (`BillingInvoiceDtos`), ingestion charge (`BillingInvoiceService`), mesin coverage klinis (`InsuranceCoverageService`), dan model pengecualian finansial. **BUKAN** audit ulang menyeluruh section 1–16 |
| Metode | Pembacaan statis source dan riwayat Git. **Tidak ada build, tidak ada eksekusi test, tidak ada perubahan source** |
| Status peta | Section 1–16 tetap berlaku di luar batas scan ini. Section ini menambah dan **membatalkan sebagian**, tidak menggantikan |

### 17.1 Ringkasan satu kalimat

Lima berkas Billing berubah sejak baseline, dan perubahan itu **sudah menyelesaikan sebagian
pekerjaan yang direncanakan roadmap revisi `2`** — termasuk satu gelombang penuh yang semula
ditandai menunggu jawaban Finance lebih dulu.

> **Diperbarui sesudah jawaban pemilik 4 September 2026.** Temuan "gelombang yang mendahului
> gerbangnya" (`MVP-8`) sudah **tertutup dengan paparan finansial nol** — lihat 17.4.A. Yang
> tersisa dari section ini bukan lagi risiko yang sedang berjalan, melainkan **peluang reuse**:
> sebagian besar `BE-BKC-022`, `024`, dan `026` sudah selesai lewat task ad-hoc di luar roadmap,
> dan roadmap revisi `2` wajib menandainya supaya tidak dibangun ulang.

### 17.2 Berkas yang berubah di dalam batas scan

| Berkas | Baris berubah | Isi perubahan |
| --- | ---: | --- |
| `Billing/Services/BillingCoverageAdapter.cs` | 140 | Pencocokan aturan per dimensi, pencabutan sebagian gerbang, hasil per komponen |
| `Billing/Services/BillingCalculationService.cs` | 144 | Gerbang PPN per jenis kunjungan, penyaluran hasil per komponen ke tiap baris |
| `Billing/Dtos/BillingInvoiceDtos.cs` | 37 | Field rupiah tanggungan per baris |
| `Billing/Services/BillingInvoiceService.cs` | 10 | Satuan item farmasi |
| `ClinicalManagement/Services/InsuranceCoverageService.cs` | 37 | Perbaikan penumpukan persentase co-payment |

### 17.3 Tabel bukti kemampuan

| ID | Kebutuhan (task roadmap yang merencanakannya) | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| CAP-10 | Rupiah tanggungan per komponen berkunci `(ComponentId, ComponentType)` — `BE-BKC-022`, `BKC-DES-015`/`016` | Billing BE | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs#BillingCoverageComponentOutcome@fd4a605` (record berkunci `ComponentId`+`ComponentType`); field per baris pada `Billing/Dtos/BillingInvoiceDtos.cs#L462-465@fd4a605` (`ItemPrimaryAmount`, `ItemUnresolvedAmount`, `TaxPrimaryAmount`, `TaxUnresolvedAmount`); biaya administrasi dan biaya kamar pada `L394-395`, `L414-415` | **Ready to reuse** | Tidak ada. Inti `MVP-4` **sudah berjalan** lewat `BE-BKC-FIX-003` yang kini ter-commit | Rendah |
| CAP-11 | Penanda `isPerItemAllocationAvailable` — `BE-BKC-022`, `FR-BKC-012`, `BKC-DES-004`/`017` | Billing BE | Pencarian `IsPerItemAllocationAvailable` pada seluruh `Areas/` **tidak menemukan satu pun kemunculan**@fd4a605 | **Missing** | Perlu dibuat. Tanpa ini, versi kalkulasi lama terbaca Rp 0 dan tidak dapat dibedakan dari "penjamin menanggung Rp 0" | Sedang — membedakan "tidak ada rincian" dari "rincian bernilai nol" |
| CAP-12 | Penjaga jumlah baris sama dengan total tanggungan — `BE-BKC-022`, `BIL-VAL-028` | Billing BE | Tidak ditemukan pemeriksaan penjumlahan alokasi pada `ApplyCoverageWaterfall@fd4a605` | **Missing** | Perlu dibuat | Sedang — tanpa penjaga, lembar tagihan yang tidak menjumlah lolos ke petugas klaim |
| CAP-13 | Lembar "Invoice Asuransi" — `BE-BKC-023`, `FR-BKC-014`–`017` | Billing BE | Pencarian `insurance-invoice-document` dan `InsuranceInvoiceDocument` pada seluruh `Areas/` **tidak menemukan satu pun kemunculan**@fd4a605 | **Missing** | Seluruh `MVP-5` belum tersentuh | Rendah |
| CAP-14 | Penanda butuh persetujuan/surat jaminan tidak lagi menahan tanggungan — `BE-BKC-024`, `FR-BKC-021` | Billing BE | `BillingCoverageAdapter.cs#ResolveAsync@fd4a605` L125-146 — `IsNeedApproval`/`IsNeedGuaranteeLetter` **sudah tidak ada** pada kondisi penahan, dengan komentar yang menyebut `BKC-DEC-062` | **Ready to reuse** | Tidak ada. Sudah dikerjakan `BE-BKC-021` | Rendah |
| CAP-15 | `CoverageStatus = "NeedApproval"` dan limit bulanan tidak lagi menahan tanggungan — `BE-BKC-024`, `FR-BKC-021`/`022`, `BKC-DEC-071` | Billing BE | `BillingCoverageAdapter.cs@fd4a605` L139-146 — keduanya **MASIH menahan**: `CoverageStatus=="NeedApproval" \|\| MaxAmountPerMonth>0 \|\| MaxQuantityPerMonth>0` → `unresolved += component.Amount` | **Extend** | **Jawaban pemilik 4 September 2026, `BKC-CQ-04` dan `BKC-CQ-07` — DITUTUP (lihat 17.4.E).** Keduanya dicabut penuh untuk rilis ini. `NeedApproval` memang tidak diperlukan. Limit bulanan diperlakukan **selalu tersedia** sampai mesin pemakaian kumulatif dibangun — kemampuan itu ditunda, dicatat sebagai coverage gap tertunda, bukan bagian rilis ini | Rendah — cakupan `BE-BKC-024` kembali ke rencana semula, dua gerbang dicabut tanpa kemampuan baru |
| CAP-16 | Tarif tanpa aturan cocok menjadi tanggungan pasien — `BE-BKC-024`, `FR-BKC-023`, `BKC-DEC-072` | Billing BE | `BillingCoverageAdapter.cs@fd4a605` L110-121 — cabang `rule is null` menulis hasil `0/0` dan **tidak** menambah `unresolved`, dengan komentar yang menyebutnya keputusan pengguna di luar roadmap | **Ready to reuse** | Tidak ada. Sudah dikerjakan `BE-BKC-FIX-004` | Rendah |
| CAP-17 | Batas per kunjungan tetap berlaku — `BE-BKC-024`, `FR-BKC-024` | Billing BE | `BillingCoverageAdapter.cs@fd4a605` L158-163 — `MaxAmountPerVisit` masih dijepit beserta akumulasi `appliedPerVisit` | **Ready to reuse** | Tidak ada. Regresi yang dikhawatirkan roadmap tidak terjadi | Rendah |
| CAP-18 | Anomali data penjamin — `BE-BKC-025`, `FR-BKC-027`–`031`, `BKC-DES-010`–`012` | Billing BE | `BillingCoverageAdapter.cs@fd4a605` L80-81 — penjamin tidak layak / polis tidak aktif / perusahaan asuransi kosong masih menghasilkan `Unresolved(context.Components, "REJECTED")`. Pencarian `DataAnomaly`, `AnomalyCodes`, `hasDataAnomaly` **nihil** | **Missing** | Seluruh `EPIC BKC-07` belum tersentuh. Perilaku hari ini persis yang hendak diganti: nominalnya menggantung, bukan jatuh ke pasien | Tinggi — inilah keadaan yang membuat kasir tidak punya angka yang dapat ditagihkan |
| CAP-19 | **Gerbang PPN rawat inap versus rawat jalan — `BE-BKC-026`, `EPIC BKC-08`, `BKC-DEC-078`/`079`** | Billing BE | `BillingCalculationService.cs@fd4a605` L174 `var isOutpatientForTax = invoice.ServiceType != AdministrationFeeServiceTypes.Ranap;`; L176 diteruskan ke `ApplyInvoiceTax`; L760 tanda tangan `ApplyInvoiceTax(..., bool isOutpatient)`; L749-759 komentar menyebut rawat jalan versus rawat inap sebagai faktor penentu kedua | **Conflict** — lihat 17.4.A | **Sudah terimplementasi seluruhnya**. Roadmap menandainya `BLOCKED` menunggu `BKC-OQ-085`, tetapi **gerbang itu kini tertutup** — lihat 17.4.A | **Rendah** (turun dari Tinggi setelah jawaban pemilik 4 September 2026) |
| CAP-20 | Basis pajak hanya obat dan alat kesehatan — `BE-BKC-026`, `FR-BKC-034` | Billing BE | `BillingCalculationService.cs@fd4a605` L772 `if (!item.IsPharmacy) continue;` di dalam `ApplyInvoiceTax`; L694-710 basis dibentuk dari `item.Category.IsPharmacy` | **Ready to reuse** | Tidak ada | Rendah |
| CAP-21 | Jenis kunjungan diambil dari tagihan, bukan pendaftaran terkini — `BE-BKC-026`, `FR-BKC-035` | Billing BE | `BillingCalculationService.cs@fd4a605` L174 membaca `invoice.ServiceType`, bukan mengambil ulang dari `TrxPatientEncounter` | **Ready to reuse** | Tidak ada | Rendah |
| CAP-22 | Jenis kunjungan tak dikenal tetap dikenai PPN — `BE-BKC-026`, `FR-BKC-036` | Billing BE | `BillingCalculationService.cs@fd4a605` L174 — bentuk `!= Ranap` membuat nilai kosong maupun teks asing jatuh ke sisi dikenai pajak, dan tidak menghentikan perhitungan | **Ready to reuse** | Tidak ada. Perilaku yang diminta `BKC-DES-019` tercapai sebagai akibat bentuk perbandingannya | Rendah — tetapi lihat 17.4.B soal `MCU`/`TELEMEDICINE`/`OTC` |
| CAP-23 | Ember `NonBillableResidualAmount` — `BE-BKC-027`/`028`, `BKC-DES-021`/`022` | Billing BE | Pencarian `NonBillableResidual` pada seluruh `Areas/` **nihil**@fd4a605. `BillingCoverageAdapter.cs@fd4a605` L166-170 masih menulis residual ke `unresolved` | **Missing** | Seluruh `MVP-11` bagian mesin belum tersentuh | Sedang |
| CAP-24 | Kolom `Category` pada kasus write-off — `BE-BKC-027`/`029`, `BKC-DES-024` | Billing BE | `Billing/Models/BilWriteOffCase.cs@fd4a605` L10-20 memuat `Id`, `InvoiceId`, `Amount`, `IsFullSettlement`, `Status`, `RequestedBy`, `ApprovedBy`, `Reason`, `IdempotencyKey`, `PayloadHash`, `CorrelationId` — **tanpa `Category`**. Pencarian `BillingWriteOffCategories` **nihil** | **Missing** | Migration dua kolom `BE-BKC-027` tetap diperlukan apa adanya | Sedang |
| CAP-25 | Jalur aturan `NotCovered` masih ke nominal menggantung — `BE-BKC-030`, revisi `0.9` | Billing BE | `BillingCoverageAdapter.cs@fd4a605` L148-155 — `NotCovered` + `IsAllowExcessPaymentByPatient=false` masih menambah `unresolved` | **Extend** | Sesuai baseline revisi `0.8`. `BE-BKC-030` tetap berlaku apa adanya | Rendah |
| CAP-26 | Nominal penjamin kedua permanen nol — `BKC-DES-014` | Billing BE | `BillingCoverageAdapter.cs@fd4a605` L176-178 — `ExcessStatus` selalu `"NOT_CONFIGURED"` dan `ExcessAmount` selalu `0` | **Ready to reuse** | Tidak ada | Rendah |
| CAP-27 | Penyelarasan dua mesin coverage (tindak lanjut 16.2.A) | Billing BE + ClinicalManagement | `BillingCoverageAdapter.cs#Matches@fd4a605` L184-199 — gerbang tunggal `ItemType == CoverageItemType` **sudah diganti** rantai per dimensi, dengan komentar yang menyatakan penyelarasan ke pola `InsuranceCoverageService.FindCoverageRuleAsync` | **Repair** (sebagian selesai) | Pola pencocokan sudah selaras. Gerbang daftar positif `MstInsuranceTariff` yang hanya ada di mesin klinis **belum** dibahas dan tetap menjadi selisih antar mesin | Sedang — 16.2.A belum tertutup penuh |
| CAP-28 | Konsumen frontend seluruh gelombang — `FE-BKC-018`–`021` | Billing FE | Repository frontend tidak tersedia pada mesin ini; tidak ada bukti yang dapat dikutip@— | **Unknown** | Wajib dipindai terpisah sebelum task frontend disetujui | Sedang — `FE-BKC-018`–`021` disusun di atas bukti frontend yang belum dikonfirmasi ulang |

### 17.4 Temuan yang perlu keputusan atau tindak lanjut

**A. ~~CONFLICT~~ — gerbang PPN mendahului gerbangnya, tetapi paparannya nol. DITUTUP 4 September 2026.**

Temuan awal audit ini: pembebasan PPN rawat inap sudah ada di dalam kode lewat `BE-BKC-FIX-004`
(task ad-hoc di luar roadmap), padahal `BKC-OQ-085` mensyaratkan dampak penurunan tagihan rawat
inap dihitung lebih dulu, dan roadmap revisi `2` menandai `BE-BKC-026` sebagai `BLOCKED` karena itu.

**Pemilik menjawab 4 September 2026: belum ada satu pun tagihan rawat inap yang masuk — pengembangan
masih diuji pada rawat jalan saja.** Konsekuensinya berurutan dan menutup temuan ini:

1. Tidak ada tagihan rawat inap yang kehilangan PPN, karena tagihan rawat inap belum ada.
2. Tidak ada deposit yang telanjur diterima sebesar total lama, sehingga **tidak ada kelebihan bayar**.
3. `BKC-OQ-085` karena itu terjawab: penurunan **nol**, kelebihan bayar **tidak ada**.

Yang semula risiko justru berbalik menjadi keuntungan waktu: perubahan itu mendarat **sebelum**
rawat inap hidup, sehingga rawat inap nanti langsung berjalan dengan perilaku yang benar dan tidak
perlu perubahan perilaku di tengah jalan.

Contoh berangka untuk pembacaan ke depan: pasien rawat inap dengan obat Rp 1.000.000 dan biaya
kamar Rp 2.000.000 pada tarif PPN 11% akan ditagih Rp 3.000.000, bukan Rp 3.110.000 — dan itu
angka pertama yang akan dilihat kasir, bukan koreksi dari angka sebelumnya.

**Satu syarat yang tetap berlaku.** Kesimpulan ini sah **selama** rawat inap belum menerima
tagihan. `BIL-AT-044` tetap wajib dijalankan sebelum rawat inap dinyatakan hidup, karena sesudah
itu tidak ada lagi kesempatan memverifikasi tanpa menyentuh tagihan sungguhan.

**B. ~~Ketiga jenis kunjungan pada `BKC-OQ-083` sudah dikenai PPN hari ini.~~ DITUTUP 4 September 2026.**

`BKC-OQ-083` menanyakan apakah pemeriksaan kesehatan berkala (`MCU`), konsultasi jarak jauh
(`TELEMEDICINE`), dan penjualan bebas (`OTC`) dikenai PPN atau dibebaskan. Bentuk perbandingan
`invoice.ServiceType != Ranap` membuat ketiganya dikenai PPN sebagai perilaku bawaan.

**Pemilik menjawab 4 September 2026: ketiganya belum dipakai sama sekali.** Karena itu tidak ada
pasien yang telanjur dipungut pajak, dan `BKC-OQ-083` turun dari memblokir menjadi pertanyaan yang
perlu dijawab **sebelum** salah satu dari ketiganya diaktifkan — bukan sebelum `MVP-8` berjalan.

**C. Empat dari sebelas task backend roadmap revisi `2` perlu dinilai ulang cakupannya —
diperbarui sesudah jawaban pemilik 4 September 2026 atas `BKC-CQ-04`, `07`, dan `08`.**

| Task | Rencana semula | Keadaan sebenarnya |
| --- | --- | --- |
| `BE-BKC-022` | Membangun alokasi per komponen dari awal | Inti alokasinya **sudah ada** (CAP-10). Sisanya tinggal dua hal: penanda ketersediaan rincian (CAP-11) dan penjaga penjumlahan (CAP-12) |
| `BE-BKC-024` | Mencabut empat gerbang | **Menyempit kembali ke rencana semula.** Dua gerbang sudah tercabut (CAP-14, CAP-16) dan batas per kunjungan aman (CAP-17). Sisa dua gerbang (`NeedApproval`, limit bulanan) **dicabut penuh** sesuai jawaban pemilik — limit bulanan diperlakukan selalu tersedia sampai mesin pemakaian kumulatif dibangun kelak. **Tidak ada kemampuan baru** yang perlu dibangun pada task ini (17.4.E) |
| `BE-BKC-026` | Membangun gerbang PPN | **Sudah selesai seluruhnya** (CAP-19–CAP-22), dan gerbang `BKC-OQ-085` kini **tertutup** karena paparannya nol (17.4.A). Yang tersisa hanya menjalankan `BIL-AT-044` sebelum rawat inap hidup — bukan koding |
| `BE-BKC-032` | Regresi lintas gelombang | **Beban tidak bertambah.** Karena paparan rawat inap nol (17.4.A), tidak ada tagihan lama yang perlu diperiksa ulang. Regresi PPN cukup diverifikasi lewat `BIL-AT-044`/`045`/`046` seperti rencana semula |

Tiga task lain — `BE-BKC-023`, `BE-BKC-025`, dan `BE-BKC-027`–`029` — **cakupannya tetap utuh**;
tidak ada satu pun bagiannya yang sudah dikerjakan.

**D. Peluang reuse yang sebaiknya dinyatakan eksplisit sebelum implementasi.**

`BE-BKC-022` **tidak boleh** membangun ulang `BillingCoverageComponentOutcome` maupun keempat field
per baris yang sudah ada. Menulis ulangnya berarti dua bentuk berbeda untuk angka yang sama, dan
itu persis kesalahan yang dicegah `BKC-DES-015`.

**E. Limit bulanan menuntut tiga keadaan, bukan dua — jawaban pemilik 4 September 2026.**

Audit ini menanyakan apakah limit bulanan dicabut dari perhitungan, tetap menahan dengan jalur
bypass, atau menahan hanya ketika pemakaian nyata terlampaui. Pemilik menjawab dengan rumusan yang
lebih tajam dari ketiganya:

> Jangan langsung membuat item menjadi menggantung hanya karena limit bulanan belum
> terpakai/terhitung. Sistem harus membedakan antara **tanggungan tersedia**, **tanggungan habis**,
> dan **tanggungan belum dapat ditentukan**.

**Cacat perilaku hari ini, dinyatakan tepat.** Kode sekarang menyamakan *"aturan ini punya limit
bulanan"* dengan *"limit bulanan itu sudah habis"*. Keduanya keadaan yang sama sekali berbeda, dan
menyamakannya membuat tanggungan yang sebenarnya masih tersedia ikut menggantung.

**Contoh berangka dari pemilik.** Aturan tanggungan 100% dengan limit bulanan Rp 500.000, pemakaian
bulan berjalan Rp 0, sehingga sisa limit Rp 500.000. Item konsultasi Rp 100.000.

| Nilai | Hasil yang benar | Perilaku hari ini |
| --- | ---: | ---: |
| Biaya yang memenuhi syarat | Rp 100.000 | Rp 100.000 |
| Ditanggung penjamin | **Rp 100.000** | Rp 0 |
| Tanggungan pasien | **Rp 0** | Rp 0 |
| Sisa tagihan | **Rp 0** | Rp 0 |
| Nominal menggantung | **Rp 0** | **Rp 100.000** |

Tiga keadaan yang wajib dibedakan, beserta ke mana nominalnya pergi:

| Keadaan | Kapan terjadi | Ke mana nominalnya |
| --- | --- | --- |
| Tanggungan **tersedia** | Sisa limit bulan berjalan masih menutup nominal item | Ditanggung penjamin, seperti aturan tanpa limit |
| Tanggungan **habis** | Sisa limit sudah tidak menutup nominal item | Bagian yang tidak tertutup mengikuti aturan residual yang sudah berlaku, bergantung penanda boleh-tidaknya ditagihkan ke pasien |
| Tanggungan **belum dapat ditentukan** | Sistem tidak dapat menghitung pemakaian bulan berjalan | Menggantung — dan **hanya keadaan inilah** yang sah menggantung |

**Kemampuan yang belum ada.** Ketiga keadaan itu menuntut sistem menghitung *pemakaian bulan
berjalan* dan *sisa limit* per aturan per pasien. Pencarian pada `BillingCoverageAdapter.cs@fd4a605`
tidak menemukan satu pun akumulasi lintas kunjungan; yang ada hanya `appliedPerVisit`, yaitu
akumulasi **dalam satu tagihan** untuk `MaxAmountPerVisit`. Membangun ketiga keadaan sekaligus
berarti kemampuan baru, bukan pencabutan gerbang.

**Jawaban pemilik 4 September 2026, `BKC-CQ-07` — DITUTUP.** Mesin pemakaian kumulatif **tidak**
dibangun pada rilis ini. Sampai mesinnya ada, limit bulanan **diperlakukan sebagai selalu tersedia**
— sama artinya dengan mencabut gerbangnya sepenuhnya, persis seperti `NeedApproval`. Keadaan
"tanggungan habis" dan "tanggungan belum dapat ditentukan" **keduanya ditunda**; hanya "tersedia"
yang berlaku untuk sementara.

**Akibatnya, `BE-BKC-024` kembali ke cakupan semula.** Kedua gerbang — `NeedApproval` dan limit
bulanan (`MaxAmountPerMonth`/`MaxQuantityPerMonth`) — dicabut penuh dari kondisi penahan pada
`ResolveAsync`, tanpa mesin pemakaian kumulatif apa pun. Ini **tidak** melebarkan scope roadmap
revisi `2`; ia mengembalikannya persis seperti rencana semula, hanya dengan alasan yang sekarang
tercatat eksplisit.

**Mesin pemakaian kumulatif dicatat sebagai kemampuan tertunda**, bukan dibuang. Ia akan
dibutuhkan begitu Finance/AR ingin gerbang limit bulanan benar-benar ditegakkan, bukan sekadar
disetel dan tidak berpengaruh. Sampai saat itu, kolom `MaxAmountPerMonth`/`MaxQuantityPerMonth`
tetap ada dan tetap dapat diisi admin (sesuai `BKC-DEC-071` asli), tetapi nilainya **murni
informasi** — sama seperti nasib `IsNeedApproval`/`IsNeedGuaranteeLetter` sejak `BE-BKC-021`.

**`BKC-DES-027`, jawaban pemilik — DITUTUP, boleh direvisi.** Karena keadaan "belum dapat
ditentukan" **tidak** diimplementasikan pada rilis ini, kesimpulan revisi `0.9` bahwa nominal
menggantung selalu bernilai nol **tetap berlaku untuk rilis ini** — tidak ada perubahan mendesak
pada `BE-BKC-030`. Pemilik mengonfirmasi `BKC-DES-027` **boleh direvisi kelak**, pada saat mesin
pemakaian kumulatif benar-benar dibangun dan keadaan "belum dapat ditentukan" diaktifkan. Sampai
saat itu tiba, `BKC-DES-027` dicatat sebagai desain yang **valid untuk keadaan saat ini, dan
diketahui akan direvisi di masa depan** — bukan desain yang salah hari ini.

### 17.5 Fact, inference, dan recommendation

**Fact.** `HEAD` `fd4a605` berada 52 commit di depan baseline `ffeb45a8`. Lima berkas Billing
berubah. Alokasi per komponen, pencabutan dua gerbang, pencocokan aturan per dimensi, dan gerbang
PPN per jenis kunjungan sudah ada di source. Anomali data penjamin, ember selisih tidak dapat
ditagihkan, kategori write-off, penanda ketersediaan rincian, penjaga penjumlahan, dan lembar
Invoice Asuransi tidak ada.

**Inference.** Sebagian besar pekerjaan yang sudah mendarat masuk lewat task ad-hoc di luar
roadmap (`BE-BKC-FIX-003` sampai `FIX-007`), bukan lewat gelombang yang direncanakan. Pola ini
menjelaskan kenapa dokumen blueprint tertinggal dari source: perbaikan berjalan lebih cepat
daripada pencatatannya. Ini inferensi atas pola kerja, bukan penilaian atas kualitas kodenya.

**Recommendation.** Empat hal, berurut, sesudah jawaban pemilik 4 September 2026 atas seluruh
pertanyaan PPN dan limit bulanan.

1. Nilai ulang cakupan `BE-BKC-022`, `024`, `026`, dan `032` pada roadmap revisi `2` mengikuti 17.4.C. Ketiga dari empat task justru **menyempit** dari rencana semula — hanya `BE-BKC-022` yang tetap mengandung pekerjaan baru (CAP-11, CAP-12).
2. Catat mesin pemakaian kumulatif bulanan sebagai **coverage gap tertunda** pada roadmap, bukan bagian rilis ini. Ini keputusan produk yang sudah diambil (17.4.E), bukan lagi pertanyaan terbuka.
3. Jadwalkan `BIL-AT-044` sebagai syarat sebelum rawat inap dinyatakan hidup. Ini satu-satunya sisa `MVP-8`, dan kesempatan menjalankannya tanpa menyentuh tagihan sungguhan akan hilang begitu rawat inap dibuka.
4. Jalankan pemindaian terpisah untuk repository frontend. Seluruh task `FE-BKC-018`–`021` saat ini berdiri di atas bukti yang belum dikonfirmasi ulang (CAP-28).

### 17.6 Closure question untuk `/grill-me`

Enam pertanyaan sudah dijawab pemilik pada 4 September 2026; dua sisanya masih terbuka.

> **Catatan wewenang.** Jawaban di bawah dicatat sebagai **bukti apa yang dinyatakan pemilik**,
> bukan sebagai keputusan bisnis yang sudah berkekuatan. Penerbitan ID keputusan resmi
> (`BKC-DEC-092` dan seterusnya) beserta pencatatannya di `00-interview-decisions.md` adalah
> pekerjaan `/qv-grill`, bukan skill audit ini.

| ID | Pertanyaan | Siapa yang menjawab | Keadaan |
| --- | --- | --- | --- |
| ~~`BKC-CQ-01`~~ | Pembebasan PPN rawat inap berjalan tanpa hitungan dampak `BKC-OQ-085` lebih dulu — dipertahankan atau dikembalikan? | Billing Owner + Finance/AR | **TERJAWAB.** Belum ada tagihan rawat inap sama sekali, sehingga tidak ada yang perlu dikembalikan. Perilaku dipertahankan; lihat 17.4.A |
| ~~`BKC-CQ-02`~~ | Berapa tagihan rawat inap yang sudah kehilangan PPN dan berapa nilai kelebihan bayarnya? | Finance/AR | **TERJAWAB.** Nol tagihan, nol kelebihan bayar — pengembangan masih diuji pada rawat jalan saja |
| ~~`BKC-CQ-03`~~ | `MCU`, `TELEMEDICINE`, dan `OTC` sudah dikenai PPN sebagai perilaku bawaan. Disahkan atau dibebaskan? | Product/Domain Owner + Finance/Tax | **TERTUNDA, tidak lagi mendesak.** Ketiganya belum dipakai. Wajib dijawab **sebelum** salah satunya diaktifkan, bukan sebelum `MVP-8` berjalan |
| ~~`BKC-CQ-04`~~ | `CoverageStatus = "NeedApproval"` dan limit bulanan masih menahan tanggungan, padahal `BKC-DEC-071` memerintahkan dicabut. Belum dikerjakan, atau sengaja? | Billing Owner + Finance/AR | **TERJAWAB.** Keduanya dicabut penuh — lihat `BKC-CQ-07` untuk cara limit bulanan dicabut |
| ~~`BKC-CQ-07`~~ | Keadaan "tanggungan belum dapat ditentukan" membutuhkan mesin pemakaian kumulatif bulanan yang belum ada. Masuk rilis ini, atau limit bulanan sementara "selalu tersedia"? | Product/Domain Owner + Finance/AR | **TERJAWAB.** Limit bulanan **selalu tersedia** sampai mesinnya dibangun. Mesin pemakaian kumulatif **ditunda**, dicatat sebagai coverage gap — lihat 17.4.E |
| ~~`BKC-CQ-08`~~ | `BKC-DES-027` menyimpulkan nominal menggantung selalu nol sesudah revisi `0.9`. Direvisi, atau keadaan "belum dapat ditentukan" diberi ember tersendiri? | Product/Domain Owner | **TERJAWAB.** Boleh direvisi — tetapi baru **saat** mesin pemakaian kumulatif dibangun dan keadaan "belum dapat ditentukan" diaktifkan. Untuk rilis ini `BKC-DES-027` tetap berlaku apa adanya |
| `BKC-CQ-05` | Gerbang daftar positif `MstInsuranceTariff` masih hanya ada di mesin coverage klinis dan tidak ada di mesin Billing (sisa 16.2.A). Apakah selisih ini disahkan sebagai perbedaan yang memang dikehendaki, atau tetap harus disatukan? | Product/Domain Owner + Payer/Insurance | Menyentuh `ClinicalManagement` dan `insurance-management` sekaligus; bukan keputusan sepihak Billing |
| `BKC-CQ-06` | Sebagian besar perubahan mendarat lewat task ad-hoc di luar roadmap. Apakah task ad-hoc semacam itu tetap diizinkan menyentuh mesin perhitungan uang tanpa melewati gerbang roadmap? | Product/Domain Owner | Pertanyaan tata kelola, bukan pertanyaan teknis |
| `BKC-CQ-09` | **Baru.** Kapan mesin pemakaian kumulatif bulanan direncanakan — rilis berikutnya, atau menunggu prioritas lain? | Product/Domain Owner + Finance/AR | Menentukan apakah gap ini masuk backlog dekat atau `POST-MVP` jangka panjang; keputusan penjadwalan, bukan temuan audit |

### 17.7 Limitasi audit ini

1. **Tidak ada build dan tidak ada eksekusi test.** Seluruh kesimpulan berasal dari pembacaan statis source dan riwayat Git.
2. **Repository frontend tidak tersedia.** Seluruh capability frontend berstatus `Unknown`, bukan `Missing` — ketidakhadiran bukti bukan bukti ketidakhadiran.
3. **Bukan audit ulang menyeluruh.** Hanya capability yang terdampak perubahan SHA yang diperiksa; section 1–16 di luar batas itu tidak dinilai ulang.
4. **Nilai data runtime tidak diperiksa.** `MstTaxRule.AllocationRule` (CAP-07) tetap `Unknown` seperti pada section 16, dan jumlah tagihan terdampak pada `BKC-CQ-02` tidak dapat dijawab dari source.

### 17.8 Pemicu impact scan berikutnya

| Yang berubah | Yang wajib ditinjau ulang |
| --- | --- |
| `BillingCoverageAdapter.cs` atau `BillingCalculationService.cs` | CAP-10 sampai CAP-27 seluruhnya — kedua berkas ini memegang perhitungan uang |
| `BilWriteOffCase.cs` atau service pengecualian finansial | CAP-24, beserta rencana migration `BE-BKC-027` |
| `InsuranceCoverageService.cs` | CAP-27 dan sisa temuan 16.2.A |
| SHA frontend | CAP-28 dan seluruh task `FE-BKC-018`–`021` |
| Munculnya `NonBillableResidual`, `DataAnomaly`, atau `IsPerItemAllocationAvailable` di source | CAP-11, CAP-18, CAP-23 — berarti gelombang terkait sudah mendarat di luar roadmap |
