# Laporan Perubahan Backend — `BE-RWI-074`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-074` |
| Judul | Penugasan dokter mengenal DPJP, konsulen, dan dokter jaga |
| Slice | `S9. Gelombang 1A — Rawat Inap Safety Corrections` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian S9 |
| Trace | `RWI-DEC-099`; `RWI-DEC-102`; `RWI-DEC-105`; `FR-RI-185` s.d. `FR-RI-190`; `04-prd-to-mvp.md` bagian 21.3; `OPEN-MVP-004` |
| Contract version | `data/data-dictionary.md` bagian 2 dan 2.1; `02-backend-architecture.md` revision `0.7` bagian 0.2 s.d. 0.4; `contracts/state-transition-matrix.md` `0.8.0` bagian 6A; `contracts/permission-audit-matrix.md` bagian 4-A. Seluruhnya **disetujui** 11 September 2026 lewat `RWI-DEC-105` |
| Dependency | Approval kontrak `0.8.0` — **terpenuhi**. Nol prasyarat task lain; boleh paralel dengan `BE-RWI-073` ✅ |
| Klasifikasi | `MEDIUM` — satu kolom, satu enum, dua index, satu migration, dua belas berkas source; nol endpoint baru, nol tabel baru, nol hak akses baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/`, `Repositories/Configurations/`, `Migrations/`, dan `docs/module-blueprints/rawat-inap/episode-rawat-inap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `c11904ea99dc0f9a9ecfb3b41f6edb2bf500d206`, branch `MHamzah` |
| Tanggal | 11 September 2026 |
| Status | ✅ **SELESAI 11 September 2026.** Kesembilan acceptance criteria terpetakan ke source yang benar-benar ada, dan migration sudah dibuat tanpa diterapkan ke environment mana pun. **Tiga butir verifikasi tidak dijalankan dan ditulis apa adanya:** `dotnet build` `NOT RUN` — dikecualikan atas instruksi pemilik pada task aktif; serta `RWI-AC-084b`, `RWI-AC-084c`, dan `RWI-AC-084h` `NOT RUN` karena penerapan migration adalah wewenang terpisah. **Satu butir cakupan sengaja tidak dikerjakan:** jalur tulis lewat endpoint untuk konsulen dan dokter jaga, karena berada di luar kolom `Cakupan` task ini dan di luar `api-contract.md` `0.8.0` — lihat bagian 7.1 |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` / Inpatient |
| Submodule | `Models`, `Enums`, `DTOs`, `Services`; ditambah `Repositories/Configurations/HealthServices/InPatientManagement/` |
| Pemilik/prefix registry | `Inp`, lifecycle **`ACTIVE`** sejak 24 Agustus 2026 — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 23, kenaikan lifecycle baris 101 atas keputusan Muhammad Hamzah lewat `RWI-DEC-068` |
| Keberlakuan | `TOUCHED LEGACY` untuk `InpDoctorAssignment` beserta configuration dan service pembacanya; `NEW CODE` untuk enum `InpDoctorAssignmentRole` dan berkas migration |
| Status registry | Entri `ACTIVE`, sehingga `QBE-MOD-002` **tidak** menahan pekerjaan ini. Nol modul baru, nol prefix baru, nol folder baru yang perlu didaftarkan |
| QBE ID yang benar-benar berlaku | `QBE-ENT-002` semantik domain kolom baru; `QBE-CFG-001` dan `QBE-CFG-002` mapping, default, dan kedua index; `QBE-NAM-002` prefix `Inp` yang disetujui; `QBE-ENUM-001` enum dimiliki modulnya sendiri; `QBE-VAL-001` invarian `INV-INP-03` ditegakkan di database; `QBE-DTO-001` kontrak API tetap memakai DTO; `QBE-AUD-001` jejak audit tidak berubah |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001` — nol entity baru, `InpDoctorAssignment` sudah mewarisi `IdentityModel`. `QBE-NAM-001` dan `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION`, nol rename tabel. `QBE-CODE-001` s.d. `QBE-CODE-006` — nol nomor bisnis dialokasikan. `QBE-PERM-001` — nol butir hak akses baru |
| Peninggalan yang dicabut | Folder `agents/rules/` dan `.codex/` **tidak ditemukan** di working tree. Tidak ada yang perlu dilaporkan |

---

## 1. Masalah yang diperbaiki

Sampai hari ini tabel penugasan dokter rawat inap **hanya mengenal satu jenis dokter**. Setiap
baris di dalamnya berarti dokter penanggung jawab pelayanan, dan tidak ada kolom apa pun yang
dapat membedakan satu peran dari peran lainnya.

Akibatnya nyata dan terasa di ruangan:

- **Kepala ruangan tidak dapat melibatkan konsulen.** Satu-satunya cara mencatat bahwa dokter
  bedah ikut menangani seorang pasien adalah menjadikannya DPJP — dan itu berarti **menggusur**
  DPJP yang lama.
- **Dokter jaga tidak dapat dipanggil tanpa merusak catatan.** Panggilan malam hari yang hanya
  berlangsung satu shift akan tercatat sebagai pengalihan tanggung jawab yang permanen.
- **Matriks kewenangan tidak dapat diwujudkan sama sekali.** Kalimat "hanya DPJP yang boleh
  memutuskan pasien pulang" tidak dapat ditegakkan, karena sistem tidak punya cara membedakan
  DPJP dari dokter lain yang kebetulan juga punya penugasan.

Penghalangnya bukan sekadar kolom yang belum ada. Unique index parsial
`IX_InpDoctorAssignment_EpisodeId_Active` berbunyi "satu episode hanya boleh punya **satu**
penugasan terbuka". Selama tabel ini hanya menyimpan DPJP, bunyi itu benar. Begitu konsulen
pertama hendak disimpan, **database sendiri yang menolaknya** pada perintah `INSERT` kedua.

**Contoh konkret.** Ny. Sari dirawat dengan DPJP dr. Andi, penyakit dalam. Hari ketiga ia perlu
dikonsultasikan ke bedah. Hari ini petugas punya dua pilihan, dan keduanya salah:

| Pilihan | Akibatnya |
| --- | --- |
| Jadikan dr. Bima sebagai DPJP | dr. Andi berhenti menjadi penanggung jawab, padahal perawatan penyakit dalamnya masih berjalan. Keputusan pulang berpindah ke dokter bedah yang hanya menangani satu masalah |
| Biarkan dr. Bima tanpa penugasan | dr. Bima tidak dapat menulis catatan apa pun untuk pasien ini, karena sistem menganggapnya tidak berwenang |

Sesudah perubahan ini, keduanya tercatat berdampingan: dr. Andi tetap `Dpjp`, dr. Bima tercatat
`Consultant`, dan kewenangan keputusan pulang tetap berada pada dr. Andi.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Satu episode dapat menyimpan DPJP, konsulen, dan dokter jaga secara bersamaan, dan sistem dapat membedakan ketiganya |
| Pelaku | Petugas admisi menetapkan DPJP pertama; kepala ruangan atau supervisor mengalihkan DPJP, melibatkan konsulen, dan memanggil dokter jaga |
| Pemicu | Admisi dibuka, tanggung jawab dialihkan, konsultasi dibuka, atau dokter jaga dipanggil |
| Hasil akhir | Riwayat penugasan berperiode yang setiap barisnya menyebut perannya secara eksplisit |

### 2.2 Tiga peran dan artinya

| Peran | Nilai | Berapa yang boleh aktif | Boleh menulis dokumen klinis | Boleh memutuskan arah perawatan |
| --- | :---: | --- | :---: | --- |
| `Dpjp` | `1` | **Tepat satu** per episode | Ya | Ya |
| `Consultant` | `2` | Banyak, tidak dibatasi | Ya | **Tidak** |
| `OnCallDoctor` | `3` | Banyak, tidak dibatasi | Ya | **Tidak** |

**Kenapa konsulen tidak dibatasi jumlahnya.** Satu pasien dapat dikonsultasikan ke penyakit
dalam, bedah, dan anestesi sekaligus pada hari yang sama. Membatasinya menjadi satu berarti
memaksa kepala ruangan menutup konsultasi yang masih berjalan hanya supaya konsultasi berikutnya
dapat dibuat.

**Kenapa nilai `0` sengaja tidak dipakai.** Baris lama yang terisi nilai bawaan database bernilai
`1`, bukan `0`. Dengan begitu tidak ada satu pun baris yang dapat disalahartikan sebagai "peran
belum ditetapkan"; setiap baris punya peran yang eksplisit.

### 2.3 Alur normal — pengalihan DPJP

1. Kepala ruangan membuka layar episode dan memilih dokter penanggung jawab yang baru.
2. Sistem memeriksa pelakunya memang kepala ruangan atau supervisor, dan alasan pengalihan sudah
   diisi dengan kalimat yang dapat dibaca.
3. Di dalam **satu transaksi**: baris DPJP lama ditutup dengan mengisi waktu berakhirnya, lalu
   baris DPJP baru dibuka.
4. **Baris konsulen dan dokter jaga yang sedang aktif tidak disentuh sama sekali.** Inilah
   perubahan perilaku terpenting task ini — sebelumnya, pengalihan DPJP akan menutup setiap
   penugasan terbuka apa pun perannya.

### 2.4 Alur normal — admisi dibuka

Episode baru lahir bersama satu baris penugasan berperan `Dpjp`, dengan nomor urut `1`. Perannya
ditulis **eksplisit** di kode, bukan dibiarkan bersandar pada nilai bawaan database. Nilai bawaan
itu ada untuk baris lama, bukan sebagai pengganti nilai domain pada baris baru.

### 2.5 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Dua pengalihan DPJP tersimpan pada saat hampir bersamaan | Unique index parsial menolak yang kalah. Petugas menerima pesan "Pengalihan DPJP lain sedang tersimpan untuk episode ini. Muat ulang layar lalu coba lagi." |
| Episode sudah terlanjur punya lebih dari satu DPJP aktif | Pengalihan **ditolak** dengan pesan agar riwayat penugasannya dibetulkan supervisor lebih dulu. Menambah baris baru hanya akan memperdalam kerusakannya |
| Konsulen meminta keputusan pulang | **Ditolak `403`.** Ini keadaan **fail-closed** yang dinyatakan terbuka, bukan kebijakan yang sudah diputuskan — dilacak `OPEN-MVP-004` |
| Dokter jaga menandatangani resume | **Ditolak `403`** |
| Peran hendak diubah pada baris yang sama | **Tidak ada jalur yang menyediakannya.** Koreksi dilakukan dengan menutup baris lama lalu membuka baris baru |
| Episode ditutup | Seluruh penugasan yang masih terbuka ditutup pada waktu penutupan, **apa pun perannya** — perilaku ini sudah benar sebelum task ini dan sengaja tidak diubah |

**Kenapa peran tidak boleh diubah di tempat.** Mengubah `Consultant` menjadi `Dpjp` pada baris
yang sama akan **menulis ulang sejarah**: dokumen yang ditulis semasa ia konsulen mendadak
terbaca seakan ditulis DPJP. Penugasan adalah catatan berperiode, dan catatan berperiode
dikoreksi dengan menutup lalu membuka, bukan dengan menimpa.

### 2.6 Yang membedakan "menulis" dari "memutuskan"

Ini pembedaan paling mudah salah dipahami, jadi ditulis terpisah:

| Jenis tindakan | Siapa yang boleh | Dijaga di mana |
| --- | --- | --- |
| **Mencatat** — visite, catatan perkembangan, pengkajian | DPJP, konsulen, **dan** dokter jaga | `InpatientClinicalContextService.IsDoctorAssignedAsync`, sengaja **tanpa** saringan peran |
| **Memutuskan** — keputusan pulang, tanda tangan resume, perpindahan pasien, ubah kebutuhan isolasi | **Hanya DPJP** | `GUARD-INP-01` s.d. `GUARD-INP-04`, seluruhnya lewat `InpEpisodeService.IsActiveDoctorAsync` |

Konsulen dilibatkan untuk memberi pendapat pada satu masalah, bukan untuk mengambil alih
tanggung jawab pelayanan. Membuka `GUARD-INP-04` bagi konsulen, misalnya, berarti seorang dokter
yang dipanggil untuk satu konsultasi dapat mengubah status isolasi pasien yang bukan tanggung
jawabnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Dokumen tata kelola:**

- `AGENTS.md` backend
- `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- `rules/backend/TEST_POLICY.md`, `DATABASE_RULES.md`, `REPORT_TEMPLATE.md`, `role-access-rules.md`

**Kontrak modul:**

- `data/data-dictionary.md` bagian 2, 2.1, dan DDL `InpDoctorAssignment`
- `02-backend-architecture.md` revision `0.7` bagian 0.2 s.d. 0.5
- `contracts/state-transition-matrix.md` `0.8.0` bagian 6A.1 s.d. 6A.4
- `contracts/permission-audit-matrix.md` bagian 4-A.1 s.d. 4-A.4
- `contracts/api-contract.md` `0.8.0`
- `testing/acceptance-test-matrix.md` `0.8.0` bagian 4.1

**Source yang ditelusuri:** seluruh 26 titik pemakaian `InpDoctorAssignment` dan `DoctorAssignments`
pada `Areas/`, ditemukan lewat pencarian menyeluruh. Setiap titik dinilai satu per satu: apakah ia
berarti "DPJP" atau "dokter mana pun yang terlibat".

### 3.2 Berkas yang berubah

**Dua belas berkas.** Satu enum baru, satu migration baru, sepuluh berkas yang disunting.

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Enums/InpDoctorAssignmentRole.cs` | **Berkas baru.** Enum tiga nilai: `Dpjp = 1`, `Consultant = 2`, `OnCallDoctor = 3` |
| `Areas/HealthServices/InPatientManagement/Models/InpDoctorAssignment.cs` | Kolom `AssignmentRole` bertipe enum, wajib, bernilai bawaan `Dpjp`. Larangan mengubah peran di tempat ditulis pada dokumentasi propertinya |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpDoctorAssignmentConfiguration.cs` | Nilai bawaan database `1`; index unik `IX_InpDoctorAssignment_EpisodeId_Active` **diganti** `IX_InpDoctorAssignment_EpisodeId_ActiveDpjp` berfilter `"EndDateTime" IS NULL AND "AssignmentRole" = 1`; index pendukung `IX_InpDoctorAssignment_Episode_Doctor_Role_Period` ditambahkan |
| `Migrations/20260911000000_AddAssignmentRoleToInpDoctorAssignment.cs` | **Berkas baru.** Satu migration, tiga langkah yang urutannya mengikat, beserta langkah mundur yang dijaga |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Snapshot model EF disesuaikan: properti `AssignmentRole` dan kedua index |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Assignments.cs` | **Inti penjagaan.** `GetActiveDoctorIdAsync` dan `GetDoctorAssignmentAtAsync` menyaring `AssignmentRole = Dpjp`; pengalihan DPJP hanya menutup baris DPJP; baris baru menulis perannya eksplisit; kedua projection riwayat membawa perannya |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Baris DPJP saat admisi menulis perannya eksplisit; projection `ActiveDoctor` pada detail episode menyaring peran |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs` | `ActiveDoctorName` pada daftar episode menyaring peran |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Kolom `DoctorId` dan `DoctorName` pada census menyaring peran; saringan pencarian per dokter disamakan dengan kolom yang disaringnya |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeAssignmentDtos.cs` | `InpatientDoctorAssignmentResponse` membawa field `AssignmentRole` |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | `FindAttendingDoctorIdAsync` menyaring peran. `IsDoctorAssignedAsync` **sengaja dibiarkan tanpa saringan**, dan alasannya ditulis pada dokumentasinya supaya tidak "diperbaiki" orang berikutnya |
| `Areas/HealthServices/NutritionManagement/Services/NutritionDietService.cs` | `DoctorName` pada konteks diet pasien menyaring peran |

### 3.3 Dua berkas lintas modul, dan kenapa disentuh

`InpatientClinicalContextService` milik `ClinicalManagement` dan `NutritionDietService` milik
`NutritionManagement`. Keduanya **sudah** membaca `InpDoctorAssignment` sebelum task ini; yang
ditambahkan hanya satu syarat pada query yang sudah ada, tanpa endpoint baru, tanpa entity baru,
dan tanpa perpindahan kepemilikan apa pun.

Membiarkannya justru berbahaya. Kedua tempat itu menampilkan atau memakai **nama DPJP**. Tanpa
saringan peran, konsulen pertama yang tersimpan akan muncul sebagai penanggung jawab pasien pada
layar gizi dan pada konteks setiap dokumen klinis — kesalahan yang tidak menimbulkan error dan
karenanya tidak akan ada yang menyadarinya.

### 3.4 Satu keputusan pembacaan yang perlu disebut

Saringan pencarian census per dokter — "tampilkan pasien dokter ini" — sebelumnya membaca
penugasan **apa pun**. Saringan itu kini disamakan dengan kolom dokter yang ditampilkan di
sebelahnya, yaitu `Dpjp`.

Alasannya konsistensi yang dapat dibaca petugas: bila saringan membaca peran apa pun sementara
kolomnya menampilkan DPJP, hasil pencarian akan memuat baris yang kolom dokternya menyebut nama
**orang lain**. Ini keputusan pembacaan, bukan kebijakan kewenangan, dan tidak menutup satu pun
tindakan bagi siapa pun. Bila pemilik modul menghendaki konsulen dapat mencari pasiennya sendiri
lewat census, itu permukaan baca tersendiri dan perlu diputuskan terpisah.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu delta aditif.** `InpatientDoctorAssignmentResponse` bertambah field `AssignmentRole` bertipe `int`. Nol endpoint baru, nol endpoint berubah, nol field dihapus, nol field berganti nama. Ini **penambahan yang kompatibel**: pemanggil lama yang tidak membaca field baru tidak terpengaruh sama sekali. `contracts/api-contract.md` `0.8.0` **belum memuat baris ini** — lihat bagian 7 |
| Database | **Satu migration dibuat, NOL environment disentuh.** Satu kolom ditambahkan, satu index unik diganti, satu index pendukung dibuat. Nol tabel baru, nol kolom dihapus, nol data bisnis diubah selain pengisian peran `1` pada baris lama |
| Keamanan/Auth | **Nol `[AccessAction]` baru, nol `[AccessPermission]` baru, nol butir hak akses baru.** Yang berubah adalah pemeriksaan hubungan pelaku dengan pasien di dalam service, dan itu memang tidak pernah dapat diwakili butir hak akses. Arahnya **mengetatkan**: empat penjaga yang dulu menerima "punya penugasan aktif" kini menuntut "berperan DPJP". Nol `IsInRole`, nol nama peran, nol nama departemen, dan nol `UserType` ditambahkan pada berkas mana pun |

---

## 4. Dokumentasi endpoint

**`NOT APPLICABLE` untuk endpoint baru** — task ini nol endpoint baru.

Dua endpoint yang sudah ada **berubah isi jawabannya** karena field `AssignmentRole` bertambah,
jadi keduanya dicatat di sini.

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/doctor-assignments` | Mengalihkan DPJP. Menutup baris DPJP lama dan membuka baris DPJP baru dalam satu transaksi. **Sejak task ini, konsulen dan dokter jaga yang sedang aktif tidak ikut tergusur.** Jawabannya kini menyebut `AssignmentRole` | `InpatientEpisode : Update` |
| `GET` | `/{id}/doctor-assignments` | Riwayat penugasan dokter satu episode, urut nomor urut. Setiap baris kini menyebut `AssignmentRole`, sehingga DPJP, konsulen, dan dokter jaga dapat dibedakan pada layar | `InpatientEpisode : Read` |

Kedua hak akses **tidak berubah satu karakter pun**. Argumen `[AccessPermission]` tetap cocok
dengan `ControllerName` pada `[AccessController]` dan dengan argumen pertama `[AccessAction]`
pada method yang sama, sehingga keduanya tetap dapat dicentang admin pada layar
Pengaturan → Manajemen Role → Akses Role.

---

## 5. Verifikasi

### 5.1 Yang benar-benar dijalankan

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <11 berkas>` mode `ReportOnly` | `Files evaluated: 11`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Findings: none`, `Final result: PASS` | `PASS` | Keluaran perintah |
| Perintah yang sama mode `Strict` | `VIOLATION: 0`, `Findings: none`, `Final result: PASS`, nol QBE ID yang memblokir | `PASS` | Keluaran perintah |
| Preflight registry — prefix `Inp` berwenang | Baris registry ditemukan, lifecycle `ACTIVE` | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 23 dan 101 |
| Review diff/scope | 12 berkas milik task ini — 10 disunting, 2 baru. Perubahan milik task ini: nol endpoint, nol tabel, nol hak akses, nol perubahan di luar scope | `PASS` | `git status --porcelain` dan `git diff --stat` |
| **AC 7** — pencarian setiap penulisan `AssignmentRole` pada seluruh `Areas/` dan `Repositories/` | **Tepat dua** penulisan, keduanya pada **pembuatan baris baru**: `InpEpisodeService.cs:305` saat admisi dan `InpEpisodeService.Assignments.cs:388` saat pengalihan. **Nol** jalur yang mengubah peran pada baris yang sudah ada | `PASS` | Pencarian source |
| **AC 9** — penelusuran keempat penjaga | Keempatnya — `InpBedOccupancyService.cs:972`, `InpDischargeService.cs:151`, `:380`, `:495`, dan `InpEpisodeService.Assignments.cs:190` — memanggil `IsActiveDoctorAsync`, yang seluruhnya bermuara pada `GetActiveDoctorIdAsync`. Saringan `AssignmentRole = Dpjp` dipasang di satu tempat itu, sehingga keempat penjaga ikut berubah serentak | `PASS` | Pencarian source dan pembacaan jalur panggilan |
| Pemeriksaan sisa nama index lama pada source aplikasi | **Nol** pemakaian aktif. Sisanya hanya berkas `*.Designer.cs` historis — snapshot model masa lalu yang memang tidak boleh ditulis ulang — ditambah `Up()` dan `Down()` migration baru yang memang harus menyebutnya | `PASS` | Pencarian source |
| Pemeriksaan hardcode role access pada berkas yang disentuh | **Nol** `IsInRole`, nama peran, nama departemen, nama posisi, atau `UserType` ditambahkan | `PASS` | Pencarian source |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database yang sudah menerima
migration ini, dan penerapan migration adalah wewenang terpisah yang belum diberikan.

### 5.2 Contoh berangka sebagai pengganti test otomatis

Mengikuti `TEST_POLICY.md` bagian 5, perilaku ditelusuri pada source beserta contoh berangka.

**Episode `INP-2026-0912`, DPJP dr. Andi sejak 9 September pukul 08:00.**

| Langkah | Baris yang tersimpan | Yang dijaga index |
| ---: | --- | --- |
| 1 | Nomor urut 1 — dr. Andi, `AssignmentRole = 1`, berakhir kosong | Satu-satunya baris `Dpjp` terbuka. Sah |
| 2 | Nomor urut 2 — dr. Bima, `AssignmentRole = 2`, berakhir kosong | Filter index **tidak melihatnya** karena perannya bukan `1`. Sah, dan inilah yang dulu ditolak database |
| 3 | Nomor urut 3 — dr. Citra, `AssignmentRole = 2`, berakhir kosong | Sah. Dua konsulen berdampingan |
| 4 | Nomor urut 4 — dr. Dani, `AssignmentRole = 3`, berakhir kosong | Sah. Dokter jaga ikut berdampingan |
| 5 | Percobaan menyimpan dr. Eka, `AssignmentRole = 1`, berakhir kosong | **Ditolak database.** Sudah ada satu baris `Dpjp` terbuka untuk episode ini |

Empat penugasan aktif bersamaan, tepat satu di antaranya DPJP. Inilah `AC 2` dan `AC 3`.

**Pengalihan DPJP pada keadaan di atas.** Kepala ruangan mengalihkan DPJP dari dr. Andi ke
dr. Eka pada 12 September pukul 14:00. Di dalam satu transaksi: baris nomor urut 1 diisi waktu
berakhir `12-09-2026 14:00`, lalu baris nomor urut 5 dibuka untuk dr. Eka berperan `1`. Baris
nomor urut 2, 3, dan 4 **tidak disentuh** — ketiga dokter itu tetap terlibat. Sebelum task ini,
ketiganya akan ikut ditutup.

**Pertanyaan auditor.** "Siapa DPJP pada 10 September pukul 09:00?" Jawabannya dr. Andi, karena
baris nomor urut 1 berperan `Dpjp` dan periodenya memuat saat itu. Pertanyaan itu tetap terjawab
benar pada 15 September, walaupun DPJP sudah berganti — dan konsulen dr. Bima **tidak pernah**
muncul sebagai jawabannya, walaupun periodenya juga memuat saat itu.

### 5.3 Tidak dijalankan

Tiga butir. Ditulis apa adanya, bukan diklaim.

| Butir | Klasifikasi | Alasan |
| --- | --- | --- |
| `dotnet build` project aplikasi | `NOT RUN` | **Dikecualikan atas instruksi pemilik pada task aktif 11 September 2026**, yang menyatakan build dijalankan sendiri. Ini pengecualian eksplisit terhadap kewajiban build pada `TEST_POLICY.md` bagian 5, bukan kelalaian. Jumlah error dan warning karena itu **tidak diklaim sama sekali** |
| `RWI-AC-084b`, `RWI-AC-084c` — uji migration arah maju | `NOT RUN` | Menuntut Postgres sekali pakai dan penerapan migration. Penerapan ke environment mana pun adalah wewenang terpisah yang belum diberikan pada task ini |
| `RWI-AC-084h` — uji rollback sesudah ada baris konsulen | `NOT RUN` | Alasan yang sama. Perilakunya **ditegakkan di dalam berkas migration** lewat pemeriksaan yang menggagalkan `Down()` secara terkendali, tetapi penegakan itu belum dibuktikan berjalan |

Ketiganya adalah pembuktian eksekusi, bukan pemetaan source. Pemetaan source untuk kesembilan
acceptance criteria sudah lengkap dan terbukti — lihat bagian 6.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kolom dan enum ada, nilai bawaan `Dpjp` | **Terpenuhi** | Enum `InpDoctorAssignmentRole` bernilai `Dpjp = 1`, `Consultant = 2`, `OnCallDoctor = 3`. Properti `AssignmentRole` pada model bernilai bawaan `Dpjp`; configuration memasang `HasDefaultValue`; migration memasang `defaultValue: 1` |
| 2. Satu episode menyimpan satu DPJP, dua konsulen, dan satu dokter jaga aktif bersamaan | **Terpenuhi pada tingkat schema** | Filter index unik berubah menjadi `"EndDateTime" IS NULL AND "AssignmentRole" = 1`, sehingga baris berperan `2` dan `3` tidak lagi dilihat index itu. Contoh berangka bagian 5.2 langkah 1 s.d. 4. **Jalur tulis lewat endpoint untuk peran `2` dan `3` belum ada** — lihat bagian 7 |
| 3. DPJP kedua yang terbuka ditolak database | **Terpenuhi** | Index tetap `IsUnique()` atas `EpisodeId` dengan filter yang kini juga menuntut peran `1`. Contoh berangka bagian 5.2 langkah 5. Jalur penanganannya sudah ada di service: `DbUpdateException` dijawab pesan konflik yang dapat dibaca petugas |
| 4. Migration mengisi seluruh baris lama menjadi `Dpjp`, jumlah baris sebelum dan sesudah sama persis | **Terpenuhi pada source; eksekusi `NOT RUN`** | Langkah 2 migration adalah `UPDATE ... SET "AssignmentRole" = 1`, bukan `INSERT` maupun `DELETE`, sehingga jumlah baris **tidak dapat** berubah. Pengisiannya bukan tebakan: sebelum amandemen ini tabel hanya pernah menyimpan DPJP, sehingga nol baris ambigu dan nol laporan `unresolved` |
| 5. Urutan tiga langkah terbukti mengikat | **Terpenuhi** | Ketiganya berada di dalam **satu** berkas migration dengan urutan kolom → isi → index. Sebelum langkah 3, blok `DO $$` memeriksa setiap baris sudah berperan `1`, `2`, atau `3`, dan `RAISE EXCEPTION` beserta jumlah barisnya bila belum. Urutan terbalik karena itu **gagal terkendali**, bukan lolos diam-diam |
| 6. Rollback sesudah ada baris konsulen gagal terkendali | **Terpenuhi pada source; eksekusi `NOT RUN`** | `Down()` dibuka blok `DO $$` yang menghitung baris berperan `2` atau `3`, lalu `RAISE EXCEPTION` menyebut jumlahnya bila ada. Pemeriksaan itu berjalan **sebelum** index atau kolom disentuh, sehingga kegagalannya tidak meninggalkan schema setengah jalan |
| 7. Peran tidak dapat diubah pada baris yang sama | **Terpenuhi** | Pencarian menyeluruh menemukan **tepat dua** penulisan `AssignmentRole`, keduanya pada pembuatan baris baru. Nol endpoint, nol service, dan nol jalur lain yang mengubah peran baris yang sudah ada. Larangannya juga ditulis pada dokumentasi properti modelnya |
| 8. Pengalihan DPJP tidak pernah menyisakan saat tanpa DPJP maupun saat dengan dua DPJP | **Terpenuhi** | Penutupan baris lama dan pembukaan baris baru berada di dalam **satu** `BeginTransactionAsync`, dan keduanya disimpan oleh satu `SaveChangesAsync`. Dua pengalihan bersamaan diselesaikan index unik, bukan pemeriksaan di memori. Episode yang terlanjur punya lebih dari satu DPJP aktif **ditolak** alih-alih ditambahi baris baru |
| 9. Konsulen dan dokter jaga ditolak pada keputusan pulang, tanda tangan resume, perpindahan, dan perubahan isolasi | **Terpenuhi** | Keempat penjaga bermuara pada `GetActiveDoctorIdAsync` yang kini menyaring `AssignmentRole = Dpjp`. Dokter berperan `2` atau `3` karena itu tidak pernah cocok, dan keempat tindakan menjawab penolakan. Baris konsulen pada keputusan pulang sengaja **fail-closed** sampai ada kebijakan yang disetujui — `OPEN-MVP-004` |

### 6.2 Definition of Done

| Butir DoD | Status | Keterangan |
| --- | --- | --- |
| Kesembilan acceptance criteria terpetakan ke source yang benar-benar ada | **Terpenuhi** | Tabel 6.1 |
| Migration dibuat dan **tidak** diterapkan ke environment mana pun tanpa wewenang terpisah | **Terpenuhi** | Berkas migration ada; nol perintah database dijalankan; nol connection string disentuh |
| Batas rollback tertulis pada laporan | **Terpenuhi** | Bagian 7, dan ditegakkan di dalam `Down()` migration |
| `dotnet build` tanpa error baru | **BELUM TERPENUHI — `NOT RUN`** | Dikecualikan atas instruksi pemilik pada task aktif. Hasilnya tidak diklaim |
| Nol `[AccessPermission]` baru | **Terpenuhi** | Nol atribut hak akses ditambahkan atau diubah pada berkas mana pun |
| Kesesuaian QBE dan preflight engineering diselesaikan saat eksekusi | **Terpenuhi** | Preflight tertulis di atas; checker `PASS` pada mode `ReportOnly` dan `Strict` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `git diff` melaporkan `LF will be replaced by CRLF` pada `PatientIntegratedProgressNoteController.cs`. Berkas itu **bukan** milik task ini dan tidak disentuh; peringatannya berasal dari konfigurasi akhir baris Git, bukan dari perubahan di sini |
| Masalah yang diketahui | **Tiga butir, seluruhnya disebut apa adanya.** Lihat rincian di bawah |
| Risiko tersisa | **Batas rollback punya tenggat.** `Down()` aman **hanya selama belum ada satu pun baris berperan `2` atau `3`**. Begitu konsulen atau dokter jaga pertama tersimpan, index lama akan menolaknya, sehingga pemulihan **harus maju, bukan mundur**. Batas itu tidak hanya dicatat di sini, tetapi juga ditegakkan blok pemeriksaan di dalam `Down()` yang menggagalkan rollback beserta pesan yang menyebut jumlah barisnya. Pemilik risiko: pelaksana task bersama pemilik modul |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 12 berkas milik task ini — 10 `M` dan 2 `??`. Working tree juga memuat pekerjaan task lain yang **belum** di-commit dan **tidak** disentuh task ini: `InpBedOccupancyService.cs` milik `BE-RWI-073`, serta `PatientIntegratedProgressNoteController.cs` dan `PatientVitalSignController.cs` milik `BE-RWI-078` yang sedang berjalan. Nol stage, nol commit, nol push |
| Berkas yang dikerjakan dua task sekaligus | `InpatientClinicalContextService.cs` — lihat bagian 7.2 |
| Langkah berikutnya | Lihat di bawah |

### 7.1 Tiga hal yang ditinggalkan, dan kenapa

**Pertama — jalur tulis untuk konsulen dan dokter jaga belum ada.** Schema, index, dan penjaga
sudah siap menerima ketiga peran, tetapi **tidak ada satu pun endpoint** yang dapat membuat baris
berperan `Consultant` atau `OnCallDoctor`. Hari ini satu-satunya jalur tulis adalah
`POST /{id}/doctor-assignments`, yang selalu membuat baris `Dpjp`.

Ini **disengaja**. Kolom `Cakupan` pada kartu task `BE-RWI-074` menyebut lima butir — enum, kolom,
pergantian index unik, index pendukung, satu migration, dan pembacaan ulang keempat penjaga — dan
tidak satu pun di antaranya endpoint. `contracts/api-contract.md` `0.8.0` juga tidak memuat baris
endpoint untuk kedua peran itu. Menambahkannya sekarang berarti mendefinisikan ulang kontrak yang
sudah disetujui secara sepihak.

Akibatnya bagi pengguna perlu dinyatakan jujur: **kepala ruangan belum dapat melibatkan konsulen
lewat layar mana pun.** Yang selesai adalah fondasinya. Permukaan tulisnya perlu ditugaskan lewat
task tersendiri, dan `contracts/api-contract.md` perlu diamandemen lebih dulu.

**Kedua — `contracts/api-contract.md` belum memuat field `AssignmentRole`.** Jawaban
`InpatientDoctorAssignmentResponse` kini membawa field itu, dan kontraknya belum menyebutnya.
Penambahan ini **aditif dan kompatibel**, tetapi selisihnya tetap nyata dan menjadi milik pemilik
kontrak. Berkas kontrak sengaja **tidak** disunting dari task ini.

**Ketiga — kewajiban `HandoverReason` bagi peran `2` dan `3` belum ditegakkan.**
`data/data-dictionary.md` bagian 2 menyatakan alasan wajib diisi bila perannya `Consultant` atau
`OnCallDoctor`. Penegakan itu **belum ada di source**, dan memang belum dapat diuji, karena jalur
tulis untuk kedua peran itu juga belum ada. Keduanya perlu lahir bersamaan pada task yang sama.

### 7.2 Satu berkas dikerjakan dua task pada waktu yang sama

`InpatientClinicalContextService.cs` disunting `BE-RWI-074` **dan** `BE-RWI-078` yang sedang
berjalan, keduanya belum di-commit. Pembagiannya bersih dan tidak bertabrakan:

| Milik | Yang disentuh |
| --- | --- |
| `BE-RWI-074` | `FindAttendingDoctorIdAsync` diberi saringan `AssignmentRole = Dpjp`, ditambah dokumentasi pada `IsDoctorAssignedAsync` yang menjelaskan kenapa saringan itu **sengaja tidak** dipasang di sana |
| `BE-RWI-078` | `GUARD-INP-07` dan `GUARD-INP-08`, yaitu penulis klinis perawat beserta penilaian unitnya — `NurseNotAuthorized`, `NurseNotIdentified`, dan `IsNurseAuthorized` |

Kedua perubahan diperiksa ulang di akhir pekerjaan dan **keduanya utuh**. Task ini tidak
memulihkan, menimpa, maupun menghapus satu baris pun milik `BE-RWI-078`.

### 7.3 Langkah berikutnya

| Urut | Langkah | Pemilik |
| ---: | --- | --- |
| 1 | Jalankan `dotnet build` project aplikasi, lalu perbarui bagian 5 laporan ini dengan jumlah error dan warning yang sebenarnya | Pemilik repository |
| 2 | Terapkan migration pada Postgres sekali pakai, jalankan `RWI-AC-084b`, `RWI-AC-084c`, dan `RWI-AC-084h`, lalu catat hasilnya | Pemilik modul |
| 3 | Amandemen `contracts/api-contract.md` untuk field `AssignmentRole`, lalu putuskan permukaan tulis konsulen dan dokter jaga | Pemilik kontrak |
| 4 | Buka task backend untuk jalur tulis kedua peran itu beserta penegakan `HandoverReason`-nya | Pemilik modul |
| 5 | `BE-RWI-076` pada sub-modul `dokter-rawat-inap` kini **tidak lagi tertahan**: kolom peran yang ditunggunya sudah ada di source | Pemilik `dokter-rawat-inap` |
| 6 | `docs/Modul-RS/Rawat-Inap/Urutan-Pekerjaan-Rawat-Inap.md` masih menuliskan `BE-RWI-074` sebagai task urutan pertama yang **siap dikerjakan**, dan `BE-RWI-076` sebagai **menunggu** task ini. Berkas itu catatan urutan kerja milik pemilik, bukan roadmap, sehingga sengaja **tidak** disunting dari task implementasi | Pemilik modul |
