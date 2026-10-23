# Laporan Perubahan Backend — `BE-LAB-97`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-97` |
| Judul | Alasan wajib pada batal pemeriksaan dan batal wadah (`VAL-155`) |
| Slice | `S16b-1` Laporan Pembatalan — prasyarat `INV-60`; gelombang `MVP-15b` |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 6au.2 |
| Trace | `FR-20.10`; `LAB-DEC-237`, `LAB-DEC-247`; BR-148, BR-154; `INV-60`; `AC-326`, `AC-327`; `T-S16B-01`..`03` |
| Contract version | `LAB-API-v1` `r49` 44.5; `LAB-VAL-v1` `r21` `VAL-155`; `LAB-STATE-v1` `r11` 13.1 — ketiganya **`approved` 2026-10-20** (Yoga Aji Pratama) |
| Dependency | Nol |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1 (±10), berkas diubah 1 (6), logika bisnis 1 (penjaga validasi pada dua jalur batal), kontrak API 1 (memperketat endpoint yang ada), database 0, keamanan 0, UI 0 |
| Task mode | `BACKEND` — backend target tulis, frontend referensi read-only |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/LaboratoryManagement/{Constants,Services,DTOs,Controllers}`, laporan ini, baris status roadmap/traceability `MVP-15` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4d3e4f98` (branch `yoga`) + working tree `BE-LAB-96` |
| Tanggal | 2026-10-20 |
| Status | ✅ **Selesai** — harness 15/15, build lulus, `422` terbukti pada aplikasi berjalan dengan nol tulis |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — Laboratory, **`ACTIVE`** |
| Keberlakuan | `NEW CODE` (`Constants/LabCancellationReason.cs`); `TOUCHED LEGACY` (`LabExaminationService`, `LabSpecimenService`, dua DTO, `LabSpecimenController`) |
| QBE yang berlaku | `QBE-VAL-001` (validasi invarian bisnis — alasan wajib), `QBE-API-001` (`ApiResponse`, kode `422` yang sudah dipetakan), `QBE-SVC-001` (penjaga di service, bukan controller), `QBE-DEL-001` (lifecycle batal beserta jejak aktor dan alasan), `QBE-MOD-001`, `QBE-NAM-001` |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*` — nol entity, configuration, migration; `QBE-PERM-001` tidak berubah — izin batal tetap `LabExamination : Update` dan `LabSpecimen : Cancel` |
| Branch | `yoga` |

---

## 1. Masalah yang diperbaiki

Pembatalan **pesanan** sudah wajib beralasan (`VAL-74`), tetapi pembatalan **pemeriksaan** dan **wadah** tidak: alasan
kosong diterima dan tersimpan kosong. Laporan Pembatalan (`S16b-1`) kelak akan memuat baris tanpa alasan, dan jejak audit
tidak dapat menjawab *mengapa* sebuah pemeriksaan dibatalkan.

**Contoh sebelum perubahan:** integrasi memanggil batal Kolesterol dengan `reason: ""` → `200`, Kolesterol batal, riwayat
mencatat alasan kosong.

## 2. Proses bisnis

1. Pemanggil — integrasi, alat, atau modul lain; **layar Laboratorium sengaja tidak punya aksi batal per pemeriksaan
   maupun per wadah** (`VAL-13`, `LAB-DEC-247`) — mengirim batal pemeriksaan atau batal wadah.
2. Backend memeriksa **keadaan lebih dulu**: pemeriksaan gugur atau sudah batal → `409`; pemeriksaan sudah dirilis →
   `422` pesan rilis; wadah sudah batal → pesan keadaan yang sama seperti sebelumnya.
3. **Baru sesudah itu** alasan diperiksa: kosong, spasi, tab, atau baris baru → `422` *"Alasan pembatalan wajib diisi."* —
   bunyi dan kode yang sama dengan `VAL-74`. **Tidak ada yang ditulis.**
4. Alasan yang diisi dirapikan (spasi tepi dibuang) dan disimpan pada riwayat perpindahan (`ReasonNote`).
5. **Batal pesanan** tidak berubah: wadahnya ikut batal dengan alasan pesanan (`VAL-74` sudah menjaminnya).
6. Pembatalan lama yang tersimpan tanpa alasan **tidak diisi surut** (`INV-60`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`LabExaminationService.cs#CancelAsync` (`:259`) dan pengecualiannya; `LabSpecimenService.cs#CancelAsync` (`:1201`),
`CancelAllForOrderInMemoryAsync`, `CancelSpecimenInMemory`, `LoadSpecimenAsync`; `LabOrderService.cs#CancelAsync` (`VAL-74`,
`:1707`); `LabExaminationController.cs` dan `LabSpecimenController.cs` (pemetaan pengecualian → `422`);
`DTOs/LabExaminationDtos.cs`, `DTOs/LabSpecimenDtos.cs`; `Constants/LabReleasableDisciplines.cs` (gaya berkas konstanta);
roadmap 6au.2; kontrak `r49` 44.5, `VAL-155`, `LAB-STATE-v1` `r11`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Constants/LabCancellationReason.cs` | **Baru.** `MissingMessage` = *"Alasan pembatalan wajib diisi."* (sama dengan `VAL-74`) dan `Require(string?)` → alasan yang dirapikan, atau `null` bila kosong |
| `Areas/HealthServices/LaboratoryManagement/Services/LabExaminationService.cs` | `CancelAsync`: penjaga alasan **sesudah** penjaga `Voided`, `Cancelled`, `ReleasedAt` dan **sebelum** mutasi, melempar `LabExaminationValidationException` (`422`); perapian alasan lama diganti `Require`. `using …Constants` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` | `CancelAsync`: penjaga alasan sesudah *sudah dibatalkan*, melempar `LabSpecimenValidationException` (`422`). `CancelAllForOrderInMemoryAsync` **tidak** disentuh. `using …Constants` |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabExaminationDtos.cs` | Komentar `CancelLabExaminationRequest.Reason`: wajib, `VAL-155`, alasan sengaja tanpa `[Required]` |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabSpecimenDtos.cs` | Komentar yang sama pada `CancelLabSpecimenRequest.Reason` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabSpecimenController.cs` | `[ProducesResponseType(... Status422UnprocessableEntity)]` pada `POST /{id}/cancel` — pemetaan `422` sudah ada di `ExecuteAsync`, kini tercatat di Swagger |

**Kenapa tanpa `[Required]`:** atribut itu ditolak pengikatan bawaan ASP.NET dengan `400` dan bunyi lain, bertentangan dengan
kontrak (`422`, pesan sama dengan `VAL-74`).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Memperketat** dua endpoint, sesuai `r49` 44.5 dan 44.6: alasan kosong kini `422`. Konsumen FE: **nol** — tidak ada pemanggil (`CAP-P30-04`). Pemanggil di luar FE yang mengirim alasan kosong mulai menerima `422` |
| Database | Nol tabel, kolom, index, migration |
| Keamanan/Auth | `NOT APPLICABLE` — izin tidak berubah |

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Examination

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/laboratory-management/lab-examinations/{id}/cancel` | Membatalkan satu pemeriksaan terpesan — **alasan wajib** (`VAL-155`) | `LabExamination : Update` |

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/laboratory-management/lab-specimens/{id}/cancel` | Membatalkan sampel secara klinis — **alasan wajib** (`VAL-155`) | `LabSpecimen : Cancel` |

| Kode | Kapan | Pesan |
| --- | --- | --- |
| `422` | `reason` kosong, spasi, tab, atau baris baru — sesudah penjaga keadaan | *"Alasan pembatalan wajib diisi."* |
| `409` / `422` / `400` | Penjaga keadaan yang sudah ada (gugur, sudah batal, sudah dirilis, sampel sudah dibatalkan) — **tidak berubah** dan **didahulukan** | Pesan lama |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (server dimatikan lebih dulu) | `Build succeeded`, 0 error | `PASS` | Keluaran build |
| Harness InMemory `be97harness` — pemeriksaan: alasan `null`, `""`, `"   "`, `"\t\n "`, body `null` | `422`-jenis (`LabExaminationValidationException`) pesan sama; status tetap `Ordered`; **nol** riwayat `Examination.Cancel` | `PASS` (5) | Keluaran harness |
| Pemeriksaan dirilis tanpa alasan (`AC-327`) | Pesan **rilis** | `PASS` | Keluaran harness |
| Pemeriksaan `Voided` / `Cancelled` tanpa alasan | `409` conflict — penjaga keadaan lebih dulu | `PASS` (2) | Keluaran harness |
| Pemeriksaan beralasan `"  pasien puasa belum cukup  "` | `Cancelled`, `ReasonNote` = `"pasien puasa belum cukup"` | `PASS` | Keluaran harness |
| Wadah: alasan `null`, `"  "` | `LabSpecimenValidationException` pesan sama; status tetap `Planned`; nol riwayat | `PASS` (2) | Keluaran harness |
| Wadah sudah batal tanpa alasan | *"Sampel sudah dibatalkan."* — penjaga keadaan lebih dulu | `PASS` | Keluaran harness |
| Wadah beralasan `" salah tabung "` | `Cancelled`, alasan dirapikan | `PASS` | Keluaran harness |
| `T-S16B-03` batal pesanan beralasan | Pesanan dan wadahnya `Cancelled`, alasan wadah diwarisi; tidak ditolak `VAL-155` | `PASS` | Keluaran harness |
| `VAL-74` tetap | Pesan sama dengan `VAL-155` | `PASS` | Keluaran harness |
| **HTTP** aplikasi berjalan (Development, superadmin lewat layar): `POST /lab-examinations/{id}/cancel` pada Leukosit `LAB-RSMMC-000024` dengan `reason` `""` dan `"   "` | `422` *"Alasan pembatalan wajib diisi."*; status dibaca ulang tetap `Ordered` — **nol tulis** | `PASS` | `scratchpad/be97-http.mjs` |
| **HTTP** `POST /lab-specimens/{id}/cancel` pada wadah pesanan yang sama, `reason` `""` dan `"   "` | `422` pesan sama; status tetap `Received` — **nol tulis** | `PASS` | `scratchpad/be97-http.mjs` |
| `POST /lab-examinations/{id tidak dikenal}/cancel`, alasan kosong | `404` — pencarian tetap didahulukan | `PASS` | `scratchpad/be97-http.mjs` |

Uji manual: `NOT APPLICABLE` — tidak ada layar yang memanggil kedua endpoint (`VAL-13`).

**Tidak dijalankan:** batal **beralasan** pada devYoga — `NOT RUN` dengan sengaja, sebab akan menulis ke basis data bersama;
jalur itu dibuktikan harness.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-326` — alasan kosong/spasi pada pemeriksaan **atau** wadah → ditolak pesan dan kode sama dengan `VAL-74`; status tetap; nol riwayat | **Terpenuhi** | Harness (7 skenario) + HTTP `422` dua endpoint |
| `AC-327` — pemeriksaan dirilis tanpa alasan → pesan rilis | **Terpenuhi** | Harness |
| `T-S16B-03` — batal pesanan beralasan → wadah ikut batal tanpa ditolak | **Terpenuhi** | Harness |
| Penjaga **sesudah** penjaga keadaan | **Terpenuhi** | Harness: `Voided`, `Cancelled`, dirilis, wadah sudah batal |
| Jalur `CancelAllForOrderInMemoryAsync` tidak disentuh | **Terpenuhi** | Diff `LabSpecimenService.cs` hanya pada `CancelAsync` dan `using` |
| `ProducesResponseType 422` pada kedua endpoint | **Terpenuhi** | Pemeriksaan sudah ada; wadah ditambahkan |
| Regresi uji batal yang sudah ada | **Terpenuhi** | `VAL-74` dan batal pesanan lulus di harness; build hijau |
| Nol migration | **Terpenuhi** | — |
| Laporan `BE-LAB-97.md` | **Terpenuhi** | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server lokal BE 7184 dan FE 3000 dimatikan untuk build dan dinyalakan ulang (pengaturan sama dengan `BE-LAB-96`); berjalan sebagai proses latar sesi ini |
| Masalah yang diketahui | Wadah yang **sudah batal** menjawab `InvalidOperationException` — kodenya mengikuti pemetaan lama controller, tidak diubah task ini |
| Risiko tersisa | Rendah. Pemanggil di luar FE yang biasa mengirim alasan kosong akan menerima `422` — perubahan yang disetujui (`r49` 44.6) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |

**Status Git akhir** (`git status --short`, di luar `docs/`): task ini mengubah `LabExaminationService.cs`,
`LabSpecimenService.cs`, `LabExaminationDtos.cs`, `LabSpecimenDtos.cs`, `LabSpecimenController.cs` dan menambah
`Constants/LabCancellationReason.cs`. `LabDashboardService.cs`, `Program.cs`, `LabOrderDemandQuery.cs` milik `BE-LAB-96`.
Berkas `RadiologyManagement` milik pekerjaan lain. Nol stage, commit, atau push.

**Task berikutnya:** `BE-LAB-98` — Statistik Pesanan, `.xlsx`, pencatat cetak, izin.
