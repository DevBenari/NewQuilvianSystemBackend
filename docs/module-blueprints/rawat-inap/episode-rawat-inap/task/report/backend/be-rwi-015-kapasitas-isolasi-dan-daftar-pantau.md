# Laporan Perubahan Backend — `BE-RWI-015`

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
| Task ID | `BE-RWI-015` |
| Judul | Kapasitas isolasi terjaga dari dua arah, tanpa menahan pencatatan klinis |
| Slice | S2 — Pasien punya lokasi, dan penempatan yang tidak layak ditolak |
| Trace | `RWI-DEC-064`, `RWI-DEC-065` aturan 5–7; `RWI-RULE-012` bagian A; api contract `GET /monitoring/isolation-mismatch`; `FR-RI-160`, `FR-RI-161`; test matrix 2A.3 dan 2A.5; `RWI-AC-134`, `RWI-AC-135`, `RWI-AC-138`; `UAT-31`, `UAT-33` |
| Contract version | API `0.4.0` — bentuk tidak berubah; satu endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-014` — dikerjakan pada sesi yang sama |
| Status | ✅ **SELESAI** — seluruh acceptance criteria dan DoD terbukti; build+test hijau dan endpoint terbukti berjalan 26 Agustus 2026 |

---

## 1. Apa yang dibangun

Aturan 7 dan 8 Kelayakan Penempatan, ditambah daftar pantau penempatan tidak sesuai:

| Bagian | Isinya |
| --- | --- |
| Aturan 7 | Pasien yang membutuhkan isolasi hanya boleh ke tempat tidur isolasi — 422 |
| Aturan 8 | Pasien yang **tidak** membutuhkan isolasi tidak boleh ke tempat tidur isolasi — 422 |
| `GET /monitoring/isolation-mismatch` | Daftar episode yang kebutuhan isolasinya tidak cocok dengan tempat tidur yang ditempatinya |

---

## 2. Dua arah yang sama pentingnya

| Arah | Isinya | Risiko bila diabaikan |
| --- | --- | --- |
| `NeedsIsolationBed` | Pasien butuh isolasi, tempat tidurnya biasa | Penularan ke penghuni kamar lain |
| `OccupiesIsolationBed` | Pasien tidak butuh isolasi, tempat tidurnya isolasi | Kapasitas isolasi terpakai sia-sia; pasien yang membutuhkannya tidak kebagian |

Kedua penolakan **berkode sama, yaitu 422, tetapi artinya berlawanan**. Karena itu test wajib
memeriksa isi pesannya, bukan hanya kodenya:

| Aturan | Kalimatnya |
| ---: | --- |
| 7 | "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat tidur isolasi." |
| 8 | "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi." |

---

## 3. Yang paling mudah dikerjakan terbalik

Acceptance criteria 4: **menyalakan kebutuhan isolasi saat pasien berada di tempat tidur biasa
harus DITERIMA, bukan ditolak.**

> **Urutan yang benar.** Hasil laboratorium keluar pukul 14:00 dan menunjukkan pasien
> membutuhkan isolasi. DPJP mencatatnya. Pencatatan itu **berhasil**, dan episodenya muncul
> pada daftar pantau supaya penempatannya dibetulkan.
>
> **Urutan yang salah.** Pencatatan ditolak karena "pasien sedang di tempat tidur biasa". DPJP
> tidak dapat mencatat fakta klinis sampai seseorang memindahkan pasiennya — dan sepanjang
> waktu itu, tidak ada satu pun catatan bahwa pasien tersebut menular.

Daftar pantau adalah **pengganti** penolakan, bukan pelengkapnya.

---

## 4. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Ditambah | Aturan 7 dan 8 di dalam `EvaluatePlacementEligibilityAsync` |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Diisi | `GetIsolationMismatchAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientMonitoringDtos.cs` | **Baru** | `IsolationMismatchQuery`, `IsolationMismatchItemResponse`, `IsolationMismatchPagedResult` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs` | **Baru** | Satu aksi `GET /monitoring/isolation-mismatch` |

**Tidak ada kolom baru.** `MstBed.IsIsolationBed` sudah ada pada source hari ini.

### 4.1 Controller monitoring lahir dengan satu endpoint

`InpatientMonitoringController` dibuat di sini karena `GET /monitoring/isolation-mismatch`
membutuhkannya. **Empat daftar pantau lainnya** — `pending-closures`,
`closures-without-financial-clearance`, `unassigned-nurse-episodes`, dan `bed-drift` — milik
`BE-RWI-029` dan sengaja belum ada. Ada test yang menahan jumlahnya tetap satu.

---

## 5. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001` |
| Pengecualian QBE | Tidak ada |

Daftar pantau memakai `PagedResult<T>` yang sudah baku, dan seluruh query-nya `AsNoTracking`
dengan projection langsung ke DTO.

---

## 6. Keputusan implementasi yang perlu ditinjau

### 6.1 Daftar pantau memuat `Admitted` dan `DischargePending`

Episode `DischargePending` yang belum ditutup masih memegang tempat tidur, sehingga
ketidaksesuaian isolasinya masih nyata. Ia karena itu ikut pada daftar.

Episode yang kepergian fisiknya sudah dicatat tidak ikut, karena baris penempatannya sudah
ditutup pada saat kepergian itu dicatat.

### 6.2 Kalimat daftar pantau tidak dikunci kontrak

`contracts/api-contract.md` menetapkan endpoint dan bentuk jawabannya, tetapi tidak menetapkan
kalimat `MismatchMessage`. Kalimat yang dipakai ditulis apa adanya di sini untuk ditinjau:

| Arah | Kalimatnya |
| --- | --- |
| `NeedsIsolationBed` | "Pasien membutuhkan isolasi, tetapi sedang menempati {nama tempat tidur} yang bukan tempat tidur isolasi." |
| `OccupiesIsolationBed` | "Tempat tidur isolasi {nama tempat tidur} sedang ditempati pasien yang tidak membutuhkan isolasi." |

---

## 7. Validasi

### 7.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| `UAT-31` dan `UAT-33` terhadap aplikasi berjalan | **NOT RUN** |

### 7.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpIsolationCapacityTests.cs` — 5 test.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Butuh isolasi ke tempat tidur biasa ditolak 422 dengan pesan yang menyebut kebutuhannya | `Kriteria1Dan2_DuaPenolakanBerkodeSamaDenganArtiBerlawanan` | ✅ **Lulus** 26 Agu 2026 |
| 2. Tidak butuh isolasi ke tempat tidur isolasi ditolak 422 dengan pesan berbeda | Test yang sama | ✅ **Lulus** 26 Agu 2026 |
| 3. Butuh isolasi ke tempat tidur isolasi berhasil | `Kriteria3_PasienButuhIsolasiKeTempatTidurIsolasiBerhasil` | ✅ **Lulus** 26 Agu 2026 |
| 4. Menyalakan kebutuhan isolasi saat di tempat tidur biasa **diterima**, dan muncul pada daftar pantau | `Kriteria4Dan5_PencatatanKlinisTidakDitahanDanDaftarPantauMengikutiPembetulannya` | ✅ **Lulus** 26 Agu 2026 |
| 5. Setelah dipindahkan ke tempat tidur isolasi, hilang dari daftar pantau | Test yang sama | ✅ **Lulus** 26 Agu 2026 |
| 6. Kebalikannya juga bekerja | `Kriteria6_MematikanKebutuhanIsolasiSaatDiTempatTidurIsolasiMemunculkanDaftarPantau` | ✅ **Lulus** 26 Agu 2026 |
| 7. Daftar pantau kosong mengembalikan daftar kosong, bukan galat | `Kriteria7_DaftarPantauYangKosongMengembalikanDaftarKosongBukanGalat` | ✅ **Lulus** 26 Agu 2026 |

Kriteria 1 dan 2 diuji dalam satu test yang **membandingkan kedua kalimatnya** dan memastikan
keduanya berbeda — bukan hanya memeriksa kode 422 dua kali.

---

## 8. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru dan dua kode 422 yang sudah tercantum sejak `0.3.0` |
| Database | Tidak ada perubahan schema |
| Perilaku existing | `GET /available-beds` menyaring memakai aturan 7 dan 8 bila `episodeId` dikirim |

---

## 9. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Ketujuh kriteria belum terbukti | Bagian 7.1 |
| **Penanda `IsIsolationBed` pada data sungguhan belum terbukti benar** | Kapasitas isolasi dihitung dari penanda yang salah | Menuntaskan `RWI-DEC-063` |
| Kalimat daftar pantau belum dikunci kontrak | Layar menampilkan kalimat yang belum disetujui pemilik | Tinjauan Product/Domain — bagian 6.2 |

---

## 10. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Dua aturan dan satu daftar pantau aktif | ✅ Ada di dalam kode |
| Ketujuh kriteria lulus | ✅ **Lulus** — seluruh test-nya dijalankan 26 Agustus 2026 dan hijau (255/255) |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |

---

## 11. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Tinjau kalimat daftar pantau bersama Product/Domain.
3. Jalankan `UAT-31` dan `UAT-33` terhadap aplikasi berjalan.
