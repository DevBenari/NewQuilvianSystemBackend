# Dokter / Rawat Jalan Billing - Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `RJ-BIL-BP-001` |
| Revision | `18` — Amendment Pass `2026-09-28` (PRD V2) di bagian akhir |
| Status | `draft` untuk butir yang belum diberi approval formal; `RJ-BIL-GATE-DEC-001` s.d. `009` sudah `OWNER_APPROVED` lewat Approval Amendment `2026-08-20` di bagian akhir |
| Interview mode | `Closure pass` |
| Final accountable owner | Direksi Rumah Sakit atau pejabat eksekutif dengan delegasi formal; assignment belum dilampirkan |
| Backend SHA | `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` |
| Frontend SHA | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| Input revision/hash | Initial closure `ad344a...`; ownership `44a839...`; multi-payer `a57c3f...`; Laboratory `4d4447...`; Radiology `3da3a2...`; Actual Consumption `e16a81...`; Financial Governance `f9125f...`; Pharmacy Ownership `d86aba...`; Reliability `7ffa20...`; Payer Integration `d9fc83e18bdc5a209fa152b83eae733b090f62ddd8284836dc565efa49450e73` |

## Scope and Outcome

- **Fact `RJ-BIL-FACT-001`:** Menurut pemilik kebutuhan, menu Dokter/Rawat Jalan telah memiliki resep, tindakan, laboratorium, radiologi, dan layanan penunjang lain, tetapi alurnya belum lengkap sampai billing.
- **Decision `RJ-BIL-DEC-001` (draft):** `Clinical Order` tidak sama dengan `Final Financial Charge`. Tarif dan coverage dapat dihitung atau di-snapshot ketika order dibuat, sedangkan charge mengikuti milestone layanan.
- **Decision `RJ-BIL-DEC-003` (draft):** Satu `EncounterId` menjadi satu billing account/folio Rawat Jalan yang mengonsolidasikan administrasi, registrasi, poli, dokter, tindakan, obat, laboratorium, radiologi, dan layanan penunjang lain.
- **Decision `RJ-BIL-DEC-003A` (draft):** Settlement dalam folio yang sama dapat dipisahkan menurut payer, termasuk asuransi, perusahaan, patient excess, tunai, dan FOC.
- **Assumption `RJ-BIL-ASM-001`:** Nama canonical blueprint adalah `rawat-jalan` dengan prefix `RJ-BIL`; perubahan identitas memerlukan revisi eksplisit sebelum kontrak turunannya diterbitkan.
- **Assumption `RJ-BIL-ASM-002`:** Pernyataan bahwa capability sudah tersedia merupakan evidence wawancara, bukan fakta source code, sampai capability audit selesai.

## Actors, Ownership, and Invariants

| ID | Type | Item | Owner | Status |
|---|---|---|---|---|
| `RJ-BIL-OWN-001` | Decision | Direksi Rumah Sakit atau pejabat eksekutif dengan delegasi formal adalah final accountable owner | Direksi/delegated executive | `draft` |
| `RJ-BIL-OWN-002` | Decision | Clinical sign-off melibatkan Pelayanan Medis/Clinical Governance, Rawat Jalan, Farmasi, Laboratorium, Radiologi, dan Komite Medis bila diperlukan | Clinical governance | `draft` |
| `RJ-BIL-OWN-003` | Decision | Financial sign-off melibatkan Finance, Billing/Revenue Cycle, serta Insurance/Corporate Relation bila menyangkut payer atau claim | Financial governance | `draft` |
| `RJ-BIL-OWN-004` | Decision | IT/HIS/Developer mengimplementasikan keputusan dan bukan pemilik business rule klinis atau finansial | Final accountable owner | `draft` |

Invariants:

1. Order klinis tidak otomatis menjadi final charge.
2. Seluruh transaksi tetap tertaut ke `EncounterId` yang sama; pemisahan payer tidak membuat encounter baru.
3. Modul klinis tidak boleh membatalkan charge secara langsung setelah item masuk billing.
4. Koreksi finansial memakai void, adjustment, reversal, refund, FOC, atau write-off yang sesuai; histori tidak boleh dihapus.
5. Semua cancellation dan correction wajib mencatat actor, waktu, reason, status sebelum/sesudah, serta audit trail.
6. Transaksi `paid`, `posted`, atau `claimed` memerlukan approval supervisor sesuai kewenangan yang disahkan.
7. Repeat atau kegagalan akibat kesalahan internal rumah sakit tidak otomatis dibebankan kepada pasien.

## States, Exceptions, and Acceptance Criteria

### Billing milestones

| Service | Sebelum final charge | Trigger/milestone charge | Bukan trigger |
|---|---|---|---|
| Resep | Harga dapat dihitung saat item resep dibuat | Resep difinalkan bersama konsultasi dokter dan billing generated | Penyerahan obat; ini fulfillment |
| Tindakan | Harga dapat dihitung saat tindakan dipilih | Tindakan benar-benar dieksekusi dan `Completed` | Pemilihan/order saja |
| Laboratorium | Order dokter belum menjadi final charge | Order/specimen diterima dan divalidasi Laboratorium untuk diproses | Terbitnya hasil |
| Radiologi | Charge dapat dipersiapkan setelah acceptance/validation | Pemeriksaan/acquisition benar-benar dilakukan | Terbitnya hasil |

### Cancellation and correction authority

1. Dokter dapat mengubah atau membatalkan order ketika masih draft atau belum diambil alih unit pelaksana.
2. Setelah unit pelaksana mulai memproses, dokter hanya dapat mengajukan pembatalan; unit pelaksana memutuskan kelayakannya berdasarkan kondisi aktual.
3. Setelah item masuk billing, koreksi dilakukan oleh Billing/Finance melalui workflow finansial.
4. Item `paid`, `posted`, atau `claimed` wajib melalui approval supervisor sesuai kewenangan.

### Actual Consumption Rule

- Belum ada pelayanan atau material terpakai: full void atau tidak ditagihkan.
- Sebagian pelayanan atau material sudah dikonsumsi: partial charge berdasarkan konsumsi aktual dan kebijakan rumah sakit.
- Pelayanan selesai: full charge.
- Repeat/kegagalan internal rumah sakit: FOC/write-off melalui workflow berwenang.
- Imaging/acquisition yang sudah dilakukan dianggap layanan terlaksana walaupun hasil belum terbit.

### Testable acceptance criteria

1. Membuat order klinis tidak menghasilkan final charge sebelum milestone layanan terkait tercapai.
2. Sistem menghasilkan charge tepat satu kali ketika milestone terpenuhi; retry atau event duplikat tidak menggandakan charge.
3. Semua charge dari satu kunjungan tertaut ke satu `EncounterId` dan dapat dialokasikan ke lebih dari satu payer tanpa membuat encounter baru.
4. Cancellation sebelum konsumsi menghasilkan full void; partial processing menghasilkan partial charge; completed service menghasilkan full charge.
5. Dokter tidak dapat melakukan direct financial cancellation setelah billing terbentuk.
6. Void, adjustment, reversal, refund, FOC, dan write-off mempertahankan histori dan audit trail.
7. Koreksi atas item `paid`, `posted`, atau `claimed` ditolak tanpa approval supervisor yang valid.
8. Kegagalan internal rumah sakit dapat diarahkan ke FOC/write-off dan tidak otomatis menjadi patient responsibility.
9. Status hasil Lab/Radiologi tidak menjadi syarat pembentukan charge apabila milestone operasional yang ditetapkan sudah tercapai.
10. Partial failure antara layanan klinis dan billing memiliki status yang dapat direkonsiliasi tanpa silent loss atau duplicate charge.

## UI Decision Authority

- Security, privacy, invariant, dan kontrak backend yang disetujui mengungguli keputusan presentasi.
- Dokumen ini tidak memutus menu, route, layout, visual treatment, wording tombol, atau komponen UI.
- UI harus membedakan order klinis, fulfillment/progress layanan, billing status, payer allocation, dan financial correction; detail interaksi menunggu capability audit dan UI authority yang sah.

## Decision Log

| Decision ID | Type | Item | Owner | Status | Approval evidence |
|---|---|---|---|---|---|
| `RJ-BIL-DEC-001` | Decision | Clinical Order tidak langsung menjadi Final Financial Charge | Joint clinical-financial governance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-002` | Decision | Billing milestone berbeda untuk resep, tindakan, laboratorium, dan radiologi | Pemilik klinis tiap layanan + Billing/Finance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-003` | Decision | Seluruh charge Rawat Jalan dikonsolidasikan berdasarkan satu `EncounterId` | Rawat Jalan + Billing/Finance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-004` | Decision | Cancellation/correction memakai status-based authority dan pemisahan clinical correction dari financial adjustment | Clinical governance + Billing/Finance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-005` | Decision | Partial processing menggunakan Actual Consumption Rule | Unit pelaksana + Billing/Finance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-006` | Decision | Joint governance dengan Direksi/delegated executive sebagai final accountable owner | Direksi/delegated executive | `draft` | Jawaban closure pengguna; governance assignment belum dilampirkan |
| `RJ-BIL-DEC-007` | Decision | **Sukma Giri ditunjuk sebagai Product/Domain Owner `LaboratoryManagement`**, dan lifecycle modul itu pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dinaikkan dari `PLANNED` menjadi `ACTIVE`. Prefix `Lab` yang sudah terdaftar tidak berubah. Sejak titik ini `QBE-MOD-002` tidak lagi menahan pembuatan entity operasional `Lab*`, sehingga `LabSpecimen` dan `LabTransitionHistory` dari `RJ-BIL-BE-003` menjadi sah kepemilikannya. **Wewenang eksekusi database di luar lokal dan deployment tetap terpisah** dan tidak diberikan oleh keputusan ini. `FORMAL_LAB_GOVERNANCE_SIGNOFF` dan `CLINICAL_GOVERNANCE_SIGNOFF` tetap `OPEN` | Sukma Giri | `approved` | Sukma Giri, 26 Agustus 2026. Penunjukan dilakukan oleh pemilik blueprint `RJ-BIL-BP-001` atas modul yang sebelumnya tidak bertuan; **tidak ada countersignature sponsor governance terpisah**. Menjawab permintaan `IGD-REQ-001` baris `80` dan melepas alasan `draft` pada `IGD-DEC-087`, keduanya milik blueprint IGD — pemilik IGD perlu diberi tahu, dan berkas IGD sengaja tidak disunting dari sisi ini |
| `RJ-BIL-DEC-008` | Decision | Urutan pengerjaan backend mengikuti **kolom Dependency pada tabel task**, bukan baris `Dependency sequence` naratif yang bertentangan dengannya. `RJ-BIL-BE-007` hanya bergantung pada `RJ-BIL-BE-001` dan Integration owner, sehingga berjalan mendahului `RJ-BIL-BE-005` yang `BLOCKED`. Dasar teknisnya: acceptance criteria `RJ-BIL-BE-006` berbunyi *close ditolak saat reconciliation pending*, sehingga `BE-006` justru bergantung pada `BE-007`; dan rekonsiliasi `BE-007` bekerja pada outcome pemrosesan fakta klinis, tidak menyentuh alokasi multi-payer sama sekali. `BUILDER_EXECUTION` untuk `RJ-BIL-BE-007` dinaikkan menjadi `AUTHORIZED` | Sukma Giri | `approved` | Sukma Giri, 26 Agustus 2026 |
| `RJ-BIL-DEC-009` | Decision | Integration test Billing boleh berjalan terhadap database dev bersama `QuilvianNewDevTim01`, **hanya** melalui opt-in kedua `QUILVIAN_BILLING_TEST_DB_ALLOW_SHARED` yang harus diketik sengaja. Perilaku bawaan tetap fail-closed. Penanda `prod`, `production`, `live`, `staging`, `stage`, dan `uat` ditolak **mutlak** dan tidak mengenal opt-in. Fallback diam ke `appsettings.Development.json` tetap tidak dihidupkan kembali. Membalik larangan sebelumnya pada handoff `RJ-BIL-BE-003`; yang dipertahankan dari larangan itu adalah intinya, yaitu tidak boleh ada migration yang terpasang ke database tim tanpa seseorang menyatakannya lebih dulu | Sukma Giri | `approved` | Sukma Giri, 26 Agustus 2026. Dasar: hanya 2 migration tertunda dan keduanya milik blueprint ini — satu aditif, satu rename murni tanpa mutasi baris |
| `RJ-BIL-DEC-010` | Decision | Ambang **financially material** pada gerbang penutupan folio `RJ-BIL-GATE-DEC-008` diwujudkan sebagai master data yang dapat diubah admin tanpa rilis, dengan nilai awal **nol** — sehingga untuk sekarang setiap permanent failure memblokir penutupan folio. Nol dipilih karena merupakan perilaku paling aman dan bukan angka karangan. Angka sebenarnya tetap `OWNER_DECISION_REQUIRED`. Pola yang sama diterapkan pada durasi SLA dan skala prioritas reconciliation case, yang menurut `RJ-BIL-GATE-DEC-008` hanya memicu peringatan, eskalasi, dan visibilitas serta tidak pernah menyentuh keputusan finansial | Sukma Giri | `approved` | Sukma Giri, 26 Agustus 2026 |
| `RJ-BIL-DEC-011` | Decision | **`RJ-BIL-BE-006` memiliki entity approval-nya sendiri dengan prefix `Bil`**, bukan menumpang mesin Workflow milik `Areas/Corporate/HumanResource/`. Dasarnya: dua invariant `RJ-BIL-GATE-DEC-006` tidak dapat ditegakkan di sana. Pertama, `MstWorkflowStep.AllowSelfApproval` adalah `bool` per step yang dapat diubah menjadi `true` lewat `WorkflowStepController`, sedangkan larangan self-approval pada `GATE-DEC-006` bersifat tanpa syarat — artinya invariant finansial dapat dimatikan dari layar konfigurasi modul lain tanpa sepengetahuan Billing. Kedua, penyaringan maker hanya terjadi sekali saat assignment dibuat (`WorkflowService.cs:543`); `ApproveAsync` tidak pernah membandingkan penyetuju dengan `RequestedByUserId`, dan `ApprovalDelegationService` tidak merujuk `RequestedByUserId` sama sekali, sehingga delegasi dapat mengembalikan persetujuan kepada pengajunya — kasus yang justru dilarang eksplisit oleh `GATE-DEC-006`. Ketiga, ketiadaan approver valid menggagalkan permintaan dengan `400` (`WorkflowService.cs:556`), sedangkan `GATE-DEC-006` menuntut permintaan **bertahan** sebagai `PendingApproval` atau `BlockedByPolicyConfiguration`. Duplikasi kapabilitas persetujuan diterima **secara sadar** sebagai harganya. Keputusan ini **tidak** menutup jalan ke mesin bersama: bila kelak tersedia, permintaan approval Billing dapat dicerminkan ke sana tanpa memindahkan kewenangan finansialnya | Sukma Giri | `approved` | Sukma Giri, 27 Agustus 2026. Bukti dan penelusurannya pada [preflight-RJ-BIL-BE-006.md](preflight-RJ-BIL-BE-006.md) bagian `4`. Temuan terhadap modul Kepegawaian adalah **pengamatan read-only** yang dibatasi pada tiga titik penegakan di atas, bukan laporan cacat modul itu; tidak ada satu berkas pun miliknya yang disunting |
| `RJ-BIL-DEC-012` | Decision | **`BUILDER_EXECUTION` untuk `RJ-BIL-BE-006` dinaikkan menjadi `AUTHORIZED`** atas otoritas Sukma Giri selaku pemilik blueprint `RJ-BIL-BP-001`. `FORMAL_FINANCE_SIGNOFF` dan `SECURITY_PRIVACY_SIGNOFF` tetap `OPEN`, **tidak** diberikan oleh keputusan ini, dan menjadi syarat sebelum aktivasi production — mengikuti pola yang sudah dipakai `RJ-BIL-BE-003` dan `RJ-BIL-BE-007`. Owner Workflow keluar dari jalur kritis karena `RJ-BIL-DEC-011` membuat Billing tidak lagi bergantung pada mesin Workflow modul lain. Wewenang ini mencakup penulisan code, pembuatan migration, dan eksekusi test; **tidak** mencakup deployment, commit, push, merge, maupun penerapan migration ke database di luar yang sudah diizinkan `RJ-BIL-DEC-009`. `IMPLEMENTATION COMPLETE` tetap **tidak** sama dengan `PRODUCTION GOVERNANCE APPROVED` | Sukma Giri | `approved` | Sukma Giri, 27 Agustus 2026. Menutup satu dari dua gate yang gagal pada [preflight-RJ-BIL-BE-006.md](preflight-RJ-BIL-BE-006.md) bagian `6`; gate sign-off Finance dan Security/Privacy **sengaja dibiarkan gagal** dan tetap tercatat sebagai blocker production |
| `RJ-BIL-DEC-013` | Decision | **`IMPLEMENTATION_AUTHORITY` dinaikkan menjadi `GRANTED` dan `BUILDER_EXECUTION` menjadi `AUTHORIZED` pada roadmap frontend `RJ-BIL`**, atas otoritas Sukma Giri selaku pemilik blueprint `RJ-BIL-BP-001`. Wewenang berlaku pada empat task yang backend pasangannya sudah selesai dan terbukti lulus test: `RJ-BIL-FE-001`, `RJ-BIL-FE-002` **bagian Lab saja**, `RJ-BIL-FE-004`, dan `RJ-BIL-FE-005`. Tiga task sisanya `RJ-BIL-FE-003`, `RJ-BIL-FE-006`, dan `RJ-BIL-FE-007` tetap `NOT_AUTHORIZED`, **bukan karena wewenangnya ditahan** melainkan karena endpoint pasangannya belum ada sama sekali; menaikkannya sekarang hanya akan membuat task tanpa backend tampak siap dikerjakan. Bagian Radiologi pada `RJ-BIL-FE-002` ikut dikecualikan selama `RJ-BIL-BE-004` terblokir. Wewenang mencakup penulisan komponen, state, klien HTTP, dan test frontend; **tidak** mencakup commit, push, merge, deployment, perubahan backend, maupun aktivasi `RJ-BIL-DEP-009`. `FRONTEND_AUTHORITY` pada `03-frontend-architecture.md` bagian `7` dan `SECURITY_PRIVACY_SIGNOFF` tetap `OPEN` dan **tidak** diberikan oleh keputusan ini | Sukma Giri | `approved` | Sukma Giri, 28 Agustus 2026. Mengikuti pola `RJ-BIL-DEC-008` dan `RJ-BIL-DEC-012`. Dasar kesiapan backend: `RJ-BIL-BE-001`, `BE-002`, `BE-003`, `BE-006`, dan `BE-007` selesai dengan bukti 157 test lulus pada 27 Agustus 2026 |
| `RJ-BIL-DEC-014` | Decision | **Sukma Giri ditunjuk sebagai Product/Domain Owner `RadiologyManagement`**, dan lifecycle modul itu pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dinaikkan dari `PLANNED` menjadi `ACTIVE`. Prefix `Rad` yang sudah terdaftar tidak berubah. Sejak titik ini `QBE-MOD-002` tidak lagi menahan pembuatan entity operasional `Rad*`, sehingga `RJ-BIL-BE-004` keluar dari status `BLOCKED`. **Isi gerbang keselamatan mengikuti standardisasi rumah sakit Indonesia pada umumnya, bukan SOP tertulis rumah sakit ini** — karena itu seluruh ambang dan daftar pemeriksaannya diwujudkan sebagai **master data yang dapat diubah admin tanpa rilis**, mengikuti pola `RJ-BIL-DEC-010`, dan **tidak** ditanam sebagai konstanta di source. Nilai bawaannya fail-closed: gerbang yang belum dikonfigurasi **menolak** acquisition, bukan meloloskannya. `FORMAL_RADIOLOGY_SAFETY_SIGNOFF` dan `CLINICAL_GOVERNANCE_SIGNOFF` tetap `OPEN`. Wewenang eksekusi database di luar lokal dan deployment tetap terpisah dan **tidak** diberikan keputusan ini | Sukma Giri | `approved` | Sukma Giri, 28 Agustus 2026. Mengikuti pola `RJ-BIL-DEC-007` untuk `LaboratoryManagement`. Penunjukan dilakukan pemilik blueprint atas modul yang sebelumnya tidak bertuan; **tidak ada countersignature sponsor governance klinis terpisah**. Baseline standardisasi Indonesia bersifat **tidak otoritatif** dan wajib diverifikasi terhadap SOP rumah sakit yang sebenarnya sebelum aktivasi production |
| `RJ-BIL-DEC-015` | Decision | **Satu kunjungan tetap tepat satu penanggung, dan tidak ada pembagian tagihan ke beberapa penanggung dalam bentuk apa pun.** Menjawab `RJ-BIL-OQ-001` dengan pilihan A (kunci satu-penanggung dipertahankan; pendaftaran, kiosk, dan laporan tidak berubah), `RJ-BIL-OQ-004` dengan pilihan B (dua asuransi pada satu kunjungan **tidak pernah** terjadi — penentuan layanan mana ditanggung siapa akan membingungkan penagihan ke asuransi), dan `RJ-BIL-OQ-005` dengan **tidak ada pembagian pada tingkat mana pun**. Peninggalan kode peran penjamin `Primary`, `Secondary`, `Tertiary`, `ExcessPayer`, `CoPaymentPayer`, dan `Backup` — yang tidak dipakai kode mana pun — karena itu boleh dibersihkan. **Konsekuensi yang tidak boleh dilewati: premis `RJ-BIL-BE-005` gugur.** Task itu berjudul *"Satu tagihan dapat dibagi ke beberapa penanggung"* dan acceptance criteria-nya menuntut `Rp1.000.000` menjadi A `Rp600.000` + B `Rp250.000` + pasien `Rp150.000`. Skenario itu kini dinyatakan tidak pernah terjadi, sehingga `RJ-BIL-BE-005` **wajib ditinjau ulang scope-nya**, bukan dikerjakan apa adanya. `RJ-BIL-CONFLICT-001` ditutup: konflik antara kunci satu-penanggung as-is dan target multi-payer hilang karena targetnya yang dicabut, bukan kuncinya | Sukma Giri | `approved` | Sukma Giri, 28 Agustus 2026. Menjawab tiga dari tujuh pertanyaan pada [owner-decision-request-RJ-BIL-001.md](owner-decision-request-RJ-BIL-001.md). `RJ-BIL-OQ-002`, `OQ-003`, `OQ-006`, dan `OQ-007` **belum dijawab** dan sebagian menjadi tidak relevan karena keputusan ini |
| `RJ-BIL-DEC-016` | Discovery | **Asuransi perusahaan ditangani sebagai reimbursement, bukan sebagai penanggung kedua.** Ketika pasien memiliki asuransi dari perusahaannya, tagihan **tetap ditagihkan kepada pasien**; rumah sakit menerbitkan **dokumen reimbursement** yang kemudian diproses sendiri oleh pasien ke asuransi perusahaannya. Rumah sakit tidak pernah menagih asuransi perusahaan secara langsung. **Kemampuan ini tidak ada pada satu pun task yang sudah disetujui** — tidak pada `RJ-BIL-BE-005`, tidak pada `RJ-BIL-BE-008`, dan tidak pada blueprint mana pun. Ia ditemukan saat menjawab `RJ-BIL-OQ-004` pada 28 Agustus 2026 dan **sengaja tidak diselundupkan** ke dalam task yang ada. Statusnya: kebutuhan nyata yang belum punya task, belum punya kontrak, dan belum punya acceptance criteria | Sukma Giri | `discovered` | Sukma Giri, 28 Agustus 2026. Butuh keputusan terpisah apakah dokumen reimbursement masuk cakupan rilis pertama. Bila ya, ia memerlukan task backend tersendiri beserta kontraknya; bila tidak, ia wajib tercatat sebagai batas modul yang diketahui, supaya tidak ditemukan sebagai kejutan saat go-live |

| `RJ-BIL-DEC-017` | Decision | **Target database test dipindahkan dari `QuilvianNewDevTim01` ke `QuilvianNewDevSukma`.** Menggantikan nama database yang disebut `RJ-BIL-DEC-009`; **seluruh pengamannya tidak berubah sedikit pun**. **KOREKSI 31 Agustus 2026:** kalimat semula pada keputusan ini menyatakan `QuilvianNewDevSukma` “menuntut opt-in `QUILVIAN_BILLING_TEST_DB_ALLOW_SHARED`”. **Itu keliru — variable tersebut tidak ada di dalam kode.** Pembacaan langsung `BillingTestDatabaseFixture` membuktikan `dev` terdaftar pada `ForbiddenDatabaseMarkers`, sederajat dengan `prod` dan `production`, dan komentar kodenya menyatakan tegas “tidak disediakan override apa pun”. Fixture juga menuntut bukti afirmatif berupa penanda `test` pada nama database. Akibatnya `QuilvianNewDevSukma` **tidak akan pernah** dapat menjadi sasaran integration test, dengan cara apa pun, dan keputusan ini hanya berlaku untuk penerapan migration — bukan untuk menjalankan test. **Tidak ada satu baris pun kode penjaga yang dilonggarkan untuk mengakomodasi perpindahan ini.** Penanda `prod`, `production`, `live`, `staging`, `stage`, dan `uat` tetap ditolak mutlak tanpa jalan keluar. Perpindahan ini justru **memperkecil** radius dampak: dari database yang dipakai bersama satu tim menjadi database pengembangan milik pemilik blueprint sendiri, sehingga kesalahan yang terjadi hanya mengganggu dirinya | Sukma Giri | `approved` | Sukma Giri, 28 Agustus 2026, dengan menunjuk `appsettings.Development.json`. **Catatan keamanan:** connection string beserta username dan password-nya sempat terkirim sebagai teks biasa di dalam sesi kerja pada tanggal yang sama. Kredensialnya tidak dituliskan ke berkas mana pun oleh pekerjaan ini, tetapi ia sudah berada di luar tempat penyimpanan aslinya; **rotasi kredensial disarankan**, terutama karena host-nya beralamat IP publik |
| `RJ-BIL-DEC-018` | Discovery | **`ApplicationDbContextModelSnapshot` meleset jauh dari model sebenarnya, dan `dotnet ef migrations add` tidak lagi aman dipakai apa adanya.** Pada 28 Agustus 2026 perintah `dotnet ef migrations add AddRadiologyManagement` menghasilkan 13.549 baris. Dari 1.006 operasi pada `Up()`, hanya **34** milik Radiology; 972 sisanya (96,6%) milik pihak lain: 24 `RenameTable`, 35 `DropColumn` atas kolom bayangan `TempId`…`TempId11` pada tabel `Bil*`, 1 `DropTable` atas `TrxEmergencyTransfer`, serta 18 `CreateTable` milik modul `Opr*`, `Emg*`, `BilNumberSeries`, dan `MstAdministrationFeePolicy`. **Sebab:** empat migration ditulis tangan tanpa memperbarui snapshot — `20260826090500_ImplementIgdFullPatientJourney`, `20260827030000_RenameEmergencyDepartureToEmgPrefix`, `20260827040000_RenameEmergencyMasterDataToEmgPrefix`, `20260827050000_RenameEmergencyTriageDetailToEmgPrefix` — sehingga EF mengira pekerjaan modul lain masih tertunda. **Akibat bagi siapa pun berikutnya:** menerapkan hasil generate berarti menjatuhkan tabel dan kolom milik tim lain serta membuat tabel dari pekerjaan mereka yang belum selesai; kemungkinan besar gagal di tengah dan meninggalkan basis data dalam keadaan setengah jalan. **Tindakan pada task ini:** migration hasil generate dibuang, snapshot dikembalikan ke keadaan `HEAD`, dan migration Radiology ditulis tangan berisi tepat 34 operasi tersebut. **Melesetnya snapshot TIDAK diperbaiki di sini** — memperbaikinya menuntut wewenang atas modul `Opr*` dan `Emg*` yang bukan milik blueprint ini, dan regenerasi snapshot secara sepihak akan menghapus kemampuan pemiliknya membuat migration mereka sendiri | Pemilik modul `OperatingTheatreManagement` dan `EmergencyInstallationManagement`, bersama pemilik arsitektur basis data | `OWNER_DECISION_REQUIRED` | Temuan mesin, 28 Agustus 2026, dari keluaran `dotnet ef migrations add` yang dibuang beserta jejak analisisnya. **Belum ada pemilik yang ditunjuk dan belum ada rencana perbaikan.** Selama ini belum ditangani, setiap `migrations add` berikutnya akan mengulang masalah yang sama, dan migration Radiology ini menambah satu lagi migration tanpa pembaruan snapshot — utang yang disadari, bukan yang tersembunyi |
| `RJ-BIL-DEC-019` | Decision | **`QuilvianNewDevSukma` boleh menjadi sasaran integration test, hanya lewat opt-in eksplisit `QUILVIAN_BILLING_TEST_DB_ALLOW_PERSONAL`.** Membalik bagian `RJ-BIL-DEC-017` yang menyatakan database itu *"tidak akan pernah dapat menjadi sasaran integration test, dengan cara apa pun"*. **Dasarnya:** setiap developer memakai database pengembangan personal masing-masing yang dapat di-restore, dan perubahan baru disatukan ke database master lewat migration terpisah, sehingga kesalahan pada test hanya berdampak ke database milik pemiliknya sendiri. **Bentuk pengamannya:** opt-in berlaku hanya bila nilainya **sama persis** dengan nama database pada `QUILVIAN_BILLING_TEST_DB`, dan nilai yang berbeda ditolak, bukan diabaikan. Yang dilepas hanya penanda `dev` dan tuntutan penanda `test`. `QuilvianNewDevTim01` beserta penanda `prod`, `production`, `live`, `staging`, `stage`, `uat`, dan `shared` tetap ditolak mutlak. Tanpa variable itu perilaku fixture **tidak berubah** bagi developer lain, dan fallback diam ke `appsettings.Development.json` tetap tidak dihidupkan. Opt-in `QUILVIAN_BILLING_TEST_DB_ALLOW_SHARED` dari `RJ-BIL-DEC-009` tetap tidak ada di kode dan tidak dihidupkan keputusan ini. **Pemicunya:** `PLT-BE-004` pada blueprint Platform tidak dapat membuktikan `AC-PLT-003`, `004`, `005`, dan `012` karena role database tidak berhak membuat database test baru, sementara pemilik memilih untuk tidak membuat database test baru | Sukma Giri | `approved` | Sukma Giri, 10 September 2026, lewat jawaban eksplisit pada sesi kerja `PLT-BE-004`. Diwujudkan pada `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Infrastructure/BillingTestDatabaseFixture.cs`. Wewenang eksekusi test yang menyertainya **hanya** untuk `QuilvianNewDevSukma`; database lain tetap menuntut wewenang tersendiri |
## Open Questions and Blockers

> **Pemutakhiran 28 Agustus 2026.** `RJ-BIL-DEC-015` menjawab tiga pertanyaan pada
> [owner-decision-request-RJ-BIL-001.md](owner-decision-request-RJ-BIL-001.md):
> `RJ-BIL-OQ-001` (satu penanggung per kunjungan — pilihan A), `RJ-BIL-OQ-004` (dua asuransi
> **tidak pernah** terjadi — pilihan B), dan `RJ-BIL-OQ-005` (**tidak ada pembagian** pada
> tingkat mana pun). `RJ-BIL-OQ-002`, `OQ-003`, `OQ-006`, dan `OQ-007` belum dijawab; sebagian
> di antaranya menjadi tidak relevan karena tidak ada lagi pembagian yang perlu ditentukan
> waktunya. `RJ-BIL-DEC-014` menutup blocker kepemilikan `RadiologyManagement`.
>
> `RJ-BIL-DEC-016` menambah satu kebutuhan **baru** yang belum punya task: dokumen reimbursement
> untuk pasien berasuransi perusahaan.


| ID | Open question/blocker | Owner | Blocks |
|---|---|---|---|
| `RJ-BIL-OQ-001` | Nama, jabatan, masa berlaku, dan bukti delegasi final accountable owner belum tersedia | Direksi Rumah Sakit | Approval formal dan production activation |
| `RJ-BIL-OQ-002` | Bukti sign-off dari tiap clinical dan financial owner belum tersedia | Joint clinical-financial governance | Status `approved` dan implementation authorization |
| `RJ-BIL-OQ-003` | Rumus, unit konsumsi, pembulatan, minimum charge, dan tariff component untuk partial charge belum ditetapkan | Unit layanan + Finance/Billing | Desain detail partial charge |
| `RJ-BIL-OQ-004` | Matriks nominal/risiko yang menentukan supervisor approval untuk void, reversal, refund, FOC, dan write-off belum ditetapkan | Finance/Billing | Desain approval finansial detail |
| `RJ-BIL-OQ-005` | Perilaku outage, event terlambat, duplicate request, dan rekonsiliasi partial failure belum dipetakan terhadap capability existing | IT architecture melalui capability audit; business owner untuk conflict | Desain integrasi dan implementasi |
| `RJ-BIL-OQ-006` | Nama status, endpoint, entity, payer allocation, prescription finalization, dan billing generation yang benar belum dibuktikan dari source | Capability audit | Desain berbasis source dan implementation planning |

## Next Step

Jalankan `trace-existing-capabilities` terhadap snapshot backend dan frontend yang tercatat di manifest. Audit harus memetakan resep, tindakan, Lab, Radiologi, encounter, billing/folio, payer allocation, cancellation/correction, authorization, audit, idempotency, dan reconciliation sebagai `READY TO REUSE`, `REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, atau `UNKNOWN`. Jangan mulai implementasi sebelum hasil audit dan approval yang relevan tersedia.

## Closure Amendment 2026-08-19 - Financial Ownership

### `RJ-BIL-GATE-DEC-001`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Canonical ownership untuk clinical facts, billing, payer approval, payment, dan accounting |
| Requirement owner | Product/domain owner response |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH CLARIFICATION`, SHA-256 `44a8394596f169de6240536d1beaafe053411a643ae9ab29544ad2895b32113c` |
| Formal governance status | `OPEN` - nama/jabatan/delegasi dan joint governance sign-off belum dilampirkan |

Keputusan requirement yang dikunci:

1. `Clinical Domain` memiliki clinical order, clinical lifecycle, execution/fulfillment, clinical cancellation/correction, actual service quantity/material, dan fakta milestone billable.
2. `Billing/Revenue Cycle` adalah satu-satunya source of truth untuk billing account/folio, charge line, financial identifier, charge aggregation berdasarkan `EncounterId`, billing generation, payer allocation, patient responsibility/excess, adjustment, void sesuai kewenangan, serta charge lifecycle.
3. `Insurance/Payer Management` memiliki coverage/authorization outcome, termasuk `InsuranceApproved`.
4. `Cashier` memiliki payment collection, settlement, receipt, payment cancellation sesuai kewenangan, dan refund execution setelah approval.
5. `Finance` memiliki accounting posting, financial reconciliation, reversal/refund accounting, closing, dan GL consequence.
6. `EncounterId` hanya correlation/aggregation key. Encounter boleh membaca financial summary, tetapi tidak membuat atau memutasi charge.
7. Clinical module tidak boleh menetapkan `Paid`, `InsuranceApproved`, `PaymentWaived`, `BillingVoided`, `Refunded`, `Reversed`, `Settled`, atau `Reconciled`.
8. Clinical module menyerahkan facts seperti prescription submission, verification/dispensing, procedure completion, Lab acceptance/performance, Radiology performance, cancellation, dan actual-consumption change. Nama event final belum diputuskan sebagai kontrak implementasi.
9. Clinical cancellation dan financial cancellation adalah transaksi berbeda. Clinical change setelah charge terbentuk menghasilkan fact; Billing menentukan no adjustment, additional/partial adjustment, void, atau reversal menurut state dan Actual Consumption Rule.
10. `PaymentWaived` adalah financial authorization dengan reason, actor, timestamp, serta approval berdasarkan threshold/kebijakan; clinical module hanya dapat mengajukan clinical rationale.

### Acceptance criteria tambahan

1. Tidak ada clinical endpoint yang authoritative untuk financial status atau nominal final charge.
2. Financial status yang dibaca modul klinis berasal dari canonical Billing/Payer/Finance owner.
3. Clinical cancellation tidak menghapus charge dan tidak otomatis menentukan financial outcome.
4. Billing dapat menelusuri setiap charge ke `EncounterId`, clinical source, dan source milestone.
5. Cashier tidak dapat mengubah fakta klinis atau menghapus clinical order.

### Conflict yang terkonfirmasi

`RJ-BIL-CONFLICT-006`: Current Pharmacy flow masih mempunyai `MarkBillingGenerated`, `MarkPaid`, `MarkInsuranceApproved`, `MarkPaymentWaived`, dan cancellation yang mengubah payment status. Perilaku ini bertentangan dengan ownership decision. Conflict dicatat sebagai implementation evidence untuk downstream design; source tidak diubah pada closure pass.

## Closure Amendment 2026-08-19 - Multi-Payer

### `RJ-BIL-GATE-DEC-002`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Multi-payer allocation dan settlement semantics |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH CLARIFICATIONS`, SHA-256 `a57c3f85ec1d97f6ecf07cecfac9c8f33f39c6ed0ecd0ad6033a041a2baadd00` |
| Formal governance status | `OPEN` - joint governance sign-off belum dilampirkan |

Keputusan requirement yang dikunci:

1. Encounter menyimpan satu `Primary Registration Payer` untuk konteks registrasi, eligibility, estimasi coverage, dan authorization awal; ini bukan source of truth settlement.
2. Billing account/folio adalah canonical financial container multi-payer untuk satu `EncounterId`.
3. Allocation berlaku pada folio level dan charge-line level karena coverage dapat berbeda per layanan/item.
4. Canonical allocation menggunakan nominal absolut. Persentase hanya boleh menjadi rule pendukung, bukan satu-satunya sumber settlement.
5. Setiap allocation minimal menelusuri payer, priority/coordination order, requested/eligible/approved/allocated/rejected amount, patient responsibility, authorization/coverage/settlement status, effective/decision time, reason, actor, dan audit timestamps.
6. Payer berikutnya memproses eligible residual setelah hasil payer sebelumnya sesuai coordination-of-benefits policy.
7. Partial approval/rejection satu payer tidak membatalkan allocation lain. Residual yang tidak ditanggung menjadi official `Patient Responsibility` kecuali financial adjustment sah.
8. Untuk setiap finalized charge line, total active finalized allocations tidak boleh melebihi eligible charge. Residual tidak boleh hilang secara diam-diam.
9. Perubahan payer memakai version/superseding allocation dengan previous reference, reason, actor, timestamp, evidence, dan status history; histori tidak boleh ditimpa.
10. Perubahan payer tidak mengubah clinical fact atau gross charge secara historis; yang berubah adalah allocation dan financial adjustment yang sah.
11. FOC adalah financial waiver/adjustment, bukan payer. FOC membutuhkan amount, reason, approval, authorization level, actor, timestamp, charge reference, dan audit trail.
12. Membership hanya menjadi payer jika mempunyai fund/program yang benar-benar membayar. Discount/benefit/tariff privilege diproses sebagai pricing benefit sebelum allocation.
13. Billing/Revenue Cycle memiliki allocation, patient responsibility, adjudication result, dan allocation version; Payer Management memiliki eligibility/authorization/guarantee/adjudication facts; Cashier dan Finance mengikuti ownership `RJ-BIL-GATE-DEC-001`.

Financial invariants:

- `Gross Charge - Approved Adjustment - Discount/Benefit = Net Eligible Charge`.
- `SUM(Payer Allocations) + Patient Responsibility = Net Eligible Charge` untuk finalized charge line.
- Settlement tidak boleh menghasilkan over-allocation, silent write-off, payer overwrite, histori hilang, payment tanpa source allocation, atau overpayment tanpa workflow overpayment/refund.

Acceptance criteria tambahan:

1. Coverage dapat dialokasikan berbeda pada setiap charge line dalam folio yang sama.
2. Payer rejection/partial approval menghasilkan residual yang eksplisit dan dapat diteruskan ke payer berikutnya atau patient responsibility.
3. Mengganti payer membuat version baru dan mempertahankan prior decision evidence.
4. FOC tidak muncul sebagai payer allocation.
5. Membership discount mengurangi net eligible charge sebelum payer allocation; funded membership dapat menjadi payer.

Conflict disposition:

`RJ-BIL-REQ-X001` ditutup pada level requirement: as-is encounter payment source one-to-one dipertahankan hanya sebagai primary registration payer, sedangkan multi-payer menjadi ownership Billing. Source tetap belum sesuai dan memerlukan downstream design/implementation terpisah.

## Closure Amendment 2026-08-19 - Laboratory

### `RJ-BIL-GATE-DEC-003`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Laboratory order, specimen, result, cancellation, safety, dan charge eligibility lifecycle |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH STRUCTURAL CLARIFICATIONS`, SHA-256 `4d44472028622f6a9c460f78c4ff61c6fa29d57b6ce5f46f8c6d2b820373c2cc` |
| Formal governance status | `OPEN` - Lab, Clinical Governance, dan Billing/Finance sign-off belum dilampirkan |

Lifecycle requirement yang dikunci:

1. Lab Order: `Draft → Requested → Accepted → InProcess → Completed`; exceptions `OnHold`, `CancelRequested`, `Cancelled`.
2. Specimen: `Planned → Collected → Received → Accepted`; exceptions `Rejected`, `RecollectionRequired`, `Cancelled`, `OnHold`.
3. Result: `Pending → InProcess → Completed → Validated → Released`; correction path mempertahankan released history melalui `Corrected/Amended → Revalidated → Released`.
4. `Validated` dan `Released` adalah result states, bukan Lab Order states dan bukan trigger awal billing.
5. Satu Lab Order dapat memiliki lebih dari satu specimen. Recollection membuat specimen identity baru dan mempertahankan specimen sebelumnya serta causal link.

Authority dan safety invariants:

- Dokter memiliki direct mutation sampai `Requested`. Setelah handoff, dokter hanya melihat, menambah informasi klinis, atau mengajukan cancellation/correction.
- Collection, receipt, acceptance/rejection, processing, validation, dan release menggunakan capability berbeda; jabatan organisasi tidak otomatis memberi authority.
- Receipt tidak sama dengan specimen acceptance. Acceptance/rejection merekam decision, actor, time, controlled reason, dan optional note.
- Setiap specimen memiliki identity/barcode operasional sendiri dan deterministic trace ke Lab Order, Encounter, dan Patient; barcode text tidak menggantikan relational identity.
- Normal processing menolak specimen tanpa patient/order identity valid kecuali audited exception workflow disetujui terpisah.
- `OnHold` mempertahankan operational state sebelumnya serta hold/resume reason, actor, dan time.
- Semua transition material menghasilkan immutable history dengan entity/source identifiers, from/to status, action, reason, actor, time, encounter/order/specimen, dan correlation ID.

Billing dan cancellation invariants:

- Untuk specimen-based examination, specimen/order `Accepted` adalah charge eligibility milestone; `Requested`, `Collected`, dan `Received` bukan examination-charge trigger.
- Lab mengirim clinical/consumption facts; Billing menjadi satu-satunya owner charge consequence.
- Cancellation sebelum `Accepted` tidak menghasilkan examination charge. Collection/material yang benar-benar dikonsumsi dinilai terpisah menurut Actual Consumption Rule.
- Cancellation setelah `Accepted/InProcess` tidak menghapus charge otomatis; Lab mencatat operational state/consumption dan Billing menentukan void, partial/full charge, atau adjustment.
- `Rejected` secara default tidak menghasilkan examination charge. Konsumsi terpisah hanya dapat dinilai bila memiliki dasar kebijakan.
- Recollection akibat internal hospital error memakai FOC/write-off/internal adjustment dan tidak dibebankan otomatis kepada pasien.
- Recollection karena patient/specimen condition atau external cause memerlukan reason, authorization, serta dasar clinical/payer policy sebelum charge baru.
- Lab tidak memiliki `Paid`, settlement, payer approval, void, refund, atau reversal authority.

Acceptance criteria tambahan:

1. Order, specimen, dan result tidak berbagi satu destructive status field.
2. Rejected/recollected specimen tetap terlihat dalam history dan tertaut ke replacement specimen.
3. Hanya authorized capability yang dapat melakukan collection, receipt, acceptance/rejection, validation, atau release.
4. Charge eligibility diterbitkan tepat pada valid `Accepted` transition dan tidak bergantung pada result release.
5. Direct doctor cancellation ditolak setelah `Requested`; cancellation request tetap dapat diproses Lab.
6. Internal-error recollection tidak menambah patient responsibility secara otomatis.

Capability disposition:

Existing `LabOrder` tetap dapat menjadi evidence/start point, tetapi capability diklasifikasikan sebagai major `Extend`: current model hanya memiliki `EncounterId`, `ProcedureId`, dan base cancellation tanpa lifecycle guard, specimen, result, charge eligibility, atau actual-consumption integration. Ini bukan izin implementasi.

## Closure Amendment 2026-08-19 - Radiology

### `RJ-BIL-GATE-DEC-004`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Radiology order, study/acquisition, report, safety, repeat/abort, dan charge eligibility lifecycle |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH STRUCTURAL AND SAFETY CLARIFICATIONS`, SHA-256 `3da3a2fdc2854cd6a77e6947fb8454fcb2347cc4899f438ff88e226a39701a40` |
| Formal governance status | `OPEN` - Radiology, Clinical Governance, dan Billing/Finance sign-off belum dilampirkan |

Lifecycle requirement yang dikunci:

1. Order: `Draft → Requested → Accepted → Scheduled → InProgress → Completed`; exceptions/supporting concepts `OnHold`, `CancelRequested`, `Cancelled`, `Rejected`, dan reschedule.
2. Study/acquisition: `Planned → PatientVerified → SafetyCleared → AcquisitionStarted → Acquired → QualityAccepted`; exceptions `OnHold`, `Aborted`, `QualityRejected`, `RepeatRequired`, `Cancelled`.
3. Report: `Pending → Drafted → Validated → Released`; released correction menggunakan versioned `AmendmentDrafted → AmendmentValidated → AmendmentReleased` dan tidak menimpa versi sebelumnya.
4. Order completion berbeda dari report release. Study dan report memiliki identity/version sendiri serta deterministic trace `Patient → Encounter → Order → Study → Report`.

Authority dan safety invariants:

- Dokter memiliki direct mutation sampai `Requested`; setelah handoff, cancellation/correction menjadi controlled request kepada Radiology.
- Radiology memiliki acceptance, scheduling, verification, safety, acquisition, technical quality, repeat/abort, study, reporting, dan amendment facts; tidak memiliki financial status authority.
- Normal `AcquisitionStarted` wajib didahului patient/encounter/order/procedure/modality verification dan seluruh mandatory safety gate yang relevan.
- Safety gate bersifat modality/procedure-specific, termasuk contrast, radiation, MRI device/implant, sedation, atau interventional prerequisites bila relevan; final checklist mengikuti SOP/clinical authority, bukan keputusan dokumen ini.
- Mandatory safety state `Pending/Failed` memblokir normal acquisition. Emergency override hanya boleh ada bila disahkan terpisah dan wajib reason, responsible clinician, authorization, time, serta audit.
- Report validity/content tidak dikendalikan Billing atau payment status. Result-distribution policy, bila ada, harus terpisah dari clinical validity.

Billing, abort, dan repeat invariants:

- `Requested`, `Accepted`, `Scheduled`, dan report release bukan initial charge trigger.
- Normal charge eligibility terjadi setelah actual acquisition menghasilkan usable study. Quality failure masuk exception workflow, bukan otomatis full charge.
- Cancellation sebelum `AcquisitionStarted` menghasilkan examination charge nol secara default; legitimate consumed preparation/material dinilai terpisah.
- `Aborted`/partial acquisition tidak otomatis full void atau full charge. Radiology merekam exposure, contrast, material/BHP, performed portion, usability, cause, actor, dan consumption facts; Billing menentukan outcome.
- Repeat mempertahankan original Study dan membuat Study baru dengan causal classification.
- Internal hospital error repeat tidak menambah patient charge secara otomatis. New clinical requirement memerlukan valid order/additional-order mechanism, reason, actor, linkage, dan authorization.
- Radiology menyerahkan performed, aborted, repeated, cancelled, dan actual-consumption facts; Billing tetap canonical owner charge/adjustment.

Acceptance criteria tambahan:

1. Order, Study, dan Report tidak menggunakan satu destructive status.
2. Acquisition ditolak ketika identity/procedure verification atau mandatory safety gate belum valid.
3. Normal charge eligibility hanya terjadi untuk performed acquisition dengan usable study.
4. Aborted/quality-rejected/repeat study mempertahankan original facts dan causal responsibility.
5. Released report hanya berubah melalui versioned amendment dengan reason, author/validator, time, dan previous-version reference.
6. Radiology tidak dapat mengubah paid, payer approval, waiver, settlement, void, refund, atau reversal.

Capability disposition:

Core Radiology workflow memerlukan bounded operational capability sendiri dan tidak boleh direduksi menjadi generic `PatientProcedure` lifecycle karena membutuhkan modality, scheduling, safety, acquisition, quality, repeat, report/amendment, dan optional RIS/PACS/DICOM identifiers. Shared procedure/tariff serta Encounter/Billing linkage tetap dapat direuse. Catatan historis bahwa actual-consumption calculation diblokir `RJ-BIL-GATE-DEC-005` telah disupersesi oleh decision revision 6.

## Closure Amendment 2026-08-19 - Actual Consumption

### `RJ-BIL-GATE-DEC-005`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Component-based actual-consumption calculation, partial service, correction, traceability, dan rounding |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH FINANCIAL CALCULATION CLARIFICATIONS`, SHA-256 `e16a812ae92ee2c7214663d365acf73c60065f6562188345da17f2ce85490dcd` |
| Formal governance status | `OPEN` - Billing/Finance dan service-owner sign-off belum dilampirkan |

Core calculation requirement yang dikunci:

1. Clinical Unit memiliki fakta aktual; Billing memiliki keputusan dan kalkulasi finansial.
2. Pelayanan dipecah menjadi measurable charge components, termasuk professional, facility/equipment, procedure, drug, reagent, contrast, BHP/material/device, administration yang sah, dan authorized other component.
3. Material terukur memakai `ActualQuantity × ApplicableUnitPriceSnapshot`. Clinical facts tidak mengirim nominal Rupiah final.
4. Billing menyimpan immutable tariff/version/effective-date/unit-price/currency/pricing-context/calculation-date snapshot; historical charge tidak dihitung ulang dari mutable master.
5. Patient billable price berbeda dari inventory/acquisition cost atau COGS.
6. Service belum dimulai menghasilkan zero service charge. Completed service memakai applicable full charge.
7. Partial service hanya dihitung melalui active approved service/catalog rule; generic percentage, full-charge, full-void, time, atau milestone fallback dilarang.
8. Partial non-material tanpa active approved rule wajib menjadi `PendingFinancialReview` pada affected component/line, bukan otomatis seluruh folio.
9. Folio tidak dapat `FullySettled/Closed` selama mandatory component masih unresolved kecuali authorized exception policy berlaku.

Rule governance dan causality:

- Reusable partial rule memiliki service/catalog, component, version, effective period, formula type/parameters, rounding, optional minimum/maximum yang sah, approval evidence, dan active status; penerapan retroaktif diam-diam dilarang.
- Formula yang dapat didukung hanya bila disahkan: actual quantity, milestone/step, time-based, fixed partial, atau explicit percentage.
- Consumption cause dipisahkan dari payer/financial disposition. Cause minimal: `InternalHospitalError`, `PatientOrClinicalCondition`, `NewClinicalRequirement`, `ExternalOrOther`.
- Financial disposition seperti covered, partial, payer rejection, patient responsibility, waived, atau internal write-off berada pada axis terpisah.
- `InternalHospitalError` tetap merekam actual quantity, tetapi additional cost akibat error tidak menjadi patient responsibility; Billing memakai FOC/write-off/internal adjustment sesuai governance.
- Actual consumption tidak otomatis berarti patient liability.

Correction, review, dan audit invariants:

- Clinical fact correction membuat version/superseding fact; financial correction membuat versioned adjustment. Original fact dan charge tidak dioverwrite.
- Setiap calculation dapat direkonstruksi dari source fact/version, encounter/service/component, quantity/unit, tariff snapshot, rule/version, raw amount, rounding/version/difference, final amount, cause, actor/time, dan correlation ID.
- Rounding dilakukan per charge component menggunakan centrally configured/versioned Billing rule sebelum aggregation; rounding difference disimpan.
- Manual review tidak mengizinkan arbitrary amount: wajib source facts, reason, prior state, decision/final amount, policy reference, reviewer, time, comment, dan approver bila threshold mensyaratkan.
- Keputusan manual satu kasus tidak otomatis menjadi reusable formula.
- Stable source fact/version/component reference wajib mencegah duplicate charge pada event replay.

Acceptance criteria tambahan:

1. Partial non-material tanpa rule tetap pending dan tidak mengubah komponen final lain.
2. Historical calculation tetap reproducible setelah master tariff/rule berubah.
3. Internal-error consumption terlihat sebagai clinical/costing truth tanpa menambah patient responsibility otomatis.
4. Correction menghasilkan delta/adjustment yang tertaut ke original, bukan destructive update.
5. Event/fact replay tidak menghasilkan charge component kedua untuk source/version/type yang sama.
6. Folio closure ditolak ketika mandatory `PendingFinancialReview` belum diselesaikan tanpa authorized exception.

Configuration boundary:

Actual Consumption core model siap untuk domain design. Formula per service/catalog tetap membutuhkan configuration/governance evidence; ketiadaan formula ditangani secara eksplisit melalui `PendingFinancialReview`, bukan blocker untuk core model dan bukan izin menebak nilai.

## Closure Amendment 2026-08-19 - Financial Governance

### `RJ-BIL-GATE-DEC-006`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Financial capabilities, maker-checker, approval lifecycle, thresholds, fail-closed, close/reopen, dan correction governance |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH FINANCIAL GOVERNANCE CLARIFICATIONS`, SHA-256 `f9125f5eba1dd0350309933a6ff0bf0a286c93135dabeb316a8fff7e293cc685` |
| Formal governance status | `OPEN` - Finance, Security/Privacy, dan delegated executive sign-off belum dilampirkan |

Capability dan ownership requirement:

1. Financial capabilities dipisahkan untuk charge create/finalize, adjustment, void create/approve, reversal create/approve, refund create/approve/execute, waiver create/approve, write-off create/approve, financial-review resolve/approve, manual override, serta folio close/reopen.
2. Billing posting/finalization berbeda dari `Finance.Accounting.Post` atau GL posting.
3. Valid deterministic system charge boleh efektif tanpa approval per transaksi bila source fact, approved/effective rule, tariff snapshot, system actor, time, audit, correlation, dan idempotency valid.
4. Draft, expired, future, unapproved, atau unversioned rule tidak boleh menghasilkan final charge.
5. Organizational title tidak di-hardcode; role/organization mapping memberi capability melalui authorization administration.

Maker-checker dan high-risk invariants:

- Maker dan checker wajib berbeda authenticated/effective `UserId`. Delegation tidak boleh membuat orang efektif yang sama menjadi keduanya.
- Memiliki create dan approve capability sekaligus tidak memberi self-approval.
- Void/reversal terhadap `Paid`, `Posted`, `Claimed`, atau `Settled`; refund settled payment; closed-folio reopen; dan cross-encounter correction selalu high-risk tanpa memandang nominal.
- Waiver, write-off, manual review/override, discretionary adjustment, dan action lain mengikuti versioned/effective threshold policy yang dapat mempertimbangkan amount, percentage, risk, transaction/payer/service type, serta hospital/unit.
- Checker hanya approve, reject, atau return for revision. Material change menghasilkan maker revision baru; request lama immutable.
- Approval terjadi sebelum financial mutation efektif. Pending approval tidak mengubah canonical financial state.

Approval policy dan fail-closed invariants:

- Approval record menelusuri request/action, maker/checker, requested/approved amount, impact, prior/requested/final status, reason/decision, policy/version, evidence, encounter/folio/charge references, correlation, dan timestamps.
- Lifecycle minimum: `Draft → Submitted → PendingApproval → Approved`, dengan `Rejected`, `ReturnedForRevision`, `Cancelled`, dan optional `Expired`; expired tidak berarti approved.
- Tidak adanya checker, valid policy/threshold, atau determinable authority mempertahankan `PendingApproval` atau `BlockedByPolicyConfiguration`; SLA hanya memicu escalation dan tidak pernah authorization bypass.
- Fail-closed hanya berlaku pada high-risk/approval-required operation dan tidak memblokir normal deterministic charge yang sah.
- Finance memiliki threshold/authority policy yang versioned, effective-dated, approved, auditable, dan non-destructive. Historical request tetap mereferensikan policy version asal.

Correction dan execution invariants:

- Void, reversal, dan refund berbeda secara semantik, lifecycle, approval, audit, dan accounting consequence.
- Waiver/write-off/manual override tidak menghapus original charge; financial effect diterapkan melalui approved adjustment/action.
- Folio close memerlukan seluruh mandatory financial prerequisites. Reopen selalu controlled high-risk request dan mempertahankan closing history.
- Cross-encounter correction dilarang mengubah `EncounterId` pada original charge; gunakan approved reversal/reallocation yang menelusuri source/target encounter, folio, charge, reason, maker/checker, approval, dan correlation.
- Approved execution wajib idempotent serta me-revalidate current target state. State conflict menghasilkan controlled revalidation state, bukan blind execution.

Acceptance criteria tambahan:

1. Self-approval gagal walaupun user memiliki create dan approve capabilities.
2. High-risk action bernilai kecil tetap memerlukan maker-checker.
3. Checker material edit tidak dapat disetujui sebagai request lama.
4. Invalid/missing approval policy tidak memakai default approver/threshold dan tidak memblokir normal deterministic charge.
5. Replayed approved request tidak menghasilkan duplicate refund/reversal/waiver/write-off/adjustment.
6. Cross-encounter correction mempertahankan original encounter and charge history.

Configuration boundary:

Core financial capability, approval, maker-checker, threshold-policy, close/reopen, dan cross-encounter correction models siap untuk domain design. Nilai threshold dan approval matrix final tetap membutuhkan Finance configuration/approval; high-risk operation tanpa valid policy harus fail-closed.

## Closure Amendment 2026-08-19 - Pharmacy and Financial State

### `RJ-BIL-GATE-DEC-007`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Pharmacy clinical ownership, read-only financial projection, dispensing prerequisite, outage, dan compatibility migration |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH OWNERSHIP AND INTEGRATION CLARIFICATIONS`, SHA-256 `d86aba5b592790a1ba592a6136b8aa2fec7bd190c8d8b7d818fac9a46cf8885b` |
| Formal governance status | **`CLOSED`** 21 September 2026 — user menyatakan eksplisit mewakili ketiga pihak sekaligus (Product/Domain Owner, Billing/Payer owner, dan Clinical Governance) pada closure amendment `pharmacy/00-interview-decisions.md` (`PHA-DEC-063`–`065`); lihat bagian itu untuk detail adopsi mekanisme dan evidence pendukung |

Ownership requirement:

1. Pharmacy memiliki prescription clinical lifecycle, review, verification/clarification, preparation/compounding, fulfillment, actual prepared/dispensed/returned/wasted quantities, substitution, cancellation, dan responsible actor/timestamps.
2. Billing/Payer/Cashier/Finance memiliki seluruh canonical financial state sesuai `RJ-BIL-GATE-DEC-001`.
3. Pharmacy dapat menyimpan read-only financial projection dengan financial reference/status/version, source domain/event, source update time, dan sync time; projection bukan financial source of truth.
4. Pharmacy endpoint dilarang melakukan canonical financial mutation.
5. Existing `MarkBillingGenerated`, `MarkPaid`, `MarkInsuranceApproved`, dan `MarkPaymentWaived` disupersesi. Temporary legacy endpoint hanya boleh menjadi restricted compatibility adapter yang memanggil canonical Billing/Payer contract dan tidak memutasi independent Pharmacy financial truth.

Workflow dan cancellation invariants:

- Financial prerequisite untuk dispensing bersifat approved-policy-driven berdasarkan payer/service/patient/medication/emergency/guarantee context dan tidak universal `Paid` hardcode.
- `ReadyForPharmacy` dipisah menjadi clinical readiness, applicable financial prerequisite, dan Pharmacy operational readiness.
- Valid clinical urgency exception dapat melanjutkan service ketika financial prerequisite unresolved, tetapi wajib approved policy, reason, authorizing actor, time, encounter/prescription, financial state, dan audit.
- Clinical urgency override bukan `PaymentWaived` dan tidak menghapus financial obligation.
- Prescription cancellation/partial dispensing mengirim clinical/operational and actual-consumption facts; Billing menentukan no charge, void, adjustment, full charge, refund/reversal, atau write-off.
- Pharmacy tidak menghitung nominal canonical dari current price.

Outage, projection, dan reconciliation invariants:

- Billing/Payer unavailable menghasilkan explicit `Unknown`, `PendingVerification`, atau `Stale`; unknown bukan paid maupun unpaid.
- Tanpa authorized clinical exception, Pharmacy mengikuti approved downtime/hold policy dan tidak membuat financial assumption.
- Downtime clinical exception mencatat exception type/reason/authorizer/time, last-known financial state, source availability, prescription/encounter, serta wajib direkonsiliasi setelah recovery.
- Projection sync idempotent, version-aware, auditable, reconcilable, dan menolak stale/out-of-order version menimpa versi baru.
- Billing/Payer canonical truth memenangkan financial projection conflict; Pharmacy canonical clinical/dispensing facts tidak boleh ditimpa oleh financial reconciliation.

Acceptance criteria tambahan:

1. Pharmacy tidak dapat membuat prescription terlihat paid/approved/waived tanpa canonical financial transaction.
2. Legacy financial endpoint merutekan ke canonical owner dan tidak menulis financial truth langsung pada Pharmacy.
3. `FinancialStatusUnknown` tidak diperlakukan sebagai paid atau unpaid.
4. Incoming older financial version tidak menimpa Pharmacy projection yang lebih baru.
5. Paid status tidak membuat `Dispensed=true`, dan dispensed status tidak membuat `Paid=true`.
6. Authorized urgent dispensing tetap mempertahankan outstanding financial workflow.

Migration disposition:

Pharmacy clinical/fulfillment capability adalah reuse/extend. Existing financial mutation adalah ownership redesign. Existing payment fields boleh dipertahankan sementara hanya sebagai compatibility projection/cache; tidak boleh ada dua canonical owners. Physical persistence dan deprecation sequencing diputuskan downstream, bukan pada closure pass.

## Closure Amendment 2026-08-19 - Reliability and Reconciliation

### `RJ-BIL-GATE-DEC-008`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Fact identity, idempotency, retry/failure outcomes, partial success, reconciliation, downtime recovery, dan folio closure gates |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH RELIABILITY AND RECONCILIATION CLARIFICATIONS`, SHA-256 `7ffa205e209d7818bb29563fa8d327b34dac1bf6d5c86f230b2ab40457854100` |
| Formal governance status | `OPEN` - Billing/Finance dan integration owner sign-off belum dilampirkan |

Fact identity dan processing invariants:

1. Setiap clinical fact memiliki stable source fact ID/version/domain/entity, encounter, occurred time, correlation, dan idempotency key. Retry tidak membuat identity baru.
2. Durable, restart-safe, concurrency-safe idempotency record mengikat source/version/operation ke satu processing outcome/result reference.
3. Duplicate/replay/concurrent duplicate mengembalikan existing result dan tidak membuat duplicate charge, component, adjustment, void, reversal, refund, atau financial event.
4. Validation rejection final untuk fact version tersebut; incorrect source fact diperbaiki melalui newer superseding version. Valid source fact yang gagal diproses tetap di-retry dengan version yang sama.
5. Processing outcome dipisahkan dari financial status dan mendukung `Received`, `Processing`, `Applied`, `PartiallyApplied`, `RejectedValidation`, `TransientFailure`, `PermanentFailure`, `OutcomeUnknown`, `PendingReconciliation`, dan `Reconciled` atau vocabulary setara.

Timeout, retry, dan exception invariants:

- Timeout/response loss menghasilkan `OutcomeUnknown`, bukan assumed failure/success. Recovery memakai original identity melalui status query; retry hanya dilakukan dengan key yang sama setelah outcome diverifikasi.
- Transient failure dapat retry dengan bounded/backoff policy. Permanent failure berhenti dari automatic retry dan masuk controlled exception/dead-letter review.
- Dead letter bukan resolution; case tetap mempunyai failure reason, owner, attempts, next action, resolution status, dan correlation.
- Missing Billing configuration dapat diperbaiki lalu fact valid/version yang sama diproses ulang; source-data correction membutuhkan fact version baru.

Partial, late, dan correction invariants:

- Partial success dicatat pada charge-component level; independent component yang applied tidak dihapus karena component lain gagal.
- Failed component tidak dianggap zero dan harus explicit pending/failure/reconciliation state.
- Atomic business dependency harus dinyatakan eksplisit; tidak semua component dipaksa satu business rollback.
- Older/stale/out-of-order version tidak menimpa applied newer version. Valid newer correction menghasilkan versioned adjustment dan mempertahankan original charge.

Reconciliation dan closure invariants:

- Reconciliation membandingkan canonical clinical facts dengan canonical Billing state untuk missing/orphan facts, amount/quantity/version mismatch, duplicate charge, missing adjustment, stale projection, unknown outcome, partial failure, dan unresolved emergency exception.
- Reconciliation tidak memutasi clinical truth agar cocok dengan financial state. Correction tetap ditentukan pada owning domain dan versioned.
- Non-deterministic mismatch membuat reconciliation case dengan type, source/version, billing/encounter reference, impact, status, owner, priority/risk, age/SLA, actions, resolution, dan audit.
- Deterministic no-impact duplicate dapat auto-resolve. Manual financial outcome tetap tunduk pada `RJ-BIL-GATE-DEC-006`.
- Folio close diblokir oleh mandatory `OutcomeUnknown`, pending reconciliation/approval/review, financially material permanent failure, failed mandatory component, unresolved allocation, atau exception. Authorized closure exception harus explicit, auditable, dan governed.
- SLA breach hanya memicu warning/escalation/priority/visibility dan tidak pernah auto-approve, write-off, charge, void, atau resolve.

Downtime recovery invariants:

- Clinical modules mempertahankan durable pending-delivery/backlog facts dengan stable identity/version/key.
- Recovery memproses backlog secara version/dependency-aware dan idempotent, tanpa mengandalkan arrival order atau distributed transaction.
- Recovery menghasilkan operational report berisi counts per outcome, affected encounter/folio, unresolved impact, owners, dan next actions.
- Billing menyediakan canonical processing-status lookup berdasarkan stable source identity dengan outcome/result/applied version/financial references/failure/reconciliation case.

Acceptance criteria tambahan:

1. Response loss setelah committed charge diselesaikan melalui status query dan tidak membuat charge kedua.
2. Concurrent duplicate fact menghasilkan tepat satu canonical processing result.
3. Invalid payload correction memakai version baru; infrastructure retry memakai version yang sama.
4. Partial applied components tetap final sementara failed component visible dan unresolved.
5. Folio close ditolak ketika mandatory reconciliation exception belum selesai tanpa authorized closure exception.
6. Recovery report mengidentifikasi seluruh unresolved encounter/folio dan assigned next action.

Readiness disposition:

Clinical fact identity, Billing idempotency, `OutcomeUnknown`, retry/exception, partial component processing, reconciliation, downtime backlog, dan folio reconciliation gates siap untuk domain design. Exact persistence and API mechanisms tetap downstream decisions.

## Closure Amendment 2026-08-19 - Payer and Claim Integration

### `RJ-BIL-GATE-DEC-009`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Internal payer orchestration, authorization/claim lifecycles, external adapters, manual release scope, dan production gates |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH PAYER INTEGRATION AND RELEASE-SCOPE CLARIFICATIONS`, SHA-256 `d9fc83e18bdc5a209fa152b83eae733b090f62ddd8284836dc565efa49450e73` |
| Formal governance status | `OPEN` - Payer/Insurance, Billing/Finance, Security, dan integration owner sign-off belum dilampirkan |

Ownership dan lifecycle requirement:

1. Internal Payer/Insurance Management menjadi canonical orchestrator eligibility, coverage verification, authorization, guarantee, coordination of benefits, claim preparation/submission/status, adjudication, payer allocation outcome, reconciliation, dan external integration state.
2. Clinical, Billing, Payer, Cashier, dan Finance ownership tetap terpisah; external response tidak langsung memutasi clinical facts, prescription, diagnostic orders/studies, procedure, encounter, atau charge database.
3. Eligibility berbeda dari service authorization. Authorization lifecycle: `Draft → Submitted → Pending → Approved`, dengan `PartiallyApproved`, `Rejected`, `Expired`, `Cancelled`, dan optional `NeedMoreInformation`; semua versioned.
4. Claim processing dipisahkan dari settlement. Processing: `Draft → Ready → Submitted → Acknowledged → InAdjudication`, dengan need-info/approved/partial/rejected/cancelled. Settlement: `NotDue → PaymentPending → PartiallyPaid → Paid → Reconciled`, dengan dispute/mismatch/reconciliation exceptions.
5. Claim approval tidak sama dengan payment. Payer rejection tidak menghapus charge; partial/rejected residual mengikuti coordination-of-benefits dan patient responsibility.

Adapter dan reliability invariants:

- Setiap named external system memakai dedicated adapter ke normalized internal contract; core domain tidak berisi payer-specific branching.
- Adapter capability profile menyatakan dukungan idempotency, status query, cancellation, amendment, partial approval, realtime eligibility/authorization, dan claim submission.
- Outbound request memiliki stable internal/version/external-system identity, idempotency key, correlation, encounter/folio/authorization/claim references, dan time.
- Timeout menjadi `OutcomeUnknown`; same-identity query/reconciliation/retry rules mengikuti `RJ-BIL-GATE-DEC-008`. Payer outage tidak otomatis approved/rejected.
- External decision dinormalisasi dan versioned sebelum menghasilkan allocation adjustment; external business rejection berbeda dari integration failure.
- Audit menyimpan metadata/reference/hash/code/amount/time/actor/correlation, bukan raw sensitive payload. Raw evidence, bila wajib, menggunakan secured storage dengan access, encryption, masking, retention, dan access audit.

Manual workflow dan governance:

- Manual payer decision sah bila transparan sebagai `ManualOperator`, bukan synthetic external API success.
- Manual record memerlukan payer/reference, decision/amount/reason, evidence, decision/record times, actor, dan audit.
- Discretionary/high-risk manual financial impact tunduk pada `RJ-BIL-GATE-DEC-006`.
- Payer reconciliation memakai `RJ-BIL-GATE-DEC-008`; financial consequence memakai `RJ-BIL-GATE-DEC-006`.

Release scope yang dikunci:

1. Release 1 production core mencakup Internal Payer/Insurance Management, cash/patient, manual commercial insurance, manual corporate guarantee, optional BPJS/JKN payer master/context, multi-payer allocation, patient responsibility, authorization/guarantee recording, manual claim/adjudication/settlement/reconciliation, evidence/audit, maker-checker, dan adapter interfaces.
2. AdMedika adalah priority external adapter candidate, tetapi production activation tetap gated sampai exact product/system, owner, contract/version, credentials/security, sandbox/UAT, idempotency/status-query, dan reconciliation evidence tersedia.
3. BPJS/JKN adalah conditional external candidate. Activation membutuhkan verified hospital participation scope, owner, credentials, official interface/version, sandbox/UAT, and reconciliation evidence.
4. BPJS operational/administrative flow tidak disatukan sembarangan dengan E-Klaim/INA-CBG/claim interoperability family.
5. Unnamed insurer/corporate/TPA APIs tidak termasuk direct Release 1 integration dan memakai internal manual workflow.
6. Promotion mengikuti named candidate → verified contract → sandbox → UAT → reconciliation proven → production approval → adapter activation tanpa mengubah canonical payer domain.

Production integration gate:

External adapter fail-closed sampai named system, business/technical owners, API contract/version/environment, credential/security/certificate authority, sandbox/UAT evidence, idempotency/duplicate/status-query/timeout/retry/error mapping, reconciliation owner, support escalation, dan cutover approval tersedia.

Acceptance criteria tambahan:

1. Approved claim tanpa received payment tetap `PaymentPending` dan bukan `Paid`.
2. External rejection mempertahankan charge dan menghasilkan versioned residual allocation.
3. Manual outcome tidak dilabeli sebagai external integration success.
4. Timeout mempertahankan unknown state dan original request identity.
5. Adapter tanpa critical production evidence tetap disabled, sementara manual core workflow tetap beroperasi.
6. External mismatch membuat reconciliation case dan tidak melakukan silent overwrite.

## Closure Pass Result 2026-08-19

Seluruh owner-dependent requirement blockers `RJ-BIL-GATE-DEC-001` sampai `RJ-BIL-GATE-DEC-009` telah dijawab dan dicatat sebagai `LOCKED-DRAFT`. Status ini mengunci requirement intent untuk assessment berikutnya, tetapi belum merupakan formal joint clinical-financial-security approval karena nama, jabatan, delegasi, dan sign-off evidence belum dilampirkan.

## Approval Amendment 2026-08-20 - Release 1 Internal/Manual

Pengguna memberikan instruksi eksplisit: `DRAFT_FOR_OWNER_APPROVAL -> OWNER_APPROVED -> IMPLEMENTATION_AUTHORIZED -> DELIVERY_PLANNING_ALLOWED`.

Disposition:

1. `RJ-BIL-GATE-DEC-001` sampai `RJ-BIL-GATE-DEC-009` dipromosikan dari historical `LOCKED-DRAFT` menjadi `OWNER_APPROVED` untuk kontrak Release 1 internal/manual `RJ-BIL-CONTRACT-001` version `1.0.0`.
2. Blueprint Release 1 internal/manual menjadi `IMPLEMENTATION_AUTHORIZED` dan boleh diproses oleh `plan-module-delivery`.
3. Authorization ini tidak memasukkan named external adapters, tidak menetapkan nilai formula/threshold/SOP yang belum tersedia, dan tidak menyatakan production activation.
4. Source implementation tetap memerlukan satu approved roadmap Task ID, repository write authority, dan execution-time preflight dari builder terkait.
5. Historical labels `LOCKED-DRAFT` di bagian sebelumnya dipertahankan sebagai rekam status pada saat masing-masing jawaban diterima; amendment ini adalah status efektif terbaru.

Next required step adalah menjalankan ulang `requirement-completeness-gate` terhadap decision revision 10. Domain architecture tidak boleh menganggap assessment revision 1 otomatis current setelah perubahan material ini.

---

# Owner Approval Amendment — Doctor / Rawat Jalan Clinical, 31 Agustus 2026

Bagian ini memuat keputusan otoritatif untuk scope **Doctor / Rawat Jalan Clinical** (`RJ-DOC`),
yang terpisah dari scope Billing (`RJ-BIL`) di seluruh bagian sebelumnya. Keputusan diberikan
Sukma Giri selaku pemilik/author blueprint `RJ-BIL-BP-001`.

| ID | Jenis | Keputusan | Owner | Status | Catatan |
|---|---|---|---|---|---|
| `RJ-DOC-DEC-001` | Approval | **Roadmap Doctor / Rawat Jalan Clinical `roadmap/doctor-consultation-roadmap.md` revision `2` dinaikkan dari `READY_FOR_OWNER_APPROVAL` menjadi `OWNER_APPROVED`.** Approval mencakup: scope, ownership boundary, klasifikasi `MANDATORY`/`CONDITIONAL`/`ARCHITECTURAL INVARIANT`/`DOWNSTREAM`, arah canonical completion, contract planning, task definitions, dependency sequence, dan Doctor Definition of Done. Approval ini **bukan** izin mengubah application source, menjalankan builder, membuat/menjalankan migration, memutasi database, commit, push, merge, deploy, maupun mengerjakan Billing. `IMPLEMENTATION_AUTHORITY` tetap `NOT_GRANTED` dan diberikan terpisah per task setelah contract freeze | Sukma Giri | `approved` | Sukma Giri, 31 Agustus 2026 |
| `RJ-DOC-DEC-002` | Decision | **`RJ-DOC-OQ-003` — Lab dan Radiology pada first release: `CONDITIONAL — NOT PART OF CURRENT MANDATORY DOCTOR BASELINE`.** Untuk baseline saat ini: Prescription wajib mengikuti lifecycle existing; Procedure/Tindakan wajib mengikuti lifecycle existing; FE ordering Lab **bukan** blocker Doctor DoD; FE ordering Radiology **bukan** blocker Doctor DoD. Lab dan Radiology tetap dicatat sebagai clinical capability yang valid dan dapat dinaikkan menjadi mandatory pada rilis berikutnya melalui approval terpisah. Capability maupun gap implementasinya **tidak boleh dihapus** dari blueprint | Sukma Giri | `approved` | Sukma Giri, 31 Agustus 2026. Menutup `RJ-DOC-OQ-003` |
| `RJ-DOC-DEC-003` | Decision | **`RJ-DOC-OQ-004` — Correction/reopen setelah `COMPLETED`: `NO ARBITRARY REOPEN IN CURRENT BASELINE`.** Konsultasi ber-`ConsultationStatus = Completed` wajib protected/locked dari normal editing. **Dilarang menciptakan workflow reopen generik pada task saat ini.** Bila correction/reopen dibutuhkan di masa depan, ia menjadi capability tersendiri yang wajib memuat reason, actor, authorization, audit trail, version/correction semantics, dan approval owner eksplisit. Doctor DoD saat ini hanya membutuhkan completed-state protection | Sukma Giri | `approved` | Sukma Giri, 31 Agustus 2026. Menutup `RJ-DOC-OQ-004`; mengikat `RJ-DOC-BE-004` |
| `RJ-DOC-DEC-004` | Decision | **`RJ-DOC-OQ-005` — Open ancillary order saat Selesai Konsultasi: `ALLOWED WITH VALID DOCTOR-SIDE ORDER STATE`.** Dokter **boleh** menyelesaikan konsultasi walaupun downstream ancillary lifecycle masih berjalan: Lab order sudah dibuat tetapi specimen belum collected/received/resulted, dan Radiology order sudah dibuat tetapi study belum performed/resulted, **tidak** menahan penyelesaian. Lifecycle eksekusi/hasil ancillary adalah tanggung jawab unit Lab/Radiology dan berlanjut setelah konsultasi selesai. Sebaliknya penyelesaian **wajib ditolak** apabila clinical order yang sedang dibuat dokter berada dalam keadaan: request gagal, data invalid, save belum berhasil, pending client-side mutation belum flush, state tidak dapat dipastikan, atau pembuatan order belum menghasilkan authoritative server state. Ringkasnya: `DOCTOR ORDER CREATION MUST BE STABLE`, tetapi `ANCILLARY EXECUTION DOES NOT NEED TO BE FINISHED`. Aturan ini **wajib masuk validation contract** | Sukma Giri | `approved` | Sukma Giri, 31 Agustus 2026. Menutup `RJ-DOC-OQ-005`; dibekukan pada `RJ-DOC-COMPLETION-001@1.0.0` bagian `1.6`; mengikat `RJ-DOC-BE-002` |
| `RJ-DOC-DEC-005` | Decision | **`RJ-DOC-OQ-006` — `CompleteImmediately`: `DEPRECATE / RESTRICT AS ALTERNATE FINALIZATION PATH`.** `POST /doctor-consultations` dengan `CompleteImmediately=true` **tidak boleh** menjadi jalur finalisasi alternatif untuk workflow Rawat Jalan normal, karena saat ini menghasilkan `Completed` tanpa authoritative validation dan tanpa producer handoff. Canonical normal completion tetap `PATCH /doctor-consultations/{id}/complete`. **API tidak dihapus pada task ini.** Kontrak menyatakan: jalur tersebut bukan canonical; consumer baru dilarang memakainya untuk Rawat Jalan normal; treatment implementasi adalah restrict/deprecate/compatibility remediation; detail implementasi dikerjakan pada `RJ-DOC-BE-001` atau task terkait **setelah** contract freeze. Bila source membuktikan endpoint dipakai flow lain seperti IGD atau internal workflow, alur tersebut **tidak boleh dirusak** dan compatibility requirement-nya dicatat eksplisit | Sukma Giri | `approved` | Sukma Giri, 31 Agustus 2026. Menutup `RJ-DOC-OQ-006`; dibekukan pada `RJ-DOC-COMPLETION-001@1.0.0` bagian `1.11` |
| `RJ-DOC-DEC-006` | Decision | **`RJ-DOC-INT-001` dan `RJ-DOC-INT-002` dibekukan.** `RJ-DOC-COMPLETION-001@1.0.0` dan `RJ-DOC-HANDOFF-001@1.0.0` berstatus `FROZEN` pada `contracts/doctor-consultation-contracts.md`. Perubahan kontrak memerlukan kenaikan versi dan approval owner tersendiri. Pembekuan ini membuat `RJ-DOC-BE-001`, `BE-002`, `BE-003`, dan `BE-005` menjadi `ELIGIBLE`, **tetapi tidak memberikan implementation authority kepada satu pun di antaranya** | Sukma Giri | `approved` | Sukma Giri, 31 Agustus 2026 |

## Compatibility requirement `RJ-DOC-DEC-005`

Diverifikasi terhadap backend `801a4f5` dan frontend `baca965`. Tiga hal **tidak boleh** rusak
ketika `CompleteImmediately` dibatasi:

| Yang dijaga | Bukti |
|---|---|
| `POST /doctor-consultations` sebagai endpoint create — aktif dipakai frontend | `use-doctor-soap.js:327`, `use-doctor-prescription.js:239` |
| Jalur IGD tanpa antrean — `QueueId` nullable, encounter di-resolve langsung | `DoctorConsultationController.cs:257-268`; komentar source `BE-IGD-028`, `FR-IGD-062` |
| `CompleteImmediately` milik `PatientAssessmentController` — field bernama sama pada DTO **berbeda**, dipakai alur screening perawat; **di luar cakupan keputusan ini** | `PatientAssessmentDtos.cs:294`; `PatientAssessmentController.cs:296`, `:370-371`, `:678` |

Consumer konsultasi yang mengirim `completeImmediately: true` pada frontend `baca965`: **tidak
ada**. Seluruh pembuatan konsultasi memakai `false` (`use-doctor-prescription.js:210`). Kemunculan
`completeImmediately` pada `use-doctor-queue.js:786` adalah milik payload **patient assessment**,
bukan konsultasi. Karena itu pembatasan ini tidak memutus satu pun consumer yang diketahui.

## Batas approval

Approval `RJ-DOC-DEC-001` **tidak** mencakup: perubahan application source; eksekusi builder;
pembuatan atau penerapan migration; mutasi database; commit; push; merge; deployment; pekerjaan
Billing; maupun aktivasi `RJ-BIL-DEP-009`. `IMPLEMENTATION_AUTHORITY` scope Dokter tetap
`NOT_GRANTED` sampai diberikan eksplisit per task.

## Amendment Pass 2026-09-28 — PRD Rawat Jalan sampai Billing V2

| Field | Nilai |
|---|---|
| Mode | `Amendment pass` — blueprint sudah pernah disetujui; keputusan lama **tidak** ditimpa |
| Masukan | `PRD-RJ-BIL-V2-001` ([PRD-Rawat-Jalan.md](PRD-Rawat-Jalan.md), *Draft for Owner Review*, belum di-commit) |
| Capability map | [01-existing-capability-map-prd-v2.md](01-existing-capability-map-prd-v2.md) — `CURRENT` pada SHA di bawah |
| Backend SHA | `063d38bc306bb6b46bdf088513fa6cdcc80399d8` (`sukmagp`) |
| Frontend SHA | `83b8b72744d4afaaedb3d2af9dd0b83fb272f9d6` (`sukmagpV2`) |
| Kontrak Billing yang berlaku | `BIL-INTEGRATION-0.4` / `BIL-INTEGRATION-1.2` |
| Prefix keputusan | `RJ-E2E-DEC-*` (mengikuti PRD §29) |
| Pengambil keputusan | Sukma Giri, pemilik blueprint `RJ-BIL-BP-001` |

### Batas scope amendment

**Satu kalimat:** menyambungkan fakta pelayanan Rawat Jalan ke invoice canonical Billing
(`BilInvoice`), ditambah Ringkasan Billing read-only di workspace dokter.

| Di dalam scope | Di luar scope — pemiliknya |
|---|---|
| Korelasi `EncounterId`; finalisasi konsultasi; sumber tagihan konsultasi, tindakan, Lab, Radiologi, obat | Kalkulasi harga, diskon, coverage, payer, pembayaran, settlement, refund — **Billing/Kasir** (hanya titik sentuh) |
| Konvergensi Folio → Invoice; idempotency, retry, rekonsiliasi | Aturan internal telaah/dispensing — **Pharmacy** (hanya titik tagih obat) |
| Pembatalan/koreksi pelayanan; batas status Encounter | Transisi Encounter `Billing → Completed` — **Registration** (`RJ-E2E-DEC-004`, sengaja ditunda) |
| Ringkasan Billing read-only; permission dan audit | Pemesanan Lab/Radiologi dari workspace dokter — tetap `CONDITIONAL` sesuai `RJ-DOC-DEC-002` (CAP-V2-19) |

### Keputusan

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-E2E-DEC-000` | Decision | **Bentuk blueprint `SINGLE`** — PRD V2 masuk sebagai amendment pada umbrella `RJ-BIL-BP-001`. `shape_decided_by = USER_CONFIRMED`. Hasil uji: jalur fakta → invoice tidak punya bounded context, kosakata status, atau master data sendiri; ia titik sambung Clinical ↔ Billing. Tidak ada kemampuan tanpa rumpun | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 | Jawaban sesi |
| `RJ-E2E-DEC-003` | Decision | **`BilFolio` menjadi ledger bukti/rekonsiliasi; `BilInvoice`/`BilInvoiceItem` adalah satu-satunya tagihan canonical.** Fakta klinis tetap dicatat di folio, lalu adapter Billing meneruskannya ke `UpsertChargeAsync`. Setiap baris folio wajib punya penanda sudah/belum masuk invoice. Tidak boleh ada pelayanan yang punya `BilChargeLine` tetapi tidak pernah masuk invoice. Pemindahan ringkasan tagihan rawat inap (`PatientBillingSummaryService`) dari folio ke invoice **bukan** scope amendment ini | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 | CAP-V2-13 |
| `RJ-E2E-DEC-005` | Decision | **Obat ditagih dua tahap.** Tahap 1: saat dokter menekan Selesai Konsultasi, item `PHARMACY` masuk invoice dengan status sementara agar kasir dapat menagih dan gerbang *lunas sebelum serah* tetap berlaku. Tahap 2: saat `DISPENSED`, versi baru menyesuaikan qty aktual. **Mengoreksi PRD FR-007 dan `AC-RJ-007`** (bukan hanya `DISPENSED`) dan **menyempurnakan, bukan menggantikan**, `RJ-BIL-DEC-002` serta Actual Consumption Rule `RJ-BIL-DEC-005`. Kontrak adapter `PHARMACY` wajib diperluas dengan status tahap 1. Contoh: resep Paracetamol 10 tablet → invoice 10 tablet → pasien bayar → farmasi hanya punya 8 → versi 2 `DISPENSED` 8 tablet; kelebihan bayar diselesaikan Billing | Sukma Giri (Farmasi + Billing/Finance untuk sign-off formal) | `approved` | Sukma Giri, 28 Sep 2026. `FORMAL_PHARMACY_SIGNOFF` dan `FORMAL_FINANCE_SIGNOFF` tetap `OPEN` | CAP-V2-07 |
| `RJ-E2E-DEC-006` | Decision | **Harga item dari jalur fakta klinis ditetapkan adapter Billing dari `MstTariff`**, termasuk `DoctorShare`. Fakta klinis hanya membawa identitas layanan/`TariffId` dan qty; `TariffSnapshot` dari modul klinis tidak dipakai sebagai harga. **Titik sentuh ke Billing:** `POST from-source` publik yang masih menerima `UnitPrice` dari pemanggil harus dikunci (hanya internal atau harga dari tarif) — keputusan teknisnya milik Billing | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026. `SECURITY_PRIVACY_SIGNOFF` tetap `OPEN` | CAP-V2-03 |
| `RJ-E2E-DEC-001` | Decision | **Jasa konsultasi memakai `SourceDomain = CONSULTATION`, milestone `COMPLETED`, satu item per konsultasi.** Konsultasi yang batal sebelum selesai tidak menghasilkan tagihan (`SuppressedNoPriorCharge`). Konsultasi yang sudah `Completed` tidak dibuka ulang (`RJ-DOC-DEC-003`); koreksinya berupa adjustment Billing. Tarif dicari dari master yang sudah ada (`MstTariff.IsConsultationFee`, `DoctorServiceRule`, `MstPatientClass.DefaultConsultationFee`) — urutan prioritasnya lihat `RJ-E2E-OQ-002` | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 | CAP-V2-08 |
| `RJ-E2E-DEC-002` | Decision | **Biaya administrasi Rawat Jalan tetap dari `MstAdministrationFeePolicy` `RAJAL`** di dalam kalkulasi Billing. Usulan sumber `REGISTRATION` pada PRD FR-009 dan baris Source Mapping-nya **dicabut** agar tidak menagih dua kali. Contoh: policy `RAJAL` Rp25.000 muncul sebagai `AdministrationFeeAmount`, bukan item invoice | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 | CAP-V2-09 |
| `RJ-E2E-DEC-007` | Decision | **Rawat Jalan tidak pernah menutup kunjungan menjadi `Completed`**, termasuk kunjungan tanpa dokter. Kunjungan yang hanya skrining perawat berhenti di status setara *selesai pelayanan*. Endpoint ubah status bebas tidak boleh dipakai Rawat Jalan untuk `Completed`. Bentuk status/penanda untuk kunjungan tanpa dokter ditetapkan saat desain | Sukma Giri (titik sentuh Registration) | `approved` | Sukma Giri, 28 Sep 2026 | CAP-V2-11 (`NurseStationQueueController.cs:327-329`, `PatientEncounterController.cs:944-953`) |
| `RJ-E2E-DEC-008` | Decision | **Ringkasan Billing memakai butir akses baru khusus ringkasan per Encounter** (nama final saat desain, mis. `EncounterBillingSummary : Read`), hanya total dan status, tanpa daftar invoice lain atau riwayat pembayaran. Pola sama dengan `PatientBillingSummary : Read` rawat inap. Tombol *Buka Detail Billing* hanya tampil bila user juga memegang `BillingInvoice : Read`, dan server tetap menolak `403` tanpa butir itu. Keadaan *belum ada invoice* ditampilkan sebagai keadaan normal, bukan galat | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 | CAP-V2-17, `SEC-RJ-005` |
| `RJ-E2E-DEC-009` | Decision | **Fakta `Pending`/`OutcomeUnknown` diselesaikan pekerja otomatis ditambah antrean manual.** Pekerja latar mengirim ulang dengan identitas dan `IdempotencyKey` yang sama. Setelah batas percobaan tercapai, fakta masuk antrean rekonsiliasi untuk petugas Billing. Batas percobaan dan jeda dijadikan master data dengan bawaan fail-closed (pola `RJ-BIL-DEC-010`). Clinical truth tidak pernah dibatalkan karena Billing gagal | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 | CAP-V2-16 |
| `RJ-E2E-DEC-010` | Decision | **Koreksi pelayanan yang sudah `PERFORMED`/`COMPLETED`**: klinis menerbitkan fakta pembatalan versi baru; Billing membuat **adjustment** dengan persetujuan sesuai `RJ-BIL-DEC-004`, bukan void biasa. Kebijakan void adapter (`CONFIRMED`/`ACCEPTED` saja) **tidak** dilonggarkan. Riwayat versi lama tetap ada. Contoh: Lab Darah Lengkap `COMPLETED` salah pasien → fakta `CANCELLED` v3 → adjustment Billing menunggu persetujuan | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 | CAP-V2-15 |
| `RJ-E2E-DEC-012` | Decision | **[Dilengkapi `RJ-E2E-DEC-023`]** **Urutan pencarian tarif jasa konsultasi** (menutup `RJ-E2E-OQ-002`): (1) `MstDoctorServiceRule` aktif dan berlaku untuk dokter + klinik + kelas pasien yang memiliki `TariffId`; (2) `MstTariff` ber-`IsConsultationFee` untuk klinik + kelas pasien; (3) `MstTariff` ber-`IsConsultationFee` untuk klinik saja; (4) tidak ditemukan → antrean rekonsiliasi, **tidak pernah Rp0**. `MstPatientClass.DefaultConsultationFee` **tidak** dipakai karena bukan `MstTariff` (`RJ-E2E-DEC-006`) | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 (sesi `design-business-module`) | CAP-V2-08 |
| `RJ-E2E-DEC-013` | Decision | **Kunjungan Rawat Jalan tanpa dokter berhenti di `EncounterStatus.Billing`** (nilai `8`, sudah ada, belum ditulis kode mana pun). Tidak ada nilai enum baru. Jalur dokter tetap berhenti di `ConsultationCompleted`; keduanya bermakna *siap ditagih*. Melengkapi `RJ-E2E-DEC-007` | Sukma Giri (titik sentuh Registration) | `approved` | Sukma Giri, 28 Sep 2026 (sesi `design-business-module`) | CAP-V2-11 |
| `RJ-E2E-DEC-014` | Decision | **Baris folio lama** yang terbentuk sebelum jembatan folio → invoice dibuat ditandai *perlu rekonsiliasi* (kode `LEGACY_PRE_BRIDGE`) saat migration. Tidak ada yang diteruskan otomatis ke invoice, sehingga tidak ada tagihan ganda dengan input manual kasir, dan tidak ada yang hilang diam-diam. Petugas Billing memutuskan per baris | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 (sesi `design-business-module`) | `RJ-E2E-DEC-003` |
| `RJ-E2E-DEC-018` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `BE-RJE-002`**: penulisan source dan validasi runtime terhadap `QuilvianNewDevSukma`. Tanpa migration (tidak diperlukan), commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("commit dulu pekerjaan yang sudah selesai kemudian lanjutkan BE-RJE-002") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-019` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `BE-RJE-003`**: penulisan source dan validasi runtime terhadap `QuilvianNewDevSukma`, termasuk data uji. Tanpa migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("commit lebih dulu BE-RJE-002, kemudian lanjutkan BE-RJE-003") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-020` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `BE-RJE-004`** (task berikutnya menurut tabel gelombang): source dan validasi runtime terhadap `QuilvianNewDevSukma`. Tanpa migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("commit BE-RJE-003 dulu lalu lanjut ke task berikutnya") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-021` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `BE-RJE-006`**: source dan validasi runtime terhadap `QuilvianNewDevSukma`, termasuk penyesuaian data uji antrean. Tanpa migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("commit BE-RJE-004 dan lanjutkan") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-022` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `BE-RJE-005`**: source dan validasi runtime terhadap `QuilvianNewDevSukma`, termasuk mengubah status invoice uji. Tanpa migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("commit dulu baru lanjutkan BE-RJE-005") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-023` | Decision | **Urutan tarif jasa konsultasi dilengkapi jalur tindakan** (melengkapi `RJ-E2E-DEC-012`, tidak menggantikan): (1) `MstDoctorServiceRule.TariffId` — **wajib** tarif ber-`IsConsultationFee`; (1b) `MstDoctorServiceRule.ProcedureId` → `MstTariff` ber-`IsConsultationFee` untuk tindakan itu, kelas pasien yang cocok lebih dulu; (2)/(3) tarif konsultasi klinik (+ kelas); tidak ketemu → `TARIFF_NOT_FOUND`, tidak pernah Rp0. Sebab: di master, 2.130 tarif konsultasi seluruhnya ditautkan ke **tindakan** konsultasi (113 tindakan × 19 kelas) dan **tidak satu pun** berklinik; satu-satunya rule dokter bertarif menunjuk tarif pemeriksaan darah. Konsekuensi: admin wajib mengisi rule layanan dokter (dokter + klinik → tindakan konsultasi); sampai diisi, konsultasi masuk antrean rekonsiliasi | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 (sesi `build-module-backend BE-RJE-007`) | Query master `QuilvianNewDevSukma`; `MstDoctorServiceRule.cs` |
| `RJ-E2E-DEC-024` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `BE-RJE-007`**: source dan validasi runtime terhadap `QuilvianNewDevSukma`, termasuk data uji master rule dokter. Tanpa migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("commit BE-RJE-005 kemudian lanjutkan BE-RJE-007") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-025` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `BE-RJE-008`, `BE-RJE-009`, `BE-RJE-010`, dan `BE-RJE-014`**, dikerjakan berurutan: source dan validasi runtime terhadap `QuilvianNewDevSukma`, termasuk data uji (resep, stok obat, shift kasir, status invoice uji). Tanpa migration ke database lain, commit (kecuali diminta), push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("commit BE-RJE-007 kemudian lanjutkan BE-RJE-008 (obat dua tahap), BE-RJE-009 (pembatalan), BE-RJE-010, dan BE-RJE-014") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-026` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk task backend V2 yang tersisa — `BE-RJE-011`, `BE-RJE-013`, `BE-RJE-012`** — dikerjakan berurutan: source dan validasi runtime terhadap `QuilvianNewDevSukma`, termasuk data uji. Tanpa migration, commit (kecuali diminta), push, merge, maupun deployment. `BE-RJE-008`/`009`/`010`/`014` sudah di-commit pemilik (`2af16d93`) | Sukma Giri | `approved` | Sukma Giri, 29 Sep 2026 ("sudah saya commit mandiri, sekarang lanjutkan task yang perlu di kerjakan") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-011` | Decision | **`CONSUMABLE` ditunda ke slice berikutnya.** Producer belum ada. Baris Source Mapping PRD dikoreksi dari `REUSE` menjadi `LATER SLICE`, dan tidak masuk Definition of Done rilis ini | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 | CAP-V2-24 |
| `RJ-E2E-DEC-016` | Decision | **Unit sinkron invoice adalah `BilProcessingEffect`, bukan `BilChargeLine`** (kontrak `RJ-E2E-CONTRACT-001@1.0.2`). Ditemukan saat inspeksi `BE-RJE-001`: folio tidak membuat atau memperbarui `BilChargeLine` untuk fakta versi baru, melainkan mencatat satu `BilProcessingEffect` `PendingFinancialReview` yang menunjuk charge line versi 1 (`BillingFolioService.cs:183-266`); unique index `BilChargeLine` tidak memuat versi. Karena itu kolom sinkron, `IsClinicalCancellation`, dan kolom rekonsiliasi pindah ke `BilProcessingEffect` (satu baris per fakta + versi, sudah dirujuk `CliClinicalMilestoneFact.BillingProcessingEffectId`). `BilChargeLine` **tidak** diubah. Perilaku folio yang ada tidak disentuh. Aturan bisnis `RJ-E2E-DEC-003`/`009`/`014` tidak berubah | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 (sesi `build-module-backend BE-RJE-001`) | `BillingOperationalConfigurations.cs` (index `BilChargeLine`), `BillingFolioService.cs:183-266` |
| `RJ-E2E-DEC-017` | Approval | **Roadmap V2 (`e2e-backend-roadmap.md`, `e2e-frontend-roadmap.md` rev `1`) dan kontrak `1.0.1` disetujui; `IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `BE-RJE-001` saja**, mencakup penulisan source, **pembuatan migration**, dan **penerapan migration ke `QuilvianNewDevSukma` saja** sebagaimana diwajibkan AC 1 kartu task. Tidak mencakup database lain, commit, push, merge, maupun deployment. Task lain tetap `NOT_GRANTED` | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("saya menyetujui semuanya, lanjutkan … BE-RJE-001") | `roadmap/e2e-backend-roadmap.md` |
| `RJ-E2E-DEC-015` | Approval | **Desain V2 dan `RJ-E2E-CONTRACT-001@1.0.0` disetujui**: `02`/`03` bagian V2, `04-prd-to-mvp.md`, `data/data-dictionary.md`, `flowcharts/`, lima kontrak bagian V2, dan acceptance test matrix bagian V2. Approval ini **bukan** izin menulis code: `IMPLEMENTATION_AUTHORITY` tetap diberikan per task. Usulan `1.0.1` (field `BillingHandoffIssues` pada `finish-consultation`) **tidak** termasuk dan menunggu approval terpisah | Sukma Giri | `approved` | Sukma Giri, 28 Sep 2026 ("saya menyetujuinya") | `blueprint-manifest.md` revisi `27` |

### Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `RJ-E2E-FE-001` | Letak Ringkasan Billing | Sukma Giri | `approved` | Tab baru *Ringkasan Billing* di workspace konsultasi dokter, sesuai PRD §8 | PRD §8 |
| `RJ-E2E-FE-002` | Isi minimum panel | Sukma Giri | `approved` | Field PRD §17; seluruh nominal dari API, tanpa hitung ulang di client (`AC-RJ-009`, `AC-RJ-010`) | PRD §17 |
| `RJ-E2E-FE-003` | Tata letak, urutan field, gaya | Developer | `DEV_DISCRETION` | Wajib base component Quilvian (`Hero`, `DataTable`, `StatusBadge`, `BaseButton`, `AccessDeniedGate`, `ToastStack`); tanpa tombol Bayar/Diskon/Void/Finalisasi/Settlement | PRD §19–20, CAP-V2-23 |
| `RJ-E2E-FE-004` | Menampilkan kegagalan handoff saat Selesai Konsultasi | Developer | `DEV_DISCRETION` | `BillingHandoffIssues` wajib terlihat oleh dokter; bentuk pesan bebas asal tidak memblokir konsultasi yang sudah selesai | CAP-V2-10 |

### Koreksi atas PRD yang wajib dibawa ke desain

| Bagian PRD | Bunyi sekarang | Koreksi | Dasar |
|---|---|---|---|
| FR-007, `AC-RJ-007` | Obat ditagih hanya saat `DISPENSED` | Dua tahap: finalisasi lalu `DISPENSED` | `RJ-E2E-DEC-005` |
| FR-009, Source Mapping | Sumber `REGISTRATION` | Dicabut; pakai policy `RAJAL` | `RJ-E2E-DEC-002` |
| Source Mapping | `CONSUMABLE` = `REUSE` | `LATER SLICE` | `RJ-E2E-DEC-011` |
| §8 | Tab Laboratorium dan Radiologi dianggap sudah ada | Belum ada; tetap `CONDITIONAL` | CAP-V2-19, `RJ-DOC-DEC-002` |
| §18 | Reuse `charge-summary` dengan akses `BillingInvoice : Read` | Endpoint/butir akses ringkasan baru | `RJ-E2E-DEC-008` |
| §1 | Billing diposisikan `EXISTING/REUSE` penuh | Billing ada, tetapi **tidak ada jalur dari fakta klinis ke invoice** dan harga `from-source` datang dari pemanggil | CAP-V2-03, CAP-V2-13 |

### Acceptance criteria tambahan yang sudah dapat diuji

1. Tindakan yang dikonfirmasi pada kunjungan X muncul sebagai satu item di invoice kunjungan X
   tanpa input kasir. Mengirim fakta yang sama dua kali tetap menghasilkan satu item.
2. Mengirim `from-source` dengan `UnitPrice` berbeda dari `MstTariff` **tidak** mengubah harga item.
3. Resep yang difinalkan menghasilkan item `PHARMACY` tahap 1. Setelah dibayar, farmasi dapat
   menelaah dan menyerahkan obat. Dispensing 8 dari 10 tablet menghasilkan versi item 8 tablet.
4. Konsultasi `Completed` menghasilkan tepat satu item `CONSULTATION`. Konsultasi yang dibatalkan
   sebelum selesai tidak menghasilkan item.
5. Invoice Rawat Jalan tidak memuat item `REGISTRATION`. Biaya admin muncul hanya lewat
   `AdministrationFeeAmount`.
6. Kunjungan tanpa dokter tidak pernah berstatus `Completed` oleh aksi Rawat Jalan.
7. Pengguna yang hanya memegang butir ringkasan mendapat `403` saat memanggil
   `GET billing/invoices` atau `GET billing/invoices/{id}`.
8. Fakta `OutcomeUnknown` dikirim ulang otomatis dengan `IdempotencyKey` yang sama, dan setelah
   batas percobaan muncul di antrean rekonsiliasi.
9. Pembatalan Lab berstatus `COMPLETED` menghasilkan permintaan adjustment, bukan void; item lama
   tetap terbaca di riwayat.

### Open question dan blocker

| ID | Isi | Owner | Memblokir |
|---|---|---|---|
| `RJ-E2E-OQ-001` | Tidak ada project automated test yang ter-commit di backend `063d38b`, sementara `RJ-BIL-DEC-019` merujuk `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/`. Perlu dipastikan apakah test berada di cabang lain atau hilang | Sukma Giri | `IMPLEMENTATION` (verifikasi DoD 17–20) |
| `RJ-E2E-OQ-002` | ~~Urutan prioritas sumber tarif konsultasi~~ — **ditutup `RJ-E2E-DEC-012`** | Billing/Finance | — |
| `RJ-E2E-OQ-003` | Kelebihan bayar obat bila dispensing lebih sedikit dari yang ditagih — refundable credit atau dikembalikan tunai | Billing/Finance | `LATER SLICE` (di luar scope, titik sentuh saja) |
| `RJ-E2E-OQ-004` | `RJ-E2E-DEC-004` — siapa mengubah Encounter `Billing → Completed` | Registration + Billing | `LATER SLICE` |
| `RJ-E2E-OQ-005` | Sign-off formal `FORMAL_PHARMACY_SIGNOFF`, `FORMAL_FINANCE_SIGNOFF`, `SECURITY_PRIVACY_SIGNOFF` untuk `RJ-E2E-DEC-005`/`006` | Farmasi, Finance, Security | Aktivasi production, **bukan** desain |

Tidak ada invariant kritis yang masih terbuka untuk tahap desain.

## Amendment Pass 2026-10-02 — Daftar Pasien Rawat Jalan

| Field | Nilai |
|---|---|
| Mode | `Amendment pass` — blueprint sudah disetujui; keputusan lama **tidak** ditimpa, hanya diamandemen dengan penanda |
| Pemicu | Petugas mendaftarkan pasien lama dan ditolak dengan pesan "Pasien masih memiliki kunjungan aktif bernomor ENC-RSMMC-00146 tanggal 30 Jul 2026. Selesaikan atau batalkan kunjungan tersebut…" (`RJ-DOC-REV-BE-007`). Frontend belum punya layar untuk membatalkan kunjungan, sehingga petugas buntu |
| Capability map | `01-existing-capability-map-prd-v2.md` tercatat pada BE `063d38b`; HEAD sekarang BE `245f0464`, FE `b7e9b7fd4`. Map **berpotensi basi** dan belum mencakup daftar kunjungan — wajib `trace-existing-capabilities` mode impact scan sebelum desain |
| Prefix keputusan | `RJ-DOC-DEC-011` dst., `RJ-DOC-FE-005` dst., `RJ-DOC-OQ-007` dst. Task nantinya `RJ-DOC-REV-*` |
| Pengambil keputusan | Sukma Giri, pemilik blueprint |

### Batas scope amendment

**Satu kalimat:** satu layar daftar kunjungan Rawat Jalan yang disaring sesuai pengguna yang
login, untuk memantau status kunjungan dan membatalkan kunjungan yang menggantung.

| Di dalam scope | Di luar scope — pemiliknya |
|---|---|
| Daftar kunjungan Rawat Jalan, disaring di backend sesuai pengguna | Transisi `Billing → Completed` — **Registration + Billing** (`RJ-E2E-OQ-004`, tetap terbuka) |
| Summary per kelompok status, termasuk kunjungan menggantung | Pembersihan massal ±165 kunjungan lama di DB dev — pekerjaan data, bukan fitur |
| Aksi Batalkan beserta guard status dan alasan | Aturan internal antrean dokter dan antrean perawat |
| Hak akses "lihat semua" dan "batal" | Kunjungan IGD (`EmergencyVisitService`) dan Rawat Inap (episode) |
| Amandemen definisi kunjungan pemblokir pendaftaran (`RJ-DOC-DEC-010` (c)) | Pembatalan/penyelesaian konsultasi — tetap lewat workspace dokter |

### Keputusan

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-DOC-DEC-011` | Decision | **Bentuk blueprint `SINGLE`** — fitur masuk sebagai revisi roadmap doctor-consultation (`RJ-DOC-REV-*`), sama seperti `RJ-DOC-REV-BE-007`. `shape_decided_by = USER_CONFIRMED`. Hasil uji: 0 dari 5 syarat pemecahan — tidak punya bounded context, kosakata status, atau master data sendiri; memakai `RegPatientEncounter` yang sudah ada. Tidak ada kemampuan tanpa rumpun | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | Jawaban sesi |
| `RJ-DOC-DEC-012` | Decision | **Daftar hanya memuat kunjungan Rawat Jalan.** IGD dan Rawat Inap tidak tampil karena masing-masing punya layar dan penjaga sendiri | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | `EmergencyVisitService`; episode rawat inap |
| `RJ-DOC-DEC-013` | Decision | **Penyaringan per pengguna dikerjakan di backend, bukan di frontend.** Aturannya: (a) dokter melihat kunjungan yang `DoctorId`-nya adalah dirinya (pola `DoctorQueueController.ResolveAllowedDoctorIdAsync`); kunjungan tanpa dokter tidak tampil bagi dokter. (b) Perawat melihat semua kunjungan di klinik yang termasuk cluster nurse station tugasnya, apa pun dokternya (pola `NurseStationQueueController.GetAllowedClusterIdsAsync` → `GetClinicIdsByClusterIdsAsync`). (c) Pengguna yang terhubung sebagai dokter **dan** perawat melihat gabungan keduanya. (d) Pengguna tanpa data dokter, tanpa cluster, dan tanpa hak "lihat semua" ditolak dengan kode 403 dan pesan yang jelas — bukan tabel kosong. **Contoh:** dr. A praktik di Poli Penyakit Dalam; perawat B bertugas di cluster yang memuat Poli Penyakit Dalam dan Poli Jantung. dr. A hanya melihat pasiennya sendiri; B melihat seluruh pasien kedua poli, termasuk pasien dr. C | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | `DoctorQueueController.cs:124-129`, `NurseStationQueueController.cs:99-110` |
| `RJ-DOC-DEC-014` | Decision | **Pengecualian "lihat semua" memakai butir hak akses baru, bukan nama role.** Nama final ditetapkan saat desain (contoh: `OutpatientEncounterList : ReadAll`). Admin rumah sakit dapat memberikannya ke Super Admin, petugas pendaftaran, atau kepala ruangan tanpa mengubah kode. Pola `IsCurrentUserSuperAdminAsync` yang menulis nama role langsung **tidak** ditiru | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | Aturan backend "tanpa hardcode role" (`RJ-DOC-REV-BE-007` §1) |
| `RJ-DOC-DEC-015` | Decision | **Tombol Batalkan hanya untuk pemegang butir hak akses batal**, dan hanya pada kunjungan yang memang boleh ia lihat menurut `RJ-DOC-DEC-013`. Server tetap menolak 403 walau tombol disembunyikan. Butir yang dipakai (`PatientEncounter : Update` atau butir baru `Cancel`) ditetapkan saat desain | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | Jawaban sesi |
| `RJ-DOC-DEC-016` | Decision | **Kunjungan hanya boleh dibatalkan selama statusnya belum masuk konsultasi**, yaitu `Draft` (0) sampai `Menunggu Dokter` (5). Mulai `Sedang Konsultasi` (6) permintaan ditolak, supaya tidak ada konsultasi, resep, order, atau tagihan yang terlepas dari kunjungan yang batal. Hasil skrining perawat yang sudah ada tetap tersimpan sebagai riwayat. **Contoh:** pasien sudah diskrining (status 5) lalu pulang sebelum dipanggil dokter → boleh dibatalkan. Pasien sedang diperiksa (status 6) → ditolak dengan pesan "Kunjungan sedang dalam konsultasi. Batalkan atau selesaikan konsultasi lewat workspace dokter." | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | `EncounterStatus.cs`; `PatientEncounterController.cs:1076-1112` |
| `RJ-DOC-DEC-017` | Decision | **Guard pembatalan dipasang pada endpoint batal khusus layar ini**, bukan pada `PATCH /patient-encounters/{id}/cancel` yang lama. Endpoint lama tidak diubah agar pemanggil lain tidak rusak. Lemahnya guard endpoint lama (hanya menolak kunjungan yang sudah selesai) dicatat sebagai `RJ-DOC-OQ-008` | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | `PatientEncounterController.cs:1085-1088` |
| `RJ-DOC-DEC-018` | Decision | **Alasan pembatalan berupa teks bebas, wajib, maksimal 250 karakter**, sesuai `PatientEncounterCancelRequest`. Tidak ada master alasan baru. Pembatalan mencatat pengguna dan waktu (`CancelledByUserId`, `CancelledAt`) dan ikut membatalkan antrean kunjungan (perilaku `CancelQueuesByEncounterAsync` yang sudah ada) | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | `PatientEncounterDtos.cs:597-602` |
| `RJ-DOC-DEC-019` | Decision | **Amandemen `RJ-DOC-DEC-010` (c): kunjungan berstatus `Konsultasi Selesai` (7) dan `Proses Billing` (8) tidak lagi menghalangi pendaftaran baru.** Pemblokir pendaftaran hanya kunjungan yang belum batal/dihapus, `CompletedAt` kosong, dan berstatus di bawah 7. Pelayanan klinisnya sudah selesai; penutupan ke `Completed` tetap urusan Registration + Billing (`RJ-E2E-OQ-004`). Karena itu layar ini **tidak** punya tombol Selesaikan dan `RJ-E2E-DEC-007` tetap utuh. **Batas teknis yang mengikat:** `MedicalRecordAccessAuditService.KunjunganMasihBerjalan` juga dipakai untuk hak akses rekam medis, sehingga definisi itu **tidak boleh** diubah; pemblokir pendaftaran memakai definisi tersendiri. **Contoh:** pasien selesai diperiksa di Poli Dalam (status 7), tagihannya belum dibayar, lalu ingin ke Poli Mata hari itu → pendaftaran diterima. Pasien yang masih `Menunggu Dokter` (5) di Poli Dalam → tetap ditolak | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026. Mengamandemen `RJ-DOC-DEC-010` (c), tidak menggantikannya | `RJ-DOC-REV-BE-007` §5 risiko 2; `MedicalRecordAccessAuditService.cs:73-96` |
| `RJ-DOC-DEC-020` | Decision | **Kunjungan yang menggantung di `Sedang Konsultasi` (6) diselesaikan lewat workspace dokter.** Layar ini hanya menampilkannya dengan petunjuk tindakan. Efek finish/cancel konsultasi terhadap status kunjungan diverifikasi saat trace (`RJ-DOC-OQ-007`) | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | Jawaban sesi |

### Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `RJ-DOC-FE-005` | Letak menu | Sukma Giri | `approved` | Menu baru "Daftar Pasien Rawat Jalan" di grup Rawat Jalan, tepat di bawah "Skrining Pasien" | Permintaan pemilik |
| `RJ-DOC-FE-006` | Komponen wajib | Sukma Giri | `approved` | Base component Quilvian: `Hero`, summary card, base filter, base table. Tanpa komponen gaya baru bila base component sudah memadai | Permintaan pemilik |
| `RJ-DOC-FE-007` | Tampilan awal | Sukma Giri | `approved` | Default kunjungan **hari ini**, ditambah tab/filter "Aktif semua tanggal". Summary card "Menggantung" (kunjungan aktif bertanggal sebelum hari ini) dapat diklik untuk langsung memfilter. **Contoh:** pada 2 Okt 2026, ENC-RSMMC-00146 (30 Jul 2026) tidak tampil di tab hari ini, tetapi terhitung di kartu Menggantung | Jawaban sesi |
| `RJ-DOC-FE-008` | Pembatalan | Sukma Giri | `approved` | Modal konfirmasi dengan alasan wajib (maks 250). Tombol hanya muncul bila pengguna punya hak batal **dan** status 0–5. Baris status 6 menampilkan petunjuk ke workspace dokter, bukan tombol | `RJ-DOC-DEC-015`/`016`/`020` |
| `RJ-DOC-FE-009` | Kolom tabel, urutan, isi filter, pengelompokan summary, gaya | Developer | `DEV_DISCRETION` | Usulan awal: kolom no. kunjungan, tanggal, pasien/no. RM, klinik, dokter, penjamin, status, aksi; filter tanggal, status, klinik, dokter (hanya bagi pemegang "lihat semua"), pencarian. Wajib base component pada `RJ-DOC-FE-006` | — |

### Acceptance criteria yang sudah dapat diuji

1. Dokter A memanggil daftar dan hanya menerima kunjungan Rawat Jalan dengan `DoctorId` = A.
   Mengirim parameter dokter lain tidak melebarkan hasil.
2. Perawat B menerima seluruh kunjungan Rawat Jalan di klinik pada cluster tugasnya, dan tidak
   menerima kunjungan klinik di luar cluster tersebut.
3. Pengguna tanpa data dokter, tanpa cluster, dan tanpa hak "lihat semua" menerima 403.
4. Pemegang hak "lihat semua" menerima kunjungan seluruh klinik tanpa perlu role Super Admin.
5. Kunjungan IGD dan Rawat Inap tidak pernah muncul di daftar.
6. Membatalkan kunjungan berstatus 0–5 dengan alasan berhasil; antreannya ikut batal; pasien
   yang sama lalu dapat didaftarkan kembali.
7. Membatalkan kunjungan berstatus 6 ke atas ditolak (400) dengan pesan yang menyebut workspace dokter.
8. Membatalkan tanpa alasan, atau alasan lebih dari 250 karakter, ditolak (400).
9. Pengguna tanpa hak batal, atau membatalkan kunjungan di luar cakupannya, ditolak (403).
10. Pasien dengan kunjungan berstatus 7 atau 8 dapat didaftarkan ke poli lain; pasien dengan
    kunjungan berstatus 0–6 tetap ditolak dengan pesan `RJ-DOC-REV-BE-007`.
11. Hak akses rekam medis (`KunjunganMasihBerjalan`) tidak berubah perilakunya setelah amandemen.
12. `PATCH /patient-encounters/{id}/cancel` lama berperilaku sama seperti sebelum fitur ini.

### Open question dan blocker

| ID | Isi | Owner | Memblokir |
|---|---|---|---|
| `RJ-DOC-OQ-007` | Apakah finish/cancel konsultasi di workspace dokter memindahkan status kunjungan keluar dari 6? Bila tidak, kunjungan status 6 bisa terjebak selamanya | Trace (`trace-existing-capabilities`) | `DESIGN` |
| `RJ-DOC-OQ-008` | `PATCH /patient-encounters/{id}/cancel` lama membolehkan pembatalan status berapa pun selama belum selesai. Perlu ditelusuri siapa pemanggilnya, lalu diputuskan apakah diperketat di task terpisah | Sukma Giri setelah trace | `LATER SLICE` |
| `RJ-DOC-OQ-009` | Cara membedakan kunjungan Rawat Jalan dari IGD/Rawat Inap di `RegPatientEncounter` (jenis kunjungan, service unit, atau penanda lain) | Trace | `DESIGN` |
| `RJ-DOC-OQ-010` | Nama final butir hak akses "lihat semua" dan "batal", serta Resource pemiliknya | Desain | `DESIGN` |
| `RJ-DOC-OQ-011` | Penanganan ±165 kunjungan lama di DB dev (dan data serupa sebelum rilis). Setelah `RJ-DOC-DEC-019`, sebagian besar mungkin sudah tidak memblokir; sisanya ditutup lewat layar ini | Sukma Giri | Di luar scope — bukan blocker desain |

Tidak ada invariant klinis atau bisnis kritis yang masih terbuka. `RJ-DOC-OQ-007` dan
`RJ-DOC-OQ-009` dijawab dari source code, bukan keputusan bisnis baru.

### Closure 2026-10-02 — hasil impact scan

Sumber: [01-capability-impact-scan-daftar-pasien-rj.md](01-capability-impact-scan-daftar-pasien-rj.md)
(BE `245f0464`, FE `b7e9b7fd4`).

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-DOC-DEC-021` | Decision | **Amandemen `RJ-DOC-DEC-016` dan `RJ-DOC-DEC-020` (menutup `CONFLICT-DP-2`):** kunjungan berstatus `Sedang Konsultasi` (6) **boleh** dibatalkan dari layar ini bila kunjungan itu **tidak punya konsultasi aktif**, yaitu seluruh konsultasinya sudah batal atau belum pernah dibuat. Bila masih ada konsultasi aktif, permintaan ditolak dan dokter menyelesaikan/membatalkan konsultasinya lebih dulu. Sebab: membatalkan konsultasi tidak memindahkan status kunjungan, sehingga tanpa aturan ini kunjungan tertahan di 6 selamanya. **Contoh:** dr. A memanggil pasien, membuka konsultasi, pasien pergi, dr. A membatalkan konsultasi → kunjungan masih 6 tanpa konsultasi aktif → petugas dapat membatalkannya. Status 7 ke atas tetap tidak dapat dibatalkan | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | `DoctorConsultationController.cs#CancelConsultation`; DB dev: 7 dari 15 kunjungan status 6 tanpa konsultasi aktif |
| `RJ-DOC-DEC-022` | Decision | **Amandemen `RJ-DOC-DEC-019` (menutup `CONFLICT-DP-1`):** pemblokir pendaftaran Rawat Jalan hanya menghitung **kunjungan Rawat Jalan berklinik** yang aktif, yaitu `EncounterType = Outpatient`, `ClinicId` terisi, tidak punya `EmgVisit`, belum batal/dihapus, `CompletedAt` kosong, dan status di bawah 7. Kunjungan penunjang tanpa klinik, IGD, dan Rawat Inap tidak lagi memblokir; masing-masing ditangani modulnya. Dengan ini, setiap kunjungan yang memblokir pasti terlihat dan dapat dibatalkan di Daftar Pasien Rawat Jalan. **Contoh:** pasien punya kunjungan lab walk-in yang menggantung, lalu mendaftar ke Poli Dalam → diterima | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026. Mengamandemen `RJ-DOC-DEC-010` (c) lebih lanjut | `CAP-DP-01`, `CAP-DP-09`; DB dev: 5 kunjungan non-RJ aktif |
| `RJ-DOC-DEC-023` | Decision | **`RJ-DOC-DEC-017` dikonfirmasi** setelah diketahui frontend tidak memanggil `PATCH /patient-encounters/{id}/cancel`: tetap endpoint batal baru; endpoint lama tidak diubah (`RJ-DOC-OQ-008` tetap `LATER SLICE`) | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 | `CAP-DP-07` |

**Jawaban open question:**

| ID | Status | Jawaban |
|---|---|---|
| `RJ-DOC-OQ-007` | `closed` | Batal konsultasi tidak mengubah status kunjungan → ditangani `RJ-DOC-DEC-021` |
| `RJ-DOC-OQ-009` | `closed` | Rawat Jalan = `EncounterType = Outpatient` + `ClinicId` terisi + tanpa `EmgVisit` (`CAP-DP-01`) |
| `RJ-DOC-OQ-010` | `open` → desain | Pola tersedia: `AccessExplicitPermission` + `AccessPermissionService.HasAccessAsync` (`CAP-DP-05`). Nama butir diputuskan saat desain |
| `RJ-DOC-OQ-008` | `open`, `LATER SLICE` | Tidak ada pemanggil di frontend maupun internal backend |
| `RJ-DOC-OQ-011` | `open`, di luar scope | Data DB dev 2 Okt 2026 (read-only): 159 kunjungan aktif. 126 RJ berklinik status 0-5 dan 7 status 6 tanpa konsultasi → dapat dibatalkan dari layar baru; 8 status 6 dengan konsultasi aktif → lewat dokter; 13 status 7-8 dan 5 non-RJ → tidak lagi memblokir. ENC-RSMMC-00146 = RJ berklinik status 3 tanpa konsultasi → dapat dibatalkan dari layar baru. 13 pasien punya lebih dari satu kunjungan RJ aktif status < 7 |

**Acceptance criteria tambahan / pengganti:**

- AC 7 diganti: membatalkan kunjungan status 6 yang masih punya konsultasi aktif, atau status 7
  ke atas, ditolak (400) dengan pesan yang menyebut workspace dokter. Status 6 tanpa konsultasi
  aktif berhasil dibatalkan.
- AC 10 diganti: pasien yang hanya punya kunjungan aktif berstatus 7-8, kunjungan penunjang
  tanpa klinik, atau kunjungan IGD dapat didaftarkan ke poliklinik. Pasien dengan kunjungan RJ
  berklinik status 0-6 tetap ditolak dengan pesan `RJ-DOC-REV-BE-007`.
- AC 13: setiap kunjungan yang disebut pesan penolakan pendaftaran dapat ditemukan di Daftar
  Pasien Rawat Jalan oleh pemegang "lihat semua".

Tidak ada conflict atau invariant kritis yang masih terbuka untuk desain.

### Approval 2026-10-02 — desain Amendment DP

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-DOC-DEC-024` | Approval | **Desain Amendment DP (revisi `28`) dan kontrak `RJ-DOC-ENCLIST-001@1.0.0` disetujui**: bagian *Amendment DP* pada `02`, `03`, `04`, `data/`, `contracts/`, `testing/`, serta `flowcharts/daftar-pasien-rawat-jalan.md`. `requirement-completeness-gate` dilewati atas persetujuan pemilik. Approval ini **bukan** izin menulis code; `IMPLEMENTATION_AUTHORITY` tetap per task | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 ("lanjutkan") | `blueprint-manifest.md` revisi `28` |
| `RJ-DOC-DEC-025` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk seluruh task Amendment DP** — `RJ-DOC-REV-BE-008`, `BE-009`, `BE-010`, `RJ-DOC-REV-FE-010`, `FE-011` — dikerjakan berurutan sesuai grafik dependency: penulisan source dan validasi runtime terhadap `QuilvianNewDevSukma`, termasuk data uji yang dibersihkan lewat endpoint aplikasi. Tanpa migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 ("semua task saya izinkan untuk implementasi") | `roadmap/doctor-consultation-roadmap.md` bagian 11 |

### Amendment 2026-10-02 — penangguhan sementara pemblokir pendaftaran

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-DOC-DEC-026` | Decision | **Pemblokir "satu pasien satu kunjungan aktif" ditangguhkan sementara.** Pendaftaran (`POST /patient-encounters`, `/admin`, `/kiosk`) tidak lagi ditolak karena pasien masih punya kunjungan Rawat Jalan berklinik yang belum selesai. Aturan `RJ-DOC-DEC-010` (c) beserta amandemen `RJ-DOC-DEC-019`/`022` **tidak dicabut**: logikanya tetap ada dan dapat dihidupkan kembali lewat konfigurasi `HealthServices:Registration:BlockActiveEncounter` (bawaan `false`) tanpa perubahan kode. Daftar Pasien Rawat Jalan dan pembatalan tetap berjalan. **Contoh:** pasien dengan ENC-RSMMC-00146 (status 3) mendaftar ke poli → diterima, dan kunjungan lama tetap terlihat di kartu Menggantung | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 ("hilangkan dulu batasan untuk pasien yang belum terselesaikan kunjungannya untuk sementara") | `RJ-DOC-REV-BE-007`, `RJ-DOC-REV-BE-008` |
| `RJ-DOC-DEC-027` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `RJ-DOC-REV-BE-011`**: source dan validasi runtime terhadap `QuilvianNewDevSukma`. Tanpa migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 2 Okt 2026 (instruksi yang sama) | `roadmap/doctor-consultation-roadmap.md` bagian 12 |

## Amendment Pass 2026-10-05 — Konsultasi Tertunda di Klinis Dokter

| Field | Nilai |
|---|---|
| Mode | `Amendment pass` — keputusan lama tidak ditimpa |
| Pemicu | Saklar `HealthServices:Registration:BlockActiveEncounter` dihidupkan. Pasien IKBAL YULIYANTO (00-00-00-15) ditolak mendaftar karena ENC-RSMMC-00172 (30 Sep 2026, Poli Anak, dr. Arif Lesmana) masih `Sedang Konsultasi` (6) dengan konsultasi aktif. Daftar Pasien Rawat Jalan menyuruh "selesaikan atau batalkan konsultasi lewat workspace dokter", tetapi Klinis Dokter hanya memuat antrean **hari ini**, sehingga dr. Arif tidak dapat membuka kunjungan itu. Kunjungan tertahan tanpa jalan keluar lewat UI |
| Source | BE `bb46ccc8`, FE `d232feb2b`. Capability map tidak diperbarui; fakta di bawah diambil langsung dari source pada sesi ini |
| Prefix keputusan | `RJ-DOC-DEC-028` dst., `RJ-DOC-FE-010` dst., `RJ-DOC-OQ-012` dst. Task nantinya `RJ-DOC-REV-*` |
| Pengambil keputusan | Sukma Giri, pemilik blueprint |

### Fakta dari source

| ID | Fakta | Evidence |
|---|---|---|
| F-KT-1 | Frontend Klinis Dokter selalu meminta antrean tanggal hari ini; layar tidak punya pemilih tanggal | `V2QuilvianSystemFrontendDev/src/lib/hooks/health-services/registration-management/doctor-queue/use-doctor-queue.js:81` |
| F-KT-2 | `GET /doctor-queues` memfilter `QueueDate.Date == selectedDate`. Parameter `queueDate` sudah diterima, tetapi hanya satu tanggal | `DoctorQueueController.cs:706-732` |
| F-KT-3 | Aksi antrean dan konsultasi berdasarkan id **tidak** memfilter tanggal. Yang terkunci hanya daftarnya | `DoctorQueueController.cs:966-983` (`GetAllowedQueueWithEncounterAsync`) |
| F-KT-4 | Finalisasi konsultasi memindahkan kunjungan ke `Konsultasi Selesai` (7), sehingga tidak lagi memblokir pendaftaran | `ConsultationFinalizationService.cs:159`; `OutpatientEncounterRules.cs:23` |
| F-KT-5 | Batal konsultasi tidak mengubah status kunjungan. Sesudahnya kunjungan status 6 tanpa konsultasi aktif dapat dibatalkan petugas di Daftar Pasien Rawat Jalan | `RJ-DOC-DEC-021`; `OutpatientEncounterListService.cs:299-309` |

### Batas scope amendment

**Satu kalimat:** dokter dapat menemukan dan membuka kunjungan Rawat Jalan bertanggal lampau
yang masih tertahan di tahap konsultasinya, lalu menyelesaikan atau membatalkan konsultasi itu
dari Klinis Dokter.

| Di dalam scope | Di luar scope — pemiliknya |
|---|---|
| Daftar "Konsultasi tertunda" lintas tanggal di panel Pasien Dokter | Aturan validasi finalisasi konsultasi — tetap seperti sekarang |
| Membuka kunjungan tertunda di workspace dan menjalankan Selesaikan/Batalkan konsultasi | Transisi `Billing → Completed` — **Registration + Billing** (`RJ-E2E-OQ-004`) |
| Peringatan untuk kunjungan bertanggal lampau | Aturan pemblokir pendaftaran (`RJ-DOC-DEC-019`/`022`/`026`) |
| Teks petunjuk di Daftar Pasien Rawat Jalan agar menunjuk ke tempat yang benar | Kunjungan status 0–5 — sudah ditangani petugas lewat Daftar Pasien Rawat Jalan |
| | IGD dan Rawat Inap |

### Keputusan

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-DOC-DEC-028` | Decision | **Bentuk blueprint `SINGLE`** — masuk roadmap doctor-consultation sebagai task revisi `RJ-DOC-REV-*`. `shape_decided_by = USER_CONFIRMED`. Hasil uji: 0 dari 5 syarat pemecahan; tidak ada bounded context, kosakata status, Resource, atau master data baru. Tidak ada kemampuan tanpa rumpun | Sukma Giri | `approved` | Sukma Giri, 5 Okt 2026 | Jawaban sesi |
| `RJ-DOC-DEC-029` | Decision | **Kunjungan lama ditemukan lewat daftar "Konsultasi tertunda", bukan pemilih tanggal.** Daftar memuat kunjungan milik dokter yang login dari tanggal **sebelum hari ini**, tanggal berapa pun, sehingga dokter tidak perlu mengingat tanggalnya. Penyaringan per dokter memakai aturan yang sama dengan antrean dokter hari ini (dokter hanya melihat antrean dengan `DoctorId` dirinya). Antrean hari ini tidak berubah. **Contoh:** pada 5 Okt 2026, dr. Arif membuka Klinis Dokter; antrean hari ini kosong, tetapi Konsultasi tertunda berisi 1 pasien: IKBAL YULIYANTO, ENC-RSMMC-00172, 30 Sep 2026 | Sukma Giri | `approved` | Sukma Giri, 5 Okt 2026 | F-KT-1, F-KT-2 |
| `RJ-DOC-DEC-030` | Decision | **Isi daftar Konsultasi tertunda hanya antrean berstatus `Sedang Konsultasi` yang kunjungannya masih status 6 dan punya konsultasi aktif** (belum batal, belum selesai). Kunjungan status 0–5 dari hari lalu tidak dimuat karena sudah dapat dibatalkan petugas di Daftar Pasien Rawat Jalan (`RJ-DOC-DEC-016`). **Contoh:** pasien kemarin yang masih `Menunggu Dokter` tidak muncul di daftar dokter; petugas membatalkannya dari Daftar Pasien Rawat Jalan | Sukma Giri | `approved` | Sukma Giri, 5 Okt 2026 | F-KT-5; `RJ-DOC-DEC-021` |
| `RJ-DOC-DEC-031` | Decision | **Pada konsultasi tertunda, dokter boleh Selesaikan dan Batalkan, sama seperti konsultasi hari ini.** Validasi finalisasi, penjaga penulis tunggal (`EnsureSoleAuthorAsync`), dan penjaga keutuhan dokumen tetap berlaku tanpa pengecualian. Waktu tanda tangan tercatat saat finalisasi dilakukan, bukan tanggal kunjungan. Sesudah Selesaikan, kunjungan menjadi status 7. Sesudah Batalkan, kunjungan tetap 6 tanpa konsultasi aktif dan petugas membatalkannya di Daftar Pasien Rawat Jalan (`RJ-DOC-DEC-021`). **Contoh:** dr. Arif menyelesaikan konsultasi IKBAL pada 5 Okt; ENC-RSMMC-00172 menjadi status 7 dan IKBAL dapat didaftarkan lagi | Sukma Giri | `approved` | Sukma Giri, 5 Okt 2026 | F-KT-3, F-KT-4, F-KT-5 |

### Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `RJ-DOC-FE-010` | Letak daftar | Sukma Giri | `approved` | Di panel kiri "Pasien Dokter" Klinis Dokter, terpisah dari antrean hari ini dan menampilkan jumlahnya. Bentuk tab atau bagian terpisah: `DEV_DISCRETION` dengan base component Quilvian | `RJ-DOC-DEC-029` |
| `RJ-DOC-FE-011` | Peringatan kunjungan lampau | Sukma Giri | `approved` | (a) Banner di workspace selama kunjungan bertanggal sebelum hari ini terbuka, menyebut tanggal kunjungan, contoh: "Kunjungan tanggal 30 Sep 2026 — pasien mungkin sudah pulang." (b) Saat Selesaikan, bila konsultasi memuat resep atau order yang akan terkirim, tampil konfirmasi tambahan sebelum finalisasi. Hanya frontend; aturan finalisasi backend tidak berubah | Jawaban sesi |
| `RJ-DOC-FE-012` | Teks petunjuk Daftar Pasien Rawat Jalan | Developer | `DEV_DISCRETION` | Petunjuk baris status 6 dengan konsultasi aktif menyebut lokasi yang benar, contoh: "Selesaikan atau batalkan lewat Klinis Dokter → Konsultasi tertunda." | Gambar pengguna 5 Okt 2026 |
| `RJ-DOC-FE-013` | Bentuk daftar tertunda (menetapkan sebagian `RJ-DOC-FE-010`) | Sukma Giri | `approved` | **Tab** di atas judul "Pasien Dokter": "Hari ini (N)" dan "Tertunda (N)", supaya antrean hari ini dan konsultasi tertunda tidak tercampur. Bawaan tab Hari ini. Memakai `ClinicalTabNav` yang sudah ada. Diputuskan 5 Okt 2026 setelah melihat versi bagian terpisah | Permintaan pemilik |

### Acceptance criteria yang sudah dapat diuji

1. Dokter A dengan antrean `Sedang Konsultasi` bertanggal kemarin atau lebih lama, yang
   kunjungannya status 6 dan punya konsultasi aktif, melihat antrean itu di Konsultasi tertunda.
2. Antrean milik dokter lain tidak pernah muncul di Konsultasi tertunda dokter A. Mengirim
   parameter dokter lain tidak melebarkan hasil.
3. Antrean bertanggal hari ini tidak muncul di Konsultasi tertunda; antrean hari ini tetap
   berperilaku seperti sebelum perubahan.
4. Kunjungan lampau berstatus 0–5, status 7 ke atas, batal, atau status 6 tanpa konsultasi aktif
   tidak muncul di Konsultasi tertunda.
5. Dokter dapat membuka konsultasi tertunda, mengisi, lalu Selesaikan. Kunjungan berubah ke
   status 7, hilang dari Konsultasi tertunda, dan pasien dapat didaftarkan lagi saat
   `BlockActiveEncounter = true`.
6. Dokter dapat Batalkan konsultasi tertunda. Kunjungan hilang dari Konsultasi tertunda dan
   petugas dapat membatalkannya dari Daftar Pasien Rawat Jalan.
7. Dokter bukan penulis konsultasi tetap ditolak saat Selesaikan/Batalkan (penjaga lama).
8. Workspace menampilkan banner tanggal kunjungan selama kunjungan lampau terbuka. Selesaikan
   yang memuat resep/order meminta konfirmasi tambahan.
9. Contoh nyata: ENC-RSMMC-00172 (IKBAL YULIYANTO) muncul di Konsultasi tertunda dr. Arif
   Lesmana dan dapat diselesaikan.

### Open question dan blocker

| ID | Isi | Owner | Memblokir |
|---|---|---|---|
| `RJ-DOC-OQ-012` | Antrean dengan status kunjungan 6 tetapi status antrean bukan `Sedang Konsultasi` (data tidak konsisten), bila ada, tidak tertangkap daftar ini. Perlu dicek di data saat desain; bila ada, diputuskan apakah ikut dimuat | Desain | `DESIGN` (bukan invariant kritis) |
| `RJ-DOC-OQ-013` | Bentuk endpoint: parameter baru pada `GET /doctor-queues` atau endpoint terpisah, beserta `AccessPermission`-nya | Desain | `DESIGN` |

Tidak ada invariant klinis atau bisnis kritis yang masih terbuka.

### Hasil desain 2026-10-05 (revisi `29`, `draft`)

| ID | Status | Jawaban / isi |
|---|---|---|
| `RJ-DOC-OQ-012` | `open`, tidak memblokir | Desain mensyaratkan antrean `InConsultation` karena `finish-consultation` menuntutnya. Task backend menghitung kasus yang tidak tertangkap di DB uji dan melaporkannya (`02` KT.3.1) |
| `RJ-DOC-OQ-013` | `closed` oleh desain (menunggu approval) | Endpoint terpisah `GET /doctor-queues/pending-consultations`, `DoctorQueue : Read`, tanpa butir hak akses baru (`02` KT.7) |
| `RJ-DOC-OQ-014` | `open`, `POST-MVP` | Frontend belum punya tombol Batalkan konsultasi untuk antrean hari ini. Amendment KT menambahkannya hanya untuk konsultasi tertunda. Perluasan ke antrean hari ini diputuskan pemilik terpisah |
| `RJ-DOC-OQ-015` | `open`, di luar scope | Batal konsultasi tidak membatalkan resep draf milik konsultasi itu (perilaku lama `CancelConsultation`) |

### Approval 2026-10-05 — desain Amendment KT

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-DOC-DEC-032` | Approval | **Desain Amendment KT (revisi `29`) dan kontrak `RJ-DOC-PENDCONS-001@1.0.0` disetujui**: bagian *Amendment KT* pada `02`, `03`, `04`, `data/`, `contracts/`, `testing/`, serta `flowcharts/konsultasi-tertunda.md`. `RJ-DOC-OQ-013` tertutup. Approval ini **bukan** izin menulis code; `IMPLEMENTATION_AUTHORITY` tetap per task | Sukma Giri | `approved` | Sukma Giri, 5 Okt 2026 ("oke lanjutkan" atas "Setujui desain dan kontrak … Setuju?") | `blueprint-manifest.md` revisi `29` |
| `RJ-DOC-DEC-033` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `RJ-DOC-REV-BE-012` lalu `RJ-DOC-REV-FE-012`**, berurutan sesuai grafik dependency: penulisan source dan validasi runtime terhadap `QuilvianNewDevSukma`, termasuk data uji yang dibersihkan lewat endpoint aplikasi. Tanpa migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 5 Okt 2026 ("ok lanjutkan") | `roadmap/doctor-consultation-roadmap.md` bagian 13 |


## Amendment Pass 2026-10-06 — Pendaftaran Pasien Rawat Jalan oleh petugas (Amendment PR)

Sumber requirement: Sukma Giri, 6 Okt 2026 — *"buatkan saya fitur pendaftaran pasien Rawat Jalan,
tampilannya samakan seperti pendaftaran pasien IGD"*.

Fakta source (baseline 6 Okt 2026):

- `F-PR-1` — `POST /patient-encounters/admin` (`PatientEncounter : Create`) sudah menerima
  encounter Rawat Jalan berklinik, termasuk jadwal dokter, tanggal kunjungan, dan tiga jenis
  pembayaran. Backend tidak perlu diubah.
- `F-PR-2` — `GET /clinics/admin/options` (`Clinic : Read`) dan `GET /doctor-schedules/admin/options`
  (`DoctorSchedule : Read`) menyediakan pilihan klinik dan jadwal dokter untuk petugas.
- `F-PR-3` — Frontend belum punya layar pendaftaran Rawat Jalan untuk petugas; yang ada hanya kiosk.

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at |
|---|---|---|---|---|---|
| `RJ-DOC-DEC-034` | UI | Layar Pendaftaran Pasien Rawat Jalan mengikuti tampilan dan alur Pendaftaran Pasien IGD: pilih jenis pasien, Cari/Input Pasien, Data Kunjungan, Metode Pembayaran, Verifikasi, Selesai. Komponen IGD dipakai ulang | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-035` | Bisnis | Pemilihan jadwal dokter wajib bila klinik bertanda `IsDoctorRequired`; selain itu boleh kosong | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-036` | Bisnis | Jenis kunjungan sama dengan IGD: Umum (`VisitType.NewVisit`) dan Rujukan/Tindak Lanjut (`VisitType.FollowUp`) | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-037` | Bisnis | Petugas boleh mendaftarkan untuk hari ini dan tanggal mendatang. Hari ini = walk-in; tanggal mendatang = appointment. Jadwal dokter yang tampil mengikuti tanggal terpilih | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-038` | Approval | `IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `RJ-DOC-REV-FE-013` saja: source frontend, laporan task, dan tanda status. Tanpa perubahan backend, migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-039` | UI | Revisi layar Pendaftaran Rawat Jalan: (a) langkah Data Kunjungan hanya menawarkan poliklinik yang punya jadwal dokter berlaku pada tanggal kunjungan; (b) label poliklinik tanpa kode; (c) tombol tanpa ikon; (d) tombol kembali bergaris warna primary. Butir (c) dan (d) **hanya** untuk Rawat Jalan; Pendaftaran IGD tidak berubah. Dikerjakan di bawah wewenang `RJ-DOC-DEC-038` | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |

## Amendment Pass 2026-10-06 — Scan kartu penjamin pada Pendaftaran Rawat Jalan (Amendment SK)

Sumber requirement: Sukma Giri, 6 Okt 2026 — tombol scan kartu polis/asuransi pada tabel
*Daftar Asuransi / Penjamin Pasien* dengan konsep seperti scan KTP/KIA/SIM di kiosk; foto hasil
scan disimpan seperti foto scan kiosk sehingga kartu yang sudah pernah dipindai cukup ditampilkan
preview-nya; field scan kartu juga pada modal *Daftarkan Penjamin Baru*.

Fakta source (baseline 6 Okt 2026):

- `F-SK-1` — `MstPatientInsurance.CardImagePath` (`varchar(500)`) sudah ada dan sudah ikut di
  response list/detail `patient-insurances`, tetapi hanya diisi sebagai teks path.
- `F-SK-2` — `MstPatientCompanyGuarantor` tidak punya kolom gambar kartu; `GuaranteeDocumentPath`
  adalah dokumen surat jaminan, bukan kartu.
- `F-SK-3` — Foto scan identitas kiosk dikirim sebagai base64 (`PhotoBase64`), lalu
  `PatientController` menyimpannya sebagai file di storage `FileStorage` (`/uploads/patient-photos/...`)
  dan mencatat path publiknya di `MstPatient.PhotoPath`.
- `F-SK-4` — Plustek Scanner Agent menyediakan `POST /scanner/scan` (gambar tanpa OCR). OCR agent
  hanya mengenali dokumen identitas, sehingga nomor kartu penjamin tidak dibaca otomatis.

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at |
|---|---|---|---|---|---|
| `RJ-DOC-DEC-040` | UI | Tabel *Daftar Asuransi / Penjamin Pasien* mendapat kolom *Kartu* tepat sebelum kolom Status: tombol *Scan Kartu* bila penjamin belum punya gambar kartu, tombol *Lihat Kartu* bila sudah. Panel *Penjamin Dipilih* menampilkan preview kartu tersimpan. Modal *Daftarkan Penjamin Baru* mendapat field scan kartu yang ikut tersimpan saat penjamin dibuat. Berlaku untuk Pendaftaran Rawat Jalan; Pendaftaran IGD tidak berubah | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-041` | Bisnis | Foto hasil scan kartu disimpan permanen dengan pola foto scan kiosk (`F-SK-3`): base64 dikirim ke backend, backend menulis file di storage `FileStorage` dan mencatat path publiknya pada penjamin pasien. Kartu yang sudah tersimpan cukup ditampilkan, tanpa scan ulang | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-042` | Data | Cakupan: asuransi pasien (memakai `MstPatientInsurance.CardImagePath`, tanpa migration) dan penjamin perusahaan pasien (kolom baru `MstPatientCompanyGuarantor.CardImagePath varchar(500) null`, butuh migration) | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-043` | Approval | `IMPLEMENTATION_AUTHORITY` `GRANTED` dalam `CROSS-REPO MODE` (backend lalu frontend) untuk `RJ-DOC-REV-BE-013`, `RJ-DOC-REV-BE-014`, dan `RJ-DOC-REV-FE-014`: source, laporan task, dan tanda status. Pembuatan dan eksekusi migration `BE-014` tetap butuh izin terpisah. Tanpa commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |
| `RJ-DOC-DEC-044` | Approval | Izin pembuatan migration `RJ-DOC-REV-BE-014` (hanya kolom `MstPatientCompanyGuarantor.CardImagePath varchar(500) null`) dan eksekusinya ke database uji `QuilvianNewDevSukma`. Tidak berlaku untuk database lain | Sukma Giri | `approved` | Sukma Giri, 6 Okt 2026 |

## Amendment Pass 2026-10-07 — Menu Konsultasi Tertunda (Amendment MT)

Sumber requirement: Sukma Giri, 7 Okt 2026. Kunjungan tertunda dipindahkan dari Klinis Dokter ke
menu baru di bawah Dokter → Rawat Jalan. Aksi baris: *Batalkan Konsultasi* dan *Simpan
Konsultasi*. *Simpan Konsultasi* mengarahkan ke Klinis Dokter untuk meninjau ulang hasil terakhir
konsultasi yang tersimpan. Konsepnya sama dengan *Batalkan Kunjungan* di Daftar Pasien Rawat Jalan.

Mode: **amendment pass** atas Amendment KT (revisi `29`). Keputusan bisnis `RJ-DOC-DEC-029`..`031`
tetap berlaku tanpa perubahan. Yang berubah hanya **letak dan alur layar**: `RJ-DOC-FE-010` dan
`RJ-DOC-FE-013` digantikan.

Baseline: backend `85962dc4` (`sukmagp`), frontend `9acc42027` (`sukmagpV2`). Tidak ada capability
map baru; fakta di bawah dibaca langsung dari source.

### Fakta dari source

- `F-MT-1` — `GET /v1/health-services/registration-management/doctor-queues/pending-consultations`
  sudah menerima `doctorId`, `queueId`, `search`, `pageNumber`, `pageSize`. Responsnya memuat
  `pendingDays`, `draftPrescriptionCount`, `procedureCount`, `consultationId`, dan
  `canCancelConsultation` (`DoctorQueueController.cs:211`, kontrak `RJ-DOC-PENDCONS-001@1.0.0`).
- `F-MT-2` — Pembatalan konsultasi memakai `cancelDoctorConsultation(consultationId, { cancelReason })`
  dengan izin `DoctorConsultation : Cancel` (`useDoctorPendingConsultations.js`).
- `F-MT-3` — Sidebar mendukung menu bertingkat (`left-sidebar-menu-handle.jsx`,
  `getNestedMenuFromSubItem`). Saat ini Dokter → Rawat Jalan adalah satu tautan langsung ke
  `/health-services/registration-management/doctor-queues`.
- `F-MT-4` — Pola *Batalkan Kunjungan*: `DataTable` dengan kolom Aksi `RowActionMenu`, lalu
  `ConfirmModal` dengan `requireReason` (maks. 250 karakter) (`outpatient-encounter-list-view.jsx`).
- `F-MT-5` — Petunjuk backend di Daftar Pasien Rawat Jalan (`OutpatientEncounterListService.cs:384`)
  menyebut "Konsultasi tertunda". Petunjuk itu tetap benar sesudah amendment ini.

**Kesimpulan fakta:** backend tidak perlu berubah. Amendment ini **frontend-only**, tanpa migration.

### Batas scope amendment

**Satu kalimat:** konsultasi tertunda dikelola dari halaman daftar tersendiri di menu Dokter →
Rawat Jalan, dan Klinis Dokter hanya dipakai untuk meninjau dan menyimpannya.

| Di dalam scope | Di luar scope — pemiliknya |
|---|---|
| Halaman baru *Konsultasi Tertunda* (daftar, pencarian, aksi baris) | Aturan isi daftar dan cakupan dokter — tetap `RJ-DOC-DEC-029`/`030` |
| Aksi *Batalkan Konsultasi* dengan alasan wajib | Aturan finalisasi dan pembatalan — tetap `RJ-DOC-DEC-031` |
| Aksi *Simpan Konsultasi*: membuka Klinis Dokter pada konsultasi tersebut | Pembatalan **kunjungan** — tetap di Daftar Pasien Rawat Jalan (`RJ-DOC-DEC-021`) |
| Menghapus tab *Hari ini/Tertunda* dari panel kiri Klinis Dokter | Perubahan endpoint atau backend |
| Pengingat jumlah tertunda di Klinis Dokter | IGD dan Rawat Inap |

### Keputusan

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-DOC-DEC-045` | Decision | **Bentuk blueprint `SINGLE`**, masuk roadmap doctor-consultation sebagai task revisi `RJ-DOC-REV-FE-*`. `shape_decided_by = USER_CONFIRMED` (mewarisi `RJ-DOC-DEC-028`). Hasil uji: 0 dari 5 syarat pemecahan. Tidak ada kemampuan tanpa rumpun | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 | Jawaban sesi |
| `RJ-DOC-DEC-046` | Decision | **Simpan Konsultasi = tinjau dulu, lalu simpan di Klinis Dokter.** Dari daftar, dokter diarahkan ke Klinis Dokter dengan konsultasi itu langsung terbuka di workspace. Workspace menampilkan seluruh tab klinis dan banner kunjungan lampau (`RJ-DOC-FE-011` a). Modal Simpan **tidak** terbuka otomatis. Dokter menekan Selesaikan sendiri, dan konfirmasi kunjungan lampau (`RJ-DOC-FE-011` b) tetap berlaku. **Contoh:** dr. Arif memilih *Simpan Konsultasi* pada IKBAL (30 Sep). Klinis Dokter terbuka dengan SOAP IKBAL. dr. Arif memeriksa resep, lalu menekan Selesaikan | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 | Jawaban sesi |
| `RJ-DOC-DEC-047` | Decision | **Batalkan Konsultasi tersedia di dua tempat:** baris daftar (utama) dan workspace Klinis Dokter saat konsultasi tertunda dibuka lewat *Simpan Konsultasi*. Keduanya memakai alasan wajib dan endpoint yang sama. Tanpa izin `DoctorConsultation : Cancel`, aksi tidak tampil | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 | Jawaban sesi; `F-MT-2` |
| `RJ-DOC-DEC-048` | Decision | **Sesudah finalisasi berhasil** pada konsultasi yang dibuka dari daftar tertunda, dokter dikembalikan ke halaman Konsultasi Tertunda dengan pesan sukses. Bila finalisasi gagal, dokter tetap di Klinis Dokter dengan pesan galatnya. Sesudah pembatalan dari workspace, dokter juga dikembalikan ke halaman Konsultasi Tertunda | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 | Jawaban sesi |
| `RJ-DOC-DEC-049` | Assumption | "Batalkan/nonaktifkan konsultasi" berarti pembatalan konsultasi yang sudah ada (`RJ-DOC-DEC-031`). Kunjungan tetap status 6 dan petugas membatalkannya di Daftar Pasien Rawat Jalan. Tidak ada status "nonaktif" baru | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 | `F-MT-2` |

### Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `RJ-DOC-FE-010` | Letak daftar | Sukma Giri | **`superseded`** oleh `RJ-DOC-FE-014` (7 Okt 2026) | — | — |
| `RJ-DOC-FE-013` | Bentuk daftar tertunda | Sukma Giri | **`superseded`** oleh `RJ-DOC-FE-014` (7 Okt 2026) | — | — |
| `RJ-DOC-FE-014` | Menu dan letak | Sukma Giri | `approved` | Sidebar: **Dokter → Rawat Jalan** menjadi grup berisi **Klinis Dokter** (rute lama) dan **Konsultasi Tertunda** (rute baru). Panel kiri Klinis Dokter kembali hanya berisi antrean hari ini, tanpa tab | Jawaban sesi |
| `RJ-DOC-FE-015` | Pengingat di Klinis Dokter | Sukma Giri | `approved` | Bila jumlah tertunda > 0, tampil info singkat dengan tautan ke Konsultasi Tertunda, contoh: "Ada 8 konsultasi tertunda dari hari sebelumnya." Bila jumlahnya 0 atau gagal dimuat, info tidak tampil dan antrean hari ini tidak terganggu. Letak: `DEV_DISCRETION` | Jawaban sesi |
| `RJ-DOC-FE-016` | Isi halaman daftar | Developer | `DEV_DISCRETION` | Mengikuti pola Daftar Pasien Rawat Jalan (`F-MT-4`). Kolom: Tanggal, Tertunda (hari), No. Kunjungan, Pasien/No. RM, Klinik, Dokter, Resep draf/Tindakan, dan Aksi. Filter: pencarian dan jumlah baris, plus filter dokter bila pengguna boleh melihat semua dokter. Tanpa summary card. Nama rute dan cara Klinis Dokter menerima konsultasi yang dibuka (mis. `?queueId=`) juga `DEV_DISCRETION` | Jawaban sesi |

### Acceptance criteria yang sudah dapat diuji

1. Sidebar menampilkan Dokter → Rawat Jalan → Klinis Dokter dan Konsultasi Tertunda. Klinis Dokter
   membuka rute lama, dan perilaku antrean hari ini tidak berubah.
2. Panel kiri Klinis Dokter tidak lagi punya tab Hari ini/Tertunda.
3. Konsultasi Tertunda menampilkan data dari endpoint `pending-consultations` dengan cakupan dokter
   yang sama. Pencarian dan pagination berjalan.
4. *Batalkan Konsultasi* membuka modal dengan alasan wajib (1–250 karakter). Sesudah berhasil, baris
   hilang dan muncul pesan agar petugas membatalkan kunjungannya di Daftar Pasien Rawat Jalan.
5. Tanpa izin `DoctorConsultation : Cancel`, aksi *Batalkan Konsultasi* tidak tampil di daftar
   maupun di workspace.
6. *Simpan Konsultasi* membuka Klinis Dokter dengan konsultasi itu terpilih. Tab klinis dapat dibaca
   dan disunting, banner kunjungan lampau tampil, dan modal Simpan **tidak** terbuka sendiri.
7. Selesaikan memunculkan konfirmasi kunjungan lampau bila ada resep draf atau tindakan. Sesudah
   berhasil, dokter kembali ke Konsultasi Tertunda dan baris itu sudah hilang.
8. Bila konsultasi yang dibuka sudah tidak tertunda (mis. sudah disimpan di tab browser lain),
   Klinis Dokter menampilkan pesan bahwa konsultasi tidak ditemukan, bukan workspace kosong.
9. Pengingat jumlah tertunda tampil hanya bila jumlahnya > 0, dan tautannya membuka Konsultasi Tertunda.

### Open question dan blocker

Nihil. Tidak ada blocker desain maupun implementasi.

### Hasil desain 2026-10-07 (revisi `30`, `draft`)

| ID | Status | Jawaban / isi |
|---|---|---|
| `RJ-DOC-OQ-016` | `open`, `POST-MVP`, tidak memblokir | Filter Dokter yang diusulkan pada `RJ-DOC-FE-016` **tidak dibuat** di MVP. Frontend tidak punya sinyal "boleh melihat semua dokter" untuk `pending-consultations`; metadata Daftar Pasien Rawat Jalan dijaga `OutpatientEncounter : Read` yang belum tentu dimiliki dokter. Pengganti: kolom Dokter (`03` MT-FE.9). Bila dibutuhkan, backend perlu menambah sinyal cakupan |
| Bentuk desain | — | Frontend-only: `02` *Amendment MT* menyatakan tanpa perubahan backend; kontrak `RJ-DOC-PENDCONS-001@1.0.0` tidak disunting. Satu task usulan `RJ-DOC-REV-FE-015` (`04` MT-8) |

### Approval 2026-10-07 — desain Amendment MT

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `RJ-DOC-DEC-050` | Approval | **Desain Amendment MT (revisi `30`) disetujui**: bagian *Amendment MT* pada `02`, `03`, `04`, `flowcharts/konsultasi-tertunda.md`, dan `testing/acceptance-test-matrix.md`, termasuk penundaan filter Dokter (`RJ-DOC-OQ-016`). Kontrak `RJ-DOC-PENDCONS-001@1.0.0` tidak berubah. Approval ini **bukan** izin menulis code; `IMPLEMENTATION_AUTHORITY` tetap per task | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 ("lanjutkan" atas "Setujui desain ini?") | `blueprint-manifest.md` revisi `30` |
| `RJ-DOC-DEC-051` | Approval | **`IMPLEMENTATION_AUTHORITY` `GRANTED` untuk `RJ-DOC-REV-FE-015`**: penulisan source frontend, laporan task, tanda status roadmap, dan uji layar terhadap `QuilvianNewDevSukma` dengan data uji yang dibuat/dibersihkan lewat endpoint aplikasi. Tanpa perubahan backend, migration, commit, push, merge, maupun deployment | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 ("lanjutkan" atas permintaan izin implementasi) | `roadmap/doctor-consultation-roadmap.md` bagian 16 |
| `RJ-DOC-DEC-052` | Approval | **Uji layar penuh `RJ-DOC-REV-FE-015`**: backend uji 7185 dari build Release `HEAD` di scratchpad (tanpa perubahan DB), data uji pasien `KSKTEST-RM-07` lewat endpoint aplikasi, dan `QueueDate` antrean data uji dimundurkan lewat SQL langsung di `QuilvianNewDevSukma`. Data dibersihkan sesudahnya. Tidak berlaku untuk database lain | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 (pilihan "Penuh, izinkan SQL") | [Laporan FE-015](task/report/frontend/RJ-DOC-REV-FE-015.md) §6 |
| `RJ-DOC-DEC-053` | Decision | **Cakupan konsultasi tertunda mengikuti akun dokter, termasuk akun dokter yang juga SuperAdmin** (memperjelas `RJ-DOC-DEC-029` untuk Amendment MT). Bila akun yang login tertaut ke data dokter (`doctorId` sesi), pengingat di Klinis Dokter dan daftar Konsultasi Tertunda hanya memuat konsultasi dokter itu. Akun tanpa tautan dokter (mis. admin SuperAdmin) tetap melihat semua. Bila jumlahnya 0, pengingat tidak tampil (`RJ-DOC-FE-015`). Frontend-only: memakai parameter `doctorId` yang sudah ada pada `RJ-DOC-PENDCONS-001@1.0.0`. **Contoh:** dr. Maya Permata Sari (akun SuperAdmin) melihat 2 konsultasi tertunda miliknya, bukan 8 milik 5 dokter | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 (temuan uji pemilik + pilihan "Pengingat + daftar per dokter") | Tangkapan layar pemilik 7 Okt 2026 |
| `RJ-DOC-DEC-054` | Decision | **Antrean pasien dokter hari ini di Klinis Dokter memakai cakupan yang sama dengan `RJ-DOC-DEC-053`.** Akun yang tertaut ke data dokter hanya melihat antrean hari ini miliknya (daftar, ringkasan Total Antrean, kunci panggil, dan grup realtime), termasuk bila akunnya SuperAdmin. Akun tanpa tautan dokter tetap memakai cakupan lama. Frontend-only: filter `doctorId` sudah didukung `GET /doctor-queues`, `/summary`, dan `/call-lock`. Wewenang implementasi diberikan sebagai revisi 2 `RJ-DOC-REV-FE-015` (tanpa backend, commit, push, deploy). **Contoh:** dr. Rendy Pangalila tidak lagi melihat AGNES YULIANI RAJA GUK GUK (pasien dr. Maya); pasien itu hanya tampil saat dr. Maya login | Sukma Giri | `approved` | Sukma Giri, 7 Okt 2026 ("pola yang sama juga terapkan untuk … antrean pasien dokter hari ini") | Tangkapan layar pemilik 7 Okt 2026 |
