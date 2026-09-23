# Kontrak Integrasi — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-INTEGRATION-1.0` |
| Status | `draft` |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | — / — |
| Input revision | `00-interview-decisions.md` revisi 1 (`FIN-DEC-001`..`023`) |
| Kontrak eksternal yang diikuti | `ACC-XMOD-0.2` milik Accounting — diratifikasi sisi Finance lewat `FIN-DEC-001` |
| Dampak kompatibilitas | `BilArHandoff` bertambah dua kolom nullable — **aditif**, konsumen lama tidak rusak |

Finance punya **dua batas integrasi**: menerima dari Billing dan Medical Fee, lalu menerbitkan
ke Accounting. Tidak ada arah ketiga.

---

## 1. Prinsip yang mengikat seluruh integrasi

| Prinsip | Ketentuan | Dasar |
|---|---|---|
| Arah | Satu arah masuk, satu arah keluar. Finance **MUST NOT** menulis ke tabel Billing, Medical Fee, maupun Accounting | `FIN-OOS-001`..`004` |
| Sumber nilai | Finance menyalin nilai dari sumber, **MUST NOT** menghitung ulang | Aturan bisnis #1 dan #2 |
| Idempotensi | Setiap fakta masuk punya kunci unik; kiriman ulang tidak melahirkan baris kedua | `FIN-DES-006`, `009`, `010` |
| Pengakuan | Menerima fakta ≠ menjurnal. Jurnal adalah pekerjaan Accounting | Aturan bisnis #4 |
| Data pasien | **MUST NOT** menyeberang ke Accounting | Aturan bisnis #6, `ACC-DEC-056` |

---

## 2. Billing → Finance: kontrak fakta penerimaan (`BilCollectionHandoff`)

Ini kontrak yang diminta Finance kepada owner Billing sesuai `FIN-DEC-005`. **Tabelnya milik
Billing**; Finance hanya menetapkan isi minimumnya (`FIN-DES-023`).

### 2.1 Bentuk yang diminta

| Field | Tipe | Wajib | Kegunaan bagi Finance |
|---|---|:---:|---|
| `TenderId` | `Guid` | Ya | Kunci idempotensi. Satu tender berhasil = satu penerimaan Finance |
| `SettlementId` | `Guid` | Ya | Telusur ke penyelesaian pembayaran Billing |
| `InvoiceId` | `Guid` | Ya | Telusur ke tagihan |
| `PaymentAllocationIds` | `Guid[]` | Ya bila ada alokasi | Membuktikan berapa uang yang benar-benar mengurangi tagihan |
| `PaymentMethodId` | `Guid` | Ya | Membedakan tunai dan non-tunai |
| `PaymentMethodAccountId` | `Guid` | Tidak | Rekening/kanal non-tunai |
| `Amount` | `decimal(18,2)` | Ya | Nominal; **disalin apa adanya** |
| `KwitansiNumber` | `string(50)` | Ya | Bukti yang dipegang pasien |
| `CashierShiftId` | `Guid` | Ya untuk tunai | Dasar rekonsiliasi shift |
| `ProviderReference` | `string(150)` | Ya untuk non-tunai | Telusur ke penyedia pembayaran |
| `ProviderEventId` | `string(100)` | Ya bila ada | Idempotensi sisi penyedia |
| `OccurredAt` | `DateTimeOffset` | Ya | Waktu uang benar-benar diterima |
| `SourceInvoiceStatus` | `string(30)` | Ya | **Penentu tahan atau kirim** — lihat bagian 5 |
| `TenderStatus` | `string(30)` | Ya | `SUCCEEDED` atau `REVERSED` |
| `HandoffKey` | `Guid` | Ya | Kunci idempotensi handoff |
| `CorrelationId` | `Guid` | Ya | Rantai telusur ujung ke ujung |
| `CausationId` | `Guid` | Ya | Tindakan penyebab |
| `Status` | `string(30)` | Ya | `CREATED` / `ACKNOWLEDGED`, mengikuti pola `BilArHandoff` |

### 2.2 Ketentuan perilaku

| Hal | Ketentuan |
|---|---|
| Kapan dibuat | Saat tender berstatus `SUCCEEDED`, **tidak menunggu** tagihan difinalisasi |
| Pembalikan | Tender `REVERSED` membuat **baris handoff baru**, bukan mengubah baris lama |
| ACK | Finance menandai `ACKNOWLEDGED` setelah penerimaan berhasil dibuat |
| Kegagalan | Bila Finance gagal mengolah, baris tetap `CREATED` dan dicoba ulang. Billing **MUST NOT** menghapusnya |
| Billing → Accounting | **Dilarang.** Billing tidak pernah mengirim kejadian ke Accounting langsung |

### 2.3 Riwayat kesepakatan

| Hal | Status |
|---|---|
| Mekanisme transport (tabel handoff atau outbox) | **Disepakati** 21 September 2026 — tabel handoff persisted (`BilCollectionHandoff`), sesuai usulan Finance (`FIN-DEC-005`, `BKC-DEC-106`) |
| Nama tabel dan kolom persisnya | Wewenang Billing — `BilCollectionHandoff` (`BKC-DES-037`), field persis sesuai bagian 2.1 di atas |
| Eksekusi task Billing | **Selesai** 22 September 2026 — `BilConsumerHandoffService.PublishForTenderAsync` (`BE-BKC-069`) menerbitkan baris sesuai kontrak ini, dipasang di `BillingSettlementService.ReconcileTenderAsync`. Konsumsi sisi Finance dibangun `BE-FIN-016` |

---

## 3. Billing → Finance: fakta AR, AP, dan koreksi (sudah ada)

Ketiga tabel ini **sudah ada di source** pada `09101d05` dan tidak perlu dibangun.

| Sumber | Kunci idempotensi | Menjadi apa di Finance | Status kontrak |
|---|---|---|---|
| `BilArHandoff` | `HandoffKey` | `FinReceivable` | Sudah ada; **perlu perluasan** untuk manfaat karyawan |
| `BilApHandoff` | `HandoffKey` | Rujukan kesiapan pada `FinDoctorPayable`; **bukan** sumber nilai | Sudah ada, dipakai apa adanya |
| `BilHandoffAdjustment` | `Id` | `FinReceivableAdjustment` | Sudah ada, dipakai apa adanya |

### 3.1 Perluasan `BilArHandoff` yang diminta

Sesuai `FIN-DEC-006`. Keadaan sekarang diverifikasi langsung dari source: `BillingArDebtorTypes`
hanya berisi `PATIENT_GUARANTOR` dan `PAYER`.

| Perubahan | Bentuk | Sifat |
|---|---|---|
| Nilai `DebtorType` baru | `EMPLOYEE_BENEFIT` | Aditif — konsumen lama tidak rusak |
| Kolom baru | `BenefitOwnerId` (`Guid?`) | Nullable — baris lama tetap sah |
| Kolom baru | `BenefitRelationship` (`string(30)?`) | Nullable — `SELF`, `SPOUSE`, `CHILD`, dan seterusnya |

**Siapa yang mengisi.** Registrasi/Billing, berdasarkan hubungan pasien dengan karyawan dan
eligibilitas yang berlaku saat pelayanan. Finance **MUST NOT** menentukan ulang identitas ini;
ketidaksesuaian dikembalikan lewat alur koreksi (`FIN-DEC-016`).

**Status.** Menunggu konfirmasi owner Billing **dan** owner HR. Sampai keduanya turun, rumpun
ini `OPEN DECISION` dan tidak masuk gelombang pengiriman mana pun.

---

## 4. Medical Fee → Finance: fee dokter yang sudah disetujui

| Hal | Ketentuan | Dasar |
|---|---|---|
| Apa yang diterima | `DoctorServiceFee` yang berstatus **sudah disetujui** | `FIN-DEC-003` Opsi B |
| Kunci idempotensi | `SourceDoctorServiceFeeId` | Satu fee disetujui = satu utang |
| Apa yang **tidak** diterima | Fee yang masih dihitung, diverifikasi, atau belum disetujui | `FIN-VAL-043` |
| Peran `BilApHandoff` | Rujukan kesiapan saja — **bukan** sumber nilai | `FIN-DEC-003` |
| Perhitungan ulang | **Dilarang.** Finance menyalin nilai fee | Aturan bisnis #1 |

**Mengapa ini penting.** Kalau Finance membentuk utang dokter dari `BilApHandoff.Amount`,
liabilitas dokter akan diakui sebelum fee-nya diverifikasi. Dan bila aturan posting Accounting
juga memuat komponen `JASA_MEDIS` pada kejadian piutang, jasa medis akan terbukukan **dua
kali**. `FIN-DEC-003` Opsi B menutup keduanya: kejadian piutang hanya memuat piutang dan
pendapatan, sedangkan jasa medis terbit terpisah lewat `PENGAKUAN-HUTANG-DOKTER` setelah fee
disetujui.

**Konsekuensi untuk Accounting:** contoh aturan posting yang memuat `JASA_MEDIS` di dalam
`PENGAKUAN-PIUTANG` **MUST** disesuaikan agar tidak berbenturan.

---

## 5. Finance → Accounting: kejadian keuangan

Mengikuti `ACC-XMOD-0.2` apa adanya (`FIN-DEC-001`). Yang ditulis di sini adalah **kewajiban
sisi Finance**, bukan penulisan ulang kontrak Accounting.

### 5.1 Tujuan kirim

| Hal | Nilai |
|---|---|
| Pintu masuk | `POST api/v1/corporate/accounting/accounting-events`, satu kejadian per permintaan |
| Autentikasi | Service account internal lewat mekanisme auth existing (`FIN-DEC-007`) — **keputusan final bersama Accounting + Platform**, belum turun |
| Mata uang | `IDR` saja |
| Status saat ini | **Endpoint belum dibangun.** Diverifikasi langsung ke source `09101d05` |

### 5.2 Dua belas field wajib

| Field | Sumber di Finance |
|---|---|
| `EventNumber` | Dibuat `FinAccountingEventOutbox` |
| `EventTypeCode` | Salah satu dari 17 kode pada bagian 5.4 |
| `SourceModule` | Selalu `Finance` |
| `SourceTransactionId` | Nomor piutang, penerimaan, utang, pembayaran, atau mutasi kas kecil |
| `SourceVersion` | Dinaikkan saat koreksi, **tidak pernah dipakai ulang** |
| `EventOccurredAt` | Waktu kejadian bisnis sebenarnya |
| `AccountingDate` | Tanggal pembukuan menurut aturan Finance |
| `Amount` | Nilai yang diakui Finance |
| `CurrencyCode` | `IDR` |
| `LegalEntityId` | Rujukan badan hukum |
| `CorrelationId` | Diwarisi dari rantai Billing → Finance |
| `CausationId` | Tindakan penyebab |

Satu field opsional: `Components` — daftar `{ ComponentCode, Amount }`. Tanpa itu, seluruh nilai
dianggap komponen `TOTAL`.

### 5.3 Kewajiban Finance atas balasan

| Kode | Arti | Yang dilakukan Finance |
|---|---|---|
| `201` | Kejadian baru diterima | Tandai `ACKNOWLEDGED`, simpan nomor jurnal |
| `200` | Sudah pernah diterima | Perlakukan sebagai berhasil. **MUST NOT** membuat kejadian baru |
| `400` | Ada isian tidak sah | Tandai `FAILED`. Perbaiki data sumber, lalu kirim ulang **baris yang sama** |
| `403` | Akun layanan tidak berhak | Tandai `FAILED`, beri tahu operasional. **MUST NOT** dicoba ulang membabi buta |
| `409` | Mata uang bukan rupiah | Tandai `FAILED`. Kiriman ulang pasti ditolak lagi |
| `422` | Aturan posting belum ada | Tandai `HELD`. **MUST NOT** mengirim kejadian baru — Accounting yang melengkapi aturannya |

### 5.4 Katalog 17 jenis kejadian

Disetujui sisi Finance lewat `FIN-DEC-002`, **menunggu ratifikasi Accounting**.

| Kode | Dipicu oleh |
|---|---|
| `PENGAKUAN-PIUTANG` | Piutang diakui dari fakta AR Billing |
| `PENERIMAAN-PIUTANG` | Uang diterima untuk piutang yang **sudah** ada |
| `PENYESUAIAN-PIUTANG` | Koreksi piutang disetujui |
| `PEMUTIHAN-PIUTANG` | Penghapusan piutang disetujui |
| `PENGAKUAN-HUTANG-SUPPLIER` | Utang supplier diinput |
| `PEMBAYARAN-HUTANG-SUPPLIER` | Pembayaran supplier ditandai sudah dibayar |
| `PENGAKUAN-HUTANG-DOKTER` | Fee dokter yang sudah disetujui menjadi utang |
| `PEMBAYARAN-HUTANG-DOKTER` | Pembayaran dokter ditandai sudah dibayar |
| `PENYESUAIAN-HUTANG` | Koreksi utang disetujui |
| `SETORAN-BANK` | Setoran bank diposting |
| `PETTY-CASH-TOP-UP` | Penambahan saldo kas kecil |
| `PETTY-CASH-DISBURSEMENT` | Pencairan kas kecil |
| `PETTY-CASH-RETURN` | Pengembalian sisa kas kecil |
| `PETTY-CASH-REVERSAL` | Pembalikan pencairan kas kecil |
| `PETTY-CASH-ADJUSTMENT` | Koreksi saldo kas kecil |
| `PENERIMAAN-KASIR` | Penerimaan dari tender Billing **sebelum/tanpa** piutang |
| `PEMBALIKAN-PENERIMAAN-KASIR` | Pembalikan penerimaan kasir |

**Beda `PENERIMAAN-KASIR` dan `PENERIMAAN-PIUTANG`.** Keduanya sama-sama uang masuk, tetapi
lawan jurnalnya berbeda. `PENERIMAAN-KASIR` terbit saat pasien membayar di kasir dan belum ada
piutang Finance sama sekali. `PENERIMAAN-PIUTANG` terbit saat penjamin melunasi piutang yang
sudah diakui sebelumnya. Menyamakan keduanya akan membuat piutang berkurang dua kali atau tidak
berkurang sama sekali.

### 5.5 Penahanan sebelum tagihan final

Wujud teknis `FIN-DEC-004`.

| Keadaan | Penerimaan di Finance | Baris kejadian |
|---|---|---|
| Tender berhasil, tagihan masih `OPEN` | Dibuat, terlihat penuh di Finance | `HELD_FOR_FINALIZATION` |
| Tagihan kemudian `FINAL` | Tidak berubah | Dilepas menjadi `PENDING` |
| Tender berhasil, tagihan sudah `FINAL` | Dibuat | Langsung `PENDING` |

Pelepasan dipicu **peristiwa finalisasi**, bukan timer. Uangnya tetap tercatat di Finance sejak
detik pertama; yang ditahan hanya jurnalnya.

### 5.6 Saldo subledger per periode

| Hal | Ketentuan |
|---|---|
| Isi minimum | Badan hukum, periode akuntansi, akun kontrol, saldo subledger, tanggal cut-off |
| Nama field yang diajukan Finance | `LegalEntityId`, `AccountingPeriodCode`, `ControlAccountCode`, `SubledgerBalance`, `AsOfDate` |
| Status | **Draf sisi Finance** (`FIN-DEC-023`), menunggu konfirmasi Accounting (`FIN-OQ-011`) |
| Kapan dikirim | Saat Finance menutup periode, sebelum Accounting menutup bukunya |

### 5.7 Titik mulai pengiriman

Sesuai `FIN-DEC-008`:

| Hal | Ketentuan |
|---|---|
| Transaksi yang dikirim | Hanya yang terjadi **setelah** tanggal go-live integrasi |
| Transaksi sebelum tanggal itu | **Tidak** dikirim, **tidak** direkonstruksi dari histori Finance |
| Saldo sebelum cutover | Diinput Accounting sebagai saldo awal manual, satu kali |
| Tanggal pastinya | Ditentukan saat gelombang `MVP-5` siap, bukan sekarang |

---

## 6. Yang Finance MUST NOT lakukan

| Larangan | Sebabnya |
|---|---|
| Menulis `BilCashierShift.SystemCash` atau `PhysicalCash` | Milik Billing. Finance membaca dan merekonsiliasi saja (aturan bisnis #12) |
| Menghitung ulang nilai tagihan atau fee dokter | Sumbernya otoritatif (aturan bisnis #1) |
| Mengirim data pasien ke Accounting | Larangan privasi (aturan bisnis #6) |
| Membuat kejadian baru saat menerima balasan `422` | Kejadiannya sudah tersimpan di Accounting |
| Memakai `SourceVersion` yang sama untuk koreksi | Koreksinya tidak akan dijurnal |
| Mengirim mata uang selain rupiah | Kontrak Accounting hanya menerima `IDR` |
| Membuka kembali periode akuntansi | Milik Accounting |
| Mengubah kas kecil dari alur kasir, atau sebaliknya | Dua kolam terpisah (aturan bisnis #15) |

---

## 7. Ketergantungan yang belum tertutup

| Ketergantungan | Pemilik | Dampak bila belum turun |
|---|---|---|
| Konfirmasi bentuk `BilCollectionHandoff` | Billing | Penerimaan dari kasir tidak dapat dikonsumsi otomatis |
| Konfirmasi perluasan `BilArHandoff` | Billing + HR | Rumpun manfaat karyawan tidak dapat dimulai |
| Endpoint penerima Accounting Event | Accounting | Worker pengiriman tetap dimatikan; outbox tetap terisi |
| Ratifikasi katalog 17 kejadian | Accounting | Kejadian berjenis belum terdaftar akan tertahan di sisi Accounting |
| Mekanisme autentikasi service account | Accounting + Platform | Pengiriman tidak dapat diaktifkan |
| Nama field saldo subledger | Accounting | Rumpun tutup periode tertunda ke `POST-MVP` |
