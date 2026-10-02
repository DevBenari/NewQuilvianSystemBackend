# Laporan Perubahan Backend — `BE-LAB-55`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-55` (backend) |
| Judul | Data induk Spesifik Specimen — gelombang `MVP-7`/`MVP-7b`, slice `S4b` |
| Trace | `LAB-DEC-098`, `099`, `129`, `130`, `131`, `132`; menegakkan `LAB-DEC-040`, `LAB-DEC-008` |
| Kontrak | `LAB-API-v1` `r27` bagian 21.5, diperluas `r28` bagian 23.2 dan 23.3; `LAB-VAL-v1` `r9` (`VAL-104`, `VAL-105`, `VAL-111`) |
| Klasifikasi | `MEDIUM` — dua tabel, delapan endpoint, satu seeder 1.767 baris |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Status | ✅ **`SELESAI`** — **1.767 baris ter-seed; idempotensi terbukti dengan TIGA kali menjalankan aplikasi** |

---

## 1. Penahan yang dibuka pemilik modul

Task ini tertahan sejak `LAB-OPEN-040`: seeder membutuhkan berkas dataset **di dalam
repository**, dan itu penulisan ke source. **Pemilik modul mengizinkannya pada 2026-09-21.**

Dataset `snomedct_specimen_dikelompokan_berdasarkan_jenis.xlsx` diekstrak menjadi CSV, bukan
disalin mentah — rancangan bagian 18.2 menuntut kolom `snomed_code`, `name_en`,
`specimen_type_code`, `sub_type_name`, dan `is_active`.

---

## 2. Yang dibangun

| Berkas | Isi |
|---|---|
| `Seeders/Data/lab-specimen-detail-types.csv` | **1.767 baris**, 172 KB, **tertanam di DLL** |
| `QuilvianSystemBackend.csproj` | Entri `EmbeddedResource` beserta alasannya |
| `Models/LabSpecimenDetailType.cs` | Dua model: `LabSpecimenDetailType` dan `LabSpecimenDetail` |
| `Repositories/Configurations/.../LabSpecimenDetailTypeConfiguration.cs` | Dua configuration |
| `Seeders/LabSpecimenTypeSeeder.cs` | **Diperluas 7 → 31 kelompok** |
| `Seeders/LabSpecimenDetailTypeSeeder.cs` | Pembaca CSV tertanam |
| `DTOs/LabSpecimenDetailTypeDtos.cs` | 5 DTO |
| `Services/LabSpecimenDetailTypeService.cs` | CRUD, empat penyaring, ringkasan |
| `Controllers/LabSpecimenDetailTypeController.cs` | **7 endpoint** |
| `Migrations/20260921051028_AddLabSpecimenDetailType.cs` | Dibangkitkan |

### Perluasan 7 → 31 aman, dan itu diperiksa bukan diasumsikan

**GUID ketujuh baris lama dipertahankan.** Ketujuhnya punya padanan di antara 31 kelompok —
`BLOOD`→Darah, `BODYFLUID`→Cairan Tubuh, dan seterusnya — sehingga ini penambahan 24 baris
beserta penyelarasan nama. `LabSpecimen.SpecimenTypeId` yang sudah menunjuk ketujuhnya **nol
perlu dipetakan ulang**.

### Kode baris memakai `SP-{snomed}`, bukan kode SNOMED telanjang

`LAB-DEC-132`. Baris lokal dari jalan keluar `Lainnya` nol punya kode SNOMED; memakainya
sebagai kode utama berarti baris lokal diberi **kode karangan di dalam kolom yang dianggap
standar internasional**.

### Index unik `SnomedCode` punya DUA pembatas

```sql
... WHERE (("IsDelete" = false) AND ("SnomedCode" IS NOT NULL))
```

Pembatas kedua **wajib**: baris lokal seluruhnya berkode SNOMED kosong, dan tanpa
`IS NOT NULL` baris lokal **kedua** akan ditolak.

---

## 3. Verifikasi

### 3.1 Seeder — dijalankan TIGA kali

| Jalan ke | Jenis specimen | Rincian | Aktif | Nonaktif |
|---|---:|---:|---:|---:|
| **Pertama** | 31 | **1.767** | **1.601** | **166** |
| **Kedua** | 31 | **1.767** | 1.601 | 166 |
| **Ketiga** | 31 | **1.767** | 1.601 | 166 |

✅ **Nol duplikat.** Angka 1.601/166 cocok persis dengan `LAB-DEC-130`.

### 3.2 ⭐ Jebakan terbesar — terjemahan bertahan melewati restart

Roadmap 6l memperingatkan: *seeder yang "menyegarkan" isinya setiap aplikasi menyala akan
**menghapus seluruh pekerjaan penerjemahan** setiap restart, dan nol galat muncul.*

Diuji langsung:

1. Baris `SP-122552005` diberi nama Indonesia **`Darah arteri`** lewat `PUT`.
2. Aplikasi **dimatikan dan dinyalakan ulang** — seeder berjalan untuk ketiga kalinya.
3. Dibaca ulang dari database:

```text
rincian = 1767 | terjemahan = Darah arteri
```

✅ **Bertahan.** Seeder hanya menyisipkan yang kodenya belum ada.

### 3.3 Endpoint dan penyaring

| Uji | Hasil |
|---|---|
| `GET /summary` | ✅ `total 1767, aktif 1601, nonaktif 166, belumDiterjemahkan 1767` |
| `GET /` tanpa penyaring | ✅ `1601` — hanya aktif |
| `?includeInactive=true` | ✅ `1767` |
| `?untranslatedOnly=true` | ✅ `1600` — turun satu sesudah terjemahan diisi |
| `?subTypeName=Darah / produk darah` | ✅ `40` |
| Pencarian **Indonesia** `Darah arteri` | ✅ `1`, `displayName: Darah arteri` |
| Pencarian **Inggris** `Arterial blood` | ✅ `1` |
| `GET /options?labSpecimenTypeId=BLOOD` | ✅ **54** baris |
| `GET /options?labSpecimenTypeId=TISSUE` | ✅ **434** baris |
| `GET /options` tanpa jenis | ✅ `400` — jenis wajib dipilih |
| `PUT` mengisi terjemahan | ✅ `isUntranslated` berubah menjadi salah |

**54 dan 434 cocok persis** dengan sebaran `LAB-EVD-007`.

### 3.4 Index terverifikasi terhadap database

Ketiganya unik dan **parsial**: `DetailTypeCode`, `SnomedCode` (dengan pembatas ganda), dan
`(LabSpecimenId, LabSpecimenDetailTypeId)` pada tabel jembatan.

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Penerjemahan 1.767 nama | `LAB-DEC-131` — bertahap, oleh kepala instalasi. Seeder **nol menebaknya** |
| Penyaringan isi awal oleh kepala instalasi | `LAB-DEC-130` — ia memangkas daftar yang sudah jalan; 166 baris `Rendah` sudah nonaktif sejak awal |
| Pemakaian `LabSpecimenDetail` pada halaman hasil | `BE-LAB-57` beserta endpoint koreksi specimen |
| `git add`, `commit`, `push` | Nol diminta |

> **Satu baris uji tertinggal:** `SP-122552005` kini bernama Indonesia `Darah arteri`.
> Terjemahan itu **benar**, tetapi dibuat untuk pengujian — kepala instalasi boleh
> mempertahankannya atau menggantinya.
