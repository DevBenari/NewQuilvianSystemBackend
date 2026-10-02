# Laporan Perubahan Backend — `BE-LAB-26`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-26` |
| Judul | Tabel pemeriksaan terpesan |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6c |
| Trace | `FR-11.2` turunan; `LAB-DEC-057`; `AC-91`; `T-91a` |
| Contract version | **Tidak ada endpoint.** Struktur saja. `LAB-API-v1` tidak bertambah |
| Dependency | — |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium, configuration, migration, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **✅ `SELESAI`** — tabel berdiri, diterapkan ke `QuilvianNewDevYoga`, unique parsial dan `Restrict` **terbukti terisolasi**, jalur `Down` lalu `Up` dibuktikan. Nol baris uji tertinggal |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 |
| Keberlakuan | `NEW CODE` seluruhnya |
| Status gerbang | `QBE-MOD-002` **tidak menahan** — lifecycle `ACTIVE` |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-CODE-004`, `QBE-ENUM-001` |

Gate lolos bersih: task berada di gelombang eksekusi 1 tanpa dependency, `LAB-DEC-057` berstatus
`approved`, `T-91a` sudah ada pada matriks uji, dan `HEAD` `e2152709` cocok dengan manifest
revision 29.

---

## 1. Masalah yang diperbaiki

`LAB-DEC-056` memutuskan petugas memilih **daftar pemeriksaan** saat pendaftaran, lalu pesanan
terbentuk. Ketika rancangannya disusun, ditemukan bahwa keputusan itu **tidak dapat
dilaksanakan** di atas model yang ada.

Tiga fakta, seluruhnya dari source:

| Fakta | Akibatnya |
| --- | --- |
| `LabOrder` memegang **tepat satu** `ProcedureId` | Satu pesanan tidak dapat menampung daftar |
| `LabExamination.SpecimenId` **wajib**, dan bagian unique index `(SpecimenId, ProcedureId)` | Pemeriksaan **tidak dapat hidup sebelum wadah fisiknya ada** |
| `LabSpecimen` mewajibkan jenis specimen sejak `BE-LAB-21` (`VAL-51`) | Wadah tidak dapat dibuat sebelum bahannya benar-benar datang |

Jadi daftar pemeriksaan yang dipilih saat pendaftaran **tidak punya tempat tinggal** — padahal
`LAB-DEC-045` sengaja memisahkan layar pendaftaran dari layar penerimaan specimen.

Akibat yang lebih halus: pertanyaan *"dari yang diminta untuk pasien ini, mana yang belum masuk
wadah"* tidak pernah dapat dijawab, karena yang belum berwadah tidak tercatat di mana pun.

---

## 2. Proses bisnis

**Dua konsep yang selama ini menumpang pada satu tabel, kini dipisah:**

| Tabel | Menjawab | Lahir kapan | Akibat finansial |
| --- | --- | --- | --- |
| `LabOrderedProcedure` | **Apa yang diminta** | Saat pendaftaran | **Tidak ada** |
| `LabExamination` | **Apa yang dikerjakan dari sebuah wadah** | Saat wadah dicatat | Menerbitkan fakta kelayakan tagih per baris (`AC-37`) |

**Alur yang dimungkinkannya:**

1. Petugas memilih pemeriksaan saat pendaftaran → satu baris `LabOrderedProcedure` per
   pemeriksaan, berstatus `Ordered`.
2. Bahan datang, petugas mencatat wadah → `LabExamination` terbentuk.
3. Baris terpesan yang terpenuhi ditandai `Fulfilled` dan **ditautkan** ke baris pemeriksaannya.
4. Yang masih `Ordered` terbaca sebagai **menunggu wadah** — bukan hilang.

Langkah 2 sampai 4 adalah cakupan `BE-LAB-27` dan `BE-LAB-28`. Task ini menyediakan tempatnya.

**Aturan yang melekat pada tabel ini:**

| Aturan | Isi | Kenapa |
| --- | --- | --- |
| Unique `(LabOrderId, ProcedureId)` bila belum terhapus | Satu jenis pemeriksaan diminta **sekali** per pesanan | Sejajar `VAL-07` pada tingkat wadah |
| Pengerjaan ganda **bukan** dua baris | Dinyatakan `IsDuplo` pada `LabExamination` | `LAB-DEC-026` |
| `DisciplineSnapshot` **disalin**, bukan dibaca ulang | Penggolongan katalog yang berubah kemudian tidak mengubah makna permintaan yang sudah terjadi | Pola sama dengan salinan kode dan nama |
| Boleh tanpa disiplin | Pemeriksaan yang belum digolongkan tetap sah dipesan | `AC-85` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` bagian 6c | Cakupan, DoD, dan batas task |
| `02-backend-architecture.md` bagian 12.1-12.2 | Alasan struktural dan bentuk kolomnya |
| `erd/data-dictionary.md` bagian 13 | Tipe, nullability, nama index dan FK |
| `Models/LabExamination.cs`, `LabExaminationConfiguration.cs` | Pola terdekat: salinan kode/nama, unique parsial, `Restrict` |
| `Enums/LaboratoryEnums.cs` | Pola enum beserta `[Display]` |
| `Repositories/ApplicationDbContext.cs` | Tempat DbSet Laboratorium |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrderedProcedure.cs` | **Baru.** Entity beserta sembilan kolom bisnis dan tiga navigation |
| `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs` | Satu enum `LabOrderedProcedureStatus` |
| `Repositories/Configurations/.../LabOrderedProcedureConfiguration.cs` | **Baru.** Tabel, panjang kolom, unique parsial, tiga relasi `Restrict` |
| `Repositories/ApplicationDbContext.cs` | Satu `DbSet<LabOrderedProcedure>` |
| `Migrations/20260915081317_AddLabOrderedProcedure.cs` | **Baru.** Tabel beserta lima index dan tiga FK |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tergenerasi |

**`LabExamination` tidak berubah satu baris pun**, dan itu butir DoD tersendiri — lihat 6.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **`NOT APPLICABLE`.** Nol endpoint, nol DTO. Tabel ini belum dibaca maupun ditulis siapa pun sampai `BE-LAB-27` |
| Database | Satu tabel baru beserta lima index dan tiga FK `RESTRICT`. **Diterapkan ke `QuilvianNewDevYoga` 2026-09-15**, jalur `Down` dibuktikan |
| Keamanan/Auth | **`NOT APPLICABLE`.** Nol permission baru |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint. Cakupannya struktur saja, dan
endpoint yang memakainya adalah `BE-LAB-27`.

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `ACTIVE`, prefix `Lab` | `PASS` | Bagian preflight |
| Review diff/scope | 3 berkas baru, 2 disentuh aditif, 1 snapshot tergenerasi | `PASS` | Bagian 3.2 |
| `dotnet build -p:RunAnalyzers=False --no-incremental` | **0 Error, 207 Warning** | `PASS` | Nol warning dari berkas task ini |
| Eksekusi migration ke `QuilvianNewDevYoga` | Tabel berdiri: **19 kolom, 3 FK `RESTRICT`, 5 index** | `PASS` | Lihat 5.2 |
| **Unique parsial `(LabOrderId, ProcedureId)`** | **Ditolak `23505`** pada index yang benar | `PASS` | Lihat 5.3 |
| **`Restrict` pada `FK_LabOrderedProcedure_LabOrder_LabOrderId`** | **Ditolak `23503`**, terisolasi beserta langkah kontrolnya | `PASS` | Lihat 5.3 |
| **Jalur `Down` lalu `Up`** | Terbukti; verifikasi ulang tetap lulus, data lama utuh | `PASS` | Lihat 5.4 |
| Kebersihan data uji | **Nol baris tertinggal** | `PASS` | Lihat 5.3 |

### 5.1 Tujuh migration diterapkan, bukan satu — dan itu keputusan pemilik modul

Saat migration task ini selesai dibuat, `dotnet ef migrations list` menunjukkan **tujuh**
migration `Pending`, bukan satu. Enam di antaranya milik modul lain dan masuk lewat merge
`origin/QuilvianIntegrationBackend` yang terjadi **sesudah** eksekusi database pagi ini:

| Migration | Milik |
| --- | --- |
| `AddAccountingPhase2Independent` | accounting |
| `AddInpatientEpisodeContextToPatientDiagnosis` | rawat-inap |
| `AddDepositFollowUpIntervalToInpatientSetting` | rawat-inap |
| `AddAssignmentRoleToInpDoctorAssignment` | rawat-inap |
| `AddCompanyGuarantorAndItemPayerFoundation` | billing/guarantor |
| `AddAccountingPostingRuleMaster` | accounting |
| **`AddLabOrderedProcedure`** | **Laboratorium** |

EF menerapkan migration berurutan menurut timestamp, dan milik task ini yang paling akhir —
sehingga tidak ada cara menerapkannya tanpa menyapu keenam lainnya.

**Pekerjaan dihentikan dan dilaporkan lebih dulu.** Pemilik modul kemudian menyatakan
**izin sudah diperoleh dari pemilik ketiga modul tersebut**, dan atas dasar itu ketujuhnya
diterapkan. Seluruhnya berhasil tanpa satu pun galat, dan keenam baris `__EFMigrationsHistory`
milik modul lain diverifikasi ikut terpasang.

> **Cara pembukuan yang harus jujur.** Izin dari pemilik `accounting`, `rawat-inap`, dan
> `billing` disampaikan **lisan dan diteruskan pemilik modul Laboratorium** — sama seperti
> `LAB-REQ-006`. Bila salah satu pemilik kelak menemukan cakupannya berbeda, catatan inilah yang
> keliru, bukan pekerjaannya.

### 5.2 Bukti terhadap database

| Butir | Hasil |
| --- | --- |
| Kolom | **19** — sembilan kolom bisnis ditambah sepuluh warisan `IdentityModel` |
| `FK_LabOrderedProcedure_LabOrder_LabOrderId` | **`delete_rule = RESTRICT`** |
| `FK_LabOrderedProcedure_MstProcedure_ProcedureId` | **`delete_rule = RESTRICT`** |
| `FK_LabOrderedProcedure_LabExamination_FulfilledExaminationId` | **`delete_rule = RESTRICT`** |
| `IX_LabOrderedProcedure_LabOrderId_ProcedureId` | **`UNIQUE`, berfilter `IsDelete`** |
| Index lain | `OrderedStatus`, `ProcedureId`, `FulfilledExaminationId`, dan PK |

### 5.3 Dua koreksi terhadap pengujian saya sendiri, dicatat apa adanya

Pembuktian `Restrict` **gagal dua kali** sebelum benar, dan kedua kegagalannya adalah **hasil
palsu yang hampir saya laporkan sebagai lulus**:

| Percobaan | Hasil | Kenapa keliru |
| --- | --- | --- |
| Ke-1 | Ditolak `25P02` | Uji unique dijalankan lebih dulu **pada transaksi yang sama**. Pelanggaran unique membuat transaksi PostgreSQL **abort**, sehingga `DELETE` berikutnya ditolak karena transaksinya sudah mati — bukan karena `Restrict` |
| Ke-2 | Ditolak `23503` pada **`FK_LabSpecimen_LabOrder_LabOrderId`** | Pesanan yang dipakai sudah punya wadah, sehingga **FK milik wadah** menghadang lebih dulu. Kode galatnya benar, constraintnya milik orang lain |

Pembuktian yang sah memerlukan **pesanan yang terisolasi**, dan tidak ada satu pun pada data
yang ada. Karena itu dibuat pesanan sintetis di dalam transaksi yang di-`ROLLBACK`, beserta
**langkah kontrol** yang membuat buktinya tertutup:

```
Pesanan sintetis tersisip.
Kontrol: tanpa permintaan, penghapusan BERHASIL (1 baris) — jadi tidak ada FK lain yang menghalangi.
Satu permintaan ditempelkan pada pesanan sintetis itu.
LULUS — ditolak 23503 pada constraint FK_LabOrderedProcedure_LabOrder_LabOrderId
ROLLBACK seluruh transaksi.
```

**Langkah kontrol itulah yang membedakan bukti dari kebetulan.** Tanpa membuktikan lebih dulu
bahwa pesanan itu memang dapat dihapus ketika tidak ada permintaan, penolakan pada langkah
berikutnya tidak membuktikan FK mana yang menolaknya.

**Unique parsial** diuji pada transaksinya sendiri: baris kedua dengan pasangan
`(LabOrderId, ProcedureId)` yang sama ditolak **`23505`** pada
`IX_LabOrderedProcedure_LabOrderId_ProcedureId`.

**Kebersihan:** `LabOrderedProcedure` **0 baris** sebelum dan sesudah seluruh pengujian, dan
jumlah `LabOrder` tetap **5** — pesanan sintetisnya ikut hilang bersama rollback.

### 5.4 Jalur `Down` dibuktikan, tanpa menyentuh migration modul lain

| Langkah | Hasil |
| --- | --- |
| `database update 20260915042458_AddLabSpecimenPhysicallyReceivedAt` | `Done.` Hanya `AddLabOrderedProcedure` yang kembali `Pending` — keenam migration modul lain **tidak tersentuh**, karena keduanya berada sebelum target |
| `database update` lagi | `Done.` Nol `Pending` |
| Verifikasi ulang sesudahnya | **19 kolom, 3 FK `RESTRICT`, 5 index, 0 baris**; data lama utuh: **5 pesanan, 5 wadah, 7 jenis specimen** |

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Tabel berdiri beserta unique parsial | **✅ Terbukti di database** | 5.2 dan 5.3 — ditolak `23505` |
| Ketiga FK `Restrict` | **✅ Terbukti di database** | 5.2 `delete_rule = RESTRICT`; 5.3 ditolak `23503` terisolasi |
| Migration jalan maju dan mundur | **✅ Terbukti** | 5.4 |
| **`LabExamination` tidak berubah satu baris pun** | **✅ Terpenuhi** | Berkas itu tidak muncul pada `git status`; `SpecimenId` tetap `IsRequired()` dan unique index `(SpecimenId, ProcedureId)` tidak disentuh |

| AC | Status | Bukti |
| --- | --- | --- |
| `AC-91` — permintaan dapat ditelusuri ke pemeriksaan yang memenuhinya | **Terpenuhi secara struktural.** `FulfilledExaminationId` dan `OrderedStatus` berdiri | Penandaan `Fulfilled` adalah cakupan `BE-LAB-28`; `T-91a` menunggu keduanya |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet ef migrations add` mencetak `HostAbortedException`; perilaku normal EF Tools, bukan kegagalan |
| Masalah yang diketahui | `NONE` untuk task ini |
| Risiko tersisa | Rendah. Tabel baru yang belum dibaca maupun ditulis siapa pun sampai `BE-LAB-27` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 3 berkas baru, 2 disentuh, 2 berkas migration, snapshot, laporan ini. Tidak ada `git add`, `commit`, maupun `push` |
| Langkah berikutnya | Lihat 7.1 |

### 7.1 Langkah berikutnya

1. **`BE-EXT-04`** menyelesaikan gelombang 1 `MVP-5b` — ruas tujuan layanan pada sesi kiosk.
2. **`BE-LAB-27`** kini terbuka: endpoint pemesanan per disiplin yang mengisi tabel ini.
3. **Database kini sejajar dengan branch `yoga`.** Keenam migration modul lain yang tertinggal
   sudah terpasang, sehingga siapa pun yang menjalankan aplikasi dari branch ini tidak lagi
   menemui schema yang tertinggal. Sebaiknya dikonfirmasi ke pemilik `accounting`,
   `rawat-inap`, dan `billing` bahwa penerapannya sudah terjadi.

### 7.2 Pelajaran pengujian yang pantas dicatat

Pembuktian `Restrict` gagal dua kali dengan cara yang **terlihat seperti lulus**. Keduanya
menghasilkan kode galat PostgreSQL yang wajar — `25P02` dan `23503` — dan keduanya akan lolos
sebagai bukti bila hanya kode galatnya yang dibaca.

Yang menyelamatkannya adalah membaca **nama constraint-nya**, lalu menambahkan **langkah
kontrol** yang membuktikan bahwa tanpa baris uji itu penghapusannya memang berhasil. Uji
penolakan tanpa langkah kontrol hanya membuktikan ada sesuatu yang menolak — bukan bahwa yang
menolak adalah yang sedang diuji.
