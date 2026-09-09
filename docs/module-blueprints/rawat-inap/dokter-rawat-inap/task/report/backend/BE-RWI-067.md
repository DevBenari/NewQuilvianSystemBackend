# Laporan Perubahan Backend — `BE-RWI-067`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-067` |
| Judul | Daftar pantau verifikasi menyebut nama penulis, bukan nomor pengguna |
| Slice | `DOK-MVP-7` — menutup tiga celah kontrak yang ditemukan layar |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 kartu `BE-RWI-067`; bagian 0.1 dan 5 |
| Trace | `CAP-021`; `FE-DOK-08`; `contracts/api-contract.md` bagian 3 baris `GET /episodes/{episodeId}/verification-status`; `03-frontend-architecture.md` bagian 3.8; `RWI-DEC-062`. **Ditemukan** saat `FE-RWI-050` dikerjakan |
| Contract version | `0.3.0` — **tidak berubah**. Endpoint-nya sudah dikontrak beserta hak aksesnya, dan kontrak tidak pernah merinci daftar kolom `VerificationStatusResponse`; melengkapi butirnya adalah mengisi kontrak yang sudah ada |
| Dependency | `BE-RWI-053` ✅ selesai 4 September 2026. Tidak menunggu `BE-RWI-066` |
| Klasifikasi | `MEDIUM` — skor 5: repository 0 (satu repository), berkas diperiksa 0 (≤8), berkas diubah 0 (≤3), logika bisnis 1 (sedang — aturan penamaan berjenjang yang mengikat), kontrak API 1 (memakai kontrak yang sudah ada), database 1 (hanya perilaku query yang sudah ada), keamanan/auth 1 (privasi data minimum), UI/workflow 0 |
| Task mode | `BACKEND` — backend target tulis, frontend rujukan hanya-baca |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, laporan ini, roadmap, dan `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | Dikerjakan di atas `21b8575db3ea941862b7c5894a662d5ab3719934`, branch `MHamzah`. Agent tidak menjalankan satu pun tindakan Git |
| Tanggal | 8 September 2026 |
| Status | ✅ **SELESAI.** Keenam acceptance criteria terpenuhi dan terpetakan ke source. Validasi nyata: `dotnet build` **0 error**; `dotnet test` **`Failed: 0, Passed: 470, Total: 470`**, 8 di antaranya uji baru task ini. **Nol migration, nol kolom tabel baru, nol endpoint baru** |

---

## 1. Masalah yang diperbaiki

Daftar pantau verifikasi menjawab pertanyaan supervisor klinis: **catatan mana yang belum
diverifikasi, dan siapa yang perlu diingatkan?** Sampai sebelum task ini, ia hanya menjawab
setengahnya.

Butir daftar pantau membawa `ProviderUserId` — deretan angka dan huruf seperti
`8f3a1c92-...`. Itu bukan nama, dan tidak menolong siapa pun. Akibatnya layar `FE-RWI-050`
terpaksa menuliskan **"Nama penulis belum tersedia"** pada kolom Penulis, dan supervisor harus
membuka catatannya satu per satu hanya untuk tahu siapa penulisnya.

**Contoh berangka.** Satu bangsal dengan 12 catatan menunggu verifikasi. Supervisor ingin
mengirim satu pesan kepada tiap perawat yang catatannya tertunggak. Sebelum task ini, ia harus
membuka 12 catatan satu per satu — 12 kali klik dan 12 kali menunggu halaman — hanya untuk
mengumpulkan nama. Sesudah task ini, kedua belas nama itu terbaca sekaligus pada satu daftar.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Supervisor klinis langsung tahu **siapa yang menulis** setiap catatan yang menunggu verifikasi, tanpa membuka catatannya |
| Pelaku | Siapa pun yang berhak membaca keadaan verifikasi satu perawatan |
| Pemicu | Membuka daftar pantau verifikasi, atau membaca keadaan verifikasi sebuah perawatan |

### 2.2 Alur normal

1. Sebuah catatan ditulis. Saat itu juga, nama penulisnya **disalin** ke kolom snapshot pada
   barisnya — mekanisme yang sudah ada sebelum task ini.
2. Bila kebijakan verifikasi aktif, catatan itu masuk daftar pantau berstatus menunggu.
3. Supervisor membuka daftar pantau. Setiap butir kini menyebut **nama penulisnya**, di samping
   nomor catatan, jenis profesi, waktu, keadaan verifikasi, batas waktu, dan penanda terlambat.
4. Namanya diambil **berjenjang: snapshot lebih dulu, baru relasi pengguna.**

### 2.3 Aturan penamaan yang mengikat, beserta contohnya

Urutan jenjangnya bukan selera. Daftar pantau adalah **catatan historis**, dan akun yang berganti
nama tidak boleh menulis ulang siapa yang menulis catatan lama.

| Tanggal | Peristiwa | Nama pada daftar pantau untuk catatan 1 September |
| --- | --- | --- |
| 1 September | Ns. Sari menulis catatan. Snapshot terisi `Ns. Sari` | `Ns. Sari` |
| 5 September | Akunnya berganti nama menjadi `Ns. Sari Wijaya` | **Tetap `Ns. Sari`** |

Bila urutannya dibalik — nama akun lebih dulu — riwayat akan ditulis ulang setiap kali seseorang
menikah, bergelar baru, atau namanya diperbaiki.

### 2.4 Jalur tidak normal

| Keadaan | Nama pada butir daftar pantau |
| --- | --- |
| Snapshot kosong atau hanya berisi spasi | Jatuh ke nama akun penulis |
| Penulis tidak dapat dikenali sama sekali | **Kosong.** Nomor pengguna **sengaja tidak** ditampilkan sebagai penggantinya, dan nama tidak pernah ditebak |
| Kebijakan verifikasi belum aktif | Daftar pantau kosong beserta penanda `isVerificationPolicyEmpty`, persis seperti sebelumnya. Nol perubahan perilaku |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap dan kontrak: kartu `BE-RWI-067` beserta bagian 0.1 dan 5 `backend-roadmap.md`;
  `contracts/api-contract.md` bagian 3; laporan [`FE-RWI-050`](../frontend/FE-RWI-050.md) yang
  menemukan celahnya.
- Source: `CpptVerificationService.cs`, `TrxPatientIntegratedProgressNote.cs`,
  `PatientIntegratedProgressNoteController.cs`, `ApplicationUser.cs`.
- Frontend sebagai rujukan konsumen, **hanya dibaca**:
  `cppt-verification-monitoring-columns.jsx`, `use-cppt-verification-monitoring.jsx`,
  `inpatient-integrated-note-utils.jsx`.
- Uji: `CpptVerificationTests.cs` beserta `Infrastructure/` sebagai pola.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/CpptVerificationService.cs` | Kolom `ProviderName` pada `CpptVerificationWatchItem`; projeksi query ikut membaca kedua sumber namanya; helper `NamaPenulis` yang menetapkan urutan snapshot-lebih-dulu |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/CpptVerificationWatchListAuthorTests.cs` | **Baru.** Delapan uji yang menutup keenam acceptance criteria beserta risiko jumlah query |

Satu berkas source. Nol controller disentuh, nol DTO lain, nol model, nol konfigurasi EF.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif dan kompatibel mundur.** Satu kolom bertambah pada butir daftar pantau; nol kolom lama diganti nama, dihapus, atau berubah arti. Nol endpoint baru, nol hak akses baru. Urutan, penyaringan, dan jumlah butir yang dikembalikan tidak berubah — dan itu dibuktikan uji, bukan sekadar dinyatakan |
| Database | **Nol migration, nol kolom tabel baru, nol perintah database dijalankan.** Kedua sumber nama sudah ada: kolom snapshot `ProviderDisplayNameSnapshot` pada barisnya, dan relasi `ProviderUser` yang sudah terkonfigurasi. Yang berubah hanya apa yang **dibaca** projeksinya |
| Keamanan/Auth | **Nol perubahan kewenangan.** Endpoint pemakainya memakai `[AccessAction]` dan `[AccessPermission]` yang sama persis. Nol `IsInRole`, nol nama peran, nol `UserType`. **Privasi:** nama penulis adalah keterangan operasional yang memang dibutuhkan supervisor, dan penambahannya **tidak** membuka satu huruf pun isi klinis — dibuktikan uji yang memeriksa seluruh nilai teks butir daftar pantau, bukan kolom yang kebetulan diingat penulis uji |

---

## 4. Dokumentasi endpoint

Nol endpoint dibuat dan nol endpoint disentuh. Endpoint berikut sudah ada dan hanya butir
balasannya yang bertambah satu kolom.

#### Health Services / Clinical Management / Patient Integrated Progress Note

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}/verification-status` | Keadaan verifikasi seluruh catatan satu perawatan beserta daftar pantaunya; setiap butir kini menyebut nama penulisnya | `PatientIntegratedProgressNote : Read` |

**Kolom yang bertambah pada setiap butir `watchList`:**

| Kolom | Jenis | Arti |
| --- | --- | --- |
| `providerName` | teks, boleh kosong | Nama penulis catatan pada saat catatan itu ditulis. Kosong berarti penulisnya tidak dapat dikenali sama sekali — **bukan** nomor pengguna, dan **bukan** nama tebakan |

---

## 5. Verifikasi

Seluruh angka berasal dari eksekusi nyata pada 8 September 2026, pada rangkaian yang sama dengan
`BE-RWI-066`.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Kompilasi berhasil, **0 error `CS`** | `PASS` untuk kompilasi; `EXISTING / ENVIRONMENT ISSUE` untuk penyalinan | Dua error `MSB3027`/`MSB3021`: berkas keluaran pada `bin\Debug\net9.0` terkunci proses aplikasi backend yang sedang dijalankan pengguna (PID 8476). Bukan kesalahan kode, dan proses pengguna tidak dimatikan |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` — keluaran dialihkan ke luar repository | `Failed: 8, Passed: 462, Total: 470` | `EXISTING / ENVIRONMENT ISSUE` | Kedelapan kegagalannya `NursingAssessmentAccessContractTests.SourceBaru_TidakMemakaiHardcodeRole`, gagal mencari `QuilvianSystemBackend.sln` dari `AppContext.BaseDirectory` karena keluarannya berada di luar repository. Kedelapan berkas yang dipindainya milik slice `keperawatan`; nol di antaranya disentuh task ini |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` — keluaran di dalam repository | **`Failed: 0, Passed: 470, Skipped: 0, Total: 470`**, durasi 5 menit 34 detik | `PASS` | Baris ringkasan `Passed!` dari runner. **8 di antaranya uji baru `BE-RWI-067`** |
| Uji `AC 1` — setiap butir membawa namanya | Butir daftar pantau menyebut nama penulis, dan namanya tidak kosong | `PASS` | `DaftarPantau_SetiapButirMembawaNamaPenulisnya` |
| Uji `AC 2` — akun berganti nama tidak menulis ulang riwayat | Snapshot `Ns. Sari`, akun berubah menjadi `Ns. Sari Wijaya`; daftar pantau **tetap** `Ns. Sari` | `PASS` | `AkunBergantiNamaSetelahMenulis_DaftarPantauTetapMenyebutNamaLama` |
| Uji `AC 2` — snapshot kosong jatuh ke relasi pengguna | Snapshot berisi spasi; namanya terbaca dari akun | `PASS` | `SnapshotKosong_NamanyaJatuhKeRelasiPengguna` |
| Uji `AC 3` — penulis tak dikenali menghasilkan kolom kosong | Nama `null`, dan nomor pengguna **tidak** dipakai sebagai penggantinya | `PASS` | `PenulisTidakDikenali_NamanyaKosongDanBukanNomorPengguna` |
| Uji `AC 4` — nol isi klinis bocor | Kedelapan kolom isi catatan diisi teks samaran `RAHASIAKLINIS-JANGAN-BOCOR`; **seluruh** properti bertipe teks pada butir daftar pantau diperiksa lewat refleksi, dan tidak satu pun memuatnya | `PASS` | `ButirDaftarPantau_TidakMemuatSatuPunIsiKlinis` |
| Uji `AC 5` — urutan, penyaringan, dan jumlah tidak berubah | Empat catatan dibuat: satu lewat batas, satu menunggu, satu terverifikasi, satu tidak diwajibkan. Daftar pantau berisi **tepat dua** yang benar, berurutan menurut waktu catatan, dan rekapitulasinya tidak bergeser | `PASS` | `UrutanPenyaringanDanJumlahButir_TidakBerubah` |
| Uji `AC 6` — kebijakan belum aktif tetap `NotRequired` | Penanda `isVerificationPolicyEmpty` benar, hitungan tidak-diwajibkan tetap 1, daftar pantau kosong | `PASS` | `KebijakanBelumAktif_TetapNotRequiredDenganPenandanya` |
| Uji risiko jumlah query | Lima catatan dengan lima penulis berbeda dibaca sekaligus; **tepat 1 perintah SQL** dijalankan, dan kelima nama terbaca benar | `PASS` | `DaftarPantauBanyakCatatan_SatuQuerySajaUntukSeluruhNamanya` |
| Pemeriksaan rahasia pada berkas yang berubah | Nol password, token, connection string, key, maupun nilai konfigurasi sensitif | `PASS` | Tinjauan diff |

**Uji manual:** `NOT APPLICABLE` — task ini menambah satu kolom pada balasan service yang sudah
ada, dan seluruh perilakunya dapat dibuktikan uji otomatis. Menjalankan aplikasi backend
memerlukan wewenang runtime terpisah dan tidak diminta task ini.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `dotnet ef migrations add` | Tidak dibutuhkan dan dilarang — nol kolom tabel bertambah |
| Perintah database apa pun | Nol perintah dijalankan terhadap basis data mana pun |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres` | Tidak diminta task ini; perilaku yang diubah tidak bergantung pada perbedaan PostgreSQL dan SQLite |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Setiap butir daftar pantau memuat nama penulis catatan | **Terpenuhi** | Kolom `ProviderName` pada `CpptVerificationWatchItem`, diisi dari projeksi query yang sama. Uji `DaftarPantau_SetiapButirMembawaNamaPenulisnya` |
| 2. Namanya diambil **berjenjang, snapshot lebih dulu**, baru relasi pengguna bila snapshot-nya kosong. Akun yang berganti nama tidak boleh mengubah nama penulis pada catatan lama | **Terpenuhi** | Helper `NamaPenulis` menetapkan urutannya. Uji `AkunBergantiNamaSetelahMenulis_DaftarPantauTetapMenyebutNamaLama` menjalankan contoh persis dari kartu task — Ns. Sari menjadi Ns. Sari Wijaya, dan daftar pantau tetap menyebut Ns. Sari. Uji `SnapshotKosong_NamanyaJatuhKeRelasiPengguna` menutup jenjang keduanya |
| 3. Penulis yang tidak dapat dikenali sama sekali menghasilkan kolom nama **kosong**, bukan nomor pengguna mentah dan bukan nama tebakan | **Terpenuhi** | `NamaPenulis` mengembalikan `null` untuk teks kosong maupun spasi, dan nomor pengguna tidak pernah dipakai sebagai penggantinya. Uji `PenulisTidakDikenali_NamanyaKosongDanBukanNomorPengguna` |
| 4. Butir daftar pantau **tidak memuat satu pun isi klinis** — dan itu **dibuktikan test** | **Terpenuhi** | Uji `ButirDaftarPantau_TidakMemuatSatuPunIsiKlinis` mengisi kedelapan kolom isi catatan dengan teks samaran, lalu memeriksa **seluruh** properti bertipe teks pada butirnya lewat refleksi. Uji ini akan ikut gagal bila kelak ada orang menambahkan kolom teks yang membocorkan isi |
| 5. Urutan, penyaringan, dan jumlah butir yang dikembalikan **tidak berubah** dibanding sebelum task ini | **Terpenuhi** | Projeksi hanya bertambah dua kolom bacaan; klausa `Where`, `OrderBy`, dan penyaringan status tidak disentuh. Uji `UrutanPenyaringanDanJumlahButir_TidakBerubah` memeriksa isi, urutan, penanda terlambat, dan keempat angka rekapitulasinya |
| 6. Kebijakan verifikasi yang tidak aktif tetap menghasilkan status `NotRequired` seperti sebelumnya, bukan daftar kosong | **Terpenuhi** | Penanda `IsVerificationPolicyEmpty` dan hitungannya tidak disentuh. Uji `KebijakanBelumAktif_TetapNotRequiredDenganPenandanya` |

### Butir Definition of Done

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terpetakan ke source yang benar-benar ada | Terpenuhi |
| `dotnet build` dijalankan dan hasilnya dicatat apa adanya | Terpenuhi — 0 error `CS`; dua error penyalinan berkas dicatat apa adanya beserta penyebabnya |
| `dotnet test` dijalankan dan hasilnya dicatat apa adanya | Terpenuhi — `Failed: 0, Passed: 470, Total: 470`. Run pertama yang gagal 8 **tidak dihapus** dari laporan ini |
| Test unit baru menutup keenam kriteria | Terpenuhi — 8 uji baru |
| Test akun berganti nama membuktikan nama lama bertahan | Terpenuhi |
| Test snapshot kosong jatuh ke relasi pengguna | Terpenuhi |
| Nol isi klinis bocor ke daftar pantau, **dibuktikan test** memakai teks samaran | Terpenuhi — dan pembuktiannya memeriksa seluruh properti teks, bukan kolom yang dipilih |
| Laporan tracked ada di `../task/report/backend/BE-RWI-067.md` | Terpenuhi — berkas ini |
| Register bagian 4.1 dan `requirement-traceability.md` diperbarui | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan peringatan `CS1573`/`CS1574`/`CS1587` pada berkas yang **tidak** disentuh task ini. Peringatan itu sudah ada sebelumnya dan tidak diperbaiki tanpa wewenang |
| Masalah yang diketahui | `NONE` pada scope task ini |
| **Risiko tersisa — bentuk daftar lintas episode belum diputuskan** | Task ini **tidak** memutuskannya dan sengaja tidak melebar ke sana. Keadaan verifikasi tetap tersedia **per episode**, sehingga `FE-RWI-050` masih menyusun daftar lintas pasien dari beberapa pembacaan. Keputusan bentuk agregasinya milik pemilik `02-module-map.md` bersama `ClinicalManagement`, dan tetap terbuka |
| Risiko tersisa — daftar pantau masih selalu kosong | Selama `RWI-RULE-021` belum disahkan, tidak satu pun catatan diberi batas waktu, sehingga daftar pantau selalu kosong dan kolom nama baru ini belum terlihat pada data sungguhan. Perilakunya sudah benar dan sudah diuji; yang belum ada adalah kebijakannya. Pemilik: Clinical Governance |
| **Delta kontrak yang dicatat** | `contracts/api-contract.md` baris 112 masih menandai `GET /episodes/{episodeId}/verification-status` sebagai **"Rencana (belum tersedia)"**, padahal sudah tersedia di source sejak `BE-RWI-053`. Selisih dokumen, bukan cacat source, dan **tidak disunting dari sini** karena kontrak bukan target tulis task ini |
| Perubahan sampingan | Penulisan berkas sempat menambahkan penanda urutan byte (BOM) pada berkas yang aslinya tidak memilikinya, termasuk `CpptVerificationService.cs`. **Dipulihkan** dan diperiksa ulang terhadap `HEAD`. Direktori keluaran build sementara `artifacts/tb/` **sudah dihapus** |
| Interupsi | `NONE` pada scope task ini |
| Status Git | `M` pada 1 berkas source milik task ini, `??` pada 1 berkas uji baru. **Nol tindakan Git dijalankan agent** |
| Langkah berikutnya | Menjalankan ulang builder frontend pada `FE-RWI-050` supaya kolom Penulis dapat diisi, lalu menutup kriteria 5-nya dengan ketetapan urutan daftar pada `02-module-map.md` |
