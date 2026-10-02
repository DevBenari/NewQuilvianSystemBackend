# Laporan Perubahan Backend — `BE-RWI-020`

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
| Task ID | `BE-RWI-020` |
| Judul | DPJP dapat menyatakan pasien boleh pulang |
| Slice | S5 — Pasien dapat dinyatakan boleh pulang |
| Trace | `RWI-DEC-016`, `RWI-DEC-017`; `RWI-RULE-010`, `RWI-RULE-011`; `GUARD-INP-02`; api contract `POST /discharges/{episodeId}/decide`; state matrix bagian 1 |
| Contract version | API `0.4.0` — bentuk tidak berubah; satu endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-018` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN.** Dua cara pulang aturan klinisnya belum disahkan; lihat bagian 5.1 |

---

## 1. Apa yang dibangun

`POST /discharges/{episodeId}/decide` beserta `GUARD-INP-02`, dan controller
`InpatientDischargeController` baru.

Episode berpindah dari `Admitted` ke `DischargePending`, satu baris `InpStatusHistory` lahir,
dan **tempat tidurnya tetap terisi**.

---

## 2. `GUARD-INP-02` — hanya DPJP aktif

Keputusan pulang adalah keputusan klinis yang melekat pada **penanggung jawab pelayanan**, bukan
pada jabatan. Dokter jaga, kepala ruangan, dan supervisor sama-sama ditolak 403.

| Pemohon | Hasilnya |
| --- | --- |
| DPJP aktif episode itu | Diterima |
| Dokter lain | **403** — "Hanya DPJP episode ini yang dapat menyatakan pasien boleh pulang." |
| Peran bukan dokter, termasuk supervisor | **403** |

Identitas dokter dibaca dari klaim `doctor_id`, tidak pernah dari isian permintaan.

---

## 3. Tempat tidur belum dilepas — dan itu disengaja

| Yang berubah | Yang **tidak** berubah |
| --- | --- |
| `InpEpisode.EpisodeStatus` menjadi `DischargePending` | `MstBed.BedStatus` tetap `Occupied` |
| `InpEpisode.DischargeType` dan `DischargeDecidedAt` terisi | Baris `InpBedPlacement` tetap terbuka |
| Satu baris `InpStatusHistory` lahir | Pasien tetap muncul pada census |

> **Kenapa tempat tidurnya tidak dilepas di sini.** Pasien yang sudah diizinkan pulang biasanya
> masih berada di kamarnya beberapa jam — menunggu keluarga, menunggu obat, menunggu surat.
> Menganggap tempat tidurnya kosong sejak DPJP menandatangani izin akan membuat pasien
> berikutnya ditempatkan di tempat tidur yang masih ada orangnya.

Pelepasannya baru terjadi ketika kepergian fisiknya dicatat (`BE-RWI-027`) atau episodenya
ditutup (`BE-RWI-025`).

Ada test yang secara khusus memeriksa `MstBed.BedStatus` **tidak** berubah pada langkah ini.

---

## 4. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Diisi | `DecideDischargeAsync` beserta `GUARD-INP-02`; konstanta `ActionDecideDischarge` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientDischargeDtos.cs` | **Baru** | `DecideDischargeRequest` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | **Baru** | Aksi `POST /{episodeId}/decide`, ditambah tiga aksi milik `BE-RWI-021` dan `BE-RWI-022` |

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Dua cara pulang yang aturan klinisnya **belum disahkan**

Roadmap acceptance criteria 2 menyebut **lima** cara pulang sesuai `RWI-RULE-011`. Enum
`InpDischargeType` pada source hanya punya tiga nilai yang berlaku:

| Nilai | Arti |
| ---: | --- |
| 1 | `DoctorApproved` — atas izin DPJP |
| 2 | `AgainstMedicalAdvice` — pulang paksa |
| 3 | `Referred` — dirujuk |
| 4, 5 | **Sengaja dikosongkan** untuk meninggal dan kabur |

Nomor 4 dan 5 dikosongkan `BE-RWI-003` supaya penambahannya kelak tidak mengubah angka yang
sudah tersimpan. Sisi klinis keduanya **masih terbuka** pada `RWI-OQ-039` dan `RWI-DEC-059`,
menunggu pemilik klinis.

**Perilaku hari ini:** nilai 4 dan 5 ditolak 422 dengan pesan *"Cara pulang yang dipilih belum
tersedia pada versi ini."* — persis seperti validation matrix bagian 6.

**Delta yang perlu diputuskan.** Roadmap menuntut lima; enum dan validation matrix menyediakan
tiga. Salah satu dokumen perlu dikoreksi. **Owner: Product/Domain bersama Clinical governance.**

### 5.2 Tujuan rujukan tidak diminta pada langkah ini

`DecideDischargeRequest` tidak punya kolom `ReferralDestination`. Tujuan rujukan milik resume
pulang, dan diwajibkan pada saat resume **ditandatangani** — validation matrix bagian 6.

Menyediakan kolomnya di kedua tempat akan membuat dua baris menyimpan nilai yang sama, dan
keduanya akan berselisih pada kasus pertama yang tujuannya berubah.

### 5.3 Keputusan pulang tidak dapat diulang

Memutuskan pulang untuk episode yang sudah `DischargePending` ditolak 422 dengan *"Pasien sudah
diputuskan boleh pulang sebelumnya."* State matrix bagian 1.2 memang menyatakan keputusan
pulang tidak dapat dibatalkan; mengubah cara pulangnya adalah koreksi, dan koreksi memerlukan
sesi koreksi (`BE-RWI-030`).

---

## 6. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-ENUM-001`** — `InpDischargeType` dimiliki modul ini dan tidak dipakai bersama modul lain.

---

## 7. Validasi

### 7.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Pemanggilan endpoint terhadap aplikasi berjalan | **NOT RUN** |

### 7.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpDischargeDecisionTests.cs` — 6 test,
salah satunya `[Theory]` dengan tiga kasus.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Hanya DPJP aktif; peran lain dan dokter lain ditolak 403 | `Kriteria1_HanyaDpjpAktifYangDapatMemutuskan` | ✅ **Lulus** 26 Agu 2026 |
| 2. Cara pulang dikenali sesuai `RWI-RULE-011` | `Kriteria2_CaraPulangYangBerlakuPadaRevisiIniDikenali` (3 kasus) dan `Kriteria2_CaraPulangKosongDitolak400DanYangBelumTersediaDitolak422` | Ditulis; **hanya tiga dari lima** — bagian 5.1 |
| 3. Episode menjadi `DischargePending` dan tempat tidur tetap terisi | `Kriteria3Dan4Dan5_TempatTidurTetapTerisiPasienTetapDiCensusDanSatuBarisRiwayatLahir` | ✅ **Lulus** 26 Agu 2026 |
| 4. Pasien masih muncul pada census | Test yang sama | ✅ **Lulus** 26 Agu 2026 |
| 5. Keputusan menulis satu baris riwayat status | Test yang sama | ✅ **Lulus** 26 Agu 2026 |

Verifikasi yang diminta roadmap — "test yang membuktikan `MstBed.BedStatus` tidak berubah pada
langkah ini" — dijawab test kriteria 3.

Dua test tambahan menjaga: episode `Draft` belum dapat diputuskan pulang, dan keputusan pulang
tidak dapat diulang.

---

## 8. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru |
| Database | Tidak ada perubahan schema |
| Keamanan | `GUARD-INP-02` ditambahkan; ia hanya bekerja bila dipanggil — `RWI-RISK-004` |

---

## 9. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 7.1 |
| **Dua cara pulang belum disahkan** | Pasien yang meninggal atau kabur tidak dapat dicatat cara pulangnya sama sekali | `RWI-OQ-039` dan `RWI-DEC-059` — **Product/Domain bersama Clinical governance** |
| Roadmap dan enum berselisih tentang jumlah cara pulang | Pembaca berikutnya mengira ada cacat implementasi | Koreksi salah satu dokumen — bagian 5.1 |

---

## 10. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ❌ **Belum.** Kriteria 2 baru terpenuhi untuk tiga dari lima cara pulang |
| Laporan menyebut dua cara pulang yang aturan klinisnya belum final | ✅ Bagian 5.1 |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |

---

## 11. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Bawa bagian 5.1 ke Product/Domain bersama Clinical governance.
