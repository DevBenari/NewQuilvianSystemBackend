# Laporan Perubahan Backend — `BE-LAB-12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-12` |
| Judul | Endpoint wadah: rencana, layak, tolak |
| Slice | `S2` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 5, gelombang `MVP-2` |
| Trace | `FR-02.1` .. `FR-02.3`, `FR-02.5`; `LAB-DEC-024`; `AC-36`; `VAL-05` .. `VAL-16`; `CAP-16`; `LAB-STATE-v1` r2 |
| Contract version | `LAB-API-v1` r3 grup Lab Specimen — **breaking**, sebagaimana sudah dinyatakan kontrak sejak awal |
| Dependency | `BE-LAB-09`, `BE-LAB-16`, `BE-LAB-19` — seluruhnya **`SELESAI`**. Lihat bagian 3.4 butir 1 soal `BE-LAB-11` |
| Klasifikasi | `HEAVY` — skor 11. Repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 2, database 1, keamanan/auth 2, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, kontrak dan artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f103fff`, branch `yoga` |
| Tanggal | 2026-09-03 |
| Status | **`SELESAI`** — ketiga endpoint berperilaku baru, `VAL-05` .. `VAL-15` terbukti, checker QBE `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `LabSpecimenService` dan `LabSpecimenController` sudah ada sebelum blueprint ini. Tiga tipe exception baru adalah `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-TXN-001`, `QBE-DEL-001`, `QBE-MOD-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | Seluruh `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*`, `QBE-NAM-003` — tidak ada entity, configuration, maupun migration. `QBE-CODE-*` — alokasi barcode sudah ada dan tidak disentuh |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Setelah `BE-LAB-09` membangun `LabExamination` dan `BE-LAB-16` memberinya endpoint, modul
Laboratorium punya **dua jalur yang tidak saling mengenal**:

> Petugas merencanakan wadah lewat `POST /lab-specimens/by-order`, dan wadah itu terbentuk
> membawa satu `ProcedureId` beserta salinan tarifnya sendiri. Terpisah dari itu, ia bisa
> menambah pemeriksaan lewat `POST /lab-examinations/by-order`, yang membentuk baris pemeriksaan
> dengan salinan tarifnya sendiri pula.
>
> Keduanya menyimpan harga. Tidak ada yang menjamin keduanya sama. Satu jenis pemeriksaan bisa
> tercatat dua kali dengan angka berbeda, dan tidak ada yang tahu mana yang dibaca Billing.

Itu risiko yang laporan `BE-LAB-16` sebut sudah **aktif**.

Masalah kedua lebih menyangkut keselamatan pasien:

> Satu tabung serum menopang Fungsi hati dan Fungsi ginjal. Serumnya keruh. Petugas menolak
> tabung itu — tetapi karena tidak ada kode yang menggugurkan isinya, kedua pemeriksaan tetap
> berdiri seolah masih akan dikerjakan. Daftar kerja laboratorium menampilkan pekerjaan yang
> bahannya sudah tidak ada.

Masalah ketiga adalah **aturan empat mata yang tidak pernah ditegakkan**. `VAL-09` menyatakan
petugas yang mengambil sampel tidak boleh menyatakan kelayakannya sendiri. `CAP-16` sudah
membuktikan sistem permission tidak dapat menegakkannya — `AccessPermissionService.HasAccessAsync`
hanya menjawab boleh atau tidak, dan tidak pernah membandingkan siapa pelaku sebelumnya atas
baris yang sama. Sampai task ini, aturan itu hanya ada di dokumen.

Masalah keempat menyangkut layar: **seluruh pelanggaran aturan menjadi `400`**, sehingga
frontend tidak dapat membedakan permintaan yang cacat bentuk dari permintaan yang melanggar
aturan bisnis, maupun dari tindakan yang di luar kewenangan.

---

## 2. Proses bisnis

### 2.1 Langkah yang berurutan

1. Dokter membuat pesanan laboratorium.
2. Petugas merencanakan **satu wadah** lewat `POST /lab-specimens/by-order/{labOrderId}`, kini
   menyertakan daftar jenis pemeriksaan yang akan dikerjakan dari wadah itu. Satu tabung ungu,
   satu barcode, tiga pemeriksaan.
3. Backend membentuk wadahnya **beserta** baris pemeriksaannya, masing-masing dengan salinan
   tarifnya sendiri — hemoglobin dan leukosit berbeda harga walaupun berasal dari tabung yang
   sama.
4. Sampel diambil. Pelakunya dicatat pada `CollectedByUserId`.
5. Sampel tiba di laboratorium dan dicatat diterima.
6. **Petugas lain** menilai mutunya:
   - **Layak** → wadah `Accepted`, dan seluruh pemeriksaan di atasnya menjadi `ChargeEligible`
     dengan waktu keputusan yang sama.
   - **Tidak layak** → wadah `Rejected`, dan seluruh pemeriksaan di atasnya `Voided`.
7. Bila perlu diambil ulang, wadah pengganti dibentuk dengan barcode baru; wadah lama berpindah
   ke `RecollectionRequired` dan tetap menjadi asal-usulnya.

### 2.2 Contoh berangka — `AC-36`

Satu tabung serum menopang dua pemeriksaan:

| Langkah | Wadah | Fungsi hati | Fungsi ginjal |
| --- | --- | --- | --- |
| Direncanakan | `Planned` | `Ordered` | `Ordered` |
| Diambil, tiba di lab | `Received` | `Ordered` | `Ordered` |
| **Ditolak** — serum keruh | `Rejected` | **`Voided`** | **`Voided`** |

Keduanya gugur, bukan satu saja. Bila bahannya tidak layak, tidak ada satu pun di antaranya yang
dapat dikerjakan.

### 2.3 Jalur tidak normal

| Keadaan | Kode | Aturan |
| --- | :---: | --- |
| Merencanakan wadah tanpa satu pun pemeriksaan | `422` | `VAL-05` |
| Merencanakan wadah pada pesanan yang sudah dibatalkan | `409` | `VAL-06` |
| Jenis pemeriksaan yang sama disertakan dua kali | `422` | `VAL-07` |
| Menyatakan layak wadah yang belum tercatat tiba | `409` | `VAL-08` |
| **Petugas yang mengambil sampel menyatakan kelayakannya sendiri** | `403` | `VAL-09` |
| Menolak tanpa memilih alasan | `422` | `VAL-10` |
| Alasan penolakan tidak dikenal atau sudah nonaktif | `422` | `VAL-11` |
| Alasan menuntut catatan, tetapi catatan kosong | `422` | `VAL-12` |
| Menolak sebagian pemeriksaan saja | — | `VAL-13` — **tidak ada jalurnya**; lihat bagian 3.4 butir 3 |
| Meminta ambil ulang tanpa sebab | `422` | `VAL-14` |
| Sebab selain kesalahan internal, tetapi alasan kosong | `422` | `VAL-15` |
| Wadah sedang diubah petugas lain | `409` | `VAL-16` |

### 2.4 Mengapa `VAL-09` ditulis sebagai kode, bukan permission

Yang dijaga adalah penilaian mutu bahan. Orang yang mengambil sampel sudah punya kepentingan
pada hasilnya dinyatakan layak — pengambilan ulang berarti pekerjaannya diulang, dan pada
sebagian sebab, pasien ditusuk dua kali. Bila ia juga yang menilai, tidak ada mata kedua yang
memeriksa pekerjaannya.

Aturan itu **tidak dapat** dinyatakan sebagai permission, karena permission hanya mengenal
"boleh" dan "tidak boleh" atas sebuah aksi — bukan "boleh, kecuali atas baris yang kamu sendiri
sentuh sebelumnya". Karena itu ia ditulis di dalam service, dan diuji tersendiri.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/backend-roadmap.md` bagian 5 dan 8.2
- `contracts/validation-matrix.md` bagian 2
- `contracts/api-contract.md` grup Lab Specimen bagian 1 dan 2
- `contracts/state-transition-matrix.md`
- `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs`
- `Areas/HealthServices/LaboratoryManagement/Services/LabExaminationService.cs` — pola yang dipakai ulang
- `Areas/HealthServices/LaboratoryManagement/Controllers/LabSpecimenController.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../DTOs/LabSpecimenDtos.cs` | `PlanLabSpecimenRequest` bertambah `Examinations`; `ProcedureId` dipertahankan sebagai jalur ringkas satu pemeriksaan bagi pemanggil lama |
| `.../Services/LabSpecimenService.cs` | `PlanAsync` membentuk wadah **beserta** pemeriksaannya; `AcceptAsync` dan `RejectAsync` memindahkan seluruh pemeriksaan; `VAL-09` ditegakkan; sepuluh aturan validasi memakai tipe galat yang benar; dua method pembantu baru — `CreateExaminationsAsync` dan `MoveExaminationsAsync`; tiga tipe exception baru |
| `.../Controllers/LabSpecimenController.cs` | Ketiga tipe galat dipetakan ke `403`, `409`, dan `422`, ditangkap **sebelum** `ArgumentException` yang lebih umum. `[ProducesResponseType]` dilengkapi pada empat endpoint |
| `Tests/.../LabSpecimenDecisionTests.cs` | **Baru.** 16 uji |
| `contracts/api-contract.md` | Ketiga endpoint berpindah dari **Rencana** menjadi **Tersedia** |
| `roadmap/backend-roadmap.md`, `roadmap/traceability.md` | Status dan bukti diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Ketiga endpoint berperilaku baru**, sebagaimana sudah dinyatakan `breaking` oleh kontrak sejak awal. `PlanLabSpecimenRequest` bertambah `Examinations`; kode status berubah dari `400` yang seragam menjadi `422`, `409`, dan `403` sesuai matriks. **Ruas lama tidak dihapus**: `ProcedureId` tetap diterima, sehingga pemanggil yang mengirim satu pemeriksaan tidak putus |
| Database | **Tidak ada dampak schema.** Tidak ada entity, kolom, index, maupun migration. Yang berubah adalah baris mana yang ditulis: `PlanAsync` kini juga menulis ke `LabExamination` |
| Keamanan/Auth | **Menguat.** `VAL-09` menambahkan penjaga yang sebelumnya tidak ada sama sekali: petugas yang mengambil sampel tidak dapat menyatakan kelayakannya sendiri. Hak akses tidak berubah — yang bertambah adalah pemeriksaan **di dalam** aksi yang hak aksesnya sudah dimiliki |

### 3.4 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Urutan terhadap `BE-LAB-11` dibalik** | Kartu task menyebut dependency `BE-LAB-11`, yang menghapus enam kolom dari `LabSpecimen`. Audit menunjukkan keenamnya **masih dipakai** `LabSpecimenService` di delapan tempat, termasuk muatan fakta tagihan ke Billing. Menghapusnya lebih dulu akan mematahkan build sekaligus jalur tagihan. Karena itu `BE-LAB-12` dikerjakan lebih dulu supaya kode berhenti memakainya, dan `BE-LAB-11` menyusul menghapus kolom yang sudah mati. Setiap langkah tetap dapat dibangun dan diuji sendiri — urutan sebaliknya tidak bisa |
| 2 | Kolom peninggalan pada wadah **masih diisi** | Selama `BE-LAB-11` belum jalan, `ProcedureId` pada `LabSpecimen` masih wajib. `PlanAsync` mengisinya dari pemeriksaan **pertama** pada daftar, sebagai jembatan sementara. Nilainya tidak lagi dibaca sebagai kebenaran; yang otoritatif adalah baris `LabExamination` |
| 3 | `VAL-13` ditegakkan **secara struktural**, bukan lewat pemeriksaan runtime | Tidak ada satu pun jalur yang menerima daftar pemeriksaan untuk ditolak sebagian: `RejectLabSpecimenRequest` tidak punya ruasnya, dan controller wadah hanya punya satu endpoint penolakan yang menyasar wadah. Penolakan yang tidak dapat dinyatakan lebih kuat daripada penolakan yang ditolak — dan keduanya diuji |
| 4 | Penerbitan fakta **belum** per pemeriksaan | `AcceptAsync` masih menerbitkan satu fakta per wadah. Memecahnya menjadi satu fakta per pemeriksaan adalah cakupan `BE-LAB-13` (`FR-05.1` .. `FR-05.4`), dan menyentuhnya dari sini akan mengambil pekerjaan task lain |
| 5 | Pemeriksaan yang sudah dibatalkan **tidak** tertimpa | `MoveExaminationsAsync` melewati baris berstatus `Cancelled`. Pembatalan satu pemeriksaan adalah keputusan klinis tersendiri yang tidak boleh terhapus oleh keputusan atas wadah |
| 6 | Kode status pada endpoint di luar ketiganya **tidak** diubah | `collect`, `receive`, `hold`, `resume`, dan `cancel` tetap mengembalikan `400` untuk galat argumen. Matriks validasi tidak menetapkan kode khusus bagi keduanya, dan mengubahnya akan memperluas dampak breaking tanpa dasar |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/by-order/{labOrderId}` | Merencanakan **satu wadah** beserta pemeriksaan yang ditopangnya | `LabSpecimen : Plan` |
| `POST` | `/{id}/accept` | Menyatakan wadah layak; seluruh pemeriksaan di atasnya menjadi layak tagih | `LabSpecimen : Accept` |
| `POST` | `/{id}/reject` | Menolak wadah; seluruh pemeriksaan di atasnya gugur | `LabSpecimen : Accept` |

`PlanLabSpecimenRequest` kini memuat `Examinations` — daftar `ProcedureId` yang akan dikerjakan
dari wadah itu — beserta `ProcedureId` tunggal dan `SpecimenDescription`.

Kode status: `200`/`201` berhasil; `400` isian cacat bentuk; `403` `VAL-09`; `404` tidak
ditemukan; `409` `VAL-06`, `VAL-08`, `VAL-16`; `422` `VAL-05`, `VAL-07`, `VAL-10` .. `VAL-12`,
`VAL-14`, `VAL-15`.

Sembilan endpoint wadah lainnya **tidak berubah perilakunya**.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test ...QuilvianSystemBackend.Tests --filter LabSpecimenDecision` | `Failed: 0, Passed: 16, Total: 16` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` seluruhnya | `Failed: 0, Passed: 200, Total: 200` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, terbuka sejak sebelum seluruh pekerjaan Laboratorium |
| Checker QBE atas 39 berkas modul | `VIOLATION: 0`, `Final result: PASS` | `PASS` | Keluaran perintah, mode `ExplicitFiles` |
| Merencanakan wadah dengan dua pemeriksaan | Satu wadah, dua baris pemeriksaan, masing-masing bertarif sendiri | `PASS` | `MerencanakanWadah_DenganDuaPemeriksaan_MenghasilkanSatuWadahDanDuaBaris` |
| Jalur ringkas satu pemeriksaan | Pemanggil lama yang mengirim `ProcedureId` tetap berhasil | `PASS` | `MerencanakanWadah_JalurRingkasSatuPemeriksaanTetapBerlaku` |
| **`VAL-05`** — wadah tanpa pemeriksaan | Ditolak beserta pesan kontrak | `PASS` | `VAL05_WadahTanpaSatuPunPemeriksaan_Ditolak` |
| **`VAL-06`** — pesanan sudah dibatalkan | Ditolak `LabSpecimenConflictException` | `PASS` | `VAL06_PesananYangSudahDibatalkan_TidakDapatMenerimaWadahBaru` |
| **`VAL-07`** — jenis sama dua kali | Ditolak; nol wadah tersimpan | `PASS` | `VAL07_JenisPemeriksaanYangSamaDuaKali_Ditolak` |
| **`AC-36`** — menolak wadah menggugurkan seluruh isinya | Kedua pemeriksaan `Voided`, tidak ada yang layak tagih | `PASS` | `AC36_MenolakWadah_MenggugurkanSeluruhPemeriksaanYangDitopangnya` |
| **`VAL-13`** — tidak ada jalur menolak sebagian | `RejectLabSpecimenRequest` terbukti tanpa ruas daftar; controller terbukti hanya punya satu jalur penolakan pengubah | `PASS` | `VAL13_TidakAdaJalurYangMenolakSebagianPemeriksaan` |
| Pemeriksaan yang sudah dibatalkan sendiri | Tetap `Cancelled`, tidak tertimpa menjadi `Voided` | `PASS` | `PemeriksaanYangSudahDibatalkanSendiri_TidakTertimpaKeputusanWadah` |
| **`VAL-08`** — belum tercatat tiba | Ditolak beserta pesan kontrak | `PASS` | `VAL08_WadahYangBelumDiterima_TidakDapatDinyatakanLayak` |
| **`VAL-09`** — aturan empat mata | Pengambil sampel ditolak `403`; **keadaan tidak bergeser sedikit pun** — wadah tetap `Received`, seluruh pemeriksaan tetap `Ordered` | `PASS` | `VAL09_PetugasYangMengambilSampel_TidakBolehMenyatakanKelayakannya` |
| **`VAL-10`** .. **`VAL-12`** | Alasan kosong, alasan tak dikenal, dan catatan wajib yang kosong — ketiganya ditolak beserta pesan kontraknya | `PASS` | Tiga uji tersendiri |
| **`VAL-14`**, **`VAL-15`** | Sebab kosong dan sebab non-internal tanpa alasan — keduanya ditolak | `PASS` | Dua uji tersendiri |
| Pemetaan kode status pada controller | Ketiga tipe galat terpetakan, dan ditangkap **sebelum** `ArgumentException` | `PASS` | `ControllerWadah_MemetakanKetigaTipeGalatKeKodeYangBenar` |

Uji manual: `NOT FEASIBLE`. Menembak endpoint sungguhan menuntut aplikasi berjalan beserta
databasenya.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Jalur penetapan layak sampai penerbitan fakta | Penerbitannya menyentuh Billing dan menjadi cakupan `BE-LAB-13`. Yang diuji di sini adalah penjaga yang berjalan **sebelum** penerbitan — `VAL-08` dan `VAL-09` — beserta perpindahan status pemeriksaan. Bukti runtime penerbitannya ada pada suite integrasi Postgres |
| Suite `QuilvianSystemBackend.IntegrationTests.Postgres` | Terhalang `QUILVIAN_BILLING_TEST_DB` yang belum diisi |
| **`VAL-16`** — konkurensi | Sudah ditegakkan `SaveWithConcurrencyGuardAsync` sejak sebelum task ini dan tidak disentuh. Provider InMemory tidak menegakkan token konkurensi, sehingga pengujiannya menuntut database sungguhan |
| Perintah database apa pun | Task ini tidak menyentuh schema |

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-35` — satu wadah menopang lebih dari satu pemeriksaan, satu barcode | **Terpenuhi** | `MerencanakanWadah_DenganDuaPemeriksaan_...`; kini juga lewat jalur perencanaan wadah, melengkapi `BE-LAB-09` dan `BE-LAB-16` |
| `AC-36` — menolak wadah menggugurkan seluruh pemeriksaan; menolak sebagian ditolak sistem | **Terpenuhi** | `AC36_MenolakWadah_...` untuk paruh pertama; `VAL13_TidakAdaJalurYangMenolakSebagianPemeriksaan` untuk paruh kedua |
| `AC-37` — kelayakan tagih terbit per pemeriksaan | **Terpenuhi pada tingkat status, belum pada penerbitan fakta** | Seluruh pemeriksaan berpindah ke `ChargeEligible` dengan waktu keputusan yang sama. Penerbitan satu fakta per pemeriksaan adalah cakupan `BE-LAB-13` |
| `AC-38` — pengambilan ulang | **Terpenuhi sebagian** | `VAL-14` dan `VAL-15` terbukti. Bagian "wadah baru menampung seluruh pemeriksaan wadah lama" **belum**: wadah pengganti masih dibentuk dari satu procedure. Lihat bagian 7 |

### 6.2 Definition of Done menurut roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Tiga endpoint berperilaku baru | **Terpenuhi** | Bagian 4 dan bagian 5 |
| `VAL-05`, `VAL-07`, `VAL-08`, `VAL-13`, `VAL-14` terbukti | **Terpenuhi** | Kelimanya punya ujinya, ditambah `VAL-06`, `VAL-09` .. `VAL-12`, dan `VAL-15` |
| Sembilan endpoint lain tidak berubah perilakunya | **Terpenuhi** | Hanya `PlanAsync`, `AcceptAsync`, `RejectAsync`, dan `RequestRecollectionAsync` disentuh; `RequestRecollection` hanya pada tipe galat `VAL-14`/`VAL-15` yang memang miliknya. Suite penuh 200 lulus tanpa regresi |
| Dampak breaking tercatat pada `contracts/api-contract.md` | **Terpenuhi** | Ketiga baris ditandai **Tersedia** beserta catatan bahwa penerbitan fakta menunggu `BE-LAB-13`; bagian 3 kontrak sudah menyatakan grup ini breaking sejak awal |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | `AC-38` baru terpenuhi sebagian: wadah pengganti pada pengambilan ulang masih dibentuk dari satu procedure, belum menyalin seluruh pemeriksaan wadah lama. Kartu `BE-LAB-12` menyebutnya pada Verifikasi. **Ini sisa pekerjaan yang belum tertutup** dan perlu diputuskan pemilik modul: apakah masuk `BE-LAB-11` yang menyusul, atau task tersendiri |
| Risiko tersisa | **Sedang, dan menurun dari sebelumnya.** Tulis ganda salinan tarif belum hilang: `PlanAsync` masih mengisi kolom peninggalan pada wadah sebagai jembatan sampai `BE-LAB-11` menghapusnya. Bedanya, kini keduanya diisi dari sumber yang sama dalam satu transaksi, sehingga tidak dapat berbeda — sebelumnya keduanya bisa diisi jalur berbeda dengan angka berbeda |
| Risiko tersisa kedua | `VAL-16` tidak diuji di sini karena provider InMemory tidak menegakkan token konkurensi. Penjaganya sudah ada sejak sebelum task ini dan tidak disentuh |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. **`BE-LAB-11`** — menghapus keenam kolom peninggalan; kini aman karena kode sudah berhenti membacanya sebagai kebenaran. 2. Memutuskan pemilik sisa `AC-38`. 3. `BE-LAB-13` — fakta kelayakan tagih per pemeriksaan. 4. `BE-LAB-10` — penandaan cito dan duplo |
