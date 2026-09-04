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

---

## Amendment 4 September 2026 — Anomali data penjamin dan gerbang PPN

`last_changed_in: BIL-STATE-0.6` · status **draft** · owner Billing/Finance/Cashier · `approved_by`/`approved_at`: belum ada. Input: `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`.

### Status invoice — tidak ada status baru

Amendment ini **tidak** menambah, menghapus, maupun mengubah satu pun status invoice, tender, settlement, shift, atau pengecualian finansial. Perubahannya seluruhnya berada di dalam perhitungan yang terjadi selama invoice berstatus `OPEN`.

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `OPEN` | hitung ulang dengan anomali data terdeteksi | `OPEN` | Kasir/Sistem | Tidak ada syarat tambahan — anomali **tidak** menghalangi perhitungan | Tidak berlaku; perhitungan tetap berhasil |
| `OPEN` | hitung ulang, `PrimaryStatus` `REJECTED` tanpa anomali tercatat | `OPEN` (tidak berpindah) | Sistem | `dataAnomalyAmount > 0` | `422` `BIL-VAL-036`; versi kalkulasi baru **tidak** dibuat |
| `OPEN` | finalisasi saat masih ada anomali data | `FINAL` | Billing | **Tidak ada syarat baru** — anomali tidak menghalangi finalisasi | Tidak berlaku. Lihat `BKC-OQ-086`: apakah seharusnya menghalangi masih pertanyaan terbuka |

### Status penjamin per komponen — kosakata yang berubah

Ini bukan status yang dipersist; ia status turunan yang dibaca layar per baris item.

| Status lama | Status baru | Kapan muncul | Dasar |
| --- | --- | --- | --- |
| `penjamin` | `penjamin` | Ada rupiah yang benar-benar ditanggung penjamin untuk baris itu | Tidak berubah |
| `tunai` | `tunai` | Tidak ada rupiah yang ditanggung penjamin, dan tidak ada anomali | Tidak berubah |
| `belum_terverifikasi` | **dihapus** | — | `BKC-DEC-071` mencabut satu-satunya sebab normalnya (menunggu approval/limit bulanan) |
| — | `anomali_data` (**baru**) | Ada rupiah pada baris itu yang tidak dapat dinilai penjaminnya karena data pendaftaran bermasalah | `BKC-DEC-073`, `BKC-DES-010` |

Urutan pemeriksaannya mengikat: **anomali diperiksa lebih dulu**, baru penjamin, baru tunai. Bila urutannya dibalik, baris yang bermasalah datanya akan tampil sebagai "Tunai" dan masalah datanya tidak pernah terlihat.

### Transisi yang tidak sah dan tetap tidak sah

- Anomali data **MUST NOT** memindahkan invoice ke status apa pun. Ia adalah keterangan pada perhitungan, bukan kejadian pada tagihan.
- Perhitungan ulang **MUST NOT** menghapus atau mengubah versi kalkulasi yang sudah terkunci. Invoice `FINAL`/`CLOSED` tetap memakai angka lamanya, termasuk PPN rawat inap yang lahir sebelum `BKC-DEC-078` berlaku.
- Hilangnya PPN pada tagihan rawat inap **MUST NOT** diselesaikan dengan menyunting tagihan yang sudah menerima pembayaran. Kelebihan bayar diselesaikan lewat jalur Pengecualian Finansial yang sudah ada (`BKC-DEC-032`–`035`).

Trace `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`. Tests `BIL-AT-036`–`048`.

---

## Amendment lanjutan 4 September 2026 — Residual non-billable dirutekan ke write-off

`last_changed_in: BIL-STATE-0.7` · status **draft** · owner Billing/Finance/Cashier · `approved_by`/`approved_at`: belum ada. Input: **`BKC-DEC-080`** beserta `BKC-DEC-036`; keputusan arsitektur `BKC-DES-021`–`025`.

### Tidak ada status baru — yang bertambah adalah kategori

Amendment ini **tidak** menambah satu pun status pada invoice, tender, settlement, shift, maupun kasus pengecualian finansial. `BilWriteOffCase` tetap mengenal tiga status saja: `SUBMITTED`, `POSTED`, `REJECTED`. Yang bertambah adalah **kategori** (`PATIENT_AR`, `NON_BILLABLE_RESIDUAL`), dan kategori bukan status — ia tidak pernah berubah sepanjang umur kasusnya (`BKC-DES-024`).

Penambahan status keempat (`DRAFT`/`PENDING`) untuk menampung kasus yang dibuat mesin **ditolak** justru karena pemicunya diputuskan manual (`BKC-DES-023`). Tanpa kasus yang lahir otomatis, tidak ada keadaan yang perlu status keempat.

### Transisi kasus write-off — sama untuk kedua kategori

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | ajukan write-off | `SUBMITTED` | Finance/kasir berwenang (`BillingWriteOff : Create`) | Invoice bukan `CLOSED`/`SETTLED_BY_WRITE_OFF`; tidak ada kasus `SUBMITTED` lain pada invoice itu; nominal dalam plafon kategorinya | `422` `BIL-VAL-040` (kategori residual) atau pesan plafon outstanding yang sudah ada (kategori `PATIENT_AR`) |
| `SUBMITTED` | setujui | `POSTED` | Penyetuju berwenang (`BillingWriteOff : Approve`) | Penyetuju **bukan** pengaju (`BIL-VAL-017`); plafon diperiksa ulang saat posting | `422`; kasus tetap `SUBMITTED` |
| `SUBMITTED` | tolak | `REJECTED` | Penyetuju berwenang | — | — |
| `POSTED` | reversal | tetap `POSTED`, dengan `BilAdjustment` `Debit` sebagai entry koreksi | Pemegang `BillingFinancialException : Reverse` | Belum pernah direversal | `409`; kasus tidak berubah |

**Tidak ada transisi baru pada tabel ini.** Ia ditulis ulang di sini hanya untuk menyatakan bahwa kategori residual mengikutinya **persis**, termasuk maker-checker — bukan memperoleh jalur pendek tersendiri.

### Dampak posting terhadap status invoice — di sinilah kategori membedakan

| Kategori | `IsFullSettlement` | Status invoice sesudah `POSTED` | Outstanding pasien sesudah `POSTED` | Dasar |
| --- | :---: | --- | --- | --- |
| `PATIENT_AR` | `true` | **Pindah** ke `SETTLED_BY_WRITE_OFF` | Menjadi `0` | `BKC-DEC-036`, tidak berubah |
| `PATIENT_AR` | `false` | Tidak berpindah | **Berkurang** sebesar nominal write-off | `BKC-DEC-036`, tidak berubah |
| `NON_BILLABLE_RESIDUAL` | `true` | — | — | **Tidak sah**; ditolak `BIL-VAL-041` sebelum kasus lahir |
| `NON_BILLABLE_RESIDUAL` | `false` | **Tidak berpindah** | **Tidak berubah sama sekali** | **`BKC-DEC-080`**, `BKC-DES-024` |

Baris terakhir itu adalah inti amendment ini, dan paling mudah salah diterapkan. Outstanding pasien diturunkan dari porsi pasien pada versi kalkulasi terkini; residual non-billable **tidak pernah** masuk porsi pasien. Bila write-off residual ikut mengurangi outstanding, rumah sakit kehilangan nominal yang sama dua kali untuk satu peristiwa: sekali karena selisihnya tidak ditagihkan, sekali lagi karena tagihan pasien ikut dipotong.

### Dampak reversal terhadap status invoice

| Kategori kasus yang direversal | Status invoice sesudah reversal | Outstanding pasien | Sisa residual yang dapat diajukan ulang |
| --- | --- | --- | --- |
| `PATIENT_AR`, `IsFullSettlement = true`, invoice `SETTLED_BY_WRITE_OFF` | **Kembali** ke `OPEN` | Terbuka kembali sebesar nominalnya | — |
| `PATIENT_AR`, `IsFullSettlement = false` | Tidak berpindah | Terbuka kembali sebesar nominalnya | — |
| `NON_BILLABLE_RESIDUAL` | **Tidak berpindah** — statusnya memang tidak pernah dipindahkan | **Tidak berubah** | **Terbuka kembali** sebesar nominalnya |

Entry koreksi reversal tetap berupa `BilAdjustment` ber-`Direction = Debit` yang menunjuk kasus aslinya lewat `ReversesWriteOffCaseId`; histori kasus **MUST NOT** dihapus (`BKC-DEC-036`). Untuk kategori residual, adjustment itu **MUST** dikecualikan dari perhitungan outstanding pasien — bila tidak, reversal akan menaikkan tagihan pasien atas uang yang tidak pernah ada di sana.

### Transisi yang tidak sah dan tetap tidak sah

- Mesin kalkulasi **MUST NOT** membuat, mengubah, atau memposting satu pun `BilWriteOffCase`. Menghitung ulang tagihan bukan peristiwa keuangan (`BKC-DES-023`).
- Write-off berkategori `NON_BILLABLE_RESIDUAL` **MUST NOT** memindahkan invoice ke `SETTLED_BY_WRITE_OFF`, dan **MUST NOT** menandai tagihan sebagai lunas.
- Kategori sebuah kasus **MUST NOT** diubah setelah kasus dibuat. Koreksinya adalah reversal lalu pengajuan ulang, bukan penyuntingan.
- Munculnya residual non-billable **MUST NOT** menghalangi pembayaran pasien maupun penutupan shift kasir. Ia bukan urusan kasir.
- Finalisasi invoice **tidak** diblokir oleh residual yang belum ditulis-off pada rilis ini — hanya diperingatkan. Bila kelak diputuskan memblokir, satu baris transisi baru wajib ditambahkan di sini. Lihat `BKC-OQ-094`.

Trace **`BKC-DEC-080`**, `BKC-DEC-036`, `BKC-DES-021`–`025`. Tests `BIL-AT-055`–`061`.
