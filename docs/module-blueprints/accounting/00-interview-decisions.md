# Accounting — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `ACC-BP-001` |
| Revision | `11` — dinaikkan 24 September 2026 (kedua); `ACC-DEC-092` dan `ACC-DEC-093` menutup dua butir desain yang tersisa saat `GATE-DESAIN-0924` dibuka. Sebelumnya `10` — dinaikkan 24 September 2026; `ACC-DEC-082` sampai `ACC-DEC-091` mencatat keputusan pasca-ratifikasi Finance (`FIN-DEC-001`..`023`, 20 September 2026): `ACC-XM-001` ditutup, katalog 17 kode diratifikasi, mode pemrosesan, isi tanda terima, `JASA_MEDIS`, bentuk saldo subledger, syarat akun layanan, gerbang cutover, dan penerimaan sebelum tagihan final sebagai uang muka pasien. Sebelumnya `9` — dinaikkan 14 September 2026; `ACC-DEC-074` sampai `ACC-DEC-081` mencatat delapan keputusan owner hasil review "rencana sampai 100%" (`OD-ACC-02`, `03`, `09`, `10`, `11`, `13`, `14`, `15`). Sebelumnya `8` — dinaikkan 11 September 2026; `ACC-DEC-072` dan `ACC-DEC-073` menutup dua keputusan terbuka `BE-ACC-P2-012` (koreksi jurnal dan template berulang yang menyentuh control account). Sebelumnya `7`, 10 September 2026; `ACC-DEC-071` menutup `DEC-ACC-P2-011` dengan memilih saldo subledger per periode dari Finance, dan membuka `ACC-GAP-013`. Sebelumnya `6` (`ACC-DEC-070`, enam peringatan, menutup `ACC-GAP-012`), `5` (`ACC-DEC-067`, `068`, `069`), dan `4`, 8 September 2026 oleh Amendment pass Phase 2 |
| Status | `approved` untuk scope MVP; `approved` untuk keputusan Phase 2. Ratifikasi lintas modul `ACC-XM-001` **tertutup 24 September 2026** (`ACC-DEC-082`); yang tersisa hanya butir lintas modul yang bukan wewenang Accounting, lihat bagian *Keputusan pasca-ratifikasi Finance* |
| Pass | `Scope pass` — **selesai** 1 September 2026 · `Amendment pass — Phase 2` — **selesai** 8 September 2026 · `Amendment pass — pasca-ratifikasi Finance` — **selesai** 24 September 2026 (`rizkiG` `b2b265af`, `RizkiV2` `c941012ac`) |
| Backend SHA — Amendment pass | `02c3219` (branch `rizkiG`) |
| Frontend SHA — Amendment pass | `e732424eb` (branch `RizkiV2`) |
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

**Seluruh 37 pertanyaan sudah tertutup pada 1 September 2026**, dan **sembilan yang ditunda ikut
tertutup pada 8 September 2026** lewat Amendment pass Phase 2. Dari jumlah itu, 29 berasal dari
`ACC-PRD-001` §35 dan 7 ditemukan dari audit dokumen serta repository.

| Hasil | Jumlah | Keterangan |
|---|---:|---|
| Dijawab owner 1 September 2026, menjadi `ACC-DEC-*` | 28 | Ditandai ~~TERJAWAB~~ pada judul pertanyaannya |
| ~~Ditunda ke Phase 2~~ **dijawab owner 8 September 2026** | 9 | Menjadi `ACC-DEC-045` sampai `ACC-DEC-053`. `ACC-DEC-036` berstatus `superseded` |
| Masih menghalangi MVP | **0** | — |
| Masih menghalangi Phase 2 | **0 keputusan bisnis**; ~~tersisa 1 ratifikasi lintas modul~~ **0 sejak 24 September 2026** | `ACC-XM-001` ditutup `ACC-DEC-082` (Accounting `ACC-DEC-044`, Billing `ACC-DEC-059`, Finance `FIN-DEC-001`). Butir lintas modul yang masih terbuka — mekanisme autentikasi, kode saldo subledger, kode deposit/refund/selisih shift — hanya menahan **pengaktifan pengiriman dan cutover**, bukan perancangan maupun pembangunan kotak masuk |

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

#### ~~`ACC-OQ-004`~~ TERJAWAB 8 September 2026 → `ACC-DEC-045` (pilihan D — berbeda menurut jenis kejadian) — Perlakuan kejadian keuangan dari modul lain
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

#### ~~`ACC-OQ-017`~~ TERJAWAB 8 September 2026 → `ACC-DEC-050` (pilihan A — draft otomatis, pengesahan manual) — Journal berulang
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Membuat draft otomatis, pengesahan tetap manual (Direkomendasikan)** — aman, karena
  nominal penyusutan atau sewa dibayar di muka tetap diperiksa manusia setiap bulan.
- **B. Membuat dan mengesahkan otomatis** — paling hemat tenaga, tetapi kesalahan template
  langsung berulang tiap bulan tanpa ada yang melihat.
- **C. Bisa diatur per template** — paling luwes, perlu kolom pengaturan tambahan.

### Kelompok H — Penutupan buku

#### ~~`ACC-OQ-018`~~ TERJAWAB 8 September 2026 → `ACC-DEC-051` (pilihan A — dua penghalang, sisanya peringatan) — Apa yang menghalangi tutup bulan
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

#### ~~`ACC-OQ-019`~~ TERJAWAB 8 September 2026 → `ACC-DEC-052` (pilihan A — perlu persetujuan tertulis) — Penutupan perlu persetujuan?
**Pemblokir:** tidak · **Owner:** Rizki

- **A. Perlu persetujuan tertulis di sistem (Direkomendasikan)** — ada bukti siapa menyatakan
  angka bulan itu final.
- **B. Cukup tindakan Accounting Manager tanpa persetujuan terpisah** — lebih cepat.

#### ~~`ACC-OQ-020`~~ TERJAWAB 8 September 2026 → `ACC-DEC-053` (pilihan A — jurnal penutup otomatis, disahkan manual) — Tutup tahun dan laba ditahan
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

#### ~~`ACC-OQ-023`~~ TERJAWAB 8 September 2026 → `ACC-DEC-048` (pilihan A — sesuai calon PRD §22) — Isi minimum pesan kejadian keuangan
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

#### ~~`ACC-OQ-025`~~ TERJAWAB 8 September 2026 → `ACC-DEC-049` (pilihan A — coba ulang 3 kali lalu daftar gagal) — Kejadian yang gagal diproses
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

#### ~~`ACC-OQ-033`~~ TERJAWAB 8 September 2026 → `ACC-DEC-046` (pilihan A — tolak, masuk daftar gagal) — Kejadian sah tetapi pemetaan akunnya belum ada
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

#### ~~`ACC-OQ-034`~~ TERJAWAB 8 September 2026 → `ACC-DEC-047` (pilihan A — masuk periode terbuka berikutnya) — Kejadian datang terlambat, periodenya sudah ditutup
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
| `ACC-DEC-036` | Decision | ~~**Sembilan pertanyaan sisa ditandai `DEFERRED` ke Phase 2:**~~ **`superseded` 8 September 2026 oleh `ACC-DEC-045` sampai `ACC-DEC-053`.** Kesembilan pertanyaan sudah dijawab owner, sehingga penundaannya tidak lagi berlaku. Isi aslinya: `ACC-OQ-004`, `017`, `018`, `019`, `020`, `023`, `025`, `033`, `034`. Semuanya menyangkut integrasi otomatis, jurnal berulang, dan tutup buku, yang sudah berada di luar MVP menurut `ACC-DEC-009`. Rilis pertama boleh berjalan tanpa jawaban atas kesembilan pertanyaan itu | Rizki | `superseded` | Rizki, 1 September 2026; digantikan 8 September 2026 | Keputusan owner, 1 September 2026. Digantikan Amendment pass Phase 2 |
| `ACC-DEC-037` | Decision | **Pembukuan dipisah per badan hukum (`MstLegalEntity`).** COA, jurnal, periode, dan neraca saldo semuanya bercabang per `LegalEntityId`. Kode akun unik per badan hukum, keseimbangan debit-kredit diukur per badan hukum, dan satu jurnal tidak boleh mencampur dua badan hukum | Rizki | `approved` | Rizki, 1 September 2026 | Jawaban `ACC-OQ-037`; bukti: `LegalEntityId` dipakai 83 berkas di `Areas/Corporate/@aa837d7`, dan `MstCostCenter` mensyaratkannya |
| `ACC-DEC-038` | Decision | **Lifecycle registry `Acc` dinaikkan `PLANNED` → `ACTIVE`.** Baris canonical `| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |`. Accounting memasuki tahap implementasi source model persisted. Wewenangnya **hanya** source model; `dotnet ef migrations add`, `dotnet ef database update`, perubahan shared database, deployment, production activation, dan bypass Migration Coordination Gate **tidak** termasuk. `BE-ACC-006` tetap punya gerbang tersendiri. Entri `Finance` / `Fin` tidak diubah | Rizki | `approved` | Rizki, 1 September 2026 | FINAL OWNER APPROVAL `ACC-BP-001` revisi 5, sesi 1 September 2026. Preseden: `Inp` (`RWI-DEC-068`, 24 Agustus 2026) dan `Mrc` (`RM-DEC-029`, 31 Agustus 2026), keduanya diaktifkan pemilik modulnya sendiri. Termaterialisasi di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` beserta catatan perubahan lifecycle |


| `ACC-DEC-039` | Decision | **Nama entity riwayat persetujuan jurnal adalah `AccJournalApproval`, bukan `AccJournalApprovalHistory`.** Artefak canonical — `erd/02-journal.md`, `erd/00-context-erd.md`, `erd/data-dictionary.md` bagian 6 beserta DDL-nya, dan `roadmap/backend-roadmap.md` — seluruhnya memakai `AccJournalApproval`. **Nama pada instruksi task bukan sumber kebenaran yang lebih tinggi daripada artefak canonical.** Entity tidak diubah menjadi `...History` | Rizki | `approved` | Rizki, 2 September 2026 | Pertentangan ditemukan saat `BE-ACC-005`, dilaporkan pada bagian 15.A laporan task dan tidak diputuskan sepihak. Implementasi `BE-ACC-005` sudah memakai nama canonical, jadi keputusan ini **tidak menuntut perubahan kode** |
| `ACC-DEC-040` | Decision | **Tanggal bisnis memakai `date`; waktu peristiwa memakai `timestamp with time zone`.** `AccountingDate` dan `DocumentDate` pada `AccJournal`, serta `StartDate` dan `EndDate` pada `AccAccountingPeriod`, bertipe `date`. `SubmittedAt`, `ApprovedAt`, `PostedAt`, `ActionAt`, `ClosedAt`, `ReopenedAt`, dan seluruh kolom waktu audit `IdentityModel` bertipe `timestamp with time zone`. Alasannya: **tanggal akuntansi menentukan periode pembukuan, bukan waktu kejadian** — menyimpan jam padanya membuka celah beda hari akibat zona waktu | Rizki | `approved` | Rizki, 2 September 2026 | Menyelesaikan pertentangan antara diagram ERD (`date`) dan DDL contoh pada `erd/data-dictionary.md` (`timestamp`). DDL contoh diperbaiki mengikuti keputusan ini. Implementasi `BE-ACC-004` dan `BE-ACC-005` sudah sesuai, jadi keputusan ini **tidak menuntut perubahan kode** |
| `ACC-DEC-041` | Decision | **MVP berjalan pada SATU badan hukum; penyaringan badan hukum per pengguna DITUNDA.** `LegalEntityId` tetap disimpan pada `AccChartOfAccount`, `AccAccountingPeriod`, dan `AccJournal`, dan tetap menegakkan **pemisahan data** (`ACC-DEC-037` tidak dibatalkan): kode akun tetap unik per badan hukum, satu jurnal tetap tidak boleh mencampur dua badan hukum. Yang ditunda hanya **penyaringan per pengguna** — pertanyaan "pengguna ini berhak atas badan hukum yang mana". Akibatnya `ACC-DEP-008` turun dari **blocker MVP** menjadi **prasyarat multi-badan-hukum**, dan acceptance `403` pada `BE-ACC-007`..`014` ditandai `DEFERRED`. Sebagai gantinya wajib ada **penjaga jumlah badan hukum**: bila badan hukum aktif lebih dari satu sementara penegakan belum ada, sistem menolak keras dan menyebutkan sebabnya | Rizki | `approved` | Rizki, 2 September 2026 | Keputusan owner, 2 September 2026. Dasar: `04-prd-to-mvp.md` baris 106 memang sudah menetapkan MVP selesai ketika **satu badan hukum** berjalan penuh; `UAT-15` menguji pemisahan data, bukan penolakan pengguna; dan mekanisme otorisasi badan hukum tidak ada di platform serta tidak ada yang akan membangunnya. Diverifikasi 2 September 2026: 17 controller menerima `LegalEntityId` dari `[FromQuery]`, **0** klaim badan hukum di JWT, **0** `HasQueryFilter` di seluruh repo |
| `ACC-DEC-042` | Decision | **Kode akun (`AccountCode`) dapat diubah selama akun belum dipakai baris jurnal yang disahkan.** Menyelesaikan pertentangan antara `ACC-API-0.1` — yang menulis `PUT` hanya mengubah "nama, induk, atau keterangan" — dengan `ACC-VALIDATION-0.2` bagian 1 dan acceptance `BE-ACC-007` (4), yang keduanya mengandaikan kode **dapat** diubah pada keadaan lain. Aturan validasi yang melarang sesuatu yang tidak pernah mungkin adalah aturan kosong. `ACC-API` naik `0.1` → `0.2`; deskripsi `PUT` diperbaiki; `UpdateChartOfAccountDto` memuat `AccountCode`. Jurnal `Draft` tidak mengunci kode | Rizki | `approved` | Rizki, 2 September 2026 | Pertentangan ditemukan saat `BE-ACC-007` dan dilaporkan pada laporan task bagian 6, tidak diputuskan sepihak. Implementasi sudah memakai bacaan ini, sehingga keputusan **tidak menuntut perubahan kode**. Dibuktikan `ChartOfAccountServiceTests.KodeAkunBertransaksi_GagalDiubah_Ditolak409` dan `JurnalDraft_TidakMenguncikanAkun` |
| `ACC-DEC-043` | Decision | **Accounting berjalan di atas badan hukum bertanda `IsDefault`, dan penjaga menuntut TEPAT SATU default — bukan tepat satu badan hukum aktif.** Menyempurnakan mekanisme `ACC-DEC-041` setelah pemeriksaan database sungguhan pada 2 September 2026 menemukan **tiga** badan hukum aktif: `LE-MMC-001` PT Metropolitan Medical Centre (bertanda `IsDefault`, memiliki 5 unit organisasi, 5 cost center, 3 lokasi kerja), serta `LE-MDC-001` dan `LE-MHS-001` yang keduanya kosong. Menolak berdasarkan **jumlah aktif** akan mematikan Accounting tanpa alasan sebenarnya, karena bahaya yang dijaga bukan "ada lebih dari satu badan hukum di master" melainkan **ketidakjelasan buku besar mana yang disentuh** — dan `IsDefault` sudah menjawabnya. Nol default maupun lebih dari satu default tetap **ditolak keras** `409`. Nol data modul lain disentuh | Rizki | `approved` | Rizki, 2 September 2026 | Ditemukan saat owner meminta pembuatan badan hukum pertama; pemeriksaan read-only dijalankan lebih dahulu justru untuk mencegah penambahan yang akan memperburuk keadaan. `IsDefault` adalah kolom platform yang sudah ada, bukan konsep baru yang dikarang Accounting. Dibuktikan `ChartOfAccountServiceTests.TigaBadanHukumAktifDenganSatuUtama_AccountingTetapBerjalan`, `LebihDariSatuBadanHukumUtama_SeluruhEndpointMenolak`, dan `TanpaBadanHukumUtama_SeluruhEndpointMenolak` |


### Keputusan Phase 2 — Amendment pass 8 September 2026

Sebelas baris berikut ditambahkan pada Amendment pass Phase 2. Sepuluh yang pertama menjawab
`ACC-XM-001` beserta sembilan pertanyaan yang dulu ditunda `ACC-DEC-036`; satu yang terakhir
adalah turunan yang muncul dari jawaban tutup tahun.

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-044` | Decision | **Finance yang menerbitkan kejadian keuangan resmi atas tagihan pasien.** Alurnya: Billing menyerahkan akibat keuangan tagihan ke Finance sesuai `BIL-INT-007`, `008`, dan `009`; Finance mencatatnya sebagai Piutang/Utang, lalu **menerbitkan satu kejadian keuangan resmi bernomor unik**; Accounting membaca kejadian itu lalu membuat jurnalnya. Accounting **tidak** berlangganan langsung ke Billing. Menjawab `ACC-XM-001` | Rizki (sisi Accounting); ratifikasi menuntut owner Billing dan owner Finance | `approved` **sisi Accounting**, `PENDING_RATIFICATION` sisi lintas modul | Rizki, 8 September 2026 | Dipilih karena **nol perubahan** pada `BIL-INTEGRATION-0.4` yang sudah `approved` 20 Agustus 2026, sehingga tidak melanggar PRD §36 aturan 13. **Dikonfirmasi owner Billing 9 September 2026** (`ACC-DEC-059`), lalu **DIPERLUAS `ACC-DEC-061`** menjadi seluruh kejadian keuangan, bukan hanya tagihan pasien. Ratifikasi bentuk pesan oleh owner Finance masih ditunggu |
| `ACC-DEC-045` | Decision | **Perlakuan kejadian keuangan masuk berbeda menurut jenis kejadiannya.** Jenis bervolume tinggi yang pemetaan akunnya sudah pasti — misalnya pengakuan piutang rawat jalan — langsung menjadi jurnal berstatus `Posted`. Jenis yang jarang dan bernilai besar — misalnya penghapusan piutang dan pelepasan aset tetap — menjadi jurnal `Draft` yang menunggu pemeriksaan manusia. Konsekuensinya perlu **satu master data baru: aturan perlakuan per jenis kejadian**. Menjawab `ACC-OQ-004` | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan D pada `ACC-OQ-004` |
| `ACC-DEC-046` | Decision | **Kejadian sah yang jenisnya belum dipetakan ke akun mana pun DITOLAK dan masuk daftar gagal.** Tidak ada jurnal yang dibuat, dan tidak ada akun sementara yang dipakai. Buku besar tidak boleh berisi tebakan. Konsekuensi yang mengikat: **daftar kejadian tertahan wajib muncul pada daftar periksa penutupan bulan**, karena angka laporan bisa kurang selama pemetaannya belum dilengkapi. Menjawab `ACC-OQ-033` | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan A pada `ACC-OQ-033`. Terkait `ACC-DEC-051` yang menjadikan kejadian gagal sebagai penghalang tutup bulan |
| `ACC-DEC-047` | Decision | **Kejadian yang datang setelah periodenya ditutup dicatat pada periode terbuka berikutnya, dengan tanggal dokumen asli tetap disimpan.** Tanggal akuntansi jurnal memakai periode terbuka; tanggal dokumen memakai tanggal kejadian sebenarnya. Periode yang sudah ditutup **tidak pernah dibuka kembali secara otomatis**. Menjawab `ACC-OQ-034` | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan A pada `ACC-OQ-034`. Sejalan `ACC-DEC-040` yang sudah memisahkan `AccountingDate` dan `DocumentDate` pada `AccJournal`, sehingga **tidak menuntut kolom baru** |
| `ACC-DEC-048` | Decision | **Isi minimum pesan kejadian keuangan mengikuti calon `ACC-PRD-001` §22 apa adanya**, yaitu sepuluh bidang: nomor kejadian, jenis kejadian, modul asal, nomor transaksi asal, waktu kejadian, tanggal akuntansi, nilai, mata uang, penanda urutan, dan kunci anti-ganda. Pengirim wajib mengisi kesepuluhnya; pesan dengan bidang wajib kosong ditolak. Menjawab `ACC-OQ-023` | Rizki, owner Finance | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan A pada `ACC-OQ-023`. Cocok dengan `ACC-DEC-035` yang menuntut dua kunci anti-ganda, dan dengan `ACC-DEC-020` yang mengunci mata uang rupiah. **DIPERLUAS `ACC-DEC-060`** 9 September 2026 menjadi **dua belas** bidang, bertambah `CorrelationId` dan `CausationId`. Keputusan ini tidak dibatalkan — kesepuluh bidang aslinya tetap wajib |
| `ACC-DEC-049` | Decision | **Kejadian yang gagal diproses dicoba ulang 3 kali dengan jeda yang makin panjang, lalu masuk daftar gagal.** Sesudah percobaan ketiga gagal, kejadian berhenti dicoba dan **Accounting Manager diberi tahu**. Daftar gagal menyediakan tombol coba ulang manual. Tidak ada percobaan ulang tanpa batas. Menjawab `ACC-OQ-025` | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan A pada `ACC-OQ-025`, termasuk penetapan penerima pemberitahuan yang sebelumnya belum diputuskan |
| `ACC-DEC-050` | Decision | **Jurnal berulang dibuat otomatis sebagai `Draft`; pengesahannya tetap manual.** Berlaku untuk penyusutan bulanan, sewa dibayar di muka, dan sejenisnya. Sistem tidak pernah mengesahkan jurnal berulang sendiri, sehingga kesalahan template ketahuan sebelum angkanya masuk buku besar. Menjawab `ACC-OQ-017` | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan A pada `ACC-OQ-017` |
| `ACC-DEC-051` | Decision | **Hanya dua hal yang menghalangi penutupan bulan: jurnal yang belum disahkan, dan kejadian keuangan yang gagal diproses.** Lima calon lainnya — jurnal belum seimbang, integrasi belum cocok, penyusutan belum dijalankan, saldo di akun sementara, dan selisih saldo awal dengan saldo akhir — muncul sebagai **peringatan** yang boleh dilewati. Menjawab `ACC-OQ-018`. **DIPERLUAS `ACC-DEC-065`** 9 September 2026 menjadi **tiga** penghalang, bertambah shift kasir yang belum ditutup. **DIPERLUAS `ACC-DEC-070`** 10 September 2026 menjadi **enam** peringatan, bertambah kejadian keuangan tertahan. **DIKURANGI `ACC-DEC-077`** 14 September 2026 menjadi **lima** peringatan: saldo di akun sementara dicabut. **DIPERLUAS `ACC-DEC-076`** 14 September 2026 menjadi **empat** penghalang, bertambah rekonsiliasi control account | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan A pada `ACC-OQ-018`. Catatan: jurnal belum seimbang tidak perlu menjadi penghalang tersendiri karena `ACC-DEC-025` sudah melarangnya diajukan maupun disahkan |
| `ACC-DEC-052` | Decision | **Penutupan periode memerlukan persetujuan tertulis di dalam sistem.** Accounting Manager mengajukan penutupan, pimpinan keuangan menyetujuinya, dan keduanya tercatat beserta waktunya. Konsekuensinya penutupan menjadi dua langkah, dan wajib ada penyetuju pengganti saat pimpinan keuangan tidak bertugas. Menjawab `ACC-OQ-019` | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan A pada `ACC-OQ-019`. Sejalan `ACC-DEC-016` (penyetuju bukan pembuat) dan `ACC-DEC-026` (hanya Accounting Manager yang menutup periode) |
| `ACC-DEC-053` | Decision | **Jurnal penutup tahun dihitung dan disusun sistem, pengesahannya manual.** Pada akhir tahun sistem menolkan saldo seluruh akun pendapatan dan beban, menghitung selisihnya, lalu menyusun jurnal penutup sebagai `Draft`. Accounting Manager yang mengesahkannya. Konsekuensinya perlu satu jenis jurnal baru khusus penutup tahun. Menjawab `ACC-OQ-020` | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Pilihan A pada `ACC-OQ-020` |
| `ACC-DEC-054` | Decision | **Seluruh selisih pendapatan dikurangi beban masuk ke SATU akun laba ditahan, tanpa pembagian ke akun lain lebih dahulu.** Akun laba ditahan berada di kelompok `3` Ekuitas. **Kode akun pastinya ditentukan pemilik proses akuntansi saat mengisi daftar akun**, dan itu keputusan pengisian data, bukan keputusan rancangan. Bila kelak ada aturan pembagian laba dari pemegang saham, pembagiannya dibuat sebagai jurnal manual terpisah sesudah jurnal penutup disahkan | Rizki | `approved` | Rizki, 8 September 2026 | Keputusan owner, 8 September 2026. Turunan `ACC-DEC-053`, menutup pertanyaan tambahan yang tertulis pada `ACC-OQ-020` — akun laba ditahan mana yang dipakai dan apakah ada pembagian sebelumnya |


### Keputusan turunan gerbang kelengkapan — 8 September 2026

Tiga baris berikut menutup gap yang ditemukan `requirement-completeness-gate` pada hari yang
sama. Buktinya ada di [evidence/08-phase2-requirement-completeness-gate.md](evidence/08-phase2-requirement-completeness-gate.md).

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-055` | Decision | **Peran Accounting bertambah menjadi TUJUH; peran ketujuh adalah `Accounting Director`.** Peran ini menyandang **satu** hak akses saja, yaitu `Period : Approve` — menyetujui penutupan periode. Hak `Period : Close` tetap milik Accounting Manager. Dengan begitu yang mengajukan penutupan bukan yang menyetujuinya, sejalan prinsip empat mata `ACC-DEC-016`. Peran ini juga menjadi padanan resmi istilah "pimpinan keuangan" pada `ACC-DEC-033` (pengesahan saldo awal) dan `ACC-DEC-052` (penutupan periode). `ACC-DEC-031` **diperluas**, bukan dibatalkan. Menutup `DEC-ACC-P2-001` | Rizki | `approved` | Rizki, 8 September 2026 | Gap ditemukan `requirement-completeness-gate` 8 September 2026: `ACC-DEC-052` memakai istilah "pimpinan keuangan" yang tidak ada padanannya di enam peran `ACC-DEC-031` maupun di `ACC-PERMISSION-0.3`, sehingga tombol Setujui Penutupan tidak punya pemilik yang dapat ditegakkan. `ACC-PERMISSION` naik `0.3` → `0.4` |
| `ACC-DEC-056` | Decision | **Kotak masuk kejadian keuangan Phase 2 TIDAK menyimpan pengenal pasien.** Yang disimpan hanya **modul asal** dan **nomor transaksi asal**; nama pasien, nomor rekam medis, dan nomor kunjungan **dilarang** disimpan di seluruh tabel Accounting. Penelusuran ke pasien dilakukan dengan membuka modul asalnya memakai nomor transaksi tersebut. Akibatnya Accounting tetap **nol kolom data pribadi** persis seperti MVP, dan layar Kejadian Gagal menampilkan nomor transaksi, bukan nama pasien. Menutup `DEC-ACC-P2-004` | Rizki | `approved` | Rizki, 8 September 2026 | Gap ditemukan `requirement-completeness-gate` 8 September 2026: Phase 2 adalah fase pertama yang menerima data berasal dari tagihan pasien, tetapi batas penyimpanan pengenal pasien belum pernah dinyatakan tegas. Meneruskan `02-backend-architecture.md` bagian 11 dan `ACC-DEC-004` ke Phase 2 |
| `ACC-DEC-057` | Decision | **Pemberitahuan kejadian gagal memakai penanda jumlah pada menu, ditambah catatan `LoggerService`.** Menu Kejadian Gagal membawa angka jumlah kejadian yang belum ditangani, dan setiap kegagalan dicatat lewat `LoggerService` yang sudah dipakai seluruh modul. **Tidak ada** pengiriman surel dan **tidak ada** SignalR Hub baru pada rilis pertama Phase 2. Menutup `DEC-ACC-P2-003` | Rizki | `approved` | Rizki, 8 September 2026 | Diperiksa langsung pada `02c3219`: nol layanan pemberitahuan umum di repository; satu-satunya Hub adalah `Hubs/QueueHub.cs` untuk antrian, dan nol berkas bernama `*Notif*`. Melengkapi `ACC-DEC-049` yang menetapkan penerimanya tetapi bukan salurannya |


### Keputusan koreksi rancangan — 8 September 2026

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-058` | Decision | **Aturan posting berbentuk DAFTAR BARIS, bukan sepasang akun.** `AccPostingRule` menjadi induk yang membawahi `AccPostingRuleLine`, meniru pola `AccJournal` dan `AccJournalLine` yang sudah terbukti. Satu aturan karena itu dapat menghasilkan tiga baris atau lebih. Konsekuensi yang mengikat: **pesan kejadian keuangan dapat membawa rincian nilai** (*komponen*) di samping nilai totalnya, dan setiap baris aturan menunjuk komponen mana yang dipakainya. Tanpa rincian itu, aturan berbaris banyak tidak tahu angka mana untuk baris mana. Kejadian yang membawa komponen tanpa baris aturan yang cocok diperlakukan sama seperti kejadian tanpa pemetaan: **Tertahan** (`ACC-DEC-046`) | Rizki | `approved` | Rizki, 8 September 2026 | Ditemukan saat peninjauan rancangan terhadap alur akuntansi rumah sakit, 8 September 2026. Bentuk sepasang akun tidak dapat mengungkapkan tiga hal yang lazim di rumah sakit: **jasa medis dokter** (1 debit 2 kredit), **potongan penjualan** (2 debit 1 kredit), dan **HPP farmasi bersamaan dengan pendapatannya**. Potongan penjualan bahkan tidak dapat dipecah menjadi dua kejadian seimbang tanpa akun perantara, sementara akun perantara dilarang `ACC-DEC-046`. Menaikkan `ACC-API` `0.6` → `0.7`, `ACC-VALIDATION` `0.4` → `0.5` |


### Ratifikasi lintas modul — 9 September 2026

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-059` | Decision | **`ACC-DEC-044` DIKONFIRMASI owner Billing.** Rantainya Billing → Finance → Accounting, dan **Finance menerbitkan kejadian keuangan tersendiri**, bukan meneruskan `BilArHandoff`. Tiga hal ikut dikunci: (1) `ACKNOWLEDGED` pada handoff Billing berarti **Finance sudah berhasil mencatat AR/AP** — bukan sekadar mengetahui, dan **bukan** sudah dijurnal, sehingga status itu **bukan urusan Accounting**; (2) utang jasa dokter baru layak diakui pada `READY` + `ReadyAt`, bukan `CREATED`; (3) `BIL-INTEGRATION-0.4` **tidak berubah arah maupun bentuk dasarnya**. Akibatnya **Accounting DILARANG mengonsumsi `BilArHandoff` langsung** — karena Finance menerbitkan kejadian terpisah, membaca keduanya akan menghasilkan dua jurnal atas satu tagihan | Owner Billing, Rizki | `approved` **sisi Billing**; bentuk pesan Finance → Accounting **belum** diratifikasi owner Finance | Owner Billing, 9 September 2026 | Jawaban tertulis owner Billing atas enam pertanyaan [`evidence/10`](evidence/10-billing-arap-handoff-scan.md) bagian 8, dicatat apa adanya pada bagian 19. Menutup pilihan B dan C pada bagian 5 dengan bukti, bukan pendapat. **Tabel `BilArHandoff` diperiksa owner: sudah dibuat, masih kosong** — sehingga kekhawatiran handoff menumpuk tanpa konsumen gugur |


### Keputusan penelusuran lintas modul — 9 September 2026

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-060` | Decision | **Pesan kejadian keuangan bertambah dua bidang wajib menjadi DUA BELAS: `CorrelationId` dan `CausationId`.** Keduanya disimpan pada kotak masuk kejadian dan **diteruskan ke jurnal** sebagai penelusuran. Dengan begitu pertanyaan *"jurnal ini berasal dari tagihan pasien yang mana"* terjawab dari satu tempat, tanpa membuka modul Finance. Menutup `DEC-ACC-P2-010`. Menaikkan `ACC-API` `0.7` → `0.8` dan `ACC-VALIDATION` `0.5` → `0.6` | Rizki | `approved` **sisi Accounting**; bentuk pesan tetap menunggu ratifikasi owner Finance | Rizki, 9 September 2026 | Dipicu jawaban owner Billing nomor 4, 9 September 2026: kejadian Finance *"tetap membawa correlation/causation ke source Billing"*. Tanpa kedua bidang itu, `SourceTransactionId` hanya berisi nomor pencatatan AR milik Finance, sehingga penelusuran ke faktur Billing **terputus di Finance** — padahal Finance sudah bersedia membawakan penghubungnya. Rinciannya di [`evidence/10`](evidence/10-billing-arap-handoff-scan.md) bagian 21.2 |


### Keputusan pola subledger — 9 September 2026

Tiga keputusan lahir dari jawaban owner Billing atas pertanyaan 7–12.

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-061` | Decision | **Finance adalah penerbit TUNGGAL seluruh kejadian keuangan ke Accounting**, memperluas `ACC-DEC-044` yang semula hanya menyebut tagihan pasien. Billing menerbitkan **fakta operasional** ke Finance, bukan jurnal, dan **tidak** menerbitkan langsung ke Accounting. Berlaku juga untuk penghapusan piutang: **Billing tetap pemilik alur persetujuannya**, sementara **Finance** yang mengurangi saldo AR dan menerbitkan kejadiannya. Ditegaskan pula bahwa **Accounting DILARANG membaca tabel modul lain** — penambahan handoff/event adalah satu-satunya jalur yang sah. **Tidak semua perubahan status menjadi jurnal**; Finance yang menentukan mana yang menjadi kejadian keuangan | Owner Billing, Rizki | `approved` sisi Billing | Owner Billing, 9 September 2026 | Jawaban 7, 8, dan 11 atas pertanyaan bagian 17, dicatat pada [`evidence/10`](evidence/10-billing-arap-handoff-scan.md) bagian 23. **Menyederhanakan** rancangan: kotak masuk kejadian hanya melayani satu sumber. Mengoreksi dugaan kami bahwa sepuluh dari sebelas peristiwa milik Billing/Kasir |
| `ACC-DEC-062` | Decision | **Accounting memakai pola SUBLEDGER dan CONTROL ACCOUNT.** Buku besar memegang **saldo ringkas**; rincian tiap transaksi, tender, dan voucher tetap tinggal di subledger modul asalnya untuk keperluan audit. Dua penerapannya: (1) **kas diringkas per shift kasir** — satu kejadian per shift, bukan per transaksi, sehingga `SourceTransactionId` berisi nomor shift; (2) **kas kecil punya control account sendiri**, terpisah dari Kas Kasir, dan dampak finansialnya tetap masuk buku besar. Akibatnya `Kas Kasir` dan `Kas Kecil` adalah **control account** yang saldonya wajib cocok dengan subledger masing-masing, dan ketidakcocokan itu yang dicari saat rekonsiliasi | Owner Billing, Rizki | `approved` sisi Billing | Owner Billing, 9 September 2026 | Jawaban 9 dan 12. Menurunkan volume jurnal dari puluhan ribu baris per bulan menjadi beberapa baris per hari, dengan penelusuran tetap terjaga lewat nomor shift. Menjawab pula kepedulian rekonsiliasi yang dicatat 8 September. Sejalan `PC-DEC-001` milik Billing yang menyatakan kas kecil dan kas fisik shift kasir adalah dua uang berbeda |
| `ACC-DEC-063` | Decision | **Selisih kas kasir dijurnal saat `REVIEWED`, bukan saat `CLOSED_WITH_VARIANCE`.** `CLOSED_WITH_VARIANCE` berarti selisihnya baru terdeteksi dan sebabnya belum diketahui; menjurnalnya saat itu berarti mengakui beban yang mungkin ternyata hanya salah hitung, lalu harus dibalik. `REVIEWED` berarti disposisinya sudah disahkan, sehingga yang masuk buku besar sudah berupa keputusan. Kejadian deteksi tetap boleh dikirim sebagai **pemberitahuan tanpa jurnal** | Rizki | `approved` | Rizki, 9 September 2026 | Jawaban 10 menawarkan dua bentuk — dua kejadian, atau satu jurnal saat `REVIEWED`. Dipilih yang kedua karena lebih sederhana dan tidak menaruh dugaan di buku besar. Sejalan `ACC-DEC-046` yang melarang buku besar berisi tebakan |


### Keputusan control account — 9 September 2026

Tiga keputusan owner sesudah membaca dampak jawaban owner Billing terhadap Phase 1 dan Phase 2.

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-064` | Decision | **Control account DIKUNCI dari jurnal manual umum.** Cakupannya empat kelompok akun: **Kas Kasir, Kas Kecil, Piutang, dan Hutang**. Pencatatan ke keempatnya hanya sah lewat kejadian akuntansi atau subledger, **bukan** lewat layar Jurnal Manual. Jurnal manual **tetap diperbolehkan** untuk akun non-control — penyesuaian, akrual, koreksi beban, dan jurnal penutup. Konsekuensi teknis: `AccChartOfAccount` bertambah satu kolom penanda, dan `AccJournalService` menolak baris jurnal manual yang menunjuk akun bertanda itu. **Ini perubahan pada artefak Phase 1**, dikerjakan sebagai bagian Phase 2 | Rizki | `approved` | Rizki, 9 September 2026 | Turunan `ACC-DEC-062`. Tanpa penguncian ini, satu jurnal manual ke `Kas Kasir` langsung membuat saldo buku besar tidak cocok dengan subledger kasir — **tanpa error apa pun**, hanya angkanya berselisih saat rekonsiliasi. Penanda disimpan sebagai kolom karena **tidak dapat diturunkan** dari data lain: `Kas Kasir` dan `Piutang` sama-sama berjenis `Asset`, tetapi tidak setiap akun `Asset` adalah control account — berbeda dari `ACC-DEC-019` yang menolak kolom `RequiresCostCenter` justru karena dapat diturunkan dari `AccountType` |
| `ACC-DEC-065` | Decision | **Shift kasir yang belum ditutup MENJADI PENGHALANG penutupan periode**, sehingga penghalang tutup bulan bertambah dari dua menjadi **tiga**. Karena Accounting **tidak boleh** membaca tabel Finance maupun Billing (`ACC-DEC-061`), penegakannya memakai kejadian: Finance/Kasir wajib menerbitkan kejadian **`CASH_SHIFT_CLOSED`**, dan Accounting hanya **memvalidasi keberadaan kejadian itu** sebelum penutupan diizinkan. Accounting tidak pernah menanyakan keadaan shift langsung ke modul asalnya. Memperluas `ACC-DEC-051` | Rizki | `approved` | Rizki, 9 September 2026 | Celah ditemukan 9 September 2026 saat menilai dampak `ACC-DEC-062`: karena kas datang **per shift**, shift yang melewati pergantian bulan dan belum ditutup membuat kasnya tidak pernah sampai ke buku besar — dan periode tetap dapat ditutup dengan angka kas yang belum lengkap. Bentuk penegakan lewat kejadian menjaga batas `ACC-DEC-061` tetap utuh |
| `ACC-DEC-066` | Decision | **Rekonsiliasi control account masuk Phase 2, bukan penghalang Phase 1.** Isinya tiga: **perbandingan saldo buku besar**, **perbandingan saldo subledger**, dan **laporan selisih**. Lingkup Phase 1 **tidak berubah** dan tetap: COA, Jurnal, Posting, Buku Besar, dan Neraca Saldo — menegaskan `ACC-DEC-030` tanpa mengubahnya | Rizki | `approved` | Rizki, 9 September 2026 | Turunan `ACC-DEC-062`: pola control account baru bermakna bila ada yang membandingkan saldo buku besar dengan total subledger. Sebelumnya kemampuan ini tidak ada di roadmap mana pun. **Satu pertanyaan turunan masih terbuka** — dari mana Accounting memperoleh saldo subledger, mengingat ia dilarang membaca tabel modul lain. Dicatat sebagai `DEC-ACC-P2-011` |
| `ACC-DEC-067` | Decision | **Jurnal penutup tahun `JT` DITERIMA periode `SoftClosed`.** Masa tenggang tutup buku kini menerima tiga jenis jurnal, bukan dua: penyesuaian `JP`, pembalikan `JB`, dan penutup tahun `JT`. Periode `Closed` tetap menolak semuanya. Menaikkan `ACC-STATE` `0.2` → `0.3` pada bagian 2.2 | Rizki | `approved` | Rizki, 10 September 2026 | Ditemukan saat mengerjakan `BE-ACC-P2-010`. Tanpa keputusan ini tutup tahun **mustahil dijalankan**, dan mustahilnya diam-diam: `ACC-VALIDATION-0.6` bagian 5 menuntut seluruh periode tahun itu sudah tertutup sebelum jurnal penutup boleh disusun, sementara tanggal akuntansi jurnal penutup wajib berada **di dalam** tahun yang ditutup — yaitu tepat pada periode yang barusan ditutup itu. Akibatnya jurnal penutup lahir sebagai draft yang **tidak pernah dapat diajukan maupun disahkan**, karena syarat ke-9 `ACC-STATE-0.1` bagian 1.3 memeriksa aturan yang sama saat pengajuan dan pengesahan. Menuntut `Closed` justru mengunci selamanya: periode tutup permanen tidak menerima jurnal apa pun, termasuk jurnal penutupnya sendiri. Bukti: [`be-acc-p2-010`](task/report/backend/be-acc-p2-010-pratinjau-dan-penyusunan-jurnal-penutup-tahun.md) |
| `ACC-DEC-068` | Decision | **Koreksi sesudah jurnal penutup tahun disahkan memakai jalur pembalikan yang sudah ada**, tanpa mekanisme "buka kembali tahun buku". Urutannya tiga langkah: (1) balik jurnal penutupnya lewat `POST /journals/{id}/reverse` — menuntut persetujuan baru (`ACC-DEC-029`); (2) buka kembali periode Desember beserta alasan tertulis (`ACC-DEC-027`), masukkan jurnal yang terlewat, lalu tutup lagi; (3) susun ulang jurnal penutupnya. **Menutup `DEC-ACC-P2-006`** | Rizki | `approved` | Rizki, 10 September 2026 | Menegaskan `ACC-DEC-006`: riwayat yang sudah disahkan permanen, koreksi berarti **menambah** jurnal, bukan mengubah yang lama. Tahun buku karena itu tidak butuh lifecycle sendiri — sejalan `02-backend-architecture.md` yang menolak aggregate `AccFiscalYear`. Sudah terbukti berjalan pada acceptance (6) `BE-ACC-P2-010`: sesudah pembalikan disahkan, saldo keempat akun kembali persis ke angka sebelum ditutup dan laba ditahan kembali nol |
| `ACC-DEC-069` | Decision | **Ratifikasi tiga delta implementasi yang selama ini menggantung.** (1) `AccPeriodClosingApproval` memakai `Restrict`, **bukan** `Cascade` seperti tulisan kamus data bagian 16 — riwayat persetujuan adalah bukti audit dan tidak boleh ikut terhapus bersama periodenya; kamus data yang disesuaikan, bukan kodenya. (2) Pengaturan akuntansi menolak `422` pada **dua** keadaan tambahan di luar tulisan kartu: akun laba ditahan milik badan hukum lain, dan akun laba ditahan yang sudah tidak aktif. (3) `GET /configuration/{legalEntityId}` menjawab `200` ber-`isConfigured: false` saat pengaturan belum ada, **bukan** `404` | Rizki | `approved` | Rizki, 10 September 2026 | Ketiganya menutup kesalahan yang **tidak menimbulkan error**. Akun badan hukum lain membuang laba ke buku besar orang lain (`ACC-DEC-037`) dengan jurnal yang tetap seimbang; akun nonaktif membuat tutup tahun gagal tepat di akhir tahun buku, saat paling mahal diperbaiki; `Cascade` menghapus jejak siapa menyetujui penutupan periode tanpa satu pun peringatan. Butir (3) membedakan "belum diisi" dari "gagal dibaca" — belum diisi adalah keadaan wajar bagi rumah sakit yang baru memakai modul ini. Bukti: [`be-acc-p2-001`](task/report/backend/be-acc-p2-001-entity-dan-enum-penutupan-periode.md), [`be-acc-p2-009`](task/report/backend/be-acc-p2-009-endpoint-pengaturan-akuntansi.md) |
| `ACC-DEC-070` | Decision | **Kejadian keuangan tertahan menjadi peringatan keenam pada daftar periksa penutupan.** `ACC-DEC-051` menyebut lima peringatan; `AccPeriodClosingService` sejak awal mengembalikan **enam**, yang keenam berkode `HELD_EVENTS`. **Yang benar adalah kode** — keputusan ini yang menyesuaikan, bukan kodenya yang dicabut. Kejadian tertahan tetap **peringatan**, bukan penghalang: ia tidak menahan penutupan. Memperluas `ACC-DEC-051` tanpa mengubah filosofinya | Rizki | `approved` | Rizki, 10 September 2026 | Kejadian tertahan adalah kejadian keuangan yang **tidak pernah menjadi jurnal**, jadi saat tutup bulan ia berarti transaksi yang belum sampai ke buku besar — persis kelas hal yang menjadi tugas daftar periksa penutupan untuk memunculkan. Mencabutnya dari kode akan membuang sinyal yang benar. Tetap peringatan dan bukan penghalang karena ia menunggu **pekerjaan pemetaan yang wajar dijadwalkan**, bukan gangguan yang menuntut tindakan segera — alasan yang sama dipakai `ACC-DEC-057` untuk tidak menghitungnya pada penanda angka di menu. Menutup `ACC-GAP-012`. Bukti: `AccPeriodClosingService.cs` daftar `peringatan`, enam butir |
| `ACC-DEC-071` | Decision | **Rekonsiliasi control account memakai saldo subledger yang diterbitkan Finance per periode akuntansi, bukan API pull.** Menutup `DEC-ACC-P2-011` dengan memilih kemungkinan (a). Enam ketetapannya: **(1)** rekonsiliasi dilakukan pada **cut-off periode akuntansi**, bukan langsung/live. **(2)** Finance adalah penerbit saldo subledger **final** untuk setiap control account pada periode itu. **(3)** Kejadian yang diterbitkan memuat sekurang-kurangnya `LegalEntity`, `AccountingPeriod`, `ControlAccount`, `SubledgerBalance`, dan `AsOfDate`. **(4)** Accounting **tidak** membaca tabel Finance maupun Billing, dan **tidak** melakukan API pull ke Finance — menegaskan `ACC-DEC-061`. **(5)** Accounting membandingkan saldo subledger yang diterima dengan saldo buku besar pada periode/cut-off yang sama, lalu mencatat selisihnya sebagai hasil rekonsiliasi. **(6)** Rekonsiliasi menjadi **bagian proses penutupan periode**: bila saldo subledger belum diterima, atau hasilnya belum cocok, penutupan **tidak boleh dianggap selesai**. Model live/API pull (kemungkinan b) **tidak dipakai pada Phase 2** | Rizki | `approved` | Rizki, 10 September 2026 | Dipilih karena menjaga arah aliran tetap satu — Finance ke Accounting — sehingga tidak ada pola integrasi baru yang perlu diamankan, dan menumpang kotak masuk kejadian yang memang sudah direncanakan gelombang `P2-1`. Kelemahan "potret basi" pada penerbitan berkala **tidak berlaku** di sini karena saldonya terikat pada periode, bukan pada waktu: rekonsiliasi periode yang sudah ditutup memberi jawaban yang sama selamanya, dan itulah yang dicari auditor. Konsekuensinya terhadap daftar periksa penutupan dicatat pada `ACC-GAP-013` |


### Keputusan penegakan control account — 11 September 2026

Dua keputusan owner yang dibutuhkan `BE-ACC-P2-012` sebelum kodenya boleh ditulis. Keduanya
menutup jalan memutar di sekitar `ACC-DEC-064`: jalur koreksi dan jalur template, yang keduanya
menyusun baris jurnal tanpa melewati layar Jurnal Manual.

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-072` | Decision | **Koreksi jurnal Disahkan yang menyentuh control account: pembalikan penuh `JB` DIBOLEHKAN, jurnal penyesuaian `JP` yang barisnya menunjuk control account DITOLAK.** **(1)** `POST /journals/{id}/reverse` dengan `CorrectionType = FullReversal` boleh membalik jurnal yang barisnya menyentuh control account. Barisnya disalin terbalik dari jurnal asal, sehingga pembalikan **tidak dapat memasukkan akun yang tidak ada di jurnal asal**, dan hasil bersihnya justru mengembalikan saldo control account ke keadaan sebelum jurnal asal. Tetap menuntut persetujuan baru (`ACC-DEC-029`). **(2)** `CorrectionType = Adjustment` ditolak `422` bila **satu saja baris penyesuaiannya** menunjuk akun ber-`IsControlAccount = true`, **terlepas dari isi jurnal asalnya**. Sebaliknya, penyesuaian atas jurnal asal yang menyentuh control account tetap boleh selama barisnya sendiri hanya menyentuh akun non-control — misalnya memindahkan pendapatan yang salah akun. **(3)** Pengecualian butir (1) dikenali dari **asal-usulnya** — jurnal lahir dari `ReverseAsync`, `ReversalOfJournalId` terisi, dan barisnya belum diubah manusia — **bukan dari kode jenis `JB`**. Jurnal pembalik yang barisnya diubah lewat Form Jurnal sesudah terbit diperiksa seperti jurnal manual. **(4)** Koreksi **nilai** control account yang sesungguhnya — bukan pembatalan — tetap hanya lewat kejadian dari Finance. Sebelum Finance berdiri, satu-satunya koreksi atas jurnal semacam itu adalah balik penuh. Menutup keputusan terbuka pertama `BE-ACC-P2-012` | Rizki | `approved` | Rizki, 11 September 2026 | Baris penyesuaian **diketik bebas** oleh petugas (`ReverseJournalRequest.AdjustmentLines`, `AccJournalService.ReverseAsync`) dan boleh dibuat atas jurnal Disahkan **mana pun**. Tanpa butir (2), jurnal Disahkan apa saja dapat "disesuaikan" dengan baris debit Kas Kasir, dan `ACC-DEC-064` menjadi percuma. Butir (3) wajib karena **Form Jurnal menerima jenis jurnal apa pun**: diperiksa di `QuilvianNewDevRizki` 11 September 2026, satu-satunya jurnal di sana, `JB/2026/09/00001`, berjenis Jurnal Pembalik tetapi `ReversalOfJournalId`-nya kosong — dibuat lewat Form Jurnal. Pengecualian menurut kode jenis dengan demikian dapat diakali siapa pun yang memilih jenis `JB` di layar. Pada tanggal yang sama nol jurnal menyentuh control account, sehingga keputusan ini tidak mengunci data yang sudah ada |
| `ACC-DEC-073` | Decision | **Template jurnal berulang yang barisnya menunjuk control account DITOLAK saat disimpan dan saat diaktifkan — bukan saat diterbitkan.** **(1)** `POST` dan `PUT /recurring-journals` menolak `422` bila ada baris template yang menunjuk akun ber-`IsControlAccount = true`, dan pesannya menyebut akun mana. **(2)** `PATCH /recurring-journals/{id}/activate` menjalankan pemeriksaan yang sama, sehingga template yang disimpan **sebelum** akunnya ditandai control tertangkap saat hendak dihidupkan. **(3)** Penerbitan — `POST /{id}/generate` maupun penjadwal — **tidak** memeriksa ulang. Draft hasil template diperlakukan sebagai jalur otomatis dan lolos pemeriksaan `BE-ACC-P2-012` saat diajukan. **(4)** Draft hasil template yang barisnya **diubah lewat Form Jurnal** diperiksa seperti jurnal manual. **(5)** Mengoreksi `ACC-VALIDATION-0.6` bagian 3b yang menyebut template berulang sebagai jalur sah menuju control account — itu bertentangan dengan `ACC-DEC-064`, yang hanya mengizinkan kejadian akuntansi atau subledger. **Sisa risiko yang diterima:** template yang **sudah aktif** lalu salah satu akunnya baru ditandai control tetap terbit sampai dinonaktifkan atau diubah. Menutup keputusan terbuka kedua `BE-ACC-P2-012` | Rizki | `approved` | Rizki, 11 September 2026 | `AccRecurringJournalService` tidak memeriksa `IsControlAccount` sama sekali — di seluruh `Areas/Corporate/AccountingManagement/` penanda itu hanya dibaca layanan rekonsiliasi dan daftar akun. Tanpa keputusan ini, siapa pun yang berhak membuat template dapat menjurnal "debit Kas Kasir" setiap bulan tanpa kejadian apa pun. Ditolak di hulu, bukan saat terbit, karena saat simpan dan aktifkan **ada manusia yang membaca pesan penolakannya**; penolakan saat terbit terjadi di penjadwal, tanpa penonton, dan hanya berujung draft yang tidak pernah muncul. Menyentuh kode `BE-ACC-P2-007`. Diperiksa 11 September 2026: nol template di `QuilvianNewDevRizki`, sehingga tidak ada template lama yang terkunci |

### Keputusan owner hasil review "rencana sampai 100%" — 14 September 2026

Delapan keputusan berikut diambil Rizki sesudah review kesiapan modul yang membandingkan PRD,
roadmap, dan source (`rizkiG` `b3ab542e`, `RizkiV2` `f6b1498fe`). Nomor `OD-ACC-##` adalah nomor
pertanyaan pada review itu; nomor resminya `ACC-DEC-###`.

**Yang sengaja TIDAK diputuskan dan tetap terbuka:** `OD-ACC-01` (boleh tidaknya kode kotak masuk
kejadian dibangun sebelum ratifikasi Finance), `OD-ACC-04` (bentuk final pesan kejadian),
`OD-ACC-05` (autentikasi penerbit), `OD-ACC-06` = `DEC-ACC-P2-002` (daftar jenis kejadian),
`OD-ACC-07` (deteksi shift kasir terbuka), `OD-ACC-08` (bentuk kejadian saldo subledger), definisi
peringatan `DEPRECIATION_NOT_RUN` dan `OPENING_CLOSING_MISMATCH`, serta ratifikasi lintas modul
`ACC-XM-001`. Seluruhnya menyangkut Finance, Kasir, atau definisi bisnis yang belum ada.

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-074` | Decision | **Jurnal hasil kejadian akuntansi memakai jenis jurnal yang ditetapkan pada aturan posting, dan pelaku otomatisnya diambil dari konfigurasi.** (1) `AccPostingRule` bertambah kolom `JournalTypeId` — setiap aturan menyebut jenis jurnal yang dihasilkannya, sama seperti `AccRecurringJournalTemplate.JournalTypeId`. (2) Jurnal yang dibuat mesin posting mencatat pelaku dari konfigurasi `SystemActorUserId`, mengikuti pola `AccRecurringJournalSchedulerOptions.SystemActorUserId`. **Contoh:** aturan untuk jenis kejadian pengakuan piutang menunjuk jenis jurnal `JU`, sehingga jurnal yang terbentuk bernomor `JU/2026/10/00001` dan pembuatnya tercatat sebagai akun sistem, bukan petugas mana pun. Menjawab `OD-ACC-02` | Rizki | `approved` | Rizki, 14 September 2026 | Review 14 September 2026 menemukan `AccJournal.JournalTypeId` wajib diisi, sementara rancangan `AccPostingRule` dan `AccAccountingEvent` tidak punya kolom jenis jurnal sama sekali — mesin posting tidak akan tahu jenis jurnal apa yang harus dibuat. Diwujudkan `BE-ACC-P2-015` |
| `ACC-DEC-075` | Decision | **Kejadian berjenis yang belum dikenal TIDAK dibuang.** (1) `AccAccountingEvent.EventTypeId` boleh kosong. (2) Kode jenis asli dari pesan disimpan pada kolom baru `EventTypeCode`. (3) Kejadian semacam itu berstatus **Tertahan**, sama seperti kejadian yang jenisnya dikenal tetapi belum punya aturan posting. (4) Kunci anti-ganda kedua **tidak boleh** bergantung hanya pada FK `EventTypeId` — ia memakai kode jenis yang tersimpan, sehingga kejadian yang sama tetap tertangkap sebagai kiriman ulang walaupun jenisnya baru didaftarkan belakangan. **Contoh:** Finance mengirim kejadian berkode `PENGHAPUSAN-PIUTANG` sebelum kode itu didaftarkan. Kejadian tersimpan Tertahan dengan `EventTypeId` kosong; begitu jenisnya ditambahkan dan kejadian diproses ulang, jurnalnya terbentuk. **Implementasi kotak masuk kejadian belum dilakukan** — keputusan ini hanya mengunci bentuknya. Menjawab `OD-ACC-03` | Rizki | `approved` | Rizki, 14 September 2026 | Review menemukan pertentangan: `ACC-VALIDATION` Phase 2 bagian 1 menyatakan jenis tak terdaftar ditahan (`422`), sedangkan kamus data bagian 9 mewajibkan FK `EventTypeId` — kejadian semacam itu mustahil disimpan |
| `ACC-DEC-076` | Decision | **Rekonsiliasi control account menjadi penghalang keempat penutupan periode, berbentuk butir baru ber-`State = Evaluated`, dengan toleransi selisih nol.** Penutupan ditahan bila saldo subledger belum lengkap **atau** tidak cocok dengan saldo buku besar. Karena butirnya `Evaluated`, mesin `CanSubmitClosing` yang sudah ada langsung menahannya tanpa perubahan mesin. Toleransi nol mengikuti `NFR-008`. **Contoh:** saldo Kas Kasir di buku besar Rp 25.000.000 dan saldo subledger yang diterbitkan Finance Rp 24.999.500. Selisih Rp 500 menahan penutupan — tidak ada batas "cukup dekat". **Implementasi menunggu Wave D.** Menutup `ACC-GAP-013`. Menjawab `OD-ACC-09` | Rizki | `approved` | Rizki, 14 September 2026 | `ACC-DEC-071` butir 6 menuntut penutupan tidak dianggap selesai bila saldo subledger belum diterima atau belum cocok, tetapi bentuk penegakannya belum diputuskan (`ACC-GAP-013`, `03-frontend-architecture.md` bagian 11.3) |
| `ACC-DEC-077` | Decision | **Peringatan `SUSPENSE_ACCOUNT_BALANCE` DICABUT dari requirement.** Peringatan "saldo tertinggal di akun sementara" bertentangan dengan `ACC-DEC-046`, yang melarang akun sementara sama sekali — tidak ada akun sementara yang dapat bersaldo. `ACC-DEC-051` karena itu tinggal **lima** peringatan, bukan enam. Dua peringatan lain — `DEPRECIATION_NOT_RUN` dan `OPENING_CLOSING_MISMATCH` — **tetap menunggu definisi owner** dan tidak boleh diberi definisi karangan. Menjawab `OD-ACC-10` sebagian | Rizki | `approved` | Rizki, 14 September 2026 | `AccPeriodClosingService.cs` baris 169–174 pada `b3ab542e` memuat butir itu sebagai `NotYetAvailable` beralasan "penandaan akun sementara belum diputuskan" — tidak pernah dapat diperiksa. Pencabutan butir di source dikerjakan task tersendiri, bukan oleh keputusan ini |
| `ACC-DEC-078` | Decision | **Tiga pertanyaan penyempurnaan Phase 2 ditetapkan.** `DEC-ACC-P2-005` **CLOSED**: isi template jurnal berulang tetap **nominal tetap**, sesuai yang sudah dibangun `BE-ACC-P2-007`. `DEC-ACC-P2-007` **CLOSED**: status `Diabaikan` dipakai untuk kejadian Gagal dengan alasan tertulis wajib, sesuai `ACC-STATE` Phase 2 bagian 1 yang sudah `approved`. `DEC-ACC-P2-008` (deteksi aturan posting yang salah) **tetap OPEN** dan **tidak** memblokir Wave A. Menjawab `OD-ACC-11` | Rizki | `approved` | Rizki, 14 September 2026 | Review menemukan `DEC-ACC-P2-007` masih tercatat `OPEN` padahal `ACC-STATE` Phase 2 yang disetujui 8 September 2026 sudah memuat perpindahan `Gagal` → `Diabaikan` |
| `ACC-DEC-079` | Decision | **Penjaga jurnal penutup tahun ganda memakai advisory transaction lock PostgreSQL, tanpa kolom maupun perubahan skema.** Penyusunan jurnal penutup mengambil kunci per badan hukum dan tahun buku di dalam transaction, lalu memeriksa ulang keberadaan jurnal penutup sesudah kunci didapat — pola yang sama dengan penomoran jurnal dan pembalikan. **Contoh:** dua manajer menekan Susun Jurnal Penutup tahun 2026 pada detik yang sama. Permintaan pertama mendapat kunci dan membuat jurnal; permintaan kedua menunggu, lalu menemukan jurnal itu dan ditolak `409`. Menjawab `OD-ACC-13` | Rizki | `approved` | Rizki, 14 September 2026 | Kartu `BE-ACC-P2-010` mencatat dua permintaan bersamaan dapat lolos keduanya. Unique constraint tidak dapat diungkapkan pada skema `AccJournal` tanpa kolom baru, sehingga kunci adalah jalan yang tidak menyentuh skema. Diwujudkan `BE-ACC-P2-032` |
| `ACC-DEC-080` | Decision | **`ACC-GAP-010` diselesaikan di frontend memakai hook `usePermission` yang sudah ada, tanpa `AvailableActions` di backend.** Tombol Bangkitkan, Tutup, dan Buka Kembali pada layar Periode Akuntansi dijaga `AccountingPeriod : Create`, `AccountingPeriod : Close`, dan `AccountingPeriod : Reopen`. Tombol yang bukan hak pengguna **dimatikan, bukan disembunyikan**, dan diberi keterangan. Backend tetap menolak `403` sebagai pengaman sesungguhnya (`NFR-005`). Menjawab `OD-ACC-14` | Rizki | `approved` | Rizki, 14 September 2026 | Hook `src/lib/hooks/auth/use-permission.jsx` sudah dipakai layar Phase 2 (Daftar Periksa Penutupan, Tutup Tahun, Pengaturan Akuntansi) pada `RizkiV2` `f6b1498fe`. Diwujudkan `FE-ACC-P2-014` |
| `ACC-DEC-081` | Decision | **Automated test bukan acceptance criterion untuk pekerjaan Accounting pada branch developer.** Lima task backend yang sebelumnya `🟡` **hanya** karena test integrasi PostgreSQL belum dijalankan — `BE-ACC-P2-005`, `006`, `010`, `012`, dan `013` — dinyatakan **selesai (`✅`)**. Keputusan ini mengikuti arahan lead yang menghapus seluruh project test backend dari branch integration (`cefd927d`, 11 September 2026). Status UAT tetap terpisah dan **tidak** ditulis lulus. Menjawab `OD-ACC-15` | Rizki | `approved` | Rizki, 14 September 2026 | Kelima kartu pada `roadmap/backend-roadmap-phase2.md` menyebut test PostgreSQL sebagai satu-satunya butir yang belum terpenuhi. Acceptance criteria-nya sudah terpetakan ke source dan terbukti lewat uji manual/SQLite yang tercatat pada laporan task masing-masing |

Keputusan `ACC-DEC-001` sampai `ACC-DEC-008` **tidak dibuka kembali** sesuai PRD §36 aturan 3.

### Keputusan pasca-ratifikasi Finance — 24 September 2026

Modul Finance berdiri dan meratifikasi kontrak `ACC-XMOD-0.2` lewat `FIN-DEC-001`, 20 September
2026. Jawabannya atas enam pertanyaan paket
[`evidence/12`](evidence/12-paket-kontrak-kejadian-untuk-finance.md) ada di
`docs/module-blueprints/finance-management/evidence/01-jawaban-untuk-owner-accounting.md`. Sepuluh
keputusan berikut diambil Rizki dalam sesi `grill-me` 24 September 2026; setiap butir memilih opsi
yang direkomendasikan setelah pilihan dan konsekuensinya disajikan.

**Keadaan yang diverifikasi ke source hari itu** (`rizkiG` `b2b265af`): kode Finance berdiri di
`Areas/Corporate/FinanceManagement/AccountingIntegration/`; `FinAccountingEventOutbox` memuat kedua
belas bidang, dua unique index anti-ganda, dan status `HELD_FOR_FINALIZATION`; ke-17 kode kejadian di
source sama persis dengan dokumen Finance; `AccountingReceiptNumber` dan `AccountingJournalNumber`
disimpan sebagai `string?` maks 50; **pengirim ke Accounting belum dibangun**. Kotak masuk Accounting
masih nol kode; `ACC-TD-022` (bagan akun sah) masih `OPEN`.

**Pertentangan sumber yang dicatat, bukan diputuskan.** Tiga jawaban Finance yang sampai ke Rizki
lewat percakapan berbeda dengan decision log Finance. Dokumen jawaban Finance sendiri menyatakan
*"bila berbeda, decision log Finance yang berlaku"*, sehingga keputusan di bawah **tidak** bersandar
pada versi percakapan:

| Butir | Versi percakapan | Decision log Finance | Perlakuan di sini |
|---|---|---|---|
| Autentikasi | Akun layanan khusus + JWT Bearer berumur pendek, hanya `AccountingEvent : Receive` | `FIN-DEC-007` **`draft`**: service account/API key lewat mekanisme auth existing | Mekanisme tetap terbuka (`ACC-DEC-088`) |
| Saldo subledger | Amplop yang sama + empat rincian | `FIN-DEC-023` **`draft`**: lima bidang berdiri sendiri | Diputuskan sisi Accounting (`ACC-DEC-087`), Finance menyesuaikan |
| Cutover | 1 Oktober 2026 00.00 WIB | `FIN-DEC-008` **`approved`**: sejak go-live, tanggal "saat MVP-5 siap" | 1 Oktober dinyatakan tidak layak (`ACC-DEC-089`) |

Yasmin diminta mengonfirmasi atau mengubah log-nya — lihat
[`evidence/13`](evidence/13-balasan-accounting-untuk-finance.md).

| ID | Type | Isi keputusan | Owner | Status | Approved by | Evidence |
|---|---|---|---|---|---|---|
| `ACC-DEC-082` | Decision | **`ACC-XM-001` ditutup penuh.** Ketiga pihak sudah setuju: Accounting (`ACC-DEC-044`, 8 Sep), Billing (`ACC-DEC-059`, 9 Sep), Finance (`FIN-DEC-001`, 20 Sep). `ACC-XMOD` naik menjadi **`approved`** dengan `approved_by` Rizki dan Yasmin (lewat `FIN-DEC-001`). **`OD-ACC-01` gugur**: kotak masuk kejadian boleh dirancang dan dibangun sekarang, dan larangan menulis kode integrasi pada `integration-contract.md` bagian 5 dan 6 dicabut. Butir yang **belum** ikut diratifikasi — mekanisme autentikasi, bentuk saldo subledger, kode tambahan — tidak menahan penutupan ini; masing-masing hanya menahan bagiannya sendiri (lihat `ACC-DEC-087`..`090`). **Contoh akibat:** task pintu masuk `POST api/v1/corporate/accounting/accounting-events` boleh direncanakan dan dikerjakan pada Wave B tanpa menunggu keputusan Platform | Rizki | `approved` | Rizki, 24 September 2026 | `FIN-DEC-001`; `finance-management/evidence/01` bagian 2 pertanyaan 1–2; `finance-management/contracts/integration-contract.md` bagian 5 |
| `ACC-DEC-083` | Decision | **Ketujuh belas kode kejadian Finance diratifikasi** persis seperti `FIN-DEC-002`: `PENGAKUAN-PIUTANG`, `PENERIMAAN-PIUTANG`, `PENYESUAIAN-PIUTANG`, `PEMUTIHAN-PIUTANG`, `PENGAKUAN-HUTANG-SUPPLIER`, `PEMBAYARAN-HUTANG-SUPPLIER`, `PENGAKUAN-HUTANG-DOKTER`, `PEMBAYARAN-HUTANG-DOKTER`, `PENYESUAIAN-HUTANG`, `SETORAN-BANK`, `PETTY-CASH-TOP-UP`, `PETTY-CASH-DISBURSEMENT`, `PETTY-CASH-RETURN`, `PETTY-CASH-REVERSAL`, `PETTY-CASH-ADJUSTMENT`, `PENERIMAAN-KASIR`, `PEMBALIKAN-PENERIMAAN-KASIR`. `SourceModule` seluruhnya `Finance`. (1) Kode **tidak boleh** diubah atau ditambah sepihak; penambahan wajib lewat keputusan kedua pihak. (2) Ratifikasi kode **bukan** aturan posting: kejadian berkode sah tanpa aturan posting tetap **Tertahan** (`ACC-DEC-046`, `075`) sampai aturannya disusun di atas bagan akun yang sah (`ACC-TD-022`). (3) Data uji `PATIENT_PAYMENT` bersumber `CASHIER` di database pengembangan bertentangan dengan `ACC-DEC-044`; ia **dinonaktifkan** lewat layar master (`PATCH .../event-types/{id}/deactivate` setelah aturan posting ujinya dinonaktifkan), **bukan** dihapus lewat SQL, dan tidak boleh dipakai di lingkungan mana pun. Menutup `DEC-ACC-P2-002` / `OD-ACC-06` | Rizki | `approved` | Rizki, 24 September 2026 | `FIN-DEC-002`; `finance-management/contracts/integration-contract.md` bagian 5.4; konstanta `FinAccountingEventTypeCodes` di source Finance |
| `ACC-DEC-084` | Decision | **Kejadian dijurnal seketika di dalam request penerimaan, dengan penjadwal sebagai cadangan.** Urutannya: validasi pesan → simpan kejadian (`Diterima`) dan **commit** → cocokkan aturan posting → buat jurnal. Hasil yang mungkin: (a) jurnal terbentuk → `Terjurnal`, balasan `201` beserta nomor jurnal; (b) kode belum terdaftar, aturan belum ada, atau komponen tidak dikenal → `Tertahan`, balasan `422`; (c) gangguan teknis **setelah** kejadian tersimpan → kejadian tetap `Diterima`, balasan tetap `201` **tanpa** nomor jurnal, lalu `AccAccountingEventSchedulerHostedService` mencoba ulang sampai 3 kali sebelum `Gagal` (`ACC-DEC-049`). Kiriman ulang dijawab `200` dengan keadaan terkini, termasuk nomor jurnal bila sudah terbentuk. **Contoh:** `EVT-300` datang saat database jurnal sibuk; kejadian tersimpan, balasan `201` dengan `JournalNumber` kosong. Dua menit kemudian penjadwal berhasil membuat `JU/2026/11/00017`. Bila Finance mengirim ulang `EVT-300`, balasannya `200` dengan nomor jurnal itu. Menolak dua alternatif: **selalu antre** membuat `422` mustahil dijawab saat itu sehingga kewajiban Finance "Tertahan = `HELD`" tidak pernah terpicu; **semua atau batal** membuat kejadian yang gagal tidak pernah tersimpan di Accounting sehingga daftar gagal (`ACC-DEC-049/051/057`) kehilangan makna | Rizki | `approved` | Rizki, 24 September 2026 | `api-contract.md` grup Accounting Event (`201`/`200`/`422`); `02-backend-architecture.md` baris `AccAccountingEventSchedulerHostedService`; `finance-management/contracts/integration-contract.md` 5.3 |
| `ACC-DEC-085` | Decision | **Isi `AccountingEventReceiptDto`**, sama bentuknya pada balasan `201`, `200`, dan `422`: `AccountingEventId` (`Guid`) — rujukan tanda terima, disimpan Finance sebagai `AccountingReceiptNumber`; `EventNumber` — gema dari pesan; `EventStatus` — `Diterima`, `Terjurnal`, atau `Tertahan`; `JournalNumber?` (maks 30) — hanya bila `Terjurnal`, disimpan Finance sebagai `AccountingJournalNumber`; `AccountingPeriodCode?` (`YYYY-MM`) — periode tempat jurnal jatuh, penting bila periode asal sudah tertutup (`ACC-DEC-047`); `HoldReasonCode?` — `EVENT_TYPE_NOT_REGISTERED`, `POSTING_RULE_MISSING`, `COMPONENT_UNMAPPED` (kejadian membawa komponen yang tidak dipakai aturan), atau `COMPONENT_MISSING` (aturan menuntut komponen yang tidak dibawa kejadian), hanya bila `Tertahan`; `ReceivedAt` (`timestamptz`) — waktu pertama kali diterima. **Tidak ada kolom baru**: rujukan tanda terima adalah `AccAccountingEvent.Id`. **Contoh balasan `422`:** `{ "AccountingEventId": "…", "EventNumber": "EVT-210", "EventStatus": "Tertahan", "JournalNumber": null, "AccountingPeriodCode": null, "HoldReasonCode": "POSTING_RULE_MISSING", "ReceivedAt": "2026-11-03T09:12:44+07:00" }`. Balasan `400`, `403`, `409`, dan `422` karena badan hukum tidak ditemukan **tidak** membawa DTO ini karena kejadiannya tidak tersimpan | Rizki | `approved` | Rizki, 24 September 2026 | `FinAccountingEventOutbox.AccountingReceiptNumber`/`AccountingJournalNumber` (`string?`, maks 50); `erd/data-dictionary.md` bagian 9; `AccJournal.JournalNumber` `string(30)` |
| `ACC-DEC-086` | Decision | **`JASA_MEDIS` tidak pernah menjadi komponen `PENGAKUAN-PIUTANG`**, mengikuti `FIN-DEC-003` (fee dokter diakui terpisah lewat `PENGAKUAN-HUTANG-DOKTER` setelah `DoctorServiceFee` disetujui). (1) Contoh di `api-contract.md`, `cross-module-contract.md`, `evidence/12`, dan kamus data diganti: contoh `PENGAKUAN-PIUTANG` memakai `TOTAL` saja (dua baris), contoh jasa medis dipindah ke `PENGAKUAN-HUTANG-DOKTER` (debit Beban Jasa Medis, kredit Utang Jasa Medis Dokter). (2) Aturan tertulis: aturan posting `PENGAKUAN-PIUTANG` **tidak boleh** memuat baris berkomponen `JASA_MEDIS`. (3) Aturan itu **tidak** ditegakkan kode — mengikat kode Accounting pada nama kode Finance ditolak. (4) Laporan task lama (`BE-ACC-P2-015`, `018`, `FE-ACC-P2-010`) adalah catatan sejarah dan tidak diubah. **Contoh bahaya yang dicegah:** petugas menyalin contoh lama dan menyusun aturan `PENGAKUAN-PIUTANG` empat baris, dua di antaranya berkomponen `JASA_MEDIS`. Karena Finance tidak pernah mengirim komponen itu, aturan validasi "komponen baris aturan harus tersedia" (`ACC-VALIDATION` Phase 2 bagian 2) menahan **setiap** kejadian pengakuan piutang (`422`, Tertahan) — tidak satu pun piutang terjurnal sampai aturannya dibetulkan. Buku besar tidak salah, tetapi berhenti bergerak tanpa ada yang menyadari sebabnya | Rizki | `approved` | Rizki, 24 September 2026 | `FIN-DEC-003`; `finance-management/evidence/01` bagian 2 pertanyaan 3 |
| `ACC-DEC-087` | Decision | **Pesan saldo subledger per periode memakai amplop dua belas bidang yang sama, ditambah dua rincian.** Bentuknya: `EventTypeCode` = kode baru khusus saldo (**usulan `SALDO-SUBLEDGER`**, butuh persetujuan Finance); `Amount` = saldo subledger, **khusus jenis ini boleh nol atau negatif**; `AccountingDate` = tanggal cut-off (menggantikan `AsOfDate` usulan Finance); `SourceVersion` naik bila Finance menyatakan ulang saldo; rincian `AccountingPeriodCode` (`string`, **maks 7**, bentuk `YYYY-MM`, sama dengan `AccAccountingPeriod.PeriodCode`) dan `ControlAccountCode` (`string`, maks 50, wajib menunjuk akun yang ditandai control account). `SubledgerBalance` dan `AsOfDate` **tidak** diulang agar tidak mungkin bertentangan dengan `Amount` dan `AccountingDate`. Kejadian saldo **tidak pernah menghasilkan jurnal**; ia dipakai penghalang rekonsiliasi `ACC-DEC-076`. Rincian perlakuannya (status, tempat simpan) dirancang lewat `design-business-module`. **Contoh:** saldo piutang penjamin per 30 November 2026 Rp 425.000.000 → `Amount` `425000000.00`, `AccountingDate` `2026-11-30`, `AccountingPeriodCode` `2026-11`, `ControlAccountCode` `1-1201`. Menjawab `OD-ACC-08` dan `FIN-OQ-011`; Finance diminta menyesuaikan `FIN-DEC-023` | Rizki (sisi Accounting); Yasmin (kode jenis + penyesuaian `FIN-DEC-023`) | `approved` sisi Accounting | Rizki, 24 September 2026 | `ACC-DEC-071`, `076`; `FIN-DEC-023`; `erd/data-dictionary.md` bagian 1 (`PeriodCode` `string(7)`) dan bagian 9 (`Amount` wajib > 0) |
| `ACC-DEC-088` | Decision | **Syarat Accounting atas akun layanan pengirim kejadian; mekanismenya tetap terbuka.** (1) Satu akun khusus untuk Finance — bukan akun manusia, **bukan SuperAdmin**. (2) Akun itu wajib punya **penugasan Departemen + Jabatan khusus** (misalnya "Integrasi Sistem") yang di `SysAccessPolicy` **hanya** diberi `AccountingEvent : Receive`; tanpa penugasan organisasi, setiap kiriman pasti `403` karena hak Accounting diberikan per Departemen + Jabatan. (3) Akun itu berhak atas badan hukum yang dikirimi kejadian; bila tidak, `403`. (4) Token berumur pendek dicatat sebagai **preferensi** Accounting, bukan syarat. Mekanismenya — "mekanisme auth existing" (`FIN-DEC-007`, `draft`) atau JWT Bearer — **tetap terbuka**, pemilik Platform + Yasmin + Rizki. Keterbukaan itu menahan **pengaktifan pengiriman**, bukan pembangunan kotak masuk. Menjawab sisi Accounting dari `OD-ACC-05` | Rizki (syarat); Platform + Yasmin + Rizki (mekanisme) | `approved` untuk syarat (1)–(4) | Rizki, 24 September 2026 | `permission-audit-matrix.md` baris `AccountingEvent : Receive`; memori uji `BE-ACC-P2-017` (departemen kosong di `QuilvianNewDevRizki` → hanya SuperAdmin yang lolos) |
| `ACC-DEC-089` | Decision | **Cutover 1 Oktober 2026 dinyatakan tidak layak.** Cutover jatuh pada **tanggal 1 pukul 00.00 WIB di awal periode akuntansi pertama setelah seluruh gerbang lolos**: (G1) kotak masuk dibangun dan diuji ujung-ke-ujung; (G2) `ACC-TD-022` ditutup — bagan akun sah tersedia — beserta aturan posting untuk setiap kode yang akan aktif; (G3) akun layanan aktif sesuai `ACC-DEC-088` dan mekanismenya sudah diputuskan; (G4) pengirim Finance siap; (G5) saldo awal manual per tanggal cutover siap diinput; (G6) lihat `ACC-DEC-090`. Tanggal pastinya diputuskan Rizki bersama Yasmin saat gerbang terakhir lolos. Transaksi sebelum cutover tidak dikirim dan tidak direkonstruksi (`FIN-DEC-008`). **Kenapa 1 Oktober tidak layak** (diperiksa 24 September 2026): G1 nol kode, Wave B belum direncanakan, G2 `OPEN`, G3 menunggu Platform, G4 belum dibangun Finance | Rizki; tanggal bersama Yasmin | `approved` | Rizki, 24 September 2026 | `FIN-DEC-008`; `finance-management/contracts/integration-contract.md` 5.7; `UTANG-TEKNIS.md` `ACC-TD-022` |
| `ACC-DEC-090` | Decision | **Deposit pasien, kelebihan bayar beserta pengembaliannya, dan selisih kas shift kasir menjadi gerbang ke-6 (G6) cutover.** Ketiganya adalah peristiwa kas di Billing yang tidak tercakup 17 kode Finance. Siklus dan penerbitannya milik Finance/Billing; Accounting hanya memutuskan bahwa cutover **tidak boleh** dilakukan sebelum salah satu terpenuhi: (a) kode kejadian untuk ketiganya disepakati, atau (b) Finance menyatakan tertulis bagaimana ketiganya tercermin pada 17 kode yang ada — termasuk jaminan bahwa top-up deposit **tidak** dikirim sebagai `PENERIMAAN-KASIR`. **Contoh bahaya yang dicegah:** top-up deposit Rp 5.000.000 terkirim sebagai `PENERIMAAN-KASIR` lalu terbukukan sebagai pendapatan, padahal uang itu kewajiban kepada pasien — buku besar tetap seimbang dan tidak ada pesan galat. Terkait `OD-ACC-07` (deteksi shift kasir terbuka) | Rizki (gerbang); Yasmin + owner Billing (kodenya) | `approved` | Rizki, 24 September 2026 | `evidence/11-pertanyaan-peristiwa-kas-untuk-billing.md`; katalog `FIN-DEC-002` |
| `ACC-DEC-091` | Decision | **Penerimaan kasir sebelum tagihan final harus masuk buku besar pada tanggal diterima, sebagai uang muka pasien.** Posisi Accounting atas tawaran Finance di bagian 4 jawabannya (`FIN-DEC-004`): (1) uang yang sudah diterima kasir **tidak boleh** ditahan dari buku besar sampai tagihan final; (2) lawan akunnya **Uang Muka Pasien** (kewajiban), bukan pendapatan dan bukan piutang; (3) saat tagihan final dan piutang diakui, uang muka dipakai melunasi piutang lewat **kejadian pemakaian uang muka** — kode baru yang wajib disepakati Finance, digabung dengan pertanyaan deposit pada gerbang G6 (`ACC-DEC-090`); (4) bagan akun sah wajib memuat akun Uang Muka Pasien (gerbang G2). **Contoh:** pasien rawat inap membayar Rp 20.000.000 pada 25 November, pulang 5 Desember dengan tagihan Rp 32.000.000. November: debit Kas Rp 20.000.000, kredit Uang Muka Pasien Rp 20.000.000. 5 Desember: pengakuan piutang Rp 32.000.000, lalu pemakaian uang muka — debit Uang Muka Pasien Rp 20.000.000, kredit Piutang Rp 20.000.000 — sehingga sisa piutang Rp 12.000.000. **Kenapa tidak tetap ditahan:** pada tutup November kas di buku besar kurang Rp 20.000.000 dari kas fisik, dan bila saldo subledger kas Finance menghitung uang itu, toleransi nol `ACC-DEC-076` menahan penutupan. **Butuh Finance mengubah `FIN-DEC-004`**; sampai itu terjadi, butir ini masuk gerbang G6 | Rizki (posisi Accounting); Yasmin (perubahan `FIN-DEC-004` dan kode pemakaian uang muka) | `approved` sisi Accounting | Rizki, 24 September 2026 | `FIN-DEC-004`; `finance-management/evidence/01` bagian 4; `finance-management/contracts/integration-contract.md` 5.5 |
| `ACC-DEC-092` | Decision | **Tombol Coba Ulang berlaku untuk kejadian `Gagal` dan `Tertahan`.** Setelah Akuntansi melengkapi aturan posting atau mendaftarkan jenis kejadian, petugas berhak `AccountingEvent : Retry` dapat memproses ulang kejadian tertahan langsung dari layar rincian. Mengabaikan tetap hanya untuk `Gagal` (`ACC-DEC-078`). **Contoh:** kejadian `EVT-210` tertahan `POSTING_RULE_MISSING`; Akuntansi menyusun aturan posting `PENGAKUAN-PIUTANG`, lalu menekan Coba Ulang — jurnal terbentuk, status `Terjurnal`. Menyelaraskan `03-frontend-architecture.md` bagian 11.2 dengan `ACC-STATE` (`Tertahan` → `Terjurnal` lewat `Retry`) dan memberi jalur bagi `UAT-P2-06` | Rizki | `approved` | Rizki, 24 September 2026 | Pertentangan yang ditemukan saat `plan-module-delivery` 24 September 2026 (`FE-ACC-P2-012`, `BE-ACC-P2-025`) |
| `ACC-DEC-093` | Decision | **Pesan saldo subledger untuk periode `Closed` tidak diberi peringatan khusus pada rilis pertama Wave D.** Pesan itu tetap disimpan berstatus `Tercatat` tanpa mengubah angka periode tertutup (`02-backend-architecture.md` bagian 22.6), dan terlihat di Kotak Masuk beserta periodenya. Peringatan khusus ditimbang ulang bila kasusnya benar-benar muncul setelah Wave D berjalan | Rizki | `approved` | Rizki, 24 September 2026 | Butir terbuka `02-backend-architecture.md` bagian 22.6 |

**Yang sengaja TIDAK diputuskan dan tetap terbuka:**

| Butir | Pemilik | Menahan |
|---|---|---|
| Mekanisme autentikasi akun layanan | Platform + Yasmin + Rizki | G3 — pengaktifan pengiriman |
| Kode jenis kejadian saldo (usulan `SALDO-SUBLEDGER`) dan penyesuaian `FIN-DEC-023` | Yasmin | Wave D |
| Daftar komponen yang dikirim Finance per jenis kejadian | Yasmin | Penyusunan aturan posting berbaris banyak |
| Kode atau pernyataan tertulis deposit, refund, selisih shift, **dan kode pemakaian uang muka** (`ACC-DEC-091`) | Yasmin + owner Billing | G6 — cutover |
| Perubahan `FIN-DEC-004` agar penerimaan sebelum tagihan final terbit segera (`ACC-DEC-091`) | Yasmin | G6 — cutover |
| Konfirmasi atau perubahan decision log Finance atas tiga pertentangan sumber di atas | Yasmin | Tidak menahan; wajib didamaikan (`cross-module-contract.md` bagian 11) |
| Tanggal cutover pasti | Rizki + Yasmin | — |
| ~~Status kejadian saldo tanpa jurnal, tempat simpan saldo, aturan tanda `Amount` di validasi~~ **Dirancang dan di-approve 24 September 2026** (`02-backend-architecture.md` bagian 22, `GATE-DESAIN-0924`) | Rizki | — |

### `ACC-DEC-041` — kenapa ditunda, bukan dibatalkan

Perbedaan ini menentukan dan mudah tertukar. Ada **dua** hal yang selama ini menyatu di bawah satu
nama:

| Hal | Yang dijamin | Keadaan sesudah `ACC-DEC-041` |
|---|---|---|
| **Pemisahan data** (`ACC-DEC-037`, `UAT-15`) | Pembukuan PT A dan PT B tidak tercampur | **Tetap berlaku penuh.** Unique index `(LegalEntityId, AccountCode)`, `(LegalEntityId, PeriodCode)`, `(LegalEntityId, JournalNumber)` sudah berdiri di database, dan validasi "seluruh baris menunjuk akun badan hukum yang sama" tetap wajib pada `BE-ACC-010` |
| **Otorisasi pengguna** (`ACC-DEP-008`) | Pengguna PT A tidak dapat membuka buku PT B | **Ditunda.** Mekanismenya tidak ada di platform |

`ACC-DEC-037` **tidak dibatalkan**, dan `LegalEntityId` **tidak dibuang**. Membuangnya akan
menuntut migration baru di atas schema yang sudah diterapkan, membatalkan keputusan yang sudah
`approved`, dan tetap tidak menghilangkan konsepnya — `MstCostCenter.LegalEntityId` berstatus
`[Required]`, sedangkan Accounting wajib merujuk cost center untuk akun beban (`ACC-DEC-019`).
Konsepnya hanya akan menjadi implisit dan tidak terlacak, yang lebih buruk daripada eksplisit.

#### Penjaga yang menjadi syarat keputusan ini

Tanpa penjaga, keputusan ini menyimpan cacat yang muncul diam-diam: begitu badan hukum kedua
didaftarkan, **setiap pengguna langsung memperoleh akses ke dua buku besar sekaligus**, tanpa ada
yang menyadarinya — dan jurnal yang sudah `Posted` tidak dapat dihapus (`ACC-DEC-015`).

Karena itu `BE-ACC-007` wajib memuat pemeriksaan berikut, dan ia **validasi biasa, bukan mekanisme
otorisasi tandingan**:

> Bila jumlah `MstLegalEntity` yang `IsActive` dan tidak terhapus **lebih dari satu**, endpoint
> Accounting menolak permintaan dengan pesan yang menyebutkan bahwa penyaringan badan hukum per
> pengguna belum tersedia dan `ACC-DEP-008` wajib diselesaikan lebih dahulu.

Menolak keras dipilih, bukan memperingatkan di log, karena pembukuan tidak punya jalan mundur yang
murah: mencampur dua buku besar baru ketahuan saat tutup buku, dan koreksinya lewat jurnal
pembalik satu per satu.

### Dua keputusan penyelesai pertentangan — 2 September 2026

`ACC-DEC-039` dan `ACC-DEC-040` berbeda sifatnya dari 38 keputusan sebelumnya. Keduanya **tidak
mengubah target**; keduanya memilih di antara dua bacaan yang sudah sama-sama ada di dalam
artefak canonical dan saling bertentangan.

Karena itu tidak ada satu pun berkas kode yang berubah akibat keduanya: implementasi `BE-ACC-004`
dan `BE-ACC-005` kebetulan sudah memilih bacaan yang sama dengan yang kini disahkan.

Nilai keputusan ini ada pada waktunya. Selama migration belum terbit, mengubah nama tabel atau
tipe kolom hanya berarti menyunting berkas. Setelah migration terbit dan tabelnya berisi data,
hal yang sama menjadi perubahan skema pada tabel produksi.

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
| Status | **`CLOSED`** — 24 September 2026 lewat `ACC-DEC-082`. Sebelumnya `DIKONFIRMASI SISI BILLING` (9 September 2026, `ACC-DEC-059`) |
| Jawaban yang dipilih | **Finance yang menerbitkan.** Billing → Finance (Piutang/Utang) → kejadian keuangan resmi → Accounting |
| Yang sudah setuju | Rizki, 8 September 2026, selaku owner modul Accounting |
| Yang sudah setuju | Rizki (8 Sep) · **Owner Billing (9 Sep)** — jawaban tertulis atas enam pertanyaan |
| Yang belum setuju | ~~Owner Finance (Yasmin)~~ — **sudah**, `FIN-DEC-001`, 20 September 2026: arah, penerbit, dan bentuk dua belas bidang diratifikasi apa adanya |
| **Bukti baru 8 Sep 2026** | [`evidence/10-billing-arap-handoff-scan.md`](evidence/10-billing-arap-handoff-scan.md) — **penerbitnya ternyata sudah ada dan berjalan di Billing** (`BilArHandoff`, `BilApHandoff`, `BilHandoffAdjustment`, ditulis `BillingArApHandoffService` di dalam transaksi finalisasi faktur), tetapi **konsumennya nol**: tidak ada satu pun kode yang mengubah status `CREATED` menjadi `ACKNOWLEDGED`. Temuan ini **tidak membatalkan** `ACC-DEC-044`, tetapi mengubah ongkosnya dan memunculkan pilihan ketiga yang belum pernah dibahas. Enam pertanyaan untuk owner Billing ada di bagian 8 dokumen itu |
| Memblokir | ~~Implementasi jalur jurnal otomatis~~ — **tidak lagi memblokir apa pun** sejak 24 September 2026. Yang masih menahan pengaktifan pengiriman adalah gerbang cutover `ACC-DEC-089`, bukan `ACC-XM-001` |
| **Tidak** memblokir | Rilis pertama, karena `ACC-DEC-009` menempatkan integrasi otomatis di tahap berikutnya |

### Kenapa Finance yang dipilih

Tiga pilihan ditimbang, dan yang menentukan adalah kontrak yang sudah terlanjur disetujui.

| Pilihan | Akibatnya pada kontrak Billing | Akibatnya pada Accounting |
|---|---|---|
| Billing yang menerbitkan | **Menuntut perubahan `BIL-INTEGRATION-0.4`** yang sudah `approved` 20 Agustus 2026. PRD §36 aturan 13 melarangnya | Accounting tidak perlu menunggu Finance |
| **Finance yang menerbitkan (dipilih)** | **Nol perubahan.** `BIL-INT-007`, `008`, dan `009` memang sudah mengarah ke Piutang/Utang, yaitu wilayah Finance | Implementasi menunggu modul Finance berdiri |
| Penerbit netral milik Platform | Nol perubahan, tetapi menambah satu komponen baru | Menunggu komponen itu dibangun, dan belum ada pemiliknya |

Pilihan kedua menang karena **tidak menyentuh satu baris pun kontrak yang sudah disetujui**.
Ongkosnya — menunggu Finance — memang nyata, tetapi ongkos itu sudah harus dibayar apa pun
pilihannya: `ACC-DEP-004` mencatat modul Finance belum ada, dan pemeriksaan 8 September 2026
membenarkannya, `Areas/Corporate/` hanya memuat `AccountingManagement` dan `HumanResource`.

### Alur bisnisnya, langkah demi langkah

Contoh nyata memakai angka. Pasien bernama Budi menjalani rawat jalan, tagihannya
Rp 10.000.000, dan penjaminnya BPJS.

1. **Kasir menutup tagihan Budi.** Modul Billing mencatat tagihan Rp 10.000.000 atas nama Budi.
2. **Billing menyerahkan akibat keuangannya ke Finance**, mengikuti `BIL-INT-007` yang sudah
   disetujui. Finance mencatat Piutang Penjamin sebesar Rp 10.000.000.
3. **Finance menerbitkan satu kejadian keuangan resmi**, misalnya bernomor `EVT-100`, berisi
   dua belas bidang yang dikunci `ACC-DEC-048` dan `ACC-DEC-060`.
4. **Accounting membaca `EVT-100`**, mencocokkan jenis kejadiannya dengan aturan perlakuan
   (`ACC-DEC-045`). Karena pengakuan piutang tergolong bervolume tinggi dan pemetaannya pasti,
   jurnalnya langsung disahkan: debit `1-1201 Piutang Penjamin` Rp 10.000.000, kredit
   `4-1001 Pendapatan Rawat Jalan` Rp 10.000.000.
5. **Bila `EVT-100` terkirim tiga kali** karena gangguan jaringan, penerimaan kedua dan ketiga
   menemukan nomor itu sudah tercatat, lalu mengembalikan nomor jurnal yang sama tanpa membuat
   jurnal baru (`ACC-DEC-035`). Buku besar tetap berisi satu catatan Rp 10.000.000.

**Inilah yang dicegah.** Bila Accounting juga berlangganan langsung ke Billing, langkah 1 akan
menghasilkan jurnal sendiri **dan** langkah 4 menghasilkan jurnal kedua. Buku besar tetap
seimbang, tetapi pendapatan rumah sakit tercatat Rp 20.000.000 dari satu tagihan Rp 10.000.000.
`ACC-DEC-044` menutup jalur itu dengan menyatakan Accounting **tidak** berlangganan ke Billing.

### Batas wewenang keputusan ini

Rizki berwenang penuh menetapkan **apa yang dilakukan Accounting**: dari mana ia membaca
kejadian, dan apa yang tidak boleh ia lakukan. Rizki **tidak** berwenang menetapkan kewajiban
baru bagi modul lain. Karena itu:

| Bagian `ACC-DEC-044` | Wewenang | Berlaku sekarang? |
|---|---|---|
| Accounting **tidak** berlangganan langsung ke Billing | Rizki, owner Accounting | **Ya**, berlaku penuh |
| Accounting membaca kejadian dari Finance | Rizki, owner Accounting | **Ya** untuk sisi pembacaan |
| Finance **wajib menerbitkan** kejadian keuangan resmi | Owner Finance (Yasmin) | **Ya** — `FIN-DEC-001`, 20 September 2026 |
| Bentuk pesan dua belas bidang (`ACC-DEC-048` + `ACC-DEC-060`) | Rizki + owner Finance | **Ya** — `FIN-DEC-001`, 20 September 2026 |

Yang harus dilakukan sebelum implementasi Phase 2 dimulai: bawa `ACC-DEC-044` dan `ACC-DEC-048`
ke owner Billing dan Yasmin untuk diratifikasi. Sampai itu terjadi, perancangan boleh berjalan
dan penulisan kode integrasi tetap **dilarang** oleh `contracts/integration-contract.md` bagian 5.

> **Pembaruan 24 September 2026.** Kedua ratifikasi di atas sudah ada (`ACC-DEC-059`, `FIN-DEC-001`).
> `ACC-DEC-082` menutup `ACC-XM-001`, dan larangan menulis kode integrasi dicabut.

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

### Sudah selesai — Scope pass, 1 September 2026

1. ~~Owner menjawab 36 pertanyaan di atas~~ — **selesai**, menjadi `ACC-DEC-009` sampai `ACC-DEC-043`.
2. ~~Jalankan `/trace-existing-capabilities`~~ — **selesai**, hasilnya `01-existing-capability-map.md`.
3. ~~Selesaikan `ACC-DEP-001` sampai `ACC-DEP-003`~~ — `ACC-DEP-001` `RESOLVED`; `ACC-DEP-002`
   dan `ACC-DEP-003` tercatat sebagai utang, tidak lagi memblokir MVP.

### Sudah selesai — Amendment pass Phase 2, 8 September 2026

4. ~~Tutup `ACC-XM-001`~~ — **selesai sisi Accounting** lewat `ACC-DEC-044`. Ratifikasi owner
   Billing dan owner Finance masih tertunda.
5. ~~Jawab sembilan pertanyaan `DEFERRED`~~ — **selesai**, menjadi `ACC-DEC-045` sampai
   `ACC-DEC-053`, ditambah `ACC-DEC-054` sebagai turunan.

### Belum — gerbang wajib sebelum Phase 2 dirancang

6. Jalankan `/requirement-completeness-gate` untuk menilai kelengkapan requirement lintas modul.
7. Jalankan `/hospital-domain-architect` untuk menetapkan bounded context, ownership, dan dampak
   billing. Phase 2 menerima kejadian yang berasal dari tagihan pasien, sehingga kelonggaran
   "kemampuan non-rumah-sakit" pada `02-backend-architecture.md` bagian 1 **tidak berlaku**.
8. Baru sesudah keduanya lewat, jalankan `/design-business-module` untuk arsitektur Phase 2.

### Belum — sebelum implementasi Phase 2

9. ~~Bawa `ACC-DEC-044` dan `ACC-DEC-048` ke owner Billing dan Yasmin untuk diratifikasi.~~
   **Selesai** — `ACC-DEC-059` (9 Sep) dan `FIN-DEC-001` (20 Sep); ditutup `ACC-DEC-082`.
10. ~~Tunggu modul Finance berdiri (`ACC-DEP-004`, pemilik: Yasmin).~~ **Selesai** — diperiksa
    24 September 2026, `Areas/Corporate/FinanceManagement/` berdiri beserta kotak keluar kejadiannya.

### Sudah selesai — Amendment pass pasca-ratifikasi Finance, 24 September 2026

11. Sepuluh keputusan `ACC-DEC-082` sampai `ACC-DEC-091`; kontrak dan dokumen turunannya
    diselaraskan pada hari yang sama. Balasan untuk Finance:
    [`evidence/13`](evidence/13-balasan-accounting-untuk-finance.md).

### Belum — sesudah 24 September 2026

12. `design-business-module` (amendment): jalur kejadian saldo tanpa jurnal dan
    `AccountingEventReceiptDto` pada arsitektur backend.
13. `plan-module-delivery`: task Wave B — kotak masuk kejadian.
14. Gerbang cutover G1–G6 (`ACC-DEC-089`, `090`) dipantau sampai lolos; `ACC-TD-022` ada di jalur
    kritisnya dan bukan pekerjaan rekayasa.
