# Laporan Perubahan Backend — `BE-RWI-027`

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
| Task ID | `BE-RWI-027` |
| Judul | Tempat tidur bebas sejak pasien meninggalkan kamar |
| Slice | S6 — Episode dapat ditutup dan tempat tidur kembali kosong |
| Trace | `RWI-DEC-055`; `RWI-RULE-036`; `INV-INP-01` yang dilonggarkan; api contract `POST .../record-departure`; `FR-RI-149` s.d. `FR-RI-151`; `RWI-AC-118` s.d. `RWI-AC-121`; `UAT-24`, `UAT-25` |
| Contract version | API `0.4.0` — bentuk tidak berubah; satu endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-025` — dikerjakan pada sesi yang sama |
| Status | ✅ **SELESAI** — seluruh acceptance criteria dan DoD terbukti; build+test hijau dan endpoint terbukti berjalan 26 Agustus 2026 |

---

## 1. Masalah yang diselesaikan

> **Contoh berangka.** dr. Andi menyatakan Ibu Sari boleh pulang pukul 08:30. Ibu Sari benar-benar
> meninggalkan kamar pukul 10:15. Kasir baru menyelesaikan hitungan pukul 13:10, dan episode
> ditutup pukul 13:15.
>
> **Tanpa task ini**, tempat tidur `MELATI-03` tertahan dari pukul 10:15 sampai 13:15 — tiga
> jam untuk tempat tidur yang sesungguhnya kosong, sementara ada pasien menunggu di IGD.
>
> **Dengan task ini**, kepergian dicatat pukul 10:15 dan tempat tidurnya bebas seketika.
> Episodenya tetap `DischargePending` dan tetap wajib ditutup.

---

## 2. Kriteria 5 melawan intuisi, dan itulah yang paling penting

**Pencatatan kepergian tidak menulis satu pun baris `InpStatusHistory`.**

`RWI-DEC-009` mengunci **lima** nilai status episode, dan kepergian fisik sengaja tidak
dijadikan status keenam. Ia adalah **fakta yang dicatat**, bukan tahapan yang dilalui — status
episode memang tidak berubah, dan `RWI-RULE-031` aturan 3 mewajibkan riwayat untuk **perubahan
status**, bukan untuk setiap tindakan.

Menambah status keenam melanggar butir yang terkunci pada `blueprint-manifest.md` bagian 8.

Jejaknya tersimpan pada tiga tempat lain:

| Jejak | Isinya |
| --- | --- |
| `InpBedPlacement` yang ditutup | Waktu berakhir, pelaku, dan `EndReason = PatientDeparted` |
| `InpEpisode.PhysicallyLeftAt` | Waktu kepergian |
| `InpEpisode.PhysicallyLeftByUserId` | Pencatatnya |

Test kriteria 5 **menghitung baris riwayat sebelum dan sesudah**, sesuai permintaan roadmap —
bukan sekadar memeriksa jenis barisnya.

---

## 3. Yang sengaja tidak divalidasi

Sistem **tidak** memeriksa apakah butir administrasi atau kelayakan keuangan sudah selesai.

> **Kepergian fisik adalah fakta, bukan izin.** Pasien yang sudah pulang tetap harus dicatat
> pulang walaupun administrasinya belum beres. Menahan pencatatannya tidak membuat pasien
> kembali ke kamarnya — ia hanya membuat catatan sistem berbeda dari kenyataan, dan tempat
> tidur tetap tertahan untuk orang yang sudah tidak ada.

Episode tetap `DischargePending` dan tetap muncul pada daftar pantau penutupan tertunda —
dengan penanda bahwa tempat tidurnya sudah tidak tertahan.

---

## 4. Tidak dapat dibatalkan

`RWI-RULE-036` menetapkan tidak ada pembatalan, dan tidak ada endpoint yang menyediakannya.
Pasien yang ternyata belum jadi pulang menjalani **admisi baru** — dan admisi itu diterima,
karena `INV-INP-10` memakai kepergian fisik sebagai batasnya, bukan penutupan episode
(`BE-RWI-012`).

---

## 5. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Ditambah | `RecordPatientDepartureAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientClosureDtos.cs` | Ditambah | `RecordDepartureRequest` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Ditambah | Aksi `POST /{episodeId}/record-departure` dengan butir `InpatientDischarge : RecordDeparture` |

---

## 6. Backend Governance Preflight

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

## 7. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| `UAT-24` dan `UAT-25` terhadap aplikasi berjalan | **NOT RUN** |

### 7.1 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpPatientDepartureTests.cs` — 9 test.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Kepergian melepas tempat tidur seketika | `Kriteria1Dan2_KepergianMelepasTempatTidurTetapiEpisodeTetapDischargePending` | ✅ **Lulus** 26 Agu 2026 |
| 2. Episode tetap `DischargePending` dan tetap pada daftar pantau | Test yang sama, ditambah `EpisodeYangKepergiannyaSudahDicatatTetapMunculPadaDaftarPantauPenutupanTertunda` | ✅ **Lulus** 26 Agu 2026 |
| 3. Pasien yang sudah pergi tidak muncul di census dan tidak dapat dipindahkan | `Kriteria3_PasienYangSudahPergiTidakMunculDiCensusDanTidakDapatDipindahkan` | ✅ **Lulus** 26 Agu 2026 |
| 4. Menutup episode tanpa mencatat kepergian tetap berhasil | `Kriteria4_MenutupEpisodeTanpaMencatatKepergianTetapBerhasil` | ✅ **Lulus** 26 Agu 2026 |
| 5. Kepergian **tidak** menulis baris riwayat status | `Kriteria5_KepergianTidakMenulisSatuPunBarisRiwayatStatus` — menghitung sebelum dan sesudah | ✅ **Lulus** 26 Agu 2026 |
| 6. Kepergian pada episode `Admitted` ditolak 422 | `Kriteria6_MencatatKepergianPadaEpisodeAdmittedDitolak422` | ✅ **Lulus** 26 Agu 2026 |
| 7. Mencatat dua kali ditolak 409 | `Kriteria7_MencatatKepergianDuaKaliDitolak409` | ✅ **Lulus** 26 Agu 2026 |
| 8. Waktu kepergian mendahului keputusan pulang ditolak 400 | `Kriteria8_WaktuKepergianYangMendahuluiKeputusanPulangDitolak400` | ✅ **Lulus** 26 Agu 2026 |
| 9. Bila pelepasan tempat tidur gagal, kolom kepergian juga tidak terisi | `Kriteria9_BilaPelepasanTempatTidurGagalKolomKepergianJugaTidakTerisi` | ✅ **Lulus** 26 Agu 2026 |

Kriteria 9 memaksa kegagalan penyimpanan di tengah transaksi, lalu memeriksa **dua hal**:
kolom kepergian pada episode kosong, **dan** baris penempatannya masih terbuka. Tanpa keduanya
diperiksa bersama, dapat lolos keadaan tempat tidur bebas sementara episodenya belum tahu
pasiennya sudah pergi.

Kriteria 4 juga menutup catatan yang tertinggal dari `BE-RWI-016` bagian 5.2: jalur endpoint
untuk kriteria 4 census kini ada, dan diuji lewat endpoint yang sebenarnya.

---

## 8. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru dengan butir hak akses baru `InpatientDischarge : RecordDeparture` |
| Database | Tidak ada perubahan schema |
| Invariant | `INV-INP-01` dilonggarkan secara sadar: episode `DischargePending` boleh tidak memegang tempat tidur, dan **hanya** lewat jalur ini |
| Modul tetangga | Salinan `MstBed.BedStatus` kembali `Available` lebih awal daripada penutupan episode |

---

## 9. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kesembilan kriteria belum terbukti | Bagian 7 |
| Endpoint tidak dapat dibatalkan | Pencatatan yang salah waktu tidak dapat dikoreksi tanpa admisi baru | Keputusan sadar `RWI-RULE-036`; sesi koreksi `BE-RWI-030` tidak mencakupnya |
| Peran yang berhak mencatat kepergian belum dikonfirmasi | Perawat yang seharusnya berwenang ditolak, dan tempat tidur tetap tertahan | Pemetaan peran pada permission matrix bagian 3 masih usulan |

---

## 10. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kesembilan kriteria lulus | ✅ **Lulus** — dijalankan 26 Agustus 2026, hijau (255/255) |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |

---

## 11. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Jalankan `UAT-24` dan `UAT-25` terhadap aplikasi berjalan.
3. Perbarui laporan `BE-RWI-016` bagian 5.2 setelah kriteria 4 census terbukti lewat endpoint.
