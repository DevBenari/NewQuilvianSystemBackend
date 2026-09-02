# Laporan Perubahan Backend — `BE-RWI-029`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> **Task ini kini ✅ SELESAI.** Seluruh butir DoD-nya hijau: test lulus (255/255) dan
> endpointnya terbukti berjalan lewat Swagger pada aplikasi tersambung PostgreSQL.
> Tandanya pada roadmap sudah dinaikkan 🟡 → ✅.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-029` |
| Judul | Empat daftar pantau dan satu laporan selisih tersedia |
| Slice | S7 — Riwayat, daftar pantau, dan koreksi |
| Trace | `RWI-DEC-032`, `RWI-DEC-039`; `RWI-RULE-023`, `RWI-RULE-027`; api contract bagian Monitoring (5 endpoint); `RWI-FE-002`; `RWI-AC-063`; `UAT-21` |
| Contract version | API `0.4.0` — bentuk tidak berubah; empat endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-027` — dikerjakan pada sesi yang sama |
| Status | ✅ **SELESAI** — seluruh acceptance criteria dan DoD terbukti; build+test hijau dan endpoint terbukti berjalan 26 Agustus 2026 |

---

## 1. Apa yang dibangun

| Endpoint | Kegunaannya |
| --- | --- |
| `GET /monitoring/pending-closures` | Episode yang sudah boleh pulang tetapi belum ditutup melewati ambang |
| `GET /monitoring/closures-without-financial-clearance` | Episode yang ditutup menembus gerbang keuangan |
| `GET /monitoring/unassigned-nurse-episodes` | Episode aktif tanpa perawat penanggung jawab |
| `GET /monitoring/bed-drift` | Tempat tidur yang salinan statusnya tidak cocok dengan catatan penempatan |

Daftar pantau kelima — penempatan tidak sesuai kebutuhan isolasi — sudah dibuka `BE-RWI-015`.
Dengan keempat ini, grup Monitoring lengkap sesuai api contract.

---

## 2. Kriteria 5 adalah satu-satunya pengawas atas satu-satunya arah tulis lintas modul

`RWI-DEC-039` menurunkan `MstBed.BedStatus` menjadi **salinan**; sumber kebenarannya adalah
`InpBedPlacement` dan `InpBedReservation`. Modul Rawat Inap adalah satu-satunya yang menulisnya,
dan `INT-INP-03` adalah satu-satunya arah tulis lintas modul yang disetujui.

Salinan dapat menyimpang karena banyak hal: perubahan langsung di database, kegagalan yang
tidak tertangani, atau modul lain yang menulisnya tanpa sepengetahuan modul ini.

> **Laporan ini hanya berguna bila ada yang membacanya.** Bila tidak pernah dibuka siapa pun,
> salinan akan menyimpang diam-diam sampai seorang pasien ditempatkan di tempat tidur yang
> sudah ada orangnya. **Ini soal proses, bukan kode** — dan roadmap secara khusus meminta
> laporan task menyebutnya.
>
> **Yang perlu ditetapkan:** siapa yang membaca laporan ini, dan seberapa sering.
> Owner: Backend/API bersama Product/Domain.

### 2.1 Yang tidak dihitung sebagai selisih

Keempat keadaan yang merupakan wewenang admin — `Cleaning`, `Maintenance`, `Blocked`,
`Inactive` — **tidak** dihitung. Modul Rawat Inap memang tidak pernah menuliskannya, sehingga
menyalahkannya di sini akan membanjiri laporan dengan baris yang tidak dapat ditindaklanjuti
siapa pun.

Ada satu test khusus yang menjaga batas itu.

---

## 3. Ambang penutupan tertunda milik admin

`MstInpatientSetting.PendingClosureThresholdHours` dibaca ulang setiap pembacaan. Angka yang
diubah admin berlaku pada pembacaan berikutnya tanpa aplikasi dinyalakan ulang — pola yang sama
dengan batas pemesanan tempat tidur dan kedaluwarsa episode `Draft`.

Setiap baris membawa `ThresholdHours` yang berlaku saat dibaca, supaya layar dapat menjelaskan
kenapa sebuah episode muncul di sana.

### 3.1 Kolom `IsBedStillHeld`

Sejak `BE-RWI-027`, episode `DischargePending` dapat berada dalam dua keadaan yang sangat
berbeda: masih memegang tempat tidur, atau kepergiannya sudah dicatat.

Keduanya sama-sama menggantung, tetapi hanya yang pertama yang **mendesak** — ia menahan tempat
tidur. Kolom `IsBedStillHeld` membedakannya, supaya kepala ruangan dapat mendahulukan yang
benar.

---

## 4. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Ditambah | `GetPendingClosuresAsync`, `GetOverrideClosuresAsync`, `GetUnassignedNurseEpisodesPagedAsync`, `GetBedDriftAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientMonitoringDtos.cs` | Ditambah | `InpatientMonitoringQuery` dan empat pasang bentuk jawaban beserta hasil bertingkatnya |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs` | Ditambah | Empat aksi baca |

`GetUnassignedNurseEpisodesAsync` sudah ditulis `BE-RWI-018` untuk membuktikan acceptance
criteria 3 task tersebut; di sini ia dibungkus menjadi bentuk bertingkat dan diberi endpoint.

---

## 5. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001` |
| Pengecualian QBE | Tidak ada |

Seluruh endpoint hanya membaca, memakai `AsNoTracking` dengan projection langsung ke DTO, dan
mengikuti konvensi project — **tidak satu pun dicatat logger**.

---

## 6. Keputusan implementasi yang perlu ditinjau

### 6.1 Kolom pelaku penutupan pada daftar penembusan gerbang

`OverrideClosureItemResponse.ClosedByUserId` dibaca dari `InpEpisode.UpdateBy`, yaitu pengubah
terakhir barisnya. Untuk episode yang ditutup lewat jalan keluar supervisor, nilainya memang
supervisor tersebut.

Namun bila episode itu **disentuh lagi** sesudahnya — misalnya lewat sesi koreksi — nilainya
ikut berubah. Sumber yang benar-benar tahan lama adalah `InpStatusHistory` dengan
`ActionType = CloseEpisodeWithOverride`.

**Perlu diputuskan:** apakah daftar ini membaca dari riwayat status alih-alih dari kolom audit.
Perubahannya kecil, tetapi bentuknya berbeda dan sebaiknya diputuskan sebelum layar dibangun.
Owner: Backend/API bersama Product/Domain.

### 6.2 Laporan selisih dihitung di memori

`GetBedDriftAsync` membaca seluruh tempat tidur yang tidak terhapus, lalu membandingkannya di
memori dengan catatan penempatan dan pemesanan. Perbandingannya tidak dapat diterjemahkan
menjadi satu query tunggal tanpa membuat query yang sulit dibaca.

Pada skala rumah sakit — ratusan sampai ribuan tempat tidur — beban itu wajar untuk laporan
yang dibaca berkala, bukan setiap detik. Bila kelak jumlahnya jauh lebih besar, penyaring unit
layanan sudah tersedia untuk membatasi cakupannya.

---

## 7. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| `UAT-21` terhadap aplikasi berjalan | **NOT RUN** |

### 7.1 Test yang ditulis

Di dalam `InpStatusHistoryAndMonitoringTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Daftar penutupan tertunda menampilkan episode yang melewati ambang | `Kriteria1Dan2_DaftarPenutupanTertundaMemakaiAmbangYangDapatDiubahAdmin` | ✅ **Lulus** 26 Agu 2026 |
| 2. Ambangnya dapat diubah admin dan berlaku pada pembacaan berikutnya | Test yang sama | ✅ **Lulus** 26 Agu 2026 |
| 3. Daftar penutupan menembus gerbang menampilkan episode bertanda | `Kriteria4Dan5_EpisodeDitandaiDanMunculPadaDaftarPantauPenutupanMenembusGerbang` pada `InpEpisodeClosureTests` | ✅ **Lulus** 26 Agu 2026 |
| 4. Daftar episode tanpa perawat | `Kriteria4_DaftarEpisodeTanpaPerawatBertingkatDanMengikutiPenugasan` | ✅ **Lulus** 26 Agu 2026 |
| 5. Laporan selisih menemukan salinan status yang menyimpang | `Kriteria5_LaporanSelisihMenemukanSalinanStatusYangMenyimpang` | ✅ **Lulus** 26 Agu 2026 |
| 6. Daftar kosong mengembalikan daftar kosong, bukan galat | `Kriteria6_KeempatDaftarPantauYangKosongMengembalikanDaftarKosongBukanGalat` | ✅ **Lulus** 26 Agu 2026 |

Kriteria 5 dikerjakan persis seperti diminta roadmap: selisihnya dibuat **secara sengaja** lewat
perubahan langsung pada database uji — salinan status dua tempat tidur diputar terbalik — lalu
laporan dibuktikan menemukan keduanya beserta arah selisihnya.

Satu test tambahan menjaga bahwa keempat keadaan wewenang admin tidak dihitung sebagai selisih,
dan satu lagi menjaga penyaringan menurut unit layanan.

---

## 8. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Empat endpoint baru; grup Monitoring lengkap |
| Database | Tidak ada perubahan schema |
| Proses | Laporan selisih memerlukan penanggung jawab yang membacanya berkala — bagian 2 |

---

## 9. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Keenam kriteria belum terbukti | Bagian 7 |
| **Laporan selisih tidak dibaca siapa pun** | `MstBed.BedStatus` menyimpang diam-diam sampai seorang pasien ditempatkan di tempat tidur yang sudah ada orangnya | Penetapan penanggung jawab dan frekuensi — bagian 2 |
| Kolom pelaku penutupan menembus gerbang dapat berubah | Daftar pantau menyebut orang yang bukan pengambil keputusannya | Keputusan bagian 6.1 |
| Daftar pantau ketiga `RWI-RULE-023` belum ada | Kepatuhan pengkajian awal dan verifikasi CPPT tidak terpantau | `DEC-INP-001`, di luar scope revisi ini |

---

## 10. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Empat endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Keenam kriteria lulus | ✅ **Lulus** — dijalankan 26 Agustus 2026, hijau (255/255) |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |

---

## 11. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. **Tetapkan siapa yang membaca laporan selisih dan seberapa sering** — tanpa itu, laporan ini
   tidak menutup risiko apa pun.
3. Putuskan sumber kolom pelaku pada daftar penembusan gerbang (bagian 6.1).
