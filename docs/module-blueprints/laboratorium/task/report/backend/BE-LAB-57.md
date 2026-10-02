# Laporan Perubahan Backend — `BE-LAB-57`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-57` (backend) |
| Judul | `LabFieldChangeLog` dan koreksi specimen — gelombang `MVP-7b`, slice `S4b` |
| Trace | `LAB-DEC-107`, `LAB-DEC-112` |
| Kontrak | `LAB-API-v1` `r27` bagian 21.4; `LAB-VAL-v1` `r10` (`VAL-104`, `VAL-105`, `VAL-109`, `VAL-110`); `LAB-PERM-v1` rev 9 bagian 10.3 |
| Klasifikasi | `MEDIUM` — satu tabel jejak, dua endpoint, satu service baru |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Dependency | `BE-LAB-53` ✅, `BE-LAB-55` ✅ |
| Status | ✅ **`SELESAI`** — tabel berdiri, dua endpoint berjalan, **`AC-170` dan `AC-175` terbukti terhadap database sungguhan** |

---

## 1. Kenapa koreksi dibuka sama sekali

Menutup koreksi mengulang jalan buntu yang persis dihindari `LAB-DEC-040`: satu salah pilih
saat penerimaan menahan pengisian hasil sampai petugas penerimaan tersedia — dan pada shift
malam ia mungkin tidak ada. **Yang tertahan bukan formulir, melainkan pekerjaan atas bahan yang
sudah terlanjur diambil dari tubuh pasien.**

`LAB-DEC-107` membuka koreksi dari halaman hasil; `LAB-DEC-112` menuntut nilai lamanya tetap
terbaca. Task ini mengerjakan keduanya sekaligus, sebab yang satu tanpa yang lain adalah
pertukaran yang buruk: koreksi tanpa jejak menghapus keterangan yang pernah benar.

---

## 2. Yang dibangun

| Berkas | Isi |
|---|---|
| `Models/LabFieldChangeLog.cs` | Jejak per ruas, bersumbu `EntityName` + `EntityId` |
| `Repositories/Configurations/.../LabFieldChangeLogConfiguration.cs` | **Nol cascade dari mana pun**; index `(EntityName, EntityId, ChangedAt)` |
| `Services/LabFieldChangeRecorder.cs` | Membandingkan nilai, mencatat **hanya yang berubah** |
| `DTOs/LabSpecimenCorrectionDtos.cs` | `LabSpecimenCorrectionRequest`, `LabFieldChangeResponse` |
| `Services/LabSpecimenCorrectionService.cs` | `ApplyCorrectionAsync`, `GetFieldChangesAsync` |
| `Controllers/LabSpecimenController.cs` | `PATCH /{id}/correction`, `GET /{id}/field-changes` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<LabFieldChangeLog>` |
| `Program.cs` | Dua pendaftaran `Scoped` |
| `Migrations/20260921061227_AddLabFieldChangeLog.cs` | Dibangkitkan |

### Kenapa `PATCH`, bukan `PUT`

Halaman hasil mengoreksi **sebagian** — biasanya satu ruas yang keliru. `PUT` menuntut pemanggil
mengirim seluruh isi specimen, dan ruas yang lupa disertakan akan terhapus diam-diam. Roadmap
menandai ini sebagai risiko utama task; bentuknya dijawab langsung: **seluruh ruas request
opsional, yang tidak dikirim tidak disentuh.**

### Kenapa tabel jejak terpisah dari `LabTransitionHistory`

`LabTransitionHistory` mencatat **perpindahan keadaan** — Planned → Collected → Received. Jejak
ruas mencatat **perubahan isi** pada keadaan yang sama. Menumpuk keduanya pada satu tabel
memaksa `FromStatus`/`ToStatus` diisi nilai palsu setiap kali sebuah keterangan diperbaiki.
`LAB-DEC-112` memisahkannya, dan `AC-175` mengujinya secara langsung.

### Penyimpangan dari cakupan roadmap yang ditulis sengaja

Roadmap menulis `ApplyCorrectionAsync` **pada `LabSpecimenService`**. Berkas itu sudah lebih
dari 2.100 baris. Implementasinya ditaruh pada `LabSpecimenCorrectionService` tersendiri;
**permukaannya tetap satu controller**, sehingga kontrak `r27` bagian 21.4 nol berubah.

---

## 3. Satu cacat ditemukan dan diperbaiki di tengah pengujian

Pengiriman ulang nilai yang **sama persis** tetap mencatat perubahan:

```
FieldName=VolumeAmount | OldValue=5.000 | NewValue=5
```

Sebabnya kolom `VolumeAmount` bertipe `numeric(12,3)`: angka 5 yang tersimpan terbaca kembali
sebagai `5.000`, dan perbandingan apa adanya sebagai teks menyatakan keduanya berbeda.

Ini melanggar DoD task itu sendiri — *"hanya ruas yang benar-benar berubah yang tercatat"* — dan
justru jejak semacam itulah yang `LabFieldChangeRecorder` katakan ingin dihindari. Diperbaiki
dengan `FormatVolume` yang membuang nol di belakang koma pada **kedua** sisi perbandingan,
bukan dengan pengecualian khusus pada pembanding.

Sesudah perbaikan, permintaan yang sama menjawab **`"Nol ruas yang berubah."`** dengan
`ruasBerubah: 0`.

---

## 4. Verifikasi

### 4.1 Terhadap database

| Pemeriksaan | Hasil |
|---|---|
| Tabel `LabFieldChangeLog` | ✅ 18 kolom sesuai rancangan |
| Constraint | ✅ **hanya `PK_LabFieldChangeLog`** — nol foreign key, sesuai keputusan nol cascade |
| Index | ✅ `("EntityName", "EntityId", "ChangedAt")` |
| `Down` migration | ✅ `DropTable` |

### 4.2 `AC-175` — satu koreksi, satu baris jejak, **nol** baris `LabTransitionHistory`

Sepuluh permintaan koreksi dijalankan terhadap aplikasi yang berjalan.

| Cacah | Sebelum | Sesudah |
|---|---|---|
| `LabFieldChangeLog` | 0 | 9 |
| `LabTransitionHistory` | 58 | 58 |

Satu-satunya baris `LabTransitionHistory` yang bertambah sesudahnya berasal dari `reopen`
(`Action=LabExamination.ReopenMicrobiologyResult`), **bukan dari koreksi**. ✅

Koreksi satu ruas menjawab `"1 ruas tercatat"` dan menambah tepat satu baris. ✅

### 4.3 `AC-170` — berjejak, dan baca-saja sesudah Final

| Langkah | Hasil |
|---|---|
| Koreksi **sebelum** Final | ✅ `200`, tercatat |
| `finalize` pemeriksaan | ✅ `200`, `finalizedAt` terisi |
| Koreksi **sesudah** Final | ✅ **`422`** *"Hasil sudah dinyatakan selesai; buka kembali hasilnya lebih dulu sebelum mengoreksi specimen."* (`VAL-109`) |
| `reopen` | ✅ `200`, `reopenCount: 1` |
| Koreksi **sesudah** reopen | ✅ `200`, tercatat kembali |

### 4.4 Validasi dan hak akses

| Uji | Hasil |
|---|---|
| `VAL-104` jenis `Lainnya` tanpa keterangan | ✅ `422` |
| `VAL-105` rincian dikirim ganda | ✅ `422` |
| `VAL-110` satuan bukan `IsForLaboratory` | ✅ `422` |
| `BR-37` waktu terima di masa depan | ✅ `422` |
| Specimen tidak ada | ✅ `404` |
| Tanpa kredensial | ✅ `401` |

**Rollback terbukti:** keempat permintaan yang ditolak menambah **nol** baris jejak.

### 4.5 Jejak terbaca dan tersekat per wadah

`GET /{id}/field-changes` menjawab urut terbaru lebih dahulu, bernama ruas Bahasa Indonesia
(`"Jenis Specimen"`, `"Spesifik Specimen"`), dan menyimpan **nilai yang terbaca manusia** —
`oldValue: "Jaringan"`, `newValue: "Blood"`, bukan GUID.

Wadah kedua menjawab **hanya satu barisnya sendiri** walaupun tabelnya berisi sembilan baris,
membuktikan penyekatan lewat `EntityName` + `EntityId` bekerja.

---

## 5. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Jejak ruas untuk entity selain `LabSpecimen` | Bentuk tabelnya sudah umum; pemakainya belum ada |
| Pembatalan satu baris jejak | Jejak yang dapat dihapus bukan jejak |
| Penyeragaman `Kind` pada `PhysicallyReceivedAt` | Codebase nol punya konvensi itu; mengadakannya di sini akan menyimpang sendiri |
| `git add`, `commit`, `push` | Nol diminta |

> **Baris uji tertinggal di dev:** sembilan baris `LabFieldChangeLog` pada dua wadah, termasuk
> **satu baris menyesatkan** `VolumeAmount 5.000 → 5` yang dihasilkan build sebelum perbaikan
> bagian 3 — perubahan itu **tidak pernah benar-benar terjadi**. Wadah
> `78b09087-…` kini bertanda jenis `Blood` dengan dua Spesifik Specimen, dan wadah
> `53e9b328-…` berketerangan uji. Pemeriksaan `6fa8a2b6-…` bertanda `reopenCount: 1`.
