# Laporan Perubahan Backend — `BE-RWI-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-016` |
| Judul | Sistem dapat menjawab siapa dirawat, di mana, dan sudah berapa hari |
| Slice | S3 — Sistem dapat menjawab siapa dirawat di mana |
| Trace | `RWI-DEC-027`; `RWI-RULE-019`; api contract bagian Census (3 endpoint); `FR-RI-113` s.d. `FR-RI-115`; `RWI-AC-064`; `UAT-05`, `UAT-06` |
| Contract version | API `0.4.0` — bentuk tidak berubah; tiga endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-011` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Apa yang dibangun

Tiga endpoint pada `InpatientCensusController`, seluruhnya hanya membaca:

| Endpoint | Kegunaannya |
| --- | --- |
| `GET /census/filters/metadata` | Pilihan penyaring census |
| `GET /census/summary` | Jumlah pasien dirawat per unit layanan dan per kelas perawatan |
| `GET /census` | Daftar pasien yang sedang dirawat beserta lokasi, DPJP, perawat, dan lama dirawat |

---

## 2. Dua batas yang menentukan bentuknya

### 2.1 Census dihitung, tidak pernah disimpan

Census selalu diturunkan dari baris `InpBedPlacement` yang `EndDateTime`-nya masih kosong.
**Tidak ada tabel census.**

> **Kenapa menyimpannya adalah kesalahan.** Tabel census melahirkan versi kedua yang harus
> disamakan terus-menerus dengan catatan penempatan. Setiap kali keduanya berselisih — dan
> keduanya akan berselisih — tidak ada cara mengetahui mana yang benar. Census yang dihitung
> tidak punya masalah itu sama sekali.

### 2.2 Lama dirawat dihitung dari selisih **tanggal**, bukan selisih jam

`RWI-RULE-019`. Hasilnya paling sedikit 1 hari, dan bertambah pada pergantian tanggal — bukan
setiap genap 24 jam.

| Masuk | Dibaca | Selisih jam | Hasilnya |
| --- | --- | ---: | ---: |
| 21 Sept 22:30 | 22 Sept 06:00 | 7,5 | **1** |
| 21 Sept 06:00 | 21 Sept 23:00 | 17 | **1** |
| 21 Sept 06:00 | 22 Sept 05:00 | 23 | **1** |
| 21 Sept 23:00 | 23 Sept 01:00 | 26 | **2** |

> **Baris pertama adalah yang paling sering salah.** Perhitungan berbasis jam menghasilkan
> 0 hari untuk pasien yang jelas-jelas menginap semalam — dan angka 0 itu masuk ke laporan
> hunian, statistik lama rawat, dan perhitungan tarif.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Diisi | `GetCensusAsync`, `GetCensusSummaryAsync`, `GetCensusFilterMetadataAsync`, `CalculateLengthOfStayDays`, `BuildCensusQuery` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientCensusDtos.cs` | **Baru** | `CensusQuery`, `CensusItemResponse`, `CensusPagedResult`, `CensusSummaryResponse`, `CensusSummaryGroupResponse`, `CensusFilterMetadataResponse` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientCensusController.cs` | **Baru** | Tiga aksi baca |

### 3.1 Dua penjaga untuk pasien yang sudah pergi

`BuildCensusQuery` menolak baris yang `Episode.PhysicallyLeftAt`-nya terisi, **selain** menolak
baris penempatan yang sudah ditutup.

Penjaga kedua itu berlebihan menurut alur normal — pencatatan kepergian menutup baris
penempatan pada tindakan yang sama. Ia ditulis supaya census tetap benar bila ada baris
penempatan yang tertinggal terbuka karena kejadian lama.

### 3.2 `CalculateLengthOfStayDays` dibuat `public static`

Perhitungannya dipisahkan sebagai method statis supaya dapat diuji tanpa database. Lima kasus
batas diuji sebagai `[Theory]`, bukan lewat query.

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-OPT-001` |
| Pengecualian QBE | Tidak ada |

Seluruh endpoint pada controller ini hanya membaca, dan mengikuti konvensi project, **tidak
satu pun dicatat logger**.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Lama dirawat dihitung terhadap waktu pembacaan

Untuk pasien yang masih dirawat, lama dirawat dihitung dari `AdmittedAt` sampai **waktu
pembacaan**. Census memang menjawab pertanyaan "sudah berapa hari sampai sekarang".

Untuk episode yang sudah `DischargePending`, angkanya tetap bertambah selama episodenya belum
ditutup dan kepergiannya belum dicatat. Bila Product/Domain menghendaki angkanya berhenti pada
`DischargeDecidedAt`, sebutkan — perubahannya satu baris.

### 5.2 Kriteria 4 baru dapat diuji penuh setelah `BE-RWI-027`

Roadmap sudah menyebutkannya: sampai endpoint pencatatan kepergian ada, test menyetel kolom
`PhysicallyLeftAt` langsung. Jalur endpoint-nya diuji ulang pada `BE-RWI-027`.

### 5.3 Status `Closed` disetel langsung pada test

Endpoint penutupan episode milik `BE-RWI-025`. Test kriteria 1 — "dari lima episode berstatus
berbeda, census memuat tepat dua" — menyetel kolom statusnya langsung untuk episode `Closed`.
Jalur endpoint-nya diuji ulang pada task tersebut.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **NOT RUN** — diminta pemilik pekerjaan |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | **NOT RUN** — diminta pemilik pekerjaan |
| `UAT-05` dan `UAT-06` terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpCensusTests.cs` — 7 test, salah satunya
`[Theory]` dengan lima kasus.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Census memuat `Admitted` dan `DischargePending` saja; dari lima status, tepat dua | `Kriteria1_CensusMemuatAdmittedDanDischargePendingSaja` | Ditulis, **belum dijalankan** |
| 2. Lama dirawat dari selisih tanggal, paling sedikit 1 hari | `Kriteria2Dan3_LamaDirawatDihitungDariSelisihTanggalDenganHasilPalingSedikitSatu` — 5 kasus batas | Ditulis, **belum dijalankan** |
| 3. Bertambah pada pergantian tanggal, bukan setiap 24 jam | Test yang sama, dua kasus terakhir | Ditulis, **belum dijalankan** |
| 4. Pasien yang kepergiannya sudah dicatat tidak muncul | `Kriteria4_PasienYangKepergiannyaSudahDicatatTidakMunculPadaCensus` | Ditulis, **belum dijalankan**; jalur endpoint menunggu `BE-RWI-027` |
| 5. Ringkasan menghitung per unit layanan dan per kelas | `Kriteria5_RingkasanMenghitungPerUnitLayananDanPerKelas` | Ditulis, **belum dijalankan** |

Dua test tambahan menjaga: census menampilkan lokasi, DPJP, dan perawat penanggung jawab; dan
census dapat disaring unit layanan, kamar, serta kebutuhan isolasi. Satu test terakhir memeriksa
bahwa `CensusItemResponse` **tidak** memuat kolom klinis mana pun.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Tiga endpoint baru |
| Database | Tidak ada perubahan schema |
| Keamanan | Census dibaca hampir seluruh peran ruangan; isi klinis dijaga tidak ikut |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 6.1 |
| Kriteria 4 baru terbukti separuh | Jalur endpoint pencatatan kepergian belum ada | `BE-RWI-027` |
| Perilaku lama dirawat untuk `DischargePending` belum dikonfirmasi | Angka lama rawat pada laporan dapat berbeda dari yang dimaksud | Konfirmasi Product/Domain — bagian 5.1 |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Tiga endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan |
| Unit test perhitungan lulus | ❌ **Belum dijalankan** — lima kasus batas sudah ditulis |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Konfirmasi perilaku lama dirawat untuk episode `DischargePending` (bagian 5.1).
3. Uji ulang kriteria 4 lewat endpoint pada `BE-RWI-027`.
