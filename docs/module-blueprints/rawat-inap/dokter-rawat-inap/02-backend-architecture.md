# Arsitektur Backend — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — satu dari tiga sub-modul modul `rawat-inap`, bentuk `COMPOSITE` sejak `RWI-DEC-082` |
| Revision | `0.1` |
| Status | `draft` — belum disetujui manusia |
| Tanggal | 2 September 2026 (`Asia/Jakarta`) |
| Kemampuan | `CAP-015`, `CAP-020` s.d. `CAP-025` — `RWI-DEC-083` |
| Masukan baseline | `PRD-RWI-FINAL-001` v1.0.0 bagian 18, 19, 23.1, 30.3 |
| Masukan keputusan | [`../00-interview-decisions.md`](../00-interview-decisions.md) revision `7` — `RWI-DEC-080` s.d. `RWI-DEC-083`; `RWI-DEC-025` definisi visite; `RWI-DEC-038`, `RWI-DEC-070` pelonggaran mesin klinis |
| Peta modul | [`../02-module-map.md`](../02-module-map.md) revision `1` |
| Sub-modul tetangga | [`../keperawatan/`](../keperawatan/) revision `0.1` — berbagi satu pelonggaran dan satu tabel |
| Backend SHA | `5afb54bd75281648010e50ef14f43ca1f80d8efd`; audit as-is dijalankan 2026-09-02 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — batas konteks dan kepemilikan datanya sudah ditetapkan `RWI-DEC-081` dan `PRD-RWI-FINAL-001` bagian 23.1, sehingga tidak ada batas domain yang perlu diturunkan ulang |

---

## 0. Tiga kalimat yang menentukan seluruh dokumen ini

> **Pertama: sub-modul ini tidak memiliki satu tabel pun.** Sama seperti `keperawatan`.
> `RWI-DEC-081` dan PRD 23.1 menaruh SOAP, CPPT, kajian medis, tindakan, dan visite pada
> `ClinicalManagement`; resep pada batas order `ClinicalManagement`/`PharmacyManagement`; hasil
> penunjang pada modul Laboratorium dan Radiologi.

> **Kedua: hampir seluruh mesinnya sudah ada.** Dari tujuh kemampuan, **enam** punya tabel yang
> sudah berdiri dan sudah dipakai poliklinik maupun IGD. Hanya **satu** yang benar-benar kosong.

> **Ketiga: penghalangnya sama persis dengan `keperawatan`, dan ada di dua tempat.** Gerbang
> "tanpa antrean hanya untuk pasien IGD" muncul dua kali di source — pada pengkajian dan pada
> konsultasi. Membuka yang pertama saja tidak cukup bagi dokter.

---

## 1. Bounded context dan ownership

### 1.1 Kedudukan sub-modul ini

| Hal | Ketetapannya |
| --- | --- |
| Jenis konteks | **Workspace context** — menyajikan dan mengumpulkan, bukan memiliki |
| Aggregate root yang dimiliki | **Nol** |
| Yang **dibaca** | `InpEpisode`, `InpDoctorAssignment` milik `episode-rawat-inap`; seluruh tabel klinis milik `ClinicalManagement`; resep milik `PharmacyManagement`; hasil lab milik `LaboratoryManagement` |
| Transaction boundary | Tidak ada transaksi milik sub-modul ini |
| Yang benar-benar dimiliki | **Aturan kewenangan DPJP berbasis episode**, dan **definisi visite sebagai peristiwa tersendiri** |

### 1.2 Invariant

| Invariant | Bunyinya | Kenapa milik sub-modul ini |
| --- | --- | --- |
| `INV-DOK-01` | Dokumentasi dokter rawat inap hanya boleh dibuat bila ada `InpEpisode` berstatus `Admitted` untuk `EncounterId` yang sama | Episode milik modul ini; mesin klinis tidak mengenal episode |
| `INV-DOK-02` | Episode `Closed` tidak menerima dokumentasi baru. Koreksi hanya lewat mekanisme amandemen yang **tidak** mengaktifkan kembali episode | `AC-CAP020-03` menyatakannya tegas |
| `INV-DOK-03` | **Visite adalah peristiwa yang dicatat, bukan yang disimpulkan.** Satu catatan SOAP tidak otomatis menambah hitungan visite, dan satu visite tidak wajib punya SOAP | `RWI-DEC-025`, `AC-CAP025-01`, `AC-CAP025-02` |
| `INV-DOK-04` | Rawat Inap **tidak pernah** menandai obat sudah diserahkan. Status pemenuhan resep hanya dibaca dari Farmasi | PRD 19 `CAP-023` aturan 6 |
| `INV-DOK-05` | Hasil pemeriksaan penunjang **tidak pernah** disalin menjadi baris kebenaran baru di sini | PRD 18 `CAP-015` aturan 4, `AC-CAP015-02` |

`INV-DOK-03` adalah yang paling mudah dilanggar tanpa sadar. Menghitung visite dari catatan SOAP
terasa hemat, tetapi membuat dokter yang menulis dua catatan pada hari yang sama terhitung dua kali
visite, dan dokter yang benar-benar datang tanpa menulis terhitung nol.

---

## 2. Tabel kepemilikan data

> Tabel kepemilikan data **seluruh modul** ada di [`../02-module-map.md`](../02-module-map.md)
> bagian 2. Yang di bawah ini hanya kelompok data yang disentuh sub-modul ini.

### 2.1 Yang dipakai, tidak dibuat ulang

| Kelompok data | Modul pemilik | Dipakai sub-modul ini | Dibuat ulang |
| --- | --- | :---: | --- |
| Episode rawat inap, penugasan DPJP | `episode-rawat-inap` | Konteks dan kewenangan | **Tidak** |
| Kajian medis awal | **Clinical Management** | Ya — ditulis lewat endpoint modul itu | **Tidak** — `RWI-DEC-081`, PRD 23.1 |
| Konsultasi dan SOAP | **Clinical Management** | Ya | **Tidak** |
| CPPT | **Clinical Management** | Ya — **kontraknya milik sub-modul ini** (`CAP-021`) | **Tidak** |
| Diagnosis dan daftar masalah | **Clinical Management** | Ya | **Tidak** |
| Tindakan dokter | **Clinical Management** | Ya | **Tidak** |
| Resep | **Pharmacy Management** (pemenuhan) / batas order `ClinicalManagement` | Ya — dibuat; **status pemenuhannya hanya dibaca** | **Tidak** — `RWI-DEC-046`, `INV-DOK-04` |
| Pesanan dan hasil laboratorium | **Laboratory Management** | Pesanan dibuat; hasil **hanya dibaca** | **Tidak** — `INV-DOK-05` |
| Hasil radiologi | **Radiology** — modul belum ada | Belum | **Tidak** |
| Tanda vital, alergi, riwayat penyakit | **Clinical Management** | Dibaca | **Tidak** |

### 2.2 Yang **belum ada di mana pun** dan diminta kepada pemiliknya

| Kelompok data | Modul pemilik | Keadaan hari ini | Diminta oleh |
| --- | --- | --- | --- |
| Catatan visite dokter | **Clinical Management** | **Tidak ada.** Nol berkas `*DoctorVisit*` maupun `*PhysicianVisit*` di seluruh repository. `EmergencyVisit` milik IGD adalah konsep lain | `CAP-025`, bagian 4.6 |

**Satu tabel baru. Itu saja.** Enam kemampuan lain berdiri di atas tabel yang sudah ada.

### 2.3 Nol baris yang belum diputuskan

Berbeda dari `keperawatan` yang menyisakan `CAP-016`, **ketujuh kemampuan sub-modul ini punya
pemilik data yang tegas** pada PRD 23.1. Tidak ada `OPEN DECISION` kepemilikan di sini.

---

## 3. Penghalang teknis: gerbang yang sama, di dua tempat

### 3.1 Temuan

Audit 2026-09-02 menemukan gerbang "tanpa antrean hanya untuk pasien IGD" muncul **dua kali**,
dengan bentuk yang sama persis:

| Berkas | Method | Yang dijaga |
| --- | --- | --- |
| `ClinicalManagement/Controllers/PatientAssessmentController.cs` | `ValidateCreateWithoutQueueAsync` | Pengkajian — sudah dibahas `keperawatan/02-backend-architecture.md` bagian 3 |
| `ClinicalManagement/Controllers/DoctorConsultationController.cs` | validasi tanpa antrean, sekitar baris 873 | **Konsultasi** — pintu bagi SOAP, diagnosis, resep, dan tindakan |

Keduanya memeriksa keberadaan `EmgVisit` dan menolak selainnya dengan pesan
"…tanpa antrean hanya untuk pasien IGD."

### 3.2 Kenapa membuka yang pertama saja tidak cukup

Karena **empat** kemampuan sub-modul ini bergantung pada konsultasi, bukan pada pengkajian:

| Kemampuan | Bergantung pada konsultasi karena |
| --- | --- |
| `CAP-020` SOAP | Kolom `Subjective`, `Objective`, `Assessment`, `Plan` **berada di dalam** `TrxDoctorConsultation` |
| `CAP-023` Resep | `TrxPrescription.ConsultationId` **wajib**, bukan nullable |
| `CAP-024` Tindakan | `TrxPatientProcedure.ConsultationId` **wajib** |
| Diagnosis | `TrxPatientDiagnosis.ConsultationId` **wajib** |

Membuka pengkajian tanpa membuka konsultasi berarti perawat dapat mencatat sementara dokter tidak.

### 3.3 Bentuk yang diminta

| Hal | Ketetapannya |
| --- | --- |
| Pemilik perubahan | **`ClinicalManagement`** — Muhammad Hamzah, disetujui `RWI-DEC-062` |
| Bentuk | Cabang tambahan pada validasi tanpa antrean **kedua controller**: encounter diterima bila punya `InpEpisode` berstatus `Admitted` |
| Yang **tidak** berubah | Perilaku rawat jalan dan medical check-up. `RWI-DEC-070` aturan 6 menyatakannya tegas |
| Nol kolom baru untuk pelonggaran ini | Kedua `QueueId` **sudah** nullable |
| Prasyarat lintas sub-modul | Cabang pengkajian dan cabang konsultasi **wajib dikerjakan bersama**. Rincian pada `contracts/integration-contract.md` `INT-DOK-01` |

### 3.4 Pelonggaran kedua yang sudah disetujui tetapi belum dikerjakan

`RWI-RULE-026` melonggarkan **batas satu konsultasi per kunjungan** dan **satu resep aktif per
konsultasi** untuk rawat inap. Tanpa itu:

| Tanpa pelonggaran | Akibatnya bagi pasien rawat inap |
| --- | --- |
| Satu konsultasi per kunjungan | Dokter hanya dapat menulis **satu** SOAP untuk seluruh masa perawatan. `CAP-020` aturan 1 menuntut banyak |
| Satu resep aktif per konsultasi | Pasien yang dirawat sepuluh hari hanya dapat menerima satu resep |

Keduanya sudah `approved` sejak `RWI-DEC-038` dan diperluas `RWI-DEC-070`. Yang belum ada
kodenya.

---

## 4. Entity: yang ada, yang diperluas, yang diminta baru

### 4.1 `TrxDoctorConsultation` — `Diperbarui` — `CAP-020`

| Hal | Isinya |
| --- | --- |
| Status | **`Diperbarui`** |
| Pemilik | `ClinicalManagement` |
| Lokasi file | `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs` |
| Keadaan hari ini | **71 kolom**, sudah memuat `Subjective`, `Objective`, `Assessment`, `Plan`, ditambah rencana tindakan, resep, penunjang, rujukan, dan edukasi. `QueueId` **sudah** nullable |
| Penilaian | **SOAP tidak perlu tabel baru.** Ia sudah ada di dalam konsultasi |

| Kolom yang diminta | Tipe | Wajib | Bawaan | Kenapa | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `InpEpisodeId` | `Guid?` | Tidak | `null` | PRD `CAP-020` aturan 2: setiap entry menyimpan konteks episode | Tidak |
| `ClinicalDateTime` | `DateTime?` | Tidak | `null` | PRD `CAP-020` aturan 2 membedakan **waktu klinis** dari waktu penulisan. Visite pukul 07.00 yang ditulis pukul 11.00 harus terbaca sebagai pukul 07.00 | Tidak |
| `VisitId` | `Guid?` | Tidak | `null` | Tautan **opsional** ke visite. `INV-DOK-03`: SOAP tidak wajib punya visite dan sebaliknya | Tidak |

| Index diminta | Bentuk | Kenapa |
| --- | --- | --- |
| `IX_TrxDoctorConsultation_InpEpisodeId` | `(InpEpisodeId, ClinicalDateTime)` | Lini masa SOAP satu episode, terurut waktu klinis |

`DeleteBehavior` pada `InpEpisodeId`: `Restrict`.

### 4.2 `TrxPatientAssessment` — `Diperbarui` — `CAP-022` ★ **butuh persetujuan pemilik**

Kajian medis awal punya **dua** jalan yang sama-sama masuk akal. Yang dipilih di sini beserta
alasannya, dan yang ditolak beserta alasannya.

| Jalan | Isinya | Dipilih? |
| --- | --- | :---: |
| **A. Pakai ulang `TrxPatientAssessment` dengan pembeda jenis** | Kajian medis menjadi nilai baru pada `AssessmentType` yang sudah diminta `keperawatan` | **Ya** |
| B. Tabel baru `TrxMedicalAssessment` | Kajian medis punya tabelnya sendiri | Tidak |

**Kenapa A.** `TrxPatientAssessment` sudah memuat keluhan utama, riwayat penyakit sekarang,
riwayat pengobatan, alergi, tanda vital, kesadaran, dan pemeriksaan umum — **tujuh dari delapan**
bagian yang dituntut PRD `CAP-022` aturan 2. Tabel itu juga **sudah** punya kolom `DoctorId`, jadi
ia memang tidak pernah menjadi tabel milik perawat saja. Jalan B menyalin sekitar 40 kolom yang
sama, dan menyalin kolom adalah persis yang dicegah tabel kepemilikan data.

**Yang perlu diketahui pemilik sebelum menyetujui.** Jalan A membuat satu tabel dipakai dua
sub-modul. Akibat yang disadari:

| Akibat | Cara menanganinya |
| --- | --- |
| Enum `PatientAssessmentType` dipakai bersama `keperawatan` | Siapa pun yang mendarat lebih dulu membuatnya; yang kedua menambah nilainya. Dicatat sebagai prasyarat pada `contracts/integration-contract.md` `INT-DOK-04` |
| Validasi harus bercabang menurut jenis | Isian wajib kajian medis berbeda dari pengkajian keperawatan — `validation-matrix.md` bagian 3 |
| Kewenangan harus bercabang | Kajian medis hanya boleh ditulis dokter; pengkajian keperawatan hanya perawat — `VAL-DOK-05` |
| Pembaca dapat salah mengira kajian medis adalah pengkajian keperawatan | Ruang kerja **memisahkan** keduanya di layar, `03-frontend-architecture.md` |

**Bila pemilik memilih jalan B**, yang berubah hanya bagian ini dan kamus datanya; kontrak API,
kewenangan, dan alur tetap sama. Karena itu ia **tidak** dicatat sebagai `OPEN DECISION` yang
menahan gelombang — ia catatan persetujuan struktur.

| Kolom yang diminta | Tipe | Wajib | Bawaan | Kenapa |
| --- | --- | :---: | --- | --- |
| `AssessmentType` | `enum` | Ya | `Initial` | Nilai baru `MedicalInitial` dan `MedicalReassessment` ditambahkan pada enum yang sama |

Kolom `InpEpisodeId`, `DueAt`, `PolicyId`, `AmendedAt`, `AmendedByUserId` **sudah diminta**
`keperawatan`; sub-modul ini memakainya apa adanya dan **tidak meminta duplikatnya**.

> `AC-CAP022-02` menuntut kajian medis dan SOAP punya record serta lifecycle berbeda. Terpenuhi:
> kajian medis hidup di `TrxPatientAssessment`, SOAP hidup di `TrxDoctorConsultation`. Keduanya
> tabel yang berbeda dengan mesin status yang berbeda.

### 4.3 `TrxPatientIntegratedProgressNote` — `Diperbarui` — `CAP-021`

| Hal | Isinya |
| --- | --- |
| Status | **`Diperbarui`** |
| Pemilik | `ClinicalManagement` |
| Keadaan hari ini | Sudah punya `ProfessionType`, `ProviderUserId`, dan seluruh FK penghubung sudah nullable. Catatan lintas profesi **sudah** dapat masuk |
| Kepemilikan kontrak | **Sub-modul ini** — `CAP-021`, `RWI-DEC-083`. `keperawatan` menulis ke sana sebagai penulis, bukan sebagai pemilik kontrak |

| Kolom yang diminta | Tipe | Wajib | Bawaan | Kenapa | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `InpEpisodeId` | `Guid?` | Tidak | `null` | PRD `CAP-021` aturan 2: konteks episode wajib dapat ditelusuri | Tidak |
| `VerificationStatus` | `enum` | Ya | `NotRequired` | PRD aturan 4: verifikasi DPJP **bila diwajibkan** | Tidak |
| `VerifiedAt` | `DateTime?` | Tidak | `null` | Aturan 4 | Tidak |
| `VerifiedByUserId` | `Guid?` | Tidak | `null` | Aturan 5: **verifikator bukan penulis asli** | Tidak |
| `VerificationDueAt` | `DateTime?` | Tidak | `null` | Aturan 4: SLA verifikasi terpantau | Tidak |
| `AmendedAt`, `AmendedByUserId`, `AmendReason` | `DateTime?`, `Guid?`, `string?` | Tidak | `null` | Aturan 6: koreksi setelah final memakai amandemen | `AmendReason` **Ya** |

`CpptVerificationStatus`: `NotRequired`, `Pending`, `Verified`, `Overdue`. Bawaan `NotRequired`.

> **Bawaan `NotRequired`, bukan `Pending`.** PRD menulis "**bila** verifikasi DPJP diwajibkan".
> Menyalakannya sebagai bawaan akan membuat setiap catatan perawat langsung terhitung menunggu
> verifikasi pada rumah sakit yang tidak mewajibkannya, lalu daftar pantau penuh sejak hari
> pertama. Kewajibannya dinyalakan lewat kebijakan, bukan lewat nilai bawaan.

> **`VerifiedByUserId` terpisah dari penulis, dan itu menjawab `AC-CAP021-03`.** Verifikasi
> **tidak pernah** menulis ulang `ProviderUserId`.

### 4.4 `TrxPatientProcedure` — `Diperbarui` — `CAP-024`

| Hal | Isinya |
| --- | --- |
| Status | **`Diperbarui`** |
| Pemilik | `ClinicalManagement` |
| Keadaan hari ini | Sudah memuat snapshot kode dan nama tindakan, tarif, penjamin, serta penanda primer/darurat/bedah. Mewajibkan `ConsultationId` dan `DoctorId` — **cocok** untuk tindakan dokter |

| Kolom yang diminta | Tipe | Wajib | Bawaan | Kenapa |
| --- | --- | :---: | --- | --- |
| `InpEpisodeId` | `Guid?` | Tidak | `null` | PRD `CAP-024` aturan 1 |
| `ProcedureRecordType` | `enum` | Ya | `Performed` | Aturan 3: membedakan **direncanakan** dari **dilakukan** |
| `PerformedAt` | `DateTime?` | Tidak | `null` | Aturan 4 |
| `IdempotencyKey` | `string?` | Tidak | `null` | Aturan 5, `AC-CAP024-02` |
| `BillingDispatchStatus` | `enum` | Ya | `NotApplicable` | Aturan 5: commit klinis tidak boleh hilang karena kegagalan tagihan |

| Constraint | Bentuk | Kenapa |
| --- | --- | --- |
| Unique parsial `IdempotencyKey` | `WHERE IdempotencyKey IS NOT NULL AND IsDelete = false` | `AC-CAP024-02` dijaga database, bukan hanya service |

> **Enum `NursingBillingDispatchStatus` yang diminta `keperawatan` dipakai ulang di sini**, bukan
> dibuat kembar. Namanya diusulkan berubah menjadi `ClinicalBillingDispatchStatus` karena kini
> dipakai dua profesi — dicatat pada `INT-DOK-04`.

### 4.5 `TrxPrescription` — `Diperbarui` — `CAP-023`

| Hal | Isinya |
| --- | --- |
| Status | **`Diperbarui`** |
| Pemilik | **`PharmacyManagement`** — pemenuhan; batas order pada `ClinicalManagement` |
| Keadaan hari ini | Mesin resep **lengkap**: `TrxPrescription`, item, racikan, review, penyiapan, template, dan ruang kerja farmasi. Sudah punya `PrescriptionStatus`, `PaymentStatus`, dan `FulfillmentStatus` |
| Penghalangnya | `ConsultationId` **wajib** — jadi resep rawat inap hidup begitu konsultasi rawat inap hidup |

| Kolom yang diminta | Tipe | Wajib | Bawaan | Kenapa |
| --- | --- | :---: | --- | --- |
| `InpEpisodeId` | `Guid?` | Tidak | `null` | PRD `CAP-023` aturan 3 |
| `PrescriptionOrderType` | `enum` | Ya | `Routine` | Aturan 7: **obat pulang wajib menjadi jenis order yang eksplisit**, `AC-CAP023-03` |
| `IdempotencyKey` | `string?` | Tidak | `null` | Aturan 8 |

`PrescriptionOrderType`: `Routine`, `Daily`, `Discharge`. Bawaan `Routine`.

> **`Discharge` menutup `RWI-GAP-009` dan menyambung `RWI-DEC-033`.** Butir daftar periksa
> administrasi "obat pulang sudah diserahkan" pada `episode-rawat-inap` hari ini **ditandai
> manual**. Dengan jenis order eksplisit, penandaannya kelak dapat dibaca dari Farmasi — tetapi
> **itu bukan pekerjaan MVP ini** dan `INV-DOK-04` tetap melarang sub-modul ini menandai sendiri.

### 4.6 Visite dokter — `Baru`, milik `ClinicalManagement` — `CAP-025`

**Satu-satunya tabel yang benar-benar baru pada sub-modul ini.**

| Class | Status | Pemilik | Lokasi file yang diusulkan |
| --- | --- | --- | --- |
| `TrxPhysicianVisit` | `Baru` | `ClinicalManagement` | `Areas/HealthServices/ClinicalManagement/Models/TrxPhysicianVisit.cs` |

| Kolom | Tipe | Wajib | Bawaan | Kenapa | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `Id` | `Guid` | Ya | `newid` | — | Tidak |
| `EncounterId` | `Guid` | Ya | — | Jangkar klinis | Tidak |
| `InpEpisodeId` | `Guid?` | Tidak | `null` | Konteks episode; nullable agar visite non-rawat-inap kelak muat | Tidak |
| `PatientId` | `Guid` | Ya | — | — | Tidak |
| `DoctorId` | `Guid` | Ya | — | PRD `CAP-025` aturan 1 | Tidak |
| `VisitDateTime` | `DateTime` | Ya | — | Aturan 1. **Waktu visite, bukan waktu pencatatan** | Tidak |
| `VisitRole` | `enum` | Ya | `Dpjp` | Aturan 1: DPJP, konsulen, atau dokter jaga | Tidak |
| `ConsultationId` | `Guid?` | Tidak | `null` | Aturan 1 dan 2: tautan **opsional** ke SOAP | Tidak |
| `ProgressNoteId` | `Guid?` | Tidak | `null` | Tautan opsional ke CPPT | Tidak |
| `Note` | `string?` | Tidak | `null` | Catatan singkat | **Ya** |
| `RecordedByUserId` | `Guid` | Ya | — | Aturan 4 dan `AC-CAP025-03` | Tidak |
| `IdempotencyKey` | `string?` | Tidak | `null` | Aturan 5 | Tidak |

`PhysicianVisitRole`: `Dpjp`, `Consultant`, `OnCall`. Bawaan `Dpjp`.

| Constraint | Bentuk | Kenapa |
| --- | --- | --- |
| Unique parsial `IdempotencyKey` | `WHERE IdempotencyKey IS NOT NULL AND IsDelete = false` | PRD aturan 5 |
| Index | `(InpEpisodeId, VisitDateTime)` | Riwayat visite satu episode, terurut |
| **Tidak ada** unique pada `(EpisodeId, DoctorId, tanggal)` | — | Dokter yang benar-benar datang dua kali pada hari yang sama adalah kejadian nyata. Menguncinya akan memaksa petugas berbohong. Duplikat dijaga kunci idempotency, bukan aturan satu-per-hari |

### 4.7 `LabOrder` — `Diperbarui` — `CAP-015`

| Hal | Isinya |
| --- | --- |
| Status | **`Diperbarui`** |
| Pemilik | `LaboratoryManagement` |
| Keadaan hari ini | Modul **ada** dan berjalan: `LabOrder`, `TrxLabSpecimen`, `TrxLabTransitionHistory`, dua controller, dua service |
| **Temuan penting** | `LabOrder` terikat pada `EncounterId` saja — **tanpa antrean dan tanpa konsultasi**. Artinya pemesanan lab untuk pasien rawat inap **tidak tertahan gerbang mana pun** |

| Kolom yang diminta | Tipe | Wajib | Bawaan | Kenapa |
| --- | --- | :---: | --- | --- |
| `InpEpisodeId` | `Guid?` | Tidak | `null` | `AC-CAP015-01`: pesanan dari episode A tidak boleh diproses sebagai milik episode B |

**Radiologi tidak punya modul.** Pencarian `Areas/HealthServices/*Radiolog*` tidak menemukan apa
pun. Karena itu `CAP-015` dirancang **sebagian**: laboratorium masuk MVP, radiologi ditunda.

---

## 5. Arsitektur folder

Nol berkas baru di bawah `InPatientManagement/`.

```text
Areas/HealthServices/ClinicalManagement/
├── Controllers/
│   ├── DoctorConsultationController.cs        Diperbarui — cabang episode (§3)
│   ├── PatientAssessmentController.cs         Diperbarui — nilai jenis kajian medis
│   ├── PatientIntegratedProgressNoteController.cs  Diperbarui — verifikasi dan amandemen
│   ├── PatientProcedureController.cs          Diperbarui — jenis catatan, idempotency
│   └── PhysicianVisitController.cs            Baru
├── Models/
│   ├── TrxDoctorConsultation.cs               Diperbarui — 3 kolom
│   ├── TrxPatientAssessment.cs                Diperbarui — nilai enum jenis
│   ├── TrxPatientIntegratedProgressNote.cs    Diperbarui — 8 kolom
│   ├── TrxPatientProcedure.cs                 Diperbarui — 5 kolom
│   └── TrxPhysicianVisit.cs                   Baru
├── Enums/
│   ├── PatientAssessmentType.cs               Diperbarui — 2 nilai medis
│   ├── CpptVerificationStatus.cs              Baru
│   ├── ProcedureRecordType.cs                 Baru
│   └── PhysicianVisitRole.cs                  Baru
└── Services/
    ├── InpatientClinicalContextResolver.cs    Diperbarui — dipakai bersama keperawatan
    └── PhysicianVisitService.cs               Baru

Areas/HealthServices/PharmacyManagement/
└── Models/TrxPrescription.cs                  Diperbarui — 3 kolom

Areas/HealthServices/LaboratoryManagement/
└── Models/LabOrder.cs                         Diperbarui — 1 kolom

Areas/HealthServices/InPatientManagement/      ◄── NOL berkas baru
```

> **Utang teknis yang sengaja tidak dirapikan.** `DoctorConsultationController.cs` dan
> `PatientAssessmentController.cs` sama-sama menaruh logika bisnis di controller, bukan service.
> Sub-modul ini **tidak** merapikannya: pemiliknya modul lain, dan refactor besar di tengah
> penambahan fitur adalah dua pekerjaan yang digabung. Ditandai sebagai utang, bukan ditiru.

---

## 6. Status model dan dampak migration

| Tabel | Status | Kolom berubah | Dampak migration |
| --- | --- | --- | --- |
| `TrxDoctorConsultation` | `Diperbarui` | `InpEpisodeId`, `ClinicalDateTime`, `VisitId` — **tiga**, seluruhnya nullable | Tanpa mematikan layanan |
| `TrxPatientAssessment` | `Diperbarui` | **Nol kolom baru dari sub-modul ini**; hanya dua nilai enum | Tanpa mematikan layanan |
| `TrxPatientIntegratedProgressNote` | `Diperbarui` | `InpEpisodeId`, `VerificationStatus`, `VerifiedAt`, `VerifiedByUserId`, `VerificationDueAt`, `AmendedAt`, `AmendedByUserId`, `AmendReason` — **delapan** | Tanpa mematikan layanan; baris lama `VerificationStatus = NotRequired` |
| `TrxPatientProcedure` | `Diperbarui` | `InpEpisodeId`, `ProcedureRecordType`, `PerformedAt`, `IdempotencyKey`, `BillingDispatchStatus` — **lima** | Tanpa mematikan layanan; baris lama `ProcedureRecordType = Performed` |
| `TrxPrescription` | `Diperbarui` | `InpEpisodeId`, `PrescriptionOrderType`, `IdempotencyKey` — **tiga** | Tanpa mematikan layanan; baris lama `Routine` |
| `LabOrder` | `Diperbarui` | `InpEpisodeId` — **satu** | Tanpa mematikan layanan |
| `TrxPhysicianVisit` | **`Baru`** | — | Tabel baru, kosong |

**Nol tabel yang bentuknya rusak, nol kolom yang dihapus, nol kolom yang berubah tipe.**

---

## 7. Rencana migration

> Urutan **antar** sub-modul dipegang [`../02-module-map.md`](../02-module-map.md) bagian 3.4.
> Seluruh langkah di bawah dijalankan **oleh pemilik modulnya**, bukan oleh task Rawat Inap.

### 7.1 Urutan

| No | Langkah | Pemilik | Tanpa mematikan layanan |
| ---: | --- | --- | :---: |
| 1 | Tambah tiga enum baru dan dua nilai pada `PatientAssessmentType` | `ClinicalManagement` | Ya |
| 2 | Tambah kolom pada empat tabel klinis | `ClinicalManagement` | Ya |
| 3 | Buat `TrxPhysicianVisit` beserta index dan unique parsialnya | `ClinicalManagement` | Ya |
| 4 | Tambah tiga kolom pada `TrxPrescription` | `PharmacyManagement` | Ya |
| 5 | Tambah satu kolom pada `LabOrder` | `LaboratoryManagement` | Ya |
| 6 | Daftarkan `DbSet` dan service | `ClinicalManagement` | Ya |
| 7 | **Longgarkan validasi kedua controller** | `ClinicalManagement` | **Tidak sepenuhnya** |
| 8 | **Longgarkan batas satu konsultasi per kunjungan dan satu resep aktif per konsultasi** untuk rawat inap | `ClinicalManagement`, `PharmacyManagement` | **Tidak sepenuhnya** |

### 7.2 Pengisian data lama

Tidak ada data lama yang perlu dipindahkan: belum ada satu pun dokumentasi dokter rawat inap.
Baris lama milik poliklinik dan IGD menerima nilai bawaan pada kolom baru dan **tidak disentuh**.

### 7.3 Langkah mundur

| Langkah gagal | Cara mundur |
| --- | --- |
| 1 s.d. 6 | Migration mundur. Tidak ada data hilang: kolomnya nullable atau bernilai bawaan, tabelnya baru dan kosong |
| 7 dan 8 | Kembalikan validasi ke bentuk semula. Tidak ada bentuk data yang berubah |

Langkah 7 dan 8 sengaja paling akhir, dan **wajib diuji bersama test regresi poliklinik dan IGD**
sesuai `RWI-DEC-051`.

---

## 8. Rencana data master awal

| Master | Isi minimum | Sumber nilai |
| --- | --- | --- |
| `MstClinicalAssessmentPolicy` | Baris untuk jenis kajian medis, bila SLA kajian medis diberlakukan | `RWI-RULE-021`, **menunggu pemilik klinis** |
| Kebijakan verifikasi CPPT | Apakah verifikasi DPJP diwajibkan, dan berapa batas waktunya | **Menunggu Clinical Governance** — PRD `CAP-021` aturan 4 |
| Master tindakan, obat, dan pemeriksaan lab | **Sudah ada** dan sudah dipakai poliklinik | — |

> **Selama kebijakan verifikasi CPPT belum ada,** `VerificationStatus` tetap `NotRequired` untuk
> seluruh catatan, daftar pantau verifikasi kosong, dan pencatatan CPPT berjalan penuh. Sama
> seperti `keperawatan`: mekanismenya dibangun, angkanya menyusul.

---

## 9. Yang sengaja tidak dibuat

| Yang ditolak | Alasan |
| --- | --- |
| Tabel `Inp*` apa pun untuk dokumentasi dokter | `RWI-DEC-081`, PRD 23.1 |
| Tabel SOAP tersendiri | `TrxDoctorConsultation` **sudah** memuat S/O/A/P. Menambah tabel berarti dua tempat untuk hal yang sama |
| `TrxMedicalAssessment` tersendiri | Menyalin ~40 kolom dari `TrxPatientAssessment`. Lihat 4.2 beserta jalan yang ditolak dan syaratnya |
| Menurunkan visite dari catatan SOAP | `INV-DOK-03`, `RWI-DEC-025`, `AC-CAP025-02`. Dokter yang menulis dua catatan sehari akan terhitung dua visite; yang datang tanpa menulis terhitung nol |
| Unique "satu visite per dokter per hari" | Dokter yang benar-benar datang dua kali sehari adalah kejadian nyata |
| Kolom status penyerahan obat milik Rawat Inap | `INV-DOK-04`, PRD `CAP-023` aturan 6. Statusnya dibaca dari Farmasi |
| Tabel salinan hasil laboratorium | `INV-DOK-05`, `AC-CAP015-02`. Hasil dibaca dari modul pemiliknya |
| Tabel radiologi | Modulnya belum ada. Membuatnya di sini berarti mengarang kepemilikan |
| Melonggarkan `ConsultationId` pada resep, tindakan, dan diagnosis | Ketiganya memang lahir dari konsultasi. Yang perlu dibuka adalah **konsultasinya**, bukan ikatannya |

---

## 10. Traceability

| Bagian | Requirement | Decision |
| --- | --- | --- |
| 1.2 `INV-DOK-03` | PRD `CAP-025`, `AC-CAP025-01`, `AC-CAP025-02` | `RWI-DEC-025` |
| 1.2 `INV-DOK-04` | PRD `CAP-023` aturan 6 | `RWI-DEC-046` |
| 1.2 `INV-DOK-05` | PRD `CAP-015` aturan 4, `AC-CAP015-02` | — |
| 2 kepemilikan | PRD 23.1 | `RWI-DEC-081`, `RWI-DEC-083` |
| 3 pelonggaran | PRD 30.3 | `RWI-DEC-038`, `RWI-DEC-062`, `RWI-DEC-070`, `RWI-DEC-080` |
| 4.1 SOAP | PRD `CAP-020` aturan 1 s.d. 5 | — |
| 4.2 kajian medis | PRD `CAP-022`, `AC-CAP022-02` | **Butuh persetujuan struktur pemilik** |
| 4.3 CPPT | PRD `CAP-021` aturan 2, 4, 5, 6 | — |
| 4.4 tindakan | PRD `CAP-024` aturan 3, 5 | — |
| 4.5 resep | PRD `CAP-023` aturan 7, 8 | `RWI-DEC-033`, `RWI-DEC-046` |
| 4.6 visite | PRD `CAP-025` aturan 1 s.d. 5 | `RWI-DEC-025` |
| 4.7 penunjang | PRD `CAP-015`, `AC-CAP015-01` | — |
