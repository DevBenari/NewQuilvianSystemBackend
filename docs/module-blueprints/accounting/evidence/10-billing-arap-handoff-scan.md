# Pemindaian Kemampuan — Handoff AR/AP milik Billing

| Field | Nilai |
|---|---|
| `blueprint_id` | `ACC-BP-001` |
| Kepentingan | Bahan keputusan lintas modul `ACC-XM-001` |
| Sifat | **Read-only** terhadap source. Nol kode diubah, nol database disentuh, nol commit |
| Backend SHA | `02c3219`, branch `rizkiG` |
| Tanggal | 8 September 2026 |
| Dibuat karena | Owner bertanya apakah Accounting benar-benar punya relasi dengan Billing dan Finance, supaya tidak kerja dua kali |
| Status temuan | `CONFIRMED` untuk keberadaan kode; `UNKNOWN` untuk isi database |

## Ringkasan satu paragraf

**Penerbit kejadian keuangan sudah ada dan sudah berjalan di Billing.** `BilArHandoff`,
`BilApHandoff`, dan `BilHandoffAdjustment` berdiri lengkap dengan penjaga anti-ganda di tingkat
database, ditulis dalam transaksi yang sama dengan finalisasi faktur. **Yang tidak ada adalah
konsumennya.** Nol kode di seluruh repository yang mengubah status handoff dari `CREATED` menjadi
`ACKNOWLEDGED`. Akibatnya `ACC-DEC-044` — yang menempatkan Finance sebagai penerbit ke Accounting —
menaruh satu modul yang belum ada di tengah rantai yang **hulunya sudah jadi**.

---

## 1. Cara memverifikasi ulang temuan ini

Seluruh perintah di bawah read-only dan dapat dijalankan siapa pun di `NewQuilvianSystemBackend`
pada `02c3219`.

```bash
# 1. Entity handoff ada?
find . -name "BilArHandoff.cs" -o -name "BilApHandoff.cs" -o -name "BilHandoffAdjustment.cs"

# 2. Siapa yang membuat handoff?
grep -rn "BilArHandoffs.Add\|BilApHandoffs.Add" --include="*.cs" Areas/

# 3. Siapa yang mengakui handoff? (inilah pemeriksaan kuncinya)
grep -rn "Acknowledged" --include="*.cs" Areas/ | grep -v "AcknowledgedAt { get"

# 4. Penjaga anti-ganda di tingkat database
grep -rn "HasIndex" --include="*ArHandoff*Configuration*.cs" .
```

Perintah nomor 3 menghasilkan **nol kecocokan yang berkaitan** — satu-satunya yang muncul milik
`WfpDisciplinaryAction` di modul HR, tidak berhubungan sama sekali.

---

## 2. Yang sudah ada di Billing

### 2.1 `BilArHandoff` — penyerahan piutang (`BIL-INT-007`)

Lokasi: `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs`

| Kolom | Isi | Berguna bagi Accounting? |
|---|---|---|
| `InvoiceId` | Faktur asal | **Ya** — calon `SourceTransactionId` |
| `DebtorType` | `PATIENT_GUARANTOR` atau `PAYER` | **Ya** — menentukan akun piutang mana |
| `DebtorReferenceId` | `InsuranceProviderId` bila penjamin | Berguna untuk laporan, **bukan** untuk jurnal |
| `Amount` | Nilai piutang | **Ya** — nilai jurnal |
| `DueDate` | Jatuh tempo | Tidak dipakai jurnal; milik Finance |
| `Status` | `CREATED` atau `ACKNOWLEDGED` | **Ya** — inilah slot konsumen yang kosong |
| `HandoffKey`, `CorrelationId`, `CausationId` | Penelusuran kejadian | Berguna untuk audit |
| `AcknowledgedAt` | Waktu diakui konsumen | **Selalu kosong hari ini** |

### 2.2 `BilApHandoff` — penyerahan utang jasa dokter (`BIL-INT-008`)

| Kolom | Isi | Catatan |
|---|---|---|
| `DoctorId` | Dokter penerima | **Pengenal orang** — lihat bagian 6 soal privasi |
| `Amount` | Bagian dokter | Nilai jurnal |
| `ReadinessStatus` | Bawaan `NotReady` | Ada daur hidup kesiapan sebelum layak dibayar |
| `ReadyAt` | Waktu menjadi siap | Menentukan kapan layak dijurnal |

### 2.3 `BilHandoffAdjustment` — koreksi (`BIL-INT-009`)

Ada, dan ditulis `BillingArApHandoffService` baris 197. Menangani penyesuaian debit/kredit atas
handoff yang sudah terkirim.

### 2.4 Kapan handoff dibuat

`BillingArApHandoffService.StageHandoffsForFinalizationAsync`, dipanggil
`BillingFinalizationService` baris 142 — yaitu **saat faktur difinalisasi**.

Komentar pada service itu menyebut dirinya sendiri:

> *"Menambahkan entity handoff ke context yang sama tanpa membuka transaksi sendiri; pemanggil
> (BillingFinalizationService) yang memegang batas transaksi dan SaveChanges."*

**Ini pola transactional outbox yang benar.** Handoff tertulis dalam transaksi yang sama dengan
finalisasi faktur, sehingga mustahil ada faktur final tanpa handoff, atau sebaliknya.

Dua handoff piutang dapat lahir dari satu faktur:

| Kondisi | `DebtorType` | Nilai |
|---|---|---|
| `isDepartureException` dan masih ada sisa tagihan | `PATIENT_GUARANTOR` | `outstandingAtFinalization` |
| `PrimaryAmount + ExcessAmount > 0` | `PAYER` | Jumlah keduanya |

---

## 3. Koreksi atas pernyataan saya sebelumnya

Pada percakapan 8 September saya menyebut *"`HandoffKey` sudah memberi jaminan anti-ganda"*.
**Itu tidak tepat**, dan koreksinya penting karena menyangkut risiko pembukuan ganda.

`HandoffKey` diisi `Guid.NewGuid()` **setiap kali** service dipanggil. Ia unik, tetapi bukan kunci
bisnis — memanggil service dua kali akan menghasilkan dua `HandoffKey` berbeda.

Penjaga yang sebenarnya ada di configuration, tiga unique index:

| Index | Melindungi dari |
|---|---|
| `HandoffKey` unique | Pemakaian ulang nilai acak yang sama secara tidak sengaja |
| `CorrelationId` unique | Sama |
| **`(InvoiceId, DebtorType)` unique** | **Inilah kunci bisnisnya.** Satu faktur paling banyak satu handoff piutang per jenis debtor |

**Kabar baiknya bagi Accounting:** `(InvoiceId, DebtorType)` adalah kunci alami yang stabil dan
memetakan bersih ke kunci kedua `ACC-DEC-035` — `SourceModule` + `SourceTransactionId` +
`EventTypeId` + `SourceVersion`. Bentuk anti-ganda yang dirancang Accounting **cocok** dengan yang
sudah ada di Billing, bukan bertabrakan.

---

## 4. Yang TIDAK ada

| Yang dicari | Hasil |
|---|---|
| Kode yang mengubah `Status` menjadi `ACKNOWLEDGED` | **Nol** |
| Kode yang mengisi `AcknowledgedAt` | **Nol** |
| Modul Finance / AR / AP | **Nol** — `Areas/Corporate/` hanya `AccountingManagement` dan `HumanResource` |
| Konsumen handoff mana pun | **Nol** |

Artinya setiap `BilArHandoff` yang dibuat hari ini **berhenti selamanya di status `CREATED`**.

### Yang tidak dapat saya verifikasi

| Hal | Kenapa |
|---|---|
| Apakah tabel handoff sudah berisi baris | Menuntut akses database; tidak dilakukan tanpa wewenang terpisah |
| Apakah alur finalisasi faktur sudah dipakai di lingkungan nyata | Menuntut pengamatan runtime |
| Apakah owner Billing memang mengharapkan Finance sebagai konsumen | Menuntut konfirmasi orangnya |

Ketiganya **wajib** ditanyakan ke owner Billing, bukan disimpulkan dari kode.

---

## 5. Dampaknya terhadap `ACC-XM-001`

`ACC-DEC-044` memutuskan **Finance** yang menerbitkan kejadian keuangan ke Accounting. Keputusan
itu diambil 8 September 2026 dengan bukti yang tersedia saat itu: kontrak `BIL-INTEGRATION-0.4`
mengarahkan `BIL-INT-007/008/009` ke AR/AP, yaitu wilayah Finance.

**Bukti baru tidak membatalkan keputusan itu, tetapi mengubah ongkosnya.**

| | Saat `ACC-DEC-044` diambil | Sesudah pemindaian ini |
|---|---|---|
| Penerbit di Billing | Diasumsikan belum ada | **Sudah ada dan berjalan** |
| Konsumen | Diasumsikan Finance akan membangunnya | **Slot kosong, nol kode** |
| Ongkos menunggu Finance | Tampak wajar | **Rantai putus di tengah, hulunya sudah jadi** |

### Tiga pilihan, dengan bukti

| Pilihan | Kapan bisa jalan | Risiko catat ganda | Kesetiaan kontrak |
|---|---|---|---|
| **A. Tetap lewat Finance** (`ACC-DEC-044` sekarang) | Sesudah Finance dibangun | Nol | Setia penuh pada `BIL-INTEGRATION-0.4` |
| **B. Accounting mengonsumsi handoff langsung** | **Sekarang** | Muncul **hanya bila** Finance kelak juga meneruskan handoff yang sama | `BIL-INT-007` menulis tujuannya **AR**, bukan Accounting. Menuntut persetujuan owner Billing |
| **C. Accounting membaca, Finance tetap yang mengakui** | Sekarang | Rendah — hanya satu pihak yang menulis `ACKNOWLEDGED` | Perlu kesepakatan siapa pemilik status `ACKNOWLEDGED` |

**Pilihan C belum pernah dibahas** dan muncul dari pemindaian ini. Ia memisahkan *membaca* dari
*mengakui*: Accounting membuat jurnal dari handoff, sementara pengakuan tetap milik Finance
sebagai penanda bahwa piutangnya sudah dicatat sebagai piutang. Konsekuensinya perlu kejelasan apa
arti `ACKNOWLEDGED` sebenarnya — sudah dijurnal, atau sudah masuk daftar piutang.

---

## 6. Dua hal yang wajib diperhatikan bila pilihan B atau C dipilih

### 6.1 `DoctorId` adalah pengenal orang

`BilApHandoff.DoctorId` menunjuk tenaga kerja. `ACC-DEC-056` melarang Accounting menyimpan
pengenal pasien, dan semangat yang sama berlaku untuk pegawai: `02-backend-architecture.md`
bagian 11 menyatakan MVP nol kolom pasien **maupun pegawai**.

Bila Accounting mengonsumsi `BilApHandoff` langsung, ia **tidak boleh** menyimpan `DoctorId`.
Yang boleh disimpan hanya nomor handoff sebagai penunjuk. Nilai utang jasa medis tetap dijurnal
sebagai satu total per akun, bukan per dokter — rincian per dokter milik Finance.

### 6.2 `ReadinessStatus` menentukan kapan boleh dijurnal

`BilApHandoff` lahir `NotReady` dan punya `ReadyAt`. Menjurnalnya saat masih `NotReady` berarti
mengakui utang yang belum tentu jadi. Aturan kapan sebuah handoff AP layak dijurnal **belum ada
keputusannya** dan bukan wewenang Accounting sendiri.

---

## 7. Yang TIDAK terpengaruh temuan ini

Tiga gelombang yang sedang direncanakan **nol tersentuh**:

| Gelombang | Menyentuh Billing atau Finance? |
|---|---|
| `P2-3` jurnal berulang | Tidak |
| `P2-4` tutup bulan | Tidak |
| `P2-5` tutup tahun | Tidak |

`BE-ACC-P2-001` sampai `BE-ACC-P2-010` tetap dapat dikerjakan tanpa menunggu keputusan apa pun
dari temuan ini. Yang terpengaruh hanya `P2-1` dan `P2-2`, yang memang belum direncanakan.

---

## 8. Pertanyaan untuk owner Billing dan owner Finance

Dibawa apa adanya ke rapat lintas modul. Jangan dijawab sepihak oleh owner Accounting.

| # | Pertanyaan | Kenapa penting |
|---:|---|---|
| 1 | Apakah `BilArHandoff` sudah benar-benar berisi baris di lingkungan nyata, atau alur finalisasi belum dipakai? | Menentukan apakah ada data menumpuk atau belum |
| 2 | Siapa yang dimaksud sebagai konsumen saat `BIL-INT-007` dirancang — Finance, atau siapa pun yang siap? | Menentukan apakah pilihan B melanggar niat aslinya |
| 3 | Apa arti `ACKNOWLEDGED` yang disepakati: sudah dicatat sebagai piutang, atau sudah dijurnal? | Menentukan siapa yang berhak mengubahnya |
| 4 | Bila Finance kelak dibangun, apakah ia akan meneruskan handoff yang sama ke Accounting? | **Ini penentu risiko pembukuan ganda.** Bila ya, pilihan B berbahaya. Bila tidak, pilihan B aman |
| 5 | Kapan sebuah `BilApHandoff` dianggap layak dijurnal — saat `CREATED` atau saat `ReadyAt` terisi? | Menentukan pengakuan utang jasa medis |
| 6 | Apakah owner Billing bersedia `BIL-INTEGRATION-0.4` diperjelas konsumennya, tanpa mengubah bentuknya? | PRD §36 aturan 13 melarang mengubah kontrak yang sudah disetujui; memperjelas konsumen mungkin bukan "mengubah" |

**Pertanyaan nomor 4 adalah yang paling menentukan.** Seluruh risiko pembukuan ganda bergantung
padanya, dan hanya owner Finance yang dapat menjawabnya.

---

## 9. Rekomendasi

**Jangan membalik `ACC-DEC-044` sekarang.** Ia diambil dengan bukti yang benar pada waktunya, dan
membaliknya sepihak oleh owner Accounting melanggar sifat lintas modulnya.

Yang disarankan: bawa dokumen ini ke owner Billing dan Yasmin, jawab keenam pertanyaan bagian 8,
lalu putuskan bersama. Bila hasilnya mengubah `ACC-DEC-044`, keputusan barunya dicatat sebagai
`ACC-DEC-059` dengan `ACC-DEC-044` berstatus `superseded` — bukan dihapus.

Sementara itu, kerjakan `P2-3`, `P2-4`, dan `P2-5` yang tidak menunggu siapa pun.


---

# BAGIAN KEDUA — Pemindaian Penuh Peristiwa Keuangan Billing

| Field | Nilai |
|---|---|
| Ditambahkan | 9 September 2026 |
| Sebabnya | Owner bertanya apakah enam pertanyaan bagian 8 sudah cukup. **Ternyata belum.** Bagian pertama hanya memindai handoff AR/AP; bagian ini memindai seluruh peristiwa keuangan Billing |
| Cakupan | 20 entity Billing yang berdampak keuangan, dibaca isinya satu per satu |
| Sifat | **Read-only.** Nol kode diubah, nol database disentuh |

## 10. Kenapa bagian ini ada

Bagian pertama menjawab *"siapa yang menerbitkan kejadian keuangan"* dan berhenti di situ.
Pertanyaan yang tidak saya ajukan pada diri sendiri: **peristiwa mana saja yang punya penerbit.**

Diperiksa 9 September 2026:

| Yang diperiksa | Hasil |
|---|---|
| Jalur keluar Billing ke keuangan | **Hanya tiga**: `BIL-INT-007` piutang, `008` jasa dokter, `009` koreksi keduanya |
| `BIL-INT-011` dan `012` | **Bacaan masuk**, bukan penerbitan keluar |
| Tempat handoff dibuat | **Satu**: `StageHandoffsForFinalizationAsync`, dipanggil `BillingFinalizationService:142` |
| Kapan | **Hanya saat faktur difinalisasi** |
| Handoff untuk kas, deposit, refund, penghapusan | **Nol** |

Artinya Accounting hanya akan menerima **sisi pengakuan pendapatan**. Seluruh pergerakan kas tidak
terlihat.

## 11. Sebelas peristiwa keuangan yang belum punya penerbit

Kolom "Jurnal seharusnya" adalah **usulan berbasis kaidah akuntansi umum**, bukan kebijakan yang
sudah disetujui. Ia perlu diverifikasi pemilik proses akuntansi.

| # | Peristiwa bisnis | Entity sumber | Penanda di data | Jurnal seharusnya | Usulan penerbit |
|---:|---|---|---|---|---|
| 1 | Pasien atau penjamin melunasi faktur | `BilSettlement` + `BilTender` | `Purpose = INVOICE_PAYMENT`, `Status = SETTLED` | **D** Kas/Bank · **K** Piutang | **Billing/Kasir** |
| 2 | Pasien menyetor deposit rawat inap | `BilSettlement` + `BilDepositMovement` | `Purpose = DEPOSIT_TOP_UP`, `MovementType = TOP_UP` | **D** Kas/Bank · **K** Utang Deposit Pasien | **Billing/Kasir** |
| 3 | Deposit dipakai membayar faktur | `BilDepositMovement` | `MovementType = ALLOCATION` | **D** Utang Deposit Pasien · **K** Piutang | **Billing/Kasir** |
| 4 | Sisa deposit dikembalikan saat pulang | `BilDepositMovement` | `MovementType = RELEASE` | **D** Utang Deposit Pasien · **K** Kas | **Billing/Kasir** |
| 5 | Pembayaran melebihi tagihan | `BilRefundableCredit` | `SourceType = ALLOCATION_EXCESS` atau `SETTLEMENT`, `Status = AVAILABLE` | **D** Piutang · **K** Utang Kelebihan Bayar | **Billing/Kasir** |
| 6 | Kelebihan bayar dikembalikan | `BilRefundCase` + `BilRefundLine` | `Status = EXECUTED` | **D** Utang Kelebihan Bayar · **K** Kas | **Billing/Kasir** |
| 7 | Piutang dihapuskan | `BilWriteOffCase` | `Status = POSTED`, `Category = PATIENT_AR` atau `NON_BILLABLE_RESIDUAL` | **D** Beban Piutang Tak Tertagih · **K** Piutang | **Perlu diputuskan** — datanya di Billing, saldonya milik Finance |
| 8 | Penyesuaian faktur sesudah final | `BilAdjustment` | `Status = POSTED`, `Direction = DEBIT` atau `CREDIT` | Mengikuti arah: `CREDIT` mengurangi piutang, `DEBIT` menambah | **Billing** |
| 9 | Selisih kas fisik saat tutup shift | `BilCashierShift` + `BilCashVarianceReview` | `Status = CLOSED_WITH_VARIANCE` lalu `REVIEWED`, `Variance != 0` | Kurang: **D** Selisih Kas · **K** Kas. Lebih: **D** Kas · **K** Selisih Kas | **Billing/Kasir** |
| 10 | Pengeluaran kas kecil | `BilPettyCashVoucher` | `Status` terbayar | **D** Beban sesuai `CategoryId` · **K** Kas Kecil | **Billing** |
| 11 | Pengisian ulang kas kecil | `BilPettyCashBudgetMovement` | Pergerakan penambahan | **D** Kas Kecil · **K** Kas | **Billing** |

### Yang paling menentukan dari tabel ini

**Sepuluh dari sebelas peristiwa itu milik Billing/Kasir, bukan Finance.**

`ACC-DEC-044` menetapkan Finance sebagai penerbit tunggal. Tetapi Finance memegang piutang dan
utang — ia **tidak punya visibilitas** atas selisih kas shift kasir, kas kecil, deposit pasien,
maupun kelebihan bayar. Menyalurkan kesebelasnya lewat Finance berarti memperluas lingkup Finance
jauh melampaui AR/AP, dan memaksanya menjadi perantara untuk data yang tidak pernah ia sentuh.

Karena itu `ACC-XM-001` berubah bentuk: bukan lagi *"siapa penerbitnya"*, melainkan
**"penerbitnya berbeda menurut jenis peristiwa"**.

## 12. Akun yang belum ada di daftar akun

Kesebelas peristiwa di atas menuntut akun yang belum pernah disebut rancangan Accounting mana pun.
Daftar akun saat ini baru terisi `1002 Kas Besar` dan `4001 Pendapatan Rawat Jalan`.

| Akun yang dibutuhkan | Kelompok | Dipakai peristiwa |
|---|---|---|
| Kas Kasir | `1` Aset | 1, 2, 4, 6, 9 |
| Bank | `1` Aset | 1, 2 |
| **Kas Kecil** | `1` Aset | 10, 11 |
| **Utang Deposit Pasien** | `2` Liabilitas | 2, 3, 4 |
| **Utang Kelebihan Bayar** | `2` Liabilitas | 5, 6 |
| **Beban Piutang Tak Tertagih** | `5` Beban | 7 |
| **Selisih Kas** | `5` Beban, atau `4` Pendapatan bila lebih | 9 |
| Beban operasional per kategori kas kecil | `5` Beban | 10 |

**Kas Kecil wajib akun terpisah dari Kas Kasir.** Komentar pada `BilPettyCashVoucher` menyatakan
tegas: *"kas kecil dan kas fisik shift kasir adalah dua uang yang berbeda"* (`PC-DEC-001`), dan
entity itu sengaja tidak punya kolom penghubung ke `BilCashierShift` maupun `BilSettlement`.
Menyatukannya di satu akun akan menghapus pemisahan yang sudah dijaga Billing.

## 13. Tiga hal yang justru memudahkan

### 13.1 Penentu akun kas sudah ada sebagai master data

`MstPaymentMethod` memuat `IsCash`, `IsBankTransfer`, `IsCardPayment`, `IsQris`, `IsInsurance`,
`IsCompanyGuarantor`, `IsMembership`, ditambah `PaymentMethodType`.

Itu **persis** dimensi yang menentukan akun kas mana yang didebit: tunai ke Kas Kasir, transfer
dan kartu serta QRIS ke Bank. Accounting tidak perlu mengarang master baru — cukup memetakan
`PaymentMethodId` ke akun.

**Tetapi ini menuntut perubahan rancangan Accounting**, lihat bagian 15.

### 13.2 Infrastruktur penerbitan sudah seragam

Hampir seluruh entity di atas sudah memiliki `IdempotencyKey`, `PayloadHash`, `CorrelationId`,
dan `CausationId`. Menambahkan handoff bagi mereka bukan membangun dari nol, melainkan mengikuti
pola yang sudah dipakai `BilArHandoff`.

### 13.3 Momen "final" sudah jelas

`BilWriteOffCase` dan `BilAdjustment` memiliki kolom bernama **`PostedAt`** beserta alur
`Submitted → Approved → Posted`. Billing sendiri sudah memperlakukannya sebagai peristiwa posting,
sehingga titik penerbitan kejadian tidak perlu diperdebatkan.

## 14. Batas privasi yang wajib dijaga

| Data | Di mana | Ketentuan |
|---|---|---|
| `BilPettyCashVoucher.RecipientName` | Kas kecil | **Ditandai SENSITIF di komentar kodenya**, teks bebas, sengaja tanpa FK. **Tidak boleh** masuk Accounting |
| `BilPettyCashVoucher.Purpose` | Kas kecil | Ditandai SENSITIF. Tidak boleh masuk Accounting |
| `BilApHandoff.DoctorId` | Jasa dokter | Pengenal orang. Tidak boleh disimpan Accounting (`ACC-DEC-056`) |
| `BilDepositAccount.EncounterId` | Deposit | Penunjuk ke kunjungan pasien. Hanya boleh disimpan sebagai nomor transaksi asal, bukan sebagai FK |

Jurnal kas kecil dicatat **per kategori beban**, bukan per penerima. Jurnal jasa medis dicatat
**per akun total**, bukan per dokter. Rincian per orang tetap milik modul asalnya.

## 15. Perubahan yang dituntut di sisi Accounting

Ini bagian yang menjawab pertanyaan *"apakah ada perubahan dari sisi kita"*.

| # | Perubahan | Sifat | Menyentuh gelombang yang sedang dikerjakan? |
|---:|---|---|:---:|
| 1 | Daftar akun bertambah 7 jenis akun baru (bagian 12) | **Pengisian data**, milik pemilik proses akuntansi | **Tidak** |
| 2 | `AccPostingRule` butuh dimensi kedua: cara pembayaran | **Perubahan rancangan** | **Tidak** — hanya `P2-0b` dan `P2-1` |
| 3 | Lingkup `ACC-P2-S1` jauh lebih luas dari "menerima kejadian dari Finance" | **Perubahan lingkup** | **Tidak** |
| 4 | `ACC-DEC-044` perlu ditinjau: penerbit berbeda per jenis peristiwa | **Keputusan lintas modul** | **Tidak** |

### Rincian perubahan nomor 2 — satu-satunya yang menyentuh rancangan

`AccPostingRule` sekarang berkunci `(LegalEntityId, EventTypeId)`. Itu cukup selama satu jenis
kejadian selalu menghasilkan akun yang sama.

**Kas mematahkan andaian itu.** Peristiwa "pelunasan faktur" adalah **satu** jenis kejadian,
tetapi akun debitnya berbeda menurut cara bayarnya:

| Cara bayar | Akun debit |
|---|---|
| Tunai | Kas Kasir |
| Transfer | Bank |
| Kartu atau QRIS | Bank |

Tiga pilihan, beserta konsekuensinya:

| Pilihan | Cara kerja | Konsekuensi |
|---|---|---|
| **A. Tambah dimensi pada aturan posting** | `AccPostingRule` bertambah kolom opsional `DimensionType` dan `DimensionValue`; untuk kas diisi `PAYMENT_METHOD` dan `PaymentMethodId` | Bentuk aturan berubah lagi sesudah `ACC-DEC-058`. Paling luwes dan menampung dimensi lain di kemudian hari |
| **B. Jenis kejadian dibuat lebih rinci** | `PELUNASAN-TUNAI`, `PELUNASAN-TRANSFER`, `PELUNASAN-KARTU` sebagai jenis terpisah | Nol perubahan rancangan Accounting. Tetapi jumlah jenis kejadian membengkak, dan **keputusan akuntansi bocor ke penamaan milik penerbit** |
| **C. Kejadian membawa kode cara bayar sebagai komponen** | Memakai mekanisme komponen `ACC-DEC-058` yang sudah ada | Menyalahgunakan komponen — komponen adalah **nilai**, bukan penggolongan |

**Usulan: pilihan A.** Alasannya, pilihan B memindahkan keputusan "akun kas mana" ke pihak yang
menerbitkan kejadian, padahal pemetaan akun adalah wewenang Accounting (`ACC-DEC-002`). Pilihan C
merusak makna komponen yang baru saja ditetapkan.

**Belum diputuskan.** Dicatat sebagai `DEC-ACC-P2-009`.

## 16. Yang TIDAK berubah

| Hal | Keadaan |
|---|---|
| Gelombang `P2-3` jurnal berulang | **Nol perubahan** |
| Gelombang `P2-4` tutup bulan | **Nol perubahan** |
| Gelombang `P2-5` tutup tahun | **Nol perubahan** |
| `BE-ACC-P2-001` sampai `BE-ACC-P2-010` | **Nol perubahan**, seluruhnya tetap `READY` |
| `ACC-DEC-045` sampai `ACC-DEC-057` | Tetap berlaku |
| Roadmap Phase 2 gelombang mandiri | Tetap `APPROVED`, tidak perlu disusun ulang |

Seluruh temuan bagian kedua ini mendarat pada `P2-0b` dan `P2-1`, yang memang **sengaja belum
direncanakan**.

## 17. Pertanyaan kiriman kedua untuk owner Billing dan Finance

> **Status: SUDAH DIKIRIM 9 September 2026, menunggu jawaban.** Versi ringkas yang berdiri
> sendiri untuk dikirim ke owner Billing ada di
> [`11-pertanyaan-peristiwa-kas-untuk-billing.md`](11-pertanyaan-peristiwa-kas-untuk-billing.md). Keenam pertanyaan di bawah sudah
> diteruskan owner Accounting ke owner Billing. Jawabannya akan dicatat sebagai bagian keempat
> dokumen ini, mengikuti cara bagian 19 mencatat jawaban pertanyaan 1–6.

Enam pertanyaan bagian 8 tetap berlaku. Berikut tambahannya.

| # | Pertanyaan | Kenapa penting |
|---:|---|---|
| 7 | Dari sebelas peristiwa bagian 11, mana yang akan **Billing terbitkan sendiri**, mana lewat Finance, dan mana yang memang sengaja **tidak** dijurnal otomatis? | Menentukan bentuk `ACC-XM-001` yang sebenarnya |
| 8 | Apakah Billing bersedia menambah handoff untuk peristiwa kas, atau lebih memilih Accounting membaca tabelnya? | Menentukan siapa yang membangun apa |
| 9 | Peristiwa kas dijurnal per transaksi, atau **diringkas per shift kasir**? | Menentukan volume jurnal. Per transaksi bisa ribuan baris per hari |
| 10 | Selisih kas kasir dijurnal saat `CLOSED_WITH_VARIANCE`, atau menunggu `REVIEWED`? | Menentukan kapan selisih diakui sebagai beban |
| 11 | Penghapusan piutang diterbitkan Billing (yang punya alur persetujuannya) atau Finance (yang punya saldonya)? | Satu-satunya peristiwa yang pemiliknya benar-benar ambigu |
| 12 | Kas kecil memang bagian dari pembukuan yang sama, atau dikelola terpisah di luar buku besar? | Bila terpisah, peristiwa 10 dan 11 gugur |

**Pertanyaan nomor 9 paling berdampak teknis.** Menjurnal setiap transaksi kas satu per satu pada
rumah sakit bervolume tinggi akan menghasilkan puluhan ribu baris jurnal per bulan. Meringkasnya
per shift kasir menghasilkan beberapa baris per hari, dengan penelusuran tetap terjaga lewat
nomor shift. Ini keputusan bisnis, bukan teknis — tetapi konsekuensinya besar bagi kecepatan
laporan.

## 18. Yang tetap tidak dapat saya verifikasi

| Hal | Kenapa |
|---|---|
| Apakah tabel-tabel itu sudah berisi data nyata | Menuntut akses database |
| Apakah Billing sudah punya rencana menerbitkan peristiwa kas | Menuntut konfirmasi ownernya |
| Apakah rumah sakit memang menghendaki kas kecil masuk buku besar yang sama | Keputusan pemilik proses akuntansi |
| Nama dan kode akun yang akan dipakai | Milik pemilik proses akuntansi |


---

# BAGIAN KETIGA — Jawaban Owner Billing

| Field | Nilai |
|---|---|
| Diterima | 9 September 2026 |
| Menjawab | Enam pertanyaan bagian 8 |
| **Tidak** menjawab | Enam pertanyaan bagian 17 (sebelas peristiwa kas) — **masih terbuka** |

## 19. Jawaban apa adanya

| # | Pertanyaan | Jawaban owner Billing |
|---:|---|---|
| 1 | Apakah `BilArHandoff` sudah berisi baris? | Tabelnya **sudah dibuat, tetapi belum ada isinya sama sekali** |
| 2 | Siapa konsumen yang dimaksud? | **AR / Finance** |
| 3 | Apa arti `ACKNOWLEDGED`? | **Durable consumer acceptance**: Finance sudah berhasil mencatat AR/AP. **Bukan** sekadar mengetahui, dan **bukan** sudah dijurnal |
| 4 | Bila Finance dibangun, apakah ia meneruskan handoff yang sama? | **Tidak.** Finance menghasilkan **financial event terpisah** yang tetap membawa correlation/causation ke source Billing |
| 5 | Kapan `BilApHandoff` layak dijurnal? | `CREATED` **bukan** readiness gate. Hanya `READY` + `ReadyAt` yang menunjukkan payable eligible; `ACKNOWLEDGED` berarti Finance sudah mengonsumsinya |
| 6 | Bersedia `BIL-INTEGRATION-0.4` diperjelas? | Arah dan bentuk dasarnya **tidak perlu diubah**. Yang perlu amendment: definisi ACK, ownership transition, AP readiness transition, dan **contract Finance → Accounting** |

## 20. Apa yang diselesaikan jawaban ini

### 20.1 `ACC-DEC-044` terkonfirmasi, bukan dibatalkan

Jawaban 2 dan 4 menegaskan rantainya: **Billing → Finance → Accounting**, dan Finance menerbitkan
kejadian **tersendiri**, bukan meneruskan handoff.

`ACC-DEC-044` yang menetapkan Finance sebagai penerbit ke Accounting **tepat**, dan tidak perlu
diubah.

### 20.2 Pilihan B dan C mati, dan alasannya kini kuat

Bagian 5 menyajikan tiga pilihan. Dua di antaranya kini **tertutup oleh bukti**, bukan oleh
pendapat:

| Pilihan | Keadaan |
|---|---|
| A. Tetap lewat Finance | **DIPILIH** — dikonfirmasi jawaban 2 dan 4 |
| B. Accounting mengonsumsi `BilArHandoff` langsung | **MATI.** Jawaban 4: Finance menerbitkan kejadian terpisah. Bila Accounting **juga** membaca handoff, satu tagihan menghasilkan dua jurnal — persis bahaya yang dijaga sejak awal |
| C. Accounting membaca, Finance yang mengakui | **MATI.** Jawaban 3: `ACKNOWLEDGED` berarti *Finance sudah mencatat AR/AP*, sama sekali bukan urusan penjurnalan. Memisahkan "membaca" dari "mengakui" tidak punya makna pada definisi itu |

### 20.3 Kekhawatiran "handoff menumpuk selamanya" gugur

Bagian pertama menyimpulkan handoff akan menumpuk tanpa konsumen. Jawaban 1 mengoreksinya:
**tabelnya masih kosong**. Tidak ada data yang menumpuk, karena alurnya memang belum pernah
berjalan.

Rantainya bukan putus di tengah — ia **belum dimulai**, dan itu keadaan yang berbeda.

### 20.4 Kesiapan AP terjawab

Jawaban 5 menutup pertanyaan kapan utang jasa dokter layak diakui: **`READY` + `ReadyAt`**, bukan
`CREATED`. Ini terutama mengikat Finance, karena Accounting menjurnal dari kejadian Finance,
bukan langsung dari `BilApHandoff`.

## 21. Yang BELUM selesai, dan ini yang penting

### 21.1 Sebelas peristiwa kas masih tanpa penerbit

Keenam jawaban hanya menyentuh jalur **AR/AP**. Tabel bagian 11 — kas masuk, deposit pasien,
kelebihan bayar, pengembalian dana, penghapusan piutang, selisih kas kasir, kas kecil — **belum
tersentuh sama sekali**.

Jawaban 6 menyebut yang perlu diperjelas adalah *"contract Finance → Accounting"*. Itu tidak
menjawab siapa yang menerbitkan peristiwa kas, karena sepuluh dari sebelasnya bahkan tidak pernah
sampai ke Finance — tidak ada handoff-nya.

**Pertanyaan bagian 17 nomor 7 sampai 12 tetap terbuka.**

### 21.2 Kontrak pesan Accounting kekurangan dua bidang

Ini temuan baru dari jawaban 4, dan menyentuh kontrak yang sudah disetujui.

Jawaban 4 menyatakan kejadian Finance **"tetap membawa correlation/causation ke source Billing"**.
Sepuluh bidang wajib `ACC-DEC-048` **tidak memuat keduanya**:

`EventNumber`, `EventTypeCode`, `SourceModule`, `SourceTransactionId`, `SourceVersion`,
`EventOccurredAt`, `AccountingDate`, `Amount`, `CurrencyCode`, `LegalEntityId`.

Akibatnya bila dibiarkan: `SourceTransactionId` akan berisi nomor pencatatan AR milik **Finance**,
bukan nomor faktur Billing. Penelusuran dari jurnal kembali ke tagihan pasien **terputus di
Finance**, padahal Finance sudah bersedia membawakan penghubungnya.

Itu tepat jenis kerugian yang paling mahal disadari belakangan: jurnalnya benar, angkanya benar,
tetapi pertanyaan *"jurnal ini berasal dari tagihan siapa"* tidak terjawab tanpa membuka dua modul.

**Usulan:** `ACC-DEC-048` bertambah dua bidang wajib — `CorrelationId` dan `CausationId` —
sehingga menjadi dua belas. Keduanya disimpan pada kotak masuk kejadian dan diteruskan ke jurnal
sebagai penelusuran.

**`DEC-ACC-P2-010` DITUTUP 9 September 2026** — owner memilih usulan ini, menjadi `ACC-DEC-060`.
Kedua bidang menjadi wajib, sehingga pesan kejadian kini **dua belas bidang**. `ACC-API` naik ke
`0.8`, `ACC-VALIDATION` ke `0.6`.

### 21.3 Ratifikasi sisi Finance belum ada

Jawaban ini datang dari owner Billing. Ia menyelesaikan sisi Billing dan menegaskan bentuk
rantainya, tetapi **bentuk pesan Finance → Accounting** — kini **dua belas bidang** lewat
`ACC-DEC-048` dan `ACC-DEC-060` — masih menuntut kesepakatan owner Finance.

Jawaban 6 justru menyebutnya sendiri sebagai hal yang perlu di-amendment.

## 22. Keadaan `ACC-XM-001` sesudah jawaban ini

| Sisi | Keadaan |
|---|---|
| Arah rantai Billing → Finance → Accounting | **DISEPAKATI** |
| Accounting tidak berlangganan ke Billing | **DISEPAKATI**, dan alasannya kini berbukti |
| Arti `ACKNOWLEDGED` | **DISEPAKATI** — milik Finance, bukan Accounting |
| Kesiapan AP | **DISEPAKATI** — `READY` + `ReadyAt` |
| Bentuk pesan Finance → Accounting | **BELUM** — menunggu owner Finance |
| Penerbit sebelas peristiwa kas | **BELUM** — pertanyaan 7–12 belum dijawab |
