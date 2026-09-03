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
