# Laporan Perubahan Backend — `BE-BKC-FIX-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-016` |
| Judul | Jembatan fakta klinis → invoice mencakup kunjungan IGD |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-08`; temuan `ISS-BKC-001-06` (sisi consumer) |
| Keputusan | `BKC-DEC-139` (K-05) — turunan `BKC-DEC-079`, `BKC-DEC-118`, `IGD-DEC-246`/`247`; bukan kebijakan baru |
| Dependency | `BE-BKC-FIX-019` (producer tindakan IGD yang langsung Completed); dirilis bersama `BE-BKC-FIX-015` |
| Target tulis | `NewQuilvianSystemBackend` |
| Commit saat dikerjakan | `d9089882` (`MHamzah`) + working tree; belum di-commit |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**, build 0 error. Uji transaksi PostgreSQL `NOT RUN` |

## 1. Masalah

`BillingClinicalChargeBridgeService.BuildPlanAsync` menolak kunjungan selain rawat jalan/rawat inap (`NOT_OUTPATIENT`). Fakta tindakan, Lab, Radiologi, alat, dan resep pada kunjungan IGD berhenti di folio dengan status `NotApplicable` dan tidak pernah sampai ke invoice yang dibaca kasir. Satu-satunya yang lolos hanyalah resep, lewat pengirim langsung yang kini dihapus `BE-BKC-FIX-015`.

## 2. Proses bisnis sesudah perbaikan

1. Pelayanan pada kunjungan IGD mencapai titik tagihnya (tindakan Completed, spesimen diterima, citra layak, obat diserahkan/resep difinalkan, pemakaian alat selesai, BMHP dicatat, tindakan Bank Darah selesai).
2. Fakta diterima folio, lalu jembatan meneruskannya ke **invoice kunjungan IGD itu sendiri** dengan titik tagih rawat jalan (`BKC-DEC-079`): resep IGD tahap 1 `PRESCRIBED`, sama seperti perilaku sebelumnya.
3. Pasien IGD yang naik rawat inap: item tetap di invoice IGD; invoice rawat inap menautnya lewat `BilInvoiceEncounterLinks` untuk settlement terpadu (`BKC-DEC-118`). Tidak ada item yang dipindah, sehingga tidak ada tagihan ganda.
4. Tagihan susulan setelah invoice IGD final menjadi adjustment (aturan jembatan yang sudah ada).

> **Contoh.** Pasien IGD dipasang infus (tindakan Rp75.000) dan Lab darah lengkap (Rp120.000), lalu naik rawat inap. Kedua item masuk invoice IGD seketika. Saat pulang, kasir menyelesaikan invoice IGD dan rawat inap dalam satu settlement; pelaporan unit tetap terpisah.

**Tidak berubah:** jasa konsultasi per catatan dokter IGD tetap tidak dikirim (`IGD-DEC-229`); jasa kunjungan dan observasi IGD tetap milik slice Billing IGD (`IGD-DEC-242`..`247`) yang dikerjakan pemilik modul IGD.

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | Gerbang jenis kunjungan menerima `EncounterType.Emergency`; pesan penolakan diperbarui; komentar keputusan |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak berubah |
| Database | Tidak ada migration |
| Keamanan | Tidak berubah |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build` (lihat `BE-BKC-FIX-015`) | **0 error**, warning baru dari berkas ini 0 |
| Uji IGD pulang dan IGD → rawat inap di database (skenario 12 bagian 8 plan-repair) | `NOT RUN` |

## 5. Acceptance criteria (`FIX-BKC-001-08`)

| AC | Bukti |
| --- | --- |
| 1. Pelayanan eligible pada encounter Emergency masuk invoice tujuan yang disetujui | Gerbang jembatan; tujuan = invoice `EncounterId` kunjungan IGD (query invoice yang sama dengan rawat jalan) |
| 2. Transfer ke ranap tidak menagih dua kali | Item tidak dipindah; konsolidasi lewat tautan invoice yang sudah ada |
| 3. Replay/late charge ke invoice yang tepat, adjustment bila final | Kunci deterministik dan cabang adjustment jembatan dipakai apa adanya |

## 6. Risiko dan catatan

- **Fakta IGD lama** yang sudah tercatat `NotApplicable` sebelum perubahan ini tidak diproses ulang otomatis. Pemulihannya butuh tindakan backfill terpisah (mengubah status efek menjadi `Pending` lalu dikirim ulang) dengan otorisasi data tersendiri.
- Slice Billing IGD (`IGD-UNK-20`) masih mencari jalur jasa kunjungan IGD; perubahan ini tidak mengirim jasa kunjungan dan tidak membuat sumber `EMERGENCY`.
