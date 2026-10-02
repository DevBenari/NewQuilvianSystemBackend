# Laporan Perubahan Backend — `BE-RWI-060`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-060` |
| Judul | Perubahan rencana asuhan menyimpan versi sebelumnya |
| Slice | Gelombang `KEP-MVP-2` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-014`, `FR-KEP-016`, `FR-KEP-017`; PRD `CAP-013` aturan 5 dan 6; `AC-CAP013-02`, `AC-CAP013-03`; `RWI-AC-177`; `INV-KEP-02` |
| Contract version | API `0.3.0` `PUT /items/{itemId}`, `PATCH /items/{itemId}/close`, `GET /items/{itemId}/revisions`; state transition `0.3.0` bagian 2 |
| Dependency | `BE-RWI-059` — ✅ selesai, lihat [laporannya](./BE-RWI-059.md) |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah > 3 (1), logika bisnis sedang (1), memakai kontrak yang sudah ada (0), entity dan migration baru (1), keamanan memakai resource yang sudah ada (0), workflow terbatas (1). Total **4** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `6f7d81e0` pada branch `MHamzah` |
| Tanggal | 7 September 2026 |
| Status | **Selesai.** Keempat acceptance criteria terbukti. Migration `20260906144956_AddNursingCarePlanItemRevision` dibuat dan **belum diterapkan ke database mana pun** |

---

## 1. Masalah yang diperbaiki

`BE-RWI-059` membuat rencana asuhan dapat disimpan, tetapi belum membuatnya dapat **berubah tanpa
kehilangan jejak**. Rencana asuhan memang berubah — itu sifatnya, bukan kesalahannya. Pasien
membaik, tujuannya bergeser, rencananya disesuaikan.

Tanpa mesin versi, perubahan itu menimpa isi lama. Akibatnya rekam medis kehilangan dua hal
sekaligus: **apa** yang dulu dinilai, dan **siapa** yang menilainya.

**Contoh yang menjelaskan seluruh task ini.** Butir "risiko jatuh tinggi" ditulis Ns. Sari pukul
08.00. Pukul 15.00 Ns. Dewi memperbaruinya menjadi "risiko jatuh sedang" karena pasien sudah mampu
berpindah dengan pendampingan. Yang salah: versi lama tersalin atas nama Ns. Dewi pukul 15.00 —
rekam medis lalu tidak dapat menunjukkan siapa yang menilai pertama kali. Yang benar: versi lama
**tetap** tercatat atas nama **Ns. Sari pukul 08.00**. `AC-CAP013-02` menguji tepat hal itu, dan
kesalahannya **tidak terlihat sama sekali dari layar**.

---

## 2. Proses bisnis

**Tujuan.** Riwayat asuhan pasien tetap utuh. Menutup satu masalah keperawatan tidak menghapus
jejak tindakan dan evaluasi yang sudah dikerjakan.

**Pelaku.** Perawat penanggung jawab dan kepala ruangan.

**Pemicu.** Keadaan pasien berubah, sehingga tujuan atau rencana tindakannya perlu disesuaikan.

**Langkah yang berurutan.**

1. Perawat membuka butir masalah yang hendak diperbarui.
2. Perawat mengirim isi versi baru: masalah, tujuan, dan rencana tindakan.
3. Sistem **menyalin keadaan butir yang sedang berlaku** ke riwayat versi lebih dulu — beserta
   penulis dan waktu versi itu, bukan penulis yang mengubah.
4. Barulah butirnya diperbarui: isinya diganti, nomor versinya naik satu, dan penulis versi
   berlaku menjadi perawat yang mengubah.
5. Penyalinan dan pembaruan berada pada **satu** penyimpanan, sehingga tidak pernah ada keadaan di
   mana butir sudah berubah tetapi versi lamanya gagal tersimpan.
6. Riwayat versinya dibaca kapan saja, termasuk setelah pasien pulang.

**Aturan yang berlaku.**

| Aturan | Isinya |
| --- | --- |
| Perubahan rencana asuhan adalah **perkembangan klinis** | Ia memakai mesin **versi**, bukan mesin **addendum**. `RWI-DEC-091` secara tegas tidak menyeret rencana asuhan ke mesin koreksi, karena menyamakan keduanya akan mengaburkan perbedaan antara *pasien membaik* dan *perawat salah tulis* |
| Versi lama menyimpan penulis dan waktu aslinya | `AC-CAP013-02`. Ini kriteria yang paling mudah salah |
| Satu butir tidak pernah punya dua baris untuk versi yang sama | Dijaga unique `(CarePlanItemId, VersionNumber)` pada database, bukan hanya oleh pemeriksaan aplikasi |
| Menutup butir tidak menghapus apa pun | `CAP-013` aturan 6. Evaluasi dan riwayat versinya tetap ada |
| Setelah perawatan ditutup, seluruh riwayat tetap terbaca | `AC-CAP013-03`, `INV-KEP-02`. Yang ditolak hanyalah perubahan |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Butir sudah ditutup | `409` — isinya tidak dapat diperbarui lagi |
| Perawatan sudah `Closed` atau `Cancelled` | `422` pada setiap penulisan; `GET` tetap `200` |
| Akun belum tertaut ke data pegawai | `400` — versi baru wajib menyebut siapa penulisnya |
| Evaluasi kedua pada butir yang sudah pernah dievaluasi | Keadaan lama **diarsipkan lebih dulu**, sehingga evaluasi pertama tidak hilang |

**Hasil akhir.** Satu butir masalah memiliki rantai versi berurutan mulai dari versi 1, masing-
masing menyebut penulis dan waktunya sendiri, dan tidak satu pun isi lama yang pernah hilang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` kartu `BE-RWI-060` | Scope, acceptance criteria, dan peringatan risikonya |
| `contracts/state-transition-matrix.md` `0.3.0` bagian 0 dan 2 | Kenapa mesin butir asuhan **tidak** ikut berubah pada `0.3.0` |
| `contracts/api-contract.md` `0.3.0` bagian 2 | Bentuk `PUT /items/{itemId}` dan `GET /items/{itemId}/revisions` |
| `data/data-dictionary.md` bagian 5 | Bentuk kolom tabel revisi, termasuk dua kolom penulis aslinya |
| `Areas/HealthServices/ClinicalManagement/Services/NursingCarePlanService.cs` | Titik sisip penyalinan versi |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalNoteAddendumService.cs` | Memastikan slice ini **tidak** memakainya — pembeda versi dan addendum |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/CliNursingCarePlanItemRevision.cs` | **Baru.** Salinan satu versi butir beserta penulis dan waktu aslinya |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliNursingCarePlanItemRevisionConfiguration.cs` | **Baru.** Unique `(CarePlanItemId, VersionNumber)`; penulis asli memakai `Restrict` |
| `Areas/HealthServices/ClinicalManagement/DTOs/NursingCarePlanDtos.cs` | `UpdateCarePlanItemRequest` dan `CarePlanItemRevisionResponse` |
| `Areas/HealthServices/ClinicalManagement/Services/NursingCarePlanService.cs` | `UpdateItemAsync`, `GetRevisionsAsync`, penolong `ArsipkanVersiSaatIni`; `EvaluateItemAsync` ikut mengarsipkan evaluasi sebelumnya |
| `Areas/HealthServices/ClinicalManagement/Controllers/NursingCarePlanController.cs` | Dua endpoint baru: `PUT /items/{itemId}` dan `GET /items/{itemId}/revisions` |
| `Repositories/ApplicationDbContext.cs` | Satu `DbSet` baru |
| `Migrations/20260906144956_AddNursingCarePlanItemRevision.cs` | **Baru.** Satu tabel, dua index, nol perubahan pada tabel lain |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingCarePlanRevisionTests.cs` | **Baru.** Enam uji acceptance |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Bertambah.** Dua endpoint baru sesuai `api-contract.md` `0.3.0`. Endpoint `PATCH /items/{itemId}/close` sudah mendarat pada `BE-RWI-059` dan perilakunya diuji ulang di sini |
| Database | **Satu tabel baru** `public."CliNursingCarePlanItemRevision"`. Migration `20260906144956_AddNursingCarePlanItemRevision` dibuat dan **belum diterapkan ke database mana pun** |
| Keamanan/Auth | **Nol resource baru.** Kedua endpoint memakai `NursingCarePlan : Update` dan `NursingCarePlan : Read` yang sudah lahir pada `BE-RWI-059` |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Nursing Care Plan

Base URL: `api/v1/health-services/clinical-management/nursing-care-plans`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/items/{itemId}` | Memperbarui butir; versi sebelumnya **tersalin**, bukan ditimpa | `NursingCarePlan : Update` |
| `GET` | `/items/{itemId}/revisions` | Riwayat versi satu butir, terurut dari versi pertama | `NursingCarePlan : Read` |
| `PATCH` | `/items/{itemId}/close` | Menutup masalah keperawatan tanpa menghapus jejaknya — mendarat pada `BE-RWI-059`, perilakunya diuji ulang di sini | `NursingCarePlan : Update` |

### Kode status dan artinya bagi pengguna

| Kode | Artinya |
| --- | --- |
| `200` | Butir diperbarui, atau riwayat versinya terbaca |
| `400` | Masalah keperawatan kosong, atau akun belum tertaut ke data pegawai |
| `404` | Butir rencana asuhan tidak ditemukan |
| `409` | Butir sudah ditutup, sehingga isinya tidak dapat diperbarui |
| `422` | Perawatan pasien sudah ditutup; catatannya hanya dapat dibaca |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | `Build succeeded`, `213 Warning(s)`, `0 Error(s)` | `PASS` | Jumlah warning sama persis dengan garis dasar `6f7d81e` |
| `dotnet test` project SQLite, tapis `NursingCarePlan` | `Failed: 0, Passed: 18, Total: 18` | `PASS` | Mencakup 12 uji `BE-RWI-059` dan 6 uji task ini |
| `dotnet test` project SQLite, seluruhnya | `Failed: 0, Passed: 448, Total: 448` | `PASS` | Garis dasar sebelum slice ini 394 |
| `dotnet test` project InMemory, seluruhnya | `Failed: 9, Passed: 917, Total: 926` | `EXISTING / ENVIRONMENT ISSUE` | Kesembilan kegagalan berada di `BillingManagement`; worktree pada commit `6f7d81e` tanpa perubahan slice ini menghasilkan angka yang sama persis |
| **Pemeriksaan penulis pada versi lama** — kriteria yang paling mudah salah | Versi 1 tercatat atas nama pegawai Ns. Sari beserta waktu aslinya; **bukan** pegawai Ns. Dewi yang mengubah | `PASS` | `VersiLama_MenyimpanPenulisAsli_BukanPengubahnya` |
| Skenario perubahan butir berulang | Dua kali pembaruan menghasilkan riwayat versi 1 dan 2 berurutan, isi lama utuh | `PASS` | `PembaruanBerulang_MenghasilkanRiwayatVersiBerurutan` |
| Pemeriksaan bahwa tidak ada baris addendum yang terbentuk | Satu baris revisi, **nol** baris `MrcClinicalNoteAddendum` | `PASS` | `PembaruanButir_TidakMembentukSatuPunAddendum` |
| Percobaan mengubah asuhan pada perawatan tertutup | `GET` tetap `200`; `POST /`, `POST /{id}/items`, `PUT /items/{itemId}`, dan `PATCH /evaluate` seluruhnya `422` | `PASS` | `PerawatanTertutup_TetapTerbacaTetapiMenolakPenulisan` |
| Menutup butir tidak menghapus evaluasi dan riwayat versi | Evaluasi terakhir utuh, satu baris revisi tetap ada | `PASS` | `MenutupButir_TidakMenghapusEvaluasiDanRiwayatVersi` |
| Evaluasi kedua tidak menimpa evaluasi pertama | Evaluasi pertama terarsip pada revisi; evaluasi kedua menjadi yang berlaku | `PASS` | `EvaluasiKedua_TidakMenimpaEvaluasiPertama` |
| Bagian `AC 3` tentang **tindakan** yang merujuk butir | Terbukti pada `BE-RWI-061` | `PASS` | `NursingInterventionTests.Tindakan_MenolakButirRencanaMilikPerawatanLain` beserta perilaku `SetNull` pada `CliNursingInterventionConfiguration`; lihat bagian 6 |

Uji manual: `NOT FEASIBLE` — alasannya sama dengan `BE-RWI-059`.

**Tidak dijalankan:**

- **Eksekusi migration ke database mana pun.** Wewenangnya terpisah dan tidak diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Memperbarui butir menyimpan versi sebelumnya **beserta penulis dan waktu aslinya**, bukan penulis yang mengubah | Terpenuhi | Kolom `OriginalAuthorEmployeeId` dan `OriginalAuthoredAt` diisi dari `AuthoredByEmployeeId`/`AuthoredAt` butir **sebelum** keduanya diganti; uji `VersiLama_MenyimpanPenulisAsli_BukanPengubahnya` memeriksa keduanya sekaligus memastikan nilainya **berbeda** dari pengubahnya |
| 2. Memperbarui butir menghasilkan **versi baru**, bukan addendum (`RWI-AC-177`) | Terpenuhi | `NursingCarePlanService.UpdateItemAsync` tidak menyentuh `ClinicalNoteAddendumService` sama sekali; uji `PembaruanButir_TidakMembentukSatuPunAddendum` memeriksa jumlah baris addendum tetap nol |
| 3. Menutup butir tidak menghapus tindakan maupun evaluasi sebelumnya; tindakan yang merujuk butir itu tetap ada dan rujukannya menjadi kosong, barisnya tidak hilang | Terpenuhi | Bagian evaluasi dan riwayat versi: uji `MenutupButir_TidakMenghapusEvaluasiDanRiwayatVersi`. Bagian tindakan: `CliNursingInterventionConfiguration` memakai `DeleteBehavior.SetNull` pada `CarePlanItemId` — barisnya tetap hidup dan hanya rujukannya yang lepas; penutupan butir sendiri **tidak** menghapus baris butirnya, sehingga rujukan tindakan bahkan tetap utuh |
| 4. Setelah episode `Closed`, seluruh riwayat asuhan tetap terbaca lewat `GET` dan setiap `POST`/`PUT` dijawab `422` (`AC-CAP013-03`) | Terpenuhi | `EnsureEpisodeAdmittedAsync` hanya dipanggil jalur penulisan; uji `PerawatanTertutup_TetapTerbacaTetapiMenolakPenulisan` memeriksa dua `GET` berhasil dan empat jalur tulis ditolak `422` |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Satu entity | Terpenuhi — `CliNursingCarePlanItemRevision` |
| Satu configuration | Terpenuhi |
| Satu `DbSet` | Terpenuhi |
| Satu migration | Terpenuhi — `20260906144956_AddNursingCarePlanItemRevision`, **belum diterapkan** |
| Tiga endpoint | **Terpenuhi dengan catatan** — dua lahir di task ini (`PUT /items/{itemId}`, `GET /items/{itemId}/revisions`); yang ketiga (`PATCH /items/{itemId}/close`) sudah mendarat pada `BE-RWI-059` karena acceptance criteria 3 task itu menuntutnya, dan perilakunya diuji ulang di sini |
| Penjaga episode tertutup | Terpenuhi |
| Keempat acceptance criteria terbukti | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Clinical` — prefix **`Cli`**, lifecycle `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` untuk entity, configuration, dan endpoint baru; `TOUCHED LEGACY` aditif pada `ApplicationDbContext.cs` |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-CODE-001` s.d. `QBE-CODE-006` — tabel revisi tidak memiliki nomor bisnis. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION` |
| `QBE-TXN-001` | **Ditegakkan.** Penyalinan versi dan pembaruan butir berada pada satu `SaveChanges`, sehingga tidak pernah ada butir yang sudah berubah tetapi versi lamanya gagal tersimpan |

### Delta kontrak yang dilaporkan ke pemilik kontrak

| Delta | Isinya | Kenapa |
| --- | --- | --- |
| **`PATCH /items/{itemId}/close` mendarat pada `BE-RWI-059`** | Kartu `BE-RWI-060` mencantumkannya sebagai salah satu dari tiga endpointnya | Acceptance criteria 3 `BE-RWI-059` menuntut penolakan penutupan tanpa evaluasi, dan itu tidak dapat dibuktikan tanpa endpoint penutupannya. Jumlah endpoint kontrak grup ini tidak berubah — hanya slice yang membuatnya |
| **Evaluasi juga mengarsipkan versi** | Mesin status bagian 2 hanya menuntut penyalinan pada baris "Memperbarui tujuan atau rencana" | Tabel revisi menyediakan kolom `EvaluationNote`, yang hanya bermakna bila evaluasi memang ikut terarsip. Tanpa ini, evaluasi kedua menimpa evaluasi pertama dan penilaian klinis yang pernah dibuat hilang. Penguatan ini bersifat aditif dan tidak mengubah satu pun aturan yang tertulis |
| **`UpdateCarePlanItemRequest` tidak memuat `SourceAssessmentId`** | Kontrak tidak merincinya | Pengkajian asal adalah fakta kelahiran butir, bukan isi yang berkembang. Mengizinkannya berubah akan membuat rujukan asal dapat dipindah tanpa jejak |

### Ringkasan lain

| Hal | Isi |
| --- | --- |
| Peringatan | Jumlah warning solusi tetap `213`, sama persis dengan garis dasar |
| Masalah yang diketahui | Nomor versi dinaikkan di dalam aplikasi. Dua permintaan pembaruan yang tiba benar-benar bersamaan ditahan unique `(CarePlanItemId, VersionNumber)` pada database, dan yang kalah menerima galat penyimpanan — bukan versi kembar. Perilaku itu belum diuji terhadap PostgreSQL sungguhan karena lingkungan ujinya belum tersedia; lihat `BE-RWI-061` bagian 7 |
| Risiko tersisa | Migration belum pernah diterapkan ke database mana pun |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 40 entri; nol berkas milik modul lain tersentuh |
| Langkah berikutnya | Gelombang `KEP-MVP-2` selesai. Berikutnya `KEP-MVP-3` — `BE-RWI-061`, `BE-RWI-062`, dan `BE-RWI-063` |
