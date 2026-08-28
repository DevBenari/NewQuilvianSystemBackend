# Human Resource — Business Overview

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Revision | `1` |
| Status | `DRAFT` |
| Design readiness | `PARTIAL` |
| Masukan keputusan | [`00-interview-decisions.md`](./00-interview-decisions.md) revision `3` |
| Masukan bukti | [`01-existing-capability-map.md`](./01-existing-capability-map.md) revision `1.1` |
| Backend SHA | `ecdc135` (branch `AndryZain`) |
| Frontend SHA | `2a1cea784` (branch `AgentCodexFrontend`) |

Dokumen ini mencatat **maksud bisnis yang sudah terverifikasi**. Ia memisahkan fakta, keputusan
yang disetujui pemilik, asumsi, konflik, dan pertanyaan terbuka. Tidak ada kebijakan, pemilik,
invariant, maupun persetujuan yang disimpulkan dari source code.

---

## 1. Maksud modul

### 1.1 Kalimat batas

> Modul `human-resource` mengelola seluruh siklus hidup tenaga kerja rumah sakit — dari
> perencanaan kebutuhan tenaga, rekrutmen, pengangkatan, penempatan, penjadwalan, kehadiran,
> cuti dan lembur, penggajian, kompetensi, kinerja, kredensial dan kewenangan klinis, kesehatan
> kerja staf, sampai pemberhentian — beserta layanan mandiri pegawai dan atasan atas
> transaksi-transaksi tersebut.

### 1.2 Masalah yang diselesaikan

Rumah sakit ini sudah punya fondasi data HR yang sangat luas, tetapi sebagian besar fondasi itu
belum menjadi pekerjaan yang bisa dilakukan orang.

Gambarannya dalam angka, seluruhnya dari capability map:

| Kenyataan | Angka |
| --- | ---: |
| Endpoint HR yang sudah tersedia di backend | 1.343 |
| Endpoint yang benar-benar dipanggil frontend | 81 |
| Endpoint operasional tanpa satu pun pemakai | ± 577 |
| Model yang berada di enam domain tanpa controller | 68 |
| Di antaranya yang benar-benar belum punya API | 67 |
| Menu yang menunjuk halaman tidak ada | 6 |
| Test yang menyentuh HR, backend maupun frontend | 0 |

Contoh yang paling mudah dipahami: seorang pegawai **tidak dapat mengajukan cuti** dari sistem
ini hari ini, padahal backend-nya sudah menyediakan 93 endpoint untuk cuti — mulai dari saldo,
kalender, pengajuan, pembatalan, sampai pencatatan kembali kerja. Yang belum ada hanya
halamannya.

Hal yang sama berlaku untuk lembur, tukar shift, ubah jadwal, koreksi kehadiran, perubahan data
pegawai, dan pengunduran diri. Sebelas dari tiga belas controller layanan mandiri tidak punya
satu pun pemakai.

### 1.3 Hasil yang dituju

| Hasil | Ukuran keberhasilan |
| --- | --- |
| Pegawai dapat mengurus urusan kepegawaiannya sendiri | Cuti, lembur, tukar shift, ubah jadwal, koreksi kehadiran, perubahan data, dan resign dapat diajukan dari akun pegawai |
| Atasan tahu apa yang menunggu keputusannya | Satu kotak masuk memuat seluruh pengajuan yang menunggu, lintas jenis transaksi |
| HR dapat bekerja per periode, bukan per orang | Penetapan gaji, penempatan, dan riwayat dapat dilihat untuk seluruh pegawai sekaligus |
| Kehadiran dapat ditutup dan diserahkan ke payroll | Periode dapat ditutup, dibuka kembali bila perlu, dan diserahkan dengan jejak yang dapat diperbaiki maupun dibatalkan |
| Tidak ada menu yang menipu | Setiap menu yang tampil membawa pengguna ke halaman yang bekerja |

---

## 2. Pelaku

Daftar ini diambil dari PRD dan **belum** diverifikasi terhadap role yang benar-benar ada di
sistem. Verifikasi itu bagian dari fase kontrak, bukan dokumen ini.

| Pelaku | Yang dikerjakan di modul ini | Status verifikasi |
| --- | --- | --- |
| Pegawai | Mengajukan cuti, lembur, tukar shift, koreksi kehadiran, perubahan data, resign; mencatat kehadiran | `UNKNOWN` |
| Atasan atau kepala unit | Menyetujui pengajuan anak buah, menyusun jadwal, menilai kinerja | `UNKNOWN` |
| HR Admin | Mengelola master data, profil pegawai, penempatan, riwayat, penetapan gaji | `UNKNOWN` |
| HR Manager | Menyetujui hal yang melampaui kewenangan atasan, menetapkan kebijakan, menangani pengecualian | `UNKNOWN` |
| Petugas payroll | Menutup periode kehadiran, menjalankan perhitungan, menyerahkan ke Finance | `UNKNOWN` |
| L&D atau Diklat | Mengelola kompetensi, pelatihan wajib, dan sertifikat | `UNKNOWN` |
| Auditor atau tim akreditasi | Membaca bukti tanpa mengubah data | `UNKNOWN` |
| Komite Medik | Kredensial dan kewenangan klinis dokter | `BLOCKED` — di luar fase yang boleh dirancang sekarang |
| Komite Keperawatan | Kredensial dan kewenangan klinis perawat | `BLOCKED` |
| K3RS | Kesehatan dan keselamatan kerja staf | `BLOCKED` |

---

## 3. Yang di dalam dan di luar batas

### 3.1 Di dalam batas dan boleh dirancang sekarang

| Kemampuan | Slice |
| --- | --- |
| Master data HR | fondasi seluruh slice |
| Administrasi kepegawaian | `S-A1` |
| Layanan mandiri pegawai | `S-A2` s.d. `S-A6` |
| Kotak masuk persetujuan terpadu | `S-A7` |
| Kehadiran dan koreksi kehadiran | `S-B1` |
| Cuti, izin, dan saldo | `S-B2` |
| Lembur | `S-B3` |
| Penjadwalan, shift, tukar shift | `S-B4` |
| Payroll sisi HR sampai perhitungan | `S-B5`, sebagian |
| Kompetensi dan pelatihan | `S-C2` |
| Manajemen kinerja | `S-C3` |
| Lifecycle dan offboarding | `S-C4` |
| Hubungan karyawan dan kedisiplinan | `S-C5` |

### 3.2 Di dalam batas tetapi belum boleh dirancang

| Kemampuan | Alasan | Menunggu |
| --- | --- | --- |
| Kredensial, lisensi, kewenangan klinis, SPK/RKK | Batas keselamatan klinis belum ditetapkan pihak berwenang | `requirement-completeness-gate`, `hospital-domain-architect`, lalu Komite Medik |
| OPPE dan FPPE | Sama, ditambah belum ada satu pun entity maupun endpoint | Sama |
| Kesehatan dan keselamatan kerja staf | Aturan akses data kesehatan pribadi belum disahkan | Kedua skill hulu, lalu K3RS |
| Perencanaan tenaga kerja, rekrutmen, benefit, layanan HR, perjalanan dinas | Skema perlu diturunkan ulang, tetapi isi tabelnya belum diketahui | `HRD-Q-05`, audit database |
| Bentuk serah terima payroll ke Finance | Batas tanggung jawab sudah final, bentuk datanya belum | `HRD-Q-10`, `HRD-Q-11` |

### 3.3 Di luar batas — milik modul lain

| Kemampuan | Pemilik | Titik sentuh yang boleh dibahas |
| --- | --- | --- |
| Pembayaran, posting akuntansi, pajak, pelaporan payroll | Finance | Bentuk data serah terima, dan apa yang terjadi bila batch ditolak |
| Data klinis pasien, tindakan, volume, mutu layanan | Health Services | Sumber angka OPPE/FPPE, pengecekan kewenangan saat pelayanan |
| Akun aplikasi, role, permission, pencabutan akses | Administrator / Identity | Perintah buat akun saat onboarding, cabut akses saat offboarding |
| Penyimpanan berkas dan dokumen | Shared platform | Cara menyimpan ijazah, STR, SIP, sertifikat, hasil MCU |
| Jadwal praktik dokter untuk pendaftaran pasien | Health Services | Sudah dipisahkan tegas oleh `HRD-DEC-006` |
| Pengendalian infeksi dan tindak lanjut pajanan | PPI / K3RS | Kepemilikan proses saat terjadi tertusuk jarum |

---

## 4. Catatan bisnis yang terverifikasi

| ID | Type | Pernyataan | Owner | Status | Bukti / persetujuan |
| --- | --- | --- | --- | --- | --- |
| `HRD-BO-001` | Fact | Backend HR menyediakan 1.343 endpoint pada 150 controller, tetapi frontend hanya memanggil 81 di antaranya | — | Terverifikasi | Capability map §2 |
| `HRD-BO-002` | Fact | Sebelas dari tiga belas controller layanan mandiri tidak punya pemakai di frontend | — | Terverifikasi | Capability map §7.3 |
| `HRD-BO-003` | Fact | Enam menu Administrasi Kepegawaian menunjuk halaman yang tidak ada, meski datanya sudah dapat diubah dari halaman detail pegawai | — | Terverifikasi | Capability map §7.2 |
| `HRD-BO-004` | Fact | Tidak ada satu pun test yang menyentuh HR di kedua repository | — | Terverifikasi | Capability map §5, `HRD-TF-007` |
| `HRD-BO-005` | Fact | Enam puluh delapan model di enam domain tidak memiliki controller, tetapi tabelnya sudah dibuat migration `20260726161839_initializeBigModulHRD2` | — | Terverifikasi | Capability map §8.2 |
| `HRD-BO-006` | Decision | Modul dirancang sebagai satu blueprint utuh untuk 21 capability; batas rilis ditulis di `04-prd-to-mvp.md` | Pengguna | `approved` | `HRD-DEC-003` |
| `HRD-BO-007` | Decision | Tanggung jawab HR atas payroll berhenti setelah serah terima dijalankan; pembayaran milik Finance | Pengguna | `approved` | `HRD-DEC-009` |
| `HRD-BO-008` | Decision | Jadwal kerja HR bukan sumber jadwal praktik dokter, dan HR bukan jalur kritis pendaftaran pasien | Pengguna | `approved` | `HRD-DEC-006` |
| `HRD-BO-009` | Decision | Persetujuan memakai satu kotak masuk, tetapi aturan bisnisnya tetap per jenis transaksi | Pengguna | `approved` | `HRD-DEC-011`, `HRD-DEC-018` |
| `HRD-BO-010` | Decision | Jam praktik dokter di luar jadwal kerjanya dicatat sebagai pengecualian yang menunggu keputusan atasan, bukan lembur otomatis | Pengguna | `approved` | `HRD-DEC-013` |
| `HRD-BO-011` | Decision | Rekam kesehatan kerja hanya dapat dibaca K3RS dan pegawai bersangkutan; pihak lain hanya melihat status kelayakan kerja | Pengguna, menunggu K3RS | `draft` | `HRD-DEC-010` |
| `HRD-BO-012` | Decision | Kredensial kedaluwarsa memberi peringatan tercatat, tidak menghentikan pelayanan | Komite Medik | `draft` | `HRD-DEC-005` |
| `HRD-BO-013` | Assumption | Pegawai memiliki akun aplikasi yang identitasnya dapat dipetakan ke profil workforce | — | Belum diuji | `HumanResourceContextService` menurunkan konteks dari pengguna terautentikasi |
| `HRD-BO-014` | Assumption | Atasan yang berwenang menyetujui dapat ditentukan dari penempatan atasan yang berlaku | — | Belum diuji | `WfpManagerAssignment` |
| `HRD-BO-015` | Conflict | PRD menyebut cakupan fungsional existing sekitar 83%; angka itu tidak dapat direproduksi dari bukti mana pun | Pemilik produk | Terbuka | `HRD-CONF-03` |
| `HRD-BO-016` | Conflict | Empat puluh entity memakai prefix `Wfp` yang tidak terdaftar di registry | Pemilik modul | Sedang ditangani | `HRD-TF-002`. Diselesaikan lewat `HRD-DEC-019` dengan **mendaftarkan `Wfp` sebagai prefix yang sah**, bukan dengan memigrasikannya |
| `HRD-BO-017` | Open Question | Nama pemilik kebijakan bisnis, wakil Komite Medik, dan wakil K3RS | Manajemen | Terbuka | `HRD-Q-01` |
| `HRD-BO-018` | Open Question | Apakah tabel 67 entity tanpa API sudah berisi data dari impor manual atau migrasi V1 | Pemilik database | Terbuka | `HRD-Q-05` |
| `HRD-BO-019` | Open Question | Bentuk data serah terima payroll dan perilaku bila Finance menolak batch | Pemilik produk bersama Finance | Terbuka | `HRD-Q-10`, `HRD-Q-11` |
| `HRD-BO-020` | Open Question | Dua puluh nilai kebijakan pada PRD pasal 28 belum punya pemilik keputusan | Pemilik produk, Komite Medik, K3RS | Terbuka | `HRD-Q-06` |

---

## 5. Hubungan dengan PRD yang sudah ada

`docs/Modul-RS/PRD_to_MVP_HRD_Quilvian_Target_100.md` dibuat di luar alur blueprint dan
diperlakukan sebagai **masukan produk**, bukan PRD yang berlaku. PRD resmi modul ini lahir
sebagai `04-prd-to-mvp.md` setelah arsitektur dan seluruh kontrak berdiri.

Tujuh konflik antara PRD itu dan source code sudah tercatat pada
[`00-interview-decisions.md`](./00-interview-decisions.md) bagian 5, lengkap dengan usulan
perbaikannya. Yang paling perlu diperhatikan pemilik produk adalah `HRD-BO-015`: angka cakupan
83% tidak punya dasar yang dapat diperiksa, dan sebaiknya diganti dengan hitungan yang punya
rumus atau dihapus.
