# Billing dan Kasir — State Transition Matrix

`contract_version: BIL-STATE-0.4` · status **approved** · approved 20 Agustus 2026 · owner Billing/Finance/Cashier.

## Invoice

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| Tidak ada | charge pertama | `OPEN` | Producer/Billing service | source tuple valid | Tolak/duplicate replay aman |
| `OPEN` | progress allocation ranap | `OPEN` | Kasir | dana sukses tersedia | Tolak nilai berlebih |
| `OPEN` | finalisasi | `FINAL` | Billing | semua order complete; kalkulasi current; patient responsibility settled atau exception sah | `422`, tampilkan checklist |
| `FINAL` | AR/AP posting sukses | `CLOSED` | Sistem | handoff idempotent tercatat | Tetap FINAL dan retry |
| `OPEN` | full write-off | `SETTLED_BY_WRITE_OFF` | Finance | case approved | Tidak boleh menjadi PAID |
| `FINAL/CLOSED` | edit/delete item | tidak sah | siapa pun | — | Tolak; gunakan adjustment |

## Tender dan settlement

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `CREATED` | submit | `PENDING` | Kasir | shift aktif untuk cash | Tolak |
| `PENDING` | provider/cash confirm | `SUCCEEDED` | Sistem/Kasir | reference valid | Replay hasil sama |
| `PENDING` | gagal definitif | `FAILED` | Sistem | response final | Outstanding tetap |
| `PENDING` | timeout | `PENDING` | Sistem | hasil belum diketahui | Jangan retry otomatis |
| `SUCCEEDED` | reversal sah | `REVERSED` | Finance/System | entry kompensasi | Tolak mutasi langsung |
| `SUCCEEDED/FAILED` | ubah status manual | tidak sah | siapa pun | — | Tolak |

Settlement: `DRAFT → IN_PROGRESS → PARTIALLY_SETTLED → SETTLED`; `FAILED` hanya bila tidak ada tender berhasil dan seluruh attempt final gagal. Tender sukses tidak hilang ketika tender lain gagal.

## Refund/write-off/adjustment

`DRAFT → SUBMITTED → APPROVED → POSTED`; approver dapat `REJECTED`; execution provider refund dapat `PARTIALLY_EXECUTED` sebelum `EXECUTED`. Dari `POSTED/EXECUTED`, reversal menghasilkan case/entry baru, bukan status mundur. Maker=approver atau amount di atas saldo selalu tidak sah.

## Shift

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| Tidak ada | open | `OPEN` | Kasir | tidak punya shift aktif | `409` |
| `OPEN` | handover | `HANDED_OVER` | Dua kasir | konfirmasi kedua pihak | Tetap OPEN |
| `OPEN` | close, variance nol | `CLOSED` | Kasir | fisik diisi | Tolak bila kosong |
| `OPEN` | close, variance ada | `CLOSED_WITH_VARIANCE` | Kasir | variance tersimpan | Wajib review, bukan hilangkan variance |
| `CLOSED_WITH_VARIANCE` | review | `REVIEWED` | Kepala Kasir | reason/resolution | Tolak |
| `CLOSED/REVIEWED` | reopen | `REOPENED` | Otoritas policy | reason + audit | `403/422` |
| Closed state | ubah saldo lama | tidak sah | siapa pun | — | Entry koreksi baru |

Exception death/emergency transfer/DAMA mengizinkan administrative departure dan AR debtor sah tanpa mengubah settlement menjadi paid. Tests: `BIL-AT-003`,`005`,`007`,`014`,`016`,`018`,`020`.

Security/privacy: setiap command transisi diperiksa permission backend dan actor; audit menyimpan reason/nominal/status tetapi tidak menyimpan identitas pasien atau payload provider pada custom log. Trace keputusan `BKC-DEC-031`–`044`.

## Amendment 2 September 2026

Tidak ada status baru pada `BilInvoice`/`BilInvoiceItem`. `POST catalog-charges` (`BKC-DEC-059`–`062`, approved) memicu transisi "Tidak ada → `OPEN`"/"`OPEN` → `OPEN`" yang SAMA seperti `POST from-source` existing pada tabel Invoice di atas — hanya sumber datanya (katalog vs free-form) yang berbeda, bukan lifecycle status invoice-nya.

## Amendment 3 September 2026 — Dokumen Invoice Asuransi

`contract_version: BIL-STATE-0.5` · status **draft** · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009`.

**Tidak ada status baru, dan tidak ada transisi baru.** `GET {id}/insurance-invoice-document` adalah endpoint baca murni: ia tidak mengubah `BilInvoice.Status`, tidak membuat `BilCalculationVersion` baru, dan tidak menyentuh `BilInvoiceItem.Status`. Mencetak dokumen tidak pernah menjadi peristiwa yang mengubah keadaan tagihan.

Yang perlu dicatat justru **ketergantungan** dokumen pada status yang sudah ada, karena sumber angkanya berbeda per status:

| Status invoice | Sumber angka dokumen | Alasan |
| --- | --- | --- |
| `OPEN` | Kalkulasi pratinjau segar (`PreviewCalculationAsync`) | Tagihan berjalan masih berubah; angka yang ditampilkan harus sama dengan yang dilihat kasir di Menu Pembayaran |
| `FINAL` | Versi kalkulasi tersimpan (`BilCalculationVersion` dengan `VersionNo == CurrentCalculationVersion`) | `PreviewCalculationAsync` menolak invoice non-`OPEN` ("Hanya invoice OPEN yang dapat dihitung ulang."), dan angka final memang harus dari versi yang terkunci |
| `CLOSED` | Sama seperti `FINAL` | Sama |
| `SETTLED_BY_WRITE_OFF` | Sama seperti `FINAL` | Sama. Tanggungan penjamin yang sudah lahir tidak dihapus oleh write-off porsi pasien |

**Transisi yang tidak sah dan tetap tidak sah:** mencetak dokumen **MUST NOT** memindahkan invoice `OPEN` ke `FINAL`, **MUST NOT** menandai klaim sebagai diajukan, dan **MUST NOT** membuat AR penjamin. Ketiga hal itu tetap milik jalur finalisasi (`BKC-DEC-024`) dan tidak boleh dipicu dari lembar cetak.

Trace `BKC-DEC-065`–`069`. Tests `BIL-AT-029`–`035`.
