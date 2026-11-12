# Inventarisasi Fisik — Roadmap Frontend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` |
| Blueprint / contract | `AST-BP-001` rev 1, `inventarisasi`, `AST-INV-1.0.0` `approved` |
| Frontend SHA | `e49e8a81` (`RizkiV2`) |
| Task | `FE-AST-015` s.d. `FE-AST-018` (`MVP-3`) |
| Handoff | `build-module-frontend`, satu task per pemanggilan, `FRONTEND MODE` eksplisit. Pilihan pustaka pemindai QR = `DEV_DISCRETION` dengan syarat pada `03-frontend-architecture.md` §7 |

**Legenda tanda status:** ✅ selesai · 🟡 sebagian · ⛔ terblokir · tanpa tanda = belum dikerjakan.

`V-FE` dan `DoD-FE` sama dengan `../../register/roadmap/frontend-roadmap.md`; tambahan untuk `FE-AST-016`:
pemindaian diuji pada HP lewat HTTPS.

## Grafik Urutan Dependency

Dipotong menjadi dua blok karena lebar; `FE-AST-017` muncul lagi sebagai sambungan blok kedua.

```text
                   BE-AST-021 [BE] ─┐    BE-AST-022 [BE] ─┐    BE-AST-023 [BE] ─┐
                                    │                     │                     │
FE-AST-001 [FE-REG] ────────────────┴─> FE-AST-015 ───────┴─> FE-AST-016 ───────┴─> FE-AST-017
```

```text
                  BE-AST-024 [BE] ─┐
                                   │
         FE-AST-017 ─┬─────────────┴─> FE-AST-018
                     │
FE-AST-010 [FE-REG] ─┘
```

`[BE]` = task backend pada `backend-roadmap.md` sub-modul ini · `[FE-REG]` = task frontend sub-modul
`register`. Cermin baca-saja.

Pasangan prasyarat→task: **9** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `FE-AST-001` [FE-REG], `BE-AST-021` [BE] | `FE-AST-015` |
| 2 | `FE-AST-015`, `BE-AST-022` [BE] | `FE-AST-016` |
| 3 | `FE-AST-016`, `BE-AST-023` [BE] | `FE-AST-017` |
| 4 | `FE-AST-017`, `BE-AST-024` [BE], `FE-AST-010` [FE-REG] | `FE-AST-018` |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-AST-015` | Jadwal dan detail sesi | `CAP-07.1` | API 2.1 | `data-table`, `resource-filter-select` | `SCR-AST-INV-01`, `08` + grup menu Inventaris | `FE-AST-001`, `BE-AST-021` | Kartu | `V-FE`, `UAT-INV-02` | — | `DoD-FE` |
| `FE-AST-016` | Pemindaian QR di lokasi | `CAP-07.2`, `AST-FE-003` | API 2.1 (pindai, temuan) | Akses kamera seperti `UploadPhotoField.jsx` | `qr-scanner.jsx`, `SCR-AST-INV-02` | `FE-AST-015`, `BE-AST-022` | Kartu | `V-FE`, `AT-INV-13`, `38`; `UAT-INV-06` | Pustaka pemindai alpha (`TS-18`) | `DoD-FE` |
| `FE-AST-017` | Pencocokan, selisih, verifikasi, berita acara, tindak lanjut | `CAP-07.3..07.6` | API 2.1, 2.2 | `confirm-modal` | `SCR-AST-INV-03..06` | `FE-AST-016`, `BE-AST-023` | Kartu | `V-FE`, `UAT-INV-01`, `04`, `05` | — | `DoD-FE` |
| `FE-AST-018` | Kartu 1.5 dan laporan inventaris | `CAP-01.5`, `11.4` | API 2.3–2.4 | `summary-grid` | Kartu di `SCR-AST-REG-01`, `SCR-AST-INV-07` | `FE-AST-017`, `BE-AST-024`, `FE-AST-010` | Kartu | `V-FE` | — | `DoD-FE` |

## Kartu task

### `FE-AST-015` — Jadwal dan detail sesi

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Jadwalkan dengan unit, ruangan opsional, tanggal, tim dan ketua. 2. Mulai menampilkan daftar proses yang menahan bila ditolak. 3. Grup menu "Inventaris & Pemeriksaan Fisik" dan butir "Jadwal Inventaris" terdaftar |

### `FE-AST-016` — Pemindaian QR

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Skema §3.1; dapat dipakai satu tangan di HP 360 px. 2. Kamera ditolak → input nomor manual tetap bekerja. 3. Nomor tak dikenal → formulir temuan. 4. Pindaian gagal kirim tetap terlihat dengan tombol kirim ulang. 5. Butir menu "Pemeriksaan Fisik" |

### `FE-AST-017` — Pencocokan sampai tindak lanjut

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. `SCR-AST-INV-03..06`. 2. Tombol verifikasi tersembunyi bagi anggota tim. 3. Proses usulan menampilkan nomor dokumen tujuan. 4. Empat butir menu terdaftar |

### `FE-AST-018` — Kartu 1.5 dan laporan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Kartu "Inventaris Tertunda" disisipkan ke dashboard tanpa mengubah kartu lain. 2. Laporan dan ekspor. 3. Butir menu "Inventaris" pada grup Laporan |
