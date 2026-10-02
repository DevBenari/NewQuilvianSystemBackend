# Laporan Perubahan Backend — `BE-LAB-22`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-22` |
| Judul | Waktu penerimaan fisik |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6b |
| Trace | `FR-11.5`; `LAB-DEC-042`, BR-37, `RULE-019` bagian specimen; `AC-65`, `AC-66`, `AC-67`, `AC-17` sebagai regresi; `VAL-58`, `VAL-59` |
| Contract version | `LAB-API-v1` `r7` perluasan `POST /lab-specimens/by-order/{labOrderId}`; `LAB-VAL-v1` `r4` — keduanya `approved`, disetujui pemilik modul 2026-09-14 |
| Dependency | `BE-LAB-21` ✅ **`SELESAI` 2026-09-15**, migrationnya diterapkan bersama task ini pada tabel yang sama |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium, configuration, migration, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9067fa73`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **`SELESAI SEBAGIAN`** — 2026-09-15. Source, migration, **dan eksekusi ke `QuilvianNewDevYoga`** selesai; jalur `Down` ikut dibuktikan. `AC-65`, `AC-67`, dan `AC-17` terpenuhi; `AC-66` **terpenuhi separuh** karena `VAL-59` **tidak dapat ditegakkan** pada kontrak `r7` — lihat bagian 6 |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 — `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 21 |
| Keberlakuan | `NEW CODE` untuk kolom, index, ruas DTO, dan aturan validasinya. Tidak ada entity baru |
| Status gerbang | `QBE-MOD-002` dan `QBE-MOD-003` **tidak menahan** |
| QBE ID yang berlaku | `QBE-ENT-002`, `QBE-CFG-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-AUD-001` |

---

## 1. Masalah yang diperbaiki

Sampel dari Klinik Sehat Sentosa tiba di meja penerimaan pukul **21.10 hari Senin**.
Laboratorium sudah tutup untuk layanan umum, dan registrasinya baru dikerjakan **Selasa pukul
08.05**.

Sebelum perubahan ini, sistem hanya menyimpan satu waktu: `ReceivedAt`, yang diisi server pada
saat petugas menekan tindakan penerimaan. Akibatnya sampel itu tercatat **datang hari Selasa**.
Laporan penerimaan harian salah menempatkannya, pertanyaan "sampel ini sudah berapa lama di
laboratorium" dijawab sebelas jam terlalu pendek, dan tidak ada satu pun cara menelusuri bahwa
selisih itu pernah ada.

Yang hilang bukan sekadar akurasi tanggal. Yang hilang adalah kemampuan menjawab pertanyaan
paling wajar di kemudian hari: **apakah sampel ini terlambat dicatat, dan berapa lama.**

---

## 2. Proses bisnis

**Pelaku.** Petugas penerimaan sampling/specimen.
**Pemicu.** Sebuah wadah fisik dicatat pada sebuah pesanan laboratorium.

**Dua waktu, dua pertanyaan berbeda:**

| Waktu | Siapa yang mengisi | Menjawab pertanyaan |
| --- | --- | --- |
| `PhysicallyReceivedAt` | **Petugas** | Kapan wadahnya benar-benar sampai di meja penerimaan |
| `ReceivedAt` | **Server**, tidak dapat diubah siapa pun | Kapan datanya masuk ke sistem |

**Langkah jalur normal:**

1. Petugas mencatat wadah beserta jenis dan volumenya (`BE-LAB-21`).
2. Ia mengisi **waktu kedatangan sebenarnya** bila wadahnya memang sudah datang. Ruas ini boleh
   dikosongkan — wadah yang direncanakan sebelum bahannya tiba memang belum punya waktu
   kedatangan.
3. Sistem menolak waktu yang berada di masa depan (`VAL-58`).
4. Wadah tersimpan, dan **satu baris jejak audit** mencatat kedua waktu berdampingan beserta
   selisihnya dalam menit.

**Contoh berangka, persis contoh `BR-37`.** Petugas mengisi waktu kedatangan
`2026-09-14 21:10`, dan sistem mencatatnya pada `2026-09-15 08:05`. Yang tersimpan pada jejak
audit:

```
Waktu penerimaan fisik 2026-09-14 21:10 UTC; tercatat sistem 2026-09-15 08:05 UTC; selisih 655 menit.
```

Rekap penerimaan menempatkan wadah itu pada **14 September**, bukan 15 September. Kepala
instalasi yang membaca selisih 655 menit tahu itu jam operasional, bukan kelalaian — dan bila
suatu hari selisih sebesar itu muncul pada wadah yang datang pukul 10.00, ia punya dasar untuk
bertanya.

**Arah penyimpangan yang membuat angka ini pantas dipercaya.** Berbeda dari kebanyakan
pemunduran tanggal, pemunduran di sini **merugikan laboratorium sendiri** — sampel akan terlihat
menunggu sebelas jam lebih lama. Tidak ada yang diuntungkan dari memundurkannya.

**Jalur tidak normal:**

| Kejadian | Yang terjadi |
| --- | --- |
| Waktu kedatangan diisi di masa depan | Ditolak `422` `VAL-58` — "Waktu penerimaan tidak boleh melewati waktu sekarang." |
| Waktu kedatangan dikosongkan | **Diterima.** Kolomnya tetap kosong, dan rekap memakai waktu sistem seperti sebelumnya |
| `ReceivedAt` dikirim dari luar | **Diabaikan tanpa pesan kesalahan** — tidak ada satu pun DTO permintaan yang memilikinya (`AC-65`) |
| Wadah pengganti pada pengambilan ulang | Waktu kedatangan **tidak** diwariskan — bahan penggantinya belum diambil, apalagi sampai di meja penerimaan |

**Yang sengaja tidak berubah.** Perhitungan keterlambatan cito **tetap** dihitung dari wadah
yang dinyatakan layak sampai hasil terbit. Waktu baru ini tidak menyentuhnya sama sekali
(`AC-17`, `BR-37` butir 6).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` bagian 6b | Cakupan, DoD, dan batas task |
| `00-interview-decisions.md` BR-37 dan `LAB-DEC-042` | Enam butir aturan beserta contoh dan alasannya |
| `contracts/validation-matrix.md` bagian 7b | Bunyi `VAL-58` dan `VAL-59` persis, beserta kolom *Berlaku pada* |
| `contracts/api-contract.md` bagian perluasan `r7` | Permintaan mana yang membawa waktu penerimaan fisik |
| `erd/data-dictionary.md` bagian 12.2 dan 12.4 | Tipe kolom, nullability, nama index |
| `testing/acceptance-test-matrix.md` bagian 11.5 | `T-65a`, `T-65b`, `T-66a`, `T-66b`, `T-67a`, `T-67b`, `T-17a` |
| `Services/LabWorklistService.cs` | Dasar perhitungan keterlambatan cito — untuk membuktikan `AC-17` tidak tersentuh |
| `Services/LabSpecimenService.cs` `GetSummaryAsync` | Rekap penerimaan yang selama ini memakai waktu sistem |
| `Models/LabTransitionHistory.cs` | Ruas jejak audit yang tersedia untuk mencatat selisih |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabSpecimen.cs` | Kolom `PhysicallyReceivedAt` baru; makna `ReceivedAt` dipertegas pada dokumentasinya, **tanpa mengubah tipe, nullability, maupun perilakunya** |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabSpecimenConfiguration.cs` | Satu index atas `PhysicallyReceivedAt` |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabSpecimenDtos.cs` | Satu ruas pada `PlanLabSpecimenRequest`; satu ruas pada `LabSpecimenResponse` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` | `ResolvePhysicalReceipt` menegakkan `VAL-58`; `ComposePhysicalReceiptNote` menyusun catatan audit; `CreateSpecimenAsync` menyimpan dan mencatatnya; `GetSummaryAsync` beralih ke waktu nyata |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabSpecimenController.cs` | Satu ruas pada `MapResponse` |
| `Migrations/20260915042458_AddLabSpecimenPhysicallyReceivedAt.cs` | **Baru.** Satu kolom nullable beserta index |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tergenerasi mengikuti migration di atas |

**Empat baris terhapus pada diff gabungan `BE-LAB-21` + `BE-LAB-22`, dan seluruhnya
disengaja:** satu baris ringkasan XML `SpecimenDescription` (milik `BE-LAB-21`), dua baris
penyaring `GetSummaryAsync` yang digantikan, dan satu argumen `reasonNote` yang kini dihitung
lebih dulu. Tidak satu pun kolom, ruas, endpoint, atau perilaku lama yang dihapus.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /lab-specimens/by-order/{labOrderId}` menerima satu ruas baru **opsional**; seluruh jalur baca wadah membawa satu ruas baru. Sesuai `LAB-API-v1` `r7`; tidak ada revisi kontrak baru yang diperlukan |
| Database | Satu kolom **nullable** beserta index. Migration **dibuat dan diterapkan ke `QuilvianNewDevYoga` pada 2026-09-15**, jalur `Down` ikut dibuktikan. `ALTER TABLE`-nya tidak menulis ulang satu baris pun — 5 baris lama terbukti utuh |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada permission yang berubah. **`ReceivedAt` tetap tidak dapat disentuh dari luar** — lihat 5.3 |

### 3.4 Satu perilaku lama yang memang berubah, dan itu diminta `AC-67`

`GET /lab-specimens/summary` sebelumnya menyaring rentang tanggalnya dengan `CreateDateTime`.
Sekarang ia menyaring dengan **waktu nyata**, dan jatuh kembali ke `CreateDateTime` hanya bila
waktu nyatanya tidak ada:

```csharp
.Where(x => !x.IsDelete &&
            (x.PhysicallyReceivedAt ?? x.CreateDateTime) >= startDate &&
            (x.PhysicallyReceivedAt ?? x.CreateDateTime) <= endDate);
```

**Cadangan `CreateDateTime` bukan pilihan kedua yang setara.** Ia ada supaya wadah lama —
seluruhnya, karena kolomnya baru — dan wadah yang waktu kedatangannya memang tidak dicatat tetap
muncul pada rekap, dengan perilaku **sama persis** seperti sebelum `LAB-DEC-042`. Perubahan ini
hanya terasa pada wadah yang waktu kedatangannya benar-benar diisi.

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Specimen

Base URL: `api/v1/health-services/laboratory-management/lab-specimens`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/by-order/{labOrderId:guid}` | Mencatat wadah, kini termasuk **waktu kedatangan sebenarnya** | `LabSpecimen : Plan` |
| `GET` | `/summary` | Rekap penerimaan wadah pada satu rentang, kini menurut **waktu nyata** | `LabSpecimen : Read` |
| `GET` | `/by-order/{labOrderId:guid}` | Daftar wadah, kini membawa waktu kedatangan sebenarnya | `LabSpecimen : Read` |
| `GET` | `/by-order/{labOrderId:guid}/history` | Jejak audit, kini memuat catatan kedua waktu beserta selisihnya | `LabSpecimen : Read` |

**Ruas baru pada `PlanLabSpecimenRequest`:**

| Ruas | Tipe | Wajib | Catatan |
| --- | --- | :---: | --- |
| `physicallyReceivedAt` | `datetime` | Tidak | Tidak boleh di masa depan (`VAL-58`) |

**Ruas baru pada `LabSpecimenResponse`:** `physicallyReceivedAt`. Berdampingan dengan
`receivedAt` yang sudah ada, sehingga selisih keduanya terbaca langsung dari satu respons
(`AC-67`).

**Tidak ada ruas permintaan bernama `receivedAt` pada endpoint mana pun.**

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `ACTIVE`, prefix `Lab`, tanpa entity baru | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 21 |
| Review diff/scope | 5 berkas source disentuh, 2 berkas migration baru, 1 snapshot tergenerasi | `PASS` | Seluruh 4 baris terhapus ditelusuri satu per satu — bagian 3.2 |
| `dotnet build -p:RunAnalyzers=False --no-incremental` | **0 Error, 191 Warning** | `PASS` | Nol warning berasal dari berkas task ini |
| Verifikasi kontrak API terhadap `r7` | Satu ruas request dan satu ruas response terpasang | `PASS` | `LabSpecimenDtos.cs:86`, `:200`; `LabSpecimenController.cs:360` |
| Bentuk SQL migration | Satu `ADD COLUMN` nullable, satu `CREATE INDEX` | `PASS` | Lihat 5.1 |
| **`T-65b`** — tidak ada ruas permintaan bernama `ReceivedAt` | **Nol kecocokan** | `PASS` | Lihat 5.3 |
| **`AC-65`** — tidak ada endpoint yang dapat mengubah `ReceivedAt` | **Satu-satunya jalur tulis diisi server** | `PASS` | Lihat 5.3 |
| **`T-17a`/`AC-17`** — perhitungan keterlambatan cito tidak berubah | **Nol rujukan, nol diff** | `PASS` | Lihat 5.4 |
| `T-66a` — waktu di masa depan ditolak `422` `VAL-58` | Ditegakkan di source, belum dijalankan runtime | `NOT RUN` | `LabSpecimenService.cs:1421-1425` |
| **`T-66b`** — waktu mendahului pengambilan ditolak `VAL-59` | **TIDAK DAPAT DIPENUHI** pada kontrak `r7` | `NEW ERROR` | Lihat bagian 6 |
| **Eksekusi migration ke `QuilvianNewDevYoga`** | Diterapkan; kolom `timestamp with time zone` nullable dan indexnya terbaca dari database | `PASS` | Lihat 5.6 |
| **Jalur `Down` lalu `Up`** | Terbukti; verifikasi ulang sesudahnya tetap lulus seluruhnya | `PASS` | Lihat 5.6 |
| `T-67a`, `T-67b`, `T-65a` | Menunggu aplikasi dijalankan dengan kredensial pengguna | `NOT RUN` | Lihat 5.5 |

### 5.1 Bentuk SQL migration

`dotnet ef migrations script 20260915035317_AddLabSpecimenTypeAndVolumeColumns 20260915042458_AddLabSpecimenPhysicallyReceivedAt`

```sql
ALTER TABLE public."LabSpecimen" ADD "PhysicallyReceivedAt" timestamp with time zone;
CREATE INDEX "IX_LabSpecimen_PhysicallyReceivedAt" ON public."LabSpecimen" ("PhysicallyReceivedAt");
```

> **Satu selisih terhadap DDL kamus data, disengaja dan dilaporkan.**
> `erd/data-dictionary.md` bagian 12.4 menulis tipenya `timestamp`; yang dibuat
> `timestamp with time zone`. Alasannya: `ReceivedAt` dan `CollectedAt` pada tabel yang sama
> keduanya `timestamp with time zone`, dan `VAL-59` beserta perhitungan selisih membandingkan
> ketiganya. Dua tipe waktu berbeda pada satu tabel yang saling dibandingkan adalah cacat yang
> menunggu terjadi. Kamus data sendiri menandai bagian itu **"dokumentasi, bukan skrip"**.

### 5.2 Jejak audit memuat kedua waktu berdampingan

Butir DoD *"selisih kedua waktu tercatat pada jejak audit"* ditegakkan di
`LabSpecimenService.cs:1507-1508`, memakai `ComposePhysicalReceiptNote` pada `:1447-1458`.
Baris riwayat `Specimen.Plan` menyimpan catatannya pada `ReasonNote`, sementara `OccurredAt`
baris itu **adalah** waktu sistem yang menjadi pembandingnya — sehingga keduanya tersimpan pada
satu baris, bukan dua tempat yang bisa menyimpang.

Jejak audit ini terbaca lewat `GET /by-order/{labOrderId}/history` yang sudah ada.

### 5.3 `AC-65` terbukti secara struktural — nol baris source diubah untuk itu

Penelusuran seluruh DTO Laboratorium atas nama `ReceivedAt`:

| Tempat ditemukan | Jenis |
| --- | --- |
| `LabSpecimenDtos.cs:192` | **Response** (`LabSpecimenResponse`), bukan permintaan |
| `LabSpecimenDtos.cs:83`, `:197` | Teks dokumentasi, bukan properti |

**Nol ruas permintaan bernama `ReceivedAt` pada seluruh DTO Laboratorium** (`T-65b`).

Penelusuran seluruh jalur tulis `ReceivedAt` pada modul Laboratorium menemukan **tepat satu**:

```csharp
// LabSpecimenService.cs:1583 — di dalam MoveOperationalStatusAsync
else if (target == LabSpecimenStatus.Received)
{
    specimen.ReceivedAt = now;          // now = DateTime.UtcNow, bukan dari permintaan
    specimen.ReceivedByUserId = actorUserId;
}
```

Karena ruasnya tidak ada pada DTO mana pun, nilai `receivedAt` yang dikirim client diabaikan
model binding tanpa pesan kesalahan — persis yang diminta `AC-65`.

### 5.4 `AC-17` terbukti tidak berubah

Keterlambatan cito dihitung `LabWorklistService.GetCitoOverdueAsync` dari
`LabExamination.ChargeEligibleAt` — waktu wadah dinyatakan layak — ditambah batas waktu per
jenis pemeriksaan. Dua bukti bahwa waktu baru ini tidak menyentuhnya:

| Bukti | Hasil |
| --- | --- |
| Rujukan `PhysicallyReceivedAt` di seluruh source aplikasi | **11 kecocokan, seluruhnya** pada model, DTO, configuration, `LabSpecimenService`, dan `LabSpecimenController`. **Nol** pada `LabWorklistService`, `LabMonitoringService`, maupun `LabExaminationService` |
| `git diff --stat` atas ketiga berkas itu | **Kosong** — tidak satu baris pun berubah |

### 5.5 Yang belum dijalankan, dan kenapa

| Pemeriksaan | Kenapa belum |
| --- | --- |
| `T-67a` — wadah 21.10 muncul pada rekap hari pertama | Menunggu aplikasi dijalankan. Kolom dan penyaringnya sudah berdiri; penyaringnya terbukti pada source (3.4) |
| `T-67b` — selisih terbaca kepala instalasi | Menunggu aplikasi dijalankan. Kedua ruas sudah ada pada respons, dan catatan selisihnya pada jejak audit |
| `T-65a` — `ReceivedAt` yang dikirim dari luar diabaikan | Menunggu aplikasi dijalankan. Pembuktian strukturalnya ada pada 5.3 dan lebih kuat daripada satu percobaan runtime |

Uji manual: `NOT FEASIBLE` pada sesi ini — memerlukan aplikasi dijalankan dengan kredensial
pengguna. Verifikasi tingkat database sudah dijalankan; hasilnya pada 5.6.

### 5.6 Eksekusi database — dijalankan 2026-09-15

Wewenang diberikan pemilik modul dengan menyebut targetnya secara tegas: **`QuilvianNewDevYoga`
di `160.22.250.77`**. `dotnet ef migrations list` lebih dulu memastikan hanya dua migration yang
`Pending` — milik `BE-LAB-21` dan task ini.

| Butir | Hasil |
| --- | --- |
| Kolom `PhysicallyReceivedAt` | **Ada** — `timestamp with time zone`, `is_nullable = YES`, sama persis dengan `ReceivedAt` dan `CollectedAt` pada tabel yang sama |
| `IX_LabSpecimen_PhysicallyReceivedAt` | **Terbaca dari `pg_indexes`** |
| Data lama | **5 baris `LabSpecimen` utuh**, nol di antaranya terisi `PhysicallyReceivedAt`. Tidak satu baris pun tersentuh |
| **Jalur `Down` lalu `Up`** | `database update 20260914084730_SeedLabSpecimenType` → `Done.`, kedua migration kembali `Pending`; `database update` lagi → `Done.`, nol `Pending`. Verifikasi ulang sesudahnya: **5 kolom, 2 FK `RESTRICT`, 3 index, 5 baris lama utuh, 7 jenis specimen** — seluruhnya tetap lulus |
| Kebersihan | **Nol baris uji tertinggal** |

Alatnya sama seperti `BE-LAB-21`: satu program sekali pakai di scratchpad sesi, **di luar
repository**, membaca connection string dari berkas konfigurasi sehingga kata sandinya tidak
pernah melewati baris perintah (`rules/backend/TEST_POLICY.md` bagian 3).

---

## 6. `VAL-59` tidak dapat ditegakkan pada kontrak `r7` — temuan, bukan kelalaian

Ini butir yang **tidak terpenuhi**, dan sebabnya bukan pada implementasi.

`VAL-59` berbunyi: *"Waktu penerimaan fisik mendahului waktu pengambilan specimen"* → ditolak
`422`. Kolom *Berlaku pada* pada matriks validasi menyebut **"Merencanakan wadah"**.

**Masalahnya: pada saat wadah direncanakan, waktu pengambilan belum ada.**

| Fakta | Bukti |
| --- | --- |
| Satu-satunya permintaan yang membawa waktu penerimaan fisik adalah `PlanLabSpecimenRequest` | `contracts/api-contract.md` baris 381 |
| `PlanLabSpecimenRequest` **tidak memiliki** ruas waktu pengambilan | DTO-nya — ruas yang ada hanya `Examinations`, `ProcedureId`, `SpecimenDescription`, dan kelima ruas amandemen `r7` |
| `CollectedAt` diisi **server** pada tindakan pengambilan, bukan dari permintaan | `LabSpecimenService.cs:1578-1579` |
| `CreateSpecimenAsync` tidak pernah mengisi `CollectedAt` | Wadah baru selalu berstatus `Planned` dengan `CollectedAt` kosong |

Pembandingnya belum ada, sehingga aturan itu tidak pernah dapat menyala pada jalur yang
ditunjuk kontraknya sendiri.

**Dan menegakkannya pada tindakan pengambilan justru salah — bukan sekadar di luar tempat.**
`CollectedAt` di sana adalah waktu petugas menekan tombol **di laboratorium**. Pada wadah
rujukan luar, waktu itu hampir selalu **lebih akhir** daripada waktu kedatangannya. Contoh
`BR-37` sendiri membuktikannya: wadah tiba Senin 21.10, diregistrasi Selasa 08.05 — bila
`VAL-59` dijaga pada tindakan pengambilan, **skenario yang menjadi alasan `LAB-DEC-042` dibuat
akan ditolak sistem.**

**Apa yang dikerjakan.** `ResolvePhysicalReceipt` menerima parameter waktu pengambilan dan
menegakkan `VAL-59` di dalamnya (`LabSpecimenService.cs:1428-1432`). Jalur perencanaan
memanggilnya dengan pembanding kosong, karena memang itu keadaannya. **Aturannya berdiri utuh
dan akan langsung menyala** begitu kontrak memberi jalan bagi waktu pengambilan yang dinyatakan
petugas — tidak ada pekerjaan tersisa selain satu baris pemanggilan.

**Yang perlu diputuskan pemilik modul — tiga pilihan:**

| Pilihan | Isi | Akibat |
| --- | --- | --- |
| **A** | Tambahkan ruas **waktu pengambilan yang dinyatakan petugas** ke `PlanLabSpecimenRequest` | Perlu amandemen kontrak ke `r10`. `VAL-59` dan `AC-66` tegak penuh. Paling sesuai maksud `RULE-019`: waktu pengambilan di klinik perujuk memang diketahui petugas dan hari ini tidak tersimpan di mana pun |
| **B** | Persempit `VAL-59` menjadi berlaku hanya bila waktu pengambilan sudah diketahui | Tidak perlu kontrak baru, tetapi aturannya menjadi tidak pernah menyala — jujur, namun praktis sama dengan mencabutnya |
| **C** | Cabut `VAL-59` beserta separuh `AC-66` | Preseden sudah ada: `VAL-60` dicabut `LAB-DEC-050` ketika terbukti tidak dapat dilaksanakan |

**Saya tidak memilih satu pun di antaranya.** `VAL-58` tetap ditegakkan penuh, dan `AC-66`
dilaporkan **terpenuhi separuh**.

> **Polanya sama persis dengan `BE-LAB-23` dan `BE-LAB-24`.** Ketiganya adalah acceptance
> criteria yang ditulis amandemen 2026-09-14 dari `LAB-EVD-001`, dan ketiganya lolos sampai
> tahap implementasi tanpa pernah diadu dengan model data yang sudah berjalan. Artifact lapangan
> menggambarkan satu layar sebagaimana dipahami penggunanya; ia tidak tahu waktu pengambilan di
> sistem ini adalah cap waktu server, bukan keterangan yang diisi petugas.
>
> Catatan pada `roadmap/traceability.md` bagian 5 sudah meminta langkah pencegahannya:
> **adu setiap acceptance criteria baru dengan ERD dan alur yang sudah berjalan sebelum
> disetujui.** Temuan ketiga ini menunjukkan langkah itu belum dijalankan untuk `BR-37`.

---

## 7. Acceptance criteria dan Definition of Done

### 7.1 Butir DoD roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kolom ada | **✅ Terbukti di database** | Terbaca dari `information_schema.columns` sebagai `timestamp with time zone` nullable — 5.6 |
| `ReceivedAt` tidak dapat diubah endpoint mana pun | **Terpenuhi** | 5.3 — satu-satunya jalur tulis diisi server |
| Tidak ada ruas permintaan bernama `ReceivedAt` pada DTO mana pun | **Terpenuhi** | 5.3 — nol kecocokan |
| Laporan penerimaan memakai waktu nyata | **Terpenuhi pada source**; kolomnya kini ada di database | `LabSpecimenService.cs:919-931`; 5.6 |
| Selisih kedua waktu tercatat pada jejak audit | **Terpenuhi** | `:1507-1508` dan `:1447-1458` |
| `AC-17` terbukti tidak berubah | **Terpenuhi dan terbukti dua kali** | 5.4 |
| *(implisit lewat `VAL-58`, `VAL-59`)* | **Terpenuhi separuh** | `VAL-58` tegak; `VAL-59` lihat bagian 6 |

### 7.2 Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| `AC-65` — tidak ada endpoint yang dapat mengubah `ReceivedAt` | **Terpenuhi** | 5.3 |
| `AC-66` — waktu di masa depan **atau** mendahului pengambilan ditolak | **Terpenuhi separuh.** Masa depan ditolak `VAL-58`; mendahului pengambilan **tidak dapat ditegakkan** | `:1421-1425`; bagian 6 |
| `AC-67` — laporan penerimaan memakai waktu nyata, selisih terlihat | **Terpenuhi pada source**; kolom dan indexnya terbukti di database | 3.4, 5.2, dan 5.6 |
| `AC-17` — perhitungan keterlambatan cito tidak berubah | **Terpenuhi dan terbukti** | 5.4 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet ef migrations add` mencetak `HostAbortedException`; ini perilaku normal EF Tools saat membangun `IHost`, bukan kegagalan |
| Masalah yang diketahui | `VAL-59` dan separuh `AC-66` — bagian 6. Perlu keputusan pemilik modul, bukan perbaikan kode |
| Risiko tersisa | Rekap `GET /summary` berubah dasarnya bagi wadah yang waktu kedatangannya diisi. Ini **diminta** `AC-67`, tetapi konsumen yang membandingkan angka rekap antar-periode perlu tahu bahwa titik potong harinya bergeser |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 44 entri, **5 di antaranya milik task ini** di luar berkas yang juga disentuh `BE-LAB-21`: dua berkas migration, laporan ini, dan pembaruan roadmap/traceability. Tidak ada `git add`, `commit`, maupun `push` |
| Langkah berikutnya | Lihat 8.1 |

### 8.1 Langkah berikutnya

1. **Putuskan `VAL-59`** — pilihan A, B, atau C pada bagian 6. Ini satu-satunya butir yang
   menahan `AC-66` terpenuhi penuh.
2. ~~Minta wewenang menerapkan migration~~ — ✅ **Selesai 2026-09-15.** Kedua migration
   diterapkan ke `QuilvianNewDevYoga` dalam satu jendela; `T-M2`, `T-M4`, dan jalur `Down` lalu
   `Up` terbukti (5.6 dan `BE-LAB-21` 5.5–5.6). Yang tersisa hanya `T-67a` dan `T-67b`, yang
   memerlukan aplikasi dijalankan.
3. **`FE-LAB-11` kini mendesak.** Migration sudah masuk, sehingga layar wadah pada
   `QuilvianNewDevYoga` **sudah** menjawab `422` — `specimenTypeId` wajib sejak `BE-LAB-21`.
4. `BE-LAB-25` kini satu-satunya task backend `MVP-5a` yang tersisa.
