# Laporan Perubahan Backend — `BE-RWI-026`

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
| Task ID | `BE-RWI-026` |
| Judul | Jalan keluar supervisor sempit dan selalu tercatat |
| Slice | S6 — Episode dapat ditutup dan tempat tidur kembali kosong |
| Trace | `RWI-DEC-015`; `RWI-RULE-009`; api contract `POST .../close-with-override`; `UAT-12`, `UAT-13` |
| Contract version | API `0.4.0` — bentuk tidak berubah; satu endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-025` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Inti task ini: jalan keluarnya **sempit**

`POST /discharges/{episodeId}/close-with-override` menembus **satu** syarat saja, yaitu
kelayakan keuangan. Keempat syarat lainnya tetap menahan, dan **tidak ada satu pun peran** yang
dapat melewatinya.

| Syarat | Ditembus jalan keluar |
| --- | :---: |
| 1. Keputusan pulang dari DPJP | Tidak |
| 2. Resume tertandatangani | Tidak |
| 3. Butir wajib administrasi | Tidak |
| 4. **Kelayakan keuangan** | **Ya** |
| 5. Keadaan tempat tidur | Tidak |

> **Kenapa ini yang paling penting.** Jalan keluar yang menembus semua syarat sekaligus akan
> menjadi jalur normal dalam hitungan minggu — bukan karena orang berniat menyalahgunakannya,
> tetapi karena ia selalu berhasil dan jalur biasa kadang tidak. Sejak saat itu kelima syarat
> tidak menahan apa pun, dan resume yang belum ditandatangani ikut lolos.

Karena itu ada test yang mencoba menembus dengan resume yang **belum** ditandatangani, dan
membuktikan penolakannya tetap berlaku.

---

## 2. Setiap penembusan meninggalkan jejak

| Yang tersimpan | Di mana |
| --- | --- |
| Penanda penembusan | `InpEpisode.IsClosedWithoutFinancialClearance` |
| Alasan supervisor | `InpEpisode.ClosedWithoutClearanceReason` |
| Pelaku, waktu, dan alasan | `InpStatusHistory` dengan `ActionType = CloseEpisodeWithOverride` |
| Daftar pantau | `GET /monitoring/closures-without-financial-clearance` |

Alasannya wajib, dan alasan yang hanya berisi tanda baca ditolak — sama seperti pada
pembatalan admisi.

> **Jalan keluar yang tidak meninggalkan jejak tidak dapat diawasi siapa pun.** Yang membuat
> jalur ini tetap menjadi pengecualian bukanlah kesulitan memakainya, melainkan kepastian
> bahwa pemakaiannya terlihat.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Ditambah | `CloseWithOverrideAsync`; konstanta `ActionCloseEpisodeWithOverride` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientClosureDtos.cs` | Ditambah | `CloseEpisodeOverrideRequest` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Ditambah | Aksi `POST /{episodeId}/close-with-override` dengan butir `InpatientEpisode : CloseOverride` |

### 3.1 Satu jalur penutupan, bukan dua

`CloseEpisodeAsync` dan `CloseWithOverrideAsync` keduanya memanggil
`CloseEpisodeInternalAsync`. Yang berbeda hanya satu penanda: apakah syarat yang bertanda
`CanBeOverridden` boleh dilewati.

> **Kenapa tidak ditulis dua jalur terpisah.** Dua jalur akan berselisih pada perubahan
> berikutnya, dan yang paling mungkin terlewat adalah jalur yang lebih jarang dipakai — yaitu
> jalan keluar supervisor. Cacat pada jalur itu justru yang paling lama tidak ketahuan.

Penanda `CanBeOverridden` berada pada **definisi syaratnya**, bukan pada logika penutupan.
Menambah syarat baru yang dapat ditembus karena itu tidak memerlukan perubahan pada jalur
penutupan sama sekali.

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Nama peran supervisor masih asumsi

Sama seperti dicatat laporan `BE-RWI-008` bagian 5.3 dan `BE-RWI-024` bagian 5.1.
`InpatientActorClaims.SupervisorRoles` berisi `SuperAdmin` dan `Supervisor`.

Bila keliru, jalan keluar ini tidak dapat dipakai siapa pun — dan pasien yang harus segera
pulang tertahan urusan kasir, yang justru merupakan keadaan yang task ini dibuat untuk
mencegahnya.

### 5.2 Butir hak akses terpisah adalah lapis pertama

`InpatientEpisode : CloseOverride` adalah butir tersendiri, sehingga mesin hak akses dapat
menjaganya. Penjaga di service adalah lapis kedua, yang bekerja walaupun butirnya keliru
diberikan lewat layar Role Access.

---

## 6. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| `UAT-12` dan `UAT-13` terhadap aplikasi berjalan | **NOT RUN** |

### 6.1 Test yang ditulis

Di dalam `InpEpisodeClosureTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Hanya supervisor yang dapat memanggilnya | `Kriteria1_HanyaSupervisorYangDapatMemanggilJalanKeluar` | ✅ **Lulus** 26 Agu 2026 |
| 2. Alasan wajib; tanpa alasan ditolak 400 | `Kriteria2_JalanKeluarTanpaAlasanDitolak400` | ✅ **Lulus** 26 Agu 2026 |
| 3. Menembus **hanya** syarat keuangan | `Kriteria3_JalanKeluarMenembusHanyaSyaratKeuangan` | ✅ **Lulus** 26 Agu 2026 |
| 4. Episode ditandai `IsClosedWithoutFinancialClearance` | `Kriteria4Dan5_EpisodeDitandaiDanMunculPadaDaftarPantauPenutupanMenembusGerbang` | ✅ **Lulus** 26 Agu 2026 |
| 5. Episode muncul pada daftar pantau penutupan menembus gerbang | Test yang sama | ✅ **Lulus** 26 Agu 2026 |

Test kriteria 3 dikerjakan persis seperti diminta roadmap: mencoba menembus dengan resume yang
**belum** ditandatangani dan butir administrasi yang **belum** ditandai, lalu memeriksa bahwa
penolakannya menyebut keduanya — dan **tidak** menyebut kelayakan keuangan, karena justru
itulah yang ditembus.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru dengan butir hak akses baru `InpatientEpisode : CloseOverride` |
| Database | Tidak ada perubahan schema; dua kolom yang diisi sudah dibuat `BE-RWI-003` |
| Keamanan | Jalur penutupan yang melewati satu gerbang, dijaga dua lapis dan selalu tercatat |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 6 |
| Nama peran supervisor keliru | Jalan keluar tidak dapat dipakai, dan pasien tertahan urusan kasir | Konfirmasi Product/Domain — bagian 5.1 |
| **Daftar pantau penembusan tidak dibaca siapa pun** | Jalur pengecualian menjadi jalur normal tanpa ada yang menyadarinya | Soal proses; perlu penanggung jawab yang membacanya berkala |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ✅ **Lulus** — seluruh test-nya dijalankan 26 Agustus 2026 dan hijau (255/255) |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Tetapkan siapa yang membaca daftar pantau penembusan gerbang, dan seberapa sering.
