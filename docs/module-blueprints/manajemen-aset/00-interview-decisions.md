# Manajemen Aset & Inventaris — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `AST-BP-001` |
| Revision | `1` |
| Decision revision | `5` — ditambah persetujuan desain (bagian 18) |
| Mode wawancara | **Scope pass** (putaran 1–7, sebelum audit) lalu **Closure pass** (putaran 8–10, sesudah `01-existing-capability-map.md` `1.0`) |
| Status | `draft` — keputusan pemilik sudah `approved` satu per satu (tabel Decision Log); decision log ini sebagai satu kesatuan menunggu audit kemampuan |
| Product/domain owner | **Rizki** — pemilik modul Manajemen Aset (`AST-DEC-002`) sekaligus pemilik modul Akuntansi (`accounting/blueprint-manifest.md`, `approved_by`) |
| Tanggal wawancara | 2026-11-12, tujuh putaran pertanyaan pilihan (pop-up) |
| Backend SHA | `1f54a17551ac49a1d12f3186ce982c1e8107c91e` (branch `rizkiG`) |
| Frontend SHA | `e49e8a8158c652f4405588c8a787f9355baea690` (branch `RizkiV2`) |

> **Peringatan duplikasi.** Batas cakupan dikunci **sebelum** audit kemampuan existing. Pemeriksaan
> cepat agent menemukan beberapa titik sentuh (bagian 9), tetapi kemungkinan duplikasi belum diperiksa
> secara menyeluruh. Pemeriksaan itu milik `trace-existing-capabilities`.

---

## 1. Cakupan dari pemilik (baseline, disalin utuh)

Daftar ini adalah masukan hulu modul. Setiap butir diberi ID kemampuan `CAP-<menu>.<butir>` agar
bisa dilacak sampai roadmap. Sub-modul pemilik setiap butir ada pada kolom terakhir dan tidak boleh
kosong (aturan requirement yatim, `bentuk-blueprint.md` bagian 5).

| ID | Menu | Butir | Sub-modul pemilik |
|---|---|---|---|
| `CAP-01.1` | 1. Dashboard Aset | Ringkasan Aset | `register` |
| `CAP-01.2` | 1. Dashboard Aset | Aset Per-Unit | `register` |
| `CAP-01.3` | 1. Dashboard Aset | Kondisi Aset | `register` |
| `CAP-01.4` | 1. Dashboard Aset | Mutasi Berjalan | `pergerakan` |
| `CAP-01.5` | 1. Dashboard Aset | Inventaris Tertunda | `inventarisasi` (`AST-DEC-031`) |
| `CAP-01.6` | 1. Dashboard Aset | Aset Rusak | `register` |
| `CAP-02.1` | 2. Master Klasifikasi Aset | Kategori Aset | `register` |
| `CAP-02.2` | 2. Master Klasifikasi Aset | Kelompok Aset | `register` |
| `CAP-02.3` | 2. Master Klasifikasi Aset | Kode Inventaris | `register` |
| `CAP-02.4` | 2. Master Klasifikasi Aset | Jenis Kepemilikan | `register` |
| `CAP-02.5` | 2. Master Klasifikasi Aset | Lokasi | `register` |
| `CAP-02.6` | 2. Master Klasifikasi Aset | Klasifikasi Aset Medis & Non-Medis | `register` |
| `CAP-03.1` | 3. Registrasi & Inventaris | Pencatatan Aset Baru | `register` |
| `CAP-03.2` | 3. Registrasi & Inventaris | Data Perolehan | `register` |
| `CAP-03.3` | 3. Registrasi & Inventaris | Nomor Inventaris | `register` |
| `CAP-03.4` | 3. Registrasi & Inventaris | Label/Aset | `register` |
| `CAP-03.5` | 3. Registrasi & Inventaris | Dokumen Kepemilikan | `register` |
| `CAP-03.6` | 3. Registrasi & Inventaris | Daftar Inventaris | `register` |
| `CAP-04.1` | 4. Penempatan & Tanggung Jawab Aset | Penempatan Per-unit/Ruangan | `register` |
| `CAP-04.2` | 4. Penempatan & Tanggung Jawab Aset | Penanggung Jawab Aset | `register` |
| `CAP-04.3` | 4. Penempatan & Tanggung Jawab Aset | Berita Acara Penyerahan | `register` |
| `CAP-04.4` | 4. Penempatan & Tanggung Jawab Aset | Riwayat Lokasi & Penggunaan | `register` |
| `CAP-05.1` | 5. Mutasi & Peminjaman Aset | Pengajuan Pemindahan | `pergerakan` |
| `CAP-05.2` | 5. Mutasi & Peminjaman Aset | Persetujuan Mutasi | `pergerakan` |
| `CAP-05.3` | 5. Mutasi & Peminjaman Aset | Serah Terima Antar Unit | `pergerakan` |
| `CAP-05.4` | 5. Mutasi & Peminjaman Aset | Peminjaman Sementara | `pergerakan` |
| `CAP-05.5` | 5. Mutasi & Peminjaman Aset | Pengembalian Aset | `pergerakan` |
| `CAP-06.1` | 6. Kondisi & Status Aset | Pemeriksaan Kondisi | `register` |
| `CAP-06.2` | 6. Kondisi & Status Aset | Aset Aktif | `register` |
| `CAP-06.3` | 6. Kondisi & Status Aset | Rusak Ringan/Berat | `register` |
| `CAP-06.4` | 6. Kondisi & Status Aset | Tidak Digunakan | `register` |
| `CAP-06.5` | 6. Kondisi & Status Aset | Hilang | `register` |
| `CAP-06.6` | 6. Kondisi & Status Aset | Riwayat Perubahan Status | `register` |
| `CAP-07.1` | 7. Inventaris & Pemeriksaan Fisik | Jadwal Inventaris | `inventarisasi` |
| `CAP-07.2` | 7. Inventaris & Pemeriksaan Fisik | Pemeriksaan Fisik | `inventarisasi` |
| `CAP-07.3` | 7. Inventaris & Pemeriksaan Fisik | Pencocokan Data | `inventarisasi` |
| `CAP-07.4` | 7. Inventaris & Pemeriksaan Fisik | Selisih Inventaris | `inventarisasi` |
| `CAP-07.5` | 7. Inventaris & Pemeriksaan Fisik | Verifikasi Hasil | `inventarisasi` |
| `CAP-07.6` | 7. Inventaris & Pemeriksaan Fisik | Berita Acara | `inventarisasi` |
| `CAP-08.1` | 8. Pemanfaatan & Pengamanan Aset | Pengajuan Aset (= permohonan pemanfaatan, `AST-DEC-021`) | `register` |
| `CAP-08.2` | 8. Pemanfaatan & Pengamanan Aset | Dokumen Kepemilikan | `register` (data yang sama dengan `CAP-03.5`, `AST-ASM-002`) |
| `CAP-08.3` | 8. Pemanfaatan & Pengamanan Aset | Pengamanan Administrasi | `register` |
| `CAP-08.4` | 8. Pemanfaatan & Pengamanan Aset | Pemantauan Aset Menganggur | `register` |
| `CAP-08.5` | 8. Pemanfaatan & Pengamanan Aset | Pemanfaatan Sesuai Kewenangan | `register` |
| `CAP-09.1` | 9. Nilai Aset & Rekonsiliasi | Harga Perolehan | `nilai` (lihat `AST-OQ-010`) |
| `CAP-09.2` | 9. Nilai Aset & Rekonsiliasi | Nilai Buku | `nilai` |
| `CAP-09.3` | 9. Nilai Aset & Rekonsiliasi | Riwayat Perubahan Nilai | `nilai` |
| `CAP-09.4` | 9. Nilai Aset & Rekonsiliasi | Data Penyusutan | `nilai` |
| `CAP-09.5` | 9. Nilai Aset & Rekonsiliasi | Rekonsiliasi Aset dengan Akuntansi | `nilai` |
| `CAP-10.1` | 10. Penghapusan & Pelepasan Aset | Usulan Penghapusan | `penghapusan` |
| `CAP-10.2` | 10. Penghapusan & Pelepasan Aset | Pemeriksaan Kelayakan Penghapusan | `penghapusan` |
| `CAP-10.3` | 10. Penghapusan & Pelepasan Aset | Persetujuan | `penghapusan` |
| `CAP-10.4` | 10. Penghapusan & Pelepasan Aset | Pemindahtanganan | `penghapusan` |
| `CAP-10.5` | 10. Penghapusan & Pelepasan Aset | Berita Acara | `penghapusan` |
| `CAP-10.6` | 10. Penghapusan & Pelepasan Aset | Riwayat Aset Dihapus | `penghapusan` |
| `CAP-11.1` | 11. Laporan Manajemen Aset | Daftar Inventaris Per-unit | `register` |
| `CAP-11.2` | 11. Laporan Manajemen Aset | Laporan Mutasi | `pergerakan` |
| `CAP-11.3` | 11. Laporan Manajemen Aset | Kondisi Aset | `register` |
| `CAP-11.4` | 11. Laporan Manajemen Aset | Inventaris | `inventarisasi` |
| `CAP-11.5` | 11. Laporan Manajemen Aset | Penghapusan | `penghapusan` |
| `CAP-11.6` | 11. Laporan Manajemen Aset | Nilai Aset | `nilai` |
| `CAP-11.7` | 11. Laporan Manajemen Aset | History & Audit | `register` — membaca riwayat seluruh sub-modul (`AST-OQ-011`) |

Jumlah: **62 kemampuan**, seluruhnya punya sub-modul pemilik. Requirement yatim: **nol**.

---

## 2. Scope dan Outcome

**Batas cakupan (satu kalimat).** Modul ini mencatat dan mengendalikan aset tetap berwujud milik
rumah sakit sejak tercatat sampai dihapus — siapa yang memegang, di mana letaknya, bagaimana
kondisinya, berapa nilainya, dan apa buktinya — ditambah inventarisasi fisik berkala.

**Hasil bisnis yang dituju.**

1. Setiap aset punya satu nomor inventaris permanen dan label QR, sehingga bisa ditemukan dan
   dipindai di lokasinya.
2. Setiap saat diketahui unit, ruangan, dan pegawai penanggung jawab tiap aset.
3. Perpindahan, peminjaman, perubahan kondisi, dan penghapusan selalu lewat persetujuan yang
   tercatat, bukan lisan.
4. Nilai buku aset yang dikapitalisasi dihitung setiap bulan dan sampai ke buku besar Akuntansi
   tanpa tercatat dua kali.
5. Hasil opname fisik bisa dibandingkan dengan catatan sistem, dan selisihnya ditindaklanjuti.

### 2.1 Di dalam cakupan (`AST-DEC-005`)

- Seluruh 62 kemampuan pada bagian 1.
- Aset tetap berwujud: alat medis, alat non-medis, mebel, kendaraan, gedung, dan tanah.
- Barang tahan lama yang nilainya di bawah batas kapitalisasi, untuk keperluan pelacakan saja
  (`AST-DEC-006`).

### 2.2 Di luar cakupan — untuk modul lain (`AST-DEC-005`)

| Kebutuhan | Pemilik | Titik sentuh dengan modul ini |
|---|---|---|
| Pembelian, PO, penerimaan barang (GR), pembayaran pemasok | Finance — Purchasing (`Fin`) | Aset boleh menautkan nomor PO/GR sebagai bukti perolehan (`AST-DEC-008`) |
| Jurnal, buku besar, tutup buku | Akuntansi (`Acc`) | Aset mengirim kejadian keuangan (`AST-DEC-015`); `ACC-OOS-007` sudah menyatakan aset tetap di luar Akuntansi |
| Penerimaan uang hasil penjualan aset | Finance (kas/penerimaan) | Aset mencatat harga jual sebagai rujukan (`AST-DEC-022`) |
| Persediaan habis pakai (obat, BMHP, ATK, stok gudang) | Farmasi / logistik | Tidak ada. "Inventaris" di modul ini berarti inventaris aset, bukan stok |
| Pemeliharaan, perbaikan, kalibrasi alat medis (IPSRS) | Modul lain, belum ada | Status kondisi `Rusak` dapat menjadi pemicu di kemudian hari |
| Pengembalian aset pegawai yang keluar | HR — Lifecycle (`TrxAssetReturn`) | Penanggung jawab aset adalah pegawai; tautan ke register menunggu audit (`AST-OQ-012`) |
| Laporan ASPAK Kemenkes | Belum dijadwalkan | Atribut alat medis (`AST-DEC-026`) menjadi bahan laporan kelak |
| Aset tak berwujud (lisensi software, hak guna) | Belum dijadwalkan | Tidak ada |
| Permintaan kebutuhan aset baru dari unit (perencanaan pengadaan) | Perencanaan / Pembelian | Tidak ada (`AST-DEC-021`) |

---

## 3. Glossary

| Istilah | Arti dalam modul ini | Contoh |
|---|---|---|
| Aset | Satu barang fisik tahan lama (dipakai lebih dari 1 tahun) yang dimiliki atau dikuasai RS | Satu unit monitor pasien merek X nomor seri SN-001 |
| Dikapitalisasi | Aset yang harganya sama dengan atau di atas batas kapitalisasi, sehingga dicatat sebagai aset tetap dan disusutkan | Batas Rp 1.000.000: monitor Rp 85.000.000 dikapitalisasi, kursi Rp 450.000 tidak |
| Batas kapitalisasi | Harga minimum agar aset dikapitalisasi; parameter yang ditetapkan Akuntansi | Rp 1.000.000 (angka contoh, nilai sebenarnya `AST-OQ-001`) |
| Nomor inventaris | Nomor unik permanen satu aset | `AST-ALKMED-000123` (`AST-DEC-033`; format lama `AST/ALK-MED/2026/000123` digantikan) |
| Penempatan | Unit dan ruangan tempat aset berada, serta pegawai penanggung jawabnya | Monitor SN-001 di IGD, ruang Resusitasi, penanggung jawab Kepala Ruang IGD |
| Mutasi | Perpindahan penempatan aset secara tetap ke unit lain | Monitor dipindah dari IGD ke ICU |
| Peminjaman | Pemakaian sementara oleh unit lain; aset tetap tercatat milik unit asal | ICU meminjam syringe pump IGD selama 3 hari |
| Serah terima | Saat fisik aset berpindah tangan, dikonfirmasi pihak penerima, dan dibuktikan berita acara | Kepala Ruang ICU mengonfirmasi monitor sudah diterima |
| Opname (inventarisasi fisik) | Pemeriksaan fisik terjadwal untuk mencocokkan aset di lapangan dengan catatan sistem | Opname IGD tahunan, 2 orang tim memindai 312 aset |
| Selisih | Perbedaan hasil opname dengan catatan | Tercatat di IGD, ditemukan di ICU |
| Nilai buku | Harga perolehan + penambahan nilai − akumulasi penyusutan − penurunan nilai | Rp 85.000.000 − (12 × Rp 1.416.667) = Rp 68.000.000 sesudah 12 bulan |
| Penyusutan | Pengurangan nilai aset setiap bulan selama umur manfaatnya | Rp 1.416.667 per bulan |
| Pemanfaatan | Pemakaian aset oleh pihak ketiga di luar fungsi utamanya, berdasarkan perjanjian | Sewa lahan ATM, KSO alat hemodialisa, pinjam pakai ke klinik mitra |
| Pemindahtanganan | Cara aset keluar dari RS saat dihapus: dijual, dihibahkan, ditukar tambah, atau dimusnahkan | Ambulans tua dijual lewat lelang |
| Kejadian keuangan | Pesan dari modul ini ke Akuntansi yang diubah menjadi jurnal oleh aturan posting (`AccAccountingEvent`) | Kejadian penyusutan bulan April untuk kelompok Alat Kedokteran |
| Tanggal awal | Tanggal batas data lama; aset yang sudah ada sebelum tanggal ini diimpor sebagai saldo awal | 31 Desember 2026 (contoh, `AST-OQ-006`) |

---

## 4. Aktor dan Tanggung Jawab

Nama peran di bawah adalah peran bisnis. Pemetaan ke Role dan Resource hak akses Quilvian dirancang
`design-business-module` (`AST-OQ-013`).

| Aktor | Mengerjakan | Menyetujui |
|---|---|---|
| Petugas Aset (Bagian Aset/Umum) | Mencatat aset, mencetak label, menempatkan, mengajukan mutasi, memproses laporan kondisi, menyusun jadwal opname, mengajukan perubahan nilai dan penghapusan, mengimpor data awal | — |
| Kepala Bagian Aset | Menetapkan kondisi, mengelola master klasifikasi | Mutasi tahap 2; status Hilang; verifikasi hasil opname dan berita acaranya; penghapusan sampai batas nilai; pemanfaatan sampai batas nilai dan masa; Pemulihan Aset |
| Kepala Unit (pemakai aset) | Melaporkan kondisi, mengajukan mutasi dari unitnya, mengonfirmasi serah terima di unit tujuan | Mutasi tahap 1 (unit asal); peminjaman aset unitnya |
| Penanggung Jawab Aset (pegawai) | Memegang aset dan melaporkan kondisinya | — |
| Tim Inventarisasi | Memindai dan memeriksa aset di lokasi saat opname | — |
| Tim Pemeriksa Kelayakan Penghapusan | Memeriksa apakah aset layak dihapus dan menuliskan rekomendasi | — |
| Kepala Akuntansi/Keuangan | Menetapkan parameter nilai (batas kapitalisasi, umur manfaat, nilai residu) | Perubahan nilai aset (`AST-DEC-030`) |
| Direktur | — | Penghapusan di atas batas nilai buku; pemanfaatan di atas batas nilai atau lebih dari 1 tahun |
| Auditor | Melihat seluruh data dan riwayat | — |

**Pemisahan tugas.** Pengaju tidak boleh menyetujui pengajuannya sendiri, di semua alur yang
memiliki persetujuan (`AST-DEC-030` dan diberlakukan sama untuk alur lain, `AST-ASM-003`).

---

## 5. Business Rules dan Invariants

Setiap aturan merujuk ke keputusan di bagian 10.

### 5.1 Register

| ID | Aturan | Contoh |
|---|---|---|
| `AST-RULE-001` | Semua barang tahan lama dicatat. Aset ditandai **dikapitalisasi** bila harga perolehannya ≥ batas kapitalisasi yang berlaku pada tanggal perolehan. Hanya aset dikapitalisasi yang disusutkan. | Batas Rp 1.000.000. Kursi Rp 450.000 dicatat dan diberi label, tanpa penyusutan. Monitor Rp 85.000.000 dicatat dan disusutkan. |
| `AST-RULE-002` | Satu nomor inventaris untuk setiap unit fisik. Pencatatan banyak unit sejenis dilakukan sekali isi lalu "buat N unit". | 40 kursi tunggu dalam satu formulir menghasilkan 40 nomor `.../000201` s.d. `.../000240` dan 40 label. |
| `AST-RULE-003` | Nomor inventaris permanen seumur aset dan tidak memuat kode lokasi. Struktur (`AST-DEC-033`): `AST-<kode kelompok>-<nomor urut 6 digit>`, satu deret per kelompok aset yang tidak pernah direset. Kode kelompok paling panjang 11 huruf/angka, karena awalan Number Series maksimal 15 karakter (`AST-` + 11). Tahun perolehan tidak masuk nomor, tetapi tercetak di label. Nomor dibuat lewat Number Series. Nomor aset yang dihapus, atau nomor yang hangus karena penyimpanan gagal, tidak dipakai ulang. | `AST-ALKMED-000123` tetap sama walaupun monitor dimutasi dari IGD ke ICU. Kode kelompok `ALATKEDOKTERAN` (14 huruf) ditolak karena terlalu panjang. |
| `AST-RULE-004` | Label berupa QR berisi nomor inventaris dan dicetak dari sistem dalam bentuk PDF. | Petugas mencetak 40 label kursi sekaligus. |
| `AST-RULE-005` | Data perolehan diisi manual; nomor PO/GR Finance boleh ditautkan sebagai bukti. Aset hibah, donasi, atau warisan dicatat tanpa GR dengan jenis perolehan yang sesuai. | Monitor dari GR `GR-2026-0456` ditautkan; kursi roda sumbangan donatur dicatat dengan jenis perolehan "Hibah masuk". |
| `AST-RULE-006` | Aset dengan klasifikasi **medis** wajib diisi merek, tipe/model, nomor seri, produsen, dan nomor izin edar (AKL/AKD). Tahun pembuatan dan distributor opsional. Untuk non-medis, merek dan nomor seri opsional. | Monitor pasien tidak bisa disimpan tanpa nomor izin edar. Meja kerja bisa disimpan tanpa merek. |
| `AST-RULE-007` | Data awal masuk lewat impor Excel per tanggal awal: data aset, lokasi, penanggung jawab, harga perolehan, dan akumulasi penyusutan sampai tanggal awal. Berlaku **semua atau tidak sama sekali** (`AST-DEC-037`): satu baris salah membuat seluruh berkas ditolak beserta daftar kesalahannya. Saldo awal tidak dikirim ke Akuntansi. | 2.400 baris diunggah; 37 baris salah karena kelompok aset tidak dikenal → nol baris tersimpan, petugas memperbaiki 37 baris lalu mengunggah ulang. Berkas besar boleh dipecah per unit. |

### 5.2 Penempatan, kondisi, dan pemanfaatan

| ID | Aturan | Contoh |
|---|---|---|
| `AST-RULE-010` | Perubahan kondisi dilaporkan penanggung jawab atau kepala unit beserta foto, lalu diperiksa dan **ditetapkan** Bagian Aset. Kondisi baru berlaku sesudah ditetapkan. | Kepala Ruang IGD melaporkan "Rusak Ringan" dengan foto layar retak; kondisi tetap "Aktif" sampai Petugas Aset menetapkannya. |
| `AST-RULE-011` | Status **Hilang** wajib disertai berita acara kehilangan dan disetujui Kepala Bagian Aset. | Laporan "Hilang" tanpa berita acara ditolak. |
| `AST-RULE-012` | Aset berstatus **Hilang** atau **Rusak Berat** tidak bisa dimutasi maupun dipinjam. | Pengajuan mutasi ventilator Rusak Berat ditolak dengan pesan "Aset rusak berat tidak dapat dipindahkan". |
| `AST-RULE-013` | Pemanfaatan oleh pihak ketiga memerlukan perjanjian, masa berlaku, dan penyetuju sesuai batas kewenangan: Kepala Bagian Aset bila nilai perjanjian di bawah batas **dan** masa paling lama 1 tahun; selain itu Direktur. | Sewa lahan ATM Rp 12 juta per tahun cukup Kepala Bagian Aset. KSO alat hemodialisa 5 tahun harus Direktur. |
| `AST-RULE-014` | Kepala unit dan penanggung jawab hanya melihat aset unitnya sendiri, termasuk aset yang sedang dipinjam ke atau dari unitnya. Bagian Aset, Akuntansi, Direktur, dan auditor melihat semua aset. Nilai rupiah (harga, nilai buku, penyusutan) hanya terlihat oleh Bagian Aset, Akuntansi, dan Direktur. | Kepala Ruang ICU melihat syringe pump pinjaman IGD, tetapi tidak melihat nilai bukunya. |

### 5.3 Mutasi dan peminjaman

| ID | Aturan | Contoh |
|---|---|---|
| `AST-RULE-020` | Mutasi disetujui dua tingkat: Kepala unit asal, lalu Bagian Aset. Unit tujuan mengonfirmasi saat serah terima. | IGD → ICU: Kepala IGD setuju, Kepala Bagian Aset setuju, Kepala Ruang ICU mengonfirmasi terima. |
| `AST-RULE-021` | Lokasi dan penanggung jawab berpindah **saat serah terima dikonfirmasi**, bukan saat disetujui. | Disetujui 3 Mei, diterima ICU 5 Mei: sampai 4 Mei monitor masih tercatat di IGD. |
| `AST-RULE-022` | Peminjaman disetujui kepala unit pemilik dan wajib punya rencana tanggal kembali. Lewat tanggal itu, pinjaman ditandai **Terlambat** dan tampil di dashboard. Pinjaman boleh diperpanjang lewat persetujuan yang sama. | Pinjam 1–3 Juni; per 4 Juni berstatus Terlambat sampai dikembalikan atau diperpanjang. |
| `AST-RULE-023` | Selama opname di sebuah unit berjalan, mutasi dan peminjaman aset di unit itu ditahan. | Opname IGD 10–12 Juni: pengajuan mutasi monitor IGD pada 11 Juni ditolak dengan pesan "Aset sedang dalam inventarisasi". |

### 5.4 Inventarisasi fisik

| ID | Aturan | Contoh |
|---|---|---|
| `AST-RULE-030` | Opname dijadwalkan Bagian Aset per unit atau ruangan, paling sedikit setahun sekali, dengan tim yang ditunjuk. | Jadwal 2027: IGD Maret, ICU April. |
| `AST-RULE-031` | Tim memindai QR di lokasi dan mencatat kondisi. Hasil pencocokan jatuh ke salah satu dari lima kategori: sesuai, tidak ditemukan, beda lokasi, beda kondisi, tidak terdaftar. | 312 aset IGD: 305 sesuai, 2 tidak ditemukan, 1 beda lokasi, 3 beda kondisi, 1 tidak terdaftar. |
| `AST-RULE-032` | Sesudah berita acara opname disetujui, sistem membuat **usulan** tindak lanjut: beda lokasi → koreksi lokasi; tidak ditemukan → status Hilang; beda kondisi → perubahan kondisi. Tiap usulan diproses lewat alurnya sendiri; data aset tidak berubah diam-diam. | 2 kursi tidak ditemukan menjadi 2 usulan status Hilang yang tetap memerlukan berita acara kehilangan (`AST-RULE-011`). |

### 5.5 Nilai

| ID | Aturan | Contoh |
|---|---|---|
| `AST-RULE-040` | Penyusutan garis lurus: per bulan = (harga perolehan − nilai residu) ÷ umur manfaat dalam bulan. Umur dan residu diatur per kelompok aset dan boleh diubah per aset dengan alasan. | Rp 85.000.000, residu Rp 0, 5 tahun → Rp 85.000.000 ÷ 60 = Rp 1.416.667 per bulan. |
| `AST-RULE-041` | Penyusutan dimulai bulan berikutnya sesudah tanggal siap pakai dan dihitung penuh per bulan. Aset yang dihapus berhenti disusutkan mulai bulan penghapusan. | Siap pakai 20 Maret 2026 → penyusutan pertama April 2026. Dihapus 10 Agustus 2028 → penyusutan terakhir Juli 2028. |
| `AST-RULE-042` | Parameter nilai ditetapkan Kepala Akuntansi dan berlaku mulai bulan berikutnya sesudah ditetapkan. Bulan yang sudah ditutup tidak pernah dihitung ulang. | Umur laptop diubah dari 4 menjadi 5 tahun pada 15 Juni → berlaku untuk penyusutan Juli; Mei dan Juni tetap 4 tahun. |
| `AST-RULE-043` | Perubahan nilai yang boleh dicatat: kapitalisasi perbaikan besar, koreksi salah input, penurunan nilai, dan revaluasi. Diajukan Petugas Aset beserta bukti dan disetujui Kepala Akuntansi. Revaluasi ditunda ke gelombang lanjutan. | Overhaul genset Rp 40.000.000 yang menambah umur 2 tahun diajukan dengan faktur, lalu disetujui Kepala Akuntansi. |
| `AST-RULE-044` | Penyusutan, penurunan nilai, penghapusan, dan Pemulihan Aset dikirim ke Akuntansi sebagai kejadian keuangan lewat `AccAccountingEvent`. **Perolehan karena pembelian tidak dikirim**, karena sudah dijurnal lewat pembelian/utang; begitu pula kapitalisasi perbaikan besar (`AST-DEC-041`) dan saldo awal. Satu-satunya perolehan yang dikirim adalah **hibah masuk** (`AST-DEC-040`). | Penyusutan April untuk 1.200 aset terkirim; pembelian monitor tidak terkirim dua kali; kursi roda hibah senilai Rp 3.000.000 terkirim sebagai kejadian perolehan hibah. |
| `AST-RULE-045` | Penyusutan bulanan diproses Petugas Aset lewat tombol Proses dengan pratinjau. Satu bulan hanya diproses sekali, harus berurutan, dan dikirim sebagai satu kejadian per kelompok aset per pusat biaya. Rincian per aset tetap di modul Aset (`AST-DEC-039`). | Pratinjau April 2026: 1.200 aset, kelompok Alat Kedokteran pusat biaya IGD Rp 12.750.000. Mei tidak bisa diproses sebelum April selesai. |
| `AST-RULE-046` | Beban penyusutan dibebankan ke pusat biaya unit tempat aset berada pada **akhir bulan** (`AST-DEC-036`). | Monitor dimutasi IGD → ICU pada 15 April: seluruh beban April Rp 1.416.667 masuk pusat biaya ICU. |
| `AST-RULE-047` | Koreksi harga perolehan yang salah ketik menghasilkan penyesuaian penyusutan sekaligus pada bulan koreksi; bulan yang sudah ditutup tidak diubah (`AST-DEC-042`). | Tertulis Rp 58.000.000 (Rp 966.667/bulan), seharusnya Rp 85.000.000 (Rp 1.416.667/bulan), sudah 6 bulan. Penyesuaian = 6 × Rp 450.000 = Rp 2.700.000 pada bulan koreksi; bulan berikutnya Rp 1.416.667. |
| `AST-RULE-048` | Pengiriman kejadian penyusutan dibangun dalam keadaan mati dan baru dinyalakan sesudah template jurnal berulang penyusutan (bila ada) dinonaktifkan Akuntansi mulai bulan yang sama (`AST-DEC-038`). | Kejadian Aset aktif mulai Januari 2027; template jurnal berulang penyusutan dinonaktifkan sebelum penyusutan Januari 2027 diproses. |

### 5.6 Penghapusan

| ID | Aturan | Contoh |
|---|---|---|
| `AST-RULE-050` | Alur penghapusan: usulan (unit atau Bagian Aset) → pemeriksaan kelayakan oleh tim → persetujuan Kepala Bagian Aset → persetujuan Direktur bila nilai buku di atas batas. | Kursi rusak nilai buku Rp 0 selesai di Kepala Bagian Aset. CT-scan nilai buku Rp 300 juta (batas contoh Rp 50 juta) harus sampai Direktur. |
| `AST-RULE-051` | Jenis pemindahtanganan: penjualan/lelang, hibah keluar, tukar tambah, pemusnahan. Laba/rugi pelepasan = harga pelepasan − nilai buku. | Nilai buku Rp 2.000.000, terjual Rp 3.500.000 → laba Rp 1.500.000. Dimusnahkan dengan nilai buku Rp 800.000 → rugi Rp 800.000. |
| `AST-RULE-052` | Uang hasil penjualan diterima Finance; modul ini hanya menyimpan harga dan nomor rujukannya. | — |

### 5.7 Koreksi

| ID | Aturan | Contoh |
|---|---|---|
| `AST-RULE-060` | Transaksi yang sudah selesai tidak pernah diubah atau dihapus. Koreksi dilakukan dengan transaksi baru. | Monitor salah dikirim ke ICU; koreksinya mutasi baru ICU → HCU. Kedua mutasi tetap terlihat di riwayat. |
| `AST-RULE-061` | Penghapusan yang sudah final hanya bisa dipulihkan lewat **Pemulihan Aset** yang disetujui Kepala Bagian Aset dan menerbitkan kejadian balik ke Akuntansi. | Laptop dihapus karena dikira hilang, lalu ditemukan; Pemulihan Aset mengembalikan statusnya dan mengirim kejadian balik penghapusan. |

---

## 6. State dan Transition

Kosakata status di bawah adalah hasil wawancara. Tabel transisi lengkap beserta pelaku dan syaratnya
disusun pada `contracts/state-transition-matrix.md` tiap sub-modul.

| Sub-modul | Objek | Status yang disebut dalam wawancara |
|---|---|---|
| `register` | Kondisi aset | `Aktif`, `Rusak Ringan`, `Rusak Berat`, `Tidak Digunakan`, `Hilang`, ditambah `Dihapus` (diterapkan sub-modul `penghapusan`) |
| `register` | Laporan kondisi | Dilaporkan → Ditetapkan / Ditolak |
| `register` | Permohonan pemanfaatan | Diajukan → Disetujui / Ditolak → Berjalan → Berakhir |
| `pergerakan` | Mutasi | Diajukan → Disetujui unit asal → Disetujui Bagian Aset → Diserahterimakan; Ditolak; Dibatalkan sebelum serah terima |
| `pergerakan` | Peminjaman | Diajukan → Disetujui → Dipinjam → (Terlambat) → Dikembalikan; perpanjangan |
| `inventarisasi` | Sesi opname | Dijadwalkan → Berjalan → Pencocokan → Diverifikasi → Berita acara disetujui |
| `nilai` | Perubahan nilai | Diajukan → Disetujui / Ditolak → kejadian terkirim |
| `nilai` | Penyusutan bulanan | Dihitung → kejadian terkirim (rincian `AST-OQ-008`) |
| `penghapusan` | Usulan penghapusan | Diusulkan → Diperiksa kelayakan → Disetujui Kepala Bagian Aset → (Disetujui Direktur) → Dipindahtangankan → Final; Ditolak; Dipulihkan |

---

## 7. Skenario Normal dan Exception

| ID | Skenario | Perilaku yang diputuskan |
|---|---|---|
| `AST-SCN-01` | Normal: 40 kursi dibeli, GR terbit di Finance | Petugas Aset mencatat sekali, membuat 40 unit, menautkan GR, mencetak 40 label QR, menempatkan di Ruang Tunggu Poli |
| `AST-SCN-02` | Normal: monitor dimutasi IGD → ICU | Dua persetujuan, serah terima dikonfirmasi ICU, lokasi berpindah saat konfirmasi |
| `AST-SCN-03` | Pengecualian: aset Rusak Berat diajukan mutasi | Ditolak (`AST-RULE-012`) |
| `AST-SCN-04` | Pengecualian: pinjaman lewat tanggal kembali | Ditandai Terlambat dan tampil di dashboard; boleh diperpanjang (`AST-RULE-022`) |
| `AST-SCN-05` | Pengecualian: mutasi diajukan saat opname unit berjalan | Ditolak sampai opname selesai (`AST-RULE-023`) |
| `AST-SCN-06` | Pengecualian: aset tidak ditemukan saat opname | Usulan status Hilang sesudah berita acara opname disetujui; tetap butuh berita acara kehilangan |
| `AST-SCN-07` | Pengecualian: kelompok aset baru belum dipetakan ke akun beban penyusutan | Kejadian ditolak dan masuk daftar gagal di Akuntansi; tidak ada jurnal tebakan (`ACC-DEC-046`) |
| `AST-SCN-08` | Pengecualian: penyusutan terkirim sesudah periode Akuntansi ditutup | Dicatat di periode terbuka berikutnya, tanggal dokumen asli tetap disimpan (`ACC-DEC-047`) |
| `AST-SCN-09` | Pengecualian: pengiriman kejadian gagal | Dicoba ulang 3 kali lalu masuk daftar gagal (`ACC-DEC-049`); perilaku sisi Aset `AST-OQ-008` |
| `AST-SCN-10` | Koreksi: mutasi selesai ternyata salah tujuan | Mutasi baru; mutasi lama tetap di riwayat (`AST-RULE-060`) |
| `AST-SCN-11` | Koreksi: aset sudah dihapus ternyata ditemukan | Pemulihan Aset + kejadian balik (`AST-RULE-061`) |
| `AST-SCN-12` | Pengecualian: pengaju menyetujui pengajuannya sendiri | Ditolak (`AST-ASM-003`) |
| `AST-SCN-13` | Pengecualian: impor data awal berisi baris salah | Seluruh berkas ditolak beserta daftar kesalahan per baris; nol baris tersimpan (`AST-RULE-007`, `AST-DEC-037`) |
| `AST-SCN-14` | Pengecualian: dua petugas menyetujui pengajuan yang sama hampir bersamaan | Hanya satu yang berhasil; yang lain mendapat pesan data sudah berubah — ditetapkan desain sebagai invariant teknis |

---

## 8. Frontend Decision Authority

Hierarki: keamanan/privasi/invariant → brief produk yang disetujui → konvensi project (design token
dan base component Quilvian) → keleluasaan developer.

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `AST-FE-001` | Letak grup menu "Manajemen Aset" pada sidebar Corporate | Rizki | `open` | Satu grup berisi 11 butir menu sesuai bagian 1 | `AST-OQ-015` |
| `AST-FE-002` | Nama dan urutan butir menu | Rizki | `approved` | Mengikuti daftar cakupan bagian 1 apa adanya | Cakupan pemilik |
| `AST-FE-003` | Pemindaian QR saat opname lewat kamera HP/tablet di browser | Rizki | `approved` | Wajib bisa dari perangkat genggam; tetap ada input nomor manual sebagai cadangan | `AST-DEC-010`, `AST-DEC-018` |
| `AST-FE-004` | Rupiah disembunyikan dari peran tanpa hak lihat nilai | Rizki | `approved` | Tidak boleh hanya disembunyikan di layar; backend tidak mengirim angkanya | `AST-DEC-025` |
| `AST-FE-005` | Tata letak dashboard, bentuk kartu, grafik, tab/modal/drawer | — | `DEV_DISCRETION` | Dalam design token dan base component Quilvian | — |
| `AST-FE-006` | Ukuran dan tata letak label QR | Bagian Aset | `open` | Bergantung printer label yang dipakai | `AST-OQ-016` |

---

## 9. Fakta dari pemeriksaan awal (bukan keputusan)

Fakta ini ditemukan agent sebelum audit resmi, untuk menajamkan pertanyaan. Statusnya tetap harus
dikonfirmasi `trace-existing-capabilities`.

| ID | Fakta | Bukti |
|---|---|---|
| `AST-FACT-001` | Belum ada blueprint, Area, model, maupun halaman frontend aset | `docs/module-blueprints/` tanpa folder aset; `find Areas` tanpa folder asset/inventaris; pencarian frontend nol hasil |
| `AST-FACT-002` | Prefix `Ast` belum dipakai | `class Ast*` nol hasil di source backend; tidak ada di registry |
| `AST-FACT-003` | Akuntansi punya jalur kejadian keuangan dari modul lain: `AccAccountingEvent` (kode jenis kejadian, modul pengirim, komponen nominal) | `Areas/Corporate/AccountingManagement/AccountingEvent/Models/AccAccountingEvent.cs` |
| `AST-FACT-004` | Daftar periksa tutup buku Akuntansi sudah punya butir "Penyusutan belum dijalankan" (`DEPRECIATION_NOT_RUN`), belum aktif dan tidak menahan | `AccountingPeriod/Services/AccPeriodClosingService.cs` baris 83 dan 183–188 |
| `AST-FACT-005` | Akuntansi menyatakan aset tetap di luar cakupannya; hanya akibat perolehan, penyusutan, revaluasi, dan pelepasan yang masuk | `accounting/00-interview-decisions.md`, `ACC-OOS-007` |
| `AST-FACT-006` | Akuntansi punya jurnal berulang (`AccRecurringJournalTemplate`) yang komentarnya menyebut penyusutan bulanan sebagai contoh pemakaian | `RecurringJournal/Models/AccRecurringJournalTemplate.cs` baris 11 |
| `AST-FACT-007` | HR offboarding punya `TrxAssetReturn` dengan `AssetCode` dan `AssetName` berupa teks bebas, tanpa tautan ke register aset | `Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxAssetReturn.cs` |
| `AST-FACT-008` | Finance Purchasing punya PO dan GR (`FinPurchaseOrder`, `FinGoodsReceipt`) | Registry baris `FinanceManagement / Purchasing` |
| `AST-FACT-009` | Number Series tersedia sebagai kapabilitas platform (`Num`) | Registry baris `Platform / NumberSeriesManagement` |

---

## 10. Decision Log

Seluruh keputusan bertipe `Decision` di bawah dijawab langsung oleh Rizki lewat pertanyaan pilihan
pada 2026-11-12. Untuk keputusan yang menyentuh Akuntansi, Rizki menjawab dalam kapasitasnya sebagai
pemilik modul Akuntansi juga.

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `AST-DEC-001` | Decision | Bentuk `COMPOSITE`, 5 sub-modul: `register`, `pergerakan`, `inventarisasi`, `nilai`, `penghapusan`. `shape_decided_by: USER_CONFIRMED`. Hasil uji pada bagian 11 | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 0 |
| `AST-DEC-002` | Decision | Pemilik modul Rizki; branch backend `rizkiG`, frontend `RizkiV2` | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 0 |
| `AST-DEC-003` | Decision | Rezim aset RS swasta: kebijakan internal badan hukum pemilik RS dan standar akuntansi aset tetap (PSAK 216). Istilah seperti Pemindahtanganan dipakai sebagai nama menu | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 0 |
| `AST-DEC-004` | Decision | Area `Areas/Corporate/AssetManagement`, prefix `Ast`, slug `manajemen-aset`, task `BE-AST-###`/`FE-AST-###` | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 0; `AST-FACT-002` |
| `AST-DEC-005` | Decision | Batas cakupan bagian 2.1 dan 2.2 dikunci | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 1 |
| `AST-DEC-006` | Decision | Semua barang tahan lama dicatat; penanda dikapitalisasi bila harga ≥ batas kapitalisasi (`AST-RULE-001`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 1 |
| `AST-DEC-007` | Decision | Satu nomor per unit fisik, dengan "buat N unit" (`AST-RULE-002`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 1 |
| `AST-DEC-008` | Decision | Sumber aset manual dengan tautan opsional ke PO/GR Finance (`AST-RULE-005`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 1 |
| `AST-DEC-009` | Decision | Nomor inventaris permanen tanpa kode lokasi (`AST-RULE-003`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 2 |
| `AST-DEC-010` | Decision | Label QR, dicetak PDF dari sistem (`AST-RULE-004`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 2 |
| `AST-DEC-011` | Decision | Mutasi: Kepala unit asal → Bagian Aset; unit tujuan konfirmasi saat serah terima (`AST-RULE-020`, `AST-RULE-021`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 2 |
| `AST-DEC-012` | Decision | Peminjaman wajib tanggal kembali, Terlambat tampil di dashboard, boleh diperpanjang (`AST-RULE-022`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 2 |
| `AST-DEC-013` | Decision | Penyusutan garis lurus saja; umur dan residu per kelompok, boleh diubah per aset dengan alasan (`AST-RULE-040`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 3 |
| `AST-DEC-014` | Decision | Penyusutan mulai bulan berikutnya, penuh per bulan; berhenti mulai bulan penghapusan (`AST-RULE-041`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 3 |
| `AST-DEC-015` | Decision | Penyusutan, perubahan nilai, dan penghapusan dikirim sebagai kejadian keuangan lewat `AccAccountingEvent`; perolehan dan saldo awal tidak dikirim (`AST-RULE-044`) | Rizki (Aset + Akuntansi) | `approved` | Rizki, 2026-11-12 | Putaran 3; `AST-FACT-003`, `AST-FACT-005` |
| `AST-DEC-016` | Decision | Jenis perubahan nilai: perbaikan besar, koreksi salah input, penurunan nilai, revaluasi (`AST-RULE-043`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 3. Catatan: revaluasi menurut PSAK berlaku untuk satu kelompok aset sekaligus dan memerlukan akun surplus revaluasi |
| `AST-DEC-017` | Decision | Kondisi dilaporkan unit dan ditetapkan Bagian Aset; Hilang butuh berita acara dan persetujuan Kepala Bagian Aset; Hilang/Rusak Berat tidak bisa dimutasi/dipinjam (`AST-RULE-010..012`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 4 |
| `AST-DEC-018` | Decision | Opname terjadwal per unit/ruangan, pindai QR, lima kategori hasil, mutasi/pinjam ditahan selama opname (`AST-RULE-023`, `AST-RULE-030`, `AST-RULE-031`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 4 |
| `AST-DEC-019` | Decision | Selisih opname menjadi usulan otomatis yang dieksekusi lewat alurnya masing-masing (`AST-RULE-032`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 4 |
| `AST-DEC-020` | Decision | Penghapusan berjenjang menurut nilai buku (`AST-RULE-050`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 4 |
| `AST-DEC-021` | Decision | "Pengajuan Aset" pada menu 8 = permohonan pemanfaatan oleh pihak ketiga (sewa, KSO, pinjam pakai). Permintaan kebutuhan aset baru di luar cakupan | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 5 |
| `AST-DEC-022` | Decision | Pemindahtanganan: penjualan/lelang, hibah keluar, tukar tambah, pemusnahan; uang diterima Finance (`AST-RULE-051`, `AST-RULE-052`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 5 |
| `AST-DEC-023` | Decision | Data awal lewat impor Excel per tanggal awal; saldo awal tidak dikirim ke Akuntansi (`AST-RULE-007`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 5 |
| `AST-DEC-024` | Decision | Urutan rilis: `register` → `pergerakan` → `inventarisasi` → `nilai` → `penghapusan` | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 5 |
| `AST-DEC-025` | Decision | Hak lihat per unit; pusat melihat semua; nilai rupiah hanya Bagian Aset, Akuntansi, Direktur (`AST-RULE-014`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 6 |
| `AST-DEC-026` | Decision | Atribut wajib alat medis (`AST-RULE-006`); kalibrasi tidak dicatat | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 6 |
| `AST-DEC-027` | Decision | Parameter nilai ditetapkan Kepala Akuntansi, berlaku mulai bulan berikutnya; Bagian Aset mengelola master klasifikasi lain (`AST-RULE-042`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 6 |
| `AST-DEC-028` | Decision | Koreksi lewat transaksi balik; penghapusan final dipulihkan lewat Pemulihan Aset (`AST-RULE-060`, `AST-RULE-061`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 6 |
| `AST-DEC-029` | Decision | Pemanfaatan disetujui berjenjang menurut nilai perjanjian dan masa (`AST-RULE-013`) | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 7 |
| `AST-DEC-030` | Decision | Perubahan nilai diajukan Petugas Aset, disetujui Kepala Akuntansi; pengaju tidak boleh menyetujui sendiri | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 7 |
| `AST-DEC-031` | Decision | Kartu "Inventaris Tertunda" = jadwal opname yang lewat tanggal tetapi belum diverifikasi, ditambah selisih opname yang belum ditindaklanjuti | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 7 |
| `AST-DEC-032` | Decision | Revaluasi ditunda ke gelombang lanjutan sub-modul `nilai`; rilis pertama `nilai` berisi penyusutan, perbaikan besar, koreksi, dan penurunan nilai. `AST-DEC-016` tetap berlaku | Rizki | `approved` | Rizki, 2026-11-12 | Putaran 7 |
| `AST-ASM-001` | Assumption | Barang tahan lama = umur pakai yang diharapkan lebih dari 1 tahun | Rizki | `draft` | — | Dipakai pada opsi putaran 1; belum diuji sebagai definisi formal |
| `AST-ASM-002` | Assumption | "Dokumen Kepemilikan" pada menu 3.5 dan 8.2 adalah data yang sama; menu 8.2 menampilkan sisi pengamanan (kelengkapan dan masa berlaku) | Rizki | `draft` | — | Butir menu ganda pada cakupan |
| `AST-ASM-003` | Assumption | Larangan menyetujui pengajuan sendiri berlaku di semua alur persetujuan, bukan hanya perubahan nilai | Rizki | `draft` | — | `AST-DEC-030` |
| `AST-CON-001` | Conflict | Akuntansi bisa saja sudah atau akan menjurnal penyusutan lewat jurnal berulang (`AST-FACT-006`). Bila keduanya berjalan, penyusutan tercatat dua kali | Rizki (Akuntansi) | `superseded` → diselesaikan `AST-DEC-038` | Rizki, 2026-11-12 | Keberadaan template di database tetap `AST-UNK-002`, menjadi butir daftar periksa sebelum pengiriman dinyalakan |

---

## 11. Hasil uji pemecahan (`bentuk-blueprint.md` bagian 4.1)

Syarat: (1) bounded context sendiri; (2) kosakata status sendiri; (3) Resource hak akses dan
pemilik peran sendiri; (4) dapat dirilis sendiri; (5) master data dan pemilik approval sendiri.

| Sub-modul | (1) | (2) | (3) | (4) | (5) | Hasil |
|---|---|---|---|---|---|---|
| `register` | Ya — register aset | Ya — kondisi aset, laporan kondisi, pemanfaatan | Ya — Petugas/Kepala Bagian Aset, Kepala Unit | Ya — gelombang pertama | Ya — master klasifikasi, approval Kepala Bagian Aset | **5/5** |
| `pergerakan` | Ya — dokumen mutasi dan pinjaman | Ya — status mutasi dan pinjaman | Ya — Kepala Unit asal/tujuan | Ya — gelombang kedua | Sebagian — tanpa master sendiri; approval Kepala Unit asal | **4/5** |
| `inventarisasi` | Ya — sesi opname | Ya — status sesi opname | Ya — Tim Inventarisasi | Ya — gelombang ketiga | Sebagian — tanpa master; verifikasi Kepala Bagian Aset | **4/5** |
| `nilai` | Ya — buku nilai aset | Ya — perubahan nilai, penyusutan | Ya — Kepala Akuntansi | Ya — gelombang keempat | Ya — parameter nilai dan approval milik Akuntansi | **5/5** |
| `penghapusan` | Ya — usulan penghapusan | Ya — status usulan | Ya — Tim Pemeriksa, Direktur | Ya — gelombang kelima | Sebagian — tanpa master; approval Direktur | **4/5** |

Seluruh sub-modul lolos. Hasil uji sesuai dengan pilihan pemilik `AST-DEC-001`.
Catatan: pemanfaatan (`CAP-08.*`) punya status dan penyetuju sendiri tetapi sengaja ditaruh di
`register` sesuai pilihan pemilik; ia memakai aset, lokasi, dan dokumen kepemilikan yang sama.

---

## 12. Acceptance Criteria yang sudah dapat diuji

| ID | Kriteria |
|---|---|
| `AST-AC-001` | Mencatat 40 kursi dalam satu formulir menghasilkan 40 aset dengan 40 nomor inventaris berurutan dan 40 label QR |
| `AST-AC-002` | Aset dengan harga di bawah batas kapitalisasi tersimpan dengan penanda tidak dikapitalisasi dan tidak pernah muncul di perhitungan penyusutan |
| `AST-AC-003` | Aset medis tanpa nomor izin edar ditolak saat disimpan; aset non-medis tanpa merek diterima |
| `AST-AC-004` | Nomor inventaris tidak berubah setelah mutasi selesai |
| `AST-AC-005` | Lokasi dan penanggung jawab baru berlaku tepat saat serah terima dikonfirmasi unit tujuan, tidak sebelumnya |
| `AST-AC-006` | Pengajuan mutasi atau pinjaman atas aset Hilang, Rusak Berat, atau yang unitnya sedang opname ditolak dengan pesan yang menjelaskan sebabnya |
| `AST-AC-007` | Pinjaman yang melewati rencana tanggal kembali tampil sebagai Terlambat di dashboard pada hari berikutnya |
| `AST-AC-008` | Status Hilang tidak bisa ditetapkan tanpa berita acara kehilangan |
| `AST-AC-009` | Monitor Rp 85.000.000, residu Rp 0, umur 60 bulan, siap pakai 20 Maret 2026 menghasilkan penyusutan pertama Rp 1.416.667 untuk April 2026 |
| `AST-AC-010` | Kejadian perolehan dan saldo awal tidak pernah terkirim ke `AccAccountingEvent` |
| `AST-AC-011` | Pengaju tidak bisa menyetujui pengajuannya sendiri |
| `AST-AC-012` | Penghapusan aset bernilai buku di atas batas tidak bisa final tanpa persetujuan Direktur |
| `AST-AC-013` | Laba/rugi pelepasan dihitung sebagai harga pelepasan − nilai buku |
| `AST-AC-014` | Kepala unit tidak menerima angka rupiah dari API untuk aset unitnya |
| `AST-AC-015` | Berita acara opname yang disetujui menghasilkan usulan tindak lanjut tanpa mengubah data aset secara langsung |
| `AST-AC-016` | Transaksi selesai tidak bisa diubah atau dihapus; koreksi selalu berupa transaksi baru yang terlihat di riwayat |

---

## 13. Open Questions dan Blocker

`Memblokir` menyebut tahap yang tertahan: `DESIGN` (desain sub-modul), `IMPLEMENTATION`
(pengerjaan task), atau `LATER SLICE` (gelombang sesudah MVP).

| ID | Pertanyaan | Sub-modul | Owner | Memblokir | Usulan aman sementara |
|---|---|---|---|---|---|
| `AST-OQ-001` | Berapa batas kapitalisasi awal? Berlaku untuk semua kelompok atau per kelompok? | `register`, `nilai` | Kepala Akuntansi (Rizki) | `IMPLEMENTATION` (nilai awal pengaturan) | Desain menyimpannya sebagai parameter bertanggal berlaku |
| `AST-OQ-002` | Daftar kelompok aset awal beserta umur manfaat dan nilai residunya | `register`, `nilai` | Kepala Akuntansi (Rizki) | `IMPLEMENTATION` (data awal) | Desain tidak bergantung pada isinya |
| `AST-OQ-003` | Berapa batas nilai buku yang memerlukan persetujuan Direktur pada penghapusan? | `penghapusan` | Rizki/Direktur | `IMPLEMENTATION` | Parameter; contoh Rp 50.000.000 hanya ilustrasi |
| `AST-OQ-004` | Berapa batas nilai perjanjian pemanfaatan yang memerlukan Direktur? | `register` | Rizki/Direktur | `IMPLEMENTATION` | Parameter |
| ~~`AST-OQ-005`~~ | Nomor urut inventaris direset per kelompok per tahun, atau berjalan terus per kelompok? | `register` | Rizki | **Ditutup** `AST-DEC-033` (2026-11-12) | Disarankan: per kelompok per tahun, sesuai struktur nomor |
| `AST-OQ-006` | Tanggal awal (cut-off) impor data lama | `register`, `nilai` | Rizki (Aset + Akuntansi) | `IMPLEMENTATION` (go-live) | — |
| ~~`AST-OQ-007`~~ | Apakah Akuntansi sudah atau akan menjurnal penyusutan lewat jurnal berulang? Bila ya, kapan dimatikan agar tidak dobel dengan kejadian dari Aset (`AST-CON-001`) | `nilai` | Rizki (Akuntansi) | **Ditutup** `AST-DEC-038` (2026-11-12) | Fail-closed: kejadian penyusutan dari Aset belum dikirim sampai konflik ini ditutup |
| ~~`AST-OQ-008`~~ | Bentuk kejadian penyusutan: satu kejadian per aset per bulan, atau satu kejadian per kelompok per bulan? Siapa yang menjalankan penyusutan bulanan (terjadwal otomatis atau tombol Petugas Aset) dan apa yang terjadi bila pengiriman gagal | `nilai` | Rizki (Akuntansi) | **Ditutup** `AST-DEC-039` (2026-11-12) | Disarankan: per kelompok per bulan dengan rincian per aset di sisi Aset |
| `AST-OQ-009` | Kode jenis kejadian dan aturan posting akun untuk penyusutan, perbaikan besar, penurunan nilai, penghapusan (per jenis pelepasan), dan Pemulihan Aset | `nilai`, `penghapusan` | Rizki (Akuntansi) | `IMPLEMENTATION` | Kejadian tanpa pemetaan otomatis masuk daftar gagal (`ACC-DEC-046`) |
| `AST-OQ-010` | Harga perolehan diisi di `register` saat pencatatan, sedangkan riwayat nilai milik `nilai`. Tabel mana yang memiliki harga perolehan? | `register`, `nilai` | Desain (`02-module-map.md`) | `DESIGN` (peta kepemilikan data) | Diselesaikan `design-business-module`; bukan keputusan bisnis |
| `AST-OQ-011` | Laporan History & Audit membaca riwayat kelima sub-modul. Cukup dibaca dari jejak audit masing-masing, atau perlu satu jejak audit gabungan? | `register` | Desain | `DESIGN` | Diselesaikan desain |
| `AST-OQ-012` | Apakah `TrxAssetReturn` HR perlu dihubungkan ke register aset (pegawai keluar wajib mengembalikan aset yang ia pegang)? | — (di luar cakupan) | Rizki + pemilik HR | `LATER SLICE` | Tidak disentuh MVP |
| `AST-OQ-013` | Pemetaan peran bisnis (bagian 4) ke Role dan Resource hak akses Quilvian | Semua | Desain + Rizki | `DESIGN` | Dirancang `permission-audit-matrix.md` |
| ~~`AST-OQ-014`~~ | Impor data awal: simpan baris yang benar dan lewati yang salah, atau tolak seluruh berkas bila ada satu baris salah? | `register` | Rizki | **Ditutup** `AST-DEC-037` (2026-11-12) | Opsi pada putaran 5 menyiratkan baris benar tetap tersimpan; dikonfirmasi saat desain |
| `AST-OQ-015` | Letak grup menu Manajemen Aset di sidebar | Semua | Rizki | `IMPLEMENTATION` (frontend) | — |
| `AST-OQ-016` | Ukuran label dan jenis printer | `register` | Bagian Aset | `IMPLEMENTATION` | PDF ukuran stiker umum |
| ~~`AST-OQ-017`~~ | Hibah masuk (aset sumbangan tanpa pembelian) belum punya jurnal dari pembelian. Apakah Aset mengirim kejadian perolehan khusus untuk jenis perolehan hibah? | `nilai` | Rizki (Akuntansi) | **Ditutup** `AST-DEC-040` (2026-11-12) | Pengecualian terbatas dari `AST-RULE-044` bila disetujui |
| ~~`AST-OQ-018`~~ | Perbaikan besar biasanya dibayar lewat faktur utang. Apakah jurnalnya dari Utang (Finance) dan Aset hanya menambah nilai tanpa kejadian, atau Aset mengirim kejadian kapitalisasi? Pertanyaan sama dengan perolehan: hindari jurnal dua kali | `nilai` | Rizki (Akuntansi + Finance) | **Ditutup** `AST-DEC-041` (2026-11-12) | Fail-closed: tidak dikirim sampai diputuskan |

**Ringkasan blocker desain (sesudah closure pass).** Tidak ada lagi keputusan bisnis yang menahan
desain kelima sub-modul. Pertanyaan yang tersisa milik desain sendiri (`AST-OQ-010`, `011`, `013`)
atau menahan implementasi/go-live saja (`AST-OQ-001..004`, `006`, `009`, `015`, `016`,
`AST-UNK-002`, `AST-UNK-004`).

---

## 14. Keadaan akhir sesi (2026-11-12)

- Scope pass (putaran 1–7) dan closure pass (putaran 8–10) selesai: **42 keputusan `approved`**
  (`AST-DEC-001..042`), satu konflik ditutup (`AST-CON-001`), satu konflik dari audit ditutup
  (`AST-CON-002` → `AST-DEC-033`).
- Peta kemampuan: `01-existing-capability-map.md` revisi `1.0`, `CURRENT` pada kedua SHA di header.
- Langkah berikutnya: `requirement-completeness-gate`, lalu `design-business-module` untuk kelima
  sub-modul.

---

## 15. Closure pass — putaran 8–10 (2026-11-12)

Masukan: `01-existing-capability-map.md` revisi `1.0` bagian 6 dan 8, ditambah empat pertanyaan
terbuka sub-modul `nilai`. Semua dijawab Rizki lewat pertanyaan pilihan.

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Menutup |
|---|---|---|---|---|---|---|
| `AST-DEC-033` | Decision | Format nomor inventaris `AST-<kode kelompok>-<urut 6 digit>`, contoh `AST-ALKMED-000123`; satu deret per kelompok, tidak pernah direset; tahun perolehan dicetak di label, tidak di nomor. **Menggantikan** contoh format pada `AST-DEC-009` (sifat permanen tetap berlaku) | Rizki | `approved` | Rizki, 2026-11-12 | `AST-CON-002`, `AST-OQ-005` |
| `AST-DEC-034` | Decision | Unit diambil dari `MstOrganizationUnit` (HR). Ruangan dikelola di master Lokasi milik Aset (gedung, lantai, ruangan) yang boleh menautkan `MstRoom` untuk ruangan klinis | Rizki | `approved` | Rizki, 2026-11-12 | `AST-UNK-001`, `AST-UNK-003` |
| `AST-DEC-035` | Decision | Satu badan hukum, satu site. Aset tetap menyimpan badan hukum dan site karena wajib untuk kejadian Akuntansi; layar tidak menampilkan pilihan entitas | Rizki | `approved` | Rizki, 2026-11-12 | `AST-UNK-005` |
| `AST-DEC-036` | Decision | Beban penyusutan per pusat biaya unit tempat aset berada pada akhir bulan (`AST-RULE-046`) | Rizki | `approved` | Rizki, 2026-11-12 | `AST-UNK-006` |
| `AST-DEC-037` | Decision | Impor data awal semua atau tidak sama sekali (`AST-RULE-007`), mengikuti pola batch saldo awal Finance | Rizki | `approved` | Rizki, 2026-11-12 | `AST-OQ-014` |
| `AST-DEC-038` | Decision | Aset menjadi satu-satunya sumber jurnal penyusutan; pengiriman kejadian penyusutan dibangun mati dan dinyalakan sesudah template jurnal berulang penyusutan dinonaktifkan mulai bulan yang sama (`AST-RULE-048`) | Rizki (Aset + Akuntansi) | `approved` | Rizki, 2026-11-12 | `AST-CON-001`, `AST-OQ-007` |
| `AST-DEC-039` | Decision | Penyusutan diproses lewat tombol dengan pratinjau, sekali per bulan, berurutan; satu kejadian per kelompok per pusat biaya (`AST-RULE-045`) | Rizki | `approved` | Rizki, 2026-11-12 | `AST-OQ-008` |
| `AST-DEC-040` | Decision | Hibah masuk dikirim sebagai kejadian perolehan hibah sesudah aset disetujui; pengecualian terbatas dari `AST-RULE-044` | Rizki (Akuntansi) | `approved` | Rizki, 2026-11-12 | `AST-OQ-017` |
| `AST-DEC-041` | Decision | Faktur perbaikan besar dijurnal langsung ke akun aset tetap oleh Finance/Akuntansi; Aset hanya menambah nilai dengan rujukan nomor faktur dan tidak mengirim kejadian; selisih terlihat pada rekonsiliasi subledger | Rizki (Akuntansi + Finance) | `approved` | Rizki, 2026-11-12 | `AST-OQ-018` |
| `AST-DEC-042` | Decision | Koreksi harga perolehan menghasilkan penyesuaian penyusutan sekaligus pada bulan koreksi (`AST-RULE-047`) | Rizki (Akuntansi) | `approved` | Rizki, 2026-11-12 | — |

Yang tetap terbuka sesudah closure pass:

| ID | Sifat | Menahan |
|---|---|---|
| `AST-UNK-002` | Ada atau tidaknya template jurnal berulang penyusutan di database | Penyalaan pengiriman kejadian penyusutan (go-live `nilai`), bukan desain |
| `AST-UNK-004` | Penugasan manajerial Kepala Unit sudah diisi HR atau belum | Uji dan go-live `pergerakan` |
| `AST-OQ-001..004`, `006`, `009`, `015`, `016` | Angka parameter, tanggal awal, kode kejadian/akun, letak menu, ukuran label | Implementasi/go-live |

---

## 16. Penutupan blocker dari Requirement Completeness Gate — putaran 11 (2026-11-12)

`requirement-completeness-gate` (`requirement-completeness-assessment.md`, `AST-RCG-001`) menemukan
empat celah yang belum pernah ditanyakan. Tiga bersifat `BLOCKING` karena mengubah siklus status
dan pemicu kejadian Akuntansi; satu menyangkut keabsahan berita acara. Semua dijawab Rizki.

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Menutup |
|---|---|---|---|---|---|---|
| `AST-DEC-043` | Decision | Pencatatan aset baru berstatus **Draft** (boleh disunting, belum bernomor) lalu disetujui Kepala Bagian Aset. Saat disetujui: nomor inventaris terbit, aset berstatus Aktif, dan untuk hibah masuk kejadian perolehan hibah terkirim. Draft yang dibatalkan tidak menghanguskan nomor karena nomor belum terbit. "Buat N unit" menghasilkan N draft | Rizki | `approved` | Rizki, 2026-11-12 | `AST-RCG-G01` |
| `AST-DEC-044` | Decision | Penempatan pertama mengikuti pola serah terima: Bagian Aset menyerahkan ke unit dan penanggung jawab, kepala unit/penanggung jawab mengonfirmasi, lalu Berita Acara Penyerahan terbit. Sebelum konfirmasi, aset tercatat **Belum ditempatkan** | Rizki | `approved` | Rizki, 2026-11-12 | `AST-RCG-G02` |
| `AST-DEC-045` | Decision | Usulan penghapusan memiliki **alasan**: Rusak Berat, Usang, atau Hilang. Untuk alasan Hilang, langkah pemindahtanganan dilewati, berita acara kehilangan wajib dilampirkan, dan nilai buku tersisa menjadi rugi kehilangan. Persetujuan berjenjang (`AST-RULE-050`) tetap berlaku | Rizki | `approved` | Rizki, 2026-11-12 | `AST-RCG-G03` |
| `AST-DEC-046` | Decision | Berita acara dicetak dari sistem dengan nama, jabatan, dan waktu setiap persetujuan; persetujuan di sistem adalah bukti yang berlaku. Scan bertanda tangan basah boleh dilampirkan, tidak wajib | Rizki | `approved` | Rizki, 2026-11-12 | `AST-RCG-G04` |

Contoh `AST-DEC-043`: Petugas Aset mengisi 40 kursi dan memilih "buat 40 unit" → 40 draft tanpa nomor.
Kepala Bagian Aset menyetujui 38 dan menolak 2 karena salah kelompok → 38 nomor terbit berurutan
(`AST-MBL-000201` s.d. `AST-MBL-000238`); 2 draft kembali ke Petugas Aset tanpa memakan nomor.

Contoh `AST-DEC-045`: laptop nilai buku Rp 4.200.000 dinyatakan Hilang dengan berita acara kehilangan.
Usulan penghapusan beralasan Hilang melewati langkah pemindahtanganan; nilai buku di bawah batas
Direktur, sehingga selesai di Kepala Bagian Aset. Kejadian penghapusan membawa rugi kehilangan
Rp 4.200.000.

Keadaan akhir: **46 keputusan `approved`** (`AST-DEC-001..046`), decision revision `3`.

---

## 17. Keputusan saat desain — putaran 12 (2026-11-12)

Ditemukan `design-business-module` saat merancang integrasi dan alur persetujuan.

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Menutup |
|---|---|---|---|---|---|---|
| `AST-CON-003` | Conflict | Aturan posting Akuntansi memegang pusat biaya tetap per baris (`AccPostingRuleLine.CostCenterId`), dan kejadian hanya membawa komponen (kode, nominal) tanpa pusat biaya (`AccAccountingEventComponent`). `AST-DEC-036` dan `AST-DEC-039` tidak bisa dijurnal per pusat biaya tanpa perubahan | Rizki (Aset + Akuntansi) | `superseded` → `AST-DEC-047` | Rizki, 2026-11-12 | — |
| `AST-DEC-047` | Decision | Akuntansi diperluas sedikit: kejadian boleh membawa pusat biaya; baris aturan posting yang pusat biayanya kosong memakai pusat biaya dari kejadian. Aset mengirim satu jenis kejadian penyusutan, satu kejadian per kelompok per pusat biaya, dengan kode kelompok sebagai komponen. Perubahan Akuntansi menjadi task lintas modul (`DEP-AST-01`) | Rizki (Akuntansi) | `approved` | Rizki, 2026-11-12 | `AST-CON-003` |
| `AST-DEC-048` | Decision | Mutasi yang diajukan Kepala Unit asal sendiri mencatat tahap 1 sebagai "diajukan dan disetujui unit asal" oleh orang itu; penyetuju Bagian Aset tetap harus orang lain. Mutasi yang diajukan Petugas Aset tetap butuh persetujuan Kepala Unit asal | Rizki | `approved` | Rizki, 2026-11-12 | Kebuntuan `AST-ASM-003` pada mutasi |
| `AST-DEC-049` | Decision | Batas persetujuan Direktur pada penghapusan dihitung dari **total** nilai buku seluruh aset dalam satu usulan | Rizki | `approved` | Rizki, 2026-11-12 | — |
| `AST-DEC-050` | Decision | Batch impor data awal disetujui Kepala Bagian Aset (data aset) lalu Kepala Akuntansi (angka nilai); pengunggah tidak boleh menyetujui | Rizki (Aset + Akuntansi) | `approved` | Rizki, 2026-11-12 | — |

Contoh `AST-DEC-049`: batas Rp 50.000.000; usulan berisi 10 kursi roda bernilai buku Rp 6.000.000 per
unit = Rp 60.000.000 → wajib sampai Direktur, walaupun tidak ada satu unit pun yang melewati batas.

Keadaan akhir: **50 keputusan `approved`** (`AST-DEC-001..050`), decision revision `4`.

---

## 18. Persetujuan desain — putaran 13 (2026-11-12)

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at |
|---|---|---|---|---|---|
| `AST-DEC-051` | Decision | **Desain kelima sub-modul disetujui seluruhnya**: `02-module-map.md` rev 1; kontrak `AST-REG-1.0.0`, `AST-MOV-1.0.0`, `AST-INV-1.0.0`, `AST-NLI-1.0.0`, `AST-HPS-1.0.0`; keputusan desain `AST-DES-001..041`; usulan standar `AST-RCG-G05..G29` (sejak ini berstatus `CONFIRMED`). Diberikan lewat jawaban pertanyaan pilihan "Setujui semua, lanjut roadmap". Persetujuan ini **tidak** memberi wewenang mendaftarkan registry, menulis source, membuat atau menjalankan migration, build, maupun publikasi Git — masing-masing tetap wewenang task terpisah | Rizki | `approved` | Rizki, 2026-11-12 |

Keadaan akhir: **51 keputusan `approved`**, decision revision `5`.
