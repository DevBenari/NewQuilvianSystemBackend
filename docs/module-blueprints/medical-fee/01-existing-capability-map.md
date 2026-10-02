# Medical Fee — Existing Capability Map

```yaml
blueprint_id: MF-BP-001
revision: 1
status: draft
scope_batas_audit: >
  Enam klaster yang relevan bagi MF-DEC-001..011 dan dua pertanyaan terbuka MF-OQ-009 dan
  MF-OQ-010: (1) jalur nilai jasa di Billing; (2) pencatatan pelaksana layanan di seluruh modul
  klinis; (3) master tarif; (4) master tenaga medis beserta tipe kepegawaian; (5) entity
  kontrak/PKS di mana pun; (6) konfirmasi ketiadaan entity perhitungan jasa.
  TIDAK mengaudit internal Billing, HR, atau modul klinis di luar titik sentuh di atas.
backend_source_sha: 09101d05
backend_branch: Yasmina
backend_working_tree: bersih kecuali folder blueprint (keluaran pass ini sendiri)
frontend_source_sha: abed49b03
frontend_sha_move_note: >
  Frontend bergerak ke 66f36b432 pada 20 September 2026, diperiksa saat Closure pass dimulai.
  TIDAK berdampak pada peta ini: keempat belas kemampuan di bawah seluruhnya backend, nol klaim
  frontend. Commit itu menambah unit test UI consistency dan tidak menyentuh satu pun area yang
  relevan bagi Medical Fee. Baseline dipertahankan pada abed49b03 karena memang tidak ada klaim
  yang perlu diverifikasi ulang.
input_decisions: docs/module-blueprints/medical-fee/00-interview-decisions.md revisi 1
reused_capability_maps:
  - docs/module-blueprints/finance-management/01-existing-capability-map.md — FIN-CAP-020 dan
    FIN-CAP-021 dikutip langsung, tidak diaudit ulang.
metode: >
  Pencarian terarah dengan rg lalu pembacaan implementasi yang relevan. Tidak ada build, tidak
  ada eksekusi test, tidak ada perubahan source.
```

## 1. Ringkasan eksekutif

1. **Modul ini benar-benar dibangun dari nol.** Tidak ada satu pun entity perhitungan jasa,
   aturan sharing, atau hasil fee di seluruh backend (`MF-CAP-001`).
2. **Basis perhitungan sudah terjawab dari source.** Billing memperlakukan bagian dokter
   sebagai komponen yang **berdiri sendiri terhadap diskon pasien** — hanya diskon yang secara
   eksplisit bertipe dokter yang menguranginya. Ini menjawab `MF-OQ-009` (`MF-CAP-002`).
3. **Pencatatan pelaksana sangat tidak merata**, dan ketidakmerataannya persis menjelaskan
   keluhan di transcript. Operasi paling siap, radiologi **nol** (`MF-CAP-005`..`008`).
4. **Temuan yang membatalkan dasar sebuah keputusan:** HR **sudah punya** manajemen kontrak
   kerja yang berjalan (`WfpContractHistory`, `MstContractType`). `MF-DEC-011` diambil dengan
   asumsi hal itu belum ada dan belum terjadwal — asumsi itu **keliru** (`MF-CAP-010`).
5. **Tipe praktik dokter sudah tercatat** (`MstDoctor.PracticeType`, `EmploymentTypeId`,
   `ContractTypeId`), sehingga tipe Full Time / Part Time / Dokter Tamu tidak perlu dibuat
   ulang (`MF-CAP-009`).

## 2. Capability evidence map

| ID | Kebutuhan | Pemilik | Bukti (`path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `MF-CAP-001` | Entity perhitungan jasa, aturan sharing, hasil fee | Medical Fee | Pencarian `DoctorServiceFee`, `MedicalFee`, folder `*MedicalFee*` — **nol hasil** | `Missing` | Seluruhnya dibangun dari nol | Dikonfirmasi ulang; sejalan `FIN-CAP-021` |
| `MF-CAP-002` | Basis nilai jasa per baris tagihan | Billing | `Billing/Models/BilInvoiceItem.cs#DoctorShare@09101d05`; `BillingInvoiceService.cs:1855` mengisinya dari `request.DoctorShare`; `:1880` hanya memvalidasi `DoctorShare <= Quantity * UnitPrice` | `Extend` | Nilainya **diketik petugas**, bukan hasil aturan — inilah yang digantikan `MF-DEC-001` | Menjawab sebagian `MF-OQ-009` |
| `MF-CAP-003` | Perlakuan diskon terhadap bagian dokter | Billing | `BillingCalculationService.cs:789-796` — diskon bertipe `DOCTOR` memakai `basis = item.DoctorShare` dan ditolak bila melebihi; diskon `PROMO_ITEM` memakai `basis = gross − itemDiscount` dan **tidak menyentuh** `DoctorShare` | `Ready to reuse` | — | **Menjawab `MF-OQ-009`** — lihat bagian 4 |
| `MF-CAP-004` | Nilai jasa yang mengalir ke Finance hari ini | Billing | `BillingArApHandoffService.cs:97-105` — `apAmount = Σ DoctorShare − Σ diskon dokter disetujui`, dokternya dari `RegPatientEncounter.DoctorId` | `Extend` | Satu dokter per kunjungan; tidak mengenal tim | Digantikan jalur baru lewat `MF-DEC-001` dan `MF-DEC-003` |
| `MF-CAP-005` | Pencatatan pelaksana — **operasi** | Operating Room | `OperatingRoomManagement/Models/OprTeamMember.cs` — `WorkforceId`, `Role`, `IsLead`, `IsCurrent`; enum `OprTeamRole` = `PrimarySurgeon`, `AssistantSurgeon`, `Anesthesiologist`, `ScrubNurse`, `CirculatingNurse`, `Other` | `Ready to reuse` | — | **Paling siap.** Sudah mendukung tim dan peran, termasuk perawat |
| `MF-CAP-006` | Pencatatan pelaksana — **tindakan klinis** | Clinical | `ClinicalManagement/Models/TrxPatientProcedure.cs` — `DoctorId` (wajib), `PerformedByUserId` (opsional, pengguna sistem bukan dokter) | `Extend` | Hanya **satu** dokter; tidak ada peran dan tidak ada tim | Perlu diperluas untuk `MF-DEC-003` |
| `MF-CAP-007` | Pencatatan pelaksana — **laboratorium** | Laboratory | `LaboratoryManagement/Models/LabOrder.cs#ExaminerDoctorId@09101d05` — nullable | `Extend` | Hanya **satu** dokter, dan **boleh kosong** | Salah satu kelompok yang transcript sebut masih Excel |
| `MF-CAP-008` | Pencatatan pelaksana — **radiologi** | Radiology | Pencarian field pelaksana pada seluruh `RadiologyManagement/Models/*.cs` (`RadOrder`, `RadStudy`, `RadReport`, `RadReportVersion`) — **nol hasil** | `Missing` | **Tidak ada sama sekali** siapa yang mengerjakan atau membaca hasil | **Risiko tertinggi.** Radiologi disebut transcript sebagai kelompok yang pembagiannya manual — dan datanya memang tidak ada |
| `MF-CAP-009` | Tipe praktik dan kepegawaian tenaga medis | HR / MasterData | `MstDoctor` — `PracticeType` (bawaan `FullTime`), `WorkforceTypeId`, `EmploymentTypeId`, `EmploymentStatusId`, `ContractTypeId`, `ContractStartDate` | `Ready to reuse` | — | Tipe Full Time / Part Time / Dokter Tamu **tidak perlu dibuat ulang** |
| `MF-CAP-010` | Manajemen kontrak kerja | HR / WorkforceCore | `WorkforceCore/Models/WfpContractHistory.cs` — `WorkforceProfileId`, `ContractTypeId`, `EmploymentTypeId`, `ContractNumber`, `ContractStatus`, `StartDate`, `EndDate`, `SignedDate`, `IsCurrent`, `RenewalSequence`, `DocumentPath`; ditambah `MstContractType` dan `TrxContractNonRenewal` | `Reuse with adapter` | Ini kontrak **kerja**, bukan kesepakatan **tarif sharing**. Sisi tarifnya belum ada | **Membatalkan dasar `MF-DEC-011`** — lihat bagian 5 |
| `MF-CAP-011` | Master tarif sebagai basis perhitungan | Health Services / MasterData | `MasterData/Models/MstTariff.cs` — `NormalPrice`, `IsNeedDoctor`, `EffectiveStartDate`/`EndDate`, `IsConsultationFee`, `IsSurgeryRelated`; ditambah `MstTariffCategory` | `Ready to reuse` | — | `IsNeedDoctor` berguna menyaring layanan yang memang berjasa dokter |
| `MF-CAP-012` | Jembatan dari baris tagihan ke sumber layanan | Billing | `BilInvoiceItem.SourceDomain` + `SourceDetailId`; `BillingChargeSourceAdapter` mengenal `PROCEDURE`, `LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSUMABLE`, `ADHOC`, `ADHOC_CATALOG` | `Ready to reuse` | — | Jalur inilah yang memungkinkan Medical Fee menemukan pelaksana dari baris tagihan |
| `MF-CAP-013` | Persetujuan diskon oleh dokter | Billing | `DiscountPolicyValues` memuat `DoctorApprover`; `BilDiscountApplication` punya `ApprovalStatus` | `Ready to reuse` | — | Sudah ada di Billing; **di luar scope** modul ini (`MF-OOS-001`) |
| `MF-CAP-014` | Aturan kelayakan layanan dokter | Health Services / MasterData | Dikutip `FIN-CAP-020` | `Conflict` | Namanya `MstDoctorServiceRule` tetapi isinya bukan aturan perhitungan | Nama itu **MUST NOT** dipakai ulang modul ini |

## 3. Kontrak as-is — jalur nilai jasa di Billing

Urutan yang benar-benar berjalan hari ini, dibaca langsung dari source:

1. Baris tagihan dibuat dengan `Quantity` dan `UnitPrice`. Nilai kotornya `Quantity × UnitPrice`.
2. Petugas mengetik `DoctorShare` untuk baris itu. Satu-satunya batas: tidak boleh melebihi
   nilai kotor baris.
3. Diskon bertipe `DOCTOR` mengurangi `DoctorShare`, dan ditolak bila melebihinya.
4. Diskon bertipe `PROMO_ITEM` mengurangi nilai kotor baris, dan **tidak menyentuh**
   `DoctorShare` sama sekali.
5. Saat tagihan difinalisasi, nilai yang dikirim ke Finance adalah jumlah seluruh `DoctorShare`
   dikurangi seluruh diskon dokter yang sudah disetujui.
6. Dokter yang disebut pada pengiriman itu diambil dari dokter penanggung jawab kunjungan —
   satu orang, berapa pun jumlah layanan di dalamnya.

## 4. Jawaban atas `MF-OQ-009`

Pertanyaannya: persentase dihitung dari tarif kotor, setelah diskon pasien, atau setelah bagian
rumah sakit?

**Source menjawab sebagian besar.** Sistem yang berjalan memperlakukan bagian dokter sebagai
komponen yang **berdiri sendiri terhadap diskon pasien**. Diskon promo yang diberikan ke pasien
tidak pernah mengurangi bagian dokter; hanya diskon yang sejak awal ditandai sebagai "diskon
dokter" yang menguranginya — dan diskon itu pun memerlukan persetujuan dokter yang bersangkutan.

**Contoh berangka dari perilaku nyata.** Tindakan bertarif Rp 1.000.000, bagian dokter
Rp 400.000. Pasien mendapat diskon promo Rp 100.000.

| Angka | Nilai | Keterangan |
|---|---|---|
| Nilai kotor baris | Rp 1.000.000 | `Quantity × UnitPrice` |
| Diskon promo | Rp 100.000 | Mengurangi tagihan pasien |
| Tagihan pasien | Rp 900.000 | |
| **Bagian dokter** | **Rp 400.000** | **Tidak berubah** — diskon promo tidak menyentuhnya |

Bila dokter sendiri yang memberi diskon Rp 50.000 dan menyetujuinya, barulah bagian dokter
menjadi Rp 350.000.

**Yang tersisa untuk diputuskan manusia:** apakah persentase `MF-DEC-004` dihitung dari **nilai
kotor baris** (Rp 1.000.000 pada contoh di atas) — konsisten dengan perilaku sekarang — atau
dari dasar lain. Source menunjukkan perilaku yang berlaku, tetapi tidak dapat memutuskan
kebijakannya.

## 5. Temuan yang membatalkan dasar `MF-DEC-011`

**Ini temuan terpenting pada pass ini, dan ia mengoreksi rekomendasi yang saya berikan sendiri
saat wawancara.**

`MF-DEC-011` memutuskan data PKS/SK dimiliki Medical Fee, dengan alasan yang tertulis di
decision log: *"tidak menunggu HR membangun manajemen kontrak yang belum terjadwal"*.

Alasan itu **tidak benar**. HR sudah memiliki manajemen kontrak kerja yang berjalan:

| Entity | Isi yang relevan |
|---|---|
| `WfpContractHistory` | Nomor kontrak, jenis kontrak, jenis kepegawaian, tanggal mulai dan berakhir, tanggal tanda tangan, status, penanda kontrak yang sedang berlaku, urutan perpanjangan, path dokumen |
| `MstContractType` | Master jenis kontrak |
| `TrxContractNonRenewal` | Kontrak yang tidak diperpanjang |
| `MstDoctor` | `ContractTypeId`, `ContractStartDate`, `EmploymentTypeId`, `PracticeType` |

**Yang memang belum ada** adalah sisi **tarif sharing**-nya — berapa persen yang menjadi hak
penerima untuk layanan apa. Itu bukan bagian dari kontrak kerja.

Jadi pilihannya bukan lagi "buat sendiri atau tunggu HR", melainkan:

1. Medical Fee membuat entity tarif sharing yang **menunjuk** `WfpContractHistory` yang sudah
   ada — masa berlaku, nomor kontrak, dan dokumennya dipakai ulang, Medical Fee hanya menambah
   sisi tarifnya; atau
2. Medical Fee menyimpan kontrak tarifnya sendiri secara mandiri penuh, terpisah dari kontrak
   kerja HR — dengan konsekuensi dua tempat menyebut kontrak yang sama.

Keputusan ini milik owner, bukan pass audit. Dicatat sebagai `MF-CQ-01`.

## 6. Kesiapan data pelaksana per sumber layanan

Tabel ini menjelaskan mengapa keluhan di transcript muncul tepat pada kelompok tertentu.

| Sumber | Pelaksana tercatat | Mendukung tim | Ada peran | Kesiapan untuk `MF-DEC-003` |
|---|---|:---:|:---:|---|
| Operasi | `WorkforceId` per anggota tim | Ya | Ya | **Siap** |
| Tindakan klinis | `DoctorId` tunggal | Tidak | Tidak | Perlu diperluas |
| Laboratorium | `ExaminerDoctorId`, boleh kosong | Tidak | Tidak | Perlu diperluas, dan pengisiannya perlu diwajibkan |
| Radiologi | **Tidak ada** | Tidak | Tidak | **Perlu dibangun dari nol** |
| Farmasi, bahan habis pakai | Tidak relevan | — | — | Bukan jasa tenaga medis |
| Ad-hoc kasir | Tidak ada | — | — | Perlu keputusan: apakah entri bebas kasir boleh berjasa medis |

Transcript menyebut kelompok Lab, Radiologi, Anestesi, Digestive, dan Urologi masih dihitung
manual lewat Excel. Audit ini menjelaskan sebabnya: **Lab hanya menyimpan satu dokter, dan
Radiologi tidak menyimpan siapa pun.** Anestesi justru sudah tertangani, tetapi hanya untuk
kasus yang lewat kamar operasi.

## 7. Fact, inference, dan rekomendasi

**Fact** (terbaca langsung dari source `09101d05`):

- Bagian dokter per baris tagihan diketik petugas, bukan dihitung aturan.
- Diskon promo pasien tidak mengurangi bagian dokter; hanya diskon bertipe dokter yang
  menguranginya, dan itu pun dibatasi tidak boleh melebihi bagian dokter.
- Dokter yang dikirim ke Finance diambil dari dokter penanggung jawab kunjungan, satu orang.
- Operasi menyimpan tim lengkap beserta peran; tindakan klinis dan lab hanya satu dokter;
  radiologi tidak menyimpan siapa pun.
- HR sudah memiliki `WfpContractHistory`, `MstContractType`, dan `MstDoctor.PracticeType`.
- Tidak ada satu pun entity perhitungan jasa medis.

**Inference** (kesimpulan wajar, bukan fakta):

- Karena `BilInvoiceItem` menyimpan `SourceDomain` dan `SourceDetailId`, Medical Fee
  kemungkinan besar dapat menemukan pelaksana dengan menelusuri balik ke modul sumbernya —
  tanpa perlu menyalin data pelaksana ke Billing.
- Karena radiologi tidak menyimpan pelaksana sama sekali, rumpun radiologi kemungkinan besar
  tidak dapat masuk rilis pertama Medical Fee tanpa pekerjaan di modul radiologi lebih dahulu.

**Rekomendasi** (bukan keputusan; tetap wewenang owner):

- Tinjau ulang `MF-DEC-011` dengan fakta `MF-CAP-010`.
- Saat menyusun urutan pengerjaan, dahulukan sumber layanan yang datanya sudah siap (operasi,
  tindakan klinis) dan perlakukan radiologi sebagai pekerjaan lintas modul tersendiri.

## 8. Closure questions

| ID | Pertanyaan | Owner | Memblokir |
|---|---|---|---|
| `MF-CQ-01` | `MF-DEC-011` diambil dengan alasan HR belum punya manajemen kontrak — dan alasan itu keliru (`MF-CAP-010`). Apakah keputusan itu ditinjau ulang: tarif sharing menunjuk `WfpContractHistory` yang sudah ada, atau tetap mandiri penuh? | Yasmin + HR | `DESIGN` untuk `MF-SC-001` dan `MF-SC-007` |
| `MF-CQ-02` | Radiologi tidak menyimpan pelaksana sama sekali (`MF-CAP-008`). Apakah rumpun radiologi masuk rilis pertama — yang berarti menuntut pekerjaan di modul radiologi — atau ditunda? | Yasmin + owner Radiologi | `DESIGN` — menentukan cakupan rilis pertama |
| `MF-CQ-03` | Entri ad-hoc kasir (`ADHOC`, `ADHOC_CATALOG`) tidak punya pelaksana. Apakah entri bebas kasir boleh menghasilkan jasa tenaga medis, dan bila ya siapa pelaksananya? | Yasmin | `DESIGN` untuk `MF-SC-002` |
| `MF-CQ-04` | `LabOrder.ExaminerDoctorId` boleh kosong (`MF-CAP-007`). Bila kosong saat jasa dihitung, apa yang terjadi — jasa tidak terbit, tertahan, atau jatuh ke dokter penanggung jawab kunjungan? | Yasmin | `DESIGN` — perilaku data tidak lengkap |

**Tidak ada `Conflict` baru.** Satu `Conflict` yang ada (`MF-CAP-014`) diwarisi dari
`FIN-CAP-020` dan sudah diketahui sebelum pass ini.

## 9. Staleness dan pemicu impact scan

Peta ini **stale** bila salah satu terjadi:

| Pemicu | Yang diperiksa ulang |
|---|---|
| Backend SHA bergerak dari `09101d05` | Seluruh klaim pada bagian 2, terutama `MF-CAP-002`..`008` |
| Billing mengubah perlakuan `DoctorShare` atau diskon dokter | `MF-CAP-002`, `MF-CAP-003`, `MF-CAP-004`, dan jawaban `MF-OQ-009` |
| Modul radiologi menambahkan pencatatan pelaksana | `MF-CAP-008` berubah dari `Missing`; `MF-CQ-02` dapat ditutup |
| HR mengubah `WfpContractHistory` | `MF-CAP-010` dan `MF-CQ-01` |
| Modul klinis atau laboratorium menambahkan pencatatan tim | `MF-CAP-006`, `MF-CAP-007` |

## 10. Handoff

`MF-OQ-009` sudah terjawab sejauh yang dapat dijawab source (bagian 4); sisanya keputusan
kebijakan. `MF-OQ-010` belum terjawab dan kini lebih tajam: bentuk pembagian porsi harus
mempertimbangkan bahwa hanya operasi yang datanya siap.

Empat closure question pada bagian 8 sebaiknya ditutup lewat `/grill-me` Closure pass sebelum
modul ini diteruskan ke `/design-business-module`. `MF-CQ-01` yang paling mendesak, karena ia
meninjau ulang keputusan yang sudah terlanjur diambil.
