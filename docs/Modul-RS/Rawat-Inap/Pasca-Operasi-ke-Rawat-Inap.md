# Module Artifact — Pasca Operasi ke Rawat Inap

## 1. Ringkasan Modul

Artifact ini merangkum bukti visual tentang dokumentasi pasien rawat inap terkait pasca operasi di HiSys. Cakupan yang terlihat meliputi pemantauan infeksi pasca operasi, catatan klinis dokter dan perawat, histori tanda vital, pemantauan transfusi, verifikasi dan pencatatan operasi, serta laporan kunjungan operasi dan transfer ruangan.

Rekaman tidak memperlihatkan transaksi pemindahan pasien dari ruang operasi/recovery room ke tempat tidur rawat inap secara end-to-end. Bukti yang tersedia hanya menunjukkan tab `Transfer Pasien` dan hasil `Laporan Transfer Ruangan`. Audio ada, tetapi tidak dapat ditranskripsikan pada runtime ini; karena itu artifact bertumpu pada layar yang terlihat.

## 2. Evidence yang Diproses

| Source | Type | Status | Coverage | Notes |
|---|---|---|---|---|
| `01-Operasi-ke-Ranap.mp4` | video | processed | 00:00-23:16 | 36 frame merata diperiksa; frame penting diperiksa pada resolusi asli. Audio tidak ditranskripsikan. |

## 3. Actors / Roles

- Dokter dan perawat terlihat sebagai tab peran klinis pada workspace rawat inap.
- Ahli gizi, farmasi klinis, fisioterapis, hemodialisa, pencatat terintegrasi, serta pengelola obat dan alat kesehatan terlihat sebagai kelompok layanan/tab.
- Dokter operator/bedah, dokter anestesi, asisten operator, asisten dokter anestesi, dokter resusitator, dokter jantung, dan anggota tim operasi lain terlihat sebagai peran dalam data operasi.
- Case manager/MPP disebut pada formulir manajemen pelayanan pasien dan skrining pasien.
- User input dan user transfer tercatat sebagai atribut audit pada daftar/laporan.

Label peran membuktikan keterlibatan atau pengelompokan pada UI, bukan batas hak akses masing-masing peran.

## 4. Capability / Feature Registry

| ID | Capability / Feature | Description | Actor | Evidence | Confidence |
|---|---|---|---|---|---|
| CAP-001 | Membuka rekam pasien rawat inap | Menampilkan identitas/registrasi pasien dan area kerja klinis rawat inap. | Pengguna klinis | `01-Operasi-ke-Ranap.mp4` 00:58 | High |
| CAP-002 | Dokumentasi SOAP perawat | Menyediakan area Subjective, Objective, Assessment, dan Planning. | Perawat | `01-Operasi-ke-Ranap.mp4` 00:58, 05:29-07:56 | High |
| CAP-003 | Surveilans infeksi pasca operasi | Mencatat indikator pasca operasi per hari dan informasi kultur/serologi serta tanda infeksi. | Perawat/pengguna klinis | `01-Operasi-ke-Ranap.mp4` 01:37-04:12 | High |
| CAP-004 | Daftar dokumen pasien | Mencari dan melihat dokumen beserta tanggal, penginput, status, dan aksi per baris. | Pengguna klinis | `01-Operasi-ke-Ranap.mp4` 04:50 | High |
| CAP-005 | Histori tanda vital | Menampilkan riwayat tanda vital menurut waktu, registrasi, unit, dan editor. | Pengguna klinis | `01-Operasi-ke-Ranap.mp4` 08:05 | High |
| CAP-006 | Dokumentasi SOAP dokter | Menampilkan tanda vital, prediagnosis, alergi, area SOAP dokter, dan pemilihan template. | Dokter | `01-Operasi-ke-Ranap.mp4` 08:44-09:22 | High |
| CAP-007 | Monitoring transfusi darah | Mendokumentasikan tanda vital sebelum dan sesudah transfusi, jenis darah, reaksi, dan perawat. | Perawat | `01-Operasi-ke-Ranap.mp4` 13:15-14:32 | High |
| CAP-008 | Verifikasi order operasi | Menampilkan data order dan pilihan Approve/Reject berikut keterangan approval. | Verifikator operasi (peran spesifik tidak terlihat) | `01-Operasi-ke-Ranap.mp4` 15:11 | High |
| CAP-009 | Pencatatan keterangan operasi | Mencatat waktu/jenis operasi, ruangan, recovery room, anestesi, detail tindakan, dan tim operasi. | Pengguna ruang operasi | `01-Operasi-ke-Ranap.mp4` 15:50-16:29 | High |
| CAP-010 | Daftar verifikasi OK | Menyaring order dan membuka aksi verifikasi dari daftar operasi. | Pengguna ruang operasi | `01-Operasi-ke-Ranap.mp4` 17:08 | High |
| CAP-011 | Laporan kunjungan OK | Menyaring, menampilkan, mencetak, dan mengekspor data kunjungan operasi. | Pengguna laporan | `01-Operasi-ke-Ranap.mp4` 17:46 | High |
| CAP-012 | Laporan transfer ruangan | Menampilkan perpindahan berdasarkan periode beserta asal/tujuan kelas dan tempat tidur serta user transfer. | Pengguna laporan | `01-Operasi-ke-Ranap.mp4` 18:25 | High |
| CAP-013 | Katalog laporan rawat inap | Menyediakan pilihan sensus, rekap pasien, penggunaan kamar, BOR, kematian, transfer, register, dan laporan lain. | Pengguna laporan | `01-Operasi-ke-Ranap.mp4` 19:04-23:16 | High |
| CAP-014 | Akses tab Transfer Pasien | Tab `Transfer Pasien` tersedia pada kelompok dokumentasi perawat. Isi dan penyimpanannya tidak terlihat. | Perawat | `01-Operasi-ke-Ranap.mp4` 10:01-12:36 | Medium |

## 5. Menu / Screen / UI Elements

### Navigasi utama pasien

- `Data Identity`, `Rawat Inap`, `Dokumen`, `Vital Sign`, `Penunjang Medis`, `Tindakan`, `Medical Equipment`, `Ruang Operasi`, dan `Ruang Bersalin VK`.
- Tombol/konteks pasien yang terlihat: `Past Visit`, `Medical History`, `History Resume`, dan `Pendaftaran Pasien Baru`.

### Workspace Rawat Inap

- Kelompok peran: `Dokter`, `Perawat`, `Ahli Gizi`, `Farmasi Klinis`, `Fisioterapis`, `Hemodialisa`, `Cat. Terintegrasi`, `Obat & Alkes`.
- Menu perawat: `S.O.A.P Perawat`, `Pengkajian Pasien`, `Catatan Perkembangan`, `Rencana Pulang`, `Transfer Pasien`, `Asesmen Edukasi`, `Transfusi Darah`, `Ringkasan Masuk dan Keluar`.
- Form terkait: `Nosokomial Formulir A (Lama)`, `Nosokomial Formulir A Baru`, `Pengkajian dan Intervensi Nyeri`, `Form. A Evaluasi Awal Manajemen Pelayanan Pasien`, `Form. B Manajemen Pelayanan Pasien / Case Manager`, dan `Seleksi / Skrining Pasien MPP`.
- Menu dokter: `S.O.A.P Dokter`, `Kajian Pasien`, `Visite Dokter`, `Resume Medis Ranap`, `Resume Medis ODC`, dan `Resep Elektronik`.

### Surveilans pasca operasi

- Grid hari ke-1 sampai ke-15.
- Indikator: suhu >= 38 derajat, drainase, pus, perforasi, dan fistula.
- Kultur: pilihan Ya/Tidak, tanggal, dan hasil.
- Serologi: HBsAg dan Anti HCV dengan pilihan Positif/Negatif.
- Tabel surveilans: lokasi, rentang tanggal, nyeri, merah, bengkak, pus, menggigil, suhu tinggi, hari ke, dan keterangan.

### Dokumen, vital sign, dan SOAP

- Dokumen Pasien: pencarian, tanggal, dokumen, user input, status, dan aksi baris. Status `draft` terlihat.
- Histori Vital Sign: tanggal/waktu, nomor registrasi, unit registrasi, dan editor.
- SOAP dokter: kartu TD, BB, HR, TB/PB, RR, suhu; prediagnosis; allergies; area SOAP; dan tombol `SOAP Template`.
- SOAP perawat: area Subjective, Objective, Assessment, dan Planning.

### Monitoring transfusi darah

- Jam/tanggal darah diterima dan jenis darah.
- TD, suhu, dan nadi sebelum transfusi; 15 menit, 1 jam, dan 4 jam setelah darah masuk.
- Reaksi transfusi dan perawat.
- Aksi `Tambah Row`, `Hapus Row`, `Simpan`, dan `Print`.

### Operasi dan pelaporan

- Verifikasi order: tanggal/jam, pilih jadwal, ruangan, tipe tindakan, kelas, diagnosis, berat badan, operator, rencana tindakan, jenis anestesi, penandaan lokasi, persetujuan, penjamin, pilihan Approve/Reject, dan keterangan approval.
- Keterangan operasi: tanggal/jam, tipe operasi/tindakan, pemakaian ruangan/biaya, recovery room, tim operasi, tipe anestesi/ASA/kelompok pasien/departemen, detail tindakan, operator, pembagian share/diskon, dan kategori operasi.
- List Verifikasi OK: filter periode, kamar, status, tipe OK; tabel ringkasan order; tombol `Verifikasi`; legenda merah untuk order rejected.
- Laporan Kunjungan OK: filter klinis/administratif dan aksi `Tampil`, `Print`, `Tutup`, `Excel`.
- Laporan Transfer Ruangan: periode, waktu transfer, nomor rekam medis, pasien, kelas/bed asal, kelas/bed tujuan, user transfer, dan `Simpan ke Excel`.

## 6. Business Process / Ordered Flow

### BP-001 — Dokumentasi klinis pasien rawat inap

1. Pengguna klinis membuka konteks pasien dan menu `Rawat Inap`; hasilnya adalah workspace pasien dengan kelompok peran dan formulir klinis (`01-Operasi-ke-Ranap.mp4` 00:58).
2. Perawat dapat membuka SOAP, pengkajian, catatan perkembangan, rencana pulang, transfer, edukasi, transfusi, atau ringkasan masuk/keluar; isi SOAP menampung S/O/A/P (`01-Operasi-ke-Ranap.mp4` 00:58, 05:29-07:56).
3. Dokter dapat membuka SOAP dokter dan melihat tanda vital terkini sebelum mendokumentasikan catatan klinis (`01-Operasi-ke-Ranap.mp4` 08:44-09:22).
4. Pengguna dapat meninjau histori tanda vital dan daftar dokumen pasien sebagai bukti rekam longitudinal (`01-Operasi-ke-Ranap.mp4` 04:50, 08:05).

### BP-002 — Surveilans pasca operasi

1. Pengguna membuka Formulir A surveilans infeksi nosokomial dalam workspace perawat (`01-Operasi-ke-Ranap.mp4` 01:37).
2. Data label pasien dan tanggal masuk/keluar menjadi konteks formulir (`01-Operasi-ke-Ranap.mp4` 03:33-04:12).
3. Pengguna mencatat indikator pasca operasi per hari, status kultur/hasil, dan/atau tanda infeksi menurut lokasi serta rentang tanggal (`01-Operasi-ke-Ranap.mp4` 01:37-02:55).
4. Bukti tidak memperlihatkan aksi simpan atau validasi hasil pencatatan.

### BP-003 — Monitoring transfusi

1. Perawat membuka `Transfusi Darah` dan menambah baris monitoring (`01-Operasi-ke-Ranap.mp4` 13:15-14:32).
2. Perawat mencatat waktu penerimaan darah, kondisi sebelum transfusi, jenis darah, dan tanda vital pada interval setelah darah masuk.
3. Perawat mencatat reaksi transfusi dan identitas perawat, lalu tersedia aksi simpan atau cetak. Keberhasilan penyimpanan tidak didemonstrasikan.

### BP-004 — Verifikasi dan pencatatan operasi

1. Pengguna membuka order operasi dan meninjau jadwal, ruangan, tindakan, diagnosis, operator, anestesi, penandaan lokasi, persetujuan, dan penjamin (`01-Operasi-ke-Ranap.mp4` 15:11).
2. Pengguna memilih `Approve` atau `Reject` dan dapat memberi keterangan approval. Efek setelah keputusan tidak terlihat (`01-Operasi-ke-Ranap.mp4` 15:11).
3. Pada `Keterangan Tentang Operasi`, pengguna dapat melengkapi data aktual operasi, recovery room, tim, anestesi, detail tindakan, operator, dan kategori (`01-Operasi-ke-Ranap.mp4` 15:50-16:29).
4. `List Verifikasi OK` menampilkan order berdasarkan filter dan menyediakan aksi `Verifikasi`; order rejected ditandai lewat legenda merah (`01-Operasi-ke-Ranap.mp4` 17:08).

### BP-005 — Pelaporan operasi dan perpindahan ruangan

1. Pengguna memilih filter pada `Laporan Kunjungan OK` lalu dapat menampilkan, mencetak, atau mengekspor hasil (`01-Operasi-ke-Ranap.mp4` 17:46).
2. Pengguna memilih periode pada `Laporan Transfer Ruangan` untuk melihat asal/tujuan kelas dan tempat tidur serta user transfer (`01-Operasi-ke-Ranap.mp4` 18:25).
3. Bukti laporan mengonfirmasi data perpindahan tersedia, tetapi tidak mengonfirmasi bagaimana transaksi transfer dibuat atau divalidasi.

## 7. Business Rules, Validation, and Exceptions

| ID | Rule / Validation / Exception | Evidence | Confidence |
|---|---|---|---|
| RULE-001 | Pemantauan pasca operasi disusun per hari ke-1 sampai ke-15 untuk indikator suhu, drainase, pus, perforasi, dan fistula. | `01-Operasi-ke-Ranap.mp4` 01:37 | High |
| RULE-002 | Monitoring transfusi menyediakan titik ukur sebelum transfusi serta 15 menit, 1 jam, dan 4 jam setelah darah masuk. | `01-Operasi-ke-Ranap.mp4` 13:15-14:32 | High |
| RULE-003 | Order operasi menyediakan dua keputusan verifikasi: Approve atau Reject, dengan keterangan approval. | `01-Operasi-ke-Ranap.mp4` 15:11 | High |
| RULE-004 | Daftar verifikasi menjelaskan order rejected dengan penanda/legenda merah. | `01-Operasi-ke-Ranap.mp4` 17:08 | High |
| RULE-005 | Laporan transfer membedakan asal dan tujuan berdasarkan kelas serta nomor tempat tidur. | `01-Operasi-ke-Ranap.mp4` 18:25 | High |
| RULE-006 | Status `draft` dapat muncul pada daftar dokumen pasien. Arti transisi status berikutnya tidak terlihat. | `01-Operasi-ke-Ranap.mp4` 04:50 | Medium |

Tidak ada pesan validasi, aturan field wajib, atau skenario gagal simpan yang terlihat jelas.

## 8. Data / Input / Output Observed

- Konteks pasien: nomor rekam medis, nama, nomor registrasi, jenis kelamin/tanggal lahir/umur, dokter, unit/penjamin, catatan penting, alergi, diagnosis, kelas, dan tanggal masuk/keluar. Nilai identitas contoh tidak direproduksi.
- Data klinis: TD, BB, HR, TB/PB, RR, suhu, prediagnosis, alergi, SOAP, indikator infeksi, kultur/hasil, serologi, dan reaksi transfusi.
- Data operasi: tanggal/jam, jadwal, ruangan, tipe tindakan/operasi, kelas, diagnosis, berat badan, rencana/detail tindakan, anestesi/ASA, penandaan lokasi, persetujuan, penjamin, tim operasi, recovery room, pemakaian ruangan/biaya, operator, share, dan diskon.
- Data transfer: timestamp, nomor rekam medis/pasien, kelas dan tempat tidur asal, kelas dan tempat tidur tujuan, serta user transfer.
- Output: daftar dokumen, histori vital sign, daftar verifikasi, laporan kunjungan OK, laporan transfer ruangan, hasil cetak, dan ekspor Excel.

## 9. Integrations / Dependencies Mentioned

- Workspace Rawat Inap terhubung secara UI dengan Dokumen, Vital Sign, Penunjang Medis, Tindakan, Medical Equipment, Ruang Operasi, dan Ruang Bersalin/VK.
- Dokumentasi rawat inap mengelompokkan kontribusi dokter, perawat, gizi, farmasi klinis, fisioterapi, hemodialisis, catatan terintegrasi, serta obat dan alat kesehatan.
- Data operasi tampak menjadi sumber untuk daftar verifikasi dan Laporan Kunjungan OK.
- Data perpindahan kelas/tempat tidur tampak menjadi sumber Laporan Transfer Ruangan. Mekanisme integrasi dan sinkronisasinya tidak terlihat.

## 10. Conflicts in Evidence

Tidak ada kontradiksi eksplisit antar-sumber karena hanya satu video diproses. Namun, nilai pasien contoh pada layar verifikasi operasi dan laporan tidak selalu sama; karena itu layar-layar tersebut diperlakukan sebagai demonstrasi capability, bukan satu perjalanan pasien yang kontinu.

## 11. Unresolved Questions

- Apa langkah dan formulir lengkap untuk memindahkan pasien dari ruang operasi/recovery room ke bed rawat inap?
- Status apa saja yang berlaku pada transfer dan order operasi, serta siapa yang berwenang mengubahnya?
- Field mana yang wajib dan validasi apa yang muncul saat simpan, approve, reject, atau transfer?
- Apakah persetujuan operasi dan penandaan lokasi menjadi prasyarat sistem untuk approval?
- Bagaimana data operasi, vital sign, transfusi, dan transfer disinkronkan dengan billing, bed management, farmasi, atau modul lain?
- Apa isi penjelasan audio yang tidak dapat ditranskripsikan, khususnya bila memuat aturan yang tidak tampak pada layar?
- Apakah tab `Transfer Pasien` membuat transaksi yang kemudian tampil di `Laporan Transfer Ruangan`? Hubungan ini masuk akal, tetapi tidak didemonstrasikan.

## 12. Source Coverage

| Source | Type | Status | Coverage | Extraction note |
|---|---|---|---|---|
| `01-Operasi-ke-Ranap.mp4` | video | processed new | 00:00-23:16 | Metadata diperiksa; 36 frame merata dan frame penting beresolusi asli diperiksa. Audio tersedia tetapi tidak ditranskripsikan. |

- ZIP containers: 0
- Evidence files/members total: 1
- Processed new/changed: 1
- Reused from cache: 0
- Unreadable/unsupported: 0
- Extraction scope: `/workspace/agent_files/Pasca Operasi ke Rawat Inap/` only
