# Laporan Perubahan Backend — `BE-RWI-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-023` |
| Judul | Daftar periksa administrasi dapat ditandai dan bersifat menahan |
| Slice | S6 — Episode dapat ditutup dan tempat tidur kembali kosong |
| Trace | `RWI-DEC-026`, `RWI-DEC-033`; `RWI-RULE-018`, `RWI-RULE-024`; api contract `GET .../clearance` dan `POST .../clearance/{itemId}/mark` |
| Contract version | API `0.4.0` — bentuk tidak berubah; dua endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-005` 🟡 |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

> **Pengerjaan dilakukan tanpa menjalankan build**, mengikuti cara kerja yang berlaku pada sesi
> ini. `dotnet build` dan `dotnet test` **tidak dijalankan**. Task ini belum boleh ditandai
> selesai.

---

## 1. Apa yang dibangun

| Endpoint | Kegunaannya |
| --- | --- |
| `GET /discharges/{episodeId}/clearance` | Daftar butir administrasi beserta status penandaannya |
| `POST /discharges/{episodeId}/clearance/{itemId}/mark` | Menandai satu butir |

Butir wajib yang belum ditandai menjadi syarat ketiga penutupan episode, dan itulah yang
membuat daftar ini **menahan**, bukan sekadar mencatat.

---

## 2. Tiga sifat yang membedakan implementasi ini

### 2.1 Yang menahan hanyalah butir wajib **yang masih aktif**

Sebuah butir menahan penutupan bila ketiganya benar: `IsMandatory`, `IsActive`, dan belum
ditandai. Butir tidak wajib tidak pernah menahan — bila ia ikut menahan, penanda `IsMandatory`
kehilangan artinya dan seluruh butir diperlakukan sama.

### 2.2 Penandaan lama tidak pernah hilang

Daftar memuat seluruh butir yang masih aktif, **ditambah** butir yang sudah dinonaktifkan
tetapi pernah ditandai pada episode ini.

> **Kenapa.** Admin menonaktifkan butir "Surat keterangan dirawat" pada bulan Maret. Episode
> yang ditutup pada Februari sudah menandainya, dan penandaan itu adalah bukti bahwa surat
> tersebut memang diserahkan. Menghilangkannya dari layar membuat episode lama terlihat seolah
> tidak pernah lengkap — dan pada audit setahun kemudian, hilangnya jejak itu tidak dapat
> dijelaskan siapa pun.

Butir yang dinonaktifkan **tidak lagi menahan**, tetapi barisnya tetap terbaca beserta
pelaku, waktu, dan catatannya.

### 2.3 Butir obat pulang ditandai manual

Modul Farmasi di luar scope revisi ini (`DEC-INP-001`). Tidak ada penandaan otomatis yang
menebak apakah obat pulang sudah diserahkan.

> Penandaan yang menebak lebih berbahaya daripada penandaan manual: ia terlihat seperti bukti
> padahal bukan.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | **Baru** | `GetClearanceChecklistAsync`, `MarkClearanceItemAsync`, `GuardEpisodeNotClosedAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientClosureDtos.cs` | **Baru** | `ClearanceChecklistResponse`, `ClearanceChecklistItemResponse`, `MarkClearanceItemRequest` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Ditambah | Dua aksi daftar periksa |

### 3.1 Penandaan ulang memperbarui, bukan menduplikasi

Unique index `(EpisodeId, ClearanceItemId)` membatasi satu penandaan per butir per episode.
Permintaan kedua memperbarui catatan dan pelakunya, bukan melahirkan baris kedua.

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

`MstInpatientClearanceItem` dan seedernya dipakai apa adanya dari `BE-RWI-001` dan
`BE-RWI-002`; tidak ada master baru.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Menandai butir yang sudah tidak aktif ditolak

Butir yang dinonaktifkan admin tidak dapat ditandai lagi (422). Ini keputusan implementasi,
bukan acceptance criteria: menandai butir yang sudah dicabut menghasilkan jejak yang tidak
dapat dijelaskan pada audit.

Penandaan yang **sudah ada sebelumnya** tetap terbaca — yang ditolak hanyalah penandaan baru.

### 5.2 Penandaan mengikuti gerbang episode tertutup

Butir tidak dapat ditandai pada episode yang sudah `Closed`, kecuali ada sesi koreksi terbuka
(`INV-INP-06`). Pemeriksaannya dipakai bersama dengan penandaan kelayakan keuangan.

---

## 6. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **NOT RUN** |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | **NOT RUN** |
| Pemanggilan kedua endpoint terhadap aplikasi berjalan | **NOT RUN** |

### 6.1 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpClearanceAndFinancialTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Daftar menampilkan seluruh butir aktif beserta status penandaannya | `Kriteria1Dan2_DaftarMenampilkanButirAktifDanPenandaanMenyimpanPelakuSertaWaktunya` | Ditulis, **belum dijalankan** |
| 2. Menandai butir menyimpan pelaku dan waktunya | Test yang sama | Ditulis, **belum dijalankan** |
| 3. Butir wajib yang belum ditandai menahan penutupan | `Kriteria3Dan4_HanyaButirWajibYangMenahanPenutupan` | Ditulis, **belum dijalankan** |
| 4. Butir tidak wajib yang belum ditandai **tidak** menahan | Test yang sama | Ditulis, **belum dijalankan** |
| 5. Butir yang dinonaktifkan tidak lagi menahan, dan penandaan lamanya tidak hilang | `Kriteria5_ButirYangDinonaktifkanTidakLagiMenahanDanPenandaanLamanyaTidakHilang` | Ditulis, **belum dijalankan** |

Kriteria 5 dikerjakan persis seperti diminta roadmap: butir dinonaktifkan **di tengah episode
berjalan**, lalu keduanya diperiksa — penandaan lama masih terbaca, dan penutupan tidak lagi
tertahan.

Satu test tambahan menjaga bahwa menandai butir yang sudah tidak aktif ditolak.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Dua endpoint baru |
| Database | Tidak ada perubahan schema |
| Keamanan | Butir `InpatientDischarge : Read/Update` dipakai apa adanya |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 6 |
| Butir obat pulang ditandai manual | Penandaan bergantung pada disiplin petugas, bukan pada catatan Farmasi | `DEC-INP-001`; kembali sebagai Amendment Pass |
| Daftar butir pada data sungguhan belum terbukti terisi | Penutupan episode tidak pernah tertahan apa pun karena daftarnya kosong | Seeder `BE-RWI-002` beserta validasinya |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Dua endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Pastikan seeder butir administrasi terbukti mengisi data sungguhan.
