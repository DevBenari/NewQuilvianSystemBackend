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
