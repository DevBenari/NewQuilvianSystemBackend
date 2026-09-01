# Accounting — Cross-Module Contract (Finance / AR / AP → Accounting)

| Field | Value |
|---|---|
| `contract_version` | `ACC-XMOD-0.1` |
| Klasifikasi artefak | **`CROSS_MODULE_REQUIRED`** |
| Status | `draft` — belum disetujui, belum berlaku |
| Consumer | Accounting (owner: Rizki) |
| Producer | Finance / AR / AP (owner: Yasmin) — **lifecycle internalnya bukan wewenang Accounting** |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `00-interview-decisions.md@3` |
| Traceability | `ACC-DEC-002`, `ACC-DEC-003`, `ACC-DEC-005`, `ACC-DEC-011`, `ACC-DEC-020`, `ACC-DEC-021`, `ACC-DEC-035`, `ACC-DEC-036`; `ACC-XM-001`; `ACC-DEP-003`, `ACC-DEP-004` |
| Implementasi | **Phase 2.** Kontrak ini tidak mengizinkan satu baris pun kode integrasi ditulis sekarang |

## 0. Kenapa berkas ini ada sekarang, padahal implementasinya Phase 2

Finance dikembangkan **paralel** oleh Yasmin, bukan setelah Accounting selesai. Kalau bentuk
batas antar keduanya baru disepakati saat Phase 2 dimulai, Finance sudah terlanjur mengunci
lifecycle AR/AP-nya dan Accounting terpaksa menerima apa pun yang sampai.

Berkas ini karena itu mengunci **bentuk batas**, bukan implementasinya. Ia memberi tahu Finance
apa yang Accounting butuhkan agar dapat membukukan dengan benar, dan menandai dengan jujur mana
yang **bukan** hak Accounting untuk menentukan.

`ACC-DEC-009` tidak berubah: tidak ada posting otomatis pada MVP.

## 1. Batas kewenangan — dibaca lebih dahulu

| Accounting **boleh** menentukan | Accounting **TIDAK boleh** menentukan |
|---|---|
| Kejadian apa yang ia butuhkan agar dapat membukukan | Kapan AR menganggap sesuatu `recognized` |
| Data apa yang wajib ada pada kejadian itu | Status internal AR/AP dan perpindahannya |
| Apa yang terjadi bila data itu kurang atau salah | Kapan Finance menerbitkan kejadian |
| Bahwa pembukuan ganda harus dapat dicegah | Bagaimana Finance menyimpan piutang dan utangnya |

Contoh yang sah: *"Accounting membutuhkan `ReceivableRecognized`."*

Contoh yang **tidak** sah: *"AR harus dianggap recognized ketika invoice difinalisasi."*
Kalimat kedua menentukan lifecycle Finance, dan itu wewenang Yasmin.

Setiap kali kontrak ini menyentuh wilayah kedua, ia menandainya
**`CROSS_MODULE_DECISION_REQUIRED`** dan menyebut siapa pemiliknya.

## 2. Arah aliran yang sudah terkunci oleh kontrak yang disetujui

Ini bukan usulan Accounting. Ini keadaan yang sudah ada di repository.

`BIL-INTEGRATION-0.4` berstatus **`approved`** sejak 20 Agustus 2026 dan mengunci semantik
`BIL-INT-007`, `BIL-INT-008`, dan `BIL-INT-009` sebagai **Billing → AR/AP**, bukan Billing →
Accounting. Kode produksinya sudah berdiri: `BilArHandoff`, `BilApHandoff`, dan
`BilHandoffAdjustment` di `Areas/HealthServices/BillingManagement/Billing/Models/`.

Sehingga bentuk rantainya:

```
Billing  ──BIL-INT-007/008/009 (APPROVED)──▶  Finance (AR/AP)  ──ACC-XMOD (berkas ini)──▶  Accounting
```

Accounting **tidak** berlangganan langsung ke Billing. Kalau ia melakukannya sementara Finance
juga meneruskan kejadian yang sama, satu tagihan Rp 10.000.000 menghasilkan dua jurnal dan
pendapatan tercatat Rp 20.000.000. Itulah risiko yang dijaga `ACC-DEP-003`.

> **`ACC-XM-001` tetap `CROSS_MODULE_DECISION_REQUIRED`.** Diagram di atas adalah pembacaan
> Accounting atas kontrak yang **sudah** disetujui, bukan keputusan yang Accounting ambil
> sendiri. Yang mengesahkannya adalah owner Billing, owner Finance/Yasmin, dan Rizki bersama.
> Sampai itu terjadi, `ACC-XM-001` **terbuka**.

## 3. Envelope kejadian — bentuk minimum

Sebelas field berikut adalah **minimum**. Producer boleh menambah; tidak boleh mengurangi.

| Field | Tipe | Wajib | Pemilik makna | Kegunaan bagi Accounting |
|---|---|:---:|---|---|
| `EventId` | `Guid` | Ya | Producer | Identitas kejadian. Kunci idempotency lapis pertama (`ACC-DEC-035`) |
| `EventType` | `string(60)` | Ya | **Bersama** | Menentukan pemetaan akun mana yang dipakai |
| `SourceDomain` | `string(30)` | Ya | Producer | Modul asal, misalnya `FINANCE_AR`. Bagian dari kunci lapis kedua |
| `SourceTransactionId` | `Guid` | Ya | Producer | Transaksi asal. Bagian dari kunci lapis kedua, dan akar penelusuran balik |
| `SourceVersion` | `int` | Ya | Producer | Versi transaksi asal. Membedakan koreksi dari kiriman ulang |
| `AccountingDate` | `date` | Ya | **Bersama** | Menentukan periode. **Bukan** waktu kirim |
| `Amount` | `decimal(18,2)` | Ya | Producer | Nilai. Presisi wajib sama dengan `NFR-008` |
| `CurrencyCode` | `string(3)` | Ya | Producer | ISO 4217. Wajib **walaupun MVP hanya IDR** — lihat bagian 4 |
| `CorrelationId` | `Guid` | Ya | Producer | Merangkai satu alur bisnis lintas modul |
| `CausationId` | `Guid` | Ya | Producer | Kejadian yang menyebabkan kejadian ini |
| `IdempotencyKey` | `string(100)` | Ya | Producer | Kunci pengulangan aman pada batas transport |

### Yang sudah ada di Billing, dan yang belum

Envelope Billing yang berjalan hari ini sudah memuat `HandoffKey`, `CorrelationId`, `CausationId`,
`Amount`, dan `Status`. Yang **belum ada sama sekali**:

| Field | Keadaan di `BilArHandoff` / `BilApHandoff` | Akibat bila tetap tidak ada |
|---|---|---|
| `CurrencyCode` | **Tidak ada** | Accounting tidak dapat menolak mata uang asing secara sah — ia tidak tahu mata uangnya |
| `AccountingDate` | **Tidak ada**; hanya `CreatedAt` dan `DueDate` | Periode ditentukan dari waktu kirim. Kejadian yang terlambat masuk ke periode yang salah |
| `EventType` | **Tidak ada** | Pemetaan akun tidak dapat ditentukan |
| `SourceVersion` | **Tidak ada** | Koreksi tidak dapat dibedakan dari kiriman ulang |

**`CROSS_MODULE_DECISION_REQUIRED`.** Apakah Finance memperkaya envelope-nya sendiri, atau
mengusulkan `BIL-INTEGRATION` naik versi, adalah keputusan owner Finance bersama owner Billing.
Accounting hanya menyatakan: **tanpa empat field itu, ia tidak dapat membukukan dengan benar.**

## 4. Mata uang — `ACC-DEC-020` dan `ACC-DEC-021`

| Aspek | Ketentuan MVP |
|---|---|
| Base currency | `IDR` |
| Mata uang transaksi yang diterima untuk posting | `IDR` **saja** |
| Keseimbangan debit = kredit | Diukur dalam `IDR` (`ACC-DEC-021`) |
| Kolom `CurrencyCode` pada tabel jurnal MVP | **Tidak ada.** Lihat bagian 6 |

Bila Accounting menerima `CurrencyCode != "IDR"`, maka pada MVP maupun Phase 2 awal:

1. **Jangan** melakukan konversi otomatis.
2. **Jangan** posting ke buku besar.
3. Hasilkan state pemrosesan `RejectedUnsupportedCurrency` yang eksplisit, dapat dilihat, dan
   dapat diambil ulang setelah keputusan multi-currency turun.

Kejadian yang ditolak **tidak hilang dan tidak diam-diam dibuang.** Ia tetap tersimpan di kotak
masuk dengan alasan penolakan yang terbaca.

`DEFERRED` — jangan ditambahkan ke MVP dalam bentuk apa pun: posting multi-currency, kurs,
selisih kurs terealisasi, selisih kurs belum terealisasi, dan revaluasi mata uang asing.

## 5. Yang Accounting jamin kepada producer

| Jaminan | Isi |
|---|---|
| Idempotency | Kejadian dengan `EventId` sama yang datang berkali-kali menghasilkan **tepat satu** jurnal. Pengiriman berikutnya mengembalikan nomor jurnal yang sama |
| Lapis kedua | Gabungan `SourceDomain` + `SourceTransactionId` + `EventType` + `SourceVersion` juga unik, sebagai jaring pengaman bila producer keliru membuat `EventId` baru (`ACC-DEC-035`) |
| Penelusuran balik | Dari baris buku besar mana pun dapat ditelusuri sampai `SourceTransactionId` |
| Tidak menerbitkan balik | Accounting adalah muara. Ia **tidak** menerbitkan kejadian keuangan ke modul lain (`ACC-DEC-002`) |
| Tidak menyentuh tabel Finance | Accounting tidak membaca dan tidak menulis tabel Finance (`ACC-DEC-003`) |

## 6. Semantik penolakan sisi consumer

Empat keadaan yang harus dapat dibedakan producer. Ini requirement sisi consumer, dan Accounting
berwenang penuh menentukannya.

| Keadaan | Perlakuan Accounting | Dapat diambil ulang? |
|---|---|---|
| Mata uang tidak didukung | `RejectedUnsupportedCurrency` | Ya, setelah keputusan multi-currency |
| Pemetaan akun belum ada | `RejectedNoAccountMapping` | Ya, setelah pemetaan dilengkapi |
| Periode akuntansinya sudah tertutup | `RejectedPeriodClosed` | Ya, sesuai kebijakan pembukaan kembali |
| Envelope tidak lengkap atau tidak sah | `RejectedInvalidEnvelope` | Tidak, sampai producer mengirim ulang yang benar |

Nama state di atas **belum final**. Ia bagian dari sembilan pertanyaan `DEFERRED` pada
`ACC-DEC-036` dan dikunci saat Phase 2 dirancang. Yang **sudah** final adalah prinsipnya:
kegagalan bersifat eksplisit, terlihat, dan tidak pernah diam.

## 7. Apa yang belum boleh dibuat sekarang

Tidak boleh ada, sampai `ACC-XM-001` diputuskan dan kedua gerbang skill dilewati:

- entity, tabel, service, endpoint, maupun migration Finance;
- tabel kotak masuk kejadian milik Accounting;
- implementasi posting otomatis;
- langganan Accounting langsung ke Billing.

Gerbang yang dimaksud ada di [integration-contract.md](integration-contract.md) bagian 4:
`requirement-completeness-gate` dan `hospital-domain-architect`, keduanya **wajib** sebelum
Phase 2 dirancang.

## 8. Aturan referensi revisi

Supaya ketidakcocokan kontrak terlihat sebelum menjadi bug, kedua sisi mencatat revisi yang
mereka pakai.

```
Finance Blueprint rev X
        └── depends_on:  ACC-XMOD-<versi> APPROVED

Accounting Blueprint rev Z
        └── provides:    ACC-XMOD-<versi> APPROVED
        └── depends_on:  FIN-XMOD-<versi> APPROVED   (bila Finance menerbitkannya)
```

| Kewajiban | Siapa |
|---|---|
| Membaca `ACC-XMOD` revisi `APPROVED` terakhir sebelum mengunci integrasi AR → Accounting, AP → Accounting, settlement yang berdampak Accounting, atau migration/artefak yang bergantung Accounting | Agent Finance / Yasmin |
| Membaca kontrak cross-module Finance revisi `APPROVED` terakhir sebelum implementasi Phase 2 Finance → Accounting | Agent Accounting / Rizki |

Bila versi yang tercatat pada satu sisi bukan versi `APPROVED` terakhir sisi lain, itu
**contract mismatch** dan pekerjaan integrasi berhenti sampai didamaikan.

## 9. Yang wajib dibaca Finance, dan yang tidak

Agent Finance **tidak perlu** membaca seluruh `docs/module-blueprints/accounting/`. Daftar
artefak yang wajib dibaca beserta klasifikasinya ada di
[../blueprint-manifest.md](../blueprint-manifest.md) bagian *Klasifikasi artefak*.
