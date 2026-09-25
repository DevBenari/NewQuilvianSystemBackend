# Laporan Perubahan Backend — `BE-HMD-06`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-06` |
| Judul | Penilaian Kesiapan Unit Shift, Validasi Kelaikan Air, dan Logistik BMHP |
| Slice | `MVP-1` — Pengelolaan Sumber Daya dan Kesiapan Unit HD |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.2 |
| Trace | `FR-HMD-040`, `FR-HMD-041`, `FR-HMD-042`, `CAP-15`, `CAP-24`, `NFR-004`; `contracts/api-contract.md` grup Unit Readiness; `state-transition-matrix.md` bagian 4; `HMD-VAL-100` s.d. `HMD-VAL-103` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-04` ✅, `BE-HMD-05` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
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
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-CODE-004` |

---

## 1. Masalah yang diperbaiki

Sebelum pelayanan dimulai, koordinator harus memastikan mesin, station, air, BMHP, dan tenaga
siap. Tanpa task ini pemeriksaan itu hanya ada di kertas; sistem tidak tahu apakah hasil uji air
masih berlaku, dan sesi bisa dimulai pada shift yang sebenarnya belum laik.

---

## 2. Proses bisnis

1. Koordinator membuka lembar kesiapan untuk tanggal dan shift tertentu (`POST /`). Sistem
   menyalin 5 butir kesiapan aktif menjadi baris berstatus `NotChecked`. Satu lembar per unit,
   tanggal, dan shift — lembar kedua ditolak `409 HMD-VAL-100`.
2. Koordinator mengisi hasil tiap butir (`PUT /{id}/items`): terpenuhi / tidak / tidak berlaku,
   tanggal hasil, nomor referensi, dan catatan. Untuk butir air, sistem menampilkan batas
   berlakunya.
3. Koordinator menyatakan siap (`POST /{id}/declare-ready`). Sistem memeriksa berurutan:
   - setiap butir **wajib** harus `Met`, bila tidak → `422 HMD-VAL-101` beserta daftar butirnya;
   - butir yang mewajibkan tanggal hasil (air) harus masih berlaku menurut pengaturan unit,
     bila tidak → `422 HMD-VAL-102` beserta rinciannya;
   - bila lolos, status `Ready`, `DeclaredByUserId` = koordinator, `DeclaredAt` = waktu server.
4. Koordinator dapat menyatakan tidak siap (`POST /{id}/declare-not-ready`) dengan alasan wajib
   (`400 HMD-VAL-103` bila kosong).

**Contoh berangka.** Pengaturan 720 jam (30 hari). Uji air terakhir 1 Agustus. Pernyataan siap
Shift Pagi 10 September: batas berlaku = tengah malam 1 Agustus waktu Jakarta + 720 jam =
31 Agustus 00.00 WIB. Waktu server sudah melewatinya (umur 40 hari), sehingga ditolak `422`
dengan butir `WATER` disebut di rincian.

**Tidak siap bukan penghentian.** Menyatakan unit `NotReady` di tengah shift tidak menyentuh
satu pun sesi. Sesi yang belum dimulai tertahan pada gerbang `HMD-VAL-042` saat hendak
dinyatakan siap atau dimulai; sesi yang sudah berjalan tetap berjalan, karena menghentikannya
adalah keputusan klinis.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Unit Readiness, `state-transition-matrix.md` bagian 4, `validation-matrix.md` `HMD-VAL-100` s.d. `103`
- `HmdUnitReadiness`, `HmdUnitReadinessDetail`, `HmdReadinessItem`, `HmdSetting`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdUnitReadinessService.cs` | Baru. Daftar, rincian, buat lembar, simpan butir, nyatakan siap/tidak siap, `IsUnitReadyAsync` (dipakai gerbang sesi), `ValidUntilUtc` |
| `Controllers/HmdUnitReadinessController.cs` | Baru, 6 endpoint |
| `DTOs/HmdUnitReadinessDtos.cs` | Baru, termasuk `HmdReadinessBlockingItem` untuk rincian butir yang menahan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 6 endpoint kontrak terpenuhi. **Delta teks**: pesan kedaluwarsa memakai teks `validation-matrix.md` `HMD-VAL-102` "Hasil pemeriksaan pengolahan air sudah kedaluwarsa. Perbarui hasilnya sebelum menyatakan unit siap." — bukan teks contoh roadmap "Hasil pemeriksaan air telah kedaluwarsa." |
| Database | Tidak ada perubahan schema |
| Keamanan/Auth | `HemodialysisUnitReadiness : Read/Create/Update/DeclareReady/DeclareNotReady` |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Unit Readiness

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-unit-readiness`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar lembar kesiapan berfilter tanggal, shift, dan status | `HemodialysisUnitReadiness : Read` |
| `GET` | `/{id}` | Rincian lembar beserta hasil setiap butir dan batas berlaku air | `HemodialysisUnitReadiness : Read` |
| `POST` | `/` | Membuka lembar untuk tanggal dan shift (`201`) | `HemodialysisUnitReadiness : Create` |
| `PUT` | `/{id}/items` | Menyimpan hasil pemeriksaan butir | `HemodialysisUnitReadiness : Update` |
| `POST` | `/{id}/declare-ready` | Menyatakan unit siap; ditolak bila butir wajib belum terpenuhi atau air kedaluwarsa | `HemodialysisUnitReadiness : DeclareReady` |
| `POST` | `/{id}/declare-not-ready` | Menyatakan unit tidak siap dengan alasan wajib | `HemodialysisUnitReadiness : DeclareNotReady` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | 6 endpoint ada dengan hak akses sama persis dengan kontrak | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source gerbang siap | Butir wajib → `HMD-VAL-101`; air → `HMD-VAL-102`; waktu server; pelaku dicatat | `PASS` | `HmdUnitReadinessService.cs` baris 274–340 |
| Pemeriksaan source tidak siap | Tidak menyentuh `HmdSession` (komentar eksplisit baris 371) | `PASS` | `HmdUnitReadinessService.cs` baris 342–376 |
| Seed butir kesiapan di DB | 5 butir, 1 mewajibkan tanggal hasil (`WATER`) | `PASS` | Kueri baca-saja `QuilvianNewDevHamzah` |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `UnitReadinessEvaluationTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Uji air berumur 40 hari (batas 30 hari) → `422` dengan pesan kedaluwarsa | Terpenuhi, **dengan delta teks** | `422 HMD-VAL-102`; teks mengikuti `validation-matrix.md` |
| 2. Seluruh 5 butir valid → `Ready`, waktu server, koordinator sebagai `DeclaredByUserId` | Terpenuhi | `DeclareReadyAsync` |
| 3. `NotReady` di tengah shift tidak membatalkan sesi berjalan | Terpenuhi | `DeclareNotReadyAsync` tidak menyentuh sesi |
| DoD: service dan controller selesai, lolos skenario air kedaluwarsa dan kelengkapan butir | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Batas berlaku dihitung dari tengah malam tanggal hasil menurut zona waktu Asia/Jakarta, karena tanggal hasil tidak membawa jam |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Lembar yang sudah `Ready` tidak dapat disunting butirnya; bila hasil baru masuk, koordinator menyatakan tidak siap lalu mengisi ulang |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji runtime oleh pemilik |
