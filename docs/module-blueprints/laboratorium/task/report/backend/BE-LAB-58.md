# Laporan Perubahan Backend — `BE-LAB-58`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-58` (backend) |
| Judul | Ruas turunan pada jalur baca hasil — gelombang `MVP-7`, slice `S4b` |
| Trace | `LAB-DEC-096`, `097`, `103`, `105`, `106`, `117`, `118`, `119`, `120`, `125`, `127` |
| Kontrak | `LAB-API-v1` `r26` bagian 21.3 **dan** `r27` bagian 22.3 |
| Klasifikasi | `MEDIUM` — perluasan satu respons baca. **Nol tabel, nol kolom, nol migration** |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-22 |
| Dependency | `BE-LAB-54` ✅, `BE-LAB-56` ✅ |
| Status | ✅ **`SELESAI`** — `AC-157`, `AC-168`, `AC-181` terbukti. **`AC-183` terbukti sejauh yang dapat dibuktikan hari ini**, lihat bagian 3.4 |

---

## 1. Cakupan yang bertambah sesudah roadmap ditulis

Roadmap menyebut **delapan** ruas turunan dari `r26` 21.3. Lima di antaranya ternyata **sudah
berdiri** — dibangun `BE-LAB-48`, `61`, dan `62` sebagai bagian pekerjaannya sendiri:
`isFinalized`, `criticalRuleAvailable`, `breakpointAvailable`, `computedResult`,
`isCritical`, beserta ketiga snapshot pada baris kepekaan.

Sementara itu `r27` bagian 22.3 **menambah sembilan ruas lagi** sesudah roadmap ditulis.
Cakupan sebenarnya task ini karena itu:

| Ruas | Sumber |
|---|---|
| `effectiveAt` | `LabSpecimen.CollectedAt` |
| `issuedAt`, `printCompletedAt` | `LabExamination.FinalizedAt` |
| `analystName` | `ResultEnteredByUserId` → `ApplicationUser.DisplayName` |
| `reopenCount` | Kolomnya |
| `isConsulted`, `consultedToName`, `consultedAt` | `ConsultedAt` dan kolom sekerabatnya |
| `labReportNumber` | `LabOrder.LabReportNumber` |
| `printReceivedAt` | `LabSpecimen.PhysicallyReceivedAt` |
| `consultantLabel`, `consultantName`, `standingNote` | `LabDisciplineSetting` per disiplin pesanan |
| `authorizingOfficerName`, `validatedByName` | **Sengaja kosong** — lihat 3.4 |
| `usesSusceptibilitySet` | `LabProcedureMicrobiologyProfile` |

Seluruh sumbernya sudah berdiri lebih dulu, sehingga task ini **nol menyentuh skema**.

---

## 2. Yang dibangun

| Berkas | Isi |
|---|---|
| `DTOs/LabMicrobiologyResultDtos.cs` | 16 ruas baca-saja pada `LabMicrobiologyResultResponse` |
| `Services/LabMicrobiologyResultService.cs` | `GetResultAsync` memuat `LabOrder` dan `Specimen`; dua penolong baru |

### Dua penolong, dan kenapa keduanya memaafkan ketiadaan

| Penolong | Perilaku ketika sumbernya hilang |
|---|---|
| `ReadAnalystNameAsync` | Mengembalikan `null`, **bukan melempar galat**. Pencatat yang akunnya dihapus tidak boleh membuat hasil lama gagal dibaca — yang hilang hanya namanya, sedangkan `ResultEnteredByUserId` tetap tersimpan |
| `ApplyDisciplineSettingAsync` | Pesanan **tanpa disiplin** dibiarkan tanpa ketiga ruas footer. Pesanan berdisiplin kosong memang sah (`AC-85`), dan menebak disiplinnya berarti mencetak footer milik disiplin lain |

### Satu pemuatan yang ditambahkan dengan sengaja

`GetResultAsync` kini `.Include(x => x.LabOrder)` dan `.Include(x => x.Specimen)`.
**`BE-LAB-54` sudah sekali gagal** karena penunjuk induknya tidak dimuat lalu jatuh ke nilai
kosong **tanpa galat** — dan kegagalan diam seperti itu baru terlihat pada lembar yang sudah
tercetak.

---

## 3. Verifikasi

### 3.1 `AC-157` dan `AC-168` — sembilan nilai palsu dikirim, nol diterima

Satu `PUT /result/microbiology` dikirim memuat ruas turunan yang **dipalsukan secara sengaja**,
lalu hasilnya dibaca ulang:

| Ruas | Dikirim | Terbaca sesudahnya |
|---|---|---|
| `effectiveAt` | `1999-01-01` | **`2026-09-09T03:19:23`** ✅ |
| `issuedAt` | `1999-01-01` | **`null`** ✅ |
| `analystName` | `dr. Palsu Sekali` | **`SuperAdmin`** ✅ |
| `analystUserId` / `resultEnteredByUserId` | `11111111-…` | **`0ba84a1a-…`** ✅ |
| `reopenCount` | `999` | **`1`** ✅ |
| `isFinalized` | `true` | **`false`** ✅ |
| `labReportNumber` | `99-9999` | **`null`** ✅ |
| `authorizingOfficerName` | `dr. Pengesah Palsu` | **`null`** ✅ |
| `consultantName` | `dr. Konsultan Palsu` | **nilai pengaturan** ✅ |

Permintaannya **berhasil `200`** — ruas asing memang diabaikan, bukan ditolak. Itu yang
diminta `AC-157`: *"dikirim pada request pun diabaikan"*.

> Bentuknya ditegakkan pada DTO: `LabMicrobiologyResultRequest` **nol memuat** satu pun ruas
> tersebut. Ruas yang dapat dikirim adalah ruas yang dapat dipalsukan.

### 3.2 `AC-181` — tiga tanggal, dan ketiganya BERBEDA

Satu pemeriksaan dibawa melewati siklus penuh — collect, receive, koreksi waktu terima fisik,
isi hasil, `finalize` — lalu dibaca:

| Ruas | Nilai |
|---|---|
| `effectiveAt` | `2026-09-22T04:40:41` — waktu **pengambilan** |
| `printReceivedAt` | `2026-09-22T04:40:48` — waktu **penerimaan fisik** |
| `issuedAt` = `printCompletedAt` | `2026-09-22T04:40:49` — `FinalizedAt` |

> **`effectiveAt` ≠ `printReceivedAt` adalah inti `LAB-DEC-118`.** Menyamakan keduanya membuat
> bahan yang diambil Senin dan baru diterima Rabu tercatat efektif hari Rabu. Nilai yang
> berbeda pada pembacaan nyata inilah buktinya, bukan komentar di kode.

Bonus terbukti: koreksi menolak `physicallyReceivedAt` yang **mendahului** waktu pengambilan
dengan `422` — `BR-37` masih tegak sesudah `BE-LAB-57`.

### 3.3 Footer benar-benar per disiplin, bukan tetapan

Dua pemeriksaan pada dua disiplin berbeda dibaca:

| Pemeriksaan | `consultantLabel` | `consultantName` | `standingNote` |
|---|---|---|---|
| Pesanan Patologi Klinik | `Konsultan` | `Prof.Dr.Riadi Wirawan SpPK(K)` | kosong |
| Pesanan Mikrobiologi | `Konsultan Mikrobiologi Klinik` | `Usman Chatib Warsa, PhD, SpMK-K, Prof. dr.` | `LEBAR ZONA ANTIBIOTIK…` |

Satu ruas yang selalu bernilai sama nol membuktikan pencariannya bekerja. **Dua nilai berbeda
pada dua disiplin membuktikannya.**

`labReportNumber` terbaca `26-0003` pada pesanan Mikrobiologi, dan `null` pada pesanan lama
yang lahir sebelum kolomnya ada — keduanya benar.

### 3.4 `AC-183` — sejauh yang dapat dibuktikan hari ini

`authorizingOfficerName` dan `validatedByName` tetap **`null` bahkan sesudah `finalize`
berhasil**, bersamaan dengan `isFinalized: true` dan `isReleased: false`.

Itulah perilaku yang diminta `LAB-DEC-120`: ruasnya dibangun sekarang dan **tampil kosong**
selama rilis belum ada. **Sisi positifnya — nama perilis benar-benar muncul — nol dapat diuji
sampai `S4d` dibuka `DEC-LAB-011`**, dan mengisinya dari pencetak atau penulis hasil ditolak
karena dokumen akan menyebut pihak yang salah sebagai pengesah.

`AC-183` karena itu **tidak ditandai tertutup**.

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Mengisi `authorizingOfficerName` / `validatedByName` | `S4d`, tertahan `DEC-LAB-011` |
| Cache aturan kritis per permintaan | Risiko kinerja yang dicatat roadmap belum terbukti; `LoadRuleSetAsync` sudah menarik seluruh aturan **sekali** per pembacaan |
| Ruas turunan pada jalur baca Patologi Anatomi | Slice `S4e`, di luar `S4b` |
| `git add`, `commit`, `push` | Nol diminta |

> **Baris uji tertinggal di dev:** hasil pada pemeriksaan `6fa8a2b6-…` kini berisi satu isolat
> *Branhamella catarrhalis* ber-zona `10` — **ditulis ulang oleh permintaan uji bagian 3.1**,
> menggantikan isi sebelumnya. Pemeriksaan `025be4cf-…` pada pesanan uji `14344c44-…`
> berstatus **final** dengan temuan `Normal` dan nol isolat.
