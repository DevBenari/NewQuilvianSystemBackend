# Kiosk — Matriks Uji Penerimaan

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` r1 |
| Status | `approved` — Sukma Giri Pratama, 30 Sep 2026 |
| `input_revision` | `00-interview-decisions.md` r2; `KSK-CONTRACT-v1` |
| Traceability | PRD §41–45 (AC-RM, AC-OLD, AC-GUA, AC-SCH, UAT 1–28), `KSK-AC-001..017` |

## Cara membuktikan

Repository backend **tidak** memakai project test otomatis, dan folder `Tests/` dilarang dibuat. Kolom **Jenis** menyatakan sifat skenario, bukan perintah membuat file test.

| Lapisan | Bukti yang dipakai |
| --- | --- |
| Backend statis | `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>`; `dotnet ef migrations has-pending-model-changes --no-build` (harus: tidak ada perubahan); `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict`; `bash tools/authorization-verifier/verify-authorization.sh` (himpunan fallback tidak berubah) |
| Backend runtime | HTTP sungguhan ke app yang dijalankan dari scratchpad terhadap DB `QuilvianNewDevSukma`, dengan data samaran, dicatat per skenario `R0..Rn` beserta keadaan DB sesudahnya. `dotnet test`: `NOT RUN — tidak ada project test` |
| Frontend | `npm run lint` pada berkas yang diubah, `npm run build`, dan uji manual/Playwright di browser berukuran layar Kiosk |

## 1. Cek Nomor Rekam Medis

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| AC-RM-001, UAT 1 | KTP 16 digit milik satu pasien Aktif | Integ (BE runtime) + E2E | `200`, `result = 1`, `patient` berisi 7 field Kartu Pasien saja; layar menampilkan kartu dan tombol "Lanjut Pendaftaran Pasien Lama" |
| AC-RM-003, UAT 2, `KSK-AC-016` | HP tersimpan `+6281234567890`, input `0812-3456-7890` | Integ | `result = 1` |
| `KSK-AC-016` | HP tersimpan `+62215551234`, input `021 555 1234` | Integ | `result = 1` |
| AC-RM-005, UAT 3–4 | KTP / HP tidak terdaftar | Integ + E2E | `result = 2`; layar "Pasien Belum Terdaftar" + "Daftar Sebagai Pasien Baru" |
| AC-RM-002, UAT 5–7, `KSK-AC-017` | KTP kosong / `A123…` / 15 digit | Unit (helper FE) + Integ | Layar: pesan validasi, **tidak ada request**. Backend (dikirim langsung): `400` |
| UAT 8 | HP `12345` / `0812abc` | Unit + Integ | Pesan validasi; backend `400` |
| SEC-KSK-002 | `value` berisi `<script>` atau karakter kontrol | Integ | `400`; tidak ada gema nilai di respons |
| UAT 9, KSK-UX-002 | Backend dimatikan / `500` | E2E | Layar `ERROR` + "Coba Lagi"; **tidak ada** tombol Pasien Baru |
| UAT 10, KSK-UX-003 | Timeout (respons ditahan > 20 detik) | E2E | Layar `ERROR`; tidak dianggap belum terdaftar |
| UAT 11, KSK-UX-001 | Tekan Cek dua kali cepat | E2E | Tab Network: satu request; tombol "Memeriksa Data..." |
| `KSK-AC-001` | HP dipakai dua pasien Aktif | Integ + E2E | `result = 3`, `patient = null`; layar "Data Perlu Diverifikasi" + "Cari dengan No. KTP"; tanpa tombol Pasien Baru |
| `KSK-AC-014` | KTP milik pasien `IsDeceased = true` | Integ + E2E | `result = 4`, `patient = null`; teks tidak memuat kata "meninggal" |
| `KSK-AC-015` | KTP milik pasien A yang digabung ke B (Aktif) | Integ | `result = 1`, `patient.patientId = B` |
| `KSK-DSN-008` | Rantai gabung A→B→A (rusak) | Integ | Tidak hang; `result = 4` |
| `KSK-AC-004`, SEC-KSK-006 | 11 lookup dalam satu menit dari satu akun perangkat | Integ | Permintaan ke-11 `429`; akun perangkat lain tetap `200`; endpoint lain tidak terkena |
| `KSK-AC-008`, SEC-KSK-005 | Periksa log setelah lookup | Integ | Log hanya memuat `****` + 4 digit terakhir; tidak ada KTP/HP/nama utuh |
| SEC-KSK-004 | Bandingkan respons dengan daftar field | Integ | Tidak ada `identityNumber`, `phoneNumber`, `address`, `birthDate` |
| `KSK-DSN-006` | Jalankan authorization verifier | Statis | Exit `0`; `approved-compatibility-fallback.txt` tidak berubah |
| PRIV-3 | Periksa URL dan riwayat browser | E2E | Tidak ada KTP, HP, atau `patientId` di URL mana pun |

## 2. Pendaftaran Pasien Lama

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| AC-OLD-001, UAT 12, `KSK-AC-007` | Dari Cek No. RM tekan "Lanjut Pendaftaran Pasien Lama" | E2E | Step 1 menampilkan pasien itu; URL `/kiosk/registration/old-patient` tanpa query |
| AC-OLD-002, UAT 13 | Identifikasi → Lanjut | E2E | Step 2 Review Data |
| AC-OLD-003, UAT 14 | Review → Lanjut | E2E | Step 3 Tujuan Layanan; bar langkah urutan 8 step baru |
| `KSK-INV-004` | Membuka halaman Pasien Lama langsung | E2E | Step awal = Identifikasi, bukan Tujuan Layanan |
| UAT 15, KSK-OLD-010 | Coba lanjut dari Tujuan Layanan tanpa memilih | E2E | Tetap di Step 3 + pesan |
| AC-OLD-004..007, UAT 16–19 | Jalur poliklinik lengkap | E2E | Step 4 → 5 → 6 → 7 berurutan |
| AC-OLD-008, UAT 20–21 | Konfirmasi → Daftar | E2E + Integ | Kunjungan terbentuk; Step 8 tiket |
| `KSK-AC-010` | Pindai kartu di Step 1, pilih Poliklinik di Step 3 | Integ (DB) | Tepat **satu** sesi kiosk untuk perjalanan itu, `TargetService` kosong, terhubung ke kunjungan |
| `KSK-AC-011`, `KSK-AC-006` | Pindai kartu di Step 1, pilih Laboratorium + surat dokter | Integ (DB) + E2E | Tepat satu sesi, `TargetService = 2`, `HasPhysicianRequest` terisi; tidak ada sesi tanpa target dari perjalanan itu; layar handoff lab |
| `KSK-DSN-004` | Pindai kartu asuransi di Step 1 | Integ + E2E | Lookup `searchType = 3` menemukan pasien tanpa membentuk sesi |
| `KSK-VAL-012` | `scan-result` gagal di Step 3 | E2E | Tetap di Step 3 + pesan; tidak maju |
| `KSK-AC-012`, KSK-BASE-002 | Pasien lahir 12 Sep 1990 | E2E | Review & Konfirmasi: `12 Sep 1990` |
| UAT 27, KSK-BASE-001 | Nama "Muhammad Rizky Aditya Pratama Wijaya" | E2E | Nama utuh di Kartu, Review, Konfirmasi, Tiket (boleh terbungkus baris, tidak terpotong per kata) |
| `KSK-AC-013` | Ketik KTP 16 digit di Step 1 | E2E | Tidak ada GET dengan KTP di URL; console bersih dari data pasien |
| `KSK-AC-005`, UAT 28, SEC-KSK-007 | Diam 120 detik di Review Data | E2E | Peringatan 15 detik; setelah habis, kembali ke Beranda; menekan Back tidak memunculkan data pasien |
| `KSK-GAP-008` | Timeout tiba saat Konfirmasi sedang menyimpan | E2E | Timeout ditunda; kunjungan tidak setengah jadi |

## 3. Penjamin Utama

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| AC-GUA-001, UAT 22 | Pasien punya Asuransi dan Perusahaan | E2E | "Pilih Penjamin Utama" muncul tanpa pilihan terpilih otomatis; Lanjutkan ditolak sebelum memilih |
| AC-GUA-002, UAT 23, `KSK-AC-003` | Pilih Asuransi | Integ (DB) | Kunjungan `PaymentType = 2`, `PatientInsuranceId` terisi; penanda utama di data pasien **tidak** berubah |
| UAT 24, `KSK-AC-009` | Pilih Perusahaan | Integ (DB) + E2E | Kunjungan `PaymentType = 3`, `PatientCompanyGuarantorId` terisi; tidak ada error "Asuransi pasien belum dipilih" |
| `KSK-VAL-015` | Relasi perusahaan kedaluwarsa | Integ | `400` dengan pesan existing; tidak ada kunjungan tersimpan |
| Kondisi A / B | Pasien hanya asuransi / hanya perusahaan | E2E | Alur existing, tanpa layar Pilih Penjamin Utama |
| `KSK-DEC-008` | Periksa layar Pembayaran | E2E | Tidak ada tombol "Jadikan Utama"; tidak ada request `PATCH …/primary` |
| `KSK-DEC-009` | Setelah Konfirmasi berhasil | E2E | Tidak ada jalan kembali ke Pembayaran |
| Regresi | Tunai dan Asuransi dari route kiosk | Integ | Tetap `200` seperti sebelumnya |

## 4. Cek Jadwal Dokter

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| AC-SCH-001, UAT 25 | Buka dropdown Poliklinik di atas grid berisi card | E2E (layar Kiosk 1080×1920 dan 1920×1080) | Seluruh menu terlihat di atas card, dapat di-scroll dan dipilih |
| AC-SCH-002, UAT 26 | Buka dropdown Spesialis sambil kursor/sentuhan di atas card | E2E | Menu tidak tertutup, termasuk saat card dalam keadaan hover |
| Regresi komponen dasar | Buka satu layar non-kiosk yang memakai `FilterSelect` | E2E | Tidak berubah (komponen dasar tidak disentuh) |


---

## Amendment PO — Pendaftaran Pasien Online dan Check-in Kiosk

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` r2 (`approved` — Sukma Giri Pratama, 19 Nov 2026 (`KSK-DEC-054`)) |
| `input_revision` | `00-interview-decisions.md` §13.9 (AC 1–20); `KSK-CONTRACT-v2` |
| Cara membuktikan | Sama dengan bagian atas: tanpa folder `Tests/`; build, `dotnet ef`, QBE, verifier otorisasi, HTTP runtime ke `QuilvianNewDevSukma` dengan data samaran (`R-PO-nn`), dan uji browser/Playwright untuk frontend |

Turnstile memakai kunci uji resmi Cloudflare di lingkungan dev (selalu lolos atau selalu gagal), sehingga jalur captcha dapat dibuktikan tanpa kunci produksi. Email diuji dengan server SMTP uji lokal atau `Email:Smtp:Enabled = false` (status `Failed`/`NotRequested` tercatat).

### PO-T.1 Halaman publik

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| AC 1 | Halaman dibuka di browser tanpa login; urutan langkah Pribadi, Asuransi, Rujukan dibandingkan dengan menu RJ | E2E | Tidak diarahkan ke login; tiga urutan langkah sama dengan `outpatient-registration.constants.js` |
| AC 2 | Pasien lama: No. RM + tanggal lahir benar | Integ + E2E | `200`, hanya `maskedName` dan `payers[]` tersamar; tidak ada NIK/No. RM/alamat di respons |
| AC 2 | Tanggal lahir salah; No. RM tidak ada; pasien non-aktif | Integ | Ketiganya `404` dengan pesan identik byte demi byte |
| AC 3, `INV-PO-01` | Kirim pendaftaran pasien lama untuk besok | Integ | `200`; baris `Status = 1`; `doctor-schedule-quotas` appointment berkurang 1; Daftar Pasien RJ, antrean, dan billing tidak berubah |
| AC 4 | Kirim pendaftaran pasien baru | Integ | Tidak ada baris `MstPatient` baru; `PatientId` kosong |
| AC 5 | Email diisi | Integ (SMTP uji) | Email berisi kode, nomor, tanggal, poli, dokter; tanpa NIK/No. RM; `EmailDeliveryStatus = 1` |
| AC 5 | No. WA diisi | E2E | Tombol membuka `https://wa.me/62…?text=` berisi kode |
| `KSK-DEC-042` | SMTP mati | Integ | Respons `200`, kode tampil, `EmailDeliveryStatus = 2` |
| AC 6 | Kontak kosong | Integ + E2E | `400` `KSK-VAL-PO-007` |
| AC 6 | Captcha kunci uji "selalu gagal" | Integ | `400` `KSK-VAL-PO-010`, tidak ada baris tersimpan |
| AC 6 | Pendaftaran kedua pasien dan tanggal yang sama | Integ | `409` `KSK-VAL-PO-023` |
| AC 6, AC 16 | Tanggal hari ini; tanggal H+31 | Integ + E2E | `400` `KSK-VAL-PO-020` |
| AC 6 | Kirim ke-11 dalam satu jam dari IP yang sama | Integ | `429` |
| AC 14 | Pasien baru menandatangani consent | Integ | Kolom `Consent*` terisi; belum ada `RegPatientGeneralConsent` |
| AC 15 | Berkas 8 MB; berkas `.exe` berganti nama `.pdf` | Integ | `400` `KSK-VAL-PO-029` / `030` |
| `KSK-DSN-PO-013` | Jadwal `IsAllowKioskRegistration = false` | Integ | Tidak muncul di opsi; dikirim langsung → `409` |
| `KSK-DEC-035` | Kartu milik pasien lain dikirim | Integ | `400` `KSK-VAL-PO-025` |

### PO-T.2 Check-in kiosk

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| AC 7 | Kode + tanggal lahir benar pada hari kunjungan, lalu buat kunjungan | Integ + E2E (kiosk) | Verifikasi `200` dengan token; kunjungan `IsAppointment = true`, `RegistrationSource = 7`; status 2; tiket tercetak |
| AC 7 | Kode yang sama dipakai lagi | Integ | `404` `KSK-VAL-PO-041` |
| AC 8 | Check-in sehari sebelum tanggal kunjungan | Integ | `409` menyebut tanggal kunjungan |
| AC 8 | Tanggal lahir salah | Integ | `404` pesan umum |
| AC 9, AC 17 | Pasien baru, KTP dengan NIK sama, NIK belum ada di master | Integ + E2E | Master pasien dari hasil pindai; No. RM terbit; kunjungan terbit |
| AC 9 | Pasien baru, NIK sudah ada di master | Integ | Kunjungan memakai `PatientId` lama; data master tidak berubah |
| AC 17 | KTP dengan NIK berbeda | E2E + Integ (kirim `PatientId` pasien lain langsung) | Layar menolak; backend `400` `KSK-VAL-PO-052`; status tetap 1 |
| AC 17 | Kartu SIM dipilih | E2E | Pilihan SIM tidak tersedia; pesan `KSK-VAL-PO-050` |
| `CAP-PO-37` | NIK hasil OCR dicoba diubah di layar check-in | E2E | Kolom NIK tidak dapat diubah |
| AC 10 | Lupa Kode dengan NIK + tanggal lahir | Integ + E2E | Pendaftaran hari ini ditemukan |
| AC 13 | Jadwal dokter dinonaktifkan sebelum check-in | Integ | `409` `KSK-VAL-PO-053`; status tetap 1; kode masih berlaku |
| AC 14 | Check-in pasien baru dengan consent | Integ | `RegPatientGeneralConsent` dengan `PatientId` baru, `PatientEncounterId`, `SignedAt` = waktu tanda tangan online |
| `KSK-DSN-PO-010` | Check-in pasien rujukan berkas 2 halaman | Integ | Dua `RegEncounterReferralDocument` dengan `StoragePath` sama; berkas terbuka lewat endpoint rujukan kunjungan |
| `KSK-VAL-PO-046` | Token lewat 15 menit | Integ | `400` |
| `KSK-VAL-PO-047` | `onlineRegistrationId` dikirim ke `POST patient-encounters` petugas | Integ | `400` |
| `KSK-VAL-PO-048` | Kiosk mengganti jadwal atau kartu pada permintaan kunjungan | Integ | `400`; status tetap 1 |

### PO-T.3 Penutupan otomatis dan kuota

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| AC 11 | Pendaftaran kemarin belum check-in; penutupan dijalankan | Integ | Status 4, riwayat `NO_SHOW`; kuota kembali |
| AC 11 | Penutupan dijalankan dua kali | Integ | Jumlah riwayat tidak bertambah |
| AC 11 | Server mati dua hari | Integ | Saat menyala, kedua hari diproses |
| AC 12 | Pasien lama punya pendaftaran hari ini lalu mendaftar lewat kiosk Pasien Lama | Integ | Status 3, `EncounterId` terisi; kode ditolak di kiosk |
| AC 12 | Pasien baru online lalu didaftarkan petugas di loket dengan NIK yang sama | Integ | Status 3 |
| `INV-PO-02`, `R-PO-17` | Dua permintaan bersamaan memperebutkan satu sisa kuota (halaman publik vs petugas) | Integ (paralel) | Tepat satu berhasil; yang lain `409` |
| `INV-PO-02` | Check-in saat kuota appointment tepat penuh oleh pendaftaran itu sendiri | Integ | Berhasil; kuota tidak terhitung dua kali |

### PO-T.4 Layar petugas

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| AC 18 | Petugas dengan `OnlineRegistration : Read` membuka daftar; filter tanggal dan status; cari dengan kode, nomor, nama, No. RM, NIK | Integ + E2E | Hasil sesuai; kode tidak pernah tampil |
| AC 18 | Petugas tanpa hak | Integ + E2E | `403`; menu tidak tampil; `AccessDeniedGate` |
| AC 19 | Ubah dari Rabu ke Kamis dengan dokter lain di poli yang sama | Integ + E2E | Status tetap 1; kuota Rabu +1, Kamis −1; riwayat nilai lama/baru; kode lama tetap berlaku |
| AC 19 | Jadwal baru penuh | Integ | `409`; pendaftaran tidak berubah |
| `KSK-DSN-PO-016` | Jadwal baru di poli lain | Integ | `400` `KSK-VAL-PO-062` |
| AC 19 | Ubah kontak | Integ | Riwayat `CONTACT_CHANGED` dengan nilai lama/baru; email tidak dikirim ulang |
| AC 20 | Batal dengan alasan | Integ + E2E | Status 5; kode ditolak di kiosk; kuota kembali |
| AC 20 | Batal tanpa alasan; batal pendaftaran berstatus akhir | Integ | `400` / `409` |
| `GATE-PO-16` | Batal dengan `expectedRowVersion` lama | Integ | `409` `KSK-VAL-PO-072` |
