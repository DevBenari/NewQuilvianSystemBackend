# Penghapusan & Pelepasan Aset — Roadmap Frontend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` |
| Blueprint / contract | `AST-BP-001` rev 1, `penghapusan`, `AST-HPS-1.0.0` `approved` |
| Frontend SHA | `e49e8a81` (`RizkiV2`) |
| Task | `FE-AST-024` s.d. `FE-AST-026` (`MVP-5`) |
| Handoff | `build-module-frontend`, satu task per pemanggilan, `FRONTEND MODE` eksplisit |

**Legenda tanda status:** ✅ selesai · 🟡 sebagian · ⛔ terblokir · tanpa tanda = belum dikerjakan.

`V-FE` dan `DoD-FE` sama dengan `../../register/roadmap/frontend-roadmap.md`.

## Grafik Urutan Dependency

```text
                                                                 BE-AST-037 [BE] ───┐
                                                                                    │
                     BE-AST-034 [BE] ─┐    BE-AST-035 [BE] ─┐    BE-AST-036 [BE] ─┐ │
                                      │                     │                     │ │
FE-AST-004 [FE-REG] ──────────────────┴─> FE-AST-024 ───────┴─> FE-AST-025 ───────┴─┴─> FE-AST-026
```

`[BE]` = task backend pada `backend-roadmap.md` sub-modul ini · `[FE-REG]` = task frontend sub-modul
`register`. Cermin baca-saja.

Pasangan prasyarat→task: **7** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `FE-AST-004` [FE-REG], `BE-AST-034` [BE] | `FE-AST-024` |
| 2 | `FE-AST-024`, `BE-AST-035` [BE] | `FE-AST-025` |
| 3 | `FE-AST-025`, `BE-AST-036`, `BE-AST-037` [BE] | `FE-AST-026` |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-AST-024` | Usulan, kelayakan, persetujuan, detail | `CAP-10.1..10.3` | API 2.1 | `asset-picker`, `confirm-modal` | `SCR-AST-HPS-01..03`, `08` + grup menu Penghapusan | `FE-AST-004`, `BE-AST-034` | Kartu | `V-FE`, `UAT-HPS-02`, `04` | — | `DoD-FE` |
| `FE-AST-025` | Pemindahtanganan dan berita acara | `CAP-10.4`, `10.5` | API 2.1 | `confirm-modal` | `SCR-AST-HPS-04`, `05` | `FE-AST-024`, `BE-AST-035` | Kartu | `V-FE`, `UAT-HPS-01`, `03`, `05` | — | `DoD-FE` |
| `FE-AST-026` | Riwayat aset dihapus, pemulihan, laporan | `CAP-10.6`, `11.5`, `CAP-EXT-03` | API 2.2–2.4 | — | `SCR-AST-HPS-06`, `07` | `FE-AST-025`, `BE-AST-036`, `BE-AST-037` | Kartu | `V-FE`, `UAT-HPS-06`, `07` | — | `DoD-FE` |

## Kartu task

### `FE-AST-024` — Usulan sampai persetujuan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Skema `03-frontend-architecture.md` §3.1. 2. Kolom rupiah hanya dari `/amounts`. 3. Catat kelayakan hanya bagi anggota tim; Setujui tersembunyi bagi pengusul/pemeriksa. 4. Grup menu dan tiga butir terdaftar |

### `FE-AST-025` — Pemindahtanganan dan berita acara

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Formulir per metode dengan aturan nilai. 2. Tersembunyi untuk alasan Hilang. 3. Finalisasi dengan konfirmasi; 409 penyusutan menautkan ke `SCR-AST-NLI-04`. 4. Cetak berita acara. 5. Dua butir menu terdaftar |

### `FE-AST-026` — Riwayat, pemulihan, laporan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Riwayat aset dihapus dengan tombol Pulihkan. 2. Lampiran wajib untuk aset terjual/dihibahkan/ditukar. 3. Laporan penghapusan hanya peran `ViewAmount`. 4. Butir menu "Riwayat Aset Dihapus" dan "Penghapusan" (laporan) |
