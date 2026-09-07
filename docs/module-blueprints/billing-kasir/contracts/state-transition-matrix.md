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

`contract_version: BIL-STATE-0.5` · status **approved** · approved_by Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · approved_at 4 September 2026 · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009` (approved).

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

`last_changed_in: BIL-STATE-0.6` · status **approved** · owner Billing/Finance/Cashier · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 4 September 2026. Input: `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`.

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

`last_changed_in: BIL-STATE-0.7` · status **approved** · owner Billing/Finance/Cashier · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 4 September 2026. Input: **`BKC-DEC-080`** beserta `BKC-DEC-036`; keputusan arsitektur `BKC-DES-021`–`025`.

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

---

## Amendment 7 September 2026 — Rumpun baru: Petty Cash (Voucher Kas Kecil)

`last_changed_in: BIL-STATE-0.8` · status **draft** · owner Kepala Kasir/Finance Operations · `approved_by`: — · `approved_at`: — · input: **`PC-DEC-001`–`PC-DEC-013`** (`approved` 7 September 2026); keputusan arsitektur `PC-DES-001`–`PC-DES-014`.

### Kosakata status — lima nilai, dikunci pemilik

`PC-DEC-013` mengunci kosakata status voucher secara utuh dan menyebutnya lengkap. Tabel di bawah memasangkan label yang dikunci itu dengan kode yang benar-benar disimpan di database (`PC-DES-003`).

| Kode persisted | Label terkunci | Kapan muncul | Terminal? |
| --- | --- | --- | :---: |
| `WAITING_APPROVAL` | **`Menunggu Persetujuan`** | Sejak voucher dibuat sampai diputuskan | Tidak |
| `APPROVED` | **`Disetujui`** | Kepala Kasir menyetujui; uang **belum** diserahkan | Tidak |
| `CASH_RECEIVED` | **`Uang Diterima`** | Kasir menekan "Uang Diberikan"; uang sudah keluar | Tidak |
| `COMPLETED` | **`Selesai`** | Bukti nota sudah dimasukkan | **Ya** |
| `REJECTED` | **`Ditolak`** | Kepala Kasir menolak | **Ya, dan immutable** |

Label di atas **MUST** dipakai apa adanya di layar. Layar **MUST NOT** menerjemahkan ulang, menyingkat, atau menambah status keenam.

**Pembatalan bukan status.** `PC-DEC-007` mengizinkan pemohon membatalkan pengajuannya, tetapi `PC-DEC-013` sudah mengunci kosakata pada lima nilai. Keduanya dipenuhi bersamaan dengan menyimpan pembatalan sebagai **penandaan** `IsCancel` warisan `IdentityModel`, bukan sebagai nilai `Status` (`PC-DES-007`). Voucher yang dibatalkan tetap ber-`Status = WAITING_APPROVAL` di database, dan layar menampilkannya sebagai penanda "Dibatalkan" berdasarkan `isCancelled`.

### Transisi yang sah

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| Tidak ada | Buat voucher | `Menunggu Persetujuan` | Kasir/petugas administrasi (`PettyCashVoucher : Create`) | Nama penerima, kategori aktif, nominal lebih besar dari nol, dan tujuan terisi | `400`/`422`; voucher tidak terbentuk dan nomor tidak terpakai |
| `Menunggu Persetujuan` | Setujui | `Disetujui` | Kepala Kasir/Finance Operations (`PettyCashVoucher : Approve`) | Nominal **tidak melebihi** sisa anggaran yang benar-benar bebas, yaitu saldo dikurangi voucher yang sudah disetujui tetapi belum dicairkan (`PC-DEC-008`, `PC-DES-005`); voucher belum dibatalkan | `422` `BIL-VAL-047`; voucher **tetap** `Menunggu Persetujuan` dan dapat disetujui lagi setelah anggaran ditambah |
| `Menunggu Persetujuan` | Tolak | `Ditolak` | Kepala Kasir/Finance Operations (`PettyCashVoucher : Reject`) | Alasan penolakan wajib terisi | `422` `BIL-VAL-049`; voucher tidak berpindah |
| `Menunggu Persetujuan` | Batalkan | `Menunggu Persetujuan` + `IsCancel = true` | **Pemohon voucher itu sendiri** (`PettyCashVoucher : Cancel`) | Pembatal **MUST** orang yang sama dengan `RequestedBy` (`PC-DEC-007`); voucher belum diputuskan | `403` bila bukan pemohonnya; `422` `BIL-VAL-050` bila sudah diputuskan |
| `Disetujui` | Uang Diberikan | `Uang Diterima` | Kasir (`PettyCashVoucher : Disburse`) | Saldo anggaran **saat itu** masih mencukupi (penjaga kedua, `PC-DES-006`); belum pernah ada baris pencairan untuk voucher ini | `422` `BIL-VAL-048`; voucher **tetap** `Disetujui` dan saldo tidak bergerak |
| `Uang Diterima` | Input Nota | `Selesai` | Kasir/petugas administrasi (`PettyCashVoucher : AttachProof`) | Nomor nota/kwitansi terisi | `422` `BIL-VAL-051`; voucher tetap `Uang Diterima` |
| `Selesai` | Koreksi nomor nota | `Selesai` (tidak berpindah) | Kasir/petugas administrasi (`PettyCashVoucher : AttachProof`) | Nomor nota baru terisi | `422`; nomor lama dipertahankan |

### Transisi yang **tidak sah** dan tetap tidak sah

Daftar ini sama pentingnya dengan daftar di atas, dan lebih sering menjadi sumber cacat.

| Dari status | Tindakan | Siapa pun | Kenapa tidak sah | Yang terjadi |
| --- | --- | --- | --- | --- |
| `Ditolak` | Sunting, ajukan ulang, setujui, batalkan, atau apa pun | Siapa pun, termasuk Kepala Kasir | `PC-DEC-003` menjadikan voucher ditolak sebagai catatan audit permanen | **Tidak ada endpoint yang menerimanya** (`PC-DES-013`). Pemohon membuat voucher baru bernomor baru |
| `Disetujui` | Batalkan | Pemohon maupun penyetuju | `PC-DEC-007` membatasi pembatalan hanya selagi belum diputuskan | `422` `BIL-VAL-050` |
| `Uang Diterima` | Batalkan, tolak, atau kembalikan ke `Disetujui` | Siapa pun | Uangnya sudah keluar. Mengembalikan status berarti mengaku uang itu tidak pernah keluar | `422`. Koreksi memakai penyesuaian anggaran beralasan, bukan mundurnya status |
| `Selesai` | Kembalikan ke `Uang Diterima` | Siapa pun | Bukti yang sudah masuk tidak dapat "belum masuk" | `422`. Salah nomor nota dikoreksi lewat aksi koreksi yang tetap `Selesai` (`PC-DES-012`) |
| `Menunggu Persetujuan` | Uang Diberikan | Kasir | Uang tidak boleh keluar sebelum disetujui | `422` `BIL-VAL-046` |
| `Menunggu Persetujuan` atau `Disetujui` | Input Nota | Siapa pun | Nota hanya ada setelah uang benar-benar keluar | `422` `BIL-VAL-051` |
| Status apa pun | Menyunting `RecipientName`, `CategoryId`, `Amount`, atau `Purpose` | Siapa pun | Tidak ada endpoint penyuntingan sama sekali | Tidak ada jalurnya. Voucher yang salah dibatalkan lalu dibuat ulang |
| Status apa pun | Menghapus voucher | Siapa pun | Voucher mencatat uang yang benar-benar keluar | Tidak ada endpoint `DELETE` |

### Di mana saldo anggaran bergerak — satu titik saja

Ini bagian yang paling mudah salah diterapkan, jadi ditulis eksplisit.

| Transisi | `CurrentBalance` | `ReservedAmount` (komitmen) | Baris ledger yang lahir |
| --- | --- | --- | --- |
| Buat voucher | **Tidak bergerak** | Tidak bergerak | Tidak ada |
| Setujui | **Tidak bergerak** (`PC-DEC-009`) | **Bertambah** sebesar nominal voucher | Tidak ada |
| Tolak | Tidak bergerak | Tidak bergerak | Tidak ada |
| Batalkan | Tidak bergerak | Tidak bergerak | Tidak ada |
| **Uang Diberikan** | **Berkurang** sebesar nominal voucher | **Berkurang** sebesar nominal voucher | **Satu** baris `DISBURSEMENT` |
| Input Nota | **Tidak bergerak** (`PC-DEC-009`) | Tidak bergerak | Tidak ada |
| Koreksi nomor nota | Tidak bergerak | Tidak bergerak | Tidak ada |

> **Kenapa komitmen ada, padahal saldo baru berkurang saat pencairan.** `PC-DEC-008` meminta persetujuan dicegah bila akan membuat saldo negatif, sedangkan `PC-DEC-009` menetapkan saldo baru berkurang saat pencairan. Di antara kedua titik itu ada jeda, dan tanpa komitmen jeda itu dapat diisi persetujuan lain.
>
> **Contoh berangka.** Saldo Rp 5.000.000. Voucher A Rp 300.000 disetujui pukul 09.00 — saldo tetap Rp 5.000.000, komitmen Rp 300.000. Pukul 09.05 voucher B Rp 4.800.000 hendak disetujui. Tanpa komitmen, penjaga membandingkannya dengan Rp 5.000.000 dan **meloloskannya**; keduanya lalu dicairkan dan saldo menjadi minus Rp 100.000. Dengan komitmen, penjaga membandingkannya dengan Rp 4.700.000 dan **menolaknya** — inilah yang `PC-DEC-008` minta.

**Penjaga kedua saat pencairan** (`PC-DES-006`) tetap diperlukan walaupun komitmen sudah ada, karena Finance dapat menurunkan kolam lewat penyesuaian setelah sebuah voucher disetujui. Tanpa pemeriksaan ulang di dalam kunci, saldo masih dapat menjadi negatif.

### Status kolam anggaran

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| Tidak ada | Seed awal | `ACTIVE` | Migration | Belum ada kolam aktif | Unique index parsial menolak kolam aktif kedua |
| `ACTIVE` | Tambah anggaran | `ACTIVE` | Finance (`PettyCashBudget : TopUp`) | Nominal lebih besar dari nol, alasan terisi | `422` |
| `ACTIVE` | Koreksi saldo | `ACTIVE` | Finance (`PettyCashBudget : Adjust`) | Hasilnya **tidak** negatif dan **tidak** di bawah komitmen berjalan | `422` `BIL-VAL-054` |
| `ACTIVE` | Nonaktifkan | `INACTIVE` | Finance | Tidak ada voucher `Disetujui` yang belum dicairkan | `422`. Menonaktifkan kolam yang masih punya janji pembayaran akan menggantung voucher yang sudah disetujui |

### Bukti nota yang tidak pernah masuk — keadaan yang dipilih sengaja

Voucher berstatus `Uang Diterima` yang notanya tidak pernah dimasukkan **tetap** `Uang Diterima` tanpa batas waktu. Tidak ada pekerjaan latar yang memindahkannya, tidak ada tenggat yang memblokir, dan tidak ada pemberitahuan otomatis.

Ini keputusan `PC-DEC-006`, bukan kelalaian desain. Finance memantau manual di luar sistem selama MVP, dan mekanisme pengingat/eskalasi ditandai sebagai kandidat rilis berikutnya. Bila kelak diputuskan memblokir atau meng-eskalasi, satu baris transisi baru **MUST** ditambahkan pada tabel di atas — dan saat itu keputusan bisnisnya sudah harus ada lebih dulu.

### Yang **tidak** berubah pada rumpun lain

Amendment ini **tidak** menambah, menghapus, maupun mengubah satu pun status pada `BilInvoice`, `BilSettlement`, `BilTender`, `BilCashierShift`, `BilWriteOffCase`, `BilRefundCase`, atau `BilAdjustment`. Secara khusus:

- **Pencairan voucher Petty Cash tidak memengaruhi `BilCashierShift` sama sekali** — tidak menambah `SystemCash`, tidak mengurangi `PhysicalCash`, dan tidak menggerakkan `Variance` (`PC-DEC-001`). Menutup shift kasir pada hari yang sama dengan pencairan voucher menghasilkan angka yang persis sama seperti bila voucher itu tidak pernah ada.
- Kasir **tidak** perlu punya shift aktif untuk menyerahkan uang kas kecil. `BIL-VAL-019` ("Buka shift kasir sebelum menerima uang tunai") berlaku untuk **penerimaan** uang dari pasien, bukan untuk pengeluaran kas kecil.

Trace **`PC-DEC-001`–`013`**, `PC-DES-001`–`014`. Tests `BIL-AT-064`–`080`.
