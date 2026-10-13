# Laporan Perubahan Backend — `BE-BKC-FIX-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-015` |
| Judul | Satu jalur tagihan resep (fakta → folio → invoice) dan racikan ditagih bahan × tarif obat |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-05`; temuan `ISS-BKC-001-04` |
| Keputusan | `BKC-DEC-136` (K-02, pilihan pemilik 12 Oktober 2026) |
| Dependency | Wajib bersama `BE-BKC-FIX-016` (jembatan mencakup IGD). Tanpa itu, resep IGD kehilangan tagihan begitu pengirim langsung dihapus |
| Target tulis | `NewQuilvianSystemBackend` |
| Commit saat dikerjakan | `d9089882` (`MHamzah`) + working tree; belum di-commit. Agen lain sedang mengubah `BillingClinicalChargeBridgeService*.cs` (kelas layanan, `InpServiceClassResolver`) pada working tree yang sama — perubahannya dipertahankan |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**, build 0 error, uji resolver lulus. Uji transaksi PostgreSQL `NOT RUN` |

## 1. Masalah

Finalisasi konsultasi menerbitkan fakta resep yang sudah diteruskan jembatan ke invoice, lalu **memanggil lagi** `PrescriptionBillingChargeProducer` yang menulis item invoice sendiri:

- Rawat jalan: dua pengirim memakai `PHARMACY`/ID resep/versi 1 dengan deskripsi berbeda → pengirim kedua ditolak "versi sama isi berbeda" (`PHM_CHARGE_FAILED`) walau tagihan sudah ada.
- Rawat inap: jembatan menunggu obat diserahkan, pengirim langsung menagih `TotalPrice` saat finalisasi → obat tertagih sebelum diserahkan (melanggar `INT-RWF-04`).
- Racikan: resolver menolak seluruh resep, pengirim langsung menagih total agregat → dua aturan untuk resep yang sama.

## 2. Proses bisnis sesudah perbaikan

1. Dokter menyelesaikan konsultasi; resep draft difinalkan.
2. Sesudah commit, satu fakta `Prescription`/`PrescriptionCharge` tahap 1 diterbitkan. **Hanya itu** — tidak ada pengirim kedua.
3. Jembatan meneruskannya: rawat jalan dan IGD → item `PHARMACY`/`PRESCRIBED`; rawat inap → menunggu diserahkan (`NOT_CHARGED`).
4. Saat Farmasi menyerahkan obat, fakta direvisi (`DISPENSED`, jumlah aktual) dan item diperbarui; invoice final → adjustment.
5. Hasil sebenarnya dilaporkan ke dokter pada `BillingHandoffIssues`: "perlu ditinjau" (mis. `TARIFF_NOT_FOUND`) atau "belum masuk invoice, dicoba ulang". Resep rawat inap yang menunggu diserahkan bukan keluhan.

**Racikan (K-02).** Satu baris per resep = Σ obat reguler (jumlah resep/serah × tarif) + Σ bahan racikan aktif (`PricingQuantity` × tarif obat bahan). Jasa racik belum ditagih. Satu bahan tanpa tarif menahan seluruh resep.

> **Contoh.** Resep: obat A 2 × Rp10.000, puyer berisi bahan B 3 × Rp2.000. Rawat jalan tahap 1 → Rp26.000. Saat diserahkan baru 1 tablet A → tahap 2 Rp16.000 (1 × Rp10.000 + bahan racikan tetap 3 × Rp2.000, karena penyerahan per bahan belum dicatat Farmasi). Rawat inap: tidak ada tagihan sampai diserahkan.

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` | Panggilan `PrescriptionBillingChargeProducer` dihapus; sesudah fakta terbit, keadaan invoice dibaca lewat `BillingClinicalChargeBridgeService.GetChargeStateAsync` dan dilaporkan sebagai `BillingHandoffIssues` (`DescribeChargeIssue`) |
| `Areas/HealthServices/PharmacyManagement/Services/PrescriptionBillingChargeProducer.cs` | **Dihapus** — tidak ada pemanggil lain |
| `Program.cs` | Registrasi DI `PrescriptionBillingChargeProducer` dihapus |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | `ResolvePharmacyAsync`: penahan racikan dicabut; bahan racikan aktif dihitung `PricingQuantity` × tarif (tarif bahan → tarif obat); bahan tanpa tarif → `TARIFF_NOT_FOUND` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | Metode baca `GetChargeStateAsync` + `BillingSourceChargeState`/`BillingSourceChargeStates` (dipakai juga `BE-BKC-FIX-017`) |
| `Tests/QuilvianSystemBackend.PharmacyTests/BillingChargeProducerTests.cs` | **Dihapus** bersama kelas yang diujinya. Uji validator adapter tahap 1/kontrak lama dipindah ke `CheckoutBillingIntegrationTests.cs` |
| `Tests/QuilvianSystemBackend.PharmacyTests/CheckoutBillingIntegrationTests.cs` | **Baru** (bersama task FIX lain) |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `PATCH api/v1/health-services/clinical-management/doctor-consultations/{id}/complete` (`DoctorConsultation : Complete`) — bentuk respons tetap; isi `billingHandoffIssues` untuk resep kini berupa hasil invoice, bukan kode `PHM_CHARGE_*` |
| Database | Tidak ada migration |
| Keamanan | Tidak berubah |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -m:1 -p:UseSharedCompilation=false -p:RunAnalyzers=false` (keluaran ke scratchpad, `--no-incremental` untuk hitung warning) | **0 error**. Warning dari berkas yang diubah task ini: **0** (satu CS1573 sempat muncul lalu diperbaiki). Total warning proyek 291 — bandingan garis dasar 230 (30-09) tidak dapat dinilai karena agen lain sedang mengubah working tree |
| `dotnet test` proyek uji sementara (`TestDatabase.cs` + `CheckoutBillingIntegrationTests.cs`) | **15/15 lulus**, termasuk `Resep_dengan_racikan_menagih_bahan_kali_tarif_obat`, `Bahan_racikan_tanpa_tarif_menahan_seluruh_resep`, `Tahap_serah_memakai_jumlah_diserahkan_dan_bahan_racikan_tetap_ditagih`, uji validator tahap 1 |
| `dotnet test Tests/QuilvianSystemBackend.PharmacyTests` | `EXISTING / BROKEN`: proyek tidak terkompilasi **sebelum** task ini — `BillingInvoiceEligibilityTests.cs` (CS0246/CS7036/CS0117) dan `BillingZeroPatientObligationTests.cs` (CS0117) memakai API lama. Tidak disentuh |
| Uji transaksi PostgreSQL (finalisasi → item invoice → dispense → adjustment) | `NOT RUN` — butuh database uji; dikecualikan sesuai keputusan pemilik bahwa build/uji berat dijalankan sendiri |

## 5. Acceptance criteria (`FIX-BKC-001-05`)

| AC | Bukti |
| --- | --- |
| 1. Resep reguler rawat jalan → satu item, tanpa konflik pengirim kedua | Pengirim kedua dihapus; hanya jembatan yang menulis item (`ConsultationFinalizationService.cs`, `PrescriptionBillingChargeProducer.cs` dihapus) |
| 2. Resep rawat inap belum diserahkan tidak ditagih; serah 2 dari 3 menagih jumlah aktual | Aturan jembatan `encounter.EncounterType == Inpatient && !isDispensed` kini satu-satunya jalur; tahap 2 memakai `DispensedQuantities` |
| 3. Resep campuran reguler/racikan berperilaku satu dan disetujui | Resolver menghitung bahan sesuai `BKC-DEC-136`; uji 3 skenario lulus |
| 4. Clearance, revisi, final/adjustment, retur tetap terlacak | Jalur fakta/jembatan/retur yang sudah ada dipakai apa adanya; `SourceDetailId` tetap ID resep sehingga surat clearance menemukan resepnya |

## 6. Risiko dan catatan

- **Invoice lama** yang sudah memuat item dari pengirim langsung tetap ada; revisi tahap 2 memperbaruinya seperti biasa. Tidak ada backfill.
- **Racikan rawat inap** baru tertagih saat resepnya pertama diserahkan; resep yang hanya berisi racikan dan tidak pernah diserahkan lewat sistem tidak tertagih — sesuai aturan rawat inap, tetapi perlu diketahui Farmasi.
- **Retur bahan racikan** tidak otomatis mengurangi tagihan (K-02: adjustment manual).
