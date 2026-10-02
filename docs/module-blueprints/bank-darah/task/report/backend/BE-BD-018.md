# Laporan Perubahan Backend — `BE-BD-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-018` |
| Judul | Order darah terbaca layar: penahanan ganda, kategori pembatalan, dan daftar kerja |
| Slice | Amendment kontrak `v5` D2/D3/D4 — Blood Order (`roadmap/backend-roadmap.md` revisi 12) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 5, kartu `BE-BD-018` |
| Trace | `DEC-BD-056`, `DEC-BD-057`, `DEC-BD-058`; `DEC-BD-044`, `DEC-BD-054`; `BD-DOM-17`, `BD-XINV-01`; `INV-BD-035` |
| Contract version | `v5` — `approved` oleh `Sukmagp` 19 September 2026 (gerbang `G5` tertutup) |
| Dependency | `BE-BD-003` ✅, `BE-BD-010` ✅, gerbang `G5` ✅ |
| Klasifikasi | `HEAVY` — skor 7 (repo 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 2, kontrak API 2, database 0, keamanan 1, UI 0), dinaikkan satu tingkat karena logika bisnis dan kontrak API sama-sama bernilai 2 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source `Areas/HealthServices/BloodBankManagement/**` dan laporan/roadmap di `docs/module-blueprints/bank-darah/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `886a60ba91dd79e035736a32a4e18299d9714901` — source task ini dibawa commit `7ac6f610` |
| Tanggal | 23 September 2026 |
| Status | ✅ **SELESAI — 23 September 2026. Keenam acceptance terbukti dan Definition of Done terpenuhi.** Build `0 Error(s)` / `214 Warning(s)`; **nol migration** sesuai DoD dan `has-pending-model-changes` bersih; QBE Strict `PASS`; validasi runtime R1–R10 `PASS` dengan dua aktor non-SuperAdmin (atestasi pemilik `Sukmagp`, bagian 5.1); catatan koreksi `BE-BD-003` §2.3 ditambahkan. **Batas bukti:** runtime berupa pernyataan hasil pemilik tanpa log primer. **Riwayat:** 🟡 SEBAGIAN — nol dari 6 terbukti runtime, 23 September 2026 sebelum validasi runtime |

---

## 1. Masalah yang diperbaiki

Layar order darah **tidak dapat dibangun** dengan kontrak `v4`. Tiga hal yang dibutuhkannya tidak
pernah sampai ke layar, walaupun backend sebenarnya sudah menghitungnya.

**Pertama, penahanan order ganda tidak terbaca mesin.** Ketika sistem menahan order karena pasien
sudah punya order aktif untuk komponen yang sama, jawabannya hanya berupa kalimat untuk dibaca
manusia. Daftar komponen yang benar-benar bentrok sudah dihitung di dalam service, lalu **dibuang**
controller. Akibatnya layar hanya punya dua pilihan buruk: mencocokkan kalimat pesan — yang rapuh dan
rusak begitu kalimatnya diperbaiki — atau tidak menandai komponen yang bentrok sama sekali.

**Kedua, layar tidak tahu alasan pembatalan mana yang sah.** Aturannya ada, tetapi hanya hidup di
dalam `POST /cancel`: dokter peminta wajib memakai alasan klinis, petugas Bank Darah wajib memakai
alasan operasional. Layar tidak punya cara mengetahuinya sebelum mengirim, sehingga satu-satunya cara
menampilkan daftar alasan yang benar adalah menyalin aturan kepemilikan itu ke frontend — dan salinan
aturan selalu berakhir berbeda dari aslinya.

**Ketiga, daftar kerja order darah tidak dapat diisi.** Skema `FE-BD-01` menuntut kolom komponen,
kolom "diminta/diberikan", dan saringan komponen. Ketiganya tidak ada pada `GET /blood-orders`.
Satu-satunya jalan adalah memanggil detail satu per satu untuk setiap baris di halaman — yaitu
tepat masalah N+1 yang akan membuat daftar melambat seiring bertambahnya order.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Layar order darah dapat mengenali penahanan ganda, mengetahui kategori alasan pembatalan dari backend, dan menampilkan daftar kerja lengkap dari satu permintaan |
| Pelaku | Dokter peminta, petugas Bank Darah |
| Pemicu | Membuka daftar order, membuat order yang ternyata ganda, atau membuka detail order untuk dibatalkan |

### 2.2 Langkah normal — daftar kerja

1. Petugas membuka layar Daftar Order Darah.
2. Layar memanggil `GET /blood-orders` **satu kali** dengan saringan dan halaman yang dipilih.
3. Backend mengambil satu halaman order, lalu mengambil seluruh baris milik halaman itu dalam
   **satu** permintaan, lalu menghitung jumlah kantong yang sudah diberikan untuk seluruh baris itu
   dalam **satu** permintaan lagi.
4. Layar menampilkan kolom komponen dan kolom "diminta/diberikan" apa adanya, **tanpa** menghitung
   sendiri dan **tanpa** meminta detail per baris.

Jumlah permintaan ke database tetap **tiga** untuk satu halaman, baik isinya sepuluh order maupun
seratus order.

### 2.3 Langkah normal — penahanan order ganda

1. Dokter memesan PRC dan trombosit pada kunjungan yang sudah punya order PRC aktif.
2. Backend menahan permintaan dan memulangkan `422`.
3. Jawabannya memuat `errors.code = "VAL-BD-001"` dan `errors.duplicateComponentIds` berisi
   **hanya** komponen yang benar-benar bentrok — PRC saja, bukan trombosit.
4. Layar mengenali penahanan dari `errors.code`, **bukan** dari kalimat pesan, lalu menandai PRC.
5. Bila order memang tetap diperlukan, petugas menulis alasan dan layar mengirim
   `POST /confirm-duplicate` dengan isian yang sama persis.

### 2.4 Langkah normal — kategori alasan pembatalan

1. Petugas membuka detail order.
2. Backend menghitung `cancellationReasonCategory` dengan aturan **yang sama persis** dengan yang
   dipakai `POST /cancel` — bukan salinannya, melainkan fungsi yang sama.
3. Bila pembuka adalah dokter peminta order itu → `OrderCancellationClinical`.
4. Bila pembuka orang lain yang berhak membatalkan → `OrderCancellationOperational`.
5. Bila order sudah tidak dapat dibatalkan → **kosong**, dan layar menyembunyikan tombol Batalkan.
6. Layar memuat daftar alasan untuk kategori itu lewat
   `GET /master-data/blood-bank-reasons/options?category=<nilai>`.

Nilai ini **tidak disimpan**. Ia dihitung ulang setiap kali detail dibaca, karena jawabannya
bergantung pada siapa yang membaca.

### 2.5 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Order ganda | Tertahan; `errors.code` dan komponen bentrok terkirim | `422` `VAL-BD-001` |
| Order ditolak karena sebab lain | Bentuk jawaban **tidak berubah** dari `v4`; `errors` tetap kosong | `400` / `403` / `404` / `422` |
| Order sudah terminal, lalu detailnya dibuka | `cancellationReasonCategory` kosong; tombol Batalkan tidak tampil | `200` |
| Pembatalan dikirim dengan alasan kategori lawannya | Ditolak — backend tetap penentu, bukan layar | `422` `VAL-BD-083` |
| Pemegang `BloodOrder : Cancel` tanpa `BloodBankReason : Read` | Daftar alasan gagal dimuat, sehingga pembatalan tidak dapat diselesaikan | `403` pada endpoint alasan |

Baris terakhir adalah **ketergantungan hak akses yang disengaja** (`DEC-BD-057`): kategori datang dari
order, tetapi daftar alasannya datang dari master data yang dijaga butir hak akses tersendiri.

### 2.6 Contoh berangka

Order PRC 2 kantong. Satu kantong sudah `Issued`, lalu satu koreksi pemberian disetujui.

| Sumber angka | Diminta | Diberikan |
| --- | ---: | ---: |
| `GET /blood-orders` — kolom daftar | 2 | 0 |
| `GET /blood-orders/{id}/fulfillment` | 2 | 0 |

Keduanya wajib sama, dan **dijamin sama secara struktur**: daftar dan ringkasan pemenuhan memanggil
fungsi penghitung yang sama, yaitu `CountIssuedByLineIdsAsync`. Fungsi itu hanya menghitung kantong
berstatus `Issued` yang alokasinya masih aktif **dan** tidak punya koreksi pemberian berstatus
`Approved`. Karena koreksinya sudah disetujui, satu kantong itu gugur dari hitungan, sehingga
diberikan kembali menjadi 0 — di kedua tempat sekaligus.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Alasan diperiksa |
| --- | --- |
| `contracts/api-contract.md` | Bentuk `errors`, isian `cancellationReasonCategory`, dan bentuk `BloodOrderListDto` pada `v5` |
| `contracts/validation-matrix.md` | Bentuk `errors` untuk `VAL-BD-001`; aturan `VAL-BD-083` |
| `contracts/permission-audit-matrix.md` bagian 2 | Ketergantungan `BloodOrder : Cancel` → `BloodBankReason : Read` |
| `testing/acceptance-test-matrix.md` bagian 11 | Skenario `AC-BD-107` sampai `AC-BD-112` |
| `00-interview-decisions.md` bagian 8.32 | Isi keputusan `DEC-BD-056`, `DEC-BD-057`, `DEC-BD-058` |
| `Areas/.../BbkBloodUnitController.cs` | Konvensi slot `errors` yang dipakai ulang |
| `Areas/.../BbkBloodOrderService.cs` | Aturan `IsRequestingDoctorAsync`, perhitungan `GetFulfillmentAsync` |
| `Areas/HealthServices/MasterData/Controllers/BloodBankReasonController.cs` | Hak akses pada `GET /options` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodOrderController.cs` | `MapFailure` mengisi slot `errors` dengan `code` dan `duplicateComponentIds` **hanya** ketika hasilnya penahanan ganda; parameter query `bloodComponentId` pada daftar; `BuildDetailAsync` menyalurkan seluruh jawaban tulis lewat `GetDetailAsync` dengan aktor saat ini |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodOrderService.cs` | `GetCancellationReasonCategoryAsync` sebagai turunan yang tidak disimpan; proyeksi `Components[]` dan `TotalIssuedQuantity` pada daftar, dihitung berkelompok; `CountIssuedByLineIdsAsync` versi batch yang dipakai bersama oleh daftar dan ringkasan pemenuhan; penyaring `bloodComponentId` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodOrderDtos.cs` | `BloodOrderListComponentDto` baru; `Components`, `TotalRequestedQuantity`, `TotalIssuedQuantity` pada `BloodOrderListDto`; `CancellationReasonCategory` pada `BloodOrderDetailDto` |

Nol migration. Nol perubahan database.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /blood-orders` menerima query `bloodComponentId` dan memulangkan `components[]`, `totalRequestedQuantity`, `totalIssuedQuantity`. Jawaban `422 VAL-BD-001` dari `POST /` dan `POST /manual` membawa slot `errors`. Seluruh jawaban yang memulangkan detail membawa `cancellationReasonCategory`. **Seluruhnya aditif** — nol klien lama yang rusak |
| Database | `NOT APPLICABLE` — nol migration, nol kolom baru, nol penghitung tersimpan. `has-pending-model-changes` bersih |
| Keamanan/Auth | Nol butir hak akses baru. Task ini **memperkenalkan ketergantungan** antarbutir yang sudah ada: kebijakan yang memberi `BloodOrder : Cancel` wajib ikut memberi `BloodBankReason : Read`, jika tidak, pembatalan tidak dapat diselesaikan dari layar. Pemeriksaan kebijakan yang berlaku adalah `AC-BD-112` dan **belum dijalankan** |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` / Blood Bank |
| Submodule | `NOT APPLICABLE` |
| Pemilik / prefix registry | Prefix `Bbk`, lifecycle **`ACTIVE`** |
| Keberlakuan | `TOUCHED LEGACY` — perubahan perilaku pada controller/service/DTO yang sudah ada; nol entity baru |
| Status registry | Terdaftar dan aktif |
| QBE ID yang berlaku | `QBE-NAM-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-SVC-001` |
| Hasil QBE Strict | **`PASS`** — 8 berkas dievaluasi, `VIOLATION 0` / `REVIEW 0` / `INFO 0` |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/blood-bank-management/blood-orders` | Daftar kerja order darah, kini dengan saringan komponen serta kolom komponen dan pemenuhan | `BloodOrder : Read` |
| `GET` | `/api/v1/health-services/blood-bank-management/blood-orders/{id}` | Detail order, kini menyertakan kategori alasan pembatalan yang berlaku bagi pembacanya | `BloodOrder : Read` |
| `POST` | `/api/v1/health-services/blood-bank-management/blood-orders` | Membuat order; penahanan ganda kini terbaca mesin | `BloodOrder : Create` |
| `POST` | `/api/v1/health-services/blood-bank-management/blood-orders/manual` | Membuat order manual; penahanan ganda kini terbaca mesin | `BloodOrder : Create` |
| `POST` | `/api/v1/health-services/blood-bank-management/blood-orders/{id}/cancel` | Membatalkan order dengan alasan terkendali; `VAL-BD-083` tetap penentu | `BloodOrder : Cancel` |

#### Health Services / Master Data / Blood Bank Reason

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/master-data/blood-bank-reasons/options?category=` | Daftar alasan aktif untuk satu kategori; dipanggil layar setelah membaca `cancellationReasonCategory` | `BloodBankReason : Read` |

**Bentuk slot `errors` pada penahanan ganda.**

```json
{
  "statusCode": 422,
  "message": "<kalimat VAL-BD-001 persis dari validation-matrix.md>",
  "errors": {
    "code": "VAL-BD-001",
    "duplicateComponentIds": ["<guid komponen yang benar-benar bentrok>"]
  }
}
```

Kegagalan order selain penahanan ganda **tetap** memulangkan `errors` kosong, sama seperti `v4`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **`0 Error(s)`, `214 Warning(s)`** | `PASS` | Keluaran perintah, 23 September 2026 |
| Peringatan pada berkas task ini | **Nol** | `PASS` | Penyaringan keluaran build |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." — sesuai DoD "nol migration" | `PASS` | Keluaran perintah |
| QBE Strict, `GitRange` `2bd9fc2a..HEAD` | `VIOLATION 0` / `REVIEW 0` / `INFO 0` | `PASS` | Keluaran `Invoke-QbeConformanceCheck.ps1` |
| Slot `errors` hanya terisi pada penahanan ganda | `MapFailure` mengisi `errors` di dalam satu `if` untuk `DuplicateOrder`; cabang lain memakai `errors` yang tetap kosong | `PASS` | `BbkBloodOrderController.cs` baris 306–328 |
| Angka daftar dijamin sama dengan `fulfillment` | Daftar dan ringkasan pemenuhan memanggil **fungsi yang sama**, `CountIssuedByLineIdsAsync` | `PASS` | `BbkBloodOrderService.cs` baris 239 dan 1130 |
| Koreksi pemberian dihormati | Penghitung mengecualikan kantong yang punya `BbkIssuanceCorrection` berstatus `Approved` | `PASS` | `BbkBloodOrderService.cs` baris 1155–1158 |
| Tanpa N+1 | Tepat tiga permintaan database untuk satu halaman, tidak bertambah mengikuti jumlah baris | `PASS` | `BbkBloodOrderService.cs` baris 214, 224, 239 |
| Kategori pembatalan memakai aturan yang sama dengan `CancelAsync` | Keduanya memanggil `IsRequestingDoctorAsync` yang sama; nol salinan aturan | `PASS` | `BbkBloodOrderService.cs` baris 516 dan 1303 |
| Kategori tersedia pada seluruh jawaban yang memulangkan detail | `GET /{id}` memakai `GetCurrentUserId()`; seluruh jawaban tulis disalurkan `BuildDetailAsync` ke `GetDetailAsync` dengan aktor yang sama | `PASS` | `BbkBloodOrderController.cs` baris 113 dan `BuildDetailAsync` |
| Kategori tidak disimpan | Nol kolom, nol properti persisted; dihitung ulang setiap pembacaan | `PASS` | Tidak ada perubahan entity maupun migration |
Uji manual: **`PASS`** — dijalankan pemilik, 23 September 2026.

### 5.1 Validasi runtime — dijalankan pemilik, 23 September 2026

Runbook bagian 8 dieksekusi oleh pemilik pekerjaan, `Sukmagp`, terhadap `QuilvianNewDevSukma`,
memakai dua aktor non-SuperAdmin: aktor **A** yang tertaut ke dokter peminta, dan aktor **B**
pemegang `BloodOrder : Cancel` yang bukan dokter peminta. Pemilik melaporkan **seluruh skenario
lulus**. Hasilnya dicatat di bawah apa adanya sebagaimana dilaporkan.

| Skenario | Kriteria | Hasil yang dilaporkan | Klasifikasi |
| --- | --- | --- | :---: |
| R1 — penahanan ganda terbaca mesin | `AC-BD-107` | `422`; `errors.code = "VAL-BD-001"`; `errors.duplicateComponentIds` berisi **hanya** komponen yang bentrok; kalimat pesan sesuai matriks validasi | `PASS` |
| R2 — kegagalan lain tidak berubah | `AC-BD-107` | Ditolak dengan bentuk jawaban `v4`; slot `errors` **kosong** | `PASS` |
| R3 — kategori bagi dokter peminta | `AC-BD-108` | Aktor **A** menerima `cancellationReasonCategory = "OrderCancellationClinical"` | `PASS` |
| R4 — kategori bagi petugas lain | `AC-BD-108` | Aktor **B** menerima `cancellationReasonCategory = "OrderCancellationOperational"` | `PASS` |
| R5 — order terminal | `AC-BD-108` | Sesudah dibatalkan, `cancellationReasonCategory` **kosong** | `PASS` |
| R6 — kategori lawan ditolak | `AC-BD-108` | Ditolak `422` `VAL-BD-083`; backend tetap penentu | `PASS` |
| R7 — saringan komponen | `AC-BD-109` | Order "hanya PRC" dan "PRC + trombosit" muncul, "hanya trombosit" **tidak**; paging dan saringan lain tetap berlaku | `PASS` |
| R8 — angka daftar = angka pemenuhan | `AC-BD-110` | `issuedQuantity` dan `totalIssuedQuantity` pada daftar **sama persis** dengan `GET /{id}/fulfillment`, sebelum maupun sesudah koreksi pemberian disetujui | `PASS` |
| R9 — tanpa N+1 | `AC-BD-111` | Jumlah kueri **tetap** untuk satu halaman dan tidak bertambah mengikuti jumlah baris | `PASS` |
| R10 — ketergantungan hak akses | `AC-BD-112` | Setiap kebijakan akses yang memberi `BloodOrder : Cancel` **juga** memberi `BloodBankReason : Read`; nol kebijakan menyimpang, nol butir hak akses baru dibuat | `PASS` |

**Batas bukti yang wajib dibaca bersama tabel di atas.** Yang tersedia adalah **pernyataan hasil dari
pemilik**, bukan log primer: tidak ada raw request/response, keluaran pencatatan kueri untuk R9,
maupun daftar kebijakan akses yang ditelusuri untuk R10. Agent tidak menyaksikan eksekusinya dan
tidak dapat mengonfirmasinya secara mandiri. Atestasi pemilik diterima sebagai bukti mengikuti
preseden penutupan `FE-BD-006` (uji pemilik R1–R8, 18 September 2026). `verify-module-readiness`
berwenang menilainya ulang.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-107` — order PRC aktif, lalu dibuat PRC + trombosit: tertahan `422`, `errors.code = "VAL-BD-001"`, `errors.duplicateComponentIds` berisi **hanya** PRC, kalimat pesan persis matriks validasi | ✅ **Terpenuhi** | Struktur: slot `errors` terisi dari `result.DuplicateComponentIds` yang dihitung service; pemetaan ke `422`. Runtime: skenario R1 `PASS` — daftar komponen bentrok terbukti berisi hanya komponen yang benar-benar bentrok (bagian 5.1) |
| `AC-BD-107` tanpa perubahan lain — order ditolak karena sebab selain ganda: bentuk jawaban tetap seperti `v4` (`errors = null`) | ✅ **Terpenuhi** | Struktur: `errors` hanya diisi di dalam cabang `DuplicateOrder`; cabang lain mewarisi `errors` yang tetap `null`. Runtime: skenario R2 `PASS` (bagian 5.1) |
| `AC-BD-108` — detail dibuka dokter peminta lalu pemegang `Cancel` lain: berturut-turut `OrderCancellationClinical` dan `OrderCancellationOperational` | ✅ **Terpenuhi** | Struktur: baris 1303–1304 memilih kategori dari `IsRequestingDoctorAsync`. Runtime: skenario R3 dan R4 `PASS` dengan **dua aktor non-SuperAdmin berbeda** (bagian 5.1) |
| `AC-BD-108` jalur gagal — order terminal → kosong; kategori lawan → `422 VAL-BD-083` | ✅ **Terpenuhi** | Struktur: `IsCancellable` menjaga bagian pertama; penjaga dua arah di baris 510–547 menjaga bagian kedua. Runtime: skenario R5 dan R6 `PASS` (bagian 5.1) |
| `AC-BD-109` — tiga order (PRC saja; PRC + trombosit; trombosit saja) disaring `bloodComponentId` = PRC: dua pertama muncul, ketiga tidak; paging dan saringan lain tetap berlaku | ✅ **Terpenuhi** | Struktur: parameter query diteruskan ke service. Runtime: skenario R7 `PASS`, termasuk pembuktian bahwa paging tetap berlaku bersamaan (bagian 5.1) |
| `AC-BD-110` — `components[].issuedQuantity` dan `totalIssuedQuantity` pada daftar sama persis dengan `GET /{id}/fulfillment`, sebelum dan sesudah koreksi | ✅ **Terpenuhi** | Struktur: **dijamin oleh konstruksi** — kedua jalur memanggil `CountIssuedByLineIdsAsync` yang sama, dan fungsi itu mengecualikan koreksi `Approved`. Runtime: skenario R8 `PASS` sebelum dan sesudah koreksi disetujui (bagian 5.1) |
| `AC-BD-111` — satu halaman berukuran maksimum berisi order multi-baris: proyeksi dihitung berkelompok, jumlah kueri **tidak** tumbuh mengikuti jumlah baris; nol penghitung tersimpan | ✅ **Terpenuhi** | Struktur: tiga permintaan tetap per halaman, terbaca dari source; nol kolom penghitung dan nol migration. Runtime: skenario R9 `PASS` — jumlah kueri tidak bertambah mengikuti jumlah baris (bagian 5.1) |
| `AC-BD-112` hak akses — setiap kebijakan akses pengembangan yang memberi `BloodOrder : Cancel` juga memberi `BloodBankReason : Read`; yang tidak memenuhi dicatat sebagai temuan tanpa membuat butir baru | ✅ **Terpenuhi** | Runtime: skenario R10 `PASS` — seluruh kebijakan yang memberi `BloodOrder : Cancel` terbukti juga memberi `BloodBankReason : Read`, **nol** kebijakan menyimpang. Nol butir hak akses baru dibuat (bagian 5.1) |

### Definition of Done

| Butir | Status |
| --- | --- |
| Keenam acceptance terbukti | ✅ **Terpenuhi** — `AC-BD-107` sampai `AC-BD-112` seluruhnya terbukti; bukti struktur dari source dan bukti runtime dari atestasi pemilik 23 September 2026 |
| Bentuk `errors` sama persis dengan `api-contract.md` | ✅ Terbukti dari source dan dikuatkan R1 serta R2 |
| Angka daftar sama dengan `GET /{id}/fulfillment` | ✅ Dijamin oleh pemakaian fungsi yang sama dan terbukti runtime lewat R8 |
| Nol migration | ✅ Terbukti — `has-pending-model-changes` bersih, nol berkas migration ditambahkan task ini |
| Catatan koreksi pada laporan `BE-BD-003` ada | ✅ Ditambahkan 23 September 2026 pada [`BE-BD-003.md`](BE-BD-003.md) bagian 2.3 |
| Laporan tracked `task/report/backend/BE-BD-018.md` | ✅ Berkas ini |

**Definition of Done terpenuhi seluruhnya.** Satu batas yang tetap melekat dan tidak menggugurkan
DoD: bukti runtime berupa atestasi pemilik tanpa log primer — lihat bagian 5.1 dan bagian 7.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan build berasal dari berkas yang disentuh task ini |
| Masalah yang diketahui | **Satu jalur pinggir pada kategori pembatalan.** `BuildDetailAsync` memulangkan `GetDetailAsync(...)` dan, bila hasilnya kosong, jatuh ke `ToDetail(entity)` yang **tidak** mengisi `cancellationReasonCategory`. Jalur itu hanya tercapai bila order lenyap di antara penulisan dan pembacaan ulang dalam satu permintaan. Akibatnya bagi pengguna kecil — tombol Batalkan tidak tampil sampai layar dimuat ulang — dan tidak pernah menampilkan kategori yang salah. Dilaporkan, tidak diperbaiki, karena perbaikannya di luar scope yang diberi wewenang |
| Risiko tersisa | **(1) Mutu bukti runtime.** Seluruh acceptance runtime bersandar pada **pernyataan hasil pemilik tanpa log primer** — nol raw request/response, nol keluaran pencatatan kueri untuk `AC-BD-111`, nol daftar kebijakan akses yang ditelusuri untuk `AC-BD-112`. Agent tidak menyaksikan eksekusinya. `verify-module-readiness` berwenang menilainya ulang sebelum sign-off modul. **(2) `AC-BD-112` bersifat potret sesaat.** Kesesuaian kebijakan terbukti pada keadaan `QuilvianNewDevSukma` hari ini, tetapi kebijakan akses diubah admin lewat layar Manajemen Role kapan saja. Kebijakan **baru** yang memberi `BloodOrder : Cancel` tanpa `BloodBankReason : Read` akan memunculkan kembali cacatnya — pemegangnya melihat tombol Batalkan tetapi tidak pernah bisa menyelesaikan pembatalan. Tidak ada penjaga otomatis untuk ini; `DEC-BD-057` menetapkannya sebagai kesepakatan, bukan aturan yang ditegakkan kode. **(3)** Beban kueri `AC-BD-111` terbukti tidak tumbuh mengikuti jumlah baris, tetapi angka absolutnya pada data produksi belum pernah diukur |
| Perubahan sampingan | `NONE` — nol berkas source diubah pada sesi ini |
| Interupsi | Satu kali, pada `dotnet build`; rinciannya pada laporan `BE-BD-017` bagian 7 |
| Status Git | `git status --short` kosong sebelum sesi ini menulis laporan dan register |
| Langkah berikutnya | Task ini **tertutup**. `BE-BD-017` dan `BE-BD-018` sama-sama ✅, sehingga **`FE-BD-002` kini terbuka** — seluruh prasyarat backend-nya terpenuhi. Disarankan memeriksa ulang `AC-BD-112` setiap kali kebijakan akses baru dibuat |

---

## 8. Runbook uji runtime — SUDAH DIEKSEKUSI 23 September 2026

> **Status: seluruh skenario di bawah dilaporkan `PASS` oleh pemilik `Sukmagp`, 23 September 2026**,
> memakai dua aktor non-SuperAdmin. Hasil per skenario tercatat pada bagian 5.1. Runbook
> dipertahankan apa adanya sebagai definisi skenario yang dipakai, supaya uji ini dapat diulang
> identik di lingkungan lain — khususnya R10, yang perlu diulang setiap kali kebijakan akses baru
> dibuat.

Seluruh skenario terhadap `QuilvianNewDevSukma`. Butuh **dua aktor berbeda**: aktor A yang tertaut ke
dokter peminta order, dan aktor B yang memegang `BloodOrder : Cancel` tetapi **bukan** dokter peminta
order itu. Keduanya non-SuperAdmin.

| # | Skenario | Permintaan | Yang diharapkan |
| :---: | --- | --- | --- |
| R1 | Penahanan ganda terbaca mesin | Buat order PRC aktif untuk satu kunjungan. Lalu `POST /blood-orders` untuk kunjungan yang sama, berisi **PRC + trombosit** | `422`; `errors.code = "VAL-BD-001"`; `errors.duplicateComponentIds` berisi **hanya** id PRC; kalimat pesan persis `validation-matrix.md` |
| R2 | Kegagalan lain tidak berubah | `POST /blood-orders` dengan jumlah kantong `0` | `400` `VAL-BD-002`; `errors` **kosong** |
| R3 | Kategori bagi dokter peminta | Aktor **A** memanggil `GET /blood-orders/{id}` untuk order yang ia minta | `cancellationReasonCategory = "OrderCancellationClinical"` |
| R4 | Kategori bagi petugas lain | Aktor **B** memanggil `GET /blood-orders/{id}` untuk order yang sama | `cancellationReasonCategory = "OrderCancellationOperational"` |
| R5 | Order terminal | Batalkan order, lalu `GET /blood-orders/{id}` lagi | `cancellationReasonCategory` **kosong** |
| R6 | Kategori lawan ditolak | Aktor **A** mengirim `POST /{id}/cancel` dengan alasan berkategori **operasional** | `422` `VAL-BD-083` |
| R7 | Saringan komponen | Siapkan tiga order: hanya PRC; PRC + trombosit; hanya trombosit. Panggil `GET /blood-orders?bloodComponentId=<id PRC>` | Dua order pertama muncul, ketiga **tidak**. Ulangi dengan `pageSize` kecil untuk memastikan paging tetap berlaku |
| R8 | Angka daftar = angka pemenuhan | Order PRC 2 kantong, satu kantong `Issued`. Bandingkan `GET /blood-orders` dengan `GET /blood-orders/{id}/fulfillment`. Lalu ajukan dan **setujui** satu koreksi pemberian, dan bandingkan lagi | Angka `issuedQuantity` dan `totalIssuedQuantity` **sama persis** di kedua tempat, sebelum maupun sesudah koreksi |
| R9 | Tanpa N+1 | Aktifkan pencatatan kueri EF, lalu panggil `GET /blood-orders` dengan halaman berukuran maksimum berisi order multi-baris | Jumlah kueri **tetap tiga**, tidak bertambah mengikuti jumlah baris |
| R10 | Ketergantungan hak akses | Buka Pengaturan → Manajemen Role → Akses Role. Telusuri **setiap** kebijakan yang mencentang `BloodOrder : Cancel` | Setiap kebijakan itu juga mencentang `BloodBankReason : Read`. Catat kebijakan yang tidak memenuhinya sebagai temuan — **jangan** membuat butir hak akses baru |

Kirim balik status HTTP, badan jawaban, dan untuk R9 jumlah kueri yang tercatat. Saya perbarui
laporan ini apa adanya dari hasil itu.
