# Laporan Perubahan Backend — `BE-RWI-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-012` |
| Judul | Satu pasien tidak pernah tercatat dirawat di dua tempat |
| Slice | S2 — Pasien punya lokasi, dan penempatan yang tidak layak ditolak |
| Trace | `RWI-DEC-054`; `RWI-RULE-035`; `INV-INP-10`; `02-backend-architecture.md` §1.5; `FR-RI-148`; `RWI-AC-116`, `RWI-AC-117`; `UAT-26` |
| Contract version | API `0.4.0` — bentuk tidak berubah; satu kode 409 baru pada penempatan |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-011` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Apa yang dibangun

Pemeriksaan `INV-INP-10` di dalam `InpEpisodeService`, dipanggil
`InpBedOccupancyService.PlacePatientAsync` **sebelum** penempatan dibuat, beserta kalimat
penolakan yang menyebut nomor episode dan lokasi yang sedang ditempati.

---

## 2. Proses bisnis

### 2.1 Apa arti "benar-benar hadir"

| Keadaan episode | Pasien dianggap hadir | Menghalangi penempatan baru |
| --- | :---: | :---: |
| `Draft` | Tidak | Tidak — hanya memunculkan peringatan |
| `Admitted` | **Ya** | **Ya** |
| `DischargePending`, kepergian **belum** dicatat | **Ya** | **Ya** |
| `DischargePending`, kepergian **sudah** dicatat | Tidak | Tidak |
| `Closed` atau `Cancelled` | Tidak | Tidak |

### 2.2 Kenapa batasnya kepergian fisik, bukan penutupan episode

> **Contoh berangka.** Tn. Budi pulang pukul 10:15 dan kepergiannya dicatat. Episodenya baru
> ditutup pukul 13:10 karena kasir masih menghitung tagihan. Pukul 12:00 Tn. Budi kembali
> dengan keluhan baru.
>
> Bila batasnya penutupan episode, admisi barunya **ditolak** — pasien tertahan di IGD selama
> satu jam sepuluh menit oleh urusan administrasi yang tidak ada hubungannya dengan kondisi
> klinisnya. Karena itu `RWI-DEC-054` memilih kepergian fisik sebagai batasnya.

Inilah alasan `InpEpisode.PhysicallyLeftAt` disimpan sebagai kolom pada episode, bukan hanya
diturunkan dari baris penempatan: tanpa kolom itu, unique index parsial
`IX_InpEpisode_PatientId_Present` tidak dapat dirumuskan.

### 2.3 Bentuk kalimat penolakannya

> *"Tn. Budi sudah dirawat pada episode RI-260921093012-A1B2C3 di Melati 3 3B. Bila memang
> pindah kamar, pakai perpindahan, bukan admisi baru."*

Nomor episode dan lokasi **wajib** ada di dalamnya. Penolakan tanpa keduanya memaksa petugas
mencari sendiri di mana pasiennya berada — dan pencarian itulah yang biasanya berakhir dengan
admisi kedua.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Assignments.cs` | **Baru** | `FindPresentEpisodeAsync`; record `InpPresentEpisodeInfo` beserta `LocationText` dan `RejectionMessage` |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Ditambah | Pemanggilan `FindPresentEpisodeAsync` di dalam `PlacePatientAsync` |

Dua lapis, sama seperti `INV-INP-02`:

| Lapis | Isinya | Diuji di mana |
| --- | --- | --- |
| 1. Pemeriksaan service | Menolak dengan kalimat yang dapat dibaca petugas | Test InMemory |
| 2. Unique index parsial `IX_InpEpisode_PatientId_Present` | Menolak baris kedua walaupun dua permintaan sama-sama lolos lapis 1 | **Belum** — hanya PostgreSQL |

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-API-001`, `QBE-DTO-001` |
| Pengecualian QBE | Tidak ada |

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Pemeriksaan dijalankan pada penempatan, bukan pada pembukaan admisi

Membuka admisi untuk pasien yang sedang dirawat **tidak** ditolak; yang ditolak adalah
menempatkannya. Ini sesuai validation matrix bagian 3, yang menempatkan aturan ini pada
`POST /bed-occupancies/placements`.

Alasannya masuk akal secara operasional: petugas admisi kadang menyiapkan admisi untuk pasien
yang perpindahannya sedang diurus, dan menolaknya di langkah pertama akan menghalangi pekerjaan
yang sah. Yang tidak boleh terjadi hanyalah **dua penempatan aktif**.

### 5.2 Lokasi pada kalimat penolakan dapat kosong

Bila episode yang menghalangi berstatus `Admitted` tetapi baris penempatannya sudah tertutup
karena kejadian lama, kalimatnya berbunyi "di tempat tidur yang belum tercatat". Keadaan itu
seharusnya tidak mungkin terjadi menurut `INV-INP-01`, tetapi kalimatnya tetap disiapkan supaya
penolakan tidak berubah menjadi galat.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **NOT RUN** — diminta pemilik pekerjaan |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | **NOT RUN** — diminta pemilik pekerjaan |
| Test unique index parsial `IX_InpEpisode_PatientId_Present` terhadap PostgreSQL | **NOT RUN** |

### 6.2 Test yang ditulis

Di dalam `InpBedPlacementTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Pasien yang sudah `Admitted` ditolak 409 dengan nomor episode dan lokasi | `InvInp10Kriteria1_MenempatkanPasienYangSudahDirawatDitolak409DenganNomorEpisodeDanLokasi` | Ditulis, **belum dijalankan** |
| 2. Admisi untuk pasien yang punya `Draft` lain tetap berhasil disertai peringatan | `InvInp10Kriteria2_MembukaAdmisiUntukPasienYangPunyaDraftLainTetapBerhasilDenganPeringatan` | Ditulis, **belum dijalankan** |
| 3. `DischargePending` yang kepergiannya **belum** dicatat ditolak 409 | `InvInp10Kriteria3Dan4_BatasnyaKepergianFisikBukanPenutupanEpisode` | Ditulis, **belum dijalankan** |
| 4. `DischargePending` yang kepergiannya **sudah** dicatat berhasil | Test yang sama | Ditulis, **belum dijalankan** |

Kriteria 3 dan 4 sengaja ditulis **berpasangan dalam satu berkas test**, sesuai permintaan
roadmap. Batas di antara keduanya adalah inti aturan ini: yang pertama mencegah data ganda,
yang kedua mencegah pasien tertahan oleh urusan administrasi. Menguji salah satunya saja
menghasilkan rasa aman yang palsu.

Kolom `PhysicallyLeftAt` disetel langsung pada test kriteria 4, karena endpoint pencatatan
kepergian milik `BE-RWI-027`. Jalur endpoint-nya diuji ulang pada task tersebut.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | Satu kode 409 baru pada `POST /placements`, sudah tercantum pada api contract `0.2.0` |
| Database | Tidak ada perubahan schema |
| Keamanan | Tidak ada |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Keempat kriteria belum terbukti | Bagian 6.1 |
| Unique index parsial belum diuji | Dua permintaan bersamaan dapat menghasilkan dua episode hadir | Test terhadap PostgreSQL |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Pemeriksaan aktif | ✅ Ada di dalam kode dan dipanggil `PlacePatientAsync` |
| Keempat kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan |
| Pesan penolakan sesuai validation matrix | ✅ Kalimatnya sama, dengan nomor episode dan lokasi disisipkan |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Jalankan test unique index parsial terhadap PostgreSQL.
