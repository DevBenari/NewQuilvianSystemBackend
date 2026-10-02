# Laporan Perubahan Backend — `BE-RAD-08`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-08` |
| Judul | Service penulisan dan pengesahan bacaan |
| Slice | `S9` — Hasil bacaan radiolog |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 4, gelombang `MVP-2` |
| Trace | `RAD-DEC-003`, `RAD-DEC-015`; `RAD-STATE-001` bagian 3 dan 4; `RAD-VAL-001` bagian 1; `RAD-PERM-001` bagian 5.1, 6, 7, dan 8; `RAD-ARCH-BE-001` bagian 4.11 |
| Contract version | `RAD-STATE-001` rev 1, `RAD-VAL-001` rev 1, `RAD-PERM-001` rev 5 — seluruhnya `approved` |
| Dependency | `BE-RAD-07` — **selesai** untuk source dan migration |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa **2**, berkas diubah 1, logika bisnis **2**, kontrak API 0, database 1, keamanan/auth **2**, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Program.cs`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **Selesai untuk source dan uji.** Build lulus 0 error; 162 uji radiologi lulus, 41 di antaranya baru; seluruh 1.367 uji in-memory lulus. **Satu penghalang ditemukan dan belum diselesaikan**: hak akses penanda `RadReport : ActAsRadiologist` belum dapat diberikan kepada peran mana pun — lihat bagian 7 |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE`. Entri riwayat registry 2026-09-10 mencabut penghalang `QBE-MOD-002` atas entity `Rad*` berikutnya |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001` service memiliki orkestrasi domain; `QBE-VAL-001` memvalidasi request dan invarian bisnis; `QBE-TXN-001` mentransaksikan konsistensi lintas record; `QBE-DTO-001` entity EF tidak menjadi kontrak API; `QBE-LOG-001` log perubahan state menyertakan aktor; `QBE-AUD-001` audit database terpisah dari application logging; `QBE-CODE-001` sampai `QBE-CODE-005` penomoran bacaan; `QBE-NAM-002` prefix `Rad`; `QBE-MOD-001` penempatan modul; `QBE-ENUM-001` |
| QBE ID yang **tidak** berlaku | `QBE-API-001`, `QBE-PERM-001`, `QBE-PAGE-001` — endpoint dan atribut hak akses milik `BE-RAD-09`; `QBE-ENT-001`, `QBE-CFG-001` — tidak ada entity baru; `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION`; `QBE-CODE-006` — provider bersama tidak dipakai, alasannya di bagian 3.4 |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Setelah `BE-RAD-07`, tabel hasil bacaan sudah ada tetapi **tidak ada satu pun jalan untuk
mengisinya**. Yang lebih menentukan: aturan paling penting pada modul ini belum ditegakkan di
mana pun.

Aturannya berbunyi sederhana — **yang menulis bacaan belum tentu boleh menyatakannya sah.**
Residen, radiografer, dan bantuan AI boleh menulis draf, tetapi hanya dokter radiolog yang boleh
mengesahkan, dan draf yang ditulis bukan-radiolog **wajib** disahkan orang lain.

> **Mengapa ini bukan sekadar urusan tombol.** Bacaan yang sudah dirilis dipakai dokter lain
> untuk memberi obat, menjadwalkan operasi, dan memulangkan pasien. Seorang residen yang dapat
> mengesahkan bacaannya sendiri berarti tidak ada satu pun dokter spesialis yang pernah melihat
> kesimpulan itu sebelum dipakai — padahal seluruh alur ini dibangun tepat untuk memastikan
> sebaliknya.

**Hak akses saja tidak dapat menegakkannya.** Penanda `[AccessPermission("RadReport",
"Validate")]` menjawab pertanyaan "boleh mencoba mengesahkan atau tidak". Ia tidak pernah
membandingkan siapa yang menulis baris yang sedang disahkan. Seorang residen yang diberi
`Validate` — supaya ia dapat mengesahkan draf yang ditulis radiografer — akan lolos atribut itu
walaupun yang ia sahkan adalah drafnya sendiri.

Karena itu pemeriksaannya wajib berada di dalam service, dan itulah yang dikerjakan task ini.

---

## 2. Proses bisnis

**Tujuan.** Menyediakan jalan bagi bacaan untuk ditulis, diperbaiki, disahkan, dan dirilis —
dengan pemisahan wewenang yang tidak dapat dilewati.

### 2.1 Alur normal, berurutan

| Langkah | Pelaku | Yang terjadi | Status bacaan |
| ---: | --- | --- | --- |
| 1 | Sistem | Citra dinyatakan layak. Wadah bacaan lahir, belum ada isinya | `Pending` |
| 2 | Radiolog, residen, radiografer, atau bantuan AI | Menulis draf. **Peran penulis dibekukan** pada baris versi | `Drafted` |
| 3 | Penulis draf itu sendiri | Memperbaiki drafnya bila perlu. Tidak melahirkan versi baru | `Drafted` |
| 4 | Dokter radiolog | Mengesahkan | `Validated` |
| 5 | Dokter radiolog | Merilis ke dokter pengirim. **Setelah ini isinya beku** | `Released` |

### 2.2 Tujuh keadaan pengesahan, ditulis apa adanya

Tabel ini disalin dari `RAD-STATE-001` bagian 3 dan **seluruh barisnya dibuktikan uji**.

| Penulis draf | Peran yang dibekukan | Pengesah | Hasil | Uji yang membuktikannya |
| --- | --- | --- | --- | --- |
| dr. Sinta, Sp.Rad | `Radiologist` | dr. Sinta sendiri | **Diterima** | `AC2_RadiologMengesahkanDrafnyaSendiriDiterima` |
| dr. Sinta, Sp.Rad | `Radiologist` | dr. Bagas, Sp.Rad | **Diterima** | `AC2_RadiologLainMengesahkanDrafRadiologDiterima` |
| dr. Rian, residen | `Resident` | dr. Rian sendiri | **Ditolak `403`** | `AC1_ResidenMengesahkanDrafnyaSendiriDitolak` |
| dr. Rian, residen | `Resident` | dr. Sinta, Sp.Rad | **Diterima** | `AC1_DrafResidenDisahkanRadiologDiterima` |
| Radiografer Tono | `Radiographer` | Radiografer Tono | **Ditolak `403`** | `AC2_RadiograferMengesahkanDrafnyaSendiriDitolak` |
| Bantuan AI | `AiAssisted` | dr. Sinta, Sp.Rad | **Diterima** | `AC2_DrafBantuanAiDisahkanRadiologDiterima` |
| Bantuan AI | `AiAssisted` | Bantuan AI | **Ditolak `403`** | `AC2_DrafBantuanAiTidakDapatDisahkanDirinyaSendiri` |

### 2.3 Aturan yang paling mudah salah dipahami: peran dibekukan, bukan dibaca ulang

> **Contoh berangka.** dr. Rian menulis draf pada **Januari** sebagai residen. Baris versinya
> menyimpan `AuthorRoleSnapshot = Resident`. Menurut aturan, draf itu wajib disahkan radiolog
> lain.
>
> Pada **Juli** dr. Rian lulus menjadi Sp.Rad dan diberi hak akses penanda
> `RadReport : ActAsRadiologist`. Ia membuka kembali draf Januari-nya dan menekan Sahkan.
>
> **Tetap ditolak `403`.** Yang dibaca service adalah `Resident` — keadaan saat bacaan itu
> disusun — bukan kewenangannya hari ini. Kalau perannya dibaca ulang, draf Januari mendadak
> terbaca ditulis radiolog dan aturan "wajib disahkan orang lain" ikut menguap untuk seluruh
> draf lama yang pernah ia tulis.

Dibuktikan `AC3_ResidenYangKemudianMenjadiRadiologTetapDitolakAtasDrafLamanya`.

### 2.4 Jalur tidak normal

| Percobaan | Jawaban sistem | Kode |
| --- | --- | --- |
| Menulis bacaan atas study yang mutunya belum dinilai | "Mutu citra belum dinilai. Nilai mutu citra lebih dulu sebelum menulis bacaan." | `422` |
| Menulis bacaan atas study yang dinyatakan tidak layak | "Citra pemeriksaan ini dinyatakan tidak layak dibaca … Buat pemeriksaan ulang lebih dulu." | `422` |
| Membuat bacaan kedua atas study yang sama | "Pemeriksaan ini sudah memiliki bacaan. Gunakan koreksi bila ingin mengubahnya." | `409` |
| Menyimpan draf tanpa mengisi kesimpulan | "Kesimpulan bacaan wajib diisi." | `400` |
| Bukan-radiolog menulis draf atas nama dokter radiolog | "Anda belum terdaftar sebagai dokter radiolog …" | `403` |
| Orang lain mengubah draf yang bukan miliknya | "Hanya penulis draf yang dapat mengubahnya sebelum disahkan." | `403` |
| Mengubah draf yang sudah disahkan | "Bacaan ini sudah disahkan, sehingga drafnya tidak dapat diubah lagi." | `409` |
| Mengubah isi versi yang sudah dirilis | "Bacaan yang sudah dirilis tidak dapat diubah. Buat koreksi bila ada yang perlu diperbaiki." | `403` |
| Merilis bacaan yang belum disahkan | "Bacaan harus disahkan lebih dulu sebelum dirilis." | `409` |
| Pengesah kedua atas bacaan yang sudah disahkan | "Hanya draf yang belum disahkan yang dapat disahkan …" | `409` |

**Dua penolakan yang sengaja tidak dilebur.** "Mutu belum dinilai" dan "dinyatakan tidak layak"
sama-sama menahan penulisan bacaan, tetapi menuntut tindakan yang berlawanan: yang pertama
menunggu radiografer menilai, yang kedua menunggu pemeriksaan diulang. Satu pesan gabungan
membuat petugas menunggu sesuatu yang tidak akan pernah datang.

### 2.5 Mengapa penolakan "pengesahan sendiri" diperiksa sebelum "bukan radiolog"

Seorang residen tanpa penanda radiolog yang mengesahkan drafnya sendiri melanggar dua aturan
sekaligus. Urutan pemeriksaannya menentukan pesan mana yang ia baca:

| Urutan | Pesan yang terbaca | Yang ia lakukan setelahnya |
| --- | --- | --- |
| Kewenangan lebih dulu | "Hanya dokter radiolog yang boleh mengesahkan hasil bacaan." | Meminta hak aksesnya ditambah — **jalan yang salah** |
| **Pengesahan sendiri lebih dulu** | "Draf yang Anda tulis harus disahkan dokter radiolog." | Mencari dokter radiolog — **jalan yang benar** |

Urutan kedua yang dipakai, dan itu pula yang diminta `RAD-VAL-001` serta acceptance criteria
AC-1.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `docs/engineering/QBE_EXCEPTIONS.json` | Governance canonical dan preflight QBE |
| `rules/backend/` — `TASK_RULES`, `TASK_CLASSIFICATION`, `DATABASE_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE` | Aturan operasional task |
| `roadmap/backend-roadmap.md` bagian 4 | Batas pekerjaan `BE-RAD-08` |
| `contracts/state-transition-matrix.md` bagian 3 dan 4 | Seluruh transisi sah dan tidak sah |
| `contracts/validation-matrix.md` bagian 1 | Pesan dan kode galat, ditulis apa adanya |
| `contracts/permission-audit-matrix.md` bagian 3, 5.1, 6, 7, 8, 9 | Pemisahan wewenang, penanda peran, larangan log |
| `contracts/api-contract.md` bagian *Rad Report* | Nama DTO yang sudah terkunci |
| `erd/data-dictionary.md` bagian 1 dan 2 | Panjang kolom dan kewajiban isian |
| `Services/RadSafetyPolicyService.cs` | Pola terdekat: pengesahan berjenjang, penjagaan pengesahan sendiri |
| `Services/RegistrationManagement/EncounterIntakeService.cs` | Pola seam pemeriksaan kewenangan di dalam service |
| `Services/BillingManagement/.../BillingDepositService.cs`, `BillingNumberSeriesService.cs` | Pola transaksi, `pg_advisory_xact_lock`, dan penomoran |
| `Services/ClinicalManagement/PhysicianVisitNumberService.cs` | Pola alokator nomor milik modul sendiri |
| `Services/Security/AccessPermissionService.cs`, `Seeders/AccessMenuSeeder.cs`, `Attributes/Access*.cs` | Cara hak akses didaftarkan dan diperiksa — sumber temuan bagian 7 |
| `Models/RadReport.cs`, `RadReportVersion.cs`, `RadStudy.cs`, konfigurasinya | Bentuk data hasil `BE-RAD-07` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Services/RadReportService.cs` | **Baru.** Lima operasi: lahirkan wadah, tulis draf, ubah draf, sahkan, rilis. Memuat seluruh penjagaan pemisahan wewenang |
| `Areas/.../Services/RadReportNumberService.cs` | **Baru.** Alokator nomor bacaan sesuai `QBE-CODE-003` |
| `Areas/.../DTOs/RadReportDtos.cs` | **Baru.** Dua request dan dua response |
| `Areas/.../Services/RadOperationResult.cs` | Sepuluh kode galat hasil bacaan ditambahkan |
| `Program.cs` | Dua registrasi `AddScoped` |
| `Tests/.../RadiologyManagement/RadReportServiceTests.cs` | **Baru.** 41 uji |

### 3.3 Empat keputusan bentuk, beserta alasannya

**Pertama — pemeriksaan penanda radiolog dibuat `protected virtual`.** Penanda
`RadReport : ActAsRadiologist` dibaca lewat `AccessPermissionService.HasAccessAsync`. Method
pembacanya dibuat `protected virtual` semata-mata sebagai *seam* uji, mengikuti pola yang sudah
berjalan pada `EncounterIntakeService.HasRegistrationAuthorityAsync`. Tanpa seam itu, tujuh baris
tabel bagian 2.2 hanya dapat diuji dengan menyusun seluruh struktur RBAC — departemen, jabatan,
kebijakan akses — untuk setiap kasus. Produksi selalu memakai `AccessPermissionService` yang
sebenarnya.

**Kedua — peran penulis dinyatakan pemanggil, tetapi klaim "radiolog" tidak dipercaya.**
`RAD-PERM-001` bagian 6 menyebut penanda hanya membedakan radiolog dari bukan-radiolog; sistem
tidak punya cara membedakan residen dari radiografer. Karena itu pemanggil menyebut perannya, dan
service memakai aturan berikut:

| Yang dinyatakan pemanggil | Memegang penanda | Yang dibekukan |
| --- | :---: | --- |
| Tidak disebut | Ya | `Radiologist` |
| Tidak disebut | Tidak | **Ditolak `400`** — "Sebutkan peran Anda saat menulis draf ini" |
| `Radiologist` | Tidak | **Ditolak `403`** — bukan diturunkan diam-diam |
| `Resident`, `Radiographer`, `AiAssisted` | Ya atau tidak | Diterima apa adanya |

Klaim "radiolog" tanpa penanda **ditolak, bukan diturunkan diam-diam menjadi residen**.
Penurunan diam-diam membuat penulisnya mengira drafnya dapat ia sahkan sendiri, lalu bacaannya
tertahan tanpa sebab yang terbaca di layar mana pun. Sebaliknya, pernyataan yang **lebih rendah**
dari kewenangan sebenarnya diterima apa adanya — radiolog yang menandai drafnya sebagai hasil
bantuan AI hanya membuat aturan pengesahan makin ketat, dan kejujuran tentang asal-usul sebuah
draf lebih berharga daripada kerapian datanya.

**Ketiga — muatan log dibentuk satu pintu.** `RAD-PERM-001` bagian 7 mengharamkan `Findings`,
`Impression`, `Recommendation`, dan `AmendmentReason` masuk application log. Kalau setiap
pemanggilan logger menyusun muatannya sendiri, cukup satu kali seseorang menambahkan
`version.Impression` "supaya mudah ditelusuri" untuk memindahkan kesimpulan klinis seorang pasien
ke tempat yang aturan aksesnya berbeda dari tabel aslinya. Seluruh log hasil bacaan karena itu
dibentuk lewat satu class `RadReportLogPayload` yang **memang tidak punya kolom untuk
menampungnya**, dan dua uji menjaganya.

**Keempat — dua radiolog yang menekan Sahkan bersamaan.** Penjagaannya berlapis dua:

1. **Keadaan versi.** Yang kedua membaca versi yang sudah `Validated` lalu ditolak `409`.
2. **`pg_advisory_xact_lock` per bacaan** di dalam transaksi `Serializable`, mekanisme yang sama
   dengan yang sudah dipakai `BillingDepositService`. Ini yang membuat keduanya berjalan
   berurutan, bukan bersamaan.

Lapis kedua **hanya aktif pada penyedia relasional**. Batas pembuktiannya disebut di bagian 5.

### 3.4 Penomoran bacaan — `QBE-CODE-003`

Nomor berbentuk `RAD-RPT-260911074012-A1B2C3`: awalan, waktu sampai detik, lalu enam huruf/angka
acak dari Guid. Panjangnya tetap 27 karakter, muat pada kolom `varchar(64)`.

**Tidak memakai `Count + 1` maupun `Max + 1`**, dan alasannya nyata pada radiologi: dua radiolog
yang menyimpan draf pada detik yang sama akan membaca angka yang sama lalu menerbitkan dua bacaan
bernomor kembar. Nomor bacaan ikut tertulis pada surat hasil dan rekam medis — dua berkas
bernomor sama berarti tidak ada lagi cara menunjuk satu bacaan tertentu. Index unik
`IX_RadReport_ReportNumber` menjadi penjaga terakhirnya (`QBE-CODE-004`).

**Mengapa bukan provider bersama `BillingNumberSeriesService` (`QBE-CODE-006`).** Provider itu
beserta tabel `BilNumberSeries` dimiliki `BillingManagement`. Memanggilnya dari Radiologi berarti
menulis ke tabel milik modul lain tanpa wewenang, dan membuat tabel seri nomor sendiri berarti
entity baru beserta migration — di luar batas task ini. Bentuk yang dipakai sama persis dengan
`PhysicianVisitNumberService` dan `InpEpisodeNumberService`, dua alokator milik modulnya sendiri
yang sudah berjalan pada repository ini dan sudah melewati review dengan alasan yang sama.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint. Grup *Rad Report* pada `RAD-API-001` tetap berlabel rencana sampai `BE-RAD-09`. Empat DTO memakai nama yang sudah terkunci kontrak: `CreateRadReportDraftRequest`, `UpdateRadReportDraftRequest`, `RadReportDetailResponse`, `RadReportVersionResponse` |
| Database | **Tidak ada perubahan schema, entity, maupun migration.** Model dan configuration hasil `BE-RAD-07` dipakai apa adanya; `ApplicationDbContextModelSnapshot.cs` **tidak** disentuh task ini. Migration `AddRadReport` tetap **belum dijalankan** ke database mana pun, sehingga service ini belum pernah diuji terhadap tabel sungguhan |
| Keamanan/Auth | **Inti task ini.** Pemisahan wewenang `RAD-DEC-003` ditegakkan di service, bukan di atribut endpoint. Empat kolom sensitif dijaga tidak masuk log lewat `RadReportLogPayload`. **Satu penghalang belum selesai**: penanda `RadReport : ActAsRadiologist` belum dapat diberikan kepada peran mana pun — bagian 7 |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah maupun mengubah endpoint. Seluruh endpoint hasil
bacaan adalah pekerjaan `BE-RAD-09`.

---

## 5. Verifikasi

Build dan test dijalankan **terpisah dan berurutan**.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Berhasil, **0 error**, 188 warning, 2 menit 16 detik | `PASS` | Keluaran perintah |
| Warning baru dari berkas radiologi | **Tidak ada satu pun** | `PASS` | Penyaringan seluruh warning build atas `RadReport` dan `RadiologyManagement` |
| `dotnet build` project uji in-memory | Berhasil, **0 error**, 10 warning | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadReportServiceTests` | **41 lulus, 0 gagal**, 12 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadiologyManagement` | **162 lulus, 0 gagal**, 12 detik | `PASS` | 121 uji radiologi sebelumnya + 41 baru, cocok persis |
| `dotnet test --no-build` seluruh project in-memory | **1.367 lulus, 0 gagal**, 35 detik | `PASS` | Keluaran perintah |

### 5.1 Bukti per baris kontrak

**`RAD-STATE-001` bagian 3 — transisi sah**

| Transisi | Uji | Hasil |
| --- | --- | --- |
| — ke `Pending`, otomatis | `StudyLayakMelahirkanBacaanBerstatusPending` | `PASS` |
| `Pending` ke `Drafted` | `RadiologMenulisDrafPeranDibekukanSebagaiRadiolog`, `ResidenMenulisDrafPeranDibekukanSebagaiResiden` | `PASS` |
| `Drafted` ke `Drafted`, ubah draf | `PenulisMengubahDrafnyaSendiriDiterima` | `PASS` |
| `Drafted` ke `Validated` | Tujuh uji tabel bagian 2.2 | `PASS` |
| `Validated` ke `Released` | `RilisSetelahDisahkanBerhasil` | `PASS` |

**`RAD-STATE-001` bagian 3 — transisi tidak sah**

| Percobaan | Uji | Hasil |
| --- | --- | --- |
| Penulis bukan-radiolog mengesahkan drafnya sendiri | `AC1_ResidenMengesahkanDrafnyaSendiriDitolak`, `AC2_RadiograferMengesahkanDrafnyaSendiriDitolak`, `AC2_DrafBantuanAiTidakDapatDisahkanDirinyaSendiri` | `PASS` |
| Mengubah isi versi berstatus `Released` | `IsiVersiYangSudahDirilisTidakDapatDiubah` | `PASS` |
| Draf atas study yang `IsUsable` bernilai `false` | `DrafAtasStudyYangDinyatakanTidakLayakDitolak` | `PASS` |
| Draf atas study yang `IsUsable` masih `null` | `DrafAtasStudyYangMutunyaBelumDinilaiDitolak` | `PASS` |
| Bacaan kedua atas study yang sama | `BacaanKeduaAtasStudyYangSamaDitolak` | `PASS` |
| Merilis bacaan yang belum disahkan | `MerilisBacaanYangBelumDisahkanDitolak` | `PASS` |
| Menghapus versi mana pun | Tidak ada method hapus pada service | `PASS` — dibuktikan ketiadaannya |

**`RAD-VAL-001` bagian 1** — sebelas baris yang berlaku pada task ini dibuktikan; pesannya
dibandingkan **persis huruf per huruf** pada tiga uji terpenting (`AC1`, pengesah bukan radiolog,
rilis sebelum sah).

**`RAD-PERM-001` bagian 7** — `MuatanLogTidakMemilikiKolomUntukIsiBacaan` dan
`MuatanLogTidakMembawaKesimpulanKlinisPasien`.

**`QBE-CODE-003` dan `QBE-CODE-004`** — `NomorBacaanTidakPernahKembarWalauDibentukPadaDetikYangSama`
membentuk 1.000 nomor dari waktu yang sama persis dan membuktikan tidak ada satu pun yang kembar.

Uji manual: `NOT APPLICABLE` — tidak ada endpoint maupun layar.

### 5.2 Batas verifikasi yang perlu diketahui

| Yang belum terbukti | Sebabnya |
| --- | --- |
| Dua pengesahan yang benar-benar **bersamaan** tertahan | Yang terbukti adalah penekanan tombol yang **berurutan**. Penyedia in-memory tidak mendukung transaksi maupun `pg_advisory_xact_lock`, sehingga lapis kedua penjagaan tidak ikut berjalan pada uji. Pembuktiannya menuntut database sungguhan |
| Index unik benar-benar menolak bacaan kedua atas satu study | Yang terbukti adalah pemeriksaan di service. Penegakan index menunggu migration dijalankan |
| Perilaku terhadap tabel sungguhan | Migration `AddRadReport` belum dijalankan ke database mana pun |
| `AccessPermissionService` benar-benar menjawab penanda radiolog | Diganti seam pada uji. Sampai penghalang bagian 7 diselesaikan, jawabannya di sistem sebenarnya adalah `false` untuk semua orang kecuali SuperAdmin |

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| `dotnet ef migrations add` | Tidak ada perubahan model. Tidak diperlukan |
| `dotnet ef database update` ke database mana pun | **Wewenang terpisah dan belum diberikan** |
| Uji integrasi Postgres radiologi | `QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi; tabelnya juga belum ada |
| Analyzer build penuh | Dijalankan dengan `-p:RunAnalyzers=False` atas permintaan pemilik modul untuk memangkas waktu build dari lebih 10 menit menjadi 2 menit. Warning compiler tetap dihitung dan disaring |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — residen mengesahkan drafnya sendiri ditolak `403` dengan pesan yang ditetapkan | **Terpenuhi** | `AC1_ResidenMengesahkanDrafnyaSendiriDitolak`, pesan dibandingkan persis |
| AC-2 — tujuh baris tabel pengesahan berlaku sebagaimana ditulis | **Terpenuhi** | Tujuh uji, satu per baris |
| AC-3 — residen yang kemudian menjadi radiolog tetap ditolak atas draf lamanya | **Terpenuhi** | `AC3_ResidenYangKemudianMenjadiRadiologTetapDitolakAtasDrafLamanya` |
| AC-4 — riwayat mencatat penulis dan pengesah sebagai dua jejak terpisah | **Terpenuhi** | `AC4_PenulisDanPengesahTerekamSebagaiDuaJejakTerpisah` |
| DoD — seluruh transisi sah dan tidak sah pada `RAD-STATE-001` bagian 3 terbukti lewat uji | **Terpenuhi untuk perilaku service** | Tabel bagian 5.1. Transisi amandemen (`AmendmentDrafted` dan seterusnya) **sengaja tidak dikerjakan** — milik `BE-RAD-10` |
| Pemeriksaan `ActAsRadiologist` berada di service, bukan hanya atribut endpoint | **Terpenuhi pada kodenya** | `RadReportService.HasRadiologistAuthorityAsync`. **Belum berfungsi di sistem sebenarnya** karena penandanya belum dapat diberikan — bagian 7 |

**Butir yang belum terpenuhi, disebut apa adanya:**

1. **Titik pemanggilan kelahiran bacaan belum tersambung.** `EnsurePendingReportAsync` sudah ada
   dan diuji, tetapi `RadStudyService` belum memanggilnya saat study berpindah ke
   `QualityAccepted`. Hari ini wadah bacaan lahir pada saat draf pertama ditulis, bukan pada saat
   citra dinyatakan layak. Penyambungannya menyentuh `RadStudyService`, yang **tidak** tercantum
   pada "Yang dikerjakan" `BE-RAD-08`, sehingga tidak dikerjakan sepihak.
2. **Penanda `RadReport : ActAsRadiologist` belum dapat diberikan.** Lihat bagian 7.
3. **Perilaku terhadap database sungguhan belum terbukti**, karena migration `AddRadReport`
   belum dijalankan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi. Build menghasilkan 188 warning, seluruhnya sudah ada sebelumnya dan berasal dari modul lain |
| Masalah yang diketahui | **Penanda `RadReport : ActAsRadiologist` belum dapat diberikan kepada peran mana pun** — rinciannya di bawah. Selain itu, `RadReport.Version` dan `RadReportVersion.Version` didokumentasikan sebagai token konkurensi pada model, tetapi configuration hasil `BE-RAD-07` **tidak** memanggil `IsConcurrencyToken()`, sehingga database tidak menegakkannya. Tidak diperbaiki di sini karena mengubah model EF tanpa menambah migration akan membuat `ApplicationDbContextModelSnapshot.cs` berselisih dengan modelnya |
| Risiko tersisa | **Pertama**, penghalang hak akses di bawah membuat seluruh alur pengesahan tidak dapat dijalankan siapa pun kecuali SuperAdmin. **Kedua**, migration belum dijalankan, sehingga service ini belum pernah menyentuh tabel sungguhan. **Ketiga**, penjagaan konkurensi lapis kedua belum terbukti |
| Perubahan sampingan | `NONE` |
| Interupsi | Build pertama dihentikan pemilik modul karena melewati 10 menit, lalu diulang dengan `-p:RunAnalyzers=False` dan selesai dalam 2 menit 16 detik. Tidak ada pekerjaan yang hilang; seluruh verifikasi di bagian 5 berasal dari build ulang itu |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | Selesaikan penghalang hak akses, lalu `BE-RAD-09` — endpoint hasil bacaan |

### 7.1 Penghalang: penanda `ActAsRadiologist` belum dapat diberikan

> **DITUTUP 2026-09-11.** Pemilik modul memutuskan menjalankan **pilihan A** di bawah.
> `AccessMenuSeeder` kini mendaftarkan hak akses penanda lewat
> `PenandaTanpaEndpointYangDidaftarkan`, dan `RadReport : ActAsRadiologist` dapat dicentang pada
> layar Akses Role. Celah pada uji kontrak hak akses — yang membuat penghalang ini luput selama
> empat task — ikut ditutup dengan butir kelima.
>
> Rinciannya: `approval-requests/2026-09-11-keputusan-empat-penghalang.md` bagian 1.
> Sisa pekerjaannya milik Administrator: **memberikan penanda itu kepada peran yang memang dokter
> radiolog.**
>
> Keterangan di bawah ini dibiarkan apa adanya sebagai catatan keadaan saat `BE-RAD-08`
> dikerjakan.

**Apa yang ditemukan.** `RAD-PERM-001` bagian 6 menetapkan `RadReport : ActAsRadiologist`
**tidak menempel pada satu endpoint pun** — ia hanya dibaca service. Sementara itu,
`AccessMenuSeeder` mendaftarkan pasangan hak akses dengan menelusuri seluruh action MVC dan hanya
mencatat yang punya `[AccessController]` pada class **dan** `[AccessAction]` pada method-nya.

Akibatnya pasangan `RadReport : ActAsRadiologist` tidak pernah masuk tabel `SysActionAccesses`.
`HasAccessAsync` mencari pasangan itu, tidak menemukannya, lalu mengembalikan `false`.

**Akibat nyatanya bila dipakai sekarang** — dengan `Security:Authorization:Enabled` bernilai
`true` dan `EnforceClinicalPolicyForSuperAdmin` bernilai `false`:

| Pengguna | Dihitung radiolog? | Yang ia alami |
| --- | :---: | --- |
| SuperAdmin | Ya | Alur berjalan |
| Siapa pun selain SuperAdmin | **Tidak** | Tidak dapat menulis draf sebagai radiolog, **dan tidak dapat mengesahkan maupun merilis bacaan apa pun** |

Ini persis kegagalan nomor dua yang diperingatkan `RadiologyRoleAccessContractTests` hasil
`BE-RAD-14`: hak akses yang diperiksa kode tetapi tidak pernah didaftarkan menghasilkan `403`
permanen yang **tidak dapat diperbaiki dari layar mana pun**, karena baris untuk dicentangnya
memang tidak ada.

**Mengapa tidak diperbaiki di task ini.** Memperbaikinya menuntut baris `SysControllerAccess`
untuk `RadReport`, dan baris itu lahir dari `RadReportController` yang belum dibuat — itu
pekerjaan `BE-RAD-09`. Menyuntik baris hak akses ke seeder bersama untuk controller yang belum
ada berarti mengarang struktur akses, dan mengubah `RAD-PERM-001` yang berstatus `approved`
adalah wewenang pemilik modul, bukan wewenang task ini.

**Yang perlu Anda putuskan.** Dua jalan, dan keduanya perlu dikerjakan pada `BE-RAD-09`:

| Pilihan | Isinya | Konsekuensinya |
| --- | --- | --- |
| **A — daftarkan sebagai aksi tanpa endpoint** | Setelah `RadReportController` ada, tambahkan satu baris pendaftaran `ActAsRadiologist` pada seeder sehingga muncul di layar Akses Role | Menjaga `RAD-PERM-001` bagian 6 apa adanya: penanda tetap terpisah dari `Validate`. Perlu satu jalur pendaftaran baru pada seeder bersama |
| **B — tempelkan pada sebuah endpoint** | Sediakan endpoint baca yang memakai `[AccessAction("ActAsRadiologist", …)]`, misalnya pemeriksaan kewenangan untuk layar | Tidak menyentuh seeder, tetapi menambah endpoint yang tidak diminta `RAD-API-001` dan perlu amandemen kontrak |

Sampai salah satunya dikerjakan, aturan `RAD-DEC-003` **ada di kode tetapi belum berjalan di
sistem sebenarnya**, dan keadaan itu disebut apa adanya di sini.

### 7.2 Status Git pada akhir pekerjaan

```text
 M Areas/HealthServices/RadiologyManagement/Services/RadOperationResult.cs
 M Program.cs
?? Areas/HealthServices/RadiologyManagement/DTOs/RadReportDtos.cs
?? Areas/HealthServices/RadiologyManagement/Services/RadReportNumberService.cs
?? Areas/HealthServices/RadiologyManagement/Services/RadReportService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadReportServiceTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-08.md
```

Berkas lain yang tampak pada `git status` — Laboratorium, `BE-RAD-04`, `BE-RAD-05`, `BE-RAD-07`,
`BE-RAD-14`, `BE-RAD-15`, migration `AddRadReport`, dan model hasil `BE-RAD-07` — sudah ada
sebelum task ini dimulai dan **bukan** hasil pekerjaan ini.

Tidak ada `git add`, commit, maupun push yang dilakukan. **Tidak ada perintah database yang
dijalankan.**
