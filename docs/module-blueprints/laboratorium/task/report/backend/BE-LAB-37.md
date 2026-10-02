# Laporan Perubahan Backend — `BE-LAB-37`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-37` |
| Judul | `r16`: nomor order terbaca dan waktu pengambilan yang dinyatakan |
| Slice | `MVP-5g`, `EPIC-LAB-11` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6h |
| Trace | `LAB-DEC-072`; **`LAB-CONFLICT-006` pilihan A**, diputuskan 2026-09-16 |
| Contract version | `LAB-API-v1` **`r16`** bagian 11 — **`approved` 2026-09-17** |
| Dependency | `BE-LAB-36` ✅ |
| Klasifikasi | `LIGHT` — dua ruas respons, satu ruas permintaan, empat proyeksi. Nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `13665452` (belum di-stage maupun di-commit) |
| Tanggal | 2026-09-17 |
| Status | **`SELESAI`** — 5 pemeriksaan runtime, seluruhnya sesuai. **Satu batas dilaporkan**: `collectedAt` dipakai sebagai pembanding, belum disimpan — lihat bagian 6 |

---

## 1. Masalah yang diperbaiki

Dua celah yang tidak berhubungan, digabung karena keduanya menyasar revisi kontrak yang sama.

**Pertama.** `BE-LAB-36` mendirikan `LabOrder.OrderNumber` — `NOT NULL`, unik, kedelapan pesanan
lama terisi. Tetapi **nol DTO mengembalikannya**, sehingga nomor itu tidak dapat dibaca siapa pun
di luar backend. Ini kemunculan ketiga pola "sesuatu berdiri tanpa jalan keluar" pada modul ini.

**Kedua.** `VAL-59` membandingkan waktu penerimaan fisik terhadap waktu pengambilan —
`ResolvePhysicalReceipt` **sudah mengimplementasikannya dengan benar sejak `BE-LAB-22`** — tetapi
pemanggilnya meneruskan `collectedAt: null` secara harfiah:

```csharp
// VAL-58 dan VAL-59. Wadah baru belum memiliki waktu pengambilan tersimpan, sehingga
// yang benar-benar dapat ditegakkan di sini hanya VAL-58.
var physicallyReceivedAt = ResolvePhysicalReceipt(
    request.PhysicallyReceivedAt,
    collectedAt: null,
    now);
```

Aturannya ada, kodenya benar, **pembandingnya yang tidak pernah datang**. `AC-66` karena itu
terpenuhi separuh sejak 2026-09-15.

---

## 2. Proses bisnis

| Yang berubah | Dari sisi pengguna |
| --- | --- |
| Nomor order terbaca | Daftar pesanan dan ketiga menu Pemeriksaan kini dapat menampilkan `LAB-RSMMC-000006` — nomor yang dapat disebut lewat telepon, bukan GUID |
| Waktu pengambilan dinyatakan | Petugas penerimaan dapat mencatat jam berapa sampel diambil **di klinik perujuk**, dan sistem menolak bila waktu kedatangan lebih awal daripada waktu pengambilan |

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `DTOs/LabOrderDtos.cs` | **+1 ruas** `OrderNumber` pada `LabOrderListResponse` |
| `DTOs/LabMonitoringDtos.cs` | **+1 ruas** `OrderNumber` pada `LabMonitoringItemResponse` |
| `DTOs/LabSpecimenDtos.cs` | **+1 ruas** `CollectedAt` opsional pada `PlanLabSpecimenRequest` |
| `Services/LabOrderService.cs` | **4 titik**: anonymous projection daftar, `LabOrderListResponse`, proyeksi detail, dan `MapDetailResponse` |
| `Services/LabMonitoringService.cs` | **+1** pada proyeksi daftar pantau |
| `Services/LabSpecimenService.cs` | Pembanding `VAL-59` diteruskan dari permintaan |

### 3.1 Satu hal yang hampir terlewat

Proyeksi daftar pesanan melewati **anonymous type** lebih dulu, lalu dipetakan ke
`LabOrderListResponse`. Menambahkan ruas hanya pada pemetaan akhir menghasilkan galat kompilasi —
yang untungnya **keras**, bukan senyap. Ruasnya wajib ditambahkan pada **kedua** tahap.

### 3.2 Kenapa dua DTO, bukan satu

Ini pelajaran `r13`/`r14` yang diterapkan **sejak awal**, bukan diulang lalu dikoreksi.

`r13` menambahkan ruas pada DTO yang **paling masuk akal namanya** dan ternyata nol dipakai layar;
`r14` harus memperbaikinya pada hari yang sama. Penempatan di sini diperiksa dari **source
konsumennya** lebih dulu: ketiga menu Pemeriksaan membaca grup **Lab Monitoring**, bukan
`LabOrderListResponse`.

---

## 4. Endpoint yang berubah

Nol endpoint ditambah, dihapus, atau berubah bentuknya. Yang berubah adalah **isi respons** dan
**satu ruas opsional pada permintaan**.

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Yang berubah |
| --- | --- | --- |
| `GET` | `/v1/.../lab-orders` | Respons memuat `orderNumber` |
| `GET` | `/v1/.../lab-orders/{id}` | Respons memuat `orderNumber` (diwarisi) |

#### Health Services / Laboratory Management / Lab Monitoring

| Method | Path | Yang berubah |
| --- | --- | --- |
| `GET` | `/v1/.../lab-monitoring/{discipline}` | Respons memuat `orderNumber` |

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Yang berubah |
| --- | --- | --- |
| `POST` | `/v1/.../lab-specimens/by-order/{labOrderId}` | Menerima `collectedAt` opsional; `VAL-59` kini dapat menyala |

---

## 5. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build -p:RunAnalyzers=False` | **0 error, 0 warning.** Penyaringan keluaran atas keenam berkas yang disentuh menghasilkan **kosong** |

### 5.1 Lima pemeriksaan dijalankan sungguhan terhadap `QuilvianNewDevYoga`

| # | Pemeriksaan | Hasil |
| ---: | --- | --- |
| `R1` | `orderNumber` pada `GET /lab-orders` | `LAB-RSMMC-000006`, `000007`, `000008` — **`PASS`** |
| `R2` | `orderNumber` pada daftar pantau — **jalur yang benar-benar dipakai ketiga menu** | `000006`, `000003`, `000002` beserta nama pasien — **`PASS`** |
| `R3` | `VAL-59` — penerimaan **lebih awal** daripada pengambilan | **`422`** *"Waktu penerimaan tidak boleh lebih awal daripada waktu pengambilan."* — kata demi kata sesuai matriks — **`PASS`** |
| `R4` | Rentang sah — penerimaan sesudah pengambilan, keduanya lampau | **`201`** diterima — **`PASS`** |
| `R5` | `collectedAt` **kosong** — muatan lama | **`201`** diterima; `VAL-59` **tidak menyala** — **`PASS`** |

**`R5` adalah pemeriksaan yang menguji ketiadaan**, dan ia menentukan. Ketika `collectedAt` tidak
dikirim, aturannya **wajib diam** — bukan menolak, dan bukan pula membandingkan terhadap cap waktu
server yang justru lebih akhir. Tanpa pemeriksaan ini, cabang itu tidak akan pernah dijalankan
satu kali pun.

> **Satu kekeliruan pemeriksaan dicatat.** `R4` versi pertama memakai `05:00Z` sebagai waktu
> penerimaan, dan ditolak `VAL-58` karena `05:00Z` masih **di masa depan** pada saat pengujian —
> pukul 03:24Z. Nilai ujinya yang keliru, bukan kodenya. Diulang dengan dua waktu lampau dan
> lulus. Dicatat karena kekeliruan yang sama mudah terulang pada pengujian bergantung waktu.

### 5.2 Kebersihan database

Satu pesanan uji beserta tiga wadah dan jejaknya dihapus. Keadaan akhir diperiksa dari koneksi
baru: **8 pesanan, 5 wadah, nomor tertinggi `LAB-RSMMC-000008`** — persis seperti sebelum task ini.

---

## 6. Batas yang dilaporkan: `collectedAt` belum disimpan

Ruasnya dipakai sebagai **pembanding `VAL-59`**, dan nilainya **tidak disimpan**.

**Alasannya bukan kelalaian.** Menyimpannya ke `LabSpecimen.CollectedAt` akan **ditimpa** tindakan
pengambilan, yang menyetel kolom itu tanpa syarat:

```csharp
if (target == LabSpecimenStatus.Collected)
{
    specimen.CollectedAt = now;
    ...
}
```

Menyimpannya di sana karena itu justru **menghapus data yang hendak diselamatkan**. Tiga jalan
keluarnya, dan ketiganya menuntut keputusan:

| Pilihan | Akibat |
| --- | --- |
| Simpan lalu buat tindakan pengambilan mempertahankan nilai yang ada | Mengubah arti kolom pada jalur **dalam gedung**, di mana `CollectedAt` memang seharusnya momen pengambilan sebenarnya |
| Kolom tersendiri `DeclaredCollectedAt` | Benar secara makna, tetapi menuntut **migration** — di luar cakupan task ini yang bertanda nol migration |
| Biarkan sebagai pembanding saja | Yang dikerjakan sekarang. `VAL-59` dan `AC-66` tegak penuh, tetapi motivasi `LAB-CONFLICT-006` — *"waktu pengambilan hilang, tidak tersimpan di mana pun"* — **belum sepenuhnya terjawab** |

**Amandemen `r16` yang disetujui menjanjikan `VAL-59` dan `AC-66` tegak penuh, dan itu tercapai.**
Ia tidak menjanjikan penyimpanan. Perbedaan itu ditulis di sini supaya tidak tertukar, dan
keputusan penyimpanannya diserahkan kepada pemilik modul.

---

## 7. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kedua ruas respons terbaca dari database | **Terpenuhi** | `R1`, `R2` |
| `collectedAt` tersimpan | **Tidak terpenuhi, dan dilaporkan** | Bagian 6 |
| `VAL-59` tegak beserta pesannya kata demi kata | **Terpenuhi** | `R3` |
| Muatan lama tetap diterima | **Terpenuhi** | `R5` |

| AC | Status |
| --- | --- |
| `AC-66` | **Terpenuhi penuh** pada penegakan aturannya — naik dari "terpenuhi separuh" |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning baru |
| Masalah yang diketahui | Penyimpanan `collectedAt` — bagian 6 |
| Dependency | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Nol `git add`, `commit`, maupun `push` |
| Langkah berikutnya | `BE-LAB-38` melaksanakan `r17`, lalu `FE-LAB-12` terbuka |
