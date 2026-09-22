# Kontrak API — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.11.0` — encounter-first, 22 September 2026, **Rencana (belum tersedia)**. **Bukan aditif murni**: lima perilaku lama berubah (bagian 8.1 nomor 1–5, 7) — penjaga episode dan tanpa-antrean pada `POST /patient-encounters` bertipe Emergency, penolakan `PATCH …/status` dan `…/cancel` Registrasi untuk Emergency, penguncian `PUT /emergency-visits/{id}`; ditambah efek samping penutupan encounter, perluasan `active-episode`, empat endpoint baru pada `Emergency Visit`, dan grup baru `Emergency Encounter Reconciliation` (`IGD-DEC-139`, `142`…`148`, `150`…`154`). Kelayakan dokter jaga **tidak** dikontrakkan (`IGD-OQ-102`/`103`). Sebelumnya `0.10.0` — pra-cek episode IGD berjalan, 21 September 2026. **Aditif** (`BE-IGD-050`, `IGD-DEC-138`): satu endpoint baca-saja `GET emergency-visits/active-episode?patientId=` pada grup `Emergency Visit`, bagian 1.3. Hak akses `EmergencyVisit : Create` (aksi `Create` yang sudah ada — nol permission baru). `POST /` **tidak berubah**: penolakan `409` episode ganda tetap sebagai jaring pengaman. Nol ruas lama berubah, nol perubahan bentuk request, nol schema. **Tidak menutup** dua celah `IGD-OQ-093` (pendaftaran serentak; klien tanpa pra-cek). Sebelumnya `0.9.0` — nama pelaku pada event kepergian, 21 September 2026. **Aditif** (`BE-IGD-049`, `IGD-DEC-137`): `EmergencyDepartureEventResponse` bertambah `recordedByName` dan `approvedByName` (`string?`), bagian 2.4; `recordedByUserId` dan `approvedByUserId` tetap dikirim. Nol route baru, nol bentuk request berubah, nol perubahan hak akses, nol schema. Sebelumnya `0.8.0` — pengisian data lama penugasan dokter, 21 September 2026. **Relaxed nullability change for legacy response**, bukan aditif murni: `assignedByUserId` pada response §3.2 (`GET /`, `GET /active`) dapat bernilai `null` khusus baris riwayat hasil pengisian data lama `BE-IGD-048` yang pelaku historisnya tidak dapat dibuktikan (`IGD-DEC-136`). **Bukan** perubahan semantics penetapan atau pengalihan baru: pelaku tetap wajib dari token dan request tidak menerimanya. Nol route baru, nol ruas dihapus, nol bentuk request berubah. **Dampak konsumen**: kode yang mengasumsikan `assignedByUserId` selalu berupa GUID (misalnya tipe non-nullable di frontend) harus direvisi sebelum menampilkan baris legacy — lihat bagian 3.2. Sebelumnya `0.7.0` — kesiapan `EPIC IGD-04`, 16 September 2026 (ketiga). **Aditif**: response §3 bertambah proyeksi `doctorName` dan `assignedByName` (`IGD-DEC-129`, bagian 3.2), dan nama tabel Registrasi diselaraskan menjadi `RegPatientEncounter` (`IGD-DEC-132`). Nol route baru, nol bentuk request berubah, nol penolakan baru. *Sebelumnya `0.6.0` — pemantauan observasi bertanda vital, 16 September 2026, **aditif**: bagian 7 baru untuk `Emergency Observation Detail` — bentuk request tidak bertambah, response bertambah proyeksi `vitalSign` dan `recordedByName`, dan dua penolakan baru ditegakkan (`IGD-DEC-122`, `IGD-DEC-126`). Ruas `recordedByUserId` pada request menjadi **usang tetapi tetap diterima**. Lihat manifest bagian 0d. Sebelumnya `0.5.0` — penyelarasan teks 15 September 2026: query `at` pada §3 dan penolakan catatan observasi lebih dari 1000 karakter* |
| Status | `draft`, **kecuali bagian 8 (encounter-first) yang `approved`** (`IGD-DEC-157`, 22 September 2026). Bagian 8 tetap **Rencana (belum tersedia)** sampai task-nya selesai |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | **Rizki Gunawan / 2026-09-22** — bagian 8 (encounter-first) lewat `IGD-DEC-157`. Bagian lain tetap `draft`; versi `approved` penuh terakhir `0.2.0` |
| `input_revision` | `00-interview-decisions.md` **154 keputusan**, terakhir `IGD-DEC-154`; `01-existing-capability-map.md` revision `3` + suplemen `3.1` + suplemen `3.2`; `evidence/02-requirement-completeness-gate.md` `0.1` (slice encounter-first). *Sebelumnya 126 keputusan sampai `IGD-DEC-126`* |
| `input_hash` | Dihitung ulang pada manifest bagian 0g (22 September 2026); sebelumnya bagian 2, penyelarasan teks 2026-09-15 |
| Versi sebelumnya | `0.4.0` (revisi 6, 26 Agustus 2026). Versi `approved` penuh terakhir: `0.2.0`, 14 Agustus 2026 |
| Commit diaudit | backend `0d13f3a8` / frontend `c941012ac` untuk bagian 8 (suplemen capability `3.2`); backend `300922c` (suplemen `3.1`); revisi 5 disusun pada `f69e9e48` |

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
| `Emergency Encounter Reconciliation` | `.../emergency-encounter-reconciliations` — **Rencana (belum tersedia)**, bagian 8.4 |
| `Patient Encounter` *(milik Registration Management)* | `api/v1/health-services/registration-management/patient-encounters` — hanya perilaku khusus Emergency, bagian 8.2 |

Seluruh balasan terbungkus `ApiResponse<T>`. Daftar memakai `PagedResult<T>`.

---

## 1. `Emergency Visit`

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses | Kode status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kunjungan IGD dengan penyaring dan halaman | `EmergencyVisit : Read` | `200`, `403` |
| `GET` | `/{id}` | Satu kunjungan beserta konteksnya | `EmergencyVisit : Read` | `200`, `403`, `404` |
| `GET` | `/active-episode?patientId={uuid}` | Pra-cek: apakah pasien masih punya kunjungan IGD berjalan — baca-saja, dipanggil **sebelum** encounter dibuat (bagian 1.3, `0.10.0`) | `EmergencyVisit : Create` | `200`, `400`, `401`, `403` |
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

### 1.3 `GET /active-episode` — pra-cek episode berjalan, baru pada `0.10.0` (`BE-IGD-050`, `IGD-DEC-138`)

`GET .../emergency-visits/active-episode?patientId={uuid}`

**Latar.** Pendaftaran IGD terdiri atas dua permintaan terpisah: `POST patient-encounters` (modul Registrasi, commit
lebih dulu) lalu `POST /` di atas. Penolakan `409` episode ganda (§1.1) baru terjadi pada permintaan kedua, sehingga
selalu meninggalkan `RegPatientEncounter` tanpa `EmgVisit`. Endpoint ini memungkinkan layar bertanya **lebih dulu**,
sebelum encounter dibuat. Ini lapis A pada `IGD-DEC-138`; lapis B tercatat pada `IGD-OQ-093`.

| Query | Tipe | Wajib | Keterangan |
| --- | --- | :---: | --- |
| `patientId` | `uuid` | Ya | Pasien yang akan didaftarkan. Kosong atau `00000000-0000-0000-0000-000000000000` → `400` |

**Respons `200`** — `ApiResponse<EmergencyActiveEpisodeResponse>`:

| Ruas | Tipe | Keterangan |
| --- | --- | --- |
| `hasActiveEpisode` | `bool` | `true` bila pasien masih punya kunjungan IGD yang episodenya berjalan |
| `visit` | `object?` | Kunjungan yang sudah ada; `null` bila `hasActiveEpisode` `false` |
| `visit.id` | `uuid` | Untuk membuka kunjungan yang sudah ada |
| `visit.encounterId` | `uuid?` | Encounter kunjungan itu |
| `visit.patientId` | `uuid` | Sama dengan `patientId` pada query |
| `visit.patientName` | `string` | Nama pasien; bila tidak ada, alias sementara; bila tidak ada, "Pasien belum teridentifikasi". **Tidak pernah kosong**, sama dengan `patientName` pada `EmergencyVisitResponse` |
| `visit.emergencyVisitNumber` | `string` | Nomor kunjungan |
| `visit.visitStatus` | `int` | Nilai `EmergencyVisitStatus` yang sama dengan `EmergencyVisitResponse.visitStatus` (`1` `Arrived` … `9` `Completed`) |
| `visit.arrivalDateTime` | `datetime` | Waktu tiba (UTC) |

```json
{ "hasActiveEpisode": true,
  "visit": { "id": "…", "encounterId": "…", "patientId": "…",
             "patientName": "RAYYAN DHAFIR PRASETYA MAULANA",
             "emergencyVisitNumber": "IGD-0001", "visitStatus": 2,
             "arrivalDateTime": "2026-09-21T09:35:00Z" } }
```

Tanpa episode berjalan: `{ "hasActiveEpisode": false, "visit": null }`. Status `200` dipilih, **bukan** `404`, supaya
pra-cek yang normal tidak menghasilkan galat pada log maupun layar.

| Kode | Sebab | Pesan |
| --- | --- | --- |
| `400` | `patientId` kosong atau `Guid.Empty` | "patientId wajib diisi. Pilih pasien lebih dulu sebelum memeriksa kunjungan IGD yang masih berjalan." |
| `401` | Belum login | — |
| `403` | Tidak memegang `EmergencyVisit : Create` | — |

*Nilai `patientId` yang bukan berbentuk GUID ditolak `400` oleh pengikat model bawaan ASP.NET Core dengan bentuk galat
bawaannya, bukan `ApiResponse` — perilaku yang sama dengan seluruh filter `Guid` pada `GET /`.*

**Aturan.**

1. **Baca-saja.** Nol `INSERT`, `UPDATE`, atau `DELETE`: jumlah baris `RegPatientEncounter` dan `EmgVisit` sebelum dan
   sesudah pemanggilan sama.
2. **Satu aturan "berjalan".** Endpoint memanggil `EmergencyVisitService.CariEpisodeAktifAsync` — method yang **sama**
   dengan penolakan `409` pada `POST /`. Kunjungan dihitung berjalan bila milik pasien itu, belum ditandai terhapus,
   dan statusnya **bukan** `Completed` maupun `Cancelled`; `Disposed` **masih berjalan**. Bila lebih dari satu, yang
   `arrivalDateTime`-nya terbaru dikembalikan. Nol salinan aturan, sehingga pra-cek tidak dapat menyimpang dari `POST /`.
3. **Tidak menahan apa pun.** Endpoint hanya menyatakan fakta. Keputusan berhenti atau lanjut ada pada pemanggil: layar
   menampilkan kunjungan yang sudah ada dan **tidak membuat encounter** bila `hasActiveEpisode` `true` dan alasan
   pendaftaran ganda (§1.1) kosong. Pra-cek yang gagal atau tidak dijawab tidak boleh menutup pendaftaran; sikap layar
   pada keadaan itu (`fail-open`) adalah keputusan pemilik untuk `FE-IGD-034`.
4. **Satu kueri.** Nama pasien terbaca dalam kueri yang sama dengan pencarian kunjungan (satu `JOIN` ke `MstPatient`).
   Nol `N+1`, nol kueri kedua.
5. **Hak akses `EmergencyVisit : Create`.** Peran yang boleh mendaftarkan kunjungan otomatis boleh memeriksanya. Memakai
   aksi `Create` yang sudah ada — **nol permission baru**, tidak ada baris baru pada layar Akses Role.
6. **`POST /` tidak berubah.** Penolakan `409` episode ganda (§1.1) tetap ada sebagai jaring pengaman.

**Celah yang tidak ditutup endpoint ini (`IGD-OQ-093`, `open`).** Pra-cek ditegakkan oleh **pemanggil**, bukan server.
Karena itu: (a) dua pendaftaran serentak untuk pasien yang sama dapat sama-sama lolos pra-cek lalu salah satunya
ditolak `409` sesudah encounter-nya terbentuk; (b) klien yang memanggil `POST patient-encounters` lalu `POST /` tanpa
memanggil pra-cek tetap dapat meninggalkan encounter yatim. Encounter yatim yang **sudah ada** tidak dibersihkan oleh
task ini; menghapus atau membersihkannya dilarang tanpa audit seluruh referensi (`IGD-DEC-138`).

**Dampak konsumen.** Aditif: tidak ada yang rusak. `FE-IGD-034` memanggil endpoint ini sebelum `POST patient-encounters`.

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

### 2.4 Nama pelaku pada event kepergian — baru pada `0.9.0` (`BE-IGD-049`, `IGD-DEC-137`)

Setiap **event** kepergian kini membawa nama tampilan pelaku **selain** ID-nya. Perubahan ini **aditif**:
tidak ada ruas yang dihapus, diganti nama, atau berubah nilainya.

| Ruas pada `EmergencyDepartureEventResponse` | Tipe | Keterangan |
| --- | --- | --- |
| `recordedByUserId` | `uuid` | **Tetap.** Pengguna yang mencatat kejadian |
| `recordedByName` | `string?` | **Baru.** Nama tampilan pencatat |
| `approvedByUserId` | `uuid?` | **Tetap.** Penyetuju pembalikan; kosong bila kejadian tidak butuh persetujuan |
| `approvedByName` | `string?` | **Baru.** Nama tampilan penyetuju |

**Aturan.**

1. **`null` bukan GUID.** Nama bernilai `null` bila pengguna tidak ditemukan, atau bila ruas ID-nya kosong
   (`approvedByUserId` kosong pada kejadian biasa, atau `Guid.Empty`). Layar **dilarang** menampilkan GUID sebagai
   gantinya; tampilkan tanda hubung.
2. **Urutan nama:** `DisplayName`, lalu `UserName`, `Email`, `UserCode` — urutan yang sama dengan `recordedByName`
   pada §7 dan `assignedByName` pada §3.2. Nilai **kosong atau spasi dilewati**: `ApplicationUser.DisplayName`
   bernilai bawaan string kosong dan tidak pernah `null`, jadi tanpa penyaringan ini urutan itu tidak pernah
   jatuh ke calon berikutnya. Hasilnya tidak pernah string kosong.
3. **Riwayat tetap terbaca.** Nama diambil tanpa menyaring status pengguna, sehingga pelaku yang sudah tidak
   aktif tetap muncul.
4. **Efisiensi.** Nama seluruh kejadian pada **satu respons** diambil dengan **satu** kueri ke tabel pengguna
   (`WHERE Id IN (...)`), berapa pun jumlah kejadian atau kepergian pada halaman itu. Nol `N+1`, nol kolom baru,
   nol migration.

**Endpoint yang bentuk responsnya berubah** (route, request, dan hak akses **tidak** berubah):

| Method | Path | Respons yang memuat event |
| --- | --- | --- |
| `GET` | `/` | Setiap kepergian pada halaman, `events[]` |
| `GET` | `/{id}` | `events[]` |
| `POST` | `/` | `events[]` (event `Prepared`) |
| `POST` | `/{id}/submit-handover`, `/depart`, `/arrive`, `/accept-handover`, `/reject-handover`; `PATCH /{id}/cancel` | `events[]` |
| `POST` | `/{id}/events/{eventId}/amend` | Satu event (`EmergencyDepartureEventResponse`) |
| `POST` | `/{id}/events/{eventId}/reverse` | Satu event; `approvedByName` terisi |

Contoh event hasil pembalikan:

```json
{
  "id": "…",
  "eventType": 11,
  "recordedByUserId": "0ba84a1a-…",
  "recordedByName": "Ns. Ani Rahmawati",
  "approvedByUserId": "5c21e0d4-…",
  "approvedByName": "dr. Budi Santoso",
  "isEffective": true
}
```

**Yang sengaja tidak termasuk (`IGD-DEC-137`).** Ruas aktor lain tetap berupa ID dan **tidak** diberi nama:
`requestedByUserId`, `sendingNurseUserId`, `receivingNurseUserId` pada kepergian, serta `actionByUserId` dan
`acceptedByUserId` pada baris pesanan. Menambahkannya butuh keputusan tersendiri. Keputusan ini juga tidak
mengubah authorization maupun siapa yang boleh membaca data pengguna: hak akses tiap endpoint sama seperti sebelumnya.

**Dampak konsumen.** Tidak ada yang rusak: konsumen lama mengabaikan dua ruas baru. `FE-IGD-017` menampilkan
`recordedByName` dan `approvedByName` pada tab kepergian menggantikan GUID mentah.

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

Setiap penulisan juga memperbarui `RegPatientEncounter.DoctorId` sebagai nilai efektif dalam
transaksi yang sama. *Nama tabel diselaraskan `IGD-DEC-132`; sebelumnya tertulis
`TrxPatientEncounter`, nama yang sudah diganti tim Registrasi pada `58c61a5b`.*

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

### 3.2 Proyeksi nama pada response — baru pada `0.7.0`; `assignedByUserId` nullable pada `0.8.0`

Ditetapkan `IGD-DEC-129`. Penambahannya **aditif**: ruas lama tetap dikirim, nol ruas dihapus,
nol kolom baru pada `EmgDoctorAssignment`.

| Ruas | Tipe | Berlaku pada | Keterangan |
| --- | --- | --- | --- |
| `doctorId` | `uuid` | `GET /`, `GET /active` | **Tetap dikirim.** Identitas untuk pemrosesan |
| `doctorName` | `string` | `GET /`, `GET /active` | **Baru.** Nama dokter untuk ditampilkan |
| `assignedByUserId` | `uuid?` | `GET /`, `GET /active` | **Tetap dikirim.** **Nullable sejak `0.8.0`** — lihat catatan legacy di bawah |
| `assignedByName` | `string` | `GET /`, `GET /active` | **Baru.** Nama pengguna yang menetapkan atau mengalihkan |

Ketentuannya:

| Ketentuan | Isi |
| --- | --- |
| Sumber nama | Proyeksi backend dari data pengguna dan dokter. **Bukan** kolom baru pada tabel penugasan |
| Cara mengambilnya | Satu kueri berproyeksi, mengikuti pola `recordedByName` pada bagian 7. **Dilarang** `N+1` |
| Nama yang tidak tersedia | Dikirim kosong; layar menampilkannya sebagai tanda hubung. **Dilarang** menampilkan GUID sebagai tampilan cadangan utama |
| Kewajiban frontend | **Dilarang** meminta nama per baris riwayat |

**`assignedByUserId` boleh `null` — `BE-IGD-048`, `IGD-DEC-136`, sejak `0.8.0`.** Hanya terjadi
pada baris riwayat hasil pengisian data lama ketika pelaku historisnya tidak dapat dibuktikan.
Pada baris itu `assignedByName` **selalu** berisi teks tetap `"Data historis"`, dihasilkan
proyeksi backend — **bukan** ditebak dari `RegPatientEncounter.UpdateBy`/`CreateBy` yang memang
tidak terbukti berasal dari operasi penetapan dokter. Untuk setiap penugasan yang dibuat lewat
`POST /` atau `POST /{id}/handover`, kedua ruas ini **tetap wajib terisi** seperti sebelumnya —
pelakunya selalu berasal dari token pengguna terautentikasi, request tidak pernah menentukannya.

**`effectiveFrom` pada baris legacy adalah *historical fallback*.** Baris hasil `BE-IGD-048` memuat
waktu kedatangan pasien di IGD, **bukan** waktu penetapan dokter yang terbukti — waktu itu tidak
tersimpan pada data lama. Konsumen yang menjawab "siapa dokter pada waktu `T`" (`GET /active?at=`)
harus tahu bahwa untuk baris legacy jawabannya berarti *tercatat sebagai penanggung jawab sejak awal
episode*, tidak lebih. Baris legacy dikenali dari `assignedByUserId = null` dan
`assignmentReason = "Data historis - pengisian BE-IGD-048"`.

**Dampak bagi konsumen.** Sebelum `0.8.0`, `assignedByUserId` selalu berupa GUID. Konsumen yang
mengetikkannya sebagai non-nullable (misalnya `string` wajib pada TypeScript, bukan
`string | null`) harus direvisi sebelum menampilkan baris legacy, atau parsing responsnya gagal.
Baris hasil transaksi baru **tidak terdampak** — bentuknya sama seperti sebelum `0.8.0`.

*Contoh response satu baris riwayat (penugasan baru, punya pelaku):*

```json
{
  "id": "6f1c…",
  "emergencyVisitId": "a72b…",
  "doctorId": "3e90…",
  "doctorName": "dr. Budi Santoso, Sp.EM",
  "effectiveFrom": "2026-09-16T08:00:00Z",
  "effectiveTo": "2026-09-16T14:00:00Z",
  "assignedByUserId": "11a4…",
  "assignedByName": "Perawat Sari",
  "assignmentReason": "Pergantian jaga sore"
}
```

*Contoh response satu baris riwayat legacy (pelaku historis tidak dapat dibuktikan, `0.8.0`):*

```json
{
  "id": "9c2e…",
  "emergencyVisitId": "a72b…",
  "doctorId": "3e90…",
  "doctorName": "dr. Budi Santoso, Sp.EM",
  "effectiveFrom": "2026-08-01T08:00:00Z",
  "effectiveTo": null,
  "assignedByUserId": null,
  "assignedByName": "Data historis",
  "assignmentReason": "Data historis - pengisian BE-IGD-048"
}
```

*Catatan:* kedua contoh di atas **tidak** memuat `isActive`. Penugasan yang sedang berjalan
dikenali dari `effectiveTo` yang kosong — `IGD-DEC-130`.

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
`Emergency Resuscitation`, `Emergency Procedure Detail`, dan
`Emergency Disposition` mempertahankan bentuk `0.2.0`, dengan tiga pengecualian.
`Emergency Observation Detail` **tidak lagi** termasuk kelompok ini sejak `0.6.0` — lihat
bagian 7.

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

---

## 7. `Emergency Observation Detail` — pemantauan bertanda vital, baru pada `0.6.0`

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observation-details`

Bentuk endpoint **tidak bertambah dan tidak berkurang**; yang berubah adalah aturan penerimaan
dan isi response. Dasar: `IGD-DEC-122` (tanda vital ditautkan, tidak disalin), `IGD-DEC-126`
(periode tertutup menolak pemantauan baru), dan `IGD-DEC-057` butir identitas pencatat.

| Method | Path | Kegunaan | Hak akses | Kode status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pemantauan dengan penyaring `emergencyObservationId`, `patientVitalSignId`, `progressNoteId`, rentang tanggal, dan halaman | `EmergencyObservationDetail : Read` | `200`, `403` |
| `GET` | `/{id}` | Satu pemantauan | `EmergencyObservationDetail : Read` | `200`, `403`, `404` |
| `POST` | `/` | Mencatat satu putaran pemantauan | `EmergencyObservationDetail : Create` | `200`, `400`, `403`, **`409`** |
| `PUT` | `/{id}` | Mengubah pemantauan | `EmergencyObservationDetail : Update` | `200`, `400`, `403`, `404`, `409` |
| `DELETE` | `/{id}` | Menandai pemantauan terhapus | `EmergencyObservationDetail : Delete` | `200`, `403`, `404` |

### 7.1 Bentuk request `POST /` dan `PUT /{id}`

| Field | Tipe | Wajib | Aturan pada `0.6.0` |
| --- | --- | :---: | --- |
| `emergencyObservationId` | `uuid` | Ya | Periode harus ada dan **belum ditutup** — lihat validation §9 |
| `patientVitalSignId` | `uuid?` | Tidak | Bila diisi: tanda vital harus ada, masih berlaku, dan **milik pasien serta encounter yang sama** dengan kunjungan IGD periode itu |
| `progressNoteId` | `uuid?` | Tidak | Aturan lingkup yang sama dengan `patientVitalSignId` |
| `recordedAt` | `datetime` | Tidak | Kosong berarti waktu server. Boleh mundur selama periodenya masih berjalan |
| `recordedByUserId` | `uuid?` | Tidak | **USANG pada `0.6.0`.** Tetap diterima demi kompatibilitas, tetapi **diabaikan**: nilai yang disimpan selalu berasal dari pengguna yang terautentikasi |
| `clinicalConditionSummary` | `string?` | Tidak | 2000 karakter. Tempat menulis evaluasi ABCDE selama observasi (`IGD-DEC-123`) |
| `interventionSummary` | `string?` | Tidak | 2000 karakter. Tempat menulis tindakan, termasuk alat/tindakan jalan napas selama bentuk terstrukturnya belum ada (`IGD-DEC-124`) |
| `patientResponseSummary` | `string?` | Tidak | 2000 karakter |
| `fluidIntakeMl`, `urineOutputMl`, `otherOutputMl`, `bleedingEstimatedMl`, `vomitEstimatedMl` | `decimal?` | Tidak | Kosong berarti **tidak diukur**, bukan nol (`IGD-DEC-056`) |
| `notes` | `string?` | Tidak | 1000 karakter |
| `isActive` | `bool` | Tidak | Bawaan `true` |

**Tidak ditambahkan** ke request: angka tanda vital apa pun, GCS, kesadaran, oksigen, ABCDE,
obat, gambaran EKG, dan DC Shock. Seluruhnya milik domain lain — lihat bagian 7.4.

### 7.2 Bentuk response — proyeksi aditif

Seluruh field response `0.5.0` **dipertahankan**, termasuk `patientVitalSignId` yang tetap
dikirim sebagai rujukan eksplisit. Yang bertambah:

| Field | Tipe | Isi |
| --- | --- | --- |
| `recordedByName` | `string?` | Nama petugas pencatat. Kosong bila penggunanya tidak ditemukan; layar **tidak** boleh menampilkan GUID sebagai gantinya |
| `vitalSign` | objek `?` | Ringkasan tanda vital yang ditautkan. `null` bila `patientVitalSignId` kosong |

Isi objek `vitalSign` diambil **apa adanya** dari `TrxPatientVitalSign`; tidak ada nilai
turunan baru yang dihitung kontrak ini:

| Field | Tipe | Sumber |
| --- | --- | --- |
| `id` | `uuid` | `Id` |
| `observationDateTime` | `datetime` | `ObservationDateTime` |
| `bloodPressureSystolic`, `bloodPressureDiastolic` | `int?` | kolom bernama sama |
| `pulseRate` | `int?` | `PulseRate` |
| `respiratoryRate` | `int?` | `RespiratoryRate` |
| `temperature` | `decimal?` | `Temperature` |
| `oxygenSaturation` | `decimal?` | `OxygenSaturation` |
| `gcsEye`, `gcsVerbal`, `gcsMotor`, `gcsTotal` | `int?` | kolom bernama sama. `gcsTotal` dikirim apa adanya; kontrak ini **tidak** menghitungnya sendiri |
| `consciousnessStatus` | `enum` | `ConsciousnessStatus` |
| `isUsingOxygen` | `bool` | `IsUsingOxygen` |
| `oxygenSupportType` | `enum` | `OxygenSupportType` — nilai di luar daftar memakai `Other` (`IGD-DEC-125`) |
| `oxygenFlowRate` | `decimal?` | `OxygenFlowRate` |
| `oxygenSupportNote` | `string?` | `OxygenSupportNote` |
| `vitalSignStatus` | `enum` | `VitalSignStatus` |
| `isAbnormal`, `isCritical` | `bool` | kolom bernama sama |

Proyeksi ini ada supaya layar riwayat pemantauan **tidak** perlu memanggil endpoint tanda vital
satu per satu per baris. Daftar `GET /` mengirim proyeksi yang sama untuk setiap baris.

*Contoh balasan `POST /` (dipangkas):*

```json
{
  "data": {
    "id": "…",
    "emergencyObservationId": "…",
    "recordedAt": "2026-09-16T10:30:00Z",
    "recordedByUserId": "…",
    "recordedByName": "Ns. Ani Rahmawati",
    "patientVitalSignId": "…",
    "vitalSign": {
      "id": "…",
      "observationDateTime": "2026-09-16T10:28:00Z",
      "bloodPressureSystolic": 128, "bloodPressureDiastolic": 82,
      "pulseRate": 96, "respiratoryRate": 20, "temperature": 37.2,
      "oxygenSaturation": 97,
      "gcsEye": 4, "gcsVerbal": 5, "gcsMotor": 6, "gcsTotal": 15,
      "consciousnessStatus": "ComposMentis",
      "isUsingOxygen": true, "oxygenSupportType": "NasalCannula",
      "oxygenFlowRate": 3, "oxygenSupportNote": null,
      "vitalSignStatus": "Recorded", "isAbnormal": false, "isCritical": false
    },
    "clinicalConditionSummary": "Nyeri dada berkurang, akral hangat.",
    "urineOutputMl": 150
  }
}
```

### 7.3 Endpoint tanda vital yang dipakai layar observasi

Milik `ClinicalManagement`, dipakai apa adanya; **tidak ada endpoint IGD baru**.

#### Health Services / Clinical Management / Patient Vital Sign

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/clinical-management/patient-vital-signs` | Mencatat tanda vital baru dari layar pemantauan | `PatientVitalSign : Create` |
| `GET` | `/v1/health-services/clinical-management/patient-vital-signs?patientId=&encounterId=` | Memilih tanda vital yang sudah tercatat pada encounter yang sama | `PatientVitalSign : Read` |

Penyaring `patientId` **dan** `encounterId` wajib dikirim bersama oleh layar; daftar tanpa
penyaring akan menampilkan tanda vital pasien lain (`IGD-EV-117`).

### 7.4 Yang sengaja tidak masuk kontrak ini

| Hal | Pemilik sebenarnya | Alasan |
| --- | --- | --- |
| Angka tanda vital, GCS, kesadaran, oksigen | `TrxPatientVitalSign` (`ClinicalManagement`) | `IGD-DEC-122` — ditautkan, tidak disalin |
| ABCDE terstruktur | `EmgTriage` (ringkasan teks) | `IGD-DEC-123`; `IGD-GAP-027` tetap ditunda |
| Alat bantu jalan napas terstruktur | Belum ada pemilik | `IGD-DEC-124`; menunggu `IGD-OQ-089` |
| Obat dan dosis | `TrxPrescription` (Farmasi), `EmgProcedureDetail` jenis `EmergencyMedication` | PRD bagian 3 — catatan pemberian obat ditunda |
| Gambaran EKG | Tindakan/penunjang | Tidak ada requirement yang disetujui |
| DC Shock, RJP, ROSC | `EmgResuscitation` | `IGD-CAP-28`; perburukan ditangani lewat aksi Eskalasi |
| Entri susulan setelah periode ditutup | — | `IGD-OQ-090`, belum dirancang |

---

## 8. Encounter-first — **Rencana (belum tersedia)**, baru pada `0.11.0`

Disusun pass `design-business-module` 22 September 2026 dari `IGD-DEC-139`, `142`…`148`, `150`…`154`
(`approved`) dan gate [evidence/02-requirement-completeness-gate.md](../evidence/02-requirement-completeness-gate.md)
sub-slice `S1`…`S6`, `S8`. **Seluruh endpoint dan perilaku di bagian ini belum ada di kode**, kecuali yang
ditulis "sudah ada". Kelayakan dokter jaga (`S7`) **tidak** dikontrakkan — ditahan `IGD-OQ-102` dan `IGD-OQ-103`.

**Status bagian ini: `approved`** — `IGD-DEC-157`, Rizki Gunawan, 22 September 2026. Isinya terkunci hash
(manifest bagian 2); task R3.13/R3.12 merealisasikannya **tanpa** mengubah teksnya. Cacat yang ditemukan saat
implementasi dikembalikan ke perencanaan, bukan ditambal di berkas ini.

**`0.11.0` bukan aditif murni.** Lima perilaku yang sudah ada berubah (bagian 8.1).

### 8.1 Dampak kompatibilitas terhadap `0.10.0`

| # | Perubahan | Sifat | Akibat bagi pemanggil | Keputusan |
| ---: | --- | --- | --- | --- |
| 1 | `POST /patient-encounters` bertipe Emergency ditolak `409` bila episode pasien masih terbuka | **Memutus perilaku** | Pendaftaran ganda tanpa alasan tidak lagi menghasilkan encounter | `IGD-DEC-139`, `146` |
| 2 | `POST /patient-encounters` bertipe Emergency **tidak** membuat antrean | **Perilaku berubah** | Pasien IGD tidak muncul di antrean dokter/perawat walau unit/klinik `IsQueueRequired` | `IGD-DEC-144` |
| 3 | `PATCH /patient-encounters/{id}/status` menolak encounter Emergency | **Memutus** | Penutupan encounter IGD lewat jalur umum ditolak `409` | `IGD-DEC-153` |
| 4 | `PATCH /patient-encounters/{id}/cancel` menolak encounter Emergency yang sudah punya kunjungan | **Memutus** | Batal sesudah kunjungan lahir harus lewat kunjungan | `IGD-DEC-153` |
| 5 | `PUT /emergency-visits/{id}` menolak perubahan `patientId`, `encounterId`, dan `arrivalDateTime` | **Memutus** — nol layar memakai `PUT` (`IGD-FACT-023`) | Identitas dikunci; waktu tiba lewat `PATCH /{id}/arrival-time` | `IGD-DEC-154`, `147`, `152` |
| 6 | `PATCH /emergency-visits/{id}/complete` dan `…/visit-status` ke `Cancelled` ikut menutup encounter | Efek samping baru | Encounter tidak lagi tertinggal terbuka | `IGD-DEC-139` butir 5 |
| 7 | `POST /emergency-visits` memakai rumus episode klausa A+B dan kunci per pasien | **Perilaku berubah** | Kunjungan untuk pasien yang punya encounter Emergency terbuka lain ditolak `409` | `IGD-DEC-139`, `146` |
| 8 | `GET /emergency-visits/active-episode` bertambah ruas `encounter` | Aditif | — | `IGD-DEC-139` |
| 9 | Ruas request `duplicateEpisodeOverrideReason` pada `POST /patient-encounters` | Aditif | Opsional | `IGD-DEC-145` |
| 10 | Endpoint baru `triage-queue`, `start-triage`, `no-show`, `{id}/arrival-time`, grup rekonsiliasi | Aditif | — | Bagian 8.3–8.4 |

**Urutan rilis wajib.** Nomor 1, 3, 4, 7 (penjaga) dirilis **sebelum** layar pendaftaran berhenti membuat
kunjungan (`FE-IGD-036`). Membalik urutannya membuka pendaftaran ganda (roadmap frontend R3.12).

### 8.2 Titik sentuh Registrasi — grup `Patient Encounter`

`[Tags("Patient Encounter")]` · Base URL `api/v1/health-services/registration-management/patient-encounters`

Grup ini **milik Registration Management**. Kontrak di bawah hanya mengatur perilaku **khusus
`encounterType = Emergency`**, di bawah izin remediasi teknis `IGD-DEC-135` — **bukan** pemindahan
kepemilikan. Tipe lain **tidak berubah sedikit pun**.

| Method | Path | Perubahan khusus Emergency | Hak akses (tidak berubah) | Kode status |
| --- | --- | --- | --- | --- |
| `POST` | `/`, `/admin`, `/kiosk` | (a) Kunci per pasien diambil **sebelum** kunci penomoran encounter; (b) rumus episode terbuka dijalankan; (c) episode terbuka + alasan kosong → `409`; alasan terisi → encounter dibuat dan catatan override ditulis dalam transaksi yang sama; (d) **tidak** membuat `TrxQueue` | `PatientEncounter : Create` | `200`, `400`, `401`, `403`, **`409` baru** |
| `PATCH` | `/{id}/status`, `/admin/{id}/status` | Encounter Emergency **ditolak** — status akhirnya hanya lewat aksi IGD | `PatientEncounter : Update` | **`409` baru** |
| `PATCH` | `/{id}/cancel`, `/admin/{id}/cancel` | Encounter Emergency yang **sudah punya kunjungan** ditolak; yang belum punya kunjungan tetap boleh (salah daftar/duplikat) | `PatientEncounter : Update` | **`409` baru** |

**Ruas request baru pada `POST /` (aditif).**

| Ruas | Tipe | Wajib | Aturan |
| --- | --- | :-: | --- |
| `duplicateEpisodeOverrideReason` | `string?` | Tidak | Di-*trim*; maksimal 500 karakter; hanya dibaca bila `encounterType = Emergency` dan episode pasien terbuka; **diabaikan** untuk tipe lain. String kosong/spasi = tidak ada alasan |

**Balasan `409` episode terbuka.** Pesan mengikuti validation §10.1 aturan 3 (menyebut nomor kunjungan bila
sudah ada, atau nomor encounter dan keterangan *"Menunggu Triage"*). Data terstruktur episode tersebut
diambil layar lewat pra-cek `GET /emergency-visits/active-episode` (bagian 8.3.6), bukan dari badan `409`.

### 8.3 `Emergency Visit` — endpoint baru dan yang berubah

`[Tags("Emergency Visit")]` · Base URL `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses | Kode status | Keadaan |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/triage-queue` | Daftar *Menunggu Triage* terpadu: encounter tanpa kunjungan + kunjungan | `EmergencyVisit : Read` | `200`, `400`, `401`, `403` | **Rencana (belum tersedia)** |
| `POST` | `/start-triage` | Melahirkan kunjungan dari encounter — mode Mulai Triage atau Tangani Segera | `EmergencyVisit : Create` | `201`, `200`, `400`, `401`, `403`, `404`, `409` | **Rencana (belum tersedia)** |
| `POST` | `/no-show` | Menandai pasien pergi sebelum ditriage | `EmergencyVisit : NoShow` | `200`, `400`, `401`, `403`, `404`, `409` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/arrival-time` | Mengonfirmasi atau mengoreksi waktu tiba | `EmergencyVisit : Update` | `200`, `400`, `401`, `403`, `404`, `409` | **Rencana (belum tersedia)** |
| `GET` | `/active-episode?patientId=` | Pra-cek — **bertambah** ruas `encounter` | `EmergencyVisit : Create` | Tidak berubah | Sudah ada; **perluasan Rencana** |
| `POST` | `/` | Jalur lama — kini kunci per pasien + klausa A+B | `EmergencyVisit : Create` | Tidak berubah | Sudah ada; **perilaku Rencana** |
| `PUT` | `/{id}` | Menolak perubahan `patientId`, `encounterId`, `arrivalDateTime` | `EmergencyVisit : Update` | + `409` | Sudah ada; **perilaku Rencana** |
| `PATCH` | `/{id}/visit-status` | Ke `Cancelled` → encounter ikut dibatalkan | `EmergencyVisit : Update` | Tidak berubah | Sudah ada; **efek samping Rencana** |
| `PATCH` | `/{id}/complete` | Encounter ikut `Completed` + catatan klinis terbuka dikunci | `EmergencyVisit : Update` | Tidak berubah | Sudah ada; **efek samping Rencana** |
| `DELETE` | `/{id}` | **Tidak** menutup encounter (hapus lunak bukan peristiwa klinis) | `EmergencyVisit : Delete` | Tidak berubah | Sudah ada; ditegaskan |

#### 8.3.1 `GET /triage-queue`

**Query.**

| Parameter | Tipe | Wajib | Bawaan | Aturan |
| --- | --- | :-: | --- | --- |
| `page` | `int` | Tidak | `1` | ≥ 1 |
| `pageSize` | `int` | Tidak | `20` | 1–100 |
| `search` | `string?` | Tidak | — | Nama pasien, nomor RM, nomor encounter, nomor kunjungan |
| `queueStatus` | `string?` | Tidak | — | `WaitingForTriage` untuk baris tanpa kunjungan; atau nilai `EmergencyVisitStatus` untuk baris kunjungan |

**Baris respons** (`EmergencyTriageQueueRowResponse`, di dalam `PagedResult<T>`).

| Ruas | Tipe | Isi |
| --- | --- | --- |
| `rowKey` | `string` | Kunci stabil baris untuk layar (`enc:{encounterId}` atau `visit:{visitId}`) |
| `encounterId` | `uuid?` | Kosong hanya untuk kunjungan lama tanpa encounter |
| `emergencyVisitId` | `uuid?` | Kosong bila kunjungan belum lahir |
| `patientId` | `uuid?` | |
| `patientName` | `string?` | Untuk pasien tanpa identitas: nama rekam pengganti apa adanya (`IGD-DEC-151`, `IGD-DEC-007`) |
| `medicalRecordNumber` | `string?` | |
| `isUnknownPatient` | `bool` | Dari kunjungan; `false` bila kunjungan belum lahir |
| `temporaryPatientAlias` | `string?` | Dari kunjungan |
| `encounterNumber` | `string?` | |
| `emergencyVisitNumber` | `string?` | |
| `queueStatus` | `string` | `WaitingForTriage` untuk baris tanpa kunjungan; nama `VisitStatus` untuk baris kunjungan |
| `visitStatus` | `int?` | Kosong bila kunjungan belum lahir |
| `registeredAt` | `datetime` | Waktu terdaftar — **ditampilkan berlabel "Terdaftar", tidak pernah sebagai waktu tiba** |
| `arrivalDateTime` | `datetime?` | Hanya untuk baris kunjungan |
| `availableActions` | `string[]` | Baris tanpa kunjungan: `StartTriage`, `ImmediateCare`, `NoShow`. Baris kunjungan `Arrived`/`WaitingForTriage`: `FillTriage`, `ImmediateCare` (`IGD-DEC-128`) |

**Aturan.** Satu episode satu baris — encounter yang sudah punya kunjungan hanya tampil sebagai baris
kunjungan. Encounter yang sudah berakhir (lima tanda, validation §10.1 aturan 1) dan encounter bertipe
lain tidak tampil. Halaman dihitung di basis data atas gabungan kedua jenis baris. Asal baris **tidak**
diekspos sebagai ruas yang wajib dibaca layar.

#### 8.3.2 `POST /start-triage`

**Request** (`StartEmergencyVisitRequest`).

| Ruas | Tipe | Wajib | Bawaan | Aturan |
| --- | --- | :-: | --- | --- |
| `encounterId` | `uuid` | **Ya** | — | Encounter Emergency yang belum berakhir |
| `mode` | `string` | **Ya** | — | `Triage` (Mulai Triage) atau `ImmediateCare` (Tangani Segera) |
| `arrivalDateTime` | `datetime?` | Wajib bila `mode = Triage` | — | Tidak di masa depan; untuk `ImmediateCare` **diabaikan** dan diisi `RegisteredAt` sebagai fallback |
| `arrivalModeId` | `uuid?` | Tidak | — | Master cara datang IGD |
| `caseTypeId` | `uuid?` | Tidak | — | Master jenis kasus IGD |
| `chiefComplaint` | `string?` | Tidak | Salinan `RegPatientEncounter.ChiefComplaint` | Maks. mengikuti kolom kunjungan |
| `isUnknownPatient` | `bool` | Tidak | `false` | Menandai rekam pengganti (`IGD-DEC-151`) |
| `temporaryPatientAlias` | `string?` | Wajib bila `isUnknownPatient` | — | Aturan yang sudah ada (validation §1 aturan 3) |

**Contoh — Mulai Triage.**

```json
{
  "encounterId": "3f2a1c9e-…",
  "mode": "Triage",
  "arrivalDateTime": "2026-09-22T02:31:00Z"
}
```

**Contoh — Tangani Segera (nol ketikan).**

```json
{ "encounterId": "3f2a1c9e-…", "mode": "ImmediateCare" }
```

**Respons** — `EmergencyVisitResponse` yang sudah ada, ditambah ruas waktu tiba bagian 8.3.4.

| Keadaan | Kode | Hasil |
| --- | --- | --- |
| Kunjungan belum ada | `201` | Lahir `WaitingForTriage` (`Triage`) atau `InTreatment` + `TreatmentStartedAt` server (`ImmediateCare`) |
| Kunjungan sudah ada, `mode = Triage` | `200` | Kunjungan yang sama, status **tidak** diubah (tidak pernah mundur) |
| Kunjungan sudah ada `WaitingForTriage`/`Arrived`, `mode = ImmediateCare` | `200` | Diteruskan ke `InTreatment` lewat penjaga transisi |
| Kunjungan sudah ada di status lain, `mode = ImmediateCare` | `200` | Kunjungan yang sama, status tidak diubah |
| Encounter tidak ada | `404` | |
| Encounter bukan Emergency | `400` | |
| Encounter sudah berakhir | `409` | |
| Satu-satunya kunjungan encounter itu pernah dihapus lunak (K4) | `409` | Unique index tanpa filter (`IGD-EV-143` butir 7) |
| Pasien punya kunjungan lain yang belum berakhir (klausa B, encounter berbeda) | `409` | Pesan validation §1 aturan 4 |

**Idempoten.** Dua panggilan serentak → tepat satu kunjungan; insert kedua bentrok unique index
`EmgVisit.EncounterId`, ditangkap, lalu dijawab sesuai tabel di atas.

#### 8.3.3 `POST /no-show`

**Request** (`MarkEmergencyEncounterNoShowRequest`).

| Ruas | Tipe | Wajib | Aturan |
| --- | --- | :-: | --- |
| `encounterId` | `uuid` | **Ya** | Encounter Emergency tanpa kunjungan, belum berakhir |
| `reason` | `string` | **Ya** | Di-*trim*; 1–500 karakter |

**Respons `200`.** `{ "encounterId": "…", "encounterStatus": 11, "noShowAt": "…", "noShowByName": "…", "noShowReason": "…" }`.
Pelaku dari token, waktu dari server. **Final** — tidak ada endpoint pembatalannya (`IGD-DEC-142`).

| Keadaan | Kode |
| --- | --- |
| Alasan kosong, atau encounter bukan Emergency | `400` |
| Encounter tidak ada | `404` |
| Encounter sudah punya kunjungan, atau sudah berakhir | `409` |

#### 8.3.4 `PATCH /{id}/arrival-time`

**Request** (`UpdateEmergencyArrivalTimeRequest`): `arrivalDateTime` (`datetime`, wajib).

Menyimpan nilai, lalu menandai **dikonfirmasi**: `arrivalTimeSource = Confirmed`, `arrivalConfirmedByUserId`
dari token, `arrivalConfirmedAt` waktu server. Mengirim nilai yang sama dengan nilai sekarang = konfirmasi
tanpa koreksi.

| Ruas baru pada `EmergencyVisitResponse` | Tipe | Isi |
| --- | --- | --- |
| `arrivalTimeSource` | `int` | `0` = data lama belum dikonfirmasi; `1` = fallback `RegisteredAt`; `2` = dikonfirmasi perawat |
| `arrivalConfirmedByName` | `string?` | |
| `arrivalConfirmedAt` | `datetime?` | |

| Keadaan | Kode |
| --- | --- |
| Nilai di masa depan | `400` |
| Nilai lebih lambat dari peristiwa klinis pertama (mulai triage, mulai penanganan, penugasan dokter pertama) | `409` — pesan menyebut peristiwanya (`IGD-DEC-152`) |
| Kunjungan tidak ada | `404` |

#### 8.3.5 Perubahan pada `PUT /{id}`

Permintaan yang **mengubah** `patientId`, `encounterId`, atau `arrivalDateTime` dari nilai tersimpan ditolak
`409`. Mengirim nilai yang sama diterima. Ruas lain tidak berubah perilakunya.

#### 8.3.6 Perluasan `GET /active-episode`

| Ruas baru | Tipe | Isi |
| --- | --- | --- |
| `encounter` | objek? | Terisi bila episode terbuka lewat klausa A dan kunjungan belum lahir: `{ "id", "encounterNumber", "registeredAt" }` |

`hasActiveEpisode` kini benar untuk klausa A **atau** B. `visit` tetap terisi bila kunjungan ada.

#### 8.3.7 Efek samping penutupan encounter

| Aksi | Yang terjadi pada encounter, pada penyimpanan yang sama |
| --- | --- |
| `PATCH /{id}/complete` | `EncounterStatus = Completed`; `CompletedAt` server bila kosong; catatan klinis belum ditandatangani dikunci (`RM-DEC-003`) |
| `PATCH /{id}/visit-status` → `Cancelled` | `EncounterStatus = Cancelled`; `IsCancel`; `IsActive = false`; `CancelledAt`/`By`; `CancelReason` = catatan permintaan atau *"Kunjungan IGD dibatalkan"* |
| Encounter sudah berakhir | Tidak ada yang ditulis |
| Encounter bertipe `Outpatient` tertaut (masa transisi) | Ikut ditutup (`IGD-DEC-148` TK-2) |

### 8.4 `Emergency Encounter Reconciliation` — grup baru

`[Tags("Emergency Encounter Reconciliation")]` · Base URL
`api/v1/health-services/emergency-installation-management/emergency-encounter-reconciliations` ·
**Rencana (belum tersedia)**

| Method | Path | Kegunaan | Hak akses | Kode status |
| --- | --- | --- | --- | --- |
| `GET` | `/preview` | Jumlah K1, K1-Outpatient, K2, K3, K4 + daftar baris K1/K1-Outpatient berhalaman; **tidak menulis** | `EmergencyEncounterReconciliation : Read` | `200`, `401`, `403` |
| `POST` | `/runs` | Menutup baris K1 dan K1-Outpatient yang lolos syarat | `EmergencyEncounterReconciliation : Process` | `201`, `400`, `401`, `403`, `409` |
| `GET` | `/runs` | Riwayat run | `EmergencyEncounterReconciliation : Read` | `200`, `401`, `403` |
| `GET` | `/runs/{id}` | Satu run beserta barisnya | `EmergencyEncounterReconciliation : Read` | `200`, `401`, `403`, `404` |
| `POST` | `/runs/{id}/reverse` | Membalik satu run berpenjaga | `EmergencyEncounterReconciliation : Reverse` | `200`, `400`, `401`, `403`, `404`, `409` |

**Request `POST /runs`:** `reason` (`string`, wajib, 1–500) dan `expectedCount` (`int`, wajib) — jumlah baris
K1 + K1-Outpatient yang dilihat petugas pada preview. Bila jumlah saat eksekusi berbeda → `409` *"Data
berubah sejak pratinjau; muat ulang pratinjau."* Ini menahan eksekusi atas data yang sudah basi.

**Request `POST /runs/{id}/reverse`:** `reason` (`string`, wajib, 1–500). Hanya baris yang encounter-nya
**masih** bernilai hasil run yang dikembalikan; baris yang sudah berubah sesudahnya dilewati dan
dilaporkan. Run yang sudah dibalik → `409`.

**Aturan eksekusi** (`IGD-DEC-148`): nol update massal — setiap baris dievaluasi syaratnya; status
mengikuti kunjungan; `CompletedAt` hanya dari `EmgVisit.VisitCompletedAt`, bila kosong dibiarkan kosong;
K2/K3/K4 hanya muncul di preview.

### 8.5 `Emergency Doctor Assignment` — tidak berubah pada `0.11.0`

Endpoint kelayakan dokter dan ruas override **belum dikontrakkan** (`IGD-OQ-102`, `IGD-OQ-103`). Perilaku
bagian 3 tetap berlaku.
