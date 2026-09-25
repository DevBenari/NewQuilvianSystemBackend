# Laporan Perubahan Backend — `BE-HMD-16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-16` |
| Judul | Penghentian atau Penyelesaian Sesi, Penilaian Pasca-HD, dan Pengajuan Dokumentasi Perawat |
| Slice | `MVP-5` — Penutupan Sesi, Pengesahan Medis, Rekam Medis, dan Penagihan |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.6 |
| Trace | `FR-HMD-070`, `FR-HMD-071`, `FR-HMD-081`, `CAP-06`, `CAP-35`, `HMD-DEC-012`; `contracts/api-contract.md` grup Session (`complete`, `stop`, `submit-documentation`); `state-transition-matrix.md` bagian 3; `HMD-VAL-060`, `HMD-VAL-061`, `HMD-VAL-070` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-14` ✅, `BE-HMD-15` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — dua acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE`; mengubah `TrxPatientProcedure.IsBillable` sesi yang dihentikan |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |

---

## 1. Masalah yang diperbaiki

"Selesai mencuci darah" dan "catatannya selesai" adalah dua hal berbeda. Tanpa pemisahan itu,
sesi yang dihentikan karena syok bisa tetap menagih jasa HD penuh, dan dokter bisa diminta
mengesahkan catatan yang berat badan akhirnya belum diisi.

---

## 2. Proses bisnis

**Mengakhiri cuci darah — dua jalur dari `InProgress`:**

| Jalur | Endpoint | Syarat | Hasil |
| --- | --- | --- | --- |
| Selesai normal | `POST /{id}/complete` | Penilaian Pasca-HD berisi berat badan, dan tujuan pasien terisi; bila tidak → `422 HMD-VAL-060` | `Completed`, `EndedAt` = waktu server, `EndedByUserId` |
| Dihentikan | `POST /{id}/stop` | Alasan penghentian wajib (`400 HMD-VAL-061`): ketidakstabilan hemodinamik, pembekuan sirkuit, masalah akses vaskular, pasien menolak, kerusakan mesin, atau lainnya, beserta catatan | `Stopped`, `EndedAt` = waktu server; `TrxPatientProcedure.IsBillable = false` dengan catatan alasan; status serah terima Billing `NotRequired` |

**Penilaian Pasca-HD** (`PUT /{id}/post-hd`) — boleh saat `InProgress`, `Completed`, atau
`Stopped`: berat badan setelah HD, jumlah cairan yang benar-benar ditarik, target tercapai atau
tidak, kondisi umum, kondisi akses, tanda vital akhir (ditulis ke `TrxPatientVitalSign`), dan
tujuan pasien (pulang, kembali ke bangsal, pindah ke IGD, lainnya). Nilainya tidak pernah disalin
dari penilaian Pra-HD.

**Pengajuan dokumentasi** (`POST /{id}/submit-documentation`) dari `Completed` atau `Stopped`:

1. Sistem memeriksa isian minimum: waktu selesai, jumlah cairan yang ditarik, tujuan pasien,
   berat badan setelah tindakan, dan tanda vital akhir. Yang kurang disebut satu per satu →
   `422 HMD-VAL-070`.
2. Lolos → `AwaitingFinalization`, `DocumentedByUserId` = perawat yang mengajukan,
   `DocumentedAt` = waktu server. Fase keperawatan terkunci: aksi tulis perawat pada sesi ini
   ditolak sampai dokter mengesahkan atau mengembalikannya (`BE-HMD-17`).

**Contoh sesi dihentikan.** Pasien hipotensi refrakter setelah 45 menit. Perawat memanggil `stop`
dengan alasan `HemodynamicInstability` dan catatan "Syok intradialitik refrakter cairan". Sesi
`Stopped`, tindakan pasien langsung bertanda tidak dapat ditagih, dan tidak ada tagihan jasa HD
yang akan diserahkan ke Billing.

**Contoh pemisahan pelaku.** Perawat Rina memanggil `submit-documentation`; `DocumentedByUserId`
berisi akun Rina dan sesi menunggu pengesahan dokter penanggung jawab.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Session, `state-transition-matrix.md` bagian 3, `validation-matrix.md` `HMD-VAL-060/061/070`, `integration-contract.md` bagian 6
- `TrxPatientProcedure`, `TrxPatientVitalSign`, `HmdSessionAssessment`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdSessionService.cs` | Pasca-HD, hentikan, selesaikan, ajukan dokumentasi, `FindMissingDocumentationAsync` (baris 359–398 dan 933–1075) |
| `Controllers/HmdSessionController.cs` | Endpoint stop, complete, post-hd, submit-documentation |
| `DTOs/HmdSessionDtos.cs` | Request stop, complete, dan penilaian Pasca-HD |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 4 endpoint kontrak terpenuhi. **Delta**: alasan penghentian berupa pilihan tetap (`HmdSessionStopReason`) ditambah catatan bebas, sesuai kamus data |
| Database | Tidak ada perubahan schema |
| Keamanan/Auth | `HemodialysisSession : Stop/Complete/Update/SubmitDocumentation` |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Session

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/stop` | Menghentikan cuci darah dengan alasan; tindakan menjadi tidak dapat ditagih | `HemodialysisSession : Stop` |
| `POST` | `/{id}/complete` | Menyelesaikan cuci darah normal dengan waktu server | `HemodialysisSession : Complete` |
| `PUT` | `/{id}/post-hd` | Menyimpan penilaian Pasca-HD dan tujuan pasien | `HemodialysisSession : Update` |
| `POST` | `/{id}/submit-documentation` | Perawat mengajukan dokumentasi untuk disahkan | `HemodialysisSession : SubmitDocumentation` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 12.19 WIB |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source penghentian | `Stopped`, `IsBillable = false`, `NotRequired` | `PASS` | `HmdSessionService.cs` baris 933–980 |
| Pemeriksaan source pengajuan | Isian minimum → `HMD-VAL-070`; `DocumentedByUserId`/`DocumentedAt` | `PASS` | `HmdSessionService.cs` baris 1019–1075 |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `SessionClosureAndDocumentationTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Sesi dihentikan dengan alasan → `Stopped` dan otomatis tidak dapat ditagih | Terpenuhi | `StopAsync` |
| 2. Perawat Rina mengajukan dokumentasi → `DocumentedByUserId` Rina, status `AwaitingFinalization` | Terpenuhi | `SubmitDocumentationAsync` |
| DoD: alur complete, stop, Pasca-HD, dan submit sesuai kontrak | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Sesi yang tidak pernah diajukan dokumentasinya tetap `Completed`/`Stopped`; worklist menandainya `NeedsDocumentation` dan menyediakan filternya |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji runtime oleh pemilik; `BE-HMD-17` |
