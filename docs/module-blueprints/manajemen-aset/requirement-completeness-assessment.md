# Manajemen Aset & Inventaris — Penilaian Kelengkapan Requirement

| Field | Value |
|---|---|
| Assessment ID | `AST-RCG-001` |
| Revision | `1` (2026-11-12) |
| Blueprint | `AST-BP-001` revision `1`, bentuk `COMPOSITE` |
| Masukan | `00-interview-decisions.md` decision revision `3` (`AST-DEC-001..046`); `01-existing-capability-map.md` revisi `1.0` |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |
| Baseline rujukan RS Indonesia | Tidak dipakai — modul non-klinis dengan aturan yang seluruhnya dijawab pemilik; `baseline_reference_coverage: NOT_USED` |
| Hasil | **Seluruh slice MVP `READY_FOR_DOMAIN_DESIGN`.** Satu slice (`N6` Revaluasi) sengaja `DEFERRED` ke gelombang lanjutan dan berstatus `BUSINESS_DECISION_REQUIRED` |

---

## 1. Scope penilaian

Satuan penilaian adalah **slice proses**, bukan menu. Satu slice dapat melayani beberapa butir menu.

| Sub-modul | Slice | Kemampuan (`CAP-*`) |
|---|---|---|
| `register` | `R1` Master klasifikasi dan lokasi | 02.1–02.6 |
| | `R2` Registrasi, nomor, label, dokumen kepemilikan | 03.1–03.6 |
| | `R3` Penempatan dan penanggung jawab | 04.1–04.4 |
| | `R4` Kondisi dan status | 06.1–06.6 |
| | `R5` Pemanfaatan dan pengamanan | 08.1–08.5 |
| | `R6` Impor data awal | `AST-DEC-023`, `037` |
| | `R7` Dashboard dan laporan register | 01.1–01.3, 01.6, 11.1, 11.3, 11.7 |
| `pergerakan` | `P1` Mutasi | 05.1–05.3 |
| | `P2` Peminjaman dan pengembalian | 05.4–05.5 |
| | `P3` Dashboard dan laporan | 01.4, 11.2 |
| `inventarisasi` | `I1` Jadwal dan tim | 07.1 |
| | `I2` Pemeriksaan fisik dan pencocokan | 07.2–07.3 |
| | `I3` Selisih, verifikasi, berita acara, tindak lanjut | 07.4–07.6 |
| | `I4` Dashboard dan laporan | 01.5, 11.4 |
| `nilai` | `N1` Parameter nilai | `AST-DEC-027` |
| | `N2` Penyusutan bulanan dan pengiriman | 09.2, 09.4 |
| | `N3` Perubahan nilai (perbaikan besar, koreksi, penurunan nilai) | 09.1, 09.3 |
| | `N4` Perolehan hibah | `AST-DEC-040` |
| | `N5` Rekonsiliasi | 09.5 |
| | `N6` Revaluasi | 09.3 (sebagian) — `DEFERRED` (`AST-DEC-032`) |
| | `N7` Laporan nilai | 11.6 |
| `penghapusan` | `H1` Usulan, kelayakan, persetujuan | 10.1–10.3 |
| | `H2` Pemindahtanganan dan berita acara | 10.4–10.5 |
| | `H3` Pemulihan Aset | `AST-DEC-028` |
| | `H4` Riwayat dan laporan | 10.6, 11.5 |

## 2. Bukti yang dipakai

| Wewenang | Bukti | Untuk pertanyaan |
|---|---|---|
| 1. Requirement eksplisit pemilik | Daftar 11 menu (baseline) dan 46 jawaban pemilik pada 11 putaran pertanyaan pilihan | Apa yang harus dibangun |
| 3. Keputusan terkonfirmasi modul tetangga | `accounting/00-interview-decisions.md` `ACC-OOS-007`, `ACC-DEC-045..050`, `ACC-DEC-061`, `ACC-DEC-066` — Rizki juga pemilik Akuntansi | Cara Akuntansi menerima kejadian |
| 6. Implementasi V2 | `01-existing-capability-map.md` `TS-01..TS-25` | Apa yang sudah ada |

Tidak ada SOP tertulis rumah sakit yang diserahkan. Seluruh aturan bersumber dari jawaban pemilik.

---

## 3. Temuan per dimensi

Kode: **C** = `CONFIRMED`, **P** = `PROPOSED`, **M** = `MISSING`, **X** = `CONFLICT`, **NA** = tidak
material (alasan di bawah tabel). Rujukan `DEC-` berarti `AST-DEC-`; `G` berarti gap pada bagian 4.

| # | Dimensi | `register` | `pergerakan` | `inventarisasi` | `nilai` | `penghapusan` |
|---|---|---|---|---|---|---|
| 01 | Tujuan | C (bagian 2 decision log) | C | C | C | C |
| 02 | Aktor | C (bagian 4) | C (DEC-011, 012) | C (DEC-018); P (G22) | C (DEC-027, 030) | C (DEC-020); P (G22) |
| 03 | Pemicu/prasyarat | C (DEC-008, 043) | C | C (DEC-018) | C (DEC-014, 039) | C (DEC-020, 045) |
| 04 | Alur utama | C (DEC-043, 044) | C (DEC-011, 012) | C (DEC-018, 019) | C (DEC-039) | C (DEC-020, 022, 045) |
| 05 | Alternatif/exception | C (DEC-017); P (G10, G23) | C (RULE-012, 023); P (G05, G06, G07) | P (G10, G11) | C (SCN-07..09); P (G16) | C (DEC-045); P (G23) |
| 06 | Data minimum | C (DEC-026); P (G14, G25, G26) | P (desain) | P (desain) | C (DEC-013) | C (DEC-022) |
| 07 | Aturan/validasi | C (RULE-001..007, 010..014) | C (RULE-020..023) | C (RULE-030..032) | C (RULE-040..048); P (G17, G19, G27) | C (RULE-050..052) |
| 08 | Status | C (DEC-017, 043, 044) | C (bagian 6) | C (bagian 6) | C (DEC-030, 039) | C (bagian 6, DEC-045) |
| 09 | Peran/authorization | C (DEC-025); P (G22) | C (DEC-011); M→data (`AST-UNK-004`) | C | C (DEC-027, 030) | C (DEC-020) |
| 10 | Dependency antarmodul | C (DEC-034; `TS-01..04`) | C (`TS-04`) | C | C (DEC-015, 038, 041) | C (DEC-022) |
| 11 | Integrasi | C (`TS-07`) | NA | NA | C (`TS-08`, `TS-09`) | C (`TS-08`) |
| 12 | Hasil akhir | C | C (RULE-021) | C (DEC-019) | C | C |
| 13 | Pembatalan/koreksi | C (DEC-028, 043) | C (DEC-028); P (G05, G06) | P (G06) | C (DEC-042) | C (DEC-028) |
| 14 | Audit/histori | C (RULE-060); P (G29) | C | C | C | C |
| 15 | Notifikasi | P (G21) | P (G21) | P (G21) | P (G21) | P (G21) |
| 16 | Dampak finansial | C (DEC-040 hibah) | NA | NA | C (DEC-015, 036, 038..042) | C (RULE-051, DEC-045) |
| 17 | Keselamatan klinis | NA | NA | NA | NA | NA |
| 18 | Pelaporan/traceability | C (menu 11) | C | C | C | C |

**Alasan NA.**

- Dimensi 11 untuk `pergerakan` dan `inventarisasi`: kedua sub-modul tidak berhubungan dengan sistem
  luar maupun modul lain selain register milik modul ini.
- Dimensi 16 untuk `pergerakan` dan `inventarisasi`: tidak ada akibat keuangan langsung. Mutasi
  memengaruhi pusat biaya penyusutan, tetapi aturannya milik `nilai` (`AST-RULE-046`).
- Dimensi 17: modul tidak mengatur pemakaian klinis alat. Kalibrasi dan pemeliharaan berada di luar
  cakupan (`AST-DEC-005`), dan status "Rusak Berat" pada alat medis tidak mencegah pemakaian klinis
  lewat sistem. **Konsekuensi yang perlu diketahui pemilik:** modul ini tidak menjadi pengaman
  keselamatan alat medis; yang ada hanya penanda dan daftar di dashboard.

---

## 4. Register gap

### 4.1 Gap `BLOCKING` — sudah ditutup pada sesi yang sama

| ID | Gap | Mengapa memblokir | Status |
|---|---|---|---|
| `AST-RCG-G01` | Persetujuan pencatatan aset baru belum diputuskan, padahal `AST-DEC-040` menyebut "sesudah aset disetujui" | Mengubah status aset dan pemicu kejadian hibah | **Ditutup `AST-DEC-043`** |
| `AST-RCG-G02` | Konfirmasi penempatan pertama dan Berita Acara Penyerahan belum diputuskan | Mengubah status penempatan dan pemilik tindakan | **Ditutup `AST-DEC-044`** |
| `AST-RCG-G03` | Penghapusan aset Hilang tidak cocok dengan keempat jenis pelepasan | Mengubah alur penghapusan dan isi kejadian Akuntansi | **Ditutup `AST-DEC-045`** |
| `AST-RCG-G04` | Keabsahan berita acara (persetujuan sistem atau tanda tangan basah) | Mengubah syarat "selesai" pada lima proses | **Ditutup `AST-DEC-046`** |

### 4.2 Gap `NON_BLOCKING_STANDARD` dan `CONFIGURABLE_DEFAULT`

Usulan di bawah dipakai desain sebagai **usulan yang terlihat**, bukan kebijakan terkonfirmasi.
Setiap butir tetap `PROPOSED` sampai pemilik menyetujui desain.

| ID | Gap | Status | Dampak | Usulan desain | Contoh |
|---|---|---|---|---|---|
| `AST-RCG-G05` | Siapa boleh membatalkan mutasi sebelum serah terima | P | NON_BLOCKING_STANDARD | Pengaju atau Kepala Bagian Aset, wajib alasan, hanya sebelum serah terima dikonfirmasi | Mutasi IGD → ICU dibatalkan pengaju pada tahap "Disetujui unit asal" dengan alasan "ICU sudah dapat dari vendor" |
| `AST-RCG-G06` | Akibat penolakan | P | NON_BLOCKING_STANDARD | Penolakan pada tahap mana pun mengakhiri pengajuan (`Ditolak`, final, wajib alasan); bila perlu, pengajuan baru dibuat | — |
| `AST-RCG-G07` | Siapa mengonfirmasi pengembalian pinjaman | P | NON_BLOCKING_STANDARD | Kepala unit pemilik mengonfirmasi penerimaan kembali sekaligus memeriksa kondisi; bila kondisi berbeda, sistem membuka laporan kondisi (`AST-RULE-010`) | Syringe pump kembali dengan kabel putus → laporan Rusak Ringan otomatis terbuka |
| `AST-RCG-G08` | Batas lama pinjaman dan jumlah perpanjangan | P | CONFIGURABLE_DEFAULT | Parameter opsional; bawaan tanpa batas | — |
| `AST-RCG-G09` | Peminjaman oleh pihak luar RS | P | NON_BLOCKING_STANDARD | Peminjaman hanya antar unit internal; pemakaian pihak luar adalah pemanfaatan (`AST-DEC-021`) | Klinik mitra meminjam USG → permohonan pemanfaatan pinjam pakai, bukan peminjaman |
| `AST-RCG-G10` | Tindak lanjut temuan "tidak terdaftar" saat opname | P | NON_BLOCKING_STANDARD | Dicatat sebagai temuan beserta foto; Petugas Aset dapat membuat draft aset baru dari temuan itu (`AST-DEC-043`) | Kursi tanpa label di IGD → temuan → draft aset |
| `AST-RCG-G11` | Aset yang sedang dipinjam saat opname | P | NON_BLOCKING_STANDARD | Diharapkan ditemukan di unit peminjam; hasil "sesuai (sedang dipinjam)" | — |
| `AST-RCG-G13` | Definisi aset menganggur | P | CONFIGURABLE_DEFAULT | Berstatus Tidak Digunakan lebih dari N hari; bawaan 90 hari | Tidak Digunakan sejak 1 Februari → tampil di Pemantauan pada 2 Mei |
| `AST-RCG-G14` | Isi pengamanan administrasi | P | NON_BLOCKING_STANDARD + CONFIGURABLE_DEFAULT | Daftar jenis dokumen wajib per kategori aset (diatur di master), nomor, masa berlaku, dan peringatan sebelum habis (bawaan 30 hari). Polis asuransi dicatat sebagai salah satu jenis dokumen | Kendaraan wajib BPKB dan STNK; STNK habis 15 Desember → peringatan mulai 15 November |
| `AST-RCG-G15` | Aset yang sedang dimanfaatkan pihak ketiga | P | NON_BLOCKING_STANDARD | Tidak bisa dimutasi atau dihapus selama perjanjian berjalan; tetap disusutkan; pendapatan sewa ditagih Finance (di luar cakupan) | — |
| `AST-RCG-G16` | Penyusutan aset Tidak Digunakan, Rusak, atau Hilang | P | NON_BLOCKING_STANDARD | Tetap disusutkan sampai penghapusan final (standar akuntansi aset tetap tidak menghentikan penyusutan saat menganggur) | — |
| `AST-RCG-G17` | Perubahan umur/residu untuk satu aset | P | NON_BLOCKING_STANDARD | Diajukan sebagai perubahan nilai (`AST-DEC-030`), disetujui Kepala Akuntansi, berlaku ke depan: sisa nilai tersusutkan ÷ sisa umur baru | Nilai buku Rp 51.000.000 sesudah 24 bulan, umur diubah 60 → 72 bulan: 51.000.000 ÷ 48 = Rp 1.062.500 per bulan |
| `AST-RCG-G18` | Pembalikan penurunan nilai | P | NON_BLOCKING_STANDARD | Tidak didukung pada MVP | — |
| `AST-RCG-G19` | Penyusutan aset hasil impor saldo awal | P | NON_BLOCKING_STANDARD | Sisa umur = umur − bulan sejak siap pakai sampai tanggal awal; penyusutan per bulan = (harga − akumulasi − residu) ÷ sisa umur | Harga Rp 60.000.000, akumulasi Rp 24.000.000, umur 60, sudah 24 bulan → 36.000.000 ÷ 36 = Rp 1.000.000 |
| `AST-RCG-G20` | Saat dan isi rekonsiliasi | P | NON_BLOCKING_STANDARD | Sesudah penyusutan bulan diproses, Aset mengirim saldo subledger per akun kontrol (harga perolehan, akumulasi penyusutan, akumulasi penurunan nilai); selisih ditindaklanjuti Akuntansi (`TS-09`) | — |
| `AST-RCG-G21` | Notifikasi | P | NON_BLOCKING_STANDARD | Daftar tugas penyetuju di aplikasi dan angka di dashboard; tanpa email/WhatsApp pada MVP | — |
| `AST-RCG-G22` | Penunjukan tim inventarisasi dan tim pemeriksa kelayakan | P | NON_BLOCKING_STANDARD | Dipilih Kepala Bagian Aset dari daftar pegawai per sesi/usulan; anggota tim tidak boleh memverifikasi hasilnya sendiri (`AST-ASM-003`) | — |
| `AST-RCG-G23` | Aset yang terlibat dua proses bersamaan | P | NON_BLOCKING_STANDARD | Satu aset hanya boleh punya satu proses aktif yang saling meniadakan (mutasi, pinjaman, pemanfaatan, usulan penghapusan). Matriksnya ditetapkan desain | Aset dengan usulan penghapusan berjalan tidak bisa diajukan mutasi |
| `AST-RCG-G24` | Tukar tambah | P | NON_BLOCKING_STANDARD | Harga pelepasan = nilai tukar yang disepakati; aset pengganti dicatat sebagai aset baru lewat pembelian | — |
| `AST-RCG-G25` | Isi jenis kepemilikan | P | NON_BLOCKING_STANDARD | Jenis kepemilikan menentukan boleh atau tidaknya dikapitalisasi: Milik Sendiri dan Hibah boleh; Sewa, KSO, dan Pinjam Pakai masuk hanya dilacak. Akuntansi hak guna (sewa) di luar cakupan | Alat hemodialisa KSO vendor dilacak lokasinya, tidak disusutkan RS |
| `AST-RCG-G26` | Hubungan kategori, kelompok, dan kode inventaris | P | NON_BLOCKING_STANDARD | Kategori (Alat Medis, Kendaraan, Gedung, Tanah, Mebel, …) → Kelompok (umur, residu, akun) → Kode Inventaris = kode singkat kelompok (≤ 11 karakter) yang masuk nomor | Kategori Alat Medis → Kelompok "Alat Kedokteran Umum" kode `ALKMED` |
| `AST-RCG-G27` | Tanah | P | NON_BLOCKING_STANDARD | Kategori Tanah ditandai tidak disusutkan | — |
| `AST-RCG-G29` | Rincian riwayat | P | NON_BLOCKING_STANDARD | Setiap perubahan status, lokasi, penanggung jawab, dan nilai menyimpan pelaku, waktu, alasan, dan nilai sebelum-sesudah | — |

### 4.3 Masih `MISSING` tetapi hanya menahan implementasi/go-live

`AST-OQ-001..004`, `006`, `009`, `015`, `016`, `AST-UNK-002`, `AST-UNK-004` — rincian di decision log
bagian 13 dan 15. Tidak ada yang mengubah struktur data atau siklus status.

---

## 5. Kesiapan per slice

| Slice | Kesiapan | Catatan |
|---|---|---|
| `R1`–`R7` | `READY_FOR_DOMAIN_DESIGN` | `R2` memakai format `AST-DEC-033` |
| `P1`–`P3` | `READY_FOR_DOMAIN_DESIGN` | Uji persetujuan unit asal bergantung data `AST-UNK-004` |
| `I1`–`I4` | `READY_FOR_DOMAIN_DESIGN` | — |
| `N1`–`N5`, `N7` | `READY_FOR_DOMAIN_DESIGN` | Penyalaan pengiriman bergantung `AST-UNK-002` dan `AST-OQ-009` |
| `N6` Revaluasi | `BUSINESS_DECISION_REQUIRED` — `DEFERRED` | Butuh kebijakan revaluasi per kelompok, akun surplus, dan laporan penilai. Didesain kosong bersebab pada gelombang lanjutan |
| `H1`–`H4` | `READY_FOR_DOMAIN_DESIGN` | — |

**Kesiapan modul: `READY_FOR_DOMAIN_DESIGN` untuk seluruh cakupan MVP.**

## 6. Yang boleh berjalan dan yang harus berhenti

| Boleh berjalan | Harus berhenti |
|---|---|
| Desain lengkap kelima sub-modul untuk seluruh slice kecuali `N6` | Desain `N6` Revaluasi |
| Usulan `AST-RCG-G05..G29` ditulis sebagai usulan yang terlihat dalam desain | Menyajikan usulan tersebut sebagai kebijakan RS sebelum desain disetujui |

## 7. Handoff

| Field | Nilai |
|---|---|
| Skill berikutnya | `design-business-module` |
| `requirement_readiness` | `READY_FOR_DOMAIN_DESIGN` (kecuali `N6`) |
| `requirement_evidence_status` | 46 keputusan `CONFIRMED`; 24 usulan `PROPOSED` (`AST-RCG-G05..G29`, tanpa `G12` dan `G28` yang tidak terpakai) |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — `hospital-domain-architect` tidak dipakai. Alasannya: modul non-klinis; kepemilikan data, master bersama yang dibaca (HR, Finance), dan integrasi Akuntansi sudah diputuskan eksplisit (`AST-DEC-015`, `034`, `038..041`) dan aturan penerimaan Akuntansi sudah ada (`ACC-DEC-045..049`) |
| `blocking_decision_ids` | Tidak ada untuk MVP; `AST-DEC-032` (Revaluasi ditunda) |
| `baseline_reference_coverage` | `NOT_USED` |
| Keluaran yang diharapkan | `02-module-map.md` dan 11 berkas per sub-modul (55 berkas) berstatus `draft` |
