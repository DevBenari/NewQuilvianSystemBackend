# Laporan Perubahan Backend — `BE-SEC-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-003` |
| Judul | Technical Permission Granularity Hardening — pilot Dokter Rawat Jalan |
| Slice | Integritas registry permission dan resolusi izin efektif |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — bagian *Task implementasi berikutnya* |
| Trace | `SEC-REQ-013`, `SEC-REQ-014`, `SEC-REQ-021`; keputusan `D-ARCH-3`, `D-ARCH-6`, **`D-ARCH-7`**, `D-ARCH-8`, `D-ARCH-10`, `O-1`; aturan dokumen `RM-DEC-004` |
| Contract version | `blueprint-manifest.md` revisi 2, `approved` 2 September 2026 |
| Dependency | `BE-SEC-002` `CLOSED`. Fase A (`85fcc3fd`, 4 September 2026) sudah ada di HEAD |
| Klasifikasi | `HEAVY` (skor 11) — sesuai roadmap; laporan ini mencakup Fase A′ saja |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackendAndryZain` — source otorisasi, test, `docs/module-blueprints/platform-authorization/`, `Migrations/scripts/` (skrip diagnostik baca-saja) |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `8131913380063aee7f2ebe2bda54e888d88b2b02` (`AndryZain`) |
| Tanggal | 11 September 2026 |
| Status | 🟡 **`PARTIAL`** — Fase A dan A′ selesai; Fase B dan C belum dijalankan; gerbang test terblokir di luar kendali task ini |

---

## 1. Masalah yang diperbaiki

Task `BE-SEC-003` memecah izin teknis yang terlalu kasar. Sebelum dipecah, satu izin `Update`
membuka banyak endpoint yang maknanya jauh berbeda — sehingga admin tidak dapat memberikan
kewenangan kecil tanpa ikut memberikan kewenangan besar.

Revalidasi [`evidence/05`](../../../evidence/05-be-sec-003-current-head-revalidation.md) menemukan
bahwa **sebagian besar pemecahan itu sudah dikerjakan** pada 4 September (commit `85fcc3fd`,
Fase A), dan menyisakan tiga hal yang diperbaiki laporan ini:

**Pertama — menulis SOAP masih menumpang izin penyuntingan biasa.**
`PATCH /doctor-consultations/{id}/soap` dan `PUT /doctor-consultations/{id}` sama-sama memakai
`DoctorConsultation.Update`. Akibatnya bagi pengguna: petugas yang hanya perlu merapikan data
header konsultasi — jam, ruang, dokter — otomatis juga berhak **menulis isi rekam medis pasien**.
Keputusan `D-ARCH-7` sudah menyatakan keduanya kemampuan berbeda, tetapi Fase A belum
memisahkannya.

**Kedua — daftar kewenangan yang diterbitkan ke layar lebih longgar daripada penjaganya.**
Endpoint yang memberi tahu frontend "tombol apa saja yang boleh ditampilkan" lupa menyaring
penempatan organisasi yang **sudah dibatalkan**. Akibatnya bagi pengguna: seseorang yang
penempatannya dibatalkan tetap melihat tombol yang tampak aktif, lalu ditolak dengan pesan
"Anda tidak memiliki akses" begitu ditekan. Menyesatkan, dan membuat petugas mengira sistemnya
rusak.

**Ketiga — kemampuan baru `PatientAssessment.Amend` belum jelas siapa pemiliknya.** Ditambahkan
tim lain di luar himpunan Fase A, terdaftar tetapi belum diberikan kepada siapa pun.

---

## 2. Proses bisnis

### 2.1 Menulis SOAP dan menyelesaikan konsultasi

| Hal | Isi |
| --- | --- |
| **Tujuan** | Memisahkan kewenangan menulis isi rekam medis dari kewenangan merapikan data header konsultasi |
| **Pelaku** | Dokter yang memeriksa pasien; admin yang mengatur hak akses |
| **Pemicu** | Dokter membuka ruang kerja konsultasi dari antrean |

Langkah berurutan:

1. Dokter memanggil pasien, lalu membuka konsultasi.
2. Dokter menulis SOAP — subjektif, objektif, asesmen, rencana. Layar menyimpan otomatis setiap
   beberapa detik lewat `PATCH /{id}/soap`. **Mulai sekarang langkah ini menuntut
   `DoctorConsultation.WriteSoap`.**
3. Bila perlu, data header konsultasi dirapikan lewat `PUT /{id}` — menuntut
   `DoctorConsultation.Update`.
4. Setelah seluruh tab lengkap, konsultasi diselesaikan lewat `PATCH /{id}/complete` — menuntut
   `DoctorConsultation.Complete`.
5. Bila konsultasi batal, `PATCH /{id}/cancel` — menuntut `DoctorConsultation.Cancel`.

Jalur tidak normal:

- Petugas memegang `Update` tetapi tidak memegang `WriteSoap` → dapat merapikan header, **ditolak**
  saat menulis SOAP. Inilah pemisahan yang diminta `D-ARCH-7`.
- Petugas memegang `WriteSoap` tetapi belum diberi izin oleh admin sampai Fase B dijalankan →
  ditolak `403`. Ini konsekuensi yang disengaja: kemampuan baru **tidak pernah** otomatis diberikan,
  sesuai invarian `AccessMenuSeeder` yang tidak pernah membuat `SysAccessPolicy`.

Hasil akhir: empat kemampuan yang benar-benar terpisah pada satu layar konsultasi.

### 2.2 Koreksi pengkajian yang sudah final

| Hal | Isi |
| --- | --- |
| **Tujuan** | Memperbaiki isi pengkajian yang sudah dikunci, tanpa menghapus jejak isi aslinya |
| **Pelaku** | Penulis asli pengkajian |
| **Pemicu** | Penulis menyadari ada yang salah setelah pengkajian difinalkan |

Langkah berurutan:

1. Pengkajian diselesaikan. Sistem mencatat **siapa penulisnya** ke daftar keutuhan dokumen dan
   mengunci isinya.
2. Dokumen yang sudah terkunci **tidak dapat disunting langsung**. Satu-satunya jalur adalah
   menambahkan koreksi, lewat `POST /patient-assessments/{id}/addendums`.
3. Sistem memeriksa dua gerbang berurutan:
   - **Gerbang izin** — apakah pengguna memegang `PatientAssessment.Amend`;
   - **Gerbang data** — apakah pengguna benar-benar penulis asli dokumen itu.
4. Koreksi disimpan sebagai catatan tambahan bernomor. **Isi aslinya tidak diubah**, sehingga
   pembaca berikutnya melihat keduanya bersebelahan.

Jalur tidak normal:

- Dokumen belum final → *"Catatan ini belum final. Perbaiki langsung pada catatannya."*
- Dokumen sudah dibatalkan → *"Catatan ini sudah dibatalkan dan tidak dapat dikoreksi."*
- Pengguna bukan penulis asli → *"Hanya penulis catatan yang dapat menambahkan koreksi."*
  **Ditolak walaupun pengguna memegang izin `Amend`.**
- Penulis asli berhalangan atau akunnya nonaktif → koreksi ditempuh lewat endpoint berbeda milik
  modul Rekam Medis, dengan izin berbeda (`ClinicalNoteAddendum.CreateAsSubstitute`).

Contoh berangka: seorang perawat memegang `PatientAssessment.Amend` dan mencoba mengoreksi
pengkajian yang ditulis rekannya. Gerbang izin lolos, gerbang data menolak. Hasilnya `403`.
Karena itu memberikan `Amend` secara luas **tidak** membuat siapa pun dapat mengubah pekerjaan
orang lain.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas/dokumen | Alasan diperiksa |
| --- | --- |
| `AGENTS.md`, `rules/backend/role-access-rules.md`, `REPORT_TEMPLATE.md`, `lokasi-laporan-task.md`, `status-task-roadmap.md` | Governance preflight dan kontrak penamaan atribut |
| `evidence/03`, `evidence/04`, `evidence/05`, `blueprint-manifest.md`, `roadmap/backend-roadmap.md` | Kontrak target, keputusan owner, dan hasil revalidasi |
| `Attributes/AccessPermissionAttribute.cs`, `Filters/AccessPermissionFilter.cs`, `Services/Security/PermissionRegistryDescriptor.cs`, `Seeders/AccessMenuSeeder.cs` | Memastikan jalur identitas dan lifecycle registry |
| `Areas/.../DoctorConsultationController.cs` | Sasaran koreksi `WriteSoap` |
| `Services/Security/AccessPermissionService.cs` | Sasaran perbaikan paritas `IsCancel` |
| `Areas/.../PatientAssessmentController.cs`, `ClinicalNoteAddendumService.cs`, `ClinicalDocumentIntegrityService.cs`, `ClinicalNoteAddendumController.cs`, `Enums/PatientAssessmentType.cs`, `Enums/ClinicalDocumentKind.cs`, `Models/IdentityModel.cs` | Analisis kewenangan `Amend` dan akar masalah blocker Billing |
| `Tests/.../Security/PermissionSplitPreparationTests.cs`, `CanonicalSecurityContractTests.cs`, `BillingManagement/AccessPermissionEnforcementTests.cs` | Himpunan identitas terkunci dan regression test |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Dua baris atribut pada `UpdateSoap`: `[AccessAction]` argumen ke-1 dan `[AccessPermission]` argumen ke-2 diubah `Update` → `WriteSoap`. **Body method tidak disentuh** |
| `Services/Security/AccessPermissionService.cs` | Satu baris: `&& !organization.IsCancel` ditambahkan pada predikat kelayakan penempatan di `GetEffectivePermissionsAsync`, menyamakannya dengan `HasAccessAsync` |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/AccessPermissionEnforcementTests.cs` | Regression test `GetEffectivePermissions_ExcludesCancelledOrganizationAssignment`; helper `AssignUserToOrganizationAsync` diberi parameter opsional `isCancel = false` |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/Security/PermissionSplitPreparationTests.cs` | `("DoctorConsultation", "WriteSoap")` ditambahkan ke himpunan terkunci (22 → 23); pemetaan endpoint `UpdateSoap` diperbarui; `WriteSoap` ditambahkan ke contoh kemampuan sensitif |
| `Migrations/scripts/diagnose-be-sec-003-permission-state.sql` | **Baru.** Skrip diagnostik **baca-saja** untuk mengukur keadaan permission sebenarnya sebelum Fase B |
| `docs/module-blueprints/platform-authorization/evidence/06-be-sec-003-readiness-correction.md` | **Baru.** Bukti koreksi, analisis kewenangan `Amend`, dan laporan blocker Billing |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| **Kontrak API** | Route, verb, payload, dan respons **tidak berubah**. Yang berubah hanya izin yang dituntut `PATCH /doctor-consultations/{id}/soap`: `DoctorConsultation.Update` → `DoctorConsultation.WriteSoap`. Frontend tidak perlu diubah |
| **Database** | **Tidak ada perubahan skema. Tidak ada EF migration. Tidak ada penulisan data.** Satu baris registry baru (`DoctorConsultation.WriteSoap`) akan dibuat `AccessMenuSeeder` saat aplikasi start berikutnya — itu perilaku seeder yang sudah ada, bukan migration. `SysAccessPolicy` **tidak disentuh sama sekali** |
| **Keamanan/Auth** | Dua perbaikan. **(1)** Menulis SOAP kini kemampuan tersendiri, memenuhi `D-ARCH-7`. Sampai Fase B dijalankan, `WriteSoap` **ditolak untuk semua orang** kecuali SuperAdmin — *fail closed*, bukan *fail open*. **(2)** Daftar kewenangan efektif tidak lagi lebih longgar daripada penjaganya pada penempatan yang dibatalkan. Tidak ada pelebaran hak bagi siapa pun |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/health-services/clinical-management/doctor-consultations/{id}` | Merapikan data header konsultasi | `DoctorConsultation : Update` |
| `PATCH` | `/api/v1/health-services/clinical-management/doctor-consultations/{id}/soap` | Menyimpan otomatis isi SOAP konsultasi | **`DoctorConsultation : WriteSoap`** *(berubah)* |
| `PATCH` | `/api/v1/health-services/clinical-management/doctor-consultations/{id}/complete` | Memvalidasi dan menyelesaikan konsultasi | `DoctorConsultation : Complete` |
| `PATCH` | `/api/v1/health-services/clinical-management/doctor-consultations/{id}/cancel` | Membatalkan konsultasi berjalan | `DoctorConsultation : Cancel` |

Endpoint `[AllowAnonymous]` pada task ini: `NONE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Kontrak penamaan atribut — argumen ke-2 `[AccessPermission]` = argumen ke-1 `[AccessAction]`, pada seluruh controller pilot | **Nol ketidakcocokan** | `PASS` | Pemeriksaan otomatis atas 7 controller; keempat identitas `DoctorConsultation` cocok huruf demi huruf |
| Argumen ke-1 `[AccessPermission]` = `ControllerName` pada `[AccessController]` | Cocok — `DoctorConsultation` | `PASS` | Pembacaan source |
| `AccessType` termasuk empat nilai yang sah | `AccessTypes.Update` | `PASS` | Pembacaan source |
| `git diff` terbatas pada yang diizinkan | 4 berkas, 37 baris tambah, 4 hapus | `PASS` | `git diff --stat`; body method tidak tersentuh |
| `dotnet build QuilvianSystemBackend.csproj` | **Berhasil tanpa error** | `PASS` | `QuilvianSystemBackend.dll` dibangun ulang pukul 10:46, sesudah seluruh suntingan source (10:29). Keluaran verbositas `quiet` kosong — tanda tidak ada error maupun warning yang dilaporkan |
| Kompilasi berkas yang diubah task ini | **Bersih — nol galat** | `PASS` | Pembangunan proyek test melaporkan 14 galat; **tidak satu pun** menyebut `DoctorConsultationController.cs`, `AccessPermissionService.cs`, `AccessPermissionEnforcementTests.cs`, atau `PermissionSplitPreparationTests.cs` |
| `dotnet build` proyek test `UnitTests.InMemory` | **`Build FAILED`** — 14 galat pada 2 berkas | `EXISTING / ENVIRONMENT ISSUE` | `BillingDepositServiceTests.cs` (11 galat `CS0117`) dan `PatientEncounterCompanyGuarantorTests.cs` (3 galat `CS8410`/`CS1061`/`CS0117`). Keduanya di luar modul otorisasi. Lihat `evidence/06` bagian D |
| `dotnet test --filter "FullyQualifiedName~Security"` | **Tidak dapat dijalankan** — nol test berjalan | `EXISTING / ENVIRONMENT ISSUE` | Eksekusi berhenti di tahap build; tidak ada baris `Passed!` maupun `Failed!` |
| Regression test `IsCancel` | **`NOT RUN`** | `NOT RUN` | Kodenya **terkompilasi bersih**, tetapi tidak dapat dieksekusi karena proyek test secara keseluruhan gagal dibangun |

Uji manual: `NOT FEASIBLE` — menuntut akses database development yang ditolak pembatas izin pada
sesi ini, dan menuntut `Security:Authorization:Enabled = true` pada lingkungan uji.

**Tidak dijalankan, beserta alasannya:**

- **Perluasan `SysAccessPolicy` (Fase B)** — dilarang eksplisit oleh instruksi pemilik sistem pada
  tahap ini.
- **Penyempitan audio antrean (Fase C)** — dilarang eksplisit; `QueueVoiceController` tidak disentuh.
- **Query ke database development** — akses ditolak pembatas izin harness. Skrip baca-saja
  disiapkan sebagai gantinya, untuk dijalankan pemilik sistem.
- **Perbaikan `BillingDepositServiceTests.cs`** — dilarang eksplisit; milik tim Billing.

### 5.1 Hasil `dotnet build`

**Proyek utama: berhasil.** `dotnet build QuilvianSystemBackend.csproj -v q` selesai tanpa satu
pun error atau warning yang dilaporkan, dan menghasilkan `QuilvianSystemBackend.dll` baru pukul
10:46:07 — sesudah suntingan source terakhir pukul 10:29:32. Kedua perubahan source task ini
karena itu terbukti kompilasi.

Build-nya memang lama (sekitar 15 menit, puncak pemakaian memori di atas 22 GB). Itu sifat
proyek ini, bukan akibat perubahan task ini: riwayat commit memuat
`hotfix/backend-0.4.0-docker-memory` dan `fix(build): use SkipMigrationMetadata for Docker build`
untuk alasan yang sama.

**Proyek test: `Build FAILED`,** 14 galat pada dua berkas milik dua modul lain. Rinciannya di
`evidence/06` bagian D. Yang penting untuk task ini: **nol galat pada keempat berkas yang diubah
task ini**, sehingga regression test `IsCancel` terbukti valid secara sintaksis dan semantik —
hanya belum dapat dieksekusi.

---

## 6. Acceptance criteria dan Definition of Done

Kriteria diambil persis dari roadmap. Task ini **belum** memenuhi seluruhnya.

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **1.** Technical permission split selesai — identitas terdaftar dan terbaca layar Akses Role; identitas lama tertutup tanpa hard delete; `PatientProcedure.Create` tetap aktif dan `Select` dibuat | **Terpenuhi di source** | Fase A (`85fcc3fd`) + Fase A′ (laporan ini). 23 identitas baru, 5 bertahan, 2 pensiun. Penutupan memakai lifecycle, bukan hard delete |
| **2.** Legacy parity terverifikasi — himpunan endpoint terjangkau identik sebelum dan sesudah; pasangan tetap 11 | **Belum terpenuhi** | Menuntut Fase B dan pengukuran database. Belum dijalankan |
| **3.** Tidak ada privilege broadening | **Terpenuhi sejauh ini** | Nol baris `SysAccessPolicy` dibuat. Kemampuan baru *fail closed* |
| **4.** Tidak ada silent privilege loss — 6 pengguna diverifikasi satu per satu | **Belum terpenuhi** | Menuntut Fase B. `evidence/05` bagian O menilai kehilangan hak `HIGHLY LIKELY` sudah terjadi akibat Fase A masuk tanpa Fase B |
| **5.** Migrasi teruji — mode laporan lebih dulu, hasil mode tulis sama dengan laporannya, perluasan idempoten | **Belum terpenuhi** | Fase B belum disusun |
| **6.** Rollback teruji | **Belum terpenuhi** | Menyusul Fase B |
| **7.** `ReconcileNeverCreatesAccessPolicy` tetap hijau | **Tidak dapat dibuktikan** | Gerbang test terblokir. Secara source, `AccessMenuSeeder` tidak disentuh dan tetap tidak menulis `SysAccessPolicy` |
| **8.** `CompatibilityFallbackMatchesApprovedLegacySetExactly` diperbarui secara sadar — jumlah tetap 69 | **Belum berlaku** | Baru relevan pada Fase C. `QueueVoiceController` tidak disentuh, sehingga himpunan fallback tidak berubah |
| **9.** Otorisasi audio terbukti **OR**, bukan AND | **Belum terpenuhi** | Fase C belum dijalankan |
| **10.** Tiga test SuperAdmin tetap hijau; seluruh test suite lulus; `has-pending-model-changes` bersih; smoke test akun non-SuperAdmin | **Tidak dapat dibuktikan** | Gerbang test terblokir blocker Billing |

**Definition of Done yang belum terpenuhi, disebut apa adanya:** migrasi data (Fase B), bukti
parity per Departemen × Posisi, bukti rollback, seluruh butir yang menuntut eksekusi test, dan
penyempitan audio (Fase C).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Peringatan** | `git diff` menampilkan peringatan `LF will be replaced by CRLF`. Diperiksa: blob `HEAD` dan salinan kerja sama-sama LF, sehingga tidak ada perubahan akhiran baris yang nyata |
| **Masalah yang diketahui** | **(1)** Proyek test `UnitTests.InMemory` tidak dapat dikompilasi karena **dua** berkas milik modul lain: `BillingDepositServiceTests.cs` (11 galat `CS0117`, tim Billing) dan `PatientEncounterCompanyGuarantorTests.cs` (3 galat, tim Registration Management) — seluruh gerbang test keamanan ikut mati. **(2)** Kehilangan hak akibat Fase A masuk tanpa Fase B kemungkinan besar sudah berjalan; belum terukur. **(3)** `PatientAssessment.Amend` belum punya pemilik |
| **Risiko tersisa** | Sampai Fase B dijalankan, `DoctorConsultation.WriteSoap` ditolak untuk seluruh dokter. Ini **menambah satu kemampuan** ke daftar yang harus dipulihkan Fase B — aritmetikanya sudah diperbarui di `evidence/06` bagian F.1 (34 → 35 baris) |
| **Perubahan sampingan** | `NONE` |
| **Interupsi** | `NONE` |
| **Status Git** | Lihat bagian 7.1 |
| **Langkah berikutnya** | Jalankan skrip diagnostik baca-saja; minta tim Billing memperbaiki blocker; tutup keputusan cakupan `Amend`; baru susun Fase B. Rincian di `evidence/06` bagian H |

### 7.1 Status Git

```text
 M Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs
 M Services/Security/AccessPermissionService.cs
 M Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/AccessPermissionEnforcementTests.cs
 M Tests/QuilvianSystemBackend.UnitTests.InMemory/Security/PermissionSplitPreparationTests.cs
 M docs/module-blueprints/platform-authorization/roadmap/backend-roadmap.md
 M docs/module-blueprints/platform-authorization/roadmap/requirement-traceability.md
?? Migrations/scripts/diagnose-be-sec-003-permission-state.sql
?? docs/module-blueprints/platform-authorization/evidence/05-be-sec-003-current-head-revalidation.md
?? docs/module-blueprints/platform-authorization/evidence/06-be-sec-003-readiness-correction.md
?? docs/module-blueprints/platform-authorization/task/report/backend/BE-SEC-003.md
```

Tidak ada berkas Billing, Patient Encounter, migration EF, maupun `QueueVoiceController` yang
tersentuh. Tidak ada `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deploy yang
dijalankan pada task ini.
