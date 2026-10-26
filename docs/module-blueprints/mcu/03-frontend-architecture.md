# MCU (Medical Check-Up) — Arsitektur Frontend

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` revision 1, `approved` 2026-10-21; `MCU-FE-002` `approved` 2026-10-21 lewat [05-screen-brief.md](05-screen-brief.md) |
| Repository | `QuilvianSystemFrontendDev` (v2), SHA `7fb1e1d3e` |
| Kedalaman | Kontrak fungsional (sesuai kontrak keluaran): kebutuhan layar, aksi per peran, data/status, penanganan state, kewenangan UI |
| Acuan alur | Keputusan wawancara; v1 hanya referensi tampilan (`MCU-DEC-016`, `MCU-FE-001`) |

> **Menu, route, tab/modal, dan tata letak sudah ditetapkan** di [05-screen-brief.md](05-screen-brief.md)
> (`MCU-FE-002`, `approved` 2026-10-21). Bila dokumen ini dan brief berbeda soal letak atau bentuk
> layar, brief yang berlaku.

## 1. Kebutuhan layar (fungsional)

| ID | Kebutuhan | Peran | Data yang dikonsumsi | Aksi |
| --- | --- | --- | --- | --- |
| `FE-MCU-01` | Kelola paket | Admin MCU | `GET/POST/PUT packages`, harga perusahaan | Buat/ubah paket, komponen, persyaratan, nilai pengembalian & jasa, harga perusahaan, nonaktifkan |
| `FE-MCU-02` | Kelola jenis Pemeriksaan MCU dan kategori kelayakan | Admin MCU | `examination-types`, `fitness-categories` | Buat/ubah isian formulir |
| `FE-MCU-03` | Jadwal & kuota | Admin MCU, kepala unit | `daily-schedules` | Atur rentang tanggal, kuota, dan dokter MCU jaga (`MCU-DEC-042`); setujui pelampauan (kepala unit) |
| `FE-MCU-04` | **Daftar Pasien MCU** | Seluruh peran MCU | `GET orders` + `summary` | Saring per tanggal/status/pintu/perusahaan/dokter; tampil *n dari m komponen tuntas* |
| `FE-MCU-05` | Booking & impor rombongan | Admisi | `POST orders/bookings`, `booking-imports`, `packages/options`, `daily-schedules` | Booking pasien lama/identitas sementara; unggah daftar; jadwal ulang; batal |
| `FE-MCU-06` | Check-in & *walk-in* admisi | Admisi | `patient-matches`, `check-in/preview`, `POST check-in`, Patient Management `POST patients` | Cocokkan pasien (konfirmasi bila tanggal lahir beda), buat pasien baru berisi isian awal, pilih dokter MCU (bawaan: dokter jaga hari itu), tandai persyaratan |
| `FE-MCU-07` | Pemeriksaan | Petugas/dokter MCU | `start-examination`, `components`, `examination-result` | Mulai pemeriksaan; lihat status komponen; isi Pemeriksaan MCU; tambah/batalkan komponen (dokter); jadwal ulang komponen |
| `FE-MCU-08` | Resume | Dokter MCU yang ditetapkan | `resumes` | Susun draf, approve, buka ulang; lihat versi lama |
| `FE-MCU-09` | Verifikasi & penyerahan | Petugas MCU | `resumes/current/verify|return`, `result-document`, `deliveries`, `release-consents` | Verifikasi/kembalikan; cetak lengkap/ringkasan; catat penyerahan; catat persetujuan; catat pemberitahuan koreksi |
| `FE-MCU-10` | Ganti dokter MCU | Kepala unit | `PATCH mcu-doctor` | Pilih dokter pengganti + alasan |
| `FE-MCU-11` | Tombol rekomendasi MCU | Dokter RJ/RI/IGD | `POST orders/recommendations` | Pilih paket + tanggal — **dikerjakan di modul RJ/RI/IGD** |
| `FE-MCU-12` | Kiosk: check-in & *walk-in* MCU | Peserta | `kiosk/*` | Dokter MCU = dokter jaga (`MCU-DEC-042`); tanpa dokter jaga diarahkan ke admisi. Di luar MVP; milik modul kiosk (`MCU-COORD-006`) |

## 2. Kewenangan UI

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
| --- | --- | --- | --- | --- | --- |
| `MCU-FE-001` | Acuan alur layar | Yoga | `approved` | Keputusan wawancara; v1 referensi tampilan | `MCU-DEC-016` |
| `MCU-FE-002` | Menu, route, tab/modal/drawer, tata letak | Yoga | `approved` | Sesuai `05-screen-brief.md`; `DEV_DISCRETION` hanya pada rentang brief bagian 7 | Yoga Aji Pratama, 2026-10-21 |
| `MCU-FE-003` | Privasi ringkasan perusahaan | Yoga | `approved` | Ringkasan disusun server; layar tidak boleh menyusun ringkasan dari data lengkap | `MCU-DEC-010` |
| `MCU-FE-004` | Komponen bentuk (form control, tabel, filter) | Konvensi proyek | `approved` | Pakai komponen yang ada (`BaseSelectField`/`FilterSelect`, tabel standar); isian angka desimal **tidak** memakai `type: "number"` mentah — pakai pola `DECIMAL_FIELD_PROPS`/`parseDecimal` | Pelajaran Lab: `BaseTextField` number membuang desimal |
| `MCU-FE-005` | Warna, ikon, jarak | Konvensi proyek → `DEV_DISCRETION` | `approved` | Mengikuti design system yang ada; warna status dari satu peta konstanta | — |

## 3. Data dan status yang dikonsumsi

| Status order (`MCU-DEC-031`) | Tampil di Daftar Pasien MCU | Aksi yang boleh ditampilkan |
| --- | --- | --- |
| Terjadwal | Ya | Check-in (hari H), jadwal ulang, batal |
| Tidak Hadir | Ya | Jadwal ulang |
| Hadir | Ya | Mulai pemeriksaan, batal penuh |
| Dalam Pemeriksaan | Ya | Detail komponen, isi Pemeriksaan MCU, batal penuh (bila belum dimulai) |
| Menunggu Resume | Ya | Resume (dokter yang ditetapkan) |
| Disetujui Dokter | Ya | Verifikasi/kembalikan (petugas), buka ulang (dokter) |
| Terverifikasi | Ya | Cetak & serahkan |
| Diserahkan | Ya | Serahkan lagi, buka ulang |
| Dibatalkan | Ya (penyaring) | Lihat saja |

Tombol aksi ditampilkan menurut **status + izin + kecocokan dokter yang ditetapkan**, tetapi server tetap
penentu akhir; layar wajib menampilkan pesan `403`/`409` apa adanya.

## 4. Penanganan state

| Keadaan | Perilaku wajib |
| --- | --- |
| Memuat | Indikator muat pada tabel dan tombol aksi |
| Kosong | Daftar Pasien MCU tanpa data menampilkan *"Belum ada peserta MCU pada tanggal ini."* |
| Gagal muat | Pesan + tombol *Coba lagi*; tidak menampilkan data lama tanpa tanda |
| Kiriman ganda | Tombol aksi nonaktif selama permintaan berjalan; check-in, mulai pemeriksaan, approve, dan penyerahan juga dijaga server (`Version`, idempotensi) |
| Data basi | Respons `409` *diubah petugas lain* → muat ulang detail order otomatis lalu tampilkan pesan |
| Hasil Lab/Rad berubah | Tombol *Muat ulang status komponen* (`components/refresh`); tidak ada polling otomatis pada rilis pertama |
| Privasi | NIK dimasking di daftar (mis. `3201********0001`); hasil lengkap hanya di detail berizin |
| Aksesibilitas | Label pada setiap isian; pesan galat terbaca pembaca layar; navigasi papan ketik pada formulir hasil |
| Responsif | Daftar dan formulir dapat dipakai di layar 1366 px; kiosk mengikuti konvensi kiosk |

## 5. Ketergantungan uji frontend

| Uji | Cara |
| --- | --- |
| Unit aturan (pemetaan status → aksi, masking NIK, parsing desimal) | `node --import ./tests/helpers/register.mjs --test tests/unit/` |
| E2E | Pola spec API-tiruan standalone; output ke scratchpad; predikat cegatan memuat `/v1/` |
| UAT | Skenario `04-prd-to-mvp.md` bagian 18 |

## 6. Titik sentuh frontend modul lain

| Modul | Perubahan | Milik |
| --- | --- | --- |
| Kiosk | Pilihan MCU diarahkan ke endpoint `kiosk/*` MCU, bukan pendaftaran poli Klinik MCU | `kiosk` — di luar MVP (`EPIC MCU-08`, `MCU-COORD-006`) |
| RJ/RI/IGD | Tombol *Rekomendasikan MCU* | Modul masing-masing |
| Lab/Rad | Label *Dicakup paket MCU* pada detail pesanan | Modul masing-masing |
