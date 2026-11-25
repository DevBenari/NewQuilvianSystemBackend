# Laporan Perubahan Backend — `BE-HRD-416`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-416` |
| Judul | Kebijakan PKWT berversi UU 27/2026 dan sinkronisasi `HRD-DEC-089` |
| Slice | `S-FIN-M08` / Perpanjangan (alias `FIN-HRD-08-BE-001`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.5, kartu `BE-HRD-416` |
| Trace | UU 27/2026; `HRD-DEC-089` (batas 5 tahun, perlu sinkron); instruksi Menu 08 `FIN-HRD-08-BE-001` |
| Contract version | Kontrak Kontrak Kerja existing (`BE-HRD-065`, `BE-HRD-080`) |
| Dependency | — |
| Klasifikasi | `LIGHT` — verifikasi + dokumentasi selisih |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | —; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `LifecycleManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Validator PKWT `HrdFixedTermPolicyService` sudah mengikuti UU 27/2026 (awal ≤ 24 bulan dihitung presisi per hari, perpanjangan maksimal 2 × ≤ 12 bulan, `RuleVersion`) dan dipakai GET kelayakan, POST, dan PUT keputusan. Yang belum: selisih dengan `HRD-DEC-089` (catatan keputusan lama: batas total 5 tahun) belum terdokumentasi. Laporan eksekusi agent: [FIN-HRD-08-BE-KONTRAK-DAN-MASA-KERJA.md](FIN-HRD-08-BE-KONTRAK-DAN-MASA-KERJA.md).

## 2. Proses bisnis

1. Kontrak awal PKWT > 24 bulan → ditolak.
2. Perpanjangan ketiga atau perpanjangan > 12 bulan → ditolak.
3. Data kontrak historis tidak diubah saat aturan berganti; setiap evaluasi mencatat `RuleVersion`.

**Selisih dengan `HRD-DEC-089` (didokumentasikan untuk sinkronisasi pemilik).**

| Aspek | `HRD-DEC-089` (tercatat) | Source (UU 27/2026) |
| --- | --- | --- |
| Batas total | 5 tahun | Awal ≤ 24 bulan + perpanjangan 2 × ≤ 12 bulan |
| Versi aturan | Tidak disebut | `RuleVersion` tersimpan per evaluasi |

Catatan keputusan tidak diubah oleh task ini; pencatatan ulang (`NEEDS_SYNC`) dilakukan pemilik lewat `manage-module-blueprint`.

**Contoh.** Kontrak awal 1 Januari 2026 s.d. 31 Januari 2028 (25 bulan) → ditolak; perpanjangan ke-3 → ditolak.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdFixedTermPolicyService.cs`, pemanggil di `HrdEmploymentContractService.cs` (`FixedTermCheckAsync`, pembuatan/keputusan), `00-interview-decisions.md` (`HRD-DEC-089`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source khusus task ini; verifikasi source |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Lifecycle Management / Employment Contracts

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/employment-contracts/{id}/fixed-term-check` | Kelayakan PKWT berversi | hak existing |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (berkas Menu 08) | 9 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: awal 25 bulan | Ditolak | `PASS` | `HrdFixedTermPolicyService` |
| Penelusuran: perpanjangan ketiga | Ditolak | `PASS` | idem |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Awal 25 bulan dan perpanjangan ketiga ditolak | Terpenuhi | validator |
| (2) Data historis tidak dimutasi | Terpenuhi | evaluasi hanya membaca + `RuleVersion` |
| (3) Perbedaan dengan `HRD-DEC-089` terdokumentasi | Terpenuhi | tabel selisih di laporan ini dan traceability |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Tindak lanjut pemilik | Tandai `HRD-DEC-089` `NEEDS_SYNC` dan catat ulang batas UU 27/2026 lewat `manage-module-blueprint` (gerbang dokumentasi, tidak menahan source) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
