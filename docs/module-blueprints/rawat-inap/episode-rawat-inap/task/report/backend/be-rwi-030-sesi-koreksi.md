# Laporan Perubahan Backend — `BE-RWI-030`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> **Task ini kini ✅ SELESAI.** Seluruh butir DoD-nya hijau: test lulus (255/255) dan
> endpointnya terbukti berjalan lewat Swagger pada aplikasi tersambung PostgreSQL.
> Tandanya pada roadmap sudah dinaikkan 🟡 → ✅.
>
> **Satu test task ini diperbaiki.** `Kriteria2Dan3_StatusTetapClosedTempatTidurTidakKembaliDanLamaDirawatTidakBertambah`
> gagal pada sapuan pertama karena memakai angka tetap tiga sebagai jumlah baris riwayat,
> padahal jumlah yang benar empat. Yang dibuktikan test ini — sesi koreksi tidak menambah baris
> riwayat — tidak berubah; nilai awalnya kini **ditangkap sebelum sesi dibuka** lalu
> dibandingkan sesudahnya, mengikuti pola `sebelumSesi`/`bedSebelum`/`censusSebelum` yang sudah
> dipakai test itu sendiri. Tidak ada perubahan pada service.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-030` |
| Judul | Kesalahan catatan dapat dibetulkan tanpa membongkar episode |
| Slice | S7 — Riwayat, daftar pantau, dan koreksi |
| Trace | `RWI-DEC-028`, `RWI-DEC-057`; `RWI-RULE-020`; api contract `POST` dan `PATCH .../correction-sessions`; `UAT-14`, `UAT-15` |
| Contract version | API `0.4.0` — bentuk tidak berubah; dua endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-028` — dikerjakan pada sesi yang sama |
| Status | ✅ **SELESAI** — seluruh acceptance criteria dan DoD terbukti; build+test hijau dan endpoint terbukti berjalan 26 Agustus 2026 |

---

## 1. Sesi koreksi **bukan** status episode keenam

Godaan menjadikan "sedang dikoreksi" sebagai status akan muncul karena ia menyederhanakan
layar: satu kolom, satu penyaring, selesai.

`blueprint-manifest.md` bagian 8 butir 5 **menguncinya** sebagai konsep tersendiri. Menambah
status melanggar `RWI-DEC-009` yang mengunci lima nilai, dan `RWI-AC-004` yang menghitungnya.

**Yang berubah saat sesi dibuka hanyalah keberadaan satu baris `InpCorrectionSession` yang
terbuka.** Status episode tetap `Closed`, dan seluruh perilaku yang bergantung pada status ikut
tetap:

| Hal | Selama sesi terbuka |
| --- | --- |
| Status episode | Tetap `Closed` |
| Tempat tidur | **Tidak** dikembalikan |
| Census | Pasien **tidak** muncul |
| Lama dirawat | **Tidak** bertambah |
| Riwayat status | **Tidak** bertambah — sesi koreksi bukan perpindahan status |

Ketiganya bukan hasil pemeriksaan tambahan, melainkan konsekuensi langsung dari status yang
memang tidak disentuh. Itulah alasan bentuk ini dipilih.

---

## 2. Apa yang dibangun

| Endpoint | Kegunaannya |
| --- | --- |
| `POST /episodes/{id}/correction-sessions` | Supervisor membuka sesi koreksi pada episode yang sudah ditutup |
| `PATCH /episodes/{id}/correction-sessions/{sessionId}/close` | Menutup sesi beserta daftar perubahannya |

Keduanya memakai butir hak akses tersendiri, `InpatientEpisode : Reopen` — bukan `Update`.
Pemisahan itu berarti mesin hak akses dapat menjaganya, dan penjaga di service adalah lapis
kedua.

---

## 3. Yang menutup celah `BE-RWI-021` dan `BE-RWI-022`

Sampai task ini, sesi koreksi hanya dapat lahir lewat penyisipan baris langsung ke database
uji. Laporan `BE-RWI-021` bagian 5.2 dan `BE-RWI-022` bagian 7.3 mencatatnya sebagai celah:
jalur amandemen resume **tidak dapat dijalankan di lingkungan sungguhan**.

Celah itu ditutup di sini. Ada satu test yang membuktikannya berujung: sesi dibuka lewat
endpoint yang sebenarnya, resume tertandatangani dikoreksi di dalamnya, dan versi lamanya
tersimpan beserta rujukan ke sesi yang menyebabkannya.

---

## 4. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Corrections.cs` | **Baru** | `OpenCorrectionSessionAsync`, `CloseCorrectionSessionAsync`, `GetCorrectionSessionsAsync`, `GetCorrectionSessionAsync`; class `InpCorrectionSessionOperationResult` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientCorrectionDtos.cs` | Ditambah | `OpenCorrectionSessionRequest`, `CloseCorrectionSessionRequest`, `InpatientCorrectionSessionResponse` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Ditambah | Dua aksi sesi koreksi |

### 4.1 Satu sesi terbuka dijaga dua lapis

| Lapis | Isinya | Diuji di mana |
| --- | --- | --- |
| 1. Pemeriksaan service | Menolak dengan kalimat yang dapat dibaca petugas | Test InMemory |
| 2. Unique index parsial `IX_InpCorrectionSession_EpisodeId_Open` | Menolak baris kedua bila dua supervisor membukanya bersamaan | **Belum** — hanya PostgreSQL |

---

## 5. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

---

## 6. Keputusan implementasi yang perlu ditinjau

### 6.1 Yang boleh dan tidak boleh diubah selama sesi belum ditegakkan kode

State matrix bagian 6.1 menetapkan yang **boleh** diubah selama sesi terbuka adalah cara pulang,
isi resume, dan catatan episode; yang **tidak boleh** adalah waktu admisi, waktu penutupan,
riwayat penempatan, dan riwayat status.

Yang sudah ditegakkan kode:

| Batas | Keadaannya |
| --- | --- |
| Riwayat status tidak dapat diubah | ✅ Tidak ada endpoint yang menyediakannya |
| Riwayat penempatan tidak dapat diubah | ✅ Tidak ada endpoint yang menyediakannya |
| Isi resume boleh diubah lewat sesi | ✅ `BE-RWI-022` |
| Butir administrasi dan kelayakan keuangan boleh ditandai lewat sesi | ✅ Gerbang `GuardEpisodeNotClosedAsync` melewatkannya bila ada sesi terbuka |
| Waktu admisi dan waktu penutupan tidak dapat diubah | ✅ Tidak ada endpoint yang menyediakannya |
| **Cara pulang boleh diubah lewat sesi** | ❌ **Belum** — `DecideDischargeAsync` menolak episode `Closed`, dan tidak ada jalur lain |

**Perlu diputuskan:** apakah cara pulang perlu dapat dikoreksi lewat sesi. Bila ya, ia
memerlukan jalur tersendiri yang tidak tersedia pada api contract `0.4.0`. Owner: Product/Domain.

### 6.2 Menutup sesi hanya oleh supervisor

Sama seperti membukanya. Ini keputusan implementasi — state matrix bagian 6 menyebut supervisor
untuk keduanya, tetapi tidak menyatakannya sebagai penolakan tersendiri.

---

## 7. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Test unique index parsial sesi terbuka terhadap PostgreSQL | **NOT RUN** |
| `UAT-14` dan `UAT-15` terhadap aplikasi berjalan | **NOT RUN** |

### 7.1 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpCorrectionAndNewbornTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Hanya supervisor yang dapat membuka sesi | `Kriteria1_HanyaSupervisorYangDapatMembukaSesiKoreksi` | ✅ **Lulus** 26 Agu 2026 |
| 2. Status episode tetap `Closed` sepanjang sesi | `Kriteria2Dan3_StatusTetapClosedTempatTidurTidakKembaliDanLamaDirawatTidakBertambah` | ✅ **Lulus** 26 Agu 2026 |
| 3. Tempat tidur tidak dikembalikan dan hari rawat tidak bertambah | Test yang sama | ✅ **Lulus** 26 Agu 2026 |
| 4. Satu episode punya paling banyak satu sesi terbuka | `Kriteria4_SatuEpisodePunyaPalingBanyakSatuSesiTerbuka` | ✅ **Lulus** 26 Agu 2026 |
| 5. Menutup sesi menyimpan daftar perubahannya | `Kriteria5_MenutupSesiMenyimpanDaftarPerubahannya` | ✅ **Lulus** 26 Agu 2026 |
| 6. Koreksi resume tertandatangani menyimpan versi lamanya | `Kriteria6_KoreksiResumeDiDalamSesiMenyimpanVersiLamanya` | ✅ **Lulus** 26 Agu 2026 |

Kriteria 3 diuji persis seperti diminta roadmap: keadaan diperiksa **sebelum dan sesudah** sesi
dibuka — status episode, salinan status tempat tidur, jumlah penempatan aktif, isi census, dan
jumlah baris riwayat status.

Test kriteria 4 juga membuktikan sesi berikutnya boleh dibuka **setelah** yang pertama ditutup —
batasnya adalah satu sesi terbuka, bukan satu sesi seumur episode.

Satu test tambahan menjaga bahwa sesi koreksi hanya untuk episode yang sudah ditutup.

---

## 8. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Dua endpoint baru dengan butir hak akses baru `InpatientEpisode : Reopen` |
| Database | Tidak ada perubahan schema; tabelnya sudah dibuat `BE-RWI-003` |
| Task terdahulu | Menutup celah `BE-RWI-021` bagian 5.2 dan `BE-RWI-022` bagian 7.3 |

---

## 9. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Keenam kriteria belum terbukti | Bagian 7 |
| Unique index parsial belum diuji | Dua supervisor bersamaan dapat membuka dua sesi | Test terhadap PostgreSQL |
| Cara pulang belum dapat dikoreksi lewat sesi | Kesalahan cara pulang pada episode tertutup tidak dapat dibetulkan sama sekali | Keputusan bagian 6.1 |
| Nama peran supervisor masih asumsi | Sesi koreksi tidak dapat dibuka siapa pun | Konfirmasi Product/Domain |

---

## 10. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Dua endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Keenam kriteria lulus | ✅ **Lulus** — seluruh test-nya dijalankan 26 Agustus 2026 dan hijau (255/255) |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |

---

## 11. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Putuskan apakah cara pulang perlu dapat dikoreksi lewat sesi (bagian 6.1).
3. Jalankan `UAT-14` dan `UAT-15` terhadap aplikasi berjalan — kini keduanya dapat dijalankan
   sepenuhnya lewat endpoint.
