# MCU (Medical Check-Up) — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian V2 — Health Services |
| Modul | MCU (Medical Check-Up), kode `MCU` |
| Blueprint | `MCU-BP-001` revision 1 |
| contract_version | `v1` — status **`approved`** |
| Owner | Yoga Aji Pratama |
| approved_by / approved_at | Yoga Aji Pratama / 2026-10-21 |
| Repository target | `NewQuilvianSystemBackend` (SHA `4d3e4f98`), `QuilvianSystemFrontendDev` (SHA `7fb1e1d3e`) |
| input_revision | Decision log r5; `MCU-EVD-002`; capability map r1; penilaian kelengkapan r2; arsitektur domain `MCU-HDA-001` r1 (`DOMAIN_ARCHITECTURE_READY`); `02-backend-architecture.md` r1; kontrak `MCU-API-v1`, `MCU-STATE-v1`, `MCU-VAL-v1`, `MCU-INT-v1`, `MCU-PERM-v1` |
| Ringkasan cakupan | Rilis pertama melayani peserta perorangan dan perusahaan dari booking admisi atau datang langsung, sampai hasil MCU diverifikasi dan diserahkan, dengan pesanan Lab/Rad yang tidak ditagih dua kali |

## 2. Ringkasan eksekutif

Hari ini rumah sakit tidak bisa melayani MCU di Quilvian V2: tidak ada paket, tidak ada layar, dan
kunjungan MCU bahkan tidak pernah lahir dari aplikasi. MVP ini memungkinkan **satu peserta MCU dilayani
dari booking sampai hasilnya diserahkan** dengan tiga jaminan: peserta membayar **satu harga paket**
(bukan harga satuan ditambah paket), **dokter MCU** jelas bertanggung jawab atas hasil kritis dan resume,
dan **perusahaan hanya menerima ringkasan kelayakan** kecuali peserta menyetujui lebih.

## 3. Masalah produk

| Keadaan sekarang | Bukti |
| --- | --- |
| Jenis kunjungan MCU dikenal sistem, tetapi tidak ada layar yang membuatnya | `MCU-CAP-01`, `MCU-CAP-03` |
| Kiosk mendaftarkan Klinik MCU sebagai kunjungan rawat jalan | `MCU-CAP-06` |
| Tidak ada master paket; penanda *tarif paket* hanya bendera | `MCU-CAP-13`, `MCU-CAP-14` |
| Lab dan Rad menagih per pemeriksaan otomatis → paket akan tertagih dobel | `MCU-CAP-22` |
| Radiologi tidak punya gerbang lunas | `MCU-CAP-18` |
| Tidak ada tempat resume, verifikasi, maupun catatan penyerahan | `MCU-CAP-30` |
| Tidak ada pengirim WhatsApp/email di backend; v1 mengirim dari browser ke server luar | `MCU-CAP-32`, `MCU-CAP-33` |
| **Sudah ada dan dipakai ulang:** penjamin perusahaan dan masa kontrak, gerbang lunas Lab, baris tagihan katalog, refund, jalur pembuatan kunjungan untuk modul lain, skrining perawat | `MCU-CAP-08`..`10`, `17`, `25`, `27`, `29` |

## 4. Visi produk

1. Admin MCU menyusun paket, harga umum, harga perusahaan, dan kuota harian.
2. Admisi mencatat booking; peserta yang belum menjadi pasien cukup identitas sementara.
3. Pada hari H peserta check-in; pasien ditautkan atau dibuat, kunjungan MCU lahir, dokter MCU ditetapkan, satu baris paket masuk tagihan.
4. Sesudah lunas atau terjamin, pesanan Lab/Rad dibuat bertanda *dicakup paket* sehingga tidak ditagih lagi.
5. Perawat menyelesaikan skrining; dokter MCU mengisi Pemeriksaan MCU.
6. Saat semua komponen tuntas, dokter MCU menyusun dan menyetujui resume; petugas memverifikasi.
7. Petugas menyerahkan hasil lengkap ke peserta dan ringkasan kelayakan ke perusahaan, lalu mencatat setiap penyerahan.

## 5. Batas MVP

**Titik mulai:**
1. Admin MCU membuat paket dan mengatur kuota harian.
2. Admisi mencatat booking satu peserta (telepon/WhatsApp), atau mendaftarkan peserta yang datang langsung di admisi.

**Titik akhir:**
1. Hasil MCU berstatus *Diserahkan* dengan catatan penyerahan untuk peserta (dan ringkasan untuk perusahaan bila berpenjamin).
2. Tagihan kunjungan MCU memuat tepat satu baris paket (ditambah pemeriksaan di luar paket bila ada) dan dapat dilunasi atau ditagihkan ke perusahaan.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab di MVP |
| --- | --- |
| Admin MCU | Paket, jenis Pemeriksaan MCU, kategori kelayakan, kuota |
| Kepala unit MCU | Pelampauan kuota, ganti dokter MCU |
| Petugas admisi MCU | Booking, jadwal ulang, batal, check-in, *walk-in* |
| Perawat | Skrining awal (layar nurse station yang ada) |
| Dokter MCU | Pemeriksaan MCU, tambah/batal komponen, resume, buka ulang |
| Petugas MCU | Mulai pemeriksaan, verifikasi, cetak, catat penyerahan & persetujuan |
| Kasir | Menerima pembayaran paket (layar Billing yang ada) |
| Analis Lab / Radiografer | Mengerjakan pesanan bertanda paket di modulnya |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Master paket, komponen, persyaratan, nilai pengembalian & jasa, harga perusahaan | `MCU-CAP-14` (Missing), `MCU-CAP-13`, `MCU-CAP-15` | Wajib; tanpa paket tidak ada yang dijual |
| Jenis Pemeriksaan MCU & kategori kelayakan | `MCU-CAP-30` (Missing) | Wajib; resume butuh kategori, pemeriksaan fisik butuh isian |
| Jadwal & kuota harian | `MCU-CAP-04` (Missing) | Wajib; booking tanpa kuota membanjiri Lab/Rad (`MCU-DEC-012`) |
| Booking, jadwal ulang, batal, tidak hadir | `MCU-CAP-04`, `MCU-CAP-07` | Wajib; titik mulai MVP |
| Check-in & *walk-in* admisi: pencocokan pasien, kunjungan, dokter MCU, persyaratan, baris paket | `MCU-CAP-02`, `MCU-CAP-05`, `MCU-CAP-25` | Wajib; tanpa kunjungan tidak ada pesanan maupun tagihan |
| Mulai pemeriksaan dengan gerbang lunas/terjamin | `MCU-CAP-17`, `MCU-CAP-10` | Wajib; butir 11 sumber |
| Pesanan Lab/Rad bertanda paket tanpa tagih dobel | `MCU-CAP-19`, `MCU-CAP-20`, `MCU-CAP-21`, `MCU-CAP-22` | Wajib; tanpa ini peserta ditagih Rp 760.000 untuk paket Rp 350.000 |
| Isi Pemeriksaan MCU, kunci skrining | `MCU-CAP-29`, `MCU-CAP-30` | Wajib |
| Tambah/batal komponen + refund; batal penuh | `MCU-CAP-27` | Wajib; tanpa ini kasus hamil-tidak-boleh-rontgen tidak dapat diselesaikan dengan benar |
| Resume berversi, approve, verifikasi, buka ulang, ganti dokter | `MCU-CAP-30` | Wajib; inti hasil MCU |
| Cetak lengkap/ringkasan, persetujuan pelepasan, catat penyerahan | `MCU-CAP-32` (pengganti manual) | Wajib; titik akhir MVP |

## 8. Kemampuan yang ditunda

| Kemampuan | ID asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Check-in & *walk-in* lewat kiosk | `MCU-CAP-06` | Butuh perubahan modul kiosk (`MCU-COORD-006`). Dokter MCU kiosk sudah diputuskan: dokter jaga harian (`MCU-DEC-042`) | Peserta check-in di admisi MCU |
| Impor booking rombongan dari Excel | — (`MCU-DEC-033`) | Bukan syarat satu kasus selesai; butuh pengurai berkas dan templat | Admisi mencatat booking rombongan satu per satu |
| Tombol rekomendasi MCU di RJ/RI/IGD | — (`MCU-DEC-006`, `017`) | Perubahan layar di tiga modul lain; endpoint MCU siap tetapi konsumennya belum | Dokter menyampaikan rekomendasi; admisi mencatat booking |
| Sumber jasa medis untuk Medical Fee | — (`MCU-DEC-034`) | Bentuk penerimaan di Medical Fee belum disepakati (`MCU-COORD-010`) | Nilai jasa tetap tersimpan per komponen; Keuangan menghitung manual dari daftar komponen selesai |
| Pengiriman WhatsApp/email otomatis | `MCU-CAP-32` | Platform belum punya pengirim (`MCU-COORD-005`) | Penyerahan manual dicatat (`MCU-DEC-027`) |
| Konfirmasi datang via WhatsApp | — (`MCU-DEC-030`) | Kanal WhatsApp belum ada | Admisi menelepon peserta bila perlu |
| Lampiran berkas EKG/audiometri/spirometri | `MCU-CAP-19` Lab (`CAP-P19-03`) | Penyimpanan berkas belum ada (`MCU-DEC-035`) | Lembar asli disimpan fisik |
| Laporan MCU (rekap & statistik) | — (butir 14) | `DEC-MCU-005` belum diputuskan | Daftar Pasien MCU dengan penyaring tanggal/status/perusahaan |

## 9. Alur bisnis target

**`FLOW-MCU-MVP-001` — Peserta perorangan, satu paket**

1. Admin MCU membuat *Paket Basic* (DL, GDP, Thorax, Pemeriksaan Fisik) bertarif Rp 350.000 dan membuka kuota 30 untuk 27 Okt.
2. 23 Okt admisi mencatat booking Budi (identitas sementara) → order *Terjadwal*, kuota 1/30.
3. 27 Okt Budi datang; admisi mencocokkan NIK + tanggal lahir → tidak ada pasien → membuat pasien baru berisi isian awal.
4. Admisi memilih dr. Rina, menandai persyaratan puasa *terpenuhi*, check-in → order *Hadir*; kunjungan MCU dan baris paket Rp 350.000 terbentuk.
5. Budi membayar di kasir; petugas MCU menekan *Mulai pemeriksaan* → order *Dalam Pemeriksaan*; pesanan Lab dan Rad bertanda paket terbentuk.
6. Perawat menyelesaikan skrining; dr. Rina mengisi Pemeriksaan Fisik.
7. Lab memvalidasi DL & GDP, Radiologi melaporkan Thorax → order *Menunggu Resume*. Tagihan tetap Rp 350.000.
8. dr. Rina menyusun dan menyetujui resume (*Fit dengan catatan*) → *Disetujui Dokter*.
9. Dewi memverifikasi → *Terverifikasi*; mencetak hasil, mengirim lewat WhatsApp resmi unit, mencatat penyerahan → *Diserahkan*.

## 10. Epic dan functional requirement

### `EPIC MCU-01` — Fondasi dan master — `MISSING / NEW`

Tujuan: modul punya prefix, tabel, dan master yang cukup untuk satu kasus.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-MCU-001` | Baris registry `Mcu` tercatat sebelum model pertama dibuat (`MCU-GOV-001`). **Contoh:** checker QBE tidak melaporkan QBE-MOD-002 untuk `McuPackage.cs` |
| `FR-MCU-002` | Admin membuat paket dengan komponen bertipe Lab/Rad/Pemeriksaan MCU; paket ditolak bila total nilai pengembalian melebihi harga umum. **Contoh:** Rp 390.000 > Rp 350.000 → `400` (`VAL-MCU-04`) |
| `FR-MCU-003` | Admin menambah harga perusahaan bermasa berlaku; tumpang tindih ditolak (`VAL-MCU-05`) |
| `FR-MCU-004` | Admin membuat jenis Pemeriksaan MCU dengan isian ber-nomor butir, dan kategori kelayakan |
| `FR-MCU-005` | Admin mengatur hari buka, kuota, dan dokter MCU jaga untuk rentang tanggal; kuota tidak boleh di bawah terpakai (`VAL-MCU-14`); dokter jaga menjadi isian bawaan dokter MCU saat check-in admisi (`MCU-DEC-042`) |
| `FR-MCU-006` | Kepala unit menyetujui pelampauan beralasan; pengguna lain `403` |

### `EPIC MCU-02` — Booking dan jadwal — `MISSING / NEW`

| FR | Perilaku |
| --- | --- |
| `FR-MCU-010` | Booking membuat order *Terjadwal* tanpa kunjungan dan tanpa tagihan. **Contoh:** sesudah booking Budi, jumlah kunjungan Budi tetap 0 |
| `FR-MCU-011` | Booking ditolak bila kuota penuh (`VAL-MCU-11`); dua booking atas sisa kuota terakhir → tepat satu berhasil (`VAL-MCU-12`) |
| `FR-MCU-012` | Peserta belum terdaftar dibooking dengan identitas sementara (NIK 16 digit + tanggal lahir wajib); tidak ada No. RM terbentuk |
| `FR-MCU-013` | Order ganda pada tanggal sama untuk peserta sama ditolak (`VAL-MCU-18`) |
| `FR-MCU-014` | Jadwal ulang memindahkan kuota; batal sebelum datang beralasan melepas kuota tanpa refund |
| `FR-MCU-015` | Order *Terjadwal* yang lewat tutup hari menjadi *Tidak Hadir* dan melepas kuota. **Contoh:** pukul 23.59 WIB 27 Okt order Budi → *Tidak Hadir*, kuota 29/30 |

### `EPIC MCU-03` — Check-in dan *walk-in* admisi — `EXTEND` (Registrasi) + `MISSING / NEW`

| FR | Perilaku |
| --- | --- |
| `FR-MCU-020` | Pencocokan NIK + tanggal lahir; NIK sama tetapi tanggal lahir beda tidak ditautkan tanpa konfirmasi petugas (`VAL-MCU-25`) |
| `FR-MCU-021` | Check-in dalam satu transaksi membuat kunjungan `MedicalCheckup` lewat `EncounterIntakeService`, menetapkan dokter MCU, menyimpan penandaan persyaratan, menambah satu baris paket. Gagal di langkah mana pun → order tetap *Terjadwal*, nol kunjungan |
| `FR-MCU-022` | Harga paket = harga perusahaan yang berlaku pada tanggal check-in, bila tidak ada harga umum. **Contoh:** PT Contoh 27 Okt Rp 300.000; 5 Jan Rp 350.000 |
| `FR-MCU-023` | Persyaratan *tidak terpenuhi* → lanjut dengan catatan, atau komponen terkait dijadwal ulang tanpa memakai kuota |
| `FR-MCU-024` | Kirim ulang check-in tidak membuat kunjungan/baris kedua |
| `FR-MCU-025` | *Walk-in* admisi = order langsung *Hadir* dan memakai kuota hari itu |

### `EPIC MCU-04` — Pemeriksaan dan komponen — `EXTEND` (Lab, Rad) + `MISSING / NEW`

| FR | Perilaku |
| --- | --- |
| `FR-MCU-030` | *Mulai pemeriksaan* ditolak bila perorangan belum lunas (`VAL-MCU-34`); perusahaan terjamin lolos |
| `FR-MCU-031` | *Mulai pemeriksaan* membuat pesanan Lab per disiplin dan pesanan Rad per komponen, bertanda `CoveringMcuOrderId`, dokter penanggung jawab = dokter MCU |
| `FR-MCU-032` | Pesanan bertanda paket tidak menerbitkan tagihan Lab/Rad. **Contoh:** sesudah spesimen dan akuisisi, tagihan Budi tetap Rp 350.000 dan tetap lunas |
| `FR-MCU-033` | Dokter MCU menambah pemeriksaan di luar paket: ditagih biasa, ikut kelengkapan |
| `FR-MCU-034` | Dokter MCU membatalkan komponen beralasan; pemeriksaan/pesanan terkait batal; refund sebesar nilai pengembalian lewat Billing yang diperluas (`MCU-DEC-043`). **Contoh:** Thorax dibatalkan → refund Rp 150.000; Audiometri dibatalkan kemudian pada baris paket yang sama → refund Rp 50.000 |
| `FR-MCU-035` | Batal penuh hanya bila belum ada komponen dimulai; refund utuh; kunjungan dibatalkan |
| `FR-MCU-036` | Pemeriksaan MCU hanya dapat diisi sesudah skrining selesai; angka desimal tersimpan utuh (`6.4` tetap `6.4`) |
| `FR-MCU-037` | Daftar Pasien MCU menampilkan *n dari m komponen tuntas*; tombol *muat ulang* menyinkronkan status Lab/Rad |

### `EPIC MCU-05` — Resume dan verifikasi — `MISSING / NEW`

| FR | Perilaku |
| --- | --- |
| `FR-MCU-040` | Approve ditolak bila ada komponen belum tuntas atau skrining belum selesai |
| `FR-MCU-041` | Hanya dokter MCU yang ditetapkan yang menyimpan, menyetujui, dan membuka ulang resume; dokter lain `403` |
| `FR-MCU-042` | Petugas memverifikasi tanpa mengubah isi; dokter pemberi approve tidak boleh memverifikasi |
| `FR-MCU-043` | Petugas mengembalikan resume beralasan → *Menunggu Resume* |
| `FR-MCU-044` | Buka ulang membuat versi baru; versi lama *Dikoreksi* tetap terbaca; penyerahan versi lama ditandai perlu pemberitahuan; isian Pemeriksaan MCU terbuka dan nilai lama tercatat |
| `FR-MCU-045` | Kepala unit mengganti dokter MCU beralasan; pesanan terbuka berpindah tanggung jawab |

### `EPIC MCU-06` — Penyerahan dan privasi — `MISSING / NEW`

| FR | Perilaku |
| --- | --- |
| `FR-MCU-050` | Dokumen hasil lengkap dan ringkasan kelayakan disusun server; ringkasan tidak memuat nilai maupun diagnosis |
| `FR-MCU-051` | Penyerahan isi lengkap ke perusahaan ditolak tanpa persetujuan berlaku (`VAL-MCU-31`) |
| `FR-MCU-052` | Setiap penyerahan tercatat: penerima, isi, saluran, tujuan, versi, petugas, waktu; penyerahan pertama → *Diserahkan* |
| `FR-MCU-053` | Persetujuan pelepasan dicatat dan dapat dicabut beralasan |

### Epic yang tidak masuk MVP

| Epic | Disposisi | Gelombang |
| --- | --- | --- |
| `EPIC MCU-07` Impor rombongan | `MISSING / NEW` | `POST-MVP` |
| `EPIC MCU-08` Kiosk | `EXTEND` (modul kiosk) — dokter jaga `MCU-DEC-042` | `POST-MVP` — menunggu `MCU-COORD-006` |
| `EPIC MCU-09` Rekomendasi dari RJ/RI/IGD | `EXTEND` (modul lain) | `POST-MVP` |
| `EPIC MCU-10` Sumber jasa medis | `EXTEND` (Medical Fee) | `POST-MVP` — menunggu `MCU-COORD-010` |
| `EPIC MCU-11` Laporan MCU | **`OPEN DECISION`** (`DEC-MCU-005`) | Tidak dijadwalkan |

## 11. Model status yang diusulkan

Sembilan status order (`MCU-DEC-031`): *Terjadwal*, *Tidak Hadir*, *Hadir*, *Dalam Pemeriksaan*,
*Menunggu Resume*, *Disetujui Dokter*, *Terverifikasi*, *Diserahkan*, *Dibatalkan*. Invariant utama:

1. Tidak ada kunjungan sebelum *Hadir*; tidak ada pesanan Lab/Rad sebelum *Dalam Pemeriksaan*.
2. *Menunggu Resume* hanya bila semua komponen tuntas.
3. *Disetujui Dokter* hanya oleh dokter MCU yang ditetapkan, sesudah skrining.
4. *Terverifikasi* hanya oleh petugas yang bukan pemberi approve.
5. *Diserahkan* hanya untuk versi terverifikasi.

Rincian: [contracts/state-transition-matrix.md](contracts/state-transition-matrix.md).

## 12. Sasaran arsitektur

| Jenis | Isi |
| --- | --- |
| Dipakai ulang | `MstPatient`, `RegPatientEncounter`, penjamin perusahaan, `MstTariff`, `MstProcedure`, `BillingInvoiceService.AddCatalogChargeAsync`, refund Billing, `LabPaymentClearanceRules`, nurse station |
| Diperluas | `EncounterIntakeService` (kunjungan MCU berantrean skrining), `LabOrder`/`RadOrder` (+`CoveringMcuOrderId`), `LabOrderService`/`LabSpecimenService`/`RadOrderService`/`RadStudyService` (jalur MCU, penahanan tagihan, batal per pemeriksaan) |
| Baru | 21 tabel `Mcu*`, 12 service, 11 controller — [02-backend-architecture.md](02-backend-architecture.md) |

## 13. Sasaran kemampuan API

Seluruhnya **Rencana (belum tersedia)**; base URL `api/v1/health-services/medical-checkup-management`.
Ringkasan per grup; rincian di [contracts/api-contract.md](contracts/api-contract.md).

| Grup `[Tags]` | Endpoint MVP | Epic | Status |
| --- | --- | --- | --- |
| `Health Services / Medical Checkup Management / Mcu Package` | `GET filters/metadata`, `GET /`, `GET options`, `GET {id}`, `POST /`, `PUT {id}`, `PATCH {id}/status`, `POST {id}/company-prices`, `PATCH {id}/company-prices/{priceId}/end` | `EPIC MCU-01` | **Rencana (belum tersedia)** |
| `Health Services / Medical Checkup Management / Mcu Reference Data` | `examination-types` (GET/POST/PUT), `fitness-categories` (GET/POST/PUT) | `EPIC MCU-01` | **Rencana (belum tersedia)** |
| `Health Services / Medical Checkup Management / Mcu Daily Schedule` | `GET /`, `PUT /`, `POST {scheduleDate}/quota-overrides` | `EPIC MCU-01`, `02` | **Rencana (belum tersedia)** |
| `Health Services / Medical Checkup Management / Mcu Order` | `GET filters/metadata`, `GET summary`, `GET /`, `GET {id}`, `GET {id}/history`, `POST bookings`, `POST walk-ins`, `PATCH {id}/reschedule`, `PATCH {id}/cancel` | `EPIC MCU-02`, `03`, `04` | **Rencana (belum tersedia)** |
| `Health Services / Medical Checkup Management / Mcu Check In` | `GET {id}/patient-matches`, `GET {id}/check-in/preview`, `POST {id}/check-in`, `PATCH {id}/mcu-doctor` | `EPIC MCU-03`, `05` | **Rencana (belum tersedia)** |
| `Health Services / Medical Checkup Management / Mcu Examination` | `POST {id}/start-examination`, `GET/POST {id}/components`, `POST {id}/components/refresh`, `PATCH .../cancel`, `PATCH .../reschedule`, `GET/PUT .../examination-result` | `EPIC MCU-04` | **Rencana (belum tersedia)** |
| `Health Services / Medical Checkup Management / Mcu Resume` | `GET /`, `PUT current`, `POST current/approve`, `POST current/return`, `POST current/verify`, `POST reopen` | `EPIC MCU-05` | **Rencana (belum tersedia)** |
| `Health Services / Medical Checkup Management / Mcu Delivery` | `GET result-document`, `GET/POST deliveries`, `PATCH deliveries/{deliveryId}/correction-notice`, `GET/POST release-consents`, `PATCH release-consents/{consentId}/revoke` | `EPIC MCU-06` | **Rencana (belum tersedia)** |

Tidak masuk MVP: `Mcu Booking Import`, `Mcu Kiosk`, `POST orders/recommendations`,
`PATCH orders/{id}/arrival-confirmation`, `Mcu Medical Fee Source`.

## 14. Matriks kewenangan

| Pelaku | String permission (persis) |
| --- | --- |
| Admin MCU | `[AccessPermission("McuPackage", "Create")]`, `[AccessPermission("McuPackage", "Update")]`, `[AccessPermission("McuReferenceData", "Create")]`, `[AccessPermission("McuReferenceData", "Update")]`, `[AccessPermission("McuSchedule", "Update")]` |
| Kepala unit MCU | `[AccessPermission("McuSchedule", "ApproveQuotaOverride")]`, `[AccessPermission("McuOrder", "ReassignDoctor")]` |
| Petugas admisi MCU | `[AccessPermission("McuOrder", "Create")]`, `[AccessPermission("McuOrder", "Update")]`, `[AccessPermission("McuOrder", "Cancel")]`, `[AccessPermission("McuOrder", "CheckIn")]` |
| Dokter MCU (yang ditetapkan) | `[AccessPermission("McuExamination", "AddComponent")]`, `[AccessPermission("McuExamination", "CancelComponent")]`, `[AccessPermission("McuResume", "Update")]`, `[AccessPermission("McuResume", "Approve")]`, `[AccessPermission("McuResume", "Reopen")]` |
| Petugas/dokter MCU | `[AccessPermission("McuExamination", "Start")]`, `[AccessPermission("McuExamination", "Update")]` |
| Petugas MCU | `[AccessPermission("McuResume", "Verify")]`, `[AccessPermission("McuDelivery", "Create")]`, `[AccessPermission("McuDelivery", "ManageConsent")]` |
| Seluruh peran MCU (baca) | `[AccessPermission("McuOrder", "Read")]`, `[AccessPermission("McuExamination", "Read")]`, `[AccessPermission("McuResume", "Read")]`, `[AccessPermission("McuDelivery", "Read")]`, `[AccessPermission("McuPackage", "Read")]`, `[AccessPermission("McuReferenceData", "Read")]`, `[AccessPermission("McuSchedule", "Read")]` |

Sumber lengkap: [contracts/permission-audit-matrix.md](contracts/permission-audit-matrix.md).

## 15. Batas integrasi dan billing

MCU **MUST NOT** membuat sendiri:

| Yang tidak boleh dibuat MCU | Pemiliknya |
| --- | --- |
| Data pasien atau salinannya | Patient Management |
| Kunjungan di luar `EncounterIntakeService` | Registrasi |
| Tarif/nominal harga paket | Master Tarif |
| Tagihan, pembayaran, refund, PPN, piutang perusahaan | Billing |
| Pesanan, hasil, validasi, pemberitahuan nilai kritis Lab/Rad | Laboratorium / Radiologi |
| Pengiriman WhatsApp/email | Platform notifikasi |
| Perhitungan jasa medis | Medical Fee |

## 16. Guardrail regulasi

| Topik | Ketentuan MVP | Status verifikasi |
| --- | --- | --- |
| Rahasia medis & data pribadi kesehatan | Ringkasan perusahaan tanpa nilai/diagnosis; persetujuan tercatat; log tanpa data sensitif (`MCU-DEC-010`) | Kebijakan pemilik; bentuk bukti persetujuan menunggu `MCU-OQ-003` |
| Rekam medis | Resume MCU tetap milik MCU; tampil di linimasa Rekam Medis **ditunda** (`MCU-GAP-04`) | `VERIFY_CURRENT_REGULATION` — belum diverifikasi |
| Tanda tangan dokter | Resume menyimpan dokter pemberi approve + waktu; cetakan menampilkannya. Tanda tangan elektronik tersertifikasi tidak termasuk (`MCU-GAP-03`) | `VERIFY_CURRENT_REGULATION` |
| Koreksi hasil | Versi lama tidak dihapus, alasan wajib (`MCU-DEC-011`) | Keputusan pemilik |

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-001` | **Atomicity:** check-in, mulai pemeriksaan, dan batal penuh berjalan dalam satu transaksi basis data lintas modul |
| `NFR-002` | **Concurrency:** kuota dijaga token konkurensi jadwal; setiap tindakan ubah order membawa `Version` |
| `NFR-003` | **Idempotensi:** kirim ulang check-in tidak menggandakan kunjungan atau baris paket (`IdempotencyKey`, `CorrelationId`) |
| `NFR-004` | **Audit:** setiap perpindahan status, penggantian dokter, pembatalan komponen, versi resume, penyerahan, persetujuan tercatat tahan lama |
| `NFR-005` | **Otorisasi:** tindakan dokter mensyaratkan izin **dan** kecocokan dokter yang ditetapkan |
| `NFR-006` | **Privasi log:** log aplikasi tidak memuat kolom bertanda Sensitif |
| `NFR-007` | **Waktu:** tanggal pemeriksaan dan tutup hari memakai tanggal kalender WIB; jam tutup hari dapat dikonfigurasi (bawaan 23.59 WIB) |
| `NFR-008` | **Koreksi:** isian terkunci sesudah approve; perubahan sesudah buka ulang menyimpan nilai lama |

## 18. Skenario UAT

| ID | Epic | Jenis | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- | --- |
| `UAT-01` | MCU-01 | Berhasil | Tarif *Paket Basic* Rp 350.000 tersedia | Admin membuat paket 4 komponen | Paket aktif tampil di pilihan booking |
| `UAT-02` | MCU-01 | Gagal | Sama | Isi nilai pengembalian total Rp 390.000 | Ditolak dengan pesan menyebut Rp 390.000 dan Rp 350.000 |
| `UAT-03` | MCU-02 | Berhasil | Kuota 27 Okt = 30, terpakai 0 | Booking Budi (belum pasien) | Order *Terjadwal*; kuota 1/30; Budi belum punya No. RM |
| `UAT-04` | MCU-02 | Gagal | Kuota 27 Okt penuh | Booking peserta lain | Ditolak *kuota penuh (30/30)* |
| `UAT-05` | MCU-02 | Berhasil | Order Budi *Terjadwal* 27 Okt | Tidak check-in sampai lewat 23.59 | *Tidak Hadir*, kuota kembali |
| `UAT-06` | MCU-03 | Berhasil | Order Budi *Terjadwal* hari ini | Check-in: pasien baru dibuat, dr. Rina, puasa terpenuhi | *Hadir*; satu kunjungan MCU; satu baris Rp 350.000 |
| `UAT-07` | MCU-03 | Gagal | NIK Budi cocok pasien lama, tanggal lahir beda | Check-in tanpa konfirmasi | Ditolak; diminta memeriksa KTP dan mengonfirmasi |
| `UAT-08` | MCU-03 | Berhasil | Siti karyawan PT Contoh (harga Rp 300.000 berlaku) | Check-in | Baris paket Rp 300.000 |
| `UAT-09` | MCU-04 | Gagal | Budi *Hadir*, belum bayar | *Mulai pemeriksaan* | Ditolak *belum lunas (sisa Rp 350.000)*; nol pesanan Lab/Rad |
| `UAT-10` | MCU-04 | Berhasil | Budi lunas | *Mulai pemeriksaan*; Lab terima spesimen; Rad akuisisi | Pesanan bertanda paket; tagihan tetap Rp 350.000 dan lunas |
| `UAT-11` | MCU-04 | Berhasil | Siti *Dalam Pemeriksaan* | dr. Rina membatalkan Thorax (hamil) | Thorax *Dibatalkan*; pengajuan refund Rp 150.000 |
| `UAT-12` | MCU-04 | Gagal | Spesimen Budi sudah diterima | Admisi mencoba batal penuh | Ditolak; diarahkan ke pembatalan per komponen |
| `UAT-13` | MCU-04 | Gagal | Skrining Budi belum selesai | dr. Rina mengisi Pemeriksaan Fisik | Ditolak *skrining belum selesai* |
| `UAT-14` | MCU-05 | Gagal | Thorax belum diekspertise | dr. Rina approve | Ditolak menyebut Thorax |
| `UAT-15` | MCU-05 | Berhasil | Semua komponen tuntas | dr. Rina approve; Dewi verifikasi | *Terverifikasi* |
| `UAT-16` | MCU-05 | Gagal | Resume Budi *Disetujui Dokter* | dr. Hadi (bukan dokter yang ditetapkan) membuka ulang | Ditolak `403` |
| `UAT-17` | MCU-05 | Berhasil | Resume Budi *Diserahkan* | dr. Rina buka ulang beralasan | Versi 1 *Dikoreksi* terbaca; versi 2 *Draf*; penyerahan lama ditandai perlu pemberitahuan |
| `UAT-18` | MCU-06 | Berhasil | Hasil Siti *Terverifikasi* | Serahkan lengkap ke Siti via WhatsApp; ringkasan ke PT Contoh via email | Dua catatan penyerahan; ringkasan tanpa nilai/diagnosis |
| `UAT-19` | MCU-06 | Gagal | Siti belum memberi persetujuan | Serahkan isi lengkap ke PT Contoh | Ditolak; diarahkan ke ringkasan |

## 19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Prefix `Mcu` tercatat di registry dan checker QBE tidak melaporkan QBE-MOD-002 | `FR-MCU-001`, keluaran checker |
| Satu peserta perorangan dapat berjalan dari booking sampai *Diserahkan* | `UAT-03`, `UAT-06`, `UAT-10`, `UAT-15`, `UAT-18` |
| Tagihan paket tidak pernah tergandakan oleh Lab/Rad | `UAT-10`, `AT-MCU-28` |
| Kuota tidak dapat terlampaui tanpa persetujuan kepala unit | `UAT-04`, `AT-MCU-05`, `AT-MCU-06` |
| Pasien salah tidak dapat tertaut otomatis | `UAT-07`, `AT-MCU-19` |
| Resume tidak dapat disetujui tanpa data lengkap dan bukan oleh dokter lain | `UAT-14`, `UAT-16`, `AT-MCU-39` |
| Perusahaan tidak dapat menerima nilai/diagnosis tanpa persetujuan | `UAT-19`, `AT-MCU-42` |
| Koreksi tidak menghapus versi lama | `UAT-17`, `AT-MCU-43` |
| Pembatalan komponen dan batal penuh menghasilkan refund yang benar | `UAT-11`, `UAT-12`, `AT-MCU-30`, `AT-MCU-32` |
| Log aplikasi bebas data sensitif | `AT-MCU-45` |
| `MCU-UNK-01`, `MCU-UNK-05`, `MCU-UNK-06` terbukti di devYoga | `AT-MCU-RUN-01`..`03` |
| Seluruh master MVP terisi dan master usulan diratifikasi dokter MCU | Rencana data master `02-backend-architecture.md` bagian 9; jawaban `MCU-OQ-002`, `MCU-OQ-003` |

## 20. Urutan pengiriman dan pertanyaan terbuka

### 20.1 Gelombang

| Gelombang | Isi | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | `EPIC MCU-01`: registry, dua migration, master paket/referensi/jadwal | **Terpenuhi 2026-10-21:** blueprint disetujui; `MCU-GOV-001`; persetujuan Lab/Rad atas kolom `CoveringMcuOrderId` |
| `MVP-1` | `EPIC MCU-02` booking & jadwal; `EPIC MCU-03` check-in & *walk-in* admisi | `MVP-0` selesai. Persetujuan Registrasi atas perluasan `EncounterIntakeService` **terpenuhi 2026-10-21** |
| `MVP-2` | `EPIC MCU-04` pemeriksaan & komponen | `MVP-1` selesai; `MCU-COORD-002`, `008` dan `ARC-TECH-01` **terpenuhi 2026-10-21**. Perluasan refund Billing (`MCU-DEC-043`) untuk `FR-MCU-034` **disetujui** Yasmina 2026-10-21 (`MCU-COORD-003`) dan dikerjakan di gelombang ini. `FR-MCU-035` (batal penuh) memakai refund utuh satu baris yang sudah didukung jalur yang ada (`MCU-EVD-002` 2.2) |
| `MVP-3` | `EPIC MCU-05` resume & verifikasi; `EPIC MCU-06` penyerahan & privasi | `MVP-2` selesai |
| `POST-MVP` | `EPIC MCU-07` impor, `EPIC MCU-08` kiosk (`MCU-COORD-006`), `EPIC MCU-09` rekomendasi, `EPIC MCU-10` jasa medis; pengiriman otomatis; lampiran | Di luar rilis pertama |
| Tidak dijadwalkan | `EPIC MCU-11` laporan (`DEC-MCU-005`) | Menunggu keputusan |

### 20.2 Pertanyaan sebelum development lock

| Pertanyaan | Siapa yang menjawab | Hasil 2026-10-21 | Memblokir |
| --- | --- | --- | :---: |
| `MCU-GOV-001` — baris registry `HealthServices / MedicalCheckupManagement / Medical Check-Up / Mcu / ACTIVE` | Pemilik registry (Muhammad Hamzah) + Yoga | **Disetujui**; baris tercatat, checker terbukti tidak melaporkan QBE-MOD-002 (`MCU-EVD-002`) | Tidak lagi |
| `MCU-COORD-002`/`008` — Lab & Rad menerima kolom `CoveringMcuOrderId`, penahanan tagihan, dokter MCU sebagai penanggung jawab | Pemilik `laboratorium`, `radiologi` | **Disetujui** (`MCU-EVD-002`) | Tidak lagi |
| Perluasan `EncounterIntakeService` untuk kunjungan MCU berantrean | Pemilik Registrasi | **Disetujui** (`MCU-EVD-002`) | Tidak lagi |
| `ARC-TECH-01` — Lab dapat membatalkan satu pemeriksaan dalam pesanan berisi beberapa pemeriksaan | Pemilik `laboratorium` | **Terbukti didukung** jalur yang ada (`MCU-EVD-002` 2.1) | Tidak lagi |
| `MCU-UNK-04` — refund nominal sebagian dari satu baris | Pemilik `billing-kasir` | **Terbukti tidak didukung** (`MCU-EVD-002` 2.2) → `MCU-DEC-043` perluas Billing; **disetujui** Yasmina, pemilik Billing, 2026-10-21 (`MCU-COORD-003`) | Tidak lagi |
| `MCU-OQ-002`, `MCU-OQ-003` — isi master Pemeriksaan MCU, kategori kelayakan, bentuk bukti persetujuan | Dokter MCU, pemilik privasi/legal | Belum | Tidak untuk development; **Ya untuk go-live** |
| Aturan berlabel **DES** pada `validation-matrix.md` | Yoga | **Disahkan** `MCU-DEC-041` | Tidak |
| Penundaan impor rombongan dan tombol rekomendasi ke `POST-MVP` | Yoga | Disetujui bersama blueprint | Tidak |
| `DES-MCU-001` — dokter MCU untuk check-in kiosk | Yoga | **Diputuskan** `MCU-DEC-042` (dokter jaga harian) | Tidak |
| `DEC-MCU-005` — definisi laporan | Yoga | Belum | Tidak |

**Blueprint disetujui Yoga Aji Pratama pada 2026-10-21.** Dokumen ini boleh diteruskan ke
`/plan-module-delivery`. Roadmap r1 disetujui 2026-10-21; persetujuan Billing atas refund sebagian
(`FR-MCU-034`) menyusul pada hari yang sama, sehingga tidak ada lagi task backend yang tertahan.
