# Pergerakan Aset — Roadmap Frontend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` |
| Blueprint / contract | `AST-BP-001` rev 1, `pergerakan`, `AST-MOV-1.0.0` `approved` |
| Frontend SHA | `e49e8a81` (`RizkiV2`) |
| Task | `FE-AST-012` s.d. `FE-AST-014` (`MVP-2`) |
| Handoff | `build-module-frontend`, satu task per pemanggilan, `FRONTEND MODE` eksplisit |

**Legenda tanda status:** ✅ selesai · 🟡 sebagian · ⛔ terblokir · tanpa tanda = belum dikerjakan.

`V-FE` dan `DoD-FE` sama dengan `../../register/roadmap/frontend-roadmap.md`.

## Grafik Urutan Dependency

```text
                      BE-AST-017 [BE] ✅ ─┐
                                       │
FE-AST-004 [FE-REG] ✅ ─┬─────────────────┴─> FE-AST-012 ✅ ───────────────────┐
                     │                                                   │
                     │                                BE-AST-019 [BE] ✅ ─┐ │
                     │                                                 │ │
                     │                BE-AST-018 [BE] ✅ ─┐               │ │
                     │                                 │               │ │
                     └─────────────────────────────────┴─> FE-AST-013 ✅ ─┴─┴─┬─> FE-AST-014 ✅
                                                                           │
                                                     FE-AST-010 [FE-REG] ──┘
```

`[BE]` = task backend pada `backend-roadmap.md` sub-modul ini · `[FE-REG]` = task frontend sub-modul
`register`. Keduanya cermin baca-saja.

Pasangan prasyarat→task: **8** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `FE-AST-004` [FE-REG] + `BE-AST-017` / `BE-AST-018` [BE] | `FE-AST-012`, `FE-AST-013` — boleh paralel |
| 2 | `FE-AST-012`, `FE-AST-013`, `BE-AST-019` [BE], `FE-AST-010` [FE-REG] | `FE-AST-014` |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-AST-012` | Mutasi dapat diajukan, disetujui, diterima | `CAP-05.1..05.3` | API 2.1 | `asset-picker`, `confirm-modal` | `SCR-AST-MOV-01..03`, `06` + butir menu | `FE-AST-004`, `BE-AST-017` | Kartu | `V-FE`, `UAT-MOV-01..03` | — | `DoD-FE` |
| `FE-AST-013` | Pinjaman dapat diajukan sampai dikembalikan | `CAP-05.4`, `05.5` | API 2.2 | `asset-picker` | `SCR-AST-MOV-04`, `05`, `07` + butir menu | `FE-AST-004`, `BE-AST-018` | Kartu | `V-FE`, `UAT-MOV-04..06` | — | `DoD-FE` |
| `FE-AST-014` | Kartu 1.4 dan laporan mutasi | `CAP-01.4`, `11.2` | API 2.3–2.4 | `summary-grid` | Kartu di `SCR-AST-REG-01`, `SCR-AST-MOV-08` + butir menu | `FE-AST-012`, `FE-AST-013`, `BE-AST-019`, `FE-AST-010` | Kartu | `V-FE` | — | `DoD-FE` |

## Kartu task

### ✅ `FE-AST-012` — Mutasi

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 19 November 2026.** Pengajuan Pemindahan (kartu, tab, + Ajukan Mutasi), formulir dengan pemilih aset unit asal, Persetujuan Mutasi (tab Tahap Unit Asal / Bagian Aset), Serah Terima Antar Unit, rincian dengan tombol dari `availableActions`, modal Terima (ruangan, penanggung jawab, kondisi per aset), cetak berita acara; 5 route, grup menu 3 butir. ESLint 20 berkas `0 errors, 0 warnings`. Uji layar Playwright **25/25** (pengaju, Kepala Unit asal, Bagian Aset, Kepala Unit tujuan, LOKET) di `next dev`; 0 galat halaman, 0 respons 5xx. Kelima kriteria terbukti. `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-19 12:37:20). Laporan: [`FE-AST-012.md`](../task/report/frontend/FE-AST-012.md) |
| Acceptance criteria | 1. Skema `03-frontend-architecture.md` §3.2–3.3. 2. Tombol mengikuti `AvailableActions`. 3. Terima mewajibkan ruangan dan penanggung jawab tujuan. 4. 409 data basi memuat ulang detail. 5. Tiga butir menu terdaftar |

### ✅ `FE-AST-013` — Peminjaman

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 20 November 2026.** Peminjaman Sementara (kartu, tab Menunggu Anda/Terlambat/status, + Ajukan Pinjaman), formulir dengan pemilih aset `loanable-assets` unit pemilik, Pengembalian Aset (`status=Dipinjam&status=MenungguKonfirmasiKembali`, tab Terlambat), rincian dengan tabel aset dan perpanjangan, tombol dari `availableActions`, modal Minta Perpanjangan (tanggal harus maju + alasan), modal Konfirmasi Kembali (kondisi per aset wajib, tanpa Hilang), cetak berita acara; 4 route, 2 butir menu. ESLint 19 berkas `0 errors, 0 warnings`. Playwright di `next dev` **32/34 + cek ulang 6/6** (dua FAIL awal = harapan skrip peka huruf pada badge; dibuktikan ulang lewat `textContent`); 0 galat halaman, 0 respons 5xx. Keempat kriteria terbukti. `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-20 11:18:04, manifest memuat 4 route `movement/loan*`); source di commit `9e205e827`. Laporan: [`FE-AST-013.md`](../task/report/frontend/FE-AST-013.md) |
| Acceptance criteria | 1. Tanda Terlambat dari `isOverdue`. 2. Konfirmasi kembali mewajibkan kondisi per aset. 3. Perpanjangan dan keputusannya. 4. Dua butir menu terdaftar |

### ✅ `FE-AST-014` — Kartu dashboard 1.4 dan laporan mutasi

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 23 November 2026.** Bagian **Mutasi & Peminjaman Berjalan** disisipkan ke Dashboard Aset sesudah Kondisi Aset (hanya bila `AssetTransfer : Read`; kartu pinjaman bila `canReadLoans`), kartu menaut ke tab daftar lewat `?tab=`; Laporan Mutasi `/reports/transfers` (varian daftar mutasi, tab status, Ekspor Excel); butir menu Laporan Mutasi. ESLint 14 berkas `0 errors, 0 warnings`. Playwright **20/22 + cek ulang 2/2** (dua FAIL awal = harapan skrip peka huruf pada label kartu/toast); 0 galat halaman, 0 respons 5xx; kartu register tetap, akun tanpa `AssetTransfer : Read` tidak melihat bagian 1.4 dan tidak memanggil API-nya, Excel dari layar sama dengan baris tab. `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-23 08:59:19). Laporan: [`FE-AST-014.md`](../task/report/frontend/FE-AST-014.md) |
| Acceptance criteria | 1. Kartu disisipkan ke dashboard `FE-AST-010` tanpa mengubah kartu lain; tersembunyi tanpa `AssetTransfer : Read`. 2. Laporan dan ekspor. 3. Butir menu "Laporan Mutasi" |
