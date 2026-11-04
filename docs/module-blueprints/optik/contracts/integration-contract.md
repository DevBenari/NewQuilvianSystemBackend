# Optik — Kontrak Integrasi

| Field | Nilai |
| --- | --- |
| `contract_version` | `OPT-INT-v1` — status `approved` (sisi Optik) |
| Owner | Yoga Aji Pratama (Optik); pemilik modul mitra untuk sisi masing-masing |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-03 |
| `input_revision` | `03-domain-architecture.md` r1 bagian J; `02-backend-architecture.md` r1 bagian 9 |

Tidak ada integrasi dengan pihak luar rumah sakit di MVP. Semua integrasi di bawah adalah
antar-modul di dalam backend yang sama.

## 1. Ringkasan

| ID | Arah | Mitra | Mekanisme | Persetujuan mitra |
| --- | --- | --- | --- | --- |
| `INT-OPT-01` | Optik → Registrasi | `EncounterIntakeService` | Panggilan service, sinkron | `OPT-APR-010` |
| `INT-OPT-02` | Billing menutup kunjungan | `BillingFinalizationService` | Finalisasi tagihan + kebijakan sumber | `OPT-APR-012` |
| `INT-OPT-03` | HR → Optik | `WfpCredentialLicense` (baca) | Query baca | Tidak perlu (baca saja) |
| `INT-OPT-04` | Optik → Billing | `ClinicalMilestoneFactProducer` | Fakta tagihan sesudah commit | `OPT-APR-001`, `011` |
| `INT-OPT-05` | Optik → Billing | `ClinicalMilestoneFactProducer.EmitClinicalCancellationAsync` | Pembatalan baris | `OPT-APR-001` |
| `INT-OPT-06` | Billing → Optik | Baca bagian pasien dan terbayar | Query baca | `OPT-APR-002`, `003` |
| `INT-OPT-07` | Optik → Farmasi | `DrugStockService` | Panggilan service di dalam transaksi Optik | `OPT-APR-005` |
| `INT-OPT-08` | Medical Fee ← Optik | `MedicalFeeServiceSourceAdapter` menarik pemeriksaan final | Tarik oleh Medical Fee | `OPT-APR-009` |
| `INT-OPT-09` | Rekam Medis ← Optik | `IMedicalRecordDocumentSource` | Baca sesuai permintaan | `OPT-APR-007` |
| `INT-OPT-10` | Rawat Jalan → Optik | `ReferralSourceEncounterId` | Isian saat pendaftaran | `OPT-APR-008` |
| `INT-OPT-11` | Optik → Billing (diskon) | `POST invoices/{id}/discounts` | Endpoint yang ada | `OPT-APR-004` |

## 2. Rincian

### `INT-OPT-01` Pendaftaran kunjungan

| Butir | Isi |
| --- | --- |
| Produsen → konsumen | `OptVisitService` → `EncounterIntakeService` |
| Isi | `EncounterType = Outpatient`, `ServiceUnitId` = unit `Optical`, pasien, penjamin/cara bayar, `ReferralSourceEncounterId?`, `ReferralDoctorId?`, kunci idempoten pendaftaran (`RegistrationIdempotencyKey`) |
| Idempotensi | Kunci `OPT-VISIT:{Idempotency-Key}`; permintaan ulang mengembalikan kunjungan yang sama |
| Gagal | Kunjungan tidak terbentuk; Optik tidak menyimpan apa pun |
| Perluasan Registrasi | Terima dan validasi `ReferralSourceEncounterId` (kunjungan pasien yang sama) |

### `INT-OPT-02` Penutupan kunjungan

Fakta source: Billing memfinalisasi tagihan otomatis begitu bagian pasien lunas
(`BillingSettlementService.cs:906-915`), dan finalisasi menutup kunjungan
(`BillingFinalizationService.cs:894`). Finalisasi ditahan bila ada item yang belum selesai
(`OrdersIncomplete`, `BillingChargeSourceAdapter.IsOrderComplete`).

| Butir | Isi |
| --- | --- |
| Perluasan Billing | Kebijakan sumber `OPTICAL`: status `ORDERED`, `IN_WORK`, `READY` dianggap **belum selesai**; `HANDED_OVER`, `NOT_COLLECTED`, `COMPLETED` (pemeriksaan) dianggap selesai. Void normal hanya dari `ORDERED`. Pembatalan sesudah pengerjaan berupa void baris frame/lensa belum dipotong yang disetujui Kepala Optik; caranya (void atau adjustment kredit) ditetapkan Billing |
| Pemicu dari Optik | Setelah pesanan terakhir di kunjungan menjadi terminal, `OptBillingGateway` meminta Billing mencoba finalisasi tagihan yang sudah lunas (`TryCompleteSettledInvoiceAsync`) |
| Akibat | Kunjungan pemesanan tetap terbuka sampai pesanan terakhir selesai (`OPT-DEC-043`). Kunjungan pemeriksaan saja tertutup saat bagian pasien lunas, seperti kunjungan penunjang lain |
| Bila belum lunas | Tagihan menunggu kasir (piutang pasien untuk `NotCollected`, `OPT-DEC-049`) |
| Idempotensi | Finalisasi Billing sudah idempoten (`EncounterStatus == Completed` → selesai) |

### `INT-OPT-04` Fakta tagihan

| Butir | Pemeriksaan | Barang |
| --- | --- | --- |
| `SourceContext` | `Optical` | `Optical` |
| `EffectType` | `OpticalExamCharge` | `OpticalItemCharge` |
| `SourceAggregateId` | `OptRefractionExam.Id` | `OptOrder.Id` |
| `SourceItemId` | `MstProcedure.Id` tindakan "Pemeriksaan Refraksi" | `OptOrderItem.Id` |
| `Quantity` / `Unit` | `1` / `Pemeriksaan` | `Quantity` item / satuan barang |
| Status sumber | `COMPLETED` (sekali) | `ORDERED` → `IN_WORK` → `READY` → `HANDED_OVER` / `NOT_COLLECTED` (revisi fakta yang sama) |
| Harga | Billing membaca tarif tindakan | Billing membaca tarif `DrugId` (`FindDrugTariffAsync`) |
| `RuleSnapshot` | `{ milestone: "ExamFinalized", examNumber }` | `{ milestone, orderNumber, lineNo, itemCategory, lensSupply }` |
| Tidak diterbitkan | Revisi pemeriksaan | Pesanan remake |
| Waktu | Sesudah commit Optik | Sesudah commit Optik |
| Gagal | Fakta `Pending` dikirim ulang lewat mekanisme producer; pemeriksaan tetap final; `BillingFactEmittedAt` diisi saat diterima | Pesanan tetap `Draft` sampai fakta diterima; petugas mencoba lagi |

### `INT-OPT-06` Status bayar

| Butir | Isi |
| --- | --- |
| Yang dibaca | Per pesanan: total bagian pasien (sesudah tanggungan penjamin dan diskon), jumlah terbayar teralokasi, sisa |
| Sebelum alokasi per baris tersedia (`OPT-APR-003`) | Terbayar dibaca per tagihan kunjungan; aturan sementara `OPT-DEC-035` memastikan hanya satu pesanan berpengerjaan yang belum lunas per kunjungan, sehingga angkanya tidak tertukar |
| Gagal | *Fail-closed*: kirim ke pengerjaan dan penyerahan ditolak `503` |

### `INT-OPT-07` Stok

| Butir | Isi |
| --- | --- |
| Potong | `IssueAsync` dengan `DrugId`, depo Optik, `Quantity`, `SourceDocumentType = "OpticalOrder"`, `SourceDocumentId = OptOrder.Id`, `IdempotencyKey = OPT:{orderId}:{itemId}:issue` |
| Kembali | Penerimaan kembali/koreksi dengan kunci `OPT:{orderId}:{itemId}:return`; jenis mutasi ditetapkan Farmasi (`OPT-CAP-17`) |
| Kapan | Berpengerjaan: frame dan lensa stok saat `InFabrication`. Penjualan langsung: saat `HandedOver`. Remake: saat `InFabrication`. Lensa custom: tidak lewat stok |
| Gagal | Stok kurang → seluruh transisi Optik dibatalkan (satu transaksi) |

### `INT-OPT-08` Jasa medis

Medical Fee menarik pemeriksaan `Final` (pemeriksa, waktu, nomor) dan mencocokkannya dengan baris
tagihan `OpticalExamCharge`. Apakah RO (bukan dokter) berhak jasa medis, dan berapa nilainya,
adalah wewenang Medical Fee (`OPT-APR-009`).

### `INT-OPT-09` Rekam Medis

`OptMedicalRecordDocumentSource.SourceModule = "Optical"`. Jenis dokumen: pemeriksaan refraksi
(revisi terakhir) dan resep (versi terakhir, termasuk resep luar bertanda). Tidak memuat harga,
diskon, maupun pembayaran.

### `INT-OPT-11` Diskon

Pengajuan lewat endpoint Billing yang ada. Billing perlu menyediakan persetujuan menurut
`ApproverRole` kebijakan "Diskon Optik", karena endpoint persetujuan yang ada khusus diskon
dokter. Sampai tersedia, diskon di atas batas tidak dapat disetujui dari layar Optik.

## 3. Rekonsiliasi

| Kejadian | Siapa yang membereskan |
| --- | --- |
| Fakta tagihan `Pending` | Mekanisme kirim ulang producer, dipantau Billing |
| Tarif tidak ditemukan | Antrean rekonsiliasi Billing; Optik menampilkan kode `OPT-VAL-045` |
| Mutasi stok gagal | Tidak terjadi setengah jadi (satu transaksi) |
| Kunjungan tidak tertutup walau pesanan terminal | Laporan pesanan terbuka (`OPT-DEC-025`) memperlihatkannya; kasir menyelesaikan tagihan |
