# Laporan Perubahan Backend — `BE-BKC-FIX-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-017` |
| Judul | Pemakaian obat/BMHP tanpa resep ditagih per butir saat dicatat; depo dan petugas ditentukan server |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-03` (+ sisi server `FIX-BKC-001-02`); temuan `ISS-BKC-001-02` |
| Keputusan | `BKC-DEC-138` (K-04, pilihan pemilik 12 Oktober 2026; selaras `IGD-DEC-244`) |
| Target tulis | `NewQuilvianSystemBackend` |
| Commit saat dikerjakan | `d9089882` (`MHamzah`) + working tree; belum di-commit |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**, build 0 error, uji resolver lulus. Uji transaksi stok–fakta–invoice `NOT RUN` |

## 1. Masalah

- `DrugUsageService.RecordAsync` mengurangi stok lalu berhenti di `NotBilled`; tidak ada pengirim ke Billing. Spuit/kassa yang dipakai pasien tanpa resep tidak pernah masuk invoice.
- Kontrak create mewajibkan `StorageLocationId` dan `RecordedByWorkforceId`. Keranjang Alkes perawat mengirim GUID kosong (komentarnya: "akan diisi depo backend") padahal backend tidak punya resolusi itu, sehingga draft ditolak `422`.
- Retur atas pemakaian tanpa resep tidak menyentuh tagihan.

## 2. Proses bisnis sesudah perbaikan

1. **Draft.** Perawat/Farmasi menyusun draft. Bila depo kosong, server memakai depo sesuai kunjungan (`PharmacyDepotRoutingService`, aturan depo yang sudah ada). Bila petugas kosong, server memakai profil petugas yang tertaut pada akun login (`ApplicationUser.WorkforceProfileId`). Keduanya tetap divalidasi; gagal → `422` dengan sebab yang terbaca. Draft tidak menagih.
2. **Catat** (hak `DrugUsage : Record`). Stok berkurang (FEFO per batch); sesudah tersimpan, satu fakta `DrugUsage`/`DrugUsageCharge` diterbitkan **per butir** (jumlah dicatat, waktu pemakaian).
3. Billing meneruskannya ke invoice kunjungan sebagai `CONSUMABLE`/`USED`, harga dari tarif obat/alkes yang berlaku; tanpa tarif → antrean rekonsiliasi `TARIFF_NOT_FOUND`, bukan Rp0.
4. **Kirim ulang** perintah catat atas pemakaian yang sudah tercatat tidak menyentuh stok; hanya butir yang belum pernah punya fakta yang diterbitkan.
5. **Retur** pemakaian tanpa resep: jumlah kembali per obat dibagikan ke butir menurut urutan baris; butir yang berubah mendapat fakta pembatalan bermarka `DrugReturn` berisi sisa jumlah yang tetap ditagih → baris dikurangi (atau adjustment bila invoice final).
6. **Detail pemakaian** kini memuat `chargeState`/`chargeStateMessage` dari Billing: menunggu Billing, masuk invoice, perlu ditinjau, dst.

> **Contoh.** 3 spuit @Rp5.000 dicatat pukul 10.00 → satu item Rp15.000 di invoice. Pukul 11.00 satu spuit utuh dikembalikan dan diverifikasi → item menjadi 2 × Rp5.000 = Rp10.000. Draft yang belum dicatat tidak menambah apa pun.

**Batas.** Pemakaian yang menempel pada resep (penyerahan resep, `PrescriptionId` terisi) tidak pernah ditagih dari sini — resolver mengembalikan `NOT_BILLABLE`. Bahan operasi memakai jalur `OPERATING_ROOM`. Bahan Radiologi tetap belum ditagih tersendiri (K-04).

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `BillingManagement/Operational/Constants/BillingSourceContract.cs` | Sumber kontrak baru `DrugUsage`/`DrugUsageCharge` + milestone `DrugUsageRecorded` |
| `BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | `DrugUsage` menjadi kandidat; domain `CONSUMABLE`, status tagih `USED`; detail item = `SourceItemId` (butir) |
| `BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | `ResolveConsumableAsync`: butir + status pemakaian, tolak draft, `NOT_BILLABLE` untuk penyerahan resep, tarif obat/alkes |
| `BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` | Komentar kebijakan `CONSUMABLE` (status `USED` sudah ada) |
| `PharmacyManagement/DTOs/DrugUsageDtos.cs` | `CreateDrugUsageRequest.StorageLocationId`/`RecordedByWorkforceId` menjadi opsional (`Guid?`); `DrugUsageDetailResponse.ChargeState`/`ChargeStateMessage`; `DrugUsageItemResponse.ChargeState` |
| `PharmacyManagement/Services/DrugUsageService.cs` | Resolusi depo/petugas; `EmitUsageChargesAsync` sesudah catat dan pada kiriman ulang; `ApplyChargeStateAsync` pada detail; dependensi `PharmacyDepotRoutingService`, `ClinicalMilestoneFactProducer`, `BillingClinicalChargeBridgeService` |
| `PharmacyManagement/Services/DrugReturnBillingHandoffService.cs` | `EmitNonPrescriptionReturnAsync` untuk retur pemakaian tanpa resep |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /drug-usages`: dua field menjadi opsional (kompatibel ke belakang). `GET /drug-usages/{id}`: field baru `chargeState`, `chargeStateMessage`, `items[].chargeState` |
| Database | Tidak ada migration. Status `PhmDrugUsage` tetap `NotBilled`; keadaan tagih dibaca dari Billing, bukan ditulis ke tabel Farmasi |
| Keamanan | Hak akses tidak berubah: create `DrugUsage : Create`, catat `DrugUsage : Record` |

### Health Services / Pharmacy Management / Drug Usage

Base URL: `api/v1/health-services/pharmacy-management/drug-usages`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Draft pemakaian; depo/petugas boleh kosong (ditentukan server) | `DrugUsage : Create` |
| `POST` | `/{id}/record` | Catat pemakaian, kurangi stok, terbitkan fakta tagih per butir | `DrugUsage : Record` |
| `GET` | `/{id}` | Detail + keadaan tagih | `DrugUsage : Read` |

`422 PHM063` akun belum tertaut profil petugas; `422 PHM064` depo tidak dapat ditentukan otomatis atau tidak boleh menyerahkan.

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build` | **0 error**, warning baru 0 |
| Uji resolver (`CheckoutBillingIntegrationTests`) | **Lulus**: `Tiga_spuit_nonresep_ditagih_per_butir_dengan_tarif_alkes` (3 × Rp5.000 = Rp15.000), `Penyerahan_resep_tidak_ditagih_lagi_sebagai_pemakaian`, `Draft_pemakaian_tidak_pernah_ditagih`, `Pemakaian_tanpa_tarif_masuk_rekonsiliasi_bukan_nol`, kontrak sumber/adapter |
| Uji transaksi PostgreSQL (catat → stok → fakta → invoice → retur) | `NOT RUN` |

## 5. Acceptance criteria (`FIX-BKC-001-03`)

| AC | Bukti |
| --- | --- |
| 1. 3 spuit × Rp5.000 → Rp15.000 dengan tautan butir | Resolver + uji; `SourceDetailId` = id butir pemakaian |
| 2. Draft tidak menagih; replay tidak mengurangi stok atau menambah invoice | Fakta hanya terbit sesudah catat; kiriman ulang melewati stok dan hanya menerbitkan butir tanpa fakta; kunci deterministik jembatan |
| 3. Penyerahan resep/bahan operasi tidak mendapat charge kedua; retur mengikuti kontrak | `NOT_BILLABLE` untuk `PrescriptionId`; bahan operasi tidak memakai `DrugUsageService`; retur → fakta `DrugReturn` per butir |

## 6. Risiko dan catatan

- Akun perawat tanpa tautan profil petugas tidak dapat menyusun draft dari keranjang; pesannya mengarahkan ke admin.
- Pemakaian tanpa resep yang dicatat **sebelum** perubahan ini tidak ditagih surut (tidak ada backfill).
