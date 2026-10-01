# Finance Management — Blueprint Manifest

```yaml
blueprint_id: FIN-BP-001
module_name: Finance Management
module_slug: finance-management
module_prefix: Fin
module_area: Areas/Corporate/FinanceManagement
revision: 14
revision_14_note: >
  Revisi 14 (1 Oktober 2026) adalah /design-business-module yang menggambar DUA PULUH TUJUH keputusan
  closure pass hari yang sama: FIN-DEC-111..137. Keputusan arsitekturnya FIN-DES-078..091 —
  SELURUHNYA `draft`, BELUM disetujui owner.
  JALANNYA SAMPAI KE SINI: /grill-me menjawab balasan Accounting evidence/16 (lima pertanyaan balik
  16.1-16.5), lalu DUA impact scan terarah (01-existing-capability-map.md bagian 18 dan 19) yang
  menemukan celah LEBIH BESAR daripada pertanyaan Accounting sendiri.
  TEMUAN YANG MENGUBAH SIFAT SELURUH AMANDEMEN, dan ia memalukan untuk dicatat tetapi MUST dicatat:
  FINANCE BELUM PERNAH MENGIRIM APA PUN KE ACCOUNTING. Program.cs mendaftarkan sebelas hosted service
  dan tidak satu pun milik Finance; EPIC FIN-12 (worker pengiriman) ditunda atas keputusan owner sendiri
  ("karena system saya belum perlu itu"). Akibatnya SELURUH baris FinAccountingEventOutbox menumpuk
  PENDING selamanya, dan janji "G4 siap" beserta "snapshot otomatis tanggal 1 pukul 00.05 WIB" pada
  evidence/15 dan evidence/21 TIDAK PUNYA KODE sama sekali. Gerbang FIN-DES-059 yang disebut surat
  Finance 16 pun tidak punya apa-apa untuk digerbang. Dibuka kembali lewat FIN-DEC-118 sebagai syarat
  G4; digambar FIN-DES-078 sebagai TIGA hosted service terpisah yang DIBANGUN DALAM KEADAAN MATI.
  TEMUAN KEDUA, yang membatalkan kesimpulan impact scan pertama: pembayaran langsung piutang
  (FinanceReceivableService.cs:621-701) mengubah OutstandingAmount TANPA baris FinReceipt/
  FinReceiptAllocation, dan pembayaran langsung utang supplier
  (FinanceSupplierPayableService.cs:207-274) bocor dengan bentuk YANG SAMA — keduanya juga menerima
  bankAccountId/paymentMethod/notes lalu MEMBUANGNYA. Rumus "asli − alokasi − penyesuaian − penghapusan"
  yang dinyatakan layak pada bagian 18.3 karena itu DICABUT pada bagian 19, dan digantikan TIGA BUKU
  MUTASI (FIN-DEC-123, FIN-DES-079).
  DAMPAK SKEMA: DELAPAN tabel baru (FinReceivableMovement, FinSupplierPayableMovement, FinCashMovement,
  FinSubledgerControlAccountMap, FinOpeningBalance, FinOpeningItemBatch, FinTransactionProof,
  MstDirectPaymentThreshold) dan DUA tabel berjalan diperbarui. EMPAT migration. Yang keempat
  (AddFinanceOpeningItemMigration) adalah satu-satunya yang menyentuh tabel berjalan: tiga kolom
  FinReceivable asal Billing menjadi NULLABLE BERSYARAT beserta check constraint baru dan satu index
  diganti filternya — dicatat lengkap beserta langkah NOT VALID/VALIDATE dan CREATE INDEX CONCURRENTLY,
  termasuk PENGAKUAN bahwa urutan DROP-lalu-CREATE pada DDL membuka jendela ketika idempotensi intake
  Billing tidak dijaga index, dan karena itu urutan yang lebih aman SHOULD dipakai.
  DAMPAK RUNTIME: TIGA hosted service, yang PERTAMA bagi modul Finance. Dibangun mati secara bawaan,
  dengan DUA LAPIS gerbang — satu konfigurasi untuk seluruh pengiriman, satu daftar kode yang tetap
  dilewati walaupun pengiriman sudah hidup (mempertahankan FIN-DES-059 apa adanya).
  DAMPAK HAK AKSES: EMPAT resource baru, dan satu action baru `Approve` pada resource baru.
  Dijelaskan kenapa itu TIDAK bertentangan dengan FIN-DEC-098/103 yang justru menolak jenjang approval:
  keduanya menyangkut transaksi harian, sedangkan Approve di sini menyangkut PEMBUKAAN BUKU yang tidak
  dapat ditarik. NOL ketergantungan pada FIN-OQ-039.
  NOL FOLDER SUBMODUL BARU, dan karena itu NOL gerbang QBE-MOD-003 — berbeda dari revisi 4. Ketujuh
  submodule Finance sudah terdaftar prefix `Fin`, dan MasterData tercakup baris `Mst`.
  TIGA KOREKSI ATAS KEPUTUSAN YANG BARU DIAMBIL HARI ITU, dicatat apa adanya:
    (a) FIN-DEC-119 (Kas Kasir dari rekap harian) SUPERSEDED oleh FIN-DEC-125 — premisnya keliru,
        rekap harian ternyata menghitung kas dari shift Billing TANPA memeriksa statusnya;
    (b) rumus FIN-DEC-127 SUPERSEDED oleh FIN-DEC-132 — rumus Kas Kasir yang diambil pagi itu LUPA
        memuat pengeluaran kas, padahal rekap harian memuatnya dan FinPayment sudah mendukung CASH;
    (c) FIN-DEC-117 (Medical Fee mengirim saldo honor dokter) SUPERSEDED oleh FIN-DEC-122 —
        FinMedicalServicePayable ternyata milik Finance dan modul Medical Fee belum ada.
  SATU CONTOH KEPUTUSAN DIKOREKSI TANPA MENCABUT KEPUTUSANNYA: contoh FIN-DEC-112 (piutang lebih bayar
  menjadi negatif) terbukti TIDAK DAPAT TERJADI — CK_FinReceivable_Outstanding menjaganya >= 0 dan
  alokasi melebihi sisa ditolak. Aturan "kirim negatif apa adanya" tetap diambil karena benar secara
  prinsip, dengan Kas Kasir sebagai satu-satunya kandidat negatif yang diketahui. Dicatat di kontrak
  integrasi sebagai kejujuran, bukan dihapus diam-diam.
  EMPAT EPIC BARU: FIN-20 (buku mutasi dan posisi per tanggal, MVP-14A), FIN-21 (pemetaan akun control
  dan saldo awal, MVP-14B), FIN-22 (jalur pengiriman, MVP-14C). DUA berstatus OPEN DECISION dan karena
  itu DI LUAR SELURUH GELOMBANG: FIN-23 (pembayaran langsung berkontrol — tertahan FIN-OQ-075, aturan
  berkas bukti, karena FIN-DEC-126 membuat bukti WAJIB) dan FIN-24 (migrasi tagihan lama — tertahan
  FIN-OQ-077, paket pembaca spreadsheet yang belum ada di proyek dan menuntut wewenang eksplisit
  menurut AGENTS.md). Keduanya membawa PERLAKUAN SEMENTARA yang eksplisit supaya petugas tidak
  kehilangan pekerjaan yang selama ini dapat dilakukan, beserta biaya penundaannya: selisih tetap
  Finance terhadap buku besar sebesar saldo awal piutang dan utang, yang MUST dijelaskan Accounting
  secara manual setiap bulan sampai FIN-24 jalan.
  DUA NILAI SENGAJA DIBIARKAN KOSONG, tidak dikarang: FIN-OQ-074 (angka ambang) dan FIN-OQ-076 (jumlah
  tagihan lama). Keduanya data konfigurasi. Tanpa baris ambang aktif, seluruh pembayaran langsung
  DITOLAK fail-closed — perilaku yang disengaja, bukan kelalaian.
  PERUBAHAN MEMUTUS: DUA endpoint yang sudah berjalan (POST /receivables/{id}/payment dan
  POST /supplier-payables/{id}/direct-payment). Urutan rilisnya MUST dijaga — layar disesuaikan sebelum
  atau bersamaan dengan backend, tidak sesudahnya.
  SATU PENEMPATAN MENU TIDAK DIPUTUSKAN SENDIRI: tujuh layar baru tidak ada di V1, sedangkan FIN-DEC-094
  mengikat menu pada bentuk V1. Dicatat FIN-OQ-079, BUKAN DEV_DISCRETION.
  NOL source aplikasi disentuh. NOL migration dibuat. NOL package ditambahkan.
revision_13_note: >
  Revisi 13 (1 Oktober 2026) adalah /grill-me Amendment pass yang MEMBALIK FIN-DEC-060 (SUPERSEDED).
  Pemicu: owner membandingkan menu Keuangan sistem produksi V1 (QuilvianSystemFrontendDev1/
  QuilvianSystemBackendDev1 — repository terpisah, sudah berjalan produksi dan UAT-approved) dengan
  menu finance governed lewat tangkapan layar langsung. LIMA KEPUTUSAN BARU: FIN-DEC-094..098,
  seluruhnya `approved`.
  FIN-DEC-094 (supersedes FIN-DEC-060): seluruh menu Transaksi A/R dan Transaksi A/P MUST mengikuti
  bentuk dan penempatan halaman persis V1 — submenu "Pembelian" DICABUT, kapabilitas yang sebelumnya
  dikonsolidasikan (Canceled Invoice, Pemutihan Piutang, Piutang Korporat, Receiveable AR Canceled,
  laporan AR per jenis, Penerima Pesanan/GR) DIPECAH kembali jadi halaman berdiri sendiri.
  FIN-DEC-095: Manajemen Klaim dibatasi SISI KEUANGAN SAJA — verifikasi dokumen klaim tetap milik
  Billing/Casemix, Finance hanya melacak status pelunasan.
  FIN-DEC-096: Ayat Silang TIDAK butuh kapabilitas baru — sudah tercakup alokasi penerimaan yang ada
  (FE-FIN-004/BE-FIN-016..018). Menutup gap MISSING yang tercatat di 01-existing-capability-map.md
  sejak audit awal proyek.
  FIN-DEC-097: Manajemen Klaim adalah PERLUASAN FinReceivableInvoiceBatch (BE-FIN-038/039,
  FE-FIN-012) — bukan entity baru. Lima status: Diajukan→Diverifikasi Payer→Disetujui(sebagian/
  penuh)→Dibayar(sebagian/lunas)→Ditutup. Selisih nominal saat disetujui lebih kecil dicatat sebagai
  item terpisah untuk write-off manual, TIDAK otomatis disesuaikan.
  FIN-DEC-098: Perubahan status klaim oleh staf AR biasa, manual, TANPA jenjang approval tambahan.
  BENTUK BLUEPRINT TETAP SINGLE: kedua kapabilitas tidak memenuhi syarat pemisahan sub-modul (Ayat
  Silang bukan kapabilitas berdiri sendiri; Manajemen Klaim adalah field/status di atas aggregate
  yang sudah ada).
  NOL source disentuh pada pass /grill-me itu sendiri.
  PASS DESAIN MENYUSUL DI REVISI YANG SAMA. Revisi 13 memuat DUA pass berurutan pada hari yang sama,
  satu benang amandemen: (a) /grill-me yang menurunkan FIN-DEC-094..098, lalu (b)
  /design-business-module yang menggambarnya. Keduanya disatukan dalam satu revisi karena memang satu
  perubahan produk — penyelarasan navigasi ke V1 beserta pelacakan klaim penjamin yang lahir darinya.
  HASIL PASS DESAIN: FIN-DES-070..073 (`draft`).
    FIN-DES-070 — status klaim adalah SUMBU KEDUA (kolom ClaimStatus baru), bukan perluasan enum
                  Status yang ada. Alasannya dibuktikan dari source: ruas "Dibayar Sebagian/Lunas"
                  pada rantai yang disebut owner sudah dipegang kolom Status dan penulisnya SISTEM,
                  sedangkan ruas jawaban penjamin penulisnya PETUGAS. Menggabungkan keduanya membuat
                  keadaan "penjamin sudah setuju tetapi uang belum masuk" tidak dapat diwakili, dan
                  membatalkan invariant FIN-DES-041 (status batch bukan sumber kebenaran pelunasan).
    FIN-DES-071 — selisih nominal yang tidak disetujui penjamin DIHITUNG pada response, tidak
                  disimpan, dan TIDAK PERNAH mengurangi OutstandingAmount sendiri. Penghapusan tetap
                  lewat maker-checker write-off yang sudah ada.
    FIN-DES-072 — NOL resource dan NOL action hak akses baru; karena itu amendment ini TIDAK
                  bergantung pada FIN-OQ-039 yang masih menunggu Security Owner.
    FIN-DES-073 — belasan layar hasil pemecahan adalah PANDANGAN TERSARING atas endpoint yang sudah
                  ada, bukan satu endpoint per layar.
  DAMPAK SKEMA: satu tabel Diperbarui (FinReceivableInvoiceBatch, tujuh kolom nullable + satu check
  constraint + satu index), satu migration AddClaimTrackingToFinReceivableInvoiceBatch, dapat
  dijalankan tanpa downtime, tanpa backfill. NOL tabel baru.
  ENDPOINT BARU: lima, seluruhnya Rencana (belum tersedia) — tiga aksi klaim, dua permukaan baca
  (GET /receivables/write-offs dan GET /receipts/reversed-allocations) yang ternyata memang belum ada.
  FIN-OQ-040 DIBUKA DAN DITUTUP PADA REVISI YANG SAMA, dan jawabannya membuka gerbang yang lebih besar.
  Isi grup "Umur Piutang (A/R Aging)" ternyata: Kasir, Parkir, Tenant. Pemeriksaan source sebelum
  merancang menemukan Parkir dan Tenant TIDAK DAPAT DIWAKILI model piutang yang berjalan — FinReceivable
  hanya mengenal PAYER/PATIENT_GUARANTOR/EMPLOYEE_BENEFIT dan setiap barisnya WAJIB berasal dari serah
  terima tagihan Billing (SourceHandoffKey, InvoiceId). Sewa parkir dan sewa unit tenant adalah
  penagihan berulang atas kontrak sewa: bukan tagihan pasien, tidak lahir dari Billing, dan menuntut
  data induk yang belum dimiliki modul mana pun. Layar V1-nya pun murni generateDummyData(), nol
  panggilan API — sehingga tidak ada aturan bisnis yang dapat dirujuk, persis seperti Manajemen Klaim.
  FIN-OQ-043 DIBUKA sebagai gantinya, MEMBLOKIR dua butir menu saja (Umur Piutang Parkir dan Tenant),
  dan dikeluarkan dari seluruh gelombang pengiriman. Butir Kasir berjalan terus — ia hanya menuntut
  satu saringan segmen pada endpoint umur piutang yang sudah ada.
  DUA PERTANYAAN TERBUKA LAIN: FIN-OQ-041
  (empat pasang butir V1 yang tampak kembar), FIN-OQ-042 (kolom PayerClaimReference adalah kesimpulan
  desain, bukan permintaan owner). Tidak ada yang memblokir EPIC FIN-18 secara keseluruhan.
  STRUKTUR: berkas flowcharts/klaim-penjamin.md dibuat mengikuti kontrak keluaran yang berlaku
  sekarang. Diagram lama blueprint ini tinggal di erd/ karena dibangun sebelum folder flowcharts/
  menjadi bagian kontrak; pemindahannya pekerjaan tersendiri, bukan efek samping amandemen ini.
  PASS KETIGA PADA REVISI YANG SAMA — penutupan FIN-OQ-043 dan desainnya. /grill-me menurunkan
  FIN-DEC-099..104, lalu /design-business-module menggambarnya sebagai bagian K.
  FIN-DEC-099 (Finance memiliki penagihan sewa sepenuhnya), FIN-DEC-100 (dicatat manual tiap periode,
  TANPA master kontrak sewa — risiko periode terlewat diterima sadar), FIN-DEC-101 (entity TERSENDIRI;
  invariant FinReceivable "wajib dari Billing" TIDAK dilonggarkan dan tidak mendapat pengecualian),
  FIN-DEC-102 (denda nominal manual, bukan dihitung sistem), FIN-DEC-103 (staf AR penuh TANPA jenjang
  approval untuk pencatatan MAUPUN penghapusan — berbeda sadar dari maker-checker piutang pasien),
  FIN-DEC-104 (satu entity dengan kolom Category PARKING/TENANT).
  HASIL DESAIN: FIN-DES-074..077 (`draft`). Dua tabel BARU (FinNonPatientReceivable,
  FinNonPatientReceivableSettlement), satu migration AddFinNonPatientReceivable, satu resource hak
  akses baru FinanceNonPatientReceivable dengan tiga action (Read/Create/Update) yang terdaftar lewat
  pemindaian atribut biasa — nol ketergantungan pada FIN-OQ-039. Definisi kelompok umur piutang
  (ReceivableAgingBuckets) DIPAKAI ULANG supaya kedua laporan umur piutang dapat dibandingkan.
  SATU BATAS YANG DICATAT TERBUKA, BUKAN DISEMBUNYIKAN: FinReceipt lahir dari intake tender Billing
  dan tidak punya jalur manual, sehingga pelunasan sewa dicatat pada jalurnya sendiri. Akibatnya uang
  sewa yang diterima TIDAK muncul di kas harian, TIDAK muncul di setoran bank, dan TIDAK menerbitkan
  kejadian akuntansi apa pun pada rilis pertama. Ini konsekuensi sah dari memisahkan jalur, bukan
  cacat yang ditutupi — dan menjadi FIN-OQ-044.
  FIN-OQ-044 DIBUKA: (a) apakah pelunasan sewa masuk kas harian/setoran bank dan lewat jalur apa;
  (b) kode kejadian akuntansi untuk pendapatan sewa — MENUNTUT RATIFIKASI ACCOUNTING, bukan wewenang
  Finance sepihak; (c) apakah rilis pertama boleh berjalan tanpa keduanya. MEMBLOKIR kelengkapan
  akuntansi EPIC FIN-19, TIDAK memblokir pembangunannya (gelombang MVP-13D boleh jalan).
  FIN-OQ-043 CLOSED.
  PEMBARUAN 1 Oktober 2026 (addendum pada 00-interview-decisions.md): FIN-OQ-041 CLOSED oleh
  FIN-DEC-107 (keempat pasang butir V1 dipertahankan sebagai layar berbeda); FIN-OQ-042 CLOSED oleh
  FIN-DEC-108 (PayerClaimReference dipertahankan, opsional); FIN-OQ-044(a) CLOSED oleh FIN-DEC-109
  (sewa dikelola terpisah dari kas harian/setoran bank; task integrasi kas dicabut) dan (c) CLOSED
  oleh FIN-DEC-110 (rilis boleh berjalan dengan banner). HANYA FIN-OQ-044(b) TERBUKA — ratifikasi kode
  kejadian akuntansi pendapatan sewa oleh Rizki (Accounting).
contract_versions_revision_13_lanjutan: >
  DRAFT lanjutan untuk revisi 13 (bagian K pada 02-backend-architecture.md, bagian 18 pada
  03-frontend-architecture.md). Lima sumbu bergerak lagi di atas angka sebelumnya:
    api-contract: FIN-API-1.4 (`draft`) — bagian E.1 baru, grup endpoint Non Patient Receivable
                          (sepuluh endpoint Rencana). Aditif murni, nol endpoint lama disentuh
    state-transition-matrix: FIN-STATE-1.5 (`draft`) — bagian E.1 baru, lima status piutang sewa
                          beserta perbedaan yang disengaja dari piutang pasien
    validation-matrix: FIN-VAL-1.6 (`draft`) — FIN-VAL-154..164 baru, beserta daftar eksplisit hal
                          yang sengaja TIDAK divalidasi akibat ketiadaan master kontrak
    permission-audit-matrix: FIN-PERM-1.6 (`draft`) — bagian F.1-F.4 baru. Satu resource dan tiga
                          action baru, serta catatan terbuka tentang kewenangan yang TIDAK dijaga
                          mesin hak akses akibat FIN-DEC-103
    acceptance-test-matrix: FIN-TEST-1.7 (`draft`) — bagian H.1 baru, termasuk skenario pemeriksaan
                          batas yang membuktikan uang sewa belum masuk kas
  TIDAK bergerak: integration-contract — justru karena belum ada kode kejadian yang disepakati untuk
  pendapatan sewa. Menambahkannya sepihak melanggar pola ratifikasi yang berlaku sejak FIN-DEC-053.
  Langkah berikutnya: /plan-module-delivery memecah EPIC FIN-18 dan EPIC FIN-19 menjadi task
  FE-FIN-xxx/BE-FIN-xxx bernomor, SESUDAH owner menyetujui desain ini. Approval tetap tindakan manusia.
contract_versions_revision_13: >
  DRAFT untuk revisi 13 (AMENDMENT REVISI 13 pada 02-backend-architecture.md dan bagian 17 pada
  03-frontend-architecture.md). Lima sumbu bergerak:
    api-contract: FIN-API-1.3 (`draft`) — bagian D.1-D.3 baru. Lima endpoint Rencana, dan field
                          klaim aditif pada ReceivableInvoiceBatchResponse. NOL endpoint berubah
                          bentuk, NOL endpoint dicabut
    state-transition-matrix: FIN-STATE-1.4 (`draft`) — bagian D.1 baru (sumbu klaim). Bagian B.7
                          TIDAK disentuh satu baris pun
    validation-matrix: FIN-VAL-1.5 (`draft`) — FIN-VAL-147..153 baru, beserta daftar eksplisit hal
                          yang sengaja TIDAK divalidasi
    permission-audit-matrix: FIN-PERM-1.5 (`draft`) — bagian E.1-E.3 baru. Isinya justru menegaskan
                          NOL resource dan NOL action baru, serta mencatat apa adanya kewenangan yang
                          TIDAK dijaga mesin hak akses akibat FIN-DEC-098 (tanpa jenjang approval)
    acceptance-test-matrix: FIN-TEST-1.6 (`draft`) — bagian G.1 baru, memuat jalur gagal
  TIDAK bergerak: integration-contract. Amandemen ini tidak menerbitkan kejadian akuntansi baru —
  persetujuan klaim bukan peristiwa keuangan sampai ia menjadi penghapusan piutang, dan penghapusan
  itu sudah punya kodenya sendiri.
revision_12_note: >
  Revisi 12 (29 September 2026) adalah /grill-me Amendment pass yang menutup FIN-OQ-038 — gerbang
  yang dibuka pass desain revisi 11. SATU KEPUTUSAN BARU: FIN-DEC-084, `approved` untuk SISI FINANCE.
  Jalan keluar yang dipilih: PERLUAS PLATFORM (opsi D) — platform-authorization diperluas supaya
  sebuah modul dapat mendeklarasikan resource yang dapat diberikan tetapi tidak dijaga endpoint mana
  pun, lewat deklarasi opt-in eksplisit, DENGAN penjaga anti-typo yang ada sekarang tetap
  dipertahankan untuk kasus normal.
  DUA ALTERNATIF DITOLAK, beserta alasannya: (C) controller pembawa milik Finance ditolak karena
  endpoint yang keberadaannya terutama untuk membawa resource adalah jebakan jangka panjang —
  peninjau berikutnya wajar menganggapnya endpoint mati lalu menghapusnya, dan itu diam-diam
  mematikan seluruh pemberian hak lewat payung; (E) membatalkan payung sama sekali TIDAK dipilih,
  walaupun premis aslinya gugur, karena kemudahan admin tetap dinilai bernilai.
  FAKTA YANG DISAMPAIKAN SEBELUM KEPUTUSAN DIAMBIL, dan ia mengubah bobot pertanyaannya: alasan asli
  payung dibuat (mencegah 403 pada butir menu, FIN-OQ-036) TIDAK TERBUKTI — lihat revision_11_note.
  Manfaat payung yang tersisa murni kemudahan admin. Owner tetap memilih membangunnya.
  BATAS WEWENANG YANG DICATAT APA ADANYA: FIN-DEC-084 adalah keputusan sisi Finance. Kode yang
  diubah milik platform-authorization, dan pemiliknya Security Owner bersama pemilik modul itu —
  BUKAN Yasmin. Karena itu FIN-OQ-039 DIBUKA sebagai permintaan lintas modul, mengikuti pola
  FIN-DEC-053 (ke Accounting) dan FIN-OQ-034/037 (ke Billing). Ia menahan implementasi mekanisme
  ekspansi, TIDAK menahan BE-FIN-042 yang sudah selesai maupun pekerjaan frontend mana pun.
  SATU SURAT EVIDENCE DITULIS pada pass yang sama: evidence/20 kepada Security Owner + owner
  platform-authorization. Surat itu sengaja menyampaikan DUA hal yang merugikan posisi Finance
  sendiri, supaya penerimanya dapat menimbang jujur: (a) alasan asli payung (mencegah 403) sudah
  terbukti gugur sehingga manfaat yang tersisa murni kemudahan admin, dan (b) dua alternatif yang
  dapat ditempuh Finance TANPA melibatkan modul lain, beserta alasan penolakannya. Surat itu
  menyatakan jawaban "ditolak atau ditunda" DAPAT DITERIMA SEPENUHNYA — Finance akan mencabut
  rencana payung dan tetap memakai pemberian hak granular. Satu temuan di luar permintaan ikut
  dilaporkan di bagian 8 surat itu (FIN-CQ-09, dua butir menu Report yang tersembunyi permanen).
  NOL source disentuh. NOL kontrak naik versi pada pass ini.
revision_11_note: >
  Revisi 11 (29 September 2026) adalah /design-business-module kecil yang menggambar mekanisme
  FIN-DEC-082 dan FIN-DEC-083. Keputusan arsitekturnya FIN-DES-066..069 — SELURUHNYA `draft`,
  BELUM disetujui owner. NOL tabel baru, NOL kolom baru, NOL migration, NOL endpoint baru.
  IMPACT SCAN DIJALANKAN pada area terdampak saja (jalur tulis SysAccessPolicy, registry hak akses,
  penyaring menu frontend): backend 7811c048 TIDAK bergerak; frontend 49b59cfaa -> a31da3c21
  BERGERAK, dan hasil pindaiannya MENGUBAH kesimpulan, bukan hanya memperbarui angka.
  TEMUAN YANG MEMBATALKAN PREMIS FIN-OQ-036. Butir menu yang dijaga Finance.AP/Finance.AR
  seluruhnya menunjuk rute V2 (/finance/payable*, /finance/receivable*) dengan aksi View/Payment/
  Report — aksi milik FinanceApController/FinanceArController sendiri. Menu dan endpointnya
  KONSISTEN; risiko 403 yang menjadi alasan FIN-OQ-036 dibuka TIDAK ADA di sana. Butir menu
  Purchasing belum pernah ada; ia baru dibangun FE-FIN-008..014. Dijawab FIN-DES-069: butir menu
  memakai resource GRANULAR, payung murni alat pemberian massal milik admin dan tidak pernah
  diperiksa penyaring menu.
  KENDALA MEKANIS YANG MENENTUKAN BENTUK DESAIN (FIN-DES-066). Resource hanya dapat terdaftar dari
  pemindaian endpoint; jalur penanda (AccessExplicitPermissionAttribute) secara eksplisit MENOLAK
  KERAS resource yang tidak dikenal. Artinya resource payung TANPA endpoint TIDAK DAPAT didaftarkan
  dengan mekanisme platform hari ini. Dua jalan keluar dirumuskan — (C) pembawa milik Finance, atau
  (D) perluasan platform-authorization — dan keduanya BUKAN wewenang Finance memutuskan sendiri.
  Dicatat FIN-OQ-038, menahan HANYA implementasi mekanisme ekspansi.
  TITIK TULIS DITETAPKAN (FIN-DES-067): RoleAccessController.ApplyPoliciesAsync — satu-satunya jalur
  penulisan SysAccessPolicy, dipakai bersama endpoint simpan dan salin. Ekspansi disisipkan SEBELUM
  gerbang validasi registry yang sudah ada, di dalam transaksi yang sudah dibuka method itu. NOL
  perubahan pada HasAccessAsync. Perilaku pencabutan yang diminta FIN-DEC-083 terbukti jatuh langsung
  dari semantik overwriteTarget yang sudah ada, tanpa kode tambahan.
  BENTUK PETA DITETAPKAN (FIN-DES-068): satu berkas statis; tabel database yang dapat disunting admin
  DITOLAK sebagai permukaan eskalasi hak akses.
  DUA TEMUAN DILAPORKAN, TIDAK DIPERBAIKI DI SINI: FIN-CQ-09 (butir menu "Report AR"/"Report AP"
  dijaga aksi Report yang tidak pernah dideklarasikan controller mana pun, sehingga tersembunyi
  permanen bagi semua orang termasuk SuperAdmin; endpoint /report-nya sendiri sehat, dijaga View) dan
  FIN-CQ-10 (dokumen menyebut berkas penyaring corporateFinance.js yang tidak ada).
  TIGA DOKUMEN DIKOREKSI: contracts/permission-audit-matrix.md D.2/D.6.1/D.6.2 (naik ke FIN-PERM-1.4,
  `draft`) — termasuk MENCABUT skrip SQL D.6.1 yang menyasar tabel SysRolePermissions yang tidak ada;
  dan 03-frontend-architecture.md 15.4.
revision_10_note: >
  Revisi 10 (29 September 2026) adalah /grill-me Amendment pass yang mengoreksi FIN-DEC-079
  (AMENDMENT REVISI 6/§D.5-D.6, 28 September 2026). Dipicu temuan saat implementasi BE-FIN-042:
  nama resource payung "Finance.AP"/"Finance.AR" yang didaftarkan FIN-DEC-079 sebagai resource
  BARU ternyata SUDAH DIPAKAI dua controller nyata yang berjalan (FinanceApController,
  FinanceArController — endpoint "V2" AP/AR). Closure pass FIN-CQ-08 (28 September) tidak
  menyilangkan temuannya dengan FIN-DEC-059 (25 September, decision log YANG SAMA) yang sudah
  eksplisit menyebut FinanceApController "yang sudah berjalan" — dua controller itu sama sekali
  tidak tersebut sepanjang closure pass FIN-CQ-08.
  DUA KEPUTUSAN BARU: FIN-DEC-082 (payung dipindah ke nama Finance.AP.Umbrella/Finance.AR.Umbrella,
  provisional; FinanceApController/FinanceArController TIDAK disentuh) dan FIN-DEC-083 (mekanisme
  ekspansi payung->granular MATERIALIZED saat admin memberi grant lewat layar Akses Role — pola
  be-sec-003b-policy-expansion.sql — BUKAN live/dihitung saat request, supaya algoritma otorisasi
  inti AccessPermissionService yang dipakai SELURUH modul aplikasi tidak ikut berubah).
  FIN-DEC-079 DIKOREKSI PARSIAL: nama payung berubah, cakupan payung-ke-granular dan matriks
  pewarisan aksi TETAP berlaku apa adanya.
  FIN-OQ-036 DIBUKA KEMBALI SEBAGIAN: klaim closure sebelumnya ("frontend tidak perlu diubah")
  tidak lagi benar bila payung berganti nama — filter menu corporateFinance.js MUST diverifikasi
  ulang /trace-existing-capabilities untuk memastikan ia menyaring untuk menu Purchasing/AR
  (bukan menu V2) sebelum diarahkan ke nama payung baru. 03-frontend-architecture.md §15.4
  (mengklaim FIN-CAP-040/FIN-OQ-036 tertutup tuntas) turut MUST ditinjau ulang.
  NOL tabel baru, NOL kolom baru, NOL migration, NOL endpoint baru — pass ini murni koreksi
  keputusan governance hak akses. BE-FIN-042 §D.5 (rename 6 controller) dan §D.6.1 (skrip
  migrasi SysAccessPolicy) TIDAK terdampak — keduanya sudah selesai dan berdiri sendiri dari
  koreksi ini.
revision_9_note: >
  Revisi 9 (29 September 2026) mencatat APPROVAL resmi owner (Yasmin) atas FIN-DES-064 dan
  FIN-DES-065 via FIN-DEC-080 dan FIN-DEC-081 (Amendment Pass 29 September 2026).
  Koreksi atas keputusan bisnis FIN-DEC-041 resmi disahkan: mutasi BilDepositMovement bertipe
  RELEASE dicabut dari PENGEMBALIAN-UANG-MUKA dan dipetakan ke PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT,
  sedangkan pemicu PENGEMBALIAN-UANG-MUKA dipersempit murni ke BilRefundCase EXECUTED.
  Kontrak FIN-INTEGRATION-1.6, FIN-VAL-1.5, dan FIN-TEST-1.6 resmi disetujui (approved).
  Blocker pemicu BE-FIN-047 resmi terbuka. Surat evidence FIN-OQ-037 disiapkan sebagai
  evidence/19 untuk meminta penanda eksplisit ke Owner Billing.
revision_8_note: >
  Revisi 8 (29 September 2026) adalah /design-business-module kecil yang menjawab SATU blocker yang
  dilaporkan pass perencanaan roadmap sehari sebelumnya: BE-FIN-047 tidak dapat direncanakan karena
  pemicu PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT tidak jelas.
  NOL tabel baru, NOL kolom baru, NOL migration, NOL endpoint baru. Keputusan arsitekturnya
  FIN-DES-064 dan FIN-DES-065 — SELURUHNYA `draft`, BELUM disetujui owner.
  IMPACT SCAN DIJALANKAN karena SHA bergerak cba60cb0 -> 7811c048. Hasilnya MENGUBAH kesimpulan
  pass perencanaan, bukan hanya memperbarui angka.
  TEMUAN UTAMA, dan ia lebih tajam daripada dugaan awal. Dugaan pass perencanaan: satu MovementType
  RELEASE membawa dua arti sehingga Finance perlu membedakannya. Kenyataan pada source: RELEASE
  hanya punya SATU penulis (BillingSettlementService:949, BKC-DEC-131, pembatalan alokasi LIFO), dan
  TIDAK ADA penulis RELEASE untuk pengembalian uang muka tunai sama sekali. Jadi masalahnya bukan
  ambiguitas yang perlu pembeda, melainkan PEMETAAN YANG TERTUKAR:
    (a) FIN-DES-035/FIN-DEC-041 memetakan RELEASE ke PENGEMBALIAN-UANG-MUKA (kredit Kas), padahal
        pada pembatalan alokasi TIDAK ADA kas yang bergerak — saldo deposit justru naik kembali.
        Dibangun apa adanya, buku besar mencatat kas keluar untuk uang yang masih ada di deposit,
        dan rekonsiliasi toleransi nol Accounting gagal tanpa sebab yang terlihat.
    (b) FIN-DES-057 menunggu pemicu mutasi REVERSAL atas ALLOCATION yang Billing TIDAK PERNAH tulis,
        sehingga pembatalan alokasi uang muka tidak pernah sampai ke buku besar.
  Keduanya bertemu pada fakta yang SAMA. Dikoreksi FIN-DES-064: mutasi RELEASE menerbitkan
  PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT (debit Piutang, kredit Uang Muka Pasien), dan
  PENGEMBALIAN-UANG-MUKA dipersempit ke BilRefundCase EXECUTED saja — kode itu TIDAK mati.
  DUA KOREKSI INI MENYENTUH KEPUTUSAN BISNIS YANG SUDAH `approved` (FIN-DEC-041) dan karena itu
  MUST DIAKUI OWNER sebelum dipakai — pola yang sama dengan temuan A/B/C/D pada revisi 6.
  SATU TEMUAN MILIK BILLING, dilaporkan dan TIDAK diperbaiki dari Finance: BillingDepositService:176
  menjumlahkan seluruh mutasi RELEASE sebagai totalRefunded ("dana yang dikembalikan"). Sesudah
  BKC-DEC-131, angka itu ikut memuat pembatalan alokasi, padahal uangnya masih ada di saldo deposit.
  SATU OPEN QUESTION BARU: FIN-OQ-037 — permintaan penanda eksplisit kepada owner Billing supaya
  arti RELEASE menjadi tunggal (opsi A: isi ReversesMovementId yang kolomnya SUDAH ADA; opsi B:
  MovementType tersendiri). Selama belum turun, mutasi RELEASE tanpa pasangan REVERSAL ditolak
  fail-closed sebagai baris intake ERROR (FIN-VAL-145) — bukan ditebak.
  KENAPA AMENDMENT INI MURAH: BE-FIN-025 belum dikerjakan, sehingga koreksi tiba SEBELUM kodenya
  ditulis. Nol perubahan perilaku pada kode yang sudah berjalan, dan nol baris outbox yang perlu
  dibetulkan.
revision_7_note: >
  Revisi 7 (28 September 2026) adalah /design-business-module kecil yang (a) MENCATAT APPROVAL owner
  atas FIN-DES-051..058, dan (b) menurunkan FIN-DEC-072..076 menjadi FIN-DES-059 dan FIN-DES-060.
  NOL tabel baru, NOL kolom baru, NOL migration, NOL endpoint baru.
  TIGA KOREKSI revisi 6 SUDAH DIAKUI owner (FIN-DEC-072/073/074), sehingga FIN-DES-053/054/056
  berlaku APA ADANYA — tidak ada satu pun teks desain revisi 6 yang diubah karenanya.
  YANG BENAR-BENAR BARU:
    FIN-DES-059 — gerbang worker untuk kedua kode penanda shift. Bentuk pesan TIDAK diubah
      (Amount = 0 dipertahankan); yang ditambah hanya gerbangnya, menunggu FIN-OQ-035. Owner
      MENOLAK jalan pintas nilai simbolis non-nol (FIN-DEC-075) karena angka palsu di buku besar
      lebih berbahaya daripada baris PENDING yang menunggu.
    FIN-DES-060 — menu Purchasing diselaraskan ke FIN-DEC-060 (FIN-DEC-076): keputusan yang
      DITEGAKKAN, bukan direvisi mengikuti implementasi yang mendahuluinya. Pemetaannya di
      03-frontend-architecture.md bagian 15.
  DUA KOREKSI DOKUMEN berbasis bukti audit:
    integration-contract.md 5.10.5 butir 1 — klaim "setiap pesan berpotensi ditolak 400" DIKOREKSI
      (FIN-CQ-05): kotak masuk Accounting menerima Components bernilai null.
    integration-contract.md 5.10.4 butir 1 — dari "mohon dikonfirmasi" menjadi "TERJAWAB: DITOLAK",
      beserta permintaan perubahan yang dikirim lewat evidence/16.
  SATU TEMUAN DIKLARIFIKASI SIFATNYA: FIN-CQ-07 ternyata celah IMPLEMENTASI, bukan celah desain —
  kelima endpoint GET / berpaging Purchasing SUDAH tercatat di api-contract.md B.1-B.5 sejak revisi
  4 berlabel `Rencana`. Karena itu api-contract.md TIDAK disunting dan tidak naik versi.
  SATU OPEN QUESTION BARU: FIN-OQ-036 — granularitas resource hak akses butir menu Purchasing
  (Finance.AP payung vs FinancePurchaseOrder granular). Menyentuh hak akses, sehingga BUKAN
  DEV_DISCRETION.
  DUA SURAT EVIDENCE DITULIS: evidence/15 (balasan atas ratifikasi, jawaban tujuh pertanyaan, empat
  kode baru, dua koreksi atas anggapan bersama) dan evidence/16 (permintaan perluasan validasi
  kotak masuk — sengaja dipisah karena levelnya perubahan aturan, bukan ratifikasi nama).
revision_6_note: >
  Revisi 6 (28 September 2026) adalah /design-business-module yang menyelaraskan katalog kode
  kejadian dengan ratifikasi owner Accounting (accounting/evidence/14). Menurunkan FIN-DEC-063..071.
  Keputusan arsitekturnya FIN-DES-051..058 — SELURUHNYA `draft`, BELUM disetujui owner.
  NOL tabel baru. SATU tabel diperbarui: FinSupplierReturn (kolom PPNAmount) — tabel yang SUDAH
  BERJALAN. SATU migration: AddPPNAmountToFinSupplierReturn.
  IMPACT SCAN WAJIB DIJALANKAN LEBIH DULU karena kedua SHA bergerak (backend 96bf9746 -> cba60cb0,
  79 commit fast-forward; frontend abed49b03 -> 49b59cfaa). Hasilnya MENGUBAH bentuk amendment,
  bukan hanya memperbarui angka — lihat impact_scan_note_revision_6.
  EMPAT TEMUAN yang MEMBATALKAN ASUMSI keputusan bisnis dan MUST diakui owner sebelum desain ini
  dipakai:
    (A) FIN-DEC-070 salah pemicu — shift tanpa selisih BERHENTI di CLOSED dan tidak pernah
        mencapai REVIEWED, sehingga mayoritas shift tidak akan pernah menerbitkan penanda tertutup
        dan tutup bulan Accounting tertahan selamanya. Dikoreksi FIN-DES-054.
    (B) FIN-CAP-024/FIN-DEC-070 salah kunci — satu shift dapat menghasilkan DUA baris
        BilCashVarianceReview bernilai sama (review -> NEEDS_FOLLOW_UP -> resolve), sehingga kunci
        berbasis Id review membuat satu selisih terjurnal DUA KALI. Dikoreksi FIN-DES-053.
    (C) FIN-DEC-063 tidak punya sumber fakta — Billing MENOLAK membalik top-up deposit yang
        dananya sudah terpakai, dan pembalikan tender top-up deposit TIDAK menulis mutasi deposit
        apa pun. Gap ada di BILLING, bukan Finance. FIN-OQ-034, surat ke owner Billing.
    (D) FIN-DEC-071 terlalu luas — refund SETTLEMENT ekonominya identik ALLOCATION_EXCESS dan
        TIDAK butuh kode baru; hanya REFERRED_OUTPATIENT_ADMIN yang benar-benar terbuka, dan akun
        debitnya BUKAN wewenang Finance. Dipersempit FIN-DES-056; MUST diakui owner.
  SATU TEMUAN yang akan gagal saat runtime bila tidak dikoreksi: ValidateRequest menolak
  Amount <= 0, sehingga kode penanda shift yang tidak membawa jurnal akan ditolak layanan Finance
  SENDIRI (FIN-DES-054).
  SATU KODE BARU yang tidak pernah diusulkan sebelumnya: PEMBALIKAN-PENUTUPAN-SHIFT-KASIR — lahir
  karena ReopenAsync mengizinkan shift tertutup dibuka kembali (FIN-OQ-032).
  TIGA PERUBAHAN PERILAKU pada kode yang SUDAH BERJALAN: nama empat EventTypeCode (5 titik tulis),
  nilai kredit retur termasuk PPN, properti Components dihilangkan dari PayloadJson.
revision_5_note_pointer: >
  Blok revision_5_note di bawah tetap berlaku apa adanya untuk revisi 5.
revision_5_note: >
  Revisi 5 (25 September 2026) adalah /design-business-module kecil yang menggambar skema empat
  keputusan yang sudah approved: FIN-DEC-057 (Deposit Retur sebagai sumber dana di dalam
  FinPayment), FIN-DEC-058 (kode potongan AR), serta FIN-DEC-061 dan FIN-DEC-062 yang dijawab
  owner di awal pass ini (kode RETUR-PEMBELIAN, PEMAKAIAN-DEPOSIT-RETUR,
  PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI). Keputusan arsitekturnya FIN-DES-045..050 — APPROVED
  26 September 2026: owner memilih "Setujui FIN-DES-045..050 dan kunci kontraknya" saat memanggil
  /plan-module-delivery. Dicatat apa adanya; BUKAN otorisasi membuat/menjalankan migration.
  DUA KOREKSI atas bentuk REVISI 4 yang belum dibangun: FinSupplierReturnDepositUsage menunjuk
  PaymentId (bukan PurchasingInvoiceId), dan FinReceiptDeduction melekat pada
  ReceiptAllocationId (tanpa itu tidak diketahui piutang mana yang dikurangi).
  SATU tabel yang SUDAH BERJALAN berubah: FinPayment — kolom DepositAppliedAmount dan check
  constraint CK_FinPayment_NetTransfer diganti (migration baru
  AddDepositAppliedAmountToFinPayment). SATU perilaku berjalan berubah: nilai AP_PAYMENT pada
  pembayaran yang memakai deposit turun sebesar porsi deposit; pembayaran tanpa deposit identik.
  Dua endpoint FIN-API-1.1 dicabut (deposits/{id}/apply, POST receipts/{id}/deductions) —
  keduanya belum pernah punya kode.
revision_4_note: >
  Revisi 4 (25 September 2026) adalah /design-business-module untuk TIGA rumpun kapabilitas
  BARU yang sebelumnya nol barisnya di backend: FIN-SC-008 Purchasing/AP siklus penuh (Purchase
  Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian, Deposit Retur,
  approval dua jenjang), FIN-SC-009 AR Invoice Agregat ke penjamin, dan FIN-SC-010 Potongan sisi
  penerimaan piutang. Keputusan bisnisnya FIN-DEC-045..055 (approved 25 September 2026);
  keputusan arsitekturnya FIN-DES-037..044 (draft, menunggu approval owner).
  EMPAT BELAS tabel baru dan SATU tabel diperbarui (FinSupplierPayable, kolom
  SourcePurchasingInvoiceId). Berkas ERD baru: erd/purchasing-ap.md.
  EPIC FIN-15/16/17 ditambahkan pada 04-prd-to-mvp.md; EPIC FIN-07 (lama) digantikan EPIC FIN-15.
  GERBANG KERAS BARU: kode kejadian PPN-MASUKAN-PEMBELIAN (usulan ke-25, evidence/06) MUST
  diratifikasi Accounting (FIN-OQ-020) sebelum EPIC FIN-15 boleh masuk /plan-module-delivery —
  diminta sendiri oleh owner Finance (FIN-DEC-046), bukan menunggu pihak luar yang belum diminta.
  Enam kontrak turunan naik versi (api, integration, state-transition, validation, permission,
  test) karena revisi ini menambah endpoint/status/aturan/hak-akses baru; prd-to-mvp naik ke
  FIN-MVP-1.2. Seluruhnya berstatus draft, BELUM dikunci owner.
  YANG TIDAK BERUBAH: FIN-DES-001..036 tetap berlaku; rumpun AR/Collection/Cash/Master Data/
  Accounting Integration existing tidak disentuh sama sekali, kecuali FinSupplierPayable
  (kolom baru, nullable, tidak mengubah perilaku baris lama).
revision_3_note: >
  Revisi 3 (25 September 2026) adalah AMENDMENT atas rumpun Accounting Integration dan Collection,
  dipicu balasan owner Accounting (evidence 13, ACC-DEC-082..091) dan koreksi hasil impact scan.
  Keputusan bisnisnya FIN-DEC-030..044 (approved); keputusan arsitekturnya FIN-DES-029..036
  (draft, menunggu approval owner).
  SATU PERUBAHAN PERILAKU pada source yang SUDAH BERJALAN: FIN-DEC-004 (HELD_FOR_FINALIZATION)
  digantikan FIN-DEC-030 — penerimaan sebelum tagihan final terbit segera sebagai
  PENERIMAAN-UANG-MUKA, tidak lagi ditahan. Ini satu-satunya amendment blueprint ini yang
  menyentuh kode berjalan (FinanceReceiptService, FinanceAccountingOutboxService).
  TUJUH KODE KEJADIAN BARU diusulkan (katalog naik 17 -> 24), seluruhnya MENUNGGU RATIFIKASI
  ACCOUNTING (FIN-OQ-017) dan karena itu EPIC FIN-14 berstatus OPEN DECISION di luar seluruh
  gelombang.
  TEMUAN YANG MENGUNTUNGKAN: deposit pasien, kelebihan bayar, dan selisih kas shift ternyata
  SUDAH dimiliki Billing sepenuhnya dan Finance sudah punya akses bacanya (FIN-CAP-022..024),
  sehingga NOL tabel baru dan NOL kontrak baru dari Billing. Hanya SATU migration untuk seluruh
  amendment: mengubah check constraint CK_FinBillingHandoffIntake_HandoffType dari 4 menjadi 8
  nilai.
  YANG TIDAK BERUBAH: seluruh FIN-DES-001..028 tetap berlaku; rumpun AR/AP/Payable/Cash/Master
  tidak disentuh sama sekali.
revision_2_note: >
  Revisi 2 (20 September 2026) adalah AMENDMENT atas rumpun Payable, dipicu keputusan modul
  Medical Fee (MF-DEC-002, MF-DEC-005, MF-DEC-008). Menutup FIN-OQ-013 dan FIN-OQ-014.
  Isinya: FinDoctorPayable DIGANTIKAN FinMedicalServicePayable yang melayani dokter maupun
  tenaga kesehatan lain; entity baru FinPaymentDeduction untuk potongan dan tambahan per
  pembayaran; kolom NetTransferAmount pada FinPayment. FIN-DES-025..028.
  YANG TIDAK BERUBAH, dan itu penting: seluruh rumpun MVP (data induk, intake Billing, piutang,
  penerimaan, alokasi/koreksi/write-off, kotak keluar) TIDAK disentuh sama sekali. Amendment ini
  hanya menyentuh EPIC FIN-08 dan FIN-09 yang keduanya POST-MVP dan NOL baris kode.
  Baseline revisi 1 beserta approval FIN-DES-001..024 TETAP berlaku apa adanya.
status: approved untuk revisi 1-6, 13, 14; revisi lain lihat status_note masing-masing
status_note_revision_14: >
  FIN-DES-078..091 `approved` 1 Oktober 2026 oleh Yasmin lewat pernyataan langsung
  "Saya setujui FIN-DES-078...091". Dicatat apa adanya; skill TIDAK menetapkannya sendiri.
  YANG IKUT TERANGKAT approval itu, mengikuti preseden status_note_revision_7: ketujuh kontrak turunan
  yang lahir dari pass desain yang sama — FIN-API-1.5, FIN-INTEGRATION-1.7, FIN-STATE-1.6, FIN-VAL-1.7,
  FIN-PERM-1.7, FIN-TEST-1.8, FIN-MVP-1.9. Bila owner bermaksud lebih sempit, koreksinya MUST dicatat.
  FIN-OQ-051 DIJAWAB pada kesempatan yang sama lewat FIN-DEC-138: pembuatan berkas migration
  DIIZINKAN sepanjang berkasnya sesuai ApplicationDbContextModelSnapshot, sedangkan PENERAPAN KE
  DATABASE TETAP MILIK YASMIN. Agent MUST NOT menjalankan migration, `dotnet ef database update`,
  maupun SQL langsung ke database mana pun. Jawaban eksplisit owner: "bisa membuat file migration yg
  sesuai DbSnapshot. tpi untuk aplikasi ke database, yg lakukan adalah saya."
  Dengan itu /plan-module-delivery TERBUKA untuk MVP-14A dan MVP-14B.
  DUA GERBANG BESAR DITUTUP pada kesempatan yang sama, dan keduanya membuka epic yang semula di luar
  seluruh gelombang:
    FIN-OQ-075 CLOSED oleh FIN-DEC-139 — aturan berkas bukti MENGIKUTI PRESEDEN repository:
        .pdf/.jpg/.jpeg/.png dari konfigurasi (pola <Modul>:<Fitur>:AllowedExtensions yang sudah
        dipakai dua layanan HR), batas ukuran dari konfigurasi, sistem TIDAK PERNAH menghapus bukti
        otomatis, bukti TIDAK DAPAT DIGANTI sesudah mutasi tertulis (koreksi = balik lalu catat ulang),
        akses tanpa pembatasan per pemilik transaksi pada rilis pertama — batas terakhir itu DITERIMA
        SADAR dan MUST disampaikan saat menyerahkan modul. MEMBUKA EPIC FIN-23.
    FIN-OQ-077 CLOSED oleh FIN-DEC-140 — DUA FORMAT didukung, CSV dan XLSX ("bisa pakai csv dan
        xlsx"). CSV nol paket; XLSX menuntut TEPAT SATU paket, dan wewenang penambahannya DIBERIKAN
        dengan dua batas keras: lisensi MUST permisif, dan EPPlus v5+ DILARANG karena lisensinya
        berubah komersial. Pembacanya MUST berupa lapisan terpisah sehingga validasi per baris
        TUNGGAL untuk kedua format. MEMBUKA EPIC FIN-24.
  AKIBAT YANG MUST DITINDAKLANJUTI, dan dicatat terbuka sebagai STALE:
    04-prd-to-mvp.md bagian 47, 48, dan 51 masih menyatakan EPIC FIN-23 dan FIN-24 `OPEN DECISION`
    dan di luar seluruh gelombang. roadmap/00, 01, dan 02 bagian REV-14 masih mencatat keduanya tanpa
    task. Keempat berkas itu STALE sejak FIN-DEC-139/140 dan MUST diperbarui lewat pass desain lalu
    /plan-module-delivery lanjutan — BUKAN lewat pass /grill-me yang menurunkan keputusan ini.
  YANG MASIH MUST DIMINTA TERPISAH, dan tidak satu pun terangkat approval di atas:
    (1) Nama dan versi paket pembaca XLSX (FIN-OQ-081) — dikonfirmasi pada task yang membawanya,
        mengikuti AGENTS.md. Memblokir bagian XLSX saja; CSV, validasi, dan batch tidak tertahan.
    (2) Persetujuan Accounting atas FIN-DEC-111 (FIN-OQ-045), ratifikasi PEMBUKAAN-SHIFT-KASIR
        (FIN-OQ-047), dan penerimaan konvensi tanggal WIB (FIN-OQ-048) — ketiganya BUKAN wewenang
        owner Finance. Diminta lewat evidence/22 yang sudah terkirim 1 Oktober 2026.
    (3) Penempatan tujuh butir menu baru (FIN-OQ-079) dan apakah ambang ditampilkan kepada staf
        (FIN-OQ-080).
    (4) Tiga nilai konfigurasi: angka ambang (FIN-OQ-074), jumlah tagihan lama (FIN-OQ-076), dan
        batas ukuran berkas bukti (FIN-OQ-082). Tanpa dua yang pertama, jalur yang bersangkutan
        DITOLAK fail-closed — perilaku yang disengaja, bukan kelalaian.
  MIGRATION NOMOR 3 DAN 4 tidak akan dibuat dalam waktu dekat walaupun wewenangnya sudah ada:
  keduanya milik EPIC FIN-23 dan FIN-24 yang berstatus OPEN DECISION dan berada di luar seluruh
  gelombang. Itu akibat disiplin roadmap, bukan batasan tambahan atas FIN-DEC-138.
  SURAT evidence/22 DITULIS DAN TERKIRIM 1 Oktober 2026 kepada Rizki (Accounting), atas instruksi
  eksplisit owner. Isinya: (a) jawaban lima pertanyaan 16.1-16.5, (b) tiga permintaan —
  FIN-OQ-045 (persetujuan PENERIMAAN-KASIR per kuitansi beserta dimensi shift dan metode, yang
  menuntut ACC-DEC-062 diubah), FIN-OQ-047 (ratifikasi PEMBUKAAN-SHIFT-KASIR beserta penambahannya ke
  daftar tertutup nilai nol kotak masuk Accounting), dan FIN-OQ-048 (penerimaan konvensi tanggal WIB),
  (c) PELURUSAN evidence/21 bagian 3.2 yang salah membaca source Finance sendiri soal sumber Kas Kasir,
  (d) PENCABUTAN klausa "nominal negatif ditolak tanpa pengecualian" pada evidence/21 butir 15.2,
  beserta pengakuan bahwa contoh yang dipakai (piutang lebih bayar) terbukti TIDAK DAPAT TERJADI, dan
  (e) PENGAKUAN bahwa janji "G4 siap" serta "snapshot otomatis tanggal 1 pukul 00.05 WIB" pada
  evidence/15 dan 21 TIDAK PUNYA KODE sama sekali — G4 dinyatakan ulang sebagai BERSYARAT.
  Mengikuti pola evidence/20: surat ini sengaja menaruh dua hal yang merugikan posisi Finance sendiri
  (butir c dan d) beserta satu pengakuan (butir e) SEBELUM permintaan apa pun, supaya penerimanya
  dapat menimbang jujur. Surat itu juga menyatakan penolakan atas ketiga permintaan DAPAT DITERIMA
  SEPENUHNYA, beserta apa yang Finance lakukan pada masing-masing penolakan.
  CATATAN CARA KIRIM: pengiriman di proyek ini berarti surat berada pada jalur kanonik
  docs/module-blueprints/finance-management/evidence/, tempat owner Accounting membaca surat Finance —
  pola yang sama dengan evidence/15, 16, dan 21 yang dirujuk balasan Accounting evidence/16.
  Pemberitahuan langsung kepada Rizki di luar repository BUKAN tindakan agent.
contract_versions_revision_14: >
  DRAFT untuk revisi 14 (AMENDMENT REVISI 14 / bagian L pada 02-backend-architecture.md, bagian 19 pada
  03-frontend-architecture.md, bagian 42-52 pada 04-prd-to-mvp.md). TUJUH sumbu bergerak — seluruh
  kontrak naik versi, termasuk integration-contract yang TIDAK bergerak pada dua revisi sebelumnya:
    api-contract: FIN-API-1.5 (`draft`) — bagian F baru, 24 endpoint Rencana pada empat grup baru,
                          tiga permukaan baca pada grup yang sudah ada, DAN bagian F.8 yang mencatat
                          DUA PERUBAHAN MEMUTUS pada endpoint berjalan
    integration-contract: FIN-INTEGRATION-1.7 (`draft`) — bagian 5.12 baru. Lima ruas dimensi pada
                          payload, satu kode baru PEMBUKAAN-SHIFT-KASIR, pencabutan larangan nilai
                          negatif pada pesan saldo, dan pengakuan bahwa janji G4 pada evidence/15 dan
                          21 belum punya kode. BERGERAK justru karena amandemen ini menyentuh kontrak
                          pertukaran, berbeda dari revisi 13 yang tidak
    state-transition-matrix: FIN-STATE-1.6 (`draft`) — bagian F baru: saldo awal cutover, batch
                          migrasi, dan perluasan siklus penanda shift ke tujuh status. Transisi yang
                          TIDAK sah ditulis bersama yang sah
    validation-matrix: FIN-VAL-1.7 (`draft`) — FIN-VAL-165..213 baru, beserta daftar eksplisit TUJUH
                          hal yang sengaja TIDAK divalidasi — termasuk pengakuan bahwa salah ketik
                          saldo awal yang kebetulan cocok dengan total item tidak terdeteksi
    permission-audit-matrix: FIN-PERM-1.7 (`draft`) — bagian G baru. Empat resource, satu action baru,
                          dan bagian G.5 yang mencatat EMPAT kewenangan yang TIDAK dijaga mesin hak
                          akses — termasuk bahwa satu orang dapat memegang Create dan Approve sekaligus
    acceptance-test-matrix: FIN-TEST-1.8 (`draft`) — bagian I baru, 60+ skenario, jalur gagal ditulis
                          bersama jalur berhasil. Termasuk satu test yang SENGAJA dibuat untuk
                          menangkap jalur yang lupa menulis mutasi
    prd-to-mvp: FIN-MVP-1.9 (`draft`) — bagian 42-52. Empat epic, 44 functional requirement bernomor,
                          tiga gelombang MVP-14A..14C, dan DUA epic OPEN DECISION di luar gelombang
  Langkah berikutnya: /plan-module-delivery memecah EPIC FIN-20..22 menjadi task BE-FIN-xxx/FE-FIN-xxx
  bernomor, SESUDAH owner menyetujui desain ini DAN menjawab FIN-OQ-051. Approval tetap tindakan manusia.
status_note_revision_13: >
  FIN-DES-070..077 beserta kelima kontrak turunannya (FIN-API-1.3/1.4, FIN-STATE-1.4/1.5,
  FIN-VAL-1.5/1.6, FIN-PERM-1.5/1.6, FIN-TEST-1.6/1.7) `approved` 1 Oktober 2026 oleh Yasmin lewat
  pernyataan langsung "saya setujui", sesudah owner meninjau batas FIN-OQ-044 (uang sewa belum
  tercatat sebagai kas masuk/kejadian akuntansi) dan dua keputusan penutup FIN-DEC-105/106.
  BE-FIN-052 dimulai di atas approval ini.
status_note_revision_7: >
  FIN-DES-051..058 (revisi 6) `approved` 28 September 2026 oleh Yasmin lewat pernyataan langsung
  "Saya approve", diberikan sesudah ketiga pengakuan koreksi (FIN-DEC-072/073/074) dan dua keputusan
  tambahan (FIN-DEC-075/076) turun. Dicatat apa adanya; skill TIDAK menetapkannya sendiri.
  YANG IKUT TERANGKAT approval itu: bagian 5.10 integration-contract (1.4), bagian D validation
  matrix (1.4), bagian D acceptance test (1.4), dan bagian 21-25 prd-to-mvp (1.5) — keempatnya isi
  desain revisi 6 yang disetujui bersamaan.
  YANG TIDAK IKUT TERANGKAT, dan MUST diminta terpisah:
    (1) Otorisasi membuat/menjalankan migration AddPPNAmountToFinSupplierReturn.
    (2) Otorisasi mengubah source aplikasi.
    (3) Approval revisi 7 sendiri (FIN-DES-059/060 beserta koreksi kontrak 1.5) — masih `draft`.
    (4) Keputusan FIN-OQ-036 (granularitas hak akses menu), yang MUST melibatkan Security Owner.
status_note_revision_6: >
  KETIGA PENGAKUAN SUDAH DIBERIKAN 28 September 2026 lewat /grill-me amendment pass kedua:
    (1) FIN-DEC-072 — koreksi FIN-DEC-070 diterima: pemicu penanda shift MENCAKUP CLOSED, bukan
        hanya REVIEWED (temuan A).
    (2) FIN-DEC-073 — koreksi kunci kejadian selisih kas diterima: BilCashierShift.Id, bukan
        BilCashVarianceReview.Id (temuan B).
    (3) FIN-DEC-074 — penyempitan FIN-DEC-071 diterima: refund SETTLEMENT tidak butuh kode baru
        (temuan D); hanya REFERRED_OUTPATIENT_ADMIN tetap OPEN DECISION.
  DUA KEPUTUSAN TAMBAHAN turut diambil pada pass yang sama, di luar tiga pengakuan awal:
    FIN-DEC-075 — lanjutkan desain penanda bernilai nol (FIN-DES-054 tidak berubah) DAN kirim
      permintaan tertulis terpisah ke Accounting untuk memperluas validasi Amount=0 (FIN-OQ-035,
      terkirim 28 Sep lewat evidence/16). Ini BUKAN penutupan FIN-CQ-04 sisi teknis — kotak masuk Accounting hari ini
      TETAP menolak Amount=0, hanya sisi keputusan bisnis Finance yang closed.
    FIN-DEC-076 — menegaskan FIN-DEC-060 tetap berlaku; menu Finance yang sudah dibangun (label
      Inggris, tanpa submenu "Pembelian") MUST disesuaikan lewat task frontend terpisah, BUKAN
      keputusannya yang diubah.
  YANG MASIH BELUM ADA: pernyataan eksplisit owner "setujui FIN-DES-051..058 dan kunci kontraknya"
  sebagai satu paket — ketiga pengakuan di atas MENJAWAB PERTANYAAN SPESIFIK, bukan approval
  desain menyeluruh mengikuti pola FIN-DEC-046/FIN-DES-045..050. Approval revisi 6 (bila diminta)
  tetap BUKAN otorisasi membuat/menjalankan migration AddPPNAmountToFinSupplierReturn, dan BUKAN
  otorisasi mengubah source.
status_note_revision_4: >
  `approved` sejak 25 September 2026. Saat memanggil /plan-module-delivery, owner memilih
  "Setujui FIN-DES-037..044 dan kunci kontraknya" atas pertanyaan eksplisit — mencakup (1)
  KEPUTUSAN ARSITEKTUR FIN-DES-037..044 dan (2) PENGUNCIAN FIN-API-1.1, FIN-INTEGRATION-1.2,
  FIN-STATE-1.2, FIN-VAL-1.2, FIN-PERM-1.1, FIN-TEST-1.2, FIN-MVP-1.3. Dicatat apa adanya,
  TIDAK ditetapkan skill. Approval terpisah dari keputusan bisnis FIN-DEC-045..056, konsisten
  dengan pola revisi 2 dan 3.
  Approval ini BUKAN otorisasi untuk: membuat atau menjalankan tiga migration revisi 4
  (02-backend-architecture.md C.9); mendaftarkan folder submodul Purchasing baru ke registry
  kepemilikan modul; mengubah source; maupun mengaktifkan worker pengiriman
  PPN-MASUKAN-PEMBELIAN (tetap menunggu FIN-OQ-020).
status_note_revision_3: >
  `approved` sejak 25 September 2026. Owner menyatakan "Saya approve semua" atas dua hal yang
  ditawarkan penutup pass desain revisi 3: (1) KEPUTUSAN ARSITEKTUR FIN-DES-029..036, dan
  (2) PENGUNCIAN lima kontrak turunan ke 1.1. Dicatat apa adanya, TIDAK ditetapkan skill.
  Approval ini BUKAN otorisasi untuk: membuat atau menjalankan migration (satu migration check
  constraint pada FIN-DES-029); mengubah source aplikasi; maupun mengaktifkan pengiriman kejadian
  ke Accounting. Ketiganya tetap memerlukan wewenang terpisah sesuai AGENTS.md.
  SATU HAL TIDAK IKUT TERANGKAT approval ini, karena bukan wewenang owner Finance:
  ratifikasi owner Accounting atas tujuh kode kejadian baru (FIN-OQ-017). EPIC FIN-14 tetap
  OPEN DECISION dan tetap di luar seluruh gelombang sampai Rizki menjawab.
status_scope_note: >
  Owner memberi DUA approval terpisah pada 20 September 2026:
    (1) KEPUTUSAN ARSITEKTUR FIN-DES-001..024 — "Saya setuju FIN-DES-001-024";
    (2) CAKUPAN MVP dan urutan gelombang pada 04-prd-to-mvp.md bagian 7, 8, dan 20.1 —
        dikunci apa adanya, tanpa perubahan epic.
  Pada hari yang sama owner memberi DUA approval tambahan sesudah /plan-module-delivery:
    (3) KEPUTUSAN ARSITEKTUR FIN-DES-025..028 (amendment revisi 2);
    (4) PENGUNCIAN versi kontrak turunan ke 1.0 — lihat contract_lock_note.
  Dengan demikian tidak ada lagi keputusan arsitektur Finance yang berstatus `draft`.
blueprint_shape: SINGLE
shape_decided_by: DESIGN_PASS_20_SEP_2026
shape_evidence: >
  Satu himpunan artefak tingkat modul untuk seluruh rumpun (AR, Collection, AP, Cash Management,
  Master Data, Accounting Integration). Tidak ada folder sub-modul dan tidak ada
  blueprint-manifest.md di dalam folder anak. Uji pemecahan dijalankan dan hasilnya SINGLE:
  keenam rumpun berbagi satu decision log, satu tabel kepemilikan data, dan satu rantai
  integrasi ke Accounting — memecahnya akan menduplikasi ketiganya. Mengikuti preseden
  billing-kasir (BIL-CASH-001) yang juga SINGLE dengan empat rumpun.
created_at: 2026-09-20T00:00:00+07:00
updated_at: 2026-10-01T00:00:00+07:00
last_owner_action_revision_14: >
  1 Oktober 2026 — /design-business-module menggambar FIN-DEC-111..137 menjadi FIN-DES-078..091
  (`draft`). Didahului /grill-me closure pass dalam 25 pertanyaan berurutan dan DUA
  /trace-existing-capabilities terarah pada hari yang sama, satu benang: menjawab balasan Accounting
  evidence/16 lalu menutup celah yang ditemukannya.
  TINDAKAN OWNER yang tercatat pada benang itu: 25 jawaban pilihan, di antaranya EMPAT jawaban
  tertulis panjang yang memperluas pilihannya sendiri (FIN-DEC-111, 126, 130, 137). Satu di antaranya
  memilih fleksibilitas di atas kekencangan (FIN-DEC-124, "karena lebih flexibel"), dan konsekuensinya
  ditutup pada pertanyaan berikutnya lewat FIN-DEC-125 — bukan dibiarkan menggantung.
  SATU PREMIS OWNER DIKOREKSI SEBELUM DESAIN DIMULAI, dan itu mengubah ukuran pekerjaannya: owner
  menyatakan "tabel dan confignya untuk menampung data tagihan sudah ada". Tabel piutang dan utang
  memang ada, TETAPI FinReceivable MEWAJIBKAN SourceHandoffKey/SourceHandoffId/InvoiceId terisi dengan
  index unik, jalur pembuatan utang supplier SELALU menerbitkan kejadian akuntansi, dan pemetaan akun
  control hanya berupa konstanta di kode. Jadi dasarnya ada, kesiapannya belum — dan selisih itu
  menjadi FIN-DES-089 beserta migration keempat.
  NOL source aplikasi disentuh. NOL migration dibuat. NOL package ditambahkan. NOL kontrak di-approve.
last_owner_action: >
  29 September 2026 (kesebelas, keempat pada tanggal ini) — /grill-me Amendment pass menutup
  FIN-OQ-038 lewat FIN-DEC-084: perluas platform-authorization supaya resource tanpa endpoint dapat
  dideklarasikan (opsi D), bukan membuat controller pembawa di Finance dan bukan membatalkan payung.
  Keputusan ini SISI FINANCE saja; pelaksanaannya menunggu Security Owner + pemilik
  platform-authorization, dicatat FIN-OQ-039. Owner diberi tahu lebih dulu bahwa premis asli payung
  (mencegah 403) sudah terbukti gugur, dan tetap memilih membangunnya demi kemudahan admin.
  NOL source disentuh; NOL kontrak naik versi.
  Sebelumnya pada tanggal yang sama (kesepuluh, ketiga) — /design-business-module kecil menggambar
  mekanisme FIN-DEC-082/083 menjadi FIN-DES-066..069 (`draft`). Impact scan frontend menemukan premis
  FIN-OQ-036 keliru (menu yang dijaga payung ternyata milik V2, bukan Purchasing) dan menemukan
  kendala mekanis yang menentukan: resource payung tanpa endpoint tidak dapat didaftarkan registry
  hari ini. Satu gerbang dibuka: FIN-OQ-038 (pembawa resource payung — Security Owner + pemilik
  platform-authorization), menahan HANYA implementasi mekanisme ekspansi. Dua temuan dilaporkan
  (FIN-CQ-09, FIN-CQ-10). Kontrak permission-audit-matrix.md naik ke FIN-PERM-1.4 (`draft`).
  NOL source disentuh. Desain ini BELUM disetujui owner.
  Sebelumnya pada tanggal yang sama (kesembilan, kedua) — /grill-me Amendment pass mengoreksi
  FIN-DEC-079: FIN-DEC-082 (payung dipindah ke nama Finance.AP.Umbrella/Finance.AR.Umbrella,
  FinanceApController/FinanceArController V2 tidak disentuh) dan FIN-DEC-083 (ekspansi
  payung->granular materialized saat grant diberikan admin, bukan live saat request — pola
  be-sec-003b-policy-expansion.sql). Dipicu temuan BE-FIN-042 bahwa Finance.AP/Finance.AR sudah
  dipakai controller V2 nyata, kontradiksi internal dengan FIN-DEC-059 yang sudah mencatatnya
  berjalan. FIN-OQ-036 dibuka kembali sebagian — lihat 00-interview-decisions.md bagian Amendment
  pass terbaru. NOL source disentuh oleh pass ini; BE-FIN-042 §D.5/D.6.1 (rename 6 controller +
  skrip migrasi SysAccessPolicy) tidak terdampak, sudah selesai sebelum pass ini.
  Sebelumnya pada tanggal yang sama (kedelapan, pertama) — /design-business-module (pemetaan payung-ke-granular FIN-CQ-08):
  Menurunkan keputusan FIN-DEC-078 dan FIN-DEC-079 secara definitif ke dalam kontrak dan arsitektur backend.
  contracts/permission-audit-matrix.md dinaikkan ke FIN-PERM-1.3 (AMENDMENT REVISI 6: pendaftaran resmi resource
  payung Finance.AP dan Finance.AR, pemetaan rinci ke 9 resource granular AP dan 4 resource granular AR,
  matriks pewarisan aksi View->Read, Operate->Maker, Approve->Checker, daftar perubahan string [AccessPermission]
  pada 6 controller legacy, serta skrip SQL idempotent migrasi data peran SysRolePermissions).
  02-backend-architecture.md diperbarui dengan AMENDMENT REVISI 8 (FIN-DES-061..063, approved).
  03-frontend-architecture.md § 15.4 diperbarui mengonfirmasi bahwa filter menu frontend TIDAK PERLU DIUBAH
  dan menutup FIN-CAP-040 serta FIN-OQ-036 secara tuntas. FIN-CQ-08 berstatus CLOSED dan siap diimplementasikan.
  Sebelumnya pada hari yang sama (ketujuh) — Billing membalas evidence/17 lewat evidence/18: FIN-OQ-034 DITUTUP
  TUNTAS dari kedua sisi. Solusi Billing (BKC-DEC-128..131, BKC-DES-051..054, BE-BKC-079): saat
  tender top-up deposit REVERSED dan saldo tidak cukup, sistem membatalkan alokasi tagihan LIFO
  otomatis (baris kompensasi ReversesAllocationId), menulis DUA mutasi BilDepositMovement (RELEASE
  untuk alokasi dibatalkan, REVERSAL untuk top-up ditarik, ReversesMovementId terisi), menyelaraskan
  status invoice CLOSED -> FINAL via SyncClosureAsync, dan menjamin AvailableBalance >= 0. NOL
  migration. Klaim ini DIVERIFIKASI LANGSUNG ke BillingSettlementService.cs working tree — simbol
  HandleDepositTopUpReversalAsync, ReversesAllocationId, SyncClosureAsync, PaymentReversed seluruhnya
  ADA. Pendeteksi baca-saja Finance (FIN-VAL-142) sekarang akan LOLOS dan kedua kejadian
  (PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT dari RELEASE, PEMBALIKAN-PENERIMAAN-UANG-MUKA dari
  REVERSAL) punya sumber fakta yang deterministik. Billing menyatakan BE-FIN-036 UNBLOCKED.
  Sebelumnya pada hari yang sama (keenam) — /grill-me menutup FIN-CQ-08: FIN-DEC-078 (kode enam
  controller lama diubah mengikuti nama penuh kontrak, MUST disertai migrasi data peran) dan
  FIN-DEC-079 (Finance.AP/AR didaftarkan resource resmi tingkat kelompok, otomatis mencakup
  resource granular di bawahnya — menutup FIN-OQ-036 sekaligus). Pemetaan payung-ke-granular dan
  migrasi data peran BELUM digambar — menyusul /design-business-module kecil.
  Sebelumnya pada hari yang sama (kelima) — owner meminta /trace-existing-capabilities dijalankan lagi dan surat
  ketiga dikirim ke owner Billing. Impact scan lanjutan (capability map bagian 16) mengaudit
  pekerjaan BE-FIN-036 yang berjalan di working tree: SEBELAS titik desain FIN-DES-045..047 SESUAI;
  dua cacat (alias konstanta yang dilarang FIN-DES-051, dan NOL test untuk +590 baris logika uang);
  satu temuan SISTEMIK (FIN-CQ-08/FIN-CAP-043 — enam resource hak akses di kode berbeda nama dari
  kontrak, dan Finance.AP/Finance.AR tidak ada di kontrak sama sekali); satu koreksi rujukan pada
  dokumen desain revisi 6 (FIN-VAL-123 -> FIN-VAL-137, empat tempat, sudah dibetulkan).
  evidence/17 ditulis ke owner Billing untuk FIN-OQ-034. dotnet build TIDAK dijalankan.
  Sebelumnya pada hari yang sama (keempat) — owner MENYETUJUI FIN-DES-051..058 lewat pernyataan langsung
  "Saya approve", lalu meminta balasan dikirim ke owner Accounting. /design-business-module revisi 7
  dijalankan: FIN-DES-059/060 diturunkan (`draft`), dua koreksi dokumen berbasis bukti audit
  diterapkan, dan DUA SURAT EVIDENCE DITULIS — evidence/15 (balasan atas ratifikasi: jawaban tujuh
  pertanyaan, nama final enam kode pecahan, empat kode baru, dua koreksi atas anggapan bersama) dan
  evidence/16 (permintaan perluasan validasi kotak masuk untuk kejadian bernilai nol). Satu open
  question baru dibuka: FIN-OQ-036 (granularitas hak akses butir menu, menuntut Security Owner).
  Approval ini BUKAN otorisasi migration maupun perubahan source; revisi 7 sendiri masih `draft`.
  Sebelumnya pada hari yang sama (ketiga) — /grill-me amendment pass kedua menjawab lima keputusan
  (FIN-DEC-072..076) menutup temuan /trace-existing-capabilities atas AMENDMENT REVISI 6: tiga
  pengakuan koreksi (FIN-DEC-070 pemicu dan kunci, FIN-DEC-071 penyempitan) DITERIMA seluruhnya;
  jalan keluar FIN-CQ-04 (kotak masuk Accounting menolak Amount=0) DIPILIH — lanjutkan desain +
  kirim permintaan tertulis terpisah (FIN-OQ-035, surat terkirim 28 Sep lewat evidence/16); FIN-CQ-06 (label menu vs
  FIN-DEC-060) DIPILIH — tegakkan keputusan, task frontend menyesuaikan. FIN-DES-051..058 MASIH
  BELUM mendapat pernyataan approval menyeluruh "setujui dan kunci kontrak" — lihat
  status_note_revision_6.
  Sebelumnya pada hari yang sama (kedua) — /design-business-module revisi 6 dijalankan, menurunkan
  FIN-DEC-063..071 menjadi FIN-DES-051..058 (`draft`).
  Sebelumnya pada hari yang sama (pertama) — /grill-me closure pass menjawab sembilan keputusan (FIN-DEC-063..071) atas
  balasan owner Accounting (evidence/14) untuk tiga surat Finance (evidence/04-05, 06, 07):
  penamaan kode pecahan selisih kas dan potongan piutang, rename PEMAKAIAN-DEPOSIT-RETUR, tiga
  syarat PENGAKUAN-KELEBIHAN-BAYAR diterima, PPN retur pembelian dan kode barunya, kredit retur
  tidak pernah dicairkan tunai, bentuk kode penanda shift tertutup, dan asumsi defensif refund
  kas SETTLEMENT/REFERRED_OUTPATIENT_ADMIN. FIN-OQ-017/020(sisi ratifikasi)/026 CLOSED; lima open
  question baru FIN-OQ-027..031 dibuka, seluruhnya hanya menahan aktivasi worker pengiriman kode
  terkait (pola FIN-DEC-056), TIDAK menahan epic manapun masuk /plan-module-delivery. Dua desain
  skema masih menyusul /design-business-module: bentuk kode FIN-OQ-031/PENUTUPAN-SHIFT-KASIR, dan
  titik tulis PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT. Surat balasan evidence/15 ke Rizki belum
  ditulis — menyusul sebagai instruksi terpisah.
  Sebelumnya, 25 September 2026 — owner MENYETUJUI FIN-DES-029..036 dan MENGUNCI lima kontrak turunan ke 1.1
  ("Saya approve semua"), lalu meminta /plan-module-delivery dijalankan. Sebelumnya pada hari yang
  sama: owner menjawab lima butir koreksi hasil impact scan (FIN-DEC-040..044,
  /grill-me Amendment pass lanjutan), menyetujui pengiriman surat koreksi ke owner Accounting
  (evidence/05), dan meminta impact scan formal dijalankan sebelum blocker
  BILLING-COLLECTION-HANDOFF ditutup. Sebelumnya pada hari yang sama: FIN-DEC-030..039 dijawab.
  Sebelumnya, 23 September 2026 — UI brief frontend FE-FIN-* dijawab lengkap (FIN-DEC-024..029, /grill-me
  Amendment pass), menutup roadmap_status ACTIVE_BLOCKED_ON_UI_BRIEF di 02-frontend-roadmap.md.
  Sebelumnya, 20 September 2026 — approval FIN-DES-025..028, penguncian tujuh kontrak turunan
  ke 1.0, dan penegasan kepemilikan modul oleh Yasmin.
owners:
  product_domain: >
    Yasmin — Product/Domain Owner modul Finance Management (AR/AP). Yasmin juga owner modul
    Medical Fee (MF-BP-001). Kepemilikan ganda ini DITEGASKAN owner 20 September 2026 dan TIDAK
    meruntuhkan batas antar keduanya: Finance tetap MENGONSUMSI MdfFinanceHandoff dan tidak
    pernah menulis ke tabel Medical Fee, sebagaimana FIN-DES-025 menetapkannya.
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: >
    Product Owner (Yasmin) — UI brief closed 23 September 2026 lewat /grill-me, tercatat
    FIN-DEC-024..029 di 00-interview-decisions.md bagian Frontend Decision Authority. Lihat
    juga 02-frontend-roadmap.md bagian 5 dan 03-frontend-architecture.md untuk kontrak
    fungsional yang tidak berubah oleh amendment ini.
  cross_module_billing: Billing Owner (FIN-DEC-005, FIN-DEC-006 menunggu konfirmasi)
  cross_module_accounting: Rizki (owner Accounting)
  cross_module_hr: HR Owner (FIN-DEC-016 menunggu konfirmasi)
approved_by: Yasmin (Product/Domain Owner Finance)
approved_at: 2026-09-20
approval_note: >
  Owner menyatakan "Saya setuju FIN-DES-001-024" pada 20 September 2026. Approval itu dicatat
  apa adanya, TIDAK ditetapkan skill.
  Approval ini BUKAN otorisasi untuk: membuat atau menjalankan migration; mengubah source
  aplikasi backend maupun frontend; mendaftarkan sendiri enam submodul baru ke registry
  kepemilikan modul; maupun mengaktifkan pengiriman kejadian ke Accounting. Keempatnya tetap
  memerlukan wewenang terpisah sesuai AGENTS.md.
  Lihat `status_scope_note` untuk tiga hal yang TIDAK ikut terangkat approval ini.

backend_commit_sha: 7f8c3014
backend_commit_sha_note_revision_14: >
  Revisi 14 mencatat 7f8c3014, naik dari 7811c048 yang tercatat sejak revisi 8. Selisih itu TIDAK
  dilompati: impact scan read-only atas area terdampak dijalankan dua kali dan tercatat lengkap pada
  01-existing-capability-map.md bagian 18 (AccountingIntegration, BillingIntake, CashManagement,
  Collection, Receivable, Payable) dan bagian 19 (Payable direct payment, bukti pembayaran, kesiapan
  migrasi). Bagian 1-17 peta kapabilitas TIDAK diaudit ulang; bagiannya yang menyentuh keenam rumpun
  di atas MUST dianggap stale sampai dipindai ulang.
backend_commit_sha_previous_revision_14: 7811c048
backend_commit_sha_previous: cba60cb0
backend_commit_sha_note_revision_8: >
  Bergerak dua commit: 0a09e18a (BE-FIN-041, kolom DepositAppliedAmount) dan 7811c048 (BE-FIN-035
  dan BE-FIN-036 beserta seluruh artefak blueprint revisi 6/7, ditambah perbaikan Billing BE-BKC-079
  yang menutup FIN-OQ-034). Impact scan terarah dijalankan pada area mutasi deposit dan menghasilkan
  FIN-DES-064/065 — lihat 02-backend-architecture.md bagian H.1. Area lain TIDAK dipindai ulang pada
  pass ini, sehingga klaim as-is di luar rumpun mutasi deposit tetap bersandar pada scan cba60cb0.
backend_commit_sha_previous: 96bf9746
backend_commit_sha_baseline: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
frontend_commit_sha: 0b54fdce6
frontend_commit_sha_note_revision_14: >
  Revisi 14 mencatat 0b54fdce6. Frontend BERGERAK dua kali selama pass ini: a31da3c21 -> 85578363b
  (dipindai terbatas pada bagian 18.6 peta kapabilitas, tiga kata kunci saja) -> 0b54fdce6 (TIDAK
  dipindai). Karena itu bagian 18.6 MUST dianggap stale, dan bagian 19 pass desain ini tidak
  mengandalkan temuan frontend apa pun. Satu akibatnya dicatat terbuka pada 03-frontend-architecture.md
  bagian 19.1: dua layar pembayaran langsung yang sudah berjalan AKAN RUSAK bila backend naik lebih
  dulu, dan urutan rilisnya MUST dijaga.
frontend_commit_sha_previous_revision_14: a31da3c21
frontend_commit_sha_previous: 49b59cfaa
frontend_branch: yasmina
frontend_commit_sha_note_revision_11: >
  DINAIKKAN 29 September 2026 sesudah impact scan read-only pada pass desain revisi 11. Pemindaian
  SENGAJA DIBATASI pada area terdampak amendment itu: penyaring menu sidebar
  (src/utils/menu-sidebar/menu-items.jsx dan permission/filter-menu-items-by-permission.jsx) serta
  pemakaian resource Finance.AP/Finance.AR. Area frontend lain TIDAK dipindai ulang, sehingga klaim
  as-is di luar penyaringan menu tetap bersandar pada scan 49b59cfaa. Hasil pindaian mengoreksi dua
  premis FIN-OQ-036 dan membuka FIN-CQ-09 serta FIN-CQ-10 — lihat revision_11_note.
impact_scan_note_revision_6: >
  SHA DINAIKKAN 28 September 2026 sesudah impact scan read-only pada awal /design-business-module
  revisi 6 (02-backend-architecture.md bagian E.1). Backend 96bf9746 -> cba60cb0 adalah 79 commit
  FAST-FORWARD (diverifikasi `git merge-base --is-ancestor`, bukan diverged).
  YANG DIPERIKSA dan berubah di dalam boundary amendment ini:
    - Areas/Corporate/FinanceManagement/Purchasing/** — SELURUH rumpun Purchasing/AP revisi 4-5
      SUDAH DIBANGUN (7 model + 5 controller + 5 service + 5 DTO). Kode kejadian
      PPN-MASUKAN-PEMBELIAN dan RETUR-PEMBELIAN SUDAH BERJALAN.
    - AccountingIntegration/Models/FinAccountingEventOutbox.cs — 2 konstanta kode ditambahkan.
    - Payable/Models/FinPayment.cs, FinSupplierPayable.cs, FinancePaymentService.cs — revisi 5
      sebagian terpasang.
    - Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs —
      sumber Bank Darah ditambahkan (BE-BD-013); DI LUAR boundary Finance, tidak berdampak.
  TEMUAN STALENESS YANG BUKAN MILIK SKILL INI UNTUK DIPERBAIKI:
    - FIN-CAP-018 (`Missing`) SALAH sejak cba60cb0 — endpoint penerima Accounting SUDAH ADA
      (Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventService.cs).
      Sejalan dengan evidence/14 bagian 4.4.
    - FIN-CAP-026..036 (rumpun Purchasing/AP) ditandai `Missing`/rencana, padahal sudah dibangun.
    - 01-existing-capability-map.md MUST diperbarui /trace-existing-capabilities; skill desain
      TIDAK menulis berkas itu. Sampai itu dijalankan, capability map berstatus STALE.
  SUDAH DITINDAKLANJUTI pada hari yang sama: /trace-existing-capabilities dijalankan 28 September
  2026 (capability map bagian 15) pada cba60cb0/49b59cfaa. Map TIDAK lagi stale. Pass itu
  MENGOREKSI dua klaim desain revisi 6 — lihat FIN-CQ-04 (memblokir FR-FIN-105) dan FIN-CQ-05
  (menurunkan prioritas pelurusan Components), dan menemukan dua Conflict frontend baru
  (FIN-CQ-06, FIN-CQ-07).
  YANG TIDAK DIPERIKSA: audit field-per-field rumpun AR/AP/Payable (utang lama dari revisi 3,
  readiness butir (h)); dan project test — keberadaannya belum diperiksa ulang pada cba60cb0.
structure_deviation_note: >
  DEVIASI TERCATAT terhadap references/blueprint-output-contract.md bagian 1.1: blueprint ini
  memakai `erd/` (7 berkas, termasuk erd/data-dictionary.md) dan TIDAK punya `flowcharts/` maupun
  `data/`. Deviasi ini SUDAH ADA sejak revisi 1 dan dipertahankan revisi 2-6.
  ALASAN dipertahankan, bukan dirapikan di sini: memindahkan 7 berkas memutus rujukan yang sudah
  tertulis di roadmap (41 task BE + 13 FE), 30+ laporan task yang sudah selesai, dan 7 surat
  evidence yang sudah DIKIRIM ke owner modul lain — termasuk surat yang sedang ditunggu jawabannya.
  Merapikannya di tengah amendment kecil menukar satu ketidakrapian dengan puluhan rujukan mati.
  DIREKOMENDASIKAN sebagai pass migrasi struktur tersendiri, yang sekaligus menulis
  flowcharts/00-alur-utama.md beserta flowchart per proses (alur yang sampai sekarang memang
  BELUM PERNAH digambar untuk modul ini — itu kekurangan nyata, bukan sekadar soal letak berkas).
sha_verification_note_revision_3_pass2: >
  SHA backend DINAIKKAN ke 96bf9746 pada 25 September 2026 sesudah /trace-existing-capabilities
  untuk tiga scope baru (01-existing-capability-map.md bagian 12) — FIN-SC-008 Purchasing/AP,
  FIN-SC-009 AR Invoice Agregat, FIN-SC-010 Potongan sisi AR. Dipicu evidence eksternal
  Keuangan.md dan keputusan bisnis FIN-DEC-045..055.
  Verifikasi staleness: d6cdfaf9 -> 96bf9746 adalah 45 commit fast-forward (bukan diverged);
  git diff --stat dikonfirmasi NOL perubahan pada Areas/Corporate/FinanceManagement/Payable/,
  MstSupplier.cs, atau area Purchasing manapun — seluruh 45 commit itu di luar boundary rumpun
  yang diaudit di sini (BillingPayerEditService.cs/BillingRefundService.cs/CashierShiftService.cs,
  bertepatan dengan implementasi BE-BUI-001/002 pada blueprint billing-kasir).
  SEBELAS entri kemampuan baru ditambahkan (FIN-CAP-026..036), termasuk TEMUAN PENTING:
  FinancePaymentService.ResolveApprovalTier SUDAH memakai placeholder Rp 50.000.000 - angka yang
  PERSIS SAMA dengan FIN-DEC-052 yang baru disepakati, dan BillingCompanyGuarantorInvoiceDocumentService
  (per-invoice, bukan agregat) dapat dipakai ulang sebagai rincian di balik AR Invoice Agregat.
sha_verification_note_revision_3: >
  SHA backend DINAIKKAN ke d6cdfaf9 pada 25 September 2026 sesudah impact scan read-only
  (01-existing-capability-map.md bagian 9.3). Baseline 09101d05 dipertahankan pada field
  backend_commit_sha_baseline karena bagian 1-10 02-backend-architecture.md, AMENDMENT REVISI 2,
  dan bagian 1-8 capability map masih mencerminkan SHA itu dan TIDAK diaudit ulang menyeluruh.
  Yang diverifikasi pada d6cdfaf9: keberadaan BilCollectionHandoff beserta konsumennya,
  BilDepositAccount/BilDepositMovement, BilRefundableCredit/BilRefundCase,
  BilCashVarianceReview/BilCashierShift, check constraint FinBillingHandoffIntake dan
  FinAccountingEventOutbox, serta ketiadaan endpoint penerima Accounting Event.
  BELUM diaudit field-per-field: rumpun AR/AP/Payable Finance yang dibangun BE-FIN-001..021
  (59 berkas, 9062 baris) — lihat "Yang TIDAK ditutup" pada capability map bagian 9.3.
  Frontend SHA TIDAK diverifikasi ulang pada revisi 3; revisi ini tidak menyentuh frontend.
sha_verification_note: >
  Kedua SHA diverifikasi ulang pada awal pass desain 20 September 2026 dan TIDAK bergerak dari
  baseline capability map. Working tree backend bersih kecuali folder
  docs/module-blueprints/finance-management/ yang belum ter-track (keluaran pass ini sendiri).
  Karena SHA tidak bergerak, impact scan tambahan TIDAK dijalankan dan memang tidak diperlukan.

input_revisions:
  decisions: >
    FIN-DEC-001..055. FIN-DEC-001..023 approved 20 September 2026; FIN-DEC-024..029 approved
    23 September 2026; FIN-DEC-030..039 approved 25 September 2026; FIN-DEC-040..044 approved
    25 September 2026 (Amendment pass lanjutan); FIN-DEC-045..055 approved 25 September 2026
    (Amendment pass rumpun Purchasing/AP + AR Invoice Agregat, dipicu Keuangan.md).
    SUPERSEDED: FIN-DEC-004 (oleh 030), FIN-DEC-007 sebagian (oleh 036), FIN-DEC-015 (oleh 045),
    FIN-DEC-023 (oleh 035), FIN-DEC-032 (oleh 040 dan 041), FIN-DEC-033 (oleh 042).
    CLOSED pada closure pass lanjutan: FIN-OQ-010 dan FIN-OQ-019 (oleh 052), FIN-OQ-021 (oleh
    054 dan 055); FIN-OQ-020 dipersempit (oleh 053) — murni menunggu ratifikasi Accounting.
  capability_map: >
    revisi 1 — 20 September 2026 diaudit pada 09101d05 / abed49b03; bagian 9.3 ditambahkan
    25 September 2026 dari impact scan pada d6cdfaf9 (FIN-CAP-007/008/009 diperbarui dari
    Missing menjadi Ready to reuse; FIN-CAP-022..025 ditambahkan); bagian 12-14 ditambahkan
    25 September 2026 (pass kedua) dari /trace-existing-capabilities pada 96bf9746 untuk
    FIN-SC-008/009/010 (FIN-CAP-026..036 ditambahkan).
  decisions_revision_6: >
    FIN-DEC-063..071 — approved 28 September 2026 (/grill-me closure pass atas
    accounting/evidence/14). SUPERSEDED tambahan: FIN-DEC-058 (oleh FIN-DEC-065).
    FIN-DEC-072..076 — approved 28 September 2026 (/grill-me amendment pass kedua, hari yang sama)
    MENUTUP pengakuan yang diminta: FIN-DEC-070 DIKOREKSI oleh 072 (pemicu)/073 (kunci); FIN-DEC-071
    DIPERSEMPIT oleh 074 (hanya REFERRED_OUTPATIENT_ADMIN tetap terbuka). FIN-DEC-075 memilih jalan
    keluar FIN-CQ-04 (lanjutkan desain + surat FIN-OQ-035). FIN-DEC-076 menegaskan FIN-DEC-060 tetap
    berlaku, menu MUST disesuaikan lewat task frontend.
  requirement_gate: NOT_RUN — lihat requirement_completeness_gate_note
  hospital_domain_architecture: >
    NOT_RUN, dan tetap DOMAIN_ARCHITECTURE_NOT_RUN untuk revisi 6. Alasannya sama seperti
    domain_architecture_readiness di bawah, dan revisi 6 justru memperkuatnya: amendment ini nol
    keputusan klinis, nol tulisan ke modul sumber, dan seluruh batas lintas konteks yang disentuh
    (Billing -> Finance -> Accounting) sudah diputus eksplisit oleh keputusan bisnis yang ada.
    SATU CATATAN: dua temuan revisi 6 (C dan D) adalah persoalan batas lintas modul yang TIDAK
    dapat diselesaikan Finance sendiri, dan keduanya dikembalikan ke pemiliknya lewat FIN-OQ-031
    dan FIN-OQ-034 — bukan diselesaikan dengan menebak di dokumen ini.
  owner_analysis_documents: >
    FIN-BRD-V2-0.3, FIN-PRD-V2-0.3, FIN-MVP-PRD-V2-0.3, FIN-ACC-XMOD-V2-0.3 — keempatnya
    bertanggal analisis 18 September 2026, dipakai sebagai masukan yang SUDAH diratifikasi
    sebagian lewat wawancara 20 September 2026. Dokumen itu BUKAN kontrak; yang mengikat adalah
    00-interview-decisions.md.
    Keuangan.md — evidence eksternal (analisis video sistem rujukan), TIDAK otoritatif, dipakai
    murni sebagai peta area untuk deteksi gap pada Amendment pass 25 September 2026.
input_hashes:
  00-interview-decisions.md: 75c1dbc8fff8df4fe94cd38fe01f72d4df45d7e1cc7ea247c39630a83e492202  # amendment pass 29 Sep 2026 (kesebelas): FIN-DEC-084, menutup FIN-OQ-038 dan membuka FIN-OQ-039; surat evidence/20 ditulis
  01-existing-capability-map.md: 3489cf24461bb582f57fc00a562be914bdea67900870ea94e03ea40788a5ca29  # impact scan lanjutan 28 Sep 2026 (bagian 16) — audit BE-FIN-036 di working tree
input_hashes_note_revision_4: >
  Hash decision log BERGERAK TIGA KALI 25 September 2026, bukan drift: (1) FIN-DEC-056 dan
  FIN-OQ-022..025 ditambahkan oleh /plan-module-delivery (nilai 5fd9d74e...); (2) FIN-DEC-057..060
  ditambahkan oleh /grill-me closure pass yang menutup keempat FIN-OQ itu dan membuka FIN-OQ-026
  (nilai sebelumnya 8c932826..., lalu 5fd9d74e...). Hash capability map tidak bergerak sejak
  trace pass kedua.
input_hashes_note: >
  SHA256 atas isi berkas apa adanya. Nilai awal dihitung 20 September 2026; DIPERBARUI
  25 September 2026 sesudah Amendment pass rumpun Purchasing/AP + AR Invoice Agregat (FIN-DEC-045..055)
  dan /trace-existing-capabilities pass kedua (bagian 12-14 capability map). Bila salah satu
  hash berubah lagi tanpa revisi manifest ini ikut naik, artefak desain di bawahnya berpotensi
  drift dan MUST diperiksa ulang sebelum dipakai /plan-module-delivery.
  Hash capability map SUDAH BERGERAK DUA KALI pada pass desain ini, dan keduanya bukan drift:
    c5fd6705... -> 704194cd...  menambahkan FIN-CAP-020 dan FIN-CAP-021 hasil pemeriksaan
                                terarah, menutup FIN-CQ-02, membuka FIN-CQ-03 (bagian 9.1);
    704194cd... -> 0576bf65...  MENGOREKSI FIN-CAP-020 dari `Ready to reuse` menjadi `Conflict`
                                setelah isi MstDoctorServiceRule dibaca langsung dan ternyata
                                bukan aturan perhitungan fee (bagian 9.2).
  Nilai di atas adalah hash SESUDAH kedua pembaruan tersebut.

contract_versions:
  api-contract: FIN-API-1.1 — draft 2026-09-25, naik dari 1.0 (locked). AMENDMENT REVISI 4 —
    tujuh grup endpoint baru (Purchase Order, Goods Receipt, Invoice Exchange, Purchasing
    Invoice, Supplier Return, Purchasing Reports, Receivable Invoice Batch), dua endpoint baru
    pada grup Receipt
  integration-contract: FIN-INTEGRATION-1.2 — draft 2026-09-25, naik dari 1.1 (locked). Bagian
    5.8 baru (kode ke-25 PPN-MASUKAN-PEMBELIAN, diusulkan), bagian 6 dan 7 diperbarui
  state-transition-matrix: FIN-STATE-1.2 — draft 2026-09-25, naik dari 1.1 (locked). AMENDMENT
    REVISI 4 — tujuh entity baru (bagian B.1-B.8)
  validation-matrix: FIN-VAL-1.2 — draft 2026-09-25, naik dari 1.1 (locked). AMENDMENT REVISI 4 —
    FIN-VAL-100..122 baru
  permission-audit-matrix: FIN-PERM-1.1 — draft 2026-09-25, naik dari 1.0 (locked). AMENDMENT
    REVISI 4 — tujuh Resource baru, dua Action baru pada grup Receipt
  prd-to-mvp: FIN-MVP-1.3 — draft 2026-09-25, naik dari 1.1 (locked untuk MVP-0..5). AMENDMENT
    REVISI 4 (1.2) — EPIC FIN-15/16/17 baru, FR-FIN-081..095, EPIC FIN-07 digantikan FIN-15;
    (1.3) FIN-DEC-056 mempersempit gerbang FIN-OQ-020 — EPIC FIN-15 boleh masuk
    /plan-module-delivery tanpa menunggu ratifikasi Accounting
  acceptance-test-matrix: FIN-TEST-1.2 — draft 2026-09-25, naik dari 1.1 (locked). AMENDMENT
    REVISI 4 — bagian B.1-B.7 baru
contract_versions_revision_11: >
  DRAFT untuk revisi 11 (AMENDMENT REVISI 11 pada 02-backend-architecture.md). Yang BERGERAK hanya
  SATU sumbu:
    permission-audit-matrix: FIN-PERM-1.4 (`draft`) — AMENDMENT REVISI 7 pada dokumen itu. Tiga
                          bagian dikoreksi: D.2 (nama payung menjadi Finance.AP.Umbrella dan
                          Finance.AR.Umbrella, mekanisme kerjanya diperbaiki), D.6.1 (skrip SQL
                          lama DICABUT karena menyasar tabel SysRolePermissions yang tidak ada;
                          diganti penjelasan mekanisme SysAccessPolicy yang sebenarnya beserta
                          rujukan ke skrip pengganti yang sudah ditulis), dan D.6.2 (ekspansi
                          pindah dari AccessMenuSeeder ke RoleAccessController.ApplyPoliciesAsync).
                          D.3, D.4, dan D.5 TIDAK disunting — isinya tidak bergerak.
  ENAM sumbu SENGAJA TIDAK DISUNTING pada revisi 11, karena isinya tidak bergerak:
    api-contract (FIN-API-1.2)              — nol endpoint baru. Endpoint pembawa payung baru akan
                                              ada bila FIN-OQ-038 dijawab dengan jalan keluar (C)
    integration-contract (FIN-INTEGRATION-1.6) — nol kode kejadian tersentuh
    validation-matrix (FIN-VAL-1.5)         — nol aturan validasi baru; penolakan ekspansi adalah
                                              gerbang teknis, bukan aturan bisnis bernomor
    state-transition-matrix (FIN-STATE-1.3) — nol status baru
    acceptance-test (FIN-TEST-1.6)          — baris ujinya menyusul bersama implementasi, sesudah
                                              FIN-OQ-038 dijawab
    prd-to-mvp (FIN-MVP-1.6)                — nol epic, nol FR, nol gelombang yang bergerak
contract_versions_revision_9: >
  APPROVED 29 September 2026 bersama FIN-DEC-080/081 (Amendment Pass). Ketiga sumbu yang bergerak
  resmi disetujui owner:
    integration-contract: FIN-INTEGRATION-1.6 (approved)
    validation-matrix:    FIN-VAL-1.5 (approved)
    acceptance-test:      FIN-TEST-1.6 (approved)
contract_versions_revision_8: >
  DRAFT untuk revisi 8 (AMENDMENT REVISI 9 pada 02-backend-architecture.md). Yang BERGERAK hanya
  TIGA sumbu, seluruhnya karena pemicu satu kode kejadian dikoreksi:
    integration-contract: FIN-INTEGRATION-1.6 (tiga pemicu dikoreksi: mutasi RELEASE menerbitkan
                          PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT dan bukan PENGEMBALIAN-UANG-MUKA;
                          pemicu kode 37 yang semula "tidak pernah ada" kini ada lewat BKC-DEC-131;
                          mutasi RELEASE tanpa pasangan REVERSAL ditolak fail-closed)
    validation-matrix:    FIN-VAL-1.5 (bagian E baru — FIN-VAL-144..146)
    acceptance-test:      FIN-TEST-1.6 (bagian F baru — sepuluh baris uji)
  EMPAT sumbu SENGAJA TIDAK DISUNTING pada revisi 8, karena isinya tidak bergerak:
    api-contract (FIN-API-1.2)              — nol endpoint baru; amendment ini murni pemetaan pemicu
    permission-audit (FIN-PERM-1.3)         — nol resource dan nol action baru
    state-transition-matrix (FIN-STATE-1.3) — nol status Finance baru; status mutasi deposit milik
                                              Billing dan tidak berubah
    prd-to-mvp (FIN-MVP-1.6)                — nol epic, nol FR, dan nol gelombang yang bergerak;
                                              FR-FIN-096/097 tetap dijawab task yang sama
contract_versions_revision_7: >
  DRAFT untuk revisi 7. Yang BERGERAK hanya TIGA sumbu:
    integration-contract: FIN-INTEGRATION-1.5 (5.10.4 butir 1 dan 5.10.5 butir 1 dikoreksi
                          berbasis bukti; bagian 5.11 baru — kontrak as-is kotak masuk Accounting)
    acceptance-test:      FIN-TEST-1.5 (bagian E baru)
    prd-to-mvp:           FIN-MVP-1.6 (bagian 26-30 baru; gerbang FR-FIN-105 berpindah ke FIN-OQ-035)
  EMPAT sumbu SENGAJA TIDAK DISUNTING pada revisi 7:
    api-contract (FIN-API-1.2)              — kelima endpoint GET / berpaging Purchasing SUDAH ADA
                                              di B.1-B.5 sejak revisi 4 berlabel `Rencana`. FIN-CQ-07
                                              adalah celah implementasi, bukan celah kontrak.
                                              Menambahkannya lagi akan membuat dua definisi untuk
                                              satu endpoint.
    validation-matrix (FIN-VAL-1.4)         — FIN-VAL-132 sudah memuat kedua kode penanda pada
                                              daftar kode yang tertahan ratifikasi; gerbang
                                              FIN-OQ-035 masuk daftar itu tanpa aturan baru.
    state-transition-matrix (FIN-STATE-1.3) — nol status Finance baru; status shift milik Billing.
    permission-audit-matrix (FIN-PERM-1.2)  — nol Resource/Action baru. CATATAN: FIN-OQ-036
                                              (granularitas hak akses butir menu) BILA dijawab
                                              dapat menyentuh berkas ini; belum sekarang.
  STATUS revisi 6: keempat sumbu yang bergerak di sana (INTEGRATION-1.4, VAL-1.4, TEST-1.4,
  MVP-1.5) DISETUJUI owner 28 September 2026 bersama FIN-DES-051..058 — label `draft` pada blok
  contract_versions_revision_6 di bawah sudah digantikan baris ini.
contract_versions_revision_6: >
  DISETUJUI owner 28 September 2026 (lihat contract_versions_revision_7). Yang BERGERAK pada
  revisi 6 hanya EMPAT sumbu:
    integration-contract: FIN-INTEGRATION-1.4 (bagian 5.10 baru, bagian 6 dan 7 diperbarui)
    validation-matrix:    FIN-VAL-1.4 (FIN-VAL-133..142 baru; 130/131/132 diperbarui)
    acceptance-test:      FIN-TEST-1.4 (bagian D baru)
    prd-to-mvp:           FIN-MVP-1.5 (bagian 21-25 baru)
  TIGA sumbu SENGAJA TIDAK DISUNTING dan `last_changed_in`-nya tetap tertinggal di belakang,
  sesuai aturan bahwa contract yang isinya tidak bergerak MUST NOT disunting hanya untuk
  menaikkan angka:
    api-contract (FIN-API-1.2)             — nol endpoint baru, nol endpoint berubah bentuk.
                                             Penyempitan nilai deductionType adalah validasi,
                                             bukan perubahan bentuk API.
    state-transition-matrix (FIN-STATE-1.3) — nol status Finance baru. Status shift/deposit/refund
                                             yang dirujuk amendment ini SELURUHNYA milik Billing.
    permission-audit-matrix (FIN-PERM-1.2)  — nol Resource dan nol Action baru; seluruh kejadian
                                             terbit sebagai efek samping operasi yang hak aksesnya
                                             sudah ada.
contract_versions_revision_5: >
  Seluruhnya locked 26 September 2026 bersama FIN-DES-045..050: FIN-API-1.2, FIN-INTEGRATION-1.3,
  FIN-STATE-1.3, FIN-VAL-1.3, FIN-PERM-1.2, FIN-TEST-1.3, FIN-MVP-1.4. Nilai pada blok
  contract_versions di atas adalah versi revisi 4 dan sudah digantikan baris ini.

artifact_hashes:
  note: >
    SHA256 dipotong 16 karakter pertama, dihitung 28 September 2026 sesudah revisi 6 ditulis.
    Dipakai mendeteksi drift: berkas yang hash-nya berubah tanpa revisi manifest ikut naik berarti
    disunting di luar jalur skill.
  00-interview-decisions.md: 0bfbc759c527f092        # amendment pass kedua 28 Sep: FIN-DEC-072..076
  01-existing-capability-map.md: 3489cf24461bb582     # bagian 16: audit BE-FIN-036 di working tree
  02-backend-architecture.md: 59bb49efa8604d98       # REVISI 7 + koreksi rujukan FIN-VAL-123 -> 137
  03-frontend-architecture.md: 118c1044c3a71731      # REVISI 7 (bagian 15)
  04-prd-to-mvp.md: 35c387a452ac9536                 # FIN-MVP-1.6 (bagian 26-30)
  contracts/integration-contract.md: c53a78149f5d9f63  # FIN-INTEGRATION-1.5 + koreksi rujukan FIN-VAL-137
  contracts/validation-matrix.md: 2a350fad6d5652db     # FIN-VAL-1.4 — TIDAK bergerak pada revisi 7
  testing/acceptance-test-matrix.md: 83ad7295aee3a51d   # FIN-TEST-1.5 (bagian E)
  erd/accounting-integration.md: 4fb771c06c9f1369       # bagian 7 baru
  erd/data-dictionary.md: 3bc759b8e26d2fe0              # C.8 FinSupplierReturn + DDL
contract_versions_note_revision_4: >
  Enam kontrak turunan plus prd-to-mvp naik versi 25 September 2026 karena revisi 4 menambah
  endpoint/status/aturan/hak-akses/epic baru. SELURUHNYA DIKUNCI owner 25 September 2026
  bersama approval FIN-DES-037..044 (lihat status_note_revision_4). Label `draft` pada entri
  contract_versions di atas sudah tidak berlaku — status yang berlaku adalah `locked`.
  Konsekuensi: kerja paralel backend-frontend DIIZINKAN untuk task EPIC FIN-15/16/17.
contract_versions_note_revision_3: >
  Lima kontrak naik ke 1.1 pada 25 September 2026; dua TIDAK bergerak dan sengaja tidak disunting
  (api-contract, permission-audit-matrix) sesuai aturan bahwa file contract yang isinya tidak
  berubah MUST NOT disunting hanya untuk menaikkan angka. `last_changed_in` masing-masing tertulis
  di kepala berkasnya.
  Kelima yang naik DIKUNCI owner pada 25 September 2026 lewat pernyataan "Saya approve semua",
  sama seperti penguncian 1.0 pada 20 September 2026. Dicatat apa adanya.
  Konsekuensi langsung: kerja paralel backend-frontend DIIZINKAN untuk task yang kontraknya
  terkunci, termasuk rumpun Accounting Integration — KECUALI bagian yang bergantung pada nama
  tujuh kode kejadian, yang tetap menunggu FIN-OQ-017.
contract_lock_note_revision_3: >
  Diperbarui 25 September 2026. Dari tiga permukaan yang tidak terkunci pada 1.0, SATU sudah
  tertutup: BilCollectionHandoff kini ada dan sudah dikonsumsi (FIN-CAP-007, FIN-CAP-025).
  Yang tersisa dua — perluasan BilArHandoff untuk manfaat karyawan, dan pengaktifan pengiriman
  ke Accounting — ditambah SATU permukaan baru: katalog tujuh kode kejadian (FIN-OQ-017), yang
  bentuk pesannya sudah final tetapi nama kodenya belum diratifikasi Accounting.
  Kelima kontrak yang naik ke 1.1 BELUM dikunci; penguncian adalah tindakan owner terpisah.
contract_lock_note: >
  Owner menyetujui penguncian ke 1.0 pada 20 September 2026, atas usulan
  roadmap/00-delivery-roadmap.md bagian 2. Dicatat apa adanya, TIDAK ditetapkan skill.
  TIGA PERMUKAAN TETAP TIDAK TERKUNCI, dan alasannya bukan status FIN-DES melainkan
  ketergantungan pada pihak lain:
    (1) BilCollectionHandoff — bentuknya milik owner Billing, belum dikonfirmasi (FIN-DEC-005)
        — CATATAN REVISI 3: butir ini sudah TERTUTUP, lihat contract_lock_note_revision_3;
    (2) perluasan BilArHandoff untuk manfaat karyawan — FIN-DEC-006/FIN-DEC-016 belum turun;
    (3) pengiriman kejadian ke Accounting — EPIC FIN-12 OPEN DECISION (FIN-CAP-018, FIN-DEC-007).
  Perubahan pada ketiganya TIDAK menaikkan versi kontrak; ia menambah permukaan baru yang
  dikunci tersendiri saat jawabannya turun.
  Konsekuensi langsung penguncian: kerja paralel backend-frontend kini DIIZINKAN untuk task
  yang kontraknya terkunci.
external_contract_dependencies:
  ACC-XMOD-0.2: >
    Kontrak Finance → Accounting milik modul Accounting. Diratifikasi sisi Finance lewat
    FIN-DEC-001. Finance MUST mengikuti bentuk 12 field kontrak itu; setiap perubahan bentuk
    pesan adalah wewenang Accounting, bukan blueprint ini.
  BIL-CASH-001: >
    Blueprint billing-kasir. Sumber BilArHandoff/BilApHandoff/BilTender/BilSettlement/
    BilPaymentAllocation/BilCashierShift, DAN pemilik seluruh keputusan operasional Petty Cash
    (PC-DEC-*/PC-DES-*) per FIN-DEC-009.

design_decision_ids: [FIN-DES-001, FIN-DES-002, FIN-DES-003, FIN-DES-004, FIN-DES-005, FIN-DES-006, FIN-DES-007, FIN-DES-008, FIN-DES-009, FIN-DES-010, FIN-DES-011, FIN-DES-012, FIN-DES-013, FIN-DES-014, FIN-DES-015, FIN-DES-016, FIN-DES-017, FIN-DES-018, FIN-DES-019, FIN-DES-020, FIN-DES-021, FIN-DES-022, FIN-DES-023, FIN-DES-024, FIN-DES-025, FIN-DES-026, FIN-DES-027, FIN-DES-028, FIN-DES-029, FIN-DES-030, FIN-DES-031, FIN-DES-032, FIN-DES-033, FIN-DES-034, FIN-DES-035, FIN-DES-036, FIN-DES-037, FIN-DES-038, FIN-DES-039, FIN-DES-040, FIN-DES-041, FIN-DES-042, FIN-DES-043, FIN-DES-044, FIN-DES-045, FIN-DES-046, FIN-DES-047, FIN-DES-048, FIN-DES-049, FIN-DES-050, FIN-DES-051, FIN-DES-052, FIN-DES-053, FIN-DES-054, FIN-DES-055, FIN-DES-056, FIN-DES-057, FIN-DES-058, FIN-DES-059, FIN-DES-060, FIN-DES-064, FIN-DES-065]
design_decision_status_revision_9: >
  FIN-DES-064 dan FIN-DES-065 `approved` 29 September 2026 oleh Yasmin (Product Owner Finance)
  lewat FIN-DEC-080 dan FIN-DEC-081 (Amendment Pass). Ringkas:
    FIN-DES-064  Mutasi RELEASE dipetakan ke PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT (D Piutang,
                 K Uang Muka Pasien), bukan PENGEMBALIAN-UANG-MUKA; PENGEMBALIAN-UANG-MUKA
                 dipersempit ke BilRefundCase EXECUTED. Mengoreksi FIN-DEC-041.
    FIN-DES-065  Aturan intake mutasi RELEASE: berpasangan REVERSAL ber-SettlementId sama
                 menerbitkan kejadian; RELEASE tanpa pasangan ditolak fail-closed sebagai intake
                 ERROR (menunjuk FIN-OQ-037 / evidence/19).
design_decision_status_revision_7: >
  FIN-DES-059 dan FIN-DES-060 `draft`, 28 September 2026, BELUM disetujui owner. Ringkas:
    FIN-DES-059  Gerbang worker kedua kode penanda shift; bentuk pesan (Amount = 0) TIDAK diubah;
                 jalan pintas nilai simbolis DITOLAK owner; menunggu FIN-OQ-035
    FIN-DES-060  Menu Purchasing diselaraskan ke FIN-DEC-060; urutan wajib backend GET / →
                 layar → menu; satu ketidakcocokan hak akses diangkat sebagai FIN-OQ-036
  FIN-DES-051..058 `approved` 28 September 2026 — lihat status_note_revision_7.
design_decision_status_revision_6: >
  FIN-DES-051..058 `approved` 28 September 2026 ("Saya approve"); sebelumnya `draft`. Ringkas:
    FIN-DES-051  Penamaan ulang lima kode; nol migration (konstanta saja)
    FIN-DES-052  Kode potongan AR dipilih dari DeductionType; OTHER DITOLAK sampai kodenya ada
    FIN-DES-053  Kejadian selisih kas satu per shift per siklus; kunci = Id shift, SourceVersion dipatok
    FIN-DES-054  Penanda shift tertutup: pemicu CLOSED+REVIEWED, Amount 0 diizinkan lewat daftar
                 tertutup, ditambah kode pembalik untuk shift yang dibuka kembali
    FIN-DES-055  Kolom PPNAmount pada FinSupplierReturn + kode PPN-MASUKAN-RETUR-PEMBELIAN
    FIN-DES-056  Refund SETTLEMENT masuk kode yang sudah ada; REFERRED_OUTPATIENT_ADMIN ditahan
                 dengan baris intake ERROR yang berbunyi
    FIN-DES-057  Kode pembalikan pemakaian uang muka disiapkan; pemicunya tertahan gap Billing
    FIN-DES-058  Empat pelurusan bentuk pesan, termasuk 5 titik tulis nama kode dan properti
                 Components yang dihilangkan
design_decision_ids_note_revision_4: >
  Array ini sebelumnya berhenti di FIN-DES-028 walau revisi 3 sudah menambah FIN-DES-029..036 —
  kesenjangan pencatatan dari pass sebelumnya, DIPERBAIKI di sini sekaligus menambah
  FIN-DES-037..044 dari revisi 4. Status approval masing-masing tetap mengikuti catatan di
  design_decision_status/_revision_2/_revision_3 di atas dan status_note_revision_4 (dekat
  bagian atas berkas ini) — FIN-DES-029..044 seluruhnya `draft`, belum disetujui owner.
design_decision_status_revision_2: >
  FIN-DES-025..028 `approved` 20 September 2026 oleh Yasmin (Product/Domain Owner Finance),
  lewat pernyataan langsung "Setujui FIN-DES-025..028". Dicatat apa adanya, TIDAK ditetapkan
  skill. Approval ini diberikan TERPISAH dari approval revisi 1, sebagaimana seharusnya —
  keempatnya tidak pernah dianggap ikut terangkat oleh approval keputusan Medical Fee di
  hulunya.
  Approval ini BUKAN otorisasi untuk membuat atau menjalankan migration, dan BUKAN otorisasi
  memulai BE-FIN-020: task itu masih tertahan FIN-OQ-010 (ambang nominal approval AP).
  SATU keputusan revisi 1 DIPERSEMPIT: FIN-DES-015 (satu entity FinPayment untuk supplier dan
  dokter) tetap berlaku, tetapi invariant "TotalAmount = jumlah alokasi" kini didampingi
  invariant kedua untuk nilai transfer bersih — lihat FIN-DES-027. Tidak ada keputusan revisi 1
  yang dicabut.
design_decision_status: >
  SELURUH FIN-DES-001..024 `approved` 20 September 2026 oleh Yasmin (Product/Domain Owner
  Finance), lewat pernyataan langsung "Saya setuju FIN-DES-001-024".
  SATU di antaranya diperkuat penegasan terpisah pada hari yang sama: FIN-DES-003 (prefix `Mst`
  untuk entity yang berperan sebagai data induk di dalam FinanceManagement, `Fin` untuk entity
  lainnya). Owner menyatakannya sendiri sebagai aturan, bukan sekadar menyetujui usulan desain —
  sehingga aturan itu kini berlaku sebagai KETENTUAN MODUL, tidak hanya sebagai keputusan
  arsitektur satu pass. Konsekuensinya: penamaan `FinBank`/`FinBankAccount`/`FinCurrency` pada
  FIN-PRD-V2-0.3 bagian 11 resmi digantikan `MstBank`/`MstBankAccount`/`MstCurrency`/
  `MstExchangeRate`, dan entity master Finance berikutnya mengikuti aturan yang sama.

requirement_completeness_gate_note: >
  NOT_RUN, dan ini DEVIASI YANG DICATAT atas keputusan owner 20 September 2026, bukan langkah
  yang terlewat. Sebagai gantinya dipakai: (1) 00-interview-decisions.md dengan 23 keputusan
  approved lengkap beserta owner, evidence, dan tanggal; (2) 01-existing-capability-map.md
  dengan 19 kemampuan berbukti source langsung beserta path dan SHA. Pola pencatatan mengikuti
  preseden billing-kasir (requirement_completeness_gate_multi_payer).
  RISIKO YANG DITERIMA: requirement sisi HR untuk employee benefit belum pernah dinilai
  kelengkapannya oleh pemilik HR. Konsekuensinya diisolasi — lihat domain_architecture_readiness.

domain_architecture_readiness: >
  DOMAIN_ARCHITECTURE_NOT_RUN, deviasi tercatat atas keputusan owner 20 September 2026.
  Alasan yang dapat diuji:
    (1) Finance Management TIDAK mengambil keputusan klinis apa pun dan tidak berdampak pada
        keselamatan pasien — ia subledger keuangan di hilir.
    (2) Finance TIDAK PERNAH menulis balik ke modul sumber. Arahnya satu arah masuk
        (Billing → Finance, Medical Fee → Finance) dan satu arah keluar (Finance → Accounting).
        Larangan menulis ini eksplisit pada aturan bisnis #9 dan #12 serta FIN-OOS-001..004.
    (3) Seluruh batas lintas bounded context yang relevan SUDAH diputus eksplisit oleh keputusan
        bisnis: FIN-DEC-001/002/008/023 (Accounting), FIN-DEC-005/006/016 (Billing),
        FIN-DEC-003/019 (Medical Fee), FIN-DEC-009 (Petty Cash/billing-kasir),
        FIN-DEC-014 (Administrator/MstSupplier). Tidak ada batas domain tersisa yang perlu
        diselesaikan hospital-domain-architect.
  SATU RISIKO TERBUKA YANG TIDAK DITUTUP DEVIASI INI: rumpun employee benefit menyentuh HR
  (identitas pemilik manfaat dan eligibilitas), dan FIN-DEC-006 serta FIN-DEC-016 keduanya
  bertanda "butuh konfirmasi Billing + HR" yang belum turun. Karena itu seluruh rumpun employee
  benefit ditandai OPEN DECISION pada 04-prd-to-mvp.md dan MUST NOT masuk gelombang pengiriman
  mana pun sampai konfirmasi itu ada. Pemisahan ini yang membuat deviasi dapat diterima:
  bagian yang requirement-nya paling tipis justru yang paling tegas dikeluarkan dari MVP.

readiness_revision_6: >
  DESIGN_DRAFT untuk revisi 6; CONTRACTS_UNLOCKED pada empat sumbu yang bergerak
  (FIN-INTEGRATION-1.4, FIN-VAL-1.4, FIN-TEST-1.4, FIN-MVP-1.5). Baseline revisi 1-5 TIDAK berubah.
  YANG MENAHAN revisi 6 diteruskan ke /plan-module-delivery:
    (i)   Approval owner atas FIN-DES-051..058, DAN pengakuan atas tiga hal di status_note_revision_6.
    (ii)  FIN-OQ-032 — bertanda MEMBLOKIR FR-FIN-105. Bila kotak masuk Accounting tidak menerima
          Amount = 0, bentuk kedua kode penanda MUST dirancang ulang.
    (iii) FIN-OQ-034 — bertanda MEMBLOKIR kelengkapan EPIC FIN-14, dan pemiliknya owner BILLING.
  YANG TIDAK tertahan dan boleh berjalan lebih dulu bila owner menyetujui desainnya:
    - FIN-DES-051 dan FIN-DES-058 (penamaan ulang + pelurusan bentuk pesan) — nol ketergantungan
      pihak luar, dan justru MUST selesai sebelum worker pengiriman mana pun hidup.
    - FIN-DES-055 (kolom PPNAmount) — butuh otorisasi migration terpisah, bukan approval desain.
    - FIN-DES-052 (pemilihan kode potongan) — masuk BE-FIN-040 yang sudah direncanakan.
  PRASYARAT TAMBAHAN revisi 6:
    (j) ~~/trace-existing-capabilities untuk memperbarui capability map~~ — SELESAI 28 September
        2026 (capability map bagian 15). Hasilnya MENAMBAH prasyarat baru, bukan hanya menutup yang
        ini: FIN-CQ-04 memblokir FR-FIN-105 berbasis bukti, dan FIN-CQ-05 menuntut satu kalimat
        pada integration-contract.md 5.10.5 dibetulkan.
    (k) Otorisasi terpisah untuk SATU migration (AddPPNAmountToFinSupplierReturn).
    (l) Keputusan operasional atas baris outbox lama yang bernama pendek (dibuang atau ditulis ulang
        sebagai versi baru) — MUST diambil sebelum worker diaktifkan; menyentuh data yang sudah ada,
        sehingga di luar wewenang dokumen desain.
readiness_revision_3: >
  DESIGN_PARTIAL. Baseline revisi 1-2 tetap DESIGN_APPROVED dan CONTRACTS_LOCKED pada 1.0;
  revisi 3 berstatus DESIGN_DRAFT dan CONTRACTS_UNLOCKED pada 1.1.
  YANG SUDAH TERBUKA sejak readiness lama ditulis:
    - Blocker BILLING-COLLECTION-HANDOFF TERTUTUP (impact scan 25 September 2026), sehingga
      MVP-2 dan MVP-3 kini juga dapat direncanakan. Seluruh MVP-0..MVP-5 tidak lagi menunggu
      pihak luar.
    - FIN-OQ-011 (nama field saldo subledger) TERTUTUP oleh FIN-DEC-035.
  YANG MENAHAN revisi 3, dan HANYA menyentuh rumpun Accounting Integration + Collection:
    (i)  Approval owner atas FIN-DES-029..036 dan atas penguncian lima kontrak ke 1.1 — belum ada.
    (ii) Ratifikasi owner Accounting atas TUJUH kode kejadian baru (FIN-OQ-017). Ini yang membuat
         EPIC FIN-14 berstatus OPEN DECISION dan membuat perubahan FIN-DEC-030 belum boleh
         dieksekusi di source, walaupun keputusan bisnisnya sudah approved dan buktinya lengkap.
  PRASYARAT IMPLEMENTASI TAMBAHAN revisi 3, di luar yang sudah tercatat di readiness lama:
    (f) Otorisasi terpisah untuk SATU migration pengubah check constraint
        CK_FinBillingHandoffIntake_HandoffType (FIN-DES-029). Desain ini TIDAK memberi wewenang itu.
    (g) Pembacaan jumlah baris berstatus HELD_FOR_FINALIZATION sebelum pengiriman diaktifkan
        (02-backend-architecture.md bagian B.6) — pembacaan database, wewenangnya tetap terpisah.
    (h) Audit field-per-field rumpun AR/AP/Payable Finance lewat trace-existing-capabilities
        sebelum blueprint dipakai sebagai acuan penuh AR/AP.
readiness: >
  DESIGN_APPROVED dan CONTRACTS_LOCKED. Seluruh FIN-DES-001..028 disetujui owner 20 September
  2026, dan tujuh kontrak turunan dikunci ke 1.0 pada hari yang sama.
  Roadmap pengiriman FIN-ROADMAP-001 sudah diturunkan; MVP-0, MVP-1, MVP-4, dan MVP-5 siap
  dimulai.
  YANG MASIH MENAHAN /plan-module-delivery: satu ketergantungan lintas modul, yaitu konfirmasi
  owner Billing atas bentuk BilCollectionHandoff (FIN-DEC-005). Ia menahan gelombang MVP-2 saja.
  Gelombang MVP-0 (data induk) dan MVP-1 (pintu masuk fakta Billing + buku piutang) TIDAK
  bergantung padanya dan sudah dapat direncanakan.
  PRASYARAT IMPLEMENTASI yang TERSISA dan MUST diminta terpisah saat eksekusi (BUKAN blocker
  perencanaan):
    (a) Pendaftaran folder submodul baru pada docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md
        sebelum file model pertama ditulis (QBE-MOD-003, QBE-NAM-004) — lihat FIN-DES-002.
    (b) Otorisasi terpisah untuk membuat dan menjalankan migration (AGENTS.md, Keselamatan
        Database). Desain ini TIDAK memberi wewenang itu.
    (c) Konfirmasi owner Billing atas FIN-DEC-005 (BilCollectionHandoff) dan FIN-DEC-006
        (perluasan BilArHandoff) sebelum task lintas repository/lintas modul dimulai.
    (d) Konfirmasi owner HR atas FIN-DEC-016 sebelum rumpun employee benefit dimulai.
    (e) ~~FIN-OQ-010 (ambang nominal approval AP) sebelum aturan validasi angka dikunci.~~
        DITUTUP 25 September 2026 — Rp 50.000.000, FIN-DEC-052. Konsisten dengan placeholder
        yang SUDAH ditulis di FinancePaymentService.ResolveApprovalTier (FIN-CAP-035).
readiness_note_rumpun_baru: >
  25 September 2026 (pass ketiga — /design-business-module). Tiga scope BARU (FIN-SC-008
  Purchasing/AP, FIN-SC-009 AR Invoice Agregat, FIN-SC-010 Potongan AR) kini berstatus
  DESIGN_DRAFT — 02-backend-architecture.md AMENDMENT REVISI 4 (FIN-DES-037..044),
  03-frontend-architecture.md bagian 12, erd/purchasing-ap.md (baru) dan amendment pada
  erd/payable.md serta erd/receivable-collection.md, enam kontrak turunan naik versi,
  04-prd-to-mvp.md EPIC FIN-15/16/17. DESIGN_APPROVED dan CONTRACTS_LOCKED 25 September 2026
  (status_note_revision_4); roadmap diturunkan pada pass /plan-module-delivery yang sama.
  DIPERBARUI SESUDAH pass desain ini, 25 September 2026: FIN-OQ-020 (ratifikasi Accounting atas
  kode PPN-MASUKAN-PEMBELIAN) semula memblokir SELURUH EPIC FIN-15 masuk /plan-module-delivery
  (FIN-DEC-046). Owner meninjau ulang dan meminta gerbang itu dipersempit (FIN-DEC-056) —
  alasannya: Finance adalah titik asal (upstream) bagi Accounting, dan pola outbox transaksional
  yang sama persis dengan EPIC FIN-11 (kotak keluar sudah berjalan MVP-0..5 tanpa endpoint
  Accounting ada) sudah cukup mengisolasi risikonya. Sekarang EPIC FIN-15 BOLEH diteruskan penuh
  ke /plan-module-delivery; yang TETAP tertahan FIN-OQ-020 hanya AKTIVASI WORKER pengiriman
  kode PPN Masukan itu sendiri, bukan epic-nya. EPIC FIN-16 (AR Invoice Agregat) dan FIN-17
  (Potongan AR) tetap tidak pernah tertahan gerbang ini — keduanya murni internal Finance.

blocking_questions:
  - id: BILLING-COLLECTION-HANDOFF
    status: CLOSED 2026-09-25
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Ditutup lewat impact scan 25 September 2026: BilCollectionHandoff
      sudah dibangun owner Billing (BKC-DES-037, 22 September 2026) DAN sudah dikonsumsi
      FinanceBillingIntakeService.SyncNewFactsAsync/ProcessAsync (BE-FIN-016). Bukti:
      01-existing-capability-map.md FIN-CAP-007 dan FIN-CAP-025, diverifikasi pada d6cdfaf9.
      Gelombang MVP-2 dan MVP-3 kini setara MVP-0/MVP-1 dari sisi ketergantungan luar.
      Sisa yang belum diverifikasi: kecocokan field-per-field bentuk kolomnya terhadap
      contracts/integration-contract.md bagian 2.1 — bukan blocker, cukup pemeriksaan saat task
      MVP-2 dimulai.
  - id: FIN-OQ-010
    blocking_for: EPIC FIN-09 (Pembayaran AP) — hanya aturan validasi angkanya, bukan modelnya. POST-MVP
  - id: FIN-OQ-011
    status: CLOSED 2026-09-25
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Accounting menetapkan bentuk pesan saldo subledger pada 24 September
      2026 dan Finance menerimanya apa adanya (FIN-DEC-035). Bentuk finalnya ada di
      contracts/integration-contract.md bagian 5.6.
  - id: FIN-OQ-017
    status: CLOSED 2026-09-28
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Accounting meratifikasi ketujuh kode lewat evidence/14: lima apa
      adanya, PENGAKUAN-KELEBIHAN-BAYAR bersyarat (syaratnya diterima FIN-DEC-067), dan
      SELISIH-KAS-SHIFT diganti dua kode baru yang namanya diterima FIN-DEC-064. EPIC FIN-14 masih
      di luar gelombang (menunggu design pass), tetapi bukan lagi karena open question ini.
  - id: FIN-OQ-016
    status: OPEN
    blocking_for: >
      Pengaktifan pengiriman (EPIC FIN-12) dan gerbang cutover G3. Mekanisme token/kredensial akun
      layanan menunggu Platform; TIGA syarat organisasinya sudah ditetapkan FIN-DEC-036 dan
      tercatat di contracts/integration-contract.md bagian 5.1.
  - id: FIN-OQ-018
    status: Sisi keputusan bisnis CLOSED 2026-09-28 (FIN-DEC-071); sisi ratifikasi kode kini FIN-OQ-031
    blocking_for: >
      Accounting menanyakan langsung lewat evidence/14 bagian 4.2/7.6. Finance menjawab dengan
      asumsi defensif: kedua kategori BISA mengeluarkan kas, perlu kode baru — TETAPI ini asumsi,
      BUKAN konfirmasi faktual owner Billing (lihat catatan risiko FIN-DEC-071). Bentuk/nama kode
      belum diusulkan; itu FIN-OQ-031, menunggu /design-business-module lebih dulu.
  - id: FIN-CQ-03
    blocking_for: EPIC FIN-04 (Employee benefit AR) — menunggu konfirmasi Billing + HR
  - id: FIN-CAP-021
    blocking_for: EPIC FIN-08 (Utang dokter) — menunggu modul Medical Fee dibangun
  - id: FIN-OQ-020
    status: CLOSED sisi ratifikasi kode 2026-09-28 (evidence/14 bagian 2)
    blocking_for: >
      TIDAK LAGI MEMBLOKIR apa pun. PPN-MASUKAN-PEMBELIAN diratifikasi Accounting; akun debit
      persis (aset PPN Masukan vs beban tidak dapat dikreditkan) menjadi urusan G2 Accounting
      sendiri, bukan open question Finance lagi. Worker pengiriman boleh diaktifkan begitu G2
      Accounting menetapkan aturan postingnya — tidak ada lagi yang ditunggu dari sisi Finance.
  - id: FIN-OQ-022
    status: CLOSED 2026-09-25 (/grill-me, FIN-DEC-060)
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Submenu "Pembelian" + butir flat "Tagihan Gabungan Penjamin" +
      label "Faktur Pembelian" diputuskan. Implementasi menyusul BE-FIN-032/033/037, BE-FIN-039.
  - id: FIN-OQ-023
    status: CLOSED sisi keputusan bisnis 2026-09-25 (/grill-me, FIN-DEC-057)
    blocking_for: >
      Deposit Retur dipakai sebagai baris alokasi non-tunai di dalam FinPayment — keputusan
      bisnis SUDAH ADA. YANG MASIH MENAHAN BE-FIN-036: konsekuensi skema (FinPaymentAllocation/
      FinSupplierReturnDepositUsage perlu penanda sumber alokasi) BELUM digambar
      /design-business-module. BE-FIN-036 tetap ⛔ sampai amendment arsitektur berikutnya.
  - id: FIN-OQ-024
    status: CLOSED sisi Finance 2026-09-25 (/grill-me, FIN-DEC-058)
    blocking_for: >
      Kode POTONGAN-PIUTANG-NON-TUNAI (ke-26) diusulkan. Ratifikasi Rizki dicatat terpisah
      sebagai FIN-OQ-026 — hanya menahan aktivasi worker (pola FIN-DEC-056), TIDAK menahan
      BE-FIN-040/FE-FIN-013 dari /plan-module-delivery. Surat evidence belum dikirim.
  - id: FIN-OQ-025
    status: CLOSED 2026-09-25 (/grill-me, FIN-DEC-059)
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. GET /purchasing/reports/aging dicabut dari FIN-API-1.1; layar
      memakai GET api/finance/payable/aging existing. BE-FIN-037 membangun empat laporan lain.
  - id: FIN-OQ-026
    status: CLOSED 2026-09-28, digantikan FIN-OQ-028
    blocking_for: >
      TIDAK LAGI RELEVAN dalam bentuk lama. Accounting menolak POTONGAN-PIUTANG-NON-TUNAI sebagai
      satu kode (evidence/14 bagian 3.5) dan memintanya dipecah jadi empat kode (FIN-DEC-065).
      Ratifikasi yang ditunggu sekarang tercatat sebagai FIN-OQ-028.
  - id: FIN-OQ-027
    status: OPEN — dibuka /grill-me 2026-09-28
    blocking_for: >
      Ratifikasi Rizki atas kode baru PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT (FIN-DEC-063).
      Hanya menahan aktivasi worker pengiriman kasus pembatalan uang muka yang sudah terpakai,
      tidak menahan EPIC FIN-14 secara keseluruhan. Surat SUDAH DIKIRIM 2026-09-28 lewat
      evidence/15 bagian 5.
  - id: FIN-OQ-028
    status: OPEN — dibuka /grill-me 2026-09-28, menggantikan FIN-OQ-026
    blocking_for: >
      Ratifikasi Rizki atas empat kode potongan piutang pengganti (FIN-DEC-065):
      POTONGAN-PPH23-PIUTANG, pembaliknya, POTONGAN-BIAYA-BANK-PIUTANG, pembaliknya. Hanya
      menahan aktivasi worker, tidak menahan EPIC FIN-17 masuk /plan-module-delivery.
  - id: FIN-OQ-029
    status: OPEN — dibuka /grill-me 2026-09-28
    blocking_for: >
      Ratifikasi Rizki atas kode baru PPN-MASUKAN-RETUR-PEMBELIAN (FIN-DEC-068). Hanya menahan
      aktivasi worker pengiriman retur pembelian yang membawa PPN.
  - id: FIN-OQ-030
    status: OPEN — dibuka /grill-me 2026-09-28
    blocking_for: >
      Ratifikasi Rizki atas kode baru PENUTUPAN-SHIFT-KASIR (FIN-DEC-070). Menahan penegakan
      ACC-DEC-065 sisi Accounting (tutup bulan menahan shift yang belum ditutup). Bentuk skemanya
      juga belum digambar /design-business-module.
  - id: FIN-OQ-031
    status: OPEN — DIPERSEMPIT /design-business-module 2026-09-28 (FIN-DES-056)
    blocking_for: >
      DIPERSEMPIT dari dua SourceType menjadi SATU. Refund SETTLEMENT ternyata TIDAK butuh kode
      baru: buktinya BillingAllocationService baris 287-330 — kredit itu lahir dari uang yang
      benar-benar diterima melebihi tagihan, ekonominya identik ALLOCATION_EXCESS, sehingga cukup
      memperluas cakupan PENGAKUAN-KELEBIHAN-BAYAR dan PENGEMBALIAN-UANG-MUKA. Penyempitan ini
      MUST diakui owner karena FIN-DEC-071 memerintahkan kode baru untuk keduanya.
      YANG TERSISA: akun debit refund kredit REFERRED_OUTPATIENT_ADMIN. Bukan wewenang Finance —
      bergantung pada apakah pendapatan administrasi rawat jalan dibalik saat kredit lahir
      (BKC-DEC-119), yaitu kebijakan Billing + Accounting. Sementara itu baris intake ditulis ERROR
      dan nol kejadian diterbitkan (FIN-VAL-141).
  - id: FIN-OQ-032
    status: >
      DIPERSEMPIT /grill-me 2026-09-28 (FIN-DEC-075) — butir (2) DIPINDAH ke FIN-OQ-035 karena
      levelnya berbeda (perubahan aturan validasi, bukan ratifikasi nama)
    blocking_for: >
      SEKARANG MURNI ratifikasi kode BARU PEMBALIKAN-PENUTUPAN-SHIFT-KASIR, yang tidak pernah
      diusulkan sebelumnya dan lahir karena CashierShiftService.ReopenAsync mengizinkan shift
      CLOSED/REVIEWED dibuka kembali; tanpa penanda pembalik, Accounting mengizinkan tutup bulan
      atas periode yang kembali terbuka. Menahan aktivasi worker kode ini saja.
  - id: FIN-CQ-08
    status: OPEN — dibuka /trace-existing-capabilities 2026-09-28 (bagian 16.3), FIN-CAP-043 `Repair`
    blocking_for: >
      PENAMAAN RESOURCE HAK AKSES MODUL FINANCE TIDAK SAMA ANTARA KODE DAN KONTRAK.
      Enam menyimpang: kontrak menyebut FinancePayment/FinanceReceipt/FinanceReceivable/
      FinanceSupplierPayable/FinanceBillingIntake/FinanceAccountingEvent, kode memakai
      Payment/Receipt/Receivable/SupplierPayable/BillingIntake/AccountingEvents.
      Tujuh cocok (seluruh rumpun Purchasing yang dibangun terakhir + FinanceBankDeposit +
      FinanceDailyCash). Enam hanya ada di kode dan TIDAK ADA di kontrak: Finance.AP, Finance.AR
      (keduanya DIPAKAI frontend untuk menyaring butir menu), BankAccount, Currency,
      PettyCashBudget, PettyCashCategory.
      BUKAN dibuat BE-FIN-036 — seluruh 11 atribut pada FinancePaymentsController sudah memakai nama
      pendek sejak sebelum task itu. Polanya: controller BARU mengikuti kontrak, controller LAMA
      tidak; modul sedang di tengah migrasi penamaan dan kontrak mendokumentasikan SASARAN.
      AKIBAT MATERIAL: penyemaian peran dari permission-audit-matrix.md akan memberikan nama yang
      TIDAK ADA di kode, sehingga endpoint pembayaran/penerimaan/piutang TIDAK DAPAT dipanggil.
      MEMPERBESAR FIN-OQ-036: persoalannya bukan hanya granularitas butir menu, melainkan dua
      resource yang diandalkan frontend tidak pernah masuk kontrak hak akses.
      MUST diputuskan owner bersama Security Owner SEBELUM peran disemai ke lingkungan mana pun.
      Audit tidak memilih.
  - id: FIN-OQ-036
    status: OPEN — dibuka /design-business-module revisi 7, 2026-09-28. DIPERBESAR /trace 2026-09-28 (lihat FIN-CQ-08)
    blocking_for: >
      Granularitas resource hak akses butir menu Purchasing. Butir menu dijaga Finance.AP (payung),
      sedangkan endpoint-nya menuntut resource granular (FinancePurchaseOrder, FinanceGoodsReceipt,
      FinanceInvoiceExchange, FinancePurchasingInvoice, FinanceSupplierReturn) — keduanya ADA di
      backend, jadi bukan nama karangan, tetapi granularitasnya berbeda sehingga pengguna dapat
      melihat butir menu lalu ditolak endpoint.
      Pilihannya: (a) butir menu memakai resource granular yang sama dengan endpoint-nya; atau
      (b) peran yang diberi Finance.AP selalu diberi kelima resource granular lewat seeder peran.
      MENYENTUH HAK AKSES, sehingga BUKAN DEV_DISCRETION — MUST diputuskan owner bersama Security
      Owner. MEMBLOKIR FR-FIN-110; TIDAK memblokir FR-FIN-108 (relabel menu) maupun FR-FIN-109
      (endpoint daftar). Menyembunyikan butir menu dengan menebak resource-nya sendiri MUST NOT
      dilakukan.
  - id: FIN-OQ-035
    status: OPEN — dibuka /grill-me 2026-09-28 (FIN-DEC-075), memisahkan diri dari FIN-OQ-032 butir (2). **Surat SUDAH DIKIRIM** 2026-09-28 lewat evidence/16
    blocking_for: >
      Permintaan agar kotak masuk Accounting memperluas validasinya menerima Amount = 0 untuk
      PENUTUPAN-SHIFT-KASIR/PEMBALIKAN-PENUTUPAN-SHIFT-KASIR, ATAU mengaktifkan jalur pesan saldo
      (BE-ACC-P2-028). DIVERIFIKASI LANGSUNG ke source (AccAccountingEventService.cs baris 1098,
      1075-1079): kotak masuk HARI INI menolak KEDUA jalur. Bukan spekulasi — bukti baca langsung.
      Surat SUDAH DIKIRIM 2026-09-28 lewat evidence/16, terpisah dari evidence/15 (ratifikasi nama kode) karena
      ini meminta perubahan aturan validasi Accounting sendiri, levelnya lebih besar.
      MEMBLOKIR FR-FIN-105 sisi aktivasi worker; TIDAK memblokir penulisan baris outbox (pola
      FIN-DEC-056) maupun EPIC FIN-14 masuk perencanaan (EPIC itu sendiri sudah OPEN DECISION
      karena sebab lain).
  - id: FIN-CQ-04
    status: CLOSED sisi keputusan bisnis 2026-09-28 (FIN-DEC-075); sisi teknis diteruskan FIN-OQ-035
    blocking_for: >
      Ditemukan /trace-existing-capabilities: kotak masuk Accounting menolak Amount<=0 (400) untuk
      pesan non-saldo DAN menolak pesan saldo (409, jalur belum dibangun) — tidak ada celah yang
      lolos untuk kode penanda bernilai nol FIN-DES-054. Yasmin memutuskan LANJUTKAN desain apa
      adanya + kirim permintaan tertulis (bukan tunda FR-FIN-105, bukan pakai nilai simbolis).
      Rinciannya 01-existing-capability-map.md bagian 15.4.
  - id: FIN-CQ-05
    status: OPEN — dibuka /trace-existing-capabilities 2026-09-28. Tidak memblokir, tidak butuh keputusan owner
    blocking_for: >
      TIDAK MEMBLOKIR dan TIDAK butuh interview — murni koreksi kalimat dokumen berbasis bukti.
      contracts/integration-contract.md bagian 5.10.5 butir 1 menulis akibat properti Components
      bernilai null sebagai "Setiap pesan berpotensi ditolak 400". Source penerima menunjukkan
      null DITERIMA (normalisasi ?? new List<>). Dibetulkan saat /design-business-module berikutnya
      menyentuh berkas itu.
  - id: FIN-CQ-06
    status: CLOSED 2026-09-28 (FIN-DEC-076)
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Yasmin memutuskan: FIN-DEC-060 TETAP berlaku, menu yang sudah dibangun
      ("Account Payable", "Purchase Order", dst., bahasa Inggris) MUST disesuaikan lewat task
      frontend terpisah (relabel + tambah submenu "Pembelian" + butir "Tagihan Gabungan Penjamin").
      Keputusan TIDAK diubah mengikuti implementasi yang mendahuluinya. Task ini belum tercatat di
      roadmap manapun — MUST masuk /plan-module-delivery berikutnya.
  - id: FIN-CQ-07
    status: OPEN — dibuka /trace-existing-capabilities 2026-09-28
    blocking_for: >
      Butir menu "Purchase Order" dan "Receiving" mengarah ke /finance/payable?tab=po dan
      ?tab=receiving, sementara frontend NOL memanggil endpoint purchasing manapun, DAN kelima
      controller Purchasing tidak punya GET / berpaging. Petugas dapat mencapai butir menu yang
      tidak dapat menampilkan data. Urutan penutupannya: backend (GET / berpaging) lebih dulu,
      baru frontend. MUST ditutup sebelum rumpun Purchasing dinyatakan selesai.
  - id: FIN-OQ-033
    status: OPEN — dibuka /design-business-module 2026-09-28
    blocking_for: >
      Kode kejadian untuk potongan piutang berjenis OTHER. Pemecahan satu kode menjadi dua
      (FIN-DEC-065) meninggalkan DeductionType = OTHER tanpa akun debit yang sah — sebelum
      pemecahan ia menumpang POTONGAN-PIUTANG-NON-TUNAI. Sementara itu OTHER DITOLAK validasi
      (FIN-VAL-137), bukan diterima tanpa kejadian. Tidak memblokir BE-FIN-040.
  - id: FIN-OQ-034
    status: CLOSED TUNTAS 2026-09-28 (FIN-DEC-077 + evidence/18 dari Billing), berdasarkan BKC-DEC-128..131
    blocking_for: >
      TIDAK LAGI MEMBLOKIR — DITUTUP DARI KEDUA SISI. Billing membalas evidence/17 lewat evidence/18:
      saat tender top-up deposit REVERSED dan saldo tidak cukup, sistem membatalkan alokasi tagihan
      LIFO otomatis (baris kompensasi ReversesAllocationId, constraint Amount>0 tetap terjaga),
      menulis DUA mutasi BilDepositMovement (RELEASE untuk alokasi dibatalkan, REVERSAL untuk top-up
      ditarik dengan ReversesMovementId terisi ke TOP_UP asal), menyelaraskan invoice CLOSED -> FINAL
      via SyncClosureAsync, dan menjamin AvailableBalance >= 0. NOL migration.
      DIVERIFIKASI LANGSUNG ke BillingSettlementService.cs working tree (bukan sekadar klaim surat):
      simbol HandleDepositTopUpReversalAsync, ReversesAllocationId, SyncClosureAsync,
      PaymentReversed seluruhnya ADA dan berkorespondensi dengan narasi evidence/18.
      Finance mengonsumsi kedua mutasi lewat intake DEPOSIT_MOVEMENT untuk menerbitkan
      PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT (dari RELEASE) dan PEMBALIKAN-PENERIMAAN-UANG-MUKA
      (dari REVERSAL). Pendeteksi FIN-VAL-142 sekarang akan LOLOS, bukan lagi jalur mati.
      Billing menyatakan BE-FIN-036 UNBLOCKED. Kontrak sisi Billing: BIL-INT-018 (DEPOSIT_MOVEMENT).
  - id: FIN-CQ-08
    status: >
      CLOSED 2026-09-28 (FIN-DEC-078, FIN-DEC-079, FIN-DES-061..063, FIN-PERM-1.3);
      SEBAGIAN DIKOREKSI 2026-09-29 (FIN-DEC-082, FIN-DEC-083, FIN-DES-066..069, FIN-PERM-1.4).
      Yang dikoreksi: nama resource payung (Finance.AP/AR sudah dipakai controller V2 yang berjalan,
      sehingga payung memakai Finance.AP.Umbrella/Finance.AR.Umbrella) dan titik tulis ekspansi
      (AccessMenuSeeder tidak pernah menulis SysAccessPolicy, sehingga pindah ke
      RoleAccessController.ApplyPoliciesAsync saat admin memberi grant). Yang TIDAK berubah:
      penyelarasan enam controller legacy (butir 2 di bawah), yang sudah diimplementasikan
      BE-FIN-042 beserta skrip migrasi datanya. Satu gerbang baru: FIN-OQ-038 (pembawa resource
      payung di registry), menahan HANYA implementasi mekanisme ekspansi.
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Selesai dirancang pada pass /design-business-module (28 September 2026):
      (1) Pemetaan resmi payung-ke-granular telah ditetapkan pada permission-audit-matrix.md
      (FIN-PERM-1.3, Bagian D), memetakan Finance.AP ke 9 resource granular dan Finance.AR ke 4
      resource granular, beserta aturan pewarisan aksi View->Read, Operate->Maker, Approve->Checker.
      (2) Penyelarasan enam controller legacy telah dikunci pada FIN-DES-062 (Payment->FinancePayment,
      Receipt->FinanceReceipt, Receivable->FinanceReceivable, SupplierPayable->FinanceSupplierPayable,
      BillingIntake->FinanceBillingIntake, AccountingEvents->FinanceAccountingEvent).
      (3) Strategi migrasi data peran SysRolePermissions berbasis skrip SQL idempotent telah
      ditetapkan pada FIN-DES-063 dan siap dieksekusi bersamaan dengan task build-module-backend.
  - id: FIN-OQ-036
    status: CLOSED 2026-09-28 (FIN-DEC-079), sebelumnya dibuka /design-business-module revisi 7
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Diselesaikan lewat pemetaan payung-ke-granular (Finance.AP/AR mencakup
      resource granular di bawahnya), BUKAN lewat mengganti filter menu frontend ke resource
      granular. FR-FIN-110 boleh dilanjutkan sesudah pemetaan konkretnya digambar.
```

## Daftar artefak blueprint

> **Pembaruan revisi 6 (28 September 2026).** Sembilan berkas berubah: `00-interview-decisions.md`
> (oleh `/grill-me`), `02-backend-architecture.md` (**AMENDMENT REVISI 6**, `E.1`-`E.13`),
> `03-frontend-architecture.md` (bagian 14), `04-prd-to-mvp.md` (`FIN-MVP-1.5`, bagian 21-25),
> `contracts/integration-contract.md` (`FIN-INTEGRATION-1.4`, bagian 5.10),
> `contracts/validation-matrix.md` (`FIN-VAL-1.4`, bagian D),
> `testing/acceptance-test-matrix.md` (`FIN-TEST-1.4`, bagian D),
> `erd/accounting-integration.md` (bagian 7 baru — bagian 6 dinyatakan tidak lagi terkini),
> `erd/data-dictionary.md` (`C.8` `FinSupplierReturn` naik menjadi `Diperbarui`).
> Tiga kontrak **sengaja tidak disunting**: `api-contract.md`, `state-transition-matrix.md`,
> `permission-audit-matrix.md` — isinya tidak bergerak, lihat `contract_versions_revision_6`.
> `01-existing-capability-map.md` **tidak disentuh** dan kini **STALE** — itu keluaran
> `/trace-existing-capabilities`, bukan skill desain.

| Berkas | Status | Keterangan |
|---|---|---|
| `blueprint-manifest.md` | Ada | Berkas ini |
| `00-interview-decisions.md` | Ada | Keluaran `/grill-me`, **71 keputusan** `FIN-DEC-001`..`071`; enam `superseded` (`004`, `007` sebagian, `015`, `023`, `032`, `033`, `058`). Closure pass 28 September 2026 menutup balasan Accounting `evidence/14` |
| `01-existing-capability-map.md` | Ada — **diperbarui 28 September 2026** | Keluaran `/trace-existing-capabilities`, **41 kemampuan** berbukti. **Bagian 15 baru** — impact scan terarah pada `cba60cb0`/`49b59cfaa`: tujuh koreksi status (`FIN-CAP-018` `Missing`→`Ready to reuse`, `028`/`029` `Missing`→`Extend`), dua koreksi klaim lama, empat `Conflict` baru (`FIN-CQ-04`..`07`), lima entri baru (`FIN-CAP-037`..`041`), dan **kontrak as-is endpoint penerima Accounting** beserta 12 aturan penolakannya |
| `02-backend-architecture.md` | Ada | Arsitektur backend, `FIN-DES-001`..`058`. **AMENDMENT REVISI 6** (`FIN-DES-051`..`058`, `draft`) di akhir dokumen — penyelarasan katalog kejadian dengan ratifikasi Accounting, nol tabel baru, satu tabel diperbarui, empat temuan yang membatalkan asumsi keputusan bisnis (`E.1`) |
| `03-frontend-architecture.md` | Ada | Kontrak fungsional frontend dan matriks kewenangan UI. **Bagian 12 baru** (revisi 4) — layar Purchasing/AP, Batch Tagihan AR, Potongan Penerimaan |
| `04-prd-to-mvp.md` | Ada | `FIN-MVP-1.3` `draft` — batas rilis, epic, UAT, DoD. **`EPIC FIN-15`/`16`/`17` baru** (`POST-MVP`, revisi 4), `FR-FIN-081`..`095` baru; `EPIC FIN-07` digantikan `FIN-15`; `EPIC FIN-14` (revisi 3) tetap `OPEN DECISION`. **`EPIC FIN-15` boleh masuk `/plan-module-delivery` tanpa menunggu `FIN-OQ-020`** (`FIN-DEC-056`) |
| `erd/00-context-erd.md` | Ada | Peta antar bounded context. **Diperbarui revisi 4** — konteks `FIN_PURCHASING` ditambahkan |
| `erd/receivable-collection.md` | Ada | ERD rumpun Piutang dan Penerimaan. **AMENDMENT REVISI 4** (bagian D) — AR Invoice Agregat, Potongan AR |
| `erd/payable.md` | Ada | ERD rumpun Utang dan Pembayaran. **AMENDMENT REVISI 4** — perluasan `FinSupplierPayable` |
| `erd/purchasing-ap.md` | **Ada (baru, revisi 4)** | ERD rumpun Purchasing/AP — Purchase Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian, Deposit Retur |
| `erd/cash-and-master-data.md` | Ada | ERD rumpun Kas dan Master Data Finance |
| `erd/accounting-integration.md` | Ada | ERD rumpun Integrasi Accounting. Bagian 4 (`HELD_FOR_FINALIZATION`) **dicabut** revisi 3, dipertahankan sebagai jejak sejarah |
| `erd/data-dictionary.md` | Ada | Kamus data seluruh kolom dan bentuk DDL. `HandoffType` naik dari 4 ke **8 nilai** (revisi 3). **AMENDMENT REVISI 4** (bagian C.1-C.16) — 14 tabel baru + 1 diperbarui, DDL lengkap |
| `contracts/api-contract.md` | Ada | **`FIN-API-1.1`** `draft` (revisi 4) — tujuh grup endpoint baru Purchasing/AP + Receivable Invoice Batch, dua endpoint baru grup Receipt |
| `contracts/state-transition-matrix.md` | Ada | **`FIN-STATE-1.2`** `draft` — satu transisi dicabut (bagian 9), empat `HandoffType` ditambah (bagian 1, revisi 1.1); **tujuh entity baru** (bagian B, revisi 4) |
| `contracts/validation-matrix.md` | Ada | **`FIN-VAL-1.2`** `draft` — `FIN-VAL-076` dicabut, `FIN-VAL-078`..`086` ditambah (revisi 1.1); **`FIN-VAL-100`..`122` ditambah** (revisi 4) |
| `contracts/integration-contract.md` | Ada | **`FIN-INTEGRATION-1.2`** `draft` — katalog 17→24 kode, bagian 2a baru, bagian 5.5 diganti total (revisi 1.1); **bagian 5.8 baru, kode ke-25** (revisi 4) |
| `contracts/permission-audit-matrix.md` | Ada | **`FIN-PERM-1.4`** `draft` (Revisi 7, 29 September 2026) — mengoreksi D.2 (nama payung menjadi `Finance.AP.Umbrella`/`Finance.AR.Umbrella`, `FIN-DES-066`), D.6.1 (skrip SQL `SysRolePermissions` DICABUT — tabelnya tidak ada; penggantinya sudah ditulis `BE-FIN-042`), dan D.6.2 (ekspansi pindah ke `RoleAccessController`, `FIN-DES-067`). Sebelumnya `FIN-PERM-1.3` `approved` (Revisi 6, 28 September 2026): pemetaan payung ke granular (`FIN-DES-061`), penyelarasan 6 controller legacy (`FIN-DES-062`, **sudah diimplementasikan**), skrip migrasi data peran (`FIN-DES-063`) |
| `testing/acceptance-test-matrix.md` | Ada | **`FIN-TEST-1.2`** `draft` — bagian 8a baru (25 skenario uang muka/deposit/selisih kas, revisi 1.1); **bagian B.1-B.7 baru** (revisi 4) |
| `evidence/01-jawaban-untuk-owner-accounting.md` | Ada | Jawaban Finance atas enam pertanyaan Rizki (paket 15 September 2026), berdiri sendiri |
| `evidence/02-permintaan-kontrak-untuk-owner-billing.md` | Ada | Permintaan `BilCollectionHandoff` dan perluasan `BilArHandoff`, berdiri sendiri. Bagian 3 juga ditujukan ke owner HR |
| `evidence/04-jawaban-atas-balasan-accounting.md` | Ada | Jawaban Finance atas `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md`, 25 September 2026 — persetujuan `FIN-DEC-030`, empat usulan kode baru (dikoreksi `evidence/05`), berdiri sendiri |
| `evidence/05-koreksi-jawaban-atas-balasan-accounting.md` | Ada | Koreksi atas `evidence/04` — kode `PEMAKAIAN-UANG-MUKA-DEPOSIT` dipersempit, dua kode baru (`PENGEMBALIAN-UANG-MUKA`, `PENGAKUAN-KELEBIHAN-BAYAR`) dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA` ditambah, total tujuh kode menunggu ratifikasi Accounting (`FIN-OQ-017`), berdiri sendiri |
| `evidence/06-usulan-kode-ppn-masukan-untuk-accounting.md` | Ada | Usulan kode kejadian baru `PPN-MASUKAN-PEMBELIAN` (kode ke-25) untuk rumpun Purchasing/AP, dipicu evidence `Keuangan.md` (`FIN-DEC-045`, `046`, `053`), menutup sisi Finance `FIN-OQ-020`, berdiri sendiri, 25 September 2026 |
| `evidence/15-balasan-finance-atas-ratifikasi-accounting.md` | **Ada (baru, revisi 7)** | Balasan Finance atas `accounting/evidence/14`, 28 September 2026 — ratifikasi diterima seluruhnya, nama final enam kode pecahan, jawaban tujuh pertanyaan bagian 7, **empat kode baru** diusulkan, tiga perubahan perilaku disampaikan, dan **dua koreksi atas anggapan bersama** (pemicu penanda shift tidak boleh hanya `REVIEWED`; skenario pertanyaan 7.1 belum dapat terjadi karena gap Billing). Berdiri sendiri |
| `evidence/16-permintaan-perluasan-validasi-kotak-masuk-accounting.md` | **Ada (baru, revisi 7)** | Permintaan agar kotak masuk Accounting menerima `Amount = 0` untuk kedua kode penanda, atau mengaktifkan jalur pesan saldo (`BE-ACC-P2-028`). Sengaja **dipisah** dari `evidence/15` karena levelnya perubahan aturan validasi, bukan ratifikasi nama. Memuat bukti baris source dan alasan penolakan jalan pintas nilai simbolis (`FIN-OQ-035`) |
| `evidence/17-permintaan-perbaikan-pembalikan-tender-deposit-untuk-billing.md` | **Ada (baru)** | Laporan temuan + permintaan perbaikan ke **owner Billing**, 28 September 2026 — pembalikan tender top-up deposit tidak menulis mutasi deposit apa pun, sehingga saldo deposit mencatat uang yang tidak pernah diterima dan dua kejadian pembalikan Finance tidak pernah terbit. Memuat contoh berangka, tiga kemungkinan perbaikan sebagai bahan diskusi (tanpa memilih), dan pendeteksi baca-saja yang Finance pasang sementara (`FIN-OQ-034`) |
| `evidence/18-balasan-billing-atas-perbaikan-pembalikan-tender-deposit.md` | **Ada (masuk dari Billing)** | Balasan Billing atas `evidence/17`, 28 September 2026 — `FIN-OQ-034` **DITUTUP TUNTAS**. Solusi: pembatalan alokasi LIFO otomatis saat defisit saldo, dua mutasi `BilDepositMovement` (`RELEASE`+`REVERSAL`), `SyncClosureAsync` mengembalikan invoice ke `FINAL`, saldo dijamin `>= 0`, nol migration. Klaim teknisnya **diverifikasi langsung** ke `BillingSettlementService.cs` working tree — simbol kunci ditemukan cocok. Billing menyatakan `BE-FIN-036` unblocked |
| `evidence/07-usulan-empat-kode-potongan-retur-deposit-untuk-accounting.md` | Ada | Usulan kode ke-26 s.d. 29 (`POTONGAN-PIUTANG-NON-TUNAI`, pembaliknya, `RETUR-PEMBELIAN`, `PEMAKAIAN-DEPOSIT-RETUR`), menutup sisi Finance `FIN-OQ-026`, 26 September 2026. Juga memberi tahu Accounting soal nama kode alias yang sudah ditulis source |
| `roadmap/00-delivery-roadmap.md` | Ada | `FIN-ROADMAP-001` revisi 8, status `ACTIVE` — payung: gelombang, traceability, coverage gap, risiko. Revisi 8 menurunkan AMENDMENT REVISI 6 dan 8 (44 task BE, 14 task FE) |
| `roadmap/01-backend-roadmap.md` | Ada | `FIN-ROADMAP-BE-001` revisi 6 — 44 task `BE-FIN-*` (`042`..`044` untuk revisi 6/8: granular FIN-CQ-08/controller/seeder/SQL, PPNAmount FinSupplierReturn, katalog akuntansi), urutan eksekusi, DoD backend |
| `roadmap/02-frontend-roadmap.md` | Ada | `FIN-ROADMAP-FE-001` revisi 6 — 14 task `FE-FIN-*` (`014` ditambahkan: penyelarasan menu sidebar navigasi Finance ke FIN-DEC-060/FIN-DES-060), kebutuhan UI brief, `DEV_DISCRETION`, DoD frontend |

Sub-pohon `task/report/` belum ada dan memang bukan keluaran pass perencanaan; ia menyusul dari
kedua skill build.

## Pemicu impact scan

Blueprint ini menjadi **stale** dan MUST diperiksa ulang bila salah satu terjadi:

| Pemicu | Yang harus diperiksa ulang |
|---|---|
| Backend SHA bergerak dari **`cba60cb0`** (bukan lagi `d6cdfaf9`/`96bf9746`) | Seluruh klaim as-is pada `01-existing-capability-map.md`. **Map sudah STALE sekarang** pada `FIN-CAP-018` dan rumpun Purchasing/AP — lihat `impact_scan_note_revision_6` |
| Frontend SHA bergerak dari **`49b59cfaa`** (bukan lagi `abed49b03`) | `03-frontend-architecture.md` bagian rute dan menu yang sudah ada. **Belum diperiksa pada `49b59cfaa`** — revisi 6 tidak menyentuh rute/menu, sehingga pemeriksaannya ditunda ke pass frontend berikutnya |
| Accounting mendaftarkan kode baru sebagai `AccEventType`, atau menolak `Amount = 0` untuk kode penanda | `contracts/integration-contract.md` bagian 5.10.4, `02-backend-architecture.md` `FIN-DES-054`, dan `FR-FIN-105` pada `04-prd-to-mvp.md`. **Penolakan `Amount = 0` menuntut perancangan ulang bentuk penanda**, bukan sekadar mengganti angka |
| Billing menutup gap pembalikan tender top-up deposit (`FIN-OQ-034`) | `FIN-DES-057`, `FIN-VAL-142`, dan kelengkapan `EPIC FIN-14`. Kode `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` baru punya sumber fakta sesudah itu |
| Rumpun AR/AP/Payable Finance dipakai sebagai acuan desain | **Audit field-per-field belum dilakukan** untuk 59 berkas yang dibangun `BE-FIN-001`..`021`. Jalankan `trace-existing-capabilities` lebih dulu — lihat capability map bagian 9.3 "Yang TIDAK ditutup" |
| Accounting meratifikasi atau mengoreksi nama tujuh kode baru | `contracts/integration-contract.md` bagian 5.4, `contracts/validation-matrix.md` `FIN-VAL-075`, `testing/acceptance-test-matrix.md` bagian 8a, dan `EPIC FIN-14` pada `04-prd-to-mvp.md` |
| Accounting meratifikasi atau mengoreksi kode `PPN-MASUKAN-PEMBELIAN` (kode ke-25) | `contracts/integration-contract.md` bagian 5.8, `contracts/validation-matrix.md` `FIN-VAL-122`; membuka `EPIC FIN-15` masuk `/plan-module-delivery` (`FIN-OQ-020`, `FIN-DEC-046`) |
| Frontend SHA bergerak dari `abed49b03` | `03-frontend-architecture.md` bagian rute dan menu yang sudah ada |
| `billing-kasir` menaikkan revisi yang menyentuh `BilArHandoff`, `BilTender`, `BilSettlement`, atau `BilPaymentAllocation` | `contracts/integration-contract.md` bagian intake Billing |
| `accounting` menaikkan `ACC-XMOD` di atas `0.2` | `contracts/integration-contract.md` bagian Finance → Accounting, dan `FIN-DEC-001` |
| Endpoint penerima Accounting Event mulai dibangun | `FIN-CAP-018` pada capability map berubah dari `Missing`; gelombang `MVP-5` bisa dimulai |
| Salah satu `input_hashes` berubah | Seluruh artefak desain — dokumen hulu bergerak tanpa revisi manifest |
