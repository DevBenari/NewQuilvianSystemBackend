# Laporan Perubahan Backend — `BE-RWI-017`

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
| Task ID | `BE-RWI-017` |
| Judul | Sistem dapat menjawab siapa DPJP pada tanggal tertentu |
| Slice | S4 — Penanggung jawab dan perpindahan |
| Trace | `RWI-DEC-022`, `RWI-DEC-024`; `RWI-RULE-016`; `GUARD-INP-01`; api contract `POST` dan `GET /episodes/{id}/doctor-assignments`; `FR-RI-116` s.d. `FR-RI-118`; `UAT-07` |
| Contract version | API `0.4.0` — bentuk tidak berubah; dua endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-016` — dikerjakan pada sesi yang sama |
| Status | ✅ **SELESAI** — seluruh acceptance criteria dan DoD terbukti; build+test hijau dan endpoint terbukti berjalan 26 Agustus 2026 |

---

## 1. Apa yang dibangun

| Endpoint | Kegunaannya |
| --- | --- |
| `POST /episodes/{id}/doctor-assignments` | Mengalihkan DPJP: menutup penugasan lama dan membuka yang baru |
| `GET /episodes/{id}/doctor-assignments` | Riwayat DPJP episode, urut nomor urut |

Ditambah `GUARD-INP-01` sebagai method yang dapat dipanggil ulang — `IsActiveDoctorAsync` —
yang sudah dipakai `BE-RWI-014`, `BE-RWI-019`, `BE-RWI-020`, dan `BE-RWI-021`.

---

## 2. Kenapa berbentuk riwayat berperiode

> **Skenario roadmap.** dr. Andi memegang Tn. Budi 21–23 September. dr. Rina memegangnya
> 23–25 September. Pada 25 September auditor bertanya: siapa yang berwenang pada 22 September?
>
> Dengan riwayat berperiode, sistem menjawab **dr. Andi**. Dengan kolom `CurrentDoctorId` yang
> ditimpa, sistem menjawab **dr. Rina** — dan jawaban itu salah tanpa ada cara mengetahuinya.

Riwayat DPJP dipakai resume pulang dan penagihan. Menyimpannya sebagai satu kolom yang ditimpa
membuat query lebih murah dan menghapus jawaban itu selamanya. Bentuk berperiode terkunci pada
`blueprint-manifest.md` bagian 8 butir 4; menggantinya bukan keputusan pelaksana.

`GetDoctorAssignmentAtAsync` adalah method yang menjawab pertanyaan itu: ia mencari penugasan
yang periodenya mencakup satu titik waktu.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Assignments.cs` | Ditambah | `HandoverDoctorAsync`, `GetDoctorAssignmentsAsync`, `GetDoctorAssignmentAtAsync`, `GetActiveDoctorIdAsync`, `IsActiveDoctorAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeAssignmentDtos.cs` | Ditambah | `HandoverDoctorRequest`, `InpatientDoctorAssignmentResponse` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Ditambah | Dua aksi penugasan DPJP |

### 3.1 `INV-INP-03` dijaga tiga lapis

| Lapis | Isinya |
| --- | --- |
| 1. Pengalihan menutup dan membuka pada tindakan yang sama | Tidak pernah ada satu saat pun episode tanpa DPJP, dan tidak pernah ada dua |
| 2. Pemeriksaan sebelum menulis | Bila ternyata sudah ada lebih dari satu penugasan aktif, permintaan **ditolak** alih-alih menambah yang ketiga |
| 3. Unique index parsial `IX_InpDoctorAssignment_EpisodeId_Active` | Menolak baris kedua bila dua pengalihan terjadi bersamaan |

Lapis 3 **belum terbukti** pada sesi ini; provider InMemory tidak menegakkan unique index.

### 3.2 Tidak ada kolom waktu mulai pada permintaan

`HandoverDoctorRequest` hanya punya `DoctorId` dan `HandoverReason`. Pengalihan berlaku sejak
permintaannya diterima.

> Menerima waktu dari pemanggil membuka jalan bagi periode yang tumpang tindih maupun berlubang
> — dan riwayat berperiode kehilangan gunanya begitu itu terjadi.

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

**`QBE-TXN-001`** — penutupan penugasan lama dan pembukaan penugasan baru berada di dalam satu
transaksi.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Kewenangan berada di service, bukan di mesin hak akses

Pengalihan DPJP memakai butir `InpatientEpisode : Update` — sama dengan pembatalan admisi dan
penugasan perawat. Mesin hak akses karena itu tidak dapat membedakan ketiganya.

Penjaga "hanya kepala ruangan atau supervisor" berada di
`InpEpisodeService.HandoverDoctorAsync`, dan controller hanya menyampaikan apakah pelakunya
berperan demikian. Daftar nama peran yang dipakai masih **asumsi** yang sama seperti dicatat
laporan `BE-RWI-008` bagian 5.3.

### 5.2 Mengalihkan kepada DPJP yang sama ditolak

Permintaan yang menunjuk dokter yang sudah menjadi DPJP aktif ditolak 422. Ini keputusan
implementasi, bukan acceptance criteria: menerimanya akan melahirkan baris riwayat yang
periodenya nol detik dan tidak berarti apa-apa.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Test unique index parsial DPJP aktif terhadap PostgreSQL | **NOT RUN** |
| `UAT-07` terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

Di dalam `QuilvianSystemBackend.Tests/InPatientManagement/InpDoctorAndNurseAssignmentTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Riwayat berperiode masih menjawab siapa berwenang pada 22 September | `Kriteria1_RiwayatBerperiodeMasihDapatMenjawabSiapaBerwenangPadaTanggalLampau` | ✅ **Lulus** 26 Agu 2026 |
| 2. Satu episode aktif punya tepat satu DPJP aktif | `Kriteria2_SatuEpisodeAktifPunyaTepatSatuDpjpAktif` | ✅ **Lulus** 26 Agu 2026 |
| 3. Pengalihan tanpa alasan ditolak 400 | `Kriteria3_PengalihanTanpaAlasanDitolak400` | ✅ **Lulus** 26 Agu 2026 |
| 4. Pengalihan hanya oleh kepala ruangan atau supervisor | `Kriteria4_PengalihanHanyaOlehKepalaRuanganAtauSupervisor` | ✅ **Lulus** 26 Agu 2026 |
| Unique index parsial DPJP aktif | **Tidak dapat diuji InMemory** | Tertunda |

Satu test tambahan menjaga: mengalihkan kepada DPJP yang sama ditolak.

Test kriteria 1 memundurkan waktu penugasan secara langsung untuk meniru berjalannya hari.
Ini disengaja: permintaan pengalihan tidak menerima waktu dari pemanggil (bagian 3.2), sehingga
periode lampau hanya dapat dibentuk lewat penyuntingan data uji.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Dua endpoint baru |
| Database | Tidak ada perubahan schema |
| Keamanan | `GUARD-INP-01` kini tersedia sebagai method yang dapat dipanggil ulang, dan sudah dipakai empat task lain |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Keempat kriteria belum terbukti | Bagian 6.1 |
| Unique index parsial belum diuji | Dua pengalihan bersamaan dapat menghasilkan dua DPJP aktif | Test terhadap PostgreSQL |
| Nama peran kepala ruangan dan supervisor masih asumsi | Kepala ruangan yang sah ditolak 403 | Konfirmasi Product/Domain |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Dua endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Keempat kriteria lulus | ✅ **Lulus** — dijalankan 26 Agustus 2026, hijau (255/255) |
| `GUARD-INP-01` dapat dipanggil ulang | ✅ `IsActiveDoctorAsync` dipakai `BE-RWI-014`, `019`, `020`, `021` |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Jalankan test unique index parsial terhadap PostgreSQL.
3. Konfirmasi nama peran kepala ruangan dan supervisor.
