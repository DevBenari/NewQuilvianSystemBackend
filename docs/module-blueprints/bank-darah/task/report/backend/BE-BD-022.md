# Laporan Perubahan Backend — `BE-BD-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-022` |
| Judul | Blood Group Conflict Issuance Gate |
| Slice | 3 — Kantong darah, pemberian, dan penyelesaiannya (prasyarat backend `FE-BD-005` butir `FE-BD-007`) |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) — kartu `BE-BD-022` |
| Trace | `DEC-BD-026`; `VAL-BD-034` (`validation-matrix.md`); `state-transition-matrix.md` baris "Memakai golongan darah saat pasien `IsConflictHeld`"; `permission-audit-matrix.md` baris "Pasien `IsConflictHeld` ditahan"; `INV-BD-014`, `INV-BD-030`; kewajiban layar `FE-BD-007` pada acceptance `FE-BD-005`; keputusan pemilik 25 September 2026 (bagian 2.1) |
| Contract version | api-contract `v5` (`approved`, `Sukmagp` 19 September 2026) + Amendment **`D8`** (25 September 2026; menegakkan kode yang sudah ada, nomor set kontrak tidak dinaikkan) |
| Dependency | `BE-BD-005` ✅, `BE-BD-011` ✅, `BE-BD-007` ✅, `BE-BD-008` ✅, `BE-BD-021` ✅ (`49c8af97`), keputusan pemilik ✅ 25 September 2026 |
| Klasifikasi | `LIGHT` — satu service disentuh (satu helper baru, satu pemeriksaan pada dua evaluator dan satu tindakan), satu record internal dan satu DTO aditif; nol endpoint, entity, migration, hak akses |
| Task mode | `BACKEND` — backend target tulis; frontend referensi read-only |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/BloodBankManagement/**`; dokumen `docs/module-blueprints/bank-darah/**` (laporan, kartu roadmap atas perintah pemilik, amandemen kontrak, matriks acceptance, traceability) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | Basis `49c8af97` (`feat(bank-darah): add blood unit gate projection BE-BD-021`), branch `sukmagp`, working tree bersih saat mulai. Belum di-commit |
| Tanggal | 25 September 2026 |
| Status | ✅ **SELESAI — 25 September 2026.** Keenam acceptance `AC-BD-132`..`AC-BD-137` terpenuhi. Build `0 Error(s)` / `214 Warning(s)` (baseline); `has-pending-model-changes` bersih, nol migration; QBE Strict `PASS` (2 berkas, `VIOLATION 0`); runtime R0–R7 **8/8 `PASS`** pada percobaan pertama, dengan konflik golongan darah **sungguhan** yang dibentuk lalu diselesaikan lewat API. **Batas bukti:** aktor tunggal `superadmin`. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BloodBankManagement` |
| Pemilik / prefix registry | `Bbk` — Blood Bank, lifecycle **`ACTIVE`** |
| Keberlakuan | `TOUCHED LEGACY` pada `BbkBloodUnitService.cs` dan `EmergencyAuthorizationDtos.cs` (disentuh sempit) |
| QBE yang berlaku | `QBE-API-001` (envelope dan kode galat mapan), `QBE-DTO-001` (DTO publik terpisah dari record internal); `QBE-MOD-002` tidak terpicu (nol entity baru) |
| Branch | `sukmagp`, bersih saat mulai |
| Governance terbaca | `AGENTS.md`, `docs/engineering/` (kontrak dan registry), `rules/backend/TASK_RULES.md`, `REPORT_TEMPLATE.md` — dibaca pada sesi yang sama untuk `BE-BD-021` |

---

## 1. Masalah yang diperbaiki

`VAL-BD-034` sudah tertulis di matriks validasi sejak blueprint disetujui — "pasien sedang `IsConflictHeld`
→ `422`" — tetapi tidak pernah ditegakkan pada kantong. Akibatnya darah dapat diberikan kepada pasien yang
hasil golongan darahnya sedang bertentangan, baik lewat jalur normal maupun jalur darurat.

Contoh dari runtime (R2–R4): pasien `e8b91912…` divalidasi A Positif, lalu pemeriksaan kedua divalidasi B
Positif. Backend pemeriksaan golongan darah menandai keduanya bertentangan dan menyatakan "gerbang klinis
tertahan". Sebelum task ini, kantong `TEST-BD007-20260914171359-04` untuk pasien itu tetap dinilai hanya dari
bukti kecocokannya, dan jalur darurat tetap dapat menerbitkan pemberian. Sesudahnya, kedua jalur ditolak
`VAL-BD-034`.

---

## 2. Proses bisnis

### 2.1 Keputusan pemilik, 25 September 2026

| No | Keputusan |
| --- | --- |
| 1 | Konflik golongan darah (`VAL-BD-034`) **wajib memblokir pemberian**, ditegakkan backend, bukan frontend |
| 2 | Memblokir **kedua** jalur: pemberian normal dan jalur darurat |
| 3 | Hanya konflik (`IsConflictHeld`) yang memblokir. Pasien yang **belum** punya golongan darah tervalidasi **tidak** diblokir |
| 4 | Input golongan darah (`FE-BD-06`) masuk cakupan `FE-BD-005`, dimulai dari konteks pasien — dikerjakan task frontend, bukan task ini |

### 2.2 Alur

1. Petugas menekan **Berikan** atau **Jalur Darurat** pada kantong Dialokasikan.
2. Backend menentukan pasien tujuan dari alokasi aktif, lalu membaca golongan darah sah pasien itu lewat
   fungsi yang sama dengan `GET /blood-group-exams/patient/{patientId}/valid`.
3. Bila pasien sedang menahan konflik, tindakan ditolak `422 VAL-BD-034` dengan pesan "Golongan darah pasien
   ini sedang bertentangan dan ditahan. Selesaikan perbedaannya lebih dulu." Kantong tidak berubah.
4. Detail kantong memproyeksikan hal yang sama sebelum tombol ditekan: `issuanceGate.validationCode =
   VAL-BD-034` dan `emergencyBypass.bloodGroupGateClosed = true`.
5. Validator klinis menyelesaikan konflik lewat pemeriksaan ulang (`FE-BD-009`). Begitu selesai, penilaian
   kembali ke gerbang berikutnya (misalnya bukti kecocokan).

### 2.3 Urutan gerbang pemberian normal

| Urutan | Syarat | Kode |
| ---: | --- | --- |
| 1 | Status `Allocated` | `VAL-BD-017` |
| 2 | Lokasi aktif | `VAL-BD-065` |
| 3 | Alokasi aktif ke pasien | `VAL-BD-017` |
| **4** | **Pasien tidak sedang menahan konflik golongan darah** | **`VAL-BD-034`** (baru ditegakkan) |
| 5–9 | Bukti kecocokan | `018`, `019`, `020b`, `079`, `020` |

### 2.4 Jalur tidak normal

| Keadaan | Hasil |
| --- | --- |
| Jalur darurat saat konflik, cakupan apa pun | `422 VAL-BD-034`, diperiksa **sebelum** kecocokan cakupan — bukan `VAL-BD-066`. Konflik bukan cakupan bypass (`INV-BD-030` tetap bukti dan lokasi) |
| Pasien belum punya golongan darah tervalidasi | Tidak diblokir (keputusan 3); gerbang berlanjut ke bukti kecocokan |
| Pasien lain | Tidak terdampak |
| Kantong bukan `Allocated` | Proyeksi tetap `null` (`BE-BD-021` `R1`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BbkBloodUnitService.cs` (gerbang normal, bypass, `EmergencyIssueAsync`, `GetDetailAsync`, konstruktor),
`BbkBloodGroupExamService.cs` (`GetValidBloodGroupAsync`, `ValidateAsync`, `ApplyValidationOutcome`,
`ResolveConflictAsync`), `BbkBloodGroupExamController.cs`, `BloodGroupExamDtos.cs`,
`EmergencyAuthorizationDtos.cs`, `Program.cs` (registrasi DI), enum `BloodType`; `validation-matrix.md`,
`state-transition-matrix.md`, `permission-audit-matrix.md`, `api-contract.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | (a) Konstanta `Val034Message` persis rumusan matriks. (b) Konstruktor menerima `BbkBloodGroupExamService` — pola yang sama dengan `BbkEncounterStatusReader`; keduanya `Scoped`, nol ketergantungan balik. (c) Helper `IsBloodGroupConflictHeldAsync` membaca `GetValidBloodGroupAsync`. (d) `EvaluateIssuanceGateAsync`: `VAL-BD-034` sesudah alokasi aktif, sebelum gerbang bukti. (e) `EvaluateEmergencyBypassAsync`: mengisi `BloodGroupGateClosed`. (f) `EmergencyIssueAsync`: menolak `VAL-BD-034` sesudah pasien diketahui, sebelum kecocokan cakupan. (g) `GetDetailAsync`: memetakan isian baru |
| `Areas/HealthServices/BloodBankManagement/DTOs/EmergencyAuthorizationDtos.cs` | Record internal `BloodUnitEmergencyBypassState` dan DTO publik `BloodUnitEmergencyBypassDto` mendapat `BloodGroupGateClosed` |
| `docs/.../contracts/api-contract.md` | Amendment `v5` **`D8`**; baris `last_changed_in` |
| `docs/.../testing/acceptance-test-matrix.md` | Bagian 15 baru, `AC-BD-132` sampai `AC-BD-137` |
| `docs/.../roadmap/backend-roadmap.md` | Kartu `BE-BD-022` baru — atas perintah pemilik, mengikuti pola kartu `BE-BD-021` |
| `docs/.../roadmap/requirement-traceability.md` | Bukti `BE-BD-022` dan baris acceptance baru |
| `docs/.../task/report/backend/BE-BD-022.md` | Laporan ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `issue`/`emergency-issue` dapat memulangkan `422 VAL-BD-034` — kode yang sudah terdaftar di kontrak. `BloodUnitEmergencyBypassDto` memperoleh `bloodGroupGateClosed` (aditif). Nol endpoint baru. Amendment `D8` |
| Database | Nol entity, konfigurasi, migration. `has-pending-model-changes` bersih. Hanya query baca tambahan (pemeriksaan golongan darah pasien tujuan) pada penilaian gerbang |
| Keamanan/Auth | Nol perubahan atribut hak akses. Pemanggil `issue`/`emergency-issue` tidak perlu `BloodGroupExam : Read` — gerbang membaca data di service, bukan lewat endpoint. Tidak ada data baru yang terbuka: `bloodGroupGateClosed` hanya boolean, dan golongan darahnya sendiri tidak dikirim |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Unit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/blood-bank-management/blood-units/{id}/issue` | Pemberian normal; **kini** ditolak `422 VAL-BD-034` saat pasien tujuan menahan konflik | `BloodUnit : Issue` |
| `POST` | `/{id}/emergency-issue` | Jalur darurat; **kini** ditolak `422 VAL-BD-034` saat konflik, apa pun cakupannya | `BloodUnit : EmergencyIssue` |
| `GET` | `/{id}` | Detail; proyeksi membawa `034` dan `bloodGroupGateClosed` | `BloodUnit : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>/out22` | **`Build succeeded`** — **`0 Error(s)`, `214 Warning(s)`**, `01:07:57` | `PASS` | Sama dengan baseline. Tiga `CS1573` pada `BbkBloodUnitService.cs` tetap milik record `BloodUnitResult` lama (baris bergeser). Nol peringatan dari baris task — parameter record baru berdokumentasi |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | Diff nol berkas model/konfigurasi/migration |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 2 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0` | `PASS` | `Final result: PASS` |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test terpisah |
| Validasi runtime R0–R7 | **8/8 `PASS`**, percobaan pertama | `PASS` | Bagian 5.1 |

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

**Tidak dijalankan:** jalur `403` dengan aktor tanpa hak akses (tidak ada atribut hak akses yang berubah;
kredensial akun lain tidak tersedia).

### 5.1 Hasil validasi runtime — 25 September 2026

Aplikasi hasil build dijalankan dari scratchpad pada `http://localhost:5217` terhadap
**`QuilvianNewDevSukma`**, sesi `superadmin` dari login seed (kredensial dibaca dari konfigurasi tanpa
dicetak; cookie dikirim sebagai header karena bertanda `secure`). Konflik dibentuk **sungguhan** lewat
endpoint pemeriksaan golongan darah, bukan lewat SQL.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; tanpa cookie `401` | — | `PASS` |
| R1 | Keadaan awal. `TEST-BD007-…-04` (pasien `e8b91912…`, belum ada pemeriksaan); `TEST-BD010-…-04` (pasien `1967de99…`, **tanpa** golongan darah tervalidasi, bukti berlaku) | `…BD007-…-04`: `VAL-BD-020`, `bloodGroupGateClosed = false`. `…BD010-…-04`: `/valid` "belum punya hasil golongan darah yang tervalidasi", **gerbang terbuka**, `bloodGroupGateClosed = false` | `AC-BD-135` | `PASS` |
| R2 | Pemeriksaan 1 (sampel → hasil A Positif → validasi), lalu pemeriksaan 2 (B Positif) | Keenam panggilan `200`. Sesudah A Positif sah, gerbang tetap `020` (golongan darah sah tidak memblokir). Validasi kedua: "berbeda dari hasil sah sebelumnya … gerbang klinis tertahan"; `/valid` `isConflictHeld = true`, dua pemeriksaan bertentangan | — | `PASS` |
| R3 | `GET /{id}` `…BD007-…-04` | `issuanceGate`: `VAL-BD-034`, pesan persis matriks dan **identik** dengan pesan `/valid`, `compatibilityEvidenceId`/`validUntil` kosong (bukan lagi `020`). `emergencyBypass`: `bloodGroupGateClosed = true`, `evidenceGateClosed = true`, `locationGateClosed = false` | `AC-BD-134`, `AC-BD-136` | `PASS` |
| R4 | `POST /issue`; `POST /emergency-issue` cakupan `0` (hasil pemetaan `D7`) dan `2` | Ketiganya `422 VAL-BD-034` — jalur darurat **bukan** `066`. Kantong tetap `Allocated`, versi tetap `10`, otorisasi darurat tetap `0` | `AC-BD-132`, `AC-BD-133` | `PASS` |
| R5 | Pasien lain selama konflik | `TEST-BD006-…-02` tetap `VAL-BD-018`; `TEST-BD010-…-04` tetap terbuka; keduanya `bloodGroupGateClosed = false` | `AC-BD-136` | `PASS` |
| R7 | Kantong Diberikan `TEST-BD007-…-01` | Proyeksi tetap `null` | `AC-BD-137` | `PASS` |
| R6 | Pemeriksaan ulang A Positif, lalu `POST /blood-group-exams/conflict-resolution` (`TBD009-ALIH`) — dijalankan di blok `finally` | Pemeriksaan ulang `200` ("masih tertahan sampai validator … menyatakan"); penyelesaian `200`; `/valid` A Positif sah, konflik `false`. Gerbang `…BD007-…-04` kembali ke `VAL-BD-020`, `bloodGroupGateClosed = false` | `AC-BD-136` | `PASS` |

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Pasien `e8b91912…` | **Tiga pemeriksaan golongan darah baru** (`TBE022-<waktu>-1/2/3`): A Positif, B Positif (keduanya riwayat konflik yang sudah diselesaikan), dan pemeriksaan ulang A Positif yang kini **sah**. Satu catatan penyelesaian konflik (`TBD009-ALIH`) |
| `TEST-BD007-20260914171359-04` | Tidak berubah (tetap Dialokasikan, versi `10`) — seluruh penolakan tanpa perubahan data |
| Kantong lain | Tidak berubah |

**Dampak ke pengujian berikutnya:** pasien `e8b91912…` kini punya golongan darah sah A Positif. Data ini
dibiarkan dan dicatat, sama dengan perlakuan data uji `FE-BD-009`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-132` — konflik menahan pemberian normal | **Terpenuhi** | R4 |
| `AC-BD-133` — konflik menahan jalur darurat, bukan `066` | **Terpenuhi** | R4 (cakupan `0` dan `2`) |
| `AC-BD-134` — urutan dan proyeksi | **Terpenuhi** | R3: `034` menggantikan `020`, `bloodGroupGateClosed = true` |
| `AC-BD-135` — hanya konflik yang menahan | **Terpenuhi** | R1 (tanpa golongan darah sah → terbuka), R2 (A Positif sah → tetap `020`) |
| `AC-BD-136` — sumber yang sama dan pelepasan | **Terpenuhi** | R3 pesan identik `/valid`; R5 pasien lain; R6 pelepasan |
| `AC-BD-137` — tanpa regresi | **Terpenuhi** | Diff: `AvailableActionsFor`, controller, atribut hak akses tidak disentuh; nol migration; nol `VAL-BD` baru; R7 |
| DoD: build, QBE, model | **Terpenuhi** | Bagian 5 |
| DoD: runtime | **Terpenuhi** | 8/8 `PASS` |
| DoD: laporan, kontrak, roadmap | **Terpenuhi** | Laporan ini; `D8`; bagian 15; kartu; traceability |
| DoD: tidak di-commit sebelum validasi final | **Terpenuhi** | Belum ada commit |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memakai memori jauh lebih besar dari biasanya (sekitar 10,6 GB pada satu proses `dotnet`), tetapi selesai `0 Error(s)`; dicatat sebagai pengamatan lingkungan |
| Masalah yang diketahui | **Alokasi dan pencatatan bukti kecocokan tidak diblokir konflik** — keputusan pemilik hanya menyebut pemberian; `state-transition-matrix` merumuskan "memakai golongan darah" secara umum, sehingga cakupan gerbang lain dapat diputuskan kemudian. Scheduler absensi HR gagal menyisipkan `HrdAttendanceProcessingRun` saat aplikasi uji berjalan — tidak terkait |
| Risiko tersisa | Satu pembacaan pemeriksaan golongan darah tambahan per penilaian gerbang (detail kantong `Allocated`, `issue`, `emergency-issue`). Jalur darurat kini tertutup total selama konflik — keputusan klinis pemilik; tidak ada jalan keluar darurat selain menyelesaikan konflik |
| Perubahan sampingan | `NONE` di repository. Alat bantu (skrip, output build, log) di scratchpad sesi. Di database: bagian 5.2 |
| Interupsi | Dua monitor build berakhir oleh batas 30 menit sebelum build selesai; dipasang ulang tanpa mengulang build |
| Status Git | `M` `BbkBloodUnitService.cs`, `EmergencyAuthorizationDtos.cs`, `api-contract.md`, `acceptance-test-matrix.md`, `backend-roadmap.md`, `requirement-traceability.md`; `??` laporan ini. Belum di-stage atau di-commit |
| Langkah berikutnya | Lanjutan `FE-BD-005`: input golongan darah dari konteks pasien pada `FE-BD-06`, dan dialog darurat membaca `bloodGroupGateClosed`. Layar kantong sudah menampilkan dan menahan `VAL-BD-034` lewat proyeksi `issuanceGate` tanpa perubahan |
