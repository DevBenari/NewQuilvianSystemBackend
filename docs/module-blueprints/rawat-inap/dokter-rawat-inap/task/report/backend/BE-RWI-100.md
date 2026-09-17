# Laporan Perubahan Backend — `BE-RWI-100`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-100` |
| Judul | Penghentian butir resep membatalkan dosis MAR |
| Slice | Gelombang 2 — `DOK-V2-2`, dirilis bersama `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-100` |
| Trace | `FR-DOK-087`; `INT-KEP-09`; `INT-DOK-16`; `RWI-DEC-121`; state matrix 0.6.0 bagian 8.4 |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-099` ✅; **`BE-RWI-114` [BE-KEP] — belum dikerjakan** |
| Klasifikasi | `HEAVY` — transaksi lintas butir resep, dosis MAR, dan order sliding scale |
| Task mode | `BACKEND` |
| Target tulis | — (tidak ada source yang ditulis) |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 16 September 2026 |
| Status | ⛔ **TERBLOKIR** — menunggu `BE-RWI-114` [BE-KEP] (mesin MAR dan pembentukan dosis) |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / PharmacyManagement` |
| Blocker | Definition of Done kartu: "Task ini **tidak boleh** dimulai sebelum `BE-RWI-114` [BE-KEP] mendarat." |
| Bukti blocker | Roadmap `keperawatan/roadmap/backend-roadmap-v2.md` kartu `BE-RWI-114` berstatus "Belum dikerjakan"; pencarian source `MedicationAdministration`, `PhmMedicationAdministration`, `MarDose` → nol hasil |
| Database | Nol |

## 1. Masalah yang akan diperbaiki

Dokter menghentikan sebuah obat, tetapi dosis yang sudah terjadwal di MAR tetap terbaca harus
diberikan. Kriteria 2, 3, dan 5 (pembatalan dosis `Due` dalam transaksi yang sama, dosis lain tidak
tersentuh, galat buatan nol perubahan) tidak dapat ditulis sebelum tabel dan service MAR ada.

## 2. Yang sudah disiapkan task lain (bukan pekerjaan task ini)

| Prasyarat | Disiapkan oleh | Keadaan |
| --- | --- | --- |
| Kolom `IsStopped`, `StoppedAt`, `StoppedByUserId`, `StopReason` | `BE-RWI-099` (R4) | Source ada; migration belum dijalankan |
| Penghentian order sliding scale butir insulin dalam transaksi pemanggil | `BE-RWI-103` — `SlidingScaleOrderService.StopForPrescriptionItemAsync` (tidak menyimpan sendiri) | Source ada |
| `MedicationAdministrationService.CancelDueDosesForItemAsync` | `BE-RWI-114` [BE-KEP] | **Belum ada** |

## 3–4. Perubahan dan endpoint

`NOT APPLICABLE` — tidak ada source yang ditulis. Endpoint `PATCH /prescriptions/items/{itemId}/stop`
(`Prescription : Stop`) belum dibuat.

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pemeriksaan prasyarat `BE-RWI-114` | Belum ada | `NOT RUN` | Bagian preflight |

## 6. Acceptance criteria

| Kriteria | Status |
| --- | --- |
| 1–5 | Belum terpenuhi — terblokir `BE-RWI-114` [BE-KEP] |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Selama task ini tertahan, penghentian butir resep dari Resep Harian belum tersedia |
| Status Git | Hanya berkas laporan ini (`??`) |
| Langkah berikutnya | Kerjakan `BE-RWI-114` di roadmap `keperawatan`, lalu kerjakan task ini memanggil `CancelDueDosesForItemAsync` dan `StopForPrescriptionItemAsync` dalam satu transaksi |
