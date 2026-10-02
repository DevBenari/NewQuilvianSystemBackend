# Laporan Perubahan Backend — `BE-RAD-15`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-15` |
| Judul | Rencana dan pengisian data master awal |
| Slice | `S4` dan `S13` — pengelolaan aturan keselamatan dan data induk alat |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 3, gelombang `MVP-1` |
| Trace | `RJ-BIL-DEC-014`, `RAD-DEC-002`, `RAD-DEC-005`, **`DEC-RAD-005`**; `RAD-ARCH-BE-001` bagian 9 |
| Contract version | `RAD-ARCH-BE-001` bagian 9 — status `approved`; pemetaan butir per alat di dalamnya berstatus **usulan**, bukan kebijakan |
| Dependency | `BE-RAD-03`, `BE-RAD-04`, `BE-RAD-05` — seluruhnya **terpenuhi** |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 0, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/Seeders/`, `Program.cs`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **SELESAI SEBAGIAN — dan sisanya sengaja tidak dikerjakan backend.** Alat pencitraan dan butir keselamatan terisi; aturan keselamatan disusun sebagai **draf**, bukan diberlakukan. Definition of Done "setiap alat punya aturan aktif" **belum terpenuhi**, terblokir `DEC-RAD-005` yang masih `OPEN` dengan pemilik tata kelola klinis |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Submodule | Seeders |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk `RadiologyMasterDataSeeder`; `TOUCHED LEGACY` untuk `Program.cs` |
| QBE ID yang berlaku | `QBE-MOD-001`; `QBE-VAL-001`; `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-API-001`, `QBE-DB-*` — tidak ada entity, configuration, endpoint, maupun migration |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Seluruh tabel master Radiologi **kosong**. Tidak ada satu pun alat pencitraan terdaftar, tidak
ada butir keselamatan, dan tidak ada aturan. Karena gerbang bersifat fail-closed, akibatnya
persis seperti yang dicatat `RAD-FACT-009`: **modul tidak dapat menjalankan satu pun
pemeriksaan.**

Tiga task sebelumnya sudah membuat semuanya dapat diisi lewat aplikasi. Yang belum ada adalah
isinya.

---

## 2. Proses bisnis

**Tujuan.** Menyiapkan isi awal sampai modul tinggal menunggu satu hal saja: keputusan klinis.

### 2.1 Yang dikerjakan seeder

| Tabel | Yang diisi | Dasarnya |
| --- | --- | --- |
| `MstRadModality` | Enam alat: X-Ray `CR`, CT-Scan `CT`, MRI `MR`, USG `US`, Mamografi `MG`, Fluoroskopi `RF` | `RAD-DEC-002` yang sudah **disetujui**; kodenya mengikuti kode modalitas DICOM — kosakata teknis internasional |
| `MstRadSafetyRequirement` | Empat butir: skrining kehamilan, skrining implan logam atau alat pacu jantung, riwayat alergi media kontras, pemeriksaan fungsi ginjal sebelum kontras | `RAD-ARCH-BE-001` bagian 9; setiap baris membawa `SourceNote` yang menyatakan ini baseline implementasi, **bukan** SOP yang sudah disahkan |
| `MstRadModalitySafetyRule` | Dua belas usulan aturan, **seluruhnya berstatus `Draft`** | Usulan pemetaan `RAD-ARCH-BE-001` bagian 9 |

### 2.2 Mengapa aturannya berhenti di draf

Aturan keselamatan menentukan pertanyaan apa yang wajib dijawab sebelum seorang pasien
disinari. `RJ-BIL-DEC-014` menyatakan daftar akhirnya mengikuti SOP dan otoritas klinis —
"bukan keputusan dokumen ini". `DEC-RAD-005` masih berstatus **`OPEN`**, dan pemiliknya
**penanggung jawab tata kelola klinis**, bukan backend.

Seeder yang menerbitkan aturan berstatus `Active` berarti sebuah program menetapkan kapan
pasien boleh disinari. Itu bukan wewenang program.

Usulan baseline pada `DEC-RAD-005` sendiri berbunyi: *"Sediakan data bawaan sebagai usulan
berstatus `Draf`, sehingga tetap harus disahkan penanggung jawab klinis sebelum berlaku."*
Itulah yang dikerjakan.

### 2.3 Apa yang terjadi setelah seeder berjalan

1. Enam alat dan empat butir keselamatan terdaftar dan siap dipakai.
2. Dua belas usulan aturan tersimpan sebagai draf — **tidak satu pun berlaku**.
3. `GET /rad-safety-rules/coverage` **tetap mengembalikan keenam alat**.
4. Setiap pemeriksaan pada alat mana pun **tetap ditolak** dengan
   `RAD_SAFETY_POLICY_NOT_CONFIGURED`.
5. Log server mencatat peringatan setiap kali menyala, menyebut berapa alat yang belum
   tercakup.

**Itu bukan kegagalan seeder.** Itu sifat fail-closed yang bekerja sebagaimana mestinya.

### 2.4 Langkah yang tersisa, dan siapa yang mengerjakannya

| Langkah | Pelaku | Caranya |
| ---: | --- | --- |
| 1 | Admin Radiologi | Buka daftar aturan, periksa dua belas draf, perbaiki bila perlu |
| 2 | Admin Radiologi | `POST /rad-safety-rules/{id}/submit` untuk setiap draf yang sudah benar |
| 3 | **Penanggung jawab klinis** | Periksa pengajuan terhadap SOP rumah sakit, lalu `POST /rad-safety-rules/{id}/approve` |
| 4 | Siapa pun | `GET /rad-safety-rules/coverage` sampai daftarnya **kosong** |

Langkah 2 dan 3 **wajib dua orang berbeda**. Pengaman pengesahan sendiri dari `BE-RAD-02`
berlaku penuh untuk draf bawaan ini — tidak ada jalan pintas lewat data bawaan. Hal itu
ditemukan justru ketika uji pertama gagal, dan sekarang menjadi uji tersendiri.

### 2.5 Dua hal yang sengaja tidak ditebak

**"Wajib bila berkontras".** `RAD-ARCH-BE-001` bagian 9 menandai alergi kontras dan fungsi
ginjal sebagai wajib **hanya bila pemeriksaannya memakai media kontras**. Model aturan saat ini
tidak dapat menyatakan syarat itu — yang ada hanya wajib atau tidak wajib, dan penyaring per
pemeriksaan. Enam baris itu karena itu disusun **tidak wajib**, dengan catatan yang menyebutkan
syaratnya apa adanya:

> Usulan: wajib hanya bila pemeriksaannya memakai media kontras. Syarat itu belum dapat
> dinyatakan model aturan saat ini, sehingga baris ini disusun tidak wajib. Penanggung jawab
> klinis dapat mengubahnya menjadi wajib, atau memecahnya menjadi aturan per pemeriksaan
> berkontras, sebelum mengesahkan.

**USG.** Tidak punya satu pun butir wajib pada bagian 9. Karena gerbang fail-closed, USG tetap
**membutuhkan** sedikitnya satu aturan berlaku — tanpa itu seluruh pemeriksaan USG tertolak.
Drafnya disediakan dengan butir skrining kehamilan ditandai tidak wajib, beserta catatan yang
menjelaskan mengapa barisnya ada dan mempersilakan menggantinya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Seeders/RadiologyMasterDataSeeder.cs` | **Baru.** Mengisi alat, butir, dan usulan aturan berstatus draf; idempoten; tidak pernah menimpa baris yang sudah ada |
| `Program.cs` | Pendaftaran seeder pada rangkaian startup, sesudah `LabRejectionReasonSeeder` |
| `Tests/.../RadiologyManagement/RadiologyMasterDataSeederTests.cs` | **Baru.** 10 uji tanpa database |

### 3.2 Keputusan rancangan

**Idempoten dan tidak pernah menimpa.** Seeder hanya menambah kode yang belum ada. Baris yang
sudah ditandai terhapus ikut dihitung supaya kodenya tidak diisi ulang, dan kombinasi alat–butir
yang sudah punya baris tidak diusulkan lagi walaupun Id-nya berbeda. Nama, penanda, urutan, dan
keputusan pengesahan adalah milik penggunanya; menimpanya setiap kali server menyala berarti
membatalkan keputusan mereka diam-diam.

**Id ditetapkan tetap, bukan dibangkitkan.** Sebuah alat punya identitas yang sama di setiap
lingkungan dan dapat dirujuk dengan pasti — mengikuti pola `LabRejectionReasonSeeder`.

**Peringatan log ditulis setiap kali, bukan hanya saat mengisi.** Selama masih ada alat yang
belum tercakup, modul belum dapat dipakai memeriksa pasien, dan itu perlu terbaca setiap kali
server menyala.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint yang ditambah atau diubah |
| Database | Tidak ada perubahan schema maupun migration. Seeder **menulis baris data** saat aplikasi menyala, mengikuti pola seeder yang sudah berjalan, dan dapat dimatikan lewat `SeedDefaultData:Enabled`. **Tidak ada perintah database yang dijalankan pada task ini** |
| Keamanan/Auth | `NOT APPLICABLE` untuk authorization. Dampaknya pada **keselamatan klinis**: seeder sengaja tidak dapat membuat satu pun aturan berlaku |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah endpoint. Endpoint yang dipakai untuk menyelesaikan
langkah berikutnya sudah tersedia sejak `BE-RAD-03`.

---

## 5. Verifikasi

Build dan test dijalankan **terpisah dan berurutan**.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -m:1 -p:RunAnalyzers=false` | Berhasil, **0 error**, 1 menit 36 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build` — percobaan pertama | **2 gagal, 86 lulus** | `NEW ERROR`, sudah diperbaiki | Perancah uji, bukan seeder. Lihat penjelasan di bawah |
| `dotnet test --no-build` — setelah perbaikan | **88 lulus, 0 gagal**, 11 detik | `PASS` | Keluaran perintah |
| Enam alat dan empat butir terisi | Kode alat `CR`, `CT`, `MG`, `MR`, `RF`, `US`; penanda pengion dan kontras benar | `PASS` | `MengisiEnamAlatDanEmpatButirKeselamatan` |
| Setiap butir menyatakan asal-usulnya | `SourceNote` terisi dan memuat "Bukan SOP rumah sakit" | `PASS` | `SetiapButirMenyatakanAsalUsulnyaApaAdanya` |
| **Tidak satu pun aturan lahir berlaku** | Seluruhnya `Draft`, `IsActive` `false`, `ApprovedAt` kosong, versi tetap 1 | `PASS` | `TidakSatuPunAturanLahirBerlaku` |
| **Gerbang tetap menolak seluruh alat setelah seeder** | `AlatBelumTercakup` bernilai 6; `GET /coverage` mengembalikan 6 baris | `PASS` | `GerbangTetapMenolakSeluruhAlatSetelahSeederBerjalan` |
| Setiap alat sudah punya draf yang tinggal disahkan | `DraftOrPendingRuleCount` lebih dari nol pada keenam alat | `PASS` | Uji yang sama |
| Cakupan menjadi kosong setelah seluruh draf disahkan | Satu draf per alat diajukan admin lalu disahkan penanggung jawab klinis; `GET /coverage` menjadi **kosong** | `PASS` | `CakupanMenjadiKosongSetelahSeluruhDrafDisahkan` |
| USG ikut mendapat draf walau tidak punya butir wajib | Satu baris, tidak wajib, catatannya menyebut fail-closed | `PASS` | `UsgIkutMendapatDrafWalauTidakPunyaButirWajib` |
| Aturan berkontras disusun tidak wajib dan menyebut syaratnya | `IsMandatory` `false`; catatannya menyebut syarat kontras | `PASS` | `AturanBerkontrasDisusunTidakWajibDanMenyebutSyaratnya` |
| Dijalankan dua kali tidak menambah baris kembar | Putaran kedua menambah 0 alat, 0 butir, 0 draf | `PASS` | `DijalankanDuaKali_TidakMenambahBarisKembar` |
| Tidak menghidupkan kembali aturan yang sudah dihentikan | Aturan tetap `Inactive`; tidak ada draf pengganti diusulkan | `PASS` | `TidakMenghidupkanKembaliAturanYangSudahDihentikan` |
| Tidak menimpa alat yang sudah disunting pengguna | Nama dan status aktif hasil suntingan tetap utuh | `PASS` | `TidakMenimpaAlatYangSudahDisuntingPengguna` |
| Regresi 78 uji radiologi yang sudah ada | Seluruhnya tetap lulus tanpa diubah | `PASS` | Lima berkas uji radiologi sebelumnya |
| Warning baru dari berkas radiologi | Tidak ada satu pun | `PASS` | Penyaringan seluruh warning build |

Uji manual: `NOT FEASIBLE` — belum ada layar; seeder berjalan saat aplikasi menyala, dan
menjalankan aplikasi tidak diminta task ini.

### Mengapa dua uji gagal pada percobaan pertama, dan apa artinya

Kedua uji itu menyahkan draf memakai **pelaku yang sama** dengan yang mengajukannya.
`BE-RAD-02` menolaknya sebagai pengesahan sendiri, sehingga aturannya berhenti di
`PendingApproval` dan cakupan tidak pernah kosong.

Temuan ini berharga, bukan sekadar salah ketik: **pengaman pengesahan sendiri berlaku penuh
untuk draf yang disiapkan seeder.** Data bawaan tidak memberi jalan pintas kepada siapa pun.
Perbaikannya membuat uji memakai dua pelaku berbeda, dan hasil `Submit` serta `Approve` kini
ikut diperiksa supaya kegagalan diam-diam seperti itu tidak terulang.

### Batas verifikasi

Seluruh uji berjalan di atas penyedia **in-memory**. Yang belum terbukti: perilaku seeder
terhadap PostgreSQL sungguhan, termasuk index unik `ModalityCode` dan `RequirementCode` bila
dua instance aplikasi menyala bersamaan.

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| Menjalankan aplikasi supaya seeder benar-benar mengisi database | Eksekusi runtime dan penulisan ke database adalah wewenang terpisah, dan tidak diminta task ini |
| Migration | Tidak ada perubahan schema |
| Pengesahan aturan menjadi `Active` | **Bukan wewenang backend.** Inilah inti `DEC-RAD-005` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-6 — enam alat sesuai `RAD-DEC-002` terdaftar | **Terpenuhi** | `MengisiEnamAlatDanEmpatButirKeselamatan` |
| Sekurang-kurangnya empat butir keselamatan | **Terpenuhi** | Empat butir sesuai `RAD-ARCH-BE-001` bagian 9 |
| AC-17 — alat tanpa aturan aktif dapat diketahui di depan | **Terpenuhi** | `GET /coverage` bekerja dan angkanya benar; peringatan ikut ditulis ke log |
| **Sekurang-kurangnya satu aturan `Active` untuk setiap alat** | **BELUM TERPENUHI** | Terblokir `DEC-RAD-005`, pemilik tata kelola klinis. Draf sudah disiapkan untuk keenam alat |
| **Test: `GET /coverage` mengembalikan daftar kosong** | **BELUM TERPENUHI** | Sesudah seeder, cakupan berisi enam alat — sengaja. Dibuktikan cakupan **menjadi kosong** begitu drafnya disahkan |
| Definition of Done: setiap alat punya aturan aktif, termasuk USG | **BELUM TERPENUHI** | Sama seperti di atas; draf USG sudah tersedia beserta alasannya |

### Yang menahan, dan siapa yang dapat melepaskannya

| Hal | Isi |
| --- | --- |
| Penahan | `DEC-RAD-005` — "Sumber nilai awal aturan keselamatan", status `OPEN` |
| Pemilik | Penanggung jawab tata kelola klinis |
| Yang dibutuhkan | Pemeriksaan dua belas usulan aturan terhadap SOP radiologi rumah sakit, lalu pengesahannya |
| Yang sudah disiapkan | Seluruh draf, endpoint pengesahan, dan cara memastikan hasilnya — `GET /coverage` |
| Perkiraan pekerjaan tersisa | Dua belas kali periksa-dan-sahkan oleh dua orang berbeda. Tidak ada pekerjaan backend tersisa |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | Model aturan belum dapat menyatakan "wajib hanya bila memakai kontras". Enam baris terdampak disusun tidak wajib beserta catatan syaratnya. Bila rumah sakit menghendaki syarat itu ditegakkan sistem, diperlukan perubahan model — task tersendiri |
| Risiko tersisa | **Pertama**, selama draf belum disahkan, modul tetap tidak dapat memeriksa satu pun pasien. Itu disengaja, tetapi perlu diketahui semua pihak sebelum tanggal rilis dijanjikan. **Kedua**, seeder belum pernah berjalan terhadap PostgreSQL sungguhan. **Ketiga**, isi draf berasal dari usulan arsitektur, bukan SOP terverifikasi — mengesahkannya tanpa memeriksa akan mengubah usulan menjadi kebijakan tanpa ada yang benar-benar menimbangnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | Dari sisi backend: `BE-RAD-14` — uji kontrak hak akses, yang juga akan membuktikan disiplin `RadSafetyRule : Deactivate` pada `RAD-PERM-001` bagian 5.3. Dari sisi tata kelola: **penutupan `DEC-RAD-005`**, yang menentukan kapan modul benar-benar dapat dipakai |

### Status Git pada akhir pekerjaan

```text
 M Program.cs
?? Areas/HealthServices/RadiologyManagement/Seeders/RadiologyMasterDataSeeder.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadiologyMasterDataSeederTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-15.md
```

Ditambah berkas `BE-RAD-04` dan `BE-RAD-05` yang belum di-commit, beserta dokumen kontrak dan
roadmap yang ikut diperbarui. Empat berkas Laboratorium (`LabPatientRegistration*`) **bukan**
hasil pekerjaan ini.

Tidak ada `git add`, commit, maupun push yang dilakukan.
