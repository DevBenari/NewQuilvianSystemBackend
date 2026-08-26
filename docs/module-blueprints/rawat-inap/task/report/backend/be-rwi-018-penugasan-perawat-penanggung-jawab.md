# Laporan Perubahan Backend — `BE-RWI-018`

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
| Task ID | `BE-RWI-018` |
| Judul | Perawat penanggung jawab tercatat, dan ketiadaannya tidak menahan apa pun |
| Slice | S4 — Penanggung jawab dan perpindahan |
| Trace | `RWI-DEC-032`; `RWI-RULE-023`; api contract `POST` dan `GET /episodes/{id}/nurse-assignments`; `FR-RI-119` |
| Contract version | API `0.4.0` — bentuk tidak berubah; dua endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-016` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN.** Kriteria 3 terbukti di tingkat service saja; lihat bagian 5.1 |

---

## 1. Apa yang dibangun

| Endpoint | Kegunaannya |
| --- | --- |
| `POST /episodes/{id}/nurse-assignments` | Menugaskan atau mengganti perawat penanggung jawab |
| `GET /episodes/{id}/nurse-assignments` | Riwayat perawat penanggung jawab, urut nomor urut |

Bentuk berperiodenya sama persis dengan penugasan DPJP dari `BE-RWI-017`.

---

## 2. Yang paling mudah dikerjakan terbalik

Acceptance criteria 2: **episode boleh berjalan tanpa perawat penanggung jawab, dan selama itu
tidak ada satu pun tindakan yang tertahan.**

> **Kenapa `RWI-DEC-032` memilih tidak menahan.** Penugasan perawat sering menyusul beberapa
> menit setelah pasien tiba di ruangan — kepala ruangan menentukannya setelah melihat beban
> shift yang sedang berjalan. Menahan penempatan, perpindahan, atau keputusan pulang sampai
> kolom itu terisi hanya memindahkan antrean ke tempat lain: pasien menunggu di lorong sementara
> seseorang mencari kepala ruangan.

Yang muncul untuk episode tanpa perawat adalah **daftar pantau**, bukan penolakan.

Ada satu test yang membuktikan ketiga tindakan besar — penempatan, perpindahan, dan keputusan
pulang — semuanya tetap berhasil tanpa perawat penanggung jawab.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Assignments.cs` | Ditambah | `AssignNurseAsync`, `GetNurseAssignmentsAsync` |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Ditambah | `GetUnassignedNurseEpisodesAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeAssignmentDtos.cs` | Ditambah | `AssignNurseRequest`, `InpatientNurseAssignmentResponse` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Ditambah | Dua aksi penugasan perawat |

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

### 5.1 Daftar pantau ada di service, endpointnya belum

Acceptance criteria 3 menuntut episode tanpa perawat muncul pada daftar pantau kepala ruangan.
Endpoint-nya — `GET /monitoring/unassigned-nurse-episodes` — tercantum pada api contract tetapi
**milik `BE-RWI-029`** beserta tiga daftar pantau lainnya.

Yang dikerjakan di sini adalah **query-nya saja**, sebagai
`InpCensusQueryService.GetUnassignedNurseEpisodesAsync`. Alasannya:

1. tanpa query itu, acceptance criteria 3 tidak dapat dibuktikan sama sekali pada task ini; dan
2. `BE-RWI-029` cukup memasang endpoint tanpa menulis ulang query-nya.

**Ini pelebaran scope yang disadari dan dibatasi.** Tidak ada endpoint baru yang dibuka, tidak
ada butir hak akses baru, dan tidak ada bentuk jawaban baru yang dikunci. Bila pemilik roadmap
menghendaki query itu ditunda ke `BE-RWI-029`, penghapusannya satu method.

### 5.2 Index unik satu perawat aktif per episode perlu dikonfirmasi

`requirement-traceability.md` sudah mencatatnya sebagai pertanyaan terbuka:
`IX_InpNurseAssignment_EpisodeId_Active` membatasi satu perawat aktif per episode, dan itu
perlu dipastikan cocok dengan kenyataan ruangan.

Implementasi ini **mengikuti** index tersebut: penugasan baru menutup seluruh penugasan yang
masih aktif. Bila kenyataannya satu episode dapat punya beberapa perawat penanggung jawab
sekaligus — misalnya per shift — index dan implementasinya sama-sama perlu diubah, dan itu
keputusan Domain, bukan pelaksana.

### 5.3 Kewenangan berada di service

Sama seperti pengalihan DPJP: butir hak aksesnya `InpatientEpisode : Update`, dan pembedaan
"hanya kepala ruangan atau supervisor" berada di service.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Pemanggilan kedua endpoint terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

Di dalam `QuilvianSystemBackend.Tests/InPatientManagement/InpDoctorAndNurseAssignmentTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Penugasan menutup penugasan sebelumnya dan membuka yang baru | `Kriteria1Dan4_PenugasanMenutupPenugasanSebelumnyaDanRiwayatnyaTerbacaUrut` | ✅ **Lulus** 26 Agu 2026 |
| 2. Episode boleh berjalan tanpa perawat; tidak ada tindakan yang tertahan | `Kriteria2_PenempatanPerpindahanDanKeputusanPulangSemuanyaBerhasilTanpaPerawat` | ✅ **Lulus** 26 Agu 2026 |
| 3. Episode tanpa perawat muncul pada daftar pantau | `Kriteria3_EpisodeTanpaPerawatMunculPadaDaftarPantauLaluHilangSetelahDitugaskan` — **tingkat service** | Ditulis; endpoint menunggu `BE-RWI-029` |
| 4. Riwayat perawat terbaca urut | Test kriteria 1 | ✅ **Lulus** 26 Agu 2026 |
| 5. Penugasan hanya oleh kepala ruangan atau supervisor | `Kriteria5_PenugasanPerawatHanyaOlehKepalaRuanganAtauSupervisor` | ✅ **Lulus** 26 Agu 2026 |

Kriteria 2 diuji persis seperti diminta roadmap: penempatan, perpindahan, **dan** keputusan
pulang ketiganya dijalankan pada episode yang tidak punya perawat, dan ketiganya berhasil.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Dua endpoint baru |
| Database | Tidak ada perubahan schema |
| Scope | Satu method query tambahan pada `InpCensusQueryService` yang endpoint-nya milik `BE-RWI-029` — bagian 5.1 |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 6.1 |
| Kriteria 3 belum punya endpoint | Kepala ruangan belum dapat melihat daftar pantaunya | `BE-RWI-029` |
| Index unik satu perawat aktif belum dikonfirmasi | Ruangan yang menugaskan perawat per shift tidak dapat mencatatnya | Keputusan Domain — bagian 5.2 |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Dua endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ❌ **Belum.** Kriteria 3 baru terbukti di tingkat service |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Bawa pertanyaan index unik satu perawat aktif ke Domain (bagian 5.2).
3. Pasang endpoint daftar pantau pada `BE-RWI-029`.
