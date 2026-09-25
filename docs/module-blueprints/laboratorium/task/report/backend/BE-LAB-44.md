# Laporan Perubahan Backend — `BE-LAB-44`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-44` (backend) |
| Judul | Dua data induk Mikrobiologi: `LabOrganism` dan `LabAntibiotic` — gelombang `MVP-6a`, slice `S4b` |
| Trace | `FR-13.6`, `FR-13.7`; `LAB-DEC-084`; `LAB-DC-041`, `LAB-DC-042`; `INV-31` |
| Kontrak | `LAB-API-v1` `r24` bagian 19.4; `LAB-PERM-v1` rev 6 bagian 8.2; `LAB-VAL-v1` `r7` (`VAL-91`) |
| Klasifikasi | `MEDIUM` |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-18 |
| Status | **✅ `SELESAI`** — dua tabel berdiri, delapan endpoint berjalan, migration **diterapkan**; `AC-98`..`AC-100` terbukti pada database **dan** terhadap aplikasi yang berjalan |

---

## 1. Satu selisih terhadap roadmap, diputuskan lewat urutan wewenang

**Cakupan roadmap menulis: *"CRUD sederhana memakai `ApplicationDbContext` langsung sesuai
konvensi — **tanpa** service"*. Itu tidak dikerjakan.**

`AGENTS.md` menetapkan urutan wewenang: (1) task, (2)
`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, (3) registry, (4) `rules/backend/`,
(5) pola source existing. Kontrak rekayasa pada tingkat (2) menyatakan:

> *"Alur baru adalah Controller → Module Service → DbContext/infrastruktur bersama/integrasi
> eksternal."*

dan `AGENTS.md` sendiri menambahkan bahwa **controller yang mengakses DbContext secara langsung**
adalah pola legacy yang **tidak memberi wewenang** untuk dipakai pada `NEW CODE`.

Baris **Reuse** pada roadmap yang sama juga mengandaikan service: *"Pola `LabSpecimenType`
**sepenuhnya**"* — dan `LabSpecimenType` punya `LabSpecimenTypeService`. Kedua baris roadmap itu
bertentangan satu sama lain, dan kontrak rekayasa memutus pertentangannya.

**Yang dikerjakan:** dua service (`LabOrganismService`, `LabAntibioticService`) dalam satu berkas,
berbagi pembantu teks dan kalimat penolakan. Selisih dilaporkan, bukan didiamkan.

---

## 2. Yang dibangun

| Berkas | Isi |
| --- | --- |
| `Models/LabOrganism.cs` | `OrganismCode` (32, unik parsial), `OrganismName` (200), `SortOrder`, `IsActive`, `Description` (256) |
| `Models/LabAntibiotic.cs` | Bentuk kembar, dengan awalan `Antibiotic` |
| `Repositories/.../LabOrganismConfiguration.cs` | Index unik **parsial** pada kode; index nama; index `(IsActive, SortOrder)` |
| `Repositories/.../LabAntibioticConfiguration.cs` | Sama |
| `DTOs/LabMicrobiologyMasterDataDtos.cs` | 10 DTO — 5 per resource |
| `Services/LabMicrobiologyMasterDataService.cs` | Dua service + pembantu bersama + satu exception |
| `Controllers/LabOrganismController.cs` | 4 endpoint |
| `Controllers/LabAntibioticController.cs` | 4 endpoint |
| `Migrations/20260918085707_AddLabMicrobiologyMasterData.cs` | Dua tabel |

### 2.1 Kembarannya dipertahankan eksplisit, bukan diabstraksikan

Kedua service hampir identik, dan itu menggoda untuk disatukan menjadi satu service generik.
**Tidak dilakukan, dan alasannya teknis:** nama kolomnya berbeda (`OrganismCode` versus
`AntibioticCode`), sehingga abstraksi lewat antarmuka membuat properti yang dipakai pada `Where`
dan `OrderBy` **nol dapat diterjemahkan EF** menjadi SQL. Yang disatukan hanyalah bagian yang
memang aman disatukan: normalisasi kode, pengenalan bentrokan, pembentukan halaman, dan
**kalimat penolakan `VAL-91`** — yang terakhir justru penting, sebab kalimat yang berbeda
antar-resource membuat satu jalur terasa menerima apa yang jalur lain tolak.

### 2.2 `LabAntibiotic` bukan data induk obat

Ditulis pada model, controller, dan kamus data sekaligus. Kesamaan namanya dengan formularium
farmasi menyesatkan; penautan keduanya adalah keputusan tersendiri yang **belum** diambil, dan
nol boleh disimpulkan dari kemiripan nama. Ini hal yang paling mungkin "dirapikan" pemelihara
berikutnya dengan niat baik.

---

## 3. Verifikasi

| # | Pemeriksaan | Hasil |
| ---: | --- | --- |
| 1 | `dotnet build` | ✅ **0 error**; **nol warning** dari berkas baru |
| 2 | Migration hanya dua tabel baru | ✅ 2 `CreateTable`; **0** `AddColumn`/`AlterColumn`/`DropColumn` |
| 3 | Dua tabel berdiri pada database | ✅ `LabOrganism`, `LabAntibiotic` |
| 4 | Index unik pada kedua kode **berbentuk parsial** | ✅ **2/2**, predikat `("IsDelete" = false)` |
| 5 | **`AC-98`** `POST` berulang kode sama → `409` | ✅ keduanya `409` dengan kalimat `VAL-91` |
| 6 | **`AC-98b`** normalisasi kode | ✅ `  zZtEsT-oRg  ` ditolak `409` — sama dengan `ZZTEST-ORG` |
| 7 | **`AC-99`** nonaktif **tidak hilang** dan tetap terbaca | ✅ `IsActive=false`, `IsDelete=false`; kelola **1**, pilihan analis **0** |
| 8 | **`AC-100`** nol endpoint `DELETE` | ✅ `405` pada kedua resource |
| 9 | Kode tidak ikut berubah lewat `PUT` | ✅ tetap `ZZTEST-ORG` sesudah dua kali `PUT` |
| 10 | `PUT` pada id tak dikenal | ✅ `404` |
| 11 | Nol kode ganda lolos ke database | ✅ `0 baris` |

### 3.1 Keluaran database — read-only

```text
===== 1. KEDUA TABEL BERDIRI =====
LabAntibiotic / LabOrganism                      (2 baris)

===== 2. INDEX UNIK PADA KEDUA KODE - wajib PARSIAL =====
IX_LabAntibiotic_AntibioticCode | ("IsDelete" = false)
IX_LabOrganism_OrganismCode     | ("IsDelete" = false)

===== 3. AC-99 - baris nonaktif TIDAK hilang =====
ZZTEST-ORG | Escherichia coli (baris uji BE-LAB-44) | IsActive=False | IsDelete=False
ZZTEST-AB  | Ceftriaxone (baris uji BE-LAB-44)      | IsActive=False | IsDelete=False

===== 4. AC-98 - adakah kode ganda yang lolos? =====
(0 baris)
```

**Baris ketiga adalah inti `AC-99`**: `IsActive=False` tetapi `IsDelete=False`. Baris itu
dinonaktifkan, **bukan dihapus** — dan isolat yang kelak menunjuk ke sana tetap dapat dibaca
(`INV-31`).

### 3.2 `AC-99` lewat API — pemisahan dua audiens

```text
GET /          (layar kelola kepala instalasi) -> totalData = 1   <- nonaktif TETAP terbaca
GET /options   (pilihan analis)                -> totalData = 0   <- nonaktif TIDAK dapat dipilih
```

Tanpa pemisahan ini, salah satu dari dua hal buruk terjadi: analis dapat memilih kuman yang sudah
ditarik, **atau** kepala instalasi nol dapat mengaktifkannya kembali karena barisnya tidak
terlihat di mana pun.

> **Catatan pembacaan:** putaran pertama pengujian sempat melaporkan keduanya `False`. Itu
> **cacat skrip PowerShell** — `.Count` pada hasil `Where-Object` yang berisi satu objek — bukan
> cacat kode. Dibuktikan ulang langsung ke database dan lewat `totalData`, dan keduanya benar.

---

## 4. Satu pengamatan, bukan cacat

`POST` dengan kode kosong menjawab **`400` berpesan kosong**, bukan `400` berpesan
*"Kode organisme wajib diisi."* — sebab `[Required]` pada DTO menembak lebih dulu daripada
pemeriksaan service.

**Tidak diubah, dan itu keputusan sadar.** Berbeda dari kasus `VAL-97` pada `BE-LAB-52` — di sana
kontrak menetapkan `422` beserta kalimat tertentu bagi pengguna, sehingga penjaga bawaan membuat
aturan yang disetujui **nol pernah tercapai**. Di sini **nol aturan `VAL` mengatur isian kosong**,
dan perilakunya **identik dengan `LabSpecimenType`** yang sudah berjalan. Mengubahnya berarti
menyimpang dari pola yang direuse tanpa dasar kontrak.

---

## 5. Data uji yang tertinggal di dev

| Tabel | Baris | Keadaan |
| --- | --- | --- |
| `LabOrganism` | `ZZTEST-ORG` — *Escherichia coli (baris uji BE-LAB-44)* | **Nonaktif** |
| `LabAntibiotic` | `ZZTEST-AB` — *Ceftriaxone (baris uji BE-LAB-44)* | **Nonaktif** |

Keduanya sengaja diberi awalan `ZZTEST-` dan keterangan yang menyebut dirinya baris uji, lalu
**dinonaktifkan di akhir pengujian** sehingga nol muncul pada daftar pilihan analis. **Nol
`DELETE` tersedia** (`AC-100`), jadi keduanya hanya dapat dibersihkan lewat SQL langsung oleh
pemilik repository, atau dibiarkan nonaktif.

**Daftar pilihan analis saat ini berisi 0 baris** — artinya `MVP-6c` masih tertahan pekerjaan yang
bukan pekerjaan programmer; lihat bagian 6.

---

## 6. Penahan yang tersisa, dan ia bukan kode

Roadmap bagian 6i.7 sudah memperingatkannya: **tabel berdiri tidak berarti terisi.**
`BE-LAB-44` menyelesaikan *cara mengisinya* — delapan endpoint, lengkap dengan jalur tulis sejak
hari pertama, persis supaya `LAB-COORD-006` dan `MST-POS-WRITE` tidak terulang untuk ketiga
kalinya.

Yang belum: **daftar organisme dan antibiotik yang sebenarnya**. Itu milik kepala instalasi
bersama `DR-LAB-002` (Mikrobiologi Klinik), bukan milik programmer. Selama kosong,
`BE-LAB-48` dapat dibangun tetapi layar pencatatan isolat nol punya kuman untuk dipilih.

---

## 7. Langkah berikutnya

| # | Langkah | Pemilik |
| ---: | --- | --- |
| 1 | Isi daftar organisme dan antibiotik yang sebenarnya | Kepala instalasi + `DR-LAB-002` |
| 2 | Bersihkan dua baris `ZZTEST-` bila dikehendaki | Pemilik repository (perlu SQL — nol `DELETE`) |
| 3 | `BE-LAB-45` — perluasan bentuk hasil, **sudah dipersempit** menjadi satu kolom | Berikutnya |
| 4 | `BE-LAB-47`, `BE-LAB-48`, `BE-LAB-49` menyusul | Setelah `BE-LAB-45` |

**Perubahan tidak terkait di working tree dipertahankan.** Nol `git add`, nol commit, nol push.
