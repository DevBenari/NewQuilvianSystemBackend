# Laporan Perubahan Backend — `BE-RWI-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-006` |
| Judul | Status terisi dan dipesan hanya lahir dari modul Rawat Inap |
| Slice | Sumber kebenaran penghunian tempat tidur |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-006` |
| Trace | `RWI-DEC-039`, `RWI-RULE-027` aturan 4 dan 5, `RWI-DEC-062`; api contract `0.4.0` bagian 7; validation matrix bagian 10; `EPIC RI-32`; `RWI-AC-060`, `RWI-AC-061` |
| Contract version | API `0.4.0`; perubahan perilaku, bukan perubahan bentuk kontrak |
| Dependency | `BE-RWI-004` selesai; `FE-RWI-001` **terbukti rilis** — lihat bagian 1 |
| Klasifikasi | `MEDIUM`, skor 8: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 2 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; badan aksi `/availability` pada `BedController`, test, dan dokumen tracked modul Rawat Inap |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `514b1d8232720eb450bc40f6deea6c6661160c8d` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | **Selesai.** Keempat acceptance criteria terbukti. Dikerjakan bersama `BE-RWI-032` sesuai `RWI-DEC-051` |

> **Menggantikan** [laporan blokir 25 Agustus 2026](be-rwi-006-terblokir-prasyarat-fe-rwi-001.md).
> Berkas ini adalah laporan task `BE-RWI-006` yang berlaku.

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Bounded context | `HealthServices / MasterData` — **modul milik pihak lain**, disentuh atas persetujuan `RWI-DEC-062` |
| Prefix ownership | `Mst` terdaftar dan `ACTIVE`; tidak ada entity, modul, maupun prefix baru |
| Applicability | `TOUCHED LEGACY`; hanya badan satu aksi yang disentuh |
| QBE berlaku | `QBE-MOD-001`, `QBE-API-001`, `QBE-PERM-001` |
| Archetype | Master data; aksi `PATCH /{id}/<aspek>` yang sudah ada, tidak ditambah maupun dihapus |
| Database authority | `NONE`; tidak ada perubahan schema, migration, maupun eksekusi database |
| Penyimpangan `QBE-SVC-001` | `BedController` memakai `ApplicationDbContext` langsung. **Tidak diperbaiki** — scope task ini hanya badan aksi `/availability`; memindahkannya ke service akan mengubah seluruh controller milik modul lain. Dicatat sebagai temuan, lihat bagian 7 |

---

## 1. Kenapa task ini boleh dikerjakan sekarang

Task ini terblokir sejak 25 Agustus 2026 dengan dua alasan, dan **keduanya sudah tidak berlaku**.

| Prasyarat | Keadaan 25 Agustus 2026 | Keadaan 1 September 2026 |
| --- | --- | --- |
| `FE-RWI-001` sudah rilis | Repository frontend tidak ada dalam workspace; tidak ada laporan; task tanpa tanda pada roadmap frontend | ✅ **Selesai.** Roadmap frontend baris `FE-RWI-001` bertanda selesai, dengan laporan `task/report/frontend/FE-RWI-001.md` |
| Persetujuan pemilik `MasterData` | Tiga dokumen kontrak menyatakan `RWI-OQ-033` "belum ada" | ✅ **Sudah diberikan** 21 Agustus 2026 lewat `RWI-DEC-062` |

**Catatan penting soal prasyarat kedua.** Ketiga dokumen itu — validation matrix bagian 10,
api contract bagian 7, dan integration contract bagian 2 — ternyata **basi**, bukan benar.
`RWI-DEC-062` berstatus `approved` atas nama Muhammad Hamzah sejak 21 Agustus 2026 dan
dinyatakan menutup `RWI-OQ-033`. Hal yang sama sudah tercatat benar pada
`02-backend-architecture.md` §7 dan `04-prd-to-mvp.md`. Ketiga dokumen yang basi itu
dibetulkan pada task ini, lihat bagian 3.2.

Kenapa `FE-RWI-001` penting: sebelum layar itu ada, endpoint `/availability` adalah
**satu-satunya** cara admin menutup tempat tidur rusak. Membatasi endpointnya lebih dulu akan
membuat tempat tidur rusak tidak dapat ditutup sama sekali.

---

## 2. Proses bisnis

**Masalah yang diperbaiki.** Kolom `MstBed.BedStatus` dapat disetel siapa pun yang punya hak
`Bed : Update` lewat menu master data — termasuk menjadi `Occupied` — tanpa menyebut pasiennya
siapa, sejak kapan, dan tanpa pemeriksaan tabrakan.

**Contoh kegagalan yang dicegah.** Admin master data menyetel bed Melati 3B menjadi `Occupied`
karena melihat ada pasien di sana. Sistem kini mengaku bed itu terisi, tetapi tidak punya
catatan penempatan. Akibatnya: papan tempat tidur menunjukkan bed penuh, pencarian tempat tidur
kosong melewatinya, census tidak memuat pasien itu, lama dirawatnya tidak terhitung, dan tidak
ada satu pun laporan yang dapat menyebut siapa yang berbaring di sana. Ketika pasien sungguhan
datang, petugas admisi tidak dapat menempatkannya di bed yang sebenarnya kosong.

**Aturan yang dikunci** — `RWI-RULE-027`:

| No | Aturan |
| ---: | --- |
| 1 | Catatan penempatan milik Rawat Inap adalah satu-satunya sumber kebenaran penghunian |
| 2 | `MstBed.BedStatus` turun kedudukan menjadi **salinan** catatan itu |
| 4 | `Reserved` dan `Occupied` **tidak boleh lagi** disetel manusia lewat endpoint master data |
| 5 | Yang tetap wewenang admin: `Cleaning`, `Maintenance`, `Blocked`, `Inactive` |

**Alur setelah perubahan.**

1. Admin membuka layar master tempat tidur dan memilih menutup satu tempat tidur.
2. Layar mengirim `PATCH /health-services/master-data/beds/{id}/availability`.
3. Tempat tidur tidak ditemukan → `404`, **sebelum** nilai diperiksa, supaya salah ketik id
   tidak tersamar sebagai penolakan aturan.
4. Nilai `Reserved` atau `Occupied` → `422` beserta kalimat yang menyebut jalan keluarnya.
5. Nilai `Unknown` → `422` "Status ketersediaan tempat tidur tidak dikenali."
6. Tempat tidur masih punya penempatan aktif → `422` yang menyebut pasien masih di sana.
7. Selain itu → tersimpan seperti semula, dengan bentuk balasan yang tidak berubah.

**Jalur normal admin tidak berubah.** Tempat tidur rusak tetap dapat ditutup lewat `Cleaning`,
`Maintenance`, `Blocked`, dan `Inactive`, lalu dibuka kembali lewat `Available`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa |
| --- | --- |
| `Areas/HealthServices/MasterData/Controllers/BedController.cs` | Badan aksi yang diubah, dan memastikan jalur lain tidak ikut tersentuh |
| `Areas/HealthServices/MasterData/Enums/BedStatus.cs` | Memastikan seluruh delapan nilai enum tertangani, termasuk `Unknown` |
| `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs` | Bentuk penempatan aktif: `BedId`, `EndDateTime`, `IsDelete` |
| `00-interview-decisions.md` | Menyelesaikan pertentangan status `RWI-OQ-033` |
| `roadmap/frontend-roadmap.md`, `task/report/frontend/FE-RWI-001.md` | Membuktikan prasyarat lintas repository |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Controllers/BedController.cs` | Tiga penjaga baru pada badan aksi `/availability`, ditambah `[ProducesResponseType]` 422 dan dokumentasi XML yang menjelaskan dasarnya |
| `QuilvianSystemBackend.Tests/InPatientManagement/BedAvailabilityRegressionTests.cs` | **Berkas baru**, memuat pembuktian task ini beserta test regresi `BE-RWI-032` |
| `contracts/validation-matrix.md` | Status `RWI-OQ-033` dibetulkan; dua aturan turunan didokumentasikan; `Available` dinyatakan tetap diizinkan |
| `contracts/api-contract.md` | Status persetujuan dibetulkan |
| `contracts/integration-contract.md` | Status persetujuan dibetulkan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Perubahan perilaku, bukan penambahan fitur.** Tidak ada endpoint baru, tidak ada endpoint dihapus, bentuk request dan response tidak berubah. Yang bertambah hanya kemungkinan jawaban `422` pada satu endpoint yang sudah ada |
| Database | `NOT APPLICABLE`. Tidak ada perubahan schema maupun migration |
| Keamanan/Auth | `NOT APPLICABLE`. Hak akses `Bed : Update` tidak berubah. Yang dibatasi adalah **nilai yang boleh dikirim**, bukan siapa yang boleh memanggil |

---

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Bed

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/availability` | Admin menutup tempat tidur rusak atau membukanya kembali. **Tidak lagi** dapat menyetel Terisi dan Dipesan | `Bed : Update` |

| Kode | Arti bagi pengguna |
| ---: | --- |
| `200` | Status ketersediaan tersimpan. Bentuk balasannya sama seperti sebelum perubahan |
| `404` | Tempat tidur tidak ditemukan |
| `422` | Nilai `Reserved`/`Occupied` ditolak; atau nilai tidak dikenali; atau tempat tidur masih ditempati pasien |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | Berhasil | `PASS` | `Build succeeded. 206 Warning(s), 0 Error(s)` |
| Seluruh suite `InPatientManagement` | 292 lulus dari 292 | `PASS` | `Failed: 0, Passed: 292` |
| Seluruh project test `QuilvianSystemBackend.Tests` | 879 lulus dari 879 | `PASS` | `Failed: 0, Passed: 879` |
| Kriteria 1 — `Reserved` dan `Occupied` ditolak 422 dengan pesan **persis** validation matrix | Lulus, 2 kasus | `PASS` | `MengirimTerisiAtauDipesan_DitolakDenganPesanDariValidationMatrix` |
| Kriteria 2 — `Cleaning`, `Maintenance`, `Blocked`, `Inactive` tetap diterima | Lulus | `PASS` | `LayarMasterTempatTidur_TetapBerfungsiUntukNilaiYangMasihDiizinkan` |
| Kriteria 3 — tempat tidur yang ditempati tidak dapat disetel `Maintenance` | Lulus | `PASS` | `TempatTidurYangSedangDitempati_TidakDapatDisetelMaintenance` |
| Kriteria 4 — bentuk balasan keempat nilai yang diizinkan tidak berubah | Lulus | `PASS` | Test yang sama memeriksa `ApiResponse<BedUpdateResponse>`, pesan, `Id`, `BedStatus`, dan `BedStatusName` |
| Penjaga tidak ditulis terlalu lebar — penempatan yang sudah berakhir tidak menahan apa pun | Lulus | `PASS` | `PenempatanYangSudahBerakhir_TidakMenahanPerubahanStatus` |
| Urutan pemeriksaan — id salah tetap dijawab `404`, bukan `422` | Lulus | `PASS` | `TempatTidurTidakDitemukan_TetapDijawab404SebelumPemeriksaanNilai` |

Uji manual: `NOT FEASIBLE` — menuntut aplikasi menyala terhadap database tim, di luar wewenang
task ini. Test otomatis di atas memanggil aksi controller yang sesungguhnya.

**Tidak dijalankan:** project `QuilvianSystemBackend.BillingTests` — menuntut
`QUILVIAN_BILLING_TEST_DB` dan di luar scope.

---

## 6. Delta terhadap kontrak

| Butir | Isi | Alasan |
| --- | --- | --- |
| `Available` tetap diizinkan | `RWI-RULE-027` aturan 5 menyebut empat nilai sebagai wewenang admin dan tidak menyebut `Available`. Aksi ini **tetap menerimanya** | Tanpa `Available`, tempat tidur yang ditutup admin untuk dibersihkan tidak akan pernah dapat dibuka kembali dari layar master — jalur pemulihannya hilang. Acceptance criteria task ini juga hanya menyebut `Reserved` dan `Occupied` sebagai yang ditolak. Didokumentasikan pada validation matrix bagian 10 |
| Dua aturan turunan | Penolakan `Unknown` dan penolakan saat ada penempatan aktif | Keduanya turunan langsung `RWI-RULE-027` aturan 2. Yang kedua adalah acceptance criteria 3 task ini. Ditambahkan ke validation matrix |

---

## 7. Risiko dan temuan yang tersisa

| Butir | Keadaan |
| --- | --- |
| **Pemesanan aktif tidak ikut menahan** | Penjaga hanya memeriksa **penempatan** aktif, sesuai bunyi acceptance criteria 3. Tempat tidur yang sedang dipesan tetapi belum ditempati masih dapat disetel `Maintenance` admin, dan pemesanannya tersingkir diam-diam. Dampaknya terbatas: pemesanan gugur sendiri setelah 2 jam, dan selisihnya tertangkap `GET /monitoring/bed-drift`. **Tidak diperluas sendiri** karena melebarkan scope task yang menyentuh modul milik pihak lain — dinaikkan sebagai butir keputusan |
| `BedController` memakai `ApplicationDbContext` langsung | Melanggar `QBE-SVC-001`. **Tidak diperbaiki**: memindahkannya ke service mengubah seluruh controller milik `MasterData`, jauh melampaui "hanya badan aksi `/availability`" |
| Aksi ini membaca `InpBedPlacement` dari controller `MasterData` | Arah baca lintas modul yang baru. Dipilih karena scope task melarang menambah service. Alternatifnya menambah dependency service Rawat Inap ke `BedController`, yang lebih besar dampaknya |

---

## 8. Task berikutnya

`BE-RWI-032` — dikerjakan bersama task ini, laporannya
[di sini](BE-RWI-032.md). Setelah itu `BE-RWI-033` sebagai penutup traceability modul.
