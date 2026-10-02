# Laporan Perubahan Backend — `BE-LAB-32`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-32` |
| Judul | Pembatalan pesanan wajib beralasan |
| Slice | `EPIC-LAB-12`, gelombang `MVP-5c` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6d |
| Trace | `FR-11.13`; `LAB-DEC-063`; `AC-96`, `AC-97`; `T-96a`, `T-97a`, `T-97b` |
| Contract version | `LAB-API-v1` **`r12`** §7.2; `LAB-VAL-v1` **`r6`** `VAL-74`, `VAL-75`; `LAB-STATE-v1` **`r3`** bagian 1a — seluruhnya `approved` 2026-09-15 |
| Dependency | `BE-LAB-30` ✅ `SELESAI` 2026-09-16 |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 0, logika bisnis 1, **kontrak API 2**, database 1, keamanan 0, UI/workflow 1. **Risikonya tetap `Tinggi`** seperti ditulis roadmap; klasifikasi mengukur besarnya pekerjaan, risiko mengukur akibat bila salah |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium dan artefak blueprint. **Nol migration, nol kolom baru** |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-16 |
| Status | **✅ `SELESAI`** — kedua aturan menolak sesuai matriks, `T-97b` membuktikan jalur yang sah tidak ikut tertutup, dan **hitungan dampak dilaporkan sebelum aturan ditegakkan** sebagaimana diwajibkan DoD |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 |
| Keberlakuan | `NEW CODE` untuk satu DTO baru; `TOUCHED LEGACY` untuk dua aturan pada method dan endpoint yang sudah ada |
| Status gerbang | `QBE-MOD-002` **tidak menahan** — lifecycle `ACTIVE`, nol entity dan nol resource permission baru |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001` (`VAL-74`, `VAL-75`), `QBE-DTO-001`, `QBE-DEL-001` (lifecycle cancel beserta audit aktornya), `QBE-LOG-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`..`003`, `QBE-CFG-001`, `QBE-NAM-001`..`004`, `QBE-DB-001`..`002` — nol entity, configuration, dan migration; `QBE-CODE-001`..`006` — nol nomor bisnis; `QBE-PERM-001` — hak akses `LabOrder : Update` dipakai ulang tanpa perubahan |

Gate lolos bersih. Dependency `BE-LAB-30` selesai; ketiga kontrak `approved`; `AC-96`, `AC-97`,
dan ketiga baris uji sudah tertulis sebelum implementasi; `HEAD` `e2152709` cocok dengan manifest.

---

## 1. Masalah yang diperbaiki

### 1.1 Pembatalan tanpa alasan

**Keadaan sebelum perubahan.** Endpoint pembatalan pesanan menerima badan permintaan yang
**opsional**. Pesanan dapat dibatalkan tanpa satu kata pun keterangan, dan jejak auditnya menyimpan
`ReasonNote` kosong.

**Akibat nyatanya.** Ketika kelak seseorang bertanya "kenapa pemeriksaan pasien ini dibatalkan?",
sistem tidak punya jawaban. Yang bertanya biasanya bukan petugas yang membatalkannya — melainkan
dokter pemesan, petugas penagihan, atau pasien itu sendiri, berhari-hari kemudian.

### 1.2 Pembatalan atas pekerjaan yang sudah dimulai

**Keadaan sebelum perubahan.** Pembatalan sah dari status apa pun kecuali `Cancelled` dan
`Completed`. Artinya pesanan yang **wadahnya sudah dinyatakan layak** — bahan pasien sudah diambil,
barcode sudah tercetak, pekerjaan sudah berjalan — masih dapat dibatalkan seolah tidak pernah
terjadi apa-apa.

**Kenapa itu salah.** Membatalkan pesanan yang sudah dikerjakan bukan lagi pembatalan, melainkan
**koreksi**. Keduanya berbeda: pembatalan menghapus rencana, koreksi memperbaiki catatan atas
sesuatu yang benar-benar terjadi pada tubuh pasien. Aturan koreksi belum diputuskan
(`LAB-P0-003`), dan selama belum ada, jalur yang tersedia hanyalah jalur yang salah.

---

## 2. Hitungan dampak — dikerjakan **sebelum** aturan ditegakkan

Roadmap mewajibkan butir ini dikerjakan lebih dulu, bukan sesudah, karena `BE-LAB-21` sudah
menunjukkan bahwa pengetatan yang tidak dihitung dampaknya lebih mahal daripada pengetatan yang
ditunda. Seluruh angka di bawah dibaca dari `QuilvianNewDevYoga` pada 2026-09-16, **sebelum** satu
baris pun diubah.

### 2.1 Pesanan yang kehilangan jalur pembatalannya

| Status | Jumlah | Dampak |
| --- | :---: | --- |
| `Requested` | **2** | Tetap dapat dibatalkan |
| `Accepted` | **2** | **Kehilangan jalur pembatalan** |
| `InProcess` | **1** | **Kehilangan jalur pembatalan** |
| `OnHold` | **0** | — |
| `Completed` | 0 | Sudah tidak dapat dibatalkan sebelum aturan ini |
| `Cancelled` | 0 | Sudah tidak dapat dibatalkan sebelum aturan ini |
| **Total kehilangan jalur** | **3** | |

**Tiga pesanan kehilangan kemampuan dibatalkan.** Angka itu tidak diperkecil dan tidak dibulatkan:
bila salah satunya perlu dibatalkan besok, tidak ada jalur yang tersisa sampai aturan koreksi
diputuskan.

### 2.2 Apakah ada pemanggil yang masih membatalkannya

Ini pertanyaan kedua yang diwajibkan roadmap, dan jawabannya menurunkan risikonya secara nyata.

| Pemeriksaan | Hasil |
| --- | --- |
| Pembatalan pesanan yang **pernah terjadi** pada database ini | **0** — nol baris `Order.Cancel` pada seluruh jejak audit |
| Pemanggil backend `LabOrderService.CancelAsync` | **1** — hanya `LabOrderController.Cancel`; nol pemanggilan internal |
| Layar frontend yang memanggil `PUT /lab-orders/{id}/cancel` | **0** — 12 berkas modul Laboratorium dirujuk; seluruh kata "cancel" di dalamnya adalah tombol tutup dialog, `handleCancel` pada form, atau `ERR_CANCELED` milik Axios |

**Artinya:** aturan ini mengetatkan jalur yang **belum pernah dipakai satu kali pun**, dan belum
punya satu pun layar yang memanggilnya. Risiko yang ditulis roadmap sebagai `Tinggi` adalah
kehati-hatian yang benar; pengukurannya menunjukkan ledakannya hari ini **nol**, dengan tiga
pesanan sebagai biaya di masa depan.

**Yang tetap harus diketahui pemilik:** begitu `FE-LAB-16` dibangun, layar itu **tidak boleh**
menampilkan aksi Batalkan pada pesanan `Diproses`, `Selesai`, atau `Dibatalkan` — sudah tertulis
pada kewenangan UI-nya. Bila tetap ditampilkan, petugas akan menekan tombol yang selalu gagal.

---

## 3. Proses bisnis

### 3.1 Alur normal, berurutan

1. Petugas membuka pesanan berstatus `Requested` atau `Confirmed`.
2. Petugas memilih Batalkan. Layar meminta **alasan pembatalan** — wajib.
3. Sistem memeriksa berurutan:
   - apakah pesanan masih berada pada status yang menerima pembatalan (`VAL-75`);
   - apakah alasannya benar-benar terisi, bukan hanya spasi (`VAL-74`).
4. Seluruh wadah yang masih berjalan ikut dibatalkan.
5. Pesanan menjadi `Cancelled`, ditandai `IsCancel`, dan satu baris riwayat `Order.Cancel`
   terbentuk berisi status asal, status tujuan, aktornya, dan **alasannya**.

### 3.2 Jalur tidak normal, seluruhnya terbukti dari database

| Keadaan | Yang terjadi | Pesan | Kode | Aturan |
| --- | --- | --- | :---: | --- |
| Status pesanan `Accepted`, `InProcess`, `OnHold`, `Completed`, atau `Cancelled` | Ditolak | "Pesanan yang sudah diproses tidak dapat dibatalkan." | `409` | `VAL-75` |
| Alasan kosong, hanya spasi, hanya tab, atau badan permintaan tidak dikirim | Ditolak | "Alasan pembatalan wajib diisi." | `422` | `VAL-74` |
| Pesanan tidak ditemukan | Ditolak | "Order laboratorium tidak ditemukan." | `404` | — |
| Dua petugas membatalkan pesanan yang sama bersamaan | Yang kalah ditolak | "Data laboratorium sudah diubah oleh petugas lain…" | `409` | token konkurensi |

### 3.3 Kenapa `VAL-75` diperiksa sebelum `VAL-74`

Keadaan pesanan lebih dulu, isi permintaan sesudahnya — urutan yang sama dipakai `ConfirmAsync`
pada `BE-LAB-31`. Alasannya sederhana: pesanan yang memang **tidak boleh** dibatalkan tidak perlu
diminta alasannya lebih dulu. Meminta petugas mengetik alasan, lalu menolaknya karena statusnya,
adalah pekerjaan yang dibuang percuma.

### 3.4 Alasannya disimpan di mana, dan kenapa nol kolom baru dibutuhkan

`LAB-DEC-063` semula menuntut kolom alasan pembatalan pada `LabOrder`. Pemeriksaan source saat
kontrak ditulis menemukan alasannya **sudah** tersimpan sebagai `ReasonNote` pada
`LabTransitionHistory` — jejak audit, tempat yang memang seharusnya — sehingga migration gelombang
ini turun dari empat kolom menjadi tiga, dan task ini tidak membutuhkan satu kolom pun.

Alasannya **dirapikan sebelum disimpan**: spasi di awal dan akhir dibuang. Contoh berangka:
`"   Pasien menolak pengambilan sampel   "` tersimpan sebagai
`"Pasien menolak pengambilan sampel"`. Dengan begitu alasan yang sama tidak terbaca sebagai dua
alasan berbeda hanya karena satu petugas menekan spasi.

---

## 4. Perubahan yang dikerjakan

### 4.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `AGENTS.md`, `CLAUDE.md`, `docs/engineering/*` | Governance, QBE yang berlaku |
| `rules/backend/API_RULES.md`, `TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md` | Konvensi kontrak API, siklus kerja, klasifikasi, kriteria penyelesaian, bentuk laporan |
| `roadmap/backend-roadmap.md` bagian 6d | Cakupan, DoD, baris uji, dan **butir yang wajib dikerjakan lebih dulu** |
| `contracts/api-contract.md` bagian 7.2 | Badan permintaan baru dan status yang sah |
| `contracts/validation-matrix.md` | `VAL-74` dan `VAL-75` beserta pesan dan kodenya, kata demi kata |
| `contracts/state-transition-matrix.md` bagian 1a | Batas baru pembatalan |
| `testing/acceptance-test-matrix.md` | `T-96a`, `T-97a`, `T-97b` |
| `roadmap/frontend-roadmap.md` — `FE-LAB-16` | Apa yang layar butuhkan, dan status mana yang tidak boleh menampilkan aksi Batalkan |
| `Areas/.../Services/LabOrderService.cs` | `CancelAsync` beserta seluruh jalur turunannya |
| `Areas/.../Controllers/LabOrderController.cs` | Pemetaan error endpoint pembatalan |
| `Areas/.../DTOs/LabSpecimenDtos.cs` | `CancelLabSpecimenRequest` yang selama ini dipakai |
| `Areas/.../Services/LabSpecimenService.cs` | `CancelAllForOrderInMemoryAsync` dan jalur penyerahan ke Billing |
| `Areas/.../Models/LabTransitionHistory.cs` | Tempat alasan tersimpan |
| `QuilvianSystemFrontendDev/src/**` — 12 berkas modul Laboratorium | **Read-only.** Mencari pemanggil endpoint pembatalan; nol ditemukan |
| `task/report/backend/BE-LAB-31.md` | Preseden harness verifikasi dan pembagian `409`/`422` |

### 4.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | `CancelLabOrderRequest` — satu ruas `CancelReason` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | `CancelAsync` menerima DTO baru; dua penjaga lama digantikan `VAL-75`; `VAL-74` ditambahkan |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Badan permintaan menjadi `CancelLabOrderRequest`; dua cabang error baru — `409` dan `422` |

Nol migration. Nol kolom baru. Nol resource permission baru. `CancelLabSpecimenRequest`
**tidak dihapus** — ia tetap dipakai pembatalan pada tingkat wadah.

### 4.3 Satu penjaga digantikan, dan itu diminta matriks uji

Sebelumnya ada dua penjaga terpisah:

```text
Cancelled atau IsCancel  -> InvalidOperationException "Order laboratorium sudah dibatalkan."      (400)
Completed                -> InvalidOperationException "…yang sudah selesai tidak dapat dibatalkan." (400)
```

Keduanya digantikan satu penjaga `VAL-75`. Alasannya bukan penyederhanaan: `T-97a` menagih
pembatalan atas pesanan **`Completed` dan `Cancelled`** ditolak `409` dengan aturan `VAL-75`,
bukan `400` dengan pesan lamanya. Satu penjaga baru menutup tujuh status sekaligus — `Accepted`,
`InProcess`, `OnHold`, `Completed`, `Cancelled`, `Draft`, dan `CancelRequested` — dan `IsCancel`
tetap ikut diperiksa supaya pesanan yang sudah ditandai batal tanpa sempat berpindah status tidak
dapat dibatalkan dua kali.

**Satu akibat dilaporkan apa adanya.** Pesanan yang sudah dibatalkan kini dijawab "Pesanan yang
sudah diproses tidak dapat dibatalkan." — kalimat yang kurang tepat untuk keadaan itu, karena
pesanan tersebut tidak "diproses" melainkan sudah batal. Pesannya diambil **kata demi kata** dari
`VAL-75` pada matriks yang sudah disetujui, dan mengarang kalimat yang lebih tepat berarti membuat
matriks dan source berbeda bunyi. Bila pemilik menghendaki pesan yang lebih spesifik untuk keadaan
"sudah dibatalkan", itu perubahan `LAB-VAL-v1` tersendiri.

### 4.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Perubahan breaking pada endpoint yang sudah ada**, persis seperti ditulis `r12` §7.2. `PUT /lab-orders/{id}/cancel`: badan permintaan berubah dari `CancelLabSpecimenRequest` **opsional** menjadi `CancelLabOrderRequest` dengan `cancelReason` **wajib**, dan status yang sah menyempit dari "selain `Cancelled` dan `Completed`" menjadi "hanya `Requested` dan `Confirmed`". Bentuk **respons tidak berubah** — tetap `ApiResponse<LabOrderCancellationResult>`. Hitungan dampaknya pada bagian 2 |
| Database | **Nol perubahan schema, nol migration, nol kolom baru.** Yang berubah hanya isi: `ReasonNote` pada `LabTransitionHistory` kini dijamin terisi untuk setiap pembatalan pesanan. Uji integrasi menulis lalu menghapus 13 pesanan uji; keadaan akhir database terbukti kembali persis — lihat 6.3 |
| Keamanan/Auth | `NOT APPLICABLE` untuk perubahan hak akses — `LabOrder : Update` dipakai ulang tanpa perubahan. Yang menguat justru pertanggungjawabannya: setiap pembatalan kini punya aktor **dan** alasan pada jejak audit, bukan aktor saja |

---

## 5. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/health-services/laboratory-management/lab-orders/{id}/cancel` | Membatalkan pesanan beserta wadah yang masih berjalan. **Alasan wajib**, dan hanya sah pada pesanan `Requested` atau `Confirmed` | `LabOrder : Update` |

**Badan permintaan — `CancelLabOrderRequest`**

| Ruas | Tipe | Wajib | Catatan |
| --- | --- | :---: | --- |
| `cancelReason` | `string` (maks. 1000) | Ya | Alasan pembatalan. Tidak boleh kosong maupun hanya spasi (`VAL-74`). Disimpan sudah dirapikan pada jejak audit |

**Respons**

| Kode | Isi | Kapan |
| :---: | --- | --- |
| `200` | `ApiResponse<LabOrderCancellationResult>` | Pembatalan berhasil |
| `404` | `ApiResponse<object>` | Pesanan tidak ditemukan |
| `409` | `ApiResponse<object>` | `VAL-75` status tidak menerima pembatalan; atau kalah konkurensi |
| `422` | `ApiResponse<object>` | `VAL-74` alasan pembatalan kosong |
| `400` | `ApiResponse<object>` | Badan permintaan cacat bentuk |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 Error, 207 Warning** | `PASS` | Sama persis dengan baseline; nol warning dari ketiga berkas yang diubah |
| **Hitungan dampak sebelum aturan ditegakkan** | 3 pesanan kehilangan jalur; 0 pembatalan pernah terjadi; 0 pemanggil frontend; 1 pemanggil backend | `PASS` | Bagian 2 |
| Verifikasi kontrak terhadap `r12` §7.2 | Badan permintaan dan status yang sah cocok; bentuk respons tidak berubah | `PASS` | Bagian 5 |
| **`T-97a` / `VAL-75`** — `Accepted` ditolak | `LabOrderConflictException` → `409` | `PASS` | 6.1 |
| **`T-97a` / `VAL-75`** — `InProcess` ditolak | Pesan dan kode yang sama | `PASS` | 6.1 |
| **`T-97a` / `VAL-75`** — `Completed` ditolak | Pesan dan kode yang sama | `PASS` | 6.1 |
| **`T-97a` / `VAL-75`** — `Cancelled` ditolak | Pesan dan kode yang sama | `PASS` | 6.1 |
| **`VAL-75`** — `OnHold` ditolak | Pesan dan kode yang sama | `PASS` | 6.1 — di luar daftar `T-97a`, diuji karena roadmap menghitungnya |
| **`T-97a`** — pesanan yang ditolak tidak ikut berubah | Kelima status tetap seperti semula sesudah penolakan | `PASS` | 6.1 |
| **`T-96a` / `VAL-74`** — alasan `null` | `LabOrderValidationException` → `422` | `PASS` | 6.1 |
| **`T-96a` / `VAL-74`** — alasan teks kosong | Pesan dan kode yang sama | `PASS` | 6.1 |
| **`T-96a` / `VAL-74`** — alasan hanya spasi | Pesan dan kode yang sama | `PASS` | 6.1 |
| **`T-96a` / `VAL-74`** — alasan hanya tab dan baris baru | Pesan dan kode yang sama | `PASS` | 6.1 |
| **`T-96a` / `VAL-74`** — badan permintaan tidak dikirim | Pesan dan kode yang sama | `PASS` | 6.1 — inilah bentuk pemanggil lama |
| **`T-96a`** — alasan terbaca kembali dari jejak audit | `ReasonNote` = alasan yang dikirim, sudah dirapikan | `PASS` | 6.2 |
| **`T-97b`** — pesanan `Requested` **tetap** dapat dibatalkan | `Cancelled`, `IsCancel`, aktor terekam | `PASS` | 6.2 |
| **`T-97b`** — pesanan `Confirmed` **tetap** dapat dibatalkan | `Order.Cancel: Confirmed→Cancelled` pada jejak audit | `PASS` | 6.2 |
| **`T-97b`** — jejak konfirmasi tidak terhapus oleh pembatalan | `ConfirmedAt`, `ConfirmedByUserId`, `ExaminerDoctorId` tetap utuh | `PASS` | 6.2 |
| **Langkah kontrol** — sesudah ditolak `VAL-74`, pembatalan yang sah tetap berhasil | Berhasil | `PASS` | 6.2 |
| Bentuk DTO permintaan | Properti publik `CancelLabOrderRequest` = `[CancelReason]` | `PASS` | 6.2 |
| Kebersihan database sesudah uji | Nol baris uji tersisa; 5 pesanan dengan sebaran status identik | `PASS` | 6.3 |
| Uji lewat HTTP sungguhan beserta `[Authorize]` dan `[AccessPermission]` | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan. Hak akses tidak berubah |
| Uji manual lewat antarmuka | Tidak dijalankan | `NOT FEASIBLE` | Layarnya adalah `FE-LAB-16`, belum dibangun |
| Penyerahan fakta pembatalan ke Billing | Tidak dijalankan | `NOT RUN` | **Jalurnya secara praktis tidak lagi tercapai** lewat pembatalan pesanan — lihat 7.1 |

### 6.1 Dua puluh tiga pemeriksaan dijalankan sungguhan terhadap database

Harness sekali pakai di scratchpad sesi, **di luar repository**, memakai `ApplicationDbContext` dan
`LabOrderService` yang **sebenarnya**. Tiga belas pesanan uji dibuat sendiri — **nol dari 5 pesanan
nyata disentuh**.

Hasil: **23 `PASS`, 0 `FAIL`.**

**Setiap penolakan diikuti pemeriksaan bahwa pesanannya tidak ikut berubah.** Lima status diuji,
dan kelimanya tetap persis seperti semula sesudah ditolak. Tanpa langkah ini, "menolak dengan
benar" tidak dapat dibedakan dari "menolak sesudah terlanjur mengubah sesuatu".

**Empat bentuk alasan kosong diuji, bukan satu.** `null`, teks kosong, spasi, serta tab dan baris
baru. Keempatnya ditolak dengan pesan yang sama — dan yang kelima, badan permintaan yang tidak
dikirim sama sekali, adalah bentuk yang akan dipakai pemanggil lama.

### 6.2 Jalur yang sah terbukti tidak ikut tertutup

Inilah butir DoD yang paling menentukan, karena pengetatan yang terlalu rakus tidak muncul sebagai
galat melainkan sebagai petugas yang tidak dapat bekerja.

| Pesanan | Status awal | Hasil | Jejak audit |
| --- | --- | --- | --- |
| Tanpa konfirmasi | `Requested` | **Berhasil dibatalkan** | `Order.Cancel: Requested→Cancelled`, `ReasonNote` terisi |
| Dikonfirmasi lebih dulu | `Confirmed` | **Berhasil dibatalkan** | `Order.Confirm: Requested→Confirmed` … `Order.Cancel: Confirmed→Cancelled` |

Jejak konfirmasi pesanan kedua **tetap utuh** sesudah dibatalkan: `ConfirmedAt`,
`ConfirmedByUserId`, dan `ExaminerDoctorId` tidak terhapus. Pembatalan tidak menghapus fakta bahwa
pesanan itu pernah dikonfirmasi, dan siapa yang mengonfirmasinya.

**Alasan terbaca kembali, dan terbukti dirapikan.** Dikirim
`"   Pasien menolak pengambilan sampel   "`, tersimpan
`"Pasien menolak pengambilan sampel"`.

### 6.3 Kebersihan database

| Butir | Sebelum | Sesudah |
| --- | :---: | :---: |
| Total `LabOrder` | 5 | **5** |
| Sebaran status | `Requested` 2, `Accepted` 2, `InProcess` 1 | **identik** |
| Baris `LabTransitionHistory` milik aktor uji | 0 | **0** |
| Total `LabSpecimen` | 5 | **5** |

Tiga belas pesanan uji dan empat baris riwayat dibuat lalu dihapus seluruhnya. Pemeriksaan penutup
dijalankan **terpisah dari harness**, memakai query langsung terhadap database, supaya
kebersihannya tidak dinilai oleh program yang sama yang membuat kotorannya.

### 6.4 Alat verifikasinya

Satu program sekali pakai di scratchpad sesi, di luar repository, yang merujuk project aplikasi.
Ia **menolak berjalan bila nama database tujuannya bukan `QuilvianNewDevYoga`** dan tidak menulis
credential ke berkas mana pun. `ClinicalMilestoneFactProducer` dilewatkan sebagai `null`; pesanan
uji sengaja dibuat **tanpa wadah**, sehingga jalur penyerahan ke Billing tidak pernah tercapai —
dan bila kelak tersentuh, harness gagal keras, bukan lolos diam-diam.

---

## 7. Acceptance criteria dan Definition of Done

### 7.1 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kedua aturan menolak sesuai matriks | **Terpenuhi** | `VAL-74` → `422` dan `VAL-75` → `409`; pesan dibandingkan kata demi kata terhadap matriks — 6.1 |
| **`T-97b` membuktikan jalur yang sah tidak ikut tertutup** | **Terpenuhi** | `Requested` dan `Confirmed` keduanya terbukti masih dapat dibatalkan — 6.2 |
| **Hitungan pesanan `Accepted`/`InProcess`/`OnHold` dilaporkan sebelum aturan ditegakkan** | **Terpenuhi** | Bagian 2, dikerjakan sebelum satu baris pun diubah: `Accepted` 2, `InProcess` 1, `OnHold` 0 — total **3** |
| Alasan terbaca kembali dari jejak audit | **Terpenuhi** | `ReasonNote` dibaca ulang dari database — 6.2 |
| Build | **Terpenuhi** | 0 Error, 207 Warning — sama persis dengan baseline |

Seluruh butir DoD terpenuhi.

### 7.2 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-96` — pembatalan **tanpa alasan ditolak**; alasan yang diterima tersimpan pada jejak audit dan **terbaca kembali per pesanan** | **Terpenuhi penuh** | Lima bentuk alasan kosong ditolak `422`; alasan yang sah dibaca ulang sebagai `ReasonNote` pada baris `Order.Cancel` pesanan itu — 6.1 dan 6.2 |
| `AC-97` — pembatalan **ditolak** ketika pesanan sudah `Accepted`, `InProcess`, `Completed`, atau `Cancelled`; **pesanan `Requested` dan `Confirmed` tetap dapat dibatalkan seperti sebelumnya** | **Terpenuhi penuh** | Keempat status ditolak `409`, ditambah `OnHold`; kedua status yang sah terbukti masih berjalan — 6.1 dan 6.2 |

Berbeda dengan `BE-LAB-31`, kedua acceptance criteria di sini **terpenuhi penuh**: keduanya tidak
menuntut satu pun ruas respons baru, sehingga tidak tersentuh celah kontrak yang menahan
`FE-LAB-15`.

---

## 8. Catatan penutup

### 8.1 Satu jalur kode menjadi praktis tidak tercapai, dan sengaja tidak dibongkar

`CancelAsync` memuat jalur penyerahan fakta pembatalan ke Billing untuk setiap wadah yang
**sebelumnya sudah dinyatakan layak**. Sesudah `VAL-75`, pesanan yang punya wadah layak berstatus
`Accepted` atau lebih — dan tidak lagi dapat dibatalkan. Jalur itu karena itu menjadi jalur yang
secara praktis tidak lagi tercapai lewat pembatalan pesanan.

**Ia sengaja dibiarkan berdiri**, dan bukan karena kelalaian:

- pembatalan pada **tingkat wadah** tetap memakainya, dan itu jalur yang berbeda;
- aturan koreksi pesanan yang sudah berjalan **belum diputuskan** (`LAB-P0-003`); ketika kelak
  diputuskan, jalur inilah yang akan dipakai;
- membongkarnya berarti menghapus kode yang terbukti benar untuk masalah yang belum dijawab.

Dicatat di sini supaya pembaca berikutnya tidak menyangka jalur itu kode mati.

### 8.2 Ringkasan

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 207 warning, **seluruhnya sudah ada sebelum task ini**; nol berasal dari ketiga berkas yang diubah |
| Masalah yang diketahui | **Satu, dan dilaporkan apa adanya:** pesanan yang sudah dibatalkan kini dijawab "Pesanan yang sudah diproses tidak dapat dibatalkan." — kalimat yang kurang tepat untuk keadaan itu. Pesannya diambil kata demi kata dari `VAL-75`; memperbaikinya adalah perubahan `LAB-VAL-v1` tersendiri. Lihat 4.3 |
| Risiko tersisa | **Pertama**, **3 pesanan** pada `QuilvianNewDevYoga` kehilangan jalur pembatalannya — 2 `Accepted`, 1 `InProcess`. Bila salah satunya perlu dibatalkan, tidak ada jalur tersisa sampai aturan koreksi diputuskan (`LAB-P0-003`). **Kedua**, ini perubahan **breaking**: setiap pemanggil yang membatalkan tanpa mengirim alasan mulai ditolak `422`. Hari ini nol pemanggil seperti itu ada, tetapi database lain mungkin berbeda dan **belum diperiksa**. **Ketiga**, `FE-LAB-16` wajib menyembunyikan aksi Batalkan pada status yang tidak sah; bila tidak, petugas akan menekan tombol yang selalu gagal |
| Perubahan sampingan | `NONE`. Perubahan yang sudah ada di working tree sebelum task ini tidak disentuh. `CancelLabSpecimenRequest` **tidak dihapus** — pembatalan tingkat wadah masih memakainya |
| Interupsi | `NONE` |
| Status Git | Lihat 8.3 |
| Langkah berikutnya | **1.** `FE-LAB-16` kini **tidak lagi tertahan** — ia hanya mengirim `cancelReason` dan membaca `orderStatus` yang sudah ada, sehingga tidak tersentuh celah kontrak yang menahan `FE-LAB-15`. Satu butir tetap terbuka pada task itu: label tombol pada alert konfirmasi akhir belum ditetapkan pemilik modul. **2.** `LAB-API-v1` `r13` — lima ruas respons yang menahan `FE-LAB-15`, usulnya pada [`BE-LAB-31.md`](BE-LAB-31.md) bagian 7.2. **3.** Periksa database selain `QuilvianNewDevYoga` sebelum aturan ini ikut ke sana: berapa pesanan `Accepted`/`InProcess`/`OnHold` di sana, dan adakah pemanggil yang masih membatalkannya. **4.** Putuskan aturan **koreksi** pesanan yang sudah berjalan (`LAB-P0-003`) — kini pertanyaannya mendesak, karena tiga pesanan sudah kehilangan jalurnya |

### 8.3 Status Git di akhir pekerjaan

Perubahan milik task ini:

```text
 M Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs
 M Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-32.md
```

Ditambah pembaruan artefak blueprint: `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`,
dan `roadmap/traceability.md`.

Ketiga berkas source **sudah** berstatus `M` sebelum task ini dimulai, karena pekerjaan
`BE-LAB-26`..`BE-LAB-31` dan `BE-EXT-04` belum di-commit. Bagian milik task ini terpisah jelas.

Nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, dan `deploy` dijalankan.
