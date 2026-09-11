# State Transition Matrix — Modul Radiologi

| Field | Value |
|---|---|
| Contract version | `RAD-STATE-001` |
| Revision | `2` |
| Status | `approved` |
| Backend SHA | `0e2eb105` |
| Input | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003`, `RAD-DEC-005`, `RAD-DEC-011`, `RAD-DA-001-r1` |

Transisi yang **tidak sah** ikut dituliskan, bukan hanya yang sah. Tanpa itu, implementer tidak
tahu apa yang harus ditolak.

---

## 1. Pesanan Radiologi — `RadOrderStatus`

Status: `Sudah ada`, tidak berubah.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat pesanan | `Requested` | Dokter pengirim | Kunjungan, prosedur, dan alat terisi | `400` isian tidak lengkap |
| `Requested` | Terima | `Accepted` | Petugas Radiologi | — | `409` status tidak sesuai |
| `Requested` | Tolak | `Rejected` | Petugas Radiologi | Alasan wajib | `400` alasan kosong |
| `Accepted` | Jadwalkan | `Scheduled` | Petugas Radiologi | — | `409` |
| `Scheduled` | Mulai | `InProgress` | Petugas Radiologi | — | `409` |
| `InProgress` | Selesaikan | `Completed` | Petugas Radiologi | — | `409` |
| `Requested`, `Accepted`, `Scheduled`, `InProgress` | Tahan | `OnHold` | Petugas Radiologi | Status sebelumnya disimpan | `409` |
| `OnHold` | Lanjutkan | Status sebelum ditahan | Petugas Radiologi | — | `409` |
| `Draft`, `Requested`, `Accepted`, `Scheduled`, `OnHold`, `CancelRequested` | Batalkan | `Cancelled` | Sesuai wewenang | Alasan wajib | `400` alasan kosong |
| Setelah diserahkan ke Radiologi | Minta pembatalan | `CancelRequested` | Dokter pengirim | — | `409` |

**Status terminal:** `Completed`, `Cancelled`, `Rejected`.

**Status `Draft` tidak pernah dihasilkan** (`RAD-DEC-011`). Nilainya dipertahankan agar angka
status lain tidak bergeser.

### Transisi yang tidak sah

| Percobaan | Mengapa ditolak | Kode |
|---|---|---|
| `Completed` ke status mana pun | Status terminal | `409` |
| `Rejected` ke `Accepted` | Penolakan tidak dibatalkan; buat pesanan baru | `409` |
| Dokter membatalkan langsung setelah `Requested` | Wewenang berpindah ke Radiologi; harus lewat permintaan | `403` |
| `Requested` langsung ke `Completed` | Melompati penerimaan dan pengerjaan | `409` |

---

## 2. Study Radiologi — `RadStudyStatus`

Status: `Sudah ada`, tidak berubah.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Rencanakan | `Planned` | Petugas Radiologi | Pesanan sudah `Accepted` | `409` |
| `Planned` | Verifikasi pasien | `PatientVerified` | Radiografer | Identitas cocok dengan pesanan | `409` |
| `PatientVerified` | Nyatakan lolos keselamatan | `SafetyCleared` | Radiografer | **Ada aturan `Active`** dan seluruh butir wajib `Passed` atau `NotApplicable` | `409` beserta daftar butir yang menahan |
| `SafetyCleared` | Mulai acquisition | `AcquisitionStarted` | Radiografer | — | `409` |
| `AcquisitionStarted` | Selesaikan acquisition | `Acquired` | Radiografer | — | `409` |
| `AcquisitionStarted` | Hentikan di tengah jalan | `Aborted` | Radiografer | Sebab dan alasan wajib | `400` |
| `Acquired` | Nilai mutu — layak | `QualityAccepted` | Radiografer atau radiolog | — | `409` |
| `Acquired` | Nilai mutu — tidak layak | `QualityRejected` | Radiografer atau radiolog | — | `409` |
| `QualityRejected` | Tandai perlu diulang | `RepeatRequired` | Petugas Radiologi | — | `409` |
| `RepeatRequired` | Buat study pengulangan | Study **baru** `Planned` | Petugas Radiologi | Sebab pengulangan wajib. Study lama **tetap** | `400` |
| Beberapa status | Tahan | `OnHold` | Radiografer | Status sebelumnya disimpan | `409` |
| `OnHold` | Lanjutkan | Status sebelum ditahan | Radiografer | — | `409` |
| Sebelum `AcquisitionStarted` | Batalkan | `Cancelled` | Petugas Radiologi | Alasan wajib | `400` |

**Status terminal:** `QualityAccepted`, `Aborted`, `Cancelled`.

### Transisi yang tidak sah

| Percobaan | Mengapa ditolak | Kode |
|---|---|---|
| `Planned` langsung ke `AcquisitionStarted` | Identitas dan keselamatan belum diperiksa | `409` |
| `PatientVerified` langsung ke `AcquisitionStarted` | Gerbang keselamatan dilewati | `409` |
| `SafetyCleared` padahal tidak ada aturan `Active` | Fail-closed: ketiadaan aturan bukan berarti aman | `409` |
| `SafetyCleared` padahal ada butir wajib `Pending` atau `Failed` | Butir wajib belum tuntas | `409` |
| Menilai mutu study yang belum `Acquired` | Belum ada citra untuk dinilai | `409` |
| Mengubah study yang sudah `QualityAccepted` | Fakta kelayakan tagih sudah terbit | `409` |
| Menghapus study yang diulang | Pengulangan tidak pernah menimpa asalnya | `403` |

---

## 3. Hasil Bacaan — `RadReportStatus`

Status: **`Baru`**. Diturunkan dari `RJ-BIL-GATE-DEC-004` dan `RAD-DEC-003`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | (otomatis) | `Pending` | Sistem | Study berpindah ke `QualityAccepted` | — |
| `Pending` | Tulis draf | `Drafted` | Radiolog, residen, radiografer, bantuan AI | Kesimpulan wajib diisi. Peran penulis dibekukan | `400` kesimpulan kosong |
| `Drafted` | Ubah draf | `Drafted` | **Penulis draf itu sendiri** | Belum disahkan | `403` bila bukan penulisnya |
| `Drafted` | Sahkan | `Validated` | **Dokter radiolog** | Bila `AuthorRoleSnapshot` bukan `Radiologist`, pengesah **wajib berbeda** dari penulis | `403` |
| `Validated` | Rilis | `Released` | Dokter radiolog | — | `409` |
| `Released` | Tulis draf koreksi | `AmendmentDrafted` | Radiolog, residen, radiografer, bantuan AI | **Alasan koreksi wajib.** Versi lama **tetap `Released` dan tetap berlaku** sampai koreksinya dirilis — lihat catatan di bawah tabel | `400` alasan kosong |
| `AmendmentDrafted` | Sahkan koreksi | `AmendmentValidated` | **Dokter radiolog** | Aturan pengesahan sama seperti draf pertama | `403` |
| `AmendmentValidated` | Rilis koreksi | `AmendmentReleased` | Dokter radiolog | Menjadi versi berlaku | `409` |
| `AmendmentReleased` | Tulis draf koreksi lagi | `AmendmentDrafted` | Sama | Alasan wajib | `400` |

**Tidak ada status terminal.** Bacaan yang sudah dirilis selalu dapat diamandemen.

> **Perbaikan 2026-09-11, disetujui pemilik modul.** Versi terdahulu baris "Tulis draf koreksi"
> berbunyi *"Versi lama menjadi `Superseded`"*, seolah perpindahan itu terjadi saat draf koreksi
> ditulis. **Itu keliru**, dan bertentangan dengan bagian 4 dokumen ini serta contoh
> `FR-RAD-020`.
>
> **Yang benar: versi lama berpindah menjadi `Superseded` ketika koreksinya DIRILIS**, bukan
> ketika drafnya ditulis.
>
> **Mengapa ini menentukan.** Antara "koreksi mulai ditulis" dan "koreksi dirilis" bisa ada jeda
> berjam-jam. Selama jeda itu versi lama adalah **satu-satunya bacaan yang sah** — ia sudah
> diperiksa dokter radiolog dan sudah dirilis; draf koreksi belum diperiksa siapa pun.
>
> Kalau versi lama dipensiunkan begitu draf koreksi ditulis, dokter jaga yang membuka hasil pada
> pukul 10 malam akan melihat salah satu dari dua hal: draf yang belum disahkan, atau tidak ada
> bacaan berlaku sama sekali. Keduanya lebih buruk daripada membaca versi lama yang memang masih
> berlaku.
>
> Penerapannya sudah mengikuti bacaan yang benar sejak `BE-RAD-10`; lihat
> `task/report/backend/BE-RAD-10.md` bagian 2.3.

### Transisi yang tidak sah

| Percobaan | Mengapa ditolak | Kode |
|---|---|---|
| Penulis bukan-radiolog mengesahkan drafnya sendiri | `RAD-DEC-003` | `403` |
| Mengubah isi versi berstatus `Released` atau `Superseded` | Koreksi wajib lewat versi baru | `403` |
| Menghapus versi mana pun | Riwayat klinis tidak boleh hilang | `403` |
| Membuat draf atas study yang `IsUsable` bernilai `false` | Tidak ada citra yang sah untuk dibaca | `422` |
| Membuat draf atas study yang `IsUsable` masih `null` | Mutu citra belum dinilai | `422` |
| Membuat bacaan kedua atas study yang sama | Satu study paling banyak satu bacaan | `409` |
| Merilis bacaan yang belum disahkan | Melompati pengesahan | `409` |

### Contoh penerapan aturan pengesahan

| Penulis draf | `AuthorRoleSnapshot` | Pengesah | Hasil |
|---|---|---|---|
| dr. Sinta, Sp.Rad | `Radiologist` | dr. Sinta sendiri | **Diterima** |
| dr. Sinta, Sp.Rad | `Radiologist` | dr. Bagas, Sp.Rad | **Diterima** |
| dr. Rian, residen | `Resident` | dr. Rian sendiri | **Ditolak `403`** |
| dr. Rian, residen | `Resident` | dr. Sinta, Sp.Rad | **Diterima** |
| Radiografer Tono | `Radiographer` | Radiografer Tono | **Ditolak `403`** |
| Bantuan AI | `AiAssisted` | dr. Sinta, Sp.Rad | **Diterima** |
| Bantuan AI | `AiAssisted` | Bantuan AI | **Ditolak `403`** — pengesah wajib manusia |

---

## 4. Versi Hasil Bacaan — `RadReportVersionStatus`

Status: **`Baru`**.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Draf dibuat | `Drafted` | Penulis draf | — |
| `Drafted` | Disahkan | `Validated` | Dokter radiolog | Aturan pengesahan terpenuhi |
| `Validated` | Dirilis | `Released` | Dokter radiolog | **Setelah ini isinya beku** |
| `Released` | Digantikan versi baru | `Superseded` | Sistem, otomatis | Versi berikutnya dirilis |

**Yang tidak sah:** mengembalikan `Superseded` menjadi `Released`. Koreksi atas koreksi
membuat versi baru lagi, bukan menghidupkan versi lama.

---

## 5. Aturan Keselamatan — `RadSafetyRuleStatus`

Status: **`Baru`** pada tabel yang `Diperbarui`. Diturunkan dari `RAD-DEC-005`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Susun draf | `Draft` | Admin Radiologi | Alat dan butir keselamatan sudah ada | `400` |
| `Draft` | Ubah | `Draft` | Admin Radiologi | — | — |
| `Draft` | Ajukan pengesahan | `PendingApproval` | Admin Radiologi | Isi lengkap | `400` |
| `PendingApproval` | Sahkan | `Active`, `RuleVersion` naik satu | **Penanggung jawab klinis** | Tidak ada aturan `Active` lain untuk kombinasi alat, pemeriksaan, dan butir yang sama | `409` bertabrakan |
| `PendingApproval` | Tolak | `Draft` | Penanggung jawab klinis | **Alasan wajib** | `400` alasan kosong |
| `Active` | Nonaktifkan | `Inactive` | Penanggung jawab klinis | — | `403` bila bukan penanggung jawab klinis |
| `Inactive` | Susun ulang | `Draft` | Admin Radiologi | Membuat baris baru, bukan menghidupkan yang lama | — |

### Yang ikut dinilai gerbang keselamatan

| `RuleStatus` | Dinilai? |
|---|:---:|
| `Draft` | **Tidak** |
| `PendingApproval` | **Tidak** |
| `Active` | **Ya** |
| `Inactive` | **Tidak** |

### Transisi yang tidak sah

| Percobaan | Mengapa ditolak | Kode |
|---|---|---|
| Admin Radiologi mengesahkan aturannya sendiri | Pengesahan milik penanggung jawab klinis | `403` |
| `Draft` langsung ke `Active` | Melompati pengesahan | `409` |
| Mengubah aturan berstatus `Active` | Perubahan wajib lewat draf baru dan pengesahan ulang | `403` |
| Mengesahkan aturan kedua untuk kombinasi yang sama | Dua aturan bertentangan tidak boleh sama-sama aktif | `409` |

> **Mengapa aturan `Active` tidak boleh diubah langsung.** Study yang sudah lolos membekukan
> `RuleVersion` yang berlaku saat itu. Kalau isi aturan berubah tanpa menaikkan versi,
> penilaian lama akan terbaca memakai aturan yang sebenarnya belum ada saat itu.

---

## 6. Traceability

| Lifecycle | Decision asal | Slice |
|---|---|---|
| Pesanan | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-011` | `S1` |
| Study | `RJ-BIL-GATE-DEC-004` | `S2`, `S3`, `S6` |
| Hasil bacaan | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003` | `S9` |
| Versi hasil bacaan | `RJ-BIL-GATE-DEC-004` | `S10` |
| Aturan keselamatan | `RAD-DEC-005`, `RJ-BIL-DEC-014` | `S4` |
