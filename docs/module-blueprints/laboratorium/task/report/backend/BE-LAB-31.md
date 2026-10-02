# Laporan Perubahan Backend — `BE-LAB-31`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-31` |
| Judul | Endpoint konfirmasi pesanan |
| Slice | `EPIC-LAB-12`, gelombang `MVP-5c` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6d |
| Trace | `FR-11.12`; `LAB-DEC-061`; `AC-94`, `AC-95`; `T-94a`, `T-94b`, `T-94c`, `T-95a`, `T-95b`, `T-97c` |
| Contract version | `LAB-API-v1` **`r12`** §7.1; `LAB-VAL-v1` **`r6`** `VAL-70`..`VAL-73`; `LAB-STATE-v1` **`r3`** bagian 1a — seluruhnya `approved` 2026-09-15 |
| Dependency | `BE-LAB-30` ✅ `SELESAI` 2026-09-16 |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 1, kontrak API 1, database 1, keamanan 1, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium dan artefak blueprint. **Nol migration** |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-16 |
| Status | **✅ `SELESAI`** — seluruh butir DoD terpenuhi; `VAL-70`..`VAL-73` keempatnya terbukti menolak sesuai matriks; `T-97c` terbukti dari database. **Dua butir acceptance criteria terpenuhi sebagian**, dan penyebabnya adalah celah kontrak yang dilaporkan pada bagian 7.2 |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 |
| Keberlakuan | `NEW CODE` untuk DTO, method service, endpoint, dan exception baru; `TOUCHED LEGACY` untuk dua penyesuaian terbatas pada berkas yang sudah ada |
| Status gerbang | `QBE-MOD-002` **tidak menahan** — lifecycle `ACTIVE`, nol entity dan nol resource permission baru |
| QBE ID yang berlaku | `QBE-SVC-001` (orkestrasi di service, controller tidak menyentuh context), `QBE-API-001` (boundary, response, status, validasi yang sudah mapan), `QBE-PERM-001` (`LabOrder : Update` yang sudah ada), `QBE-DTO-001` (nol entity EF diekspos), `QBE-VAL-001` (`VAL-70`..`VAL-73`), `QBE-LOG-001` (log perubahan state menyertakan aktor), `QBE-AUD-001` (jejak audit terpisah dari application logging) |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`..`003`, `QBE-CFG-001`, `QBE-NAM-001`..`004`, `QBE-DB-001`..`002` — nol entity, nol configuration, nol migration; `QBE-CODE-001`..`006` — nol nomor bisnis; `QBE-PAGE-001` — bukan capability list |

Gate lolos bersih. Dependency `BE-LAB-30` selesai dan terverifikasi pada hari yang sama; ketiga
kontrak yang dirujuk berstatus `approved`; keenam baris uji sudah tertulis pada matriks sebelum
implementasi dimulai; `HEAD` `e2152709` cocok dengan `backend_commit_sha` pada manifest.

---

## 1. Masalah yang diperbaiki

**Keadaan sebelum perubahan.** `BE-LAB-30` menyiapkan tempat menyimpan konfirmasi — tiga kolom dan
satu nilai status. Tetapi tidak ada satu pun cara mengisinya. Lemarinya berdiri, kuncinya belum
dibuat.

**Akibat nyatanya.** Petugas laboratorium yang memeriksa pesanan masuk dan memilih dokter pemeriksa
tetap bekerja di luar sistem. Tidak ada tombol, tidak ada catatan, dan tidak ada cara menjawab
"siapa yang menyatakan pesanan ini siap dikerjakan".

**Contoh konkretnya.** Pada `QuilvianNewDevYoga` ada 2 pesanan berstatus `Requested` yang menunggu
diperiksa petugas. Sebelum task ini, satu-satunya cara memajukan keduanya adalah menunggu wadah
pertamanya dinyatakan layak — melewatkan langkah pemeriksaan pesanan sepenuhnya.

**Satu jebakan ditemukan saat mengerjakan task ini, dan ditutup.** Turunan otomatis
`Confirmed` → `Accepted` yang diwajibkan `LAB-STATE-v1` `r3` bagian 1a **tidak dimiliki satu pun
task** pada gelombang `MVP-5c`. Tanpa turunan itu, setiap pesanan yang dikonfirmasi akan berhenti
selamanya di `Confirmed`: `StartProcessAsync` hanya menerima `Accepted`, sehingga **mengonfirmasi
pesanan justru membuatnya tidak dapat dikerjakan**. Rinciannya pada bagian 3.4, dan bukti
jebakannya pada bagian 5.3.

---

## 2. Proses bisnis

### 2.1 Alur normal, berurutan

1. Dokter memesan pemeriksaan. Pesanan lahir berstatus **`Requested`**.
2. Petugas laboratorium membuka pesanan yang masuk dan **memilih dokter pemeriksa**.
3. Petugas menekan Konfirmasi. Sistem:
   - memastikan pesanan belum pernah dikonfirmasi;
   - memastikan statusnya memang masih `Requested`;
   - memastikan dokter pemeriksa dipilih, ada, dan masih aktif;
   - menyimpan **siapa** (dari pengguna yang sedang login), **kapan** (dari jam server), dan
     **dokter pemeriksa mana**;
   - memindahkan status ke **`Confirmed`**;
   - menuliskan satu baris riwayat `Order.Confirm` berisi status asal, status tujuan, dan aktornya.
4. Wadah pertama pesanan dinyatakan layak. Pesanan berpindah otomatis ke **`Accepted`**.
5. Pemeriksaan dikerjakan — `InProcess` — lalu diselesaikan — `Completed`.

### 2.2 Jalur tidak normal, seluruhnya terbukti dari database

| Keadaan | Yang terjadi | Pesan | Kode | Aturan |
| --- | --- | --- | :---: | --- |
| Pesanan sudah pernah dikonfirmasi, lalu dikonfirmasi lagi | Ditolak | "Pesanan ini sudah dikonfirmasi." | `409` | `VAL-70` |
| Status pesanan bukan `Requested` — misalnya `Accepted` atau `InProcess` | Ditolak | "Pesanan ini sudah melewati tahap konfirmasi." | `409` | `VAL-71` |
| Dokter pemeriksa tidak dipilih | Ditolak | "Pilih dokter pemeriksa terlebih dahulu." | `422` | `VAL-72` |
| Dokter pemeriksa tidak ada, sudah dihapus, atau tidak aktif | Ditolak | "Dokter pemeriksa tidak ditemukan atau tidak aktif." | `422` | `VAL-73` |
| Pesanan tidak ditemukan | Ditolak | "Order laboratorium tidak ditemukan." | `404` | — |
| Dua petugas mengonfirmasi pesanan yang sama bersamaan | Yang kalah ditolak | "Data laboratorium sudah diubah oleh petugas lain…" | `409` | token konkurensi `Version` |

### 2.3 Kenapa urutan pemeriksaan `VAL-70` sebelum `VAL-71` bermakna

Sebuah pesanan yang sudah dikonfirmasi lalu melaju ke `Accepted` memenuhi **kedua** syarat
penolakan sekaligus. Bila `VAL-71` diperiksa lebih dulu, petugas membaca "sudah melewati tahap
konfirmasi" — padahal yang benar dan lebih menolong adalah "sudah dikonfirmasi". Karena itu
`VAL-70` diperiksa lebih dulu, dan jejaknya dibaca dari `ConfirmedAt`, bukan dari statusnya saja,
supaya pesanan yang sudah melaju tetap terbaca pernah dikonfirmasi.

### 2.4 Konfirmasi tidak diwajibkan, dan itu keputusan sadar

Pesanan yang tidak pernah dikonfirmasi **tetap** berpindah `Requested` → `Accepted` ketika wadah
pertamanya dinyatakan layak. Mewajibkan konfirmasi berarti menghentikan setiap pesanan yang sedang
berjalan sampai seseorang mengonfirmasinya satu per satu; keputusan itu dicatat terpisah sebagai
`LAB-OPEN-027` dan belum diambil. Butir ini dibuktikan, bukan diklaim — lihat `T-97c` pada
bagian 5.2.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `AGENTS.md`, `CLAUDE.md`, `docs/engineering/*` | Governance, QBE yang berlaku, prefix dan lifecycle |
| `rules/backend/API_RULES.md`, `TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md` | Konvensi kontrak API, siklus kerja, klasifikasi, kriteria penyelesaian, bentuk laporan |
| `roadmap/backend-roadmap.md` bagian 6d | Cakupan, DoD, baris uji, dan verifikasi task |
| `contracts/api-contract.md` bagian 7.1 | Verb, path, hak akses, bentuk permintaan dan respons |
| `contracts/validation-matrix.md` | `VAL-70`..`VAL-73` beserta pesan dan kodenya, kata demi kata |
| `contracts/state-transition-matrix.md` bagian 1a | Transisi yang sah, termasuk **`Confirmed` → `Accepted`** |
| `contracts/permission-audit-matrix.md` | `LabOrder : Update` sudah ada; nol resource permission baru |
| `testing/acceptance-test-matrix.md` | `T-94a`..`T-95b`, `T-97c` |
| `roadmap/frontend-roadmap.md` — `FE-LAB-15` | Apa yang layar butuhkan dari respons endpoint ini |
| `Areas/.../Controllers/LabOrderController.cs` | Pola endpoint, atribut akses, dan pemetaan error |
| `Areas/.../Services/LabOrderService.cs` | Pola `HoldAsync`, `CancelAsync`, dan seluruh helper transisinya |
| `Areas/.../Services/LabSpecimenService.cs` | Turunan otomatis menuju `Accepted`; aturan empat mata `VAL-09` |
| `Areas/.../Services/LabExaminationService.cs`, `LabWorklistService.cs`, `LabFilterMetadataFactory.cs` | Seluruh tempat yang mengenumerasi status pesanan — pemeriksaan dampak nilai `Confirmed` |
| `Areas/.../Controllers/LabExaminationController.cs` | Preseden pemetaan `409` dan `422` di dalam modul ini |
| `Areas/Corporate/.../Models/MstDoctor.cs` | Penanda `IsActive` dan `IsDelete` untuk `VAL-73` |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodOrderService.cs` | Preseden pemeriksaan keberadaan dokter |
| `Seeders/AccessMenuSeeder.cs` | Cara `AccessAction` dikunci, untuk menilai dampak berbagi kunci `Update` |
| `task/report/backend/BE-LAB-27.md`, `BE-LAB-30.md` | Preseden harness verifikasi dan pembuktian berbasis database |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | `ConfirmLabOrderRequest` — **satu ruas saja**, `ExaminerDoctorId` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | `ConfirmAsync`; `LabOrderConflictException`; satu `using` untuk `MstDoctor` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Endpoint `POST /{id}/confirm`; dua cabang baru pada `ExecuteAsync` — `409` dan `422` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` | Turunan otomatis menuju `Accepted` kini juga berlaku dari `Confirmed` — **lihat 3.4** |

Nol migration. Nol perubahan entity. Nol resource permission baru.

### 3.3 Kenapa `ExaminerDoctorId` bertipe `Guid`, bukan `Guid?`

Terlihat sepele, tetapi menentukan kode HTTP yang keluar.

Ruas wajib bertipe nullable akan ditolak model binding ASP.NET sebagai **`400`**. Matriks validasi
menetapkan **`422`** untuk keadaan "dokter pemeriksa belum dipilih" (`VAL-72`). Dengan tipe
non-nullable, ruas yang tidak dikirim menghasilkan `Guid.Empty`, permintaannya masuk ke service,
dan penolakannya keluar sebagai `422` — persis seperti yang ditulis kontrak. Perbedaan ini nyata
bagi layar: `400` dibaca sebagai "permintaannya rusak", `422` dibaca sebagai "isiannya belum
lengkap", dan hanya yang kedua yang boleh memunculkan pesan di samping kotak pilihan dokter.

### 3.4 Satu berkas di luar daftar cakupan roadmap, dan alasannya bukan kerapian

Roadmap menulis cakupan task ini "satu DTO permintaan, satu method service, satu endpoint".
`LabSpecimenService.cs` ikut berubah — **satu kondisi, tiga baris**.

**Yang berubah.** Turunan otomatis "pesanan mengikuti wadah pertama yang dinyatakan layak"
sebelumnya hanya berlaku dari `Draft` dan `Requested`. Kini `Confirmed` ikut.

**Kenapa ini keharusan, bukan pilihan.** `LAB-STATE-v1` `r3` bagian 1a menuliskannya sebagai baris
transisi yang sah:

| Dari status | Tindakan | Ke status | Siapa yang boleh |
|---|---|---|---|
| `Confirmed` | Wadah pertama dinyatakan layak | `Accepted` | Turunan otomatis sistem |

Baris itu `approved` 2026-09-15, tetapi **tidak satu pun** dari `BE-LAB-30`, `BE-LAB-31`, maupun
`BE-LAB-32` menyebutnya pada cakupannya. Ia jatuh di antara ketiganya.

**Akibatnya bila dibiarkan, dan ini terukur.** `StartProcessAsync` hanya menerima pesanan
berstatus `Accepted`. Pesanan yang dikonfirmasi tidak akan pernah mencapai `Accepted`, sehingga
tidak akan pernah dapat diproses. Tombol Konfirmasi yang dibangun `FE-LAB-15` akan menjadi tombol
yang **merusak pesanan yang ditekannya** — dan kerusakannya tidak muncul sebagai galat, melainkan
sebagai pesanan yang diam. Jebakan ini dibuktikan di bagian 5.3, bukan diperkirakan.

**Kenapa penambahannya aman.** Kondisinya hanya **bertambah**, tidak berkurang. `Draft` dan
`Requested` tetap di sana, dan nol pesanan pada database berstatus `Confirmed` sebelum endpoint
ini ada — sehingga tidak ada satu pun perilaku lama yang berubah. Keduanya dibuktikan pada
bagian 5.2.

### 3.5 Pemeriksaan dampak nilai status baru terhadap seluruh modul

Menambahkan satu nilai status berarti setiap tempat yang mengenumerasi status perlu ditinjau.
Seluruhnya diperiksa, dan hasilnya ditulis apa adanya:

| Tempat | Penyaringnya | Perlu `Confirmed`? | Tindakan |
| --- | --- | :---: | --- |
| `LabSpecimenService` — turunan menuju `Accepted` | `Draft`, `Requested` | **Ya** | **Ditambahkan** — 3.4 |
| `LabSpecimenService` — penjaga rencana wadah | menolak `Cancelled`, `Completed`, `OnHold` | Tidak | `Confirmed` memang harus boleh direncanakan wadahnya |
| `LabSpecimenService` — penjaga pembatalan wadah | menolak `Cancelled`, `OnHold` | Tidak | Sama |
| `LabExaminationService` — dua penjaga | menolak `Cancelled`, `Completed` | Tidak | Pesanan terkonfirmasi memang masih menerima pemeriksaan |
| `LabOrderService.StartProcessAsync` | menerima hanya `Accepted` | Tidak | Benar sesuai kontrak: `Confirmed` wajib lewat `Accepted` |
| `LabOrderService.HoldAsync` | menolak `OnHold`, `Cancelled`, `Completed` | Tidak | `Confirmed` memang boleh ditahan |
| `LabOrderService.CancelAsync` | menolak `Cancelled`, `Completed` | Tidak | Penyempitannya adalah `VAL-75`, milik **`BE-LAB-32`** |
| `LabWorklistService` | mengecualikan `Completed`, `Cancelled` | Tidak | Pesanan terkonfirmasi memang harus muncul di daftar kerja |
| `LabFilterMetadataFactory` | menelusuri seluruh nilai enum | Sudah | Label `Dikonfirmasi` ditambahkan `BE-LAB-30` |
| `LabOrderService.GetSummaryAsync` | delapan ember status **tetap** | **Ya, tetapi** | Menambah ember adalah perubahan `LAB-API-v1` — lihat 7.1 |

### 3.6 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu endpoint baru**, persis seperti `LAB-API-v1` `r12` §7.1: `POST /lab-orders/{id}/confirm`, hak akses `LabOrder : Update`, permintaan `ConfirmLabOrderRequest`, respons `ApiResponse<LabOrderDetailResponse>` status `200`. **Nol endpoint yang sudah ada berubah perilakunya**, dan nol ruas respons bertambah — beserta akibatnya, yang dilaporkan pada 7.2 |
| Database | **Nol perubahan schema dan nol migration.** Yang berubah hanya isi kolom: tiga kolom yang didirikan `BE-LAB-30` kini dapat terisi, dan satu baris `LabTransitionHistory` terbentuk per konfirmasi. Uji integrasi menulis lalu menghapus 7 pesanan uji beserta turunannya; keadaan akhir database terbukti kembali persis — lihat 5.4 |
| Keamanan/Auth | **Nol resource permission baru.** Endpoint memakai `LabOrder : Update` yang sudah dipakai pembatalan, sesuai kontrak. **Konfirmator diturunkan server** dari klaim `NameIdentifier` pengguna yang login dan tidak dapat dikirim pemanggil — ditegakkan oleh ketiadaan ruasnya pada DTO, dan dibuktikan `T-95b`. Satu catatan metadata akses dilaporkan pada 7.3 |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/laboratory-management/lab-orders/{id}/confirm` | Mengonfirmasi pesanan beserta dokter pemeriksanya. Nama konfirmator dan waktunya diisi server, bukan dikirim pemanggil | `LabOrder : Update` |

**Badan permintaan — `ConfirmLabOrderRequest`**

| Ruas | Tipe | Wajib | Catatan |
| --- | --- | :---: | --- |
| `examinerDoctorId` | `guid` | Ya | Dokter pemeriksa. Wajib terisi (`VAL-72`) serta wajib ada dan masih aktif (`VAL-73`) |

**Yang sengaja tidak ada pada badan permintaan:** ruas konfirmator dan ruas waktu konfirmasi.
Keduanya diturunkan server. Ruas yang dapat dikirim pemanggil adalah ruas yang dapat dipalsukan
pemanggil, dan nama konfirmator adalah pertanyaan audit.

**Respons**

| Kode | Isi | Kapan |
| :---: | --- | --- |
| `200` | `ApiResponse<LabOrderDetailResponse>` | Konfirmasi berhasil. Status pesanan menjadi `Confirmed` |
| `404` | `ApiResponse<object>` | Pesanan tidak ditemukan |
| `409` | `ApiResponse<object>` | `VAL-70` sudah dikonfirmasi; `VAL-71` sudah melewati tahap konfirmasi; atau kalah konkurensi |
| `422` | `ApiResponse<object>` | `VAL-72` dokter belum dipilih; `VAL-73` dokter tidak ada atau tidak aktif |
| `400` | `ApiResponse<object>` | Badan permintaan cacat bentuk |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 Error, 207 Warning** | `PASS` | Jumlah warning sama persis dengan baseline; nol warning dari keempat berkas yang diubah |
| Verifikasi kontrak terhadap `r12` | Verb, path, hak akses, bentuk permintaan, dan bentuk respons cocok kata demi kata | `PASS` | Bagian 4 |
| **`T-94a`** — konfirmasi pertama berhasil | `Requested` → `Confirmed`; konfirmator, waktu, dan dokter terekam | `PASS` | 5.1 |
| **`T-94a`** — jejak audit terbentuk | Satu baris `Order.Confirm` berisi `Requested→Confirmed` beserta aktornya | `PASS` | 5.1 |
| **`T-94a`** — waktu diturunkan server | `ConfirmedAt` berada di dalam rentang jam server saat pemanggilan | `PASS` | 5.1 |
| **`T-94a`** — konfirmator diturunkan server | `ConfirmedByUserId` sama dengan klaim `NameIdentifier` | `PASS` | 5.1 |
| **`T-94b` / `VAL-70`** — konfirmasi kedua ditolak | `LabOrderConflictException` "Pesanan ini sudah dikonfirmasi." → `409` | `PASS` | 5.1 |
| **`T-94c` / `VAL-71`** — pesanan `Accepted` ditolak | "Pesanan ini sudah melewati tahap konfirmasi." → `409` | `PASS` | 5.1 |
| **`T-94c` / `VAL-71`** — pesanan `InProcess` ditolak | Pesan dan kode yang sama | `PASS` | 5.1 |
| **`T-95a` / `VAL-72`** — dokter belum dipilih | `LabOrderValidationException` "Pilih dokter pemeriksa terlebih dahulu." → `422` | `PASS` | 5.1 |
| **`T-95a` / `VAL-73`** — dokter tidak ditemukan | "Dokter pemeriksa tidak ditemukan atau tidak aktif." → `422` | `PASS` | 5.1 |
| **`T-95a` / `VAL-73`** — dokter **tidak aktif** | Pesan dan kode yang sama, dengan dokter tidak aktif yang benar-benar ada di database | `PASS` | 5.1 |
| **Langkah kontrol** — sesudah tiga penolakan, konfirmasi yang sah tetap berhasil | Berhasil | `PASS` | 5.1 |
| **`T-95b`** — konfirmator dan waktu tidak dapat dikirim pemanggil | Properti publik `ConfirmLabOrderRequest` = `[ExaminerDoctorId]` | `PASS` | 5.1 |
| **`T-97c`** — jalur lama `Requested` → `Accepted` tanpa konfirmasi | Pesanan mencapai `Accepted` dengan ketiga kolom konfirmasi tetap `null` | `PASS` | 5.2 |
| **`LAB-STATE-v1` `r3` 1a** — `Confirmed` → `Accepted` | Pesanan terkonfirmasi mencapai `Accepted`; jejak konfirmasinya tetap utuh | `PASS` | 5.2 |
| **Jebakan tanpa turunan itu** — pesanan `Confirmed` ditolak `StartProcessAsync` | "Pesanan berstatus Confirmed tidak dapat dipindahkan ke InProcess." | `PASS` | 5.3 |
| Kebersihan database sesudah uji | Nol baris uji tersisa; 5 pesanan dengan sebaran status identik | `PASS` | 5.4 |
| Uji lewat HTTP sungguhan beserta `[Authorize]` dan `[AccessPermission]` | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan. Service dipanggil sungguhan; yang belum terlewati adalah lapisan atribut akses, yang memakai ulang `LabOrder : Update` tanpa perubahan |
| Uji manual lewat antarmuka | Tidak dijalankan | `NOT FEASIBLE` | Layarnya adalah `FE-LAB-15`, belum dibangun |

### 5.1 Empat belas pemeriksaan dijalankan sungguhan terhadap database

Harness sekali pakai di scratchpad sesi, **di luar repository**, memakai `ApplicationDbContext`
dan `LabOrderService` yang **sebenarnya**. Pesanan uji dibuat sendiri — **nol dari 5 pesanan nyata
disentuh**.

Hasil: **14 `PASS`, 0 `FAIL`.**

Yang pantas dibaca dari hasilnya:

| Butir | Nilai yang terbaca |
| --- | --- |
| Status sesudah konfirmasi | `Confirmed` — nilai enum `9` |
| Konfirmator | sama persis dengan klaim `NameIdentifier` harness |
| Waktu konfirmasi | berada di dalam rentang jam server saat pemanggilan, sedangkan permintaan **tidak memuat ruas waktu sama sekali** |
| Riwayat | tepat **satu** baris `Order.Confirm`, `Requested→Confirmed`, beserta aktornya |
| Bentuk DTO permintaan | tepat **satu** properti publik: `ExaminerDoctorId` |

**Langkah kontrol dipakai, dan itu pelajaran `BE-LAB-26`.** Sesudah tiga penolakan berturut-turut,
pesanan yang sama dikonfirmasi lagi dengan dokter yang sah — dan berhasil. Tanpa langkah ini,
"service menolak dengan benar" tidak dapat dibedakan dari "service rusak dan menolak segalanya".

**Cabang "tidak aktif" pada `VAL-73` benar-benar teruji**, bukan dilewati: database memuat dokter
ber-`IsActive = false`, dan dokter itulah yang dipakai sebagai bahan uji.

### 5.2 Kedua turunan otomatis dibuktikan, bukan ditelusuri pada source

Harness kedua menjalankan rantai wadah yang sesungguhnya — rencana, ambil, terima, nyatakan layak —
terhadap dua pesanan: satu **tanpa** konfirmasi, satu **dengan**.

| Pesanan | Status awal | Status akhir | Riwayat yang terbaca |
| --- | --- | --- | --- |
| Tanpa konfirmasi | `Requested` | **`Accepted`** | `Order.Accept: Requested→Accepted` |
| Dengan konfirmasi | `Confirmed` | **`Accepted`** | `Order.Confirm: Requested→Confirmed` … `Order.Accept: Confirmed→Accepted` |

Hasil: **6 `PASS`, 0 `FAIL`.** Ketiga kolom konfirmasi pesanan pertama tetap `null` sesudahnya, dan
jejak konfirmasi pesanan kedua tetap utuh setelah ia melaju ke `Accepted`.

**Nol fakta kelayakan tagih tertulis ke database, dan itu disengaja.** Penerbitan fakta ke Billing
berjalan **sesudah** `SaveChangesAsync`, sehingga perpindahan statusnya sudah tersimpan ketika
penerbitan itu dihentikan. `ClinicalMilestoneFactProducer` sengaja dilewatkan sebagai `null`, dan
rantainya berhenti dengan `NullReferenceException` yang keras — bukan lolos diam-diam. Dengan cara
itu turunan status dapat dibuktikan tanpa menulis satu pun tagihan atas pesanan yang tidak nyata.

**Satu kegagalan harness yang justru membuktikan aturan bisnis bekerja.** Percobaan pertama gagal
dengan `LabSpecimenForbiddenException` — "Petugas yang mengambil sampel tidak boleh menyatakan
kelayakannya." Itu `VAL-09`, aturan empat mata, dan yang salah adalah harnessnya: ia memakai satu
aktor untuk seluruh langkah. Harness diperbaiki memakai **dua aktor**, dan rantainya berjalan.

### 5.3 Jebakannya dibuktikan, bukan diperkirakan

Sebelum turunan `Confirmed` → `Accepted` ditambahkan, pesanan berstatus `Confirmed` dipanggil
`StartProcessAsync`:

```text
InvalidOperationException: "Pesanan berstatus Confirmed tidak dapat dipindahkan ke InProcess."
```

Inilah bentuk kerusakannya bila baris pada 3.4 tidak ada: pesanan yang dikonfirmasi tidak dapat
dikerjakan, dan tidak ada satu pun galat yang muncul pada saat konfirmasi untuk memperingatkannya.

### 5.4 Kebersihan database

| Butir | Sebelum | Sesudah |
| --- | :---: | :---: |
| Total `LabOrder` | 5 | **5** |
| Sebaran status | `Requested` 2, `Accepted` 2, `InProcess` 1 | **identik** |
| Pesanan berstatus `Confirmed` | 0 | **0** |
| Kolom `ConfirmedAt` / `ExaminerDoctorId` terisi | 0 | **0** |
| Total `LabSpecimen` | 5 | **5** |
| Baris `LabTransitionHistory` milik aktor uji | 0 | **0** |

Yang dibuat lalu dihapus seluruhnya: 7 pesanan uji, 2 wadah, 2 pemeriksaan, dan 15 baris riwayat.
Pemeriksaan penutup dijalankan terpisah dari harness — memakai query langsung terhadap database —
supaya kebersihannya tidak dinilai oleh program yang sama yang membuat kotorannya.

### 5.5 Alat verifikasinya

Dua program sekali pakai di scratchpad sesi, **di luar repository**, yang merujuk project aplikasi
sehingga memakai `ApplicationDbContext`, `LabOrderService`, dan `LabSpecimenService` yang
sebenarnya. Keduanya **menolak berjalan bila nama database tujuannya bukan `QuilvianNewDevYoga`**
dan tidak menulis credential ke berkas mana pun.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Endpoint menjawab sesuai `r12` | **Terpenuhi** | Bagian 4 — verb, path, hak akses, bentuk permintaan dan respons cocok kata demi kata |
| Konfirmator dan waktu **diturunkan server**, tidak dari badan permintaan | **Terpenuhi** | `T-94a` membuktikan nilainya; `T-95b` membuktikan ruasnya memang tidak ada pada DTO |
| Konfirmasi kedua ditolak | **Terpenuhi** | `T-94b` — `409`, "Pesanan ini sudah dikonfirmasi." |
| **`T-97c` membuktikan jalur lama tidak tertutup** | **Terpenuhi** | 5.2 — pesanan tanpa konfirmasi mencapai `Accepted` dengan ketiga kolom konfirmasi tetap `null` |
| `VAL-70`..`VAL-73` menolak sesuai matriks | **Terpenuhi, keempatnya** | 5.1 — pesan dan kode dibandingkan kata demi kata terhadap matriks |
| Build | **Terpenuhi** | 0 Error, 207 Warning — sama persis dengan baseline |

Seluruh butir DoD terpenuhi.

### 6.2 Acceptance criteria

| Kriteria | Status | Bukti dan yang belum |
| --- | --- | --- |
| `AC-94` — konfirmasi hanya sah **sekali**; sesudahnya nama konfirmator dan tanggal/waktu **terekam** dan **terbaca pada daftar** | **Terpenuhi sebagian** | "Hanya sekali" **terbukti** (`T-94b`). "Terekam" **terbukti** (`T-94a`). **"Terbaca pada daftar" belum** — `LabOrderListResponse` dan `LabOrderDetailResponse` tidak memuat ruas konfirmator maupun waktu konfirmasi, dan `LAB-API-v1` `r12` **tidak mendefinisikannya**. Lihat 7.2 |
| `AC-95` — konfirmasi **menolak** bila dokter pemeriksa belum dipilih, dan dokter yang dipilih **tampil pada daftar serta ringkasan cetak** | **Terpenuhi sebagian** | "Menolak" **terbukti** (`T-95a`, `VAL-72` dan `VAL-73`). **"Tampil pada daftar dan cetak" belum** — sebab yang sama: ruas responsnya belum ada pada kontrak. Lihat 7.2 |

Kedua butir yang belum terpenuhi **tidak** disebabkan pekerjaan yang kurang pada task ini,
melainkan oleh celah pada kontrak yang disetujui. Keduanya ditulis apa adanya, bukan didiamkan.

---

## 7. Catatan penutup

### 7.1 Rekap pesanan belum mengenal `Confirmed`

Diwariskan dari `BE-LAB-30` dan **kini menjadi nyata**, karena pesanan sudah dapat berstatus
`Confirmed`.

`LabOrderService.GetSummaryAsync` mencacah pesanan ke dalam delapan ember status yang **tetap** —
`Draft`, `Diminta`, `Diterima`, `SedangDikerjakan`, `Selesai`, `Ditahan`, `PembatalanDiminta`,
`Dibatalkan` — sementara `TotalPesanan` mencacah seluruhnya. Sejak hari ini, jumlah kedelapan ember
itu **tidak lagi sama dengan `TotalPesanan`**, dan selisihnya persis pesanan yang sedang menunggu
dikerjakan.

Contoh berangka: 10 pesanan, 3 di antaranya `Confirmed` → `TotalPesanan` = 10, jumlah ember = 7.
Tiga pesanan hilang dari rekap tanpa ada galat apa pun.

Sengaja tidak diperbaiki di sini: menambah ember berarti menambah ruas pada
`LabOrderSummaryResponse`, dan itu perubahan `LAB-API-v1` yang tidak diberi wewenang pada task ini.

### 7.2 Celah kontrak — layar `FE-LAB-15` belum punya ruas yang dibacanya

**Ini temuan terpenting laporan ini, dan ia menahan pekerjaan orang lain.**

`FE-LAB-15` diwajibkan roadmap menampilkan, pada kolom Konfirmasi ketiga datatable, "**nama
konfirmator beserta tanggal dan waktu**", dan `AC-95` menuntut dokter pemeriksa "tampil pada daftar
serta ringkasan cetak".

Sementara itu `LAB-API-v1` `r12` §7.1 hanya mendefinisikan **badan permintaan**. Ia tidak menambah
satu pun ruas pada `LabOrderListResponse` maupun `LabOrderDetailResponse`. Akibatnya ketiga nilai
itu — `confirmedByName`, `confirmedAt`, `examinerDoctorName` — **tidak dikembalikan endpoint mana
pun**, dan layar tidak punya cara menampilkannya.

**Kenapa tidak ditambahkan saja di sini.** Menambah ruas respons adalah perubahan kontrak.
Preseden modul ini tegas: `BE-LAB-01` menambahkan `discipline` pada `LabOrderDetailResponse`
**sesudah** `LAB-API-v1` `r3` mengamandemennya, bukan sebelum. Mengubahnya sepihak di sini akan
mengulangi persis kesalahan yang aturan itu cegah.

**Usul yang perlu diputuskan pemilik modul**, ditulis lengkap supaya tinggal disetujui atau ditolak:

| Ruas usulan | Tipe | Pada | Isi |
| --- | --- | --- | --- |
| `confirmedAt` | `datetime?` | `LabOrderListResponse` | Waktu konfirmasi; kosong bila belum dikonfirmasi |
| `confirmedByUserId` | `guid?` | `LabOrderDetailResponse` | Penunjuk konfirmator |
| `confirmedByName` | `string?` | `LabOrderListResponse` | Nama konfirmator siap tampil — layar tidak boleh menampilkan penunjuk (`no-uuid-display`) |
| `examinerDoctorId` | `guid?` | `LabOrderDetailResponse` | Penunjuk dokter pemeriksa |
| `examinerDoctorName` | `string?` | `LabOrderListResponse` | Nama dokter pemeriksa siap tampil |

Kelimanya **penambahan**, sehingga aman bagi pembaca lama — penilaian yang sama sudah dipakai
`LAB-API-v1` bagian 324 untuk tiga ruas kesegeraan. Pola `RequestedByName` yang sudah ada dapat
dipakai ulang apa adanya, termasuk `ResolveUserNameAsync` yang menyediakannya.

**Sampai ruas itu disetujui, `FE-LAB-15` tidak dapat memenuhi DoD-nya**, dan `AC-94` serta `AC-95`
tetap terpenuhi sebagian.

### 7.3 Satu catatan metadata akses

Endpoint ini memakai `[AccessAction("Update", …)]`, kunci yang sama dengan pembatalan pesanan —
karena kontrak menetapkan hak aksesnya `LabOrder : Update` dan nol resource permission baru.

`AccessMenuSeeder` mengunci sebuah action pada pasangan (controller, `ActionName`), sehingga dua
endpoint yang berbagi kunci akan saling menimpa `DisplayName`, `RoutePath`, dan `HttpMethod` milik
baris seeder itu. Ini **bukan hal baru**: `Process` sudah dipakai dua endpoint, begitu pula `Hold`.
Konvensi yang dipakai keduanya adalah `DisplayName` yang sama dan `Description` yang berbeda, dan
konvensi itulah yang diikuti di sini — `DisplayName` dibiarkan `"Cancel Lab Order"` supaya label
pada layar Akses Role **tidak berubah** oleh task ini.

Akibatnya label itu kini kurang tepat: ia menaungi pembatalan **dan** konfirmasi. Memperbaikinya
berarti mengganti `DisplayName` bersama menjadi sesuatu yang netral — perubahan kecil yang
menyentuh baris milik pembatalan, sehingga pantas berdiri sebagai keputusan tersendiri. Pemeriksaan
hak aksesnya sendiri tidak terpengaruh sama sekali: ia dikunci `[AccessPermission("LabOrder",
"Update")]`, bukan oleh label.

### 7.4 Ringkasan

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 207 warning, **seluruhnya sudah ada sebelum task ini**; nol berasal dari keempat berkas yang diubah |
| Masalah yang diketahui | **Tiga**, seluruhnya di atas: rekap pesanan belum mengenal `Confirmed` (7.1); ruas respons untuk `FE-LAB-15` belum ada pada kontrak (7.2); label metadata akses bersama (7.3) |
| Risiko tersisa | **Pertama**, `FE-LAB-15` tertahan sampai `LAB-API-v1` `r13` menyetujui kelima ruas respons pada 7.2 — dan bila layar itu dibangun tanpa ruasnya, kolom Konfirmasi akan kosong walaupun backendnya benar. **Kedua**, rekap pesanan akan kehilangan pesanan berstatus `Confirmed` sejak konfirmasi pertama dijalankan di lingkungan mana pun. **Ketiga**, endpoint ini belum pernah dilewati lapisan `[Authorize]` dan `[AccessPermission]` secara sungguhan; risikonya rendah karena hak aksesnya memakai ulang yang sudah ada tanpa perubahan |
| Perubahan sampingan | `NONE`. Perubahan yang sudah ada di working tree sebelum task ini — `BE-LAB-26`..`BE-LAB-30`, `BE-EXT-04`, `BE-EXT-04b`, beserta artefak blueprintnya — tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Lihat 7.5 |
| Langkah berikutnya | **1.** Ajukan `LAB-API-v1` `r13` berisi kelima ruas respons pada 7.2; tanpa itu `FE-LAB-15` tidak dapat dimulai. **2.** `BE-LAB-32` — pembatalan wajib beralasan; penahannya sudah terangkat, dan angka dampaknya sudah dihitung: `Accepted` 2, `InProcess` 1, `OnHold` 0, sehingga **3 pesanan** akan kehilangan kemampuan dibatalkan begitu `VAL-75` ditegakkan. **3.** Putuskan bagaimana `Confirmed` dicacah pada rekap pesanan. **4.** Putuskan apakah konfirmasi kelak menjadi **wajib** sebelum `Accepted` (`LAB-OPEN-027`) — kini pertanyaannya tidak lagi teoretis, karena jalurnya sudah ada |

### 7.5 Status Git di akhir pekerjaan

Perubahan milik task ini:

```text
 M Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs
 M Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-31.md
```

Ditambah pembaruan artefak blueprint: `roadmap/backend-roadmap.md` dan `roadmap/traceability.md`.

Keempat berkas source **sudah** berstatus `M` sebelum task ini dimulai, karena pekerjaan
`BE-LAB-26`..`BE-LAB-30` dan `BE-EXT-04` belum di-commit. Bagian milik task ini terpisah jelas dan
tidak menimpa apa pun.

Nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, dan `deploy` dijalankan.
