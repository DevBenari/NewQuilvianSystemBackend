# Accounting — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `ACC-BP-001` |
| Revision | `3` |
| Status | `approved` untuk scope MVP |
| Pass | `Scope pass` — **selesai** 1 September 2026 |
| Product/domain owner | Rizki |
| Backend SHA | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) |
| Frontend SHA | `fc49cc7714baa9a2c37ed6519fbaba5dffcbda99` (branch `RizkiV2`) — baseline **saat dokumen ini disusun**. Baseline blueprint kini `31a82c8` (`QuilvianIntegrationFrontend`); kutipan di bawah tetap berlaku, lihat `evidence/02-frontend-rebaseline-impact-scan.md` |
| Masukan | `ACC-PRD-001` revisi `0.1` |
| Tanggal mulai | 1 September 2026 |

## Catatan cara kerja

Wawancara ini menjawab pertanyaan **"aturan bisnisnya bagaimana"**, bukan "apa yang sudah ada
di sistem".

Scope dikunci **tanpa audit kemampuan existing yang penuh**. Yang sudah dilakukan hanya
pemeriksaan terarah, hasilnya ada di [01-existing-capability-map.md](01-existing-capability-map.md).
Kesimpulannya modul Accounting dimulai dari nol, sehingga risiko membangun sesuatu yang sudah
ada tergolong rendah. Meski begitu, audit penuh lewat `/trace-existing-capabilities` tetap
disarankan sebelum arsitektur dikunci.

Rekomendasi pada setiap pertanyaan di bawah **bukan keputusan dan bukan persetujuan**. Owner
yang berwenang tetap harus memilih. Setiap pertanyaan selalu punya opsi tambahan
`Other — tuliskan pilihan atau batasan lain`, walaupun tidak ditulis ulang satu per satu.

---

## Scope dan Outcome

**Modul:** Accounting

**Satu kalimat batas scope:** Accounting mencatat, mengesahkan, dan melaporkan akibat
keuangan dari kejadian yang diterbitkan modul lain, tanpa pernah mengambil alih transaksi
operasional milik modul tersebut.

### Di dalam scope

| ID | Kemampuan | Keterangan |
|---|---|---|
| `ACC-SC-001` | Setup Accounting | Pengaturan dasar, jenis journal, pemetaan posting, dimensi akuntansi, konfigurasi saldo awal |
| `ACC-SC-002` | Chart of Accounts | Daftar akun, pengelompokan, hirarki, konfigurasi akun |
| `ACC-SC-003` | Journal Management | Journal manual, otomatis, impor, berulang, persetujuan, pengesahan, pembalikan |
| `ACC-SC-004` | Accounting Integration | Kotak masuk kejadian keuangan, pemrosesan, penanganan gagal, pengulangan, rekonsiliasi |
| `ACC-SC-005` | General Ledger | Buku besar, mutasi per akun, saldo awal dan akhir |
| `ACC-SC-006` | Accounting Period | Periode akuntansi, penguncian, penutupan, pembukaan kembali |
| `ACC-SC-007` | Financial Closing | Daftar periksa penutupan, tutup bulan, tutup tahun, pengecualian |
| `ACC-SC-008` | Financial Reporting | Neraca saldo, laporan buku besar, laba rugi, neraca |
| `ACC-SC-009` | Accounting Audit | Jejak audit atas seluruh tindakan akuntansi |

### Di luar scope — untuk modul lain

| ID | Kemampuan | Pemilik | Titik sentuh yang tetap dibahas |
|---|---|---|---|
| `ACC-OOS-001` | Faktur, item tagihan, tanggung jawab pasien, kasir | Billing dan Kasir | Hanya arah dan isi pesan kejadian keuangan |
| `ACC-OOS-002` | Siklus piutang, siklus utang, kas dan bank, penyelesaian pembayaran | Finance | Hanya kontrak kejadian keuangan |
| `ACC-OOS-003` | Kas kecil operasional | Finance/Umum | Hanya kejadian pengeluaran dan pengisian ulang |
| `ACC-OOS-004` | Pengajuan dan persetujuan anggaran | Budgeting | Anggaran boleh memakai COA sebagai rujukan |
| `ACC-OOS-005` | Stok, opname, batch, kedaluwarsa, mutasi gudang | Inventory | Hanya akibat penilaian persediaan |
| `ACC-OOS-006` | Pembelian dan penerimaan barang | Purchasing | Hanya akibat kewajiban ke pemasok |
| `ACC-OOS-007` | Pendaftaran, pemindahan, dan penguasaan aset tetap | Fixed Asset | Hanya akibat perolehan, penyusutan, revaluasi, pelepasan |
| `ACC-OOS-008` | Master Cost Center dan Profit Center | Belum ditentukan | Accounting hanya menyimpan rujukan identitasnya |

Batas ini berasal dari `ACC-PRD-001` §1, §8, dan §27. **Kedua daftar di atas menunggu
konfirmasi owner** sebelum wawancara masuk ke pertanyaan berikutnya.

---

## Aktor dan Tanggung Jawab

Peran di bawah masih **calon**, belum keputusan. Penetapan finalnya ada di `ACC-OQ-028`.

| Calon peran | Tugas utama | Contoh orangnya |
|---|---|---|
| Accounting Viewer | Melihat journal dan laporan, tidak boleh mengubah apa pun | Manajemen, unit terkait |
| Accounting Staff | Membuat journal manual, mengimpor journal, memperbaiki draft | Staf akuntansi |
| Accounting Approver | Menyetujui journal sebelum disahkan | Supervisor akuntansi |
| Accounting Manager | Menutup dan membuka kembali periode, mengubah pemetaan posting | Kepala bagian akuntansi |
| Auditor | Membaca seluruh riwayat dan jejak audit, tanpa hak mengubah | Auditor internal/eksternal |
| Accounting Administrator | Mengatur COA dan konfigurasi teknis akuntansi | Admin sistem akuntansi |

---

## Open Questions dan Blocker

**Seluruh 37 pertanyaan sudah tertutup pada 1 September 2026.** Dari jumlah itu, 29 berasal dari
`ACC-PRD-001` §35 dan 7 ditemukan dari audit dokumen serta repository.

| Hasil | Jumlah | Keterangan |
|---|---:|---|
| Dijawab owner, menjadi `ACC-DEC-*` | 28 | Ditandai ~~TERJAWAB~~ pada judul pertanyaannya |
| Ditunda resmi ke Phase 2 | 9 | Ditandai `DEFERRED`, dasar hukumnya `ACC-DEC-036` |
| Masih menghalangi MVP | **0** | — |

Sembilan yang ditunda seluruhnya menyangkut integrasi otomatis, jurnal berulang, dan tutup buku,
yang sudah berada di luar MVP menurut `ACC-DEC-009`. Menundanya **tidak** membuat MVP menggantung:
rilis pertama tidak memuat satu pun jalur jurnal otomatis, sehingga tidak ada perilaku yang
bergantung pada jawaban kesembilan pertanyaan itu.

Setiap pertanyaan selalu punya opsi tambahan `Other — tuliskan pilihan atau batasan lain`,
walaupun tidak ditulis ulang satu per satu. Tanda **(Direkomendasikan)** adalah usulan, bukan
keputusan.

### Kelompok A — Struktur Accounting

#### ~~`ACC-OQ-001`~~ TERJAWAB → `ACC-DEC-010` (pilihan C — berbeda menurut jenis jurnal) — Alur hidup journal
**Pemblokir:** YA · **Sumber:** PRD §35 · **Owner:** Rizki

Sebuah journal melewati tahap apa saja sejak dibuat sampai sah masuk buku besar?

- **A. Draft → Disetujui → Disahkan** — paling ringkas, tetapi tidak ada tahap "menunggu
  persetujuan" yang terlihat, sehingga antrean approval sulit dipantau.
- **B. Draft → Menunggu Persetujuan → Disetujui → Disahkan** — jelas dan seragam, tetapi
  journal otomatis dari modul lain ikut antre walau isinya sudah pasti benar.
- **C. Berbeda menurut jenis journal (Direkomendasikan)** — journal manual memakai alur penuh
  seperti opsi B, journal otomatis memakai alur pendek. Alasannya journal manual dibuat
  manusia dan rawan salah, sedangkan journal otomatis lahir dari kejadian yang sudah disahkan
  modul lain. Konsekuensinya perlu tabel jenis journal yang menyimpan aturan alurnya.

**Contoh dampaknya.** Bila dipilih B, 500 kejadian tagihan per hari dari Billing akan menumpuk
di antrean persetujuan dan harus diklik satu per satu oleh supervisor.

#### ~~`ACC-OQ-002`~~ TERJAWAB → `ACC-DEC-015` (pilihan A — empat peran terpisah) — Siapa yang berwenang
**Pemblokir:** YA · **Owner:** Rizki

Siapa yang boleh mengajukan, menyetujui, mengesahkan, dan membalik journal?

- **A. Empat peran terpisah (Direkomendasikan)** — Staff mengajukan, Approver menyetujui,
  Manager mengesahkan dan membalik. Pemisahan ini yang biasa diminta auditor. Konsekuensinya
  minimal tiga orang harus tersedia setiap hari kerja.
- **B. Dua peran saja** — Staff mengajukan, Manager menyetujui sekaligus mengesahkan. Lebih
  ringan, kendali internalnya lebih longgar.
- **C. Pembalikan hanya boleh Manager, sisanya bebas** — paling longgar, berisiko pada audit.

#### ~~`ACC-OQ-003`~~ TERJAWAB → `ACC-DEC-016` (pilihan A — tidak pernah boleh) — Boleh menyetujui journal buatan sendiri?
**Pemblokir:** YA · **Owner:** Rizki

- **A. Tidak pernah boleh (Direkomendasikan)** — prinsip "empat mata" yang standar di
  akuntansi. Konsekuensinya harus ada approver pengganti saat yang bersangkutan cuti.
- **B. Boleh untuk jenis journal tertentu** — perlu daftar jenis yang dikecualikan.
- **C. Boleh bila nilainya di bawah batas tertentu** — misalnya di bawah Rp 5.000.000. Perlu
  keputusan angka batasnya dan siapa yang boleh mengubah angka itu.

### Kelompok B — Posting otomatis

#### `ACC-OQ-004` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Perlakuan kejadian keuangan dari modul lain
**Pemblokir:** YA · **Owner:** Rizki

- **A. Langsung disahkan otomatis** — paling cepat, tetapi kesalahan pemetaan langsung masuk
  buku besar dan hanya bisa diperbaiki lewat pembalikan.
- **B. Menjadi draft, menunggu orang** — paling aman, tetapi menumpuk pekerjaan manual harian.
- **C. Masuk ruang tunggu lalu ditinjau akuntansi** — mirip B dengan tampilan khusus.
- **D. Berbeda menurut jenis kejadian (Direkomendasikan)** — kejadian bervolume tinggi yang
  pemetaannya sudah pasti, seperti pengakuan piutang, langsung disahkan; kejadian jarang dan
  bernilai besar, seperti penghapusan piutang atau pelepasan aset, masuk draft. Konsekuensinya
  perlu tabel aturan per jenis kejadian.

#### ~~`ACC-OQ-005`~~ TERJAWAB → `ACC-DEC-011` (pilihan C — satu kejadian resmi, dua konsumen) — Sumber resmi pengakuan akuntansi atas tagihan
**Pemblokir:** YA, dan **lintas modul** · **Owner:** Owner Billing, owner Finance, Rizki

Ini pertanyaan paling berisiko di seluruh daftar. Salah pilih berarti satu tagihan tercatat
dua kali di buku besar.

Bukti yang sudah ada: kontrak `BIL-INTEGRATION-0.4` **sudah disetujui** pada 20 Agustus 2026
dan mengarahkan `BIL-INT-007` ke Piutang, `BIL-INT-008` ke Utang, `BIL-INT-009` ke penyesuaian
Piutang/Utang — semuanya wilayah Finance, bukan Accounting. PRD §36 aturan 13 melarang
mengubah kontrak Billing yang sudah disetujui.

- **A. Billing langsung ke Accounting** — **praktis tertutup**, bertentangan dengan kontrak
  yang sudah disetujui. Hanya mungkin lewat keputusan lintas modul yang mengubah kontrak
  Billing.
- **B. Finance meneruskan ke Accounting** — paling sesuai kontrak yang berlaku sekarang.
  Konsekuensinya Accounting bergantung pada Finance yang belum dibangun, sehingga pengakuan
  tagihan tertunda sampai Finance jadi.
- **C. Satu kejadian resmi diterbitkan sekali, dikonsumsi Finance dan Accounting
  (Direkomendasikan)** — kejadian keuangan diterbitkan satu kali dengan nomor unik, lalu
  Finance dan Accounting masing-masing membacanya untuk keperluan berbeda. Karena nomornya
  sama, pencatatan ganda bisa dicegah, dan Accounting tidak perlu menunggu Finance selesai.
  Konsekuensinya perlu kesepakatan siapa yang menerbitkan kejadian resmi itu — keputusan
  lintas modul yang harus melibatkan owner Billing.

**Contoh risikonya.** Bila Accounting berlangganan langsung ke Billing **dan** Finance juga
meneruskan kejadian yang sama, tagihan Budi Rp 10.000.000 menghasilkan dua journal. Buku besar
tetap seimbang, tetapi pendapatan rumah sakit tercatat Rp 20.000.000.

### Kelompok C — Chart of Accounts

#### ~~`ACC-OQ-006`~~ TERJAWAB → `ACC-DEC-022` (pilihan A — tidak pernah boleh) — Akun induk boleh menerima transaksi?
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Akun induk tidak pernah boleh menerima transaksi (Direkomendasikan)** — mencegah saldo
  tercatat di dua tingkat sekaligus. Contoh: `1-1000 Kas dan Setara Kas` hanya menjadi
  penjumlahan, transaksi masuk ke `1-1001 Kas Besar`.
- **B. Boleh dalam kondisi tertentu** — lebih luwes, laporan berisiko salah jumlah.

#### ~~`ACC-OQ-007`~~ TERJAWAB → `ACC-DEC-023` (pilihan A — tidak boleh diubah) — Kode akun boleh diubah setelah punya transaksi?
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Tidak boleh diubah sama sekali (Direkomendasikan)** — kode akun ikut tercetak di laporan
  lama; mengubahnya membuat laporan periode lalu tidak bisa direproduksi.
- **B. Boleh, kode lama disimpan sebagai riwayat** — lebih luwes, perlu tabel riwayat.
- **C. Boleh bebas** — paling sederhana, paling berisiko untuk audit.

#### ~~`ACC-OQ-008`~~ TERJAWAB → `ACC-DEC-019` (pilihan A — Cost Center saja) — Dimensi akuntansi yang diperlukan
**Pemblokir:** tidak · **Owner:** Rizki

Dimensi adalah label tambahan pada setiap baris journal supaya laporan bisa dipilah.

- **A. Cost Center saja, wajib untuk akun beban (Direkomendasikan)** — cukup untuk laporan laba
  rugi per unit, tidak membebani petugas. Contoh: beban obat wajib menyebut unit Rawat Inap
  Lantai 3.
- **B. Cost Center dan Profit Center, wajib untuk akun tertentu** — laporan lebih kaya,
  pengisian lebih berat.
- **C. Lengkap: Cost Center, Profit Center, Departemen, Unit, Service Line** — paling rinci,
  berisiko banyak baris journal tertahan karena dimensi belum diisi.

Perlu diputuskan juga: kewajiban dimensi ditentukan **per akun** atau **per jenis transaksi**.

### Kelompok D — Validasi journal

#### ~~`ACC-OQ-009`~~ TERJAWAB → `ACC-DEC-025` (pilihan A — boleh Draft, tidak boleh diajukan) — Journal yang belum seimbang
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Boleh disimpan sebagai draft, tidak boleh diajukan atau disahkan (Direkomendasikan)** —
  petugas bisa menyicil journal panjang tanpa kehilangan pekerjaan. Contoh: journal penggajian
  40 baris tidak harus selesai dalam satu duduk.
- **B. Tidak boleh disimpan sama sekali** — paling ketat, menyiksa untuk journal panjang.
- **C. Boleh diajukan dengan peringatan** — berbahaya, membuka jalan journal tidak seimbang
  lolos ke buku besar.

#### ~~`ACC-OQ-010`~~ TERJAWAB → `ACC-DEC-020` (pilihan A — rupiah saja) — Mata uang selain rupiah
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Hanya rupiah pada rilis pertama (Direkomendasikan)** — menghilangkan kebutuhan kurs,
  pembulatan, dan selisih kurs dari lingkup awal. Konsekuensinya penambahan valuta asing nanti
  bukan pekerjaan kecil.
- **B. Mendukung banyak mata uang sejak awal** — perlu keputusan tambahan: sumber kurs, tanggal
  kurs yang dipakai, aturan pembulatan, dan perlakuan selisih kurs.

### Kelompok E — Periode akuntansi

#### ~~`ACC-OQ-011`~~ TERJAWAB → `ACC-DEC-012` (pilihan B — Terbuka / Tutup Sementara / Tutup Permanen) — Alur hidup periode, dan arti `SOFT_CLOSED`
**Pemblokir:** YA · **Sumber:** PRD §35, diperluas dari audit · **Owner:** Rizki

PRD §18 menyebut status `SOFT_CLOSED` tetapi hanya menjelaskannya sebagai "belum approved".
Itu belum bisa diuji. Perlu ditegaskan apa yang boleh dan tidak boleh dilakukan pada status itu.

- **A. Hanya Terbuka dan Tertutup** — paling sederhana, tidak ada masa tenggang untuk
  penyesuaian tutup buku.
- **B. Terbuka → Tutup Sementara → Tutup Permanen (Direkomendasikan)** — pada Tutup Sementara,
  journal biasa ditolak tetapi journal penyesuaian dari akuntansi masih diterima. Ini
  mencerminkan praktik tutup buku yang sebenarnya. Konsekuensinya pemeriksaan hak akses menjadi
  dua lapis, bukan satu.
- **C. Periode terbuka berbeda per kelompok akun** — paling luwes, paling rumit dijelaskan ke
  pengguna dan paling rawan salah.

#### ~~`ACC-OQ-012`~~ TERJAWAB → `ACC-DEC-026` (pilihan A — Accounting Manager saja) — Siapa yang boleh menutup periode
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Accounting Manager saja (Direkomendasikan)** — penutupan periode mengunci pekerjaan
  seluruh rumah sakit, jadi wewenangnya sempit.
- **B. Manager atau Administrator** — ada cadangan saat manager berhalangan.

#### ~~`ACC-OQ-013`~~ TERJAWAB → `ACC-DEC-027` (pilihan A — boleh, wajib alasan tertulis) — Pembukaan kembali periode yang sudah ditutup
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Boleh, wajib alasan tertulis dan tercatat di jejak audit (Direkomendasikan)** — ada jalan
  keluar saat ditemukan kesalahan besar, tanpa menghilangkan pertanggungjawaban.
- **B. Boleh, wajib alasan dan persetujuan tingkat lebih tinggi** — lebih ketat, lebih lambat.
- **C. Tidak boleh sama sekali; koreksi masuk periode berjalan** — paling aman untuk audit,
  tetapi laporan periode lama tidak pernah bisa diperbaiki.

Perlu diputuskan juga apakah ada batas waktu, misalnya periode yang sudah lewat lebih dari
12 bulan tidak boleh dibuka lagi.

#### ~~`ACC-OQ-014`~~ TERJAWAB → `ACC-DEC-028` (pilihan B — hanya penyesuaian/pembalikan baru) — Setelah periode dibuka kembali
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Transaksi lama boleh diubah** — melanggar `ACC-DEC-006` tentang riwayat permanen.
  **Opsi ini tidak ditawarkan.**
- **B. Hanya boleh penyesuaian atau pembalikan baru (Direkomendasikan)** — sesuai
  `ACC-DEC-006`; riwayat lama utuh dan koreksi terlihat sebagai catatan tersendiri.
- **C. Gabungan, tergantung kasus** — memerlukan aturan tambahan yang belum ada.

### Kelompok F — Pembalikan dan koreksi

#### ~~`ACC-OQ-015`~~ TERJAWAB → `ACC-DEC-017` (pilihan C — keduanya sesuai kasus) — Cara mengoreksi journal yang sudah disahkan
**Pemblokir:** YA · **Owner:** Rizki

- **A. Selalu batalkan penuh lalu buat journal baru** — paling mudah dijelaskan, tetapi buku
  besar penuh pasangan catatan besar untuk kesalahan kecil.
- **B. Selalu journal penyesuaian atas selisihnya saja** — buku besar lebih ringkas, tetapi
  sulit menelusuri isi journal yang benar seharusnya seperti apa.
- **C. Keduanya, tergantung kasus (Direkomendasikan)** — salah akun atau salah pihak dibalik
  penuh, salah nominal cukup disesuaikan selisihnya. Konsekuensinya perlu aturan tertulis kapan
  memakai yang mana, supaya petugas tidak memilih sesuka hati.

**Contohnya.** Beban listrik Rp 12.000.000 salah dicatat ke akun beban air. Karena akunnya
salah, journal dibalik penuh lalu dibuat ulang. Bandingkan dengan beban listrik tercatat
Rp 12.000.000 padahal seharusnya Rp 12.500.000; di sini cukup penyesuaian Rp 500.000.

#### ~~`ACC-OQ-016`~~ TERJAWAB → `ACC-DEC-029` (pilihan A — ya, selalu perlu persetujuan) — Pembalikan perlu persetujuan baru?
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Ya, selalu perlu persetujuan (Direkomendasikan)** — pembalikan mengubah angka yang sudah
  dilaporkan, jadi setara dengan journal baru.
- **B. Tidak perlu, karena hanya membatalkan yang sudah disetujui** — lebih cepat, membuka celah
  pembatalan sepihak.

### Kelompok G — Journal berulang

#### `ACC-OQ-017` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Journal berulang
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Membuat draft otomatis, pengesahan tetap manual (Direkomendasikan)** — aman, karena
  nominal penyusutan atau sewa dibayar di muka tetap diperiksa manusia setiap bulan.
- **B. Membuat dan mengesahkan otomatis** — paling hemat tenaga, tetapi kesalahan template
  langsung berulang tiap bulan tanpa ada yang melihat.
- **C. Bisa diatur per template** — paling luwes, perlu kolom pengaturan tambahan.

### Kelompok H — Penutupan buku

#### `ACC-OQ-018` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Apa yang menghalangi tutup bulan
**Pemblokir:** tidak · **Owner:** Rizki

Mana yang benar-benar **menghalangi** penutupan, dan mana yang hanya peringatan? Calon
penghalang: journal belum disahkan; journal belum seimbang; kejadian keuangan gagal diproses;
integrasi belum cocok; penyusutan belum dijalankan; saldo di akun sementara; selisih saldo awal
dan akhir.

- **A. Journal belum disahkan dan kejadian gagal menjadi penghalang; sisanya peringatan
  (Direkomendasikan)** — keduanya pasti mengubah angka laporan bila dibiarkan. Sisanya bisa
  ditindaklanjuti tanpa menahan penutupan.
- **B. Semua calon di atas menjadi penghalang** — paling ketat, berisiko tutup buku molor
  berhari-hari karena hal kecil.
- **C. Semua hanya peringatan** — penutupan lancar, tetapi angka laporan bisa tidak final.

#### `ACC-OQ-019` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Penutupan perlu persetujuan?
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Perlu persetujuan tertulis di sistem (Direkomendasikan)** — ada bukti siapa menyatakan
  angka bulan itu final.
- **B. Cukup tindakan Accounting Manager tanpa persetujuan terpisah** — lebih cepat.

#### `ACC-OQ-020` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Tutup tahun dan laba ditahan
**Pemblokir:** tidak · **Owner:** Rizki

Pada akhir tahun, saldo akun pendapatan dan beban dinolkan, lalu selisihnya dipindahkan ke akun
laba ditahan.

- **A. Sistem membuat journal penutup otomatis, disahkan manual (Direkomendasikan)** —
  perhitungan mesin lebih teliti, keputusan tetap di tangan manusia.
- **B. Seluruhnya journal manual** — paling sederhana dibangun, paling rawan salah hitung.

Perlu keputusan tambahan: akun laba ditahan yang dipakai, dan apakah ada pembagian ke akun lain
sebelum masuk laba ditahan.

### Kelompok I — Pelaporan

#### ~~`ACC-OQ-021`~~ TERJAWAB → `ACC-DEC-030` (pilihan B — Neraca Saldo dan Buku Besar) — Laporan pada rilis pertama
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Empat laporan inti: Neraca Saldo, Buku Besar, Laba Rugi, Neraca (Direkomendasikan)** —
  keempatnya cukup untuk membuktikan modul bekerja benar dari ujung ke ujung.
- **B. Neraca Saldo dan Buku Besar dulu** — rilis lebih cepat, belum bisa dipakai manajemen.
- **C. Empat laporan inti ditambah laporan audit akuntansi** — paling lengkap, menambah beban
  rilis pertama.

#### ~~`ACC-OQ-022`~~ TERJAWAB → `ACC-DEC-034` (pilihan A — di luar kepemilikan Accounting) — Laporan pajak
**Pemblokir:** tidak, tetapi **lintas modul** · **Owner:** Rizki dan owner Finance/Billing

- **A. Di luar kepemilikan Accounting (Direkomendasikan)** — Accounting hanya menyediakan data
  akuntansi; penyusunan dan pelaporan pajak dimiliki pihak lain. Sesuai PRD §7.8 yang menahan
  laporan pajak sampai kepemilikannya dipastikan.
- **B. Accounting yang memiliki laporan pajak** — memperluas lingkup cukup jauh dan menuntut
  aturan perpajakan yang belum dikumpulkan.

### Kelompok J — Integrasi

#### `ACC-OQ-023` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Isi minimum pesan kejadian keuangan
**Pemblokir:** tidak, tetapi mengunci integrasi paralel · **Owner:** Rizki, owner Finance

- **A. Sesuai calon di PRD §22 apa adanya (Direkomendasikan)** — sudah memuat nomor kejadian,
  jenis, modul asal, nomor transaksi asal, waktu kejadian, tanggal akuntansi, nilai, mata uang,
  penanda urutan, dan kunci anti-ganda. Cukup lengkap untuk mulai.
- **B. Disederhanakan dulu, ditambah saat dibutuhkan** — lebih cepat disepakati, tetapi
  perubahan kontrak di tengah jalan mahal bagi modul lain.

#### ~~`ACC-OQ-024`~~ TERJAWAB → `ACC-DEC-035` (pilihan C — keduanya dipakai bersama) — Dasar pencegahan pencatatan ganda
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Nomor kejadian saja** — sederhana, gagal bila pengirim membuat ulang nomor.
- **B. Gabungan modul asal, nomor transaksi asal, jenis kejadian, dan versi** — tahan terhadap
  pengiriman ulang, rumit bila satu transaksi menghasilkan beberapa kejadian sejenis.
- **C. Keduanya dipakai bersama (Direkomendasikan)** — nomor kejadian sebagai kunci utama,
  gabungan sebagai jaring pengaman kedua. Konsekuensinya perlu dua indeks unik.

#### `ACC-OQ-025` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Kejadian yang gagal diproses
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Coba ulang otomatis beberapa kali, lalu masuk daftar gagal untuk ditangani manusia
  (Direkomendasikan)** — gangguan sesaat sembuh sendiri, gangguan sungguhan tetap terlihat.
  Contoh: dicoba ulang 3 kali dengan jeda naik, setelah itu masuk daftar gagal.
- **B. Langsung masuk daftar gagal tanpa coba ulang** — paling mudah dipantau, tetapi gangguan
  jaringan sesaat pun menuntut tindakan manual.
- **C. Coba ulang terus tanpa batas** — berbahaya, kejadian rusak akan mengulang selamanya.

Perlu diputuskan juga siapa yang diberi tahu saat sebuah kejadian masuk daftar gagal.

### Kelompok K — Migrasi dan saldo awal

#### ~~`ACC-OQ-026`~~ TERJAWAB → `ACC-DEC-018` (pilihan A — saldo awal saja) — Titik mulai Accounting V2
**Pemblokir:** YA untuk rancangan go-live · **Owner:** Rizki

- **A. Saldo awal saja (Direkomendasikan)** — cukup memasukkan posisi saldo per tanggal mulai,
  misalnya per 1 Januari 2027. Paling cepat dan paling kecil risikonya. Konsekuensinya laporan
  pembanding tahun sebelumnya tidak tersedia di sistem baru.
- **B. Seluruh riwayat journal dipindahkan** — laporan pembanding lengkap, tetapi memindahkan
  data akuntansi lama menuntut pembersihan besar dan berisiko membawa kesalahan lama.
- **C. Riwayat periode terbatas** — misalnya 12 bulan terakhir. Jalan tengah, tetap menuntut
  pemetaan COA lama ke COA baru.

#### ~~`ACC-OQ-027`~~ TERJAWAB → `ACC-DEC-033` (pilihan A — Manager + pimpinan keuangan) — Siapa yang mengesahkan saldo awal
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Accounting Manager, dengan persetujuan pimpinan keuangan (Direkomendasikan)** — saldo
  awal menentukan seluruh angka setelahnya, jadi wewenangnya paling tinggi.
- **B. Accounting Manager saja** — lebih cepat.

### Kelompok L — Keamanan

#### ~~`ACC-OQ-028`~~ TERJAWAB → `ACC-DEC-031` (pilihan A — enam peran) — Peran dan hak akses final
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Enam peran seperti calon di PRD §33 (Direkomendasikan)** — sudah memisahkan pembuat,
  penyetuju, pengesah, dan pembaca. Perlu dicocokkan dengan sistem hak akses yang sudah ada
  (`[AccessController]`, `[AccessPermission]`, `AccessTypes`).
- **B. Disederhanakan menjadi tiga peran** — lebih mudah dikelola, tetapi pemisahan tugas
  melemah dan berpotensi menjadi temuan audit.

#### ~~`ACC-OQ-029`~~ TERJAWAB → `ACC-DEC-032` (pilihan A — hanya laporan dan pemetaan posting) — Pembacaan data sensitif perlu dicatat?
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Hanya laporan keuangan dan riwayat pemetaan posting yang pembacaannya dicatat
  (Direkomendasikan)** — menangkap akses yang benar-benar sensitif tanpa membanjiri jejak audit.
  Contoh: satu pengguna membuka Buku Besar 200 kali sehari akan menghasilkan 200 baris yang
  tidak berguna bila semua dicatat.
- **B. Semua pembacaan dicatat** — paling lengkap, jejak audit membengkak cepat.
- **C. Tidak ada pembacaan yang dicatat** — paling ringan, berisiko pada audit.

### Kelompok M — Temuan audit, belum ada di PRD

Tujuh pertanyaan berikut tidak ada di `ACC-PRD-001` §35, tetapi tanpa jawabannya entity dan
alur tidak bisa dibentuk. Semuanya ditemukan saat menelaah dokumen dan repository.

#### ~~`ACC-OQ-030`~~ TERJAWAB → `ACC-DEC-009` (pilihan A — tulang punggung akuntansi) — Garis potong rilis pertama
**Pemblokir:** YA · **Owner:** Rizki

PRD §7 mendaftar sekitar 40 submenu, dan PRD §37 B menuntut setiap kemampuan digolongkan
menjadi MVP, Phase 2, Deferred, atau Excluded. Tidak ada satu pun pertanyaan di §35 yang
menetapkan garis potongnya. Ini pertanyaan dengan pengaruh terbesar, karena menentukan isi
seluruh dokumen setelahnya.

- **A. Rilis pertama = tulang punggung akuntansi (Direkomendasikan)** — COA, Journal manual,
  Pengesahan, Buku Besar, Periode, Neraca Saldo. Cukup untuk membuktikan pembukuan berjalan
  benar tanpa bergantung pada modul lain yang belum ada. Integrasi otomatis, journal berulang,
  impor, dan tutup tahun masuk tahap berikutnya.
- **B. Rilis pertama = tulang punggung + integrasi otomatis** — lebih bernilai bagi pengguna,
  tetapi bergantung pada `ACC-OQ-005` yang masih lintas modul dan pada Finance yang belum ada.
- **C. Seluruh isi PRD §7 sekaligus** — paling lengkap, waktu rilis paling panjang, risiko
  tertinggi.

#### ~~`ACC-OQ-031`~~ TERJAWAB → `ACC-DEC-013` (pilihan A — bulanan, tahun kalender) — Kalender periode akuntansi
**Pemblokir:** YA · **Owner:** Rizki

PRD §18 mengatur status periode tetapi tidak pernah mendefinisikan periodenya sendiri. Tanpa
ini, tabel periode tidak bisa dibentuk.

- **A. Bulanan, tahun buku mengikuti tahun kalender, 12 periode setahun (Direkomendasikan)** —
  praktik paling umum di rumah sakit Indonesia dan paling mudah dijelaskan. Contoh: periode
  `2026-09` berjalan 1–30 September 2026.
- **B. Bulanan, ditambah satu periode ke-13 khusus penyesuaian tutup tahun** — memisahkan
  penyesuaian audit dari angka Desember, tetapi menambah kerumitan pada semua laporan.
- **C. Tahun buku tidak mengikuti tahun kalender** — misalnya Juli sampai Juni. Perlu keputusan
  bulan awalnya.

#### ~~`ACC-OQ-032`~~ TERJAWAB → `ACC-DEC-014` (pilihan A — urut per jenis per bulan, boleh terlewat) — Penomoran journal
**Pemblokir:** YA · **Owner:** Rizki

PRD §10 menyebut `Journal Number` sebagai field, tetapi tidak ada aturan pembentukannya.

- **A. Bernomor urut per jenis journal per bulan, boleh ada nomor terlewat
  (Direkomendasikan)** — contoh `JU/2026/09/00001`. Aman dijalankan banyak pengguna sekaligus.
  Nomor bisa terlewat bila sebuah journal batal dibuat, dan itu wajar.
- **B. Bernomor urut tanpa boleh ada yang terlewat** — lebih rapi di mata sebagian auditor,
  tetapi menuntut penguncian antrean nomor. Bila dua petugas menyimpan journal bersamaan, satu
  harus menunggu, dan pada jam sibuk ini terasa lambat.
- **C. Nomor bebas diisi petugas** — paling luwes, paling rawan bentrok dan salah ketik.

#### `ACC-OQ-033` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Kejadian sah tetapi pemetaan akunnya belum ada
**Pemblokir:** YA · **Owner:** Rizki

PRD §20 dan §23 menyebut akun sementara dan akun perantara, tetapi tidak ada yang memutuskan
perlakuannya. `ACC-OQ-025` hanya membahas kejadian yang **gagal**, bukan kejadian sah yang
**belum punya pemetaan akun**.

- **A. Tolak, masuk daftar gagal, tidak ada journal yang dibuat (Direkomendasikan)** — buku
  besar tidak pernah berisi tebakan. Konsekuensinya angka laporan bisa kurang selama pemetaan
  belum dilengkapi, dan itu harus terlihat pada daftar periksa penutupan.
- **B. Tetap buat journal memakai akun sementara** — buku besar selalu lengkap dan seimbang,
  tetapi akun sementara bisa menumpuk dan harus dibersihkan sebelum tutup buku.

**Contohnya.** Fixed Asset mengirim kejadian penyusutan untuk kelompok aset baru "Alat
Laboratorium Molekuler" yang belum dipetakan ke akun beban penyusutan mana pun. Opsi A menahan
kejadian itu sampai akuntansi menambah pemetaan. Opsi B mencatat Rp 4.000.000 ke akun sementara
lalu memindahkannya nanti.

#### `ACC-OQ-034` `DEFERRED` ke Phase 2 (`ACC-DEC-036`) — Kejadian datang terlambat, periodenya sudah ditutup
**Pemblokir:** YA · **Owner:** Rizki

Tidak ada pertanyaan di §35 yang menangani persinggungan antara posting otomatis dan penguncian
periode.

- **A. Catat di periode terbuka berikutnya, tanggal dokumen asli tetap disimpan
  (Direkomendasikan)** — kejadian tidak pernah hilang, dan laporan periode yang sudah ditutup
  tidak berubah setelah dinyatakan final. Contoh: kejadian tertanggal 28 September masuk pada
  1 Oktober; journal-nya bertanggal akuntansi Oktober, tetapi menyimpan tanggal dokumen
  28 September untuk penelusuran.
- **B. Tolak, masuk daftar gagal, akuntansi memutuskan manual** — paling terkendali, tetapi
  menumpuk pekerjaan manual setiap awal bulan.
- **C. Buka kembali periode secara otomatis** — **tidak dianjurkan**; membuat angka laporan
  yang sudah final berubah diam-diam.

#### ~~`ACC-OQ-035`~~ TERJAWAB → `ACC-DEC-021` (pilihan A — diukur pada rupiah) — Keseimbangan diukur di mata uang mana
**Pemblokir:** tidak, tetapi mengunci invariant · **Owner:** Rizki

PRD §13 mewajibkan total debit sama dengan total kredit, sedangkan PRD §11 menyediakan dua
kolom nilai: nilai mata uang transaksi dan nilai mata uang dasar. Tidak dinyatakan yang mana
yang harus seimbang.

- **A. Keseimbangan diukur pada nilai rupiah (mata uang dasar) (Direkomendasikan)** — buku besar
  selalu seimbang apa pun mata uang transaksinya. Jawaban ini otomatis benar bila `ACC-OQ-010`
  memilih rupiah saja.
- **B. Harus seimbang di kedua mata uang sekaligus** — paling ketat, tetapi praktis mustahil
  saat kurs menghasilkan pembulatan.

#### ~~`ACC-OQ-036`~~ TERJAWAB → `ACC-DEC-024` (pilihan A — tidak boleh selama saldo belum nol) — Menonaktifkan akun yang saldonya belum nol
**Pemblokir:** tidak · **Owner:** Rizki

`ACC-OQ-007` hanya membahas perubahan kode akun, bukan penonaktifannya.

- **A. Tidak boleh dinonaktifkan selama saldo belum nol (Direkomendasikan)** — mencegah saldo
  tersembunyi yang tidak muncul di daftar akun aktif tetapi tetap ikut di neraca. Contoh: akun
  `1-1201 Piutang Asuransi X` bersaldo Rp 15.000.000 harus dipindahkan dulu lewat journal
  sebelum akun ditutup.
- **B. Boleh dinonaktifkan dengan peringatan** — lebih luwes, berisiko saldo terlupakan.

#### ~~`ACC-OQ-037`~~ TERJAWAB → `ACC-DEC-037` (pilihan A — per badan hukum) — Pemisahan pembukuan per badan hukum
**Pemblokir:** YA · **Sumber:** audit saat penyusunan blueprint, 1 September 2026 · **Owner:** Rizki

Tidak ada di `ACC-PRD-001` maupun di 36 pertanyaan sebelumnya. Ditemukan ketika memeriksa master
yang harus dirujuk `ACC-DEC-019`.

Bukti yang memicunya: kolom `LegalEntityId` dipakai pada **83 berkas** di `Areas/Corporate/`,
yaitu domain yang sama dengan Accounting, dan `MstCostCenter` yang wajib dirujuk Accounting
mensyaratkan kolom itu. Sebaliknya, modul Billing tidak memakainya sama sekali.

- **A. Ya, pembukuan dipisah per badan hukum (dipilih)** — konsisten dengan seluruh modul
  Corporate. Setiap badan hukum punya neraca sendiri, sebagaimana dituntut hukum perseroan.
- **B. Satu buku tunggal** — lebih sederhana, tetapi menyusul multi-badan-hukum nanti berarti
  membongkar COA, jurnal, dan periode sekaligus setelah ada data.
- **C. Disiapkan kolomnya, ditunda pemakaiannya** — jalan tengah, laporan MVP belum bisa dipilah.

**Akibat terpilihnya A:** `LegalEntityId` menjadi kolom wajib pada `AccChartOfAccount`,
`AccAccountingPeriod`, dan `AccJournal`. Unique index kode akun menjadi gabungan
`LegalEntityId` + `AccountCode`, bukan `AccountCode` saja. Neraca saldo dihitung per badan hukum.

---

## Catatan atas ketidakkonsistenan PRD

`ACC-CONF-002` — `ACC-PRD-001` §35 hanya menandai 8 pertanyaan sebagai pemblokir, sedangkan §38
Definition of Ready menuntut kepastian atas aturan COA, kebijakan penutupan, laporan MVP, dan
keamanan Accounting. Keempat hal itu berasal dari pertanyaan yang **tidak** ditandai pemblokir
(`ACC-OQ-006` sampai `008`, `018` sampai `021`, dan `028`).

Akibatnya gerbang §37 bisa dinyatakan lulus sementara §38 masih gagal. Wawancara ini
memperlakukan **seluruh 36 pertanyaan** sebagai wajib tertutup sebelum arsitektur dikunci, dan
mengabaikan pembedaan pemblokir/bukan-pemblokir milik §35 untuk keperluan itu. Bila owner ingin
memakai pembedaan §35 apa adanya, keputusan itu perlu dinyatakan tersendiri.

---

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-001` | Decision | Accounting menjadi bounded context tersendiri | Rizki | `approved` | Rizki, 1 September 2026 | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-002` | Decision | Accounting pemilik tunggal Journal, Posting, GL, Period, Closing | Rizki | `approved` | Rizki, 1 September 2026 | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-003` | Decision | Finance adalah bounded context di luar Accounting | Rizki | `approved` | Rizki, 1 September 2026 | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-004` | Decision | Billing dan Kasir tetap di luar Accounting | Rizki | `approved` | Rizki, 1 September 2026 | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-005` | Decision | Transaksi sumber tetap milik modul penerbitnya | Rizki | `approved` | Rizki, 1 September 2026 | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-006` | Decision | Riwayat yang sudah disahkan permanen; koreksi lewat pembalikan atau journal koreksi | Rizki | `approved` | Rizki, 1 September 2026 | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-007` | Decision | Accounting boleh dikembangkan paralel dengan Finance | Rizki | `approved` | Rizki, 1 September 2026 | `ACC-PRD-001@0.1` §4 |
| `ACC-DEC-008` | Decision | Migration Accounting boleh dibuat lebih dulu | Rizki | `approved` | Rizki, 1 September 2026 | `ACC-PRD-001@0.1` §4; **perlu ditinjau**, lihat `ACC-DEP-001` |
| `ACC-DEC-009` | Decision | **Garis potong rilis pertama = tulang punggung akuntansi.** Rilis pertama berisi COA, Jurnal manual, Pengesahan, Buku Besar, Periode, dan Neraca Saldo. Integrasi otomatis, jurnal berulang, impor CSV, tutup buku, serta Laba Rugi dan Neraca masuk tahap berikutnya | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-030` |
| `ACC-DEC-010` | Decision | **Alur hidup jurnal berbeda menurut jenis jurnal.** Jurnal manual memakai alur penuh Draft → Menunggu Persetujuan → Disetujui → Disahkan. Jurnal otomatis memakai alur pendek. Diperlukan tabel jenis jurnal yang menyimpan aturan alurnya | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-001` |
| `ACC-DEC-011` | Decision | **Satu kejadian keuangan resmi diterbitkan sekali, dikonsumsi Finance dan Accounting.** Kejadian membawa nomor unik yang sama bagi kedua konsumen, sehingga pencatatan ganda tercegah dan Accounting tidak menunggu Finance selesai | Rizki | `approved` **untuk sisi Accounting saja** | Rizki, 1 September 2026 | Jawaban `ACC-OQ-005`; lihat `CROSS_MODULE_DECISION_REQUIRED` di bawah |
| `ACC-DEC-012` | Decision | **Periode akuntansi memakai tiga status: Terbuka → Tutup Sementara → Tutup Permanen.** Pada Tutup Sementara, jurnal biasa ditolak tetapi jurnal penyesuaian dari akuntansi masih diterima. Pemeriksaan hak akses menjadi dua lapis | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-011` |
| `ACC-DEC-013` | Decision | **Periode akuntansi bulanan, tahun buku mengikuti tahun kalender, 12 periode setahun.** Contoh: periode `2026-09` berjalan 1 sampai 30 September 2026 | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-031` |
| `ACC-DEC-014` | Decision | **Nomor jurnal urut per jenis jurnal per bulan, dan nomor terlewat diperbolehkan.** Contoh `JU/2026/09/00001`. Tidak ada penguncian antrean nomor, sehingga beberapa petugas dapat menyimpan jurnal bersamaan tanpa saling menunggu | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-032` |
| `ACC-DEC-015` | Decision | **Empat peran terpisah.** Accounting Staff mengajukan, Accounting Approver menyetujui, Accounting Manager mengesahkan dan membalik. Wajib tersedia approver pengganti | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-002` |
| `ACC-DEC-016` | Decision | **Pembuat jurnal tidak pernah boleh menyetujui jurnalnya sendiri**, tanpa pengecualian nilai maupun jenis jurnal | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-003` |
| `ACC-DEC-017` | Decision | **Koreksi jurnal yang sudah disahkan memakai dua cara sesuai kasus.** Salah akun atau salah pihak dibalik penuh lalu dibuat ulang; salah nominal cukup jurnal penyesuaian atas selisihnya. Aturan tertulis kapan memakai yang mana wajib disusun agar petugas tidak memilih sesuka hati | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-015` |
| `ACC-DEC-018` | Decision | **Accounting V2 dimulai dari saldo awal saja**, tanpa memindahkan riwayat jurnal lama. Laporan pembanding tahun sebelumnya tidak tersedia di sistem baru | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-026` |
| `ACC-DEC-019` | Decision | **Dimensi akuntansi hanya Cost Center**, wajib diisi untuk akun beban dan tidak wajib untuk akun lain | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-008` |
| `ACC-DEC-020` | Decision | **Rilis pertama hanya mendukung rupiah.** Tidak ada kolom kurs, tidak ada perhitungan selisih kurs | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-010` |
| `ACC-DEC-021` | Decision | **Keseimbangan debit dan kredit diukur pada nilai rupiah.** Merupakan akibat langsung `ACC-DEC-020`; `ACC-OQ-035` tertutup tanpa pertanyaan terpisah | Rizki | `approved` | Rizki, 1 September 2026 | Turunan `ACC-DEC-020`, jawaban `ACC-OQ-035` |
| `ACC-DEC-022` | Decision | **Akun induk tidak pernah boleh menerima transaksi.** Hanya akun paling bawah yang dapat diisi jurnal | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-006` |
| `ACC-DEC-023` | Decision | **Kode akun tidak boleh diubah setelah akun mempunyai transaksi.** Bila diperlukan kode berbeda, buat akun baru lalu pindahkan saldonya lewat jurnal | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-007` |
| `ACC-DEC-024` | Decision | **Akun tidak boleh dinonaktifkan selama saldonya belum nol.** Saldo wajib dipindahkan lewat jurnal lebih dahulu | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-036` |
| `ACC-DEC-025` | Decision | **Jurnal yang belum seimbang boleh disimpan sebagai Draft, tetapi tidak boleh diajukan maupun disahkan.** Tombol Ajukan tetap mati sampai selisih menjadi nol | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-009` |
| `ACC-DEC-026` | Decision | **Hanya Accounting Manager yang boleh menutup periode**, karena penutupan mengunci pekerjaan seluruh rumah sakit | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-012` |
| `ACC-DEC-027` | Decision | **Periode yang sudah ditutup boleh dibuka kembali dengan alasan tertulis wajib**, dan alasan itu tercatat di jejak audit. Tidak diperlukan persetujuan tingkat lebih tinggi | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-013` |
| `ACC-DEC-028` | Decision | **Setelah periode dibuka kembali, hanya penyesuaian atau pembalikan baru yang diperbolehkan.** Transaksi lama tetap tidak dapat diubah, sesuai `ACC-DEC-006` | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-014` |
| `ACC-DEC-029` | Decision | **Pembalikan jurnal memerlukan persetujuan baru**, diperlakukan setara jurnal baru karena mengubah angka yang sudah dilaporkan | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-016` |
| `ACC-DEC-030` | Decision | **Laporan pada rilis pertama hanya Neraca Saldo dan Buku Besar.** Laba Rugi dan Neraca masuk tahap berikutnya karena menuntut klasifikasi COA yang matang | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-021`; sejalan `ACC-DEC-009` |
| `ACC-DEC-031` | Decision | **Enam peran Accounting dipakai:** Viewer, Staff, Approver, Manager, Auditor, dan Administrator. Pemetaan ke mekanisme hak akses yang sudah ada wajib diperiksa saat implementasi | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-028`; sejalan `ACC-DEC-015` |
| `ACC-DEC-032` | Decision | **Hanya pembacaan laporan keuangan dan riwayat pemetaan posting yang dicatat di jejak audit.** Pembacaan daftar dan rincian jurnal sehari-hari tidak dicatat | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-029` |
| `ACC-DEC-033` | Decision | **Saldo awal disahkan Accounting Manager dengan persetujuan pimpinan keuangan.** Wewenangnya di atas pengesahan jurnal biasa, karena saldo awal menentukan seluruh angka setelahnya | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-027` |
| `ACC-DEC-034` | Decision | **Laporan pajak berada di luar kepemilikan Accounting.** Accounting hanya menyediakan data akuntansi; penyusunan dan pelaporan pajak dimiliki pihak lain | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-022`; sejalan PRD §7.8 |
| `ACC-DEC-035` | Decision | **Pencegahan pencatatan ganda memakai dua kunci sekaligus:** nomor kejadian sebagai kunci utama, dan gabungan modul asal + nomor transaksi asal + jenis kejadian + versi sebagai jaring pengaman kedua. Diperlukan dua indeks unik | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-024` |
| `ACC-DEC-036` | Decision | **Sembilan pertanyaan sisa ditandai `DEFERRED` ke Phase 2:** `ACC-OQ-004`, `017`, `018`, `019`, `020`, `023`, `025`, `033`, `034`. Semuanya menyangkut integrasi otomatis, jurnal berulang, dan tutup buku, yang sudah berada di luar MVP menurut `ACC-DEC-009`. Rilis pertama boleh berjalan tanpa jawaban atas kesembilan pertanyaan itu | Rizki | `approved` | Rizki, 1 September 2026 | Keputusan owner, 1 September 2026 |
| `ACC-DEC-037` | Decision | **Pembukuan dipisah per badan hukum (`MstLegalEntity`).** COA, jurnal, periode, dan neraca saldo semuanya bercabang per `LegalEntityId`. Kode akun unik per badan hukum, keseimbangan debit-kredit diukur per badan hukum, dan satu jurnal tidak boleh mencampur dua badan hukum | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-037`; bukti: `LegalEntityId` dipakai 83 berkas di `Areas/Corporate/@aa837d7`, dan `MstCostCenter` mensyaratkannya |
| `ACC-DEC-038` | Decision | **Lifecycle registry `Acc` dinaikkan `PLANNED` → `ACTIVE`.** Baris canonical `| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |`. Accounting memasuki tahap implementasi source model persisted. Wewenangnya **hanya** source model; `dotnet ef migrations add`, `dotnet ef database update`, perubahan shared database, deployment, production activation, dan bypass Migration Coordination Gate **tidak** termasuk. `BE-ACC-006` tetap punya gerbang tersendiri. Entri `Finance` / `Fin` tidak diubah | Rizki | `approved` | Rizki, 1 September 2026 | FINAL OWNER APPROVAL `ACC-BP-001` revisi 5, sesi 1 September 2026. Preseden: `Inp` (`RWI-DEC-068`, 24 Agustus 2026) dan `Mrc` (`RM-DEC-029`, 31 Agustus 2026), keduanya diaktifkan pemilik modulnya sendiri. Termaterialisasi di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` beserta catatan perubahan lifecycle |


Keputusan `ACC-DEC-001` sampai `ACC-DEC-008` **tidak dibuka kembali** sesuai PRD §36 aturan 3.

---

## `CROSS_MODULE_DECISION_REQUIRED` — turunan `ACC-DEC-011`

`ACC-DEC-011` menetapkan bahwa satu kejadian keuangan diterbitkan **sekali** lalu dibaca Finance
dan Accounting. Rizki berwenang menyetujui sisi Accounting, tetapi **tidak berwenang** menetapkan
siapa yang menerbitkan kejadian itu, karena penerbitnya berada di modul lain.

| Butir | Isi |
|---|---|
| ID | `ACC-XM-001` |
| Pertanyaan | Siapa yang menerbitkan kejadian keuangan resmi atas tagihan pasien: Billing, atau Finance setelah menerima serah terima dari Billing? |
| Pihak yang harus setuju | Owner Billing, owner Finance, dan Rizki |
| Bukti yang mengikat | `billing-kasir/contracts/integration-contract.md#BIL-INT-007..009@aa837d7`, status **approved** 20 Agustus 2026 |
| Batasan | PRD §36 aturan 13 melarang mengubah kontrak Billing yang sudah disetujui. `ACC-DEC-011` **tidak** mengubah `BIL-INT-007` sampai `BIL-INT-009`; keduanya bisa berjalan berdampingan bila kejadian resmi diterbitkan sekali dan nomornya dipakai bersama |
| Status | `TERBUKA` |
| Memblokir | Kontrak integrasi Accounting, dan seluruh jalur jurnal otomatis |
| **Tidak** memblokir | Rilis pertama, karena `ACC-DEC-009` menempatkan integrasi otomatis di tahap berikutnya |

Butir ini sengaja tidak diputuskan sepihak. Yang bisa dilakukan Accounting sekarang adalah
menyiapkan bentuk kotak masuk kejadian yang netral terhadap siapa pun penerbitnya, sehingga
keputusan `ACC-XM-001` nanti tidak memaksa perombakan.

**Kenapa ini aman ditunda.** Rilis pertama tidak memuat jurnal otomatis sama sekali. Seluruh
jurnal pada rilis pertama dibuat manusia lewat layar Jurnal Manual. Jadi tidak ada satu pun
jalur yang bisa menghasilkan pencatatan ganda sebelum `ACC-XM-001` diputuskan.

---

## Acceptance Criteria yang sudah dapat diuji

Tiga hal berikut sudah pasti apa pun jawaban atas pertanyaan terbuka, karena berasal dari
keputusan yang sudah disetujui.

| ID | Kriteria | Berasal dari |
|---|---|---|
| `ACC-AC-001` | Journal yang total debit dan total kreditnya tidak sama persis harus ditolak saat pengesahan. Contoh: debit Rp 10.000.000 lawan kredit Rp 9.999.999 ditolak | PRD §13 rule 1 |
| `ACC-AC-002` | Journal yang sudah disahkan tidak dapat diubah maupun dihapus lewat jalur mana pun, termasuk jalur administrator | `ACC-DEC-006` |
| `ACC-AC-003` | Satu kejadian keuangan yang sama, dikirim tiga kali, hanya menghasilkan satu journal. Pengiriman kedua dan ketiga mengembalikan hasil yang sama tanpa membuat catatan baru | PRD §24 |

---

## Langkah berikutnya

1. Owner menjawab 36 pertanyaan di atas; setiap jawaban menjadi `ACC-DEC-009` dan seterusnya.
2. Jalankan `/trace-existing-capabilities` untuk audit kemampuan yang penuh.
3. Jalankan `/requirement-completeness-gate` untuk menilai kelengkapan requirement.
4. Jalankan `/hospital-domain-architect`, lalu `/design-business-module` untuk arsitektur final.
5. Selesaikan `ACC-DEP-001` sampai `ACC-DEP-003` bersama lead sebelum entity dan migration
   pertama dibuat.
