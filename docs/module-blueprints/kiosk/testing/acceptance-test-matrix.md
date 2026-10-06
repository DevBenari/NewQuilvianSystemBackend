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
