# Laporan Perubahan Backend — `BE-BKC-FIX-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-023` |
| Judul | Penyaringan kunjungan yang sudah selesai dan lunas (*closed billing*) pada opsi pemilihan kunjungan invoice manual |
| Modul | `HealthServices / BillingManagement / Billing` |
| Target tulis | `NewQuilvianSystemBackend` |
| Tanggal | 2026-10-15 |
| Berkas yang diubah | 1. `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs`<br>2. `Tests/QuilvianSystemBackend.PharmacyTests/BillingInvoiceEligibilityTests.cs` |
| Klasifikasi | `LIGHT` — Tanpa skema database baru, tanpa migration, perbaikan kueri LINQ dan validasi defensif |
| Status | ✅ **Source dan Unit Tests Selesai Diperbarui** |

---

## 1. Masalah yang Diperbaiki

Pada halaman pengujian / pembuatan tagihan manual (**Tambah Invoice Manual** / `create-manual-invoice-view.jsx`), kasir atau staf keuangan memilih kunjungan pasien yang akan dibuatkan invoice atau ditambahkan biaya katalognya.

Pilihan kunjungan diambil melalui endpoint backend:
`GET /v1/health-services/billing-management/billing/invoices/encounter-options`

Sebelumnya, kueri LINQ pada method `GetActiveEncounterOptionsAsync` hanya memeriksa tabel kunjungan (`RegPatientEncounters`):
```csharp
where !encounter.IsDelete
    && !encounter.IsCancel
    && encounter.IsActive
    && ManualChargeEligibleEncounterStatuses.Contains(encounter.EncounterStatus)
    && !encounter.CompletedAt.HasValue
```

**Kelemahan dan Penyebab Cacat:**
1. Kueri tersebut **sama sekali tidak memeriksa status tagihan** pada tabel `BilInvoices`.
2. Ketika tagihan pasien telah dilunasi dan ditutup (*closed billing*), status invoice di `BilInvoices` berubah menjadi `CLOSED` dengan penanda waktu `ClosedAt` terisi. Namun pada data registrasi kunjungan (`RegPatientEncounters`), status kunjungan masih berada pada status `Billing` dan kolom `CompletedAt` bernilai `null` (karena proses penutupan kepulangan fisik/medis terpisah dari kasir).
3. Akibatnya, pasien seperti **Walter White** dan **Jesse Pinkman** yang tagihannya sudah lunas dan berstatus *closed billing* tetap muncul pada dropdown pemilihan kunjungan dengan label tambahan `· sudah ada invoice`.
4. Jika kasir mencoba memilih kunjungan tersebut dan menambahkan biaya, sistem backend menolaknya dengan galat validasi 422:
   > *"Tagihan susulan ditolak karena invoice telah ditutup. Pembukaan kembali memerlukan persetujuan Supervisor Kasir."*

Kunjungan yang sudah lunas dan selesai penagihannya (*closed billing*) seharusnya **tidak boleh muncul sama sekali** dalam daftar pemilihan kunjungan pembuatan invoice manual.

---

## 2. Alur Proses Bisnis Sesudah Perbaikan

1. **Kasir Membuka Layar Tambah Invoice Manual**:
   - Kasir mengakses menu penambahan invoice manual untuk pasien rawat jalan, IGD, atau rawat inap.
2. **Sistem Menarik Opsi Kunjungan Aktif (`GET encounter-options`)**:
   - Backend melakukan kueri ke `RegPatientEncounters` yang digabungkan dengan pengecekan tabel `BilInvoices`.
   - Sistem secara otomatis **mengecualikan** seluruh kunjungan yang sudah memiliki invoice berstatus:
     - `CLOSED` (tagihan sudah lunas dan kasir telah menutup billing).
     - `SETTLED_BY_WRITE_OFF` (tagihan telah diselesaikan melalui pemutihan / write-off).
     - `FINAL` (tagihan telah difinalkan dan siap bayar / tidak boleh ditambah item manual tanpa adjustment).
     - Memiliki `ClosedAt` yang sudah terisi.
3. **Penyajian Daftar Kunjungan yang Memenuhi Syarat**:
   - Pasien yang **belum memiliki invoice sama sekali** akan tampil (tanpa tanda `sudah ada invoice`).
   - Pasien yang memiliki invoice tetapi statusnya masih **`OPEN`** akan tampil dengan tanda `· sudah ada invoice` (karena biaya baru masih dapat disusulkan ke invoice berjalan).
   - Pasien yang sudah selesai dan lunas (*closed billing*, seperti Walter White dan Jesse Pinkman) **bersih dari daftar dropdown**.
4. **Pencegahan Galat Transaksi**:
   - Kasir tidak lagi dapat secara keliru memilih pasien yang tagihannya sudah ditutup, sehingga terhindar dari galat penolakan sistem saat menekan tombol simpan tagihan.

> **Contoh Skenario Rumah Sakit:**  
> - **Skenario Pasien Lunas (Walter White & Jesse Pinkman):** Pasien Tn. Walter White telah menyelesaikan konsultasi Rawat Jalan, seluruh invoice telah dibayar lunas oleh asuransi/pasien, dan kasir telah menerbitkan kuitansi pelunasan (`BilInvoice.Status = 'CLOSED'`). Saat kasir membuka form Tambah Invoice Manual, nama Walter White dan Jesse Pinkman tidak lagi muncul dalam daftar dropdown pasien karena tagihan telah rampung.  
> - **Skenario Pasien Berjalan (Agnes Yuliani):** Pasien Ny. Agnes masih dalam proses pelayanan rawat jalan dan invoice-nya masih berstatus `OPEN`. Nama Agnes tetap muncul di dropdown dengan penanda `· sudah ada invoice`, sehingga kasir dapat menambahkan tindakan katalog baru ke invoice berjalan tersebut.  
> - **Skenario Pasien Baru (Mira Setiawan):** Pasien Nn. Mira baru didaftarkan di IGD dan belum memiliki invoice. Nama Mira muncul di dropdown tanpa penanda invoice, dan saat dipilih kasir dapat membuatkan invoice perdana.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### [Tags("BillingInvoice")]

| Aspek | Detail |
| --- | --- |
| **Method** | `GET` |
| **Path** | `/v1/health-services/billing-management/billing/invoices/encounter-options` |
| **Deskripsi** | Mengambil daftar pilihan kunjungan aktif pasien yang belum selesai dan belum lunas (*not closed*) untuk entri invoice manual / pengujian billing. |
| **Otorisasi** | Bearer Token JWT; Permission: `BillingInvoice:Read` |
| **Query Parameters** | `search` (string, opsional) — Kata kunci pencarian (Nama Pasien, No RM, atau No Kunjungan)<br>`limit` (int, default 25, aman 1–100) — Jumlah batas data |

#### Respons Berhasil (`200 OK`)

```json
{
  "success": true,
  "message": "Daftar kunjungan aktif berhasil diambil.",
  "data": [
    {
      "id": "7f8b9c0d-1234-5678-9abc-def012345678",
      "encounterNumber": "ENC-20261015-0001",
      "patientName": "AGNES YULIANI RAJA GUK GUK",
      "medicalRecordNumber": "00-00-00-13",
      "encounterType": "Outpatient",
      "encounterStatus": "Billing",
      "encounterDate": "2026-10-15T08:30:00Z",
      "hasInvoice": true,
      "serviceType": "RAJAL",
      "paymentType": "Cash",
      "paymentTypeLabel": "Tunai",
      "guarantorName": null,
      "serviceUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "clinicId": "2fa85f64-5717-4562-b3fc-2c963f66afa5",
      "patientClassId": null
    }
  ]
}
```

---

## 4. Rincian Perubahan Kode

### A. `BillingInvoiceService.cs` (`GetActiveEncounterOptionsAsync`)
Menambahkan klausa `NOT EXISTS` terhadap entitas `BilInvoices` pada kueri utama:
```csharp
where !encounter.IsDelete
    && !encounter.IsCancel
    && encounter.IsActive
    && ManualChargeEligibleEncounterStatuses.Contains(encounter.EncounterStatus)
    && !encounter.CompletedAt.HasValue
    && !_dbContext.BilInvoices.Any(inv =>
        inv.EncounterId == encounter.Id
        && !inv.IsDelete
        && !inv.IsCancel
        && (inv.Status == BillingInvoiceStatuses.Closed
            || inv.Status == BillingInvoiceStatuses.SettledByWriteOff
            || inv.Status == BillingInvoiceStatuses.Final
            || inv.ClosedAt.HasValue))
```

Serta memperketat penentuan `HasInvoice` hanya untuk invoice yang aktif dan berstatus `OPEN`:
```csharp
var invoicedEncounterIds = encounterIds.Count == 0
    ? []
    : await _dbContext.BilInvoices.AsNoTracking()
        .Where(x => encounterIds.Contains(x.EncounterId)
            && !x.IsDelete
            && !x.IsCancel
            && x.Status == BillingInvoiceStatuses.Open
            && !x.ClosedAt.HasValue)
        .Select(x => x.EncounterId)
        .Distinct()
        .ToListAsync(cancellationToken);
```

### B. `BillingInvoiceService.cs` (`AddCatalogChargeAsync`)
Memperkuat perlindungan mutasi:
```csharp
if (invoice.Status == BillingInvoiceStatuses.Closed || invoice.ClosedAt.HasValue)
{
    throw new BillingInvoiceValidationException(
        "Tagihan susulan ditolak karena invoice telah ditutup. Pembukaan kembali memerlukan persetujuan Supervisor Kasir.");
}
```

### C. `BillingInvoiceEligibilityTests.cs`
Menambahkan 4 unit test berbasis skenario penutupan tagihan:
1. `L_Encounter_Dengan_Invoice_CLOSED_TIDAK_Muncul_Di_EncounterOptions`: Memverifikasi encounter dengan invoice `CLOSED` tidak dipulangkan dalam opsi kunjungan.
2. `M_Encounter_Dengan_Invoice_SETTLED_BY_WRITE_OFF_TIDAK_Muncul_Di_EncounterOptions`: Memverifikasi invoice pemutihan tidak dipulangkan.
3. `N_Encounter_Dengan_Invoice_FINAL_TIDAK_Muncul_Di_EncounterOptions`: Memverifikasi invoice `FINAL` tidak dipulangkan.
4. `O_Encounter_Dengan_Invoice_OPEN_Muncul_Di_EncounterOptions_Dengan_HasInvoice_True`: Memverifikasi invoice `OPEN` tetap muncul dengan bendera `HasInvoice = true`.
