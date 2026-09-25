# PHA-BE-004 — Resep Terlepas dari Keadaan Menunggu Pembayaran

## Ringkasan untuk Pembaca Umum

Sejak 24 Agustus 2026, resep rawat jalan macet permanen. Penyebabnya bukan kerusakan: empat
endpoint yang dulu dapat menyatakan sebuah resep sudah dibayar dihapus permanen, karena siapa
pun yang boleh mengubah resep dapat menyatakannya lunas — dan obat keluar tanpa uang masuk.
Yang tidak pernah dibangun sesudahnya adalah penggantinya. Akibatnya tidak ada satu pun jalur
kode yang memindahkan resep keluar dari keadaan "menunggu pembayaran", sehingga resep yang
sebenarnya sudah lunas di kasir tetap tidak pernah sampai ke antrean apoteker.

Task ini membangun penggantinya. Ketika kasir menyelesaikan tagihan, Billing menerbitkan surat
yang menyatakan resep itu beres secara finansial, beserta jenis kebersanya: lunas tunai,
disetujui penjamin, atau pembayarannya ditiadakan. Farmasi membaca surat itu, menyimpan
salinannya, lalu memindahkan resep ke antrean apoteker tanpa petugas menekan apa pun.

Tiga hal disengaja. Pertama, Farmasi **tidak** menghitung sendiri apakah sebuah resep sudah
dibayar — tagihan, tender, dan alokasi pembayaran tidak dibaca sama sekali; hanya suratnya yang
dibaca, dan hasilnya disalin apa adanya. Kedua, surat yang datang terlambat dan bernomor lebih
lama diabaikan tanpa suara, karena surat dapat tiba tidak berurutan dan yang lebih tua memang
harus kalah — jika tidak, resep yang izinnya sudah dicabut bisa terbaca boleh dikerjakan lagi.
Ketiga, ketika surat menyatakan boleh dikerjakan tetapi jenis kebersannya tidak dikenali,
sistem menolak melanjutkan dan mencatat sebabnya, alih-alih menebak — obat tidak keluar atas
dasar tebakan.

---

- TASK ID: PHA-BE-004
- TASK TYPE: Fitur (konsumsi surat clearance Billing, salinan keadaan finansial resep, dan pelepasan resep dari `WaitingForPayment`)
- COMPLEXITY: MEDIUM
- MODEL: Claude Opus 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/PharmacyManagement/**`, `Repositories/**`, `Program.cs` (source); `docs/module-blueprints/pharmacy/task/report/backend/**`, `roadmap/**` (laporan task)
- FILES INSPECTED:
  - `docs/module-blueprints/pharmacy/roadmap/backend-roadmap.md` (`PHA-BE-004` beserta acceptance criteria dan Definition of Done)
  - `docs/module-blueprints/pharmacy/contracts/integration-contract.md` (`PHA-INT-CLEARANCE-v1`)
  - `docs/module-blueprints/pharmacy/contracts/state-transition-matrix.md` (`PHA-STATE-CLEARANCE-v1`)
  - `docs/module-blueprints/pharmacy/contracts/validation-matrix.md` (`PHA-VAL-CLEARANCE-v1`)
  - `docs/module-blueprints/pharmacy/contracts/api-contract.md` (`PHA-API-CLEARANCE-v1` — nol endpoint baru)
  - `docs/module-blueprints/pharmacy/data/data-dictionary.md` (rancangan kolom `PhmPrescriptionFinancialProjection`)
  - `docs/module-blueprints/pharmacy/flowcharts/pelepasan-dan-penahanan-resep.md`
  - `docs/module-blueprints/pharmacy/testing/acceptance-test-matrix.md` (`PHA-AT-CLR-01`, `02`, `03`, `11`)
  - `docs/module-blueprints/pharmacy/blueprint-manifest.md` (revisi 4, status approved)
  - `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` (status prasyarat `BE-BKC-067`, `BE-BKC-068`)
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilPrescriptionClearanceHandoff.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingConsumerHandoffDtos.cs`
  - `Areas/HealthServices/PharmacyManagement/Models/PhmPrescription.cs`
  - `Areas/HealthServices/PharmacyManagement/Enums/PrescriptionFulfillmentStatus.cs`, `PrescriptionPaymentStatus.cs`
  - `Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkflowService.cs`, `PrescriptionReviewService.cs`, `PrescriptionDispensingService.cs`
  - `Repositories/Configurations/HealthServices/PharmacyManagement/DrugUsageConfigurations.cs` (pola configuration terdekat)
- FILES CHANGED:
  - **Dibuat**: `Areas/HealthServices/PharmacyManagement/Models/PhmPrescriptionFinancialProjection.cs` — tabel salinan beserta konstanta `PrescriptionClearanceProjectionStatuses` dan `PrescriptionClearanceSyncStates`
  - **Dibuat**: `Repositories/Configurations/HealthServices/PharmacyManagement/PrescriptionFinancialProjectionConfiguration.cs` — configuration EF Core beserta index uniknya
  - **Dibuat**: `Areas/HealthServices/PharmacyManagement/Services/PrescriptionFinancialClearanceService.cs` — service konsumsi
  - **Diperbarui**: `Repositories/ApplicationDbContext.cs` — satu `DbSet`
  - **Diperbarui**: `Program.cs` — satu registrasi `AddScoped`
- IMPLEMENTATION:
  1. **Tabel salinan `PhmPrescriptionFinancialProjection`.** Enam belas kolom persis seperti
     `data/data-dictionary.md`, ditambah kolom audit warisan `IdentityModel`. Index **unik** pada
     `PrescriptionId` menjamin tepat satu baris per resep. `InvoiceId` dan `SourceHandoffId`
     sengaja **bukan** foreign key: keduanya menunjuk baris milik Billing, dan mengunci skema
     lintas modul justru yang dihindari rancangan surat ini. Relasi ke resep memakai
     `DeleteBehavior.Restrict`. `RowVersion` dipasang sebagai concurrency token.
  2. **Service konsumsi `PrescriptionFinancialClearanceService`.** Membaca baris
     `BilPrescriptionClearanceHandoff` secara `AsNoTracking`, diurutkan menaik menurut
     `FinancialVersion` supaya urutan penerapannya benar ketika beberapa surat untuk satu resep
     terbaca dalam satu sapuan. Dua permukaan: `ConsumeForPrescriptionAsync` untuk satu resep dan
     `ConsumePendingAsync` untuk sapuan. Keduanya dipanggil **di dalam proses saat dibutuhkan**;
     `PHA-API-CLEARANCE-v1` menolak tombol sinkronisasi manual.
  3. **Penjagaan nomor versi.** Surat pertama diterima apa pun nomor versinya; sesudahnya hanya
     nomor versi yang **lebih tinggi** yang mengubah salinan. Surat bernomor lebih rendah
     diabaikan tanpa galat, tanpa percobaan ulang, dan tanpa ditampilkan ke petugas
     (`PHA_CLR_STALE_VERSION`).
  4. **Pemindahan keadaan resep.** Surat `CLEARED` memindahkan resep dari `WaitingForPayment` ke
     `QueuedAtPharmacy` — transisi yang selama ini tidak punya jalur kode. Resep yang sudah
     bergerak lebih jauh **dibiarkan pada keadaannya**, sehingga surat pemulihan tidak mengulang
     antrean, telaah, maupun penyiapan yang sudah selesai (`PHA-DEC-069`).
  5. **Salinan kenyamanan pada kolom pembayaran.** Pemetaan satu-satu tanpa penafsiran:
     `PAID` → `Paid`, `INSURANCE_APPROVED` → `InsuranceApproved`, `PAYMENT_WAIVED` →
     `PaymentWaived`. Hasil penjaminan karena itu tercatat sebagai disetujui penjamin, **bukan**
     lunas tunai (`PHA-DEC-065`).
  6. **Fail-closed atas hasil yang tidak dikenali.** Surat `CLEARED` yang hasil finansialnya
     bukan salah satu dari ketiga nilai itu dicatat apa adanya pada salinan, tetapi **tidak**
     dijadikan izin: keadaan pemenuhan tidak berpindah, kolom pembayaran tidak ditulis,
     `SyncState` menjadi `FAILED` dan sebabnya dicatat tanpa satu pun data klinis. Farmasi tidak
     menebak hasil yang dimaksud (`PHA-DEC-067`).
  7. **Konkurensi.** Penerapan satu surat diulang **satu kali** bila salinannya berubah di tengah
     jalan: perubahan yang kalah dibuang, salinan dibaca ulang, lalu keputusan diambil kembali
     atas nomor versinya. Hasilnya tepat satu baris dengan versi tertinggi yang menang
     (`PHA-AT-CLR-11`).
  8. **Pencabutan.** Surat `REVOKED` hanya memperbarui salinan. Keadaan pemenuhan sengaja
     dibiarkan apa adanya — tidak ditarik mundur, racikan tidak dikembalikan menjadi bahan.
     Penahanan pekerjaannya adalah gerbang milik `PHA-BE-005`, bukan task ini.
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `PharmacyManagement` · Prefix: `Phm` · Registry: `ACTIVE`
  - Keberlakuan: `NEW CODE` untuk ketiga berkas baru; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs` dan `Program.cs`.
  - Penamaan tabel baru memakai prefix modul (`PhmPrescriptionFinancialProjection`), bukan `Trx*` (`QBE-NAM-001`).
- BLUEPRINT STATUS/EVIDENCE: `PHA-BP-001` revisi 4 · `approved`. Gate lolos: task ber-ID, keputusan `PHA-DEC-063`–`065` dan `PHA-DES-001`/`002`/`004` approved, contract version approved, dependency dan acceptance criteria tercatat, rencana verifikasi `PHA-AT-CLR-01`/`02`/`03`/`11` ada.
  - **Catatan staleness**: `backend_source_sha` manifest (`6782ae65`) adalah leluhur `HEAD` (`3750455f`) — drift maju, wajar. Impact scan read-only atas berkas yang disentuh task ini: `PhmPrescription.cs` dan `PrescriptionWorkflowService.cs` **tidak berubah** sejak SHA manifest; satu-satunya perubahan pada berkas terdampak adalah `PrescriptionFulfillmentStatus.cs`, yaitu penambahan `AwaitingFinalCheck = 12` oleh pemilik modul sendiri, yang tidak menggeser nilai 1–11 dan tidak menyentuh `WaitingForPayment = 2` maupun `QueuedAtPharmacy = 4`.
- API CONTRACT IMPACT: **Nol endpoint baru**, sesuai `PHA-API-CLEARANCE-v1`. Nol response yang berubah — penambahan field pada layar kerja adalah `PHA-BE-006`.
- DATABASE IMPACT: Satu tabel baru pada model EF Core. **Migration belum dibuat dan belum dijalankan** — `PHA-DEC-071` menyetujui desainnya, bukan eksekusinya, dan roadmap mewajibkan wewenang terpisah untuk `AddPrescriptionFinancialProjection`. **Nol baris tabel Billing tersentuh**: seluruh akses ke `BilPrescriptionClearanceHandoff` adalah `AsNoTracking`.
- SECURITY IMPACT: Menutup jalur yang memungkinkan siapa pun di Farmasi menyatakan resep lunas. Nol permukaan override, termasuk bagi Kepala Farmasi dan Supervisor (`PHA-DEC-067`). Kolom `ErrorMessage` sengaja tidak memuat data klinis.
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `dotnet build -p:SkipMigrationMetadata=true` | `1 Error` — **bukan** dari task ini | PARTIALLY VERIFIED | Satu-satunya error: `Program.cs(1403,19): CS0103 'LabDummyDataSeeder' does not exist`. Sudah ada pada `origin/Ikbal` sebelum task ini (`git show origin/Ikbal:Program.cs` memuat pemanggilan yang sama; berkas seedernya dihapus commit `0bc921b0`). Nol error pada berkas task ini setelah `IsActive` yang tidak dimiliki `IdentityModel` dibuang. |
  | QBE Conformance | **Belum dijalankan** | NOT VERIFIED | Mode `GitRange` yang dipakai CI membutuhkan commit, dan commit belum diizinkan pemilik. Mode `ExplicitFiles` **tidak dipakai** karena melewati aturan yang bergerbang `$isNew` sehingga hasilnya menyesatkan. |
  | `PHA-AT-CLR-01` (resep terlepas ke antrean) | Terpenuhi pada source | VERIFIED (Logic) | `CLEARED` + hasil dikenali → `PaymentStatus` disalin, dan `WaitingForPayment` → `QueuedAtPharmacy`. `PrescriptionReviewService.StartAsync` menerima `ReadyForPharmacy` **atau** `QueuedAtPharmacy`, sehingga telaah tetap dapat dimulai dari keadaan baru ini. |
  | `PHA-AT-CLR-02` (surat basi) | Terpenuhi pada source | VERIFIED (Logic) | `letter.FinancialVersion <= projection.FinancialVersion` → `IgnoredStaleVersion++`, salinan tidak disentuh, tidak ada galat, tidak ada percobaan ulang. |
  | `PHA-AT-CLR-03` (hasil penjaminan) | Terpenuhi pada source | VERIFIED (Logic) | `INSURANCE_APPROVED` → `PrescriptionPaymentStatus.InsuranceApproved`. Gerbang penyerahan yang sudah ada (`PrescriptionDispensingService`) memang meloloskan ketiga keadaan itu, sehingga tidak ada gerbang baru yang perlu ditulis. |
  | `PHA-AT-CLR-11` (dua surat bersamaan) | Terpenuhi pada source | VERIFIED (Logic) | Index unik pada `PrescriptionId` + `RowVersion` concurrency token + satu kali baca-ulang. Baris tetap satu; versi tertinggi menang. |
  | Runtime — resep benar-benar muncul di antrean | **Belum** | NOT VERIFIED | Terhalang dua hal: migration belum dijalankan (wewenang terpisah) dan build masih gagal karena `LabDummyDataSeeder`. |
  | Review diff dan scope | Selesai | VERIFIED | Lima berkas; nol endpoint; nol kolom baru pada `PhmPrescription`; nol penulisan ke tabel Billing; nol perubahan pada modul lain. |
- WARNINGS:
  - **Build cabang `Ikbal` sudah gagal sebelum task ini.** `Program.cs:1403` memanggil `LabDummyDataSeeder` yang berkasnya sudah dihapus. Milik modul Lab, di luar batas task ini, dan tidak saya ubah. Selama ini belum dibereskan, verifikasi runtime task apa pun pada cabang ini tidak dapat dijalankan.
- KNOWN ISSUES / OPEN QUESTION:
  - **Kolom pembayaran saat pencabutan — TERJAWAB 23 September 2026.** Keputusan pemilik modul: `PaymentStatus` pada resep adalah **histori pembayaran**, dan sengaja tidak ditulis ulang ketika izin dicabut; keadaan finansial (`FinancialClearance`) adalah **sumber keputusan** apakah Farmasi boleh melanjutkan proses. Keduanya boleh berbeda bunyi, dan perbedaan itu disengaja. Perilaku yang sudah berjalan dipertahankan apa adanya. Keselamatan tidak bergantung pada kolom itu karena seluruh gerbang membaca salinan finansial. Bukti runtime: [validasi PHA-BE-005/006](../../../../../testing/PHA-BE-005-PHA-BE-006-runtime-validation.md).
  - **Protokol pengakuan surat belum dikonsumsi.** `PHA-INT-CLEARANCE-v1` baris kedua menyebut Farmasi mengabari Billing bahwa surat sudah diproses, dan Billing menyediakan `AcknowledgeHandoffAsync`. Memanggilnya akan menulis baris tabel Billing, dan Definition of Done task ini mewajibkan **nol baris tabel Billing tersentuh**. Karena itu pengakuannya tidak dipasang di sini; konsumsinya dibuat idempoten lewat penjagaan nomor versi sehingga surat yang terbaca berulang tidak mengubah apa pun. Pemasangan pengakuan perlu task tersendiri beserta izin menulisnya.
  - **Pemicu sapuan belum dipasang.** Task ini menyediakan service konsumsinya; siapa yang memanggilnya belum ditentukan roadmap. `PHA-API-CLEARANCE-v1` menutup pintu tombol manual, dan lingkup `PHA-BE-004` tidak memuat pemasangan pemicu. Kandidat yang tersisa: pemanggilan di jalur baca layar kerja (`PHA-BE-006`) atau penahanan (`PHA-BE-005`).
  - **Ambang `PENDING_VERIFICATION` dan `STALE` belum dipakai.** Kedua keadaan sudah ada pada kolomnya, tetapi perpindahan ke sana menuntut ambang percobaan ulang yang menjadi lingkup `PHA-BE-005` bersama permukaan pemeriksaan ulang.
- NEXT TASKS: `PHA-BE-005` (gerbang penahanan pada empat titik), `PHA-BE-006` (keadaan finansial terbaca pada layar kerja).
