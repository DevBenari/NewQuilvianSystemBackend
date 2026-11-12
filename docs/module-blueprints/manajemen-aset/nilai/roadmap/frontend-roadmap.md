# Nilai Aset — Roadmap Frontend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` |
| Blueprint / contract | `AST-BP-001` rev 1, `nilai`, `AST-NLI-1.0.0` `approved` |
| Frontend SHA | `e49e8a81` (`RizkiV2`) |
| Task | `FE-AST-011` (`MVP-0`) dan `FE-AST-019` s.d. `FE-AST-023` (`MVP-4`) |
| Handoff | `build-module-frontend`, satu task per pemanggilan, wewenang `FRONTEND MODE` eksplisit |

**Legenda tanda status:** ✅ selesai · 🟡 sebagian · ⛔ terblokir · tanpa tanda = belum dikerjakan.

`V-FE` dan `DoD-FE` sama dengan `../../register/roadmap/frontend-roadmap.md`. Seluruh layar memuat rupiah:
hanya peran ber-hak nilai yang melihat butir menunya.

## Grafik Urutan Dependency

Dipotong menjadi dua blok karena lebar; `FE-AST-019` muncul lagi sebagai sambungan blok kedua.

```text
                    BE-AST-015 [BE] ─┐               BE-AST-026 [BE] ─┐
                                     │                                │
FE-AST-001 [FE-REG] ─────────────────┴─> FE-AST-011 ─┬────────────────┴─> FE-AST-019
                                                     │
                                                     │   BE-AST-031 [BE] ─┐
                                                     │                    │
                                                     └────────────────────┴─> FE-AST-022
```

```text
                                      BE-AST-028 [BE] ─┐
                                                       │
BE-AST-027 [BE] ─┬─> FE-AST-020 ─┬─────────────────────┴─> FE-AST-021
                 │               │
     FE-AST-019 ─┘               │    BE-AST-030 [BE] ───┐
                                 │                       │
                                 │    BE-AST-029 [BE] ─┐ │
                                 │                     │ │
                                 └─────────────────────┴─┴─> FE-AST-023
```

`[BE]` = task backend pada `backend-roadmap.md` sub-modul ini · `[FE-REG]` = task frontend sub-modul
`register` pada `../../register/roadmap/frontend-roadmap.md`. Keduanya cermin baca-saja.

Pasangan prasyarat→task: **13** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `FE-AST-001` [FE-REG], `BE-AST-015` [BE] | `FE-AST-011` |
| 2 | `FE-AST-011` + `BE-AST-026` / `BE-AST-031` [BE] | `FE-AST-019`, `FE-AST-022` — boleh paralel |
| 3 | `FE-AST-019`, `BE-AST-027` [BE] | `FE-AST-020` |
| 4 | `FE-AST-020` + backend masing-masing | `FE-AST-021`, `FE-AST-023` — boleh paralel |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-AST-011` | Kepala Akuntansi menetapkan parameter dan kebijakan kelompok | `CAP-EXT-02`, `AST-DEC-027` | API 2.1–2.2 | `currency/` | `SCR-AST-NLI-06` bagian 1–2 + grup menu Nilai | `FE-AST-001`, `BE-AST-015` | Kartu | `V-FE`, `UAT-NLI-01` | — | `DoD-FE` |
| `FE-AST-019` | Akun kontrol dan pemetaan pusat biaya | `AST-DES-011`, `035` | API 2.3–2.4 | — | `SCR-AST-NLI-06` bagian 3–4 | `FE-AST-011`, `BE-AST-026` | Kartu | `V-FE`, `UAT-NLI-02` | — | `DoD-FE` |
| `FE-AST-020` | Pratinjau dan proses penyusutan | `CAP-09.4` | API 2.6 | `confirm-modal` | `SCR-AST-NLI-04`, `08` | `FE-AST-019`, `BE-AST-027` | Kartu | `V-FE`, `UAT-NLI-03`, `04` | — | `DoD-FE` |
| `FE-AST-021` | Pengajuan dan persetujuan perubahan nilai | `CAP-09.3` | API 2.7 | — | `SCR-AST-NLI-03` | `FE-AST-020`, `BE-AST-028` | Kartu | `V-FE`, `UAT-NLI-06`, `07` | — | `DoD-FE` |
| `FE-AST-022` | Harga perolehan, nilai buku, laporan nilai | `CAP-09.1`, `09.2`, `11.6` | API 2.5, 2.10 | — | `SCR-AST-NLI-01`, `02`, `07` | `FE-AST-011`, `BE-AST-031` | Kartu | `V-FE` | — | `DoD-FE` |
| `FE-AST-023` | Rekonsiliasi dan kotak keluar | `CAP-09.5` | API 2.8–2.9 | — | `SCR-AST-NLI-05` | `FE-AST-020`, `BE-AST-029`, `BE-AST-030` | Kartu | `V-FE`, `UAT-NLI-08` | — | `DoD-FE` |

## Kartu task

### `FE-AST-011` — Parameter nilai dan kebijakan kelompok

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Parameter berlaku, yang akan berlaku, dan riwayat. 2. Setiap simpan menampilkan "Berlaku mulai {bulan}". 3. Grup menu "Nilai Aset & Rekonsiliasi" dan butir "Parameter Nilai" terdaftar, tersembunyi tanpa hak |

### `FE-AST-019` — Akun kontrol dan pusat biaya

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Bagian 3–4 `SCR-AST-NLI-06`. 2. Saringan "belum jelas" untuk unit. 3. Galat kode akun dari backend tampil per baris |

### `FE-AST-020` — Data penyusutan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Skema `03-frontend-architecture.md` §3.1. 2. Tombol Proses nonaktif bila ada galat. 3. Konfirmasi menyebut periode dan total. 4. Detail proses `SCR-AST-NLI-08` dari riwayat. 5. Butir menu "Data Penyusutan" |

### `FE-AST-021` — Perubahan nilai

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Formulir per jenis (isian berbeda per jenis). 2. Setujui/Tolak tersembunyi bagi pengaju. 3. Entri riwayat per aset. 4. Butir menu "Riwayat Perubahan Nilai" |

### `FE-AST-022` — Harga perolehan, nilai buku, laporan nilai

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. `SCR-AST-NLI-01`, `02`, `07` dengan saringan periode. 2. Ekspor. 3. Tiga butir menu terdaftar |

### `FE-AST-023` — Rekonsiliasi dan kotak keluar

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Saldo per akun kontrol dan status kirim; "Ditahan" untuk kode yang belum dinyalakan. 2. Kirim ulang dan coba ulang sesuai hak. 3. Tautan ke rekonsiliasi Akuntansi tampil bila pengguna berhak. 4. Butir menu "Rekonsiliasi Aset dengan Akuntansi" |
