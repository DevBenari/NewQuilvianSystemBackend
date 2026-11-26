# Laporan Perubahan Backend — `BE-KSK-PO-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-005` |
| Judul | Kirim pendaftaran online |
| Slice | `EPIC KSK-PO-02` — halaman publik backend (`FR-KSK-PO-012`), gelombang teknis 4, rilis `MVP-1` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-005` |
| Trace | `KSK-DEC-030`, `032`, `034`, `035`, `038`, `042`, `043`, `047`; `INV-PO-01` s.d. `06`; `KSK-DSN-PO-002`, `004`, `005`, `009`, `011`, `012`, `013`; `KSK-VAL-PO-001` s.d. `034` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | `BE-KSK-PO-004` ✅ |
| Klasifikasi | `HIGH` — endpoint publik tanpa login yang menulis data, menerima unggah berkas, memakai kuota dokter, dan mengirim email |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Controllers,DTOS,Services}`, `Program.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a86001e2` (`sukmagp`) |
| Tanggal | 2026-11-20 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di runtime dan di database (kueri baca-saja dengan izin pemilik) |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` (`OnlineRegistrationPublicService.Submit.cs`, DTO kirim, endpoint `POST`) dan `TOUCHED LEGACY` (`ReferralDocumentStorageService`, `OutpatientGeneralConsentService`: empat helper privat dibuka menjadi `internal` tanpa perubahan isi) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), `QBE-CODE-006` (nomor bisnis lewat `NumberSeriesAllocator`), `QBE-CODE-003` (nomor tidak dihitung dari data), `QBE-API-001`/`QBE-DTO-001` (route `api/v1/...`, `ApiResponse<T>`), `QBE-LOG-001` (log tanpa data pribadi), `QBE-PERM-001` (endpoint publik tanpa `[AccessController]`, sama dengan endpoint publik `BE-KSK-PO-004`) |
| Hardcode role | Tidak ada |

## 1. Masalah yang diperbaiki

Halaman publik sudah bisa menampilkan pilihan poli, jadwal, dan mencocokkan pasien lama (`BE-KSK-PO-004`), tetapi belum bisa **menyimpan** pendaftaran. Task ini menambah satu endpoint yang menerima seluruh isian halaman online, menyimpannya sebagai pendaftaran berstatus Menunggu Check-in, memakai satu kuota appointment, lalu mengembalikan nomor pendaftaran dan kode check-in.

## 2. Proses bisnis

1. Pasien menyelesaikan langkah-langkah halaman online, lalu menekan Kirim. Halaman mengirim isian (JSON) dan 0–3 berkas surat rujukan sekaligus.
2. Server memeriksa bentuk isian lebih dulu tanpa menyentuh database: data pasien, kontak, tanggal, jenis pasien dan penjamin, rujukan, consent, dan berkas. Contoh: kolom kontak `0812-3456-7890` diterima dan disimpan sebagai `081234567890`; `uji@contoh` ditolak "Alamat email tidak valid."
3. Captcha diperiksa. Ditolak Cloudflare → `400`; Cloudflare tidak dapat dihubungi → `503`.
4. Pasien lama dicocokkan ulang dengan NIK/No. RM dan tanggal lahir yang dikirim ulang, karena halaman tidak menyimpan sesi (`KSK-DSN-PO-005`). Tidak cocok → `404` dengan pesan seragam.
5. Server memastikan jadwal masih ditawarkan (aktif, appointment **dan** kiosk diizinkan, berlaku pada tanggal itu), kartu penjamin memang milik pasien, penyedia asuransi kartu baru aktif, dan faskes perujuk dikenal.
6. Transaksi dibuka. Kunci pasien diambil dulu, lalu kunci kuota jadwal dan tanggal. Di bawah kunci, server memeriksa ulang: belum ada pendaftaran Menunggu Check-in untuk pasien/NIK itu pada tanggal yang sama, pasien lama tidak punya kunjungan Rawat Jalan aktif pada tanggal itu, dan kuota appointment masih ada.
7. Kode 8 karakter diterbitkan (contoh tampilan `K7QM-3XRP`), hanya hash SHA-256-nya yang disimpan. Nomor diterbitkan lewat alokator nomor bersama (contoh `PO-20261120-0001`). Berkas ditulis ke folder privat `online-registration-referrals/{id}/`. Baris pendaftaran, berkas, dan riwayat `SUBMITTED` disimpan bersama.
8. Sesudah tersimpan, bila kontaknya email, kode dikirim lewat SMTP. Email gagal tidak membatalkan pendaftaran; statusnya dicatat dan dikembalikan sebagai `emailStatus`.
9. Respons berisi nomor, kode, tanggal, poli, dokter, jam, status email, dan No. WhatsApp (untuk tombol "Kirim kode ke WhatsApp saya"). Kode tidak dapat diambil lagi sesudah ini.

Contoh: Ibu Rina (pasien lama) mendaftar Kamis malam untuk poli Paru hari Jumat, kontak email. Respons `200` berisi `PO-20261120-0001` dan kodenya; kuota appointment dr. di jadwal itu turun dari 5 menjadi 4; satu menit kemudian email berisi kode tiba. Bila ia menekan Kirim sekali lagi, jawabannya `409` "Anda sudah memiliki pendaftaran online pada tanggal ini."

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OutpatientPriorityReservationService.cs` (pola kunci pasien, cek kunjungan aktif, `NumberSeriesAllocator`, hash SHA-256); `NumberSeriesAllocator.cs` dan `NumberAllocationRequest.cs`; `ReferralDocumentStorageService.cs` (root privat, deteksi isi berkas, pembersihan berkas); `OutpatientGeneralConsentService.cs` dan DTO-nya (aturan consent RJ); `OutpatientEncounterRules.cs` (`WhereBlocksRegistration`, saklar kunjungan aktif); `PatientEncounterController.cs` (kunci `REG_ENCOUNTER_ACTIVE_*`, batas unggah multipart); `SmtpEmailSender.cs`; `DoctorScheduleQuotaService.cs`; modul lain yang memakai `SystemFallbackUserId`; konstanta langkah RJ di frontend (`outpatient-registration.constants.js`, langkah Penjamin rujukan menawarkan Pribadi).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Services/OnlineRegistrationPublicService.Submit.cs` | **Baru** (partial). `SubmitAsync`: validasi bentuk, baca dan periksa berkas, captcha, pencocokan ulang, cek jadwal/penjamin/faskes, transaksi berkunci, kode + hash, nomor `PO`, simpan berkas dengan kompensasi hapus, riwayat `SUBMITTED`, email sesudah transaksi. Helper publik `NormalizeCode`, `ComputeCodeHash`, `NormalizeWhatsAppNumber` untuk dipakai ulang check-in (`BE-KSK-PO-006`) dan ubah kontak (`BE-KSK-PO-010`) |
| `.../Services/OnlineRegistrationPublicService.cs` | Kelas menjadi `partial`; konstruktor menerima `NumberSeriesAllocator`, `ReferralDocumentStorageService`, `SmtpEmailSender`, `IConfiguration`; outcome `Conflict` (`409`); kueri penyedia asuransi aktif dijadikan satu helper untuk opsi dan pemeriksaan kirim |
| `.../Controllers/OnlineRegistrationPublicController.cs` | `POST` multipart (`payload` + `referralDocuments`), batas badan 20 MB, policy `OnlineRegistrationPublicSubmit`, pemetaan `409` |
| `.../DTOS/OnlineRegistrationPublicDtos.cs` | `OnlineRegistrationSubmitRequest` beserta input pasien lama, pasien baru, kartu baru, rujukan, consent; `OnlineRegistrationSubmitResponse` |
| `.../Services/ReferralDocumentStorageService.cs` | `TryResolvePrivateRoot`, `DetectContentType`, `SanitizeFileName`, `DeleteFiles`: `private` → `internal`. Isi tidak berubah |
| `.../Services/OutpatientGeneralConsentService.cs` | `IsPngDataUrl`: `private` → `internal`. Isi tidak berubah |
| `Program.cs` | Policy `OnlineRegistrationPublicSubmit` (per IP, `RegistrationPermitPerHour`, bawaan 10/jam) dan cabang `OnRejected` dengan pesan publik |

**Delta terhadap desain (perlu diketahui pemilik):**

| Desain | Source | Alasan |
| --- | --- | --- |
| Nomor `PO-yyMMdd-nnnn` (roadmap, kamus data) | `PO-yyyyMMdd-nnnn`, contoh `PO-20261120-0001` | `NumberSeriesAllocator` (wajib menurut `QBE-CODE-006`) hanya menyusun periode harian `yyyyMMdd`, sama dengan nomor Reservasi Prioritas `RSV-yyyyMMdd-nnnn`. Panjang 16 karakter, masih di bawah kolom 30. Tidak mengubah bentuk respons |
| Pelaku alokasi nomor | `00000000-0000-0000-0000-000000000001` | Alokator menolak pelaku kosong; halaman publik tidak punya pengguna. Nilai sama dengan `SystemFallbackUserId` modul lain. Baris pendaftaran tetap `CreateBy` kosong dan riwayat `ActorUserId` kosong sesuai kamus data |
| DTO di `OnlineRegistrationDtos.cs` | `OnlineRegistrationPublicDtos.cs` | Melanjutkan pemisahan DTO publik dari `BE-KSK-PO-004` |
| — | `existingPatient` memakai DTO sendiri (`identityType`, `identityValue`, `birthDate`) | Sama dengan contoh kontrak; tanpa field captcha bersarang |
| — | Rujukan wajib 1–3 berkas; Pasien Pribadi/Asuransi yang mengirim rujukan atau berkas ditolak `400` | `KSK-DSN-PO-011` menyebut 1–3 berkas; pesan memakai `KSK-VAL-PO-030` |
| — | Consent yang dikirim untuk pasien lama ditolak `400` | `KSK-DEC-034`: consent hanya untuk pasien baru. Menolak lebih aman daripada diam-diam membuang isian |
| — | Batas bawah No. WA 8 angka | Setara 9 digit bentuk `62…` pada `KSK-VAL-006`; batas atas 13 angka dari `KSK-DEC-022` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: `POST online-registrations/public` sesuai `KSK-CONTRACT-v2`. Endpoint lain tidak berubah |
| Database | Menulis ke tabel `RegOnlineRegistration`, `RegOnlineRegistrationReferralDocument`, `RegOnlineRegistrationHistory` (migration `BE-KSK-PO-001`, sudah diterapkan). Tanpa migration baru. Deret nomor baru `REG_ONLINE_REGISTRATION_NUMBER` dibuat otomatis oleh alokator pada pemakaian pertama |
| Keamanan/Auth | Endpoint publik pertama yang **menulis** data dan menerima berkas. Penjagaan: captcha, batas laju per IP 10/jam, batas badan 20 MB, isi berkas diperiksa dari magic bytes, berkas di root privat dengan nama acak, `404` seragam, kode hanya disimpan sebagai hash, `no-store`, log tanpa NIK/No. RM/tanggal lahir/kontak/kode |
| Berkas | Folder baru `online-registration-referrals/{id}/` di bawah `FileStorage:PrivateRootPath`. Bila root privat belum siap, kirim dengan berkas dijawab `503` dan log mencatat nama kunci konfigurasinya saja |

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Online Registration Public

Base URL: `api/v1/health-services/registration-management/online-registrations/public`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `` | Mengirim pendaftaran online | Tanpa login, `OnlineRegistrationPublicSubmit` (10/jam/IP), captcha | `multipart/form-data`: `payload` (teks JSON `OnlineRegistrationSubmitRequest`), `referralDocuments` (0–3 berkas, wajib 1–3 untuk Pasien Rujukan) | `ApiResponse<OnlineRegistrationSubmitResponse>` |

Contoh respons `200` (data contoh):

```json
{
  "registrationNumber": "PO-20261120-0001",
  "code": "K7QM-3XRP",
  "visitDate": "2026-11-21",
  "clinicName": "Poli Paru",
  "doctorName": "dr. Contoh",
  "startTime": "09:07",
  "endTime": "09:12",
  "emailStatus": 1,
  "whatsAppNumber": null
}
```

`emailStatus`: `0` tidak diminta (kontak No. WA), `1` terkirim, `2` gagal. `payload` dikirim sebagai field teks biasa; bila frontend memakai `Blob`, jangan beri nama berkas, karena bagian berberkas dibaca sebagai berkas.

Kode status: `200` berhasil; `400` isian tidak sah, captcha ditolak, tanggal di luar H+1 s.d. H+30, berkas terlalu besar atau bukan PDF/JPG/PNG; `404` data pasien lama tidak cocok (pesan seragam); `409` jadwal tidak tersedia, kuota penuh, sudah ada pendaftaran online atau kunjungan aktif pada tanggal itu; `413` badan permintaan lebih dari 20 MB; `429` lebih dari 10 kiriman per jam dari satu IP; `503` captcha tidak dapat diperiksa, fitur dimatikan, atau penyimpanan belum siap.

## 5. Verifikasi

Build dari source saat ini disalin ke scratchpad dan dijalankan di `https://localhost:7185` terhadap `QuilvianNewDevSukma`. Kunci uji resmi Cloudflare dan alamat SMTP diberikan lewat variabel lingkungan, tidak ditulis ke berkas. Empat run backend: (A) captcha lolos + SMTP sink lokal; (B) SMTP tidak terjangkau; (C) captcha selalu gagal + batas laju bawaan; (D) Cloudflare tidak terjangkau. Backend pengguna di 7184 tidak disentuh.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | Build succeeded, 0 error, 0 peringatan pada berkas task ini | `PASS` | Keluaran build |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | PASS: metadata gap 0, endpoint telanjang baru 0, identitas wajib 24/24 | `PASS` | Keluaran verifier |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 37 berkas, VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| T1–T1c pasien lama, Pribadi/Tunai, email | `200`, kode `XXXX-XXXX`, nomor `PO-20261120-0001`, respons hanya 9 field kontrak, `no-store` | `PASS` | `po005_test.py main` |
| T1d–T1e kuota | `doctor-schedule-quotas` `usedAppointment` 1→2 dan `usedTotal` 1→2; sisa kuota opsi publik 5→4 | `PASS` | `po005_test.py main` |
| T1f–T1g email (SMTP sink) | Email diterima; berisi kode, nomor, tanggal, poli, dokter, jam, petunjuk kiosk; tanpa NIK, No. RM, tanggal lahir; kode tidak di subjek | `PASS` | `po005_test.py main` |
| T2 kirim kedua pasien dan tanggal sama | `409` `KSK-VAL-PO-023` | `PASS` | `po005_test.py main` |
| T3 dua kiriman identik bersamaan | `[200, 409]` | `PASS` | `po005_test.py main` |
| T4 pasien baru Rujukan + kartu asuransi baru + PDF dan PNG + consent, kontak `0812-3456-7890` | `200`, `whatsAppNumber = 081234567890`, `emailStatus = 0`; 2 berkas tersimpan utuh di `online-registration-referrals/{id}/` dengan nama acak | `PASS` | `po005_test.py main` |
| T4b master pasien | 5 NIK pasien baru online: 0 baris master pasien (kontrol positif NIK pasien lama = 1) | `PASS` | Pemeriksaan ulang lewat API daftar pasien (skrip awalnya salah membaca nama field jumlah; diperbaiki lalu dijalankan ulang) |
| T5a–T5ag validasi (34 skenario) | Kontak kosong/WA salah/WA terlalu pendek/14 digit/email salah; tanggal hari ini dan H+31; jenis kelamin, nama, NIK 15 digit; consent kurang klausul, tanpa tanda tangan, tanpa consent, tanda tangan 400 KB, consent untuk pasien lama; berkas 8 MB, `.exe` bernama `.pdf`, 4 berkas, rujukan tanpa berkas, tanpa faskes, faskes asing, surat rujukan besok, berkas pada Pasien Pribadi; kartu baru tanpa polis, penyedia asing, kartu pasien lain, kartu lama untuk pasien baru, Pribadi dengan asuransi; tanggal lahir salah `404`; jadwal tanpa izin kiosk `409`, poli lain `409`; payload bukan JSON, tanpa payload. Semua dengan kode dan pesan persis matriks validasi | `PASS` | `po005_test.py main` |
| T5ah pasien lama dengan kunjungan RJ appointment aktif pada H+1 | `409` `KSK-VAL-PO-024` | `PASS` | `po005_test.py main` |
| T6a–T6b jadwal berkuota appointment 1 | Slot terakhir `200`; kiriman berikutnya `409` `KSK-VAL-PO-022` | `PASS` | `po005_test.py main` |
| T6c bagian `payload` ber-`Content-Type: application/json` | `200` | `PASS` | `po005_test.py main` |
| T7 penolakan tidak menyimpan baris | `usedAppointment` jadwal A = awal + 4 (tepat empat kiriman sukses) | `PASS` | `po005_test.py main` |
| B1–B2 SMTP mati | `200`, kode tampil, `emailStatus = 2`; kuota tetap terpakai | `PASS` | `po005_test.py smtpdown` |
| C1–C3 captcha kunci "selalu gagal" | `400` `KSK-VAL-PO-010`; kiriman ke-2 s.d. ke-10 `400`, ke-11 `429` dengan `Retry-After: 3600`; kuota tidak berubah | `PASS` | `po005_test.py captchafail` |
| D1 Cloudflare tidak terjangkau | `503` `KSK-VAL-PO-011` | `PASS` | `po005_test.py captchadown` |
| L1 log empat run | 21 nilai sensitif (NIK, No. RM, tanggal lahir, email, No. WA, kode) dicari di 4 berkas log: 0 temuan. Yang tercatat hanya id, nomor, kode aturan, dan status email | `PASS` | `po005_test.py logcheck` |
| DB1–DB11 kueri baca-saja (izin Sukma Giri Pratama 2026-11-20, sesi `READ ONLY`, hanya `SELECT`) | 6 pendaftaran uji: 6/6 hash = SHA-256 kode yang terbit dan kode asli tidak ada di baris mana pun; pasien baru `PatientId` kosong, NIK dan nama hanya di pendaftaran; pasien lama tanpa salinan NIK/nama; consent 12 klausul + tanda tangan PNG + `ConsentSignedAt`; status 1, `CreateBy` kosong; `EmailDeliveryStatus` `[1,1,1,1,2]` dengan `EmailAttemptedAt`; WA `081234567890` status 0; 6 riwayat `SUBMITTED` (sumber halaman publik, tanpa aktor); 2 baris berkas dengan path relatif dan urutan 1–2; 0 `RegPatientGeneralConsent` baru selama jendela uji | `PASS` | `po005_dbcheck.py` |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

**Data uji:** dibuat lewat API. Tiga jadwal `TEST-PO005 …` (Poli Paru, 2026-11-21) sudah dinonaktifkan; satu kunjungan appointment pasien `RJPR UJI*` untuk uji kunjungan aktif sudah dibatalkan. Enam pendaftaran online uji `PO-20261120-0001` s.d. `0006` tetap berstatus Menunggu Check-in pada jadwal uji yang nonaktif, karena endpoint batal baru tersedia di `BE-KSK-PO-010`; keenamnya akan ditutup Tidak Datang oleh `BE-KSK-PO-008` atau dapat dibatalkan lewat `BE-KSK-PO-010`. Satu di antaranya milik pasien lama dev, sehingga pasien itu tidak dapat mendaftar online lagi untuk 2026-11-21. Dua berkas uji tertulis di root privat relatif milik build scratchpad, bukan di folder repository.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 3 / `INV-PO-01`: kirim pasien lama untuk besok → `200`, status Menunggu Check-in, kuota appointment berkurang 1, tidak menjadi kunjungan | Terpenuhi | T1, T1d, T1e, DB6. Tidak ada baris kunjungan dibuat oleh jalur ini (service tidak menulis `RegPatientEncounter`) |
| AC 4: pasien baru → tidak ada master pasien baru, `PatientId` kosong | Terpenuhi | T4b, DB2 |
| AC 5 (email): email berisi kode, nomor, tanggal, poli, dokter; tanpa NIK/No. RM; `EmailDeliveryStatus = 1` | Terpenuhi | T1f, T1g, DB7 |
| AC 6: kontak kosong `400`; captcha gagal `400` tanpa baris; kedua kali `409`; hari ini dan H+31 `400`; kiriman ke-11 `429` | Terpenuhi | T5a, C1, C3, T2, T5f, T5g, C2 |
| AC 14: consent pasien baru tersimpan di pendaftaran, belum ada `RegPatientGeneralConsent` | Terpenuhi | T4, DB4, DB11 |
| AC 15: berkas 8 MB `400` `029`; `.exe` bernama `.pdf` `400` `030` | Terpenuhi | T5p, T5q |
| AC 16: tanggal hari ini dan H+31 ditolak | Terpenuhi | T5f, T5g |
| Tambahan (1) kode hanya di respons, tidak tersimpan asli | Terpenuhi | T1c, DB1, L1 |
| Tambahan (2) email tanpa NIK/No. RM/penjamin | Terpenuhi | T1g; isi email tidak memuat field penjamin (`BuildCodeEmail`) |
| Tambahan (3) SMTP mati → `200` dengan `emailStatus = 2` | Terpenuhi | B1, DB7 |
| Tambahan (4) kirim ganda bersamaan → satu `200`, satu `409` | Terpenuhi | T3 |
| `KSK-DSN-PO-013` jadwal tanpa izin kiosk dikirim langsung → `409` | Terpenuhi | T5ad |
| `KSK-DEC-035` kartu milik pasien lain → `400` `025` | Terpenuhi | T5z |
| DoD: seluruh kriteria terbukti; log tanpa data sensitif | Terpenuhi | Bagian 5, L1 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Batas laju memakai IP koneksi; di balik proxy tanpa penerusan IP asli, semua pasien terhitung satu IP (`KSK-PO-OQ-010`, IT). Email dikirim di dalam permintaan, sehingga respons dapat tertunda sampai `Email:Smtp:TimeoutSeconds` (bawaan 10 detik) bila server SMTP lambat |
| Masalah yang diketahui | Format nomor `PO-yyyyMMdd-nnnn`, bukan `PO-yyMMdd-nnnn` (lihat delta). Bila pemilik tetap menginginkan `yyMMdd`, alokator platform perlu opsi format periode baru di luar task ini |
| Risiko tersisa | Unggah berkas tanpa login. Isi berkas diperiksa dari magic bytes dan disimpan di root privat, tetapi belum ada pemindaian antivirus (tidak diminta). Saat go-live, pastikan `FileStorage:PrivateRootPath` berada di luar folder publik dan kunci Turnstile serta SMTP produksi terisi (`KSK-PO-OQ-015`, `016`) |
| Perubahan sampingan | `NONE` di repository. Keluaran build, skrip uji, SMTP sink, dan log berada di scratchpad; `bin/Release` tidak dilacak |
| Interupsi | Sesi terputus sesudah build; dilanjutkan dari kondisi build terakhir tanpa mengulang langkah yang sudah selesai |
| Status Git | Milik task ini: 1 berkas baru (`OnlineRegistrationPublicService.Submit.cs`) dan perubahan pada `OnlineRegistrationPublicService.cs`, `OnlineRegistrationPublicController.cs`, `OnlineRegistrationPublicDtos.cs` (ketiganya masih berkas baru dari `BE-KSK-PO-004`), `ReferralDocumentStorageService.cs`, `OutpatientGeneralConsentService.cs`, `Program.cs`; ditambah perubahan task sebelumnya yang belum di-commit. Tidak ada commit |
| Dampak ke task lain | `BE-KSK-PO-002` AC 2 kini terbukti di runtime dengan baris pendaftaran online sungguhan (T1d) |
| Langkah berikutnya | `BE-KSK-PO-006` (verifikasi check-in kiosk), `BE-KSK-PO-008`, atau `BE-KSK-PO-009` (boleh paralel). Frontend `FE-KSK-PO-003` (kirim dan langkah Selesai) kini memiliki backend |
