# Laporan Perubahan Backend — `BE-RWI-085`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-085` |
| Judul | Resume pulang delapan bagian (migration E2) |
| Slice | Gelombang 4 — `RI-V2-2`, `EPIC RI-40` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-085` |
| Trace | `FR-RI-196`; `RWI-DEC-112`, `RWI-DEC-057`; `RWI-RULE-032`; `data/data-dictionary.md` 18.2, 18.3, 18.4; `02-backend-architecture.md` 11.5.2, 11.8; `contracts/api-contract.md` `0.9.0` bagian 10.3 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-084` — **sebagian** (bentuk kontraknya lengkap; dua angka akibat menunggu slice modul lain). Tidak menahan task ini: keduanya tidak bersinggungan di source |
| Klasifikasi | `MEDIUM` — enam kolom pada dua tabel, satu migration dua arah, tiga isian melalui baca/simpan/versi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `Repositories/Configurations/**`, `Migrations/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-16 |
| Status | **Selesai di source.** `dotnet build`, verifikasi skema, dan uji migration **`NOT RUN`** atas permintaan pemilik pekerjaan |

---

## 1. Masalah yang diperbaiki

Resume pulang adalah dokumen yang **dibawa pasien** ke fasilitas kesehatan berikutnya. Dokter
penerima membacanya untuk mengetahui apa yang terjadi selama perawatan.

Tiga bagian yang justru paling dibaca dokter penerima selama ini **tidak ada tempatnya**:

| Bagian | Kenapa penting bagi dokter penerima |
| --- | --- |
| **Pemeriksaan Penting** | Di sinilah terbaca **kenapa** sebuah tindakan diambil — bukan pada daftar tindakannya |
| **Kondisi Saat Pulang** | Garis dasar untuk menilai apakah pasien membaik atau memburuk sesudah pulang |
| **Edukasi** | Apa yang **sudah** dijelaskan, sehingga tidak diulang atau justru terlewat |

Sebelum task ini, ketiganya hanya dapat ditulis dengan menumpuknya ke dalam kotak Ringkasan
Perawatan — bercampur dengan hal lain, dan tidak dapat ditampilkan sebagai bagian tersendiri di
layar maupun di cetakan.

---

## 2. Proses bisnis

**Tujuan.** Resume pulang memuat delapan bagian, termasuk tiga yang selama ini tidak ada.

**Pelaku.** DPJP aktif episode — `GUARD-INP-03`.

**Pemicu.** Dokter menyusun resume pulang sebelum pasien meninggalkan rumah sakit.

**Delapan bagian dan tempat menyimpannya.**

| No | Bagian di layar | Kolom | Keadaan |
| ---: | --- | --- | --- |
| 1 | Diagnosis | `PrimaryDiagnosisText`, `SecondaryDiagnosisText` | Sudah ada |
| 2 | Ringkasan Perawatan | `ClinicalSummary` | Kolom lama, **label baru** |
| 3 | **Pemeriksaan Penting** | `ImportantFindingsSummary` | **Baru** |
| 4 | Tindakan | `ProcedureSummary` | Sudah ada |
| 5 | Obat/Terapi | `DischargeMedicationNote` | Sudah ada |
| 6 | **Kondisi Saat Pulang** | `DischargeConditionNote` | **Baru** |
| 7 | Rencana Kontrol | `FollowUpInstruction`, `ReferralDestination` | Sudah ada |
| 8 | **Edukasi** | `EducationSummary` | **Baru** |

**Langkah yang berurutan.**

1. Dokter membuka resume lewat `GET /{episodeId}/summary`. Ketiga isian baru ikut terbaca.
2. Ia menyimpan draf lewat `PUT /{episodeId}/summary`. Ketiga isian baru ikut tersimpan.
3. Ia menandatangani resume. Isinya terkunci.
4. Bila kelak ada koreksi lewat sesi koreksi, **salinan versi lama disimpan lebih dulu** —
   termasuk ketiga isian baru.

**Kenapa ketiganya nullable.** Resume yang sudah ditandatangani adalah rekam medis; ia tidak dapat
diisi ulang oleh migration. Membuat kolomnya `NOT NULL` berarti memilih antara dua hal yang
sama-sama salah: mengarang isi klinis, atau menolak seluruh resume lama terbaca. Isi minimal resume
berada di bawah gerbang pemilik klinis — `RWI-RULE-032` dan `04-prd-to-mvp.md` 22.7 nomor 3 —
sehingga kewajiban isian ditetapkan di sana, bukan di dalam kode.

**Kenapa ketiganya juga lahir di tabel revisi.** Tabel revisi menyimpan salinan isi resume sebelum
digantikan. Bila ketiga kolom hanya ada di tabel induk, setiap penandatanganan ulang lewat sesi
koreksi akan **menghapus** Pemeriksaan Penting, Kondisi Saat Pulang, dan Edukasi versi lama tanpa
jejak — persis kebalikan dari gunanya tabel revisi, `RWI-DEC-057`.

**Contoh berangka.** Resume Joko ditandatangani 16 September 2026 pukul 10.00 dengan
`ImportantFindingsSummary` berisi "Hb 7,8 g/dL (12/09) → transfusi 2 kolf". Pada 18 September
supervisor membuka sesi koreksi, dan dokter memperbaikinya menjadi "Hb 7,8 → 10,2 g/dL (12/09 →
15/09) setelah transfusi 2 kolf". Baris revisi nomor 1 menyimpan **kalimat yang pertama** beserta
nama penandatangan lama dan waktunya. Keduanya tetap terbaca.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Resume lama tanpa ketiga isian | Tetap terbaca; ketiganya bernilai kosong. Tetap dapat ditandatangani |
| Isian melebihi batas panjang | Ditolak validasi DTO — `4000` untuk Pemeriksaan Penting, `2000` untuk dua lainnya |
| Migration mundur ketika ketiganya masih kosong | Berjalan; keenam kolom dibuang |
| Migration mundur ketika sudah ada yang terisi | **Ditolak** dengan pesan yang menyebut jumlah barisnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `data/data-dictionary.md` 18.2, 18.3, 18.4 | Nama kolom, tipe, panjang, sensitivitas, dan DDL-nya |
| `.../02-backend-architecture.md` 11.5.2, 11.8 | Pemetaan delapan bagian PRD dan urutan migration `E2` |
| `contracts/api-contract.md` `0.9.0` bagian 10.3 | Endpoint yang membawa ketiga isian |
| `Areas/.../Models/InpDischargeSummary.cs`, `InpDischargeSummaryRevision.cs` | Bentuk entity saat ini |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpDischargeSummary*Configuration.cs` | Pola konfigurasi panjang kolom |
| `Areas/.../Services/InpDischargeService.cs` — `GetSummaryAsync`, `ApplySummaryContent`, `AddRevisionSnapshotAsync` | Tiga tempat yang wajib ikut membawa isian baru |
| `Migrations/20260911000000_AddAssignmentRoleToInpDoctorAssignment.cs` | Pola migration tulis-tangan beserta penjaga rollback-nya |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Models/InpDischargeSummary.cs` | Tiga kolom nullable baru |
| `Areas/.../Models/InpDischargeSummaryRevision.cs` | Tiga kolom nullable yang sama |
| `Repositories/.../InpDischargeSummaryConfiguration.cs` | Panjang ketiga kolom |
| `Repositories/.../InpDischargeSummaryRevisionConfiguration.cs` | Panjang ketiga kolom |
| `Migrations/20260916001000_AddEightSectionColumnsToInpDischargeSummary.cs` | **Baru.** Migration `E2` maju dan mundur |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Enam kolom baru |
| `Areas/.../DTOs/InpatientDischargeDtos.cs` | Ketiga isian pada request simpan, response baca, dan response revisi |
| `Areas/.../Services/InpDischargeService.cs` | Ketiga isian pada proyeksi baca, salinan versi, dan penerapan isi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Tiga isian pada `GET`, `PUT`, dan pada setiap baris revisi. Tidak ada field existing yang berubah nama, tipe, maupun sifat wajibnya. `ClinicalSummary` **tidak berubah bentuk**; yang berubah hanya labelnya di layar |
| Database | **Ada.** Enam kolom `varchar` nullable pada dua tabel. Migration `20260916001000_AddEightSectionColumnsToInpDischargeSummary` **sudah dibuat, belum dijalankan ke database mana pun.** Menjalankannya adalah wewenang terpisah |
| Keamanan/Auth | **Ada.** Ketiga kolom bertanda **SENSITIF** — memuat keterangan klinis. Tidak boleh masuk payload logger maupun endpoint daftar mana pun; keduanya sudah dipenuhi karena resume hanya muncul pada endpoint detail resume. Tidak ada perubahan atribut akses |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/summary` | Membaca resume pulang. **Berubah:** bertambah tiga isian | `InpatientDischarge : Read` |
| `PUT` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/summary` | Menyusun atau memperbarui resume. **Berubah:** bertambah tiga isian | `InpatientDischarge : Update` |
| `PATCH` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/summary/sign` | Menandatangani resume. Tidak berubah bentuknya; ketiga isian ikut terkunci bersama isi lainnya | `InpatientDischarge : Sign` |

```yaml
PUT /api/v1/health-services/inpatient-management/discharges/{episodeId}/summary:
  requestBody:
    content:
      application/json:
        schema:
          type: object
          required: [primaryDiagnosisText]
          properties:
            primaryDiagnosisText:     { type: string, maxLength: 1000 }
            secondaryDiagnosisText:   { type: string, maxLength: 2000, nullable: true }
            clinicalSummary:          { type: string, maxLength: 4000, nullable: true, description: "Label layar: Ringkasan Perawatan" }
            importantFindingsSummary: { type: string, maxLength: 4000, nullable: true, description: "BARU — Pemeriksaan Penting. SENSITIF" }
            procedureSummary:         { type: string, maxLength: 2000, nullable: true }
            dischargeMedicationNote:  { type: string, maxLength: 2000, nullable: true }
            dischargeConditionNote:   { type: string, maxLength: 2000, nullable: true, description: "BARU — Kondisi Saat Pulang. SENSITIF" }
            followUpInstruction:      { type: string, maxLength: 2000, nullable: true }
            referralDestination:      { type: string, maxLength: 250, nullable: true }
            educationSummary:         { type: string, maxLength: 2000, nullable: true, description: "BARU — Edukasi. SENSITIF" }
```

**Catatan lintas sub-modul.** Kontrak yang sama dipakai tab Resume Medis `FE-DOK-12` milik
`dokter-rawat-inap` — `FR-DOK-107`. Karena ketiga isian ditambahkan pada DTO yang **sama**, bukan
pada bentuk kedua, payload-nya tidak dapat berbeda antara dua permukaan itu.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan pemilik pekerjaan pada permintaan task ini |
| Verifikasi skema pada Postgres sekali pakai | Tidak dijalankan | `NOT RUN` | Menuntut migration dijalankan, dan itu bersandar pada build |
| Verifikasi kontrak API | Ketiga isian dibandingkan dengan `api-contract.md` `0.9.0` 10.3 dan `data-dictionary.md` 18.2–18.3: nama, panjang, sifat nullable, dan sensitivitasnya sama persis | `PASS` | Bandingkan bagian 4 dengan kedua dokumen |
| Pemeriksaan kesesuaian DDL | Nama kolom, tipe, dan panjangnya sama persis dengan DDL pada `data-dictionary.md` 18.4 | `PASS` | `Migrations/20260916001000_AddEightSectionColumnsToInpDischargeSummary.cs` |
| Pemeriksaan tiga tempat yang wajib ikut | (1) proyeksi `GetSummaryAsync` untuk resume **dan** revisi, (2) `ApplySummaryContent`, (3) `AddRevisionSnapshotAsync` — ketiganya membawa ketiga isian | `PASS` | `git diff` `InpDischargeService.cs` |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE`. Keberlakuan `TOUCHED LEGACY` | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Pemeriksaan QBE yang berlaku | `QBE-MOD-002` tidak menahan; `QBE-DB-001` dan `QBE-DB-002` diperhatikan — migration dibuat, eksekusi database dinyatakan terpisah | `PASS` | Bagian 3.3 |
| Review diff dan scope | Delapan berkas disentuh; tidak ada satu pun di luar `InPatientManagement`, `Repositories/Configurations`, dan `Migrations` | `PASS` | `git status --short` |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database yang sudah dimigrasi.

**Tidak dijalankan:** `dotnet build`, verifikasi skema, dan uji migration maju/mundur. Seluruhnya
dikecualikan pemilik pekerjaan yang menyatakan akan menjalankan build sendiri.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Ketiga kolom ada pada `InpDischargeSummary` dan `InpDischargeSummaryRevision`, seluruhnya nullable | Terpenuhi di source | Enam `AddColumn<string>(... nullable: true)` pada migration; `[MaxLength]` tanpa `[Required]` pada kedua model; snapshot diperbarui |
| AC-2 — Resume lama tanpa ketiga isian tetap terbaca dan tetap dapat ditandatangani | Terpenuhi di source | Kolomnya nullable dan tidak satu pun penjaga penandatanganan memeriksanya. `SignSummaryAsync` tidak disentuh task ini |
| AC-3 — Perubahan pada resume tersimpan bervers sebagai revisi, termasuk ketiga isian baru | Terpenuhi di source | `AddRevisionSnapshotAsync` menyalin `ImportantFindingsSummary`, `DischargeConditionNote`, dan `EducationSummary` bersama isi lainnya |
| AC-4 — Migration mundur menghapus kolom selama kosong | Terpenuhi di source | `Down()` membuang keenam kolom, didahului blok `DO $$ ... RAISE EXCEPTION` yang menolak rollback bila ada baris terisi dan menyebut jumlahnya |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi |
| Verifikasi skema pada Postgres sekali pakai | **Belum terpenuhi** — `NOT RUN`, dikecualikan pemilik pekerjaan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` yang dapat dipastikan — compiler tidak dijalankan |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | **Kolom belum ada di database mana pun.** Sampai migration `E2` dijalankan, `GET` dan `PUT` resume gagal pada runtime karena proyeksinya menyebut kolom yang belum ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-086`. Branch `MHamzah`, upstream `origin/MHamzah`. Tidak ada operasi Git yang dilakukan |
| Langkah berikutnya | Pemilik menjalankan build; setelah hijau, jalankan migration `E2` maju dan mundur pada container Postgres sekali pakai lalu tempelkan keluarannya ke bagian 5. Selaraskan tab Resume Medis `FE-DOK-12` agar memakai payload yang sama |
