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

---

# Amendment 11 September 2026 — Rumpun Edit Tagihan & Multi-Payer Coverage

> `last_changed_in`: `BIL-VALIDATION-0.9` / revisi blueprint `1.1`, status **draft**. Masukan: `MPY-DEC-001`–`010`, `MPY-DES-001`–`017`.
>
> Seluruh pesan ditulis sebagaimana dibaca kasir atau admin, bukan sebagai istilah teknis.

## Gerbang kelayakan edit — berlaku untuk ketiga perintah

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `BIL-VAL-059` | Ganti payer, penanggung item, disposisi obat | Tagihan tidak berstatus `OPEN` | "Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi." | `409` |
| `BIL-VAL-060` | Ketiganya | Sudah ada pembayaran berhasil pada tagihan ini | "Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan." | `409` |
| `BIL-VAL-061` | Ketiganya | Versi baris tagihan yang dikirim berbeda dari yang tersimpan | "Data tagihan telah berubah sejak layar ini dibuka. Muat ulang data sebelum menyimpan kembali." | `409` |
| `BIL-VAL-062` | Ketiganya | Alasan kosong | "Alasan perubahan wajib diisi." | `400` |
| `BIL-VAL-063` | Ketiganya | `Idempotency-Key` tidak dikirim | "Permintaan tidak lengkap. Muat ulang halaman lalu coba lagi." | `400` |

Ketika salah satu gerbang ini menolak, **tidak ada satu pun perubahan yang tersimpan** — termasuk perubahan yang secara terpisah sebenarnya sah.

## Ganti payer kunjungan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `BIL-VAL-064` | Ganti payer | `paymentType` bernilai di luar `CASH`/`INSURANCE`/`COMPANY_GUARANTOR` | "Jenis pembayaran tidak dikenali." | `400` |
| `BIL-VAL-065` | Ganti payer | `paymentType = CASH` tetapi ada id kartu asuransi atau kartu perusahaan yang ikut dikirim | "Pembayaran tunai tidak boleh disertai kartu penjamin." | `400` |
| `BIL-VAL-066` | Ganti payer | `paymentType = INSURANCE` tetapi kartu asuransi tidak dikirim, atau justru kartu perusahaan yang dikirim | "Pilih kartu asuransi yang akan dipakai." | `400` |
| `BIL-VAL-067` | Ganti payer | `paymentType = COMPANY_GUARANTOR` tetapi kartu penjamin perusahaan tidak dikirim, atau justru kartu asuransi yang dikirim | "Pilih kartu penjamin perusahaan yang akan dipakai." | `400` |
| `BIL-VAL-068` | Ganti payer | Kartu yang dipilih bukan milik pasien pada kunjungan ini | "Kartu penjamin yang dipilih bukan milik pasien ini." | `422` |
| `BIL-VAL-069` | Ganti payer | Kartu yang dipilih sudah tidak aktif atau sudah ditandai terhapus | "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi." | `422` |
| `BIL-VAL-070` | Ganti payer | Tanggal layanan berada di luar masa berlaku kartu | "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi." | `422` |
| `BIL-VAL-071` | Ganti payer | Kartu belum dinyatakan layak dipakai | "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai." | `422` |
| `BIL-VAL-072` | Ganti payer | Perusahaan asuransi pada kartu sudah tidak aktif atau kontraknya sudah berakhir | "Kerja sama dengan perusahaan asuransi ini sudah berakhir pada tanggal pelayanan." | `422` |
| `BIL-VAL-073` | Ganti payer | Payer kandidat sama persis dengan payer yang sedang berlaku | "Penjamin yang dipilih sama dengan yang sedang dipakai. Tidak ada yang perlu diubah." | `422` |
| `BIL-VAL-074` | Ganti payer | Kunjungan tidak memiliki baris sumber pembayaran sama sekali | "Data penjamin kunjungan ini belum lengkap. Hubungi Registrasi sebelum mengubah tagihan." | `422` |

**Contoh `BIL-VAL-070`.** Kartu penjamin PT Sejahtera milik Ny. S berlaku 1 Januari 2026 sampai 31 Agustus 2026. Kunjungan yang sedang ditagih terjadi 11 September 2026. Kasir memilih kartu itu, lalu permintaannya ditolak dengan kode `422` dan pesan di atas. Tidak ada perubahan payer yang tersimpan, dan tagihan tetap memakai payer sebelumnya.

## Penanggung per baris biaya

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `BIL-VAL-075` | Penanggung item | Baris biaya yang dikirim bukan milik tagihan ini | "Ada baris biaya yang tidak terdaftar pada tagihan ini." | `422` |
| `BIL-VAL-076` | Penanggung item | Baris biaya berstatus dibatalkan | "Baris biaya yang sudah dibatalkan tidak dapat diubah penanggungnya." | `422` |
| `BIL-VAL-077` | Penanggung item | Penanggung `INSURANCE` dipilih padahal kunjungan tidak berpayer asuransi | "Kunjungan ini tidak memakai asuransi, sehingga baris biaya tidak dapat ditanggung asuransi." | `422` |
| `BIL-VAL-078` | Penanggung item | Penanggung `COMPANY_GUARANTOR` dipilih padahal kunjungan tidak berpenjamin perusahaan | "Kunjungan ini tidak memakai penjamin perusahaan, sehingga baris biaya tidak dapat ditanggung penjamin." | `422` |
| `BIL-VAL-079` | Penanggung item | Daftar penanggung yang dikirim kosong | "Tidak ada perubahan penanggung yang dikirim." | `400` |
| `BIL-VAL-080` | Penanggung item | Satu baris biaya muncul lebih dari sekali pada permintaan yang sama | "Ada baris biaya yang dikirim lebih dari satu kali." | `400` |

`BIL-VAL-077` dan `BIL-VAL-078` adalah **satu-satunya** gerbang pada penanggung item. Hasil perhitungan tanggungan **bukan** gerbang: baris yang menurut aturan tidak tertanggung tetap boleh ditandai `INSURANCE` atau `COMPANY_GUARANTOR`, dan hasilnya nol tertanggung (`MPY-DEC-004`).

## Disposisi penebusan obat

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `BIL-VAL-081` | Disposisi obat | Kunjungan berjenis rawat inap | "Penebusan obat tidak dapat diubah untuk kunjungan rawat inap." | `422` |
| `BIL-VAL-082` | Disposisi obat | Baris yang dikirim bukan item obat | "Hanya baris obat yang dapat diatur penebusannya." | `422` |
| `BIL-VAL-083` | Disposisi obat | Mode `PARTIAL_REDEEMED` tetapi daftar baris yang ditebus kosong | "Pilih baris obat yang ditebus, atau pilih Tidak Ditebus untuk seluruhnya." | `400` |
| `BIL-VAL-084` | Disposisi obat | Mode `ALL_REDEEMED` atau `NOT_REDEEMED` tetapi daftar baris tetap dikirim | "Daftar baris hanya dipakai pada penebusan sebagian." | `400` |
| `BIL-VAL-085` | Disposisi obat | Baris yang dikirim tidak termasuk baris obat yang layak diedit pada tagihan ini | "Ada baris obat yang tidak dapat diatur penebusannya pada tagihan ini." | `422` |
| `BIL-VAL-086` | Disposisi obat | Tagihan tidak memiliki satu pun baris obat yang layak | "Tagihan ini tidak memiliki item obat yang dapat diatur penebusannya." | `422` |

**Contoh `BIL-VAL-081`.** Kunjungan rawat inap Tn. B memiliki sembilan baris obat. Kasir membuka Edit Billing dan menekan Tidak Ditebus. Permintaan ditolak `422` dengan pesan di atas; kesembilan baris tetap masuk tagihan apa adanya. Pada layar, tombol itu memang sudah dinonaktifkan lebih dulu — penolakan server adalah lapis kedua, bukan satu-satunya penjaga.

## Master rute reimbursement perusahaan penjamin

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `BIL-VAL-087` | Rute reimbursement | `RouteType = SELF` tetapi perusahaan asuransi mitra ikut diisi | "Perusahaan yang menanggung sendiri tidak memerlukan asuransi mitra." | `400` |
| `BIL-VAL-088` | Rute reimbursement | `RouteType = INSURANCE_PROVIDER` tetapi perusahaan asuransi mitra kosong | "Pilih perusahaan asuransi mitra untuk rute ini." | `400` |
| `BIL-VAL-089` | Rute reimbursement | Perusahaan asuransi mitra yang dipilih sudah tidak aktif | "Perusahaan asuransi yang dipilih sudah tidak aktif." | `422` |
| `BIL-VAL-090` | Rute reimbursement | Menandai rute sebagai bawaan padahal perusahaan itu sudah punya rute bawaan aktif lain | "Perusahaan ini sudah memiliki rute bawaan. Nonaktifkan yang lama lebih dulu." | `422` |
| `BIL-VAL-091` | Rute reimbursement | Tanggal akhir masa berlaku lebih awal dari tanggal mulai | "Tanggal akhir masa berlaku tidak boleh mendahului tanggal mulai." | `400` |

## Master aturan tanggungan perusahaan penjamin

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `BIL-VAL-092` | Aturan tanggungan | Persentase tanggungan di luar rentang 0 sampai 100 | "Persentase tanggungan harus berada di antara 0 dan 100." | `400` |
| `BIL-VAL-093` | Aturan tanggungan | Kode aturan sudah dipakai aturan lain pada perusahaan yang sama | "Kode aturan ini sudah dipakai pada perusahaan penjamin tersebut." | `422` |
| `BIL-VAL-094` | Aturan tanggungan | Jenis item menuntut rujukan tertentu tetapi rujukannya kosong — misalnya jenis `Drug` tanpa obat yang dipilih | "Lengkapi item yang menjadi sasaran aturan ini." | `400` |
| `BIL-VAL-095` | Aturan tanggungan | Lebih dari satu rujukan item diisi sekaligus sehingga sasarannya ambigu | "Aturan hanya boleh menyasar satu jenis item." | `400` |
| `BIL-VAL-096` | Aturan tanggungan | Tanggal akhir masa berlaku lebih awal dari tanggal mulai | "Tanggal akhir masa berlaku tidak boleh mendahului tanggal mulai." | `400` |
| `BIL-VAL-097` | Aturan tanggungan | Menghapus aturan yang sedang dipakai versi perhitungan yang tersimpan | "Aturan ini sudah dipakai pada tagihan yang tersimpan. Nonaktifkan saja, jangan dihapus." | `422` |

**Contoh `BIL-VAL-092`.** Admin mengisi persentase tanggungan `120`. Permintaan ditolak `400`. Bila ia mengisi `80`, sistem menyimpan `CoveragePercent = 80` dan **menurunkan sendiri** `CoPaymentPercent = 20` — nilai yang dikirim klien untuk urun biaya diabaikan, mengikuti pola yang sudah berlaku pada aturan tanggungan asuransi.

## Batas isi pesan galat

Pesan galat pada rumpun ini **MUST NOT** memuat nomor polis, nomor kartu, nomor karyawan, maupun nama karyawan. Nama perusahaan penjamin dan nama perusahaan asuransi boleh disebut karena keduanya justru yang dibutuhkan pengguna untuk bertindak. Nominal dan tanggal masa berlaku boleh disebut dengan alasan yang sama.

Trace **`MPY-DEC-001`–`010`**, `MPY-DES-001`–`017`. Test mapping: `BIL-AT-081`–`BIL-AT-100`.

---

## Amendment 15 September 2026 — Revisi Petty Cash: pencairan langsung dan anggaran per periode

`last_changed_in: BIL-VALIDATION-1.0` · status **approved** · owner Finance Operations/Billing · `approved_by`: Product/Domain Owner (`PC-DEC-026`) · `approved_at`: 2026-09-15 · input: **`PC-DEC-016`–`PC-DEC-025`**; keputusan arsitektur `PC-DES-015`–`PC-DES-025`.

### Aturan lama yang berubah atau tidak berlaku lagi

| Aturan | Keadaan | Sebab |
| --- | --- | --- |
| `BIL-VAL-046` (uang tidak boleh keluar sebelum disetujui) | **Tidak berlaku** | Gerbang persetujuan dicabut (`PC-DEC-016`) |
| `BIL-VAL-047` (nominal melebihi sisa anggaran bebas saat persetujuan) | **Tidak berlaku** | Tidak ada peristiwa persetujuan lagi; penjaga saldo pindah seluruhnya ke `BIL-VAL-048` |
| `BIL-VAL-049` (alasan penolakan wajib) | **Tidak berlaku** | Tidak ada peristiwa penolakan lagi |
| `BIL-VAL-048` (saldo tidak mencukupi saat pencairan) | **Berlaku, cakupan diperluas** | Kini juga memeriksa keberadaan periode `ACTIVE`, bukan hanya nominal saldo |
| `BIL-VAL-050` (pembatalan hanya selagi belum diputuskan) | **Berlaku, syarat berubah** | Syaratnya kini "belum dicairkan", bukan "belum diputuskan" |
| `BIL-VAL-051` (nota hanya setelah uang keluar) | **Berlaku apa adanya** | Tidak tersentuh revisi ini |
| `BIL-VAL-054` (koreksi saldo tidak boleh negatif) | **Berlaku, disederhanakan** | Klausa "tidak di bawah komitmen berjalan" dihapus bersama `ReservedAmount` (`PC-DES-016`) |

### Aturan baru

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `BIL-VAL-098` | Kembalikan sisa uang | Nominal pengembalian kurang dari atau sama dengan nol, atau total seluruh pengembalian pada voucher itu melampaui nominal voucher | "Nominal pengembalian tidak boleh melebihi sisa uang yang masih ada di tangan penerima. Sisa saat ini Rp {outstanding}." | `422` |
| `BIL-VAL-099` | Balikkan pencairan | Voucher sudah pernah dibalik, atau alasan pembalikan kosong | "Pencairan ini sudah pernah dibatalkan, atau alasan pembatalan belum diisi." | `422` |
| `BIL-VAL-100` | Voucher berstatus `Dibatalkan (Uang Dikembalikan)` | Aksi apa pun dijalankan atasnya | "Pencairan ini sudah dibatalkan dan uangnya sudah kembali. Buat permintaan baru bila pengeluaran ini masih diperlukan." | `422` |
| `BIL-VAL-101` | Kembalikan sisa atau balikkan pencairan | Voucher masih berstatus `Menunggu Pencairan` | "Uang untuk permintaan ini belum diserahkan, jadi tidak ada yang bisa dikembalikan atau dibatalkan." | `422` |
| `BIL-VAL-102` | Buat periode anggaran | Tanggal mulai tumpang tindih dengan periode lain pada kolam yang sama, atau plafon kurang dari atau sama dengan nol, atau tanggal selesai lebih awal dari tanggal mulai | "Periode anggaran ini bertabrakan dengan periode yang sudah ada, atau tanggal dan plafonnya belum benar." | `422` |
| `BIL-VAL-103` | Aktifkan periode anggaran | Sudah ada periode berstatus `ACTIVE` pada kolam yang sama | "Masih ada periode anggaran yang aktif. Tutup periode itu lebih dulu sebelum mengaktifkan yang baru." | `422` |
| `BIL-VAL-104` | Tutup periode anggaran | Masih ada voucher `Menunggu Pencairan` pada periode itu, atau sisa saldo lebih besar dari nol tetapi periode penerus tidak disebutkan/tidak sah | "Periode ini belum bisa ditutup: masih ada permintaan yang belum dicairkan, atau periode penerus untuk sisa saldo belum dipilih." | `422` |
| `BIL-VAL-105` | Periode anggaran berstatus `CLOSED` | Penambahan saldo, koreksi, pencairan, pengembalian, atau pembalikan dijalankan pada periode itu | "Periode anggaran ini sudah ditutup dan tidak menerima pergerakan lagi." | `422` |
| `BIL-VAL-106` | Pencairan, pengembalian, pembalikan, penambahan saldo | Tidak ada satu pun periode anggaran berstatus `ACTIVE` | "Belum ada periode anggaran yang aktif. Finance perlu membuat dan mengaktifkan periode anggaran lebih dulu." | `422` |

> **Kenapa `BIL-VAL-106` berdiri sendiri, bukan digabung ke `BIL-VAL-048`.** Keduanya sama-sama menghalangi pencairan, tetapi yang harus dikerjakan petugas berbeda sama sekali. `BIL-VAL-048` berarti uangnya kurang — kasir menunggu Finance menambah saldo. `BIL-VAL-106` berarti belum ada wadah anggarannya sama sekali — Finance harus membuat periode lebih dulu. Menggabungkan keduanya menjadi satu pesan "saldo tidak mencukupi" akan mengirim kasir menunggu penambahan saldo yang tidak akan menyelesaikan apa pun.

### Contoh berangka

**Pengembalian sisa bertahap.** Voucher Rp 500.000 sudah dicairkan. Penerima mengembalikan Rp 50.000 — sah, `returnedAmount` menjadi Rp 50.000, sisa di tangan Rp 450.000. Ia mengembalikan Rp 60.000 lagi — sah, `returnedAmount` Rp 110.000. Ia mencoba mengembalikan Rp 400.000 — **ditolak** `BIL-VAL-098`, karena sisa yang masih di tangan hanya Rp 390.000.

**Pembalikan setelah pengembalian sebagian.** Voucher yang sama, `returnedAmount` Rp 110.000. Kasir membalik pencairannya: saldo bertambah Rp 390.000 (bukan Rp 500.000 — Rp 110.000 sudah kembali lebih dulu lewat baris `RETURN`). Total yang kembali ke kolam tetap Rp 500.000, dan ledger memperlihatkan keduanya secara terpisah.

---

## Amendment 18 September 2026 — Penutupan gap `FINAL`→`CLOSED`

`last_changed_in: BIL-VALIDATION-1.1` · status **approved** (`BKC-DEC-105`, 18 September 2026) · input: **`BKC-DEC-100`–`BKC-DEC-102`**; keputusan arsitektur `BKC-DES-028`–`BKC-DES-035`.

Amendment ini menambah **tiga** aturan dan sengaja menambah **nol** pesan galat baru bagi pengguna. Perpindahan status `FINAL`↔`CLOSED` adalah akibat, bukan perintah — tidak ada layar yang memintanya, sehingga tidak ada layar yang perlu diberi tahu bila ia tidak terjadi.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `BIL-VAL-107` | Perhitungan sisa tagihan pasien | Invoice tidak memiliki versi kalkulasi berjalan | "Invoice belum memiliki hasil perhitungan terkini." — **pesan yang sudah ada hari ini, MUST NOT berubah** walaupun perhitungannya dipindahkan ke service bersama (`BKC-DES-028`) | `422` |
| `BIL-VAL-108` | Penyelarasan status penutupan | Status invoice `OPEN` atau `SETTLED_BY_WRITE_OFF` | — (tidak ada pesan; penyelarasan berhenti tanpa menulis apa pun dan tanpa menggagalkan peristiwa pemicunya) | — |
| `BIL-VAL-109` | Perpindahan `FINAL`↔`CLOSED` | Pihak mana pun mencoba memindahkannya lewat endpoint atau layar | — (tidak ada endpointnya sama sekali; permintaan semacam itu berakhir `404` pada lapis routing) | `404` |

### Kenapa tidak ada pesan galat baru

Bila perhitungan sisa tagihan gagal di tengah sebuah pembayaran, yang gagal adalah **pembayarannya** — seluruh transaksi dibatalkan dan kasir menerima pesan galat milik pembayaran itu, bukan pesan baru tentang status invoice. Menambahkan pesan tersendiri hanya akan memberi tahu kasir tentang mekanisme internal yang tidak dapat ia perbaiki.

Sebaliknya, bila penyelarasan berhenti karena statusnya memang bukan urusannya (`BIL-VAL-108`), itu **bukan** kegagalan: peristiwa pemicunya tetap berhasil dan tetap tersimpan.

### Contoh berangka

**Tagihan lunas menjadi tertutup.** Tagihan Ny. Sari Rp 1.500.000 berstatus `FINAL`. Kasir menerima pembayaran tunai Rp 1.500.000. Pada transaksi yang sama, sisa tagihan terhitung Rp 0, status berpindah ke `CLOSED`, dan `closedAt` diisi waktu pembayaran itu. Kasir tidak melihat pesan tambahan apa pun — ia hanya melihat pembayarannya berhasil.

**Tagihan tertutup yang terbuka kembali.** Tagihan yang sama, tiga hari kemudian, pembayarannya dibalik karena kesalahan mesin EDC. Sisa tagihan terhitung Rp 1.500.000 lagi, status kembali ke `FINAL`, `closedAt` dikosongkan. Tagihan itu muncul lagi pada daftar tagihan yang masih punya sisa — yang memang seharusnya terjadi, karena uangnya memang tidak jadi diterima.

Trace **`BKC-DEC-100`–`102`**, `BKC-DES-028`–`035`. Tests `BIL-AT-121`–`BIL-AT-134`.

---

## Amendment 21 September 2026 — Aturan penerbitan fakta ke modul konsumen

`last_changed_in: BIL-VALIDATION-1.2` · status **draft** · input `BKC-DEC-106`–`109`, `BKC-DES-036`–`041`.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Satu tender satu surat per keadaan | Penerbitan surat penerimaan | Sudah ada surat untuk pasangan tender dan status yang sama | *Tidak tampil ke pengguna* — penerbitan kedua diabaikan tanpa membuat baris baru | `BIL-VAL-110` |
| Shift kasir wajib untuk tunai | Penerbitan surat penerimaan | Tender memakai cara bayar tunai tetapi tidak membawa identitas shift | Pembayaran tunai tidak dapat diteruskan ke pembukuan karena shift kasirnya tidak diketahui. Tutup dan buka kembali shift, lalu ulangi | `BIL-VAL-111` |
| Nomor versi clearance wajib naik | Penerbitan surat clearance | Nomor versi yang hendak dipakai sudah pernah terbit untuk resep itu | *Tidak tampil ke pengguna* — transaksi diulang dengan nomor berikutnya | `BIL-VAL-112` |
| Sebab wajib sesuai arah | Penerbitan surat clearance | Sebab bertanda pencabutan dipakai untuk menyatakan resep menjadi boleh diambil, atau sebaliknya | *Kesalahan internal* — penerbitan dibatalkan beserta transaksinya | `BIL-VAL-113` |
| Hasil finansial wajib ada saat menyatakan boleh diambil | Penerbitan surat clearance | Keadaan `CLEARED` tanpa hasil finansial | *Kesalahan internal* — penerbitan dibatalkan | `BIL-VAL-114` |
| Biaya bukan obat tidak mencabut clearance | Penerbitan surat clearance | Tagihan kembali bersisa semata karena biaya tindakan, laboratorium, radiologi, atau kamar | *Tidak ada surat yang terbit* — ini perilaku yang benar, bukan penolakan | `BIL-VAL-115` |
| Pengakuan hanya sekali | Pengakuan penerimaan surat | Surat sudah berstatus diakui | Surat ini sudah diakui sebelumnya. Tidak ada yang perlu dilakukan lagi | `BIL-VAL-116` |
| Resep tidak dikenal bukan berarti lunas | Pembacaan keadaan clearance | Resep yang ditanyakan belum pernah punya surat | Keadaan pembayaran resep ini belum diketahui. Obat belum boleh diserahkan | `BIL-VAL-117` |

### Dua aturan yang paling mudah salah dipahami

**`BIL-VAL-115` bukan penolakan.** Ketiadaan surat pada kasus itu adalah hasil yang benar.
Contoh: pasien lunas pukul 09.00, resepnya boleh dikerjakan. Pukul 09.30 kasir mencatat biaya
tindakan yang terlewat, tagihan kembali bersisa Rp 350.000. Tidak ada surat pencabutan yang
terbit, dan apoteker tetap boleh menyerahkan obat yang sudah dibayar. Pasien punya kewajiban
baru atas tindakan itu — bukan atas obatnya.

**`BIL-VAL-117` fail-closed.** Resep yang tidak dikenal **MUST NOT** diperlakukan sebagai lunas,
dan **MUST NOT** melempar galat teknis yang membuat layar Farmasi gagal dimuat. Ia menjawab
dengan keadaan "belum diketahui", yang menurut `PHA-DEC-067` sama sekali bukan izin menyerahkan
obat.

Trace `BKC-DEC-106`–`109`, `PHA-DEC-067`, `PHA-DEC-068`. Tests `BIL-AT-135`–`BIL-AT-142`.

---

## Amendment 24 September 2026 — Aturan Validasi Integrasi Rawat Inap & Financial Clearance

`last_changed_in: BIL-VALIDATION-1.3` · status **draft** · input `BKC-DEC-112`–`119`, `BKC-AC-080`–`087`, `BKC-DES-042`–`050`.

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna |
| --- | --- | --- | --- | --- |
| `BIL-VAL-118` | Tarif jam masuk hari pertama bertingkat | Perhitungan sewa kamar | Masuk `< 18:00` (100%), `18:00 - < 22:00` (50%), `22:00 - < 00:00` (20%), `00:00` hari baru (0%) | *Kalkulasi otomatis* — rincian sewa kamar hari pertama disesuaikan dengan jam masuk pasien |
| `BIL-VAL-119` | Pro-rata sewa kamar transfer multipel | Perhitungan sewa kamar | Pasien pindah kamar lebih dari 1 kali dalam hari kalender yang sama | *Kalkulasi otomatis* — tarif sewa kamar dibagi proporsional berdasarkan durasi menit riil tiap kamar |
| `BIL-VAL-120` | Pagu biaya administrasi rawat inap Rp6.000.000 | Perhitungan biaya administrasi | 7% dari tagihan memenuhi syarat melebihi Rp6.000.000 | Biaya administrasi rawat inap dikenakan maksimal sebesar pagu Rp6.000.000 |
| `BIL-VAL-121` | Verifikasi deposit 100% tindakan besar atas ekses | Validasi order tindakan / operasi besar | Saldo deposit pasien `<` porsi tanggung jawab pasien (*patient responsibility*) | Saldo deposit belum memenuhi 100% porsi tanggung jawab pasien untuk tindakan besar. Pasien/keluarga wajib menyetor kekurangan deposit |
| `BIL-VAL-122` | Izin pulang finansial hanya untuk tagihan lunas | Evaluasi Financial Clearance | Sisa tagihan pasien (`PatientOutstanding`) `> 0` | Pasien belum dapat diberikan izin pulang finansial karena masih memiliki sisa tagihan yang belum diselesaikan |
| `BIL-VAL-123` | Pencabutan izin pulang saat tagihan susulan (Auto-Reblock) | Intake tagihan baru pasca-clearance | Tagihan baru masuk pada invoice rawat inap berstatus `CLEARED` | Status izin pulang dicabut otomatis karena ada tagihan pelayanan susulan. Pasien wajib menyelesaikan selisih tagihan di kasir |
| `BIL-VAL-124` | Integritas rincian alihan IGD | Konsolidasi tagihan rawat inap | Invoice rawat inap memuat rincian tindakan/obat gawat darurat | *Invariant sistem* — rincian biaya IGD tetap berstatus `EMERGENCY` dan tidak boleh ditimpa menjadi rawat inap |
| `BIL-VAL-125` | Koreksi penempatan kamar idempoten | Penerimaan event `ROOM_CORRECTION` | Data bed/kamar dikoreksi oleh bangsal | Tagihan kamar sebelumnya dibatalkan otomatis dan tagihan baru dihitung ulang berdasarkan kamar yang benar |
| `BIL-VAL-126` | Pembatalan atau pengalihan biaya admin rajal | Alihan pasien rawat jalan ke rawat inap | Pasien memiliki tagihan admin rajal pada encounter rujukan | Biaya administrasi rawat jalan dibatalkan dan digantikan biaya administrasi rawat inap; pembayaran yang sudah masuk dialihkan sebagai kredit tagihan |

### Contoh Kasus Validasi Nyata di Rumah Sakit

**Contoh 1: Pasien Masuk Malam Hari (`BIL-VAL-118`).** Pasien Tn. Budi masuk kamar Kelas 1 (tarif Rp 1.000.000/hari) pada pukul 22.30 WIB. Sistem secara otomatis mengenakan tarif 20% untuk hari pertama tersebut, yaitu Rp 200.000. Jika Tn. Budi baru masuk pada pukul 00.15 WIB keesokan harinya, hari sebelumnya tidak dikenakan biaya sama sekali (0%).

**Contoh 2: Pindah Kamar Dua Kali dalam Sehari (`BIL-VAL-119`).** Pasien Ny. Siti pada tanggal 10 Oktober menempati Kamar Standar (Rp 600.000/hari) selama 360 menit (6 jam), kemudian dipindahkan ke ICU (Rp 2.400.000/hari) selama 1080 menit (18 jam). Total durasi 1440 menit (24 jam). Biaya kamar tanggal 10 Oktober dihitung pro-rata: `(360/1440 * Rp 600.000) + (1080/1440 * Rp 2.400.000) = Rp 150.000 + Rp 1.800.000 = Rp 1.950.000`.

**Contoh 3: Deposit Tindakan Operasi Jaminan Asuransi (`BIL-VAL-121`).** Pasien anak memerlukan operasi besar dengan estimasi biaya Rp 50.000.000. Asuransi menjamin 80% (Rp 40.000.000), sehingga porsi tanggung jawab pasien adalah 20% (Rp 10.000.000). Kasir hanya mewajibkan setoran deposit sebesar Rp 10.000.000 (bukan Rp 50.000.000). Jika deposit pasien baru Rp 4.000.000, sistem menolak verifikasi izin tindakan besar dengan pesan `BIL-VAL-121` kekurangan Rp 6.000.000.

Trace `BKC-DEC-112`–`119`, `BKC-AC-080`–`087`, `BKC-DES-042`–`050`. Tests `BIL-AT-143`–`BIL-AT-152`.



# Amendment 24 September 2026 — Revisi UI Billing (Revisi 1.6, `BIL-VALIDATION-1.4`)

Status: `draft`. Basis: `00-interview-decisions.md` `BUI-DEC-001`–`015`.

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Lapis |
|---|---|---|---|---|---|
| `BUI-VAL-01` | Memo Dokter TTD wajib sebelum submit | Form Apply Discount (`BUI-DES-009`) | `DoctorDiscountMemoFile` kosong/null | "Memo Dokter TTD wajib diunggah sebelum pengajuan diskon dapat dikirim." | Frontend saja — backend TIDAK menegakkan `[Required]` (`CAP-BUI-10`). Dicatat sebagai risiko residual, bukan diperbaiki amendment ini |
| `BUI-VAL-02` | Sumber refund wajib dipilih | Modal Ajukan Refund (`BUI-DES-011`) | `RefundCategory` belum dipilih (belum ada radio tersorot) | "Pilih sumber refund: Billing atau Deposito." | Frontend |
| `BUI-VAL-03` | Refund sumber Billing wajib sekurang-kurangnya satu item tercentang | Modal Ajukan Refund | `RefundCategory === "BILLING"` dan `SelectedBillingItemIds` kosong | "Pilih sekurang-kurangnya satu item yang akan direfund." | Frontend |
| `BUI-VAL-04` | Refund sumber Deposito tidak boleh melebihi sisa deposito | Modal Ajukan Refund | `RequestedAmount > RemainingDepositAmount` | "Nominal refund tidak boleh melebihi sisa deposito pasien." | Frontend (tampilan nominal terkunci ke sisa deposito) **dan** backend — `CreateRefundRequest.RequestedAmount` divalidasi service (existing, tidak berubah amendment ini) |
| `BUI-VAL-05` | Nominal refund harus positif | Modal Ajukan Refund | `RequestedAmount <= 0` | Sudah ditegakkan `[Range("0.01", ...)]` pada `CreateRefundRequest` — existing, tidak berubah | Backend (existing) |
| `BUI-VAL-06` | Filter tanggal Billing: tanggal akhir tidak boleh sebelum tanggal awal | Layar Daftar Billing (`BUI-DES-005`) | `EndDate < StartDate` | "Tanggal akhir tidak boleh sebelum tanggal awal." | Frontend |
| `BUI-VAL-07` | Default status tagihan mengikuti coverage seluruh item | Edit Status Tagihan / Payment Method (`BUI-DES-001`, `003`) | Backend: seluruh item aktif `isCovered == true` → `"INSURANCE"`; selain itu → `"CASH"` | Tidak ada pesan pengguna — ini nilai saran, bukan penolakan | Backend (`BUI-DES-001`) |

Trace `BUI-DEC-001`–`015`, `BUI-DES-001`–`012`. Tests: lihat
`testing/acceptance-test-matrix.md` amendment revisi 1.6.
