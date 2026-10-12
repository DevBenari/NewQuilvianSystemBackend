# ISSUE-001 — Belum semua item checkout klinis sampai ke invoice kasir

```yaml
issue_id: ISSUE-BKC-001
module_id: billing-kasir
submodule: null
layar: "Invoice & Billing Kasir; checkout dokter/perawat dan layanan penerima order"
sumber_laporan: "Permintaan audit pemilik 12 Oktober 2026; satu screenshot daftar kasir"
tanggal_issue: "2026-10-12"
status: TERBUKA
keparahan_tertinggi: High
source_sha_backend: "05634d1f498a57c990789367521c8b430e08c32e (MHamzah)"
source_sha_frontend: "7d44526ff11822e04d2ed558994f602986b4e2ef (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md
```

## 1. Ringkasan

**Belum semua item terintegrasi sampai invoice kasir.** Terdapat jalur yang sudah diimplementasikan untuk tindakan medis, laboratorium, radiologi, konsultasi dokter, pemakaian alat, dan komponen operasi, tetapi keberhasilannya tetap bergantung pada tahap pelayanan, tarif, dan sinkronisasi. Intervensi keperawatan, pemakaian obat/Alkes tanpa resep, Bank Darah, serta Gizi belum mempunyai rangkaian otomatis lengkap ke invoice yang dibaca kasir. Resep justru mempunyai dua pengirim dengan aturan berbeda sehingga dapat menghasilkan konflik atau tagihan sebelum waktunya. Daftar kasir juga dapat memakai perhitungan lama; kolom “Biaya & Tgl Bayar” sendiri berisi **uang yang telah dibayar**, sehingga angka nol pada screenshot bukan bukti tunggal kehilangan tagihan.

Audit memecah satu pertanyaan pelapor menjadi delapan kelompok temuan dan satu temuan tambahan. Beberapa celah sudah dinyatakan sebagai kemampuan yang ditunda dalam kontrak; temuan tersebut tidak disebut sebagai penyimpangan implementasi. Tidak ada perbaikan source atau perubahan database dalam pekerjaan ini.

### 1.1 Cakupan dan batas bukti

- `SUDAH-VERIFIKASI`: alur source frontend/backend, kontrak blueprint, dan uji unit terarah yang benar-benar dijalankan.
- `DARI-CAPTURE`: daftar kasir menampilkan beberapa nilai nol, status belum bayar, dan satu sisa pembayaran yang positif. Data pasien tidak disalin.
- Belum ada pembuktian transaksi pasien tertentu, keadaan master tarif/kebijakan sinkronisasi, atau respons aplikasi yang sedang berjalan. Database dan browser tidak dijalankan/diperiksa dalam audit ini.
- Workspace mempunyai perubahan existing di admisi dan komponen pendukungnya. File inti billing/checkout yang dirujuk tidak termasuk perubahan existing tersebut; SHA di metadata menjadi basis kutipan. Tidak ada klaim bahwa aplikasi localhost pasti memakai SHA yang sama.
- Kepemilikan laporan ditetapkan dari route screenshot → `cashier-overview-view.jsx` → `useCashierBillingOverview` → `BillingInvoicesController.GetCashierOverview` → blueprint `billing-kasir/03-frontend-architecture.md`. Tidak ditemukan `05-skema-tampilan.md` pada root billing; desain layar yang tersedia adalah `03-frontend-architecture.md` dan kontraknya.

### 1.2 Matriks seluruh kategori yang ditemukan

“Jalur tersedia” berarti rangkaian implementasinya ditemukan, **bukan** hasil uji transaksi langsung. Checkout/order, pelayanan selesai, folio, dan invoice adalah empat keadaan berbeda. Folio merupakan catatan pengakuan pelayanan; kasir pada screenshot membaca invoice `BilInvoice` beserta item `BilInvoiceItem`.

| Kategori / langkah | Pemicu finansial pada source | Sampai invoice kasir? | Bukti / catatan |
| --- | --- | --- | --- |
| Order tindakan dokter/perawat rawat inap | Checkout membuat `Ordered`; bukan tagihan saat itu | Belum pada saat order; jalur tersedia setelah dieksekusi | E01–E03; bukan otomatis bug |
| Tindakan medis melalui execute resmi | `Completed` → fakta → folio → jembatan invoice | Jalur tersedia untuk rawat jalan/rawat inap, dengan tarif sah | E03–E05 |
| Tindakan/intervensi harian keperawatan | Pencatatan `IsBillable` hanya memberi `BillingDispatchStatus.Pending` | **Belum tersambung** | E06; ISS-01 |
| Resep obat reguler rawat jalan | Finalisasi konsultasi/resep; jumlah aktual saat dispense | Ada, tetapi dua pengirim dapat konflik | E07–E10; ISS-04 |
| Resep obat reguler rawat inap, harian, dan pulang | Jembatan menunggu dispense; pengirim langsung sudah menagih saat finalisasi | **Aturan saling bertentangan** | E07–E11; ISS-04 |
| Racikan dan resep campuran reguler + racikan | Resolver menolak seluruh resep; pengirim langsung memakai total termasuk bahan racikan | **Tidak konsisten**; bukan dukungan otomatis yang utuh | E09–E10; ISS-04 |
| Alkes/BMHP dari keranjang perawat | UI mencoba membuat draft pemakaian; fallback resep tidak lengkap | **Checkout bermasalah dan jalur tagihan belum lengkap** | E12–E14; ISS-02 |
| Pemakaian obat/Alkes pasien tanpa resep melalui Farmasi | `DrugUsage.RecordAsync` mengurangi stok dan memberi `NotBilled` | **Tidak ditemukan pengirim invoice otomatis** | E13–E14; ISS-02 |
| Pemberian dosis/MAR dan obat bawaan pasien | Pencatatan klinis; kontrak menyatakan MAR tidak menagih | Tidak menambah charge tersendiri; mencegah penagihan obat berulang | E11; jangan menganggap setiap catatan klinis wajib ditagih |
| Laboratorium | Spesimen diterima, fakta per pemeriksaan | Jalur tersedia; order saja belum menagih | E15, E04–E05 |
| Radiologi | Keputusan mutu citra; pemeriksaan yang layak ditagih | Jalur tersedia; ulang karena kesalahan internal tidak ditagih lagi | E16, E04–E05 |
| Konsumsi bahan Radiologi | Pencatatan `RadAcquisitionConsumption` | Tidak ditemukan charge bahan terpisah; belum diketahui apakah termasuk tarif pemeriksaan | E16; K-04 |
| Bank Darah/transfusi | Tindakan selesai → fakta `BloodBank` → folio | **Berhenti sebelum invoice kasir** | E17, E04, E18; ISS-03 |
| Konsultasi/asuhan Gizi | Order dan catatan asuhan, tanpa producer billing | **Belum ada jalur otomatis** | E19; ISS-05 |
| Hemodialisa | Finalisasi sesi billable membentuk tindakan → fakta `Procedure` | Jalur tersedia lewat tindakan; sesi stopped tidak otomatis ditagih | E20, E04–E05 |
| Pemakaian alat reusable/per waktu | Pemakaian diselesaikan melalui `FinishAsync` atau ditutup otomatis saat pasien keluar; fakta `EQUIPMENT_USAGE` | Jalur tersedia; berlaku juga untuk `PerUse`, berbeda dari keranjang BMHP | E21, E04–E05 |
| Kamar Operasi: tindakan, anestesi, sewa kamar, bahan terpakai | Kasus selesai; tindakan melalui execute, komponen melalui `OPERATING_ROOM` | Jalur tersedia per tindakan/komponen dengan tarif sah | E22, E04–E05 |
| Kamar rawat inap dan administrasi | Perhitungan Billing membaca hunian/kebijakan tarif | Jalur kalkulasi tersedia; tampilan daftar dapat tertinggal | E23–E24; ISS-07 |
| Gratis/FoC atau nonbillable | Resolver menandai `NotBillable` | Sengaja tidak menghasilkan charge | E05 |
| Biaya lain/manual kasir | `ADHOC` / `ADHOC_CATALOG` di Billing | Jalur manual tersedia, tidak membuktikan integrasi otomatis klinis | E18, E24 |
| Pelayanan IGD | Jembatan membatasi encounter ke rawat jalan/rawat inap | **Belum tercakup oleh jembatan ini**; resep langsung dapat masuk melalui jalur berbeda | E04, E07–E08; ISS-06 |
| Retur/koreksi setelah pelayanan | Fakta pembatalan/revisi; invoice final memakai adjustment | Jalur tersedia untuk resep dan bahan operasi yang terlacak; pemakaian tanpa resep belum lengkap | E25, E04 |

### 1.3 Proses bisnis yang ditelusuri

Tujuannya memastikan pelayanan yang layak ditagih tercatat sekali, masuk ke kunjungan yang tepat, dan terlihat oleh kasir. Dokter/perawat membuat order atau mencatat tindakan; unit layanan melaksanakan/menyerahkan; Billing menetapkan tarif dan penanggung; kasir membaca serta menerima pembayaran.

1. Dokter/perawat memilih item dan mengirim keranjang. Pada tindakan rawat inap ini baru membuat order. Pada resep ini membuat draft yang menempel pada catatan dokter.
2. Pelayanan mencapai pemicu tagih: tindakan selesai, spesimen diterima, citra dinilai layak, atau obat diserahkan. Resep rawat jalan mempunyai tahap tagihan awal ketika difinalkan.
3. Producer menyimpan fakta klinis dan menyerahkannya ke `BillingFolioService`.
4. Folio menyimpan efek pemrosesan. Untuk sumber yang didukung, jembatan mencari tarif dan meneruskan charge ke invoice kunjungan.
5. Billing menghitung ulang nilai item, kamar, administrasi, diskon, pajak, dan tanggungan penjamin/pasien. Kasir membaca invoice dan pembayaran, bukan menjumlah langsung seluruh tabel klinis.
6. Jika tarif belum ada, sumber tidak didukung, pengiriman gagal, atau invoice sudah final, hasilnya dapat berupa pengecualian, rekonsiliasi, atau adjustment. Keberhasilan klinis tidak dengan sendirinya berarti invoice sudah berubah.

| Dari | Pemicu | Keadaan hasil | Pelaku / akibat |
| --- | --- | --- | --- |
| Keranjang tindakan | Simpan order | `Ordered` | Dokter/perawat; belum eligible charge |
| Order tindakan | Execute resmi | `Completed` + fakta | Pelaksana yang berwenang; eligible charge |
| Draft resep | Finalisasi konsultasi | Resep final + fakta tahap awal | Dokter; aturan rawat jalan/rawat inap berbeda |
| Resep final | Dispense | Jumlah aktual + fakta versi baru | Farmasi; sumber aktual jumlah tertagih |
| Efek folio `Pending` | Sinkron berhasil | `Synced` + invoice/item | Billing |
| Efek folio | Tarif tidak ditemukan | `ReconciliationRequired` / `TARIFF_NOT_FOUND` | Harus ditangani sebelum finalisasi |
| Charge datang ke invoice final | Ajukan selisih | Adjustment menunggu proses Billing | Nilai belum otomatis menjadi tagihan efektif |

**Contoh:** dua tindakan masing-masing Rp100.000 yang baru dipesan belum menambah Rp200.000 ke invoice. Setelah keduanya benar-benar dikerjakan, tarif tersedia, dan sinkronisasi berhasil, nilai item menjadi Rp200.000 sebelum komponen lain. Resep tiga vial yang hanya diserahkan dua harus mengikuti jumlah aktual; catatan pemberian dua dosis oleh perawat tidak membuat charge kedua atas obat yang sama.

## 2. Laporan asli

| No. laporan | Pertanyaan pelapor | Lampiran |
| --- | --- | --- |
| 1 — tidak diberi nomor oleh pelapor | Audit seluruh item checkout dokter/perawat, termasuk tindakan, obat, dan lainnya: apakah terintegrasi billing dan muncul di kasir? | Satu screenshot route `/health-services/billing-management/billing/invoices/cashier-overview` |

Aturan baru yang ditetapkan pelapor: tidak ada. Permintaan audit mencakup semua kategori, bukan izin menulis source atau menetapkan kebijakan tarif baru.

## 3. Ringkasan temuan

Seluruh klaim perilaku di tabel ini `SUDAH-VERIFIKASI` pada source; dampak transaksi tertentu belum diuji langsung. Nomor pelapor tetap 1 meskipun analisis dipecah.

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ISS-BKC-001-01 | 1 | Intervensi keperawatan hanya menunggu pengiriman | DESIGN_CHANGE — kemampuan tertunda dalam kontrak | Clinical + Billing | High | SUDAH-VERIFIKASI | FIX-01 |
| ISS-BKC-001-02 | 1 | Keranjang Alkes tidak sesuai API; pemakaian tanpa resep belum mengirim tagihan | CONTRACT_MISMATCH + kemampuan belum tersedia | FE Keperawatan + Farmasi + Billing | High | SUDAH-VERIFIKASI | FIX-02, FIX-03 |
| ISS-BKC-001-03 | 1 | Bank Darah diakui folio, tidak diteruskan ke invoice | DESIGN_CHANGE — perluasan consumer | Billing + Bank Darah | High | SUDAH-VERIFIKASI | FIX-04 |
| ISS-BKC-001-04 | 1 | Dua pengirim resep memakai aturan berbeda | BUG + CONTRACT_MISMATCH | Farmasi + Billing | High | SUDAH-VERIFIKASI | FIX-05 |
| ISS-BKC-001-05 | 1 | Gizi belum mempunyai pemicu tagih | PERLU_KONFIRMASI + DESIGN_CHANGE | Gizi + Billing | Medium | SUDAH-VERIFIKASI untuk ketiadaan jalur; kebijakan belum diketahui | FIX-06 |
| ISS-BKC-001-06 | 1 | Jalur tindakan langsung Completed dan IGD melewati rangkaian invoice | BUG pada producer; PERLU_KONFIRMASI cakupan IGD | Clinical + IGD + Billing | High | SUDAH-VERIFIKASI | FIX-07, FIX-08 |
| ISS-BKC-001-07 | 1 | Daftar kasir memakai kalkulasi lama atau nol sebelum kalkulasi penjamin | BUG + keterbatasan proyeksi | Billing | High | SUDAH-VERIFIKASI | FIX-09 |
| ISS-BKC-001-08 | 1 | Angka nol pembayaran terlihat seperti biaya nol | DESIGN_CHANGE pada penjelasan UI | FE Billing | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | FIX-10 |

## 4. Rincian per temuan

### ISS-BKC-001-01 — Intervensi keperawatan belum diteruskan

**Jenis/area/keparahan:** `DESIGN_CHANGE`, Clinical + Billing, High. **Bukti:** `SUDAH-VERIFIKASI` E06. **Perbaikan:** `FIX-BKC-001-01`.

**Apa yang terjadi.** Tindakan harian yang ditandai dapat ditagih tersimpan secara klinis, tetapi tidak otomatis menjadi item kasir.

**Kenapa terjadi.** `NursingInterventionService.cs:289` dan `:439` menetapkan status `Pending`. Metode pencatatan hasil pengiriman pada `:879` hanya mempunyai deklarasi; pencarian seluruh source C# tidak menemukan pemanggil. Tidak ada sumber Nursing pada gerbang `BillingSourceContract`/resolver/jembatan. Dengan demikian status siap-kirim belum diikuti pengirim dan penerima invoice.

**Kesesuaian desain.** `keperawatan/contracts/integration-contract.md:84–93` secara eksplisit mengatakan kemampuan transaksi tujuan belum ada dan status tetap `Pending`. Ini celah kemampuan yang diketahui desain, bukan bukti bahwa builder melanggar kontrak tersebut.

**Dampak.** Dua intervensi billable yang selesai dapat tetap tidak menambah tagihan. Nilai kerugian aktual belum dihitung karena tarif dan data transaksi tidak diperiksa.

**Rekomendasi.** Pilih integrasi lewat fakta/folio/jembatan existing dengan identitas tindakan stabil. Hindari kasir mengetik ulang seluruh tindakan. Pemilik perlu menyetujui pemicu dan pemetaan tarifnya (K-01).

### ISS-BKC-001-02 — Alkes/BMHP dan pemakaian tanpa resep

**Jenis/area/keparahan:** `CONTRACT_MISMATCH`, FE Keperawatan + Farmasi + Billing, High. **Bukti:** `SUDAH-VERIFIKASI` E12–E14. **Perbaikan:** `FIX-BKC-001-02`, `03`.

**Apa yang terjadi.** Keranjang Alkes mempunyai pesan sukses, tetapi request utamanya mengirim identitas petugas/lokasi kosong. Bahkan jika draft berhasil dibuat, langkah itu belum mencatat pemakaian dan belum membentuk tagihan.

**Kenapa terjadi.** `order-alat-kesehatan-panel.jsx:191` mengirim GUID kosong untuk `storageLocationId`/`recordedByWorkforceId`, padahal `DrugUsageService.cs:164–168` dan `:458–480` memvalidasi master petugas serta lokasi; tidak ada pemilihan depot otomatis seperti komentar UI. UI hanya memanggil `createDrugUsage`, tidak memanggil `recordDrugUsage`. Pada fallback `:216` request resep tidak membawa `consultationId`, memakai `orderType` alih-alih `prescriptionOrderType`; `PrescriptionController.cs:747–750` mensyaratkan konsultasi yang sah. Di sisi lain, `DrugUsageService.RecordAsync` hanya mengurangi stok dan berakhir `NotBilled`; pencarian source Billing tidak menemukan konsumen `PhmDrugUsage` untuk membuat charge pemakaian tanpa resep.

**Kesesuaian desain.** Ketidaksesuaian request–DTO/validasi adalah `CONTRACT_MISMATCH`. Jalur otomatis pemakaian tanpa resep adalah kemampuan tambahan yang perlu dikontrak; `CONSUMABLE` yang diterima adapter bukan bukti ada producer pemakaian.

**Dampak.** Checkout spuit/kassa dapat ditolak sebelum tersimpan. Pemakaian yang dicatat melalui menu Farmasi dapat mengurangi stok namun belum menambah invoice. Contoh tiga spuit Rp5.000 tidak boleh diasumsikan sudah menambah Rp15.000 hanya karena transaksi bertanda siap ditagihkan.

**Rekomendasi.** Selaraskan checkout dengan API pemakaian yang sah dan pisahkan draft/order dari pencatatan aktual. Tambahkan pengiriman pemakaian nonresep ke Billing dengan kunci per item. Jangan mengubah jenis dokumen menjadi resep hanya karena request pertama gagal.

### ISS-BKC-001-03 — Bank Darah berhenti pada folio

**Jenis/area/keparahan:** `DESIGN_CHANGE`, Billing + Bank Darah, High. **Bukti:** `SUDAH-VERIFIKASI` E04, E17–E18. **Perbaikan:** `FIX-BKC-001-04`.

**Apa yang terjadi.** Tindakan Bank Darah selesai dapat mempunyai fakta biaya yang diterima Billing, tetapi belum menjadi item invoice kasir.

**Kenapa terjadi.** Producer mengirim `BloodBank`/`BloodBankCharge`; gerbang folio mengizinkannya. Namun `BillingClinicalChargeBridgeService.IsCandidateSourceContext` tidak memasukkan `BloodBank`. `BillingFolioService.cs:543` memberi status `NotApplicable` untuk sumber tersebut, sehingga worker invoice tidak mengambilnya. Adapter invoice/resolver juga belum menyediakan pemetaan Bank Darah. Keberhasilan handoff folio tidak sama dengan keberhasilan charge invoice.

**Kesesuaian desain.** Kontrak Bank Darah `integration-contract.md:82–96` memang menjamin sampai `RecognizeMilestoneAsync`, bukan invoice kasir. Perluasan consumer harus disetujui lintas pemilik. Jangan menagih per kantong jika keputusan existing menetapkan fakta per tindakan.

**Dampak.** Petugas melihat tindakan selesai dan penyerahan fakta berhasil, sementara total kasir belum bertambah.

**Rekomendasi.** Perluas jembatan, resolver, dan kontrak invoice dengan sumber Bank Darah yang eksplisit; pertahankan fakta serta idempotensi existing. Status UI perlu membedakan “fakta diterima” dan “masuk invoice”.

### ISS-BKC-001-04 — Dua jalur resep saling membantah

**Jenis/area/keparahan:** `BUG` + `CONTRACT_MISMATCH`, Farmasi + Billing, High. **Bukti:** `SUDAH-VERIFIKASI` E07–E11. **Perbaikan:** `FIX-BKC-001-05`.

**Apa yang terjadi.** Sebagian obat dapat masuk invoice, tetapi tidak melalui satu aturan yang konsisten. Resep di invoice merupakan satu baris per resep; rincian tiap obat tidak menjadi baris invoice tersendiri pada jalur ini.

**Kenapa terjadi.** `ConsultationFinalizationService.cs:251` menerbitkan fakta yang sudah memanggil folio dan jembatan; kemudian `:281` kembali memanggil `PrescriptionBillingChargeProducer` secara langsung.

1. Untuk resep rawat jalan yang berhasil dijembatani, keduanya memakai `PHARMACY`, ID resep, dan versi 1. Deskripsi berbeda: resolver “Resep …”, producer langsung “Obat resep … (… item)”. Deskripsi masuk hash pada `BillingInvoiceService.cs:2311`; versi sama dengan isi berbeda ditolak di `:1858`. Akibat yang dapat diturunkan dari source: pengiriman kedua dapat menghasilkan `PHM_CHARGE_FAILED` meskipun invoice pertama sudah tersimpan. Ini konflik, bukan bukti otomatis tagihan ganda.
2. Untuk rawat inap, jembatan pada `:211` menunda tahap awal sampai dispense. Producer langsung tidak memeriksa care setting dan menagih `TotalPrice` saat finalisasi. Ia melewati aturan tersebut.
3. Resolver pada `BillingSourceTariffResolver.cs:239` menolak seluruh resep yang memiliki racikan. Producer langsung justru menerima total agregat yang termasuk bahan racikan (`PrescriptionAggregateService.cs:83–86`). Penolakan racikan dapat hidup bersamaan dengan tagihan langsungnya; kebijakan koreksi/dispense tidak seragam.

**Kesesuaian desain.** `rawat-jalan/contracts/integration-contract.md` menetapkan jalur fakta sebagai batas resmi; `integrasi-billing/contracts/integration-contract.md:226–232` menetapkan obat ranap ditagih setelah diserahkan. Pengirim langsung melewati kedua batas. Aturan racikan yang final masih memerlukan keputusan K-02.

**Dampak.** Dokter dapat menerima keluhan billing meski tagihan sudah ada; obat rawat inap dapat tertagih sebelum diserahkan; resep campuran tidak dapat dinyatakan otomatis aman untuk seluruh item.

**Rekomendasi.** Satu jalur resmi lewat fakta/jembatan. Pertahankan adapter status `PRESCRIBED` untuk rawat jalan dan `DISPENSED` untuk rawat inap. Dukungan racikan wajib dikunci sebelum dianggap selesai; jangan menghapus penahan resolver tanpa aturan jumlah, tarif, dan retur bahan.

### ISS-BKC-001-05 — Gizi belum terintegrasi

**Jenis/area/keparahan:** `PERLU_KONFIRMASI` + `DESIGN_CHANGE`, Gizi + Billing, Medium. **Bukti:** `SUDAH-VERIFIKASI` E19 untuk ketiadaan jalur; kebijakan tagih belum diketahui. **Perbaikan:** `FIX-BKC-001-06`.

**Apa yang terjadi/kenapa.** Order konsultasi dan catatan asuhan Gizi disimpan, tetapi tidak ditemukan producer fakta atau invoice. Sumber Gizi juga belum terdaftar pada gerbang folio. Mapping label `NUTRITION` pada ringkasan Billing hanya mengelompokkan item jika sudah ada; mapping itu tidak membuat charge.

**Kesesuaian desain.** `gizi/00-interview-decisions.md:40` menempatkan tarif/tagihan di luar scope modul Gizi dan tetap milik Billing. Belum ada dasar untuk memutuskan otomatis menagih order, kunjungan pertama, setiap kunjungan, atau paket.

**Dampak.** Jika konsultasi gizi seharusnya ditagih tersendiri, kasir belum mendapatkannya otomatis. Jika sudah termasuk paket kamar, membuat charge baru justru berpotensi menagih dua kali.

**Rekomendasi.** Kunci K-03 dahulu, lalu integrasikan milestone yang dipilih. Jangan menyimpulkan kebijakan tarif dari tampilan order.

### ISS-BKC-001-06 — Completed langsung dan cakupan IGD

**Jenis/area/keparahan:** `BUG` pada producer; `PERLU_KONFIRMASI`/`DESIGN_CHANGE` cakupan IGD, Clinical + IGD + Billing, High. **Bukti:** `SUDAH-VERIFIKASI` E26, E03–E04. **Perbaikan:** `FIX-BKC-001-07`, `08`.

**Apa yang terjadi/kenapa.** `PatientProcedureController.cs:883` dapat membuat tindakan langsung `Completed`/executed lewat `ExecuteImmediately`, tetapi blok penyimpanan `:933–962` tidak menerbitkan fakta. `PatientProcedureOrderService.cs:437–496` juga mencatat tindakan keperawatan IGD langsung selesai tanpa producer. Execute resmi kemudian mengembalikan `AlreadyExecuted` sebelum emisi fakta (`PatientProcedureExecutionService.cs:91`), sehingga menekan execute lagi tidak memperbaikinya. Parameter `executeImmediately` tidak ditemukan pada caller FE saat ini; keberadaan cabang API terbukti, penggunaannya oleh UI belum terbukti.

Terpisah dari gap producer tersebut, jembatan pada `BillingClinicalChargeBridgeService.cs:138` mengecualikan `Emergency`. Jadi menambahkan producer IGD saja belum membuat invoice kasir. Konsultasi IGD juga dikecualikan dari emisi jasa konsultasi, sementara resep langsung tidak mempunyai pembatas itu.

**Kesesuaian desain.** Pemicu tindakan selesai seharusnya konsisten dengan execute resmi. Cakupan layanan IGD perlu keputusan pemilik K-05; jangan mencabut filter dan menggabungkan invoice IGD/ranap tanpa aturan episode/transfer.

**Dampak/rekomendasi.** Tindakan yang sudah selesai bisa tidak pernah mempunyai fakta untuk dijembatani. Satukan penerbitan untuk semua jalur Completed yang billable; lalu tangani cakupan IGD sebagai perubahan kontrak tersendiri. Tidak ada backfill transaksi lama yang dijalankan pada audit ini.

### ISS-BKC-001-07 — Angka daftar kasir dapat tertinggal

**Jenis/area/keparahan:** `BUG` + keterbatasan proyeksi, Billing, High. **Bukti:** `SUDAH-VERIFIKASI` E24. **Perbaikan:** `FIX-BKC-001-09`.

**Apa yang terjadi.** Item baru dapat sudah ada pada invoice, tetapi total/sisa pembayaran di daftar kasir belum mencerminkannya.

**Kenapa terjadi.** Upsert charge menyimpan item dan mengganti versi invoice tanpa membuat kalkulasi baru (`BillingInvoiceService.cs:1867–1884`, `:1990`). Daftar kasir mengambil kalkulasi terakhir berdasarkan nomor versi (`:642–662`), tanpa membandingkan perubahan item dengan waktu kalkulasi. Pada `:819`, hasil kalkulasi lama selalu didahulukan daripada total item terbaru. Bila belum ada kalkulasi, pasien tunai mendapat fallback jumlah item, sedangkan penjamin diberi nol karena coverage belum diketahui (`:818–820`). Tidak ada tanda pada row yang menjelaskan angka tersebut masih belum dihitung.

**Contoh.** Kalkulasi lama Rp100.000, kemudian tindakan Rp50.000 masuk: running item menjadi Rp150.000, tetapi daftar tetap dapat menampilkan kewajiban Rp100.000 sampai dihitung ulang. Untuk penjamin, Rp0 sebelum kalkulasi tidak membuktikan seluruh biaya ditanggung.

**Kesesuaian desain.** Desain daftar menyajikan outstanding sebagai informasi operasional; angka yang tertinggal tanpa penanda menyesatkan pengguna. Endpoint detail `calculation-preview` melakukan perhitungan live, sehingga daftar dan detail dapat berbeda.

**Rekomendasi.** Gunakan basis kalkulasi yang sama dan deteksi kekinian. Jika belum dapat menghasilkan angka sah, tampilkan “Belum dihitung/Perlu hitung ulang”; jangan membuat kewajiban penjamin terlihat pasti nol. Hindari kalkulasi transaksi satu per satu yang menulis database hanya karena daftar dibuka.

### ISS-BKC-001-08 — Makna kolom pada screenshot

**Jenis/area/keparahan:** `DESIGN_CHANGE`, FE Billing, Medium. **Bukti:** `SUDAH-VERIFIKASI` E27 dan `DARI-CAPTURE`. **Perbaikan:** `FIX-BKC-001-10`.

**Apa yang terjadi/kenapa.** `cashier-overview-view.jsx:135–154` berjudul “Biaya & Tgl Bayar” tetapi mengambil `totalPaidAmount` atau alias pembayaran. Tagihan Rp200.000 yang belum dibayar memang memberi Rp0 di kolom ini. Sisa pembayaran merupakan kewajiban pasien, bukan selalu seluruh invoice termasuk penjamin. Backend mengirim `TotalInvoiceAmount`, tetapi kolom tersebut tidak menampilkannya.

**Kesesuaian desain.** Uji existing justru memastikan kolom pembayaran terpisah dari sisa pembayaran. Tidak ada dasar menyebut mapping pembayaran ini bug; penjelasan/labelnya perlu diperjelas dan total tagihan dapat ditambahkan bila desain disetujui.

**Dampak/rekomendasi.** Pelapor dapat menyangka biaya hilang padahal angka yang dilihat adalah pembayaran. Pilih label “Sudah Dibayar & Tgl Bayar” dan sajikan “Total Tagihan” secara jelas. Jangan mengubah field menjadi total tagihan sambil mempertahankan makna tanggal/status pembayaran yang lama.

## 5. Tanya-jawab pelapor

> **T:** Apakah semua item checkout dokter/perawat sudah terintegrasi billing dan muncul di kasir?
>
> **J:** Belum. Jalur tindakan medis, Lab, Radiologi, alat, operasi, dan konsultasi sudah tersedia pada milestone tertentu. Intervensi keperawatan, Bank Darah, pemakaian nonresep/BMHP, dan Gizi belum lengkap ke invoice; resep dan proyeksi daftar juga mempunyai ketidakkonsistenan. Matriks bagian 1.2 menjelaskan setiap kategori.

> **Tersirat dari screenshot:** Apakah Rp0 membuktikan checkout tidak masuk?
>
> **J:** Tidak. Kolom itu berisi pembayaran. Untuk memastikan transaksi tertentu, cocokkan item invoice/detail, status sinkron, tahap pelayanan, dan kalkulasi terkini pada kunjungan yang sama. Keadaan database pasien pada screenshot belum diperiksa.

## 6. Temuan tambahan

### ISS-BKC-001-T1 — Penjaga finalisasi belum memeriksa semua sumber tertunda

**Kenapa dicantumkan:** melindungi kasir dari menutup invoice ketika charge klinis belum masuk. **Jenis/area/keparahan:** `BUG` pada kelengkapan penjaga, Billing, High. **Bukti:** `SUDAH-VERIFIKASI` E28. **Perbaikan:** `FIX-BKC-001-11`.

**Apa yang terjadi/kenapa.** `AreAllOrdersCompleteAsync` hanya memeriksa item invoice yang sudah ada (`BillingFinalizationService.cs:619`). Charge yang belum pernah menjadi item tidak ikut diperiksa. Penjaga efek folio khusus mencari `ReconciliationRequired` dengan kode `TARIFF_NOT_FOUND` (`:594–605`); pending/failed, sumber unsupported, atau racikan `SOURCE_REJECTED` tidak tercakup oleh pemeriksaan itu. `RequiresReview` yang ditulis kalkulasi hanya melindungi biaya kamar manual + otomatis, bukan seluruh kegagalan sinkron.

**Dampak.** Dengan syarat lain terpenuhi, invoice berpotensi difinalkan sebelum semua charge eligible telah diteruskan. Ini risiko yang diturunkan dari source, bukan klaim ada pasien yang sudah dirugikan. Invoice yang kemudian menerima selisih memerlukan adjustment.

**Kesesuaian desain/rekomendasi.** Kontrak existing melindungi kelengkapan order dan pengecualian finansial, tetapi perlu definisi eksplisit sumber tertunda yang harus menahan finalisasi. Tambahkan pemeriksaan sumber eligible yang belum selesai diproses, dengan pengecualian sah untuk gratis/nonbillable dan tahap belum eligible. Jangan menghalangi penyelesaian klinis karena gangguan Billing.

### 6.1 Bukti pengujian existing

Command yang dijalankan di frontend:

```text
node --import ./tests/helpers/register.mjs --test tests/unit/cashier-billing-overview.test.mjs tests/unit/doctor-billing-handoff-issues.test.mjs tests/unit/doctor-billing-summary.test.mjs tests/unit/inpatient-nursing-billing-summary.test.mjs tests/unit/blood-bank-procedure-billing-handoff.test.mjs
```

Hasil aktual: **39 test, 38 lulus, 1 gagal**, exit code 1. Test gagal `FE-RWI-094: Switcher sections menyambungkan NursingBillingSection ke case 'billing'` pada `tests/unit/inpatient-nursing-billing-summary.test.mjs:120`. Source pengalih menu saat ini tidak mengimpor/render section tersebut; komponen tagihan masih dipakai oleh workspace admisi. Ini **existing mismatch antara test dan susunan menu**, bukan bukti kegagalan pengiriman charge. Perlu dicocokkan dengan desain navigasi terkini sebelum memutuskan mengubah source atau test. Audit tidak memperbaikinya dan tidak menyebut seluruh suite lulus.

Unit test ini memvalidasi logika/presentasi tertentu; ia tidak membuktikan transaksi database atau integrasi ujung-ke-ujung. Build, migration, query database, backend runtime, dan checkout langsung tidak dijalankan.

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor

Tidak ada pertanyaan yang menahan kesimpulan audit source. Pembuktian apakah **transaksi tertentu** pada screenshot sudah lengkap memerlukan bukti detail invoice/status sinkron dari kunjungan yang sama; data tersebut belum diberikan. Identitas pasien tidak perlu dimuat ke dokumen audit.

### Untuk pemilik

| No. | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| --- | --- | --- | --- | --- |
| K-01 | Titik tagih dan pemetaan tarif intervensi keperawatan | Pencatatan aktual / finalisasi catatan / termasuk paket | Gunakan tindakan aktual yang eligible dan pemetaan tarif eksplisit; jangan menagih dua kali dengan order prosedur | FIX-01 |
| K-02 | Dukungan racikan | Billable bahan aktual + jasa racik / paket / manual terkontrol | Kunci jumlah serah, tarif, jasa racik, dan retur sebelum membuka otomatisasi | Bagian racikan FIX-05 |
| K-03 | Penagihan Gizi | Per kunjungan / per episode / paket / tidak terpisah | Tetapkan layanan yang billable beserta milestone dan tarifnya | FIX-06 |
| K-04 | Bahan Radiologi dan BMHP | Termasuk tarif layanan / ditagih terpisah | Cegah duplikasi; charge per bahan hanya jika kontrak tarif menyatakan terpisah | Perluasan bahan Radiologi; scope BMHP FIX-03 |
| K-05 | Cakupan invoice IGD dan transfer ke ranap | Invoice IGD tersendiri + konsolidasi resmi / skema existing yang dikunci owner | Gunakan aturan episode/transfer yang disetujui, jangan hanya menghapus filter encounter | FIX-08 |
| K-06 | Perluasan Bank Darah ke invoice | Otomatis per tindakan / manual terkontrol | Pertahankan satu fakta per tindakan sesuai kontrak existing, tambah consumer invoice | FIX-04 |
| K-07 | Makna dan isi daftar kasir | Perjelas label saja / tambah total tagihan dan kekinian | Tambah total tagihan, pisahkan pembayaran dan sisa pasien; tunjukkan kalkulasi tertunda | FIX-10 dan perubahan kontrak respons FIX-09 |

## 8. Catatan pola dan katalog bukti

Pola utamanya: **“tersimpan klinis”, “diterima folio”, “masuk invoice”, dan “terhitung di kasir” sering dianggap satu status**. Dukungan domain pada adapter atau label ringkasan juga tidak membuktikan producer dan pemetaan tarifnya sudah ada. Pencegahan yang disarankan adalah satu matriks producer → milestone → folio → invoice → kalkulasi → kasir, serta pengujian yang membuktikan setiap batas tersebut.

Semua path E01–E28 di bawah relatif terhadap repository yang disebut. SHA berlaku sebagaimana metadata. Simbol dan baris menunjukkan titik awal bukti; pembaca perlu membaca method terkait untuk konteks lengkap.

| Kode | Repository + path:baris / simbol | Yang dibuktikan |
| --- | --- | --- |
| E01 | FE `src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx:376`; `use-inpatient-nursing-procedure.jsx:408` | Keranjang dokter/perawat memanggil `createInpatientProcedureOrder` per item |
| E02 | BE `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs:76`, `:254`, `:289` | Order rawat inap tersimpan sebagai Ordered |
| E03 | BE `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureExecutionService.cs:91`, `:107`, `:157`, `:162`, `:246` | Execute resmi dan penerbitan fakta; short circuit AlreadyExecuted |
| E04 | BE `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs:57`, `:138`, `:211`, `:219`, `:245`, `:267`; `Operational/Services/BillingFolioService.cs:137`, `:543` | Kandidat sumber, batas encounter, tahap resep, pemetaan invoice dan sinkronisasi |
| E05 | BE `Areas/HealthServices/BillingManagement/Billing/Services/BillingSourceTariffResolver.cs:29`, `:63`, `:75`, `:85`, `:102`, `:140`, `:430` | Sumber yang didukung, gratis, dan validitas tarif |
| E06 | BE `Areas/HealthServices/ClinicalManagement/Services/NursingInterventionService.cs:289`, `:439`, `:879`; `docs/module-blueprints/rawat-inap/keperawatan/contracts/integration-contract.md:84` | Pending intervensi dan kemampuan yang ditunda |
| E07 | BE `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs:220`, `:251`, `:281` | Dua pengirim resep setelah finalisasi; konsultasi IGD dikecualikan |
| E08 | BE `Areas/HealthServices/PharmacyManagement/Services/PrescriptionBillingChargeProducer.cs:106`, `:130`, `:155`, `:192` | Charge langsung versi 1 dari TotalPrice; kategori farmasi |
| E09 | BE `Areas/HealthServices/BillingManagement/Billing/Services/BillingSourceTariffResolver.cs:222`, `:239`, `:275`; `PharmacyManagement/Services/PrescriptionAggregateService.cs:83` | Satu baris per resep, penolakan racikan, dan agregat harga bahan |
| E10 | BE `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs:1858`, `:2274`, `:2311` | Penolakan versi sama isi berbeda, hash memasukkan deskripsi |
| E11 | BE `docs/module-blueprints/rawat-inap/integrasi-billing/contracts/integration-contract.md:224`; `Areas/HealthServices/PharmacyManagement/Services/PrescriptionDispensingService.cs:349`, `:370`, `:379`, `:387` | Milestone rawat inap; dispense memerlukan fakta resep sebelumnya |
| E12 | FE `src/components/view/health-services/inpatient-management/nursing-workspace/sections/medication/panels/order-alat-kesehatan-panel.jsx:191`, `:214`, `:218`; `sections/medication/nursing-medication-section.jsx:53`; `sections/nursing-care/nursing-care-section.jsx:44` | UI keranjang Alkes reachable dan payload/fallback |
| E13 | BE `Areas/HealthServices/PharmacyManagement/Services/DrugUsageService.cs:153`, `:164`, `:181`, `:250`, `:319`, `:458`, `:467` | Draft, validasi petugas/lokasi, record stok tanpa pengiriman invoice |
| E14 | BE `Areas/HealthServices/PharmacyManagement/DTOs/DrugUsageDtos.cs:19`; `DTOs/PrescriptionDtos.cs:162`; `Controllers/PrescriptionController.cs:747`; FE `src/lib/services/health-services/pharmacy-management/drug-usage.service.js:20` | Kontrak request; create bukan record |
| E15 | BE `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs:673`, `:1682` / `EmitChargeEligibilityAsync` | Penerimaan spesimen; fakta per pemeriksaan |
| E16 | BE `Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs` / `DecideQualityAsync`, `EmitChargeEligibilityAsync` (`:1246`), `:827`; `BillingManagement/Billing/Services/BillingSourceTariffResolver.cs:115` | Kualitas citra; konsumsi bahan tersendiri; pengulangan nonbillable |
| E17 | BE `Areas/HealthServices/BloodBankManagement/Services/BbkBloodBankProcedureService.cs:562`, `:669`; `docs/module-blueprints/bank-darah/contracts/integration-contract.md:82` | Fakta Bank Darah selesai dan batas kontrak folio |
| E18 | BE `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs:63`, `:96`; `Billing/Services/BillingChargeSourceAdapter.cs:29` | BloodBank diizinkan folio, belum ada domain invoice; ADHOC/CONSUMABLE bukan producer |
| E19 | BE `Areas/HealthServices/NutritionManagement/Services/NutritionOrderService.cs:144`, `:284`, `:369`; `docs/module-blueprints/gizi/00-interview-decisions.md:40`; `BillingManagement/Operational/Services/PatientBillingSummaryService.Breakdown.cs:164` | Alur Gizi tanpa producer; label kelompok tidak menghasilkan charge |
| E20 | BE `Areas/HealthServices/HemodialysisManagement/Services/HmdBillingHandoffService.cs` / `BuildFactRequest`, `:175`; `HmdSessionFinalizationService.cs` / `FinalizeAsync` | Handoff HD melalui Procedure |
| E21 | BE `Areas/HealthServices/ClinicalManagement/Services/CliEquipmentUsageService.cs:66`, `:134`, `:157` / `FinishAsync`, `CloseRunningForDepartureAsync`, `EmitAsync`; `BillingManagement/Billing/Services/BillingSourceTariffResolver.cs:48` | Producer/resolver pemakaian alat setelah selesai atau auto close |
| E22 | BE `Areas/HealthServices/OperatingRoomManagement/Services/OperatingRoomCompletionEffects.cs:75`; `OperatingRoomIntegrationService.cs:271`; `BillingManagement/Billing/Services/BillingSourceTariffResolver.cs:297` | Penyelesaian operasi, biaya anestesi/sewa/bahan |
| E23 | BE `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs:200`, `:208`, `:346` | Kalkulasi biaya kamar dan administrasi |
| E24 | BE `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs:507`, `:642`, `:819`, `:826`, `:916`, `:1044`, `:1990` | Daftar membaca invoice; kalkulasi terakhir/fallback; preview live; upsert tidak recalculate |
| E25 | BE `Areas/HealthServices/PharmacyManagement/Services/DrugReturnBillingHandoffService.cs:60`, `:90`, `:94`, `:115`; `BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.DrugReturn.cs` / `SyncDrugReturnAsync` | Retur resep/bahan operasi; jalur nonresep tidak dicakup |
| E26 | BE `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs:883`, `:933`; `Services/PatientProcedureOrderService.cs:437`, `:480` | Cabang Completed langsung tanpa emisi fakta |
| E27 | FE `src/components/view/health-services/billing-management/billing-invoices/cashier-overview/cashier-overview-view.jsx:135`; `src/lib/hooks/health-services/billing-management/billing-invoices/use-cashier-billing-overview.js:31`, `:119`; BE `docs/module-blueprints/billing-kasir/03-frontend-architecture.md:13` | Kolom pembayaran, query dan desain daftar |
| E28 | BE `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs:545`, `:594`, `:619`, `:629`; `BillingCalculationService.cs:389` | Penjaga finalisasi, tarif, kekinian, dan RequiresReview |

Penelusuran negatif menggunakan `rg` pada seluruh source domain terkait: pemanggil `RecordBillingDispatchOutcomeAsync` hanya deklarasi; `PhmDrugUsage`, `CliNursingIntervention`, `BloodBank`, `GziNutrition` pada Billing tidak menemukan producer invoice alternatif untuk celah di atas. Registrasi `BillingClinicalChargeBridgeService`/resolver dan `BilInvoiceSyncWorker` ditemukan di service extensions dan `Program.cs:1058`; keberadaan registrasi tidak membuktikan worker/kebijakan aktif pada runtime.

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`.

| Method | Path | Kegunaan | Hak akses | Request | Response dalam ApiResponse |
| --- | --- | --- | --- | --- | --- |
| GET | `/cashier-overview` | Daftar invoice kasir, pembayaran, dan sisa pasien | BillingInvoice : Read | CashierBillingInvoiceQuery | PagedResult<CashierBillingInvoiceListItemResponse> |

`200`: daftar berhasil dibaca; `422`: filter tidak sah, misalnya tanggal mulai setelah tanggal akhir; pengguna harus login dan memiliki hak baca. Endpoint tidak membuat charge klinis.

### Health Services / Pharmacy Management / Drug Usage

Base URL: `api/v1/health-services/pharmacy-management/drug-usages`.

| Method | Path | Kegunaan | Hak akses | Request | Response dalam ApiResponse |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | Membuat draft pemakaian; belum mengurangi stok | DrugUsage : Create | CreateDrugUsageRequest | DrugUsageDetailResponse |
| POST | `/{id}/record` | Mencatat pemakaian dan mengurangi stok | DrugUsage : Record | DrugUsageCommandRequest | DrugUsageDetailResponse |

`200`: langkah bersangkutan berhasil; `404`: transaksi tidak ditemukan; `409`: versi/status konflik; `422`: master atau stok tidak memenuhi syarat. Contoh petugas/lokasi yang tidak ditemukan tidak boleh diperlakukan sebagai depot default.

### Health Services / Clinical Management / Nursing Intervention

Base URL: `api/v1/health-services/clinical-management/nursing-interventions`.

| Method | Path | Kegunaan | Hak akses | Request | Response dalam ApiResponse |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | Mencatat tindakan keperawatan | NursingIntervention : Create | CreateNursingInterventionRequest | NursingInterventionResponse |
| POST | `/batch` | Mencatat sekumpulan tindakan harian | NursingIntervention : Create | CreateBatchNursingInterventionRequest | BatchNursingInterventionResponse |
| GET | `/{id}/billing-dispatch` | Membaca status pengiriman tagihan tindakan | NursingIntervention : Read | ID tindakan | BillingDispatchResponse |

`201`: catatan baru; `200`: pembacaan/kiriman ulang sah; `400`: isian tidak sah; `403`: tidak berwenang; `404`: data tidak ditemukan; `422`: prasyarat klinis tidak terpenuhi. Keberhasilan endpoint pencatatan tidak membuktikan item invoice telah dibuat.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-12 | Audit source seluruh kategori checkout; delapan kelompok temuan, satu tambahan, matriks cakupan dan hasil uji 38/39; tanpa perubahan aplikasi/database | diagnose-module-issue |
