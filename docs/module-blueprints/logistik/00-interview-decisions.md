# Logistik & Manajemen Persediaan — Keputusan Wawancara (Interview Decisions)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `LGS-BP-001` |
| Revision | `5` — keputusan `LGS-OQ-020`..`023` disetujui dan dicatat sebagai `LGS-DEC-003`..`006`; verifikasi susulan CITO diterapkan (2026-11-17). `r4` menyempurnakan penerapan `LGS-DEC-002`; `r3` mencatat `LGS-DEC-002`; `r2` mencatat `LGS-DEC-001`; `r1` adalah kerangka awal |
| Pass | Belum ada pass wawancara penuh. `LGS-DEC-001` berasal dari instruksi tertulis pemrakarsa; pertanyaan di bagian 8 menunggu dibahas analis bersama pemilik proses |
| Capability map | [01-existing-capability-map.md](01-existing-capability-map.md) r2 (2026-11-17), disusun sebelum wawancara; menambah `LGS-OQ-017` dan `LGS-OQ-018` |
| Status | **Enam keputusan `approved`: `LGS-DEC-001` (struktur modul), `LGS-DEC-002` (persetujuan permintaan barang unit), `LGS-DEC-003` (atasan unit), `LGS-DEC-004` (otorisasi CITO), `LGS-DEC-005` (verifikasi susulan CITO), `LGS-DEC-006` (batas aktivasi).** Jawaban lain di bagian 8 masih usulan |
| Pemilik produk/domain & approver | Belum ditetapkan (`LGS-OQ-015`) |
| SHA backend | `6011bb32` saat kerangka disusun; disegarkan ke `d946b20b` lewat impact scan capability map `r2` |
| SHA frontend | `72ff3a950` saat kerangka disusun; disegarkan ke `1e9adc790` lewat impact scan capability map `r2` |

> **Catatan.** Dokumen ini dibuat **sebelum** wawancara dan sebelum capability map. Isinya kerangka
> dari dokumen usulan alur yang sudah diserahkan ke analis pada 13 November 2026. Jangan
> memperlakukan usulan jawaban di bagian 7 sebagai keputusan sampai statusnya `approved`.

## 1. Latar belakang

Pada 13 November 2026 pemrakarsa meminta menu baru **Logistik & Manajemen Persediaan** dengan
11 grup dan 67 entri menu: Dashboard Logistik, Master Barang & Gudang, Permintaan Barang Unit,
Penerimaan Barang, Penyimpanan & Penempatan Barang, Stok & Kartu Persediaan, Distribusi &
Pengeluaran Barang, Mutasi & Retur Barang, Stock Opname & Penyesuaian, Pengendalian
Persediaan, serta Laporan & Rekonsiliasi Logistik.

Menu sudah dibuat di frontend. Sebanyak 16 item ditautkan ke halaman Farmasi dan Finance yang
sudah ada; sisanya tampil nonaktif karena backend-nya belum ada.

**Hasil yang diharapkan (usulan):** setiap barang yang masuk ke rumah sakit dapat dilacak dari
penerimaan, penyimpanan, permintaan unit, sampai diterima unit. Selisih stok dapat ditemukan
lewat stock opname, dan nilai persediaan dapat dicocokkan dengan buku besar.

> **Contoh.** Rawat Inap Lantai 3 meminta 40 rol kasa. Kepala ruangan menyetujui, gudang
> mengirim 30 rol karena stok tidak cukup, perawat mengonfirmasi menerima 30 rol, dan sisa 20
> rol menunggu stok masuk. Di akhir bulan, saldo kasa di gudang dihitung ulang dalam stock
> opname, dan nilai persediaannya dicocokkan dengan akun persediaan di buku besar.

## 2. Scope sementara (belum disahkan)

| No | Tahap alur | Cakupan |
| --- | --- | --- |
| 1 | Penerimaan barang | Pemeriksaan jumlah dan kondisi barang dari penyedia, batch/lot, kedaluwarsa, barang tidak sesuai, dokumen penerimaan |
| 2 | Penyimpanan dan penempatan | Lokasi gudang/ruang/rak/bin, FEFO/FIFO, syarat penyimpanan, karantina, penelusuran lokasi |
| 3 | Permintaan barang unit | Pengajuan, validasi atasan, pemeriksaan stok, status pemenuhan, riwayat |
| 4 | Distribusi dan pengeluaran | Penyiapan, pengeluaran gudang, pengiriman, serah terima, pemenuhan sebagian |
| 5 | Mutasi dan retur | Transfer antar gudang, perpindahan lokasi, retur unit, retur ke penyedia, riwayat |
| 6 | Stock opname dan penyesuaian | Jadwal, cut-off, hitung fisik, selisih, verifikasi, persetujuan, berita acara |
| 7 | Pengendalian persediaan | Min/max, reorder point, rekomendasi pengisian ulang, pemantauan kedaluwarsa, rusak, lambat bergerak |
| 8 | Laporan dan rekonsiliasi | Laporan operasional, nilai persediaan, rekonsiliasi dengan buku besar |

**Di luar scope (usulan):** penerbitan Purchase Order, tanda terima barang, tukar faktur, dan
retur pembelian tetap milik Finance (`FIN-DEC-045`). Logistik hanya memakai hasilnya.

## 3. Aktor (sementara)

| Aktor | Tanggung jawab yang diusulkan |
| --- | --- |
| Unit pemohon | Mengajukan permintaan, menerima dan mengonfirmasi barang, retur barang tidak terpakai |
| Atasan unit | Memvalidasi permintaan |
| Petugas gudang (farmasi/logistik umum) | Menerima, menyimpan, menyiapkan, mengeluarkan, mengirim, menghitung opname |
| Penanggung jawab gudang | Menetapkan min/max, menyetujui penyesuaian, retur ke penyedia, pemusnahan |
| Tim penerima barang | Memeriksa jumlah dan kondisi barang dari penyedia |
| Purchasing (Finance) | Purchase Order, tanda terima barang, retur pembelian |
| Akuntansi | Jurnal persediaan dan rekonsiliasi |
| Tim stock opname | Hitung fisik dan verifikasi |

## 4. Keputusan modul lain yang menjadi batasan

Keputusan berikut sudah `approved` di modul asalnya. Logistik wajib mematuhinya atau mengajukan
amendment ke pemiliknya; blueprint ini tidak berwenang mengubahnya.

| ID | Modul asal | Isi singkat | Status |
| --- | --- | --- | --- |
| `PHA-DEC-003` | Farmasi | Custody stok obat: Gudang Utama → Farmasi → Unit → Pasien | `approved` |
| `PHA-DEC-010` | Farmasi | Stok minimum adalah reorder alert, bukan safety stock | `approved` |
| `PHA-DEC-012` | Farmasi | Ketersediaan resep hanya memakai stok farmasi/depo pelayanan | `approved` |
| `PHA-DEC-019` | Farmasi | Stok yang bisa diresepkan = stok layak − reservasi − safety stock | `approved` |
| `PHA-DEC-020` | Farmasi | Reorder point dan safety stock dipisah | `approved` |
| `PHA-DEC-022` | Farmasi | Reorder point dan safety stock per obat per lokasi, ditetapkan petugas berwenang dengan audit | `approved` |
| `PHA-DEC-030` | Farmasi | Stock opname per lokasi/batch: cut-off, maker-checker, persetujuan sebelum penyesuaian, investigasi selisih obat khusus | `approved` |
| `PHA-DEC-031` | Farmasi | Transfer berstatus Dalam Perjalanan; stok tujuan bertambah sesuai penerimaan aktual | `approved` |
| `PHA-OQ-003` | Farmasi | Lifecycle batch/lot, FEFO, karantina, recall, stock opname, koreksi stok | **`open`** |
| `FIN-DEC-045` | Finance | Siklus Purchasing/AP penuh: PO, Goods Receipt, tukar faktur, faktur pembelian | `approved` |
| `FIN-DEC-261` | Finance | BLM, sistem luar, adalah sumber utama PO dan Goods Receipt; BLM mengirim data ke Quilvian | `approved` |
| `FIN-DEC-263` | Finance | ID dokumen BLM disimpan sebagai identitas eksternal; ID Quilvian tetap kunci utama | `approved` |
| `FIN-DEC-268` | Finance | Barang rusak dan menunggu pemeriksaan tidak dihitung sebagai penerimaan sah | `approved` |

## 5. Fakta awal dari kode (bukan audit)

Hasil pembacaan cepat read-only pada 13 November 2026. Audit menyeluruh menjadi tugas
`trace-existing-capabilities`.

| Kemampuan | Yang terlihat |
| --- | --- |
| Saldo stok obat | Endpoint `drug-stock`: `summary`, `balances`, `batches`, `expiring`, `mutations`, `opening-balance`, `adjust`, `change-status`, `mutations/correct` |
| Permintaan obat unit | `stock-requests`, termasuk pencatatan jumlah yang benar-benar diserahkan |
| Transfer, pemakaian, retur obat | `stock-transfers`, `drug-usages`, `drug-returns` |
| Master lokasi | `MstDrugStorageLocation` dengan kode rak/shelf/bin, penanda rantai dingin, high alert, karantina; seeder mengisi 123 gudang resmi (`GDG-001`..`GDG-123`) |
| Pembelian | Finance: Goods Receipt dan Supplier Return (`FinanceGoodsReceipt`, `FinanceSupplierReturn`) |
| Barang non-medis | Tidak ditemukan katalog maupun stok barang non-medis |

## 6. Kondisi frontend saat blueprint dibuat

- Menu **Logistik & Manajemen Persediaan** menggantikan menu Gudang di
  `src/utils/menu-sidebar/menu-items.jsx`; belum di-commit per 17 November 2026.
- Route baru `/health-services/pharmacy-management/drug-stock/summary` dan `/expiring`
  membuka tab yang sesuai pada halaman Stok Farmasi; belum di-commit.
- Perubahan itu dikerjakan sebagai perubahan ad-hoc `FRONTEND MODE` atas permintaan
  pemrakarsa, bukan task roadmap. Bila keputusan pada blueprint ini mengubah struktur menu,
  menu disesuaikan lewat task frontend resmi.

### 6.1 Rekonsiliasi 67 entri menu (2026-11-17)

Hasil pembandingan otomatis daftar asli pemrakarsa (13 November 2026) dengan
`src/utils/menu-sidebar/menu-items.jsx`:

- **67 entri = 66 item yang dapat dipilih + 1 sub-grup.** Sub-grupnya "Pemantauan Barang" di
  grup Pengendalian Persediaan, yang membawahi Kadaluarsa, Barang Rusak, dan Stok Lambat
  Bergerak. Pada daftar asli keempatnya sejajar; susunan sub-grup dipilih saat menu dibuat dan
  dilaporkan pada hari itu. Jumlah dan urutan entri per grup sama persis dengan daftar asli.
- **Tidak ada entri yang ditambah atau dihapus.**
- **14 label dinormalisasi** saat menu dibuat: "Stock" → "Stok" (kecuali istilah "Stock Opname"),
  "Per-Gudang"/"Per-Unit" → "per Gudang"/"per Unit", spasi setelah garis miring dihapus,
  "Pengajuan kebutuhan" → "Pengajuan Kebutuhan", dan salah ketik "Riawayat" → "Riwayat".
  Label ini tidak diubah lagi; mengembalikan ejaan asli menunggu keputusan pemrakarsa.

## 7. Decision Log

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-DEC-001` | Decision | **Struktur modul.** (1) Logistik & Manajemen Persediaan adalah modul utama mandiri dan tampil sebagai bagian navigasi tersendiri, terpisah dari Pelayanan Kesehatan. (2) Instalasi Farmasi tetap bertanggung jawab atas proses persediaan farmasi; keputusan `PHA-DEC-*` tetap berlaku. (3) Finance tetap bertanggung jawab atas Purchasing dan Akuntansi. (4) Logistik wajib terintegrasi dengan Farmasi dan Finance, tanpa menggandakan entity stok Farmasi. (5) Sebelas grup menu dan 67 entri yang sudah dibuat dipertahankan | Pemrakarsa | `approved` | Ikbal Yuliyanto, 2026-11-17 | Instruksi tertulis pemrakarsa ("Master Prompt — Percepat Implementasi Modul Logistik"), 17 November 2026 |

> **Batas keputusan ini.** `LGS-DEC-001` hanya mengunci struktur dan pembagian tanggung jawab
> antarmodul. Keputusan berikut **tidak** ikut disetujui dan tetap terbuka: pemilik produk/domain
> dan approver (`LGS-OQ-015`), alur Goods Receipt ke saldo gudang (`LGS-OQ-017`), cakupan BLM
> (`LGS-OQ-018`), metode valuasi (`LGS-OQ-008`), perlakuan saldo di unit (`LGS-OQ-003`), dan
> seluruh kebijakan operasional lain di bagian 8. `LGS-OQ-001` terjawab sebagian: struktur dan
> pembagian Farmasi/Finance sudah ditetapkan, sedangkan pemilik barang non-medis menunggu
> `LGS-OQ-015`.

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-DEC-002` | Decision | **Persetujuan permintaan barang unit (menutup `LGS-OQ-004`).** (1) Permintaan normal wajib melalui satu tingkat persetujuan atasan unit. (2) Atasan dapat menyetujui atau menolak. (3) Penolakan wajib disertai alasan. (4) Gudang hanya memproses permintaan normal yang sudah disetujui. (5) Permintaan CITO memakai jalur prioritas dengan otorisasi petugas berwenang, alasan wajib, dan audit trail. (6) Setiap keputusan mencatat identitas, waktu, dan perubahan status. (7) Pemohon tidak boleh menyetujui permintaannya sendiri. (8) Permintaan yang ditolak tidak boleh diproses sebagai permintaan aktif | Pemrakarsa | `approved` | Ikbal Yuliyanto, 2026-11-17 | Instruksi tertulis pemrakarsa ("Instruksi Eksekusi Agent — LGS-OQ-004"), 17 November 2026 |
| `LGS-DEC-003` | Decision | **Atasan unit (menutup `LGS-OQ-020`).** (1) Atasan unit adalah pegawai dengan penempatan manajerial aktif pada unit organisasi yang ditautkan ke unit layanan pemohon. (2) Kewenangan berlaku untuk unit yang ditugaskan. (3) Kewenangan tidak otomatis diwariskan ke unit lain atau ke seluruh hierarki organisasi. (4) Pemohon dilarang menyetujui permintaannya sendiri. (5) HRD/administrator organisasi yang berwenang memelihara tautan unit dan penempatan manajerial. (6) Manajemen rumah sakit menetapkan PIC operasionalnya | Pemrakarsa | `approved` | Ikbal Yuliyanto, 2026-11-17 | Instruksi tertulis pemrakarsa ("Master Prompt — Finishing Persetujuan Permintaan Barang Logistik"), 17 November 2026 |
| `LGS-DEC-004` | Decision | **Otorisasi CITO (menutup `LGS-OQ-021`).** (1) Otorisasi CITO diberikan kepada jabatan operasional yang ditunjuk eksplisit oleh manajemen. (2) Kandidat jabatan: Kepala Instalasi/Unit atau Penanggung Jawab Gudang sesuai kewenangan masing-masing. (3) Daftar jabatan definitif belum tersedia; izin tidak diberikan otomatis kepada seluruh jabatan kandidat. (4) SuperAdmin tidak mendapat hak CITO hanya karena identitas administrator sistem. (5) Izin wajib diperiksa eksplisit terhadap penempatan jabatan yang aktif | Pemrakarsa | `approved` | Ikbal Yuliyanto, 2026-11-17 | Idem |
| `LGS-DEC-005` | Decision | **Verifikasi susulan CITO (menutup `LGS-OQ-022`).** (1) CITO dengan otorisasi sah dapat segera diproses. (2) Verifikasi administratif susulan wajib dilakukan atasan unit. (3) Verifikasi susulan tidak memposting ulang stok. (4) Penolakan atau temuan susulan tidak boleh diam-diam menghapus transaksi fisik yang sudah terjadi. (5) Penyimpangan harus menghasilkan catatan audit dan proses tindak lanjut | Pemrakarsa | `approved` | Ikbal Yuliyanto, 2026-11-17 | Idem |
| `LGS-DEC-006` | Decision | **Batas aktivasi persetujuan (menutup `LGS-OQ-023`).** (1) Persetujuan diberlakukan lewat batas aktivasi per lingkungan. (2) Permintaan sebelum batas mengikuti aturan transisi yang disetujui. (3) Permintaan setelah batas wajib memakai persetujuan. (4) Batas dikunci pada saat aktivasi; perubahan surut membutuhkan persetujuan tersendiri. (5) Nilai batas kosong tidak boleh diartikan sebagai izin jalan pintas. Nilai batas ditentukan saat aktivasi di tiap lingkungan, bukan pada keputusan ini | Pemrakarsa | `approved` | Ikbal Yuliyanto, 2026-11-17 | Idem |

> **Cara `LGS-DEC-002` sampai `LGS-DEC-006` diterapkan di source (bukan bagian keputusan, r5).**
>
> - **Status.** Nilai `StockRequestStatus` tidak diubah. `Submitted` (2) = Menunggu Persetujuan,
>   `Approved` (3) = Disetujui dan satu-satunya status yang boleh diserahkan gudang (kecuali aturan
>   peralihan), `Rejected` (5) = Ditolak dan final. "Diproses Gudang" bukan status tersendiri.
>   "Dipenuhi Sebagian" = `Completed` + `isPartiallyFulfilled`. "CITO Diotorisasi", "Menunggu
>   Verifikasi Susulan", "Verifikasi Selesai", dan "Temuan Penyimpangan" adalah keadaan turunan dari
>   riwayat (`emergencyReviewState`), bukan nilai status baru.
> - **Atasan unit (`LGS-DEC-003`).** `MstServiceUnit.OrganizationUnitId` → `MstOrganizationUnit`
>   (aktif dan berlaku) ← `WfpOrganizationAssignment` manajerial yang aktif, tidak batal, berlaku, dan
>   profil tenaga kerjanya aktif. Hanya satu tingkat; tidak diwariskan ke unit induk. Setuju, tolak, dan
>   verifikasi susulan butuh **izin `StockRequest:Approve` eksplisit pada penempatan jabatan dan
>   kewenangan atasan unit sekaligus**, diperiksa di service (`StockRequestApprovalAuthority`) sehingga
>   tidak ikut dilewati SuperAdmin maupun `Security:Authorization:Enabled=false`.
> - **Data organisasi.** Audit ulang 2026-11-17 (baca saja): basis data tim 0/20 unit layanan tertaut,
>   5 unit organisasi aktif, 0 penempatan manajerial; basis data lokal 0/5 unit layanan tertaut dan
>   0 unit organisasi. Sampai HRD mengisi tautan dan penempatan manajerial, tidak ada yang dapat
>   menyetujui. Prosedur dan kandidat identifier: `evidence/06-dataset-uji-persetujuan.md`.
> - **Otorisasi CITO (`LGS-DEC-004`).** Izin `StockRequest:AuthorizeEmergency` diperiksa ulang di
>   service terhadap penempatan jabatan aktif, tanpa jalan pintas SuperAdmin. Izin belum diberikan
>   kepada jabatan mana pun; jabatan definitif menunggu penunjukan manajemen. Audit basis data tim tidak
>   menemukan jabatan bernama "Kepala Instalasi" maupun "Penanggung Jawab Gudang".
> - **Verifikasi susulan CITO (`LGS-DEC-005`).** Endpoint `POST /stock-requests/{id}/verify-emergency`
>   dengan hasil `Confirmed` atau `Deviation` (catatan wajib). Disimpan sebagai baris riwayat dengan
>   aksi `VerifyEmergencyStockRequestConfirmed`/`VerifyEmergencyStockRequestDeviation`; status dan
>   stok tidak berubah; versi dinaikkan sebagai penjaga verifikasi ganda. Pemohon dan pengotorisasi CITO
>   tidak boleh memverifikasi. Temuan tercatat di riwayat dan log audit (peringatan). **Blocker
>   persistence:** proses tindak lanjut temuan (penanggung jawab, status penyelesaian) belum punya tempat
>   penyimpanan dan membutuhkan entity baru (migration).
> - **Batas aktivasi (`LGS-DEC-006`).** `PharmacyManagement:StockRequest:ApprovalEnforcedFromUtc`,
>   UTC, dibandingkan dengan `SubmittedAt` yang ditulis server. Kosong, tidak sah, atau di masa depan
>   → semua permintaan wajib disetujui. Permintaan tanpa `SubmittedAt` tidak pernah dianggap
>   permintaan lama. Setiap nilai baru tercatat di log. Penguncian setelah aktivasi bergantung pada
>   kendali perubahan konfigurasi; penolakan teknis atas perubahan surut membutuhkan tabel aktivasi
>   (migration). Prosedur: `evidence/05-aktivasi-batas-persetujuan.md`. Audit data: 0 permintaan
>   `Submitted` di basis data tim dan lokal.

## 8. Open Questions dan Blocker

Kolom "Usulan jawaban" adalah titik awal diskusi, bukan keputusan.

| ID | Pertanyaan | Usulan jawaban | Owner | Memblokir |
| --- | --- | --- | --- | --- |
| `LGS-OQ-001` | ~~Apakah Logistik modul baru, perluasan Farmasi, atau modul payung?~~ Terjawab sebagian oleh `LGS-DEC-001` (modul utama mandiri; persediaan farmasi tetap di Farmasi). Sisa: siapa pemilik barang non-medis | Barang non-medis dikelola modul Logistik | Analis + manajemen | Tahap 1–8 untuk non-medis |
| `LGS-OQ-002` | Gudang mana yang memegang barang non-medis? | Pakai master lokasi yang sudah berisi 123 gudang; tandai peruntukan medis, non-medis, atau keduanya | Analis + pemilik gudang | Tahap 1–8 untuk non-medis |
| `LGS-OQ-003` | Apakah unit punya saldo sendiri atau barang dianggap terpakai saat diserahkan? | Medis: unit memegang stok (`PHA-DEC-003`). Non-medis: terpakai saat diserahkan, dibebankan ke unit | Analis + Akuntansi | Tahap 4, 6, 8 |
| `LGS-OQ-004` | ~~Siapa yang memvalidasi permintaan unit, dan apa pengecualiannya?~~ **Ditutup oleh `LGS-DEC-002`** (2026-11-17). Pertanyaan lanjutan: `LGS-OQ-020`, `021`, `022` | — | — | — |
| `LGS-OQ-005` | Sisa permintaan yang terpenuhi sebagian ditangani bagaimana? | Menunggu stok 7 hari, lalu ditutup otomatis; unit mengajukan ulang bila perlu | Analis | Tahap 4 |
| `LGS-OQ-006` | Apakah unit wajib mengonfirmasi penerimaan? | Wajib; tanpa konfirmasi 1×24 jam status tetap Dikirim dan unit diingatkan | Analis | Tahap 4 |
| `LGS-OQ-007` | FEFO atau FIFO? | FEFO wajib untuk medis, FIFO untuk non-medis; detail obat menunggu `PHA-OQ-003` | Analis + pemilik Farmasi | Tahap 2, 4 |
| `LGS-OQ-008` | Metode penilaian persediaan? | **Tidak ada usulan.** Pilihan umum: rata-rata tertimbang atau FIFO | Akuntansi | Tahap 8 |
| `LGS-OQ-009` | Frekuensi, tim, toleransi selisih, dan persetujuan stock opname? | Ikuti `PHA-DEC-030`; persetujuan berjenjang sesuai nilai selisih | Analis + pemilik Farmasi + manajemen | Tahap 6 |
| `LGS-OQ-010` | Alur barang rusak dan kedaluwarsa sampai pemusnahan? | Karantina → berita acara pemusnahan → persetujuan pejabat berwenang → jurnal penghapusan | Analis + Akuntansi | Tahap 5, 7 |
| `LGS-OQ-011` | Apakah rekomendasi pengisian ulang langsung membuat permintaan pembelian? | Tidak; hanya rekomendasi, petugas meneruskan ke Purchasing | Analis + Finance | Tahap 7 |
| `LGS-OQ-012` | Transaksi mana yang membuat jurnal akuntansi? | Penerimaan (sudah lewat Finance), pengeluaran ke unit, penyesuaian opname, pemusnahan | Akuntansi | Tahap 6, 8 |
| `LGS-OQ-013` | Siapa penanggung jawab tiap gudang dan apa wewenangnya? | Satu penanggung jawab dan satu pengganti per gudang; petugas hanya bertransaksi di gudang yang ditugaskan | Analis + manajemen | Tahap 1–7 |
| `LGS-OQ-014` | Kapan barang disebut lambat bergerak? | Tidak ada pengeluaran 90 hari; bisa diatur per kategori | Analis | Tahap 7 |
| `LGS-OQ-015` | Siapa pemilik produk/domain dan approver blueprint Logistik? | — | Manajemen | Seluruh approval |
| `LGS-OQ-016` | Prefix entity apa yang didaftarkan di registry? | `Lgs` = *Logistik*; diajukan setelah `LGS-OQ-001` dan `LGS-OQ-015` terjawab | Pemilik registry | Pembuatan entity pertama |
| `LGS-OQ-017` | Bagaimana barang dari Goods Receipt Finance masuk ke saldo gudang? Saat ini item Purchase Order berupa teks bebas dan Goods Receipt tidak menambah stok (`LGS-CAP-07`) | **Tidak ada usulan.** Pilihan: (a) item PO ditautkan ke katalog sehingga Goods Receipt langsung menambah saldo, atau (b) gudang mencatat penerimaan sendiri yang merujuk nomor Goods Receipt | Pemilik Finance + pemilik Farmasi | Tahap 1, 5, 8 |
| `LGS-OQ-018` | Apakah BLM, sumber PO dan Goods Receipt (`FIN-DEC-261`), juga mengelola gudang, distribusi, atau stock opname? Audit Finance mencatat BLM sudah punya fondasi Warehousing (`FIN-DEC-289`), tetapi pemakaian operasionalnya belum diketahui. Investigasi: `evidence/LGS-BLM-INVESTIGATION.md` (fitur ada menurut audit Finance; pemakaian operasional BELUM TERVERIFIKASI; pemrakarsa memutuskan investigasi lebih dulu, 2026-11-17) | **Tidak ada usulan.** Bila ya, Logistik di Quilvian menjadi penampung/integrasi data BLM, bukan duplikasi. Bila tidak, alur pada dokumen usulan berlaku | Analis + pemilik Finance + pemilik Farmasi | Semua tahap; dijawab bersama `LGS-OQ-001` |
| `LGS-OQ-019` | Peringatan "Barang Hampir Habis": (a) dibandingkan dengan saldo fisik siap pakai atau saldo tersedia (fisik − reserved)? (b) Ambangnya `ReorderPointQuantity` atau `MinimumStockQuantity`? `PHA-DEC-010` menyebut stok minimum sebagai reorder alert, sedangkan `PHA-DEC-020` memakai istilah reorder point, dan master kebijakan menyimpan keduanya | **Tidak ada usulan.** Implementasi `LGS-BE-ADH-01` saat ini memakai (a) saldo fisik siap pakai dan (b) reorder point, dan menampilkan reserved serta available supaya bedanya terlihat | Pemilik Farmasi | Ketepatan daftar Barang Hampir Habis |
| `LGS-OQ-020` | ~~Definisi atasan unit~~ **Ditutup oleh `LGS-DEC-003`** (2026-11-17) | — | — | — |
| `LGS-OQ-021` | ~~Jabatan otorisasi CITO~~ **Ditutup oleh `LGS-DEC-004`** (2026-11-17). Daftar jabatan definitif masih menunggu penunjukan manajemen (tindakan operasional, bukan keputusan baru) | — | — | — |
| `LGS-OQ-022` | ~~Verifikasi susulan CITO~~ **Ditutup oleh `LGS-DEC-005`** (2026-11-17) | — | — | — |
| `LGS-OQ-023` | ~~Aturan peralihan~~ **Ditutup oleh `LGS-DEC-006`** (2026-11-17). Nilai batas ditentukan saat aktivasi per lingkungan | — | — | — |

## 9. Pemisahan pekerjaan: READY TO IMPLEMENT dan REQUIRES DECISION

Disusun 17 November 2026 atas permintaan pemrakarsa. Pekerjaan masuk kategori A hanya bila
aturan bisnisnya sudah `approved` (di blueprint ini atau `PHA-DEC-*`/`FIN-DEC-*`), kontrak datanya
sudah ada di source, dan tidak membutuhkan tabel/kolom baru (pembuatan migration belum
diizinkan).

### A. READY TO IMPLEMENT

| Pekerjaan | Dasar | Status 2026-11-17 |
| --- | --- | --- |
| Logistik sebagai bagian navigasi tersendiri | `LGS-DEC-001` | Dikerjakan (`LGS-FE-ADH-01`) |
| Peringatan stok di bawah reorder point (Barang Hampir Habis) | `PHA-DEC-010`, `020`, `022`; `MstDrugStockPolicy` | Dikerjakan (`LGS-BE-ADH-01`, `LGS-FE-ADH-01`) |
| Daftar Batch/Lot | `GET /drug-stock/batches` sudah ada (`LGS-CAP-09`) | Dikerjakan (`LGS-FE-ADH-01`) |
| Pemantauan barang rusak | Status `Damaged` pada saldo (`LGS-CAP-29`) | Dikerjakan (`LGS-FE-ADH-01`) |
| Riwayat barang masuk/keluar | Kartu stok (`LGS-CAP-31`) | Ditautkan (`LGS-FE-ADH-01`) |
| Ringkasan stok, kedaluwarsa, permintaan unit, transfer, retur | Halaman Farmasi yang sudah ada | Ditautkan sejak 13 November |
| Penelusuran lokasi barang (cari barang → lokasi) | `GET /drug-stock/balances` (`LGS-CAP-13`) ditambah kode rak/shelf/bin dari master lokasi | Dikerjakan (`LGS-BE-ADH-01` lanjutan, `LGS-FE-ADH-01` lanjutan) |
| Penyesuaian status karantina dan rusak dari layar saldo | `POST /drug-stock/change-status` sudah ada | Sudah tersedia lewat tombol Kelola di layar saldo |
| Persetujuan atasan, penolakan beralasan, otorisasi CITO, verifikasi susulan CITO, batas aktivasi, dan penyerahan hanya untuk permintaan Disetujui | `LGS-DEC-002`..`006` | Source selesai (`LGS-BE-ADH-02`..`04`, `LGS-FE-ADH-02`..`04`); belum diverifikasi runtime. Dapat dipakai setelah HRD mengisi data organisasi, manajemen menunjuk jabatan CITO, dan izin diberikan |

### B. REQUIRES DECISION

Rekomendasi di bawah bukan approval.

| Keputusan | Rekomendasi | Dampak teknis | Pekerjaan yang tertahan |
| --- | --- | --- | --- |
| `LGS-OQ-015` pemilik dan approver | Tetapkan satu pemilik produk Logistik dari unit logistik/rumah tangga RS, dengan pemilik Farmasi sebagai approver untuk barang medis | Tanpa pemilik, tidak ada yang berwenang menyetujui keputusan lain maupun baris registry prefix | Seluruh keputusan bagian 8, pendaftaran prefix `Lgs` (`LGS-OQ-016`) |
| `LGS-OQ-017` Goods Receipt ke saldo gudang | Gudang mencatat penerimaan sendiri yang merujuk Goods Receipt dan memetakan produk BLM ke master obat, sehingga Finance dan BLM tidak diubah | Butuh tabel pemetaan produk BLM ↔ master obat dan tabel penerimaan gudang (migration); posting stok memakai `ReceiveBatchAsync` yang sudah ada dengan sumber `GoodsReceipt` | Penerimaan barang tahap 1, retur ke penyedia mengurangi saldo, nilai persediaan |
| `LGS-OQ-008` metode valuasi | Diputuskan Akuntansi; rata-rata tertimbang lebih sederhana untuk persediaan RS dengan banyak batch | Butuh kolom harga per batch atau per mutasi (migration) dan sumber harga dari PO/GR | Laporan nilai persediaan, rekonsiliasi buku besar, jurnal persediaan |
| `LGS-OQ-003` saldo di unit | Barang medis: unit punya lokasi stok sendiri sesuai `PHA-DEC-003`; barang non-medis: dianggap terpakai saat diserahkan | Bila unit punya saldo, penyerahan permintaan harus memakai pola transfer (Dalam Perjalanan → diterima) agar saldo tidak tercatat dua kali | Serah terima di unit, stok per unit, retur unit lengkap, opname di unit |
| `LGS-OQ-009` stock opname | Ikuti `PHA-DEC-030` | Butuh tabel sesi dan baris opname (migration); posting memakai `adjust` dengan maker-checker | Seluruh grup Stock Opname & Penyesuaian |
| `LGS-OQ-012` jurnal persediaan | Diputuskan Akuntansi bersama `LGS-OQ-008` | Integrasi ke modul Akuntansi | Rekonsiliasi dengan Keuangan |

## 10. Langkah berikutnya

1. **`LGS-OQ-018` — arahan pemrakarsa 2026-11-17: verifikasi pemakaian Warehousing BLM lebih
   dulu.** Hasil investigasi kode dan dokumen ada di `evidence/LGS-BLM-INVESTIGATION.md`; bukti
   pemakaian operasional masih harus diminta ke pemilik Finance dan pengelola BLM (daftar bukti
   di bagian 8 laporan itu). Status tetap `open`; belum ada sumber saldo yang ditetapkan.
2. Analis membahas pertanyaan yang masih terbuka (`LGS-OQ-001`..`003`, `005`..`019`) bersama
   pemilik proses; `LGS-OQ-015` (pemilik) dan `LGS-OQ-019` (dasar peringatan hampir habis, lihat
   `evidence/03-audit-barang-hampir-habis.md`) didahulukan. Untuk persetujuan permintaan unit, yang
   tersisa adalah tindakan operasional: HRD mengisi tautan unit dan penempatan manajerial
   (`LGS-DEC-003`), manajemen menunjuk jabatan CITO (`LGS-DEC-004`), dan batas aktivasi ditetapkan
   per lingkungan (`LGS-DEC-006`).
3. Desain backend-first untuk persetujuan, opname, distribusi, penerimaan, dan valuasi ada di
   `evidence/01-daftar-pekerjaan-backend-first.md` (draf, menunggu keputusan).
4. Jawaban yang disetujui dicatat sebagai `LGS-DEC-*` pada revisi berikutnya, lalu requirement
   completeness gate, arsitektur domain, desain blueprint, dan roadmap.
