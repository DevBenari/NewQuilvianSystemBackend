# Permintaan Kontrak Finance kepada Billing

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Owner modul Billing dan Kasir (`BIL-CASH-001`) |
| Tembusan | Owner HR — hanya untuk bagian 3 |
| Tanggal | 20 September 2026 |
| Sifat | **Dua permintaan perubahan kontrak beserta alasannya.** Belum meminta Billing menulis kode sekarang |
| Dasar | `docs/module-blueprints/finance-management/00-interview-decisions.md`, keputusan `FIN-DEC-005` dan `FIN-DEC-006`, `approved` 20 September 2026 |
| Yang memblokir | Permintaan pada bagian 2 menahan gelombang `MVP-2` Finance |

Berkas ini berdiri sendiri, dapat dibaca tanpa membuka blueprint Finance.

---

## 1. Mengapa Finance menghubungi Billing

Finance sedang membangun buku piutang dan buku penerimaan. Sumber datanya Billing, dan dua dari
tiga jalur sudah tersedia:

| Jalur | Keadaan |
|---|---|
| Fakta piutang (`BilArHandoff`) | **Sudah ada**, dipakai apa adanya |
| Fakta utang dokter (`BilApHandoff`) | **Sudah ada**, dipakai sebagai rujukan kesiapan |
| Koreksi (`BilHandoffAdjustment`) | **Sudah ada**, dipakai apa adanya |
| **Fakta penerimaan uang dari kasir** | **Belum ada** — inilah permintaan utama berkas ini |

Tanpa jalur keempat, Finance tidak punya cara resmi mengetahui bahwa pasien sudah membayar.
Akibatnya bukan sekadar data kurang lengkap: uang yang sudah diterima kasir berisiko dicatat
ulang sebagai piutang, sehingga satu tagihan terhitung dua kali.

Finance **tidak** akan membaca tabel `BilTender` langsung. Itu akan melanggar batas modul dan
membuat Finance pecah setiap kali Billing berubah.

---

## 2. Permintaan pertama — fakta penerimaan dari kasir

### 2.1 Yang diminta

Satu jalur handoff baru, sejenis `BilArHandoff` yang sudah ada, yang memberi tahu Finance bahwa
sebuah tender berhasil. Nama kerjanya `BilCollectionHandoff`; nama sebenarnya terserah Billing.

**Bentuk transport diserahkan ke Billing.** Finance mengusulkan tabel handoff persisted karena
polanya sudah dipahami kedua tim dan lebih mudah diperiksa saat ada masalah. Bila Billing lebih
menghendaki outbox/event, Finance mengikuti — asalkan empat sifat berikut terpenuhi:

1. **Tidak berubah** setelah dibuat.
2. **Aman diulang**, dikunci nomor tender.
3. **Dapat di-ACK** oleh Finance.
4. **Billing tidak mengirim apa pun langsung ke Accounting** — Finance yang menerbitkan
   kejadiannya.

### 2.2 Isi yang Finance butuhkan

| Bidang | Wajib | Kenapa Finance memerlukannya |
|---|:---:|---|
| `TenderId` | Ya | Kunci anti-ganda. Satu tender berhasil = satu penerimaan Finance |
| `SettlementId` | Ya | Telusur ke penyelesaian pembayaran |
| `InvoiceId` | Ya | Telusur ke tagihan |
| `PaymentAllocationIds` | Ya bila ada | Membuktikan berapa uang yang benar-benar mengurangi tagihan |
| `PaymentMethodId` | Ya | Membedakan tunai dan non-tunai |
| `PaymentMethodAccountId` | Tidak | Rekening atau kanal non-tunai |
| `Amount` | Ya | Disalin apa adanya; Finance tidak menghitung ulang |
| `KwitansiNumber` | Ya | Bukti yang dipegang pasien |
| `CashierShiftId` | Ya untuk tunai | Dasar rekonsiliasi kas dengan shift kasir |
| `ProviderReference` | Ya untuk non-tunai | Telusur ke penyedia pembayaran |
| `ProviderEventId` | Ya bila ada | Anti-ganda dari sisi penyedia |
| `OccurredAt` | Ya | Waktu uang benar-benar diterima |
| `SourceInvoiceStatus` | Ya | **Penentu Finance menahan jurnal atau tidak** — lihat 2.4 |
| `TenderStatus` | Ya | `SUCCEEDED` atau `REVERSED` |
| `HandoffKey` | Ya | Kunci idempotensi, sama pola dengan `BilArHandoff` |
| `CorrelationId`, `CausationId` | Ya | Rantai telusur ujung ke ujung |
| `Status` | Ya | `CREATED` / `ACKNOWLEDGED`, sama pola dengan `BilArHandoff` |

Seluruhnya sudah ada di `BilTender` dan `BilSettlement` hari ini — Finance memeriksanya langsung
ke source pada commit `09101d05`. Jadi permintaan ini tidak menuntut Billing menyimpan data
baru, hanya meneruskan yang sudah ada.

### 2.3 Kapan handoff dibuat

**Saat tender berstatus `SUCCEEDED`, tidak menunggu tagihan difinalisasi.**

Ini bagian yang paling penting. Billing hari ini memungkinkan pasien membayar sementara tagihan
masih terbuka, dan finalisasi menyusul belakangan. Kalau handoff baru dibuat saat finalisasi,
Finance tidak akan pernah tahu tentang uang yang masuk lebih dahulu.

### 2.4 Mengapa `SourceInvoiceStatus` diminta

Finance mencatat penerimaannya segera, tetapi **menahan** penerbitan jurnal ke Accounting sampai
tagihannya final. Bidang ini yang memberi tahu Finance harus menahan atau tidak.

| Keadaan saat tender berhasil | Di Finance | Jurnal ke Accounting |
|---|---|---|
| Tagihan masih terbuka | Penerimaan tercatat penuh | Ditahan |
| Tagihan sudah final | Penerimaan tercatat penuh | Siap diterbitkan |

Finance juga perlu tahu **kapan** tagihan itu akhirnya final, agar jurnal yang tertahan dapat
dilepas. Dua cara yang sama-sama dapat Finance terima:

- `BilArHandoff` yang menyusul saat finalisasi sudah cukup sebagai penanda, **atau**
- satu handoff tambahan bertipe pemberitahuan finalisasi.

Finance mengikuti mana pun yang lebih mudah bagi Billing.

### 2.5 Pembalikan tender

Bila tender yang sudah berhasil kemudian dibalik, Finance meminta **baris handoff baru**
dengan `TenderStatus = REVERSED`, bukan pembaruan baris lama.

Alasannya: Finance membalik penerimaannya dengan membuat baris pembalik, bukan menghapus. Bila
baris handoff lama ikut berubah, riwayatnya hilang di kedua sisi.

### 2.6 Yang Finance janjikan sebagai imbalannya

| Hal | Janji Finance |
|---|---|
| ACK | Finance menandai handoff `ACKNOWLEDGED` setelah penerimaannya berhasil dibuat |
| Kegagalan | Bila Finance gagal mengolah, handoff **tetap** `CREATED` dan Finance mengulangnya sendiri. Billing tidak perlu melakukan apa pun |
| Tabel Billing | Finance tidak pernah menulis ke tabel Billing mana pun, termasuk `BilCashierShift` |
| Nilai | Finance menyalin nominal apa adanya, tidak pernah menghitung ulang |
| Accounting | Finance yang menerbitkan seluruh kejadian ke Accounting; Billing tidak perlu ikut |

---

## 3. Permintaan kedua — piutang manfaat karyawan

**Bagian ini juga ditujukan kepada owner HR.** Berbeda dari bagian 2, permintaan ini **tidak**
menahan pekerjaan Finance mana pun — rumpunnya sudah sengaja dikeluarkan dari rilis pertama
sampai kedua owner menjawab.

### 3.1 Keadaan sekarang

`BilArHandoff.DebtorType` hari ini hanya mengenal dua nilai. Finance memeriksanya langsung ke
source pada `09101d05`:

```
PATIENT_GUARANTOR
PAYER
```

Tidak ada cara menyebut bahwa sebuah piutang sebenarnya tanggungan **pegawai**, sementara yang
dilayani adalah anggota keluarganya.

### 3.2 Yang diminta

| Perubahan | Bentuk | Sifat |
|---|---|---|
| Nilai `DebtorType` baru | `EMPLOYEE_BENEFIT` | Aditif — konsumen lama tidak rusak |
| Kolom baru | `BenefitOwnerId` (`Guid`, boleh kosong) | Nullable — baris lama tetap sah |
| Kolom baru | `BenefitRelationship` (`string(30)`, boleh kosong) | `SELF`, `SPOUSE`, `CHILD`, dan seterusnya |

Keduanya nullable, sehingga seluruh baris `BilArHandoff` yang sudah ada tetap sah tanpa
pengisian data lama.

### 3.3 Pembagian tanggung jawab yang Finance usulkan

| Pihak | Tanggung jawab |
|---|---|
| Registrasi / Billing | **Menentukan** identitas pemilik manfaat dan hubungannya, berdasarkan eligibilitas yang berlaku saat pelayanan |
| Finance | **Menerima apa adanya.** Finance tidak menentukan ulang identitas itu |
| HR | Sumber kebenaran status kepegawaian dan eligibilitas manfaat |

Bila ternyata ada ketidaksesuaian, Finance mengembalikannya lewat alur koreksi — bukan dengan
mengubah sendiri.

### 3.4 Pertanyaan untuk kedua owner

| # | Pertanyaan | Untuk |
|---:|---|---|
| 1 | Apakah perluasan dua kolom dan satu nilai enum di atas dapat diterima? | Billing |
| 2 | Apakah Registrasi/Billing memang pihak yang menentukan identitas pemilik manfaat saat pelayanan? | Billing + HR |
| 3 | Dari mana eligibilitas manfaat dibaca saat itu? | HR |
| 4 | Apakah ada aturan manfaat yang membatasi siapa saja yang boleh ditanggung? | HR |

Finance **tidak** mengarang jawaban atas keempatnya, dan rumpun ini tidak akan dimulai sebelum
jawabannya ada.

---

## 4. Yang tidak Finance minta

Supaya jelas batasnya:

| Tidak diminta | Alasan |
|---|---|
| Perubahan pada `BilTender`, `BilSettlement`, `BilPaymentAllocation` | Seluruh bidang yang Finance butuhkan sudah ada di sana |
| Perubahan pada `BilCashierShift` | Finance hanya membaca `CashierShiftId` untuk rekonsiliasi |
| Perubahan pada perhitungan atau finalisasi tagihan | Bukan urusan Finance |
| Billing mengirim apa pun ke Accounting | Finance yang menerbitkan seluruh kejadian |
| Perubahan pada rumpun kas kecil | Keputusannya tetap milik blueprint `billing-kasir` |

---

## 5. Dampak bila permintaan ini belum turun

| Permintaan | Yang tertahan | Yang tetap jalan |
|---|---|---|
| Bagian 2 — fakta penerimaan | Gelombang `MVP-2` Finance: buku penerimaan dan pembagian bayar-vs-piutang | Data induk, pintu masuk fakta piutang, buku piutang, setoran bank, kas harian |
| Bagian 3 — manfaat karyawan | Rumpun piutang pegawai | Seluruh rumpun piutang lain. Piutang pegawai sementara dicatat sebagai piutang penjamin biasa dengan keterangan tertulis |

Jadi hanya satu gelombang yang benar-benar menunggu, dan Finance sudah menyiapkan urutan kerja
yang tidak macet karenanya.

---

## 6. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/contracts/integration-contract.md` bagian 2 dan 3 | Bentuk kontrak lengkap beserta ketentuan perilakunya |
| `docs/module-blueprints/finance-management/00-interview-decisions.md` | `FIN-DEC-005` dan `FIN-DEC-006` beserta alasannya |
| `docs/module-blueprints/finance-management/04-prd-to-mvp.md` bagian 20 | Urutan gelombang Finance dan apa yang tertahan |
| `docs/module-blueprints/billing-kasir/` | Blueprint Billing, tempat keputusan ini akan dicatat bila disetujui |

---

## 7. Jawaban owner Billing — 21 September 2026

**Permintaan pada bagian 2 DISETUJUI.** Dicatat pada `billing-kasir/00-interview-decisions.md`,
bagian "Amandemen 21 September 2026 — Penerbitan fakta finansial ke modul konsumen".

| Hal | Jawaban Billing | Keputusan |
| --- | --- | --- |
| Jalur pemberitahuan tender berhasil | Disetujui dibangun | `BKC-DEC-106` |
| Bentuk transport | Tabel handoff persisted berkolom tegas, pola `BilArHandoff` — sesuai usulan Finance | `BKC-DEC-106` |
| Dapat di-ACK | Ya; pengambilan surat dicatat dan surat yang menggantung dapat diperiksa. Billing tidak menggantungkan perilakunya pada ACK | `BKC-DEC-108` |
| Retensi baris handoff | Disimpan selamanya sebagai jejak audit lintas modul | `BKC-DEC-109` |

**Yang perlu Finance ketahui.** Billing memutuskan melayani **dua** konsumen dari satu titik
deteksi peristiwa yang sama: Finance dan Farmasi. Keduanya bertumpu pada data yang sama —
`PaymentMethodId` sekaligus membedakan tunai dari non-tunai bagi Finance dan menentukan hasil
`Paid` versus `InsuranceApproved` bagi Farmasi. Bentuk ini menjamin tidak mungkin Finance
mengetahui sebuah pembayaran sementara Farmasi tidak.

Bagi Finance, isi dan pemicu yang diminta pada bagian 2.2 dan 2.3 **diterima apa adanya** —
termasuk terbit saat tender `SUCCEEDED` tanpa menunggu finalisasi, dan pembalikan tender sebagai
baris baru sesuai bagian 2.5.

**Pembaruan 22 September 2026 — task Billing selesai dieksekusi.** `BilConsumerHandoffService.
PublishForTenderAsync` (`BE-BKC-069`) menerbitkan `BilCollectionHandoff` persis sesuai bagian 2.2
di atas, dipasang di `BillingSettlementService.ReconcileTenderAsync` saat tender mencapai
`SUCCEEDED`/`REVERSED`. `BE-FIN-016` dibangun di atasnya hari yang sama (entity `FinReceipt`/
`FinReceiptAllocation`, migration `AddFinanceCollection`, konsumsi lewat `FinanceBillingIntakeService`
diperluas untuk `HandoffType = COLLECTION`) — lihat
`docs/module-blueprints/finance-management/task/report/backend/BE-FIN-016.md`. `BE-FIN-017`
(pembagian bayar-vs-piutang) tetap `BLOCKED` menunggu task pemiliknya sendiri, bukan lagi
menunggu Billing.

**Satu butir terbuka yang tidak memblokir.** Berapa lama sebuah surat boleh menggantung sebelum
dianggap tidak wajar, dan siapa yang menerima peringatannya, dicatat sebagai `BKC-OQ-101` dan
sebaiknya ditetapkan bersama Finance serta Farmasi setelah jalurnya berjalan.
