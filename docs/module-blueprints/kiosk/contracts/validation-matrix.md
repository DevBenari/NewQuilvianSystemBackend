# Kiosk — Matriks Validasi

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v1` |
| `last_changed_in` | `v1` |
| Status | `approved` |
| Owner | Sukma Giri Pratama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-09-30 |
| `input_revision` | `00-interview-decisions.md` r2; `02-backend-architecture.md` r1 |
| Traceability | KSK-RM-003/004, KSK-OLD-010, KSK-GUA-002, SEC-KSK-001/002, `KSK-DEC-013/016/017/018/019` |

Validasi di layar Kiosk hanya untuk kenyamanan. **Backend selalu memvalidasi ulang** (SEC-KSK-001). Kode `FE` berarti pesan ditampilkan layar tanpa mengirim request.

## 1. Lookup No. RM

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `KSK-VAL-001` | Jenis pencarian | `searchType` kosong atau bukan 1–4 | "Pilih cara pencarian terlebih dahulu." | `400` / `FE` |
| `KSK-VAL-002` | Isian kosong | `value` kosong setelah spasi dibuang | "Nomor wajib diisi." | `400` / `FE` |
| `KSK-VAL-003` | Karakter berbahaya | `value` memuat karakter kontrol, `<`, `>`, atau lebih dari 32 karakter | "Isian mengandung karakter yang tidak diizinkan." | `400` / `FE` |
| `KSK-VAL-004` | KTP | Setelah spasi dibuang, bukan tepat 16 digit angka | "Nomor KTP harus terdiri dari 16 digit." | `400` / `FE` |
| `KSK-VAL-005` | HP — karakter | Memuat karakter selain angka, `+`, spasi, `-`, `.`, `(`, `)` | "Nomor HP hanya boleh berisi angka." | `400` / `FE` |
| `KSK-VAL-006` | HP — bentuk | Setelah dinormalkan (buang non-digit; `0…` → `62…`) tidak diawali `62`, atau panjangnya di luar 9–15 digit | "Nomor HP tidak valid. Contoh: 081234567890." | `400` / `FE` |
| `KSK-VAL-007` | Rate limit | > 10 pencarian dalam satu menit dari perangkat yang sama | "Terlalu banyak percobaan. Silakan coba lagi sebentar." | `429` |
| `KSK-VAL-008` | Kirim ganda | Tombol ditekan lagi selagi pemeriksaan berjalan | Tombol terkunci, bertuliskan "Memeriksa Data..." | `FE` |

Contoh normalisasi HP:

| Input | Hasil normalisasi | Sah? |
| --- | --- | --- |
| `081234567890` | `6281234567890` | Ya |
| `+62 812-3456-7890` | `6281234567890` | Ya |
| `021 555 1234` | `62215551234` | Ya (telepon rumah, `KSK-DEC-018`) |
| `12345` | `12345` | Tidak — tidak diawali `62` |
| `0812abc` | — | Tidak — `KSK-VAL-005` |

## 2. Pesan hasil lookup (bukan penolakan)

| Hasil | Judul | Subteks | Tombol |
| --- | --- | --- | --- |
| Ditemukan | "Pasien Ditemukan" | — (Kartu Pasien ditampilkan) | "Lanjut Pendaftaran Pasien Lama" |
| Tidak ditemukan | "Pasien Belum Terdaftar" | "Data pasien dengan informasi yang dimasukkan belum ditemukan pada sistem rumah sakit." | "Daftar Sebagai Pasien Baru" |
| Cocok ganda (HP) | "Data Perlu Diverifikasi" | "Nomor HP ini terdaftar untuk lebih dari satu pasien. Silakan cari memakai No. KTP atau hubungi petugas pendaftaran." | "Cari dengan No. KTP" |
| Cocok ganda (KTP/kartu) atau hubungi petugas | "Data Perlu Diverifikasi" | "Silakan hubungi petugas pendaftaran." | "Kembali ke Beranda" |
| Gagal teknis | "Data Belum Dapat Diperiksa" | "Data pasien belum dapat diperiksa. Silakan coba kembali." | "Coba Lagi" |

Pesan "hubungi petugas" **tidak pernah** menyebut alasannya (meninggal, diblokir, tidak aktif) — `KSK-INV-009`.

## 3. Flow Pasien Lama

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `KSK-VAL-010` | Lanjut dari Identifikasi | Belum ada pasien terpilih | "Silakan cari data pasien terlebih dahulu." | `FE` |
| `KSK-VAL-011` | Lanjut dari Tujuan Layanan | Belum memilih tujuan; atau memilih Laboratorium tetapi belum menjawab surat dokter | "Silakan pilih tujuan layanan terlebih dahulu." | `FE` |
| `KSK-VAL-012` | Membentuk sesi kiosk di Step 3 | Pembentukan sesi gagal | "Pilihan layanan belum dapat disimpan. Silakan coba lagi atau hubungi petugas." | `FE` (dari `4xx/5xx` `scan-result`) |
| `KSK-VAL-013` | Lanjut dari Pembayaran — Kondisi C | Pasien punya asuransi dan perusahaan tetapi belum memilih satu penjamin utama | "Silakan pilih penjamin utama untuk kunjungan ini." | `FE` |
| `KSK-VAL-014` | Lanjut dari Pembayaran — asuransi/perusahaan | Penjamin dipilih tetapi datanya belum dimuat/tidak sah | "Data penjamin belum lengkap. Silakan pilih ulang." | `FE` |
| `KSK-VAL-015` | Konfirmasi — Penjamin Perusahaan | Relasi perusahaan tidak aktif / tidak eligible / di luar masa berlaku | Pesan backend existing, diawali "Penjamin perusahaan tidak dapat dipakai:" | `400` |
| `KSK-VAL-016` | Lanjut dari Layanan & Dokter | Layanan/poli, dokter, atau jadwal belum dipilih | "Silakan pilih layanan, dokter, dan jadwal praktik terlebih dahulu." | `FE` |
| `KSK-VAL-017` | Loncat step | Upaya membuka step yang prasyaratnya belum selesai | Layar tetap di step terakhir yang sah (tanpa pesan) | `FE` |

## 4. Pencarian Step 1 (klasifikasi input, `KSK-DSN-009`)

| Bentuk input | Diperlakukan sebagai | Jalur |
| --- | --- | --- |
| Tepat 16 digit | No. KTP | `POST kiosk-patient-lookups` `searchType = 1` |
| Diawali `0`, `62`, atau `+62` dan 9–15 digit | No. HP | `POST kiosk-patient-lookups` `searchType = 2` |
| Pola `99-99-99-99` atau tepat 8 digit | No. RM | `GET patients/kiosk?search=` (existing) |
| Hasil pindai kartu berisi `patientId` | Kartu pasien | `GET patients/kiosk/{id}` |
| Hasil pindai kartu asuransi / member | Kartu asuransi / member | `POST kiosk-patient-lookups` `searchType = 3/4` |
| Lainnya (min. 2 huruf) | Nama | `GET patients/kiosk?search=` (existing) |


---

## Amendment PO — Validasi Pendaftaran Pasien Online

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v2` |
| `last_changed_in` | `v2` (bagian ini saja) |
| Status | `approved` |
| Owner | Sukma Giri Pratama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-11-19 (`KSK-DEC-054`) |
| `input_revision` | `00-interview-decisions.md` §13; `02-backend-architecture.md` *Amendment PO* |
| Traceability | `KSK-DEC-031` s.d. `053`; AC 1–20 |

Backend selalu memvalidasi ulang. Kode `FE` berarti layar menolak tanpa mengirim request. Pesan publik tidak pernah menyebut bagian data pasien yang salah.

### PO-V.1 Halaman publik — pengenalan pasien dan kontak

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `KSK-VAL-PO-001` | NIK | Bukan tepat 16 digit angka | "NIK harus terdiri dari 16 digit angka." | `400` / `FE` |
| `KSK-VAL-PO-002` | No. RM | Kosong atau memuat karakter selain angka | "No. Rekam Medis hanya berisi angka." | `400` / `FE` |
| `KSK-VAL-PO-003` | Tanggal lahir | Kosong, di masa depan, atau lebih dari 150 tahun lalu | "Tanggal lahir tidak valid." | `400` / `FE` |
| `KSK-VAL-PO-004` | Pencocokan pasien lama | Tidak cocok, pasien gabungan, non-aktif, atau tanggal lahir berbeda | "Data tidak ditemukan. Periksa kembali isian Anda atau daftar di loket pendaftaran." | `404` |
| `KSK-VAL-PO-005` | Pasien baru: nama | Kosong atau lebih dari 150 karakter | "Nama lengkap wajib diisi." | `400` / `FE` |
| `KSK-VAL-PO-006` | Pasien baru: jenis kelamin | Kosong | "Pilih jenis kelamin." | `400` / `FE` |
| `KSK-VAL-PO-007` | Kontak | Kosong | "Isi No. WhatsApp atau email untuk menerima kode." | `400` / `FE` |
| `KSK-VAL-PO-008` | Kontak berupa nomor | Setelah dinormalkan bukan `08…` dengan panjang sesuai No. HP kiosk (`KSK-DEC-022`) | "No. WhatsApp tidak valid. Contoh: 081234567890." | `400` / `FE` |
| `KSK-VAL-PO-009` | Kontak berupa email | Bentuk email tidak sah atau lebih dari 150 karakter | "Alamat email tidak valid." | `400` / `FE` |
| `KSK-VAL-PO-010` | Captcha | Token kosong atau ditolak Cloudflare | "Verifikasi keamanan gagal. Silakan ulangi." | `400` |
| `KSK-VAL-PO-011` | Captcha | Cloudflare tidak terjangkau dan `FailOpen = false` | "Layanan sedang tidak tersedia. Silakan coba lagi atau daftar di loket pendaftaran." | `503` |
| `KSK-VAL-PO-012` | Batas laju pencocokan | Lebih dari `MatchPermitPerHour` dari satu IP | "Terlalu banyak percobaan. Silakan coba lagi nanti." | `429` |

### PO-V.2 Halaman publik — kunjungan, penjamin, rujukan, consent

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `KSK-VAL-PO-020` | Tanggal kunjungan | Hari ini, kemarin, atau lebih dari 30 hari ke depan | "Pendaftaran online hanya untuk besok sampai 30 hari ke depan. Untuk berobat hari ini, silakan daftar di kiosk rumah sakit." | `400` / `FE` |
| `KSK-VAL-PO-021` | Jadwal dokter | Bukan jadwal poli terpilih, tidak berlaku pada tanggal itu, tidak mengizinkan appointment atau kiosk | "Jadwal dokter tidak tersedia pada tanggal ini. Pilih jadwal lain." | `409` |
| `KSK-VAL-PO-022` | Kuota | Kuota appointment atau kuota pasien penuh | "Kuota dokter pada jadwal ini sudah penuh. Pilih jadwal atau tanggal lain." | `409` |
| `KSK-VAL-PO-023` | Satu per tanggal | Pasien (atau NIK pasien baru) sudah punya pendaftaran Menunggu Check-in pada tanggal itu | "Anda sudah memiliki pendaftaran online pada tanggal ini." | `409` |
| `KSK-VAL-PO-024` | Kunjungan aktif | Pasien lama sudah punya kunjungan Rawat Jalan aktif pada tanggal itu | "Anda sudah terdaftar pada tanggal ini." | `409` |
| `KSK-VAL-PO-025` | Penjamin | Kartu yang dipilih bukan milik pasien atau tidak aktif | "Kartu penjamin tidak dapat dipakai. Pilih kartu lain atau Tunai." | `400` |
| `KSK-VAL-PO-026` | Kartu baru | Penyedia kosong atau nomor polis kosong/lebih dari 50 karakter | "Lengkapi penyedia asuransi dan nomor kartu." | `400` / `FE` |
| `KSK-VAL-PO-027` | Rujukan | Faskes perujuk kosong atau tidak dikenal | "Pilih faskes perujuk." | `400` / `FE` |
| `KSK-VAL-PO-028` | Rujukan | Tanggal surat rujukan di masa depan | "Tanggal surat rujukan tidak boleh melewati hari ini." | `400` / `FE` |
| `KSK-VAL-PO-029` | Berkas rujukan | Lebih dari 5 MB | "Ukuran berkas maksimal 5 MB." | `400` / `FE` |
| `KSK-VAL-PO-030` | Berkas rujukan | Bukan PDF/JPG/PNG (diperiksa dari isi berkas), atau lebih dari 3 berkas | "Unggah PDF, JPG, atau PNG, paling banyak 3 berkas." | `400` / `FE` |
| `KSK-VAL-PO-031` | Consent pasien baru | Belum menyetujui klausul wajib atau belum tanda tangan | "Lengkapi persetujuan umum dan tanda tangan." | `400` / `FE` |
| `KSK-VAL-PO-032` | Tanda tangan | Gambar lebih dari `MaxSignatureKb` | "Tanda tangan terlalu besar. Silakan ulangi." | `400` |
| `KSK-VAL-PO-033` | Batas laju kirim | Lebih dari `RegistrationPermitPerHour` dari satu IP | "Terlalu banyak percobaan. Silakan coba lagi nanti." | `429` |
| `KSK-VAL-PO-034` | Kirim ganda | Tombol Kirim ditekan lagi selagi proses berjalan | Tombol terkunci, bertuliskan "Mengirim..." | `FE` |

### PO-V.3 Check-in di kiosk

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `KSK-VAL-PO-040` | Kode | Setelah dinormalkan bukan 8 karakter dari himpunan kode | "Kode terdiri dari 8 huruf dan angka, contoh K7QM-4TRZ." | `400` / `FE` |
| `KSK-VAL-PO-041` | Kode/identitas + tanggal lahir | Tidak cocok, atau pendaftaran sudah berstatus akhir | "Data tidak sesuai atau pendaftaran sudah tidak berlaku. Periksa kembali atau hubungi petugas." | `404` |
| `KSK-VAL-PO-042` | Hari kunjungan | Tanggal kunjungan bukan hari ini | "Check-in hanya dapat dilakukan pada {tanggal kunjungan}." | `409` |
| `KSK-VAL-PO-043` | Lupa Kode | NIK/No. RM tidak sah | Sama dengan `KSK-VAL-PO-001`/`002` | `400` / `FE` |
| `KSK-VAL-PO-044` | Batas laju kiosk | Lebih dari `CheckInPermitPerMinute` dari satu akun kiosk | "Terlalu banyak percobaan. Silakan coba lagi sebentar." | `429` |
| `KSK-VAL-PO-045` | Kirim ganda | Tombol ditekan lagi selagi pemeriksaan berjalan | Tombol terkunci | `FE` |
| `KSK-VAL-PO-046` | Token | Token kosong, salah, atau kedaluwarsa | "Waktu check-in habis. Silakan ulangi dari awal." | `400` |
| `KSK-VAL-PO-047` | Kanal | `onlineRegistrationId` dikirim dari kanal petugas | "Pendaftaran online hanya dapat di-check-in di kiosk." | `400` |
| `KSK-VAL-PO-048` | Isi kunjungan | Poli, jadwal, dokter, jenis pembayaran, kartu, atau faskes perujuk berbeda dari pendaftaran; `IsAppointment` bukan `true` | "Data check-in tidak sesuai dengan pendaftaran online. Silakan ulangi." | `400` |
| `KSK-VAL-PO-049` | Pasien lama | `PatientId` berbeda dari pendaftaran | Sama dengan `KSK-VAL-PO-048` | `400` |
| `KSK-VAL-PO-050` | Kartu identitas pasien baru | Jenis kartu bukan KTP atau KIA | "Untuk check-in pendaftaran online, gunakan KTP atau KIA. Bila tidak membawa, silakan ke loket pendaftaran." | `FE` |
| `KSK-VAL-PO-051` | NIK pasien baru | NIK tidak terbaca dari hasil pindai | "NIK tidak terbaca. Pindai ulang kartu, atau ke loket pendaftaran." | `FE` |
| `KSK-VAL-PO-052` | NIK pasien baru | NIK hasil pindai berbeda dari NIK online (layar), atau NIK master pasien yang dikirim berbeda (backend) | "Data kartu identitas tidak sesuai dengan pendaftaran online. Silakan ke loket pendaftaran." | `FE` / `400` |
| `KSK-VAL-PO-053` | Jadwal pada hari H | Jadwal dibatalkan, berubah, atau tidak berlaku hari ini (validasi jalur kunjungan) | "Jadwal dokter berubah, silakan ke loket pendaftaran." | `409` |
| `KSK-VAL-PO-054` | Kuota pada hari H | Kuota pasien total penuh oleh kunjungan lain (validasi jalur kunjungan) | "Kuota dokter sudah penuh, silakan ke loket pendaftaran." | `409` |
| `KSK-VAL-PO-055` | Tabrakan | Pendaftaran berubah status selagi check-in berjalan | "Pendaftaran sudah berubah. Silakan hubungi petugas." | `409` |

### PO-V.4 Layar petugas

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `KSK-VAL-PO-060` | Status | Bukan Menunggu Check-in | "Pendaftaran sudah tidak dapat diubah." | `409` |
| `KSK-VAL-PO-061` | Tanggal baru | Di luar H+1 s.d. H+30 dari hari ini | "Tanggal kunjungan harus antara besok sampai 30 hari ke depan." | `400` / `FE` |
| `KSK-VAL-PO-062` | Jadwal baru | Poli berbeda | "Jadwal baru harus di poli yang sama. Untuk pindah poli, batalkan lalu minta pasien mendaftar ulang." | `400` / `FE` |
| `KSK-VAL-PO-063` | Jadwal baru | Tidak berlaku, tidak mengizinkan appointment atau kiosk | Sama dengan `KSK-VAL-PO-021` | `409` |
| `KSK-VAL-PO-064` | Kuota jadwal baru | Penuh | Sama dengan `KSK-VAL-PO-022`; pendaftaran lama tetap | `409` |
| `KSK-VAL-PO-065` | Tanggal baru | Pasien sudah punya pendaftaran lain atau kunjungan aktif pada tanggal baru | "Pasien sudah memiliki pendaftaran atau kunjungan pada tanggal itu." | `409` |
| `KSK-VAL-PO-066` | Tidak ada perubahan | Tanggal dan jadwal sama dengan sekarang | "Tidak ada perubahan jadwal." | `400` / `FE` |
| `KSK-VAL-PO-067` | Kontak baru | Sama dengan `KSK-VAL-PO-007` s.d. `009` | Sama | `400` / `FE` |
| `KSK-VAL-PO-068` | Pencarian | Kurang dari 3 karakter | "Kata kunci pencarian minimal 3 karakter." | `400` / `FE` |
| `KSK-VAL-PO-069` | Rentang tanggal | Akhir sebelum awal, atau lebih dari 62 hari | "Rentang tanggal tidak valid." | `400` / `FE` |
| `KSK-VAL-PO-070` | Alasan batal | Kosong, kurang dari 5, atau lebih dari 500 karakter | "Alasan pembatalan wajib diisi (5–500 karakter)." | `400` / `FE` |
| `KSK-VAL-PO-071` | Status | Batal pada status akhir | Sama dengan `KSK-VAL-PO-060` | `409` |
| `KSK-VAL-PO-072` | Konkurensi | `expectedRowVersion` tidak cocok | "Data sudah berubah. Muat ulang lalu coba lagi." | `409` |
