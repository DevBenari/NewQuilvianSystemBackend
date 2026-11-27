# Kiosk — Penilaian Kelengkapan Requirement: Pendaftaran Pasien Online dan Check-in Kiosk (Amendment PO)

| Field | Nilai |
| --- | --- |
| Assessment ID | `KSK-RCG-002` |
| Revision | `2` (penilaian ulang 19 Nov 2026, §9). Revisi 1 di §1–§8 dipertahankan sebagai riwayat |
| Blueprint ID | `KSK-BP-001`, Amendment PO, bentuk `SINGLE` (`KSK-DEC-028`) |
| Status | `draft` |
| **Kesiapan** | **`READY_FOR_DOMAIN_DESIGN`** untuk 8 dari 8 slice (revisi 2, §9.5). Revisi 1 menghasilkan `PARTIALLY_READY`; kedua blocker-nya ditutup `KSK-DEC-047`, `048`, dan `053` |
| Keputusan yang dinilai | Revisi 1: `KSK-DEC-026` s.d. `046`, `KSK-PO-FE-001` s.d. `004`, `KSK-PO-AS-001`, `KSK-PO-OQ-001` s.d. `018`. Revisi 2 menambah `KSK-DEC-047` s.d. `053`, `KSK-PO-FE-005`, `KSK-PO-AS-002`, `003`, `KSK-PO-OQ-019` s.d. `022`, dan AC 16–20 ([00-interview-decisions.md](00-interview-decisions.md) bagian 13) |
| Bukti source | [01-capability-impact-scan-pendaftaran-online.md](01-capability-impact-scan-pendaftaran-online.md) `CAP-PO-01` s.d. `29`, ditambah addendum §8 `CAP-PO-30` s.d. `37`, serta pemeriksaan read-only pada §2 |
| SHA backend | `5e777833` (`sukmagp`) |
| SHA frontend | `5f18e9f04` (`sukmagpV2`) |
| Tanggal penilaian | 19 November 2026 |
| `indonesia-hospital-domain-reference` | Tidak dipakai. Semua butir dinilai dari keputusan pemilik dan bukti source |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` direkomendasikan (alasan di §8) |
| Task mode | `MODULE BLUEPRINT MODE`, read-only terhadap source aplikasi |

> **Cara membaca dokumen ini.** Dokumen ini hanya menjawab satu pertanyaan: *apakah requirement Amendment PO sudah cukup lengkap untuk mulai dirancang?* Dokumen ini tidak merancang tabel, endpoint, atau layar, dan tidak menjawab keputusan bisnis atas nama pemilik.
>
> | Status bukti | Artinya |
> | --- | --- |
> | `CONFIRMED` | Didukung keputusan pemilik, atau didukung source untuk kondisi yang sudah ada |
> | `PROPOSED` | Usulan wajar yang **belum** dikonfirmasi pemilik |
> | `MISSING` | Tidak ada dalam bukti |
> | `CONFLICT` | Dua sumber bertentangan tanpa penyelesaian |
>
> | Dampak gap | Artinya |
> | --- | --- |
> | `BLOCKING` | Desain slice terdampak harus berhenti |
> | `NON_BLOCKING_STANDARD` | Usulan standar boleh dipakai asal tetap terlihat |
> | `CONFIGURABLE_DEFAULT` | Wajar berbeda antar rumah sakit, aman dijadikan pengaturan |

---

## 1. Scope penilaian

Yang dinilai adalah perjalanan pasien dari mengisi halaman Pendaftaran Pasien Online di HP sampai menerima tiket antrean di kiosk (`KSK-DEC-026`, batas scope §13.2). Penilaian dipecah menjadi tujuh slice, supaya satu gap tidak menahan semuanya.

| Slice | Isi | Keputusan utama |
| --- | --- | --- |
| `S-PO-1` | Pengenalan pasien di halaman publik: pasien lama (NIK/No. RM + tanggal lahir, nama tersamar), formulir data diri pasien baru, dan kolom No. WA/email | `KSK-DEC-031`, `032`, `039`, `043`, `KSK-PO-FE-001`, `002` |
| `S-PO-2` | Langkah pendaftaran sesuai menu RJ: jenis pasien, penjamin, data kunjungan (tanggal, jadwal, kuota), rujukan beserta unggah surat, general consent, verifikasi, lalu tersimpan sebagai Menunggu Check-in | `KSK-DEC-029`, `030`, `034`, `035`, `038` butir 1–2 |
| `S-PO-3` | Kode pendaftaran online dan pengirimannya: tampil di layar, email otomatis, tombol WhatsApp | `KSK-DEC-033`, `041`, `042`, `043`, `KSK-PO-FE-003` |
| `S-PO-4` | Perlindungan halaman publik: batas laju per IP dan captcha Turnstile | `KSK-DEC-038` butir 3–4, `044` |
| `S-PO-5` | Check-in pasien lama di kiosk: kode + tanggal lahir, Lupa Kode, pemeriksaan ulang jadwal, kunjungan, nomor antrean, dan tiket | `KSK-DEC-033`, `036`, `040`, `046` |
| `S-PO-6` | Check-in pasien baru di kiosk: pindai kartu identitas, pencocokan dengan data online, pembuatan master pasien dan No. RM atau pemakaian No. RM lama, serta pelekatan consent | `KSK-DEC-032`, `034`, `039` |
| `S-PO-7` | Penutupan otomatis: Tidak Datang setelah 23.59 WIB dan Didaftarkan lewat Jalur Lain, beserta pelepasan kuota | `KSK-DEC-037`, `045` |

## 2. Bukti yang dipakai

| Urutan wewenang | Sumber | Dipakai untuk | Status |
| --- | --- | --- | --- |
| 1, requirement eksplisit | Permintaan Sukma Giri, 19 Nov 2026 (kutipan di §13 decision log) | Apa yang harus dibangun | `CONFIRMED` |
| 1, requirement eksplisit | Keputusan `KSK-DEC-026` s.d. `046` (Sukma Giri, pemilik kiosk dan RJ) | Aturan bisnis Amendment PO | `CONFIRMED` |
| 3, keputusan tercatat | Blueprint `rawat-jalan`: `RJ-DOC-DEC-034` s.d. `039`, Amendment RP (`RJ-PRI-DEC-009`, `010`, `012`, `013`, `021`, `027`, `028`) | Aturan RJ yang dirujuk `KSK-DEC-026` syarat 2, dan pola halaman publik | `CONFIRMED` |
| 6, implementasi V2 | Impact scan `CAP-PO-01` s.d. `29` pada SHA di atas | Apa yang sudah ada hari ini | `CONFIRMED` sebagai bukti as-is |
| 6, implementasi V2 | Pemeriksaan tambahan gerbang ini, lihat tabel di bawah | Dua fakta yang belum tercakup impact scan | `CONFIRMED` sebagai bukti as-is |

Pemeriksaan tambahan pada SHA frontend `5f18e9f04`:

| ID | Fakta | Bukti |
| --- | --- | --- |
| `F-GATE-PO-1` | Menu Pendaftaran Pasien RJ menentukan jenis kuota dari tanggal kunjungan. Tanggal mendatang memakai kuota appointment, hari ini memakai kuota walk-in. Ini wujud `RJ-DOC-DEC-037` ("Hari ini = walk-in; tanggal mendatang = appointment") | `src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-schedule-quotas.js:37-44` (`isAppointmentDate(visitDate)`) |
| `F-GATE-PO-2` | Kiosk Pasien Baru menerima tiga kartu identitas: KTP, KIA (Kartu Identitas Anak), dan SIM. Tombol lanjut tetap nonaktif sampai kartu dipindai. Tidak ada jalur isi manual tanpa pindai | `src/lib/helpers/kiosk/registration/kiosk-new-patient-registration.helpers.jsx:46-147`; `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-view.jsx:537`, `:2515` |

## 3. Temuan 18 dimensi kelengkapan

| No | Dimensi | Temuan | Klasifikasi | Dampak |
| ---: | --- | --- | --- | --- |
| 01 | Tujuan | Pasien mendaftar RJ sendiri tanpa login, menerima kode, lalu check-in di kiosk saat datang tanpa antre di loket | `CONFIRMED` (§13.2, `KSK-DEC-026`) | — |
| 02 | Aktor | Pasien atau wali di halaman publik (tanpa login), pasien di kiosk (akun perangkat kiosk), sistem terjadwal, dan petugas loket sebagai jalur cadangan | `CONFIRMED` (`KSK-DEC-030`, `037`, `046`) | — |
| 03 | Pemicu / Prasyarat | Jadwal dokter mengizinkan appointment dan kuota appointment masih ada. Kartu Check-in hanya aktif di perangkat kiosk yang diizinkan menerima appointment (`enabledType: "appointment"` di beranda kiosk). **Belum jelas:** apakah pendaftaran online boleh untuk hari ini | `CONFIRMED`, kecuali hari ini: `CONFLICT` | `KSK-PO-OQ-019`, `BLOCKING` untuk `S-PO-2` |
| 04 | Alur Utama | Isi halaman online, tersimpan Menunggu Check-in, kode tampil dan terkirim, pasien check-in di kiosk pada tanggal kunjungan, kunjungan dan tiket terbit | `CONFIRMED` (§13.6, §13.7) | — |
| 05 | Alur Alternatif / Exception | Empat belas skenario sudah tercatat di §13.7. **Belum ada:** (a) kartu identitas yang dipindai saat check-in pasien baru tidak cocok dengan data online, atau pasien tidak membawa kartu identitas; (b) data yang diisi online (consent, rujukan, kartu asuransi baru) ketika pasien akhirnya didaftarkan di loket | (a) `MISSING`; (b) `PROPOSED` | (a) `KSK-PO-OQ-020`, `BLOCKING` untuk `S-PO-6`; (b) `GATE-PO-01`, `NON_BLOCKING_STANDARD` |
| 06 | Data Minimum | Pasien atau data diri pasien baru, kontak WA/email, tanggal kunjungan, jadwal dokter, jenis pasien, penjamin, rujukan dan berkas, consent, hash kode, status, riwayat. Isian data diri pasien baru meniru kiosk Pasien Baru (`CAP-PO-23`). **Belum ada:** masa simpan data diri, tanda tangan, dan berkas dari pendaftaran yang berakhir Tidak Datang | Sebagian `PROPOSED` | `GATE-PO-02`, `GATE-PO-03`, `NON_BLOCKING_STANDARD` |
| 07 | Aturan Bisnis / Validation | Satu pendaftaran terbuka per pasien per tanggal, ditolak bila ada kunjungan aktif, maksimal 30 hari ke depan, batas laju, captcha, wajib salah satu kontak, berkas maksimal 5 MB | `CONFIRMED` (`KSK-DEC-035`, `038`, `043`). Angka batas laju `CONFIGURABLE_DEFAULT`. Cara menerapkan "satu per pasien" pada pasien baru yang belum punya No. RM `PROPOSED` | `GATE-PO-04`, `NON_BLOCKING_STANDARD` |
| 08 | Status / Perubahan Status | Empat status (Menunggu Check-in, Menjadi Kunjungan, Didaftarkan lewat Jalur Lain, Tidak Datang) dan tiga transisi. Penolakan check-in tidak mengubah status | `CONFIRMED` (§13.6). Nasib pendaftaran bila kunjungan hasil check-in kemudian dibatalkan petugas: `PROPOSED` | `GATE-PO-05`, `NON_BLOCKING_STANDARD` |
| 09 | Peran / Authorization | Halaman online tanpa login. Check-in hanya akun perangkat kiosk (`KioskRead`, `CAP-PO-28`). Tidak ada layar petugas untuk pendaftaran online (`KSK-DEC-027`) | `CONFIRMED` | — |
| 10 | Dependency Antarmodul | Registration (wadah, kunjungan, kuota), Patient Management (master pasien, No. RM, kartu asuransi), Master Data (poli, jadwal), aturan RJ (`KSK-DEC-026` syarat 2) | `CONFIRMED` (`CAP-PO-09`, `10`, `13`, `15`, `16`, `19`) | Perubahan hitungan kuota dan jalur pembuatan kunjungan ikut mengubah perilaku menu petugas. Lihat `GATE-PO-06` |
| 11 | Integrasi Internal / Eksternal | SMTP rumah sakit, Cloudflare Turnstile, tautan `wa.me`, akses internet ke halaman dan API | Keputusan `CONFIRMED` (`KSK-DEC-042`, `044`). Akun SMTP, kunci Turnstile, akses internet, dan IP asli `MISSING` | Blocker `IMPLEMENTATION` (go-live): `KSK-PO-OQ-010`, `015`, `016`. Perilaku saat Cloudflare tidak terjangkau: `GATE-PO-07`, `CONFIGURABLE_DEFAULT` |
| 12 | Hasil Akhir | Pendaftaran berakhir sebagai Menjadi Kunjungan (dengan tiket), Didaftarkan lewat Jalur Lain, atau Tidak Datang | `CONFIRMED` | — |
| 13 | Pembatalan / Koreksi | Pasien tidak dapat membatalkan atau mengubah pendaftaran (`KSK-PO-AS-001`). Petugas juga tidak punya layar untuk membatalkan | `PROPOSED` (asumsi, bukan keputusan eksplisit) | `GATE-PO-08`, `NON_BLOCKING_STANDARD`. Risiko kuota tertahan dicatat |
| 14 | Audit / Histori | Riwayat status tidak dihapus (`KSK-DEC-037`). Pelaku, waktu, perangkat kiosk saat check-in, dan sumber penutupan otomatis belum dirinci | `PROPOSED` | `GATE-PO-09`, `NON_BLOCKING_STANDARD` (pola riwayat Reservasi Prioritas) |
| 15 | Notifikasi | Email berisi kode sesudah pendaftaran tersimpan dan tombol WhatsApp yang dikirim pasien sendiri. Pengingat H-1 dan kabar Tidak Datang tidak diminta | `CONFIRMED` (`KSK-DEC-042`). Pengiriman ulang email otomatis bila gagal: `PROPOSED` | `GATE-PO-10`, `NON_BLOCKING_STANDARD` |
| 16 | Dampak Billing / Charge | Pendaftaran online tidak membuat tagihan. Tagihan muncul dari kunjungan seperti RJ biasa (§13.2, `KSK-DEC-030`) | `CONFIRMED` | — |
| 17 | Dampak Keselamatan Klinis | Identifikasi pasien: pasien lama dicocokkan dengan dua penanda (No. RM/NIK + tanggal lahir). Pasien baru diverifikasi lewat pindai kartu identitas saat check-in (`KSK-DEC-032`). **Belum jelas:** apa yang dicocokkan dan apa yang terjadi bila tidak cocok. Consent yang ditandatangani online sebelum kunjungan ada sudah diputuskan (`KSK-DEC-034`) | Sebagian `MISSING` | `KSK-PO-OQ-020`, `BLOCKING` untuk `S-PO-6` |
| 18 | Pelaporan / Traceability | Kunjungan hasil check-in tertaut ke pendaftaran online asalnya. Laporan rekap pendaftaran online tidak diminta (`KSK-DEC-027`) | `PROPOSED` | `NON_BLOCKING_STANDARD`. Tautan kunjungan cukup untuk penelusuran |

## 4. Gap material dan Decision Log

### 4.1 Gap pemblokir

| ID | Pertanyaan | Slice | Bukti saat ini | Klasifikasi | Dampak | Owner | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `KSK-PO-OQ-019` | Bolehkah pasien mendaftar online untuk **hari ini**? Bila boleh, kuota mana yang dipakai: appointment atau walk-in? | `S-PO-2`, berdampak ke `S-PO-7` | `KSK-DEC-030`: pendaftaran online memakai kuota appointment. `KSK-DEC-038`: paling jauh 30 hari ke depan, tanpa batas paling dekat. `RJ-DOC-DEC-037` dan `F-GATE-PO-1`: hari ini = walk-in. `KSK-DEC-026` syarat 2: aturan RJ diikuti | `CONFLICT` | `BLOCKING`. Menentukan aturan tanggal, kuota mana yang berkurang, kuota mana yang dilepas saat penutupan, dan jenis kunjungan yang terbit saat check-in | Sukma Giri | `OPEN` |
| `KSK-PO-OQ-020` | Saat check-in pasien baru, data apa yang harus sama antara kartu identitas yang dipindai dan data online? Apa yang terjadi bila tidak sama atau pasien tidak membawa kartu identitas? Data master pasien diambil dari hasil pindai atau dari isian online? | `S-PO-6` | `KSK-DEC-032`: kiosk memindai KTP dan mencocokkannya sebelum master dibuat, dan data palsu dari internet tidak boleh masuk master. `KSK-DEC-039`: NIK yang sudah terdaftar memakai No. RM lama. `F-GATE-PO-2`: kiosk menerima KTP, KIA, dan SIM, dan pindai wajib. Aturan saat tidak cocok belum ada | `MISSING` | `BLOCKING`. Menyangkut identifikasi pasien, integritas master pasien, dan perpindahan status | Sukma Giri | `OPEN` |

**Contoh dampak `KSK-PO-OQ-019`.** Pukul 06.30 Pak Andi mendaftar online untuk poli Penyakit Dalam hari ini. Menurut `KSK-DEC-030` kuota appointment dr. Hadi berkurang satu. Menurut aturan RJ, pasien hari ini memakai kuota walk-in. Bila desain memilih salah satu tanpa keputusan, dua akibat bisa terjadi. Pertama, kuota appointment hari ini dapat habis oleh pendaftar pagi, padahal kuota itu disiapkan untuk pasien yang mendaftar sejak hari sebelumnya. Kedua, jika memakai walk-in, pendaftar online hari ini bersaing dengan pasien yang sudah antre di kiosk.

**Contoh dampak `KSK-PO-OQ-020`.** Ibu Wati mendaftar online sebagai pasien baru dengan NIK `3174…0011`. Saat check-in ia memindai KTP suaminya yang terbawa, dengan NIK `3174…0022`. Tanpa aturan, kiosk bisa membuat No. RM atas nama suaminya dengan consent yang ditandatangani Ibu Wati. Akibatnya, rekam medis dan tagihan tercatat atas nama orang yang salah.

### 4.2 Gap non-blocking dan usulan standar

Butir di bawah tidak menahan desain. Usulannya dipakai saat desain dan tetap terlihat sebagai usulan, bukan kebijakan yang dikonfirmasi.

| ID | Gap | Slice | Usulan | Klasifikasi | Dampak |
| --- | --- | --- | --- | --- | --- |
| `GATE-PO-01` | Pasien yang diarahkan ke loket (`KSK-DEC-046`) atau mendaftar lewat jalur lain (`KSK-DEC-045`) sudah mengisi consent, rujukan, dan kartu asuransi baru secara online | `S-PO-7` | Data online **tidak** dipindah ke kunjungan buatan petugas. Petugas mengisi seperti biasa di menu RJ. Pendaftaran online hanya ditautkan dan ditutup. Dasarnya `KSK-DEC-027`: hanya yang diminta | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-02` | Formulir data diri pasien baru di halaman online | `S-PO-1` | Isian dan aturan wajibnya sama dengan kiosk Pasien Baru, termasuk NIK 16 digit. Pasien tanpa NIK (misalnya WNA) diarahkan ke loket, karena kiosk hanya menerima KTP, KIA, dan SIM (`F-GATE-PO-2`) | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-03` | Masa simpan data diri, tanda tangan consent, dan berkas rujukan dari pendaftaran yang berakhir Tidak Datang atau Didaftarkan lewat Jalur Lain | `S-PO-7` | Disimpan seperti catatan pendaftaran lain. Kebijakan penghapusan menyusul bila RS menetapkan aturan retensi data pribadi. Pemilik keamanan/privasi di manifest masih `BELUM DITUNJUK` | `PROPOSED` | `NON_BLOCKING_STANDARD`. Penghapusan kelak cukup ditambah sebagai proses terjadwal |
| `GATE-PO-04` | "Satu pendaftaran terbuka per pasien per tanggal" untuk pasien baru yang belum punya No. RM | `S-PO-2`, `S-PO-7` | Pasien baru dikenali dari NIK yang diisi. Penutupan Jalur Lain (`KSK-DEC-045`) juga mencari pendaftaran pasien baru lewat NIK pasien kunjungan. Penolakan tidak menyebut bahwa NIK sudah terdaftar di master, sejalan dengan `KSK-DEC-039` | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-05` | Kunjungan hasil check-in kemudian dibatalkan petugas | `S-PO-5` | Pendaftaran online tetap Menjadi Kunjungan. Pembatalan kunjungan dan kuotanya mengikuti aturan RJ | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-06` | Hitungan kuota (`CAP-PO-10`) dan jalur pembuatan kunjungan (`CAP-PO-15`, `16`) dipakai bersama menu petugas, kiosk, dan IGD | `S-PO-2`, `S-PO-7` | Perubahan dicatat sebagai titik sentuh di blueprint `rawat-jalan` saat desain. Menu petugas akan melihat sisa kuota appointment yang sudah dikurangi pendaftaran online, sesuai `KSK-DEC-030` | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-07` | Cloudflare tidak terjangkau saat verifikasi captcha | `S-PO-4` | Bawaan menolak (fail-closed) dengan pesan "Layanan sedang tidak tersedia, silakan coba lagi atau daftar di loket". Dapat diubah lewat konfigurasi | `PROPOSED` | `CONFIGURABLE_DEFAULT` |
| `GATE-PO-08` | Tidak ada pembatalan oleh pasien maupun petugas (`KSK-PO-AS-001`). Pendaftaran palsu sebagai pasien baru tetap menahan kuota appointment sampai 23.59 tanggal kunjungan | `S-PO-2` | Diterima, dengan peredam yang sudah diputuskan: captcha, batas laju per IP, satu pendaftaran per NIK per tanggal. Pembatalan oleh petugas menjadi slice berikutnya bila penyalahgunaan terjadi. Menambah status Dibatalkan kelak tidak mengubah status yang lain | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-09` | Isi riwayat | `S-PO-2`, `S-PO-5`, `S-PO-7` | Pola Reservasi Prioritas: status, waktu, sumber (halaman online, perangkat kiosk dengan kode perangkatnya, atau sistem), dan alasan penutupan. Percobaan gagal hanya dicatat jumlah dan waktunya, tanpa data pribadi. Alamat IP tidak disimpan di riwayat | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-10` | Email gagal terkirim | `S-PO-3` | Satu percobaan sesudah pendaftaran tersimpan, kegagalan dicatat di log, tanpa antrean kirim ulang. Lupa Kode menjadi jalan keluarnya (`KSK-DEC-042`) | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-11` | Pemakaian istilah "memindai KTP" di `KSK-DEC-032` | `S-PO-6` | Dibaca sebagai "memindai kartu identitas yang diterima kiosk Pasien Baru" (KTP, KIA, SIM), sehingga anak dengan KIA dapat check-in | `PROPOSED` | `NON_BLOCKING_STANDARD`, dikonfirmasi bersama `KSK-PO-OQ-020` |
| — | Angka batas laju, panjang dan himpunan karakter kode, serta bentuk penampungan consent/rujukan | Beberapa | Ditetapkan saat desain (impact scan §6) | `PROPOSED` | `CONFIGURABLE_DEFAULT` untuk angka, keputusan desain untuk sisanya |

### 4.3 Blocker implementasi (go-live) yang dipertahankan

| ID | Isi | Owner | Status |
| --- | --- | --- | --- |
| `KSK-PO-OQ-010` / `UNK-PO-01` | Domain halaman dan API terbuka ke internet, proxy meneruskan IP asli | IT/infrastruktur | `OPEN` |
| `KSK-PO-OQ-015` | Akun SMTP rumah sakit, termasuk izin koneksi keluar dari server | IT/infrastruktur | `OPEN` |
| `KSK-PO-OQ-016` | Akun Cloudflare Turnstile beserta site key dan secret, termasuk izin koneksi keluar ke Cloudflare | IT/infrastruktur | `OPEN` |

## 5. Kesiapan per slice

| Slice | Kesiapan | Alasan |
| --- | --- | --- |
| `S-PO-1` | `READY_FOR_DOMAIN_DESIGN` | Keputusan tertutup. Sisa usulan non-blocking (`GATE-PO-02`) |
| `S-PO-2` | `BUSINESS_DECISION_REQUIRED` | `KSK-PO-OQ-019`. Bagian lain slice ini (penjamin, rujukan, consent, verifikasi) tidak terdampak |
| `S-PO-3` | `READY_FOR_DOMAIN_DESIGN` | Keputusan tertutup. Akun SMTP adalah blocker go-live |
| `S-PO-4` | `READY_FOR_DOMAIN_DESIGN` | Keputusan tertutup. Perilaku saat Cloudflare tidak terjangkau `CONFIGURABLE_DEFAULT` |
| `S-PO-5` | `READY_FOR_DOMAIN_DESIGN` | Keputusan tertutup. Validasi jadwal ulang sudah ada di jalur kunjungan (`CAP-PO-15`) |
| `S-PO-6` | `BUSINESS_DECISION_REQUIRED` | `KSK-PO-OQ-020` |
| `S-PO-7` | `READY_FOR_DOMAIN_DESIGN`, dengan dependency | Aturan penutupan tertutup. Kuota yang dilepas mengikuti jawaban `KSK-PO-OQ-019` |

**Ringkasan:** `PARTIALLY_READY`.

## 6. Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan ke desain:** `S-PO-1`, `S-PO-3`, `S-PO-4`, `S-PO-5`, dan aturan penutupan `S-PO-7`.

**Harus berhenti sampai keputusan turun:**

- Aturan tanggal dan jenis kuota pada `S-PO-2` (`KSK-PO-OQ-019`).
- Seluruh `S-PO-6` (`KSK-PO-OQ-020`).

**Dependency:**

- `S-PO-7` melepas kuota yang dipakai `S-PO-2`, jadi bagian pelepasan kuota menunggu `KSK-PO-OQ-019`.
- `S-PO-6` memakai layar check-in `S-PO-5` dan menambah langkah pindai kartu identitas.
- `S-PO-3` dan `S-PO-5` memakai kode yang terbit di `S-PO-2`.

**Rekomendasi urutan:** kedua pertanyaan cukup dijawab dalam satu `grill-me` singkat sebelum `design-business-module`. Alasannya, desain menghasilkan satu versi kontrak untuk seluruh Amendment PO. Bila desain dijalankan sebagian lebih dulu, kontrak kuota dan check-in harus diubah lagi.

## 7. Keputusan pemilik yang dibutuhkan

| Decision ID | Pertanyaan singkat | Pilihan yang akan diajukan di `grill-me` |
| --- | --- | --- |
| `KSK-PO-OQ-019` | Pendaftaran online untuk hari ini | (A) Hanya H+1 s.d. H+30. Pasien hari ini memakai kiosk Pasien Lama/Baru (walk-in). Aturan RJ tetap utuh. (B) Boleh hari ini dengan kuota appointment. (C) Boleh hari ini dengan kuota walk-in |
| `KSK-PO-OQ-020` | Pencocokan kartu identitas pasien baru saat check-in | (A) NIK dan tanggal lahir hasil pindai wajib sama dengan data online. Bila tidak sama atau tidak membawa kartu identitas, check-in ditolak dan pasien diarahkan ke loket. Master dibuat dari hasil pindai, sedangkan kontak dari isian online. (B) Hasil pindai selalu menang tanpa syarat kecocokan. (C) Hanya NIK yang wajib sama |

Rekomendasi untuk kedua pertanyaan diberikan di sesi `grill-me`, bukan di dokumen ini.

## 8. Handoff berikutnya

1. **`grill-me` singkat** untuk `KSK-PO-OQ-019` dan `KSK-PO-OQ-020`. Bila sempat, konfirmasi juga `KSK-PO-AS-001` (`GATE-PO-08`) dan `GATE-PO-11` sebagai keputusan, walaupun keduanya tidak memblokir.
2. Sesudah itu, **`design-business-module`** untuk seluruh Amendment PO. `hospital-domain-architect` direkomendasikan **tidak** dipakai (`DOMAIN_ARCHITECTURE_NOT_RUN`), dengan empat alasan:
   - Semua slice berada di bounded context Registration (`KSK-DEC-026` syarat 3).
   - Titik sentuh ke Patient Management dan Master Data sudah punya kontrak yang berjalan (`POST .../patients/kiosk`, `POST .../patient-insurances/kiosk`, opsi poli dan jadwal).
   - Dampak billing nihil.
   - Dampak keselamatan klinis terbatas pada identifikasi pasien, yang diselesaikan lewat `KSK-PO-OQ-020`.
3. Pertahankan pada handoff:
   - `blueprint_id` `KSK-BP-001`, Amendment PO, SHA `5e777833`/`5f18e9f04`.
   - Keputusan `KSK-DEC-026` s.d. `046` dan keputusan baru dari `grill-me`.
   - Impact scan `CAP-PO-01` s.d. `29`.
   - Usulan `GATE-PO-01` s.d. `11`.
   - Blocker go-live `KSK-PO-OQ-010`, `015`, `016`.

---

## 9. Penilaian ulang (revisi 2) — 19 November 2026

### 9.1 Apa yang berubah sejak revisi 1

| Perubahan | Sumber | Akibat bagi penilaian |
| --- | --- | --- |
| Pendaftaran online hanya untuk H+1 s.d. H+30. Semua memakai kuota appointment | `KSK-DEC-047` | `CONFLICT` `KSK-PO-OQ-019` selesai. Backend sudah menerima kunjungan appointment bertanggal hari ini saat check-in (`CAP-PO-36`) |
| Check-in pasien baru: NIK hasil pindai wajib sama dengan NIK online, master dibuat dari hasil pindai, hanya KTP atau KIA | `KSK-DEC-048`, `053` (`049` digantikan) | `MISSING` `KSK-PO-OQ-020` dan `GATE-PO-11` selesai. Langkah pindai kiosk perlu varian dengan NIK terkunci (`CAP-PO-37`) |
| Scope bertambah: layar petugas berisi daftar, filter, pencarian, ubah tanggal/jadwal/kontak, dan batal dengan alasan (status baru Dibatalkan) | `KSK-DEC-050`, `051`, `052` | Slice baru `S-PO-8`. `KSK-PO-AS-001` dan `GATE-PO-08` digantikan |
| Hak akses layar petugas memakai Resource baru (Lihat, Ubah, Batalkan). Kode tetap setelah diubah | `KSK-PO-AS-002`, `003` | Asumsi tercatat, dinilai di §9.3 |
| Addendum impact scan `CAP-PO-30` s.d. `37` | [01-capability-impact-scan-pendaftaran-online.md](01-capability-impact-scan-pendaftaran-online.md) §8 | Tidak ada Missing, Unknown, atau Conflict baru |

### 9.2 Slice yang dinilai

Tujuh slice revisi 1 tetap, ditambah satu slice baru:

| Slice | Isi | Keputusan utama |
| --- | --- | --- |
| `S-PO-8` | Layar petugas Pendaftaran Online: daftar per tanggal kunjungan dan status, ringkasan, pencarian kode/NIK/No. RM/nama, ubah tanggal/jadwal dokter/No. WA-email, dan batal dengan alasan | `KSK-DEC-050`, `051`, `052`, `KSK-PO-FE-005`, `KSK-PO-AS-002`, `003` |

### 9.3 Temuan 18 dimensi untuk `S-PO-8`

| No | Dimensi | Temuan | Klasifikasi | Dampak |
| ---: | --- | --- | --- | --- |
| 01 | Tujuan | Melayani pasien yang meminta perubahan atau pembatalan, dan membersihkan pendaftaran mencurigakan | `CONFIRMED` (`KSK-DEC-050`) | — |
| 02 | Aktor | Petugas admisi yang diberi hak akses | `CONFIRMED`. Peran penerimanya diatur admin | `CONFIGURABLE_DEFAULT` |
| 03 | Pemicu / Prasyarat | Ubah dan batal hanya pada status Menunggu Check-in | `CONFIRMED` (`KSK-DEC-051`, `052`) | — |
| 04 | Alur Utama | Buka daftar, filter atau cari, buka detail, lalu ubah atau batalkan | `CONFIRMED` | — |
| 05 | Alur Alternatif / Exception | Kuota jadwal baru habis, sehingga perubahan ditolak dan pendaftaran lama tetap (`KSK-DEC-051`). **Belum dirinci:** petugas mengubah atau membatalkan tepat saat pasien sedang check-in, dan dua petugas mengubah data yang sama | Sebagian `PROPOSED` | `GATE-PO-16`, `NON_BLOCKING_STANDARD` |
| 06 | Data Minimum | Kolom daftar dan detail belum dirinci. Kode tidak dapat ditampilkan karena hanya hash-nya yang disimpan (`CAP-PO-07`) | `PROPOSED` | `GATE-PO-15`, `NON_BLOCKING_STANDARD` |
| 07 | Aturan Bisnis / Validation | Perubahan tunduk pada H+1 s.d. H+30, izin appointment, kuota, dan satu pendaftaran per pasien per tanggal. Penjamin, rujukan, consent, dan identitas tidak dapat diubah. **Belum jelas:** apakah jadwal baru boleh berada di poli lain | Sebagian `PROPOSED` | `GATE-PO-13`, `NON_BLOCKING_STANDARD` |
| 08 | Status / Perubahan Status | Ubah tidak mengubah status. Batal menuju status akhir Dibatalkan dan tidak dapat dibatalkan ulang | `CONFIRMED` (§13.6) | — |
| 09 | Peran / Authorization | Resource baru dengan aksi Lihat, Ubah, dan Batalkan, didaftarkan otomatis dari atribut controller (`CAP-PO-30`) | `PROPOSED` (`KSK-PO-AS-002`) | `NON_BLOCKING_STANDARD`. Mengikuti pola baku Quilvian dan Reservasi Prioritas |
| 10 | Dependency Antarmodul | Kuota jadwal dokter (`CAP-PO-10`), jadwal dan poli (Master Data) | `CONFIRMED` | Kunci kuota per jadwal: `GATE-PO-17` |
| 11 | Integrasi Internal / Eksternal | Tidak ada integrasi baru | `CONFIRMED` | — |
| 12 | Hasil Akhir | Pendaftaran berpindah tanggal/jadwal/kontak, atau berstatus Dibatalkan dengan kuota dilepas | `CONFIRMED` | — |
| 13 | Pembatalan / Koreksi | Ubah adalah mekanisme koreksi. Batal bersifat final; pasien yang berubah pikiran mendaftar ulang | `CONFIRMED` (`KSK-DEC-052`) | — |
| 14 | Audit / Histori | Nilai lama, nilai baru, petugas, waktu, dan alasan batal tercatat di riwayat | `CONFIRMED` (`KSK-DEC-051`, `052`) | — |
| 15 | Notifikasi | Pasien tidak diberi kabar otomatis setelah diubah atau dibatalkan. Email yang sudah terkirim masih memuat tanggal lama | `PROPOSED` | `GATE-PO-12`, `NON_BLOCKING_STANDARD` |
| 16 | Dampak Billing / Charge | Tidak ada. Belum ada kunjungan | `CONFIRMED` | — |
| 17 | Dampak Keselamatan Klinis | Pindah dokter dalam poli yang sama tidak mengubah makna klinis. Pindah poli dapat membuat surat rujukan tidak sesuai dengan poli tujuan | `PROPOSED` | `GATE-PO-13` |
| 18 | Pelaporan / Traceability | Daftar per status dan riwayat perubahan cukup untuk keperluan operasional | `CONFIRMED` | — |

### 9.4 Status gap setelah revisi 2

**Gap pemblokir yang ditutup:**

| ID | Penutup | Status |
| --- | --- | --- |
| `KSK-PO-OQ-019` | `KSK-DEC-047` | `CONFIRMED` |
| `KSK-PO-OQ-020` | `KSK-DEC-048` dan `053` | `CONFIRMED` |
| `KSK-PO-OQ-021` (muncul dari jawaban konfirmasi `KSK-PO-AS-001`) | `KSK-DEC-050`, `051`, `052` | `CONFIRMED` |
| `KSK-PO-OQ-022` (SIM tanpa NIK) | `KSK-DEC-053` | `CONFIRMED` |

**Usulan revisi 1 yang berubah:**

| ID | Perubahan |
| --- | --- |
| `GATE-PO-08` | Tidak berlaku lagi. Pembatalan oleh petugas diputuskan `KSK-DEC-052`, sehingga pendaftaran palsu dapat dibersihkan sebelum hari H |
| `GATE-PO-11` | Tidak berlaku lagi. Ditutup `KSK-DEC-049`, lalu diganti `KSK-DEC-053` (KTP dan KIA) |
| `GATE-PO-04` | Tetap. Aturan "satu per NIK per tanggal" juga berlaku saat petugas memindahkan tanggal (`KSK-DEC-051`) |
| `GATE-PO-01` s.d. `03`, `05` s.d. `07`, `09`, `10` | Tetap sebagai usulan non-blocking |

**Usulan non-blocking baru:**

| ID | Gap | Slice | Usulan | Klasifikasi | Dampak |
| --- | --- | --- | --- | --- | --- |
| `GATE-PO-12` | Kabar ke pasien sesudah petugas mengubah atau membatalkan | `S-PO-8` | Tidak ada kiriman otomatis. Perubahan umumnya atas permintaan pasien sendiri lewat telepon, sehingga petugas menyampaikannya langsung. Kode tetap berlaku (`KSK-PO-AS-003`), dan kiosk menampilkan tanggal yang baru | `PROPOSED` | `NON_BLOCKING_STANDARD`. Kiriman email perubahan dapat ditambah kemudian tanpa mengubah status |
| `GATE-PO-13` | Ganti jadwal dokter ke poli lain | `S-PO-8` | Hanya jadwal di poli yang sama. Pindah poli dilakukan dengan membatalkan lalu mendaftar ulang, karena surat rujukan dan pilihan penjamin terikat pada poli tujuan. Contoh: pindah dari dr. Sari ke dr. Rina di Poli Anak diperbolehkan. Pindah dari Poli Anak ke Poli THT ditolak dengan arahan "Batalkan lalu daftar ulang" | `PROPOSED` | `NON_BLOCKING_STANDARD`. Kelak dapat dilonggarkan tanpa mengubah struktur |
| `GATE-PO-14` | NIK hasil pindai hanya dijamin oleh layar kiosk, karena OCR berjalan di agen pemindai lokal dan backend tidak menerima hasil OCR mentah (`CAP-PO-37`) | `S-PO-6` | Layar check-in mengunci NIK pada hasil OCR, lalu backend membandingkan NIK kiriman kiosk dengan NIK online. Sisa risiko diterima, sama seperti kiosk Pasien Baru yang sudah berjalan, karena perangkatnya sudah login sebagai akun kiosk | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-15` | Kolom daftar dan detail | `S-PO-8` | Daftar memuat tanggal kunjungan, poli, dokter, nama pasien, No. RM atau label "Pasien baru", jenis pasien, kontak, dan status. NIK tampil penuh bagi pemegang hak akses. Alasan batal berupa teks bebas wajib, seperti Reservasi Prioritas. Kode tidak ditampilkan | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-16` | Dua aksi bersamaan pada pendaftaran yang sama, misalnya petugas membatalkan saat pasien sedang check-in | `S-PO-5`, `S-PO-8` | Token konkurensi seperti `RowVersion` Reservasi Prioritas (`CAP-PO-33`). Aksi kedua ditolak dengan pesan "Data sudah berubah, muat ulang" | `PROPOSED` | `NON_BLOCKING_STANDARD` |
| `GATE-PO-17` | Kunci kuota per jadwal dan tanggal (`CAP-PO-34`). AC 3 dan AC 19 menuntut kuota tidak terpakai ganda | `S-PO-2`, `S-PO-5`, `S-PO-8` | Kunci dipasang di semua jalur pendaftaran online: daftar, ubah, dan check-in. Pemasangan di jalur petugas, kiosk, dan IGD ditimbang saat desain, lalu dicatat sebagai titik sentuh blueprint `rawat-jalan` (`GATE-PO-06`) | `PROPOSED` | `NON_BLOCKING_STANDARD`. Ini aturan integritas data, bukan aturan bisnis baru |

**Blocker go-live yang tetap:** `KSK-PO-OQ-010`, `015`, `016` (§4.3).

### 9.5 Kesiapan per slice (revisi 2)

| Slice | Kesiapan | Dasar |
| --- | --- | --- |
| `S-PO-1` | `READY_FOR_DOMAIN_DESIGN` | Tidak berubah |
| `S-PO-2` | `READY_FOR_DOMAIN_DESIGN` | `KSK-DEC-047` menutup `KSK-PO-OQ-019` |
| `S-PO-3` | `READY_FOR_DOMAIN_DESIGN` | Tidak berubah |
| `S-PO-4` | `READY_FOR_DOMAIN_DESIGN` | Tidak berubah |
| `S-PO-5` | `READY_FOR_DOMAIN_DESIGN` | Tidak berubah. Kunjungan appointment hari ini sudah diterima backend (`CAP-PO-36`) |
| `S-PO-6` | `READY_FOR_DOMAIN_DESIGN` | `KSK-DEC-048` dan `053` menutup `KSK-PO-OQ-020` dan `022` |
| `S-PO-7` | `READY_FOR_DOMAIN_DESIGN` | Dependency kuota selesai. Pelepasan kuota juga terjadi saat Dibatalkan dan saat jadwal diubah |
| `S-PO-8` | `READY_FOR_DOMAIN_DESIGN` | Keputusan tertutup (`KSK-DEC-050` s.d. `052`). Sisa usulan non-blocking |

**Ringkasan revisi 2:** `READY_FOR_DOMAIN_DESIGN` untuk seluruh Amendment PO. Tidak ada butir `BLOCKING` yang terbuka.

### 9.6 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan:** seluruh delapan slice, ke `design-business-module` sebagai satu desain dengan satu versi kontrak.

**Harus berhenti:** tidak ada untuk desain. Go-live tetap menunggu `KSK-PO-OQ-010`, `015`, dan `016` dari IT/infrastruktur.

**Dependency yang dibawa ke desain:**

- `S-PO-8` dan `S-PO-7` memakai perhitungan kuota yang sama dengan `S-PO-2` (`CAP-PO-10`, `GATE-PO-17`).
- `S-PO-6` memakai layar check-in `S-PO-5` dan varian langkah pindai dengan NIK terkunci (`CAP-PO-37`).
- Perubahan pada jalur bersama (hitungan kuota, penutupan otomatis di pembuatan kunjungan, kunci kuota) dicatat sebagai titik sentuh blueprint `rawat-jalan` (`GATE-PO-06`).

### 9.7 Handoff berikutnya

1. **`design-business-module`** untuk seluruh Amendment PO.
2. `hospital-domain-architect` tetap direkomendasikan **tidak** dipakai (`DOMAIN_ARCHITECTURE_NOT_RUN`), dengan alasan yang sama seperti §8. Layar petugas tidak menambah bounded context, karena data, status, dan pemiliknya sama.
3. Pertahankan pada handoff:
   - `blueprint_id` `KSK-BP-001`, Amendment PO, SHA `5e777833`/`5f18e9f04`.
   - Keputusan `KSK-DEC-026` s.d. `053` (`049` berstatus `superseded`), `KSK-PO-FE-001` s.d. `005`, `KSK-PO-AS-002`, `003`.
   - Impact scan `CAP-PO-01` s.d. `37`.
   - Usulan non-blocking `GATE-PO-01` s.d. `07`, `09`, `10`, `12` s.d. `17`.
   - Acceptance criteria 1–20.
   - Blocker go-live `KSK-PO-OQ-010`, `015`, `016`.
   - Pertanyaan desain pada impact scan §6 butir 1–6 dan §8.3 butir 7–10.
