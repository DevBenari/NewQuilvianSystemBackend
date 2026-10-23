# Laporan Perubahan Backend — `BE-IGD-085`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-085` |
| Judul | _Check-in_ reservasi memakai encounter IGD dari loket (perbaikan `BE-IGD-080` acceptance 7) |
| Slice | `EPIC IGD-16` · R3.19 — perbaikan temuan A audit 18 Oktober 2026 |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.19, kartu `BE-IGD-085` |
| Requirement | `FR-IGD-122` (`AT-IGD-242`); `IGD-DEC-314` (C) sebagaimana disempitkan `IGD-DEC-325` |
| Keputusan | `IGD-DEC-325`, `IGD-DEC-314`, `IGD-DEC-315`; fakta `IGD-FACT-137`, `138`, `144` |
| Contract version | API **`0.20.0`** §13.2; validation **`0.20.0`** §15.1 aturan 1–6; integration **`0.11.0`** §10.2 — `approved` `IGD-DEC-325`, manifest bagian 0t |
| Dependency | — (kemampuan yang dipakai sudah ✅: `BE-IGD-053`, `BE-IGD-055`) |
| Klasifikasi | `LIGHT`–`MEDIUM` — satu method di satu berkas service; nol schema, nol endpoint, nol DTO; mengubah perilaku satu aksi yang terlihat petugas |
| Task mode | `BACKEND` — atas perintah pemilik _"kerjakan point 1 dan 2"_ (Rizki Gunawan, 18 Oktober 2026 malam) |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyPreArrivalReservationService.cs`, laporan ini, baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb18fe6` (`rizkiG`) + working tree `BE-IGD-083` (lima berkas `.cs`, belum di-commit) dan dokumen blueprint 18 Oktober 2026 |
| Tanggal | 18 Oktober 2026 |
| Status | ✅ **SELESAI — 19 Oktober 2026 pagi: 8 dari 8 acceptance.** #8 terpenuhi oleh build penuh Rizki di PC kantor 19 Oktober 2026 (`--no-incremental`, `rizkiG` `4cf5c1fd`): 0 error, 285 warning (5 NuGet `NU1902`/`NU1903` `SixLabors.ImageSharp`, 280 kompilator; nol dari berkas task pada daftar yang terbaca) — bagian 5. *Sebelumnya:* 🟡 **SEBAGIAN — 18 Oktober 2026 malam, putaran P1 disahkan sebatas bukti log (`IGD-DEC-328`): 7 dari 8 acceptance; tinggal jumlah warning build.** *Sebelumnya:* 🟡 **SEBAGIAN — implementasi selesai, QBE Strict `PASS`; build Rizki dan putaran P1 belum.** Acceptance 3, 5, 7 terpenuhi; 6 sebagian (diff); 1, 2, 4 menunggu P1; 8 menunggu build |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik / prefix registry | `Emg` — *Emergency*, `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `TOUCHED LEGACY` — service yang sudah ada disunting di satu method; tidak ada entity, configuration, maupun controller baru |
| Entity / migration baru | Tidak ada — QBE-MOD-002/003, QBE-ENT-001, QBE-CFG-001 tidak terpicu |
| QBE ID yang berlaku | QBE-SVC-001 (aturan di service), QBE-API-001 (pola `Hasil` → `ApiResponse` dan kode `400`/`404`/`409` yang sudah ada), QBE-VAL-001 (encounter loket wajib, waktu tiba), QBE-TXN-001 (kunjungan dan reservasi dalam satu transaksi di bawah kunci episode pasien), QBE-LOG-001 (log controller yang sudah ada tetap mencatat aktor, kunjungan, dan nomor) |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/TASK_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `TEST_POLICY.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry repo |
| Sisa folder yang dicabut | Tidak ditemukan `agents/rules/` maupun `.codex/` |

---

## 1. Masalah yang diperbaiki

_Check-in_ reservasi pra-kedatangan (`BE-IGD-080`) melahirkan kunjungan IGD **tanpa encounter dan tanpa penjamin**
(`IGD-FACT-137`). Kunjungan seperti itu tidak dapat diberi diagnosis terhitung, tindak lanjutnya tidak dapat dikonfirmasi,
Form Permintaan Rawat Inap ditolak, tidak ada tagihan, dan loket menolak mendaftarkan pasien itu lagi. Penjaganya juga
hanya menahan kunjungan berjalan, sehingga pasien yang sudah didaftarkan loket dan masih menunggu triage mendapat kunjungan
kedua (`IGD-FACT-138`).

_Contoh sebelum perbaikan._ Tn. Hadi didaftarkan loket pukul 09.38 (encounter asuransi). Pukul 09.40 admisi menekan
_Konfirmasi Tiba_ pada reservasinya → lahir kunjungan **baru** tanpa encounter, sementara encounter loket tetap menunggu
triage. Dokter yang membuka kunjungan hasil _check-in_ tidak dapat membuat form rawat inap.

## 2. Proses bisnis

Sesudah perbaikan (`IGD-DEC-325`):

1. Keluarga menelepon; admisi mencatat reservasi (tidak berubah).
2. Pasien tiba. GRO mengantar keluarga ke **loket IGD**; petugas loket mendaftarkan pasien beserta penjaminnya — encounter
   `Emergency` lahir dan pasien masuk daftar _menunggu triage_.
3. Admisi menekan _Konfirmasi Tiba_ pada reservasi. Server:
   - mengunci episode pasien (kunci yang sama dengan _Mulai Triage_), lalu mencari episode terbuka;
   - bila pasien punya **kunjungan** IGD berjalan → `409` dengan pesan yang ada (_"Pasien ini masih memiliki kunjungan IGD
     aktif bernomor …"_);
   - bila pasien **tidak** punya encounter IGD terbuka → `409` _"Pasien belum didaftarkan di loket IGD. Daftarkan di loket
     lebih dulu, lalu ulangi Konfirmasi Tiba."_ — reservasi tetap menunggu;
   - bila encounter itu pernah punya kunjungan (termasuk yang dihapus) → `409`, tanpa kunjungan baru;
   - selain itu, melahirkan kunjungan IGD **tertaut encounter loket**: status _Menunggu Triage_ (`2`), unit layanan dan
     waktu pendaftaran dari encounter, waktu tiba terkonfirmasi; dalam penyimpanan yang sama reservasi menjadi _Arrived_
     (`2`) dengan tautan ke kunjungan itu.
4. Perawat melanjutkan triage seperti pasien loket biasa; diagnosis, tindak lanjut, form rawat inap, dan tagihan berjalan
   normal.

_Jalur tidak normal._ Waktu tiba yang diisi melewati waktu sekarang ditolak `400` _"Waktu tiba tidak boleh melewati waktu
sekarang."_ (sama dengan _Mulai Triage_). Bila perawat menekan _Mulai Triage_ pada encounter yang sama di saat bersamaan,
keduanya antre pada kunci episode pasien; yang kedua mendapat `409` kunjungan berjalan, dan admisi membatalkan reservasinya
beralasan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `Services/EmergencyPreArrivalReservationService.cs` | Method yang diubah |
| `Services/EmergencyVisitService.cs` — `StartVisitAsync`, `MulaiKunjunganDalamKunciAsync` (`:950`–`1052`), `NormalizeUtc`, `IsUniqueViolation` | Pembanding penjaga dan isian _Mulai Triage_ |
| `Services/EmergencyEpisodeRule.cs` — `FindOpenEpisodeAsync` (`:163`), `LockPatientEpisodeAsync` (`:207`) | Penjaga episode dan kunci pasien (wajib di dalam transaksi) |
| `Controllers/EmergencyPreArrivalReservationController.cs` — `CheckIn` (`:163`–`190`) | Hak akses dan log yang tidak berubah |
| `Repositories/Configurations/…/EmgVisitConfiguration.cs` `:37` | Index unik `EncounterId` |
| `use-emergency-pre-arrival.jsx` `:288`–`325` (frontend) | Pesan galat backend sudah ditampilkan lewat toast |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyPreArrivalReservationService.cs` | `CheckInReservationAsync`: transaksi dibuka sebelum penjaga; `LockPatientEpisodeAsync`; `FindOpenEpisodeAsync` di dalam kunci; encounter loket wajib (konstanta publik `PesanBelumTerdaftarDiLoket`); penolakan encounter yang pernah punya kunjungan; `EmgVisit` diisi `EncounterId`, `ServiceUnitId` dan `RegistrationCompletedAt` dari encounter, `VisitStatus` `WaitingForTriage`, `IsImmediateCareAllowed`; validasi waktu tiba; satu `SaveChangesAsync`; pelanggaran index unik → `409`. Pembacaan pengaturan IGD (`ResolveActiveSettingAsync`) dihapus karena unit layanan kini dari encounter. Helper privat `NormalizeUtc`; `using Npgsql`. +69/−27 baris |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request/response `POST …/pre-arrival-reservations/{id}/check-in` tidak berubah. Perilaku berubah sesuai API `0.20.0` §13.2: satu `409` baru, satu `400` baru (waktu tiba), dan `400` _pengaturan IGD tidak tersedia_ tidak lagi mungkin. **Delta kecil terhadap validation §15 aturan 6:** pelanggaran index unik — yang hanya terjadi bila penulis lain tidak memakai kunci episode pasien — mengembalikan pesan sendiri (_"Pasien ini baru saja mulai ditangani lewat kunjungan IGD lain…"_), bukan `PesanKunjunganLainBerjalan`, karena di transaksi PostgreSQL yang sudah batal kunjungan lain tidak dapat dibaca. Balapan dengan _Mulai Triage_ diselesaikan kunci episode dan tetap mendapat `PesanKunjunganLainBerjalan` |
| Database | Nol schema, nol migration. Tidak ada eksekusi basis data oleh agent |
| Keamanan/Auth | `NOT APPLICABLE` — `[AccessAction]`/`[AccessPermission]` pada controller tidak disentuh (`EmergencyPreArrivalReservation : Update`) |

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Pre-Arrival Reservation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations/{id}/check-in` | Konfirmasi tiba: melahirkan kunjungan IGD dari encounter loket pasien dan menandai reservasi _Arrived_ | `EmergencyPreArrivalReservation : Update` |

| Kode | Kapan |
| --- | --- |
| `200` | Kunjungan lahir tertaut encounter loket; reservasi `2` |
| `400` | Reservasi bukan `PendingArrival`; pasien belum ditautkan; waktu tiba melewati saat ini |
| `404` | Reservasi tidak ditemukan |
| `409` | Pasien belum didaftarkan di loket IGD; pasien punya kunjungan IGD berjalan; encounter pernah punya kunjungan |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling\qbe\Invoke-QbeConformanceCheck.ps1 -Mode Strict` (working tree) | Files evaluated 6 (lima berkas `BE-IGD-083` + berkas ini); VIOLATION 0, REVIEW 0, INFO 0; `PASS` | `PASS` | Keluaran perintah, 18 Oktober 2026 malam |
| Review diff dan scope | Hanya `EmergencyPreArrivalReservationService.cs` berubah untuk task ini; berkas `.cs` lain yang berubah milik `BE-IGD-083` | `PASS` | `git diff --stat` |
| Komentar dan akhiran baris | Nol komentar baru; CRLF tanpa BOM (551 CRLF, 0 LF) | `PASS` | Hitungan byte Node |
| Penelusuran jalur normal | Encounter loket ada → kunci → episode `Encounter` → tidak ada `EmgVisit` untuk encounter → kunjungan `WaitingForTriage` + reservasi `Arrived` dalam satu `SaveChangesAsync` → commit | `PASS` (source) | Diff `:413`–`495` |
| Penelusuran jalur tidak normal | Tanpa encounter → `409` sebelum tulis apa pun; kunjungan berjalan → `409` pesan lama; waktu tiba di masa depan → `400` sebelum transaksi; galat unik → `409`, transaksi dibuang tanpa commit | `PASS` (source) | Diff |
| `dotnet build` (Rizki) | Build Rizki 18 Oktober 2026: DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` 16.04.59, lebih baru dari seluruh suntingan, memuat teks penanda kode task ini (diperiksa agent lewat pencarian byte); DLL hanya ditulis bila kompilasi berhasil, jadi 0 error. Jumlah warning belum dilaporkan | `PASS` (error); warning menunggu | Cap waktu DLL; teks _"Pasien belum didaftarkan di loket IGD"_ ada di DLL |
| `dotnet build --no-incremental` (Rizki, PC kantor) | Build ulang penuh: build penuh Rizki di PC kantor 19 Oktober 2026 (`dotnet build-server shutdown`, lalu `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false -m:1 -nodeReuse:false --no-incremental`) pada `rizkiG` `4cf5c1fd`: **0 error, 285 warning** pada proyek (104,3 detik). Lima di antaranya `NU1902`/`NU1903` paket `SixLabors.ImageSharp` 3.1.11 yang juga muncul di tahap restore; 280 sisanya warning kompilator `CS…`. Pada bagian daftar yang terbaca (tempelan keluaran terpotong sebelum akhir daftar), nol warning berasal dari lima berkas yang diubah `BE-IGD-084`/`085`. DLL 09.13.52 memuat teks penanda kode task (pencarian UTF-16) | `PASS` | Keluaran build yang ditempel Rizki di sesi; cap waktu DLL; teks _"Pasien belum didaftarkan di loket IGD"_ ada di DLL |
| Putaran P1 (panduan uji sisa bagian 3F) | P1-03 `409` pesan kontrak (17.18.52); P1-04 kunjungan `IGD-261018105233-F75F42` ber-encounter loket `103b9248…`, `visitStatus` `2`, reservasi `2`; P1-05 _Buat Form_ `201`; P1-07 _Mulai Triage_ `201`. Persiapan penguji memakai SQL dan sandi tertulis (`IGD-FACT-148`) — disahkan sebatas bukti log (`IGD-DEC-328`) | `PASS` | [penilaian P1](../../../testing/2026-10-18-laporan-uji-putaran-p1.md) |

Uji manual: `REQUIRED` — putaran P1.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Langkah build untuk Rizki** (jalur ringan, dari folder backend; matikan dulu backend yang sedang berjalan karena DLL-nya
terkunci):

```powershell
dotnet build-server shutdown
dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false -m:1 -nodeReuse:false --no-incremental
```

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pasien loket (asuransi) di-_check-in_ → `200`; kunjungan `encounterId` = encounter loket, `visitStatus` `2`; reservasi `2` dengan `resultEmergencyVisitId` | Terpenuhi — P1-04 | Source siap; P1-04 |
| 2 | Belum didaftarkan loket → `409` pesan aturan 3; reservasi tetap `1`; nol kunjungan baru | Terpenuhi — P1-03 | Source: penolakan sebelum `Add`; P1-03 |
| 3 | Kunjungan berjalan → `409` `PesanKunjunganLainBerjalan` | Terpenuhi (diff) | Cabang `OpenEpisodeKind.Visit` tidak berubah, kini di dalam kunci |
| 4 | Kunjungan hasil _check-in_ berjalan seperti pasien loket sampai _Buat Form_ | Terpenuhi — P1-05 | `EncounterId` terisi menghilangkan sebab `PesanKunjunganTanpaPasien`; P1-05 |
| 5 | Atomik; balapan dengan _Mulai Triage_ ditolak `409` | Terpenuhi (diff), dengan delta pesan di 3.3 | Satu transaksi, kunci episode, satu `SaveChangesAsync` |
| 6 | Regresi: _Mulai Triage_, buat/ubah/batal reservasi tidak berubah | Terpenuhi — diff; P1-01 (buat reservasi), P1-07 (_Mulai Triage_ `201`), pembatalan backend `200` | Method lain dan `EmergencyVisitService.cs` tidak disentuh task ini |
| 7 | Diff hanya Cakupan; nol frontend, migration, DTO/route/enum, `Program.cs`; nol komentar baru; CRLF; QBE `PASS` | Terpenuhi | Bagian 5 |
| 8 | Build 0 error; jumlah warning dilaporkan | Terpenuhi — build penuh Rizki di PC kantor 19 Oktober 2026 (`--no-incremental`, `rizkiG` `4cf5c1fd`): 0 error, 285 warning (5 NuGet `NU1902`/`NU1903` `SixLabors.ImageSharp`, 280 kompilator; nol dari berkas task pada daftar yang terbaca). *Sebelumnya:* sebagian — 0 error terbukti (DLL 16.04.59), jumlah warning belum dilaporkan | Bagian 5 |

DoD: laporan ini ✅; register, kartu, dan traceability diperbarui ✅. Belum: build, P1. **19 Oktober 2026 pagi:** keduanya terpenuhi (P1 18 Oktober malam, build penuh 19 Oktober) — DoD lengkap. Sesudah P1 lulus, `BE-IGD-080`
(#4–7) dan `FE-IGD-067` (#4, #5, #7) dinilai ulang dari bukti yang sama.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kunjungan tanpa encounter yang terlanjur lahir di dev sebelum perbaikan tetap ada dan menahan pasiennya (episode berjalan). P1 memakai pasien baru |
| Masalah yang diketahui | Delta pesan pada cabang galat unik (3.3). Pesan _pernah dihapus_ disalin dari _Mulai Triage_ sebagai konstanta privat, bukan dipakai bersama |
| Risiko tersisa | Petugas yang terbiasa menekan _Konfirmasi Tiba_ sebelum loket kini mendapat `409`; perlu SOP singkat untuk demo (GRO ke loket dulu) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | **19 Oktober 2026:** source task ini sudah di-commit Rizki (`4fbf5ff6`, termuat di `rizkiG` `4cf5c1fd`); pembaruan laporan ini belum di-commit. *Sebelumnya:* berkas ini, `EmergencyPreArrivalReservationService.cs`, dan perubahan sebelumnya (`BE-IGD-083`, dokumen blueprint) — belum di-commit |
| Langkah berikutnya | Tidak ada untuk task ini. *Sebelumnya:* `BE-IGD-084`; build Rizki; putaran P1 |
