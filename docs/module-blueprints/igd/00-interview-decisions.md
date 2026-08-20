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
