# CRM Membership — Interview Decisions

| Field | Nilai |
|---|---|
| Blueprint ID | `crm-membership` |
| Revision | `1` |
| Status | `draft` |
| Mode | Amendment pass (pass 3) |
| Product/domain owner | Yoga Aji Pratama |
| Backend SHA | `36422a4d` (branch `yoga`), diaudit di `01-existing-capability-map.md`; impact scan `1aecf642` |
| Frontend SHA | `9aba9929d`, diaudit di `01-existing-capability-map.md`; impact scan `f1e5e273a` |
| Sumber masukan | PDF "Pendaftaran membership pasien prioritas" (12 Okt 2026, 10:47) dan PDF "Pendaftaran pasien prioritas IGD" (12 Okt 2026, 14:19) |

> **Catatan audit:** scope dikunci sebelum capability map ada. Audit menyusul pada 2026-10-13
> (`01-existing-capability-map.md`) dan mengoreksi `CRM-FACT-005`.

## Scope dan Outcome

**Batas scope (satu kalimat):** modul CRM Membership memegang data keanggotaan pasien prioritas
beserta tingkat, masa berlaku, status, dan aturan manfaat per tingkat. Modul lain membaca
status itu untuk memberi perlakuan prioritas.

**Tujuan bisnis:** rumah sakit dapat mendaftarkan pasien sebagai member (Silver, Gold,
Platinum, Executive), melihat daftar member, lalu setiap titik layanan (pendaftaran RJ, IGD,
antrean, kasir) dapat mengenali pasien itu sebagai pasien prioritas.

### Di dalam scope

1. Menu **Pendaftaran Membership**: formulir pendaftaran pasien membership/pasien prioritas.
2. Menu **Daftar Membership**: daftar pasien prioritas yang berhasil terdaftar.
3. Tingkat keanggotaan dan masa berlakunya.
4. Status keanggotaan beserta perubahannya (aktif, kedaluwarsa, dan seterusnya).
5. Aturan manfaat per tingkat: antrean prioritas, hak reservasi tanpa poli/dokter, dan diskon. Wallet ditunda ke rilis berikutnya (`CRM-DEC-009`).
6. Layanan pengecekan "apakah pasien ini member aktif dan tingkat apa" berdasarkan No. RM, No. Telp, atau NIK.

### Di luar scope — untuk modul lain (hanya titik sentuh)

| Kebutuhan dari PDF | Modul pemilik | Titik sentuh dengan CRM |
|---|---|---|
| Reservasi RJ via WA/telp, disimpan tanpa poli/dokter, link konfirmasi kedatangan, screening dan admisi menghampiri pasien | `rawat-jalan` | Membaca status dan hak manfaat member |
| Nomor antrean prioritas kelipatan 5 dan disembunyikan dari display | `rawat-jalan` / `kiosk` (antrean) | Membaca flag antrean prioritas per tingkat |
| Reservasi prioritas IGD, order ambulans, pendampingan petugas khusus, penanganan sesuai triage | `igd` | Membaca status member |
| Prioritas mendapatkan kamar rawat inap | `rawat-inap` | Membaca status member |
| Tagihan dikirim via WA/email, cara bayar (self-pay, asuransi cashless/reimbursement, corporate guarantee, membership discount, prepaid/wallet), billing final closing dan Lunas | `billing-kasir` | Diskon per tingkat dan saldo wallet |
| Pengiriman pesan WhatsApp | Platform/notifikasi (belum diketahui pemiliknya) | — |

## Fakta source (penelusuran singkat)

| ID | Fakta | Bukti |
|---|---|---|
| `CRM-FACT-001` | Master tingkat keanggotaan sudah ada (`MstMembershipTier`) dengan grup Swagger `Administrator / Master Data / Membership Tier`, base URL `api/v1/administrator/master-data/membership-tiers` | `Areas/Administrator/MasterData/Models/MstMembershipTier.cs`, `Controllers/MembershipTierController.cs:26,36` @ `36422a4d` |
| `CRM-FACT-002` | Master tingkat sudah punya kolom `PriorityLevel`, `PriorityQueue`, `IsSelectableInAdmission`, `IsSelectableInKiosk`, `IsManagedByMarketingOnly`, diskon per kelompok layanan (pendaftaran, konsultasi, tindakan, lab, radiologi, farmasi), `ValidityMonths` (bawaan 12), `MinimumSpendAmount` | `MstMembershipTier.cs:32-62` |
| `CRM-FACT-003` | Enum `MembershipTierType` memuat Regular, Silver, Gold, Platinum, Executive, **Corporate, Family, Other** (PDF hanya menyebut lima yang pertama) | `Enums/MembershipTierType.cs` |
| `CRM-FACT-004` | Enum `MembershipStatus` memuat Active, Expired, Suspended, Cancelled, Upgraded, Downgraded | `Enums/MembershipStatus.cs` |
| `CRM-FACT-005` | ~~Belum ditemukan data keanggotaan per pasien maupun wallet member.~~ **`superseded` oleh `CRM-FACT-007`**: penelusuran awal keliru | grep `Membership` di `Areas/` dan `Models/` |
| `CRM-FACT-007` | Data membership per pasien **sudah ada** (`MstPatientMembership`, PatientManagement) beserta halaman FE patient-memberships, tetapi aturannya bertentangan dengan keputusan (`CRM-CONF-003` s.d. `CRM-CONF-008` di capability map). Wallet memang belum ada | `01-existing-capability-map.md` `CRM-CAP-002` s.d. `CRM-CAP-007` |
| `CRM-FACT-006` | Metode bayar sudah punya penanda `IsMembership` yang dibaca saat pelunasan | `BillingSettlementService.cs:1185` |

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `CRM-DEC-001` | Decision | Scope: modul baru `crm-membership` memegang keanggotaan dan aturan manfaat per tingkat. Alur RJ, IGD, rawat inap, dan pembayaran hanya dicatat sebagai titik sentuh, lalu diteruskan sebagai amendment ke blueprint pemiliknya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-002` | Decision | Owner keputusan modul ini: Yoga Aji Pratama. Jawaban di sesi wawancara dicatat `approved` atas namanya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-003` | Decision | Membership **wajib** menempel pada satu pasien yang sudah tercatat di master pasien (punya No. RM). Calon member yang belum punya RM didaftarkan sebagai pasien baru lebih dulu. Contoh: Ibu Sari (nama samaran) belum pernah berobat. Petugas membuat RM baru untuknya, baru kemudian mendaftarkan membership Gold | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara; PDF poin 2C |
| `CRM-DEC-004` | Decision | Membership berbayar. Setelah formulir disimpan, statusnya **Menunggu Pembayaran**, dan baru **Aktif** setelah kasir mencatat iuran lunas. Sebelum aktif, pasien terbaca sebagai Reguler dan tidak mendapat manfaat apa pun. Contoh: pendaftaran Gold disimpan 13 Okt pukul 09.00, iuran dibayar pukul 10.15. Kunjungan pukul 09.30 masih Reguler, kunjungan pukul 10.20 sudah Gold. Titik sentuh: tagihan iuran diproses `billing-kasir` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-005` | Decision | Saat masa berlaku habis tanpa perpanjangan, status otomatis menjadi **Kedaluwarsa** dan pasien terbaca Reguler di semua titik layanan. Tidak ada masa tenggang dan tidak ada perpanjangan otomatis. Kunjungan yang sudah terdaftar sebelum tanggal habis tetap memakai tingkat saat didaftarkan. Perpanjangan sesudahnya membuat periode baru. Contoh: Gold berlaku 14 Okt 2025 s.d. 13 Okt 2026. Reservasi yang terdaftar 13 Okt 2026 dan dilayani 14 Okt tetap Gold, sedangkan pendaftaran baru 14 Okt 2026 sudah Reguler | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-006` | Decision | Tingkat yang dipakai mengikuti PDF. **Reguler** berarti bukan member dan tidak didaftarkan lewat CRM. **Silver, Gold, Platinum, Executive** adalah tingkat berbayar. Corporate, Family, dan Other yang sudah ada di enum source tidak dipakai pada rilis pertama, sehingga member keluarga dan member perusahaan belum didukung | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara; menutup `CRM-CONF-002` |
| `CRM-DEC-007` | Decision (invariant keselamatan) | Status membership **tidak pernah** mengubah urutan penanganan medis. Di IGD, urutan hanya ditentukan level triage (PDF IGD poin 5). Membership hanya memberi layanan non-klinis: reservasi, pendampingan petugas khusus, dan hak prioritas kamar yang diatur `rawat-inap`. Contoh: pasien Platinum dengan triage Hijau tetap ditangani setelah pasien Reguler dengan triage Merah | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara; diteruskan ke `igd` dan `rawat-inap` sebagai batasan titik sentuh |
| `CRM-DEC-008` | Decision | Diskon membership adalah **potongan harga** yang diterapkan sebelum tagihan dibagi ke penjamin, bukan cara bayar. Ini selaras dengan keputusan RJ yang sudah terkunci. "Membership Discount" pada daftar cara bayar di PDF dibaca sebagai manfaat. Contoh: tagihan Rp 1.000.000, diskon Gold 10%, tagihan bersih Rp 900.000, lalu dibayar tunai, asuransi, atau wallet | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara; menutup `CRM-CONF-001`; `rawat-jalan/00-interview-decisions.md:201,216` |
| `CRM-DEC-009` | Decision | **Wallet / Membership Prepaid ditunda** ke rilis berikutnya. Rilis pertama hanya memuat keanggotaan, tingkat, status, dan diskon. Alasannya, saldo wallet adalah uang titipan pasien yang menjadi kewajiban rumah sakit, sehingga aturan top-up, refund, kedaluwarsa saldo, dan jurnalnya perlu diputuskan bersama Finance/Accounting | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-010` | Decision | Pengecekan status member lewat No. Telp **menampilkan semua pasien** yang memakai nomor itu (nama, tanggal lahir, No. RM, status member), lalu admisi memilih satu. Status prioritas tidak boleh menempel otomatis. Pencarian lewat No. RM atau NIK menghasilkan paling banyak satu pasien. Contoh: nomor 0812-xxxx dipakai Ibu Sari (Gold) dan anaknya Dika (Reguler). Keduanya tampil, dan admisi memilih Dika, sehingga pendaftaran berjalan sebagai Reguler | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara; PDF poin 2C |
| `CRM-DEC-011` | Decision | Hanya **petugas CRM/Marketing** yang boleh mendaftarkan, mengubah tingkat, dan membatalkan membership. Admisi RJ/IGD hanya dapat melihat status member. Selaras dengan penanda `IsManagedByMarketingOnly` pada master tier (`CRM-FACT-002`). Pasien yang ingin mendaftar di loket admisi diarahkan ke CRM. Hak akses mengikuti pola izin per departemen-posisi yang berlaku | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-012` | Decision | **Satu pasien hanya punya satu membership aktif.** Naik tingkat di tengah masa berlaku membuat periode baru 12 bulan dengan iuran tingkat baru secara penuh. Membership lama ditutup dengan status `Upgraded` tepat saat iuran baru lunas (mengikuti `CRM-DEC-004`), dan sisa masa lama hangus tanpa kompensasi. Contoh: Silver berlaku 1 Mei 2026 s.d. 30 Apr 2027. Upgrade ke Gold lunas 13 Okt 2026, sehingga Silver ditutup 13 Okt 2026 dan Gold berlaku 13 Okt 2026 s.d. 12 Okt 2027 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-013` | Decision | Pembatalan membership aktif wajib disertai alasan, dan riwayatnya tidak dihapus. (a) **Atas permintaan pasien:** status `Cancelled`, tanpa pengembalian iuran. (b) **Karena salah input petugas** dan belum ada manfaat yang terpakai: status `Cancelled` dengan iuran dikembalikan penuh lewat proses refund kasir (titik sentuh `billing-kasir`). Contoh: petugas salah memilih Platinum untuk pasien yang membeli Gold. Belum ada kunjungan yang memakai diskon, sehingga iuran Platinum dikembalikan dan Gold didaftarkan ulang. Bila diskon sudah terpakai, jalur (b) tidak berlaku | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-014` | Decision | Pendaftaran berstatus Menunggu Pembayaran yang iurannya tidak lunas dalam **7 hari kalender** sejak disimpan otomatis menjadi batal dengan alasan "tidak dibayar", dan tagihan iurannya ikut dibatalkan. Pasien boleh mendaftar ulang. Contoh: disimpan 13 Okt 2026 pukul 09.00 dan belum dibayar sampai 20 Okt 2026 pukul 09.00, sehingga otomatis batal. Pembayaran yang masuk sesudahnya ditolak dan pasien diminta daftar ulang | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara |
| `CRM-DEC-015` | Decision | Manfaat non-diskon **sama untuk semua tingkat berbayar** (Silver, Gold, Platinum, Executive): reservasi tanpa poli/dokter, link konfirmasi kedatangan, antrean prioritas, dan ruang tunggu prioritas/VIP. Yang membedakan tingkat hanya persen diskon per kelompok layanan di master tier. Contoh: pasien Silver dan Executive sama-sama boleh reservasi tanpa memilih poli, tetapi diskon konsultasinya bisa berbeda (misalnya 5% dan 20%) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara; PDF poin 2E, 2G |
| `CRM-DEC-016` | Decision | Diskon member hanya diterapkan bila **kunjungan dibayar pribadi sepenuhnya** (penjamin utama = pribadi). Bila kunjungan memakai asuransi (cashless/reimbursement) atau corporate guarantee, diskon member **tidak diterapkan sama sekali**, termasuk pada sisa yang dibayar pasien. Tetap selaras dengan `CRM-DEC-008` dan aturan RJ (diskon sebelum pembagian penjamin). Contoh: kunjungan pribadi Rp 1.000.000 dengan diskon Gold 10% menjadi Rp 900.000. Kunjungan asuransi Rp 1.000.000 yang ditanggung Rp 800.000 tidak didiskon, sehingga sisa Rp 200.000 dibayar penuh | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Jawaban wawancara (diklarifikasi dari jawaban awal "hanya self-pay") |
| `CRM-DEC-017` | Decision | Data dan aturan membership **tetap di PatientManagement**: `MstPatientMembership` diperluas dan diperbaiki sesuai `CRM-DEC-003` s.d. `CRM-DEC-016`, memakai prefix `Pat`/`Mst` yang sudah `ACTIVE` sehingga tidak perlu registrasi prefix baru. "CRM" adalah bagian menu di frontend yang membuka halaman membership. Tidak ada tabel membership kedua. Modul CRM yang lebih luas (kampanye, poin) diputuskan terpisah bila kelak dibutuhkan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Closure pass; menutup `CRM-OQ-007`; `01-existing-capability-map.md` `CRM-CAP-002`, `CRM-CAP-013` |
| `CRM-DEC-018` | Decision | Akun kiosk **tidak boleh membuat** membership. Rute `POST /` dan `POST /kiosk` yang hanya dijaga policy `KioskRead` ditutup untuk pembuatan. Kiosk tetap boleh membaca (pilihan membership, pencarian nomor member). Pembuatan hanya lewat rute ber-izin `PatientMembership : Create` yang diberikan ke petugas CRM. Frontend halaman membership dialihkan ke rute `/admin` agar petugas Marketing tanpa role Administrator tidak ditolak (403). Dampak ke kiosk: frontend kiosk hanya memakai `GET .../patient-memberships/options` (`kiosk-old-patient-registration.service.js:19` @`9aba9929d`), sehingga tidak ada alur kiosk yang putus | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Closure pass; menutup `CRM-OQ-008` dan `CRM-CONF-007` |
| `CRM-DEC-019` | Decision (kebutuhan CRM ke billing) | Iuran membership ditagih lewat **transaksi non-kunjungan** di billing-kasir yang berdiri sendiri dan terhubung ke tepat satu membership. Saat tagihan lunas, billing memberi tahu sistem dan membership berpindah dari Menunggu Pembayaran ke Aktif. Pembatalan salah input memakai refund kasir (`CRM-DEC-013`). Batal otomatis 7 hari ikut membatalkan tagihan iuran (`CRM-DEC-014`). Tidak boleh memakai kunjungan semu, dan tidak boleh berupa bukti bayar manual. Contoh: Gold Rp 2.400.000 (angka samaran) disimpan CRM, lalu kasir menerima pembayaran tanpa memilih kunjungan, dan begitu lunas membership Ibu Sari aktif | Yoga Aji Pratama | `approved` (sisi CRM) | Yoga Aji Pratama, 2026-10-13 | Closure pass; `CRM-CAP-008`. Bentuk teknis di billing butuh amendment dan persetujuan owner `billing-kasir` |
| `CRM-DEC-020` | Decision | Amendment billing untuk tagihan iuran (`CRM-DEC-019`) disetujui oleh **owner billing-kasir secara terpisah**, bukan oleh owner CRM. Permintaannya ada di `approval-requests/bkc-amendment-tagihan-iuran-membership.md`. Desain CRM boleh berjalan. Hanya bagian aktivasi-setelah-lunas dan refund iuran yang bergantung pada persetujuan itu | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Closure pass |
| `CRM-DEC-021` | Decision | Membership yang dibuat dengan aturan lama dan tidak punya bukti bayar iuran ditandai **Perlu Verifikasi** saat aturan baru dipasang. Selama itu pasien terbaca Reguler. Petugas CRM memverifikasi satu per satu: **diakui** (diberi tanggal mulai dan berakhir yang sah, lalu Aktif) atau **dibatalkan** dengan alasan. Jumlah data baru diketahui lewat pengecekan DB dengan izin terpisah. Contoh: membership Gold lama milik pasien X tanpa tagihan iuran menjadi Perlu Verifikasi. CRM mengonfirmasi pembayaran lewat arsip lama, lalu mengakuinya dengan masa berlaku 1 Mar 2026 s.d. 28 Feb 2027 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Closure pass; menutup `CRM-OQ-009` |
| `CRM-DEC-022` | Decision | Setelah membership berstatus Aktif, **pasien, tingkat, dan tanggal berlaku tidak dapat diubah**. Yang boleh diubah CRM hanya catatan. Salah pasien atau salah tingkat diselesaikan lewat pembatalan salah input (`CRM-DEC-013`) lalu pendaftaran ulang. Perubahan tingkat yang sah lewat upgrade (`CRM-DEC-012`). Contoh: membership Gold tercatat atas nama Dika padahal pembelinya Ibu Sari. CRM tidak boleh mengganti pemiliknya. CRM membatalkan sebagai salah input (refund bila diskon belum terpakai), lalu mendaftarkan Gold untuk Ibu Sari | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Closure pass; menutup `CRM-CONF-008` bersama `CRM-DEC-013` |
| `CRM-DEC-023` | Decision | Nomor member **dibuat otomatis** oleh sistem saat pendaftaran disimpan, unik, dan tidak diketik petugas. Setiap pendaftaran, termasuk upgrade dan perpanjangan, mendapat nomor baru. Format final ditentukan saat desain, contoh ilustrasi `MBR-2610-000123` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Closure pass |
| `CRM-DEC-024` | Decision | **Perpanjangan sebelum habis disambung** setelah periode lama. Pendaftaran perpanjangan mengikuti alur biasa (Menunggu Pembayaran, batal otomatis 7 hari). Setelah iuran lunas, statusnya **Terjadwal** dan otomatis menjadi Aktif pada hari setelah periode lama berakhir. Perpanjangan paling cepat dibuka **30 hari** sebelum tanggal berakhir, dan angka 30 dapat diubah sebagai pengaturan. Selama masa sambungan, pasien boleh punya satu baris Aktif dan satu baris Terjadwal, tetapi tetap hanya satu yang Aktif (melengkapi `CRM-DEC-012`). Contoh: Gold berlaku s.d. 12 Okt 2027. Perpanjangan dibuka 12 Sep 2027, dan iuran lunas 1 Okt 2027, sehingga periode baru Terjadwal 13 Okt 2027 s.d. 12 Okt 2028 dan menjadi Aktif otomatis pada 13 Okt 2027 pukul 00.00 WIB. Perpanjangan pada 1 Sep 2027 ditolak karena belum masuk jendela 30 hari | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Amendment pass; menutup `DEC-CRM-004` |
| `CRM-DEC-025` | Decision | Bila member yang punya periode **Terjadwal** melakukan upgrade atau berhenti sebelum periode itu mulai, periode Terjadwal **dibatalkan dengan refund iuran penuh** lewat kasir, karena belum ada hari maupun manfaat yang terpakai. Periode Aktif mengikuti aturan biasa: upgrade sesuai `CRM-DEC-012`, berhenti tanpa refund sesuai `CRM-DEC-013`. Contoh: Gold Aktif s.d. 12 Okt 2027 dan Gold Terjadwal 13 Okt 2027 s.d. 12 Okt 2028 (iuran Rp 2.400.000, angka samaran). Pada 5 Okt 2027 pasien upgrade ke Platinum dan iurannya lunas. Akibatnya Gold Aktif menjadi `Upgraded`, Gold Terjadwal dibatalkan dan Rp 2.400.000 dikembalikan, lalu Platinum Aktif 5 Okt 2027 s.d. 4 Okt 2028 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Amendment pass |
| `CRM-DEC-026` | Decision | Bila penjamin kunjungan berubah sebelum tagihan final, diskon member **dihitung ulang otomatis mengikuti penjamin pada saat tagihan difinalkan**. Pribadi sepenuhnya mendapat diskon, sedangkan kunjungan dengan asuransi atau perusahaan tidak mendapat diskon (`CRM-DEC-016`). Setiap pencabutan atau pemberian diskon tercatat sebagai riwayat (siapa, kapan, sebab). Setelah final, perubahan penjamin mengikuti aturan koreksi billing yang berlaku. Contoh: konsultasi Rp 500.000 untuk member Gold (diskon 10%). Saat pribadi, tagihan Rp 450.000. Kasir menambahkan asuransi sebelum final, sehingga diskon dicabut dan tagihan ke asuransi Rp 500.000. Kebalikannya, asuransi dilepas sebelum final, sehingga diskon diterapkan lagi dan tagihan Rp 450.000 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Amendment pass; menutup `DEC-CRM-002`; titik sentuh `billing-kasir` |
| `CRM-DEC-027` | Decision | Kelompok diskon ditentukan dari **asal item tagihan** yang sudah ada di billing: `CONSULTATION` = konsultasi, `PROCEDURE` = tindakan, `LABORATORY` = laboratorium, `RADIOLOGY` = radiologi, `PHARMACY` = farmasi, dan biaya pendaftaran/administrasi = pendaftaran. Item lain (kamar operasi `OPERATING_ROOM`, pemakaian alat `EQUIPMENT_USAGE`, item ad-hoc `ADHOC`/`ADHOC_CATALOG`, kamar rawat) **tidak didiskon**. Tidak ada master pemetaan baru. Contoh: member Gold dengan diskon konsultasi 10%, farmasi 5%. Konsultasi Rp 300.000 didiskon Rp 30.000, obat Rp 200.000 didiskon Rp 10.000, dan item ad-hoc Rp 50.000 tidak didiskon. Total diskon Rp 40.000, tagihan Rp 510.000 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Amendment pass; menutup `DEC-CRM-003`; asal item: `BillingBridgeSourceDomains` (`BillingClinicalChargeBridgeService.cs:761-772@1aecf642`) |
| `CRM-DEC-028` | Decision | Diskon member berlaku untuk **semua jenis kunjungan**: rawat jalan, IGD, dan rawat inap, selama dibayar pribadi sepenuhnya (`CRM-DEC-016`), dengan pemetaan item yang sama (`CRM-DEC-027`). Kamar rawat tetap tidak didiskon. Sejalan dengan PDF IGD poin 7. Contoh: rawat inap pribadi member Gold (farmasi 5%), obat Rp 1.000.000 didiskon Rp 50.000, dan kamar Rp 3.000.000 tidak didiskon | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Ditanyakan saat arsitektur domain (`DEC-CRM-005`) |
| `CRM-DEC-029` | Decision | Empat usulan desain disetujui: (1) urutan tingkat untuk upgrade **Silver < Gold < Platinum < Executive** mengikuti `PriorityLevel` (menutup `AG-04`); (2) petugas CRM boleh membatalkan pendaftaran **Menunggu Pembayaran** dengan alasan wajib, dan tagihan iurannya ikut dibatalkan; (3) data lama yang **diakui** dengan tanggal akhir yang sudah lewat langsung berstatus **Kedaluwarsa**; (4) jendela perpanjangan 30 hari dan batas bayar 7 hari disimpan di **konfigurasi aplikasi** (dapat diubah tanpa ubah kode, perlu restart) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Review blueprint revision 1 |
| `CRM-DEC-030` | Decision | Blueprint revision 1 **disetujui**. Perencanaan delivery dilakukan **terbatas** pada `MVP-0` s.d. `MVP-2` (`EPIC CRM-01` s.d. `CRM-07`). Owner menerima bahwa rilis ini belum bisa mengaktifkan member baru. Member lama tetap bisa aktif lewat verifikasi. `EPIC CRM-08`/`09` direncanakan setelah `DEC-CRM-001` diputuskan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-13 | Review blueprint revision 1 |
| `CRM-DEC-031` | Decision (mengubah `CRM-FE-001`) | Bagian menu **CRM** hanya berisi **Daftar Membership**. Menu **Pendaftaran Membership** dihapus. Formulir pendaftaran dibuka dari tombol **+ Pendaftaran Membership** di halaman Daftar Membership (tombol tampil bagi pemegang `PatientMembership : Create`), dan rute `/create` tetap menyorot menu Daftar Membership. Akibat yang diterima: pengguna yang hanya punya `PatientMembership : Create` tanpa `Read` tidak punya jalan ke halaman pendaftaran, sehingga petugas CRM perlu diberi `Read` dan `Create` sekaligus. Contoh: petugas Marketing dengan izin `Read` + `Create` membuka CRM → Daftar Membership, lalu menekan + Pendaftaran Membership. Petugas yang hanya diberi `Create` tidak melihat grup CRM sama sekali | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-16 | Keputusan owner saat implementasi `FE-CRM-002`; `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx` (grup `healthServicesCrm`) |
| `CRM-CONF-001` | Conflict | PDF menaruh **Membership Discount** sebagai cara bayar. Keputusan rawat jalan yang sudah terkunci menetapkan diskon membership sebagai potongan harga **sebelum** pembagian penjamin, bukan sebagai penjamin/cara bayar. Contoh: tagihan Rp 1.000.000, diskon Gold 10%. Menurut aturan RJ, tagihan bersih menjadi Rp 900.000 lalu dibayar pasien atau asuransi. Menurut PDF, diskon tercatat sebagai "pembayaran" Rp 100.000 | Yoga Aji Pratama | ditutup oleh `CRM-DEC-008` | Yoga Aji Pratama, 2026-10-13 | `rawat-jalan/00-interview-decisions.md:201,216` |
| `CRM-CONF-002` | Conflict | Daftar tingkat di PDF (5) berbeda dari enum source (8: ditambah Corporate, Family, Other) | Yoga Aji Pratama | ditutup oleh `CRM-DEC-006` | Yoga Aji Pratama, 2026-10-13 | `CRM-FACT-003` |

## Glossary

| Istilah | Arti di modul ini |
|---|---|
| Member / pasien prioritas | Pasien dengan membership berstatus Aktif pada tingkat Silver, Gold, Platinum, atau Executive |
| Reguler | Pasien tanpa membership aktif. Bukan tingkat yang didaftarkan |
| Iuran | Biaya membeli membership untuk satu periode, ditagih lewat kasir |
| Periode | Rentang tanggal berlaku satu membership, misalnya 13 Okt 2026 s.d. 12 Okt 2027 |
| Manfaat | Hak yang didapat member: diskon per kelompok layanan dan perlakuan prioritas non-klinis |

## Aktor dan Tanggung Jawab

| Aktor | Boleh melakukan | Dasar |
|---|---|---|
| Petugas CRM/Marketing | Mendaftarkan, menaikkan tingkat, membatalkan membership, melihat Daftar Membership | `CRM-DEC-011` |
| Kasir | Menerima iuran (yang mengaktifkan membership) dan memproses refund pembatalan salah input | `CRM-DEC-004`, `CRM-DEC-013` |
| Admisi RJ / IGD | Hanya melihat status member saat mencari pasien | `CRM-DEC-011` |
| Sistem (proses terjadwal) | Mengubah status menjadi Kedaluwarsa, dan membatalkan pendaftaran yang tidak dibayar 7 hari | `CRM-DEC-005`, `CRM-DEC-014` |

## Proses Bisnis — Pendaftaran Membership

1. **Tujuan:** pasien terdaftar sebagai member pada satu tingkat dan mendapat manfaatnya setelah membayar.
2. **Pelaku:** petugas CRM/Marketing mendaftarkan, kasir menerima iuran.
3. **Pemicu:** pasien meminta menjadi member.
4. **Prasyarat:** pasien sudah punya No. RM (`CRM-DEC-003`), dan tidak sedang punya membership Aktif atau Menunggu Pembayaran kecuali untuk upgrade (`CRM-DEC-012`).
5. **Langkah utama:**
   1. Petugas CRM mencari pasien lewat No. RM, NIK, atau No. Telp, lalu memilih satu orang (`CRM-DEC-010`).
   2. Petugas memilih tingkat (Silver/Gold/Platinum/Executive) dan menyimpan formulir.
   3. Sistem membuat tagihan iuran sesuai tingkat, dan status menjadi Menunggu Pembayaran.
   4. Pasien membayar iuran di kasir.
   5. Sistem mengubah status menjadi Aktif dan mulai menghitung periode 12 bulan dari tanggal lunas.
   6. Pasien muncul di Daftar Membership sebagai member aktif.
6. **Hasil akhir:** status member terbaca di pendaftaran RJ, IGD, antrean, dan kasir.

## State dan Transition

> `Menunggu Pembayaran` belum ada di enum `MembershipStatus` source (`CRM-FACT-004`).
> Pemetaannya diputuskan saat desain, bukan di sini.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Contoh |
|---|---|---|---|---|---|
| — | Simpan formulir pendaftaran | Menunggu Pembayaran | Petugas CRM | Pasien punya No. RM, tanpa membership aktif lain | Gold untuk Ibu Sari disimpan 13 Okt 09.00 |
| Menunggu Pembayaran | Iuran lunas | Aktif | Kasir (memicu lewat pelunasan) | Lunas sebelum batas 7 hari | Lunas 13 Okt 10.15, berlaku s.d. 12 Okt 2027 |
| Menunggu Pembayaran | 7 hari tanpa pelunasan | Batal (tidak dibayar) | Sistem | — | Disimpan 13 Okt, batal 20 Okt 09.00 |
| Aktif | Tanggal akhir lewat | Kedaluwarsa | Sistem | Tidak diperpanjang | Berakhir 12 Okt 2027, 13 Okt sudah Reguler |
| Aktif | Upgrade lunas | Upgraded (lama) + Aktif (baru) | Petugas CRM + kasir | Iuran tingkat baru lunas | Silver ditutup, Gold mulai 13 Okt 2026 |
| Aktif | Berhenti atas permintaan pasien | Cancelled, tanpa refund | Petugas CRM | Alasan wajib | — |
| Menunggu Pembayaran (perpanjangan) | Iuran lunas | Terjadwal | Kasir (memicu lewat pelunasan) | Dalam jendela 30 hari sebelum periode lama berakhir | Lunas 1 Okt 2027, periode 13 Okt 2027 s.d. 12 Okt 2028 |
| Terjadwal | Periode lama berakhir | Aktif | Sistem | — | 13 Okt 2027 00.00 WIB |
| Terjadwal | Upgrade atau berhenti sebelum mulai | Cancelled, refund penuh | Petugas CRM + kasir | Alasan wajib | Upgrade ke Platinum 5 Okt 2027 |
| Aktif | Pembatalan salah input | Cancelled, refund penuh | Petugas CRM + kasir | Alasan wajib, belum ada diskon terpakai | Platinum keliru, padahal pasien membeli Gold |
| Data lama tanpa bukti bayar | Aturan baru dipasang | Perlu Verifikasi | Sistem (sekali saat migrasi aturan) | Tidak ada tagihan iuran lunas | Gold lama milik pasien X |
| Perlu Verifikasi | Diakui CRM | Aktif | Petugas CRM | Tanggal mulai dan berakhir sah diisi | Berlaku 1 Mar 2026 s.d. 28 Feb 2027 |
| Perlu Verifikasi | Dibatalkan CRM | Cancelled | Petugas CRM | Alasan wajib | Tidak ada bukti pembayaran |

## Skenario Normal dan Exception

| Skenario | Perilaku yang disepakati | Dasar |
|---|---|---|
| Kunjungan saat membership masih Menunggu Pembayaran | Pasien diperlakukan Reguler | `CRM-DEC-004` |
| Kunjungan terdaftar sebelum kedaluwarsa, dilayani sesudahnya | Tetap memakai tingkat saat didaftarkan | `CRM-DEC-005` |
| Satu No. Telp dipakai beberapa pasien | Semua ditampilkan, admisi memilih | `CRM-DEC-010` |
| Member datang ke IGD dengan triage Hijau, ada pasien Merah | Pasien Merah lebih dulu | `CRM-DEC-007` |
| Member berobat dengan asuransi | Tanpa diskon member | `CRM-DEC-016` |
| Pembayaran iuran masuk setelah pendaftaran batal otomatis | Ditolak, pasien mendaftar ulang | `CRM-DEC-014` |

## Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `CRM-FE-001` | Nama bagian dan menu | Yoga Aji Pratama | `approved` | Bagian **CRM** dengan satu menu **Daftar Membership**. Pendaftaran dibuka dari tombol **+ Pendaftaran Membership** di daftar (diubah 2026-10-16 oleh `CRM-DEC-031`; semula dua menu: Pendaftaran Membership dan Daftar Membership) | PDF membership poin 1; `CRM-DEC-031` |
| `CRM-FE-002` | Hak tampil menu | Yoga Aji Pratama | `approved` | Menu CRM hanya untuk petugas CRM/Marketing. Admisi melihat status member di layar pendaftarannya sendiri | `CRM-DEC-011` |
| `CRM-FE-004` | Isian formulir pendaftaran | Yoga Aji Pratama | `approved` | Petugas hanya memilih pasien dan tingkat (plus catatan). Status, tanggal berlaku, nomor member, poin, dan total belanja diisi sistem atau tidak ditampilkan | `CRM-DEC-004`, `CRM-DEC-005`, `CRM-DEC-022`, `CRM-DEC-023` |
| `CRM-FE-005` | Halaman yang dipakai | Yoga Aji Pratama | `approved` | Menu CRM membuka halaman patient-memberships yang sudah ada (diperluas, bukan dibuat ulang), memakai rute backend `/admin` | `CRM-DEC-017`, `CRM-DEC-018` |
| `CRM-FE-003` | Route, tata letak, kolom Daftar Membership, filter, bentuk formulir (halaman/modal) | — | `DEV_DISCRETION` | Ikuti konvensi frontend yang berlaku | Belum ada brief UI |

## Acceptance Criteria (sudah dapat diuji)

1. Formulir pendaftaran menolak pasien yang belum punya No. RM.
2. Membership yang baru disimpan berstatus Menunggu Pembayaran, dan pasien terbaca Reguler sampai iuran lunas.
3. Setelah iuran lunas, status menjadi Aktif dengan periode 12 bulan sejak tanggal lunas.
4. Pendaftaran yang tidak dibayar 7 hari otomatis batal.
5. Sehari setelah tanggal akhir, pasien terbaca Reguler, sedangkan kunjungan yang terdaftar sebelumnya tetap bertingkat lama.
6. Satu pasien tidak dapat memiliki dua membership aktif sekaligus.
7. Upgrade menutup membership lama sebagai `Upgraded` tepat saat iuran baru lunas.
8. Pembatalan tanpa alasan ditolak. Refund hanya tersedia untuk salah input tanpa diskon terpakai.
9. Pencarian lewat No. Telp yang dipakai dua pasien menampilkan keduanya, tanpa memilih otomatis.
10. Pengguna non-CRM tidak dapat mendaftar, upgrade, atau membatalkan (kode 403).
11. Hanya tingkat Silver, Gold, Platinum, dan Executive yang dapat dipilih saat pendaftaran.
12. Diskon member tidak diterapkan pada kunjungan berpenjamin asuransi atau perusahaan.
13. Akun kiosk yang memanggil pembuatan membership ditolak (kode 403), sedangkan pencarian nomor member di kiosk tetap berjalan.
14. Petugas CRM dengan izin `PatientMembership : Create` (tanpa role Administrator) dapat membuka daftar dan mendaftarkan membership.
15. Membership Aktif tidak bisa diubah pasien, tingkat, maupun tanggalnya. Hanya catatan yang tersimpan.
16. Nomor member terisi otomatis dan unik. Isian nomor dari request diabaikan atau ditolak.
17. Membership lama tanpa tagihan iuran lunas terbaca Perlu Verifikasi, dan pasiennya terbaca Reguler.
18. Perpanjangan pada tingkat yang sama setelah kedaluwarsa diterima.
19. Perpanjangan lebih dari 30 hari sebelum tanggal berakhir ditolak. Perpanjangan yang lunas di dalam jendela berstatus Terjadwal dan menjadi Aktif otomatis pada hari setelah periode lama berakhir.
20. Upgrade atau berhenti saat ada periode Terjadwal membatalkan periode itu dengan refund penuh.
21. Menambahkan asuransi sebelum tagihan final mencabut diskon member. Melepasnya mengembalikan diskon. Keduanya tercatat di riwayat.
22. Diskon hanya mengenai item berasal konsultasi, tindakan, laboratorium, radiologi, farmasi, dan biaya pendaftaran, sesuai persen tingkat. Item kamar operasi, alat, ad-hoc, dan kamar rawat tidak didiskon.

## Amendment yang diteruskan ke modul lain

| Ke modul | Isi yang harus dibahas di modul tersebut | Batasan dari CRM |
|---|---|---|
| `rawat-jalan` | Pendaftaran member tanpa poli/dokter, nomor reservasi dan link konfirmasi kedatangan via WA (termasuk keamanan link: masa berlaku, sekali pakai), screening dan admisi menghampiri pasien, dokter menghampiri pasien | Hak berlaku untuk semua tingkat berbayar (`CRM-DEC-015`) |
| `kiosk` / antrean | Nomor prioritas kelipatan 5 yang tidak tampil di display, termasuk nasib nomor kelipatan 5 bila tidak ada pasien prioritas dan bila pasien prioritas lebih dari jatah | Antrean prioritas untuk semua tingkat berbayar |
| `igd` | Reservasi prioritas via WA/telp, order ambulans, pendampingan petugas khusus | Tidak boleh mengubah urutan triage (`CRM-DEC-007`) |
| `rawat-inap` | Prioritas mendapatkan kamar rawat | Tidak boleh mengalahkan kebutuhan medis (`CRM-DEC-007`) |
| `billing-kasir` | **Sudah diajukan:** `approval-requests/bkc-amendment-tagihan-iuran-membership.md`. Tagihan iuran membership di luar kunjungan, refund iuran salah input, penerapan diskon per tingkat untuk kunjungan pribadi, pengiriman tagihan via WA/email, arti "ketentuan pembayaran IGD sama dengan RJ" | `CRM-DEC-004`, `CRM-DEC-008`, `CRM-DEC-013`, `CRM-DEC-016` |
| Finance/Accounting | Wallet / Membership Prepaid (rilis berikutnya) | `CRM-DEC-009` |

## Penyelesaian Conflict dari Capability Map

| Conflict | Diselesaikan oleh | Sifat penyelesaian |
|---|---|---|
| `CRM-CONF-003` aktif tanpa bayar | `CRM-DEC-004`, `CRM-DEC-019` | Perbaikan source, menunggu amendment billing untuk sisi pelunasan |
| `CRM-CONF-004` tidak pernah kedaluwarsa | `CRM-DEC-005` | Perbaikan source (proses terjadwal dan penentu status) |
| `CRM-CONF-005` dua membership aktif | `CRM-DEC-012` | Perbaikan validasi |
| `CRM-CONF-006` perpanjangan tingkat sama ditolak | `CRM-DEC-005`, `CRM-DEC-012` | Perbaikan validasi: larangan hanya untuk membership yang masih aktif/menunggu |
| `CRM-CONF-007` hak akses kiosk dan rute FE | `CRM-DEC-011`, `CRM-DEC-018` | Perbaikan otorisasi BE dan pengalihan rute FE |
| `CRM-CONF-008` riwayat tertimpa | `CRM-DEC-013`, `CRM-DEC-022` | Perbaikan source: ubah dibatasi, batal wajib alasan |

## Open Questions dan Blocker

| ID | Pertanyaan | Owner | Memblokir |
|---|---|---|---|
| `CRM-OQ-001` | Nominal iuran dan persen diskon per kelompok layanan untuk tiap tingkat | Yoga Aji Pratama / Marketing | `IMPLEMENTATION` (isian master), tidak memblokir desain |
| `CRM-OQ-002` | Apakah status `Suspended` dan `Downgraded` dipakai? Usulan awal: turun tingkat hanya saat perpanjangan | Yoga Aji Pratama | `LATER SLICE` |
| `CRM-OQ-003` | Pengingat kedaluwarsa dan aktivasi via WA. Pemilik layanan WhatsApp belum diketahui | Platform/notifikasi | `LATER SLICE` |
| `CRM-OQ-004` | Kartu member (kolom `CardTitle`/`CardColor`/`CardImagePath` sudah ada): dicetak atau digital? | Yoga Aji Pratama | `LATER SLICE` |
| `CRM-OQ-005` | Kebutuhan CRM sudah diputuskan (`CRM-DEC-019`). Tersisa persetujuan owner `billing-kasir` atas amendment tagihan non-kunjungan (`approval-requests/bkc-amendment-tagihan-iuran-membership.md`) | Owner `billing-kasir` (nama belum tercatat) | `DESIGN` hanya untuk bagian aktivasi dan refund iuran; bagian lain tidak terhambat |
| `CRM-OQ-006` | ~~Apakah data membership per pasien sudah ada?~~ **Terjawab: sudah ada, perlu diperluas dan diperbaiki** (`CRM-FACT-007`) | — | ditutup |
| `CRM-OQ-007` | ~~Ditutup oleh `CRM-DEC-017`~~ CRM memperluas `MstPatientMembership` milik PatientManagement, atau menjadi modul sendiri dengan prefix baru yang wajib didaftarkan (`CRM-CAP-013`)? | Yoga Aji Pratama | `DESIGN` |
| `CRM-OQ-008` | ~~Ditutup oleh `CRM-DEC-018`~~ Apakah rute `POST /kiosk` (kiosk membuat membership) ditutup demi `CRM-DEC-011` (`CRM-CONF-007`)? | Yoga Aji Pratama + owner `kiosk` | `DESIGN` |
| `CRM-OQ-009` | ~~Ditutup oleh `CRM-DEC-021`~~ Membership lama yang sudah tersimpan berstatus Aktif tanpa bukti bayar: diakui apa adanya atau diverifikasi ulang? Butuh pengecekan DB dengan izin terpisah (`CRM-CAP-015`) | Yoga Aji Pratama | `IMPLEMENTATION` |

| `DEC-CRM-002` | ~~Ditutup oleh `CRM-DEC-026`~~ Penjamin berubah di tengah kunjungan setelah diskon diterapkan (`02-requirement-completeness-assessment.md`) | Yoga Aji Pratama | `DESIGN` slice S4 (diskon) |
| `DEC-CRM-003` | ~~Ditutup oleh `CRM-DEC-027`~~ Pemetaan enam kelompok diskon tier ke kategori item tagihan | Yoga Aji Pratama | `DESIGN` slice S4 (diskon) |
| `DEC-CRM-004` | ~~Ditutup oleh `CRM-DEC-024`~~ Perpanjangan sebelum masa berlaku habis: periode baru mulai kapan | Yoga Aji Pratama | `DESIGN` slice S1b |

| `CRM-OQ-010` | Bagaimana billing mengenali item "biaya pendaftaran/administrasi" untuk kelompok diskon pendaftaran (`CRM-DEC-027`)? Pertanyaan fakta source, dijawab saat desain lewat penelusuran `AdministrationFeeCalculationService` | Desain (titik sentuh `billing-kasir`) | `DESIGN` S4, tidak memblokir keputusan |

**Blocker desain:** tidak ada blocker untuk keseluruhan desain. Satu ketergantungan parsial:
`CRM-OQ-005`, yaitu persetujuan owner `billing-kasir` atas tagihan iuran non-kunjungan. Desain
boleh berjalan, dan bagian aktivasi-setelah-lunas serta refund iuran ditandai bergantung pada
persetujuan itu.

## Riwayat Pass

| Pass | Tanggal | Mode | Ringkasan |
|---|---|---|---|
| 1 | 2026-10-13 | Scope pass | Scope dikunci. 16 keputusan disetujui owner. Dua conflict (diskon sebagai cara bayar, daftar tingkat) ditutup. Wallet ditunda |
| 3g | 2026-10-16 | Amendment (frontend) | `CRM-DEC-031` mengubah `CRM-FE-001`: menu Pendaftaran Membership dihapus, pendaftaran lewat tombol di Daftar Membership. Pemegang `Create` tanpa `Read` kehilangan jalan ke halaman pendaftaran |
| 3f | 2026-10-13 | Approval roadmap + registry | Roadmap r1 `approved` oleh Yoga Aji Pratama. Baris registry folder `PatientManagement/Membership` (`Pat`) ditambahkan atas persetujuan Muhammad Hamzah (`BE-CRM-000` selesai) |
| 3e | 2026-10-13 | Perencanaan delivery | `roadmap/` r1 `draft`: 13 task BE (`BE-CRM-000` s.d. `023`, dua ⛔), 7 task FE, 8 coverage gap terkait `DEC-CRM-001`/`AG-01`/`AG-03` |
| 3d | 2026-10-13 | Approval | Blueprint revision 1 `approved` (`CRM-DEC-030`), empat usulan disetujui (`CRM-DEC-029`). Perencanaan terbatas MVP-0..2 |
| 3c | 2026-10-13 | Desain blueprint | Blueprint revision 1 (`draft`): `02` s.d. `04`, `erd/`, `contracts/`, `testing/`, `blueprint-manifest.md`. Development lock menunggu `DEC-CRM-001` |
| 3b | 2026-10-13 | Arsitektur domain | `03-domain-architecture.md` revision 1: `DOMAIN_ARCHITECTURE_PARTIAL` (S1a, S1b, S3, S4, S5 siap; S2 terblokir `DEC-CRM-001`) |
| 3a | 2026-10-13 | Arsitektur domain | `DEC-CRM-005` (jenis kunjungan yang mendapat diskon) muncul saat pemodelan dan ditutup oleh `CRM-DEC-028` |
| 3 | 2026-10-13 | Amendment pass | Impact scan BE `1aecf642`/FE `f1e5e273a` menemukan definisi "member aktif" kedua di antrean RJ (`CRM-CAP-016`). `CRM-DEC-024` s.d. `CRM-DEC-027` menutup `DEC-CRM-002` s.d. `DEC-CRM-004`. Permintaan amendment billing diperluas (refund Terjadwal, penerapan diskon) |
| 2a | 2026-10-13 | Requirement gate | `PARTIALLY_READY`: S1a, S3, S5 siap. S1b, S2, S4 menunggu `DEC-CRM-001` s.d. `DEC-CRM-004` |
| 2 | 2026-10-13 | Closure pass | 7 keputusan baru (`CRM-DEC-017` s.d. `CRM-DEC-023`). `CRM-OQ-007`/`008`/`009` ditutup, `CRM-CONF-003` s.d. `CRM-CONF-008` dipetakan ke keputusan. Permintaan amendment billing diajukan |
| 1a | 2026-10-13 | Audit (trace) | Capability map revision 1. `CRM-FACT-005` dikoreksi oleh `CRM-FACT-007`. `CRM-OQ-005`/`006` terjawab. Muncul `CRM-OQ-007` s.d. `CRM-OQ-009` |
