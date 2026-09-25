# Laporan Perubahan Backend — `BE-LAB-28`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-28` |
| Judul | Wadah hanya memuat yang dipesan |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6c |
| Trace | `LAB-DEC-057`, `BR-47`; `AC-91`; `VAL-68`, `VAL-69` |
| Contract version | `LAB-VAL-v1` **`r5`** `VAL-68` dan `VAL-69` — `approved` 2026-09-15 |
| Dependency | `BE-LAB-27` ✅ `SELESAI` 2026-09-15 |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — satu berkas source Laboratorium dan artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **✅ `SELESAI`** — kedua penjagaan berdiri dan **dijalankan terhadap database sebenarnya**; `AC-91` ditutup. Nol migration, nol endpoint baru, nol baris uji tertinggal |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** |
| Keberlakuan | `NEW CODE` untuk tiga method privat baru; `TOUCHED LEGACY` untuk dua sisipan pada method yang sudah ada |
| Status gerbang | Tidak menahan — nol entity baru, nol migration |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001` |

Sumber governance canonical terbaca: `AGENTS.md` backend, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 21 — `Lab`, `ACTIVE`.

---

## 1. Masalah yang diperbaiki

Sejak `BE-LAB-26` dan `BE-LAB-27`, sistem tahu **apa yang dipesan** untuk seorang pasien. Tetapi
jalur wadah belum membaca daftar itu sama sekali: petugas dapat memasukkan pemeriksaan apa pun ke
wadah mana pun, termasuk pemeriksaan yang tidak pernah diminta dokter, dan dapat memasukkan
pemeriksaan yang sama ke dua wadah berbeda.

Keduanya tidak menghasilkan pesan kesalahan. Yang pertama menerbitkan fakta kelayakan tagih atas
pemeriksaan yang tidak diminta — pasien ditagih untuk sesuatu yang tidak ada dalam permintaan.
Yang kedua menagihkan satu permintaan dua kali.

Sisi sebaliknya sama sunyinya: permintaan yang **belum** berwadah tidak tercatat di mana pun
sebagai menunggu. Ia hanya tidak muncul — dan tidak munculnya sesuatu jauh lebih sulit
disadari daripada munculnya sesuatu yang salah.

---

## 2. Proses bisnis

Petugas penerimaan memesan Hemoglobin dan Leukosit untuk seorang pasien. Dua permintaan berdiri;
belum ada satu pun tabung.

Kemudian tabung darahnya diambil. Petugas mencatat wadah itu dan memilih pemeriksaan yang akan
dikerjakan darinya:

1. **Hemoglobin** — ada pada daftar yang dipesan. Diterima, dan permintaannya ditandai sudah
   berwadah beserta tautan ke baris pemeriksaan yang akan mengerjakannya.
2. **Leukosit** — belum dimasukkan ke wadah mana pun. Ia terbaca **menunggu wadah**, bukan
   hilang.
3. **Trombosit** — tidak pernah dipesan untuk pasien ini. Ditolak `422`.
4. **Hemoglobin lagi**, ke wadah kedua — sudah dikerjakan dari wadah pertama. Ditolak `409`.

Dan satu hal yang **tidak** berubah sama sekali: pesanan lama, yang dibuat sebelum tabel
permintaan ada, tetap menerima wadah apa pun seperti sebelumnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas

| Berkas | Sifat | Isi |
| --- | --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` | `Diperbarui` | Tiga method privat baru; dua sisipan pemanggilan; satu method yang sudah ada diubah supaya menyerahkan baris pemeriksaan yang dibuatnya |

**Satu berkas source.** Nol DTO baru, nol endpoint baru, nol permission baru, nol migration.
Controller tidak disentuh: `LabSpecimenValidationException` sudah dipetakan ke `422` dan
`LabSpecimenConflictException` ke `409` oleh penangan yang sudah ada.

### 3.2 `EnsureOrderedProcedureGuardAsync` — penjagaannya

Dipanggil dari `PlanAsync`, **sesudah** daftar pemeriksaannya dinyatakan sah dan **sebelum** satu
baris pun dibuat — alasannya sama dengan pemeriksaan bahan yang sudah ada di sana: permintaan yang
ditolak tidak boleh meninggalkan wadah setengah jadi.

Pintu keluarnya berada di baris paling awal: **pesanan tanpa baris terpesan langsung keluar tanpa
diperiksa apa pun.** Itulah yang membuat penjagaan ini aditif, dan itu pula satu-satunya hal yang
menjaga pesanan lama tetap berperilaku seperti sebelumnya.

### 3.3 `MarkOrderedProceduresFulfilledAsync` — penandaan dan tautannya

Diletakkan di dalam `CreateExaminationsAsync`, **bukan** di `PlanAsync`. Sebabnya: jalur
pengambilan ulang memanggil method yang sama tanpa melewati `PlanAsync`. Dengan penandaan berada
di sini, wadah pengganti ikut menautkan permintaannya ke baris pemeriksaan yang **benar-benar akan
dikerjakan**, bukan ke baris pada wadah yang sudah ditolak.

### 3.4 Satu jebakan yang ditemukan sebelum ditulis, bukan sesudah

`VAL-69` berbunyi "pemeriksaan terpesan yang **sudah** masuk wadah lain". Cara termudah
menegakkannya adalah membaca penanda `Fulfilled` pada baris terpesan. **Cara itu ditolak.**

Sebabnya ditemukan saat membaca `CancelSpecimenInMemory`: **membatalkan wadah tidak membatalkan
pemeriksaan di dalamnya.** Bila penjagaan membaca penanda, maka pemeriksaan yang wadahnya
dibatalkan akan tertandai `Fulfilled` selamanya — dan petugas tidak akan pernah bisa
memasukkannya ke wadah baru. Permintaan dokter yang masih sah terkunci, tanpa jalan keluar, oleh
penjagaan yang dipasang untuk melindunginya.

Yang dibangun membaca **keadaan yang sebenarnya**: adakah baris pemeriksaan yang masih hidup, pada
wadah yang masih hidup, atas pemeriksaan itu. Penjagaan ini pulih sendiri — wadah dibatalkan,
pemeriksaannya tidak lagi dihitung, perencanaan ulang terbuka lagi.

### 3.5 `ReleaseOrderedProceduresAsync` — tambahan cakupan yang disengaja dan dilaporkan

Cakupan task ini tertulis "penandaan `Fulfilled` beserta tautannya". Pelepasan penanda saat wadah
dibatalkan **tidak** tertulis di sana. Ia tetap dibangun, dan alasannya perlu berdiri sendiri:

Tanpa pelepasan, penjagaan `VAL-69` akan mengizinkan perencanaan ulang (karena ia membaca keadaan
sebenarnya) sementara penanda pada baris terpesan tetap berbunyi `Fulfilled`. Keduanya akan
menyatakan dua hal berbeda tentang permintaan yang sama, dan daftar "menunggu wadah" pada `AC-91`
akan kehilangan satu baris yang sebenarnya masih menunggu.

Satu tempat, satu pemanggilan, di dalam `CancelAsync` wadah tunggal.

### 3.6 Keputusan penafsiran yang perlu diketahui pemilik modul

**Permintaan yang sudah dibatalkan diperlakukan sebagai tidak ada pada daftar terpesan**, sehingga
memasukkannya ke wadah ditolak `VAL-68`. Matriks tidak menyebutkan hal ini secara eksplisit.
Penafsiran sebaliknya — membiarkannya lolos — berarti permintaan yang sudah dicabut hidup kembali
lewat pintu wadah tanpa ada yang memutuskannya. Bila pemilik modul menghendaki sebaliknya, satu
baris pada `EnsureOrderedProcedureGuardAsync` yang berubah.

---

## 4. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `ACTIVE`, prefix `Lab` | `PASS` | Bagian preflight |
| Review diff/scope | 1 berkas source, nol migration | `PASS` | 3.1 |
| `dotnet build -p:RunAnalyzers=False --no-incremental` | **0 Error, 207 Warning** | `PASS` | Nol warning dari berkas task ini |
| **`T-68a`** `VAL-68` | **422**, pesan **sama persis** dengan matriks | `PASS` | 4.1 |
| **`T-69a`** `VAL-69` | **409**, pesan **sama persis** dengan matriks | `PASS` | 4.1 |
| **`T-91a`** tertaut | Hemoglobin → `Fulfilled`, menunjuk baris pemeriksaannya | `PASS` | 4.1 |
| **`T-91a`** menunggu | Leukosit → `Ordered`, terbaca belum berwadah | `PASS` | 4.1 |
| **`T-68b`** pesanan lama tidak tersentuh | Pesanan 0 baris terpesan **menerima** wadah | `PASS` | 4.2 |
| **Kontrol** penjagaan memang aktif | Permintaan **identik**: ditolak di satu pesanan, diterima di pesanan lain | `PASS` | 4.2 |
| Wadah dibatalkan → boleh diwadahi ulang | Penanda dilepas, perencanaan ulang berhasil | `PASS` | 4.1 |
| Kebersihan | 5 / 0 / 5 / 7 sebelum **dan** sesudah | `PASS` | 4.3 |

### 4.1 Dijalankan terhadap `QuilvianNewDevYoga`, bukan ditelusuri pada source

`LabSpecimenService` yang sebenarnya dipanggil di dalam transaksi yang di-`ROLLBACK`.

```
Bahan uji: dipesan = Hemoglobin + Leukosit
           di luar pesanan = Trombosit

T-68a  [LULUS] 422 LabSpecimenValidationException
       "Pemeriksaan ini tidak ada pada daftar yang dipesan untuk pasien ini."
T-91a  [LULUS] Hemoglobin -> Fulfilled, tautan menunjuk pemeriksaannya
T-91a  [LULUS] Leukosit   -> Ordered, belum berwadah
T-69a  [LULUS] 409 LabSpecimenConflictException
       "Pemeriksaan ini sudah masuk wadah lain."
       [LULUS] wadah dibatalkan -> Hemoglobin kembali Ordered, tautan dilepas
       [LULUS] boleh diwadahi ulang — petugas tidak terkunci
```

Kedua pesan dibandingkan **kata demi kata** terhadap `contracts/validation-matrix.md` baris 195
dan 196, bukan dinilai mirip.

### 4.2 Butir DoD yang paling mudah dilewatkan: membuktikan **ketiadaan** perubahan

```
T-68b  [LULUS] pesanan 5afcc717 (0 baris terpesan) MENERIMA wadah 0bc82ecf
               atas Trombosit — persis seperti sebelumnya
Kontrol[LULUS] permintaan identik: ditolak pada pesanan berbaris terpesan,
               diterima pada pesanan lama
```

Baris kontrol itulah yang membuat `T-68b` bermakna. Tanpanya, "pesanan lama menerima wadah" bisa
saja lulus karena penjagaannya tidak pernah berjalan sama sekali. **Permintaan yang dipakai pada
keduanya sama persis** — pemeriksaan `Trombosit` yang tidak ada pada daftar terpesan — dan hasilnya
berbeda hanya karena satu pesanan punya baris terpesan dan satunya tidak.

### 4.3 Kebersihan

```
SEBELUM  pesanan=5 terpesan=0 wadah=5 pemeriksaan=7
SESUDAH  pesanan=5 terpesan=0 wadah=5 pemeriksaan=7
```

### 4.4 Alat verifikasinya

Satu program sekali pakai di **scratchpad sesi, di luar repository**, merujuk project aplikasi
sehingga memakai `ApplicationDbContext`, `LabSpecimenService`, dan `LabOrderService` **yang
sebenarnya**. `ClinicalMilestoneFactProducer` dilewatkan `null`: jalur yang diuji tidak
menyentuhnya — pembatalan dilakukan atas wadah berstatus `Planned`, bukan `Accepted`.

Uji lewat Swagger: `NOT FEASIBLE` pada sesi ini. Yang belum terbukti hanyalah lapisan HTTP-nya —
routing, `[AccessPermission]`, dan pemetaan `422`/`409` yang sudah ada sebelumnya.

---

## 5. Acceptance criteria dan Definition of Done

| Butir | Status | Bukti |
| --- | --- | --- |
| `AC-91` — setiap pemeriksaan yang dipesan dapat ditelusuri ke baris yang memenuhinya; yang belum berwadah terbaca menunggu | ✅ **Terpenuhi** | 4.1 — keduanya terbukti pada database |
| `VAL-68` menolak sesuai matriks | ✅ | `422`, pesan sama persis |
| `VAL-69` menolak sesuai matriks | ✅ | `409`, pesan sama persis |
| Keduanya terbukti **tidak menyentuh** pesanan tanpa baris terpesan | ✅ | 4.2, beserta baris kontrolnya |
| Baris terpesan tertaut ke pemeriksaan yang memenuhinya | ✅ | 4.1 |
| Nol migration | ✅ | Nol perubahan pada `Models/` maupun `Migrations/` |

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari berkas task ini |
| Masalah yang diketahui | Pembatalan **pesanan** (bukan wadah) tidak menyentuh baris terpesan — lihat 6.1 |
| Risiko tersisa | Rendah. Pengetatan hanya mengenai pesanan yang punya baris terpesan, dan seluruh pesanan yang ada saat ini tidak punya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 1 berkas source, laporan ini, pembaruan roadmap dan traceability. Tidak ada `git add`, `commit`, maupun `push` |

### 6.1 Satu batas yang sengaja tidak dilewati

Ketika **pesanan** dibatalkan seluruhnya, seluruh wadahnya ikut dibatalkan lewat jalur massal yang
berbeda dari `CancelAsync` wadah tunggal. Jalur itu **tidak** disentuh task ini: baris terpesan
pada pesanan yang dibatalkan mempertahankan penandaan terakhirnya.

Ini tidak mengunci siapa pun — pesanannya sendiri sudah dibatalkan, sehingga tidak ada wadah baru
yang boleh dibuat atasnya. Tetapi bila kelak ada layar yang membaca "menunggu wadah" tanpa
menyaring status pesanan, baris-baris itu akan ikut terbaca. Menutupnya berarti memutuskan apakah
permintaan pada pesanan yang dibatalkan menjadi `Cancelled` — keputusan yang belum pernah diambil,
dan tempatnya bukan di sini.

### 6.2 Langkah berikutnya

1. **`FE-LAB-14`** — layar pendaftaran lab; endpointnya sudah ada sejak `BE-LAB-27`.
2. **`BE-EXT-05`** — kunjungan dari sesi kiosk, milik `registration-management`, risiko tinggi.
3. **`AC-87`** masih menunggu bahan uji, bukan menunggu kode — katalog lab 10 dari 10 sudah
   tergolong disiplin.
