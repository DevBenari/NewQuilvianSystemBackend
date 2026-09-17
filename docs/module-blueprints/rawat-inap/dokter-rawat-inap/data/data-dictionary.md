# Kamus Data — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` |
| Revision | **`0.5`** — amandemen penyelarasan `PRD-RWI-V2-001`, blueprint revision `7`. Tabel baru dan kolom baru ada pada **bagian 13**. Bagian 7 memakai nama lama `TrxPrescription`; nama yang benar `PhmPrescription` |
| Status | **`draft`** untuk `0.5`. Revision `0.4` disetujui Muhammad Hamzah, 2026-09-09; bagian 3 direvisi dan disetujui ulang 2026-09-05 |
| `approved_by` / `approved_at` | — untuk `0.5`. **Muhammad Hamzah** / **2026-09-09** untuk revision `0.4`; `0.3` disetujui 2026-09-03 |
| Tanggal | 2 September 2026; diamendemen 9 September 2026; **diamendemen 15 September 2026** |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) revision `0.4` bagian 4; **revision `0.5` bagian 11** |
| Backend SHA | `93b3227c431401d8f586dec4e1fb25fbf41766e3`; **bagian 13 dibaca pada `df3679c0d5b2f08106702153eb242d3a6cb2929b`** |

---

## 0. Empat hal yang wajib dibaca lebih dulu

**Pertama: tidak satu tabel pun di dokumen ini dimiliki modul Rawat Inap.** Kolom **Pemilik**
menyebut modul yang berwenang mengubahnya — `RWI-DEC-081`.

**Kedua: sepuluh kolom warisan `IdentityModel` tidak diulang per tabel.** Seluruh tabel mewarisi
`IdentityModel`, sehingga memiliki `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`,
`DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`.

**Ketiga: penghapusan bersifat penandaan** melalui `IsDelete`, bukan penghapusan baris.

**Keempat: kolom bertanda Sensitif** tidak boleh masuk custom logger dan tidak boleh dipakai
sebagai contoh berisi data asli.

### 0.1 Yang berubah dari revision `0.1`

| Perubahan | Alasan |
| --- | --- |
| `TrxPhysicianVisit` → **`CliPhysicianVisit`** | `QBE-NAM-001` melarang `Trx*` untuk kode baru; prefix registry `ClinicalManagement` adalah `Cli` |
| Enam kolom **dicabut**: tiga kolom amandemen pada CPPT, `ProcedureRecordType`, dan `BillingDispatchStatus` | Mesinnya sudah ada — addendum `MedicalRecordManagement`, dan status tindakan beserta penanda tagihan yang sudah tersedia |
| Tujuh kolom **ditambah** pada tabel visite | Status, pembatalan beralasan, tautan tindakan, nomor bisnis, dan penunjuk event yang digantikan |
| `IdempotencyKey` visite menjadi **wajib** | `INV-DOK-06` tidak dapat dijamin bila kuncinya boleh kosong |
| `RadOrder` masuk kamus | Modul Radiologi terbukti ada |

---

## 1. Status dan kepemilikan tabel

| Entity | Status | Owner | Kemampuan | Catatan |
| --- | --- | --- | --- | --- |
| `TrxDoctorConsultation` | **`Diperbarui`** | `ClinicalManagement` | `CAP-020` | Entity legacy `Trx*`; **jangan ditiru** modul baru |
| `TrxPatientAssessment` | **`Diperbarui`** — nilai enum + 3 kolom isian medis | `ClinicalManagement` | `CAP-022` | Dibagi dengan `keperawatan` |
| `TrxPatientIntegratedProgressNote` | **`Diperbarui`** | `ClinicalManagement` | `CAP-021` | Kontraknya milik sub-modul ini |
| `TrxPatientProcedure` | **`Diperbarui`** | `ClinicalManagement` | `CAP-024` | — |
| **`CliPhysicianVisit`** | **`Baru`** | `ClinicalManagement` | `CAP-025` | Memakai prefix registry `Cli` — **bukan** `Trx*` |
| `TrxPrescription` | **`Diperbarui`** | `PharmacyManagement` | `CAP-023` | Status pemenuhan **hanya dibaca** |
| `LabOrder` | **`Diperbarui`** | `LaboratoryManagement` | `CAP-015` | — |
| `RadOrder` | **`Diperbarui`** | `RadiologyManagement` | `CAP-015` | **Baru masuk kamus pada `0.2`** |
| `MrcClinicalDocumentIntegrity` | `Sudah ada` | `MedicalRecordManagement` | Koreksi dokumen | **Nol perubahan** |
| `MrcClinicalNoteAddendum` | `Sudah ada` | `MedicalRecordManagement` | Koreksi dokumen | **Nol perubahan** |
| `MrcClinicalNoteAuthorDelegation` | `Sudah ada` | `MedicalRecordManagement` | Penulis pengganti | **Nol perubahan** |
| `CliClinicalMilestoneFact` | `Sudah ada` | `ClinicalManagement` | Fakta ke Billing | **Nol perubahan** |
| `TrxPatientDiagnosis` | **`Diperbarui`** ★ `0.4` | `ClinicalManagement` | `CAP-022` aturan 5 | Naik dari `Sudah ada`. Satu kolom ditambah, satu kewajiban dilonggarkan — bagian 10.1. **MUST NOT** disalin |
| `InpEpisode`, `InpDoctorAssignment` | `Sudah ada` | `InPatientManagement` | Konteks dan kewenangan | Direferensikan, **MUST NOT** disalin |

---

## 2. `TrxDoctorConsultation` — `Diperbarui`

Kolom yang sudah ada tidak diulang; rujukannya
`Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, ClinicalDateTime)` | FK ke `InpEpisode` | `Restrict` | Tidak | Konteks episode |
| `ClinicalDateTime` | `timestamptz` | Tidak | `null` | bagian index di atas | — | — | Tidak | **Waktu klinis**, berbeda dari waktu penulisan |
| `PhysicianVisitId` | `uuid` | Tidak | `null` | Index | FK ke `CliPhysicianVisit` | `SetNull` | Tidak | Tautan **opsional** ke event visite. Berganti nama dari `VisitId` |

### 2.1 Kolom lama yang berubah artinya

| Kolom | Yang berubah |
| --- | --- |
| `QueueId` | Bentuknya **tidak berubah** — sudah boleh kosong. Yang berubah: kapan ia boleh kosong, kini juga saat kunjungan punya episode rawat inap yang berjalan |

### 2.2 Kolom lama yang sensitif

`Subjective`, `Objective`, dan seluruh kolom rencana tindakan, resep, penunjang, rujukan, serta
edukasi.

> **`PhysicianVisitId` memakai `SetNull`, bukan `Restrict`.** Event visite yang dibatalkan tidak
> boleh menyeret catatan SOAP-nya. `INV-DOK-07`: keduanya memang tidak terikat mati.

---

## 3. `TrxPatientAssessment` — `Diperbarui`

| Yang berubah | Isinya |
| --- | --- |
| Kolom baru dari sub-modul ini | **Tiga** — isian medis kajian DPJP |
| Jenis kajian | Bertambah nilai kajian medis awal dan kajian medis ulang |

> **Perubahan keputusan struktur, 5 September 2026.** Revisi sebelumnya menyatakan sub-modul ini
> menambahkan **nol** kolom pada tabel ini. Keputusan itu **diganti** Product/Domain bersama
> pemilik `ClinicalManagement` — jalan **A** pada
> [`../02-backend-architecture.md`](../02-backend-architecture.md) bagian 4.2 diambil, dan
> `TrxPatientAssessment` memperoleh tiga kolom isian medis.
>
> **Alasannya.** `VAL-DOK-10` menolak penyelesaian kajian ketika pemeriksaan atau rencana kosong,
> dan `VAL-DOK-11` menolaknya ketika diagnosis kosong. Selama ketiganya tidak punya kolom, kedua
> aturan itu **tidak dapat ditegakkan** — dan `BE-RWI-045` kriteria 4 tidak dapat dibuktikan.
> Dua jalan lain ditolak: menggantungkan **seluruh isian kajian** pada `TrxPatientDiagnosis`
> menuntut `ConsultationId` dilonggarkan, yaitu menyentuh tabel yang sedang dipakai poliklinik, dan
> tetap tidak menyediakan tempat bagi pemeriksaan fisik maupun rencana terapi; sedangkan tabel
> kajian medis tersendiri berarti menyalin puluhan kolom yang sudah ada di sini.
>
> **Catatan revision `0.4`.** Keberatan "menyentuh tabel yang dipakai poliklinik" **dijawab**, bukan
> dibatalkan, oleh `INT-DOK-10`: pelonggarannya hanya berlaku pada kunjungan bertipe `Inpatient`,
> jalur poliklinik tetap menuntut nomor konsultasi dengan kalimat penolakan yang sama persis
> (`VAL-DOK-38`), dan nol baris lama berubah. Keberatan kedua — tidak ada tempat bagi pemeriksaan
> fisik dan rencana terapi — **tetap berlaku dan tetap benar**, dan itulah sebabnya ketiga kolom di
> bawah tidak dicabut.

### 3.0 Kolom baru — isian medis kajian DPJP

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `PhysicalExamination` | `varchar(2000)` | Tidak | `null` | — | — | — | **Ya** | Pemeriksaan fisik naratif oleh DPJP |
| `WorkingDiagnosis` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Diagnosis kerja pada kajian medis awal |
| `TherapyPlan` | `varchar(2000)` | Tidak | `null` | — | — | — | **Ya** | Rencana terapi |

Ketiganya **nullable**, sehingga seluruh baris pengkajian keperawatan, poliklinik, dan IGD yang
sudah ada tidak perlu disentuh sama sekali. Wajibnya ditegakkan **aturan bisnis saat penyelesaian
kajian medis**, bukan oleh `NOT NULL` — pengkajian keperawatan memang tidak mengisinya.

`WorkingDiagnosis` **bukan pengganti** `TrxPatientDiagnosis`. Diagnosis berkode ICD tetap tinggal
di sana; kolom ini menampung diagnosis kerja naratif pada saat pemeriksaan pertama.

> **Diperbarui pada revision `0.4`.** Kalimat asli berbunyi diagnosis berkode ICD "tetap menggantung
> pada catatan dokter". Sejak `INT-DOK-10` itu **tidak lagi seluruhnya benar**: diagnosis
> terstruktur kini boleh menggantung pada **perawatan rawat inap** ketika catatan dokter memang
> belum ada. Yang tidak berubah adalah pembagian perannya — teks bebas untuk narasi, daftar
> terstruktur untuk kode ICD yang dapat dicari dan dinyatakan teratasi. Rinciannya pada bagian 10.1.

Migration: `20260905081108_AddMedicalAssessmentContentColumns`.

Kolom `InpEpisodeId`, `DueAt`, dan `PolicyId` **sudah diminta** `keperawatan` dan dipakai apa adanya
di sini. Rinciannya di [`../../keperawatan/data/data-dictionary.md`](../../keperawatan/data/data-dictionary.md).

### 3.1 Kolom kunci yang sudah ada

| Kolom | Dipakai untuk |
| --- | --- |
| `EncounterId` | Jangkar klinis; **wajib** |
| `QueueId` | Sudah boleh kosong sejak jalur IGD dibuka |
| `AssessmentStatus` | `Draft`, `InProgress`, `Completed`, `Cancelled` |
| `DoctorId` | Membuktikan tabel ini memang tidak pernah menjadi milik perawat saja |

> Berbagi satu tabel adalah keputusan struktur yang **sudah disetujui pemilik** 5 September 2026
> — `../02-backend-architecture.md` bagian 4.2 jalan A. Harganya tetap seperti yang dicatat di
> sana: mesin hak akses hanya melihat satu sumber daya untuk dua jenis dokumen, sehingga
> pembedaan kajian medis dan pengkajian keperawatan bersandar sepenuhnya pada aturan bisnis.

---

## 4. `TrxPatientIntegratedProgressNote` — `Diperbarui`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, NoteDateTime)` | FK ke `InpEpisode` | `Restrict` | Tidak | Konteks episode |
| `VerificationStatus` | `integer` | **Ya** | `NotRequired` | Index | — | — | Tidak | Enum, disimpan sebagai integer |
| `VerifiedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | Waktu verifikasi |
| `VerifiedByUserId` | `uuid` | Tidak | `null` | — | FK ke pengguna | `Restrict` | Tidak | **Terpisah dari penulis asli** |
| `VerificationDueAt` | `timestamptz` | Tidak | `null` | Index parsial pada yang menunggu | — | — | Tidak | Batas waktu verifikasi |

`CpptVerificationStatus`: `NotRequired`, `Pending`, `Verified`, `Overdue`. Bawaan `NotRequired`.

> **`VerifiedByUserId` sengaja terpisah dari penulis.** Itulah yang membuat `AC-CAP021-03` dapat
> dibuktikan: verifikator **tidak pernah** menggantikan penulis asli.
>
> **Index parsial hanya pada yang menunggu**, karena daftar pantau hanya membaca baris itu.
> Meng-index seluruh baris memboroskan tanpa dipakai.
>
> **Tiga kolom amandemen dari revision `0.1` dicabut.** Alasan, penulis, dan nomor urut koreksi
> sudah dipegang `MrcClinicalNoteAddendum`.

---

## 5. `TrxPatientProcedure` — `Diperbarui`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, PerformedAt)` | FK ke `InpEpisode` | `Restrict` | Tidak | Konteks episode |
| `PhysicianVisitId` | `uuid` | Tidak | `null` | Index | FK ke `CliPhysicianVisit` | `SetNull` | Tidak | Tautan **opsional** ke event visite |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | **Unique parsial** | — | — | Tidak | Mencegah tindakan dan tagihan ganda |

### 5.1 Kolom kunci yang sudah ada dan tidak jadi ditambah

| Kolom | Kenapa cukup |
| --- | --- |
| `ProcedureStatus` | Sudah memuat `Planned`, `Ordered`, `InProgress`, `Completed`, `Cancelled` — perbedaan rencana dan pelaksanaan sudah terwakili |
| `IsExecuted`, `ExecutedAt`, `PerformedAt` | Menjawab kapan tindakan benar-benar dikerjakan |
| `IsBillingGenerated`, `BillingGeneratedAt`, `BillingItemId` | Menjawab keadaan penagihan tanpa kolom status keempat |

---

## 6. `CliPhysicianVisit` — `Baru`

Satu-satunya tabel yang benar-benar baru pada sub-modul ini.

| Aspek | Nilai |
| --- | --- |
| Nama tabel | `public."CliPhysicianVisit"` |
| `DbSet` | `CliPhysicianVisits` |
| Configuration | `Repositories/Configurations/HealthServices/ClinicalManagement/CliPhysicianVisitConfiguration.cs` |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PhysicianVisitNumber` | `varchar(30)` | Ya | — | **Unique** | — | — | Tidak | Nomor bisnis terbaca manusia, dialokasikan service |
| `EncounterId` | `uuid` | Ya | — | Index | FK ke kunjungan | `Restrict` | Tidak | Jangkar klinis |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, VisitDateTime)` | FK ke `InpEpisode` | `Restrict` | Tidak | Konteks episode |
| `PatientId` | `uuid` | Ya | — | Index | FK ke pasien | `Restrict` | Tidak | Penjaga salah pasien |
| `DoctorId` | `uuid` | Ya | — | `(DoctorId, VisitDateTime)` | FK ke dokter | `Restrict` | Tidak | Subjek fakta |
| `VisitDateTime` | `timestamptz` | Ya | — | bagian dua index di atas | — | — | Tidak | **Waktu kedatangan**, bukan waktu pencatatan |
| `VisitRole` | `integer` | Ya | `Dpjp` | — | — | — | Tidak | Enum peran dokter saat visite |
| `VisitStatus` | `integer` | Ya | `Recorded` | Index | — | — | Tidak | Enum; menjaga `INV-DOK-08` |
| `ConsultationId` | `uuid` | Tidak | `null` | — | FK ke catatan dokter | `SetNull` | Tidak | Tautan **opsional** |
| `ProgressNoteId` | `uuid` | Tidak | `null` | — | FK ke CPPT | `SetNull` | Tidak | Tautan **opsional** |
| `PatientProcedureId` | `uuid` | Tidak | `null` | — | FK ke tindakan | `SetNull` | Tidak | Tautan **opsional** |
| `Note` | `varchar(1000)` | Tidak | `null` | — | — | — | **Ya** | Catatan singkat dokter |
| `RecordedByUserId` | `uuid` | Ya | — | — | FK ke pengguna | `Restrict` | Tidak | Pelaku pencatatan |
| `IdempotencyKey` | `varchar(100)` | **Ya** | — | **Unique** | — | — | Tidak | Kunci permintaan; **wajib** sejak `0.2` |
| `CancelledAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | Waktu pembatalan |
| `CancelledByUserId` | `uuid` | Tidak | `null` | — | FK ke pengguna | `Restrict` | Tidak | Pelaku pembatalan |
| `CancelReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | **Wajib diisi saat membatalkan** |
| `CorrectsVisitId` | `uuid` | Tidak | `null` | Index | FK ke `CliPhysicianVisit` | `Restrict` | Tidak | Event yang digantikan setelah koreksi |

`PhysicianVisitRole`: `Dpjp`, `Consultant`, `OnCall`. Bawaan `Dpjp`.
`PhysicianVisitStatus`: `Recorded`, `Cancelled`. Bawaan `Recorded`.

> **Ketiga tautan dokumen nullable dan `SetNull`.** `INV-DOK-07`: satu event tidak wajib punya
> catatan, dan satu catatan tidak wajib punya event. Membuat salah satunya wajib menghidupkan
> kembali aturan lama yang sudah `superseded`.
>
> **Unique penuh pada kunci permintaan, bukan unique parsial.** Kuncinya kini wajib terisi, dan
> kunci milik event yang **sudah dibatalkan pun tidak boleh dipakai ulang** — bila boleh, sebuah
> kiriman ulang lama dapat menghidupkan kembali event yang sengaja dibatalkan.
>
> **Tidak ada unique pada pasangan episode, dokter, dan tanggal.** `RWI-DEC-085`: dokter yang
> benar-benar datang dua kali pada hari yang sama menghasilkan **dua** event.

---

## 7. `TrxPrescription` — `Diperbarui` — milik `PharmacyManagement`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Konteks episode |
| `PrescriptionOrderType` | `integer` | **Ya** | `Routine` | Index | — | — | Tidak | Enum jenis resep |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | **Unique parsial** | — | — | Tidak | Mencegah resep ganda |

`PrescriptionOrderType`: `Routine`, `Daily`, `Discharge`. Bawaan `Routine`.

### 7.1 Kolom yang hanya dibaca sub-modul ini

| Kolom | Kenapa hanya dibaca |
| --- | --- |
| `PrescriptionStatus`, `PaymentStatus`, `FulfillmentStatus` | `RUL-DOK-01`. Rawat Inap **tidak pernah** menandai obat sudah diserahkan |
| `ConsultationId` | **Wajib** dan tetap wajib. Resep memang lahir dari catatan dokter |

---

## 8. `LabOrder` — `Diperbarui` — milik `LaboratoryManagement`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, CreateDateTime)` | FK ke `InpEpisode` | `Restrict` | Tidak | Konteks episode |

Satu kolom. Pesanan sudah terikat kunjungan tanpa antrean maupun catatan dokter, sehingga pemesanan
lab rawat inap sudah mungkin hari ini; kolom ini yang membuat `AC-CAP015-01` dapat dibuktikan.

### 8.1 Yang hanya dibaca

`OrderStatus`, hasil, spesimen, dan riwayat transisi — `RUL-DOK-02`, `AC-CAP015-02`.

---

## 9. `RadOrder` — `Diperbarui` — milik `RadiologyManagement` ★ baru pada `0.2`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, CreateDateTime)` | FK ke `InpEpisode` | `Restrict` | Tidak | Konteks episode |

### 9.1 Kolom kunci yang sudah ada

| Kolom | Dipakai untuk |
| --- | --- |
| `EncounterId` | Jangkar klinis; daftar pesanan **sudah** dapat disaring dengannya |
| `ModalityId` | Jenis pencitraan |
| `OrderStatus` | Lifecycle pesanan milik modul Radiologi |

### 9.2 Yang hanya dibaca

`OrderStatus`, studi, dan hasil — `RUL-DOK-02`.

---

## 10. Tabel `Sudah ada` — kolom kunci saja

### `MrcClinicalDocumentIntegrity` — `MedicalRecordManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `DocumentKind`, `DocumentId` | Menautkan mesin integritas ke dokumen mana pun tanpa foreign key langsung |
| `IntegrityStatus` | `Draft`, `Signed`, `LockedUnsigned`, `Cancelled` |
| `LockedAt`, `LockTrigger` | Kapan dan kenapa dokumen terkunci |

Rujukan model: `Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalDocumentIntegrity.cs`

### `MrcClinicalNoteAddendum` — `MedicalRecordManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `IntegrityId`, `Sequence` | Nomor urut koreksi pada satu dokumen |
| `AuthorUserId`, `IsSubstituteAuthor`, `DelegationId` | Siapa yang mengoreksi dan atas dasar apa |
| `CorrectionReason` | **Alasan koreksi** — menggantikan kolom `AmendReason` yang tidak jadi dibuat |

### `CliClinicalMilestoneFact` — `ClinicalManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `EncounterId`, `EffectType` | Peristiwa klinis apa, pada kunjungan mana |
| `IdempotencyKey` | Mencegah tagihan ganda saat kiriman diulang |

### `TrxPatientDiagnosis` — `ClinicalManagement` — **`Diperbarui`** ★ `0.4`

Tabel ini pindah dari daftar "kolom kunci saja" menjadi tabel `Diperbarui`. Rinciannya pada 10.1.

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `EncounterId` | Kunjungan tempat diagnosis dicatat. **Tetap wajib** |
| `ConsultationId` | Catatan dokter yang menaungi diagnosis. **Menjadi boleh kosong pada `0.4`** |
| `InpEpisodeId` | **Kolom baru `0.4`** — perawatan rawat inap yang menaungi diagnosis dari kajian medis |
| `DiagnosisCode`, `DiagnosisName`, `IcdVersion` | Kode ICD dan namanya; inilah yang membedakannya dari teks bebas |
| `DiagnosisType`, `IsPrimary` | Daftar masalah terstruktur |
| `DiagnosisStatus`, `ResolvedAt`, `CancelledAt` | Masalah dapat dinyatakan teratasi atau dibatalkan beralasan, tanpa dihapus |

#### 10.1 Kolom yang berubah pada revision `0.4`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | Ya, bersama `PatientId` | `InpEpisode` | `Restrict` | Tidak | **Baru.** Konteks perawatan bagi diagnosis yang lahir dari kajian medis. Nama dan polanya sama persis dengan kolom sejenis pada empat tabel klinis lain sejak `BE-RWI-040` |
| `ConsultationId` | `uuid` | **Tidak** — turun dari **Ya** | `null` | Ya | `TrxDoctorConsultation` | `Restrict` | Tidak | **Dilonggarkan.** Seluruh baris lama sudah terisi dan **tidak disentuh**; melepas kewajiban terisi tidak mengubah satu nilai pun |

**Kolom sensitif pada tabel ini, didaftarkan pada `0.4`:** `ClinicalNote`, `AssessmentNote`,
`PlanNote`, `DifferentialDiagnosisNote`, `SupportingFindingNote`, `ResolvedReason`, dan
`CancelReason`. Ketujuhnya **MUST NOT** masuk payload custom logger —
[`../contracts/permission-audit-matrix.md`](../contracts/permission-audit-matrix.md) bagian 5.

> **Kewajibannya tidak hilang, ia berpindah tempat.** Tidak ada satu kolom pun yang selalu terisi
> pada kedua jalur, sehingga "salah satu wajib" tidak dapat ditegakkan `NOT NULL`. Penjagaannya
> pindah ke aturan bisnis `VAL-DOK-36`. Konsekuensinya jujur: **basis data tidak lagi menjadi
> jaring pengaman terakhir** bagi diagnosis tanpa konteks, dan itulah sebabnya
> `testing/acceptance-test-matrix.md` bagian 11 menguji kasus keduanya kosong secara khusus.
>
> **Langkah mundurnya tidak simetris** — `02-backend-architecture.md` bagian 7.3 langkah 11.

### `InpDoctorAssignment` — `InPatientManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `EpisodeId`, `DoctorId` | Menjawab siapa DPJP |
| `StartDateTime`, `EndDateTime` | **Berperiode** — kewenangan pada tanggal tertentu, bukan penugasan terkini |

### `InpEpisode` — `InPatientManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `EncounterId`, `PatientId` | Jembatan ke mesin klinis |
| `EpisodeStatus` | `INV-DOK-01` s.d. `INV-DOK-03` |

---

## 11. Skema DDL

> **Peringatan.** Bagian ini **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Skema
> sungguhan lahir dari EF Core migration milik modul pemiliknya, dan menjalankan skrip ini akan
> berbenturan dengan migration. Kolom warisan `IdentityModel` tidak ditulis ulang di sini.

```sql
-- Catatan dokter dan SOAP
ALTER TABLE public."TrxDoctorConsultation" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE public."TrxDoctorConsultation" ADD COLUMN "ClinicalDateTime" timestamptz NULL;
ALTER TABLE public."TrxDoctorConsultation" ADD COLUMN "PhysicianVisitId" uuid NULL;
CREATE INDEX "IX_TrxDoctorConsultation_Episode_ClinicalTime"
    ON public."TrxDoctorConsultation" ("InpEpisodeId", "ClinicalDateTime");

-- CPPT: konteks episode dan verifikasi DPJP
ALTER TABLE public."TrxPatientIntegratedProgressNote" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE public."TrxPatientIntegratedProgressNote" ADD COLUMN "VerificationStatus" integer NOT NULL DEFAULT 0;
ALTER TABLE public."TrxPatientIntegratedProgressNote" ADD COLUMN "VerifiedAt" timestamptz NULL;
ALTER TABLE public."TrxPatientIntegratedProgressNote" ADD COLUMN "VerifiedByUserId" uuid NULL;
ALTER TABLE public."TrxPatientIntegratedProgressNote" ADD COLUMN "VerificationDueAt" timestamptz NULL;
CREATE INDEX "IX_Cppt_PendingVerification"
    ON public."TrxPatientIntegratedProgressNote" ("VerificationDueAt")
    WHERE "VerificationStatus" = 1 AND "IsDelete" = false;

-- Tindakan dokter
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "PhysicianVisitId" uuid NULL;
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "IdempotencyKey" varchar(100) NULL;
CREATE UNIQUE INDEX "UX_TrxPatientProcedure_IdempotencyKey"
    ON public."TrxPatientProcedure" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND "IsDelete" = false;

-- Event visite dokter: tabel baru, prefix registry Cli
CREATE TABLE public."CliPhysicianVisit" (
    "Id"                    uuid          NOT NULL,
    "PhysicianVisitNumber"  varchar(30)   NOT NULL,
    "EncounterId"           uuid          NOT NULL,
    "InpEpisodeId"          uuid          NULL,
    "PatientId"             uuid          NOT NULL,
    "DoctorId"              uuid          NOT NULL,
    "VisitDateTime"         timestamptz   NOT NULL,
    "VisitRole"             integer       NOT NULL,   -- enum, HasConversion<int>
    "VisitStatus"           integer       NOT NULL,   -- enum, HasConversion<int>
    "ConsultationId"        uuid          NULL,
    "ProgressNoteId"        uuid          NULL,
    "PatientProcedureId"    uuid          NULL,
    "Note"                  varchar(1000) NULL,       -- SENSITIF
    "RecordedByUserId"      uuid          NOT NULL,
    "IdempotencyKey"        varchar(100)  NOT NULL,
    "CancelledAt"           timestamptz   NULL,
    "CancelledByUserId"     uuid          NULL,
    "CancelReason"          varchar(500)  NULL,       -- SENSITIF
    "CorrectsVisitId"       uuid          NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_CliPhysicianVisit" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CliPhysicianVisit_InpEpisode_InpEpisodeId"
        FOREIGN KEY ("InpEpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CliPhysicianVisit_CliPhysicianVisit_CorrectsVisitId"
        FOREIGN KEY ("CorrectsVisitId") REFERENCES public."CliPhysicianVisit" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "UX_CliPhysicianVisit_IdempotencyKey"
    ON public."CliPhysicianVisit" ("IdempotencyKey");
CREATE UNIQUE INDEX "UX_CliPhysicianVisit_PhysicianVisitNumber"
    ON public."CliPhysicianVisit" ("PhysicianVisitNumber");
CREATE INDEX "IX_CliPhysicianVisit_Episode_VisitTime"
    ON public."CliPhysicianVisit" ("InpEpisodeId", "VisitDateTime");
CREATE INDEX "IX_CliPhysicianVisit_Doctor_VisitTime"
    ON public."CliPhysicianVisit" ("DoctorId", "VisitDateTime");

-- Resep
ALTER TABLE public."TrxPrescription" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE public."TrxPrescription" ADD COLUMN "PrescriptionOrderType" integer NOT NULL DEFAULT 0;
ALTER TABLE public."TrxPrescription" ADD COLUMN "IdempotencyKey" varchar(100) NULL;
CREATE UNIQUE INDEX "UX_TrxPrescription_IdempotencyKey"
    ON public."TrxPrescription" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND "IsDelete" = false;

-- Pesanan laboratorium dan radiologi
ALTER TABLE public."LabOrder" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE public."RadOrder" ADD COLUMN "InpEpisodeId" uuid NULL;
```

---

## 12. Tabel yang tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Tabel `Inp*` apa pun untuk dokumentasi dokter | `RWI-DEC-081` |
| **Tabel baru berawalan `Trx*`** | `QBE-NAM-001` |
| Tabel SOAP tersendiri | Isi SOAP sudah berada di dalam catatan dokter |
| Bentuk penyimpanan kajian medis tersendiri | Menyalin puluhan kolom; lihat `../02-backend-architecture.md` bagian 4.2 |
| **Tabel maupun kolom amandemen** | Mesin addendum `MedicalRecordManagement` sudah menyimpannya |
| Salinan hasil laboratorium maupun radiologi | `RUL-DOK-02`, `AC-CAP015-02` |
| Kolom status penyerahan obat milik Rawat Inap | `RUL-DOK-01` |
| Tabel hitungan visite harian | Hitungan diturunkan dari event; menyimpannya melahirkan angka kedua — `RWI-DEC-085` |

---

## 13. Amandemen revision `0.5` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

Seluruh tabel pada bagian ini mewarisi `IdentityModel` sebagaimana kepala dokumen. Kolom audit tidak
diulang. Untuk tabel `Diperbarui` yang kolomnya sangat banyak, bagian ini mengikuti preseden bagian 7:
yang ditulis kolom baru dan kolom yang berubah; sumber lengkap kolom lain adalah berkas model yang
disebut. Kolom yang tidak disebut **tidak berubah**.

### 13.0 Status dan kepemilikan tabel pada `0.5`

| Entity | Status | Owner | Kemampuan | Catatan |
| --- | --- | --- | --- | --- |
| `PhmPrescription` | `Sudah ada` | `PharmacyManagement` | `CAP-023-RSP` | Nama yang benar bagi `TrxPrescription` pada bagian 1 dan 7 — `RWI-DEC-132` butir (6). Nol kolom baru pada `0.5` |
| `PhmPrescriptionItem` | **`Diperbarui`** | `PharmacyManagement` | `CAP-023-RSP` | Penghentian butir dan jenis dosis — 13.1 |
| `PhmMedicationReconciliationItem` | **`Baru`** | `PharmacyManagement` | `CAP-023-RSP` | Obat bawaan — 13.2 |
| `PhmMedicationReconciliationDecision` | **`Baru`** | `PharmacyManagement` | `CAP-023-RSP` | Riwayat keputusan dokter — 13.3 |
| `PhmSlidingScaleTemplate` | **`Baru`** | `PharmacyManagement` | `CAP-023-RSP` | 13.4 |
| `PhmSlidingScaleTemplateVersion` | **`Baru`** | `PharmacyManagement` | `CAP-023-RSP` | 13.5 |
| `PhmSlidingScaleRange` | **`Baru`** | `PharmacyManagement` | `CAP-023-RSP` | Dipakai versi template **atau** versi order — 13.6 |
| `PhmSlidingScaleOrder` | **`Baru`** | `PharmacyManagement` | `CAP-023-RSP` | 13.7 |
| `PhmSlidingScaleOrderVersion` | **`Baru`** | `PharmacyManagement` | `CAP-023-RSP` | 13.8 |
| `TrxPatientIntegratedProgressNote` | **`Diperbarui`** | `ClinicalManagement` | `CAP-021` | `NoteKind` — 13.9 |
| `TrxPatientProcedure` | **`Diperbarui`** | `ClinicalManagement` | `CAP-024` | Pesanan dan instruksi — 13.10 |
| `LabOrder` | **`Diperbarui`** | `LaboratoryManagement` | `CAP-015-LAB` | Gerbang persetujuan pemilik — 13.11 |
| `RadOrder` | **`Diperbarui`** | `RadiologyManagement` | `CAP-015-RAD` | Gerbang persetujuan pemilik — 13.11 |
| `MstPrescriptionTemplate` | `Sudah ada` | `PharmacyManagement` | `CAP-023-RSP` | Perilaku berubah, bentuk tidak — 13.12 |
| `MstDrug` | `Sudah ada` | `MasterData` | `CAP-023-RSP` | `IsFormulary` dipaksa `false` pada jalur non-formularium — 13.12 |
| `MrcClinicalDocumentIntegrity` | `Sudah ada` | `MedicalRecordManagement` | Catatan Saya, penguncian | Nol perubahan bentuk — 13.12 |
| `PhmMedicationAdministration`, `CliBloodGlucoseReading` | `Baru` — **dirancang `keperawatan`** | `PharmacyManagement`, `ClinicalManagement` | `CAP-023-MAR`, `CAP-012` | Dirujuk, tidak dirancang di sini |
| `InpDoctorAssignment`, `InpDischargeSummary` | `Diperbarui` — **dirancang `episode-rawat-inap`** | `InPatientManagement` | Konteks, `CAP-026` | Dirujuk, tidak dirancang di sini |

### 13.1 `PhmPrescriptionItem` — `Diperbarui` — sumber lengkap `Areas/HealthServices/PharmacyManagement/Models/PhmPrescriptionItem.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `IsStopped` | `boolean` | Ya | `false` | Index bersama `PrescriptionId` | — | — | Tidak | `true` bila dokter menghentikan butir dari Resep Harian — `RWI-DEC-121` |
| `StoppedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | Waktu penghentian. Wajib terisi bila `IsStopped = true` |
| `StoppedByUserId` | `uuid` | Tidak | `null` | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Dokter penghenti, **dari akun login** |
| `StopReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib bila `IsStopped = true`. Contoh: "kultur sensitif, ganti oral" |
| `DoseKind` | `integer` | Ya | `0` `Fixed` | — | — | — | Tidak | `0` `Fixed`, `1` `SlidingScale`. `SlidingScale` mewajibkan tepat satu `PhmSlidingScaleOrder` |

**Kolom yang tetap hanya dibaca:** `Dose`, `FrequencyCode`, `IsHighAlertSnapshot`, `IsFormularySnapshot` — dipakai MAR dan
rekonsiliasi, tidak diubah jalur penghentian.

### 13.2 `PhmMedicationReconciliationItem` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EncounterId` | `uuid` | Ya | — | Index | FK ke `RegPatientEncounter` | `Restrict` | Tidak | Jangkar kunjungan |
| `InpEpisodeId` | `uuid` | **Ya** | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Rekonsiliasi pada rilis ini hanya saat admisi rawat inap |
| `PatientId` | `uuid` | Ya | — | Index | FK ke `MstPatient` | `Restrict` | Tidak | Penjaga salah pasien |
| `DrugId` | `uuid` | **Ya** | — | Index | FK ke `MstDrug` | `Restrict` | Tidak | **Tidak ada nama obat teks bebas** — `RWI-DEC-134` |
| `DrugNameSnapshot` | `varchar(250)` | Ya | — | — | — | — | Tidak | Nama obat saat dicatat |
| `IsFormularySnapshot` | `boolean` | Ya | — | — | — | — | Tidak | Salinan `MstDrug.IsFormulary` saat dicatat |
| `Dose` | `numeric(12,4)` | Tidak | `null` | — | — | — | Tidak | Contoh `10` |
| `DoseUnitMeasurementId` | `uuid` | Tidak | `null` | — | FK ke `MstMeasurement` | `Restrict` | Tidak | Contoh mg |
| `DrugFormSnapshot` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | Contoh tablet |
| `FrequencyText` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | Contoh "1×1 pagi" |
| `Route` | `integer` | Ya | — | — | — | — | Tidak | `HomeMedicationRoute`. Isian V1 `RWI-FACT-032` |
| `Note` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Catatan perawat |
| `RecordedAt` | `timestamptz` | Ya | — | Index | — | — | Tidak | Waktu pencatatan |
| `RecordedByEmployeeId` | `uuid` | Ya | — | — | FK ke `MstEmployee` | `Restrict` | Tidak | Perawat pencatat, **dari akun login** |
| `RecordedByUserId` | `uuid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Akun pencatat |
| `CurrentDecision` | `integer` | Ya | `0` `Pending` | Index | — | — | Tidak | Salinan keputusan terakhir; sumbernya baris 13.3 terbaru |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | **Unique parsial** `WHERE "IdempotencyKey" IS NOT NULL AND "IsDelete" = false` | — | — | Tidak | Simpan dua kali tidak melahirkan dua baris |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | `false` bila baris salah catat dibatalkan perawat sebelum ada keputusan |

### 13.3 `PhmMedicationReconciliationDecision` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReconciliationItemId` | `uuid` | Ya | — | Unique bersama `SequenceNumber` | FK ke `PhmMedicationReconciliationItem` | `Restrict` | Tidak | Obat bawaan yang diputuskan |
| `SequenceNumber` | `integer` | Ya | — | Unique bersama `ReconciliationItemId` | — | — | Tidak | Mulai 1 |
| `DecisionType` | `integer` | Ya | — | — | — | — | Tidak | `1` `ContinueSame`, `2` `ContinueModified`, `3` `Stopped`. `0` tidak pernah disimpan di tabel ini |
| `DecisionNote` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Contoh "pasien dipuasakan" |
| `DecidedByDoctorId` | `uuid` | Ya | — | Index | FK ke `MstDoctor` | `Restrict` | Tidak | Dokter pemutus |
| `DecidedByUserId` | `uuid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Akun login |
| `DecidedAt` | `timestamptz` | Ya | — | — | — | — | Tidak | Waktu keputusan |
| `ResultPrescriptionId` | `uuid` | Tidak | `null` | Index | FK ke `PhmPrescription` | `Restrict` | Tidak | Draft resep yang diisi. Kosong untuk `Stopped` |
| `ResultPrescriptionItemId` | `uuid` | Tidak | `null` | Index | FK ke `PhmPrescriptionItem` | `Restrict` | Tidak | Butir draft yang lahir. Kosong untuk `Stopped` |
| `SupersedesDecisionId` | `uuid` | Tidak | `null` | — | FK ke tabel ini | `Restrict` | Tidak | Keputusan yang digantikan, bila dokter mengganti keputusan selama draft |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | Baris tidak pernah dinonaktifkan |

### 13.4 `PhmSlidingScaleTemplate` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `TemplateCode` | `varchar(50)` | Ya | — | **Unique** | — | — | Tidak | Contoh `SSI-DEWASA` |
| `TemplateName` | `varchar(200)` | Ya | — | — | — | — | Tidak | Contoh "Sliding Scale Insulin Dewasa" |
| `Description` | `varchar(500)` | Tidak | `null` | — | — | — | Tidak | Kelompok pasien yang dituju |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | Template nonaktif tidak dapat dipesan |

### 13.5 `PhmSlidingScaleTemplateVersion` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `TemplateId` | `uuid` | Ya | — | Unique bersama `VersionNumber` | FK ke `PhmSlidingScaleTemplate` | `Restrict` | Tidak | Template induk |
| `VersionNumber` | `integer` | Ya | — | Unique bersama `TemplateId` | — | — | Tidak | Mulai 1, naik setiap draft baru |
| `VersionStatus` | `integer` | Ya | `1` `Draft` | **Unique parsial** `("TemplateId") WHERE "VersionStatus" = 2 AND "IsDelete" = false` | — | — | Tidak | Tepat satu versi `Approved` per template |
| `GlucoseUnit` | `integer` | Ya | — | — | — | — | Tidak | `1` mg/dL, `2` mmol/L — gate `G-25`. Tidak berbawaan, wajib dipilih |
| `DefinitionHash` | `char(64)` | Tidak | `null` | — | — | — | Tidak | SHA-256 atas rentang yang disahkan; diisi saat pengesahan |
| `LastModifiedByUserId` | `uuid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Pengubah terakhir. **Tidak boleh** menjadi pengesah |
| `LastModifiedAt` | `timestamptz` | Ya | — | — | — | — | Tidak | — |
| `ApprovedByUserId` | `uuid` | Tidak | `null` | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Pengesah |
| `ApprovedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |
| `RetiredAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | Diisi saat versi sah berikutnya disahkan |
| `ApprovalNote` | `varchar(500)` | Tidak | `null` | — | — | — | Tidak | Catatan pengesah |

### 13.6 `PhmSlidingScaleRange` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `TemplateVersionId` | `uuid` | Tidak | `null` | Index bersama `SortOrder` | FK ke `PhmSlidingScaleTemplateVersion` | `Restrict` | Tidak | Pemilik bila rentang standar |
| `OrderVersionId` | `uuid` | Tidak | `null` | Index bersama `SortOrder` | FK ke `PhmSlidingScaleOrderVersion` | `Restrict` | Tidak | Pemilik bila rentang pasien |
| `LowerBoundInclusive` | `numeric(7,2)` | Tidak | `null` | — | — | — | Tidak | Kosong berarti terbuka ke bawah |
| `UpperBoundExclusive` | `numeric(7,2)` | Tidak | `null` | — | — | — | Tidak | Kosong berarti terbuka ke atas |
| `DoseUnits` | `numeric(6,2)` | Ya | — | — | — | — | Tidak | `≥ 0`. `0` sah untuk rentang tanpa insulin |
| `InstructionText` | `varchar(300)` | Tidak | `null` | — | — | — | Tidak | Contoh "lapor dokter" |
| `RequiresPhysicianNotification` | `boolean` | Ya | `false` | — | — | — | Tidak | Penanda tampilan saja — gate `G-24` |
| `SortOrder` | `integer` | Ya | — | — | — | — | Tidak | Urutan dari batas terendah |

**Check constraint** `CK_PhmSlidingScaleRange_SingleOwner`: `num_nonnulls("TemplateVersionId", "OrderVersionId") = 1`.
Aturan tidak bertumpuk dan tidak berlubang dijaga service saat menyimpan — `VAL-DOK-54`, `VAL-DOK-55` — karena
pemeriksaan antar-baris tidak dapat dinyatakan satu check constraint.

### 13.7 `PhmSlidingScaleOrder` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OrderNumber` | `varchar(30)` | Ya | — | **Unique** | — | — | Tidak | Dialokasikan provider number-series, bukan `Count+1` (`QBE-CODE-002`) |
| `PrescriptionId` | `uuid` | Ya | — | Index | FK ke `PhmPrescription` | `Restrict` | Tidak | Resep induk |
| `PrescriptionItemId` | `uuid` | Ya | — | **Unique parsial** `WHERE "IsDelete" = false` | FK ke `PhmPrescriptionItem` | `Restrict` | Tidak | Butir insulin `DoseKind = SlidingScale` |
| `EncounterId` | `uuid` | Ya | — | Index | FK ke `RegPatientEncounter` | `Restrict` | Tidak | — |
| `InpEpisodeId` | `uuid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | — |
| `PatientId` | `uuid` | Ya | — | Index | FK ke `MstPatient` | `Restrict` | Tidak | — |
| `TemplateId` | `uuid` | Ya | — | Index | FK ke `PhmSlidingScaleTemplate` | `Restrict` | Tidak | Template asal |
| `OrderStatus` | `integer` | Ya | `1` `Active` | Index | — | — | Tidak | `1` `Active`, `2` `Stopped` |
| `CurrentVersionNumber` | `integer` | Ya | `1` | — | — | — | Tidak | Juga dipakai sebagai pemeriksaan benturan: permintaan ubah membawa nomor yang dibacanya |
| `CheckFrequencyCode` | `varchar(30)` | Tidak | `null` | — | — | — | Tidak | **Usulan `G-22`**: jadwal pemeriksaan GDS yang membentuk dosis terjadwal MAR |
| `StoppedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |
| `StoppedByUserId` | `uuid` | Tidak | `null` | — | FK ke `ApplicationUser` | `Restrict` | Tidak | — |
| `StopReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib bila `Stopped` |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | — |

### 13.8 `PhmSlidingScaleOrderVersion` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OrderId` | `uuid` | Ya | — | Unique bersama `VersionNumber` | FK ke `PhmSlidingScaleOrder` | `Restrict` | Tidak | — |
| `VersionNumber` | `integer` | Ya | — | Unique bersama `OrderId` | — | — | Tidak | Mulai 1 |
| `TemplateVersionId` | `uuid` | Ya | — | Index | FK ke `PhmSlidingScaleTemplateVersion` | `Restrict` | Tidak | **Wajib berstatus `Approved` saat dipesan** |
| `IsAdjusted` | `boolean` | Ya | `false` | — | — | — | Tidak | `true` bila rentang atau dosis berbeda dari versi template |
| `AdjustmentReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib bila `IsAdjusted = true`. Contoh "pasien sensitif insulin" |
| `OrderedByDoctorId` | `uuid` | Ya | — | Index | FK ke `MstDoctor` | `Restrict` | Tidak | Dari akun login |
| `OrderedByUserId` | `uuid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | — |
| `OrderedAt` | `timestamptz` | Ya | — | — | — | — | Tidak | — |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | — |

### 13.9 `TrxPatientIntegratedProgressNote` — `Diperbarui` — sumber lengkap `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `NoteKind` | `integer` | Ya | `0` `Unspecified` | Index `("InpEpisodeId", "NoteKind", "NoteDateTime")` | — | — | Tidak | `0` `Unspecified`, `1` `PhysicianNote`, `2` `NursingSoap`, `3` `NursingNarrative`, `4` `OtherProfessionNote` — `RWI-DEC-140` |

Kolom yang sudah ada dan dipakai aturan baru: `ProfessionType` (penentu nilai `NoteKind` yang sah), `VerificationStatus`,
`VerifiedByUserId`, `ProviderUserId` (penulis), `InpEpisodeId`, `NoteDateTime` (waktu klinis).

### 13.10 `TrxPatientProcedure` — `Diperbarui` — sumber lengkap `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `ConsultationId` | `uuid` | **Tidak** ← dari Ya | — | Index | FK ke `TrxDoctorConsultation` | `Restrict` | Tidak | **Dilonggarkan.** Salah satu dari `ConsultationId` atau `InpEpisodeId` wajib — `VAL-DOK-45` |
| `OrderedByUserId` | `uuid` | Tidak | `null` | Index | FK ke `ApplicationUser` | `Restrict` | Tidak | **Penginput pesanan** — `RWI-DEC-139`. Baris lama dibiarkan kosong; penjaga penginput hanya berlaku bagi baris yang terisi, dan baris kosong jatuh ke aturan lama (dokter pemesan = `DoctorId`) |
| `InstructingDoctorId` | `uuid` | Tidak | `null` | Index bersama `InstructionVerificationStatus` | FK ke `MstDoctor` | `Restrict` | Tidak | Terisi hanya pada pesanan yang dibuat perawat |
| `InstructionVerificationStatus` | `integer` | Ya | `0` `NotRequired` | Index | — | — | Tidak | `1` `Pending` saat perawat menyimpan; `2` `Verified` setelah dokter memverifikasi |
| `InstructionVerifiedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |
| `InstructionVerifiedByUserId` | `uuid` | Tidak | `null` | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Harus akun dokter pemberi instruksi |
| `CancelledByEpisodeClosure` | `boolean` | Ya | `false` | — | — | — | Tidak | `true` bila dibatalkan otomatis saat penutupan — `RWI-DEC-143` butir (3) |

Kolom yang sudah ada dan dipakai aturan baru: `ProcedureStatus` (`Planned`/`Ordered` = pesanan tertunda), `IsExecuted`,
`PerformedByUserId` (penulis catatan pelaksanaan), `IsBillingGenerated` (pesanan tertagih tidak dibatalkan otomatis),
`CancelledAt`, `CancelledByUserId`, `CancelReason`.

### 13.11 `LabOrder` dan `RadOrder` — `Diperbarui`

Kolom yang sama ditambahkan pada kedua tabel. Sumber lengkap `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`
dan `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InstructingDoctorId` | `uuid` | Tidak | `null` | Index bersama `InstructionVerificationStatus` | FK ke `MstDoctor` | `Restrict` | Tidak | Pemberi instruksi pada pesanan perawat |
| `InstructionVerificationStatus` | `integer` | Ya | `0` `NotRequired` | Index | — | — | Tidak | Enum milik modul masing-masing |
| `InstructionVerifiedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |
| `InstructionVerifiedByUserId` | `uuid` | Tidak | `null` | — | FK ke `ApplicationUser` | `Restrict` | Tidak | — |

Penginput memakai `RequestedByUserId` yang sudah ada.

### 13.12 Tabel `Sudah ada` yang perilakunya dipakai berbeda — kolom kunci saja

| Tabel | Kolom kunci | Yang berubah pada `0.5` |
| --- | --- | --- |
| `MstPrescriptionTemplate` — `PharmacyManagement/Models/MstPrescriptionTemplate.cs` | `Id`, `OwnerDoctorId`, `IsShared`, `IsActive`, `TotalItemCount` | `OwnerDoctorId` **selalu** dokter akun login pada buat, buat-dari-resep, dan ubah; ruang kerja rawat inap hanya menampilkan `OwnerDoctorId` = dokter login — `RWI-DEC-135` |
| `MstDrug` — `MasterData/Models/MstDrug.cs` | `Id`, `DrugName`, `IsFormulary`, `IsActive`, `IsPrescribable`, `IsHighAlert` | Jalur pendaftaran non-formularium menulis `IsFormulary = false` di server — `RWI-DEC-134` |
| `MrcClinicalDocumentIntegrity` — `MedicalRecordManagement/Models/MrcClinicalDocumentIntegrity.cs` | `DocumentKind`, `DocumentId`, `IntegrityStatus`, `AuthorUserId`, `LockTrigger` | Konsep SOAP dan kajian medis rawat inap terdaftar `Draft` sejak dibuat — `RWI-DEC-144` |

### 13.13 Skema DDL revision `0.5`

> **Peringatan.** Dokumentasi bentuk tabel, bukan skrip yang dijalankan. Skema sungguhan lahir dari EF Core
> migration milik modul pemiliknya. Kolom warisan `IdentityModel` tidak ditulis ulang.

```sql
-- Farmasi: penghentian butir dan jenis dosis
ALTER TABLE public."PhmPrescriptionItem" ADD COLUMN "IsStopped" boolean NOT NULL DEFAULT false;
ALTER TABLE public."PhmPrescriptionItem" ADD COLUMN "StoppedAt" timestamptz NULL;
ALTER TABLE public."PhmPrescriptionItem" ADD COLUMN "StoppedByUserId" uuid NULL;
ALTER TABLE public."PhmPrescriptionItem" ADD COLUMN "StopReason" varchar(500) NULL;          -- SENSITIF
ALTER TABLE public."PhmPrescriptionItem" ADD COLUMN "DoseKind" integer NOT NULL DEFAULT 0;
CREATE INDEX "IX_PhmPrescriptionItem_PrescriptionId_IsStopped"
    ON public."PhmPrescriptionItem" ("PrescriptionId", "IsStopped");

CREATE TABLE public."PhmMedicationReconciliationItem" (
    "Id"                    uuid          NOT NULL,
    "EncounterId"           uuid          NOT NULL,
    "InpEpisodeId"          uuid          NOT NULL,
    "PatientId"             uuid          NOT NULL,
    "DrugId"                uuid          NOT NULL,
    "DrugNameSnapshot"      varchar(250)  NOT NULL,
    "IsFormularySnapshot"   boolean       NOT NULL,
    "Dose"                  numeric(12,4) NULL,
    "DoseUnitMeasurementId" uuid          NULL,
    "DrugFormSnapshot"      varchar(100)  NULL,
    "FrequencyText"         varchar(100)  NULL,
    "Route"                 integer       NOT NULL,
    "Note"                  varchar(500)  NULL,                 -- SENSITIF
    "RecordedAt"            timestamptz   NOT NULL,
    "RecordedByEmployeeId"  uuid          NOT NULL,
    "RecordedByUserId"      uuid          NOT NULL,
    "CurrentDecision"       integer       NOT NULL DEFAULT 0,
    "IdempotencyKey"        varchar(100)  NULL,
    "IsActive"              boolean       NOT NULL DEFAULT true,
    CONSTRAINT "PK_PhmMedicationReconciliationItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PhmMedicationReconciliationItem_InpEpisode_InpEpisodeId"
        FOREIGN KEY ("InpEpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PhmMedicationReconciliationItem_MstDrug_DrugId"
        FOREIGN KEY ("DrugId") REFERENCES public."MstDrug" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_PhmMedicationReconciliationItem_InpEpisodeId" ON public."PhmMedicationReconciliationItem" ("InpEpisodeId");
CREATE UNIQUE INDEX "IX_PhmMedicationReconciliationItem_IdempotencyKey"
    ON public."PhmMedicationReconciliationItem" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND "IsDelete" = false;

CREATE TABLE public."PhmMedicationReconciliationDecision" (
    "Id"                       uuid         NOT NULL,
    "ReconciliationItemId"     uuid         NOT NULL,
    "SequenceNumber"           integer      NOT NULL,
    "DecisionType"             integer      NOT NULL,
    "DecisionNote"             varchar(500) NULL,                -- SENSITIF
    "DecidedByDoctorId"        uuid         NOT NULL,
    "DecidedByUserId"          uuid         NOT NULL,
    "DecidedAt"                timestamptz  NOT NULL,
    "ResultPrescriptionId"     uuid         NULL,
    "ResultPrescriptionItemId" uuid         NULL,
    "SupersedesDecisionId"     uuid         NULL,
    "IsActive"                 boolean      NOT NULL DEFAULT true,
    CONSTRAINT "PK_PhmMedicationReconciliationDecision" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PhmMedicationReconciliationDecision_Item"
        FOREIGN KEY ("ReconciliationItemId") REFERENCES public."PhmMedicationReconciliationItem" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_PhmMedicationReconciliationDecision_Item_Sequence"
    ON public."PhmMedicationReconciliationDecision" ("ReconciliationItemId", "SequenceNumber");

CREATE TABLE public."PhmSlidingScaleTemplate" (
    "Id"           uuid         NOT NULL,
    "TemplateCode" varchar(50)  NOT NULL,
    "TemplateName" varchar(200) NOT NULL,
    "Description"  varchar(500) NULL,
    "IsActive"     boolean      NOT NULL DEFAULT true,
    CONSTRAINT "PK_PhmSlidingScaleTemplate" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_PhmSlidingScaleTemplate_TemplateCode" ON public."PhmSlidingScaleTemplate" ("TemplateCode");

CREATE TABLE public."PhmSlidingScaleTemplateVersion" (
    "Id"                   uuid         NOT NULL,
    "TemplateId"           uuid         NOT NULL,
    "VersionNumber"        integer      NOT NULL,
    "VersionStatus"        integer      NOT NULL DEFAULT 1,
    "GlucoseUnit"          integer      NOT NULL,
    "DefinitionHash"       char(64)     NULL,
    "LastModifiedByUserId" uuid         NOT NULL,
    "LastModifiedAt"       timestamptz  NOT NULL,
    "ApprovedByUserId"     uuid         NULL,
    "ApprovedAt"           timestamptz  NULL,
    "RetiredAt"            timestamptz  NULL,
    "ApprovalNote"         varchar(500) NULL,
    CONSTRAINT "PK_PhmSlidingScaleTemplateVersion" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PhmSlidingScaleTemplateVersion_Template"
        FOREIGN KEY ("TemplateId") REFERENCES public."PhmSlidingScaleTemplate" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_PhmSlidingScaleTemplateVersion_Template_Version"
    ON public."PhmSlidingScaleTemplateVersion" ("TemplateId", "VersionNumber");
CREATE UNIQUE INDEX "IX_PhmSlidingScaleTemplateVersion_Template_Approved"
    ON public."PhmSlidingScaleTemplateVersion" ("TemplateId")
    WHERE "VersionStatus" = 2 AND "IsDelete" = false;

CREATE TABLE public."PhmSlidingScaleOrder" (
    "Id"                   uuid         NOT NULL,
    "OrderNumber"          varchar(30)  NOT NULL,
    "PrescriptionId"       uuid         NOT NULL,
    "PrescriptionItemId"   uuid         NOT NULL,
    "EncounterId"          uuid         NOT NULL,
    "InpEpisodeId"         uuid         NOT NULL,
    "PatientId"            uuid         NOT NULL,
    "TemplateId"           uuid         NOT NULL,
    "OrderStatus"          integer      NOT NULL DEFAULT 1,
    "CurrentVersionNumber" integer      NOT NULL DEFAULT 1,
    "CheckFrequencyCode"   varchar(30)  NULL,
    "StoppedAt"            timestamptz  NULL,
    "StoppedByUserId"      uuid         NULL,
    "StopReason"           varchar(500) NULL,                  -- SENSITIF
    "IsActive"             boolean      NOT NULL DEFAULT true,
    CONSTRAINT "PK_PhmSlidingScaleOrder" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PhmSlidingScaleOrder_PrescriptionItem"
        FOREIGN KEY ("PrescriptionItemId") REFERENCES public."PhmPrescriptionItem" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_PhmSlidingScaleOrder_OrderNumber" ON public."PhmSlidingScaleOrder" ("OrderNumber");
CREATE UNIQUE INDEX "IX_PhmSlidingScaleOrder_PrescriptionItemId"
    ON public."PhmSlidingScaleOrder" ("PrescriptionItemId") WHERE "IsDelete" = false;

CREATE TABLE public."PhmSlidingScaleOrderVersion" (
    "Id"                uuid         NOT NULL,
    "OrderId"           uuid         NOT NULL,
    "VersionNumber"     integer      NOT NULL,
    "TemplateVersionId" uuid         NOT NULL,
    "IsAdjusted"        boolean      NOT NULL DEFAULT false,
    "AdjustmentReason"  varchar(500) NULL,                     -- SENSITIF
    "OrderedByDoctorId" uuid         NOT NULL,
    "OrderedByUserId"   uuid         NOT NULL,
    "OrderedAt"         timestamptz  NOT NULL,
    "IsActive"          boolean      NOT NULL DEFAULT true,
    CONSTRAINT "PK_PhmSlidingScaleOrderVersion" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PhmSlidingScaleOrderVersion_Order"
        FOREIGN KEY ("OrderId") REFERENCES public."PhmSlidingScaleOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PhmSlidingScaleOrderVersion_TemplateVersion"
        FOREIGN KEY ("TemplateVersionId") REFERENCES public."PhmSlidingScaleTemplateVersion" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_PhmSlidingScaleOrderVersion_Order_Version"
    ON public."PhmSlidingScaleOrderVersion" ("OrderId", "VersionNumber");

CREATE TABLE public."PhmSlidingScaleRange" (
    "Id"                            uuid         NOT NULL,
    "TemplateVersionId"             uuid         NULL,
    "OrderVersionId"                uuid         NULL,
    "LowerBoundInclusive"           numeric(7,2) NULL,
    "UpperBoundExclusive"           numeric(7,2) NULL,
    "DoseUnits"                     numeric(6,2) NOT NULL,
    "InstructionText"               varchar(300) NULL,
    "RequiresPhysicianNotification" boolean      NOT NULL DEFAULT false,
    "SortOrder"                     integer      NOT NULL,
    CONSTRAINT "PK_PhmSlidingScaleRange" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_PhmSlidingScaleRange_SingleOwner"
        CHECK (num_nonnulls("TemplateVersionId", "OrderVersionId") = 1),
    CONSTRAINT "CK_PhmSlidingScaleRange_DoseUnits" CHECK ("DoseUnits" >= 0),
    CONSTRAINT "FK_PhmSlidingScaleRange_TemplateVersion"
        FOREIGN KEY ("TemplateVersionId") REFERENCES public."PhmSlidingScaleTemplateVersion" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PhmSlidingScaleRange_OrderVersion"
        FOREIGN KEY ("OrderVersionId") REFERENCES public."PhmSlidingScaleOrderVersion" ("Id") ON DELETE RESTRICT
);

-- Klinis: jenis catatan dan pesanan tindakan
ALTER TABLE public."TrxPatientIntegratedProgressNote" ADD COLUMN "NoteKind" integer NOT NULL DEFAULT 0;
CREATE INDEX "IX_TrxPatientIntegratedProgressNote_Episode_Kind_Time"
    ON public."TrxPatientIntegratedProgressNote" ("InpEpisodeId", "NoteKind", "NoteDateTime");

ALTER TABLE public."TrxPatientProcedure" ALTER COLUMN "ConsultationId" DROP NOT NULL;
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "OrderedByUserId" uuid NULL;
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "InstructingDoctorId" uuid NULL;
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "InstructionVerificationStatus" integer NOT NULL DEFAULT 0;
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "InstructionVerifiedAt" timestamptz NULL;
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "InstructionVerifiedByUserId" uuid NULL;
ALTER TABLE public."TrxPatientProcedure" ADD COLUMN "CancelledByEpisodeClosure" boolean NOT NULL DEFAULT false;
CREATE INDEX "IX_TrxPatientProcedure_InstructingDoctor_Verification"
    ON public."TrxPatientProcedure" ("InstructingDoctorId", "InstructionVerificationStatus");

-- Laboratorium dan Radiologi: pemberi instruksi (setelah persetujuan pemilik modul)
ALTER TABLE public."LabOrder" ADD COLUMN "InstructingDoctorId" uuid NULL;
ALTER TABLE public."LabOrder" ADD COLUMN "InstructionVerificationStatus" integer NOT NULL DEFAULT 0;
ALTER TABLE public."LabOrder" ADD COLUMN "InstructionVerifiedAt" timestamptz NULL;
ALTER TABLE public."LabOrder" ADD COLUMN "InstructionVerifiedByUserId" uuid NULL;
ALTER TABLE public."RadOrder" ADD COLUMN "InstructingDoctorId" uuid NULL;
ALTER TABLE public."RadOrder" ADD COLUMN "InstructionVerificationStatus" integer NOT NULL DEFAULT 0;
ALTER TABLE public."RadOrder" ADD COLUMN "InstructionVerifiedAt" timestamptz NULL;
ALTER TABLE public."RadOrder" ADD COLUMN "InstructionVerifiedByUserId" uuid NULL;
```

### 13.14 Tabel yang tidak dibuat pada `0.5`

| Yang tidak dibuat | Alasan |
| --- | --- |
| Kolom nama obat teks bebas pada rekonsiliasi | `RWI-DEC-134` |
| Tabel rentang terpisah untuk template dan order | Bentuknya identik; satu tabel dengan check constraint "tepat satu pemilik" mencegah dua definisi rentang yang dapat menyimpang |
| Kolom GDS pada order atau versi order sliding scale | Sumber GDS satu, milik `ClinicalManagement` — `RWI-DEC-148` |
| Tabel verifikasi resep | `RWI-DEC-121` butir (6) |
| Tabel penanda "catatan milik saya" | `RWI-DEC-142` |
