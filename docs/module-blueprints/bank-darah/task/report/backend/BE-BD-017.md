# Laporan Perubahan Backend — `BE-BD-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-017` |
| Judul | Golongan darah diminta tercatat pada order darah |
| Slice | Amendment kontrak `v5` D1 — Blood Order (`roadmap/backend-roadmap.md` revisi 12) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 5, kartu `BE-BD-017` |
| Trace | `DEC-BD-055`; `FE-BD-001`, `ASM-BD-004`; `INV-BD-011`, `INV-BD-014`; `BD-AGG-01` |
| Contract version | `v5` — `approved` oleh `Sukmagp` 19 September 2026 (gerbang `G5` tertutup; persetujuan proses klinis `DEC-BD-055` diberikan hari yang sama) |
| Dependency | `BE-BD-003` ✅, `BE-BD-005` ✅, gerbang `G5` ✅, `DEC-BD-055` ✅ |
| Klasifikasi | `HEAVY` — skor 8 (repo 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 2, database 2, keamanan 1, UI 0), dinaikkan satu tingkat karena dua faktor bernilai 2 (kontrak API dan database) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source `Areas/HealthServices/BloodBankManagement/**`, `Migrations/**`, `Repositories/Configurations/**`, dan laporan/roadmap di `docs/module-blueprints/bank-darah/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `886a60ba91dd79e035736a32a4e18299d9714901` — source task ini dibawa commit `7ac6f610` |
| Tanggal | 23 September 2026 |
| Status | ✅ **SELESAI — 23 September 2026. Keempat acceptance terbukti dan Definition of Done terpenuhi.** Build `0 Error(s)` / `214 Warning(s)`; migration terterapkan di `QuilvianNewDevSukma` nol tertunda; `has-pending-model-changes` bersih; QBE Strict `PASS`; validasi runtime R1–R9 `PASS` (atestasi pemilik `Sukmagp`, bagian 5.1). **Batas bukti:** runtime berupa pernyataan hasil pemilik tanpa log primer. **Riwayat:** 🟡 SEBAGIAN — 1 dari 4 terbukti penuh, 23 September 2026 sebelum validasi runtime |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, **order darah tidak pernah menyebutkan golongan darah apa yang diminta.**
Dokter menulis "PRC 2 kantong" dan sistem menyimpannya begitu saja, tanpa satu tempat pun yang
mencatat "yang diminta A Positif".

Akibatnya terasa di layar. Kontrak frontend `FE-BD-001` mewajibkan layar order darah membedakan
dengan jelas **golongan darah yang diminta** dari **golongan darah hasil pemeriksaan**. Kewajiban itu
tidak mungkin dipenuhi: nilai pertamanya tidak ada di mana pun — tidak di request, tidak di DTO,
tidak di entity, tidak di kamus data. Layar hanya punya satu angka, yaitu hasil pemeriksaan, sehingga
satu-satunya cara "memenuhi" kewajiban itu adalah menampilkan hasil pemeriksaan di dua tempat dengan
dua nama berbeda — persis kekeliruan yang `INV-BD-011` larang.

**Contoh nyata.** Dokter memesan PRC untuk pasien yang di berkasnya tertulis A Positif. Sampel pasien
ternyata belum pernah diperiksa di Bank Darah. Sebelum perubahan ini, order itu tersimpan tanpa jejak
bahwa yang diminta A Positif, sehingga ketika petugas membuka order, tidak ada cara membedakan
"dokter meminta A Positif" dari "pasien terbukti A Positif". Keduanya adalah pernyataan yang sangat
berbeda: yang pertama keterangan permintaan, yang kedua dasar keselamatan transfusi.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Setiap order darah **baru** membawa keterangan golongan darah yang diminta, terpisah dan tidak pernah tertukar dengan hasil pemeriksaan |
| Pelaku | Dokter peminta (order elektronik) dan petugas Bank Darah (order manual) |
| Pemicu | Pembuatan order darah lewat `POST /`, `POST /manual`, atau `POST /confirm-duplicate` |
| Hasil | Nilai tersimpan pada `BbkBloodOrder.RequestedBloodGroup`, terbaca kembali lewat `GET /{id}` |

### 2.2 Langkah normal

1. Dokter membuka layar order darah dan memilih pasien serta kunjungan.
2. Dokter memilih komponen darah dan jumlah kantong.
3. **Dokter memilih golongan darah yang diminta** — delapan golongan ABO beserta Rhesus-nya, atau
   "Tidak diketahui" bila memang belum diketahui.
4. Sistem memeriksa kelengkapan rujukan, lalu **memeriksa golongan darah yang diminta**.
5. Baru sesudah itu sistem memeriksa order ganda dan meminta nomor order.
6. Order tersimpan beserta golongan darah yang diminta.

Urutan langkah 4 dan 5 **disengaja dan mengikat**: pemeriksaan golongan darah diminta berjalan
**sebelum** deteksi ganda dan **sebelum** nomor order diminta. Dengan begitu permintaan yang memang
tidak sah tidak pernah menghabiskan satu nomor order pun (`INV-PLT-002`).

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Golongan darah diminta tidak dikirim sama sekali | Ditolak; **nol order tersimpan, nol nomor terbit** | `400` `VAL-BD-085` |
| Golongan darah diminta bernilai "Tidak diinformasikan" (`NotDisclosed`) | Ditolak | `400` `VAL-BD-085` |
| Golongan darah diminta berupa angka di luar daftar yang dikenal | Ditolak | `400` `VAL-BD-085` |
| Golongan darah diminta bernilai "Tidak diketahui" (`Unknown`) | **Diterima**; tersimpan sebagai `Unknown`, bukan kosong | `200` |
| Order dibuat sebelum `v5`, lalu dibuka sesudahnya | `requestedBloodGroup` bernilai kosong; layar menulisnya sebagai "tidak tercatat pada order lama" | `200` |

Perbedaan antara "tidak dikirim" dan "Tidak diketahui" adalah inti keputusan `DEC-BD-055`. Keduanya
terlihat mirip tetapi bermakna berlawanan: yang pertama berarti petugas lupa mengisi, yang kedua
berarti petugas sudah menyatakan bahwa golongan darahnya memang belum diketahui. Yang pertama
ditolak, yang kedua diterima dan disimpan.

### 2.4 Batas yang tidak boleh dilanggar

Nilai ini **keterangan permintaan, bukan fakta klinis**. Ia tidak pernah menjadi golongan darah sah,
tidak pernah menjadi bukti kecocokan, tidak pernah menjadi dasar alokasi, dan tidak pernah menjadi
dasar pemberian. `INV-BD-011` tetap mengikat penuh: satu-satunya golongan darah yang boleh dipakai
untuk keputusan klinis adalah hasil pemeriksaan yang tervalidasi.

### 2.5 Contoh berangka dari awal sampai akhir

Pasien A, kunjungan `RI-001`.

1. Dokter memesan PRC 2 kantong dengan golongan darah diminta **A Positif**. Order tersimpan
   `ORD-00000010`, `RequestedBloodGroup = APositive (1)`.
2. Sampel pasien diperiksa di Bank Darah. Hasil tervalidasi ternyata **B Positif**.
3. Petugas membuka `GET /blood-orders/{id}` → `requestedBloodGroup = 1`,
   `requestedBloodGroupLabel = "A Positif"`.
4. Petugas membuka `GET /blood-group-exams/patient/{patientId}/valid` → **B Positif**.
5. Layar menampilkan keduanya dengan label berbeda: "Golongan darah diminta: A Positif" dan
   "Golongan darah hasil pemeriksaan (sah): B Positif".
6. Alokasi, bukti kecocokan, dan pemberian dinilai **hanya** terhadap B Positif. Angka A Positif
   tidak pernah ikut menentukan apa pun.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Alasan diperiksa |
| --- | --- |
| `contracts/validation-matrix.md` | Kalimat kanonik `VAL-BD-085` dan status HTTP-nya |
| `contracts/api-contract.md` | Bentuk request/response Blood Order `v5` |
| `data/data-dictionary.md` | Definisi `BbkBloodOrder.RequestedBloodGroup` |
| `testing/acceptance-test-matrix.md` bagian 11 | Skenario `AC-BD-103` sampai `AC-BD-106` |
| `00-interview-decisions.md` bagian 8.32 | Isi keputusan `DEC-BD-055` |
| `Enums/BloodType.cs` | Nilai enum yang dipakai ulang, termasuk posisi `Unknown` dan `NotDisclosed` |
| `Areas/.../BbkBloodOrderService.cs` | Urutan validasi terhadap deteksi ganda dan alokasi nomor |
| `Areas/.../BbkBloodOrderController.cs` | Pemetaan hasil ke status HTTP |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Status kepemilikan prefix `Bbk` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodOrder.cs` | Properti baru `RequestedBloodGroup` bertipe `BloodType?` pada tingkat order, bukan pada baris order |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodOrderDtos.cs` | Isian `RequestedBloodGroup` pada `CreateBloodOrderRequest` — otomatis berlaku juga bagi `CreateManualBloodOrderRequest` dan `ConfirmDuplicateOrderRequest` yang mewarisinya; `RequestedBloodGroup` dan `RequestedBloodGroupLabel` pada `BloodOrderDetailDto` |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodOrderService.cs` | Konstanta pesan `RequestedBloodGroupRequiredMessage`; penjaga `IsAcceptableRequestedBloodGroup`; pemeriksaan `VAL-BD-085` ditempatkan sebelum deteksi ganda dan alokasi nomor; pengisian nilai dan labelnya pada jawaban detail |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodOrderController.cs` | Dokumentasi endpoint dan tipe balasan `400` pada ketiga endpoint pembuatan |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodOrderConfiguration.cs` | Pemetaan kolom `RequestedBloodGroup` |
| `Migrations/20260921032324_AddBbkBloodOrderRequestedBloodGroup.cs` | Satu `ADD COLUMN` nullable bertipe `integer`, tanpa default, tanpa index, tanpa backfill |
| `Migrations/20260921032324_AddBbkBloodOrderRequestedBloodGroup.Designer.cs` | Berkas bangkitan EF |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Berkas bangkitan EF, tiga baris |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint pembuatan menerima isian baru `requestedBloodGroup` yang **wajib**; `GET /{id}` memulangkan `requestedBloodGroup` dan `requestedBloodGroupLabel`. Satu perubahan menolak klien lama — permintaan tanpa isian ini kini ditolak `400 VAL-BD-085`. Belum ada klien frontend order darah yang tayang, sehingga tidak ada pemakai yang rusak |
| Database | Satu kolom baru `BbkBloodOrder.RequestedBloodGroup`, `integer`, nullable. Migration `20260921032324_AddBbkBloodOrderRequestedBloodGroup`. **Sudah terterapkan** di `QuilvianNewDevSukma`, nol tertunda. `QuilvianNewDevTim01`, staging, dan production **belum**, dan masing-masing tetap wewenang tersendiri |
| Keamanan/Auth | `NOT APPLICABLE` — nol butir hak akses baru, nol perubahan pada penjaga endpoint. Hak akses `BloodOrder : Create` yang sudah ada tetap berlaku apa adanya |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` / Blood Bank |
| Submodule | `NOT APPLICABLE` |
| Pemilik / prefix registry | Prefix `Bbk`, lifecycle **`ACTIVE`** sejak 3 September 2026 |
| Keberlakuan | `TOUCHED LEGACY` pada `BbkBloodOrder` yang sudah ada — satu properti ditambahkan, nol entity baru |
| Status registry | Terdaftar dan aktif; `QBE-MOD-002` terpenuhi |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-NAM-001`, `QBE-MOD-002`, `QBE-DB-001` |
| Hasil QBE Strict | **`PASS`** — 8 berkas dievaluasi, `VIOLATION 0` / `REVIEW 0` / `INFO 0` |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/blood-bank-management/blood-orders` | Membuat order darah elektronik, kini wajib menyertakan golongan darah yang diminta | `BloodOrder : Create` |
| `POST` | `/api/v1/health-services/blood-bank-management/blood-orders/manual` | Membuat order darah manual oleh petugas Bank Darah, dengan kewajiban isian yang sama | `BloodOrder : Create` |
| `POST` | `/api/v1/health-services/blood-bank-management/blood-orders/confirm-duplicate` | Melanjutkan order yang tertahan deteksi ganda, mengirim ulang isian yang sama termasuk golongan darah yang diminta | `BloodOrder : Create` |
| `GET` | `/api/v1/health-services/blood-bank-management/blood-orders/{id}` | Membaca detail order beserta golongan darah yang diminta dan labelnya | `BloodOrder : Read` |

**Isian baru pada ketiga endpoint pembuatan.**

| Isian | Tipe | Wajib | Nilai yang diterima |
| --- | --- | :---: | --- |
| `requestedBloodGroup` | angka | Ya | `0` Tidak diketahui · `1` A Positif · `2` A Negatif · `3` B Positif · `4` B Negatif · `5` AB Positif · `6` AB Negatif · `7` O Positif · `8` O Negatif |

Nilai `99` ("Tidak diinformasikan") **tidak** diterima, dan isian yang tidak dikirim juga ditolak.
Keduanya memulangkan `400` dengan kalimat:

> "Golongan darah yang diminta wajib dipilih: golongan darah beserta Rhesus-nya, atau 'Tidak
> diketahui' bila memang belum diketahui."

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **`0 Error(s)`, `214 Warning(s)`**, waktu 01:02:33 | `PASS` | Keluaran perintah, 23 September 2026 |
| Peringatan pada berkas task ini | **Nol.** Tiga peringatan unik Bank Darah seluruhnya di `BbkBloodUnitService.cs` (wilayah `BE-BD-009`/`BE-BD-010`), nol di kelima berkas order | `PASS` | Penyaringan keluaran build |
| `dotnet ef migrations list` | 192 migration terdaftar, **nol** bertanda `(Pending)`, nol peringatan koneksi | `PASS` | Keluaran perintah |
| `dotnet ef database update` | `Done.` tanpa satu pun baris `Applying migration` — database **sudah** mutakhir sebelum perintah dijalankan | `PASS` | Keluaran perintah |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| QBE Strict, `GitRange` `2bd9fc2a..HEAD` | 8 berkas dievaluasi, `VIOLATION 0` / `REVIEW 0` / `INFO 0` | `PASS` | Keluaran `Invoke-QbeConformanceCheck.ps1` |
| Kalimat `VAL-BD-085` sama persis matriks validasi | Identik kata per kata | `PASS` | `BbkBloodOrderService.cs` konstanta `RequestedBloodGroupRequiredMessage` lawan `validation-matrix.md` baris 33 |
| Urutan validasi sebelum deteksi ganda dan alokasi nomor | Penjaga berada di atas pemeriksaan baris, komponen, pasien, kunjungan, deteksi ganda, dan permintaan nomor | `PASS` | `BbkBloodOrderService.cs` baris 663 |
| Nol pembacaan `RequestedBloodGroup` di luar layanan order | Pencarian seluruh `Areas/` mengembalikan nol hasil di luar lima berkas order | `PASS` | Hasil pencarian |
| Migration tanpa backfill, tanpa default, tanpa index | Hanya satu `AddColumn` nullable; `Down` menghapus kolom | `PASS` | Isi berkas migration |
Uji manual: **`PASS`** — dijalankan pemilik, 23 September 2026.

### 5.1 Validasi runtime — dijalankan pemilik, 23 September 2026

Runbook bagian 8 dieksekusi oleh pemilik pekerjaan, `Sukmagp`, terhadap `QuilvianNewDevSukma`.
Pemilik melaporkan **seluruh skenario lulus**. Hasilnya dicatat di bawah apa adanya sebagaimana
dilaporkan.

| Skenario | Kriteria | Hasil yang dilaporkan | Klasifikasi |
| --- | --- | --- | :---: |
| R1 — order elektronik dengan A Positif | `AC-BD-103` | Order tersimpan beserta golongan darah yang diminta | `PASS` |
| R2 — baca kembali detail | `AC-BD-103` | `GET /{id}` memulangkan nilai **dan** labelnya; baris order **tidak** memuat golongan darah | `PASS` |
| R3 — order manual dengan A Positif | `AC-BD-103` | Nilai tersimpan sama seperti jalur elektronik | `PASS` |
| R4 — "Tidak diketahui" diterima | `AC-BD-103` | Diterima dan tersimpan sebagai `Unknown`, **bukan** kosong | `PASS` |
| R5 — isian tidak dikirim | `AC-BD-104` | Ditolak `400` `VAL-BD-085`; nomor order **tidak** terbit | `PASS` |
| R6 — "Tidak diinformasikan" (`NotDisclosed`) | `AC-BD-104` | Ditolak `400` `VAL-BD-085` | `PASS` |
| R7 — angka di luar enum | `AC-BD-104` | Ditolak `400` `VAL-BD-085` | `PASS` |
| R7b — ketiga endpoint dijaga sama | `AC-BD-104` | `POST /`, `POST /manual`, dan `POST /confirm-duplicate` sama-sama menolak | `PASS` |
| R8 — batas klinis | `AC-BD-106` | Pemeriksaan tervalidasi tetap memulangkan B Positif; alokasi dinilai terhadap hasil pemeriksaan, **bukan** golongan darah yang diminta | `PASS` |
| R9 — order lama jujur | `AC-BD-105` | Order sebelum 21 September 2026 tetap bernilai kosong; nol backfill | `PASS` |

**Batas bukti yang wajib dibaca bersama tabel di atas.** Yang tersedia adalah **pernyataan hasil dari
pemilik**, bukan log primer: tidak ada raw request/response, nomor order yang terbit, maupun keluaran
kueri database yang dilampirkan ke laporan ini. Agent tidak menyaksikan eksekusinya dan tidak dapat
mengonfirmasinya secara mandiri. Atestasi pemilik diterima sebagai bukti mengikuti preseden penutupan
`FE-BD-006` (uji pemilik R1–R8, 18 September 2026). Keterbatasan yang sama pernah tercatat pada
`BE-BD-006` dan `verify-module-readiness` berwenang menilainya ulang.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-103` — order elektronik, manual, dan lanjutan order ganda dibuat dengan golongan darah diminta A Positif; tersimpan; `GET /{id}` memulangkan nilai dan labelnya; nilai **tidak** tersalin ke baris order | ✅ **Terpenuhi** | Struktur: isian diwarisi ketiga request dari `CreateBloodOrderRequest`; kolom berada pada `BbkBloodOrder`, **bukan** `BbkBloodOrderLine`; `RequestedBloodGroupLabel` ada pada `BloodOrderDetailDto`. Runtime: skenario R1–R3 `PASS` (bagian 5.1, atestasi pemilik) |
| `AC-BD-103` "Tidak diketahui" — diterima dan tersimpan `Unknown`, bukan kosong | ✅ **Terpenuhi** | Struktur: `IsAcceptableRequestedBloodGroup` menerima `Unknown` karena `Enum.IsDefined(0)` benar dan `0 != NotDisclosed`. Runtime: skenario R4 `PASS` (bagian 5.1) |
| `AC-BD-104` jalur gagal — tidak dikirim, `NotDisclosed`, atau di luar enum, pada ketiga endpoint → `400 VAL-BD-085`, nol order tersimpan, nomor tidak terbit | ✅ **Terpenuhi** | Struktur: penjaga menolak ketiga bentuk; `Invalid` dipetakan ke `BadRequest`; penempatannya di atas alokasi nomor. Runtime: skenario R5, R6, R7, dan R7b `PASS`, termasuk pembuktian bahwa **nomor order tidak terbit** (bagian 5.1) |
| `AC-BD-105` — order yang dibuat sebelum `v5` dibuka sesudah migration: `requestedBloodGroup = null`, tidak diisi dari `MstPatient.BloodType` maupun hasil pemeriksaan, migration tanpa backfill | ✅ **Terpenuhi** | Struktur: migration hanya `AddColumn` nullable tanpa `UPDATE` apa pun; nol kode yang menulis nilai ini di luar jalur pembuatan order. Runtime: skenario R9 `PASS` (bagian 5.1) |
| `AC-BD-106` batas klinis — order meminta A Positif sementara pemeriksaan tervalidasi B Positif: `GET /blood-group-exams/patient/{id}/valid` tetap B Positif; alokasi, bukti kecocokan, dan pemberian dinilai terhadap hasil pemeriksaan saja; nol pembacaan `RequestedBloodGroup` di luar layanan order | ✅ **Terpenuhi** | Struktur: pencarian seluruh `Areas/` membuktikan nol pembacaan di luar lima berkas order, sehingga secara struktur mustahil nilai ini memengaruhi alokasi/bukti/pemberian. Runtime: skenario R8 `PASS` — pemeriksaan tervalidasi tetap B Positif dan alokasi dinilai terhadapnya (bagian 5.1) |

### Definition of Done

| Butir | Status |
| --- | --- |
| Keempat acceptance terbukti | ✅ **Terpenuhi** — `AC-BD-103` sampai `AC-BD-106` seluruhnya terbukti; bukti struktur dari source dan bukti runtime dari atestasi pemilik 23 September 2026 |
| `VAL-BD-085` terkirim dengan kalimat persis `validation-matrix.md` | ✅ Kalimat konstanta identik kata per kata; pengirimannya terbukti runtime lewat R5–R7b |
| Nol pembacaan `RequestedBloodGroup` di luar layanan order | ✅ Terbukti dari source dan dikuatkan R8 |
| Migration terterapkan di database pengembangan yang diberi wewenang dengan `0 pending` | ✅ Terbukti di `QuilvianNewDevSukma` |
| Laporan tracked `task/report/backend/BE-BD-017.md` | ✅ Berkas ini |

**Definition of Done terpenuhi seluruhnya.** Satu batas yang tetap melekat dan tidak menggugurkan
DoD: bukti runtime berupa atestasi pemilik tanpa log primer — lihat bagian 5.1 dan bagian 7.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memulangkan `214 Warning(s)`, seluruhnya bergaya dokumentasi XML (`CS1573`, `CS1574`, `CS1587`, `CS1734`). Baseline yang tercatat manifest adalah `210` pada `8e30aa9` (11 September 2026). Selisih empat peringatan **tidak dapat saya atribusikan** tanpa membangun ulang pada SHA lama, dan itu menuntut satu jam lagi. Yang dapat dipastikan: nol peringatan berasal dari kelima berkas order yang disentuh task ini |
| Masalah yang diketahui | **Biaya build repository sangat tinggi.** Folder `Migrations/` memuat 384 berkas dengan total **12,3 juta baris**; setiap `*.Designer.cs` berukuran ±4,5 MB dan `ApplicationDbContextModelSnapshot.cs` 4,6 MB. Satu `dotnet build` memakan **1 jam 2 menit** dan puncak memori ±25 GB. Ini kondisi repository yang sudah ada, **bukan** akibat task ini — tetapi migration baru menambah 4,6 MB lagi ke beban itu, dan bebannya bertambah pada setiap migration berikutnya di seluruh modul |
| Risiko tersisa | **(1) Mutu bukti runtime.** Seluruh acceptance runtime bersandar pada **pernyataan hasil pemilik tanpa log primer** — nol raw request/response, nol nomor order, nol keluaran kueri dilampirkan. Agent tidak menyaksikan eksekusinya. Ini keterbatasan yang sama persis dengan yang sudah tercatat pada `BE-BD-006`, dan `verify-module-readiness` berwenang menilainya ulang sebelum sign-off modul. **(2)** Kolom baru sudah ada di `QuilvianNewDevSukma` saja; `QuilvianNewDevTim01`, staging, dan production belum, dan klien yang menembak database itu akan gagal. **(3)** Perubahan ini **menolak klien lama** pada ketiga endpoint pembuatan — aman hari ini karena belum ada klien order darah yang tayang, tetapi berhenti aman begitu `FE-BD-002` dirilis ke lingkungan yang databasenya belum dimigrasi |
| Perubahan sampingan | `NONE` — nol berkas diubah pada sesi ini. Source task sudah ter-commit sebagai `7ac6f610` sebelum sesi dimulai; sesi ini menambahkan verifikasi, laporan, dan register |
| Interupsi | Satu kali. `dotnet build` pertama berhenti tanpa progres selama 19 menit; dihentikan, `dotnet build-server shutdown` dijalankan, lalu build diulang sampai selesai dengan exit code 0. Tidak ada pekerjaan yang hilang karena tidak ada berkas source yang sedang disunting |
| Status Git | `git status --short` mengembalikan **kosong** sebelum sesi ini menulis laporan dan register. Nol stage, nol commit, nol push dilakukan |
| Langkah berikutnya | Task ini **tertutup**. Berikutnya `FE-BD-002`, yang kedua prasyarat backend-nya kini ✅. Bila suatu saat log primer runtime tersedia, lampirkan ke bagian 5.1 tanpa mengubah status |

---

## 8. Runbook uji runtime — SUDAH DIEKSEKUSI 23 September 2026

> **Status: seluruh skenario di bawah dilaporkan `PASS` oleh pemilik `Sukmagp`, 23 September 2026.**
> Hasil per skenario tercatat pada bagian 5.1. Runbook dipertahankan apa adanya sebagai definisi
> skenario yang dipakai, supaya uji ini dapat diulang identik di lingkungan lain.

Seluruh skenario dijalankan terhadap `QuilvianNewDevSukma`. Login lebih dulu lewat
`POST /api/v1/Auth/login`, lalu pakai token-nya pada seluruh permintaan di bawah.

**Data yang dibutuhkan:** satu `patientId`, satu `encounterId` milik pasien itu yang **masih
berjalan**, satu `serviceUnitId` yang `IsAvailableForBloodOrder`, satu `requestingDoctorId`, dan dua
`bloodComponentId` aktif (misalnya PRC dan trombosit). Seluruhnya sudah tersedia dari task
`BE-BD-003` sampai `BE-BD-013`.

| # | Skenario | Permintaan | Yang diharapkan |
| :---: | --- | --- | --- |
| R1 | Order elektronik dengan A Positif | `POST /blood-orders` body berisi rujukan lengkap, `requestedBloodGroup: 1`, satu baris PRC 2 kantong | `200`. Catat `id` dan `orderNumber` |
| R2 | Baca kembali | `GET /blood-orders/{id R1}` | `requestedBloodGroup: 1`, `requestedBloodGroupLabel: "A Positif"`. Periksa juga bahwa **baris order tidak memuat** golongan darah |
| R3 | Order manual dengan A Positif | `POST /blood-orders/manual`, body sama, `requestedBloodGroup: 1` | `200`, nilai tersimpan sama |
| R4 | "Tidak diketahui" diterima | `POST /blood-orders` dengan `requestedBloodGroup: 0` | `200`; `GET` memulangkan `0` / "Tidak diketahui", **bukan** kosong |
| R5 | Isian tidak dikirim | `POST /blood-orders` **tanpa** field `requestedBloodGroup` | `400`, pesan persis `VAL-BD-085`. Pastikan **nomor order tidak bertambah** |
| R6 | "Tidak diinformasikan" ditolak | `POST /blood-orders` dengan `requestedBloodGroup: 99` | `400` `VAL-BD-085` |
| R7 | Angka di luar enum ditolak | `POST /blood-orders` dengan `requestedBloodGroup: 77` | `400` `VAL-BD-085` |
| R7b | Ketiga endpoint dijaga sama | Ulangi R5 pada `POST /manual` dan `POST /confirm-duplicate` | `400` `VAL-BD-085` pada keduanya |
| R8 | Batas klinis | Pakai pasien yang punya pemeriksaan tervalidasi **B Positif**, lalu buat order dengan `requestedBloodGroup: 1` (A Positif). Panggil `GET /blood-group-exams/patient/{patientId}/valid`, lalu coba alokasi kantong A Positif ke order itu | `valid` tetap memulangkan **B Positif**. Alokasi dinilai terhadap B Positif, **bukan** A Positif |
| R9 | Order lama jujur | `GET /blood-orders/{id}` untuk order yang dibuat sebelum 21 September 2026 | `requestedBloodGroup: null` |

Kirim balik status HTTP, badan jawaban, dan nomor order yang terbit (atau bukti bahwa nomor tidak
terbit) untuk setiap baris. Saya perbarui laporan ini apa adanya dari hasil itu.
