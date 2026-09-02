# Arsitektur Backend — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — satu dari tiga sub-modul modul `rawat-inap`, bentuk `COMPOSITE` sejak `RWI-DEC-082` |
| Revision | `0.1` |
| Status | `draft` — belum disetujui manusia |
| Tanggal | 2 September 2026 (`Asia/Jakarta`) |
| Kemampuan | `CAP-012`, `CAP-013`, `CAP-014`, `CAP-016`, `CAP-027` — `RWI-DEC-083` |
| Masukan baseline | `PRD-RWI-FINAL-001` v1.0.0 bagian 16, 17, 20, 23.1, 30.3 |
| Masukan keputusan | [`../00-interview-decisions.md`](../00-interview-decisions.md) revision `7` — `RWI-DEC-080` s.d. `RWI-DEC-083` |
| Peta modul | [`../02-module-map.md`](../02-module-map.md) revision `1` |
| Manifest sub-modul | [`blueprint-manifest.md`](./blueprint-manifest.md) |
| Backend SHA | `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`); audit as-is dijalankan 2026-09-02 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — `/qv-domain` tidak dijalankan untuk slice ini. Alasannya: batas konteks dan kepemilikan datanya **sudah** ditetapkan `RWI-DEC-081` dan `PRD-RWI-FINAL-001` bagian 23.1, sehingga tidak ada batas domain yang perlu diturunkan ulang |

---

## 0. Kalimat terpenting pada dokumen ini

> **Sub-modul ini tidak memiliki satu tabel pun, dan itu disengaja.**

`RWI-DEC-081` dan `PRD-RWI-FINAL-001` bagian 23.1 menetapkan Nursing Assessment, Nursing Care, dan
Nursing Interventions dimiliki **`ClinicalManagement`**; Nutrition Assessment/Care dimiliki **modul
Gizi**. Rawat Inap hanya menyediakan **konteks episode, ruang kerja, dan kontrak**.

Karena itu dokumen ini berbeda bentuk dari `02-backend-architecture.md` milik
`episode-rawat-inap`. Ia tidak merancang tabel milik sendiri; ia menyatakan:

1. apa yang **sudah ada** dan dapat dipakai apa adanya;
2. apa yang **kurang** dan diminta kepada modul pemiliknya, beserta bentuk persisnya;
3. penjaga apa yang membuktikan sub-modul ini **tidak** membuat tabel tandingan.

Bila kelak desain terasa menuntut tabel `Inp*` untuk pengkajian atau catatan keperawatan, yang
benar adalah kembali ke `/qv-grill`, **bukan** membuatnya diam-diam. Larangan ini diwariskan
`RWI-DEC-081` dan tercatat pada `blueprint-manifest.md` bagian 8.

---

## 1. Bounded context dan ownership

### 1.1 Kedudukan sub-modul ini

| Hal | Ketetapannya |
| --- | --- |
| Jenis konteks | **Workspace context** — mengumpulkan dan menyajikan, bukan memiliki |
| Aggregate root yang dimiliki | **Nol** |
| Aggregate root yang **dibaca** | `InpEpisode` milik `episode-rawat-inap`; `TrxPatientAssessment` milik `ClinicalManagement` |
| Transaction boundary | **Tidak ada transaksi milik sub-modul ini.** Setiap tulisan terjadi di dalam transaksi milik modul pemilik tabelnya |
| Yang menjadi milik sub-modul ini | **Aturan kewenangan berbasis episode** dan **kontrak konteks** — lihat 1.3 |

### 1.2 Konteks yang bersinggungan

| Konteks | Modul | Hubungannya dengan sub-modul ini |
| --- | --- | --- |
| `CTX-INP` Episode rawat inap | `InPatientManagement` / `episode-rawat-inap` | **Dibaca.** Sumber konteks: pasien siapa, di mana, episodenya hidup atau tidak, perawat penanggung jawabnya siapa |
| `CTX-CLI` Dokumentasi klinis | `ClinicalManagement` | **Pemilik tabel** pengkajian, asuhan, dan tindakan keperawatan |
| `CTX-NUT` Gizi | Modul Gizi (`PLANNED`) | **Pemilik** asuhan gizi. Sub-modul ini hanya menghasilkan pemicu rujukan dan membaca statusnya |
| `CTX-INV` Persediaan dan aset | Modul persediaan | Pemilik master alat. **Kepemilikan catatan pemakaiannya belum diputuskan** — lihat 2.3 |
| `CTX-BIL` Billing | `BillingManagement` | Penerima pemicu tagihan untuk tindakan dan pemakaian alat yang dapat ditagih |

### 1.3 Satu-satunya hal yang benar-benar dimiliki sub-modul ini

**Aturan kewenangan berbasis episode.** Hari ini kewenangan menulis dokumentasi klinis diturunkan
dari **antrean** atau **kunjungan IGD**. Pasien rawat inap tidak punya keduanya. Yang
menggantikannya adalah episode:

| Invariant | Bunyinya | Kenapa milik sub-modul ini |
| --- | --- | --- |
| `INV-KEP-01` | Sebuah pengkajian, asuhan, atau tindakan keperawatan rawat inap **hanya** boleh dibuat bila ada `InpEpisode` yang berstatus `Admitted` untuk `EncounterId` yang sama | Episode adalah milik modul ini; mesin klinis tidak tahu apa itu episode |
| `INV-KEP-02` | Episode yang sudah `Closed` **tidak** menerima dokumentasi baru; isinya menjadi hanya-baca | `AC-CAP013-03` menuntut riwayat tetap terbaca setelah penutupan |
| `INV-KEP-03` | Dokumentasi keperawatan **tidak pernah** menahan pekerjaan dokter maupun penempatan tempat tidur | `PRD` bagian 16.3: "Dokter tidak perlu menunggu Initial Nursing Assessment selesai". Menjadikannya gerbang akan menahan pasien di IGD atau di lorong |

`INV-KEP-03` adalah penjaga keselamatan, bukan kenyamanan. Ia melarang siapa pun kelak
menjadikan pengkajian sebagai syarat penempatan.

---

## 2. Tabel kepemilikan data

> Tabel kepemilikan data **seluruh modul** ada di [`../02-module-map.md`](../02-module-map.md)
> bagian 2. Yang di bawah ini hanya kelompok data yang disentuh sub-modul ini, beserta
> statusnya.

### 2.1 Yang dipakai, tidak dibuat ulang

| Kelompok data | Modul pemilik | Dipakai sub-modul ini | Dibuat ulang |
| --- | --- | :---: | --- |
| Pasien | Patient Management | Ya — dibaca lewat episode | **Tidak** |
| Kunjungan pasien | Registration Management | Ya — jangkar `EncounterId` | **Tidak** |
| Episode rawat inap | `episode-rawat-inap` | Ya — sumber konteks dan kewenangan | **Tidak** |
| Perawat penanggung jawab | `episode-rawat-inap` | Ya — `InpNurseAssignment` menentukan siapa yang berwenang | **Tidak** |
| Pengkajian pasien | **Clinical Management** | Ya — **ditulis** lewat endpoint milik modul itu | **Tidak** — `RWI-DEC-081`, PRD 23.1 |
| CPPT | **Clinical Management** | Ya — catatan keperawatan tampil di sana | **Tidak** |
| Tanda vital | **Clinical Management** | Ya — dibaca dan ditulis lewat endpoint milik modul itu | **Tidak** |
| Alergi, riwayat penyakit, riwayat keluarga | **Clinical Management** | Ya — dibaca saat pengkajian awal | **Tidak** |
| Asuhan gizi | **Modul Gizi** (`PLANNED`) | Status dan ringkasannya dibaca | **Tidak** — PRD 23.1 |
| Master alat/aset | Modul persediaan | Ya — dirujuk lewat Id | **Tidak** |

### 2.2 Yang **belum ada di mana pun** dan diminta kepada pemiliknya

Ketiga kelompok berikut tidak punya tabel di seluruh repository per audit 2026-09-02. Karena
pemiliknya sudah ditetapkan, yang dilakukan sub-modul ini adalah **meminta**, bukan membuat.

| Kelompok data | Modul pemilik | Keadaan hari ini | Diminta oleh |
| --- | --- | --- | --- |
| Rencana asuhan keperawatan | **Clinical Management** | **Tidak ada.** Nol berkas `*CarePlan*`, nol berkas `*Nursing*` | `CAP-013`, bagian 4.2 |
| Tindakan/catatan keperawatan | **Clinical Management** | **Tidak ada yang cocok.** `TrxPatientProcedure` mewajibkan `ConsultationId` dan `DoctorId`, sehingga tindakan perawat tidak dapat masuk ke sana | `CAP-014`, bagian 4.3 |
| Riwayat amandemen pengkajian | **Clinical Management** | **Tidak ada.** `TrxPatientAssessment` tidak menyimpan versi | `CAP-012` aturan 13, bagian 4.1 |

### 2.3 Satu kelompok data yang kepemilikannya **belum diputuskan**

| Kelompok data | Pemilik | Dipakai sub-modul ini | Dibuat ulang |
| --- | --- | :---: | --- |
| Catatan pemakaian alat pada pasien | **BELUM DIPUTUSKAN** | `CAP-016` | **`OPEN DECISION`** |

**Ditulis apa adanya, dan ini menahan `CAP-016`.**

| Hal | Keadaannya |
| --- | --- |
| Kenapa terbuka | `PRD-RWI-FINAL-001` bagian 23.1 memuat 28 baris *source of truth* dan **tidak satu pun** menyebut Equipment Usage. `RWI-DEC-081` juga tidak menyebutnya — keputusan itu hanya mencakup pengkajian, CPPT, SOAP, kajian medis, resep, dan tindakan |
| Calon pemiliknya | Tiga: modul persediaan/aset (karena alatnya miliknya), `ClinicalManagement` (karena pemakaiannya peristiwa klinis), atau `InPatientManagement` (karena terikat episode) |
| Kenapa tidak dipilih di sini | Memilih pemilik tabel adalah keputusan pemilik modul, bukan keputusan blueprint. `PRD` bagian 20 aturan 2 hanya melarang **menduplikasi master alat**, dan itu tidak menjawab siapa pemilik catatan pemakaiannya |
| Akibatnya | `CAP-016` berstatus `OPEN DECISION` dan **MUST NOT** masuk gelombang pengiriman mana pun — `04-prd-to-mvp.md` bagian 8 dan 19 |
| Yang **tidak** ditahannya | `CAP-012`, `CAP-013`, `CAP-014`, dan `CAP-027`. Keempatnya kepemilikannya sudah tegas |

Dicatat sebagai pertanyaan terbuka untuk `/qv-grill`; usulan ID `RWI-OQ-048`.

---

## 3. Penghalang teknis: *shared inpatient clinical context resolver*

`PRD-RWI-FINAL-001` bagian 30.3 menyebutnya *critical technical gap*. Audit source 2026-09-02
menemukan bentuknya **jauh lebih kecil dari yang diduga** — dan itu temuan terpenting dokumen ini.

### 3.1 Keadaan sebenarnya di source hari ini

| Yang diduga | Yang sebenarnya |
| --- | --- |
| Mesin klinis mewajibkan antrean, sehingga butuh jalur baru | Jalur **tanpa antrean sudah ada** dan sudah berjalan di produksi untuk pasien IGD |
| Butuh subsistem resolver baru | Butuh **satu cabang tambahan** pada satu method validasi yang sudah ada |

Buktinya: `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`
method `ValidateCreateWithoutQueueAsync`. Kolom `TrxPatientAssessment.QueueId` **sudah**
`Guid?` — nullable — dan `CreateAssessment` sudah mengambil identitas klinis dari encounter
ketika antrean tidak ada.

Yang menutup pintu bagi rawat inap hanya satu pemeriksaan:

```text
Jalur tanpa antrean terbuka HANYA bila encounter punya baris kunjungan IGD.
Bila tidak: ditolak, "Pengkajian tanpa antrean hanya untuk pasien IGD."
```

Komentar `IGD-DEC-109` pada berkas itu menjelaskan kenapa gerbangnya sempit: melepas kewajiban
antrean **tanpa syarat** akan membuat pengkajian rawat jalan dapat dibuat melewati screening.
Alasan itu benar dan **tetap berlaku** — jadi yang diminta bukan mencabut gerbangnya, melainkan
**menambah satu pintu yang syaratnya setara**.

### 3.2 Bentuk yang diminta

| Hal | Ketetapannya |
| --- | --- |
| Pemilik perubahan | **`ClinicalManagement`** — Muhammad Hamzah, persetujuan sudah diberikan `RWI-DEC-062` |
| Bentuk perubahan | Cabang ketiga pada `ValidateCreateWithoutQueueAsync`: encounter diterima bila punya `InpEpisode` berstatus `Admitted` |
| Syarat yang setara dengan IGD | IGD memakai keberadaan `EmgVisit`; rawat inap memakai keberadaan `InpEpisode` **yang sedang `Admitted`**. Keduanya sama-sama bukti pasien benar-benar sedang dilayani, bukan sekadar terdaftar |
| Yang **tidak** boleh berubah | Perilaku rawat jalan dan medical check-up. `RWI-DEC-070` menegaskan keduanya tetap wajib berantrean |
| Kenapa `Admitted`, bukan sekadar episode ada | Episode `Draft` berarti pasien belum tiba. Menerima `Draft` membuat pengkajian dapat ditulis untuk pasien yang belum ada di kamar |
| Nol kolom baru | Perubahan ini **tidak** menyentuh satu kolom pun. Ia murni pelonggaran validasi |

### 3.3 Kenapa ini bukan pekerjaan sub-modul ini

Perubahannya ada di dalam controller milik `ClinicalManagement`. Sub-modul ini **menyatakan
kebutuhannya dan menyediakan cara memeriksanya** — pembacaan `InpEpisode` — tetapi tidak menulis
kodenya. Kontraknya ada di [`contracts/integration-contract.md`](./contracts/integration-contract.md)
sebagai `INT-KEP-01`.

---

## 4. Entity: yang ada, yang diperluas, yang diminta baru

Status memakai kosakata `Sudah ada` / `Diperbarui` / `Baru`. Kolom **Pemilik** menyebut modul yang
berwenang mengubahnya — **bukan** sub-modul ini.

### 4.1 `TrxPatientAssessment` — `Diperbarui`

| Hal | Isinya |
| --- | --- |
| Status | **`Diperbarui`** |
| Pemilik | `ClinicalManagement` |
| Lokasi file | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` |
| Keadaan hari ini | **85 kolom.** Sudah memuat kajian umum, kesadaran, tanda vital, EWS, nyeri, alergi, imunisasi, gizi, risiko jatuh, status fungsional, psikososial, edukasi, dan catatan perawat |
| Penilaian terhadap PRD bagian 16.1 | **Sepuluh dari tiga belas** section target sudah tertampung kolomnya |

Kolom yang **diminta ditambahkan**, satu per satu:

| Kolom | Tipe | Wajib | Bawaan | Kenapa | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `InpEpisodeId` | `Guid?` | Tidak | `null` | PRD 16.2 aturan 1: pengkajian wajib terikat ke episode. Nullable karena pengkajian poliklinik dan IGD tidak punya episode | Tidak |
| `AssessmentType` | `enum` | Ya | `Initial` | PRD 16.2 aturan 3: initial dan reassessment **wajib** record terpisah. Tanpa kolom ini keduanya tidak dapat dibedakan | Tidak |
| `DueAt` | `DateTime?` | Tidak | `null` | PRD 16.2 aturan 11: sistem wajib dapat memantau `DueAt`, `CompletedAt`, dan keadaan terlambat | Tidak |
| `PolicyId` | `Guid?` | Tidak | `null` | Menunjuk konfigurasi SLA yang berlaku saat pengkajian dibuat, supaya perubahan kebijakan tidak mengubah penilaian keterlambatan yang lalu | Tidak |
| `AmendedAt` | `DateTime?` | Tidak | `null` | PRD 16.2 aturan 13 | Tidak |
| `AmendedByUserId` | `Guid?` | Tidak | `null` | Sama | Tidak |

Enum yang diminta:

| Enum | Nilai | Bawaan |
| --- | --- | --- |
| `PatientAssessmentType` | `Initial`, `Reassessment`, `DailyReassessment`, `DischargePlanning` | `Initial` |
| `PatientAssessmentStatus` — **diperluas** | `Draft`, `InProgress`, `Completed`, `Cancelled`, **`Amended`** | `Draft` |

> **`Amended` adalah nilai baru, bukan pengganti.** PRD 16.2 aturan 10 menyebut `NotStarted`,
> `Draft`, `Completed`, `Amended`. `NotStarted` **sengaja tidak dibuat** — lihat bagian 8.

Index dan constraint yang diminta:

| Nama | Bentuk | Kenapa |
| --- | --- | --- |
| `IX_TrxPatientAssessment_InpEpisodeId` | Index biasa pada `InpEpisodeId` | Ruang kerja membaca seluruh pengkajian satu episode |
| `IX_TrxPatientAssessment_Episode_Type_Active` | Index parsial `(InpEpisodeId, AssessmentType)` `WHERE AssessmentType = Initial AND IsDelete = false` | Menemukan pengkajian awal satu episode tanpa memindai seluruh tabel |
| Unique constraint | **Tidak diminta** | Menguncinya "satu pengkajian awal per episode" terdengar benar, tetapi pengkajian awal yang dibatalkan lalu diulang adalah kejadian nyata. Aturan itu dijaga di tingkat service, bukan database |
| `DeleteBehavior` pada `InpEpisodeId` | `Restrict` | Episode tidak boleh terhapus selagi pengkajiannya ada. Selaras dengan `AC-CAP013-03` |

### 4.2 Rencana asuhan keperawatan — `Baru`, milik `ClinicalManagement`

`CAP-013`. Bentuk yang diusulkan; **pemilik yang menetapkan bentuk akhirnya.**

| Class | Status | Pemilik | Lokasi file yang diusulkan | Kegunaan |
| --- | --- | --- | --- | --- |
| `TrxNursingCarePlan` | `Baru` | `ClinicalManagement` | `Areas/HealthServices/ClinicalManagement/Models/TrxNursingCarePlan.cs` | Satu rencana asuhan per episode; wadah bagi butir-butirnya |
| `TrxNursingCarePlanItem` | `Baru` | `ClinicalManagement` | `…/Models/TrxNursingCarePlanItem.cs` | Satu masalah keperawatan beserta tujuan, rencana tindakan, dan evaluasinya |
| `TrxNursingCarePlanItemRevision` | `Baru` | `ClinicalManagement` | `…/Models/TrxNursingCarePlanItemRevision.cs` | Salinan versi sebelumnya. `AC-CAP013-02` menuntut perubahan menyimpan riwayat **tanpa** mengubah penulis dan waktu versi lama |
| `MstNursingDiagnosis` | **`OPEN DECISION`** | `ClinicalManagement` / Master Data | — | Katalog terminologi. PRD 17 aturan 3 mensyaratkannya **hanya bila** rumah sakit memakai SDKI/SLKI/SIKI, dan itu belum dinyatakan |

**Kenapa butir dipisah dari rencananya.** Satu pasien punya beberapa masalah keperawatan yang
lahir dan tutup pada waktu berbeda. Menaruhnya dalam satu baris berarti menutup satu masalah
menyentuh baris yang sama dengan masalah lain yang masih berjalan.

**Kenapa revisi berbentuk tabel terpisah, bukan kolom.** `AC-CAP013-02` menuntut versi lama
mempertahankan penulis dan waktunya. Menimpa kolom akan menghapus keduanya.

### 4.3 Tindakan dan catatan keperawatan — `Baru`, milik `ClinicalManagement`

`CAP-014`.

| Class | Status | Pemilik | Lokasi file yang diusulkan | Kegunaan |
| --- | --- | --- | --- | --- |
| `TrxNursingIntervention` | `Baru` | `ClinicalManagement` | `…/Models/TrxNursingIntervention.cs` | Tindakan yang **benar-benar dilakukan** perawat: apa, kapan, oleh siapa, hasilnya, konteks episodenya |

Kolom yang material:

| Kolom | Tipe | Wajib | Kenapa | Sensitif |
| --- | --- | :---: | --- | :---: |
| `EncounterId` | `Guid` | Ya | Jangkar yang sama dengan seluruh dokumentasi klinis | Tidak |
| `InpEpisodeId` | `Guid?` | Tidak | Konteks episode. Nullable supaya tindakan keperawatan non-rawat-inap kelak dapat memakai tabel yang sama | Tidak |
| `CarePlanItemId` | `Guid?` | Tidak | **Nullable, dan ini keputusan.** PRD 17 `CAP-014` aturan 3: rencana boleh menjadi rujukan tetapi **bukan syarat** bagi tindakan mendadak yang perlu secara klinis | Tidak |
| `PerformedAt` | `DateTime` | Ya | Waktu tindakan, bukan waktu pencatatan | Tidak |
| `PerformedByEmployeeId` | `Guid` | Ya | Pelaku sebenarnya | Tidak |
| `ResultNote` | `string?` | Tidak | Hasil atau catatan | **Ya** |
| `IdempotencyKey` | `string?` | Tidak | `AC-CAP014-01`: satu tindakan tersimpan sekali walaupun permintaan diulang | Tidak |
| `BillingDispatchStatus` | `enum` | Ya | `AC-CAP014-02`: catatan klinis tetap tersimpan walaupun pengiriman ke Billing gagal | Tidak |
| `FinalizedAt` | `DateTime?` | Tidak | `AC-CAP014-03`: catatan yang sudah final tidak dapat disunting diam-diam | Tidak |

| Constraint | Bentuk | Kenapa |
| --- | --- | --- |
| Unique parsial pada `IdempotencyKey` | `WHERE IdempotencyKey IS NOT NULL AND IsDelete = false` | Menjaga `AC-CAP014-01` di database, bukan hanya di service. Percobaan ulang yang menembus lapisan aplikasi tetap tertolak |

> **Kenapa `TrxPatientProcedure` tidak dipakai ulang.** Ia mewajibkan `ConsultationId` dan
> `DoctorId`. Tindakan perawat tidak punya konsultasi dan tidak punya dokter. Melonggarkan kedua
> kolom itu menjadi nullable akan melemahkan penjagaan bagi tindakan **dokter**, yang justru
> memerlukannya untuk penagihan. Menambah tabel terpisah lebih murah daripada melemahkan tabel
> yang sudah dipakai modul lain.

### 4.4 CPPT — `Sudah ada`, dipakai apa adanya

| Hal | Isinya |
| --- | --- |
| Class | `TrxPatientIntegratedProgressNote` |
| Status | **`Sudah ada`** — nol perubahan diminta |
| Pemilik | `ClinicalManagement` |
| Kenapa cocok | Ia sudah punya `ProfessionType`, dan `EncounterId`, `QueueId`, `ConsultationId`, `DoctorId` seluruhnya **sudah** nullable. Catatan keperawatan dapat masuk tanpa satu perubahan pun |
| Kepemilikan kontraknya | **`dokter-rawat-inap`** — `CAP-021`, `RWI-DEC-083`. Sub-modul ini **menulis** ke sana, tidak memiliki kontraknya. CPPT memang lintas profesi; itu sifatnya |

### 4.5 Konfigurasi SLA klinis — `Baru`, milik `ClinicalManagement`

`CAP-012` aturan 11 menuntut SLA **dapat dikonfigurasi Clinical Governance**, dan melarang
menanam angka yang belum disetujui.

| Class | Status | Pemilik | Kegunaan |
| --- | --- | --- | --- |
| `MstClinicalAssessmentPolicy` | `Baru` | `ClinicalManagement` | Batas waktu per jenis pengkajian per jenis pelayanan, berversi supaya penilaian keterlambatan yang lalu tidak berubah saat kebijakan diubah |

> **Angkanya sengaja tidak diisi blueprint ini.** `RWI-RULE-021` masih menunggu pemilik klinis.
> Yang dirancang adalah **tempatnya**, bukan isinya. Modul dengan master kosong tidak dapat
> dipakai — karena itu bagian 7 menyatakan rencana data master awalnya, dan bagian 9 menyatakan
> apa yang terjadi selama angkanya belum ada.

---

## 5. Arsitektur folder

Tidak ada folder baru di bawah `Areas/HealthServices/InPatientManagement/`. Seluruh berkas yang
diminta berada di dalam modul pemiliknya.

```text
Areas/HealthServices/ClinicalManagement/          ◄── PEMILIK, bukan sub-modul ini
├── Controllers/
│   ├── PatientAssessmentController.cs            Diperbarui — cabang episode pada validasi
│   ├── NursingCarePlanController.cs              Baru
│   └── NursingInterventionController.cs          Baru
├── Models/
│   ├── TrxPatientAssessment.cs                   Diperbarui — 6 kolom
│   ├── TrxNursingCarePlan.cs                     Baru
│   ├── TrxNursingCarePlanItem.cs                 Baru
│   ├── TrxNursingCarePlanItemRevision.cs         Baru
│   ├── TrxNursingIntervention.cs                 Baru
│   └── MstClinicalAssessmentPolicy.cs            Baru
├── Enums/
│   ├── PatientAssessmentStatus.cs                Diperbarui — nilai `Amended`
│   ├── PatientAssessmentType.cs                  Baru
│   ├── NursingCarePlanItemStatus.cs              Baru
│   └── NursingBillingDispatchStatus.cs           Baru
└── Services/
    ├── InpatientClinicalContextResolver.cs       Baru — penjaga `INV-KEP-01`
    ├── NursingCarePlanService.cs                 Baru
    └── NursingInterventionService.cs             Baru

Areas/HealthServices/InPatientManagement/         ◄── NOL berkas baru dari sub-modul ini
```

> **Utang teknis yang sengaja tidak dirapikan.** `PatientAssessmentController.cs` berisi 1.298
> baris dan memuat logika bisnis di dalam controller, bukan di service — menyimpang dari pola
> `Controller → Service` yang dipakai `InPatientManagement`. Sub-modul ini **tidak** merapikannya:
> pemiliknya modul lain, dan refactor 1.298 baris di tengah penambahan fitur adalah dua pekerjaan
> yang digabung. Ditandai sebagai utang, bukan ditiru dan bukan pula diperbaiki diam-diam.

---

## 6. Status model dan dampak migration

| Tabel | Status | Kolom yang berubah | Dampak migration |
| --- | --- | --- | --- |
| `TrxPatientAssessment` | `Diperbarui` | `InpEpisodeId`, `AssessmentType`, `DueAt`, `PolicyId`, `AmendedAt`, `AmendedByUserId` — **enam**, seluruhnya nullable kecuali `AssessmentType` yang punya nilai bawaan | Dapat berjalan tanpa mematikan layanan. Baris lama terisi `AssessmentType = Initial` |
| `TrxNursingCarePlan` | `Baru` | — | Tabel baru, kosong |
| `TrxNursingCarePlanItem` | `Baru` | — | Tabel baru, kosong |
| `TrxNursingCarePlanItemRevision` | `Baru` | — | Tabel baru, kosong |
| `TrxNursingIntervention` | `Baru` | — | Tabel baru, kosong |
| `MstClinicalAssessmentPolicy` | `Baru` | — | Tabel master baru; **wajib diisi** sebelum pemantauan keterlambatan menyala |
| `TrxPatientIntegratedProgressNote` | `Sudah ada` | **Nol** | Tidak ada |
| `TrxPatientProcedure` | `Sudah ada` | **Nol** | Tidak ada — sengaja tidak dilonggarkan, lihat 4.3 |

---

## 7. Rencana migration

> Urutan **antar** sub-modul dipegang [`../02-module-map.md`](../02-module-map.md) bagian 3.4.
> Yang di bawah ini urutan **di dalam** sub-modul ini, dan seluruhnya dijalankan **oleh pemilik
> `ClinicalManagement`**, bukan oleh task Rawat Inap.

### 7.1 Urutan

| No | Langkah | Tanpa mematikan layanan | Keterangan |
| ---: | --- | :---: | --- |
| 1 | Tambah enum `PatientAssessmentType` dan nilai `Amended` pada `PatientAssessmentStatus` | Ya | Perubahan kode |
| 2 | Buat `MstClinicalAssessmentPolicy` | Ya | Tabel baru |
| 3 | Tambah enam kolom pada `TrxPatientAssessment` | Ya | Seluruhnya nullable atau bernilai bawaan; baris lama tidak perlu disentuh |
| 4 | Buat empat tabel transaksi keperawatan | Ya | Tabel baru |
| 5 | Buat index episode dan unique parsial `IdempotencyKey` | Ya | Tabel masih kosong |
| 6 | Daftarkan `DbSet` dan service pada `ApplicationDbContext` serta `Program.cs` | Ya | Perubahan kode |
| 7 | **Longgarkan validasi**: cabang episode pada `ValidateCreateWithoutQueueAsync` | **Tidak sepenuhnya** | Mengubah perilaku endpoint yang sudah dipakai IGD dan poliklinik. Lihat 7.3 |

### 7.2 Pengisian data lama

**Tidak ada data lama yang perlu dipindahkan.** Belum ada satu pun pengkajian rawat inap di dalam
sistem, karena jalurnya memang belum terbuka. Baris `TrxPatientAssessment` yang sudah ada milik
poliklinik dan IGD; keenam kolom baru dibiarkan `null` dan `AssessmentType` terisi `Initial`.

Yang perlu diperiksa sebelum langkah 7: pastikan tidak ada encounter rawat jalan yang punya
`InpEpisode` menggantung berstatus `Admitted`. Bila ada, cabang baru akan membuka pengkajian
tanpa antrean bagi encounter yang seharusnya tetap berantrean.

### 7.3 Langkah mundur bila gagal

| Langkah gagal | Cara mundur |
| --- | --- |
| 1 s.d. 6 | Migration mundur. Tidak ada data hilang: tabelnya baru dan kosong, kolomnya nullable |
| 7 | Kembalikan `ValidateCreateWithoutQueueAsync` ke bentuk semula. Tidak ada bentuk data yang berubah, sehingga tidak ada yang perlu dipulihkan |

Langkah 7 sengaja paling akhir supaya seluruh tempat penyimpanan sudah berdiri sebelum satu-satunya
perubahan perilaku dijalankan.

---

## 8. Rencana data master awal

| Master | Isi minimum | Sumber nilai |
| --- | --- | --- |
| `MstClinicalAssessmentPolicy` | Sekurang-kurangnya satu baris berlaku untuk jenis pelayanan rawat inap: batas waktu pengkajian awal, batas waktu pengkajian ulang harian | **`RWI-RULE-021`, menunggu pemilik klinis** |
| `MstNursingDiagnosis` | Bergantung `OPEN DECISION` pada 4.2 | Katalog SDKI bila rumah sakit memakainya |

> **Selama `MstClinicalAssessmentPolicy` kosong, apa yang terjadi.** Pengkajian tetap dapat dibuat,
> disimpan, dan dibaca — `DueAt` sekadar tidak terisi dan tidak ada yang dinyatakan terlambat.
> Pemantauan keterlambatan mati, pencatatan hidup. Ini keputusan sadar: menahan pencatatan karena
> angka kebijakan belum turun akan menahan pekerjaan perawat demi laporan.

---

## 9. Yang sengaja tidak dibuat

| Yang ditolak | Alasan |
| --- | --- |
| `InpNursingAssessment` atau tabel `Inp*` apa pun untuk dokumentasi klinis | `RWI-DEC-081` dan PRD 23.1 menaruhnya pada `ClinicalManagement`. Membuat tandingan adalah pelanggaran batas, bukan kemandirian |
| Status `NotStarted` pada `PatientAssessmentStatus` | PRD 16.2 aturan 10 menyebutnya, tetapi "belum dimulai" berarti **tidak ada barisnya** — bukan baris berstatus tertentu. Menyimpannya sebagai status memaksa membuat baris kosong untuk setiap episode. Ruang kerja menurunkan "belum dimulai" dari ketiadaan baris |
| Status episode keenam untuk menandai pengkajian selesai | `RWI-DEC-009` mengunci lima nilai status episode, dan `AC-CAP012-03` secara tegas melarang menambah status episode baru |
| Melonggarkan `ConsultationId` dan `DoctorId` pada `TrxPatientProcedure` | Melemahkan penjagaan bagi tindakan dokter yang membutuhkannya untuk penagihan. Lihat 4.3 |
| Menyalin master alat ke dalam modul ini | PRD 20 aturan 2 melarangnya tegas |
| Tabel gizi milik Rawat Inap | PRD 23.1 menaruh Nutrition Assessment/Care pada modul Gizi. Sub-modul ini hanya menghasilkan pemicu rujukan dan membaca status |
| Menjadikan pengkajian awal sebagai gerbang penempatan atau gerbang instruksi dokter | `INV-KEP-03`. PRD 16.3 menyatakannya tegas |

---

## 10. Traceability

| Bagian | Requirement | Decision |
| --- | --- | --- |
| 1.3 invariant | PRD 16.3, `AC-CAP012-03` | `RWI-DEC-009` |
| 2 kepemilikan data | PRD 23.1 | `RWI-DEC-081`, `RWI-DEC-083` |
| 2.3 `CAP-016` terbuka | PRD 20, 23.1 (tidak memuat barisnya) | **Belum ada** — usulan `RWI-OQ-048` |
| 3 resolver | PRD 30.3 | `RWI-DEC-062`, `RWI-DEC-070`, `RWI-DEC-080` |
| 4.1 kolom pengkajian | PRD 16.2 aturan 1, 3, 10, 11, 13 | `RWI-DEC-081` |
| 4.2 asuhan keperawatan | PRD 17 `CAP-013`, `AC-CAP013-01` s.d. `03` | `RWI-DEC-081` |
| 4.3 tindakan keperawatan | PRD 17 `CAP-014`, `AC-CAP014-01` s.d. `03` | `RWI-DEC-081` |
| 4.5 SLA | PRD 16.2 aturan 11, `AC-CAP012-04` | `RWI-RULE-021` **terbuka** |
| 8 data master | PRD 16.2 aturan 11 | `RWI-RULE-021` **terbuka** |
