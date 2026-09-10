# Permission dan Audit Matrix — Modul Radiologi

| Field | Value |
|---|---|
| Contract version | `RAD-PERM-001` |
| Revision | `2` |
| Status | `approved` |
| Backend SHA | `0e2eb105` |
| Input | `RAD-ARCH-BE-001`, `RAD-API-001`, `RAD-DEC-003`, `RAD-DEC-005` |

String `[AccessPermission(...)]` ditulis **apa adanya** supaya implementer menyalin, bukan
menerjemahkan.

Konvensi project: **`GET` tidak dicatat logger.** Create, Update, perubahan status, dan Delete
dicatat.

---

## 1. Rad Order — sudah berjalan

Base URL: `api/v1/health-services/radiology-management/rad-orders`

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /` | `RadOrder` | `Read` | `[AccessPermission("RadOrder", "Read")]` | Tidak |
| `GET /{id}` | `RadOrder` | `Read` | `[AccessPermission("RadOrder", "Read")]` | Tidak |
| `GET /episodes/{episodeId}` | `RadOrder` | `Read` | `[AccessPermission("RadOrder", "Read")]` | Tidak |
| `POST /` | `RadOrder` | `Create` | `[AccessPermission("RadOrder", "Create")]` | Ya |
| `PUT /{id}/accept` | `RadOrder` | `Process` | `[AccessPermission("RadOrder", "Process")]` | Ya |
| `PUT /{id}/schedule` | `RadOrder` | `Schedule` | `[AccessPermission("RadOrder", "Schedule")]` | Ya |
| `PUT /{id}/start` | `RadOrder` | `Process` | `[AccessPermission("RadOrder", "Process")]` | Ya |
| `PUT /{id}/complete` | `RadOrder` | `Process` | `[AccessPermission("RadOrder", "Process")]` | Ya |
| `PUT /{id}/hold` | `RadOrder` | `Hold` | `[AccessPermission("RadOrder", "Hold")]` | Ya |
| `PUT /{id}/resume` | `RadOrder` | `Hold` | `[AccessPermission("RadOrder", "Hold")]` | Ya |
| `PUT /{id}/reject` | `RadOrder` | `Update` | `[AccessPermission("RadOrder", "Update")]` | Ya |
| `PUT /{id}/cancel` | `RadOrder` | `Cancel` | `[AccessPermission("RadOrder", "Cancel")]` | Ya |
| `GET /worklist` **(rencana)** | `RadOrder` | `Read` | `[AccessPermission("RadOrder", "Read")]` | Tidak |
| `PUT /{id}/urgency` **(rencana)** | `RadOrder` | `Update` | `[AccessPermission("RadOrder", "Update")]` | Ya |

## 2. Rad Study — sudah berjalan

Base URL: `api/v1/health-services/radiology-management/rad-studies`

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /modalities` | `RadStudy` | `Read` | `[AccessPermission("RadStudy", "Read")]` | Tidak |
| `GET /safety-requirements` | `RadStudy` | `Read` | `[AccessPermission("RadStudy", "Read")]` | Tidak |
| `GET /by-order/{radOrderId}` | `RadStudy` | `Read` | `[AccessPermission("RadStudy", "Read")]` | Tidak |
| `GET /by-order/{radOrderId}/history` | `RadStudy` | `Read` | `[AccessPermission("RadStudy", "Read")]` | Tidak |
| `POST /by-order/{radOrderId}` | `RadStudy` | `Create` | `[AccessPermission("RadStudy", "Create")]` | Ya |
| `POST /{id}/verify-patient` | `RadStudy` | `Verify` | `[AccessPermission("RadStudy", "Verify")]` | Ya |
| `POST /{id}/safety-checks` | `RadStudy` | `Safety` | `[AccessPermission("RadStudy", "Safety")]` | Ya |
| `POST /{id}/clear-safety` | `RadStudy` | `Safety` | `[AccessPermission("RadStudy", "Safety")]` | Ya |
| `POST /{id}/start-acquisition` | `RadStudy` | `Acquire` | `[AccessPermission("RadStudy", "Acquire")]` | Ya |
| `POST /{id}/complete-acquisition` | `RadStudy` | `Acquire` | `[AccessPermission("RadStudy", "Acquire")]` | Ya |
| `POST /{id}/abort-acquisition` | `RadStudy` | `Acquire` | `[AccessPermission("RadStudy", "Acquire")]` | Ya |
| `POST /{id}/decide-quality` | `RadStudy` | `Quality` | `[AccessPermission("RadStudy", "Quality")]` | Ya |
| `POST /{id}/repeat` | `RadStudy` | `Repeat` | `[AccessPermission("RadStudy", "Repeat")]` | Ya |
| `POST /{id}/consumptions` | `RadStudy` | `Consumption` | `[AccessPermission("RadStudy", "Consumption")]` | Ya |

## 3. Rad Report — rencana

Base URL: `api/v1/health-services/radiology-management/rad-reports`

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /` | `RadReport` | `Read` | `[AccessPermission("RadReport", "Read")]` | Tidak |
| `GET /{id}` | `RadReport` | `Read` | `[AccessPermission("RadReport", "Read")]` | Tidak |
| `GET /{id}/versions` | `RadReport` | `Read` | `[AccessPermission("RadReport", "Read")]` | Tidak |
| `GET /by-study/{radStudyId}` | `RadReport` | `Read` | `[AccessPermission("RadReport", "Read")]` | Tidak |
| `GET /by-encounter/{encounterId}` | `RadReport` | `Read` | `[AccessPermission("RadReport", "Read")]` | Tidak |
| `POST /by-study/{radStudyId}/draft` | `RadReport` | `Create` | `[AccessPermission("RadReport", "Create")]` | Ya |
| `PUT /{id}/draft` | `RadReport` | `Update` | `[AccessPermission("RadReport", "Update")]` | Ya |
| `POST /{id}/validate` | `RadReport` | `Validate` | `[AccessPermission("RadReport", "Validate")]` | Ya |
| `POST /{id}/release` | `RadReport` | `Release` | `[AccessPermission("RadReport", "Release")]` | Ya |
| `POST /{id}/amendments` | `RadReport` | `Amend` | `[AccessPermission("RadReport", "Amend")]` | Ya |

## 4. Master Data — rencana

Base URL: `api/v1/health-services/radiology-management/master-data/rad-modalities`

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /` | `RadModality` | `Read` | `[AccessPermission("RadModality", "Read")]` | Tidak |
| `GET /{id}` | `RadModality` | `Read` | `[AccessPermission("RadModality", "Read")]` | Tidak |
| `POST /` | `RadModality` | `Create` | `[AccessPermission("RadModality", "Create")]` | Ya |
| `PUT /{id}` | `RadModality` | `Update` | `[AccessPermission("RadModality", "Update")]` | Ya |
| `DELETE /{id}` | `RadModality` | `Delete` | `[AccessPermission("RadModality", "Delete")]` | Ya |

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-requirements`

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /` | `RadSafetyRequirement` | `Read` | `[AccessPermission("RadSafetyRequirement", "Read")]` | Tidak |
| `GET /{id}` | `RadSafetyRequirement` | `Read` | `[AccessPermission("RadSafetyRequirement", "Read")]` | Tidak |
| `POST /` | `RadSafetyRequirement` | `Create` | `[AccessPermission("RadSafetyRequirement", "Create")]` | Ya |
| `PUT /{id}` | `RadSafetyRequirement` | `Update` | `[AccessPermission("RadSafetyRequirement", "Update")]` | Ya |
| `DELETE /{id}` | `RadSafetyRequirement` | `Delete` | `[AccessPermission("RadSafetyRequirement", "Delete")]` | Ya |

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-rules`

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /` | `RadSafetyRule` | `Read` | `[AccessPermission("RadSafetyRule", "Read")]` | Tidak |
| `GET /coverage` | `RadSafetyRule` | `Read` | `[AccessPermission("RadSafetyRule", "Read")]` | Tidak |
| `POST /` | `RadSafetyRule` | `Create` | `[AccessPermission("RadSafetyRule", "Create")]` | Ya |
| `PUT /{id}` | `RadSafetyRule` | `Update` | `[AccessPermission("RadSafetyRule", "Update")]` | Ya |
| `POST /{id}/submit` | `RadSafetyRule` | `Submit` | `[AccessPermission("RadSafetyRule", "Submit")]` | Ya |
| `POST /{id}/approve` | `RadSafetyRule` | `Approve` | `[AccessPermission("RadSafetyRule", "Approve")]` | Ya |
| `POST /{id}/reject` | `RadSafetyRule` | `Reject` | `[AccessPermission("RadSafetyRule", "Reject")]` | Ya |
| `POST /{id}/deactivate` | `RadSafetyRule` | `Deactivate` | `[AccessPermission("RadSafetyRule", "Deactivate")]` | Ya |

---

## 5. Pemisahan Wewenang yang Wajib Dijaga

Hak akses saja **tidak cukup** untuk dua aturan berikut. Keduanya wajib diperiksa di dalam
service, bukan hanya oleh atribut endpoint.

### 5.1 Pengesahan hasil bacaan — `RAD-DEC-003`

Memiliki `RadReport : Validate` **tidak dengan sendirinya** memperbolehkan seseorang
mengesahkan sebuah draf. Service wajib memeriksa dua hal: `AuthorRoleSnapshot` pada versi yang
akan disahkan, dan apakah pengesah memegang `RadReport : ActAsRadiologist` (`RAD-DEC-015`).

| `AuthorRoleSnapshot` | Pengesah boleh orang yang sama? |
|---|:---:|
| `Radiologist` | **Ya** |
| `Resident` | **Tidak** |
| `Radiographer` | **Tidak** |
| `AiAssisted` | **Tidak** |

Pelanggaran ditolak `403` dengan pesan "Draf yang Anda tulis harus disahkan dokter radiolog."

### 5.2 Pengesahan aturan keselamatan — `RAD-DEC-005`

`RadSafetyRule : Create` dan `RadSafetyRule : Approve` **wajib dipegang peran yang berbeda**.
Admin Radiologi menyusun; penanggung jawab klinis mengesahkan.

Memberikan keduanya kepada satu peran akan meniadakan seluruh gunanya pengesahan berjenjang.
Ini **wajib** dijaga saat menyusun peran, dan **wajib** diuji.

---

## 6. Pengenalan Peran Klinis — Ditetapkan `RAD-DEC-015`

Peran klinis dikenali lewat **hak akses penanda**, bukan lewat nama peran maupun tabel
pemetaan tersendiri.

| Sebutan | Hak akses penanda | String yang dipakai | Keadaan |
|---|---|---|---|
| Dokter radiolog | `RadReport : ActAsRadiologist` | `[AccessPermission("RadReport", "ActAsRadiologist")]` | **Baru** |
| Penanggung jawab klinis | `RadSafetyRule : Approve` | `[AccessPermission("RadSafetyRule", "Approve")]` | Sudah pada tabel bagian 4 |
| DPJP | Belum ditetapkan | — | Menyusul bersama `DEC-RAD-001`; `S5` belum dirancang |
| Dokter jaga senior | Belum ditetapkan | — | Sama |

### `ActAsRadiologist` bukan endpoint

Berbeda dari seluruh baris pada bagian 1 sampai 4, hak akses ini **tidak menempel pada satu
endpoint pun**. Ia dibaca service ketika menilai `AuthorRoleSnapshot` dan ketika memutuskan
boleh atau tidaknya pengesahan sendiri.

### Bedanya dengan `Validate`

| Hak akses | Artinya |
|---|---|
| `RadReport : Validate` | Boleh **mencoba** mengesahkan bacaan |
| `RadReport : ActAsRadiologist` | **Dihitung sebagai** dokter radiolog |

> **Contoh mengapa dipisah.** Seorang residen dapat diberi `Validate` supaya ia dapat
> mengesahkan draf yang ditulis radiografer. Tanpa `ActAsRadiologist`, draf yang **ia tulis
> sendiri** tetap ditolak saat ia mencoba mengesahkannya. Menggabungkan keduanya menjadi satu
> hak akses akan menghapus perbedaan itu, dan aturan `RAD-DEC-003` ikut hilang.

### Cara `AuthorRoleSnapshot` diisi

Saat draf dibuat, service menilai hak akses penulis **pada saat itu**, lalu membekukannya:

| Hak akses penulis saat menulis draf | `AuthorRoleSnapshot` |
|---|---|
| Memegang `RadReport : ActAsRadiologist` | `Radiologist` |
| Tidak memegangnya, dan bukan proses sistem | `Resident` atau `Radiographer` sesuai peran yang diberikan Administrator |
| Draf dihasilkan bantuan AI | `AiAssisted` |

Perubahan hak akses seseorang di kemudian hari **tidak** menulis ulang draf yang sudah ada.

### Disiplin pemberian

`RadReport : ActAsRadiologist` menentukan siapa yang boleh menyatakan sebuah bacaan sah dipakai
dokter lain. Ia **tidak boleh** masuk paket hak akses umum, dan pemberiannya wajib dapat
ditelusuri.

### Yang masih terbuka

Nama peran Quilvian yang memegang hak akses ini belum ditetapkan. Itu pekerjaan Administrator
saat menyusun peran, dan **tidak menahan desain maupun implementasi** — yang dirancang adalah
hak aksesnya, bukan nama perannya.

---

## 7. Aturan Logging

| Aspek | Ketentuan |
|---|---|
| Yang dicatat | `EntityId`, nama controller, nama action, kode status |
| Yang **tidak** dicatat | Seluruh kolom bertanda **Sensitif** pada kamus data |
| `GET` | Tidak dicatat, mengikuti konvensi project |

### Kolom yang haram masuk log

`ClinicalIndication`, `ClosureReason`, `QualityNote`, `AbortReason`, `PerformedPortionNote`,
`Note` pada `RadStudySafetyCheck`, `ReasonNote` pada `RadTransitionHistory`, serta `Findings`,
`Impression`, `Recommendation`, dan `AmendmentReason` pada `RadReportVersion`.

Empat kolom terakhir adalah kesimpulan klinis atas seorang pasien. Bocornya ke log berarti data
medis tersimpan di tempat yang aturan aksesnya berbeda dari tabel aslinya.

---

## 8. Audit yang Wajib Terekam

| Kejadian | Yang direkam | Dapat diubah? |
|---|---|:---:|
| Perpindahan status pesanan dan study | Pelaku, waktu, status asal, status tujuan, alasan | Tidak |
| Jawaban butir keselamatan | Pelaku, waktu, keadaan jawaban, salinan aturan yang berlaku | Tidak |
| Pernyataan lolos keselamatan | Pelaku, waktu, **versi aturan saat itu** | Tidak |
| Penilaian mutu citra | Pelaku, waktu, layak atau tidak | Tidak |
| Penerbitan fakta kelayakan tagih | Waktu, penanda terkirim | Tidak |
| **Setiap versi hasil bacaan** | Penulis, **peran penulis saat menulis**, pengesah, waktu draf, waktu sah, waktu rilis, alasan koreksi, versi sebelumnya | Tidak |
| **Pengesahan aturan keselamatan** | Pengesah, waktu, nomor versi baru | Tidak |
| **Penolakan aturan keselamatan** | Penolak, waktu, alasan | Tidak |
| **Penandaan cito pada pesanan** | Siapa yang menandai, kapan | Tidak |

Seluruh jejak di atas hanya bertambah. **Tidak ada endpoint ubah maupun hapus** yang boleh
disediakan untuknya.

---

## 9. Uji Kontrak Hak Akses

Modul Laboratorium dan Bank Darah memiliki uji kontrak hak akses; Radiologi **belum**
(`RAD-CAP-025`). Uji itu direncanakan pada `02-backend-architecture.md` bagian 13.

Yang wajib dibuktikan uji tersebut:

1. Setiap endpoint memuat `[AccessPermission(...)]` dengan string yang persis seperti tabel di
   atas.
2. Tidak ada endpoint radiologi tanpa atribut hak akses.
3. Aturan pemisahan wewenang pada bagian 5 benar-benar ditegakkan service, bukan hanya
   didokumentasikan.
