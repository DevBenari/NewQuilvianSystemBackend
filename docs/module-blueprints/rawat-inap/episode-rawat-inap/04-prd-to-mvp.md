# Rawat Inap — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Rawat Inap — `InPatientManagement`, prefix entity `Inp`, lifecycle registry `ACTIVE` sejak `RWI-DEC-068` |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `episode-rawat-inap` — satu dari tiga sub-modul modul `rawat-inap`, bentuk `COMPOSITE` sejak `RWI-DEC-082`. [Manifest sub-modul](./blueprint-manifest.md), [peta modul](../02-module-map.md) |
| Revision artefak | **`0.12.0`** — draft Bed Management, bagian26, 10 Oktober2026. Riwayat metadata: **`0.11.0`** — bagian 25 Amandemen Alur Admisi Pendaftaran (10 Oktober 2026, `approved`), menyerap `RWI-DEC-267` s.d. `RWI-DEC-273` dan `RWI-AC-388` s.d. `RWI-AC-395`. Sebelumnya **`0.10.0`** — bagian 24 Workspace PPRI, 7 Oktober 2026 (`approved` 2026-10-08, `RWI-DEC-265`); `0.9.0` — bagian 23 Finishing (`approved`, `RWI-DEC-221`); **`0.8.0`** — bagian 22 amandemen terbatas penyelarasan `PRD-RWI-V2-001`, 15 September 2026. Garis keturunan: `0.7.0` → `0.6.1` → `0.6.0` → `0.5.0` → `0.4.1` |
| `contract_version` | Mengikuti set pada manifest anak, Bed Management draft; last_changed_in0.12.0. Riwayat metadata: **`0.11.0`** untuk bagian 24 & 25 (`approved` 10 Oktober 2026; bagian 24 `approved` 2026-10-08 lewat `RWI-DEC-265`); `0.10.0` untuk bagian 23 (`approved`); **`0.9.0`** untuk bagian 22 |
| Batas dokumen ini | MVP sub-modul `episode-rawat-inap` saja. Kemampuan milik dua sub-modul lain **bukan** bagian dari MVP di sini, dan itu bukan penundaan keputusan |
| Status | **`draft`** — Bed Management belum disetujui. Riwayat metadata: **`approved`** — amandemen bagian 25 (`0.11.0`) disetujui pengguna pada 10 Oktober 2026 atas instruksi eksplisit "setujui dan lakukan /plan-module-delivery". Bagian 24 `approved` Muhammad Hamzah 2026-10-08 (`RWI-DEC-265`) |
| Repository target | `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` |
| Backend SHA baseline | Bed Management: d4e1eca06fb28c05934c68c1e51a4dca01935a10. Riwayat metadata: `44099e4ddd921d51140d802cabf1cebbc5291d30` — branch `MHamzah`, memuat merge `6993212` dari `QuilvianIntegrationBackend`. Sebelumnya `5afb54bd75281648010e50ef14f43ca1f80d8efd` |
| Frontend SHA baseline | Bed Management: 969acfcc04cdf31074a1911e9827c31d25ddadd0. Riwayat metadata: `30db3734a5d1e1ed0de35197ffabc30ae9c8d4e3` — branch `HamzahV2`. Sebelumnya `dec4fdeff07c3c96ad9f07f41f184c54cf771371` |
| Masukan | Bed Management: backend14, FE15, data21, contracts target, gate1.12, decision46. Riwayat metadata: `02-backend-architecture.md` rev `0.3`; `contracts/api-contract.md`, `contracts/validation-matrix.md`, `contracts/permission-audit-matrix.md` rev `0.3.0`; `erd/01-inpatient-episode.md` dan `data/data-dictionary.md` rev `0.3`; `00-interview-decisions.md` rev `5`; `evidence/03-hospital-domain-architecture.md` rev `0.1` (`DOMAIN_ARCHITECTURE_PARTIAL`); arahan scope produk 2026-09-03 untuk memasukkan deposit rawat inap; evidence legacy V1 `ApplicationDbContext.cs` dan `Program.cs`; arahan operasional 2026-09-08 atas layar `Input Deposit Rawat Inap` pada multi-step admisi; evidence frontend `inpatient-admission-flow-constants.jsx`, `inpatient-admission-payment-step.jsx`, `use-inpatient-admission-doctor.jsx`; evidence backend `BilDepositAccount.cs`, `BillingPatientFundsController.cs` |
| Ringkasan cakupan | Satu pasien dapat dirawat inap dari admisi sampai episode ditutup dan tempat tidur kembali kosong, **termasuk penetapan deposit di dalam multi-step admisi, top-up, dan settlement deposit melalui Billing/Kasir**, tanpa dokumentasi klinis, tanpa resep, dan tanpa jalur masuk IGD |

> **Catatan port 2026-09-08.** Revisi `0.5.0` dan `0.6.0` sempat disusun di luar pohon blueprint,
> pada `docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-deposit.md`, sehingga blueprint kanonik tertinggal di
> `0.4.1` tanpa deposit sama sekali. Berkas ini sekarang **memuat keduanya** dan menjadi satu-satunya
> sumber kebenaran; berkas di `docs/Modul-RS` ditandai `SUPERSEDED`. Pemeriksaan superset dijalankan
> sebelum port: nol judul bagian, nol `FR-RI-*`, nol `UAT-*`, dan nol `EPIC RI-*` milik `0.4.1` yang
> hilang.

**Koreksi pada `contract_version` `0.6.1`.** Trace ulang terhadap source hasil merge menemukan
tiga hal yang membuat `0.6.0` salah, bukan sekadar kurang lengkap.

| Yang dikoreksi | Buktinya | Akibatnya |
| --- | --- | --- |
| **Kolom `EpisodeId` pada akun deposit dibatalkan** | `BilDepositAccountConfiguration.cs:27` mengunci `EncounterId` **unique**, dan `InpEpisodeConfiguration.cs:26` juga mengunci `EncounterId` **unique**. Episode dan akun deposit karena itu sudah 1:1 lewat kunjungan | `FR-RI-163` tetap berlaku; **mekanismenya** berubah dari kolom baru menjadi join. Nol migration pada tabel finansial yang sudah berisi data |
| **Rute refund dibetulkan** | `BillingFinancialExceptionsController.cs:112` sudah menyediakan `POST /financial-exceptions/refunds` beserta `approve`, di bawah kontrak `BIL-API-0.4` yang **sudah disetujui** | Usulan `POST /deposits/episodes/{id}/refunds` pada `0.6.0` **dicabut**. Kontrak Rawat Inap yang mengalah, bukan kontrak Billing |
| **Klaim charge kamar dicabut** | `BillingCalculationService.cs:455-470` menghitung charge kamar hidup-hidup dari `InpBedPlacement` sesuai `BKC-DEC-043` | Bagian 15.3 dibetulkan; asumsi "Billing belum bisa apa-apa" tidak boleh dipakai lagi |

Satu temuan lagi tidak mengubah keputusan tetapi memperkecil pekerjaan: **idempotensi penerimaan
deposit sudah berjalan**. `BillingPatientFundsController.cs:99` menerima header `Idempotency-Key`,
`BilDepositMovementConfiguration.cs:31` menguncinya unique, dan `BillingDepositService.cs:76-80`
mengembalikan transaksi pertama saat kunci diulang. `FR-RI-166` karena itu sudah terpenuhi source,
tinggal dibuktikan lewat UAT.

**Perubahan pada `contract_version` `0.6.0`.** Peninjauan layar operasional 2026-09-08 menemukan selisih yang nyata: multi-step admisi rawat inap **sudah** mempunyai langkah `Input Deposit Rawat Inap` di antara langkah tipe pembayaran dan langkah Dokter, sedangkan revisi `0.5.0` hanya mengenal deposit sebagai aktivitas Billing/Kasir **sesudah** episode `Draft` ada. Empat arahan operasional berikut menutup selisih itu.

| Arahan operasional 2026-09-08 | Masuk ke |
| --- | --- |
| `OPS-2026-09-08/A` deposit menjadi **langkah tersendiri** pada multi-step admisi, tepat setelah langkah tipe pembayaran dan sebelum langkah Dokter | Bagian 4, 9, `FR-RI-174`, `FR-RI-178` |
| `OPS-2026-09-08/B` nilai minimum deposit berasal dari **kebijakan per penjamin dan kelas perawatan**, bukan angka tetap yang ditulis di layar | Bagian 5.1, `FR-RI-175` |
| `OPS-2026-09-08/C` deposit di bawah minimum **tidak menghentikan admisi**; layar hanya memberi peringatan | `FR-RI-176`, `UAT-42` |
| `OPS-2026-09-08/D` kekurangan minimum deposit ditagih **berkala** pada perawatan panjang, dan diperhitungkan pada pelunasan akhir untuk perawatan singkat | `FR-RI-177`, `UAT-44` |

Layar hari ini menuliskan "Deposit wajib diisi dan tidak boleh 0" beserta minimum Rp100.000.000 sebagai angka tetap. Keduanya **tidak** dikunci sebagai aturan produk: `FR-RI-164` tetap berlaku penuh, dan angka itu berpindah menjadi nilai kebijakan yang dibaca layar.

Dua koreksi teknis ikut masuk pada revisi ini.

**Pertama, endpoint deposit tidak dibuat dari nol.** `BillingManagement` sudah mempunyai `BilDepositAccount`, `BilDepositMovement`, `BillingDepositService`, dan `BillingPatientFundsController` dengan rute `api/v1/health-services/billing-management/billing/patient-funds/deposits/{encounterId}` beserta `/top-ups` dan `/allocations`. Usulan base URL `billing-management/inpatient-deposits` pada `0.5.0` **dicabut**, karena ia melahirkan ledger deposit kedua di modul yang sama. Yang dikerjakan `EPIC RI-35` adalah **memperluas** rute yang sudah ada, termasuk menambahkan `EpisodeId` pada akun deposit yang hari ini hanya mengenal `EncounterId`.

**Kedua, urutan langkah admisi tidak diubah demi kontrak.** Layar deposit berdiri sebelum episode dibuat, sedangkan `FR-RI-163` mewajibkan tiap transaksi terikat `EpisodeId`. Penyelesaiannya ada pada `FR-RI-178`: nominal deposit ditahan sebagai isian langkah selama admisi berjalan, dan transaksi uang baru dibentuk tepat setelah `POST /episodes` berhasil pada langkah Dokter — titik tulis yang memang sudah dipakai frontend hari ini.

> **Catatan tata kelola.** Keempat arahan di atas beserta kedua koreksi teknisnya belum mempunyai ID `RWI-DEC-*`. Sebelum development lock, seluruhnya wajib disalin ke `00-interview-decisions.md` bersama arahan scope 2026-09-03 yang juga masih menunggu ID.

**Perubahan pada `contract_version` `0.5.0`.** Arahan scope produk 2026-09-03 memindahkan **deposit pasien rawat inap** dari daftar kemampuan ditunda ke dalam MVP. Perubahan ini melahirkan `EPIC RI-35` dan `FR-RI-163` s.d. `FR-RI-173`. Deposit tetap **bukan milik entity `Inp`**: transaksi uang, kwitansi, alokasi, settlement, dan refund tetap dimiliki `BillingManagement`/Kasir; sub-modul `episode-rawat-inap` hanya menjadi konteks episode dan closure gate.

Revisi ini **tidak** otomatis memasukkan seluruh estimasi biaya, tagihan berjalan rinci, atau klaim ke MVP. Ketiganya tetap berada pada bagian 8. Yang masuk adalah slice minimum agar pasien rawat inap dapat menerima deposit, menambah deposit, menggunakan deposit pada final settlement, membayar kekurangan atau menerima refund, lalu memperoleh `FinancialClearance`.

> **Catatan tata kelola.** Arahan 2026-09-03 adalah keputusan scope yang dipakai untuk menyusun revisi ini, tetapi belum diberikan ID `RWI-DEC-*` pada sumber yang tersedia. Sebelum development lock, keputusan tersebut harus disalin ke `00-interview-decisions.md` dan kontrak API/permission/validation harus dinaikkan agar tidak ada kontrak yang tertinggal.

**Perubahan pada `contract_version` `0.3.0`.** Tiga keputusan penutupan butir organisasi
2026-08-21 masuk ke dokumen ini. **Satu kemampuan berpindah dari daftar ditunda ke dalam MVP**, dan
**satu epic baru lahir** — satu-satunya epic baru sejak dokumen ini disusun.

| Keputusan | Masuk ke |
| --- | --- |
| `RWI-DEC-064` jenis kelamin dan isolasi menjadi aturan yang **menolak** penempatan | Bagian 7 dan 8 (kemampuan berpindah), `EPIC RI-34` baru |
| `RWI-DEC-065` kebutuhan isolasi menjadi atribut episode | `EPIC RI-34`, `FR-RI-158` s.d. `FR-RI-161`, bagian 11, 13, dan 14 |
| `RWI-DEC-066` seluruh kamar tidak boleh ditempati campur, tanpa kolom baru pada `MstRoom` | `EPIC RI-34`, `FR-RI-154` s.d. `FR-RI-157` |

Satu gerbang keras sebelum produksi pada bagian 16 **dicabut** karena keputusannya sudah turun, dan
tiga pertanyaan memblokir pada bagian 20.2 tertutup.

**Perubahan pada `contract_version` `0.2.0`.** Empat keputusan Amendment Pass 2026-08-21 masuk ke
dokumen ini. Tidak ada kemampuan `MUST HAVE` yang dicabut dan tidak ada epic baru; keempatnya
menempel pada epic yang sudah ada.

| Keputusan | Masuk ke |
| --- | --- |
| `RWI-DEC-054` satu pasien satu episode | `EPIC RI-23`, `FR-RI-148` |
| `RWI-DEC-055` kepergian fisik pasien | `EPIC RI-28`, `FR-RI-149` s.d. `FR-RI-151` |
| `RWI-DEC-056` hubungan bayi dan ibu | `EPIC RI-33`, `FR-RI-152` |
| `RWI-DEC-057` versi resume pulang | `EPIC RI-27`, `FR-RI-153` |

**Penomoran.** Dokumen ini memakai rentang nomor **baru** — `EPIC RI-21` ke atas, `FR-RI-101` ke
atas — supaya tidak mendaur ulang nomor yang sudah dipakai PRD asli (`docs/Modul-RS/PRD-Modul-Rawat-Inap.md`)
untuk isi yang berbeda.

---

## 2. Ringkasan eksekutif

Rumah sakit hari ini **tidak dapat merawat inap satu pasien pun lewat sistem**. Tidak ada catatan
siapa menempati tempat tidur mana, tidak ada daftar pasien yang sedang dirawat, dan tidak ada cara
menutup perawatan selain mengubah data master secara manual.

MVP ini menyelesaikan satu hal: **membuat satu perjalanan rawat inap benar-benar bisa berjalan dari
awal sampai akhir tanpa ada petugas yang harus mengubah database.** Dari petugas admisi membuka
admisi, memesan tempat tidur, menempatkan pasien; perawat dan dokter berganti penanggung jawab;
pasien pindah kamar bila perlu; **deposit awal maupun tambahan dicatat oleh Billing/Kasir bila diperlukan**; sampai DPJP menyatakan boleh pulang, resume ditandatangani, deposit direkonsiliasi terhadap tagihan final, kasir menyelesaikan kekurangan atau refund, episode ditutup, dan tempat tidur kembali kosong untuk pasien berikutnya.

Yang **belum** dikerjakan MVP ini: menulis pengkajian dan catatan dokter, membuat resep, menerima
pasien dari IGD, dan mengirim data ke SATUSEHAT. Keempatnya bukan karena tidak penting.

> **Dikoreksi 2026-09-02 — dokumentasi klinis tidak lagi ditahan `DEC-INP-001`.** `DEC-INP-001`
> hanya menanyakan persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement`, dan
> persetujuan itu **sudah diberikan** 2026-08-21 lewat `RWI-DEC-062` yang menutup `RWI-OQ-032`.
> Kalimat sebelumnya di sini masih menyatakannya terbuka; itu **basi** dan diperbaiki sekarang.
> Sejak `RWI-DEC-080`, menulis pengkajian, catatan dokter, dan resep **masuk scope modul** dan
> berpindah pemilik ke dua sub-modul lain: `keperawatan/` dan `dokter-rawat-inap/` — lihat
> [`02-module-map.md`](../02-module-map.md). Keduanya belum dirancang, sehingga ketiga kemampuan
> itu tetap **di luar MVP dokumen ini**. Bedanya sekarang: karena **belum dirancang dan bukan
> milik sub-modul ini**, bukan karena keputusan bisnisnya belum turun.

Yang benar-benar masih ditahan keputusan yang belum turun tinggal empat: menerima pasien dari IGD
(`DEC-INP-002`), persetujuan umum (`DEC-INP-003`), pengiriman SATUSEHAT (`DEC-INP-005`), serta
`DEC-INP-006` serah terima klinis antar shift dan `DEC-INP-007` cara pulang meninggal dan kabur.

**Sejak `0.5.0`, deposit menjadi bagian dari perjalanan MVP.** Kebutuhan deposit tidak di-hardcode untuk semua pasien; Billing/Kasir menentukannya berdasarkan kebijakan finansial dan penjamin. Bila deposit diperlukan, setiap penerimaan dan top-up menjadi transaksi baru dengan kwitansi sendiri. Saat pasien pulang, saldo deposit direkonsiliasi terhadap tagihan final: kekurangan dibayar, kelebihan menjadi refund, dan `FinancialClearance` tidak dapat menjadi `Cleared` selama settlement belum selesai.

**Sejak `0.6.0`, deposit tidak lagi menunggu kasir sesudah admisi selesai.** Ia berdiri sebagai langkah tersendiri di dalam multi-step admisi, tepat setelah petugas memilih cara bayar dan kelas perawatan, karena begitulah rumah sakit menjalankannya hari ini. Nominal di bawah minimum kebijakan tetap diterima disertai peringatan — admisi tidak pernah berhenti karena uang muka kurang — dan kekurangannya ditagih berkala bila perawatan berlangsung lama.

**Sejak `0.3.0`, MVP juga menolak penempatan yang tidak layak.** `DEC-INP-004` turun pada
2026-08-21: jenis kelamin dan kebutuhan isolasi tidak lagi sekadar menyaring hasil pencarian,
melainkan **menolak** penempatan dan perpindahan. Sistem tidak akan pernah menempatkan laki-laki di
kamar yang sedang dihuni perempuan, dan tidak akan pernah menempatkan pasien yang membutuhkan
isolasi di tempat tidur biasa — walaupun petugas memaksa.

---

## 3. Masalah produk

### 3.1 Yang sudah ada

| Yang sudah ada | Bukti kode |
| --- | --- |
| Master tempat tidur lengkap, termasuk penanda isolasi, jenis kelamin, boks bayi, dan dapat dipesan | `Areas/HealthServices/MasterData/Models/MstBed.cs:27-41` |
| Nilai `Reserved` dan `Occupied` sudah ada pada enum | `Areas/HealthServices/MasterData/Enums/BedStatus.cs:8` |
| Pencarian tempat tidur dengan 13 penyaring dan ringkasan jumlah | `Areas/HealthServices/MasterData/Controllers/BedController.cs:104-220` |
| Kunjungan sudah mengenal tipe rawat inap | `Areas/HealthServices/RegistrationManagement/Enums/EncounterType.cs:8` |
| Kelas pasien sudah punya penanda rawat inap dan tarif kamar harian | `Areas/HealthServices/MasterData/Models/MstPatientClass.cs:33,47` |
| Mesin hak akses yang mendaftarkan butir hak secara otomatis | `Seeders/AccessMenuSeeder.cs:22-60` |

### 3.2 Yang belum ada

| Yang belum ada | Bukti |
| --- | --- |
| Modul Rawat Inap | Tidak ada folder `Areas/HealthServices/InPatientManagement/`; tidak ada satu pun berkas berawalan `Inp` |
| Catatan penghunian tempat tidur | Dari 446 `DbSet`, tidak satu pun berkaitan dengan penempatan pasien |
| Mesin penggerak status tempat tidur | Satu-satunya penulis `MstBed.BedStatus` adalah CRUD master data |
| Daftar pasien dirawat | Tidak ada endpoint, view, maupun query census |
| Layar Rawat Inap di frontend | Tidak ada satu pun route; menu hanya mengenal Rawat Jalan dan IGD |
| ~~Kemampuan transaksi Billing~~ — **baris ini basi, dikoreksi 2026-09-08** | Pada peninjauan 2026-09-08, `BillingManagement` sudah punya lima controller (`invoices`, `finalizations`, `patient-funds`, `settlements`, `financial-exceptions`), lima belas service termasuk `BillingDepositService`, `BillingSettlementService`, dan `BillingRefundService`, serta master `MstRoomChargePolicy` dan `MstPaymentMethod`. Yang benar-benar belum ada untuk rawat inap tinggal **pengikatan `EpisodeId`** pada akun deposit dan **kebijakan minimum deposit** |

**Evidence legacy V1, bukan bukti implementasi V2.** Lampiran backend lama menunjukkan `DepositRanap` dipetakan dengan `NoKwitansi` wajib dan unique, tersedia `DbSet<DepositRanap>` serta `DbSet<DepositPersentase>`, dan `Program.cs` mendaftarkan `IPerkiraanBillingRanapService` serta `IDepositRanapNumberService`. Artinya konsep deposit rawat inap pernah ada pada V1 dan layak menjadi referensi migrasi, tetapi schema V1 **tidak otomatis dipakai ulang** untuk target V2. `EPIC RI-35` tetap diklasifikasikan sebagai cross-module `MISSING / EXTEND` sampai kontrak Billing V2 ditetapkan.

### 3.3 Akibatnya hari ini

Petugas menempatkan Tn. Budi di tempat tidur `BD-RSMMC-00042`. Sistem tidak punya tempat untuk
menyimpan fakta itu. Yang bisa dilakukan hanya mengubah kolom status tempat tidur menjadi
`Occupied` lewat menu master data — tanpa jejak siapa yang menempati dan sejak kapan. Bila lupa
dikembalikan, kamar terlihat penuh selamanya padahal kosong.

Pada sisi finansial, PRD sebelumnya juga tidak menyediakan perjalanan deposit. Bila keluarga membayar uang muka Rp5.000.000 lalu menambah Rp3.000.000 selama perawatan, MVP lama tidak mempunyai contract untuk menyimpan dua transaksi itu sebagai penerimaan yang terpisah, tidak mempunyai ringkasan saldo deposit per episode, dan tidak dapat membuktikan bagaimana deposit tersebut dipakai atau dikembalikan saat final settlement.

---

## 4. Visi produk

Rantai keterhubungan data yang ingin dicapai, ditulis sebagai urutan:

1. Pasien terdaftar → **kunjungan** bertipe rawat inap dibuat atau dipakai.
2. Kunjungan → **episode rawat inap** dibuka, satu kunjungan tepat satu episode.
3. Kunjungan → **kategori pembayaran, penjamin, dan kelas perawatan** dipilih; kombinasi itu menentukan **kebijakan deposit** yang berlaku — perlu atau tidak, dan berapa minimumnya.
4. Kebijakan → **nominal deposit ditetapkan pada langkah admisi tersendiri**, bukan pada layar terpisah sesudah admisi. Nominal di bawah minimum diterima disertai peringatan.
5. Episode → **DPJP** ditetapkan, berbentuk riwayat berperiode. Begitu `EpisodeId` terbentuk, nominal dari langkah Deposit menjadi **penerimaan pembayaran** bernomor kwitansi unik; pembayaran berikutnya menjadi **top-up baru**, bukan mengubah transaksi lama.
6. Episode → **pemesanan tempat tidur**, berlaku 2 jam.
7. Episode → **kebutuhan isolasi** direkam petugas admisi atau diputuskan DPJP.
8. Pemesanan → **Kelayakan Penempatan** diperiksa: tempat tidur, jenis kelamin, pencampuran kamar, dan isolasi → **penempatan tempat tidur**, dan episode menjadi aktif.
9. Penempatan → **census** menjawab siapa dirawat, di mana, oleh siapa, sudah berapa hari.
10. Penempatan → **perpindahan**, membentuk riwayat lokasi dan riwayat kelas.
11. Episode → **perawat penanggung jawab**, juga berbentuk riwayat.
12. Selama perawatan → Billing/Kasir dapat membuat **permintaan top-up deposit**; seluruh penerimaan tetap terikat pada episode yang sama.
13. Episode → **keputusan pulang** oleh DPJP → **resume pulang** ditandatangani.
14. Billing/Kasir → **final settlement**: tagihan final dikurangi deposit yang dapat dialokasikan; kekurangan dibayar atau kelebihan dicatat sebagai refund.
15. Episode → **daftar periksa administrasi** dan **kelayakan keuangan**; `Cleared` hanya bila settlement finansial selesai atau ditutup lewat override supervisor yang diaudit.
16. Episode → **penutupan**, dan tempat tidur kembali kosong.
17. Seluruh langkah di atas → **riwayat status** yang tidak dapat diubah; transaksi uang tetap diaudit di Billing/Kasir.
---

## 5. Batas MVP

### 5.1 Titik mulai

1. Pasien sudah terdaftar pada modul Patient Management.
2. Master unit layanan, kamar, tempat tidur, dan kelas pasien sudah terisi lewat layar aplikasi, **beserta penanda jenis kelamin, isolasi, dan boks bayi pada tiap tempat tidur**. Sejak `0.3.0` penanda itu bukan lagi sekadar penyaring pencarian, melainkan penentu diterima atau ditolaknya penempatan, sehingga isian yang salah akan menolak penempatan yang sah.
3. Petugas admisi membuka layar admisi rawat inap.
4. Kebijakan deposit per penjamin dan kelas perawatan sudah terisi: perlu atau tidaknya deposit, nominal minimum, dan ambang hari tindak lanjut. Selama kebijakan itu kosong, langkah Deposit tampil tanpa minimum dan seluruh nominal diterima tanpa peringatan.

### 5.2 Titik akhir

1. Episode berstatus `Closed`.
2. Tempat tidur yang tadinya ditempati terbaca `Available` pada pencarian berikutnya.
3. Riwayat status episode lengkap dan dapat ditelusuri urut.
4. Resume pulang tersimpan dan tertandatangani.
5. Episode muncul pada laporan pengecualian bila ditutup menembus gerbang keuangan.
6. Bila episode memiliki deposit, seluruh deposit sudah direkonsiliasi: tidak ada kekurangan yang belum dibayar dan tidak ada kelebihan yang belum dibuatkan transaksi refund/penyelesaian finansial.

---

## 6. Pelaku sasaran

| Pelaku | Tanggung jawabnya di dalam MVP |
| --- | --- |
| Petugas admisi | Membuka admisi, **memasukkan nominal deposit pada langkah Deposit**, merekam kebutuhan isolasi sebagai catatan awal selagi episode `Draft`, memesan dan menempatkan tempat tidur, menandai butir administrasi, menutup episode |
| Perawat pelaksana | Melihat census, memindahkan pasien |
| Kepala ruangan | Menugaskan perawat penanggung jawab, mengalihkan DPJP, memindahkan pasien, menindaklanjuti daftar pantau |
| DPJP | Menetapkan dan memperbarui kebutuhan isolasi sebagai keputusan klinis, memindahkan pasien yang menjadi tanggung jawabnya, menyatakan pasien boleh pulang, menyusun dan menandatangani resume |
| Petugas kasir atau billing | Menentukan kebutuhan deposit sesuai kebijakan finansial, membuat permintaan deposit/top-up, menerima pembayaran, menerbitkan kwitansi, melakukan final settlement dan refund bila ada, lalu menandai kelayakan keuangan setelah validasi sistem |
| Supervisor | Membatalkan admisi setelah pasien dirawat, menutup menembus gerbang keuangan, membuka sesi koreksi |
| Admin master data | Mengisi master kamar dan tempat tidur, mengatur batas waktu dan butir administrasi |

---

## 7. Pemilihan kemampuan MVP

Uji yang dipakai untuk setiap kemampuan: *tanpa ini, apakah satu kasus nyata bisa selesai dari awal
sampai akhir?* dan *kalau tidak bisa, apakah ada jalan sementara yang aman dan tetap dapat diaudit?*

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Memilih pasien terdaftar untuk dirawat | `RWI-CAP-001` | Wajib; tanpa ini tidak ada yang bisa dirawat |
| Menentukan penjamin saat masuk | `RWI-CAP-002` | Wajib; menjadi konteks kelayakan keuangan |
| Menentukan DPJP | `RWI-CAP-003` | Wajib; kewenangan pulang dan perpindahan bergantung padanya |
| Mencari tempat tidur tersedia | `RWI-CAP-004` | Wajib; tanpa ini petugas tidak tahu ke mana pasien ditempatkan |
| Pemesanan tempat tidur dan kedaluwarsanya | `RWI-CAP-006` | Wajib; tanpa ini dua petugas merebut tempat tidur yang sama |
| Penempatan pasien pada tempat tidur | `RWI-CAP-007` | Wajib; tanpa ini pasien tidak punya lokasi |
| Episode beserta model statusnya | `RWI-CAP-008` | Wajib; seluruh catatan lain menempel padanya |
| Census pasien dirawat | `RWI-CAP-012` | Wajib; tanpa ini perawat tidak tahu siapa saja yang dirawat |
| Perhitungan lama dirawat | `RWI-CAP-013` | Wajib; dipakai census dan resume |
| Penugasan perawat penanggung jawab | `RWI-CAP-014` | Wajib; daftar pantau butuh orang yang jelas untuk ditagih |
| Perpindahan dan pindah kelas | `RWI-CAP-017` | Wajib; kamar penuh dan perubahan kondisi adalah kejadian sehari-hari |
| Resume pulang | `RWI-CAP-025` | Wajib; syarat penutupan episode |
| Daftar periksa administrasi | `RWI-CAP-028` | Wajib; syarat penutupan episode |
| Kelayakan keuangan | `RWI-CAP-027` | Wajib; syarat penutupan episode. Sejak `0.5.0`, `Cleared` harus tervalidasi terhadap settlement Billing/Kasir, bukan penandaan buta; lihat bagian 15 |
| Penutupan episode dan pelepasan tempat tidur | `RWI-CAP-029` | Wajib; tanpa ini tempat tidur tidak pernah kembali kosong |
| Pencatatan kepergian fisik pasien | `RWI-CAP-029` | Wajib; tanpa ini tempat tidur tertahan berjam-jam setelah pasien pulang. Ditambahkan pada `0.2.0` |
| Satu pasien satu episode aktif | `RWI-CAP-008` | Wajib; tanpa ini satu pasien bisa tercatat dirawat di dua tempat. Ditambahkan pada `0.2.0` |
| Riwayat versi resume pulang | `RWI-CAP-025` | Wajib; koreksi resume adalah amandemen rekam medis dan harus dapat ditelusuri. Ditambahkan pada `0.2.0` |
| Penanda bayi dirawat gabung dengan ibunya | `RWI-CAP-032` | Wajib; menyangkut kepastian identitas. Ditambahkan pada `0.2.0` |
| Sesi koreksi episode | `RWI-CAP-030` | Wajib; tanpa ini kesalahan cara pulang tidak dapat dibetulkan sama sekali |
| Riwayat status episode | `RWI-CAP-037` | Wajib; sumber data laporan pengecualian dan daftar pantau |
| Daftar pantau | `RWI-CAP-039` | Wajib; dua dari tiga daftar tersedia pada MVP |
| Pengaturan yang dapat diubah admin | `RWI-CAP-034` | Wajib; angka tidak boleh ditanam di kode |
| Boks bayi sebagai tempat tidur | `RWI-CAP-032` | Wajib; masternya sudah siap, tinggal dipakai |
| Hak akses per peran | `RWI-CAP-035` | Wajib; dipakai ulang dari mesin yang sudah ada |
| Kewenangan per pasien untuk DPJP | `RWI-CAP-036` | Wajib; `RWI-DEC-023` dan `RWI-DEC-024` menuntutnya |
| Penolakan penempatan karena jenis kelamin dan isolasi | `RWI-CAP-033` | Wajib; tanpa ini sistem ikut menyebabkan pelanggaran privasi dan pengendalian infeksi. **Berpindah dari daftar ditunda pada `0.3.0`** lewat `RWI-DEC-064` |
| Kebutuhan isolasi sebagai atribut episode | `RWI-CAP-033` | Wajib; aturan di atas tidak dapat dijalankan tanpa tempat menyimpan datanya. Ditambahkan pada `0.3.0` lewat `RWI-DEC-065` |
| Deposit pasien rawat inap | `RWI-CAP-027` sebagian | **Wajib sejak `0.5.0`**; memungkinkan penerimaan uang muka yang terikat episode tanpa membuat entity finansial di `Inp` |
| Top-up dan ringkasan saldo deposit | `RWI-CAP-027` sebagian | **Wajib sejak `0.5.0`**; satu episode dapat memiliki banyak transaksi deposit yang semuanya tetap dapat ditelusuri |
| Settlement deposit saat pulang | `RWI-CAP-027` sebagian | **Wajib sejak `0.5.0`**; deposit harus dialokasikan terhadap tagihan final, kekurangan dibayar, dan kelebihan diselesaikan sebagai refund sebelum clearance normal |
| Penetapan deposit di dalam multi-step admisi | `RWI-CAP-027` sebagian | **Wajib sejak `0.6.0`**; layar admisi operasional sudah memilikinya, dan tanpa langkah ini nominal deposit tidak pernah masuk sistem pada saat pasien benar-benar membayar |

---

## 8. Kemampuan yang ditunda

Setiap baris menyebut **alasan bersebab** dan **pengganti selama MVP berjalan**.

> **Satu baris keluar dari daftar ini pada `0.3.0`.** "Penolakan penempatan karena isolasi atau
> jenis kelamin" sebelumnya ditunda karena `DEC-INP-004` belum turun. Keputusannya turun 2026-08-21
> lewat `RWI-DEC-064` sampai `RWI-DEC-066`, sehingga kemampuan itu **masuk MVP** sebagai
> `EPIC RI-34`. Daftar ini kini berisi sembilan baris, bukan sepuluh.
>
> **Perubahan `0.5.0`.** Deposit keluar dari baris finansial yang ditunda dan masuk MVP sebagai `EPIC RI-35`. Estimasi biaya otomatis, tagihan berjalan rinci, dan klaim **tetap ditunda**, sehingga jumlah baris pada tabel ini tetap sembilan.

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Pengkajian awal, catatan dokter, CPPT, tindakan, visite | `RWI-CAP-015`, `018`, `019`, `023`, `024` — pada penomoran PRD final: `CAP-012`, `CAP-014`, `CAP-020` s.d. `CAP-022`, `CAP-024`, `CAP-025` | **Bukan lagi karena `DEC-INP-001`.** Persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement` sudah diberikan 2026-08-21 lewat `RWI-DEC-062`. Sejak `RWI-DEC-080` kemampuan ini **masuk scope modul**, dan `RWI-DEC-083` memberikannya kepada sub-modul `keperawatan/` serta `dokter-rawat-inap/` yang **belum dirancang**. Ia keluar dari MVP dokumen ini karena bukan lagi milik sub-modul ini | Dokumentasi klinis tetap ditulis di luar sistem sebagaimana hari ini. Sub-modul ini menyediakan riwayat lokasi, riwayat DPJP, dan resume, sehingga rekam medis tetap punya kerangka waktunya. Penghalang yang tersisa bersifat teknis, bukan keputusan bisnis: *shared inpatient clinical context resolver*, `PRD-RWI-FINAL-001` bagian 30.3 |
| Resep rawat inap dan obat pulang | `RWI-CAP-021`, `RWI-CAP-022` — pada penomoran PRD final: `CAP-023` | Sama seperti baris di atas: **bukan** `DEC-INP-001`, melainkan karena `RWI-DEC-083` memberikan `CAP-023` kepada sub-modul `dokter-rawat-inap/` yang belum dirancang. Mesin pemenuhan resepnya tetap milik `PharmacyManagement` | Butir "obat pulang sudah diserahkan" tersedia pada daftar periksa administrasi dan **ditandai manual** petugas admisi |
| Serah terima IGD ke rawat inap | `RWI-CAP-038` | Menentukan kunjungan mana yang menjadi jangkar episode; menyentuh modul IGD — `DEC-INP-002`, yang pemiliknya bernama sejak `RWI-DEC-069`: Rizki Gunawan | Petugas admisi membuka admisi rawat inap secara manual untuk pasien yang datang dari IGD, memakai jalur pasien datang langsung |
| Persetujuan umum rawat inap | `RWI-CAP-031` | Keputusan hukum dan privasi, pemiliknya belum ditunjuk — `DEC-INP-003` | Persetujuan tetap dikumpulkan di atas kertas seperti hari ini. Butir daftar periksa dapat ditambahkan admin bila diinginkan |
| Pengiriman SATUSEHAT | Belum punya ID kemampuan | Belum pernah dibahas; pemilik dan isi kiriman belum ditentukan — `DEC-INP-005` | Data disimpan dalam bentuk riwayat yang dapat dibaca ulang, sehingga pengiriman kelak tinggal membaca |
| Serah terima klinis antar shift | Belum punya ID kemampuan | Ditandai `SAFETY_CHECK` oleh baseline; belum pernah dibahas — `DEC-INP-006` | Serah terima tetap dilakukan lisan dan tertulis di luar sistem. Modul mencatat siapa perawat penanggung jawab dan sejak kapan |
| Cara pulang meninggal dan kabur | Bagian `RWI-CAP-026` | Sisi klinisnya masih terbuka — `DEC-INP-007` | Tiga cara pulang lain tersedia. Untuk dua kasus ini, episode ditutup lewat jalur supervisor disertai alasan, dan tercatat pada laporan pengecualian |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bagian `RWI-CAP-039` | Bergantung pada dokumentasi klinis, yang sejak `RWI-DEC-083` dimiliki sub-modul `keperawatan/` dan `dokter-rawat-inap/`. **Bukan** `DEC-INP-001`, yang sudah tertutup `RWI-DEC-062` 2026-08-21 | Dua daftar pantau lain tersedia |
| Estimasi biaya otomatis, tagihan berjalan rinci, klaim | `RWI-CAP-027` sebagian | Full billing engine dan claim engine belum menjadi bagian dari sub-modul ini. **Deposit tidak lagi termasuk baris ini sejak `0.5.0`** | Deposit dan settlement minimum dikerjakan `EPIC RI-35`. Estimasi/tagihan rinci tetap dapat dikembangkan pada BillingManagement; data lama dirawat dan riwayat kelas tetap disimpan agar charge kamar dapat direkonstruksi |

---

## 9. Alur bisnis target

`FLOW-RI-MVP-001` — Satu pasien dirawat inap dari masuk sampai pulang.

1. Petugas admisi memilih pasien yang sudah terdaftar.
2. Sistem membuat kunjungan bertipe rawat inap, atau memakai kunjungan poliklinik yang sudah ada.
3. Petugas memilih kategori pembayaran, penjamin, dan kelas perawatan pada langkah **Pembayaran**. Kombinasi penjamin dan kelas itu menentukan kebijakan deposit yang berlaku: perlu atau tidak, berapa minimumnya, dan berapa ambang hari tindak lanjutnya.
4. Pada langkah **Deposit** — langkah tersendiri tepat sesudah langkah Pembayaran dan sebelum langkah Dokter — petugas memasukkan nominal deposit yang diterima. Layar menampilkan minimum menurut kebijakan tadi. Nominal di bawah minimum **diterima disertai peringatan** dan tidak menghentikan admisi. Bila kebijakan menyatakan deposit tidak diperlukan, langkah ini dilewati tanpa membuat transaksi bernilai nol.
5. Petugas memilih unit layanan dan DPJP. Sistem membuat kunjungan lalu episode berstatus `Draft`. Tepat setelah `EpisodeId` terbentuk, nominal dari langkah Deposit dikirim sebagai transaksi penerimaan dengan kwitansi unik dan `idempotencyKey`. Bila pembuatan episode gagal, tidak ada transaksi uang yang terbentuk dan isian deposit tetap utuh di layar.
6. Bila surat rujukan menyebut kebutuhan isolasi, petugas admisi merekamnya sebagai catatan awal selagi episode masih `Draft`. Kebijakan yang mensyaratkan deposit lunas sebelum penempatan tetap dimungkinkan, tetapi ia dimiliki domain finansial dan tidak di-hardcode oleh modul Rawat Inap.
7. Petugas mencari tempat tidur kosong, lalu memesannya. Hasil pencarian sudah tersaring oleh kedelapan aturan Kelayakan Penempatan. Tempat tidur terbaca `Reserved` selama 2 jam.
8. Pasien sampai di kamar. Petugas menekan konfirmasi masuk. Kelayakan Penempatan diperiksa **ulang** di sini — jenis kelamin, pencampuran kamar, dan isolasi termasuk di dalamnya. Bila salah satu gagal, penempatan ditolak dan isian admisi tetap utuh. Bila lolos, episode menjadi `Admitted`, tempat tidur `Occupied`, dan pasien muncul pada census.
9. Kepala ruangan menugaskan perawat penanggung jawab.
10. Bila DPJP kemudian mengubah kebutuhan isolasi, perubahannya diterima seketika. Bila tempat tidur yang sedang ditempati jadi tidak sesuai, episode muncul pada daftar pantau penempatan tidak sesuai sampai pasien dipindahkan.
11. Bila kamar perlu berganti, kepala ruangan, perawat, supervisor, atau DPJP memindahkan pasien. Kelayakan Penempatan diperiksa dengan aturan yang sama persis seperti penempatan awal. Penempatan lama ditutup dan yang baru dibuka dalam satu tindakan utuh.
12. Bila DPJP berhalangan, kepala ruangan atau supervisor mengalihkan tanggung jawab DPJP disertai alasan.
13. Selama episode aktif, Billing/Kasir dapat membuat permintaan **top-up**. Pembayaran top-up selalu membuat transaksi baru; transaksi deposit sebelumnya tidak diubah dan seluruh kwitansi tetap dapat dibaca. Bila deposit yang sudah diterima masih di bawah minimum kebijakan dan lama rawat melewati ambang tindak lanjut — bawaan tiga hari, dapat diubah admin — episode muncul pada daftar pantau kekurangan deposit sebagai penagihan pelunasan berkala. Perawatan yang lebih singkat dari ambang itu tidak ditagih terpisah; kekurangannya langsung diperhitungkan pada pelunasan akhir di langkah 17.
14. DPJP menyatakan pasien boleh pulang dan memilih cara pulangnya. Episode menjadi `DischargePending`. Tempat tidur **belum** dilepas.
15. DPJP menyusun resume pulang lalu menandatanganinya.
16. Petugas admisi menandai butir daftar periksa administrasi.
17. Billing/Kasir menjalankan **final settlement**. Sistem membaca tagihan final dari BillingManagement dan menghitung posisi deposit episode: total diterima, total yang dialokasikan, saldo yang tersisa, kekurangan pembayaran, dan refund yang harus diberikan.
18. Bila deposit lebih kecil dari tagihan final, pasien/penanggung membayar kekurangan. Bila deposit lebih besar, Billing/Kasir membuat transaksi refund/penyelesaian kelebihan. Tidak ada transaksi deposit lama yang dihapus atau ditimpa.
19. Setelah settlement selesai, kasir menandai kelayakan keuangan `Cleared`. Service memvalidasi ringkasan Billing terlebih dahulu; permintaan `Cleared` ditolak bila masih ada kekurangan atau refund yang belum diselesaikan.
20. Keluarga menjemput dan pasien meninggalkan kamar. Petugas ruangan mencatat kepergiannya. Tempat tidur **langsung bebas** dan boleh dipesan pasien berikutnya, walaupun episodenya belum ditutup.
21. Petugas admisi menutup episode. Episode menjadi `Closed`.
22. Bila kelayakan keuangan belum `Cleared` sementara pasien harus segera pulang, supervisor dapat menutup episode disertai alasan. Episode ditandai dan masuk laporan pengecualian; transaksi deposit/utang/refund yang belum selesai **tidak dihapus** oleh override tersebut.
23. Bila kemudian ditemukan kesalahan catatan, supervisor membuka sesi koreksi, membetulkan, lalu menutup sesinya. Status episode tetap `Closed` sepanjang sesi, dan versi resume sebelumnya tersimpan.
---

## 10. Epic dan functional requirement

### `EPIC RI-21` — Fondasi episode dan data master

**Tujuan:** menyediakan tabel, master, dan mesin status sehingga episode dapat hidup.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-101` — Episode menempel pada tepat satu kunjungan**
> Sistem menolak pembuatan episode kedua pada kunjungan yang sudah punya episode.
> **Contoh:** kunjungan `ENC-2026-09-000456` sudah punya episode `RI-2026-09-000123`. Percobaan
> membuka admisi kedua pada kunjungan itu ditolak dengan pesan "Kunjungan ini sudah punya episode
> rawat inap" dan kode 409.

> **`FR-RI-102` — Model status episode terkunci lima nilai**
> Hanya `Draft`, `Admitted`, `DischargePending`, `Closed`, dan `Cancelled` yang diterima.
> **Contoh:** permintaan yang mengirim nilai `InCare` ditolak. Tidak ada endpoint yang menerima
> status bebas.

> **`FR-RI-103` — Setiap perpindahan status meninggalkan tepat satu baris riwayat**
> **Contoh:** episode yang berjalan `Draft` → `Admitted` → `DischargePending` → `Closed`
> meninggalkan empat baris riwayat bernomor urut 1 sampai 4, masing-masing dengan pelaku, waktu,
> dan alasan.

> **`FR-RI-104` — Data master awal tersedia**
> Satu baris pengaturan berkode `DEFAULT` dan tiga butir daftar periksa administrasi.
> **Contoh:** `BedReservationMinutes` bernilai 120, dan butir `ADM-DOC`, `RETURN-ITEM`,
> `DISCHARGE-MED` ada, dengan `DISCHARGE-MED` bertanda tidak wajib.

### `EPIC RI-22` — Pencarian dan pemesanan tempat tidur

**Tujuan:** petugas dapat menemukan tempat tidur kosong dan menguncinya sementara.
**Disposisi backend:** `EXTEND` — master tempat tidur sudah ada, pemesanan belum

> **`FR-RI-105` — Pencarian menyembunyikan tempat tidur yang sedang dipesan**
> **Contoh:** `BD-RSMMC-00042` dipesan pukul 09:15. Pencarian pukul 09:20 tidak memuatnya.

> **`FR-RI-106` — Pemesanan gugur sendiri setelah batas waktu, dihitung saat dibaca**
> **Contoh:** pemesanan pukul 09:15 dengan batas 120 menit. Pembacaan pukul 11:14 masih mengunci;
> pembacaan pukul 11:16 sudah `Available`. Tidak ada proses latar belakang yang dijalankan.

> **`FR-RI-107` — Batas waktu dapat diubah admin dan langsung berlaku**
> **Contoh:** admin mengubah 120 menjadi 180 menit pukul 14:00. Pemesanan pukul 14:05 berlaku
> sampai 17:05. Pemesanan yang dibuat pukul 13:30 tetap memakai batas lama.

> **`FR-RI-108` — Satu tempat tidur hanya boleh punya satu pemesanan aktif**
> **Contoh:** episode A memesan `BD-RSMMC-00042`. Episode B memesan tempat tidur yang sama dan
> ditolak dengan kode 409.

### `EPIC RI-23` — Penempatan pasien dan pengaktifan episode

**Tujuan:** pasien punya lokasi yang tercatat, dan tempat tidur ganda mustahil terjadi.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-109` — Pencegahan tempat tidur ganda**
> Sistem menolak penempatan pasien ke tempat tidur yang sedang ditempati.
> **Contoh:** Sdri. Wati menempatkan Tn. Budi ke `BD-RSMMC-00042` pukul 09.00.01. Pada saat hampir
> bersamaan Sdri. Rina menempatkan Ny. Sari ke tempat tidur yang sama. Permintaan Sdri. Rina
> ditolak dengan pesan "Tempat tidur BD-RSMMC-00042 sudah ditempati pasien lain" dan kode 409.
> Tidak ada satu pun data penempatan ganda yang tersimpan.

> **`FR-RI-110` — Penempatan menutup pemesanan dan mengubah salinan status tempat tidur dalam satu transaksi**
> **Contoh:** bila penulisan salinan status gagal, catatan penempatan juga tidak tersimpan, dan
> episode tetap `Draft`.

> **`FR-RI-111` — Pemesanan yang gugur tidak menghalangi penempatan bila tempat tidur masih kosong**
> **Contoh:** pemesanan Ny. Sari gugur pukul 11:15. Ny. Sari sampai pukul 11:40. Karena tempat
> tidur masih kosong, penempatan tetap berhasil tanpa peringatan apa pun.

> **`FR-RI-112` — Penolakan penempatan tidak menghapus isian admisi**
> **Contoh:** penempatan ditolak karena tempat tidur diambil pasien lain. Episode tetap `Draft`,
> dan penjamin, DPJP, serta kelas yang sudah diisi tetap tersimpan.

> **`FR-RI-148` — Satu pasien satu episode yang benar-benar hadir**
> Sistem menolak menempatkan pasien yang sudah punya episode berjalan.
> **Contoh:** Tn. Budi sedang dirawat di Melati 3B. Pukul 14:00 petugas lain mencoba
> menempatkannya di Anggrek 1A karena mengira ia pasien baru. Ditolak dengan pesan "Tn. Budi sudah
> dirawat pada episode RI-2026-09-000123 di Melati 3B" dan kode 409, sehingga petugas langsung tahu
> bahwa yang dibutuhkan adalah perpindahan.
>
> Sebaliknya, bila Tn. Budi sudah pulang pukul 10:15 dan kepergiannya sudah dicatat, lalu ia
> kembali pukul 12:00 dengan keluhan baru, admisi barunya **diterima** walaupun episode lama belum
> ditutup.

### `EPIC RI-24` — Census dan lama dirawat

**Tujuan:** perawat dan admisi tahu siapa dirawat, di mana, dan sudah berapa hari.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-113` — Census menampilkan pasien `Admitted` dan `DischargePending` saja**
> **Contoh:** dari lima episode berstatus berbeda, census memuat tepat dua.

> **`FR-RI-114` — Lama dirawat dihitung dari selisih tanggal dengan hasil paling sedikit 1 hari**
> **Contoh:** masuk 21 September pukul 22:30, pulang 22 September pukul 06:00. Selisih jamnya 7,5
> jam, tetapi lama dirawat tercatat **1 hari**, bukan 0 hari.

> **`FR-RI-115` — Lama dirawat bertambah pada pergantian tanggal**
> **Contoh:** pasien masuk 21 September pukul 22:30. Pada 22 September pukul 00:30 lama dirawat
> sudah bernilai 1, bukan menunggu sampai 22 September pukul 22:30.

### `EPIC RI-25` — Penanggung jawab episode

**Tujuan:** sistem dapat menjawab siapa DPJP dan siapa perawat pada tanggal tertentu.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-116` — DPJP berbentuk riwayat berperiode, bukan satu kolom yang ditimpa**
> **Contoh:** dr. Andi menjadi DPJP 21–23 September, dr. Rina 23–25 September. Pada 25 September
> sistem masih dapat menjawab bahwa perpindahan 22 September diminta dr. Andi selagi ia berwenang.

> **`FR-RI-117` — Satu episode aktif punya tepat satu DPJP aktif**
> **Contoh:** percobaan membuat penugasan DPJP kedua tanpa menutup yang pertama ditolak.

> **`FR-RI-118` — Pengalihan DPJP wajib beralasan**
> **Contoh:** pengalihan tanpa alasan ditolak dengan kode 400.

> **`FR-RI-119` — Episode boleh berjalan tanpa perawat penanggung jawab**
> **Contoh:** antara pukul 10:40 dan 11:00 episode Tn. Budi belum punya perawat. Selama 20 menit
> itu tidak ada satu pun tindakan yang tertahan, dan episode muncul pada daftar pantau kepala
> ruangan.

### `EPIC RI-26` — Perpindahan pasien dan pindah kelas

**Tujuan:** pasien dapat berpindah tempat tidur tanpa episode terputus, dan kelas tagihan mengikuti.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-120` — Perpindahan bersifat utuh**
> **Contoh:** bila pembukaan penempatan baru gagal, penempatan lama **tidak** jadi ditutup. Tn.
> Budi tetap tercatat di `BD-RSMMC-00042`. Tidak pernah ada saat pasien tercatat tanpa tempat tidur.

> **`FR-RI-121` — Kelas yang ditagihkan mengikuti kamar yang ditempati**
> **Contoh:** Tn. Budi pindah dari Melati 3B kelas 2 ke Anggrek 1A kelas 1 pada 23 September pukul
> 09:30. Riwayat penempatan menunjukkan 2 hari kelas 2 dan 2 hari kelas 1.

> **`FR-RI-122` — Dokter yang bukan DPJP aktif tidak dapat memindahkan pasien**
> **Contoh:** dr. Rina, dokter jaga, mencoba memindahkan Tn. Budi yang DPJP-nya dr. Andi.
> Permintaan ditolak dengan kode 403 dan pesan "Hanya DPJP episode ini yang dapat memindahkan
> pasien." Tidak ada kolom keterangan yang dapat dipakai melewatinya.

> **`FR-RI-123` — Perpindahan wajib beralasan medis**
> **Contoh:** perpindahan tanpa alasan ditolak dengan kode 400.

### `EPIC RI-27` — Keputusan pulang dan resume

**Tujuan:** keputusan pulang tercatat, dan setiap episode meninggalkan ringkasan resmi.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-124` — Hanya DPJP aktif yang menyatakan pasien boleh pulang**
> **Contoh:** dr. Rina yang bukan DPJP ditolak dengan kode 403.

> **`FR-RI-125` — Satu episode punya paling banyak satu resume pulang**
> **Contoh:** percobaan membuat resume kedua ditolak dengan kode 409.

> **`FR-RI-126` — Resume mengisi DPJP beserta periodenya secara otomatis**
> **Contoh:** resume Tn. Budi menampilkan "dr. Andi, 21–23 Sept; dr. Rina, 23–25 Sept" tanpa
> diketik ulang.

> **`FR-RI-127` — Isi wajib resume menyesuaikan cara pulang**
> **Contoh:** cara pulang `Referred` sementara tujuan rujukan kosong ditolak dengan kode 400.

> **`FR-RI-128` — Resume terkunci setelah episode ditutup**
> **Contoh:** percobaan mengubah resume episode `Closed` tanpa sesi koreksi ditolak dengan kode 409.

> **`FR-RI-153` — Perubahan resume yang sudah ditandatangani menyimpan versi lamanya**
> **Contoh:** resume Ibu Sari ditandatangani dr. Andi 15 Agustus dengan cara pulang "kabur". Pada
> 17 Agustus supervisor membuka sesi koreksi dan mengubahnya menjadi "atas permintaan sendiri".
> Sistem menyimpan salinan versi 15 Agustus lengkap dengan isi dan nama penandatangan lamanya.
> Menyunting resume yang **belum** ditandatangani tidak membuat versi apa pun.

### `EPIC RI-28` — Daftar periksa, kelayakan keuangan, dan penutupan

**Tujuan:** episode hanya dapat ditutup bila kelima syaratnya benar-benar terpenuhi.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-129` — Kelima syarat penutupan diperiksa dan dilaporkan satu per satu**
> **Contoh:** permintaan tutup pukul 10:00 ditolak 422 dengan daftar: resume belum ditandatangani,
> kelayakan keuangan masih `Pending`. Tiga syarat lain sudah terpenuhi dan ikut ditampilkan.

> **`FR-RI-130` — Hanya kasir atau billing yang dapat meminta `FinancialClearance = Cleared`, dan service memvalidasi settlement Billing terlebih dahulu**
> **Contoh:** petugas admisi menandai `Cleared` dan ditolak 403. Kasir meminta `Cleared` ketika masih ada kekurangan Rp750.000 atau refund deposit yang belum diselesaikan dan ditolak 422. Kasir menandai tanpa catatan juga ditolak 400.

> **`FR-RI-131` — Jalan keluar supervisor hanya menembus syarat keuangan**
> **Contoh:** supervisor menutup episode sementara resume belum ditandatangani, dan tetap ditolak
> 422. Keempat syarat lain tidak dapat dilewati siapa pun.

> **`FR-RI-132` — Penutupan melepas tempat tidur dalam satu tindakan**
> **Contoh:** episode `Closed` pukul 13:10, dan `BD-RSMMC-00105` muncul pada pencarian tempat tidur
> kosong pukul 13:11.

> **`FR-RI-133` — Butir daftar periksa yang dinonaktifkan tidak lagi menahan penutupan**
> **Contoh:** admin menonaktifkan butir `DISCHARGE-MED`. Episode berikutnya dapat ditutup tanpa
> menandai butir itu, dan penandaan lama tetap tersimpan.

> **`FR-RI-149` — Kepergian fisik pasien melepas tempat tidur seketika**
> **Contoh:** Tn. Budi diputuskan boleh pulang pukul 09:20. Keluarga menjemput dan ia meninggalkan
> kamar pukul 10:15; perawat mencatatnya. Bed `BD-RSMMC-00105` langsung tersedia, dan pukul 10:40
> sudah dipesankan untuk Ny. Sari. Episode Tn. Budi baru ditutup pukul 13:10. Tanpa aturan ini,
> tempat tidur itu baru bebas pukul 13:10 — selisih dua setengah jam pada satu tempat tidur saja.

> **`FR-RI-150` — Kepergian fisik bukan penutupan**
> **Contoh:** setelah kepergian dicatat, episode Tn. Budi tetap berstatus rencana pulang, tetap
> wajib ditutup, dan tetap muncul pada daftar pantau penutupan tertunda. Yang berubah hanya tempat
> tidurnya.

> **`FR-RI-151` — Pasien yang sudah pergi tidak dapat dipindahkan dan tidak muncul di census**
> **Contoh:** percobaan memindahkan Tn. Budi pukul 11:00 ditolak dengan pesan "Pasien sudah tercatat
> meninggalkan ruangan". Census pukul 11:00 juga tidak lagi memuat namanya.

### `EPIC RI-29` — Riwayat status dan daftar pantau

**Tujuan:** setiap perubahan dapat ditelusuri, dan keterlambatan terlihat tanpa menghalangi kerja.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-134` — Baris riwayat tidak dapat diubah dan tidak dapat dihapus**
> **Contoh:** tidak ada endpoint update maupun delete untuk riwayat status. Percobaan memanggilnya
> menghasilkan 404.

> **`FR-RI-135` — Perubahan yang dihitung sistem ditandai sebagai dilakukan sistem**
> **Contoh:** pemesanan yang gugur meninggalkan baris riwayat dengan penanda sistem dan tanpa nama
> orang, sehingga audit tidak salah menuduh siapa pun.

> **`FR-RI-136` — Tiga daftar pantau tersedia dan tidak menahan tindakan apa pun**
> **Contoh:** episode `DischargePending` yang belum ditutup lebih dari 4 jam muncul pada daftar
> pantau petugas admisi, tetapi penutupan tetap dapat dijalankan kapan saja.

> **`FR-RI-137` — Laporan selisih tempat tidur tersedia**
> **Contoh:** tempat tidur tertulis `Available` padahal masih ada penempatan aktif atas nama Tn.
> Budi muncul sebagai satu baris laporan lengkap dengan nama pasien dan waktu mulai.

### `EPIC RI-30` — Sesi koreksi episode

**Tujuan:** kesalahan catatan dapat dibetulkan tanpa mengganggu tempat tidur dan tanpa menambah lama dirawat.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-138` — Sesi koreksi tidak mengubah status episode**
> **Contoh:** episode Ibu Sari ditutup 15 Agustus. Pada 17 Agustus supervisor membuka sesi koreksi
> dan mengubah cara pulang. Sepanjang 17 Agustus status episode tetap `Closed`, `MELATI-03` tetap
> ditempati pasien lain tanpa terganggu, dan lama dirawat tetap 3 hari.

> **`FR-RI-139` — Hanya supervisor, dan alasan wajib**
> **Contoh:** kepala ruangan membuka sesi koreksi dan ditolak 403. Supervisor tanpa alasan ditolak
> 400.

> **`FR-RI-140` — Menutup sesi wajib menyertakan daftar perubahan**
> **Contoh:** menutup sesi tanpa mengisi apa saja yang berubah ditolak 400. Ini satu-satunya jejak
> koreksi, karena status episode tidak berubah sehingga riwayat status tidak mencatat apa pun.

### `EPIC RI-31` — Pengaturan yang dapat diubah admin

**Tujuan:** seluruh angka berada di satu tempat dan tidak tertanam di kode.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-141` — Lima angka dapat diubah dari satu layar dan berlaku pada pembacaan berikutnya**
> **Contoh:** admin mengubah ambang penutupan tertunda dari 4 jam menjadi 6 jam. Daftar pantau
> berikutnya memakai 6 jam tanpa aplikasi dinyalakan ulang.

> **`FR-RI-143` — Ambang tindak lanjut kekurangan deposit ikut diatur di layar yang sama** (**baru `0.6.0`**)
> Angka keenam menyusul bersama `FR-RI-177`: berapa hari sekali episode dengan kekurangan deposit
> muncul kembali pada daftar pantau. Nilai bawaannya tiga hari, dan mengubahnya tidak boleh
> menyentuh transaksi deposit yang sudah tercatat.
> **Contoh:** admin mengubah ambang dari 3 hari menjadi 5 hari; episode yang kekurangannya belum
> tertutup muncul berikutnya pada hari kelima, sementara kwitansi lama tidak berubah sama sekali.

> **`FR-RI-142` — Modul tetap berjalan bila pengaturan belum terisi**
> **Contoh:** pada lingkungan baru tanpa baris pengaturan, sistem memakai nilai bawaan dan mencatat
> peringatan, bukan gagal.

### `EPIC RI-32` — Perbaikan tempat tidur dan pembatasan wewenang status

**Tujuan:** admin tetap dapat menutup tempat tidur yang rusak, dan status penghunian hanya lahir dari Rawat Inap.
**Disposisi backend:** `EXTEND` — menyentuh `BedController` dan slice Redux frontend yang sudah ada

> **`FR-RI-143` — Tombol aktifkan dan nonaktifkan tempat tidur berfungsi**
> **Contoh:** hari ini tombol memanggil endpoint yang tidak ada dan selalu gagal 404. Setelah
> diperbaiki, menonaktifkan `BD-RSMMC-00042` berhasil dan tempat tidur itu hilang dari pencarian.

> **`FR-RI-144` — Menyetel status terisi atau dipesan lewat menu master data ditolak**
> **Contoh:** admin mencoba menyetel `BD-RSMMC-00042` menjadi `Occupied` dan ditolak 422 dengan
> pesan yang mengarahkan ke modul Rawat Inap.

> **`FR-RI-145` — Wewenang admin atas keadaan non-pasien tidak berkurang**
> **Contoh:** menyetel `Maintenance` tetap berhasil.

### `EPIC RI-33` — Bayi baru lahir dan boks bayi

**Tujuan:** bayi mendapat episode sendiri, dan boks bayi diperlakukan sebagai tempat tidur.
**Disposisi backend:** `EXISTING / REUSE` untuk masternya; bergantung pada `EPIC RI-23`

> **`FR-RI-146` — Boks bayi diperlakukan sebagai tempat tidur biasa**
> **Contoh:** boks `BOX-MELATI-03-A` didaftarkan sebagai tempat tidur bertanda `IsForNewborn` di
> kamar Melati 3. Bayi Ny. Sari mendapat episode dan kunjungan sendiri, lalu ditempatkan di boks
> itu. Census menampilkan dua baris: Ny. Sari dan bayinya.

> **`FR-RI-147` — Episode ibu dan bayi sepenuhnya terpisah**
> **Contoh:** menutup episode Ny. Sari tidak menutup episode bayinya, dan tidak melepas boks bayi.

> **`FR-RI-152` — Episode bayi menyimpan penanda rawat gabung dengan ibunya**
> **Contoh:** perawat membuka detail boks `BOX-MELATI-03-A` dan sistem menjawab bahwa penghuninya
> adalah bayi Ny. Sari yang dirawat di Melati 3. Tanpa penanda ini, satu-satunya petunjuk hanyalah
> kesamaan kamar. Penanda ini boleh kosong untuk episode yang bukan bayi rawat gabung, dan
> **tidak boleh** menunjuk episode milik pasien yang sama.

### `EPIC RI-34` — Kelayakan penempatan menurut jenis kelamin dan isolasi

**Tujuan:** sistem tidak pernah menempatkan pasien pada tempat tidur atau kamar yang secara privasi
atau pengendalian infeksi tidak layak baginya, walaupun petugas memaksa.
**Disposisi backend:** `MISSING / NEW`; menempel pada `EPIC RI-23` penempatan dan `EPIC RI-26`
perpindahan
**Dasar keputusan:** `RWI-DEC-064`, `RWI-DEC-065`, `RWI-DEC-066`, dirinci pada `RWI-RULE-012`

> **Kenapa epic ini baru lahir pada `0.3.0`.** Sampai `0.2.0`, penanda jenis kelamin dan isolasi
> pada master tempat tidur hanya **menyaring hasil pencarian**: petugas tetap dapat menempatkan
> pasien di mana pun ia mau. Pemilik berwenang mengubah arahnya menjadi aturan yang **menolak**.
> Sisi teknisnya murah — bentuk daftar aturan Kelayakan Penempatan memang dirancang sejak awal
> untuk ditambahi — tetapi dampaknya besar, sehingga diberi epic sendiri agar dapat diuji terpisah.

#### Bagian A — Pemisahan jenis kelamin

> **`FR-RI-154` — Penempatan ditolak bila penanda tempat tidur tidak menerima jenis kelamin pasien**
> **Contoh:** `MELATI-03-A` bertanda hanya menerima perempuan. Petugas mencoba menempatkan Tn. Budi
> di sana. Ditolak dengan pesan "Tempat tidur ini hanya untuk pasien perempuan" dan kode 422.
> Sebelum `0.3.0`, tempat tidur itu sekadar tidak muncul pada hasil pencarian, sementara penempatan
> paksa tetap berhasil.

> **`FR-RI-155` — Kamar tidak boleh ditempati campur laki-laki dan perempuan**
> Pemeriksaannya membaca **penghuni yang sedang ada**, bukan penanda pada master kamar. Tidak ada
> kolom "boleh campur" pada `MstRoom`, dan `RWI-DEC-066` menolaknya secara tegas.
> **Contoh:** Kamar Melati 3 berisi tiga tempat tidur. Pukul 08:00 Ny. Sari menempati `MELATI-03-A`.
> Pukul 10:00 petugas mencoba menempatkan Tn. Budi di `MELATI-03-B`. Ditolak dengan pesan "Kamar
> Melati 3 sedang dihuni pasien perempuan" dan kode 422. Pukul 10:30 Ny. Rina ditempatkan di
> `MELATI-03-B` dan **diterima**.
>
> Kamar berisi satu tempat tidur tidak pernah tersentuh aturan ini, karena tidak mungkin ada
> penghuni lain.

> **`FR-RI-156` — Boks bayi dikecualikan dari kedua sisi pemeriksaan**
> **Contoh:** bayi Ny. Sari berjenis kelamin laki-laki dan menempati `BOX-MELATI-03-A` di kamar
> ibunya. Penempatan bayi itu **berhasil** walaupun kamarnya sedang dihuni perempuan. Sebaliknya,
> ketika Ny. Rina hendak masuk ke `MELATI-03-B`, bayi laki-laki itu **tidak dihitung** sebagai
> penghuni, sehingga penempatan Ny. Rina tetap diterima.
>
> **Kenapa dua arah.** Bayi tidak boleh menutup kamar bagi pasien lain, dan bayi juga tidak boleh
> ditolak hanya karena jenis kelamin ibunya berbeda.

> **`FR-RI-157` — Pasien yang jenis kelaminnya belum tercatat hanya boleh masuk kamar kosong**
> **Contoh:** pasien tidak dikenal dari kecelakaan, jenis kelaminnya belum terisi. Ia hanya dapat
> ditempatkan pada tempat tidur yang menerima laki-laki dan perempuan sekaligus, **dan** hanya ke
> kamar yang belum ada penghuninya. Penempatan ke kamar berpenghuni ditolak dengan kode 422.

#### Bagian B — Kebutuhan isolasi

> **`FR-RI-158` — Kebutuhan isolasi adalah atribut episode, bukan atribut pasien**
> Melekat pada satu masa perawatan, bernilai tidak secara bawaan, dan tersimpan bersama siapa serta
> kapan terakhir menetapkannya.
> **Contoh:** Tn. Budi butuh isolasi pada episode September karena suspek penyakit menular. Ketika
> ia dirawat lagi pada Desember untuk patah tulang, episode barunya bernilai tidak — tanpa ada
> petugas yang perlu mematikannya.

> **`FR-RI-159` — Petugas admisi merekam catatan awal, DPJP mengambil keputusan klinis**
> Selagi episode `Draft`, petugas admisi boleh merekam nilainya berdasarkan surat atau keterangan
> dokter pengirim, dan hasilnya ditandai **catatan awal**. Setelah episode aktif, hanya **DPJP
> aktif** yang boleh mengubahnya, dan hasilnya ditandai **keputusan klinis**.
> **Contoh:** pukul 09:15 petugas admisi merekam "membutuhkan isolasi" untuk Tn. Budi berdasarkan
> surat rujukan puskesmas, tertandai catatan awal. Hari kedua dr. Andi selaku DPJP mengubahnya
> menjadi tidak, tertandai keputusan klinis atas namanya. Percobaan dr. Rina yang bukan DPJP
> mengubah nilai yang sama ditolak dengan kode 403.
>
> **Kenapa dibedakan, bukan disamakan.** Penempatan tidak boleh menunggu pengkajian klinis yang
> slice-nya di luar MVP dokumen ini karena dimiliki sub-modul `keperawatan/` yang belum dirancang
> — **bukan** karena `DEC-INP-001`, yang sudah tertutup `RWI-DEC-062`. Tetapi merekam keterangan
> orang lain berbeda dari memutuskan secara klinis, dan rekam medis harus dapat menunjukkan
> bedanya.

> **`FR-RI-160` — Tempat tidur isolasi dijaga dari dua arah**
> Pasien yang membutuhkan isolasi **hanya** boleh ke tempat tidur isolasi, dan pasien yang tidak
> membutuhkannya **tidak boleh** menempati tempat tidur isolasi.
> **Contoh:** percobaan menempatkan Tn. Budi yang butuh isolasi di `BD-RSMMC-00042` yang bukan
> isolasi ditolak dengan pesan "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan
> pada tempat tidur isolasi". Sebaliknya, menempatkan pasien biasa di tempat tidur isolasi ditolak
> dengan pesan "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi", supaya kapasitas
> isolasi tidak habis terpakai sia-sia.

> **`FR-RI-161` — Perubahan kebutuhan isolasi tidak pernah ditahan; yang muncul adalah daftar pantau**
> Bila kebutuhan isolasi berubah menjadi ya sementara pasien sudah berbaring di tempat tidur biasa,
> perubahan itu **tetap diterima**. Episode itu muncul pada daftar pantau **penempatan tidak
> sesuai** sampai pasien dipindahkan.
> **Contoh:** pukul 14:00 dr. Andi menyatakan Tn. Budi di `MELATI-03-B` membutuhkan isolasi.
> Pencatatannya diterima seketika. Episode Tn. Budi muncul pada daftar pantau, dan hilang dari sana
> begitu ia dipindahkan ke tempat tidur isolasi pukul 15:20.
>
> **Kenapa tidak ditahan.** Menahan pencatatan klinis demi menjaga aturan penempatan adalah urutan
> terbalik. Yang benar: fakta klinis dicatat lebih dulu, lalu sistem menunjukkan bahwa
> penempatannya perlu dibetulkan.

> **`FR-RI-162` — Aturan yang sama berlaku pada perpindahan, bukan hanya penempatan**
> Kedelapan aturan Kelayakan Penempatan dipanggil dari dua tindakan: menempatkan dan memindahkan.
> **Contoh:** memindahkan Tn. Budi ke `ANGGREK-01-B` di kamar yang sedang dihuni pasien perempuan
> ditolak dengan alasan dan kode yang sama persis seperti penempatan awal.


### `EPIC RI-35` — Deposit dan settlement finansial rawat inap

**Tujuan:** uang muka pasien dapat diterima, ditambah, ditelusuri, dan diselesaikan terhadap tagihan akhir tanpa membuat ledger finansial kedua di modul Rawat Inap.
**Disposisi backend:** `CROSS-MODULE / EXTEND` — **diturunkan dari `MISSING + EXTEND` pada `0.6.0`**. Transaksi dan ledger dimiliki `BillingManagement`/Kasir dan **sebagian besar sudah berjalan**: `BilDepositAccount`, `BilDepositMovement` dengan `IdempotencyKey`, `BillingDepositService`, `BillingSettlementService`, `BillingRefundService`, dan `BillingPatientFundsController`. `InPatientManagement` tetap hanya memberikan konteks `EpisodeId`, menampilkan ringkasan, dan memakai hasil settlement sebagai closure gate.
**Yang benar-benar `MISSING`, dipersempit `0.6.1`:** kebijakan minimum deposit per penjamin dan kelas, ringkasan deposit per episode, langkah Deposit pada admisi, dan daftar pantau kekurangan deposit. ~~Pengikatan `EpisodeId`~~ dicabut dari daftar ini karena sudah tercapai lewat join (`RWI-FACT-017`).
**Dasar scope:** arahan produk 2026-09-03. ID keputusan formal `RWI-DEC-*` belum tersedia pada sumber dan wajib disinkronkan sebelum development lock.
**Evidence legacy:** V1 memiliki `DepositRanap`, `DepositPersentase`, `NoKwitansi` unique, `IPerkiraanBillingRanapService`, dan `IDepositRanapNumberService`; evidence ini dipakai sebagai referensi capability, **bukan** sebagai keputusan untuk menyalin schema V1.

> **`FR-RI-163` — Setiap transaksi deposit terikat pada tepat satu episode rawat inap**
> Deposit tidak boleh hanya menempel pada pasien karena pasien yang sama dapat memiliki banyak episode sepanjang waktu.
> **Mekanismenya, dikoreksi `0.6.1`:** penelusuran dicapai lewat **kunjungan**, bukan lewat kolom baru. `BilDepositAccount.EncounterId` unique dan `InpEpisode.EncounterId` unique, sehingga satu akun deposit menunjuk tepat satu episode tanpa keraguan. Menambahkan kolom `EpisodeId` justru menciptakan dua sumber kebenaran yang bisa berbeda isi.
> **Contoh:** deposit Rp5.000.000 untuk episode September tidak boleh otomatis muncul sebagai saldo episode Desember milik pasien yang sama.

> **`FR-RI-164` — Kebutuhan deposit mengikuti kebijakan finansial, bukan aturan hardcode semua pasien**
> Billing/Kasir menentukan apakah deposit diperlukan dan berapa targetnya berdasarkan kebijakan yang dimiliki domain finansial/penjamin. Bila deposit tidak diperlukan, sistem tidak membuat transaksi palsu bernilai nol.
> **Contoh:** pasien dengan penjamin yang tidak mensyaratkan uang muka tetap dapat ditempatkan tanpa baris deposit Rp0.

> **`FR-RI-165` — Deposit awal dan setiap top-up adalah transaksi append-only yang terpisah**
> Pembayaran berikutnya tidak pernah mengubah nominal transaksi sebelumnya.
> **Contoh:** Rp5.000.000 pukul 09:15, Rp3.000.000 hari kedua, dan Rp2.000.000 hari keempat tersimpan sebagai tiga transaksi dengan total penerimaan Rp10.000.000.

> **`FR-RI-166` — Setiap penerimaan mempunyai kwitansi unik dan perlindungan idempotensi**
> Satu pembayaran yang dikirim ulang karena timeout tidak boleh membentuk dua penerimaan.
> **Contoh:** request pembayaran dengan `idempotencyKey` yang sama dikirim dua kali; response kedua mengembalikan transaksi pertama, bukan membuat kwitansi baru.

> **`FR-RI-167` — Ringkasan deposit episode dapat dibaca tanpa menghitung ulang dari frontend**
> Billing mengembalikan sekurang-kurangnya target/requested deposit bila ada, total diterima, total dialokasikan, total refund, saldo tersedia, dan kebutuhan top-up yang masih terbuka.
> **Contoh:** dari penerimaan Rp8.000.000, alokasi Rp6.500.000, dan refund Rp500.000, layar menampilkan saldo tersedia Rp1.000.000 dari response backend yang sama.

> **`FR-RI-168` — Permintaan top-up tidak mengubah histori pembayaran**
> Billing/Kasir boleh membuat permintaan tambahan selama episode aktif berdasarkan kebijakan atau keputusan finansial. Top-up request dan top-up payment adalah dua kejadian berbeda.
> **Contoh:** kasir meminta tambahan Rp2.000.000. Keluarga baru membayar Rp1.500.000; sistem tetap menunjukkan outstanding top-up Rp500.000 tanpa mengubah kwitansi deposit awal.

> **`FR-RI-169` — Saldo deposit tidak dapat dipindahkan diam-diam ke episode lain**
> Pemindahan, refund, atau penggunaan lintas episode harus melalui transaksi finansial eksplisit dengan audit trail.
> **Contoh:** episode lama dibatalkan dan pasien dibuka episode baru; operator tidak boleh sekadar mengganti `EpisodeId` pada transaksi deposit lama.

> **`FR-RI-170` — Final settlement mengalokasikan deposit terhadap tagihan final**
> Billing menghitung `FinalBillAmount`, deposit yang dapat dialokasikan, dan selisih setelah alokasi. Modul Rawat Inap tidak menghitung tarif atau tagihan sendiri.
> **Contoh:** tagihan final Rp12.000.000 dan deposit tersedia Rp10.000.000 menghasilkan kekurangan Rp2.000.000; setelah kekurangan dibayar, settlement dapat selesai.

> **`FR-RI-171` — Kelebihan deposit menghasilkan refund/penyelesaian kelebihan yang eksplisit**
> Saldo negatif atau penghapusan saldo tanpa transaksi dilarang.
> **Contoh:** tagihan final Rp7.500.000 dan deposit tersedia Rp10.000.000 menghasilkan kelebihan Rp2.500.000. Billing membuat transaksi refund Rp2.500.000; histori tiga pembayaran deposit sebelumnya tetap utuh.

> **`FR-RI-172` — `FinancialClearance = Cleared` hanya boleh setelah settlement finansial selesai**
> Service penutupan membaca ringkasan Billing. Kekurangan pembayaran, deposit yang belum dialokasikan sesuai settlement, atau refund yang belum diselesaikan menahan clearance normal. Supervisor tetap dapat memakai `CloseOverride`, tetapi override tidak mengubah ledger Billing dan selalu masuk laporan pengecualian.
> **Contoh:** kasir mencoba memberi `Cleared` saat refund Rp2.500.000 masih pending dan ditolak 422.

> **`FR-RI-173` — Pembatalan admisi setelah deposit tidak pernah menghapus transaksi uang**
> Pembatalan episode memicu kebutuhan refund/reversal pada Billing; transaksi penerimaan dan kwitansi asli tetap dapat diaudit.
> **Contoh:** pasien membayar deposit Rp5.000.000 lalu admisi dibatalkan sebelum penempatan. Episode menjadi `Cancelled` hanya setelah jalur finansial mencatat refund/reversal yang sesuai, atau supervisor menggunakan override yang meninggalkan exception terbuka.

> **`FR-RI-174` — Deposit adalah langkah tersendiri di dalam multi-step admisi**
> Langkah Deposit berdiri sesudah langkah Pembayaran dan sebelum langkah Dokter, bukan layar terpisah yang dibuka kasir setelah admisi selesai. Isinya satu nominal deposit yang diterima, nilai minimum menurut kebijakan, dan status pemenuhannya.
> **Contoh:** petugas memilih penjamin dan kelas perawatan, menekan lanjut, lalu langsung sampai pada layar `Input Deposit Rawat Inap` sebelum memilih dokter.

> **`FR-RI-175` — Minimum deposit adalah nilai kebijakan per penjamin dan kelas perawatan**
> Nominal minimum tidak boleh ditulis tetap di layar. Ia dibaca dari kebijakan finansial untuk kombinasi penjamin dan kelas yang dipilih pada langkah sebelumnya. Bila kebijakan tidak mensyaratkan deposit, langkah Deposit tidak menuntut nominal apa pun dan tidak membuat transaksi Rp0 — `FR-RI-164` tetap berlaku penuh.
> **Contoh:** pasien umum kelas VIP meminta minimum Rp100.000.000, sedangkan pasien dengan penjamin yang menanggung penuh melewati langkah ini tanpa nominal.

> **`FR-RI-176` — Deposit di bawah minimum memberi peringatan, bukan penolakan**
> Admisi tetap berlanjut ketika nominal yang diterima lebih kecil dari minimum kebijakan. Layar menyatakan selisihnya, dan selisih itu tersimpan sebagai kekurangan deposit yang terbaca pada ringkasan episode. Nominal nol pada episode yang kebijakannya mensyaratkan deposit juga hanya menghasilkan peringatan.
> **Contoh:** minimum Rp100.000.000 sementara keluarga membayar Rp40.000.000. Petugas tetap dapat maju ke langkah Dokter, dan ringkasan episode menunjukkan kekurangan Rp60.000.000.

> **`FR-RI-177` — Kekurangan minimum deposit ditagih berkala pada perawatan yang melewati ambang hari**
> Selama episode aktif dan kekurangan belum tertutup, episode muncul pada daftar pantau kekurangan deposit setiap kelipatan ambang tindak lanjut — bawaan tiga hari, diatur admin lewat `EPIC RI-31`. Perawatan yang selesai sebelum ambang pertama tidak menghasilkan penagihan terpisah; kekurangannya diperhitungkan pada pelunasan akhir. Penagihan ini adalah pengingat kerja, bukan gerbang yang menahan perawatan.
> **Contoh:** pasien dirawat sembilan hari dengan kekurangan Rp60.000.000. Episode muncul pada daftar pantau di hari ketiga, keenam, dan kesembilan sampai kekurangannya tertutup lewat top-up atau lewat pelunasan akhir.

> **`FR-RI-178` — Transaksi deposit admisi terbentuk hanya setelah `EpisodeId` ada**
> Nominal pada langkah Deposit ditahan sebagai isian langkah, bukan sebagai transaksi uang. Transaksi penerimaan dibentuk tepat setelah episode `Draft` berhasil dibuat pada langkah Dokter, memakai `idempotencyKey` sehingga percobaan ulang tidak melahirkan kwitansi ganda. Bila pembuatan episode gagal, tidak ada penerimaan yang tersimpan dan tidak ada kwitansi yang terbit.
> **Contoh:** petugas mengisi deposit Rp5.000.000, lalu `POST /episodes` ditolak 409 karena kunjungan sudah punya episode. Tidak ada transaksi deposit yang terbentuk; petugas kembali ke langkah sebelumnya dengan nominal masih terisi.

> **Risiko yang diterima sadar — `RWI-RISK-006`.** Di antara petugas menekan lanjut pada langkah Deposit dan episode berhasil dibuat, uang sudah berada di tangan petugas sementara sistem belum menerbitkan kwitansi. Jendelanya sempit karena kedua langkah berurutan dalam satu sesi admisi, tetapi ia nyata. Mitigasinya: kwitansi hanya sah setelah transaksi terbentuk, dan admisi yang gagal pada langkah Dokter tidak boleh ditinggalkan dengan uang yang sudah diterima.

---

## 11. Model status yang diusulkan

| Objek | Status | Invariant utama |
| --- | --- | --- |
| Episode | `Draft`, `Admitted`, `DischargePending`, `Closed`, `Cancelled` | `Admitted` wajib punya tepat satu penempatan aktif. `DischargePending` wajib punya satu **sampai kepergian pasien dicatat**, setelah itu nol |
| Kehadiran pasien | Bukan status yang disimpan; diturunkan dari status episode dan waktu kepergian | Satu pasien paling banyak satu episode yang benar-benar hadir |
| Kebutuhan isolasi | Bukan status berperiode; satu penanda pada episode beserta asalnya — catatan awal admisi atau keputusan klinis DPJP | Yang tersimpan hanya **nilai yang berlaku sekarang**, bukan riwayat. Selagi `Draft` boleh disetel petugas admisi; setelah aktif hanya DPJP aktif |
| Pemesanan tempat tidur | `Active`, `Consumed`, `Expired`, `Cancelled` | Satu tempat tidur paling banyak satu pemesanan aktif |
| Penempatan tempat tidur | `Aktif`, `Berakhir` | Satu tempat tidur paling banyak satu penempatan aktif |
| Kelayakan keuangan | `Pending`, `Cleared`, `Blocked` | Hanya `Cleared` yang membuka penutupan normal. Sejak `0.5.0`, transisi ke `Cleared` divalidasi terhadap settlement Billing/Kasir |
| Ringkasan deposit episode | Bukan status `Inp`; nilai turunan dari Billing/Kasir | Membaca minimum kebijakan bila ada, total diterima, dialokasikan, refund, saldo tersedia, **kekurangan terhadap minimum kebijakan**, kekurangan terhadap tagihan final, dan outstanding top-up; tidak menjadi ledger kedua di Rawat Inap |
| Resume pulang | Belum ditandatangani, Tertandatangani | Satu episode paling banyak satu resume **yang berlaku**; versi sebelumnya tersimpan sebagai salinan |
| Sesi koreksi | `Terbuka`, `Tertutup` | Satu episode paling banyak satu sesi terbuka |

Rincian lengkap beserta perpindahan yang **tidak sah** ada pada
[`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md).

---

## 12. Sasaran arsitektur

| Kelompok | Isinya |
| --- | --- |
| **Dipakai ulang apa adanya** | Pasien, kunjungan, penjamin, tempat tidur, kamar, unit layanan, kelas pasien, dokter, pegawai, mesin hak akses, pola `ApiResponse`, pola seeder |
| **Diperluas** | Perilaku `BedController.UpdateBedAvailability`; slice Redux tempat tidur di frontend; integrasi `FinancialClearance` dengan ringkasan settlement Billing/Kasir |
| **Baru** | Sebelas tabel transaksi berawalan `Inp`, dua master `MstInpatient*`, enam service, lima controller modul, dua controller master; **ditambah contract cross-module deposit pada Billing/Kasir tanpa tabel `InpDeposit`** |

Tidak satu pun tabel milik modul lain berubah bentuknya. **Tiga belas** tabel baru, nol perubahan
kolom pada tabel existing.

**`0.5.0` tidak menambah ledger finansial ke `InPatientManagement`.** `EPIC RI-35` boleh menambah atau memperluas tabel transaksi pada `BillingManagement`, tetapi nama entity dan tabelnya **tidak ditetapkan oleh PRD Rawat Inap ini** karena kontrak arsitektur Billing V2 tidak disertakan. Satu-satunya invariant lintas modul yang dikunci di sini adalah setiap transaksi deposit harus dapat dirujuk kembali ke `EpisodeId`, dan Inpatient hanya menyimpan/membaca referensi atau snapshot clearance yang diperlukan untuk closure gate.

**`0.6.1` menambah satu master saja, dan nol kolom.** Rencana `0.6.0` untuk menambahkan `EpisodeId` pada `BilDepositAccount` **dibatalkan** setelah trace membuktikan kedua sisi sudah unique pada `EncounterId`; penelusuran episode dicapai lewat join. Yang tersisa sebagai tambahan hanyalah **master kebijakan minimum deposit** per penjamin dan kelas perawatan, dan tempatnya pada `BillingManagement` atau master penjamin — **bukan** pada `InPatientManagement`. Dengan begitu kalimat "nol perubahan kolom pada tabel existing" kini berlaku untuk seluruh modul, Billing termasuk.

**`0.3.0` tidak menambah satu tabel pun.** Kebutuhan isolasi masuk sebagai enam kolom pada
`InpEpisode` beserta satu enum `InpIsolationSource`, dan aturan pencampuran kamar dijalankan dengan
membaca penghuni yang sedang ada. `RWI-DEC-066` menolak menambah kolom "boleh campur" pada
`MstRoom`, sehingga janji "nol perubahan kolom pada tabel modul lain" tetap utuh.

Rincian lengkap ada pada [`02-backend-architecture.md`](./02-backend-architecture.md).

---

## 13. Sasaran kemampuan API

Endpoint yang sudah ada pada revisi sebelumnya mengikuti
[`contracts/api-contract.md`](./contracts/api-contract.md) rev `0.3.0`. **Endpoint deposit pada `0.6.0` terbagi dua: rute `patient-funds` yang sudah berjalan, dan tambahan baru yang diusulkan PRD ini.** Keduanya wajib disinkronkan ke `contracts/api-contract.md`, `validation-matrix.md`, dan `permission-audit-matrix.md` sebelum implementasi. Dengan demikian dokumen ini tidak mengklaim bahwa contract rev `0.3.0` sudah memuat deposit.

### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuka admisi | `InpatientEpisode : Create` | `OpenAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-21` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan admisi | `InpatientEpisode : Update` | `CancelAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-21` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/doctor-assignments` | Mengalihkan DPJP | `InpatientEpisode : Update` | `HandoverDoctorRequest` | `ApiResponse<InpatientDoctorAssignmentResponse>` | `EPIC RI-25` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/nurse-assignments` | Menugaskan perawat | `InpatientEpisode : Update` | `AssignNurseRequest` | `ApiResponse<InpatientNurseAssignmentResponse>` | `EPIC RI-25` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/status-history` | Riwayat status | `InpatientEpisode : Read` | – | `ApiResponse<List<InpatientStatusHistoryResponse>>` | `EPIC RI-29` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/correction-sessions` | Membuka sesi koreksi | `InpatientEpisode : Reopen` | `OpenCorrectionSessionRequest` | `ApiResponse<InpatientCorrectionSessionResponse>` | `EPIC RI-30` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/isolation-requirement` | Menetapkan atau mengubah kebutuhan isolasi | `InpatientEpisode : SetIsolation` | `SetIsolationRequirementRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-34` | **Rencana (belum tersedia)** |

### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/available-beds` | Mencari tempat tidur yang benar-benar dapat ditempati | `InpatientBedOccupancy : Read` | Query | `ApiResponse<AvailableBedPagedResult>` | `EPIC RI-22` | **Rencana (belum tersedia)** |
| `POST` | `/reservations` | Memesan tempat tidur | `InpatientBedOccupancy : Create` | `ReserveBedRequest` | `ApiResponse<BedReservationResponse>` | `EPIC RI-22` | **Rencana (belum tersedia)** |
| `POST` | `/placements` | Menempatkan pasien dan mengaktifkan episode | `InpatientBedOccupancy : Create` | `PlacePatientRequest` | `ApiResponse<BedPlacementResponse>` | `EPIC RI-23` | **Rencana (belum tersedia)** |
| `POST` | `/placements/transfer` | Memindahkan pasien | `InpatientBedOccupancy : Transfer` | `TransferPatientRequest` | `ApiResponse<BedPlacementResponse>` | `EPIC RI-26` | **Rencana (belum tersedia)** |

### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{episodeId}/decide` | Menyatakan pasien boleh pulang | `InpatientDischarge : Update` | `DecideDischargeRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-27` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/record-departure` | Mencatat pasien sudah meninggalkan ruangan | `InpatientDischarge : RecordDeparture` | `RecordDepartureRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |
| `PATCH` | `/{episodeId}/summary/sign` | Menandatangani resume | `InpatientDischarge : Sign` | `SignDischargeSummaryRequest` | `ApiResponse<DischargeSummaryResponse>` | `EPIC RI-27` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/financial-clearance` | Menandai kelayakan keuangan | `InpatientFinancialClearance : Update` | `MarkFinancialClearanceRequest` | `ApiResponse<FinancialClearanceResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |
| `GET` | `/{episodeId}/closure-readiness` | Memeriksa kelima syarat penutupan | `InpatientDischarge : Read` | – | `ApiResponse<ClosureReadinessResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/close` | Menutup episode | `InpatientEpisode : Close` | `CloseEpisodeRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/close-with-override` | Menutup menembus gerbang keuangan | `InpatientEpisode : CloseOverride` | `CloseEpisodeOverrideRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Deposit Rawat Inap

Base URL: `api/v1/health-services/billing-management/billing/patient-funds` — **rute yang sudah ada**, bukan base URL baru. Usulan `billing-management/inpatient-deposits` pada `0.5.0` dicabut oleh `0.6.0` agar tidak lahir ledger deposit kedua.

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/deposits/{encounterId}` | Membaca akun deposit satu kunjungan | `BillingDeposit : Read` | – | `ApiResponse<BillingDepositResponse>` | `EPIC RI-35` | **Sudah ada** — `BillingPatientFundsController.cs:67` |
| `POST` | `/deposits/{encounterId}/top-ups` | Menerima deposit awal dan top-up | `BillingDeposit : Create` | `TopUpDepositRequest` | `ApiResponse<BillingDepositResponse>` | `EPIC RI-35` | **Sudah ada, perlu `EXTEND`** — wajib menerima dan menyimpan `episodeId` |
| `POST` | `/deposits/{encounterId}/allocations` | Mengalokasikan deposit ke tagihan | `BillingDeposit : Allocate` | `AllocateDepositRequest` | `ApiResponse<BillingAllocationResponse>` | `EPIC RI-35` | **Sudah ada, perlu `EXTEND`** |
| `GET` | `/deposit-policies` | Membaca kebijakan deposit untuk kombinasi penjamin dan kelas perawatan | `BillingDeposit : Read` | `guarantorId`, `patientClassId` | `ApiResponse<DepositPolicyResponse>` | `EPIC RI-35` | **Baru `0.6.0`** — sumber minimum pada langkah Deposit |
| `GET` | `/deposits/episodes/{episodeId}` | Ringkasan deposit satu episode: minimum kebijakan, diterima, dialokasikan, refund, saldo, **dua** angka kekurangan, outstanding top-up | `BillingDeposit : Read` | – | `ApiResponse<EpisodeDepositSummaryResponse>` | `EPIC RI-35` | **Baru `0.6.0`, tetap berlaku** |
| `GET` | `/invoices/encounters/{encounterId}/charge-summary` | Rekap tagihan satu kunjungan; sumber angka tagihan final pada settlement | `BillingInvoice : Read` | – | `ApiResponse<EncounterChargeSummaryResponse>` | `EPIC RI-35` | ✅ **Sudah ada** — `BillingInvoicesController.cs:122` |
| `POST` | `/financial-exceptions/refunds` beserta `/refunds/{id}/approve` | Mencatat dan menyetujui refund kelebihan deposit | `BillingRefund : Create` / `Approve` | Kontrak `BIL-API-0.4` | – | `EPIC RI-35` | ✅ **Sudah ada** — `BillingFinancialExceptionsController.cs:112,157`. Menggantikan usulan `/deposits/episodes/{id}/refunds` yang **dicabut `0.6.1`** |

Daftar pantau kekurangan deposit `FR-RI-177` tidak menambah endpoint Billing. Ia dibaca lewat daftar pantau Rawat Inap yang sudah direncanakan `EPIC RI-29`, dengan satu penyaring baru dan angka kekurangan yang diambil dari ringkasan episode di atas.

**Catatan:** ownership implementasi seluruh rute di atas tetap pada `BillingManagement`. Tidak boleh dibuat controller `InpDepositController` yang menyimpan ledger uang di area Rawat Inap, dan tidak boleh dibuat controller deposit kedua di `BillingManagement` yang menduplikasi `BillingPatientFundsController`.

### Health Services / Inpatient Management / Inpatient Census

Base URL: `api/v1/health-services/inpatient-management/census`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pasien dirawat beserta lokasi dan lama dirawat | `InpatientCensus : Read` | Query | `ApiResponse<CensusPagedResult>` | `EPIC RI-24` | **Rencana (belum tersedia)** |

### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/pending-closures` | Daftar pantau penutupan tertunda | `InpatientMonitoring : Read` | Query | `ApiResponse<PendingClosurePagedResult>` | `EPIC RI-29` | **Rencana (belum tersedia)** |
| `GET` | `/bed-drift` | Laporan selisih tempat tidur | `InpatientMonitoring : Read` | Query | `ApiResponse<BedDriftPagedResult>` | `EPIC RI-29` | **Rencana (belum tersedia)** |
| `GET` | `/isolation-mismatch` | Daftar pantau episode yang kebutuhan isolasinya tidak cocok dengan tempat tidur yang ditempati | `InpatientMonitoring : Read` | Query | `ApiResponse<IsolationMismatchPagedResult>` | `EPIC RI-34` | **Rencana (belum tersedia)** |

### Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Mengubah nilai pengaturan | `InpatientSetting : Update` | `UpdateInpatientSettingRequest` | `ApiResponse<InpatientSettingResponse>` | `EPIC RI-31` | **Rencana (belum tersedia)** |

### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/availability` | Menolak nilai terisi dan dipesan | `Bed : Update` | `UpdateBedAvailabilityRequest` | `ApiResponse<BedUpdateResponse>` | `EPIC RI-32` | **Rencana perubahan perilaku** |

---

## 14. Matriks kewenangan

String hak akses yang sudah ada mengikuti
[`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md). String `InpatientDeposit` yang diusulkan `0.5.0` **dicabut** pada `0.6.0`: `BillingManagement` sudah memakai `BillingDeposit` dengan aksi `Read`, `Create`, dan `Allocate` (`BillingPatientFundsController.cs:31-97`). Yang perlu ditambahkan ke matrix tinggal dua aksi baru, `Settle` dan `Refund`.

| Tindakan | Peran | String yang dipakai |
| --- | --- | --- |
| Membuka admisi | Petugas admisi, Supervisor | `[AccessPermission("InpatientEpisode", "Create")]` |
| Memesan dan menempatkan tempat tidur | Petugas admisi, Supervisor | `[AccessPermission("InpatientBedOccupancy", "Create")]` |
| Memindahkan pasien | Kepala ruangan, Perawat, DPJP, Supervisor | `[AccessPermission("InpatientBedOccupancy", "Transfer")]` |
| Mengalihkan DPJP, menugaskan perawat, membatalkan admisi | Kepala ruangan, Supervisor | `[AccessPermission("InpatientEpisode", "Update")]` |
| Menyatakan pasien boleh pulang, menyusun resume | DPJP | `[AccessPermission("InpatientDischarge", "Update")]` |
| Menandatangani resume | DPJP | `[AccessPermission("InpatientDischarge", "Sign")]` |
| Menandai kelayakan keuangan | Kasir, Billing | `[AccessPermission("InpatientFinancialClearance", "Update")]` |
| Memasukkan nominal deposit pada langkah admisi | Petugas admisi, Kasir, Billing | `[AccessPermission("BillingDeposit", "Create")]` — **kepemilikan peran masih terbuka, lihat 20.2** |
| Menerima top-up deposit selama perawatan | Kasir, Billing | `[AccessPermission("BillingDeposit", "Create")]` |
| Melihat kebijakan dan ringkasan deposit episode | Kasir, Billing, Petugas admisi, Supervisor | `[AccessPermission("BillingDeposit", "Read")]` |
| Mengalokasikan deposit ke tagihan | Kasir, Billing | `[AccessPermission("BillingDeposit", "Allocate")]` |
| Menjalankan final settlement deposit | Kasir, Billing | `[AccessPermission("BillingDeposit", "Settle")]` — **baru `0.6.0`** |
| Mencatat dan menyetujui refund deposit | Kasir, Billing sesuai kewenangan finansial | `[AccessPermission("BillingRefund", "Create")]` dan `[AccessPermission("BillingRefund", "Approve")]` — **dikoreksi `0.6.1`**; usulan `BillingDeposit : Refund` dicabut karena resource `BillingRefund` sudah ada |
| Mencatat pasien sudah meninggalkan ruangan | Petugas admisi, Perawat, Kepala ruangan, Supervisor | `[AccessPermission("InpatientDischarge", "RecordDeparture")]` |
| Menutup episode | Petugas admisi, Supervisor | `[AccessPermission("InpatientEpisode", "Close")]` |
| Menutup menembus gerbang keuangan | Supervisor | `[AccessPermission("InpatientEpisode", "CloseOverride")]` |
| Membuka dan menutup sesi koreksi | Supervisor | `[AccessPermission("InpatientEpisode", "Reopen")]` |
| Melihat census dan daftar pantau | Seluruh peran klinis dan admisi | `[AccessPermission("InpatientCensus", "Read")]`, `[AccessPermission("InpatientMonitoring", "Read")]` |
| Menetapkan kebutuhan isolasi | Petugas admisi selagi episode `Draft`; DPJP aktif setelah episode aktif | `[AccessPermission("InpatientEpisode", "SetIsolation")]` |
| Mengubah pengaturan dan butir administrasi | Admin master data | `[AccessPermission("InpatientSetting", "Update")]`, `[AccessPermission("InpatientClearanceItem", "Update")]` |

**Kewenangan yang tidak dijaga mesin hak akses.** **Empat** penjaga berikut ditulis di dalam
service, karena mesin hak akses hanya mengenal peran terhadap endpoint: `GUARD-INP-01` perpindahan
oleh DPJP, `GUARD-INP-02` keputusan pulang, `GUARD-INP-03` penandatanganan resume, dan
`GUARD-INP-04` perubahan kebutuhan isolasi setelah episode aktif.

`GUARD-INP-04` adalah alasan kenapa `SetIsolation` dimiliki dua peran sekaligus pada tabel di atas.
Mesin hak akses hanya dapat menjawab "peran ini boleh memanggil endpoint ini"; ia tidak dapat
membedakan petugas admisi yang menyetel nilai selagi `Draft` dari dokter yang bukan DPJP episode
tersebut. Pembedaannya dikerjakan service.

---

## 15. Batas integrasi dan billing

### 15.1 Yang **tidak boleh** dibuat sendiri modul ini

| Yang tidak boleh dibuat | Pemiliknya |
| --- | --- |
| Salinan pasien, dokter, pegawai, tempat tidur, kamar, unit layanan, kelas pasien | Patient Management, HR Workforce, Master Data |
| Faktur, tagihan berjalan, tarif, perhitungan biaya, refund, klaim | Billing Management dan Insurance Management |
| Tabel pengkajian, catatan dokter, diagnosis, tindakan, resep versi Rawat Inap | Clinical Management dan Pharmacy Management |
| Antrean untuk pasien rawat inap | Laporan antrean poliklinik tidak boleh tercemar |
| Mesin hak akses baru | Sudah ada dan dipakai ulang |

### 15.2 Kelayakan keuangan pada MVP

Sejak `0.5.0`, penandaan `FinancialClearance` **tidak lagi boleh menjadi penandaan manual buta**. Status tetap disimpan pada episode sebagai closure gate, tetapi permintaan mengubahnya ke `Cleared` dilakukan oleh Kasir/Billing dan service wajib membaca ringkasan settlement dari `BillingManagement` terlebih dahulu.

`Cleared` hanya diterima bila tidak ada kekurangan pembayaran dan, bila deposit menghasilkan kelebihan, refund/penyelesaiannya sudah tercatat. `Blocked` tetap dapat dipakai Billing/Kasir untuk menyatakan ada hambatan finansial yang memang diketahui. Bila integrasi Billing tidak dapat dibaca, status **tidak boleh diasumsikan Cleared**; jalur normal tetap `Pending`/`Blocked`.

`CloseOverride` milik Supervisor tetap dipertahankan untuk kebutuhan operasional luar biasa. Override hanya menembus closure gate pada episode; ia **tidak** menghapus piutang, deposit, refund, atau transaksi Billing yang belum selesai. Episode selalu masuk laporan pengecualian.

`RWI-RISK-003` dari revisi sebelumnya — kasir dapat menandai `Cleared` tanpa tagihan yang benar-benar diperiksa — **tidak lagi diterima sebagai jalur normal pada `0.5.0`**. Risiko itu diganti dependency eksplisit terhadap API settlement Billing.

### 15.3 Charge kamar

**Dikoreksi `0.6.1`.** Kalimat berikut sudah **tidak berlaku** sejak `BKC-DEC-043`: Billing
menghitung charge kamar langsung dari `InpBedPlacement` setiap kali invoice dihitung ulang, termasuk
segmen yang masih berjalan, sehingga invoice terbuka menunjukkan estimasi hidup selama pasien masih
dirawat (`BillingCalculationService.cs:455-470`). Yang tetap benar: **modul Rawat Inap tidak
menghitung tarif apa pun sendiri**; ia hanya menyediakan garis waktu penghunian.

~~Tidak satu pun charge kamar tercatat selama MVP.~~ Yang dijamin arsitektur adalah **datanya dapat
direkonstruksi**: dari riwayat penempatan, kelas dan lamanya menempati setiap kamar terbaca lengkap.

Keputusan apakah episode lama ikut ditagihkan mundur adalah keputusan keuangan yang belum ada
pemiliknya.

> **Catatan bukti 2026-09-08.** `MstRoomChargePolicy` sudah ada pada `BillingManagement`, begitu pula
> jalur invoice dan finalisasi. Keputusan MVP di atas tidak berubah — modul Rawat Inap tetap tidak
> menghitung charge kamar — tetapi "Billing belum bisa apa-apa" bukan lagi gambaran yang benar, dan
> asumsi itu tidak boleh dipakai lagi untuk menunda `FR-RI-170` s.d. `FR-RI-172`.

### 15.4 Batas minimum Billing yang wajib ada untuk `EPIC RI-35`

Deposit tidak memaksa `InPatientManagement` membangun billing engine. Namun MVP deposit **tidak dapat dinyatakan selesai** hanya dengan tabel penerimaan uang. Billing/Kasir minimal harus mampu:

1. membuat permintaan deposit awal dan top-up yang merujuk `EpisodeId`;
2. menerima pembayaran secara idempotent dan menerbitkan nomor kwitansi unik;
3. mengembalikan ringkasan total diterima, dialokasikan, refund, saldo tersedia, dan outstanding;
4. menyediakan `FinalBillAmount` atau referensi final bill yang menjadi sumber settlement;
5. mengalokasikan deposit ke final bill tanpa mengubah transaksi penerimaan lama;
6. mencatat kekurangan pembayaran dan refund/kelebihan secara eksplisit;
7. memberi hasil settlement yang dapat diverifikasi oleh `FinancialClearance`;
8. ~~menyimpan `EpisodeId` pada akun deposit~~ — **dicabut `0.6.1`**; penelusuran episode dicapai lewat join pada `EncounterId` yang sudah unique di kedua sisi, tanpa kolom baru; dan
9. menyediakan kebijakan minimum deposit per penjamin dan kelas perawatan yang dapat dibaca langkah admisi.

Bila butir 4 belum tersedia karena charge kamar/full billing belum operasional, deposit tetap dapat **diterima dan ditambah**, tetapi `FR-RI-170` s.d. `FR-RI-172` belum lulus dan MVP end-to-end belum memenuhi Definition of Done. Ini adalah dependency delivery, bukan alasan memindahkan deposit kembali ke POST-MVP.

**Dikoreksi `0.6.1`.** Butir 1, 2, 3, 5, 6, dan sebagian 7 **sudah terpenuhi** oleh source hasil
merge: penerimaan dan top-up berjalan lewat `POST /patient-funds/deposits/{encounterId}/top-ups`
dengan idempotensi terkunci di database, alokasi lewat `/allocations`, refund lewat
`/financial-exceptions/refunds`, dan tagihan final lewat `charge-summary`. Yang benar-benar
tersisa tinggal **butir 9** — kebijakan minimum deposit — ditambah satu ringkasan per episode yang
menggabungkan angka-angka itu menjadi satu jawaban. Keduanya prasyarat `EPIC RI-35a`.

---

## 16. Guardrail regulasi

| Kewajiban | Yang dipenuhi MVP | Yang belum |
| --- | --- | --- |
| Rekam medis elektronik | Resume pulang tersimpan, tertandatangani, dan terkunci setelah episode ditutup. Riwayat lokasi, DPJP, dan status tersimpan lengkap | Pengkajian, catatan dokter, dan CPPT belum masuk sistem. Sejak `RWI-DEC-080` ketiganya **sudah masuk scope modul** dan `RWI-DEC-083` memberikannya kepada sub-modul `keperawatan/` serta `dokter-rawat-inap/` yang belum dirancang. **Bukan** lagi `DEC-INP-001`, yang tertutup `RWI-DEC-062` 2026-08-21 |
| Keterlacakan tindakan | Setiap perubahan status meninggalkan jejak yang tidak dapat diubah, lengkap dengan pelaku dan waktu | — |
| Keterlacakan transaksi deposit | Setiap penerimaan, top-up, alokasi, refund, reversal, dan override finansial mempunyai referensi transaksi, pelaku, waktu, dan tidak dihapus saat episode ditutup/dibatalkan | Detail akuntansi/GL tetap milik Billing/Finance dan tidak ditentukan PRD Rawat Inap |
| Koreksi rekam medis | Koreksi hanya lewat sesi koreksi supervisor, beralasan, daftar perubahannya tersimpan, dan versi resume sebelumnya tersalin | — |
| Pengendalian infeksi dan privasi kamar | Penempatan dan perpindahan **ditolak** bila jenis kelamin tidak cocok, bila kamar sedang dihuni jenis kelamin berbeda, atau bila kebutuhan isolasi tidak cocok dengan sifat tempat tidur. Kebutuhan isolasi tersimpan beserta siapa dan kapan menetapkannya | Kebutuhan isolasi tersimpan sebagai **nilai berlaku**, bukan riwayat. Bila kelak audit pengendalian infeksi menuntut rentang tanggalnya, dibutuhkan Amendment Pass |
| Masa simpan data | — | **Belum diputuskan** — `RWI-OQ-035`, keputusan hukum |
| Interoperabilitas nasional | — | **Belum diputuskan** — `DEC-INP-005` |
| Persetujuan pasien | — | **Belum diputuskan** — `DEC-INP-003` |

**Yang wajib disadari:** **tiga** baris terakhir adalah gerbang keras. Modul ini **tidak boleh**
dipakai melayani pasien sungguhan sebelum ketiganya terjawab, walaupun MVP-nya sudah selesai
dikerjakan.

**Satu gerbang keras dicabut pada `0.3.0`.** Pengendalian infeksi dan privasi kamar sebelumnya
tercatat "belum diputuskan" dan menahan pemakaian modul untuk pasien sungguhan. `RWI-DEC-064`
sampai `RWI-DEC-066` menurunkan keputusannya pada 2026-08-21, dan `EPIC RI-34` mengerjakannya di
dalam MVP. Gerbangnya kini berpindah bentuk: bukan lagi menunggu keputusan, melainkan menunggu
`EPIC RI-34` benar-benar lolos uji.

---

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan | Isinya |
| --- | --- | --- |
| `NFR-001` | Keutuhan tindakan | Penempatan, perpindahan, pembatalan, dan penutupan bersifat utuh: berhasil seluruhnya atau tidak ada yang berubah |
| `NFR-002` | Pencegahan tabrakan | Satu tempat tidur tidak pernah dipegang dua episode, dan satu pasien tidak pernah punya dua episode yang hadir. Dijaga penguncian baris ditambah **empat** unique index parsial |
| `NFR-003` | Jejak audit | Setiap perpindahan status meninggalkan baris riwayat yang tidak dapat diubah, ditulis dalam transaksi yang sama |
| `NFR-004` | Otorisasi | Hak akses per peran memakai mesin yang sudah ada; kewenangan per pasien dijaga di dalam service |
| `NFR-005` | Koreksi | Kesalahan dibetulkan lewat sesi koreksi, bukan lewat penyuntingan diam-diam |
| `NFR-006` | Penanganan waktu | Seluruh waktu UTC. Kedaluwarsa dihitung saat dibaca, tanpa program penjadwal. Lama dirawat dari selisih tanggal |
| `NFR-007` | Privasi | Kolom sensitif tidak masuk log dan tidak tampil pada daftar |
| `NFR-008` | Test regresi | Setiap task yang menyentuh modul milik pihak lain membawa test regresi jalur lama |
| `NFR-009` | Idempotensi finansial | Penerimaan deposit, settlement, dan refund memakai idempotency key/reference unik sehingga retry tidak menghasilkan transaksi uang ganda |
| `NFR-010` | Keutuhan nilai uang | Semua nilai finansial memakai tipe desimal yang sesuai domain Billing, bukan floating point; total ringkasan harus dapat direkonsiliasi ke transaksi sumber |
| `NFR-011` | Konsistensi lintas modul | Penutupan episode tidak menghapus atau menulis ulang ledger Billing. Kegagalan sinkronisasi clearance menghasilkan status aman (`Pending`/`Blocked`) dan jejak error, bukan `Cleared` optimistis |

---

## 18. Skenario UAT

Setiap epic `MUST HAVE` punya sekurang-kurangnya satu skenario berhasil dan satu skenario gagal.

> **`UAT-01` — Satu pasien dari masuk sampai pulang** (`EPIC RI-21` s.d. `RI-28`, `EPIC RI-35`)
> **Kondisi awal:** master kamar dan tempat tidur terisi; `BD-RSMMC-00042` tersedia; kebijakan finansial menyatakan deposit awal Rp5.000.000 diperlukan.
> **Langkah:** petugas admisi membuka admisi Tn. Budi; kasir menerima deposit Rp5.000.000; petugas memesan tempat tidur dan menempatkan; kepala ruangan menugaskan perawat; selama perawatan kasir menerima top-up Rp3.000.000; DPJP menyatakan boleh pulang dan menandatangani resume; petugas menandai tiga butir administrasi; Billing menjalankan final settlement, menyelesaikan kekurangan/refund bila ada; kasir meminta `FinancialClearance = Cleared`; petugas menutup episode.
> **Hasil yang diharapkan:** episode `Closed`, `BD-RSMMC-00042` kembali tersedia, dua transaksi deposit dan dua kwitansi tetap terbaca, settlement berstatus selesai, tidak ada saldo finansial yang menggantung, riwayat status memuat empat baris berurutan, resume tersimpan tertandatangani.

> **`UAT-02` — Dua petugas merebut tempat tidur yang sama** (`EPIC RI-23`)
> **Kondisi awal:** `BD-RSMMC-00042` tersedia.
> **Langkah:** dua petugas menempatkan pasien berbeda ke tempat tidur itu pada waktu hampir
> bersamaan.
> **Hasil yang diharapkan:** satu berhasil, satu ditolak dengan pesan yang terbaca pengguna. Census
> menampilkan tepat satu pasien pada tempat tidur tersebut.

> **`UAT-03` — Pemesanan gugur sendiri** (`EPIC RI-22`)
> **Kondisi awal:** pemesanan dibuat pukul 09:15 dengan batas 2 jam.
> **Langkah:** buka daftar tempat tidur pukul 11:14, lalu pukul 11:16. Tidak ada proses latar
> belakang yang dijalankan.
> **Hasil yang diharapkan:** pukul 11:14 masih terkunci; pukul 11:16 sudah tersedia.

> **`UAT-04` — Memesan tempat tidur yang sudah dipesan** (`EPIC RI-22`, gagal)
> **Hasil yang diharapkan:** ditolak dengan pesan "Tempat tidur ini sudah dipesan untuk pasien
> lain". Tidak ada pemesanan kedua yang tersimpan.

> **`UAT-05` — Lama dirawat pasien yang menginap semalam** (`EPIC RI-24`)
> **Kondisi awal:** pasien masuk 21 September pukul 22:30.
> **Langkah:** buka census pada 22 September pukul 06:00.
> **Hasil yang diharapkan:** lama dirawat tertulis **1 hari**, dan layar menjelaskan bahwa itu
> hitungan hari rawat, bukan lama waktu sebenarnya.

> **`UAT-06` — Census tidak menampilkan episode yang belum aktif** (`EPIC RI-24`, gagal)
> **Kondisi awal:** ada episode `Draft`, `Closed`, dan `Cancelled`.
> **Hasil yang diharapkan:** ketiganya **tidak** muncul di census.

> **`UAT-07` — Pengalihan DPJP dan buktinya** (`EPIC RI-25`)
> **Langkah:** dr. Andi memindahkan pasien pada hari kedua. Pada hari ketiga DPJP dialihkan ke
> dr. Rina. Buka riwayat DPJP pada hari kelima.
> **Hasil yang diharapkan:** riwayat menampilkan dua baris berperiode, dan perpindahan hari kedua
> masih terbukti diminta dokter yang saat itu berwenang.

> **`UAT-08` — Dokter jaga mencoba memindahkan pasien orang lain** (`EPIC RI-26`, gagal)
> **Langkah:** dr. Rina yang bukan DPJP memindahkan Tn. Budi.
> **Hasil yang diharapkan:** ditolak dengan pesan "Hanya DPJP episode ini yang dapat memindahkan
> pasien". Tidak ada kolom keterangan yang dapat dipakai melewatinya.

> **`UAT-09` — Perpindahan gagal di tengah jalan** (`EPIC RI-26`, gagal)
> **Langkah:** paksa kegagalan saat penempatan baru dibuka.
> **Hasil yang diharapkan:** Tn. Budi tetap tercatat di tempat tidur semula. Tidak pernah ada saat
> pasien tercatat tanpa tempat tidur.

> **`UAT-10` — Resume ditandatangani orang yang salah** (`EPIC RI-27`, gagal)
> **Langkah:** dokter yang bukan DPJP aktif menandatangani resume.
> **Hasil yang diharapkan:** ditolak dengan pesan yang menyebut alasannya.

> **`UAT-11` — Menutup episode yang syaratnya belum lengkap** (`EPIC RI-28`, `EPIC RI-35`, gagal)
> **Langkah:** petugas admisi menutup episode pukul 10:00, sementara resume belum ditandatangani dan settlement finansial masih mempunyai kekurangan pembayaran.
> **Hasil yang diharapkan:** ditolak, dan layar menampilkan **kelima syarat** beserta tanda sudah atau belum. Syarat finansial menjelaskan bahwa settlement belum selesai, bukan sekadar satu kalimat umum.

> **`UAT-12` — Supervisor menutup menembus gerbang keuangan** (`EPIC RI-28`)
> **Langkah:** kasir tidak di tempat, pasien harus segera pulang. Supervisor menutup disertai alasan.
> **Hasil yang diharapkan:** episode `Closed`, ditandai, dan muncul pada laporan pengecualian.

> **`UAT-13` — Supervisor mencoba menembus syarat selain keuangan** (`EPIC RI-28`, gagal)
> **Langkah:** supervisor menutup sementara resume belum ditandatangani.
> **Hasil yang diharapkan:** tetap ditolak. Jalan keluar hanya menembus syarat keuangan.

> **`UAT-14` — Koreksi cara pulang setelah episode ditutup** (`EPIC RI-30`)
> **Kondisi awal:** episode Ibu Sari ditutup 15 Agustus; `MELATI-03` sudah ditempati pasien lain.
> **Langkah:** pada 17 Agustus supervisor membuka sesi koreksi, mengubah cara pulang, menutup sesi
> beserta daftar perubahan.
> **Hasil yang diharapkan:** cara pulang berubah; status episode tetap Selesai; `MELATI-03` tidak
> terganggu; lama dirawat tetap 3 hari; Ibu Sari tidak muncul di census.

> **`UAT-15` — Menutup sesi koreksi tanpa daftar perubahan** (`EPIC RI-30`, gagal)
> **Hasil yang diharapkan:** ditolak. Ini satu-satunya jejak koreksi.

> **`UAT-16` — Riwayat pemesanan yang gugur** (`EPIC RI-29`)
> **Hasil yang diharapkan:** baris riwayat bertanda dilakukan sistem, tanpa nama orang.

> **`UAT-17` — Mencoba menghapus riwayat status** (`EPIC RI-29`, gagal)
> **Hasil yang diharapkan:** tidak ada endpoint yang menyediakannya.

> **`UAT-18` — Admin mengubah batas pemesanan** (`EPIC RI-31`)
> **Langkah:** ubah dari 2 jam menjadi 3 jam pukul 14:00.
> **Hasil yang diharapkan:** pemesanan pukul 14:05 berlaku sampai 17:05. Pemesanan yang dibuat
> pukul 13:30 tetap memakai batas lama.

> **`UAT-19` — Modul berjalan tanpa baris pengaturan** (`EPIC RI-31`, gagal)
> **Kondisi awal:** lingkungan baru tanpa `MstInpatientSetting`.
> **Hasil yang diharapkan:** modul tetap berjalan memakai nilai bawaan, dan peringatan tercatat.

> **`UAT-20` — Menonaktifkan tempat tidur yang rusak** (`EPIC RI-32`)
> **Langkah:** admin menonaktifkan `BD-RSMMC-00042` dari halaman detail.
> **Hasil yang diharapkan:** berhasil, dan tempat tidur hilang dari pencarian. Hari ini tombol ini
> selalu gagal.

> **`UAT-21` — Admin mencoba menyetel tempat tidur menjadi terisi** (`EPIC RI-32`, gagal)
> **Hasil yang diharapkan:** ditolak dengan pesan yang mengarahkan ke modul Rawat Inap. Menyetel
> Perbaikan tetap berhasil.

> **`UAT-22` — Bayi dirawat gabung dengan ibunya** (`EPIC RI-33`)
> **Kondisi awal:** boks `BOX-MELATI-03-A` terdaftar sebagai tempat tidur di kamar Melati 3.
> **Langkah:** bayi Ny. Sari didaftarkan, dibuatkan episode sendiri, lalu ditempatkan di boks itu.
> **Hasil yang diharapkan:** census menampilkan dua baris. Menutup episode Ny. Sari tidak menutup
> episode bayinya.

> **`UAT-24` — Tempat tidur bebas sejak pasien meninggalkan kamar** (`EPIC RI-28`)
> **Kondisi awal:** Tn. Budi berstatus rencana pulang di `BD-RSMMC-00105`.
> **Langkah:** keluarga menjemput pukul 10:15. Perawat mencatat kepergiannya. Pukul 10:40 petugas
> admisi memesan tempat tidur itu untuk Ny. Sari. Episode Tn. Budi baru ditutup pukul 13:10.
> **Hasil yang diharapkan:** pemesanan pukul 10:40 berhasil. Episode Tn. Budi tetap berstatus
> rencana pulang sampai 13:10 dan tetap muncul pada daftar pantau penutupan tertunda.

> **`UAT-25` — Mencatat kepergian pasien yang belum diputuskan pulang** (`EPIC RI-28`, gagal)
> **Langkah:** perawat mencatat kepergian pada episode yang masih berstatus sedang dirawat.
> **Hasil yang diharapkan:** ditolak dengan pesan yang menyebut bahwa DPJP harus menyatakan pasien
> boleh pulang lebih dulu. Tempat tidur tidak berubah.

> **`UAT-26` — Satu pasien tidak dapat dirawat di dua tempat** (`EPIC RI-23`, gagal)
> **Kondisi awal:** Tn. Budi sedang dirawat di Melati 3B.
> **Langkah:** petugas lain menempatkan Tn. Budi di Anggrek 1A.
> **Hasil yang diharapkan:** ditolak dengan pesan yang menyebut nomor episode dan lokasi yang
> sedang ditempati, sehingga petugas tahu yang dibutuhkan adalah perpindahan.

> **`UAT-27` — Koreksi resume menyimpan versi lamanya** (`EPIC RI-27`)
> **Kondisi awal:** resume Ibu Sari sudah ditandatangani dr. Andi dengan cara pulang "kabur".
> **Langkah:** supervisor membuka sesi koreksi, mengubah cara pulang menjadi "atas permintaan
> sendiri", lalu menutup sesi.
> **Hasil yang diharapkan:** resume yang berlaku menampilkan cara pulang baru, dan versi lama
> beserta nama penandatangannya tetap dapat dibaca.

> **`UAT-28` — Perawat menemukan bayi siapa yang ada di boks** (`EPIC RI-33`)
> **Kondisi awal:** bayi Ny. Sari ditempatkan di `BOX-MELATI-03-A`.
> **Langkah:** perawat membuka detail boks tersebut.
> **Hasil yang diharapkan:** sistem menyebut bahwa penghuninya bayi Ny. Sari yang dirawat di
> Melati 3, bukan hanya menampilkan nama bayi tanpa hubungan.

> **`UAT-29` — Kamar tidak menjadi campur** (`EPIC RI-34`, gagal)
> **Kondisi awal:** Kamar Melati 3 berisi tiga tempat tidur. Ny. Sari menempati `MELATI-03-A`.
> **Langkah:** petugas admisi menempatkan Tn. Budi di `MELATI-03-B`.
> **Hasil yang diharapkan:** ditolak dengan kode 422 dan pesan yang **menyebut nama kamarnya**,
> supaya petugas langsung tahu kamar mana yang terhalang. Berikutnya Ny. Rina ditempatkan di tempat
> tidur yang sama dan **berhasil**.

> **`UAT-30` — Bayi tidak menutup kamar dan tidak tertutup kamar** (`EPIC RI-34`)
> **Kondisi awal:** Ny. Sari di `MELATI-03-A`, bayinya laki-laki.
> **Langkah:** perawat menempatkan bayi di boks `BOX-MELATI-03-A`, lalu petugas menempatkan Ny.
> Rina di `MELATI-03-B`.
> **Hasil yang diharapkan:** keduanya **berhasil**. Penempatan bayi tidak ditolak walaupun jenis
> kelaminnya berbeda dari penghuni kamar, dan kehadiran bayi laki-laki itu tidak menghalangi
> Ny. Rina.

> **`UAT-31` — Tempat tidur isolasi dijaga dari dua arah** (`EPIC RI-34`, gagal)
> **Kondisi awal:** Tn. Budi bertanda membutuhkan isolasi. `BD-RSMMC-00042` bukan tempat tidur
> isolasi; `ISO-01-A` adalah tempat tidur isolasi. Ny. Rina tidak membutuhkan isolasi.
> **Langkah:** petugas menempatkan Tn. Budi di `BD-RSMMC-00042`, lalu menempatkan Ny. Rina di
> `ISO-01-A`.
> **Hasil yang diharapkan:** keduanya ditolak dengan kode 422 dan pesan yang berbeda — yang pertama
> menyebut pasien membutuhkan isolasi, yang kedua menyebut kapasitas isolasi tidak boleh terpakai
> pasien biasa.

> **`UAT-32` — Petugas admisi merekam, DPJP memutuskan** (`EPIC RI-34`)
> **Kondisi awal:** episode Tn. Budi masih `Draft`, surat rujukan menyebut suspek penyakit menular.
> **Langkah:** petugas admisi menyalakan kebutuhan isolasi disertai keterangan. Setelah episode
> aktif, dr. Rina yang bukan DPJP mencoba mematikannya. Kemudian dr. Andi selaku DPJP aktif
> mematikannya.
> **Hasil yang diharapkan:** yang pertama tersimpan bertanda **catatan awal** atas nama petugas
> admisi. Percobaan dr. Rina ditolak dengan kode 403. Perubahan dr. Andi tersimpan bertanda
> **keputusan klinis** atas namanya. Percobaan menyalakan tanpa keterangan ditolak dengan kode 400.

> **`UAT-33` — Perubahan isolasi tidak pernah ditahan** (`EPIC RI-34`)
> **Kondisi awal:** Tn. Budi sedang berbaring di `MELATI-03-B` yang bukan tempat tidur isolasi.
> **Langkah:** dr. Andi menyalakan kebutuhan isolasi pukul 14:00. Petugas membuka daftar pantau
> penempatan tidak sesuai. Pukul 15:20 Tn. Budi dipindahkan ke `ISO-01-A`.
> **Hasil yang diharapkan:** pencatatan pukul 14:00 **diterima**, tidak ditahan. Episode Tn. Budi
> muncul pada daftar pantau di antara pukul 14:00 dan 15:20, lalu hilang dari sana setelah
> dipindahkan. Perpindahan itu sendiri lolos karena tempat tidur tujuannya isolasi.

> **`UAT-34` — Deposit awal menghasilkan transaksi dan kwitansi unik** (`EPIC RI-35`)
> **Kondisi awal:** episode Tn. Budi `Draft`; Billing menyatakan deposit Rp5.000.000 diperlukan.
> **Langkah:** kasir menerima pembayaran Rp5.000.000.
> **Hasil yang diharapkan:** satu transaksi penerimaan terbentuk, terikat pada `EpisodeId`, mempunyai nomor kwitansi unik, dan ringkasan episode menunjukkan total diterima Rp5.000.000.

> **`UAT-35` — Top-up tidak menimpa deposit lama** (`EPIC RI-35`)
> **Kondisi awal:** sudah ada deposit Rp5.000.000.
> **Langkah:** kasir menerima top-up Rp3.000.000 lalu membuka histori.
> **Hasil yang diharapkan:** dua transaksi dan dua kwitansi tetap ada; total diterima Rp8.000.000. Tidak ada update nominal pada transaksi pertama.

> **`UAT-36` — Retry pembayaran tidak membuat deposit ganda** (`EPIC RI-35`, gagal aman)
> **Langkah:** request penerimaan Rp2.000.000 dikirim dua kali memakai `idempotencyKey` yang sama karena response pertama timeout.
> **Hasil yang diharapkan:** hanya satu transaksi Rp2.000.000 dan satu kwitansi yang tersimpan; response retry menunjuk transaksi yang sama.

> **`UAT-37` — Deposit kurang dari tagihan final** (`EPIC RI-35`)
> **Kondisi awal:** deposit tersedia Rp10.000.000; final bill Rp12.000.000.
> **Langkah:** Billing menjalankan settlement, lalu keluarga membayar kekurangan Rp2.000.000.
> **Hasil yang diharapkan:** sebelum pembayaran, clearance ditolak dan kekurangan Rp2.000.000 terlihat. Setelah pembayaran, settlement selesai dan `FinancialClearance` dapat menjadi `Cleared`.

> **`UAT-38` — Deposit lebih besar dari tagihan final** (`EPIC RI-35`)
> **Kondisi awal:** deposit tersedia Rp10.000.000; final bill Rp7.500.000.
> **Langkah:** Billing menjalankan settlement lalu mencatat refund Rp2.500.000.
> **Hasil yang diharapkan:** sebelum refund diselesaikan, `Cleared` ditolak. Setelah transaksi refund tercatat sesuai kebijakan Billing, settlement selesai; seluruh transaksi deposit awal tetap terbaca.

> **`UAT-39` — Kasir mencoba clearance dengan settlement belum selesai** (`EPIC RI-28`, `EPIC RI-35`, gagal)
> **Langkah:** kasir meminta `Cleared` saat masih ada outstanding top-up/kekurangan atau refund pending.
> **Hasil yang diharapkan:** ditolak 422 dengan rincian alasan finansial yang dapat ditindaklanjuti.

> **`UAT-40` — Pembatalan admisi setelah deposit tidak menghapus uang** (`EPIC RI-21`, `EPIC RI-35`)
> **Kondisi awal:** episode `Draft`, deposit Rp5.000.000 sudah diterima.
> **Langkah:** supervisor membatalkan admisi dan Billing menjalankan refund/reversal.
> **Hasil yang diharapkan:** episode `Cancelled`; transaksi penerimaan dan kwitansi awal tetap ada; transaksi refund/reversal tercatat terpisah; saldo episode selesai. Bila refund/reversal gagal, pembatalan normal tidak berpura-pura menyelesaikan uang dan exception tetap terlihat.

> **`UAT-41` — Langkah Deposit muncul pada urutan yang benar dan minimumnya dari kebijakan** (`EPIC RI-35`)
> **Kondisi awal:** kebijakan deposit untuk pasien umum kelas VIP bernilai minimum Rp100.000.000.
> **Langkah:** petugas memilih pasien lama, tipe pasien, lalu kategori pembayaran tunai dan kelas VIP, kemudian menekan lanjut.
> **Hasil yang diharapkan:** layar berikutnya adalah langkah Deposit, bukan langkah Dokter. Minimum yang tampil Rp100.000.000 dan berasal dari response kebijakan, bukan dari angka yang ditulis di layar. Untuk penjamin yang tidak mensyaratkan deposit, langkah ini dilewati tanpa transaksi Rp0.

> **`UAT-42` — Deposit di bawah minimum tetap melanjutkan admisi** (`EPIC RI-35`)
> **Kondisi awal:** minimum kebijakan Rp100.000.000.
> **Langkah:** petugas memasukkan Rp40.000.000 lalu menekan lanjut.
> **Hasil yang diharapkan:** peringatan selisih Rp60.000.000 tampil, tombol lanjut **tidak** terkunci, dan setelah episode terbentuk ringkasan deposit episode menunjukkan kekurangan Rp60.000.000.

> **`UAT-43` — Episode gagal dibuat berarti tidak ada uang tercatat** (`EPIC RI-35`, gagal aman)
> **Kondisi awal:** petugas mengisi deposit Rp5.000.000; kunjungan yang dipakai ternyata sudah punya episode.
> **Langkah:** petugas menekan lanjut pada langkah Dokter dan `POST /episodes` ditolak 409.
> **Hasil yang diharapkan:** tidak ada transaksi penerimaan dan tidak ada kwitansi terbit. Nominal deposit tetap terisi di layar, dan percobaan ulang setelah kunjungan diperbaiki hanya menghasilkan satu transaksi.

> **`UAT-44` — Penagihan pelunasan berkala pada perawatan panjang** (`EPIC RI-35`)
> **Kondisi awal:** kekurangan deposit Rp60.000.000; ambang tindak lanjut tiga hari.
> **Langkah:** episode berjalan sembilan hari tanpa top-up, lalu keluarga membayar Rp60.000.000 pada hari kesembilan.
> **Hasil yang diharapkan:** episode muncul pada daftar pantau kekurangan deposit di hari ketiga, keenam, dan kesembilan; setelah pembayaran ia hilang dari daftar tanpa mengubah transaksi deposit sebelumnya. Episode yang selesai pada hari kedua tidak pernah muncul di daftar itu.

> **`UAT-23` — Membatalkan admisi setelah pasien dirawat** (`EPIC RI-21`, gagal)
> **Langkah:** petugas admisi membatalkan episode berstatus Sedang dirawat.
> **Hasil yang diharapkan:** ditolak. Hanya supervisor atau kepala ruangan yang boleh.

---

## 19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Satu pasien dapat berjalan dari admisi sampai tempat tidur dilepas | `UAT-01` |
| Tempat tidur ganda tidak mungkin terjadi | `UAT-02`, `UAT-04` |
| Pemesanan gugur sendiri tanpa program penjadwal | `UAT-03` |
| Lama dirawat dihitung dari selisih tanggal dan terbaca jelas maknanya | `UAT-05` |
| Census hanya menampilkan pasien yang benar-benar sedang dirawat | `UAT-06` |
| Sistem dapat menjawab siapa DPJP pada tanggal tertentu | `UAT-07` |
| Dokter yang bukan DPJP tidak dapat memindahkan pasien | `UAT-08` |
| Pasien tidak pernah tercatat tanpa tempat tidur | `UAT-09` |
| Resume hanya dapat ditandatangani DPJP aktif | `UAT-10` |
| Kelima syarat penutupan diperiksa dan dilaporkan satu per satu | `UAT-11` |
| Jalan keluar supervisor hanya menembus syarat keuangan | `UAT-12`, `UAT-13` |
| Koreksi tidak mengganggu tempat tidur dan tidak menambah lama dirawat | `UAT-14` |
| Setiap koreksi meninggalkan jejak | `UAT-15` |
| Perubahan yang dihitung sistem tidak menuduh orang | `UAT-16` |
| Riwayat status tidak dapat dihapus | `UAT-17` |
| Seluruh angka dapat diubah admin dan berlaku pada pembacaan berikutnya | `UAT-18`, `UAT-19` |
| Admin tetap dapat menutup tempat tidur yang rusak | `UAT-20` |
| Status penghunian hanya lahir dari modul Rawat Inap | `UAT-21` |
| Bayi mendapat episode sendiri di boks kamar ibunya | `UAT-22` |
| Sistem dapat menjawab bayi siapa yang berada di boks kamar mana | `UAT-28` |
| Tempat tidur bebas sejak pasien meninggalkan kamar, tanpa menunggu penutupan | `UAT-24` |
| Kepergian hanya dapat dicatat setelah DPJP menyatakan pasien boleh pulang | `UAT-25` |
| Satu pasien tidak pernah tercatat dirawat di dua tempat | `UAT-26` |
| Koreksi resume yang sudah ditandatangani menyimpan versi lamanya | `UAT-27` |
| Pembatalan setelah pasien dirawat hanya oleh peran yang berwenang | `UAT-23` |
| Kamar tidak pernah menjadi campur laki-laki dan perempuan | `UAT-29` |
| Boks bayi dikecualikan dari kedua sisi pemeriksaan jenis kelamin | `UAT-30` |
| Kapasitas isolasi terjaga dari dua arah | `UAT-31` |
| Catatan awal admisi dapat dibedakan dari keputusan klinis DPJP | `UAT-32` |
| Pencatatan klinis tidak pernah ditahan demi aturan penempatan | `UAT-33` |
| Deposit awal dan top-up tersimpan sebagai transaksi terpisah dengan kwitansi unik | `UAT-34`, `UAT-35` |
| Retry transaksi finansial tidak menghasilkan deposit ganda | `UAT-36` |
| Kekurangan setelah alokasi deposit wajib diselesaikan sebelum clearance normal | `UAT-37`, `UAT-39` |
| Kelebihan deposit menghasilkan refund/penyelesaian yang eksplisit | `UAT-38` |
| Pembatalan admisi tidak pernah menghapus histori penerimaan deposit | `UAT-40` |
| Langkah Deposit berdiri di antara langkah Pembayaran dan langkah Dokter | `UAT-41` |
| Minimum deposit dibaca dari kebijakan penjamin dan kelas, bukan angka tetap di layar | `UAT-41` |
| Deposit di bawah minimum tidak pernah menghentikan admisi | `UAT-42` |
| Gagalnya pembuatan episode tidak meninggalkan transaksi deposit menggantung | `UAT-43` |
| Kekurangan deposit tertagih berkala pada perawatan yang melewati ambang hari | `UAT-44` |
| Tidak ada controller deposit kedua; yang dipakai adalah rute `patient-funds` yang diperluas | Review arsitektur `EPIC RI-35`, bagian 13 |
| Tidak ada entity/ledger deposit baru di `InPatientManagement`; ledger tetap milik Billing/Kasir | Review arsitektur `EPIC RI-35`, bagian 12 dan 15 |
| Aturan penempatan berlaku sama pada perpindahan | `UAT-29` dijalankan ulang lewat perpindahan; `RWI-AC-133` |
| Seluruh tabel master MVP sudah terisi | Rencana data master awal pada `02-backend-architecture.md` bagian 8 |
| Setiap task yang menyentuh modul lain membawa test regresi | `RWI-AC-114`, `testing/acceptance-test-matrix.md` bagian 12 |

---

## 20. Urutan pengiriman dan pertanyaan terbuka

### 20.1 Gelombang pengiriman

Ditulis sebagai gelombang, bukan tanggal. Penjadwalan tetap wewenang manusia.

| Gelombang | Epic yang tercakup | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | `EPIC RI-21` fondasi, `EPIC RI-31` pengaturan, `EPIC RI-32` perbaikan tempat tidur | Blueprint disetujui; persetujuan pemilik Master Data untuk `RI-32` |
| `MVP-1` | `EPIC RI-22` pemesanan, `EPIC RI-23` penempatan beserta aturan satu pasien satu episode, `EPIC RI-24` census, **`EPIC RI-34` kelayakan penempatan**, **`EPIC RI-35a` langkah Deposit pada admisi** | `MVP-0` selesai; master kamar dan tempat tidur terisi **beserta penanda jenis kelamin, isolasi, dan boks bayi yang benar**; kebijakan deposit per penjamin dan kelas terisi; `EpisodeId` sudah ada pada akun deposit Billing |
| `MVP-2` | `EPIC RI-25` penanggung jawab, `EPIC RI-26` perpindahan | `MVP-1` selesai |
| `MVP-3` | `EPIC RI-27` pulang, resume, dan versi resume; `EPIC RI-28` penutupan dan pencatatan kepergian fisik; **`EPIC RI-35b` settlement, refund, dan gerbang clearance finansial** | `MVP-2` selesai; contract minimum Billing/Kasir untuk final settlement sudah disinkronkan |
| `MVP-4` | `EPIC RI-29` riwayat dan daftar pantau, `EPIC RI-30` sesi koreksi, `EPIC RI-33` bayi beserta penanda rawat gabung | `MVP-3` selesai |
| `POST-MVP` | Seluruh kemampuan yang ditunda pada bagian 8 | Di luar cakupan rilis pertama; masing-masing menunggu Decision ID-nya |

**`MVP-1` adalah gelombang pertama yang menghasilkan data nyata.** Setelah gelombang itu selesai,
rumah sakit sudah dapat mencatat siapa menempati tempat tidur mana — kemampuan yang hari ini sama
sekali tidak ada.

**`EPIC RI-34` sengaja ditaruh di `MVP-1`, bukan digeser ke gelombang belakang.** Alasannya: aturan
penempatan yang menolak harus sudah berlaku **sejak data penempatan pertama lahir**. Bila epic ini
menyusul di `MVP-4`, gelombang sebelumnya akan lebih dulu menghasilkan penempatan yang melanggar,
dan penempatan yang sudah telanjur ada tidak dapat ditolak surut. Konsekuensinya, syarat mulai
`MVP-1` bertambah: penanda pada master tempat tidur harus **benar**, bukan sekadar terisi.

Tidak ada satu pun epic berstatus `OPEN DECISION` yang masuk gelombang mana pun. Sembilan
kemampuan yang ditunda pada bagian 8 seluruhnya berada di `POST-MVP`.

**`EPIC RI-35` dipecah dua sejak `0.6.0`.** `RI-35a` — langkah Deposit pada admisi, kebijakan minimum, peringatan kekurangan, pengikatan `EpisodeId`, ringkasan episode, dan daftar pantau kekurangan — pindah ke `MVP-1`, karena layar admisi operasional sudah memilikinya; menundanya berarti gelombang pertama menghasilkan episode yang nominal depositnya hilang dari sistem. `RI-35b` — final settlement, refund, dan validasi `FinancialClearance` — tetap di `MVP-3` bersama discharge dan closure, karena ia bergantung pada tagihan final. Rilis MVP end-to-end tetap **tidak boleh dinyatakan selesai** sebelum `UAT-34` s.d. `UAT-44` lulus seluruhnya.

### 20.2 Pertanyaan terbuka sebelum development lock

**Empat pertanyaan yang memblokir seluruhnya tertutup pada 2026-08-21.** Daftar di bawah
mempertahankan barisnya beserta jawabannya, bukan menghapusnya, supaya pembaca berikutnya tahu
kenapa keputusannya berbunyi demikian. **Satu pertanyaan memblokir yang baru terbuka pada
2026-09-08** menyusul di baris terakhir, dan ia hanya menahan `EPIC RI-35a`.

| Pertanyaan | Siapa yang menjawab | Status | Memblokir |
| --- | --- | --- | :---: |
| Siapa nama orang atau komite yang berwenang menyetujui modul ini? | Manajemen rumah sakit | **Tertutup** `RWI-DEC-061` — Muhammad Hamzah, ditunjuk 2026-08-21. Jabatan formalnya belum diisi | ~~Ya~~ |
| Apakah pemilik `MasterData` menyetujui pembatasan endpoint ketersediaan tempat tidur? | Pemilik `MasterData` | **Tertutup** `RWI-DEC-062` — keempat modul tetangga berada di bawah kepemilikan yang sama, dan persetujuannya diberikan | ~~Ya~~ |
| Siapa yang bertanggung jawab mengisi master kamar dan tempat tidur, dan kapan batasnya? | Manajemen rumah sakit | **Tertutup** `RWI-DEC-063` — Admin Master Data / Tim Master Data, target 22 Agustus 2026. Gerbangnya baru benar-benar tertutup ketika datanya terisi, bukan ketika penanggung jawabnya ditunjuk | Sampai data terisi |
| Apakah kebutuhan isolasi dan pemisahan jenis kelamin menolak penempatan, atau hanya menyaring? | Pemilik klinis dan privasi | **Tertutup** `RWI-DEC-064` s.d. `RWI-DEC-066` — **menolak**. Dikerjakan `EPIC RI-34` di dalam MVP | ~~Ya, untuk produksi~~ |
| Berapa lama riwayat status disimpan sebelum boleh diarsipkan? | Pemilik keamanan dan privasi | Sudah dijawab `RWI-DEC-060`, menunggu pemilik hukum. Tidak memblokir MVP | Tidak |
| Apakah kepergian fisik pasien dicatat sebagai kejadian tersendiri? | Pemilik proses | **Tertutup** `RWI-DEC-055` — ya, dan tempat tidur bebas sejak saat itu | Tidak |
| Apakah satu pasien boleh punya dua episode aktif sekaligus? | Pemilik proses | **Tertutup** `RWI-DEC-054` — tidak, dijaga unique index parsial | Tidak |
| Apakah resume pulang perlu riwayat versi? | Pemilik klinis | **Tertutup** `RWI-DEC-057` — ya, versi sebelumnya tersalin saat koreksi | Tidak |
| Apakah bayi dan ibunya perlu penanda rawat gabung? | Pemilik proses | **Tertutup** `RWI-DEC-056` — ya, kolom opsional rujukan episode ibu | Tidak |
| Siapa yang berwenang menerbitkan penerimaan deposit pada langkah admisi — petugas admisi sendiri, atau langkah itu hanya mencatat nominal sementara kwitansi diterbitkan kasir? | Pemilik keuangan | **Terbuka** sejak 2026-09-08. Layar admisi dipegang petugas admisi, sedangkan penerimaan uang selama ini milik kasir. Jawabannya menentukan siapa pemegang `BillingDeposit : Create` pada bagian 14 | Ya, untuk `EPIC RI-35a` |

### 20.3 Yang masih menahan, dan bentuknya bukan pertanyaan

| Butir | Bentuknya | Menahan apa |
| --- | --- | --- |
| Master kamar dan tempat tidur terisi **dan penandanya benar** | Pekerjaan data, bukan keputusan | `MVP-1`, termasuk `EPIC RI-34`. Penanda jenis kelamin, isolasi, dan boks bayi yang salah setel akan menolak penempatan yang sah, atau lebih buruk, meloloskan yang tidak sah |
| Test regresi jalur lama untuk modul yang disentuh | Pekerjaan uji | `EPIC RI-32` dan seluruh task yang menyentuh modul lain — `NFR-008` |
| Perbaikan pemanggilan tombol tempat tidur di frontend | Pekerjaan perbaikan | `EPIC RI-32` |
| Sinkronisasi keputusan scope deposit 2026-09-03 **dan arahan operasional 2026-09-08 (`OPS-2026-09-08/A` s.d. `/D`)** ke `00-interview-decisions.md`, beserta kenaikan kontrak API/validation/permission Billing ke `0.6.0` | Pekerjaan dokumentasi dan contract lock, **bukan pertanyaan bisnis baru** | `EPIC RI-35` sebelum development lock |
| Masa simpan data, interoperabilitas nasional, persetujuan pasien | Keputusan hukum dan klinis | Melayani pasien sungguhan, bukan pengerjaan MVP — bagian 16 |

Sesuai kontrak, dokumen ini tetap berstatus `draft` sampai ada approval manusia. Yang berubah pada
`0.3.0`: **tidak ada lagi pertanyaan memblokir yang menahan `/plan-module-delivery`.** Yang berubah
pada `0.6.0`: keadaan itu tetap berlaku untuk seluruh epic **kecuali `EPIC RI-35a`**, yang menunggu
jawaban kewenangan penerbitan kwitansi pada tabel 20.2.

## 21. Gelombang 1A — Rawat Inap Safety Corrections

**Ditambahkan 11 September 2026**, menyerap `RWI-DEC-097` s.d. `RWI-DEC-104`. Bagian ini **tidak**
menggantikan bagian 1 sampai 20; ia menambahkan satu gelombang perbaikan di atas MVP yang sudah
dirancang. Seluruh isinya **menurunkan** dari `02-backend-architecture.md` revision `0.7`,
`data/data-dictionary.md`, dan ketiga kontrak `0.8.0`.

### 21.1 Kenapa gelombang ini ada

`PRD-to-MVP-Rawat-Inap-V2` menemukan empat penyimpangan yang harus dibereskan sebelum modul ini
dipakai melayani pasien sungguhan. Dua di antaranya dimiliki sub-modul ini.

| Penyimpangan | Keadaan hari ini di source | Kenapa berbahaya |
| --- | --- | --- |
| Kelayakan tempat tidur menilai jenis kelamin penghuni kamar lain | Berjalan, kode `ROOM_GENDER_MIXED` terbit dari `InpBedOccupancyService` | Kamar berisi satu pasien laki-laki menolak seluruh pasien perempuan walau tempat tidurnya dikonfigurasi menerima keduanya. Petugas admisi tidak punya jalan keluar selain memindahkan pasien yang sudah dirawat |
| Penugasan dokter tidak mengenal peran | `InpDoctorAssignment` tidak punya kolom peran sama sekali | Konsulen dan dokter jaga tidak dapat dibedakan dari DPJP, sehingga matriks kewenangan bagian 14 tidak dapat diwujudkan |

### 21.2 Batas gelombang ini

**Titik mulai.**

1. Blueprint revision `6` dan kontrak `0.8.0` berstatus `draft`.
2. Keputusan `RWI-DEC-099` dan `RWI-DEC-101` sudah `approved`.
3. Snapshot `BE@201de753` dan `FE@7f6b9356`.

**Titik akhir.**

1. Kode `ROOM_GENDER_MIXED` tidak terbit dari jalur mana pun, dan pemetaannya hilang dari frontend.
2. `InpDoctorAssignment` menyimpan peran, dan satu episode dapat memiliki satu DPJP bersama beberapa konsulen dan dokter jaga.
3. Aturan isolasi dan `BED_GENDER_MISMATCH` terbukti masih menolak.
4. Seluruh acceptance criteria pada bagian 21.5 lulus.

**Yang sengaja di luar gelombang ini.** Kelayakan keuangan, walaupun berstatus `P0`. `RWI-DEC-102`
mengeluarkannya karena bergantung pada `BE-BKC-040` milik roadmap `billing-kasir`, yang nol
barisnya ada. Ia **tidak** turun menjadi `P1`; statusnya `P0 — external dependency`.

### 21.3 Epic dan functional requirement

| ID | Epic | Prioritas | Disposisi |
| --- | --- | --- | --- |
| `EPIC RI-36` | Kelayakan tempat tidur hanya menilai penanda tempat tidur | `P0` | `EXTEND` — memangkas aturan yang sudah ada |
| `EPIC RI-37` | Penugasan dokter mengenal peran | `P0` | `EXTEND` — satu kolom, satu enum, dua index |

| FR | Bunyi requirement | Epic | Disposisi |
| --- | --- | --- | --- |
| `FR-RI-179` | Penempatan dan perpindahan **tidak** memeriksa jenis kelamin penghuni kamar lain dalam bentuk apa pun | `RI-36` | `EXTEND` |
| `FR-RI-180` | Kode `ROOM_GENDER_MIXED` tidak terbit dari pencarian, pemesanan, penempatan, maupun perpindahan | `RI-36` | `EXTEND` |
| `FR-RI-181` | Pasien tanpa jenis kelamin tercatat dapat ditempatkan pada tempat tidur yang menerima keduanya, **tanpa memandang** ada tidaknya penghuni lain | `RI-36` | `EXTEND` |
| `FR-RI-182` | Tempat tidur yang dikonfigurasi satu jenis kelamin **tetap** menolak jenis kelamin lain | `RI-36` | `EXISTING / REUSE` |
| `FR-RI-183` | Aturan isolasi dua arah **tetap** menolak | `RI-36` | `EXISTING / REUSE` |
| `FR-RI-184` | Penempatan ke boks bayi tetap dikecualikan dari aturan jenis kelamin yang tersisa | `RI-36` | `EXISTING / REUSE` |
| `FR-RI-185` | Penugasan dokter menyimpan peran bernilai `Dpjp`, `Consultant`, atau `OnCallDoctor` | `RI-37` | `MISSING / NEW` |
| `FR-RI-186` | Satu episode memiliki **tepat satu** DPJP aktif, dan boleh memiliki beberapa konsulen serta dokter jaga aktif bersamaan | `RI-37` | `EXTEND` |
| `FR-RI-187` | Kewenangan dinilai pada waktu klinis dokumen, bukan waktu penyimpanan | `RI-37` | `MISSING / NEW` |
| `FR-RI-188` | Peran pada satu baris penugasan **tidak dapat** diubah; perubahan dilakukan dengan menutup baris lalu membuka baris baru | `RI-37` | `MISSING / NEW` |
| `FR-RI-189` | Migration mengisi seluruh baris lama menjadi `Dpjp` tanpa satu pun baris ambigu | `RI-37` | `MISSING / NEW` |
| `FR-RI-190` | Konsulen dan dokter jaga **tidak** memperoleh kewenangan perpindahan, keputusan pulang, tanda tangan resume, maupun perubahan isolasi | `RI-37` | `EXTEND` |

### 21.4 Skenario UAT

**Jalur berhasil.**

1. Petugas admisi menempatkan Ny. Sari pada tempat tidur netral di kamar yang sudah dihuni Tn. Budi. Penempatan **berhasil**, dan tidak ada peringatan apa pun tentang pencampuran.
2. Pasien yang jenis kelaminnya belum tercatat ditempatkan pada tempat tidur yang menerima keduanya, di kamar berpenghuni. Penempatan **berhasil**.
3. Kepala ruangan melibatkan dua konsulen sekaligus pada satu pasien yang sudah punya DPJP. Ketiganya tercatat aktif bersamaan.
4. Supervisor mengalihkan DPJP. Baris lama tertutup beserta alasannya, baris baru terbuka, dan tidak pernah ada saat tanpa DPJP.

**Jalur gagal.**

1. Petugas menempatkan pasien laki-laki pada tempat tidur bertanda perempuan saja. **Ditolak**, pesannya menyebut batasan tempat tidurnya.
2. Petugas menempatkan pasien tanpa kebutuhan isolasi pada tempat tidur isolasi. **Ditolak**.
3. Pasien yang membutuhkan isolasi ditempatkan pada tempat tidur biasa. **Ditolak**.
4. Konsulen meminta keputusan pulang. **Ditolak**, dan penolakan itu dinyatakan sebagai keadaan sementara yang menunggu kebijakan, bukan sebagai kesalahan petugas.
5. Supervisor mencoba menugaskan DPJP kedua tanpa menutup yang lama. **Ditolak**.

### 21.5 Definition of Done gelombang ini

| Butir | Cara menjawabnya |
| --- | --- |
| Kode `ROOM_GENDER_MIXED` nol hasil pada pencarian source backend | `grep` pada `Areas/**` mengembalikan nol baris di luar komentar sejarah |
| Pemetaan kode itu hilang dari frontend, dan ketiga berkas test disesuaikan | Pencarian pada `src/` dan `tests/` mengembalikan nol baris aktif |
| Aturan isolasi terbukti masih menolak | `RWI-AC-133a` lulus |
| Pengecualian boks bayi terbukti masih berlaku | `RWI-AC-133b` lulus |
| Kolom peran ada, terisi, dan index unik memfilter peran | `RWI-AC-084a` dan `RWI-AC-084b` lulus |
| Urutan migration terbukti mengikat | `RWI-AC-084c` lulus |
| Batas rollback terbukti nyata | `RWI-AC-084h` lulus |
| Konsulen dan dokter jaga terbukti tidak memperoleh kewenangan DPJP | `RWI-AC-084f` dan `RWI-AC-084g` lulus |
| Backend dan frontend dirilis pada gelombang yang sama | Bukti rilis menyebut kedua repository |
| Nol `[AccessPermission]` baru lahir | Daftar permission sebelum dan sesudah sama persis |

**Satu butir yang sengaja tidak ada di sini.** Definition of Done ini **tidak** memuat pernyataan
bahwa MVP Rawat Inap siap produksi. `RWI-DEC-102` menegaskan kesiapan itu tetap menunggu kelayakan
keuangan, yang berada di luar gelombang ini.

### 21.6 Pertanyaan terbuka yang menyertai gelombang ini

| ID | Pertanyaan | Memblokir? |
| --- | --- | --- |
| `OPEN-MVP-004` | Kewenangan konsulen atas keputusan pulang | **Tidak memblokir gelombang ini.** Perilaku fail-closed berlaku: ditolak |
| `RWI-OQ-053` | Pemilik `BillingManagement` belum bernama | Tidak memblokir gelombang ini; memblokir gelombang kelayakan keuangan |
| `RWI-OQ-054` | Penomoran ulang sebelas task ID | **Memblokir `plan-module-delivery`**, bukan desain ini. Dijawab `RWI-DEC-103`, eksekusinya belum dikerjakan |

---

---

## 22. Amandemen terbatas — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status `draft`.** Menurunkan dari `02-backend-architecture.md` `0.8` bagian 11, `data/data-dictionary.md` `0.5`
bagian 18, `03-frontend-architecture.md` bagian 12, dan kontrak `0.9.0`.

### 22.1 Batas

| Batas | Isinya |
| --- | --- |
| **Titik mulai** | Kontrak `0.8.0` `approved`; permintaan `INT-DOK-11` s.d. `13`, `INT-DOK-20`, `INT-KEP-15` |
| **Titik akhir** | (1) Dokter melihat hanya pasien penugasannya; (2) kepala ruangan dapat menugaskan konsulen, dokter jaga, dan penugasan singkat berbatas waktu; (3) resume memuat delapan bagian dengan usulan isian bersumber; (4) penutupan episode mengunci konsep, membatalkan pesanan tertunda yang belum ditagih dan dosis masa depan dalam satu transaksi tanpa menahan penutupan |
| **Di luar batas** | Seluruh isi bagian 1 s.d. 21 yang tidak disebut; Resume ODC; pembatalan pesanan tertagih; census rumah sakit bagi dokter |

### 22.2 Epic dan functional requirement

| Epic | Nama | Prioritas | Disposisi |
| --- | --- | --- | --- |
| `EPIC RI-38` | Census dokter dari penugasan aktif | `P0` | `EXTEND` |
| `EPIC RI-39` | Penugasan konsulen, dokter jaga, dan penugasan singkat | `P0` | `MISSING / NEW` — `RWI-FACT-030` |
| `EPIC RI-40` | Resume delapan bagian | `P1` | `EXTEND` |
| `EPIC RI-41` | Akibat penutupan episode | `P0` | `MISSING / NEW` — `RLN3-CAP-38` |

| FR | Bunyi requirement | Epic | Disposisi | Bukti |
| --- | --- | --- | --- | --- |
| `FR-RI-191` | Census dengan `assignedToMe=true` hanya memuat pasien dengan penugasan aktif dokter login, dokter dari akun login, ringkasan dari daftar yang sama | `RI-38` | `EXTEND` | API 10.1 |
| `FR-RI-192` | Akun tanpa data dokter mendapat daftar kosong berpesan, bukan penolakan | `RI-38` | `MISSING / NEW` | `VAL-INP-11` |
| `FR-RI-193` | Kepala ruangan atau supervisor membuat penugasan konsulen dan dokter jaga beralasan | `RI-39` | `MISSING / NEW` | API 10.2 |
| `FR-RI-194` | Penugasan singkat penulisan catatan terlambat selalu dokter jaga, berwaktu mulai sekarang, berwaktu selesai, beralasan; DPJP tidak tergusur | `RI-39` | `MISSING / NEW` | `VAL-INP-01`, `08`; `INV-INP-12` |
| `FR-RI-195` | Penugasan konsulen dan dokter jaga dapat diakhiri; DPJP tidak | `RI-39` | `MISSING / NEW` | `VAL-INP-09` |
| `FR-RI-196` | Resume menyimpan Pemeriksaan Penting, Kondisi Saat Pulang, Edukasi beserta versinya | `RI-40` | `EXTEND` | Data 18.2–18.3 |
| `FR-RI-197` | Usulan isian resume dari sumber klinis beserta label sumber, tanpa menyimpan, tahan terhadap sumber gagal | `RI-40` | `MISSING / NEW` | API 10.3 |
| `FR-RI-198` | Penutupan mengunci konsep catatan dokter encounter | `RI-41` | `MISSING / NEW` | `INT-INP-08` |
| `FR-RI-199` | Penutupan membatalkan pesanan tindakan tertunda yang belum ditagih; yang tertagih masuk daftar pantau | `RI-41` | `MISSING / NEW` | `INT-INP-09`, API 10.4 |
| `FR-RI-200` | Penutupan membatalkan dosis obat berjadwal setelah waktu tutup | `RI-41` | `MISSING / NEW` | `INT-INP-10` |
| `FR-RI-201` | Kesiapan penutupan menampilkan peringatan yang tidak menahan; hasil penutupan menampilkan akibatnya | `RI-41` | `EXTEND` | `VAL-INP-13` s.d. `17` |

### 22.3 Kebutuhan non-fungsional

| ID | Kebutuhan | Cara membuktikan |
| --- | --- | --- |
| `NFR-025` | Penutupan beserta akibatnya atomik pada PostgreSQL sungguhan | Galat buatan tiap langkah → nol perubahan |
| `NFR-026` | Census `assignedToMe` memakai index penugasan per dokter; waktu aktif dibaca saat query tanpa proses latar | Rencana eksekusi memakai index; uji batas 06.59/07.01 |
| `NFR-027` | Usulan isian resume selesai paling lama 5 detik per sumber — **angka usulan desain**, dikonfirmasi saat approval | Pengukuran pada laporan task |

### 22.4 Skenario UAT

| ID | Epic | Jalur | Skenario | Hasil |
| --- | --- | --- | --- | --- |
| `UAT-45` | `RI-38` | Berhasil | dr. Ahmad membuka daftarnya | Dua pasien, Budi dan Sari |
| `UAT-46` | `RI-39` | Berhasil | Kepala ruangan membuat penugasan singkat dr. Rina 10.00–11.00; dr. Rina menulis kajian medis Selasa 15.00 pukul 10.15 | Diterima; dr. Ahmad tetap DPJP |
| `UAT-47` | `RI-39` | Gagal | Kepala ruangan lupa mengisi waktu selesai | Ditolak |
| `UAT-48` | `RI-39` | Gagal | Perawat pelaksana mencoba menambah dokter pendukung | Tombol tidak ada; permintaan langsung ditolak |
| `UAT-49` | `RI-40` | Berhasil | dr. Rina menekan "Isi dari data klinis", melengkapi Kondisi Saat Pulang, menandatangani | Resume bertanda tangan; episode belum tertutup |
| `UAT-50` | `RI-41` | Berhasil | Petugas menutup episode Joko dengan satu konsep, satu pesanan, satu dosis tertunda | Penutupan berhasil; konsep terkunci, pesanan dan dosis batal; ringkasan akibat tampil |
| `UAT-51` | `RI-41` | Gagal | Gangguan database saat penutupan | "Penutupan gagal disimpan, coba lagi"; episode tetap `DischargePending` |

### 22.5 Definition of Done

| Butir | Bukti |
| --- | --- |
| Census dokter sama dengan daftar tulis | Acceptance 18.1 |
| Penugasan singkat dijaga di service **dan** database | Acceptance 18.2 termasuk check constraint |
| Resume delapan bagian dan usulan bersumber | Acceptance 18.3 |
| Penutupan atomik dan tidak tertahan | Acceptance 18.4; `NFR-025` |
| Regresi census unit, pengalihan DPJP, dan penutupan tanpa konsep/pesanan/dosis | Test regresi hijau |
| Pemberitahuan kepada Yoga Aji Pratama atas `INT-INP-08` tercatat | Catatan pada laporan task |

### 22.6 Urutan pengiriman

| Gelombang | Isinya | Syarat mulai |
| --- | --- | --- |
| `RI-V2-1` | `EPIC RI-38`, `EPIC RI-39` (migration E1), `FR-RI-198`, `199`, `201` | Blueprint revision `7` disetujui; dikerjakan bersama `DOK-V2-1` |
| `RI-V2-2` | `EPIC RI-40` (migration E2) | `RI-V2-1`; sebelum `DOK-V2-4` |
| `RI-V2-3` | `FR-RI-200` | Bersama `KEP-V2-2` |

Nol epic `OPEN DECISION`.

### 22.7 Pertanyaan terbuka

| No | Pertanyaan | Siapa yang menjawab | Memblokir |
| ---: | --- | --- | :---: |
| 1 | Nasib pesanan tindakan tertunda yang sudah ditagih setelah penutupan | Muhammad Hamzah bersama pemilik Billing | Tidak — daftar pantau tersedia |
| 2 | Dosis lewat jadwal yang belum dicatat saat penutupan tetap `Due` hanya-baca — sama dengan `keperawatan` 22.20 nomor 9 | Muhammad Hamzah | Tidak |
| 3 | Isi minimal resume, termasuk apakah tiga isian baru wajib sebelum tanda tangan | Pemilik klinis, belum ditunjuk | Tidak untuk desain; gerbang produksi |
| 4 | ~~Persetujuan Yoga Aji Pratama atas pemanggil penguncian dari `InPatientManagement`~~ — **`closed` 2026-09-16** oleh `RWI-DEC-151` | Yoga Aji Pratama | Tidak — pemberitahuan sebelum rilis `RI-V2-1` **sudah terpenuhi**; DoD `BE-RWI-082` cukup merujuk keputusan itu |

---

## 23. Finishing Rawat Inap — revision `0.9` / kontrak `0.10.0` ★ 1 Oktober 2026

Menurunkan dari `02-backend-architecture.md` 12, `03-frontend-architecture.md` 13, `contracts/` bagian `0.10.0`, `data/data-dictionary.md` 19, dan `flowcharts/`.

### 23.1 Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian — Rawat Inap, sub-modul `episode-rawat-inap` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Baseline | Backend `c8e99ce5` (HEAD `425cfeae`); frontend `22ad67330` |
| Masukan | `PRD-RWI-FINISHING-001` v`0.4`; decision log revision `30`; gate `1.9` (`INP-S27`, `S31`, `S33`, `S36`, `S37` `READY_FOR_DOMAIN_DESIGN`; `INP-S32` `PARTIALLY_READY`) |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — batas domain diambil dari keputusan `RWI-DEC-173` s.d. `205` dan source yang dibaca |
| Cakupan | Pasien operasi dari bangsal sampai kembali ke bed, admisi dari kamar pulih, penolakan order, ringkasan operasi, serah terima transfer (`P2`), laporan transfer (`P2`) |

### 23.2 Ringkasan eksekutif

Bangsal dapat memesan ruang bedah dari order tindakan, mengirim catatan pra-operasi yang dikonfirmasi OK, menerima pasien kembali dengan serah terima yang sah, dan melihat ringkasan operasinya. Biaya operasi masuk invoice satu kali saat kasus selesai. Pasien operasi elektif dari poliklinik yang perlu dirawat muncul di daftar admisi, tanpa admisi otomatis.

### 23.3 Masalah produk

`FIN-CAP-21` s.d. `26`, `33`: menu Pemesanan Ruangan Bedah masih *placeholder*; bangsal tidak melihat status operasi; serah terima OK dapat diterima siapa saja dengan permission yang sama dengan pengirimnya; kiriman biaya OK ke Billing ditahan; keputusan kamar pulih "rawat inap" tidak menghasilkan apa pun; laporan operasi tidak terbaca di bangsal.

### 23.4 Visi produk

1. Order tindakan dokter → pesanan ruang bedah dari bangsal → kasus OK.
2. Pra-operasi dua sisi → kasus "Siap" → operasi → kamar pulih.
3. Serah terima diterima di bed unit tujuan → kasus selesai → biaya operasi masuk invoice.

### 23.5 Batas MVP

**Titik mulai.** (1) Episode `Admitted` dengan order tindakan operasi aktif; atau (2) pasien non-rawat inap yang kasus OK-nya sampai di kamar pulih. **Titik akhir.** (1) Kasus OK `Completed` atau `Rejected` dan terbaca di bangsal. (2) Biaya operasi tercatat di invoice. (3) Permintaan admisi `Completed` atau `Cancelled`.

### 23.6 Pelaku sasaran

| Pelaku | Tanggung jawab |
|---|---|
| Perawat bangsal | Memesan ruang bedah; mengirim pra-operasi; menerima serah terima |
| Dokter bangsal | Memesan order tindakan dan ruang bedah; membaca ringkasan operasi |
| Petugas penjadwalan OK | Menjadwalkan atau menolak order |
| Perawat OK | Mengonfirmasi pra-operasi; mengirim serah terima |
| Petugas admisi | Menyelesaikan admisi dari permintaan |
| Kepala ruangan, manajemen | Laporan transfer (`P2`) |

### 23.7 Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Pemesanan ruang bedah dan Obgyn | `CAP-RWF-07`, `FIN-CAP-21` | Wajib (`P1`) |
| Pasien operasi terhubung ke bangsal | `CAP-RWF-08`, `FIN-CAP-22` s.d. `24` | Wajib (`P1`) |
| Admisi dari kamar pulih | `CAP-RWF-18`, temuan 37 | Wajib (`P1`, `RWI-DEC-217`) |
| Ringkasan operasi di bangsal | `CAP-RWF-19`, temuan 36 | Wajib (`P1`, `RWI-DEC-217`) |
| Penolakan order operasi | `CAP-RWF-22`, temuan 40 | Wajib (`P1`, `RWI-DEC-217`) |

### 23.8 Kemampuan yang ditunda

| Kemampuan | Alasan | Pengganti selama MVP |
|---|---|---|
| Serah terima klinis transfer (`CAP-RWF-16`) | `P2` (`RWF-W5`) | Tetap "Integrasi belum tersedia"; transfer tetap berjalan |
| Laporan transfer ruangan (`CAP-RWF-23`) | `P2`; butir menu Laporan Rawat Inap (`RWI-DEC-214`, `215`) | Riwayat penempatan per episode |
| ~~Penggabungan biaya operasi kunjungan asal~~ | **Tidak lagi ditunda** — `RWI-DEC-207`: tautan non-destruktif oleh Billing (`integrasi-billing` 9.14), masuk `MVP-2` | — |
| Jasa medis, asisten, diskon operasi | Milik modul jasa medis dan Billing (`RWI-DEC-197`) | — |

### 23.9 Alur bisnis target

`flowcharts/00-alur-utama.md`; rincian `01` s.d. `04`.

### 23.10 Epic dan functional requirement

| Epic | FR | Disposisi backend |
|---|---|---|
| `EPIC-RWF-05` Pemesanan ruang bedah dan pasien operasi | `FR-RWF-040` s.d. `049`, `090` | `EXTEND` (`OprCase`, serah terima, kesiapan, kiriman OK), `MISSING / NEW` (adapter pemesanan, pra-operasi, efek kasus selesai, master butir) |
| `EPIC-RWF-09` Pasca operasi — bagian sub-modul ini | `FR-RWF-080` s.d. `082`, `086` s.d. `089` | `MISSING / NEW` (permintaan admisi, bacaan ringkasan, laporan), `EXTEND` (status `Rejected`); aturan Billing butir (b): `EXTEND` di `integrasi-billing` (`RWI-DEC-207`) |
| `EPIC-RWF-08` bagian transfer | `FR-RWF-071` | `MISSING / NEW` (`CliTransferHandover`) |

`FR-RWF-083` s.d. `085`, `091` s.d. `093` (surveilans, transfusi) dirancang `keperawatan` kontrak `0.6.0`.

### 23.11 Model status yang diusulkan

`contracts/state-transition-matrix.md` bagian 9. Invariant `INV-RWF-25` s.d. `34`.

### 23.12 Sasaran arsitektur

Dipakai ulang: `OprCase`, `OprCaseProcedure`, `OprHandover`, kesiapan OK, outbox `OprIntegrationDelivery`, jalur order tindakan, admisi berlangkah, linimasa penempatan. Diperluas: `OprCase`, `MstInpatientSetting`, `MstTariff`. Baru: `InpSurgeryBookingAdapter`, `OprWardPreOp*`, `MstSurgicalPreparationItem`, `OperatingRoomCompletionEffects`, `PatientProcedureExecutionService` (ekstraksi), `InpAdmissionReferral`, `CliTransferHandover`, `InpRoomTransferReportService`.

### 23.13 Sasaran kemampuan API

| Tag | Endpoint | Hak akses | Epic | Status |
|---|---|---|---|---|
| `Health Services / Inpatient Management / Inpatient Surgery Booking` | `POST …/episodes/{id}/surgery-bookings` | `OperatingRoomCase : Create` | `EPIC-RWF-05` | **Rencana (belum tersedia)** |
| `Health Services / Operating Room Management / Preparation` | `…/preparation/ward-pre-op` (5 operasi) | `OperatingRoomWardPreOp : Read/Send/Confirm` | `EPIC-RWF-05` | **Rencana (belum tersedia)** |
| `Health Services / Master Data / Surgical Preparation Item` | `…/surgical-preparation-items` | `SurgicalPreparationItem : Read/Create/Update` | `EPIC-RWF-05` | **Rencana (belum tersedia)** |
| `Health Services / Operating Room Management / Cases` | `PATCH /{id}/reject`, `GET /{id}/post-operative-summary`; isian baru | `OperatingRoomCase : Reject`, `: Read` | `EPIC-RWF-05`, `09` | **Rencana**; isian pada endpoint ✅ yang ada |
| `Health Services / Operating Room Management / Execution` | `POST handovers`, `PATCH handovers/{id}/accept`, `PUT recovery` | `OperatingRoomHandover : Send/Receive`; `OperatingRoomAnesthesia : Update` | `EPIC-RWF-05`, `09` | ✅ Tersedia; permission dan perilaku **Rencana** |
| `Health Services / Operating Room Management / Handovers` | `GET /handovers` | `OperatingRoomHandover : Read` | `EPIC-RWF-09` | **Rencana (belum tersedia)** |
| `Health Services / Inpatient Management / Inpatient Admission Referral` | `GET /admission-referrals`, `/{id}` | `InpatientAdmissionReferral : Read` | `EPIC-RWF-09` | **Rencana (belum tersedia)** |
| `Health Services / Inpatient Management / Inpatient Report` | `GET /reports/room-transfers`, `/export` | `InpatientReport : ReadRoomTransfer/ExportRoomTransfer` | `EPIC-RWF-09` | **Rencana (belum tersedia)**, `P2` |
| `Health Services / Clinical Management / Transfer Handover` | `…/transfer-handovers` | `TransferHandover : Read/Send/Receive` | `EPIC-RWF-08` | **Rencana (belum tersedia)**, `P2` |

### 23.14 Matriks kewenangan

`contracts/permission-audit-matrix.md` bagian 9.

### 23.15 Batas integrasi dan billing

Tindakan operasi ditagih sekali lewat order tindakan; OK menagih anestesi, sewa kamar operasi, dan bahan (`RWI-DEC-196`). Kasus yang dibatalkan atau ditolak tidak menimbulkan biaya. Biaya operasi tidak lewat ketukan pintu Rawat Inap. Biaya operasi pasien dari poliklinik atau ODC tetap pada invoice kunjungan asal, ditautkan Billing ke invoice `RANAP`, dan dibayar bersama saat pulang (`RWI-DEC-207`, pola `BKC-DEC-118`).

### 23.16 Guardrail regulasi dan keselamatan

Penandaan area operasi dan konfirmasi dua akun mengikuti praktik keselamatan pasien bedah (`RWI-DEC-173`, `174`). Isi butir persiapan disahkan pemilik klinis sebelum produksi.

### 23.17 Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-RWF-12` | Pemeriksaan lokasi bed untuk penerimaan serah terima gagal tertutup |
| `NFR-RWF-13` | Efek kasus selesai idempoten: dijalankan berulang tidak menggandakan baris tagihan |
| `NFR-RWF-14` | Laporan transfer 31 hari memuat ≤ 2 detik untuk 500 transfer; ekspor tidak disimpan di server |
| `NFR-RWF-15` | Daftar Pesanan Ruang Bedah tidak memerlukan dorongan waktu nyata; status segar saat menu dibuka atau dimuat ulang |

### 23.18 Skenario UAT

| ID | Jalur | Langkah | Hasil |
|---|---|---|---|
| `UAT-RWF-05` | Berhasil | Pesan Bedah Obgyn → dijadwalkan → pra-operasi lengkap dua sisi → operasi → serah terima diterima | Status terlihat di setiap langkah; jenis Obstetri; biaya muncul setelah `Completed` |
| `UAT-RWF-13` | Gagal | Serah terima ke ICU saat pasien di bangsal; perawat OK mencoba menerima sendiri | Ditolak; tombol Terima ICU terkunci sampai transfer |
| `UAT-RWF-16` | Berhasil | Pasien poliklinik selesai operasi; kamar pulih memutuskan rawat inap | Muncul di daftar permintaan; setelah admisi dan bed, serah terima diterima dan kasus `Completed` |
| `UAT-RWF-17` | Berhasil | Perawat membuka ringkasan operasi | Baca-saja |
| `UAT-RWF-21` | Berhasil | Kasus ditunda lalu dijadwalkan ulang dengan TD baru | Versi lama "perlu diperbarui"; versi baru memuat TD terbaru; "Siap" setelah konfirmasi akun lain |
| `UAT-RWF-24` | Gagal lalu berhasil | Tolak order tanpa alasan, dengan alasan; bangsal memesan ulang | Ditolak; Ditolak tampil di bangsal; kasus baru; tanpa biaya |
| `UAT-RWF-32` | Gagal | Pesan ruang bedah tanpa order tindakan | Ditolak "Tindakan operasi belum dipesan dokter" |
| `UAT-RWF-33` | Gagal | Admisi biasa untuk pasien yang punya permintaan dari kamar pulih | Ditolak dan diarahkan ke permintaan |
| `UAT-RWF-14` | Berhasil (`P2`) | Pasien dipindahkan bangsal → ICU | Dokumen lahir; "Serah terima tertunda" sampai diterima |
| `UAT-RWF-20` | Berhasil dan gagal (`P2`) | Laporan transfer satu minggu; pengguna tanpa permission | Baris lengkap, koreksi bertanda; 403 |

### 23.19 Definition of Done

| Butir | Bukti |
|---|---|
| Menu Pemesanan Ruangan Bedah tidak lagi *placeholder* dan setiap pesanan merujuk satu order | `UAT-RWF-05`, `32`; `AC-RWF-040`, `048` |
| Kasus tidak dapat "Siap" tanpa pra-operasi versi terbaru terkonfirmasi dua akun | `AC-RWF-042`, `046`, `093`, `094`; `UAT-RWF-21` |
| Serah terima hanya diterima penerima sah di bed unit tujuan | `AC-RWF-043`, `047`; `UAT-RWF-13` |
| Biaya operasi tepat sekali per komponen, nol untuk kasus batal atau ditolak | `AC-RWF-044`, `092`, `099`; test idempotensi `INV-RWF-29` |
| Permintaan admisi tanpa admisi otomatis | `AC-RWF-080`, `088`, `089`; `UAT-RWF-16`, `33` — persetujuan OK sudah ada (`RWI-DEC-208`) |
| Status Ditolak di OK dan bangsal | `AC-RWF-085`, `099`; `UAT-RWF-24` — persetujuan OK sudah ada (`RWI-DEC-208`) |
| Ringkasan operasi baca-saja | `AC-RWF-081`, `082`; `UAT-RWF-17` |
| Alur OK lama tidak berubah | Regresi bagian 20 testing |

### 23.20 Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-1` (`RWF-W3`) | `E4`, `E5`; adapter pemesanan; pra-operasi; permission serah terima; aturan penerimaan; efek kasus selesai dan tujuan Billing; `FE-INP-25` s.d. `28`, `33`, `34` | Kontrak `0.10.0` disetujui; `integrasi-billing` `RWF-W1` (komponen Billing `OPERATING_ROOM`) |
| `MVP-2` (`RWF-W7`) | Ringkasan operasi (boleh lebih dulu); status `Rejected`; `E6` permintaan admisi; daftar pantau; `FE-INP-29`, `30` | `RWF-W3` (persetujuan OK `RWI-DEC-208` sudah ada) |
| `POST-MVP` (`RWF-W5`, `RWF-W7` `P2`) | `E7` serah terima transfer `FE-INP-31`; laporan transfer `FE-INP-32` | `RWF-W1` (koreksi penempatan) |
| `MVP-2` (`RWF-W7`) | Tautan kunjungan asal ke invoice `RANAP` dan baris operasinya di Tagihan Pasien (`integrasi-billing` `I6`, `RWI-DEC-207`) | `E6` |

| Pertanyaan | Siapa | Dampak | Memblokir |
|---|---|---|:---:|
| ~~`DEC-INP-018` invoice tujuan biaya operasi dari poliklinik/ODC~~ | Yasmina | Aturan Billing penggabungan; sisi Rawat Inap dan OK tidak berubah | Tidak — **ditutup 2 Oktober 2026** |
| ~~`RWI-OQ-114` (a) OK mengirim dan membatalkan permintaan admisi~~ | Ikbal Yulianto | Implementasi `INT-RWF-23`, `24` | Tidak — **ditutup 2 Oktober 2026** |
| ~~`RWI-OQ-114` (c) status `Rejected` di OK~~ | Ikbal Yulianto | Implementasi `PATCH reject` | Tidak — **ditutup 2 Oktober 2026** |
| ~~Prioritas `CAP-RWF-18`, `19`, `22` (PRD 11.2 butir 13)~~ | Muhammad Hamzah | Urutan gelombang | Tidak — **ditutup 2 Oktober 2026** |
| ~~`UI-RWF-03` letak ringkasan di ruang kerja dokter; `UI-RWF-04` letak laporan transfer; `UI-RWF-05` urutan daftar pantau~~ | Muhammad Hamzah | Task frontend layar terkait | Tidak — **ditutup 2 Oktober 2026** |
| Nilai `OprPlannedAnesthesiaType` dan isi awal butir persiapan | Pemilik OK; pemilik klinis | Data master | Tidak — gerbang produksi |

**Penyelarasan decision log revision `31` ★ 2 Oktober 2026.** Seluruh pertanyaan 23.20 tertutup: `DEC-INP-018` (`RWI-DEC-207`), `RWI-OQ-114` (a) dan (c) (`RWI-DEC-208`), prioritas (`RWI-DEC-217`), `UI-RWF-03` s.d. `05` (`RWI-DEC-213` s.d. `216`). Tidak ada epic `OPEN DECISION`. Yang tersisa hanya gerbang produksi: isi awal butir persiapan bedah dan nilai jenis anestesi rencana.

| ID | Jalur | Langkah | Hasil |
|---|---|---|---|
| `UAT-RWF-41` | Gagal | Petugas admisi membuka admisi biasa untuk pasien yang punya permintaan dari kamar pulih | Ditolak dan diarahkan ke permintaan; admisi dari permintaan berhasil (`RWI-AC-331`) |
| `UAT-RWF-42` | Gagal | Perawat memesan ruang bedah untuk episode `DischargePending` | Ditolak (`RWI-AC-332`) |
| `UAT-RWF-43` | Berhasil | Perawat memesan ruang bedah dari order tindakan berharga | Perkiraan tarif tindakan dan keterangan komponen OK tampil (`RWI-AC-338`) |
| `UAT-RWF-44` | Berhasil dan gagal | Pengguna dengan dan tanpa permission laporan membuka sidebar Rawat Inap | Butir Laporan Rawat Inap hanya tampil bagi pemegang permission; paling banyak sepuluh butir (`RWI-AC-340`) |

---

## 24. Workspace PPRI — revision `0.10` / kontrak `0.11.0` ★ 7 Oktober 2026

Menurunkan dari `02-backend-architecture.md` 13, `03-frontend-architecture.md` 14, `contracts/` bagian `0.11.0`, `data/data-dictionary.md` 20, dan `flowcharts/` `05` s.d. `08`. Nomor epic, FR, NFR, dan UAT memakai nomor `PRD-RWI-ADMISI-001` v`0.2` dan **tidak didaur ulang**; nomor baru mulai `EPIC-RWA-14`, `FR-RWA-140`, `NFR-RWA-15`, `UAT-RWA-28`. Bila PRD hulu dan bagian ini berbeda, bagian ini dan decision log yang berlaku (G-50).

### 24.1 Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | QuilvianFinal — Rawat Inap, sub-modul `episode-rawat-inap`, **Workspace PPRI** (Ruang Kerja Penerimaan Pasien Rawat Inap) |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`) |
| Repository dan baseline | Backend `NewQuilvianSystemBackend` `MHamzah` @ `fdf85a07`; frontend `QuilvianSystemFrontendDev` `HamzahV2` @ `27889662a` |
| Masukan | `PRD-RWI-ADMISI-001` v`0.2` (SHA-256 `f1fd336f…dc1192`); decision log revision `38` (`RWI-DEC-225` s.d. `264`); gate `1.11` bagian 20; capability map `1.7` bagian 20 |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — gate `1.11` 20.11 |
| Ringkasan cakupan | Satu ruang kerja per episode untuk menyelesaikan dokumen penerimaan pasien dengan isian V1: serah terima, gelang dan label, privasi, IPD, pelunasan deposit, selisih biaya, nilai kepercayaan, dan General Consent cetak saja — tersimpan, berversi, bertanda tangan kertas dan atestasi petugas, tercetak dari profil rumah sakit |

### 24.2 Ringkasan eksekutif

Petugas admisi Final belum punya tempat untuk dokumen penerimaan pasien: gelang tidak bisa dicetak, persetujuan umum hanya dicetak kosong, dan delapan dokumen V1 lain tidak ada. MVP ini memberi **Workspace PPRI** di Detail Episode, tepat sesudah Workspace Dokter, dengan tata letak Workspace Keperawatan. Hasil yang dikejar: (1) seluruh dokumen wajib selesai di satu tempat tanpa mengetik ulang data pasien; (2) setiap dokumen tersimpan, berversi, dan tercetak sama dengan yang ditandatangani; (3) tidak ada data karangan; (4) petugas, perawat, dan dokter melihat dokumen mana yang belum lengkap tanpa menahan perawatan.

### 24.3 Masalah produk

Bukti `PPRI-CAP-01` s.d. `54`: tidak ada tombol, halaman, tabel, maupun endpoint dokumen admisi (`PPRI-CAP-06`, `14`, `19`, `21`, `31`, `35`); master butir hanya untuk penutupan dan dibaca seluruhnya oleh penutupan episode (`PPRI-CAP-13`); cetak persetujuan menanam identitas rumah sakit client (`PPRI-CAP-09`); pengaturan tidak punya kode formulir dan kota (`PPRI-CAP-50`); data header tersebar di delapan hak baca modul lain (`PPRI-CAP-53`). Yang sudah ada dan dipakai: template ruang kerja (`PPRI-CAP-03`), isi QR No. RM (`PPRI-CAP-17`), ringkasan deposit Billing (`PPRI-CAP-29`), pola entity, konkurensi, dan idempotensi (`PPRI-CAP-43` s.d. `45`).

### 24.4 Visi produk

1. Admisi dikonfirmasi → episode `Admitted`.
2. Detail Episode menampilkan "Workspace PPRI" bagi pemegang hak baca dokumen admisi.
3. Server merangkai identitas, penjamin, kamar, DPJP, kontak darurat, alergi, dan deposit dari service modul pemiliknya.
4. Server menghitung dokumen wajib dan kelengkapannya.
5. Petugas mengisi formulir V1 yang sudah terisi otomatis, mengunci, mencetak lembar untuk ditandatangani.
6. Pasien atau keluarga menandatangani kertas; petugas mencatatnya; petugas lain menandatangani kolomnya lewat akun sendiri.
7. Dokumen lengkap; isi beku sejak dikunci; koreksi lewat versi baru.
8. Serah Terima ditutup tiga petugas; kelengkapan penuh; peringatan di Detail Episode hilang.

### 24.5 Batas MVP

**Titik mulai.** (1) Episode `Admitted` atau `DischargePending`. (2) Pengguna memegang `InpatientAdmissionDocument : Read`. (3) Pengguna menekan "Workspace PPRI".

**Titik akhir.** (1) Dokumen wajib menurut `RWI-DEC-234` berstatus `Completed` atau sudah dicetak, misalnya "6 dari 6". (2) Gelang dicetak minimal sekali dan tercatat. (3) Serah Terima ditandatangani petugas admisi, CRO, dan perawat penerima. (4) Setiap dokumen dapat dicetak ulang beralasan dengan isi yang sama. (5) Saat episode `Closed` atau `Cancelled`, semua dokumen hanya-baca.

### 24.6 Pelaku sasaran

| Pelaku | Tanggung jawab di MVP | Hak (`contracts/permission-audit-matrix.md` 10.1) |
|---|---|---|
| Petugas admisi / PPRI | Membuka ruang kerja, mengisi, mengunci, mencatat tanda tangan kertas, menandatangani kolom Admission/Petugas PPRI, mencetak | `Read`, `ViewAmount`, `Create`, `Update`, `Sign`, `Print` |
| CRO | Menandatangani Serah Terima | `Read`, `SignAsCro`, `Print` |
| Perawat ruangan | Menandatangani Serah Terima sebagai penerima; mencetak gelang | `Read`, `SignAsNurse`, `Print` |
| Kepala ruangan atau wakil yang ditunjuk | Menandatangani kolom Kepala Ruangan pada Privasi | `Read`, `SignAsNurse`, `SignAsHeadNurse`, `Print` |
| Kasir | Membaca dan mencetak Pelunasan Deposit | `Read`, `ViewAmount`, `Print` |
| Supervisor admisi | Membatalkan dokumen beralasan | + `Cancel` |
| Pasien / keluarga | Menandatangani kertas | Tanpa akun |
| Admin | Mengisi butir serah terima dan pengaturan cetak | `InpatientClearanceItem : Create/Update`, `InpatientSetting : Update` |

### 24.7 Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Ruang kerja, header, kelengkapan, tombol, peringatan Detail Episode | `CAP-RWA-01` (`PPRI-CAP-01` s.d. `08`) | Wajib; tanpa ini dokumen lain tidak punya tempat |
| Fondasi: siklus, versi, tanda tangan kertas, atestasi, log cetak, kop dan kode formulir | `CAP-RWA-14` (kertas), `15`, `16`, `17` (`PPRI-CAP-42` s.d. `51`) | Wajib; tanpa ini kelemahan V1 nomor 4, 5, 6, 10, 11 terulang |
| General Consent cetak saja dan pengalihan Cetak Persetujuan | `CAP-RWA-02` bagian cetak (`PPRI-CAP-09` s.d. `11`) | Wajib; jalan aman selama *fail-closed* (`RWI-DEC-230`, `233`, `246`) |
| Serah Terima Pasien Baru | `CAP-RWA-03` (`PPRI-CAP-13` s.d. `15`) | Wajib; satu-satunya bukti pasien diserahkan ke ruangan |
| Gelang dan label | `CAP-RWA-05` (`PPRI-CAP-16` s.d. `20`) | Wajib; sarana identifikasi pasien |
| Permintaan Privasi | `CAP-RWA-06` (`PPRI-CAP-21`, `23`) | Wajib; hak pasien yang disepakati client |
| IPD | `CAP-RWA-07` (`PPRI-CAP-24` s.d. `28`) | Wajib; butir 9 Serah Terima |
| Pelunasan Deposit | `CAP-RWA-08` (`PPRI-CAP-29` s.d. `33`) | Wajib; diputuskan `RWI-DEC-231` |
| Selisih Biaya | `CAP-RWA-11` (`PPRI-CAP-34`, `35`) | Wajib untuk penjamin asuransi/perusahaan |
| Nilai Kepercayaan | `CAP-RWA-13` (`PPRI-CAP-22`, `23`) | Wajib; hak pasien dan keselamatan |

### 24.8 Kemampuan yang ditunda atau menunggu keputusan

| Kemampuan | ID | Alasan bersebab | Pengganti selama MVP |
|---|---|---|---|
| Estimasi Biaya Rekap | `CAP-RWA-09` | Tarif visit dokter dan catatan cito, lebih dari 4 jam, *standby*, anestesi belum punya sumber (`DEC-INP-020`). Estimasi tanpa baris itu akan menjelaskan biaya lebih rendah kepada pasien | Penjelasan lisan petugas dengan lembar kertas; menu tidak ditampilkan; tidak dihitung kelengkapan (`RWI-DEC-234`) |
| Penyimpanan General Consent | `CAP-RWA-02`, slice `INP-S10` | Keabsahan dan penyimpanan menunggu pemilik privasi (`DEC-INP-003`) | Dua lembar cetak tanpa simpan, ditandatangani basah (`RWI-DEC-233`) |
| Tanda tangan digital di tablet | `CAP-RWA-14` bagian digital | Sama (`DEC-INP-003`, `RWI-DEC-235` `draft`) | Catatan tanda tangan kertas (`RWI-DEC-230`) |
| Estimasi Biaya Rinci | `CAP-RWA-10` | Isinya tagihan berjalan Billing; hak lihat rupiah per butir belum diputuskan (`RWI-OQ-119`) | Kasir mencetak dari Billing |
| MP Benefit | `CAP-RWA-12` | Tidak ada sumber nilai benefit (`RWI-OQ-120`) | Menu tidak ditampilkan; dicatat di butir 15 Serah Terima |
| Peringatan nilai kepercayaan dan privasi di Workspace Keperawatan dan Dokter | — | Mengubah dua sub-modul `approved`; amandemen terpisah | Terbaca di Workspace PPRI, IPD, dan `GET …/patient-rights`; gerbang produksi G-42 |
| Daftar tindak lanjut jatuh tempo di Billing | — | Perubahan modul Billing (G-46) | Peringatan di Workspace PPRI dan Detail Episode |
| Perluasan master pasien | — | Milik `PatientManagement` | Garis kosong pada IPD (`RWI-DEC-244`) |

`CAP-RWA-04` **dibatalkan** (`RWI-DEC-226`), bukan ditunda.

### 24.9 Alur bisnis target

`FLOW-RWA-MVP-001` — `flowcharts/00-alur-utama.md` bagian 3; jalur gagal `05` s.d. `08`.

### 24.10 Epic dan functional requirement

| Epic | FR | Disposisi backend | Status MVP |
|---|---|---|---|
| `EPIC-RWA-01` Ruang kerja dan kelengkapan | `FR-RWA-001` s.d. `008` | `MISSING / NEW` (summary, evaluator, peringatan); `EXISTING / REUSE` (template frontend, episode) | `MUST HAVE` |
| `EPIC-RWA-03` Serah Terima Pasien Baru | `FR-RWA-030` s.d. `035` | `MISSING / NEW` (dokumen, butir, slot); `EXTEND` (`MstInpatientClearanceItem`, penutupan) | `MUST HAVE` |
| `EPIC-RWA-05` Gelang dan label | `FR-RWA-050` s.d. `053` | `MISSING / NEW` (data cetak, log); `EXTEND` (`MstInpatientSetting`); `EXISTING / REUSE` (QR No. RM) | `MUST HAVE` |
| `EPIC-RWA-06` Permintaan Privasi | `FR-RWA-060` s.d. `062` | `MISSING / NEW` | `MUST HAVE` |
| `EPIC-RWA-07` IPD | `FR-RWA-070` s.d. `072` | `MISSING / NEW` (bacaan terangkai); `EXTEND` (konteks penjamin Clinical, surat pengantar) | `MUST HAVE` |
| `EPIC-RWA-08` Pelunasan Deposit | `FR-RWA-080` s.d. `085` | `MISSING / NEW` (surat); `EXISTING / REUSE` (ringkasan deposit Billing) | `MUST HAVE` — keputusan sudah turun (`RWI-DEC-231`) |
| `EPIC-RWA-10` Selisih Biaya | `FR-RWA-100` s.d. `103` | `MISSING / NEW` | `MUST HAVE` |
| `EPIC-RWA-11` Nilai Kepercayaan | `FR-RWA-110` s.d. `113` | `MISSING / NEW` | `MUST HAVE` |
| `EPIC-RWA-12` Fondasi dokumen | `FR-RWA-120` s.d. `128` | `MISSING / NEW` (siklus, slot, log, salinan beku); `EXTEND` (pengaturan); `MISSING / NEW` di modul pemilik (service baca, `RWI-DEC-264`) | `MUST HAVE` |
| **`EPIC-RWA-14`** General Consent cetak saja dan Cetak Persetujuan | `FR-RWA-020` s.d. `022` bagian cetak; `FR-RWA-140` s.d. `142` | `MISSING / NEW` (data cetak terangkai); frontend *Repair* kop | `MUST HAVE` |
| `EPIC-RWA-02` Penyimpanan General Consent | `FR-RWA-023` s.d. `025` | `OPEN DECISION` (`DEC-INP-003`) | Di luar gelombang |
| `EPIC-RWA-09` Estimasi Biaya Rekap | `FR-RWA-090` s.d. `093` | `OPEN DECISION` (`DEC-INP-020`); data model sudah dirancang (`E11`) | Di luar gelombang |
| `EPIC-RWA-13` Tanda tangan digital | `FR-RWA-130`, `131` | `OPEN DECISION` (`DEC-INP-003`) | Di luar gelombang |
| `EPIC-RWA-04` | `FR-RWA-040`, `041` | Dibatalkan (`RWI-DEC-226`) | — |

**FR baru `EPIC-RWA-14`:**

> **FR-RWA-140 — Dua cetakan tanpa simpan.** Menu General Consent punya tab Surat Persetujuan 12 butir dan tab Formulir General Consent V1, keduanya terisi dari server dan hanya dicetak. **Contoh:** Sari memilih hubungan "Istri"; nama Rina Santoso dan alamatnya terisi dari relasi pasien; tipe kamar "Umum"; Sari mencetak kedua lembar, dan tidak satu pun permintaan tulis terkirim (`RWI-AC-347`, `348`).

> **FR-RWA-141 — Cetak Persetujuan mengarah ke Workspace PPRI.** Sejak `RWA-MVP-1`, tombol Cetak Persetujuan di Detail Episode dan tautan lama `consent-print` membuka tab Surat Persetujuan, dijaga `InpatientAdmissionDocument : Read`. **Contoh:** pengguna Gizi yang hanya memegang `InpatientEpisode : Read` tidak melihat tombol itu (`RWI-AC-349`).

> **FR-RWA-142 — Kop dari profil rumah sakit.** Surat 12 butir di Workspace PPRI dan pada langkah 8 alur admisi memakai kop dari profil rumah sakit, bukan nilai yang ditanam. **Contoh:** admin mengganti nomor telepon rumah sakit di profil situs; cetakan berikutnya memakai nomor baru tanpa perubahan program.

**Penyesuaian FR PRD oleh keputusan:** `FR-RWA-003` hak rupiah memakai `ViewAmount`, bukan `ReadAmount` (`RWI-DEC-258`); `FR-RWA-004` label menu dan tombol "Workspace PPRI" (`RWI-DEC-245`), delapan menu tampil sampai Estimasi dikirim; `FR-RWA-005` tanpa General Consent (`RWI-DEC-234`); `FR-RWA-052` QR = No. RM saja (`RWI-DEC-259`); `FR-RWA-084`, `124` dibekukan saat **dikunci** (`RWI-DEC-263`); `FR-RWA-122` cetakan `AwaitingSignature` tanpa tanda konsep (`RWI-AC-384`); `FR-RWA-126` kop dari service HR (`RWI-DEC-264`); keutuhan Rekam Medis tidak dipakai (`RWI-DEC-229`).

### 24.11 Model status yang diusulkan

`contracts/state-transition-matrix.md` bagian 10: `Draft` → `AwaitingSignature` → `Completed`, ditambah `Superseded` dan `Cancelled`. Invariant `INV-RWA-01` s.d. `15` (`02-backend-architecture.md` 13.3).

### 24.12 Sasaran arsitektur

**Dipakai ulang:** template ruang kerja klinis, `InpPatientLocationQuery`, `BillingDepositService`, `InsuranceCoverageService`, `OperatingRoomCaseService`, komponen surat dan cetak, pola `RowVersion`, `Idempotency-Key`, `SnapshotJson`. **Diperluas:** `MstInpatientClearanceItem`, `MstInpatientSetting`, `EncounterInsuranceService`, `DoctorCertificateService`, `InpEpisodeService` (peringatan), `InpDischargeService.Closure` (saringan jenis). **Baru:** sepuluh tabel `InpAdmission*` gelombang MVP (+ tiga tabel Estimasi di luar gelombang), `InpatientAdmissionDocumentController`, sebelas service Rawat Inap, dan service baca di modul pemilik (`PatientProfileQueryService`, `PatientAllergyQueryService`, `HospitalSiteProfileQueryService`, `EncounterReferralQueryService`, method tarif kamar Billing).

### 24.13 Sasaran kemampuan API

Bagian dari `contracts/api-contract.md` 12.2 dan 12.4; tidak ada endpoint yang hanya muncul di sini.

| Tag | Endpoint | Hak akses | Epic | Status |
|---|---|---|---|---|
| `Health Services / Inpatient Management / Inpatient Admission Workspace` | `GET /summary`, `/print-logs` | `InpatientAdmissionDocument : Read` | `EPIC-RWA-01` | **Rencana (belum tersedia)** |
| Sama | `GET /summary/amounts` | `: ViewAmount` | `EPIC-RWA-01`, `08` | **Rencana (belum tersedia)** |
| Sama | `GET /letterhead`, `/general-consent/print-data` | `: Read` | `EPIC-RWA-14` | **Rencana (belum tersedia)** |
| Sama | `GET /prefill/{documentType}`, `/documents`, `/documents/{id}` | `: Read` | `EPIC-RWA-03`, `06`, `08`, `10`, `11`, `12` | **Rencana (belum tersedia)** |
| Sama | `POST /documents`; `PUT /documents/{id}`; `PATCH …/lock`, `/unlock`, `/discard`; `POST …/revisions` | `: Create`, `: Update` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| Sama | `PATCH …/cancel` | `: Cancel` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| Sama | `POST …/signatures/patient-or-family`, `/admission-officer`, `/cro`, `/receiving-nurse`, `/head-nurse` | `: Sign`, `: SignAsCro`, `: SignAsNurse`, `: SignAsHeadNurse` | `EPIC-RWA-03`, `06`, `12` | **Rencana (belum tersedia)** |
| Sama | `GET …/documents/{id}/amounts`, `/amount-print` | `: ViewAmount` | `EPIC-RWA-08` | **Rencana (belum tersedia)** |
| Sama | `GET …/documents/{id}/print`; `POST /print-logs`; `GET /identity-labels` | `: Print` | `EPIC-RWA-05`, `12` | **Rencana (belum tersedia)** |
| Sama | `GET /base-data`, `/base-data/amounts` | `: Read`, `: ViewAmount` | `EPIC-RWA-07` | **Rencana (belum tersedia)** |
| Sama | `GET /patient-rights` | `InpatientEpisode : Read` | `EPIC-RWA-06`, `11` | **Rencana (belum tersedia)** |
| Sama | `PUT /procedure-plan-mark` | `: Update` | `EPIC-RWA-09` | **Rencana (belum tersedia)** — di luar gelombang |
| `Health Services / Master Data / Inpatient Clearance Item` | Isian jenis, induk, sumber saran | `InpatientClearanceItem : Read/Create/Update` | `EPIC-RWA-03` | ✅ Tersedia; isian **Rencana** |
| `Health Services / Master Data / Inpatient Setting` | Sebelas isian | `InpatientSetting : Read/Update` | `EPIC-RWA-05`, `12` | ✅ Tersedia; isian **Rencana** |
| `Health Services / Inpatient Management / Inpatient Episode` | `GET /{id}` peringatan | `InpatientEpisode : Read` | `EPIC-RWA-01` | ✅ Tersedia; isi **Rencana** |

### 24.14 Matriks kewenangan

`contracts/permission-audit-matrix.md` bagian 10 — sepuluh aksi `InpatientAdmissionDocument`, penjaga `GUARD-RWA-01` s.d. `07`.

### 24.15 Batas integrasi dan billing

Workspace PPRI **tidak** menghitung kekurangan deposit atau tarif, **tidak** menulis ke Billing, Clinical, Patient Management, Registration, maupun Rekam Medis, **tidak** membuat tabel persetujuan kedua, **tidak** menahan admisi, penempatan, perawatan, transfer, keputusan pulang, atau penutupan, dan **tidak** menurunkan kelas otomatis. Semua bacaan lewat service modul pemilik (`INT-RWA-01` s.d. `14`). Dokumen admisi tidak menimbulkan biaya, sehingga kontrak `integrasi-billing` tidak bergerak.

### 24.16 Guardrail regulasi

Rujukan PRD bagian 16 tetap sebagai acuan verifikasi pemilik hukum: persetujuan pasien (General Consent tetap cetak sampai `DEC-INP-003`), rekam medis elektronik (tanpa hapus permanen, versi, jejak audit), pelindungan data pribadi (kolom sensitif tidak masuk log, nomor identitas disamarkan, QR tanpa data pribadi), keabsahan tanda tangan elektronik (mode kertas), standar akreditasi hak pasien dan identifikasi gelang. Gerbang produksi: G-35 masa simpan dan privasi data keluarga, G-38 verifikasi keselamatan pasien atas aturan gelang, G-42 penerimaan risiko nilai kepercayaan belum tampil di ruang kerja klinis.

### 24.17 Kebutuhan non-fungsional

`NFR-RWA-01` s.d. `14` PRD tetap berlaku, dengan penyesuaian: `NFR-RWA-05` hanya "dokumen `Completed` tidak berubah" (hash berkas tanda tangan milik `EPIC-RWA-13`); `NFR-RWA-13` dipenuhi penyegaran berkala.

| ID | Kebutuhan | Ukuran |
|---|---|---|
| `NFR-RWA-15` | Penyegaran dokumen yang menunggu tanda tangan setiap 30 detik, berhenti saat lengkap atau layar ditinggalkan | Tanda tangan pihak lain tampil ≤ 30 detik |
| `NFR-RWA-16` | Kegagalan sumber modul lain tidak menggagalkan Detail Episode | Detail episode tampil dengan peringatan "tidak dapat dihitung" |
| `NFR-RWA-17` | Tanpa kode identitas rumah sakit, kota, dan kode formulir di berkas service, controller, DTO, dan komponen cetak Workspace PPRI | Pencarian source nol hasil (`RWI-AC-368`) |

### 24.18 Skenario UAT

Skenario lengkap dan buktinya di `testing/acceptance-test-matrix.md` bagian 21. UAT PRD yang tetap: `UAT-RWA-01` (dengan delapan menu sampai Estimasi dikirim), `02`, `05` s.d. `07`, `10` s.d. `18`, `21` s.d. `27`. `UAT-RWA-03`, `04` menunggu `EPIC-RWA-02`; `UAT-RWA-19`, `20` menunggu `EPIC-RWA-09`.

| ID | Epic | Jalur | Kondisi awal → langkah | Hasil yang diharapkan |
|---|---|---|---|---|
| `UAT-RWA-28` | `EPIC-RWA-03` | Gagal lalu berhasil | Bed Budi masih dipesan → Andi menandatangani Perawat; Budi menempati bed → ulangi | Ditolak "Pasien belum menempati tempat tidur"; lalu `Completed` |
| `UAT-RWA-29` | `EPIC-RWA-01`, `08` | Gagal | Andi tanpa `ViewAmount` membuka header, IPD, dan Pelunasan Deposit | "lihat kasir"; tombol cetak Pelunasan Deposit tidak ada; panggilan langsung `403` |
| `UAT-RWA-30` | `EPIC-RWA-14` | Berhasil | Sari menekan Cetak Persetujuan; mencetak kedua tab | Tiba di tab Surat Persetujuan; nol permintaan tulis; kop dari profil |
| `UAT-RWA-31` | `EPIC-RWA-12` | Berhasil | Pelunasan Deposit dikunci Rp 3.000.000; deposit bertambah Rp 1.000.000; tanda tangan dicatat | Dokumen tetap Rp 3.000.000; header Rp 2.000.000 (`RWI-AC-385`) |
| `UAT-RWA-32` | `EPIC-RWA-03` | Gagal (regresi) | 18 butir serah terima aktif; petugas menutup episode lain | Penutupan hanya membaca butir penutupan; episode dapat ditutup |
| `UAT-RWA-33` | `EPIC-RWA-05` | Gagal | Label pasien asuransi dengan kartu kosong; gelang dewasa dicatat untuk bayi | Baris No. Kartu kosong; catatan cetak ditolak |

### 24.19 Definition of Done

| Butir | Bukti |
|---|---|
| Tombol "Workspace PPRI" tampil tepat sesudah Workspace Dokter hanya bagi pemegang hak baca | `UAT-RWA-01`, `02`; `RWI-AC-343` |
| Pengguna Workspace PPRI tidak butuh hak baca modul lain | `RWI-AC-378` |
| Kelengkapan dihitung menurut `RWI-DEC-234` dan hanya memperingatkan | `UAT-RWA-01`; `RWI-AC-345`, `346` |
| Serah Terima: tiga orang berbeda, perawat sesudah pasien di bed, butir dari master dan beku | `UAT-RWA-05` s.d. `07`, `28`; `RWI-AC-353`, `359`, `360`, `376` |
| Penutupan episode tidak terganggu butir serah terima | `UAT-RWA-32`; `RWI-AC-361` |
| Gelang dan label benar, QR No. RM saja, tanpa nomor karangan, cetak ulang beralasan | `UAT-RWA-10` s.d. `12`, `33`; `RWI-AC-363`, `364`, `374`, `380` |
| IPD merangkum dokumen, garis kosong untuk isian tanpa sumber, cetak ditahan bila data wajib gagal | `UAT-RWA-15`, `16`; `RWI-AC-365`, `375` |
| Privasi, Selisih Biaya, Nilai Kepercayaan tersimpan per baris, aturan wajib dan batas butir ditegakkan | `UAT-RWA-13`, `14`, `21` s.d. `24`; `RWI-AC-357`, `362`, `377` |
| Pelunasan Deposit dari angka Billing, jatuh tempo dalam batas, angka beku saat dikunci | `UAT-RWA-17`, `18`, `31`; `RWI-AC-366`, `367`, `382`, `385` |
| Rupiah hanya bagi `ViewAmount` | `UAT-RWA-29`; `RWI-AC-379` |
| General Consent cetak saja; Cetak Persetujuan dialihkan | `UAT-RWA-30`; `RWI-AC-347` s.d. `349` |
| Koreksi lewat versi; tidak ada hapus; batal beralasan; simpan bersamaan ditolak | `UAT-RWA-25` s.d. `27`; `RWI-AC-355`, `356` |
| Tidak ada identitas rumah sakit, kota, kode formulir, maupun data karangan di kode Workspace PPRI | `RWI-AC-368`; `NFR-RWA-17` |
| Sepuluh aksi `InpatientAdmissionDocument` terdaftar di registry | Acceptance 21.8 |
| Butir serah terima dan pengaturan cetak terisi di lingkungan target | `02-backend-architecture.md` 13.13; seeder non-produksi; isian admin produksi |
| Tidak ada epic `OPEN DECISION` di gelombang mana pun | 24.20 |

### 24.20 Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Epic | Syarat mulai |
|---|---|---|---|
| `RWA-MVP-0` | `E9` beserta saringan jenis penutupan; `E10`; `E12` hak dan data; service baca di modul pemilik; service inti dokumen; `FE-INP-12`, `FE-INP-13` | `EPIC-RWA-12` | Amandemen `0.11.0` disetujui; `RWI-OQ-126`, `RWI-OQ-127` disetujui; gerbang `MasterData` `RWI-DEC-193` |
| `RWA-MVP-1` | `FE-INP-35`, `36`, `38`, `40`; perubahan `FE-INP-04`, `18`, `03` | `EPIC-RWA-01`, `05`, `07`, `14` | `RWA-MVP-0` selesai |
| `RWA-MVP-2` | `FE-INP-37`, `39`, `41`, `43`, `44` | `EPIC-RWA-03`, `06`, `08`, `10`, `11` | `RWA-MVP-1` selesai |
| Di luar gelombang | `EPIC-RWA-09` (`E11`, `FE-INP-42`) sesudah `DEC-INP-020`; `EPIC-RWA-02` dan `EPIC-RWA-13` sesudah `DEC-INP-003` | `OPEN DECISION` | Keputusan tercatat di decision log, lalu amandemen kecil |
| `POST-MVP` | Kemampuan bagian 24.8 selain tiga epic di atas | — | Di luar rilis pertama |

Gelombang memakai awalan `RWA-` agar tidak bertabrakan dengan gelombang Rawat Inap lain. Task Finishing (`BE-RWI-172` s.d. `184`, `FE-RWI-192` s.d. `201`) tidak bergantung pada gelombang ini; ID task bebas berikutnya per 7 Oktober 2026: `BE-RWI-185` dan `FE-RWI-210` (`FE-RWI-202` s.d. `209` sudah dipakai roadmap dan laporan lain); diperiksa ulang saat perencanaan.

**Pertanyaan terbuka sebelum development lock**

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| ~~`RWI-OQ-126` service baca `PatientManagement`~~ | Pemilik `PatientManagement`, lewat Muhammad Hamzah | **Disetujui `RWI-DEC-266`, 2026-10-08** | Tidak — tertutup |
| ~~`RWI-OQ-127` service baca profil rumah sakit~~ | Pemilik HR Master Data, lewat Muhammad Hamzah | **Disetujui `RWI-DEC-266`, 2026-10-08** | Tidak — tertutup |
| `RWI-OQ-124` peran CRO, supervisor admisi, `SignAsHeadNurse` di lingkungan target | Admin Akses Role | Uji penerimaan tiga tanda tangan tidak dapat dijalankan | Tidak untuk pembangunan; **ya** untuk UAT |
| `RWI-OQ-128` dokter perujuk luar | Pemilik Registration | Satu isian IPD garis kosong | Tidak |
| `RWI-OQ-129` method tarif kamar harian Billing | Yasmina | "Rencana @ Kamar (Rp)" tertulis "lihat kasir" | Tidak |
| `DEC-INP-020` (`RWI-OQ-122`) tarif visit dan catatan biaya bedah | Yasmina | `EPIC-RWA-09` tetap di luar gelombang | Ya — hanya `EPIC-RWA-09` |
| `DEC-INP-003` (`RWI-OQ-116`) pemilik privasi/hukum | Muhammad Hamzah menunjuk | `EPIC-RWA-02`, `13` tetap di luar gelombang; gerbang produksi G-35 | Ya — hanya epic di luar gelombang |
| `RWI-OQ-121` judul halaman | Muhammad Hamzah | Usulan "Ruang Kerja PPRI" dipakai | Tidak |
| `RWI-OQ-123` isian `SD_BELIEFS` keperawatan | Muhammad Hamzah | Dua tempat mencatat nilai kepercayaan sampai amandemen keperawatan | Tidak |

**Konsekuensi yang perlu disadari pemilik.** Karena Estimasi Biaya berada di luar gelombang, navigasi MVP menampilkan **delapan** menu, bukan sembilan seperti `RWI-DEC-226`; menu kesembilan muncul saat `EPIC-RWA-09` dikirim, mengikuti aturan "menu yang kemampuannya belum dikirim tidak ditampilkan" (`FR-RWA-004`).

**Diperbarui 8 Oktober 2026.** Dokumen ini disetujui (`RWI-DEC-265`) dan `RWI-OQ-126` serta `RWI-OQ-127` disetujui (`RWI-DEC-266`), sehingga tidak ada lagi pertanyaan memblokir untuk `RWA-MVP-0` s.d. `RWA-MVP-2`. Dokumen ini diteruskan ke `plan-module-delivery`: `roadmap/backend-roadmap-workspace-ppri.md` dan `roadmap/frontend-roadmap-workspace-ppri.md`.

---

## 25. Amandemen Alur Admisi Pendaftaran Pasien Rawat Inap ★ Revisi Tim Analisis Bisnis (`RWI-DEC-267` s.d. `RWI-DEC-273`)

Ditulis 10 Oktober 2026 sebagai respon atas mandat revisi operasional dari Tim Analisis Bisnis (Mba Ilma). Status: **`approved`** — disetujui pengguna 10 Oktober 2026 atas instruksi eksplisit "setujui dan lakukan /plan-module-delivery". Bagian ini mengamandemen struktur pendaftaran admisi rawat inap (Pasien Baru dan Pasien Lama) serta mekanisme penandatanganan formulir persetujuan umum rawat inap.

### 25.1 Latar Belakang & Mandat Bisnis

Hasil evaluasi alur admisi rawat inap di meja pendaftaran rumah sakit menemukan beberapa titik gesekan operasional:
1. Validasi nomor telepon kontak darurat belum membatasi panjang digit secara ketat, menyebabkan inkonsistensi data integrasi SMS/WhatsApp gateway.
2. Pemilihan Unit Layanan Rawat Inap pada formulir dokter membingungkan petugas karena unit perawatan rawat inap sudah terikat secara implisit pada instalasi rawat inap.
3. Alur persetujuan umum (*General Consent*) sebelumnya hanya mencetak lembar kosong untuk ditandatangani basah, lalu diinput ulang di ruang kerja PPRI. Dibutuhkan digitalisasi formulir dan tanda tangan langsung di loket admisi (*digital signature canvas*) agar proses di loket PPRI murni mencetak (*read-only / print-ready*).
4. Pembedaan jenis kunjungan (Umum vs Rujukan luar) belum terekam sejak awal kedatangan pasien baru, padahal data rujukan faskes perujuk dibutuhkan untuk kelengkapan administrasi klaim dan rujukan balik.
5. Pilihan kategori pasien terlalu banyak dan tumpang tindih; perlu disederhanakan menjadi tepat 3 opsi fungsional.
6. Pencarian dan verifikasi data pasien lama terpecah menjadi dua langkah terpisah yang memperlambat pelayanan loket.

### 25.2 Batas Scope Amandemen

| Aspek | Di Dalam Scope Amandemen | Di Luar Scope Amandemen |
|---|---|---|
| **Kontak Darurat** | Validasi `emergencyContactPhoneNumber` numerik, `maxLength={13}` pada frontend dan backend (`RWI-DEC-267`, `RWI-AC-388`) | Perubahan struktur tabel `TrxPatientEmergencyContact` |
| **Unit Tujuan Dokter** | Penyembunyian elemen dropdown unit layanan dari antarmuka langkah Dokter; pengikatan otomatis default Service Unit Rawat Inap pada backend payload (`RWI-DEC-268`, `RWI-AC-389`) | Perubahan relasi `ServiceUnitId` pada `TrxPatientEncounter` |
| **Jenis Kunjungan (PB)** | Langkah 1 Pasien Baru: Pilihan `Umum` vs `Rujukan`. Perekaman 5 atribut rujukan tekstual ringkas (Nomor, Tgl/Jam, Faskes, Dokter, Diagnosa) tanpa upload file fisik (`RWI-DEC-269`, `RWI-AC-390`) | Integrasi bridging rujukan online BPJS P-Care/VClaim otomatis |
| **Kategori Pasien (PB)** | Langkah 2 Pasien Baru: Tepat 3 opsi (`Umum`, `Bayi Baru Lahir`, `Pegawai`). Penautan episode ibu untuk bayi (`RWI-DEC-270`, `RWI-AC-391`) | Pembuatan master kategori pasien baru |
| **Pasien Lama** | Langkah 1 Pasien Lama: Split layout 1 langkah terpadu (Kanan: Form Cari No RM/NIK, Kiri: Kartu Identitas Terverifikasi). Langkah 2: Jenis Kunjungan & Kategori Pasien Lama (`RWI-DEC-271`, `RWI-AC-392`) | Penggabungan data duplikat pasien lama |
| **Persetujuan & TTD Digital** | Langkah 10 PB / Langkah 9 PL: *Dedicated Step* form persetujuan rawat inap & kanvas TTD digital interaktif (`react-signature-canvas`). Penyimpanan formulir dan citra PNG TTD ke episode (`RWI-DEC-272`, `RWI-AC-393`) | Penerbitan sertifikat digital PSrE berbayar pihak ketiga (BSrE/Peruri) |
| **Invariant Hukum Bayi** | Penguncian otomatis subjek penanda tangan pada kategori Bayi Baru Lahir ke Orang Tua / Wali sah; penolakan opsi Pasien Sendiri (`RWI-DEC-273`, `RWI-AC-394`) | Dispensasi hukum medis di luar perwalian |
| **Integrasi PPRI** | Menu General Consent di Workspace PPRI berstatus *read-only / print-ready* mengambil formulir ber-TTD yang sudah tersimpan di admisi (`RWI-AC-395`) | Modifikasi siklus dokumen Workspace PPRI yang lain |

### 25.3 Functional Requirements Baru

| ID | Kebutuhan Fungsional | Prioritas | Dasar |
|---|---|:---:|---|
| `FR-RI-179` | Sistem wajib memvalidasi nomor telepon kontak darurat pasien maksimal 13 karakter numerik pada langkah pendaftaran admisi | **MUST** | `RWI-DEC-267`, `RWI-AC-388` |
| `FR-RI-180` | Sistem wajib menyediakan langkah pemilihan jenis kunjungan (Umum / Rujukan) pada awal alur pendaftaran pasien baru dan merekam data rujukan tekstual ringkas bila opsi rujukan dipilih | **MUST** | `RWI-DEC-269`, `RWI-AC-390` |
| `FR-RI-181` | Sistem wajib membatasi pilihan kategori pasien pada langkah admisi rawat inap menjadi tepat 3 opsi: Pasien Umum, Bayi Baru Lahir, dan Pegawai/Karyawan RS | **MUST** | `RWI-DEC-270`, `RWI-AC-391` |
| `FR-RI-182` | Sistem wajib menyatukan form pencarian dan panel verifikasi data identitas pasien lama ke dalam 1 tampilan layar terpadu (split layout kanan-kiri) pada Langkah 1 jalur Pasien Lama | **MUST** | `RWI-DEC-271`, `RWI-AC-392` |
| `FR-RI-183` | Sistem wajib menyembunyikan dropdown pemilihan unit layanan rawat inap dari antarmuka langkah Dokter dan otomatis mengikat default unit layanan rawat inap pada pembuatan kunjungan/episode | **MUST** | `RWI-DEC-268`, `RWI-AC-389` |
| `FR-RI-184` | Sistem wajib menyediakan langkah mandiri (*Dedicated Step*) Form Persetujuan Rawat Inap & Tanda Tangan Digital sebelum cetak, menyimpan isian formulir dan citra tanda tangan digital ke episode rawat inap | **MUST** | `RWI-DEC-272`, `RWI-AC-393` |
| `FR-RI-185` | Sistem wajib menegakkan invariant hukum medis yang mengunci subjek penanda tangan persetujuan rawat inap untuk pasien kategori Bayi Baru Lahir kepada Orang Tua atau Wali sah dan melarang penandatanganan mandiri | **MUST** | `RWI-DEC-273`, `RWI-AC-394` |
| `FR-RI-186` | Sistem wajib menampilkan dokumen General Consent di Workspace PPRI dalam keadaan terisi lengkap dan bertanda tangan (*read-only / print-ready*) tanpa memerlukan penginputan atau penandatanganan ulang di loket PPRI | **MUST** | `RWI-DEC-272`, `RWI-AC-395` |

### 25.4 Acceptance Criteria & Verification Traceability

| ID Kriteria | Deskripsi Kriteria Penerimaan | Verifikasi |
|---|---|---|
| `RWI-AC-388` | Input No. HP Kontak Darurat membatasi panjang maksimal 13 karakter numerik dan menolak karakter alfabet/simbol pada form pendaftaran pasien baru | Automated UI test & Backend validation test |
| `RWI-AC-389` | Elemen dropdown Unit Tujuan tidak dirender pada langkah Dokter, dan payload `POST /patient-encounters` otomatis mengirimkan `ServiceUnitId` default rawat inap yang valid | End-to-end integration test |
| `RWI-AC-390` | Langkah 1 Pasien Baru menampilkan opsi Umum dan Rujukan; memilih Rujukan memunculkan form tekstual nomor, tanggal/jam, faskes, dokter, dan diagnosa rujukan tanpa meminta upload file | Form interaction test |
| `RWI-AC-391` | Langkah 2 Pasien Baru hanya menampilkan 3 kartu kategori (Umum, Bayi Baru Lahir, Pegawai); opsi Ibu, Anak, dan Korporat tidak muncul di antarmuka | Visual regression test |
| `RWI-AC-392` | Langkah 1 Pasien Lama menampilkan layout split terpadu (kolom kanan: input pencarian No. RM/NIK + tombol Cari; kolom kiri: kartu hasil identitas pasien terdaftar + tombol Ganti Pasien) | Responsive layout test |
| `RWI-AC-393` | Langkah 10 PB / Langkah 9 PL menyediakan formulir persetujuan rawat inap dan kanvas digital signature interaktif dengan tombol bersihkan dan kunci tanda tangan, serta menyimpan berkas ke episode | Signature canvas component test |
| `RWI-AC-394` | Pasien berkategori Bayi Baru Lahir secara otomatis mengunci opsi penanda tangan menjadi "Orang Tua / Wali" dan menonaktifkan opsi "Pasien Sendiri" | Authorization & validation gate test |
| `RWI-AC-395` | Menu General Consent pada Workspace PPRI untuk episode yang telah disetujui menampilkan dokumen lengkap ber-TTD digital dalam format siap cetak tanpa form isian aktif | Integration test PPRI |

---

## 26. Amandemen Bed Management — PRD ke MVP, 10 Oktober 2026

Bagian26 adalah batas rilis pertama **slice Bed Management** dan menggantikan hanya aturan bed yang bertentangan pada versi lama. Bagian modul lain tidak berubah. Disusun terakhir setelah arsitektur, data, kontrak dan flow berdiri; tidak membuat keputusan/endpoint baru. **draft; belum mendapat approval desain manusia.**

### 26.1 Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk / blueprint | Quilvian / RWI-BP-001, sub-modul episode-rawat-inap, slice Bed Management |
| Revision artefak / status | 0.12.0 / draft; set kontrak mengikuti manifest anak; last_changed_in0.12.0 |
| Owner / approval | Produk/domain/API Muhammad Hamzah; UI DEC-292; security/privacy OPEN; approved_by=null; approved_at=null |
| Repository / baseline | NewQuilvianSystemBackend@d4e1eca06fb28c05934c68c1e51a4dca01935a10; QuilvianSystemFrontendDev@969acfcc04cdf31074a1911e9827c31d25ddadd0 |
| Input revision/readiness | Decision46 DEC-274–294/AC396–426; BM-AUD-20261010-01 rev1; BM-RCG-20261010-01 gate1.12 enam scope READY_FOR_DOMAIN_DESIGN; DOMAIN_ARCHITECTURE_NOT_RUN untuk bounded slice ini |
| Ringkasan cakupan | Bed siap dipesan → hunian/transfer → used release → cleaning+verifikasi → siap kembali, dengan history dan guard semua writer |

Input hashes upstream mengikuti manifest14.1. Input hash artefak teknis saat PRD diturunkan:

| Input relatif | SHA256 |
| --- | --- |
| 02-backend-architecture.md | 8eb9ae69d05cfe2013400a6cb73c721d7a235a9b0f3d7115ffe0296693e815f6 |
| data/data-dictionary.md | f411d5b87f54ddb53b241b78c51f5ab4ebd22100c6a3e07aae8b192263e87098 |
| contracts/api-contract.md | ddd31d88a469239e6f731eea2fe3023499fd0137ae3e35c3de284b03a1c7c531 |
| contracts/state-transition-matrix.md | 78a286e0f8441dd626c5e9fb6687ecdc549d7410d8e7867a1cdc45a12bd274e5 |
| contracts/validation-matrix.md | a8a225a84f51b9a1e455285edfc0458c830ef1775d59f767d6e49e1664bf1dce |
| contracts/permission-audit-matrix.md | 893c0c2d24e8ed3de0f25b42aec853d5b05e22bc89c0acf06371e6949f680960 |
| contracts/integration-contract.md | 6ea617368b475f2493c59994181e7b8238574f1913f8857cb0095e4c84ac3714 |
| 03-frontend-architecture.md | 5f18ddfbf44e7af2b8192f407a1cf6c625e6b38e7a1699ffe0bbabec308165d4 |
| testing/acceptance-test-matrix.md | c8c67cb6504021e68832b2b42b9d272f3209f6435b025bdab88c88380d0f26af |
| flowcharts/00-alur-utama.md | a2a5d5897366620db03972cdad6fa32ec807af6539b3f8522588ea3ae71ed13b |
| flowcharts/09-bed-reservation-release.md | d7015fe25aa0b71af88b97b0756bc59e88f3034d66c6dbedc601ffb67f475837 |
| flowcharts/10-bed-transfer.md | adff6804dbc83f16f1ff99c6ace63bcb80cf409bdf54aa0452d61019cbc19609 |
| flowcharts/11-bed-cleaning-readiness.md | a2bd18ac9d8b49125e69e6fd929e1d19423149c3dc8118f4a7dd42cab35f838c |
| flowcharts/12-bed-closure-reopen.md | a9340f628728ef16c556b3f2bf1e718e0826c8b095a89b3b1f47dd0f54f3abb4 |
| flowcharts/13-bed-usage-history-correction.md | 92d3e1eaf472c44ec7e94592bd4039b651cebf0a98f69ab9545dde8ff6161651 |
| flowcharts/14-bed-uncertain-outcome.md | 114cd5d9bd6b8cd3a8b6d28019810c8362f5935fe25ac2d267b9ed2f90edd788 |

### 26.2 Ringkasan eksekutif

MVP membantu admisi memilih bed yang benar-benar siap, petugas memindahkan pasien tanpa kehilangan lokasi, HK mencatat pekerjaan dan perawat yang ditunjuk menyatakan kesiapan. Pengelola dapat melihat siapa memakai bed/kamar pada suatu periode termasuk pasien yang tidak berpindah. Setelah used release, bed ditahan sampai siap; selesai dibersihkan saja belum cukup.

### 26.3 Masalah produk

Audit source BM-AUD-20261010-01 menemukan alur reservasi, transfer dan history episode yang dapat dipakai ulang, tetapi raw Cleaning hanya satu, used release langsung Available, guard master/predicate tidak seragam, cancel reason hilang dan konfirmasi dapat membaca data lama (F01–07). History transfer bukan semua penggunaan per bed. Itu bukti source, bukan reproduksi bug PostgreSQL atau signoff lingkungan rumah sakit.

### 26.4 Visi produk

Master bed/kamar/unit/kelas resmi → readiness terverifikasi → reservasi/hunian → lokasi pasien current → transfer atau keluar fisik → cleaning dan pengesahan → bed siap kembali; semua segmen hunian tersambung ke episode dan audit, sementara Billing tetap membaca timeline sah.

### 26.5 Batas MVP

Titik mulai:

1. Master bed/kamar/unit/kelas existing valid dan actor memiliki scope sah.
2. Bed mempunyai root kesiapan; default legacy Unverified ditahan sampai ada bukti sah.
3. Episode/DPJP/eligibility/folio existing tetap sumber guard klinis dan keuangan.

Titik akhir:

1. Used bed belum bookable sampai verifikator sah mengesahkan current cycle.
2. Transfer sah atomik dengan asal otomatis, tujuan available dan kategori manual tervalidasi.
3. History initial/no-transfer/transfer/ongoing/correction dapat dibaca sesuai hak.
4. Runtime baru dapat dinyatakan siap setelah proof gates dan verifikasi target terpenuhi.

### 26.6 Pelaku sasaran

| Pelaku | Tanggung jawab |
| --- | --- |
| Admisi | Pesan/place/cancel sesuai existing rights, alasan wajib dan readiness current |
| Perawat ruangan | Transfer/record departure sesuai existing rights dan episode scope |
| Housekeeping individu | Mulai/selesai sesuai unit dan SOP sah; tidak membaca pasien/history |
| Perawat verifikator ditunjuk | Inspeksi dan sahkan/tolak current cycle dengan referensi bukti |
| Admin MasterData seluruh tim | Kelola master melalui guard; bukti urutan kelas resmi |
| Viewer/supervisor authorized | History masked sesuai hak, correction existing Billing OPEN |
| Pemilik akses/privacy/operasional | Menyediakan proof actual sebelum aktivasi, bukan grant dari AI |

### 26.7 Pemilihan kemampuan MVP

Seluruh epic berikut MUST HAVE karena tanpa salah satunya rangkaian bed siap→dipakai→dilepas→siap kembali atau bukti history/akses tidak utuh. Tidak ada jalan sementara aman yang meloloskan dirty bed atau identitas ke HK.

| Kemampuan | ID kemampuan asal audit section5 | Keputusan MVP |
| --- | --- | --- |
| EPIC BM-01 — Monitoring dan akses konsisten | BM-CAP-01/02/03/15 | MUST HAVE; EXTEND |
| EPIC BM-02 — Pemesanan, pelepasan dan keselamatan transaksi | BM-CAP-04/05/08/14/16 | MUST HAVE; EXTEND |
| EPIC BM-03 — Pembersihan dan pengesahan | BM-CAP-07/09 | MUST HAVE; MISSING / NEW |
| EPIC BM-04 — Tidak Tersedia beralasan | BM-CAP-10 | MUST HAVE; EXTEND |
| EPIC BM-05 — Transfer manual tervalidasi | BM-CAP-11/12 | MUST HAVE; EXTEND |
| EPIC BM-06 — Usage history dan koreksi | BM-CAP-13/17 | MUST HAVE; EXTEND |

BM-CAP-16 adalah proof readiness pada epic02/DoD, bukan klaim kemampuan runtime sudah ada. Link audit: [Capability evidence map](../../../../../artifacts/bed-management/01-existing-capability-map.md).

### 26.8 Kemampuan yang ditunda

| Kemampuan | ID/asal | Alasan | Pengganti selama MVP |
| --- | --- | --- | --- |
| Reminder30 menit / extend reservation | BM-CAP-06 | DEC-283 mempertahankan TTL existing tanpa perluasan pass | Countdown informatif + lazy server expiry120m |
| Reservasi tujuan dan penerimaan transfer dua fase | BM-CAP-11 alternatif audit | DEC-284 mempertahankan atomic one-step, tidak menambah gate handover | Transfer commit + handover existing sesudahnya |
| History export baru / timeline pasien mencampur HK dan closure | Perluasan BM-CAP-13 | DEC-286 memisahkan patient stay segments dan operational audit | History per-bed paged; transfer report/export existing tetap |
| Offline queue / checklist klinis cleaning / akun dan role otomatis | DEC-287/290/293 | Bukan scope produk dan SOP/authority belum dibuktikan | Tahan aksi saat gangguan, SOP sah+reconcile authorized; gate activation |
| Tarif/ledger/room charge kedua | BM-CAP-17 | Pemilik Billing existing | Gunakan timeline + notification canonical integrasi-billing |

### 26.9 Alur bisnis target

1. Petugas membuka Bed Management dan mendapat data scoped dengan enam status.
2. Admisi memilih bed available/eligible; server memeriksa ulang dan mencatat reservation atau placement.
3. Transfer mengambil asal current otomatis; petugas memilih tujuan tersedia, kategori manual dan alasan, lalu confirmation menunggu refresh selesai.
4. Server mengakhiri asal dan menempatkan tujuan dalam satu commit; asal atau bed bekas departure masuk WaitingCleaning.
5. HK mencatat mulai dan selesai fisik dengan akun masing-masing; selesai tetap menunggu verifikasi.
6. Perawat ditunjuk memeriksa dan mengesahkan current cycle, atau menolak beralasan sehingga perlu cleaning lagi.
7. Setelah semua syarat sah, bed Available dapat dipesan; history semua penggunaan dan versi koreksi tetap tersedia bagi pihak berwenang.
8. Timeout diperiksa melalui own committed outcome; tidak ada sukses fiktif, retry ganda atau pelepasan bed pasien berikutnya.

Flow references: FLOW-BM-MVP-001..006 pada flowcharts09..14; flowchart tidak disalin ke PRD.

### 26.10 Epic dan functional requirement

#### EPIC BM-01 — Monitoring dan akses konsisten

**MUST HAVE. Disposisi backend: EXTEND.** Tujuan: Monitoring dan akses konsisten. Trace: DEC-274/285/287/289/292; RWI-AC-396–398/416–417/420/424. Istilah EXTEND mencakup repair yang ditandai audit; tidak ada OPEN DECISION pada epic ini.

| FR | Perilaku yang diuji | Contoh expected |
| --- | --- | --- |
| FR-BM-001 | Menu route lama bernama Bed Management mempunyai tiga tab; enam status/counter mengikuti shared predicate dan permission scoped. | Fixture 6 status + bed inactive: jumlah Available mengecualikan inactive; HK tidak menerima PatientName/EpisodeId dalam JSON. |
| FR-BM-002 | Server dan UI memeriksa scope/unit/episode; unknown deny; confirmation menunggu pembacaan fresh selesai. | Actor unitA membuka unitB →403; refetch pending →confirm disabled; response lama tidak menimpa hasil baru. |

#### EPIC BM-02 — Pemesanan, pelepasan dan keselamatan transaksi

**MUST HAVE. Disposisi backend: EXTEND.** Tujuan: Pemesanan, pelepasan dan keselamatan transaksi. Trace: DEC-281/283/288–290; RWI-AC-407–408/413/419/421–422/426. Istilah EXTEND mencakup repair yang ditandai audit; tidak ada OPEN DECISION pada epic ini.

| FR | Perilaku yang diuji | Contoh expected |
| --- | --- | --- |
| FR-BM-003 | Reserve tetap parameter120 menit, unused cancel/expiry tidak dirty; cancel menyimpan alasan. Used departure/release membuat WaitingCleaning. | A keluar10.00, B pesan10.01 sebelum verifikasi →422; expiry unused tidak membuat cleaning attempt. |
| FR-BM-004 | Seluruh mutation atomik, recheck holder setelah lock, expected versions dan key stabil; old episode closure tidak melepas new occupant. | Reserve A versus place B →satu pemenang; samekey retry →satu commit; closure A13.00 bed B tetap occupied. |

#### EPIC BM-03 — Pembersihan dan pengesahan

**MUST HAVE. Disposisi backend: MISSING / NEW.** Tujuan: Pembersihan dan pengesahan. Trace: DEC-278/280/282/287/293; RWI-AC-406/409–411/425. Istilah EXTEND mencakup repair yang ditandai audit; tidak ada OPEN DECISION pada epic ini.

| FR | Perilaku yang diuji | Contoh expected |
| --- | --- | --- |
| FR-BM-005 | HK individu mencatat mulai/selesai current cycle; selesai menunggu verifikasi, status tetap Dalam Pembersihan. | HK selesai10.20 →AwaitingVerification; permintaan pesan10.22 ditolak. |
| FR-BM-006 | Perawat verifikator sah mengesahkan atau menolak beralasan; upaya/actor/time tersimpan dan request lama tidak membuka siklus baru. | Verifier10.25 ready diterima; reject blank ditolak; current reject valid kembali Waiting, attempt lama retained. |

#### EPIC BM-04 — Tidak Tersedia beralasan

**MUST HAVE. Disposisi backend: EXTEND.** Tujuan: Tidak Tersedia beralasan. Trace: DEC-285/289; RWI-AC-412/419/424. Istilah EXTEND mencakup repair yang ditandai audit; tidak ada OPEN DECISION pada epic ini.

| FR | Perilaku yang diuji | Contoh expected |
| --- | --- | --- |
| FR-BM-007 | Seluruh master writer termasuk hierarchy terdampak menolak penutupan/disable/move bed held, dan menyimpan alasan sah. | Bed reserved: PUT/status/availability/delete/nonactive →409, reservation tetap. |
| FR-BM-008 | Reopen menginvalidasi bukti lama dan belum Ready; konflik legacy menampilkan holder authoritative dan block pemesanan. | Bed kosong closed dibuka→Unverified/Waiting sesuai fakta; rawUnknown tanpa holder tidak Available. |

#### EPIC BM-05 — Transfer manual tervalidasi

**MUST HAVE. Disposisi backend: EXTEND.** Tujuan: Transfer manual tervalidasi. Trace: DEC-275–277/284/291; RWI-AC-399–405/414/423. Istilah EXTEND mencakup repair yang ditandai audit; tidak ada OPEN DECISION pada epic ini.

| FR | Perilaku yang diuji | Contoh expected |
| --- | --- | --- |
| FR-BM-009 | Server menyediakan asal current otomatis dan tujuan eligible tersedia; petugas wajib memilih Down/Up/Same secara manual sesuai order resmi global. | Same actual class ID beda bed: manual Same diterima; Up resmi pilih Down→422 no mutation. |
| FR-BM-010 | Transfer tetap satu commit, snapshot lokasi/kelas/kategori direkam; source waiting; handover sesudah commit tidak membalik success. | Inject fail antara end source dan new destination →rollback penuh; callback gagal sesudah commit →transfer tetap sah. |

#### EPIC BM-06 — Usage history dan koreksi

**MUST HAVE. Disposisi backend: EXTEND.** Tujuan: Usage history dan koreksi. Trace: DEC-286–288; RWI-AC-402/415/416/418. Istilah EXTEND mencakup repair yang ditandai audit; tidak ada OPEN DECISION pada epic ini.

| FR | Perilaku yang diuji | Contoh expected |
| --- | --- | --- |
| FR-BM-011 | Bed/periode history mencakup initial/no-transfer, transfer, ongoing dan versions; identity mengikuti existing authorization, HK deny. | Episode tanpa transfer tampil satu segmen; current End=NULL; master rename tidak mengganti snapshot lama. |
| FR-BM-012 | Correction versioned memakai existing Correct/Billing OPEN/reason/version; tidak delete atau cancel committed transfer. | Billing CLOSED →correction reject; normal transfer IsSuperseded=true tetap efektif jika SupersededByCorrectionId NULL. |

### 26.11 Model status yang diusulkan

Publik tepat enam: Tersedia, Terisi, Dipesan, Menunggu Pembersihan, Dalam Pembersihan, Tidak Tersedia. Selesai fisik adalah subphase Menunggu verifikasi pada Dalam Pembersihan. Holder authoritative tetap terlihat ketika raw/admin konflik; tidak bookable. Ready harus pada siklus terbaru, empty, master valid/aktif/reservable dan tidak ditutup. State authoritative pada contract state11, tidak menciptakan status dari PRD.

### 26.12 Sasaran arsitektur

Reuse master/episode/placement/reservation/permission/outbox/Billing, extend placement dengan kategori/snapshot dan reservation dengan cancel reason, tambah readiness/attempt/lifecycle event/receipt dalam InPatientManagement. Enums raw BedStatus dan route FE existing dipertahankan. Tabel ownership/backend14 dan data21 authoritative. M1–M3 backfill/cutover/rollback menjaga dirty bed fail-closed; old clients diperbarui bersama new guards. Frontend15 menyatakan reuse base components dan child screens.

### 26.13 Sasaran kemampuan API

Subset turunan API contract13, bukan mapping permission kedua yang boleh diedit terpisah. Semua Data JSON memakai ApiResponse existing; old routes berlabel diperbarui belum memiliki target behavior. GET detail master tambahan memasok versions. Header key/expected versions serta error canonical ada di contract.

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /metadata | Metadata tab/status/kategori dan hak aksi | InpatientBedManagement : Read | serviceUnitId? | BedManagementMetadataResponse | EPIC BM-01 | Rencana (belum tersedia) |
| GET | /monitoring | Monitoring sanitized dan counts | InpatientBedManagement : Read | BedMonitoringQuery | BedMonitoringResponse | EPIC BM-01 | Rencana (belum tersedia) |
| GET | /beds/{bedId}/cleaning-attempts | Jejak operasional bed tanpa pasien | InpatientBedManagement : Read | pageNumber/pageSize | PagedResult<BedCleaningAttemptResponse> | EPIC BM-03 | Rencana (belum tersedia) |
| GET | /usage-history | Semua segmen penggunaan per bed/periode | InpatientBedManagement : ReadUsageHistory | BedUsageHistoryQuery | PagedResult<BedUsageHistoryResponse> | EPIC BM-06 | Rencana (belum tersedia) |
| GET | /operations/{key} | Baca own committed outcome | InpatientBedManagement : Read | key; actor dari token | OperationOutcomeResponse | EPIC BM-02 | Rencana (belum tersedia) |
| POST | /beds/{bedId}/cleaning-attempts | Mulai pekerjaan | InpatientBedManagement : StartCleaning | StartBedCleaningRequest | BedOperationResponse | EPIC BM-03 | Rencana (belum tersedia) |
| PATCH | /cleaning-attempts/{attemptId}/complete | Selesai fisik, menunggu verifikasi | InpatientBedManagement : CompleteCleaning | CompleteBedCleaningRequest | BedOperationResponse | EPIC BM-03 | Rencana (belum tersedia) |
| POST | /beds/{bedId}/readiness-verifications | Sahkan/tolak kesiapan | InpatientBedManagement : VerifyReadiness | VerifyBedReadinessRequest | BedOperationResponse | EPIC BM-03 | Rencana (belum tersedia) |

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /available-beds | Shared predicate + eligibility existing | InpatientBedOccupancy : Read | AvailableBedQuery existing | AvailableBedPagedResult + operational version | EPIC BM-02 | Existing; diperbarui |
| GET | /bed-board | Adapter konsisten untuk konsumen existing | InpatientBedOccupancy : Read | serviceUnitId? | BedBoardResponse + operational fields | EPIC BM-01 | Existing; diperbarui |
| GET | /transfer-context | Asal otomatis dari placement current | InpatientBedOccupancy : Read | episodeId required | BedTransferContextResponse | EPIC BM-05 | Rencana (belum tersedia) |
| POST | /reservations | Reserve TTL existing | InpatientBedOccupancy : Create | ReserveBedRequest + ExpectedBedVersion | BedReservationResponse + OperationMeta | EPIC BM-02 | Existing; diperbarui |
| PATCH | /reservations/{id}/cancel | Cancel beralasan persisten | InpatientBedOccupancy : Update | CancelReservationRequest + Reason wajib + ExpectedBedVersion | BedReservationResponse + OperationMeta | EPIC BM-02 | Existing; diperbarui |
| POST | /placements | Tempatkan dan snapshot awal | InpatientBedOccupancy : Create | PlacePatientRequest + ExpectedBedVersion | BedPlacementResponse + OperationMeta | EPIC BM-02 | Existing; diperbarui |
| POST | /placements/transfer | Transfer atomik satu langkah | InpatientBedOccupancy : Transfer | TransferPatientRequest diperluas | BedPlacementResponse + OperationMeta | EPIC BM-05 | Existing; diperbarui |
| POST | /placements/{placementId}/corrections | Koreksi versioned existing | InpatientBedOccupancy : Correct | CorrectPlacementRequest existing + AffectedBedVersions | BedPlacementResponse + OperationMeta | EPIC BM-06 | Existing; diperbarui |
| GET | /placements/by-episode/{episodeId} | Histori episode existing | InpatientBedOccupancy : Read | episodeId | List<BedPlacementResponse> + snapshot/category | EPIC BM-06 | Existing; diperbarui |

#### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /{id} | Master detail + versi operasional (tanpa pasien) | Bed : Read | id | Bed detail existing + OperationalVersion/CycleId | EPIC BM-04 | Existing; diperbarui |
| POST | / | Master bed baru; kesiapan Unverified | Bed : Create | CreateBedRequest existing; OperationReason | BedCreateResponse existing + OperationalVersion/OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| PUT | /{id} | Ubah master melalui guard penuh | Bed : Update | UpdateBedRequest + ExpectedBedVersion/OperationReason | BedUpdateResponse + OperationMeta | EPIC BM-04 | Existing; diperbarui |
| PATCH | /{id}/status | Status administratif; bukan override kesiapan | Bed : Update | UpdateBedStatusRequest + ExpectedBedVersion/OperationReason | BedUpdateResponse + OperationMeta | EPIC BM-04 | Existing; diperbarui |
| PATCH | /{id}/availability | Close/reopen beralasan | Bed : Update | UpdateBedAvailabilityRequest + ExpectedBedVersion/OperationReason | BedUpdateResponse + OperationMeta | EPIC BM-04 | Existing; diperbarui |
| DELETE | /{id} | Soft-delete master tidak melepas holder | Bed : Delete | ExpectedBedVersion/OperationReason (body target) | Envelope existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| POST | /{episodeId}/record-departure | Kepergian fisik; used bed menunggu bersih | InpatientDischarge : RecordDeparture | RecordDepartureRequest existing + ExpectedPlacementId/ExpectedBedVersion | InpatientDepartureResponse + OperationMeta | EPIC BM-02 | Existing; diperbarui |

#### Health Services / Inpatient Management / Inpatient Report

Base URL: `api/v1/health-services/inpatient-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /room-transfers | Laporan transfer existing | InpatientReport : ReadRoomTransfer | RoomTransferReportQuery existing | PagedResult<RoomTransferReportRow> | EPIC BM-06 | Existing / Reuse |
| GET | /room-transfers/export | Export transfer existing, batas existing 31 hari | InpatientReport : ExportRoomTransfer | Query existing | File xlsx existing | EPIC BM-06 | Existing / Reuse |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /{id} | Detail + versi bed terdampak untuk mutation | Room : Read | id | Detail existing + AffectedBedVersions:[{BedId,ExpectedVersion}] | EPIC BM-04 | Existing; diperbarui |
| PUT | /{id} | Guard hierarchy yang mengubah availability/class bed | Room : Update | UpdateRoomRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| PATCH | /{id}/status | Aktivasi/nonaktif hierarchy terkoordinasi | Room : Update | UpdateRoomStatusRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| DELETE | /{id} | Soft-delete hierarchy tanpa melepas holder | Room : Delete | DeleteRoomRequest existing + hierarchy mutation extension | Envelope existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |

#### Health Services / Master Data / Service Unit

Base URL: `api/v1/health-services/master-data/service-units`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /{id} | Detail + versi bed terdampak untuk mutation | ServiceUnit : Read | id | Detail existing + AffectedBedVersions:[{BedId,ExpectedVersion}] | EPIC BM-04 | Existing; diperbarui |
| PUT | /{id} | Guard hierarchy yang mengubah availability/class bed | ServiceUnit : Update | UpdateServiceUnitRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| PATCH | /{id}/status | Aktivasi/nonaktif hierarchy terkoordinasi | ServiceUnit : Update | UpdateServiceUnitStatusRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| DELETE | /{id} | Soft-delete hierarchy tanpa melepas holder | ServiceUnit : Delete | DeleteServiceUnitRequest existing + hierarchy mutation extension | Envelope existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |

#### Health Services / Master Data / Patient Class

Base URL: `api/v1/health-services/master-data/patient-classes`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /{id} | Detail + versi bed terdampak untuk mutation | PatientClass : Read | id | Detail existing + AffectedBedVersions:[{BedId,ExpectedVersion}] | EPIC BM-04 | Existing; diperbarui |
| PUT | /{id} | Guard hierarchy yang mengubah availability/class bed | PatientClass : Update | UpdatePatientClassRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| PATCH | /{id}/status | Aktivasi/nonaktif hierarchy terkoordinasi | PatientClass : Update | UpdatePatientClassStatusRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| DELETE | /{id} | Soft-delete hierarchy tanpa melepas holder | PatientClass : Delete | DeletePatientClassRequest existing + hierarchy mutation extension | Envelope existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |

### 26.14 Matriks kewenangan

| Pelaku | Resource/action exact | Guard | Identity |
| --- | --- | --- | --- |
| Admisi berwenang | InpatientBedManagement : Read; InpatientBedOccupancy : Read/Create/Update; InpatientBedOccupancy : Transfer hanya jika diberikan existing | Scope semua unit/bed terlibat dan episode; bukan hak otomatis | Hanya jika existing patient/episode access sah |
| Perawat ruangan berwenang | InpatientBedManagement : Read; InpatientBedOccupancy : Read/Create/Transfer sesuai permission existing; InpatientDischarge : RecordDeparture sesuai existing | Penugasan unit nyata; tidak otomatis menjadi verifier | Sesuai episode rights, tanpa clinical fields baru |
| Perawat verifikator yang ditunjuk | InpatientBedManagement : Read/VerifyReadiness | IsReadinessVerifier + referensi penunjukan/SOP sah dan unit bed; akun HK OperationalOnly dilarang verify | Aksi readiness tidak membutuhkan identitas pasien |
| Housekeeping individu | InpatientBedManagement : Read/StartCleaning/CompleteCleaning | OperationalOnly wajib; unit assignment actual; start/complete state guards | Tidak menerima patient/episode/reservation ID, diagnosis, riwayat lintas pasien |
| Pembaca usage history berwenang | InpatientBedManagement : Read/ReadUsageHistory | Bukan OperationalOnly; scope bed/unit; identity per-row episode permission | Identity masked jika existing episode/patient right tidak terbukti |
| Supervisor/Admisi correction authorized | InpatientBedOccupancy : Correct existing; ReadUsageHistory bila diberikan | Billing OPEN, reason+version existing, scope semua affected bed/episode | Sesuai existing correction rights |
| Admin MasterData berwenang | Bed : Create/Read/Update/Delete existing | Ownership seluruh tim tidak otomatis grant semua akun; state guards berlaku semua writer | Master bed tidak memberi patient access |
| Pemilik laporan transfer existing | InpatientReport : ReadRoomTransfer/ExportRoomTransfer existing | Guard report dan patient access existing tetap | Tidak memperluas rights dari route report ke history baru |

Tabel turunan permission11; slash daftar literal actions, bukan permission baru. OperationalOnly/HK dilarang patient identity/history/verify meski punya broad permission; scope unknown deny, tidak menyimpulkan akun actual sudah diberi hak. New resource actions register saja, grant menunggu BM-G03.

### 26.15 Batas integrasi dan billing

Bed Management MUST NOT membuat pasien/master kelas kedua, tarif/ledger/folio/invoice atau engine room charge. Grade manual tidak menentukan harga. Transfer/correction memakai existing notification/outbox dan Billing requery timeline canonical1.1.0; SupersededByCorrectionId membedakan correction dari ordinary transfer. Callback/handover sesudah commit tidak membalik transfer. Physical departure tidak ditambah gate kasir baru.

### 26.16 Guardrail regulasi

MVP menerapkan batas privasi/audit yang sudah dipilih produk: least privilege, masking server, HK tanpa identitas/diagnosis/history, actor/waktu/reason dan versi koreksi. Desain ini tidak menetapkan regulasi atau retensi hukum baru; kebijakan rekam medis/retensi existing dan review pemilik privacy/hukum tetap harus dibuktikan BM-G03. SOP pembersihan/PPI/readiness/downtime tetap BM-G02, bukan checklist medis dari AI.

### 26.17 Kebutuhan non-fungsional

| ID | Kebutuhan | Hasil teruji | Bukti target |
| --- | --- | --- | --- |
| NFR-701 | Atomicity/concurrency | Bed locks semua writer, dua sumber holder direcheck, full rollback audit/receipt/outbox | BM-AT-419/414 PostgreSQL |
| NFR-702 | Authorization/privacy | Scope server, operational HK JSON tanpa PHI, unknown deny, tidak grant otomatis | BM-AT-416/417/425 |
| NFR-703 | Audit/correction | Actor individu, occurred/recorded UTC, before/after/reason, immutable snapshot+versions | BM-AT-406/411/415/418 |
| NFR-704 | Freshness/idempotency | Expected versions/cycle dan receipt key stable; timeout diperiksa, no optimistic/offline overwrite | BM-AT-420/421/422 |
| NFR-705 | Waktu/query | Server expiry parameter120m; UTC; history overlap interval stable paging max100, no timer ready | BM-AT-413/415 |
| NFR-706 | Migration/compatibility | Fail-closed backfill; coordinated writers/clients cutover; rollback menjaga guards/history | Acceptance22.4 + migration dry run BM-G04 |

Belum menetapkan SLA numerik response/cleaning atau jadwal retensi baru. Paging/bounded query dan no N+1 dalam projection diverifikasi saat delivery sesuai engineering contract.

### 26.18 Skenario UAT

Setiap epic MUST mempunyai satu berhasil dan satu gagal. Data contoh/akun/proof fixture samaran; pelaksanaan rumah sakit menunggu actual gates. Semua hasil **NOT_RUN**.

| ID | Epic | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- | --- |
| UAT-701 | EPIC BM-01 | Berhasil | Fixture dua unit dengan enam status; actor berhak satu unit | Buka menu dan Monitoring, filter unit/kamar | Satu leaf, tiga tab, enam counts sesuai bed dalam scope; HK tidak menerima identitas |
| UAT-702 | EPIC BM-01 | Gagal | Actor hanya baca unit A; respons lama ditunda | Deep-link unit B/Transfer tanpa hak; buka confirm selama refetch | 403/akses ditolak tanpa data bocor; confirm pending disabled, no stale result |
| UAT-703 | EPIC BM-02 | Berhasil | Bed A Ready, episode sah; unused reservation lalu patient ditempatkan | Reserve120m/cancel beralasan; place; record departure | Cancel reason tersimpan tanpa dirty baru; sesudah used release Waiting, no immediate reserve |
| UAT-704 | EPIC BM-02 | Gagal | Dua petugas/sessions bersaing pada bed Ready | Reserve episode A versus place episode B; simulasi timeout lalu retry same key | Satu holder sah; unknown diperiksa, retry tidak menggandakan; old closure tidak lepas new patient |
| UAT-705 | EPIC BM-03 | Berhasil | Bed Waiting; HK dan perawat fixture punya proof assignment/SOP | HK mulai/selesai; perawat inspeksi dan sahkan | Selesai menunggu verifikasi; baru Ready setelah pengesahan, actor/time traced |
| UAT-706 | EPIC BM-03 | Gagal | Bed AwaitingVerification atau siklus sudah baru | HK coba sahkan; verifier reject tanpa alasan; attempt lama coba verify | HK403; blankreason reject; stalecycle409; rejection valid menjaga jejak dan kembali Waiting |
| UAT-707 | EPIC BM-04 | Berhasil | Bed kosong tanpa reservation | Admin close alasan, lalu reopen | Close Unavailable; reopen belum Available sampai readiness sah |
| UAT-708 | EPIC BM-04 | Gagal | Bed terisi atau reserved; master data raw konflik | Coba nonactive/status/PUT/delete/hierarchy availability writer | 409 no closure; pasien aktif tetap terlihat+flag; invalid tidak bookable |
| UAT-709 | EPIC BM-05 | Berhasil | Source current, destination Ready, official class proof fixture | Asal otomatis; manual Same/Up/Down sesuai fixture; confirm refreshed | Satu transfer commit, kategori+snapshot, asal Waiting, tujuan Occupied; handover aftercommit |
| UAT-710 | EPIC BM-05 | Gagal | Source current, tujuan diambil actor lain atau order proof tidak ada | Pilih wrongcategory/stale target/default0 bedaID; inject DB failure | 409/422, source/dest/history utuh; tidak infer harga/nama, rollback lengkap |
| UAT-711 | EPIC BM-06 | Berhasil | Satu episode tanpa transfer, satu transfer dengan correction valid | History bed/periode, buka versions; master rename kemudian refresh | Semua overlap segments termasuk initial/ongoing/correction; snapshot lama tetap; Billing flag tepat |
| UAT-712 | EPIC BM-06 | Gagal | HK OperationalOnly / actor noepisode rights; Billing CLOSED | Coba history/directAPI identity; coba koreksi/delete committed transfer | HK403; identity masked bagi viewer tanpa right; closed correction rejected; no history delete |

Seluruh31AC memiliki BM-AT-396..426 pada testing22, termasuk PostgreSQL interleaving, stale cycle, privacy JSON, old episode closure dan migration dry run.

### 26.19 Definition of Done

| Butir | Jawaban | Bukti |
| --- | --- | --- |
| Keputusan produk ditutup dan bounded gate siap desain | ya | Decision46, gate1.12/BM-RCG-20261010-01 |
| Blueprint draft lengkap dan traced ke31AC | ya | Arsitektur14/FE15/data21/5contracts/testing22/flows09..14; pemeriksaan dokumen |
| Desain target disetujui manusia | belum | approved_by/at amandemen null pada manifest11 |
| Master actual/order global valid | belum | BM-G01 proof + UAT-709/710 |
| SOP/shift HK/verifier/downtime sah | belum | BM-G02 proof + UAT-705/706 |
| Permission/scopes/response privacy actual sah | belum | BM-G03 proof + UAT-701/702/712 |
| Migrations, backfill dan coordinated cutover teruji | belum | BM-G04 PostgreSQL/migration/cutover runbook |
| Satu siklus reservasi→hunian→release→cleaning→ready berjalan | belum | UAT-703/705 dan API/PG results |
| No double holder/dirty reserve/duplicate retry/old closure release | belum | BM-AT-407/419/421/426 PostgreSQL |
| Semua epic positive/negative UAT diterima | belum | UAT-701..712 signed results |
| Billing/handover/corrections regression lulus | belum | BM-AT-414/415/418/422 dan receiver tests canonical |

### 26.20 Urutan pengiriman dan bukti yang masih terbuka

| Gelombang | Epic | Isi | Syarat mulai/akhir |
| --- | --- | --- | --- |
| MVP-0 | BM-01..06 fondasi bersama | Approve desain, additive schema/config/guard proof adapters, data dry run dan writer inventory | Blueprint approval; fixture aman. Belum production activation |
| MVP-1 | BM-01/02/04 | Shared projection/availability + all write guards/idempotency, reserve/place/release/closure + FE coordinated consumers | MVP-0; tidak mixed old writers; semua bed tetap fail-closed sampai ready |
| MVP-2 | BM-03 | HK/verifier cycle, attempt audit dan child UI | MVP-1; aktivasi memerlukan BM-G02/03; no autoReady bypass |
| MVP-3 | BM-05/06 | Transfer grade+snapshot dan per-bed history/correction adapter | MVP-1; comparison class proof BM-G01 dan history rights BM-G03; integrasi Billing regression |
| MVP-4 | BM-01..06 | End-to-end PG/API/privacy/migration/UAT proof dan readiness review | Semua epic lengkap, BM-G01..04 terkait lulus sebelum operasi bergantung dinyalakan |
| POST-MVP | Di luar enam epic | Kemampuan pada26.8 hanya jika scope baru disetujui | Tidak masuk rilis pertama; tidak memberi otorisasi task baru |

| Bukti terbuka, bukan pertanyaan produk ulang | Pemilik bukti | Memblokir development lock? | Memblokir aktivasi |
| --- | --- | --- | --- |
| BM-G01 makna/arah/isi order kelas | MasterData seluruh tim + BA | Tidak untuk adapter fail-closed; data actual bukan pilihan AI | Ya, comparison lintas kelas terkait |
| BM-G02 SOP/pemeriksaan/downtime/shift assignment | HK/keperawatan/PPI; nama actual belum terbukti | Tidak untuk workflow software bounded; SOP tidak dikarang | Ya, cleaning/readiness/downtime terkait |
| BM-G03 grant/scope/accounts/privacy approval | Admin akses + pemilik unit; privacy owner OPEN | Tidak untuk implementasi deny-by-default; tidak grant otomatis | Ya, seluruh hak/identity terkait |
| BM-G04 repair F01–07 dan runtime proof | Tim implementasi + penguji + pemilik lingkungan | Merupakan acceptance delivery; bukan product OPEN DECISION | Ya, klaim siap runtime |
| Persetujuan desain draft | Pemilik desain manusia | Ya, sebelum plan-module-delivery mengunci task target | Ya, bukan approval yang dapat diberikan AI |

Tidak ada pertanyaan pilihan produk tersisa atau epic OPEN DECISION. Jika proof kemudian bertentangan dengan state/authority/data yang telah dipilih, reassess slice terdampak. Gelombang bukan task/tanggal; roadmap dibuat oleh plan-module-delivery setelah approval desain. Source aplikasi, database, deploy dan Git mutations belum menjadi wewenang fase ini.
