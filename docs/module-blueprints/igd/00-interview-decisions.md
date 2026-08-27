# IGD — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `IGD-BP-001` |
| Revision | `4` |
| Status | `approved sebagian` — lihat [Approval 2026-08-14](#approval-2026-08-14) |
| Interview mode | `Closure pass` |
| Product/domain owner | Pemegang sementara sesuai `IGD-DEC-046`; nama formal perlu diisi |
| Backend SHA | `pending-trace` |
| Frontend SHA | `pending-trace` |
| Primary evidence | `docs/Modul-RS/Alur_Bisnis_Modul_IGD_NewQuilvian.md`, versi 1.0, 5 Agustus 2026 |

## Scope dan Outcome

- **Fact:** Closure Pass 13 Agustus 2026 membaca `01-existing-capability-map.md` dan
  menemukan bahwa model encounter yang diaudit masih membutuhkan identitas pasien, sementara
  kontrak representasi pasien provisional sebelum `PatientId` definitif belum memiliki owner
  maupun approval otoritatif. Ini adalah blocker desain untuk jalur provisional; keputusan dan
  histori Scope Pass sebelumnya tetap berlaku.
- **Fact:** Dokumen sumber mencakup kedatangan, triage/retriage, registrasi, pelayanan
  klinis, observasi, resusitasi, transfer, disposition, billing/administrasi, dan penutupan
  kunjungan IGD.
- **Fact:** Outcome yang dinyatakan adalah satu episode IGD yang dapat ditelusuri melalui
  `EncounterId`, tanpa menduplikasi data klinis umum dan tanpa menghambat pasien gawat.
- **Assumption:** Blueprint bernama canonical `igd` dan dokumen sumber menjadi input Scope
  Pass, bukan bukti approval final.
- **Assumption:** Audit capability backend dan frontend belum dilakukan; nama class, tabel,
  konfigurasi, dan integrasi di dokumen sumber masih harus dibuktikan dari source code.
- **Decision (draft):** Mode korban massal/bencana termasuk scope rilis awal modul IGD.

## Glossary

| Istilah | Makna kerja saat ini | Status |
|---|---|---|
| Episode IGD | Satu rangkaian pelayanan IGD yang berpusat pada satu `TrxPatientEncounter` dan satu `TrxEmergencyVisit` aktif | `draft` |
| Encounter provisional | Encounter yang memungkinkan pasien gawat atau pasien dengan identitas/administrasi belum lengkap langsung memperoleh pelayanan IGD tanpa menunggu registrasi lengkap | `draft`; field minimum ditetapkan pada `IGD-DEC-016` |
| Temporary patient | Identitas sementara yang dipakai ketika pasien belum dapat diidentifikasi definitif saat pelayanan IGD dimulai | `draft`; lifecycle ditetapkan pada `IGD-DEC-017` |
| Reconciliation temporary patient | Proses terkontrol untuk menghubungkan `TemporaryPatientId` ke `PatientId` definitif tanpa membuat encounter baru atau memindahkan transaksi klinis | `draft`; role approver kasus ambigu mengikuti SOP MMC Hospital |
| Registrasi lengkap | `AdministrativeStatus = Complete` setelah seluruh data administratif wajib menurut SOP/konfigurasi registrasi MMC terpenuhi | `draft`; daftar field final tidak di-hardcode di workflow IGD |
| Stabil | Kondisi yang menurut tenaga klinis berwenang memungkinkan proses administratif dilakukan; sistem tidak menentukannya otomatis dari triase atau vital sign | `draft` |
| Disposition | Keputusan klinis dokter mengenai kelanjutan pasien | `draft` |
| Transfer | Eksekusi perpindahan pasien, terpisah dari keputusan disposition | `draft` |
| Selesai/Completed | Episode klinis selesai setelah disposition dieksekusi dan seluruh clinical/transfer/discharge closure gate terpenuhi | `draft`; billing final bukan universal gate |
| Billing status | Lifecycle finansial terpisah: `NotStarted`, `InProgress`, `Pending`, `Outstanding`, `Final`, atau `Cancelled` | `draft`; nama enum final mengikuti convention existing setelah capability audit |
| Financial clearance | Evaluasi bahwa tanggung jawab finansial encounter cukup terselesaikan untuk mengizinkan physical release; berbeda dari billing completion, pelunasan seluruh invoice, atau penerimaan seluruh receivable | `draft`; hasil konseptual: `Pending`, `ClearedByPayment`, `ClearedByCoverage`, `ClearedByGuarantee`, `ClearedByException`, `Blocked` |
| Exceptional departure | Event ketika pasien faktual meninggalkan rumah sakit tanpa financial clearance atau exception berwenang; bukan `AdministrativeReleaseStatus = Released` | `draft`; nama final, misalnya `LeftWithoutFinancialClearance`, mengikuti convention/SOP |
| Administrative release status | Keputusan kesiapan/pelepasan fisik pasien yang terpisah dari completion klinis: `NotReady`, `Waiting`, `Cleared`, atau `Released` | `draft`; nama enum final mengikuti convention existing setelah capability audit |

## Aktor dan Tanggung Jawab

| Aktor | Tanggung jawab yang dinyatakan dokumen | Status |
|---|---|---|
| Petugas penerimaan/registrasi | Identifikasi awal, membuat atau melengkapi encounter, penjamin, penanggung jawab, dan rujukan | `draft` |
| Petugas Registrasi/Master Patient | Melakukan pencarian kandidat dan reconciliation temporary patient; dapat menyelesaikan kasus simple-match dengan permission `PatientIdentity.Reconcile`/`PatientIdentity.Merge`, atau menjadi maker pada kasus ambigu | `draft`; pemetaan nama role organisasi bersifat configurable |
| Approver patient identity | Menyetujui atau menolak merge/reconciliation ambigu atau konflik dengan permission `PatientIdentity.ApproveAmbiguousMerge`; harus berbeda `UserId` dari maker | `draft`; nama role organisasi final mengikuti struktur kewenangan MMC Hospital |
| Perawat triage | Triage awal, ABCDE/red flag, level prioritas, dan retriage | `draft` |
| Dokter IGD | SOAP/konsultasi, diagnosis, tindakan, serta menetapkan disposition | `draft` |
| Perawat IGD | Monitoring, CPPT, observasi, edukasi, dan handover sesuai kewenangan klinis | `draft` |
| Tim/unit tujuan | Menerima atau menolak transfer dan mengonfirmasi kedatangan | `draft` |
| Billing/administrasi | Finalisasi biaya dan kekurangan administrasi sebelum penutupan sesuai kebijakan | `draft` |
| Product/domain owner | Memutus scope dan aturan bisnis IGD | `OPEN — orang/role belum ditetapkan` |
| Clinical governance owner | Menyetujui sistem triage, target respons, override, dan aturan keselamatan klinis | `OPEN — orang/role belum ditetapkan` |
| Security/privacy owner | Menyetujui akses, audit, data pasien tak dikenal, serta retention | `OPEN — orang/role belum ditetapkan` |
| Kepala IGD | Mengaktifkan mode korban massal/bencana dalam status menunggu konfirmasi; tidak boleh menonaktifkan kecuali resmi merangkap incident commander | `draft` |
| Incident commander | Mengaktifkan langsung sebagai terkonfirmasi dan dapat menonaktifkan mode korban massal/bencana | `draft` |
| Direktur rumah sakit | Mengonfirmasi/menolak aktivasi, menerima notifikasi, dan dapat menonaktifkan mode | `draft` |
| Pejabat/komite bencana yang ditunjuk | Mengonfirmasi/menolak aktivasi dan menerima notifikasi sesuai penunjukan resmi | `draft` |

## Business Rules dan Invariants

Pernyataan berikut berasal dari dokumen sumber dan dicatat sebagai keputusan **draft**, belum
sebagai fakta source code atau approval manusia:

1. Encounter IGD menggunakan tipe `OP`, asal kunjungan IGD, dan `ServiceUnitId` IGD.
2. `TrxPatientEncounter` menjadi pusat episode; `TrxEmergencyVisit` menjadi extension IGD.
3. Satu `EncounterId` hanya boleh memiliki satu `TrxEmergencyVisit` aktif.
4. Pelengkapan administrasi provisional tidak boleh membuat encounter kedua.
5. Pasien level 1–2 dapat dilayani dengan encounter provisional dan identitas minimum.
6. Data klinis umum dimiliki Clinical Management; data yang unik bagi IGD dimiliki
   Emergency Installation Management.
7. Retriage adalah transaksi baru pada visit yang sama dan tidak menimpa histori triage.
8. Tindakan utama tetap menjadi `TrxPatientProcedure`; atribut khusus IGD menjadi extension.
9. Observation/resuscitation tidak boleh selesai sebelum waktu mulai.
10. Disposition dan transfer adalah konsep terpisah.
11. Visit hanya dapat ditutup setelah disposition dieksekusi dan gate yang relevan terpenuhi.
12. Data klinis memakai soft delete dan audit trail.
13. Log mutasi tidak boleh membawa identitas pasien atau payload klinis lengkap.
14. Arah triase yang dipilih adalah kategori warna Indonesia: Merah, Kuning, Hijau, dan
    Hitam. Permenkes 47/2018 menerapkan kategori ini pada pelayanan IGD rumah sakit;
    kejadian bencana/korban massal menambahkan tag triase dan tata kelola krisis.
15. Kategori Hitam tidak boleh ditentukan otomatis oleh aplikasi. Dokumen regulasi
    mendefinisikannya sebagai pasien meninggal atau cedera fatal yang jelas dan tidak mungkin
    diresusitasi; penetapan pada pasien konkret memerlukan tenaga klinis berwenang, alasan,
    waktu, dan audit trail.
16. Aktivasi oleh Kepala IGD menghasilkan status `Active — Pending Confirmation` agar respons
    klinis dan operasional tidak menunggu administrasi.
17. Aktivasi oleh incident commander yang telah ditetapkan resmi menghasilkan status
    `Active — Confirmed`; direktur dan komite bencana tetap menerima notifikasi.
18. Penonaktifan hanya boleh dilakukan incident commander atau direktur. Kepala IGD hanya
    boleh menonaktifkan apabila pada saat itu juga ditetapkan resmi sebagai incident commander.
19. Aktivasi, konfirmasi, penolakan konfirmasi, dan penonaktifan wajib mencatat identitas dan
    jabatan pelaku, waktu server, alasan, jenis/lokasi kejadian, sumber laporan, referensi
    bukti, serta status sebelum dan sesudah tindakan.
20. Lampiran bukti tidak boleh menjadi prasyarat aktivasi. Alasan dan sumber laporan wajib
    saat aktivasi; dokumen/foto/bukti tambahan boleh dilengkapi setelah mode aktif.
21. Bukti sensitif disimpan pada penyimpanan terbatas. Audit log hanya menyimpan ID/referensi
    bukti dan tidak boleh memuat identitas pasien atau payload klinis lengkap.
22. Batas waktu konfirmasi dan jalur eskalasi mengikuti SOP internal MMC Hospital atau
    Hospital Disaster Plan yang dapat dikonfigurasi; keduanya tidak boleh di-hardcode.
23. Penolakan konfirmasi tidak menonaktifkan mode korban massal/bencana atau menghentikan
    pelayanan yang sedang berjalan. Mode tetap aktif dengan penanda `Confirmation Rejected`
    sampai incident commander atau direktur melakukan penonaktifan eksplisit; penolakan dan
    penonaktifan dicatat sebagai event audit terpisah.
24. Provisional encounter memungkinkan pelayanan IGD segera bagi pasien gawat atau pasien
    dengan identitas/administrasi belum lengkap. Field wajibnya adalah `EncounterId` buatan
    sistem; `PatientId` atau `TemporaryPatientId`; nama/alias sementara, jenis kelamin, dan
    perkiraan umur/tanggal lahir bila diketahui; `ServiceUnitId` IGD; tipe/asal kunjungan IGD;
    waktu kedatangan; cara kedatangan bila diketahui; alasan provisional; serta `CreatedBy` dan
    `CreatedAt`.
25. NIK, nomor rekam medis existing, alamat lengkap, nomor telepon, keluarga/penanggung jawab,
    penjamin/asuransi, dokumen identitas, dan data rujukan bukan blocker pembuatan provisional
    encounter bagi pasien gawat atau pasien tidak dikenal.
26. Administrasi dilengkapi sesegera mungkin ketika tenaga klinis berwenang menyatakan kondisi
    pasien memungkinkan proses administratif. Reminder dan eskalasi mengikuti SOP rumah sakit
    yang dapat dikonfigurasi, tanpa batas waktu hard-code; keterlambatan tidak boleh otomatis
    menghentikan atau memblokir pelayanan klinis yang berjalan.
27. Petugas registrasi/admission IGD adalah penanggung jawab utama pelengkapan registrasi.
    Dokter dan perawat bertanggung jawab atas data klinis dan dapat menyatakan bahwa proses
    administratif sudah dapat dilakukan.
28. Pelengkapan provisional menjadi registrasi lengkap dilakukan pada `EncounterId` yang sama;
    sistem tidak boleh membuat encounter kedua dan seluruh triage, retriage, tindakan,
    observasi, order, serta data klinis yang ada tetap tertaut kepadanya.
29. Status klinis dan administratif dipisahkan. Encounter provisional dapat memiliki
    `EncounterStatus = Provisional`, `AdministrativeStatus = Incomplete`, dan
    `ClinicalServiceStatus = InProgress`. `AdministrativeStatus` hanya menjadi `Complete`
    setelah seluruh data administratif wajib menurut SOP/konfigurasi registrasi MMC terpenuhi;
    daftar field final tidak di-hardcode di workflow IGD.
30. `TemporaryPatientId` digunakan bila pasien belum dapat diidentifikasi definitif saat
    pelayanan IGD dimulai. Lifecycle-nya adalah `Temporary → IdentityFound →
    ReconciliationPending → Merged / Resolved`; sistem tidak melakukan merge otomatis ketika
    identitas mulai diketahui.
31. Sebelum membuat `PatientId` definitif, sistem wajib mencari kandidat pasien existing dengan
    prioritas identifier nasional yang valid, nomor rekam medis existing, kombinasi nama dan
    tanggal lahir/umur serta jenis kelamin, lalu nomor telepon atau demografi lain bila tersedia.
    Hasilnya hanya candidate match; similarity nama/demografi tidak boleh memicu automatic merge.
32. Bila satu kandidat diyakini benar, petugas berwenang melakukan reconciliation ke `PatientId`
    tersebut. Bila kandidat lebih dari satu, identifier bertentangan, atau identitas diragukan,
    status tetap `ReconciliationPending` sampai diverifikasi oleh role berwenang lebih tinggi.
    Jika tidak ada pasien existing setelah pencarian, petugas Registrasi/Master Patient dapat
    membuat `PatientId` definitif baru dan melakukan reconciliation.
33. Petugas Registrasi/Master Patient bertanggung jawab utama atas reconciliation. Dokter dan
    perawat dapat mencatat informasi identitas baru serta menandai `IdentityFound`, tetapi tidak
    boleh merge master patient. Kasus ambigu, konflik duplikasi, atau identitas tidak konsisten
    membutuhkan approval Supervisor Registrasi/Master Patient atau role yang ditetapkan SOP.
34. Reconciliation/merge tidak boleh menghentikan pelayanan klinis. Setelah berhasil,
    `PatientId` definitif dipakai selanjutnya, sedangkan `TemporaryPatientId` tidak dihapus,
    berstatus `Merged`/`Resolved`, dan menyimpan referensi historis ke `PatientId` definitif.
35. Reconciliation tidak boleh mengubah `EncounterId`, membuat encounter baru, atau menyalin
    ulang transaksi klinis. Seluruh triage, retriage, tindakan, observasi, CPPT, order, farmasi,
    lab, radiologi, billing, transfer, dan transaksi klinis lain tetap pada encounter yang sama.
36. Setiap reconciliation mencatat `TemporaryPatientId`, `PatientId` definitif, actor dan role,
    approver beserta role bila diperlukan, waktu server, alasan, metode verifikasi identitas,
    status sebelum/sesudah, serta reference ID evidence bila ada. Mutation/audit log hanya
    menyimpan identifier/reference dan metadata perubahan, tanpa PHI atau payload klinis lengkap.
37. Merge bersifat idempotent; request yang sama tidak menghasilkan merge berulang atau
    duplikasi `PatientId`. Temporary patient berstatus `Merged` tidak boleh digunakan untuk
    membuat encounter baru. Kesalahan reconciliation ditangani melalui workflow
    correction/reversal terkontrol dengan authorization dan audit trail tersendiri, tanpa
    menghapus histori atau mengubah merge secara diam-diam.
38. Separation of duties wajib untuk patient merge/reconciliation yang ambigu atau memiliki
    konflik. Kasus simple-match dapat diselesaikan tanpa approval kedua oleh petugas berwenang
    dengan permission `PatientIdentity.Reconcile`/`PatientIdentity.Merge` apabila ada identifier
    kuat yang cocok (misalnya NIK atau nomor rekam medis), hanya satu kandidat valid, tidak ada
    konflik identifier/demografi material, serta tidak ada indikasi dua `PatientId` aktif
    mewakili orang berbeda.
39. Kasus ambigu/konflik wajib memakai maker-checker. Maker mencari kandidat, memeriksa evidence
    identitas, mencatat perbedaan data, memilih kandidat usulan, dan mengajukan reconciliation.
    Approver menyetujui atau menolak dengan permission `PatientIdentity.ApproveAmbiguousMerge`.
    Maker dan approver harus memiliki `UserId` berbeda; self-approval dilarang.
40. Approval wajib apabila ada lebih dari satu kandidat; NIK/identifier bertentangan; nomor
    rekam medis berbeda tetapi diduga pasien sama; konflik material nama/tanggal lahir/jenis
    kelamin; temporary patient memiliki transaksi klinis signifikan tetapi kandidat meragukan;
    dua `PatientId` definitif diduga duplikat; merge pernah dilakukan atau dibatalkan; atau maker
    meminta eskalasi manual. Dokter/perawat bukan approver merge, tetapi dapat memberi informasi
    klinis/identitas pendukung.
41. Business rule tidak boleh meng-hardcode jabatan organisasi. Permission
    `PatientIdentity.Reconcile`, `PatientIdentity.Merge`,
    `PatientIdentity.ApproveAmbiguousMerge`, dan `PatientIdentity.ReverseMerge` dipetakan ke
    role organisasi melalui konfigurasi authorization. Nama role maker dan approver MMC Hospital
    wajib diverifikasi dari SOP, struktur organisasi, atau keputusan owner sebelum produksi.
42. Audit merge/reconciliation mencatat maker dan role; approver dan role bila diperlukan;
    `TemporaryPatientId`; source dan target `PatientId`; waktu server; alasan; hasil pemeriksaan
    duplicate; evidence/reference; status sebelum/sesudah; serta keputusan approve/reject dan
    alasannya. Rejection tidak menghapus data atau mengubah `EncounterId`; temporary patient
    tetap aktif/pending reconciliation. Reversal merge yang salah wajib memakai workflow khusus,
    bukan edit langsung database.
43. Capability model patient identity dikunci dengan `PatientIdentity.Reconcile`,
    `PatientIdentity.Merge`, `PatientIdentity.ApproveAmbiguousMerge`, dan
    `PatientIdentity.ReverseMerge`. Capability reconcile/merge dipetakan ke maker administrasi/
    master patient; approve ambiguous merge ke approver berwenang lebih tinggi yang berbeda
    `UserId` dari maker; dan reverse merge ke role khusus berwenang lebih tinggi.
44. Nama role organisasi MMC Hospital tidak boleh di-hardcode pada business rule atau source
    code. Mapping `Organization Role → Permission/Capability` harus configurable melalui
    authorization system dan dilakukan berdasarkan evidence SOP, struktur organisasi, atau
    keputusan owner tanpa mengubah workflow bisnis utama. Satu role boleh memegang beberapa
    capability jika SOP menetapkannya, namun self-approval untuk merge ambigu/konflik tetap
    dilarang. Sampai evidence tersedia, mapping nama role berstatus `OPEN` dan bukan blocker
    desain modul IGD.
45. Lifecycle encounter IGD memakai authority berbasis capability yang divalidasi backend.
    Registrasi/admission dapat membuat encounter normal atau provisional; clinician berwenang
    dapat membuat provisional dalam emergensi. Dokter IGD berwenang menetapkan/mengubah
    disposition; unit tujuan menerima/menolak transfer; unit pengirim mencatat `Departed`; dan
    unit penerima mengonfirmasi `Arrived`.
46. Completion hanya terjadi setelah backend memvalidasi disposition telah ditetapkan dan
    dieksekusi, gate klinis terpenuhi, transfer telah `Arrived` bila relevan, gate billing/
    administratif terpenuhi sesuai aturan physical release dan payer/payment yang berlaku, serta
    tidak ada mandatory workflow aktif.
    Cancellation hanya untuk encounter keliru tanpa aktivitas klinis material; correction bersifat
    amendment/append-only; reopening adalah exception dengan authorization lebih tinggi.
47. Pasien yang datang kembali setelah episode IGD selesai harus memperoleh `EncounterId` baru,
    bukan membuka kembali encounter lama. Reopen hanya untuk koreksi atau penyelesaian transaksi
    episode lama yang sah dan dapat memakai maker-checker untuk kasus high-impact.
48. Setiap command yang mengubah state wajib memiliki authorization backend, server timestamp,
    actor `UserId`, previous/new state, alasan untuk exception/correction/cancel/reopen/reject,
    idempotency protection, dan immutable audit trail. Frontend hanya menggunakan permission dari
    backend dan tidak menentukan sendiri hak transisi.
49. Clinical completion, billing completion, dan administrative release adalah lifecycle terpisah.
    Encounter IGD boleh `Completed` setelah disposition dan seluruh closure gate klinis/transfer/
    discharge yang relevan terpenuhi walaupun `BillingStatus` masih `Pending` atau `Outstanding`.
    Billing final bukan universal blocker clinical completion.
50. Completion klinis wajib mencerminkan keadaan klinis sebenarnya dan tidak boleh menahan pasien
    sebagai aktif di IGD semata-mata karena billing belum final. Ini mencegah distorsi LOS,
    occupancy, patient queue, workload, dan dashboard operasional.
51. Bila encounter completed dengan billing `Pending`/`Outstanding`, sistem wajib membuat billing
    handoff berisi `EncounterId`, billing status, alasan, owner/queue penanggung jawab, waktu,
    billing/claim reference bila ada, nilai outstanding bila diketahui, next action, SLA/escalasi
    menurut SOP, dan audit trail. Billing tetap diproses sampai final melalui Billing Management
    dengan `EncounterId` yang sama.
52. Late charge, koreksi billing, claim adjustment, biaya tambahan valid, atau proses finansial
    setelah clinical completion memakai post-close financial adjustment pada `EncounterId` yang
    sama dan tidak otomatis reopen clinical encounter.
53. Physical release terpisah dari clinical completion. Misalnya `EncounterStatus = Completed`,
    `BillingStatus = Pending`, `AdministrativeReleaseStatus = Waiting` diperbolehkan. Jika SOP
    mengizinkan pasien pulang saat outstanding, status `Completed` + `Outstanding` + `Released`
    juga diperbolehkan dengan reason, owner, next action, dan audit trail. Financial clearance
    untuk physical release mengikuti SOP dan payer/payment class yang configurable, tidak boleh
    di-hardcode atau mempertahankan `EncounterStatus = InService`.
54. Financial clearance wajib dievaluasi sebelum `AdministrativeReleaseStatus = Released`, tetapi
    tidak berarti `BillingStatus = Final`, seluruh invoice telah dibayar, atau seluruh receivable
    rumah sakit telah diterima. Prinsipnya: clinical completion ≠ billing completion ≠ financial
    clearance ≠ administrative release; `Released` hanya setelah financial clearance `Cleared`.
55. Untuk self-pay/pasien umum, patient responsibility wajib diselesaikan melalui payment method
    valid sebelum released sebagai default. `Cash` bukan uang tunai literal dan dapat memakai
    cash, debit, credit card, transfer, QRIS, atau metode aktif lain. `Outstanding + Released`
    hanya melalui financial exception berwenang (misalnya deferred payment, assistance, waiver,
    atau exception SOP), tanpa generic bypass. Exception mencatat encounter, responsibility dan
    outstanding amount, jenis/reason, requester/approver/waktu approval, owner, due date/next
    action bila relevan, notes, dan audit trail.
56. Untuk insurance, actual payment dari insurer tidak wajib diterima sebelum released. Clearance
    diberikan bila eligibility/policy valid, authorization/referral dan guarantee/coverage
    confirmation yang diwajibkan tersedia, benefit/limit diketahui, serta copayment, deductible,
    excess, atau non-covered patient responsibility telah diselesaikan sesuai SOP. Eligibility
    invalid, policy tidak aktif, dokumen wajib tidak tersedia, coverage ditolak/tidak cukup dengan
    excess belum selesai, atau payer tidak jelas menghasilkan `Pending`/`Blocked` kecuali ada
    exception berwenang.
57. Corporate/company guarantee memakai prinsip insurance: eligibility karyawan, guarantor dan
    benefit plan valid, verifikasi karyawan/dokumen/coverage yang diwajibkan terpenuhi, serta
    patient responsibility diselesaikan. Guarantee tidak tersedia/ditolak/kedaluwarsa, limit
    terlampaui, atau excess pasien mengubah bagian itu menjadi patient responsibility yang harus
    selesai sebelum released kecuali ada exception berwenang.
58. Patient outstanding harus dipisahkan dari payer outstanding. Minimum konsepnya
    `PatientResponsibilityAmount`, `PayerResponsibilityAmount`, `PatientOutstandingAmount`, dan
    `PayerOutstandingAmount`. Jika patient outstanding nol dan coverage/guarantee payer disetujui,
    clearance dapat `Cleared`; bila patient outstanding lebih dari nol tanpa arrangement deferred/
    exception disetujui, clearance harus `Pending`/`Blocked`. Payer receivable dapat tetap
    outstanding setelah released dan bukan unresolved patient liability.
59. Setiap financial clearance menyimpan snapshot/evidence: encounter, kategori payer/payment,
    total responsibility, responsibility/outstanding pasien dan payer, reference coverage/
    guarantee, hasil clearance, waktu dan evaluator, reference exception bila ada, notes, serta
    audit trail. Nama enum final mengikuti convention existing setelah capability audit.
60. Untuk self-pay, `PatientOutstandingAmount > 0` tanpa approved financial exception berarti
    financial clearance tidak boleh `Cleared` dan administrative release tidak boleh `Released`.
    `Outstanding + Released` adalah controlled exception, bukan normal flow.
61. Capability model financial exception minimal mencakup kategori `DeferredPayment`,
    `FinancialAssistanceOrWaiver`, `PaymentInfrastructureFailure`,
    `HospitalBillingReconciliation`, dan `DepositSecuredRelease`; daftar final mengikuti SOP MMC.
    Tidak boleh ada generic bypass "Release Anyway" tanpa exception type, reason, outstanding,
    responsible party, requester, approval, next action, dan audit trail.
62. Pasien yang faktual meninggalkan rumah sakit tanpa clearance/exception tidak boleh ditandai
    `AdministrativeReleaseStatus = Released` atau dipalsukan menjadi financial clearance
    `Cleared`. Sistem mencatat exceptional departure event tersendiri sesuai SOP dengan audit.
63. Deposit dapat dipakai dalam evaluasi clearance hanya setelah dialokasikan terhadap patient
    responsibility dan diverifikasi cukup, dengan mempertimbangkan `AvailableDepositAmount`,
    `PatientResponsibilityAmount`, `PatientOutstandingAmount`, dan
    `PendingOrUnpostedPatientCharges`. Deposit semata tidak otomatis clearance dan
    `ClearedByDeposit` berbeda dari waiver/exception; sisa deposit memakai workflow refund/credit
    terpisah. Unresolved financial exposure membuat clearance `Pending` kecuali ada exception
    berwenang.
64. Approval exception memakai capability `FinancialRelease.Exception.Request`,
    `FinancialRelease.Exception.ApproveTier1`, `FinancialRelease.Exception.ApproveTier2`,
    `FinancialRelease.Exception.ApproveTier3`, dan `FinancialRelease.DepositClear`. Maker dan
    approver harus berbeda untuk exception yang menghasilkan patient outstanding lebih dari nol
    dengan status released; self-approval dilarang.
65. Approval matrix dikonfigurasi menurut exception type, minimum/maksimum amount, required tier,
    kebutuhan second approval, dan izin release; juga dapat mempertimbangkan kategori pasien,
    riwayat exception berulang, coverage deposit/pembayaran, financial risk, serta special-case
    flags. Nominal threshold, dual approval per tipe, dan nama jabatan approver tidak di-hardcode
    dan menunggu SOP/delegation-of-authority MMC.
66. Approved outstanding release mencatat encounter/patient, exception type, patient
    responsibility, paid/deposit applied/outstanding amount, reason, requester/approver, tier,
    waktu approval, next action, due date bila relevan, owner/queue, reference/evidence, serta
    audit trail. Approval tidak mengubah histori billing atau menghapus outstanding; pembayaran
    berikutnya memakai billing/payment workflow normal pada encounter yang sama tanpa reopen
    clinical encounter.
67. Diagnostic result yang final setelah clinical completion tetap terkait pada
    `DiagnosticOrderId`, `Result`/`ReportId`, dan `EncounterId` asal. Late result tidak otomatis
    mengubah `Completed → InService` atau reopen clinical encounter; sistem menggunakan workflow
    review/follow-up terpisah.
68. Setiap diagnostic order memiliki `ReviewOwner` eksplisit. Default IGD adalah ordering/
    requesting clinician; bila tidak tersedia, ownership dialihkan kepada designated IGD covering
    clinician dari roster/authorization configuration. Untuk internal transfer, review owner dapat
    berpindah ke receiving responsible clinician/team hanya setelah handover diterima eksplisit,
    dengan histori ordering clinician, encounter asal, transferred owner, waktu handover, dan
    penerima tetap tersimpan.
69. Hasil late-result pasien pulang direview clinician berwenang, yang memilih action seperti
    no further action, contact patient, outpatient follow-up, consult clinician lain, rekomendasi
    kembali ke emergency, atau action klinis berwenang lain. Staf administratif/perawat dapat
    membantu komunikasi sesuai capability/SOP tetapi tidak menafsirkan hasil klinis.
70. Workflow non-critical adalah `FinalResultAvailable → ReviewPending → Reviewed →
    FollowUpClosed`, dengan reviewer/waktu, interpretation/action, follow-up requirement, dan
    clinical note/reference tercatat. SLA review non-critical mengikuti SOP dan tidak di-hardcode.
71. Critical result menjalankan `CriticalResultVerified → CriticalNotificationPending →
    ClinicianAcknowledged → ClinicalActionDetermined → PatientOrReceivingFacilityContacted →
    FollowUpClosed`. Notification sent tidak cukup; acknowledgement harus dapat dibuktikan.
    Unit diagnostik memulai notifikasi ke review owner dan mengeskalasi ke covering clinician
    lalu authority klinis lebih tinggi bila belum di-acknowledge, menurut SOP.
72. Hasil critical/high-risk yang membutuhkan contact dan pasien tidak dapat dihubungi tidak boleh
    langsung ditutup; gunakan escalation workflow. Contact attempt membedakan `Attempted`,
    `Reached`, dan `Acknowledged`, serta mencatat waktu, metode, target, outcome, dan actor.
    Komunikasi memakai channel yang disetujui, minimum necessary information, serta tidak
    mengirim payload hasil penuh/PHI tak perlu melalui infrastructure notification.
73. Pasien yang kembali karena late result memperoleh `EncounterId` baru; encounter asal tetap
    completed dan encounter baru mereferensikan original encounter, diagnostic order/result, dan
    follow-up record. Dokumentasi episode lama menggunakan amendment/addendum/follow-up record,
    bukan overwrite discharge/disposition atau reopen.
74. Late-result follow-up mengaudit identifier order/result dan source system; waktu final/
    verified; critical rule; review owner/reviewer; action; contact, acknowledgement, escalation,
    instruction, status, dan closure. Mutation/integration/notification log hanya menyimpan
    identifier/reference serta metadata tanpa full result payload atau PHI tak perlu. Repeated
    delivery LIS/RIS memakai stable external/result identifier dan idempotency agar tidak membuat
    duplicate result, review task, atau critical-result case.
75. Transaksi lintas modul tidak menggunakan distributed ACID transaction. Setiap bounded context
    memiliki data/state sendiri dan memakai local atomic commit + transactional outbox +
    idempotent receiver/inbox + acknowledgement + retry + reconciliation. Database transaction
    lokal tidak boleh ditahan selama network call ke dependency.
76. Mutating command/event lintas modul memuat `MessageId`/`CommandId`, `IdempotencyKey`,
    `CorrelationId`, `CausationId` bila relevan, `EncounterId`, aggregate/resource identifier,
    operation type, waktu request/occurred, source/destination module, dan schema/version tanpa
    PHI tak perlu. Retry selalu menggunakan idempotency key sama.
77. Receiver menyimpan processed-command/message registry atau unique constraint dalam local
    transaction bersama business mutation. Key sama dengan logical request sama me-replay outcome
    sebelumnya tanpa side effect kedua; key sama dengan payload berbeda menghasilkan
    `IdempotencyConflict` dan audit.
78. Delivery memakai at-least-once dengan idempotent processing untuk effectively-once business
    side effects, bukan klaim true exactly-once. Outbox disimpan bersama domain change dalam local
    transaction dan dispatcher mengirim setelah commit; receiver inbox menandai processed dalam
    transaction yang konsisten bersama mutasi bisnis.
79. Automatic retry hanya untuk transient failure secara bounded dengan exponential backoff+jitter;
    retry harus mengikuti error classification dan konfigurasi per destination. Validation,
    unauthorized/forbidden, invalid state, malformed identifier, unsupported operation, dan
    business rule rejection tidak di-loop otomatis. HTTP 401 hanya dapat refresh credential bila
    integration contract mendukungnya.
80. Interactive retry hanya menggunakan latency budget operasi pengguna; setelah habis, return
    state deterministik `Accepted`/`Pending` atau `DependencyUnavailable` sesuai semantik. Outbox
    background memiliki retry window lebih panjang; setelah limit, message masuk
    `NeedsReconciliation`/dead-letter queue, tidak di-drop, dan reprocess berwenang diaudit.
81. Network call memiliki finite timeout yang configurable. Timeout bukan otomatis business
    failure: bila remote mungkin sudah memproses, status menjadi `OutcomeUnknown`/`Unconfirmed`
    dan direkonsiliasi memakai idempotency/command/correlation/external reference atau status API;
    tidak membuat command baru dengan key baru.
82. Business state dipisahkan dari integration delivery state (misalnya `Pending`, `Dispatching`,
    `Accepted`, `RetryScheduled`, `OutcomeUnknown`, `Rejected`, `NeedsReconciliation`,
    `Completed`). Partial failure tidak memakai destructive global rollback: encounter/tindakan
    klinis yang sah tetap valid dan downstream billing/pharmacy/lab menjadi pending atau perlu
    rekonsiliasi.
83. Domain compensation digunakan bila perlu (misalnya `CancelOrder` yang sah), bukan generic
    rollback lintas modul. Tindakan yang sudah dilakukan tidak dihapus; gunakan correction,
    amendment, atau reconciliation. Transfer berubah state hanya setelah acknowledgement
    authoritative dan transport timeout tidak berarti acceptance.
84. Out-of-order/duplicate event harus ditangani dengan aggregate ID dan version/sequence bila
    perlu agar event lama tidak memundurkan state. `CorrelationId` stabil diteruskan untuk tracing
    lintas module dan bukan patient identifier publik.
85. Reconciliation record menyimpan ID pesan/idempotensi/korelasi/encounter, source/destination,
    operation, integration status, attempt/waktu retry, safe error metadata, external reference,
    serta outcome/actor/waktu reconciliation tanpa PHI/payload klinis penuh. Technical transport
    ownership dipisahkan dari business/clinical authority untuk correction/compensation.
86. UI membedakan `Succeeded`, `Pending`, `Failed`/`Rejected`, dan `NeedsAttention`; tidak boleh
    menampilkan success hanya karena local API menerima request ketika remote acknowledgement
    penting. Manual retry memakai transaksi asal dan key yang sama atau linked retry sesuai
    contract, membutuhkan authorization/audit, serta tidak boleh menduplikasi klinis.
87. Resilience/circuit policy mencegah synchronous hammering ketika dependency gagal berulang.
    Jika aman, workflow lokal berlanjut dan integration pending; bila acknowledgement downstream
    wajib untuk keamanan, tampilkan blocked/pending dan lakukan escalation. Observability minimal
    memantau timeout, retry, retry success, duplicate prevention, pending/age, reconciliation,
    rejection, dan dependency availability dengan alert berdasar impact klinis/bisnis.
88. Timeout, attempts, backoff, jitter, retry classification, reconciliation/circuit threshold,
    dan perubahan konfigurasi bersifat per destination, controlled, dan auditable; tidak
    di-hardcode satu angka untuk seluruh module.
89. Authorization IGD adalah capability-based, context-aware, dan backend-enforced. Role
    organisasi hanya mapping configurable terhadap capability. Mutation diizinkan hanya bila
    `HasCapability`, context, state transition, credential/privilege, dan segregation of duties
    semuanya valid; frontend hanya menampilkan allowed action dan mengirim command.
90. Capability sensitif dipisahkan secara granular (view/create/update draft/finalize/amend/
    correct/approve/reject/cancel/reopen/merge/reverse/override/break-glass/export/print/
    disclosure), bukan generic CRUD. Hak view tidak otomatis memberikan export, print, atau
    disclosure; akses PHI mengikuti minimum necessary dan treatment/business relationship.
91. Registrasi/Master Patient mengelola encounter administratif dan identity workflow, tetapi
    tidak otomatis mendapat triage, clinical diagnosis/order, disposition, clinical amendment,
    atau financial exception approval. Merge simple-match tetap memerlukan strong verified match,
    satu candidate definitif, tanpa material conflict, serta policy authorization.
92. Perawat memiliki capability yang unit-, credential-, dan encounter-context-scoped untuk
    start service, triage/retriage, nursing/vital/observation/resuscitation record, authorized
    medication/procedure administration, transfer departure, dan clinical communication. Perawat
    tidak otomatis menetapkan diagnosis/disposition, membuat physician-only order, merge identity,
    approve financial exception, atau bertindak sebagai receiving unit di luar contextnya.
93. Dokter IGD memiliki capability yang clinical privilege/unit/state-aware untuk assessment,
    diagnosis, diagnostic/medication/procedure order, resuscitation action, disposition,
    transfer request, clinical amendment, dan late-result review/follow-up. Clinical role tidak
    otomatis memberi patient identity approval/reverse merge, financial approval, user/capability
    management, atau unrestricted technical administration.
94. Unit tujuan adalah authorization context, bukan role universal. `Transfer.Accept`, `Reject`,
    `MarkArrived`, clinical handover, atau late-result ownership hanya berlaku bila destination
    unit sesuai authorized unit user dan state valid. Authority transfer dipisahkan: clinician
    request/disposition, sender `MarkDeparted`, receiver accept/reject/arrived.
95. Billing/Finance mengelola financial view/posting/adjust/reconcile/payment/clearance,
    exception request, deposit/refund bila capability tersedia, dan collection. Finance tidak
    mengubah clinical truth, disposition, atau reopen encounter untuk billing. Financial exception
    menggunakan requester/approver tier berbeda dan approval self tidak diperbolehkan.
96. Supervisor bersifat domain-scoped, bukan allow-everything. Registration supervisor dapat
    memiliki identity approval/reverse; clinical supervisor dapat higher-risk correction/reopen/
    cancel/disposition correction; finance supervisor dapat financial exception/adjustment/refund/
    write-off approval—semua hanya bila policy memberi capability eksplisit.
97. Technical administrator dapat mengelola user, mapping role-capability, configuration,
    integration/reprocess, system health/audit, dan reference data sesuai delegasi, namun bukan
    business superuser. Tidak ada direct database/state mutation sebagai normal workflow; repair
    darurat memakai technical incident procedure terpisah. Admin tidak otomatis memiliki clinical,
    identity merge, financial approval, reopen, atau blanket PHI access.
98. Break-glass adalah capability terpisah dan time-bounded dengan reason, target patient/
    encounter, emergency scope, actor/waktu server, immutable audit, dan post-event review. Tidak
    dipakai untuk kenyamanan finansial, bypass merge/billing, atau workflow rutin.
99. Segregation of duties wajib untuk merge ambigu, financial release exception, sender/receiver
    transfer, clinical versus financial truth, technical administration versus business approval,
    reverse merge, dan high-impact correction/reopen/cancel sesuai risk configuration. High-risk
    audit memuat actor, capability efektif, assignment context, action/resource, prior/new state,
    result authorization, reason/approver, server time, dan correlation/reference tanpa PHI tak
    perlu.
100. Temporary cover/on-call memakai delegated contextual assignment (unit, scope, effective
     start/end, delegator/reference, audit), bukan broad permanent permission. Backend mengecek
     unit/encounter/state/authorized units/credential/privilege/assignment, payer/exception
     context, serta requester-approver `UserId`; hidden UI button bukan security.
101. Respons klinis IGD memakai clock server-authoritative yang terpisah: `ArrivalAt →
     TriageStartedAt` (door-to-triage), `ArrivalAt → FirstQualifiedClinicalResponseAt`
     (door-to-qualified-clinical-response), dan `TriageCategoryEffectiveAt →
     FirstClinicalInterventionAt` (triage-to-intervention). Ketiganya tidak saling menggantikan.
102. KPI patient-safety utama adalah `ArrivalAt → FirstQualifiedClinicalResponseAt`,
     dikelompokkan menurut initial triage category. `ArrivalAt` adalah waktu pasien aktual
     diterima di IGD, bukan registration-completed; pre-arrival ambulance notification disimpan
     terpisah dan bukan SLA start kecuali SOP mengatur. Triage start berarti clinical personnel
     berwenang benar memulai triase, bukan membuka halaman/encounter.
103. `TriageCategoryEffectiveAt` direkam per assessment append-only. Qualifying clinical response
     berasal dari aktivitas klinis nyata menurut policy (misalnya assessment/primary survey/
     resuscitation), bukan membaca chart/notifikasi; intervention adalah tindakan nyata, bukan
     sekadar draft order kecuali policy eksplisit menganggapnya qualifying.
104. Merah memiliki target `Immediate`/zero-minute response dan tidak menunggu registrasi,
     payer/admin/form/queue/billing. Audit performance tetap menyimpan elapsed duration presisi.
     SLA Merah dievaluasi dari `ArrivalAt`; clinical care/intervention boleh berjalan paralel atau
     mendahului triage form. Kuning/Hijau memakai target duration configurable dan versioned,
     tanpa angka hard-code sampai SOP MMC tersedia.
105. Hitam bukan ordinary waiting-queue SLA. Klasifikasi tetap clinical-authorized dengan reason,
     waktu server, clinical basis, dan audit; target confirmation/family/mortuary/medicolegal,
     bila ada, menjadi workflow SLA terpisah. Retriage append-only membuka active SLA window baru
     dari retriage effective time tanpa mengubah arrival atau menghapus historical wait/breach.
106. Upgrade triage langsung menerapkan target kategori lebih tinggi; downgrade tidak menghapus
     breach historis. Ketika target terlewati, status `Breached` tetap tercatat walau kemudian
     mendapat response, dengan breach/responded/resolved time dan escalation status.
107. Escalation lifecycle adalah `OnTrack → Warning → Breached → Escalated → Acknowledged →
     ClinicalResponseRecorded → Resolved`. Merah langsung menimbulkan alert/escalation aktif;
     Kuning/Hijau memakai warning/deadline/tier/interval configurable. Acknowledgement bukan
     clinical response dan escalation tidak mengubah triage, diagnosis, disposition, timestamp,
     intervention, atau closure.
108. Escalation recipient ditentukan oleh capability, assignment/shift, unit, privilege, dan
     on-call/coverage context—not hard-coded job title. Queue/resource load tidak menghentikan
     atau mereset raw clock; reason operasional dicatat terpisah. Exclusion/adjustment bila sah
     menyimpan raw immutable duration, adjusted duration, reason, dan policy version.
109. Disaster mode memakai `OperationMode` eksplisit dan policy SLA bisa berbeda dari normal;
     monitoring tidak dimatikan diam-diam. Policy memakai configuration versioned/effective-dated
     menurut metric, category, operation mode, target/warning/escalation, berlaku dari/sampai,
     dan setiap evaluasi mereferensikan applied policy/version historis.
110. Backend menjadi authority timestamp, SLA calculation, breach/escalation, policy version,
     acknowledgement, dan resolution. Frontend hanya menampilkan timer/status/allowed action.
     Audit metric memuat encounter/visit/assessment, kategori dan seluruh clock, policy, warning/
     breach/escalation/acknowledgement/response/resolution, serta server time tanpa PHI tak perlu.
111. IGD mempunyai tepat satu accountable Product/Domain Owner untuk scope, workflow,
     prioritization, backlog, cross-domain coordination, operational acceptance/readiness, dan
     escalation governance. Owner bukan universal final approver; nama/jabatan/pemegang MMC tetap
     menunggu evidence internal.
112. Final approval dipisahkan sebagai governance capability konseptual Product/Operational,
     Clinical, Finance, Privacy, Security, dan Integration. Capability mengikuti convention
     authorization existing bila tersedia dan dipetakan secara configurable ke posisi organisasi,
     bukan hard-coded title/person.
113. Clinical approver berwenang final atas patient safety, triage, assessment, resuscitation,
     observation, diagnosis/procedure/medication workflow, disposition, clinical documentation/
     amendment, clinical SLA, dan escalation. Product owner maupun technical administrator tidak
     dapat meng-override clinical safety/policy.
114. Finance approver berwenang final atas billing/tariff, payer/guarantee, financial release,
     outstanding/reconciliation, correction/write-off/adjustment, dan financial closure gate.
     Privacy approver berwenang atas patient data use/disclosure/sharing/minimization/retention/
     secondary use dan privacy integration. Security approver berwenang atas privileged access,
     authentication/secrets, technical access exception, risk acceptance, and security control.
     Privacy dan security tetap distinct accountability walau MMC kelak dapat memetakan keduanya
     ke pihak sama.
115. Integration approval memisahkan business/data-semantic approval dari technical interface
     approval. Clinical, financial, atau identity integration memerlukan affected domain approval
     serta integration technical approval dan privacy/security bila data/risk terdampak; IT tidak
     boleh menjadi sole final approver atas arti klinis/finansial data.
116. Cross-domain decision membutuhkan seluruh mandatory impacted-domain approval, bukan majority
     vote. Tidak ada super approver berdasarkan hierarchy/CEO/director/admin semata; emergency/
     break-glass mengikuti policy tersendiri. Product owner dapat propose/prioritize/coordinate/
     accept operational delivery, tetapi tidak unilateral waive clinical, finance, privacy,
     security, atau cross-domain production integration control.
117. Governance approval mengikuti capability/context/backend validation dan maker-checker bila
     diwajibkan; technical admin tidak boleh self-assign atau bypass business approval. Decision
     record menyimpan domain, change/evidence/SOP/policy reference, proposer/reviewer/approver/
     rejection, effective dates/version/status dan audit tanpa PHI. Multi-domain lifecycle adalah
     `Draft → UnderReview → PartiallyApproved → Approved/Rejected → Effective → Superseded`, dan
     approved hanya bila semua domain wajib telah menyetujui; perubahan menghasilkan version/
     superseding decision, bukan overwrite.
118. High-impact encounter operation ditentukan oleh impact clinical, identity, financial,
     downstream, legal/regulatory, finalization, dan workflow—not operation name atau senioritas.
     Authorization tetap capability/context/impacted-domain/segregation-of-duties dan backend
     validated.
119. Correction high-impact bila mengubah clinical meaning, identity linkage, record signed/
     finalized/completed, authoritative chronology/SLA/KPI timestamp, downstream-consumed/
     external data, material financial/claim/guarantee data, completed history, executed workflow,
     legal/regulatory data, bulk records, atau record dengan correction/reversal/dispute history.
     Low-impact hanya clerical non-material sebelum final, tanpa clinical/identity/financial/
     downstream/SLA/legal/workflow impact, dan tetap auditable.
120. High-impact correction wajib maker-checker: maker `UserId` berbeda checker dan checker
     memiliki approval capability impacted domain. Cross-domain correction membutuhkan seluruh
     approval domain menurut `IGD-DEC-028`. Low-impact correction dapat one-actor bila SOP tidak
     menuntut checker. Technical detection dapat membuat alert/request tetapi tidak self-approve
     business correction.
121. Cancellation hanya untuk encounter erroneous/duplicate/mistakenly created tanpa material
     clinical activity. Triage/retriage, material clinical documentation, diagnosis, procedure,
     medication, lab/radiology, observation/resuscitation, disposition, executed transfer, atau
     consequential financial/claim/downstream transaction membuat cancellation ditolak; approval
     tidak boleh meng-override invariant. Gunakan amendment/correction/void/closure sesuai kasus,
     tanpa physical delete.
122. Eligible cancellation menghasilkan semantic cancelled state dengan reason, actor, waktu
     server, dependency/downstream check, immutable audit, dan idempotency. Empty deterministic
     error dapat one-actor bila policy mengizinkan; identity ambiguity, external/downstream,
     finalized state yang diizinkan SOP, cross-domain impact, history/dispute/escalation, atau SOP
     requirement menjadikannya high-risk eligible cancellation dan wajib maker-checker.
123. Setiap manual `Encounter.Reopen` selalu high-impact dan maker-checker wajib. Reopen hanya
     untuk scoped historical correction/omitted transaction approved, bukan membuka semua record
     lama; physical patient return selalu `EncounterId` baru. Request menyimpan encounter/target/
     change/reason/domain/evidence/requester/waktu/current state/expected scope.
124. Approved reopen dibatasi target eksplisit, lalu correction, validation, downstream
     reconciliation, dan re-close. Impact downstream memakai state reconciliation seperti pending,
     in progress, reconciled, failed, atau manual review; local audit tidak boleh fake success
     bila external system belum mengakui correction.
125. Capability konseptualnya `Encounter.CorrectionRequest`/`Correct`/`CorrectionApprove`,
     `Encounter.Cancel`/`CancelApprove`, dan `Encounter.ReopenRequest`/`Reopen`/`ReopenApprove`;
     gunakan convention authorization existing bila compatible. Backend memvalidasi maker/checker
     identity/capability/context/domain/approval/state/target; UI hanya menampilkan action.
126. High-impact audit memuat operation/encounter/type/domain/reason, maker/checker/capability/
     decision/timestamps, evidence, previous/effective state, target resource, execution,
     reconciliation, dan version. Correction mengikuti original + amendment + current effective
     representation; request/approve/reject/execute/cancel/reopen idempotent dan rejected request
     tetap historis—perubahan material membuat request/version baru.
127. Pada production awal, self-pay dengan `BillingStatus = Outstanding` tidak dapat memperoleh
     normal `AdministrativeReleaseStatus = Released` (fail-closed financial exception). Backend
     menolak release bila tidak ada approved policy/exception valid; eligibility, threshold,
     deposit, reason, evidence, approval, settlement SLA, dan escalation tidak boleh diarang atau
     di-hardcode.
128. Clinical completion tetap mengikuti `IGD-DEC-021`: encounter dapat `Completed` dengan billing
     outstanding dan `AdministrativeReleaseStatus = PendingFinancialClearance`, tanpa dianggap
     tetap `InService`. Financial rule fail-closed tidak menjadi clinical-care blocker; emergency
     care, resuscitation, required intervention, safe transfer, dan clinical disposition berjalan
     tanpa menunggu finance approval, dengan billing handoff/reconciliation.
129. Administrative release berbeda dari fakta physical departure. Sistem dapat mencatat
     `PhysicalDepartureAt` dan `PhysicalDepartureReason` saat pasien benar-benar pergi, meskipun
     administrative clearance masih pending; status tidak boleh dipalsukan menjadi pasien aktif
     IGD atau administratively released.
130. Bila kemudian diaktifkan, self-pay `Outstanding + Released` adalah high-impact controlled
     exception dengan policy versioned/effective-dated yang memuat payer, allowed release,
     maximum outstanding amount/percentage, minimum deposit amount/percentage, reason code,
     required approval/evidence, settlement SLA/escalation, dan active period. Maker-checker
     wajib (`UserId` berbeda), Finance adalah mandatory approval domain, dan cross-domain approval
     mengikuti `IGD-DEC-028`.
131. Financial handoff mencatat encounter, billing/outstanding, pending reason, owner/queue,
     next action, due/SLA, billing reference, waktu/creator. Future exception audit mencatat
     outstanding/deposit, reason/policy, maker/checker, request/approval/release, evidence, dan
     previous/new state menggunakan server time tanpa PHI/payload billing penuh pada generic log.
132. Identity merge maker adalah Registration/Master Patient actor ber-capability reconcile/merge.
     Ambiguous merge maker-checker dan reverse merge selalu high-impact; tanpa mapped approver,
     ambiguous/reverse merge disabled dan case tetap `ReconciliationPending`. Clinician hanya
     flag/support identity issue dan cross-domain consequence menambah approval sesuai `IGD-DEC-028`.
133. Late result default owner adalah ordering/requesting clinician dengan IGD covering clinician
     fallback; ownership transfer internal hanya setelah acceptance eksplisit. Criticality memakai
     versioned clinical-governance catalog, not generic abnormal flag. Critical result dimulai dari
     `ResultVerifiedAt`, target baseline <30 menit, dan non-critical default review ≤24 jam;
     acknowledgement dan documented follow-up wajib sebelum close.
134. Semua integration production memiliki Reliability Profile. Baseline API internal synchronous
     timeout 10 detik dan maksimal 2 interactive attempts yang safe/idempotent; external/vendor
     timeout 30 detik. State-changing vendor call tanpa idempotency/status-query terbukti tidak
     boleh blind retry—gunakan `OutcomeUnknown` + reconciliation. Profile belum lengkap berarti
     integration tidak production-active.
135. Capability menggunakan functional bundle configurable, credential/privilege effective-dated,
     transfer authority terpisah, delegation scoped/audited, and deny-by-default sensitive output.
     Break-glass terpisah, scope/time-bounded, audited/reviewed, dan tidak untuk finance/identity/
     routine. Technical admin tidak mendapat blanket PHI/clinical/business approval.
136. SLA Merah immediate; Kuning/Hijau `TargetUnconfigured` sampai SOP authoritative tersedia dan
     status itu bukan compliant/breached. Qualifying response adalah earliest primary survey,
     initial clinical assessment, atau first emergency clinical intervention oleh actor berwenang;
     chart/view/acknowledge/draft order bukan qualifying. Disaster tanpa profile memakai Normal
     conservative fallback dengan marker `FallbackPolicyUsed`.
137. Sebelum production, governance assignment aktif wajib tersedia untuk Product Owner,
     Clinical, Finance, Privacy, Security, and Integration approver, memakai UserId/capability/
     validity/evidence record. Affected governance decision fail-closed bila approver wajib tidak
     terkonfigurasi; historical assignment tidak di-overwrite.
138. Financial impact non-nol pada finalized record dan bulk yang memengaruhi ≥2 encounter atau
     ≥2 patient default high-impact sampai threshold SOP tersedia. Approval expiry menjadi
     overdue/escalated, tidak auto-approved; checker mapping kosong membuat operation fail-closed.
139. `SelfPayOutstandingReleaseEnabled = false` sampai written policy, catalog exception, threshold,
     deposit, maker-checker Finance+Product approval, evidence, receivable ownership, audit, UAT,
     dan effective-dated feature flag tersedia. Outstanding tetap receivable dan physical departure
     terpisah; financial exception tidak menghambat care/resuscitation/required transfer/disposition.

## State dan Transition

State utama yang tersirat pada dokumen sumber:

`Provisional → Registered → In Service → Disposition → Completed`

Untuk encounter provisional, state administratif dan pelayanan klinis berjalan terpisah:

`EncounterStatus = Provisional` + `AdministrativeStatus = Incomplete` +
`ClinicalServiceStatus = InProgress`

Pelengkapan registrasi memperbarui `AdministrativeStatus` menjadi `Complete` pada
`EncounterId` yang sama setelah field SOP/konfigurasi terpenuhi; hal ini tidak otomatis
menghentikan pelayanan klinis.

Status klinis, billing, dan physical release terpisah. Contoh state valid:

`EncounterStatus = Completed` + `BillingStatus = Pending` +
`AdministrativeReleaseStatus = Waiting`

atau, bila SOP/payer mengizinkan:

`EncounterStatus = Completed` + `BillingStatus = Outstanding` +
`AdministrativeReleaseStatus = Released`

Nilai enum final mengikuti convention source code setelah capability audit.

Physical release selalu membutuhkan hasil financial clearance `Cleared`. Contoh insurance valid:

`PatientOutstandingAmount = 0` + `PayerOutstandingAmount > 0` +
`FinancialClearanceStatus = ClearedByCoverage` +
`AdministrativeReleaseStatus = Released`

Approved self-pay exception juga dapat menghasilkan:

`EncounterStatus = Completed` + `BillingStatus = Outstanding` +
`PatientOutstandingAmount > 0` + `FinancialClearanceStatus = ClearedByException` +
`AdministrativeReleaseStatus = Released`

Lifecycle temporary patient:

`Temporary → IdentityFound → ReconciliationPending → Merged / Resolved`

`ReconciliationPending` dipertahankan untuk kandidat ganda, identifier bertentangan, atau
keraguan identitas hingga role berwenang mengonfirmasi. Reconciliation tidak mengubah
`EncounterId` maupun menghentikan pelayanan klinis.

Untuk kasus ambigu/konflik, transisi dari `ReconciliationPending` ke `Merged`/`Resolved`
memerlukan maker-checker dengan `UserId` berbeda. Penolakan mengembalikan/menahan kasus pada
`ReconciliationPending` tanpa menghapus data atau mengubah `EncounterId`.

Transfer rawat inap yang dinyatakan:

`None → Requested → Accepted → Departed → Arrived`

atau `Requested → Rejected`; penolakan tidak otomatis mengubah disposition dokter.

State aktivasi mode korban massal/bencana yang sudah diputuskan secara draft:

- Kepala IGD: `Inactive → Active — Pending Confirmation`.
- Incident commander resmi: `Inactive → Active — Confirmed`.
- `Active — Pending Confirmation → Active — Confirmed` melalui konfirmasi direktur,
  incident commander, atau pejabat/komite bencana yang ditunjuk.
- `Active — Pending Confirmation → Active — Pending Confirmation` dengan penanda
  `Confirmation Rejected` melalui penolakan konfirmasi; state ini tidak menghentikan
  pelayanan yang sedang berjalan.
- `Active → Deactivated` hanya oleh incident commander atau direktur.

Hal yang belum ditentukan:

- legal transition, guard, actor, dan timestamp untuk setiap perpindahan state;
- perilaku penolakan transfer, pembatalan, koreksi, reopening, dan duplicate command;
- apakah `Registered` selalu mengikuti `Provisional` atau keduanya alternatif initial state;
- sinkronisasi status emergency visit, encounter, billing, admission, dan transfer;
- behavior ketika dependency lintas modul unavailable atau hanya sebagian berhasil.

### Capability-Based Lifecycle Authority

Nama jabatan organisasi MMC tidak menjadi business rule atau source-code constant. Seluruh
mapping `Organization Role → Capability` configurable melalui authorization system dan menunggu
evidence SOP/struktur/keputusan owner.

| Command/transition | Capability | Authority dan guard |
|---|---|---|
| Membuat encounter normal/provisional | `Encounter.Create`, `Encounter.CreateProvisional` | Registrasi/admission sesuai kewenangan; clinician IGD berwenang dapat membuat provisional saat emergensi. `EncounterId` dibuat sistem; provisional mencatat actor, waktu server, dan alasan; emergency visit aktif kedua untuk encounter yang sama ditolak; approval kedua tidak diperlukan pada alur normal. |
| `Registered`/`Provisional → InService` | `Encounter.StartService` | Dokter atau perawat IGD yang berwenang pada encounter. Encounter provisional dapat langsung memulai layanan; administrasi belum lengkap bukan blocker; waktu mulai memakai waktu server. |
| `InService → Disposition` | `Disposition.Set` | Hanya dokter IGD dengan clinical authority. Catat actor, waktu, jenis disposition, serta clinical reason/reference; bukan keputusan administrasi, frontend, atau sistem otomatis. |
| Mengubah disposition | `Disposition.Change` | Dokter IGD berwenang. Sebelum eksekusi: reason wajib dan histori disposition dipertahankan. Setelah admission/transfer/discharge mulai dieksekusi: gunakan controlled correction/change workflow, periksa dependency, dan terapkan approval SOP untuk high-impact. |
| `Transfer.None → Requested` | `Transfer.Request` | Dokter IGD atau workflow admission berwenang, dengan disposition/order klinis yang mendasari. Disposition dan transfer tetap state terpisah. |
| `Requested → Accepted`/`Rejected` | `Transfer.Accept`, `Transfer.Reject` | Tim/unit tujuan berwenang. Catat actor, unit, dan waktu; reject wajib beralasan serta mengembalikan keputusan alternatif kepada workflow dokter/IGD. |
| `Accepted → Departed` | `Transfer.MarkDeparted` | Petugas/perawat unit pengirim setelah handover sesuai SOP; waktu keberangkatan memakai waktu server. |
| `Departed → Arrived` | `Transfer.MarkArrived` | Petugas/perawat unit penerima; konfirmasi tidak boleh dilakukan unit pengirim dan wajib mencatat actor, waktu, serta unit tujuan. |
| `Disposition → Completed` | `Encounter.Complete` | Actor berwenang dapat memicu evaluasi, tetapi backend melakukan final transition hanya setelah seluruh closure gate tervalidasi. Catat triggering actor bila manual, `completedAt`, disposition final, dan hasil evaluasi gate. |
| Cancel encounter | `Encounter.Cancel` | Hanya ketika encounter keliru dan belum memiliki aktivitas klinis material (triage/retriage, SOAP/CPPT, diagnosis, procedure, medication, order lab/radiologi, observasi/resusitasi, atau disposition). Wajib alasan, actor, waktu, audit; SOP dapat meminta approval supervisor untuk kasus berisiko. |
| Correction/amendment | `Encounter.Correct`, `ClinicalRecord.Amend`, `Disposition.Correct` | Model `Original Record → Amendment/Correction → Current Effective Value`; nilai asli tetap ter-audit. Reason, actor, dan waktu wajib. Koreksi klinis oleh clinical role dan administratif oleh administrative role. Dampak pada disposition/transfer/billing/admission memakai controlled workflow dan approval sesuai dampak. |
| Reopen completed encounter | `Encounter.Reopen` | Exception untuk koreksi/penyelesaian episode lama yang sah, bukan pasien kembali. Wajib reason, actor, waktu, referensi encounter, daftar gate/state yang dibuka, dan audit. Membutuhkan authority lebih tinggi dari complete biasa; high-impact menggunakan maker-checker tanpa self-approval. |

### Late Diagnostic Result Follow-Up

| Area | Rule |
|---|---|
| Review ownership | Setiap order memiliki `ReviewOwner`: default ordering/requesting clinician, dengan fallback covering clinician dari roster/authorization. Hasil tidak boleh hanya bergantung pada inbox satu dokter tanpa fallback. |
| Pasien pulang | Clinician berwenang menentukan `NoFurtherAction`, `ContactPatient`, `ArrangeOutpatientFollowUp`, `ConsultOtherClinician`, `RecommendReturnToEmergency`, atau action lain sesuai convention. |
| Transfer internal | Pending order wajib masuk handover. Ownership berganti ke receiving responsible clinician/team hanya setelah acceptance eksplisit; provenance ordering clinician/order/encounter tidak berubah. |
| Transfer eksternal | Hasil yang perlu follow-up dikomunikasikan aktif ke fasilitas/team penerima melalui channel disetujui. Critical result memerlukan acknowledgement atau escalation, termasuk facility/reference, recipient/role, metode, waktu contact, acknowledgement dan waktunya. |
| Non-critical | `FinalResultAvailable → ReviewPending → Reviewed → FollowUpClosed`; exact review SLA mengikuti SOP MMC. |
| Critical | `CriticalResultVerified → CriticalNotificationPending → ClinicianAcknowledged → ClinicalActionDetermined → PatientOrReceivingFacilityContacted → FollowUpClosed`. `CriticalResultVerifiedAt` menjadi titik awal monitoring SLA bila baseline regulatori yang diklaim telah terverifikasi; target internal boleh lebih ketat, tidak boleh lebih longgar dari requirement yang terbukti berlaku. |
| Escalation & read-back | Primary target adalah current review owner, lalu covering clinician, lalu higher clinical escalation authority mengikuti SOP. Untuk komunikasi verbal/telepon yang membutuhkan read-back, simpan notifier, waktu, penerima, status read-back/confirmation, dan acknowledgement time. |
| Patient contact | Gunakan channel approved dan minimum necessary information. Status contact: `ContactPending`, `Attempted`, `Reached`, `Acknowledged`, `UnableToReach`, `Escalated`, `Closed`; attempt bukan reached dan reached bukan acknowledgement. |
| Unreachable & return | Critical/high-risk `UnableToReach` tetap dieskalasi. Bila pasien kembali, buat encounter baru yang mereferensikan episode/order/result/follow-up asal; jangan reopen encounter asal. |
| Audit & idempotency | Audit memuat order/result/encounter/source, waktu final/verified, criticality/rule, ownership/review/action/contact/acknowledgement/escalation/instruction/status/closure. LIS/RIS repeated delivery harus idempotent berdasarkan stable external/result identifier. |

### Cross-Module Reliability Contract

| Area | Rule |
|---|---|
| Local transaction | Simpan domain change dan outbox message dalam satu local transaction, commit, lalu dispatcher mengirim. Jangan membuka transaction lalu menunggu HTTP Lab/Radiology/Billing/etc. |
| Idempotency | `CreateLabOrder`, `CreateRadiologyOrder`, `DispenseMedication`, `CreateCharge`, `RequestTransfer`, `AcceptTransfer`, `SendDiagnosticResult`, dan `PostBillingItem` memakai stable `IdempotencyKey`. Key tidak berubah karena UI resubmit, timeout, dispatcher retry, atau restart. |
| Retry classification | Transient: network error/reset, safe timeout, 408, 429 dengan `Retry-After`, 502/503/504, atau dependency temporary unavailable. Business/permanent: umumnya 400/401/403/409/422, invalid validation/state/payload, dan rule rejection. HTTP 5xx lain retryable hanya bila contract menyatakannya. |
| Unknown outcome | Timeout setelah pengiriman yang mungkin diterima tidak boleh langsung failed atau membuat request baru; gunakan `OutcomeUnknown` lalu status query/reconciliation memakai correlation/idempotency/external reference. |
| Examples | Clinical order valid + Lab pending: order tetap valid. Prescription created + Pharmacy belum accept: tampilkan dispensing belum confirmed. Procedure/medication sah + billing gagal: clinical record tetap valid, billing `NeedsReconciliation`. |
| Compensation | Batal yang sah menghasilkan domain command, misalnya `CancelOrder`; tidak menghapus tindakan atau encounter yang telah terjadi. |
| Reconciliation | Technical team menangani transport/queue; clinical/business owner menangani kebenaran transaksi dan patient-safety impact. Manual retry/reprocess berwenang, memakai transaksi asal/key terkait, dan diaudit. |
| Config & open inventory | Per destination: timeout, max attempts, base/max delay, jitter, retry classification, reconciliation threshold/window, circuit threshold, dan retention idempotency record. Nilai Lab, Radiology, Pharmacy, Billing, Inpatient, external API serta status-query/native-idempotency vendor menunggu integration inventory/evidence. |

### Authorization Model

`Allowed = HasCapability ∧ ContextValid ∧ StateTransitionValid ∧ CredentialOrPrivilegeValid ∧ SegregationOfDutiesPass`

| Domain/context | Capability and guardrail |
|---|---|
| Registration/Master Patient | `Encounter.Create`, `Encounter.CreateProvisional`, `Encounter.CompleteRegistration`, `PatientDemographics.UpdateAdministrative`, `PatientIdentity.Reconcile`, `PatientIdentity.FlagPotentialDuplicate`, dan merge simple-match sesuai policy. Tidak otomatis memiliki clinical/triage/disposition/financial-approval capability. |
| Identity ambiguous merge | Maker `PatientIdentity.Reconcile`/`Merge`; checker `PatientIdentity.ApproveAmbiguousMerge`, dengan `UserId` berbeda. `PatientIdentity.ReverseMerge` adalah higher privilege terpisah. |
| Nursing | `Encounter.StartService`, `Triage.Create`/`Retriage`, nursing/vital/observation/resuscitation record, authorized medication/procedure administration bila order/protocol dan credential valid, `Transfer.MarkDeparted`, serta clinical communication berdasarkan instruction/SOP. |
| Clinical doctor | Assessment, diagnosis, diagnostic/medication/procedure order, clinical resuscitation, `Disposition.Set`/`Change`, `Transfer.Request`, clinical amendment, late-result review/follow-up, semuanya tunduk pada credential, privilege, unit, encounter, dan state. |
| Receiving unit | `Transfer.Accept`, `Transfer.Reject`, `Transfer.MarkArrived`, `ClinicalHandover.Accept`, `LateResult.AcceptReviewOwnership` hanya saat `DestinationUnitId` sesuai unit user dan workflow state benar. Exact receiving actor per transfer type tetap configurable. |
| Billing/Finance | Billing/payment/clearance/reconciliation, exception request, deposit/refund hanya bila capability terimplementasi. Tidak mengubah clinical state atau reopen untuk memperbaiki billing. |
| Supervisory authority | Domain-scoped approval/exception saja: identity, clinical, dan finance supervisor memiliki capability eksplisit berbeda. Tidak ada `Supervisor => Allow Everything`. |
| Technical administrator | User/config/integration/reprocess/health/audit/reference configuration sesuai delegasi, tetapi tidak otomatis clinical/business authority atau unrestricted PHI. |
| Break-glass | `EmergencyAccess.BreakGlass` terpisah, beralasan dan time-bounded, dengan post-event review; bukan side effect administrator/supervisor. |
| Context checks | Evaluasi meliputi service unit, encounter/state, authorized units, clinical credential/privilege, current/on-call assignment, transfer source/destination, payer/exception amount/type, serta requester/approver identity. |

### Governance and Approval Model

| Governance domain | Accountable approval boundary |
|---|---|
| Product/Operational | Satu Product/Domain Owner accountable untuk end-to-end operational scope, prioritization, coordination, dan delivery acceptance; bukan approver universal. |
| Clinical | Patient safety, clinical workflow/documentation, triage/SLA/escalation, disposition, dan clinical correction. |
| Finance | Billing/tariff, payer/guarantee, financial clearance/release, reconciliation/correction/write-off, dan financial closure. |
| Privacy | Patient-data use/disclosure/sharing/minimization/retention/secondary use dan privacy aspect of integration. |
| Security | Privileged access, authentication/secrets, security control/exception, risk acceptance, dan production security requirement. |
| Integration | Technical API/transport/connectivity/protocol/credentials/reliability/observability; data semantics tetap pada affected clinical/finance/identity domain. |
| Cross-domain | Semua mandatory impacted domains wajib approve. Decision hanya `Approved` setelah seluruh approval wajib terpenuhi. |
| Organization mapping | Governance capability dipetakan ke posisi/user MMC secara configurable. Historical approval menyimpan user sebenarnya saat keputusan dibuat tanpa mengikat workflow code pada person tertentu. |

### High-Impact Encounter Governance

| Operation | Rule |
|---|---|
| High-impact correction | Any clinical/identity/finalized/timestamp/downstream/financial/legal/bulk/history impact requires maker-checker. Cross-domain change requires all mandatory domain approvals. |
| Low-impact correction | Only pre-final clerical non-material correction without identity/clinical/finance/downstream/SLA/legal/workflow impact. Single authorized actor allowed only if SOP permits; original remains auditable. |
| Cancellation | Only erroneous/duplicate/mistaken encounter without material clinical activity or consequential transaction. Material activity means cancellation is denied—not approvable. Successful cancel is semantic state, never physical delete. |
| Reopen | Every manual reopen is high-impact and maker-checker mandatory. Scope must identify target transaction/change; once handled, reconcile downstream and re-close. Return visit always creates a new encounter. |
| Decision controls | Maker and checker have different `UserId`; checker has impacted-domain approval capability. Backend validates capability/context/domain/current state/target state and idempotency. |
| Open configuration | MMC organization mapping, material finance/bulk threshold, empty-cancel checker policy, exact activity definition, rejection/escalation path, approval SLA, and existing authorization mapping. |

### Mandatory Production Configuration and Activation Gates

These are not unresolved architecture questions. Missing mandatory configuration produces a
fail-closed outcome for the affected privileged, financial, or integration action; emergency
clinical care remains patient-safety-first.

#### `IGD-DEC-031` — Identity Merge Authority

- Maker: Registration/Master Patient with `PatientIdentity.Reconcile` and `PatientIdentity.Merge`.
- Ambiguous merge: `MakerUserId != CheckerUserId`, checker requires
  `PatientIdentity.ApproveAmbiguousMerge`; missing mapping disables ambiguous merge and leaves the
  case `ReconciliationPending`.
- Reverse merge is always high-impact using separate request/approve capability and is disabled
  without mapped approver. Impacted Clinical/Finance/Claim/Medication/Lab/Radiology/downstream
  domains add mandatory review/approval.

#### `IGD-DEC-032` — Late Diagnostic Result Governance

- Owner defaults to ordering/requesting clinician, with designated IGD covering clinician fallback.
  Internal handover changes ownership only after explicit acceptance; external contact needs
  facility, recipient/role, contact time, and acknowledgement.
- Criticality comes from a versioned Clinical Governance catalog. Critical baseline starts at
  `ResultVerifiedAt`, targets under 30 minutes, and escalates: immediate notification, Tier 1 at
  T+5, Tier 2 at T+10, highest configured clinical escalation at T+20, and breach at T+30.
- Non-critical review defaults to within 24 hours. Critical `UnableToReach` never auto-closes.
  Approved channel/minimum-necessary contact, acknowledgement, documented follow-up, new encounter
  on return, and no automatic reopen remain mandatory.

#### `IGD-DEC-033` — Integration Reliability Profile

- Every integration requires a production profile: internal synchronous timeout 10 seconds with at
  most 2 safe/idempotent interactive attempts; external/vendor timeout 30 seconds.
- State-changing vendor calls may retry only after verified idempotency/deduplication. Otherwise a
  timeout is `OutcomeUnknown` and must reconcile, never blind retry.
- Vendor matrix declares native idempotency, status query, correlation echo, async acknowledgement,
  retry-after, deduplication, cancellation, reversal, and webhook callback. Unknown means
  unsupported/untrusted. Missing profile/ownership means activation denied.

#### `IGD-DEC-034` — Capability Assignment and Privileged Access

- Use configurable functional bundles—not MMC job-title constants—for registration/master-patient,
  IGD nurse/physician/supervisor, receiving unit, finance, privacy/security/integration, and
  technical administration.
- Transfer: physician sets disposition/requests; sender marks departed; receiving clinical actor
  accepts/rejects; receiving nurse/staff marks arrived. Bed/admission coordination is not clinical
  acceptance.
- Credential/privilege uses authoritative registry or effective-dated assignment. Delegation is
  capability/unit/time scoped and audited; sensitive PHI output is deny-by-default and split among
  view/print/export/disclose.

#### `IGD-DEC-035` — Triage SLA Operational Policy

- Retains `IGD-DEC-027`: Red is immediate; Yellow/Green remain `TargetUnconfigured` rather than
  compliant/breached until SOP config exists. Normal and Disaster profiles are separate/versioned;
  disaster without a profile uses marked Normal fallback.
- A qualifying response is the earliest valid `PrimarySurveyStarted`,
  `InitialClinicalAssessmentStarted`, or `FirstEmergencyClinicalIntervention`; viewing or drafting
  does not qualify. Triage colour conflict remains recorded and does not alter `IGD-DEC-007`.

#### `IGD-DEC-036` — Governance Assignment

- Mandatory go-live `GovernanceAssignment` exists for Product Owner and Clinical/Finance/Privacy/
  Security/Integration approver, with governance domain, stable UserId, capability,
  primary/delegate, validity, assigner, and evidence reference.
- Missing required approver fails the affected decision closed. Assignment/person details are go-live
  evidence/configuration, not remaining architecture questions.

#### `IGD-DEC-037` — High-Impact Parameters

- Always high-impact: clinical/identity/disposition/safety-critical correction, material clinical
  timestamp, finalized/completed record, downstream/external data, and every manual reopen.
- Until threshold exists, finalized financial impact ≠ 0 is high-impact; bulk impact ≥2 encounter
  or ≥2 patient is high-impact. Material clinical activity always denies cancellation.
- Approval expiry is `Overdue → Escalated`, never auto-approval. Missing checker mapping denies
  execution.

#### `IGD-DEC-038` — Future Self-Pay Outstanding + Released Activation

- Default flag is false. Activation needs policy/version/owner, approved reason catalog, threshold,
  deposit formula/allocation/refund, requester≠approver, Finance + Product governance, evidence,
  receivable owner/amount/due/next action/collection/escalation, audit, full UAT, and an
  effective-dated feature flag that can be disabled without deployment.
- Receivable is never deleted to release a patient. Physical departure is separate from release;
  patient-safety clinical actions remain unrestricted by this financial gate.

## Skenario Normal dan Exception

Skenario awal dari dokumen sumber:

- pasien lama level 4 melalui registrasi standar;
- pasien tak dikenal level 1 melalui temporary patient dan provisional encounter;
- kondisi memburuk dari level 3 ke level 2 melalui retriage tanpa menghapus histori;
- observasi berkala mereferensikan vital sign tanpa duplikasi;
- rawat inap mensyaratkan admission/unit tujuan dan transfer sampai `Arrived`;
- pulang mensyaratkan instruksi, resep, billing, dan administrasi;
- PAPS mensyaratkan alasan, consent/saksi, kondisi keluar, instruksi keselamatan, dan audit;
- pembuatan emergency visit aktif kedua ditolak.

Skenario yang masih terbuka:

- pasien datang kembali setelah visit selesai;
- salah identitas dan merge temporary patient;
- koreksi triage/disposition tanpa merusak audit trail;
- pembatalan atau perubahan disposition;
- pasien meninggal sebelum identitas/administrasi lengkap;
- pasien kabur saat billing, resep, atau hasil penunjang belum selesai;
- unit rawat inap menolak atau membatalkan penerimaan setelah sebelumnya menerima;
- downtime/offline, retry, duplicate submit, timeout, serta partial failure antar modul;
- hasil lab/radiologi datang setelah pasien meninggalkan IGD;
- penutupan visit ketika billing belum final tetapi pasien secara klinis sudah boleh keluar.

## Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `IGD-UI-001` | Privacy dan masking data klinis/identitas | Security/privacy owner | `open` | Harus mengikuti klasifikasi data dan permission approved | Belum tersedia |
| `IGD-UI-002` | Siapa boleh melihat/mengubah state klinis dan administratif | Product/domain + security owner | `open` | Tidak boleh ditentukan oleh frontend | Belum tersedia |
| `IGD-UI-003` | Peringatan SLA triage dan eskalasi | Clinical governance + product owner | `open` | Severity dan tindakan harus berasal dari rule approved | Dokumen §5 belum menetapkan escalation policy |
| `IGD-UI-004` | Menu, route, bentuk page/tab/modal/drawer, layout, dan warna presentasi | Frontend authority | `DEV_DISCRETION` setelah constraint approved | Ikuti project convention dan accessibility; warna klinis tidak boleh mengubah arti level | Belum diaudit |
| `IGD-UI-005` | Kontrol aktivasi, konfirmasi, penolakan, dan penonaktifan mode bencana | Product/domain + security owner | `draft` | UI wajib mengikuti role dan legal transition; tidak boleh didelegasikan ke developer | Jawaban user 13 Agustus 2026 |

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `IGD-DEC-001` | Decision | Gunakan satu encounter OP asal/unit IGD untuk satu episode, dengan emergency visit sebagai extension | Product/domain owner + Registration API owner | `draft` | — | Dokumen §1–4 dan §9 |
| `IGD-DEC-002` | Decision | Pasien gawat boleh memakai provisional encounter tanpa menunggu administrasi lengkap | Product/domain + clinical governance owner | `draft` | — | Dokumen §4.2 |
| `IGD-DEC-003` | Decision | Data klinis umum tidak diduplikasi di modul IGD | Product/domain + data/API owners | `draft` | — | Dokumen §1, §2, dan §6 |
| `IGD-DEC-004` | Decision | Retriage append-only dan mereferensikan histori sebelumnya | Clinical governance owner | `draft` | — | Dokumen §5.2 |
| `IGD-DEC-005` | Decision | Disposition terpisah dari eksekusi transfer | Product/domain + clinical governance owner | `draft` | — | Dokumen §7.3–8 |
| `IGD-DEC-006` | Decision | Mutation logging tidak menyimpan PHI/payload klinis lengkap | Security/privacy owner | `draft` | — | Dokumen §9.3 |
| `IGD-OQ-001` | Conflict | Sistem triage disebut “ATS atau ESI”, tetapi level, warna, target waktu, dan fast-track digabung seolah satu skema | Clinical governance owner | `superseded` oleh `IGD-DEC-007` | — | Dokumen §5; jawaban user 13 Agustus 2026 |
| `IGD-OQ-002` | Open Question | Siapa product/domain owner dan siapa approver final keputusan kritis? | Sponsor modul | `superseded` oleh `IGD-DEC-028` untuk governance model | — | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-003` | Open Question | Apa field minimum provisional encounter dan kapan administrasi dianggap lengkap/stabil? | Registration + product/domain owner | `superseded` oleh `IGD-DEC-016` | — | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-004` | Open Question | Bagaimana lifecycle, merge, deduplication, dan audit temporary patient? | Master Patient owner + privacy owner | `superseded` oleh `IGD-DEC-017` | — | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-005` | Open Question | Apa legal transition dan permission untuk visit, disposition, transfer, correction, cancellation, dan reopening? | Product/domain + clinical governance owner | `superseded` oleh `IGD-DEC-020` | — | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-006` | Open Question | Apakah billing harus benar-benar final untuk close, atau boleh outstanding/ditagihkan dengan reason dan owner? | Finance/billing + product owner | `superseded` oleh `IGD-DEC-021` | — | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-007` | Open Question | Bagaimana aturan hasil penunjang terlambat dan clinical follow-up setelah pasien keluar? | Clinical governance owner | `superseded` oleh `IGD-DEC-024` untuk prinsip workflow | — | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-008` | Open Question | Apa failure/retry/idempotency policy untuk transaksi lintas Registration, Clinical, Pharmacy, Lab/Radiology, Billing, dan Inpatient? | API/data owners | `superseded` oleh `IGD-DEC-025` untuk architectural policy | — | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-009` | Open Question | Permission matrix dan separation of duties per aktor belum ditentukan | Product/domain + security owner | `superseded` oleh `IGD-DEC-026` untuk authorization architecture | — | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-010` | Open Question | Apakah target response adalah SLA ke triage, kontak klinis pertama, atau intervensi; dari timestamp mana dihitung dan apa eskalasinya? | Clinical governance owner | `superseded` oleh `IGD-DEC-027` untuk response measurement architecture | — | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-007` | Decision | Gunakan sistem triase warna Indonesia: Merah, Kuning, Hijau, dan Hitam; jangan memakai label ATS/ESI sebagai source of truth | Clinical governance owner | `draft` | — | Jawaban user 13 Agustus 2026; Permenkes 47/2018 |
| `IGD-OQ-011` | Open Question | Apakah satu skema empat warna berlaku sama untuk IGD harian dan bencana/mass-casualty, khususnya penggunaan kategori Hitam? | Clinical governance + product/domain owner | `superseded` oleh `IGD-FACT-001` dan `IGD-OQ-012` | — | Permenkes 47/2018 dan Kepmenkes 1502/2023 memberi jawaban normatif yang lebih tepat |
| `IGD-FACT-001` | Fact | Empat kategori warna berlaku pada pelayanan IGD; bencana/korban massal menambahkan tag triase, manajemen korban massal, dan organisasi krisis. Status harus dinilai ulang dan perubahan dicatat sebagai retriase | — | `approved` | Kemenkes evidence verified/13 Agustus 2026 | Permenkes 47/2018; Permenkes 1/2026; Kepmenkes 1502/2023 |
| `IGD-OQ-012` | Open Question | Apakah disaster/mass-casualty mode termasuk scope rilis awal modul IGD atau menjadi fase terpisah setelah alur IGD harian? | Product/domain owner | `superseded` oleh `IGD-DEC-009` | — | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-008` | Decision | Pada korban massal/bencana, korban tetap diklasifikasikan dengan empat warna berdasarkan tingkat kegawatdaruratan; perubahan kategori dilakukan melalui retriase dan histori tidak ditimpa | Clinical governance + product/domain owner | `draft` | — | Jawaban user 13 Agustus 2026; Permenkes 47/2018; Kepmenkes 1502/2023 |
| `IGD-DEC-009` | Decision | Mode korban massal/bencana termasuk scope rilis awal modul IGD | Product/domain owner | `draft` | — | Jawaban user “ya”, 13 Agustus 2026; owner/approver belum diidentifikasi |
| `IGD-OQ-013` | Open Question | Siapa yang boleh mengaktifkan dan menonaktifkan mode korban massal/bencana di rumah sakit, termasuk aktivasi sementara sebelum status eksternal diterbitkan? | Product/domain + clinical governance + security owner | `superseded` oleh `IGD-DEC-010` sampai `IGD-DEC-013` | — | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-010` | Decision | Kepala IGD mengaktifkan sebagai `Active — Pending Confirmation`; incident commander resmi mengaktifkan sebagai `Active — Confirmed` | Product/domain + clinical governance owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user butir 1–3 |
| `IGD-DEC-011` | Decision | Direktur, incident commander, atau pejabat/komite bencana yang ditunjuk dapat mengonfirmasi; aktivasi terkonfirmasi oleh incident commander tetap menotifikasi direktur dan komite | Product/domain + security owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user butir 2–3 |
| `IGD-DEC-012` | Decision | Hanya incident commander atau direktur yang dapat menonaktifkan; Kepala IGD hanya bila resmi merangkap incident commander | Product/domain + security owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user butir 4 |
| `IGD-DEC-013` | Decision | Semua perubahan state memakai waktu server dan audit lengkap; bukti boleh menyusul, disimpan terbatas, dan audit hanya menyimpan referensi tanpa PHI/payload klinis | Security/privacy + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user butir 5–7 |
| `IGD-OQ-014` | Open Question | Apa dampak penolakan konfirmasi terhadap mode dan pelayanan korban yang sudah berjalan? | Clinical governance + incident command + product/domain owner | `superseded` oleh `IGD-DEC-014` | — | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-014` | Decision | Penolakan konfirmasi tidak menonaktifkan mode atau menghentikan pelayanan yang sedang berjalan. Mode tetap aktif dengan penanda `Confirmation Rejected` sampai incident commander atau direktur melakukan penonaktifan eksplisit; keduanya dicatat sebagai event audit terpisah | Clinical governance + incident command + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-015` | Open Question | Berapa batas waktu konfirmasi dan eskalasinya? | MMC Hospital Disaster Plan/SOP owner | `superseded` oleh `IGD-DEC-015` | — | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-015` | Decision | Batas waktu konfirmasi dan jalur eskalasi mengikuti SOP internal MMC Hospital atau Hospital Disaster Plan yang dapat dikonfigurasi; angka maupun jalur eskalasi tidak di-hardcode | MMC Hospital Disaster Plan/SOP owner + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-016` | Decision | Provisional encounter dibuat dengan field minimum yang ditetapkan untuk memungkinkan pelayanan IGD segera. Pelengkapan registrasi dilakukan pada `EncounterId` yang sama oleh registrasi/admission saat kondisi pasien memungkinkan menurut tenaga klinis; status administratif dan klinis dipisahkan, seluruh field final/reminder/eskalasi mengikuti SOP yang dapat dikonfigurasi tanpa hard-code, dan keterlambatan administrasi tidak memblokir pelayanan klinis | Registration/admission IGD + clinical governance + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-017` | Decision | Temporary patient memakai lifecycle terkontrol dari `Temporary` hingga `Merged`/`Resolved`; pencarian kandidat wajib sebelum membuat master patient baru, tidak ada automatic merge, reconciliation tidak mengubah `EncounterId` atau menghentikan pelayanan, dan seluruh mutasi dapat diaudit tanpa PHI/payload klinis lengkap | Registration/Master Patient + security/privacy + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-018` | Decision | Separation of duties wajib untuk patient merge/reconciliation yang ambigu atau memiliki konflik. Maker dan approver harus berbeda. Reconciliation sederhana dapat dilakukan oleh petugas berwenang tanpa approval kedua jika memenuhi kriteria strong-match dan tidak terdapat konflik | Registration/Master Patient + security/privacy + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-019` | Decision | Capability patient identity dikunci sebagai `PatientIdentity.Reconcile`, `PatientIdentity.Merge`, `PatientIdentity.ApproveAmbiguousMerge`, dan `PatientIdentity.ReverseMerge`; mapping nama role organisasi configurable berbasis evidence dan tidak mengubah workflow bisnis utama | Registration/Master Patient + security/privacy + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-016` | Open Question | Siapa nama role organisasi final MMC Hospital yang dipetakan sebagai maker dan approver patient merge/reconciliation? | Master Patient/SOP owner + security/privacy owner | `superseded` oleh `IGD-DEC-031` | — | Jawaban user 13 Agustus 2026; mapping role menjadi mandatory governance configuration |
| `IGD-DEC-020` | Decision | Lifecycle encounter IGD menggunakan capability-based transition authority. Registrasi/authorized clinician membuat encounter; dokter berwenang menetapkan dan mengubah disposition; unit tujuan menerima/menolak transfer; unit pengirim mencatat Departed dan unit penerima mencatat Arrived; completion divalidasi backend berdasarkan disposition dan closure gates. Cancellation hanya untuk encounter tanpa aktivitas klinis material, correction bersifat amendment/append-only, dan reopening merupakan exception dengan authorization lebih tinggi. Pasien yang kembali setelah encounter selesai harus memperoleh encounter baru, bukan reopening encounter lama | Product/domain + clinical governance + security/privacy owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-017` | Open Question | Apa level approval dan capability final untuk high-impact correction/reopen? | Clinical governance + product/domain + security owner | `superseded` oleh `IGD-DEC-029` untuk classification dan maker-checker governance | — | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-021` | Decision | Clinical completion, billing completion, dan administrative release merupakan lifecycle terpisah. Encounter IGD dapat Completed setelah disposition dan seluruh closure gate klinis/transfer yang relevan terpenuhi meskipun billing masih Pending atau Outstanding, selama terdapat billing handoff dengan reason, owner, next action, dan audit trail. Billing yang belum final tidak membuat pasien tetap dianggap aktif secara klinis di IGD. Aturan physical release dan financial clearance mengikuti SOP serta kategori penjamin/pembayaran MMC | Finance/billing + product/domain + clinical governance owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-018` | Open Question | Payer/payment class mana yang mensyaratkan financial clearance sebelum physical release, termasuk pasien umum/tunai, asuransi/corporate/guarantee, dan pengecualian `Outstanding + Released`? | Finance/billing + product/domain + payer policy owner | `superseded` oleh `IGD-DEC-022` | — | Jawaban user 13 Agustus 2026 |
| `IGD-GAP-001` | Capability Gap (pending audit) | Bila capability audit membuktikan `EmergencyVisitStatus.Disposed` sekaligus mengisi `VisitCompletedAt` tanpa explicit completed/closure state dan evaluasi gate, model state belum memisahkan disposition/disposed dari clinical completion | Backend/API owner + product/domain owner | `pending-audit` | — | Klaim source dari user 13 Agustus 2026; harus diverifikasi di source code |
| `IGD-CONFLICT-001` | Potential Conflict (pending audit) | Bila implementasi backend lain mengharuskan billing `Final` untuk clinical completion, implementasi tersebut bertentangan dengan `IGD-DEC-021` dan tidak boleh mengubah requirement operasional secara diam-diam | Backend/API + Finance/billing + product owner | `pending-audit` | — | `IGD-DEC-021`; verifikasi capability audit diperlukan |
| `IGD-DEC-022` | Decision | Financial clearance merupakan gate wajib sebelum physical release pasien tetapi tidak mensyaratkan seluruh billing atau receivable telah settled. Untuk self-pay, patient responsibility secara default harus diselesaikan sebelum Released kecuali terdapat authorized financial exception. Untuk insurance dan corporate/guarantee, pasien dapat Released ketika eligibility, authorization/guarantee, coverage, dan patient responsibility telah resolved; receivable yang menjadi tanggung jawab payer/guarantor boleh tetap Outstanding setelah pasien Released | Finance/billing + product/domain + payer policy owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026; elaborasi `IGD-DEC-021` |
| `IGD-OQ-019` | Open Question | Apa konfigurasi SOP final untuk financial exception dan financial clearance: jenis exception self-pay, approver `Outstanding + Released`, threshold approval, deposit, co-pay/deductible/excess, dokumen guarantee/authorization, SLA collection, serta coverage dispute pasca-release? | Finance/billing + product/domain + payer policy owner | `superseded` oleh `IGD-DEC-023` untuk prinsip workflow | — | Jawaban user 13 Agustus 2026 |
| `IGD-GAP-002` | Capability Gap (pending audit) | Jika source audit membuktikan encounter payment type aktif hanya mendukung Cash/Insurance sementara corporate masih berada pada legacy type, corporate encounter payment belum dapat dianggap fully implemented | Backend/API + Finance/billing owner | `pending-audit` | — | Klaim source dari user 13 Agustus 2026; verifikasi model, kontrak API, migration, dan penggunaan legacy diperlukan |
| `IGD-AUDIT-001` | Audit Item | Audit penggunaan `PatientEncounterGuarantorType` dan `PatientEncounterGuarantorStatus` legacy untuk menentukan apakah masih kontrak API/database/migration atau dead capability; jangan diaktifkan atau dihapus sebelum evidence tersedia | Backend/API + data owner | `pending-audit` | — | Klaim source dari user 13 Agustus 2026 |
| `IGD-DEC-023` | Decision | Untuk self-pay, Outstanding + Released merupakan controlled financial exception dan bukan normal flow. Exception hanya dapat terjadi melalui kategori yang diizinkan policy, reason, explicit approval, responsible owner, next action, dan immutable audit trail. Deposit dapat digunakan untuk financial clearance hanya setelah dialokasikan terhadap patient responsibility dan dinyatakan mencukupi oleh clearance evaluator; keberadaan deposit semata tidak otomatis memberikan clearance. Approval menggunakan configurable tier/capability matrix, sedangkan nominal threshold dan mapping jabatan MMC tetap mengikuti SOP/delegation-of-authority | Finance/billing + product/domain + security/privacy owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026; elaborasi `IGD-DEC-022` |
| `IGD-FACT-002` | Fact | MMC menerima pasien dengan pembayaran tunai maupun jaminan perusahaan/asuransi | — | `approved` | User stated fact/13 Agustus 2026 | Evidence operasional dari user; tidak membuktikan capability teknis corporate pada encounter payment model |
| `IGD-OQ-020` | Open Question | Apa konfigurasi SOP final financial exception self-pay: kategori yang benar-benar diizinkan, role per approval tier, threshold, dual approval, formula deposit, refund/credit, tenor deferred payment, repeat exception, dan escalation/default collection? | Finance/billing + product/domain + payer policy owner | `superseded` oleh `IGD-DEC-030` untuk production activation gate | — | Jawaban user 13 Agustus 2026 |
| `IGD-GAP-003` | Capability Gap (pending audit) | Belum ada capability source yang terverifikasi untuk patient deposit, deferred-payment agreement, financial release exception, tiered approval, atau self-pay outstanding release; telusuri sebelum membuat duplikasi implementasi | Backend/API + Finance/billing owner | `pending-audit` | — | Klaim source dari user 13 Agustus 2026; harus diverifikasi di source code |
| `IGD-DEC-024` | Decision | Diagnostic result yang menjadi final setelah clinical completion IGD tetap terikat ke diagnostic order dan EncounterId asal dan tidak melakukan automatic reopen. Setiap late result menghasilkan separate review/follow-up workflow dengan explicit clinical ReviewOwner dan fallback owner. Critical results harus masuk acknowledgement dan escalation workflow sesuai batas pelaporan hasil kritis yang berlaku. Pasien atau receiving clinical facility dihubungi berdasarkan keputusan clinical reviewer dan seluruh review, contact attempt, acknowledgement, escalation, serta follow-up diaudit. Apabila pasien kembali untuk pelayanan baru, dibuat EncounterId baru yang mereferensikan episode dan hasil sebelumnya | Clinical governance + diagnostic services + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-021` | Open Question | Apa konfigurasi SOP MMC untuk late-result follow-up: role review owner/fallback, critical value/findings, internal SLA, escalation hierarchy/interval, channel contact, emergency contact, attempts/unreachable, read-back, dan acceptance handover? | Clinical governance + diagnostic services + product/domain owner | `superseded` oleh `IGD-DEC-032` | — | Jawaban user 13 Agustus 2026; configuration yang belum ada fail-closed sesuai policy |
| `IGD-REG-001` | Regulatory Baseline (pending verification) | Klaim bahwa standar akreditasi RS Indonesia mewajibkan pelaporan critical diagnostic result kurang dari 30 menit sejak verifikasi PPA pada unit diagnostik, mencakup lab, radiologi/imaging, cardiac diagnostics, dan POCT | Clinical governance + compliance owner | `pending-verification` | — | Klaim user 13 Agustus 2026; tautan/edisi standar yang berlaku harus diverifikasi sebelum dijadikan batas hard rule |
| `IGD-GAP-004` | Capability Gap (pending audit) | Belum ada capability source yang terverifikasi untuk diagnostic order/result lab-radiologi, DiagnosticReport, post-encounter review, critical acknowledgement, atau late-result contact/escalation; audit integration layer dan alternate naming sebelum membuat model baru | Backend/API + Lab/Radiology integration owners | `pending-audit` | — | Klaim source trace user 13 Agustus 2026; harus diverifikasi di source code |
| `IGD-DEC-025` | Decision | Transaksi lintas modul IGD tidak menggunakan distributed ACID transaction. Setiap module melakukan local atomic commit dan komunikasi lintas module menggunakan stable Command/Message Id, IdempotencyKey, CorrelationId, transactional outbox serta idempotent receiver/inbox. Automatic retry hanya untuk transient failure dengan bounded exponential backoff dan configuration-driven timeout. Timeout tidak otomatis berarti business failure; bila remote outcome tidak diketahui transaksi masuk state OutcomeUnknown dan harus direkonsiliasi sebelum command baru dibuat. Partial failure tidak melakukan destructive global rollback: clinical transaction yang sah tetap dipertahankan sedangkan downstream failure dicatat sebagai Pending/Retry/NeedsReconciliation. Permanent business failure memerlukan correction atau domain-specific compensation dan seluruh retry/reconciliation harus auditable tanpa menyimpan unnecessary PHI | Backend/API + data/integration + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-022` | Open Question | Apa nilai konfigurasi dan ownership final per integration: timeout, retry/backoff, retry window, retention idempotency, reconciliation SLA/queue owner, circuit threshold, serta status-query/native-idempotency vendor? | Integration/API + operations + product/domain owners | `superseded` oleh `IGD-DEC-033` | — | Jawaban user 13 Agustus 2026; integration profile mandatory sebelum production activation |
| `IGD-EVID-001` | Capability Evidence (pending trace) | Klaim source audit: precedent `IdempotencyKey`, `RequestCorrelationId`, unique DB index pada sejumlah model HR, local transaction PatientEncounter, serta commit encounter yang tidak dibatalkan ketika realtime notification gagal | Backend/API owner | `pending-trace` | — | Klaim user 13 Agustus 2026; file/class/commit trace belum dicatat di blueprint |
| `IGD-GAP-005` | Capability Gap (pending audit) | Pattern commit lalu send notification dengan hanya log jika gagal tidak memberi guaranteed cross-module delivery; transactional outbox/inbox/reconciliation perlu diaudit atau diimplementasikan sebelum diandalkan | Backend/API + integration owner | `pending-audit` | — | Klaim source user 13 Agustus 2026; verifikasi existing implementation diperlukan |
| `IGD-DEC-026` | Decision | Authorization IGD menggunakan capability-based, context-aware, dan backend-enforced model. Organizational role hanya merupakan configurable mapping terhadap capability. Registration/Master Patient berwenang pada administrative dan identity workflow; perawat pada triage, nursing care, serta authorized administration; dokter pada clinical decision, order, disposition, dan clinical follow-up; receiving unit pada transfer acceptance/rejection/arrival sesuai destination context; Billing/Finance pada financial processing dan clearance; supervisor hanya memiliki exception/approval capability sesuai domainnya; technical administrator tidak otomatis memiliki clinical atau business authority. Maker-checker wajib untuk ambiguous identity reconciliation dan financial release exception, authority sending/receiving transfer dipisahkan, serta seluruh operation tetap tunduk pada resource context, state transition, credential/privilege, dan segregation-of-duties validation | Security/privacy + product/domain + clinical governance owners | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-023` | Open Question | Apa konfigurasi MMC final untuk mapping job-title ke capability, receiving actor per transfer type, credential source, high-impact approval, finance tier, break-glass review, delegation/on-call, PHI output, dan implementasi authorization existing? | Security/privacy + product/domain + clinical governance owners | `superseded` oleh `IGD-DEC-034` | — | Jawaban user 13 Agustus 2026; configuration/assignment mandatory untuk action terkait |
| `IGD-GAP-006` | Capability Gap (pending audit) | Jika authorization existing hanya broad role/controller/action dan tidak dapat mengekspresikan resource/unit scope, credential, maker-checker, approval tier, atau temporary delegation, capability framework perlu diperluas secara terkontrol | Backend/API + security/privacy owner | `pending-audit` | — | Klaim source trace belum memadai menurut user 13 Agustus 2026; audit authentication, policy/claim, endpoint, assignment, dan credential implementation diperlukan |
| `IGD-DEC-027` | Decision | Respons klinis IGD diukur menggunakan distinct server-authoritative clinical clocks, sekurang-kurangnya Arrival-to-Triage, Arrival-to-Qualified-Clinical-Response, dan Triage-Category-to-First-Clinical-Intervention. Primary patient-safety KPI memakai ArrivalAt sampai FirstQualifiedClinicalResponseAt berdasarkan initial triage category. Merah bertarget Immediate/Zero-Minute tanpa menunggu administrasi; Kuning/Hijau configurable sampai SOP MMC tersedia; Hitam bukan ordinary queue SLA. Retriage append-only membuka category window baru tanpa menghapus arrival/breach. Breach memicu escalation berbasis capability/context dan tetap historis; policy SLA/escalation versioned, effective-dated, dan memiliki profile Normal/Disaster | Clinical governance + product/domain + security/privacy owners | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026; Permenkes 47/2018 untuk baseline kategori/response Merah |
| `IGD-OQ-024` | Open Question | Apa konfigurasi SOP MMC final untuk SLA triase: target Kuning/Hijau, qualifying event, door-to-triage, warning/escalation, Hitam, Normal/Disaster, adjustment, dan policy reuse? | Clinical governance + product/domain owner | `superseded` oleh `IGD-DEC-035` | — | Jawaban user 13 Agustus 2026; target Kuning/Hijau `TargetUnconfigured` sampai SOP tersedia |
| `IGD-CONFLICT-002` | Policy Conflict (pending verification) | Situs publik MMC diklaim menyebut Merah/Jingga/Kuning/Hijau, berkonflik dengan `IGD-DEC-007` dan baseline Permenkes 47/2018 Merah/Kuning/Hijau/Hitam | Clinical governance + product/domain owner | `pending-verification` | — | Klaim user 13 Agustus 2026; perlu snapshot/URL dan SOP triase MMC yang berlaku. Jangan mengubah `IGD-DEC-007` diam-diam |
| `IGD-GAP-007` | Capability Gap (pending audit) | Belum ada evidence source yang cukup mengenai engine SLA triase per kategori, breach/escalation policy, atau model timestamp arrival/triage/qualified response yang authoritative; trace model/controller/service/settings sebelum membangun baru | Backend/API + clinical governance owner | `pending-audit` | — | Klaim branch trace MHamzah dari user 13 Agustus 2026; extend reusable infrastructure bila ada |
| `IGD-DEC-028` | Decision | IGD menggunakan satu accountable Product/Domain Owner untuk end-to-end ownership terhadap scope, workflow, prioritization, cross-domain coordination, dan operational acceptance. Product/Domain Owner bukan universal final approver. Final approval dipisahkan menjadi Clinical, Finance, Privacy, Security, dan Integration authority. Keputusan multi-domain membutuhkan seluruh mandatory impacted-domain approval. Integration memerlukan business/data semantics approval dari affected domain, technical interface approval dari Integration authority, serta Privacy/Security sesuai impact. Authority memakai explicit capability + context, backend validation, separation of duties, dan tidak diturunkan dari technical administrator privilege. Nama orang, jabatan MMC, serta mapping capability existing tetap menunggu evidence internal/source | Product/domain + clinical + finance + privacy + security + integration governance | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-025` | Open Question | Apa pemetaan MMC final untuk Product Owner, Clinical/Finance/Privacy/Security/Integration approver, existing capability, maker-checker governance, dan escalation jika mandatory approver tidak sepakat? | MMC governance sponsor + all domain owners | `superseded` oleh `IGD-DEC-036` | — | Jawaban user 13 Agustus 2026; assignment mandatory go-live configuration |
| `IGD-FACT-004` | Fact | Evidence publik MMC menyatakan komitmen keselamatan pasien, tata kelola manajerial/klinis, TIK terintegrasi, dan pembatasan personal information kepada authorized staff; evidence tersebut tidak menetapkan owner/approver IGD definitif | — | `approved` | User stated fact/13 Agustus 2026 | Evidence publik MMC sebagaimana dirujuk user; detail sumber/URL belum dicatat di blueprint |
| `IGD-GAP-008` | Capability Gap (pending audit) | Source trace belum membuktikan mapping authority existing untuk Product Owner, Clinical/Finance/Privacy/Security/Integration approver; trace authorization/role/claim/policy and existing mapping sebelum membangun duplicate framework | Backend/API + security/privacy + governance owners | `pending-audit` | — | Klaim user 13 Agustus 2026; gunakan capability existing bila compatible |
| `IGD-DEC-029` | Decision | High-impact encounter operation ditentukan berdasarkan clinical, identity, financial, downstream, legal/regulatory, finalization, dan workflow impact. High-impact correction wajib maker-checker. Cancellation hanya diperbolehkan untuk erroneous/duplicate/mistaken encounter tanpa material clinical activity; jika aktivitas material ada, cancellation dilarang dan tidak dapat dilegalkan oleh approval. Low-risk empty cancellation dapat one-actor bila SOP mengizinkan, sedangkan high-risk eligible cancellation wajib maker-checker. Manual reopen selalu high-impact, maker-checker, dan bounded terhadap specific scope; physical return setelah completed selalu encounter baru. Maker berasal dari affected-domain actor, checker UserId berbeda dengan approval capability domain, dan cross-domain operation mengikuti `IGD-DEC-028`. Semua backend-validated, immutable-audited, server-timestamped, idempotent, tanpa destructive clinical mutation | Clinical + registration/master patient + finance + security/privacy governance | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-026` | Open Question | Apa parameter SOP MMC final untuk high-impact operation: maker/checker mapping, financial/bulk threshold, material clinical activity, empty-cancel checker, rejection/escalation, approval SLA, dan capability mapping existing? | Clinical + finance + registration/master patient + security/privacy governance | `superseded` oleh `IGD-DEC-037` | — | Jawaban user 13 Agustus 2026; missing checker/configuration fails closed |
| `IGD-GAP-009` | Capability Gap (pending audit) | Source trace belum membuktikan end-to-end workflow request-approval-execution untuk encounter correction, cancellation, atau reopen; audit/extend infrastructure compatible sebelum membuat framework baru | Backend/API + security/privacy + governance owners | `pending-audit` | — | Klaim user 13 Agustus 2026; reuse/extend jika capability approval/audit/maker-checker tersedia |
| `IGD-DEC-030` | Decision | Pada production awal, self-pay dengan BillingStatus Outstanding tidak dapat memperoleh normal AdministrativeReleaseStatus Released. Encounter tetap dapat Completed secara klinis sesuai `IGD-DEC-021` dan billing outstanding tidak boleh membuat pasien tetap dianggap aktif secara klinis di IGD. Financial clearance tidak boleh menghambat emergency care atau clinically required disposition/transfer. Physical departure dicatat terpisah dari financial administrative release. Outstanding + Released hanya dapat diaktifkan sebagai high-impact maker-checker controlled exception setelah SOP/authority MMC menetapkan eligibility, threshold, deposit, reason, evidence, approval authority, settlement SLA, dan escalation. Finance adalah mandatory approval domain; cross-domain approval mengikuti `IGD-DEC-028` | Finance/billing + product/domain + clinical governance owners | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026; elaborasi `IGD-DEC-021`/`IGD-DEC-023` |
| `IGD-OQ-027` | Open Question | Apa policy MMC final untuk mengaktifkan self-pay `Outstanding + Released`: eligibility, threshold/percentage, deposit, reason/evidence, finance maker-checker, settlement SLA/escalation, dan physical departure sebelum clearance? | Finance/billing + product/domain + payer policy owner | `superseded` oleh `IGD-DEC-038` | — | Jawaban user 13 Agustus 2026; feature flag disabled sampai seluruh activation gate tersedia |
| `IGD-EVID-002` | Capability Evidence (pending trace) | Klaim source: EmergencyVisitService memungkinkan transition klinis ke Disposed tanpa billing validation dan BillingManagement belum menunjukkan transactional financial-release governance | Backend/API + Finance/billing owner | `pending-trace` | — | Klaim user 13 Agustus 2026; file/class/commit trace belum dicatat di blueprint |
| `IGD-GAP-010` | Capability Gap (pending audit) | Financial-release policy/workflow untuk self-pay outstanding belum boleh dianggap ada sampai implementation/evidence teridentifikasi; audit/extend capability existing sebelum membangun baru | Backend/API + Finance/billing owner | `pending-audit` | — | `IGD-DEC-030`; klaim source user 13 Agustus 2026 |
| `IGD-DEC-031` | Decision | Identity merge menggunakan maker Registration/Master Patient ber-capability; ambiguous merge dan reverse merge maker-checker/high-impact, fail-closed bila approver belum dipetakan, dan cross-domain impact mengikuti `IGD-DEC-028` | Master Patient + security/privacy + impacted-domain governance | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-032` | Decision | Late result memakai ordering clinician dengan covering fallback, critical catalog/version, acknowledgment/follow-up wajib, escalation critical T+0/T+5/T+10/T+20 dan breach T+30, serta default review non-critical ≤24 jam | Clinical governance + diagnostic services + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-033` | Decision | Setiap integration production membutuhkan Reliability Profile: internal timeout 10s/≤2 safe attempts, external timeout 30s, state-changing vendor retry hanya bila idempotency terbukti, otherwise OutcomeUnknown + reconciliation, dan local outbox/inbox architecture tetap berlaku | Integration/API + operations + product/domain owners | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-034` | Decision | Capability assignment memakai functional bundle/configuration, credential effective-dated, transfer authority terpisah, scoped/audited delegation, separate break-glass, dan deny-by-default PHI output tanpa blanket technical-admin access | Security/privacy + product/domain + clinical governance owners | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-035` | Decision | Triage SLA operational policy mempertahankan Red immediate, Yellow/Green TargetUnconfigured sampai SOP ada, qualifying clinical response yang eksplisit, versioned Normal/Disaster profile, Normal fallback dalam disaster bila profile belum tersedia, dan immutability retriage/breach | Clinical governance + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-036` | Decision | Mandatory GovernanceAssignment harus aktif sebelum production untuk Product Owner dan approver Clinical/Finance/Privacy/Security/Integration; missing assignment fail-closed untuk affected governance decision dan exact person/job-title menjadi go-live evidence/configuration | MMC governance sponsor + all domain owners | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-037` | Decision | High-impact parameter memakai conservative defaults: finalized financial impact non-nol dan bulk ≥2 encounter/patient high-impact, approval expiry tidak auto-approve, missing checker fails closed, material clinical activity denies cancellation, dan manual reopen always high-impact/scoped/maker-checker | Clinical + finance + registration/master patient + security/privacy governance | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-038` | Decision | Self-pay Outstanding + Released tetap disabled sampai seluruh written policy, exception catalog, threshold/deposit, maker-checker Finance+Product governance, evidence, receivable ownership, audit, UAT, dan feature-flag activation gate terpenuhi; clinical safety tidak dihambat | Finance/billing + product/domain + clinical governance owners | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user 13 Agustus 2026 |
| `IGD-OQ-028` | Open Question | Siapa owner Registration/Master Patient yang berwenang menyetujui kontrak provisional identity, termasuk representasi yang sah sebelum `PatientId` definitif ada dan boundary terhadap `TemporaryPatientId`? | Registration/Master Patient owner + Product/Domain Owner | `superseded` oleh `IGD-DEC-039` | â€” | Closure Pass 13 Agustus 2026; jawaban user memilih opsi A |
| `IGD-DEC-039` | Decision | Registration Management accountable atas representasi provisional identity pada encounter. Patient Management menyetujui boundary lifecycle/reconciliation terhadap master patient, dan Product/Domain Owner menyetujui kontrak lintas-domain. Keputusan ini tidak membuat `TemporaryPatientId` atau master patient IGD baru dan tetap memerlukan approval formal dari owner berwenang. | Registration Management + Patient Management + Product/Domain Owner | `draft` | User selected option A/13 Agustus 2026; authority formal belum diidentifikasi | Closure Pass 13 Agustus 2026; `01-existing-capability-map.md` ownership map untuk encounter dan master patient |
| `IGD-OQ-029` | Open Question | Siapa pemilik dan approver kontrak clinical context untuk pelayanan IGD provisional, agar Clinical Management dan Pharmacy dapat memakai encounter yang sama tanpa duplikasi rekam klinis? | Clinical Management owner + Registration API owner + Pharmacy owner | `superseded` oleh `IGD-DEC-040` | â€” | Closure Pass 13 Agustus 2026; jawaban user memilih opsi A |
| `IGD-OQ-030` | Conflict | Apakah `EncounterType.Outpatient` tetap menjadi contract canonical IGD, atau target harus diubah menjadi `EncounterType.Emergency`, mengingat frontend saat ini mengirim Emergency sementara service emergency visit hanya menerima Outpatient? | Product/Domain Owner + Registration API owner + Emergency Installation owner | `superseded` oleh `IGD-DEC-041` | â€” | Closure Pass 13 Agustus 2026; jawaban user memilih opsi A |
| `IGD-OQ-031` | Open Question | Siapa owner canonical untuk disposition, transfer, clinical completion, administrative release, dan billing completion, serta bagaimana kontrak state antar-owner tersebut disetujui tanpa menjadikan salah satu state sebagai proxy bagi yang lain? | Clinical governance + Emergency Installation + Registration + Finance/Billing owners | `superseded` oleh `IGD-DEC-042` | â€” | Closure Pass 13 Agustus 2026; jawaban user memilih opsi A |
| `IGD-OQ-032` | Unknown | Sistem apa yang memiliki order/result lab-radiologi, critical-result routing, dan late-result follow-up; siapa owner yang menyetujui semantic contract, reliability profile, serta responsibility handover ke IGD? | Diagnostic Services + Clinical governance + Integration owners | `superseded` oleh `IGD-DEC-043` | â€” | Closure Pass 13 Agustus 2026; jawaban user memilih opsi A |
| `IGD-OQ-033` | Open Question | Apakah mass-casualty state merupakan source of truth domain incident/disaster eksternal atau dimiliki Emergency Installation Management, dan siapa approver contract aktivasi/konfirmasi/penonaktifannya? | Incident command + Product/Domain Owner + Integration owner | `superseded` oleh `IGD-DEC-044` | â€” | Closure Pass 13 Agustus 2026; jawaban user memilih opsi A |
| `IGD-OQ-034` | Ownership | Siapa assignee formal untuk Product/Domain, Clinical, Finance, Privacy, Security, dan Integration approval; serta siapa escalation authority ketika mandatory approver lintas-domain tidak sepakat? | MMC governance sponsor | `superseded` oleh `IGD-DEC-045` | â€” | Closure Pass 13 Agustus 2026; jawaban user memilih opsi A |
| `IGD-DEC-040` | Decision | Clinical Management menjadi accountable owner kontrak clinical context untuk encounter provisional. `EncounterId` adalah konteks klinis canonical. Registration Management menyetujui aturan penggunaan encounter/provisional dan Pharmacy Management menyetujui kompatibilitas konsumsi konteks. Kontrak tidak boleh membuat atau menduplikasi rekam klinis IGD. | Clinical Management + Registration Management + Pharmacy Management | `draft` | User memilih opsi A/13 Agustus 2026; authority formal belum diidentifikasi | Closure Pass 13 Agustus 2026; `01-existing-capability-map.md` ownership map shared clinical facts, encounter, dan pharmacy |
| `IGD-DEC-041` | Decision | `EncounterType.Outpatient` tetap menjadi tipe encounter canonical IGD sesuai `IGD-DEC-001`. Frontend wajib diselaraskan dengan kontrak backend; `EncounterType.Emergency` pada frontend saat ini adalah conflict provider/consumer dan bukan kontrak target yang disetujui. | Product/Domain Owner + Registration API owner + Emergency Installation owner | `draft` | User memilih opsi A/13 Agustus 2026; authority formal belum diidentifikasi | Closure Pass 13 Agustus 2026; `IGD-DEC-001`; `01-existing-capability-map.md` provider/consumer mismatch encounter type |
| `IGD-DEC-042` | Decision | Ownership state bersifat federated: Emergency Installation mengelola disposition, transfer, dan closure episode; Clinical Governance menyetujui guard clinical completion; Registration mengelola administrative release; Finance/Billing mengelola billing dan financial clearance. Kontrak state wajib versioned dan satu state tidak boleh menjadi proxy bagi state milik domain lain. | Emergency Installation + Clinical Governance + Registration + Finance/Billing | `draft` | User memilih opsi A/13 Agustus 2026; authority formal belum diidentifikasi | Closure Pass 13 Agustus 2026; `IGD-DEC-020`, `IGD-DEC-021`; `01-existing-capability-map.md` closure-state conflict |
| `IGD-DEC-043` | Decision | Sistem/domain Diagnostic Services yang ditunjuk menjadi source of truth untuk diagnostic order dan result. IGD mengelola acknowledgement dan handover follow-up, Integration menyetujui Reliability Profile, dan Clinical Governance menyetujui semantic serta critical-result rule. Sampai sistem dan owner bernama dibuktikan, IGD tidak boleh menduplikasi fakta diagnostik atau mengaktifkan integrasi produksi. | Diagnostic Services + Clinical Governance + Integration + Emergency Installation | `draft` | User memilih opsi A/13 Agustus 2026; sistem/authority formal belum diidentifikasi | Closure Pass 13 Agustus 2026; `IGD-DEC-024`, `IGD-DEC-032`, `IGD-DEC-033`; `01-existing-capability-map.md` diagnostic owner/contract belum teridentifikasi |
| `IGD-DEC-044` | Decision | Sementara itu Emergency Installation menjadi source of truth state operasional mode korban massal/bencana berdasarkan `IGD-DEC-010` sampai `IGD-DEC-015`. Sinkronisasi otomatis dengan domain incident/disaster eksternal diblokir sampai owner, kontrak, approver, dan reliability evidence eksternal terbukti. Keputusan ini bukan penetapan permanent enterprise source of truth. | Emergency Installation + Incident Command + Product/Domain Owner + Integration | `draft` | User memilih opsi A/13 Agustus 2026; authority formal belum diidentifikasi | Closure Pass 13 Agustus 2026; `IGD-DEC-010`–`IGD-DEC-015`; `01-existing-capability-map.md` disaster domain di luar repository belum diketahui |
| `IGD-DEC-045` | Decision | Sponsor governance MMC menetapkan `GovernanceAssignment` untuk Product/Domain, Clinical, Finance, Privacy, Security, dan Integration, masing-masing dengan `UserId`, capability, scope, primary/delegate, masa berlaku, dan evidence. Direktur atau pejabat governance yang ditunjuk menjadi authority eskalasi deadlock. Assignment dan jalur eskalasi belum dianggap aktif sampai evidence penetapan formal tersedia. | MMC governance sponsor | `draft` | User memilih opsi A/13 Agustus 2026; assignee dan authority formal belum diidentifikasi | Closure Pass 13 Agustus 2026; `IGD-DEC-028`; `IGD-DEC-036` |

## Evidence Eksternal Indonesia

- [Permenkes Nomor 1 Tahun 2026 tentang KLB, Wabah, dan Krisis Kesehatan](https://jdih.kemkes.go.id/documents/peraturan-menteri-kesehatan-nomor-1-tahun-2026)
  adalah payung terkini dan berlaku sejak 21 Januari 2026. Regulasi ini mencabut Permenkes
  75/2019 dan Permenkes 19/2016; ketentuan SPGDT kini berada di Permenkes 1/2026.
- [Permenkes Nomor 47 Tahun 2018 tentang Pelayanan Kegawatdaruratan](https://jdih.kemkes.go.id/documents/peraturan-menteri-kesehatan-nomor-47-tahun-2018)
  tercatat tetap berlaku pada JDIH Kementerian Kesehatan per 13 Agustus 2026. Regulasi ini
  menetapkan empat warna untuk triase IGD, retriase berkelanjutan, dan tag triase ketika
  menangani bencana/korban dalam jumlah banyak.
- [Kepmenkes HK.01.07/Menkes/1502/2023 tentang Pedoman Nasional Penanggulangan Krisis Kesehatan](https://jdih.kemkes.go.id/documents/keputusan-menteri-kesehatan-nomor-hk0107menkes15022023)
  tercatat tetap berlaku dan menjadi pedoman operasional klaster kesehatan, EMT, triase,
  surge, rujukan, data/surveilans, serta penanganan lintas sub-klaster.
- [Pedoman Nasional Penanggulangan Krisis Kesehatan](https://pusatkrisis.kemkes.go.id/__pub/files49279Final_Pedoman_Nasional_Penanggulangan_Krisis_Kesehatan.pdf)
  mendefinisikan kategori Merah, Kuning, Hijau, dan Hitam berdasarkan prioritas serta ABCDE
  dalam konteks krisis/bencana dan triase di fasilitas kesehatan.
- [Artikel Direktorat Jenderal Kesehatan Lanjutan mengenai IGD RSUP Dr. Sardjito](https://keslan.kemkes.go.id/view_artikel/1081/ruang-resusitasi-igd-rsup-dr-sardjito-yogyakarta-untuk-pelayanan-lebih-baik)
  mendeskripsikan penggunaan empat warna pada pemilahan pasien IGD.
- [Playbook SATUSEHAT Pelayanan IGD](https://satusehat.kemkes.go.id/platform/docs/id/interoperability/igd/)
  menjadi evidence awal bahwa data triase terhubung ke satu `Encounter`; pemetaan kontrak
  final tetap menunggu capability audit.

## Acceptance Criteria

Acceptance criteria awal dari dokumen sumber, seluruhnya berstatus `draft` sampai owner dan
rule terkait disetujui:

1. Sistem menolak emergency visit aktif kedua pada encounter yang sama.
2. Pelengkapan encounter provisional mempertahankan `EncounterId` dan seluruh relasi klinis.
3. Pasien level cepat dapat memperoleh tindakan/resusitasi tanpa menunggu administrasi
   lengkap, tetapi setiap transaksi tetap tertaut ke encounter.
4. Retriage membuat record baru dan mempertahankan record sebelumnya.
5. Observation detail dapat mereferensikan vital sign/CPPT tanpa menyalin data klinis umum.
6. Rawat inap tidak menutup visit sebelum admission, handover, dan transfer mencapai state
   yang disetujui.
7. PAPS menyimpan alasan, bukti consent/saksi, kondisi saat keluar, instruksi keselamatan,
   dan audit.
8. Semua mutasi penting dapat diaudit tanpa mengekspos PHI pada payload log.
9. Dalam mode korban massal/bencana, setiap korban memiliki hasil triase empat warna yang
   tertaut pada konteks insiden; retriase membuat histori baru tanpa menimpa hasil sebelumnya.
10. Aktivasi oleh Kepala IGD langsung berhasil tanpa lampiran dan menghasilkan `Active —
    Pending Confirmation`, sedangkan incident commander resmi menghasilkan `Active —
    Confirmed` serta notifikasi kepada direktur dan komite.
11. Kepala IGD yang tidak merangkap incident commander ditolak ketika mencoba menonaktifkan
    mode; incident commander dan direktur dapat menonaktifkannya.
12. Setiap perubahan state menggunakan waktu server dan menyimpan seluruh field audit yang
    diputuskan; audit log tidak memuat PHI atau payload klinis lengkap.
13. Bukti tambahan dapat ditautkan setelah aktivasi tanpa mengubah histori aktivasi awal dan
    hanya dapat diakses oleh role yang berwenang.
14. Penolakan konfirmasi tidak menonaktifkan mode atau menghentikan pelayanan; mode tetap
    aktif dengan penanda `Confirmation Rejected` hingga incident commander atau direktur
    menonaktifkannya secara eksplisit, dan kedua event memiliki audit trail terpisah.
15. Batas waktu konfirmasi dan jalur eskalasi berasal dari konfigurasi SOP yang berlaku dan
    tidak di-hardcode.
16. Provisional encounter dapat dibuat dengan field minimum `IGD-DEC-016` tanpa NIK, nomor
    rekam medis existing, alamat lengkap, kontak, penanggung jawab, penjamin/asuransi,
    dokumen identitas, atau data rujukan sebagai blocker bagi pasien gawat/tidak dikenal.
17. Pelengkapan registrasi menggunakan `EncounterId` yang sama dan tidak memutus relasi triage,
    retriage, tindakan, observasi, order, atau data klinis yang telah terbentuk.
18. Keterlambatan pelengkapan administrasi tidak otomatis menghentikan atau memblokir pelayanan
    klinis; `AdministrativeStatus = Complete` hanya tercapai ketika field SOP/konfigurasi
    registrasi MMC terpenuhi.
19. Temporary patient melalui lifecycle `Temporary → IdentityFound → ReconciliationPending →
    Merged / Resolved`; pencarian kandidat pasien existing wajib sebelum membuat `PatientId`
    definitif baru dan sistem tidak melakukan automatic merge dari similarity data.
20. Reconciliation tidak mengubah `EncounterId`, tidak membuat encounter baru atau menyalin
    transaksi klinis, tidak menghentikan pelayanan, dan menyisakan `TemporaryPatientId` sebagai
    historical linkage ke `PatientId` definitif.
21. Merge idempotent, temporary patient yang telah `Merged` tidak dapat membuat encounter baru,
    dan kesalahan reconciliation memakai correction/reversal terkontrol dengan audit trail.
22. Audit reconciliation menyimpan metadata keputusan dan reference ID evidence tanpa PHI atau
    payload klinis lengkap.
23. Reconciliation simple-match tanpa konflik dapat diselesaikan oleh permission
    `PatientIdentity.Reconcile`/`PatientIdentity.Merge`; kasus ambigu/konflik wajib memakai
    maker-checker dengan `UserId` maker dan approver yang berbeda.
24. Penolakan approval tidak menghapus data atau mengubah `EncounterId`; temporary patient tetap
    pending reconciliation. Reversal merge yang salah hanya memakai workflow khusus dan tidak
    boleh dilakukan lewat edit langsung database.
25. Capability patient identity dikunci dan mapping `Organization Role → Permission/Capability`
    configurable; nama role organisasi MMC tidak di-hardcode, menunggu evidence, dan bukan
    blocker desain modul IGD.
26. Backend menolak transisi lifecycle tanpa capability yang sesuai, menyimpan audit immutable,
    dan menerapkan idempotency pada seluruh command perubahan state.
27. Completion hanya berhasil setelah backend memvalidasi disposition dan seluruh closure gate;
    transfer rawat inap/transfer harus `Arrived` sebelum completion.
28. Cancel ditolak setelah ada aktivitas klinis material; correction menggunakan amendment yang
    mempertahankan histori; pasien kembali setelah completed memperoleh encounter baru.
29. Encounter dapat completed dengan billing `Pending`/`Outstanding` hanya setelah seluruh
    clinical closure gate terpenuhi dan billing handoff berisi reason, owner, next action,
    SLA/escalation SOP, serta audit trail telah dibuat.
30. Late charge, koreksi/adjustment billing, dan proses finansial pasca-close tidak otomatis
    reopen clinical encounter dan tetap mereferensikan `EncounterId` yang sama.
31. Physical release mengikuti aturan financial clearance yang configurable menurut payer/payment
    class; billing belum final tidak mempertahankan pasien pada `InService`.
32. `AdministrativeReleaseStatus = Released` ditolak bila financial clearance belum `Cleared`,
    kecuali hasil clearance menunjukkan exception berwenang dengan evidence dan audit lengkap.
33. Payer receivable dapat outstanding setelah release jika coverage/guarantee disetujui dan
    `PatientOutstandingAmount = 0`; patient outstanding tanpa arrangement disetujui memblokir
    release.
34. Self-pay release dengan patient outstanding hanya dapat terjadi melalui exception berwenang;
    self-approval dilarang dan outstanding tetap menjadi receivable yang dilunasi tanpa reopen
    clinical encounter.
35. Deposit tidak otomatis clearance; sistem wajib memvalidasi alokasi, kecukupan terhadap
    responsibility, dan charge mandatory yang masih belum terposting.
36. Late result tidak otomatis reopen encounter; review/follow-up, contact, acknowledgement, dan
    escalation tercatat terpisah serta idempotent terhadap repeated delivery diagnostic result.
37. Critical result ditutup hanya setelah acknowledgement dan action/follow-up yang disyaratkan;
    pasien kembali karena late result memperoleh encounter baru yang mereferensikan episode asal.
38. Cross-module retry memakai idempotency key yang sama, bounded retry hanya untuk failure
    transient, dan `OutcomeUnknown` wajib direkonsiliasi sebelum command baru dibuat.
39. Partial failure tidak menghapus transaksi klinis yang sah; delivery state ditampilkan dan
    diaudit terpisah, dengan reconciliation/compensation sesuai domain.
40. Semua mutation endpoint melakukan capability, context, state, credential/privilege, dan
    segregation-of-duties validation di backend; role organisasi/frontend flag tidak cukup.
41. Maker-checker wajib pada identity merge ambigu dan financial release exception; transfer
    sender/receiver, clinical/finance, dan technical/business authority dipisahkan.
42. SLA triase dihitung backend dari clinical clock server-authoritative; Merah immediate tanpa
    menunggu administrasi, Kuning/Hijau configurable, dan breach/escalation tetap audit-historis.
43. Retriage membuka SLA window kategori baru tanpa mengubah `ArrivalAt` atau menghapus breach;
    operation mode/policy version disimpan pada setiap evaluasi.
44. Product/Domain Owner accountable atas operasi tetapi bukan universal approver; keputusan
    lintas domain hanya approved setelah seluruh capability domain wajib telah menyetujui.
45. Approval governance memakai capability/context/backend validation, versioned decision record,
    dan maker-checker sesuai risk; technical access tidak memberi business approval authority.
46. High-impact correction dan seluruh manual reopen wajib maker-checker; cancellation ditolak
    setelah aktivitas klinis material dan tidak dapat di-override oleh approval.
47. Correction/cancel/reopen append-only, bounded, idempotent, auditable, serta merekonsiliasi
    downstream tanpa physical delete atau silent overwrite.
48. Self-pay outstanding tidak dapat administrative release pada production awal; encounter tetap
    clinical completed dan physical departure dicatat terpisah bila terjadi.
49. Aktivasi future `Outstanding + Released` membutuhkan policy/evidence, Finance maker-checker,
    dan cross-domain approval; financial exception tidak menunda tindakan keselamatan pasien.
50. Ambiguous/reverse identity merge ditolak saat checker/approver assignment belum tersedia;
    case mempertahankan status pending tanpa auto-merge.
51. Critical late result menghasilkan escalation T+0/T+5/T+10/T+20 dan breach T+30; non-critical
    review default tercatat paling lambat 24 jam, dan critical `UnableToReach` tidak auto-close.
52. Integration tanpa Reliability Profile ditolak dari production activation; vendor state-changing
    timeout tanpa idempotency/status-query menjadi `OutcomeUnknown`, bukan blind retry.
53. Transfer, credential/delegation, break-glass, dan PHI output ditolak di backend tanpa
    capability/context/effective assignment yang valid.
54. Triage Yellow/Green tanpa target policy tampil `TargetUnconfigured`, bukan compliant/breached;
    disaster tanpa profile memakai Normal fallback yang tercatat.
55. Governance decision dengan mandatory approver yang belum terassignment tidak menjadi approved;
    high-impact operation tanpa checker tidak dapat dieksekusi atau auto-approved ketika SLA lewat.
56. Financialized record dengan impact non-nol dan bulk impact minimal dua encounter/patient
    diperlakukan high-impact sampai threshold policy yang lebih spesifik tersedia.
57. Feature self-pay outstanding release tetap disabled sampai seluruh policy, approval, evidence,
    receivable ownership, audit, UAT, dan effective-dated activation gate terpenuhi.
58. Clinical Management dan Pharmacy menggunakan `EncounterId` yang sama sebagai clinical context
    provisional; modul IGD tidak membuat rekam klinis paralel.
59. Frontend mengirim `EncounterType.Outpatient` untuk episode IGD dan backend menolak kontrak
    type yang tidak canonical secara konsisten.
60. Disposition/transfer/episode closure, clinical-completion guard, administrative release, serta
    billing/financial clearance hanya dapat dimutasi oleh owner domainnya dan tidak saling
    mengimplikasikan state lain tanpa kontrak versioned yang disetujui.
61. Diagnostic order/result canonical tetap berada pada Diagnostic Services; tanpa sistem/owner
    bernama, semantic contract, dan Reliability Profile yang disetujui, integrasi diagnostik
    produksi tidak aktif dan fakta diagnostik tidak diduplikasi di IGD.
62. State operasional mode korban massal/bencana dikelola Emergency Installation sementara;
    sinkronisasi otomatis ke domain incident/disaster eksternal tetap diblokir sampai kontrak dan
    reliability evidence eksternal disetujui.
63. Governance decision lintas-domain tidak dapat menjadi approved tanpa `GovernanceAssignment`
    aktif dan terbukti; deadlock mengikuti authority eskalasi yang ditetapkan formal.

## Open Questions dan Blocker

### Status arsitektur saat ini

`IGD-DEC-031` sampai `IGD-DEC-045` menutup pertanyaan arsitektur yang saat ini telah dijawab
secara draft. Tidak satu pun merupakan approval formal: `IGD-DEC-039` sampai `IGD-DEC-045`
menunggu assignment dan approval owner yang berwenang.

Sebelum production, wajib tersedia: governance assignment; identity merge checker/reverse approver;
nama sistem/owner serta kontrak Diagnostic Services; critical-result catalog dan escalation;
Integration Reliability Profile/vendor matrix; bukti runtime/test registrasi `Emergency*Service`
dan aktivasi controller; SLA Kuning/Hijau dan Disaster profile atau fallback; high-impact
checker/approval configuration; serta self-pay outstanding release yang tetap disabled hingga
seluruh activation gate terpenuhi.

Item `IGD-GAP-*`, `IGD-EVID-*`, `IGD-AUDIT-*`, `IGD-CONFLICT-*`, dan `IGD-REG-001` adalah audit/
evidence item, bukan open architecture question. Missing configuration/evidence bersifat
fail-closed; clinical emergency care tetap patient-safety-first.

### Pertanyaan aktif — jawab satu per giliran

Tidak ada `IGD-OQ` aktif. `IGD-OQ-028` sampai `IGD-OQ-034` telah tersupersesi oleh
`IGD-DEC-039` sampai `IGD-DEC-045` sebagai draft. Evidence dan konfigurasi yang belum tersedia
tetap mandatory go-live gate dengan behavior fail-closed; layanan klinis emergensi tidak boleh
diasumsikan menunggu administrasi.

### Catatan blocker historis (superseded)

- Kewenangan aktivasi/penonaktifan serta behavior penolakan konfirmasi ditutup secara draft
  melalui `IGD-DEC-010` sampai `IGD-DEC-014`.
- Nilai batas waktu dan jalur eskalasi harus diisi serta dikelola dari SOP yang berlaku sebelum
  konfigurasi mode bencana digunakan secara operasional.
- Model governance telah ditutup melalui `IGD-DEC-028`; pemetaan MMC final dan deadlock
  escalation tetap terbuka pada `IGD-OQ-025`.
- `IGD-OQ-016` hanya menunggu evidence pemetaan nama role organisasi dan bukan blocker desain;
  capability, separation of duties, dan safety rule temporary patient telah ditutup secara draft
  melalui `IGD-DEC-017` sampai `IGD-DEC-019`.
- Parameter dan mapping high-impact correction/reopen terbuka pada `IGD-OQ-026`; guardrail
  workflow telah dikunci melalui `IGD-DEC-029`.
- Parameter self-pay outstanding-release terbuka pada `IGD-OQ-027`; production gate telah dikunci
  melalui `IGD-DEC-030`, tanpa mengubah clinical completion pada `IGD-DEC-021`.
- `IGD-GAP-001` dan `IGD-CONFLICT-001` menunggu capability audit backend; keduanya belum
  merupakan fakta source code.
- `IGD-GAP-002` dan `IGD-AUDIT-001` menunggu audit model corporate serta enum guarantor legacy;
  corporate encounter payment belum boleh diasumsikan tersedia.
- `IGD-GAP-003` menunggu capability audit finansial; tidak boleh membuat duplikasi bila capability
  deposit, deferred payment, exception, atau approval bertingkat ternyata sudah tersedia.
- `IGD-EVID-002` dan `IGD-GAP-010` menunggu trace/audit financial-release implementation;
  outstanding release self-pay tidak boleh diasumsikan tersedia.
- `IGD-OQ-021`, `IGD-REG-001`, dan `IGD-GAP-004` menunggu SOP/evidence regulatori serta audit
  capability diagnostic-result; workflow patient-safety telah dikunci melalui `IGD-DEC-024`.
- Nilai konfigurasi reliability dan evidence outbox/inbox menunggu `IGD-OQ-022`, `IGD-EVID-001`,
  serta `IGD-GAP-005`; architectural policy telah dikunci melalui `IGD-DEC-025`.
- Pemetaan organisasi dan authorization evidence menunggu `IGD-OQ-023` dan `IGD-GAP-006`;
  capability/context/SOD model telah dikunci melalui `IGD-DEC-026`.
- Parameter SLA klinis, conflict warna, dan capability engine menunggu `IGD-OQ-024`,
  `IGD-CONFLICT-002`, dan `IGD-GAP-007`; measurement architecture telah dikunci melalui
  `IGD-DEC-027`.
- Governance mapping/evidence menunggu `IGD-OQ-025` dan `IGD-GAP-008`; high-impact
  correction/reopen detail masih diblokir oleh `IGD-OQ-026`.
  Pass cukup.

## Closure Pass 2026-08-14

Pass ini menutup blocker yang menghalangi `design-business-module` melewati gerbang input.
Konteks lengkap ada di `docs/agency/update-skills/03-revisi-design-business-module.md`.

### Fakta yang ditemukan dari source, bukan dari wawancara

| ID | Fakta | Bukti |
| --- | --- | --- |
| `IGD-FACT-005` | Skema warna triase belum ada di kode. `MstEmergencyTriageLevel` memiliki kolom `ColorName` dan `ColorHex`, tetapi tidak ada seeder level triase | `Areas/HealthServices/MasterData/Models/MstEmergencyTriageLevel.cs`; pencarian seeder tidak menemukan hasil |
| `IGD-FACT-006` | Enum `EmergencyTriageSystem` hanya memuat `ATS` dan `ESI`, keduanya skala lima level, sedangkan Permenkes 47/2018 memakai empat kategori warna. Pemetaan warna ke level belum ditetapkan | `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyTriageSystem.cs` |
| `IGD-FACT-007` | Modul IGD tidak memiliki satu pun rujukan ke billing. Status akhir kunjungan adalah `Disposed`, tidak ada status `Closed` terpisah | Pencarian pada seluruh `Areas/HealthServices/EmergencyInstallationManagement/`; `EmergencyVisitStatus.cs` |
| `IGD-FACT-008` | Tidak ada file `.cs` maupun `src/` yang berubah antara snapshot manifest revision 3 dan HEAD kedua repository. Bukti source pada capability map masih sahih | Impact scan `fa772b71..HEAD` backend dan `e77ebd80..HEAD` frontend |

### Keputusan

| ID | Jenis | Isi | Owner | Status | Bukti | Catatan |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-DEC-046` | Decision | Product/Domain Owner IGD dipegang sementara oleh pemilik suite skill, berwenang atas scope, workflow, prioritisasi, dan penerimaan operasional. Clinical governance owner dan security/privacy owner tetap `OPEN` dan menjadi syarat go-live. Keputusan klinis tidak disahkan oleh Product/Domain Owner; keputusan tersebut memakai regulasi yang berlaku sebagai baseline sampai clinical governance owner ditunjuk | Product/domain owner | `approved` | Jawaban pemilik suite skill 14 Agustus 2026 | Menutup blocker kepemilikan pada gerbang `design-business-module`. Nama orang perlu diisi sebelum dipakai sebagai bukti approval formal |

### Blocker yang tersisa dan penanganannya

| ID | Status | Penanganan |
| --- | --- | --- |
| `IGD-CONFLICT-002` | Sedang ditutup pada pass ini | Skema kategori triase |
| `IGD-CONFLICT-001` | Sedang ditutup pada pass ini | Penutupan klinis versus billing |
| `IGD-GAP-006`, `IGD-GAP-007`, `IGD-GAP-008` | Bukan pertanyaan wawancara | Jawabannya ada di source code. Diteruskan ke `/trace-existing-capabilities` mode impact scan, bukan ditanyakan kepada pengguna |

| `IGD-DEC-047` | Decision | Skema kategori triase IGD memakai baseline regulasi Permenkes 47/2018: Merah, Kuning, Hijau, dan Hitam. Klaim skema Merah/Jingga/Kuning/Hijau dari situs publik MMC dicatat sebagai evidence belum terverifikasi dan tidak dipakai. Klaim tersebut hanya boleh menggantikan baseline apabila SOP triase MMC yang disahkan dan masih berlaku diserahkan, disertai snapshot atau URL sumber | Product/domain owner, dengan clinical governance owner sebagai approver akhir saat ditunjuk | `approved` | Jawaban pemilik suite skill 14 Agustus 2026; hierarki sumber kebenaran project menaruh regulasi di urutan 1 dan klaim tanpa bukti di urutan 7 | Menutup `IGD-CONFLICT-002`. Tidak mengubah `IGD-DEC-007`, melainkan menegaskannya. Keputusan ini dibuat sebagai baseline regulasi sesuai `IGD-DEC-046`, bukan sebagai persetujuan klinis |
| `IGD-DEC-048` | Decision | Skala triase tetap memakai `EmergencyTriageSystem` ATS atau ESI dengan `Level` 1 sampai 5 sebagaimana enum yang sudah ada di kode. Kategori warna Permenkes menjadi pengelompokan atas skala tersebut: Merah untuk level 1 dan 2, Kuning untuk level 3, Hijau untuk level 4 dan 5. Hitam adalah kategori tersendiri di luar skala antrean dan tidak menjadi nilai `Level` biasa | Product/domain owner, dengan clinical governance owner sebagai approver akhir saat ditunjuk | `approved` | Jawaban pemilik suite skill 14 Agustus 2026 | Menjaga makna frasa "pasien level 1–2" dan "pasien tak dikenal level 1" pada keputusan terdahulu tetap sama. Sejalan dengan keputusan bahwa Hitam bukan SLA antrean biasa dan tidak boleh ditentukan otomatis oleh aplikasi |

### Hasil audit source untuk conflict dan gap

| ID | Status baru | Temuan |
| --- | --- | --- |
| `IGD-CONFLICT-001` | `resolved — tidak terkonfirmasi` | Tidak ditemukan satu pun implementasi backend yang mensyaratkan billing `Final` untuk clinical completion. `BillingManagement` tidak merujuk `EncounterId` sama sekali, dan modul IGD tidak memiliki rujukan billing. Satu-satunya keterkaitan billing yang ditemukan ada di `PharmacyManagement/Services/PrescriptionWorkflowService.cs` baris 68, yaitu resep harus final sebelum billing dibuat — arah kebalikannya dan bukan tentang penutupan encounter. `IGD-DEC-021` tidak bertentangan dengan source |
| `IGD-GAP-001` | `confirmed` | `TrxEmergencyVisit` memiliki `VisitCompletedAt` pada baris 67, tetapi `EmergencyVisitStatus` tidak memiliki nilai `Completed`. Status yang tersedia hanya `Arrived`, `WaitingForTriage`, `Triaged`, `InTreatment`, `UnderObservation`, `AwaitingDisposition`, `Disposed`, dan `Cancelled`. Model state memang belum memisahkan disposition dari clinical completion, persis seperti yang diduga gap ini |

| ID | Jenis | Isi | Owner | Status | Bukti | Catatan |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-DEC-049` | Decision | `EmergencyVisitStatus` mendapat nilai baru `Completed` yang ditempatkan setelah `Disposed`. `Disposed` berarti dokter telah menetapkan tindak lanjut; `Completed` berarti seluruh kewajiban klinis dan transfer yang relevan telah tuntas dan `VisitCompletedAt` terisi. Transisi `Disposed` ke `Completed` hanya sah bila seluruh closure gate klinis dan transfer terpenuhi | Product/domain owner, dengan clinical governance owner sebagai approver akhir saat ditunjuk | `approved` | Jawaban pemilik suite skill 14 Agustus 2026; `IGD-GAP-001` terkonfirmasi di source | Menutup `IGD-GAP-001`. Tidak membuka keputusan baru karena `IGD-DEC-021` sudah menyatakan encounter dapat `Completed` setelah disposition. Konsekuensi implementasi: penambahan nilai enum berarti perubahan model dan migration, ditambah aturan transisi baru pada state-transition matrix |

### Penutupan Closure Pass 2026-08-14

| Blocker | Status akhir | Ditutup oleh |
| --- | --- | --- |
| B1 — Kepemilikan keputusan | `resolved` | `IGD-DEC-046` |
| B2 — Skema kategori triase | `resolved` | `IGD-DEC-047` dan `IGD-DEC-048` |
| B3 — Penutupan klinis versus billing | `resolved` | Audit source: `IGD-CONFLICT-001` tidak terkonfirmasi; `IGD-GAP-001` ditutup `IGD-DEC-049` |
| B4 — Capability gap authorization, SLA, dan authority mapping | **masih terbuka** | Bukan pertanyaan wawancara. Diteruskan ke `/trace-existing-capabilities` mode impact scan untuk `IGD-GAP-006`, `IGD-GAP-007`, dan `IGD-GAP-008` |

Catatan penting: decision log ini berubah pada pass tersebut, sehingga `input_hashes` pada
`blueprint-manifest.md` menjadi stale dan wajib diperbarui saat blueprint dinaikkan ke
revision 4.

### Tambahan pasca impact scan 2026-08-14

| ID | Jenis | Isi | Owner | Status | Bukti | Catatan |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-FACT-009` | Fact | Endpoint bertanda `IsSystemOnly` sengaja dikecualikan dari pencarian policy pada `AccessPermissionService`, sehingga saat ini hanya dapat diakses melalui bypass SuperAdmin. Contoh pemakaian ada pada `HumanResourceContextController` | — | `approved` | backend + `Services/Security/AccessPermissionService.cs` baris 65 dan 70; `Attributes/AccessControllerAttribute.cs` baris 30; `Areas/SelfServices/HumanResource/Controllers/HumanResourceContextController.cs` baris 23 dan 44 + `e5331a0` | Bypass SuperAdmin memiliki fungsi teknis yang sah dan sedang dipakai |
| `IGD-FACT-010` | Fact | Tidak ada mekanisme break-glass atau emergency access pada kode aplikasi | — | `approved` | Pencarian pada seluruh source backend + `e5331a0` | Akses darurat klinis belum punya jalur resmi |
| `IGD-DEC-050` | Decision | Kewenangan SuperAdmin dipisahkan menurut jenis endpoint. Untuk endpoint bertanda `IsSystemOnly`, SuperAdmin tetap berwenang penuh karena itu wilayah teknis. Untuk endpoint klinis dan bisnis, SuperAdmin wajib melewati pemeriksaan policy seperti pengguna lain dan tidak lagi memperoleh bypass. Akses darurat klinis ditangani mekanisme break-glass tersendiri yang tercatat, berbatas waktu, dan dapat ditinjau | Product/domain owner sebagai baseline desain; security/privacy owner sebagai approver akhir saat ditunjuk | `approved` | Jawaban pemilik suite skill 14 Agustus 2026 | Menutup `IGD-CONFLICT-003` tanpa memutus jalur teknis yang sedang berjalan. Mekanisme break-glass belum ada di kode sehingga menjadi kebutuhan desain baru. Persetujuan security/privacy owner menjadi syarat go-live sesuai `IGD-DEC-046` |

### Status gerbang `design-business-module` setelah pass ini

| Blocker | Status |
| --- | --- |
| B1 Kepemilikan keputusan | `resolved` — `IGD-DEC-046` |
| B2 Skema kategori triase | `resolved` — `IGD-DEC-047`, `IGD-DEC-048` |
| B3 Penutupan klinis versus billing | `resolved` — audit source dan `IGD-DEC-049` |
| B4 Capability gap | `resolved` — impact scan 2026-08-14 pada capability map |
| `IGD-CONFLICT-003` | `resolved` — `IGD-DEC-050` |

Tidak ada blocker tersisa yang menghalangi penulisan blueprint revision 4.

## Approval 2026-08-14

Pemegang Product/Domain Owner sementara sesuai `IGD-DEC-046` menyatakan persetujuan atas
seluruh butir yang diajukan pada 14 Agustus 2026. Persetujuan dicatat menurut batas kewenangan
yang ditetapkan `IGD-DEC-046` sendiri.

### Yang menjadi `approved` oleh persetujuan ini

| Butir | Keputusan terkait | Akibat |
| --- | --- | --- |
| Scope, workflow, dan urutan prioritas modul IGD | `IGD-DEC-046` | Blueprint revision 4 menjadi baseline delivery |
| Status `Completed` pada kunjungan IGD | `IGD-DEC-049` | Nilai enum baru dan endpoint penyelesaian boleh dibangun |
| Penanda pelampauan target respons dan hosted service pemantaunya | `IGD-GAP-007`, `IGD-DEC-027` | Kolom breach, index, worker, dan endpoint daftar breach boleh dibangun |
| Kontrak API, state, validasi, integrasi, dan permission | seluruhnya | Naik dari `0.2.0-draft` menjadi `0.2.0` dan hash-nya dikunci di manifest |

### Yang **tidak** berubah statusnya walaupun disetujui

Ketiga butir berikut berada di luar kewenangan Product/Domain Owner menurut `IGD-DEC-046`.
Pernyataan setuju dicatat sebagai dukungan pemilik proses, bukan sebagai pengesahan.

| Butir | Menunggu | Status tetap |
| --- | --- | --- |
| Nilai target waktu triase level 2 sampai 5 | SOP triase MMC | `TargetUnconfigured`; angka tidak boleh ditebak |
| Pemisahan kewenangan SuperAdmin dan mekanisme break-glass | Security/privacy owner | Boleh dirancang dan dibangun, tidak boleh diaktifkan di produksi |
| Skema kategori triase sebagai aturan klinis | Clinical governance owner | Tetap baseline regulasi Permenkes 47/2018, bukan persetujuan klinis |

### Catatan yang harus diisi sebelum dipakai sebagai bukti formal

Nama dan jabatan pemegang Product/Domain Owner belum tertulis. Sampai diisi, baris
`approved_by` pada manifest bernilai peran, bukan orang, sehingga belum memenuhi syarat
`GovernanceAssignment` pada `IGD-DEC-036` dan `IGD-DEC-045`.

---

## Closure Pass terbatas 2026-08-19 — amendment revision 4

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Revision dasar | `4` |
| Status pass | `draft` — wawancara berjalan; belum merupakan approval amendment |
| Backend SHA saat pass dimulai | `a468a4506a03ad5795b4b581bcb72c582936d2d0` |
| Frontend SHA saat pass dimulai | `db9fb86735207b3db25ec3ed82fe5e9a5e5823d9` |
| Contract dasar | API, State, Validation, Integration, dan Permission/Audit `0.2.0` |

### Scope yang dikonfirmasi

Pass ini hanya menutup empat topik yang menghalangi kelanjutan roadmap backend:

1. representasi kategori triase Hitam;
2. relasi pengguna dengan unit pelayanan;
3. mekanisme dan owner akses darurat *break-glass*;
4. approval serta urutan `backend-roadmap.md`.

Di luar scope pass ini adalah implementasi source dan migration, desain UI frontend, nilai
target SLA level 2 sampai 5, serta coverage gap `CG-01` sampai `CG-07` selain titik yang
bersinggungan langsung dengan empat topik di atas.

### Decision log pass berjalan

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-DEC-051` | Decision | Closure Pass tetap dibatasi pada empat topik: kategori Hitam, relasi pengguna–unit, break-glass, dan approval/urutan roadmap backend. Coverage gap lain dibahas pada pass terpisah | Product/Domain Owner | `draft` — pilihan pengguna sudah jelas, tetapi identitas dan kewenangan formal pemberi jawaban belum dicatat | Jawaban pengguna 19 Agustus 2026; authority unverified | Pilihan A pada konfirmasi scope Closure Pass |
| `IGD-OQ-035` | Open Question | Bagaimana kategori Hitam direpresentasikan tanpa menjadikannya level antrean biasa? | Product/Domain Owner + Clinical Governance | `superseded` oleh `IGD-DEC-052` | Jawaban pengguna 19 Agustus 2026; authority unverified | `IGD-DEC-047`, `IGD-DEC-048`; blocker pada laporan `BE-IGD-003` |
| `IGD-OQ-036` | Open Question | Apa sumber kebenaran relasi pengguna dengan unit pelayanan dan masa berlakunya? | Product/Domain Owner + Security/Privacy + Workforce/Organization owner | `superseded` oleh `IGD-DEC-053` | Jawaban pengguna 19 Agustus 2026; authority unverified | `IGD-DEC-026`, `IGD-GAP-006` |
| `IGD-OQ-037` | Open Question | Siapa yang boleh menerbitkan, memakai, meninjau, dan mencabut break-glass, serta berapa batas waktunya? | Security/Privacy + Clinical Governance | `draft` — memblokir desain `BE-IGD-011` dan aktivasi `BE-IGD-012` | — | `IGD-DEC-034`, `IGD-DEC-050` |
| `IGD-OQ-038` | Open Question | Siapa approver bernama untuk roadmap backend dan apakah urutan `BE-IGD-001` sampai `BE-IGD-014` diterima? | Product/Domain Owner | `draft` — memblokir perubahan roadmap dari `DRAFT` menjadi approved | — | `IGD-DEC-046`; `backend-roadmap.md` revision 1 |
| `IGD-DEC-052` | Decision | Hitam adalah kategori klinis tersendiri di luar `Level` 1 sampai 5. Hanya tenaga klinis berwenang yang boleh menetapkannya. Setiap penetapan wajib memuat alasan, waktu dari server, dan jejak audit yang tidak dapat diubah. Kategori ini memerlukan desain dan penyimpanan khusus; tidak memperoleh `ResponseDueAt`, tidak mengikuti SLA antrean biasa, dan tidak boleh ditetapkan otomatis oleh aplikasi | Product/Domain Owner, dengan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban dan approval Clinical Governance belum tercatat | Jawaban pengguna 19 Agustus 2026; authority unverified | Pilihan A; memperinci `IGD-DEC-048` tanpa mengubah skala triase level 1 sampai 5 |
| `IGD-DEC-053` | Decision | Sumber kebenaran relasi pengguna dengan unit pelayanan adalah penugasan tersendiri yang memiliki tanggal mulai dan tanggal berakhir. Workforce/Organization mengelola penugasan tersebut dan authorization memakainya sebagai batas unit pelayanan. Penugasan unit tidak memberikan capability klinis atau bisnis dengan sendirinya; tindakan tetap memerlukan capability yang sesuai menurut `IGD-DEC-026` | Product/Domain Owner + Workforce/Organization owner, dengan Security/Privacy sebagai approver authorization | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban serta approval Workforce/Organization dan Security/Privacy belum tercatat | Jawaban pengguna 19 Agustus 2026; authority unverified | Pilihan A; menutup sumber kebenaran pada `IGD-OQ-036` dan melengkapi kekurangan scope unit pada `IGD-GAP-006` |

### Acceptance criteria yang dapat diuji dari `IGD-DEC-052`

1. Sistem tidak dapat menyimpan kategori Hitam melalui nilai `Level` 1 sampai 5.
2. Permintaan dari pengguna yang bukan tenaga klinis berwenang ditolak dan tidak mengubah kategori pasien.
3. Penetapan kategori Hitam tanpa alasan ditolak.
4. Waktu penetapan berasal dari server, bukan waktu yang dikirim pengguna.
5. Setiap penetapan menyimpan pelaku, alasan, waktu server, dan riwayat audit yang tidak dapat ditimpa.
6. Pasien berkategori Hitam tidak menerima `ResponseDueAt`, tidak dihitung sebagai pelampauan SLA antrean biasa, dan tidak pernah ditetapkan Hitam secara otomatis.

**Contoh:** tenaga klinis berwenang menetapkan kategori Hitam dengan alasan klinis yang
tercatat. Sistem menyimpan waktu server dan pelakunya, mempertahankan riwayat tersebut untuk
audit, serta tidak memasukkan pasien ke perhitungan keterlambatan respons level 1 sampai 5.

### Acceptance criteria yang dapat diuji dari `IGD-DEC-053`

1. Setiap penugasan menghubungkan satu pengguna dengan satu unit pelayanan serta memiliki
   tanggal mulai dan tanggal berakhir.
2. Authorization hanya menganggap penugasan berlaku ketika waktu server berada dalam masa
   berlakunya.
3. Penugasan yang belum dimulai atau sudah berakhir tidak dapat dipakai untuk membuka akses
   unit pelayanan.
4. Pengguna yang ditugaskan pada suatu unit tetapi tidak memiliki capability tindakan tetap
   ditolak.
5. Workforce/Organization menjadi pengelola penugasan; perubahan relasi tidak bersumber
   hanya dari claim atau token sesi.

**Contoh:** seorang perawat mendapat penugasan sementara di IGD dari 19 sampai 21 Agustus
2026. Selama rentang itu, penugasan dapat memenuhi batas unit IGD, tetapi tindakan triase
tetap ditolak jika perawat tersebut tidak memiliki capability triase. Setelah tanggal akhir,
permintaan baru ke resource IGD ditolak meskipun sesi login lama masih aktif.

---

## Amendment Pass 2026-08-20 — Pengkajian Keperawatan IGD Setelah Triase

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Revision dasar | `4` |
| Status pass | `draft` — wawancara berjalan; belum merupakan approval amendment |
| Backend SHA saat pass dimulai | `2285e303c0bb1930d847d5a408b58d8633decdd2` |
| Frontend SHA saat pass dimulai | `db9fb86735207b3db25ec3ed82fe5e9a5e5823d9` |
| Contract dasar | API, State, Validation, Integration, dan Permission/Audit `0.2.0` |

### Scope yang dikonfirmasi

**Batas scope:** pass ini menetapkan proses pengkajian keperawatan lengkap yang dilakukan
setelah triase pada encounter IGD yang sama.

Di dalam scope:

1. primary survey ABCDE;
2. secondary survey;
3. tanda vital dan penilaian nyeri;
4. alergi dan riwayat kesehatan yang relevan;
5. penilaian risiko keperawatan;
6. intervensi awal oleh perawat sesuai kewenangan;
7. evaluasi atau pengkajian ulang; dan
8. serah terima hasil pengkajian.

Di luar scope pass ini adalah pengkajian medis dokter, penetapan diagnosis medis, order
dokter, proses billing, bentuk menu atau layout frontend, implementasi source, migration,
serta perubahan terhadap keputusan triase yang sudah ada. Modul lain hanya dibahas pada
titik serah-terima atau data bersama yang langsung dibutuhkan pengkajian keperawatan IGD.

### Decision log pass berjalan

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-DEC-054` | Decision | Pengkajian keperawatan IGD lengkap menjadi proses tersendiri setelah triase. Cakupannya meliputi primary survey ABCDE, secondary survey, tanda vital, nyeri, alergi, riwayat kesehatan yang relevan, risiko keperawatan, intervensi awal sesuai kewenangan, evaluasi atau pengkajian ulang, dan serah terima. Pengkajian tetap terhubung pada encounter IGD yang sama dan selesainya triase tidak berarti pengkajian keperawatan lengkap sudah selesai | Product/Domain Owner, dengan Clinical Governance dan Nursing authority sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban serta approval klinis dan keperawatan belum tercatat | — | Jawaban pengguna 20 Agustus 2026; pilihan A pada konfirmasi scope |
| `IGD-OQ-039` | Open Question | Kapan pengkajian keperawatan lengkap wajib dimulai setelah triase, khususnya untuk pasien Merah yang memerlukan tindakan penyelamatan segera? | Product/Domain Owner + Clinical Governance + Nursing authority | `superseded` oleh `IGD-DEC-055` | Jawaban pengguna 20 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-054` |
| `IGD-DEC-055` | Decision | Waktu mulai dan kelengkapan pengkajian mengikuti kegawatan. Pasien Merah langsung mendapat tindakan penyelamatan; pemeriksaan klinis serta dokumentasi minimum berjalan bersama resusitasi sejauh aman, sedangkan bagian lengkap diselesaikan setelah pasien stabil. Pasien selain Merah langsung menjalani pengkajian keperawatan setelah triase. Dokumentasi tidak boleh menunda tindakan penyelamatan | Product/Domain Owner, dengan Clinical Governance dan Nursing authority sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban serta approval klinis dan keperawatan belum tercatat | — | Jawaban pengguna 20 Agustus 2026; pilihan B untuk `IGD-OQ-039` |
| `IGD-OQ-040` | Open Question | Bagian pengkajian apa yang wajib terisi agar pengkajian boleh dinyatakan selesai, dan bagaimana mencatat bagian yang tidak dapat dinilai karena kondisi pasien? | Product/Domain Owner + Clinical Governance + Nursing authority | `superseded` oleh `IGD-DEC-056` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-054` dan `IGD-DEC-055` |
| `IGD-DEC-056` | Decision | Syarat kelengkapan pengkajian menyesuaikan kondisi pasien. Data inti tetap wajib. Bagian yang secara klinis tidak relevan boleh ditandai tidak berlaku, sedangkan bagian yang relevan tetapi tidak dapat dinilai wajib disertai alasan. Sistem harus membedakan nilai normal, tidak berlaku, belum dinilai, dan tidak dapat dinilai; sistem tidak boleh mengisi nilai klinis secara otomatis untuk sekadar meloloskan validasi. Aturan ini berlaku antara lain untuk pasien tidak sadar, pasien anak, dan pasien tanpa keluarga atau sumber informasi | Product/Domain Owner, dengan Clinical Governance dan Nursing authority sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban serta approval klinis dan keperawatan belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-040` |
| `IGD-OQ-041` | Open Question | Data apa saja yang menjadi data inti minimum dan harus tersedia sebelum pengkajian boleh dinyatakan selesai? | Product/Domain Owner + Clinical Governance + Nursing authority | `superseded` oleh `IGD-DEC-057` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-056` |
| `IGD-DEC-057` | Decision | Data inti minimum pengkajian terdiri atas referensi encounter dan pasien definitif atau pasien sementara, identitas perawat pengkaji dan waktu server, sumber informasi, hasil primary survey ABCDE, tanda vital atau alasan tidak dapat diukur, hasil penilaian nyeri, status alergi, risiko utama, intervensi awal, evaluasi, dan serah terima. Seluruh unsur harus memiliki nilai yang sah atau status pengecualian yang diperbolehkan `IGD-DEC-056` sebelum pengkajian dinyatakan selesai. Identitas pasien sementara tidak boleh menghalangi pengkajian | Product/Domain Owner, dengan Clinical Governance dan Nursing authority sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban serta approval klinis dan keperawatan belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-041` |
| `IGD-OQ-042` | Open Question | Siapa yang boleh membuat, menyelesaikan, mengoreksi, dan membatalkan pengkajian keperawatan IGD? | Product/Domain Owner + Nursing authority + Security/Privacy | `superseded` oleh `IGD-DEC-058` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-057` |
| `IGD-DEC-058` | Decision | Perawat hanya boleh membuat dan menyelesaikan pengkajian bila memiliki penugasan IGD yang aktif dan capability pengkajian yang sesuai. Koreksi dilakukan secara append-only: nilai asli tetap tersimpan, sedangkan koreksi wajib mencatat pelaku, waktu server, dan alasan. Pembatalan hanya diperbolehkan untuk pengkajian duplikat atau keliru oleh perawat yang berwenang atau supervisor sesuai capability. Catatan yang sudah selesai tidak boleh dihapus. Penugasan unit saja tidak memberikan capability klinis dan capability saja tidak melewati batas penugasan unit | Product/Domain Owner, dengan Nursing authority dan Security/Privacy sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban serta approval keperawatan dan keamanan belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-042`; konsisten dengan `IGD-DEC-053` |
| `IGD-OQ-043` | Open Question | Bagaimana memperlakukan pengkajian berstatus selesai yang kemudian terbukti duplikat dibandingkan pengkajian yang isi klinisnya salah? | Product/Domain Owner + Nursing authority + Clinical Governance | `superseded` oleh `IGD-DEC-059` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-058` |
| `IGD-DEC-059` | Decision | Pengkajian duplikat dan pengkajian dengan kesalahan isi klinis memakai jalur berbeda. Catatan duplikat ditandai `Void` atau batal, tetap dipertahankan, dan wajib menunjuk pengkajian sah yang menggantikannya. Kesalahan isi klinis diperbaiki melalui amendment append-only; nilai asli tetap tersedia dan nilai hasil amendment menjadi nilai efektif terbaru. Seluruh referensi downstream harus tetap dapat ditelusuri ke catatan asal, status void, catatan sah, dan amendment terkait; tidak ada hard delete atau pemutusan referensi | Product/Domain Owner, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban serta approval klinis dan keperawatan belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-043` |
| `IGD-OQ-044` | Open Question | Kapan evaluasi atau pengkajian ulang wajib dilakukan setelah pengkajian awal? | Product/Domain Owner + Nursing authority + Clinical Governance | `superseded` oleh `IGD-DEC-060` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut cakupan evaluasi pada `IGD-DEC-054` |
| `IGD-DEC-060` | Decision | Evaluasi atau pengkajian ulang wajib mengikuti tingkat kegawatan dan kejadian klinis. Pemicu minimum adalah setelah intervensi, ketika kondisi berubah, pada perpindahan atau serah terima, dan sebelum disposition. Pengkajian ulang juga mengikuti interval SOP per tingkat kegawatan. Nilai interval tidak boleh ditebak atau di-hardcode; nilai tersebut harus dapat dikonfigurasi dari SOP yang disahkan. Interval yang belum dikonfigurasi harus ditandai sebagai belum tersedia dan tidak boleh dianggap patuh atau terlambat secara otomatis. Keterlambatan dokumentasi tidak boleh memblokir tindakan klinis yang dibutuhkan | Product/Domain Owner, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban, approval klinis/keperawatan, dan SOP interval belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-044` |
| `IGD-OQ-045` | Open Question | Kapan serah terima pengkajian dianggap selesai dan apakah penerima wajib memberikan pengakuan? | Product/Domain Owner + Nursing authority + Clinical Governance | `superseded` oleh `IGD-DEC-061` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-057` dan `IGD-DEC-060` |
| `IGD-DEC-061` | Decision | Serah terima pengkajian selesai hanya setelah penerima yang berwenang meninjau dan menyatakan menerima. Pengirim mengirim ringkasan dan status menjadi `Pending`; penerima dapat mengubahnya menjadi `Accepted` atau `Rejected`. Penolakan wajib menyimpan alasan, pelaku, dan waktu server. Serah terima yang belum diakui tetap berstatus tertunda dan mengikuti eskalasi yang dapat diaudit. Status tertunda atau penolakan tidak boleh menghentikan pelayanan klinis darurat | Product/Domain Owner, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban, approval klinis/keperawatan, dan aturan waktu eskalasi belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-045` |
| `IGD-OQ-046` | Open Question | Siapa yang memegang tanggung jawab klinis selama serah terima masih `Pending` atau `Rejected`? | Product/Domain Owner + Nursing authority + Clinical Governance | `superseded` oleh `IGD-DEC-062` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-061` |
| `IGD-DEC-062` | Decision | Tanggung jawab klinis mengikuti keberadaan fisik pasien. Status serah terima `Pending` atau `Rejected` tidak boleh menyebabkan pasien kehilangan owner klinis. Selama pasien masih berada di unit asal, tanggung jawab klinis tetap pada unit atau perawat pengirim. Setelah pasien berada di unit penerima, pelayanan klinis aktif menjadi tanggung jawab unit penerima. Unit atau perawat pengirim tetap bertanggung jawab melengkapi dan mengoreksi dokumentasi serah terima sampai handover berstatus `Accepted`; sebelum itu handover tetap tercatat sebagai outstanding | Product/Domain Owner, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pengguna menyatakan pilihan B disetujui, tetapi identitas/kewenangan formal pemberi persetujuan serta approval keperawatan dan clinical governance belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan B untuk `IGD-OQ-046`; melengkapi `IGD-DEC-061` |
| `IGD-OQ-047` | Open Question | Bagaimana batas waktu dan jalur eskalasi handover `Pending` atau `Rejected` tanpa menghambat pelayanan klinis? | Product/Domain Owner + Nursing authority + Clinical Governance | `superseded` oleh `IGD-DEC-063` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-061` dan `IGD-DEC-062` |
| `IGD-DEC-063` | Decision | Handover `Pending` mengikuti batas waktu berdasarkan tingkat kegawatan. Handover `Rejected` langsung menghasilkan notifikasi kepada pengirim. Jika handover belum diselesaikan, sistem melakukan eskalasi bertahap kepada penanggung jawab shift atau unit. Nilai batas waktu dan urutan eskalasi wajib berasal dari konfigurasi SOP yang disahkan dan tidak boleh di-hardcode. Reminder atau eskalasi tidak boleh otomatis mengubah handover menjadi `Accepted` dan tidak boleh menunda atau menghambat pelayanan klinis | Product/Domain Owner, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban, approval keperawatan/clinical governance, dan SOP waktu eskalasi belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan B untuk `IGD-OQ-047`; melengkapi `IGD-DEC-061` dan `IGD-DEC-062` |
| `IGD-OQ-048` | Open Question | Bukti sistem apa yang menjadi sumber kebenaran bahwa pasien telah tiba secara fisik di unit penerima sehingga owner pelayanan klinis berpindah? | Product/Domain Owner + Nursing authority + Clinical Governance | `superseded` oleh `IGD-DEC-064` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-062`; harus tetap terpisah dari status penerimaan dokumentasi handover |
| `IGD-DEC-064` | Decision | Petugas unit penerima yang berwenang mencatat event `Arrived` sebagai bukti sistem bahwa pasien telah tiba secara fisik. Event ini wajib mencatat pelaku, waktu server, dan waktu kedatangan aktual apabila pencatatannya terlambat. `Arrived` memindahkan owner pelayanan klinis aktif ke unit penerima, tetapi tidak otomatis mengubah handover dokumentasi menjadi `Accepted` | Product/Domain Owner, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban serta approval keperawatan dan clinical governance belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-048`; memperinci trigger pada `IGD-DEC-062` |
| `IGD-OQ-049` | Open Question | Bagaimana menjaga ownership dan audit ketika pasien sudah tiba tetapi event `Arrived` belum dapat dicatat karena downtime atau kondisi darurat? | Product/Domain Owner + Nursing authority + Clinical Governance + Integration/Operations owner | `superseded` oleh `IGD-DEC-065` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-062` dan `IGD-DEC-064` |
| `IGD-DEC-065` | Decision | Jika event `Arrived` tidak dapat dicatat karena downtime atau kondisi darurat, petugas memakai catatan downtime/manual dan melakukan pencatatan susulan setelah sistem tersedia. Tanggung jawab klinis berpindah ke unit penerima sejak waktu kedatangan aktual, bukan sejak waktu entri susulan. Entri susulan wajib menyimpan waktu kedatangan aktual, waktu server saat pencatatan, pelaku, alasan keterlambatan, dan referensi catatan downtime/manual | Product/Domain Owner, dengan Nursing authority, Clinical Governance, dan Integration/Operations owner sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban, approval owner terkait, dan SOP downtime belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-049`; melengkapi failure behavior `IGD-DEC-064` |
| `IGD-OQ-050` | Open Question | Bagaimana memperbaiki event `Arrived` yang salah pasien, salah unit, atau salah waktu tanpa menghilangkan histori perpindahan ownership? | Product/Domain Owner + Nursing authority + Clinical Governance + Security/Privacy | `superseded` oleh `IGD-DEC-066` | Jawaban pengguna 21 Agustus 2026; authority unverified | Tindak lanjut `IGD-DEC-062`, `IGD-DEC-064`, dan `IGD-DEC-065` |
| `IGD-DEC-066` | Decision | Koreksi event `Arrived` bersifat append-only dan mengikuti tingkat dampak. Kesalahan waktu dapat diperbaiki melalui amendment oleh petugas berwenang dengan alasan. Kesalahan pasien atau unit memerlukan reversal dan persetujuan petugas kedua atau supervisor; pembuat dan pemberi persetujuan harus berbeda. Event asli tidak dihapus, tetapi ditandai tidak berlaku dan dihubungkan ke amendment atau event pengganti yang sah | Product/Domain Owner, dengan Nursing authority, Clinical Governance, dan Security/Privacy sebagai approver akhir | `draft` — pilihan pengguna sudah jelas, tetapi identitas/kewenangan formal pemberi jawaban dan approval owner terkait belum tercatat | — | Jawaban pengguna 21 Agustus 2026; pilihan A untuk `IGD-OQ-050`; melengkapi tata kelola koreksi `IGD-DEC-064` dan `IGD-DEC-065` |
| `IGD-OQ-051` | Open Question | Apa yang wajib dilakukan terhadap unit, petugas, dan catatan downstream yang telah memakai event `Arrived` sebelum event tersebut dikoreksi atau dibalik? | Product/Domain Owner + Nursing authority + Clinical Governance + Integration owner | `draft` — memblokir aturan propagasi koreksi, notifikasi, dan penanganan dampak downstream | — | Tindak lanjut `IGD-DEC-066`; koreksi tidak boleh dilakukan diam-diam |

### Acceptance criteria awal dari `IGD-DEC-054`

1. Sistem membedakan status selesai triase dari status selesai pengkajian keperawatan
   lengkap.
2. Pengkajian keperawatan menggunakan encounter IGD yang sama dengan triase dan tidak
   membuat kunjungan kedua.
3. Pengkajian menyediakan cakupan minimum yang telah dipilih, tetapi field wajib, aturan
   pengosongan, dan kewenangan final menunggu keputusan lanjutan dalam pass ini.
4. Pengkajian medis dokter dan diagnosis medis tidak dianggap sebagai bagian yang harus
   diisi perawat.
5. Bentuk menu, halaman, tab, atau layout belum diputuskan dan tetap mengikuti hierarchy
   kewenangan UI saat desain dilakukan.
6. Pada pasien Merah, sistem tidak mewajibkan penyelesaian seluruh dokumentasi sebelum
   tindakan penyelamatan dimulai.
7. Pada pasien Merah, sistem dapat membedakan dokumentasi minimum saat resusitasi dari
   pengkajian lengkap setelah pasien stabil.
8. Pada pasien selain Merah, pengkajian keperawatan dimulai setelah triase tanpa menunggu
   instruksi dokter sebagai pemicu umum.
9. Sistem tidak boleh menganggap tertundanya dokumentasi lengkap selama resusitasi sebagai
   alasan untuk menolak atau menghentikan tindakan klinis.
10. Validasi kelengkapan menyesuaikan usia, kesadaran, kondisi klinis, dan ketersediaan
    sumber informasi pasien.
11. Bagian yang tidak relevan dan bagian yang relevan tetapi tidak dapat dinilai memiliki
    makna berbeda dan tidak boleh disimpan sebagai keadaan yang sama.
12. Bagian yang relevan tetapi tidak dapat dinilai wajib menyimpan alasan yang dapat dibaca
    oleh petugas berikutnya dan auditor klinis.
13. Sistem tidak boleh menganggap field kosong sebagai `Normal`, `Tidak Ada`, atau hasil
    klinis lain tanpa pilihan eksplisit dari perawat.
14. Pengkajian tidak dapat berstatus selesai bila referensi encounter, identitas pengkaji,
    waktu, sumber informasi, ABCDE, tanda vital, nyeri, status alergi, risiko utama,
    intervensi, evaluasi, atau serah terima belum memiliki nilai atau pengecualian yang sah.
15. Waktu pencatatan inti berasal dari server dan identitas pengkaji berasal dari pengguna
    yang terautentikasi, bukan teks bebas yang dapat mengganti pelaku.
16. Pasien dengan identitas sementara tetap dapat menjalani pengkajian pada encounter yang
    sama; rekonsiliasi identitas kemudian tidak membuat pengkajian baru.
17. Alasan tanda vital tidak dapat diukur harus tersimpan sebagai alasan, bukan diubah
    menjadi angka nol atau nilai normal.
18. Permintaan membuat atau menyelesaikan pengkajian ditolak bila penugasan IGD tidak aktif
    atau capability pengkajian tidak dimiliki.
19. Koreksi tidak mengubah atau menghapus nilai lama; pembaca dapat mengetahui nilai
    efektif terbaru beserta riwayat koreksinya.
20. Koreksi tanpa alasan, pelaku terautentikasi, atau waktu server harus ditolak.
21. Penghapusan permanen terhadap pengkajian yang sudah selesai tidak tersedia.
22. Pembatalan hanya dapat diajukan ketika keberadaan catatannya keliru, termasuk catatan
    duplikat; pembatalan tetap menyimpan pelaku, waktu, alasan, serta rekam asal untuk audit.
23. Catatan duplikat berstatus void tidak boleh dibaca sebagai pengkajian klinis yang masih
    berlaku dan wajib menunjuk catatan yang sah.
24. Kesalahan nilai klinis tidak diselesaikan dengan void bila catatan memang mewakili
    pengkajian yang benar-benar dilakukan; perbaikannya memakai amendment.
25. Consumer downstream harus dapat menentukan nilai efektif terbaru tanpa kehilangan
    kemampuan menelusuri nilai asli dan alasan perubahan.
26. Operasi void atau amendment tidak boleh menghapus referensi yang sudah dibuat oleh
    dokumentasi klinis lain.
27. Pengkajian ulang wajib dapat ditautkan ke pemicu kejadian klinis atau jadwal SOP yang
    mendasarinya.
28. Intervensi, perubahan kondisi, perpindahan/serah terima, dan keadaan sebelum disposition
    masing-masing dapat memicu pengkajian ulang tanpa menimpa pengkajian sebelumnya.
29. Sistem tidak boleh membuat angka interval sendiri ketika konfigurasi SOP belum tersedia.
30. Interval yang belum dikonfigurasi tidak boleh menghasilkan klaim pasien patuh atau
    terlambat terhadap interval pengkajian ulang.
31. Pengkajian ulang yang terlambat tetap dapat dicatat dan tindakan klinis tetap dapat
    dilakukan; keterlambatan disimpan untuk audit dan tindak lanjut.
32. Tindakan mengirim serah terima hanya menghasilkan status `Pending` dan belum membuktikan
    bahwa penerima telah mengambil alih.
33. Hanya penerima berwenang yang dapat menghasilkan status `Accepted` atau `Rejected`.
34. Penolakan tanpa alasan, identitas penerima, atau waktu server harus ditolak.
35. Riwayat pengiriman, penerimaan, penolakan, pengiriman ulang, dan eskalasi tidak boleh
    ditimpa.
36. Serah terima yang tertunda atau ditolak tidak boleh menutup akses terhadap tindakan
    klinis darurat yang dibutuhkan pasien.
37. Selama pasien masih berada di unit asal, sistem harus menunjukkan unit atau perawat
    pengirim sebagai owner pelayanan klinis aktif.
38. Ketika keberadaan fisik pasien tercatat sudah berpindah ke unit penerima, sistem harus
    menunjukkan unit penerima sebagai owner pelayanan klinis aktif walaupun handover masih
    `Pending` atau `Rejected`.
39. Handover `Pending` atau `Rejected` tidak boleh menghasilkan keadaan tanpa owner klinis
    maupun mengembalikan tanggung jawab klinis pasien yang sudah berada di unit penerima
    kepada unit asal.
40. Perpindahan owner klinis tidak otomatis mengubah handover menjadi `Accepted`.
41. Sampai penerima menyatakan `Accepted`, unit atau perawat pengirim tetap menjadi owner
    penyelesaian dokumentasi dan wajib dapat melengkapi atau mengoreksi ringkasan serah
    terima tanpa mengubah histori pengiriman sebelumnya.
42. Sistem harus dapat menampilkan secara terpisah owner pelayanan klinis aktif dan owner
    penyelesaian dokumentasi handover agar pengguna tidak menganggap keduanya selalu pihak
    yang sama.
43. Handover `Pending` harus memperoleh batas waktu berdasarkan tingkat kegawatan dari
    konfigurasi SOP aktif, bukan dari angka yang ditanam langsung dalam aplikasi.
44. Handover `Rejected` harus langsung mengirim notifikasi kepada pengirim tanpa menunggu
    batas waktu `Pending` berakhir.
45. Handover yang belum selesai setelah batas waktunya harus dieskalasikan secara bertahap
    kepada penanggung jawab shift atau unit sesuai konfigurasi SOP.
46. Setiap reminder dan eskalasi harus mencatat handover, penerima notifikasi, tahap
    eskalasi, waktu server, dan hasil pengiriman agar dapat diaudit.
47. Reminder atau eskalasi tidak boleh otomatis mengubah status menjadi `Accepted`, karena
    hanya penerima berwenang yang dapat menyatakan penerimaan.
48. Kegagalan pengiriman notifikasi atau handover yang melewati batas waktu tidak boleh
    memblokir pelayanan klinis, tetapi tetap harus tercatat sebagai outstanding dan dapat
    dilihat oleh pihak yang bertanggung jawab.
49. Jika konfigurasi SOP untuk tingkat kegawatan terkait belum tersedia, sistem tidak boleh
    menebak batas waktu atau menyatakan handover patuh maupun terlambat; status konfigurasi
    yang belum tersedia harus terlihat dan pelayanan klinis tetap berjalan.
50. Hanya petugas unit penerima yang berwenang yang dapat mencatat event `Arrived`.
51. Event `Arrived` harus menyimpan identitas pelaku dan waktu pencatatan dari server.
52. Jika pencatatan dilakukan setelah pasien tiba, event harus membedakan waktu kedatangan
    aktual dari waktu pencatatan server agar keterlambatan dokumentasi dapat ditelusuri.
53. Event `Arrived` harus memindahkan owner pelayanan klinis aktif ke unit penerima.
54. Event `Arrived` tidak boleh otomatis mengubah handover menjadi `Accepted`; penerimaan
    dokumentasi tetap memerlukan tindakan terpisah dari penerima berwenang.
55. Permintaan `Arrived` yang dikirim ulang tidak boleh menghasilkan perpindahan owner atau
    event kedatangan ganda.
56. Ketika sistem tidak tersedia, pelayanan dan perpindahan tanggung jawab klinis tidak
    boleh menunggu pencatatan event elektronik.
57. Catatan downtime/manual harus dapat direferensikan secara unik oleh entri susulan agar
    auditor dapat mencocokkan kejadian fisik dengan pencatatan elektronik.
58. Entri susulan harus menyimpan waktu kedatangan aktual secara terpisah dari waktu server
    saat data dimasukkan.
59. Entri susulan tanpa pelaku terautentikasi, alasan keterlambatan, atau referensi catatan
    downtime/manual harus ditolak.
60. Sistem harus merekonstruksi owner klinis berdasarkan waktu kedatangan aktual yang sah,
    tanpa menyamarkan bahwa event elektronik dicatat kemudian.
61. Pencatatan susulan yang dikirim ulang untuk referensi downtime yang sama tidak boleh
    membuat event kedatangan atau perpindahan owner ganda.
62. Kesalahan waktu kedatangan harus diperbaiki melalui amendment yang menyimpan nilai
    lama, nilai baru, pelaku, waktu server, dan alasan koreksi.
63. Kesalahan pasien atau unit tidak boleh diselesaikan dengan mengubah atau menghapus event
    asli secara langsung.
64. Reversal akibat salah pasien atau unit harus diajukan oleh petugas berwenang dan
    disetujui petugas kedua atau supervisor dengan identitas pengguna berbeda.
65. Event yang dibalik harus tetap tersedia untuk audit, ditandai tidak berlaku, dan
    menunjuk event pengganti yang sah bila pengganti diperlukan.
66. Sistem harus dapat merekonstruksi urutan event asli, permintaan koreksi, persetujuan atau
    penolakan, reversal, serta event pengganti tanpa kehilangan histori ownership.
67. Permintaan amendment atau reversal tanpa alasan, pelaku terautentikasi, dan waktu server
    harus ditolak.

**Contoh:** setelah triase pasien selesai, perawat melanjutkan pengkajian keperawatan pada
encounter yang sama. Sistem tidak boleh menandai pengkajian lengkap hanya karena kategori
triase sudah tersimpan. Perawat masih perlu mencatat bagian pengkajian yang diwajibkan dan
menyelesaikan proses sesuai keputusan klinis yang akan ditutup dalam pass ini.

**Contoh kegawatan:** pasien Merah datang dengan gangguan jalan napas. Perawat langsung
membantu resusitasi dan mencatat data minimum yang aman dicatat saat tindakan berlangsung.
Riwayat kesehatan lengkap tidak boleh menjadi prasyarat untuk membuka atau mencatat
tindakan tersebut. Setelah kondisi pasien stabil, perawat melengkapi bagian pengkajian yang
tertunda pada encounter yang sama.

**Contoh kelengkapan kontekstual:** pasien dewasa datang tidak sadar tanpa keluarga. Bagian
riwayat alergi tetap relevan, tetapi belum dapat diperoleh dari pasien atau keluarga. Perawat
tidak boleh memilih `Tidak Ada Alergi` hanya agar formulir selesai. Perawat mencatat bahwa
alergi tidak dapat dinilai beserta alasannya. Sebaliknya, pertanyaan perkembangan khusus
anak dapat ditandai tidak berlaku karena pasien adalah orang dewasa.

**Contoh data inti:** pasien belum teridentifikasi definitif dan memakai identitas sementara.
Perawat tetap mencatat ABCDE, tanda vital, nyeri, status alergi, risiko, intervensi, evaluasi,
dan serah terima pada encounter tersebut. Ketika identitas pasien kemudian ditemukan,
sistem mempertahankan pengkajian yang sama dan hanya merekonsiliasi referensi pasien sesuai
workflow identitas yang berwenang.

**Contoh kewenangan:** seorang perawat memiliki penugasan aktif di IGD tetapi belum memiliki
capability pengkajian. Sistem menolak penyelesaian pengkajian. Setelah capability diberikan
melalui proses yang berwenang, perawat dapat menyelesaikannya. Jika kemudian ada kesalahan
isi, perawat membuat koreksi beralasan; sistem mempertahankan nilai sebelumnya dan tidak
menghapus catatan yang sudah selesai.

**Contoh void dan amendment:** dua pengkajian tidak sengaja dibuat untuk pemeriksaan yang
sama. Perawat berwenang menandai salah satunya void dan menghubungkannya ke pengkajian yang
sah. Bila pada pengkajian sah tekanan darah tertulis `120/80` padahal catatan sumber yang
benar adalah `170/100`, perawat tidak membatalkan seluruh pengkajian. Perawat membuat
amendment beralasan sehingga `170/100` menjadi nilai efektif, sementara `120/80` tetap dapat
ditelusuri sebagai nilai asli.

**Contoh pengkajian ulang:** setelah pemberian intervensi awal, perawat mencatat respons
pasien sebagai pengkajian ulang baru. Ketika pasien kemudian akan dipindahkan, perawat
melakukan pengkajian ulang sebelum serah terima. Jika SOP interval per tingkat kegawatan
belum tersedia di konfigurasi, sistem tidak menebak angka menit dan tidak memberi label
patuh atau terlambat, tetapi pemicu berbasis kejadian klinis tetap berlaku.

**Contoh serah terima:** perawat IGD mengirim ringkasan kepada perawat unit penerima. Status
menjadi `Pending`, bukan langsung selesai. Jika penerima menemukan informasi penting belum
jelas, penerima menolak dengan alasan dan status menjadi `Rejected`. Pengirim memperbaiki
atau melengkapi ringkasan lalu mengirim ulang. Seluruh kejadian tetap tersimpan, sementara
pelayanan yang diperlukan pasien terus berjalan.

**Contoh ownership selama handover:** ketika pasien masih berada di IGD dan handover ke
unit rawat inap berstatus `Pending`, IGD tetap menjadi owner pelayanan klinis. Setelah pasien
secara fisik tiba di unit rawat inap, unit tersebut menjadi owner pelayanan klinis aktif
meskipun handover masih `Rejected` karena ringkasannya belum lengkap. Perawat IGD tetap wajib
memperbaiki ringkasan sampai penerima menyatakan `Accepted`, tetapi kekurangan dokumentasi
tersebut tidak membuat pasien kehilangan owner klinis di unit rawat inap.

**Contoh eskalasi:** handover pasien dengan tingkat kegawatan tertentu masih `Pending`.
Sistem memakai batas waktu dari konfigurasi SOP aktif dan mengirim reminder ketika batas
tersebut tercapai. Jika penerima menolak ringkasan, sistem langsung memberi tahu pengirim.
Apabila pengirim belum menyelesaikannya, sistem meneruskan eskalasi kepada penanggung jawab
shift lalu unit sesuai urutan SOP. Eskalasi tidak mengubah status menjadi `Accepted` dan
tidak menghentikan pelayanan pasien.

**Contoh event kedatangan:** pasien tiba di unit rawat inap pada pukul 14.05 dan petugas
penerima berwenang mencatat `Arrived`. Sistem menyimpan identitas petugas dan waktu server,
lalu memindahkan owner pelayanan klinis aktif ke unit rawat inap. Jika ringkasan handover
masih belum lengkap, status dokumentasinya tetap `Pending` atau `Rejected`, bukan otomatis
`Accepted`.

**Contoh downtime:** pasien tiba di unit rawat inap pukul 14.05 ketika sistem tidak dapat
digunakan. Petugas penerima mencatat kedatangan pada formulir downtime dan langsung menjadi
owner pelayanan klinis karena pasien sudah berada di unit tersebut. Sistem kembali tersedia
pukul 14.40. Petugas membuat entri susulan yang menyimpan pukul 14.05 sebagai waktu
kedatangan aktual, pukul 14.40 sebagai waktu pencatatan server, identitas pelaku, alasan
keterlambatan, dan referensi formulir downtime. Sistem tidak boleh menyamarkan seolah-olah
entri elektronik dibuat pukul 14.05.

**Contoh koreksi:** petugas mencatat pasien tiba pukul 14.50, tetapi bukti kedatangan yang
sah menunjukkan pukul 14.05. Petugas berwenang membuat amendment beralasan; sistem tetap
menyimpan kedua nilai dan memakai nilai hasil amendment sebagai waktu efektif. Jika event
ternyata dicatat untuk pasien atau unit yang salah, petugas mengajukan reversal dan petugas
kedua atau supervisor menyetujuinya. Event asli tetap terlihat sebagai tidak berlaku dan
ditautkan ke event pengganti yang benar.

### Blocker pass berjalan

Urutan tindakan dan dokumentasi, kelengkapan, data inti, kewenangan, koreksi, void,
pengkajian ulang, pengakuan serah terima, serta pembagian ownership klinis dan dokumentasi
telah dipilih pada `IGD-DEC-055` sampai `IGD-DEC-062`. Keputusan terakhir tetap menunggu
pencatatan identitas dan kewenangan formal pemberi persetujuan serta approval Nursing
authority dan Clinical Governance. Aturan batas waktu dan jalur eskalasi untuk handover
`Pending` atau `Rejected` telah dipilih pada `IGD-DEC-063`, tetapi nilai waktunya masih
menunggu SOP yang disahkan. Event `Arrived` sebagai sumber kebenaran sistem untuk perpindahan
owner klinis telah dipilih pada `IGD-DEC-064`. Failure behavior ketika event tersebut belum
dapat dicatat akibat downtime atau kondisi darurat telah dipilih pada `IGD-DEC-065`, tetapi
SOP downtime masih menunggu bukti dan approval. Mekanisme koreksi atau reversal event
`Arrived` yang salah telah dipilih pada `IGD-DEC-066`. Propagasi koreksi kepada unit,
petugas, dan catatan downstream yang telah memakai event lama belum diputuskan.

---

## Amendment Pass 2026-08-24 — Kaji Ulang Menyeluruh IGD: Pendaftaran sampai Transfer ke Rawat Inap

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Revision dasar | `4` |
| Status pass | `draft` — wawancara berjalan; **bukan** approval amendment |
| Backend SHA saat pass dimulai | `dd64c16` (branch `rizkiG`, 21 Agustus 2026) |
| Frontend SHA saat pass dimulai | `96a912011` (branch `RizkiV2`, 21 Agustus 2026) |
| SHA pada capability map revision 2 | Backend `e5331a0`, Frontend `08c84d371` |
| Akibat perbedaan SHA | `01-existing-capability-map.md` **berpotensi basi**. Pass ini memakai pembacaan source langsung tanggal 24 Agustus 2026 sebagai bukti, bukan capability map |
| Contract dasar | API, State, Validation, Integration, dan Permission/Audit `0.2.0` |
| Pemicu | Permintaan pengguna 24 Agustus 2026: audit menyeluruh proses bisnis IGD dari Pendaftaran sampai Transfer ke Rawat Inap, lintas frontend, kontrak API, backend, entity, basis data, integrasi, lifecycle, dan jejak audit |

### Batas scope pass ini

**Satu kalimat batas:** pass ini menutup aturan bisnis satu episode pasien IGD, sejak pasien
tiba di IGD sampai tanggung jawab klinisnya berpindah ke unit rawat inap penerima.

**Di dalam scope:**

1. pendaftaran pasien IGD, termasuk pasien tanpa identitas dan pendaftaran sementara;
2. triase dan penilaian ulang (retriase);
3. penetapan dokter pemeriksa setelah triase;
4. pengkajian keperawatan dan pengkajian medis di IGD, termasuk primary survey ABCDE;
5. pemantauan berkala: tanda vital serial, GCS, nyeri, respons pasien, perburukan kondisi;
6. permintaan dan pelaksanaan penunjang, obat, tindakan, dan konsultasi **sebatas titik
   sentuh IGD** — aturan internal modul Laboratorium, Radiologi, dan Farmasi tetap milik
   modul masing-masing;
7. keputusan tindak lanjut (disposition) beserta koreksinya;
8. permintaan rawat inap, unit tujuan, permintaan dan penetapan tempat tidur;
9. kesiapan pindah, perpindahan internal, serah terima klinis, perpindahan fisik, kedatangan,
   penerimaan, dan penutupan episode IGD;
10. kepemilikan klinis pasien (siapa yang bertanggung jawab) pada setiap tahap;
11. jejak audit, koreksi, pembatalan, dan perilaku saat gagal untuk seluruh butir di atas.

**Di luar scope pass ini — milik modul lain:**

| Kemampuan | Pemilik | Alasan |
| --- | --- | --- |
| Aturan internal pemeriksaan laboratorium dan radiologi | Modul Diagnostic Services (**belum ada di source**) | IGD hanya memesan dan menerima hasil |
| Aturan racik, telaah, dan penyerahan obat | Modul Farmasi (`PharmacyManagement`) | Sudah ada; IGD hanya titik pemesanan dan pemberian |
| Aturan tarif, klaim, dan penagihan | Modul Billing (`BillingManagement`) | `IGD-DEC-021` sudah memisahkan penutupan klinis dari penagihan |
| Aturan internal perawatan di bangsal rawat inap | Modul Rawat Inap (**belum ada di source**) | IGD berhenti pada titik serah terima |
| Aturan master pengguna, jabatan, dan unit organisasi | Modul Corporate/HR | Dibutuhkan IGD, tetapi bukan IGD yang memutuskan bentuknya |
| Bentuk menu, route, tab, warna, dan layout | `DEV_DISCRETION` sesuai hierarchy kewenangan UI | Tidak ditetapkan agent |

### Fakta yang ditemukan dari source, bukan dari wawancara

Seluruh butir di bawah adalah **Fact** hasil pembacaan source pada SHA di atas. Butir ini
tidak ditanyakan kepada pengguna.

#### F-1. Alur pendaftaran IGD yang benar-benar berjalan

Layar pendaftaran memanggil tiga endpoint berurutan:

| Urutan | Endpoint | Akibat |
| --- | --- | --- |
| 1 | `POST /api/v1/health-services/patient-management/master-data/patients` | Hanya bila pasien baru |
| 2 | `POST /api/v1/health-services/registration-management/patient-encounters` | Membuat kunjungan pasien |
| 3 | `POST /api/v1/health-services/emergency-installation-management/emergency-visits` | Membuat kunjungan IGD |

Bukti: `use-emergency-registration.js` baris 909–1035.

Langkah 2 dan 3 **tidak berada dalam satu transaksi**. Bila langkah 3 gagal, encounter
langkah 2 sudah tersimpan. Kode sudah menyadari ini dan menahan hasil langkah 2 supaya
percobaan berikutnya memakai encounter yang sama, tetapi bila petugas menutup layar,
encounter itu menjadi **encounter menggantung** tanpa kunjungan IGD dan tanpa jalan
pembersihan.

#### F-2. Jenis kunjungan IGD dipaksa menjadi Rawat Jalan

`EmergencyVisitService.ValidateRequestAsync` menolak encounter yang jenisnya bukan
`EncounterType.Outpatient`, dengan pesan "Jenis kunjungan pasien IGD harus OP".

Padahal enum `EncounterType` **sudah memiliki nilai `Emergency = 2`** yang tidak pernah
dipakai satu pun jalur IGD.

#### F-3. Pendaftaran IGD tidak membuat antrean

Tidak ada satu pun jalur yang membuat baris `TrxQueue` untuk pasien IGD. Ini konsisten
dengan sifat IGD yang tidak mengantre. Akibatnya diuraikan pada F-8.

#### F-4. Tidak ada penjagaan episode ganda

Yang dijaga hanya "satu encounter hanya boleh punya satu kunjungan IGD" lewat index unik pada
`TrxEmergencyVisit.EncounterId`. Tidak ada pemeriksaan yang mencegah **pasien yang sama**
memiliki dua encounter IGD aktif sekaligus.

Contoh: pasien Budi didaftarkan pukul 08.00, lalu petugas lain mendaftarkannya lagi pukul
08.05 karena mengira belum terdaftar. Sistem menerima keduanya. Sekarang ada dua episode IGD
aktif untuk satu orang, dengan triase, tanda vital, dan tindak lanjut yang terpecah dua.

#### F-5. Pengaturan IGD tersimpan tetapi tidak menjalankan apa pun

Empat kolom pada `MstEmergencySetting` **hanya disimpan dan divalidasi rentangnya**, tidak
pernah dipakai untuk memutuskan apa pun dalam alur kerja:

| Kolom | Maksud yang tersirat dari namanya | Dipakai memutuskan? |
| --- | --- | --- |
| `AutoCreateProvisionalEncounter` | Membuat encounter sementara otomatis | **Tidak** |
| `ImmediateCareLevelThreshold` | Batas level yang boleh langsung ditangani | **Tidak** |
| `RequireRegistrationBeforeTreatmentFromLevel` | Level yang wajib daftar dulu | **Tidak** |
| `RequireTriageBeforeStandardRegistration` | Triase dulu baru daftar lengkap | **Tidak** |

Satu-satunya sumber izin "boleh ditangani sebelum administrasi selesai" adalah
`MstEmergencyTriageLevel.AllowsTreatmentBeforeRegistration`, yang disalin ke
`TrxEmergencyTriage.ImmediateCareAllowed` lalu ke `TrxEmergencyVisit.IsImmediateCareAllowed`.

#### F-6. Penetapan dokter hanya menimpa satu kolom

Layar triase memanggil
`PATCH /api/v1/health-services/registration-management/patient-encounters/{id}/doctor`.
Endpoint itu hanya menulis `TrxPatientEncounter.DoctorId`.

Yang **tidak** ada: riwayat penetapan, waktu penetapan, siapa yang menetapkan (selain
`UpdateBy` yang tertimpa perubahan berikutnya), alasan, konsep DPJP, penerimaan oleh dokter,
dan kaitan ke hasil triase yang mendasarinya. Menetapkan dokter kedua menghapus jejak dokter
pertama.

#### F-7. Status kunjungan dapat mundur, melewati aturan transisinya sendiri

`EmergencyVisitService.CanTransition` mendefinisikan transisi yang sah. Tetapi dua tempat
menulis `visit.VisitStatus = Triaged` **secara langsung tanpa memanggil `CanTransition`**:

- `EmergencyTriageController.Create` ketika triase langsung berstatus `Completed`;
- `EmergencyTriageController.UpdateTriageStatus` ketika triase menjadi `Completed`.

Akibat nyata: pasien yang sudah `InTreatment` lalu dinilai ulang akan **kembali** ke
`Triaged`. Lebih jauh, `UpdateTriageStatus` tidak memeriksa status kunjungan sama sekali,
sehingga triase yang masih `Draft` pada kunjungan yang sudah `Disposed` dapat diselesaikan
dan **membuka kembali kunjungan yang sudah ditutup**.

#### F-8. Rantai wajib yang memutus pencatatan klinis IGD

Empat tabel klinis mewajibkan kolom yang tidak pernah ada pada pasien IGD:

| Tabel | Kolom wajib | Rantai kebutuhannya | Akibat pada IGD |
| --- | --- | --- | --- |
| `TrxPatientAssessment` | `QueueId` wajib | Butuh `TrxQueue` | Pengkajian keperawatan **tidak dapat dibuat** untuk pasien IGD |
| `TrxDoctorConsultation` | `QueueId` wajib | Butuh `TrxQueue` | Konsultasi dokter tidak dapat dibuat |
| `TrxPatientDiagnosis` | `ConsultationId` wajib | Butuh konsultasi | Diagnosis medis IGD tidak dapat dicatat |
| `TrxPatientProcedure` | `ConsultationId` wajib | Butuh konsultasi | Tindakan tidak dapat dicatat |
| `TrxPrescription` | `ConsultationId` wajib | Butuh konsultasi | Resep IGD tidak dapat dibuat |

`TrxEmergencyProcedureDetail.PatientProcedureId` juga wajib, sehingga detail tindakan IGD
ikut terkunci di ujung rantai yang sama.

Yang **tidak** terkunci: `TrxPatientVitalSign` dan `TrxPatientIntegratedProgressNote`,
karena `QueueId` dan `ConsultationId` keduanya boleh kosong.

#### F-9. Layar pengkajian IGD sebagian besar hanya membaca

Layar `emergency-assessment` memiliki sembilan tab. Yang benar-benar dapat menyimpan hanya
tujuh:

| Tab | Dapat menyimpan? | Endpoint tulis |
| --- | --- | --- |
| Assesmen Awal IGD | **Tidak** — hanya menampilkan daftar | — |
| Tanda Vital | Ya | `POST .../clinical-management/patient-vital-signs` |
| SOAP | Ya | `POST .../clinical-management/patient-integrated-progress-notes` |
| Nosokomial | Ya | `POST .../clinical-management/nosocomial-infections` |
| Catatan Terintegrasi | Ya | `POST .../clinical-management/patient-integrated-progress-notes` |
| Observasi | Ya | `POST .../emergency-observations` |
| Tindak Lanjut | Ya | `POST .../emergency-dispositions` |
| Resep | **Tidak** — hanya menampilkan daftar | — |
| Transfer Pasien | Ya | `POST .../emergency-transfers` |

#### F-10. Urutan tindak lanjut dan perpindahan saling mengunci

Tiga aturan yang sudah ada bertabrakan:

1. `EmergencyDispositionController.UpdateDispositionStatus` mengubah kunjungan menjadi
   `Disposed` begitu tindak lanjut dijalankan (`Executed`);
2. `EmergencyTransferService.ValidateRequestAsync` **menolak** pembuatan perpindahan bila
   kunjungan sudah `Disposed`;
3. `EmergencyDispositionService.ValidateVisitClosureAsync` **mewajibkan** kunjungan sudah
   `Disposed` sebelum boleh diselesaikan, dan mewajibkan tidak ada perpindahan yang belum
   tuntas.

Akibat nyata: bila dokter menjalankan keputusan "Rawat inap" lebih dulu — urutan yang paling
masuk akal bagi manusia — perpindahan pasien **tidak dapat lagi dibuat**. Satu-satunya urutan
yang berhasil adalah membuat perpindahan **sebelum** tindak lanjut dijalankan. Tidak ada satu
pun pesan yang memberitahu petugas hal ini.

Hal yang sama berlaku untuk observasi: `EmergencyObservationService` juga menolak kunjungan
yang sudah `Disposed`.

#### F-11. Penanda `ClosesEmergencyVisit` tidak pernah dipakai

`MstEmergencyDispositionType.ClosesEmergencyVisit` diisi `true` untuk seluruh tujuh jenis
tindak lanjut oleh seeder, dikirim pada balasan API, tetapi **tidak pernah dibaca untuk
memutuskan apa pun**. "Pulang" dan "Rawat inap" diperlakukan sama persis, padahal yang satu
mengakhiri kehadiran pasien dan yang satu lagi baru memulai perpindahan.

#### F-12. Tempat tidur dan ruangan pada perpindahan adalah angka bebas

`TrxEmergencyTransfer` memiliki `FromRoomId`, `ToRoomId`, `FromBedId`, dan `ToBedId`.
Keempatnya **diberi index tetapi tidak diberi foreign key dan tidak punya navigation
property** (`TrxEmergencyTransferConfiguration.cs` baris 29–32).

Artinya: nilai apa pun dapat disimpan di sana, termasuk id tempat tidur yang tidak ada, id
tempat tidur milik unit lain, atau id tempat tidur yang sedang dipakai pasien lain. Tidak ada
pemesanan tempat tidur, tidak ada perubahan `MstBed.BedStatus`, dan tidak ada pencegahan dua
pasien dialokasikan ke satu tempat tidur.

#### F-13. Modul Rawat Inap belum ada

| Yang dicari | Hasil |
| --- | --- |
| Entity permintaan rawat inap (*admission request*) | Tidak ada |
| Entity pemesanan/alokasi tempat tidur | Tidak ada |
| Entity kunjungan rawat inap aktif | Tidak ada |
| `EncounterStatus` untuk rawat inap | Tidak ada — daftarnya berhenti di alur poliklinik |
| Master tempat tidur `MstBed` | **Ada**, tetapi hanya master; tidak ada transaksi hunian |

`EncounterType.Inpatient = 3` ada di enum, tetapi tidak ada satu pun proses yang membuatnya.

#### F-14. Perpindahan tidak membedakan kejadian yang berbeda

`EmergencyTransferStatus` hanya punya enam nilai: `Requested`, `Accepted`, `InTransit`,
`Completed`, `Rejected`, `Cancelled`.

Yang **tidak** terbedakan:

| Kejadian nyata | Terwakili? |
| --- | --- |
| Tempat tidur sudah dialokasikan | Tidak |
| Pasien sudah siap dipindahkan | Tidak |
| Serah terima dokumen diajukan | Tercampur dengan `Requested` |
| Serah terima diterima penerima | Tercampur dengan `Accepted` |
| Pasien meninggalkan IGD | Hanya kolom `DepartedAt`, tanpa status |
| Pasien tiba di unit tujuan | Hanya kolom `ArrivedAt`, tanpa status |
| Pasien diterima unit tujuan | Tercampur dengan `Completed` |

`Accepted` karena itu ambigu: ia dapat berarti "unit tujuan setuju menerima" atau "pasien
sudah diterima secara fisik". `DepartedAt` dan `ArrivedAt` tidak pernah diisi oleh satu pun
endpoint yang ditemukan; keduanya hanya kolom kosong.

Serah terima juga tidak punya isi minimum: hanya satu kolom teks bebas `HandoverSummary`
sepanjang 2000 karakter. Tidak ada SBAR, tidak ada pemisahan serah terima perawat dan dokter,
tidak ada daftar barang atau obat yang ikut, tidak ada eskalasi.

#### F-15. Hak akses tidak mengenal unit pelayanan

Seluruh controller IGD memakai `[Authorize]` ditambah `AccessPermission` per resource.
Sesuai laporan `BE-IGD-010` tanggal 18 Agustus 2026, **tidak ada jalur data apa pun dari
pengguna ke unit pelayanan** di dalam basis data.

Akibat nyata: perawat unit mana pun yang memiliki izin `EmergencyTransfer.Update` dapat
menerima perpindahan pasien ke unit mana pun, termasuk unit yang bukan tempatnya bertugas.

#### F-16. Jejak audit hanya menyimpan penulis terakhir

Basis audit adalah `IdentityModel`: `CreateBy`, `CreateDateTime`, `UpdateBy`,
`UpdateDateTime`, `DeleteBy`, `CancelBy`.

Artinya hanya perubahan **terakhir** yang tercatat. Bila catatan klinis diubah tiga kali,
sistem hanya tahu siapa yang mengubahnya paling akhir. Nilai sebelumnya hilang.
`LoggerService.AuditAsync` menulis ke Serilog (berkas log), bukan ke tabel yang dapat
ditelusuri per baris data.

Pengecualian yang sudah benar: **retriase bersifat append-only**. Penilaian lama menjadi
`Superseded` dan penilaian baru menunjuk yang lama.

#### F-17. Tidak ada modul penunjang diagnostik

Pencarian berkas dengan kata `laborator`, `radiolog`, `specimen`, dan `imaging` di seluruh
`Areas/` menghasilkan **nol berkas**. Permintaan laboratorium dan radiologi belum ada dalam
bentuk apa pun.

#### F-18. Pemberian obat belum terbedakan dari peresepan

Modul Farmasi memiliki resep, telaah, racikan, penyiapan, dan penyerahan. Yang **tidak** ada
adalah catatan pemberian obat kepada pasien (*medication administration record*): siapa
menyuntikkan, pukul berapa, dosis berapa, dan bagaimana reaksi pasien.

Untuk IGD ini penting karena obat gawat darurat diberikan langsung oleh perawat, bukan
diserahkan ke pasien.

### Yang sudah benar dan tidak perlu diubah

| Kemampuan | Bukti | Klasifikasi |
| --- | --- | --- |
| Retriase append-only dengan riwayat utuh | `EmergencyTriageService.RetriageAsync` — satu transaksi, `Superseded`, `PreviousTriageId` | `REUSE_EXISTING` |
| Target waktu triase yang belum disahkan dibiarkan kosong, bukan dianggap nol | `MaxWaitingMinutesSnapshot` dan `ResponseDueAt` boleh kosong | `REUSE_EXISTING` |
| Penanda pelampauan batas waktu bersifat permanen dan idempoten | `MarkSlaBreachesAsync` menyaring `!IsSlaBreached` | `REUSE_EXISTING` |
| Salinan nilai master pada saat penilaian dibuat | `MaxWaitingMinutesSnapshot`, `IndicatorCodeSnapshot`, `IndicatorNameSnapshot` | `REUSE_EXISTING` |
| Pemisahan "keputusan sudah ditetapkan" dari "pelayanan sudah tuntas" | `Disposed` versus `Completed`, dan `VisitCompletedAt` hanya diisi endpoint `complete` | `REUSE_EXISTING` |
| Gerbang penutupan kunjungan | `ValidateVisitClosureAsync` menolak observasi dan perpindahan yang belum tuntas | `REUSE_EXISTING` |
| Pasien tanpa identitas dapat didaftarkan | `IsUnknownPatient`, `TemporaryPatientAlias` | `REUSE_EXISTING` |
| Nomor dokumen unik tidak menyaring data terhapus | `GenerateVisitNumberAsync` | `REUSE_EXISTING` |
| Alasan wajib saat membatalkan tindak lanjut dan menolak perpindahan | `BE-IGD-016` | `REUSE_EXISTING` |

### Matriks kesenjangan

| ID | Kesenjangan | Klasifikasi | Tingkat | Bukti |
| --- | --- | --- | --- | --- |
| `IGD-GAP-011` | Pengkajian keperawatan IGD tidak dapat disimpan karena `TrxPatientAssessment` mewajibkan `QueueId` | `MODIFY_EXISTING` | `CRITICAL` | F-3, F-8, F-9 |
| `IGD-GAP-012` | Pengkajian medis dokter, diagnosis, tindakan, dan resep IGD terkunci di balik `ConsultationId` | `MODIFY_EXISTING` | `CRITICAL` | F-8 |
| `IGD-GAP-013` | Urutan tindak lanjut dan perpindahan saling mengunci; jalur rawat inap dapat buntu | `MODIFY_EXISTING` | `CRITICAL` | F-10 |
| `IGD-GAP-014` | Status kunjungan dapat mundur dan kunjungan tertutup dapat terbuka kembali | `MODIFY_EXISTING` | `CRITICAL` | F-7 |
| `IGD-GAP-015` | Tidak ada pemilik klinis yang tercatat pada tahap menunggu tempat tidur, serah terima tertunda, dan dalam perjalanan | `MISSING_NEW` | `CRITICAL` | F-14, `IGD-DEC-062` |
| `IGD-GAP-016` | Modul rawat inap, permintaan rawat inap, dan alokasi tempat tidur belum ada | `MISSING_NEW` | `CRITICAL` | F-13 |
| `IGD-GAP-017` | Tempat tidur dan ruangan pada perpindahan tanpa foreign key dan tanpa pencegahan tabrakan | `MODIFY_EXISTING` | `CRITICAL` | F-12 |
| `IGD-GAP-018` | Perpindahan tidak membedakan alokasi, kesiapan, keberangkatan, kedatangan, dan penerimaan | `EXTEND_EXISTING` | `HIGH` | F-14 |
| `IGD-GAP-019` | Serah terima tanpa isi minimum, tanpa pemisahan perawat dan dokter, tanpa eskalasi | `EXTEND_EXISTING` | `HIGH` | F-14 |
| `IGD-GAP-020` | Jejak audit hanya menyimpan penulis terakhir; nilai klinis sebelumnya hilang | `MODIFY_EXISTING` | `HIGH` | F-16 |
| `IGD-GAP-021` | Hak akses tidak mengenal unit pelayanan | `MISSING_NEW` | `HIGH` | F-15 |
| `IGD-GAP-022` | Penetapan dokter tanpa riwayat, waktu, alasan, dan penerimaan | `EXTEND_EXISTING` | `HIGH` | F-6 |
| `IGD-GAP-023` | Pemantauan berkala tidak punya pemicu, jadwal, dan penanda perburukan | `MISSING_NEW` | `HIGH` | F-9 |
| `IGD-GAP-024` | Permintaan laboratorium dan radiologi belum ada | `MISSING_NEW` | `HIGH` | F-17 |
| `IGD-GAP-025` | Catatan pemberian obat belum ada | `MISSING_NEW` | `HIGH` | F-18 |
| `IGD-GAP-026` | Pesanan yang belum selesai saat pasien pindah tidak punya perlakuan | `MISSING_NEW` | `HIGH` | F-10, F-13 |
| `IGD-GAP-027` | Primary survey ABCDE hanya berupa enam kolom ringkasan teks pada triase | `EXTEND_EXISTING` | `MEDIUM` | `TrxEmergencyTriage` kolom `AirwaySummary` sampai `RedFlagSummary` |
| `IGD-GAP-028` | Pendaftaran dan kunjungan IGD tidak atomik; encounter dapat menggantung | `MODIFY_EXISTING` | `MEDIUM` | F-1 |
| `IGD-GAP-029` | Tidak ada penjagaan episode IGD ganda untuk satu pasien | `EXTEND_EXISTING` | `MEDIUM` | F-4 |
| `IGD-GAP-030` | Jenis kunjungan IGD dipaksa Rawat Jalan walau `EncounterType.Emergency` tersedia | `UNVERIFIED` | `MEDIUM` | F-2 |
| `IGD-GAP-031` | Empat pengaturan IGD tersimpan tetapi tidak menjalankan apa pun | `MODIFY_EXISTING` | `MEDIUM` | F-5 |
| `IGD-GAP-032` | `ClosesEmergencyVisit` tidak pernah dipakai memutuskan | `MODIFY_EXISTING` | `MEDIUM` | F-11 |
| `IGD-GAP-033` | Backend tidak punya proyek test; tidak satu pun `AT-IGD-*` dapat dijalankan | `MISSING_NEW` | `MEDIUM` | Tidak ada `*.Tests.csproj` |

### Ketergantungan regulasi dan SOP

Tidak ada satu pun butir di bawah yang diverifikasi terhadap teks regulasi dalam sesi ini.
Seluruhnya berstatus `REGULATORY_VERIFICATION_REQUIRED` sampai diperiksa pemilik yang
berwenang.

| ID | Topik | Klasifikasi sementara | Menunggu |
| --- | --- | --- | --- |
| `IGD-REG-002` | Kewajiban jejak audit rekam medis elektronik per baris perubahan | `REGULATORY_VERIFICATION_REQUIRED` | Security/Privacy owner + rujukan Permenkes rekam medis elektronik yang berlaku |
| `IGD-REG-003` | Kewajiban serah terima pasien antar unit dan isinya | `HOSPITAL_SOP_REQUIREMENT` | SOP MMC + Clinical Governance |
| `IGD-REG-004` | Interval pengkajian ulang per tingkat kegawatan | `HOSPITAL_SOP_REQUIREMENT` | SOP MMC — sudah menjadi blocker `IGD-DEC-060` |
| `IGD-REG-005` | Batas waktu respons triase level 2 sampai 5 | `HOSPITAL_SOP_REQUIREMENT` | SOP triase MMC — blocker lama yang masih terbuka |
| `IGD-REG-006` | Kewajiban visum dan pelaporan kematian di IGD | `REGULATORY_VERIFICATION_REQUIRED` | Legal + Clinical Governance |

### Antrean pertanyaan pass ini

Urutan disusun menurut ketergantungan: pertanyaan di atas menentukan jawaban di bawahnya.

| Urutan | ID | Pokok | Memblokir |
| ---: | --- | --- | --- |
| 1 | `IGD-OQ-052` | Bentuk episode IGD: satu encounter untuk seluruh episode, atau encounter terpisah saat masuk rawat inap | `DESIGN` — menentukan ERD, lifecycle, dan seluruh transisi transfer |
| 2 | `IGD-OQ-053` | Cara memutus rantai `QueueId` dan `ConsultationId` agar pencatatan klinis IGD dapat berjalan | `IMPLEMENTATION` |
| 3 | `IGD-OQ-054` | Urutan sah antara keputusan tindak lanjut dan proses perpindahan | `DESIGN` |
| 4 | `IGD-OQ-055` | Daftar kejadian perpindahan yang wajib dibedakan dan artinya masing-masing | `DESIGN` |
| 5 | `IGD-OQ-056` | Siapa pemilik klinis pada setiap tahap, dan apa buktinya di sistem | `DESIGN` |
| 6 | `IGD-OQ-057` | Siapa yang memesan dan mengalokasikan tempat tidur, dan aturan tabrakan alokasi | `DESIGN` |
| 7 | `IGD-OQ-058` | Perlakuan pesanan yang belum selesai saat pasien pindah | `DESIGN` |
| 8 | `IGD-OQ-059` | Isi minimum serah terima dan siapa yang wajib menandatangani | `DESIGN` |
| 9 | `IGD-OQ-060` | Kedalaman jejak audit perubahan catatan klinis | `DESIGN` |
| 10 | `IGD-OQ-061` | Hubungan pengguna ke unit pelayanan dan pengisian data lama | `IMPLEMENTATION` |
| 11 | `IGD-OQ-062` | Riwayat dan penerimaan penetapan dokter pemeriksa | `LATER SLICE` |
| 12 | `IGD-OQ-063` | Pemicu dan jadwal pemantauan berkala | `LATER SLICE` |
| 13 | `IGD-OQ-064` | Penjagaan episode IGD ganda dan penggabungan bila terlanjur | `LATER SLICE` |

`IGD-OQ-051` dari pass 2026-08-20 **tetap terbuka** dan tidak digantikan pass ini.

### Decision log pass berjalan

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-OQ-052` | Open Question | Ketika pasien IGD diputuskan rawat inap, apakah episode rawat inap memakai kunjungan (encounter) yang sama dengan IGD, atau kunjungan baru yang terhubung ke kunjungan IGD? | Product/Domain Owner + Registration API owner + Clinical Governance | `draft` — memblokir ERD, lifecycle, dan seluruh transisi perpindahan | — | F-2, F-13, F-14; `IGD-DEC-001` |

### Temuan lintas modul 2026-08-24 — blueprint Rawat Inap sudah menjawab sebagian

Pengguna menunjuk `docs/module-blueprints/rawat-inap/04-prd-to-mvp.md`. Blueprint itu ada,
lengkap, dan **statusnya lebih maju daripada blueprint IGD**.

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `RWI-BP-001` |
| `revision` | `3` |
| `status` | `approved` — disetujui **Muhammad Hamzah** 2026-08-24 lewat `RWI-DEC-067` |
| `registry_lifecycle` | `ACTIVE` |
| Modul | `InPatientManagement`, prefix entity `Inp` |
| Dampak kompatibilitas yang dijanjikan | **Tiga belas tabel baru, nol perubahan kolom pada tabel modul lain** |
| Backend SHA | `5afb54bd` (branch `MHamzah`) |
| Frontend SHA | `dec4fdef` (branch `HamzahV2`) |

#### Yang sudah diputuskan pihak Rawat Inap dan menyentuh IGD

| ID milik Rawat Inap | Isi | Status di sana |
| --- | --- | --- |
| `RWI-DEC-041` | Saat disposisi `RANAP` dijalankan, kunjungan IGD **ditutup** dan kunjungan baru bertipe rawat inap **dibuat** sebagai jangkar episode | `approved` untuk arah desain; **implementasi terblokir** menunggu persetujuan pemilik `EmergencyInstallationManagement` |
| `RWI-RULE-029` | Tujuh aturan rinci serah terima IGD ke rawat inap | Terkunci di sisi Rawat Inap |
| `RWI-RULE-026` aturan 6 | Pelonggaran keharusan antrean dan konsultasi **hanya** untuk kunjungan bertipe rawat inap. Perilaku rawat jalan, **IGD**, dan medical check-up **tidak boleh berubah sedikit pun** | Terkunci di sisi Rawat Inap |
| `RWI-OQ-034` / `DEC-INP-002` | Pertanyaan formal kepada pemilik IGD: setujukah bahwa disposisi `RANAP` menutup kunjungan IGD dan membuat kunjungan rawat inap baru, serta setujukah `ClosesEmergencyVisit` mulai benar-benar dijalankan? | `OPEN` — memblokir slice `INP-S09` |

Isi lengkap `RWI-RULE-029`:

| No | Aturan |
| ---: | --- |
| 1 | Disposisi `RANAP` dijalankan → kunjungan IGD **ditutup**, kunjungan baru bertipe rawat inap **dibuat**. Kunjungan baru itulah jangkar episode |
| 2 | Kedua kunjungan dihubungkan sebagai **satu rangkaian kedatangan** |
| 3 | Kunjungan rawat inap mewarisi pasien dan penjamin dari kunjungan IGD. Unit, kelas, dan DPJP diisi admisi rawat inap |
| 4 | Penutupan IGD dan pembuatan kunjungan rawat inap adalah **satu tindakan utuh** — berhasil dua-duanya atau tidak ada yang berubah |
| 5 | Bila gagal di tengah, kunjungan IGD tetap terbuka. Tidak boleh ada pasien "tidak ada di mana-mana" |
| 6 | Catatan klinis IGD **tetap menempel** pada kunjungan IGD. Tidak dipindah, tidak disalin |
| 7 | `ClosesEmergencyVisit` menjadi penentu perilaku ini dan mulai benar-benar dijalankan. `RANAP` tetap `true` |

#### Akibat pada `IGD-OQ-052`

`RWI-DEC-041` **identik dengan pilihan B** pada `IGD-OQ-052`. Pertanyaan itu karena itu tidak
lagi terbuka sebagai pilihan bebas; yang tersisa adalah persetujuan pemilik IGD atas arah yang
sudah dipilih pihak Rawat Inap.

#### Tiga konflik yang ditemukan antara kedua blueprint

| ID | Konflik | Tingkat | Bukti |
| --- | --- | --- | --- |
| `IGD-CONFLICT-003` | Blueprint Rawat Inap menganggap kunjungan IGD bertipe `Emergency`. Source memaksa `Outpatient` | `HIGH` | `RWI-RULE-029` bagian "Keadaan yang menjadi masalah" dan contoh pukul 20:10 versus `EmergencyVisitService.ValidateRequestAsync` (F-2) |
| `IGD-CONFLICT-004` | `RWI-RULE-026` aturan 6 melarang perilaku IGD berubah, padahal pembatas yang sama persis — `QueueId` dan `ConsultationId` wajib — adalah penyebab `IGD-GAP-011` dan `IGD-GAP-012` yang membuat pengkajian, diagnosis, tindakan, dan resep IGD tidak dapat disimpan | `CRITICAL` | `RWI-RULE-026` aturan 3 dan 6 versus F-8 |
| `IGD-CONFLICT-005` | `RWI-RULE-029` aturan 2 mewajibkan kedua kunjungan dihubungkan, tetapi kolom penghubungnya **tidak ada di ERD, kamus data, arsitektur, maupun kontrak Rawat Inap**. Manifest Rawat Inap juga menjanjikan "nol perubahan kolom pada tabel modul lain", sedangkan penghubung itu hanya mungkin sebagai kolom pada `TrxPatientEncounter` | `HIGH` | Pencarian "rangkaian kedatangan" hanya ditemukan di `00-interview-decisions.md`; `InpEpisode.EncounterId` menunjuk kunjungan rawat inap saja |

**Penjelasan `IGD-CONFLICT-004` dengan contoh.** `RWI-RULE-026` melonggarkan keharusan
`QueueId` dan `ConsultationId` **hanya** untuk kunjungan bertipe rawat inap, dan aturan 6-nya
menutup rapat kemungkinan perilaku IGD ikut berubah. Padahal pembatas yang sama itulah yang
membuat perawat IGD hari ini tidak dapat menyimpan pengkajian keperawatan sama sekali.

Bila `RWI-RULE-026` dijalankan apa adanya: Tn. Budi yang dirawat inap dapat dikaji, didiagnosis,
dan diresepkan. Ny. Sari yang masih di IGD tidak dapat dikaji sama sekali — dan tetap tidak
dapat, bahkan setelah seluruh modul Rawat Inap selesai dibangun.

#### Akibat pada matriks kesenjangan IGD

| Gap IGD | Perubahan setelah membaca blueprint Rawat Inap |
| --- | --- |
| `IGD-GAP-016` modul rawat inap belum ada | **Turun** dari `CRITICAL` menjadi `MEDIUM`. Blueprint `approved` sudah ada beserta 13 tabel dan roadmap-nya; yang belum adalah implementasinya, dan itu milik tim lain |
| `IGD-GAP-017` bed dan room tanpa foreign key | **Berubah arah**. Alokasi tempat tidur menjadi milik Rawat Inap (`InpBedPlacement`), bukan milik `TrxEmergencyTransfer`. Empat kolom bed/room pada transfer IGD kemungkinan besar harus dicabut, bukan diberi foreign key |
| `IGD-GAP-013` disposition dan transfer saling mengunci | **Berubah arah**. Untuk jalur `RANAP`, perpindahan dikerjakan Rawat Inap. Pertanyaannya berubah menjadi: jalur mana yang masih memakai `TrxEmergencyTransfer` |
| `IGD-GAP-032` `ClosesEmergencyVisit` tidak dipakai | **Naik prioritas**. `RWI-RULE-029` aturan 7 menjadikannya penentu perilaku |
| `IGD-GAP-011` dan `IGD-GAP-012` | **Tetap `CRITICAL`**, dan kini berbenturan langsung dengan `RWI-RULE-026` aturan 6 |

### Decision log lanjutan pass berjalan

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-OQ-052` | Open Question | Bentuk episode saat pasien IGD masuk rawat inap | Product/Domain Owner IGD + Registration API owner + Clinical Governance | `superseded` oleh `IGD-DEC-067` | — | Dijawab lewat penyelarasan dengan `RWI-DEC-041`, bukan lewat pilihan bebas |
| `IGD-DEC-067` | Decision | Modul IGD mengikuti `RWI-DEC-041` dan `RWI-RULE-029`: ketika disposisi `RANAP` dijalankan, kunjungan IGD ditutup dan kunjungan baru bertipe rawat inap dibuat sebagai jangkar episode rawat inap. Catatan klinis IGD tetap menempel pada kunjungan IGD. Penanda `ClosesEmergencyVisit` mulai benar-benar dijalankan. Penutupan dan pembuatan bersifat satu tindakan utuh | Product/Domain Owner IGD, dengan pemilik `EmergencyInstallationManagement` sebagai pemberi persetujuan yang diminta `RWI-OQ-034` dan `DEC-INP-002` | `approved` | Rizki Gunawan, 24 Agustus 2026 | `RWI-DEC-041`, `RWI-RULE-029`, `RWI-TF-016`, `RWI-TF-017`; jawaban pengguna 24 Agustus 2026 |
| `IGD-CONFLICT-003` | Conflict | Blueprint Rawat Inap menganggap kunjungan IGD bertipe `Emergency`; source memaksa `Outpatient` | Product/Domain Owner IGD + Registration API owner | `draft` — belum diputuskan | — | F-2 versus `RWI-RULE-029` |
| `IGD-CONFLICT-004` | Conflict | `RWI-RULE-026` aturan 6 melarang perilaku IGD berubah, sedangkan pembatas yang dilonggarkannya adalah penyebab `IGD-GAP-011` dan `IGD-GAP-012` | Product/Domain Owner IGD + pemilik `ClinicalManagement` + pemilik `PharmacyManagement` + Product/Domain Owner Rawat Inap | `draft` — **memblokir seluruh pencatatan klinis IGD** | — | F-8 versus `RWI-RULE-026` |
| `IGD-CONFLICT-005` | Conflict | Penghubung antara kunjungan IGD dan kunjungan rawat inap diwajibkan `RWI-RULE-029` aturan 2 tetapi tidak dirancang di mana pun | Product/Domain Owner IGD + Product/Domain Owner Rawat Inap + Registration API owner | `draft` | — | Pencarian menyeluruh pada blueprint Rawat Inap |
| `IGD-OQ-053` | Open Question | Bagaimana pembatas `QueueId` dan `ConsultationId` dilonggarkan supaya pencatatan klinis IGD dapat berjalan, mengingat `RWI-RULE-026` aturan 6 melarang perilaku IGD berubah? | Product/Domain Owner IGD + pemilik `ClinicalManagement` + pemilik `PharmacyManagement` | `draft` — memblokir `IGD-GAP-011`, `IGD-GAP-012`, dan seluruh slice pengkajian | — | `IGD-CONFLICT-004` |

| `IGD-OQ-053` | Open Question | Bagaimana pembatas `QueueId` dan `ConsultationId` dilonggarkan supaya pencatatan klinis IGD dapat berjalan? | Product/Domain Owner IGD + pemilik `ClinicalManagement` + pemilik `PharmacyManagement` | `superseded` oleh `IGD-DEC-068` | — | `IGD-CONFLICT-004` |
| `IGD-DEC-068` | Decision | Pelonggaran `RWI-RULE-026` diperluas sehingga berlaku untuk kunjungan bertipe rawat inap **dan** IGD. Aturan 6 pada `RWI-RULE-026` direvisi. Keharusan mengisi antrean dan konsultasi dilonggarkan untuk kedua tipe kunjungan, sehingga catatan klinis boleh menempel langsung pada kunjungan. IGD **tidak** membuat tabel pengkajian, diagnosis, tindakan, atau resep tandingan, dan **tidak** membuat antrean semu. Perilaku rawat jalan dan medical check-up tetap tidak berubah sedikit pun | Product/Domain Owner IGD, dengan pemilik `ClinicalManagement`, pemilik `PharmacyManagement`, dan Product/Domain Owner Rawat Inap sebagai approver akhir | `draft` — pilihan pengguna jelas, tetapi ketiga pemilik yang harus menyetujui **belum ditunjuk namanya**; revisi `RWI-RULE-026` juga membutuhkan persetujuan pemilik Rawat Inap | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-053`; menutup `IGD-CONFLICT-004`; konsisten dengan `IGD-DEC-003` dan `RWI-RULE-026` aturan 1 dan 2 |

#### Acceptance criteria awal dari `IGD-DEC-068`

1. Pengkajian keperawatan dapat disimpan untuk pasien IGD tanpa satu pun baris `TrxQueue`
   dibuat untuk pasien itu.
2. Daftar antrean poliklinik tidak memuat satu pun pasien IGD, sebelum maupun sesudah
   perubahan.
3. Konsultasi dokter dapat dibuat untuk kunjungan IGD tanpa baris antrean.
4. Diagnosis, tindakan, dan resep dapat dibuat untuk pasien IGD.
5. Perilaku rawat jalan tidak berubah: pengkajian dan konsultasi rawat jalan tetap menolak
   permintaan yang tidak menyertakan antrean.
6. Perilaku medical check-up tidak berubah.
7. Tersedia test regresi untuk jalur rawat jalan yang menyentuh pengkajian, konsultasi,
   diagnosis, tindakan, dan resep, sesuai kewajiban `RWI-DEC-051`.
8. Rekam medis pasien tetap berada pada tabel yang sama; tidak ada tabel klinis tandingan
   milik IGD.

#### Blocker yang muncul dari `IGD-DEC-068`

| Blocker | Menunggu | Memblokir |
| --- | --- | --- |
| Persetujuan pemilik `ClinicalManagement` | Pemilik belum ditunjuk — sama dengan `DEC-INP-001` milik Rawat Inap | `IMPLEMENTATION` |
| Persetujuan pemilik `PharmacyManagement` | Pemilik belum ditunjuk — sama dengan `DEC-INP-001` | `IMPLEMENTATION` |
| Revisi `RWI-RULE-026` aturan 6 | Product/Domain Owner Rawat Inap, **Muhammad Hamzah** | `IMPLEMENTATION` |
| Test regresi jalur rawat jalan | Tidak ada proyek test sama sekali di backend — `IGD-GAP-033` | `IMPLEMENTATION` |

Tidak satu pun memblokir `DESIGN`. Perancangan ERD, kontrak, dan state matrix boleh berjalan.

#### Catatan efisiensi lintas modul

`DEC-INP-001` milik Rawat Inap menanyakan hal yang sama kepada pemilik yang sama.
`IGD-DEC-068` dan `DEC-INP-001` sebaiknya diajukan sebagai **satu permintaan persetujuan**
kepada pemilik `ClinicalManagement` dan `PharmacyManagement`, bukan dua permintaan terpisah.
Bila diajukan terpisah, ada risiko keduanya dijawab berbeda dan menghasilkan dua perilaku
untuk pembatas yang sama.

| `IGD-OQ-054` | Open Question | Setelah `IGD-DEC-067` berlaku, jalur perpindahan mana yang masih memakai `TrxEmergencyTransfer`, dan apa yang terjadi pada empat kolom tempat tidur dan ruangan di dalamnya? | Product/Domain Owner IGD + Product/Domain Owner Rawat Inap | `draft` — memblokir desain state transfer dan `IGD-GAP-013`, `IGD-GAP-017`, `IGD-GAP-018` | — | `IGD-DEC-067`, `RWI-RULE-029`, `RWI-DEC-020`, F-10, F-12, F-14 |

| `IGD-OQ-054` | Open Question | Jalur perpindahan mana yang masih memakai `TrxEmergencyTransfer` setelah `IGD-DEC-067`? | Product/Domain Owner IGD + Product/Domain Owner Rawat Inap | `superseded` oleh `IGD-DEC-069` | — | `IGD-DEC-067`, `RWI-RULE-029`, `RWI-DEC-020` |
| `IGD-DEC-069` | Decision | `TrxEmergencyTransfer` berhenti mengurus tempat tidur dan berubah menjadi **catatan kepergian pasien dari IGD**. Empat kolom `FromRoomId`, `ToRoomId`, `FromBedId`, dan `ToBedId` dicabut. Yang tetap ada: unit asal dan tujuan, waktu berangkat, waktu tiba, perawat pengirim, perawat penerima, dan serah terima. Catatan ini dibuat untuk **seluruh** kepergian pasien dari IGD ke unit di dalam rumah sakit, termasuk rawat inap, ICU, kamar operasi, dan kamar jenazah. Pemesanan dan penempatan tempat tidur sepenuhnya milik Rawat Inap lewat `InpBedPlacement` | Product/Domain Owner IGD, dengan Product/Domain Owner Rawat Inap sebagai pihak yang harus menyepakati batasnya | `draft` — pilihan pengguna jelas; kesepakatan dengan pemilik Rawat Inap dan approval formal belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-054`; F-12, F-14; `RWI-DEC-020`, `RWI-RULE-015`, `RWI-RULE-029` |

#### Acceptance criteria awal dari `IGD-DEC-069`

1. `TrxEmergencyTransfer` tidak lagi memiliki kolom tempat tidur maupun ruangan, sehingga
   tidak mungkin lagi menyimpan id tempat tidur yang tidak ada atau milik unit lain.
2. Tidak ada satu pun jalur IGD yang mengubah `MstBed.BedStatus`.
3. Kepergian pasien ke kamar operasi dan kamar jenazah tetap tercatat, walaupun keduanya
   tidak dimiliki modul mana pun.
4. Pertanyaan "pukul berapa pasien meninggalkan IGD" dapat dijawab dari data milik IGD
   sendiri, tanpa membaca tabel modul lain.
5. Untuk jalur rawat inap dan ICU, catatan kepergian IGD dan `InpBedPlacement` tidak boleh
   saling bertentangan; sumber kebenaran waktu tiba ditetapkan satu, bukan dua.
6. Serah terima yang gagal tetap meninggalkan jejak pada IGD, sesuai `RWI-RULE-029` aturan 5
   yang melarang keadaan pasien "tidak ada di mana-mana".

#### Yang menjadi terbuka akibat `IGD-DEC-069`

| Butir | Keterangan |
| --- | --- |
| Sumber kebenaran waktu tiba | Untuk jalur rawat inap, `ArrivedAt` pada catatan kepergian IGD dan waktu penempatan pada `InpBedPlacement` berpotensi berbeda. Harus disepakati satu |
| Migration pencabutan empat kolom | Basis data pengembangan dipakai bersama satu tim. Pencabutan kolom memerlukan pemeriksaan data lama lebih dulu |
| Nilai `EmergencyTransferStatus` | Enam nilai yang ada tidak lagi cocok untuk catatan kepergian; dibahas pada `IGD-OQ-055` |

| `IGD-OQ-055` | Open Question | Kejadian apa saja yang wajib dibedakan pada catatan kepergian pasien dari IGD, dan siapa sumber kebenaran waktu tiba ketika tujuannya rawat inap? | Product/Domain Owner IGD + Nursing authority + Clinical Governance + Product/Domain Owner Rawat Inap | `draft` — memblokir desain state matrix transfer, `IGD-GAP-018`, dan penegakan `IGD-DEC-062` sampai `IGD-DEC-066` | — | `IGD-DEC-069`, F-14, `IGD-DEC-061` sampai `IGD-DEC-066` |

| `IGD-OQ-055` | Open Question | Kejadian apa saja yang wajib dibedakan pada catatan kepergian pasien dari IGD, dan siapa sumber kebenaran waktu tiba? | Product/Domain Owner IGD + Nursing authority + Clinical Governance + Product/Domain Owner Rawat Inap | `superseded` oleh `IGD-DEC-070` dan `IGD-DEC-071` | — | `IGD-DEC-069`, F-14 |
| `IGD-DEC-070` | Decision | Catatan kepergian pasien dari IGD menyimpan **dua rangkaian status yang berjalan sendiri-sendiri**. Rangkaian **fisik** menentukan siapa pemilik klinis: `Disiapkan` → `Berangkat` → `Tiba`. Rangkaian **dokumen** menentukan tuntasnya serah terima: `Diajukan` → `Tertunda` → `Diterima` atau `Ditolak`. Keadaan "pasien sudah `Tiba` sementara dokumen masih `Tertunda`" adalah keadaan yang **sah**, bukan galat. Eskalasi menyasar rangkaian dokumen, tidak pernah menahan rangkaian fisik | Product/Domain Owner IGD, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna jelas; approval keperawatan dan klinis belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-055`; menegakkan `IGD-DEC-061` sampai `IGD-DEC-066`; menutup `IGD-GAP-018` |
| `IGD-DEC-071` | Decision | Untuk jalur rawat inap, **event `Tiba` pada catatan kepergian IGD adalah sumber kebenaran** waktu pasien tiba di unit tujuan. `InpBedPlacement` milik Rawat Inap membacanya dan tidak menetapkan waktu tiba sendiri. Aturan ini berlaku seragam untuk seluruh tujuan, termasuk kamar operasi dan kamar jenazah yang tidak memiliki `InpBedPlacement` | Product/Domain Owner IGD, dengan Product/Domain Owner Rawat Inap sebagai pihak yang harus menyepakati | `draft` — pilihan pengguna jelas; kesepakatan dengan pemilik Rawat Inap belum tercatat dan menyentuh `INP-S01` yang sudah `approved` | — | Jawaban pengguna 24 Agustus 2026; konsisten dengan `IGD-DEC-064` |

#### Bentuk yang dikunci `IGD-DEC-070`

| Rangkaian | Nilai | Menentukan | Diisi oleh |
| --- | --- | --- | --- |
| Fisik | `Disiapkan` | Pasien siap dipindahkan, belum berangkat | Perawat IGD |
| Fisik | `Berangkat` | Pasien meninggalkan IGD — mengisi waktu berangkat | Perawat IGD |
| Fisik | `Tiba` | Pasien sampai di unit tujuan — **memindahkan pemilik klinis** | Petugas unit penerima, sesuai `IGD-DEC-064` |
| Dokumen | `Diajukan` | Ringkasan serah terima dikirim | Perawat IGD |
| Dokumen | `Tertunda` | Menunggu peninjauan penerima — **eskalasi berjalan di sini** | Sistem |
| Dokumen | `Diterima` | Penerima berwenang menyatakan menerima | Petugas unit penerima |
| Dokumen | `Ditolak` | Penolakan beserta alasan, pelaku, dan waktu server | Petugas unit penerima |

#### Acceptance criteria awal dari `IGD-DEC-070` dan `IGD-DEC-071`

1. Sistem menerima dan menyimpan keadaan fisik `Tiba` bersamaan dengan keadaan dokumen
   `Tertunda`, tanpa menampilkannya sebagai galat.
2. Event `Tiba` memindahkan pemilik klinis ke unit penerima walaupun dokumen belum
   `Diterima`.
3. Event `Tiba` **tidak** mengubah keadaan dokumen menjadi `Diterima` secara otomatis.
4. Eskalasi dokumen `Tertunda` tidak pernah menahan, menunda, atau membatalkan rangkaian
   fisik.
5. Kombinasi yang tidak masuk akal ditolak, misalnya dokumen `Diterima` sementara fisik
   belum `Berangkat`.
6. Penolakan dokumen wajib menyimpan alasan, pelaku, dan waktu server.
7. Waktu berangkat dan waktu tiba benar-benar terisi oleh endpoint, bukan tetap kosong
   seperti keadaan sekarang.
8. `InpBedPlacement` untuk pasien asal IGD memakai waktu tiba dari catatan kepergian IGD,
   bukan waktu yang ditetapkannya sendiri.
9. Laporan lama tinggal pasien IGD dapat dihitung dari data milik IGD sendiri.

#### Blocker yang muncul

| Blocker | Menunggu | Memblokir |
| --- | --- | --- |
| Kesepakatan sumber kebenaran waktu tiba | Product/Domain Owner Rawat Inap, **Muhammad Hamzah** — menyentuh `INP-S01` yang sudah `approved` | `IMPLEMENTATION` |
| Nilai batas waktu dan urutan eskalasi dokumen | SOP MMC, sesuai `IGD-DEC-063` yang masih menunggu | `IMPLEMENTATION` |
| Approval Nursing authority dan Clinical Governance | Pemilik belum ditunjuk | `IMPLEMENTATION` |

| `IGD-OQ-056` | Open Question | Siapa pemilik klinis pasien selama rangkaian fisik berstatus `Berangkat`, yaitu ketika pasien sudah meninggalkan IGD tetapi belum tiba di unit tujuan? Dan siapa dokter penanggung jawab antara saat keputusan rawat inap ditetapkan sampai DPJP rawat inap mulai berlaku? | Product/Domain Owner IGD + Clinical Governance + Nursing authority + Product/Domain Owner Rawat Inap | `draft` — memblokir `IGD-GAP-015` dan matriks kepemilikan pasien | — | `IGD-DEC-062`, `IGD-DEC-070`, `RWI-RULE-030` |

| `IGD-OQ-056` | Open Question | Pemilik klinis saat status fisik `Berangkat`, dan dokter penanggung jawab sebelum DPJP rawat inap berlaku | Product/Domain Owner IGD + Clinical Governance + Nursing authority + Product/Domain Owner Rawat Inap | `superseded` oleh `IGD-DEC-072` dan `IGD-DEC-073` | — | `IGD-DEC-062`, `IGD-DEC-070`, `RWI-RULE-030` |
| `IGD-DEC-072` | Decision | Pemilik klinis pasien **tetap unit pengirim** selama status fisik `Berangkat`, yaitu sejak pasien meninggalkan IGD sampai petugas unit penerima mencatat event `Tiba`. Perawat pengantar dari IGD membawa tanggung jawab itu bersamanya dan tidak boleh meninggalkan pasien sebelum event `Tiba` tercatat. Tidak ada satu detik pun keadaan tanpa pemilik | Product/Domain Owner IGD, dengan Clinical Governance dan Nursing authority sebagai approver akhir | `draft` — pilihan pengguna jelas; approval klinis dan keperawatan belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-056`; melengkapi `IGD-DEC-062`; menegakkan `IGD-DEC-064`; menutup `IGD-GAP-015` |
| `IGD-DEC-073` | Decision | Dokter IGD yang menetapkan keputusan rawat inap **tetap menjadi dokter penanggung jawab** sampai baris pertama DPJP rawat inap berlaku sesuai `RWI-RULE-030`. Perpindahan tanggung jawab dokter terjadi pada saat yang sama dengan perpindahan tanggung jawab unit. Batas waktu menunggu dan jalur eskalasi bila DPJP rawat inap tak kunjung ditetapkan wajib berasal dari SOP yang disahkan dan tidak boleh ditebak | Product/Domain Owner IGD, dengan Clinical Governance dan Product/Domain Owner Rawat Inap sebagai approver akhir | `draft` — pilihan pengguna jelas; approval klinis belum tercatat dan nilai batas waktu menunggu SOP | — | Jawaban pengguna 24 Agustus 2026; konsisten dengan `RWI-RULE-030` tanpa mengubahnya |

#### Matriks kepemilikan pasien yang kini lengkap

Tidak ada satu baris pun berisi `NO OWNER`.

| Tahap | Pemilik unit | Dokter penanggung jawab | Dasar |
| --- | --- | --- | --- |
| Tiba di IGD, menunggu triase | IGD | Belum ada | `IGD-DEC-062` |
| Triase selesai | IGD | Dokter IGD setelah penetapan | `IGD-DEC-062` |
| Pengkajian dan tindakan berlangsung | IGD | Dokter IGD | `IGD-DEC-062` |
| Keputusan rawat inap ditetapkan | IGD | Dokter IGD | `IGD-DEC-073` |
| Menunggu tempat tidur | IGD | Dokter IGD | `IGD-DEC-072`, `IGD-DEC-073` |
| Tempat tidur dialokasikan Rawat Inap | IGD | Dokter IGD | `IGD-DEC-072` |
| Fisik `Disiapkan` | IGD | Dokter IGD | `IGD-DEC-072` |
| **Fisik `Berangkat`** | **IGD** | **Dokter IGD** | `IGD-DEC-072`, `IGD-DEC-073` |
| Fisik `Tiba` | Unit penerima | DPJP rawat inap | `IGD-DEC-064`, `RWI-RULE-030` |
| Dokumen `Tertunda` setelah `Tiba` | Unit penerima | DPJP rawat inap | `IGD-DEC-062` |
| Dokumen `Ditolak` setelah `Tiba` | Unit penerima | DPJP rawat inap | `IGD-DEC-062` |
| Dokumen `Diterima` | Unit penerima | DPJP rawat inap | `IGD-DEC-061` |

**Contoh konkret.** Ny. Sari diputuskan rawat inap pukul 22:15 oleh dr. Budi, dokter jaga IGD.
Tempat tidur Melati 2A baru siap pukul 23:40. Perawat IGD mengantar Ny. Sari pukul 23:45 dan
petugas bangsal mencatat event `Tiba` pukul 23:52.

| Waktu | Pemilik unit | Dokter penanggung jawab |
| --- | --- | --- |
| 22:15 – 23:45 | IGD | dr. Budi |
| 23:45 – 23:52 (di lift dan koridor) | **IGD** | **dr. Budi** |
| 23:52 dan seterusnya | Bangsal Melati | dr. Andi, DPJP rawat inap |

Bila Ny. Sari sesak berat pukul 23:48 di dalam lift, yang bertanggung jawab adalah IGD dan
dr. Budi. Perawat pengantar IGD ada di sana dan tidak boleh meninggalkannya.

#### Acceptance criteria awal dari `IGD-DEC-072` dan `IGD-DEC-073`

1. Sistem selalu dapat menjawab "siapa unit penanggung jawab pasien ini sekarang" dengan
   **tepat satu** nama unit, pada setiap tahap tanpa kecuali.
2. Sistem selalu dapat menjawab "siapa dokter penanggung jawab pasien ini sekarang" dengan
   tepat satu nama, sejak dokter IGD ditetapkan.
3. Tidak ada kombinasi status yang menghasilkan keadaan tanpa pemilik unit.
4. Perpindahan pemilik unit dan perpindahan dokter penanggung jawab terjadi pada kejadian
   yang sama, yaitu event `Tiba`.
5. Perpindahan kepemilikan tercatat sebagai baris tersendiri yang dapat ditelusuri, bukan
   disimpulkan dari kolom yang tertimpa.
6. Daftar pantau menampilkan pasien yang sudah `Berangkat` tetapi belum `Tiba` melebihi
   batas waktu, sebagai peringatan bahwa event `Tiba` mungkin lalai dicatat.
7. Batas waktu pada butir 6 dan batas waktu menunggu DPJP rawat inap dibaca dari
   konfigurasi SOP, dan ditandai belum tersedia bila SOP belum disahkan.

#### Butir yang tertutup tanpa pertanyaan

| ID | Keterangan |
| --- | --- |
| `IGD-OQ-057` | Rencana pertanyaan tentang siapa yang memesan dan mengalokasikan tempat tidur **gugur**. `IGD-DEC-069` sudah memindahkan seluruh urusan tempat tidur ke Rawat Inap lewat `InpBedPlacement`, dan aturan tabrakan alokasi sudah dikunci `RWI-RULE-015` milik Rawat Inap. IGD tidak lagi punya kewenangan di sana |
| `IGD-GAP-015` | Tertutup oleh `IGD-DEC-072` dan `IGD-DEC-073` |
| `IGD-GAP-017` | Tertutup oleh `IGD-DEC-069` |
| `IGD-GAP-018` | Tertutup oleh `IGD-DEC-070` |

| `IGD-OQ-057` | Open Question | Apakah kunjungan IGD berubah menjadi `EncounterType.Emergency`? Pelonggaran `IGD-DEC-068` disaring berdasarkan tipe kunjungan, sedangkan kunjungan IGD hari ini bertipe `Outpatient` yang sama persis dengan poliklinik | Product/Domain Owner IGD + Registration API owner + Product/Domain Owner Rawat Inap | `draft` — **memblokir implementasi `IGD-DEC-068`** dan menutup `IGD-CONFLICT-003` | — | F-2, `IGD-CONFLICT-003`, `IGD-DEC-068`, `RWI-RULE-026` aturan 6 |

| `IGD-OQ-057` | Open Question | Apakah kunjungan IGD berubah menjadi `EncounterType.Emergency`? | Product/Domain Owner IGD + Registration API owner + Product/Domain Owner Rawat Inap | `superseded` oleh `IGD-DEC-074` | — | F-2, `IGD-CONFLICT-003` |
| `IGD-DEC-074` | Decision | Kunjungan IGD memakai `EncounterType.Emergency`, bukan `Outpatient`. Validasi yang memaksa `Outpatient` diganti menjadi mewajibkan `Emergency`. Data kunjungan IGD lama yang bernilai `Outpatient` diperbaiki lewat migration, dikenali dari keberadaan baris `TrxEmergencyVisit` yang menunjuknya. Pelonggaran `IGD-DEC-068` disaring dari tipe kunjungan, sehingga poliklinik tidak ikut terpengaruh | Product/Domain Owner IGD, dengan Registration API owner sebagai approver akhir | `draft` — pilihan pengguna jelas; approval Registration API owner belum tercatat dan migration belum diotorisasi | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-057`; menutup `IGD-CONFLICT-003`; menyelaraskan dengan asumsi `RWI-RULE-029`; prasyarat implementasi `IGD-DEC-068` |
| `IGD-CONFLICT-003` | Conflict | Blueprint Rawat Inap menganggap kunjungan IGD bertipe `Emergency`; source memaksa `Outpatient` | Product/Domain Owner IGD + Registration API owner | `resolved` oleh `IGD-DEC-074` | — | F-2 versus `RWI-RULE-029` |

#### Verifikasi source sebelum keputusan ini dicatat

| Yang diperiksa | Hasil |
| --- | --- |
| Kode yang menulis `EncounterType.Emergency` | **Nol.** Nilai `Emergency = 2` belum pernah dipakai satu jalur pun, sehingga tidak ada data lama bernilai `2` yang dapat rusak |
| Kode yang membandingkan `EncounterType.Outpatient` | **Tiga tempat**, dirinci di bawah |
| Kode yang menganggap semua non-`Inpatient` sebagai rawat jalan | Tidak ditemukan |

#### Dua temuan yang mengubah cakupan pekerjaan

**Temuan 1 — kelas pasien IGD akan hilang bila tidak ikut diperbaiki.**

`PatientEncounterController.ResolvePatientClassAsync` baris 1470 menetapkan kelas pasien
bawaan **hanya ketika** `EncounterType == Outpatient`:

```
if (request.EncounterType == EncounterType.Outpatient)
{
    // cari master kelas pasien bawaan rawat jalan
    // gagalkan bila tidak ada atau ada lebih dari satu
}
```

Begitu kunjungan IGD menjadi `Emergency`, cabang ini tidak lagi berjalan untuk IGD. Akibatnya
`PatientClassId` kunjungan IGD menjadi kosong, dan konteks tarifnya hilang. Ini **tidak akan
memunculkan galat** — pendaftaran tetap berhasil, hanya kelas pasiennya yang kosong. Kegagalan
yang diam seperti ini persis pola yang diperingatkan pelajaran `BE-IGD-016`.

Karena itu keputusan ini **wajib** disertai penetapan kelas pasien untuk kunjungan
`Emergency`. Kelas mana yang dipakai IGD belum diputuskan dan menjadi `IGD-OQ-059`.

**Temuan 2 — validasi tipe kunjungan ditulis dua kali.**

Aturan yang sama ada di dua tempat:

| Berkas | Baris |
| --- | --- |
| `EmergencyVisitService.cs` | 97 |
| `EmergencyVisitController.cs` | 525 |

Keduanya harus diubah bersamaan. Mengubah salah satu saja menghasilkan persis jenis cacat
`BE-IGD-016`: satu jalur diperbaiki, jalur kedua yang dipakai sehari-hari terlewat.

#### Acceptance criteria awal dari `IGD-DEC-074`

1. Kunjungan IGD baru tersimpan dengan `EncounterType = Emergency`.
2. Permintaan pembuatan kunjungan IGD dengan tipe selain `Emergency` ditolak, dan
   penolakan itu berlaku pada **kedua** tempat validasi.
3. Seluruh kunjungan IGD lama bernilai `Outpatient` berubah menjadi `Emergency`, dan
   jumlah baris yang berubah sama persis dengan jumlah baris `TrxEmergencyVisit` yang
   `EncounterId`-nya terisi.
4. Tidak ada satu pun kunjungan poliklinik yang ikut berubah tipenya.
5. Kunjungan IGD tetap memperoleh kelas pasien; `PatientClassId` tidak kosong.
6. Pelonggaran `IGD-DEC-068` menyala untuk kunjungan `Emergency` dan `Inpatient`, dan
   **tidak** menyala untuk `Outpatient`.
7. Laporan jumlah kunjungan rawat jalan tidak lagi memuat pasien IGD.
8. Tersedia cara mundur bila migration harus dibatalkan.

#### Blocker yang muncul

| Blocker | Menunggu | Memblokir |
| --- | --- | --- |
| Otorisasi menjalankan migration | Basis data pengembangan **dipakai bersama satu tim** dan berisi data pasien. Perubahan tipe kunjungan menyentuh data lama, bukan hanya skema | `IMPLEMENTATION` |
| Kelas pasien untuk kunjungan `Emergency` | `IGD-OQ-059` | `IMPLEMENTATION` |
| Pemberitahuan perubahan angka laporan | Pemilik laporan; angka rawat jalan akan turun karena pasien IGD keluar dari hitungan | `IMPLEMENTATION` |
| Approval Registration API owner | Pemilik belum ditunjuk | `IMPLEMENTATION` |

Tidak satu pun memblokir `DESIGN`.

| `IGD-OQ-058` | Open Question | Bagaimana kunjungan IGD dan kunjungan rawat inap dihubungkan sebagai satu rangkaian kedatangan, mengingat `RWI-RULE-029` aturan 2 mewajibkannya tetapi tidak merancangnya, dan manifest Rawat Inap menjanjikan nol perubahan kolom pada tabel modul lain? | Product/Domain Owner IGD + Product/Domain Owner Rawat Inap + Registration API owner | `draft` — memblokir ERD dan menutup `IGD-CONFLICT-005` | — | `IGD-CONFLICT-005`, `RWI-RULE-029` aturan 2 |
| `IGD-OQ-059` | Open Question | Kelas pasien mana yang dipakai kunjungan IGD, dan apakah kelasnya berubah ketika pasien naik ke rawat inap? | Product/Domain Owner IGD + Finance owner + Product/Domain Owner Rawat Inap | `draft` — memblokir implementasi `IGD-DEC-074` | — | Temuan 1 pada `IGD-DEC-074` |

| `IGD-OQ-058` | Open Question | Bagaimana kunjungan IGD dan kunjungan rawat inap dihubungkan sebagai satu rangkaian kedatangan? | Product/Domain Owner IGD + Product/Domain Owner Rawat Inap + Registration API owner | `superseded` oleh `IGD-DEC-075` | — | `IGD-CONFLICT-005` |
| `IGD-DEC-075` | Decision | Rangkaian kedatangan diwujudkan sebagai satu kolom `OriginEncounterId` yang boleh kosong pada `TrxPatientEncounter`, menunjuk kunjungan sebelumnya dalam rangkaian yang sama. Kunjungan rawat inap yang berasal dari IGD mengisinya dengan Id kunjungan IGD. Kolom ini bersifat umum: pola yang sama melayani rangkaian lain, misalnya poliklinik ke rawat inap. Seluruh data lama tetap sah karena kolomnya boleh kosong | Product/Domain Owner IGD, dengan Registration API owner dan Product/Domain Owner Rawat Inap sebagai approver akhir | `draft` — pilihan pengguna jelas; **membatalkan janji "nol perubahan kolom pada tabel modul lain"** pada manifest Rawat Inap revision 3 yang sudah `approved`, sehingga memerlukan kesepakatan Muhammad Hamzah dan revisi manifest itu | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-058`; menutup `IGD-CONFLICT-005`; melaksanakan `RWI-RULE-029` aturan 2 |
| `IGD-CONFLICT-005` | Conflict | Penghubung antara kunjungan IGD dan kunjungan rawat inap diwajibkan `RWI-RULE-029` aturan 2 tetapi tidak dirancang di mana pun | Product/Domain Owner IGD + Product/Domain Owner Rawat Inap + Registration API owner | `resolved` oleh `IGD-DEC-075` | — | Pencarian menyeluruh pada blueprint Rawat Inap |

#### Acceptance criteria awal dari `IGD-DEC-075`

1. Kunjungan rawat inap yang lahir dari disposisi `RANAP` menyimpan Id kunjungan IGD pada
   `OriginEncounterId`.
2. Riwayat pasien dapat menampilkan kunjungan IGD dan kunjungan rawat inap sebagai satu
   rangkaian, dengan satu penggabungan data.
3. Kunjungan yang tidak berasal dari kunjungan lain menyimpan nilai kosong, dan itu sah.
4. Seluruh kunjungan lama tetap terbaca tanpa diubah.
5. Sebuah kunjungan tidak boleh menunjuk dirinya sendiri.
6. Rangkaian tidak boleh membentuk lingkaran.
7. Ketika serah terima gagal dan kunjungan rawat inap tidak jadi dibuat, tidak ada baris
   menggantung yang menunjuk ke mana pun.

#### Dampak pada blueprint Rawat Inap

| Artefak Rawat Inap | Yang perlu berubah |
| --- | --- |
| `blueprint-manifest.md` field `compatibility_impact` | Janji "nol perubahan kolom pada tabel modul lain" tidak lagi benar. Menjadi satu kolom baru pada `TrxPatientEncounter` |
| `RWI-RULE-029` aturan 2 | Ditambahi rincian mekanismenya, yang selama ini kosong |
| `erd/00-context-erd.md` dan kamus data | Kolom `OriginEncounterId` masuk sebagai penghubung |

Perubahan ini **bukan wewenang modul IGD**. Ia diusulkan kepada Product/Domain Owner Rawat
Inap, Muhammad Hamzah, sebagai konsekuensi yang tidak dapat dihindari dari aturan yang sudah
ia kunci sendiri.

#### Bukti tambahan untuk `IGD-OQ-059`

`MstPatientClass` **sudah memiliki** kolom penanda `IsForEmergency`, berdampingan dengan
`IsForOutpatient`, `IsForInpatient`, `IsForIntensiveCare`, dan `IsForNewborn`. Slot untuk
kelas pasien IGD karena itu sudah dirancang sejak awal dan tinggal dipakai.

Yang perlu dicatat: penetapan kelas rawat jalan hari ini **tidak** memakai penanda
`IsForOutpatient`. Ia mencari master berdasarkan **nama yang ditulis tetap di dalam kode**,
yaitu `RAWAT JALAN` pada `PatientEncounterController` baris 55. Bila nama master itu diubah
petugas, pendaftaran rawat jalan langsung gagal.

| `IGD-OQ-059` | Open Question | Kelas pasien untuk kunjungan IGD dan nasibnya saat naik ke rawat inap | Product/Domain Owner IGD + Finance owner + Product/Domain Owner Rawat Inap | `superseded` oleh `IGD-DEC-076` dan `IGD-DEC-077` | — | Temuan 1 pada `IGD-DEC-074` |
| `IGD-DEC-076` | Decision | Kelas pasien kunjungan IGD ditetapkan backend dari master `MstPatientClass` yang bertanda `IsForEmergency` dan `IsDefault`, bukan dari nama yang ditulis tetap di dalam kode dan bukan dari isian petugas. Bila master itu belum ada, tidak aktif, atau ditemukan lebih dari satu, pendaftaran IGD ditolak dengan pesan yang menyebutkan sebabnya. Penetapan kelas rawat jalan yang hari ini memakai nama tertulis-tetap **tidak** ikut diubah oleh keputusan ini | Product/Domain Owner IGD, dengan Finance owner sebagai approver akhir | `draft` — pilihan pengguna jelas; approval Finance owner belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-059`; menutup Temuan 1 pada `IGD-DEC-074`; memakai kolom `MstPatientClass.IsForEmergency` yang sudah ada |
| `IGD-DEC-077` | Decision | Kunjungan rawat inap **tidak mewarisi** kelas pasien dari kunjungan IGD. Kelasnya ditetapkan admisi rawat inap berdasarkan hak penjamin dan ketersediaan kamar. Kelas pada kunjungan IGD tetap tersimpan apa adanya dan tidak berubah surut | Product/Domain Owner IGD, dengan Product/Domain Owner Rawat Inap dan Finance owner sebagai approver akhir | `draft` — pilihan pengguna jelas; approval belum tercatat | — | Jawaban pengguna 24 Agustus 2026; **sudah sesuai `RWI-RULE-029` aturan 3**, sehingga tidak menuntut perubahan apa pun di sisi Rawat Inap |

#### Acceptance criteria awal dari `IGD-DEC-076` dan `IGD-DEC-077`

1. Kunjungan IGD baru memperoleh kelas pasien dari master bertanda `IsForEmergency` dan
   `IsDefault`; `PatientClassId` tidak pernah kosong.
2. Nilai kelas yang dikirim frontend diabaikan, sama seperti perlakuan pada rawat jalan.
3. Bila master kelas IGD belum diisi, pendaftaran IGD ditolak dengan pesan yang menyebutkan
   master mana yang kurang — bukan gagal diam-diam dengan kelas kosong.
4. Bila ditemukan lebih dari satu master bertanda `IsForEmergency` dan `IsDefault`,
   pendaftaran ditolak dan pesannya meminta data master dirapikan.
5. Mengganti nama master kelas IGD tidak membuat pendaftaran IGD gagal.
6. Pendaftaran rawat jalan tidak berubah perilakunya sama sekali.
7. Kunjungan rawat inap asal IGD memiliki kelas yang ditetapkan admisi, dan nilainya boleh
   berbeda dari kelas kunjungan IGD-nya.
8. Kelas pada kunjungan IGD tidak berubah ketika kunjungan rawat inap dibuat.

#### Gerbang data master bertambah satu

Manifest revision 4 mencatat **enam** tabel master IGD yang wajib terisi sebelum modul dapat
dipakai. `IGD-DEC-076` menambah satu lagi:

| No | Master | Keadaan |
| ---: | --- | --- |
| 7 | `MstPatientClass` dengan `IsForEmergency = true` dan `IsDefault = true` | **Baru** — belum ada satu pun baris seperti ini, dan `EmergencyMasterDataSeeder` belum mengisinya |

Tanpa baris itu, pendaftaran IGD berhenti total setelah `IGD-DEC-074` dijalankan. Karena itu
pengisian master ini **wajib berada dalam task yang sama** dengan perubahan tipe kunjungan,
bukan task terpisah sesudahnya.

| `IGD-OQ-060` | Open Question | Apa yang terjadi pada pesanan yang belum selesai — obat belum diberikan, tindakan belum dikerjakan, pemeriksaan penunjang belum keluar hasilnya — ketika pasien meninggalkan IGD? | Product/Domain Owner IGD + Clinical Governance + pemilik `PharmacyManagement` + Product/Domain Owner Rawat Inap | `draft` — memblokir `IGD-GAP-026` dan gerbang penutupan kunjungan | — | F-10, `IGD-GAP-026`, `ValidateVisitClosureAsync` |

| `IGD-OQ-060` | Open Question | Perlakuan pesanan yang belum selesai ketika pasien meninggalkan IGD | Product/Domain Owner IGD + Clinical Governance + pemilik `PharmacyManagement` + Product/Domain Owner Rawat Inap | `superseded` oleh `IGD-DEC-078` | — | F-10, `IGD-GAP-026` |
| `IGD-DEC-078` | Decision | Sebelum serah terima diajukan, sistem menampilkan seluruh pesanan yang belum selesai pada kunjungan IGD. Setiap pesanan wajib diberi salah satu dari tiga sikap oleh petugas berwenang: **sudah dikerjakan**, **dibatalkan** dengan alasan, atau **diteruskan** ke unit penerima. Pesanan yang diteruskan muncul sebagai tugas pada unit penerima. Daftar sikap ini menjadi bagian isi serah terima. Kewajiban ini **tidak pernah menahan kepergian fisik pasien**: bila petugas belum bersikap, yang tertahan hanya pengajuan dokumen serah terima, sementara rangkaian fisik tetap boleh berjalan | Product/Domain Owner IGD, dengan Clinical Governance, pemilik `PharmacyManagement`, dan Product/Domain Owner Rawat Inap sebagai approver akhir | `draft` — pilihan pengguna jelas; approval keempat pihak belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-060`; menutup `IGD-GAP-026`; konsisten dengan `IGD-DEC-055`, `IGD-DEC-070`, dan `IGD-DEC-021` |

#### Cakupan `IGD-DEC-078` menurut apa yang benar-benar ada di source

Keputusan ini menyebut tiga jenis pesanan. Hanya dua di antaranya punya tempat di source hari
ini:

| Jenis pesanan | Ada di source? | Yang dipakai memeriksa |
| --- | --- | --- |
| Obat yang belum diberikan | **Sebagian** | `TrxPrescription` beserta status pemenuhannya. Catatan pemberian obat kepada pasien belum ada sama sekali — `IGD-GAP-025` |
| Tindakan yang belum dikerjakan | **Ya** | `TrxPatientProcedure` beserta statusnya |
| Pemeriksaan penunjang yang belum keluar hasilnya | **Tidak** | Modul Laboratorium dan Radiologi **belum ada** — `IGD-GAP-024`, `F-17` |

Karena itu penerapan `IGD-DEC-078` dilakukan bertahap. Yang dapat dikerjakan sekarang adalah
obat dan tindakan. Bagian penunjang menunggu modul Diagnostic Services dan dicatat sebagai
`LATER SLICE`, bukan sebagai bagian yang dilupakan.

Perbedaan "sudah diresepkan" dan "sudah diberikan" juga belum dapat ditegakkan sepenuhnya
selama `IGD-GAP-025` terbuka. Yang terbaca hari ini hanya sampai "sudah diserahkan farmasi",
bukan "sudah masuk ke tubuh pasien".

#### Acceptance criteria awal dari `IGD-DEC-078`

1. Daftar pesanan yang belum selesai muncul sebelum serah terima diajukan, dan memuat obat
   serta tindakan yang statusnya belum tuntas.
2. Setiap pesanan pada daftar itu wajib memiliki tepat satu sikap sebelum dokumen serah
   terima dapat diajukan.
3. Pembatalan pesanan wajib menyimpan alasan, pelaku, dan waktu server.
4. Pesanan yang diteruskan terlihat pada unit penerima sebagai tugas, bukan hanya sebagai
   teks pada ringkasan serah terima.
5. Rangkaian fisik `Disiapkan`, `Berangkat`, dan `Tiba` tetap dapat dijalankan walaupun
   belum satu pun pesanan diberi sikap.
6. Daftar sikap tersimpan sebagai bagian catatan serah terima dan dapat dibaca kembali
   sesudahnya.
7. Pesanan yang sudah diberi sikap tidak dapat berubah sikap tanpa jejak; perubahan
   bersifat append-only.
8. Status tagihan tidak diperiksa sama sekali, sesuai `IGD-DEC-021`.

#### Blocker yang muncul

| Blocker | Menunggu | Memblokir |
| --- | --- | --- |
| Bentuk "diteruskan" untuk obat | Pemilik `PharmacyManagement` — apakah resep IGD dipindahkan ke kunjungan rawat inap atau tetap menempel pada kunjungan IGD tetapi dikerjakan unit penerima | `IMPLEMENTATION` |
| Bentuk daftar tugas pada unit penerima | Product/Domain Owner Rawat Inap — apakah memakai daftar pantau `InpEpisode` yang sudah ada | `IMPLEMENTATION` |
| Bagian pemeriksaan penunjang | Modul Diagnostic Services belum ada | `LATER SLICE` |
| Pembedaan "diberikan" dari "diserahkan" | `IGD-GAP-025`, catatan pemberian obat belum ada | `LATER SLICE` |

| `IGD-OQ-061` | Open Question | Apa isi minimum serah terima klinis dari IGD, dan apakah serah terima perawat dan serah terima dokter dipisahkan? | Product/Domain Owner IGD + Nursing authority + Clinical Governance | `draft` — memblokir `IGD-GAP-019` dan desain isi serah terima | — | F-14, `IGD-DEC-061`, `IGD-DEC-078`, `IGD-REG-003` |

| `IGD-OQ-061` | Open Question | Isi minimum serah terima klinis dari IGD dan pemisahan serah terima perawat dan dokter | Product/Domain Owner IGD + Nursing authority + Clinical Governance | `superseded` oleh `IGD-DEC-079` | — | F-14, `IGD-DEC-061`, `IGD-REG-003` |
| `IGD-DEC-079` | Decision | Serah terima klinis dari IGD memakai bentuk terstruktur SBAR dengan **empat bagian wajib diisi petugas** — kondisi saat ini, ringkasan perjalanan di IGD, masalah aktif beserta tingkat kegawatan terakhir, dan yang harus dilanjutkan — ditambah **tiga bagian yang diisi sistem** dari data yang sudah ada, yaitu daftar sikap pesanan sesuai `IGD-DEC-078`, alergi beserta risiko utama, dan tanda vital terakhir. Bentuk ini berlaku sama untuk seluruh tujuan, termasuk bangsal, ICU, kamar operasi, dan kamar jenazah. Serah terima perawat dan serah terima dokter **tidak** dipisahkan menjadi dua dokumen pada rilis pertama; keduanya mengisi dokumen yang sama | Product/Domain Owner IGD, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna jelas; approval keperawatan dan klinis belum tercatat, dan `IGD-REG-003` masih `HOSPITAL_SOP_REQUIREMENT` yang belum diverifikasi ke SOP MMC | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-061`; menutup `IGD-GAP-019`; memberi arti pada peninjauan yang diwajibkan `IGD-DEC-061` |

#### Bentuk yang dikunci `IGD-DEC-079`

| Bagian | Diisi oleh | Wajib | Sumber |
| --- | --- | --- | --- |
| Kondisi pasien saat ini | Petugas | Ya | Ditulis petugas |
| Ringkasan perjalanan di IGD | Petugas | Ya | Ditulis petugas |
| Masalah aktif dan tingkat kegawatan terakhir | Petugas | Ya | Ditulis petugas, tingkat kegawatan disalin dari penilaian triase terakhir |
| Yang harus dilanjutkan | Petugas | Ya | Ditulis petugas |
| Daftar sikap pesanan | Sistem | Otomatis | `IGD-DEC-078` |
| Alergi dan risiko utama | Sistem | Otomatis | Pengkajian keperawatan dan `TrxPatientAllergy` |
| Tanda vital terakhir | Sistem | Otomatis | `TrxPatientVitalSign` terbaru pada kunjungan itu |

#### Pengecualian untuk pasien yang paling gawat

`IGD-DEC-055` menetapkan dokumentasi tidak boleh menunda tindakan penyelamatan, dan
`IGD-DEC-056` menetapkan bagian yang tidak dapat dinilai wajib disertai alasan, bukan
dikosongkan diam-diam. Kedua aturan itu berlaku di sini:

1. Untuk pasien tingkat kegawatan tertinggi, empat bagian wajib tetap **diminta**, tetapi
   petugas boleh menandai sebuah bagian sebagai tidak dapat diisi saat itu beserta alasannya.
2. Penandaan itu **tidak** menahan rangkaian fisik `Disiapkan`, `Berangkat`, dan `Tiba`,
   sesuai `IGD-DEC-070` dan `IGD-DEC-078`.
3. Bagian yang ditandai tidak dapat diisi wajib dilengkapi setelah pasien stabil, dan
   sampai saat itu serah terima tetap tercatat sebagai belum lengkap.
4. Sistem **tidak boleh** mengisi bagian klinis apa pun secara otomatis hanya untuk
   meloloskan validasi, sesuai `IGD-DEC-056`.

#### Acceptance criteria awal dari `IGD-DEC-079`

1. Dokumen serah terima tidak dapat diajukan bila salah satu dari empat bagian wajib kosong
   dan tidak ditandai sebagai tidak dapat diisi.
2. Bagian yang ditandai tidak dapat diisi wajib menyimpan alasan, pelaku, dan waktu server.
3. Tiga bagian otomatis terisi tanpa petugas mengetik ulang, dan nilainya diambil pada saat
   serah terima diajukan, bukan dihitung ulang setiap kali dibaca.
4. Tingkat kegawatan pada bagian masalah aktif berasal dari penilaian triase terakhir yang
   berstatus selesai, bukan dari penilaian yang sudah `Superseded` atau `Cancelled`.
5. Penerima melihat ketujuh bagian sebagai daftar yang dapat diperiksa satu per satu.
6. Penolakan serah terima wajib menyebutkan bagian mana yang dianggap kurang.
7. Rangkaian fisik tetap dapat berjalan walaupun dokumen serah terima belum lengkap.
8. Serah terima yang belum lengkap muncul pada daftar pantau sebagai pekerjaan yang belum
   tuntas, sesuai `IGD-DEC-062`.
9. Satu dokumen serah terima menampung isian perawat dan dokter; tidak ada dokumen kedua.

#### Catatan lintas modul

`RWI-OQ-038` milik Rawat Inap — isi serah terima antar shift di bangsal — **masih terbuka**
di sisinya. Bentuk SBAR yang dikunci di sini sebaiknya ditawarkan sebagai bahan jawaban
`RWI-OQ-038`, supaya rumah sakit tidak berakhir dengan dua bentuk serah terima yang berbeda
untuk kejadian yang sejenis.

| `IGD-OQ-062` | Open Question | Seberapa dalam jejak audit perubahan catatan klinis IGD harus disimpan, mengingat hari ini hanya penulis terakhir yang tercatat dan nilai sebelumnya hilang? | Product/Domain Owner IGD + Security/Privacy owner + Clinical Governance | `draft` — memblokir `IGD-GAP-020` dan penegakan `IGD-DEC-058`, `IGD-DEC-059`, `IGD-DEC-066` | — | F-16, `IGD-REG-002` |

| `IGD-OQ-062` | Open Question | Kedalaman jejak audit perubahan catatan klinis IGD | Product/Domain Owner IGD + Security/Privacy owner + Clinical Governance | `superseded` oleh `IGD-DEC-080` | — | F-16, `IGD-REG-002` |
| `IGD-DEC-080` | Decision | Catatan klinis IGD **tidak diubah di tempat**. Setiap koreksi membuat baris baru yang menunjuk baris yang dikoreksi; baris lama ditandai bukan nilai efektif tetapi isinya tetap utuh dan tidak pernah ditimpa. Aturan ini berlaku untuk pengkajian, tanda vital, catatan perkembangan, penilaian triase, dan event kepergian. Tabel non-klinis seperti master dan pengaturan tetap memakai `IdentityModel` apa adanya. Tidak ada penghapusan permanen pada catatan klinis | Product/Domain Owner IGD, dengan Security/Privacy owner dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna jelas; approval keamanan dan klinis belum tercatat, dan `IGD-REG-002` masih `REGULATORY_VERIFICATION_REQUIRED` | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-062`; menutup `IGD-GAP-020`; memungkinkan penegakan `IGD-DEC-058`, `IGD-DEC-059`, dan `IGD-DEC-066` |

#### Pola ini sudah terbukti di repository yang sama

`IGD-DEC-080` **bukan pola baru**. Ia memperluas cara kerja retriase yang sudah berjalan:

| Unsur pola | Sudah ada pada retriase | Diperluas ke |
| --- | --- | --- |
| Baris lama tidak ditimpa | `TrxEmergencyTriage` lama hanya berubah status menjadi `Superseded` | Pengkajian, tanda vital, catatan perkembangan, event kepergian |
| Baris baru menunjuk baris lama | `PreviousTriageId` | Kolom penunjuk sejenis pada tabel klinis lain |
| Satu transaksi | `RetriageAsync` menyimpan keduanya dalam satu `SaveChangesAsync` | Seluruh koreksi klinis |
| Kolom klinis lama tidak disentuh | Hanya status dan jejak audit yang berubah | Sama |

Karena itu risikonya lebih rendah daripada memperkenalkan mekanisme yang belum pernah
dipakai di sini.

#### Acceptance criteria awal dari `IGD-DEC-080`

1. Mengoreksi catatan klinis menghasilkan baris baru; jumlah baris bertambah, tidak tetap.
2. Baris yang dikoreksi tetap dapat dibaca lengkap beserta seluruh nilai klinis aslinya.
3. Tepat satu baris berstatus nilai efektif untuk setiap catatan pada satu waktu.
4. Baris baru menyimpan penunjuk ke baris yang dikoreksi, pelaku, waktu server, dan alasan.
5. Pembuatan baris koreksi dan penandaan baris lama terjadi dalam satu transaksi; mustahil
   ada keadaan baris lama sudah ditandai sementara penggantinya gagal tersimpan.
6. Riwayat lengkap sebuah catatan dapat ditampilkan berurutan menurut waktu.
7. Tidak ada endpoint yang menghapus permanen catatan klinis.
8. Pembacaan biasa hanya mengembalikan nilai efektif, sehingga layar yang sudah ada tidak
   menampilkan nilai lama sebagai fakta klinis.
9. Referensi dari catatan lain tetap dapat ditelusuri ke catatan asal maupun ke koreksinya,
   sesuai `IGD-DEC-059`.

#### Contoh yang menjelaskan kenapa ini penting

Perawat A mencatat tekanan darah 120/80 pukul 20:00. Perawat B mengoreksinya menjadi 90/60
pukul 20:15. Perawat C mengoreksi lagi menjadi 100/70 pukul 20:30.

| Yang tersimpan hari ini | Yang tersimpan setelah `IGD-DEC-080` |
| --- | --- |
| Satu baris: 100/70, diubah terakhir oleh Perawat C pukul 20:30 | Tiga baris: 120/80 oleh A, 90/60 oleh B, 100/70 oleh C — hanya baris ketiga berstatus efektif |

Pada pasien yang memburuk, urutan 120/80 lalu 90/60 adalah informasi klinis yang paling
menentukan. Hari ini urutan itu hilang tanpa jejak.

#### Blocker yang muncul

| Blocker | Menunggu | Memblokir |
| --- | --- | --- |
| Approval Security/Privacy owner | Pemilik belum ditunjuk | `IMPLEMENTATION` |
| Verifikasi `IGD-REG-002` | Rujukan regulasi rekam medis elektronik yang berlaku, belum diverifikasi siapa pun | `IMPLEMENTATION` |
| Kebijakan masa simpan riwayat | `RWI-OQ-035` di sisi Rawat Inap menanyakan hal sejenis dan masih terbuka | `LATER SLICE` |
| Migration penambahan kolom penanda | Basis data dipakai bersama satu tim | `IMPLEMENTATION` |

| `IGD-OQ-063` | Open Question | Cara hak akses mengenal unit pelayanan | Product/Domain Owner IGD + Security/Privacy owner + pemilik Corporate/HR | `superseded` oleh `IGD-DEC-081` | — | F-15, laporan `BE-IGD-010` |
| `IGD-DEC-081` | Decision | Hubungan pengguna ke unit pelayanan dibuat sebagai **tabel penugasan tersendiri**, berisi pengguna, unit pelayanan, berlaku sejak, berlaku sampai, dan siapa yang menugaskan. Penjaga kewenangan unit ditulis **di dalam service IGD**, bukan di mesin hak akses `SysAccessPolicy`. Setiap endpoint yang menuntut kewenangan unit wajib memanggilnya. Struktur organisasi dan mesin hak akses yang dipakai seluruh aplikasi **tidak disentuh** | Product/Domain Owner IGD, dengan Security/Privacy owner dan pemilik Corporate/HR sebagai approver akhir | `draft` — pilihan pengguna jelas; approval belum tercatat dan pengisian data penugasan untuk petugas yang sudah ada adalah keputusan organisasi | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-063`; menutup `IGD-GAP-021` dan membuka jalan `BE-IGD-010` yang selama ini terhalang desain; memakai pola yang sama dengan `RWI-RULE-030` aturan 6 |
| `IGD-OQ-064` | Open Question | Cara mencatat penetapan dokter pemeriksa IGD | Product/Domain Owner IGD + Clinical Governance | `superseded` oleh `IGD-DEC-082` | — | F-6, `IGD-GAP-022` |
| `IGD-DEC-082` | Decision | Penetapan dokter pemeriksa IGD dicatat pada **tabel riwayat penugasan** berisi dokter, berlaku sejak, berakhir kapan, siapa yang menugaskan, dan alasannya. Pada satu waktu tepat satu dokter aktif untuk satu kunjungan IGD. Baris lama diberi waktu berakhir dan **tidak pernah ditimpa**. `TrxPatientEncounter.DoctorId` tetap diisi sebagai nilai efektif supaya layar dan laporan yang sudah ada tidak rusak | Product/Domain Owner IGD, dengan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna jelas; approval klinis belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-064`; menutup `IGD-GAP-022`; menegakkan `IGD-DEC-073`; sejalan dengan `IGD-DEC-080`; bentuknya sama dengan `RWI-RULE-030` |
| `IGD-OQ-065` | Open Question | Perilaku sistem ketika pemicu pengkajian ulang terpenuhi tetapi pengkajian ulang belum dilakukan | Product/Domain Owner IGD + Nursing authority + Clinical Governance | `superseded` oleh `IGD-DEC-083` | — | `IGD-DEC-060`, `IGD-GAP-023` |
| `IGD-DEC-083` | Decision | Pemicu pengkajian ulang yang sudah terpenuhi tetapi belum ditindaklanjuti ditampilkan sebagai **daftar pantau**, dan **tidak pernah memblokir** tindakan klinis maupun keputusan tindak lanjut. Perhitungannya memakai pola yang sama dengan pemantau pelampauan batas waktu triase yang sudah berjalan. Interval yang belum disahkan SOP ditandai belum tersedia dan **tidak boleh** dianggap patuh maupun terlambat secara otomatis | Product/Domain Owner IGD, dengan Nursing authority dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna jelas; approval belum tercatat dan nilai interval menunggu SOP MMC | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-065`; menutup `IGD-GAP-023`; menegakkan `IGD-DEC-060`; memperluas pola `EmergencyTriageSlaMonitorHostedService` |

#### Acceptance criteria awal dari `IGD-DEC-081`

1. Petugas yang tidak memiliki penugasan aktif pada unit tujuan ditolak ketika mencoba
   mencatat event `Tiba` atau menerima serah terima untuk unit itu.
2. Penolakan itu memakai kode 403 beserta pesan yang menyebutkan sebabnya.
3. Penugasan yang sudah berakhir waktunya tidak lagi memberi kewenangan.
4. Memiliki penugasan unit **tidak dengan sendirinya** memberi capability klinis, dan
   memiliki capability **tidak** melewati batas penugasan unit, sesuai `IGD-DEC-058`.
5. Mesin hak akses `SysAccessPolicy` dan struktur organisasi tidak berubah perilakunya.
6. Tersedia aturan peralihan yang disengaja untuk keadaan tabel penugasan masih kosong,
   sehingga penerapannya tidak mengunci seluruh petugas sekaligus.
7. Pelayanan klinis darurat **tidak pernah** diblokir oleh ketiadaan penugasan.

#### Acceptance criteria awal dari `IGD-DEC-082`

1. Menetapkan dokter kedua tidak menghapus jejak dokter pertama.
2. Sistem dapat menjawab siapa dokter penanggung jawab kunjungan IGD **pada waktu tertentu**,
   bukan hanya sekarang.
3. Tepat satu dokter aktif untuk satu kunjungan IGD pada satu waktu.
4. Pergantian dokter menyimpan alasan, pelaku, dan waktu server.
5. `TrxPatientEncounter.DoctorId` selalu sama dengan dokter yang sedang aktif.
6. Layar dan laporan yang membaca `DoctorId` tidak berubah perilakunya.
7. Pencabutan dokter tanpa pengganti memiliki aturan yang disengaja, bukan menghasilkan
   kunjungan tanpa dokter secara diam-diam.

#### Acceptance criteria awal dari `IGD-DEC-083`

1. Daftar pantau memuat pasien yang pemicu pengkajian ulangnya sudah terpenuhi tetapi
   pengkajian ulangnya belum ada.
2. Pasien yang intervalnya belum dikonfigurasi SOP ditampilkan dengan penanda **belum
   tersedia**, bukan disembunyikan dan bukan dianggap patuh.
3. Tidak ada satu pun endpoint klinis yang menolak permintaan karena pengkajian ulang
   tertunggak.
4. Keputusan tindak lanjut tetap dapat dijalankan walaupun pengkajian ulang tertunggak.
5. Penanda tertunggak hilang dari daftar setelah pengkajian ulang dilakukan, tanpa riwayat
   ketertunggakannya dihapus.
6. Pemicu berbasis kejadian — setelah intervensi, saat kondisi berubah, saat serah terima,
   sebelum tindak lanjut — dihitung terpisah dari pemicu berbasis interval.

#### Catatan penting untuk `IGD-DEC-081`

Laporan `BE-IGD-010` tanggal 18 Agustus 2026 berstatus **terhalang desain** justru karena
keputusan ini belum ada. `IGD-DEC-081` membuka jalannya. Yang tetap **bukan** pekerjaan kode
adalah pengisian data penugasan untuk seluruh petugas yang sudah ada — itu keputusan
organisasi dan harus diselesaikan sebelum penjagaan dinyalakan di produksi.

| `IGD-OQ-066` | Open Question | Cara mencegah satu pasien memiliki dua episode IGD aktif | Product/Domain Owner IGD + Registration API owner | `superseded` oleh `IGD-DEC-084` | — | F-4, `IGD-GAP-029` |
| `IGD-DEC-084` | Decision | Pendaftaran IGD ditolak selama pasien yang sama masih memiliki kunjungan IGD yang belum `Completed` dan belum `Cancelled`. Pesan penolakan **wajib menyebutkan nomor kunjungan yang sudah ada** beserta cara membukanya, sehingga petugas melanjutkan ke kunjungan itu alih-alih mengulang pendaftaran. Tersedia jalan keluar beralasan untuk keadaan yang sah, dan pemakaian jalan keluar itu tercatat | Product/Domain Owner IGD, dengan Registration API owner sebagai approver akhir | `draft` — pilihan pengguna jelas; approval belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-066`; menutup `IGD-GAP-029`; sejajar dengan `RWI-DEC-054` dan `INV-INP-10` milik Rawat Inap |
| `IGD-OQ-051` | Open Question | Perlakuan terhadap unit, petugas, dan catatan downstream yang telah memakai event `Arrived` sebelum event itu dikoreksi atau dibalik | Product/Domain Owner IGD + Nursing authority + Clinical Governance + Integration owner | `superseded` oleh `IGD-DEC-085` | — | Tindak lanjut `IGD-DEC-066`; **butir terakhir yang tersisa dari Amendment Pass 2026-08-20** |
| `IGD-DEC-085` | Decision | Koreksi atau pembalikan event `Arrived` **wajib memberitahu** unit yang terlanjur menerima tanggung jawab klinis dan petugas yang membuat catatan turunan. Catatan klinis yang sudah ditulis **tidak diubah dan tidak dihapus**; catatan itu hanya diberi penanda bahwa dasar kepemilikannya dikoreksi, beserta tautan ke event pengganti yang sah. Penilaian apakah isi catatan masih berlaku diserahkan kepada petugas yang menulisnya, bukan kepada sistem. Penanda yang belum ditindaklanjuti muncul pada daftar pantau | Product/Domain Owner IGD, dengan Nursing authority, Clinical Governance, dan Integration owner sebagai approver akhir | `draft` — pilihan pengguna jelas; approval belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-051`; melaksanakan penutup `IGD-DEC-066` "koreksi tidak boleh dilakukan diam-diam"; sejalan dengan `IGD-DEC-080` |

#### Acceptance criteria awal dari `IGD-DEC-084`

1. Pendaftaran IGD kedua untuk pasien yang kunjungan IGD-nya masih aktif ditolak.
2. Pesan penolakan memuat nomor kunjungan yang sudah ada dan waktu kedatangannya.
3. Pasien yang kunjungan IGD sebelumnya sudah `Completed` atau `Cancelled` dapat
   didaftarkan kembali tanpa hambatan.
4. Pasien tanpa identitas yang belum tertaut ke data pasien definitif tidak ikut tertolak
   oleh aturan ini, karena belum dapat dipastikan orang yang sama.
5. Pemakaian jalan keluar beralasan menyimpan alasan, pelaku, dan waktu server.
6. Aturan ini **tidak** menahan penanganan klinis; yang tertahan hanya pembuatan kunjungan
   kedua.

#### Acceptance criteria awal dari `IGD-DEC-085`

1. Koreksi waktu, koreksi pasien, dan pembalikan event `Arrived` mengirim pemberitahuan ke
   unit yang terdampak.
2. Petugas yang membuat catatan turunan setelah event yang dikoreksi ikut diberi tahu.
3. Catatan klinis turunan tetap utuh isinya; tidak satu pun kolom klinisnya berubah.
4. Catatan turunan memperoleh penanda yang menyebutkan bahwa dasar kepemilikannya
   dikoreksi, beserta tautan ke event pengganti.
5. Penanda itu muncul pada daftar pantau sampai petugas yang berwenang menyatakan
   sikapnya.
6. Event asli tetap terlihat sebagai tidak berlaku dan tertaut ke penggantinya, sesuai
   `IGD-DEC-066`.
7. Pemberitahuan yang gagal terkirim tidak membatalkan koreksinya, tetapi tercatat sebagai
   pekerjaan yang belum tuntas.

---

### Penutupan Amendment Pass 2026-08-24

#### Keputusan baru pada pass ini

Sembilan belas keputusan, `IGD-DEC-067` sampai `IGD-DEC-085`. Seluruhnya berstatus `draft`.
**Tidak satu pun `approved`**, karena tidak satu pun pemilik berwenang yang tercatat namanya
memberikan persetujuan formal dalam sesi ini.

| Kelompok | Keputusan |
| --- | --- |
| Bentuk episode dan penghubungnya | `IGD-DEC-067`, `IGD-DEC-074`, `IGD-DEC-075`, `IGD-DEC-084` |
| Pencatatan klinis IGD | `IGD-DEC-068`, `IGD-DEC-080` |
| Kepergian pasien dan serah terima | `IGD-DEC-069`, `IGD-DEC-070`, `IGD-DEC-071`, `IGD-DEC-078`, `IGD-DEC-079`, `IGD-DEC-085` |
| Kepemilikan pasien | `IGD-DEC-072`, `IGD-DEC-073` |
| Kelas pasien | `IGD-DEC-076`, `IGD-DEC-077` |
| Kewenangan dan pemantauan | `IGD-DEC-081`, `IGD-DEC-082`, `IGD-DEC-083` |

#### Konflik yang tertutup

| ID | Keadaan |
| --- | --- |
| `IGD-CONFLICT-003` | `resolved` oleh `IGD-DEC-074` |
| `IGD-CONFLICT-004` | `resolved` oleh `IGD-DEC-068` |
| `IGD-CONFLICT-005` | `resolved` oleh `IGD-DEC-075` |

#### Kesenjangan yang tertutup keputusan

`IGD-GAP-011`, `IGD-GAP-012`, `IGD-GAP-013`, `IGD-GAP-015`, `IGD-GAP-017`, `IGD-GAP-018`,
`IGD-GAP-019`, `IGD-GAP-020`, `IGD-GAP-021`, `IGD-GAP-022`, `IGD-GAP-023`, `IGD-GAP-026`,
`IGD-GAP-029`, `IGD-GAP-030`, `IGD-GAP-032`.

#### Kesenjangan yang sengaja tidak ditanyakan

| ID | Alasan | Sifat |
| --- | --- | --- |
| `IGD-GAP-014` | Status kunjungan dapat mundur dan kunjungan tertutup dapat terbuka kembali. Ini **cacat perilaku murni**, bukan pertanyaan bisnis: `EmergencyVisitService.CanTransition` sudah menyatakan aturan yang benar, tetapi `EmergencyTriageController` menulis `VisitStatus` secara langsung tanpa memanggilnya. Tidak ada keputusan owner yang dibutuhkan untuk memperbaikinya | Perbaikan, bukan keputusan |
| `IGD-GAP-016` | Modul rawat inap milik blueprint `RWI-BP-001` yang sudah `approved`. Bukan pekerjaan IGD | Milik modul lain |
| `IGD-GAP-024` | Laboratorium dan radiologi belum ada modulnya sama sekali. Menanyakan aturannya sekarang berarti merancang modul yang bukan scope pass ini | `LATER SLICE` |
| `IGD-GAP-025` | Catatan pemberian obat menyentuh `PharmacyManagement`. Menunggu pemilik modul itu ditunjuk | `LATER SLICE` |
| `IGD-GAP-027` | Bentuk terstruktur primary survey ABCDE. `IGD-DEC-057` sudah mewajibkan hasil ABCDE sebagai data inti; bentuk kolomnya adalah pekerjaan desain, bukan keputusan owner | Desain |
| `IGD-GAP-028` | Pendaftaran dan kunjungan IGD tidak atomik. Perbaikan teknis; kode sudah menahan hasil langkah pertama sehingga tidak menghasilkan encounter kembar selama layar tidak ditutup | Perbaikan, bukan keputusan |
| `IGD-GAP-031` | Empat pengaturan IGD yang tersimpan tetapi tidak menjalankan apa pun. Perlu diputuskan apakah dijalankan atau dicabut — **layak menjadi pertanyaan pass berikutnya**, tidak memblokir desain | Terbuka |
| `IGD-GAP-033` | Tidak ada proyek test di backend. Sudah menjadi kewajiban lewat `RWI-DEC-051` di sisi Rawat Inap dan menjadi blocker `IGD-DEC-068` | Blocker implementasi |

#### Blocker yang tersisa

Tidak satu pun memblokir `DESIGN`. Seluruhnya memblokir `IMPLEMENTATION`.

| Blocker | Menunggu | Keputusan terdampak |
| --- | --- | --- |
| Pemilik `ClinicalManagement` dan `PharmacyManagement` belum ditunjuk | Organisasi. Sama dengan `DEC-INP-001` milik Rawat Inap — **ajukan sebagai satu permintaan** | `IGD-DEC-068`, `IGD-DEC-078` |
| Persetujuan Muhammad Hamzah atas revisi `RWI-RULE-026` aturan 6 dan manifest `compatibility_impact` | Product/Domain Owner Rawat Inap | `IGD-DEC-068`, `IGD-DEC-071`, `IGD-DEC-075` |
| Persetujuan pemilik `EmergencyInstallationManagement` atas `RWI-OQ-034` dan `DEC-INP-002` | **Pemilik IGD, yaitu pihak yang menjawab pass ini** — namanya belum tercatat | `IGD-DEC-067`, dan slice `INP-S09` milik Rawat Inap |
| Otorisasi migration pada basis data yang dipakai bersama satu tim | Pemilik basis data pengembangan | `IGD-DEC-074`, `IGD-DEC-075`, `IGD-DEC-080`, `IGD-DEC-082` |
| Data master kelas pasien IGD | Penanggung jawab data master | `IGD-DEC-076` |
| Pengisian data penugasan unit untuk petugas yang sudah ada | Organisasi | `IGD-DEC-081` |
| Nilai batas waktu dan interval dari SOP MMC | SOP triase dan SOP pengkajian ulang | `IGD-DEC-063`, `IGD-DEC-083`, `IGD-REG-004`, `IGD-REG-005` |
| Approval Clinical Governance, Nursing authority, dan Security/Privacy | Ketiganya belum ditunjuk | Hampir seluruh keputusan pass ini |
| Verifikasi `IGD-REG-002` dan `IGD-REG-006` | Rujukan regulasi yang berlaku; **belum diverifikasi siapa pun dalam sesi ini** | `IGD-DEC-080` |
| Proyek test backend belum ada | `IGD-GAP-033` | `IGD-DEC-068` dan seluruh perubahan yang menyentuh modul tetangga |

#### Yang wajib disampaikan kepada pemilik modul Rawat Inap

Tiga keputusan pass ini menuntut perubahan pada blueprint `RWI-BP-001` revision 3 yang sudah
`approved`. Perubahan itu **bukan wewenang modul IGD** dan diusulkan, bukan diberlakukan:

| Usulan | Artefak Rawat Inap yang terdampak | Sebab |
| --- | --- | --- |
| Perluas pelonggaran `RWI-RULE-026` ke kunjungan bertipe `Emergency`; aturan 6 direvisi | `00-interview-decisions.md`, `02-backend-architecture.md`, kontrak validasi | `IGD-DEC-068` — tanpa ini pengkajian IGD tetap mustahil disimpan |
| Ubah `compatibility_impact` pada manifest: bukan lagi nol perubahan kolom pada tabel modul lain | `blueprint-manifest.md` | `IGD-DEC-075` — `RWI-RULE-029` aturan 2 mustahil dijalankan tanpa kolom penghubung |
| `InpBedPlacement` membaca waktu tiba dari catatan kepergian IGD, tidak menetapkan sendiri | `02-backend-architecture.md`, kontrak API, `INP-S01` | `IGD-DEC-071` |
| Tawarkan bentuk SBAR `IGD-DEC-079` sebagai bahan jawaban `RWI-OQ-038` | `00-interview-decisions.md` | Supaya rumah sakit tidak punya dua bentuk serah terima berbeda |

#### Status gerbang untuk langkah berikutnya

| Gerbang | Keadaan |
| --- | --- |
| Conflict lintas blueprint tertutup | **Ya** — ketiganya `resolved` |
| Keputusan yang memblokir desain tertutup | **Ya** — tidak ada `IGD-OQ` yang berstatus memblokir `DESIGN` |
| Approval formal owner tercatat | **Tidak** — seluruh keputusan `draft` |
| Boleh lanjut ke `/qv-design` | **Ya, dengan catatan** — desain boleh disusun di atas keputusan `draft`, tetapi blueprint hasilnya tidak boleh ditandai `approved` sebelum owner nyata menyetujui |
| Boleh lanjut ke implementasi | **Tidak** — sepuluh blocker implementasi masih terbuka |

#### Status akhir seluruh Open Question setelah pass ini

Setiap `IGD-OQ` pada pass ini muncul **dua kali** di dokumen: sekali ketika diajukan dengan
status `draft`, dan sekali lagi ketika ditutup dengan status `superseded`. Baris kedua yang
berlaku. Tabel di bawah adalah rujukan tunggal agar tidak salah baca.

| ID | Pokok | Status akhir | Ditutup oleh |
| --- | --- | --- | --- |
| `IGD-OQ-051` | Propagasi koreksi event `Arrived` ke downstream | `superseded` | `IGD-DEC-085` |
| `IGD-OQ-052` | Bentuk episode saat pasien IGD masuk rawat inap | `superseded` | `IGD-DEC-067` |
| `IGD-OQ-053` | Pelonggaran `QueueId` dan `ConsultationId` | `superseded` | `IGD-DEC-068` |
| `IGD-OQ-054` | Peran `TrxEmergencyTransfer` setelah jalur `RANAP` pindah ke Rawat Inap | `superseded` | `IGD-DEC-069` |
| `IGD-OQ-055` | Kejadian kepergian yang wajib dibedakan dan sumber kebenaran waktu tiba | `superseded` | `IGD-DEC-070`, `IGD-DEC-071` |
| `IGD-OQ-056` | Pemilik klinis saat `Berangkat` dan dokter penanggung jawab sebelum DPJP ranap | `superseded` | `IGD-DEC-072`, `IGD-DEC-073` |
| `IGD-OQ-057` | Perubahan kunjungan IGD menjadi `EncounterType.Emergency` | `superseded` | `IGD-DEC-074` |
| `IGD-OQ-058` | Penghubung kunjungan IGD dan kunjungan rawat inap | `superseded` | `IGD-DEC-075` |
| `IGD-OQ-059` | Kelas pasien IGD dan nasibnya saat naik ke rawat inap | `superseded` | `IGD-DEC-076`, `IGD-DEC-077` |
| `IGD-OQ-060` | Pesanan yang belum selesai saat pasien meninggalkan IGD | `superseded` | `IGD-DEC-078` |
| `IGD-OQ-061` | Isi minimum serah terima klinis | `superseded` | `IGD-DEC-079` |
| `IGD-OQ-062` | Kedalaman jejak audit catatan klinis | `superseded` | `IGD-DEC-080` |
| `IGD-OQ-063` | Hak akses mengenal unit pelayanan | `superseded` | `IGD-DEC-081` |
| `IGD-OQ-064` | Pencatatan penetapan dokter pemeriksa | `superseded` | `IGD-DEC-082` |
| `IGD-OQ-065` | Perilaku saat pemicu pengkajian ulang tertunggak | `superseded` | `IGD-DEC-083` |
| `IGD-OQ-066` | Pencegahan episode IGD ganda | `superseded` | `IGD-DEC-084` |

Rencana pertanyaan tentang pemesanan dan alokasi tempat tidur **gugur sebelum diajukan**,
karena `IGD-DEC-069` sudah memindahkan seluruh urusan tempat tidur ke modul Rawat Inap.

#### Open Question dari pass sebelumnya yang tetap terbuka

Keduanya bersifat organisasi, bukan keputusan desain, dan **tidak** memblokir langkah
berikutnya:

| ID | Pokok | Menunggu |
| --- | --- | --- |
| `IGD-OQ-037` | Kewenangan dan batas waktu break-glass | Security/Privacy owner dan Clinical Governance yang belum ditunjuk |
| `IGD-OQ-038` | Approver bernama untuk roadmap backend | Product/Domain Owner yang namanya belum diisi |

#### Pertanyaan yang layak diajukan pada pass berikutnya

| Pokok | Sebab | Sifat |
| --- | --- | --- |
| Empat pengaturan `MstEmergencySetting` yang tidak menjalankan apa pun — dijalankan atau dicabut? | `IGD-GAP-031`, F-5 | Tidak memblokir desain |
| Bentuk terstruktur primary survey ABCDE | `IGD-GAP-027` | Dapat diputuskan saat desain |
| Perlakuan hasil penunjang yang datang terlambat setelah pasien pindah | `IGD-GAP-024`, menunggu modul Diagnostic Services | `LATER SLICE` |
| Pembedaan obat "diserahkan" dan "diberikan" | `IGD-GAP-025`, menunggu pemilik `PharmacyManagement` | `LATER SLICE` |

---

## Koreksi 2026-08-24 — `F-17` salah, dan akibatnya pada tiga butir

### Yang salah

Butir **`F-17`** pada Amendment Pass 2026-08-24 menyatakan:

> "Pencarian berkas dengan kata `laborator`, `radiolog`, `specimen`, dan `imaging` di seluruh
> `Areas/` menghasilkan **nol berkas**. Permintaan laboratorium dan radiologi belum ada dalam
> bentuk apa pun."

**Pernyataan itu tidak benar.** Modul `LaboratoryManagement` sudah ada di `HEAD`, terlacak
git, dan bersih. Ia masuk lewat commit `1a8a9ce feat: add laboratory order foundation` beserta
migration `20260815103436_initializeLabOrder`.

| Berkas | Keberadaan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | Ada |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Ada |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | Ada |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | Ada |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Ada |
| Tabel `LabOrder` pada `ApplicationDbContextModelSnapshot` | Ada |

### Yang sebenarnya ada — `F-17b`

`LabOrder` berbentuk rintisan, bukan modul penunjang yang lengkap:

```
LabOrder : IdentityModel
├─ Id
├─ EncounterId   (wajib)  → TrxPatientEncounter
└─ ProcedureId   (wajib)  → MstProcedure
```

Endpoint pada `api/v1/health-services/laboratory-management/lab-orders`:

| Metode | Route | Izin |
| --- | --- | --- |
| `GET` | `/` | `LabOrder : Read` |
| `GET` | `/{id}` | `LabOrder : Read` |
| `POST` | `/` | `LabOrder : Create` |
| `PUT` | `/{id}/cancel` | `LabOrder : Update` |

Yang **belum** ada pada `LabOrder`: status pesanan, hasil pemeriksaan, waktu pesan, dokter
pemesan, tingkat kegawatan pesanan, jenis spesimen, waktu pengambilan spesimen, waktu hasil
keluar, penanda nilai kritis, dan alasan pembatalan.

Radiologi memang **belum ada** dalam bentuk apa pun. Bagian `F-17` yang menyangkut radiologi
tetap berlaku.

### Satu hal penting yang terlewat karena kekeliruan ini

`LabOrder.EncounterId` menunjuk **langsung** ke `TrxPatientEncounter` dan **tidak mewajibkan
`ConsultationId`**. Artinya pemesanan laboratorium untuk pasien IGD sebenarnya **sudah dapat
berjalan hari ini**, berbeda dengan diagnosis, tindakan, dan resep yang terkunci di balik
`ConsultationId` sesuai `F-8`.

Ini mengubah gambaran kesenjangan: penunjang bukan "tidak ada sama sekali", melainkan "ada
pintu masuknya, belum ada isinya".

### Butir yang harus diperbaiki

| Butir | Keadaan sebelum koreksi | Keadaan setelah koreksi |
| --- | --- | --- |
| `F-17` | "Permintaan laboratorium dan radiologi belum ada dalam bentuk apa pun" | **Salah untuk laboratorium.** Diganti `F-17b` di atas. Tetap benar untuk radiologi |
| `IGD-GAP-024` | "Permintaan laboratorium dan radiologi belum ada", `MISSING_NEW`, `HIGH` | Dipecah: **`IGD-GAP-024a`** laboratorium — `EXTEND_EXISTING`, `HIGH`, karena rintisannya ada tetapi tanpa status, hasil, spesimen, dan nilai kritis. **`IGD-GAP-024b`** radiologi — `MISSING_NEW`, `HIGH` |
| Cakupan `IGD-DEC-078` | "Pemeriksaan penunjang: modul Laboratorium dan Radiologi belum ada" | **Alasannya berubah.** Untuk laboratorium, pesanannya dapat dibuat tetapi "belum keluar hasilnya" **tidak dapat diketahui sistem** karena `LabOrder` tidak punya kolom status maupun hasil. Untuk radiologi, memang belum ada modulnya |

`IGD-DEC-078` sendiri **tidak berubah isinya**. Yang berubah hanya alasan mengapa bagian
penunjang belum dapat ditegakkan.

### Mengapa kekeliruan ini terjadi dan apa artinya

Pencarian pada tahap discovery dijalankan sendiri oleh agent terhadap `Areas/`, bukan dibaca
dari capability map, karena capability map memang sudah basi. Pencarian itu melewatkan satu
folder yang jelas-jelas ada.

Inilah alasan `01-existing-capability-map.md` wajib disegarkan sebelum desain disusun.
Discovery yang dikerjakan sambil lalu di dalam pass wawancara **bukan pengganti** audit
kemampuan yang menyeluruh. Satu modul terlewat pada pass ini; tidak ada jaminan tidak ada
yang lain.

---

## Closure Pass 2026-08-24 — penutupan closure question capability map revision 3

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Revision dasar | `4` |
| Status pass | `draft` — wawancara berjalan; bukan approval |
| Backend SHA | `f69e9e483052845d11c91d8b7bbdce33c4acc8d8` (branch `rizkiG`) |
| Frontend SHA | `96a9120111f6acc6b7c0f37973ea0c717ba41f17` (branch `RizkiV2`) |
| Masukan | `01-existing-capability-map.md` **revision 3**, ditulis 24 Agustus 2026 pada SHA yang sama |
| Cakupan | Hanya tujuh closure question `IGD-TRQ-01` sampai `IGD-TRQ-07`, ditambah conflict dan unknown yang muncul bersamanya. Tidak membuka kembali keputusan yang sudah ditutup |

### Bukti tambahan yang ditemukan saat pass ini

Penelusuran untuk menjawab `IGD-TRQ-03` menemukan rantai yang **lebih lengkap** daripada yang
tercatat pada `IGD-CAP-41` maupun pada laporan `BE-IGD-010`:

| Mata rantai | Berkas | Keadaan |
| --- | --- | --- |
| Pengguna ke profil pegawai | `NewQuilvianSystemBackend — Models/ApplicationUser.cs:16 (WorkforceProfileId)` | **Ada** |
| Pengguna ke departemen dan jabatan | `NewQuilvianSystemBackend — Models/ApplicationUserOrganization.cs (UserId, DepartmentId, PositionId, EffectiveStartDate, EffectiveEndDate, IsPrimary)` | **Ada**, berjangka waktu |
| Profil pegawai ke simpul organisasi | `NewQuilvianSystemBackend — Areas/Corporate/HumanResource/WorkforceCore/Models/WfpOrganizationAssignment.cs:15,19,40,42 (WorkforceProfileId, OrganizationUnitId, EffectiveStartDate, EffectiveEndDate)` | **Ada**, berjangka waktu, punya `WfpOrganizationAssignmentController` |
| Simpul organisasi sebagai master | `NewQuilvianSystemBackend — Areas/Corporate/HumanResource/MasterData/Organization/Models/MstOrganizationUnit.cs (UnitCode, UnitName, UnitType, IsOperationalUnit, ParentOrganizationUnitId)` | **Ada**, berhierarki |
| Unit layanan ke simpul organisasi | `NewQuilvianSystemBackend — Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` | **Tidak ada** — inilah satu-satunya mata rantai yang putus |

Artinya kesenjangan `IGD-CAP-41` bukan "tidak ada jalur sama sekali", melainkan **satu mata
rantai terakhir yang putus**. `IGD-CAP-41` pada capability map perlu dibaca dengan koreksi ini.

### Decision log pass berjalan

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-TRQ-03` | Closure Question | Jembatan unit layanan ke organisasi, atau tabel penugasan pengguna ke unit tersendiri? | Product/Domain Owner IGD + Security/Privacy owner + pemilik Corporate/HR + pemilik Master Data | `superseded` oleh `IGD-DEC-086` | — | `IGD-CAP-41` beserta bukti tambahan di atas |
| `IGD-DEC-086` | Decision | Kewenangan unit **tidak** memakai tabel penugasan tersendiri. Sebagai gantinya `MstServiceUnit` memperoleh satu kolom penunjuk simpul organisasi yang boleh kosong, dan kewenangan diturunkan dari penugasan pegawai yang sudah ada: pengguna ke profil pegawai, profil pegawai ke penugasan organisasi yang sedang berlaku, lalu simpul organisasi ke unit layanan. Penjaga kewenangan tetap ditulis di dalam service IGD, bukan di mesin hak akses. Pengisian dan pemeliharaan data penugasan tetap menjadi pekerjaan rutin Corporate/HR, bukan pekerjaan baru bagi tim klinis | Product/Domain Owner IGD, dengan pemilik Corporate/HR, pemilik Master Data, dan Security/Privacy owner sebagai approver akhir | `draft` — pilihan pengguna jelas; approval ketiga pemilik belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-TRQ-03`; **men-`superseded` sebagian `IGD-DEC-081`** pada bagian bentuk penyimpanannya; bagian "penjaga di dalam service" pada `IGD-DEC-081` tetap berlaku |
| `IGD-DEC-081` | Decision | Hubungan pengguna ke unit pelayanan sebagai tabel penugasan tersendiri | Product/Domain Owner IGD | `superseded` sebagian oleh `IGD-DEC-086` | — | Bagian bentuk penyimpanan dicabut. Bagian penjaga di dalam service, dan bagian yang melarang mesin hak akses disentuh, **tetap berlaku** |

#### Acceptance criteria awal dari `IGD-DEC-086`

1. Petugas yang tidak memiliki penugasan organisasi yang sedang berlaku pada simpul
   organisasi milik unit tujuan ditolak ketika mencatat event `Tiba` atau menerima serah
   terima untuk unit itu.
2. Penolakan memakai kode 403 dengan pesan yang menyebutkan sebabnya.
3. Penugasan yang `EffectiveEndDate`-nya sudah lewat tidak lagi memberi kewenangan.
4. Unit layanan yang kolom penunjuk organisasinya kosong memiliki perilaku yang **disengaja
   dan tertulis**, bukan diam-diam menolak semua orang atau diam-diam mengizinkan semua orang.
5. Tidak ada tabel penugasan pengguna ke unit layanan yang dibuat modul IGD.
6. Mesin hak akses `SysAccessPolicy` dan struktur organisasi tidak berubah perilakunya.
7. Pelayanan klinis darurat tidak pernah diblokir oleh ketiadaan penugasan.
8. Memiliki penugasan unit tidak dengan sendirinya memberi capability klinis, sesuai
   `IGD-DEC-058`.

#### Risiko yang diterima secara sadar

| Risiko | Sebab | Penanganan yang disepakati |
| --- | --- | --- |
| `OrganizationUnitId` pada `WfpOrganizationAssignment` boleh kosong | Kolomnya nullable; belum diperiksa apakah terisi pada data nyata | Wajib diverifikasi sebelum penjagaan dinyalakan. Tercatat sebagai `IGD-UNK-06` |
| Unit layanan dan simpul organisasi belum tentu sepadan | `MstServiceUnit` adalah layanan klinis; `MstOrganizationUnit` adalah simpul bagan organisasi. Satu simpul dapat menaungi beberapa unit layanan | Kolom penunjuk berarah dari unit layanan ke simpul organisasi, sehingga banyak unit layanan boleh menunjuk satu simpul yang sama |
| Perawat bantuan lintas unit tanpa baris penugasan akan tertolak | Data HR tidak selalu mencerminkan penugasan sementara harian | **Belum diputuskan.** Menjadi `IGD-OQ-067` |

#### Unknown baru

| ID | Yang belum diketahui | Cara memastikannya |
| --- | --- | --- |
| `IGD-UNK-06` | Apakah `WfpOrganizationAssignment.OrganizationUnitId` benar-benar terisi untuk petugas IGD, dan apakah `ApplicationUser.WorkforceProfileId` terisi untuk akun petugas klinis | Kueri ke basis data pengembangan. **Tidak dikerjakan audit read-only** |
| `IGD-UNK-07` | Apakah setiap unit layanan dapat dipetakan ke tepat satu simpul organisasi tanpa memaksa | Peninjauan data master bersama Corporate/HR |

| `IGD-OQ-067` | Open Question | Bagaimana perawat bantuan lintas unit memperoleh kewenangan sementara, mengingat penugasan hariannya tidak selalu tercermin pada data kepegawaian? | Product/Domain Owner IGD + Nursing authority + pemilik Corporate/HR | `draft` — tidak memblokir desain; memblokir penyalaan penjagaan di produksi | — | Risiko yang diterima pada `IGD-DEC-086` |

| `IGD-TRQ-01` | Closure Question | Apakah IGD memakai `LabOrder` apa adanya, atau menunggu dilengkapi? | Product/Domain Owner IGD + pemilik `LaboratoryManagement` + Clinical Governance | `superseded` oleh `IGD-DEC-087` | — | `IGD-CAP-29` |
| `IGD-DEC-087` | Decision | IGD memakai `LabOrder` **apa adanya** pada rilis pertama. Dokter IGD dapat memesan pemeriksaan laboratorium, dan pesanan itu menempel pada kunjungan IGD tanpa memerlukan konsultasi. IGD **tidak** melengkapi `LabOrder` dan **tidak** membuat pemesanan laboratorium tandingan. Pelengkapan `LabOrder` — status pesanan, hasil, jenis spesimen, dokter pemesan, prioritas, dan penanda nilai kritis — menjadi slice tersendiri dengan pemiliknya sendiri. Bagian pemeriksaan penunjang pada daftar sikap `IGD-DEC-078` dinyatakan **belum dapat ditegakkan** secara terbuka pada dokumen dan pada layar, bukan dilewati diam-diam | Product/Domain Owner IGD, dengan pemilik `LaboratoryManagement` dan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna jelas; pemilik `LaboratoryManagement` belum ditunjuk | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-TRQ-01`; `IGD-CAP-29`; memperjelas cakupan `IGD-DEC-078` |

#### Acceptance criteria awal dari `IGD-DEC-087`

1. Dokter IGD dapat membuat pesanan laboratorium yang menempel pada kunjungan IGD, tanpa
   satu pun baris konsultasi dibuat.
2. Pesanan laboratorium milik satu kunjungan IGD dapat ditampilkan pada layar pengkajian.
3. IGD tidak membuat entity pemesanan laboratorium sendiri.
4. IGD tidak menambah kolom pada `LabOrder`.
5. Daftar sikap pesanan pada serah terima memuat obat dan tindakan, dan menampilkan
   keterangan **tertulis** bahwa pemeriksaan penunjang belum dapat dihitung sistem.
6. Keterangan pada butir 5 muncul di layar, bukan hanya di dokumen, sehingga perawat tidak
   mengira daftarnya sudah lengkap.
7. Perawat tetap dapat menuliskan pemeriksaan yang hasilnya belum keluar pada bagian "yang
   harus dilanjutkan" dalam serah terima SBAR.

#### Akibat pada `IGD-DEC-078`

`IGD-DEC-078` **tidak berubah isinya**. Yang berubah adalah pernyataan cakupannya:

| Jenis pesanan | Rilis pertama | Sebab |
| --- | --- | --- |
| Obat | Dihitung sistem | `TrxPrescription` punya status pemenuhan |
| Tindakan | Dihitung sistem | `TrxPatientProcedure` punya status |
| Laboratorium | **Tidak dapat dihitung sistem**, ditulis manual pada SBAR | `LabOrder` tidak punya kolom status maupun hasil |
| Radiologi | Tidak ada | Modulnya belum ada — `IGD-CAP-30` |

#### Risiko yang diterima secara sadar

Serah terima ke bangsal tidak dapat menyebut pemeriksaan yang hasilnya belum keluar secara
otomatis. Contoh: Ny. Sari dikirim ke bangsal pukul 23.45 sementara hasil darah lengkapnya
belum keluar. Sistem tidak mengetahuinya; yang menutup celah adalah perawat yang menuliskannya
pada bagian "yang harus dilanjutkan". Bila perawat lupa, tidak ada yang mengingatkan.

Risiko ini **diterima** untuk rilis pertama dan menjadi alasan utama slice pelengkapan
`LabOrder` diprioritaskan setelahnya.

| `IGD-TRQ-02` | Closure Question | Siapa pemilik `LaboratoryManagement`, dan apakah pelengkapan `LabOrder` menjadi pekerjaan IGD? | Product/Domain Owner IGD + sponsor governance | `superseded` oleh `IGD-DEC-088` | — | `IGD-CAP-29` |
| `IGD-DEC-088` | Decision | Slice pelengkapan `LabOrder` dicatat sebagai **dependency eksternal** pada roadmap IGD: bernomor, beralasan, dan terlihat, tetapi **tidak dijadwalkan** dan **tidak dikerjakan** tim IGD. Roadmap IGD berjalan penuh tanpanya. Keterbatasan pemeriksaan penunjang sesuai `IGD-DEC-087` berlaku sampai pemilik `LaboratoryManagement` ditunjuk dan slice itu dikerjakan pemiliknya. Daftar dependency eksternal wajib **ditinjau berkala**, bukan dicatat lalu dilupakan | Product/Domain Owner IGD | `approved` | Rizki Gunawan, 24 Agustus 2026 | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-TRQ-02`; sejalan dengan `IGD-DEC-087` |

#### Daftar dependency eksternal roadmap IGD

Ketiganya berada **di luar** kendali tim IGD dan **tidak** menahan rilis pertama:

| No | Dependency | Pemilik | Akibat bila tidak pernah selesai |
| ---: | --- | --- | --- |
| 1 | Pelengkapan `LabOrder` — status, hasil, spesimen, prioritas, nilai kritis | `LaboratoryManagement`, **belum ditunjuk** | Bagian penunjang pada daftar sikap `IGD-DEC-078` tetap manual selamanya; serah terima bergantung pada ingatan perawat |
| 2 | Pelonggaran pembatas antrean dan konsultasi | `ClinicalManagement` dan `PharmacyManagement`, **belum ditunjuk** | `IGD-DEC-068` tidak dapat dijalankan; pengkajian, diagnosis, tindakan, dan resep IGD **tetap tidak dapat disimpan**. Ini **menahan rilis** — berbeda dari nomor 1 dan 3 |
| 3 | Modul Rawat Inap beserta `InpBedPlacement` | `RWI-BP-001`, Muhammad Hamzah | Jalur `RANAP` berhenti pada catatan kepergian IGD; tidak ada episode rawat inap yang terbentuk |

**Perbedaan penting.** Nomor 1 dan 3 tidak menahan rilis pertama IGD. **Nomor 2 menahan.**
Tanpa pelonggaran `IGD-DEC-068`, kemampuan inti yang menjadi alasan seluruh pekerjaan ini —
menyimpan pengkajian pasien IGD — tetap mustahil.

### Penutupan Closure Pass 2026-08-24

#### Yang ditutup pass ini

| ID | Ditutup oleh | Sifat |
| --- | --- | --- |
| `IGD-TRQ-01` | `IGD-DEC-087` | Pemblokir `DESIGN` |
| `IGD-TRQ-02` | `IGD-DEC-088` | Ownership |
| `IGD-TRQ-03` | `IGD-DEC-086` | Pemblokir `DESIGN` |

Tiga keputusan baru: `IGD-DEC-086`, `IGD-DEC-087`, `IGD-DEC-088`. Seluruhnya `draft`.
`IGD-DEC-081` di-`superseded` sebagian.

#### Yang sengaja tidak ditanyakan pada pass ini

Closure Pass hanya menangani conflict, unknown, ownership, dan keputusan yang memblokir
desain. Empat closure question sisanya bukan salah satu di antaranya:

| ID | Pokok | Sifat | Diserahkan kepada |
| --- | --- | --- | --- |
| `IGD-TRQ-04` | Pembaruan test `FE-IGD-001 K1` dan pemakai lain yang bergantung pada jenis kunjungan `Outpatient` | Urutan pekerjaan | `/qv-plan` — menjadi bagian acceptance task `IGD-DEC-074` |
| `IGD-TRQ-05` | Nasib data pada empat kolom tempat tidur yang akan dicabut | Urutan pekerjaan, bergantung `IGD-UNK-03` | `/qv-plan` — memerlukan kueri basis data lebih dulu |
| `IGD-TRQ-06` | Cakupan uji minimum sebelum menyentuh `ClinicalManagement` | Urutan pekerjaan | `/qv-plan` — memakai prasarana `IGD-CAP-43` yang sudah ada |
| `IGD-TRQ-07` | Realtime untuk daftar pantau IGD | `LATER SLICE` | Pass berikutnya bila daftar pantau terbukti perlu |

#### Unknown yang tetap terbuka

| ID | Yang belum diketahui | Memblokir |
| --- | --- | --- |
| `IGD-UNK-01` | Apakah enam master IGD dan master kelas pasien IGD sudah terisi | `IMPLEMENTATION` |
| `IGD-UNK-02` | Adakah kunjungan IGD lama yang `EncounterId`-nya kosong | `IMPLEMENTATION` `IGD-DEC-074` |
| `IGD-UNK-03` | Berapa baris `TrxEmergencyTransfer` yang sudah mengisi kolom tempat tidur | `IMPLEMENTATION` `IGD-DEC-069` |
| `IGD-UNK-04` | Apakah `LabOrder` sudah dipakai di produksi | Tidak memblokir |
| `IGD-UNK-06` | Apakah `WfpOrganizationAssignment.OrganizationUnitId` dan `ApplicationUser.WorkforceProfileId` terisi untuk petugas IGD | `IMPLEMENTATION` `IGD-DEC-086` |
| `IGD-UNK-07` | Apakah setiap unit layanan dapat dipetakan ke tepat satu simpul organisasi | `IMPLEMENTATION` `IGD-DEC-086` |

Keenamnya **hanya dapat dijawab dengan kueri ke basis data**, bukan dengan membaca source.
Seluruhnya memblokir implementasi, **tidak satu pun memblokir desain**.

#### Open Question yang tetap terbuka

| ID | Pokok | Memblokir |
| --- | --- | --- |
| `IGD-OQ-037` | Kewenangan dan batas waktu break-glass | Organisasi |
| `IGD-OQ-038` | Approver bernama untuk roadmap backend | Organisasi |
| `IGD-OQ-067` | Kewenangan sementara perawat bantuan lintas unit | Penyalaan penjagaan di produksi |

#### Status gerbang setelah Closure Pass ini

| Gerbang | Keadaan |
| --- | --- |
| Capability map segar dan sahih | **Ya** — revision `3` pada SHA yang sama dengan pass ini |
| Conflict tertutup | **Ya** — `IGD-CONFLICT-003`, `004`, `005` dan `IGD-CONF-01` sampai `05` sudah punya keputusan atau tercatat sebagai perbaikan |
| Closure question yang memblokir desain tertutup | **Ya** — `IGD-TRQ-01` dan `IGD-TRQ-03` |
| Unknown yang memblokir desain | **Tidak ada** — keenamnya memblokir implementasi saja |
| Approval formal owner | **Tidak** — seluruh keputusan `draft` |
| Boleh lanjut ke `/qv-design` | **Ya** — dengan syarat blueprint hasilnya tidak ditandai `approved` |

---

## Pertanyaan yang lahir dari desain 2026-08-24 — revision 5

Empat pertanyaan berikut **tidak** berasal dari wawancara. Keduanya muncul ketika keputusan
`draft` diterjemahkan menjadi bentuk teknis, dan tidak dapat dijawab agent.

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-OQ-068` | Open Question | `IGD-DEC-070` memilih dua kolom status dan **menolak** bentuk daftar kejadian. Tetapi `IGD-DEC-065` (entri susulan dengan waktu sebenarnya), `IGD-DEC-066` (koreksi tambah-saja dan pembalikan berpersetujuan), dan `IGD-DEC-085` (pemberitahuan koreksi) menuntut penyimpanan yang tidak dapat diwadahi kolom status. Desain menyatukannya: kolom status sebagai turunan yang cepat dibaca, ditambah tabel kejadian tambah-saja sebagai sumber audit. Apakah penafsiran ini dapat diterima, atau `IGD-DEC-070` memang melarang tabel kejadian sama sekali? | Product/Domain Owner IGD + Clinical Governance | `superseded` oleh `IGD-DEC-090` | — | `02-backend-architecture.md` bagian 2.4 |
| `IGD-OQ-069` | Open Question | `IGD-GAP-031` mencatat empat kolom `MstEmergencySetting` yang tersimpan tanpa menjalankan apa pun. Desain mengusulkan dua dipertahankan dan mulai dibaca (`ImmediateCareLevelThreshold`, `RequireRegistrationBeforeTreatmentFromLevel`), dua dicabut (`AutoCreateProvisionalEncounter`, `RequireTriageBeforeStandardRegistration`). Apakah pencabutan dua kolom itu benar, atau justru keduanya harus diberi arti? | Product/Domain Owner IGD | `draft` — tidak memblokir desain maupun rilis | — | `02-backend-architecture.md` bagian 5.1 |
| `IGD-OQ-070` | Open Question | `IGD-DEC-069` mengubah arti `TrxEmergencyTransfer` dari "perpindahan beserta tempat tidur" menjadi "catatan kepergian pasien". Desain mengusulkan penggantian nama menjadi `TrxEmergencyDeparture`, yang mengubah nama tabel, nama kelas, dan seluruh route API. Apakah penggantian nama diterima, atau nama lama dipertahankan demi pemakai luar yang mungkin belum diketahui? | Product/Domain Owner IGD + pemilik integrasi | `superseded` oleh `IGD-DEC-091` | — | `02-backend-architecture.md` bagian 2.3; `contracts/api-contract.md` bagian 6 |
| `IGD-OQ-071` | Open Question | `IGD-DEC-086` menurunkan kewenangan unit dari pemetaan `MstServiceUnit` ke simpul organisasi. Bagaimana perilaku sistem untuk unit yang kolom pemetaannya **belum diisi**? Menolak semua orang menghentikan pelayanan; mengizinkan semua orang menghapus penjagaannya. Keduanya buruk | Security/Privacy owner + Product/Domain Owner IGD | `superseded` oleh `IGD-DEC-092` — **sementara**, pengesahan Security/Privacy owner masih ditunggu | — | `contracts/validation-matrix.md` bagian 7 aturan 3 |

### Catatan tentang keempatnya

`IGD-OQ-069` **tidak** memblokir apa pun. Tiga lainnya memblokir implementasi satu atau dua
epic, tetapi **tidak** memblokir gelombang `MVP-0` dan `MVP-1`.

Ketiganya juga **tidak** memblokir penyusunan desain, karena bentuk teknisnya sudah dirancang
lengkap beserta alternatifnya; yang ditunggu hanyalah pilihan owner di antara alternatif yang
sudah tertulis.

---

## Penetapan Kepemilikan Modul 2026-08-24

| Field | Value |
| --- | --- |
| Tanggal | 24 Agustus 2026 |
| Ditetapkan lewat | Pernyataan langsung pemegang peran pada sesi kerja |
| Backend SHA | `f69e9e483052845d11c91d8b7bbdce33c4acc8d8` |

### Decision log

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-DEC-089` | Decision | **Rizki Gunawan** adalah pemegang modul `EmergencyInstallationManagement` sekaligus Product/Domain Owner modul IGD. Penetapan ini mengisi kolom `owners` dan `approved_by` yang selama ini berbunyi "pemegang sementara, nama belum diisi" pada `IGD-DEC-046`, pada manifest, dan pada seluruh keputusan `IGD-DEC-067` sampai `IGD-DEC-088` | Rizki Gunawan | `approved` — pernyataan pemegang peran itu sendiri, sehingga tidak memerlukan pihak ketiga | Rizki Gunawan, 24 Agustus 2026 | Pernyataan pengguna 24 Agustus 2026; dikuatkan identitas git `Rizki Gunawan <101787601+Rizki0720@users.noreply.github.com>` dan riwayat commit pada `Areas/HealthServices/EmergencyInstallationManagement/` |

### Apa yang berubah karena penetapan ini

`IGD-DEC-046` menetapkan Product/Domain Owner sebagai "pemegang sementara" tanpa nama.
`IGD-DEC-089` mengisi nama itu. `IGD-DEC-046` **tidak** dibatalkan; ia hanya kini punya orang.

### Batas kewenangan yang perlu ditegaskan

Penetapan ini **hanya** mencakup modul `EmergencyInstallationManagement`. Ia **tidak**
menjadikan pemegangnya berwenang atas:

| Peran | Keadaan | Dibutuhkan untuk |
| --- | --- | --- |
| Pemilik `ClinicalManagement` | **Belum ditunjuk** | `IGD-DEC-068`, `IGD-DEC-080` |
| Pemilik `PharmacyManagement` | **Belum ditunjuk** | `IGD-DEC-068`, `IGD-DEC-078` |
| Pemilik `LaboratoryManagement` | **Belum ditunjuk** | `IGD-DEC-087` |
| Pemilik Master Data | **Belum ditunjuk** | `IGD-DEC-086` |
| Registration API owner | **Belum ditunjuk** | `IGD-DEC-074`, `IGD-DEC-075`, `IGD-DEC-084` |
| Finance owner | **Belum ditunjuk** | `IGD-DEC-076`, `IGD-DEC-077` |
| Clinical Governance | **Belum ditunjuk** | Sebelas keputusan klinis |
| Nursing authority | **Belum ditunjuk** | Lima keputusan keperawatan |
| Security/Privacy owner | **Belum ditunjuk** | `IGD-DEC-080`, `IGD-DEC-081`, `IGD-DEC-086`, `IGD-OQ-071` |
| Integration owner | **Belum ditunjuk** | `IGD-DEC-085` |
| Product/Domain Owner Rawat Inap | **Muhammad Hamzah** | `IGD-DEC-069`, `071`, `073`, `075`, `077`, `078` |
| Pemilik Corporate/HR | **Belum ditunjuk** | `IGD-DEC-081`, `IGD-DEC-086` |

### Dua keputusan yang kini dapat disetujui tanpa menunggu siapa pun

Dari dua puluh dua keputusan `IGD-DEC-067` sampai `IGD-DEC-088`, hanya dua yang seluruh
approver-nya kini terisi:

| ID | Approver yang dibutuhkan | Keadaan |
| --- | --- | --- |
| `IGD-DEC-067` | Product/Domain Owner IGD **dan** pemilik `EmergencyInstallationManagement` | **Keduanya Rizki Gunawan.** Dapat disetujui |
| `IGD-DEC-088` | Product/Domain Owner IGD saja | **Rizki Gunawan.** Dapat disetujui |

Dua puluh keputusan lain tetap `draft` karena masing-masing masih menunggu sedikitnya satu
peran yang belum ditunjuk.

### Akibat lintas modul

`IGD-DEC-067` adalah jawaban yang ditunggu `RWI-OQ-034` dan `DEC-INP-002` pada blueprint
Rawat Inap. Kedua butir itu berbunyi: *"Apakah pemilik `EmergencyInstallationManagement`
menyetujui bahwa disposisi `RANAP` menutup kunjungan IGD dan membuat kunjungan rawat inap
baru, serta menyetujui penanda `ClosesEmergencyVisit` mulai benar-benar dijalankan?"*

Selama ini keduanya berstatus `OPEN` dengan keterangan "pemilik belum ditunjuk", dan slice
`INP-S09` milik Rawat Inap berhenti karenanya. Dengan `IGD-DEC-089`, pemiliknya kini ada.

**Persetujuan atas `IGD-DEC-067` belum dicatat pada pass ini.** Penetapan kepemilikan adalah
pernyataan peran, bukan persetujuan atas isi keputusan. Keduanya sengaja dipisahkan supaya
tidak ada keputusan yang dianggap disetujui hanya karena pemiliknya sudah bernama.

---

## Approval 2026-08-24 — Rizki Gunawan

| Field | Nilai |
| --- | --- |
| Yang menyetujui | **Rizki Gunawan**, Product/Domain Owner IGD sekaligus pemilik `EmergencyInstallationManagement`, ditetapkan `IGD-DEC-089` |
| Tanggal | 24 Agustus 2026 |
| Bentuk persetujuan | Pernyataan langsung pada sesi kerja, sesudah membaca teks lengkap kedua keputusan beserta konsekuensinya |
| Backend SHA | `f69e9e483052845d11c91d8b7bbdce33c4acc8d8` |

### 1. Yang menjadi `approved` penuh

Dua keputusan yang seluruh peran approver-nya dipegang Rizki Gunawan:

| ID | Isi ringkas | Akibat |
| --- | --- | --- |
| `IGD-DEC-067` | Disposisi `RANAP` menutup kunjungan IGD dan membuat kunjungan rawat inap baru sebagai satu tindakan utuh; catatan klinis IGD tetap di kunjungan IGD; `ClosesEmergencyVisit` mulai dijalankan | **Menjawab `RWI-OQ-034` dan `DEC-INP-002`.** Slice `INP-S09` milik Rawat Inap tidak lagi terhalang oleh pihak IGD |
| `IGD-DEC-088` | Slice pelengkapan `LabOrder` menjadi dependency eksternal; tidak dijadwalkan dan tidak dikerjakan tim IGD; ditinjau berkala | Roadmap IGD berjalan penuh tanpa modul laboratorium matang |

### 2. Persetujuan sisi IGD atas dua puluh keputusan lain

Pemilik modul menyatakan menyetujui **seluruh** keputusan pada revisi ini. Persetujuan itu
dicatat sebagai **persetujuan sisi IGD**, dan **tidak** menjadikan keputusan-keputusan berikut
`approved` penuh.

Alasannya: setiap keputusan di bawah menuntut sedikitnya satu peran yang **tidak dipegang**
Rizki Gunawan. Seseorang tidak dapat menyetujui atas nama peran yang bukan miliknya, betapa
pun jelas maksudnya.

| ID | Peran yang masih ditunggu |
| --- | --- |
| `IGD-DEC-068` | Pemilik `ClinicalManagement`, pemilik `PharmacyManagement`, Product/Domain Owner Rawat Inap |
| `IGD-DEC-069` | Product/Domain Owner Rawat Inap |
| `IGD-DEC-070` | Nursing authority, Clinical Governance |
| `IGD-DEC-071` | Product/Domain Owner Rawat Inap |
| `IGD-DEC-072` | Clinical Governance, Nursing authority |
| `IGD-DEC-073` | Clinical Governance, Product/Domain Owner Rawat Inap |
| `IGD-DEC-074` | Registration API owner |
| `IGD-DEC-075` | Registration API owner, Product/Domain Owner Rawat Inap |
| `IGD-DEC-076` | Finance owner |
| `IGD-DEC-077` | Product/Domain Owner Rawat Inap, Finance owner |
| `IGD-DEC-078` | Clinical Governance, pemilik `PharmacyManagement`, Product/Domain Owner Rawat Inap |
| `IGD-DEC-079` | Nursing authority, Clinical Governance |
| `IGD-DEC-080` | Security/Privacy owner, Clinical Governance |
| `IGD-DEC-081` | Security/Privacy owner, pemilik Corporate/HR |
| `IGD-DEC-082` | Clinical Governance |
| `IGD-DEC-083` | Nursing authority, Clinical Governance |
| `IGD-DEC-084` | Registration API owner |
| `IGD-DEC-085` | Nursing authority, Clinical Governance, Integration owner |
| `IGD-DEC-086` | Pemilik Corporate/HR, pemilik Master Data, Security/Privacy owner |
| `IGD-DEC-087` | Pemilik `LaboratoryManagement`, Clinical Governance |

**Arti praktisnya.** Sisi IGD sudah selesai berdebat. Yang tersisa **bukan** lagi pertanyaan
"apa aturannya", melainkan "siapa yang berwenang mengiyakan". Setiap keputusan di atas tinggal
menunggu tanda tangan, bukan menunggu keputusan baru.

### 3. Yang wajib disampaikan kepada pemilik modul Rawat Inap

`IGD-DEC-067` menjadi `approved` oleh pemilik `EmergencyInstallationManagement`. Ini adalah
jawaban yang ditunggu dua butir pada blueprint `RWI-BP-001`:

| Butir milik Rawat Inap | Keadaan sebelumnya | Keadaan sesudah |
| --- | --- | --- |
| `RWI-OQ-034` | `OPEN` — "pemilik `EmergencyInstallationManagement` belum ditunjuk" | **Terjawab** oleh `IGD-DEC-067`, disetujui Rizki Gunawan 24 Agustus 2026 |
| `DEC-INP-002` | `OPEN`, memblokir slice `INP-S09` | **Terjawab**; `INP-S09` tidak lagi terhalang pihak IGD |

Blueprint Rawat Inap **tidak diubah** oleh pass ini. Mengubah dokumen milik modul lain
melanggar batas kepemilikan yang justru sedang ditegakkan. Pembaruan `RWI-OQ-034` dan
`DEC-INP-002` adalah pekerjaan Product/Domain Owner Rawat Inap, dengan catatan ini sebagai
buktinya.

Yang perlu disampaikan bersamaan, karena ketiganya konsekuensi dari aturan yang ia kunci
sendiri:

1. `RWI-RULE-026` aturan 6 perlu diperluas ke kunjungan bertipe `Emergency` — `IGD-DEC-068`;
2. `compatibility_impact` pada manifest tidak lagi dapat berbunyi "nol perubahan kolom pada
   tabel modul lain" — `IGD-DEC-075`;
3. `InpBedPlacement` membaca waktu tiba dari catatan kepergian IGD — `IGD-DEC-071`.

### 4. Yang tidak berubah oleh approval ini

| Hal | Keadaan |
| --- | --- |
| Sembilan belas peran yang belum ditunjuk | Tetap belum ditunjuk |
| `IGD-OQ-068`, `IGD-OQ-070`, `IGD-OQ-071` | Tetap terbuka dan tetap memblokir implementasi tiga epic |
| `IGD-UNK-01` sampai `IGD-UNK-07` | Tetap hanya dapat dijawab kueri basis data |
| Status blueprint revision `5` | Tetap `draft` |
| Otorisasi menjalankan migration | Tetap belum diberikan |
| Gerbang kemampuan rumah sakit | Tetap belum terpenuhi |

Blueprint **tidak** naik menjadi `approved` hanya karena dua keputusan di dalamnya disetujui.

---

## Amendment Pass 2026-08-24 (kedua) — penutupan tiga pertanyaan memblokir

Pass ini menutup `IGD-OQ-068`, `IGD-OQ-070`, dan `IGD-OQ-071` — tiga pertanyaan yang lahir
dari desain revision `5` dan memblokir implementasi `EPIC IGD-05`, `EPIC IGD-06`, dan
`EPIC IGD-08`. Pass dijalankan atas permintaan Rizki Gunawan setelah `/qv-design` selesai.

`IGD-OQ-069` **tidak** ditanyakan pada pass ini karena tidak memblokir apa pun.

### Bukti source yang dikumpulkan lebih dulu

Dua dari tiga pertanyaan memuat asumsi yang dapat diperiksa dari source, sehingga tidak
ditanyakan kepada pengguna. Diperiksa pada backend `f69e9e48` dan frontend `96a91201`.

| Kode | Yang diperiksa | Hasil |
| --- | --- | --- |
| `IGD-EV-090` | Pemanggil route `emergency-transfers` di frontend | **Satu** pemanggil nyata: `TRANSFER_URL` pada `emergency-assessment-slice.jsx:16`. Satu kecocokan lain hanya komentar pada `emergency-assessment-constant.jsx:432` |
| `IGD-EV-091` | URL halaman yang memuat kata `transfer` | **Nol.** Tab perpindahan berada di dalam `emergency-assessment/[slug]`, sehingga tidak ada bookmark petugas yang rusak oleh penggantian nama route |
| `IGD-EV-092` | Berkas backend yang menyebut `EmergencyTransfer` | **25 kecocokan, 9 berkas nyata**: controller, DTO, enum, model, `TrxEmergencyVisit`, dua service, konfigurasi EF, `Program.cs`, `ApplicationDbContext`. Enam belas sisanya adalah `*.Designer.cs` dan snapshot migration yang dihasilkan tooling dan tidak disentuh manual |
| `IGD-EV-093` | Kolom pemetaan organisasi pada `MstServiceUnit` | **Belum ada sama sekali.** `MstServiceUnit.cs` tidak memuat `OrganizationUnitId` maupun padanannya. Konsekuensinya: pada hari migration mendarat, **100% unit layanan berstatus belum dipetakan** — bukan sebagian kecil |

`IGD-EV-093` mengubah bentuk `IGD-OQ-071` secara mendasar dan disampaikan kepada pengguna
sebelum ia menjawab.

### Decision log pass berjalan

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-OQ-068` | Open Question | Bentuk penyimpanan kepergian pasien: kolom status saja, tabel kejadian saja, atau keduanya | Product/Domain Owner IGD + Clinical Governance | `superseded` oleh `IGD-DEC-090` | — | `02-backend-architecture.md` bagian 2.4 |
| `IGD-DEC-090` | Decision | Kepergian pasien disimpan dalam **dua lapis**. Lapis pertama: dua kolom status pada `TrxEmergencyDeparture` (`PhysicalStatus`, `HandoverStatus`) yang tetap ada dan tetap cepat dibaca untuk daftar pantau dan penyaring — sesuai `IGD-DEC-070`. Lapis kedua: `TrxEmergencyDepartureEvent` yang bersifat **tambah-saja** dan menyimpan setiap perubahan beserta pelaku, waktu server, waktu kejadian sebenarnya, alasan, koreksi, dan pembalikan. Kolom status adalah **turunan** dari kejadian terakhir yang berlaku, bukan sumber kebenaran tandingan; setiap penulisan kejadian memperbarui kolom status **dalam transaksi yang sama**. Baris kejadian **tidak pernah** ditimpa maupun dihapus — koreksi ditulis sebagai baris baru yang menunjuk baris lama lewat `SupersedesEventId`, dan baris lama ditandai tidak-efektif | Product/Domain Owner IGD, dengan Clinical Governance sebagai approver akhir | `draft` — pilihan pengguna jelas; approval Clinical Governance belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-068`; **memperluas `IGD-DEC-070`, tidak membatalkannya**; menegakkan `IGD-DEC-065`, `IGD-DEC-066`, `IGD-DEC-080`, `IGD-DEC-085` |
| `IGD-OQ-070` | Open Question | Apakah `TrxEmergencyTransfer` diganti nama menjadi `TrxEmergencyDeparture` | Product/Domain Owner IGD + pemilik integrasi | `superseded` oleh `IGD-DEC-091` | — | `02-backend-architecture.md` bagian 2.3 |
| `IGD-DEC-091` | Decision | Penggantian nama **diterima penuh**. `TrxEmergencyTransfer` menjadi `TrxEmergencyDeparture`; kelas, enum, service, controller, DTO, dan konfigurasi EF ikut berganti; grup route `emergency-transfers` menjadi `emergency-departures`. **Tidak ada** route usang yang dipertahankan. Migration wajib memakai `RENAME TABLE`, **bukan** drop-create, sehingga nol baris data hilang dan langkah mundurnya sekadar `RENAME` balik. Dasarnya: nama lama menyesatkan setelah `IGD-DEC-069` mengubah artinya — pasien pulang dan pasien meninggal juga tercatat pada tabel ini, dan tak satu pun "pindah" | Product/Domain Owner IGD, dengan pemilik integrasi sebagai approver akhir | `draft` — pilihan pengguna jelas; approval pemilik integrasi belum tercatat | — | Jawaban pengguna 24 Agustus 2026, pilihan A untuk `IGD-OQ-070`; berdasar `IGD-EV-090`, `IGD-EV-091`, `IGD-EV-092` |
| `IGD-OQ-071` | Open Question | Perilaku unit layanan yang kolom simpul organisasinya belum diisi | Security/Privacy owner + Product/Domain Owner IGD | `superseded` oleh `IGD-DEC-092` | — | `contracts/validation-matrix.md` bagian 7 aturan 3 |
| `IGD-DEC-092` | Decision | Unit layanan yang kolom simpul organisasinya kosong **menolak** akses — fail-closed. Tersedia **jalan keluar beralasan** yang mencatat nama pengguna, unit, waktu, dan alasan, mengikuti pola `IGD-DEC-084`. Pelayanan klinis darurat **tidak pernah** diblokir aturan ini, sesuai `IGD-DEC-086` butir 7. **Syarat yang melekat pada keputusan ini:** karena `IGD-EV-093` membuktikan seluruh unit kosong pada hari migration, penjagaan hanya boleh dinyalakan **setelah** pengisian pemetaan dinyatakan selesai oleh pemilik Master Data. Jalan keluar beralasan diperuntukkan bagi **sisa celah**, bukan bagi keadaan kosong massal; bila dipakai setiap hari oleh semua orang, catatannya berhenti menjadi bukti dan berubah menjadi derau | Product/Domain Owner IGD **sebagai keputusan sementara**; Security/Privacy owner sebagai approver akhir | `draft` — **sementara**. Pemilik sah pertanyaan ini adalah Security/Privacy owner yang **belum ditunjuk**. Keputusan ini berlaku sebagai arah kerja, dan **wajib ditinjau ulang** begitu Security/Privacy owner ada | — | Jawaban pengguna 24 Agustus 2026, pilihan B untuk `IGD-OQ-071`; syarat urutan ditambahkan atas dasar `IGD-EV-093` |

### Penutupan Amendment Pass 2026-08-24 (kedua)

**Yang terbuka blokirnya.**

| Epic | Sebelumnya | Sekarang |
| --- | --- | --- |
| `EPIC IGD-05` | Terblokir `IGD-OQ-068` dan `IGD-OQ-070` | **Tidak terblokir keputusan.** Tetap menunggu `IGD-UNK-03` untuk gelombang `MVP-3` |
| `EPIC IGD-06` | Terblokir `IGD-OQ-068` | **Tidak terblokir keputusan** |
| `EPIC IGD-08` | Terblokir `IGD-OQ-071` | **Tidak terblokir keputusan.** Tetap menunggu pengisian data pemetaan, dan pengesahan Security/Privacy owner |

**Yang tidak berubah.**

| Hal | Keadaan |
| --- | --- |
| `EPIC IGD-09` | Tetap `OPEN DECISION`. Pemilik `ClinicalManagement` dan `PharmacyManagement` tetap belum ditunjuk |
| `IGD-OQ-069` | Tetap terbuka. Tidak memblokir apa pun |
| `IGD-OQ-067`, `IGD-OQ-037`, `IGD-OQ-038` | Tetap terbuka |
| `IGD-UNK-01` sampai `IGD-UNK-07` | Tetap hanya dapat dijawab kueri basis data bersama |
| Sembilan belas peran yang belum ditunjuk | Tetap belum ditunjuk |
| Status blueprint revision `5` | Tetap `draft` |
| Otorisasi menjalankan migration | Tetap belum diberikan |
| Gerbang kemampuan rumah sakit | Tetap belum terpenuhi |

**Catatan atas bagian "Approval 2026-08-24" di atas.** Baris yang berbunyi *"`IGD-OQ-068`,
`IGD-OQ-070`, `IGD-OQ-071` tetap terbuka dan tetap memblokir implementasi tiga epic"* benar
pada saat pass itu ditulis. Pass ini menggantikannya. Baris tersebut sengaja **tidak dihapus**
karena merupakan catatan keadaan pada pass sebelumnya.

**Yang tidak dikerjakan pass ini.**

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Menandai keputusan `approved` | Approval adalah tindakan manusia. `IGD-DEC-090`, `091`, `092` seluruhnya `draft` |
| Menyunting `02-backend-architecture.md`, `04-prd-to-mvp.md`, dan contracts | Keluaran `/qv-design`, bukan `/qv-grill`. Ketiga keputusan ini **membenarkan** isi yang sudah tertulis di sana, sehingga tidak ada yang perlu diubah — hanya status pertanyaannya |
| Source code, migration, atau UI | Di luar wewenang tahap wawancara |
| Roadmap dan task | Keluaran `/qv-plan` |

---

## Approval sempit 2026-08-24 — gerbang `/qv-plan` untuk `EPIC IGD-03`

Rizki Gunawan, Product/Domain Owner IGD (`IGD-DEC-089`), memberi approval **terbatas** agar
`/qv-plan` dapat berjalan untuk gelombang `MVP-0`. Approval ini sengaja dipersempit ke bagian
kontrak yang benar-benar dibutuhkan `EPIC IGD-03`, bukan ke seluruh kontrak `0.3.0`.

### Decision log

| ID | Jenis | Isi | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-DEC-093` | Approval | `contracts/state-transition-matrix.md` bagian 1, 1.1, dan 1.2 — seluruhnya mengenai `EmergencyVisitStatus` — beserta `contracts/validation-matrix.md` bagian 2 aturan 4 dan 5 dinyatakan **`approved`** pada versi `0.3.0`. Bagian lain kedua kontrak, dan tiga kontrak sisanya, **tetap `draft`**. Dasar kewenangan: seluruh enum dan tabel yang disentuh bagian tersebut — `EmergencyVisitStatus`, `TrxEmergencyVisit`, `TrxEmergencyTriage` — milik `EmergencyInstallationManagement`, sehingga tidak memerlukan approver modul lain | Rizki Gunawan | **`approved`** | **Rizki Gunawan / 2026-08-24** | Jawaban pengguna 24 Agustus 2026, pilihan A pada gerbang `/qv-plan` |

### Yang dibuka approval ini

`/qv-plan` boleh menghasilkan task untuk `EPIC IGD-03` — `FR-IGD-013`, `FR-IGD-014`,
`FR-IGD-015` — dengan kontrak terkunci pada hash yang tercatat di roadmap revision `2`.

### Yang **tidak** dibuka approval ini

| Hal | Alasan |
| --- | --- |
| `EPIC IGD-05`, `06`, `07`, `08` | Kontraknya masih `draft` meski keputusannya sudah ada |
| Dua isi `MVP-0` selain `EPIC IGD-03` | Master kelas pasien IGD dan pemetaan unit ke simpul organisasi menyentuh `MstServiceUnit` dan data master milik **Master Data**, yang pemiliknya belum ditunjuk |
| Otorisasi menjalankan migration | Tetap belum diberikan. `EPIC IGD-03` memang tidak membutuhkannya |
| Status blueprint revision `5` | Tetap `draft` |

### Temuan yang mengubah bukti acceptance

`QuilvianSystemBackend.Tests` **ada** di dalam solution — project xUnit dengan
`Microsoft.EntityFrameworkCore.InMemory`, ber-`ProjectReference` ke project utama. Dicatat
sebagai `IGD-EV-094`.

Ini membantah dua catatan yang beredar: `NewQuilvianSystemBackend/CLAUDE.md` yang menyatakan
solution *"hanya berisi satu project — tidak ada test project sama sekali"*, dan catatan pada
laporan-laporan `BE-IGD-*` sebelumnya yang menyimpulkan `AT-IGD-*` tidak dapat dijalankan.

Akibatnya, butir 1 Definition of Done (`04-prd-to-mvp.md` bagian 6) — *"seluruh functional
requirement gelombangnya punya test yang lulus"* — **dapat dipenuhi** untuk `EPIC IGD-03`,
dan roadmap revision `2` menuntutnya sebagai bukti, bukan mengecualikannya.
