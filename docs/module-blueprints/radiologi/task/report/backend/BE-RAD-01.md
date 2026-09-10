# Laporan Perubahan Backend — `BE-RAD-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-01` |
| Judul | Siklus pengesahan pada aturan keselamatan |
| Slice | `S4` — Pengelolaan aturan keselamatan |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 2, gelombang `MVP-0` |
| Trace | `RAD-DEC-005`, `RJ-BIL-DEC-014`; arsitektur `RAD-ARCH-BE-001` bagian 4.8, 7, dan 8; ERD `RAD-ERD-SAF-001`; kamus data `RAD-ERD-DICT-001` bagian 3; state matrix `RAD-STATE-001` bagian 5 |
| Contract version | `RAD-ARCH-BE-001` — status `approved` 2026-09-10 |
| Dependency | Registry `Rad` berstatus `ACTIVE` — **terpenuhi** 2026-09-10 lewat `RAD-REQ-001` |
| Klasifikasi | `MEDIUM` — satu entity diperbarui, satu enum baru, satu configuration, satu migration; tidak menyentuh controller maupun service |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Repositories/Configurations/HealthServices/RadiologyManagement/`, `Migrations/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `0e2eb105` |
| Tanggal | 2026-09-10 |
| Status | **Selesai.** Source dan migration selesai, build lulus, dan migration **sudah diterapkan** ke database pengembangan `QuilvianNewDevYoga` atas izin eksplisit pemilik modul pada 2026-09-10 |

---

## 1. Masalah yang diperbaiki

Aturan keselamatan radiologi menentukan boleh atau tidaknya seorang pasien disinari. Sebelum
perubahan ini, aturan itu hanya punya satu penanda: aktif atau tidak aktif.

Akibatnya siapa pun yang dapat menyunting data induk dapat langsung membuat sebuah butir
keselamatan berlaku bagi seluruh pasien — tanpa pengesahan siapa pun.

> **Contoh nyata.** Admin Radiologi menonaktifkan aturan "skrining kehamilan wajib untuk
> CT-Scan" karena dianggap memperlambat antrian. Sejak saat itu seluruh pasien perempuan
> menjalani CT-Scan tanpa ditanyai kemungkinan hamil, dan tidak ada satu pun catatan bahwa
> keputusan itu pernah diambil, oleh siapa, atau atas dasar apa.

`RAD-DEC-005` menutup celah itu: admin menyusun, penanggung jawab klinis mengesahkan.

---

## 2. Proses bisnis

**Tujuan.** Memastikan aturan yang menentukan keselamatan pasien hanya berlaku setelah disahkan
pihak yang berwenang secara klinis.

**Pelaku.**

| Pelaku | Tugasnya |
| --- | --- |
| Admin Radiologi | Menyusun dan mengubah draf aturan, lalu mengajukan pengesahan |
| Penanggung jawab klinis | Mengesahkan, menolak, atau menonaktifkan aturan |

**Pemicu.** Rumah sakit menetapkan atau mengubah butir keselamatan yang wajib untuk sebuah alat.

**Prasyarat.** Alat pencitraan dan butir keselamatan sudah terdaftar.

**Langkah utama.**

1. Admin menyusun aturan. Aturan lahir berstatus `Draft`.
2. Admin mengajukan pengesahan. Status menjadi `PendingApproval`.
3. Penanggung jawab klinis mengesahkan. Status menjadi `Active`, dan `RuleVersion` naik satu.
4. Aturan mulai ikut dinilai gerbang keselamatan.

**Aturan yang berlaku.**

- Hanya aturan berstatus `Active` yang ikut dinilai gerbang keselamatan.
- Satu kombinasi alat, pemeriksaan, dan butir keselamatan hanya boleh punya **satu** aturan
  `Active` pada satu waktu.
- Study yang sudah lolos membekukan nomor versi aturan yang berlaku saat itu, sehingga
  perubahan aturan berikutnya tidak menulis ulang penilaian yang sudah terjadi.

**Perubahan status.**

| Dari | Tindakan | Ke | Siapa |
| --- | --- | --- | --- |
| — | Susun draf | `Draft` | Admin Radiologi |
| `Draft` | Ajukan pengesahan | `PendingApproval` | Admin Radiologi |
| `PendingApproval` | Sahkan | `Active`, versi naik | Penanggung jawab klinis |
| `PendingApproval` | Tolak | `Draft` | Penanggung jawab klinis, alasan wajib |
| `Active` | Nonaktifkan | `Inactive` | Penanggung jawab klinis |

**Jalur tidak normal.** Aturan yang ditolak kembali menjadi `Draft` beserta alasannya, bukan
dihapus. Aturan yang dinonaktifkan tidak dihidupkan kembali; penyusunan ulang membuat baris
baru.

**Hasil akhir.** Setiap aturan yang berlaku punya jejak siapa menyusun, siapa mengesahkan, dan
kapan.

> **Catatan penting.** Task ini baru menyiapkan **tempat penyimpanannya**. Endpoint dan service
> yang menjalankan alur di atas adalah `BE-RAD-02` dan `BE-RAD-03`, yang belum dikerjakan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `AGENTS.md` | Task mode, batas wewenang, aturan migration |
| `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` | Keberlakuan dan QBE rule |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Status registry `Rad` |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan dependency |
| `02-backend-architecture.md` bagian 4.8, 7, 8 | Kolom yang ditambahkan dan rencana migration |
| `erd/data-dictionary.md` bagian 3 | Tipe, panjang, dan sifat wajib tiap kolom |
| `contracts/state-transition-matrix.md` bagian 5 | Nilai status dan transisinya |
| `Models/RadOrder.cs` | Pola `using` dan gaya komentar modul ini |
| `Services/RadStudyService.cs#LoadApplicableRulesAsync` | Penyaring yang kelak diubah `BE-RAD-06` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RadiologyManagement/Enums/RadiologyEnums.cs` | Enum `RadSafetyRuleStatus` ditambahkan — `Draft` 1, `PendingApproval` 2, `Active` 3, `Inactive` 4 |
| `Areas/HealthServices/RadiologyManagement/Models/MstRadModalitySafetyRule.cs` | Enam kolom ditambahkan: `RuleStatus`, `SubmittedByUserId`, `SubmittedAt`, `RejectedByUserId`, `RejectedAt`, `RejectionReason`. Satu `using` namespace enum ditambahkan |
| `Repositories/Configurations/HealthServices/RadiologyManagement/MstRadModalitySafetyRuleConfiguration.cs` | `RuleStatus` dikonversi ke `int`; `RejectionReason` dibatasi 1000 karakter; filter index unik parsial dipindahkan dari `IsActive` ke `RuleStatus = 3` |
| `Migrations/20260910030523_AddRadSafetyRuleApprovalLifecycle.cs` | Migration baru, disunting tangan pada urutan dan nilai bawaannya — lihat bagian 3.4 |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui EF mengikuti perubahan model |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini tidak menyentuh controller, DTO, maupun endpoint |
| Database | Satu tabel diperbarui, enam kolom baru, satu index unik parsial diganti filternya. Migration **dibuat, belum dijalankan** |
| Keamanan/Auth | Tidak langsung. Task ini menyiapkan kolom yang kelak dipakai `BE-RAD-02` untuk menegakkan pemisahan wewenang menyusun dan mengesahkan |

### 3.4 Dua koreksi tangan pada migration yang dihasilkan EF

Migration yang dihasilkan `dotnet ef migrations add` memuat dua hal yang **berbahaya bila
dibiarkan**, dan keduanya persis yang diperingatkan `RAD-ARCH-BE-001` bagian 8.

| No | Yang dihasilkan EF | Mengapa berbahaya | Yang dilakukan |
| ---: | --- | --- | --- |
| 1 | `RuleStatus` bernilai bawaan `0` | `0` bukan anggota enum yang sah. Karena hanya `Active` (3) yang dinilai, **seluruh aturan yang selama ini berlaku berhenti dihitung** — dan gerbang yang fail-closed akan menolak setiap pemeriksaan radiologi | Nilai bawaan diubah menjadi `1` (`Draft`), yang sah dan tetap fail-closed |
| 2 | Index lama dihapus **sebelum** kolom terisi | Index unik parsial baru terbentuk di atas kolom yang seluruh barisnya masih bernilai bawaan | Urutan diubah: tambah kolom → isi data lama → ganti index |

Pengisian data lama ditulis manual, karena EF tidak menghasilkannya sendiri:

| Kondisi baris lama | `RuleStatus` yang diisi |
| --- | --- |
| `IsActive = true` dan `IsDelete = false` | `3` — `Active` |
| Selain itu | `4` — `Inactive` |

> **Contoh dampaknya.** Sebuah rumah sakit punya 14 aturan keselamatan aktif untuk enam alat.
> Tanpa pengisian data lama, seluruh 14 aturan itu akan bernilai `0` setelah migration
> dijalankan — dan **seluruh pemeriksaan radiologi tertolak**, karena tidak ada satu pun aturan
> ber-`RuleStatus = 3`. Dengan pengisian ini, keempat belas aturan menjadi `Active` dan
> pelayanan berjalan seperti sebelumnya.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini tidak menambah, mengubah, maupun menghapus endpoint. Endpoint
pengelolaan aturan keselamatan adalah lingkup `BE-RAD-03`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` — percobaan pertama | Gagal, `error CS0246: RadSafetyRuleStatus could not be found` | `NEW ERROR`, sudah diperbaiki | Model kurang `using` namespace enum; ditambahkan lalu build diulang |
| `dotnet build QuilvianSystemBackend.csproj` — setelah perbaikan | Berhasil, `EXIT=0`, 0 error | `PASS` | `/tmp/build2.log`, 7 menit 58 detik |
| `dotnet ef migrations add AddRadSafetyRuleApprovalLifecycle` | Berhasil, migration terbentuk dengan enam `AddColumn` dan penggantian index | `PASS` | `Migrations/20260910030523_AddRadSafetyRuleApprovalLifecycle.cs` |
| `dotnet build` — setelah migration disunting tangan | Berhasil, `EXIT=0`, 0 error | `PASS` | `/tmp/build3.log`, 10 menit 47 detik |
| Kesehatan `ApplicationDbContextModelSnapshot.cs` | 1.303 → 1.304 entity; himpunan nama entity **identik**, tidak ada yang hilang | `PASS` | Perbandingan `comm` antara HEAD dan working copy |
| Blok snapshot `MstRadModalitySafetyRule` | Enam kolom baru hadir, tidak ada index liar | `PASS` | Pembacaan langsung snapshot |
| `dotnet ef migrations list` sebelum penerapan | Tepat **satu** migration `(Pending)`, yaitu milik task ini. Tidak ada migration lain yang ikut terbawa | `PASS` | 166 migration terdaftar, 1 pending |
| Tinjauan SQL sebelum dijalankan | Enam `ALTER TABLE ADD`, satu `UPDATE` pengisian, lalu penggantian index — seluruhnya di dalam satu `START TRANSACTION ... COMMIT` | `PASS` | `dotnet ef migrations script` atas rentang satu migration |
| `dotnet ef database update` ke `QuilvianNewDevYoga` | Berhasil, `EXIT=0`, `Done.` | `PASS` | `/tmp/dbupdate.log` |
| `dotnet ef migrations list` sesudah penerapan | Migration tercatat diterapkan; **0 pending** | `PASS` | Keluaran perintah |
| `RadiologySafetyGateTests` | 13 lulus, 0 gagal, 114 ms | `PASS` | `dotnet test --filter` |

Uji manual: `NOT APPLICABLE` — belum ada endpoint maupun layar yang dapat dijalankan.

### Batas verifikasi yang perlu diketahui

Lingkungan ini **tidak memiliki klien PostgreSQL** — tidak ada `psql`, tidak ada `docker`.
Akibatnya jumlah baris `MstRadModalitySafetyRule` sebelum dan sesudah pengisian **tidak dapat
dihitung langsung**.

Yang dapat dipastikan tanpa klien database:

| Yang dipastikan | Dasarnya |
| --- | --- |
| Tidak ada baris tertinggal pada nilai bawaan | Pernyataan `UPDATE` memakai `CASE ... ELSE 4`, sehingga setiap baris pasti memperoleh `3` atau `4` |
| Pengisian benar-benar berjalan | Berada di dalam transaksi yang sama dengan `ALTER TABLE`; bila gagal, seluruh migration batal dan `database update` akan melaporkan error |
| Skema akhir sesuai rancangan | `migrations list` mencatat migration diterapkan dan tidak ada yang tersisa |

Yang **belum** dapat dipastikan: jumlah baris yang berubah menjadi `Active` versus `Inactive`.
Pemeriksaan itu memerlukan klien database, dan sebaiknya dilakukan pemilik modul sebelum
`BE-RAD-15` mengisi data master awal.

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| Menjalankan migration ke database mana pun | Wewenang terpisah dan **tidak diberikan**. Pemilik modul secara eksplisit memilih "buat migration, jangan dijalankan" pada 2026-09-10 |
| Uji integrasi Postgres radiologi | Membutuhkan database yang berjalan; termasuk wewenang yang sama di atas |
| `BE-RAD-06` — mengubah `LoadApplicableRulesAsync` agar menyaring `RuleStatus` | Task terpisah. Dijelaskan pada bagian 7 mengapa membiarkannya aman |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-12 — aturan `Draft` atau `PendingApproval` tidak ikut dinilai gerbang | **Belum terpenuhi** | Kolomnya sudah ada, tetapi penyaringnya baru diubah `BE-RAD-06` |
| AC-14 — pengesahan menaikkan versi tepat satu kali | **Belum terpenuhi** | Perilakunya milik `BE-RAD-02` |
| AC-16 — study yang sudah lolos mempertahankan nomor versi aturan | Terpenuhi, tidak berubah | `SafetyRuleVersionAtClearance` sudah ada sebelumnya dan tidak disentuh |
| Migration dapat dijalankan | **Terpenuhi** | Diterapkan ke `QuilvianNewDevYoga`, `EXIT=0`, 0 pending tersisa |
| Migration dapat dimundurkan | **Belum terbukti** | `Down()` ada dan terkompilasi, tetapi tidak dijalankan. Memundurkannya akan menghapus enam kolom pada database yang kini dipakai, dan itu tidak diminta |
| Uji pengisian data lama lulus | **Terpenuhi sebagian** | Pengisian berjalan di dalam transaksi yang sukses dan mencakup seluruh baris menurut bentuk `CASE ... ELSE`. Jumlah baris per status **tidak terhitung** karena tidak ada klien database |
| Tidak ada aturan yang berubah arti | Terpenuhi | `RuleStatus` diturunkan dari `IsActive`; `IsActive` sendiri tidak diubah, sehingga penyaring lama pada `LoadApplicableRulesAsync` tetap menghasilkan himpunan yang sama |

**Dua butir masih milik task lain** (`BE-RAD-02` untuk AC-14, `BE-RAD-06` untuk AC-12). Dua
butir lain terpenuhi sebagian sebagaimana dijelaskan di atas.

Task ini dinyatakan **selesai** untuk lingkupnya sendiri: menyiapkan tempat penyimpanan siklus
pengesahan beserta penerapannya ke database pengembangan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 188 warning, seluruhnya warning dokumentasi XML yang sudah ada sebelumnya pada modul MedicalRecord, OperatingRoom, Pharmacy, dan Registration. Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | `IsActive` dan `RuleStatus` kini menyimpan makna yang sama pada dua kolom berbeda. Ini disengaja demi kompatibilitas, dan `RAD-ERD-DICT-001` menyatakan `RuleStatus` yang berlaku bila keduanya berselisih. Penghapusan `IsActive` adalah pekerjaan tersendiri di kemudian hari |
| Risiko tersisa | Migration sudah diterapkan ke `QuilvianNewDevYoga` dan berhasil. Yang tersisa: **jumlah baris per status belum terhitung** karena tidak ada klien database, dan **`Down()` belum pernah dijalankan**. Selain itu, database pengembangan lain dan production **belum** menerima migration ini |
| Batas izin yang dihormati | Pemilik modul mengizinkan migrasi ke `devYoga`. Izin itu **tidak** diperlakukan sebagai izin menjalankan uji integrasi Postgres, karena uji tersebut menargetkan database yang sama dan **menulis baris nyata**. Uji itu sengaja tidak dijalankan |
| Perubahan sampingan | Satu percobaan `dotnet ef migrations add --no-build` menghasilkan migration kosong dan meregenerasi `ApplicationDbContextModelSnapshot.cs` dari assembly lama. Kedua berkas migration kosong dihapus dan snapshot dikembalikan dengan `git checkout` pada berkas itu saja. Berkas tersebut tidak tersentuh sebelum perintah itu, sehingga tidak ada pekerjaan orang lain yang hilang |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | `BE-RAD-02` — service siklus pengesahan; atau `BE-RAD-06` bila ingin penyaring gerbang segera memakai `RuleStatus` |

### Mengapa membiarkan `BE-RAD-06` belum dikerjakan itu aman

`LoadApplicableRulesAsync` masih menyaring `IsActive`, sementara index unik kini menyaring
`RuleStatus`. Keduanya tampak berselisih, tetapi tidak menimbulkan perbedaan perilaku:

pengisian data lama menurunkan `RuleStatus` **dari** `IsActive`, sehingga untuk seluruh baris
yang ada keduanya selalu sepakat. Perbedaan baru mungkin muncul ketika aturan dibuat lewat alur
pengesahan — dan alur itu belum ada sampai `BE-RAD-02` dan `BE-RAD-03` selesai.

### Perubahan yang bukan milik task ini

Dua berkas berikut berubah di worktree dan **bukan** hasil pekerjaan ini. Keduanya sengaja
dibiarkan utuh:

| Berkas | Isinya |
| --- | --- |
| `docs/module-blueprints/rekam-medis/00-interview-decisions.md` | Amandemen sambungan Laboratorium, `RM-DEC-030` sampai `RM-DEC-036` |
| `docs/module-blueprints/rekam-medis/blueprint-manifest.md` | Kenaikan revision 2 ke 3 pada modul Rekam Medis |

Selain itu, perubahan pada `EmergencyOrderKind.cs`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, dan
seluruh berkas blueprint radiologi berasal dari pekerjaan tata kelola dan perencanaan hari yang
sama, bukan dari `BE-RAD-01`.

### Status Git pada akhir pekerjaan

Berkas yang berubah karena `BE-RAD-01`:

```text
 M Areas/HealthServices/RadiologyManagement/Enums/RadiologyEnums.cs
 M Areas/HealthServices/RadiologyManagement/Models/MstRadModalitySafetyRule.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/Configurations/HealthServices/RadiologyManagement/MstRadModalitySafetyRuleConfiguration.cs
?? Migrations/20260910030523_AddRadSafetyRuleApprovalLifecycle.cs
?? Migrations/20260910030523_AddRadSafetyRuleApprovalLifecycle.Designer.cs
```

Tidak ada `git add`, commit, maupun push yang dilakukan.
