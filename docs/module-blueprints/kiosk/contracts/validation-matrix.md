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
