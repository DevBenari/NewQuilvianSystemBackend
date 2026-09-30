# Kiosk — Status Modul

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` |
| Nama modul | Kiosk (Revisi Module Kiosk & Cek Nomor Rekam Medis) |
| Revision | `1` |
| Status modul | `IN_PROGRESS` — blueprint & roadmap approved (Sukma, 30 Sep 2026) |
| Fase saat ini | `KSK-PH-003` — Perencanaan delivery |
| Terakhir diverifikasi | belum diverifikasi |
| SHA source backend | `419b910fca5188285946850a95976ebff83ae8ce` |
| SHA source frontend | `4ec51b0bf5e724e899b95f118e273351437b71e7` |

## Keadaan fase

| Fase selesai | Fase aktif | Fase terblokir |
| --- | --- | --- |
| `KSK-PH-001` Discovery, `KSK-PH-002` Desain | `KSK-PH-003` Perencanaan delivery | Tidak ada |

## Keadaan delivery

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| `IN_PROGRESS` — `BE-KSK-001` ✅, `BE-KSK-002` ✅ | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

## Keputusan

Seluruh keputusan tercatat di [00-interview-decisions.md](00-interview-decisions.md) r2 (`KSK-DEC-001..019`, status `approved`). Keputusan terbuka PRD dan seluruh conflict/unknown capability map r1 sudah ditutup.

## Blocker dan pemiliknya

Tidak ada blocker desain. Dua tindak lanjut lintas blueprint berada di luar wewenang tulis blueprint Kiosk dan baru memblokir **implementasi**:

| ID | Ringkasan | Pemilik | Fase terdampak | Bisa jalan terpisah |
| --- | --- | --- | --- | --- |
| `KSK-OQ-004` | Amendment blueprint Laboratorium `FE-LAB-13` / `AC-93` (urutan & waktu sesi) | Sukma | Build FE Flow Pasien Lama | Ya |
| ~~`KSK-OQ-005`~~ | **Ditutup 30 Sep 2026** — `RWI-ENC-PAYER-001` `1.1.0` tercatat di blueprint rawat-inap | — | — | — |

## Bukti usang

| Artefak/bukti | SHA tercatat | SHA saat ini | Impact review |
| --- | --- | --- | --- |
| — | — | — | — |

## Tindakan berikutnya yang disarankan

1. Roadmap `KSK-RM-BE-001` r1 dan `KSK-RM-FE-001` r1 **approved** (Sukma, 30 Sep 2026).
2. Task yang boleh jalan setelah approval (gelombang 1): `BE-KSK-001` (`TASK MODE: BACKEND`), `FE-KSK-001` dan `FE-KSK-002` (`TASK MODE: FRONTEND`).
3. Tutup `KSK-OQ-004` (membuka `FE-KSK-007`) dan `KSK-OQ-005` (membuka `BE-KSK-003` → `FE-KSK-008`).

## Progres delivery

`2 / 11` task approved selesai (3 BE, 8 FE): `BE-KSK-001` ✅, `BE-KSK-002` ✅ — 30 Sep 2026 ([laporan 001](task/report/backend/BE-KSK-001.md), [laporan 002](task/report/backend/BE-KSK-002.md)). Tertahan: `FE-KSK-007` (`KSK-OQ-004`), `FE-KSK-008` (turunan). `BE-KSK-003` terbuka sejak `KSK-OQ-005` ditutup 30 Sep 2026.
