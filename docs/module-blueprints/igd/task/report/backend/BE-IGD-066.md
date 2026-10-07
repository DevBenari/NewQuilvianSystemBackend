# Laporan Perubahan Backend — `BE-IGD-066`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-066` |
| Judul | Kajian medis dokter diterima untuk kunjungan IGD |
| Slice | `EPIC IGD-14` / `MVP-9` — tab Pengkajian Medis |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-097`; `AT-IGD-201`, `202`; DoD PRD §10.4 butir 2, 3 |
| Keputusan | `IGD-DEC-221`; `IGD-DEC-230` pilihan desain 5 (kajian baru ditolak pada kunjungan berakhir) |
| Contract version | API **`0.15.0`** §10.1 nomor 7, §10.5; validation **`0.14.0`** §12.2 aturan 4–6 — `approved` (`IGD-DEC-230`, sementara pola `IGD-DEC-174`) |
| Dependency | — (baseline: `BE-IGD-027` ✅ pengkajian tanpa antrean untuk kunjungan IGD) |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 0, berkas diubah 0 (1 berkas), logika bisnis 1, kontrak API 1 (memakai kontrak yang ada; endpoint tetap), database 1 (perilaku kueri), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` — izin pemilik 7 Oktober 2026 (*"sudah saya build lanjutkan saja"*, urutan R3.16.4 langkah 2). Frontend baca-saja |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`; laporan ini, baris status roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `30ea0a3a` (`rizkiG`) + working tree `BE-IGD-065` (3 berkas IGD, sudah dibuild pemilik 09.54 WIB). Berkas task ini tidak berubah sejak `43dab6da` |
| Commit frontend rujukan | `6c66327aa` (`RizkiV2`) + working tree `FE-IGD-045` |
| Tanggal | 7 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 7 Oktober 2026: implementasi selesai; 1 dari 7 acceptance terbukti** (6, lewat diff — dengan catatan). Satu berkas (+42/−0); QBE checker Strict `PASS`. **Belum:** build pemilik (7) dan uji API/layar 1–5 pada putaran 1 bersama `FE-IGD-046`. Tanpa UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `Cli` = *Clinical*, `BUSINESS DOMAIN / MODULE`, `ACTIVE / LEGACY` — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 22. Pemilik modul `OPEN`; sementara Product/Domain Owner IGD (`IGD-DEC-107`) |
| Applicability | **`TOUCHED LEGACY`** — penjaga validasi yang sudah ada pada `CreateAssessment`. Nol entity, nol model persisted, nol rename, nol migration. `TrxPatientAssessment` dibaca apa adanya (prefix lama, tidak dinormalkan — di luar cakupan) |
| QBE yang berlaku | `QBE-API-001` (route, envelope, dan pola kode status `CreateGuard` tetap; `409` untuk keadaan, sama dengan penjaga lain), `QBE-VAL-001` (invarian kajian medis per kunjungan IGD), `QBE-PERM-001` (`PatientAssessment : Create` tidak disentuh) |
| QBE yang dicatat, tidak ditangani | `QBE-SVC-001` — penjaga tinggal di controller dengan `ApplicationDbContext` langsung (pola lama controller 3537 baris); `QBE-NAM-001` — `TrxPatientAssessment` legacy, hanya dibaca |
| QBE yang tidak berlaku | `QBE-MOD-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*` |
| Checker | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path …/PatientAssessmentController.cs -Mode Strict` — VIOLATION 0, REVIEW 0, INFO 0, *Final result: PASS* |

---

## 1. Masalah yang diperbaiki

Dokter IGD tidak dapat menulis kajian medis. `POST /patient-assessments` bertipe `MedicalInitial`/`MedicalReassessment`
selalu meminta konteks rawat inap (`ResolveForDoctorWriteAsync`), sehingga pasien IGD ditolak *"Pasien ini tidak sedang
dirawat inap."* (`IGD-CAP-75`). Tab Pengkajian Medis di layar dokter IGD (`FE-IGD-046`) tidak akan pernah dapat menyimpan.

*Contoh.* dr. Ani memeriksa Tn. Budi di IGD dan menulis kajian medis awal. Hari ini server menolak dengan kalimat rawat
inap, padahal Tn. Budi memang tidak dirawat inap — ia pasien IGD.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Kajian medis dokter untuk pasien IGD tercatat di rekam medis, satu kajian awal per kunjungan dan kajian ulang sesuai kebutuhan |
| Pelaku | Dokter IGD (akun yang terhubung ke data dokter) |
| Pemicu | Dokter menyimpan kajian medis dari tab Pengkajian Medis |
| Prasyarat | Kunjungan IGD masih berjalan; encounter-nya milik kunjungan IGD |

**Langkah.**

1. Server memeriksa enum dan penanda perawatan (tidak berubah).
2. Aturan 4: pengguna wajib terhubung ke data dokter, selain itu `403` (pesan yang ada).
3. **Baru:** bila encounter milik kunjungan IGD, server tidak mencari perawatan rawat inap. Ia memeriksa status kunjungan
   (aturan 5) dan, untuk kajian awal, keunikannya per kunjungan (aturan 6).
4. Kajian tersimpan dengan `InpEpisodeId` kosong (penentuan episode yang sudah ada mengembalikan `null` untuk IGD).

**Aturan.**

| No | Aturan | Kode | Pesan |
| ---: | --- | :-: | --- |
| 4 | Hanya pengguna yang terhubung ke data dokter | `403` | *(pesan yang ada)* *"Catatan ini hanya dapat ditulis dokter."* |
| 5 | Kunjungan IGD belum `Completed`/`Cancelled` | `409` | *"Kunjungan IGD ini sudah berakhir, sehingga kajian medis baru tidak dapat dibuat. Gunakan addendum pada kajian yang sudah ada."* |
| 6 | `MedicalInitial` satu per kunjungan IGD — dihitung dari kajian awal yang tidak dihapus, tidak dibatalkan, dan tidak berstatus `Cancelled` pada encounter kunjungan itu; `MedicalReassessment` tidak dibatasi aturan ini | `409` | *"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."* |

**Status.** Tidak ada status baru; kajian lahir dengan status yang sama seperti jalur rawat inap.

**Jalur tidak normal.**

| Keadaan | Perilaku |
| --- | --- |
| Perawat mencoba kajian medis | `403` aturan 4 |
| Kajian awal kedua | `409` aturan 6, **sebelum** pemeriksaan "draft assessment untuk encounter ini sudah ada" yang lebih umum |
| Kajian ulang kedua saat draf kajian ulang masih terbuka | `400` *"Draft assessment untuk encounter ini sudah ada…"* — perilaku lama `ValidateCreateWithoutQueueAsync`, tidak diubah |
| Kunjungan sudah *Selesai* atau *Dibatalkan* | `409` aturan 5 |
| Encounter poliklinik (bukan IGD, tanpa episode) | Ditolak seperti hari ini lewat cabang rawat inap |
| Pasien rawat inap | Cabang rawat inap tidak berubah satu baris pun |

**Hasil akhir.** Satu kajian medis awal per kunjungan IGD tersimpan atas nama dokter penulisnya; kajian ulang dapat
ditambahkan selama kunjungan berjalan; koreksi sesudah kunjungan berakhir lewat addendum.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `PatientAssessmentController.cs` — `CreateAssessment` `:617`, `ValidateCreateRequestAsync` `:1979`,
  `ValidateCreateWithoutQueueAsync` `:2114`, `ValidateWithoutQueueGateAsync` `:2162` (pengenalan kunjungan IGD lewat
  `EmgVisit` yang sudah dipakai), `EnsureSoleAuthorAsync` `:2312` (jalur ubah/selesai meloloskan IGD),
  `ValidateInpatientMarkersAsync` `:2345`, `ValidateMedicalAssessmentRuleAsync` `:2442`
- `EmergencyInstallationManagement/Enums/EmergencyVisitStatus.cs`; nama tipe di 19 enum IGD dibandingkan dengan namespace yang
  diimpor controller — nol bentrok

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` (+42/−0; CRLF, tanpa BOM — dipertahankan) | + `using …EmergencyInstallationManagement.Enums`. + konstanta `PenolakanKajianMedisKunjunganIgdBerakhir` dan `PenolakanKajianMedisAwalIgdGanda` (kalimat persis validation §12.2, pola konstanta `Penolakan*` berkas ini). `ValidateMedicalAssessmentRuleAsync`: sesudah aturan 4, status kunjungan IGD dibaca dari `EmgVisit` (baca-saja; kunjungan terbaru bila ada lebih dari satu); bila ada, aturan 5 dan 6 diterapkan dan fungsi kembali **sebelum** resolusi rawat inap. Cabang rawat inap tidak berubah |

Nol baris komentar ditambahkan; komentar lama tidak disunting.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai API `0.15.0` §10.5: kajian medis kini diterima untuk kunjungan IGD (sebelumnya ditolak). Bentuk request/response tidak berubah. **Delta kecil:** acceptance 6 menyebut *"hanya `ValidateMedicalAssessmentRuleAsync`"*; diff juga memuat satu `using` dan dua konstanta pesan — keduanya bagian langsung dari perubahan itu |
| Database | Nol schema/migration. Dua kueri baca tambahan pada pembuatan kajian medis: status `EmgVisit` dan keberadaan kajian awal pada encounter |
| Keamanan/Auth | `NOT APPLICABLE` — butir hak akses tidak berubah; penjaga dokter pelaku (aturan 4) tetap paling awal |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Kajian medis dokter (`MedicalInitial`/`MedicalReassessment`) **kini** diterima untuk pasien IGD | `PatientAssessment : Create` | `CreatePatientAssessmentRequest` (tidak berubah; `inpEpisodeId` kosong) | `PatientAssessmentCreateResponse` |

`201`/`200` tersimpan; `403` aturan 4; `409` aturan 5 dan 6 dengan kalimat persis di atas.

---

## 5. Verifikasi

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path …/PatientAssessmentController.cs -Mode Strict` | VIOLATION 0, REVIEW 0, INFO 0 — *Final result: PASS* | `PASS` |
| `git diff --numstat` | 1 berkas, +42/−0 | `PASS` |
| Baris komentar ditambah | Nol | `PASS` |
| Akhiran baris dan BOM | 3579 CRLF, 0 LF, tanpa BOM (sebelum: 3537 CRLF) | `PASS` |
| Bentrok nama dari `using` baru | 19 enum IGD dibandingkan; nol tipe senama di namespace yang diimpor | `PASS` |
| `dotnet build` | Belum — dijalankan Rizki | `NOT RUN` |
| Uji API/layar acceptance 1–5 | Belum — putaran 1 bersama `FE-IGD-046` | `NOT RUN` |

AUTOMATED TEST: SKIPPED (opsional) — atas perintah pemilik.

### 5.1 Perintah build

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

Mohon laporkan jumlah **error dan warning**.

### 5.2 Skenario uji untuk panduan putaran 1

Dokter `ranger.biru@admin.com`; perawat `dimas.kurniawan@rsmmc.local` (`IGD-DEC-215`); sandi dari variabel lingkungan; data
lewat layar, tidak lewat SQL.

| No | Langkah | Harapan | Acceptance |
| ---: | --- | --- | --- |
| `066-S1` | Dokter menyimpan kajian medis awal untuk kunjungan IGD berjalan | `200`/`201`; tersimpan tanpa `inpEpisodeId`; tampil di tab | 1 |
| `066-S2` | Dokter menyimpan kajian medis awal kedua; lalu kajian ulang | Pertama `409` aturan 6; kajian ulang tersimpan | 2 |
| `066-S3` | Kajian medis baru pada kunjungan `Completed` (bila ada) | `409` aturan 5 | 3 |
| `066-S4` | Perawat (`dimas`) mencoba kajian medis | `403` *"Catatan ini hanya dapat ditulis dokter."* | 4 |
| `066-R1` | Kajian medis pasien rawat inap (bila ada) dan encounter poliklinik | Perilaku tidak berubah | 5 |

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Kajian medis awal kunjungan IGD berjalan tersimpan tanpa episode rawat inap (`AT-IGD-201`) | **Terpetakan, belum diuji** | Cabang IGD kembali `Ok` sebelum resolusi rawat inap; `FindOpenEpisodeIdAsync` → `null` untuk IGD (`CreateAssessment`) |
| 2 | Kajian awal kedua `409` aturan 6; kajian ulang diterima (`AT-IGD-202`) | **Terpetakan, belum diuji** | `kajianAwalIgdSudahAda`; `MedicalReassessment` kembali `Ok` |
| 3 | Kajian baru pada kunjungan `Completed`/`Cancelled` `409` aturan 5 | **Terpetakan, belum diuji** | `statusKunjunganIgd is Completed or Cancelled` |
| 4 | Pengguna tanpa data dokter `403` (aturan 4) | **Terpetakan, belum diuji** | Penjaga lama, tetap paling awal |
| 5 | Regresi rawat inap dan poliklinik tidak berubah | **Terpetakan lewat diff** | Cabang IGD hanya aktif bila `EmgVisit` ada; cabang rawat inap tidak disentuh |
| 6 | Diff satu berkas, hanya `ValidateMedicalAssessmentRuleAsync`; nol komentar baru | **Terpenuhi dengan catatan** | Satu berkas; tambahan `using` dan dua konstanta pesan (bagian 3.3) |
| 7 | Build 0 error; warning dilaporkan | **Belum** | Build milik Rizki |

**Definition of Done:** laporan tracked ✅; register, node grafik R3.16, dan traceability ditandai 🟡; QBE preflight dan
checker ✅; tanpa UAT PASS ✅. **Belum:** build (7) dan uji 1–5 pada putaran 1.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nihil dari checker |
| Masalah yang diketahui | (1) Delta acceptance 6 (bagian 3.3). (2) `QBE-SVC-001` dan `QBE-NAM-001` untuk kode lama, tidak ditangani |
| Keadaan migration | Nol |
| Eksekusi database | Nol |
| Risiko tersisa | Rendah–sedang: controller dipakai rawat inap; cabang baru hanya aktif bila encounter punya `EmgVisit`. Pasien IGD yang kemudian dirawat inap memakai encounter rawat inap baru, sehingga tidak masuk cabang ini |
| Perubahan sampingan | `NONE`. Working tree `BE-IGD-065` dan dokumen bukan hasil task ini |
| Interupsi | `NONE` |
| Langkah berikutnya | `build-module-frontend` `FE-IGD-046` (pasangan, dikerjakan sesudah ini); build backend oleh Rizki |

`git status --short` (source backend) di akhir pekerjaan:

```text
 M Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs
 M Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyVisitDtos.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDoctorAssignmentService.cs
```

Tiga berkas `EmergencyInstallationManagement` milik `BE-IGD-065`, bukan task ini.
