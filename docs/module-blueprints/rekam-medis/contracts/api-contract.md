# API Contract — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Contract version | `0.1.0` |
| Status | `draft` |
| Owner | API authority: `OPEN`; Product/domain: `OPEN` |
| `approved_by` / `approved_at` | — / — |
| Input revisions | `00-interview-decisions.md` revision `2`; `01-existing-capability-map.md` revision `1` |
| Backend SHA | `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` |
| Compatibility impact | **Aditif.** Seluruh endpoint baru. Tidak ada endpoint berjalan yang berubah bentuk permintaan maupun responsnya. Yang berubah adalah **perilaku** dua endpoint yang sudah ada, dijelaskan pada bagian 7 |

> **PERINGATAN DASAR DESAIN.** Kontrak ini disusun di atas keputusan berstatus `draft` yang
> belum disetujui owner mana pun. Lihat `RM-DEC-025`.

Seluruh endpoint pada dokumen ini berlabel **Rencana (belum tersedia)** kecuali dinyatakan
lain. Tidak ada satu pun yang sudah dapat dipanggil.

Pembungkus respons mengikuti konvensi project: `ApiResponse<T>.Ok(data, pesan)` untuk berhasil
dan `ApiResponse<T>.Fail(kode, pesan)` untuk gagal.

---

## 1. As-is contract — yang sudah ada dan dipakai

Bagian ini memisahkan kenyataan sekarang dari rencana. Hanya satu grup yang relevan.

### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`
Status: **sudah berjalan dan dipakai frontend**

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/timeline` | Riwayat CPPT per pasien atau per kunjungan | `PatientIntegratedProgressNote : Read` | Tersedia |
| `GET` | `/filters/metadata` | Daftar pilihan penyaring | `PatientIntegratedProgressNote : Read` | Tersedia |
| `GET` | `/{id}` | Detail satu CPPT | `PatientIntegratedProgressNote : Read` | Tersedia |
| `POST` | `/from-consultation/{consultationId}` | Membuat CPPT dari konsultasi | `PatientIntegratedProgressNote : Create` | Tersedia |
| `PUT` | `/{id}` | Mengubah CPPT | `PatientIntegratedProgressNote : Update` | Tersedia |
| `PATCH` | `/{id}/cancel` | Membatalkan CPPT | `PatientIntegratedProgressNote : Delete` | Tersedia |

Bukti: `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs:51-565`,
dipakai `src/lib/services/health-services/clinical-management/patient-integrated-progress-note.service.js:38`.

Perbedaan terhadap kebutuhan target: endpoint `/timeline` hanya mengembalikan CPPT. Modul rekam
medis membutuhkan riwayat gabungan dari tiga belas sumber, dan itu belum ada.

---

## 2. Health Services / Medical Record Management / Medical Record

Base URL: `api/v1/health-services/medical-record-management/medical-records`
Contract version: `0.1.0` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{patientId}/summary` | Ringkasan berkas: identitas, alergi aktif, diagnosis aktif, jumlah dokumen per jenis | `MedicalRecord : Read` | Query: `accessPurposeId`, `accessReason` | `ApiResponse<MedicalRecordSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{patientId}/timeline` | Riwayat gabungan lintas kunjungan, urut waktu | `MedicalRecord : Read` | Query: `documentKinds`, `encounterId`, `startDate`, `endDate`, `accessPurposeId`, `accessReason`, `page`, `pageSize` | `ApiResponse<PagedResult<MedicalRecordTimelineItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}` | Detail satu dokumen beserta addendumnya | `MedicalRecord : Read` | Query: `accessPurposeId`, `accessReason` | `ApiResponse<MedicalRecordDocumentDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}/private-note` | Membuka `PrivateNote`, selalu lewat jalur akses beralasan | `MedicalRecord : ReadPrivateNote` | Query: `accessPurposeId` **wajib**, `accessReason` **wajib** | `ApiResponse<MedicalRecordPrivateNoteResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/filters/metadata` | Daftar pilihan penyaring dan keperluan akses | `MedicalRecord : Read` | — | `ApiResponse<MedicalRecordFilterMetadataResponse>` | **Rencana (belum tersedia)** |

Kode status dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
|---:|---|
| `200` | Berkas berhasil dibuka. Satu baris jejak akses sudah tercatat |
| `400` | Permintaan tidak lengkap, misalnya alasan akses kosong padahal pasien tidak punya kunjungan aktif |
| `401` | Belum masuk, atau sesi sudah berakhir |
| `403` | Tidak punya hak akses ke menu rekam medis |
| `404` | Pasien tidak ditemukan |
| `409` | Pasien merupakan hasil penggabungan nomor rekam medis; buka nomor penggantinya |
| `503` | Jejak akses gagal dicatat, sehingga isi tidak dikembalikan. Coba lagi |

Dua kode status terakhir perlu penjelasan karena tidak lazim.

`409` menjawab keterbatasan nomor 6 pada arsitektur backend. Bila `MstPatient.MergedToPatientId`
terisi, riwayat pasien berpotensi tampil terpecah. Daripada menampilkan riwayat yang terpotong
tanpa memberi tahu pembacanya, permintaan ditolak disertai nomor rekam medis penggantinya.

`503` adalah penerapan aturan "gagal mencatat jejak berarti gagal membaca". Ini pilihan yang
menutup rapat: lebih baik pengguna mencoba lagi daripada ada pembacaan yang tidak tercatat.

---

## 3. Health Services / Medical Record Management / Clinical Document Integrity

Base URL: `api/v1/health-services/medical-record-management/clinical-document-integrities`
Contract version: `0.1.0` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-document/{documentKind}/{documentId}` | Status keutuhan satu dokumen | `ClinicalDocumentIntegrity : Read` | — | `ApiResponse<ClinicalDocumentIntegrityResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/by-document/{documentKind}/{documentId}/sign` | Menandatangani dan mengunci dokumen | `ClinicalDocumentIntegrity : Update` | `SignClinicalDocumentRequest` | `ApiResponse<ClinicalDocumentIntegrityResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/my-unsigned` | Daftar catatan milik pengguna yang belum ditandatangani | `ClinicalDocumentIntegrity : Read` | Query: `page`, `pageSize` | `ApiResponse<PagedResult<UnsignedDocumentResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/by-encounter/{encounterId}` | Daftar keutuhan seluruh dokumen dalam satu kunjungan | `ClinicalDocumentIntegrity : Read` | — | `ApiResponse<List<ClinicalDocumentIntegrityResponse>>` | **Rencana (belum tersedia)** |

Kode status:

| Kode | Arti bagi pengguna |
|---:|---|
| `200` | Berhasil |
| `400` | Dokumen sudah terkunci, jadi tidak dapat ditandatangani lagi |
| `403` | Anda bukan penulis catatan ini, jadi tidak dapat menandatanganinya |
| `404` | Dokumen tidak ditemukan, atau belum terdaftar pada daftar keutuhan |

`SignClinicalDocumentRequest` tidak memuat kata sandi maupun data biometrik apa pun. Ini
penerapan `RM-DEC-021`: tanda tangan cukup memakai identitas pengguna yang sedang masuk.
Perangkat dan alamat IP diambil server dari permintaan, bukan dikirim klien — bila dikirim
klien, nilainya dapat dipalsukan dan kehilangan makna sebagai bukti.

Endpoint `/my-unsigned` bukan pelengkap. Tanpa layar yang menunjukkan "catatan saya yang belum
saya tandatangani", dokter tidak punya cara menemukannya, dan seluruh catatan akan berakhir
sebagai `LockedUnsigned` saat kunjungan ditutup — hasil yang berlawanan dengan tujuan
`RM-DEC-003`.

---

## 4. Health Services / Medical Record Management / Clinical Note Addendum

Base URL: `api/v1/health-services/medical-record-management/clinical-note-addendums`
Contract version: `0.1.0` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-document/{documentKind}/{documentId}` | Daftar addendum sebuah dokumen, urut `Sequence` | `ClinicalNoteAddendum : Read` | — | `ApiResponse<List<ClinicalNoteAddendumResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/by-document/{documentKind}/{documentId}` | Membuat addendum | `ClinicalNoteAddendum : Create` | `CreateClinicalNoteAddendumRequest` | `ApiResponse<ClinicalNoteAddendumResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/authority/{documentKind}/{documentId}` | Memeriksa apakah pengguna berhak membuat addendum, dan atas dasar apa | `ClinicalNoteAddendum : Read` | — | `ApiResponse<AddendumAuthorityResponse>` | **Rencana (belum tersedia)** |

`CreateClinicalNoteAddendumRequest`:

| Field | Tipe | Wajib | Batas | Keterangan |
|---|---|:---:|---|---|
| `AddendumText` | `string` | Ya | 4000 | Isi koreksi |
| `CorrectionReason` | `string` | Ya | 500 | Alasan koreksi. Wajib tanpa kecuali |

Yang **tidak** ada di permintaan ini, dan sengaja demikian: `AuthorUserId`,
`IsSubstituteAuthor`, dan `DelegationId`. Ketiganya ditentukan server. Bila klien boleh
mengirimkannya, pembuat addendum dapat mengaku sebagai orang lain — persis celah `RM-CAP-012`
yang sedang ditutup. Pelajaran dari celah itu diterapkan sejak awal di sini.

Kode status:

| Kode | Arti bagi pengguna |
|---:|---|
| `201` | Addendum berhasil ditambahkan |
| `400` | Dokumen belum terkunci, jadi koreksi dilakukan dengan mengubah catatannya langsung, bukan lewat addendum |
| `403` | Anda bukan penulis catatan ini, dan tidak ada penetapan yang memberi Anda kewenangan pengganti |
| `404` | Dokumen tidak ditemukan |

Endpoint `/authority` memungkinkan frontend menampilkan tombol addendum hanya kepada yang
berhak, sekaligus menjelaskan alasannya bila tidak berhak. Menampilkan tombol yang selalu
gagal saat ditekan adalah pengalaman yang buruk dan mendorong pengguna mencari jalan lain.

---

## 5. Health Services / Medical Record Management / Clinical Note Author Delegation

Base URL: `api/v1/health-services/medical-record-management/clinical-note-author-delegations`
Contract version: `0.1.0` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar penetapan berhalangan | `ClinicalNoteAuthorDelegation : Read` | Query: `originalAuthorUserId`, `isActive`, `page`, `pageSize` | `ApiResponse<PagedResult<AuthorDelegationResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menetapkan seorang penulis berhalangan | `ClinicalNoteAuthorDelegation : Create` | `CreateAuthorDelegationRequest` | `ApiResponse<AuthorDelegationResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/revoke` | Mencabut penetapan lebih awal | `ClinicalNoteAuthorDelegation : Update` | `RevokeAuthorDelegationRequest` | `ApiResponse<AuthorDelegationResponse>` | **Rencana (belum tersedia)** |

`CreateAuthorDelegationRequest`:

| Field | Tipe | Wajib | Batas | Keterangan |
|---|---|:---:|---|---|
| `OriginalAuthorUserId` | `Guid` | Ya | — | Penulis yang dinyatakan berhalangan |
| `GrantReason` | `string` | Ya | 500 | Alasan penetapan |
| `ValidUntil` | `DateTime` | Ya | — | Batas berlaku. **Wajib** — penetapan tanpa batas waktu ditolak |

Endpoint ini hanya membuat penetapan bertipe `UnitHeadGrant`. Penetapan bertipe
`InactiveAccount` tidak dibuat lewat API mana pun; sistem menyimpulkannya sendiri dari keadaan
akun. Ini disengaja: keadaan yang dapat disimpulkan otomatis tidak boleh bergantung pada
seseorang mengingat untuk mencatatnya.

---

## 6. Health Services / Medical Record Management / Medical Record Access Log

Base URL: `api/v1/health-services/medical-record-management/medical-record-access-logs`
Contract version: `0.1.0` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar jejak akses | `MedicalRecordAccessLog : Read` | Query: `patientId`, `userId`, `accessType`, `isFlaggedForReview`, `startDate`, `endDate`, `page`, `pageSize` | `ApiResponse<PagedResult<MedicalRecordAccessLogResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/pending-review` | Antrean akses yang belum ditinjau | `MedicalRecordAccessLog : Read` | Query: `page`, `pageSize` | `ApiResponse<PagedResult<MedicalRecordAccessLogResponse>>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/mark-reviewed` | Menandai satu akses sudah ditinjau | `MedicalRecordAccessLog : Update` | `MarkAccessReviewedRequest` | `ApiResponse<MedicalRecordAccessLogResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Rekap jumlah akses per jenis dan per periode | `MedicalRecordAccessLog : Read` | Query: `startDate`, `endDate` | `ApiResponse<MedicalRecordAccessSummaryResponse>` | **Rencana (belum tersedia)** |

Tidak ada endpoint `POST`, `PUT`, maupun `DELETE` untuk jejak akses, dan itu bukan kelalaian.
Baris jejak hanya dibuat sistem saat rekam medis dibuka, tidak pernah oleh permintaan manusia.
Baris jejak juga tidak dapat diubah maupun dihapus. Satu-satunya perubahan yang diizinkan
adalah menandainya sudah ditinjau — yang menambah keterangan, bukan mengubah isi jejak.

Perhatian privasi pada endpoint ini: daftar jejak akses memuat `AccessReason` yang bertanda
sensitif, sebab alasan akses dapat mengungkap keadaan pasien. Karena itu hak akses
`MedicalRecordAccessLog : Read` **tidak** boleh diberikan seluas hak baca rekam medis, dan
sebaiknya terbatas pada unit rekam medis serta auditor.

---

## 7. Health Services / Master Data / Medical Record Access Purpose

Base URL: `api/v1/health-services/master-data/medical-record-access-purposes`
Contract version: `0.1.0` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar keperluan akses | `MedicalRecordAccessPurpose : Read` | Query: `search`, `isActive`, `page`, `pageSize` | `ApiResponse<PagedResult<MedicalRecordAccessPurposeResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Pilihan untuk kotak isian alasan | `MedicalRecordAccessPurpose : Read` | — | `ApiResponse<List<OptionResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail satu keperluan | `MedicalRecordAccessPurpose : Read` | — | `ApiResponse<MedicalRecordAccessPurposeResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah keperluan | `MedicalRecordAccessPurpose : Create` | `CreateMedicalRecordAccessPurposeRequest` | `ApiResponse<MedicalRecordAccessPurposeResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah keperluan | `MedicalRecordAccessPurpose : Update` | `UpdateMedicalRecordAccessPurposeRequest` | `ApiResponse<MedicalRecordAccessPurposeResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan | `MedicalRecordAccessPurpose : Update` | `StatusRequest` | `ApiResponse<object>` | **Rencana (belum tersedia)** |

---

## 8. Perubahan perilaku pada endpoint yang sudah ada

Bagian ini yang paling penting bagi implementer, karena menyentuh kode berjalan. **Tidak ada
perubahan pada bentuk permintaan maupun respons.** Yang berubah hanya perilakunya.

### `PUT api/v1/health-services/clinical-management/patient-integrated-progress-notes/{id}`

| Perubahan | Sebelum | Sesudah | Menutup |
|---|---|---|---|
| Pemeriksaan keutuhan | Tidak ada | Menolak bila dokumen `Signed` atau `LockedUnsigned`, dengan pesan yang mengarahkan ke addendum | `RM-CAP-011` |
| `ProviderUserId` | Ditetapkan dari isi permintaan, baris 533 | **Tidak lagi ditetapkan dari permintaan.** Penentu penulis yang sah adalah `AuthorUserId` pada tabel keutuhan | `RM-CAP-012` |
| `IsReadOnlyGenerated` | Ditetapkan dari isi permintaan, baris 550 | **Tidak lagi ditetapkan dari permintaan** | `RM-CAP-013` |

Dampak kompatibilitas yang harus diketahui: klien yang selama ini mengirim `ProviderUserId`
atau `IsReadOnlyGenerated` pada permintaan ubah **tidak akan menerima galat**, tetapi nilainya
diabaikan. Ini pilihan sadar — menolak permintaan akan memutus frontend yang sedang berjalan,
sedangkan mengabaikan nilai menutup celah tanpa memutus siapa pun. Perilaku baru ini wajib
disebut pada catatan rilis, sebab diam-diam mengabaikan kiriman klien tanpa pemberitahuan juga
bukan praktik yang baik.

Kode status baru: `400` bila dokumen sudah terkunci, dengan pesan
*"Catatan ini sudah ditandatangani dan tidak dapat diubah. Gunakan addendum untuk membetulkan."*

### `POST api/v1/health-services/clinical-management/patient-integrated-progress-notes/from-consultation/{consultationId}`

Bertambah satu langkah: mendaftarkan baris keutuhan berstatus `Draft` setelah CPPT dibuat,
dalam transaksi yang sama. Bila pendaftaran gagal, pembuatan CPPT ikut dibatalkan — CPPT tanpa
baris keutuhan adalah keadaan yang tidak boleh terjadi, karena dokumen itu akan luput dari
seluruh aturan penguncian.

### `PATCH api/v1/health-services/registration-management/patient-encounters/{id}/status`

Bertambah satu langkah: ketika status berpindah **menuju** `Completed`, seluruh dokumen
berstatus `Draft` pada kunjungan itu dikunci menjadi `LockedUnsigned`, dalam transaksi yang
sama. Bila penguncian gagal, penutupan kunjungan ikut dibatalkan.

Perlu diketahui: endpoint ini tidak memiliki validasi perpindahan status (`RM-CAP-019`),
sehingga status dapat melompat dari nilai mana pun ke `Completed`. Penguncian karena itu
dipicu oleh perpindahan menuju `Completed`, bukan oleh urutan tertentu. Ini bukan perbaikan
atas `RM-CAP-019` — celah itu tetap terbuka dan tercatat sebagai keterbatasan nomor 5.

---

## 9. Traceability

| Endpoint atau perubahan | Decision | Capability yang ditutup | Acceptance test |
|---|---|---|---|
| `GET /{patientId}/timeline` | `RM-DEC-002` | `RM-CAP-004` | `AT-RM-09` |
| `GET /{patientId}/summary` | `RM-DEC-002` | `RM-CAP-004` | `AT-RM-09` |
| `POST /sign` | `RM-DEC-003`, `RM-DEC-021` | `RM-CAP-014` | `AT-RM-02`, `AT-RM-15` |
| `GET /my-unsigned` | `RM-DEC-003` | — | `AT-RM-18` |
| `POST` addendum | `RM-DEC-004`, `RM-DEC-020` | `RM-CAP-016` | `AT-RM-04`, `AT-RM-05`, `AT-RM-14` |
| Jejak akses pada setiap `GET` rekam medis | `RM-DEC-005`, `RM-DEC-015`, `RM-DEC-016` | `RM-CAP-022`, `RM-CAP-024` | `AT-RM-06`, `AT-RM-07`, `AT-RM-12` |
| `GET /private-note` | `RM-DEC-022` | `RM-CAP-027` | `AT-RM-16` |
| Perubahan `PUT` CPPT | `RM-DEC-019` | `RM-CAP-011`, `012`, `013` | `AT-RM-01`, `AT-RM-19`, `AT-RM-20` |
| Perubahan `PATCH` status kunjungan | `RM-DEC-003` | `RM-CAP-018` | `AT-RM-03` |
| Perilaku `SuperAdmin` | `RM-DEC-017` | `RM-CAP-025` | `AT-RM-13` |
