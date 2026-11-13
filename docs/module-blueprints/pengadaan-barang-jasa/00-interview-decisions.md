# Pengadaan Barang & Jasa — Keputusan Wawancara

| Field | Nilai |
|---|---|
| Blueprint ID | `PBJ-BP-001` |
| Revisi | `1` |
| Status | `draft` |
| Mode wawancara | Scope pass (belum ada capability map) |
| Pemilik produk/domain | Pengguna sesi (pemilik kebutuhan); pemilik proses Pengadaan formal **belum disebut** (`PBJ-OQ-017`) |
| SHA backend | `100c011d54147bb19ca4bd874fdfb200448ba46a` (branch `sukmagp`) |
| SHA frontend | `763d665b2db3f31cf370ddba68623dc8aa57cf18` (branch `sukmagpV2`) |
| Masukan | BRD & PRD v1.0 DRAFT (12 November 2026); `00-business-overview.md` revisi 1 |

> **Catatan audit.** Scope dikunci **tanpa** audit kemampuan existing (`01-existing-capability-map.md`
> belum ada). Kemungkinan duplikasi dengan modul lain baru diperiksa sebatas blueprint Finance
> Management dan registry prefix. Audit lengkap menyusul lewat `trace-existing-capabilities`.

---

## 1. Batas scope

**Satu kalimat:** Modul Pengadaan Barang & Jasa mengelola tahap *hulu* pengadaan, yaitu dari
kebutuhan unit sampai vendor ditetapkan, lalu memantau pelaksanaan, pemeriksaan hasil, dan
evaluasi vendor. Pembuatan PO dan pencatatan GR tetap berada di BLM dan Finance Management.

### Di dalam scope (usulan dari `PBJ-DEC-001`, menunggu konfirmasi `PBJ-DEC-003`)

| No | Kemampuan | Menu sumber |
|---|---|---|
| K-01 | Rencana kebutuhan tahunan, konsolidasi, rencana paket, revisi rencana | Menu 2 |
| K-02 | Rencana anggaran pengadaan (pagu per unit/kategori/periode) | Menu 2 (pemilik data: `PBJ-OQ-009`) |
| K-03 | Permintaan pengadaan (PR), verifikasi, permintaan mendesak, riwayat | Menu 3 |
| K-04 | Pemeriksaan stok sebelum membeli (hanya membaca stok) | Menu 3 (sumber: `PBJ-OQ-008`) |
| K-05 | Persiapan: spesifikasi, KAK/TOR, HPS, metode, checklist dokumen | Menu 4 |
| K-06 | Data vendor pengadaan: kategori, legalitas, rekening, status, riwayat | Menu 5 (bentuk: `PBJ-OQ-010`) |
| K-07 | Permintaan penawaran (RFQ), data penawaran, evaluasi teknis, perbandingan harga, negosiasi, penetapan penyedia | Menu 6 |
| K-08 | SPK, kontrak, termin, addendum, arsip kontrak | Menu 7 (kepemilikan: `PBJ-OQ-011`) |
| K-09 | Monitoring pesanan, jadwal kirim, progres jasa, keterlambatan, perubahan jadwal, kendala | Menu 8 |
| K-10 | Pemeriksaan hasil, serah terima pekerjaan jasa, BAST, ketidaksesuaian (NCR), penyelesaian | Menu 9 (kepemilikan: `PBJ-OQ-012`) |
| K-11 | Penilaian kinerja vendor, evaluasi kontrak, rekap keterlambatan dan NCR, rekomendasi | Menu 10 |
| K-12 | Antrean persetujuan, validasi, persetujuan bertingkat paket dan kontrak | Menu 11 |
| K-13 | Dashboard dan laporan: pembelian, realisasi, audit | Menu 1, Menu 11 |

### Di luar scope — milik modul lain

| Kemampuan | Pemilik | Titik sentuh dengan modul ini |
|---|---|---|
| Membuat, mengubah, menyetujui PO | BLM (sumber), Finance Management menyimpan salinan | Pengadaan menyerahkan hasil penetapan vendor (`PBJ-OQ-014`) dan membaca status PO |
| Mencatat GR (tanda terima barang) | BLM / Finance Management | Pengadaan membaca kuantitas diterima untuk monitoring |
| Tukar Faktur, Faktur Pembelian, Retur, utang, pembayaran | Finance Management | Laporan realisasi membaca nilai faktur/bayar (`PBJ-OQ-009`) |
| Master supplier inti `MstSupplier` | Administrator / Master Data | Data vendor Pengadaan memakai atau memperluasnya (`PBJ-OQ-010`) |
| Mutasi stok dan penerimaan ke gudang | Pemilik stok (belum diketahui, `PBJ-OQ-008`) | Pengadaan hanya membaca stok |
| Jurnal akuntansi | Accounting | Tidak ada titik sentuh langsung |

### Di luar scope — dicatat untuk modul lain

Pembayaran bank, jurnal, tender publik, tanda tangan elektronik tersertifikasi, e-catalog
eksternal, perpajakan mendalam (BRD A5, out-of-scope awal).

---

## 2. Penilaian bentuk blueprint (saran, belum keputusan)

Rumpun disusun dari kepemilikan data dan siklus hidup, bukan dari menu.

| Rumpun | Kemampuan | Konteks sendiri | Status sendiri | Hak akses & peran sendiri | Rilis sendiri | Master & approval sendiri | Lolos |
|---|---|---|---|---|---|---|---|
| R1 Perencanaan & Permintaan | K-01..K-04 | Ya | Ya (PR `DRAFT→SUBMITTED→...`) | Ya (Unit Peminta, Kepala Unit) | Ya | Sebagian | 4/5 |
| R2 Pemilihan Penyedia | K-05, K-07 | Ya | Ya (paket `PREPARATION→RFQ_OPEN→...→AWARDED`) | Ya (Tim Pengadaan, panitia evaluasi) | Ya, tetapi butuh R1 | Ya (metode, kriteria) | 4/5 |
| R3 Manajemen Vendor | K-06, K-11 | Ya | Ya (status vendor) | Ya (verifikator rekening) | Ya | Ya (master vendor) | 5/5 |
| R4 Kontrak & Pelaksanaan | K-08..K-10 | Ya | Ya (kontrak, BAST, NCR) | Ya (Pemeriksa Teknis, Legal) | Tidak, butuh R2 dan BLM | Belum jelas | 3/5, tertahan `PBJ-OQ-011/012` |
| — | K-12 Persetujuan, K-13 Laporan | Lintas rumpun | — | — | — | — | **Calon requirement yatim** (`PBJ-OQ-018`) |

**Hasil uji:** lebih dari dua rumpun lolos, jadi bentuk yang disarankan adalah `COMPOSITE`.
**Kemampuan tanpa rumpun:** K-12 dan K-13. Keduanya dipakai semua rumpun, sehingga harus diputuskan
apakah menjadi sub-modul sendiri atau memakai mesin approval platform (bila ada, `PBJ-DEP-004`).

---

## 3. Log keputusan

| ID | Jenis | Keputusan / pertanyaan | Pemilik | Status | Disetujui oleh/pada | Bukti |
|---|---|---|---|---|---|---|
| `PBJ-DEC-001` | Decision | Pilihan A: Pengadaan modul hulu; PO/GR milik BLM/Finance, hanya dibaca | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `00-business-overview.md` bagian 2 |
| `PBJ-DEC-002` | Decision | Blueprint baru `PBJ-BP-001`, tidak menggantikan blueprint lain | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Idem |
| `PBJ-DEC-003` | Decision | Daftar "Di dalam scope" (K-01..K-13) dan "Di luar scope" pada bagian 1 **dikonfirmasi** apa adanya. SPK/kontrak (K-08) dan pemeriksaan/BAST/NCR (K-10) tetap di dalam scope; detail kepemilikannya masih `PBJ-OQ-011`/`012` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-004` | Decision | **`blueprint_shape: COMPOSITE`**, `shape_decided_by: USER_CONFIRMED`. Empat calon sub-modul: `perencanaan-permintaan` (K-01..K-04), `pemilihan-penyedia` (K-05, K-07), `manajemen-vendor` (K-06, K-11), `kontrak-pelaksanaan` (K-08..K-10). Menutup `PBJ-OQ-001`. Folder sub-modul dibuat oleh `design-business-module`, bukan di wawancara ini | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Bagian 2 |
| `PBJ-DEC-005` | Decision | K-12 Persetujuan dan K-13 Dashboard/Laporan ditempatkan di **tingkat modul**, tidak menjadi sub-modul. Persetujuan memakai mesin approval platform **bila** audit membuktikan mesin itu ada (`PBJ-DEP-004`); tiap sub-modul hanya mendaftarkan jenis dokumennya. Bila tidak ada, keputusan ini dibuka kembali. Menutup `PBJ-OQ-018` | Pengguna sesi | `approved` (bersyarat hasil audit) | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-006` | Decision | Rumah sakit berstatus **swasta**, dan pengadaan mengikuti **SOP internal**. Metode pengadaan, batas nominal, dan dokumen wajib **dikonfigurasi** sesuai kebijakan direksi, tidak ditulis mati di kode dan tidak diturunkan dari regulasi BLU/BLUD. Menutup `PBJ-OQ-003` (dokumen SOP-nya sendiri belum diserahkan, lihat `PBJ-OQ-019`) | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-007` | Decision | **Matriks persetujuan terkonfigurasi.** Jenjang dan peran penyetuju disimpan sebagai data, dengan kunci: jenis dokumen (PR, rencana, paket, penetapan vendor, kontrak, addendum, perubahan rekening vendor), rentang nominal, dan urgensi. Nilai awal diisi dari SOP (`PBJ-OQ-019`). **Invariant:** pengusul tidak boleh menyetujui dokumennya sendiri; jenjang yang berlaku dan nominal saat disetujui dicatat permanen (tidak dihitung ulang bila matriks berubah). **Contoh:** PR Rp 35.000.000 dengan matriks "< Rp 50 juta → Kepala Unit lalu Manajer Pengadaan". Kepala Unit A yang membuat PR itu tidak bisa menyetujuinya sendiri; tugasnya diarahkan ke penyetuju lain sesuai matriks. Bila ambang kelak diubah menjadi Rp 30 juta, PR yang sudah disetujui tetap tercatat dengan jenjang lama. Menutup `PBJ-OQ-004` | Pengguna sesi | `approved` (struktur); nilai awal menunggu `PBJ-OQ-019` | Pengguna sesi, 2026-11-12 | Jawaban wawancara; pola `FIN-DEC-052` (jenjang dicatat saat approval) |
| `PBJ-DEC-008` | Decision | **Pemeriksaan stok tanpa integrasi pada slice pertama.** Petugas mencatat hasil cek stok secara manual per baris PR: jumlah tersedia, gudang/sumber (teks), waktu cek, pemeriksa, dan keputusan (penuhi dari stok, beli sebagian, beli penuh). Kebutuhan beli = jumlah diminta − jumlah dipenuhi dari stok. **Contoh:** PR 20 kursi roda; petugas mencatat 5 tersedia di Gudang Umum dan dipenuhi dari stok, sehingga kebutuhan beli = 15. Integrasi sumber stok ditunda dan `PBJ-OQ-008` tetap terbuka sebagai `LATER SLICE`. Tidak ada reservasi stok | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara; `PBJ-FACT-005` |
| `PBJ-DEC-009` | Decision | **Pagu di Pengadaan, realisasi dari Finance.** Pengadaan memiliki pagu (per unit, kategori, sumber dana, periode) dan nilai **komitmen** (nilai penetapan vendor/kontrak yang disetujui). Nilai **ditagih** dan **dibayar** dibaca dari Finance, tidak disalin sebagai sumber kebenaran. Laporan memisahkan empat angka: pagu, komitmen, ditagih, dibayar. **Contoh:** pagu Unit Radiologi 2027 Rp 500 juta; penetapan vendor disetujui Rp 120 juta → komitmen Rp 120 juta, sisa pagu Rp 380 juta; Finance mencatat faktur Rp 118 juta → ditagih Rp 118 juta. Menutup `PBJ-OQ-009` (pemilik). Kaitan nilai ditagih ke paket Pengadaan bergantung pada rantai paket → PO BLM → faktur (`PBJ-OQ-014`) | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-010` | Decision | **Data vendor di tabel pendamping milik Pengadaan.** Identitas vendor tetap `MstSupplier` (satu vendor satu ID di semua modul). Kategori, legalitas, status pengadaan (aktif/dibatasi/diblokir), riwayat, dan evaluasi disimpan di tabel Pengadaan yang merujuk `MstSupplier`. Tabel milik Administrator **tidak diubah**. **Contoh:** PT Sehat Medika sudah ada di `MstSupplier`; Pengadaan menambahkan dokumen NIB berlaku sampai 31-12-2027 dan status "Aktif" tanpa membuat ID vendor baru. Pemilik data rekening vendor masih `PBJ-OQ-020`. Menutup `PBJ-OQ-010` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara; `PBJ-FACT-003` |
| `PBJ-DEC-011` | Decision | **Penyerahan ke BLM secara manual dengan nomor rujukan.** Setelah penetapan vendor disetujui, Tim Pengadaan membuat PO di BLM, lalu mencatat nomor PO BLM pada paket di Quilvian. Paket berubah ke status "Sudah dipesan" hanya bila nomor PO dicatat. Saat salinan PO tersedia di Finance, keduanya dicocokkan lewat nomor PO. **Invariant:** satu nomor PO BLM tidak boleh dikaitkan ke dua paket. **Contoh:** paket PKT-2027-0031 (15 kursi roda, PT Sehat Medika) disetujui. Petugas membuat PO-2026-0412 di BLM dan mencatatnya di paket. Bila nomor yang sama dicatat di paket PKT-2027-0035, sistem menolak. Integrasi otomatis menyusul (`LATER SLICE`, mengikuti `FIN-OQ-141`). Menutup `PBJ-OQ-014` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara; `FIN-DEC-263` (nomor PO BLM dipertahankan) |
| `PBJ-DEC-012` | Decision | **SPK, Kontrak, Addendum, dan Arsip dimiliki Pengadaan; termin bersifat informatif.** Termin menyimpan jadwal, persentase/nilai, dan syarat (mis. BAST). Pengadaan **tidak** membuat utang dan tidak mengirim kejadian keuangan. Status bayar dibaca dari Finance. **Invariant:** jumlah nilai termin = nilai kontrak (setelah addendum terakhir yang disetujui). **Contoh:** kontrak pemeliharaan AC Rp 120 juta dibagi 3 termin @Rp 40 juta. Bila addendum menaikkan nilai menjadi Rp 132 juta, termin harus disesuaikan sampai jumlahnya Rp 132 juta sebelum addendum bisa diajukan. Menutup `PBJ-OQ-011` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-013` | Decision | **Pemeriksaan, BAST, dan NCR dicatat di Pengadaan.** Untuk barang, catatan merujuk nomor GR BLM tanpa mengubah GR. Untuk jasa, serah terima pekerjaan dan BAST sepenuhnya di Pengadaan. **Invariant:** paket tidak bisa ditutup bila ada NCR berstatus kritis yang masih terbuka, kecuali ada pengecualian yang disetujui sesuai matriks (`PBJ-DEC-007`). **Contoh:** GR-2026-0877 mencatat 15 kursi roda diterima. Pemeriksa menemukan 2 unit rusak, lalu membuat NCR kritis dengan tindak lanjut retur. Paket PKT-2027-0031 tidak bisa ditutup sampai NCR itu selesai. Retur fisik dan kreditnya tetap di BLM/Finance (`FIN-DEC-280`). Menutup `PBJ-OQ-012` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-014` | Decision | **PR tidak wajib merujuk rencana tahunan.** PR tanpa rencana ditandai "Di luar rencana", wajib beralasan, dan matriks approval boleh menambah jenjang untuk jenis ini. **Contoh:** Unit IGD mengajukan 2 monitor pasien yang tidak ada di rencana 2027, dengan alasan "unit lama rusak berat". PR tetap bisa diajukan, ditandai "Di luar rencana", dan muncul di laporan penyimpangan rencana. Menutup `PBJ-OQ-006` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-015` | Decision | **Rekening vendor tidak disimpan di Pengadaan.** Rekening tetap dikelola Finance/Master Data, karena pembayaran ada di sana. Submenu "Rekening Vendor" hanya **menampilkan** nomor tersamarkan (mis. `****1234`), nama bank, dan status verifikasi, dibaca dari pemiliknya. Pengadaan tidak bisa mengubah rekening. Bila sumber rekening tidak tersedia, submenu menampilkan "Data rekening dikelola Finance" tanpa data. Menutup `PBJ-OQ-020` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-016` | Decision | **Permintaan mendesak: jalur cepat dengan kontrol tetap.** PR mendesak wajib berisi alasan dan batas waktu kebutuhan, dan memakai baris matriks khusus "mendesak" (jenjang boleh lebih pendek). **Tidak ada persetujuan setelah kejadian**: pembelian tetap menunggu approval. Pemeriksaan hasil dan jejak audit tetap wajib. **Contoh:** IGD mengajukan PR mendesak untuk 1 defibrilator, butuh dalam 2 hari, alasan "unit satu-satunya rusak". Matriks mendesak hanya meminta Manajer Pengadaan (tanpa Kepala Unit). PR tanpa alasan ditolak saat diajukan. Menutup `PBJ-OQ-013` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-017` | Decision | **Pengguna sesi adalah Product Owner Pengadaan** yang berwenang menyetujui keputusan bisnis dan desain modul ini. Menutup `PBJ-OQ-017` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |
| `PBJ-DEC-018` | Decision | **Urutan: sub-modul `perencanaan-permintaan` dirancang dan dibangun pertama** (PR, verifikasi, cek stok manual, rencana dan pagu), bersama fondasi tingkat modul yang dibutuhkannya (matriks approval, audit, nomor dokumen, unggah berkas). `manajemen-vendor` boleh dirancang paralel. `pemilihan-penyedia` menyusul, lalu `kontrak-pelaksanaan`. Menutup `PBJ-OQ-016` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara; BRD B12 fase F1 |
| `PBJ-FACT-001`..`005` | Fact | Lihat `00-business-overview.md` bagian 3 | — | — | — | — |
| `PBJ-FACT-006` | Fact | **Hasil audit `01-existing-capability-map.md` revisi 1** (@`100c011d`/`763d665b`). `WorkflowManagement` terdaftar sebagai `SHARED PLATFORM CAPABILITY` prefix `Wfl`, tetapi hanya meneruskan hasil persetujuan ke jenis dokumen HR (`PBJ-CAP-001`). `MstSupplier` punya `IsBlacklisted` dan mengirim nomor rekening utuh (`PBJ-CAP-010`, `011`). Tidak ada master barang umum (`PBJ-CAP-012`) maupun layanan berkas bersama (`PBJ-CAP-004`) | — | — | — | `01-existing-capability-map.md` |

### Closure pass — 12 November 2026

| ID | Jenis | Keputusan | Pemilik | Status | Disetujui oleh/pada | Bukti |
|---|---|---|---|---|---|---|
| `PBJ-DEC-019` | Decision | **Mesin approval `Wfl` diperluas, bukan diduplikasi.** `WorkflowManagement` diberi titik daftar "penangan jenis dokumen", sehingga modul non-HR dapat mendaftarkan jenis dokumennya (mis. `PROCUREMENT_REQUEST`) beserta penangan yang mengubah status dokumen asal saat disetujui, ditolak, atau dikembalikan. Penangan HR yang sudah ada tidak berubah perilakunya. Pengadaan memakai matriks (`MstApprovalMatrix`), kotak masuk bersama, penjaga anti-setujui-sendiri, delegasi, dan SLA milik `Wfl`. **Akibat yang diterima:** setiap penyetuju Pengadaan wajib punya data pegawai yang terhubung ke akun pengguna. **Contoh:** PR-2027-00031 diajukan → `Wfl` membuat instance dengan `ReferenceType = PROCUREMENT_REQUEST` → Manajer Pengadaan menyetujui dari kotak masuk bersama → penangan Pengadaan mengubah PR menjadi `APPROVED`. Bila penanganan gagal, keputusan approval tetap sah dan kegagalan sinkron dicatat untuk diulang, mengikuti perilaku `Wfl` yang ada. **Menutup `PBJ-OQ-021` dan memenuhi syarat `PBJ-DEC-005`** | Pengguna sesi (juga pemilik `WorkflowManagement`) | `approved` | Pengguna sesi, 2026-11-12 | `PBJ-CAP-001`, `PBJ-CAP-002` |
| `PBJ-DEC-020` | Decision | **"Unit" pada PR, rencana, dan pagu = master organisasi HR** (`MstOrganizationUnit`/`MstDepartment`). Pembatasan data per unit memakai proyeksi `AspNetUserOrganization`. **Contoh:** pengguna yang ditempatkan di Departemen IGD hanya melihat PR milik IGD, kecuali perannya diberi cakupan lintas unit (mis. Tim Pengadaan, Auditor). Pengguna tanpa penempatan organisasi tidak melihat PR unit mana pun. Menutup `PBJ-OQ-022` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `PBJ-CAP-007`, `PBJ-CAP-008` |
| `PBJ-DEC-021` | Decision | **Status blokir vendor bersumber satu: `MstSupplier.IsBlacklisted`.** Pengadaan mengajukan blokir dan buka blokir lewat approval (`PBJ-DEC-007`). Riwayat, alasan, periode, dan penyetuju disimpan di tabel pendamping Pengadaan. Saat disetujui, hasil akhirnya menulis `IsBlacklisted` dan `BlacklistReason`. Status "Dibatasi" (masih bisa dipakai dengan peringatan) hanya ada di Pengadaan. **Contoh:** PT Alkes Maju diusulkan diblokir karena 3 NCR kritis dalam 6 bulan → disetujui Direktur → `IsBlacklisted = true` → vendor tidak bisa dipilih di RFQ Pengadaan dan tetap terlihat terblokir di Finance. Pengadaan diizinkan menulis kedua kolom itu hanya lewat jalur ini. Melengkapi `PBJ-DEC-010` dan menutup `PBJ-OQ-023` | Pengguna sesi (juga pemilik Master Data Supplier) | `approved` | Pengguna sesi, 2026-11-12 | `PBJ-CAP-010` |
| `PBJ-DEC-022` | Decision | **Katalog barang/jasa milik Pengadaan.** Isi: kode, nama, jenis (`PBJ-DEC-024`), kategori, satuan dari `MstMeasurement`, dan rujukan opsional ke `MstDrug` untuk obat. Baris rencana dan PR **wajib** memilih barang dari katalog. Spesifikasi tambahan per baris boleh berupa teks. Konsolidasi kebutuhan digabung per kode katalog, tidak pernah per nama. **Contoh:** IGD meminta 3 "Kursi Roda Standar" (KTL-00045) dan Rawat Inap 5 unit KTL-00045 → konsolidasi 8 unit KTL-00045, asal tiap unit tetap tercatat. Barang yang belum ada di katalog diajukan sebagai katalog baru oleh Tim Pengadaan. Menutup `PBJ-OQ-024` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `PBJ-CAP-012`; selaras `FIN-DEC-275` (tidak mencocokkan berdasarkan nama) |
| `PBJ-DEC-023` | Decision | **Penyimpanan berkas milik Pengadaan.** Satu tabel lampiran yang menempel ke dokumen Pengadaan mana pun (PR, paket, vendor, kontrak, BAST, NCR). Setiap berkas punya versi, dan berkas lama tidak ditimpa diam-diam. **Aturan keamanan wajib (BRD B6):** daftar tipe yang diizinkan diperiksa dari ekstensi **dan** isi berkas; ada batas ukuran; nama berkas yang disimpan diacak; unduhan hanya lewat endpoint yang memeriksa hak akses atas dokumen induknya. **Contoh:** berkas `penawaran.html` atau `.exe` yang diganti namanya menjadi `.pdf` ditolak, karena isinya bukan PDF. Daftar tipe dan ukuran maksimum ditetapkan saat desain. Menutup `PBJ-OQ-025` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `PBJ-CAP-004` |
| `PBJ-DEC-024` | Decision | **Empat jenis pengadaan: barang stok, aset, barang nonstok, jasa**, disimpan sebagai data sehingga bisa ditambah. Jenis menentukan dokumen wajib dan jalur serah terima: barang (stok, aset, nonstok) lewat GR BLM + pemeriksaan Pengadaan (`PBJ-DEC-013`); jasa lewat serah terima pekerjaan dan BAST di Pengadaan. Menutup `PBJ-OQ-007` bagian jenis | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | BRD A7.5 |
| `PBJ-DEC-025` | Decision | **[Bagian format `superseded` oleh `PBJ-DEC-031`]** **Format nomor seragam `<KODE>-<TAHUN>-<urutan 5 digit>`, deret direset tiap tahun**, dikelola lewat Number Series Platform (`PBJ-CAP-003`). Kode usulan: `PR`, `RKB` (rencana), `PKT` (paket), `RFQ`, `SPK`, `KTR` (kontrak), `ADD` (addendum), `BAST`, `NCR`, `KTL` (katalog, tanpa tahun). Nomor dibuat server saat dokumen **diajukan**, bukan saat draft, dan tidak dipakai ulang bila dokumen batal. **Contoh:** PR ketiga puluh satu yang diajukan pada 2027 bernomor `PR-2027-00031`; bila dibatalkan, nomor itu tidak dipakai PR lain. Menutup `PBJ-OQ-007` bagian nomor | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `PBJ-CAP-003`; `DEC-PLT-008` (nomor hangus) |
| `PBJ-DEC-026` | Decision | **Wewenang lintas modul:** pengguna sesi menyatakan diri sebagai pemilik `WorkflowManagement` dan Master Data Supplier, selain Product Owner Pengadaan (`PBJ-DEC-017`). Karena itu `PBJ-DEC-019` dan `PBJ-DEC-021` disetujui langsung. **Catatan audit:** registry tidak mencatat nama pemilik kedua modul itu. Pernyataan ini bersumber dari jawaban sesi dan sebaiknya dicatat juga di blueprint `human-resource`/Master Data oleh pemiliknya | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban wawancara |

### Amendment pass — penutupan blocker gerbang kelengkapan, 12 November 2026

| ID | Jenis | Keputusan | Pemilik | Status | Disetujui oleh/pada | Bukti |
|---|---|---|---|---|---|---|
| `PBJ-DEC-027` | Decision | **Urutan PR: verifikasi dan cek stok sebelum approval.** Alurnya: pengusul mengajukan → Tim Pengadaan memverifikasi kelengkapan dan mencatat cek stok per baris (`PBJ-DEC-008`) → server menghitung kebutuhan beli → approval matriks memakai **nilai kebutuhan beli** sebagai nominal penentu jenjang. Bila seluruh baris dipenuhi dari stok, PR selesai sebagai "Dipenuhi dari stok" tanpa approval pembelian. **Contoh:** PR 20 kursi roda @Rp 3.000.000, dengan 5 unit dari stok → kebutuhan beli 15 unit = Rp 45.000.000 → jenjang dipilih untuk Rp 45 juta, bukan Rp 60 juta. Pencatat cek stok = Tim Pengadaan. Menutup `PBJ-OQ-026` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `02-requirement-completeness-assessment.md` `G-01` |
| `PBJ-DEC-028` | Decision | **Rencana paket dan paket pengadaan adalah dua entity.** Rencana paket (submenu 2.4) dimiliki `perencanaan-permintaan`, sebagai hasil pengelompokan baris rencana tahunan. Paket eksekusi dimiliki `pemilihan-penyedia` dan dibuat **dari** rencana paket dan/atau PR yang disetujui, dengan rujukan balik yang tetap tersimpan. **Contoh:** rencana paket "Alat Bantu Mobilitas 2027" berisi 30 kursi roda dari 3 unit. Saat PR-PR terkait disetujui, Tim Pengadaan membuat paket PKT-2027-00012 dari PR-PR itu. Laporan dapat membandingkan rencana paket (30 unit) dengan yang dieksekusi. Menutup `PBJ-OQ-027` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `G-06`; `bentuk-blueprint.md` 4.2 |
| `PBJ-DEC-029` | Decision | **PR mengurangi sisa kuantitas baris rencana; kelebihan ditandai per baris.** Bagian PR yang melebihi sisa rencana ditandai "Di luar rencana" dan wajib beralasan (`PBJ-DEC-014`). PR yang batal atau ditolak mengembalikan sisanya. **Contoh:** rencana IGD 2027 = 10 monitor. PR-1 (6 unit) disetujui, sisa 4. PR-2 (6 unit) tercatat 4 unit dalam rencana dan 2 unit di luar rencana. Bila PR-1 kemudian dibatalkan sebelum masuk paket, sisa kembali menjadi 10 − 4 = 6. Pengurangan dihitung di server. Menutup `PBJ-OQ-028` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `G-02` |
| `PBJ-DEC-030` | Decision | **PR melebihi sisa pagu tetap boleh diajukan**, ditandai "Melebihi pagu", dan matriks dapat menambah jenjang khusus untuk kondisi ini (dapat dikonfigurasi). Pagu belum terikat saat PR; komitmen tetap baru tercatat saat penetapan vendor (`PBJ-DEC-009`). **Contoh:** sisa pagu Radiologi Rp 30 juta, kebutuhan beli PR Rp 45 juta → PR diberi tanda "Melebihi pagu Rp 15.000.000" dan matriks menambah persetujuan Direktur Keuangan. Menutup `PBJ-OQ-029` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `G-03` |

### Penutupan desain dan approval `perencanaan-permintaan` — 12 November 2026

| ID | Jenis | Keputusan | Pemilik | Status | Disetujui oleh/pada | Bukti |
|---|---|---|---|---|---|---|
| `PBJ-DEC-031` | Decision | **Nomor mengikuti invariant Platform `INV-PLT-004`:** deret Pengadaan tidak pernah direset (`NEVER`), 6 digit, tanpa tahun. **Contoh:** `PR-000031`, `RKB-000004`, `KTL-000045`. **Menggantikan bagian format `PBJ-DEC-025`** (pola `<KODE>-<TAHUN>-<urut>` dan reset tahunan); bagian lain `PBJ-DEC-025` tetap berlaku (nomor dibuat saat diajukan, tidak dipakai ulang). Menutup `PBJ-OQ-031` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `perencanaan-permintaan/02-backend-architecture.md` bagian 9 |
| `PBJ-DEC-032` | Decision | **Grup menu tambahan "Pengaturan Pengadaan"** di luar 11 menu, berisi Katalog Barang/Jasa serta Kategori & Sumber Dana. Daftar 61 submenu SOURCE (`PBJ-FE-001`) tetap utuh. Menutup `PBJ-OQ-032` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `02-module-map.md` bagian 3 |
| `PBJ-DEC-033` | Decision | **Usulan standar dikonfirmasi apa adanya:** `G-04` (pembatalan/penarikan PR; tolak duplikat oleh Tim Pengadaan; dokumen final tidak dihidupkan lagi), `G-05` (siklus rencana, rencana paket, pagu dengan revisi berversi), `G-07` (katalog dikelola Tim Pengadaan), `G-08` (izin lintas unit untuk Tim Pengadaan, penyetuju, Keuangan, Auditor), `G-11` (notifikasi `Wfl`, tanpa nominal) | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | `02-requirement-completeness-assessment.md` bagian 3 |
| `PBJ-DEC-034` | Decision | **Approval desain sub-modul `perencanaan-permintaan`** (11 berkas + bagiannya pada `02-module-map.md`, kontrak set `1.0`) dengan `PBJ-DEC-031`..`033` diterapkan. Persetujuan ini **tidak** memberi wewenang membuat atau menerapkan migration, menulis kode, maupun mendaftarkan registry; wewenang itu tetap per task | Pengguna sesi (Product Owner Pengadaan) | `approved` | Pengguna sesi, 2026-11-12 | Jawaban eksplisit "Setujui, lanjut plan-module-delivery" |

### Registry prefix — 12 November 2026

| ID | Jenis | Keputusan | Pemilik | Status | Disetujui oleh/pada | Bukti |
|---|---|---|---|---|---|---|
| `PBJ-DEC-035` | Decision | **Prefix entity `Prc` = *Procurement* dan Area `Corporate / Procurement` didaftarkan `ACTIVE`** di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`: baris `ProcurementManagement / Procurement` ditambah tiga submodule eksplisit `Planning`, `Request`, `Common`. Folder `ProcurementManagement/MasterData` tidak didaftarkan terpisah karena model `Mst*` di sana tercakup baris `Master / Reference / MasterData`. Simulasi pencocokan checker atas 949 model existing: 0 berubah. Menutup `PBJ-OQ-002`. Wewenang penamaan saja, bukan implementasi | Pengguna sesi (Product Owner Pengadaan, pemegang registry untuk modul ini) | `approved` | Pengguna sesi, 2026-11-12 | Jawaban "Ya, daftarkan Prc"; registry baris baru dan catatan perubahan 2026-11-12 |
| `PBJ-DEC-036` | Decision | **Izin "lihat semua unit" berbentuk aksi `ReadAllUnits` per resource dokumen**, menggantikan resource tunggal `ProcurementCrossUnit : Read` pada `PBJ-PP-PERM 1.0`. Sebab: mekanisme `AccessExplicitPermission` repository hanya menambah aksi pada resource yang sudah punya endpoint, sehingga resource tanpa endpoint tidak dapat didaftarkan (ditemukan saat `BE-PBJ-002`). Aksinya: `ProcurementRequest : ReadAllUnits`, `ProcurementNeedPlan : ReadAllUnits`, `ProcurementBudget : ReadAllUnits`, `ProcurementPackagePlan : ReadAllUnits`, didaftarkan oleh task yang membuat controller dokumennya (`BE-PBJ-006`, `009`, `010`, `011`). **Contoh:** Keuangan diberi `ProcurementBudget : ReadAllUnits` sehingga melihat pagu semua unit, tanpa ikut melihat PR semua unit. Kontrak hak akses naik ke `PBJ-PP-PERM 1.1` | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban "Aksi ReadAllUnits per dokumen"; `BE/Attributes/AccessExplicitPermissionAttribute.cs` |
| `PBJ-DEC-037` | Decision | **Satu controller per master, masing-masing 9 endpoint baku** (standar master data repository): Katalog `ProcurementItem`, Kategori `ProcurementItemCategory`, Sumber Dana `ProcurementFundSource`, masing-masing dengan izin `Read`/`Create`/`Update`/`Delete`. Jenis Pengadaan `ProcurementType` **hanya-baca** lewat API (jenis menentukan alur serah terima dan diisi lewat data awal, `PBJ-DEC-024`). Resource `ProcurementMasterData` dihapus. Sebab: aturan wajib "argumen ke-1 `[AccessPermission]` = `ControllerName`" membuat controller "Reference" gabungan pada API 1.0 menghasilkan 403 permanen (ditemukan saat `BE-PBJ-003`). Kontrak naik ke `PBJ-PP-API 1.1` dan `PBJ-PP-PERM 1.2`. **Contoh:** petugas Keuangan yang menyusun pagu memerlukan `ProcurementItemCategory : Read` dan `ProcurementFundSource : Read` untuk mengisi pilihan kategori dan sumber dana | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban "Satu controller per master"; `rules/backend/role-access-rules.md`, `master-data-endpoint-standard.md` |

### Mesin approval atas nama pemohon — 12 November 2026

| ID | Jenis | Keputusan | Pemilik | Status | Disetujui oleh/pada | Bukti |
|---|---|---|---|---|---|---|
| `PBJ-DEC-038` | Decision | **Mesin approval bersama (Wfl) diperluas dengan "buat & ajukan atas nama pemohon"** (`WorkflowService.CreateAndSubmitOnBehalfAsync`). Sebab: PR baru masuk approval saat **verifikator** menyelesaikan verifikasi (`PBJ-DEC-027`), padahal Wfl membaca pengaju dari pengguna yang sedang login, sehingga verifikator tercatat sebagai pengaju dan jenjang "atasan pemohon" menunjuk atasan verifikator. Aturannya: pemohon asli diambil dari PR, kunci idempoten wajib (`<jenis>:<id>:<ke-berapa diajukan>`), urutan "Wfl dulu, baru status dokumen" sehingga kegagalan Wfl tidak meninggalkan PR setengah jadi, dan pemohon tetap dilarang menyetujui pengajuannya sendiri (`AllowSelfApproval=false` + pemeriksaan ulang di penangan lifecycle). Definisi alur `PBJ-PR`, `PBJ-PR-MENDESAK`, `PBJ-RKB`, `PBJ-RPK`, `PBJ-PAGU` di-seed **tidak aktif** tanpa baris matriks; admin mengaktifkan setelah matriks diisi (fail-closed). **Contoh:** Rina (IGD) membuat PR; Budi (Tim Pengadaan) menyelesaikan verifikasi; instance approval tercatat diajukan oleh Rina, dan langkah pertama menunggu atasan Rina, bukan atasan Budi | Pengguna sesi | `approved` | Pengguna sesi, 2026-11-12 | Jawaban "Perluas Wfl: buat & ajukan atas nama (Direkomendasikan)"; `BE-PBJ-005` |

## 4. Kewenangan keputusan frontend

| ID | Area | Pemilik | Status | Rentang yang diizinkan | Bukti |
|---|---|---|---|---|---|
| `PBJ-FE-001` | Nama 11 menu dan 61 submenu | Pengguna sesi | `approved` | Tetap sesuai daftar SOURCE; tampil atau tidaknya mengikuti hak akses | BRD B10, `PBJ-DEC-003` |
| `PBJ-FE-002` | Route, layout, tab/modal/drawer, warna | Developer | `DEV_DISCRETION` | Mengikuti konvensi dan design token Quilvian yang ada | Tidak ada brief UI |
| `PBJ-FE-003` | Keamanan tampilan | — | Invariant | Rekening selalu tersamarkan; tombol tersembunyi bukan pengganti cek hak akses di backend | BRD B6, `PBJ-DEC-015` |

## 5. Pertanyaan terbuka dan blocker

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `PBJ-OQ-005` | Daftar metode pengadaan dan batas nilainya | Pengguna sesi, dari SOP | `DESIGN` nilai awal `pemilihan-penyedia`; tidak memblokir `perencanaan-permintaan` |
| `PBJ-OQ-008` | Sumber stok untuk integrasi | Pemilik stok (belum diketahui) | `LATER SLICE` (`PBJ-DEC-008`) |
| `PBJ-OQ-015` | SLA per tahap, kalender kerja, retensi dokumen, notifikasi | Pengguna sesi | `DESIGN` dashboard "Pengadaan Tertunda" dan keterlambatan |
| `PBJ-OQ-019` | Dokumen SOP pengadaan internal (nilai awal matriks, metode, dokumen wajib) | Pengguna sesi | Nilai konfigurasi awal, bukan struktur |
| ~~`PBJ-OQ-001`, `003`, `004`, `006`, `007`, `009`..`014`, `016`..`018`, `002`, `020`..`029`, `031`, `032`~~ | Ditutup oleh `PBJ-DEC-004`..`PBJ-DEC-035` | — | — |

## 6. Kriteria penerimaan yang sudah dapat diuji

| ID | Kriteria |
|---|---|
| `PBJ-AC-001` | Modul Pengadaan tidak menyediakan aksi membuat, mengubah, atau menyetujui PO maupun GR. **Contoh:** tidak ada endpoint pembuatan PO di grup API Pengadaan. |
| `PBJ-AC-002` | Pengusul tidak dapat menyetujui dokumennya sendiri pada jenjang mana pun. **Contoh:** Kepala Unit A membuat PR, lalu mencoba menyetujuinya → ditolak dengan pesan "Anda tidak dapat menyetujui pengajuan Anda sendiri" (kode 403). |
| `PBJ-AC-003` | Jenjang dan nominal yang berlaku tercatat saat approval dan tidak berubah meski matriks diubah kemudian. |
| `PBJ-AC-004` | Kebutuhan beli = jumlah diminta − jumlah dipenuhi dari stok, dihitung di server. **Contoh:** 20 − 5 = 15. |
| `PBJ-AC-005` | PR "Di luar rencana" atau "Mendesak" tanpa alasan ditolak saat diajukan. |
| `PBJ-AC-006` | Satu nomor PO BLM hanya boleh tercatat pada satu paket. |
| `PBJ-AC-007` | Jumlah nilai termin selalu sama dengan nilai kontrak yang berlaku. |
| `PBJ-AC-008` | Paket tidak dapat ditutup selama ada NCR kritis yang terbuka tanpa pengecualian yang disetujui. |
| `PBJ-AC-009` | Rekening vendor hanya tampil tersamarkan dan tidak dapat diubah dari modul Pengadaan. |
| `PBJ-AC-010` | Sisa pagu = pagu − komitmen yang disetujui. **Contoh:** Rp 500 juta − Rp 120 juta = Rp 380 juta. |

## 7. Ringkasan pass

- **Scope pass** (`PBJ-DEC-001`..`018`) dan **closure pass** (`PBJ-DEC-019`..`026`) selesai pada 12 November 2026.
- **Pertanyaan tersisa:** `PBJ-OQ-002` (registry prefix, memblokir `IMPLEMENTATION`), `PBJ-OQ-005` (metode, untuk `pemilihan-penyedia`), `PBJ-OQ-008` (`LATER SLICE`), `PBJ-OQ-015` (SLA), `PBJ-OQ-019` (nilai awal SOP). Tidak ada yang memblokir desain struktur `perencanaan-permintaan`.
- **Amendment pass** (`PBJ-DEC-027`..`030`) menutup blocker gerbang kelengkapan.
- **Langkah berikutnya (diperbarui):** `design-business-module` untuk sub-modul `perencanaan-permintaan`. Sebelumnya: `requirement-completeness-gate` untuk sub-modul `perencanaan-permintaan` beserta fondasi tingkat modul (perluasan `Wfl`, katalog, lampiran, nomor).
