# Kesiapan Teknis — Modul Gizi

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Status desain domain | **`BLOCKED / WAITING BUSINESS DECISION`** — lihat [`03-desain-domain.md`](03-desain-domain.md) |
| Sifat dokumen | Audit **read-only** atas source pada `597cdde9`, 24 September 2026 |
| Nol keputusan bisnis | Dokumen ini **tidak** menjawab `GIZ-OQ-002`, `004`, maupun `006`, dan tidak menurunkan struktur tabel apa pun dari tebakan |

## Mengapa lembar ini ada

Desain domain Gizi tertahan menunggu keputusan pemilik proses. Yang **tidak** tertahan adalah
kenyataan teknis: sebagian modul Gizi ternyata sudah berjalan di source, dan pengetahuan itu
menentukan seberapa besar pekerjaan yang tersisa begitu keputusan turun.

Lembar ini mencatat apa yang sudah ada, apa yang bergantung pada modul lain, dan bagian mana yang
dapat dikerjakan **tanpa** menunggu keputusan domain. Seluruh isinya berasal dari pembacaan source
dan database uji — nol asumsi.

## Yang sudah berjalan di source

| Lapisan | Isi |
|---|---|
| Controller | `NutritionOrderController`, `NutritionDietController`, `NutritionMasterController` |
| Service | `NutritionOrderService`, `NutritionDietService` |
| Model | `GziNutritionOrder`, `GziNutritionOrderHistory`, `GziNutritionCareRecord`, `GziPatientDiet`, `GziProductionBatch`, `GziNutritionMasters` |
| Configuration | `GziNutritionConfigurations.cs` |
| Tabel pada snapshot | **10**: `GziDietType`, `GziFoodForm`, `GziMealSchedule`, `GziMealDelivery`, `GziNutritionOrder`, `GziNutritionOrderHistory`, `GziNutritionCareRecord`, `GziPatientDiet`, `GziProductionBatch`, `GziProductionBatchDetail` |
| DTO / enum | 2 berkas DTO, 1 berkas enum |

### Endpoint yang sudah terdaftar

| Controller | Endpoint |
|---|---|
| `NutritionOrderController` | `GET /`, `GET /screening-candidates`, `GET /{id}`, `POST /`, `PUT /{id}`, `POST /{id}/close`, `POST /{id}/cancel`, `POST /{id}/records` |
| `NutritionDietController` | `GET /patients`, `GET /history/{encounterId}`, `POST /`, `POST /{dietId}/stop`, `GET /batches`, `GET /batches/{batchId}`, `POST /batches`, `POST /batches/{batchId}/status`, `POST /distribution` |
| `NutritionMasterController` | `GET|POST /diet-types`, `GET|POST /food-forms`, `GET|POST /meal-schedules` |

Jadi alur pemesanan konsultasi gizi, catatan asuhan, diet pasien, dan produksi dapur sudah punya
permukaan API. Yang belum jelas bukan ada atau tidaknya, melainkan **apakah bentuknya sesuai
proses rumah sakit** — dan itu persis yang ditanyakan pada lembar pertanyaan.

## Dependency teknis lintas modul

Dari pembacaan `using` dan foreign key pada configuration Gizi:

| Modul yang dibutuhkan | Dipakai untuk | Sifat |
|---|---|---|
| `RegistrationManagement` | `EncounterId`, identitas kunjungan | **Keras** — tanpa ini pesanan gizi tidak punya konteks pasien |
| `HumanResource.MasterData.Workforce` | `PrescribedByWorkforceId`, `AssignedWorkforceId`, `DeliveredByWorkforceId`, `RecordedByWorkforceId` | **Keras** — pelaku tiap langkah |
| `PatientManagement.MasterData` | identitas pasien | Keras |
| `ClinicalManagement` | `TrxPatientAssessment` untuk membaca `NutritionRiskStatus` dan `NutritionRiskScore` (skrining) | **Keras** — kandidat skrining diambil dari sini |
| `MasterData` (`MstDiagnosis`) | diagnosis gizi | **Bergantung keputusan** — lihat di bawah |

### Temuan yang paling menentukan: diagnosis gizi menumpang `MstDiagnosis`

`GziNutritionCareRecord.NutritionDiagnosisId` adalah `Guid?` yang menunjuk **`MstDiagnosis`**,
bukan tabel master gizi tersendiri. Validasinya ada di
`NutritionOrderService.EnsureNutritionDiagnosisAsync`:

```csharp
x.Id == diagnosisId && !x.IsDelete && x.DiagnosisType == DiagnosisTypeNutrition
```

dengan `DiagnosisTypeNutrition = "NUTRITION"`.

Artinya sudah ada **jawaban sementara yang tertanam di kode** untuk `GIZ-OQ-002`: diagnosis gizi
dianggap baris `MstDiagnosis` bertipe `NUTRITION`. Keputusan resmi pemilik proses bisa
mengukuhkannya, atau justru menggantinya dengan master tersendiri — dan bila diganti, perubahannya
menyentuh model, configuration, service, DTO, sekaligus menuntut migration.

**Keadaan datanya kosong**, diperiksa pada database uji `QuilvianNewDevIkbalFr`:

| Tabel | Baris |
|---|---|
| `MstDiagnosis` (seluruh tipe) | **0** |
| `GziDietType`, `GziFoodForm`, `GziMealSchedule` | **0** |
| `GziNutritionOrder`, `GziPatientDiet`, `GziProductionBatch` | **0** |

Jadi belum ada satu pun data yang akan tertimpa atau bermigrasi seandainya keputusan nanti
mengubah bentuknya. Itu kabar baik: biaya berubah arah saat ini **nol**, dan akan naik begitu data
sungguhan mulai masuk.

## Sisa rename `Gz` → `Gzi` — satu temuan terbuka

Validasi penuh pada 24 September 2026:

| Ruang | Hasil |
|---|---|
| Source `.cs` | **0** sisa `Gz` |
| Snapshot | 10 `Gzi`, **0** `Gz` |
| Tabel database uji | 10 `Gzi`, **0** `Gz` |
| Index database uji | 43 `Gzi`, **0** basi |
| Konstanta frontend | 3 komentar basi — **sudah diperbaiki** |

Satu-satunya berkas yang masih memuat `Gz` adalah `20260901081830_AddNutritionManagement.cs`
beserta `*.Designer.cs`, yaitu migration yang dahulu **membuat** tabel bernama lama. Keduanya
memang tidak boleh disunting: sejarah migration harus tetap menggambarkan keadaan saat itu.

### Temuan terbuka: tiga foreign key masih bernama lama

Tabelnya sudah berganti nama, tetapi tiga constraint-nya tidak ikut:

* `FK_GzNutritionOrder_RegPatientEncounter_EncounterId`
* `FK_GzPatientDiet_RegPatientEncounter_EncounterId`
* `FK_GzProductionBatchDetail_RegPatientEncounter_EncounterId`

Sebabnya sudah dipastikan, bukan dugaan: ketiganya memang tidak pernah tercantum pada peta
pasangan nama di migration rename (`grep -c` = 0 untuk masing-masing). Jadi migration itu
melewatkannya, bukan gagal menjalankannya.

**Dampaknya nol pada perilaku** — nama constraint tidak mengubah penegakan foreign key. Yang
terganggu adalah kerapian dan pesan galat: pelanggaran FK akan menyebut nama tabel yang sudah
tidak ada.

Perbaikannya menuntut migration baru, sehingga **ditahan** sesuai aturan "jangan membuat
migration domain baru" dan diserahkan sebagai keputusan. Menggabungkannya dengan migration
Gizi yang nanti lahir sesudah keputusan bisnis turun adalah pilihan paling murah.

### Kontrak enum frontend ↔ backend

Diperiksa satu per satu, keenam enum cocok nilainya:

| Enum backend | Konstanta frontend | Cocok |
|---|---|---|
| `GziOrderStatus` 1–4 | `NUTRITION_ORDER_STATUS` | ✓ |
| `GziOrderPriority` 1–2 | `NUTRITION_ORDER_PRIORITY` | ✓ |
| `GziCareRecordType` 1–2 | `NUTRITION_RECORD_TYPE_LABEL` | ✓ |
| `GziPatientDietStatus` 1–3 | `DIET_STATUS` | ✓ |
| `GziProductionBatchStatus` 1–6 | `BATCH_STATUS` | ✓ |
| `GziMealDeliveryStatus` 1–3 | `DELIVERY_STATUS` | ✓ |

## Yang dapat dikerjakan tanpa menunggu keputusan domain

| Pekerjaan | Alasan boleh |
|---|---|
| Perbaikan bug frontend Gizi | Tidak menyentuh bentuk domain. **Sudah dikerjakan** 24 September 2026: `serverSide` diteruskan ke `FilterSelect` dan sepuluh select dilepas dari pembungkus `<label>` |
| Rename prefix `Gz` → `Gzi` | Penamaan, bukan bentuk domain. **Sudah dikerjakan**: source, snapshot, dan database uji |
| Audit dan dokumentasi seperti lembar ini | Read-only |
| Menyiapkan fixture uji untuk endpoint yang sudah ada | Tidak menetapkan aturan bisnis baru |

## Yang TIDAK boleh dikerjakan sebelum keputusan

* Entity Gizi baru, termasuk master diagnosis gizi tersendiri.
* Migration Gizi baru.
* Mengubah bentuk `GziNutritionCareRecord` atau tabel kebutuhan nutrisi.
* Menetapkan siapa yang berwenang menyetujui, baik lewat permission, enum, maupun kolom.

## Urutan sesudah jawaban tersedia

1. Terbitkan `GIZ-DEC-011` dan `GIZ-DEC-012`.
2. Perbarui `03-desain-domain.md`, naikkan statusnya kembali dari BLOCKED.
3. Desain entity — termasuk memutuskan apakah `MstDiagnosis` bertipe `NUTRITION` dipertahankan.
4. Buat migration, dengan wewenang eksekusi yang diminta terpisah.

Traceability: `GIZ-OQ-002`, `GIZ-OQ-004`, `GIZ-OQ-006`.
