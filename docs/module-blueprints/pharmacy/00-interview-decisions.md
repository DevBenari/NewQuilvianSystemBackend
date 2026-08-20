# Farmasi — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `PHA-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Interview mode | `Scope pass` |
| Scope-pass result | `concluded 19 Agustus 2026`; keputusan lintas-owner masih perlu verifikasi |
| Product/domain owner | User — menyatakan sebagai pemilik keputusan pada 18 Agustus 2026 |
| Backend SHA | `36d7eca7cd3d4b3f1f6520a6fe9340936cced320` |
| Frontend SHA | `400104f2a0f3239c14c40f5905b419977a538450` |
| Input evidence | Jawaban wawancara 18–19 Agustus 2026 |

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
| `PHA-DEC-019` | Decision | `Prescribable stock = usable on-hand - reserved - safety stock`; penyimpanan resep menaikkan reserved secara atomik, sedangkan on-hand baru berkurang ketika obat diserahkan | Product/domain + pharmacy/warehouse owner | `approved` oleh product owner | User memilih rekomendasi assistant, 19 Agustus 2026 |
| `PHA-DEC-020` | Decision | Pisahkan `reorder point` untuk peringatan pengadaan dan `safety stock` untuk jumlah yang tidak tersedia bagi resep rutin | Product/domain + pharmacy/warehouse owner | `approved` oleh product owner | User memilih rekomendasi assistant, 19 Agustus 2026 |
| `PHA-DEC-021` | Decision | Reservasi stok terjadi saat resep disimpan, tetapi resep baru masuk antrean/cetak Farmasi setelah pembayaran atau jaminan dikonfirmasi | Product/domain + pharmacy + Billing owner | `approved` oleh product owner | User memilih rekomendasi assistant, 19 Agustus 2026 |
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
