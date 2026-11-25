# Laporan Kesiapan — Kiosk

Berkas ini menyimpan hasil audit kesiapan (`verify-module-readiness`) untuk blueprint kiosk. Setiap audit ditulis sebagai bagian sendiri. Audit yang lebih baru tidak menghapus bagian lama.

## Amendment PO — Pendaftaran Pasien Online dan Check-in Kiosk (audit 2026-11-25)

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `KSK-BP-001` |
| Revisi yang diaudit | `2` (`approved`, `KSK-DEC-054`) dan `2.1` (`approved`, `KSK-DEC-058`) |
| Cakupan | Halaman publik Pendaftaran Pasien Online, Check-in Pasien Online di kiosk, layar petugas Daftar Pendaftaran Online. Revisi 1 (Cek No. RM, Pasien Lama, Penjamin Utama, Cek Jadwal Dokter) **tidak** diaudit di sini |
| Kontrak | `KSK-CONTRACT-v2` dan `KSK-CONTRACT-v2.1`, keduanya `approved` |
| Keputusan | `KSK-DEC-026` s.d. `058`. Seluruh open question Amendment PO sudah ditutup, kecuali `KSK-PO-OQ-010`, `015`, `016` (milik IT, untuk go-live) |
| Roadmap | `KSK-PO-RM-BE-001` dan `KSK-PO-RM-FE-001` r1.1; traceability `KSK-PO-TRACE-001` r1.1 |
| Commit backend yang diaudit | `827fdd33` (`sukmagp`), bersih dan sinkron dengan remote |
| Commit frontend yang diaudit | `6cd211e92` (`sukmagpV2`), bersih dan sinkron dengan remote |
| Bentuk blueprint | `SINGLE` — satu verdict |
| Auditor | Claude Opus 5.5 (`claude-opus-5-5`), read-only. Satu-satunya perintah yang dijalankan adalah verifier otorisasi, yang membangun ke folder sementara tanpa mengubah repository |
| Verdict | **`READY_WITH_CONDITIONS`** |

### Ringkasan untuk pembaca umum

Seluruh fitur Amendment PO sudah selesai dibangun dan sudah dicoba di lingkungan uji (dev): pasien dapat mendaftar online, menerima kode, check-in di kiosk, lalu mendapat tiket antrean poli. Petugas dapat melihat, mengubah jadwal, mengubah kontak, dan membatalkan pendaftaran online.

Fitur ini **belum boleh dipakai pasien sungguhan** sampai lima syarat operasional (K1–K5) selesai. Contohnya: tanpa kunci captcha produksi (K1), tombol Kirim Pendaftaran di halaman publik tidak akan pernah aktif, sehingga tidak ada pasien yang bisa mendaftar.

### Kemajuan build dan kesiapan go-live dihitung terpisah

| Ukuran | Hasil |
| --- | --- |
| Task build | 20 dari 20 selesai (backend 11/11, frontend 9/9), masing-masing punya laporan di `task/report/` |
| Syarat operasional go-live | 0 dari 7 sudah tertutup (lihat bagian *Syarat*) |

Build yang lengkap belum berarti siap dipakai. Angka kedua yang menentukan kapan fitur boleh dibuka untuk pasien.

### Skor per dimensi

| Dimensi | Bobot | Skor | Dasar perhitungan dan bukti | Kekurangan |
| --- | ---: | ---: | --- | --- |
| Fondasi (keputusan, kontrak, roadmap) | 15 | 95% | Manifest revisi 2 dan 2.1; roadmap r1.1 dengan grafik dependency cocok dengan kolom `Dependency` (12 pasangan); traceability AC 1–20, `FR-KSK-PO-*`, `KSK-DSN-PO-*` lengkap | Tiga catatan dokumen pada bagian *Catatan dokumen* (dua sudah dirapikan saat audit ini) |
| Backend | 25 | 11/11 | Verifier otorisasi diulang di `827fdd33`: PASS (fallback 68 cocok, endpoint bisnis telanjang baru 0, identitas wajib 24/24). Migration `20261119134155_AddRegOnlineRegistration` ada; model `RegOnlineRegistration*` tidak berubah sesudah migration (satu commit `93f0fa1b`). Build, QBE Strict, dan runtime HTTP per task tercatat di laporan backend | Migration baru diterapkan ke DB dev `QuilvianNewDevSukma` |
| Frontend | 20 | 9/9 | `npm run lint:errors` PASS dan `npm run build` PASS (856/856 halaman) pada isi yang sama dengan `6cd211e92`; uji browser per task tercatat di laporan frontend | — |
| Integrasi dan runtime | 20 | Dev 100%, produksi 0/7 | Dev: kunci uji Turnstile resmi, server SMTP uji, backend uji nyata, job penutupan `OnlineRegistrationClosureHostedService` terdaftar (`Program.cs:1870`) dan terbukti menutup pendaftaran yang terlewat (`BE-KSK-PO-008`) | K1–K7 |
| Verifikasi (baris uji PO-T) | 20 | 49/52 (94%) | 52 baris pada `testing/acceptance-test-matrix.md` PO-T.1–PO-T.4 dipetakan ke laporan task | 3 baris terbukti sebagian (tabel di bawah) |

### Baris uji yang baru terbukti sebagian

| Baris PO-T | Yang sudah terbukti | Yang belum, dan alasannya | Risiko |
| --- | --- | --- | --- |
| PO-T.1 `KSK-DEC-057` — faskes non-mitra tidak pernah muncul | Faskes mitra yang perjanjiannya sudah berakhir dan faskes mitra nonaktif tidak muncul; mitra dengan perjanjian s.d. 5 Des muncul pada 5 Des dan hilang pada 6 Des (`BE-KSK-PO-011`, `FE-KSK-PO-009`) | Faskes dengan `IsPartner = false` tidak bisa dibuat: API master faskes selalu menyimpan `IsPartner = true` (`ReferralMasterDataService` baris 405 dan 503) | Rendah. Saringan yang sama (`ReferralPartnerEligibility.IsEligibleOn`) mensyaratkan `IsPartner`, dan data seperti itu tidak bisa lahir lewat aplikasi |
| PO-T.2 AC 7 — tiket tercetak | Tiket thermal 80 mm dibentuk, dicetak otomatis sekali, dan Cetak Ulang mencetak lagi — di browser (`FE-KSK-PO-005` K20–K22) | Belum dicetak di printer thermal kiosk fisik | Sedang. Bila printer gagal, tiket tetap tampil di layar dan pasien diarahkan ke menu Cetak Antrean (`KSK-DEC-056`) |
| PO-T.2 AC 9 dan AC 17 — pindai KTP/KIA pasien baru | Alur lengkap dengan agen pemindai tiruan yang mengirim data OCR dalam bentuk yang dibaca kiosk (`FE-KSK-PO-006`, 15/15) | Belum dengan pemindai Plustek sungguhan | Sedang. Fungsi pindai dan OCR yang dipakai sama dengan menu Pasien Baru kiosk yang sudah berjalan |

### Syarat sebelum go-live, diurutkan dari yang paling berdampak

| No | Syarat | Dampak nyata bila dibuka sekarang | Pemilik | Cara menutup |
| ---: | --- | --- | --- | --- |
| K1 | `KSK-PO-OQ-016` — kunci Cloudflare Turnstile produksi | `appsettings.json` berisi `Turnstile:SiteKey` dan `SecretKey` kosong dengan `FailOpen: false`. Pencocokan pasien lama dan Kirim Pendaftaran ditolak, sehingga **tidak ada pasien yang bisa mendaftar**. Di halaman, tanpa site key tombol yang memerlukan captcha tetap nonaktif | IT | Isi kedua kunci lewat penyimpanan rahasia server (bukan di repository). Isi juga `NEXT_PUBLIC_TURNSTILE_SITE_KEY` **sebelum** `next build`, karena nilai ini ditanam ke halaman saat build. Lalu uji satu pendaftaran dari domain produksi |
| K2 | `KSK-PO-OQ-010` — domain publik dan proxy | `Program.cs` baris 2354–2364 menerima header `X-Forwarded-For` dari sumber mana pun (`KnownNetworks` dan `KnownProxies` dikosongkan; kode lama `c63cf2de`, bukan bagian Amendment PO). Bila API dapat diakses langsung dari internet, penyerang bisa mengarang alamat IP di header itu. Contoh: batas 10 kiriman per jam per IP dilewati dengan mengganti nilai header setiap kiriman | IT | Pastikan API hanya dapat dicapai lewat proxy yang **menimpa** `X-Forwarded-For` dengan IP asli pengunjung, atau isi `KnownProxies`/`KnownNetworks` dengan alamat proxy. Halaman publik dan API harus terjangkau dari internet |
| K3 | Migration `AddRegOnlineRegistration` di DB produksi | Tiga tabel `RegOnlineRegistration*` tidak ada, sehingga seluruh endpoint Amendment PO gagal | Pemilik (izin deploy terpisah) | Terapkan saat deploy, lalu pastikan `dotnet ef migrations list` menunjukkan 0 Pending |
| K4 | Centang Akses Role | Petugas tidak melihat menu Pendaftaran Online dan mendapat `403` | Admin aplikasi | Di Pengaturan → Manajemen Role → Akses Role, centang `OnlineRegistration` `Read`, `Update`, `Cancel` untuk peran petugas pendaftaran. Akun kiosk tetap memakai hak kiosk yang sudah ada |
| K5 | Uji di kiosk fisik | Pemindai dan printer belum pernah dicoba sungguhan (tiga baris di atas) | Pemilik bersama implementer | Satu check-in pasien lama (cetak tiket dan Cetak Ulang) dan satu check-in pasien baru (pindai KTP) di kiosk fisik |
| K6 | `KSK-PO-OQ-015` — akun SMTP rumah sakit | Tidak menghalangi. `Email:Smtp:Enabled` masih `false`, sehingga email kode tidak terkirim. Kode tetap tampil di layar Selesai, dan tombol WhatsApp tetap ada (`KSK-DEC-042`) | IT | Isi `Email:Smtp` lewat penyimpanan rahasia, set `Enabled: true`, lalu kirim satu pendaftaran uji ke alamat email rumah sakit |
| K7 | Data uji di DB dev | Tidak menghalangi produksi; hanya mengotori DB dev | Pemilik | Batalkan pendaftaran dan nonaktifkan jadwal/faskes uji lewat API. Penghapusan permanen memerlukan izin eksekusi DB terpisah. Daftar data uji ada di bagian *Catatan penutup* setiap laporan task |

### Pengecualian yang sudah tercatat dan diterima

| Hal | Laporan | Alasan diterima |
| --- | --- | --- |
| `dotnet test` tidak dijalankan | Semua laporan backend | Repository tidak memakai project test. Bukti diganti uji runtime HTTP dan uji browser |
| `403` untuk akun bukan kiosk pada verifikasi check-in | `BE-KSK-PO-006` | Policy `KioskRead` yang sama sudah terbukti di `BE-KSK-002` |
| `KSK-VAL-PO-065` varian kunjungan aktif | `BE-KSK-PO-010` | Memakai helper yang sama dengan `KSK-VAL-PO-024`, yang terbukti di `BE-KSK-PO-005` |
| Penolakan `KSK-VAL-PO-073` di halaman diuji dengan respons tiruan | `FE-KSK-PO-009` | Halaman tidak dapat memilih faskes non-mitra lewat jalur normal; `073` asli dari backend terbukti di `BE-KSK-PO-011` |

### Catatan dokumen

| No | Catatan | Tindak lanjut |
| ---: | --- | --- |
| 1 | Traceability §3: baris `KSK-DSN-PO-001`–`003` dan `014`, `005` dan `013`, `006` dan `007` belum memberi tanda ✅ pada `BE-KSK-PO-001`, `004`, `003` | Dirapikan bersama audit ini (tanda dan tanggal dari kartu roadmap) |
| 2 | Manifest belum mencatat hasil audit kesiapan untuk revisi 2/2.1 | Dirapikan bersama audit ini (baris *Verifikasi kesiapan* pada blok Revisi 2.1) |
| 3 | Blok YAML revisi 1 manifest masih `last_verified_at: null` dan `tahap_belum_dijalankan: build_*`; `01-existing-capability-map.md` r1 basi untuk area Amendment PO | Dibiarkan. Blok YAML milik revisi 1, yang tidak diaudit di sini. Peta kemampuan PO sudah digantikan `01-capability-impact-scan-pendaftaran-online.md` |

### Temuan di luar modul

- `OnlineRegistrationOptions.PublicPageBaseUrl` (`appsettings.json` masih menunjuk domain dev) tidak dibaca kode mana pun. Tidak berdampak, tetapi sebaiknya dihapus atau diisi domain produksi agar tidak menyesatkan.
- Kunjungan yang dibatalkan lewat `PATCH patient-encounters/{id}/cancel` (respons `200`) masih tampil berstatus "Menunggu Perawat". Ini milik modul kunjungan, bukan kiosk, dan perlu dicek terpisah.

### Verdict

**`READY_WITH_CONDITIONS`**

Seluruh kemampuan Amendment PO sudah terbukti di lingkungan dev: 20/20 task, 49/52 baris uji penuh, dan 3 baris sisanya terbukti sebagian dengan risiko terbatas. Fitur boleh dibuka untuk pasien sesudah K1–K5 ditutup. K6 dan K7 dianjurkan, tetapi tidak menghalangi.

### Langkah berikutnya

1. Serahkan K1, K2, dan K6 ke IT. Dari sisi keamanan, K2 paling penting.
2. Jadwalkan uji kiosk fisik (K5).
3. Saat deploy: terapkan migration (K3), lalu centang Akses Role (K4).
4. Jalankan `verify-module-readiness` ulang sesudah K1–K5 tertutup, untuk mengganti verdict menjadi `READY`.
