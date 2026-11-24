# Laporan Perubahan Backend — `BE-HRD-400`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-400` |
| Judul | Hardening Employee 360 dual-mode dan masking payload |
| Slice | `S-FIN-M04` / Profil (alias `FIN-HRD-04-BE-014`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-400` |
| Trace | `HRD-DEC-053`; instruksi Menu 04 `FIN-HRD-04-BE-014`; `UAT-04-17`, `31`, `42` |
| Contract version | Kontrak Employee 360 (`BE-HRD-026`) |
| Dependency | `BE-HRD-387` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Services/HrdEmployee360Service.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-PERM-001, QBE-DTO-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Employee 360 punya dua mode: HR (hak `HrdEmployee360 : ReadFull` atau `HrdPersonnelActionOrder : Read`) melihat profil penuh; atasan utama aktif hanya melihat profil pekerjaan bawahan langsung. Penolakan `403` bagi bukan atasan dan pengosongan profil keuangan/disiplin pada mode atasan sudah ada. Dua celah:

1. **Hak atasan dinilai pada tanggal kiriman pemanggil.** Parameter `asOfDate` ikut dipakai untuk memeriksa "masih atasan langsung". Mantan atasan dapat mengirim tanggal lampau dan kembali membuka profil mantan bawahannya.
2. **Alasan SK bocor ke mode atasan.** Daftar SK pada tampilan atasan memuat `ReasonText`, yang dapat berisi alasan demosi/disiplin.

## 2. Proses bisnis

1. Pemanggil membuka Employee 360 pegawai X.
2. Bila punya hak HR → mode HR (profil penuh: gaji, rekening, NIK, keluarga, sanksi).
3. Bila tidak: diperiksa apakah pemanggil atasan utama aktif X **pada tanggal server hari ini**. Bukan → `403`.
4. Mode atasan: kontak pribadi (telepon/WA) disamarkan; profil keuangan dan disiplin `null`; alasan SK dikosongkan. `asOfDate` tetap boleh dipakai untuk menampilkan data pekerjaan per tanggal tertentu.

**Contoh.** Kepala ruang Y menjadi atasan Z sampai 30 Juni 2026. Pada 20 November 2026 Y memanggil Employee 360 Z dengan `asOfDate=2026-05-01` → `403` (dulu berhasil).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdEmployee360Service.cs`, `HrdEmployee360Controller.cs`, `PersonnelActionOrderDtos.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/Services/HrdEmployee360Service.cs` | Pemeriksaan atasan memakai tanggal server; `ReasonText` SK kosong pada mode atasan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; `ReasonText` bernilai `""` pada `ViewMode = DirectManager` |
| Database | Tanpa migration |
| Keamanan/Auth | Menutup akses mantan atasan lewat tanggal lampau |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Employee 360

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employee-360` | Profil 360 dual-mode | Hak HR atau atasan utama aktif (dinilai di service) |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) (satu warning nullable dari perubahan ini diperbaiki sebelum build final) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: bukan atasan, tanpa hak HR | `403` | `PASS` (penelusuran) | `GetEmployee360Async` |
| Penelusuran: mantan atasan + `asOfDate` lampau | `403` | `PASS` (penelusuran) | `serverToday` |
| Penelusuran: mode atasan | `ConfidentialFinancialProfile = null`, `ConfidentialPersonalAndDiscipline = null`, telepon/WA `null`, `ReasonText = ""` | `PASS` (penelusuran) | blok `else` + proyeksi SK |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Bukan atasan langsung ditolak `403` | Terpenuhi | Pemeriksaan atasan utama aktif pada tanggal server |
| (2) Payload keuangan/disiplin kosong pada mode atasan | Terpenuhi | Kedua blok `null`; alasan SK dikosongkan |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di luar scope | Hak `HrdPersonnelActionOrder : Read` ikut membuka mode HR penuh (termasuk gaji dan rekening). Ini desain `BE-HRD-026`; dicatat untuk ditinjau pemilik |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-401` |
