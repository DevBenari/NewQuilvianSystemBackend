# Laporan Perubahan Backend — `BE-EXT-04b`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-EXT-04b` — susulan atas `BE-EXT-04` |
| Judul | [Registrasi] Jalur tulis tujuan layanan pada sesi kiosk |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6c |
| Trace | `LAB-DEC-052`; wewenang `LAB-REQ-006` §1.1 |
| Kontrak | Dikontrakkan di sisi `registration-management`; bentuknya ditetapkan `LAB-REQ-006` §1.1 dan `02-backend-architecture.md` §12.6.1 |
| Dependency | `BE-EXT-04` ✅ `SELESAI` 2026-09-15 |
| Klasifikasi | `LOW` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source `RegistrationManagement`, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **✅ `SELESAI`** — kedua ruas `BE-EXT-04` kini dapat diisi. Nol migration, nol endpoint baru, nol baris uji tertinggal |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RegistrationManagement / Registration` |
| Pemilik dan prefix registry | Prefix **`Reg`**, lifecycle **`ACTIVE / LEGACY`** — baris 16 registry |
| Keberlakuan | `TOUCHED LEGACY` — tiga sisipan pada DTO dan controller yang sudah ada |
| Wewenang lintas modul | `LAB-REQ-006` §1.1, disetujui **Andry Zain** 2026-09-15 |
| Status gerbang | Tidak menahan — nol entity baru, nol kolom baru, nol migration |
| QBE ID yang berlaku | `QBE-VAL-001`, `QBE-DTO-001` |

---

## 1. Masalah yang diperbaiki

`BE-EXT-04` mendirikan dua kolom pada `TrxKioskScanSession` — `TargetService` dan
`HasPhysicianRequest` — beserta penyaring bacanya pada `GET /options`. **Tetapi jalur tulisnya
tidak ikut dibuka.**

`CreateKioskScanSessionRequest`, satu-satunya jalur tulis sesi kiosk yang ada, tidak memuat kedua
ruas itu. Akibatnya kolomnya berdiri tanpa ada satu pun cara mengisinya — dan buktinya terbaca
langsung pada data: **16 dari 16 sesi bernilai `null`**, termasuk sesi yang dibuat sesudah
kolomnya ada.

**Yang tertahan karenanya bukan hanya satu task:**

| Yang tertahan | Sebab |
| --- | --- |
| `BE-EXT-05` | Pemicu "pasien selesai di kiosk **bertujuan Laboratorium**" tidak akan pernah menyala |
| `FE-LAB-14` | Daftar sesi kiosk bertujuan Laboratorium akan selalu kosong, berapa pun sesi yang masuk |

Ini kelalaian pada task sebelumnya, bukan temuan pada pekerjaan orang lain — `BE-EXT-04`
dikerjakan sesi yang sama dengan laporan ini.

---

## 2. Perubahan yang dikerjakan

| Berkas | Sifat | Isi |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/DTOS/KioskScanSessionDtos.cs` | `Diperbarui` | Dua ruas opsional pada `CreateKioskScanSessionRequest` |
| `Areas/HealthServices/RegistrationManagement/Controllers/KioskScanSessionController.cs` | `Diperbarui` | Satu pemeriksaan nilai enum; dua penyalinan ke entity |

**Keduanya nullable dan tidak wajib.** Kiosk yang belum menanyakan tujuan layanan tetap mengirim
muatan lamanya dan tetap diterima — itu dibuktikan, bukan diandaikan (bagian 3).

`null` pada `HasPhysicianRequest` berarti **belum ditanyakan**, bukan "tidak membawa permintaan".
Ketiga keadaan disalin apa adanya, sejalan dengan dokumentasi kolomnya yang ditulis `BE-EXT-04`.

**Yang sengaja tidak dibangun:** jalur **ubah** sesi kiosk. Kedua ruas hanya dapat dikirim saat
sesi dibuat, karena itulah satu-satunya jalur tulis yang ada hari ini. Bila alur kiosk
sebenarnya menanyakan layanan **sesudah** kartu dipindai, diperlukan satu jalur ubah tersendiri —
keputusan milik `registration-management`, dicatat pada bagian 5.

---

## 3. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `Reg` `ACTIVE`, wewenang `LAB-REQ-006` §1.1 | `PASS` | Bagian preflight |
| `dotnet build -p:RunAnalyzers=False` | **0 Error, 207 Warning** — jumlah **sama persis** dengan sebelum perubahan | `PASS` | Nol warning baru |
| Tujuan layanan tersimpan | `TargetService=Laboratory` terbaca kembali dari database | `PASS` | 3.1 |
| Jalur permintaan dokter tersimpan | `HasPhysicianRequest=True` | `PASS` | 3.1 |
| Penyaring `BE-EXT-04` menemukan sesi | **1**, dari sebelumnya **selalu 0** | `PASS` | 3.1 |
| Nilai di luar daftar ditolak | **400**, nol sesi terbentuk | `PASS` | 3.1 |
| **Muatan lama tidak ikut diketatkan** | Diterima, kedua ruas tetap `null` | `PASS` | 3.1 |
| Kebersihan | 16/0 sebelum **dan** sesudah, nol baris uji tersisa | `PASS` | 3.2 |

### 3.1 Controller yang sebenarnya dipanggil, bukan logikanya ditiru

```
SEBELUM  sesi kiosk=16, bertujuan terisi=0

1. Kiosk menyatakan tujuan Laboratorium + membawa permintaan dokter
  [LULUS] sesi KSC-RSMMC-00017 -> TargetService=Laboratory
  [LULUS] HasPhysicianRequest=True
2. Penyaring tujuan Laboratorium — yang selama ini selalu kosong
  [LULUS] 1 sesi bertujuan Laboratorium (sebelum perubahan ini: 0)
3. Tujuan layanan di luar daftar
  [LULUS] status 400, sesi terbentuk=False
4. Muatan lama — tanpa kedua ruas baru sama sekali
  [LULUS] sesi KSC-RSMMC-00018 terbentuk, kedua ruas tetap null
```

`KioskScanSessionController.CreateFromScanResultForKiosk` dipanggil langsung dengan
`DefaultHttpContext`, sehingga validasi, penomoran sesi, transaksi, dan penyalinan ke entity
seluruhnya berjalan sebagaimana saat dipanggil lewat HTTP.

### 3.2 Kebersihan — dan kenapa caranya berbeda dari task sebelumnya

```
Pembersihan: 2 baris uji dihapus permanen.
SESUDAH  sesi kiosk=16, bertujuan terisi=0, sisa baris uji=0
```

**Controller ini membuka transaksinya sendiri**, sehingga membungkusnya dalam transaksi luar yang
di-`ROLLBACK` — cara yang dipakai `BE-LAB-27` dan `BE-LAB-28` — **gagal** dengan
`The connection is already in a transaction`. Dua baris uji karena itu benar-benar tersimpan,
lalu dihapus permanen pada blok `finally`, dan kebersihannya dibuktikan dengan hitungan ulang
dari koneksi baru. `LoggerService` diperiksa lebih dulu tidak memiliki `DbContext`, sehingga
pemanggilan ini tidak meninggalkan baris log di database.

---

## 4. Definition of Done

| Butir | Status | Bukti |
| --- | --- | --- |
| Kedua ruas `BE-EXT-04` dapat diisi lewat jalur tulis | ✅ | 3.1 |
| Penyaring baca `BE-EXT-04` terbukti menemukan sesi | ✅ | 3.1 |
| Nilai enum di luar daftar ditolak | ✅ | `400` |
| **Pemanggil lama tidak ikut diketatkan** | ✅ | 3.1 butir 4 |
| Nol migration | ✅ | Kolomnya sudah berdiri sejak `BE-EXT-04` |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari berkas task ini |
| Masalah yang diketahui | Tidak ada jalur **ubah** sesi kiosk — 5.1 |
| Risiko tersisa | Rendah. Aditif dan terbukti tidak mengubah perilaku pemanggil lama |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 2 berkas source, laporan ini, pembaruan roadmap, traceability, decisions, dan manifest. Tidak ada `git add`, `commit`, maupun `push` |

### 5.1 Urutan yang belum diketahui, dan kenapa itu bukan tebakan yang boleh diambil di sini

Kedua ruas hanya dapat dikirim **saat sesi dibuat**. Bila di lapangan pasien memilih layanan
**sesudah** kartunya dipindai, kiosk akan memerlukan satu jalur ubah — dan jalur itu tidak ada
sama sekali hari ini: `KioskScanSessionController` hanya punya **satu** `HttpPost`.

Yang menentukan mana yang benar adalah alur layar kiosk yang sebenarnya, milik
`registration-management`. Menebaknya berarti membangun endpoint yang mungkin tidak pernah
dipakai, atau lebih buruk, mengunci urutan pertanyaan di kiosk dari sisi backend.

### 5.2 `BE-EXT-05` masih tertahan, dan penahannya bukan lagi yang ini

Task ini membuka **satu** dari tiga penahan `BE-EXT-05`. Dua sisanya tidak tersentuh:

| Penahan | Isi |
| --- | --- |
| `LAB-OPEN-025` | Apakah prinsipal kiosk boleh membentuk kunjungan. Kiosk berjalan di bawah `KioskReadPolicy`; `EncounterIntakeService.RegisterAsync` menuntut `PatientEncounter:Create`. Bila tidak dipegang, setiap pasien kiosk gagal didaftarkan — dan gagalnya tidak terlihat sebagai galat sistem |
| `LAB-OPEN-026` | Unit layanan `SU-LAB-001 Laboratorium Klinik` ber-`IsAvailableForKiosk = false`. Perubahan data induk, wewenangnya terpisah |

Yang **sudah** ditetapkan: pukul penutupan, lewat `LAB-DEC-059` — 21:00 WIB, sebagai konfigurasi.
Ia ditetapkan pemilik modul Laboratorium dan **belum dikonfirmasi** terhadap jam operasional resmi
maupun terhadap pemilik `registration-management`.

### 5.3 Langkah berikutnya

1. **`FE-LAB-14`** — kini bermakna: daftar sesi kiosk bertujuan Laboratorium dapat berisi.
2. **`BE-EXT-05`** — menunggu `LAB-OPEN-025` dan `LAB-OPEN-026`.
