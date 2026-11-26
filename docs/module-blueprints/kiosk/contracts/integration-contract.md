# Kiosk — Kontrak Integrasi

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v1` |
| `last_changed_in` | `v1` |
| Status | `approved` |
| Owner | Sukma Giri Pratama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-09-30 |
| `input_revision` | `00-interview-decisions.md` r2; `01-existing-capability-map.md` r1 |
| Traceability | `KSK-DEC-002/013/014`, `KSK-OQ-004/005`, `RJ-BIL-DEC-015`, `RWI-ENC-PAYER-001` |

Kiosk **tidak memanggil sistem luar** (tidak ada Dukcapil, BPJS, OTP, atau pihak ketiga — PRD §46). Kontrak ini hanya mengatur titik sentuh **antar-modul internal**.

## 1. Arah baca dan tulis

| Modul lain | Arah | Apa | Lewat | Sinkron? | Kalau modul lain gagal |
| --- | --- | --- | --- | --- | --- |
| PatientManagement (`Pat`/`Mst`) | Kiosk **membaca** | Pasien, dokumen identitas, kartu asuransi, member, relasi perusahaan | `KioskPatientLookupService` (query baca-saja); endpoint `patients/kiosk/{id}`, `*-kiosk/options` existing | Ya | Lookup menjawab `500` → layar `ERROR` + Coba Lagi. Tidak ada data yang ditulis. |
| RegistrationManagement — sesi kiosk | Kiosk **menulis** | Satu sesi per perjalanan Pasien Lama, dibentuk di Step 3 | `POST kiosk-scan-sessions/kiosk/scan-result` (existing) | Ya | Tetap di Step 3 + `KSK-VAL-012`; tidak lanjut tanpa sesi |
| RegistrationManagement — kunjungan | Kiosk **menulis** | Kunjungan + satu sumber pembayaran | `POST patient-encounters/kiosk` (existing, aturan dilonggarkan) | Ya | Tetap di Konfirmasi + pesan; transaksi existing menjamin tidak ada yang tersimpan separuh |
| Laboratorium | Laboratorium **membaca** sesi kiosk | Sesi dengan `targetService = 2` | Panel lab existing | — | Di luar Kiosk |
| `KioskEncounterClosureService` | Membaca kunjungan + sesi | Menutup kunjungan kiosk yang tidak dilanjutkan | Hosted service existing | Terjadwal | Tidak berubah; sesi poliklinik tetap tanpa target (`KSK-DSN-007`) |
| Billing-kasir | Billing **membaca** sumber pembayaran kunjungan | Penanggung (`RegPatientEncounterGuarantor`) | Existing | — | Tidak berubah. Kunjungan Penjamin Perusahaan dari Kiosk diperlakukan sama dengan dari route admin (`RWI-ENC-PAYER-001`) |

## 2. Idempotensi, timeout, retry

| Titik | Aturan |
| --- | --- |
| Lookup | Aman diulang (tidak menulis). Satu request aktif per layar; request lama dibatalkan (`AbortController`) bila pasien mengubah isian. Timeout layar 20 detik → `ERROR`. |
| Pembentukan sesi di Step 3 | Satu kali per perjalanan. Tombol terkunci selama request. Bila jawaban hilang (timeout), layar **tidak** otomatis mengulang; pasien diberi Coba Lagi. Satu kali ulang yang berhasil setelah timeout dapat menghasilkan sesi kedua — risiko ini sama dengan perilaku hari ini dan dicatat sebagai risiko sisa (`KSK-RISK-001`). |
| Create encounter | Mengikuti `RegistrationIdempotencyKey` existing bila dipakai layar; tidak berubah oleh revisi ini. |

## 3. Dependency lintas blueprint

| ID | Blueprint lain | Isi | Memblokir |
| --- | --- | --- | --- |
| `KSK-OQ-004` | `laboratorium` | Amendment `FE-LAB-13` / `AC-93`: Tujuan Layanan pindah ke setelah Review; sesi dibentuk di Step 3 | Task FE urutan step |
| `KSK-OQ-005` | `rawat-inap` (`episode-rawat-inap/contracts/encounter-company-guarantor-contract.md`) | Amendment `RWI-ENC-PAYER-001` v1.0.0 → v1.1.0: route kiosk menerima `CompanyGuarantor` (disetujui Muhammad Hamzah, `KSK-DEC-013`) | **Ditutup 30 Sep 2026** — v1.1.0 tercatat (§7, §11) |


---

## Amendment PO — Integrasi Pendaftaran Pasien Online

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v2` |
| `last_changed_in` | `v2` (bagian ini saja) |
| Status | `approved` |
| Owner | Sukma Giri Pratama; akun SMTP dan Turnstile: IT/infrastruktur |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-11-19 (`KSK-DEC-054`) |
| `input_revision` | `00-interview-decisions.md` §13; impact scan `CAP-PO-03`, `04`, `05`, `06`, `27` |
| Traceability | `KSK-DEC-038`, `042`, `044`; `KSK-PO-OQ-010`, `015`, `016` |

### PO-I.1 Integrasi eksternal

| ID | Sistem | Arah | Kapan | Sinkron/asinkron | Timeout dan retry | Bila gagal |
| --- | --- | --- | --- | --- | --- | --- |
| `INT-PO-01` | Cloudflare Turnstile `POST https://challenges.cloudflare.com/turnstile/v0/siteverify` (`secret`, `response`, `remoteip`) | Backend → Cloudflare | Pencocokan pasien dan pengiriman pendaftaran | Sinkron | 5 detik, tanpa retry | Ditolak `503` (`KSK-VAL-PO-011`). Tidak ada data tersimpan |
| `INT-PO-02` | Widget Turnstile `https://challenges.cloudflare.com/turnstile/v0/api.js` | Browser pasien → Cloudflare | Halaman online dimuat | — | — | Widget kosong; tombol Kirim tidak aktif; pesan "Verifikasi keamanan belum siap, muat ulang halaman" |
| `INT-PO-03` | Server SMTP rumah sakit | Backend → SMTP | Sesudah pendaftaran tersimpan, bila email diisi | Sinkron sesudah transaksi | 10 detik, satu percobaan | `EmailDeliveryStatus = Failed`; respons tetap `200` dengan kode di layar; Lupa Kode tersedia (`KSK-DEC-042`) |
| `INT-PO-04` | WhatsApp (`https://wa.me/62…?text=…`) | Browser pasien → WhatsApp | Pasien menekan tombol | — | — | Tidak ada; pasien menyalin kode manual. Memakai `buildWhatsAppUrl` yang ada (`CAP-PO-06`) |

Isi email dan pesan WhatsApp (`KSK-PO-FE-003`): nomor pendaftaran, kode, tanggal kunjungan, poli, dokter, jam praktik, dan petunjuk singkat check-in di kiosk. **Tanpa** NIK, No. RM, tanggal lahir, maupun data penjamin. Kalimat persisnya `DEV_DISCRETION`.

### PO-I.2 Arah baca dan tulis antar modul

| Modul | Dibaca | Ditulis | Lewat |
| --- | --- | --- | --- |
| Master Data | Poli, jadwal dokter, faskes perujuk | — | Query di `OnlineRegistrationPublicService` (field minimum) |
| Administrator Master Data | Penyedia asuransi | — | Query di `OnlineRegistrationPublicService` |
| Patient Management | Pasien, kartu asuransi, penjamin perusahaan | Master pasien, dokumen identitas, kartu asuransi baru saat check-in | Baca: query. Tulis: endpoint kiosk yang sudah ada (`POST patients/kiosk`, `patient-identity-documents/kiosk`, `patient-insurances/kiosk`) dipanggil frontend kiosk |
| Registration (kunjungan) | Kunjungan aktif, pemakaian kuota | Kunjungan, consent, dokumen rujukan, penutupan pendaftaran | Hook di `PatientEncounterController.CreateEncounterCoreAsync` (pola Reservasi Prioritas) |

### PO-I.3 Idempotensi dan tabrakan

| Kejadian | Perlindungan |
| --- | --- |
| Pasien menekan Kirim dua kali | Tombol terkunci; di server, kunci pasien + `INV-PO-03` menolak pendaftaran kedua `409` |
| Dua pasien berebut slot terakhir | Kunci kuota per jadwal dan tanggal (`KSK-DSN-PO-009`) |
| Kiosk mengirim pembuatan kunjungan dua kali | Kunci pasien `REG_ENCOUNTER_ACTIVE_*` dan aturan kunjungan aktif menolak yang kedua; token sudah dihapus setelah check-in |
| Penutupan harian berjalan dua kali atau tertinggal | Hanya menyentuh Menunggu Check-in dengan tanggal sebelum hari ini |
| Petugas mengubah atau membatalkan saat pasien check-in | `RowVersion` dan pemeriksaan status di dalam transaksi |
| Berkas sudah ditulis tetapi transaksi pendaftaran gagal | Berkas dihapus kembali di blok kompensasi |

### PO-I.4 Prasyarat infrastruktur (blocker go-live)

| ID | Kebutuhan | Owner |
| --- | --- | --- |
| `KSK-PO-OQ-010` | Halaman dan API dapat dibuka dari internet; proxy meneruskan IP asli (`X-Forwarded-For`) ke backend, dengan `KnownProxies`/`KnownNetworks` diisi sesuai proxy rumah sakit | IT/infrastruktur |
| `KSK-PO-OQ-015` | Akun SMTP (host, port, TLS, pengguna, sandi, alamat pengirim) dan izin koneksi keluar ke port SMTP | IT/infrastruktur |
| `KSK-PO-OQ-016` | Site key dan secret Turnstile untuk domain halaman online, dan izin koneksi keluar HTTPS ke `challenges.cloudflare.com` | IT/infrastruktur |
| — | CSP frontend: tambahkan `https://challenges.cloudflare.com` pada `script-src` dan `frame-src` (`CAP-PO-29`) | Frontend |

### PO-I.5 Dependency lintas blueprint

| Blueprint | Titik sentuh | Tindak lanjut |
| --- | --- | --- |
| `rawat-jalan` | Hitungan kuota (`DoctorScheduleQuotaService`), kunci kuota baru, dan penutupan pendaftaran online di `CreateEncounterCoreAsync` mengubah perilaku jalur petugas | Catat titik sentuh di decision log `rawat-jalan` saat blueprint ini disetujui (`GATE-PO-06`). Aturan RJ tidak berubah |
| `rawat-jalan` Amendment RP | Kunjungan yang melengkapi Reservasi Prioritas juga menutup pendaftaran online pasien itu pada tanggal yang sama | Tidak ada perubahan pada Reservasi Prioritas |
