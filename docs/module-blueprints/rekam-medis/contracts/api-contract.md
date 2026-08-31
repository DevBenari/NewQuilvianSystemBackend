# API Contract — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Contract version | `0.1.1` — penyegaran status, **bentuk kontrak tidak berubah** |
| Status | **`approved`** |
| Owner | API authority: **Yoga Aji Pratama**; Product/domain: **Yoga Aji Pratama** |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 27 Agustus 2026 (`RM-DEC-028`); `0.1.1` disahkan 27 Agustus 2026 |
| Input revisions | `00-interview-decisions.md` revision `4`; `01-existing-capability-map.md` revision `2` |
| Backend SHA | `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` |
| Compatibility impact | **Aditif.** Seluruh endpoint baru. Tidak ada endpoint berjalan yang berubah bentuk permintaan maupun responsnya. Yang berubah adalah **perilaku** dua endpoint yang sudah ada, dijelaskan pada bagian 7 |

> **KONTRAK INI SUDAH DISAHKAN.** Pengesahan dilakukan Yoga Aji Pratama selaku pemilik API pada
> 27 Agustus 2026, dicatat pada `RM-DEC-028`. Dengan itu **gerbang paralel frontend terbuka**:
> sepuluh task `FE-00` sampai `FE-09` tidak lagi `TERTAHAN KONTRAK`.
>
> **Dua delta yang ikut disahkan**, keduanya diterapkan `BE-14` dan dirinci pada bagian 2:
> bentuk balasan `/timeline` berubah menjadi selubung `MedicalRecordTimelineResponse`, dan field
> `access` ditambahkan pada seluruh balasan endpoint berkas rekam medis.
>
> Batas yang tetap berlaku, sama seperti `RM-DEC-027`: tinjauan komite medik dan pihak
> perlindungan data belum dilakukan. Bila tinjauan itu kelak menghasilkan keputusan berbeda,
> bagian kontrak yang bergantung padanya wajib dirombak.

### Revisi `0.1.1` — 27 Agustus 2026

**Yang berubah hanya kolom Status.** Tidak satu pun path, method, hak akses, bentuk permintaan,
atau bentuk balasan berubah. Klien yang ditulis di atas `0.1.0` **tidak perlu disesuaikan
sedikit pun**. Ini penting dinyatakan di depan: kenaikan versi kontrak biasanya berarti
bentuknya bergeser, dan di sini tidak.

Alasannya: kolom status pada `0.1.0` sudah usang. Bagian 3, 5, dan 6 masih menandai grupnya
`Rencana (belum tersedia)` padahal ketiga controller-nya sudah ada sejak 26 Agustus 2026.
Kontrak yang menyatakan endpoint belum dibangun padahal sudah hidup membuat frontend menunda
pekerjaan yang sebenarnya tidak tertahan apa pun.

Penyegaran ini dihitung dari atribut `[Route]`, `[Http*]`, dan `[AccessPermission]` pada
`Areas/HealthServices/MedicalRecordManagement/Controllers/`, bukan dari catatan roadmap:

| Grup | Direncanakan | Hidup | `0.1.0` menyatakan | `0.1.1` menyatakan |
|---|---:|---:|---|---|
| 2 — Medical Record | 5 | 5 | Tersedia | Tersedia — tidak berubah |
| 3 — Clinical Document Integrity | 4 | 4 | `Rencana` | **Tersedia** — dikoreksi |
| 4 — Clinical Note Addendum | 4 | 4 | Tersedia | Tersedia — tidak berubah |
| 5 — Clinical Note Author Delegation | 3 | 3 | `Rencana` | **Tersedia** — dikoreksi |
| 6 — Medical Record Access Log | 4 | 4 | `Rencana` | **Tersedia** — dikoreksi |
| 7 — Medical Record Access Purpose | 6 | 0 | `Rencana` | `Rencana` — **tetap benar**, lihat catatan bagian 7 |
| **Total** | **26** | **20** | — | 20 hidup, 6 belum ada |

> **Tabel di atas adalah riwayat revisi `0.1.1` per 27 Agustus 2026 dan sengaja tidak diubah.**
> Keadaan hari ini berbeda: grup 7 dibangun `BE-20` pada 31 Agustus 2026, sehingga **26 dari 26
> endpoint hidup** dan tidak ada lagi grup berstatus `Rencana`.

Seluruh path, method, dan hak akses pada bagian 2 sampai 6 diperiksa satu per satu terhadap
source dan **cocok persis**. Tidak ada endpoint yang berbeda dari yang dijanjikan kontrak, dan
tidak ada endpoint tak terdaftar yang menyelinap masuk ke dalam grup mana pun.

**Satu endpoint hidup di luar kontrak, dan disengaja.** `MedicalRecordBackfillController`
menyediakan `GET api/v1/health-services/medical-record-management/backfill/survey` dan
`POST .../run-batch`, izin `MedicalRecordBackfill : Read` dan `: Update`. Ia alat penelaahan dan
pengisian data lama milik `BE-08`, dipakai sekali oleh operator, **bukan** endpoint pelayanan.
Ia tidak dimasukkan ke dalam kontrak karena frontend tidak boleh memanggilnya — dicatat di sini
supaya keberadaannya tidak terbaca sebagai endpoint liar saat seseorang membandingkan Swagger
dengan dokumen ini.

---

Status keterlaksanaan tiap endpoint tercantum pada kolom **Status** masing-masing tabel.
Endpoint modul Rekam Medis berlabel **Tersedia** sudah dapat dipanggil; yang berlabel
**Rencana** belum dibangun.

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
Contract version: `0.1.1` — status `approved`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{patientId}/summary` | Ringkasan berkas: identitas, alergi aktif, diagnosis aktif, jumlah dokumen per jenis | `MedicalRecord : Read` | Query: `accessPurposeId`, `accessReason` | `ApiResponse<MedicalRecordSummaryResponse>` | **Tersedia** — `BE-14` |
| `GET` | `/{patientId}/timeline` | Riwayat gabungan lintas kunjungan, urut waktu | `MedicalRecord : Read` | Query: `documentKinds`, `encounterId`, `startDate`, `endDate`, `includeCancelled`, `newestFirst`, `accessPurposeId`, `accessReason`, `page`, `pageSize` | `ApiResponse<MedicalRecordTimelineResponse>` | **Tersedia** — `BE-14`. **Bentuk balasan berubah**, lihat catatan di bawah |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}` | Detail satu dokumen beserta addendumnya | `MedicalRecord : Read` | Query: `accessPurposeId`, `accessReason` | `ApiResponse<MedicalRecordDocumentDetailResponse>` | **Tersedia** — `BE-14` |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}/private-note` | Membuka `PrivateNote`, selalu lewat jalur akses beralasan | `MedicalRecord : ReadPrivateNote` | Query: `accessPurposeId` **wajib**, `accessReason` **wajib** | `ApiResponse<MedicalRecordPrivateNoteResponse>` | **Tersedia** — `BE-15`. Izin terpisah; keperluan akses selalu wajib |
| `GET` | `/filters/metadata` | Daftar pilihan penyaring dan keperluan akses | `MedicalRecord : Read` | — | `ApiResponse<MedicalRecordFilterMetadataResponse>` | **Tersedia** — `BE-14`. Tidak menghasilkan jejak akses |

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

### Perubahan kontrak pada `BE-14`: bentuk balasan riwayat

**Rancangan semula:** `ApiResponse<PagedResult<MedicalRecordTimelineItemResponse>>`.
**Yang diterapkan:** `ApiResponse<MedicalRecordTimelineResponse>`.

Alasannya satu, dan tidak dapat dihindari. Riwayat digabungkan dari tiga belas sumber, dan
acceptance criteria `BE-13` nomor 4 mewajibkan: bila satu sumber gagal dibaca, sumber lain tetap
tampil **dan yang gagal ditandai**. Bentuk `PagedResult` tidak punya tempat untuk menyatakan
hal itu. Memaksakannya berarti daftar yang kurang satu jenis dokumen akan terbaca sebagai
daftar lengkap — persis kekeliruan yang paling berbahaya pada berkas rekam medis.

`MedicalRecordTimelineResponse` membungkus halaman yang sama, ditambah empat keterangan:

| Field | Isi |
|---|---|
| `page` | `PagedResult<MedicalRecordTimelineItemResponse>` — persis bentuk semula |
| `access` | Keterangan pembukaan: jenis akses, dan apakah akan ditelaah |
| `requestedKinds` | Jenis dokumen yang benar-benar ditanyakan pada permintaan ini |
| `failedSources` | Sumber yang gagal dibaca beserta alasannya. Kosong berarti lengkap |
| `isTruncated` | Ada sumber yang datanya melampaui batas pengambilan |
| `isComplete` | Ringkasan dua field di atas, untuk dipakai layar |

**Dampak frontend:** pembacaan berubah dari `data.items` menjadi `data.page.items`. Belum ada
kode frontend yang memanggil endpoint ini, sehingga tidak ada pemanggil lama yang rusak.

**Field `access` ada pada seluruh balasan endpoint ini**, termasuk ringkasan dan detail dokumen.
Ini disengaja: pengguna berhak tahu bahwa pembukaannya tercatat, dan bila aksesnya ditandai
untuk ditelaah, ia berhak tahu sekarang — bukan baru saat ditanya unit rekam medis.

**Sudah disahkan.** Kedua delta disetujui pemilik API pada 27 Agustus 2026 bersama kontrak
`0.1.0` (`RM-DEC-028`), dan tidak berubah pada `0.1.1`.

### Catatan `BE-15`: endpoint `private-note`

Endpoint ini selesai pada `BE-15` dan sengaja berbeda dari empat endpoint lain di grup yang sama.

| Perbedaan | Alasan |
|---|---|
| Izin `MedicalRecord : ReadPrivateNote`, bukan `Read` | Seseorang dapat diberi hak membaca seluruh berkas rekam medis tanpa pernah dapat membuka catatan pribadi |
| `accessPurposeId` **selalu** wajib, apa pun keadaan kunjungan | `RM-DEC-022`. Berbeda dari isi rekam medis lain, yang tidak menuntut alasan bila pasien sedang dirawat pengguna |
| Jejak bercakupan `PrivateNote` | Agar pembukaannya dihitung terpisah pada rekap tinjauan |

Hanya CPPT yang memiliki kolom catatan pribadi. Jenis dokumen lain dijawab `404` dengan
keterangan tegas bahwa jenis itu **tidak memiliki** catatan pribadi — bukan dibiarkan seolah-olah
isinya disembunyikan. Permintaan seperti itu **tidak** menghasilkan jejak akses, karena ia
permintaan yang keliru bentuknya, bukan percobaan membuka berkas.

Balasannya membedakan "dokumennya tidak memuat catatan pribadi" (`hasPrivateNote = false`) dari
"catatannya ada". Tanpa pembedaan itu, pembaca tidak punya cara tahu mana yang benar — dan itu
justru mendorongnya mencari lewat jalur lain.

Empat endpoint lain pada grup ini **tetap tidak pernah** mengembalikan isi `PrivateNote`. Yang
mereka bawa hanya penanda `hasPrivateNote` pada detail dokumen.

---

## 3. Health Services / Medical Record Management / Clinical Document Integrity

Base URL: `api/v1/health-services/medical-record-management/clinical-document-integrities`
Contract version: `0.1.1` — status `approved`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-document/{documentKind}/{documentId}` | Status keutuhan satu dokumen | `ClinicalDocumentIntegrity : Read` | — | `ApiResponse<ClinicalDocumentIntegrityResponse>` | **Tersedia** — `BE-04` |
| `POST` | `/by-document/{documentKind}/{documentId}/sign` | Menandatangani dan mengunci dokumen | `ClinicalDocumentIntegrity : Update` | `SignClinicalDocumentRequest` | `ApiResponse<ClinicalDocumentIntegrityResponse>` | **Tersedia** — `BE-04` |
| `GET` | `/my-unsigned` | Daftar catatan milik pengguna yang belum ditandatangani | `ClinicalDocumentIntegrity : Read` | Query: `page`, `pageSize` | `ApiResponse<PagedResult<UnsignedDocumentResponse>>` | **Tersedia** — `BE-04`. Dipakai `FE-03` |
| `GET` | `/by-encounter/{encounterId}` | Daftar keutuhan seluruh dokumen dalam satu kunjungan | `ClinicalDocumentIntegrity : Read` | — | `ApiResponse<List<ClinicalDocumentIntegrityResponse>>` | **Tersedia** — `BE-04` |

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
Contract version: `0.1.1` — status `approved`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-document/{documentKind}/{documentId}` | Daftar addendum sebuah dokumen, urut `Sequence` | `ClinicalNoteAddendum : Read` | — | `ApiResponse<List<ClinicalNoteAddendumResponse>>` | **Tersedia** — `BE-06` |
| `POST` | `/by-document/{documentKind}/{documentId}` | Membuat addendum pada catatan sendiri | `ClinicalNoteAddendum : Create` | `CreateClinicalNoteAddendumRequest` | `ApiResponse<ClinicalNoteAddendumResponse>` | **Tersedia** — `BE-06` |
| `POST` | `/by-document/{documentKind}/{documentId}/as-substitute` | Membuat addendum menggantikan penulis yang berhalangan | `ClinicalNoteAddendum : CreateAsSubstitute` | `CreateClinicalNoteAddendumRequest` | `ApiResponse<ClinicalNoteAddendumResponse>` | **Tersedia** — `BE-06`. **Endpoint tambahan**, lihat catatan di bawah |
| `GET` | `/authority/{documentKind}/{documentId}` | Memeriksa apakah pengguna berhak membuat addendum, dan atas dasar apa | `ClinicalNoteAddendum : Read` | — | `ApiResponse<AddendumAuthorityResponse>` | **Tersedia** — `BE-06` |

**Perubahan kontrak pada `BE-06`: satu endpoint tambahan.** Rancangan semula menyatukan
pembuatan addendum biasa dan addendum pengganti dalam satu endpoint, dengan kewenangan
pengganti diperiksa di dalamnya. Itu ternyata tidak dapat diterapkan: atribut `[AccessAction]`
hanya boleh satu per endpoint, sehingga hak akses `CreateAsSubstitute` tidak akan pernah
terdaftar dan karenanya tidak dapat diberikan kepada siapa pun.

Pemisahan menjadi dua endpoint menyelesaikannya, sekaligus membawa dua keuntungan: tindakan
pengganti terdaftar sebagai hak akses tersendiri sehingga dapat diberikan kepada kepala unit
dan DPJP tanpa ikut memberi hak lain, dan pemakaiannya tercatat terpisah pada log sehingga
dapat ditinjau tersendiri.

Jumlah endpoint pada grup ini menjadi **4**, bukan 3.

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
Contract version: `0.1.1` — status `approved`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar penetapan berhalangan | `ClinicalNoteAuthorDelegation : Read` | Query: `originalAuthorUserId`, `isActive`, `page`, `pageSize` | `ApiResponse<PagedResult<AuthorDelegationResponse>>` | **Tersedia** — `BE-05` |
| `POST` | `/` | Menetapkan seorang penulis berhalangan | `ClinicalNoteAuthorDelegation : Create` | `CreateAuthorDelegationRequest` | `ApiResponse<AuthorDelegationResponse>` | **Tersedia** — `BE-05` |
| `PATCH` | `/{id}/revoke` | Mencabut penetapan lebih awal | `ClinicalNoteAuthorDelegation : Update` | `RevokeAuthorDelegationRequest` | `ApiResponse<AuthorDelegationResponse>` | **Tersedia** — `BE-05` |

**Ketiga endpoint ini hidup, tetapi belum punya layar.** Layar penetapan penulis berhalangan
ditunda dari rilis pertama oleh pemilik frontend pada 27 Agustus 2026
(`03-frontend-architecture.md` bagian 7 dan 10.6). Sampai layarnya dibuat, `UnitHeadGrant` hanya
dapat dibuat lewat pemanggilan API langsung. Ini disebut terbuka supaya tidak ada yang mengira
kemampuannya belum ada — ia ada, hanya belum dapat dicapai dari antarmuka.

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
Contract version: `0.1.1` — status `approved`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar jejak akses | `MedicalRecordAccessLog : Read` | Query: `patientId`, `userId`, `accessType`, `isFlaggedForReview`, `startDate`, `endDate`, `page`, `pageSize` | `ApiResponse<PagedResult<MedicalRecordAccessLogResponse>>` | **Tersedia** — `BE-12` |
| `GET` | `/pending-review` | Antrean akses yang belum ditinjau | `MedicalRecordAccessLog : Read` | Query: `accessType`, `startDate`, `endDate`, `page`, `pageSize` | `ApiResponse<PagedResult<MedicalRecordAccessLogResponse>>` | **Tersedia** — `BE-12`. Dipakai `FE-05`; tiga penyaring pertama ditambahkan 31 Agustus 2026, lihat catatan di bawah |
| `PATCH` | `/{id}/mark-reviewed` | Menandai satu akses sudah ditinjau | `MedicalRecordAccessLog : Update` | `MarkAccessReviewedRequest` | `ApiResponse<MedicalRecordAccessLogResponse>` | **Tersedia** — `BE-12` |
| `GET` | `/summary` | Rekap jumlah akses per jenis dan per periode | `MedicalRecordAccessLog : Read` | Query: `startDate`, `endDate` | `ApiResponse<MedicalRecordAccessSummaryResponse>` | **Tersedia** — `BE-12`. Belum dipakai layar mana pun pada rilis pertama |

**Penambahan penyaring pada `/pending-review` — 31 Agustus 2026.** Endpoint antrean kini
menerima `accessType`, `startDate`, dan `endDate`: penyaring yang sama persis dengan daftar
seluruh jejak. Perubahannya **aditif** — ketiganya boleh dikosongkan, dan permintaan tanpa
ketiganya berperilaku persis seperti sebelumnya, sehingga klien yang ditulis lebih dulu tidak
perlu disesuaikan sedikit pun.

Yang **tidak** ikut dibuka, dan tidak boleh dibuka: syarat antreannya sendiri.
`isFlaggedForReview` dan syarat "belum ditinjau" tetap dipatok controller, bukan dikirim
pemanggil. Penyaring layar hanya dapat mempersempit antrean, tidak pernah melebarkannya —
begitu syarat itu dapat dipilih lewat kueri, "perlu ditinjau" berhenti berarti apa pun.

Alasannya datang dari layar. `FE-05` menampilkan penyaring pada tab seluruh jejak tetapi tidak
pada tab antrean, sehingga bilah penyaring tab antrean hanya berisi tombol muat ulang dan dua
tab yang sama tampak menuntut dua tata cara berbeda. Kenaikan nomor versi kontrak beserta
pengesahannya menunggu keputusan pemilik API.

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
Contract version: `0.1.1` — status `approved`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar keperluan akses | `MedicalRecordAccessPurpose : Read` | Query: `search`, `isActive`, `page`, `pageSize` | `ApiResponse<PagedResult<MedicalRecordAccessPurposeResponse>>` | **Tersedia** — `BE-20` |
| `GET` | `/options` | Pilihan untuk kotak isian alasan | `MedicalRecordAccessPurpose : Read` | — | `ApiResponse<List<OptionResponse>>` | **Tersedia** — `BE-20` |
| `GET` | `/{id}` | Detail satu keperluan | `MedicalRecordAccessPurpose : Read` | — | `ApiResponse<MedicalRecordAccessPurposeResponse>` | **Tersedia** — `BE-20` |
| `POST` | `/` | Menambah keperluan | `MedicalRecordAccessPurpose : Create` | `CreateMedicalRecordAccessPurposeRequest` | `ApiResponse<MedicalRecordAccessPurposeResponse>` | **Tersedia** — `BE-20` |
| `PUT` | `/{id}` | Mengubah keperluan | `MedicalRecordAccessPurpose : Update` | `UpdateMedicalRecordAccessPurposeRequest` | `ApiResponse<MedicalRecordAccessPurposeResponse>` | **Tersedia** — `BE-20` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan | `MedicalRecordAccessPurpose : Update` | `StatusRequest` | `ApiResponse<object>` | **Tersedia** — `BE-20` |

> **DIKOREKSI 31 Agustus 2026.** Grup ini **sudah ada** sejak `BE-20`. Catatan di bawah ditulis
> saat keenam endpoint belum dibangun dan dipertahankan sebagai riwayat, tetapi kalimat
> "belum ada" di dalamnya tidak lagi berlaku. Yang masih berlaku: **isi masternya** memang
> masih menunggu SOP rekam medis rumah sakit, dan sekarang dapat diisi lewat layar `FE-06`
> tanpa meminta perubahan kode. Dua penahan itu berbeda dan tidak boleh tertukar.
>
> Delta terhadap rancangan semula: `/options` mengembalikan
> `List<MedicalRecordAccessPurposeOptionResponse>`, bentuk yang sudah dipakai
> `/medical-records/filters/metadata`, bukan tipe `OptionResponse` yang disebut tabel di atas —
> tipe itu tidak ada di codebase. Penyaring halaman menerima `pageNumber` maupun `page`.

### Satu-satunya grup yang benar-benar belum ada

Ditegaskan pada `0.1.1` karena mudah disalahbaca sebagai "menunggu SOP". Yang ada dan yang tidak:

| Bagian | Keadaan |
|---|---|
| `MstMedicalRecordAccessPurpose.cs` | **Ada** — `Areas/HealthServices/MasterData/Models/` |
| Configuration dan migration | **Ada** — `MstMedicalRecordAccessPurposeConfiguration.cs`, migration `AddMedicalRecordAccessAuditTables` |
| DTO, controller, keenam endpoint di atas | **Tidak ada.** Tidak ada `MedicalRecordAccessPurposeController` di seluruh source backend |
| Isi masternya | **Kosong** — menunggu SOP rekam medis rumah sakit |

Dua penahan yang berbeda menumpuk di satu tempat, dan pemisahannya penting: **isi** menunggu SOP,
**endpoint** menunggu seseorang menulisnya. Yang kedua dapat dikerjakan sekarang juga tanpa
menunggu yang pertama. Bacaan "`BE-09` tinggal menunggu SOP" membuat pekerjaan kode yang terlewat
ini tidak terlihat.

Akibatnya berantai: selama master kosong, **pembukaan berkas pasien di luar rawatan selalu
ditolak** — dan tanpa controller-nya, tidak ada cara mengisi lewat antarmuka. `FE-06` tertahan
karenanya.

**Sumber daftar keperluan bagi frontend, sementara ini.** `FE-02` **tidak** memakai `/options`.
Daftar keperluan sudah tersedia pada `GET /medical-records/filters/metadata` → `accessPurposes`,
bersama penanda `isAccessPurposeMasterEmpty` (bagian 2). Endpoint itu juga tidak menghasilkan
jejak akses, sehingga aman dipanggil sebelum penghalang keperluan dijawab. `/options` tetap
direncanakan untuk layar master, bukan untuk kotak keperluan.

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
