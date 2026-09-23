# Laporan Perubahan Backend — `BE-LAB-34`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-34` |
| Judul | Ruas konfirmasi pada daftar pantau |
| Slice | `EPIC-LAB-12`, gelombang `MVP-5d` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6e |
| Trace | `FR-11.12`; `LAB-DEC-061`; melengkapi `AC-94` dan `AC-95` bagian "terbaca pada daftar" |
| Contract version | `LAB-API-v1` **`r14`** bagian 9 — `approved` pemilik modul 2026-09-16 |
| Dependency | `BE-LAB-30` ✅, `BE-LAB-31` ✅ |
| Klasifikasi | `LIGHT` — skor 3 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — DTO dan service Laboratorium. **Nol migration** |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-16 |
| Status | **✅ `SELESAI`** — ketiga ruas terbaca dari database lewat endpoint daftar pantau yang sebenarnya; pesanan yang belum dikonfirmasi terbukti mengembalikan ketiganya `null`. 15 pemeriksaan, 15 `PASS` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 |
| Keberlakuan | `TOUCHED LEGACY` pada DTO dan service yang sudah ada |
| Status gerbang | `QBE-MOD-002` **tidak menahan** |
| QBE ID yang berlaku | `QBE-API-001`, `QBE-DTO-001`, `QBE-SVC-001` |
| QBE ID yang **tidak** berlaku | Seluruh QBE entity, configuration, penamaan, database, validasi, permission, dan nomor bisnis — nol di antaranya tersentuh |

---

## 1. Masalah yang diperbaiki

**Ini koreksi atas kesalahan saya sendiri, dan pantas ditulis sebagai itu.**

`BE-LAB-33` menambahkan lima ruas respons konfirmasi persis seperti diminta `LAB-API-v1` `r13`,
dan pekerjaannya benar terhadap kontrak itu. **Kontraknya yang salah sasaran.**

Usul `r13` — yang saya tulis pada [`BE-LAB-31.md`](BE-LAB-31.md) bagian 7.2 — disusun dengan
membaca **apa yang dibutuhkan layar**, tetapi **tanpa memeriksa endpoint mana yang layar itu
benar-benar panggil**. Keduanya pertanyaan yang berbeda, dan hanya yang kedua dapat dijawab dari
source.

| Yang disebut `r13` | Yang sebenarnya dibaca ketiga menu pemeriksaan |
| --- | --- |
| `LabOrderListResponse`, dipakai `GET /lab-orders` dan `GET /lab-orders/by-discipline/{discipline}` | **`LabMonitoringItemResponse`**, dipakai grup `Lab Monitoring` |

Ketiga menu itu adalah `lab-monitoring/clinical-pathology`, `lab-monitoring/anatomic-pathology`,
dan `lab-monitoring/microbiology`. Endpoint `GET /lab-orders/by-discipline/{discipline}` yang
memang menerima kelima ruas `r13` **nol dipakai frontend** — diperiksa dengan pencarian di seluruh
source frontend, nol kemunculan.

**Akibatnya:** sesudah `BE-LAB-33` selesai, `FE-LAB-15` **masih** tidak dapat membangun kolom
Konfirmasi. Celah itu ketahuan ketika `FE-LAB-15` hendak dimulai — bukan oleh build, bukan oleh
uji, bukan oleh tinjauan kontrak.

**Yang tidak terbuang dari `BE-LAB-33`.** Detail pesanan dan setiap jawaban aksi yang melewati
`GetDetailAsync` — termasuk `POST /lab-orders/{id}/confirm` — membawa kelima ruasnya. Pop-up
konfirmasi `FE-LAB-15` memakai itu untuk menampilkan hasilnya seketika tanpa memuat ulang daftar.

---

## 2. Proses bisnis

Tidak ada tindakan baru. Yang berubah adalah apa yang dapat dilihat pada **ketiga menu
pemeriksaan**: pesanan yang sudah dikonfirmasi kini membawa nama konfirmator, waktu konfirmasi,
dan nama dokter pemeriksa — siap ditampilkan apa adanya. Pesanan yang belum dikonfirmasi membawa
ketiganya kosong, dan itu keadaan sah.

### 2.1 Kenapa hanya tiga ruas, bukan lima

**Nol penunjuk dikirim, dan itu disengaja.** Daftar pantau adalah layar **baca**: ia menampilkan
antrean dan tidak melakukan aksi apa pun terhadap dokter pemeriksa maupun konfirmator. Penunjuk
hanya dibutuhkan aksi, dan aksi pada modul ini berjalan lewat **detail pesanan** — yang sudah
membawa `confirmedByUserId` dan `examinerDoctorId` sejak `r13`.

Mengirim penunjuk yang tidak dipakai berarti mengirim nilai yang **tidak boleh ditampilkan**
(`no-uuid-display`) ke layar yang tidak membutuhkannya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabMonitoringDtos.cs` | Tiga ruas pada `LabMonitoringItemResponse` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabMonitoringService.cs` | Pengisian ketiganya di dalam proyeksi yang sama; satu `using` untuk `MstDoctor` |

Nol migration. Nol entity. Nol endpoint baru. Nol permission baru.

### 3.2 Terjemahan nama dikerjakan di dalam proyeksi, bukan sesudahnya

Kedua nama diterjemahkan sebagai sub-query **di dalam proyeksi yang sama** — mengikuti cara
`PatientName` dan `MedicalRecordNumber` yang sudah ada pada method itu, bukan pola baru.
Menerjemahkannya per baris sesudah proyeksi akan mengubah satu halaman 25 pesanan menjadi 51
perjalanan ke database.

Jalur terjemahannya **sama persis** dengan yang dipakai `LabOrderService`: nama pengguna dari
`DisplayName ?? UserName ?? Email ?? UserCode`, nama dokter dari `MstDoctor.FullName`. Satu orang
karena itu tidak dapat terbaca dengan dua nama berbeda antar layar.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tiga ruas bertambah** pada `LabMonitoringItemResponse`, persis seperti `r14` bagian 9. Aditif — nol endpoint, ruas, nilai enum, atau pembungkus yang berubah |
| Database | **Nol perubahan schema, nol migration.** Ketiga kolomnya sudah berdiri sejak `BE-LAB-30`. Yang bertambah hanya dua sub-query terjemahan nama per halaman |
| Keamanan/Auth | `NOT APPLICABLE`. Grup `Lab Monitoring` tetap memakai hak aksesnya yang sudah ada |

---

## 4. Endpoint yang berubah

#### Health Services / Laboratory Management / Lab Monitoring

| Method | Path | Ruas yang bertambah |
| --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-monitoring/{discipline}` | `confirmedAt`, `confirmedByName`, `examinerDoctorName` |

Ketiga menu pemeriksaan memakai endpoint yang sama dengan penyaring disiplin yang berbeda,
sehingga ketiganya memperoleh ruas ini sekaligus.

---

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -p:RunAnalyzers=False` | **0 Error, 207 Warning** | `PASS` | Sama persis dengan baseline; nol warning dari kedua berkas yang diubah |
| Verifikasi kontrak terhadap `r14` bagian 9 | Ketiga ruas, tipe, dan tempatnya cocok | `PASS` | Bagian 4 |
| **Daftar pantau** — kedua pesanan uji terbaca | 5 baris Patologi Klinik terbaca | `PASS` | 5.1 |
| **Daftar pantau** — ketiga ruas terisi pada pesanan terkonfirmasi | `ConfirmedByName="Dewi"`, `ExaminerDoctorName="dr. Fajar Rama Ragussa"` | `PASS` | 5.1 |
| **Daftar pantau** — pesanan belum dikonfirmasi mengembalikan ketiganya `null` | Ketiganya `null` | `PASS` | 5.1 |
| **Daftar pantau** — baris lain tidak ikut terisi | Tepat 1 dari 5 baris membawa nama konfirmator | `PASS` | 5.1 |
| **Daftar pantau** — ruas lama tetap terisi apa adanya | `OrderStatus`, `Discipline`, `ProcedureName`, `PatientName`, `SpecimenCount` seluruhnya benar | `PASS` | 5.1 |
| Sebelas pemeriksaan `BE-LAB-33` dijalankan ulang | Seluruhnya tetap `PASS` | `PASS` | Jalur `r13` tidak tersentuh perubahan ini |
| Kebersihan database sesudah uji | Nol baris uji tersisa; 5 pesanan | `PASS` | 5.2 |
| Uji lewat HTTP sungguhan | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan; hak akses tidak berubah |

### 5.1 Lima belas pemeriksaan dijalankan sungguhan terhadap database

Harness sekali pakai di scratchpad sesi, di luar repository, memakai `ApplicationDbContext`,
`LabOrderService`, dan **`LabMonitoringService`** yang sebenarnya. Dua pesanan uji dibuat sendiri —
nol dari 5 pesanan nyata disentuh.

Hasil: **15 `PASS`, 0 `FAIL`.** Sebelas di antaranya adalah pemeriksaan `BE-LAB-33` yang
dijalankan ulang untuk memastikan jalur `r13` tidak tersentuh; empat sisanya menguji jalur daftar
pantau yang baru.

**Aktornya pengguna nyata**, bukan GUID karangan — nama diterjemahkan dari tabel `Users`, dan
aktor karangan hanya menghasilkan `null` yang tidak membuktikan apa pun.

**Satu pemeriksaan sengaja menguji ketiadaan:** dari 5 baris Patologi Klinik pada database,
**tepat 1** membawa nama konfirmator. Tanpa ini, sub-query yang keliru mengikat dapat mengisi
setiap baris dengan nama yang sama dan tetap terlihat benar pada baris pertama.

### 5.2 Kebersihan database

| Butir | Sebelum | Sesudah |
| --- | :---: | :---: |
| Total `LabOrder` | 5 | **5** |
| Pesanan berstatus `Confirmed` | 0 | **0** |
| Baris riwayat milik aktor uji | 0 | **0** |

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Ketiga ruas ada pada respons daftar pantau sesuai `r14` | **Terpenuhi** | Bagian 4; terbaca dari database — 5.1 |
| Nama siap tampil, bukan penunjuk | **Terpenuhi** | `"Dewi"` dan `"dr. Fajar Rama Ragussa"` — 5.1 |
| Pesanan lama mengembalikan `null` | **Terpenuhi** | Ketiganya `null` tanpa galat — 5.1 |
| Nol ruas daftar pantau yang sudah ada berubah | **Terpenuhi** | Pemeriksaan regresi atas lima ruas lama — 5.1 |
| Build | **Terpenuhi** | 0 Error, 207 Warning — baseline |

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-94` bagian **"terbaca pada daftar"** | **Sisi backend selesai** | `confirmedByName` dan `confirmedAt` terbaca pada jalur yang benar-benar dipakai ketiga menu |
| `AC-95` bagian **"tampil pada daftar"** | **Sisi backend selesai** | `examinerDoctorName` terbaca pada jalur yang sama |

Keduanya belum dapat ditandai terpenuhi penuh: keduanya menagih sesuatu yang **tampil di layar**,
dan layarnya adalah `FE-LAB-15` — yang kini **nol penahan**.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 207 warning, seluruhnya sudah ada sebelum task ini |
| Masalah yang diketahui | **Satu, dan ia tentang proses, bukan kode.** `r13` disusun tanpa memverifikasi endpoint yang dipanggil layar konsumennya, sehingga `BE-LAB-33` mengerjakan DTO yang benar terhadap kontrak tetapi salah sasaran terhadap kebutuhan. Pelajarannya ditulis pada kontrak bagian 9.6: **amandemen yang menambah ruas respons wajib menyebut endpoint dan DTO yang diverifikasi dari source konsumennya**, bukan DTO yang paling masuk akal namanya |
| Risiko tersisa | **Pertama**, dua sub-query terjemahan nama bertambah pada setiap pembacaan daftar pantau; keduanya di dalam proyeksi yang sama sehingga tidak menambah perjalanan ke database, tetapi **belum diukur pada daftar besar** — database uji hanya berisi 5 pesanan. **Kedua**, `GET /lab-orders/by-discipline/{discipline}` kini membawa kelima ruas `r13` tetapi **nol dipakai siapa pun**; ia bukan kode mati — endpointnya sudah ada sebelum `r13` — tetapi ruas barunya belum punya pembaca. **Ketiga**, endpoint ini belum pernah dilewati lapisan `[Authorize]` secara sungguhan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M LabMonitoringDtos.cs`, `M LabMonitoringService.cs`, `?? BE-LAB-34.md`, ditambah artefak blueprint |
| Langkah berikutnya | **1.** `FE-LAB-15` kini **nol penahan** — ketiga nilai yang dibutuhkan kolom Konfirmasi terbaca pada jalur yang benar. **2.** `FE-LAB-16` juga nol penahan. **3.** `FE-LAB-17` tetap tertahan penahan lain |

Nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, dan `deploy` dijalankan.
