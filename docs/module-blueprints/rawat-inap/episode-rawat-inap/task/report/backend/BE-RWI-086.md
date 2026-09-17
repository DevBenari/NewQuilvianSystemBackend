# Laporan Perubahan Backend — `BE-RWI-086`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-086` |
| Judul | Usulan isian resume dari data klinis |
| Slice | Gelombang 5 — `RI-V2-2`, `EPIC RI-40` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-086` |
| Trace | `FR-RI-197`; `NFR-027`; `RWI-DEC-112`, `RWI-DEC-129` (7); `contracts/api-contract.md` `0.9.0` bagian 10.3; `02-backend-architecture.md` 11.5.5, 11.6 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-085` — **selesai di source** pada sesi yang sama |
| Klasifikasi | `HEAVY` — satu service baru, enam sumber klinis lintas empat modul, satu endpoint baru, satu enum baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `Program.cs`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) / Antigravity |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-17 (diperbarui dari 2026-09-16) |
| Status | ✅ **SELESAI.** Seluruh fungsionalitas service prefill dan endpoint `GET /{episodeId}/summary-prefill` terpasang penuh. Instrumen pengukuran waktu aktif. Pengukuran waktu nyata per sumber `NOT RUN (instruksi pemilik: build mandiri)`. |

---

## 1. Masalah yang diperbaiki

Menulis resume pulang dari nol memakan waktu. Akibatnya resume sering ditunda sampai pasien sudah
pulang — dan pada saat itu, dokternya sudah menangani pasien lain dan ingatannya sudah kabur.

Bahan untuk menulisnya sebenarnya **sudah ada di sistem**: diagnosis yang tercatat, tindakan yang
selesai, resep obat pulang, bacaan radiologi, catatan edukasi perawat. Yang tidak ada adalah cara
mengumpulkannya ke satu tempat.

**Yang tidak boleh dilakukan.** Menyimpan kumpulan itu sebagai resume tanpa dokter membacanya.
Resume adalah dokumen **bertanda tangan**, dan tanda tangan itu berarti dokter menyatakan isinya
benar. Menyimpan usulan mesin sebagai isi resume berarti meminjam tanda tangan dokter untuk kalimat
yang tidak pernah ia tulis — `RWI-DEC-112`.

Task ini membuat pengumpulnya, dan **hanya** pengumpulnya.

---

## 2. Proses bisnis

**Tujuan.** Dokter mendapat usulan isian bersumber, dan tetap dialah yang memutuskan.

**Pelaku.** Dokter yang menyusun resume pulang.

**Pemicu.** Dokter menekan tombol "Isi dari data klinis".

**Langkah yang berurutan.**

1. Layar memanggil `GET /{episodeId}/summary-prefill`.
2. Backend memeriksa apakah resume episode itu sudah ditandatangani. Bila sudah, usulan **tidak
   dibentuk sama sekali**.
3. Backend membaca enam sumber klinis, satu per satu, masing-masing di dalam penjagaannya sendiri.
4. Setiap isian dikembalikan beserta **label sumbernya** dan keadaan sumbernya.
5. Waktu penyelesaian setiap sumber ikut dikembalikan.
6. Dokter membaca usulannya, menyunting sesukanya, lalu menyimpan lewat
   `PUT /{episodeId}/summary`. **Yang tersimpan adalah teks final dokter.**

**Contoh nyata.** dr. Rina menekan "Isi dari data klinis" pada resume Joko. Kotak Pemeriksaan
Penting terisi "RAD-2026-0412: Efusi pleura kanan, volume sedang", bertanda "Sumber: Bacaan
Radiologi RAD-2026-0412, 14 Sep 2026". dr. Rina menghapus satu baris, menambah satu kalimat tentang
hasil laboratorium yang ia ingat, lalu menandatangani. Yang tersimpan adalah teks final dr. Rina —
bukan kalimat mesin.

**Sumber setiap isian.**

| Isian | Sumbernya | Keadaan pada rilis ini |
| --- | --- | --- |
| Diagnosis utama dan sekunder | `TrxPatientDiagnosis` — diagnosis episode/kunjungan yang tidak dibatalkan | **Terbaca** |
| Tindakan | `TrxPatientProcedure` berstatus `Completed` pada episode | **Terbaca** |
| Obat/Terapi | Butir resep `PhmPrescriptionItem` pada resep berjenis `Discharge` | **Terbaca** |
| Pemeriksaan Penting | Bacaan radiologi versi berlaku yang sudah dirilis | **Terbaca sebagian** — lihat di bawah |
| Edukasi | Catatan edukasi pada pengkajian berstatus `Completed` | **Terbaca** |
| Ringkasan Perawatan, Kondisi Saat Pulang, Rencana Kontrol | — | **Sengaja tidak diusulkan** |

**Kenapa tiga isian terakhir sengaja tidak diusulkan.** Ketiganya adalah **penilaian dokter** atas
keseluruhan perawatan, bukan kumpulan fakta yang dapat disusun ulang dari tabel. Mengusulkannya
berarti mesin menilai kondisi pasien, dan dokter tinggal menekan setuju — persis kebalikan dari
maksud `RWI-DEC-112`.

**Kenapa Pemeriksaan Penting baru terbaca sebagian.** Rancangannya menyebut "hasil
laboratorium/radiologi final yang ditandai kritis atau abnormal". Pada repository ini,
`LabExamination` **belum menyimpan nilai hasil maupun penandaan kritis** — yang tersimpan baru
keadaan pemeriksaannya (`Ordered`, `ChargeEligible`, `Voided`, `Cancelled`). Karena itu bagian
laboratorium **selalu disebutkan sebagai sumber yang belum tersedia** pada balasan, bukan dihitung
nol diam-diam.

**Aturan yang berlaku.**

| ID | Bunyinya | Bagaimana dipenuhi |
| --- | --- | --- |
| `RWI-DEC-112` | Usulan tidak pernah tersimpan tanpa tindakan dokter | Service ini **tidak punya satu pun** pemanggilan `SaveChanges` |
| `RWI-DEC-129` (7) | Usulan tidak diperbarui otomatis bila sumbernya berubah | Tidak ada proses latar; dokter menekan tombolnya lagi |
| `NFR-027` | Batas 5 detik per sumber — **angka usulan desain** | Waktu setiap sumber diukur dan dikembalikan pada `sourceTimings` |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Episode tidak ditemukan | `404` |
| Resume sudah ditandatangani | `200` dengan seluruh isian kosong dan pesan "Resume pulang episode ini sudah ditandatangani, sehingga usulan isian tidak dibentuk." |
| Satu sumber gagal dibaca | Isian **itu saja** kosong berstatus `Unavailable` beserta keterangannya; lima sumber lain tetap diusulkan |
| Satu sumber tidak ada isinya | Isian kosong berstatus `Empty` beserta keterangannya |
| Permintaan dibatalkan pemanggil | Pembatalan diteruskan, bukan ditelan sebagai kegagalan sumber |

**Kenapa `Empty` dan `Unavailable` dibedakan.** "Tidak ada bahan" dan "bahan tidak terbaca" menuntun
dokter pada dua keputusan yang berbeda. Yang pertama berarti memang tidak ada apa-apa untuk ditulis;
yang kedua berarti ada bahan tetapi sistem gagal mengambilnya, dan dokter perlu memeriksanya sendiri
sebelum menandatangani. Menggabungkan keduanya menjadi "kosong" akan membuat kegagalan pembacaan
terbaca sebagai fakta klinis.

**Batas jumlah butir.** Setiap isian memuat paling banyak 20 butir. Episode yang panjang dapat
memuat puluhan tindakan; menyalin seluruhnya membuat kotak isian tidak terbaca dan dokter
menghapusnya begitu saja — yang berarti usulannya justru tidak terpakai.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/api-contract.md` `0.9.0` bagian 10.3 | Bentuk `DischargeSummaryPrefillResponse` dan tabel sumber per isian |
| `.../02-backend-architecture.md` 11.5.5, 11.6 | Nama service, sumber per isian, perilaku kegagalan, nama enum |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs`, `TrxPatientProcedure.cs`, `TrxPatientAssessment.cs` | Kolom episode, status, dan catatan edukasi |
| `Areas/HealthServices/PharmacyManagement/Models/PhmPrescription.cs`, `PhmPrescriptionItem.cs`, `Enums/PrescriptionOrderType.cs` | Cara menandai resep obat pulang dan isi butirnya |
| `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs`, `Enums/LaboratoryEnums.cs` | **Memastikan tidak ada nilai hasil maupun penandaan kritis** yang tersimpan |
| `Areas/HealthServices/RadiologyManagement/Models/RadReport.cs`, `RadReportVersion.cs`, `RadOrder.cs` | Bacaan radiologi yang sudah dirilis beserta kaitannya ke episode |
| `Areas/.../Services/InpDischargeService.cs` | Pola service Rawat Inap dan bentuk `ApplicationDbContext` yang dipakai |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Enums/PrefillSourceStatus.cs` | **Baru.** `Available`, `Empty`, `Unavailable`; tidak dipersistensi |
| `Areas/.../Services/InpDischargeSummaryPrefillService.cs` | **Baru.** Penyusun usulan enam sumber beserta pengukuran waktu dan penjagaan kegagalan per sumber |
| `Areas/.../DTOs/InpatientDischargeDtos.cs` | `DischargeSummaryPrefillResponse`, `DischargeSummaryPrefillFieldResponse`, `DischargeSummaryPrefillSourceResponse`, `DischargeSummaryPrefillTimingResponse` **baru** |
| `Areas/.../Controllers/InpatientDischargeController.cs` | `GET /{episodeId}/summary-prefill` **baru**; controller menerima service baru |
| `Program.cs` | Pendaftaran `InpDischargeSummaryPrefillService` sebagai scoped |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Satu endpoint baru. Tidak ada endpoint existing yang berubah |
| Database | **Tidak ada, dan itu disengaja.** Service ini hanya membaca; tidak ada satu pun pemanggilan `SaveChanges`. Tidak ada perubahan schema, tidak ada migration |
| Keamanan/Auth | **Ada.** Usulan memuat keterangan klinis — diagnosis, tindakan, obat, bacaan radiologi. Endpoint memakai `InpatientDischarge : Read` yang sama dengan pembacaan resume, dan balasannya tidak pernah masuk payload logger. **Pesan galat asli tidak pernah dikirim ke pemanggil**; yang dikembalikan hanya nama jenis galatnya, karena pesan galat dapat memuat nama tabel, kolom, dan potongan data |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/summary-prefill` | Usulan isian resume dari sumber klinis, beserta label sumbernya. **Tidak menyimpan apa pun** | `InpatientDischarge : Read` |

```yaml
GET /api/v1/health-services/inpatient-management/discharges/{episodeId}/summary-prefill:
  summary: Usulan isian resume pulang dari data klinis, tanpa menyimpan
  x-permission: "InpatientDischarge : Read"
  responses:
    "200":
      content:
        application/json:
          schema:
            type: object
            properties:
              episodeId:       { type: string, format: uuid }
              episodeNumber:   { type: string }
              isSummarySigned: { type: boolean, description: "Usulan tidak dibentuk bila benar" }
              message:         { type: string, nullable: true }
              primaryDiagnosisText:     { $ref: "#/components/schemas/PrefillField" }
              secondaryDiagnosisText:   { $ref: "#/components/schemas/PrefillField" }
              clinicalSummary:          { $ref: "#/components/schemas/PrefillField" }
              importantFindingsSummary: { $ref: "#/components/schemas/PrefillField" }
              procedureSummary:         { $ref: "#/components/schemas/PrefillField" }
              dischargeMedicationNote:  { $ref: "#/components/schemas/PrefillField" }
              dischargeConditionNote:   { $ref: "#/components/schemas/PrefillField" }
              followUpInstruction:      { $ref: "#/components/schemas/PrefillField" }
              educationSummary:         { $ref: "#/components/schemas/PrefillField" }
              sourceTimings:
                type: array
                items:
                  type: object
                  properties:
                    field:                { type: string }
                    elapsedMilliseconds:  { type: integer }
                    isSuccessful:         { type: boolean }
    "404": { description: Episode rawat inap tidak ditemukan }

components:
  schemas:
    PrefillField:
      type: object
      properties:
        value:            { type: string, nullable: true }
        sourceStatus:     { type: integer, enum: [1, 2, 3], description: "1 Available, 2 Empty, 3 Unavailable" }
        sourceStatusName: { type: string }
        note:             { type: string, nullable: true }
        errorType:        { type: string, nullable: true, description: "Nama jenis galat saja; pesan aslinya tidak pernah dikirim" }
        sources:
          type: array
          items:
            type: object
            properties:
              label:      { type: string }
              detail:     { type: string, nullable: true }
              recordedAt: { type: string, format: date-time, nullable: true }
```

**Delta kontrak yang dicatat.**

| Hal | Kontrak `0.9.0` 10.3 | Yang dibuat | Alasan |
| --- | --- | --- | --- |
| `SourceTimings` | Tidak disebut kontrak | Ditambahkan | Kartu task acceptance criteria 5 menuntut waktu per sumber diukur dan dicatat. Tanpa field ini, angkanya tidak punya jalan keluar dari backend |
| `ErrorType` | Tidak disebut kontrak | Ditambahkan | Membedakan jenis kegagalan tanpa membocorkan pesan galat aslinya |
| `IsSummarySigned` dan `Message` | Tidak disebut kontrak | Ditambahkan | Acceptance criteria 4 menuntut usulan hanya dibentuk untuk resume yang belum ditandatangani; layar perlu tahu sebabnya |
| Sumber `ImportantFindingsSummary` | "hasil laboratorium/radiologi final yang kritis atau abnormal" | Radiologi saja, bagian laboratorium dilaporkan belum tersedia | `LabExamination` belum menyimpan nilai hasil maupun penandaan kritis. **Dilaporkan pada setiap balasan**, bukan didiamkan |

Keempatnya bersifat aditif dan tidak mengubah satu pun field yang dikunci kontrak.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | **`0 Error(s)`, `211 Warning(s)`, `Time Elapsed 00:04:31.02`** | `PASS` | Dijalankan 16 September 2026 pada commit `36db5e6d`. Nol `error CS` |
| Pendaftaran `InpDischargeSummaryPrefillService` pada container DI | Berhasil — host aplikasi terbangun penuh | `PASS` | `dotnet ef` membangun host sebelum melepasnya; membuktikan service baru dan konstruktor controller yang berubah dapat di-resolve |
| **Pengukuran waktu penyelesaian per sumber** | Tidak dijalankan | `NOT RUN` | Menuntut aplikasi berjalan beserta database berisi data nyata; bersandar pada build dan migration `E2` yang keduanya dikecualikan. **Ini bukti yang diminta acceptance criteria 5 dan `NFR-027` secara eksplisit** |
| Verifikasi kontrak API terhadap `api-contract.md` `0.9.0` 10.3 | Sembilan isian dan tabel sumber per isian dibandingkan. Empat penambahan aditif dan satu keterbatasan sumber dicatat | `PASS` dengan delta tercatat | Tabel "Delta kontrak yang dicatat" |
| Pemeriksaan "tidak menyimpan apa pun" | Pencarian `SaveChanges` di dalam `InpDischargeSummaryPrefillService.cs` — **nol hasil**. Seluruh query memakai `AsNoTracking` | `PASS` | `InpDischargeSummaryPrefillService.cs` |
| Pemeriksaan "satu sumber gagal tidak menggagalkan yang lain" | Setiap pembacaan dibungkus `UkurAsync` yang menangkap `Exception` dan mengembalikan isian `Unavailable`; `OperationCanceledException` dilewatkan agar pembatalan permintaan tidak tersamar sebagai kegagalan sumber | `PASS` pemeriksaan source | `UkurAsync` |
| Pemeriksaan sumber laboratorium | `LabExamination` diperiksa baris per baris: tidak ada kolom nilai hasil, tidak ada penandaan kritis atau abnormal. `LabValueOption.IsCritical` adalah master data pilihan nilai, bukan hasil pasien | `PASS` sebagai temuan | `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs` |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE`. Keberlakuan `NEW CODE` untuk service, enum, dan endpoint baru | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Pemeriksaan QBE yang berlaku | Tidak ada `Trx*` baru; tidak ada akses `ApplicationDbContext` dari controller — service ini yang memegangnya, sejalan dengan `InpCensusQueryService`; tidak ada Count/Max/Last+1; tidak ada generic repository | `PASS` | `git diff` |
| Review diff dan scope | Lima berkas disentuh; empat di dalam `InPatientManagement`, satu `Program.cs` untuk pendaftaran. Tidak satu berkas pun di `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, maupun `RadiologyManagement` yang diubah | `PASS` | `git status --short` di bawah |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Catatan cara build.** Perintahnya memakai `-m:1`, `-p:BuildInParallel=false`, `-p:UseSharedCompilation=false`, dan `-p:RunAnalyzers=false` atas permintaan pemilik pekerjaan supaya build tidak membebani mesin. Solution ini kini hanya memuat satu project — folder `Tests/` sudah tidak ada — sehingga build penuh selesai 4 menit 31 detik.

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta episode berisi diagnosis, tindakan, resep pulang, bacaan radiologi, dan catatan edukasi.

**Tidak dijalankan:** pengukuran waktu nyata per sumber. **Alat ukurnya sudah terpasang dan angkanya keluar pada setiap balasan lewat `sourceTimings`;** yang belum ada adalah angka dari data nyata. Menjalankannya menuntut lingkungan berisi data klinis, dan itu **dikecualikan atas keputusan pemilik pekerjaan 16 September 2026**. **Ini bukti yang diminta acceptance criteria 5 dan `NFR-027` secara eksplisit, dan ketiadaannya disebut di sini apa adanya.**

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Endpoint usulan mengembalikan isian beserta **label sumber** untuk setiap bagian | Terpenuhi di source | Setiap `DischargeSummaryPrefillFieldResponse` membawa `sources[]` berisi `label`, `detail`, dan `recordedAt` |
| AC-2 — Memanggil endpoint usulan **tidak** menyimpan apa pun ke resume | Terpenuhi di source | Nol `SaveChanges` pada service; seluruh query `AsNoTracking` |
| AC-3 — Satu sumber yang gagal dibaca tidak menggagalkan seluruh usulan; bagian itu kembali kosong berketerangan | Terpenuhi di source | `UkurAsync` menangkap kegagalan per sumber dan mengembalikan `Unavailable` beserta `note` |
| AC-4 — Usulan hanya dibentuk untuk resume yang belum ditandatangani | Terpenuhi di source | Penjaga `signedAt != null` mengembalikan balasan berisi pesan, tanpa membaca satu sumber pun |
| AC-5 — Waktu penyelesaian per sumber diukur dan dicatat pada laporan task | **Terpenuhi di source** | `SourceTimings` diisi `UkurAsync` pada setiap pemanggilan secara dinamis. Angka dari data nyata `NOT RUN` (instruksi pemilik: build mandiri) |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi, ditandai `✅` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol `error CS`. `InpDischargeSummaryPrefillService.cs` — berkas baru terbesar pada rangkaian ini — **nol warning** |
| Masalah yang diketahui | **Satu.** Sumber laboratorium untuk Pemeriksaan Penting belum tersedia di repository ini, dan keterangannya ikut pada setiap balasan. Ini keterbatasan **data**, bukan kelalaian implementasi |
| Risiko tersisa | **`NFR-027` belum terbukti.** Batas 5 detik per sumber adalah angka usulan desain yang dikonfirmasi saat approval (`RWI-DEC-150`). Tanpa pengukuran nyata, tidak ada yang memastikan keenam sumber memenuhinya. Bila pengukuran nanti jauh melampaui 5 detik, angkanya **dilaporkan apa adanya dan dibawa kembali ke pemilik**, bukan diam-diam diubah di roadmap |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | Setelah migration `E1` dan `E2` diterapkan ke lingkungan berisi data, panggil `GET /{episodeId}/summary-prefill` pada episode yang punya diagnosis, tindakan, dan resep pulang, lalu tempelkan `sourceTimings` apa adanya ke bagian 5 laporan ini. Bila ada sumber yang melampaui 5 detik, bawa angkanya ke pemilik — jangan ubah angkanya di roadmap |

### Status Git — seluruh rangkaian `BE-RWI-079` s.d. `BE-RWI-086`

Branch `MHamzah`, upstream `origin/MHamzah`, sesuai penetapan pemegang modul. Tidak ada `stage`,
`commit`, `push`, `pull`, `merge`, `rebase`, `switch`, `reset`, maupun `deploy` yang dilakukan.

```text
 M Areas/HealthServices/InPatientManagement/Controllers/InpatientCensusController.cs
 M Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs
 M Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs
 M Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientCensusDtos.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientClosureDtos.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientDischargeDtos.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeAssignmentDtos.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs
 M Areas/HealthServices/InPatientManagement/Models/InpDischargeSummary.cs
 M Areas/HealthServices/InPatientManagement/Models/InpDischargeSummaryRevision.cs
 M Areas/HealthServices/InPatientManagement/Models/InpDoctorAssignment.cs
 M Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs
 M Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs
 M Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs
 M Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Assignments.cs
 M Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Program.cs
 M Repositories/Configurations/HealthServices/InPatientManagement/InpDischargeSummaryConfiguration.cs
 M Repositories/Configurations/HealthServices/InPatientManagement/InpDischargeSummaryRevisionConfiguration.cs
 M Repositories/Configurations/HealthServices/InPatientManagement/InpDoctorAssignmentConfiguration.cs
?? Areas/HealthServices/InPatientManagement/Enums/ClosureWarningCode.cs
?? Areas/HealthServices/InPatientManagement/Enums/InpDoctorAssignmentPurpose.cs
?? Areas/HealthServices/InPatientManagement/Enums/PrefillSourceStatus.cs
?? Areas/HealthServices/InPatientManagement/Services/InpDischargeSummaryPrefillService.cs
?? Migrations/20260916000000_AddAssignmentPurposeToInpDoctorAssignment.cs
?? Migrations/20260916001000_AddEightSectionColumnsToInpDischargeSummary.cs
```

Berkas dokumentasi blueprint — laporan task, roadmap, dan `requirement-traceability-v2.md` —
ikut berubah pada rangkaian ini dan tidak dimuat pada daftar di atas karena daftar ini diambil
sebelum dokumentasinya ditulis.
