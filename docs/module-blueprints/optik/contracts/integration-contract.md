# Optik — Kontrak Integrasi

| Field | Nilai |
| --- | --- |
| `contract_version` | `OPT-INT-v1.3` — status `approved` (sisi Optik), Yoga Aji Pratama 2026-11-06. Dasarnya: `OPT-INT-v1.2` `approved` (sisi Optik), Yoga Aji Pratama 2026-11-06. Sisi Registrasi disetujui `OPT-APR-013`, sisi Billing disetujui `OPT-APR-014` dan `OPT-APR-015` (`OPT-DEC-084`). Tambahan `v1.3`: perilaku kunjungan tertutup tanpa tagihan (`OPT-OQ-023`) |
| Owner | Yoga Aji Pratama (Optik); pemilik modul mitra untuk sisi masing-masing |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-06 untuk `v1.3`, `v1.2`, dan `v1.1`; `v1` 2026-11-03 |
| `input_revision` | `03-domain-architecture.md` r1 bagian J; `02-backend-architecture.md` r4 bagian 9; `00-interview-decisions.md` r12 |

Tidak ada integrasi dengan pihak luar rumah sakit di MVP. Semua integrasi di bawah adalah
antar-modul di dalam backend yang sama.

## 1. Ringkasan

| ID | Arah | Mitra | Mekanisme | Persetujuan mitra |
| --- | --- | --- | --- | --- |
| `INT-OPT-01` | Optik → Registrasi | `EncounterIntakeService` | Panggilan service, sinkron | `OPT-APR-010`; pengecualian penjamin: `OPT-APR-013` disetujui |
| `INT-OPT-02` | Billing menutup kunjungan | `BillingFinalizationService` | Finalisasi tagihan + kebijakan sumber | `OPT-APR-012` |
| `INT-OPT-03` | HR → Optik | `WfpCredentialLicense` (baca) | Query baca | Tidak perlu (baca saja) |
| `INT-OPT-04` | Optik → Billing | `ClinicalMilestoneFactProducer` | Fakta tagihan sesudah commit; pemeriksaan yang tertinggal dikirim ulang pekerja latar | `OPT-APR-001`, `011`; adjustment tarif terlambat: `OPT-APR-015` disetujui |
| `INT-OPT-05` | Optik → Billing | `ClinicalMilestoneFactProducer.EmitClinicalCancellationAsync` | Pembatalan baris | `OPT-APR-001` |
| `INT-OPT-06` | Billing → Optik | Baca bagian pasien dan terbayar | Query baca | `OPT-APR-002`, `003` |
| `INT-OPT-07` | Optik → Farmasi | `DrugStockService` | Panggilan service di dalam transaksi Optik | `OPT-APR-005` |
| `INT-OPT-08` | Medical Fee ← Optik | `MedicalFeeServiceSourceAdapter` menarik pemeriksaan final | Tarik oleh Medical Fee | `OPT-APR-009` |
| `INT-OPT-09` | Rekam Medis ← Optik | `IMedicalRecordDocumentSource` | Baca sesuai permintaan | `OPT-APR-007` |
| `INT-OPT-10` | Rawat Jalan → Optik | `ReferralSourceEncounterId` | Isian saat pendaftaran | `OPT-APR-008` |
| `INT-OPT-11` | Optik → Billing (diskon) | `POST invoices/{id}/discounts` | Endpoint yang ada | `OPT-APR-014` disetujui; di atas 10%: `OPT-APR-004` (`POST-MVP`) |

## 2. Rincian

### `INT-OPT-01` Pendaftaran kunjungan

| Butir | Isi |
| --- | --- |
| Produsen → konsumen | `OptVisitService` → `EncounterIntakeService` |
| Isi | `EncounterType = Outpatient`, `ServiceUnitId` = unit `Optical`, pasien, penjamin/cara bayar, `ReferralSourceEncounterId?`, `ReferralDoctorId?`, kunci idempoten pendaftaran (`RegistrationIdempotencyKey`) |
| Idempotensi | Kunci `OPT-VISIT:{Idempotency-Key}`; permintaan ulang mengembalikan kunjungan yang sama |
| Gagal | Kunjungan tidak terbentuk; Optik tidak menyimpan apa pun |
| Perluasan Registrasi | Terima dan validasi `ReferralSourceEncounterId` (kunjungan pasien yang sama) |
| Pengecualian Penjamin Perusahaan (`OPT-DEC-078`, `OPT-APR-013`, `BE-OPT-061`) | `EncounterIntakeService.LoadPaymentSourceReferenceAsync` menolak Penjamin Perusahaan selain `EncounterType.MedicalCheckup`, dengan pengecualian sempit: kunjungan `Outpatient` yang unitnya `ServiceUnitType.Optical`. Jenis unit dibaca dari `MstServiceUnit` memakai `request.ServiceUnitId`, karena profil kunjungan penunjang tidak membawa jenis unit. Seluruh pemeriksaan kartu di `LoadCompanyCardAsync` tetap berlaku, termasuk penolakan benefit karyawan internal |
| Contoh penjamin | Kunjungan unit Optik dengan kartu PT Contoh (samaran) tersimpan. Kunjungan Laboratorium dengan kartu yang sama tetap ditolak "Penjamin perusahaan hanya dapat dipilih lewat pendaftaran loket." |
| Akibat ke Billing | Sumber bayar kunjungan menyimpan snapshot penjamin yang sama dengan loket, sehingga Billing membacanya seperti penjamin loket. `OPT-AC-027` wajib lulus dengan penjamin sungguhan sebelum rilis |

### `INT-OPT-02` Penutupan kunjungan

Fakta source: Billing memfinalisasi tagihan otomatis begitu bagian pasien lunas
(`BillingSettlementService.cs:906-915`), dan finalisasi menutup kunjungan
(`BillingFinalizationService.cs:894`). Finalisasi ditahan bila ada item yang belum selesai
(`OrdersIncomplete`, `BillingChargeSourceAdapter.IsOrderComplete`).

| Butir | Isi |
| --- | --- |
| Perluasan Billing | Kebijakan sumber `OPTICAL`: status `ORDERED`, `IN_WORK`, `READY` dianggap **belum selesai**; `HANDED_OVER`, `NOT_COLLECTED`, `COMPLETED` (pemeriksaan) dianggap selesai. Void normal hanya dari `ORDERED`. Pembatalan selama tagihan belum final berupa void baris. Pesanan yang berakhir tanpa diserahkan (batal sesudah pengerjaan, tidak diambil) membawa **semua** baris tagihannya ke `NOT_COLLECTED`; baris yang batal lalu diterbitkan pembatalannya dan Billing membuat adjustment kredit. Alasannya: adjustment tidak mengubah status baris, sehingga baris `IN_WORK`/`READY` akan menahan finalisasi selamanya (laporan `BE-OPT-041`) |
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
| Status sumber | `COMPLETED` (sekali), dibawa `RuleSnapshot.milestone = ExamFinalized` | `ORDERED` → `IN_WORK` → `READY` → `HANDED_OVER` / `NOT_COLLECTED` (revisi fakta yang sama), dibawa `RuleSnapshot.milestone` (laporan `BE-OPT-030`) |
| Harga | Billing membaca tarif tindakan | Billing membaca tarif `DrugId` (`FindDrugTariffAsync`) |
| `RuleSnapshot` | `{ milestone: "ExamFinalized", examNumber }` | `{ milestone, orderNumber, lineNo, itemCategory, lensSupply }` |
| Tidak diterbitkan | Revisi pemeriksaan | Pesanan remake |
| Waktu | Sesudah commit Optik | Sesudah commit Optik |
| Gagal | Fakta yang sudah tercatat dikirim ulang lewat mekanisme producer. Yang belum tercatat dikirim ulang pekerja latar Optik (lihat di bawah). Pemeriksaan tetap final; `BillingFactEmittedAt` diisi saat producer mencatat | Pesanan tetap `Draft` sampai fakta diterima; petugas mencoba lagi |

### Kirim ulang tarif pemeriksaan (`OPT-DEC-082`, `083`, `BE-OPT-062`)

Ada dua lapis pengaman yang berbeda. `ClinicalFactDispatchWorker` milik Clinical Management mengirim ulang fakta yang **sudah tercatat** tetapi hasilnya belum pasti. Pekerja latar Optik menangani yang **belum sempat tercatat sama sekali**, yaitu saat tindakan belum diatur atau basis data putus sebelum producer mencatat. Sebelum amendment ini, kasus kedua hanya dicoba lagi saat pemeriksaan direvisi.

| Butir | Isi |
| --- | --- |
| Pelaksana | `OptExamChargeRetryHostedService`, memakai `OptBillingGateway.EmitExamChargeAsync` yang sama dengan saat final dan revisi |
| Yang dipilih | Pemeriksaan `Final` dengan `BillingFactEmittedAt` kosong, yang difinalkan lebih dari 5 menit lalu |
| Pelaku pada fakta | `FinalizedBy`, yaitu pemeriksa yang memfinalkan. Pekerja latar tidak punya akun sendiri, dan `CreateBy` fakta tetap menunjuk orang yang melakukan tindakan klinisnya |
| Waktu pada fakta | `FinalizedAt`, sama dengan kiriman pertama, sehingga identitas fakta tidak berubah |
| Idempoten | Producer memakai identitas yang sama (Emitted/Replayed), dan penanda diisi lewat pembaruan bersyarat. Dua putaran atau dua proses tidak menghasilkan dua baris tarif |
| Tagihan masih terbuka | Baris tarif masuk tagihan |
| Tagihan sudah final | Menjadi adjustment yang menunggu persetujuan Billing (`BillingClinicalChargeBridgeService.cs#L326-L332`, `RJ-BIL-DEC-004`). Total tagihan final tidak berubah |
| Kunjungan sudah tertutup dan belum punya tagihan | Billing membuat tagihan baru berstatus `OPEN` pada kunjungan itu, dan tarif masuk ke sana. Tarif tidak hilang, tetapi kasir perlu menagihnya. Perilaku Billing ini tidak diubah; apakah Billing menerimanya ditanyakan lewat `OPT-OQ-023`. **Contoh:** kunjungan pemeriksaan Rp 75.000 sudah selesai tanpa tagihan, lalu kasir melihat tagihan baru Rp 75.000 pada kunjungan itu |
| Tindakan belum diatur | Tidak ada fakta dan penanda tetap kosong. Satu peringatan per putaran, lalu dicoba lagi pada putaran berikutnya |

**Syarat agar tarif langsung masuk tagihan.** Billing mencari tarif menurut `OccurredAt`, yaitu waktu pemeriksaan difinalkan, bukan waktu pengiriman ulang. Tarif harus berlaku pada tanggal pemeriksaan itu. Kalau tidak, fakta tetap tercatat, tetapi masuk rekonsiliasi Billing, dan pekerja latar tidak mengulangnya.

**Contoh:** pemeriksaan difinalkan 3 November, sedangkan tindakan dan tarifnya baru diatur 10 November. Bila tarif diisi berlaku sejak 1 November, tarif masuk tagihan pada putaran berikutnya. Bila tarif diisi berlaku sejak 10 November, tarif pemeriksaan 3 November masuk rekonsiliasi Billing.

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
| Kembali | Stok Masuk batch yang sama ke lokasi asal (`ReceiveBatchAsync`), bukan koreksi, dengan kunci `OPT:{orderId}:{itemId}:return` (`OPT-DEC-060`) |
| Kapan | Berpengerjaan: frame dan lensa stok saat `InFabrication`. Penjualan langsung: saat `HandedOver`. Remake: saat `InFabrication`. Lensa custom: tidak lewat stok |
| Gagal | Stok kurang → seluruh transisi Optik dibatalkan (satu transaksi) |

### `INT-OPT-08` Jasa medis

Medical Fee menarik pemeriksaan `Final` (pemeriksa, waktu, nomor) dan mencocokkannya dengan baris
tagihan `OpticalExamCharge`. Apakah RO (bukan dokter) berhak jasa medis, dan berapa nilainya,
adalah wewenang Medical Fee (`OPT-APR-009`).

### `INT-OPT-09` Rekam Medis

`OptMedicalRecordDocumentSource.SourceModule = "Optical"`. Jenis dokumen: pemeriksaan refraksi
(revisi terakhir) dan resep (versi terakhir, termasuk resep luar bertanda). Rekam Medis membaca versi terakhir dari dokumen RM per
versi (`OPT-DEC-050`). Tidak memuat harga, diskon, maupun pembayaran.

### `INT-OPT-11` Diskon

Penerapan lewat endpoint Billing yang ada, dengan aturan pakai sementara MVP (`OPT-DEC-079`):

| Butir | Isi |
| --- | --- |
| Batas nilai | Pemilik Billing hanya membuat kebijakan Optik bernilai tetap paling tinggi 10% (`OPT-APR-014`). Batas ditegakkan oleh nilai kebijakan, bukan oleh Pengaturan Optik |
| Bentuk kebijakan | `DiscountType = PROMO_ITEM`, `TargetComponent = INVOICE_ITEM`, `ValueType = PERCENTAGE`, `Value` ≤ 10. `RequiresApproval = false`, karena Billing hanya mewajibkan persetujuan untuk diskon dokter |
| Penanda | Layar mengenali kebijakan Optik dari kode berawalan `OPT-`, misalnya `OPT-DISC-10` (`OPT-DEC-084`) |
| Penyaringan | Hanya di layar. Backend Billing belum membatasi kebijakan per sumber; risiko ini diterima untuk MVP |
| Izin | Petugas butuh `BillingInvoice : Read` untuk membaca baris tagihan, `DiscountPolicy : Read` untuk daftar kebijakan, dan `BillingDiscount : Create` untuk menerapkan |
| Di atas 10% | Tidak punya jalur di sistem sampai `OPT-APR-004` tersedia |

**Contoh:** Dedi memilih Diskon Optik 10% untuk frame Rp 450.000, dan tagihan frame langsung berkurang Rp 45.000. Promo 50% milik unit
lain tidak tampil. Permintaan 15% dari pelanggan ditolak petugas karena pilihannya tidak ada.

## 3. Rekonsiliasi

| Kejadian | Siapa yang membereskan |
| --- | --- |
| Fakta tagihan `Pending` | Mekanisme kirim ulang producer, dipantau Billing |
| Tarif pemeriksaan belum tercatat | Pekerja latar Optik mengirim ulang; layar pemeriksaan menampilkan peringatan sampai tercatat |
| Tarif pemeriksaan terlambat, tagihan sudah final | Billing memutuskan adjustment-nya (`OPT-APR-015`) |
| Tarif tidak berlaku pada tanggal pemeriksaan | Antrean rekonsiliasi Billing; petugas tarif memperbaiki tanggal berlakunya |
| Tarif tidak ditemukan | Antrean rekonsiliasi Billing; Optik menampilkan kode `OPT-VAL-045` |
| Mutasi stok gagal | Tidak terjadi setengah jadi (satu transaksi) |
| Kunjungan tidak tertutup walau pesanan terminal | Laporan pesanan terbuka (`OPT-DEC-025`) memperlihatkannya; kasir menyelesaikan tagihan |
