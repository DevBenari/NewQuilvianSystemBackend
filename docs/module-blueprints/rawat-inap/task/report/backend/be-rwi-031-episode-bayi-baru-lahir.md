# Laporan Perubahan Backend — `BE-RWI-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-031` |
| Judul | Bayi baru lahir punya episode sendiri di boks kamar ibunya |
| Slice | S8 — Bayi baru lahir |
| Trace | `RWI-DEC-020`, `RWI-DEC-056`; `RWI-RULE-014`; `FR-RI-146`, `FR-RI-147`, `FR-RI-152`; `RWI-AC-122`, `RWI-AC-123`; `UAT-22`, `UAT-28` |
| Contract version | API `0.4.0` — **tidak ada endpoint baru**; satu kolom aditif pada dua bentuk permintaan yang sudah ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-029` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Apa yang dibangun

| Bagian | Isinya |
| --- | --- |
| Kolom `MotherEpisodeId` pada permintaan admisi | Diisi saat admisi dibuka, dan dapat dibetulkan selagi episode masih `Draft` |
| Empat aturan validasi | Validation matrix bagian 5B |
| Census menampilkan dua baris | Ibu dan bayinya, masing-masing dengan hari rawat sendiri |
| `GetNewbornOccupantsAsync` | Menjawab **bayi siapa** yang ada di boks kamar tertentu |

Boks bayi sudah diperlakukan sebagai tempat tidur sejak `BE-RWI-013`; pengecualiannya dari
aturan jenis kelamin sudah aktif, dan kolom `MotherEpisodeId` sudah dibuat `BE-RWI-003`. Yang
kurang hanyalah pengisian, pembacaan, dan pembuktiannya.

---

## 2. Kriteria 3 adalah yang paling mudah dikerjakan terbalik

> **Menutup episode ibu TIDAK menutup episode bayinya, dan TIDAK melepas boks bayinya.**

Bayi sering pulang pada hari yang berbeda dari ibunya — ibu pulang hari ketiga, bayi menunggu
hasil skrining sampai hari kelima. Episode bayi yang tertutup paksa akan **menghapus hari rawat
bayi dari tagihan**, dan kesalahan itu baru ketahuan saat keluarga menerima kuitansi.

Implementasi ini tidak melakukan apa pun terhadap episode bayi saat episode ibu ditutup —
penutupan bekerja per episode, dan tidak ada satu baris pun yang menelusuri
`MotherEpisodeId` ke arah sebaliknya. Ada test yang membuktikannya: setelah ibu ditutup, episode
bayi masih `Admitted`, boksnya masih `Occupied`, penempatannya masih terbuka, dan bayinya masih
muncul pada census.

---

## 3. Empat aturan `MotherEpisodeId`

Mengikuti validation matrix bagian 5B apa adanya:

| Aturan | Kode | Kalimatnya |
| --- | ---: | --- |
| Boleh kosong | 200 | Bukan penolakan — sebagian besar episode memang bukan bayi rawat gabung |
| Tidak boleh menunjuk diri sendiri | 400 | "Episode tidak dapat menunjuk dirinya sendiri sebagai episode ibu." |
| Tidak boleh milik pasien yang sama | 422 | "Episode ibu harus milik pasien yang berbeda." |
| Episode ibu ada dan belum selesai | 422 | "Episode ibu tidak ditemukan atau sudah selesai." |

> **Aturan ketiga yang paling mudah terlewat.** Tanpa ia, seorang pasien dapat tercatat sebagai
> ibu dari dirinya sendiri lewat dua episode berbeda — dan pertanyaan "bayi siapa yang ada di
> boks kamar ini" mulai menjawab hal yang mustahil.

Pemeriksaannya dijalankan **sebelum apa pun ditulis**, sehingga rujukan yang keliru tidak
pernah tersimpan walaupun sesaat.

---

## 4. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Corrections.cs` | Ditambah | `ValidateMotherEpisodeAsync`, `GetNewbornOccupantsAsync` |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Diubah | `OpenAdmissionAsync` dan `UpdateAdmissionAsync` memeriksa dan menyimpan `MotherEpisodeId`; `GetDetailResponseAsync` menambah tiga kolom ibu |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Diubah | Census menambah `MotherEpisodeId`, `MotherEpisodeNumber`, `MotherPatientName`, `IsNewbornBed` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` | Ditambah | Kolom `MotherEpisodeId` pada dua bentuk permintaan; tiga kolom ibu pada detail |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientCensusDtos.cs` | Ditambah | Empat kolom pada `CensusItemResponse` |

### 4.1 Dampak kontrak: aditif, tanpa endpoint baru

DoD roadmap menyatakan "api contract tidak berubah" — dan tidak ada endpoint yang bertambah,
berubah bentuknya, maupun berubah perilakunya bagi pemanggil yang tidak mengirim
`MotherEpisodeId`.

Yang bertambah adalah **satu kolom opsional** pada `OpenAdmissionRequest` dan
`UpdateAdmissionRequest`, ditambah kolom baca pada detail dan census. Seluruhnya aditif dan
kompatibel mundur, tetapi tetap perlu tercatat sebagai delta terhadap dokumen kontrak.

---

## 5. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENT-003` |
| Pengecualian QBE | Tidak ada |

**`QBE-ENT-003`** — tidak ada kolom persisten baru. `MotherEpisodeId` dan
`MstBed.IsForNewborn` keduanya sudah ada.

---

## 6. Keputusan implementasi yang perlu ditinjau

### 6.1 Bayi mendapat kunjungan sendiri lewat jalur datang langsung

Admisi bayi tanpa `EncounterId` membuat kunjungan bertipe rawat inap sendiri — jalur yang sama
dengan pasien datang langsung (`BE-RWI-007`). Itu memenuhi `RWI-DEC-020`: bayi punya episode
**dan kunjungan** sendiri.

Konsekuensinya, nomor kunjungan bayi memakai bentuk yang sama dengan yang dicatat laporan
`BE-RWI-007` bagian 5.2 — berbeda dari nomor kunjungan pendaftaran, dan masih menunggu
keputusan pemilik `RegistrationManagement`.

### 6.2 `MotherEpisodeId` tidak dapat diubah setelah episode `Admitted`

Ia hanya dapat diisi saat admisi dibuka atau dibetulkan selagi `Draft`, karena
`UpdateAdmissionAsync` menolak episode yang bukan `Draft`.

**Perlu diputuskan:** apakah hubungan ibu dan bayi perlu dapat dibetulkan setelah bayi
ditempatkan. Kasusnya nyata — bayi kembar yang tertukar rujukan episodenya — dan saat ini
satu-satunya jalan adalah sesi koreksi, yang belum punya jalur untuk kolom ini. Owner:
Product/Domain.

### 6.3 Census tidak menyaring boks bayi secara khusus

Bayi muncul sebagai baris census biasa, dengan penanda `IsNewbornBed` dan rujukan ke episode
ibunya. Layar yang ingin mengelompokkan ibu dan bayinya dapat melakukannya dari kedua kolom
itu tanpa endpoint tambahan.

---

## 7. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **NOT RUN** |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | **NOT RUN** |
| `UAT-22` dan `UAT-28` terhadap aplikasi berjalan | **NOT RUN** |

### 7.1 Test yang ditulis

Di dalam `InpCorrectionAndNewbornTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Bayi mendapat episode dan kunjungan sendiri di boks bertanda `IsForNewborn` | `Kriteria1Dan2_BayiPunyaEpisodeSendiriDanCensusMenampilkanDuaBaris` | Ditulis, **belum dijalankan** |
| 2. Census menampilkan dua baris: ibu dan bayinya | Test yang sama | Ditulis, **belum dijalankan** |
| 3. Menutup episode ibu tidak menutup episode bayi dan tidak melepas boksnya | `Kriteria3_MenutupEpisodeIbuTidakMenutupEpisodeBayiDanTidakMelepasBoksnya` | Ditulis, **belum dijalankan** |
| 4. Sistem dapat menjawab bayi siapa yang ada di boks kamar tertentu | `Kriteria4_SistemDapatMenjawabBayiSiapaYangAdaDiBoksKamarTertentu` | Ditulis, **belum dijalankan** |
| 5. `MotherEpisodeId` boleh kosong dan tidak boleh milik pasien yang sama | `Kriteria5_RujukanEpisodeIbuBolehKosongTetapiTidakBolehMilikPasienYangSama`, `Kriteria5_EpisodeTidakDapatMenunjukDirinyaSendiriSebagaiEpisodeIbu` | Ditulis, **belum dijalankan** |

Test kriteria 1 juga memeriksa bahwa `EncounterId` bayi **berbeda** dari milik ibunya — bukti
bahwa bayi tidak menumpang kunjungan ibunya.

Test kriteria 5 dikerjakan persis seperti diminta roadmap: mencoba menunjuk episode pasien yang
sama, dan membuktikan ditolak.

---

## 8. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif tanpa endpoint baru.** Satu kolom opsional pada dua permintaan; empat kolom baca pada census dan tiga pada detail |
| Database | Tidak ada perubahan schema |
| Perilaku existing | Admisi tanpa `MotherEpisodeId` berperilaku sama persis seperti sebelumnya |

---

## 9. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 7 |
| Rujukan ibu tidak dapat dibetulkan setelah bayi ditempatkan | Bayi kembar yang tertukar rujukannya tidak dapat diperbaiki | Keputusan bagian 6.2 |
| Bentuk nomor kunjungan bayi | Sama seperti catatan `BE-RWI-007` bagian 5.2 | Pemilik `RegistrationManagement` |
| Data master boks bayi belum terbukti terisi | Bayi tidak dapat ditempatkan sama sekali karena tidak ada tempat tidur bertanda `IsForNewborn` | `RWI-DEC-063` |

---

## 10. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Kelima kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan |
| Census terbukti menampilkan dua baris | ❌ **Belum dijalankan** — test-nya sudah ditulis |
| Api contract tidak berubah | ✅ Tidak ada endpoint baru; penambahan kolomnya aditif dan dicatat pada bagian 4.1 |

---

## 11. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Putuskan apakah rujukan episode ibu perlu dapat dibetulkan setelah bayi ditempatkan
   (bagian 6.2).
3. Jalankan `UAT-22` dan `UAT-28` setelah data master boks bayi terbukti terisi.
