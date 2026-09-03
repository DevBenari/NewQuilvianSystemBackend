# Laporan Perubahan Backend — `BE-LAB-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-05` |
| Judul | Endpoint pengajuan dan persetujuan batas kritis |
| Slice | `S3` — pengelolaan batas nilai (`roadmap/backend-roadmap.md` bagian 3, gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 3 |
| Trace | `FR-03.4`; `LAB-DEC-023` (BR-19); `LAB-STATE-v1` r2 bagian 4; `LAB-API-v1` r3 grup Lab Critical Bound Approval; `LAB-PERM-v1` r3; `LAB-VAL-v1` r3 `VAL-31` .. `VAL-35`; `CAP-16`, `CAP-17` |
| Contract version | `LAB-API-v1` r3, `LAB-STATE-v1` r2, `LAB-PERM-v1` r3 — `approved`, dikunci 2026-09-02 |
| Dependency | `BE-LAB-03` dan `BE-LAB-04`, keduanya **`SELESAI`** 2026-09-02 |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 1, database 1, keamanan 2, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/{DTOs,Services,Controllers}`, `Program.cs`, `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/`, dan `docs/module-blueprints/laboratorium/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `d8d67c3`, branch `yoga` |
| Tanggal | 2026-09-03 |
| Status | **Selesai.** Lima endpoint tersedia, `VAL-31` sampai `VAL-35` ditegakkan di service, dan tidak ada perubahan schema sehingga tidak ada migration. Satu hal di luar kendali teknik tetap terbuka: peran pemegang `LabCriticalBound : Approve` belum ditetapkan manajemen rumah sakit |

---

## 1. Masalah yang diperbaiki

`BE-LAB-04` menutup jalur ubah biasa untuk batas kritis lewat `VAL-28`. Akibatnya benar secara
keselamatan, tetapi menyisakan keadaan yang tidak dapat dipakai: **batas kritis tidak dapat
diubah lewat cara apa pun**. Sampai task ini selesai, satu-satunya jalan mengubahnya adalah
perintah database langsung — persis jenis perubahan tanpa jejak yang hendak dicegah
`LAB-DEC-023`.

Task ini membuka jalur penggantinya, dan jalur itu punya satu syarat yang tidak dapat ditawar:
**pengaju tidak boleh menyetujui pengajuannya sendiri**.

Kenapa syarat itu penting sampai menjadi alasan seluruh task ini ada:

> Kepala instalasi merasa peringatan nilai kritis terlalu sering muncul. Ia menaikkan batas
> kritis atas Kalium dari 6,0 menjadi 8,0 mmol/L. Sejak saat itu pasien dengan Kalium 7,2
> tidak lagi memicu kewajiban pelaporan nilai kritis.
>
> Bila ia juga yang mengesahkan perubahannya sendiri, persetujuan klinis hanya menjadi
> formalitas yang dilewati satu orang dalam dua klik.

`CAP-16` sudah membuktikan sistem izin yang ada **tidak dapat** menegakkan syarat itu.
`AccessPermissionService.HasAccessAsync` menjawab "boleh atau tidak" untuk sebuah aksi, dan tidak
pernah membandingkan siapa pelaku sebelumnya pada baris data yang sama. Seseorang yang memegang
`LabCriticalBound : Approve` akan lolos pemeriksaan izin walaupun dialah yang mengajukan. Karena
itu aturannya wajib berupa kode di dalam service — dan itulah yang dikerjakan di sini.

---

## 2. Proses bisnis

**Tujuan.** Perubahan batas kritis menempuh jalur pengajuan yang diputuskan orang lain, dengan
jejak yang lengkap.

**Pelaku.** Kepala instalasi laboratorium sebagai pengaju (`LabValueBound : Update`), dan
pemegang kewenangan persetujuan batas kritis sebagai pemutus (`LabCriticalBound : Approve`).
Keduanya wajib orang yang berbeda.

**Langkah yang berurutan:**

1. Kepala instalasi mengajukan perubahan batas kritis beserta **alasannya**. Batas yang berlaku
   tidak bergerak sedikit pun.
2. Sistem memeriksa usulan itu masuk akal terhadap batas normal yang berlaku, dan cocok dengan
   bentuk hasil pemeriksaannya.
3. Pengajuan berdiri berstatus `Submitted`. Selama itu tidak ada pengajuan lain yang boleh
   dibuat untuk batas nilai yang sama.
4. Pemutus dari pihak klinis menyetujui atau menolak. **Bukan pengajunya.**
5. Bila disetujui: usulan diperiksa **ulang** terhadap keadaan terkini, batas kritis diperbarui,
   dan satu baris riwayat diterbitkan dengan pelaku = pengaju dan penyetuju = pemutus.
6. Bila ditolak: tidak ada yang berubah pada batas nilai.
7. Pengaju dapat menarik pengajuannya sendiri selama belum diputuskan.

**Aturan yang berlaku:**

| Aturan | Kondisi | Kode |
| --- | --- | --- |
| `VAL-31` | Alasan pengajuan kosong | `422` |
| `VAL-32` | Sudah ada pengajuan berjalan untuk batas nilai yang sama | `409` |
| **`VAL-33`** | **Yang memutuskan adalah pengaju sendiri** | **`403`** |
| `VAL-34` | Pengajuan sudah diputuskan sebelumnya | `409` |
| `VAL-35` | Yang menarik bukan pengaju | `403` |
| — | Pelaku tidak dikenali | `403` |
| `CAP-17` | Dua pemutus memutuskan pengajuan yang sama bersamaan | `409` |

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Pengaju menyetujui pengajuannya sendiri | Ditolak `403`. Batas lama tetap, status tetap `Submitted`, riwayat kosong |
| Pengaju **menolak** pengajuannya sendiri | Juga ditolak `403` — keputusan atas pengajuan sendiri tetap keputusan atas pengajuan sendiri |
| Pemanggil tanpa identitas yang dapat dikenali | Ditolak `403` pada semua tindakan, termasuk mengajukan |
| Batas normal bergeser antara pengajuan dan persetujuan | Usulan diperiksa ulang; yang menjadi mustahil ditolak `422` |
| Kode pilihan yang diusulkan tidak ada pada batas nilainya | Ditolak `422` sejak diajukan |
| Pengajuan ditarik | Batas nilai bebas untuk pengajuan baru; batas yang berlaku tidak tersentuh |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabCriticalBoundApprovalDtos.cs` | **Baru.** Tiga DTO: pengajuan, keputusan, dan respons |
| `Areas/HealthServices/LaboratoryManagement/Services/LabCriticalBoundApprovalService.cs` | **Baru.** Seluruh logika pengajuan, persetujuan, penolakan, penarikan, penegakan `VAL-31` .. `VAL-35`, dan tiga exception yang dipetakan menjadi `422`, `409`, dan `403` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabCriticalBoundApprovalController.cs` | **Baru.** Lima endpoint beserta metadata akses dan Swagger |
| `Program.cs` | Mendaftarkan `LabCriticalBoundApprovalService`, satu baris |
| `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabCriticalBoundApprovalTests.cs` | **Baru.** Uji jalur berhasil, kelima jalur gagal, temuan audit, dan kontrak kelima endpoint |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Lima endpoint baru**, persis seperti `LAB-API-v1` r3 grup Lab Critical Bound Approval. Tidak ada endpoint lama yang disentuh |
| Database | `NOT APPLICABLE`. Tidak ada perubahan schema, **tidak ada migration**, dan tidak ada perintah database yang dijalankan. Ketiga tabelnya sudah dibuat `BE-LAB-02` dan `BE-LAB-03` |
| Keamanan/Auth | **Inti task ini.** `LabCriticalBound : Read` dan `: Approve` dipakai untuk membaca dan memutuskan; `LabValueBound : Update` untuk mengajukan dan menarik. Pemisahan itulah yang memungkinkan dua peran berbeda diberikan kepada dua orang berbeda. Di atas itu, `VAL-33` ditegakkan sebagai kode di service — bukan sebagai konfigurasi permission, karena `CAP-16` membuktikan konfigurasi tidak dapat menegakkannya |

**Keputusan yang perlu diketahui: pelaku tanpa identitas ditolak.**

Setiap tindakan menolak pemanggil yang identitasnya tidak dapat dikenali. Ini bukan kehati-hatian
berlebihan, melainkan syarat agar `VAL-33` tetap bermakna.

Tanpa penolakan itu, `GetCurrentUserId()` mengembalikan `Guid.Empty` untuk pemanggil tanpa claim.
Akibatnya: pengajuan yang dibuat pengguna sungguhan dapat **disetujui** oleh pemanggil tanpa
identitas, karena `Guid.Empty` tidak sama dengan id pengaju mana pun sehingga perbandingan
`VAL-33` lolos. Pengaman itu akan tetap terlihat bekerja padahal sudah bocor — dan pengaman yang
kehilangan artinya lebih berbahaya daripada pengaman yang tidak ada.

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Critical Bound Approval

Base URL: `api/v1/health-services/laboratory-management/lab-value-bounds/{valueBoundId}/critical-change-requests`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pengajuan untuk satu batas nilai, terbaru lebih dulu | `LabCriticalBound : Read` |
| `POST` | `/` | Mengajukan perubahan batas kritis | `LabValueBound : Update` |
| `POST` | `/{requestId}/approve` | Menyetujui; batas baru mulai berlaku dan riwayat terbit | `LabCriticalBound : Approve` |
| `POST` | `/{requestId}/reject` | Menolak; batas lama tetap berlaku | `LabCriticalBound : Approve` |
| `POST` | `/{requestId}/withdraw` | Menarik pengajuan sendiri | `LabValueBound : Update` |

Kode status: `200`, `403`, `404`, `409`, `422`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil | `PASS` | `0 Error(s)` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | Lolos | `PASS` | `Files evaluated: 28`, `VIOLATION: 0`, `Final result: PASS` |
| Mengajukan — pengajuan `Submitted`, batas lama tidak bergerak | Sesuai harapan | `PASS` | `Mengajukan_MenghasilkanPengajuanSubmittedDanBatasLamaTidakBergerak` |
| **Gagal `VAL-31`** — alasan kosong | Ditolak, nol baris tersimpan | `PASS` | `VAL31_MengajukanTanpaAlasan_Ditolak` |
| **Gagal `VAL-32`** — pengajuan kedua saat yang pertama belum diputuskan | Ditolak, tetap satu baris | `PASS` | `VAL32_PengajuanKeduaSaatYangPertamaBelumDiputuskan_Ditolak` |
| **Gagal `VAL-33`** — pengaju menyetujui pengajuannya sendiri | Ditolak; batas lama tetap, status tetap, riwayat kosong | `PASS` | `VAL33_PengajuMenyetujuiPengajuannyaSendiri_Ditolak` |
| **Gagal `VAL-33`** — pengaju menolak pengajuannya sendiri | Ditolak | `PASS` | `VAL33_PengajuMenolakPengajuannyaSendiri_JugaDitolak` |
| **Gagal** — pelaku tanpa identitas, saat mengajukan maupun memutuskan | Ditolak keduanya | `PASS` | `PelakuTanpaIdentitas_DitolakSaatMengajukanMaupunMemutuskan` |
| **Gagal `VAL-34`** — memutuskan ulang pengajuan terminal | Ditolak, dua kasus | `PASS` | `VAL34_MemutuskanUlangPengajuanYangSudahTerminal_Ditolak` |
| **Gagal `VAL-35`** — yang menarik bukan pengaju | Ditolak | `PASS` | `VAL35_YangMenarikBukanPengaju_Ditolak` |
| Menyetujui — batas kritis berubah, riwayat terisi penyetuju | Sesuai harapan | `PASS` | `Menyetujui_MengubahBatasKritisDanMengisiPenyetujuPadaRiwayat`; pelaku dan penyetuju diperiksa dua orang berbeda |
| Menolak — batas kritis tidak berubah sama sekali | Sesuai harapan | `PASS` | `Menolak_TidakMengubahBatasKritisSamaSekali` |
| Menarik — membebaskan batas nilai untuk pengajuan baru | Sesuai harapan | `PASS` | `MenarikPengajuanSendiri_MembebaskanBatasNilaiUntukPengajuanBaru` |
| Kontrak kelima endpoint: route, verb, `[AccessPermission]` | Sesuai kontrak | `PASS` | `KelimaEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak`, lima `[InlineData]` |
| Base route dan pemisahan dua peran | Lima endpoint, dua di antaranya `Approve` | `PASS` | `ControllerPersetujuan_MemakaiBaseRouteYangDikunciKontrak` |
| Seluruh test `LabCriticalBoundApprovalTests` | Hijau | `PASS` | `Failed: 0` |
| Seluruh suite `QuilvianSystemBackend.Tests` | 922 lulus, 1 gagal | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan Billing `FINAL`/`CLOSED` yang terbuka sejak sebelum task ini |
| Migration | Tidak ada | `NOT APPLICABLE` | Task ini tidak menyentuh schema |
| Uji lewat HTTP sungguhan | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan |

Uji manual lewat antarmuka: `NOT FEASIBLE`.

### 5.1 Audit adversarial dan enam perbaikan yang lahir darinya

Sebelum task ini ditutup, implementasinya diaudit lewat empat lensa berbeda — mengalahkan
`VAL-33`, konkurensi, daur hidup, dan kebenaran data. **Audit itu tidak selesai:** ia terhenti
karena batas sesi, dan seluruh tahap verifikasi silangnya tidak sempat berjalan. Karena itu
angka "nol temuan bertahan" yang dihasilkannya **tidak berarti apa-apa** dan tidak dipakai
sebagai bukti apa pun di laporan ini.

Yang dipakai adalah tahap pencariannya, yang sempat selesai. Enam temuannya diperiksa ulang
secara langsung terhadap source, dan seluruhnya nyata:

| Temuan | Akibatnya bila dibiarkan | Perbaikan |
| --- | --- | --- |
| **`Version` tidak pernah dinaikkan** | Token konkurensi tidak pernah berubah, sehingga klausa `WHERE` milik EF tetap cocok bagi penulis kedua. `CAP-17` **tidak pernah menyala sama sekali**, padahal terlihat terpasang | `entity.Version++` pada persetujuan, penolakan, dan penarikan. Diverifikasi terhadap pola yang sudah dipakai `LabOrderService` dan `LabSpecimenService` |
| Usulan tidak diperiksa ulang saat disetujui | Batas normal dapat bergeser lewat `PUT` biasa setelah pengajuan dibuat, sehingga usulan yang tadinya masuk akal tersimpan sebagai batas kritis di dalam rentang normal | `EnsureProposedBoundsMakeSense` dipanggil dua kali — saat diajukan dan saat diputuskan |
| Kode pilihan tak dikenal diabaikan diam-diam | Penerapan memadamkan penanda kritis pada pilihan yang tidak disebut. Satu salah ketik — `P5` alih-alih `P4` — mencabut **seluruh** penanda kritis tanpa satu pun pesan | Kode yang tidak ada pada batas nilai ditolak `422` sejak diajukan |
| Bentuk hasil tidak pernah diperiksa | Usulan batas angka pada pemeriksaan berhasil pilihan (dan sebaliknya) diterima, lalu dilaporkan sudah berlaku padahal tidak berakibat apa pun | `EnsureProposalFitsResultForm` menolak keduanya |
| Penarikan mengisi `DecidedByUserId` dengan pengaju | Baris itu terbaca seolah pengaju dan pemutusnya orang yang sama — meracuni satu-satunya penanda yang dapat membongkar pelanggaran `VAL-33` | Ruas itu dibiarkan kosong pada penarikan; siapa yang menarik sudah terjawab `RequestedByUserId` |
| Nilai riwayat tidak dipotong 200 karakter | Daftar pilihan kritis yang panjang membuat persetujuan yang sah gagal dengan galat tak tertangani | `Truncate` dipasang pada `OldValue` dan `NewValue` |

Ditambah satu temuan lagi yang muncul dari pengujian, bukan dari audit: `VAL-31` semula
menghasilkan `400` dari validasi model ASP.NET Core, bukan `422` beserta kalimat yang ditetapkan
`LAB-VAL-v1` r3. Atribut `[Required]` dilepas dari DTO-nya supaya pemeriksaan terjadi di service
dan kode statusnya sesuai kontrak.

Setiap perbaikan di atas diberi ujinya sendiri, supaya tidak dapat kembali diam-diam.

### 5.2 Satu kesalahan pengujian yang ditemukan dan diperbaiki

Uji `VAL-33` sempat **lulus karena kebetulan urutan**, bukan karena aturannya ditegakkan.

`HttpContextAccessor` bawaan framework menyimpan `HttpContext` pada `AsyncLocal` **statis**.
Membuat instance kedua karena itu menimpa nilai instance pertama di dalam alur async yang sama,
sehingga dua service yang seharusnya membawa dua identitas berbeda justru membaca pelaku yang
sama. Pengujian dua-identitas — dan justru itulah yang dibutuhkan untuk menguji `VAL-33` —
menjadi tidak berarti.

Itu terungkap ketika uji pelaku-tanpa-identitas gagal dengan pesan `VAL-33`, bukan pesan yang
diharapkan. Diganti dengan accessor yang menyimpan konteksnya sendiri per instance. Kelulusan
yang lahir dari kebetulan lebih berbahaya daripada kegagalan, karena ia menutup mata.

**Tidak dijalankan:** uji perlombaan konkurensi yang sesungguhnya. Provider InMemory tidak
menegakkan token konkurensi seperti PostgreSQL, sehingga yang dibuktikan di sini adalah
`Version` benar-benar naik pada setiap keputusan — bukan bahwa dua penulis bersamaan saling
menggagalkan. Pembuktian itu memerlukan database sungguhan dan wewenang tersendiri.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-33` seluruh jalur — batas normal langsung berlaku, batas kritis tertahan sampai disetujui pihak klinis | **Terpenuhi** | Bagian "langsung berlaku" dibuktikan `BE-LAB-04`; bagian "tertahan" dibuktikan `Mengajukan_...`; jalur persetujuan, penolakan, dan penarikan masing-masing punya ujinya; dan `VAL-33` dibuktikan dua arah — menyetujui maupun menolak pengajuan sendiri |

### 6.2 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Lima endpoint tersedia | **Terpenuhi** | `ControllerPersetujuan_MemakaiBaseRouteYangDikunciKontrak` memastikan tepat lima, dan `KelimaEndpoint_...` memeriksa route beserta verb dan permission-nya satu per satu |
| `VAL-32` dan `VAL-33` terbukti lewat uji | **Terpenuhi** | `VAL32_...`, `VAL33_PengajuMenyetujuiPengajuannyaSendiri_Ditolak`, dan `VAL33_PengajuMenolakPengajuannyaSendiri_JugaDitolak` |
| Larangan menyetujui sendiri ada sebagai **kode di service**, bukan sekadar konfigurasi permission | **Terpenuhi** | `LoadDecidableRequestAsync` membandingkan `RequestedByUserId` dengan pelaku dan melempar `LabCriticalBoundForbiddenException`. Diperkuat penolakan pelaku tanpa identitas, tanpa mana perbandingan itu dapat dilewati |

**Seluruh butir DoD terpenuhi.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build tidak menghasilkan error |
| Masalah yang diketahui | Satu test Billing tetap merah sejak sebelum task ini |
| Risiko tersisa | **Pertama dan paling penting:** peran pemegang `LabCriticalBound : Approve` **belum ditetapkan manajemen rumah sakit**. Sampai itu terjadi, tidak ada satu pun akun yang dapat menyetujui, sehingga batas kritis tetap tidak dapat diubah lewat aplikasi. Ini bukan cacat teknik — ini keputusan organisasi yang memang bukan wewenang roadmap maupun saya. **Kedua**, perlombaan konkurensi belum diuji terhadap database sungguhan. **Ketiga**, `VAL-32` berupa periksa-lalu-tulis tanpa penjaga unik di database; dua pengajuan yang lahir pada saat yang sama secara teori dapat lolos berdua. Menutupnya memerlukan index unik parsial dan karena itu migration tersendiri — di luar cakupan task ini, dan dicatat sebagai pekerjaan lanjutan. **Keempat**, endpoint ini belum pernah diuji lewat HTTP sungguhan |
| Perubahan sampingan | `NONE`. Satu baris pada `Program.cs`, sisanya berkas baru |
| Interupsi | Audit adversarial terhenti oleh batas sesi di tengah jalan. Tahap pencariannya sempat selesai dan dipakai; tahap verifikasinya tidak, dan karena itu hasilnya tidak dipakai sebagai bukti. Dicatat apa adanya pada bagian 5.1 |
| Status Git | Tiga berkas source baru, satu berkas test baru, satu baris pada `Program.cs`. Tidak ada operasi Git yang dijalankan |
| Langkah berikutnya | **1.** Tetapkan pemegang `LabCriticalBound : Approve` bersama manajemen rumah sakit — tanpa itu jalur ini tidak dapat dipakai. **2.** Berikan hak akses `LabValueBound` dan `LabCriticalBound` kepada peran yang sesuai. **3.** Pertimbangkan index unik parsial untuk `VAL-32`. **4.** `BE-LAB-06` adalah task berikutnya yang penahannya hanya gerbang global |

---

## 8. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik/prefix pada registry | `Laboratory`, prefix `Lab`, `ACTIVE` sejak 2026-09-02 |
| Keberlakuan | `NEW CODE` |
| Sumber tata kelola | `AGENTS.md`, contract, dan registry seluruhnya terbaca; nol pengecualian QBE |

### QBE ID yang berlaku

| QBE ID | Bagaimana dipenuhi |
| --- | --- |
| QBE-SVC-001 | Seluruh logika di service; controller tidak menyentuh `ApplicationDbContext` |
| QBE-API-001 | Route bertversi, `ApiResponse<T>`, dan kode status mengikuti keluarga endpoint yang ada |
| QBE-DTO-001 | Tidak ada entity EF yang diekspos |
| QBE-PERM-001 | `[Authorize]`, `[AccessController]`, `[AccessAction]`, `[AccessPermission]` terpasang pada kelima endpoint |
| QBE-VAL-001 | `VAL-31` .. `VAL-35` beserta pemeriksaan bentuk hasil dan kode pilihan ditegakkan di service |
| QBE-LOG-001 | Empat peristiwa dicatat lewat `LoggerService` beserta pengaju dan pemutusnya |
| QBE-TXN-001 | Perubahan batas kritis, penerbitan riwayat, dan pembaruan pengajuan disimpan dalam satu `SaveChangesAsync` |
| QBE-DEL-001 | Tidak ada jalur hapus; pengajuan hanya berpindah ke status terminal |

### QBE ID yang tidak berlaku

| QBE ID | Alasan |
| --- | --- |
| QBE-ENT-001, QBE-CFG-001, QBE-MOD-002, QBE-MOD-003, QBE-NAM-001 .. 004 | Tidak ada entity, tabel, maupun migration baru pada task ini |
| QBE-CODE-001 .. 006 | Tidak ada nomor bisnis yang dialokasikan |
| QBE-ENUM-001 | Tidak ada enum baru; `LabBoundChangeStatus` sudah ada sejak `BE-LAB-03` |
| QBE-PAGE-001 | Daftar pengajuan per batas nilai berukuran kecil dan tidak berpaging, sesuai kontrak |
| QBE-DB-001, QBE-DB-002 | Khusus `LEGACY MIGRATION` |
