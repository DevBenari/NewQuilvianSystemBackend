# Laporan Perubahan Backend — `BE-BD-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-012` |
| Judul | Tindakan Bank Darah dicatat tanpa penyaluran biaya |
| Slice | `MVP-1` — jalur bekas `G4`; `ProcedureNumber` dari provider nomor bersama |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md`, blok task `BE-BD-012` |
| Trace | `DEC-BD-016`, `DEC-BD-021`, `DEC-BD-034` · `BD-AGG-05`, `BD-DOM-12` · `INV-BD-024` · `contracts/api-contract.md` §Blood Bank Procedure · `contracts/state-transition-matrix.md` §5 · `contracts/validation-matrix.md` §5 (`VAL-BD-026/027`) · `data/data-dictionary.md` §`BbkBloodBankProcedure` |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` ✅ · `G2b` ✅ · `G4` ✅ · `BE-BD-003` ✅ |
| Klasifikasi | `HEAVY` — skor 10: satu repository (0), berkas diperiksa lebih dari 20 (2), logika kompleks (2), memakai kontrak yang sudah disetujui (1), entity baru beserta migration (2), hak akses berkaitan tetapi bukan inti (1), dampak finansial lintas modul (2) |
| Task mode | `BACKEND` — dinyatakan eksplisit oleh pemilik pekerjaan |
| Target tulis | `NewQuilvianSystemBackend` — source `BE-BD-012`, migration, `Tests/**`, serta laporan, roadmap, traceability, dan `MODULE-STATUS` Bank Darah |
| Wewenang database | Pembuatan migration dan penerapannya **hanya** ke `QuilvianNewDevSukma` — **tidak dipakai**, karena task berhenti sebelum implementasi |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `1a4ea58` cabang `sukmagp`, working tree bersih |
| Tanggal | `2026-09-11` |
| Status | ⛔ **`BLOCKED`** — berhenti **sebelum satu baris source pun ditulis**. Tiga keputusan yang tidak ada di kontrak `v4` dibutuhkan: aturan pemilihan tarif, sumber unit dan kelas pasien, dan rumah kedua acceptance criteria. Lihat bagian 6 dan 7 |

---

## 1. Masalah yang hendak diselesaikan

Bank Darah melakukan tindakan — misalnya pemeriksaan kecocokan atau penyiapan komponen — yang kelak
menjadi dasar biaya. `DEC-BD-021` menetapkan biaya Bank Darah **berasal dari tindakan, bukan dari
kantong**: tiga kantong yang diberikan dalam satu tindakan tetap satu tindakan. Task ini semestinya
mencatat tindakan itu beserta **salinan tarifnya** pada saat kejadian, supaya perubahan data induk
tarif kelak tidak mengubah catatan lama — tanpa menyalurkan apa pun ke Billing.

**Kenapa berhenti.** Angka tarif yang disalin sepenuhnya bergantung pada **tarif mana yang dipilih**.
Aturan pemilihan itu tidak ada di kontrak, dan source memuat dua aturan yang memberi hasil berbeda.
Memilih satu sama dengan menetapkan kebijakan harga — wewenang pemilik tarif, bukan builder.

**Contoh konkretnya.** Tindakan "Uji Silang Serasi" punya dua tarif aktif di data induk: tarif umum
Rp150.000, dan tarif khusus kelas VIP Rp250.000. Pasiennya dirawat di kelas VIP.

| Aturan yang ada di source | Tarif yang terpilih | Salinan yang tersimpan |
| --- | --- | --- |
| Pola Laboratorium — `LabExaminationService`, rujukan `BD-CAP-008` kontrak | Yang **paling baru berlaku**, tanpa melihat kelas | Tergantung tanggal mulai berlaku — bisa Rp150.000 |
| Pola Klinis — `InsuranceCoverageService.FindProcedureTariffAsync`, dipakai resep dan Billing | Yang **paling spesifik** terhadap unit, klinik, dan kelas kunjungan | Rp250.000 |

Keduanya "sah" menurut source. Salinan yang keliru tidak dapat diperbaiki kemudian tanpa melanggar
sifat salinan itu sendiri — justru itulah gunanya salinan.

---

## 2. Proses bisnis menurut kontrak

| Hal | Isi |
| --- | --- |
| Pelaku | Petugas Bank Darah; Dokter BDRS sebagai penanggung jawab tindakan |
| Langkah | Catat tindakan atas satu order sah → nyatakan selesai |
| Status | `Recorded` → `Completed` (`state-transition-matrix.md` §5) |
| Aturan | Menunjuk satu order sah (`VAL-BD-026`); tarif **tidak** dihitung sendiri, dirujuk dari data tindakan bertarif (`VAL-BD-027`); satu tindakan menghasilkan paling banyak satu fakta biaya; koreksi tidak membalik biaya otomatis (`INV-BD-024`) |
| Batas | **Tidak ada endpoint penyaluran biaya** — tertahan `DEC-BD-016` |

Kontrak cukup untuk bentuknya — entity, empat endpoint, dua status, nomor dari number-series — tetapi
**tidak** untuk tiga hal pada bagian 6.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` · `CLAUDE.md` · suite skill `1.18.0`: `rules/backend/*`, `BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `transaction-endpoint-standard.md`, `role-access-rules.md`, `rules/rule-output/status-task-roadmap.md` |
| Kontrak modul | `roadmap/backend-roadmap.md` kartu `BE-BD-012` · `roadmap/requirement-traceability.md` · `contracts/api-contract.md` §Blood Bank Procedure · `contracts/validation-matrix.md` §5 · `data/data-dictionary.md` §`BbkBloodBankProcedure` · `02-backend-architecture.md` §F · `03-domain-architecture.md` §`BD-AGG-05` dan relasi · `03-frontend-architecture.md` `FE-BD-07` · `00-interview-decisions.md` `DEC-BD-016/021/034`, `AC-BD-026/058` · `testing/acceptance-test-matrix.md` §5 · `02-existing-capability-map.md` `BD-CAP-008/015` · `02-requirement-completeness-assessment.md` `BD-SLICE-08` |
| Source pembanding | `MstTariff.cs` · `MstProcedure.cs` · `MstTariffCategory.cs` · `LabExamination.cs` · `LabExaminationService.cs` · `LabSpecimenService.cs` · `InsuranceCoverageService.cs` · `TrxPatientEncounter.cs` · registrasi `Program.cs` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `docs/module-blueprints/bank-darah/task/report/backend/BE-BD-012.md` | **Baru.** Laporan ini |
| `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` | Status `BE-BD-012` 🟡 → ⛔ beserta nama blocker-nya |
| `docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md` | Bukti `BE-BD-012` dan tiga gap baru |
| `docs/module-blueprints/bank-darah/MODULE-STATUS.md` | Status task dan task berikutnya |

**Nol berkas source, test, maupun migration disentuh.**

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint dibuat |
| Database | `NOT APPLICABLE` — nol migration dibuat, nol eksekusi database. `QuilvianNewDevSukma` tidak disentuh |
| Keamanan/Auth | `NOT APPLICABLE` — nol butir hak akses baru |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` |
| Submodule | `NOT APPLICABLE` |
| Pemilik / prefix registry | `BloodBankManagement / Blood Bank` → prefix **`Bbk`** |
| Status registry | **`ACTIVE`** — registry suite skill `1.18.0` baris 29 |
| Keberlakuan | **`NEW CODE`** |
| QBE ID yang akan berlaku | `QBE-ENT-001/003` · `QBE-NAM-001/002/004` · `QBE-CFG-001` · `QBE-MOD-001/002/003` · `QBE-SVC-001` · `QBE-API-001` · `QBE-PERM-001` · `QBE-LOG-001` · `QBE-DTO-001` · `QBE-VAL-001` · `QBE-TXN-001` · `QBE-CODE-001..006` untuk `ProcedureNumber` |
| Pengecualian QBE | `NONE` |
| Preflight | **Lolos.** Yang menahan bukan tata kelola, melainkan keputusan bisnis dan perencanaan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — nol endpoint dibuat. Rencana kontrak `v4` untuk grup
**Health Services / Blood Bank Management / Blood Bank Procedure**, base URL
`api/v1/health-services/blood-bank-management/blood-bank-procedures`:

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar tindakan Bank Darah | `BloodBankProcedure : Read` |
| `GET` | `/{id}` | Detail tindakan | `BloodBankProcedure : Read` |
| `POST` | `/` | Catat tindakan atas satu order | `BloodBankProcedure : Create` |
| `POST` | `/{id}/complete` | Nyatakan tindakan selesai | `BloodBankProcedure : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `git status --short` sebelum mulai | Bersih pada `1a4ea58` | `PASS` | Keluaran perintah |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Nol source berubah; build hanya akan mengulang baseline `1a4ea58` |
| `dotnet test`, uji PostgreSQL, migration | Tidak dijalankan | `NOT RUN` | Nol source, test, maupun migration dibuat |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria, keputusan yang dibutuhkan, dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Isi kanonis | Status | Kenapa tidak dapat dibuktikan pada task ini |
| --- | --- | --- | --- |
| `AC-BD-026` | "Satu tindakan Bank Darah selesai dengan 2 kantong diberikan → **satu fakta biaya dikirim ke Billing**, bukan dua" | **Tidak dapat dibuktikan** | Menuntut **penyaluran fakta biaya ke Billing** — dilarang task ini dan milik `BE-BD-013` yang tertahan `DEC-BD-016` `OPEN`. Matriks acceptance sendiri menandainya "**tertunda `DEC-BD-016`**". Juga menuntut "2 kantong diberikan", yang lahir pada `BE-BD-007` |
| `AC-BD-058` | "Koreksi pencatatan pemberian dibuat; sistem mencoba otomatis membalik **fakta biaya** tindakan → ditolak" | **Tidak dapat dibuktikan** | Menuntut **koreksi pemberian** (`BE-BD-010`) dan **fakta biaya** yang terkirim (`BE-BD-013`). Tanpa keduanya, "tidak ada pembalikan" hanya benar karena tidak ada apa pun yang dapat dibalik — bukan bukti perilaku |

**Akibatnya:** walau entity, empat endpoint, snapshot, dan nomor dikerjakan sempurna, task ini
**tidak dapat pernah** mencapai ✅ dengan kedua kriteria itu. Bagian yang **memang** milik
`BE-BD-012` — pencatatan tindakan dan salinan tarif yang tidak berubah ketika data induk tarif
berubah — **tidak punya acceptance criteria sama sekali** pada roadmap.

### 6.2 Keputusan yang dibutuhkan

| No | Keputusan | Bukti bahwa kontrak tidak menjawabnya | Pemilik |
| ---: | --- | --- | --- |
| 1 | **Aturan pemilihan tarif tindakan Bank Darah.** Tarif mana yang dirujuk ketika satu tindakan punya lebih dari satu tarif aktif — per unit, klinik, kelas, atau tanggal berlaku — dan siapa yang memilih: petugas memilih `TariffId`, atau backend menyelesaikannya | Kamus data hanya menulis `TariffId` "Tarif dirujuk". `02-backend-architecture.md` dan `BD-CAP-008` hanya menetapkan **pola salinan**, bukan pemilihan. Source memuat dua aturan yang bertentangan (bagian 1). `DEC-BD-021`: tarif **dimiliki Billing** | Pemilik BillingManagement bersama pemilik proses BDRS |
| 2 | **Sumber `ServiceUnitId` dan `PatientClassId`.** Unit yang dimaksud adalah unit pemesan order, unit kunjungan, atau unit BDRS pelaksana tindakan; kelas diambil dari kunjungan atau diisi petugas | Kamus data hanya menulis "Unit" dan "Kelas", keduanya wajib. `TrxPatientEncounter.PatientClassId` boleh kosong. Keduanya menentukan tarif mana yang cocok, sehingga terikat keputusan 1 | Pemilik proses BDRS |
| 3 | **Rumah `AC-BD-026` dan `AC-BD-058`, dan acceptance criteria pengganti untuk `BE-BD-012`.** Misalnya: `AC-BD-026` ke `BE-BD-013`, `AC-BD-058` ke `BE-BD-010`/`BE-BD-013`; `BE-BD-012` diberi kriteria pencatatan saja — tindakan tercatat bernomor, salinan tarif tidak berubah ketika data induk tarif berubah, tanpa satu pun jalur ke Billing | Roadmap mengikat kedua kriteria ke `BE-BD-012`, sementara matriks acceptance menandai `AC-BD-026` tertunda `DEC-BD-016` dan `BD-SLICE-08` dinilai `PARTIALLY_READY` — "pencatatan tindakan siap dirancang; penyerahan biaya menunggu `DEC-BD-016`" | Pemilik roadmap, lewat `plan-module-delivery` |

Satu hal kecil yang ikut sebaiknya diputuskan bersama keputusan 3: arti "order **sah**" pada
`VAL-BD-026` — apakah tindakan boleh dicatat atas order yang sudah dibatalkan atau kedaluwarsa, bila
tindakannya sudah dikerjakan sebelum order berhenti.

### 6.3 Definition of Done

| Butir DoD | Status |
| --- | --- |
| Source dalam scope selesai | **Belum** — sengaja tidak dimulai |
| Number Series Platform direuse | **Belum** — rancangannya jelas (`NumberSeriesAllocator`, deret milik Bank Darah, pola `BE-BD-003`/`004`) dan tidak termasuk blocker |
| Snapshot tarif terbukti | **Belum** — tertahan keputusan 1 dan 2 |
| Tidak ada Billing posting | **Terpenuhi secara trivial** — nol kode |
| `AC-BD-026`, `AC-BD-058` terbukti | **Tidak dapat** — keputusan 3 |
| Build, test, migration, `has-pending-model-changes` | `NOT RUN` |
| Laporan, roadmap, traceability tersinkron | **Terpenuhi** — berkas ini dan ketiga register |

---

## 7. Delta kontrak

`NONE` — tidak ada yang diimplementasikan. Tiga kekosongan kontrak pada bagian 6.2 **tidak** diisi
sepihak.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Tiga keputusan pada bagian 6.2 |
| Risiko tersisa | Bila keputusan 1 diambil tanpa pemilik Billing, salinan tarif Bank Darah dapat berbeda dari angka yang dihitung Billing untuk tindakan yang sama ketika `BE-BD-013` kelak menyalurkan faktanya |
| Temuan di luar scope | **(1)** Laboratorium sendiri punya dua cara memilih tarif — `LabExaminationService` dan `LabSpecimenService.ResolveTariffAsync` — keduanya tanpa melihat kelas, dan tidak memeriksa `IsActive`. Berbeda dengan `InsuranceCoverageService` yang memeriksa keduanya. Milik pemilik Laboratorium. **(2)** `BE-BD-004` masih menunggu keputusan penerusan tiga kriterianya; itu tetap penahan jalur kritis `BE-BD-015` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Hanya empat berkas dokumentasi pada bagian 3.2 |
| Langkah berikutnya | **(1)** Satu pass perencanaan `plan-module-delivery` yang sekaligus meneruskan tiga kriteria `BE-BD-004` dan menata ulang kriteria `BE-BD-012` (keputusan 3). **(2)** Keputusan 1 dan 2 dari pemilik Billing dan BDRS — bila menyentuh kamus data atau api-contract, lewat `design-business-module` sebagai amandemen kontrak. **(3)** Sesudahnya: `BE-BD-015` di jalur kritis, lalu `BE-BD-012` |
