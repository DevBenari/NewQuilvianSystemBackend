# Laporan Perubahan Backend — `BE-RWI-013`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> **Task ini kini ✅ SELESAI.** Seluruh acceptance criteria-nya punya test yang lulus, dan
> ketiga butir DoD-nya hijau. Tandanya pada roadmap sudah dinaikkan 🟡 → ✅.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-013` |
| Judul | Kamar tidak pernah menjadi campur laki-laki dan perempuan |
| Slice | S2 — Pasien punya lokasi, dan penempatan yang tidak layak ditolak |
| Trace | `RWI-DEC-064`, `RWI-DEC-066`; `RWI-RULE-012` bagian B; `EPIC RI-34`, `FR-RI-154` s.d. `FR-RI-157`; validation matrix bagian 4; test matrix 2A.1 dan 2A.2; `RWI-AC-128` s.d. `RWI-AC-133`; `UAT-29`, `UAT-30` |
| Contract version | API `0.4.0` — bentuk tidak berubah; tiga kode 422 baru sudah tercantum sejak `0.3.0` |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-011` — dikerjakan pada sesi yang sama |
| Status | ✅ **SELESAI** — seluruh acceptance criteria dan DoD terbukti; build dan test dijalankan 26 Agustus 2026 dan hijau (255/255) |

---

## 1. Apa yang dibangun

Aturan 4, 5, dan 6 Kelayakan Penempatan beserta **dua pengecualian boks bayi**, ditambah
penyaringan `GET /available-beds` yang memakai daftar aturan yang sama persis.

| No | Aturan | Kode |
| ---: | --- | ---: |
| 4 | Penanda tempat tidur menerima jenis kelamin pasien | 422 |
| 5 | Jenis kelamin belum tercatat: tempat tidur harus menerima keduanya **dan** kamar belum berpenghuni | 422 |
| 6 | Kamar belum dihuni pasien berjenis kelamin berbeda | 422 |

| Pengecualian | Isinya |
| --- | --- |
| Menempatkan **ke** boks bayi | Aturan 4, 5, dan 6 dilewati |
| Penghuni yang **berada di** boks bayi | Tidak dihitung saat aturan 5 dan 6 memeriksa penghuni kamar |

---

## 2. Dua hal yang menentukan bentuk implementasinya

### 2.1 Aturan 6 diperiksa dari penghuni, bukan dari penanda kamar

`MstRoom.IsForMale` dan `IsForFemale` bernilai `true` secara bawaan untuk **setiap** kamar,
sehingga keduanya tidak dapat membedakan kamar yang boleh campur dari kamar yang tidak.
`RWI-DEC-066` menolak penambahan kolom "boleh campur" secara tegas, dan penolakan itu terkunci
pada `blueprint-manifest.md` bagian 8 butir 7.

Karena itu aturan 6 membaca **penempatan yang sedang aktif di kamar tersebut**, lalu
membandingkan jenis kelamin penghuninya dengan jenis kelamin pasien yang akan masuk.

> **Konsekuensi yang perlu diketahui.** Kamar yang berpenghuni satu pasien laki-laki tertutup
> bagi seluruh pasien perempuan sampai laki-laki itu keluar — dan begitu ia keluar, kamarnya
> terbuka lagi bagi siapa pun. Tidak ada penanda apa pun yang perlu diubah petugas.

### 2.2 "Belum tercatat" mencakup dua nilai, bukan satu

Enum `Gender` punya empat nilai: `Unknown`, `Male`, `Female`, dan `NotDisclosed`. Aturan
privasi memperlakukan `Unknown` **dan** `NotDisclosed` sama-sama sebagai belum tercatat.

> **Kenapa `NotDisclosed` tidak boleh dianggap sudah terisi.** Nilai itu berarti pasien menolak
> menyebutkan jenis kelaminnya. Untuk pendaftaran ia adalah jawaban yang sah; untuk aturan
> privasi kamar ia sama saja dengan kosong, karena sistem tetap tidak dapat membuktikan
> kamarnya tidak menjadi campur.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Ditambah | Aturan 4, 5, dan 6 di dalam `EvaluatePlacementEligibilityAsync`; `LoadRoomOccupantsAsync`; `NormalizeGender`; `GenderLabel`; `BedGenderMessage` |

**Tidak ada kolom baru pada modul mana pun.** `MstBed.IsForMale`, `IsForFemale`,
`IsForNewborn`, dan `RoomId` semuanya sudah ada pada source hari ini.

### 3.1 Kalimat penolakannya

Ditulis persis seperti validation matrix bagian 3:

| Aturan | Kalimatnya |
| ---: | --- |
| 4 | "Tempat tidur ini hanya untuk pasien laki-laki." / "…perempuan." |
| 5 | "Jenis kelamin pasien belum tercatat. Pilih tempat tidur yang menerima laki-laki dan perempuan, di kamar yang belum ada penghuninya." |
| 6 | "Kamar Melati 3 sedang dihuni pasien perempuan, sehingga tidak dapat menerima pasien laki-laki." |

Nama kamar disisipkan ke dalam kalimat aturan 6, sesuai acceptance criteria 2.

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-API-001`, `QBE-ENT-003` |
| Pengecualian QBE | Tidak ada |

**`QBE-ENT-003`** relevan justru karena yang **tidak** dilakukan: tidak ada kolom persisten
baru yang ditambahkan untuk menyederhanakan pemeriksaan ini.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Penghuni yang jenis kelaminnya tidak tercatat diperlakukan sebagai penghalang

Bila seorang penghuni kamar ternyata tidak punya jenis kelamin tercatat, aturan 6 menolak
pasien berikutnya. Alasannya: sistem tidak dapat membuktikan kamarnya tidak menjadi campur.

Keadaan itu seharusnya tidak muncul — aturan 5 sudah memastikan pasien tanpa jenis kelamin
hanya masuk ke kamar kosong — tetapi ia mungkin terjadi bila data pasien diubah **setelah**
penempatan. Perilaku ini konservatif dan disengaja; bila Product/Domain menghendaki yang lain,
sebutkan.

### 5.2 Kamar berisi satu tempat tidur

Acceptance criteria 7 menuntut kamar berisi satu tempat tidur tidak pernah tersentuh aturan
pencampuran. Itu terpenuhi **dengan sendirinya**: aturan 6 membaca penghuni kamar selain
episode yang sedang diproses, dan pada kamar satu tempat tidur tidak pernah ada penghuni lain.

Tidak ada cabang khusus yang ditulis untuk kasus ini — menambahkannya justru akan menciptakan
jalur kedua yang dapat berselisih dengan jalur utama.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| `UAT-29` dan `UAT-30` terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpPlacementGenderRuleTests.cs` — 7 test.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Pasien perempuan ke tempat tidur hanya laki-laki ditolak 422 | `Kriteria1_PasienPerempuanKeTempatTidurHanyaLakiLakiDitolak422` | ✅ **Lulus** 26 Agu 2026 |
| 2. Kamar berpenghuni jenis kelamin berbeda ditolak 422, pesannya menyebut nama kamar | `Kriteria2_KamarYangSudahDihuniJenisKelaminBerbedaDitolak422DanPesannyaMenyebutNamaKamar` | ✅ **Lulus** 26 Agu 2026 |
| 3. Pasien berikutnya berjenis kelamin sama diterima | `Kriteria3_PasienBerikutnyaBerjenisKelaminSamaDiterima` | ✅ **Lulus** 26 Agu 2026 |
| 4. Jenis kelamin belum tercatat: gagal salah satu saja ditolak | `Kriteria4_JenisKelaminBelumTercatatHanyaBolehKeTempatTidurNetralDiKamarKosong` | ✅ **Lulus** 26 Agu 2026 |
| 5. Bayi laki-laki ke boks bayi di kamar ibunya berhasil | `Kriteria5Dan6_BoksBayiDikecualikanDariKeduaSisiPemeriksaan` | ✅ **Lulus** 26 Agu 2026 |
| 6. Penghuni boks bayi tidak dihitung saat memeriksa pencampuran | Test yang sama | ✅ **Lulus** 26 Agu 2026 |
| 7. Kamar satu tempat tidur tidak tersentuh aturan pencampuran | `Kriteria7_KamarBerisiSatuTempatTidurTidakPernahTersentuhAturanPencampuran` | ✅ **Lulus** 26 Agu 2026 |

Kriteria 5 dan 6 sengaja **berpasangan dalam satu test**, sesuai permintaan roadmap, supaya
sifat dua arah pengecualian boks bayi terbukti sekaligus.

Satu test tambahan — `HasilPencarianDanHasilPenolakanSelaluSama` — menjawab verifikasi roadmap
bahwa penyaring dan penolak tidak boleh memberi jawaban berbeda: tempat tidur yang tidak
ditawarkan `GET /available-beds` memang benar-benar ditolak `POST /placements`, dan yang
ditawarkan memang benar-benar diterima.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | Tiga kode 422 baru, sudah tercantum sejak `0.3.0` |
| Database | Tidak ada perubahan schema, tidak ada kolom baru |
| Perilaku existing | `GET /available-beds` menyaring memakai aturan ini bila `episodeId` dikirim |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Ketujuh kriteria belum terbukti | Bagian 6.1 |
| **Penanda `IsForMale`/`IsForFemale` pada data sungguhan belum terbukti benar** | Aturan bekerja benar terhadap data yang salah, dan hasilnya tampak seperti cacat program | Menuntaskan `RWI-DEC-063` |
| Penghuni tanpa jenis kelamin diperlakukan sebagai penghalang | Kamar tertutup lebih ketat daripada yang dimaksud | Konfirmasi Product/Domain — bagian 5.1 |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Tiga aturan dan dua pengecualian aktif | ✅ Ada di dalam kode |
| Ketujuh kriteria lulus | ✅ **Lulus** — seluruh test-nya dijalankan 26 Agustus 2026 dan hijau (255/255) |
| Validation matrix dan kenyataan pesan cocok kata demi kata | ✅ Kalimatnya disalin apa adanya; test memeriksa isinya, bukan hanya kodenya |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Jalankan `UAT-29` dan `UAT-30` terhadap aplikasi berjalan setelah data master siap.
