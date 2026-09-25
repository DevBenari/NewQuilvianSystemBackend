# Bank Darah — API Contract

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version **`v5` — `approved`** (`Sukmagp` 2026-09-19; `v4` kini `superseded`). **Riwayat:** `v5` `draft` 18 September 2026. Arah disetujui `Sukmagp` 2026-09-18 (`DEC-BD-055`..`058`). **Riwayat:** `v4` — `approved` `Sukmagp` 2026-09-03 |
| `last_changed_in` | **`v5`** — grup Blood Order saja (bagian "Amendment `v5`" di bawah tabel grup itu). Grup lain tidak berubah. **23 September 2026:** bagian `D5` ditambahkan ke amandemen yang sama — penyaring rentang tanggal `startDate`/`endDate` pada `GET /` (`DEC-BD-059`, `DEC-BD-060`, task `BE-BD-019`). Aditif penuh; **nomor set kontrak tidak dinaikkan**, karena tidak ada satu pun klien lama yang rusak. **24 September 2026:** bagian `D6` ditambahkan pada grup Blood Unit — penyaring `inactiveLocation` pada `GET /` (keputusan pemilik `Sukmagp`, task `BE-BD-020`). Aditif penuh; nomor set kontrak tidak dinaikkan. **25 September 2026:** bagian `D7` ditambahkan pada grup Blood Unit — proyeksi `issuanceGate` dan `emergencyBypass` pada `BloodUnitDetailDto` (keputusan pemilik `Sukmagp` `R1`–`R5`, task `BE-BD-021`). Aditif penuh; nomor set kontrak tidak dinaikkan. **Riwayat:** `v4` |
| Owner | Pemilik arsitektur backend (bentuk kontrak) · pemilik proses BDRS (perilaku) |
| `approved_by` / `approved_at` | `Sukmagp` / `2026-09-19` (`v5`). **Riwayat:** `Sukmagp` / `2026-09-03` (`v4`) |
| Sumber | `02-backend-architecture.md` (controller) · `contracts/state-transition-matrix.md` · `contracts/validation-matrix.md` |

**Seluruh endpoint di bawah berstatus `Rencana (belum tersedia)`** — belum ada di kode. Route & grup
Swagger mengikuti pola `LabOrderController` (`BD-CAP-014`); jangan menyimpulkan route dari URL frontend.
Respons dibungkus `ApiResponse<T>`; daftar memakai `PagedResult<T>` (`BD-CAP-012`).

Kolom **Hak akses** di sini adalah **satu-satunya** tempat pemetaan endpoint→hak akses hidup;
`permission-audit-matrix.md` tidak mendaftar ulang endpoint. Nilai ditulis `Resource : Action`, setara
`[AccessPermission("Resource", "Action")]`.

Kode status yang berlaku untuk seluruh grup: `200` berhasil · `400` isian tidak lengkap/format salah ·
`401` belum masuk · `403` tidak berhak · `404` tidak ditemukan · `409` bentrok konkurensi/status sudah
berubah · `422` melanggar aturan bisnis (kode `VAL-BD-*`). Diulang ringkas per grup hanya bila khas.

---

### Health Services / Blood Bank Management / Blood Order

Base URL: `api/v1/health-services/blood-bank-management/blood-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar order darah (daftar kerja #1, `DEC-BD-023`) | `BloodOrder : Read` | `BloodOrderPagedQuery` | `ApiResponse<PagedResult<BloodOrderListDto>>` | Rencana |
| `GET` | `/{id}` | Detail satu order beserta baris & pemenuhannya | `BloodOrder : Read` | — | `ApiResponse<BloodOrderDetailDto>` | Rencana |
| `GET` | `/{id}/fulfillment` | Ringkasan pemenuhan (diminta/diberikan/sisa), menghormati koreksi (`BD-DOM-17`) | `BloodOrder : Read` | — | `ApiResponse<FulfillmentSummaryDto>` | Rencana |
| `POST` | `/` | Buat order elektronik dari unit pelayanan | `BloodOrder : Create` | `CreateBloodOrderRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana · `422 VAL-BD-001/013` |
| `POST` | `/manual` | Buat order manual oleh Bank Darah | `BloodOrder : Create` | `CreateManualBloodOrderRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana · `400 VAL-BD-010` |
| `POST` | `/confirm-duplicate` | Lanjutkan order ganda dengan alasan tertulis (`ASM-BD-001`) | `BloodOrder : Create` | `ConfirmDuplicateOrderRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana |
| `POST` | `/{id}/cancel` | Batalkan order dengan alasan terkendali — oleh **dokter peminta** atau **petugas BDRS** (`DEC-BD-044`) | **`BloodOrder : Cancel`** | `CancelWithReasonRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana · `422 VAL-BD-016/083` |

Kedaluwarsa order (`Expired`) tidak punya endpoint — dipicu sistem dari sinyal kunjungan (`DEC-BD-014`).

**`BloodOrder : Cancel` dipisah dari `BloodOrder : Update` (`DEC-BD-044`).** Pemisahan ini membuat
wewenang membatalkan dapat diberikan kepada dokter peminta **tanpa** ikut memberikan wewenang menyunting
order secara umum. Keduanya memakai **satu** butir yang sama — dokter maupun petugas BDRS — dan yang
membedakan sebabnya pada rekam adalah **kategori alasan** yang wajib diisi: pembatalan klinis atau
pembatalan operasional. Tidak ada pembatalan order tanpa audit (`INV-BD-035`).

**Keadaan terkini sebelum `v5`.** Ketujuh endpoint di atas **tersedia** sejak `BE-BD-003` (11 September
2026), ditambah tiga permukaan teknis `GET /filters/metadata`, `GET /summary`, dan `GET /{id}/status-history`.
`BloodOrder : Update` tetap **tanpa** endpoint. Label "Rencana" pada tabel adalah riwayat penulisan `v4`.

#### Amendment `v5` — Blood Order (18 September 2026; **`approved`** `Sukmagp` 2026-09-19)

Empat perubahan **aditif** untuk `FE-BD-002`, diputuskan `Sukmagp` 18 September 2026. Nol endpoint baru,
nol endpoint dihapus, nol butir hak akses baru. Seluruh perubahan berstatus **`Rencana (belum tersedia)`**
sampai task backend `v5` pemiliknya selesai.

| Method | Path | Yang berubah pada `v5` | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Query baru `bloodComponentId`; isian baru `Components` dan `TotalIssuedQuantity` pada `BloodOrderListDto` (`DEC-BD-058`) | `BloodOrder : Read` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Isian baru `RequestedBloodGroup` + label (`DEC-BD-055`) dan `CancellationReasonCategory` (`DEC-BD-057`) pada `BloodOrderDetailDto` | `BloodOrder : Read` | Rencana (belum tersedia) |
| `POST` | `/` | Isian wajib baru `RequestedBloodGroup` (`400 VAL-BD-085`); `422 VAL-BD-001` membawa `errors` terstruktur (`DEC-BD-056`) | `BloodOrder : Create` | Rencana (belum tersedia) |
| `POST` | `/manual` | Sama dengan `POST /` | `BloodOrder : Create` | Rencana (belum tersedia) |
| `POST` | `/confirm-duplicate` | Isian wajib baru `RequestedBloodGroup` (`400 VAL-BD-085`) | `BloodOrder : Create` | Rencana (belum tersedia) |
| `POST` | `/{id}/cancel` | Request tidak berubah. Jawaban detail ikut membawa isian baru | `BloodOrder : Cancel` | Rencana (belum tersedia) |

**D1 — `RequestedBloodGroup` (`DEC-BD-055`).**

| Tempat | Isian | Tipe | Wajib | Aturan |
| --- | --- | --- | --- | --- |
| `CreateBloodOrderRequest`, `CreateManualBloodOrderRequest`, `ConfirmDuplicateOrderRequest` | `requestedBloodGroup` | `BloodType?` (angka enum `BloodType` yang sudah ada) | **Ya** | Harus **dikirim**. Kosong, `NotDisclosed` (`99`), atau angka di luar enum → `400 VAL-BD-085`, **sebelum** nomor order diminta. `Unknown` (`0`) **sah**. Tipe request sengaja nullable: `Unknown` adalah nilai bawaan enum, sehingga isian yang lupa dikirim tidak boleh diam-diam terbaca sebagai "Tidak diketahui" |
| `BloodOrderDetailDto` | `requestedBloodGroup`, `requestedBloodGroupLabel` | `BloodType?`, `string?` | — | Nilai tersimpan apa adanya. `null` hanya pada order sebelum `v5` — layar menulisnya sebagai "golongan darah diminta tidak tercatat pada order lama" |

`confirm-duplicate` membawa `requestedBloodGroup` sebagai bagian body pembuatan yang utuh. Backend **tidak**
menyimpan percobaan yang tertahan, sehingga backend menjaga keberadaan dan keabsahan nilainya, sedangkan
kesamaan nilai dengan percobaan yang tertahan dijaga layar: panel lanjutan order ganda mengirim ulang
isian percobaan itu tanpa menyuntingnya.

**Batas klinis.** `RequestedBloodGroup` tidak pernah mengubah, menggantikan, atau dibaca oleh golongan
darah sah (`GET /blood-group-exams/patient/{patientId}/valid`), bukti kecocokan, alokasi, maupun pemberian
(`INV-BD-011`). Golongan darah hasil pemeriksaan **tidak** ditambahkan ke `BloodOrderDetailDto`.

**D2 — `errors` pada penahanan order ganda (`DEC-BD-056`).** Hanya untuk `422 VAL-BD-001` dari `POST /`
dan `POST /manual`. Mengikuti konvensi slot `errors` `BbkBloodUnitController`:

```json
{
  "success": false,
  "statusCode": 422,
  "message": "Sudah ada order darah aktif untuk pasien dan komponen ini pada kunjungan yang sama. Lanjutkan hanya dengan alasan tertulis.",
  "data": null,
  "errors": {
    "code": "VAL-BD-001",
    "duplicateComponentIds": ["<BloodComponentId yang benar-benar bentrok>"]
  }
}
```

`duplicateComponentIds` hanya memuat komponen yang bentrok. Contoh: order baru PRC + trombosit, dan hanya
PRC yang sudah punya order aktif, maka isinya satu ID PRC. `message` hanya untuk ditampilkan; layar mengenali
penahanan dari `errors.code`. Kegagalan order lain **tidak** diubah bentuknya pada `v5` (`errors` tetap `null`).

**D3 — `CancellationReasonCategory` (`DEC-BD-057`).**

| Isian | Tipe | Nilai |
| --- | --- | --- |
| `BloodOrderDetailDto.cancellationReasonCategory` | `string?` | `OrderCancellationClinical` bila pengguna yang login tertaut ke dokter peminta order ini (`ApplicationUser.DoctorId == RequestingDoctorId`); `OrderCancellationOperational` untuk pengguna lain; `null` bila `Cancel` tidak ada di `AvailableActions` |

Nilainya **diturunkan** per request dengan fungsi yang sama dengan `POST /{id}/cancel`, dan **tidak disimpan**.
Isian ini hanya menyatakan kategori mana yang akan diterima; ia **bukan** pernyataan bahwa pengguna berhak
membatalkan, yang tetap dijaga `BloodOrder : Cancel`. `POST /{id}/cancel` tetap memeriksa `VAL-BD-083`.
Layar memakainya untuk memanggil `GET /api/v1/health-services/master-data/blood-bank-reasons/options?category=<nilai>`.

**D4 — daftar kerja (`DEC-BD-058`).**

| Tempat | Isian | Tipe | Aturan |
| --- | --- | --- | --- |
| `GET /` query | `bloodComponentId` | `Guid?` | Order yang memiliki **sekurang-kurangnya satu** baris dengan komponen itu. Digabung dengan penyaring lain secara "dan" |
| `BloodOrderListDto` | `components` | `List<BloodOrderListComponentDto>` | Satu butir per baris order, urut `Sequence` |
| `BloodOrderListComponentDto` | `bloodComponentId`, `bloodComponentCode`, `bloodComponentName`, `requestedQuantity`, `issuedQuantity` | `Guid`, `string?`, `string?`, `int`, `int` | `issuedQuantity` dihitung dengan aturan `BD-DOM-17` yang sama dengan `GET /{id}/fulfillment`: kantong `Issued` nyata, dikurangi yang koreksinya `Approved` (`DEC-BD-054`) |
| `BloodOrderListDto` | `totalIssuedQuantity` | `int` | Jumlah `issuedQuantity` seluruh baris. `totalRequestedQuantity` yang sudah ada tetap |

Perhitungan dikerjakan sekaligus untuk seluruh order pada satu halaman — satu kueri berkelompok per jenis
data, bukan satu kueri per baris — sampai ukuran halaman maksimum yang sudah berlaku. Tidak ada penghitung
pemenuhan yang disimpan.

**Kompatibilitas.** Seluruh isian respons baru bersifat aditif. Satu-satunya perubahan yang menolak klien lama
adalah `requestedBloodGroup` yang **wajib** pada tiga endpoint pembuatan. Klien lama yang tidak mengirimnya
akan ditolak `400 VAL-BD-085`; pada 18 September 2026 belum ada klien frontend order darah.

#### Amendment `v5` D5 — penyaring rentang tanggal daftar kerja (23 September 2026)

Ditambahkan atas `DEC-BD-059` dan `DEC-BD-060`, `approved` `Sukmagp` 23 September 2026, menutup
`BD-UI-GAP-004` dengan Opsi B. **Aditif penuh**: dua parameter query opsional, nol perubahan bentuk
respons, nol endpoint baru, nol butir hak akses baru. Dikerjakan task `BE-BD-019`.

| Method | Path | Yang berubah | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Query baru `startDate` dan `endDate`; isian bawaan keduanya pada `BloodOrderDefaultFilterResponse` | `BloodOrder : Read` |

| Tempat | Isian | Tipe | Wajib | Aturan |
| --- | --- | --- | :---: | --- |
| `GET /` query | `startDate` | `DateTime?` | Tidak | **Tanggal operasional waktu aplikasi** (`Asia/Jakarta`), bukan saat UTC. Bentuk yang dikontrakkan `YYYY-MM-DD`. Bagian waktu **dan** penanda zona yang ikut terkirim **diabaikan** — yang dibaca hanya harinya |
| `GET /` query | `endDate` | `DateTime?` | Tidak | Sama, dan **inklusif sampai akhir hari** |
| `BloodOrderDefaultFilterResponse` | `startDate`, `endDate` | `DateTime?` | — | Bawaan `null` — daftar kerja tidak dibatasi waktu sampai petugas memilih rentang |

**Kolom yang disaring.** `BbkBloodOrder.CreateDateTime`, dan hanya itu. Penyaringan dikerjakan
**server-side**; layar tidak pernah menyaring tanggal secara lokal, karena daftarnya berhalaman di server
dan menyaring satu halaman akan membuat nomor halaman serta `totalData` tidak lagi berarti.

**Penerjemahan rentang ke UTC (`DEC-BD-060`).** `CreateDateTime` **tersimpan dalam UTC**, sedangkan kedua
parameter di atas adalah tanggal waktu Jakarta. Karena itu batasnya **dikonversi**, bukan distempel:

| Batas | Nilai |
| --- | --- |
| Bawah, **inklusif** | Pukul `00:00` waktu aplikasi pada `startDate`, dikonversi ke UTC |
| Atas, **eksklusif** | Pukul `00:00` waktu aplikasi pada `endDate + 1 hari`, dikonversi ke UTC |

Batas atas eksklusif pada hari berikutnya itulah yang mewujudkan "inklusif sampai akhir hari" tanpa
kehilangan pecahan detik terakhir — yang akan terbuang bila batasnya ditulis `<= 23:59:59`.

**Contoh mengikat.** `startDate = endDate = 2026-09-23` menghasilkan rentang efektif:

```
CreateDateTime >= 2026-09-22T17:00:00Z
CreateDateTime <  2026-09-23T17:00:00Z
```

Order yang dibuat pukul `02:00` WIB tanggal 23 September tersimpan `2026-09-22T19:00:00Z`, dan **masuk**
ke rentang itu. Menstempel `DateTimeKind.Utc` apa adanya akan membuangnya — pola keliru yang ada pada
beberapa controller master data dan **tidak boleh** ditiru di sini.

**Penggabungan.** Rentang digabung "dan" dengan `search`, `patientId`, `encounterId`, `serviceUnitId`,
`bloodComponentId`, `orderStatus`, dan `orderSource`. Paging berlaku atas hasil yang sudah tersaring:
`totalData` dan `totalPage` menghitung hasil akhir.

**Jalur tidak normal.**

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| `startDate` melewati `endDate` | Ditolak sebelum menyentuh database | `400` `VAL-BD-086` |
| `startDate` sama dengan `endDate` | **Sah** — menyaring satu hari penuh | `200` |
| Hanya salah satu dikirim | **Sah** — rentangnya terbuka di sisi yang tidak dikirim | `200` |
| Keduanya kosong | Perilaku **sama persis** dengan sebelum `D5` | `200` |

Rentang terbalik **tidak** diserahkan ke database. Query yang mustahil memulangkan nol baris, dan nol
baris terbaca petugas sebagai "tidak ada order" — bukan sebagai "filternya salah".

---

### Health Services / Master Data / Blood Storage Location

Base URL: `api/v1/health-services/master-data/blood-storage-locations`

Master lokasi penyimpanan darah milik BDRS (`DEC-BD-035`). **Bukan** cold storage farmasi —
`MstDrugStorageLocation` punya controller sendiri dan tidak disentuh modul ini.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar lokasi penyimpanan darah | `BloodStorageLocation : Read` | `BloodStorageLocationPagedQuery` | `ApiResponse<PagedResult<BloodStorageLocationListDto>>` | Rencana |
| `GET` | `/options` | Pilihan lokasi untuk dropdown; **hanya yang `IsActive = true`** | `BloodStorageLocation : Read` | — | `ApiResponse<List<OptionDto>>` | Rencana |
| `GET` | `/{id}` | Detail satu lokasi | `BloodStorageLocation : Read` | — | `ApiResponse<BloodStorageLocationDto>` | Rencana |
| `POST` | `/` | Tambah lokasi penyimpanan darah | `BloodStorageLocation : Create` | `CreateBloodStorageLocationRequest` | `ApiResponse<BloodStorageLocationDto>` | Rencana · `422 VAL-BD-067` |
| `PUT` | `/{id}` | Ubah kode, nama, keterangan | `BloodStorageLocation : Update` | `UpdateBloodStorageLocationRequest` | `ApiResponse<BloodStorageLocationDto>` | Rencana · `422 VAL-BD-067` |
| `PATCH` | `/{id}/status` | **Aktifkan atau nonaktifkan lokasi** (`DEC-BD-037`) | `BloodStorageLocation : Update` | `SetActiveStatusRequest` | `ApiResponse<BloodStorageLocationDto>` | Rencana · `200 VAL-BD-068` |
| `DELETE` | `/{id}` | Tandai lokasi terhapus (soft delete) — untuk keadaan sehari-hari **menonaktifkan tetap lebih tepat** | `BloodStorageLocation : Delete` | — | `ApiResponse<bool>` | **Terimplementasi `BE-BD-014`** — baris disinkronkan `BE-BD-016` 17 September 2026 |

`GET /options` sengaja menyaring hanya lokasi aktif, sehingga frontend tidak perlu menyaring sendiri dan
tidak mungkin menawarkan lokasi nonaktif sebagai tujuan penyimpanan (`INV-BD-027`).

`PATCH /{id}/status` **tidak** memindahkan kantong apa pun dan **tidak** mengembalikan daftar kantong
terdampak sebagai akibat. Respons `200` disertai peringatan `VAL-BD-068` yang menyebut **berapa banyak**
kantong kini tertahan, supaya petugas tahu ada pekerjaan yang menunggu — pekerjaan itu dikerjakan lewat
`PUT /blood-units/{id}/storage-location`, satu per satu, oleh manusia (`DEC-BD-037`).

Lokasi **tidak dapat dihapus**; hanya dinonaktifkan. Penempatan lama menunjuk ke sini lewat `Restrict`,
dan riwayat kantong wajib tetap terbaca.

---

### Health Services / Blood Bank Management / Provider Request

Base URL: `api/v1/health-services/blood-bank-management/provider-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar permintaan ke PMI | `BloodProviderRequest : Read` | `ProviderRequestPagedQuery` | `ApiResponse<PagedResult<ProviderRequestListDto>>` | Rencana |
| `GET` | `/{id}` | Detail permintaan + riwayat penerimaan | `BloodProviderRequest : Read` | — | `ApiResponse<ProviderRequestDetailDto>` | Rencana |
| `POST` | `/` | Buat permintaan atas nama satu pasien | `BloodProviderRequest : Create` | `CreateProviderRequestRequest` | `ApiResponse<ProviderRequestDetailDto>` | Rencana · `422 VAL-BD-006` |
| `POST` | `/{id}/receipts` | Catat penerimaan kantong (termasuk kelebihan) | `BloodProviderRequest : Process` | `RecordReceiptRequest` | `ApiResponse<ProviderRequestDetailDto>` | Rencana · `200 VAL-BD-014` |
| `POST` | `/{id}/cancel` | Batalkan permintaan | `BloodProviderRequest : Update` | `CancelWithReasonRequest` | `ApiResponse<ProviderRequestDetailDto>` | Rencana |

Penutupan administratif (`ClosedEncounter`) dipicu sistem, bukan endpoint (`DEC-BD-020`).

---

### Health Services / Blood Bank Management / Blood Unit

Base URL: `api/v1/health-services/blood-bank-management/blood-units`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kantong; filter `status=PendingReview` = daftar kerja #2; filter `emergencyPendingEvidence=true` = daftar kerja #3; filter `inactiveLocation=true` = kantong tertahan di lokasi nonaktif (**`D6`**, 24 September 2026) | `BloodUnit : Read` | `BloodUnitPagedQuery` | `ApiResponse<PagedResult<BloodUnitListDto>>` | Rencana · `inactiveLocation` **terimplementasi `BE-BD-020`** |
| `GET` | `/{id}` | Detail kantong + riwayat alokasi/bukti/koreksi | `BloodUnit : Read` | — | `ApiResponse<BloodUnitDetailDto>` | Rencana |
| `GET` | `/{id}/placements` | Riwayat penempatan kantong: di kulkas mana, sejak kapan, oleh siapa | `BloodUnit : Read` | — | `ApiResponse<List<BloodUnitPlacementDto>>` | Rencana |
| `POST` | `/{id}/storage-location` | **Tetapkan lokasi penyimpanan pertama** — membawa kantong `Received`→`Stored`→`Available` (`DEC-BD-036`) | `BloodUnit : Store` | `AssignStorageLocationRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-060/061` |
| `PUT` | `/{id}/storage-location` | **Pindahkan kantong ke lokasi lain** — status **tidak** berubah (`INV-BD-026`) | `BloodUnit : Store` | `MoveStorageLocationRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-060/062` |
| `POST` | `/{id}/allocate` | Alokasikan kantong ke satu baris kebutuhan | `BloodUnit : Allocate` | `AllocateUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `409 VAL-BD-018c` · `422 VAL-BD-033/063/064` |
| `POST` | `/{id}/cancel-allocation` | Batalkan alokasi keliru sebelum pemberian (`DEC-BD-029`) | `BloodUnit : Allocate` | `CancelWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-023` |
| `POST` | `/{id}/compatibility-evidence` | Catat bukti kecocokan terhadap pasien tujuan, **beserta hasil keputusannya** (`DEC-BD-042`) | `BloodUnit : Compatibility` | `RecordEvidenceRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-078` · `422 VAL-BD-079` |
| `POST` | `/{id}/issue` | Berikan kantong kepada pasien | `BloodUnit : Issue` | `IssueUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-017/018/019/020/020b/065/079` |
| `POST` | `/{id}/emergency-issue` | Berikan lewat jalur darurat, melewati gerbang bukti dan/atau lokasi nonaktif (`DEC-BD-017`, `DEC-BD-038`). Penerbit **Dokter BDRS atau DPJP** (`DEC-BD-040`) | `BloodUnit : EmergencyIssue` | `EmergencyIssueRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-021/072` · `422 VAL-BD-066/070/071` |
| `POST` | `/{id}/corrections` | **Ajukan** koreksi pencatatan pemberian; koreksi belum berlaku (`DEC-BD-041`) | `BloodUnit : Correct` | `RequestIssuanceCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | **Terimplementasi `BE-BD-010`** · `400 VAL-BD-016` · `403 VAL-BD-024` · `422 VAL-BD-025/049/076` |
| `POST` | `/{id}/corrections/{correctionId}/approve` | **Setujui** koreksi; sejak saat ini koreksi berlaku dan pemenuhan dihitung ulang | `BloodUnit : ApproveCorrection` | `DecideCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | **Terimplementasi `BE-BD-010`** · `403 VAL-BD-074` · `422 VAL-BD-073/075` |
| `POST` | `/{id}/corrections/{correctionId}/reject` | **Tolak** koreksi; rekam tidak berubah sama sekali | `BloodUnit : ApproveCorrection` | `DecideCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | **Terimplementasi `BE-BD-010`** · `403 VAL-BD-074` · `422 VAL-BD-073/075/077` |
| `GET` | `/{id}/corrections` | Daftar koreksi pada kantong ini beserta keadaannya | `BloodUnit : Read` | — | `ApiResponse<List<IssuanceCorrectionDto>>` | **Terimplementasi `BE-BD-010`** |
| `POST` | `/{id}/reallocate` | Alihkan kantong `PendingReview` ke pasien lain | **`BloodUnit : ResolveReallocate`** | `ReallocateUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | **Terimplementasi `BE-BD-009`** · `400 VAL-BD-016` · `403 VAL-BD-080` · `422 VAL-BD-064` |
| `POST` | `/{id}/return-to-provider` | Kembalikan kantong ke PMI | **`BloodUnit : ResolveReturn`** | `ResolveWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | **Terimplementasi `BE-BD-009`** · `400 VAL-BD-016` · `403 VAL-BD-081` |
| `POST` | `/{id}/mark-not-usable` | Nyatakan kantong tidak layak | **`BloodUnit : ResolveNotUsable`** | `ResolveWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | **Terimplementasi `BE-BD-009`** · `400 VAL-BD-016` · `403 VAL-BD-082` |

#### Amendment `v5` D6 — penyaring kantong tertahan di lokasi nonaktif (24 September 2026)

Ditambahkan atas keputusan pemilik `Sukmagp` 24 September 2026 (butir `D2`–`D4` pada audit
[`BE-BD-020`](../task/report/backend/BE-BD-020.md) bagian 2), menutup saringan `inactiveLocation` yang sudah
dijanjikan `03-frontend-architecture.md` untuk `FE-BD-012` tetapi belum pernah ada di kontrak ini.
**Aditif penuh**: satu parameter query opsional, satu isian bawaan pada metadata penyaring, nol perubahan
bentuk respons, nol endpoint baru, nol butir hak akses baru. Dikerjakan task `BE-BD-020`.

| Method | Path | Yang berubah | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Query baru `inactiveLocation` | `BloodUnit : Read` |
| `GET` | `/filters/metadata` | Isian bawaan `inactiveLocation` pada `BloodUnitDefaultFilterResponse` | `BloodUnit : Read` |

| Tempat | Isian | Tipe | Wajib | Aturan |
| --- | --- | --- | :---: | --- |
| `GET /` query | `inactiveLocation` | `bool?` | Tidak | `true` = hanya kantong tertahan; `false` = kebalikan persisnya; kosong = tidak menyaring |
| `BloodUnitDefaultFilterResponse` | `inactiveLocation` | `bool?` | — | Bawaan `null` |

**Definisi "tertahan di lokasi nonaktif".** Sebuah kantong tertahan bila **ketiga** syarat ini terpenuhi
sekaligus:

| Syarat | Isi | Keputusan |
| --- | --- | --- |
| Masih di stok | Status `Stored`, `Available`, `Allocated`, `PendingReview`, atau `Reallocated` — himpunan yang sama dengan hitungan peringatan `VAL-BD-068` | `D2` |
| Punya lokasi berlaku | `CurrentPlacementId` terisi. Kantong **tanpa lokasi** (`Received`) **tidak** dianggap tertahan | — |
| Lokasinya nonaktif | Lokasi berlakunya `IsActive = false` **atau** terhapus (`IsDelete = true`) — definisi yang sama dengan gerbang alokasi `VAL-BD-064` dan penanda `isCurrentStorageLocationActive` | `D3` |

Kantong berstatus akhir — `Issued`, `ReturnedToProvider`, `NotUsable` — **tidak pernah** muncul pada
`inactiveLocation=true` walaupun `CurrentPlacementId`-nya masih menunjuk lokasi terakhir yang kini
nonaktif: kantongnya sudah keluar dari stok, sehingga tidak ada lagi yang perlu dipindahkan.

**`inactiveLocation=false` adalah kebalikan persisnya (`D4`)**, mengikuti pola `emergencyPendingEvidence`:
seluruh kantong yang **tidak** memenuhi ketiga syarat di atas — kantong di lokasi aktif, kantong tanpa
lokasi, dan kantong berstatus akhir. Untuk setiap kantong berlaku: muncul di `true` **atau** di `false`,
tidak pernah keduanya dan tidak pernah tidak sama sekali.

**Penggabungan.** Digabung "dan" dengan `search`, `unitStatus`, `isExcess`, `providerRequestId`,
`bloodComponentId`, dan `emergencyPendingEvidence`. Paging berlaku atas hasil yang sudah tersaring:
`totalData` dan `totalPage` menghitung hasil akhir. Penyaringan dikerjakan **server-side**.

**Yang tidak berubah.** `GET /summary` tidak memperoleh hitungan kantong tertahan. Gerbang alokasi dan
pemberian (`VAL-BD-064`/`065`) tidak disentuh — penyaring ini hanya membaca. Kantong tidak dipindahkan
dan statusnya tidak diubah (`DEC-BD-037`).

**Catatan nama parameter status.** Baris `GET /` di atas dan `03-frontend-architecture.md` menulis
`status=`; nama parameter sebenarnya di source adalah **`unitStatus`**. Selisih penulisan ini sudah ada
sebelum `D6` dan **tidak** diubah amandemen ini — klien memakai `unitStatus`.

#### Amendment `v5` D7 — proyeksi gerbang pemberian pada detail kantong (25 September 2026)

Ditambahkan atas keputusan pemilik `Sukmagp` 25 September 2026 (butir `R1`–`R5` pada audit
[`BE-BD-021`](../task/report/backend/BE-BD-021.md) bagian 2). Menutup backlog "proyeksi gerbang pemberian"
yang dicatat laporan `FE-BD-005` bagian 8: layar kantong baru tahu gerbang pemberian tertutup **sesudah**
tombol Berikan ditekan dan ditolak. **Aditif penuh**: dua isian nullable baru pada `BloodUnitDetailDto`,
nol endpoint baru, nol butir hak akses baru, nol kode `VAL-BD-*` baru, nol perubahan aturan gerbang.
Dikerjakan task `BE-BD-021`.

| Method | Path | Yang berubah | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}` | Respons membawa `issuanceGate` dan `emergencyBypass` | `BloodUnit : Read` |
| `POST` | Seluruh aksi kantong yang memulangkan `ApiResponse<BloodUnitDetailDto>` | Respons suksesnya ikut membawa kedua isian, karena detailnya dibangun fungsi yang sama | Tidak berubah |

| Tempat | Isian | Tipe | Aturan |
| --- | --- | --- | --- |
| `BloodUnitDetailDto` | `issuanceGate` | `BloodUnitIssuanceGateDto?` | Terisi **hanya** bila `unitStatus = Allocated` (`R1`); `null` pada status lain |
| `BloodUnitDetailDto` | `emergencyBypass` | `BloodUnitEmergencyBypassDto?` | Sama dengan `issuanceGate` |
| `BloodUnitIssuanceGateDto` | `isOpen` | `bool` | `true` = `POST /{id}/issue` tidak ditahan gerbang pada keadaan saat detail dibaca |
| `BloodUnitIssuanceGateDto` | `validationCode` | `string?` | Kode penahan pertama: `VAL-BD-017`, `065`, `018`, `019`, `020b`, `079`, atau `020`. `null` bila terbuka |
| `BloodUnitIssuanceGateDto` | `message` | `string` | Pesan yang sama persis dengan penolakan `issue` untuk kode itu |
| `BloodUnitIssuanceGateDto` | `compatibilityEvidenceId` | `Guid?` | Bukti pasien tujuan yang dinilai, bila penilaian sampai ke sana (`079`, `020`, atau terbuka) |
| `BloodUnitIssuanceGateDto` | `validUntil` | `DateTime?` (UTC) | Terisi **hanya** bila terbuka atau `020` (`R3`). `null` berarti **tidak dihitung**, bukan berlaku tanpa batas |
| `BloodUnitEmergencyBypassDto` | `evidenceGateClosed` | `bool` | Gerbang bukti menahan, dinilai terlepas dari lokasi |
| `BloodUnitEmergencyBypassDto` | `locationGateClosed` | `bool` | Kantong tanpa penempatan atau lokasinya tidak aktif |
| `BloodUnitEmergencyBypassDto` | `patientId` | `Guid?` | Pasien tujuan dari alokasi aktif |
| `BloodUnitEmergencyBypassDto` | `validCompatibilityEvidenceId` | `Guid?` | Bukti yang berlaku, hanya bila `evidenceGateClosed = false` |

**Sumber nilainya.** Kedua isian adalah hasil evaluator yang sama yang dipakai tindakannya,
`EvaluateIssuanceGateAsync` untuk `issue` dan `EvaluateEmergencyBypassAsync` untuk `emergency-issue`.
Keduanya dibaca apa adanya, bukan dihitung ulang dan bukan disalin.

**Urutan penilaian gerbang normal tidak berubah** dan berhenti pada penahan pertama:
status → lokasi (`065`) → alokasi aktif (`017`) → `018` → `019` → `020b` → `079` → `020`. Contoh: kantong di
kulkas nonaktif yang buktinya juga kedaluwarsa memulangkan `issuanceGate.validationCode = "VAL-BD-065"`
tanpa `validUntil`. Keadaan buktinya tetap terbaca di `emergencyBypass.evidenceGateClosed = true`.

**Cara membaca `emergencyBypass` untuk jalur darurat (`R5`).** Klien memetakan kedua boolean ke
`bypassScope` tanpa isian turunan dari backend:

| `evidenceGateClosed` | `locationGateClosed` | `bypassScope` yang diterima `emergency-issue` |
| :---: | :---: | --- |
| `true` | `false` | `CompatibilityEvidence` (`0`) |
| `false` | `true` | `InactiveStorageLocation` (`1`) |
| `true` | `true` | `Both` (`2`) |
| `false` | `false` | Tidak ada. Jalur normal terbuka, dan setiap cakupan ditolak `VAL-BD-066` |

**Petunjuk, bukan izin.** Nilainya potret saat detail dibaca. `issue` dan `emergency-issue` tetap menilai
ulang gerbang saat ditekan, jadi keadaan yang berubah di antaranya (misalnya bukti lewat `validUntil`
selagi layar terbuka) tetap ditolak dengan kode dan pesan yang sama seperti sebelum `D7`. Klien tetap wajib
menangani `422`.

**Yang tidak berubah.** `AvailableActions`, request dan kode galat `issue`/`emergency-issue`, daftar
kantong `GET /`, dan penanda `isCurrentStorageLocationActive`. Penanda itu memakai definisi aktif
`IsActive && !IsDelete`, sedangkan gerbang lokasi juga memeriksa `IsCancel`. Selisih ini sudah ada sebelum
`D7`, **tidak** diubah amandemen ini, dan dicatat sebagai technical debt (`R4`). Bila keduanya berbeda,
`emergencyBypass.locationGateClosed` yang menjadi acuan gerbang.

Pemberian (`issue`/`emergency-issue`) tidak dapat dibatalkan — status terminal. Koreksi tidak
memindahkan kantong keluar dari `Issued` dan tidak dapat dipakai memindahkan pemberian ke pasien lain
(`422 VAL-BD-049`, `DEC-BD-052`). Sesudah koreksi disetujui kantong **tetap `Issued`**; penanganan fisik
kantong yang ternyata masih ada berada di luar jalur koreksi (`DEC-BD-051`).

**Isian penjaga request-only pada `RequestIssuanceCorrectionRequest` (`DEC-BD-053`).** Selain isian yang
disimpan — `WhatWasWrong`, `WhatIsCorrect`, `ReasonCode`, `SupportingEvidenceNote` — body boleh membawa dua
isian penjaga: `AnnulIssuance` (`bool?`) dan `IssuedToPatientId` (`Guid?`). Keduanya **penjaga transport/request
saja**: **tidak pernah disimpan** dan **bukan** kolom `BbkIssuanceCorrection`. `AnnulIssuance: true` →
`422 VAL-BD-025`; `IssuedToPatientId` diisi dan berbeda dari penerima pemberian asal → `422 VAL-BD-049`.
Pelaku, waktu, status koreksi, dan pemutus tidak pernah diterima dari body.

**Efek koreksi terhadap pemenuhan (`DEC-BD-054`).** Ringkasan pemenuhan `BD-DOM-17`
(`GET /blood-orders/{id}/fulfillment`) menghitung kantong `Issued` nyata. Kantong dengan koreksi `Approved`
**dikeluarkan** dari jumlah diberikan; koreksi `Requested` dan `Rejected` **tidak** memengaruhinya. Pemberian
asal, status `Issued`, dan penerima tidak berubah.

**Tiga endpoint penyelesaian, tiga butir hak akses berbeda (`DEC-BD-043`).** Ketiganya berangkat dari
`PendingReview` tetapi arah risikonya berlawanan: pengalihan **memasukkan** darah ke tubuh pasien baru,
sedangkan pengembalian dan penetapan tidak layak **mengeluarkan** darah dari peredaran. Satu butir
`Resolve` untuk ketiganya berarti siapa pun yang boleh membuang kantong rusak otomatis boleh
mengalihkan darah ke pasien lain — dan itu justru tindakan paling berisiko di antara ketiganya.
Endpoint-nya sendiri **tidak berubah**; yang berubah hanya penjaganya.

**Sinkronisasi HTTP penolakan penyelesaian — `BE-BD-009`, 17 September 2026.** Baris `reallocate`
semula mengelompokkan `422 VAL-BD-016/064`. Kode HTTP per kode validasi dimiliki
[validation-matrix](validation-matrix.md), yang menetapkan **`VAL-BD-016` = `400`** dan
**`VAL-BD-064` = `422`**; runtime `BE-BD-009` membuktikan keduanya persis demikian pada endpoint ini.
`VAL-BD-016` juga berlaku pada `return-to-provider` dan `mark-not-usable` — matriks perpindahan
status §3 sudah mencantumkannya sejak semula, dan runtime membuktikannya. Ini penyelarasan penulisan,
bukan perubahan aturan.

**Bukti kecocokan kini menyimpan hasil keputusan.** `RecordEvidenceRequest` bertambah satu isian wajib:
hasilnya cocok atau tidak cocok. Bukti bertanda tidak cocok **tetap tersimpan** dan **tidak** membuka
gerbang pemberian — karena itu `POST /{id}/issue` bertambah kemungkinan penolakan `VAL-BD-079`.

**Koreksi memakai tiga endpoint, bukan satu, dan itu bukan pemecahan kosmetik.** `DEC-BD-041`
menjadikan koreksi proses dua tahap dengan dua pelaku berbeda dan dua butir hak akses berbeda.
Menyatukannya menjadi satu endpoint bersaklar akan membuat satu butir hak akses menjaga dua tindakan
yang justru sengaja dipisah, sehingga pemisahan wewenangnya hilang di lapisan API.

`correctionId` muncul pada path karena satu kantong dapat punya lebih dari satu koreksi sepanjang
riwayatnya — sebagian disetujui, sebagian ditolak. Keputusan selalu menunjuk satu permintaan tertentu.

**Respons koreksi mengembalikan `IssuanceCorrectionDto`, bukan `BloodUnitDetailDto`.** Berbeda dengan
`v2`, mengajukan koreksi **tidak mengubah keadaan kantong** — kantong tetap `Issued` dan angka
pemenuhan tidak bergerak sampai persetujuan turun. Mengembalikan detail kantong akan menyiratkan
sesuatu telah berubah pada kantong, padahal belum.

**Dua endpoint penyimpanan memakai method berbeda dengan sengaja.** `POST /{id}/storage-location`
adalah penetapan **pertama** dan satu-satunya yang memindahkan status (`Received`→`Stored`→`Available`);
ia gagal bila kantong sudah pernah ditempatkan. `PUT /{id}/storage-location` adalah **perpindahan** dan
tidak pernah menyentuh status; ia gagal bila kantong belum pernah ditempatkan. Keduanya sama-sama
menambah satu baris riwayat yang tidak pernah dihapus (`INV-BD-026`), dan keduanya menolak lokasi
tujuan yang nonaktif (`INV-BD-027`).

**Tidak ada endpoint untuk memindahkan kantong secara massal saat lokasi dinonaktifkan.** Itu disengaja:
`DEC-BD-037` menetapkan sistem tidak memindahkan kantong dengan sendirinya. Petugas memindahkan satu per
satu lewat `PUT /{id}/storage-location`, dan setiap perpindahan menyimpan pelaku serta waktunya sendiri.

**Tidak ada endpoint untuk "menutup gerbang".** Gerbang dinilai saat alokasi dan pemberian dicoba
(`ARCH-BD-POS-06`, `ARCH-BD-POS-07`); menonaktifkan lokasi lewat endpoint master sudah cukup untuk
menutupnya pada saat yang sama.

---

### Health Services / Blood Bank Management / Blood Group Exam

Base URL: `api/v1/health-services/blood-bank-management/blood-group-exams`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pemeriksaan golongan darah | `BloodGroupExam : Read` | `BloodGroupExamPagedQuery` | `ApiResponse<PagedResult<BloodGroupExamListDto>>` | Rencana |
| `GET` | `/{id}` | Detail pemeriksaan + status konflik | `BloodGroupExam : Read` | — | `ApiResponse<BloodGroupExamDetailDto>` | Rencana |
| `GET` | `/patient/{patientId}/valid` | Golongan darah sah pasien / penanda konflik (`BD-DOM-21`) | `BloodGroupExam : Read` | — | `ApiResponse<ValidBloodGroupDto>` | Rencana |
| `POST` | `/` | Catat pengambilan sampel | `BloodGroupExam : Create` | `RecordSampleRequest` | `ApiResponse<BloodGroupExamDetailDto>` | Rencana |
| `POST` | `/{id}/result` | Catat hasil ABO & Rhesus | `BloodGroupExam : Update` | `RecordResultRequest` | `ApiResponse<BloodGroupExamDetailDto>` | Rencana · `400 VAL-BD-030` |
| `POST` | `/{id}/validate` | Validasi hasil **rutin** (deteksi konflik `BD-XINV-04`) | `BloodGroupExam : Validate` | — | `ApiResponse<BloodGroupExamDetailDto>` | Rencana · `403 VAL-BD-037` |
| `POST` | `/conflict-resolution` | Selesaikan konflik dengan menunjuk pemeriksaan ulang tervalidasi (`DEC-BD-031`) | **`BloodGroupExam : ResolveConflict`** | `ResolveConflictRequest` | `ApiResponse<ValidBloodGroupDto>` | Rencana · `403 VAL-BD-069` · `422 VAL-BD-051/054` |

**Dua butir hak akses, bukan satu (`DEC-BD-039`).** `Validate` menjaga validasi hasil rutin dan boleh
dipegang petugas BDRS yang ditunjuk; `ResolveConflict` menjaga penyelesaian konflik dan hanya dipegang
validator klinis. Pemisahan ini ada di lapisan hak akses, bukan hanya di dokumen: satu butir yang
menjaga keduanya membuat siapa pun yang boleh memvalidasi hasil rutin otomatis boleh menutup konflik.

Gerbang wewenang **tidak** menggantikan prasyarat. `DEC-BD-031` tetap berlaku penuh — penyelesaian
konflik wajib menunjuk pemeriksaan ulang tervalidasi (`VAL-BD-051`), dan validator klinis sekalipun
tidak dapat menutup konflik tanpa itu.

Penyelesaian konflik dilakukan di layar pemeriksaan, **bukan** daftar kerja keempat (`DEC-BD-033`).

---

### Health Services / Blood Bank Management / Blood Bank Procedure

Base URL: `api/v1/health-services/blood-bank-management/blood-bank-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar tindakan Bank Darah | `BloodBankProcedure : Read` | `ProcedurePagedQuery` | `ApiResponse<PagedResult<ProcedureListDto>>` | Rencana |
| `GET` | `/{id}` | Detail tindakan | `BloodBankProcedure : Read` | — | `ApiResponse<ProcedureDetailDto>` | Rencana |
| `POST` | `/` | Catat tindakan atas satu order | `BloodBankProcedure : Create` | `CreateProcedureRequest` | `ApiResponse<ProcedureDetailDto>` | Rencana · `400 VAL-BD-026` |
| `POST` | `/{id}/complete` | Nyatakan tindakan selesai; sesudah tersimpan, serahkan satu fakta biaya ke Billing | `BloodBankProcedure : Update` | — | `ApiResponse<ProcedureDetailDto>` + `BillingHandoff` | Tersedia · `422` tindakan tidak `Recorded` · `409` konkurensi |
| `POST` | `/{id}/resend-cost-fact` | Kirim ulang fakta biaya tindakan yang sudah selesai, tanpa perpindahan status | `BloodBankProcedure : Update` | — | `ApiResponse<ProcedureDetailDto>` + `BillingHandoff` | Tersedia · `422` belum `Completed` atau fakta ditolak · `409` hasil kiriman sebelumnya belum pasti |

**Delta 17 September 2026 — `DEC-BD-016` disetujui, `BE-BD-013`.** Satu endpoint baru dan satu isian respons
baru; tidak ada yang dihapus maupun diganti nama.

- **`BillingHandoff`** — isian **aditif dan nullable** pada `ProcedureDetailDto`. Terisi hanya pada jawaban
  `complete` dan `resend-cost-fact`; selalu `null` pada `GET`. Isinya `Kind` (`Emitted`, `Replayed`,
  `OutcomeUnknown`, `ReconciliationRequired`, `RejectedByBilling`, `Invalid`), `IsClinicallySafe`,
  `MilestoneFactId`, `MilestoneFactVersion`, `DispatchStatus`, `Code`, `Message`. Keterangan proses, **bukan**
  status pembayaran.
- **`complete`** tetap `200` bila penyelesaian tersimpan, walau Billing menolak atau belum pasti — pesannya
  menyebut bahwa penyerahan memerlukan tinjauan. Menyelesaikan ulang tindakan `Completed` tetap `422`.
- **`resend-cost-fact`** memakai hak akses yang sama dengan `complete`, sehingga butir hak akses tidak
  bertambah. `200` untuk `Emitted`/`Replayed`; `409` untuk `OutcomeUnknown`/`ReconciliationRequired`; `422`
  untuk `RejectedByBilling`/`Invalid` atau tindakan belum `Completed`. Ringkasan penyerahan ikut pada `errors`.
- **Tidak ada isian Billing dari client.** Kedua endpoint tanpa body; konteks sumber, jenis efek, kunjungan,
  identitas fakta, dan nominal diturunkan backend.
- `AvailableActions` **tidak** berubah: kirim ulang adalah jalur pemulihan, bukan langkah lifecycle.

**Riwayat — sampai 17 September 2026:** tidak ada endpoint penyaluran biaya ke Billing — tertahan
`DEC-BD-016`. Fakta biaya boleh dirancang sebagai kejadian domain nanti, tetapi kontraknya belum dibekukan.

---

### Health Services / Master Data / Blood Component

Base URL: `api/v1/health-services/master-data/blood-components`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar komponen darah | `BloodComponent : Read` | `PagedQuery` | `ApiResponse<PagedResult<BloodComponentDto>>` | Rencana |
| `GET` | `/options` | Opsi komponen untuk dropdown | `BloodComponent : Read` | — | `ApiResponse<List<OptionDto>>` | Rencana |
| `GET` | `/{id}` | Detail komponen | `BloodComponent : Read` | — | `ApiResponse<BloodComponentDto>` | Rencana |
| `POST` | `/` | Tambah komponen | `BloodComponent : Create` | `UpsertBloodComponentRequest` | `ApiResponse<BloodComponentDto>` | Rencana |
| `PUT` | `/{id}` | Ubah komponen (termasuk `CompatibilityEvidenceValidityHours`) | `BloodComponent : Update` | `UpsertBloodComponentRequest` | `ApiResponse<BloodComponentDto>` | Rencana |
| `DELETE` | `/{id}` | Nonaktifkan komponen (soft delete) | `BloodComponent : Delete` | — | `ApiResponse<bool>` | Rencana |

---

### Health Services / Master Data / Blood Bank Reason

Base URL: `api/v1/health-services/master-data/blood-bank-reasons`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar alasan terkendali | `BloodBankReason : Read` | `PagedQuery` | `ApiResponse<PagedResult<BloodBankReasonDto>>` | Rencana |
| `GET` | `/options` | Opsi alasan per kategori (`?category=`) | `BloodBankReason : Read` | `category` | `ApiResponse<List<OptionDto>>` | Rencana |
| `GET` | `/{id}` | Detail alasan | `BloodBankReason : Read` | — | `ApiResponse<BloodBankReasonDto>` | Rencana |
| `POST` | `/` | Tambah alasan | `BloodBankReason : Create` | `UpsertReasonRequest` | `ApiResponse<BloodBankReasonDto>` | Rencana |
| `PUT` | `/{id}` | Ubah alasan | `BloodBankReason : Update` | `UpsertReasonRequest` | `ApiResponse<BloodBankReasonDto>` | Rencana |
| `DELETE` | `/{id}` | Nonaktifkan alasan | `BloodBankReason : Delete` | — | `ApiResponse<bool>` | Rencana |

---

## Catatan kontrak

- **Kewenangan unit** (`DEC-BD-012`) tidak punya endpoint tersendiri; ia kolom `IsAvailableForBloodOrder`
  pada `MstServiceUnit`, dikelola lewat kontrak Master Data unit pelayanan yang sudah ada.
- **Tiga daftar kerja MVP** (`DEC-BD-023`) semuanya endpoint `GET` dengan filter, bukan modul laporan.
- DTO didaftar di `02-backend-architecture.md`/`data-dictionary.md`; field lengkap dibekukan saat
  implementasi. Nomor bisnis dialokasikan service lewat number-series, **tidak** dikirim klien.
