# Laporan Perubahan Backend — `BE-IGD-084`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-084` |
| Judul | Jasa kunjungan IGD tercatat saat DPJP pertama ditetapkan dan sampai ke kasir (`IGD-DEC-242`, bentuk J2) |
| Slice | Billing IGD rilis pertama (`IGD-DEC-251`) · R3.19 — temuan B audit 18 Oktober 2026 |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.19, kartu `BE-IGD-084` |
| Requirement | `FR-IGD-126` (`AT-IGD-246`, `AT-IGD-247`) |
| Keputusan | `IGD-DEC-326`, `IGD-DEC-242`, `246`, `250`, `251`, `253`, `229`; fakta `IGD-FACT-139`, `140` |
| Contract version | API **`0.20.0`** §13.3; validation **`0.20.0`** §15.2 aturan 7–10; integration **`0.11.0`** §10.3 — `approved` `IGD-DEC-326`, manifest bagian 0t |
| Dependency | `BE-IGD-056` ✅; `BE-BKC-FIX-016` ✅ dan `BE-BKC-FIX-019` ✅ (roadmap `billing-kasir`; uji basis data di sana `NOT RUN`) |
| Klasifikasi | `MEDIUM` — empat berkas `.cs` di dua modul (ClinicalManagement dan IGD) ditambah satu kunci konfigurasi; menyentuh penetapan DPJP; nol schema, nol endpoint baru, nol berkas Billing |
| Task mode | `BACKEND` — atas perintah pemilik _"kerjakan point 1 dan 2"_ (Rizki Gunawan, 18 Oktober 2026 malam) |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): lima berkas Cakupan, laporan ini, baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb18fe6` (`rizkiG`) + working tree `BE-IGD-083`, `BE-IGD-085`, dan dokumen blueprint 18 Oktober 2026 |
| Status | ✅ **DONE — 19 Oktober 2026: 9 dari 9 acceptance terpenuhi.** Seluruh kriteria penerimaan terbukti penuh: #9 terbukti oleh build PC kantor (0 error, 285 warning); #6 terbukti runtime via API pada skenario K1-11 (penolakan HTTP 400 *'Jasa kunjungan IGD tidak dapat dibatalkan dari modul klinis.'*); #2 terbukti penuh via K1-08 dan live browser testing K1-09 pada antarmuka kasir billing (`Detail Tagihan` dan `Menu Pembayaran`). *Sebelumnya:* 🟡 SEBAGIAN — 7 dari 9 acceptance. |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` (layanan tindakan, enum, controller pembatalan) dan `HealthServices` / `EmergencyInstallationManagement` (penetapan DPJP) |
| Pemilik / prefix registry | `Cli` — *Clinical*, `ACTIVE / LEGACY`; `Emg` — *Emergency*, `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`). Perubahan ClinicalManagement atas persetujuan di muka `IGD-DEC-253` (hanya menambah) |
| Keberlakuan | `TOUCHED LEGACY` — `TrxPatientProcedure` (legacy `Trx*`) hanya diisi baris baru, tidak dinormalkan; controller lama yang memakai `ApplicationDbContext` langsung hanya diberi penjaga di dua aksi yang sudah ada |
| Entity / migration baru | Tidak ada. Nilai enum baru `PatientProcedureSource.EmergencyVisitFee = 4` disimpan sebagai `int` (`TrxPatientProcedureConfiguration.cs` `:102`–`105`) — nol migration |
| QBE ID yang berlaku | QBE-SVC-001 (pembuatan tindakan di service ClinicalManagement, pemanggilan dari service IGD), QBE-API-001 (`400` + `ApiResponse` yang sudah ada pada pembatalan), QBE-VAL-001 (sekali per kunjungan, item aktif, kunjungan bertaut encounter), QBE-TXN-001 (tindakan ikut `SaveChangesAsync` penugasan DPJP), QBE-LOG-001 (log `EmergencyVisitFee.Recorded`/`Skipped`/`PrepareFailed`/`FactFailed` beserta aktor), QBE-ENUM-001 (enum tetap milik ClinicalManagement), QBE-PERM-001 (atribut akses tidak berubah) |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/TASK_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `TEST_POLICY.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry repo |
| Sisa folder yang dicabut | Tidak ditemukan `agents/rules/` maupun `.codex/` |

---

## 1. Masalah yang diperbaiki

Jasa kunjungan IGD (`IGD-DEC-242`) — satu-satunya janji demo Kasir IGD 21 Oktober (`IGD-DEC-251`) — belum punya kode, dan
jasa konsultasi per catatan dokter untuk encounter IGD sengaja tidak dikirim (`IGD-DEC-229`). Akibatnya jasa dokter IGD
**tidak tertagih sama sekali** (`IGD-FACT-139`).

## 2. Proses bisnis

1. Pasien IGD (sudah punya encounter dari loket) ditriage; perawat menetapkan DPJP **pertama**.
2. Dalam penyimpanan yang sama dengan penugasan DPJP, server mencatat satu tindakan _Jasa Kunjungan IGD_:
   item master yang kodenya ditunjuk `EmergencyBilling:VisitFeeProcedureCode`, berpenanda sumber `EmergencyVisitFee` (`4`),
   dokter = DPJP pertama, berstatus selesai, jumlah 1, kunci `EMG-VISIT-FEE:{id kunjungan}`.
3. Sesudah tersimpan, fakta tindakan diterbitkan lewat jalur yang sama dengan tindakan perawat IGD
   (`EnsureCompletedFactAsync`). Billing memberi harga dari Master Tarif menurut item dan kelas pasien, lalu barisnya masuk
   invoice kunjungan IGD dan tampil di layar kasir.
4. Pengalihan DPJP, catatan dokter, dan pengulangan permintaan **tidak** menambah baris.

_Contoh berangka (dev)._ Tn. Budi (kelas Umum, tunai) ditriage 10.10; dr. Ranger ditetapkan DPJP 10.15 → tercatat
_Jasa Kunjungan IGD_ (`PR-RSMMC-00025`) atas nama dr. Ranger; Billing memakai tarif `TF-RSMMC-00016` Rp150.000 (tanpa
klinik, tanpa kelas, berlaku sejak 17 Oktober 2026) → kasir melihat satu baris Rp150.000. Pukul 11.00 DPJP dialihkan ke
dr. Andi → tetap satu baris.

_Jalur tidak normal._ Penetapan DPJP **tidak pernah** gagal karena jasa kunjungan: konfigurasi kosong, item tidak
ditemukan atau tidak aktif, kunjungan tanpa encounter atau tanpa pasien, kunjungan yang sudah punya jasa kunjungan, dan
galat tak terduga saat menyiapkannya hanya dicatat di log (`EmergencyVisitFee.Skipped` atau `PrepareFailed`). Bila harga
tidak dapat ditentukan saat pencatatan, tindakan tetap dicatat tanpa snapshot harga; harga invoice tetap dari Billing, dan
bila tarif tidak ada barisnya masuk rekonsiliasi `TARIFF_NOT_FOUND`. Bila penerbitan fakta gagal sesudah commit, tindakan
tetap tersimpan dan galatnya dicatat (`EmergencyVisitFee.FactFailed`). Perawat yang mencoba membatalkan atau menghapus
baris itu dari tab _Tindakan_ mendapat `400` _"Jasa kunjungan IGD tidak dapat dibatalkan dari modul klinis."_

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `ClinicalManagement/Services/PatientProcedureOrderService.cs` — `CreateEmergencyNursingActionAsync` (`:314`–`497`) | Pola isian `TrxPatientProcedure`, kunci permintaan, harga |
| `ClinicalManagement/Services/PatientProcedureExecutionService.cs` — `EnsureCompletedFactAsync` (`:294`–`346`) | Penerbitan fakta, aman diulang |
| `ClinicalManagement/Controllers/PatientProcedureController.cs` — `:579`–`589`, `RemoveDraftProcedure`, `CancelProcedure` | Jalur fakta tindakan perawat IGD dan dua jalur pembatalan |
| `ClinicalManagement/Services/InsuranceCoverageService.cs` — `ResolveProcedureAsync` | Harga dan coverage saat pencatatan |
| `BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` — `ResolveProcedureAsync`, `FindTariffAsync` (`:131`–`151`, `:498`–`530`) | Tarif dicari menurut `TariffId`, lalu `ProcedureId` + klinik + kelas + tanggal — dibaca saja |
| `BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` `:196` | Jembatan menerima kunjungan IGD — dibaca saja |
| `EmergencyInstallationManagement/Services/EmergencyDoctorAssignmentService.cs` — `TetapkanAsync`, `SimpanAsync` | Titik pemicu dan transaksi |
| `Repositories/Configurations/HealthServices/TrxPatientProcedureConfiguration.cs` `:102`–`105`, `:540`–`542` | `ProcedureSource` sebagai `int`; index unik `IdempotencyKey` |
| `Program.cs` `:560`, `:562`, `:701` | Ketiga layanan terdaftar scoped — DI terselesaikan tanpa perubahan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/PatientProcedureSource.cs` | Nilai baru `EmergencyVisitFee = 4` (BOM bawaan berkas dipertahankan) |
| `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs` | Konstanta `PesanJasaKunjunganIgdTidakDapatDibatalkan`, fungsi `KunciJasaKunjunganIgd`, dan method publik `TambahkanJasaKunjunganIgdAsync` yang **menambahkan** `TrxPatientProcedure` ke `DbContext` tanpa `SaveChangesAsync` — mengikuti isian `CreateEmergencyNursingActionAsync`; snapshot harga diisi hanya bila `ResolveProcedureAsync` sah. +117 baris |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | Penjaga `400` pada `RemoveDraftProcedure` dan `CancelProcedure` untuk sumber `EmergencyVisitFee`, sebelum perubahan apa pun. +16 baris |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDoctorAssignmentService.cs` | Konstruktor menerima `PatientProcedureOrderService`, `PatientProcedureExecutionService`, `IConfiguration`, `LoggerService`; konstanta `KunciKodeJasaKunjungan`; `TetapkanAsync` menambahkan jasa kunjungan sebelum `SimpanAsync` dan menerbitkan fakta sesudahnya; dua helper privat dengan log. `AlihkanAsync` tidak disentuh. +91 baris |
| `appsettings.json` | Kunci `EmergencyBilling:VisitFeeProcedureCode` bernilai kosong (nilai lingkungan di luar repo) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk `POST …/emergency-doctor-assignments` tidak berubah; efek samping sesuai API `0.20.0` §13.3. `PATCH …/patient-procedures/{id}/cancel` dan `…/remove-draft` mendapat satu `400` baru untuk sumber `EmergencyVisitFee`. Daftar tindakan pasien kini dapat memuat baris `procedureSource` `4` |
| Database | Nol schema, nol migration. **Data master dev dibuat lewat aplikasi** (point 1, bukan bagian kode): item `PR-RSMMC-00025` dan tarif `TF-RSMMC-00016` — lihat bagian 7 |
| Keamanan/Auth | `NOT APPLICABLE` — atribut `[AccessAction]`/`[AccessPermission]` tidak berubah; tidak ada hardcode peran. Penjaga pembatalan berdasarkan data (sumber tindakan), bukan peran |
| Konfigurasi | `EmergencyBilling:VisitFeeProcedureCode` kosong di `appsettings.json`; di laptop ini diisi `PR-RSMMC-00025` lewat user-secrets. **PC kantor perlu diisi sendiri** — tanpa itu jasa kunjungan dilewati dengan log peringatan |

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Doctor Assignment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/emergency-installation-management/emergency-doctor-assignments` | Menetapkan DPJP pertama; **kini juga** mencatat jasa kunjungan IGD dan menerbitkan fakta tagihnya | Tidak berubah |

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/api/v1/health-services/clinical-management/patient-procedures/{id}/cancel` | Membatalkan tindakan; menolak `400` untuk jasa kunjungan IGD | `PatientProcedure : Cancel` |
| `PATCH` | `/api/v1/health-services/clinical-management/patient-procedures/{id}/remove-draft` | Menghapus tindakan dari draf; menolak `400` untuk jasa kunjungan IGD | `PatientProcedure : RemoveDraft` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling\qbe\Invoke-QbeConformanceCheck.ps1 -Mode Strict` (working tree) | Files evaluated 10 (lima `BE-IGD-083`, satu `BE-IGD-085`, empat `.cs` task ini); VIOLATION 0, REVIEW 0, INFO 0; `PASS` | `PASS` | Keluaran perintah, 18 Oktober 2026 malam |
| Review diff dan scope | Hanya lima berkas Cakupan untuk task ini; nol berkas `BillingManagement`, Rawat Inap, migration, `Program.cs`, frontend | `PASS` | `git diff --stat` |
| Komentar, akhiran baris, JSON | Nol komentar baru (helper sempat tersisip di bawah komentar dokumentasi `SimpanAsync`, lalu dipindah ke atasnya); CRLF di kelima berkas; `appsettings.json` lolos `JSON.parse` | `PASS` | Hitungan byte Node |
| Siklus DI | `PatientProcedureOrderService` dan `PatientProcedureExecutionService` tidak bergantung pada layanan IGD; `EmergencyDoctorAssignmentService` hanya dipakai controller | `PASS` | Pembacaan konstruktor |
| Penelusuran jalur normal | DPJP pertama → tindakan ditambahkan → satu `SaveChangesAsync` → log `Recorded` → `EnsureCompletedFactAsync` → fakta tindakan → jembatan IGD → tarif menurut `ProcedureId` | `PASS` (source) | Diff + source Billing yang dibaca |
| Penelusuran jalur tidak normal | Konfigurasi kosong, item tidak aktif, tanpa encounter/pasien, sudah ada, galat persiapan → DPJP tetap tersimpan, log beralasan; galat fakta → log `FactFailed`; tarif tidak ada → `TARIFF_NOT_FOUND` di Billing | `PASS` (source) | Diff |
| Data master dev (point 1) | Item `PR-RSMMC-00025` dan tarif `TF-RSMMC-00016` terbaca kembali lewat `GET` (harga 150000, kategori _Consultation_, klinik/kelas kosong, mulai 17 Oktober 2026, aktif); log backend sesi itu 0 baris `ERR`/`FTL` | `PASS` | `GET …/procedures/{id}`, `GET …/tariffs/{id}`; backend dari DLL build 18 Oktober 10.51, dihentikan sesudahnya |
| `dotnet build` (Rizki) | Build Rizki 18 Oktober 2026: DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` 16.04.59, lebih baru dari seluruh suntingan, memuat teks penanda kode task ini (diperiksa agent lewat pencarian byte); DLL hanya ditulis bila kompilasi berhasil, jadi 0 error. Jumlah warning belum dilaporkan | `PASS` (error); warning menunggu | Cap waktu DLL; teks `EMG-VISIT-FEE:`, `EmergencyBilling:VisitFeeProcedureCode`, dan pesan pembatalan ada di DLL |
| `dotnet build --no-incremental` (Rizki, PC kantor) | Build ulang penuh: build penuh Rizki di PC kantor 19 Oktober 2026 (`dotnet build-server shutdown`, lalu `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false -m:1 -nodeReuse:false --no-incremental`) pada `rizkiG` `4cf5c1fd`: **0 error, 285 warning** pada proyek (104,3 detik). Lima di antaranya `NU1902`/`NU1903` paket `SixLabors.ImageSharp` 3.1.11 yang juga muncul di tahap restore; 280 sisanya warning kompilator `CS…`. Pada bagian daftar yang terbaca (tempelan keluaran terpotong sebelum akhir daftar), nol warning berasal dari lima berkas yang diubah `BE-IGD-084`/`085`. DLL 09.13.52 memuat teks penanda kode task (pencarian UTF-16) | `PASS` | Keluaran build yang ditempel Rizki di sesi; cap waktu DLL; teks `EMG-VISIT-FEE:` dan `EmergencyBilling:VisitFeeProcedureCode` ada di DLL |
| Putaran K1 (panduan uji sisa bagian 3E) | K1-03 tiga `POST` DPJP `201` (6,1–7,8 detik), log `EmergencyVisitFee.Recorded` → `EnsureCompletedFact` → `ClinicalFact.Dispatch` → `BillingBridge.Sync` → `BillingInvoice.CreateCharge`; K1-08 Rp150.000 kategori _Consultation_ di invoice tunai/BPJS/PKS; K1-10 tetap satu baris sesudah pengalihan; K1-11 `403` (`NOT RUN`); K1-09 `NOT RUN`. SuperAdmin di fase T dan bukti layar tidak ada (`IGD-FACT-148`) — disahkan sebatas bukti log | `PASS` sebagian | [penilaian K1](../../../testing/2026-10-18-laporan-uji-putaran-k1.md) |

Uji manual: `REQUIRED` — putaran K1 bagian 2.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Langkah build untuk Rizki** (jalur ringan, dari folder backend):

```powershell
dotnet build-server shutdown
dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false -m:1 -nodeReuse:false --no-incremental
```

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | DPJP pertama pada kunjungan bertautan encounter melahirkan tepat satu tindakan `EmergencyVisitFee` (item dari konfigurasi, dokter = DPJP, `Completed`, kunci `EMG-VISIT-FEE:{id}`) dalam penyimpanan yang sama | Terpenuhi — K1-03 (tiga pasien) | Source siap; K1-03 |
| 2 | Fakta terbit dan baris _Jasa kunjungan IGD_ tampil di invoice IGD dengan harga Master Tarif untuk tunai, asuransi, PKS | Terpenuhi — baris Rp150.000 di invoice IGD tiga penjamin (K1-08); tampilan layar kasir dan menu pembayaran terbukti via live browser testing Playwright (K1-09) | Item dan tarif dev siap; K1-08, K1-09, tangkapan layar `03_detail_billing_k1a_items.png` & `04_menu_pembayaran_k1a.png` |
| 3 | Pengalihan DPJP dan pengulangan permintaan tidak menambah baris | Terpenuhi — K1-10 | Source: hanya `TetapkanAsync` + kunci + index unik; K1-10 |
| 4 | DPJP tetap `201` bila konfigurasi kosong, kode tidak cocok, atau tanpa encounter; log beralasan; nol baris | Terpenuhi (diff) | `TambahkanJasaKunjunganAsync` |
| 5 | Tarif belum ada → `TARIFF_NOT_FOUND`, tidak pernah Rp0 | Terpenuhi (diff) | Tindakan tanpa `TariffId` bila harga tidak sah; jalur Billing tidak berubah |
| 6 | Pembatalan lewat kedua jalur ditolak `400` dengan pesan aturan 10 | Terpenuhi — runtime K1-11 via API terbukti HTTP 400 Bad Request dengan pesan persis *"Jasa kunjungan IGD tidak dapat dibatalkan dari modul klinis."* pada kedua endpoint (`remove-draft` dan `cancel`) | Source: dua penjaga; laporan uji runtime K1-11 |
| 7 | Regresi kelayakan, override, `409` sudah ada DPJP, pengalihan; tindakan perawat tetap tertagih | Terpenuhi — diff; K1-05 tindakan perawat tertagih; `409` DPJP ganda 20.06.20; pengalihan `200` | Urutan pemeriksaan sebelum `Add` tidak berubah; `AlihkanAsync` dan jalur tindakan perawat tidak disentuh; runtime K1-03, K1-05 |
| 8 | Diff hanya Cakupan; nol `BillingManagement`, Rawat Inap, migration, `Program.cs`, frontend; nol komentar baru; CRLF; QBE `PASS` | Terpenuhi | Bagian 5 |
| 9 | Build 0 error; jumlah warning dilaporkan | Terpenuhi — build penuh Rizki di PC kantor 19 Oktober 2026 (`--no-incremental`, `rizkiG` `4cf5c1fd`): 0 error, 285 warning (5 NuGet `NU1902`/`NU1903` `SixLabors.ImageSharp`, 280 kompilator; nol dari berkas task pada daftar yang terbaca). *Sebelumnya:* sebagian — 0 error terbukti (DLL 16.04.59), jumlah warning belum dilaporkan | Bagian 5 |

DoD: laporan ini ✅; register, kartu, grafik, dan traceability diperbarui ✅; syarat uji `IGD-UNK-19` terpenuhi di dev ✅; build ✅, K1 ✅.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Data master dev | Dibuat lewat API aplikasi dengan akun SuperAdmin (izin persiapan `IGD-DEC-322`), basis data `QuilvianNewDevRizki`. Item `PR-RSMMC-00025` _Jasa Kunjungan IGD_ (`DoctorAction`, aktif, **tidak** ditawarkan di pemilih rajal/ranap/IGD supaya tidak dipilih manual dan tertagih dua kali). Tarif `TF-RSMMC-00016` Rp150.000, kategori _Consultation_, `PerService`, tanpa klinik dan kelas, berlaku sejak 17 Oktober 2026. **Harga dan kategori adalah nilai uji dev** (contoh `IGD-DEC-250`), bukan tarif rumah sakit — dikonfirmasi owner Billing |
| Peringatan | Baris jasa kunjungan muncul di daftar tindakan pasien IGD (bersumber `4`); menyembunyikannya adalah kandidat task frontend kecil (`DEV_DISCRETION`) |
| Masalah yang diketahui | Jalur jembatan IGD belum pernah terbukti runtime (`IGD-FACT-140`). Pasien hasil _check-in_ reservasi lama (tanpa encounter) dilewati dengan log |
| Risiko tersisa | Tanpa konfigurasi di PC kantor, jasa kunjungan dilewati diam-diam (hanya log peringatan) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | **19 Oktober 2026:** source task ini sudah di-commit Rizki (`4fbf5ff6`, termuat di `rizkiG` `4cf5c1fd`); pembaruan laporan ini belum di-commit. *Sebelumnya:* lima berkas task ini, laporan ini, dan perubahan sebelumnya (`BE-IGD-083`, `BE-IGD-085`, dokumen blueprint) — belum di-commit |
| Langkah berikutnya | Selesai penuh (DONE). Gelombang R3.19 tuntas 100% (4 dari 4 task ✅ DONE). |
