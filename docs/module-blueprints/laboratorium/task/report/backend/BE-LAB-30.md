# Laporan Perubahan Backend — `BE-LAB-30`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-30` |
| Judul | Status `Confirmed` dan tiga kolomnya |
| Slice | `EPIC-LAB-12`, gelombang `MVP-5c` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6d |
| Trace | `FR-11.12`; `LAB-DEC-061`; prasyarat `AC-94` dan `AC-95`; menutup sebagian `LAB-P0-002` |
| Contract version | `LAB-STATE-v1` **`r3`** bagian 1a — `approved` 2026-09-15. **Nol endpoint baru**; `LAB-API-v1` tidak bertambah pada task ini |
| Dependency | — (nol dependency; task pembuka gelombang `MVP-5c`) |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 0, kontrak API 1, database 2, keamanan 0, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium, configuration, migration, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-16 |
| Status | **✅ `SELESAI`** — ketiga kolom berdiri, nilai enum `Confirmed` ada, migration diterapkan ke `QuilvianNewDevYoga`, jalur `Down` lalu `Up` dibuktikan, dan **kelima pesanan lama terbukti utuh dengan ketiga kolom barunya `null`** |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 |
| Keberlakuan | `TOUCHED LEGACY` pada `LabOrder` yang sudah ada; penambahannya sendiri mengikuti aturan `NEW CODE` |
| Status gerbang | `QBE-MOD-002` **tidak menahan** — lifecycle `ACTIVE`, dan nol entity baru dibuat |
| QBE ID yang berlaku | `QBE-ENT-002` (nullability mengikuti semantik domain), `QBE-CFG-001` dan `QBE-CFG-002` (mapping, index, relasi), `QBE-MOD-001`, `QBE-ENUM-001` (enum tetap dimiliki modulnya), `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001` — nol entity baru; `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001` — nol endpoint, DTO, validasi, dan permission baru; `QBE-CODE-001`..`006` — nol nomor bisnis |

Gate lolos bersih. `LAB-DEC-061` berstatus `approved` 2026-09-15 oleh pemilik modul; `LAB-STATE-v1`
`r3` bagian 1a `approved` pada tanggal yang sama; task berada di kepala gelombang tanpa dependency;
`HEAD` `e2152709` cocok dengan `backend_commit_sha` pada manifest revisi 34.

---

## 1. Masalah yang diperbaiki

**Keadaan sebelum perubahan.** Di laboratorium, sebelum sebuah pesanan dikerjakan, ada satu langkah
yang nyata dilakukan orang tetapi tidak punya tempat di sistem: **petugas laboratorium memeriksa
pesanan yang masuk, memilih dokter pemeriksa, lalu mengonfirmasi bahwa pesanan itu benar dan siap
dikerjakan.** Sistem tidak menyimpan satu pun jejaknya — tidak siapa yang mengonfirmasi, tidak kapan,
dan tidak dokter pemeriksa mana yang dipilih.

**Akibat nyatanya.** Ketika kelak muncul pertanyaan "siapa yang menyatakan pesanan ini siap
dikerjakan?", jawabannya hanya ada di ingatan orang. Pada daftar pesanan, kolom dokter pemeriksa
tidak dapat ditampilkan sama sekali, dan pada ringkasan cetak hasil, nama dokter pemeriksa harus
ditulis di luar sistem.

**Contoh konkretnya.** Pada `QuilvianNewDevYoga` hari ini ada 5 pesanan: 2 berstatus `Requested`,
2 berstatus `Accepted`, 1 berstatus `InProcess`. Dua pesanan yang sudah berstatus `Accepted` itu
pasti pernah dilihat dan diteruskan seseorang — tetapi tidak ada satu baris pun di database yang
dapat menyebutkan siapa orangnya.

**Yang ditutup task ini.** Pertanyaan posisi `Confirmed` terbuka sejak 2026-09-01 sebagai
`LAB-P0-002`, dan dijawab `LAB-DEC-061` pada 2026-09-15: `Confirmed` berdiri **antara** `Requested`
dan `Accepted`. Task ini mendirikan **tempat penyimpanannya** — bukan aksinya.

**Batas task ini, supaya tidak disalahpahami.** Sesudah task ini selesai, belum ada satu pun cara
mengonfirmasi pesanan. Tombolnya, endpointnya, dan validasinya adalah `BE-LAB-31`. Task ini
menyiapkan lemarinya; yang mengisi lemari itu task berikutnya.

---

## 2. Proses bisnis

### 2.1 Alur kerja yang dituju, berurutan

1. Dokter memesan pemeriksaan laboratorium. Pesanan lahir berstatus **`Requested`**.
2. Petugas laboratorium membuka pesanan yang masuk, memeriksa isinya, lalu **memilih dokter
   pemeriksa**.
3. Petugas menekan Konfirmasi. Pesanan berpindah ke **`Confirmed`**. Sistem merekam tiga hal
   sekaligus: **siapa** yang mengonfirmasi — diambil dari pengguna yang sedang login, bukan dari
   isian layar — **kapan**, dan **dokter pemeriksa mana** yang dipilih.
4. Wadah pertama pesanan itu dinyatakan layak. Pesanan berpindah otomatis ke **`Accepted`**.
5. Pemeriksaan dikerjakan — `InProcess` — lalu diselesaikan — `Completed`.

Langkah 2 sampai 4 baru dapat berjalan sesudah `BE-LAB-31` berdiri. Yang ditegakkan task ini adalah
langkah 3 punya tempat menulis.

### 2.2 Jalur tidak normal yang sudah diputuskan

| Keadaan | Yang terjadi | Dasar |
| --- | --- | --- |
| Pesanan tidak pernah dikonfirmasi, lalu wadah pertamanya dinyatakan layak | **Tetap sah.** Pesanan berpindah `Requested` → `Accepted` seperti sebelumnya | `LAB-STATE-v1` `r3` bagian 1a, alinea "`Requested` → `Accepted` tetap sah" |
| Pesanan yang sudah `Confirmed` dikonfirmasi lagi | Ditolak `409` — konfirmasi hanya sah sekali | `LAB-STATE-v1` `r3`; ditegakkan `BE-LAB-31` |
| Pesanan sudah `Accepted`, `InProcess`, `Completed`, atau `Cancelled` lalu dikonfirmasi | Ditolak `409` — sudah melewati tahap konfirmasi | `LAB-STATE-v1` `r3`; ditegakkan `BE-LAB-31` |
| Konfirmasi tanpa memilih dokter pemeriksa | Ditolak | `AC-95`, `VAL-72`; ditegakkan `BE-LAB-31` |

### 2.3 Kenapa jalur lama sengaja tidak dicabut

Mewajibkan konfirmasi berarti mengetatkan jalur yang sedang dipakai: kedua pesanan yang hari ini
berstatus `Requested` — beserta wadah yang berjalan di atasnya — akan berhenti dapat diproses sampai
seseorang mengonfirmasinya satu per satu. Keputusan menjadikan konfirmasi wajib dicatat terpisah
sebagai `LAB-OPEN-027` dan **belum** diambil.

### 2.4 Kenapa ketiga kolomnya boleh kosong

Kelima pesanan yang sudah ada tidak pernah dikonfirmasi — itu kenyataan, bukan data yang hilang.
Kolom wajib hanya menyisakan dua pilihan, dan keduanya buruk: migrationnya gagal, atau seseorang
mengarang nilai bawaan atas pesanan yang benar-benar sudah terjadi. `column_default = NULL` adalah
cara menuliskan kenyataan itu apa adanya. Pelajaran yang sama dengan `BE-EXT-04`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `AGENTS.md`, `CLAUDE.md` | Governance repository, routing skill, larangan otomatisasi |
| `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` | QBE ID yang berlaku |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Prefix `Lab`, lifecycle `ACTIVE`, wewenang migration |
| `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `DATABASE_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md` | Siklus kerja, klasifikasi, disiplin persistence, kriteria penyelesaian, bentuk laporan |
| `roadmap/backend-roadmap.md` bagian 6d | Cakupan, DoD, dan verifikasi task |
| `contracts/state-transition-matrix.md` bagian 1a | Posisi `Confirmed` dan transisi yang sah |
| `erd/data-dictionary.md` bagian 1 | Nama, tipe, index, dan relasi ketiga kolom |
| `00-interview-decisions.md` | `LAB-DEC-061`, `AC-94`, `AC-95`, `LAB-P0-002` |
| `blueprint-manifest.md` | Kesesuaian `backend_commit_sha` dengan `HEAD` |
| `Areas/.../Models/LabOrder.cs` | Pola kolom, komentar, dan urutan properti |
| `Areas/.../Enums/LaboratoryEnums.cs` | Nilai enum yang sudah terpakai |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Pola konfigurasi, index, dan foreign key |
| `Areas/.../Services/LabFilterMetadataFactory.cs` | Dampak penambahan nilai enum pada daftar pilihan penyaring |
| `Areas/.../Services/LabOrderService.cs` | Dampak penambahan status pada rekap dan pada seluruh pembanding status |
| `Areas/.../Services/LabExaminationService.cs` | Pembanding status lain yang mungkin terdampak |
| `Migrations/20260915083316_AddKioskServiceTargetAndPhysicianRequest.cs` | Konvensi penulisan migration terdekat |
| `Repositories/Configurations/Corporate/.../MstDoctorConfiguration.cs` | Nama tabel tujuan foreign key |
| `task/report/backend/BE-LAB-01.md`, `BE-EXT-04.md`, `BE-LAB-26.md` | Preseden pembuktian migration dan eksekusi database |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs` | Nilai `Confirmed = 9` pada `LabOrderStatus`, beserta keterangan kenapa angkanya 9 dan bukan 3 |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | Tiga properti baru: `ConfirmedByUserId`, `ConfirmedAt`, `ExaminerDoctorId` — ketiganya nullable |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Ketiga kolom dinyatakan tidak wajib; foreign key `ExaminerDoctorId` ke `MstDoctor` ber-`Restrict`; index `IX_LabOrder_ExaminerDoctorId`; satu `using` untuk `MstDoctor` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabFilterMetadataFactory.cs` | Satu baris: label Bahasa Indonesia **"Dikonfirmasi"** untuk `Confirmed` |
| `Migrations/20260916022245_AddLabOrderConfirmation.cs` dan `.Designer.cs` | Migration baru — tiga kolom, satu index, satu foreign key; `Down` membalik seluruhnya |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Snapshot model menyusul migration |

### 3.3 Kenapa angka enumnya 9, bukan 3

`LAB-STATE-v1` menempatkan `Confirmed` **antara** `Requested` dan `Accepted`. Itu urutan **alur
kerja**, bukan urutan angka. Nilai `LabOrderStatus` dipersistensi sebagai `integer` pada kolom
`public."LabOrder"."OrderStatus"`, dan `Accepted` sudah bernilai `3` pada baris yang sudah tersimpan.

Bila `Confirmed` disisipkan sebagai `3`, seluruh nilai sesudahnya bergeser satu:

| Pesanan di database | Angka tersimpan | Dibaca sebelum penyisipan | Dibaca sesudah penyisipan |
| --- | :---: | --- | --- |
| 2 pesanan | `3` | `Accepted` | **`Confirmed`** — salah |
| 1 pesanan | `4` | `InProcess` | **`Accepted`** — salah |

Tiga dari lima pesanan akan berubah artinya tanpa satu baris pun berubah isinya. Karena itu nilainya
ditambahkan di ujung: `Confirmed = 9`. Urutan alur kerja tetap dijaga matriks transisi, bukan oleh
urutan angkanya.

### 3.4 Satu baris di luar daftar cakupan roadmap, beserta alasannya

Roadmap menulis cakupan task ini "satu nilai enum, tiga kolom nullable, satu migration, satu
configuration". Satu berkas lagi ikut berubah: `LabFilterMetadataFactory.cs`, **satu baris**.

Alasannya bukan kerapian. `LabFilterMetadataFactory` membangun daftar pilihan penyaring dengan
menelusuri **seluruh** nilai enum, lalu memberi setiap nilai label Bahasa Indonesia. Menambahkan
`Confirmed` tanpa menambahkan labelnya membuat daftar pilihan status pada layar daftar pesanan
memunculkan satu pilihan berlabel **"Confirmed"** dalam bahasa Inggris, di tengah delapan pilihan
lain yang seluruhnya berbahasa Indonesia. Barisnya ditambahkan supaya labelnya berbunyi
**"Dikonfirmasi"**.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol endpoint baru dan nol perubahan bentuk pesan.** Satu payload yang sudah ada bertambah isinya: `GET /api/v1/health-services/laboratory-management/lab-orders/filters/metadata` kini mengembalikan **9 pilihan status**, bukan 8 — pilihan ke-9 bernilai `9`, bernama `Confirmed`, berlabel `Dikonfirmasi`. Bentuk larik dan bentuk setiap elemennya tidak berubah. `LAB-API-v1` **tidak** dinaikkan revisinya karena kontrak itu tidak pernah mencacah isi daftar enum |
| Database | **Aditif seluruhnya.** Tiga kolom nullable tanpa nilai bawaan pada `public."LabOrder"`, satu index btree, satu foreign key ber-`ON DELETE RESTRICT` ke `public."MstDoctor"`. **Diterapkan ke `QuilvianNewDevYoga`** atas wewenang eksplisit pemilik pada sesi ini; jalur `Down` lalu `Up` dibuktikan. Lihat bagian 5.2 |
| Keamanan/Auth | `NOT APPLICABLE`. Nol permission, nol `[AccessPermission]`, dan nol aturan otorisasi yang berubah. Kolom `ConfirmedByUserId` dirancang diisi server dari pengguna yang login — penegakannya ada pada `BE-LAB-31` |

---

## 4. Dokumentasi endpoint

Task ini **tidak menambah maupun mengubah satu pun endpoint**. Satu endpoint yang sudah ada berubah
**isi** jawabannya, dan dicatat di sini supaya tidak ditemukan sebagai kejutan:

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders/filters/metadata` | Daftar pilihan penyaring layar daftar pesanan. **Pilihan status bertambah satu**: `Dikonfirmasi` bernilai `9` | `LabOrder : Read` |

Endpoint konfirmasi `POST /lab-orders/{id}/confirm` yang dijanjikan `LAB-API-v1` `r12` §7.1 **belum
ada** dan memang bukan cakupan task ini — ia milik `BE-LAB-31`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 Error, 207 Warning** | `PASS` | Jumlah warning **sama persis** dengan build terakhir sebelum task ini (`BE-EXT-04`, 207). Nol warning berasal dari keempat berkas yang diubah |
| `dotnet ef migrations add AddLabOrderConfirmation` | Migration terbentuk; delta modelnya **hanya** tiga kolom, satu index, satu foreign key | `PASS` | Bagian 5.1 |
| `dotnet ef migrations list` sebelum penerapan | Tepat **satu** migration `Pending` — milik task ini | `PASS` | Nol migration modul lain ikut terbawa |
| `dotnet ef database update` — jalur `Up` | `Done.` | `PASS` | Bagian 5.2 |
| Ketiga kolom berdiri sebagai nullable tanpa nilai bawaan | `is_nullable = YES`, `column_default = NULL` untuk ketiganya | `PASS` | Bagian 5.2 |
| Index dan foreign key berdiri | `IX_LabOrder_ExaminerDoctorId` btree; `FOREIGN KEY ("ExaminerDoctorId") REFERENCES "MstDoctor"("Id") ON DELETE RESTRICT` | `PASS` | Bagian 5.2 |
| **Nol pesanan lama berubah nilainya** | 5 pesanan sebelum dan sesudah; sebaran status `Requested` 2, `Accepted` 2, `InProcess` 1 **identik** | `PASS` | Bagian 5.2 |
| **Ketiga kolom baru `null` pada seluruh pesanan lama** | `count("ConfirmedAt") = 0`, `count("ConfirmedByUserId") = 0`, `count("ExaminerDoctorId") = 0` dari 5 baris | `PASS` | Bagian 5.2 |
| Nol pesanan berstatus `Confirmed` sesudah migration | `0` baris ber-`OrderStatus = 9` | `PASS` | Sesuai harapan — belum ada jalur yang dapat menuliskannya |
| **Jalur `Down` dibuktikan** | Ketiga kolom, index, dan foreign key hilang; baris riwayat migration terhapus; **5 pesanan tetap utuh dengan sebaran status yang sama** | `PASS` | Bagian 5.2 |
| **Jalur `Up` ulang dibuktikan** | Ketiganya berdiri lagi; `migrations list` menunjukkan **0** `Pending` | `PASS` | Bagian 5.2 |
| Uji integrasi otomatis | Tidak dijalankan | `NOT RUN` | Repository ini belum memiliki project test. Sudah dicatat `AGENTS.md` bagian *Platform Backend Saat Ini*; bukan kondisi yang diperkenalkan task ini |
| Uji lewat HTTP sungguhan beserta `[Authorize]` dan `[AccessPermission]` | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan. Hak akses tidak berubah, dan satu-satunya perubahan payload adalah bertambahnya satu pilihan penyaring |
| `T-94a`..`T-95b`, `T-97c` | Tidak dijalankan | `NOT RUN` | Seluruhnya menguji **aksi** konfirmasi, yang endpointnya adalah `BE-LAB-31`. Tidak ada yang dapat diuji pada task ini |

Uji manual lewat antarmuka: `NOT FEASIBLE` — belum ada layar maupun endpoint konfirmasi.

### 5.1 Bukti bahwa migration tidak menyapu perubahan model lain

Berkas `Designer.cs` sebuah migration memuat snapshot model **sesudah** migration itu. Membandingkan
snapshot migration ini dengan snapshot migration sebelumnya menunjukkan persis apa yang menurut EF
berubah:

| Baris yang bertambah | Isi |
| --- | --- |
| `b.Property<DateTime?>("ConfirmedAt")` | `timestamp with time zone` |
| `b.Property<Guid?>("ConfirmedByUserId")` | `uuid` |
| `b.Property<Guid?>("ExaminerDoctorId")` | `uuid` |
| `b.HasIndex("ExaminerDoctorId")` | — |
| `b.HasOne(MstDoctor, null).WithMany().HasForeignKey("ExaminerDoctorId").OnDelete(DeleteBehavior.Restrict)` | — |

Tidak ada baris lain. Ini penting karena snapshot model adalah berkas yang paling mudah membawa
perubahan modul lain tanpa disadari.

### 5.2 Bukti eksekusi migration

**Wewenang.** Pemilik menunjuk `QuilvianNewDevYoga` sebagai targetnya pada sesi ini, sesudah
ditanyakan secara terpisah. Wewenang itu mencakup penerapan migration task ini saja.

**Gerbang yang diperiksa lebih dulu.** `dotnet ef migrations list` menunjukkan **tepat satu**
migration `Pending` — milik task ini. Pelajaran `BE-LAB-26`, yang menemukan tujuh `Pending`
sekaligus, dipakai sebagai pemeriksaan wajib sebelum eksekusi, bukan sesudahnya.

**Cara pembuktiannya.** `psql` tidak terpasang pada mesin ini, sehingga pemeriksaan schema dan data
dilakukan lewat runner Npgsql sementara **di luar repository**, yang membaca connection string saat
berjalan dan **menolak berjalan bila nama database tujuannya bukan `QuilvianNewDevYoga`**. Runner itu
tidak menulis credential ke berkas mana pun. Cara yang sama dipakai `BE-LAB-01`.

**Urutan bukti yang direkam:**

| Langkah | Tiga kolom baru | `IX_LabOrder_ExaminerDoctorId` | `FK_LabOrder_MstDoctor_ExaminerDoctorId` | Jumlah pesanan | Sebaran status |
| --- | --- | --- | --- | :---: | --- |
| Sebelum apa pun dijalankan | TIDAK ADA | TIDAK ADA | TIDAK ADA | 5 | `Requested` 2, `Accepted` 2, `InProcess` 1 |
| Sesudah `Up` | ADA, ketiganya `is_nullable = YES`, `column_default = NULL` | ADA, btree | ADA, `ON DELETE RESTRICT` | 5 | **identik** |
| Sesudah `Down` | TIDAK ADA | TIDAK ADA | TIDAK ADA | 5 | **identik** |
| Sesudah `Up` ulang | ADA | ADA | ADA | 5 | **identik** |

**Isi ketiga kolom baru pada kelima pesanan lama:** `null` seluruhnya — `count("ConfirmedAt") = 0`,
`count("ConfirmedByUserId") = 0`, `count("ExaminerDoctorId") = 0` dari 5 baris. Inilah butir DoD
"nol pesanan lama berubah nilainya", dan ia terbukti dari data, bukan disimpulkan dari bentuk
migrationnya.

**Keadaan akhir database: termigrasi.** `dotnet ef migrations list` sesudahnya menunjukkan **0**
migration `Pending`.

**Catatan keadaan data.** Berbeda dengan `BE-LAB-01` yang berjalan saat `public."LabOrder"` masih
kosong, tabel ini kini berisi **5 pesanan nyata**. Risiko "menambah kolom pada tabel berisi data"
yang dicatat roadmap karena itu benar-benar teruji di sini, bukan lolos karena tabelnya kosong.
Menambahkan kolom nullable tanpa nilai bawaan pada PostgreSQL adalah operasi metadata dan tidak
menulis ulang baris yang ada — dan kelima baris itu terbukti utuh sesudahnya.

**Tidak dijalankan:**

- **Eksekusi ke database selain `QuilvianNewDevYoga`.** Tidak ada database lain yang disentuh.
- **Migration modul lain.** Tidak ada yang `Pending`, sehingga tidak ada yang dapat ikut terbawa.
- **Perubahan data.** Nol baris `LabOrder` diubah isinya.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

`AC-94` dan `AC-95` adalah kriteria milik **`BE-LAB-31`**. Roadmap menulis peran task ini sebagai
**prasyarat** keduanya, bukan pemenuhnya.

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-94` — konfirmasi hanya sah sekali; nama konfirmator dan waktu konfirmasi terekam dan terbaca pada daftar | **Prasyarat terpenuhi; kriterianya sendiri belum** | Tempat penyimpanan `ConfirmedByUserId` dan `ConfirmedAt` berdiri dan terbukti di database. Aturan "hanya sekali" beserta jalur penulisannya adalah `BE-LAB-31` |
| `AC-95` — konfirmasi menolak bila dokter pemeriksa belum dipilih; dokter yang dipilih tampil pada daftar dan ringkasan cetak | **Prasyarat terpenuhi; kriterianya sendiri belum** | `ExaminerDoctorId` berdiri beserta foreign key ke `MstDoctor` dan indexnya. Penolakan `VAL-72` adalah `BE-LAB-31`; penampilan pada daftar dan cetak adalah `FE-LAB-15` |

### 6.2 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Ketiga kolom berdiri | **Terpenuhi** | `information_schema.columns` membaca ketiganya; ketiganya `is_nullable = YES` dengan `column_default = NULL` |
| Nilai enum `Confirmed` ada | **Terpenuhi** | `LabOrderStatus.Confirmed = 9`; build hijau; nilainya ikut muncul sebagai pilihan penyaring berlabel `Dikonfirmasi` |
| **Nol pesanan lama berubah nilainya** | **Terpenuhi** | 5 pesanan sebelum, sesudah `Up`, sesudah `Down`, dan sesudah `Up` ulang; sebaran status identik di keempat titik; ketiga kolom baru `null` pada seluruh 5 baris |
| **`Down` terbukti** | **Terpenuhi** | Ketiga kolom, index, dan foreign key hilang sesudah `Down`, lalu berdiri lagi sesudah `Up`; data tetap utuh di setiap langkah |
| Migration diterapkan | **Terpenuhi** | `dotnet ef database update` menjawab `Done.`; `migrations list` menunjukkan 0 `Pending` |
| Build | **Terpenuhi** | 0 Error, 207 Warning — sama persis dengan baseline |

Seluruh butir DoD terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 207 warning, **seluruhnya sudah ada sebelum task ini** dan tidak satu pun berasal dari keempat berkas yang diubah. Jumlahnya tidak bertambah maupun berkurang |
| Masalah yang diketahui | **Rekap pesanan belum mengenal `Confirmed`.** `LabOrderService.GetSummaryAsync` mencacah pesanan ke dalam delapan ember status yang **tetap** — `Draft`, `Diminta`, `Diterima`, `SedangDikerjakan`, `Selesai`, `Ditahan`, `PembatalanDiminta`, `Dibatalkan` — sementara `TotalPesanan` mencacah seluruhnya. Hari ini tidak berdampak karena nol pesanan dapat berstatus `Confirmed`. **Begitu `BE-LAB-31` berdiri, jumlah kedelapan ember itu tidak akan lagi sama dengan `TotalPesanan`**, dan selisihnya persis pesanan yang sedang menunggu dikerjakan. Sengaja tidak diperbaiki di sini: menambah ember berarti menambah ruas pada `LabOrderSummaryResponse`, dan itu perubahan kontrak `LAB-API-v1` yang tidak diberi wewenang pada task ini |
| Risiko tersisa | **Pertama**, `QuilvianNewDevYoga` sudah menerima migration ini, tetapi **database lain belum** — menjalankan kode ini terhadap database yang belum dimigrasi akan gagal pada setiap query `LabOrder`. Berkas migrationnya tersedia; penerapannya ke database lain adalah wewenang tersendiri. **Kedua**, `ExaminerDoctorId` ber-`Restrict` berarti seorang dokter yang pernah menjadi dokter pemeriksa tidak akan dapat dihapus keras dari `MstDoctor`. Itu memang yang diinginkan, dan penghapusan di aplikasi ini bersifat penandaan `IsDelete` — bukan penghapusan baris — sehingga tidak ada alur yang terganggu hari ini. **Ketiga**, daftar pilihan penyaring status kini memuat `Dikonfirmasi` walaupun belum ada satu pun pesanan yang dapat berstatus itu; menyaringnya akan menghasilkan daftar kosong sampai `BE-LAB-31` berdiri |
| Perubahan sampingan | `NONE`. Perubahan yang sudah ada di working tree sebelum task ini — `BE-LAB-26`..`BE-LAB-29`, `BE-EXT-04`, `BE-EXT-04b`, beserta artefak blueprintnya — tidak disentuh |
| Interupsi | **Satu kejadian, dipulihkan penuh.** Lihat bagian 7.2 |
| Status Git | Lihat bagian 7.1 |
| Langkah berikutnya | **1.** `BE-LAB-31` — endpoint `POST /lab-orders/{id}/confirm` beserta `VAL-70`..`VAL-73`; kini tidak lagi tertahan. **2.** `BE-LAB-32` — pembatalan wajib beralasan; kini tidak lagi tertahan, tetapi roadmap mewajibkan **hitungan pesanan berstatus `Accepted`, `InProcess`, dan `OnHold` dilaporkan sebelum `VAL-75` ditegakkan**; angkanya hari ini: `Accepted` 2, `InProcess` 1, `OnHold` 0 — artinya **3 pesanan** akan kehilangan kemampuan dibatalkan. **3.** Putuskan bagaimana `Confirmed` dicacah pada rekap pesanan, sebaiknya bersamaan dengan `BE-LAB-31`. **4.** Terapkan migration ini ke database lain yang membutuhkannya, lewat wewenang tersendiri |

### 7.1 Status Git di akhir pekerjaan

Perubahan milik task ini:

```text
 M Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs
 M Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabFilterMetadataFactory.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/Configurations/HealthServices/LabOrderConfiguration.cs
?? Migrations/20260916022245_AddLabOrderConfirmation.cs
?? Migrations/20260916022245_AddLabOrderConfirmation.Designer.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-30.md
```

Ditambah pembaruan artefak blueprint: `roadmap/backend-roadmap.md` dan `roadmap/traceability.md`.

Berkas `LaboratoryEnums.cs` dan `ApplicationDbContextModelSnapshot.cs` **sudah** berstatus `M` sebelum
task ini dimulai, karena pekerjaan `BE-LAB-26`..`BE-LAB-29` dan `BE-EXT-04` belum di-commit. Bagian
milik task ini pada kedua berkas itu terpisah jelas dan tidak menimpa apa pun.

Nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, dan `deploy` dijalankan.

### 7.2 Berkas roadmap sempat rusak dan dipulihkan

Ini dicatat karena kerusakan yang tidak ditulis akan terulang.

**Apa yang terjadi.** Saat menandai status task pada `roadmap/backend-roadmap.md`, dipakai perintah
penyuntingan baris `perl -i -pe` dengan pola yang salah tanda kutipnya. Pola itu **cocok dengan
setiap baris**, bukan dengan satu baris yang dituju. Akibatnya teks pengganti tersisip di depan
**seluruh 1786 baris** berkas itu, dan seluruh isinya ikut ter-*encode* ganda — huruf beraksen dan
tanda pisah panjang berubah menjadi karakter kacau.

**Kenapa tidak dipulihkan lewat Git.** Berkas itu **sudah** berstatus berubah sebelum task ini
dimulai: ia memuat pekerjaan sesi-sesi sebelumnya — seluruh bagian 6c dan 6d, baris status
`BE-LAB-26`..`BE-LAB-29`, `BE-EXT-04`, `BE-EXT-04b`, dan sembilan entri riwayat revisi — yang belum
di-*commit*. Mengembalikannya ke versi `HEAD` akan membuang **417 baris pekerjaan orang lain**.
Karena itu jalur itu tidak diambil.

**Bagaimana dipulihkan.** Kerusakannya ternyata berlapis rapi dan dapat dibalik: teks yang tersisip
menempel di **depan** setiap baris, dan isi aslinya masih utuh di belakangnya. Pemulihannya dua
langkah, dijalankan lewat alat sementara di luar repository terhadap **salinan**, bukan terhadap
berkas aslinya:

1. Membuang teks sisipan dari setiap baris — **1786 dari 1786 baris** terbukti memilikinya.
2. Membalik *encode* ganda, dengan memetakan kembali setiap karakter ke byte aslinya.

**Bagaimana pemulihannya diperiksa sebelum dipasang.** Hasilnya dibandingkan terhadap versi `HEAD`:
selisihnya **422 baris dalam 8 kelompok**, dan setiap kelompok cocok dengan pekerjaan sesi
sebelumnya yang memang seharusnya ada — bagian 6c dan 6d (+390 baris), sepuluh baris tabel status,
dan sembilan entri riwayat revisi. Nol karakter kacau tersisa, dan berkasnya kembali terbaca sebagai
UTF-8 yang sah. Baru sesudah itu hasilnya dipasang.

**Satu baris yang isinya benar-benar hilang**, dan itu disebutkan supaya tidak ada yang mengira
pemulihannya sempurna: baris tabel status `BE-LAB-30` sendiri — satu-satunya baris yang polanya
memang dituju. Baris itu berbunyi `| BE-LAB-30 | MVP-5c | EPIC-LAB-12 | Belum dikerjakan | Tidak ada
— terbuka |`, dan memang baris itulah yang hendak diganti. Ia ditulis ulang sebagai baris berstatus
`SELESAI`.

**Pelajarannya.** Penyuntingan baris berpola terhadap berkas dokumentasi panjang yang memuat
pekerjaan belum di-*commit* tidak dilakukan lagi di sini; penyuntingan yang cocok-persis dipakai
sebagai gantinya. Seluruh perubahan roadmap sesudah kejadian ini ditulis dengan cara itu.
