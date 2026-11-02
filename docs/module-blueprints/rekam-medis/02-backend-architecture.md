# Rekam Medis — Arsitektur Backend

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Contract version | `0.1.0` |
| Backend SHA | `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` |
| Frontend SHA | `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| Input revisions | `00-interview-decisions.md` revision `3`; `01-existing-capability-map.md` revision `2` |
| Owners | Product/domain, clinical governance, security/privacy: **seluruhnya `OPEN`** |

> **PERINGATAN DASAR DESAIN.** Dokumen ini disusun di atas keputusan yang **belum disetujui
> owner mana pun**. Seluruh `RM-DEC-001` sampai `RM-DEC-026` berstatus `draft`. Ini pilihan
> sadar yang tercatat pada `RM-DEC-025`. Dua keputusan paling rawan ditolak adalah
> `RM-DEC-017` (kewenangan `SuperAdmin`, berdampak ke seluruh aplikasi) dan `RM-DEC-014`
> (perlakuan catatan lama). Bila salah satunya berubah, bagian desain yang bergantung padanya
> ikut berubah. Dokumen ini **bukan** izin untuk mulai membangun.

---

## 1. Gagasan inti desain

Ada satu pilihan yang menentukan seluruh bentuk desain ini, dan sebaiknya dipahami lebih dulu
sebelum membaca sisanya.

**Status keutuhan tidak ditempelkan ke tiga belas tabel klinis, melainkan disimpan di satu
tabel daftar tersendiri.**

Bayangkan buku tamu di depan ruang arsip. Buku itu tidak mengubah isi map mana pun. Ia hanya
mencatat, untuk setiap map: sudah ditandatangani siapa, kapan dikunci, dan apakah masih boleh
diubah. Tabel `TrxClinicalDocumentIntegrity` berperan seperti buku tamu itu.

Akibat langsung dari pilihan ini, dan inilah alasan utamanya:

| Akibat | Penjelasan |
|---|---|
| **Nol perubahan kolom pada 13 tabel klinis** | Tidak ada satu pun kolom ditambah atau diubah pada `TrxPatientIntegratedProgressNote`, `TrxDoctorConsultation`, dan sebelas tabel klinis lainnya. Alur IGD, antrean dokter, dan farmasi yang memakai tabel-tabel itu tidak tersentuh sama sekali |
| **Satu aturan, satu tempat** | Aturan penguncian ditulis sekali di satu service, bukan tiga belas kali di tiga belas controller. Ini menutup `RM-CAP-010` yang temuannya justru "aturan tersebar dan mudah terlewat" |
| **Empat model status lama dibiarkan hidup** | Persis yang diminta `RM-DEC-013`: status keutuhan berdampingan, bukan menggantikan |
| **Pengisian data lama menjadi sisipan baris, bukan pengubahan tabel** | `RM-DEC-014` dijalankan dengan menambah baris ke tabel daftar, bukan memutakhirkan tiga belas tabel yang sedang dipakai |

Risiko yang harus diterima dari pilihan ini juga perlu dinyatakan terbuka: karena status
keutuhan berada di tabel terpisah, **penegakannya sepenuhnya bergantung pada disiplin memanggil
service**. Sebuah controller yang lupa memanggil pemeriksa keutuhan tetap dapat mengubah
catatan terkunci. Cara menutupnya dijelaskan pada bagian 7.

---

## 2. Bounded context dan ownership

| Konteks | Owner | Aggregate root | Invariant utama |
|---|---|---|---|
| **Keutuhan Catatan Klinis** | `MedicalRecordManagement` (baru) | `TrxClinicalDocumentIntegrity` | Satu dokumen klinis memiliki tepat satu baris keutuhan. Setelah `Signed` atau `LockedUnsigned`, isi dokumen tidak dapat berubah |
| **Jejak dan Kewenangan Akses** | `MedicalRecordManagement` (baru) | `TrxMedicalRecordAccessLog` | Setiap pembukaan berkas rekam medis menghasilkan tepat satu baris jejak, sebelum isi dikembalikan |
| **Penelusuran Berkas** | `MedicalRecordManagement` (baru) | — (hanya baca) | Tidak memiliki data sendiri. Membaca milik `ClinicalManagement` tanpa menyalinnya |

Batas transaksi:

| Operasi | Batas transaksi | Alasan |
|---|---|---|
| Menandatangani dokumen | Satu transaksi mencakup pembaruan baris keutuhan saja | Isi dokumen tidak ikut berubah, jadi tidak perlu mengunci tabel klinis |
| Menutup kunjungan | Satu transaksi mencakup pembaruan status kunjungan **dan** penguncian seluruh dokumen terbuka di dalamnya | Bila penguncian gagal, penutupan kunjungan ikut dibatalkan. Kunjungan yang tertutup dengan dokumen masih terbuka adalah keadaan yang dilarang `RM-DEC-003` |
| Membuat addendum | Satu transaksi mencakup penyisipan addendum saja | Dokumen induk tidak berubah — itulah inti addendum |
| Mencatat jejak akses | Transaksi tersendiri, **selesai sebelum** isi rekam medis dikembalikan | Bila pencatatan gagal, isi tidak boleh dikembalikan. Membaca tanpa jejak adalah kegagalan, bukan gangguan kecil |

Baris terakhir perlu ditegaskan: pada modul ini, **gagal mencatat jejak berarti gagal
membaca**. Ini pilihan yang menutup rapat, bukan yang melonggarkan. Konsekuensinya, gangguan
pada tabel jejak akan menghambat pembacaan rekam medis — dan itu memang risiko yang dipilih
dengan sadar, karena membaca diam-diam lebih berbahaya daripada tidak bisa membaca.

---

## 3. Tabel kepemilikan data

Ini pertahanan langsung terhadap duplikasi entity. Kolom terakhir adalah yang paling penting.

| Kelompok data | Modul pemilik | Dipakai modul ini | Dibuat ulang di modul ini |
|---|---|:---:|---|
| Pasien dan nomor rekam medis | `PatientManagement` | Ya | **Tidak.** Dirujuk lewat `PatientId` |
| Kunjungan | `RegistrationManagement` | Ya | **Tidak.** Dirujuk lewat `EncounterId` |
| Antrean | `RegistrationManagement` | Tidak | Tidak |
| Isi klinis: asesmen, SOAP, CPPT, diagnosis, tindakan, tanda vital, alergi, riwayat | `ClinicalManagement` | Ya | **Tidak.** Dibaca apa adanya, sesuai `RM-DEC-001` |
| Dokumen klinis dan lampiran berkas | `ClinicalManagement` | Ya | **Tidak** |
| Surat keterangan medis dan persetujuan tindakan | `ClinicalManagement` | Ya | **Tidak** |
| Resep dan penyerahan obat | `PharmacyManagement` | Ya, hanya untuk ditampilkan | **Tidak** |
| Dokter, pegawai, dan pengguna | `Corporate/HumanResource` dan Identity | Ya | **Tidak** |
| Diagnosis dan tindakan master | `HealthServices/MasterData` | Ya | **Tidak** |
| Unit pelayanan dan klinik | `HealthServices/MasterData` | Ya | **Tidak** |
| **Status keutuhan dokumen klinis** | `MedicalRecordManagement` | Ya | **Ya, karena belum ada pemiliknya.** Konsep ini tidak ada di modul mana pun |
| **Addendum catatan klinis** | `MedicalRecordManagement` | Ya | **Ya, karena belum ada pemiliknya** |
| **Jejak akses rekam medis** | `MedicalRecordManagement` | Ya | **Ya, karena belum ada pemiliknya** |
| **Penetapan penulis berhalangan** | `MedicalRecordManagement` | Ya | **Ya, karena belum ada pemiliknya** |
| **Keperluan akses (master)** | `HealthServices/MasterData` | Ya | **Ya, master baru** |

---

## 4. Class diagram

Dipecah tiga agar setiap diagram muat dibaca dalam satu layar.

### 4.1 Konteks Keutuhan Catatan Klinis

```mermaid
classDiagram
    class TrxClinicalDocumentIntegrity {
        +Guid Id
        +ClinicalDocumentKind DocumentKind
        +Guid DocumentId
        +Guid PatientId
        +Guid EncounterId
        +ClinicalDocumentIntegrityStatus IntegrityStatus
        +Guid AuthorUserId
        +DateTime? SignedAt
        +DateTime? LockedAt
        +ClinicalDocumentLockTrigger? LockTrigger
    }
    class TrxClinicalNoteAddendum {
        +Guid Id
        +Guid IntegrityId
        +int Sequence
        +Guid AuthorUserId
        +bool IsSubstituteAuthor
        +Guid? DelegationId
        +string AddendumText
        +string CorrectionReason
    }
    class TrxClinicalNoteAuthorDelegation {
        +Guid Id
        +Guid OriginalAuthorUserId
        +AuthorDelegationTrigger Trigger
        +Guid? GrantedByUserId
        +string? GrantReason
        +DateTime? ValidUntil
    }
    class ClinicalDocumentIntegrityService {
        +RegisterAsync()
        +SignAsync()
        +LockOpenDocumentsForEncounterAsync()
        +EnsureMutableAsync()
    }
    class ClinicalNoteAddendumService {
        +CreateAsync()
        +ResolveAuthorityAsync()
    }
    TrxClinicalDocumentIntegrity "1" --> "0..*" TrxClinicalNoteAddendum : dikoreksi oleh
    TrxClinicalNoteAuthorDelegation "0..1" --> "0..*" TrxClinicalNoteAddendum : mendasari
    ClinicalDocumentIntegrityService ..> TrxClinicalDocumentIntegrity : mengelola
    ClinicalNoteAddendumService ..> TrxClinicalNoteAddendum : mengelola
    ClinicalNoteAddendumService ..> ClinicalDocumentIntegrityService : memeriksa keutuhan
```

### 4.2 Konteks Jejak dan Kewenangan Akses

```mermaid
classDiagram
    class TrxMedicalRecordAccessLog {
        +Guid Id
        +Guid PatientId
        +Guid UserId
        +MedicalRecordAccessType AccessType
        +MedicalRecordAccessScope AccessScope
        +Guid? AccessPurposeId
        +string? AccessReason
        +bool IsFlaggedForReview
        +DateTime AccessedAt
    }
    class MstMedicalRecordAccessPurpose {
        +Guid Id
        +string PurposeCode
        +string PurposeName
        +bool IsFreeTextRequired
    }
    class MedicalRecordAccessAuditService {
        +EvaluateAsync()
        +RecordAsync()
    }
    class MedicalRecordAccessReviewService {
        +GetPendingReviewAsync()
        +MarkReviewedAsync()
    }
    MstMedicalRecordAccessPurpose "1" --> "0..*" TrxMedicalRecordAccessLog : mengelompokkan
    MedicalRecordAccessAuditService ..> TrxMedicalRecordAccessLog : menulis
    MedicalRecordAccessReviewService ..> TrxMedicalRecordAccessLog : meninjau
```

### 4.3 Konteks Penelusuran Berkas dan controller

```mermaid
classDiagram
    class MedicalRecordController {
        +GetSummary()
        +GetTimeline()
        +GetPrivateNote()
    }
    class ClinicalDocumentIntegrityController {
        +Sign()
        +GetByDocument()
    }
    class ClinicalNoteAddendumController {
        +Create()
        +ListByDocument()
    }
    class MedicalRecordAccessLogController {
        +List()
        +MarkReviewed()
    }
    class MedicalRecordTimelineService {
        +BuildTimelineAsync()
        +BuildSummaryAsync()
    }
    MedicalRecordController ..> MedicalRecordTimelineService : memakai
    MedicalRecordController ..> MedicalRecordAccessAuditService : mencatat akses
    ClinicalDocumentIntegrityController ..> ClinicalDocumentIntegrityService : memakai
    ClinicalNoteAddendumController ..> ClinicalNoteAddendumService : memakai
    MedicalRecordAccessLogController ..> MedicalRecordAccessReviewService : memakai
    MedicalRecordTimelineService ..> ClinicalDocumentIntegrityService : mengambil status keutuhan
```

---

## 5. Penjelasan setiap class

### 5.1 `TrxClinicalDocumentIntegrity`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalDocumentIntegrity.cs` |
| Kategori | Transaksi |
| Tanggung jawab utama | Menyimpan keadaan keutuhan satu dokumen klinis: siapa penulisnya, sudah ditandatangani atau belum, sudah terkunci atau belum, dan karena apa terkuncinya. Tabel ini tidak menyimpan isi klinis apa pun — hanya keterangan tentang dokumennya |
| Field penting | `DocumentKind`, `DocumentId`, `PatientId`, `EncounterId`, `IntegrityStatus`, `AuthorUserId`, `SignedAt`, `SignedByUserId`, `LockedAt`, `LockTrigger`, `SignatureDeviceInfo` |
| Navigation property dan relasi | Menunjuk `MstPatient` dan `TrxPatientEncounter`; memiliki banyak `TrxClinicalNoteAddendum`. **Tidak** memiliki navigation property ke tiga belas tabel klinis, karena `DocumentId` bersifat polimorfik |
| Pemakaian dalam alur bisnis | Baris dibuat otomatis saat dokumen klinis pertama kali disimpan. Diperbarui saat penulis menandatangani, atau saat kunjungan ditutup |
| Catatan desain | `AuthorUserId` **tidak pernah** boleh diubah setelah baris dibuat. Inilah yang menutup `RM-CAP-012`, karena kepemilikan penulis dipindahkan ke tabel yang tidak bisa disentuh permintaan ubah dokumen. `DocumentKind` dan `DocumentId` wajib unik bersama |
| Ekuivalen model lama | — |

### 5.2 `TrxClinicalNoteAddendum`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalNoteAddendum.cs` |
| Kategori | Transaksi |
| Tanggung jawab utama | Menyimpan koreksi atau tambahan terhadap dokumen yang sudah terkunci. Addendum tidak pernah menimpa isi lama; ia menempel di bawahnya |
| Field penting | `IntegrityId`, `Sequence`, `AuthorUserId`, `IsSubstituteAuthor`, `DelegationId`, `AddendumText`, `CorrectionReason` |
| Navigation property dan relasi | Milik `TrxClinicalDocumentIntegrity`; boleh menunjuk `TrxClinicalNoteAuthorDelegation` bila dibuat penulis pengganti |
| Pemakaian dalam alur bisnis | Dibuat ketika penulis menyadari kesalahan pada catatan yang sudah ditandatangani, atau ketika kepala unit membetulkan catatan penulis yang berhalangan |
| Catatan desain | `Sequence` unik bersama `IntegrityId` supaya urutan koreksi terbaca pasti. Addendum **tidak dapat** dihapus maupun diubah; koreksi atas addendum dibuat sebagai addendum berikutnya. `IsSubstituteAuthor` bernilai benar **hanya** bila `DelegationId` terisi |
| Ekuivalen model lama | — |

### 5.3 `TrxClinicalNoteAuthorDelegation`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalNoteAuthorDelegation.cs` |
| Kategori | Transaksi |
| Tanggung jawab utama | Mencatat bahwa seorang penulis dinyatakan berhalangan, sehingga kepala unit atau DPJP boleh membuat addendum menggantikannya. Menjawab `RM-DEC-020` |
| Field penting | `OriginalAuthorUserId`, `Trigger`, `GrantedByUserId`, `GrantReason`, `ValidFrom`, `ValidUntil`, `IsActive` |
| Navigation property dan relasi | Menunjuk `ApplicationUser` dua kali: penulis asli dan pemberi penetapan |
| Pemakaian dalam alur bisnis | Untuk `Trigger = InactiveAccount`, baris tidak perlu dibuat manusia — sistem menyimpulkannya dari keadaan akun. Untuk `Trigger = UnitHeadGrant`, kepala unit membuat baris ini disertai alasan |
| Catatan desain | Penetapan manual wajib punya masa berlaku. Penetapan tanpa batas waktu adalah pintu belakang permanen, dan itu justru yang harus dihindari. Setiap penetapan manual ikut menjadi bahan tinjauan unit rekam medis |
| Ekuivalen model lama | — |

### 5.4 `TrxMedicalRecordAccessLog`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MedicalRecordManagement/Models/TrxMedicalRecordAccessLog.cs` |
| Kategori | Transaksi |
| Tanggung jawab utama | Menyimpan satu baris untuk setiap pembukaan berkas rekam medis: siapa, pasien siapa, kapan, dengan keperluan apa, dan apakah perlu ditinjau |
| Field penting | `PatientId`, `UserId`, `AccessType`, `AccessScope`, `AccessPurposeId`, `AccessReason`, `IsFlaggedForReview`, `ReviewedAt`, `ReviewedByUserId`, `AccessedAt`, `IpAddress`, `ClientInfo` |
| Navigation property dan relasi | Menunjuk `MstPatient`, `ApplicationUser`, dan `MstMedicalRecordAccessPurpose` |
| Pemakaian dalam alur bisnis | Ditulis setiap kali layar rekam medis dibuka, sebelum isinya dikembalikan |
| Catatan desain | Tabel ini **tumbuh paling cepat di seluruh sistem**. Pembagian tabel per periode wajib dirancang sejak migration pertama, memakai masa simpan 25 tahun yang ditetapkan `RM-DEC-024`, terbagi per tahun berdasarkan `AccessedAt`. Tabel ini **tidak** boleh memuat isi klinis apa pun — hanya keterangan tentang siapa membuka apa. Baris pada tabel ini **tidak** boleh dihapus lewat `IsDelete` biasa |
| Ekuivalen model lama | — |

### 5.5 `MstMedicalRecordAccessPurpose`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MasterData/Models/MstMedicalRecordAccessPurpose.cs` |
| Kategori | Master |
| Tanggung jawab utama | Daftar keperluan akses yang dapat dipilih pengguna saat membuka rekam medis pasien di luar rawatannya |
| Field penting | `PurposeCode`, `PurposeName`, `IsFreeTextRequired`, `RequiresReview`, `SortOrder`, `IsActive` |
| Navigation property dan relasi | Dipakai banyak `TrxMedicalRecordAccessLog` |
| Pemakaian dalam alur bisnis | Muncul sebagai pilihan pada kotak isian alasan |
| Catatan desain | Menyediakan pilihan jauh lebih baik daripada kotak teks bebas saja: alasan menjadi dapat dihitung dan dibandingkan saat ditinjau. Tetap sediakan satu pilihan "Lainnya" dengan `IsFreeTextRequired` bernilai benar. Letak file mengikuti aturan struktur: master tinggal di `MasterData/Models/`, **bukan** di folder submodulnya |
| Ekuivalen model lama | — |

### 5.6 `ClinicalDocumentIntegrityService`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` |
| Kategori | Service |
| Tanggung jawab utama | Satu-satunya tempat aturan keutuhan ditegakkan: mendaftarkan dokumen baru, menandatangani, mengunci saat kunjungan ditutup, dan menolak perubahan pada dokumen terkunci |
| Dipanggil oleh | `ClinicalDocumentIntegrityController`, `ClinicalNoteAddendumService`, `MedicalRecordTimelineService`, `PatientEncounterController`, dan controller `ClinicalManagement` yang mengubah dokumen |
| Membuka transaksi database | Ya, pada `SignAsync` dan `LockOpenDocumentsForEncounterAsync` |
| Catatan desain | `EnsureMutableAsync` adalah metode yang wajib dipanggil setiap controller sebelum mengubah dokumen klinis. Bila metode ini tidak dipanggil, aturan penguncian tidak berlaku — lihat bagian 7 tentang cara menutup risiko itu |

### 5.7 `MedicalRecordAccessAuditService`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordAccessAuditService.cs` |
| Kategori | Service |
| Tanggung jawab utama | Menentukan apakah pengguna sedang membuka pasien rawatannya atau bukan, lalu menuliskan baris jejak akses |
| Dipanggil oleh | `MedicalRecordController` |
| Membuka transaksi database | Ya, transaksi tersendiri yang selesai sebelum isi dikembalikan |
| Catatan desain | Penentuan "pasien rawatan" mengikuti `RM-DEC-016`: pasien memiliki kunjungan yang belum ditutup. Aturan ini diletakkan di satu metode agar mudah diperketat pada rilis berikutnya tanpa menyentuh controller |

### 5.8 `MedicalRecordTimelineService`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` |
| Kategori | Service |
| Tanggung jawab utama | Menggabungkan tiga belas sumber dokumen klinis menjadi satu daftar berurut waktu, lengkap dengan status keutuhan masing-masing |
| Dipanggil oleh | `MedicalRecordController` |
| Membuka transaksi database | Tidak. Hanya membaca, memakai `AsNoTracking` |
| Catatan desain | Menggabungkan tiga belas sumber berpotensi menghasilkan tiga belas query. Pembatasan wajib: penyaringan rentang tanggal, batas jumlah baris, dan pengambilan hanya jenis dokumen yang diminta. Pola penggabungan mencontoh `PrescriptionWorkspaceService` yang sudah terbukti |

### 5.9 `MedicalRecordController`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/HealthServices/MedicalRecordManagement/Controllers/MedicalRecordController.cs` |
| Kategori | Controller |
| Service yang dipakai | `MedicalRecordTimelineService`, `MedicalRecordAccessAuditService` |
| Endpoint yang diurus | Ringkasan berkas, riwayat berurut waktu, dan pembukaan `PrivateNote` |
| Catatan desain | Controller ini **wajib** memakai service, bukan `ApplicationDbContext` langsung, karena setiap pembacaan harus melewati pencatatan jejak akses lebih dulu |

### 5.10 `PatientIntegratedProgressNoteController`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Diperbarui` |
| **Lokasi file** | `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` |
| Kategori | Controller |
| Service yang dipakai | Bertambah: `ClinicalDocumentIntegrityService` |
| Tanggung jawab utama | Tidak berubah. Yang berubah hanya tiga celah keutuhan yang ditutup |
| Catatan desain | Tiga perubahan wajib, seluruhnya pada perilaku, **bukan pada skema tabel**: (1) memanggil `EnsureMutableAsync` sebelum mengubah, menutup `RM-CAP-011`; (2) **menghapus** penetapan `entity.ProviderUserId` dari isi permintaan pada baris 533, menutup `RM-CAP-012`; (3) **menghapus** penetapan `entity.IsReadOnlyGenerated` dari isi permintaan pada baris 550, menutup `RM-CAP-013`. Selain itu memanggil `RegisterAsync` saat CPPT dibuat |
| Ekuivalen model lama | — |

### 5.11 `PatientEncounterController`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Diperbarui` |
| **Lokasi file** | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` |
| Kategori | Controller |
| Service yang dipakai | Bertambah: `ClinicalDocumentIntegrityService` |
| Catatan desain | Pada endpoint `PATCH /{id}/status`, ketika status berpindah ke `Completed`, panggil `LockOpenDocumentsForEncounterAsync` di dalam transaksi yang sama. Ini menjalankan lapis kedua `RM-DEC-003`. Perlu diketahui: endpoint ini **tidak memiliki validasi perpindahan status** (`RM-CAP-019`), sehingga status dapat melompat dari nilai mana pun ke `Completed`. Penguncian karena itu dipicu oleh perpindahan **menuju** `Completed`, bukan oleh urutan tertentu |
| Ekuivalen model lama | — |

---

## 6. Arsitektur folder

```text
Areas/HealthServices/MedicalRecordManagement/          # BARU
├── Controllers/
│   ├── MedicalRecordController.cs                     # Baru
│   ├── ClinicalDocumentIntegrityController.cs         # Baru
│   ├── ClinicalNoteAddendumController.cs              # Baru
│   ├── ClinicalNoteAuthorDelegationController.cs      # Baru
│   └── MedicalRecordAccessLogController.cs            # Baru
├── DTOs/
│   ├── MedicalRecordDtos.cs                           # Baru
│   ├── ClinicalDocumentIntegrityDtos.cs               # Baru
│   ├── ClinicalNoteAddendumDtos.cs                    # Baru
│   ├── ClinicalNoteAuthorDelegationDtos.cs            # Baru
│   └── MedicalRecordAccessLogDtos.cs                  # Baru
├── Enums/
│   ├── ClinicalDocumentKind.cs                        # Baru
│   ├── ClinicalDocumentIntegrityStatus.cs             # Baru
│   ├── ClinicalDocumentLockTrigger.cs                 # Baru
│   ├── AuthorDelegationTrigger.cs                     # Baru
│   ├── MedicalRecordAccessType.cs                     # Baru
│   └── MedicalRecordAccessScope.cs                    # Baru
├── Models/
│   ├── TrxClinicalDocumentIntegrity.cs                # Baru
│   ├── TrxClinicalNoteAddendum.cs                     # Baru
│   ├── TrxClinicalNoteAuthorDelegation.cs             # Baru
│   └── TrxMedicalRecordAccessLog.cs                   # Baru
└── Services/
    ├── ClinicalDocumentIntegrityService.cs            # Baru
    ├── ClinicalNoteAddendumService.cs                 # Baru
    ├── MedicalRecordAccessAuditService.cs             # Baru
    ├── MedicalRecordAccessReviewService.cs            # Baru
    └── MedicalRecordTimelineService.cs                # Baru

Areas/HealthServices/MasterData/
├── Models/
│   └── MstMedicalRecordAccessPurpose.cs               # Baru
├── DTOs/
│   └── MedicalRecordAccessPurposeDtos.cs              # Baru
└── Controllers/
    └── MedicalRecordAccessPurposeController.cs        # Baru

Areas/HealthServices/ClinicalManagement/Controllers/
└── PatientIntegratedProgressNoteController.cs         # Diperbarui — 3 celah ditutup

Areas/HealthServices/RegistrationManagement/Controllers/
└── PatientEncounterController.cs                      # Diperbarui — pemicu penguncian

Repositories/Configurations/HealthServices/            # bentuk jamak, setelah perapian 27 Agustus 2026
└── MedicalRecordManagement/                           # BARU
    ├── TrxClinicalDocumentIntegrityConfiguration.cs   # Baru
    ├── TrxClinicalNoteAddendumConfiguration.cs        # Baru
    ├── TrxClinicalNoteAuthorDelegationConfiguration.cs # Baru
    ├── TrxMedicalRecordAccessLogConfiguration.cs      # Baru
    └── MstMedicalRecordAccessPurposeConfiguration.cs  # Baru

Migrations/                                            # 3 migration baru, lihat bagian 8
Program.cs                                             # Diperbarui — 5 AddScoped baru
```

Dua hal yang wajib diperhatikan implementer:

1. **File configuration tidak berada di dalam `Areas/`.** Letaknya di
   `Repositories/Configurations/HealthServices/MedicalRecordManagement/`. Pendaftarannya
   otomatis lewat `ApplyConfigurationsFromAssembly` pada `Repositories/ApplicationDbContext.cs`,
   jadi tidak perlu menambah baris `ApplyConfiguration` satu per satu.
2. **Master tidak berada di folder submodul.** `MstMedicalRecordAccessPurpose` tinggal di
   `Areas/HealthServices/MasterData/Models/`, mengikuti pola `MstEmergency*` yang sudah ada.

**Utang teknis nama folder sudah lunas — 27 Agustus 2026.** Sebelumnya folder ini bernama
`Repositories/Configurations/HealthServices/` dengan bentuk tunggal, sedangkan pola standarnya
`HealthServices`. Saat blueprint ini disusun, perapian sengaja ditunda agar konsisten dengan 68
file yang sudah ada.

Perapian itu akhirnya dikerjakan tim IGD, yang memindahkan seluruh isi folder tunggal ke bentuk
jamak. Configuration modul rekam medis sempat tertinggal di folder lama sehingga terlihat seperti
folder kembar, lalu ikut dipindahkan menyusul. Namespace kelima berkasnya kini
`...Configurations.HealthServices.MedicalRecordManagement`, dan folder bentuk tunggal **sudah
tidak ada lagi**.

Pemindahan ini tidak mengubah perilaku apa pun: pendaftaran configuration berjalan lewat
`ApplyConfigurationsFromAssembly`, yang memindai assembly dan tidak peduli pada nama folder
maupun namespace.

---

## 7. Cara menutup risiko "service yang lupa dipanggil"

Bagian 1 menyebut satu risiko nyata: karena status keutuhan ada di tabel terpisah, controller
yang lupa memanggil `EnsureMutableAsync` tetap bisa mengubah catatan terkunci. Risiko ini tidak
boleh dibiarkan menggantung.

Tiga lapis penutupnya:

| Lapis | Cara | Kekuatan |
|---|---|---|
| 1. Daftar terbatas | Hanya jenis dokumen yang terdaftar di `ClinicalDocumentKind` yang tunduk aturan keutuhan. Rilis pertama mendaftarkan **satu** jenis saja: CPPT | Kuat. Cakupan sempit dan diketahui pasti |
| 2. Uji penerimaan | Setiap jenis dokumen yang didaftarkan wajib punya uji yang membuktikan perubahan pada dokumen terkunci ditolak | Kuat, tetapi bergantung uji benar-benar ditulis |
| 3. Tinjauan berkala | Daftar `ClinicalDocumentKind` dibandingkan dengan daftar controller yang memanggil `EnsureMutableAsync` | Lemah, bergantung disiplin manusia |

Keputusan desain yang mengikutinya: **rilis pertama hanya mendaftarkan CPPT.** Dua belas jenis
dokumen lain belum tunduk aturan keutuhan, dan itu **wajib dinyatakan terbuka** di layar, bukan
didiamkan. Menyatakan cakupan yang jujur lebih baik daripada memberi kesan seluruh rekam medis
sudah terlindungi padahal baru satu jenis dokumen.

Alasan CPPT dipilih lebih dulu: ia dokumen yang paling sering ditulis, dan ia satu-satunya yang
temuan auditnya berstatus `Repair` — celah nyata pada kode berjalan, bukan sekadar fitur belum
ada.

---

## 8. Status model dan rencana migration

### 8.1 Status model

| Model | Status | Dampak migration |
|---|---|---|
| `TrxClinicalDocumentIntegrity` | `Baru` | Tabel baru beserta index |
| `TrxClinicalNoteAddendum` | `Baru` | Tabel baru beserta index |
| `TrxClinicalNoteAuthorDelegation` | `Baru` | Tabel baru beserta index |
| `TrxMedicalRecordAccessLog` | `Baru` | Tabel baru, dirancang terbagi per periode |
| `MstMedicalRecordAccessPurpose` | `Baru` | Tabel baru beserta data awal |
| `TrxPatientIntegratedProgressNote` | `Sudah ada` | **Tidak ada perubahan kolom.** Yang berubah hanya perilaku controller |
| `TrxPatientEncounter` | `Sudah ada` | **Tidak ada perubahan kolom.** Yang berubah hanya perilaku controller |
| Sebelas tabel klinis lainnya | `Sudah ada` | **Tidak ada perubahan sama sekali** |

Perlu ditegaskan karena inilah sifat terpenting desain ini: **rilis pertama tidak mengubah satu
kolom pun pada tabel yang sedang dipakai.** Seluruh perubahan berupa tabel baru ditambah
perubahan perilaku pada dua controller.

### 8.2 Rencana migration

| Urutan | Nama | Isi | Tanpa mematikan layanan? | Cara mundur |
|---:|---|---|:---:|---|
| 1 | `AddMedicalRecordIntegrityTables` | Membuat `TrxClinicalDocumentIntegrity`, `TrxClinicalNoteAddendum`, `TrxClinicalNoteAuthorDelegation` beserta index | **Ya** — hanya menambah tabel baru | Hapus tabel. Aman karena belum ada yang memakainya |
| 2 | `AddMedicalRecordAccessAuditTables` | Membuat `MstMedicalRecordAccessPurpose` dan `TrxMedicalRecordAccessLog`, mengisi data awal keperluan akses | **Ya** | Hapus tabel |
| 3 | `BackfillProgressNoteIntegrity` | Mengisi baris keutuhan untuk seluruh CPPT yang sudah ada, sesuai `RM-DEC-014` | **Ya**, tetapi lihat catatan di bawah | Hapus seluruh baris yang dibuat migration ini |

Catatan penting untuk migration ketiga. Ini satu-satunya migration yang menyentuh data nyata,
dan cara kerjanya:

```text
Untuk setiap CPPT yang belum punya baris keutuhan:
  ambil kunjungan yang menaunginya
  bila kunjungan sudah Completed atau Cancelled  -> IntegrityStatus = LockedUnsigned
                                                     LockTrigger    = BackfillEncounterClosed
  bila kunjungan masih berjalan                  -> IntegrityStatus = Draft
  bila CPPT sudah dibatalkan (IsCancel)          -> IntegrityStatus = Cancelled
  AuthorUserId <- ProviderUserId; bila kosong, tandai AuthorUnknown
```

Tiga hal yang harus disiapkan sebelum menjalankannya:

1. **Jumlah barisnya belum diketahui.** Audit tidak melihat data produksi (batas audit nomor 3
   pada capability map). Migration ini wajib dijalankan bertahap per potongan, bukan sekali
   jalan, agar tidak mengunci tabel terlalu lama.
2. **CPPT tanpa `ProviderUserId` akan muncul.** Kolom itu boleh kosong pada model yang ada.
   Barisnya tetap dibuat dengan penanda penulis tidak diketahui, dan itu ikut terlihat pada
   laporan kelengkapan. Menyembunyikannya berarti berbohong tentang keadaan data.
3. **Unit rekam medis harus diberi tahu lebih dulu.** Sesuai `RM-DEC-014`, laporan kelengkapan
   akan menampilkan banyak catatan bertanda tidak ditandatangani sejak hari pertama.

---

## 9. Rencana data master awal

Modul dengan tabel master kosong tidak dapat dipakai sama sekali. Hanya ada satu master pada
rilis pertama.

| Master | Isi minimum | Sumber nilai |
|---|---|---|
| `MstMedicalRecordAccessPurpose` | Sekurang-kurangnya lima keperluan: `Konsultasi lintas unit`, `Penanganan gawat darurat`, `Penelusuran kelengkapan berkas`, `Permintaan resmi pasien atau keluarga`, `Lainnya` | SOP rekam medis rumah sakit. **Belum tersedia** — lihat blocker nomor 3 pada decision log |

Aturan yang mengikat: pilihan keperluan akses **wajib** berasal dari master ini dan **tidak
boleh** ditulis tetap di dalam controller maupun frontend. Alasannya praktis — daftar keperluan
akan berubah setelah tinjauan pertama unit rekam medis, dan mengubah master jauh lebih murah
daripada mengubah kode.

Baris `Lainnya` wajib memiliki `IsFreeTextRequired` bernilai benar, sehingga pengguna yang
memilihnya harus menuliskan alasan sendiri.

---

## 10. Kewenangan, privasi, dan pencatatan

| Aspek | Ketetapan |
|---|---|
| Autentikasi | Mengikuti pola yang ada. Tidak ada perubahan |
| Kewenangan fungsi | Otomatis terdaftar lewat `[AccessController]` dan `[AccessAction]`, dibaca `Seeders/AccessMenuSeeder.cs` saat aplikasi mulai |
| Kewenangan tingkat pasien | **Baru.** Ditegakkan `MedicalRecordAccessAuditService`, bukan oleh `AccessPermissionFilter` yang hanya mengenal controller dan action |
| `SuperAdmin` | Tetap melewati kewenangan fungsi, tetapi **tunduk** pada aturan akses rekam medis. Diterapkan di dalam service, bukan di `AccessPermissionService`, agar perilaku modul lain tidak ikut berubah |
| Pencatatan lewat `LoggerService` | Mengikuti konvensi project: Create, Update, perubahan status, dan Delete dicatat; GET tidak dicatat |
| Pencatatan jejak akses | **Terpisah dari `LoggerService`.** Ditulis ke tabel `TrxMedicalRecordAccessLog`, termasuk untuk permintaan GET |
| Isi log | Payload `LoggerService` hanya memuat `EntityId`, controller, action, dan status. **Tidak boleh** memuat isi klinis, alasan akses, maupun `PrivateNote` |

Baris kelima adalah penjelasan atas satu hal yang tampak bertentangan. Konvensi project
menyatakan GET tidak dicatat logger, sementara modul ini mewajibkan setiap pembacaan tercatat.
Keduanya tidak bertentangan karena **tempatnya berbeda**: `LoggerService` tetap mengikuti
konvensi, sedangkan jejak akses ditulis ke tabelnya sendiri. Membedakan keduanya penting agar
implementer tidak mengira konvensi logging sedang dilanggar.

Cara `RM-DEC-017` diterapkan juga perlu digarisbawahi. `AccessPermissionService.cs:54-56`
**tidak diubah**. Pembatasan `SuperAdmin` diterapkan di dalam `MedicalRecordAccessAuditService`.
Dengan begitu, keputusan yang berdampak lintas aplikasi itu dibatasi pengaruhnya hanya pada
modul rekam medis sampai security/privacy owner memutuskan lebih luas.

---

## 11. Yang sengaja tidak dibuat

Bagian ini mencegah orang berikutnya mengusulkan ulang hal yang sudah dipertimbangkan.

| Yang ditolak | Alasan |
|---|---|
| `MstMedicalRecordPatient` atau salinan pasien | Pasien dimiliki `PatientManagement`. Nomor rekam medis sudah ada di `MstPatient.MedicalRecordNumber` dan sudah dijamin unik |
| Tabel salinan isi klinis untuk mempercepat penelusuran | Menyalin isi klinis melanggar `RM-DEC-001` dan menciptakan dua sumber kebenaran. Bila kecepatan jadi masalah, jawabannya index dan pembatasan rentang tanggal, bukan salinan |
| Kolom status keutuhan pada tiga belas tabel klinis | Ditolak `RM-DEC-013`. Menyentuh tabel yang dipakai IGD, antrean dokter, dan farmasi tanpa uji otomatis adalah risiko yang tidak sebanding |
| Kolom versi pada tabel klinis untuk menyimpan nilai lama | Tidak diperlukan pada rilis pertama. Setelah dokumen terkunci, isinya tidak berubah lagi, sehingga tidak ada nilai lama yang perlu disimpan. Riwayat perubahan sebelum penguncian memang tidak tersimpan, dan itu keterbatasan yang dinyatakan terbuka |
| `TrxMedicalRecordBorrowing` untuk peminjaman berkas | Berada pada cakupan 7, rilis berikutnya menurut `RM-DEC-002` |
| Tabel retensi dan pemusnahan | Berada pada cakupan 8, rilis berikutnya |
| Perubahan pada `AccessPermissionService` | Ditolak untuk rilis pertama. Mengubahnya berdampak ke seluruh aplikasi termasuk IGD, sedangkan security/privacy owner belum ditunjuk |
| Penegakan `ConfidentialityLevel` | Ditolak `RM-DEC-018` untuk rilis pertama, karena jumlah dokumen yang terlanjur ditandai belum diketahui |
| Modul jejak akses generik untuk seluruh aplikasi | Menggoda, tetapi memperluas scope jauh melampaui `RM-DEC-001`. Bila kelak dibutuhkan, tabel ini dapat menjadi contohnya |

---

## 12. Keterbatasan yang dinyatakan terbuka

Bukan daftar pekerjaan yang tertinggal, melainkan hal yang memang tidak dijawab rilis pertama
dan **wajib** disampaikan kepada pengguna dan auditor.

| No | Keterbatasan | Sumber keputusan |
|---:|---|---|
| 1 | Hanya CPPT yang tunduk aturan keutuhan. Dua belas jenis dokumen lain belum | Bagian 7, turunan `RM-DEC-019` |
| 2 | Perubahan pada dokumen **sebelum** terkunci tidak menyimpan nilai lama | Bagian 11; `RM-CAP-017` tetap `Missing` |
| 3 | Label tingkat kerahasiaan belum membatasi akses | `RM-DEC-018` |
| 4 | Definisi pasien rawatan masih longgar: siapa pun tenaga klinis dapat membuka pasien berkunjungan aktif tanpa alasan | `RM-DEC-016` |
| 5 | Tidak ada validasi perpindahan status kunjungan, sehingga penutupan kunjungan dapat terjadi dari status mana pun | `RM-CAP-019`, berstatus `Repair` dan tidak masuk rilis pertama |
| 6 | Riwayat pasien hasil penggabungan nomor rekam medis ganda dapat tampil terpecah | `RM-CAP-007`, masih `Unknown` |

Keterbatasan nomor 6 perlu perhatian khusus. Kolom `MergedToPatientId` ada pada `MstPatient`,
tetapi alur penggabungannya tidak ditemukan di controller mana pun. Bila di lapangan ternyata
ada pasien dengan dua nomor rekam medis, layar penelusuran akan menampilkan riwayat yang
terpotong tanpa memberi tahu pembacanya. Sebelum rilis, ini **wajib** ditelusuri lebih dulu.

---

## 13. Traceability

| Keputusan | Diwujudkan oleh |
|---|---|
| `RM-DEC-001` | Tabel kepemilikan data bagian 3; tidak ada entity isi klinis yang dibuat ulang |
| `RM-DEC-002` | Cakupan rilis pertama: bagian 6 sampai 9 |
| `RM-DEC-003` | `ClinicalDocumentIntegrityStatus`; `SignAsync` dan `LockOpenDocumentsForEncounterAsync` |
| `RM-DEC-004` | `TrxClinicalNoteAddendum` beserta `ClinicalNoteAddendumService.ResolveAuthorityAsync` |
| `RM-DEC-005` | `MedicalRecordAccessAuditService`; `TrxMedicalRecordAccessLog.IsFlaggedForReview` |
| `RM-DEC-006` | Tidak ada transisi kembali ke `Draft`; entri susulan tidak membuka kunjungan |
| `RM-DEC-013` | Gagasan inti bagian 1: tabel daftar terpisah, bukan kolom pada tabel klinis |
| `RM-DEC-014` | Migration ketiga bagian 8.2 |
| `RM-DEC-015` | `TrxMedicalRecordAccessLog` sebagai tabel, bukan log teks |
| `RM-DEC-016` | Metode penentu pasien rawatan pada `MedicalRecordAccessAuditService` |
| `RM-DEC-017` | Bagian 10: pembatasan diterapkan di service modul, bukan di `AccessPermissionService` |
| `RM-DEC-018` | Bagian 11: penegakan kerahasiaan ditolak untuk rilis pertama |
| `RM-DEC-019` | Bagian 7: CPPT didahulukan; urutan migration bagian 8.2 |
| `RM-DEC-020` | `TrxClinicalNoteAuthorDelegation` beserta `AuthorDelegationTrigger` |
| `RM-DEC-021` | `SignedAt`, `SignedByUserId`, `SignatureDeviceInfo`; tanpa pengesahan ulang |
| `RM-DEC-022` | Endpoint pembukaan `PrivateNote` terpisah, melewati jalur akses beralasan |
| `RM-DEC-023` | Rancangan pembagian tabel jejak per periode, bagian 5.4 |


---

# Amandemen revision 4 — Sambungan seluruh modul dan kelengkapan berkas

| Field | Value |
|---|---|
| Blueprint revision | `4` — **`approved`**, Yoga Aji Pratama, 28 Oktober 2026 (`RM-DEC-062`) |
| Tanggal | 28 Oktober 2026 |
| Scope | Sepuluh slice siap pada `03-domain-architecture.md` (`RM-DA-001` revision 1): S-A1, S-A2, S-A3, S-A4, S-A5, S-A6, S-A7, S-B1, S-B2, S-B3. **S-A1b (hasil Patologi Anatomi) dikecualikan** — hanya keadaan "belum tersedia" yang dirancang |
| Input | Decision log revision `9` (`RM-DEC-037`–`061`); capability map revision `3`; `RM-RCG-001` revision 2; `RM-DA-001` revision 1 (`DOMAIN_ARCHITECTURE_PARTIAL`) |
| Source SHA | Backend `363de34f`; frontend `f0a3b07b8` — diverifikasi sama saat desain ditulis |
| Yang tidak dirancang ulang | Bounded context, aggregate, ownership, lifecycle, billing, dan batas keselamatan diambil apa adanya dari `RM-DA-001` |

Bagian di atas garis ini adalah desain rilis pertama dan **tetap berlaku**. Bagian ini hanya
menambah.

## A4.1 Bounded context dan batas transaksi

| Context (`RM-DA-001`) | Aggregate | Batas transaksi | Rollback |
|---|---|---|---|
| `BC-RM-01` Berkas | `AGG-RM-01` (baca saja) | Tidak membuka transaksi tulis selain jejak akses yang sudah ada | — |
| `BC-RM-02` Keutuhan | `AGG-RM-02` Penutupan Klinis | **Ikut transaksi pemanggil** — context yang menutup kunjungan. Ditulis di dalam `LockOpenDocumentsForEncounterAsync` yang sudah dipanggil di dalam transaksi penutupan | Bila gagal, penutupan kunjungan ikut gagal (sama dengan penguncian hari ini) |
| `BC-RM-02` Keutuhan | Addendum pengesahan | Satu transaksi: addendum + `AddendumCount` + penilaian ulang ditandai | Seluruhnya batal |
| `BC-RM-03` Kelengkapan | `AGG-RM-03` Penilaian | Satu transaksi per kunjungan saat menyimpan hasil. **Pembacaan sumber terjadi sebelum transaksi dibuka** | Penilaian lama tetap; dicoba lagi pada pemeriksaan berkala |
| `BC-RM-03` Kelengkapan | `AGG-RM-04` Profil dokumen wajib | Satu transaksi per profil beserta butirnya | Seluruhnya batal |

## A4.2 Tabel kepemilikan data

| Kelompok data | Modul pemilik | Dipakai amandemen | Dibuat ulang di rekam medis |
|---|---|:---:|---|
| Pasien | Patient Management | Ya | **Tidak** |
| Kunjungan, jenis kunjungan, unit, rujukan | Registrasi | Ya | **Tidak** |
| Jenis unit layanan | Master Data (`MstServiceUnit.ServiceUnitType`) | Ya | **Tidak** |
| Hasil laboratorium, pemeriksaan tertunda | Laboratorium | Ya | **Tidak** — dibaca lewat sumber (`RM-DEC-030`, `038`) |
| Laporan radiologi, pemeriksaan tertunda | Radiologi | Ya | **Tidak** |
| Triase, observasi, resusitasi, disposisi | IGD | Ya | **Tidak** |
| Dokumen admisi, ringkasan pulang, episode | Rawat Inap | Ya | **Tidak** |
| Laporan operasi, anestesi, checklist, kelahiran, kasus | Kamar Operasi | Ya | **Tidak** |
| Sesi hemodialisa | Hemodialisa | Ya | **Tidak** |
| Asuhan gizi | Gizi | Ya | **Tidak** |
| Golongan darah, reaksi transfusi | Bank Darah | Ya | **Tidak** |
| Resep, pemberian obat, rekonsiliasi | Farmasi | Ya | **Tidak** |
| Hasil dan resume MCU | MCU | Ya | **Tidak** |
| Catatan klinis lama dan baru | `ClinicalManagement` | Ya | **Tidak** |
| Status keutuhan, addendum | Rekam Medis | Ya | Sudah ada — **diperluas** (jenis addendum) |
| **Penutupan klinis kunjungan** | Rekam Medis | Ya | **Ya, baru** — tidak ada pemilik lain; hari ini hanya tersirat pada dokumen konsep (`AGG-RM-02`) |
| **Penilaian kelengkapan** | Rekam Medis | Ya | **Ya, baru** — kesimpulan rekam medis sendiri, bukan isi dokumen |
| **Profil dokumen wajib** | Rekam Medis (master) | Ya | **Ya, baru** |

## A4.3 Class diagram

### Berkas dan sumber (`BC-RM-01`)

```mermaid
classDiagram
    class MedicalRecordTimelineService {
        <<Diperbarui>>
        +GetTimelineAsync(query) MedicalRecordTimelineResult
        -BacaSumberDenganBatasWaktu(sumber, kueri)
    }
    class IMedicalRecordDocumentSource {
        <<interface Baru>>
        +Kinds IReadOnlyCollection~ClinicalDocumentKind~
        +ReadAsync(MedicalRecordSourceQuery) MedicalRecordSourcePage
        +ReadDetailAsync(kind, documentId, patientId) MedicalRecordSourceDocumentDetail
    }
    class MedicalRecordFinalDocumentDescriptor {
        <<DTO Baru>>
        +Kind
        +DocumentId
        +EncounterId
        +FinalizedAt
        +IsCorrected
        +IsProvisional
    }
    class LabMedicalRecordDocumentSource {
        <<Baru, LaboratoryManagement>>
    }
    class RadMedicalRecordDocumentSource {
        <<Baru, RadiologyManagement>>
    }
    class PeriodicObservationSummaryService {
        <<Baru>>
        +SummarizeAsync(patientId, range)
        +GetDayDetailAsync(patientId, kind, date)
    }
    class MrcEncounterClinicalClosure {
        <<Model Baru>>
        +EncounterId
        +ClinicalClosedAt
    }
    MedicalRecordTimelineService --> IMedicalRecordDocumentSource : memanggil satu per satu
    MedicalRecordTimelineService --> PeriodicObservationSummaryService
    MedicalRecordTimelineService --> MrcEncounterClinicalClosure : menurunkan Susulan
    IMedicalRecordDocumentSource <|.. LabMedicalRecordDocumentSource
    IMedicalRecordDocumentSource <|.. RadMedicalRecordDocumentSource
    IMedicalRecordDocumentSource ..> MedicalRecordFinalDocumentDescriptor
```

Delapan sumber lain — IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah, Farmasi,
MCU — berbentuk sama dengan `LabMedicalRecordDocumentSource` dan tidak digambar satu per satu
supaya diagram terbaca. Daftarnya pada A4.10.

### Kelengkapan (`BC-RM-03`) dan keutuhan (`BC-RM-02`)

```mermaid
classDiagram
    class MedicalRecordCompletenessService {
        <<Baru>>
        +EvaluateAsync(encounterId) CompletenessResult
        +ListWorklistAsync(query)
    }
    class IMedicalRecordCompletenessProbe {
        <<interface Baru>>
        +Criteria
        +Conditions
        +ProbeAsync(encounterId, patientId) CompletenessProbeResult
    }
    class MedicalRecordCompletenessRecheckHostedService {
        <<Baru, BackgroundService>>
    }
    class MrcEncounterCompletenessAssessment {
        <<Model Baru>>
        +State
        +Deficiencies jsonb
        +EvaluatedAt
        +FirstCompletedAt
        +NextCheckAt
    }
    class MstMedicalRecordCompletenessProfile {
        <<Model Baru>>
        +EncounterType
        +ServiceUnitType
        +Condition
    }
    class MstMedicalRecordCompletenessProfileItem {
        <<Model Baru>>
        +Criterion
    }
    class ClinicalDocumentIntegrityService {
        <<Diperbarui>>
        +LockOpenDocumentsForEncounterAsync()
        -CatatPenutupanKlinis()
    }
    class MrcClinicalNoteAddendum {
        <<Model Diperbarui>>
        +AddendumPurpose
    }
    MedicalRecordCompletenessService --> IMedicalRecordCompletenessProbe
    MedicalRecordCompletenessService --> MstMedicalRecordCompletenessProfile
    MedicalRecordCompletenessService --> MrcEncounterCompletenessAssessment
    MedicalRecordCompletenessRecheckHostedService --> MedicalRecordCompletenessService
    MstMedicalRecordCompletenessProfile "1" --> "0..*" MstMedicalRecordCompletenessProfileItem
    ClinicalDocumentIntegrityService --> MrcEncounterClinicalClosure
    MrcClinicalNoteAddendum --> MrcClinicalDocumentIntegrity
```

## A4.4 Penjelasan setiap class

### Model

| Class | Status | Lokasi file | Kegunaan | Catatan |
|---|---|---|---|---|
| `MrcEncounterClinicalClosure` | **Baru** | `Areas/HealthServices/MedicalRecordManagement/Models/MrcEncounterClinicalClosure.cs` | Satu baris per kunjungan yang sudah tutup klinis (`AGG-RM-02`) | Hanya disisipkan, tidak pernah diubah. Keberadaan barisnya sekaligus menjadi penanda "masuk daftar kerja" (`RM-DEC-050`): baris hanya lahir sejak fitur dirilis |
| `MrcEncounterCompletenessAssessment` | **Baru** | `.../MedicalRecordManagement/Models/MrcEncounterCompletenessAssessment.cs` | Hasil penilaian terakhir satu kunjungan (`AGG-RM-03`) | Kekurangan disimpan sebagai `jsonb`, tidak sebagai tabel anak, supaya penilaian ulang yang sering tidak meninggalkan ribuan baris bertanda hapus. Pola `jsonb` sudah dipakai di 154 configuration |
| `MstMedicalRecordCompletenessProfile` | **Baru** | `Areas/HealthServices/MasterData/Models/MstMedicalRecordCompletenessProfile.cs` | Satu himpunan dokumen wajib untuk satu kunci (jenis kunjungan, jenis unit, kondisi) | Master tinggal di `MasterData`, bukan di folder submodul |
| `MstMedicalRecordCompletenessProfileItem` | **Baru** | `Areas/HealthServices/MasterData/Models/MstMedicalRecordCompletenessProfileItem.cs` | Satu kriteria dalam profil | Profil **tanpa** butir sah dan bermakna "tidak ada dokumen dokter wajib" — dipakai unit Laboratorium dan Radiologi (`RM-DEC-060`) |
| `MrcClinicalNoteAddendum` | **Diperbarui** | `.../MedicalRecordManagement/Models/MrcClinicalNoteAddendum.cs` | Menambah jenis addendum | Kolom baru `AddendumPurpose` (A4.6) |
| `MrcClinicalDocumentIntegrity` | Sudah ada | `.../Models/MrcClinicalDocumentIntegrity.cs` | Syarat 1 | Tidak berubah |

### Enum

| Enum | Status | Lokasi | Nilai | Bawaan |
|---|---|---|---|---|
| `ClinicalDocumentKind` | **Diperbarui** | `.../MedicalRecordManagement/Enums/ClinicalDocumentKind.cs` | Nilai 1–15 tetap. Ditambah: `RadiologyReport = 16`, `EmergencyTriage = 17`, `EmergencyObservation = 18`, `EmergencyResuscitation = 19`, `EmergencyDisposition = 20`, `DischargeSummary = 21`, `AdmissionDocument = 22`, `SurgeryReport = 23`, `AnesthesiaRecord = 24`, `SurgicalSafetyChecklist = 25`, `BirthRecord = 26`, `NutritionCareRecord = 27`, `BloodGroupExam = 28`, `TransfusionReactionNotice = 29`, `Prescription = 30`, `MedicationAdministration = 31`, `MedicationReconciliation = 32`, `McuResume = 33`, `McuExaminationResult = 34`, `NursingCarePlan = 35`, `PhysicianVisit = 36`, `NursingIntervention = 37`, `TransfusionMonitoring = 38`, `DoctorCertificate = 39`, `CaseManagementEvaluation = 40`, `NosocomialInfection = 41`, `TransferHandover = 42`, `PeriodicObservationSummary = 43`, `PathologyAnatomyResult = 44` | — |
| `PeriodicObservationKind` | **Baru** | `.../MedicalRecordManagement/Enums/PeriodicObservationKind.cs` | `DailyObservation = 1`, `FluidBalance = 2`, `BloodGlucose = 3`, `EquipmentUsage = 4`, `WsdDrain = 5`, `SurgicalSiteSurveillance = 6` | — |
| `ClinicalClosureSource` | **Baru** | `.../MedicalRecordManagement/Enums/ClinicalClosureSource.cs` | `EncounterStatusPatch = 1`, `DoctorQueue = 2`, `NurseStation = 3`, `EmergencyVisit = 4`, `EmergencyAdmissionDisposition = 5`, `InpatientDischarge = 6`, `BillingFinalization = 7` | — |
| `CompletenessState` | **Baru** | `.../MedicalRecordManagement/Enums/CompletenessState.cs` | `Complete = 1`, `Incomplete = 2`, `AwaitingResult = 3`, `Undetermined = 4` | — |
| `CompletenessDeficiencyType` | **Baru** | `.../MedicalRecordManagement/Enums/CompletenessDeficiencyType.cs` | `UnsignedDocument = 1`, `PendingResult = 2`, `MissingRequiredDocument = 3`, `SourceUnavailable = 4`, `RuleNotFound = 5` | — |
| `CompletenessCriterion` | **Baru** — katalog `DC-RM-06a` | `.../MedicalRecordManagement/Enums/CompletenessCriterion.cs` | Lihat A4.11 | — |
| `CompletenessCondition` | **Baru** | `.../MedicalRecordManagement/Enums/CompletenessCondition.cs` | `None = 0`, `Surgery = 1`, `HemodialysisSession = 2`, `Transfusion = 3` | `None` |
| `ClinicalNoteAddendumPurpose` | **Baru** | `.../MedicalRecordManagement/Enums/ClinicalNoteAddendumPurpose.cs` | `Correction = 1`, `Attestation = 2` | `Correction` |

**Perhatian pada `ClinicalDocumentKind`.** Hari ini daftar seluruh jenis diturunkan langsung dari
enum (`MedicalRecordTimelineService.cs:96-97`), sehingga setiap nilai baru otomatis diminta pada
setiap pembukaan berkas. Inilah penyebab kekosongan diam-diam jenis 14 dan 15 (`RM-CAP-035`).
Desain mengganti sumber daftar itu menjadi **daftar jenis yang benar-benar punya pembaca**
(A4.9). Jenis tanpa pembaca dilaporkan sebagai "belum tersedia", bukan dilewati.

### Service

| Service | Status | Lokasi | Fungsi | Dipanggil oleh | Transaksi |
|---|---|---|---|---|---|
| `MedicalRecordTimelineService` | **Diperbarui** | `.../MedicalRecordManagement/Services/` | Menggabungkan 13 jenis lama, model `ClinicalManagement` baru, dan seluruh `IMedicalRecordDocumentSource`; menurunkan `Susulan`; membatasi waktu per sumber | `MedicalRecordController` | Tidak |
| `PeriodicObservationSummaryService` | **Baru** | sama | Ringkasan per hari per jenis dan rinciannya (`RM-DEC-052`) | `MedicalRecordTimelineService`, `MedicalRecordController` | Tidak |
| `MedicalRecordCompletenessService` | **Baru** | sama | Memilih profil, memanggil seluruh `IMedicalRecordCompletenessProbe`, menyusun kekurangan, menyimpan penilaian | `MedicalRecordCompletenessController`, hosted service, `ClinicalDocumentIntegrityService`, `ClinicalNoteAddendumService` | Ya, satu per kunjungan, sesudah pembacaan sumber |
| `MedicalRecordCompletenessRecheckHostedService` | **Baru** | sama | Pemeriksaan berkala (A4.13) | Host | Ya, per kunjungan |
| `MedicalRecordCompletenessProfileService` | **Baru** | `Areas/HealthServices/MasterData/Services/` | Tambah, ubah, aktif/nonaktif profil dan butir; mencatat perubahan | `MedicalRecordCompletenessProfileController` | Ya |
| `ClinicalDocumentIntegrityService` | **Diperbarui** | `.../MedicalRecordManagement/Services/` | `LockOpenDocumentsForEncounterAsync` kini juga mencatat penutupan klinis (idempoten), lalu menandai kunjungan untuk dinilai | Enam jalur penutupan yang sudah ada + jalur disposisi rawat inap IGD (`GAP-ARCH-RM-01`) | Ikut transaksi pemanggil |
| `ClinicalNoteAddendumService` | **Diperbarui** | sama | Menerima `purpose`; menegakkan aturan pengesahan (A4.12) | `ClinicalNoteAddendumController` | Ya |
| Sepuluh pembaca sumber `*MedicalRecordDocumentSource` | **Baru**, di modul pemilik | A4.10 | Menerjemahkan id pasien menjadi dokumen final miliknya | `MedicalRecordTimelineService` | Tidak |
| Pembaca uji kelengkapan `*MedicalRecordCompletenessProbe` | **Baru**, di modul pemilik | A4.11 | Menjawab kriteria, kondisi, dan pemeriksaan tertunda satu kunjungan | `MedicalRecordCompletenessService` | Tidak |

**Penyimpangan yang disengaja dari konvensi "service tanpa interface".** Dua kontrak sumber
(`IMedicalRecordDocumentSource`, `IMedicalRecordCompletenessProbe`) memakai interface dan
didaftarkan banyak implementasi lewat `IEnumerable<>`. Alasannya: rekam medis harus membaca
belasan modul tanpa bergantung pada service internal masing-masing, dan `RM-DEC-038` menuntut
**modul pemilik** yang menerjemahkan id pasien. Preseden yang sama sudah ada di source:
`IBillingChargeSourceAdapter`, `IOpeningItemFileReader` (`IEnumerable<IOpeningItemFileReader>`,
`FinanceOpeningItemBatchService.cs:59-67`). Ini **bukan** utang teknis.

### Controller

| Controller | Status | Lokasi | Service | Atribut akses |
|---|---|---|---|---|
| `MedicalRecordController` | **Diperbarui** | `.../MedicalRecordManagement/Controllers/` | `MedicalRecordTimelineService`, `PeriodicObservationSummaryService` | `[AccessController]`; endpoint baru `[AccessPermission("MedicalRecord", "Read")]` |
| `MedicalRecordCompletenessController` | **Baru** | `.../MedicalRecordManagement/Controllers/` | `MedicalRecordCompletenessService` | `MedicalRecordCompleteness : Read`, `Evaluate` |
| `MedicalRecordCompletenessProfileController` | **Baru** | `Areas/HealthServices/MasterData/Controllers/` | `MedicalRecordCompletenessProfileService` | `MedicalRecordCompletenessProfile : Read`, `Create`, `Update` |
| `ClinicalNoteAddendumController` | **Diperbarui** | `.../MedicalRecordManagement/Controllers/` | `ClinicalNoteAddendumService` | Tidak berubah |

Seluruh controller baru mendeklarasikan `filters/metadata` dan `summary` tepat sesudah
konstruktor, dan **tidak** memakai komentar `<summary>` XML pada action.

## A4.5 Arsitektur folder

```text
Areas/HealthServices/
├── MedicalRecordManagement/
│   ├── Controllers/
│   │   ├── MedicalRecordController.cs                         # Diperbarui
│   │   ├── MedicalRecordCompletenessController.cs             # Baru
│   │   └── ClinicalNoteAddendumController.cs                  # Diperbarui
│   ├── DTOs/
│   │   ├── MedicalRecordTimelineDtos.cs                       # Diperbarui
│   │   ├── MedicalRecordSourceDtos.cs                         # Baru
│   │   ├── MedicalRecordCompletenessDtos.cs                   # Baru
│   │   └── ClinicalNoteAddendumDtos.cs                        # Diperbarui (purpose)
│   ├── Enums/
│   │   ├── ClinicalDocumentKind.cs                            # Diperbarui
│   │   ├── PeriodicObservationKind.cs                         # Baru
│   │   ├── ClinicalClosureSource.cs                           # Baru
│   │   ├── CompletenessState.cs                               # Baru
│   │   ├── CompletenessDeficiencyType.cs                      # Baru
│   │   ├── CompletenessCriterion.cs                           # Baru
│   │   ├── CompletenessCondition.cs                           # Baru
│   │   └── ClinicalNoteAddendumPurpose.cs                     # Baru
│   ├── Models/
│   │   ├── MrcEncounterClinicalClosure.cs                     # Baru
│   │   ├── MrcEncounterCompletenessAssessment.cs              # Baru
│   │   └── MrcClinicalNoteAddendum.cs                         # Diperbarui
│   └── Services/
│       ├── Sources/
│       │   ├── IMedicalRecordDocumentSource.cs                # Baru
│       │   └── IMedicalRecordCompletenessProbe.cs             # Baru
│       ├── MedicalRecordTimelineService.cs                    # Diperbarui
│       ├── PeriodicObservationSummaryService.cs               # Baru
│       ├── MedicalRecordCompletenessService.cs                # Baru
│       ├── MedicalRecordCompletenessRecheckHostedService.cs   # Baru
│       ├── ClinicalDocumentIntegrityService.cs                # Diperbarui
│       └── ClinicalNoteAddendumService.cs                     # Diperbarui
├── MasterData/
│   ├── Controllers/MedicalRecordCompletenessProfileController.cs   # Baru
│   ├── DTOs/MedicalRecordCompletenessProfileDtos.cs                # Baru
│   ├── Models/MstMedicalRecordCompletenessProfile.cs               # Baru
│   ├── Models/MstMedicalRecordCompletenessProfileItem.cs           # Baru
│   └── Services/MedicalRecordCompletenessProfileService.cs         # Baru
├── LaboratoryManagement/Services/LabMedicalRecordDocumentSource.cs        # Baru — milik Lab
├── RadiologyManagement/Services/RadMedicalRecordDocumentSource.cs         # Baru — milik Radiologi
├── EmergencyInstallationManagement/Services/EmgMedicalRecordDocumentSource.cs   # Baru — milik IGD
├── InPatientManagement/Services/InpMedicalRecordDocumentSource.cs         # Baru — milik Rawat Inap
├── OperatingRoomManagement/Services/OprMedicalRecordDocumentSource.cs     # Baru — milik Kamar Operasi
├── HemodialysisManagement/Services/HmdMedicalRecordDocumentSource.cs      # Baru — milik Hemodialisa
├── NutritionManagement/Services/GziMedicalRecordDocumentSource.cs         # Baru — milik Gizi
├── BloodBankManagement/Services/BbkMedicalRecordDocumentSource.cs         # Baru — milik Bank Darah
├── PharmacyManagement/Services/PhmMedicalRecordDocumentSource.cs          # Baru — milik Farmasi
└── MedicalCheckupManagement/Services/McuMedicalRecordDocumentSource.cs    # Baru — milik MCU

Repositories/Configurations/HealthServices/MedicalRecordManagement/
├── MrcEncounterClinicalClosureConfiguration.cs                # Baru
├── MrcEncounterCompletenessAssessmentConfiguration.cs         # Baru
├── MrcClinicalNoteAddendumConfiguration.cs                    # Diperbarui
├── MstMedicalRecordCompletenessProfileConfiguration.cs        # Baru
└── MstMedicalRecordCompletenessProfileItemConfiguration.cs    # Baru

Areas/HealthServices/MasterData/Seeders/
└── MedicalRecordCompletenessProfileSeeder.cs                  # Baru — isi awal A4.8
```

Kelas pembaca uji kelengkapan (`*MedicalRecordCompletenessProbe`) tinggal **di berkas yang sama**
dengan pembaca sumber modul itu bila modul tersebut menjawab kriteria; daftar modulnya pada A4.11.
Configuration kedua master ditaruh di `Repositories/Configurations/HealthServices/MedicalRecordManagement/`,
mengikuti preseden `MstMedicalRecordAccessPurposeConfiguration.cs` yang sudah ada di sana. Isi awal
diisi seeder di `Areas/HealthServices/MasterData/Seeders/`, mengikuti pola seeder master lain di
folder itu, sehingga migration nomor 2 cukup membuat tabel.

**Berkas di modul lain wajib disetujui pemilik modul tersebut.** Sepuluh pembaca sumber ditulis
di folder modul pemiliknya karena `RM-DEC-038` menetapkan pemiliklah yang menerjemahkan. Setiap
task yang menyentuh folder itu memerlukan persetujuan pemilik modulnya (lihat
`contracts/integration-contract.md` bagian amandemen).

## A4.6 Status model dan dampak migration

| Tabel | Status | Kolom yang berubah | Dampak migration |
|---|---|---|---|
| `MrcEncounterClinicalClosure` | **Baru** | Seluruh kolom (data dictionary) | Tabel baru |
| `MrcEncounterCompletenessAssessment` | **Baru** | Seluruh kolom | Tabel baru |
| `MstMedicalRecordCompletenessProfile` | **Baru** | Seluruh kolom | Tabel baru + seed |
| `MstMedicalRecordCompletenessProfileItem` | **Baru** | Seluruh kolom | Tabel baru + seed |
| `MrcClinicalNoteAddendum` | **Diperbarui** | **`AddendumPurpose`** `integer` wajib, bawaan `1` (`Correction`). Index unik terfilter baru `(IntegrityId)` **WHERE** `AddendumPurpose = 2 AND "IsDelete" = false` | Tambah kolom ber-bawaan: seluruh addendum lama otomatis `Correction`. Tanpa downtime |
| Tabel sumber modul lain | Sudah ada | **Nol perubahan** | — |

## A4.7 Rencana migration

| Urutan | Nama | Isi | Tanpa downtime | Data lama | Langkah mundur |
|---:|---|---|:---:|---|---|
| 1 | `AddMrcEncounterClinicalClosure` | Tabel penutupan klinis + index unik `EncounterId` | Ya | **Tidak diisi** — kunjungan lama sengaja tidak punya baris (`RM-DEC-050`) | Hapus tabel |
| 2 | `AddMedicalRecordCompletenessProfile` | Dua tabel master; isi A4.8 lewat `MedicalRecordCompletenessProfileSeeder` (idempoten: hanya menyisipkan kode profil yang belum ada) | Ya | — | Hapus tabel |
| 3 | `AddMrcEncounterCompletenessAssessment` | Tabel penilaian | Ya | Tidak diisi | Hapus tabel |
| 4 | `AddMrcClinicalNoteAddendumPurpose` | Kolom `AddendumPurpose` + index unik terfilter | Ya | Bawaan `1` | Hapus kolom dan index |

Keempatnya aditif dan tidak menyentuh tabel modul lain. Pembuatan dan eksekusi migration tetap
memerlukan instruksi terpisah (`CLAUDE.md` backend).

## A4.8 Rencana data master awal

Sumber nilai: `RM-DEC-047`, `053`–`056`, `060`, `061`. **Bukan** SOP rumah sakit; diselaraskan bila
SOP diserahkan.

| Kode profil | Jenis kunjungan | Jenis unit | Kondisi | Kriteria (A4.11) |
|---|---|---|---|---|
| `RJ` | `Outpatient` | (semua) | `None` | `InitialAssessmentAny`, `PrimaryDiagnosis`, `DoctorNoteOrConsultation` |
| `RJ-LAB` | `Outpatient` | `Laboratory` | `None` | **Tanpa butir** |
| `RJ-RAD` | `Outpatient` | `Radiology` | `None` | **Tanpa butir** |
| `RJ-HD` | `Outpatient` | `Hemodialysis` | `None` | `HemodialysisSessionSigned` |
| `TELE` | `Telemedicine` | (semua) | `None` | `DoctorNoteOrConsultation`, `PrimaryDiagnosis` |
| `IGD` | `Emergency` | (semua) | `None` | `EmergencyTriage`, `InitialAssessmentAny`, `PrimaryDiagnosis`, `EmergencyDisposition` |
| `RI` | `Inpatient` | (semua) | `None` | `InpatientGeneralConsent`, `InitialNursingAssessment`, `InitialMedicalAssessment`, `PrimaryDiagnosis`, `ProgressNoteAny`, `DischargeSummary` |
| `MCU` | `MedicalCheckup` | (semua) | `None` | `McuResume` |
| `K-OK` | (semua) | (semua) | `Surgery` | `SurgeryReport`, `AnesthesiaRecord`, `SurgicalSafetyChecklist` |
| `K-HD` | (semua) | (semua) | `HemodialysisSession` | `HemodialysisSessionSigned` |
| `K-TRF` | (semua) | (semua) | `Transfusion` | `TransfusionMonitoring` |

Kunjungan berjenis `Unknown` sengaja **tidak** punya profil: penilaiannya menghasilkan kekurangan
`RuleNotFound` dan keadaan `Undetermined`, supaya datanya dibetulkan, bukan dianggap lengkap.

## A4.9 Perubahan perilaku pada layanan linimasa yang sudah ada

| No | Perubahan | Sebab |
|---:|---|---|
| 1 | Daftar seluruh jenis = jenis yang punya pembaca (13 jenis lama, model `ClinicalManagement` baru, dan `Kinds` setiap `IMedicalRecordDocumentSource`) **ditambah** jenis tanpa pembaca yang dilaporkan sebagai "belum tersedia" (`PathologyAnatomyResult`) | `RM-CAP-035`; `AC-39`, `AC-49` |
| 2 | Setiap sumber dibaca dengan **batas waktu per sumber**. Batas waktu yang habis dicatat sebagai gangguan sumber. **`OperationCanceledException` hanya diteruskan bila pembatalan berasal dari permintaan pengguna**; pembatalan karena batas waktu sumber dicatat sebagai gangguan. Hari ini seluruh `OperationCanceledException` diteruskan (`MedicalRecordTimelineService.cs:153`), sehingga batas waktu sumber akan membatalkan seluruh berkas bila tidak diubah | `GAP-ARCH-RM-05`, `RM-DEC-044` |
| 3 | Sumber tetap dibaca **berurutan**, tidak serentak: seluruh sumber memakai satu `ApplicationDbContext` per permintaan, dan EF Core menolak dua query bersamaan pada satu context | Keputusan desain — lihat A4.14 |
| 4 | Setiap baris membawa `IsLateEntry` (dari `MrcEncounterClinicalClosure`), `IsCorrected`, `IsProvisional` | `RM-DEC-042`, `043`, `059` |
| 5 | Pencatatan berkala dikeluarkan sebagai satu baris `PeriodicObservationSummary` per jenis per hari | `RM-DEC-052` |
| 6 | Pemberitahuan cakupan di frontend mengikuti daftar jenis yang benar-benar ditegakkan | `RM-CAP-041` |

## A4.10 Kontrak sumber per modul (mempertajam `DC-RM-03`)

Seluruh pembaca hanya mengembalikan dokumen **final** menurut kolom di bawah. Kolom diambil dari
capability map revision 3 kelompok F dan G, dan dari `RM-DEC-059` untuk tiga sumber sementara.

| Pembaca | Lokasi | Jenis | Final bila | Waktu final | Jalan ke pasien | `IsCorrected` | `IsProvisional` |
|---|---|---|---|---|---|---|---|
| `LabMedicalRecordDocumentSource` | `LaboratoryManagement/Services/` | `LaboratoryResult` (disiplin PK dan Mikrobiologi) | `ReleasedAt != null` | `ReleasedAt` | `LabOrder.EncounterId` → kunjungan pasien | Selalu `false` sampai `S6` (`RM-DEC-057`) | Tidak |
| sama | sama | `PathologyAnatomyResult` | **Tidak dibaca** — dilaporkan "belum tersedia" (`RM-DEC-058`) | — | — | — | — |
| `RadMedicalRecordDocumentSource` | `RadiologyManagement/Services/` | `RadiologyReport` | Memakai `RadReportService.GetByPatientAsync` apa adanya | `FirstReleasedAt` | Sudah ditangani service itu | `CurrentVersionNumber > 1` | Tidak |
| `EmgMedicalRecordDocumentSource` | `EmergencyInstallationManagement/Services/` | `EmergencyTriage` | `Completed` | `CompletedAt` | `EmgVisit.PatientId` | Ada triase pengganti (`Superseded`) — ditampilkan sebagai triase lama yang digantikan | Tidak |
| sama | sama | `EmergencyObservation`, `EmergencyResuscitation` | `Completed`, `Escalated` / `Completed`, `Stopped` | `EndedAt` / `CompletedAt` | sama | `false` | Tidak |
| sama | sama | `EmergencyDisposition` | `Confirmed` atau `Executed` (sementara, `RM-DEC-059`) | `ConfirmedAt` | sama | `false` | Tidak |
| `InpMedicalRecordDocumentSource` | `InPatientManagement/Services/` | `DischargeSummary` | `SignedAt != null` | `SignedAt` | `InpEpisode.PatientId` | Ada `InpDischargeSummaryRevision` | Tidak |
| sama | sama | `AdmissionDocument` | `Status = Completed` | `CompletedAt` | `PatientId` | Ada versi `Superseded` sebelumnya | Tidak |
| `OprMedicalRecordDocumentSource` | `OperatingRoomManagement/Services/` | `SurgeryReport`, `AnesthesiaRecord` | `Final` | `FinalizedAt` | `OprCase.PatientId` | Laporan operasi: ada `OprExecutionAddendum` | Tidak |
| sama | sama | `SurgicalSafetyChecklist` | `Completed` | `SignedAt` | sama | `Revision > 1` | Tidak |
| sama | sama | `BirthRecord` | Saat tersimpan (sementara) | `CreateDateTime` | sama | `false` | Tidak |
| `HmdMedicalRecordDocumentSource` | `HemodialysisManagement/Services/` | `HemodialysisSession` | `Finalized` | `SignedAt` | `HmdEpisode.PatientId` | `AddendumCount > 0` pada keutuhan | Tidak |
| `GziMedicalRecordDocumentSource` | `NutritionManagement/Services/` | `NutritionCareRecord` | Saat tersimpan (sementara) | `CreateDateTime` | `GziNutritionOrder.PatientId` | `false` | **Ya** — "dapat berubah" |
| `BbkMedicalRecordDocumentSource` | `BloodBankManagement/Services/` | `BloodGroupExam`, `TransfusionReactionNotice` | `Validated` / `Acknowledged` | `ValidatedAt` / `AcknowledgedAt` | `PatientId` | Golongan darah: ada `BbkBloodGroupConflictResolution` | Tidak |
| `PhmMedicalRecordDocumentSource` | `PharmacyManagement/Services/` | `Prescription`, `MedicationAdministration`, `MedicationReconciliation` | `Submitted` / `Administered` / ada keputusan | `SubmittedAt` / `AdministeredAt` / `DecidedAt` | `PatientId` (rekonsiliasi lewat episode) | Pemberian: ada `PhmMedicationAdministrationRevision` | Tidak |
| `McuMedicalRecordDocumentSource` | `MedicalCheckupManagement/Services/` | `McuResume`, `McuExaminationResult` | ~~Resume `Verified`/`Corrected`~~ → **versi terakhir yang pernah diverifikasi** (`VerifiedAt` terisi), berketerangan "sedang dikoreksi" bila ada versi lebih baru yang belum diverifikasi (`RM-DEC-065`); hasil mengikuti versi itu (`RM-DEC-066`) | `VerifiedAt` versi itu | `McuOrder.PatientId` — peserta tanpa pasien tidak terbaca | ~~Resume `Corrected` atau `ReopenedAt`~~ → resume: versi > 1; hasil: ada revisi nilai dengan versi resume < versi terverifikasi | Tidak |

**Model `ClinicalManagement` baru** (`NursingCarePlan` sampai `TransferHandover`, dan enam
pencatatan berkala) dibaca di `MedicalRecordTimelineService` lewat penarik generik yang sudah
ada, karena seluruhnya menyimpan `PatientId` (`RM-CAP-042`), kecuali `CliTransferHandover` yang
diterjemahkan lewat `InpEpisodeId`.

**Rincian dokumen** (`GET /{patientId}/documents/{documentKind}/{documentId}`) diteruskan ke
`ReadDetailAsync` pemilik. Isinya dibaca saat diminta dan **tidak** disimpan.

## A4.11 Katalog kriteria (`DC-RM-06a`) dan pembaca uji

| Nilai `CompletenessCriterion` | Terpenuhi bila | Dijawab oleh | Keputusan |
|---|---|---|---|
| `InitialAssessmentAny = 1` | Ada `TrxPatientAssessment` `Initial` atau `MedicalInitial` berstatus `Completed` | Rekam medis (generik) | `RM-DEC-053` |
| `InitialNursingAssessment = 2` | `Initial` `Completed` | Rekam medis | `RM-DEC-053` |
| `InitialMedicalAssessment = 3` | `MedicalInitial` `Completed` | Rekam medis | `RM-DEC-053` |
| `PrimaryDiagnosis = 4` | `IsPrimary` **atau** `DiagnosisType = Primary`, bukan `Cancelled`/`RuledOut` | Rekam medis | `RM-DEC-054` |
| `DoctorNoteOrConsultation = 5` | CPPT berprofesi dokter **atau** `TrxDoctorConsultation`, tidak dibatalkan | Rekam medis | `RM-DEC-055` |
| `ProgressNoteAny = 6` | Minimal satu CPPT tidak dibatalkan | Rekam medis | `RM-DEC-047` |
| `EmergencyTriage = 7` | Triase `Completed` | `EmgMedicalRecordDocumentSource` | `RM-DEC-047` |
| `EmergencyDisposition = 8` | Disposisi `Confirmed` atau `Executed` | sama | `RM-DEC-059` |
| `InpatientGeneralConsent = 9` | Dokumen admisi `GeneralConsent` `Completed` | `InpMedicalRecordDocumentSource` | `RM-DEC-056` |
| `DischargeSummary = 10` | Ringkasan pulang `SignedAt != null` | sama | `RM-DEC-047` |
| `McuResume = 11` | Ada versi resume yang pernah diverifikasi (`RM-DEC-065`; dikoreksi dari "`Verified`/`Corrected`") | `McuMedicalRecordDocumentSource` | `RM-DEC-047`, `065` |
| `SurgeryReport = 12` | `OprExecutionRecord` `Final` | `OprMedicalRecordDocumentSource` | `RM-DEC-047` |
| `AnesthesiaRecord = 13` | `OprAnesthesiaRecord` `Final` | sama | `RM-DEC-047` |
| `SurgicalSafetyChecklist = 14` | Checklist `Completed` | sama | `RM-DEC-047` |
| `HemodialysisSessionSigned = 15` | Setiap sesi `Completed` ke atas punya keutuhan `Signed` | `HmdMedicalRecordDocumentSource` | `RM-DEC-047`, `060` |
| `TransfusionMonitoring = 16` | Setiap monitoring yang tidak batal berstatus `Completed` | Rekam medis (generik) | `RM-DEC-047` |

| Kondisi | Ada bila | Dijawab oleh |
|---|---|---|
| `Surgery` | Ada `OprCase` kunjungan itu berstatus `Completed` | `OprMedicalRecordDocumentSource` |
| `HemodialysisSession` | Ada `HmdSession` kunjungan/episode itu berstatus `Completed` ke atas | `HmdMedicalRecordDocumentSource` |
| `Transfusion` | Ada `CliTransfusionMonitoring` tidak batal | Rekam medis (generik) |

| Pemeriksaan tertunda (syarat 2) | Dijawab oleh |
|---|---|
| Pemeriksaan lab yang tidak `Voided`/`Cancelled` dan `ReleasedAt = null`, **termasuk PA** | `LabMedicalRecordDocumentSource` |
| Order radiologi tidak batal tanpa laporan yang dirilis | `RadMedicalRecordDocumentSource` |

**Penyusunan profil satu kunjungan:** (1) cari profil `None` dengan jenis kunjungan dan jenis unit
yang persis; bila tidak ada, profil `None` jenis kunjungan dengan unit kosong; bila tidak ada
juga, kekurangan `RuleNotFound`. (2) Tambahkan setiap profil kondisi yang kondisinya ada. (3)
Kriteria kembar dihitung satu kali.

## A4.12 Penutupan klinis dan addendum pengesahan

**Penutupan klinis.** `LockOpenDocumentsForEncounterAsync` mencatat baris
`MrcEncounterClinicalClosure` pada panggilan pertama untuk kunjungan itu, memakai
`encounterClosedAtUtc ?? nowUtc` dan sumber pemanggil (parameter baru `ClinicalClosureSource`).
Panggilan berikutnya tidak mengubah apa pun. Tabrakan index unik pada dua penutupan serentak
diperlakukan sebagai "sudah tercatat". Jalur disposisi rawat inap IGD
(`EmergencyDispositionService.AdmissionRequest.cs:1585-1598`) **wajib** memanggilnya
(`GAP-ARCH-RM-01`) — perubahan pada berkas milik IGD.

**Waktu tutup klinis data lama** (hanya untuk penilaian atas permintaan, `RM-DEC-050`), urutan
sumber pertama yang tersedia:

1. `MrcClinicalDocumentIntegrity.LockedEncounterClosedAt` paling awal pada kunjungan itu;
2. `InpEpisode.ClosedAt` untuk kunjungan rawat inap;
3. `EmgVisit.VisitCompletedAt` untuk kunjungan IGD;
4. `RegPatientEncounter.CompletedAt`.

Waktu turunan ini **tidak disimpan** dan ditandai "diturunkan" pada respons.

**Addendum pengesahan.** Syarat: catatan berstatus `LockedUnsigned`; pembuat mengikuti aturan
addendum yang ada (penulis asli, atau pengganti lewat `as-substitute`); belum ada addendum
pengesahan lain pada catatan itu. Addendum di sistem ini sudah ditandatangani saat dibuat
(`MrcClinicalNoteAddendum.SignedAt` wajib), sehingga **pembuatan = penandatanganan**. Status
keutuhan catatan tidak diubah. Sesudah tersimpan, kunjungan catatan itu ditandai untuk dinilai
ulang.

## A4.13 Pemeriksaan berkala

| Parameter | Nilai bawaan | Letak | Alasan |
|---|---|---|---|
| Jarak pemeriksaan untuk kunjungan belum `Complete` yang tutup klinis ≤ 30 hari | 60 menit | `appsettings`: `MedicalRecordCompleteness:RecentRecheckMinutes` | Hasil lab rutin dan tanda tangan biasanya menyusul dalam hitungan jam |
| Jarak pemeriksaan untuk kunjungan belum `Complete` yang lebih lama | 24 jam | `MedicalRecordCompleteness:OldRecheckHours` | Kultur dan PA dapat berminggu-minggu (`RM-DEC-035`); tidak ada batas akhir |
| Jumlah kunjungan per putaran | 200 | `MedicalRecordCompleteness:BatchSize` | Sama dengan ukuran batch penguncian yang ada |
| Kunjungan `Complete` | Tidak diperiksa berkala; hanya saat dibuka atau profil berubah | — | — |

Kolom `NextCheckAt` pada penilaian menentukan giliran. Angka di atas adalah **bawaan teknis**,
bukan kebijakan rumah sakit, dan boleh diubah per lingkungan.

## A4.14 Batas waktu per sumber

| Parameter | Nilai bawaan | Letak |
|---|---|---|
| Batas waktu satu sumber | 3 detik | `MedicalRecord:SourceTimeoutSeconds` |
| Batas waktu seluruh berkas | 20 detik | `MedicalRecord:FileTimeoutSeconds` |

Sumber yang belum dibaca saat batas berkas habis dicatat sebagai gangguan, tidak dilewati diam-diam.
Bila kelak pembacaan berurutan terbukti terlalu lambat, pembacaan serentak memerlukan context
terpisah per sumber — dicatat sebagai pilihan lanjutan, tidak dirancang sekarang.

## A4.15 Yang sengaja tidak dibuat

| Yang ditolak | Alasan |
|---|---|
| Tabel salinan dokumen modul lain | Dilarang `RM-DEC-030`, `038` |
| Tabel anak kekurangan (`MrcEncounterCompletenessDeficiency`) | Penilaian ulang berkala akan menghasilkan ribuan baris bertanda hapus; `jsonb` cukup karena kekurangan selalu dibaca utuh bersama penilaiannya |
| Status keutuhan baru `Attested` | `RM-DEC-049`: status catatan tetap `LockedUnsigned` |
| Kolom `PatientId` pada tabel Laboratorium | Dilarang `RM-DEC-031` |
| Endpoint HTTP berlingkup pasien di modul sumber untuk dipanggil rekam medis | Akan dijaga izin modul asal, bertentangan dengan `RM-DEC-040`; pembaca dipanggil dalam proses |
| Konfigurasi tanggal mulai daftar kerja | Tidak perlu: hanya kunjungan yang punya baris penutupan klinis yang masuk daftar kerja, dan baris itu baru lahir sejak fitur dirilis |
| Riwayat perubahan keadaan penilaian | `RM-DA-001` bagian I: cukup keadaan terakhir dan waktu pertama kali lengkap |
| Event "dokumen final" dari modul sumber | Tidak ada di source; pemeriksaan berkala menggantikannya (`GAP-ARCH-RM-04`) |
| Pembaca hasil Patologi Anatomi | `RM-DEC-058` |
