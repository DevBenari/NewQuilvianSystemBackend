# Laporan Perubahan Backend — `BE-HRD-395`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-395` |
| Judul | Pengalaman kerja eksternal terstruktur |
| Slice | `S-FIN-M04` / Riwayat Pekerjaan (alias `FIN-HRD-04-BE-009`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-395` |
| Trace | Instruksi Menu 04 `FIN-HRD-04-BE-009` (rekomendasi pengalaman eksternal, `NEEDS_SYNC`) |
| Contract version | Kontrak riwayat kepegawaian Workforce Core existing |
| Dependency | — |
| Klasifikasi | `LIGHT` — verifikasi source; satu perubahan cek profil bersama `BE-HRD-401` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | Laporan, roadmap, traceability; `WfpEmploymentHistoryController.cs` lewat `BE-HRD-396`/`401` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Pengalaman kerja di rumah sakit/perusahaan sebelumnya perlu dicatat terpisah dari riwayat resmi di RS (yang bersumber dari SK). Source sudah menyediakannya tanpa tabel baru: `HistoryType = "ExternalPrior"` pada `WfpEmploymentHistory`, dengan filter `?source=external|internal`. Task ini membuktikan kriterianya pada build berbukti; tidak ditemukan cacat baru selain cek profil aktif (lihat `BE-HRD-401`).

## 2. Proses bisnis

1. HR mencatat pengalaman eksternal: jenis `ExternalPrior`, tanggal mulai/akhir, alasan/keterangan, berkas bukti opsional.
2. Tanggal akhir sebelum tanggal mulai → `400`.
3. Daftar riwayat dapat disaring: `source=external` hanya `ExternalPrior`; `source=internal` selain `ExternalPrior`.
4. Pengalaman eksternal tetap dapat diubah/dihapus (bukan riwayat resmi); riwayat internal resmi dikunci (`BE-HRD-396`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpEmploymentHistoryController.cs` (`AllowedHistoryTypes`, `GetEmploymentHistories` filter `source`, `ValidateRequestAsync`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan khusus task ini; perubahan berkas yang sama tercatat di `BE-HRD-396` dan `BE-HRD-401` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak berubah |
| Database | Tanpa migration; tanpa tabel baru (sesuai kartu) |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Employment History

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employment-histories?source=external` | Daftar pengalaman eksternal | `WfpEmploymentHistory : Read` |
| `POST` | `…/employment-histories` (`HistoryType = ExternalPrior`) | Catat pengalaman eksternal | `WfpEmploymentHistory : Create` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: mulai 1 Mar 2020, akhir 1 Jan 2020 | `400` | `PASS` (penelusuran) | `ValidateRequestAsync` |
| Penelusuran: `source=external` | Hanya `ExternalPrior` | `PASS` (penelusuran) | `GetEmploymentHistories` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Pengalaman eksternal tersimpan dan terfilter terpisah dari riwayat RS | Terpenuhi | `ExternalPrior` + filter `source` |
| (2) Tanggal mulai ≤ tanggal selesai | Terpenuhi | `ValidateRequestAsync` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Field khusus (nama pemberi kerja, jabatan, lokasi) masih memakai kolom generik (`Reason`/`Description`); tabel terpisah menunggu gerbang skema |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-396` |
