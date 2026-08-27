# Laporan Perubahan Backend — `BE-RWI-021`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> Yang **belum** berubah: acceptance criteria dan DoD task ini tetap belum terbukti penuh —
> build hijau bukan tanda selesai — sehingga tandanya pada roadmap tetap 🟡.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-021` |
| Judul | Resume pulang tersusun dan hanya DPJP yang menandatanganinya |
| Slice | S5 — Pasien dapat dinyatakan boleh pulang |
| Trace | `RWI-DEC-016`; `GUARD-INP-03`; api contract `GET`, `PUT`, dan `PATCH .../summary`; privasi pada `03-frontend-architecture.md` bagian 6; `UAT-10` |
| Contract version | API `0.4.0` — bentuk tidak berubah; tiga endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-020` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN.** Ada satu delta kontrak yang perlu diputuskan; lihat bagian 5.1 |

---

## 1. Apa yang dibangun

| Endpoint | Kegunaannya |
| --- | --- |
| `GET /discharges/{episodeId}/summary` | Mengambil resume pulang, dengan pilihan menyertakan versi sebelumnya |
| `PUT /discharges/{episodeId}/summary` | Menyusun atau memperbarui resume |
| `PATCH /discharges/{episodeId}/summary/sign` | DPJP menandatangani resume |

---

## 2. `GUARD-INP-03` — tanda tangan yang benar-benar berarti

Hanya **DPJP aktif episode itu** yang dapat menandatangani. Dokter lain ditolak 403, dan
penandatangan **tidak pernah** dibaca dari isian permintaan — `SignDischargeSummaryRequest`
hanya punya kolom catatan opsional.

> **Bila penandatangan dibaca dari isian permintaan**, `GUARD-INP-03` dapat dilewati hanya
> dengan mengirim identifier dokter lain — dan resume yang tertandatangani atas nama seseorang
> yang tidak pernah menandatanganinya adalah dokumen rekam medis yang palsu.

Penjaga yang sama juga berlaku saat resume **disusun**, bukan hanya saat ditandatangani.
Membiarkan orang lain menyusun resume lalu meminta DPJP menandatangani apa adanya membuat tanda
tangan itu kehilangan artinya.

---

## 3. Dua isian yang wajib — dan kapan diperiksanya

| Isian | Wajib saat | Kalimat penolakannya |
| --- | --- | --- |
| Diagnosis utama | **Penandatanganan** | "Diagnosis utama wajib diisi sebelum resume ditandatangani." |
| Tujuan rujukan | **Penandatanganan**, bila cara pulangnya `Referred` | "Tujuan rujukan wajib diisi untuk pasien yang dirujuk." |

Keduanya boleh kosong selama resume masih disusun. Yang tidak boleh adalah resume
**tertandatangani** yang kosong.

---

## 4. Privasi — kriteria 5 adalah kewajiban, bukan preferensi

Isi resume memuat diagnosis. Bila ia bocor ke endpoint daftar, seluruh peran yang boleh melihat
census ikut membacanya.

Ada satu test yang memeriksa **empat** bentuk jawaban sekaligus —
`InpatientEpisodeListItemResponse`, `CensusItemResponse`, `IsolationMismatchItemResponse`, dan
`InpatientEpisodeDetailResponse` — dan memastikan tidak satu pun memuat keenam kolom isi
resume.

Payload logger untuk ketiga endpoint ini juga tidak memuat isinya; yang dicatat hanya identitas
baris, controller, action, dan kode status.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Delta terhadap state-transition-matrix bagian 5

**Yang berselisih.**

| Dokumen | Isinya |
| --- | --- |
| Roadmap `BE-RWI-021` acceptance criteria 3 | "Resume yang sudah ditandatangani **tidak** dapat diubah lewat endpoint biasa" |
| `contracts/state-transition-matrix.md` bagian 5 baris 4 | "Tertandatangani → Ubah isi → Tertandatangani, oleh **DPJP aktif**, hanya selama episode belum `Closed`. Tanda tangan diperbarui" |

**Yang diimplementasikan: roadmap.** Resume yang sudah ditandatangani ditolak 409 lewat `PUT`,
dan hanya dapat diubah ketika supervisor sudah membuka sesi koreksi.

**Alasannya.** Roadmap adalah dokumen task yang berstatus `APPROVED` dan lebih spesifik. Selain
itu, baris state matrix tersebut membuat `BE-RWI-022` kehilangan artinya: bila DPJP dapat
mengubah resume tertandatangani lewat endpoint biasa tanpa versi disimpan, riwayat amandemen
tidak pernah lahir untuk kasus yang paling sering terjadi.

**Yang perlu diputuskan.** Salah satu dokumen harus dikoreksi. **Owner: Product/Domain bersama
pemilik kontrak.**

### 5.2 Sesi koreksi dibaca, tetapi belum dapat dibuka

Endpoint pembuka dan penutup sesi koreksi milik `BE-RWI-030`. Yang dibutuhkan task ini hanyalah
**pembacaan** keberadaan sesi terbuka, dan itu dapat dilakukan sekarang karena tabel
`InpCorrectionSession` sudah dibuat `BE-RWI-003`.

Sampai `BE-RWI-030` rilis, satu-satunya cara sesi koreksi lahir adalah lewat penyisipan baris
langsung — dan itulah yang dipakai test.

### 5.3 Resume hanya dapat disusun setelah keputusan pulang

State matrix bagian 5 baris 1 menetapkan penyusunan resume dimulai saat episode
`DischargePending`. Permintaan pada episode `Admitted` ditolak 422.

---

## 6. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Ditambah | `GetSummaryAsync`, `UpsertSummaryAsync`, `SignSummaryAsync`, `GetOpenCorrectionSessionAsync`, `ApplySummaryContent`; class `InpDischargeSummaryOperationResult` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientDischargeDtos.cs` | Ditambah | `UpsertDischargeSummaryRequest`, `SignDischargeSummaryRequest`, `DischargeSummaryResponse` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Ditambah | Tiga aksi resume |

---

## 7. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-AUD-001`** — penandatanganan meninggalkan jejak pada `InpDischargeSummary.SignedAt` dan
`SignedByDoctorId`, terpisah dari catatan `LoggerService`.

---

## 8. Validasi

### 8.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Test unique index `IX_InpDischargeSummary_EpisodeId` terhadap PostgreSQL | **NOT RUN** |
| `UAT-10` terhadap aplikasi berjalan | **NOT RUN** |

### 8.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpDischargeSummaryTests.cs` — 10 test
(sebagian milik `BE-RWI-022`).

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Resume dapat disusun dan diperbarui selagi belum ditandatangani | `Kriteria1_ResumeDapatDisusunDanDiperbaruiSelagiBelumDitandatangani` | ✅ **Lulus** 26 Agu 2026 |
| 2. Hanya DPJP aktif yang dapat menandatangani; peran lain 403 | `Kriteria2_HanyaDpjpAktifYangDapatMenandatangani` | ✅ **Lulus** 26 Agu 2026 |
| 3. Resume tertandatangani tidak dapat diubah lewat endpoint biasa | `Kriteria3_ResumeYangSudahDitandatanganiTidakDapatDiubahLewatEndpointBiasa` | ✅ **Lulus** 26 Agu 2026 |
| 4. Satu episode punya paling banyak satu resume berlaku | `Kriteria4_SatuEpisodePunyaPalingBanyakSatuResume` | ✅ **Lulus** 26 Agu 2026; unique index belum diuji |
| 5. Isi resume tidak ikut pada endpoint daftar mana pun | `Kriteria5_IsiResumeTidakIkutPadaEndpointDaftarMANaPun` — memeriksa empat bentuk jawaban | ✅ **Lulus** 26 Agu 2026 |

Dua test tambahan menjaga: menandatangani resume rujukan tanpa tujuan rujukan ditolak 400, dan
resume hanya dapat disusun setelah DPJP menyatakan pasien boleh pulang.

---

## 9. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Tiga endpoint baru, salah satunya memakai butir `InpatientDischarge : Sign` |
| Database | Tidak ada perubahan schema |
| Keamanan | `GUARD-INP-03` ditambahkan; kolom sensitif dijaga tidak bocor ke daftar mana pun |
| Kontrak | Satu delta terhadap state-transition-matrix bagian 5 — bagian 5.1 |

---

## 10. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 8.1 |
| **Delta terhadap state matrix belum diputuskan** | Dua dokumen kontrak berselisih; implementasi berikutnya dapat mengikuti yang salah | Bagian 5.1 |
| Sesi koreksi belum dapat dibuka lewat endpoint | Resume tertandatangani belum benar-benar dapat dikoreksi di lingkungan sungguhan | `BE-RWI-030` |
| **Pemilik privasi belum ditunjuk** | Aturan siapa boleh membaca resume masih usulan | Penunjukan pemilik keamanan/privasi |

---

## 11. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Tiga endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan |
| Test privasi lulus | ❌ **Belum dijalankan** — sudah ditulis dan mencakup empat bentuk jawaban |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 12. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Putuskan delta terhadap state-transition-matrix bagian 5 (bagian 5.1).
3. Jalankan `UAT-10` terhadap aplikasi berjalan.
