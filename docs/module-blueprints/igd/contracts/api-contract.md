# Kontrak API — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.5.0` — penyelarasan teks 15 September 2026. **Bukan aditif murni**: §3 bertambah query `at` pada `GET /active` (`IGD-DEC-117`, aditif), dan §5 mencatat satu penolakan `400` baru pada `PATCH .../observation-status` (`IGD-DEC-119`). Lihat manifest bagian 0c. *Sebelumnya `0.4.0` — revisi 6, bukan aditif: dua route revisi 5 diganti (`/pending-orders`, `/order-actions`); lihat manifest 0a.2* |
| Status | `draft` |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | — / — |
| `input_revision` | `00-interview-decisions.md` **121 keputusan**, terakhir `IGD-DEC-121`; `01-existing-capability-map.md` revision `3` + suplemen `3.1` |
| `input_hash` | Dihitung ulang pada manifest bagian 2, penyelarasan teks 2026-09-15 |
| Versi sebelumnya | `0.4.0` (revisi 6, 26 Agustus 2026). Versi `approved` penuh terakhir: `0.2.0`, 14 Agustus 2026 |
| Commit diaudit | backend `300922c` (suplemen capability `3.1`); revisi 5 disusun pada `f69e9e48` |

## Dampak kompatibilitas terhadap `0.2.0`

| Perubahan | Sifat | Akibat bagi pemakai lama |
| --- | --- | --- |
| Grup `emergency-transfers` **berganti nama** menjadi `emergency-departures` | **Memutus** | Seluruh pemanggil route lama gagal. Lihat rencana peralihan |
| `TransferStatus` dipecah menjadi `PhysicalStatus` dan `HandoverStatus` | **Memutus** | Pemakai yang membaca satu kolom status harus membaca dua |
| Empat field tempat tidur dan ruangan dihapus dari request dan response | **Memutus** | Pemanggil yang mengirimnya akan ditolak |
| Kunjungan IGD wajib `EncounterType.Emergency` | **Memutus** | Pemanggil yang mengirim `Outpatient` ditolak — termasuk test `FE-IGD-001 K1` |
| Endpoint baru penetapan dokter, daftar pantau pengkajian ulang, sikap pesanan | Aditif | Tidak memutus |
| Field SBAR baru pada kepergian | Aditif | Tidak memutus |

**Rencana peralihan.** Route lama `emergency-transfers` dipertahankan sebagai alias yang
menjawab `410 Gone` beserta pesan yang menyebut route penggantinya, selama satu siklus rilis.
Alias ini **tidak** meneruskan permintaan, karena bentuk datanya sudah berbeda dan meneruskan
diam-diam akan menyimpan data yang salah.

---

## Grup Swagger

| Grup `[Tags(...)]` | Base URL |
| --- | --- |
| `Emergency Visit` | `api/v1/health-services/emergency-installation-management/emergency-visits` |
| `Emergency Triage` | `.../emergency-triages` |
| `Emergency Triage Detail` | `.../emergency-triage-details` |
| `Emergency Observation` | `.../emergency-observations` |
| `Emergency Observation Detail` | `.../emergency-observation-details` |
| `Emergency Resuscitation` | `.../emergency-resuscitations` |
| `Emergency Procedure Detail` | `.../emergency-procedure-details` |
| `Emergency Disposition` | `.../emergency-dispositions` |
| `Emergency Departure` | `.../emergency-departures` |
| `Emergency Doctor Assignment` | `.../emergency-doctor-assignments` |
| `Emergency Reassessment Watchlist` | `.../emergency-reassessment-watchlist` |

Seluruh balasan terbungkus `ApiResponse<T>`. Daftar memakai `PagedResult<T>`.

---

## 1. `Emergency Visit`

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses | Kode status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kunjungan IGD dengan penyaring dan halaman | `EmergencyVisit : Read` | `200`, `403` |
| `GET` | `/{id}` | Satu kunjungan beserta konteksnya | `EmergencyVisit : Read` | `200`, `403`, `404` |
| `POST` | `/` | Membuat kunjungan IGD | `EmergencyVisit : Create` | `201`, `400`, `403`, `409` |
| `PUT` | `/{id}` | Mengubah data kunjungan | `EmergencyVisit : Update` | `200`, `400`, `403`, `404` |
| `PATCH` | `/{id}/registration-status` | Mengubah status registrasi | `EmergencyVisit : Update` | `200`, `400`, `403`, `404` |
| `PATCH` | `/{id}/visit-status` | Mengubah status kunjungan | `EmergencyVisit : Update` | `200`, `400`, `403`, `404`, `409` |
| `PATCH` | `/{id}/complete` | Menyelesaikan kunjungan setelah gerbang penutupan lulus | `EmergencyVisit : Update` | `200`, `403`, `404`, `409` |
| `DELETE` | `/{id}` | Menandai kunjungan terhapus | `EmergencyVisit : Delete` | `200`, `403`, `404` |

### 1.1 `POST /` — perubahan pada `0.3.0`

| Field request | Tipe | Wajib | Perubahan |
| --- | --- | :---: | --- |
| `encounterId` | `uuid?` | Bersyarat | Wajib bila `registrationStatus` `Registered` atau `Completed` |
| `patientId` | `uuid?` | Bersyarat | Wajib bila bukan pasien tanpa identitas |
| `serviceUnitId` | `uuid` | Ya | Harus sama dengan unit IGD pada pengaturan aktif |
| `isUnknownPatient` | `bool` | Tidak | — |
| `temporaryPatientAlias` | `string?` | Bersyarat | Wajib bila `isUnknownPatient` |

**Penolakan baru:**

| Kode | Sebab | Keputusan |
| --- | --- | --- |
| `400` | Encounter bertipe selain `Emergency` | `IGD-DEC-074` |
| `409` | Pasien masih memiliki kunjungan IGD aktif. Pesan **wajib** memuat nomor kunjungan yang sudah ada | `IGD-DEC-084` |
| `400` | Master kelas pasien bertanda `IsForEmergency` dan `IsDefault` tidak ada atau lebih dari satu | `IGD-DEC-076` |

### 1.2 `PATCH /{id}/complete`

Gerbang penutupan diperluas. Menolak `409` bila salah satu berlaku:

1. status kunjungan bukan `Disposed`;
2. masih ada observasi berstatus `Active`;
3. masih ada kepergian yang rangkaian fisiknya belum `Arrived` atau `Cancelled`;
4. **baru** — masih ada pesanan yang belum diberi sikap pada kepergian yang dokumennya sudah
   diajukan.

Status tagihan **tidak** diperiksa, sesuai `IGD-DEC-021`.

---

## 2. `Emergency Departure`

Base URL: `.../emergency-departures`. Menggantikan `emergency-transfers`.

| Method | Path | Kegunaan | Hak akses | Kode status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kepergian dengan penyaring dua rangkaian status | `EmergencyDeparture : Read` | `200`, `403` |
| `GET` | `/{id}` | Satu kepergian beserta kejadian dan daftar pesanannya | `EmergencyDeparture : Read` | `200`, `403`, `404` |
| `POST` | `/` | Membuat catatan kepergian, rangkaian fisik `Prepared` | `EmergencyDeparture : Create` | `201`, `400`, `403` |
| `GET` | `/{id}/order-items` | Daftar pesanan yang belum selesai beserta sikap dan keadaan penerimaannya — **menggantikan `/pending-orders` pada revisi 6** | `EmergencyDeparture : Read` | `200`, `403`, `404` |
| `POST` | `/{id}/order-items` | Mendaftarkan pesanan yang dibuat **di luar sistem** | `EmergencyDeparture : Update` | `201`, `400`, `403`, `404` |
| `PATCH` | `/{id}/order-items/{itemId}/action` | Menetapkan sikap `Continue`, `Handover`, atau `Cancel` — **menggantikan `/order-actions` pada revisi 6** | `EmergencyDeparture : Update` | `200`, `400`, `403`, `404`, `409` |
| `POST` | `/{id}/order-items/{itemId}/accept` | Unit penerima menerima **satu pesanan** | `EmergencyDeparture : Update` | `200`, `403`, `404`, `409` |
| `POST` | `/{id}/order-items/{itemId}/reject` | Unit penerima menolak satu pesanan; alasan wajib | `EmergencyDeparture : Update` | `200`, `400`, `403`, `404`, `409` |
| `POST` | `/{id}/submit-handover` | Mengajukan dokumen serah terima; mengisi tiga bagian otomatis | `EmergencyDeparture : Update` | `200`, `400`, `403`, `404`, `409` |
| `POST` | `/{id}/depart` | Mencatat pasien meninggalkan IGD | `EmergencyDeparture : Update` | `200`, `400`, `403`, `404`, `409` |
| `POST` | `/{id}/arrive` | Mencatat pasien tiba di unit tujuan; **memindahkan pemilik klinis** | `EmergencyDeparture : Update` | `200`, `400`, `403`, `404`, `409` |
| `POST` | `/{id}/accept-handover` | Penerima menyatakan menerima dokumen | `EmergencyDeparture : Update` | `200`, `403`, `404`, `409` |
| `POST` | `/{id}/reject-handover` | Penerima menolak; alasan wajib | `EmergencyDeparture : Update` | `200`, `400`, `403`, `404`, `409` |
| `POST` | `/{id}/events/{eventId}/amend` | Mengoreksi waktu sebuah kejadian | `EmergencyDeparture : Update` | `200`, `400`, `403`, `404` |
| `POST` | `/{id}/events/{eventId}/reverse` | Membalik kejadian salah pasien atau salah unit; butuh persetujuan orang kedua | `EmergencyDeparture : Approve` | `200`, `400`, `403`, `404`, `409` |
| `PATCH` | `/{id}/cancel` | Membatalkan kepergian; alasan wajib | `EmergencyDeparture : Update` | `200`, `400`, `403`, `404`, `409` |

### 2.1 `POST /{id}/arrive`

| Field request | Tipe | Wajib | Keterangan |
| --- | --- | :---: | --- |
| `occurredAt` | `datetime?` | Tidak | Waktu kedatangan **sebenarnya**. Kosong berarti sama dengan waktu server |
| `downtimeReference` | `string?` | Bersyarat | **Wajib** bila `occurredAt` terpaut lebih dari ambang yang dikonfigurasi dari waktu server |
| `notes` | `string?` | Tidak | — |

**Hak akses tambahan:** pemanggil **wajib** berwenang atas `toServiceUnitId`, ditentukan
`EmergencyUnitAuthorityService`. Ditolak `403` bila tidak.

**Akibat:** rangkaian fisik menjadi `Arrived`; pemilik klinis berpindah ke unit penerima;
rangkaian dokumen **tidak** berubah (`IGD-DEC-064`).

### 2.2 `POST /{id}/submit-handover`

Tiga bagian diisi sistem, bukan pemanggil: `allergySnapshot`, `lastVitalSignId`, dan
`triageLevelSnapshot`. Empat bagian SBAR diisi pemanggil.

Menolak `400` bila salah satu bagian SBAR kosong dan tidak ditandai tidak dapat diisi beserta
alasannya.

**Tidak** menahan rangkaian fisik: `POST /{id}/depart` dan `POST /{id}/arrive` tetap dapat
dipanggil walaupun dokumen belum diajukan (`IGD-DEC-070`, `IGD-DEC-078`).

### 2.3 Sikap dan penerimaan pesanan — baru pada revisi 6

Ditetapkan `IGD-DEC-100`, `101`, `102`, `103`. Endpoint-nya tercantum pada tabel bagian 2.

**Dua route revisi 5 digantikan.** `GET /{id}/pending-orders` dan `POST /{id}/order-actions`
disusun sebelum penerimaan per pesanan ada, sehingga tidak punya tempat untuk `accept` dan
`reject` per baris. Keduanya diganti keluarga `order-items` yang memperlakukan pesanan sebagai
resource, bukan sebagai aksi lepas.

| Route revisi 5 | Pengganti revisi 6 |
| --- | --- |
| `GET /{id}/pending-orders` | `GET /{id}/order-items` |
| `POST /{id}/order-actions` | `PATCH /{id}/order-items/{itemId}/action` |

Keduanya **belum pernah diimplementasikan** — nol berkas di source — sehingga penggantian ini
tidak memutus pemakai mana pun.

#### Empat hal yang wajib dipegang pemanggil

1. **Penerimaan pesanan tidak menggeser penerimaan pasien.** `reject` pada satu pesanan
   **tidak** mengubah `handoverStatus` maupun `physicalStatus` — `IGD-DEC-102` butir (d).
2. **Penolakan menuntut sikap pengganti.** Setelah `reject`, pesanan itu kembali menjadi
   tanggung jawab dokter pemesan. `PATCH …/action` berikutnya membuat **baris baru** yang
   menunjuk baris lama; baris lama tetap terbaca sebagai tidak berlaku.
3. **`Continue` tidak menahan penutupan kunjungan.** Ia sikap yang sah dan disengaja, bukan
   pesanan yang terlupakan.
4. **Sikap pesanan laboratorium berasal dari petugas, bukan dari sistem laboratorium.**
   Response **wajib** memuat penanda itu, dan layar wajib menampilkannya — `IGD-DEC-101`.
   Menyajikannya seolah dibaca dari `LabOrder` melanggar kontrak.

#### Bentuk baris pesanan

| Field | Wajib | Catatan |
| --- | :-: | --- |
| `orderKind` | Ya | `Medication`, `Procedure`, `LaboratoryOrder`, `RadiologyOrder` |
| `orderSource` | Ya | `Internal` atau `External` |
| `orderReferenceId` | Hanya `Internal` | Kosong untuk pesanan luar sistem |
| `externalReference` | Hanya `External` | Nomor rujukan dari sistem luar |
| `orderDescription` | **Selalu** | Uraian yang dapat diaudit |
| `action`, `actionReason`, `actionByUserId`, `actionAt` | Ya | `actionReason` wajib untuk `Cancel` |
| `acceptanceStatus`, `acceptedByUserId`, `acceptedAt`, `rejectionReason` | — | Diisi jalur `accept` / `reject` |
| `isEffective`, `supersedesOrderItemId` | — | Menandai baris yang sudah digantikan |

---

## 3. `Emergency Doctor Assignment` — grup baru, Rencana (belum tersedia)

Base URL: `.../emergency-doctor-assignments`

| Method | Path | Kegunaan | Hak akses | Kode status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Riwayat penugasan dokter pada satu kunjungan IGD | `EmergencyDoctorAssignment : Read` | `200`, `403` |
| `GET` | `/active` | Dokter yang aktif pada satu kunjungan — **sekarang**, atau **pada waktu tertentu** lewat query `at` (bagian 3.1) | `EmergencyDoctorAssignment : Read` | `200`, `403`, `404` |
| `POST` | `/` | Menetapkan dokter pertama | `EmergencyDoctorAssignment : Create` | `201`, `400`, `403`, `409` |
| `POST` | `/{id}/handover` | Mengalihkan ke dokter lain; alasan wajib | `EmergencyDoctorAssignment : Update` | `200`, `400`, `403`, `409` |

`POST /` menolak `409` bila kunjungan sudah memiliki dokter aktif — pengalihan wajib memakai
`/{id}/handover` supaya baris lama memperoleh waktu berakhir dan alasannya tercatat.

Setiap penulisan juga memperbarui `TrxPatientEncounter.DoctorId` sebagai nilai efektif dalam
transaksi yang sama.

Endpoint lama `PATCH /patient-encounters/{id}/doctor` **tetap ada** dan tetap milik
Registration Management, tetapi **tidak lagi dipakai layar IGD**.

Riwayat penugasan disimpan pada tabel **`EmgDoctorAssignment`** (`IGD-DEC-116`). Nama
`TrxEmergencyDoctorAssignment` pada dokumen desain lama dibaca sebagai nama yang sudah diganti.

### 3.1 Query `at` pada `GET /active` — baru pada `0.5.0`

Ditetapkan `IGD-DEC-117`. Pertanyaan *"siapa dokter penanggung jawab pasien ini pukul 10.30
tadi?"* dijawab oleh endpoint yang sama, bukan endpoint baru.

| Query | Tipe | Wajib | Nilai bawaan | Keterangan |
| --- | --- | :---: | --- | --- |
| `at` | `datetime?` | Tidak | Kosong = waktu sekarang | Waktu yang ditanyakan. Dokter dihitung dari riwayat `EmgDoctorAssignment` |

Cara menyebut kunjungan yang ditanyakan mengikuti parameter yang sama dengan `GET /active` tanpa
`at`; query `at` tidak mengubahnya.

| Keadaan | Jawaban |
| --- | --- |
| Tanpa `at` | `200` — dokter yang aktif **sekarang** (perilaku lama, tidak berubah) |
| Dengan `at`, ada penugasan yang berlaku pada waktu itu | `200` — dokter yang aktif **pada waktu itu** |
| Dengan `at`, tidak ada dokter pada waktu itu — misalnya sebelum penugasan pertama | `404` |

**Dilarang** membuat endpoint terpisah untuk pencarian berdasarkan waktu selama query parameter
ini sudah cukup (`IGD-DEC-117`).

*Contoh:* dr. Budi ditetapkan pukul 08.00, lalu dialihkan ke dr. Sita pukul 14.00. `GET /active`
dengan `at=2026-09-15T10:30:00` menjawab dr. Budi. Tanpa `at` pada pukul 16.00 menjawab dr. Sita.
Dengan `at=2026-09-15T07:00:00` menjawab `404`.

---

## 4. `Emergency Reassessment Watchlist` — grup baru

Base URL: `.../emergency-reassessment-watchlist`

| Method | Path | Kegunaan | Hak akses | Kode status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Pasien yang pemicu pengkajian ulangnya sudah terpenuhi tetapi belum ditindaklanjuti | `EmergencyVisit : Read` | `200`, `403` |

Setiap baris memuat penanda `intervalStatus` bernilai `Configured` atau `NotConfigured`.
Baris `NotConfigured` **tetap ditampilkan** dan **tidak** dianggap patuh maupun terlambat
(`IGD-DEC-083`).

Endpoint ini **tidak pernah** menolak tindakan klinis apa pun; ia hanya membaca.

---

## 5. Grup yang tidak berubah

`Emergency Triage`, `Emergency Triage Detail`, `Emergency Observation`,
`Emergency Observation Detail`, `Emergency Resuscitation`, `Emergency Procedure Detail`, dan
`Emergency Disposition` mempertahankan bentuk `0.2.0`, dengan tiga pengecualian:

| Grup | Perubahan perilaku, bukan bentuk |
| --- | --- |
| `Emergency Triage` | `PATCH /{id}/triage-status` menjadi `Completed` kini **wajib** lewat `CanTransition` dan **menolak** `409` bila kunjungan sudah `Disposed`, `Completed`, atau `Cancelled` |
| `Emergency Disposition` | `PATCH /{id}/disposition-status` menjadi `Executed` membaca `ClosesEmergencyVisit` untuk menentukan apakah kunjungan menjadi `Disposed` |
| `Emergency Observation` | **Baru pada `0.5.0`.** `PATCH /{id}/observation-status` dengan target `Completed` atau `Escalated` **menolak** `400` bila catatan lebih dari 1000 karakter; catatan **tidak pernah dipotong diam-diam**. Rincian di validation §8 (`IGD-DEC-119`) |

---

## 6. Kontrak as-is yang digantikan

| As-is `0.2.0` | To-be `0.3.0` |
| --- | --- |
| `POST .../emergency-transfers` dengan `fromBedId`, `toBedId`, `fromRoomId`, `toRoomId` | `POST .../emergency-departures` tanpa keempatnya |
| `PATCH .../emergency-transfers/{id}/transfer-status` dengan satu nilai status | Enam endpoint tindakan tersendiri, masing-masing menulis satu kejadian |
| Encounter IGD bertipe `Outpatient` | Bertipe `Emergency` |
| Dokter ditetapkan lewat `PATCH /patient-encounters/{id}/doctor` | Lewat grup `Emergency Doctor Assignment` |
