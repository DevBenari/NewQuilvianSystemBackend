# Billing dan Kasir — Validation Matrix

`contract_version: BIL-VALIDATION-0.4`; status **approved**; owner Product/Billing/Finance/Security; approved 20 Agustus 2026.

| Kode | Aturan | Berlaku pada | Kondisi | Pesan pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-001` | Satu invoice/encounter | Create charge | invoice sudah ada | “Kunjungan ini sudah memiliki invoice; item ditambahkan ke invoice yang sama.” |
| `BIL-VAL-002` | Source aktif unik | Charge | tuple pernah aktif | “Item pelayanan ini sudah tercatat di Billing.” |
| `BIL-VAL-003` | Void sebelum pemeriksaan/bayar | Void | source complete/teralokasi | “Item tidak dapat dibatalkan karena pelayanan atau pembayaran sudah diproses.” |
| `BIL-VAL-004` | Farmasi pakai qty diserahkan | Charge | final dispense belum ada | “Jumlah obat yang diserahkan belum final.” |
| `BIL-VAL-005` | Admin fee sekali/hari | Calculation | pasien sudah dikenai fee lokal hari itu | “Biaya administrasi hari ini sudah dikenakan pada invoice pertama.” |
| `BIL-VAL-006` | Rajal diganti ranap | Calculation | transfer encounter sama | “Biaya administrasi rawat jalan diganti biaya rawat inap.” |
| `BIL-VAL-007` | Admin fee tanpa diskon | Discount | target admin fee | “Biaya administrasi tidak dapat didiskon.” |
| `BIL-VAL-008` | Doctor discount hanya share | Discount | amount > doctor share | “Diskon dokter melebihi komponen jasa dokter.” |
| `BIL-VAL-009` | Doctor approval | Discount | belum approved dokter terkait | “Diskon jasa dokter menunggu persetujuan dokter.” |
| `BIL-VAL-010` | Coverage cap | Calculate | primary+excess > eligible | “Total tanggungan penjamin melebihi biaya yang memenuhi syarat.” |
| `BIL-VAL-011` | Deposit allocation | Allocation | amount > available/outstanding | “Dana deposit atau saldo tagihan tidak mencukupi.” |
| `BIL-VAL-012` | Split exact | Settlement | tender total melewati outstanding | “Total metode pembayaran melebihi saldo yang harus dibayar.” |
| `BIL-VAL-013` | OTC lunas | Clearance | outstanding > 0/pending | “Layanan OTC belum dapat dimulai karena pembayaran belum lunas.” |
| `BIL-VAL-014` | Final order complete | Finalize | ada order belum complete | “Semua order harus selesai sebelum invoice difinalkan.” |
| `BIL-VAL-015` | Calculation current | Finalize | source/tariff changed | “Tagihan berubah; hitung ulang sebelum finalisasi.” |
| `BIL-VAL-016` | Debtor valid | Departure/AR | identitas pihak penanggung kosong | “Pihak yang menanggung sisa tagihan harus dicatat.” |
| `BIL-VAL-017` | Maker-checker | Approve | actor sama | “Pengaju tidak boleh menyetujui permohonannya sendiri.” |
| `BIL-VAL-018` | Write-off not paid | Outcome | full write-off | “Tagihan diselesaikan melalui write-off, bukan pembayaran.” |
| `BIL-VAL-019` | Shift aktif | Cash tender | tidak ada shift OPEN | “Buka shift kasir sebelum menerima uang tunai.” |
| `BIL-VAL-020` | Concurrency | Semua command | version berbeda | “Data telah berubah. Muat ulang sebelum melanjutkan.” |
| `BIL-VAL-021` | Idempotency | Semua command | key reuse dengan payload beda | “Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.” |
| `BIL-VAL-022` | Effective policy | Rule selection | tidak ada/overlap | “Kebijakan tarif yang berlaku belum dikonfigurasi dengan benar.” |
| `BIL-VAL-023` | Insurance rejection | Reallocate | kontrak tidak mengizinkan patient shift | “Penolakan klaim tidak dapat otomatis dibebankan kepada pasien.” |
| `BIL-VAL-024` | Post-final immutable | Edit | invoice final | “Invoice final tidak dapat diedit; ajukan adjustment.” |
| `BIL-VAL-025` (**baru, approved**) | Tarif aktif/efektif | Charge katalog | `TariffId` tidak ditemukan/tidak aktif/di luar periode efektif | “Tarif yang dipilih tidak ditemukan atau sudah tidak berlaku.” |
| `BIL-VAL-026` (**baru, approved**) | Harga katalog tidak dapat diubah manual | Charge katalog | Structural — `AddCatalogChargeRequest` tidak memiliki field harga sama sekali | Tidak ada pesan runtime; invariant ditegakkan lewat kontrak DTO, bukan pengecekan nilai |
| `BIL-VAL-027` (**baru, approved**) | Encounter/tarif valid untuk preview | Coverage preview | `encounterId`/`tariffId` tidak valid atau tidak ditemukan | “Data kunjungan atau tarif tidak valid untuk memeriksa status coverage.” |

Validasi wajib server-side; UI hanya membantu. Seluruh nominal non-negatif, currency konsisten, waktu effective-dated dibandingkan dalam timezone yang didefinisikan, reason wajib untuk void/exception/reopen. Test mapping: `BIL-AT-001`–`024`.

Security/privacy: validasi tidak boleh mengulang nomor identitas, detail klinis, atau provider reference penuh dalam error. Denied action tidak mengungkap keberadaan invoice di luar scope pengguna. Trace `BKC-DEC-001`–`044`.

## Amendment 3 September 2026 — Dokumen Invoice Asuransi

`contract_version: BIL-VALIDATION-0.5` · status **draft** · owner Product/Billing/Finance/Security · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009`.

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-028` (**baru, draft**) | Rincian tanggungan per baris wajib menjumlah ke total tanggungan | Setiap perhitungan invoice (`ApplyCoverageWaterfall`) | Jumlah `coveredAmount` seluruh baris tidak sama dengan `primaryAmount + excessAmount` | "Rincian tanggungan penjamin per baris tidak menjumlah ke total tanggungan; hubungi tim teknis." |
| `BIL-VAL-029` (**baru, draft**) | Dokumen Invoice Asuransi hanya untuk kunjungan berpenjamin asuransi | `GET {id}/insurance-invoice-document` | `TrxPatientEncounterGuarantor.PaymentType` bernilai `Cash` | "Kunjungan ini dibayar mandiri, sehingga tidak ada Invoice Asuransi yang dapat diterbitkan." (`200`, `isPrintable=false` — bukan galat) |
| `BIL-VAL-030` (**baru, draft**) | Penjamin perusahaan tempat kerja belum didukung dokumen ini | `GET {id}/insurance-invoice-document` | `PaymentType` bernilai `CompanyGuarantor` | "Penjamin kunjungan ini adalah perusahaan tempat kerja, bukan perusahaan asuransi. Dokumen ini belum mendukung penjamin perusahaan." (`200`, `isPrintable=false`) |
| `BIL-VAL-031` (**baru, draft**) | Sumber pembayaran kunjungan wajib tercatat | `GET {id}/insurance-invoice-document` | Tidak ada baris penjamin aktif untuk kunjungan itu | "Sumber pembayaran kunjungan ini belum tercatat. Lengkapi data penjamin di Registrasi terlebih dahulu." (`200`, `isPrintable=false`) |
| `BIL-VAL-032` (**baru, draft**) | Dokumen tanpa baris tercover tidak dapat dicetak | `GET {id}/insurance-invoice-document` | Pasien asuransi tetapi tidak ada baris dengan `coveredAmount > 0` | "Tidak ada item yang ditanggung asuransi pada tagihan ini." (`200`, `items` kosong, `isPrintable=false`) |
| `BIL-VAL-033` (**baru, draft**) | Rincian per baris tidak tersedia untuk versi kalkulasi lama | `GET {id}/insurance-invoice-document` | Invoice non-`OPEN` dan `isPerItemAllocationAvailable` pada snapshot bernilai `false` | "Rincian per item tidak tersedia untuk tagihan yang difinalkan sebelum pembaruan sistem ini. Total tanggungan penjamin tetap sah." (`200`, `isPrintable=false`) |
| `BIL-VAL-034` (**baru, draft**) | Data perusahaan asuransi wajib ada di master | `GET {id}/insurance-invoice-document` | `InsuranceProviderId` terisi tetapi barisnya tidak ditemukan/tidak aktif di `MstInsuranceProvider` | "Data perusahaan asuransi tidak ditemukan pada master. Hubungi admin master data." (`200`, `isPrintable=false`) |

**Contoh berangka untuk `BIL-VAL-028`.** Tagihan dengan tiga item tercover Rp 100.000, Rp 240.000, dan Rp 15.000 menghasilkan jumlah baris Rp 355.000. Bila `primaryAmount` yang dihitung mesin coverage ternyata Rp 350.000, perhitungan **dihentikan** — bukan diteruskan dengan selisih Rp 5.000 yang akan muncul sebagai lembar tagihan yang tidak menjumlah. Aturan ini tanpa toleransi pembulatan, karena setiap nominal per komponen sudah dibulatkan dua desimal di sumbernya, sehingga selisih apa pun berarti bug alokasi, bukan pembulatan.

**Catatan penting soal `BIL-VAL-029`–`034`.** Keenam aturan ini menghasilkan `200`, bukan `422`. Alasannya (`BKC-DES-008`): keadaan seperti "pasien ini bayar tunai" adalah keadaan bisnis normal, bukan permintaan yang gagal. Yang membedakannya dari keberhasilan adalah `isPrintable=false` dan isi `warnings`. Layar **MUST** menampilkannya sebagai keterangan biru, bukan pesan galat merah.

Validasi tetap wajib server-side; layar hanya membantu. Pesan pada `warnings` **MUST NOT** memuat nomor rekam medis, nomor polis, nama pasien, maupun kode aturan asuransi. Trace `BKC-DEC-065`–`069`, `BKC-DES-001`–`009`. Test mapping: `BIL-AT-029`–`035`.

---

## Amendment 4 September 2026 — Anomali data penjamin dan gerbang PPN care setting

`last_changed_in: BIL-VALIDATION-0.6` · status **draft** · owner Product/Billing/Finance/Security · `approved_by`/`approved_at`: belum ada. Input: `BKC-DEC-070`–`079` (approved 4 September 2026), keputusan arsitektur `BKC-DES-010`–`020`. Dampak kompatibilitas: **additive** untuk aturan baru; **satu aturan existing diubah syaratnya** (`BIL-VAL-028`) dan **empat gerbang lama dicabut**.

### Aturan yang dicabut

| Kode | Aturan lama | Status | Dasar |
| --- | --- | --- | --- |
| — | Rule `CoverageStatus = "NeedApproval"` menahan komponen ke `unresolved` | **Dicabut** | `BKC-DEC-071` |
| — | `IsNeedApproval`/`IsNeedGuaranteeLetter` menahan komponen ke `unresolved` | **Dicabut** (sudah dicabut sebagian oleh `BKC-DEC-062`, kini tuntas) | `BKC-DEC-071` |
| — | `MaxAmountPerMonth` terisi menahan komponen ke `unresolved` | **Dicabut** | `BKC-DEC-071` |
| — | `MaxQuantityPerMonth` terisi menahan komponen ke `unresolved` | **Dicabut** | `BKC-DEC-071` |

Pencabutan ini **MUST NOT** dibaca sebagai "keempat kolom itu tidak berarti lagi". Kolomnya tetap ada, tetap dapat diisi admin, dan tetap dibaca `InsuranceCoverageService` untuk keperluan advisory di layar entri. Yang dicabut adalah kemampuannya **menahan perhitungan tagihan**.

### Aturan yang diubah

| Kode | Aturan | Berlaku pada | Kondisi | Pesan pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-028` (**diubah, draft**) | Rincian tanggungan per baris wajib menjumlah ke total tanggungan | Setiap perhitungan invoice (`ApplyCoverageWaterfall`) | Jumlah `itemPrimaryAmount + taxPrimaryAmount` seluruh baris, ditambah `primaryAmount` biaya administrasi dan biaya kamar, tidak sama dengan `coverage.primaryAmount` | "Rincian tanggungan penjamin per baris tidak menjumlah ke total tanggungan; hubungi tim teknis." |

Perubahannya hanya pada **cara menjumlah**: `BKC-DES-015` memutuskan tidak ada field turunan `coveredAmount` per baris, sehingga penjumlahannya memakai field yang benar-benar ada. Ambang toleransinya tetap **nol**, dengan alasan yang sama seperti sebelumnya: setiap nominal per komponen sudah dibulatkan dua desimal di sumbernya, sehingga selisih apa pun berarti bug alokasi, bukan pembulatan.

### Aturan baru

| Kode | Aturan | Berlaku pada | Kondisi | Pesan pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-035` (**baru, draft**) | Nominal anomali data tidak boleh melebihi biaya yang memenuhi syarat | `ApplyCoverageWaterfall` | `dataAnomalyAmount > coverableAmount` | "Nilai anomali data penjamin melebihi biaya yang memenuhi syarat; hubungi tim teknis." (`422`) |
| `BIL-VAL-036` (**baru, draft**) | Tanggungan penjamin yang ditolak hanya boleh menjadi tanggungan pasien bila tercatat sebagai anomali data | `ApplyCoverageWaterfall` | `primaryStatus` mengandung `REJECTED`, `coverableAmount > 0`, dan `dataAnomalyAmount == 0` | "Coverage yang ditolak tidak boleh otomatis dipindahkan ke pasien tanpa policy kontrak." (`422`) |
| `BIL-VAL-037` (**baru, draft**) | Setiap anomali data wajib punya kode dan kalimat penjelas | `ResolveAsync` → `CoverageCalculationResponse` | `hasDataAnomaly` bernilai `true` tetapi `anomalyCodes` kosong | "Anomali data penjamin terdeteksi tanpa keterangan; hubungi tim teknis." (`422`) — invariant internal, seharusnya tidak pernah muncul bagi pengguna |
| `BIL-VAL-038` (**baru, draft**) | PPN dibebaskan untuk kunjungan rawat inap | `ApplyInvoiceTax` | `BilInvoice.ServiceType` bernilai `"RANAP"` | Tidak ada pesan penolakan. Pajak tidak dihitung, `taxes` kosong, `taxAmount` setiap item `0`. Ini keadaan normal, bukan galat (`BKC-DEC-078`) |
| `BIL-VAL-039` (**baru, draft**) | Care setting yang tidak dikenal tetap dikenai PPN | `ApplyInvoiceTax` | `ServiceType` bernilai `null` atau teks di luar daftar yang dikenal | Tidak ada pesan penolakan. Pajak tetap dihitung (`BKC-DES-019`). Menghentikan seluruh kalkulasi karena satu teks care setting yang tidak dikenal jauh lebih merugikan daripada memungut pajak yang dapat dikoreksi |

### Pesan anomali data yang tampil ke pengguna

Keempat kode di bawah menghasilkan kalkulasi yang **berhasil** (`200`), bukan galat. Yang membedakannya dari keadaan normal adalah `hasDataAnomaly = true` dan isi `anomalyMessages`. Layar **MUST** menampilkannya sebagai peringatan kuning di atas Ringkasan Pembayaran, **MUST NOT** sebagai baris subtotal, dan **MUST NOT** sebagai pesan galat merah (`BKC-DES-011`).

| Kode | Kondisi | Kalimat yang dibaca kasir |
| --- | --- | --- |
| `PAYER_NOT_ELIGIBLE` | `TrxPatientEncounterGuarantor.IsEligible` bernilai `false` | "Penjamin kunjungan ini belum dinyatakan layak (eligible). Seluruh biaya untuk sementara dibebankan ke pasien. Periksa data penjamin di Registrasi sebelum menagih." |
| `POLICY_INACTIVE` | `IsPolicyActive` bernilai `false` | "Polis asuransi kunjungan ini tercatat tidak aktif. Seluruh biaya untuk sementara dibebankan ke pasien. Periksa data penjamin di Registrasi sebelum menagih." |
| `INSURANCE_PROVIDER_MISSING` | `InsuranceProviderId` kosong padahal jenis pembayaran bukan tunai | "Perusahaan asuransi kunjungan ini belum dipilih. Seluruh biaya untuk sementara dibebankan ke pasien. Lengkapi data penjamin di Registrasi." |
| `ENCOUNTER_NOT_FOUND` | Data kunjungan tidak ditemukan saat penilaian penjamin | "Data kunjungan tidak ditemukan saat memeriksa penjamin. Hubungi tim teknis sebelum menagih." |

> **Contoh berangka.** Kunjungan rawat jalan pasien asuransi dengan biaya coverable Rp 440.000, tetapi kolom `IsEligible` belum dicentang petugas pendaftaran. Perhitungan **berhasil**: Subtotal Asuransi Rp 0, Subtotal Mandiri Rp 440.000, Total Tagihan Rp 440.000, `dataAnomalyAmount = 440000`, `anomalyCodes = ["PAYER_NOT_ELIGIBLE"]`. Kasir melihat peringatan kuning dan tetap dapat menerima pembayaran. **Sebelum amendment ini**, Rp 440.000 yang sama muncul sebagai "Penjamin Belum Terverifikasi" dengan Subtotal Mandiri Rp 0, dan kasir tidak punya angka yang dapat ditagihkan.

> **Contoh berangka untuk `BIL-VAL-038`.** Pasien rawat inap menerima obat senilai Rp 1.000.000 dan biaya kamar Rp 2.000.000. Tarif PPN aktif 11%. **Sebelum amendment ini** tagihan memuat PPN Rp 110.000 atas obatnya sehingga total Rp 3.110.000. **Sesudah** tidak ada PPN sama sekali dan total menjadi Rp 3.000.000. Pasien rawat jalan yang menerima obat yang sama tetap dikenai PPN Rp 110.000 (`BKC-DEC-078`); pasien IGD diperlakukan sama dengan rawat jalan (`BKC-DEC-079`).

Trace `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`. Test mapping: `BIL-AT-036`–`048`.

---

## Amendment lanjutan 4 September 2026 — Residual non-billable dirutekan ke write-off

`last_changed_in: BIL-VALIDATION-0.7` · status **draft** · owner Product/Billing/Finance/Security · `approved_by`/`approved_at`: belum ada. Input: **`BKC-DEC-080`** (`approved` 4 September 2026) beserta `BKC-DEC-036` (`approved` 20 Agustus 2026); keputusan arsitektur `BKC-DES-021`–`025`. Dampak kompatibilitas: **additive** — tiga aturan baru, satu aturan lama dipertegas cakupannya, tidak ada aturan yang dicabut.

### Apa yang sedang dijaga aturan-aturan ini

Sebuah aturan tanggungan dapat menyatakan dua hal sekaligus: penjamin hanya menanggung sebagian, **dan** selisihnya tidak boleh ditagihkan ke pasien. Selisih itu tidak menjadi milik siapa pun. `BKC-DEC-080` memutuskan rumah sakit yang menanggungnya, lewat jalur Pengecualian Finansial/write-off yang sudah ada — bukan lewat angka yang berhenti di layar tanpa tindak lanjut.

Aturan di bawah menjaga tiga hal: nominal yang ditulis-off tidak melebihi selisih yang benar-benar ada, penulisan-off itu tidak diam-diam mengurangi tagihan pasien, dan kategorinya tidak dapat diisi sembarang teks.

### Aturan yang dipertegas cakupannya

| Kode | Aturan | Berlaku pada | Kondisi | Pesan pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-018` (**dipertegas, draft**) | Write-off tidak pernah menghasilkan `PAID` | Approve write-off | Hanya write-off kategori `PATIENT_AR` dengan `IsFullSettlement = true` yang memindahkan invoice ke `SETTLED_BY_WRITE_OFF`. Kategori `NON_BILLABLE_RESIDUAL` **MUST NOT** memindahkan status invoice ke mana pun | “Tagihan diselesaikan melalui write-off, bukan pembayaran.” (tidak berubah; hanya berlaku untuk kategori `PATIENT_AR`) |
| `BIL-VAL-023` (**dipertegas, draft**) | Penolakan klaim tidak dapat otomatis dibebankan kepada pasien | Reallocate/Calculate | Sesudah `BKC-DEC-080`, aturan ini punya jalur penyelesaian yang jelas: selisih yang kontraknya melarang penagihan ke pasien masuk `nonBillableResidualAmount` dan diselesaikan lewat write-off, **bukan** ditahan tanpa tindak lanjut | “Penolakan klaim tidak dapat otomatis dibebankan kepada pasien.” (tidak berubah) |

### Aturan baru

| Kode | Aturan | Berlaku pada | Kondisi | Pesan pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-040` (**baru, draft**) | Write-off residual non-billable dibatasi sisa residual, bukan outstanding pasien | `POST .../financial-exceptions/write-offs` dan `POST .../write-offs/{id}/approve`, kategori `NON_BILLABLE_RESIDUAL` | `Amount` melebihi sisa residual non-billable invoice itu (nominal pada versi kalkulasi terkini dikurangi write-off residual yang sudah `POSTED` dan belum direversal) | “Nominal write-off melebihi selisih yang tidak dapat ditagihkan pada tagihan ini.” (`422`) |
| `BIL-VAL-041` (**baru, draft**) | Write-off residual non-billable bukan pelunasan tagihan | `POST .../financial-exceptions/write-offs`, kategori `NON_BILLABLE_RESIDUAL` | `IsFullSettlement` bernilai `true` | “Selisih yang tidak dapat ditagihkan bukan pelunasan tagihan pasien; hapus tanda pelunasan penuh.” (`422`) |
| `BIL-VAL-042` (**baru, draft**) | Kategori write-off wajib salah satu nilai yang terdaftar | `POST .../financial-exceptions/write-offs` | `Category` terisi tetapi bukan `PATIENT_AR` maupun `NON_BILLABLE_RESIDUAL` | “Kategori write-off tidak dikenali.” (`422`). Nilai kosong diperlakukan `PATIENT_AR`; teks asing **MUST NOT** diperlakukan sebagai nilai bawaan |
| `BIL-VAL-043` (**baru, draft**) | Nominal residual non-billable tidak boleh melebihi biaya yang memenuhi syarat | `ApplyCoverageWaterfall` | `primaryAmount + excessAmount + unresolvedAmount + nonBillableResidualAmount > coverableAmount` | “Selisih yang tidak dapat ditagihkan melebihi biaya yang memenuhi syarat; hubungi tim teknis.” (`422`) |

**Kenapa `BIL-VAL-043` menjumlahkan, sedangkan `DataAnomalyAmount` justru dikecualikan.** Keduanya terlihat serupa dan perlakuannya berlawanan, jadi alasannya ditulis eksplisit agar tidak “dirapikan” orang berikutnya. Nominal anomali data **sudah** terwakili sebagai porsi pasien (`BKC-DES-011`), sehingga menjumlahkannya berarti menghitung uang yang sama dua kali. Residual non-billable **tidak** terwakili di suku mana pun — ia dikeluarkan dari porsi pasien dan tidak masuk porsi penjamin — sehingga bila ia juga dikecualikan dari pemeriksaan batas, tidak ada satu pun penjaga yang mencegahnya membengkak melebihi biaya tagihannya.

**Kenapa pengajuan write-off tetap perbuatan manusia.** Tidak ada aturan pada tabel di atas yang membuat sistem mengajukan write-off sendiri, dan itu disengaja (`BKC-DES-023`). Sistem menghitung nominalnya, menandainya, dan menyiapkannya untuk diisikan; Finance yang mengajukan dan orang kedua yang menyetujui. `BIL-VAL-017` (pengaju tidak boleh menyetujui pengajuannya sendiri) tetap berlaku utuh untuk kategori residual — dan justru aturan itulah yang akan runtuh bila pengajuannya dibuat mesin.

### Contoh berangka

**Kasus normal.** Tagihan rawat jalan Rp 100.000 untuk satu tindakan. Aturan tanggungan menanggung 70% dan menandai selisihnya tidak boleh ditagihkan ke pasien.

| Nominal | Nilai |
| --- | ---: |
| Subtotal Asuransi | Rp 70.000 |
| Subtotal Mandiri (ditagih kasir) | Rp 0 |
| `nonBillableResidualAmount` | Rp 30.000 |
| Total Tagihan | Rp 100.000 |

Finance membuka Pengecualian Finansial pada tagihan itu, melihat “Selisih tidak dapat ditagihkan yang belum ditulis-off: Rp 30.000”, mengajukan write-off Rp 30.000 berkategori `NON_BILLABLE_RESIDUAL` beserta alasannya, dan atasannya menyetujui. Sesudah disetujui: outstanding pasien **tetap Rp 0**, status tagihan **tidak berpindah**, dan sisa residual menjadi Rp 0.

**Kasus yang ditolak `BIL-VAL-040`.** Pada tagihan yang sama, Finance mengajukan Rp 45.000. Ditolak `422` — sisa residualnya hanya Rp 30.000, walaupun Total Tagihan Rp 100.000 dan walaupun pasien pada tagihan lain punya outstanding yang jauh lebih besar. Plafonnya adalah selisihnya sendiri, bukan tagihannya.

**Kasus yang ditolak `BIL-VAL-041`.** Finance mengajukan Rp 30.000 berkategori residual sambil mencentang “pelunasan penuh”. Ditolak `422`. Bila dibiarkan, tagihan yang porsi pasiennya memang sudah Rp 0 akan tercatat “diselesaikan lewat write-off”, dan auditor akan membaca bahwa rumah sakit menghapus piutang pasien — padahal pasien tidak pernah berutang satu rupiah pun pada tagihan itu.

Validasi tetap wajib server-side; layar hanya membantu. `Reason` pada pengajuan write-off **MUST NOT** memuat nomor polis, nomor anggota, nama pasien, maupun diagnosis. Trace **`BKC-DEC-080`**, `BKC-DEC-036`, `BKC-DES-021`–`025`. Test mapping: `BIL-AT-055`–`061`, beserta `BIL-AT-040` yang dikoreksi.
