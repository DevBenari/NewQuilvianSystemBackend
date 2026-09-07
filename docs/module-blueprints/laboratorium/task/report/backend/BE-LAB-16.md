# Laporan Perubahan Backend — `BE-LAB-16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-16` |
| Judul | Endpoint pemeriksaan terpesan |
| Slice | `S2` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, gelombang `MVP-1` |
| Trace | `FR-02.1`, `FR-02.2`; `LAB-DEC-024`, `LAB-DEC-026`; BR-20; `AC-35`; `VAL-17` .. `VAL-20`; `LAB-STATE-v1` r2 bagian 3 |
| Contract version | `LAB-API-v1` r3 grup Lab Examination — `approved`, dikunci 2026-09-02 |
| Dependency | `BE-LAB-09` — **`SELESAI`** |
| Klasifikasi | `HEAVY` — skor 9. Repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 1, database 1, keamanan/auth 1, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, kontrak dan artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `17a331b`, branch `yoga` |
| Tanggal | 2026-09-03 |
| Status | **`SELESAI`** — empat endpoint tersedia, `VAL-17` .. `VAL-20` terbukti, checker QBE `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` seluruhnya — controller, service, DTO, dan test semuanya baru |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DEL-001`, `QBE-MOD-001`, `QBE-NAM-002`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | Seluruh `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*` — entity dan configurationnya sudah dibuat `BE-LAB-09`, dan task ini tidak menyentuh schema. `QBE-CODE-*` — tidak ada alokasi nomor bisnis. `QBE-PAGE-001` — kontrak mengunci kedua endpoint baca sebagai `List<T>` ber-scope satu pesanan atau satu wadah, bukan daftar global |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

`BE-LAB-09` membangun tabel `LabExamination` beserta seluruh aturan strukturnya, tetapi
**tidak satu pun endpoint** yang dapat menyentuhnya. Tabelnya berdiri kosong: tidak ada cara
menambah pemeriksaan terpesan, tidak ada cara membacanya, dan tidak ada cara membatalkan satu
pemeriksaan.

Akibat nyatanya bagi petugas:

> Ny. Sari dipesankan darah lengkap. Petugas mengambil satu tabung ungu, dan dari tabung itu
> akan dikerjakan hemoglobin, leukosit, dan trombosit. Struktur untuk mencatat ketiganya sudah
> ada sejak `BE-LAB-09` — tetapi tanpa endpoint, tidak ada satu pun layar yang dapat mengisinya.
> Pemisahan wadah dari pemeriksaan yang sudah dibangun itu belum berarti apa-apa bagi siapa pun
> di lapangan.

Ada kebutuhan kedua yang lebih halus, dan inilah yang membuat task ini bukan sekadar
"menambahkan CRUD":

> Dokter membatalkan permintaan hemoglobin karena hasil pemeriksaan lain sudah menjawab
> pertanyaannya, sementara leukosit dan trombosit tetap dikerjakan dari tabung yang sama.
> Pembatalan itu harus mengenai **satu** pemeriksaan saja. Bila ia ikut menggugurkan dua
> pemeriksaan lain, atau ikut mengubah status tabungnya, petugas kehilangan pekerjaan yang
> sebenarnya masih berjalan — dan tabung yang masih sah menjadi tidak dapat diproses.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Aspek | Isi |
| --- | --- |
| Tujuan | Pemeriksaan terpesan dapat ditambahkan, dibaca, dan dibatalkan satu per satu, tanpa mengganggu wadah maupun pemeriksaan lain |
| Pelaku | Petugas yang merencanakan pemeriksaan (`LabExamination : Create`); petugas yang membaca daftar (`: Read`); petugas berwenang yang membatalkan (`: Update`) |
| Pemicu | Wadah sudah direncanakan, dan jenis pemeriksaan yang akan dikerjakan darinya ditetapkan |
| Hasil akhir | Satu wadah berbarcode tunggal menopang beberapa baris pemeriksaan, masing-masing dengan salinan tarifnya sendiri |

### 2.2 Langkah yang berurutan

1. Petugas merencanakan wadah pada sebuah pesanan — satu tabung, satu barcode.
2. Untuk setiap jenis pemeriksaan yang akan dikerjakan dari tabung itu, ia memanggil
   `POST /lab-examinations/by-order/{labOrderId}` dengan `specimenId` dan `procedureId`.
3. Backend memeriksa berurutan: pesanannya masih menerima; wadahnya milik pesanan itu; wadahnya
   belum diputuskan; jenis pemeriksaannya benar-benar pemeriksaan laboratorium; belum ada
   pemeriksaan sejenis pada wadah itu; dan tarifnya sudah diatur.
4. Barulah baris pemeriksaan terbentuk, berstatus `Ordered`, dengan **salinan tarif yang
   diambil backend** — bukan harga kiriman pemanggil.
5. Layar membaca isinya lewat `GET /by-specimen/{specimenId}` untuk melihat apa saja yang
   ditopang satu tabung, atau `GET /by-order/{labOrderId}` untuk melihat seluruh isi pesanan.
6. Bila satu pemeriksaan dibatalkan, `POST /{id}/cancel` mengubah **hanya baris itu**.

### 2.3 Contoh berangka

| Langkah | Tindakan | Hasil |
| ---: | --- | --- |
| 1 | Tambah Hemoglobin ke tabung `BC-0001` | Baris 1, `Ordered`, harga 35.000 |
| 2 | Tambah Leukosit ke tabung yang sama | Baris 2, `Ordered`, harga 30.000 |
| 3 | `GET /by-specimen` | **Dua** baris, keduanya berbarcode `BC-0001` |
| 4 | Batalkan Hemoglobin | Baris 1 `Cancelled`; **baris 2 tetap `Ordered`**; tabung tetap `Planned` |

Langkah 4 adalah inti butir DoD task ini.

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi | Kode | Aturan |
| --- | --- | :---: | --- |
| Jenis pemeriksaan bukan pemeriksaan laboratorium | Ditolak | `422` | `VAL-17` |
| Wadah penopang sudah dinyatakan layak atau ditolak | Ditolak | `409` | `VAL-18` |
| Membatalkan pemeriksaan yang sudah gugur bersama wadah yang ditolak | Ditolak | `409` | `VAL-19` |
| Tarif jenis pemeriksaan belum diatur | Ditolak, dan **tidak ada baris yang terlanjur tersimpan tanpa harga** | `422` | `VAL-20` |
| Jenis pemeriksaan yang sama dimasukkan dua kali pada satu wadah | Ditolak | `409` | BR-20 |
| Wadah milik pesanan lain | Ditolak | `422` | — |
| Pesanan sudah dibatalkan atau selesai | Ditolak | `409` | — |
| Membatalkan pemeriksaan yang sudah dibatalkan | Ditolak | `409` | — |
| Pesanan, wadah, atau pemeriksaan tidak ditemukan | Ditolak | `404` | — |

**Mengapa `VAL-20` menghentikan penyimpanan, bukan menyimpan harga kosong.** Baris pemeriksaan
tanpa harga akan sampai ke Billing sebagai nilai nol yang tampak sah. Menolak di depan membuat
kekurangan data induk terlihat oleh orang yang dapat memperbaikinya, alih-alih terkubur menjadi
tagihan yang salah.

**Jenis pemeriksaan yang sama boleh berdiri pada wadah yang berbeda.** Keunikannya per wadah,
bukan per pesanan — misalnya pemeriksaan ulang dari tabung kedua.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/backend-roadmap.md` bagian 4 dan 8.2
- `contracts/api-contract.md` grup Lab Examination
- `contracts/validation-matrix.md` bagian 3
- `contracts/state-transition-matrix.md` bagian 3
- `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` — pola penyalinan tarif yang dipakai ulang
- `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs` dan configurationnya
- `Areas/HealthServices/LaboratoryManagement/Controllers/LabValueBoundController.cs` — pola pemetaan exception ke status HTTP

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../DTOs/LabExaminationDtos.cs` | **Baru.** `LabExaminationResponse`, `AddLabExaminationRequest`, `CancelLabExaminationRequest` |
| `.../Services/LabExaminationService.cs` | **Baru.** Empat operasi domain beserta penegakan `VAL-17` .. `VAL-20`, dan dua tipe exception yang dipetakan menjadi `409` dan `422` |
| `.../Controllers/LabExaminationController.cs` | **Baru.** Empat endpoint beserta `[AccessAction]` dan `[AccessPermission]`, sehingga permissionnya terdaftar sendiri lewat `AccessMenuSeeder` |
| `Program.cs` | Registrasi `LabExaminationService` sebagai scoped service |
| `tests/.../LabExaminationEndpointTests.cs` | **Baru.** 22 uji |
| `contracts/api-contract.md` | Keempat endpoint berpindah dari **Rencana** menjadi **Tersedia** |
| `roadmap/backend-roadmap.md`, `roadmap/traceability.md` | Status dan bukti diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Empat endpoint** grup Lab Examination berpindah dari **Rencana (belum tersedia)** menjadi tersedia. Bentuk route, verb, request, dan response cocok satu per satu dengan `LAB-API-v1` r3. **Tidak ada amandemen kontrak** — task ini melaksanakan apa yang sudah dikunci |
| Database | **Tidak ada dampak schema.** Tabel, index, dan relasinya sudah dibuat `BE-LAB-09` lewat migration `20260903071535_AddLabExamination`. **Tidak ada migration baru dan tidak ada perintah database yang dijalankan** |
| Keamanan/Auth | Tiga pasangan hak akses baru terdaftar sendiri lewat metadata: `LabExamination : Read`, `: Create`, dan `: Update`. Tidak ada model otorisasi baru |

### 3.4 Selisih yang perlu diketahui

| No | Selisih | Penjelasan |
| ---: | --- | --- |
| 1 | Kartu task pada roadmap semula menyebut **`VAL-05` dan `VAL-07`** pada Verifikasi dan DoD; yang dikerjakan adalah **`VAL-17` .. `VAL-20`** | Bagian 8.2 roadmap yang sama menempatkan `VAL-05` .. `VAL-16` pada `BE-LAB-12` dan `VAL-17` .. `VAL-20` pada `BE-LAB-16`. Bagian 8.2 adalah peta kepemilikan yang menyeluruh dan konsisten; kartu tasknya keliru. `VAL-05` — *wadah tanpa satu pun pemeriksaan* — melekat pada endpoint perencanaan wadah yang tidak dimiliki task ini, sehingga tidak dapat ditegakkan dari sini. Inti `VAL-07` — *jenis pemeriksaan yang sama dua kali dalam satu wadah* — **tetap ditegakkan**, karena jalur tambah pemeriksaan memang dapat melanggarnya. **Kartu task sudah diselaraskan 2026-09-03** atas instruksi pemilik modul: Verifikasi dan DoD-nya kini berbunyi `VAL-17` .. `VAL-20` |
| 2 | Grup Lab Examination punya enam endpoint pada kontrak; task ini membuat empat | `PUT /{id}/urgency` dan `PUT /{id}/duplo` dimiliki `BE-LAB-10` menurut bagian 8.1 roadmap. Keduanya sengaja belum dibuat, dan uji kontrak menghitung tepat empat supaya penambahan diam-diam ketahuan |
| 3 | Alasan pembatalan disimpan pada log, bukan pada baris pemeriksaan | Kamus data `LabExamination` tidak memuat kolom alasan pembatalan, dan menambahkannya adalah perubahan schema yang tidak diberi wewenang task ini |
| 4 | `LabExaminationResponse` memuat `UrgencyMarkedByUserId` di samping `UrgencyMarkedByUserName` yang disebut kontrak | Kontrak menyebut nama tampilannya; id-nya ditambahkan karena seluruh DTO Laboratorium lain mengekspos pelaku sebagai `Guid`, dan layar membutuhkannya untuk membandingkan dengan pengguna yang sedang login. Penambahan aditif |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/by-order/{labOrderId}` | Daftar pemeriksaan terpesan pada satu pesanan | `LabExamination : Read` |
| `GET` | `/by-specimen/{specimenId}` | Daftar pemeriksaan yang ditopang satu wadah | `LabExamination : Read` |
| `POST` | `/by-order/{labOrderId}` | Menambah pemeriksaan terpesan dan menautkannya ke wadah | `LabExamination : Create` |
| `POST` | `/{id}/cancel` | Membatalkan **satu** pemeriksaan terpesan | `LabExamination : Update` |

| Endpoint | Request | Response |
| --- | --- | --- |
| `POST /by-order/{labOrderId}` | `AddLabExaminationRequest` — `SpecimenId`, `ProcedureId` | `ApiResponse<LabExaminationResponse>` |
| `POST /{id}/cancel` | `CancelLabExaminationRequest` — `Reason` | `ApiResponse<LabExaminationResponse>` |
| Kedua `GET` | — | `ApiResponse<List<LabExaminationResponse>>` |

Kode status: `200` berhasil; `400` isian tidak sah; `401` belum login; `403` tanpa hak akses;
`404` pesanan, wadah, atau pemeriksaan tidak ditemukan; `409` `VAL-18`, `VAL-19`, jenis ganda,
pesanan sudah ditutup, atau pemeriksaan sudah dibatalkan; `422` `VAL-17`, `VAL-20`, atau wadah
milik pesanan lain.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test ...UnitTests.InMemory --filter "FullyQualifiedName~LabExaminationEndpoint"` | `Failed: 0, Passed: 22, Total: 22` | `PASS` | Keluaran perintah |
| Seluruh suite `QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 1073, Total: 1074` | `EXISTING / ENVIRONMENT ISSUE` | Satu-satunya kegagalan adalah `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`, terbuka sejak sebelum seluruh pekerjaan Laboratorium |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | `VIOLATION: 0`, `Final result: PASS` | `PASS` | Keluaran perintah |
| Keempat endpoint memakai route, verb, dan hak akses yang dikunci kontrak | Seluruhnya cocok | `PASS` | `KeempatEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak` |
| Base route dan jumlah endpoint | `lab-examinations`, tepat empat endpoint | `PASS` | `ControllerPemeriksaan_MemakaiBaseRouteYangDikunciKontrak` |
| Tidak ada jalur hapus | Nol `DELETE` | `PASS` | `ControllerPemeriksaan_TidakMemilikiJalurHapus` |
| **`AC-35`** — satu wadah menopang dua pemeriksaan | Keduanya terbaca lewat `GET /by-specimen` dengan barcode `BC-0001` yang sama, harga 35.000 dan 30.000 | `PASS` | `AC35_SatuWadahMenopangDuaPemeriksaan_KeduanyaTerbacaLewatBySpecimen` |
| Salinan tarif diambil backend | Harga, kode tarif, dan nama pemeriksaan terisi dari data induk | `PASS` | `MenambahPemeriksaan_MenyalinTarifDariDataInduk` |
| Harga dan kesegeraan tidak dapat diselipkan pemanggil | `AddLabExaminationRequest` terbukti hanya punya `SpecimenId` dan `ProcedureId` | `PASS` | `PermintaanTambah_TidakPunyaRuasHargaMaupunKesegeraan` |
| **`VAL-17`** — bukan pemeriksaan laboratorium | Ditolak beserta pesan kontrak | `PASS` | `VAL17_JenisPemeriksaanBukanLaboratorium_Ditolak` |
| **`VAL-18`** — wadah sudah diputuskan | Ditolak untuk `Accepted` maupun `Rejected` | `PASS` | `VAL18_WadahYangSudahDiputuskan_TidakDapatBertambahIsinya`, dua baris `InlineData` |
| **`VAL-19`** — pemeriksaan sudah gugur | Ditolak beserta pesan kontrak | `PASS` | `VAL19_PemeriksaanYangSudahGugurBersamaWadah_TidakDapatDibatalkan` |
| **`VAL-20`** — tarif belum diatur | Ditolak, dan nol baris tersimpan | `PASS` | `VAL20_TarifBelumDiatur_Ditolak` |
| Jenis pemeriksaan ganda pada satu wadah | Ditolak; hanya satu baris tersimpan | `PASS` | `JenisPemeriksaanYangSama_TidakBolehDuaKaliPadaSatuWadah` |
| Jenis yang sama pada wadah berbeda | Diizinkan; dua baris tersimpan | `PASS` | `JenisPemeriksaanYangSama_BolehPadaWadahYangBerbeda` |
| Wadah milik pesanan lain | Ditolak | `PASS` | `WadahMilikPesananLain_Ditolak` |
| **Butir DoD terpenting** — pembatalan satu pemeriksaan | Baris itu `Cancelled`; **pemeriksaan tetangga tetap `Ordered`**; **wadahnya tetap `Planned`** | `PASS` | `MembatalkanSatuPemeriksaan_TidakMengubahPemeriksaanLainMaupunWadahnya` |
| Jejak pembatalan dan token konkurensi | `IsCancel`, `CancelDateTime`, `CancelBy`, `UpdateBy` terisi; `Version` naik dari 0 ke 1 | `PASS` | `MembatalkanPemeriksaan_MencatatJejakPembatalanDanMenaikkanTokenKonkurensi` |
| Pembatalan ganda | Ditolak | `PASS` | `MembatalkanPemeriksaanDuaKali_Ditolak` |
| Pesanan sudah dibatalkan | Tidak dapat menerima pemeriksaan baru | `PASS` | `PesananYangSudahDibatalkan_TidakDapatMenerimaPemeriksaanBaru` |
| Jalur tidak ditemukan | Keempat jalur menolak dengan `KeyNotFoundException` | `PASS` | `PesananWadahDanPemeriksaanYangTidakAda_Ditolak` |

Uji manual: `NOT FEASIBLE`. Menembak endpoint sungguhan menuntut aplikasi berjalan beserta
databasenya.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Penegakan index unik terhadap data sungguhan | Provider InMemory tidak menegakkan index fisik. Yang diuji di sini adalah pemeriksaan di service; index `IX_LabExamination_SpecimenId_ProcedureId` beserta filternya sudah dibuktikan pada laporan [`BE-LAB-09`](BE-LAB-09.md) |
| Suite `QuilvianSystemBackend.IntegrationTests.Postgres` | Terhalang `QUILVIAN_BILLING_TEST_DB` yang belum diisi |
| Perintah database apa pun | Task ini tidak menyentuh schema |
| Penerbitan fakta pembatalan ke Billing | Cakupan `BE-LAB-13`. Di sini pembatalan pemeriksaan yang sudah layak tagih dicatat pada log beserta penanda `SudahLayakTagih`, supaya sebabnya dapat ditelusuri saat slice itu dikerjakan |

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-35` — satu wadah fisik dapat menopang lebih dari satu pemeriksaan terpesan, dan hanya memiliki satu barcode | **Terpenuhi** | `AC35_SatuWadahMenopangDuaPemeriksaan_KeduanyaTerbacaLewatBySpecimen`. Bersama `BE-LAB-09` yang membuktikan strukturnya, `AC-35` kini terbukti ujung-ke-ujung |
| `AC-36` — menolak sebuah wadah menggugurkan seluruh pemeriksaan yang ditopangnya | **Belum terpenuhi, dan memang bukan milik task ini** | Penolakan wadah beserta pengguguran isinya adalah endpoint `POST /lab-specimens/{id}/reject` milik `BE-LAB-12`. Yang dibuktikan di sini adalah sisi sebaliknya: pembatalan **satu** pemeriksaan tidak menggugurkan yang lain — batas yang menjaga `VAL-13` tetap dapat ditegakkan `BE-LAB-12` nanti |

### 6.2 Definition of Done menurut roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Empat endpoint tersedia dan terdokumentasi Swagger | **Terpenuhi** | `ControllerPemeriksaan_MemakaiBaseRouteYangDikunciKontrak` menghitung tepat empat; `[Tags]` dan `[ProducesResponseType]` terpasang pada seluruhnya |
| `VAL-17` sampai `VAL-20` terbukti | **Terpenuhi** | Keempatnya punya ujinya masing-masing pada bagian 5. Larangan jenis pemeriksaan ganda per wadah — inti `VAL-07` — juga terbukti. Butir DoD ini semula berbunyi "`VAL-05` dan `VAL-07`" dan sudah diselaraskan pada roadmap 2026-09-03; lihat bagian 3.4 butir 1 |
| Pembatalan satu pemeriksaan tidak mengubah status pemeriksaan lain pada wadah yang sama | **Terpenuhi** | `MembatalkanSatuPemeriksaan_TidakMengubahPemeriksaanLainMaupunWadahnya`, yang juga membuktikan status wadahnya ikut tidak berubah |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | `NONE` yang tersisa. Kartu task `BE-LAB-16` yang semula menyebut aturan validasi bukan miliknya sudah diselaraskan pada roadmap 2026-09-03 — lihat bagian 3.4 butir 1 |
| Risiko tersisa | **Sedang.** Sampai `BE-LAB-11` dan `BE-LAB-12` selesai, `TrxLabSpecimen` masih memuat `ProcedureId` dan salinan tarifnya sendiri. Sejak task ini, **kedua jalur itu dapat sama-sama ditulis**: `POST /lab-specimens/by-order` tetap membuat wadah bersalinan tarif, sementara `POST /lab-examinations/by-order` membuat baris pemeriksaan bersalinan tarif pula. Risiko yang pada laporan `BE-LAB-09` disebut "belum aktif" kini **menjadi aktif**. Selama keduanya hidup berdampingan, satu jenis pemeriksaan dapat tercatat dua kali dengan harga yang berbeda |
| Risiko tersisa kedua | Pembatalan pemeriksaan yang sudah `ChargeEligible` belum menerbitkan fakta pembatalan ke Billing; ia hanya tercatat pada log. Penerbitannya milik `BE-LAB-13` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. **`BE-LAB-12`** — endpoint wadah beserta pengguguran isinya, yang menutup `AC-36` dan menghentikan tulis ganda di atas. 2. `BE-LAB-11` masih `BLOCKED` oleh `LAB-OPEN-012`, dan penahannya bukan milik Laboratorium. 3. `BE-LAB-10` — penandaan cito dan duplo per pemeriksaan, dua endpoint sisa pada grup ini |
