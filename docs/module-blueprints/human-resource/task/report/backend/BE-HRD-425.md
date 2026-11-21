# Laporan Perubahan Backend — `BE-HRD-425`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-425` |
| Judul | Gerbang backend Menu 08 dan regresi `REG-HRD-08-001..010` |
| Slice | `S-FIN-M08` / Verifikasi (alias `FIN-HRD-08-BE-010`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.5, kartu `BE-HRD-425` |
| Trace | Seluruh task `BE-HRD-416` s.d. `BE-HRD-424` |
| Contract version | Kontrak Kontrak Kerja existing (`BE-HRD-065`, `BE-HRD-080`) |
| Dependency | `BE-HRD-419` ✅, `421` ✅, `423` ✅, `424` ✅ |
| Klasifikasi | `LIGHT` — verifikasi |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | —; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — tidak ada P0 terbuka; build/QBE berlog; REG/UAT dicatat `NOT RUN` apa adanya |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `LifecycleManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Laporan agent mencatat 40 UAT dan 10 regresi "CODE PASS" tanpa eksekusi HTTP/DB, dengan dua P0 terbuka (`BE-HRD-419`, `BE-HRD-424`). Kedua P0 kini tertutup di source.

## 2. Proses bisnis

| Task | Hasil |
| --- | --- |
| `BE-HRD-416` ✅ | Validator PKWT UU 27/2026; selisih `HRD-DEC-089` terdokumentasi |
| `BE-HRD-417`, `418`, `420`, `421`, `422` | Tetap ✅ (tidak disentuh) |
| `BE-HRD-419` ✅ | Status H-7 dari artefak; eskalasi (migration belum dijalankan) |
| `BE-HRD-423` ✅ | Kunci antar-instance scheduler |
| `BE-HRD-424` ✅ | Cakupan baca kontrak + penanda `ReadAllUnits` |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Seluruh berkas yang diubah task `419`, `423`, `424`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source khusus task ini; verifikasi source |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Delta: endpoint `notice-evidence`; cakupan baca kontrak |
| Database | Migration `20261120092000` belum dijalankan |
| Keamanan/Auth | Penanda baru `WorkforceEmploymentContract : ReadAllUnits` |

## 4. Dokumentasi endpoint

Lihat laporan `BE-HRD-419` dan `BE-HRD-424`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (berkas Menu 08) | 9 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| REG-HRD-08-001..010 | Belum dijalankan via HTTP/DB | `NOT RUN` | Wewenang uji DB dev belum diberikan |
| 40 UAT Menu 08 | Belum dijalankan | `NOT RUN` | idem |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tidak ada P0 terbuka | Terpenuhi | `419` dan `424` tertutup di source |
| (2) REG dan UAT dicatat PASS/FAIL/NOT RUN dengan log | Terpenuhi | tercatat `NOT RUN` beserta alasan |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | Slice `S-FIN-M09` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |


## Pembaruan 21 November 2026 — Status Migration

Migration `20261120092000_AddHrdContractDecisionNoticeEvidence` sudah diterapkan ke database pengembangan `QuilvianNewDevHamzah`: pemilik menjalankan `dotnet ef database update --no-build` ("No migrations were applied. The database is already up to date.") dan `dotnet ef migrations list --no-build` terhadap `QuilvianNewDevHamzah` tidak menunjukkan satu pun migration `Pending`. Pernyataan "migration belum dijalankan" dan "wajib dijalankan sebelum dipakai" pada bagian di atas sudah tidak berlaku untuk DB dev tersebut; database lain (staging/production) tetap wajib menjalankannya sebelum fitur dipakai. Status task tetap ✅; REG/UAT tetap `NOT RUN`.
