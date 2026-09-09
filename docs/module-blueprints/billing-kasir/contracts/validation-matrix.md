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

`contract_version: BIL-VALIDATION-0.5` · status **approved** · owner Product/Billing/Finance/Security · approved_by Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · approved_at 4 September 2026 · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009` (approved).

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

`last_changed_in: BIL-VALIDATION-0.6` · status **approved** · owner Product/Billing/Finance/Security · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 4 September 2026. Input: `BKC-DEC-070`–`079` (approved 4 September 2026), keputusan arsitektur `BKC-DES-010`–`020`. Dampak kompatibilitas: **additive** untuk aturan baru; **satu aturan existing diubah syaratnya** (`BIL-VAL-028`) dan **empat gerbang lama dicabut**.

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

`last_changed_in: BIL-VALIDATION-0.7` · status **approved** · owner Product/Billing/Finance/Security · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 4 September 2026. Input: **`BKC-DEC-080`** (`approved` 4 September 2026) beserta `BKC-DEC-036` (`approved` 20 Agustus 2026); keputusan arsitektur `BKC-DES-021`–`025`. Dampak kompatibilitas: **additive** — tiga aturan baru, satu aturan lama dipertegas cakupannya, tidak ada aturan yang dicabut.

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

---

## Amendment 7 September 2026 — Rumpun baru: Petty Cash (Voucher Kas Kecil)

`last_changed_in: BIL-VALIDATION-0.8` · status **draft** · owner Product/Billing/Finance Operations/Security · `approved_by`: — · `approved_at`: — · input: **`PC-DEC-001`–`PC-DEC-013`** (`approved` 7 September 2026); keputusan arsitektur `PC-DES-001`–`PC-DES-014`. Dampak kompatibilitas: **sepenuhnya aditif** — lima belas aturan baru pada endpoint yang seluruhnya baru. **Tidak ada** aturan existing yang dicabut, diubah, atau dipertegas.

### Apa yang sedang dijaga aturan-aturan ini

Tiga hal, berurut dari yang paling mahal bila gagal:

1. **Uang rumah sakit tidak boleh keluar melebihi yang tersedia.** Saldo kas kecil tidak boleh negatif, dalam keadaan apa pun, termasuk ketika dua kasir menekan tombol pada saat hampir bersamaan.
2. **Uang tidak boleh keluar dua kali untuk satu voucher.** Tombol yang tertekan dua kali, jaringan yang terputus lalu dicoba ulang, dan dua petugas yang membuka voucher yang sama harus tetap menghasilkan tepat satu penyerahan uang.
3. **Setiap pengeluaran punya nama dan alasan.** Voucher yang ditolak tetap tercatat, dan penolakan tanpa alasan tidak diterima.

### Aturan baru — voucher

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-044` (**baru, draft**) | Isian voucher wajib lengkap dan masuk akal | `POST /petty-cash/vouchers` | `recipientName` kosong atau lebih dari 150 karakter; `purpose` kosong atau lebih dari 500 karakter; `amount` kurang dari atau sama dengan nol | "Nama Penerima, Kategori, Nominal Voucher, dan Tujuan wajib diisi. Nominal harus lebih besar dari nol." (`400`) |
| `BIL-VAL-045` (**baru, draft**) | Kategori voucher wajib kategori yang masih aktif | `POST /petty-cash/vouchers` | `categoryId` tidak ditemukan, sudah ditandai terhapus, atau `IsActive = false` | "Kategori yang dipilih sudah tidak aktif. Pilih kategori lain." (`422`) |
| `BIL-VAL-046` (**baru, draft**) | Uang hanya boleh diserahkan untuk voucher yang sudah disetujui | `POST /petty-cash/vouchers/{id}/disburse` | Status bukan `APPROVED` | "Voucher ini belum disetujui, jadi uangnya belum bisa diserahkan." (`422`) |
| `BIL-VAL-047` (**baru, draft**) | Persetujuan dibatasi sisa anggaran yang benar-benar bebas | `POST /petty-cash/vouchers/{id}/approve` | `amount` melebihi `CurrentBalance − ReservedAmount` | "Nominal voucher melebihi sisa anggaran kas kecil yang tersedia. Sisa yang bisa dipakai saat ini Rp {availableAmount}." (`422`) |
| `BIL-VAL-048` (**baru, draft**) | Penyerahan uang diperiksa ulang terhadap saldo saat itu juga | `POST /petty-cash/vouchers/{id}/disburse` | `amount` melebihi `CurrentBalance` pada saat pencairan | "Saldo kas kecil tidak mencukupi untuk menyerahkan uang voucher ini. Tambah anggaran terlebih dahulu." (`422`) |
| `BIL-VAL-049` (**baru, draft**) | Penolakan wajib beralasan | `POST /petty-cash/vouchers/{id}/reject` | `rejectionReason` kosong atau lebih dari 500 karakter | "Alasan penolakan wajib diisi." (`422`) |
| `BIL-VAL-050` (**baru, draft**) | Pembatalan hanya oleh pemohon dan hanya selagi belum diputuskan | `POST /petty-cash/vouchers/{id}/cancel` | Status bukan `WAITING_APPROVAL`, atau pembatal bukan `RequestedBy` | Status salah → "Voucher yang sudah diputuskan tidak dapat dibatalkan." (`422`). Bukan pemohonnya → "Hanya pemohon voucher ini yang dapat membatalkannya." (`403`) |
| `BIL-VAL-051` (**baru, draft**) | Bukti nota hanya untuk voucher yang uangnya sudah diserahkan | `POST /petty-cash/vouchers/{id}/proofs` | Status bukan `CASH_RECEIVED` maupun `COMPLETED`; atau `proofReferenceNumber` kosong | Status salah → "Bukti nota hanya dapat dimasukkan setelah uang diserahkan." (`422`). Nomor kosong → "Nomor nota atau kwitansi wajib diisi." (`400`) |
| `BIL-VAL-052` (**baru, draft**) | Voucher yang ditolak tidak dapat diubah oleh jalur mana pun | Seluruh endpoint voucher | Status bernilai `REJECTED` | "Voucher yang sudah ditolak tidak dapat diubah. Buat voucher baru bila pengeluarannya masih diperlukan." (`422`). **Catatan penerapan:** aturan ini ditegakkan **secara struktural** dengan meniadakan endpoint pengubahnya (`PC-DES-013`); pemeriksaan runtime ini adalah lapis kedua, bukan satu-satunya penjaga |

### Aturan baru — kategori dan anggaran

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-053` (**baru, draft**) | Kategori yang sudah dipakai tidak dapat dihapus | `DELETE /master-data/petty-cash-categories/{id}` | Masih ada `BilPettyCashVoucher` yang menunjuk kategori itu dan belum ditandai terhapus | "Kategori ini tidak dapat dihapus karena sudah dipakai voucher. Nonaktifkan saja bila tidak dipakai lagi." (`400`) |
| `BIL-VAL-054` (**baru, draft**) | Koreksi saldo tidak boleh membuat anggaran negatif atau mengingkari janji yang sudah disetujui | `POST /petty-cash/budget/adjustments` | Saldo hasil koreksi kurang dari nol, **atau** kurang dari nominal voucher yang sudah disetujui tetapi belum dicairkan | "Koreksi ini akan membuat saldo kas kecil tidak mencukupi untuk voucher yang sudah disetujui. Sisa yang sudah dijanjikan Rp {reservedAmount}." (`422`) |
| `BIL-VAL-055` (**baru, draft**) | Penambahan dan koreksi anggaran wajib beralasan | `POST /petty-cash/budget/top-ups` dan `/adjustments` | `reason` kosong atau lebih dari 500 karakter; atau `amount` kurang dari atau sama dengan nol | "Nominal dan alasan wajib diisi, dan nominal harus lebih besar dari nol." (`400`) |
| `BIL-VAL-056` (**baru, draft**) | Kode kategori wajib unik | `POST` dan `PUT /master-data/petty-cash-categories` | `categoryCode` sudah dipakai kategori lain yang belum ditandai terhapus | "Kode kategori sudah dipakai kategori lain. Gunakan kode yang berbeda." (`409`) |
| `BIL-VAL-057` (**baru, draft**) | Satu voucher paling banyak satu pengurangan saldo | `POST /petty-cash/vouchers/{id}/disburse` | Sudah ada baris ledger `DISBURSEMENT` untuk voucher itu | "Uang untuk voucher ini sudah pernah diserahkan." (`409`). **Dijaga tiga lapis**: kunci baris kolam, `RowVersion`, dan unique index parsial di database |
| `BIL-VAL-058` (**baru, draft**) | Nomor voucher wajib unik dan dibuat sistem | `POST /petty-cash/vouchers` | Alokasi nomor gagal, atau nomor bentrok | "Nomor voucher gagal dibuat. Coba lagi." (`409`). Seluruh transaction dibatalkan; **tidak ada** voucher tanpa nomor yang tersimpan |

### Contoh berangka untuk `BIL-VAL-047` dan `BIL-VAL-048`

Kedua aturan ini terlihat mirip dan perlakuannya berbeda, jadi contohnya ditulis lengkap.

**Kasus `BIL-VAL-047` — persetujuan ditolak karena komitmen.** Saldo kas kecil Rp 5.000.000. Voucher A Rp 300.000 sudah disetujui pukul 09.00 tetapi uangnya belum diserahkan, sehingga `reservedAmount` Rp 300.000 dan `availableAmount` Rp 4.700.000. Pukul 09.05 Kepala Kasir hendak menyetujui voucher B senilai Rp 4.800.000.

| Nominal | Nilai |
| --- | ---: |
| `currentBalance` | Rp 5.000.000 |
| `reservedAmount` | Rp 300.000 |
| `availableAmount` | Rp 4.700.000 |
| Nominal voucher B | Rp 4.800.000 |

Rp 4.800.000 melebihi Rp 4.700.000, sehingga persetujuan **ditolak** `422` dengan pesan yang menyebut angka Rp 4.700.000. Voucher B tetap `Menunggu Persetujuan` dan dapat disetujui setelah Finance menambah anggaran. **Yang terjadi bila aturan ini tidak ada:** keduanya lolos, keduanya dicairkan, dan saldo menjadi minus Rp 100.000.

**Kasus `BIL-VAL-048` — pencairan ditolak walaupun persetujuannya sah.** Voucher A Rp 300.000 disetujui pukul 09.00 saat saldo Rp 5.000.000. Pukul 10.00 Finance mengoreksi saldo turun menjadi Rp 200.000 karena penghitungan ulang uang fisik. Pukul 11.00 kasir menekan "Uang Diberikan" untuk voucher A.

Rp 300.000 melebihi Rp 200.000, sehingga pencairan **ditolak** `422`. Voucher A tetap `Disetujui`, saldo tetap Rp 200.000, dan tidak ada baris ledger yang lahir. Ini yang dijaga penjaga kedua `PC-DES-006`: persetujuannya memang sah pada saat disetujui, tetapi keadaannya berubah sesudahnya.

Perlu dicatat bahwa `BIL-VAL-054` sebenarnya sudah mencegah skenario ini terjadi lewat jalur koreksi — Finance akan ditolak saat mencoba menurunkan saldo di bawah komitmen Rp 300.000. Penjaga kedua tetap ada karena keadaan lain masih mungkin, misalnya voucher yang disetujui sebelum aturan komitmen berlaku.

### Contoh berangka untuk `BIL-VAL-057`

Kasir menekan "Uang Diberikan" pada voucher `PTC-20260907-0001` senilai Rp 300.000. Jaringan lambat, kasir menekan lagi.

| Keadaan | Yang terjadi |
| --- | --- |
| Permintaan kedua membawa `Idempotency-Key` yang sama | Sistem mengembalikan hasil permintaan pertama apa adanya. Saldo berkurang **satu kali** Rp 300.000 |
| Permintaan kedua tanpa `Idempotency-Key` | Status voucher sudah `Uang Diterima`, bukan `Disetujui`, sehingga ditolak `BIL-VAL-046` |
| Kedua permintaan tiba benar-benar bersamaan | Kunci penasihat mengantrekannya. Yang kedua menemukan status sudah berubah dan ditolak |
| Ketiga lapis di atas gagal karena sebab yang tidak terduga | Unique index parsial pada `BilPettyCashBudgetMovement` menolak baris pencairan kedua di tingkat database |

Saldo akhir Rp 4.700.000 pada keempat keadaan. Inilah yang dimaksud "dijaga tiga lapis".

### Aturan yang **tidak** dibuat, beserta alasannya

| Aturan yang tidak dibuat | Alasan |
| --- | --- |
| Penyetuju tidak boleh menyetujui voucher yang diajukannya sendiri | `PC-DEC-004` menetapkan **satu jenjang** persetujuan tanpa menyebut pemeriksaan dua orang, berbeda dari write-off yang memang punya `BIL-VAL-017`. Membuat aturannya berarti mengarang kebijakan yang pemiliknya tidak minta. Risikonya dicatat apa adanya pada `contracts/permission-audit-matrix.md` dan diangkat sebagai `PC-OQ-004` |
| Batas nominal maksimum per voucher | Tidak ada keputusan yang menetapkannya. `PC-DEC-004` justru menolak eskalasi berjenjang berdasar nominal |
| Tenggat waktu bukti nota | `PC-DEC-006` menyatakan eksplisit tidak ada mekanisme pemaksaan pada MVP ini |
| Larangan membuat dua voucher serupa pada hari yang sama | Tidak diminta, dan pengeluaran kas kecil yang berulang untuk penerima yang sama adalah hal wajar |
| Pembatasan kategori berdasarkan peran pengguna | Tidak diminta. Kategori adalah pengelompokan pelaporan, bukan pembatas kewenangan |

Validasi tetap **wajib server-side**; layar hanya membantu pengguna mengisi lebih cepat. Seluruh nominal non-negatif, waktu dibandingkan dalam zona Asia/Jakarta, dan `reason` wajib untuk penolakan, pembatalan, penambahan anggaran, serta koreksi anggaran.

Pesan galat **MUST NOT** memuat `RecipientName`, `Purpose`, maupun `RejectionReason`; nominal dan sisa anggaran boleh disebut karena keduanya justru yang dibutuhkan pengguna untuk bertindak.

Trace **`PC-DEC-001`–`013`**, `PC-DES-001`–`014`. Test mapping: `BIL-AT-064`–`BIL-AT-080`.
