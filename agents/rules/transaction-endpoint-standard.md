# Standar Endpoint Transaksi

| Field | Nilai |
| --- | --- |
| Status | Baseline wajib untuk capability transaksi |
| Sumber asal | Diturunkan dari source, bukan dari PDF. Tidak ada dokumen standar transaksi di `docs/update-skilss/`, `agents/rules/`, maupun `agents/rules/engineering/` |
| Diverifikasi terhadap | Seluruh folder `Controllers/` non-`MasterData` pada `Areas/` |
| Pendamping | [master-data-endpoint-standard.md](master-data-endpoint-standard.md) |
| Presedensi | `AGENTS.md` > `agents/rules/engineering/` > `agents/rules/` > dokumen ini. Bila source repository berbeda, source yang berlaku dan selisihnya dilaporkan |

Master data menjawab "data apa yang tersedia". Transaksi menjawab "apa yang sedang terjadi pada
satu kejadian nyata". Keduanya butuh bentuk endpoint yang berbeda, dan mencampurnya adalah
sumber kesalahan yang paling sering pada modul rumah sakit.

---

## 1. Apa yang berbeda dari master data

| Aspek | Master data | Transaksi |
| --- | --- | --- |
| Perubahan utama | Menyunting field | Berpindah status karena ada kejadian |
| Cara mengakhiri | `DELETE /{id}` soft delete | Dibatalkan lewat aksi `cancel`, bukan dihapus |
| Toggle status | `PATCH /{id}/status` generik | Aksi bernama: `submit`, `approve`, `reject`, `complete`, `cancel` |
| Sumber dropdown | Ya, lewat `GET /options` | Tidak. Transaksi bukan data pilihan |
| Riwayat | Cukup jejak `UpdateBy` terakhir | Wajib dapat ditelusuri per perpindahan status |
| Siapa boleh apa | Hak akses per operasi CRUD | Hak akses ditambah kelayakan status dan kewenangan aktor |

Tiga endpoint baseline master data **tidak** dibuat untuk transaksi:

- `GET /options` — transaksi tidak menjadi isi dropdown. Bila sebuah layar butuh memilih
  transaksi, yang dipilih biasanya master data atau pasien, bukan transaksinya.
- `PATCH /{id}/status` generik — status transaksi berpindah karena kejadian bernama, bukan
  karena seseorang menyetel nilai. Lihat bagian 3.
- `DELETE /{id}` — transaksi yang sudah terjadi tidak dihapus. Lihat bagian 6.

---

## 2. Empat arketipe transaksi

Tentukan arketipe **sebelum** merancang route. Salah arketipe berarti seluruh permukaan salah.

### 2.1 Aggregate ber-lifecycle

Transaksi utama yang punya identitas sendiri, dibuat sekali, lalu berpindah status sampai
selesai. Contoh: `InpatientEpisode`, `PatientEncounter`, `LabOrder`, `OperatingRoomCase`,
`Prescription`.

| Method | Path | Kegunaan |
| --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi filter dan form, sama seperti master data |
| `GET` | `/summary` | Ringkasan jumlah per status |
| `GET` | `/` | Daftar transaksi dengan filter, search, sort, pagination |
| `GET` | `/{id}` | Detail satu transaksi beserta status dan aksi yang tersedia |
| `POST` | `/` | Membuka transaksi baru |
| `PUT` | `/{id}` | Menyunting isi transaksi selama statusnya masih boleh disunting |
| `POST` | `/{id}/<aksi>` | Satu endpoint per perpindahan status |
| `GET` | `/{id}/status-history` | Riwayat perpindahan status |

Contoh nyata riwayat status: `InpatientEpisodeController.cs` baris 502,
`GET /{id}/status-history`.

### 2.2 Worklist dan antrean

Daftar kerja yang isinya lahir dari transaksi lain, bukan dibuat manual. Contoh:
`DoctorQueue`, `NurseStationQueue`, `QueueDisplayRuntime`.

| Method | Path | Kegunaan |
| --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi filter layar antrean |
| `GET` | `/summary` | Jumlah menunggu, dipanggil, selesai |
| `GET` | `/` | Isi antrean |
| `POST` | `/{id}/<aksi>` | Aksi per item: `call`, `skip`, `no-show`, `requeue` |

**Tidak ada** `POST /`, `PUT /{id}`, maupun `DELETE /{id}`. Item antrean tidak dibuat dan tidak
dihapus lewat controller ini — ia muncul karena transaksi induknya. Membuka endpoint create di
sini berarti membuat dua sumber kebenaran untuk antrean yang sama.

Contoh nyata: `DoctorQueueController` menyediakan `call`, `start-consultation`,
`finish-consultation`, `skip`, `no-show`, dan `requeue`, tanpa satu pun endpoint create.

### 2.3 Sub-proses yang di-scope ke induknya

Proses yang tidak punya arti tanpa transaksi induk. Contoh: `PrescriptionReview`,
`PrescriptionPreparation`, `InpatientDischarge`, `OperatingRoomPreparation`,
`OperatingRoomRecovery`.

Route-nya diawali identitas induk, bukan identitas dirinya sendiri:

```http
GET  /by-prescription/{prescriptionId}
POST /by-prescription/{prescriptionId}/start
GET  /{episodeId}/clearance
POST /{episodeId}/clearance/{itemId}/mark
```

Dokumen hasil proses memakai pasangan `GET` dan `PUT` pada path tetap, bukan `POST` berulang:

```http
GET /{episodeId}/summary
PUT /{episodeId}/summary
GET /operation-record
PUT /operation-record
```

Alasannya, dokumen itu hanya ada satu per induk. `PUT` menegaskan bahwa penyimpanan berulang
menimpa dokumen yang sama, bukan membuat dokumen kedua.

### 2.4 Monitoring dan laporan read-only

Hanya `GET`, tanpa satu pun endpoint yang mengubah data. Contoh: `OperatingRoomReport` dengan
`operations`, `utilization`, dan `materials`; `InpatientMonitoring` dengan `isolation-mismatch`,
`pending-closures`, `closures-without-financial-clearance`, `unassigned-nurse-episodes`, dan
`bed-drift`.

Nama path menyebutkan **kondisi yang dipantau**, bukan nama tabel. `pending-closures` langsung
memberi tahu pembaca apa yang dicari; `episodes-query-3` tidak.

---

## 3. Aturan verb untuk aksi

Ini aturan yang paling sering dilanggar, jadi ditulis tegas.

| Verb | Dipakai untuk | Contoh |
| --- | --- | --- |
| `POST /{id}/<aksi>` | **Perintah** yang memindahkan status dan menghasilkan kejadian | `cancel`, `submit`, `approve`, `reject`, `complete`, `close`, `verify`, `execute` |
| `PATCH /{id}/<field>` | **Menyunting atribut** tanpa memindahkan status | `status`, `doctor`, `isolation-requirement` |
| `PUT /{id}` | Menyunting keseluruhan isi transaksi | Edit transaksi yang masih boleh disunting |
| `PUT /{induk}/<dokumen>` | Menyimpan dokumen tunggal milik induk | `operation-record`, `anesthesia-record`, `summary` |
| `PUT /{id}/<aksi>` | **Dilarang** | — |

Cara memutuskan dalam satu kalimat: bila hasilnya layak dicatat sebagai baris riwayat status,
itu perintah, maka `POST`. Bila hasilnya hanya mengubah isi satu kolom, itu suntingan atribut,
maka `PATCH`.

Contoh penerapan:

> Perawat membatalkan episode rawat inap. Kejadian ini masuk riwayat status, punya alasan
> pembatalan, dan tidak bisa diulang. Maka `POST /{id}/cancel`, bukan `PATCH /{id}/cancel`.
>
> Petugas mengganti dokter penanggung jawab episode yang sedang berjalan. Statusnya tetap
> berjalan; yang berubah hanya satu kolom. Maka `PATCH /{id}/doctor`.

Nama aksi memakai kata kerja, huruf kecil, dipisah tanda hubung: `start-consultation`,
`request-recollection`, `record-departure`. Jangan memakai `do`, `process`, `handle`, atau
`update` sebagai nama aksi — ketiganya tidak memberi tahu apa yang sebenarnya terjadi.

---

## 4. Yang tetap diwarisi dari master data

Permukaan baca transaksi sengaja dibuat sama supaya frontend memakai kerangka halaman yang
sama. Ikuti [master-data-endpoint-standard.md](master-data-endpoint-standard.md) bagian 2.1
sampai 2.3 apa adanya untuk:

- `GET /filters/metadata` — termasuk `DefaultFilter`, `SortOptions`, `PageSizeOptions`,
  `QueryParameters`, dan metadata form.
- `GET /summary` — untuk transaksi, isinya adalah jumlah per status ditambah pencacah kondisi
  yang perlu perhatian. Contoh: total episode berjalan, menunggu pemulangan, dan melewati batas
  waktu.
- `GET /` — `PagedResult<T>` dengan `pageNumber`, `pageSize`, `totalData`, `totalPage`, `items`.

Bungkus `ApiResponse<T>`, route bertversi, `[Authorize]`, `[AccessController]`,
`[AccessAction]`, dan `[AccessPermission]` berlaku sama persis.

---

## 5. Kewenangan transisi — yang tidak ada di master data

### 5.1 Backend memutuskan aksi apa yang boleh

Response detail transaksi menyertakan daftar aksi yang sedang boleh dijalankan, supaya frontend
tidak menebak dari nilai status.

Pola yang sudah ada di source memakai field `AvailableActions` berisi daftar nama aksi:

```csharp
if (editable) response.AvailableActions.AddRange(new[] { "Update", "UploadEvidence", "Submit", "Cancel", "Delete" });
else if (!terminal) response.AvailableActions.Add("Cancel");
if (response.CanApply) response.AvailableActions.Add("Apply");
```

Bukti: `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceCorrectionService.cs`
baris 1322-1324, dan DTO `AvailableActions` pada beberapa modul Leave Management.

Yang wajib dipahami: `AvailableActions` adalah **bantuan tampilan**, bukan pengaman. Setiap
endpoint aksi tetap memeriksa ulang kelayakan status dan kewenangan aktor di backend. Tombol
yang disembunyikan frontend bukan authorization.

### 5.2 Tolak transisi yang tidak sah dengan pesan yang terbaca

| Kode | Kapan dipakai | Contoh pesan |
| --- | --- | --- |
| `400` | Aksi tidak sah untuk status saat ini | "Episode sudah ditutup, jadi tidak bisa dibatalkan." |
| `403` | Aktor tidak berwenang atas aksi itu | "Hanya dokter penanggung jawab yang boleh menyetujui." |
| `404` | Transaksi tidak ditemukan | "Data tidak ditemukan." |
| `409` | Transaksi diubah pihak lain lebih dulu | "Data sudah diperbarui petugas lain. Muat ulang lalu coba lagi." |

Jangan mengembalikan `200` dengan pesan gagal. Kegagalan transisi adalah kegagalan, bukan
sukses yang berisi keluhan.

### 5.3 Aksi kritis menerima kunci idempotency

Aksi yang berdampak keuangan, klinis, atau tidak bisa dibatalkan wajib menerima
`IdempotencyKey` opsional pada request, supaya klik ganda atau retry jaringan tidak
menghasilkan dua kejadian.

> **Contoh:** perawat menekan Simpan pada penutupan episode, jaringan putus, lalu ia menekan
> lagi. Dengan kunci yang sama, permintaan kedua mengembalikan hasil permintaan pertama, dan
> episode tetap tertutup satu kali.

Bukti pola: `AttendanceCorrectionDtos.cs` baris 246 dan 257, serta pemakaiannya di
`AttendanceCorrectionService.cs`.

### 5.4 Setiap perpindahan status meninggalkan jejak

Simpan siapa, kapan, dari status apa ke status apa, dan alasannya bila aksi itu menuntut
alasan. Sediakan `GET /{id}/status-history` pada transaksi ber-lifecycle.

Alasan wajib diisi minimal untuk `cancel` dan `reject`. Tanpa alasan, audit tidak bisa
menjelaskan mengapa sebuah tindakan medis dibatalkan.

---

## 6. Pembatalan, bukan penghapusan

Transaksi mencatat kejadian yang benar-benar terjadi. Menghapusnya berarti menghapus fakta.

| Keadaan | Yang benar |
| --- | --- |
| Transaksi keliru dan belum berjalan | `POST /{id}/cancel` beserta alasan |
| Transaksi sudah berjalan dan perlu dikoreksi | Sesi koreksi yang tercatat, bukan menimpa data lama |
| Transaksi terlanjur salah dan berdampak keuangan | Aksi pembalik (`reverse`) yang membuat catatan penyeimbang |

Contoh koreksi terkendali pada source: `InpatientEpisodeController` menyediakan
`POST /{id}/correction-sessions` dan `PATCH /{id}/correction-sessions/{sessionId}/close`,
sehingga koreksi punya awal, akhir, dan pemiliknya.

`DELETE` pada transaksi hanya dibenarkan untuk data yang **belum pernah berlaku**, misalnya
draft yang belum pernah disubmit, dan tetap berupa soft delete.

---

## 7. Drift yang ada di source — laporkan, jangan tiru

Survei seluruh `Controllers/` non-`MasterData` menemukan pemakaian yang saling bertentangan
untuk konsep yang sama:

| Temuan | Angka | Sikap |
| --- | --- | --- |
| `POST /{id}/<aksi>` | 186 | Sesuai standar bagian 3 |
| `PATCH /{id}/<aksi>` untuk perintah | Sebagian dari 150 | Menyimpang bila itu perintah; `PATCH` untuk atribut tetap benar |
| `PUT /{id}/<aksi>` | 9 | Menyimpang. Contoh: `LabOrder` memakai `PUT /{id}/complete`, `/hold`, `/resume`, `/cancel` |
| `DELETE` pada transaksi | `Prescription`, `PrescriptionItem`, `PatientEncounter`, `KioskScanSession` | Periksa apakah seharusnya `cancel` |
| `GET /options` pada transaksi | `PatientEncounter`, `Prescription`, `KioskScanSession` | Periksa apakah konsumennya memang butuh transaksi sebagai dropdown |

Tiga verb untuk konsep yang sama terlihat jelas bila disandingkan: `LabOrder` memakai
`PUT /{id}/cancel`, `InpatientEpisode` memakai `PATCH /{id}/cancel`, dan `LabSpecimen` memakai
`POST /{id}/cancel`.

Sikap yang diambil saat mengerjakan task:

- **Kode baru** mengikuti standar ini tanpa kecuali.
- **Menyentuh kode lama** tidak dengan sendirinya memberi wewenang merapikan bentuknya.
  Mengubah verb sebuah endpoint yang sudah dipakai frontend adalah perubahan yang merusak
  kompatibilitas, dan itu menuntut wewenang eksplisit beserta penilaian dampak konsumen sesuai
  `API_RULES.md`.
- Selisih yang ditemukan dicatat pada laporan task sebagai temuan, bukan diperbaiki diam-diam.

---

## 8. Checklist sebelum task transaksi dianggap selesai

1. Arketipe sudah ditentukan dan ditulis di laporan: aggregate ber-lifecycle, worklist,
   sub-proses ter-scope induk, atau read-only.
2. Tidak ada `GET /options`, `PATCH /{id}/status` generik, atau `DELETE /{id}` yang dipasang
   hanya karena meniru master data.
3. Setiap perpindahan status punya endpoint `POST /{id}/<aksi>` bernama kata kerja yang jelas.
4. Suntingan atribut memakai `PATCH /{id}/<field>`, dan tidak ada `PUT /{id}/<aksi>`.
5. Detail transaksi menyertakan daftar aksi yang sedang boleh dijalankan.
6. Setiap endpoint aksi memeriksa ulang kelayakan status dan kewenangan aktor di backend.
7. Transisi tidak sah ditolak dengan `400`, `403`, `404`, atau `409` beserta pesan yang terbaca
   pengguna — bukan `200` berisi pesan gagal.
8. Aksi kritis menerima `IdempotencyKey`, dan pengiriman ganda tidak menghasilkan dua kejadian.
9. Riwayat status tersimpan, dan `cancel` serta `reject` menyimpan alasan.
10. Pembatalan memakai aksi `cancel`; `DELETE` hanya untuk draft yang belum pernah berlaku.
11. Drift yang ditemukan pada endpoint lama dicatat di laporan task, bukan diperbaiki tanpa
    wewenang.
