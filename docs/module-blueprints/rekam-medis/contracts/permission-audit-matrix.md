# Permission dan Audit Matrix — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Contract version | `0.1.0` |
| Status | `draft` |
| Owner | Security/privacy authority: `OPEN` |
| `approved_by` / `approved_at` | — / — |
| Input revisions | `00-interview-decisions.md` revision `2` |
| Compatibility impact | **Aditif** untuk izin baru. `RM-DEC-017` mengubah perilaku `SuperAdmin`, tetapi **hanya di dalam modul ini** — `AccessPermissionService` tidak disentuh |

> **PERINGATAN DASAR DESAIN.** Disusun di atas keputusan berstatus `draft`. Lihat `RM-DEC-025`.

---

## 1. Dua lapis kewenangan yang berbeda

Modul ini adalah yang pertama di sistem yang memakai **dua lapis** kewenangan. Membedakannya
penting agar implementer tidak mencari kewenangan pasien di tempat yang salah.

| Lapis | Menjawab pertanyaan | Ditegakkan di mana | Sudah ada? |
|---|---|---|:---:|
| Kewenangan fungsi | Boleh membuka menu rekam medis? | `AccessPermissionFilter` lewat `[AccessPermission]` | Ya |
| **Kewenangan pasien** | Boleh membuka rekam medis **pasien ini**? | `MedicalRecordAccessAuditService` | **Tidak, baru** |

Lapis kedua tidak dapat dititipkan ke lapis pertama, karena `SysAccessPolicy` hanya mengenal
departemen, jabatan, controller, dan action. Tidak ada dimensi pasien di dalamnya
(`RM-CAP-024`).

---

## 2. Matriks izin per endpoint

Kolom "String yang dipakai" ditulis apa adanya supaya implementer menyalin, bukan menerjemahkan.

### Health Services / Medical Record Management / Medical Record

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger | Jejak akses |
|---|---|---|---|:---:|:---:|
| `GET /{patientId}/summary` | `MedicalRecord` | `Read` | `[AccessPermission("MedicalRecord", "Read")]` | Tidak | **Ya** |
| `GET /{patientId}/timeline` | `MedicalRecord` | `Read` | `[AccessPermission("MedicalRecord", "Read")]` | Tidak | **Ya** |
| `GET /{patientId}/documents/{kind}/{id}` | `MedicalRecord` | `Read` | `[AccessPermission("MedicalRecord", "Read")]` | Tidak | **Ya** |
| `GET /{patientId}/documents/{kind}/{id}/private-note` | `MedicalRecord` | `ReadPrivateNote` | `[AccessPermission("MedicalRecord", "ReadPrivateNote")]` | Tidak | **Ya** |
| `GET /filters/metadata` | `MedicalRecord` | `Read` | `[AccessPermission("MedicalRecord", "Read")]` | Tidak | Tidak |

### Health Services / Medical Record Management / Clinical Document Integrity

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger | Jejak akses |
|---|---|---|---|:---:|:---:|
| `GET /by-document/{kind}/{id}` | `ClinicalDocumentIntegrity` | `Read` | `[AccessPermission("ClinicalDocumentIntegrity", "Read")]` | Tidak | Tidak |
| `POST /by-document/{kind}/{id}/sign` | `ClinicalDocumentIntegrity` | `Update` | `[AccessPermission("ClinicalDocumentIntegrity", "Update")]` | **Ya** | Tidak |
| `GET /my-unsigned` | `ClinicalDocumentIntegrity` | `Read` | `[AccessPermission("ClinicalDocumentIntegrity", "Read")]` | Tidak | Tidak |
| `GET /by-encounter/{encounterId}` | `ClinicalDocumentIntegrity` | `Read` | `[AccessPermission("ClinicalDocumentIntegrity", "Read")]` | Tidak | Tidak |

### Health Services / Medical Record Management / Clinical Note Addendum

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger | Jejak akses |
|---|---|---|---|:---:|:---:|
| `GET /by-document/{kind}/{id}` | `ClinicalNoteAddendum` | `Read` | `[AccessPermission("ClinicalNoteAddendum", "Read")]` | Tidak | Tidak |
| `POST /by-document/{kind}/{id}` | `ClinicalNoteAddendum` | `Create` | `[AccessPermission("ClinicalNoteAddendum", "Create")]` | **Ya** | Tidak |
| `GET /authority/{kind}/{id}` | `ClinicalNoteAddendum` | `Read` | `[AccessPermission("ClinicalNoteAddendum", "Read")]` | Tidak | Tidak |

### Health Services / Medical Record Management / Clinical Note Author Delegation

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger | Jejak akses |
|---|---|---|---|:---:|:---:|
| `GET /` | `ClinicalNoteAuthorDelegation` | `Read` | `[AccessPermission("ClinicalNoteAuthorDelegation", "Read")]` | Tidak | Tidak |
| `POST /` | `ClinicalNoteAuthorDelegation` | `Create` | `[AccessPermission("ClinicalNoteAuthorDelegation", "Create")]` | **Ya** | Tidak |
| `PATCH /{id}/revoke` | `ClinicalNoteAuthorDelegation` | `Update` | `[AccessPermission("ClinicalNoteAuthorDelegation", "Update")]` | **Ya** | Tidak |

### Health Services / Medical Record Management / Medical Record Access Log

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger | Jejak akses |
|---|---|---|---|:---:|:---:|
| `GET /` | `MedicalRecordAccessLog` | `Read` | `[AccessPermission("MedicalRecordAccessLog", "Read")]` | Tidak | Tidak |
| `GET /pending-review` | `MedicalRecordAccessLog` | `Read` | `[AccessPermission("MedicalRecordAccessLog", "Read")]` | Tidak | Tidak |
| `PATCH /{id}/mark-reviewed` | `MedicalRecordAccessLog` | `Update` | `[AccessPermission("MedicalRecordAccessLog", "Update")]` | **Ya** | Tidak |
| `GET /summary` | `MedicalRecordAccessLog` | `Read` | `[AccessPermission("MedicalRecordAccessLog", "Read")]` | Tidak | Tidak |

### Health Services / Master Data / Medical Record Access Purpose

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger | Jejak akses |
|---|---|---|---|:---:|:---:|
| `GET /` | `MedicalRecordAccessPurpose` | `Read` | `[AccessPermission("MedicalRecordAccessPurpose", "Read")]` | Tidak | Tidak |
| `GET /options` | `MedicalRecordAccessPurpose` | `Read` | `[AccessPermission("MedicalRecordAccessPurpose", "Read")]` | Tidak | Tidak |
| `GET /{id}` | `MedicalRecordAccessPurpose` | `Read` | `[AccessPermission("MedicalRecordAccessPurpose", "Read")]` | Tidak | Tidak |
| `POST /` | `MedicalRecordAccessPurpose` | `Create` | `[AccessPermission("MedicalRecordAccessPurpose", "Create")]` | **Ya** | Tidak |
| `PUT /{id}` | `MedicalRecordAccessPurpose` | `Update` | `[AccessPermission("MedicalRecordAccessPurpose", "Update")]` | **Ya** | Tidak |
| `PATCH /{id}/status` | `MedicalRecordAccessPurpose` | `Update` | `[AccessPermission("MedicalRecordAccessPurpose", "Update")]` | **Ya** | Tidak |

---

## 3. Mengapa dua kolom terakhir berbeda

Tabel di atas punya dua kolom yang mudah tertukar, dan perbedaannya menentukan kepatuhan.

| Kolom | Menulis ke mana | Mengikuti konvensi project? |
|---|---|---|
| Dicatat logger | `LoggerService`, keluar ke log terpusat | **Ya.** GET tidak dicatat; Create, Update, dan perubahan status dicatat |
| Jejak akses | Tabel `TrxMedicalRecordAccessLog` | Tidak berlaku. Ini mekanisme baru milik modul ini |

Sekilas tampak modul ini melanggar konvensi karena mencatat GET. Sesungguhnya tidak: GET tetap
tidak masuk `LoggerService`. Yang mencatat GET adalah **tabel jejak akses**, yang merupakan
mekanisme berbeda dengan tujuan berbeda. Membedakan keduanya penting agar peninjau kode tidak
menganggap konvensi sedang dilanggar.

---

## 4. Aturan isi log

| Aturan | Alasan |
|---|---|
| Payload `LoggerService` hanya memuat `EntityId`, controller, action, dan status | Konvensi project |
| `AddendumText` dan `CorrectionReason` **tidak boleh** masuk `LoggerService` | Bertanda sensitif pada kamus data. Keduanya berisi data klinis |
| `AccessReason` **tidak boleh** masuk `LoggerService` | Bertanda sensitif. Alasan akses dapat mengungkap keadaan pasien, misalnya "konsultasi kejiwaan" |
| `PrivateNote` **tidak boleh** masuk log mana pun | Kolom paling sensitif di modul ini |
| `CancelledReason` **tidak boleh** masuk `LoggerService` | Bertanda sensitif |
| Tabel jejak akses **tidak boleh** memuat isi klinis | Bila memuat, ia menjadi salinan rekam medis kedua yang justru memperluas permukaan kebocoran |

---

## 5. Perlakuan `SuperAdmin`

Ini penerapan `RM-DEC-017`, dan cara penerapannya sengaja dibatasi.

| Aspek | Ketetapan |
|---|---|
| Kewenangan fungsi | **Tidak berubah.** `SuperAdmin` tetap melewati `AccessPermissionFilter` seperti sekarang |
| `AccessPermissionService.cs:54-56` | **Tidak disentuh sama sekali** |
| Kewenangan pasien | `SuperAdmin` **tunduk** aturan yang sama seperti pengguna lain: tercatat pada jejak akses, dan wajib mengisi alasan bila pasien tidak punya kunjungan aktif |
| Tempat penegakan | Di dalam `MedicalRecordAccessAuditService`, bukan di lapisan kewenangan bersama |

Alasan pembatasan ini penting. `RM-DEC-017` adalah keputusan yang **berdampak ke seluruh
aplikasi**, sementara security/privacy owner belum ditunjuk. Menerapkannya di
`AccessPermissionService` akan mengubah perilaku IGD, farmasi, dan seluruh modul lain sekaligus
— tanpa persetujuan siapa pun. Menerapkannya di dalam service modul membatasi pengaruhnya pada
rekam medis saja, sehingga keputusan yang lebih luas tetap terbuka bagi owner ketika ia
ditunjuk.

Konsekuensi yang harus diketahui: `SuperAdmin` masih dapat membaca data klinis lewat endpoint
`ClinicalManagement` yang sudah ada, **tanpa jejak akses dan tanpa alasan**. Modul rekam medis
tidak menutup jalur itu. Ini keterbatasan nyata, bukan celah yang terlewat, dan wajib
disampaikan kepada security/privacy owner saat ia menilai `RM-DEC-017`.

---

## 6. Saran pembagian hak akses

Bagian ini **saran**, bukan ketetapan. Pemetaan peran organisasi ke izin adalah kewenangan
security/privacy owner yang belum ditunjuk.

| Peran | `MedicalRecord : Read` | `MedicalRecord : ReadPrivateNote` | `ClinicalDocumentIntegrity : Update` | `ClinicalNoteAddendum : Create` | `MedicalRecordAccessLog : Read` | `ClinicalNoteAuthorDelegation : Create` |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| Dokter | Ya | Ya | Ya | Ya | Tidak | Tidak |
| Perawat | Ya | Tidak | Ya | Ya | Tidak | Tidak |
| Kepala unit | Ya | Ya | Ya | Ya | Tidak | **Ya** |
| Petugas rekam medis | Ya | Tidak | Tidak | Tidak | **Ya** | Tidak |
| Koder | Ya | Tidak | Tidak | Tidak | Tidak | Tidak |
| Auditor internal | Tidak | Tidak | Tidak | Tidak | **Ya** | Tidak |

Dua baris yang patut diperhatikan:

**Petugas rekam medis tidak diberi `ReadPrivateNote`.** Ia mengelola berkas, bukan membaca
catatan pribadi klinisi. Bila suatu saat perlu, jalur akses beralasan tetap terbuka setelah
izinnya diberikan secara khusus.

**Auditor internal tidak diberi `MedicalRecord : Read`.** Tugasnya memeriksa siapa membuka apa,
bukan membaca isi rekam medis. Memberi auditor hak baca penuh justru memperluas permukaan
kebocoran yang seharusnya ia awasi.

---

## 7. Pendaftaran izin

Seluruh izin baru terdaftar otomatis. Tidak ada langkah manual.

| Langkah | Keterangan |
|---|---|
| Atribut pada controller | `[AccessController(moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD", moduleName: "Health Service Medical Record", ...)]` |
| Atribut pada endpoint | `[AccessAction(...)]` dan `[AccessPermission("Resource", "Action")]` |
| Pendaftaran | `Seeders/AccessMenuSeeder.cs` membacanya saat aplikasi mulai, dipanggil dari `Program.cs:788` |
| Hasilnya | Controller dan action baru muncul sendiri di layar pengaturan hak akses |

Pemetaan peran organisasi ke izin tetap dilakukan manusia lewat layar pengaturan hak akses.
Sistem hanya menyediakan daftarnya.

---

## 8. Traceability

| Aspek | Decision | Capability | Acceptance test |
|---|---|---|---|
| Kewenangan pasien | `RM-DEC-005`, `RM-DEC-016` | `RM-CAP-024` | `AT-RM-06`, `AT-RM-07` |
| Jejak akses tercatat | `RM-DEC-015` | `RM-CAP-022` | `AT-RM-12` |
| `SuperAdmin` tunduk aturan | `RM-DEC-017` | `RM-CAP-025` | `AT-RM-13` |
| `PrivateNote` selalu beralasan | `RM-DEC-022` | `RM-CAP-027` | `AT-RM-16` |
| Kolom sensitif tidak masuk log | Aturan output dokumentasi | — | `AT-RM-23` |
