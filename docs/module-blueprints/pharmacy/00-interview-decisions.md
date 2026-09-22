# Farmasi — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `PHA-BP-001` |
| Revision | `3` |
| Status | `draft` |
| Interview mode | `Scope pass` |
| Scope-pass result | `concluded 19 Agustus 2026`; keputusan lintas-owner masih perlu verifikasi |
| Product/domain owner | User — menyatakan sebagai pemilik keputusan pada 18 Agustus 2026 |
| Backend SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend SHA | `400104f2a0f3239c14c40f5905b419977a538450` |
| Input evidence | Jawaban wawancara 18–20 Agustus 2026; dokumen `FARMASI BUSINESS DECISION` 3 September 2026 |

## Scope dan outcome

- **Decision:** Blueprint mencakup seluruh kapabilitas Modul Farmasi; tidak ada kapabilitas
  yang dinyatakan di luar scope blueprint.
- **Decision:** Implementasi wajib dibagi menjadi beberapa delivery slice meskipun blueprint
  akhirnya mencakup seluruh modul.
- **Open Question:** Daftar kapabilitas yang dimaksud dengan “semuanya”, batas setiap slice,
  urutan delivery, dan outcome teruji tiap slice belum disepakati.

## Aktor, ownership, dan invariant

- **Decision:** Aliran custody stok adalah `Gudang Utama → Farmasi → Unit → Pasien`.
- **Decision:** Stok negatif dilarang pada setiap lokasi.
- **Decision:** Stok minimum adalah batas peringatan pengadaan, bukan cadangan yang dikunci;
  obat tetap dapat diresepkan selama available stock lebih dari nol.
- **Decision:** Safety stock adalah cadangan terpisah yang tidak dapat dipakai untuk resep
  rutin. `Prescribable stock = usable on-hand - reserved - safety stock`.
- **Decision:** Reorder point dan safety stock dikonfigurasi per obat dan per lokasi oleh
  Kepala Farmasi atau petugas persediaan yang diberi kewenangan. Perubahan wajib mencatat
  pelaku, waktu, nilai lama, nilai baru, dan alasan.
- **Decision:** Obat tidak ditampilkan ketika prescribable stock nol. Penyimpanan resep
  ditolak jika jumlah yang diminta melebihi prescribable stock.
- **Decision:** Saat dokter menyimpan resep, jumlah yang diminta divalidasi terhadap available
  stock farmasi/depo pelayanan. Jika tidak cukup, penyimpanan gagal dan dokter harus memilih
  obat atau jumlah pengganti; sistem tidak boleh mengganti keputusan klinis secara otomatis.
- **Decision:** Available stock resep hanya berasal dari farmasi/depo yang ditetapkan melayani
  pasien. Stok gudang utama atau lokasi lain belum tersedia untuk resep tersebut sampai
  transfer diterima secara resmi oleh farmasi/depo pelayanan.
- **Decision:** Dokter/dokter gigi yang berwenang membuat resep dan memutus perubahan klinis
  terhadap obat, dosis, rute, atau frekuensi.
- **Decision:** Apoteker memverifikasi resep dan melakukan dispensing. Substitusi hanya boleh
  dilakukan melalui perubahan resep oleh dokter; apoteker dan sistem tidak boleh mengganti
  merek, generik, zat aktif, kekuatan, bentuk sediaan, dosis, atau rute.
- **Decision:** Jika obat yang sudah direservasi menjadi tidak tersedia akibat discrepancy,
  recall, kerusakan, atau sebab lain, Farmasi mengembalikan resep kepada dokter. Dokter
  membatalkan/mengubah resep dan Billing menghitung ulang harga atau reversal secara terlacak.
- **Decision:** Alergi berat yang terdokumentasi menjadi hard stop saat resep disimpan.
  Override hanya oleh dokter berwenang dengan alasan klinis wajib dan harus diverifikasi
  apoteker sebelum obat diproses.
- **Decision:** Interaksi obat berisiko berat menjadi hard stop. Override hanya oleh dokter
  berwenang dengan alasan klinis dan verifikasi apoteker; pasangan obat, tingkat risiko,
  serta sumber/versi data interaksi wajib tercatat.
- **Decision:** Pemeriksaan klinis memakai severity `Critical`, `Warning`, dan `Information`.
  Critical menjadi hard stop; Warning memerlukan acknowledgement dan alasan dokter;
  Information tidak memblokir. Severity berasal dari aturan klinis approved dan berversi,
  bukan keputusan developer.
- **Decision:** Pembatalan sebelum penyerahan obat dapat dilakukan apoteker dengan alasan
  tercatat. Pembatalan setelah penyerahan memerlukan persetujuan penanggung jawab farmasi
  dan transaksi reversal.
- **Decision:** Retur diverifikasi apoteker; kelayakan obat untuk kembali ke stok mengikuti
  SOP yang masih harus tersedia.
- **Invariant:** Setiap tindakan material mencatat pelaku, waktu, alasan, status sebelum dan
  sesudah, serta mempertahankan histori.
- **Decision:** Pemilihan batch menggunakan FEFO. Batch kedaluwarsa, dikarantina, atau terkena
  recall tidak boleh diresepkan/diserahkan; batch juga harus tetap berlaku sampai durasi
  penggunaan pasien selesai.
- **Decision:** Apoteker dapat memilih batch lain dengan alasan tercatat. Nomor batch dan
  tanggal kedaluwarsa wajib dapat ditelusuri sampai pasien penerima.
- **Decision:** Kepala Farmasi atau apoteker berwenang dapat mengaktifkan recall batch.
  Aktivasi segera memblokir resep, dispensing, dan transfer; stok tersisa dikarantina.
- **Decision:** Sistem menyediakan penelusuran lokasi stok dan pasien penerima batch recall,
  serta mencatat notifikasi, upaya kontak, pengembalian, penggantian, hasil tindak lanjut,
  dan histori recall yang tidak dapat dihapus.
- **Decision:** Narkotika, psikotropika, dan obat high-alert hanya dapat diresepkan oleh dokter
  berwenang, wajib diverifikasi apoteker, dan penyiapan/penyerahannya memerlukan pemeriksaan
  dua petugas berwenang. Substitusi otomatis dilarang.
- **Decision:** Transaksi obat khusus mencatat jumlah, saldo, pasien, pemberi resep, apoteker,
  penerima, waktu, dan saksi. Selisih stok menghentikan transaksi dan dieskalasikan kepada
  Kepala Farmasi; retur dan pemusnahan memerlukan persetujuan serta saksi.
- **Decision:** Saldo stok tidak boleh diedit langsung. Koreksi dilakukan melalui sesi stock
  opname per lokasi/batch yang mencatat saldo awal, hasil hitung, cut-off, selisih, alasan,
  petugas penghitung, dan approver yang berbeda.
- **Decision:** Adjustment stok baru dibuat setelah persetujuan. Selisih obat khusus tidak
  disesuaikan otomatis dan wajib melalui investigasi.
- **Decision:** Barang yang diserahkan lokasi asal masuk status `Dalam Perjalanan` dan belum
  menjadi stok lokasi tujuan. Lokasi tujuan menambah stok hanya sebesar jumlah aktual yang
  diterima.
- **Decision:** Selisih transfer tetap berstatus discrepancy sampai investigasi dan resolusi
  disetujui; selisih tidak boleh dihapus atau dibebankan otomatis ke salah satu lokasi.
- **Decision:** Obat yang telah dibawa pulang pasien tidak boleh langsung dikembalikan ke stok
  jual karena integritas penyimpanannya tidak dapat dipastikan. Obat tersebut masuk pemeriksaan,
  karantina, atau pemusnahan.
- **Decision:** Obat dari unit rawat inap yang belum diberikan dapat direstock hanya setelah
  apoteker memverifikasi kemasan, batch, kedaluwarsa, suhu, dan rantai penyimpanan. Racikan
  tidak boleh kembali ke stok umum.
- **Decision:** Refund tidak otomatis mengikuti retur; hanya alasan yang disetujui, termasuk
  kesalahan Farmasi atau recall, yang dapat memicu reversal/refund. Restock, reversal, dan
  pemusnahan memerlukan persetujuan apoteker serta audit.

## State, finalisasi, dan koreksi

- **Decision:** Resep final setelah dikirim atau ditandatangani pemberi resep.
- **Fact:** Farmasi baru menerima antrean/data resep setelah pembayaran atau jaminan
  dikonfirmasi; sebelum itu Farmasi tidak mencetak atau mulai memproses obat.
- **Decision:** Saat dokter menyimpan resep, sistem memeriksa dan mereservasi prescribable
  stock secara atomik pada sistem persediaan. Reservasi ini tidak membuat antrean atau
  cetakan Farmasi. Resep baru masuk antrean Farmasi setelah pembayaran/jaminan dikonfirmasi.
- **Decision:** Resep final tidak ditimpa. Koreksi dilakukan melalui pembatalan item dan order
  pengganti yang dapat ditelusuri.
- **Decision:** Verifikasi final setelah apoteker menyetujui resep.
- **Decision:** Dispensing rawat jalan final ketika obat diserahkan kepada pasien.
- **Decision:** Dispensing rawat inap/IGD final ketika obat diserahkan kepada unit; pencatatan
  pemberian kepada pasien merupakan proses terpisah.
- **Decision:** Stok berkurang ketika serah-terima fisik berhasil.
- **Decision:** Tagihan rawat inap/IGD timbul setelah obat benar-benar diberikan kepada pasien,
  bukan ketika obat dikirim ke unit.
- **Decision:** Ketika kunjungan rawat jalan pada hari yang sama berlanjut menjadi rawat
  inap, biaya yang sudah sah digabungkan ke satu akun tagihan rawat inap dan tidak boleh
  ditagihkan ganda.
- **Decision:** Resep pulang merupakan resep terpisah dari pemberian obat selama
  rawat inap. Obat pulang direservasi saat resep disimpan dan ditagihkan/final ketika obat
  diserahkan kepada pasien, bukan melalui catatan pemberian perawat.
- **Decision:** Discharge administratif baru selesai setelah obat pulang diserahkan atau
  pasien menolak/tidak mengambil obat dengan alasan dan edukasi yang terdokumentasi.
- **Decision:** Untuk rawat jalan umum/tunai, obat boleh disiapkan setelah verifikasi tetapi
  hanya diserahkan setelah Billing/Kasir mengonfirmasi pembayaran berhasil.
- **Decision:** Untuk rawat jalan dengan penjamin, obat diserahkan setelah Billing
  mengonfirmasi jaminan; selisih yang menjadi tanggungan pasien mengikuti penyelesaian Billing.
- **Invariant:** Farmasi tidak menetapkan status lunas atau jaminan secara manual; status
  tersebut bersumber dari Billing/Kasir.
- **Decision:** Farmasi tidak mulai meracik obat rawat jalan sebelum Billing mengonfirmasi
  pembayaran atau jaminan. Obat non-racikan boleh dialokasikan/disiapkan, tetapi belum boleh
  diserahkan sebelum konfirmasi tersebut.
- **Proposed Decision:** Obat yang sudah dibayar dan selesai disiapkan ditahan maksimal 24
  jam sejak notifikasi siap diambil; detail penanganan setelah batas waktu masih menunggu
  persetujuan. Tidak ada reservasi 24 jam untuk resep yang belum dibayar.
- **Decision:** Koreksi setelah final menggunakan retur/reversal; transaksi final tidak dihapus.

## UI decision authority

Belum ada keputusan menu, route, layout, atau bentuk visual. Keputusan tersebut menunggu
invariant, permission, privacy, dan brief yang disetujui.

## Decision log

| Decision ID | Type | Item | Owner | Status | Approval evidence |
|---|---|---|---|---|---|
| `PHA-DEC-001` | Decision | Blueprint mencakup seluruh Modul Farmasi tanpa pengecualian yang dinyatakan | Product/domain owner | `approved` | Jawaban user 18 Agustus 2026 |
| `PHA-DEC-002` | Decision | Implementasi dibagi menjadi beberapa slice | Product/domain owner | `approved` | “semua setuju”, 19 Agustus 2026 |
| `PHA-DEC-003` | Decision | Custody stok: Gudang Utama → Farmasi → Unit → Pasien | Product/domain owner | `approved` | Jawaban user 18 Agustus 2026 |
| `PHA-DEC-004` | Decision | Stok negatif dilarang | Product/domain owner | `approved` | Jawaban user 18 Agustus 2026 |
| `PHA-DEC-005` | Decision | Otorisasi resep, verifikasi, pembatalan, dan retur mengikuti pembagian kewenangan yang dicatat di atas | Product/domain owner; clinical/pharmacy governance perlu memverifikasi | `approved` oleh product owner | “semua setuju”, 19 Agustus 2026 |
| `PHA-DEC-006` | Decision | Finalisasi dan koreksi mengikuti aturan append/reversal yang dicatat di atas | Product/domain owner; clinical/pharmacy governance perlu memverifikasi | `approved` oleh product owner | “semua setuju”, 19 Agustus 2026 |
| `PHA-DEC-007` | Decision | Tagihan rawat inap/IGD timbul setelah pemberian obat kepada pasien | Product/domain owner; Billing/Finance owner perlu memverifikasi | `approved` oleh product owner | Jawaban user 19 Agustus 2026 |
| `PHA-DEC-008` | Decision | Rawat jalan umum/tunai membayar sebelum penyerahan obat; pasien penjamin menunggu konfirmasi jaminan dari Billing | Product/domain owner; Billing/Finance owner perlu memverifikasi | `approved` oleh product owner | Jawaban user “nomor 1, yang sewajarnya di Indonesia”, 19 Agustus 2026 |
| `PHA-OQ-001` | Open Question | Kapabilitas konkret dalam “semuanya”, batas slice, urutan delivery, dan acceptance outcome | Product/domain owner | `open` | Belum diputuskan |
| `PHA-OQ-002` | Open Question | SOP formularium, substitusi, retur, obat high-alert/narkotika, dan segregation of duties | Clinical/pharmacy governance owner | `open` | Belum tersedia |
| `PHA-OQ-003` | Open Question | Lifecycle batch/lot, kedaluwarsa, FEFO, karantina, recall, stock opname, dan koreksi stok | Pharmacy + warehouse owner | `open` | Belum dibahas |
| `PHA-OQ-004` | Open Question | Partial dispensing, kekurangan stok, duplicate submit, downtime, retry, dan partial failure lintas modul | Product/domain + integration owners | `open` | Belum dibahas |
| `PHA-OQ-005` | Open Question | Reversal tagihan ketika pemberian obat dikoreksi atau dibatalkan | Billing/Finance + product owner | `open` | Belum dibahas |
| `PHA-DEC-009` | Decision | Obat tidak dapat dipilih pada resep ketika stok mencapai batas minimum | Product/domain owner; pharmacy governance perlu memverifikasi | `superseded` oleh `PHA-DEC-010` dan `PHA-DEC-011` | Klarifikasi user 19 Agustus 2026 |
| `PHA-DEC-010` | Decision | Stok minimum adalah reorder alert, bukan safety stock; obat masih boleh diresepkan selama available stock lebih dari nol | Product/domain owner; pharmacy/warehouse owner perlu memverifikasi | `approved` oleh product owner | User memilih poin 2, 19 Agustus 2026 |
| `PHA-DEC-011` | Decision | Obat tidak ditampilkan pada pilihan resep dokter setelah available stock habis | Product/domain owner; clinical/pharmacy governance perlu memverifikasi | `approved` oleh product owner | Klarifikasi user 19 Agustus 2026 |
| `PHA-OQ-006` | Open Question | Available stock resep dihitung dari lokasi mana dan kapan stok direservasi agar dua resep tidak menggunakan stok yang sama | Product/domain + pharmacy/warehouse owner | `superseded` oleh `PHA-DEC-012` dan `PHA-OQ-007` | Lokasi diputuskan; waktu reservasi masih terbuka |
| `PHA-DEC-012` | Decision | Ketersediaan resep hanya memakai stok farmasi/depo pelayanan; stok lokasi lain baru dihitung setelah transfer diterima | Product/domain owner; pharmacy/warehouse owner perlu memverifikasi | `approved` oleh product owner | User memilih poin 1, 19 Agustus 2026 |
| `PHA-OQ-007` | Open Question | Kapan stok direservasi agar resep bersamaan tidak menggunakan stok yang sama | Product/domain + pharmacy owner | `superseded` oleh `PHA-DEC-013` | Diputuskan 19 Agustus 2026 |
| `PHA-DEC-013` | Decision | Stok direservasi secara atomik saat dokter menandatangani/mengirim resep; pembatalan atau expiry melepaskan reservasi tanpa menghapus histori | Product/domain owner; pharmacy owner perlu memverifikasi | `superseded` oleh `PHA-FACT-001` dan usulan `PHA-DEC-016` | Klarifikasi user bahwa Farmasi belum menerima data sebelum pembayaran, 19 Agustus 2026 |
| `PHA-OQ-008` | Open Question | Berapa lama reservasi rawat jalan bertahan dan status resep setelah reservasi dilepas | Product/domain + pharmacy owner | `superseded` oleh `PHA-DEC-014`, `PHA-DEC-015`, dan `PHA-OQ-009` | Klarifikasi user 19 Agustus 2026 |
| `PHA-DEC-014` | Decision | Racikan rawat jalan tidak mulai dibuat sebelum konfirmasi pembayaran/jaminan; obat non-racikan dapat dialokasikan tetapi belum diserahkan | Product/domain owner; pharmacy + Billing owner perlu memverifikasi | `approved` oleh product owner | Pernyataan user 19 Agustus 2026 |
| `PHA-DEC-015` | Decision | Gunakan dua batas 24 jam: reservasi belum dibayar sejak resep dikirim dan penyimpanan obat dibayar sejak notifikasi siap diambil | Product/domain owner; pharmacy + Billing owner perlu memverifikasi | `superseded` oleh usulan `PHA-DEC-016` dan `PHA-DEC-017` | Klarifikasi alur user 19 Agustus 2026 |
| `PHA-OQ-009` | Open Question | Setelah batas pengambilan 24 jam, bagaimana retur stok, racikan yang tidak dapat digunakan ulang, reversal/refund, dan notifikasi pasien ditangani | Product/domain + pharmacy + Billing owner | `open` | Menunggu keputusan |
| `PHA-FACT-001` | Fact | Farmasi baru menerima antrean/data resep setelah pembayaran atau jaminan dikonfirmasi | Product/domain owner | `approved` | Klarifikasi user 19 Agustus 2026 |
| `PHA-DEC-016` | Decision | Resep belum dibayar tidak mereservasi stok; saat pembayaran/jaminan, stok diperiksa dan direservasi atomik sebelum konfirmasi berhasil | Product/domain + pharmacy + Billing owner | `superseded` oleh `PHA-DEC-019` dan `PHA-DEC-021` | Product owner memilih rekomendasi final 19 Agustus 2026 |
| `PHA-DEC-017` | Decision | Setelah pembayaran/jaminan dan reservasi berhasil, resep masuk antrean Farmasi; obat siap ditahan 24 jam sejak notifikasi siap diambil | Product/domain + pharmacy + Billing owner | `superseded` oleh `PHA-DEC-021`; ketentuan 24 jam tetap terbuka pada `PHA-OQ-009` | Reservasi dipastikan terjadi saat resep disimpan |
| `PHA-DEC-018` | Decision | Penyimpanan resep gagal jika jumlah obat melebihi available stock; dokter harus memilih obat/jumlah pengganti | Product/domain + clinical/pharmacy governance owner | `approved` oleh product owner | Pernyataan user 19 Agustus 2026 |
| `PHA-OQ-010` | Open Question | Setelah validasi resep berhasil tetapi sebelum pembayaran, apakah jumlah obat dikunci secara terpusat atau hanya diperiksa ulang saat pembayaran | Product/domain + pharmacy + Billing owner | `superseded` oleh `PHA-DEC-019` dan `PHA-DEC-021` | Diputuskan 19 Agustus 2026 |
| `PHA-CON-001` | Conflict | `PHA-DEC-010` menetapkan stok minimum hanya sebagai reorder alert, sedangkan analis meminta obat tidak tampil ketika available stock di bawah minimum sehingga minimum berfungsi sebagai protected/safety stock | Product/domain + pharmacy/warehouse owner | `superseded` oleh `PHA-DEC-020` | Product/domain owner memilih model dua ambang, 19 Agustus 2026 |
| `PHA-DEC-019` | Decision | `Prescribable stock = usable on-hand - reserved - safety stock`; penyimpanan resep menaikkan reserved secara atomik, sedangkan on-hand baru berkurang ketika obat diserahkan | Product/domain + pharmacy/warehouse owner | `superseded` sebagian oleh `PHA-DEC-041`; rumus stok tetap berlaku, waktu reservasi berubah | Klarifikasi final user 20 Agustus 2026 |
| `PHA-DEC-020` | Decision | Pisahkan `reorder point` untuk peringatan pengadaan dan `safety stock` untuk jumlah yang tidak tersedia bagi resep rutin | Product/domain + pharmacy/warehouse owner | `approved` oleh product owner | User memilih rekomendasi assistant, 19 Agustus 2026 |
| `PHA-DEC-021` | Decision | Reservasi stok terjadi saat resep disimpan, tetapi resep baru masuk antrean/cetak Farmasi setelah pembayaran atau jaminan dikonfirmasi | Product/domain + pharmacy + Billing owner | `superseded` oleh `PHA-DEC-041` | Klarifikasi final user 20 Agustus 2026 |
| `PHA-DEC-022` | Decision | Reorder point dan safety stock ditetapkan per obat/per lokasi oleh Kepala Farmasi atau petugas persediaan berwenang dengan audit lengkap | Product/domain + pharmacy/warehouse owner | `approved` oleh product owner | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-023` | Decision | Usulan: Kasir berwenang menandai pembayaran tidak dilanjutkan dan melepaskan reservasi stok, tetapi tidak dapat menghapus/mengubah resep klinis | Product/domain + Billing/Finance + security owner | `draft` | Permintaan user 19 Agustus 2026 |
| `PHA-OQ-011` | Open Question | Apakah pelepasan manual oleh kasir tetap disertai fallback otomatis setelah 24 jam dan bagaimana pembayaran parsial/jaminan pending ditangani | Product/domain + Billing/Finance owner | `open` | Menunggu keputusan |
| `PHA-DEC-024` | Decision | Biaya rawat jalan yang sah pada hari yang sama sebelum admission digabungkan ke akun tagihan rawat inap tanpa duplikasi | Product/domain + Billing/Finance owner | `approved` oleh product owner | Jawaban user “ya”, 19 Agustus 2026 |
| `PHA-DEC-025` | Decision | Resep pulang terpisah dari order/pemberian rawat inap; biaya obat pulang final saat diserahkan kepada pasien | Product/domain + clinical/pharmacy + Billing owner | `approved` oleh product owner | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-OQ-012` | Open Question | Apakah discharge administratif harus menunggu obat pulang diserahkan atau boleh selesai dengan penolakan/tidak mengambil yang terdokumentasi | Product/domain + clinical/pharmacy owner | `superseded` oleh `PHA-DEC-026` | Diputuskan 19 Agustus 2026 |
| `PHA-DEC-026` | Decision | Discharge administratif menunggu penyerahan obat pulang atau penolakan/tidak mengambil yang disertai alasan dan edukasi terdokumentasi | Product/domain + clinical/pharmacy owner | `approved` oleh product owner | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-027` | Decision | Gunakan FEFO; kecualikan batch expired/quarantine/recall dan batch yang kedaluwarsa sebelum terapi selesai; override apoteker wajib beralasan dan batch ditelusuri ke pasien | Product/domain + pharmacy/clinical owner | `approved` oleh product owner | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-028` | Decision | Recall batch oleh Kepala Farmasi/apoteker berwenang memblokir pemakaian dan transfer, mengarantina sisa stok, menelusuri pasien/lokasi, serta mempertahankan audit tindak lanjut | Product/domain + pharmacy/clinical owner | `approved` oleh product owner | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-029` | Decision | Terapkan kontrol peresepan, verifikasi apoteker, dual-check, larangan substitusi otomatis, pencatatan saldo/saksi, eskalasi selisih, serta approval retur/pemusnahan untuk narkotika, psikotropika, dan high-alert | Product/domain + clinical/pharmacy governance owner | `approved` oleh product owner; `VERIFY_CURRENT_REGULATION` | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-030` | Decision | Stock opname per lokasi/batch memakai cut-off, maker-checker, approval sebelum adjustment, audit lengkap, dan investigasi wajib untuk selisih obat khusus | Product/domain + pharmacy/warehouse owner | `approved` oleh product owner | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-031` | Decision | Transfer memakai status Dalam Perjalanan, stok tujuan bertambah sesuai penerimaan aktual, dan discrepancy parsial memerlukan investigasi/resolusi beralasan tanpa write-off otomatis | Product/domain + pharmacy/warehouse owner | `approved` oleh product owner | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-032` | Decision | Retur pasien tidak langsung direstock; retur unit dapat direstock setelah verifikasi integritas; racikan dilarang direstock; refund hanya untuk alasan approved dan seluruh tindakan memerlukan approval/audit | Product/domain + pharmacy + Billing/Finance owner | `approved` oleh product owner | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-033` | Decision | Apoteker/sistem tidak boleh melakukan substitusi obat; setiap perubahan obat/merek/generik/kekuatan/bentuk/dosis/rute hanya oleh dokter melalui perubahan resep dan rekalkulasi Billing | Product/domain + clinical/pharmacy + Billing owner | `approved` oleh product owner | Koreksi user 19 Agustus 2026; menggantikan klausul substitusi pada keputusan awal |
| `PHA-DEC-034` | Decision | Jika obat tidak dapat dipenuhi karena recall/kerusakan rumah sakit, dokter memilih pengganti yang sesuai klinis dengan harga sama atau tidak jauh berbeda; keputusan klinis mendahului rekalkulasi Billing | Product/domain + clinical + Billing/Finance owner | `superseded` oleh `PHA-DEC-035` | Product owner memutuskan selisih ditanggung rumah sakit, 19 Agustus 2026 |
| `PHA-OQ-013` | Open Question | Berapa batas nominal/persentase “harga tidak jauh”, siapa menyetujui pengecualian, dan siapa menanggung selisih akibat kesalahan rumah sakit | Product/domain + Billing/Finance owner | `superseded` sebagian oleh `PHA-DEC-035`; approval internal masih terbuka | Selisih pasien ditutup; kontrol biaya internal belum diputuskan |
| `PHA-DEC-035` | Decision | Untuk penggantian akibat recall/kerusakan sebelum penyerahan, dokter memilih obat yang sesuai klinis; kenaikan harga tidak dibebankan kepada pasien dan menjadi beban rumah sakit, sedangkan recovery vendor diproses terpisah | Product/domain + Billing/Finance owner | `approved` oleh product owner; Finance perlu memverifikasi posting | Jawaban user “ditanggung RS”, 19 Agustus 2026 |
| `PHA-DEC-036` | Decision | Setelah pasien membayar, penggantian akibat kesalahan rumah sakit tidak meminta pasien kembali ke kasir; seluruh selisih dan koreksi finansial diproses internal, sedangkan pasien hanya menerima obat pengganti dan informasi klinis yang diperlukan | Product/domain + Billing/Finance + pharmacy owner | `approved` oleh product owner; Finance perlu memverifikasi posting | Klarifikasi user 19 Agustus 2026 |
| `PHA-DEC-037` | Decision | Alergi berat menjadi hard stop; override hanya oleh dokter berwenang dengan alasan klinis dan wajib diverifikasi apoteker sebelum pemrosesan | Product/domain + clinical/pharmacy governance owner | `approved` oleh product owner; clinical governance perlu memverifikasi severity source | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-038` | Decision | Interaksi obat berat menjadi hard stop dengan override dokter beralasan, verifikasi apoteker, dan audit pasangan obat/severity/sumber-versi data | Product/domain + clinical/pharmacy governance owner | `approved` oleh product owner; clinical governance perlu memverifikasi severity source | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-039` | Decision | Gunakan severity Critical/Warning/Information dengan perilaku hard-stop/reason/informational yang berasal dari aturan klinis approved dan berversi | Product/domain + clinical/pharmacy governance owner | `approved` oleh product owner; threshold klinis belum ditetapkan | Jawaban user “setuju”, 19 Agustus 2026 |
| `PHA-DEC-040` | Decision | Depo pelayanan ditentukan otomatis dari layanan encounter menggunakan kontrak `PHA-DEPOT-ROUTING-v1`; hasil wajib tepat satu dan tidak boleh memilih Gudang Utama, lokasi karantina, atau kandidat secara acak | Product/domain owner | `approved` | Persetujuan user 20 Agustus 2026 |
| `PHA-DEC-041` | Decision | Reservasi dilakukan setelah pembayaran/jaminan valid dan Farmasi mulai memproses resep; penentuan Depo tidak mengambil stok, sedangkan stok fisik baru berkurang saat penyerahan berhasil | Product/domain owner; Billing dan Pharmacy owner perlu memverifikasi integrasi | `approved` oleh product owner | Klarifikasi dan persetujuan user 20 Agustus 2026 |
| `PHA-OQ-014` | Open Question | Hasil resmi ketika pembayaran/jaminan valid tetapi reservasi atomik gagal karena stok sudah habis | Product/domain + Billing/Finance + Pharmacy | `open`; `BLOCKING` untuk reservasi | Requirement gate `PHA-RCG-001`, 20 Agustus 2026 |
| `PHA-OQ-015` | Open Question | Sumber authoritative status paid/approved/reversal/refund serta idempotency callback Billing | Billing/Finance + Security | **`closed`** — dijawab `PHA-DEC-063`, `PHA-DEC-064`, `PHA-DEC-065`; reversal/refund detail tetap terbuka pada `PHA-OQ-005` | Closure amendment 21 September 2026 |
| `PHA-OQ-016` | Open Question | Kebijakan partial dispensing, cakupan obat/layanan, approver, sisa resep, dan koreksi tagihan | Pharmacy + Clinical Governance + Billing | `open`; `BLOCKING` untuk penyerahan | Requirement gate `PHA-RCG-001`, 20 Agustus 2026 |
| `PHA-OQ-017` | Open Question | Daftar obat yang wajib checker kedua dan matriks kewenangan checker | Pharmacy + Clinical Governance + Security | `open`; `BLOCKING` untuk penyerahan | Requirement gate `PHA-RCG-001`, 20 Agustus 2026 |
| `PHA-DEC-042` | Decision | Klasifikasi pelayanan: OP = rawat jalan, IP = rawat inap, APS = pasien mandiri tanpa encounter reguler. APS tidak boleh disamakan dengan IP/OP dan hanya dipakai bila rumah sakit menyediakan pelayanan farmasi mandiri | Product/domain owner | `approved`; pemakaian APS masih bersyarat | `FARMASI BUSINESS DECISION` §1, 3 September 2026 |
| `PHA-DEC-043` | Decision | Pemakaian obat tidak langsung menjadi tagihan. Alur: Pemakaian → `NOT BILLED` → validasi transaksi dan aturan charge → kirim ke Billing → `BILLED`. Billing tetap authority pembentukan tagihan; Farmasi hanya menghasilkan transaksi yang dapat ditagihkan | Product/domain owner; Billing/Finance perlu memverifikasi | `approved` oleh product owner | `FARMASI BUSINESS DECISION` §2, 3 September 2026 |
| `PHA-DEC-044` | Decision | Lifecycle resep: Created → Reviewed → Prepared → Verified → Dispensed → Completed, ditambah Cancelled, Rejected, dan On Hold | Product/domain owner | `approved`; berbenturan dengan `PHA-DEC-008`/`PHA-DEC-041`, lihat `PHA-OQ-018` | `FARMASI BUSINESS DECISION` §3, 3 September 2026 |
| `PHA-DEC-045` | Decision | Rawat inap memisahkan Prescription dari Medication Administration / Giving Record; satu resep dapat diberikan beberapa kali sesuai jadwal. `BELUM DIBERIKAN` tidak dipakai sebagai status utama resep | Product/domain owner | `approved` | `FARMASI BUSINESS DECISION` §3, 3 September 2026 |
| `PHA-DEC-046` | Decision | Validasi persediaan: stok tidak boleh negatif; batch dan expired date wajib dicatat; pengeluaran memakai FEFO; transfer tidak boleh melebihi stok tersedia; retur melalui verifikasi; racikan menyimpan komponen penyusun; adjustment wajib beralasan dan ter-audit; setiap perubahan stok menghasilkan histori mutasi | Product/domain owner | `approved` | `FARMASI BUSINESS DECISION` §4, 3 September 2026 |
| `PHA-DEC-047` | Decision | Baseline peran: Dokter membuat resep; Apoteker review, final check, dan validasi obat; Petugas Farmasi menyiapkan dan menjalankan dispensing; Checker melakukan double check untuk obat tertentu sesuai kebijakan; Kepala Farmasi/Supervisor menyetujui koreksi dan pembatalan transaksi tertentu | Product/domain owner; Clinical/Pharmacy Governance perlu memverifikasi | `approved` oleh product owner | `FARMASI BUSINESS DECISION` §5, 3 September 2026 |
| `PHA-DEC-048` | Decision | Autorisasi pasien keluar bukan authority Modul Farmasi. Farmasi hanya menyelesaikan obat pulang bila ada dan memberikan status/informasi terkait obat; Billing menyelesaikan tagihan | Product/domain owner | `approved` | `FARMASI BUSINESS DECISION` §6, 3 September 2026 |
| `PHA-DEC-049` | Decision | Batch dan expired menjadi bagian inti persediaan dengan struktur Obat → Batch → Expired Date → Lokasi/Depo → Saldo, diperlukan untuk FEFO, recall, near-expired monitoring, dan traceability | Product/domain owner | `approved` | `FARMASI BUSINESS DECISION` §7, 3 September 2026 |
| `PHA-DEC-050` | Decision | Depo adalah lokasi yang memiliki saldo stok sendiri. Baseline depo: Gudang Farmasi, Depo Rawat Jalan, Depo Rawat Inap, Depo IGD. ICU, ICCU, OK, Kebidanan, Kamar Bayi, HD, Poli, dan CCVC tidak otomatis dianggap depo | Product/domain owner | `approved`; status tiap unit belum ditetapkan, lihat `PHA-OQ-019` | `FARMASI BUSINESS DECISION` §8, 3 September 2026 |
| `PHA-DEC-051` | Decision | Transaksi lama tidak boleh diedit langsung. Koreksi melalui adjustment yang menghasilkan transaksi baru dan audit history, menyimpan nilai sebelum, nilai sesudah, alasan koreksi, user, dan waktu koreksi | Product/domain owner | `approved` | `FARMASI BUSINESS DECISION` §9, 3 September 2026 |
| `PHA-DEC-052` | Decision | Urutan implementasi: (1) master item farmasi, (2) lokasi/depo, (3) batch dan expired, (4) saldo stok per lokasi, (5) kartu stok/ledger mutasi, (6) transfer obat, (7) pemakaian obat pasien, (8) retur, (9) etiket dan copy resep, (10) integrasi Billing. Pemakaian obat tidak boleh dibangun sebelum ledger stok tersedia | Product/domain owner | `approved`; menutup `PHA-OQ-001` | `FARMASI BUSINESS DECISION`, 3 September 2026 |
| `PHA-DEC-053` | Decision | Prinsip implementasi: reuse capability existing; tidak membuat duplicate source of truth; tidak membuat master pasien/dokter/lokasi baru bila sudah ada; backend menjadi authority validasi; seluruh transaksi stok immutable melalui mutation history; tidak membuat keputusan klinis yang belum disetujui | Product/domain owner | `approved` | `FARMASI BUSINESS DECISION`, 3 September 2026 |
| `PHA-OQ-018` | Open Question | Bagaimana lifecycle `PHA-DEC-044` berdamai dengan gating pembayaran/jaminan (`PHA-DEC-008`) dan reservasi setelah pembayaran (`PHA-DEC-041`)? Lifecycle baru tidak memuat state pembayaran, sedangkan enum terpasang memuat `WaitingForPayment` dan `ReadyForPharmacy` | Product/domain + Billing/Finance owner | **`partially closed`** — `PHA-DEC-063` menegaskan gating pembayaran TETAP berlaku dan `WaitingForPayment`/`ReadyForPharmacy` TIDAK dihapus; pemetaan penuh `PHA-DEC-044` enam-state ke enum sebelas-state tetap `open`, bukan wewenang closure pass ini | Closure amendment 21 September 2026 |
| `PHA-OQ-019` | Open Question | Unit mana di antara ICU, ICCU, OK, Kebidanan, Kamar Bayi, HD, Poli, dan CCVC yang memiliki saldo stok sendiri sehingga menjadi Depo, dan mana yang hanya unit pelayanan | Pharmacy + warehouse owner | `superseded` oleh `PHA-DEC-054` | Diputuskan 3 September 2026 |
| `PHA-DEC-054` | Decision | Lokasi dengan saldo stok sendiri hanya empat: Gudang Farmasi, Depo Rawat Jalan, Depo Rawat Inap, dan Depo IGD. ICU, ICCU, OK, Kebidanan, Kamar Bayi, HD, Poli, dan CCVC bukan depo stok mandiri pada tahap ini dan memperoleh obat lewat permintaan kepada Farmasi/Depo terkait. Bila kelak sebuah unit membutuhkan stok mandiri, dibuat keputusan dan konfigurasi terpisah | Product/domain owner | `approved` | Keputusan `PHA-OQ-019`, 3 September 2026 |
| `PHA-DEC-055` | Decision | Transfer antar lokasi hanya sah bila lokasi asal berbendera `IsAllowTransferOut` dan lokasi tujuan berbendera `IsAllowTransferIn` pada `MstDrugStorageLocation`. Bendera yang sudah ada dipakai ulang; tidak dibuat penanda depo tersendiri | Product/domain owner | `approved`; hasil audit model lokasi 3 September 2026 | Implementasi transfer, 3 September 2026 |
| `PHA-DEC-056` | Decision | Alur transfer: Draft → Diajukan → Disetujui (stok ditahan menurut FEFO) → Dikeluarkan dari asal → Diterima di tujuan. Keluar dan terima adalah dua langkah terpisah supaya selisih barang yang hilang atau rusak dalam perjalanan tetap terbaca. Pembatalan sebelum barang keluar melepas kembali stok yang tertahan | Product/domain owner | `approved` | Implementasi transfer, 3 September 2026 |
| `PHA-OQ-020` | Open Question | Apakah reservasi stok (`PHA-DEC-013`, `PHA-DEC-041`) tetap berlaku setelah `PHA-DEC-046`? Daftar validasi persediaan 3 September 2026 tidak menyebut reservasi sama sekali | Product/domain + pharmacy owner | `superseded` oleh `PHA-DEC-052a` | Reservasi ditegaskan tetap berlaku, 3 September 2026 |
| `PHA-DEC-057` | Decision | Warna dan jenis etiket dipilih Petugas Farmasi saat dispensing. Rute pemberian hanya ditampilkan sebagai informasi pendukung bila tersedia. Sistem DILARANG menurunkan warna etiket secara otomatis dari rute, karena data rute pada master obat belum lengkap dan inferensi otomatis akan menghasilkan label yang salah | Product/domain owner | `approved` | Keputusan 3 September 2026; bukti: 6.231 dari 7.864 baris `MstDrug` tidak memiliki `Route` |
| `PHA-DEC-058` | Decision | **Direvisi 4 September 2026.** Copy resep ditunda, tetapi bukan karena SIA/SIPA. Audit master membuktikan legalitas sudah punya source-of-truth: `MstHospitalSite` untuk identitas fasilitas, `WfpCredentialLicense` untuk izin praktik apoteker, `MstLicenseType` untuk jenis izin. Konteksnya Instalasi Farmasi RS, bukan apotek eksternal, sehingga kop dokumen memakai identitas rumah sakit. SIPA adalah atribut orang yang menyerahkan, dibaca dari master saat cetak. **Dilarang membuat input SIA maupun SIPA pada form copy resep.** Data SIPA yang belum terisi adalah masalah kelengkapan master, bukan blocker workflow | Product/domain owner | `approved`; **diperbarui 7 September 2026** — capability dispensing sudah selesai dan teruji sehingga blocker itu terangkat, tetapi statusnya kembali menjadi **`BLOCKED_BY_BUSINESS_DECISION`** dengan sebab yang berbeda: format legal dokumennya belum diputuskan. Lihat `PHA-DEC-061` | Audit read-only 4 September 2026; pembaruan 7 September 2026 |
| `PHA-DEC-059` | Decision | Copy resep memerlukan kapabilitas **Prescription Item Dispensing History**: Prescription → Prescription Item → Dispensing/Handover Record → Qty Dispensed → Qty Remaining. Untuk tiap item sistem harus tahu jumlah diresepkan, jumlah sudah diserahkan, jumlah tersisa, dan status `det`/`nedet`. Copy resep wajib mengambil datanya dari histori itu, bukan dari status resep. **`det`/`nedet` diturunkan dari sisa, tidak boleh diisi manual.** Urutan: bangun dispensing lebih dulu, copy resep terakhir | Product/domain owner | `approved`; menjadi prasyarat tunggal copy resep | Keputusan 4 September 2026 |
| `PHA-DEC-060` | Decision | Identitas fasilitas dan apoteker pada copy resep diambil otomatis dari master, tidak pernah diketik petugas. Dilarang membuat duplicate source-of-truth dispensing | Product/domain owner | `approved` | Keputusan 4 September 2026 |
| `PHA-OQ-021` | Open Question | Sumber Nomor SIA untuk instalasi farmasi | Pharmacy + legal/perizinan owner | **`closed`** — bukan blocker. Konteks Instalasi Farmasi RS memakai identitas rumah sakit dari `MstHospitalSite`; SIA apotek eksternal tidak berlaku | `PHA-DEC-058` revisi, 4 September 2026 |
| `PHA-OQ-022` | Open Question | Sumber data apoteker penanggung jawab dan Nomor SIPA | Pharmacy + HR owner | **`closed`** — struktur sudah ada di `WfpCredentialLicense`. Tersisa pekerjaan data: tabelnya masih kosong, dan `MstLicenseType` belum memuat jenis `SIPA` (baru ada `STR Apoteker`) | `PHA-DEC-058` revisi, 4 September 2026 |
| `PHA-OQ-023` | Open Question | Bentuk pencatatan penyerahan per item resep | Product/domain + pharmacy owner | **`closed`** — dijawab `PHA-DEC-059`. Inilah blocker copy resep yang sebenarnya | `PHA-DEC-058` revisi, 4 September 2026 |
| `PHA-OQ-024` | Open Question | Izin operasional rumah sakit tidak punya tempat di master mana pun. `MstHospitalSite` hanya menyediakan `AccreditationNumber`, `MstLegalEntity` hanya NPWP dan nomor izin usaha. Perlu ditetapkan apakah nomor izin operasional dicetak pada dokumen farmasi | Legal/perizinan owner | `open`; **tidak blocking** untuk copy resep | Audit 4 September 2026 |
| `PHA-DEC-061` | Decision | **Format legal copy resep belum diputuskan — `BUSINESS DECISION REQUIRED`.** Sampai ada keputusan: nomor copy resep memakai nomor sistem internal, identitas fasilitas dari `MstHospitalSite`, identitas apoteker dari `WfpCredentialLicense`. Dilarang mengarang nomor SIPA, nomor izin operasional, maupun pernyataan legal seperti *pro copy conform*, dan dilarang mengklaim format yang ada sebagai format legal final. UI final copy resep tidak boleh dibangun sebelum formatnya diputuskan | Product/domain + legal owner | `approved`; menjadi satu-satunya blocker copy resep | Keputusan 7 September 2026 |
| `PHA-DEC-062` | Decision | Retur **tidak** mengurangi jumlah yang sudah diserahkan pada sebuah baris resep. Diresepkan 20, diserahkan 20, diretur 5 tetap menghasilkan diserahkan 20, sisa 0, penanda `det`. Alasannya: histori penyerahan menyatakan obat memang pernah sampai ke pasien, dan retur adalah transaksi tersendiri yang tidak mengubah fakta itu. Retur tetap tercatat dan dapat ditelusuri lewat `TrxDrugUsage`/`TrxDrugReturn` | Product/domain owner | `approved` | Penutupan `PHA-OQ-025`, 7 September 2026 |
| `PHA-OQ-025` | Open Question | Apakah retur mengembalikan sisa sebuah baris resep. Retur pada sistem ini menunjuk pemakaian pada tingkat dokumen (`SourceDrugUsageId`), bukan tingkat baris resep, sehingga jumlahnya tidak dapat dikaitkan kembali ke baris tertentu | Product/domain + pharmacy owner | **`closed`** — dijawab `PHA-DEC-062`: retur tidak mengurangi histori penyerahan | Audit 7 September 2026 |

## Keputusan bisnis 3 September 2026

Dokumen `FARMASI BUSINESS DECISION` diterima dari product owner dan dicatat sebagai
`PHA-DEC-042` sampai `PHA-DEC-053`. Dokumen itu menutup sebagian pertanyaan yang selama ini
memblokir, tetapi tidak menutup seluruhnya.

### Pertanyaan yang ditutup

| Pertanyaan | Ditutup oleh | Catatan |
|---|---|---|
| `PHA-OQ-001` | `PHA-DEC-052` | Urutan sepuluh langkah dan batas slice pertama sudah ditetapkan. |
| `PHA-OQ-003` | `PHA-DEC-046`, `PHA-DEC-049`, `PHA-DEC-051` | Batch, expired, FEFO, dan koreksi stok sudah diputuskan. Karantina dan stock opname belum disebut. |

### Pertanyaan yang tetap terbuka

- `PHA-OQ-002` — `PHA-DEC-047` menetapkan pembagian peran, tetapi SOP formularium, substitusi,
  dan obat high-alert/narkotika belum disebut.
- `PHA-OQ-004` dan `PHA-OQ-016` — partial dispensing, kekurangan stok, duplicate submit, dan
  partial failure lintas modul belum dibahas dokumen 3 September.
- `PHA-OQ-005`, `PHA-OQ-015` — `PHA-DEC-043` menegaskan Billing sebagai authority, tetapi
  reversal tagihan, sumber status paid/refund, dan idempotency callback belum ditetapkan.
- `PHA-OQ-009` — batas pengambilan 24 jam beserta retur, refund, dan notifikasi belum dibahas.
- `PHA-OQ-014` — kegagalan reservasi atomik belum dibahas; lihat juga `PHA-OQ-020`.
- `PHA-OQ-017` — `PHA-DEC-047` menyebut checker mengikuti kebijakan, sehingga daftar obatnya
  justru tetap terbuka.

### Benturan dengan keputusan sebelumnya

Tiga hal berikut tidak boleh diselesaikan tanpa keputusan owner, karena menyentuh kode yang
sudah terpasang dan keputusan yang sudah disetujui sebelumnya.

1. **Lifecycle resep** (`PHA-OQ-018`). Lifecycle `PHA-DEC-044` tidak memuat state pembayaran,
   sedangkan `PHA-DEC-008` dan `PHA-DEC-041` menggantungkan pemrosesan Farmasi pada
   pembayaran/jaminan, dan enum terpasang `PrescriptionFulfillmentStatus` memuat sebelas nilai
   termasuk `WaitingForPayment`, `ReadyForPharmacy`, dan `PartiallyDispensed`. Memetakan enum
   lama ke lifecycle baru begitu saja akan menghapus gating pembayaran yang sudah disetujui.
2. **Reservasi stok** (`PHA-OQ-020`). Daftar validasi `PHA-DEC-046` tidak menyebut reservasi,
   sedangkan `PHA-DEC-041` menetapkannya. Diamnya dokumen baru bukan pencabutan.
3. **Status depo per unit** (`PHA-OQ-019`). `PHA-DEC-050` melarang mengasumsikan setiap unit
   memiliki stok, sehingga saldo stok per lokasi tidak dapat dirancang final sebelum daftarnya
   ditetapkan.

## Acceptance criteria awal

1. Sistem menolak transaksi yang membuat stok lokasi menjadi negatif.
2. Setiap perpindahan stok mencatat lokasi asal, lokasi tujuan, jumlah, pelaku, waktu, dan
   hasil serah-terima.
3. Koreksi resep final mempertahankan resep semula serta hubungan ke pembatalan dan order
   pengganti.
4. Setiap substitusi oleh apoteker atau sistem ditolak; perubahan hanya dapat dilakukan dokter
   melalui perubahan resep yang mempertahankan histori dan memicu rekalkulasi Billing.
5. Pengiriman obat ke unit rawat inap/IGD mengurangi stok tetapi belum menimbulkan tagihan pasien.
6. Pencatatan pemberian obat yang berhasil menimbulkan tagihan tepat satu kali.
7. Retur atau reversal tidak menghapus transaksi asli dan dapat diaudit.
8. Farmasi menolak penyerahan obat rawat jalan umum/tunai ketika Billing belum
   mengonfirmasi pembayaran berhasil.
9. Farmasi tidak dapat mengubah sendiri status pembayaran atau persetujuan penjamin.
10. Obat dengan available stock nol pada farmasi/depo pelayanan tidak ditampilkan pada
    pilihan resep walaupun gudang utama atau lokasi lain masih memiliki stok.
11. Stok hasil transfer baru tersedia untuk resep setelah penerimaan di lokasi tujuan berhasil.
12. Jika dua dokter bersamaan menyimpan resep yang meminta prescribable stock terakhir,
    tepat satu resep berhasil mereservasi dan resep lainnya ditolak.
13. Pembatalan/reversal melepaskan reservasi tepat satu kali dan mempertahankan resep,
    pembayaran, serta audit event aslinya.
14. Penyimpanan resep dengan jumlah melebihi available stock ditolak tanpa membuat resep
    parsial dan tanpa mengganti obat secara otomatis.
15. Reorder point hanya menghasilkan peringatan pengadaan dan tidak mengurangi prescribable
    stock; safety stock mengurangi prescribable stock dan tidak dapat direservasi resep rutin.
16. Pengguna tanpa kewenangan ditolak ketika mengubah ambang stok; perubahan yang berhasil
    merekam lokasi, obat, nilai lama-baru, pelaku, waktu, dan alasan.
17. Obat pulang tidak dicatat sebagai pemberian perawat dan tidak menghasilkan tagihan ganda
    dengan pemberian obat selama rawat inap.
18. Discharge administratif ditolak jika obat pulang belum diserahkan dan tidak ada
    penolakan/tidak mengambil beserta alasan dan edukasi yang terdokumentasi.
19. Sistem tidak mengalokasikan batch expired, quarantine, recall, atau yang kedaluwarsa
    sebelum durasi penggunaan pasien selesai.
20. Setiap obat yang diserahkan dapat ditelusuri dari pasien ke batch dan dari batch ke semua
    pasien penerima; override FEFO tanpa alasan ditolak.
21. Setelah recall aktif, transaksi baru atas batch ditolak dan seluruh lokasi serta pasien
    terdampak dapat diidentifikasi tanpa menghapus histori transaksi sebelumnya.
22. Penyerahan obat khusus tanpa verifikasi apoteker dan pemeriksaan dua petugas ditolak;
    selisih saldo menghentikan transaksi dan menghasilkan eskalasi yang dapat diaudit.
23. Pengguna tidak dapat mengubah saldo secara langsung; adjustment hanya terbentuk dari
    hasil stock opname yang disetujui oleh petugas berbeda dari penghitung.
24. Transaksi selama penghitungan direkonsiliasi terhadap waktu cut-off dan selisih obat
    khusus tidak menghasilkan adjustment otomatis.
25. Pengiriman 100 dan penerimaan 98 menambah stok tujuan sebesar 98 serta mempertahankan
    discrepancy 2 sampai resolusi disetujui dan dapat diaudit.
26. Retur pasien tidak menambah available stock secara otomatis; retur unit hanya menambah
    stok setelah verifikasi apoteker, dan reversal finansial tidak terjadi tanpa alasan approved.
27. Jika obat pengganti karena recall/kerusakan lebih mahal, kewajiban pasien tidak melebihi
    biaya obat semula dan selisih dicatat sebagai beban rumah sakit tanpa menunda penggantian.
28. Penggantian setelah pembayaran tidak menghasilkan permintaan pembayaran tambahan atau
    kunjungan kasir baru bagi pasien; adjustment finansial tetap dapat diaudit secara internal.
29. Resep dengan alergi berat ditolak tanpa override dokter yang sah, alasan klinis, dan
    verifikasi apoteker; seluruh override dapat ditelusuri.
30. Resep dengan interaksi berat ditolak tanpa override dan verifikasi yang sah; audit
    menunjukkan pasangan obat, severity, serta sumber/versi aturan yang digunakan.
31. Developer tidak dapat menetapkan severity klinis; perubahan rule set mempertahankan versi,
    waktu berlaku, approver, dan audit keputusan yang memakai versi sebelumnya.

## Reference status dan blockers

- Indonesia Hospital Domain Reference untuk Pharmacy berstatus `NOT_YET_AVAILABLE`.
- Aturan klinis, farmasi, finansial, dan otorisasi di atas memerlukan verifikasi terhadap SOP
  rumah sakit dan owner terkait sebelum desain final atau implementasi.
- `PHA-OQ-001` memblokir pemilihan delivery slice pertama.
- `PHA-OQ-002` sampai `PHA-OQ-005` memblokir desain lengkap dan acceptance final.

## Handoff scope pass

- Scope pass ditutup pada 19 Agustus 2026 agar discovery tidak berubah menjadi wawancara tanpa
  batas. Pertanyaan detail berikutnya harus dibatasi pada delivery slice yang dipilih.
- Keputusan product owner telah tersedia untuk stok, resep, Billing, rawat inap/pulang,
  FEFO/recall, obat khusus, stock opname, transfer, retur, substitusi, serta clinical alert.
- Approval Clinical/Pharmacy Governance, Billing/Finance, Security, SOP, dan verifikasi regulasi
  tetap diperlukan pada keputusan yang menandainya.
- Langkah berikutnya: `trace-existing-capabilities` untuk membuktikan kemampuan backend dan
  frontend yang dapat dipakai, diperbaiki, atau masih hilang sebelum blueprint target dibuat.

## Status kapabilitas Farmasi per 3 September 2026

Dicatat agar prioritas berikutnya tidak mengulang pekerjaan yang sudah selesai.

### Selesai dan terbukti terhadap PostgreSQL

| Kapabilitas | Bukti |
|---|---|
| Batch dan kedaluwarsa | `MstDrugBatch`; satu nomor batch satu kedaluwarsa |
| Saldo stok per lokasi | `TrxDrugStockBalance`; check constraint saldo tidak negatif |
| Kartu stok / mutasi | `TrxDrugStockMutation`; append-only, koreksi lewat baris baru |
| FEFO | Batch kedaluwarsa terdekat keluar lebih dahulu |
| Reservasi | OnHand − Reserved = Available; reservasi ganda ditolak |
| Karantina sebagai stock state | Stok karantina tidak pernah ikut dilayankan |
| Permintaan stok depo → gudang | Termasuk pemotongan stok saat penyerahan |
| Transfer antar lokasi | Dua langkah keluar–terima; kartu stok dua sisi |
| Pemakaian obat pasien | Draft → dicatat; stok berkurang, batch tersimpan |
| Retur obat | Stok bertambah hanya setelah diperiksa |
| Stok dan kartu stok UI | Layar Stok Farmasi dan Kartu Stok |
| Etiket obat | `PHA-DEC-057`; warna dipilih petugas |

### Tertahan

| Kapabilitas | Sebab |
|---|---|
| Copy resep — data | Selesai; angka diambil dari histori penyerahan, identitas penerbit dari master |
| Copy resep — dokumen | `PHA-DEC-061`; **`BUSINESS DECISION REQUIRED`** untuk format legalnya. UI final belum boleh dibangun |
| Pencatatan penyerahan obat per item | `PHA-DEC-059`; **selesai dan teruji** — `PhmPrescriptionCopy` membacanya sebagai sumber |
| Integrasi Billing | `PHA-OQ-015` closed 21 September 2026 (`PHA-DEC-063`–`065`); tertahan sekarang oleh wewenang implementasi (belum ada task roadmap) dan sign-off lintas pemilik `RJ-BIL-GATE-DEC-007` yang belum lengkap, bukan lagi oleh open question |
| Penerimaan barang dari pemasok | `PHA-DEC-052` menempatkannya pada fase berikutnya |

### Belum diverifikasi

- Pelonggaran otorisasi pengembangan (`Security:Authorization:Enabled = false`) baru terbukti
  lewat pengujian unit, belum dengan akun non-superadmin pada sistem berjalan. Superadmin
  melewati pemeriksaan kebijakan lebih dahulu, sehingga pengujian dengannya tidak membuktikan
  apa pun tentang sakelar itu.

## Closure Amendment 21 September 2026 — Handoff financial clearance Billing → Farmasi

Menutup `PHA-OQ-015` dan sebagian `PHA-OQ-018`. Dipicu permintaan user untuk menutup tiga hal:
linkage `PrescriptionId` ke tagihan/pembayaran, mekanisme Billing memberi tahu Farmasi bahwa
resep *financially clear*, dan pemetaan outcome finansial ke `Paid`/`InsuranceApproved`/
`PaymentWaived`. Seluruh evidence diverifikasi langsung ke source sebelum pertanyaan diajukan;
tidak ada klaim yang diterima tanpa dicek.

### Evidence yang mendasari (dicek langsung ke source, bukan dokumen)

| Bukti | Lokasi | Arti |
|---|---|---|
| `PrescriptionItemId` menautkan penyerahan ke baris resep | `PhmDrugUsageItem` | Linkage `PrescriptionId` → baris resep → catatan penyerahan bersifat deterministic |
| `SourceDomain="PHARMACY"`, `SourceDetailId` = id item penyerahan | `BillingChargeSourceAdapter.cs:36`; `BKC-DEC-039` | Linkage catatan penyerahan → `BilInvoiceItem` → `BilInvoice` bersifat deterministic |
| `BilInvoice.EncounterId` ber-`unique index` | `BilInvoiceConfiguration.cs:26` | Satu kunjungan = tepat satu invoice; charge resep selalu bergabung dengan charge lain di kunjungan yang sama |
| `BilInvoiceItem` tidak punya kolom nominal terbayar per baris | `BilInvoiceItem.cs` | Billing hanya melacak outstanding di level invoice, bukan per item |
| `BillingInvoiceStatuses.Closed` vs `SettledByWriteOff` adalah dua status invoice yang berbeda | `BilInvoice.cs:26-29` | Jalur pembayaran normal dan jalur waiver/write-off sudah bisa dibedakan tanpa skema baru |
| `MstPaymentMethod.IsInsurance` / `IsCompanyGuarantor` | `MstPaymentMethod.cs:21-37` | Tender asuransi/penjamin sudah bisa dibedakan dari tender tunai/kartu tanpa skema baru |
| `PrescriptionPaymentStatus.PaymentStatus` hanya pernah ditulis `NotBilled` di seluruh source | `PrescriptionWorkflowService.cs:39`; tidak ada penulis lain | Tidak ada jalur aktif yang menetapkan `Paid`/`InsuranceApproved`/`PaymentWaived` hari ini |
| `FulfillmentStatus` tidak pernah ditulis `ReadyForPharmacy` di seluruh source | Grep menyeluruh `Areas/HealthServices/PharmacyManagement` | Transisi `WaitingForPayment` → `ReadyForPharmacy` tidak punya jalur kode sama sekali hari ini |
| `PrescriptionReviewService.StartAsync` mensyaratkan `FulfillmentStatus` sudah `QueuedAtPharmacy`/`ReadyForPharmacy` | `PrescriptionReviewService.cs:51-56` | **Akibat gabungan dua baris di atas: setiap resep rawat jalan macet permanen di `WaitingForPayment` sejak `RJ-BIL-BE-002` menghapus jalur lama** — bukan risiko masa depan, tapi kondisi berjalan saat ini |
| `SettledPayments = [Paid, InsuranceApproved, PaymentWaived]` | `PrescriptionDispensingService.cs:58-63` | Tiga outcome yang ditanyakan user sudah menjadi gate di kode, hanya belum pernah terisi |
| `RJ-BIL-GATE-DEC-007` (`locked-draft`, governance `OPEN`) | `rawat-jalan/00-interview-decisions.md` | Rancangan mekanisme read-only financial projection sudah ada, disetujui product owner, belum sign-off lintas pemilik |
| `RJ-BIL-BE-002` `COMPLETE` | `rawat-jalan/execution-evidence-RJ-BIL-BE-002.md` | Jalur lama (Farmasi menulis status bayar sendiri) sudah ditutup 24 Agustus 2026 |
| `RJ-BIL-BE-005` scope-nya multi-payer allocation, bukan projection ini | `rawat-jalan/roadmap/backend-roadmap.md:132` | Belum ada task roadmap untuk membangun proyeksi Billing → Farmasi ini |

### `PHA-DEC-063` — Mekanisme handoff

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Adopsi `RJ-BIL-GATE-DEC-007` sebagai jawaban formal mekanisme handoff Billing → Farmasi. Farmasi menyimpan **read-only financial projection** (bukan event mentah, bukan service call sinkron) yang disinkronkan dari Billing — idempotent, versioned, auditable, reconcilable; versi basi/out-of-order ditolak; kebenaran Billing selalu menang saat konflik. Syarat dispensing bersifat policy-driven, bukan hardcode "harus Paid". Empat endpoint lama tetap tidak aktif (sudah ditutup `RJ-BIL-BE-002`); `WaitingForPayment` dan `ReadyForPharmacy` **dipertahankan** sebagai state gating pembayaran nyata, bukan dihapus dari lifecycle |
| Owner | Product/Domain Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user atas opsi "Adopsi RJ-BIL-GATE-DEC-007 sebagai jawaban formal (Direkomendasikan)" dari 3 opsi, 21 September 2026 |
| Catatan | Governance `RJ-BIL-GATE-DEC-007` awalnya `OPEN` menunggu sign-off Billing/Payer dan Clinical Governance terpisah dari Product/Domain Owner. Ditutup `CLOSED` 21 September 2026 setelah user menyatakan eksplisit mewakili ketiga pihak sekaligus — lihat `PHA-DEC-066` dan pembaruan field "Formal governance status" pada `rawat-jalan/00-interview-decisions.md` |

### `PHA-DEC-064` — Definisi financially clear tingkat invoice

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Karena satu kunjungan = satu invoice (unique constraint `BilInvoice.EncounterId`) dan Billing tidak melacak nominal terbayar per baris item, "resep ini financially clear" didefinisikan sebagai **seluruh invoice kunjungan tersebut berstatus `CLOSED`** (`BKC-DEC-100`) — bukan status per baris item. Konsekuensi disadari dan diterima: item lain pada invoice yang sama yang masih menunggak ikut menahan resep, walau resep itu sendiri "lunas" secara logis |
| Owner | Product/Domain Owner; Billing/Finance perlu memverifikasi |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user atas opsi "Invoice-level: seluruh invoice harus CLOSED (Direkomendasikan)" dari 3 opsi, 21 September 2026 |
| Catatan | Opsi item-level settlement tracking ditolak eksplisit — bukan diam-diam diabaikan. Bila kebutuhan itu muncul kembali di kemudian hari, itu scope baru bagi Billing (menyentuh `BillingInvoiceClosureService`), bukan amandemen kecil atas keputusan ini |

### `PHA-DEC-065` — Pemetaan outcome finansial

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Pemetaan `BilInvoice`/tender ke tiga outcome Farmasi: (1) `BilInvoice.Status = CLOSED` dan ADA sekurang-kurangnya satu tender sukses dengan `MstPaymentMethod.IsInsurance = true` atau `IsCompanyGuarantor = true` → `InsuranceApproved`, berlaku juga untuk tender campuran (sebagian cash/excess pasien, sebagian asuransi/penjamin) — kehadiran satu tender penjamin sudah cukup, tanpa membandingkan proporsi nominal; (2) `BilInvoice.Status = CLOSED` dan seluruh tender sukses tanpa flag asuransi/penjamin → `Paid`; (3) `BilInvoice.Status = SETTLED_BY_WRITE_OFF` → `PaymentWaived` |
| Owner | Product/Domain Owner; Billing/Finance perlu memverifikasi |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user atas opsi "Ada tender asuransi/penjamin → InsuranceApproved (Direkomendasikan)" dari 3 opsi, 21 September 2026, menyusul konfirmasi opsi invoice-level pada `PHA-DEC-064` |

### `PHA-DEC-066` — Kapasitas owner pada closure amendment ini

| Field | Isi |
|---|---|
| Type | Decision |
| Item | User closure amendment ini menyatakan eksplisit menjawab mewakili ketiga pihak sekaligus: Product/Domain Owner, Billing/Payer owner, dan Clinical Governance — bukan hanya Product/Domain Owner seperti pola default seluruh decision log modul lain sebelumnya. Atas dasar ini, governance `RJ-BIL-GATE-DEC-007` yang sebelumnya `OPEN` dinyatakan `CLOSED` |
| Owner | Product/Domain Owner, Billing/Payer owner, Clinical Governance (ketiganya, oleh satu orang yang sama) |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user atas opsi "Mewakili ketiganya" dari 3 opsi, 21 September 2026 |
| Catatan | Ini pola pengecualian dari kebiasaan project sejauh ini (modul lain selalu mencatat sign-off terpisah per pemilik). Bila kelak ternyata Billing/Payer atau Clinical Governance punya pemilik berbeda yang belum diajak bicara, keputusan ini perlu ditinjau ulang — dicatat di sini supaya mudah ditemukan, bukan untuk membatalkan approval yang sudah diberikan |

### `PHA-DEC-067` — Kebijakan outage sinkronisasi proyeksi financial

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Ketika sinkronisasi proyeksi financial dari Billing terputus (state `Unknown`/`PendingVerification`/`Stale` per `RJ-BIL-GATE-DEC-007`), resep **tetap tertahan tanpa override** sampai sinkronisasi pulih dan mengonfirmasi status terbaru — tidak ada jalur manual bagi petugas Farmasi biasa maupun Kepala Farmasi/Supervisor untuk melewati outage teknis ini. Ini **berbeda** dari, dan **tidak mengubah**, invariant clinical urgency exception yang sudah diadopsi lewat `PHA-DEC-063` (`RJ-BIL-GATE-DEC-007`): darurat klinis nyata tetap punya jalurnya sendiri lewat approved policy/reason/authorizing actor yang terpisah dari sekadar "Billing sedang tidak terjangkau" |
| Owner | Product/Domain Owner, Billing/Payer owner, Clinical Governance |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user atas opsi "Tahan total sampai proyeksi pulih, tanpa override (Direkomendasikan)" dari 3 opsi, 21 September 2026 |
| Catatan | Durasi outage yang memicu eskalasi/notifikasi operasional (berapa lama dianggap wajar sebelum jadi insiden) belum digali — dicatat sebagai sisa open item, bukan bagian keputusan fail-closed ini |

## Closure Amendment lanjutan 21 September 2026 — Pencabutan clearance (`PHA-OQ-026`)

Menutup `PHA-OQ-026` yang ditemukan `PHA-RCG-002`. Pemicunya: `BillingInvoiceClosureService.cs:104-126`
membuktikan invoice `CLOSED` **kembali** menjadi `FINAL` ketika sisa tagihan naik lagi
(disetujui pemilik Billing sebagai `BKC-DES-031` dalam `BKC-DEC-105`), sementara tidak satu pun
`PHA-DEC-063`–`067` mengatur akibatnya pada resep yang sudah dikerjakan Farmasi.

### `PHA-DEC-068` — Pencabutan clearance tidak mengikuti status invoice

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Clearance resep **tidak dicabut otomatis** hanya karena invoice kunjungan berubah dari `CLOSED` kembali ke `FINAL`. Clearance ditentukan pada tingkat resep, bukan tingkat seluruh kunjungan. Penambahan biaya yang tidak menyentuh resep — tindakan, laboratorium, kamar — membuat pasien punya kewajiban baru atas biaya itu, tetapi **tidak** membatalkan kenyataan bahwa obatnya sudah dibayar; resep tetap `Paid`/`InsuranceApproved`/`PaymentWaived` dan Farmasi tetap boleh bekerja. Clearance hanya dicabut bila perubahan finansialnya material terhadap resep itu sendiri |
| Owner | Product/Domain Owner, Billing/Payer owner, Clinical Governance (ketiganya per `PHA-DEC-066`) |
| Status | `approved` |
| Approval evidence | Jawaban tertulis user 21 September 2026 yang mengoreksi premis pertanyaan: "jangan otomatis mencabut clearance resep hanya karena invoice kunjungan berubah dari `CLOSED` kembali menjadi `FINAL`; clearance harus ditentukan pada level prescription, bukan level seluruh encounter/invoice" |
| Hubungan dengan `PHA-DEC-064` | **Menyempurnakan, bukan membatalkan.** `PHA-DEC-064` tetap berlaku untuk **pemberian** clearance: resep menjadi clear ketika invoice kunjungan mencapai `CLOSED`. Yang diubah keputusan ini adalah **pencabutannya**, yang sengaja dibuat **tidak simetris** — clearance yang sudah diberikan tidak ikut gugur saat invoice kembali `FINAL` |

**Daftar sebab yang mencabut dan yang tidak.** Dipisah berdasarkan apakah Billing benar-benar
dapat membuktikan perubahan itu menyentuh resep.

| Sebab perubahan | Mencabut clearance? | Dasar kemampuan |
|---|---|---|
| Biaya tindakan, laboratorium, radiologi, atau kamar ditambahkan | **Tidak** | `BilInvoiceItem.SourceDomain` membedakan baris milik resep (`PHARMACY`) dari baris lain |
| Harga atau jumlah obat pada resep itu dikoreksi naik | **Ya** | Perubahan terjadi pada baris ber-`SourceDomain` `PHARMACY` yang `SourceDetailId`-nya menunjuk penyerahan resep itu |
| Pembayaran direversal | **Ya, fail-closed** | Lihat `PHA-DEC-068-A` |
| Penghapusan tagihan (write-off) direversal | **Ya, fail-closed** | Lihat `PHA-DEC-068-A` |
| Penjaminan penjamin dikurangi atau dibatalkan | **Ya, fail-closed** | Lihat `PHA-DEC-068-A` |

### `PHA-DEC-068-A` — Fail-closed untuk sebab berbasis uang

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Untuk tiga sebab yang berbasis penarikan uang — pembayaran direversal, write-off direversal, dan penjaminan dicabut — clearance **selalu** dicabut untuk seluruh resep pada invoice itu, tanpa mencoba menentukan apakah uang yang ditarik itu porsi resep atau porsi layanan lain |
| Owner | Product/Domain Owner, Billing/Payer owner, Clinical Governance |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Selalu cabut clearance (fail-closed)" atas pertanyaan tentang tiga ReasonCode berbasis uang, 21 September 2026 |
| Alasan berbukti | Billing **tidak dapat** membuktikan uang mana milik resep. `BilPaymentAllocation.cs:12,21-24` hanya mengenal satu jenis sasaran alokasi, yaitu `INVOICE` — pembayaran tidak pernah dialokasikan per baris. `BilWriteOffCase.cs:11` juga hanya menyimpan `InvoiceId`. Angka `CoveredAmount`/`PatientPayAmount` per resep memang ada, tetapi tersimpan di sisi Farmasi (`PhmPrescription.cs:157-159`) — justru angka finansial milik modul klinis yang `RJ-BIL-CONFLICT-001` tandai bermasalah dan sedang dipindahkan ke Billing; memakainya sebagai dasar akan memutar balik `PHA-DEC-063` |
| Konsekuensi yang disadari | Reversal yang sebenarnya administratif — misalnya kasir salah memilih metode bayar lalu mengulanginya — akan ikut mencabut clearance walau nominalnya segera dipulihkan. Resep akan tertahan sesaat sampai pembayaran ulang membuat invoice `CLOSED` kembali. Ini diterima sebagai harga dari sikap fail-closed |
| Yang ditolak | Membangun alokasi uang per baris invoice di Billing. Itu scope baru yang besar dan membalik penolakan item-level pada `PHA-DEC-064`. Bila kelak dibutuhkan, itu keputusan tersendiri milik pemilik Billing |

### `PHA-DEC-069` — Perlakuan resep saat clearance benar-benar dicabut

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Ketika pencabutan yang sah terjadi sementara Farmasi sudah mulai bekerja, resep **ditahan di tempat dan kemajuannya dikunci** — bukan ditarik mundur. Resep yang sudah `InPreparation` tetap `InPreparation`; obat yang sudah diracik tetap tercatat sudah diracik dan **tidak** direstock bila secara fisik tidak memungkinkan (`PHA-DEC-032`). Sistem memasang penanda tahan finansial sehingga resep tidak boleh maju ke tahap berikutnya — termasuk `ReadyToDispense` dan penyerahan — sampai Billing menyatakan clear kembali. Setelah pasien melunasi kekurangannya, penahanan dilepas dan proses **dilanjutkan dari titik terakhir**, tanpa mengulang antrean, telaah apoteker, maupun penyiapan |
| Owner | Product/Domain Owner, Billing/Payer owner, Clinical Governance |
| Status | `approved` |
| Approval evidence | Jawaban tertulis user 21 September 2026: "tahan di tempat dan kunci kemajuannya", berikut penegasan "hold dilepas dan proses dilanjutkan dari titik terakhir tanpa mengulang antrean, verifikasi, atau preparation" |
| Alasan | Menarik status mundur akan membuat catatan berbohong tentang keadaan fisik: obat sudah diracik dan stok sudah berkurang, sementara status menyatakan belum diproses — dan saat lunas kembali, apoteker berisiko meracik untuk kedua kalinya. Menahan di tempat menjaga catatan tetap jujur, sejalan dengan `PHA-DEC-032` dan `PHA-DEC-051` |
| Catatan penyajian | Penahanan finansial **wajib terlihat** pada daftar kerja Farmasi beserta sebabnya, karena petugas bisa sedang memegang obatnya saat penahanan terjadi. Bentuk tampilannya `DEV_DISCRETION`; keberadaan dan keterbacaannya bukan |

### `PHA-DEC-070` — Bentuk kontrak clearance

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Kontrak Billing → Farmasi berbentuk pernyataan clearance **per resep**, bukan per invoice, memuat sekurang-kurangnya: identitas resep, identitas invoice, keadaan clearance (`Cleared`/`Revoked`), hasil finansial (`Paid`/`InsuranceApproved`/`PaymentWaived` sesuai `PHA-DEC-065`), kode sebab, nomor versi finansial, dan waktu berlaku. Billing hanya menerbitkan pencabutan untuk sebab yang material terhadap resep sesuai `PHA-DEC-068` dan `PHA-DEC-068-A` |
| Owner | Product/Domain Owner, Billing/Payer owner |
| Status | `approved` |
| Approval evidence | Jawaban tertulis user 21 September 2026 yang menyebut isi kontrak beserta daftar kode sebab yang mencabut dan yang tidak |
| Batas | Ini menetapkan **isi minimum dan tingkat granularitas** kontrak, bukan bentuk teknisnya. Apakah diwujudkan sebagai baris handoff, event, tabel proyeksi, atau gabungannya adalah wewenang `design-business-module`. Pola `BilArHandoff` ↔ `FinBillingHandoffIntake` yang sudah dipakai konsumen lain patut dinilai lebih dulu sebelum membuat pola baru |

### Konsekuensi asimetri yang perlu disadari

`PHA-DEC-064` (pemberian clearance mengikuti invoice `CLOSED`) digabung `PHA-DEC-068`
(pencabutan tidak mengikuti invoice) menghasilkan satu akibat yang ditentukan urutan waktu:

- Pasien lunas pukul 09.00, biaya tindakan yang terlewat baru dicatat pukul 09.30 → resep
  **tetap** clear, obat boleh diserahkan, dan tindakan itu menjadi kewajiban baru pasien.
- Biaya tindakan yang sama dicatat pukul 08.50, sebelum pasien membayar → invoice tidak pernah
  mencapai `CLOSED`, sehingga resep **tidak pernah** clear sampai seluruhnya dilunasi, walau
  porsi obatnya sudah dibayar.

Dua pasien dengan tagihan identik dapat mengalami hasil berbeda semata karena urutan pencatatan.
Ini konsekuensi langsung dari tidak adanya alokasi uang per baris, dan diterima sebagai batas
yang diketahui — bukan cacat yang tersembunyi. Menghilangkannya menuntut keputusan Billing
tersendiri untuk membangun alokasi per baris.

### Acceptance criteria closure amendment ini

1. Resep tidak dapat mencapai `FulfillmentStatus` `QueuedAtPharmacy`/`ReadyForPharmacy` tanpa proyeksi financial dari Billing yang secara eksplisit menyatakan clear — Farmasi tidak pernah menyimpulkannya sendiri.
2. Proyeksi financial di Farmasi menolak versi basi/out-of-order dari Billing; versi terbaru dari Billing selalu menang saat konflik.
3. **Pemberian clearance:** resep yang belum pernah clear dan invoice kunjungannya masih memiliki item menunggak tetap tertahan sampai seluruh invoice berstatus `CLOSED` — bukan diloloskan per baris.
4. Resep pada invoice yang ditutup dengan tender bercampur (ada sekurang-kurangnya satu tender `IsInsurance`/`IsCompanyGuarantor`) tercatat `InsuranceApproved`, bukan `Paid`, terlepas dari proporsi nominal tunai vs asuransi.
5. Resep pada invoice berstatus `SETTLED_BY_WRITE_OFF` tercatat `PaymentWaived`.
6. Saat proyeksi berstatus `Unknown`/`PendingVerification`/`Stale` akibat outage Billing, tidak ada jalur override oleh petugas Farmasi maupun Kepala Farmasi/Supervisor untuk melanjutkan resep tanpa proyeksi valid — berbeda dari jalur clinical urgency exception yang tetap tersedia terpisah.
7. **Pencabutan tidak mengikuti invoice:** resep yang sudah clear, lalu invoicenya kembali `FINAL` semata karena penambahan biaya tindakan, laboratorium, radiologi, atau kamar, **tetap** clear dan Farmasi tetap boleh melanjutkan pekerjaannya.
8. Kenaikan harga atau jumlah obat pada resep itu sendiri mencabut clearance resep tersebut.
9. Reversal pembayaran, reversal write-off, atau pencabutan penjaminan mencabut clearance **seluruh** resep pada invoice itu, tanpa mencoba memilah porsi uangnya.
10. Resep yang clearance-nya dicabut tetap berada pada tahap terakhirnya, tidak dapat maju ke tahap berikutnya, tidak dapat diserahkan, dan obat yang sudah diracik tidak direstock.
11. Resep yang clearance-nya dipulihkan melanjutkan dari tahap terakhirnya — antrean, telaah apoteker, dan penyiapan yang sudah selesai tidak diulang.
12. Penahanan finansial terlihat pada daftar kerja Farmasi beserta sebabnya, sehingga petugas yang sedang memegang obat mengetahui alasan pekerjaannya terhenti.

### `PHA-DEC-071` — Approval keputusan arsitektur slice Financial Clearance

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menyetujui `PHA-DES-001`–`PHA-DES-006` secara utuh** — satu baris proyeksi per resep yang diperbarui di tempat (`001`), surat bernomor versi lebih rendah ditolak diam-diam tanpa menimbulkan galat (`002`), penanda tahan finansial dihitung dan tidak disimpan sebagai kolom (`003`), `PaymentStatus` dipertahankan sebagai salinan satu-penulis (`004`), gerbang dipasang di empat titik yaitu telaah, penyiapan, pemeriksaan akhir, dan penyerahan (`005`), serta keadaan tidak diketahui diperlakukan sebagai belum boleh (`006`). Dengan ini keenam sumbu kontrak `PHA-*-CLEARANCE-v1` naik dari `draft` menjadi `approved` |
| Owner | Product/Domain Owner, Billing/Payer owner, Clinical Governance |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Saya setujui desainnya sekarang" atas pertanyaan gerbang `plan-module-delivery`, 21 September 2026 |
| Batas approval | Bukan wewenang menulis source maupun migration; keduanya diminta terpisah per task. Seluruh kontrak `PHA-*-ROUTING-v1` tidak tersentuh dan tetap `approved` apa adanya |
| Ketergantungan yang tetap berlaku | Sisi penerbit Billing (`BKC-DES-036`–`041`) **MUST** berdiri lebih dulu. Slice ini tidak dapat dieksekusi tanpanya, dan itu bukan pilihan urutan melainkan kenyataan: tanpa surat yang terbit, tidak ada masukan apa pun |

### Pertanyaan yang tetap terbuka setelah closure amendment ini

- ~~**`PHA-OQ-026` — pencabutan status lunas atas resep yang sudah diproses**~~ — **`closed`** 21 September 2026 oleh `PHA-DEC-068`, `PHA-DEC-068-A`, `PHA-DEC-069`, dan `PHA-DEC-070`. Jawabannya mengoreksi premis pertanyaan: pencabutan tidak mengikuti status invoice, melainkan ditentukan pada tingkat resep; dan bila pencabutan memang sah, resep ditahan di tempat, bukan ditarik mundur.
- **`PHA-OQ-027` — kebijakan pengecualian darurat klinis** (tidak memblokir, ditemukan `PHA-RCG-002`). `RJ-BIL-GATE-DEC-007` menyediakan jalur darurat klinis yang mendahului penyelesaian finansial, tetapi kebijakan yang disahkan beserta pemberi wewenangnya belum ada. Selama terbuka, satu-satunya jalan resep masuk antrean adalah pernyataan lunas dari Billing. Pemilik: Clinical Governance + Billing/Finance.
- **Belum ada task roadmap** — tidak ada task backend yang mencakup pembangunan projection sync ini; `RJ-BIL-BE-005` scope-nya multi-payer allocation, bukan ini. Perlu ditambahkan ke roadmap `rawat-jalan` dan/atau `pharmacy` pada `plan-module-delivery`.
- **Manifest `PHA-BP-001` usang** — mencatat `decision_revision: 2` dan SHA `767470f7`/`400104f2`, padahal decision log sudah di `PHA-DEC-067` dan SHA kini `6782ae65`/`1b138b9a`; `domain_architecture_readiness` hanya berlaku untuk slice Routing Depo. Perlu disinkronkan sebelum atau saat desain slice ini.
- **Durasi outage sebelum eskalasi** — `PHA-DEC-067` menutup perilaku fail-closed-nya, tapi berapa lama outage dianggap wajar sebelum menjadi insiden operasional (SLA, notifikasi ke siapa) belum digali.
- **Pemetaan penuh `PHA-DEC-044` ke enum sebelas-state** — `PHA-OQ-018` baru tertutup sebagian; kepastian bahwa gating pembayaran tetap ada tidak sama dengan rancangan pemetaan state-machine yang lengkap.
- **Reversal/refund dan idempotency callback detail** — tetap milik `PHA-OQ-005`, belum digali pada closure amendment ini.

### Status kapabilitas — pembaruan baris "Integrasi Billing"

Baris "Integrasi Billing" pada tabel "Tertahan" (§ Status kapabilitas Farmasi per 3 September
2026) diperbarui: sebab tertahannya sekarang **bukan lagi** "menunggu `PHA-OQ-005` dan
`PHA-OQ-015`" — `PHA-OQ-015` sudah tertutup lewat closure amendment ini. Sebab tertahan yang
tersisa: **belum ada wewenang implementasi** (task roadmap belum dibuat) dan **sign-off lintas
pemilik `RJ-BIL-GATE-DEC-007` belum lengkap**, sebagaimana tercatat pada bagian "Pertanyaan yang
tetap terbuka" di atas.
