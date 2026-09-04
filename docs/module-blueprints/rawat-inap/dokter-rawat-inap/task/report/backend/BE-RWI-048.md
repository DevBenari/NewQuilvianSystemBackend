# Laporan Perubahan Backend — `BE-RWI-048`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-048` |
| Judul | Kunjungan dokter tercatat sebagai kejadian tersendiri |
| Slice | `DOK-MVP-4` — visite |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-048` |
| Trace | `EPIC DOK-05`; `FR-DOK-023` s.d. `FR-DOK-028`, `FR-DOK-039`; `RWI-AC-150` s.d. `RWI-AC-155`; `contracts/api-contract.md` §4; `INV-DOK-06`, `INV-DOK-07`; `VAL-DOK-08`, `VAL-DOK-16`, `VAL-DOK-17`, `VAL-DOK-27` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-041` 🟡 **sebagian** ([laporan](BE-RWI-041.md)) — tabel dan service visite sudah ada; yang tertunda hanya uji PostgreSQL. `BE-RWI-044` **selesai** ([laporan](BE-RWI-044.md)) |
| Klasifikasi | `HEAVY`, skor 12: repository 0, berkas diperiksa 2, berkas diubah 3, logika bisnis 3, kontrak API 3, database 0, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9be5526d248d9813a4044f063e43066a2364dd7d` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | 🟡 **Sebagian.** Ketujuh acceptance criteria terbukti pada SQLite. Butir DoD "test concurrency PostgreSQL hijau" **belum terpenuhi**: uji-nya sudah ada sejak `BE-RWI-041` dan tetap terhalang `BLOCKED_BY_TEST_DB_CONFIGURATION` |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` — `ACTIVE / LEGACY` |
| Applicability | `NEW CODE`. Controller, DTO, dan seluruh endpoint visite baru |
| QBE berlaku | `QBE-API-001`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-PAGE-001`, `QBE-LOG-001`, `QBE-CODE-002`, `QBE-DEL-001` |
| Entity operasional baru | `NONE`. `CliPhysicianVisit` sudah dibuat `BE-RWI-041`; task ini tidak menambah kolom |
| Archetype | **Transaksi**, arketipe aggregate ber-lifecycle ber-scope perawatan. Nol `GET /options`, nol `PATCH /{id}/status` generik, nol `DELETE /{id}` |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**Tidak ada satu pun tempat yang mencatat bahwa dokter benar-benar mendatangi pasien.**

Sebelum task ini, satu-satunya jejak kunjungan dokter adalah catatan yang ia tulis. Menghitung
visite dari catatan tampak masuk akal dan gratis, tetapi angkanya salah pada **dua arah
sekaligus**:

| Keadaan nyata | Yang terbaca bila dihitung dari catatan | Yang benar |
| --- | ---: | ---: |
| Dokter datang pukul 07.40, terpanggil ke ruangan lain sebelum sempat mengetik | 0 | **1** |
| Dokter datang sekali, menulis tiga catatan sepanjang kunjungan itu | 3 | **1** |
| Dokter datang pagi lalu dipanggil lagi sore karena pasien memburuk | tergantung jumlah catatan | **2** |

`INV-DOK-07` melarang cara itu, dan task ini menyediakan penggantinya: kejadian visite sebagai
fakta tersendiri, terpisah dari dokumen apa pun.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Kunjungan dokter tercatat sebagai kejadian yang berdiri sendiri, dapat dihitung, dan tidak dapat berganda karena tombol tertekan dua kali |
| Pelaku | Dokter yang mendatangi pasien |
| Pemicu | Dokter menekan Catat Visite setelah selesai memeriksa |
| Hasil akhir | Satu baris kejadian berisi perawatan, dokter, peran, **waktu kedatangan**, dan pencatatnya |

### 2.2 Langkah berurutan

1. Dokter membuka pasien rawat inap, lalu mencatat visite beserta **waktu kedatangannya** —
   bukan waktu ia mengetik.
2. Backend memastikan pengguna memang terhubung ke baris dokter. Bila tidak, permintaan ditolak
   `403` — `VAL-DOK-08`.
3. Backend membentuk konteks perawatan: pasien, perawatan, kelayakan status, dan kewenangan
   dokter atas pasien itu.
4. Kunci permintaan diperiksa. Bila kunci yang sama sudah tersimpan, **kejadian yang sudah ada**
   dikembalikan dengan kode `200` — bukan kejadian kedua, dan bukan `409`.
5. Kejadian tersimpan beserta nomor bisnis yang dialokasikan penyedia seri nomor.

### 2.3 Kenapa kiriman ulang dijawab `200`, bukan `409`

Bagi dokter yang jaringannya terputus lalu menekan Simpan sekali lagi, hasilnya memang berhasil —
kejadiannya sudah tersimpan. Menjawab `409` akan membuatnya mengira pencatatannya gagal, lalu ia
mencatat ulang dengan kunci baru, dan **lahirlah kunjungan kedua yang tidak pernah terjadi**.

### 2.4 Hitungan, dengan angka

Mengikuti `state-transition-matrix.md` bagian 5.3:

| Keadaan | Baris tersimpan | Hitungan |
| --- | ---: | ---: |
| dr. Andi visite pukul 07.40 lalu kembali pukul 16.10 | 2 | **2** |
| Tombol Simpan tertekan dua kali dengan kunci sama | 1 | **1** |
| Tiga catatan ditulis tanpa satu pun kejadian visite | 0 | **0** |

### 2.5 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Pengguna tidak terhubung ke dokter mana pun | "Visite hanya dapat dicatat dokter." | `403` |
| Kunci permintaan kosong | "Kunci permintaan wajib diisi." | `400` |
| Waktu visite melewati waktu sekarang | "Waktu visite tidak boleh melewati waktu sekarang." | `400` |
| Dokter tidak berwenang atas pasien itu | Ditolak | `403` |
| Perawatan sudah ditutup | Ditolak | `422` |
| Kunci permintaan sama dengan yang sudah tersimpan | **Bukan galat.** Kejadian yang sama dikembalikan | `200` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan DoD |
| `contracts/api-contract.md` §4 | Bentuk endpoint, kode status, dan perilaku yang mengikat |
| `contracts/state-transition-matrix.md` §5 | Mesin status kejadian dan tabel hitungan |
| `contracts/validation-matrix.md` §4 | `VAL-DOK-16`, `VAL-DOK-17`, `VAL-DOK-27` |
| `contracts/permission-audit-matrix.md` §1.1, §2, §5 | Butir hak akses baru dan kolom sensitif |
| `rules/backend/transaction-endpoint-standard.md` | Arketipe transaksi dan aturan verb |
| `Areas/HealthServices/ClinicalManagement/Models/CliPhysicianVisit.cs` | Bentuk data dari `BE-RWI-041` |
| `Areas/HealthServices/ClinicalManagement/Services/PhysicianVisitService.cs` | Perintah yang sudah tersedia |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Pola pemetaan pengguna ke baris dokter |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PhysicianVisitController.cs` | **Baru.** Delapan endpoint: metadata filter, ringkasan, daftar, riwayat per perawatan, detail, pencatatan, pembatalan, dan penautan dokumen |
| `Areas/HealthServices/ClinicalManagement/DTOs/PhysicianVisitDtos.cs` | **Baru.** Permintaan pencatatan, pembatalan, penautan; balasan detail, baris riwayat, ringkasan, dan metadata filter |
| `Areas/HealthServices/ClinicalManagement/Services/PhysicianVisitService.cs` | Penolakan waktu di masa depan; penautan dokumen; query riwayat bersama; hitungan kejadian batal; pembacaan satu kejadian |
| `Tests/.../ClinicalManagement/PhysicianVisitRecordingTests.cs` | **Baru.** Sepuluh uji |
| `Tests/.../ClinicalManagement/InpatientClinicalSchemaTests.cs` | Waktu uji `DuaVisitePadaTanggalSama_MenghasilkanDuaBaris` digeser ke hari sebelumnya — lihat bagian 7 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Grup endpoint baru** `Health Services / Clinical Management / Physician Visit`. Lima endpoint sesuai `api-contract.md` §4, ditambah tiga endpoint baca baseline transaksi — dicatat sebagai delta kontrak pada bagian 7 |
| Database | Nol perubahan schema, nol migration, nol eksekusi database. Tabel `CliPhysicianVisit` sudah dibuat `BE-RWI-041` |
| Keamanan/Auth | Resource baru `PhysicianVisit` dengan Action `Read`, `Create`, `Update`, dan `Cancel`. Seluruhnya memakai nama yang **sama persis** pada `[AccessAction]` dan `[AccessPermission]` — pelajaran `BE-RWI-034`. Butirnya terdaftar otomatis lewat pemindaian atribut pada `AccessMenuSeeder` |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Physician Visit

Base URL: `api/v1/health-services/clinical-management/physician-visits`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan penyaring, peran, dan keadaan kejadian untuk layar riwayat visite | `PhysicianVisit : Read` |
| `GET` | `/summary` | **Hitungan visite** — berlaku, batal, jumlah dokter berbeda, dan waktu kedatangan terakhir | `PhysicianVisit : Read` |
| `GET` | `/` | Daftar kejadian dengan penyaring perawatan, kunjungan, dokter, dan rentang waktu | `PhysicianVisit : Read` |
| `GET` | `/episodes/{episodeId}` | Riwayat visite satu perawatan, terurut waktu kedatangan; kejadian batal ikut tampil beserta alasannya | `PhysicianVisit : Read` |
| `GET` | `/{id}` | Satu kejadian beserta tautan dokumennya dan aksi yang sedang boleh dijalankan | `PhysicianVisit : Read` |
| `POST` | `/` | Mencatat kejadian visite. Kunci permintaan wajib, boleh lewat badan permintaan atau header `Idempotency-Key` | `PhysicianVisit : Create` |
| `PATCH` | `/{id}/cancel` | Membatalkan kejadian yang salah catat beserta alasannya — `BE-RWI-049` | `PhysicianVisit : Cancel` |
| `PATCH` | `/{id}/links` | Menautkan catatan dokter, catatan terpadu, atau tindakan pada kejadian | `PhysicianVisit : Update` |

**Kode status pencatatan:** `201` kejadian tercatat; `200` kiriman ulang berkunci sama; `400`
waktu di masa depan atau kunci kosong; `403` bukan dokter, atau dokter tidak berwenang atas
pasien itu; `422` perawatan belum dimulai atau sudah ditutup.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)`, `185 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet test` project uji SQLite | `Failed: 0, Passed: 320` | `PASS` | Keluaran perintah |
| Visite tanpa satu pun catatan tetap muncul pada riwayat | Satu baris; perawatan, dokter, peran `DPJP`, waktu kedatangan, pencatat terisi; nol catatan dokter tercipta | `PASS` | `VisiteTanpaSatuPunCatatan_TetapMunculPadaRiwayatBesertaIsinya` |
| Waktu kedatangan tidak bergeser saat catatan menyusul | Waktu tersimpan tetap waktu kedatangan; tautan catatan tersimpan | `PASS` | `VisitePukulLebihAwal_TetapTerbacaSaatCatatanMenyusulKemudian` |
| Tiga catatan tanpa kejadian visite → hitungan nol | `RecordedCount` = 0, `TotalCount` = 0, waktu terakhir kosong, sementara catatan dokter berjumlah 3 | `PASS` | `TigaCatatanTanpaKejadianVisite_MenghasilkanHitunganNol` |
| Dua visite nyata pada tanggal sama → dua baris, hitungan dua | Dua `201`; dua baris; `RecordedCount` = 2; `DistinctDoctorCount` = 1 | `PASS` | `DuaVisiteNyataPadaTanggalYangSama_MenghasilkanDuaBarisDanHitunganDua` |
| Dua pengiriman berkunci sama → satu kejadian, `200` pada yang kedua | `201` lalu `200`; identitas kejadian identik; satu baris | `PASS` | `DuaPengirimanBerkunciSama_MenghasilkanSatuKejadianDanKode200` |
| Kunci permintaan kosong ditolak | `400`, pesan memuat "Kunci permintaan"; nol baris | `PASS` | `KunciPermintaanKosong_Ditolak400` |
| Perawat ditolak | `403`, pesan persis "Visite hanya dapat dicatat dokter."; nol baris | `PASS` | `PerawatMencatatVisite_Ditolak403` |
| Waktu visite di masa depan ditolak | `400`, pesan memuat "melewati waktu sekarang" | `PASS` | `WaktuVisiteDiMasaDepan_Ditolak400` |
| Perawatan tertutup menolak kejadian baru | `422` | `PASS` | `PerawatanTertutup_MenolakKejadianVisiteBaru422` |
| Dokter tanpa penugasan ditolak | `403` | `PASS` | `DokterTanpaPenugasanPadaPerawatan_Ditolak403` |
| **Dua permintaan bersamaan berkunci sama terhadap PostgreSQL** | **Belum dijalankan** | `NOT RUN` | `PhysicianVisitUniquenessTests.KunciPermintaanKembar_DitolakDatabase` ada sejak `BE-RWI-041` dan berhenti pada penjagaannya sendiri: `BLOCKED_BY_TEST_DB_CONFIGURATION` — `QUILVIAN_BILLING_TEST_DB` belum diisi |
| `dotnet test` project uji InMemory | `Failed: 1, Passed: 908` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, berkas tidak disentuh task ini |
| `dotnet test` project uji PostgreSQL | `Failed: 54, Passed: 34` | `EXISTING / ENVIRONMENT ISSUE` | Satu sebab: `BLOCKED_BY_TEST_DB_CONFIGURATION` |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** uji concurrency PostgreSQL, karena tidak ada database uji yang tersedia.
Aturan repository melarang mengarahkan uji integrasi ke database dev bersama.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Visite pukul 07.40 muncul pada riwayat walaupun catatannya menyusul **atau tidak ditulis sama sekali** | Terpenuhi | `VisiteTanpaSatuPunCatatan_...` dan `VisitePukulLebihAwal_...` |
| 2. Tiga catatan tanpa satu pun kejadian visite menghasilkan hitungan **nol** | Terpenuhi | `TigaCatatanTanpaKejadianVisite_MenghasilkanHitunganNol` |
| 3. Dua pengiriman berkunci sama menghasilkan **satu** kejadian dengan identitas sama, kode `200` pada yang kedua | Terpenuhi | `DuaPengirimanBerkunciSama_MenghasilkanSatuKejadianDanKode200` |
| 4. Dua visite nyata pada tanggal yang sama menghasilkan **dua** baris dan hitungan **dua** | Terpenuhi | `DuaVisiteNyataPadaTanggalYangSama_...` |
| 5. Kunci permintaan kosong ditolak `400` | Terpenuhi | `KunciPermintaanKosong_Ditolak400` |
| 6. Perawat ditolak `403` | Terpenuhi | `PerawatMencatatVisite_Ditolak403` |
| 7. Riwayat menampilkan perawatan, dokter, peran, waktu, pencatat, dan tautan bila ada | Terpenuhi | `VisiteTanpaSatuPunCatatan_TetapMunculPadaRiwayatBesertaIsinya` |

### Definition of Done

| Butir | Status |
| --- | --- |
| Ketujuh acceptance criteria terbukti | ✅ |
| **Test concurrency PostgreSQL hijau** | ⛔ **Belum.** Uji-nya ada dan terkompilasi; lingkungannya tidak tersedia |
| Laporan mencantumkan hitungan pada keenam keadaan matriks status §5.3 | ✅ Bagian 2.4 dan tabel verifikasi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan build baru |
| **Delta kontrak yang dilaporkan** | `api-contract.md` §4 mencantumkan lima endpoint. Yang dibuat delapan: ditambahkan `GET /filters/metadata`, `GET /summary`, dan `GET /` sesuai permukaan baca baseline transaksi pada `rules/backend/transaction-endpoint-standard.md` bagian 4. `GET /summary` bukan sekadar kelengkapan — ia satu-satunya tempat hitungan visite dijawab, dan tanpanya kriteria 2 dan 4 tidak punya permukaan yang dapat dipanggil layar |
| **Selisih terhadap standar endpoint transaksi** | Standar bagian 3 menyatakan perpindahan status memakai `POST /{id}/<aksi>`. Yang dipakai adalah `PATCH /{id}/cancel`, mengikuti dua hal yang lebih tinggi presedensinya: `api-contract.md` §4 yang sudah disetujui, dan konvensi tetangga terdekatnya — `DoctorConsultation`, `PatientAssessment`, dan `PatientProcedure` semuanya memakai `PATCH /{id}/cancel`. Standar itu sendiri mencatat pemakaian ini sebagai drift yang ada di source. Dilaporkan, tidak diseragamkan sepihak |
| **Perubahan pada uji yang sudah ada** | `InpatientClinicalSchemaTests.DuaVisitePadaTanggalSama_MenghasilkanDuaBaris` memakai pukul 07.00 dan 16.00 **hari ini**. Sejak `VAL-DOK-16` ditegakkan, pukul 16.00 hari ini adalah masa depan bila uji berjalan pagi, sehingga permintaannya ditolak. Waktu ujinya digeser ke hari sebelumnya; yang dibuktikan tetap sama, yaitu dua visite pada **tanggal yang sama** menghasilkan dua baris |
| Masalah yang diketahui | Kebijakan pencatatan visite **atas nama** dokter lain belum ada — gerbang terbuka pada roadmap bagian 5. Bawaan yang aman dipilih: seorang dokter hanya mencatat visite miliknya sendiri, dan percobaan mencatat atas nama dokter lain ditolak `403` |
| Risiko tersisa | Pencegahan kejadian ganda pada dua permintaan yang tiba **benar-benar bersamaan** bersandar pada unique index basis data, dan index itu belum pernah diuji terhadap PostgreSQL sungguhan. Pemeriksaan di dalam aplikasi menangani kiriman ulang biasa, tetapi tidak dapat mencegah perlombaan |
| Toleransi jam maju | Penolakan waktu di masa depan memberi toleransi dua menit, karena jam perangkat pencatat dan jam server tidak selalu sama persis. Tanpa toleransi, dokter yang mencatat "sekarang" dari perangkat yang jamnya lebih cepat beberapa detik akan ditolak tanpa mengerti sebabnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Bersih sebelum task; tidak ada stage, commit, maupun push |
| Langkah berikutnya | Menyediakan `QUILVIAN_BILLING_TEST_DB` yang menunjuk database uji tersendiri, lalu menjalankan `PhysicianVisitUniquenessTests` untuk menaikkan task ini menjadi ✅ |
