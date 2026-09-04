# Laporan Perubahan Backend — `BE-RWI-034`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-034` |
| Judul | Sembilan endpoint yang hak aksesnya tidak dapat diberikan kepada siapa pun |
| Slice | Perbaikan metadata hak akses modul, ditambah endpoint baca kelayakan keuangan |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-034` |
| Trace | `contracts/permission-audit-matrix.md` bagian 2 dan 3; `Seeders/AccessMenuSeeder.cs`; `Services/Security/AccessPermissionService.cs`; `Filters/AccessPermissionFilter.cs`; ditemukan saat preflight `FE-RWI-013` |
| Contract version | API `0.4.0`; permission matrix diperbarui pada task ini |
| Dependency | `BE-RWI-024` selesai — endpoint bacanya memakai `GetFinancialClearanceAsync` yang sudah ada. Perbaikan hak aksesnya tidak bergantung pada apa pun |
| Klasifikasi | `MEDIUM`, skor 9: repository 0, berkas diperiksa 3, berkas diubah 2, logika bisnis 0, kontrak API 1, database 0, keamanan/auth 3, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `InPatientManagement`, test, dan dokumen tracked modul Rawat Inap |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `514b1d8232720eb450bc40f6deea6c6661160c8d` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | **Selesai.** Kelima acceptance criteria terbukti; tidak ada migration maupun eksekusi database |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Bounded context | `HealthServices / InPatientManagement` |
| Prefix ownership | `Inp` terdaftar dan `ACTIVE`; task tidak menambah entity, modul, maupun prefix |
| Applicability | `TOUCHED LEGACY`; perubahan dibatasi pada atribut hak akses, satu aksi `GET` baru, test, dan dokumen tracked |
| QBE berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-AUD-001` |
| Archetype transaksi | Sub-proses ter-scope induk (`episodeId`); aksi baru bersifat read-only dan tidak menambah perpindahan status |
| Database authority | `NONE`; tidak ada perubahan schema, migration, maupun eksekusi database |
| Frontend | Tidak disentuh |

---

## 1. Masalah yang diperbaiki

Sembilan endpoint paling penting di modul Rawat Inap **tidak dapat dipakai satu pun petugas
sungguhan**. Bukan karena kodenya salah menghitung sesuatu, melainkan karena kemampuannya tidak
pernah muncul di layar Pengaturan → Manajemen Role → Akses Role, sehingga admin tidak punya
kotak untuk dicentang.

Penyebabnya satu kesalahan penamaan yang berulang. Hak akses di repository ini bekerja dari dua
atribut yang harus cocok huruf demi huruf:

- `AccessMenuSeeder` membuat baris yang dapat dicentang admin memakai `ControllerName` dari
  `[AccessController]` dan argumen pertama `[AccessAction]`;
- `AccessPermissionFilter` mencari baris itu memakai kedua argumen `[AccessPermission]`.

Ketika keduanya berbeda, pencarian tidak menemukan apa pun, `HasAccessAsync` memulangkan
`false`, dan hasilnya **403 permanen yang tidak dapat diperbaiki dari layar mana pun** — karena
baris untuk dicentangnya memang tidak pernah dibuat.

**Contoh nyata.** Seorang DPJP hendak menandatangani resume pulang Ibu Rina. Endpoint
`PATCH /discharges/{episodeId}/summary/sign` memeriksa pasangan `InpatientDischarge : Sign`.
Yang didaftarkan seeder di bawah `InpatientDischarge` hanyalah `Read` dan `Update` — karena
`[AccessAction]` pada aksi itu tertulis `"Update"`, bukan `"Sign"`. Admin membuka layar Akses
Role, mencari kemampuan "tanda tangan resume", dan tidak menemukannya. DPJP terus ditolak, dan
karena resume yang belum ditandatangani menahan penutupan episode, Ibu Rina ikut tertahan.

**Kenapa tidak ketahuan lebih awal.** Bukti "terbukti berjalan" pada laporan `BE-RWI-020` s.d.
`BE-RWI-027` diambil lewat Swagger memakai akun SuperAdmin. `AccessPermissionService`
memulangkan `true` untuk SuperAdmin **sebelum satu baris hak akses pun dibaca**, sehingga
seluruh pengujian itu lolos tanpa pernah menyentuh cacatnya.

Satu masalah kedua ikut ditemukan saat mengerjakan ini, dijelaskan pada bagian 3.4.

---

## 2. Proses bisnis

**Tujuan.** Setiap kemampuan modul Rawat Inap dapat diberikan admin kepada peran yang tepat,
dan penandaan kelayakan keuangan dapat dibaca ulang oleh petugas yang menandainya.

**Pelaku.** Admin sistem (memberi hak akses), petugas admisi, DPJP, perawat, supervisor, dan
petugas kasir.

**Langkah yang berurutan.**

1. Aplikasi menyala. `AccessMenuSeeder` membaca seluruh atribut `[AccessController]` dan
   `[AccessAction]` lewat refleksi, lalu membuat atau memperbarui baris `SysControllerAccess`
   dan `SysActionAccess`.
2. Admin membuka layar Akses Role. Layar itu menampilkan baris hasil langkah 1, disaring
   `AccessTypes.AllowedForRoleAccess` — hanya `Read`, `Create`, `Update`, dan `Delete`.
3. Admin mencentang kemampuan untuk pasangan Departemen × Posisi. Tersimpan sebagai baris
   `SysAccessPolicy`.
4. Petugas memanggil endpoint. `AccessPermissionFilter` menanyakan pasangan
   `[AccessPermission]` kepada `AccessPermissionService`.
5. Service mencocokkan pasangan itu ke `SysActionAccess`, lalu memeriksa apakah penempatan
   organisasi petugas — Departemen dan Posisi beserta masa berlakunya — punya `SysAccessPolicy`
   yang mengizinkannya.

**Aturan yang berlaku.** Argumen kedua `[AccessPermission]` menentukan **nama baris**, bukan
jenis kolomnya. Kolom Read/Create/Update/Delete ditentukan properti `AccessType`. Karena itu
sebuah kemampuan boleh bernama `Sign` dan tetap muncul di kolom Update — dan itulah bentuk yang
dipakai task ini.

**Jalur tidak normal.** Bila petugas belum diberi kemampuannya, jawabannya 403 dengan pesan
"Anda tidak memiliki akses ke menu atau fitur ini", dan kejadiannya dicatat logger sebagai
`Security / AccessDenied`. Yang berubah setelah task ini: penolakan itu kini **dapat
diselesaikan admin** lewat layar Akses Role, tidak lagi permanen.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa |
| --- | --- |
| `Seeders/AccessMenuSeeder.cs` | Memastikan aturan pembentukan baris: kunci `(ControllerAccessId, ActionName)`, sifat upsert, dan penyatuan nama kembar |
| `Filters/AccessPermissionFilter.cs` | Memastikan pasangan yang dicari berasal dari `Arguments` `[AccessPermission]` |
| `Services/Security/AccessPermissionService.cs` | Memastikan jalur pencocokan dan membuktikan SuperAdmin memang memulangkan `true` lebih awal |
| `Constants/AccessTypes.cs` | Memastikan keempat nilai yang ditampilkan layar Akses Role |
| `Attributes/AccessPermissionAttribute.cs` | Memastikan pasangannya disimpan pada `Arguments`, bukan sebagai properti |
| `QuilvianSystemBackend.Tests/BillingManagement/AccessPermissionEnforcementTests.cs` | Pola harness Identity sungguhan yang dipakai ulang |
| `contracts/permission-audit-matrix.md`, `contracts/api-contract.md` | Dokumen yang wajib ikut mutakhir |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Lima `[AccessAction]` diberi nama sendiri (`Sign`, `MarkFinancialClearance`, `Close`, `CloseOverride`, `RecordDeparture`); tiga `[AccessPermission]` dibetulkan nama resource-nya menjadi `InpatientDischarge`; **satu aksi baru** `GET /{episodeId}/financial-clearance`; label `Update` disamakan |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | `[AccessAction]` untuk `SetIsolation` dan `Reopen` diberi nama sendiri; label `Update` disamakan |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs` | `[AccessAction]` untuk `Transfer` diberi nama sendiri; label `Create` disamakan |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientRoleAccessContractTests.cs` | **Berkas baru.** 23 test penjaga, termasuk pembuktian tanpa SuperAdmin |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientModuleControllerContractTests.cs` | Assertion lama yang mengharapkan pasangan rusak diperbarui; jumlah endpoint discharge 11 → 12 |
| `docs/module-blueprints/rawat-inap/contracts/permission-audit-matrix.md` | Bagian 2.3 dan 3 dibetulkan; catatan kenapa kolom `Resource` bukan pilihan bebas |
| `docs/module-blueprints/rawat-inap/contracts/api-contract.md` | Tiga baris hak akses dibetulkan; satu baris endpoint baru |

### 3.3 Kesembilan pasangan, sebelum dan sesudah

| Endpoint | Sebelum — diperiksa filter | Sebelum — didaftarkan seeder | Sesudah, keduanya |
| --- | --- | --- | --- |
| `PATCH /discharges/{episodeId}/summary/sign` | `InpatientDischarge : Sign` | `InpatientDischarge : Update` | `InpatientDischarge : Sign` |
| `POST /discharges/{episodeId}/financial-clearance` | `InpatientFinancialClearance : Update` | `InpatientDischarge : Update` | `InpatientDischarge : MarkFinancialClearance` |
| `POST /discharges/{episodeId}/close` | `InpatientEpisode : Close` | `InpatientDischarge : Update` | `InpatientDischarge : Close` |
| `POST /discharges/{episodeId}/close-with-override` | `InpatientEpisode : CloseOverride` | `InpatientDischarge : Update` | `InpatientDischarge : CloseOverride` |
| `POST /discharges/{episodeId}/record-departure` | `InpatientDischarge : RecordDeparture` | `InpatientDischarge : Update` | `InpatientDischarge : RecordDeparture` |
| `PATCH /episodes/{id}/isolation-requirement` | `InpatientEpisode : SetIsolation` | `InpatientEpisode : Update` | `InpatientEpisode : SetIsolation` |
| `POST /episodes/{id}/correction-sessions` | `InpatientEpisode : Reopen` | `InpatientEpisode : Update` | `InpatientEpisode : Reopen` |
| `PATCH /episodes/{id}/correction-sessions/{sessionId}/close` | `InpatientEpisode : Reopen` | `InpatientEpisode : Update` | `InpatientEpisode : Reopen` |
| `POST /bed-occupancies/placements/transfer` | `InpatientBedOccupancy : Transfer` | `InpatientBedOccupancy : Update` | `InpatientBedOccupancy : Transfer` |

**Arah perbaikan yang dipilih.** Roadmap menyebut dua kemungkinan dan menyerahkan pilihannya
kepada pemilik keamanan. Yang dipilih adalah **butir halus per aksi**: setiap kemampuan
mendapat `[AccessAction]` bernama sendiri, sehingga `Sign`, `Close`, `CloseOverride`, dan
`Transfer` tetap terpisah dari `Update`. Arah sebaliknya — melebur semuanya menjadi `Update` —
ditolak karena akan membuat siapa pun yang boleh menyunting resume ikut boleh
menandatanganinya.

**Konsekuensi yang perlu diketahui admin.** Delapan butir baru kini muncul di layar Akses Role
dan **wajib diberikan** kepada peran yang berhak. Selama belum diberikan, kesembilan endpoint
itu tetap ditolak — bedanya, penolakan itu sekarang dapat diselesaikan sendiri oleh admin.
Peta peran yang disarankan ada pada `permission-audit-matrix.md` bagian 3.

**Perubahan nama resource pada tiga baris.** `close`, `close-with-override`, dan
`financial-clearance` sebelumnya memakai nama resource `InpatientEpisode` dan
`InpatientFinancialClearance`. Keduanya tidak dapat dipertahankan: ketiga aksi itu berada pada
`InpatientDischargeController`, dan seeder selalu mendaftarkan baris di bawah `ControllerName`
milik controller tempat aksinya berada. Memindahkan aksinya ke controller lain akan mengubah
route dan memutus kontrak `0.4.0`, sehingga yang disesuaikan adalah nama resource-nya.

### 3.4 Temuan kedua: satu baris, banyak label

Saat menulis test penjaga, ditemukan bahwa beberapa endpoint memakai `ActionName` yang sama
dengan `DisplayName` berbeda. Karena seeder menyatukannya menjadi **satu baris**, label yang
akhirnya tampil di layar Akses Role bergantung pada urutan pendaftaran — dan urutan itu tidak
dijamin.

| Controller | `ActionName` | Label yang bertabrakan | Label sesudah |
| --- | --- | --- | --- |
| `InpatientEpisode` | `Update` | Update Episode, Cancel Episode, Handover Doctor, Assign Nurse | Update Inpatient Episode |
| `InpatientDischarge` | `Update` | Decide Discharge, Upsert Summary, Mark Clearance Item | Update Inpatient Discharge |
| `InpatientBedOccupancy` | `Create` | Create Bed Reservation, Create Bed Placement | Create Inpatient Bed Occupancy |

Label dan keterangannya disamakan supaya menggambarkan **seluruh** kemampuan yang digerbang
baris itu. **Siapa yang berwenang tidak berubah sama sekali** — keempat aksi episode tetap satu
butir `Update`, persis seperti sebelumnya. Yang berubah hanya kalimat yang dibaca admin, dari
menyesatkan menjadi benar.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru `GET /discharges/{episodeId}/financial-clearance`. Sembilan baris kolom hak akses berubah nilainya. Tidak ada endpoint yang dihapus, dan tidak ada bentuk payload yang berubah |
| Database | `NOT APPLICABLE` untuk schema. Baris `SysActionAccess` baru dibuat sendiri oleh `AccessMenuSeeder` saat aplikasi menyala; tidak ada migration dan tidak ada SQL yang dijalankan task ini |
| Keamanan/Auth | Inti task. Sembilan kemampuan berubah dari **tidak dapat diberikan kepada siapa pun** menjadi dapat diberikan. Tidak ada kewenangan yang dilonggarkan: `GUARD-INP-01` s.d. `GUARD-INP-04` tetap berjalan di dalam service, di samping pemeriksaan hak akses |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{episodeId}/financial-clearance` | Membaca penandaan kelayakan keuangan beserta seluruh riwayatnya, supaya kasir dapat memeriksa ulang penandaannya sendiri | `InpatientDischarge : ReadFinancialClearance` |

Jawaban `200` berisi `FinancialClearanceResponse`: nomor episode, status yang berlaku, penanda
`IsCleared`, dan daftar `History` berurut nomor. Episode yang tidak ada dijawab `404` dengan
pesan "Episode rawat inap tidak ditemukan."

Hak aksesnya sengaja **bukan** `InpatientDischarge : Read`. `Read` menggerbang
`GET /{episodeId}/summary`, yaitu isi resume pulang berisi diagnosis dan ringkasan perawatan.
Kasir tidak berkepentingan membacanya, dan butir terpisah membuat admin dapat memberi kasir
kemampuan kelayakan keuangan tanpa ikut membuka isi resume.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | Berhasil | `PASS` | `Build succeeded. 200 Warning(s), 0 Error(s)` |
| Seluruh suite `InPatientManagement` | 280 lulus dari 280 | `PASS` | `Failed: 0, Passed: 280, Skipped: 0` |
| Seluruh project test `QuilvianSystemBackend.Tests` | 867 lulus dari 867 | `PASS` | `Failed: 0, Passed: 867, Skipped: 0` |
| Kriteria 1 dan 5 — setiap pasangan `[AccessPermission]` modul ada sebagai baris yang dapat dicentang | Lulus | `PASS` | `SetiapPasanganHakAksesModul_AdaSebagaiBarisYangDapatDicentang` |
| Kriteria 2 — kesembilan pasangan dapat diberikan kepada peran **non-SuperAdmin** | Lulus, 9 kasus | `PASS` | `PasanganYangDahuluRusak_DapatDiberikanKepadaPeranNonSuperAdmin` |
| Kendali negatif — pasangan yang sama tetap ditolak bila kebijakannya belum diberikan | Lulus, 9 kasus | `PASS` | `PasanganYangDahuluRusak_TetapDitolakBilaBelumDiberikan` |
| Kriteria 3 — kasir dapat diberi kelayakan keuangan tanpa ikut membaca resume pulang | Lulus | `PASS` | `KelayakanKeuangan_DapatDiberikanTanpaIkutMemberiBacaResumePulang` |
| Kriteria 3 — endpoint bacanya ada dengan butir hak akses sendiri | Lulus | `PASS` | `EndpointBacaKelayakanKeuangan_AdaDenganButirHakAksesSendiri` |
| Kriteria 4 — seluruh aksi modul muncul dan dapat diberikan di layar Akses Role | Lulus | `PASS` | `SetiapAksiModul_MunculDanDapatDiberikanDiLayarAksesRole` |
| Label baris kembar konsisten | Lulus setelah perbaikan 3.4 | `PASS` | `ActionNameKembar_MemakaiLabelYangSama` |

**Cara kriteria 2 dibuktikan tanpa SuperAdmin.** Test tidak memeriksa atribut, melainkan
memanggil `AccessPermissionService.HasAccessAsync` yang sesungguhnya. Registry
`SysControllerAccess` dan `SysActionAccess` di-seed memakai aturan yang sama persis dengan
`AccessMenuSeeder`, dibaca dari atribut yang benar-benar terpasang. Penggunanya
`UserType.Employee` tanpa peran SuperAdmin, memakai `UserManager` ASP.NET Core Identity
sungguhan di atas `ApplicationDbContext` InMemory — bukan mock. Kendali negatifnya memastikan
hasil `true` benar-benar berasal dari `SysAccessPolicy` yang diberikan, bukan dari jalur pintas
mana pun.

Uji manual: `NOT FEASIBLE`. Pembuktian terhadap database tim menuntut aplikasi menyala dan
`AccessMenuSeeder` dijalankan di sana; keduanya wewenang terpisah dan tidak diberikan pada task
ini. Test otomatis di atas menutup lubang yang sama tanpa menyentuh database mana pun.

**Tidak dijalankan:**

- Project `QuilvianSystemBackend.BillingTests` — menuntut `QUILVIAN_BILLING_TEST_DB` dan berada
  di luar scope task.
- Eksekusi `AccessMenuSeeder` terhadap database tim — di luar wewenang, lihat bagian 6.

---

## 6. Yang wajib dikerjakan setelah task ini

| Butir | Alasan | Pemilik |
| --- | --- | --- |
| Jalankan aplikasi supaya `AccessMenuSeeder` membuat delapan baris `SysActionAccess` baru | Baris itu belum ada di database mana pun; selama belum dibuat, kesembilan endpoint tetap ditolak | Pemilik lingkungan |
| Admin memberikan kedelapan butir baru kepada peran yang berhak | Kode hanya mendeklarasikan kemampuan; yang memutuskan siapa boleh memakainya adalah admin lewat layar Akses Role | Admin sistem |
| Verifikasi ulang `FE-RWI-009` s.d. `FE-RWI-015` memakai akun **bukan** SuperAdmin | Bukti kelayakan sebelumnya diambil sebagai SuperAdmin dan karena itu tidak membuktikan apa pun soal hak akses | Frontend bersama QA |

---

## 7. Risiko yang tersisa

| Risiko | Keadaan |
| --- | --- |
| Hardcode peran pada `Helpers/InpatientActorClaims.cs` — `SupervisorOrWardHeadRoles`, `SupervisorRoles`, `CashierOrBillingRoles` | **Tetap terbuka, tidak disentuh task ini.** Daftar nama peran tetap sebagai teks di dalam kode. Bila nama peran kasir di rumah sakit berbeda, penandaan kelayakan keuangan tetap ditolak untuk petugas yang sebenarnya berwenang. Menggantinya mengubah siapa yang dapat memakai fitur, sehingga menuntut keputusan pemilik proses — dicatat sebagai temuan sesuai `role-access-rules.md` bagian 5 |
| SuperAdmin tetap melewati seluruh pemeriksaan | Perilaku bawaan `AccessPermissionService`, dikendalikan `Security:Authorization:EnforceClinicalPolicyForSuperAdmin`. Tidak diubah task ini. Akibatnya pengujian apa pun memakai SuperAdmin tetap tidak membuktikan hak akses |
| Baris `SysActionAccess` lama bernama `Update` tidak dihapus | Seeder bersifat upsert dan tidak menghapus baris yang tidak lagi dipakai. Kebijakan lama yang menunjuk baris `Update` tetap sah dan tetap menggerbang aksi yang memang masih memakai `Update` |

---

## 8. Task berikutnya

`BE-RWI-006` bersama `BE-RWI-032`, lalu `BE-RWI-033` sebagai penutup traceability modul.
