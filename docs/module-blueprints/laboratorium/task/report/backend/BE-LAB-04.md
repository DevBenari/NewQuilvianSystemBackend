# Laporan Perubahan Backend — `BE-LAB-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-04` |
| Judul | Endpoint pengelolaan batas nilai |
| Slice | `S3` — pengelolaan batas nilai (`roadmap/backend-roadmap.md` bagian 3, gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 3 |
| Trace | `FR-03.1` .. `FR-03.3`, `FR-03.5`; `LAB-DEC-018`, `LAB-DEC-021`, `LAB-DEC-023`; `LAB-VAL-v1` r3 `VAL-21` .. `VAL-30`; `LAB-API-v1` r3 grup Lab Value Bound |
| Contract version | `LAB-API-v1` r3 dan `LAB-VAL-v1` r3 — `approved`, dikunci 2026-09-02 |
| Dependency | `BE-LAB-02` dan `BE-LAB-03`, keduanya **`SELESAI`** 2026-09-02 |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 1, database 1, keamanan 1, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/{DTOs,Services,Controllers}`, `Program.cs`, `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/`, dan `docs/module-blueprints/laboratorium/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `d8d67c3`, branch `yoga`, 2026-09-02 |
| Tanggal | 2026-09-02 |
| Status | **Selesai.** Enam endpoint tersedia, seluruh jalur gagal yang diwajibkan terbukti, dan tidak ada perubahan schema sehingga tidak ada migration. Seluruh butir DoD terpenuhi |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-LAB-02` dan `BE-LAB-03`, tiga tabel batas nilai sudah ada — tetapi **kosong dan tidak
punya satu pun jalur pengisian**. Kepala instalasi laboratorium tidak dapat memasukkan satu baris
batas pun kecuali lewat perintah database langsung.

Akibatnya bukan sekadar merepotkan. Selama tabel itu kosong, tidak ada satu pun hasil pemeriksaan
yang dapat dinilai: tidak ada yang tahu Kalium 7,2 mmol/L itu kritis, dan tidak ada yang tahu
Hemoglobin 6,8 g/dL pada seorang anak berada jauh di bawah batas.

Ada pula lubang yang lebih halus. `LAB-DEC-023` menetapkan batas kritis hanya boleh berubah lewat
persetujuan klinis, dan `BE-LAB-03` sudah menyediakan tabel pengajuannya. Tetapi aturan itu belum
ditegakkan di mana pun: begitu endpoint ubah dibuat tanpa penjagaan, kepala instalasi dapat
menaikkan batas kritis atas Kalium dari 6,0 menjadi 8,0 lewat jalur ubah biasa, dan sejak saat itu
pasien dengan Kalium 7,2 mmol/L tidak lagi memicu kewajiban pelaporan — tanpa satu pun aturan
dilanggar dan tanpa ada yang menyadarinya.

Task ini menutup keduanya: enam endpoint pengelolaan, dan `VAL-28` sebagai pengaman yang menolak
setiap upaya mengubah batas kritis lewat jalur biasa.

---

## 2. Proses bisnis

**Tujuan.** Kepala instalasi laboratorium dapat mengelola batas nilai rujukan sendiri, tanpa
menerbitkan versi aplikasi baru, sementara angka yang menentukan keselamatan pasien tetap
terlindungi.

**Pelaku.** Kepala instalasi laboratorium, pemegang `LabValueBound : Read`, `: Create`, dan
`: Update`.

**Pemicu.** Rumah sakit menetapkan atau meninjau ulang nilai rujukan sebuah pemeriksaan.

**Langkah yang berurutan:**

1. Petugas membuka daftar batas nilai, menyaringnya per jenis pemeriksaan, atau mencarinya lewat
   kode dan nama pemeriksaan.
2. Untuk pemeriksaan baru, ia membuat satu baris batas: memilih jenis pemeriksaan, bentuk hasil,
   kelompok pasien (jenis kelamin dan kelompok umur), lalu mengisi isinya.
3. Sistem memeriksa isian itu masuk akal sebelum menyimpannya — lihat tabel aturan di bawah.
4. Untuk mengubah, ia mengirim satuan, batas normal, batas waktu cito, dan daftar pilihan yang
   baru. Perubahan **langsung berlaku**, dan setiap kolom yang berubah menerbitkan satu baris
   riwayat.
5. Bila permintaan ubah itu memuat perubahan batas kritis, seluruh permintaan **ditolak** —
   bukan sebagian dijalankan. Perubahan itu harus lewat pengajuan (`BE-LAB-05`).
6. Batas yang tidak dipakai lagi dinonaktifkan, kecuali ia satu-satunya yang aktif untuk
   pemeriksaan itu.
7. Riwayat perubahan sebuah batas dapat ditelusuri kapan saja, terbaru lebih dulu.

**Aturan yang berlaku:**

| Aturan | Kapan | Isi | Kode |
| --- | --- | --- | --- |
| `VAL-21` | Membuat | Kombinasi pemeriksaan, jenis kelamin, dan kelompok umur sudah ada | `409` |
| `VAL-22` | Membuat/mengubah | Bentuk angka wajib punya satuan | `422` |
| `VAL-23` | Membuat/mengubah | Bentuk pilihan wajib punya sekurang-kurangnya satu pilihan | `422` |
| `VAL-24` | Membuat/mengubah | Bentuk angka tidak boleh punya daftar pilihan | `422` |
| `VAL-25` | Membuat/mengubah | Batas normal bawah tidak boleh melebihi batas atas | `422` |
| `VAL-26` | Membuat/mengubah | Batas kritis bawah harus di bawah batas normal bawah | `422` |
| `VAL-27` | Membuat/mengubah | Batas kritis atas harus di atas batas normal atas | `422` |
| **`VAL-28`** | **Mengubah** | **Batas kritis tidak boleh berubah lewat jalur ubah biasa** | **`422`** |
| `VAL-29` | Membuat/mengubah | Batas waktu cito harus lebih dari nol menit | `422` |
| `VAL-30` | Menonaktifkan | Batas aktif terakhir sebuah pemeriksaan tidak boleh dinonaktifkan | `422` |

**Contoh berangka `VAL-26` dan `VAL-27`.** Kalium normal 3,5–5,1 mmol/L. Batas kritis wajib
berada **di luar** rentang itu — misalnya 2,5 dan 6,0. Batas kritis atas 4,0 ditolak, karena
angka 4,5 yang masih normal akan ikut terhitung kritis dan peringatan menjadi tidak berarti.

**Status yang dihasilkan.** Tidak ada status workflow. Yang berubah hanya penanda aktif sebuah
batas nilai.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Permintaan ubah memuat batas kritis yang berbeda | **Ditolak seluruhnya** `422`. Batas lama tetap berlaku dan tidak ada riwayat yang terbit |
| Permintaan ubah menurunkan penanda kritis sebuah pilihan | Ditolak `422` — mengubah "+3" dari kritis menjadi tidak kritis adalah perubahan batas kritis, walaupun tidak menyentuh satu pun angka |
| Permintaan ubah membuang pilihan yang bertanda kritis | Ditolak `422` — membuang "+4" dari daftar sama dengan mencabut batas kritis |
| Permintaan ubah mengirim batas kritis yang **sama** dengan yang berlaku | Diterima. Yang dilarang adalah perubahannya, bukan menyebutkannya |
| Dua petugas membuat batas untuk kelompok pasien yang sama bersamaan | Salah satu ditolak `409` oleh index unik database, walaupun keduanya lolos pemeriksaan awal |
| Batas nilai tidak ditemukan | `404` pada ubah, nonaktifkan, dan riwayat; badan kosong pada detail |

**Hasil akhir.** Batas nilai dapat dikelola sepenuhnya lewat aplikasi, dan batas kritis tetap
hanya dapat berubah lewat persetujuan klinis.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola:** `AGENTS.md`; `CLAUDE.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `tooling/qbe/Invoke-QbeConformanceCheck.ps1`;
`rules/backend/API_RULES.md`; `rules/backend/TASK_RULES.md`; `rules/backend/TASK_CLASSIFICATION.md`;
`rules/backend/REPORT_TEMPLATE.md`

**Blueprint:** `roadmap/backend-roadmap.md` bagian 3 dan 7; `roadmap/traceability.md`;
`contracts/api-contract.md` grup Lab Value Bound; `contracts/validation-matrix.md` `VAL-21` .. `VAL-30`;
`contracts/state-transition-matrix.md` bagian 4; `00-interview-decisions.md` (BR-14, BR-17, BR-19,
`AC-24`, `AC-28`, `AC-33`, `AC-34`); `erd/data-dictionary.md` bagian 5 sampai 8

**Source:** `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs`;
`.../Services/LabOrderService.cs`; `.../DTOs/LabOrderDtos.cs`; `.../Models/LabValueBound.cs`;
`.../Models/LabValueOption.cs`; `.../Models/LabValueBoundHistory.cs`;
`.../Models/LabValueBoundChangeRequest.cs`; `Responses/ApiResponse.cs`; `Responses/PagedResult.cs`;
`Attributes/AccessPermissionAttribute.cs`; `Constants/AccessTypes.cs`; `Program.cs`;
`Areas/HealthServices/BillingManagement/Billing/Controllers/BillingFinalizationsController.cs`
(pola pemetaan `422`); `.../Services/BillingDiscountService.cs` (pola paging)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabValueBoundDtos.cs` | **Baru.** Delapan DTO: query berpaging, list, detail, pilihan, permintaan buat, permintaan ubah, permintaan pilihan, dan riwayat |
| `Areas/HealthServices/LaboratoryManagement/Services/LabValueBoundService.cs` | **Baru.** Seluruh logika: baca, buat, ubah, nonaktifkan, riwayat, sepuluh validasi, penerbitan riwayat per kolom, dan dua exception yang dipetakan menjadi `422` dan `409` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabValueBoundController.cs` | **Baru.** Enam endpoint beserta `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, `[Tags]`, dan `[ProducesResponseType]` |
| `Program.cs` | Mendaftarkan `LabValueBoundService` sebagai `Scoped`, tepat satu baris di antara service Laboratorium lainnya |
| `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabValueBoundServiceTests.cs` | **Baru.** 28 kasus uji: jalur berhasil, sepuluh jalur gagal, dan kontrak keenam endpoint |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Enam endpoint baru**, persis seperti `LAB-API-v1` r3 grup Lab Value Bound — route, verb, hak akses, dan bentuk response-nya cocok satu per satu. Tidak ada endpoint lama yang disentuh, diganti nama, atau berubah perilakunya |
| Database | `NOT APPLICABLE` untuk schema — task ini tidak menambah, mengubah, maupun menghapus satu pun kolom atau tabel, sehingga **tidak ada migration** dan tidak ada perintah database yang dijalankan. Yang bertambah hanya pemakaian tabel yang sudah dibuat `BE-LAB-02` dan `BE-LAB-03` |
| Keamanan/Auth | Tiga hak akses dipakai: `LabValueBound : Read`, `: Create`, dan `: Update`, seluruhnya lewat `[AccessPermission]` sehingga terdaftar sendiri mengikuti `CAP-14`. Grup ini **tidak** menyediakan satu pun jalur hapus — batas nilai hanya dinonaktifkan, supaya riwayat yang menunjuk kepadanya tidak pernah menggantung |

**Catatan cakupan validasi.** Roadmap mewajibkan empat jalur gagal dibuktikan: `VAL-22`,
`VAL-23`, `VAL-24`, dan `VAL-28`. Yang diimplementasikan sepuluh — `VAL-21` sampai `VAL-30` —
karena `contracts/validation-matrix.md` menempelkan seluruhnya pada tindakan "membuat",
"mengubah", dan "menonaktifkan" batas nilai, dan ketiganya adalah persis endpoint task ini.
Menerbitkan endpoint yang diam-diam menerima batas kritis di dalam rentang normal berarti
mengirimkan lubang keselamatan yang sudah diketahui.

**Delta terhadap kontrak yang perlu diketahui.** `LAB-API-v1` r3 tidak merinci isi
`UpdateLabValueBoundRequest`. Ruas `CriticalLow` dan `CriticalHigh` tetap disertakan di sana,
padahal keduanya tidak dapat diubah lewat endpoint itu. Alasannya: tanpa ruas itu, permintaan
yang membawa niat mengubah batas kritis akan **diabaikan diam-diam**, dan pemanggil mengira
perubahannya tersimpan. Dengan ruasnya ada, niat itu terbaca dan ditolak terbuka lewat `VAL-28`.
Menolak lebih jujur daripada mengabaikan.

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Value Bound

Base URL: `api/v1/health-services/laboratory-management/lab-value-bounds`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar batas nilai berpaging, dapat disaring per jenis pemeriksaan, status aktif, dan pencarian kode/nama pemeriksaan | `LabValueBound : Read` |
| `GET` | `/{id}` | Detail satu batas nilai beserta daftar pilihannya, dan penanda apakah ada pengajuan perubahan batas kritis yang belum diputuskan | `LabValueBound : Read` |
| `POST` | `/` | Membuat batas nilai baru untuk satu kelompok pasien | `LabValueBound : Create` |
| `PUT` | `/{id}` | Mengubah satuan, batas normal, batas waktu cito, dan daftar pilihan. **Batas kritis ditolak** `VAL-28` | `LabValueBound : Update` |
| `PUT` | `/{id}/deactivate` | Menonaktifkan batas nilai | `LabValueBound : Update` |
| `GET` | `/{id}/history` | Riwayat perubahan, terbaru lebih dulu | `LabValueBound : Read` |

Kode status yang diterbitkan: `200`, `201`, `400`, `404`, `409`, dan `422`. Tidak ada `DELETE`
pada grup ini.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil | `PASS` | `0 Error(s)` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | Lolos | `PASS` | `Files evaluated: 24`, `VIOLATION: 0`, `Final result: PASS` |
| Membuat batas berbentuk angka | Tersimpan beserta satuan dan keempat batasnya | `PASS` | `MembuatBatasAngka_TersimpanBesertaSatuanDanBatasnya` |
| Membuat batas berbentuk pilihan | Tersimpan beserta lima pilihan dan penanda kritisnya | `PASS` | `MembuatBatasPilihan_TersimpanBesertaDaftarPilihannya` |
| `AC-24` — tiga baris Hemoglobin untuk satu pemeriksaan | Ketiganya tersimpan dan terbaca lewat daftar berpaging | `PASS` | `AC24_TigaBarisBatasHemoglobin_DapatDibuatUntukSatuPemeriksaan` |
| `AC-33` bagian "langsung berlaku" — mengubah batas normal | Berlaku seketika, dan menerbitkan riwayat | `PASS` | `MengubahBatasNormal_LangsungBerlakuDanMenerbitkanRiwayat` |
| `AC-34` — riwayat memuat nilai lama, nilai baru, pelaku, alasan, dan penyetuju kosong | Sesuai harapan | `PASS` | Uji yang sama; baris `NormalHigh` diperiksa satu per satu |
| Menonaktifkan batas saat masih ada batas lain yang aktif | Berhasil dan menerbitkan riwayat `IsActive` | `PASS` | `MenonaktifkanBatas_BerhasilBilaMasihAdaBatasLainYangAktif` |
| **Gagal `VAL-22`** — batas angka tanpa satuan | Ditolak, nol baris tersimpan | `PASS` | `VAL22_BatasAngkaTanpaSatuan_Ditolak` |
| **Gagal `VAL-23`** — batas pilihan tanpa satu pun pilihan | Ditolak, nol baris tersimpan | `PASS` | `VAL23_BatasPilihanTanpaSatuPunPilihan_Ditolak` |
| **Gagal `VAL-24`** — batas angka disertai daftar pilihan | Ditolak, nol baris tersimpan | `PASS` | `VAL24_BatasAngkaDisertaiDaftarPilihan_Ditolak` |
| **Gagal `VAL-28`** — mengubah batas kritis lewat `PUT` biasa | Ditolak; batas lama tetap 6,1 dan **nol** baris riwayat terbit | `PASS` | `VAL28_MengubahBatasKritisLewatPutBiasa_Ditolak` |
| **Gagal `VAL-28`** — menurunkan penanda kritis sebuah pilihan | Ditolak; penanda lama tidak berubah | `PASS` | `VAL28_MengubahPenandaPilihanKritisLewatPutBiasa_Ditolak` |
| **Gagal `VAL-28`** — membuang pilihan yang bertanda kritis | Ditolak; kelima pilihan tetap utuh | `PASS` | `VAL28_MenghapusPilihanKritisLewatPutBiasa_Ditolak` |
| Mengubah daftar pilihan tanpa menyentuh penanda kritis | Diterima | `PASS` | `MengubahDaftarPilihanTanpaMenyentuhPenandaKritis_Diterima` — membuktikan `VAL-28` tidak kebablasan memblokir perubahan yang sah |
| **Gagal `VAL-21`** — kombinasi kelompok pasien yang sudah ada | Ditolak; tetap satu baris | `PASS` | `VAL21_KombinasiKelompokPasienYangSudahAda_Ditolak` |
| **Gagal `VAL-21`** — dua baris "semua umur" | Ditolak | `PASS` | `VAL21_KelompokUmurKosong_JugaDijagaSebagaiSatuKelompok` |
| **Gagal `VAL-25`, `VAL-26`, `VAL-27`** — batas yang tidak masuk akal | Ketiganya ditolak dengan pesan masing-masing | `PASS` | `VAL25SampaiVAL27_BatasYangTidakMasukAkal_Ditolak`, tiga `[InlineData]` |
| **Gagal `VAL-29`** — batas waktu cito nol | Ditolak | `PASS` | `VAL29_BatasWaktuCitoNolAtauNegatif_Ditolak` |
| **Gagal `VAL-30`** — menonaktifkan batas aktif terakhir | Ditolak; batasnya tetap aktif | `PASS` | `VAL30_MenonaktifkanBatasAktifTerakhir_Ditolak` |
| **Gagal** — membuat batas untuk procedure bukan laboratorium | Ditolak | `PASS` | `MembuatBatasUntukProcedureBukanLaboratorium_Ditolak` |
| **Gagal** — batas nilai tidak ditemukan pada ubah, nonaktifkan, dan riwayat | Ketiganya `KeyNotFoundException` → `404` | `PASS` | `MembacaBatasNilaiYangTidakAda_MenghasilkanKosongDanRiwayatnyaDitolak` |
| Kontrak keenam endpoint: route, verb, dan `[AccessPermission]` | Sesuai kontrak | `PASS` | `KeenamEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak`, enam `[InlineData]` |
| Base route dan jumlah endpoint | Enam, tanpa satu pun `DELETE` | `PASS` | `ControllerBatasNilai_MemakaiBaseRouteYangDikunciKontrak` |
| Seluruh test `LabValueBoundServiceTests` | Hijau | `PASS` | `Failed: 0, Passed: 28, Skipped: 0, Total: 28` |
| Seluruh suite `QuilvianSystemBackend.Tests` | 893 lulus, 1 gagal | `EXISTING / ENVIRONMENT ISSUE` | `Failed: 1, Passed: 893, Total: 894`. Satu-satunya kegagalan adalah `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` milik modul Billing, terbuka sejak sebelum task ini |
| Migration | Tidak ada | `NOT APPLICABLE` | Task ini tidak menyentuh schema; `dotnet ef` tidak dijalankan sama sekali |
| Uji lewat HTTP sungguhan beserta `[Authorize]` | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan. Yang diperiksa lewat reflection adalah atribut route dan permission yang terpasang, bukan perilaku filter saat runtime |

Uji manual lewat antarmuka: `NOT FEASIBLE` — memerlukan aplikasi berjalan; layar pengelolaannya
adalah pekerjaan frontend `FE-LAB-02`.

**Tidak dijalankan:**

- **`VAL-31` sampai `VAL-33`.** Ketiganya melekat pada endpoint pengajuan dan persetujuan batas
  kritis, yang merupakan cakupan `BE-LAB-05`.
- **Pemeriksaan filter authorization saat runtime.** `AccessPermissionFilter` berjalan di dalam
  pipeline MVC; membuktikannya memerlukan host HTTP. Yang dibuktikan di sini adalah atributnya
  terpasang dengan resource dan action yang benar pada keenam endpoint.
- **Perintah database apa pun.** Task ini tidak menyentuh schema.

**Catatan kejujuran atas satu koreksi.** Tiga test sempat merah pada putaran pertama. Dua di
antaranya adalah kesalahan **data uji saya sendiri**: helper batas angka memakai rentang normal
13–17 tetapi batas kritis atas 6,0, yang justru melanggar `VAL-27` yang sedang diuji di tempat
lain. Validasinya bekerja benar; datanya yang salah, dan helper itu diperbaiki agar batas
kritisnya diturunkan dari rentang normalnya. Satu lagi adalah **bug nyata pada source**:
`ReplaceOptions` mengosongkan koleksi navigasi sesudah `RemoveRange`, sehingga EF mencoba
memperbarui baris yang sudah ditandai hapus. Itu diperbaiki dengan menghapus dan menambah lewat
`DbSet` tanpa memutasi koleksi navigasinya.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-24` — satu jenis pemeriksaan dapat memiliki lebih dari satu baris batas menurut jenis kelamin dan kelompok umur | **Terpenuhi** | `AC24_TigaBarisBatasHemoglobin_DapatDibuatUntukSatuPemeriksaan` membuat ketiganya lewat endpoint dan membacanya kembali lewat daftar berpaging. Batas keempat berkombinasi sama ditolak `409` |
| `AC-28` — pemeriksaan berhasil pilihan hanya menerima nilai dari daftar yang sah | **Terpenuhi untuk cakupan task ini** | Daftar pilihan sah dapat dibuat dan diubah lewat endpoint, kode pilihannya dijaga unik, dan bentuk angka dilarang punya daftar pilihan (`VAL-24`). Penolakan pengetikan bebas saat **hasil diinput** terjadi pada jalur input hasil, yang bukan cakupan task ini |
| `AC-33` jalur tolak — perubahan batas kritis tertahan | **Terpenuhi** | Tiga uji `VAL-28` menutup ketiga cara mengubah batas kritis: angkanya, penanda kritis sebuah pilihan, dan membuang pilihan yang bertanda kritis. Ditambah satu uji yang membuktikan perubahan sah tetap diterima. Bagian "batas normal langsung berlaku" juga terbukti |
| `AC-34` — setiap perubahan menyimpan kolom, nilai lama, nilai baru, pelaku, penyetuju, waktu, dan alasan | **Terpenuhi** | `MengubahBatasNormal_LangsungBerlakuDanMenerbitkanRiwayat` memeriksa ketujuhnya pada satu baris riwayat yang diterbitkan endpoint, dan `GET /{id}/history` mengembalikannya |

### 6.2 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Enam endpoint tersedia dan terdokumentasi Swagger | **Terpenuhi** | `ControllerBatasNilai_MemakaiBaseRouteYangDikunciKontrak` memastikan tepat enam, dan `KeenamEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak` memeriksa route beserta verb-nya satu per satu. Metadata Swagger lengkap: `[Tags]`, `[ProducesResponseType]` untuk setiap kode status, dan komentar XML pada tiap aksi |
| `[AccessPermission]` terpasang sehingga permissionnya terdaftar sendiri | **Terpenuhi** | Keenam endpoint punya `[AccessPermission]` dengan resource `LabValueBound` dan action `Read`/`Create`/`Update` sesuai kontrak; diperiksa lewat reflection, mengikuti pola uji yang sudah dipakai modul Billing |
| Seluruh jalur gagal terbukti | **Terpenuhi** | Keempat yang diwajibkan roadmap — `VAL-22`, `VAL-23`, `VAL-24`, `VAL-28` — punya ujinya masing-masing, ditambah `VAL-21`, `VAL-25` .. `VAL-27`, `VAL-29`, dan `VAL-30` |

**Seluruh butir DoD terpenuhi.** Satu hal disebut apa adanya sebagai batas cakupan: `VAL-28`
menolak perubahan batas kritis, tetapi **jalur penggantinya belum ada**. Sampai `BE-LAB-05`
selesai, batas kritis hanya dapat diubah lewat perintah database langsung. Itu keadaan yang
disengaja — lebih baik tertutup rapat daripada terbuka diam-diam — tetapi perlu diketahui
sebelum modul ini dipakai sungguhan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build tidak menghasilkan error. Warning yang ada seluruhnya komentar XML milik modul lain dan jumlahnya tidak bertambah |
| Masalah yang diketahui | Satu test Billing tetap merah sejak sebelum task ini (`FINAL`/`CLOSED`, sudah diajukan lewat `approval-requests/2026-09-02-temuan-billing-final-closed.md`) |
| Risiko tersisa | **Pertama**, batas kritis kini terkunci tanpa jalur pengganti sampai `BE-LAB-05` selesai. **Kedua**, tiga hak akses baru belum diberikan kepada peran mana pun; sampai administrator memberikannya, keenam endpoint akan menolak semua pemanggil. **Ketiga**, `VAL-30` menjaga batas aktif terakhir, tetapi tidak ada yang menjaga sebuah pemeriksaan laboratorium **belum pernah** punya batas nilai sama sekali — pemeriksaan tanpa batas tetap dapat dipesan, dan hasilnya tidak akan dapat dinilai. Itu di luar cakupan task ini dan layak diangkat sebagai pertanyaan tersendiri kepada pemilik modul. **Keempat**, endpoint ini belum pernah diuji lewat HTTP sungguhan |
| Perubahan sampingan | `NONE`. Satu baris ditambahkan pada `Program.cs`, dan tidak ada berkas lain di luar cakupan yang tersentuh |
| Interupsi | `NONE` |
| Selisih snapshot source | Roadmap menyebut Backend SHA `c87d9c0`; pekerjaan ini berjalan di atas `d8d67c3`. Permukaan yang disentuh diperiksa langsung dan cocok |
| Status Git | Bertambah tiga berkas source baru, satu berkas test baru, dan satu baris pada `Program.cs`. Tidak ada `git add`, `commit`, `push`, `merge`, maupun `rebase` yang dijalankan |
| Langkah berikutnya | **1.** `BE-LAB-05` — endpoint pengajuan dan persetujuan batas kritis. Ia dapat dibangun sekarang, tetapi tidak dapat dinyatakan siap pakai sebelum manajemen rumah sakit menetapkan pemegang `LabCriticalBound : Approve`. **2.** Berikan ketiga hak akses `LabValueBound` kepada peran kepala instalasi laboratorium lewat access management. **3.** Angkat pertanyaan pemeriksaan tanpa batas nilai kepada pemilik modul. **4.** `FE-LAB-02` dapat mulai memakai keenam endpoint ini |

---

## 8. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement` |
| Submodule | — |
| Pemilik/prefix pada registry | `LaboratoryManagement / Laboratory`, prefix `Lab`, `BUSINESS DOMAIN / MODULE` |
| Status registry | `ACTIVE` sejak 2026-09-02 |
| Keberlakuan | `NEW CODE` |
| Sumber tata kelola | `AGENTS.md`, contract, dan registry seluruhnya terbaca; `QBE_EXCEPTIONS.json` berisi nol pengecualian |

### QBE ID yang berlaku

| QBE ID | Bagaimana dipenuhi |
| --- | --- |
| QBE-SVC-001 | Seluruh CRUD dan orkestrasi domain berada di `LabValueBoundService`. Controller tidak menyentuh `ApplicationDbContext` sama sekali |
| QBE-API-001 | Route bertversi `api/v1/...`, pembungkus `ApiResponse<T>`, paging `PagedResult<T>`, dan kode status mengikuti keluarga endpoint yang sudah ada |
| QBE-DTO-001 | Tidak ada entity EF yang diekspos. Seluruh request dan response memakai DTO di folder `DTOs/` milik modulnya |
| QBE-PERM-001 | `[Authorize]`, `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]` terpasang mengikuti pola `LabOrderController` |
| QBE-VAL-001 | Sepuluh validasi bisnis ditegakkan di service, bukan hanya lewat data annotation |
| QBE-LOG-001 | `LabValueBound.Create`, `.Update`, dan `.Deactivate` dicatat lewat `LoggerService` beserta pelakunya |
| QBE-PAGE-001 | Daftar memakai paging, penyaring, dan pencarian yang sudah mapan; `PageSize` dibatasi 1–100 |
| QBE-DEL-001 | Grup ini tidak menyediakan jalur hapus. Batas nilai dinonaktifkan, dan penonaktifannya menerbitkan riwayat beserta pelakunya |
| QBE-ENT-001, QBE-CFG-001, QBE-NAM-002 | Tidak ada entity baru pada task ini; ketiganya sudah dipenuhi `BE-LAB-02` dan `BE-LAB-03` |
| QBE-TXN-001 | Perubahan batas nilai beserta riwayatnya disimpan dalam satu `SaveChangesAsync`, sehingga riwayat tidak mungkin terbit tanpa perubahannya, maupun sebaliknya |

### QBE ID yang tidak berlaku

| QBE ID | Alasan |
| --- | --- |
| QBE-CODE-001 sampai QBE-CODE-006 | Tidak ada nomor bisnis yang dialokasikan. `OptionCode` diisi pengguna, bukan dibangkitkan sistem |
| QBE-NAM-001, QBE-NAM-003, QBE-DB-001, QBE-DB-002 | Tidak ada entity, tabel, maupun rename yang dibuat task ini |
| QBE-MOD-002, QBE-MOD-003 | Tidak ada model persisted baru |
| QBE-ENUM-001 | Tidak ada enum baru; `LabResultForm` dan `LabGenderScope` sudah ada sejak `BE-LAB-02` |
| QBE-OPT-001 | Tidak ada endpoint options/metadata yang dibuat, karena belum ada yang mengonsumsinya |
| QBE-AUD-001 | Kolom audit datang dari base model dan tidak disentuh task ini |
