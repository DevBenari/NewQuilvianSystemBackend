# Laporan Perubahan Backend — `BE-KSK-PO-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-003` |
| Judul | Pemeriksa captcha dan pengirim email |
| Slice | `EPIC KSK-PO-02` — halaman publik backend (`FR-KSK-PO-013`), gelombang teknis 1, rilis `MVP-1` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-003` |
| Trace | `KSK-DEC-042`, `044`; `KSK-DSN-PO-006`, `007`; `GATE-PO-07`, `GATE-PO-10`; `contracts/integration-contract.md` `INT-PO-01`, `INT-PO-03`; `KSK-VAL-PO-010`, `011` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | — (tidak ada prasyarat) |
| Klasifikasi | `MEDIUM` — dua layanan platform baru, satu integrasi HTTP keluar pertama di backend, tanpa entity, endpoint, maupun migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Services/Security/`, `Services/Notification/`, `Program.cs`, `appsettings.json`, serta laporan dan baris status roadmap kiosk |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a86001e2` (`sukmagp`). Source aplikasi identik dengan baseline roadmap `5e777833`; selisihnya hanya dokumen blueprint |
| Tanggal | 2026-11-19 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | Infrastruktur platform root (`Services/Security`, `Services/Notification`), dipakai Registration (`Reg`) |
| Owner/prefix registry | Tidak berlaku: tidak ada entity operasional baru |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-MOD-001` (penempatan di folder layanan platform yang sudah ada polanya), `QBE-SVC-001` (layanan tanpa akses context, controller belum ada), `QBE-LOG-001` (log tanpa data sensitif; layanan ini tidak mengubah state sehingga log aktor belum berlaku) |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*`, `QBE-CODE-*`, `QBE-PERM-001`, `QBE-API-001` — tidak ada entity, configuration EF, nomor bisnis, endpoint, atau hak akses yang disentuh |

## 1. Masalah yang diperbaiki

Halaman publik Pendaftaran Pasien Online membutuhkan dua kemampuan yang belum ada di backend sama sekali (`F-PO-8`, `CAP-PO-04`, `CAP-PO-05`):

1. **Pemeriksa captcha.** Tanpa ini, halaman tanpa login dapat diisi robot dan menahan kuota dokter (`KSK-DEC-038`, `044`).
2. **Pengirim email.** Tanpa ini, kode pendaftaran hanya tampil di layar dan tidak sampai ke email pasien (`KSK-DEC-042`).

Task ini membangun keduanya sebagai layanan yang siap dipanggil. Layanan belum dipakai endpoint mana pun; pemakainya adalah `BE-KSK-PO-004` (pencocokan pasien) dan `BE-KSK-PO-005` (kirim pendaftaran).

## 2. Proses bisnis

**Captcha.** Saat pasien menekan Kirim di halaman online, widget Cloudflare di browser memberi token. Backend meneruskan token itu ke Cloudflare dan mendapat jawaban:

| Keadaan | Hasil | Akibat bagi pasien (kelak di `BE-KSK-PO-004/005`) |
| --- | --- | --- |
| Token sah | `Passed` | Pendaftaran diproses |
| Token kosong atau ditolak Cloudflare | `Rejected` | "Verifikasi keamanan gagal. Silakan ulangi." (`400`) |
| Cloudflare tidak menjawab dalam 5 detik, menolak koneksi, atau secret belum diisi | `Unavailable` | "Layanan sedang tidak tersedia…" (`503`). Tidak ada data tersimpan |
| `Turnstile:Enabled = false` (dev saja) | `Passed` | Aplikasi mencatat peringatan saat menyala |

Contoh: Cloudflare sedang gangguan. Pasien menunggu paling lama 5 detik, lalu diminta mencoba lagi atau daftar di loket. Captcha **tidak** diloloskan diam-diam, kecuali IT sengaja menyalakan `Turnstile:FailOpen`.

**Email.** Sesudah pendaftaran tersimpan, backend mengirim satu email teks. Hasilnya dicatat, bukan dilempar sebagai galat:

| Keadaan | Hasil |
| --- | --- |
| Server SMTP menerima | `Sent` |
| Server menolak, tidak terjangkau, alamat tidak sah, atau tidak menjawab dalam 10 detik | `Failed` — pendaftaran tetap sah, kode tetap tampil, Lupa Kode tersedia |
| `Email:Smtp:Enabled = false` atau Host/FromAddress kosong | `Disabled` |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `Program.cs` (pola `Configure<T>`, blok sesudah `builder.Build()`); `Services/Security/AccessPermissionService.cs` (gaya namespace); `Areas/HealthServices/RegistrationManagement/Services/KioskEncounterClosureOptions.cs` (pola kelas opsi); `appsettings.json`, `appsettings.Development.json`; `QuilvianSystemBackend.csproj` (`ImplicitUsings`, tanpa paket HTTP/email tambahan).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/Security/TurnstileOptions.cs` | **Baru.** Opsi `Turnstile`: `Enabled`, `SiteKey`, `SecretKey` (rahasia), `VerifyUrl`, `TimeoutSeconds` (5), `FailOpen` (`false`) |
| `Services/Security/TurnstileVerificationService.cs` | **Baru.** `VerifyAsync(token, remoteIp)` → `Passed`/`Rejected`/`Unavailable`; klien bernama `Turnstile`, batas waktu 5 detik lewat `CancellationTokenSource`, tanpa retry, fail-closed; `WarnIfMisconfigured` untuk peringatan saat menyala |
| `Services/Notification/SmtpEmailOptions.cs` | **Baru** (folder baru). Opsi `Email:Smtp`: `Enabled` (`false`), `Host`, `Port` (587), `EnableSsl`, `UserName`, `Password` (rahasia), `FromAddress`, `FromName`, `TimeoutSeconds` (10) |
| `Services/Notification/SmtpEmailSender.cs` | **Baru.** `SendAsync(EmailMessage)` → `Sent`/`Failed`/`Disabled`; `System.Net.Mail` bawaan, satu percobaan, 10 detik; kegagalan tidak dilempar |
| `Program.cs` | `using QuilvianSystemBackend.Services.Notification`; `Configure<TurnstileOptions>`, `AddHttpClient("Turnstile")`, `AddScoped<TurnstileVerificationService>`, `Configure<SmtpEmailOptions>`, `AddScoped<SmtpEmailSender>`; pemanggilan `WarnIfMisconfigured` sesudah `builder.Build()` |
| `appsettings.json` | Bagian `Turnstile` dan `Email:Smtp` dengan kunci rahasia **kosong** |

Delta terhadap desain: desain PO.7 hanya menyebut dua berkas layanan. Kelas opsi ditulis di berkas terpisah (`TurnstileOptions.cs`, `SmtpEmailOptions.cs`) mengikuti pola `KioskEncounterClosureOptions`. Tidak ada perubahan kontrak.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — belum ada endpoint yang memakai layanan ini |
| Database | `NOT APPLICABLE` — tanpa entity, tanpa migration |
| Keamanan/Auth | Panggilan HTTP keluar pertama ke `challenges.cloudflare.com` dan koneksi keluar ke server SMTP. Secret dan sandi hanya dari user-secrets/variabel lingkungan (`Turnstile__SecretKey`, `Email__Smtp__Password`). Log tidak memuat token, secret, IP, alamat email, isi email, maupun sandi (terbukti `L1`) |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat atau mengubah endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>` | Build succeeded, 0 error. 604 warning lama; **0** warning di berkas baru dan baris `Program.cs` yang diubah | `PASS` | Keluaran build |
| `appsettings.json` valid JSON | Valid | `PASS` | `json.load` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 5 berkas dievaluasi, VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| T1 kunci uji resmi Cloudflare "selalu lolos" | `Passed`, 0,16 s | `PASS` | Harness scratchpad (tidak di-commit) memanggil `siteverify` sungguhan |
| T2 kunci uji resmi "selalu gagal" | `Rejected`, 0,03 s; log mencatat kode galat `invalid-input-response` saja | `PASS` | Harness |
| T3 host tidak terjangkau (`10.255.255.1`) | `Unavailable`, 5,00 s | `PASS` | Harness |
| T4 host menolak koneksi | `Unavailable`, 2,04 s | `PASS` | Harness |
| T5 token kosong | `Rejected` tanpa panggilan keluar | `PASS` | Harness |
| T6 secret kosong | `Unavailable` (fail-closed) + log Error konfigurasi | `PASS` | Harness |
| T7 `Enabled = false` | `Passed` | `PASS` | Harness |
| T8 `FailOpen = true`, host menolak | `Passed` | `PASS` | Harness |
| T9 peringatan saat menyala bila `Enabled = false` | Log Warning "Turnstile dimatikan …" | `PASS` | Harness memanggil `WarnIfMisconfigured` |
| S1/S1b kirim ke server SMTP uji lokal | `Sent`, 0,04 s; server uji menerima penerima dan isi (UTF-8 base64) | `PASS` | Harness dengan server SMTP tiruan |
| S2 server SMTP menolak koneksi | `Failed`, 2,06 s, tanpa exception lolos | `PASS` | Harness |
| S3 host SMTP tidak terjangkau | `Failed`, 2,06 s | `PASS` | Harness |
| S6 server SMTP menerima koneksi tetapi diam | `Failed`, 10,01 s (batas waktu bekerja) | `PASS` | Harness |
| S4 `Enabled = false` | `Disabled` | `PASS` | Harness |
| S5 alamat pengirim tidak sah | `Failed` (`FormatException` tertangkap) | `PASS` | Harness |
| L1 log bersih | 10 baris log; tidak ada secret, token, IP, sandi, alamat, atau isi email | `PASS` | Harness memeriksa seluruh baris log |
| Runtime aplikasi penuh | Tidak dijalankan; belum ada endpoint yang memakai layanan ini | `NOT RUN` | Dibuktikan ulang di `BE-KSK-PO-004`/`005` |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kunci uji Cloudflare "selalu lolos" → lolos; "selalu gagal" → gagal | Terpenuhi | T1, T2 |
| (2) Host Turnstile tidak terjangkau → `Unavailable` dalam ≤ 5 detik | Terpenuhi | T3 (5,00 s), T4 |
| (3) Email terkirim ke SMTP uji; SMTP mati → gagal dalam ≤ 10 detik tanpa exception yang lolos | Terpenuhi | S1, S1b, S2, S3, S6 (10,01 s) |
| (4) Log tidak memuat secret, token, sandi, maupun alamat email | Terpenuhi | L1 |
| DoD: kriteria 1–4 terbukti | Terpenuhi | Bagian 5 |
| Tanpa paket NuGet baru | Terpenuhi | `QuilvianSystemBackend.csproj` tidak berubah |
| Rahasia tidak di berkas yang di-commit | Terpenuhi | `appsettings.json`: `SecretKey` dan `Password` kosong |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Dengan `appsettings.json` bawaan (`Turnstile:Enabled = true`, secret kosong), aplikasi mencatat peringatan saat menyala. Ini disengaja: pendaftaran online akan ditolak `503` sampai secret diisi. Untuk dev, isi kunci uji Cloudflare lewat user-secrets |
| Masalah yang diketahui | Bagian konfigurasi pendaftaran online pada desain disebut `Registration:OnlineRegistration`, padahal source menaruh `Registration` di bawah `HealthServices`. Jalur sebenarnya `HealthServices:Registration:OnlineRegistration`; dicatat untuk `BE-KSK-PO-001` |
| Risiko tersisa | Firewall server produksi harus mengizinkan HTTPS keluar ke `challenges.cloudflare.com` dan port SMTP (`KSK-PO-OQ-015`, `016`) |
| Perubahan sampingan | `NONE` — keluaran build dan harness berada di scratchpad, di luar repository |
| Interupsi | `NONE` |
| Status Git | `M Program.cs`, `M appsettings.json`, `?? Services/Notification/`, `?? Services/Security/TurnstileOptions.cs`, `?? Services/Security/TurnstileVerificationService.cs`, ditambah berkas dokumen kiosk (laporan ini, baris status roadmap dan traceability) |
| Langkah berikutnya | `FE-KSK-PO-004` (gelombang 1, frontend) dan `BE-KSK-PO-001` (skema; meminta konfirmasi sebelum membuat migration) |
