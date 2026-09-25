# Laporan Perubahan Backend — `BE-HMD-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-05` |
| Judul | Pengaturan Kebijakan Unit HD dan Penentuan Butir Persiapan Overridable |
| Slice | `MVP-1` — Pengelolaan Sumber Daya dan Kesiapan Unit HD |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.2 |
| Trace | `FR-HMD-041`, `FR-HMD-051`, `NFR-011`, `HMD-ASM-001`, `HMD-GATE-002`, `HMD-DEC-010`, `HMD-DEC-011`; `contracts/api-contract.md` grup Settings & Checklist Items |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-03` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — tiga acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-OPT-001`, `QBE-CODE-004` |

---

## 1. Masalah yang diperbaiki

Kebijakan unit HD — batas pasien per perawat, masa berlaku hasil uji air, dan butir checklist
mana yang boleh dilewati dokter — harus bisa diubah tanpa rilis ulang aplikasi. Tanpa task ini,
keputusan Komite Medis yang melonggarkan satu butir checklist baru bisa berlaku setelah
programmer mengubah kode dan aplikasi dirilis ulang.

---

## 2. Proses bisnis

**Pengaturan unit.**

1. Admin unit membuka pengaturan unit HD (`GET /{serviceUnitId}`), lalu menyimpan perubahan
   (`PUT /{serviceUnitId}`). Batas isian: rasio perawat 1–50, masa berlaku air 1–87.600 jam,
   toleransi mulai 0–1.440 menit; tindakan yang dipilih harus ada dan aktif di `MstProcedure`.
2. Nilai dibaca langsung dari database setiap kali dibutuhkan, sehingga berlaku pada pernyataan
   berikutnya tanpa restart.

**Contoh berangka.** `WaterResultValidityHours` diubah dari 720 menjadi 1440 jam (60 hari).
Uji air terakhir 1 Agustus. Pernyataan siap Shift Pagi 10 September — umur hasil 40 hari — yang
sebelumnya ditolak kini diterima (`BE-HMD-06`).

**Butir boleh-dilewati.**

1. Setiap butir baru selalu dibuat `IsOverridable = false` — `POST` tidak menerima nilai itu.
2. Pemegang hak `HemodialysisChecklistItem : SetOverridable` memanggil
   `PATCH /{id}/overridable` dengan `{ "isOverridable": true, "clinicalGovernanceNote":
   "Keputusan Komite Medis No. 42" }`.
3. Catatan tata kelola **wajib**; tanpa catatan ditolak `400`. Catatan, pelaku, dan waktu server
   tersimpan pada butir itu (`OverridableDecisionNote/DecidedByUserId/DecidedAt`) dan satu baris
   logger aplikasi ditulis.
4. Nilai baru langsung terbaca gerbang override `BE-HMD-12`.

**Jalur tidak normal.** Pengguna tanpa hak `SetOverridable` ditolak `403` oleh
`[AccessPermission]` sebelum service dipanggil. Siapa pemegang hak itu ditentukan admin lewat
layar Akses Role — tidak ada role yang di-hardcode.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Settings & Checklist, `permission-audit-matrix.md`, `data-dictionary.md` bagian 3.18 dan 3.22
- `00-interview-decisions.md` `HMD-ASM-001`, `HMD-DEC-010`, `HMD-DEC-011`, `HMD-GATE-002`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdResourceService.cs` | Bagian pengaturan (baris 725–810) dan butir checklist (baris 812–985) |
| `Controllers/HmdSettingController.cs` | Baru, 2 endpoint |
| `Controllers/HmdChecklistItemController.cs` | Baru, 8 endpoint |
| `DTOs/HmdResourceDtos.cs` | `UpdateHmdSettingRequest` dengan `[Range]`, `SetOverridableRequest`, request/response butir checklist |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 6 endpoint kontrak terpenuhi. **Delta**: (a) baca baseline butir checklist `filters/metadata`, `summary`, `options`, `{id}`; (b) **sengaja tanpa** `DELETE` dan `PATCH /{id}/status` untuk butir checklist, karena menghapus atau menonaktifkan butir pengaman adalah keputusan tata kelola klinis (`HMD-GATE-002`) dan matriks hak akses tidak menyediakan butirnya; (c) pengaturan unit hanya `GET`/`PUT` per unit — satu baris per unit, sehingga sembilan endpoint baseline master data tidak berlaku |
| Database | Tidak ada perubahan schema |
| Keamanan/Auth | `HemodialysisSetting : Read/Update`; `HemodialysisChecklistItem : Read/Create/Update/SetOverridable` |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Master Data / Hemodialysis Setting

Base: `/api/v1/health-services/hemodialysis-management/master-data/hemodialysis-settings`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{serviceUnitId}` | Pengaturan kebijakan unit HD | `HemodialysisSetting : Read` |
| `PUT` | `/{serviceUnitId}` | Menyimpan pengaturan; baris dibuat bila belum ada | `HemodialysisSetting : Update` |

#### Health Services / Hemodialysis Management / Master Data / Hemodialysis Checklist Item

Base: `/api/v1/health-services/hemodialysis-management/master-data/hemodialysis-checklist-items`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan filter layar butir checklist | `HemodialysisChecklistItem : Read` |
| `GET` | `/summary` | Jumlah butir, butir wajib, dan butir boleh-dilewati | `HemodialysisChecklistItem : Read` |
| `GET` | `/` | Daftar utuh butir menurut urutan pemeriksaan | `HemodialysisChecklistItem : Read` |
| `GET` | `/options` | Butir aktif untuk pilihan | `HemodialysisChecklistItem : Read` |
| `GET` | `/{id}` | Rincian satu butir beserta catatan keputusan tata kelolanya | `HemodialysisChecklistItem : Read` |
| `POST` | `/` | Menambah butir; selalu tidak boleh dilewati (`201`) | `HemodialysisChecklistItem : Create` |
| `PUT` | `/{id}` | Memperbarui nama, kategori, wajib, urutan, dan status aktif | `HemodialysisChecklistItem : Update` |
| `PATCH` | `/{id}/overridable` | Menetapkan boleh-dilewati dengan catatan tata kelola wajib | `HemodialysisChecklistItem : SetOverridable` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis; `SetOverridable` terpisah dari `Update` | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source `SetChecklistItemOverridableAsync` | Catatan wajib, pelaku, dan waktu server disimpan | `PASS` | `HmdResourceService.cs` baris 951–982 |
| Pemeriksaan source pembacaan pengaturan air | `DeclareReadyAsync` membaca `WaterResultValidityHours` langsung dari DB | `PASS` | `HmdUnitReadinessService.cs` baris 296–313 |
| Uji runtime HTTP (termasuk `403`) | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `ChecklistPolicyUpdateTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `PATCH …/overridable` dengan catatan Komite Medis → `IsOverridable = true` dan tercatat jejaknya | Terpenuhi | Jejak disimpan pada butir itu sendiri dan logger aplikasi `HemodialysisChecklistItem.SetOverridable` |
| 2. `WaterResultValidityHours` menjadi 1440 tersimpan dan langsung memengaruhi kedaluwarsa air shift berikutnya | Terpenuhi | `UpdateSettingAsync` + pembacaan langsung pada `DeclareReadyAsync` |
| 3. Tanpa hak `SetOverridable` → `403` | Terpenuhi (source) | `[AccessPermission("HemodialysisChecklistItem", "SetOverridable")]`; runtime `NOT RUN` |
| DoD: endpoint sesuai Swagger | Terpenuhi | Audit akses |
| DoD: lulus pengujian otorisasi dan audit log | Pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | Diganti pemeriksaan source |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Perlu keputusan pemilik.** Pemegang `HemodialysisChecklistItem : Update` dapat menjadikan butir tidak wajib atau nonaktif lewat `PUT /{id}` tanpa catatan tata kelola. Efeknya setara melonggarkan pemeriksaan keselamatan. Kontrak tidak membatasinya dan desain ini tertulis sengaja pada controller, tetapi bertentangan dengan semangat *fail-closed* `HMD-DEC-010`. Pilihan: (a) hak `Update` hanya diberikan ke tata kelola klinis lewat layar Akses Role; atau (b) task lanjutan yang menolak pelonggaran lewat `PUT` |
| Masalah yang diketahui | Sakelar `AllowMultipleActiveEpisodePerPatient = true` dapat disimpan, tetapi tidak berlaku karena unique index bersyarat pada kamus data — lihat `BE-HMD-08` |
| Risiko tersisa | Pemegang akun tata kelola klinis masih *acting* (`HMD-DEC-011`); penunjukan badan klinis adalah gerbang go-live `HMD-GATE-002` |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Pemilik memutuskan pilihan (a)/(b) di atas; admin memberi hak `SetOverridable` hanya kepada tata kelola klinis |
