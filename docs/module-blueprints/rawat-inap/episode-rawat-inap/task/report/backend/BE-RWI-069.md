# Laporan Perubahan Backend — `BE-RWI-069`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-069` |
| Judul | Tempat tidur yang ditolak ikut menyebutkan alasannya |
| Slice | `S13` — Petugas tahu kenapa sebuah tempat tidur ditolak; `EPIC RI-36`, gelombang `MVP-1` |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` bagian 4, kartu `BE-RWI-069` |
| Trace | `RWI-RULE-012` bagian A dan B; `RWI-DEC-064` s.d. `RWI-DEC-066`; bukti runtime pemilik 9 September 2026 |
| Contract version | API **`0.7.0`** — **disetujui 10 September 2026** oleh Muhammad Hamzah lewat instruksi eksplisit mengerjakan task ini. Sebelumnya `draft` |
| Dependency | `BE-RWI-013` ✅, `BE-RWI-015` ✅, ditambah gerbang approval API `0.7.0` yang **dicabut hari ini** |
| Klasifikasi | `MEDIUM` — dua DTO, satu perubahan alur di satu service, enam kriteria dengan dua provider uji berbeda |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**` dan kedua project uji |
| Model | claude-opus-5 |
| Commit backend saat dikerjakan | `4c3a458dac3fd10dcac770adb938bfa2b4e0a7dd` |
| Tanggal | 10 September 2026 |
| Status | ✅ **SELESAI.** Keenam acceptance criteria terbukti. **Nol migration** |

---

## 1. Masalah yang diperbaiki

Petugas admisi membuka layar pemilihan tempat tidur, melihat sebuah tempat tidur tidak dapat
dipilih, dan tidak diberi tahu apa pun kecuali kalimat **"Tidak lolos kelayakan"**. Kalimat itu
tidak menyebut aturan mana yang menolak, sehingga petugas tidak tahu apa yang harus dilakukan
berikutnya. Pemilik menyatakan pada 9 September 2026 bahwa developer sendiri pun tidak dapat
membacanya.

Yang membuatnya menyakitkan: **server sebenarnya sudah tahu jawabannya.** Pemeriksaan kelayakan
berjalan lengkap untuk setiap tempat tidur kandidat, menghasilkan daftar aturan yang menolak
beserta kalimatnya, lalu **membuang daftar itu** dan hanya mengembalikan tempat tidur yang lolos.
Layar akhirnya terpaksa menebak dari kolom seadanya.

**Contoh nyata dari bukti runtime 9 September 2026.** Kamar Melati 3 sedang dihuni pasien
perempuan. Petugas mencarikan tempat tidur untuk pasien laki-laki. Seluruh tempat tidur di kamar
itu hilang dari daftar tanpa keterangan, begitu pula tempat tidur isolasi di kamar sebelah.
Petugas tidak dapat membedakan "kamarnya sedang dihuni lawan jenis" dari "tempat tidur ini khusus
pasien isolasi", padahal tindakan lanjutannya berbeda: yang pertama berarti cari kamar lain, yang
kedua berarti tempat tidur itu memang bukan untuk pasien ini.

Sesudah perubahan, kedua keadaan itu terkirim dengan kalimatnya sendiri:

- `Kamar Melati 3 sedang dihuni pasien perempuan, sehingga tidak dapat menerima pasien laki-laki.`
- `Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi.`

---

## 2. Proses bisnis

**Tujuan.** Petugas yang melihat sebuah tempat tidur tidak dapat dipilih langsung membaca aturan
mana yang menolaknya, dengan kalimat yang sama persis seperti yang akan muncul bila penempatan
tetap dipaksakan.

**Pelaku.** Petugas admisi, dan siapa pun yang memegang `InpatientBedOccupancy : Read`.

**Pemicu.** Layar pemilihan tempat tidur dibuka untuk satu episode tertentu.

**Langkah yang berurutan.**

1. Layar memanggil `GET /bed-occupancies/available-beds` dengan `episodeId` pasiennya, dan sejak
   sekarang boleh menambahkan `includeIneligible=true`.
2. Server menyusun daftar tempat tidur kandidat memakai penyaring yang sudah ada — unit layanan,
   kamar, kelas, kata kunci, penanda isolasi, penanda boks bayi.
3. Untuk **setiap** kandidat, server menjalankan pemeriksaan Kelayakan Penempatan yang sudah ada.
   Pemeriksaan ini tidak diubah sama sekali.
4. Kandidat yang lolos masuk daftar `items`, persis seperti sebelumnya.
5. Kandidat yang **ditolak** kini disimpan ke daftar `ineligible` beserta **seluruh** aturan yang
   menolaknya — bukan hanya aturan pertama.
6. Layar menampilkan alasannya di kartu tempat tidur yang bersangkutan.

**Aturan yang berlaku.** Kesembilan aturan Kelayakan Penempatan berlaku apa adanya. Nol aturan
baru ditambahkan, dan nol aturan lama diubah.

**Status yang dihasilkan.** Tidak ada. Task ini murni operasi baca; nol baris tersimpan, nol
status episode maupun tempat tidur berubah.

**Jalur tidak normal pertama — permintaan tanpa episode.** Bila `includeIneligible=true` dikirim
tanpa `episodeId`, daftar `ineligible` dijawab **kosong**. Alasannya bukan kemalasan: tanpa
episode, aturan jenis kelamin pasien dan aturan kebutuhan isolasi tidak dapat dinilai sama sekali,
sehingga alasan yang terkirim akan menyesatkan petugas. Alasan sebagian lebih buruk daripada tidak
ada alasan.

**Jalur tidak normal kedua — pemanggil lama.** Pemanggil yang tidak mengirim `includeIneligible`
menerima jawaban yang sama persis seperti sebelum perubahan, dengan `ineligible` berupa array
kosong. Tidak ada satu pun pemanggil lama yang perlu disesuaikan.

**Hasil akhir.** Petugas membaca sebab, bukan kalimat buntu, dan sebab itu datang dari server —
bukan dari tebakan layar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` kartu `BE-RWI-069` | Scope, keenam acceptance criteria, dan batas yang mengikat |
| `contracts/api-contract.md` `0.7.0` | Bentuk `includeIneligible` dan `ineligible` yang dijanjikan |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Tempat `evaluation.Failures` selama ini dibuang |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientSharedDtos.cs` | Bentuk `PlacementEligibilityFailureResponse` yang sudah dipakai jawaban 422 |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs` | Cara `AvailableBedQuery` diikat dari query string, dan atribut hak aksesnya |
| `rules/backend/role-access-rules.md` | Memastikan nol perubahan hak akses memang benar |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs` | `AvailableBedQuery` menerima `IncludeIneligible` bawaan `false`; `AvailableBedPagedResult` membawa `Ineligible`; DTO baru `IneligibleBedResponse` berisi identitas tempat tidur beserta `Failures` |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | `SearchAvailableBedsAsync` menyimpan `evaluation.Failures` alih-alih membuangnya, dan hanya ketika episodenya benar-benar ada |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/InPatientManagement/InpIneligibleBedReasonTests.cs` | **Berkas baru.** Lima test untuk kriteria 1 sampai 5 |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/HealthServices/InpatientIneligibleBedQueryCountTests.cs` | **Berkas baru.** Satu test untuk kriteria 6, memakai provider relasional supaya perintah SQL benar-benar terhitung |
| `contracts/api-contract.md` | Status `0.7.0` naik `draft` → `approved`; baris `ineligible` naik **Rencana** → **Tersedia** |

**Yang sengaja tidak disentuh**, sesuai batas pada kartu task: `EvaluatePlacementEligibilityAsync`,
`GetBedBoardAsync`, jalur penempatan, dan jalur pemesanan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif seluruhnya.** Satu query baru `includeIneligible` bawaan mati, dan satu field baru `ineligible` yang terkirim sebagai array kosong bagi pemanggil yang tidak memintanya. Nol endpoint baru, nol bentuk lama berubah |
| Database | **`NOT APPLICABLE` — nol migration.** Nol tabel, nol kolom, dan nol index disentuh. Nol perintah dikirim ke database mana pun |
| Keamanan/Auth | **Nol perubahan.** Hak aksesnya tetap `InpatientBedOccupancy : Read`. `[AccessController(ControllerName = "InpatientBedOccupancy")]`, `[AccessAction("Read", …)]`, dan `[AccessPermission("InpatientBedOccupancy", "Read")]` sudah sejajar sebelum perubahan dan tidak disentuh. Nol hardcode peran ditambahkan |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/available-beds` | Mencari tempat tidur yang dapat ditempati. Dengan `includeIneligible=true` dan `episodeId`, jawabannya sekaligus menyebutkan tempat tidur yang **ditolak** beserta seluruh aturan yang menolaknya | `InpatientBedOccupancy : Read` |

**Parameter yang bertambah.**

| Nama | Tipe | Bawaan | Arti |
| --- | --- | --- | --- |
| `includeIneligible` | `bool` | `false` | Bila benar dan `episodeId` terisi, daftar `ineligible` ikut dikirim |

**Field yang bertambah pada `AvailableBedPagedResult`.**

| Nama | Tipe | Arti |
| --- | --- | --- |
| `ineligible` | daftar `IneligibleBedResponse` | Tempat tidur yang ditolak. Kosong bila tidak diminta, atau bila diminta tanpa `episodeId` |

Setiap `IneligibleBedResponse` memuat `bedId`, `bedCode`, `bedName`, `bedNumber`, `roomId`,
`roomCode`, `roomName`, dan `failures[]`. Bentuk `failures[]` **tidak baru** — persis bentuk yang
sudah dipakai jawaban 422 pada `POST /placements`, memuat `ruleNumber`, `code`, `message`, dan
`statusCode`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` seluruh solution | **`0 error CS`**, 197 warning | `PASS` | Kompilasi C# bersih. Dua error `MSB3021`/`MSB3027` muncul dan **bukan** kesalahan kode — lihat catatan di bawah |
| Kriteria 1 — tanpa `includeIneligible`, jawaban tidak berubah dan `ineligible` kosong | Lulus | `PASS` | `InpIneligibleBedReasonTests.Kriteria1_TanpaIncludeIneligible_JawabanTidakBerubahDanDaftarKosong` |
| Kriteria 2 — setiap bed ditolak membawa **seluruh** aturannya | Lulus. Tiga bed ditolak; bed sekamar menghasilkan `ROOM_GENDER_MIXED` aturan 6, bed isolasi menghasilkan `ISOLATION_BED_RESERVED` aturan 8, bed yang sedang dihuni menghasilkan **lebih dari satu** aturan sekaligus | `PASS` | `…Kriteria2_DenganIncludeIneligible_SetiapBedDitolakMembawaSeluruhAturannya` |
| Kriteria 3 — kalimat identik dengan penolakan `POST /placements` | Lulus. Kalimat dibandingkan **utuh**, bukan hanya kode aturannya, untuk dua tempat tidur berbeda | `PASS` | `…Kriteria3_KalimatAlasanIdentikDenganPenolakanPenempatan` |
| Kriteria 4 — bed yang lolos tidak pernah muncul di kedua daftar | Lulus. Irisan kedua daftar kosong, dan jumlahnya menutup keempat bed kandidat tanpa sisa maupun rangkap | `PASS` | `…Kriteria4_BedYangLolosTidakPernahMunculDiKeduaDaftar` |
| Kriteria 5 — `includeIneligible=true` tanpa `episodeId` menjawab kosong | Lulus | `PASS` | `…Kriteria5_TanpaEpisodeId_DaftarPenolakanKosongBukanAlasanSebagian` |
| Kriteria 6 — jumlah query tidak bertambah | Lulus. Perintah SQL dihitung sungguhan lewat `RelationalEventId.CommandExecuted`; hitungan dengan dan tanpa `includeIneligible` **sama persis** | `PASS` | `InpatientIneligibleBedQueryCountTests.Kriteria6_MemintaAlasanPenolakanTidakMenambahQuery` |
| `dotnet test` project uji SQLite, disaring pada test kriteria 6 | `Failed: 0, Passed: 1` | `PASS` | Keluaran perintah |
| `dotnet test` kedua kelas uji task ini beserta `InpatientSettingServiceTests` | `Failed: 0, Passed: 21` | `PASS` | Keluaran perintah |
| `dotnet test` project uji InMemory **penuh** | `Failed: 17, Passed: 1023, Total: 1040` | `EXISTING / ENVIRONMENT ISSUE` | Ketujuh belas kegagalan **seluruhnya milik `BillingManagement`** dan **sudah ada sebelum task ini** — lihat pembuktian garis dasar di bawah |
| Garis dasar pada `HEAD` tanpa perubahan apa pun | `Failed: 17, Passed: 1012, Total: 1029` | `PASS` | Perubahan di-`stash` lebih dulu, suite dijalankan, lalu dikembalikan. **Angka gagalnya sama persis: 17.** Selisih 11 test adalah test baru sesi ini, dan seluruhnya lulus |

Uji manual: `NOT FEASIBLE` — layar pemilihan tempat tidur adalah pekerjaan `FE-RWI-057` pada
repository frontend, dan task itu belum dikerjakan.

**Tidak dijalankan:** uji terhadap PostgreSQL sungguhan. Alasannya tegas: task ini **nol
migration** dan **nol perubahan schema**, sehingga tidak ada apa pun yang perlu dibuktikan maju
atau mundur. Kriteria 6 yang biasanya menuntut database relasional justru **sudah** dibuktikan
terhadap provider relasional lewat SQLite.

### 5.1 Dua error build yang bukan kesalahan kode

`dotnet build` melaporkan `2 Error(s)`, keduanya `MSB3021` dan `MSB3027`:

```text
Could not copy "obj\Debug\net9.0\QuilvianSystemBackend.dll" to "bin\Debug\net9.0\QuilvianSystemBackend.dll".
The file is locked by: "QuilvianSystemBackend (9180)"
```

Aplikasi backend sedang **berjalan** di komputer ini sejak 09:42 dan memegang berkas di `bin/`.
Kompilasi C# sendiri **berhasil sepenuhnya**: `0 error CS`. Proses milik pemilik tidak dihentikan;
sebagai gantinya seluruh build dan test dijalankan dengan `-p:BaseOutputPath` ke folder sementara,
sehingga tidak ada satu pun berkas milik pemilik yang ditimpa dan test tetap berjalan terhadap
assembly yang **baru**, bukan yang basi.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tanpa `includeIneligible`, jawaban sama persis dengan sebelumnya dan `ineligible` array kosong | **Terpenuhi** | Test kriteria 1 |
| 2. Dengan `includeIneligible=true` dan `episodeId`, setiap bed yang tidak lolos muncul beserta **seluruh** aturan yang menolaknya | **Terpenuhi** | Test kriteria 2, termasuk satu bed yang ditolak lebih dari satu aturan |
| 3. Kalimat `failures[].message` **identik** dengan kalimat 422 `POST /placements` untuk bed dan episode yang sama | **Terpenuhi** | Test kriteria 3, membandingkan kalimat utuh |
| 4. Bed yang lolos tidak pernah muncul di kedua daftar sekaligus | **Terpenuhi** | Test kriteria 4 |
| 5. `includeIneligible=true` tanpa `episodeId` menjawab `ineligible` kosong | **Terpenuhi** | Test kriteria 5 |
| 6. Jumlah query ke database tidak bertambah | **Terpenuhi** | Test kriteria 6, menghitung perintah SQL sungguhan pada provider relasional |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Keenam kriteria lulus | ✅ Terpenuhi |
| API contract `0.7.0` disetujui dan baris `ineligible` naik dari **Rencana** menjadi **Tersedia** | ✅ Terpenuhi — `contracts/api-contract.md` diperbarui pada hari yang sama |
| `dotnet build` 0 error | ✅ Terpenuhi untuk kompilasi C# — `0 error CS`. Dua error penyalinan berkas berasal dari aplikasi yang sedang berjalan, bukan dari kode |
| Suite `InPatientManagement` lulus | ✅ Terpenuhi — nol kegagalan pada seluruh test `InPatientManagement` |
| Laporan task ditulis di `task/report/backend/` | ✅ Terpenuhi — berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Daftar `ineligible` **tidak dipaging**. Ia memuat seluruh tempat tidur kandidat yang ditolak sesudah penyaring unit, kamar, kelas, dan kata kunci diterapkan. Bentuk ini mengikuti contoh jawaban pada kontrak `0.7.0`. Pemanggilan tanpa penyaring kamar pada rumah sakit berkapasitas besar karena itu dapat mengembalikan daftar yang panjang; layar sebaiknya tetap menyaring per kamar atau per unit |
| Masalah yang diketahui | Tujuh belas test `BillingManagement` gagal pada project uji InMemory. Kegagalan itu **sudah ada pada `HEAD`** dan tidak ada hubungannya dengan task ini — dibuktikan dengan menjalankan suite yang sama pada kode tanpa perubahan. Perbaikannya milik pemilik `BillingManagement` |
| Risiko tersisa | Godaan yang disebut kartu task — menambahkan penyaringan baru di dalam `SearchAvailableBedsAsync` supaya daftar `ineligible` terlihat rapi — **tidak** dilakukan, dan kriteria 3 kini menjaganya secara otomatis. Bila kelak seseorang menyusun kalimat alasan tersendiri di jalur pencarian, test kriteria 3 langsung gagal |
| Perubahan sampingan | `NONE`. Perubahan sempat di-`stash` dan dikembalikan utuh untuk mengukur garis dasar test; `git stash pop` dikonfirmasi memulihkan keempat belas berkas |
| Interupsi | `NONE` |
| Status Git | Berkas berubah dan bertambah; nol operasi `add`, `commit`, `push`, `merge`, maupun `rebase` dijalankan |
| Langkah berikutnya | `FE-RWI-057` pada roadmap frontend dapat memakai field `ineligible` untuk mengganti kalimat "Tidak lolos kelayakan" pada kartu tempat tidur. Gerbang task itu diputuskan terpisah dan **tidak** ikut terbuka oleh approval kontrak hari ini |
