# Kamus Data — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` |
| Revision | `0.4` — amendment atas `0.3`, menyerap `INT-DOK-10` |
| Status | **`approved`** — revision `0.4` disetujui Muhammad Hamzah, 2026-09-09; bagian 3 direvisi dan disetujui ulang 2026-09-05 |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-09** untuk revision `0.4`; `0.3` disetujui 2026-09-03 |
| Tanggal | 2 September 2026; diamendemen 9 September 2026 |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) revision `0.4` bagian 4 |
| Backend SHA | `93b3227c431401d8f586dec4e1fb25fbf41766e3` |

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
