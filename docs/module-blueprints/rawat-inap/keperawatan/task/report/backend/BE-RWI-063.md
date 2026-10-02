# Laporan Perubahan Backend — `BE-RWI-063`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-063` |
| Judul | Catatan keperawatan tampil pada catatan terpadu tanpa tabel baru |
| Slice | Gelombang `KEP-MVP-3` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-023`; PRD `CAP-014` aturan 4; `INT-KEP-03`; `RWI-RULE-026` |
| Contract version | Integration `0.3.0` `INT-KEP-03`; API `0.3.0` grup Patient Integrated Progress Note |
| Dependency | `BE-RWI-061` — 🟡 sebagian. Butir yang tertunda di sana (uji PostgreSQL idempotency) tidak menyentuh satu pun kriteria task ini |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah > 3 (1), logika bisnis ringan (0), memakai kontrak yang sudah ada (1), **nol** entity dan **nol** migration (0), keamanan tidak berubah (0), menyentuh perilaku lintas profesi (1). Total **3** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `6f7d81e0` pada branch `MHamzah` |
| Tanggal | 7 September 2026 |
| Status | **Selesai.** Keempat acceptance criteria terbukti. **Nol tabel baru, nol kolom baru; migration task ini kosong** |

---

## 1. Masalah yang diperbaiki

Kartu task menyatakan tabel catatan terpadu **sudah ada**, sudah menerima banyak profesi, dan
seluruh kolom penghubungnya sudah nullable. Semuanya benar. Yang ditemukan saat implementasi adalah
gap yang satu tingkat lebih dalam:

> **Kolom `InpEpisodeId` pada `TrxPatientIntegratedProgressNote` tidak pernah diisi siapa pun.**

Pencarian di seluruh `Areas/` menemukan nol tempat yang menulis kolom itu. Kolomnya lahir pada
`BE-RWI-040`, dan endpoint pembacanya lahir pada `BE-RWI-053`:

```text
GET /api/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{episodeId}
```

Endpoint itu menyaring `x.InpEpisodeId == episodeId`. Karena tidak ada satu baris pun yang pernah
mengisi kolom tersebut, **lini masa catatan terpadu satu perawatan selalu kosong** — dan bukan hanya
untuk catatan perawat, melainkan juga untuk catatan dokter yang sudah berjalan selama ini.

**Akibatnya bagi pekerjaan nyata.** Dokter membuka lembar catatan terpadu Tn. Budi untuk membaca
perkembangan pasiennya dan menemukan halaman kosong, padahal catatan perawat dan catatan dokter
sama-sama sudah tersimpan. Keduanya hanya dapat ditemukan lewat pencarian per kunjungan, bukan lewat
lembar terpadu yang justru dibuat untuk itu.

---

## 2. Proses bisnis

**Tujuan.** Dokter membaca catatan perawat pada catatan terpadu yang sama, sehingga seluruh profesi
melihat satu perkembangan pasien.

**Pelaku.** Perawat, dokter, dan profesi lain yang menulis pada lembar terpadu.

**Pemicu.** Sebuah catatan perkembangan ditulis untuk pasien yang sedang dirawat inap.

**Langkah yang berurutan.**

1. Penulis mengirim catatan beserta kunjungan pasien dan profesinya.
2. Sistem menormalkan profesinya — *"Perawat"* menjadi `Nurse`, *"Dokter"* menjadi `Doctor`.
3. Sistem menurunkan konteks klinisnya seperti sebelumnya: konsultasi, pengkajian, tanda vital,
   unit layanan, dan klinik.
4. **Baru pada task ini:** sistem menurunkan juga **perawatan rawat inap** yang menaungi kunjungan
   itu, lalu menyimpannya pada catatan.
5. Catatan itu kini muncul pada lini masa catatan terpadu perawatan tersebut, berdampingan dengan
   catatan profesi lain, terurut menurut waktu catatan.
6. Koreksi atas catatan mana pun memakai mesin addendum yang sudah ada — bernomor, beralasan, dan
   tidak mengubah isi asli.

**Aturan yang berlaku.**

| Aturan | Isinya |
| --- | --- |
| Konteks perawatan **diturunkan backend** | Bukan diterima apa adanya dari layar. Penanda yang dikirim klien tetap diperiksa: bila tidak cocok dengan perawatan milik kunjungan itu, permintaan ditolak |
| Berlaku untuk **seluruh profesi** | Mencabangkan pengisian menurut profesi akan melahirkan dua perilaku pada satu tabel yang sama, dan lembar terpadu justru dibuat supaya seluruh profesi terbaca sebagai satu perkembangan pasien |
| Catatan di luar rawat inap tidak terbawa | Kunjungan poliklinik, medical check-up, dan IGD tidak punya perawatan rawat inap, sehingga konteksnya tetap kosong |
| Nol tabel dan nol kolom baru | `INT-KEP-03` menyatakan perubahan yang dibutuhkan adalah **nol** |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Penanda perawatan dikirim, tetapi tidak cocok dengan kunjungannya | `400` — *"Perawatan rawat inap tidak sesuai dengan kunjungannya."* |
| Penanda perawatan dikirim tanpa kunjungan | `400` — perawatan hanya dapat ditentukan dari kunjungan pasien |
| Kunjungan tidak punya perawatan rawat inap | Catatan tetap tersimpan; konteks perawatannya kosong |
| Perawatan sudah ditutup | Konteksnya tidak diisi, karena yang dicari adalah perawatan yang masih berjalan |

**Contoh nyata.** Ns. Sari menulis *"Tanda vital stabil, luka bersih"* pada lembar terpadu Tn. Budi
pukul 10.00. Dr. Andi menulis *"Terapi dilanjutkan, rencana pulang besok"* pukul 11.00. Ketika
siapa pun membuka lembar terpadu perawatan Tn. Budi, keduanya tampil berurutan pada satu halaman —
sebelum task ini, halaman itu kosong.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` kartu `BE-RWI-063` | Scope, acceptance criteria, dan larangan tabel baru |
| `contracts/integration-contract.md` `0.3.0` `INT-KEP-03` | Arah integrasi dan nol perubahan yang dibutuhkan |
| `contracts/api-contract.md` `0.3.0` bagian 4 | Nol perubahan diminta pada grup catatan terpadu |
| `data/data-dictionary.md` bagian 8 | Kolom kunci `TrxPatientIntegratedProgressNote` |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs` | Keberadaan dan nullability seluruh kolom penghubungnya |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Penurunan konteks klinis, pendaftaran keutuhan, dan endpoint lini masa `BE-RWI-053` |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | `FindOpenEpisodeIdAsync`, penurunan perawatan berjalan yang dipakai ulang |
| Pencarian `InpEpisodeId = ` di seluruh `Areas/` | **Temuan utama:** nol tempat yang mengisi kolom itu |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | `InpatientClinicalContextService` disuntikkan; `ResolveClinicalContextAsync` menjadi pembungkus yang menurunkan perawatan rawat inap, sementara penurunan lama dipindahkan apa adanya ke `ResolveClinicalContextCoreAsync`; `InpEpisodeId` diisi saat catatan dibuat dan ikut pada balasan |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientIntegratedProgressNoteDtos.cs` | `InpEpisodeId` pada permintaan pembuatan dan pada tiga bentuk balasan |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingProgressNoteRoutingTests.cs` | **Baru.** Enam uji acceptance termasuk regresi catatan dokter |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/CpptVerificationTests.cs` | Penyesuaian pembentukan controller |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/ClinicalDocumentFinalizationIntegrityTests.cs` | Penyesuaian pembentukan controller |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/MedicalRecordManagement/MedicalRecordFailurePathTests.cs` | Penyesuaian pembentukan controller |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/MedicalRecordManagement/ProgressNoteIntegrityRepairTests.cs` | Penyesuaian pembentukan controller |

> **Penurunan lama sengaja tidak disentuh.** Seluruh cabang `ResolveClinicalContextCoreAsync` —
> konsultasi, pengkajian, tanda vital, unit, klinik — dipindahkan tanpa satu baris pun berubah.
> Itulah sebabnya regresi catatan dokter dapat dijamin.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol endpoint baru.** Permintaan pembuatan menerima satu field opsional `InpEpisodeId`, dan tiga bentuk balasan ikut mengembalikannya. Perubahan bersifat aditif dan tidak merusak konsumen yang sudah ada |
| Database | **Nol tabel baru, nol kolom baru.** `dotnet ef migrations has-pending-model-changes` menjawab *"No changes have been made to the model since the last migration."* — **migration task ini kosong** |
| Keamanan/Auth | `NOT APPLICABLE` — nol resource dan nol action baru. Ketiga endpoint yang tersentuh memakai butir hak akses `PatientIntegratedProgressNote` yang sudah ada |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Menulis catatan ke lembar terpadu. **Perubahan task ini:** konteks perawatan rawat inap diturunkan backend dan ikut tersimpan | `PatientIntegratedProgressNote : Create` |
| `GET` | `/episodes/{episodeId}` | Lini masa catatan terpadu lintas profesi satu perawatan. **Perubahan task ini:** akhirnya berisi, karena konteksnya sudah terisi | `PatientIntegratedProgressNote : Read` |
| `GET` | `/{id}` | Detail satu catatan; kini ikut menyebut perawatan yang menaunginya | `PatientIntegratedProgressNote : Read` |

> Ketiganya **sudah ada sebelum task ini**. Tidak satu pun endpoint baru dibuat.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | `Build succeeded`, `213 Warning(s)`, `0 Error(s)` | `PASS` | Jumlah warning sama persis dengan garis dasar `6f7d81e` |
| **`dotnet ef migrations has-pending-model-changes`** | *"No changes have been made to the model since the last migration."* | `PASS` | **Pemeriksaan bahwa migration task ini kosong** — acceptance criteria 2 |
| `dotnet test` project SQLite, tapis `NursingProgressNoteRoutingTests` | `Failed: 0, Passed: 6, Total: 6` | `PASS` | Keluaran perintah |
| `dotnet test` project SQLite, seluruhnya | `Failed: 0, Passed: 448, Total: 448` | `PASS` | Garis dasar sebelum slice ini 394 |
| `dotnet test` project InMemory, seluruhnya | `Failed: 9, Passed: 917, Total: 926` | `EXISTING / ENVIRONMENT ISSUE` | Kesembilan kegagalan berada di `BillingManagement`; worktree pada commit `6f7d81e` tanpa perubahan slice ini menghasilkan angka yang sama persis |
| Skenario catatan terpadu berisi catatan dua profesi | Lini masa satu perawatan memuat catatan `Nurse` dan `Doctor` sekaligus | `PASS` | `LiniMasaSatuPerawatan_MemuatCatatanDuaProfesi` |
| Catatan keperawatan tampil pada lini masa | Profesi tersimpan `Nurse`; konteks perawatan terisi; barisnya terbaca lewat lini masa | `PASS` | `CatatanKeperawatan_TampilPadaLiniMasaCatatanTerpadu` |
| **Test regresi catatan dokter** | Pembuatan `200`; profesi tetap `Doctor`; detail terbaca; pendaftaran keutuhan tetap satu baris | `PASS` | `CatatanDokter_PerilakunyaTidakBerubah` |
| Regresi catatan di luar rawat inap | Kunjungan poliklinik menghasilkan konteks perawatan kosong | `PASS` | `CatatanTanpaPerawatanRawatInap_KonteksnyaTetapKosong` |
| Kolom penghubung sudah ada dan nullable | Lima kolom penghubung ditemukan pada model, seluruhnya nullable; nama tabel tidak berubah | `PASS` | `KolomPenghubungCatatanTerpadu_SudahAdaDanNullable` |
| Koreksi perawat dan koreksi dokter pada perawatan yang sama | Dua addendum bernomor 1 pada catatan masing-masing, beralasan, isi asli kedua catatan tidak berubah | `PASS` | `SatuPerawatan_MemuatKoreksiPerawatDanKoreksiDokter_KeduanyaAddendum` |
| Uji milik `MedicalRecordManagement` yang menyentuh controller ini | Seluruhnya tetap hijau setelah penyesuaian pembentukan controller | `PASS` | Termasuk dalam angka `448` di atas |

Uji manual: `NOT FEASIBLE` — alasannya sama dengan `BE-RWI-059`.

**Tidak dijalankan:**

- **Eksekusi migration.** Task ini tidak membuat migration sama sekali, dan itu justru dibuktikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Catatan keperawatan tampil pada catatan terpadu dengan `ProfessionType` perawat | Terpenuhi | `NormalizeProfessionType` yang sudah ada memetakan *"Perawat"* menjadi `Nurse`; konteks perawatan kini terisi sehingga barisnya sampai ke lini masa. Uji `CatatanKeperawatan_TampilPadaLiniMasaCatatanTerpadu` dan `LiniMasaSatuPerawatan_MemuatCatatanDuaProfesi` |
| 2. **Nol tabel dan nol kolom baru** dibuat task ini, dan migration task ini **kosong** | Terpenuhi | `dotnet ef migrations has-pending-model-changes` menjawab tidak ada perubahan model; uji `KolomPenghubungCatatanTerpadu_SudahAdaDanNullable` memeriksa kolomnya memang sudah ada sejak sebelum task ini |
| 3. Catatan dokter yang sudah ada pada catatan terpadu tidak berubah perilakunya, dibuktikan test regresi | Terpenuhi | Penurunan konteks lama dipindahkan **tanpa satu baris pun berubah** ke `ResolveClinicalContextCoreAsync`. Uji `CatatanDokter_PerilakunyaTidakBerubah` dan `CatatanTanpaPerawatanRawatInap_KonteksnyaTetapKosong`; seluruh uji catatan terpadu milik `MedicalRecordManagement` tetap hijau |
| 4. Satu episode dapat memuat koreksi perawat dan koreksi dokter pada catatan terpadu yang sama, keduanya dalam bentuk **addendum bernomor** — bukan satu versi dan satu addendum (`RWI-DEC-091`) | Terpenuhi | `SatuPerawatan_MemuatKoreksiPerawatDanKoreksiDokter_KeduanyaAddendum` memeriksa dua koreksi lahir sebagai `MrcClinicalNoteAddendum` bernomor beserta alasannya, dan isi asli kedua catatan tidak berubah. Nol baris versi terbentuk — mesin versi memang tidak dipakai lembar terpadu |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Nol tabel baru | Terpenuhi |
| Migration kosong | Terpenuhi — dibuktikan `has-pending-model-changes` |
| Penyaluran catatan terpadu berjalan | Terpenuhi |
| Keempat acceptance criteria terbukti | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Clinical` — prefix **`Cli`**, lifecycle `ACTIVE / LEGACY` |
| Keberlakuan | **`TOUCHED LEGACY`** — `TrxPatientIntegratedProgressNote` dan controller-nya adalah legacy yang disentuh secara terbatas. Nol entity baru, sehingga `NEW CODE` tidak berlaku bagi berkas mana pun |
| QBE ID yang berlaku | `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-MOD-*`, `QBE-CODE-*` — nol entity, configuration, tabel, dan nomor bisnis. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION` |
| Legacy ratchet | **Dihormati.** Nama `Trx*` pada tabel yang disentuh **tidak** dinormalkan; penormalannya adalah kampanye `LEGACY MIGRATION` tersendiri milik pemilik modul, dan menyeretnya ke sini akan melanggar larangan penulisan ulang massal |
| `QBE-SVC-001` | **Tidak ditegakkan pada berkas ini, dan itu disengaja.** `PatientIntegratedProgressNoteController` adalah controller legacy yang mengakses `ApplicationDbContext` langsung. Memindahkannya ke service berarti penulisan ulang di luar scope task ini. Dicatat sebagai temuan, bukan diperbaiki tanpa wewenang |

### Delta kontrak yang dilaporkan ke pemilik kontrak

| Delta | Isinya | Kenapa |
| --- | --- | --- |
| **`InpEpisodeId` menjadi field permintaan dan field balasan** | `api-contract.md` `0.3.0` bagian 4 menyatakan "nol perubahan diminta pada grup ini" | Kolomnya sudah ada; yang ditambahkan hanyalah jalan mengisinya dan membacanya kembali. Tanpa keduanya, `INT-KEP-03` tidak dapat dipenuhi sama sekali. Perubahan bersifat aditif dan tidak merusak konsumen yang sudah ada |
| **Pengisian konteks berlaku untuk seluruh profesi** | Kartu task menyebut "penyaluran catatan keperawatan" | Mencabangkan pengisian menurut profesi akan melahirkan dua perilaku pada satu tabel. Akibat sampingnya justru menguntungkan: endpoint lini masa milik `BE-RWI-053`, yang selama ini selalu kosong, kini berisi bagi catatan dokter juga |
| **Kebijakan tampil belum ditetapkan** | PRD `CAP-014` aturan 4 menyebut "sesuai policy" tanpa menyebut kebijakannya — pertanyaan 4 pada `04-prd-to-mvp.md` bagian 20 | **Tidak memblokir**, dan tidak diputuskan di sini. Yang dibangun adalah penyalurannya; catatan tetap tersimpan dan tetap terbaca apa pun kebijakan tampilnya kelak. Pemilik: clinical governance |

### Temuan yang dilaporkan, bukan diperbaiki

| Temuan | Sikap |
| --- | --- |
| Endpoint `GET /episodes/{episodeId}` milik `BE-RWI-053` **tidak pernah berisi** sejak dibuat, karena kolom penyaringnya tidak pernah diisi siapa pun | Ditutup task ini sebagai akibat langsung dari `INT-KEP-03`. Dilaporkan supaya sub-modul `dokter-rawat-inap` mengetahui bahwa permukaan miliknya baru benar-benar berfungsi sekarang |
| Catatan terpadu yang **sudah tersimpan sebelum task ini** tetap berkonteks kosong | Tidak diisi ulang. Pengisian data lama adalah pekerjaan tersendiri yang menyentuh baris milik pasien nyata, dan wewenangnya tidak diberikan task ini |
| `PatientIntegratedProgressNoteController` mengakses `ApplicationDbContext` langsung | Utang teknis legacy. Tidak diperbaiki; legacy ratchet melarang penulisan ulang massal di luar scope |

### Ringkasan lain

| Hal | Isi |
| --- | --- |
| Peringatan | Jumlah warning solusi tetap `213`, sama persis dengan garis dasar |
| Masalah yang diketahui | Catatan terpadu lama tetap tidak muncul pada lini masa perawatan sampai ada task pengisian data lama yang diberi wewenang tersendiri |
| Risiko tersisa | Kebijakan tampil catatan keperawatan pada lembar terpadu belum ditetapkan clinical governance. Sampai itu terjadi, seluruh catatan tampil apa adanya — dan itu perilaku yang paling aman secara klinis |
| Perubahan sampingan | `NONE`. Empat berkas uji milik slice lain disesuaikan karena konstruktor controller bertambah satu ketergantungan; nol perilaku uji yang berubah |
| Interupsi | `NONE` |
| Status Git | 40 entri; nol berkas milik modul lain yang berubah selain empat berkas uji di atas |
| Langkah berikutnya | Gelombang `KEP-MVP-3` tinggal menunggu uji PostgreSQL `BE-RWI-061`. Setelah itu wewenang eksekusi ketiga migration diminta terpisah kepada pemilik database |
