# Rekam Medis — Keputusan Wawancara

| Field | Nilai |
|---|---|
| Blueprint ID | `QV-RM-001` |
| Revision | `1` |
| Status | `draft` |
| Product/domain owner | Unit Rekam Medis |
| Clinical approval authority | Komite Medis/Direktur Pelayanan Medis |
| Backend SHA | `5103e68` |
| Frontend SHA | `c4e2ef2a6` |

## Scope dan Hasil yang Diharapkan

Modul Rekam Medis mengelola dokumentasi klinis pasien yang terikat pada kunjungan atau
episode perawatan rawat jalan, IGD, dan rawat inap, termasuk pengesahan, koreksi, privasi,
dan jejak audit.

### Di dalam scope

- Catatan dokter, perawat, dan tenaga kesehatan lain.
- Status draft, final atau ditandatangani, koreksi, addendum, pembatalan, dan pembukaan kembali.
- Catatan terlambat, duplikasi pengiriman, kondisi sistem tidak tersedia, dan kegagalan sebagian.
- Hak akses, kerahasiaan, audit, dan riwayat perubahan.
- Titik sentuh dengan identitas pasien, kunjungan, dan hasil pemeriksaan penunjang.

### Di luar scope — untuk modul lain

- Master identitas pasien.
- Pendaftaran, admission, jadwal, dan pengelolaan tempat tidur.
- Proses internal laboratorium, radiologi, farmasi, dan tindakan.
- Tarif, billing, klaim, dan pembayaran.
- Implementasi source code, endpoint, database, serta keputusan desain antarmuka pengguna.

Modul tetangga hanya dibahas sebatas data bersama, kontrak integrasi, dan urutan proses yang
bersentuhan dengan Rekam Medis.

Laboratorium dan radiologi tetap menjadi pemilik order, hasil, validasi, penanganan hasil kritis,
dan acknowledgment. Rekam Medis menerima referensi serta salinan hasil yang sudah dirilis dan
ditandatangani, lalu menyimpan bukti versi hasil yang diterima. Salinan tersebut bukan sumber
kebenaran baru yang menggantikan modul penunjang.

Jika owner penunjang mengoreksi atau menarik hasil, Rekam Medis menyimpan seluruh versi. Versi lama
ditandai `Digantikan` atau `Ditarik`, lalu ditautkan ke versi baru tanpa menimpa isi lama. Owner
penunjang mengirim event koreksi dan notifikasi kepada DPJP atau tim aktif.

Rekam Medis menerbitkan status kelengkapannya untuk dipakai modul coding, claim, dan billing.
Masing-masing proses finansial menentukan readiness-nya sendiri. Status finansial tidak boleh
menahan tanda tangan catatan atau perubahan Rekam Medis menjadi `Ditutup Final`.

## Aktor dan Tanggung Jawab

| Aktor | Tanggung jawab |
|---|---|
| Tenaga kesehatan pembuat catatan | Membuat, menandatangani, dan mengoreksi catatannya sendiri sesuai kewenangan profesi. |
| DPJP atau pengganti resmi | Memberikan koreksi pengganti pada episode aktif ketika pembuat tidak tersedia. |
| Kepala Pelayanan/pejabat yang ditunjuk Komite Medis | Memberikan koreksi pengganti setelah episode ditutup. |
| Unit Rekam Medis | Memiliki proses, menetapkan checklist bersama Komite Medis, memantau kelengkapan, memverifikasi pelepasan informasi, dan meninjau akses darurat. |
| Komite Medis/Direktur Pelayanan Medis | Memberikan approval klinis dan ikut menetapkan aturan klinis lintas layanan. |
| Pejabat privasi/hukum yang ditunjuk | Menyetujui aturan privasi dan pelepasan informasi serta meninjau akses darurat. |
| Kepala Pelayanan | Memberikan konteks klinis ketika akses darurat ditinjau. |

Seluruh jawaban pada sesi wawancara ini tetap berstatus `draft`. Approval formal diberikan terpisah
oleh individu bernama dari Unit Rekam Medis untuk proses operasional, Komite Medis/Direktur
Pelayanan Medis untuk aturan klinis, dan pejabat privasi/hukum untuk privacy serta release. Bukti
approval berupa memo persetujuan bertanda tangan yang menyimpan identitas setiap approver, waktu
persetujuan, tanggal berlaku, serta versi dan hash decision log yang disetujui. Memo menjadi artefak
approval yang ditautkan ke revision blueprint terkait.

Pada 21 Agustus 2026, pengguna mengonfirmasi bahwa individu untuk ketiga kewenangan approval belum
ditunjuk. Blueprint tetap `draft`; konfirmasi ini bukan pengganti penunjukan atau approval formal.

Pada fase development, ketiadaan approver formal tidak menghentikan analisis requirement,
arsitektur domain, penyusunan desain draft, atau persiapan backlog draft. Seluruh artefak tetap
berstatus `draft` dan tidak boleh disebut approved. Approval formal tetap wajib sebelum kontrak
diaktifkan, fitur berisiko dinyalakan, deployment produksi, atau sign-off kesiapan modul. Akses
darurat, release, retensi/penghapusan, serta policy klinis yang mensyaratkan approval tetap
fail-closed selama approval belum tersedia.

## Proses Bisnis Utama

**Tujuan:** menghasilkan Rekam Medis yang sah, lengkap, terlindungi, dan dapat ditelusuri
untuk setiap episode rawat jalan, IGD, dan rawat inap.

**Pemicu:** episode pelayanan pasien dimulai oleh modul pelayanan yang berwenang.

**Prasyarat:** identitas pasien dan episode pelayanan tersedia; pengguna memiliki peran serta
hubungan pelayanan atau penugasan yang sah; checklist versi aktif dapat ditentukan.

1. Sistem mengikat episode ke versi checklist yang aktif saat episode dimulai.
2. Tenaga kesehatan membuat catatan sesuai kewenangan profesi.
3. Pembuat menandatangani catatan. Catatan menjadi `Final` dan tidak dapat ditimpa.
4. Saat layanan berakhir, sistem memeriksa seluruh dokumen wajib.
5. Jika ada dokumen wajib yang belum ditandatangani, Rekam Medis menjadi `Belum Lengkap` dan
   mengikuti pengingat serta eskalasi yang berlaku.
6. Setelah seluruh dokumen wajib ditandatangani, Rekam Medis menjadi `Ditutup Final`.
7. Koreksi setelah tanda tangan dibuat sebagai catatan terpisah dan tidak menghapus versi lama.

**Jalur tidak normal yang sudah diputuskan:** pembuat tidak tersedia saat koreksi, akses di
luar hubungan pelayanan karena keadaan darurat, keterlambatan kelengkapan, dan permintaan
pelepasan informasi sensitif. Penanganan duplikasi, downtime, data terlambat, kegagalan
integrasi sebagian, dan pembatalan episode masih menjadi pertanyaan terbuka.

**Hasil akhir:** episode memiliki status kelengkapan yang dapat diuji, catatan final beserta
riwayat koreksinya tetap utuh, dan seluruh akses serta pelepasan informasi dapat diaudit.

Modul menyediakan worklist episode belum lengkap menurut layanan, jenis dokumen, penanggung jawab,
dan umur keterlambatan. Laporan lain mencakup signature/koreksi/`Entered in Error`, review akses
darurat, audit pelepasan informasi, serta status downtime dan sinkronisasi. Hak melihat dan
mengekspor laporan dibatasi per peran; setiap ekspor diaudit.

Worklist tersedia secara real-time. Sistem mengirim ringkasan harian pukul 07.00 WIB kepada Unit
Rekam Medis dan Kepala Pelayanan, laporan mingguan setiap Senin pukul 08.00 WIB kepada Komite Medis,
serta laporan bulanan paling lambat pada hari kerja kelima bulan berikutnya kepada manajemen dan
pejabat privasi/hukum. Akses darurat dan dugaan penyalahgunaan tetap menghasilkan notifikasi
berbasis kejadian sesuai SLA review; penerima tidak menunggu laporan terjadwal.

**Contoh distribusi:** Kekurangan signature yang muncul pukul 09.00 langsung terlihat di worklist dan
ikut masuk ringkasan harian berikutnya. Dugaan penyalahgunaan akses yang muncul pukul 10.00 langsung
dikirim untuk review dan tidak ditahan sampai laporan mingguan atau bulanan.

Sistem tidak menghapus laporan atau audit secara otomatis sebelum policy retensi per jenis data
memiliki tanggal berlaku dan approval. Legal hold selalu menghentikan penghapusan data terkait
sampai pejabat berwenang mencabut hold tersebut.

Setelah policy disahkan, Rekam Medis beserta seluruh versi, tanda tangan, koreksi, dan catatan
`Entered in Error` disimpan minimal 25 tahun sejak interaksi terakhir pasien. Audit akses,
break-glass, pelepasan informasi, dan ekspor disimpan 10 tahun sejak kejadian. Bukti downtime dan
integrasi disimpan 10 tahun sejak rekonsiliasi selesai. Snapshot laporan terjadwal disimpan 5 tahun.
Worklist real-time tidak memiliki salinan retensi terpisah karena dibentuk ulang dari data sumber.
Jika legal hold atau aturan lain mewajibkan masa lebih panjang, sistem memakai batas yang lebih
panjang dan tidak menghapus data saat legal hold masih aktif.

**Contoh worklist:** Petugas Unit Rekam Medis memfilter episode rawat inap dengan ringkasan pulang
belum ditandatangani dan mengurutkannya dari umur keterlambatan tertinggi. Pengguna yang hanya
berwenang memantau satu unit tidak dapat melihat unit lain. Ketika laporan diekspor, sistem mencatat
pelaku, waktu, filter, dan ruang data ekspor.

**Contoh legal hold:** Audit release sudah mencapai akhir masa retensi, tetapi sedang terkait
pemeriksaan hukum. Sistem tidak menghapus audit selama legal hold aktif. Penghapusan baru dapat
dinilai lagi setelah pejabat berwenang mencabut hold.

**Contoh retensi:** Interaksi terakhir pasien terjadi 1 Agustus 2026. Rekam Medis dan seluruh
riwayatnya tidak boleh dipertimbangkan untuk penghapusan sebelum 1 Agustus 2051. Audit ekspor yang
terjadi 10 Agustus 2026 tidak boleh dipertimbangkan sebelum 10 Agustus 2036. Jika keduanya berada
dalam legal hold pada tanggal tersebut, keduanya tetap disimpan sampai hold dicabut.

Setiap tindakan yang membuat atau mengubah keadaan resmi harus membawa kunci permintaan unik.
Jika request dikirim ulang dengan kunci dan isi yang sama, sistem mengembalikan hasil pertama tanpa
membuat data kedua. Jika kuncinya sama tetapi isinya berbeda, sistem menolak sebagai konflik.

**Contoh retry:** Dokter menekan tombol tanda tangan, tetapi respons terlambat sehingga aplikasi
mengirim ulang request yang sama. Sistem mengembalikan bukti signature pertama dan tidak membuat
signature kedua. Jika request kedua membawa isi catatan berbeda dengan kunci yang sama, sistem
menolak dan meminta pengguna menyelesaikan konflik.

Ketika sistem tidak tersedia, tenaga kesehatan menggunakan formulir downtime resmi yang memiliki
nomor unik. Setelah sistem pulih, petugas memasukkan catatan sebagai `Entri Downtime` dengan waktu
kejadian dan waktu input yang disimpan terpisah. Pembuat melakukan autentikasi ulang dan
menandatangani entri tersebut. Rekonsiliasi memakai nomor formulir untuk mencegah catatan ganda.

**Contoh downtime:** Pemeriksaan terjadi pukul 10.00 ketika sistem mati dan dicatat pada formulir
`DT-00125`. Sistem pulih pukul 13.00. Petugas memasukkan isi sebagai `Entri Downtime`, menyimpan
waktu kejadian 10.00 dan waktu input 13.00. Dokter melakukan autentikasi ulang serta tanda tangan.
Jika formulir `DT-00125` dimasukkan kembali, sistem menolaknya sebagai duplikat.

Jika catatan berhasil ditandatangani tetapi pengiriman ke modul lain gagal, catatan tetap sah dan
tidak dihapus. Sistem menandai integrasi `Sinkronisasi Tertunda`, menyimpan event secara tahan gagal,
mencoba ulang otomatis, dan menyediakan rekonsiliasi manual. Status `Ditutup Final` hanya tertahan
jika kegagalan membuat item wajib belum terbukti tersedia.

**Contoh partial failure:** Catatan tindakan sudah ditandatangani, tetapi event ke Rekam Medis gagal
dikirim. Owner catatan tidak membatalkan signature. Event masuk antrean tahan gagal dan status
integrasi menjadi `Sinkronisasi Tertunda`. Jika catatan tindakan adalah item wajib dan Rekam Medis
belum menerima buktinya, episode belum boleh `Ditutup Final` sampai retry atau rekonsiliasi berhasil.

Dokumen atau hasil yang tiba setelah `Ditutup Final` disimpan sebagai dokumen pasca-penutupan dan
membuat item review; sistem tidak membuka episode secara otomatis. Jika review membuktikan dampak
terhadap kewajiban atau keselamatan, Unit Rekam Medis bersama pejabat klinis dapat membuka kembali
episode menjadi `Belum Lengkap` dengan alasan dan audit.

**Contoh data terlambat:** Hasil koreksi laboratorium tiba sehari setelah episode ditutup. Sistem
menyimpan hasil sebagai dokumen pasca-penutupan dan membuat review. Jika hasil hanya menambah konteks,
episode tetap final. Jika hasil menunjukkan kewajiban atau tindak lanjut keselamatan belum selesai,
Unit Rekam Medis dan pejabat klinis membuka kembali episode dengan alasan tercatat.

## Aturan Bisnis dan Invariant

### Tata kelola privasi

Unit Rekam Medis menyusun aturan privasi dan pelepasan informasi medis. Pejabat privasi atau
hukum yang ditunjuk manajemen memberikan approval atas aturan tersebut. Aturan akses dalam
keadaan darurat harus mendapat approval bersama dari Komite Medis.

Akses normal tenaga kesehatan memerlukan dua syarat: pengguna memiliki peran yang sesuai dan
memiliki hubungan pelayanan aktif dengan pasien atau penugasan resmi. Peran saja tidak cukup
untuk membuka seluruh Rekam Medis pasien rumah sakit.

Hubungan pelayanan aktif hanya berasal dari penugasan formal yang memiliki waktu mulai dan akhir,
misalnya sebagai anggota tim perawatan, DPJP, perawat penanggung jawab, konsultan, atau pengganti
resmi. Akses normal dimulai ketika penugasan berlaku dan berakhir ketika penugasan dicabut, diganti,
atau episode ditutup. Berada pada unit, shift, atau profesi yang sama tidak otomatis membentuk
hubungan pelayanan.

Setelah episode ditutup, mantan anggota tim tidak lagi memiliki akses normal. Akses lanjutan hanya
dapat diberikan melalui penugasan follow-up baru atau workflow koreksi/addendum yang mencatat
tujuan, batas data yang boleh dibuka, dan masa berlaku tertentu.

Jika keadaan darurat mengharuskan akses di luar hubungan pelayanan, pengguna harus melakukan
autentikasi ulang dan mengisi alasan. Akses bersifat sementara. Sistem mengaudit seluruh
aktivitas selama akses darurat dan mengirim pemberitahuan otomatis untuk peninjauan.
Unit Rekam Medis dan pejabat privasi meninjau seluruh penggunaan akses darurat. Kepala
Pelayanan dilibatkan jika penilaian membutuhkan konteks klinis. Dugaan penyalahgunaan
dieskalasi sesuai tata kelola rumah sakit.
Akses darurat tidak boleh diaktifkan sebelum policy menetapkan reviewer, batas waktu review, jenis
hasil review, serta jalur eskalasi dugaan penyalahgunaan, dan seluruh konfigurasi tersebut disahkan.
Seluruh akses darurat harus direview paling lambat satu hari kerja. Pembukaan kategori sangat
sensitif harus direview paling lambat empat jam. Dugaan penyalahgunaan dieskalasi segera tanpa
menunggu batas waktu review.
Empat jam untuk kategori sangat sensitif dihitung sebagai waktu berjalan selama 24 jam sehari.
Jika terjadi di luar jam kerja, reviewer on-call menerima eskalasi.

Hasil review memakai status `Sesuai`, `Perlu Klarifikasi`, `Dugaan Penyalahgunaan`,
`Penyalahgunaan Terkonfirmasi`, atau `Tidak Terbukti`. Semua hasil wajib menyimpan alasan dan
evidence. Status `Penyalahgunaan Terkonfirmasi` hanya dapat ditetapkan setelah investigasi oleh
pejabat privasi atau hukum bersama manajemen. Unit Rekam Medis dan pejabat privasi menjadi reviewer
awal; Kepala Pelayanan memberi konteks klinis bila diperlukan.
Akses darurat berakhir setelah 15 menit atau lebih cepat ketika pengguna mendapat penugasan resmi.
Perpanjangan selama 15 menit berikutnya memerlukan autentikasi ulang dan alasan baru.
Jika durasi, tanggal berlaku, dan approval policy belum tersedia, akses darurat tidak boleh
diaktifkan. Sistem harus menampilkan bahwa policy durasi belum dikonfigurasi dan tidak boleh
menggunakan angka bawaan atau durasi yang dipilih developer.
Selama akses darurat, pengguna boleh melihat informasi klinis yang diperlukan dan membuat
catatan baru sesuai kewenangan profesinya. Pengguna tidak boleh mengubah catatan final lama,
mengunduh data secara massal, atau melakukan pelepasan informasi medis. Kewenangan pada modul
tetangga tetap mengikuti izin modul masing-masing.
Saat akses darurat dimulai, data inti keselamatan tersedia terlebih dahulu: alergi, obat aktif,
masalah atau diagnosis penting, episode aktif, dan hasil terbaru. Riwayat tambahan dibuka per
kategori dengan alasan tambahan, dan tindakan tersebut masuk audit.
Kategori yang ditetapkan rumah sakit sebagai sangat sensitif tetap tersembunyi secara awal.
Pembukaannya memerlukan autentikasi ulang dan alasan khusus, lalu masuk peninjauan prioritas.
Dalam keadaan darurat, pengguna tidak perlu menunggu persetujuan sebelum membuka, tetapi
seluruh tindakannya tetap diaudit. Daftar kategori harus disahkan pejabat privasi atau hukum
bersama Komite Medis.

Kategori sangat sensitif meliputi HIV atau infeksi menular seksual, kesehatan jiwa, penggunaan zat,
kesehatan seksual atau reproduksi, data genetik, kekerasan atau pelecehan, serta catatan forensik
atau perkara hukum.

Unit Rekam Medis dan pejabat privasi memelihara aturan klasifikasi terpusat berdasarkan jenis
dokumen dan kode klinis. Komite Medis menyetujui makna klinis pemetaannya. Penandaan manual atau
pelepasan data dari kategori sensitif memerlukan alasan dan review.

Akses darurat baru dapat diaktifkan jika policy durasi dan daftar kategori sangat sensitif telah
sama-sama disahkan. Sistem tidak boleh menebak klasifikasi data atau mengaktifkan akses darurat
dengan salah satu bagian policy tersebut belum tersedia.

**Contoh:** Unit Rekam Medis mengusulkan kondisi tenaga kesehatan boleh membuka Rekam Medis
di luar unitnya saat keadaan darurat. Aturan tidak dianggap berlaku hanya karena sudah dibuat;
pejabat privasi atau hukum dan Komite Medis harus memberikan approval sesuai kewenangannya.

**Contoh akses normal:** Dokter yang tercatat dalam tim perawatan pasien dapat membuka catatan
yang diperlukan untuk pelayanan. Dokter dari unit lain yang tidak merawat pasien dan tidak
memiliki penugasan resmi ditolak, walaupun sama-sama berprofesi sebagai dokter.

**Contoh masa penugasan:** Dokter konsultan ditugaskan mulai pukul 09.00 sampai pukul 15.00.
Akses normalnya berlaku dalam rentang tersebut. Jika penugasan dicabut pukul 13.00, akses normal
berakhir saat itu juga. Dokter lain pada unit yang sama tidak memperoleh akses hanya karena sedang
bertugas pada shift yang sama.

**Contoh akses setelah penutupan:** Dokter yang pernah merawat pasien perlu membuat addendum dua
hari setelah episode ditutup. Keanggotaan tim lama tidak otomatis memberinya akses. Dokter masuk
melalui workflow addendum dengan tujuan dan ruang data yang ditentukan; akses berakhir ketika
workflow selesai atau masa berlakunya habis.

**Contoh akses darurat:** Pasien tidak sadar tiba di IGD dan dokter jaga belum tercatat sebagai
anggota tim pasien. Dokter melakukan autentikasi ulang, memilih alasan keadaan darurat, lalu
membuka Rekam Medis dalam waktu terbatas. Sistem mencatat siapa yang membuka, waktu, alasan,
data yang dilihat atau diubah, dan mengirim pemberitahuan untuk ditinjau.

**Contoh review belum siap:** Durasi dan kategori sensitif sudah disahkan, tetapi rumah sakit belum
menetapkan tenggat review serta jalur eskalasi. Sistem tetap menolak aktivasi akses darurat karena
setiap penggunaan harus memiliki reviewer dan tindak lanjut yang jelas.

**Contoh prioritas:** Akses darurat biasa terjadi Senin pukul 09.00 dan harus selesai direview paling
lambat satu hari kerja. Jika akses membuka kategori sangat sensitif, target review berubah menjadi
empat jam. Jika sejak awal terindikasi penyalahgunaan, sistem langsung mengeskalasi tanpa menunggu
empat jam atau satu hari kerja.

**Contoh on-call:** Kategori sensitif dibuka pukul 23.00. Tenggat review tetap pukul 03.00, bukan
empat jam kerja pada hari berikutnya. Sistem mengirim eskalasi kepada reviewer on-call.

**Contoh hasil review:** Reviewer awal menemukan alasan akses belum cukup dan memberi status
`Perlu Klarifikasi`. Setelah keterangan pengguna dan Kepala Pelayanan diperiksa, hasil dapat menjadi
`Sesuai`. Jika muncul indikasi penyalahgunaan, status menjadi `Dugaan Penyalahgunaan`; status
terkonfirmasi baru diberikan setelah investigasi privacy/legal dan manajemen.

**Contoh akhir akses darurat:** Dokter mengaktifkan akses darurat, lalu lima menit kemudian
resmi ditambahkan ke tim pelayanan pasien. Sesi darurat berakhir dan akses berikutnya memakai
hubungan pelayanan normal. Jika belum ada penugasan dan sesi darurat kedaluwarsa, dokter harus
melakukan autentikasi ulang serta memberikan alasan baru untuk memperpanjang.

**Contoh durasi:** Akses darurat dimulai pukul 10.00 dan tidak ada penugasan resmi yang terbentuk.
Sesi berakhir pukul 10.15. Jika akses masih diperlukan, dokter melakukan autentikasi ulang dan
memberikan alasan baru untuk sesi berikutnya; sistem tidak memperpanjang secara otomatis.

**Contoh policy belum tersedia:** Dokter mencoba mengaktifkan akses darurat ketika rumah sakit
belum mengesahkan durasinya. Sistem menolak aktivasi dan menampilkan `Policy durasi akses darurat
belum dikonfigurasi`. Sistem tidak memakai 30 menit atau angka lain sebagai nilai sementara.

**Contoh tindakan:** Dokter IGD dengan akses darurat dapat membaca informasi yang diperlukan
dan menandatangani catatan pemeriksaan baru. Dokter tidak dapat mengubah catatan final dokter
lain atau memakai akses darurat untuk mengekspor seluruh Rekam Medis pasien.

**Contoh perluasan data:** Dokter awalnya melihat alergi, obat aktif, diagnosis penting,
episode aktif, dan hasil terbaru. Jika riwayat operasi lama diperlukan, dokter membuka kategori
tersebut dengan menuliskan alasan tambahan. Sistem tidak membuka seluruh riwayat secara otomatis.

**Contoh data sangat sensitif:** Jika suatu kategori telah ditetapkan kebijakan sebagai sangat
sensitif, kategori itu tidak ikut tampil bersama data inti. Dokter harus melakukan autentikasi
ulang dan menuliskan alasan klinis khusus. Sistem segera mencatat akses untuk peninjauan
prioritas. Contoh ini tidak menetapkan kategori mana yang termasuk sangat sensitif.

**Contoh kategori:** Catatan layanan kesehatan jiwa tidak ikut terbuka bersama data inti akses
darurat. Pengguna melakukan autentikasi ulang dan menulis alasan khusus sebelum membukanya; akses
tersebut langsung masuk antrean review prioritas.

**Contoh pemetaan:** Policy memetakan jenis dokumen tertentu dan kode klinis tertentu ke kategori
kesehatan jiwa. Pembuat catatan tidak perlu mengingat klasifikasinya. Jika petugas meminta agar
sebuah dokumen dikeluarkan dari kategori tersebut, sistem mewajibkan alasan dan review; perubahan
tidak berlaku hanya karena petugas mengubah label.

**Contoh konfigurasi belum lengkap:** Durasi akses darurat sudah disahkan, tetapi daftar kategori
sangat sensitif belum tersedia. Sistem tetap menolak aktivasi akses darurat dan tidak menganggap
semua data sebagai data biasa.

### Pelepasan informasi medis

Pelepasan informasi medis harus diawali permintaan formal yang mencatat pemohon, kewenangan
atau dasar permintaan, tujuan, penerima, dan ruang data. Unit Rekam Medis memverifikasi serta
menyetujui permintaan. Pejabat privasi atau hukum meninjau kasus sensitif dan pengecualian.
Sistem harus mencatat data yang disetujui, pihak yang menyerahkan, penerima, waktu, dan hasil
penyerahan.

Setelah disetujui, proses penyerahan dapat berstatus `Disiapkan`, `Diserahkan`, `Diserahkan
Sebagian`, `Gagal`, `Dibatalkan`, `Kedaluwarsa`, atau `Dicabut`. Percobaan ulang hanya boleh
dilakukan dalam ruang data serta masa berlaku approval yang sama. Permintaan baru diwajibkan jika
approval telah kedaluwarsa atau dicabut.

Pemohon boleh membatalkan permintaan hanya sebelum penyerahan pertama. Unit Rekam Medis boleh
membatalkan karena masalah operasional. Pejabat privasi atau hukum boleh mencabut approval sebelum
atau selama sisa penyerahan. Data yang sudah diterima pihak tujuan tidak dianggap dapat ditarik
kembali; kejadian tersebut harus dicatat dan ditangani melalui proses insiden/review.

Pelepasan informasi dinonaktifkan sampai policy memiliki daftar bukti dan pengecualian yang telah
disahkan untuk setiap jenis pemohon, termasuk pasien, wali, keluarga, penjamin, aparat, pengadilan,
dan pihak lain. Petugas tidak boleh mengganti matriks tersebut dengan penilaian pribadi per kasus.

Matriks bukti minimum adalah:

- pasien dewasa: identitas resmi dan pencocokan data;
- wali: identitas pasien dan wali serta bukti kewenangan;
- keluarga: identitas serta persetujuan pasien atau dasar hukum;
- penjamin: permintaan resmi serta dasar kontrak atau otorisasi;
- aparat atau pengadilan: identitas petugas, surat resmi atau perintah, dasar hukum, tujuan, dan
  ruang data;
- pihak lain: identitas serta kewenangan khusus dengan review pejabat privasi atau hukum.

Untuk pasien anak, pemohon harus menjadi wali sah dan menunjukkan bukti kewenangannya. Untuk
pasien yang tidak mampu memberi persetujuan, pemohon harus menjadi wakil sah atau memiliki dasar
klinis/hukum yang terdokumentasi. Untuk pasien meninggal, pemohon harus merupakan ahli waris atau
wakil yang sah, atau membawa perintah hukum. Seluruh kasus tersebut wajib direview pejabat privasi
atau hukum; hubungan keluarga tidak otomatis memberi hak akses.

Bukti kewenangan khusus yang diterima adalah:

- anak: akta kelahiran atau kartu keluarga dan identitas wali, atau putusan penetapan wali;
- pasien tidak mampu: surat kuasa yang masih sah, penetapan wali/pengampuan, perintah pengadilan,
  atau bukti klinis ketidakmampuan sesuai policy;
- pasien meninggal: surat kematian serta bukti ahli waris/wakil atau perintah pengadilan.

DPJP dapat mendokumentasikan ketidakmampuan klinis sementara dengan alasan dan masa berlaku. Untuk
kondisi berkepanjangan, sengketa, atau kewenangan yang luas, pemohon wajib menunjukkan penetapan
wali/pengampuan atau perintah hukum. Seluruh release tetap melalui review privacy/legal.

**Contoh:** Seorang pasien meminta salinan ringkasan perawatan. Petugas memverifikasi identitas,
mencatat tujuan dan dokumen yang diminta, lalu Unit Rekam Medis menyetujui ruang data yang boleh
dilepas. Jika pemohon adalah pihak lain atau permintaan mencakup kategori sensitif, pejabat
privasi atau hukum ikut meninjau sebelum informasi diserahkan.

**Contoh policy belum tersedia:** Rumah sakit belum mengesahkan bukti yang diterima untuk permintaan
dari anggota keluarga. Sistem menolak pemrosesan pelepasan dan tidak membolehkan petugas memilih
dokumen identitas menurut penilaiannya sendiri.

**Contoh aparat:** Seorang petugas membawa identitas dan surat resmi. Unit Rekam Medis tetap
memeriksa dasar hukum, tujuan, dan ruang data pada surat. Sistem tidak melepas seluruh Rekam Medis
jika surat hanya meminta satu ringkasan tertentu.

**Contoh pasien meninggal:** Anggota keluarga meminta seluruh Rekam Medis pasien yang telah
meninggal. Hubungan keluarga saja tidak cukup. Petugas meminta bukti ahli waris/wakil yang sah atau
perintah hukum, lalu mengirim kasus untuk review privacy/legal sebelum approval release.

**Contoh ketidakmampuan sementara:** DPJP mencatat bahwa pasien tidak mampu memberi persetujuan
selama kondisi akut, beserta alasan dan masa berlaku penilaian. Release yang diajukan wakil tetap
direview privacy/legal. Jika ketidakmampuan berlanjut atau disengketakan, bukti klinis sementara
tidak cukup dan diperlukan penetapan hukum.

**Contoh gagal sebagian:** Approval mengizinkan ringkasan pulang dan hasil penunjang. Ringkasan
berhasil diserahkan, tetapi pengiriman hasil penunjang gagal. Status menjadi `Diserahkan Sebagian`.
Petugas boleh mencoba kembali hanya untuk hasil penunjang, selama approval belum kedaluwarsa. Jika
approval sudah kedaluwarsa, pemohon harus membuat permintaan baru.

**Contoh pencabutan:** Ringkasan pulang telah diterima, tetapi hasil penunjang belum dikirim ketika
pejabat privasi mencabut approval. Sistem menghentikan sisa penyerahan dan mencatat ringkasan yang
sudah diterima sebagai bagian insiden/review; sistem tidak mengklaim file tersebut telah ditarik.

### Pengesahan catatan klinis

Setiap catatan klinis menjadi dokumen resmi ketika pembuatnya menandatangani catatan tersebut.
Setelah ditandatangani, isi catatan asli tidak boleh ditimpa. Perubahan hanya boleh dilakukan
melalui koreksi atau addendum, dan versi lama harus tetap tersimpan dalam riwayat audit.
Setiap tindakan tanda tangan memerlukan autentikasi ulang. Bukti tanda tangan menyimpan identitas
penanda tangan, profesi atau perannya pada waktu tanda tangan, waktu, makna tanda tangan, dan sidik
isi catatan. Sidik isi adalah penanda digital yang berubah jika isi catatan berubah, sehingga sistem
dapat membuktikan isi mana yang benar-benar ditandatangani.

**Contoh:** Dokter menandatangani catatan pada pukul 10.00. Pada pukul 11.00 dokter menyadari
dosis yang tertulis salah. Sistem tidak mengubah diam-diam catatan pukul 10.00. Dokter harus
membuat koreksi atau addendum yang mencantumkan isi perbaikan, waktu perubahan, dan hubungan
dengan catatan asli.

**Contoh bukti tanda tangan:** Dokter melakukan autentikasi ulang pada pukul 10.00 dan memilih
makna "Saya mengesahkan catatan pelayanan ini". Sistem menyimpan identitas dokter, profesi/peran,
waktu, makna tersebut, serta sidik isi. Jika satu karakter pada catatan diubah setelahnya, sidik isi
tidak lagi cocok dan sistem harus menolak perlakuan seolah-olah isi baru telah ditandatangani.

### Kewenangan koreksi

Pembuat catatan mengoreksi catatannya sendiri. Jika pembuat tidak tersedia, pejabat klinis
yang berwenang boleh membuat koreksi terpisah dengan alasan wajib dan identitasnya sendiri.
Pejabat tersebut tidak boleh mengubah atau bertindak seolah-olah menjadi pembuat catatan asli.
Untuk episode aktif, pejabat tersebut adalah Dokter Penanggung Jawab Pelayanan (DPJP) atau
pengganti resminya. Setelah episode ditutup, kewenangan beralih kepada Kepala Pelayanan atau
pejabat yang ditunjuk Komite Medis.

**Contoh:** Dokter pembuat catatan sedang tidak bertugas ketika kesalahan alergi ditemukan.
Pejabat klinis yang berwenang membuat koreksi baru atas namanya sendiri, mencatat alasan
"pembuat tidak tersedia", dan menghubungkan koreksi itu ke catatan yang salah. Nama dokter
pembuat awal dan isi catatan awal tetap terlihat dalam riwayat audit.

### Catatan pada pasien yang salah

Catatan final yang ternyata dicatat pada pasien yang salah tidak boleh dipindahkan atau dihapus.
Sistem menandai catatan asli sebagai `Entered in Error`, mempertahankan seluruh riwayatnya, lalu
membuat catatan baru pada pasien yang benar. Catatan salah dan catatan pengganti harus saling
terhubung agar proses koreksi dapat ditelusuri.

Pada tampilan klinis biasa, catatan `Entered in Error` hanya muncul sebagai baris ringkas dengan
penanda yang jelas. Sistem mengeluarkannya dari ringkasan klinis, perhitungan, dan proses otomatis
yang memakai fakta klinis. Isi asli hanya dapat dibuka melalui tindakan khusus dan setiap pembukaan
wajib masuk audit.

Unit Rekam Medis, pejabat privasi atau hukum, dan pengesah klinis boleh membuka isi asli untuk
pemeriksaan. Tenaga klinis yang sedang merawat pasien hanya boleh membukanya ketika diperlukan
untuk keselamatan pasien, setelah melakukan autentikasi ulang dan mengisi alasan wajib. Hak akses
umum atau hubungan pelayanan saja tidak otomatis membuka isi asli tersebut.

Perubahan catatan final menjadi `Entered in Error` harus diajukan oleh pembuat catatan atau DPJP.
Unit Rekam Medis memverifikasi bahwa catatan memang terkait dengan pasien atau episode yang
dilaporkan. Setelah verifikasi, pejabat klinis berwenang mengesahkan perubahan status. Pengajuan
atau verifikasi saja belum mengubah status catatan.

Pengesah klinis wajib berbeda dari pengaju. Pada episode aktif, DPJP menjadi pengesah. Jika DPJP
sendiri menjadi pengaju, pengesahan beralih kepada pengganti resmi atau Kepala Pelayanan. Setelah
episode ditutup, pengesahan dilakukan oleh Kepala Pelayanan atau pejabat yang ditunjuk Komite
Medis.

**Contoh:** Catatan pemeriksaan milik pasien B keliru ditandatangani dalam episode pasien A.
Catatan pada pasien A diberi status `Entered in Error` dan tetap berada dalam riwayat audit.
Petugas yang berwenang membuat catatan baru pada pasien B. Sistem menyimpan hubungan antara
catatan salah dan penggantinya; sistem tidak sekadar mengganti `PatientId` pada catatan lama.
Pada timeline pasien A, pengguna hanya melihat penanda ringkas bahwa sebuah catatan telah berstatus
`Entered in Error`. Isi salah tersebut tidak ikut muncul dalam ringkasan diagnosis atau perhitungan
klinis. Jika isi lama perlu diperiksa untuk investigasi, pengguna melakukan tindakan buka khusus
dan sistem mencatat identitas, waktu, serta catatan yang dibuka.

### Penutupan episode Rekam Medis

Berakhirnya layanan pasien tidak otomatis menutup Rekam Medis secara final. Jika masih ada
dokumen wajib yang belum ditandatangani, episode Rekam Medis berstatus `Belum Lengkap`.
Episode baru berstatus `Ditutup Final` setelah seluruh dokumen wajib ditandatangani.

Daftar dokumen wajib berbeda menurut jenis layanan dan kondisi pasien. Unit Rekam Medis dan
Komite Medis menetapkannya bersama. Setiap checklist memiliki nomor versi dan tanggal mulai
berlaku agar perubahan aturan dapat ditelusuri. Versi yang aktif saat episode dimulai tetap
berlaku sampai episode ditutup.

Versi aturan tetap dibekukan, tetapi kondisi yang menentukan item wajib dievaluasi ulang selama
episode. Jika suatu kondisi atau pelayanan baru muncul, sistem menambahkan item wajib yang sesuai
dari versi aturan yang sudah dipilih pada awal episode. Sistem tidak berpindah ke versi policy yang
lebih baru untuk episode tersebut.

Item conditional hanya ditambahkan setelah peristiwa nyata yang ditetapkan policy terjadi. Jika
pemicu kemudian dibatalkan atau dinyatakan salah, sistem tidak menghapus kewajiban secara otomatis.
Pengeluaran item memerlukan alasan, review Unit Rekam Medis, dan pengesahan klinis. Riwayat ketika
item ditambahkan, ditinjau, dan dikeluarkan tetap tersimpan.

Batas waktu penyelesaian dapat berbeda menurut layanan dan jenis dokumen. Unit Rekam Medis
dan Komite Medis menetapkannya bersama. Sistem memberikan pengingat dan eskalasi bertahap,
tetapi tidak pernah menutup episode secara otomatis hanya karena batas waktu telah lewat.
Pemicu perhitungan tenggat ditentukan per jenis dokumen berdasarkan peristiwa bisnis yang
relevan, seperti tindakan selesai, hasil diterima, atau pasien pulang.

Untuk setiap tenggat berbasis durasi, sistem mengingatkan pembuat saat 75% waktu yang tersedia
telah terpakai. Ketika tenggat lewat, sistem memberi notifikasi kepada pembuat dan DPJP. Setelah
terlambat 24 jam, sistem mengeskalasi kepada Kepala Pelayanan dan Unit Rekam Medis. Setelah
terlambat 72 jam, sistem mengeskalasi kepada Komite Medis atau Direktur Pelayanan Medis. Untuk
dokumen yang wajib selesai sebelum pasien pulang atau transfer, sistem memberi peringatan ketika
proses pulang atau transfer dimulai. Eskalasi tidak mengubah atau menutup status episode otomatis.

**Contoh eskalasi:** Dokumen conditional memiliki tenggat 24 jam sejak tindakan selesai pukul
Senin 08.00. Pengingat pertama dikirim setelah 18 jam, yaitu Selasa 02.00. Jika belum selesai pada
Selasa 08.00, pembuat dan DPJP menerima notifikasi keterlambatan. Kepala Pelayanan dan Unit Rekam
Medis menerima eskalasi pada Rabu 08.00, lalu Komite Medis atau Direktur Pelayanan Medis pada
Jumat 08.00 jika dokumen tetap belum selesai.

Sistem tidak menyediakan angka SLA bawaan. Sebelum policy resmi memuat nilai, tahap eskalasi,
tanggal berlaku, dan approval, pemeriksaan kelengkapan tetap berjalan tetapi deadline serta reminder
ditandai `Belum Dikonfigurasi` dan tidak boleh menghasilkan kesan bahwa kepatuhan waktu telah dinilai.

**Contoh:** Pasien rawat inap pulang pada pukul 14.00, tetapi ringkasan pulang belum
ditandatangani. Proses kepulangan tetap dapat selesai. Rekam Medis pasien berstatus
`Belum Lengkap`, bukan `Ditutup Final`, sampai ringkasan pulang ditandatangani.

**Contoh checklist:** Rawat jalan umum dapat mewajibkan asesmen dan catatan pelayanan,
sedangkan rawat inap dengan operasi dapat menambahkan laporan operasi dan ringkasan pulang.
Jika checklist versi 2 mulai berlaku pada 1 September 2026, sistem harus menyimpan versi yang
dipakai setiap episode. Episode yang dimulai pada 31 Agustus 2026 tetap memakai versi 1 sampai
ditutup. Episode yang dimulai pada 1 September 2026 memakai versi 2.

**Contoh kondisi baru:** Episode rawat inap dimulai pada 31 Agustus dengan checklist versi 1.
Pada 2 September pasien menjalani operasi. Sistem menambahkan laporan operasi yang diwajibkan oleh
versi 1. Sistem tidak memakai versi 2 walaupun versi tersebut sudah berlaku pada tanggal operasi.

**Contoh pembatalan pemicu:** Operasi sempat dijadwalkan tetapi belum dimulai, sehingga laporan
operasi belum menjadi item wajib. Jika data sistem keliru menyatakan operasi telah selesai dan item
laporan operasi telanjur ditambahkan, petugas tidak dapat menghapusnya diam-diam. Unit Rekam Medis
meninjau bukti pembatalan, pejabat klinis mengesahkan, dan riwayat item tetap dapat dilihat.

#### Checklist minimum rawat jalan umum

Setiap episode rawat jalan umum mewajibkan asesmen awal bertanda tangan, catatan pelayanan atau
SOAP dokter bertanda tangan, dan diagnosis utama. Dokumen berikut menjadi wajib hanya jika peristiwa
terkait terjadi: resep ketika obat diresepkan, catatan tindakan ketika tindakan dilakukan, hasil
penunjang ketika hasil diterima, surat rujukan ketika pasien dirujuk, serta instruksi tindak lanjut
ketika instruksi diberikan.

Asesmen awal harus selesai sebelum pelayanan klinis utama. SOAP dan diagnosis utama harus selesai
paling lambat 24 jam setelah layanan berakhir. Dokumen conditional harus selesai paling lambat
24 jam setelah peristiwa pemicunya.

**Contoh:** Pasien diperiksa tanpa tindakan dan tanpa obat. Checklist berisi asesmen awal, SOAP
bertanda tangan, dan diagnosis utama. Pada pasien lain dokter meresepkan obat dan melakukan tindakan,
maka resep serta catatan tindakan ikut menjadi item wajib sebelum episode dapat `Ditutup Final`.

**Contoh SLA rawat jalan:** Layanan berakhir Senin pukul 10.00. SOAP dan diagnosis utama harus
selesai paling lambat Selasa pukul 10.00. Jika tindakan selesai Senin pukul 09.30, dokumen tindakan
harus selesai paling lambat Selasa pukul 09.30.

#### Checklist minimum IGD

Setiap episode IGD mewajibkan triage, asesmen awal, catatan dokter atau CPPT, diagnosis utama, dan
keputusan akhir/disposisi. Setiap dokumen harus lengkap dan ditandatangani oleh pembuat sesuai
kewenangannya. Catatan resusitasi, observasi, tindakan, obat, hasil penunjang, transfer, rujukan,
atau kematian menjadi wajib hanya jika peristiwa terkait terjadi.

Triage harus selesai sebelum pelayanan non-resusitasi. Pada resusitasi langsung, triage dicatat
paling lambat 30 menit setelah pasien dinyatakan stabil. Asesmen, catatan dokter/CPPT, diagnosis,
dan disposisi harus selesai
sebelum pasien pulang, transfer, atau admission. Dokumen conditional harus selesai paling lambat
24 jam setelah peristiwa pemicunya.

**Contoh:** Pasien diperiksa di IGD lalu dipulangkan tanpa resusitasi atau transfer. Checklist wajib
berisi triage, asesmen awal, catatan dokter/CPPT, diagnosis utama, dan disposisi pulang. Jika pasien
menjalani resusitasi lalu ditransfer, catatan resusitasi dan transfer ikut wajib ditandatangani.

**Contoh SLA IGD:** Pasien akan ditransfer pukul 12.00. Asesmen, catatan dokter/CPPT, diagnosis, dan
disposisi transfer harus selesai sebelum keberangkatan. Jika tindakan selesai pukul 10.00, catatan
tindakan conditional harus selesai paling lambat hari berikutnya pukul 10.00.

**Contoh triage pascaresusitasi:** Pasien resusitasi dinyatakan stabil pukul 09.10. Triage harus
selesai paling lambat pukul 09.40. Waktu 30 menit dihitung dari waktu stabilisasi yang dicatat,
bukan dari waktu pasien datang atau waktu resusitasi dimulai.

#### Checklist minimum rawat inap

Setiap episode rawat inap mewajibkan asesmen awal medis dan keperawatan, CPPT selama perawatan,
diagnosis utama dan akhir, rekonsiliasi obat, ringkasan pulang, serta instruksi tindak lanjut. Semua
dokumen harus lengkap dan ditandatangani sesuai pembuat dan kewenangannya. Dokumen operasi,
anestesi, consent, transfusi, tindakan, konsultasi, hasil penunjang, transfer ruang, atau kematian
menjadi wajib hanya jika peristiwa terkait terjadi.

Asesmen awal medis dan keperawatan harus selesai maksimal 24 jam sejak admission. CPPT wajib dibuat
pada setiap pelayanan penting. Artinya, setiap profesi yang benar-benar memberikan pelayanan wajib
membuat CPPT minimal sekali pada setiap shift kerjanya. CPPT tambahan wajib dibuat setelah perubahan
kondisi atau rencana klinis, tindakan, keputusan konsultasi, transfer, atau insiden. Ringkasan pulang
dan instruksi tindak lanjut harus selesai sebelum pasien pulang.

Diagnosis utama dan rekonsiliasi obat saat masuk harus selesai maksimal 24 jam sejak admission.
Diagnosis akhir dan rekonsiliasi obat pulang harus selesai sebelum pasien pulang. Consent harus
selesai sebelum tindakan yang memerlukannya dimulai. Dokumen operasi, anestesi, transfusi, tindakan,
konsultasi, transfer ruang, atau kematian harus selesai maksimal 24 jam setelah peristiwanya.

**Contoh:** Pasien dirawat tanpa operasi dan kemudian pulang. Checklist tidak memuat laporan operasi
atau anestesi, tetapi tetap mewajibkan asesmen medis/keperawatan, CPPT, diagnosis, rekonsiliasi obat,
ringkasan pulang, dan instruksi tindak lanjut. Jika pasien kemudian menjalani operasi, laporan operasi,
dokumen anestesi, dan consent yang sesuai ikut menjadi item wajib dari versi policy episode tersebut.

**Contoh SLA rawat inap:** Pasien masuk rawat inap Senin pukul 10.00. Asesmen awal medis dan
keperawatan harus selesai paling lambat Selasa pukul 10.00. Ketika pasien akan pulang Rabu pukul
14.00, ringkasan pulang dan instruksi tindak lanjut harus sudah selesai sebelum pasien meninggalkan
rumah sakit. Episode tetap `Belum Lengkap` jika dokumen tersebut belum lengkap dan ditandatangani.

**Contoh CPPT:** Perawat memberi pelayanan pada shift pagi dan membuat sedikitnya satu CPPT pada
shift tersebut. Dokter yang tidak memberi pelayanan pada shift itu tidak diwajibkan membuat CPPT
hanya karena tercantum dalam tim. Jika kondisi pasien memburuk pada pukul 11.00 atau pasien
dipindahkan ke ruang intensif, profesi yang menangani peristiwa tersebut membuat CPPT tambahan.

**Contoh dokumen conditional:** Operasi dimulai setelah consent yang diwajibkan telah selesai.
Operasi berakhir Selasa pukul 15.00 sehingga dokumen operasi dan anestesi harus selesai paling lambat
Rabu pukul 15.00. Jika pasien akan pulang Rabu pukul 10.00, diagnosis akhir dan rekonsiliasi obat
pulang harus selesai sebelum proses pulang diselesaikan.

**Contoh policy belum tersedia:** Checklist menunjukkan ringkasan pulang masih belum ditandatangani,
tetapi kolom deadline menampilkan `Belum Dikonfigurasi`. Sistem tidak menghitung keterlambatan 24 jam
atau angka lain sampai policy yang telah disetujui mulai berlaku.

**Contoh pemicu:** Tenggat laporan operasi dapat dimulai saat tindakan operasi selesai,
sedangkan tenggat ringkasan pulang dimulai saat pasien dinyatakan pulang. Durasi masing-masing
belum ditetapkan dan harus mengikuti kebijakan rumah sakit.

**Contoh hasil penunjang:** Laboratorium merilis hasil versi 1 yang telah ditandatangani. Rekam
Medis menyimpan referensi ke hasil di laboratorium, salinan yang diterima, dan identitas versi 1.
Validasi serta acknowledgment hasil kritis tetap dilakukan pada owner laboratorium, bukan dibuat
ulang sebagai proses terpisah di Rekam Medis.

**Contoh koreksi hasil:** Laboratorium mengganti hasil versi 1 dengan versi 2. Rekam Medis tidak
menghapus versi 1, tetapi menandainya `Digantikan`, menautkannya ke versi 2, dan menampilkan versi 2
sebagai yang berlaku. DPJP/tim aktif menerima notifikasi koreksi dari owner laboratorium.

**Contoh billing:** Rekam Medis sudah lengkap dan menjadi `Ditutup Final`, sementara claim masih
menunggu pemeriksaan penjamin. Rekam Medis tetap final. Sebaliknya, billing sudah siap tetapi masih
ada ringkasan pulang yang belum ditandatangani; status billing tidak boleh memaksa Rekam Medis final.

### Pembatalan episode

Episode yang belum memiliki catatan bertanda tangan dapat diubah menjadi `Dibatalkan`. Setelah
itu, sistem menghentikan pemeriksaan kelengkapan episode tersebut. Jika episode sudah memiliki
setidaknya satu catatan bertanda tangan, pembatalan memerlukan review Unit Rekam Medis dan
pengesahan klinis. Seluruh catatan serta audit tetap tersimpan setelah pembatalan.

Episode yang sudah memiliki catatan bertanda tangan hanya boleh dibatalkan jika terbukti merupakan
registrasi salah atau duplikat dan tidak ada pelayanan nyata yang diberikan melalui episode tersebut.
Jika pelayanan nyata telah terjadi, episode tidak boleh dibatalkan. Catatan yang salah diperbaiki
melalui koreksi dan episode tetap ditutup melalui alur kelengkapan normal.

**Contoh:** Registrasi episode dibuat dua kali dan salah satunya belum mempunyai catatan bertanda
tangan. Episode kosong dapat dibatalkan dan tidak lagi muncul sebagai Rekam Medis belum lengkap.
Jika episode yang hendak dibatalkan sudah memuat catatan dokter bertanda tangan, tombol pembatalan
tidak boleh langsung mengubah status. Unit Rekam Medis harus meninjau dan pejabat klinis harus
mengesahkan; catatan dokter tetap tersimpan apa pun hasil review tersebut.

**Contoh pelayanan nyata:** Episode duplikat memuat catatan bertanda tangan karena dokter benar-benar
memeriksa pasien melalui episode tersebut. Episode tidak boleh dibatalkan hanya untuk merapikan data.
Petugas membuat koreksi yang diperlukan, mempertahankan riwayat pemeriksaan, dan menyelesaikan
kelengkapan episode melalui alur normal.

Jika dua episode duplikat sama-sama memiliki catatan bertanda tangan, Unit Rekam Medis menetapkan
satu episode sebagai episode kanonik. Episode lain diberi status `Duplikat` dan dihubungkan ke episode
kanonik tanpa menghapus episode atau memindahkan catatan. Catatan yang dinilai sah melalui verifikasi
klinis boleh ikut ditampilkan dan diperhitungkan dalam kelengkapan episode kanonik.

DPJP episode kanonik memverifikasi catatan tertaut tersebut. Jika DPJP episode duplikat berbeda,
DPJP episode kanonik tidak tersedia, atau terjadi sengketa klinis, Kepala Pelayanan atau pejabat yang
ditunjuk Komite Medis membuat keputusan verifikasi.

**Contoh episode kanonik:** Episode A dan B ternyata merepresentasikan kunjungan yang sama dan
keduanya memiliki catatan bertanda tangan. Unit Rekam Medis menetapkan episode A sebagai kanonik dan
menandai episode B `Duplikat`. Catatan tetap tersimpan pada episode asalnya. Setelah verifikasi klinis,
catatan sah dari episode B dapat terlihat sebagai catatan tertaut dan memenuhi item checklist episode A;
sistem tidak menghitung episode B sebagai Rekam Medis aktif kedua.

## Status dan Perubahan Status

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| `Draft` | Menandatangani catatan | `Final` | Pembuat catatan | Ketentuan kelengkapan dan kewenangan penanda tangan masih perlu diputuskan. |
| `Final` | Membuat koreksi atau addendum | `Final dengan revisi` | Belum diputuskan | Catatan asli tetap tersimpan dan dapat ditelusuri. |
| `Final` | Membuat koreksi saat pembuat tidak tersedia pada episode aktif | `Final dengan revisi` | DPJP atau pengganti resmi | Alasan wajib, memakai identitas pejabat sendiri, dan catatan asli tetap utuh. |
| `Final` | Membuat koreksi setelah episode ditutup | `Final dengan revisi` | Kepala Pelayanan atau pejabat yang ditunjuk Komite Medis | Alasan wajib, memakai identitas pejabat sendiri, dan catatan asli tetap utuh. |
| `Final` | Pengajuan salah pasien telah diverifikasi dan disahkan | `Entered in Error` | Pejabat klinis berwenang setelah verifikasi Unit Rekam Medis | Pengaju adalah pembuat atau DPJP; keterkaitan pasien diverifikasi; alasan dan seluruh pelaku dicatat. |
| `Aktif` | Layanan pasien berakhir tetapi dokumen wajib belum lengkap | `Belum Lengkap` | Sistem berdasarkan aturan kelengkapan | Setidaknya satu dokumen wajib belum ditandatangani. |
| `Aktif` | Layanan pasien berakhir dan seluruh dokumen wajib lengkap | `Ditutup Final` | Sistem berdasarkan aturan kelengkapan | Seluruh dokumen wajib sudah ditandatangani. |
| `Aktif` | Membatalkan episode tanpa catatan bertanda tangan | `Dibatalkan` | Petugas berwenang pada modul pelayanan | Tidak ada catatan bertanda tangan; alasan dan pelaku dicatat; pemeriksaan kelengkapan berhenti. |
| `Aktif` | Membatalkan episode yang memiliki catatan bertanda tangan | `Dibatalkan` | Sistem setelah review Unit Rekam Medis dan pengesahan klinis | Seluruh catatan/audit dipertahankan; alasan pembatalan telah diterima. |
| `Aktif` | Menetapkan episode sebagai duplikat | `Duplikat` | Unit Rekam Medis | Episode kanonik telah dipilih; kedua episode dihubungkan; catatan tidak dipindahkan; penggunaan catatan tertaut memerlukan verifikasi klinis. |
| `Belum Lengkap` | Dokumen wajib terakhir ditandatangani | `Ditutup Final` | Sistem berdasarkan aturan kelengkapan | Tidak ada lagi dokumen wajib yang belum ditandatangani. |
| `Ditutup Final` | Review data pasca-penutupan membuktikan kewajiban/keselamatan belum selesai | `Belum Lengkap` | Unit Rekam Medis bersama pejabat klinis | Dokumen pasca-penutupan tersimpan; alasan dan hasil review diaudit; tidak ada reopening otomatis. |
| `Diajukan` | Verifikasi dan persetujuan permintaan pelepasan | `Disetujui` | Unit Rekam Medis | Identitas/kewenangan, tujuan, penerima, dan ruang data telah diverifikasi; kasus sensitif atau pengecualian telah ditinjau pejabat privasi/hukum. |
| `Diajukan` | Menolak permintaan pelepasan | `Ditolak` | Unit Rekam Medis atau pejabat privasi/hukum sesuai kasus | Alasan penolakan wajib dicatat. |
| `Disetujui` | Menyiapkan paket informasi | `Disiapkan` | Petugas yang berwenang | Paket tidak melebihi ruang data dan approval masih berlaku. |
| `Disiapkan` | Menyerahkan seluruh informasi | `Diserahkan` | Petugas yang berwenang | Penerima diverifikasi dan seluruh item diserahkan. |
| `Disiapkan` | Menyerahkan sebagian informasi | `Diserahkan Sebagian` | Petugas yang berwenang | Item yang berhasil/gagal dicatat terpisah dan approval masih berlaku. |
| `Disiapkan` atau `Diserahkan Sebagian` | Penyerahan gagal | `Gagal` | Sistem/petugas yang berwenang | Alasan dan item gagal dicatat; retry tetap dibatasi scope serta masa approval. |
| `Disetujui` atau `Disiapkan` | Pemohon membatalkan sebelum penyerahan pertama | `Dibatalkan` | Pemohon | Belum ada data yang diserahkan; alasan dan waktu dicatat. |
| `Disetujui`, `Disiapkan`, `Diserahkan Sebagian`, atau `Gagal` | Membatalkan karena masalah operasional | `Dibatalkan` | Unit Rekam Medis | Item yang sudah/tidak diserahkan dibedakan dan alasan dicatat. |
| `Disetujui`, `Disiapkan`, `Diserahkan Sebagian`, atau `Gagal` | Mencabut approval | `Dicabut` | Pejabat privasi/hukum | Sisa penyerahan berhenti; data yang sudah diterima masuk insiden/review. |
| `Disetujui`, `Disiapkan`, `Diserahkan Sebagian`, atau `Gagal` | Masa approval habis | `Kedaluwarsa` | Sistem | Penyerahan/retry berhenti dan permintaan baru diperlukan. |

## Decision Log

| Decision ID | Tipe | Keputusan/pertanyaan | Owner | Status | Disetujui oleh/pada | Bukti |
|---|---|---|---|---|---|---|
| `RM-SCP-001` | Decision | Scope mencakup inti Rekam Medis untuk rawat jalan, IGD, dan rawat inap. | Unit Rekam Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-EVD-001` | Fact | Belum tersedia capability map untuk modul Rekam Medis; kemungkinan duplikasi dengan kemampuan existing belum diperiksa. | Tim engineering | `superseded` | - | Digantikan `RM-EVD-002` setelah capability audit selesai |
| `RM-EVD-002` | Fact | Capability map Rekam Medis tersedia dan memetakan 30 capability pada backend `5103e68` serta frontend `c4e2ef2a6`. | Tim engineering | `draft` | - | `01-existing-capability-map.md`, SHA-256 `E16740282974D0820742E62C862B1A3F7CEA6BCE3449268667E17586925694C6` |
| `RM-GOV-001` | Decision | Unit Rekam Medis menjadi pemilik proses. Keputusan klinis memerlukan approval Komite Medis/Direktur Pelayanan Medis. | Unit Rekam Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-GOV-002` | Decision | Unit Rekam Medis menyusun aturan privasi dan pelepasan informasi; pejabat privasi/hukum yang ditunjuk manajemen menyetujuinya; akses darurat juga memerlukan approval Komite Medis. | Unit Rekam Medis, pejabat privasi/hukum, dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-PRV-001` | Decision | Akses normal memerlukan peran yang sesuai dan hubungan pelayanan aktif dengan pasien atau penugasan resmi. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-REL-001` | Decision | Hubungan pelayanan aktif hanya berasal dari penugasan formal bertanggal seperti tim perawatan, DPJP, perawat penanggung jawab, konsultan, atau pengganti resmi; akses normal berakhir saat dicabut, diganti, atau episode ditutup. | Unit Rekam Medis, Komite Medis, dan owner penugasan pelayanan | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-REL-002` | Decision | Setelah episode ditutup, mantan tim tidak memiliki akses normal. Akses hanya melalui penugasan follow-up baru atau workflow koreksi/addendum yang memiliki tujuan, batas data, dan masa berlaku tertentu. | Unit Rekam Medis, Komite Medis, dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-PRV-002` | Decision | Akses darurat memerlukan autentikasi ulang dan alasan wajib, dibatasi waktu, diaudit sepenuhnya, serta menghasilkan notifikasi otomatis untuk peninjauan. | Komite Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-PRV-003` | Decision | Unit Rekam Medis dan pejabat privasi meninjau seluruh akses darurat; Kepala Pelayanan dilibatkan bila perlu menilai konteks klinis; dugaan penyalahgunaan dieskalasi sesuai tata kelola rumah sakit. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-PRV-013` | Decision | Reviewer, batas waktu review, jenis hasil review, dan jalur eskalasi dugaan penyalahgunaan harus dikonfigurasi serta disahkan sebelum akses darurat dapat diaktifkan. | Unit Rekam Medis, pejabat privasi/hukum, dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-PRV-015` | Decision | Semua akses darurat direview maksimal satu hari kerja; pembukaan kategori sangat sensitif maksimal empat jam; dugaan penyalahgunaan dieskalasi segera tanpa menunggu tenggat. | Unit Rekam Medis, pejabat privasi/hukum, Komite Medis, dan manajemen | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-PRV-016` | Decision | SLA empat jam kategori sangat sensitif dihitung sebagai empat jam berjalan 24/7; di luar jam kerja sistem mengeskalasi kepada reviewer on-call. | Unit Rekam Medis, pejabat privasi/hukum, dan manajemen | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-PRV-017` | Decision | Hasil review adalah `Sesuai`, `Perlu Klarifikasi`, `Dugaan Penyalahgunaan`, `Penyalahgunaan Terkonfirmasi`, atau `Tidak Terbukti`; seluruhnya memerlukan alasan/evidence dan status terkonfirmasi hanya setelah investigasi privacy/legal serta manajemen. | Unit Rekam Medis, pejabat privasi/hukum, Komite Medis, dan manajemen | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-PRV-010` | Decision | Review dilakukan Unit Rekam Medis dan pejabat privasi; Kepala Pelayanan memberi konteks klinis; SLA satu hari kerja atau empat jam 24/7 untuk data sensitif; dugaan segera dieskalasi dan konfirmasi memerlukan privacy/legal serta manajemen. | Unit Rekam Medis, pejabat privasi/hukum, Komite Medis, dan manajemen | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026; dirinci dalam `RM-PRV-015`–`RM-PRV-017` |
| `RM-PRV-004` | Decision | Akses darurat berakhir setelah sesi singkat yang ditetapkan kebijakan atau lebih cepat ketika penugasan resmi terbentuk; perpanjangan memerlukan autentikasi ulang dan alasan baru. | Komite Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-PRV-006` | Decision | Durasi satu sesi akses darurat adalah 15 menit; perpanjangan memerlukan autentikasi ulang dan alasan baru, sedangkan penugasan resmi mengakhiri sesi lebih cepat. | Komite Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-PRV-011` | Decision | Akses darurat tidak dapat diaktifkan sebelum policy memiliki durasi, tanggal berlaku, dan approval resmi; sistem menampilkan policy belum dikonfigurasi dan tidak memakai nilai default. | Komite Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-PRV-005` | Decision | Selama akses darurat, pengguna boleh melihat informasi klinis yang diperlukan dan membuat catatan baru sesuai kewenangan profesi; tidak boleh mengubah catatan final lama, mengunduh massal, atau melepaskan informasi medis. | Komite Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-PRV-007` | Decision | Akses darurat mula-mula menampilkan data inti keselamatan; riwayat tambahan dibuka per kategori dengan alasan tambahan. | Komite Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-PRV-008` | Decision | Kategori informasi sangat sensitif tersembunyi secara awal; pembukaan memerlukan autentikasi ulang dan alasan khusus, lalu ditinjau secara prioritas tanpa persetujuan sebelumnya. | Pejabat privasi/hukum dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-PRV-009` | Decision | Kategori sangat sensitif mencakup HIV/infeksi menular seksual, kesehatan jiwa, penggunaan zat, kesehatan seksual/reproduksi, data genetik, kekerasan/pelecehan, serta catatan forensik atau perkara hukum. | Pejabat privasi/hukum dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-PRV-014` | Decision | Unit Rekam Medis dan pejabat privasi memelihara klasifikasi terpusat berdasarkan jenis dokumen/kode klinis; Komite Medis menyetujui makna klinis; penandaan atau pelepasan manual memerlukan alasan dan review. | Unit Rekam Medis, pejabat privasi/hukum, dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-PRV-012` | Decision | Akses darurat hanya dapat diaktifkan setelah policy durasi dan daftar kategori sangat sensitif sama-sama disahkan; sistem tidak boleh menebak klasifikasi data. | Pejabat privasi/hukum dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RLS-001` | Decision | Pelepasan informasi dilakukan melalui permintaan formal yang mencatat pemohon, dasar kewenangan, tujuan, penerima, dan ruang data; Unit Rekam Medis memverifikasi dan menyetujui, sedangkan kasus sensitif atau pengecualian ditinjau pejabat privasi/hukum. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-RLS-004` | Decision | Pelepasan informasi dinonaktifkan sampai setiap jenis pemohon memiliki daftar bukti dan pengecualian yang disahkan; petugas tidak boleh memakai penilaian ad hoc sebagai pengganti policy. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RLS-003` | Decision | Release memakai status `Disiapkan`, `Diserahkan`, `Diserahkan Sebagian`, `Gagal`, `Dibatalkan`, `Kedaluwarsa`, dan `Dicabut`; retry hanya dalam scope/masa approval, sedangkan kedaluwarsa/dicabut memerlukan permintaan baru. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RLS-005` | Decision | Pemohon membatalkan sebelum penyerahan pertama; Unit Rekam Medis membatalkan karena masalah operasional; pejabat privasi/hukum mencabut approval. Data yang sudah diterima tidak dapat ditarik dan masuk insiden/review. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RLS-006` | Decision | Matriks bukti minimum dibedakan untuk pasien dewasa, wali, keluarga, penjamin, aparat/pengadilan, dan pihak lain; bukti mencakup identitas, kewenangan/dasar, tujuan, serta ruang data sesuai jenis pemohon. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RLS-007` | Decision | Anak diwakili wali sah; pasien tidak mampu oleh wakil sah atau dasar klinis/hukum terdokumentasi; pasien meninggal oleh ahli waris/wakil sah atau perintah hukum. Semua kasus direview privacy/legal dan keluarga tidak mendapat akses otomatis. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RLS-008` | Decision | Bukti khusus: anak memakai akta lahir/kartu keluarga dan identitas wali atau penetapan wali; tidak mampu memakai kuasa sah, pengampuan, perintah pengadilan, atau bukti klinis sesuai policy; meninggal memakai surat kematian serta bukti ahli waris/wakil atau perintah pengadilan. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RLS-009` | Decision | DPJP menetapkan ketidakmampuan klinis sementara dengan alasan dan masa berlaku; kondisi berkepanjangan, sengketa, atau kewenangan luas memerlukan penetapan wali/pengampuan atau perintah hukum; release tetap direview privacy/legal. | Unit Rekam Medis, Komite Medis, dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RLS-002` | Decision | Matriks bukti release dibedakan menurut jenis pemohon dan kondisi khusus; identitas, dasar kewenangan, tujuan, ruang data, dokumen pendukung, dan review privacy/legal diterapkan sesuai `RM-RLS-006`–`RM-RLS-009`. | Unit Rekam Medis dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026; dirinci dalam `RM-RLS-006`–`RM-RLS-009` |
| `RM-LFC-001` | Decision | Catatan menjadi resmi saat ditandatangani pembuatnya. Setelah itu perubahan hanya melalui koreksi atau addendum; versi lama tetap tersimpan. | Unit Rekam Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-SIG-001` | Decision | Setiap tanda tangan memerlukan autentikasi ulang dan menyimpan identitas, profesi/peran, waktu, makna tanda tangan, serta sidik isi catatan. | Unit Rekam Medis dan Komite Medis/Direktur Pelayanan Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-COR-001` | Decision | Pembuat mengoreksi catatannya sendiri. Jika tidak tersedia, pejabat klinis berwenang membuat koreksi terpisah dengan alasan dan identitasnya sendiri. | Unit Rekam Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-COR-002` | Decision | Pada episode aktif, koreksi pengganti dibuat DPJP atau pengganti resmi. Setelah episode ditutup, koreksi pengganti dibuat Kepala Pelayanan atau pejabat yang ditunjuk Komite Medis. | Komite Medis/Direktur Pelayanan Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-002` | Decision | Catatan final pada pasien yang salah ditandai `Entered in Error` tanpa dipindahkan atau dihapus; riwayat dipertahankan, catatan baru dibuat pada pasien yang benar, dan keduanya dihubungkan. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-003` | Decision | Catatan `Entered in Error` tampil sebagai baris ringkas bertanda jelas, dikeluarkan dari ringkasan klinis/perhitungan, dan isi lama hanya dapat dibuka melalui tindakan khusus yang diaudit. | Unit Rekam Medis, Komite Medis, dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-006` | Decision | Pembuat atau DPJP mengajukan perubahan catatan final menjadi `Entered in Error`; Unit Rekam Medis memverifikasi keterkaitan pasien; pejabat klinis berwenang mengesahkan. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-007` | Decision | Pengesah `Entered in Error` harus berbeda dari pengaju. Episode aktif disahkan DPJP; jika DPJP pengaju, pengesahan beralih ke pengganti resmi/Kepala Pelayanan. Episode tertutup disahkan Kepala Pelayanan atau pejabat Komite Medis. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-008` | Decision | Isi asli catatan `Entered in Error` dapat dibuka untuk pemeriksaan oleh Unit Rekam Medis, pejabat privasi/hukum, dan pengesah klinis. Tenaga klinis yang sedang merawat hanya dapat membukanya untuk keselamatan pasien dengan autentikasi ulang dan alasan wajib. | Unit Rekam Medis, Komite Medis, dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-004` | Decision | Episode tanpa catatan bertanda tangan dapat dibatalkan dan pemeriksaan kelengkapannya berhenti. Episode yang sudah memiliki catatan bertanda tangan hanya dapat dibatalkan setelah review Unit Rekam Medis dan pengesahan klinis; seluruh catatan/audit tetap disimpan. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-009` | Decision | Episode dengan catatan bertanda tangan hanya boleh dibatalkan bila terbukti salah registrasi/duplikat dan tidak ada pelayanan nyata. Bila pelayanan nyata terjadi, episode tidak boleh dibatalkan; catatan dikoreksi dan episode ditutup melalui alur normal. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-005` | Decision | Unit Rekam Medis menetapkan satu episode kanonik; episode duplikat ditandai dan dihubungkan tanpa menghapus/memindahkan catatan. Catatan sah dapat ikut tampilan dan kelengkapan episode kanonik setelah verifikasi klinis. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-LFC-010` | Decision | DPJP episode kanonik memverifikasi catatan episode duplikat. Jika DPJP berbeda/tidak tersedia atau ada sengketa, Kepala Pelayanan atau pejabat Komite Medis memutuskan. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-001` | Decision | Layanan pasien boleh berakhir ketika dokumen wajib belum lengkap. Rekam Medis berstatus `Belum Lengkap` dan baru `Ditutup Final` setelah seluruh dokumen wajib ditandatangani. | Unit Rekam Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-002` | Decision | Checklist dokumen wajib berbeda menurut layanan dan kondisi pasien, ditetapkan bersama oleh Unit Rekam Medis dan Komite Medis, serta memiliki versi dan tanggal berlaku. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-003` | Decision | Versi checklist yang aktif saat episode dimulai tetap berlaku sampai episode ditutup. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-004` | Decision | Batas waktu dapat berbeda menurut layanan dan jenis dokumen, ditetapkan Unit Rekam Medis bersama Komite Medis, dengan pengingat dan eskalasi bertahap tanpa penutupan otomatis. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-005` | Decision | Pemicu tenggat ditentukan per jenis dokumen berdasarkan peristiwa bisnisnya, misalnya tindakan selesai, hasil diterima, atau pasien pulang. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-008` | Decision | Versi checklist tetap versi saat episode dimulai, tetapi kondisi applicability dievaluasi ulang dan item wajib baru ditambahkan dari versi tersebut ketika kondisi/pelayanan baru muncul. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-009` | Decision | Item conditional ditambahkan setelah peristiwa nyata. Jika pemicu dibatalkan/dinyatakan salah, pengeluaran item memerlukan alasan, review Unit Rekam Medis, pengesahan klinis, dan riwayat item tetap disimpan. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-010` | Decision | Checklist rawat jalan umum mewajibkan asesmen awal bertanda tangan, SOAP dokter bertanda tangan, dan diagnosis utama; resep, tindakan, hasil penunjang, rujukan, dan instruksi tindak lanjut wajib bila peristiwa terkait terjadi. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-011` | Decision | Checklist IGD mewajibkan triage, asesmen awal, catatan dokter/CPPT, diagnosis utama, dan keputusan akhir/disposisi yang lengkap serta ditandatangani; resusitasi, observasi, tindakan, obat, hasil penunjang, transfer, rujukan, dan kematian wajib bila peristiwa terkait terjadi. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-012` | Decision | Checklist rawat inap mewajibkan asesmen awal medis/keperawatan, CPPT, diagnosis utama/akhir, rekonsiliasi obat, ringkasan pulang, dan instruksi tindak lanjut; operasi, anestesi, consent, transfusi, tindakan, konsultasi, hasil penunjang, transfer ruang, dan kematian wajib bila terjadi. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-CLS-013` | Decision | Tidak ada SLA default. Kelengkapan tetap diperiksa, tetapi deadline/reminder berstatus `Belum Dikonfigurasi` dan baru aktif setelah policy berisi nilai, tahap eskalasi, tanggal berlaku, serta approval. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-CLS-014` | Decision | SLA rawat jalan: asesmen awal sebelum pelayanan klinis utama; SOAP dan diagnosis utama maksimal 24 jam setelah layanan berakhir; dokumen conditional maksimal 24 jam setelah peristiwa pemicunya. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-CLS-015` | Decision | SLA IGD: triage sebelum layanan non-resusitasi atau segera setelah stabilisasi; asesmen, catatan dokter/CPPT, diagnosis, dan disposisi sebelum pulang/transfer/admission; dokumen conditional maksimal 24 jam setelah peristiwa. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-CLS-016` | Decision | Pada resusitasi langsung di IGD, triage harus selesai maksimal 30 menit setelah pasien dinyatakan stabil. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-CLS-017` | Decision | SLA rawat inap: asesmen awal medis dan keperawatan maksimal 24 jam sejak admission; CPPT dibuat pada setiap pelayanan penting; ringkasan pulang dan instruksi tindak lanjut selesai sebelum pasien pulang. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-CLS-018` | Decision | Pelayanan penting untuk kewajiban CPPT berarti minimal satu CPPT per shift oleh setiap profesi yang benar-benar memberi pelayanan, serta CPPT tambahan setelah perubahan kondisi/rencana klinis, tindakan, keputusan konsultasi, transfer, atau insiden. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-CLS-019` | Decision | Eskalasi seluruh layanan: pengingat saat 75% waktu tersedia terpakai; notifikasi pembuat dan DPJP ketika tenggat lewat; eskalasi kepada Kepala Pelayanan dan Unit Rekam Medis setelah terlambat 24 jam; eskalasi kepada Komite Medis/Direktur Pelayanan Medis setelah terlambat 72 jam. Dokumen yang wajib selesai sebelum pulang/transfer diperingatkan ketika proses tersebut dimulai. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-CLS-020` | Decision | Sisa SLA rawat inap: diagnosis utama dan rekonsiliasi obat masuk maksimal 24 jam sejak admission; diagnosis akhir dan rekonsiliasi obat pulang sebelum pasien pulang; consent sebelum tindakan; dokumen operasi, anestesi, transfusi, tindakan, konsultasi, transfer ruang, atau kematian maksimal 24 jam setelah peristiwa. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-CLS-006` | Decision | Nilai SLA, pemicu, pengingat, dan tahap eskalasi telah ditetapkan untuk rawat jalan, IGD, serta rawat inap melalui `RM-CLS-014` sampai `RM-CLS-020`; tidak ada penutupan episode otomatis akibat keterlambatan. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna tanggal 20–21 Agustus 2026; rincian `RM-CLS-014`–`RM-CLS-020` |
| `RM-INT-001` | Decision | Laboratorium/radiologi tetap menjadi owner order, hasil, validasi, hasil kritis, dan acknowledgment. Rekam Medis menerima referensi serta salinan hasil yang sudah dirilis/ditandatangani dan menyimpan bukti versinya. | Owner laboratorium/radiologi, Unit Rekam Medis, dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-INT-002` | Decision | Rekam Medis menyimpan semua versi hasil; versi lama ditandai `Digantikan`/`Ditarik` dan ditautkan ke versi baru tanpa ditimpa. Owner penunjang mengirim event koreksi serta notifikasi kepada DPJP/tim aktif. | Owner laboratorium/radiologi, Unit Rekam Medis, dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-BIL-001` | Decision | Rekam Medis menerbitkan status kelengkapan, sedangkan coding/claim/billing menentukan readiness sendiri; proses finansial tidak boleh menahan signature atau closure Rekam Medis. | Unit Rekam Medis dan owner Billing/Casemix/Keuangan | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-EXC-002` | Decision | Setiap tindakan memiliki kunci permintaan unik; retry dengan isi sama mengembalikan hasil pertama, sedangkan kunci sama dengan isi berbeda ditolak sebagai konflik. | Unit Rekam Medis dan engineering | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-EXC-003` | Decision | Downtime memakai formulir resmi bernomor unik; setelah pulih dicatat sebagai `Entri Downtime` dengan waktu kejadian/input terpisah, autentikasi ulang dan signature pembuat, serta rekonsiliasi anti-duplikat. | Unit Rekam Medis, Komite Medis, dan engineering | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-EXC-004` | Decision | Catatan signed tetap sah saat integrasi gagal; status menjadi `Sinkronisasi Tertunda`, event disimpan tahan gagal, retry otomatis dan rekonsiliasi manual tersedia. Final closure tertahan hanya bila bukti item wajib belum tersedia. | Unit Rekam Medis dan engineering | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-EXC-005` | Decision | Data pasca-penutupan disimpan dan direview tanpa reopening otomatis. Unit Rekam Medis bersama pejabat klinis dapat membuka kembali ke `Belum Lengkap` jika kewajiban/keselamatan terdampak, dengan alasan dan audit. | Unit Rekam Medis dan Komite Medis | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-EXC-001` | Decision | Duplicate submit, downtime, late data, dan partial failure ditangani melalui idempotency, entri downtime bernomor, review pasca-penutupan, event tahan gagal, retry, dan rekonsiliasi tanpa menghapus catatan signed. | Unit Rekam Medis, Komite Medis, dan engineering | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026; dirinci dalam `RM-EXC-002`–`RM-EXC-005` |
| `RM-RPT-001` | Decision | Wajib tersedia worklist kelengkapan per layanan/dokumen/penanggung jawab/umur; laporan signature, correction, `Entered in Error`, break-glass, release, downtime, dan sinkronisasi; akses/ekspor dibatasi per peran dan diaudit. | Unit Rekam Medis, Komite Medis, dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RPT-003` | Decision | Tidak ada penghapusan otomatis sebelum policy retensi per jenis data disahkan; legal hold menghentikan penghapusan sampai dicabut pejabat berwenang. | Unit Rekam Medis, pejabat privasi/hukum, dan manajemen | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RPT-004` | Decision | Worklist tersedia real-time; ringkasan harian pukul 07.00 WIB untuk Unit Rekam Medis dan Kepala Pelayanan; laporan mingguan Senin pukul 08.00 WIB untuk Komite Medis; laporan bulanan paling lambat hari kerja kelima untuk manajemen dan pejabat privasi/hukum. Break-glass dan dugaan penyalahgunaan tetap dikirim berbasis kejadian sesuai SLA review. | Unit Rekam Medis, Komite Medis, pejabat privasi/hukum, dan manajemen | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RPT-005` | Decision | Baseline retensi: Rekam Medis beserta seluruh versi/signature/koreksi/`Entered in Error` minimal 25 tahun sejak interaksi terakhir; audit akses/break-glass/release/ekspor 10 tahun sejak kejadian; bukti downtime/integrasi 10 tahun sejak rekonsiliasi selesai; snapshot laporan terjadwal 5 tahun; worklist real-time tidak disimpan terpisah. Legal hold dan aturan yang lebih panjang selalu mengalahkan batas tersebut. | Unit Rekam Medis, pejabat privasi/hukum, dan manajemen | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-RPT-002` | Decision | Masa simpan dan distribusi worklist/laporan/audit telah ditetapkan melalui `RM-RPT-004` dan `RM-RPT-005`; legal hold tetap menghentikan penghapusan melalui `RM-RPT-003`. | Unit Rekam Medis, pejabat privasi/hukum, dan manajemen | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026; rincian `RM-RPT-003`–`RM-RPT-005` |
| `RM-APR-001` | Decision | Semua jawaban tetap `draft`; approval formal diberikan terpisah oleh individu bernama sesuai kewenangan operasional, klinis, dan privacy/release, dengan identitas, waktu, versi/hash, serta artefak approval. | Unit Rekam Medis, Komite Medis/Direktur Pelayanan Medis, dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-APR-003` | Decision | Artefak approval menggunakan memo persetujuan bertanda tangan yang mencantumkan identitas approver, waktu persetujuan, tanggal berlaku, versi dan hash decision log, serta ditautkan ke revision blueprint terkait. | Unit Rekam Medis, Komite Medis/Direktur Pelayanan Medis, dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-APR-004` | Decision | Blueprint tetap `draft` sampai nama approver operasional, klinis, dan privacy/release serta memo approval yang benar-benar ditandatangani tersedia. Jawaban wawancara tidak diperlakukan sebagai approval formal. | Unit Rekam Medis, Komite Medis/Direktur Pelayanan Medis, dan pejabat privasi/hukum | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 21 Agustus 2026 |
| `RM-APR-005` | Fact | Individu approver operasional, klinis, dan privacy/release belum ditunjuk. Dampaknya terhadap fase development dirinci oleh keputusan `RM-APR-006`. | Manajemen rumah sakit | `draft` | Belum disetujui secara formal | Konfirmasi pengguna pada 21 Agustus 2026 |
| `RM-APR-006` | Decision | Approval formal ditunda sampai sebelum aktivasi kontrak, fitur berisiko, deployment produksi, atau sign-off kesiapan. Selama development, requirement analysis, domain architecture, desain draft, dan backlog draft boleh dilanjutkan; seluruh artefak tetap `draft`, tidak boleh disebut approved, dan fitur yang mensyaratkan approval tetap fail-closed. | Product/domain owner dan manajemen rumah sakit | `draft` | Belum disetujui secara formal | Arahan pengguna pada 21 Agustus 2026 |
| `RM-UI-001` | Decision | Invariant klinis dan privasi wajib disetujui owner; alur utama mengikuti product/UI brief; detail visual menjadi `DEV_DISCRETION` selama mengikuti pola project dan tidak mengubah makna. | Unit Rekam Medis/Product Owner | `draft` | Belum disetujui secara formal | Jawaban pengguna pada 20 Agustus 2026 |

## Acceptance Criteria

- Ketika pembuat menandatangani catatan `Draft`, sistem mengubahnya menjadi `Final` dan
  mencegah isi catatan asli ditimpa.
- Sistem menolak tanda tangan tanpa autentikasi ulang. Bukti tanda tangan menyimpan identitas,
  profesi/peran, waktu, makna tanda tangan, dan sidik isi yang cocok dengan isi final.
- Ketika catatan `Final` diperbaiki, sistem menyimpan koreksi atau addendum sebagai catatan
  yang terhubung dan mempertahankan isi, pembuat, serta waktu versi sebelumnya.
- Ketika catatan final diketahui berada pada pasien yang salah, sistem menolak pemindahan atau
  penghapusan catatan lama, menandainya `Entered in Error`, dan mewajibkan catatan pengganti pada
  pasien yang benar terhubung dengan catatan tersebut.
- Sistem tidak memakai catatan `Entered in Error` untuk ringkasan klinis, perhitungan, atau proses
  otomatis. Pembukaan isi lamanya memerlukan tindakan khusus dan menghasilkan audit.
- Sistem menolak perubahan langsung catatan final menjadi `Entered in Error` tanpa pengajuan oleh
  pembuat/DPJP, verifikasi Unit Rekam Medis, dan pengesahan pejabat klinis berwenang.
- Sistem menolak pengesahan `Entered in Error` oleh orang yang sama dengan pengaju dan menerapkan
  pengesah berbeda sesuai status aktif atau tertutupnya episode.
- Sistem membatasi pembukaan isi asli `Entered in Error` kepada peran pemeriksa yang ditetapkan.
  Tenaga klinis aktif ditolak jika tidak melakukan autentikasi ulang atau tidak mengisi alasan
  keselamatan pasien; seluruh pembukaan dicatat dalam audit.
- Sistem mengizinkan pembatalan langsung hanya bila episode belum mempunyai catatan bertanda
  tangan. Jika sudah ada, sistem mewajibkan review Unit Rekam Medis dan pengesahan klinis serta
  mempertahankan semua catatan dan audit.
- Sistem menolak pembatalan episode yang memiliki bukti pelayanan nyata, walaupun episode tersebut
  terindikasi duplikat. Episode tetap melalui koreksi dan penutupan normal.
- Ketika dua episode berisi catatan bertanda tangan dinyatakan duplikat, sistem mempertahankan catatan
  pada episode asal, menautkan episode duplikat ke episode kanonik, dan hanya memperhitungkan catatan
  tertaut yang telah diverifikasi klinis.
- Sistem hanya menerima verifikasi klinis catatan tertaut dari DPJP episode kanonik. Ketika syarat
  eskalasi terpenuhi, sistem mengalihkan keputusan kepada Kepala Pelayanan/pejabat Komite Medis dan
  mencatat alasan eskalasi.
- Ketika pembuat tidak tersedia, sistem hanya mengizinkan pejabat klinis yang berwenang
  membuat koreksi terpisah setelah mengisi alasan; audit mencatat identitas pejabat tersebut
  dan tidak mengubah identitas pembuat awal.
- Pada episode aktif, sistem membatasi koreksi pengganti kepada DPJP atau pengganti resmi.
  Setelah episode ditutup, sistem membatasinya kepada Kepala Pelayanan atau pejabat yang
  ditunjuk Komite Medis.
- Ketika layanan berakhir dan ada dokumen wajib yang belum ditandatangani, sistem menetapkan
  episode Rekam Medis sebagai `Belum Lengkap`, bukan `Ditutup Final`.
- Ketika dokumen wajib terakhir ditandatangani, sistem mengubah episode `Belum Lengkap`
  menjadi `Ditutup Final`.
- Sistem menghitung kelengkapan memakai checklist yang sesuai dengan jenis layanan dan kondisi
  pasien, serta menyimpan nomor versi checklist pada episode agar hasilnya dapat diaudit.
- Perubahan checklist tidak mengubah kewajiban episode yang sudah berjalan. Episode baru
  memakai versi yang aktif pada waktu episode tersebut dimulai.
- Ketika kondisi/pelayanan baru muncul selama episode, sistem mengevaluasi ulang applicability dan
  menambahkan item wajib dari versi checklist yang sudah terikat pada episode, tanpa mengganti versi.
- Sistem tidak menghapus item conditional secara otomatis ketika pemicunya dibatalkan atau dikoreksi.
  Pengeluaran item memerlukan alasan, review Unit Rekam Medis, pengesahan klinis, dan audit lengkap.
- Untuk rawat jalan umum, sistem selalu mewajibkan asesmen awal bertanda tangan, SOAP dokter
  bertanda tangan, dan diagnosis utama, serta menambah item conditional sesuai peristiwa pelayanan.
- Untuk IGD, sistem selalu mewajibkan triage, asesmen awal, catatan dokter/CPPT, diagnosis utama,
  dan keputusan akhir/disposisi, serta menambah dokumen conditional sesuai peristiwa kegawatan.
- Untuk rawat inap, sistem selalu mewajibkan asesmen awal medis/keperawatan, CPPT, diagnosis,
  rekonsiliasi obat, ringkasan pulang, dan instruksi tindak lanjut, serta menambah dokumen conditional
  sesuai pelayanan yang terjadi.
- Rekam Medis hanya menerima hasil laboratorium/radiologi yang sudah dirilis/ditandatangani, lengkap
  dengan referensi dan versi; ownership validasi, hasil kritis, serta acknowledgment tidak berpindah.
- Ketika hasil dikoreksi/ditarik, sistem mempertahankan versi lama, menandai statusnya, menautkan
  versi baru, dan menerima event/notifikasi owner penunjang untuk DPJP/tim aktif.
- Sistem menerbitkan status kelengkapan untuk modul finansial tetapi tidak menerima financial status
  sebagai syarat signature atau `Ditutup Final`.
- Sistem tidak membuat signature/catatan ganda untuk retry dengan kunci dan isi yang sama, serta
  menolak kunci yang sama bila isi request berbeda.
- Sistem menerima catatan downtime hanya dengan nomor formulir unik, menyimpan waktu kejadian dan
  input terpisah, mewajibkan signature pembuat, serta menolak rekonsiliasi formulir yang sama dua kali.
- Sistem tidak menghapus/membatalkan catatan signed karena kegagalan integrasi, melainkan menyimpan
  event tahan gagal, menandai sinkronisasi tertunda, melakukan retry, dan menyediakan rekonsiliasi.
- Sistem menyimpan data pasca-penutupan dan membuat review tanpa reopening otomatis. Reopening hanya
  dilakukan Unit Rekam Medis bersama pejabat klinis dengan alasan serta audit.
- Sistem menyediakan worklist/laporan minimum yang diputuskan, membatasi akses dan ekspor sesuai
  peran, serta mengaudit setiap ekspor berikut filter dan ruang datanya.
- Sistem memperbarui worklist secara real-time dan mendistribusikan ringkasan harian, mingguan, dan
  bulanan kepada penerima pada jadwal yang ditetapkan, tanpa menunda notifikasi insiden berbasis
  kejadian.
- Sistem tidak menghapus laporan/audit tanpa policy retensi yang disahkan dan selalu menghentikan
  penghapusan ketika legal hold aktif.
- Setelah policy disahkan, sistem menerapkan retensi minimum 25 tahun untuk Rekam Medis dan
  riwayatnya, 10 tahun untuk audit akses/release/ekspor serta bukti downtime/integrasi, dan 5 tahun
  untuk snapshot laporan; batas yang lebih panjang dan legal hold selalu menang.
- Sistem menjalankan pengingat dan eskalasi sesuai aturan layanan serta jenis dokumen. Ketika
  tenggat terlewati, episode tidak berubah menjadi `Ditutup Final` secara otomatis.
- Untuk tenggat berbasis durasi, sistem mengirim pengingat pada 75% waktu, notifikasi pembuat dan
  DPJP saat terlambat, eskalasi ke Kepala Pelayanan dan Unit Rekam Medis setelah 24 jam terlambat,
  serta eskalasi ke Komite Medis/Direktur Pelayanan Medis setelah 72 jam terlambat.
- Untuk dokumen yang wajib selesai sebelum pulang atau transfer, sistem memberi peringatan ketika
  proses pulang atau transfer dimulai apabila dokumen belum lengkap dan ditandatangani.
- Jika policy SLA belum disetujui, sistem tetap menghitung kelengkapan tetapi menandai deadline dan
  reminder `Belum Dikonfigurasi`; sistem tidak memakai angka contoh atau angka buatan developer.
- Untuk rawat jalan, sistem menghitung deadline asesmen sebelum pelayanan utama, SOAP/diagnosis
  24 jam dari akhir layanan, dan dokumen conditional 24 jam dari peristiwa pemicunya.
- Untuk IGD, sistem mewajibkan dokumen inti sebelum pasien meninggalkan IGD dan menghitung dokumen
  conditional 24 jam dari peristiwa; resusitasi langsung menjadi pengecualian urutan triage,
  dengan batas pencatatan triage maksimal 30 menit setelah pasien dinyatakan stabil.
- Untuk rawat inap, sistem menghitung batas asesmen awal medis dan keperawatan 24 jam sejak
  admission, mewajibkan sedikitnya satu CPPT per shift bagi setiap profesi yang memberi pelayanan,
  serta meminta CPPT tambahan setelah peristiwa klinis penting. Sistem menahan status `Ditutup Final`
  sampai ringkasan pulang dan instruksi tindak lanjut yang harus selesai sebelum pasien pulang telah
  lengkap dan ditandatangani.
- Untuk rawat inap, sistem juga menghitung diagnosis utama dan rekonsiliasi obat masuk 24 jam sejak
  admission, mewajibkan diagnosis akhir serta rekonsiliasi obat pulang sebelum pasien pulang,
  mewajibkan consent sebelum tindakan, dan menghitung dokumen conditional terkait maksimal 24 jam
  setelah peristiwa.
- Sistem memulai hitungan tenggat setiap dokumen hanya ketika peristiwa bisnis yang ditetapkan
  untuk jenis dokumen tersebut terjadi.
- Sistem mengizinkan akses normal hanya jika pengguna memiliki peran yang sesuai sekaligus
  hubungan pelayanan aktif atau penugasan resmi; penolakan akses dicatat untuk audit sesuai
  kebijakan yang akan ditetapkan.
- Sistem menentukan hubungan pelayanan dari penugasan formal beserta waktu berlakunya. Sistem
  menolak akses normal sebelum penugasan mulai serta setelah dicabut, diganti, atau episode ditutup.
- Setelah episode ditutup, sistem menolak akses berdasarkan keanggotaan tim lama. Penugasan follow-up
  atau workflow koreksi/addendum harus membatasi tujuan, ruang data, dan masa berlaku akses.
- Sistem menolak aktivasi akses darurat tanpa autentikasi ulang dan alasan. Setelah aktif,
  sistem membatasi waktunya, mencatat seluruh aktivitas, dan mengirim notifikasi peninjauan.
- Setiap akses darurat masuk antrean peninjauan Unit Rekam Medis dan pejabat privasi. Sistem
  mendukung pelibatan Kepala Pelayanan dan pencatatan hasil eskalasi dugaan penyalahgunaan.
- Sistem menolak aktivasi akses darurat bila reviewer, tenggat review, jenis hasil, atau jalur
  eskalasi belum dikonfigurasi dan disahkan.
- Sistem menandai overdue jika review biasa melewati satu hari kerja atau review kategori sangat
  sensitif melewati empat jam, serta mengeskalasi dugaan penyalahgunaan segera.
- Sistem menghitung SLA kategori sensitif selama 24/7 dan mengirim eskalasi kepada reviewer on-call
  ketika review berlangsung di luar jam kerja.
- Sistem hanya menerima hasil review baku dengan alasan/evidence dan membatasi penetapan
  `Penyalahgunaan Terkonfirmasi` kepada hasil investigasi privacy/legal serta manajemen.
- Sistem mengakhiri akses darurat saat durasi kebijakan habis atau penugasan resmi terbentuk,
  mana yang terjadi lebih dahulu. Perpanjangan ditolak tanpa autentikasi ulang dan alasan baru.
- Sistem mengakhiri sesi akses darurat paling lambat 15 menit setelah aktivasi dan tidak
  memperpanjangnya tanpa autentikasi ulang serta alasan baru.
- Sistem menolak aktivasi akses darurat bila durasi, tanggal berlaku, atau approval policy belum
  tersedia dan tidak menggantinya dengan nilai default.
- Selama akses darurat, sistem menerapkan kewenangan profesi untuk pembuatan catatan baru dan
  menolak perubahan catatan final lama, unduhan massal, serta pelepasan informasi medis.
- Saat akses darurat dimulai, sistem menampilkan data inti keselamatan. Pembukaan kategori
  riwayat tambahan memerlukan alasan tambahan dan dicatat dalam audit.
- Sistem menyembunyikan kategori sangat sensitif secara awal. Dalam akses darurat, sistem
  hanya membukanya setelah autentikasi ulang dan alasan khusus, lalu membuat item peninjauan
  prioritas tanpa menunggu persetujuan sebelumnya.
- Sistem menerapkan kategori sangat sensitif yang telah diputuskan dan tidak menyertakannya dalam
  data inti akses darurat.
- Sistem mengklasifikasikan data sensitif dari aturan terpusat dan menolak perubahan manual tanpa
  alasan serta review.
- Sistem menolak aktivasi akses darurat sampai policy durasi dan daftar kategori sangat sensitif
  sama-sama disahkan; tidak ada klasifikasi default buatan sistem/developer.
- Sistem menolak pelepasan informasi tanpa permintaan formal dan verifikasi. Informasi yang
  diserahkan tidak boleh melebihi ruang data yang disetujui, dan seluruh tahap dicatat dalam audit.
- Sistem menolak seluruh pelepasan jika matriks bukti dan pengecualian untuk jenis pemohon terkait
  belum disahkan; tidak ada override ad hoc oleh petugas.
- Sistem membedakan hasil penyerahan penuh, sebagian, gagal, dibatalkan, kedaluwarsa, dan dicabut.
  Retry ditolak jika melebihi scope atau masa approval; kedaluwarsa/dicabut memerlukan request baru.
- Sistem membatasi pembatalan/pencabutan sesuai tahap dan pelakunya. Jika sebagian data sudah diterima,
  sistem menghentikan sisa penyerahan dan membuat catatan insiden/review tanpa mengklaim penarikan.
- Sistem memvalidasi bukti minimum sesuai jenis pemohon dan menolak permintaan yang tidak memiliki
  identitas, kewenangan/dasar, tujuan, atau ruang data yang diwajibkan matriks.
- Sistem tidak menerima hubungan keluarga sebagai kewenangan otomatis untuk anak, pasien tidak
  mampu, atau pasien meninggal; bukti kewenangan dan review privacy/legal wajib tersedia.
- Sistem memvalidasi dokumen kewenangan khusus sesuai kondisi anak, ketidakmampuan, atau kematian
  dan menolak bukti di luar matriks tanpa review pengecualian.
- Sistem menerima ketidakmampuan klinis sementara hanya dari DPJP dengan alasan dan masa berlaku,
  serta mewajibkan bukti hukum untuk kondisi berkepanjangan, sengketa, atau kewenangan luas.

## Kewenangan Keputusan Frontend

Urutan kewenangan adalah: keamanan, privasi, dan invariant yang disetujui; product/UI brief
yang disetujui; pola project; lalu kebebasan developer.

| Decision ID | Area | Owner | Status | Rentang yang diperbolehkan | Bukti |
|---|---|---|---|---|---|
| `RM-UI-001` | Aturan klinis, status, permission, privasi, peringatan keselamatan, dan field wajib | Unit Rekam Medis serta approver terkait | `draft` | Wajib mengikuti keputusan yang disetujui; bukan kewenangan developer. | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-UI-001-A` | Alur utama | Product Owner/Unit Rekam Medis | `draft` | Wajib mengikuti product/UI brief yang telah disetujui. | Jawaban pengguna pada 20 Agustus 2026 |
| `RM-UI-001-B` | Detail visual | Developer | `DEV_DISCRETION` | Boleh mengikuti pola project selama tidak mengubah makna, urutan kewenangan, privasi, atau invariant. | Jawaban pengguna pada 20 Agustus 2026 |

## Open Questions dan Blocker

| Decision ID | Pertanyaan terbuka | Owner yang dibutuhkan | Dampak |
|---|---|---|---|
| `RM-APR-002` | Siapa individu yang akan ditunjuk untuk approval operasional, klinis, dan privacy/release, serta kapan memo approval ditandatangani dan mulai berlaku? | Manajemen rumah sakit, Unit Rekam Medis, Komite Medis/Direktur Pelayanan Medis, dan pejabat privasi/hukum | Menurut `RM-APR-006`, gap ini tidak lagi memblokir analisis/desain draft, tetapi tetap memblokir status `approved`, aktivasi policy/fitur berisiko, deployment produksi, dan sign-off kesiapan. |
