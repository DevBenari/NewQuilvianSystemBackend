# Panduan Uji Putaran A — Form Permintaan Rawat Inap (`BE-IGD-072` + `FE-IGD-059`)

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-072`, `FE-IGD-059` — diuji bersama dari layar perawat (pasangan BE/FE) |
| Acceptance yang dibuktikan | `AT-IGD-221`…`224`, `227`; BE-IGD-072 acceptance 1–7; FE-IGD-059 acceptance 1–6 |
| Kontrak | API `0.17.1` §12.2–12.3, validation `0.16.0` §14, state `0.11.0` §11.1 |
| Lingkungan | Backend hasil build Rizki 12 Oktober 2026 15.02 dengan migration `20261012080102_AddEmergencyAdmissionRequest`; frontend `npm run dev` port 3000 |
| Disusun | 12 Oktober 2026, agent |

Panduan ini tidak menggantikan UAT. Hasilnya menjadi bukti *DevVerif* untuk kedua task; UAT tetap milik tim terpisah.

## 1. Prasyarat — dikerjakan sebelum skenario

| No | Prasyarat | Siapa | Cara memeriksa |
| ---: | --- | --- | --- |
| P1 | Jenis tindak lanjut `RANAP` bertanda `requiresAdmissionRequest = true` | Rizki, lewat API master | `GET /api/v1/health-services/emergency-installation-management/master-data/emergency-disposition-types` → baris `RANAP` memuat `"requiresAdmissionRequest": true`. Untuk mengubahnya: `PUT /…/emergency-disposition-types/{id}` dengan seluruh isi baris itu dan `"requiresAdmissionRequest": true` |
| P2 | Butir hak akses `EmergencyAdmissionRequest` diberikan lewat layar Pengaturan → Manajemen Role → Akses Role: Perawat IGD `Read`, `Create`, `Update`, `Cancel`; Dokter IGD `Read`, `Sign` | Pengurus Akses Role | Tanpa butir ini layar menampilkan *"Anda tidak memiliki hak akses Form Permintaan Rawat Inap."* — itu temuan prasyarat, bukan cacat |
| P3 | Satu pasien IGD dengan DPJP IGD aktif = dokter yang terhubung ke akun `ranger.biru@admin.com`, dan tindak lanjut `RANAP` yang sudah **dikonfirmasi** dokter, dengan unit tujuan, alasan, dan instruksi terisi | Rizki / penguji | Tab *Tindak Lanjut* layar perawat menampilkan baris *Rawat inap* berstatus *Dikonfirmasi* |
| P4 | Satu unit rawat inap yang punya jadwal dokter aktif dan satu yang tidak | Data master yang ada | Dipakai skenario A-04 |
| P5 | Login perawat `dimas.kurniawan` (sandi tidak ditulis di sini) | Penguji | — |

Jangan memakai SuperAdmin untuk menguji, jangan menekan *Simpan Akses* atas nama peran lain, dan jangan menjalankan SQL untuk
membuat skenario lulus.

## 2. Skenario

| ID | Langkah | Hasil yang diharapkan | Bukti |
| --- | --- | --- | --- |
| A-01 | Buka layar perawat → pasien P3 → tab *Tindak Lanjut* | Baris *Rawat inap* memuat kotak *Form Permintaan Rawat Inap* bertuliskan *"Belum ada Form Permintaan Rawat Inap."* dan tombol *Buat Form*. Baris jenis lain (pulang, rujuk) **tanpa** kotak itu. Bila tindak lanjut rawat inap masih *Draf*: kotak tampil, tanpa *Buat Form*, dengan *"Form dapat dibuat sesudah tindak lanjut dikonfirmasi dokter."* | PNG |
| A-02 | Tekan *Buat Form* | Panel form terbuka di bawah daftar, lencana *Draf*. Unit layanan, indikasi klinis, dan rencana perawatan terisi dari tindak lanjut; lama rawat `3`. Identitas, alergi, penjamin, diagnosis tampil baca-saja; alergi kosong tertulis *"Belum ada data alergi"*; diagnosis utama paling atas dengan tanda *(utama)* | PNG |
| A-03 | Sesudah A-02, lihat kotak di baris tindak lanjut; lalu lewat Swagger kirim `POST /emergency-admission-requests` dengan `dispositionId` yang sama | Kotak menampilkan *Buka Form* dan **tidak** lagi *Buat Form*. Swagger: `409` *"Kunjungan ini sudah punya Form Permintaan Rawat Inap yang aktif."* | PNG + JSON |
| A-04 | Pilih unit yang punya jadwal dokter, buka pilihan DPJP; lalu pilih unit tanpa jadwal | Unit berjadwal: hanya dokter yang berjadwal di unit itu. Unit tanpa jadwal: semua dokter aktif dan keterangan *"Unit ini belum punya jadwal dokter; menampilkan semua dokter aktif."* Mengganti unit mengosongkan DPJP usulan | PNG ×2 |
| A-05 | Isi lama rawat `120` → *Simpan Draf* | `400` tampil di bawah form: *"Estimasi lama rawat harus antara 1 sampai 90 hari."*; isian lain **tidak** terhapus | PNG |
| A-06 | Kosongkan indikasi klinis → *Minta TTD …* → konfirmasi | Penolakan tampil di dialog: *"Lengkapi unit layanan, kelas perawatan, DPJP usulan, estimasi lama rawat, dan indikasi klinis sebelum meminta tanda tangan."* | PNG |
| A-07 | Lengkapi semua isian wajib (lama rawat `4`, CITO, *Bantuan oksigen*) → *Minta TTD dr. …* → konfirmasi | Lencana *Menunggu TTD · CITO*; isian menjadi baca-saja; nama DPJP IGD aktif tampil di bagian *Dokter pengirim*; kotak di baris tindak lanjut: *"Tanda tangan diminta pukul …"*; riwayat memuat *Form dibuat* dan *Tanda tangan diminta* | PNG |
| A-08 | Login `ranger.biru@admin.com` → Swagger `GET /emergency-admission-requests/pending-signature` | Form A-07 ada di daftar | JSON |
| A-09 | Dengan akun dokter yang **bukan** DPJP aktif pasien → Swagger `POST /{id}/return` `{ "note": "Uji", "rowVersion": "…" }` | `403` *"Hanya DPJP IGD aktif pasien ini yang dapat menandatangani atau mengembalikan form."* | JSON |
| A-10 | Dengan `ranger.biru@admin.com` → `POST /{id}/return` `{ "note": "Kelas tidak sesuai hak penjamin", "rowVersion": "<dari GET /{id}>" }`; lalu muat ulang layar perawat → *Buka Form* | Form kembali *Draf* dan dapat diubah; kotak kuning *"Dikembalikan … "* memuat catatan itu; riwayat memuat *Dikembalikan ke perawat* beserta catatan | JSON + PNG |
| A-11 | Buka form yang sama di dua tab peramban; ubah dan *Simpan Draf* di tab 1; ubah dan *Simpan Draf* di tab 2 | Tab 2: *"Form sudah diubah orang lain. Muat ulang lalu coba lagi."* dan tombol *Muat ulang form*; isian tab 2 tidak terhapus sebelum tombol itu ditekan | PNG |
| A-12 | *Batalkan Form* → dialog meminta alasan → isi *"Pasien membaik, rencana pulang"* → konfirmasi | Lencana *Dibatalkan*; kotak di baris tindak lanjut memuat alasan, *Lihat Form*, dan *Buat Form* lagi | PNG |
| A-13 | Pasien IGD lain dengan tindak lanjut rawat inap terkonfirmasi tetapi **tanpa** DPJP IGD aktif → *Buat Form* | Form terbuka; peringatan *"Pasien belum punya DPJP IGD. Tetapkan DPJP lebih dulu."*; tombol *Minta TTD* nonaktif | PNG — opsional bila data tersedia |
| A-14 | Swagger `GET /emergency-dispositions?emergencyVisitId=…` | Baris `RANAP` memuat `"requiresAdmissionRequest": true` | JSON |

## 3. Bukti yang dikumpulkan

1. PNG per skenario, satu layar penuh, nama berkas diawali ID skenario.
2. JSON respons Swagger untuk A-03, A-08, A-09, A-10, A-14 dengan token dan sandi **disensor**.
3. Rentang jam pengujian, supaya agent mencocokkan jumlah permintaan dengan `Logs/quilvian-backend-20261012.json`
   (`UserName`, `Path`, `StatusCode`).

Hasil dinilai agent dari bukti mentah, bukan dari ringkasan.
