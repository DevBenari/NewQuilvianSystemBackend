# Laporan Perubahan Backend — `BE-LAB-81`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-81` |
| Judul | Penjaga penyelesaian order |
| Slice | Gelombang `MVP-9e` — satu task |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6am.1** |
| Trace | `FR-15.17`; `LAB-DEC-154` (menutup `LAB-CONFLICT-014`), `LAB-DEC-156`; `LAB-DEC-135`, `AC-199` |
| Contract version | `LAB-API-v1` **`r36`** bagian 31; `LAB-VAL-v1` **`r14`** `VAL-146`; `LAB-STATE-v1` **`r7`** bagian 9 — ketiganya **`approved` 2026-09-25**, beserta keempat butir `02-backend-architecture.md` 22.7 |
| Dependency | `BE-LAB-76` ✅ (`ReleasedAt`, turunan `resultStatus`, `LabOrderResultProgressRules.Counted`) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1 (±14), berkas diubah 2 (3), logika bisnis **2** (aturan penyelesaian baru), kontrak API **2** (perilaku endpoint berubah; `400` → `409`; `errors` baru), database 0 (baca saja), keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`77` yang belum ter-commit. Rancangan disusun pada `cfafad8d`; **impact scan:** 112 commit, yang menyentuh Laboratorium hanya `LabExaminationController` dan `LabExaminationService` — **nol** pada ketiga berkas task ini. Rancangan bagian 22 tetap sahih; rujukan baris bergeser (`CompleteAsync` `:1051` → `:1063`, `LabOrderConflictException` `:1477` → `:1489`) |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** — penjaga berjalan **lewat HTTP terhadap PostgreSQL** (`409` `VAL-146` dengan rincian persis 31.3; `409` bagi order bukan `InProcess`; `404`; `401`) **tanpa satu pun penulisan**; build 0 error tanpa warning baru; harness **25/25**; regresi `BE-LAB-69` 31/31, `72` 43/43, `73` 38/38, `74` 28/28, `75` 20/20, `76` 16/16, `77` 21/21. Registri tetap **1575**. Penyelesaian yang **diterima** (`200`, `AC-244`) terbukti pada harness; lewat HTTP ia menulis ke database bersama dan dev nol punya order yang memenuhi syaratnya |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `LabOrderService.CompleteAsync`, `LabOrderController.ExecuteAsync`, `LabOrderDtos`. `NEW CODE` — `LabOrderCompletionBlockedException`, `LabOrderCompletionBlockedItem`, `ReadCompletionBlockersAsync` |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service, controller hanya memetakan), `QBE-API-001` (`ApiResponse`, `errors` mengikuti preseden Farmasi), `QBE-DTO-001` (rincian lewat DTO; entity tidak diekspos), `QBE-LOG-001` (penyelesaian yang berhasil tetap tercatat sebagai riwayat `Order.Complete` beserta pelakunya, lewat `MoveOrderStatusAsync` yang tidak diubah; penolakan bukan perubahan state). **Tidak berlaku:** `QBE-PERM-*` (nol aksi baru — tetap `LabOrder : Process`), `QBE-ENT-*`/`CFG-*` (nol entity), `QBE-DB-*` (nol migration) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (baris `Lab` `ACTIVE`) |

---

## 1. Masalah yang diperbaiki

**Dua *Selesai* dengan dua arti** (`LAB-CONFLICT-014`). `PUT /lab-orders/{id}/complete` memindahkan
order `InProcess` → `Completed` tanpa memeriksa hasil apa pun. Sejak `BE-LAB-76`, label hasil order
juga berbunyi *Selesai*, tetapi artinya *seluruh pemeriksaan tidak batal sudah dirilis*. Petugas
dapat menandai order *Selesai* sementara Hemoglobin belum divalidasi, dan layar menampilkan dua
*Selesai* yang tidak sepakat.

**Kode status yang menyimpang dari kontrak.** Order yang bukan `InProcess` dijawab `400`, padahal
`LAB-STATE-v1` bagian 1 sudah menjanjikan `409`.

---

## 2. Proses bisnis

**Aturan baru** (`LAB-DEC-154`): order hanya dapat ditandai *Selesai* bila **setiap** pemeriksaan yang
tidak batal dan tidak gugur **sudah dirilis**.

**Contoh — `AC-243`, lalu `AC-244`.** Order berisi Kalium (dirilis), Hemoglobin (tervalidasi), Ureum
(Draft), dan Glukosa (dibatalkan). Petugas menekan *Selesai*:

> `409` — *"Order belum dapat diselesaikan karena masih terdapat pemeriksaan yang belum dirilis."*
>
> | Pemeriksaan | Keadaan |
> | --- | --- |
> | Hemoglobin | Tervalidasi |
> | Ureum | Draft |

Kalium tidak disebut karena sudah dirilis; Glukosa tidak disebut karena batal. Order tetap
`InProcess`. Sesudah Hemoglobin dan Ureum dirilis, *Selesai* diterima: order `Completed` dan satu
baris riwayat tercatat.

**Petugas membaca seluruh penahan sekaligus.** Rincian memuat **setiap** pemeriksaan yang belum
dirilis, bukan hanya yang pertama. Tanpa itu petugas harus menekan *Selesai* berulang kali untuk
menemukan satu per satu.

**Label keadaan** (`LAB-DEC-156`):

| Keadaan hasil | Label |
| --- | --- |
| Belum diisi | *Menunggu Hasil* |
| Draft | *Draft* |
| Final — termasuk hasil Mikrobiologi *Sementara* | *Menunggu Validasi* |
| Tervalidasi, belum dirilis | *Tervalidasi* |

**Patologi Anatomi** tidak berhasil per pemeriksaan. Keadaannya dibaca dari laporan PA order:
belum ada laporan → *Menunggu Hasil*; laporan belum difinalkan → *Draft*; difinalkan →
*Menunggu Validasi*.

**Order Patologi Anatomi dan Mikrobiologi tidak dapat diselesaikan** sampai jalur validasinya berdiri
(`AC-245`, `LAB-DEC-154` butir 5). Ini disengaja. Hari ini nol layar memanggil endpoint ini, sehingga
nol pengguna terhenti.

**Order yang seluruh pemeriksaannya batal diterima** (22.7 butir 3). Menolaknya berarti order itu
terkunci di `InProcess` selamanya, sebab pembatalan order hanya sah pada `Requested`/`Confirmed`.

**Order yang bukan `InProcess`** kini dijawab `409`, bukan `400`. Bunyi pesannya tetap sama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6am.0-6am.2 | Cakupan, tiga jebakan, DoD |
| `contracts/api-contract.md` bagian 31 | Kode status, bentuk `errors`, ruas rincian |
| `contracts/validation-matrix.md` `VAL-146` | Teks dan kode |
| `contracts/state-transition-matrix.md` bagian 9 | Transisi sah dan tidak sah |
| `02-backend-architecture.md` bagian 22 | Urutan pemeriksaan, label, keputusan 22.7 |
| `testing/acceptance-test-matrix.md` amandemen 2026-09-25 (ketiga) | `AC-243`..`AC-245`, baris 22.7 butir 3 dan 4 |
| `Services/LabOrderService.cs` — `CompleteAsync`, `MoveOrderStatusAsync`, `LoadTrackedAsync` | Jalur yang diubah dan yang wajib dibiarkan |
| `Services/LabOrderResultProgressRules.cs` | Definisi *pemeriksaan yang dihitung* yang wajib dipakai ulang |
| `Services/LabExaminationService.cs` — `DeriveResultStatus`, `ResolveDiscipline` | Turunan keadaan dan pembacaan disiplin |
| `Models/LabPathologyReport.cs`, `Services/LabPathologyReportService.cs` | Kapan laporan PA lahir dan kapan difinalkan |
| `PharmacyManagement/Controllers/DrugReturnController.cs` | Preseden `errors` |
| `Responses/ApiResponse.cs` | `Fail(statusCode, message, errors)` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabOrderService.cs` | `CompleteAsync` kini: muat order (`404`); bukan `InProcess` → `LabOrderConflictException` dengan bunyi pesan lama; `ReadCompletionBlockersAsync`; ada penahan → `LabOrderCompletionBlockedException`; selain itu `MoveOrderStatusAsync` **apa adanya**. `ReadCompletionBlockersAsync` — satu kueri pemeriksaan (`Counted`, `ReleasedAt` kosong, urut pemesanan), ditambah satu kueri laporan PA **hanya bila** ada pemeriksaan PA. `CompletionBlockedLabel` — keempat label `LAB-DEC-156`. `LabOrderCompletionBlockedException` baru dengan `Code` dan `Details` |
| `Controllers/LabOrderController.cs` | Satu `catch` di `ExecuteAsync` → `409` dengan `errors: { code, details }`, sebelum `catch (InvalidOperationException)`. `complete` tidak lagi mengiklankan `400` di Swagger |
| `DTOs/LabOrderDtos.cs` | `LabOrderCompletionBlockedItem` — `examinationId`, `procedureName`, `resultStatus`, `status` |

**Nol entity, nol migration, nol aksi hak akses baru.** Registri tetap 1575.

**Ketiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Berhenti pada penahan **pertama** | Seluruh baris dikumpulkan dalam satu kueri | 18 penahan → 18 baris |
| Menghitung keadaan dengan rumus sendiri | `LabExaminationService.DeriveResultStatus` untuk setiap disiplin — **termasuk PA**, yang diberi waktu laporan PA (`CreateDateTime` sebagai *sudah diisi*, `FinalizedAt` sebagai *Final*). Pemeriksaan yang dihitung = `LabOrderResultProgressRules.Counted`, definisi yang sama dengan label `resultProgress` | Keempat keadaan tepat pada satu order |
| Mengubah `MoveOrderStatusAsync` | Tidak disentuh; penjaga berjalan **sebelum** memanggilnya | `start-process` pada order `InProcess` tetap `InvalidOperationException` → `400` |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Turunan `resultStatus` | **Tidak dipindah.** Roadmap memintanya dipindah bila hasil `BE-LAB-76` khusus Patologi Klinik. Ternyata `DeriveResultStatus` hanya membaca kolom `LabExamination` yang juga diisi jalur Mikrobiologi, jadi sudah netral disiplin |
| Disiplin order lama yang kosong | Dibaca seperti `ResolveDiscipline`: disiplin order, lalu katalog pemeriksaan. Pemeriksaan PA pada order tanpa disiplin tetap dibaca dari laporan PA |
| Urutan rincian | Urut pemesanan (`CreateDateTime`, lalu `Id`) — tetap sama di setiap panggilan |
| `procedureName` kosong | Snapshot pemeriksaan, lalu nama katalog; string kosong bila keduanya tidak ada, sebab kontrak menyatakan ruas ini tidak boleh `null` |
| Log penolakan | **Tidak ditambah.** Penolakan bukan perubahan data (22.8), dan `MoveOrderStatusAsync` pun tidak menulis log aplikasi — riwayatnya `LabTransitionHistory` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Sesuai `r36` bagian 31.** Perilaku `PUT /{id}/complete` berubah; `400` → `409` bagi order bukan `InProcess`; `errors.code` dan `errors.details` aditif. **Nol pemanggil hari ini** — frontend tidak memanggil endpoint ini |
| Database | **Baca saja pada penolakan.** Penolakan Patologi Klinik atau Mikrobiologi: 2 kueri (order, pemeriksaan) berapa pun jumlah penahannya; Patologi Anatomi: 3. Penyelesaian yang diterima menulis persis seperti sebelumnya |
| Keamanan/Auth | Hak yang sudah ada, `LabOrder : Process`. Rincian memuat id, **nama pemeriksaan**, dan keadaannya — **nol nilai hasil**, nol nama pasien (22.8) |

**Selisih kode–kontrak yang dicatat, tidak diubah.** Kontrak 31.2 menulis pesan `404` sebagai
*"Pesanan laboratorium tidak ditemukan."*, sedangkan kode, sebelum dan sesudah task ini, menjawab
*"Order laboratorium tidak ditemukan."*. Pesan itu berasal dari `LoadTrackedAsync`, yang dipakai
seluruh tindakan order, dan 31.4 tidak mencantumkannya sebagai perubahan. Mengubahnya di sini berarti
mengubah jawaban tindakan lain tanpa keputusan. Diusulkan kontraknya yang diselaraskan pada revisi
berikutnya.

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}/complete` | Menandai pesanan selesai — **hanya bila seluruh pemeriksaan tidak batal sudah dirilis** | `LabOrder : Process` | - | `ApiResponse<LabOrderDetailResponse>` |

| Kode | Kapan | Pesan |
| --- | --- | --- |
| `200` | Seluruh pemeriksaan tidak batal dirilis, atau tidak ada pemeriksaan tidak batal | *"Order laboratorium selesai dikerjakan."* |
| `401` | Tanpa token | — |
| `403` | Jabatan tidak memegang `LabOrder : Process` | — |
| `404` | Order tidak ada | *"Order laboratorium tidak ditemukan."* |
| `409` | Order bukan `InProcess` | *"Pesanan berstatus {status} tidak dapat dipindahkan ke Completed."* |
| `409` | `VAL-146` | *"Order belum dapat diselesaikan karena masih terdapat pemeriksaan yang belum dirilis."* + `errors` |
| `409` | Bentrok versi | Pesan bentrokan yang sudah berlaku |

Contoh respons `VAL-146` — **tangkapan sungguhan** dari database dev (order LAB-RSMMC-000002):

```json
{
  "success": false,
  "statusCode": 409,
  "message": "Order belum dapat diselesaikan karena masih terdapat pemeriksaan yang belum dirilis.",
  "data": null,
  "errors": {
    "code": "LAB_ORDER_COMPLETION_BLOCKED",
    "details": [
      { "examinationId": "b71ed868-…", "procedureName": "Kalium", "resultStatus": "NotEntered", "status": "Menunggu Hasil" }
    ]
  }
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error, 230 warning**; **nol** dari berkas yang disentuh; 87 detik | `PASS` | Keluaran build |
| Harness EF InMemory `BE-LAB-81` | **25 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-69` 31/31, `72` 43/43, `73` 38/38, `74` 28/28, `75` 20/20, `76` 16/16, `77` 21/21 | `PASS` | Harness yang sama dijalankan ulang |
| Startup Development | Registri tetap **1575**; nol galat di luar yang sudah dikenal | `PASS` | Log startup |
| Swagger `PUT /{id}/complete` | Respons `200`, `404`, `409` — `400` hilang; tanpa teks deskripsi | `PASS` | `/swagger/health-services/swagger.json` |
| **HTTP terhadap PostgreSQL** | Rincian di bawah | `PASS` | Panggilan sungguhan |
| Nol penulisan oleh panggilan HTTP | Status, `Version`, dan `CompletedAt` keempat order identik sebelum dan sesudah; baris riwayat `Order.Complete` tetap **0** | `PASS` | Kueri baca-saja |
| **`AC-244` kueri terbalik** | Order `Completed` yang memuat pemeriksaan tidak batal belum dirilis: **0** | `PASS` | Kueri baca-saja — dev belum punya order `Completed` sama sekali |

**Rincian HTTP** — aplikasi sungguhan, database dev bersama, akun superadmin. Order dipilih lebih dulu
lewat kueri baca-saja. Satu-satunya order `InProcess` di dev punya pemeriksaan yang belum dirilis,
sehingga **tidak ada panggilan yang dapat menulis**.

| Order | Keadaan | Hasil |
| --- | --- | --- |
| LAB-RSMMC-000002 | `InProcess`, Kalium belum diisi | **`409`** `VAL-146`; `errors.code = LAB_ORDER_COMPLETION_BLOCKED`; satu baris *Kalium — NotEntered — Menunggu Hasil*; `Version` tetap 4 |
| LAB-RSMMC-000001 | `Accepted` | **`409`** — *"Pesanan berstatus Accepted tidak dapat dipindahkan ke Completed."* (sebelumnya `400`) |
| LAB-RSMMC-000009 | `Requested`, Patologi Anatomi | **`409`** — pesan yang sama untuk `Requested` |
| LAB-RSMMC-000006 | `Confirmed` | **`409`** — pesan yang sama untuk `Confirmed` |
| Id acak | — | `404` |
| Tanpa token | — | `401` |

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| **`AC-243`** — Kalium dirilis, Hemoglobin tervalidasi, Ureum Draft, Glukosa batal, Klorida gugur, Natrium terhapus | `409`, pesan kata per kata, kode benar; **tepat dua** baris urut pemesanan — *Hemoglobin Tervalidasi*, *Ureum Draft* |
| … yang dirilis, batal, gugur, terhapus | Tidak disebut |
| … keadaan order sesudahnya | `InProcess`; `Version` 3 → 3; `CompletedAt` kosong; nol riwayat |
| Ruas rincian | Hanya empat ruas — nol nilai hasil |
| **`AC-244`** — sesudah Hemoglobin dan Ureum dirilis | `200`; `Completed`; `CompletedAt` terisi; `Version` 3 → 4; **tepat satu** riwayat `Order.Complete`, `InProcess → Completed`, pelaku sesuai |
| Menyelesaikan ulang order `Completed` | `409` (bukan `400`) |
| **`AC-245`** — PA laporan Final (dua pemeriksaan) | `409`; kedua baris *Final — Menunggu Validasi* |
| PA laporan belum difinalkan; PA tanpa laporan | *Draft*; *Menunggu Hasil* |
| **`AC-245`** — Mikrobiologi hasil *Sementara* dan Final biasa | `409`; keduanya *Menunggu Validasi* |
| Order lama tanpa disiplin — katalog PA; katalog PK | Dari laporan PA; dari pemeriksaan |
| Keempat keadaan pada satu order | *Menunggu Hasil*, *Draft*, *Menunggu Validasi*, *Tervalidasi*; tidak pernah `Released` |
| **22.7 butir 3** — semua batal/gugur; tanpa pemeriksaan | `200` keduanya |
| **22.7 butir 4** — `Accepted`, `Requested`, `OnHold`, `Cancelled` | `409` dengan bunyi lama; order tak berubah |
| Order tidak ada | `404` |
| Jumlah kueri penolakan | 18 penahan = 2 penahan = **2 kueri**; PA **3** |
| `MoveOrderStatusAsync` bagi tindakan lain | `start-process` pada order `InProcess` tetap `InvalidOperationException` → `400` |

Uji manual: **`NOT FEASIBLE`** — nol layar memanggil endpoint ini (22.6).

**Tidak dijalankan:**

- Penyelesaian yang **diterima** lewat HTTP (`AC-244`), dan `AC-245` lewat HTTP. Keduanya butuh order
  `InProcess` yang seluruh pemeriksaannya dirilis, atau order PA/Mikrobiologi `InProcess`. Dev nol
  punya keduanya, dan menyiapkannya berarti menulis ke database bersama.
- Balapan penyelesaian lawan penambahan pemeriksaan — risiko yang disadari 22.6, tidak ditutup task ini.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-243`, `VAL-146` — dua baris rincian tepat; order tetap `InProcess`; `Version` tidak naik | ✅ **Terpenuhi** pada harness; bentuk respons `VAL-146` dan nol penulisan **lewat HTTP** | — |
| `AC-244` — `200`, `Completed`, `CompletedAt`, satu riwayat; yang batal tidak disebut | ✅ **Terpenuhi** pada harness | Belum lewat HTTP |
| `AC-244` kueri terbalik — nol order `Completed` berpenahan | ✅ **0** di dev | Kueri baca-saja |
| `AC-245` — PA laporan Final dan Mikrobiologi *Sementara* → `409` *Menunggu Validasi* | ✅ **Terpenuhi** pada harness | — |
| `AC-247` bagian backend — label rincian | ✅ **Terpenuhi** — keempat label tepat per `resultStatus` | Label *Dirilis* dan tampilannya milik frontend |
| 22.7 butir 3 — order yang seluruh pemeriksaannya batal → `200` | ✅ **Terpenuhi** pada harness | — |
| 22.7 butir 4 — order `Accepted` → `409`, bukan `400` | ✅ **Terpenuhi** — harness **dan HTTP** | — |
| DoD — rincian lengkap; `400` → `409`; nol migration; nol permission baru; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** Selisih pesan `404` kontrak–kode (3.3) — diusulkan kontraknya diselaraskan. **2.** Risiko yang disadari 22.6: pemeriksaan yang ditambahkan pada detik yang sama dengan penyelesaian lolos, sebab jalur tambah pemeriksaan tidak menaikkan `Version` order |
| Risiko tersisa | **Rendah.** Perilaku berubah hanya pada satu endpoint yang nol pemanggilnya hari ini; jalur tulis tidak diubah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-81`: ` M` `Services/LabOrderService.cs`, `Controllers/LabOrderController.cs`, `DTOs/LabOrderDtos.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`; `??` laporan ini. Perubahan `BE-LAB-67`..`77` yang belum ter-commit ikut ada, termasuk suntingan `BE-LAB-76` pada `LabOrderService.cs` dan `LabOrderDtos.cs`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** Syarat *"`BE-LAB-81` terpasang sebelum langkah 4 `MVP-9d`"* kini terpenuhi **pada kode**; deploy tetap wewenang terpisah. **2.** Larangan *jangan bersamaan dengan `BE-LAB-79`* tidak lagi berlaku; `BE-LAB-79` tetap menunggu `BE-LAB-78`. **3.** Task backend yang siap: `BE-LAB-78` dan `BE-LAB-82` |
