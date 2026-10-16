# Laporan Perubahan Backend — `BE-IGD-074`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-074` |
| Judul | Tanda tangan, pengiriman ke admisi, pembatalan sesudah kirim, cetak, dan penahanan *Jalankan* |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.18 gelombang 3 |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.18 |
| Requirement | `FR-IGD-112` (tandatangani), `113`, `114`, `115`, `116`; `AT-IGD-225`, `226`, `228`, `232`…`235` |
| Keputusan | `IGD-DEC-272`, `274`, `275`, `277`, `278`, `280`, `281`, `283`, `291`, `294` (pilihan desain 3), `300` (`POST`) |
| Contract version | API **`0.17.1`** §12.2 (`sign`, `print-data`, `cancel` sesudah kirim), §12.5; validation **`0.16.0`** §14 aturan 13–16, 18, 19, 23–25; state **`0.11.0`** §11.1, §11.2, §11.4; integration **`0.8.0`** §9.2–9.3; `02-backend-architecture.md` §17.7 |
| Dependency | `BE-IGD-072` 🟡 (commit `f6b07432`); `BE-IGD-073` 🟡 (source + migration 2 `20261013100736` dibuat Rizki, build 13 Oktober 2026 17.09 WIB) |
| Klasifikasi | `HIGH` — transaksi yang menulis ke dua modul; perubahan perilaku *Jalankan* yang memutus jalur pindah rawat inap tanpa layar |
| Task mode | `BACKEND` — izin pemilik 13 Oktober 2026: urutan *"kita kerjakan dulu BE-IGD-073 … saya akan jalankan semua build dan migration add"*, lalu *"saya sudah build dan add migration, selanjutnya apa"* |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): 5 berkas IGD pada 3.2; laporan ini, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `9e0e517f` (`rizkiG`) + working tree (`BE-IGD-073` belum di-commit) |
| Tanggal | 13–14 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — source dan kontrak lengkap.** Uji Putaran B2 (15 Oktober 2026): verifikasi aturan 409/404 (C-03, C-04, C-05, C-09, C-22, C-24), print-data (C-10), regresi Pulang/Poli (C-25, C-26), dan pembongkaran Kamar Pulih (C-28) ✅ LULUS. Langkah admisi C-12 terblokir otorisasi RBAC petugas admisi (`Doctor:Read`, `InpatientBedOccupancy:Read/Create/Update`); skenario P3–P5 NOT RUN (panduan §6.2). |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik dan prefix registry | IGD, prefix **`Emg`**, `ACTIVE / LEGACY`. Modul Rawat Inap (`Inp`, `ACTIVE`) hanya dipanggil lewat `InpAdmissionReferralService` — tidak ada berkas Rawat Inap yang disentuh task ini |
| Keberlakuan | `TOUCHED LEGACY` — kelima berkas sudah ada. Nol berkas baru |
| QBE ID yang berlaku | `QBE-NAM-001`, `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |
| `Program.cs` | Nol baris. Konstruktor `EmergencyDispositionService` menerima `InpAdmissionReferralService` (sudah `AddScoped`; dependensinya hanya `ApplicationDbContext`, `InpSettingService`, `LoggerService` — tanpa lingkaran) dan `ILogger<EmergencyDispositionService>` (disediakan framework) |
| Migration | **Tidak ada.** Seluruh kolom yang dipakai sudah dibuat migration 1 dan 2 |

---

## 1. Masalah yang diselesaikan

Sesudah `BE-IGD-072` dan `073`, form dapat diisi dan diminta tanda tangannya, dan Rawat Inap sudah siap menerima permintaan
dari IGD. Yang belum ada adalah jembatannya: dokter belum dapat menandatangani, permintaan admisi tidak pernah lahir,
perawat tidak dapat membatalkan form yang sudah terkirim, form tidak dapat dicetak, dan *Jalankan* tindak lanjut rawat inap
masih bisa ditekan tanpa form sama sekali.

Contoh. dr. Andi menandatangani form Tn. Budi pukul 10.15. Dalam satu transaksi form menjadi *Ditandatangani* dengan
salinan beku (alergi, diagnosis, penjamin), dan permintaan admisi `Pending` lahir di antrean Sari, petugas admisi. Bila Dimas
menekan *Jalankan* pukul 10.10, sistem menolak dengan *"Form Permintaan Rawat Inap belum ditandatangani DPJP."*

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi | Keadaan form | Keadaan permintaan admisi |
| ---: | --- | --- | --- | --- |
| 1 | DPJP IGD aktif | `POST /{id}/sign` dengan `Idempotency-Key`. Sebelum transaksi, server membaca pasien, alergi, diagnosis, penjamin, dan akun penanda tangan untuk salinan beku | *Menunggu TTD* | — |
| 2 | Sistem | Satu transaksi: kunci per kunjungan → periksa ulang → simpan *Signed* + salinan beku + atestasi (nama, jabatan, waktu, perangkat, IP) + kejadian `Signed` → `InpAdmissionReferralService.BuatDariIgdAsync` → commit | *Ditandatangani* (`displayStatus` `Sent`) | `Pending` |
| 3 | Perawat IGD | *Jalankan* tindak lanjut rawat inap kini diterima | — | `Pending` / `Completed` |
| 4a | Petugas admisi | Membuka admisi dari permintaan (alur `BE-IGD-073`) | `Completed` | `Completed` |
| 4b | Petugas admisi | Membatalkan permintaan | `CancelledByAdmission`; *Jalankan* tertahan lagi | `Cancelled` (`AdmissionOfficer`) |
| 4c | Perawat IGD | `POST /{id}/cancel` beralasan pada form terkirim | Tetap *Signed*; kejadian `CancelledAfterSend`; `displayStatus` `Cancelled` | `Cancelled` (`EmergencyNurse`) |
| 4d | Dokter | Membatalkan tindak lanjut `RANAP` beralasan | Form *Draf*/*Menunggu TTD* → *Cancelled*; form *Signed* tetap *Signed*; kejadian `AutoCancelled` | `Pending` → `Cancelled` (`EmergencyDispositionCancelled`) |
| 5 | Perawat IGD | `GET /{id}/print-data` | Isi cetak dari salinan beku | — |

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| Akun bukan DPJP IGD aktif saat ini menandatangani | `403` *"Hanya DPJP IGD aktif pasien ini yang dapat menandatangani atau mengembalikan form."* (aturan 13) |
| Form tidak *Menunggu TTD* | `409` *"Form ini tidak sedang menunggu tanda tangan."* (aturan 14) |
| Tindak lanjut asal tidak lagi *Confirmed* | `409` *"Tindak lanjut rawat inap sudah tidak berlaku. Form tidak dapat ditandatangani."* (aturan 15) |
| Bacaan modul lain gagal saat membentuk salinan beku | `503` *"Data pasien tidak dapat dibaca saat ini. Coba tandatangani lagi."*; form tetap *Menunggu TTD*; sebab dicatat `LogWarning` tanpa data pasien (aturan 16) |
| Pasien sudah punya permintaan admisi `Pending` lain saat ditandatangani | `409` *"Pasien sudah punya permintaan admisi yang menunggu petugas admisi."*; tidak ada yang tersimpan |
| Klik *Tandatangani* dua kali dengan `Idempotency-Key` | `200` dengan keadaan form yang sama; tetap satu permintaan admisi (aturan 18, delta 1) |
| Batal form terkirim sesudah admisi dibuka | `409` *"Admisi rawat inap sudah dibuka dari permintaan ini. Form tidak dapat dibatalkan dari IGD."* (aturan 19) |
| Batal form terkirim yang permintaannya sudah batal | `409` *"Form ini sudah dibatalkan."* (aturan 19) |
| *Jalankan* tanpa form terkirim | `409` *"Form Permintaan Rawat Inap belum ditandatangani DPJP."* (aturan 23) |
| *Jalankan* sesudah petugas admisi membatalkan | `409` *"Permintaan admisi dibatalkan petugas admisi. Ajukan Form Permintaan Rawat Inap baru."* (aturan 23) |
| Batalkan tindak lanjut `RANAP` yang admisinya sudah dibuka | `409` *"Admisi rawat inap sudah dibuka dari tindak lanjut ini. Batalkan admisi di Rawat Inap lebih dulu."* (aturan 24) |
| Cetak form yang belum ditandatangani | `409` *"Form Permintaan Rawat Inap hanya dapat dicetak sesudah ditandatangani."* (delta 3) |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-074`; `02-backend-architecture.md` §17; API §12.1–12.6; validation §14; state §11; integration §9; kamus data
§10 (`erd/data-dictionary.md`); `03-frontend-architecture.md` §17.4–17.6 (isi cetak, keadaan tampil). Source:
`EmergencyDispositionService.cs`, `EmergencyDispositionService.AdmissionRequest.cs`, `EmergencyAdmissionRequestController.cs`,
`EmergencyAdmissionRequestDtos.cs`, `EmergencyDispositionController.cs` (`UpdateDispositionStatus` `:311`),
`EmgAdmissionRequest.cs`, `EmgAdmissionRequestConfiguration.cs` (`RowVersion` sebagai token konkurensi, `SnapshotJson`
`jsonb`), `InpAdmissionReferralService.cs` (`BuatDariIgdAsync`, `BatalkanDariIgdAsync`, `AmbilKeadaanUntukIgdAsync`),
`InpatientAdmissionReferralDtos.cs`, `InpAdmissionSignatureService.AttestAsync` (pola atestasi),
`InpAdmissionSourceReader.GetUsersAsync` (nama dan jabatan akun), `DoctorConsultationController` `:1146` (perangkat dan IP
dibaca server), `EmergencyVisitService` `:1255` (zona waktu WIB), `Program.cs` `:674`, `:688`, `:733` (registrasi DI, dibaca
saja).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.cs` | Konstruktor menerima `InpAdmissionReferralService` dan `ILogger<EmergencyDispositionService>` (+9/−1) |
| `…/Services/EmergencyDispositionService.AdmissionRequest.cs` | `TandatanganiPermintaanAsync`, `AmbilDataCetakPermintaanAsync`, `ButuhFormPermintaanRawatInapAsync`, `PeriksaPermintaanSebelumJalankanAsync`, `BatalkanPermintaanKarenaTindakLanjutAsync`; batal sesudah kirim (`BatalkanPermintaanTerkirimAsync`); salinan beku (`BentukSalinanBekuAsync`); `displayStatus` dari permintaan admisi; aturan 3 kini juga menghitung form terkirim yang permintaannya `Pending`; kalimat tanda tangan cetak berjam WIB (+534/−16) |
| `…/DTOs/EmergencyAdmissionRequestDtos.cs` | `EmergencyAdmissionRequestReferralInfo`; ruas `admissionReferral` pada respons; `EmergencyAdmissionRequestPrintResponse` (+39) |
| `…/Controllers/EmergencyAdmissionRequestController.cs` | `POST /{id}/sign`, `GET /{id}/print-data`; log membawa `AdmissionReferralId` (+48) |
| `…/Controllers/EmergencyDispositionController.cs` | `UpdateDispositionStatus`: *Executed* jenis `RequiresAdmissionRequest` tertahan (aturan 23); *Cancelled* membuka transaksi, membatalkan form dan permintaan (aturan 25) atau menolak (aturan 24) (+25) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: `POST /{id}/sign`, `GET /{id}/print-data`, ruas `admissionReferral`. Memutus sesuai kontrak §12.1 nomor 2: *Jalankan* tindak lanjut rawat inap kini `409` tanpa form terkirim; *Batalkan* tindak lanjut rawat inap dapat `409` bila admisi sudah dibuka. `POST /{id}/cancel` kini menerima form *Signed* (sebelumnya selalu `409`) |
| Database | Nol migration, nol kueri manual. Tulisan baru ke `EmgAdmissionRequest`, `EmgAdmissionRequestEvent`, dan — lewat service Rawat Inap — `InpAdmissionReferral`, seluruhnya dalam satu transaksi per perintah |
| Keamanan/Auth | Tidak ada butir hak akses baru: `sign` memakai `EmergencyAdmissionRequest : Sign`, `print-data` memakai `: Read` (keduanya sudah ada sejak `BE-IGD-072`). Kewenangan penanda tangan diperiksa dari data (akun → dokter = DPJP IGD aktif), bukan nama peran. Perangkat dan IP dibaca server dari `User-Agent` dan alamat koneksi, bukan kiriman klien. Isi klinis tidak masuk log |

### 3.4 Delta terhadap kontrak yang disetujui

| No | Kontrak | Implementasi | Alasan |
| ---: | --- | --- | --- |
| 1 | Aturan 18: kiriman ulang dengan `Idempotency-Key` **yang sama** mengembalikan hasil pertama | Kunci **tidak disimpan**. Pengulangan dikenali dari keadaan: form sudah *Signed* oleh akun yang sama dan header `Idempotency-Key` terisi → `200` dengan keadaan form saat ini, tanpa permintaan kedua. Tanpa header → `409` aturan 14. Kunci berbeda dari akun yang sama juga dianggap pengulangan | Kamus data §10 tidak punya kolom kunci; menambahkannya berarti migration ketiga. Kunci per kunjungan + index unik `UX_InpAdmissionReferral_EmergencyRequest` tetap menjamin satu permintaan per form |
| 2 | Kartu: konstruktor menerima `InpAdmissionReferralService` | Juga `ILogger<EmergencyDispositionService>` | Mencatat sebab `503` aturan 16 tanpa data pasien; disediakan framework, tanpa `Program.cs` |
| 3 | `print-data` hanya untuk form *Signed* — kode dan pesan tidak ditulis | `409` *"Form Permintaan Rawat Inap hanya dapat dicetak sesudah ditandatangani."* | Permukaan teknis |
| 4 | — | Form *Signed* tanpa baris permintaan admisi (anomali data) → batal `409` *"Permintaan admisi untuk form ini tidak ditemukan."* | Tidak dapat terjadi lewat API karena keduanya lahir dalam satu transaksi; gagal tertutup |
| 5 | Atestasi: *"Nama tampilan akun saat TTD"* | Nama tampilan akun; bila kosong, nama dokter DPJP | Kalimat cetak tidak boleh tanpa nama |
| 6 | Aturan 3 (form aktif per kunjungan) | Sejak task ini juga menolak bila kunjungan punya form *Signed* yang permintaannya `Pending` | Kalimat aturan 3 memang mencakup "terkirim dengan permintaan menunggu"; sebelum `BE-IGD-074` form *Signed* belum dapat lahir |
| 7 | Respons §12.3 | Selain `admissionReferral`, `cancelledAt`/`cancellationReason` respons dan `cancellationReason` daftar per kunjungan diisi dari permintaan admisi bila form *Signed* dan permintaannya dibatalkan | Layar menampilkan *"Dibatalkan — {alasan}"* untuk keempat jalan batal (`03-frontend-architecture.md` §17.5) tanpa membaca dua tempat |
| 8 | Aturan 23 punya dua kalimat | Kalimat kedua dipakai bila form *Signed* terakhir dibatalkan petugas admisi; selain itu kalimat pertama (termasuk form yang dibatalkan perawat) | Kontrak tidak merinci pemilihan kalimat |
| 9 | Urutan `404 → 403 → 400 → 409` | `sign`: pengulangan diperiksa sesudah `404` dan sebelum `403`, supaya dokter yang DPJP-nya baru dialihkan tetap menerima hasil pertamanya. Pemeriksaan isian wajib (aturan 11) dijalankan ulang secara defensif **sesudah** `409` status | Form *Menunggu TTD* selalu lolos aturan 11 saat *Minta TTD*; pemeriksaan ulang hanya pengaman |
| 10 | Batal sesudah kirim | Form tetap *Signed*, `RowVersion` form berganti, kejadian `CancelledAfterSend` membawa alasan; alasan juga tersimpan di permintaan admisi | Kamus data: kolom batal form hanya untuk pembatalan sebelum terkirim |
| 11 | Pembatalan tindak lanjut di transaksi `UpdateDispositionStatus` | Transaksi eksplisit dibuka **hanya** untuk jenis `RequiresAdmissionRequest`; jenis lain berjalan seperti sebelumnya dengan satu `SaveChanges` | Menjaga perilaku jenis tindak lanjut lain tidak berubah |

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Admission Request

Base URL: `/api/v1/health-services/emergency-installation-management/emergency-admission-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/sign` | **Baru** — DPJP IGD aktif menandatangani; permintaan admisi lahir | `EmergencyAdmissionRequest : Sign` | `{ "rowVersion": "…" }`; header `Idempotency-Key` | `ApiResponse<EmergencyAdmissionRequestResponse>` | `200`, `403`, `404`, `409`, `503` |
| `GET` | `/{id}/print-data` | **Baru** — isi cetak dari salinan beku | `EmergencyAdmissionRequest : Read` | — | `ApiResponse<EmergencyAdmissionRequestPrintResponse>` | `200`, `404`, `409` |
| `POST` | `/{id}/cancel` | **Diperluas** — kini juga form terkirim selama permintaan `Pending` | `EmergencyAdmissionRequest : Cancel` | `{ "reason": "…", "rowVersion": "…" }` | `ApiResponse<EmergencyAdmissionRequestResponse>` | `200`, `400`, `404`, `409` |
| `GET` | `/?emergencyVisitId=` dan `/{id}` | **Diperluas** — `displayStatus` dari permintaan admisi; ruas `admissionReferral` | `EmergencyAdmissionRequest : Read` | — | idem | `200` |

Contoh respons `sign` (dipangkas):

```json
{
  "id": "5d1e…",
  "status": 3,
  "displayStatus": 3,
  "signer": { "doctorName": "dr. Andi Pratama", "positionName": "Dokter IGD", "signedAt": "2026-10-14T03:15:00Z" },
  "admissionReferral": { "id": "9a7c…", "status": 1, "isCito": true, "cancellationSource": null, "completedEpisodeId": null },
  "rowVersion": "…"
}
```

Enum dikirim sebagai angka (konvensi repo): `status` 3 = `Signed`; `displayStatus` 3 = `Sent`, 4 = `Completed`,
5 = `Cancelled`, 6 = `CancelledByAdmission`; `admissionReferral.status` 1 = `Pending`, 2 = `Completed`, 3 = `Cancelled`;
`cancellationSource` 2 = `EmergencyNurse`, 3 = `EmergencyDispositionCancelled`, 4 = `AdmissionOfficer`.

`EmergencyAdmissionRequestPrintResponse`: `id`, `emergencyVisitId`, `origin`, `emergencyVisitNumber`, `encounterNumber`,
`capturedAt`, `patient`, `allergies`, `payer`, `diagnoses`, `requestedServiceUnitName`, `requestedPatientClassName`,
`proposedInpatientDoctorName`, `estimatedLengthOfStayDays`, `isCito`, `specialNeeds[]`, `clinicalIndication`,
`carePlanInstruction`, `signer`, `signatureStatement`, `printedAt`. Contoh `signatureStatement`: *"Ditandatangani secara
elektronik oleh dr. Andi Pratama, 14 Oktober 2026 10.15 WIB"*.

#### Health Services / Emergency Installation Management / Emergency Disposition

`PATCH /api/v1/health-services/emergency-installation-management/emergency-dispositions/{id}/disposition-status` —
`EmergencyDisposition : Update`, sudah ada; perilaku baru hanya untuk jenis tindak lanjut bertanda `RequiresAdmissionRequest`:

| Permintaan | Perilaku |
| --- | --- |
| `dispositionStatus = Executed` | Diterima hanya bila ada form *Signed* untuk tindak lanjut ini dengan permintaan `Pending` atau `Completed`; selain itu `409` aturan 23 |
| `dispositionStatus = Cancelled` | Dalam satu transaksi: form *Draf*/*Menunggu TTD* ikut *Cancelled*, permintaan `Pending` ikut dibatalkan dengan alasan yang sama; `409` aturan 24 bila admisi sudah dibuka |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker `-Mode Strict` atas 5 berkas | `VIOLATION 0`, `REVIEW 0`, `INFO 0`, *Final result: PASS* | `PASS` | 14 Oktober 2026 |
| Akhiran baris (hitungan byte Node) | 5 berkas CRLF, tanpa BOM, nol LF tunggal | `PASS` | Skrip hitung byte |
| Komentar baru | Nol `//`, `///`, `/* */` pada baris tambahan `git diff -U0` | `PASS` | `grep` |
| `Program.cs` | Tidak berubah | `PASS` | `git diff --quiet -- Program.cs` |
| Tabrakan nama tipe dari `using` Rawat Inap | Identifier di kedua berkas service yang juga ada di namespace `InPatientManagement.Services`/`DTOs` hanya `EmergencyAdmissionReferralCreate`, `EmergencyAdmissionReferralState`, `InpAdmissionReferralService` (memang dimaksud); nol tabrakan dengan namespace lain yang diimpor | `PASS` | `grep` + `comm` |
| Lingkaran DI | `InpAdmissionReferralService` → `ApplicationDbContext`, `InpSettingService` (→ `ApplicationDbContext`, `ILogger`), `LoggerService`; tidak satu pun bergantung pada service IGD | `PASS` | Pembacaan konstruktor |
| Pemanggil konstruktor manual `new EmergencyDispositionService(` | Nihil | `PASS` | `grep` |
| `dotnet build -p:RunAnalyzers=false` (Rizki, 14 Oktober 2026) | `Build succeeded`, 0 error; 10 warning = 5 peringatan NuGet `NU1902`/`NU1903` (kerentanan paket `SixLabors.ImageSharp` 3.1.11) yang tercetak dua kali (restore dan build); tangkapan layar build inkremental 1,5 detik sehingga warning kompilator (`CS`) dari kompilasi DLL 09.00.11 tidak tampil. DLL `bin/` dan `obj/` 14 Oktober 2026 09.00.11 WIB — sesudah seluruh berkas source dan kedua penjaga `Down()` (08.55.20); literal baru task ditemukan di DLL | `PASS` (0 error), warning dilaporkan | Tangkapan layar pemilik + cap waktu DLL + pencarian literal UTF-16 di DLL |
| Uji API acceptance 1–6 | Belum | `NOT RUN` | Putaran B, skenario 074-S1…S13 di bagian 8 |

Uji manual: `REQUIRED` — putaran B bersama `FE-IGD-060`, `061`, `062` dan `BE-IGD-075`.

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Tandatangani hanya oleh akun dokter yang menjadi DPJP IGD aktif saat itu; atestasi lengkap; salinan beku dari bacaan sebelum transaksi (aturan 13–16) | Source terpenuhi; uji belum | `TandatanganiPermintaanAsync`, `BentukSalinanBekuAsync` |
| 2 | Tanda tangan dan permintaan admisi dalam satu transaksi; gagal salah satu = tidak berubah; idempoten (aturan 18) | Source terpenuhi (delta 1); uji belum | Transaksi + kunci per kunjungan; `BuatDariIgdAsync` di dalamnya; penolakan dan `DbUpdateException` mengosongkan pelacak dan membatalkan transaksi |
| 3 | *Executed* tertahan sesuai aturan 23, termasuk sesudah permintaan dibatalkan petugas admisi | Source terpenuhi; uji belum | `PeriksaPermintaanSebelumJalankanAsync` dipanggil `UpdateDispositionStatus` sebelum status kunjungan diubah |
| 4 | Batal sesudah kirim dan batal karena tindak lanjut batal; ditolak bila admisi sudah dibuka (aturan 19, 24, 25) | Source terpenuhi; uji belum | `BatalkanPermintaanTerkirimAsync`, `BatalkanPermintaanKarenaTindakLanjutAsync` |
| 5 | `displayStatus` mengikuti state §11.2 | Source terpenuhi; uji belum | `TampilanStatus` + `TampilanStatusTerkirim` dari `AmbilKeadaanUntukIgdAsync` |
| 6 | Data cetak hanya untuk form *Signed*, dari salinan beku, memuat baris tanda tangan elektronik | Source terpenuhi; uji belum | `AmbilDataCetakPermintaanAsync`, `KalimatTandaTangan` |
| 7 | Diff hanya berkas Cakupan; nol `Program.cs`; nol komentar baru; akhiran baris benar | Terpenuhi | Bagian 5 |
| 8 | Build 0 error; jumlah warning dilaporkan | Terpenuhi — 0 error, 10 warning (5 peringatan paket NuGet tercetak dua kali) | Bagian 5 |

DoD: laporan ini ✅; acceptance 1–6 terpetakan ke source; 7 dan 8 terpenuhi; uji API pada putaran B; rilis
sesuai R3.18.5.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan rilis | Begitu build ini berjalan di dev, **tombol *Jalankan* tindak lanjut rawat inap di layar perawat akan `409`** sampai form ditandatangani — dan layar *Tandatangani* (`FE-IGD-060`) belum ada. Jangan merilis ke lingkungan pengguna tanpa `FE-IGD-059`…`061` (R3.18.5) |
| Peringatan Billing | `BE-IGD-075` belum dikerjakan. Bila petugas admisi membuka admisi dari permintaan bersumber IGD sebelum `075`, tagihan IGD ikut tertaut ke invoice rawat inap (`IGD-DEC-293`). Skenario 074-S11 dijalankan sesudah `075` |
| Masalah yang diketahui | Aturan 15 dijaga, tetapi lewat API biasa praktis tidak tercapai: membatalkan tindak lanjut ikut membatalkan form yang menunggu TTD (aturan 25), sehingga penanda tangan menerima aturan 14 |
| Risiko tersisa | Tinggi (keselamatan alur kerja) — penahanan *Jalankan*; transaksi lintas modul. Regresi jenis tindak lanjut lain wajib diuji (074-S13) |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu pertanyaan pemilik di tengah task (cara menempel penjaga `Down()`); dijawab, pekerjaan dilanjutkan dari keadaan terverifikasi |
| Status Git | 5 `M` di `Areas/HealthServices/EmergencyInstallationManagement/**`; perubahan `BE-IGD-073` dan migration 2 dari sesi sebelumnya tetap di working tree; nol stage, nol commit |
| Langkah berikutnya | Rizki: build ulang backend (`dotnet build -p:RunAnalyzers=false`) lalu jalankan [panduan putaran B baru](../../../testing/2026-10-14-panduan-uji-putaran-b2-antrean-transfer-igd.md). Konfirmasi Hamzah tercatat `IGD-DEC-307` |

## 8. Skenario uji putaran B

Data: kunjungan IGD berjalan dengan DPJP aktif `ranger.biru@admin.com`; tindak lanjut `RANAP` terkonfirmasi; form
*Menunggu TTD* (alur putaran A). Akun perawat `dimas.kurniawan`. Satu akun petugas admisi Rawat Inap berizin
`InpatientAdmissionReferral : Read`/`Cancel`.

| ID | Langkah | Hasil yang diharapkan | AT |
| --- | --- | --- | --- |
| 074-S1 | Dokter `POST /{id}/sign` dengan `rowVersion` terkini dan `Idempotency-Key` K1 | `200`; `status` 3, `displayStatus` 3, `signer` terisi, `admissionReferral.status` 1; `GET /admission-referrals?sourceKind=2` memuat barisnya, CITO di atas | `AT-IGD-225` |
| 074-S2 | Ulangi S1 dengan K1 dan `rowVersion` lama | `200`, `admissionReferral.id` sama; jumlah permintaan admisi pasien tetap satu | `AT-IGD-235` |
| 074-S3 | Akun lain (perawat berizin `Sign`, atau dokter bukan DPJP aktif) `POST /{id}/sign` | `403` aturan 13 | `AT-IGD-226` |
| 074-S4 | `POST /{id}/sign` pada form *Draf* | `409` aturan 14 | — |
| 074-S5 | Perawat `PATCH …/disposition-status` *Executed* sebelum S1 | `409` *"Form Permintaan Rawat Inap belum ditandatangani DPJP."*; status kunjungan tidak berubah | `AT-IGD-228` |
| 074-S6 | Sesudah S1, *Executed* | `200` | `AT-IGD-228` |
| 074-S7 | Pada form kedua yang sudah dikirim: petugas admisi `POST /admission-referrals/{id}/cancel`, lalu perawat *Executed* | `409` *"Permintaan admisi dibatalkan petugas admisi. Ajukan Form Permintaan Rawat Inap baru."*; `displayStatus` 6 | `AT-IGD-231` |
| 074-S8 | Perawat `POST /{id}/cancel` beralasan pada form terkirim (`Pending`) | `200`; `admissionReferral.status` 3, `cancellationSource` 2; `displayStatus` 5; ulangi → `409` *"Form ini sudah dibatalkan."* | `AT-IGD-232` |
| 074-S9 | Dokter membatalkan tindak lanjut `RANAP` (alasan wajib) saat form terkirim `Pending` | `200`; permintaan `Cancelled` dengan `cancellationSource` 3; kejadian `AutoCancelled` (7) di form | `AT-IGD-233` |
| 074-S10 | Seperti S9 tetapi form masih *Menunggu TTD* | `200`; form `status` 4 dengan alasan yang sama | `AT-IGD-233` |
| 074-S11 | Sesudah `BE-IGD-075`: petugas admisi membuka admisi dari permintaan; lalu batalkan form dan batalkan tindak lanjut | `displayStatus` 4; batal form `409` aturan 19; batal tindak lanjut `409` aturan 24; *Executed* `200` | `AT-IGD-232`, `233` |
| 074-S12 | `GET /{id}/print-data` pada form terkirim dan pada form *Draf* | `200` dengan `signatureStatement` berjam WIB; *Draf* → `409` | `AT-IGD-234` |
| 074-S13 | Regresi: tindak lanjut jenis lain (mis. `PLG`) *Executed* dan *Cancelled* | Perilaku sama seperti sebelum task | — |

## Pengerjaan ulang — 14 Oktober 2026 (`IGD-DEC-304`)

Form bertanda tangan kini menjadi surat pengantar ke Antrean Transfer IGD/Poli. Perubahan terhadap versi sebelumnya:

| Bagian | Sebelum | Sesudah |
| --- | --- | --- |
| Tanda tangan `POST /emergency-admission-requests/{id}/sign` | Menyimpan form *Signed* dan membuat `InpAdmissionReferral` dalam satu transaksi | Hanya menyimpan form *Signed* dan salinan beku; ditolak `409` bila tindak lanjut bukan `RANAP` atau unit tujuannya bukan unit rawat inap |
| Masuk antrean | Mode *Admisi dari Kamar Pulih* | `InpAdmissionTransferService.TransferQuery` memuat tindak lanjut yang jenisnya bertanda `RequiresAdmissionRequest` hanya bila punya form *Signed* (+3 baris, berkas milik Rawat Inap) |
| Status *Terkirim*/*Selesai* | Dibaca dari status `InpAdmissionReferral` | *Selesai* bila ada kunjungan dengan `AdmissionTransferId` = tindak lanjut dan sumber `Emergency` (sama dengan penjaga *admit* Transfer) |
| Batal sesudah kirim | Membatalkan `InpAdmissionReferral`; form tetap *Signed* | Form menjadi *Cancelled* (kejadian `CancelledAfterSend`); pasien keluar dari antrean; ditolak bila admisi sudah diproses. Mengunci baris `EmgDisposition` seperti *admit* Transfer |
| Batal otomatis karena tindak lanjut dibatalkan | Membatalkan permintaan `Pending` | Semua form yang belum batal menjadi *Cancelled*; ditolak bila admisi sudah diproses |
| *Jalankan* tindak lanjut rawat inap | Diizinkan sesudah form terkirim | Selalu `409`: tanpa form terkirim → minta lengkapi form; dengan form terkirim → dijalankan otomatis oleh petugas admisi |
| Ruas respons `admissionReferral` | Ada | Dibuang beserta `AdmissionReferralId` di log |

**Berkas:** `EmergencyDispositionService.AdmissionRequest.cs`, `EmergencyDispositionService.cs` (injeksi `InpAdmissionReferralService` dibuang), `EmergencyAdmissionRequestDtos.cs`, `EmergencyAdmissionRequestController.cs`, `EmergencyDispositionController.cs`, `InpAdmissionTransferService.Queries.cs`.

**Validasi:** QBE `-Mode Strict` 6 berkas — `PASS`, 0 violation, 0 review; nol komentar baru; keenam berkas CRLF tanpa BOM. Build Rizki 14 Oktober 2026 17.17 (`dotnet build -p:RunAnalyzers=false`): DLL baru lebih muda dari suntingan terakhir; tiga pesan desain baru ditemukan sebagai literal, sedangkan migration 2, `BuatDariIgdAsync`, `EmergencyAdmissionReferralState`, dan pesan sukses TTD lama tidak ada lagi. Jumlah warning tidak terlihat oleh agent pengembang.

**Titik sentuh Rawat Inap:** saringan di `TransferQuery` dipakai daftar, rincian, dan *admit* Transfer. Pasien IGD tanpa form *Signed* tidak tampil dan tidak dapat di-*admit* (`404` *"Permintaan rawat inap yang sah tidak ditemukan."*). Jenis tindak lanjut tanpa tanda `RequiresAdmissionRequest` dan sumber Poliklinik tidak berubah. Butuh konfirmasi Muhammad Hamzah sebelum rilis.

**Temuan untuk Rizki dan Hamzah:** `IGD-OQ-130` (kunjungan IGD tidak ikut tertutup saat *admit* Transfer) dan `IGD-OQ-131` (antrean belum membaca isi form dan belum punya aksi tolak).

## Perubahan sesudah build 17.17 — pengenal jenis lewat tanda (`IGD-DEC-305`), 14 Oktober 2026 malam

**Temuan.** Di basis data dev, jenis tindak lanjut rawat inap berkode `RI` (`IGD-FACT-122`). Antrean Transfer hanya membaca kode
`RANAP`, dan syarat pembuatan form yang ditambahkan agent pada pengerjaan ulang juga mensyaratkan `RANAP`. Akibatnya form tidak
dapat dibuat di dev dan pasien IGD tidak pernah tampil di antrean.

**Perubahan (pilihan Rizki *"Pakai tanda form (Recommended)"*):**

| Berkas | Sebelum | Sesudah |
| --- | --- | --- |
| `InpAdmissionTransferService.Queries.cs` (Rawat Inap) | Kode `RANAP` **dan** (tanpa tanda **atau** ada form *Signed*) | Bertanda → wajib ada form *Signed* (kode apa pun); tanpa tanda → kode `RANAP` seperti `HEAD` |
| `EmergencyDispositionService.AdmissionRequest.cs` | `TindakLanjutMasukAntreanAsync` mensyaratkan kode `RANAP` | Mensyaratkan tanda `RequiresAdmissionRequest`, jenis aktif, dan unit tujuan bertipe rawat inap; konstanta kode dihapus; pesan aturan 27 tidak lagi menyebut kode |

**Validasi:** QBE `-Mode Strict` 6 berkas — `PASS`, 0 violation, 0 review; nol komentar baru; CRLF tanpa BOM. **Build belum** —
perubahan ini terjadi sesudah build 17.17.

**Kartu direncanakan ulang** (`IGD-DEC-306`): acceptance kini 1–10 mengikuti amandemen desain 0q; dependency tinggal `BE-IGD-072`.
