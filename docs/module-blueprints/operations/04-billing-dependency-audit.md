# Dependency Operasi → Farmasi → Billing

| Field | Value |
|---|---|
| Blueprint ID | `OPS-BP-001` |
| Tanggal | 4 September 2026 |
| Status Billing | **Dependency / future integration** — bukan scope inti Modul Operasi (`OPS-DEC-037`) |
| Metode | Pembacaan kode; setiap klaim menyebut berkas yang membuktikannya |

## Kedudukan dokumen ini

Dokumen ini semula ditulis sebagai audit temuan yang harus diperbaiki. Setelah `OPS-DEC-037`,
kedudukannya berubah: **bagian Billing di bawah adalah catatan dependency, bukan daftar
pekerjaan Modul Operasi.**

Tanggung jawab Modul Operasi terbatas pada empat hal:

1. mencatat episode operasi;
2. mencatat tindakan;
3. mencatat material/BHP yang digunakan;
4. menghasilkan data yang dapat dipakai modul lain.

Keempatnya sudah terpenuhi. Billing dibahas saat modul Billing/Finance dikerjakan.

## Operasi → Farmasi: selesai

| Tahap | Bukti |
|---|---|
| Pemakaian dicatat | `OperatingRoomMaterialService.RecordAsync` menulis `OprMaterialUsage` dan outbox dalam satu transaksi |
| Outbox dibukukan | `OperatingRoomInventoryDispatchService.DispatchCaseAsync` |
| Stok berkurang | `DrugStockService.IssueBatchAsync`, di depo hasil pemetaan `MstOperatingRoomStockSource` |
| Retur diajukan | Consumer memanggil `DrugReturnService.CreateAsync`, bukan menambah stok sendiri |
| Stok kembali | `DrugReturnService.VerifyAsync`, hanya sebesar yang diterima apoteker |

Terbukti oleh `OperatingRoomToPharmacyEndToEndTests`, seluruhnya lewat service asli:
100 vial → dipakai 10 → diretur 4 → apoteker menerima 3 → saldo akhir 93. Kartu stok dua baris,
batch sama di kedua sisi, satu vial yang ditolak tidak masuk saldo mana pun.

Inilah pemenuhan tanggung jawab nomor 4: data pemakaian operasi sudah dapat dipakai modul lain,
dan modul pertama yang memakainya — Farmasi — sudah tersambung penuh.

---

# Catatan dependency Billing

Bagian di bawah **tidak dikerjakan sekarang**. Ia direkam supaya keadaannya diketahui saat
modul Billing/Finance dibuka, dan supaya tidak ditemukan ulang dari nol.

## Jalur penagihan yang sudah bekerja di sistem

```
Modul klinis
    → ClinicalMilestoneFactProducer          (menulis TrxClinicalMilestoneFact)
    → BillingFolioService.RecognizeMilestoneAsync
    → BilProcessingEffect / BilChargeLine
    → DispatchStatus = Dispatched
```

Sinkron, dalam proses, idempoten berdasarkan `MilestoneFactId` + `MilestoneFactVersion`, dan
mencatat hasilnya kembali ke fakta (`Dispatched`, `Rejected`, `OutcomeUnknown`,
`SuppressedNoPriorCharge`).

Sudah dipakai oleh Clinical Management (`PatientProcedureController`), Laboratory
(`LabSpecimenService`), dan Farmasi (`PrescriptionController`, `PrescriptionWorkflowService`,
`ConsultationFinalizationService`). Endpoint intake-nya:
`POST .../billing-management/folios/internal/milestones/recognize`.

## Catatan yang perlu dibawa ke pembahasan Billing

### DEP-BIL-001 — Ada dua mekanisme penyerahan yang berjalan paralel

`OperatingRoomIntegrationService` menulis baris `OprIntegrationDelivery` bertujuan `Billing`.
Satu-satunya pembacanya adalah controller Operasi sendiri, untuk rekonsiliasi dan retry manual.

Sementara `ClinicalMilestoneFactProducer` sudah melakukan pekerjaan sejenis untuk empat modul
lain — termasuk untuk tindakan pasien, yang bentuknya paling dekat dengan tindakan operasi.

Selama outbox Billing Operasi tidak punya penerima, biayanya nol. Yang perlu diputuskan saat
modul Billing dibuka: mana yang menjadi kontrak resmi. Rekomendasi teknis dari pembacaan kode
adalah memakai `ClinicalMilestoneFactProducer`, karena sudah teruji dipakai empat modul dan
sudah memiliki idempotency serta pelacakan hasil yang setara — tetapi itu keputusan pemilik
modul Billing, bukan keputusan Modul Operasi.

### DEP-BIL-002 — Operasi baru men-stage satu jenis chargeable event

`OperatingRoomExecutionService` memanggil `StageChargeDeliveryAsync(entity.Id, "procedure", ...)`
saat catatan operasi difinalkan. Jasa dokter, jasa anestesi, material, dan BHP belum di-stage.

Datanya sudah lengkap dan tidak perlu dikumpulkan ulang: material sudah memiliki jumlah, batch,
satuan stok, dan depo karena pembukuan persediaan menuntutnya; tim sudah memiliki peran dan
kewenangan pada `OprTeamMember`.

### DEP-BIL-003 — Pemakaian obat Farmasi tidak pernah menjadi tagihan

Ini catatan milik **Farmasi**, bukan Operasi, tetapi berada di rantai yang sama sehingga
direkam di sini.

`DrugUsageStatus.Billed` ada di enum dan dijaga `DrugUsageService`, tetapi tidak ada satu baris
kode pun yang menetapkannya; setiap pemakaian obat pasien berhenti selamanya di `NotBilled`.
`TrxDrugUsage` juga tidak memanggil `ClinicalMilestoneFactProducer`, berbeda dari resep yang
memanggilnya — sehingga obat yang diserahkan lewat resep menagih, sedangkan obat yang dicatat
lewat pemakaian tidak.

Transaksi Farmasi lain — transfer, retur, permintaan stok — memang tidak seharusnya menagih.

## Yang Tidak Diaudit

- Perhitungan tarif, diskon, dan penjaminan. Milik Billing (`OPS-DEC-029`).
- Kebenaran `BillingFolioService` sendiri.
- Apakah `SuppressedNoPriorCharge` dan `OutcomeUnknown` ditangani benar oleh modul yang sudah
  memakainya.
