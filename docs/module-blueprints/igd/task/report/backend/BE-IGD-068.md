# Laporan Perubahan Backend — `BE-IGD-068`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-068` |
| Judul | Penyelesaian catatan dokter IGD tidak mengirim fakta jasa konsultasi |
| Slice | `EPIC IGD-14` / `MVP-9` — penyelesaian catatan dokter |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-105`; `AT-IGD-204`; DoD butir 5 |
| Keputusan | `IGD-DEC-229` (menjawab `IGD-CONF-10`) |
| Contract version | API **`0.15.0`** §10.1 nomor 6; validation **`0.14.0`** §12.6 aturan 16; integration **`0.6.0`** §7.3 |
| Dependency | — |
| Klasifikasi | `LOW` / `MEDIUM` — Modifikasi satu service pada Farmasi (`ConsultationFinalizationService.cs`) |
| Task mode | `BACKEND` — izin implementasi diberikan Rizki |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `ConsultationFinalizationService.cs`, laporan ini, roadmap, requirement traceability |
| Model | Claude Opus 5.5 / Antigravity |
| Tanggal | 8 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 8 Oktober 2026: implementasi selesai dan dibuild pemilik; 4 dari 5 acceptance terbukti** (2–4 lewat diff, 5). Satu berkas diubah; nol migration; nol komentar baru; CRLF tanpa BOM. Build pemilik: `dotnet build` 0 error, 235 warning (identik baseline; dilaporkan 8 Oktober 2026). Menunggu putaran uji 1 bersama `FE-IGD-049` (`AT-IGD-204`) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PharmacyManagement` |
| Pemilik / prefix registry | `Phm` / `PharmacyManagement`, `BUSINESS DOMAIN / MODULE` — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Applicability | `TOUCHED LEGACY` pada `ConsultationFinalizationService.cs` (`FinalizeAsync`); nol entity baru, nol migration |
| QBE yang berlaku | `QBE-API-001` (alur finalisasi tetap mengikuti kontrak lama), `QBE-SVC-001` (efisiensi LINQ & pengecekan type tanpa query tambahan) |
| Checker | `powershell -NoProfile -ExecutionPolicy Bypass -File tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` — **VIOLATION: 0, REVIEW: 0, INFO: 0, Final result: PASS** |

---

## 1. Masalah yang Diperbaiki

Pada alur poliklinik, saat catatan dokter diselesaikan (`PATCH /doctor-consultations/{id}/complete`), sistem mengirimkan dua jenis fakta tagihan ke modul Billing: fakta jasa konsultasi dokter (`ConsultationCompleted`) dan fakta tagihan resep obat (`PrescriptionDispensed`/`PrescriptionBilled`). Sesuai kesepakatan tata kelola IGD (`IGD-DEC-229`), jasa penanganan gawat darurat dibebankan secara terpisah (misalnya melalui tindakan/akomodasi IGD), bukan sebagai jasa konsultasi poliklinik per catatan. Karena itu, untuk encounter bertipe `Emergency`, penyelesaian catatan dokter tidak boleh mengirimkan fakta jasa konsultasi ke Billing, namun fakta resep tetap wajib dikirimkan.

---

## 2. Perubahan yang Dikerjakan

### 2.1 Berkas yang Diubah

1. `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs`:
   - Pada metode `FinalizeAsync`, blok penyerahan fakta penagihan konsultasi (`PublishBillingFactsAsync`) yang memicu event `ConsultationCompleted` dibungkus dengan penjaga kondisi:
     ```csharp
     if (consultation.Encounter?.EncounterType != EncounterType.Emergency)
     {
         ...
     }
     ```
   - Pengiriman fakta penagihan resep di blok berikutnya tetap dijalankan tanpa modifikasi sehingga pasien IGD tetap ditagihkan obat resepnya secara akurat.
   - Properti `consultation.Encounter` sudah dimuat secara `Include` di awal metode (`:79`), sehingga pembacaan `EncounterType` tidak menimbulkan overhead query tambahan ke database.

---

## 3. Kriteria Penerimaan & Status

| # | Kriteria | Status | Bukti |
| ---: | --- | :---: | --- |
| 1 | Catatan dokter encounter IGD dengan satu resep diselesaikan → satu fakta resep, nol fakta `ConsultationCompleted` untuk konsultasi itu (`AT-IGD-204`) | 🟡 | Menunggu uji putaran 1 |
| 2 | Regresi: catatan dokter poliklinik tetap mengirim fakta jasa konsultasi | ✅ | Diff penalaran: pengecekan `!= EncounterType.Emergency` meloloskan poliklinik |
| 3 | Penyelesaian tetap atomik: kegagalan pendaftaran rekam medis tetap membatalkan finalisasi | ✅ | Diff penalaran: logika transaksi dan try/catch tidak diubah |
| 4 | Diff satu berkas, hanya blok pengiriman fakta konsultasi; nol komentar baru | ✅ | `git diff` terbukti |
| 5 | Build 0 error; warning dilaporkan | ✅ | Build pemilik 8 Oktober 2026: 0 error, 235 warning (identik baseline 235 warning) |

---

## 4. Perintah Kompilasi Backend untuk Pemilik

```powershell
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```
