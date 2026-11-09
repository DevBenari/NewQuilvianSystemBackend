# Roadmap Repair 043 — Rencana Kontrol Terstruktur dan Sumber Dokter Perujuk Luar pada IPD

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` |
| tanggal | 2026-11-02 Asia/Jakarta |
| status / approval | Disetujui pemilik sistem pada 2026-11-02 ("oke sekarang kerjakan semua task tersebut dan perbaiki"); K-04 diputuskan pada hari yang sama: nama perujuk luar yang dicatat admisi dipakai sebagai sumber ketiga IPD |
| issue / rencana | [ISSUE-EPS-043](../docs/issue/issue-043-patah-alur-admisi-discharge-dan-koordinasi-bedah.md) / [PLAN-REPAIR-EPS-043](../docs/plan-repair/plan-repair-043-patah-alur-admisi-discharge-dan-koordinasi-bedah.md) |
| source SHA backend / frontend saat didaftarkan | `396c8363` (MHamzah) / `9c4000724` (HamzahV2) |
| scope / ID | 3 task: `BE-RWI-256`, `FE-RWI-286`, `BE-RWI-257`. Penomoran berlanjut dari [roadmap repair 015 keperawatan](../../keperawatan/roadmap/roadmap-repair-015.md). Berikutnya `BE-RWI-258` dan `FE-RWI-287` |

---

## Arti Tanda Status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai dan acceptance criteria terbukti pada laporan task |
| 🟡 | Implementasi sebagian atau bukti belum lengkap |
| ⛔ | Terblokir oleh prasyarat yang belum terpenuhi |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
BE-RWI-256 ✅ ──> FE-RWI-286 🟡
BE-RWI-257 ✅   tanpa dependency
```

| Gelombang | Boleh mulai setelah | Task ID |
| ---: | --- | --- |
| 1 | — | `BE-RWI-256`, `BE-RWI-257` |
| 2 | `BE-RWI-256` | `FE-RWI-286` |

---

## Register Task

| Task ID / Status | Outcome | Menutup | Lapisan | Dependency | Acceptance criteria | Laporan |
| --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-256` ✅ | Resume medis menyimpan rencana kontrol terstruktur (tanggal, poli, dokter) beserta validasinya dan salinan revisinya | `FIX-EPS-043-02` (`ISS-EPS-043-02`) | BE | — | Plan repair `FIX-EPS-043-02` AC 1-5 dari sisi server | [BE-RWI-256.md](../task/report/backend/BE-RWI-256.md) |
| `FE-RWI-286` 🟡 | Form, detail, dan cetakan resume medis menampilkan rencana kontrol terstruktur | `FIX-EPS-043-02` (`ISS-EPS-043-02`) | FE | `BE-RWI-256` | Plan repair `FIX-EPS-043-02` AC 1-5 dari sisi layar; rangka plan repair bagian 3.B | [FE-RWI-286.md](../task/report/frontend/FE-RWI-286.md) |
| `BE-RWI-257` ✅ | Lembar IPD memakai nama perujuk luar pada episode sebagai sumber ketiga | `FIX-EPS-043-05` (`ISS-EPS-043-03` bagian B) | BE | — | Plan repair `FIX-EPS-043-05` AC 1-4 | [BE-RWI-257.md](../task/report/backend/BE-RWI-257.md) |

---

## Kartu Task

### ✅ BE-RWI-256 — Rencana kontrol terstruktur pada resume medis

| Hal | Isi |
| --- | --- |
| Status | ✅ **SELESAI 9 November 2026.** Source masuk commit `da737fb9`; AC 1-5 sisi server terpetakan ke `ValidateFollowUpPlanAsync`, penyimpanan, salinan revisi, dan proyeksi detail; `dotnet build` 0 error / 12 warning (paket NuGet lama); migration `20261102050639_AddStructuredFollowUpToInpDischargeSummary` sudah diterapkan di `QuilvianNewDevHamzah` (kolom terbukti lewat `information_schema`). Uji manual dijalankan bersama `FE-RWI-286`. [Laporan](../task/report/backend/BE-RWI-256.md) |
| Berkas | `InpDischargeSummary.cs`, `InpDischargeSummaryRevision.cs`, `InpatientDischargeDtos.cs`, service resume, satu migration baru (file migration disiapkan; penerapan ke database oleh pemilik) |
| Kontrak | Kolom opsional tanggal kontrol, poli kontrol, dokter kontrol; `FollowUpInstruction` tetap sebagai catatan |
| Acceptance criteria | Plan repair `FIX-EPS-043-02` AC 1-5 dari sisi server |
| DoD | `dotnet build` pada project aplikasi; laporan `task/report/backend/BE-RWI-256.md` |

### 🟡 FE-RWI-286 — Rencana kontrol terstruktur pada layar resume medis

| Hal | Isi |
| --- | --- |
| Status | 🟡 **SEBAGIAN 9 November 2026.** Source lengkap; sesi ini menambah penandaan isian dari pesan server (AC 2). AC 4 terbukti di layar; AC 2 dan AC 3 terbukti di server dan unit test. **Belum:** AC 1 (simpan + cetak), AC 5 (salinan revisi), dan tampilan form edit — `VISUAL CHECK: NOT FEASIBLE` karena butuh akun DPJP (dr. Rendy Pangalila, `rendi@admin.com`), sedangkan kata sandinya tidak ada di manifest uji. Lint/build exit 0; unit 0 kegagalan baru. [Laporan](../task/report/frontend/FE-RWI-286.md) |
| Berkas | `inpatient-discharge-view.jsx`, `inpatient-discharge-utils.jsx`, cetakan resume |
| Kontrak | Hasil `BE-RWI-256` |
| Acceptance criteria | Plan repair `FIX-EPS-043-02` AC 1-5 dari sisi layar |
| DoD | `npm run lint:errors`, `npm run test:unit`, `npm run build`; laporan `task/report/frontend/FE-RWI-286.md` |

### ✅ BE-RWI-257 — Sumber ketiga dokter perujuk luar pada IPD

| Hal | Isi |
| --- | --- |
| Status | ✅ **SELESAI 9 November 2026.** Source masuk commit `da737fb9`; AC 1-4 terpetakan ke cabang ketiga `referringDoctor` pada `InpAdmissionWorkspaceQueryService.cs`; `dotnet build` 0 error / 12 warning; tanpa perubahan database. Uji cetak empat kasus dijalankan pada sesi uji langsung MVP. Pencatatan amandemen `RWI-DEC-254`/`RWI-AC-386` di `00-interview-decisions.md` masuk Slice 6 MVP. [Laporan](../task/report/backend/BE-RWI-257.md) |
| Berkas | `InpAdmissionWorkspaceQueryService.cs` |
| Keputusan | K-04 (2026-11-02) mengamandemen `RWI-DEC-254` dan `RWI-AC-386`: urutan sumber "surat pengantar → perujuk luar Registrasi → perujuk luar yang dicatat admisi → garis kosong" |
| Acceptance criteria | Plan repair `FIX-EPS-043-05` AC 1-4 |
| DoD | `dotnet build` pada project aplikasi; laporan `task/report/backend/BE-RWI-257.md` |
