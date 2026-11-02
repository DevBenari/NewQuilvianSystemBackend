# Laporan Perubahan Backend — `BE-IGD-071`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-071` |
| Judul | Pesanan lab dan radiologi perawat IGD wajib menyebut dokter pemberi instruksi, lalu diverifikasi dokter itu |
| Slice | `EPIC IGD-14` / `MVP-9` — pesanan perawat atas instruksi |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.16, kartu revisi 16 |
| Trace | `FR-IGD-102`; `AT-IGD-209`, `210`, `249`, `250`; `IGD-DEC-224`, `230` (6), `360`, `362`…`368` |
| Contract version | API **`0.21.0`** §10.1 n.9–11, §10.7, §10.7a; validation **`0.21.0`** §12.4 aturan 13–13c; state **`0.16.0`** §10.2; permission/audit `0.13.0` §9.1; integration `0.12.0` — seluruhnya **`approved`** (`IGD-DEC-368`, manifest 0u) |
| Dependency | — (`IGD-OQ-117` dijawab `IGD-DEC-360`) |
| Klasifikasi | `MEDIUM` — 5 berkas di tiga modul, 1 endpoint baru, 1 DTO baru, perilaku `POST` dua modul berubah memutus untuk perawat IGD; nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): lima berkas source pada 3.2; laporan ini, roadmap, traceability, `MODULE-STATUS.md`, decision log |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `1c00e31d` (`rizkiG`) + working tree |
| Tanggal | 27 Oktober 2026 |
| Status | ✅ **SELESAI — 28 Oktober 2026 (siang).** Acceptance 1–13 terpenuhi: sesudah `IGD-OQ-145` dipenuhi, `uji-ulang-oq145.mjs` 16/16 `PASS` (dokter jaga, dokter rangkap, verifikasi termasuk sesudah pengalihan, penolakan dokter lain, akun dokter kedua) dan uji layar `FE-IGD-057`/`058` lulus; kriteria 11 diterima lewat diff dan penolakan rawat inap yang teruji (`IGD-DEC-371`). Belum di-commit. *Sebelumnya:* 🟡 **SEBAGIAN — 27 Oktober 2026 (petang), sesudah uji API.** Terpenuhi: acceptance 1 (API), 3, 4, 5, 8, 9, 12, 13. **Belum lengkap:** 2 (`NOT RUN` — tidak ada dokter IGD berjadwal jaga), 6 (worklist kedua arah terbukti; verifikasi `NOT RUN` — posisi dokter IGD tanpa izin `LabOrder:Verify`/`RadOrder:Verify`), 7 (bagian `basis 2` dan dokter rangkap `NOT RUN`), 10 (dokter jaga, verifikasi `NOT RUN`), 11 (penolakan rawat inap terbukti; jalur `Pending` rawat inap dan seluruh rawat jalan `NOT RUN` — tidak ada data). Nol cacat kode ditemukan. *Sebelumnya:* 🟡 27 Oktober 2026 (sore) — source selesai, QBE `PASS`, menunggu build dan uji API |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` (`Emg`, `ACTIVE / LEGACY`); `LaboratoryManagement` (`Lab`, `ACTIVE`); `RadiologyManagement` (`Rad`, `ACTIVE`) — registry `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `TOUCHED LEGACY` — nol entity, nol tabel, nol prefix baru |
| QBE yang berlaku | `QBE-SVC-001` (logika di service, controller tanpa `DbContext`), `QBE-API-001` (`ApiResponse<T>`, kode status mapan), `QBE-PERM-001` (`[AccessAction]` + `[AccessPermission]` yang cocok), `QBE-VAL-001` (aturan 13, 13a), `QBE-DTO-001` (DTO, bukan entity) |
| Wewenang lintas modul | `IGD-DEC-360` — pemilik Laboratorium dan Radiologi (Yoga Aji Pratama) menyetujui perluasan; perubahan di dua service mereka terbatas pada cabang encounter IGD |
| `AGENTS.md` / `rules/backend/` | Terbaca; tidak ada sisa `agents/rules/` di repository |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, pesanan laboratorium dan radiologi yang dibuat perawat untuk pasien IGD selalu berstatus
verifikasi `NotRequired`, dan isian dokter pemberi instruksi **diabaikan**: `ResolveInstructionAsync` (Lab) dan blok
instruksi `CreateCoreAsync` (Rad) hanya bekerja untuk pesanan yang membawa episode rawat inap. Akibatnya pesanan perawat
atas instruksi lisan dokter IGD tidak pernah masuk daftar *Perlu Verifikasi* dokter itu, dan tidak ada jejak siapa
yang memberi instruksi (`IGD-FACT-082`). Pemilik Laboratorium dan Radiologi menyetujui perluasan aturan pada
27 Oktober 2026 dengan syarat dokter pemberi instruksi yang sah (`IGD-DEC-360`), dirumuskan sebagai DPJP IGD aktif atau
dokter jaga IGD aktif (`IGD-DEC-362`).

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Perawat IGD | Membuka formulir pesanan lab/radiologi; layar mengambil `GET …/instructing-doctor-options?emergencyVisitId=` — DPJP IGD aktif lalu dokter jaga IGD aktif |
| 2 | Perawat IGD | Memilih dokter yang memberi instruksi lisan, mengirim `POST /lab-orders` atau `POST /rad-orders` dengan `instructingDoctorId` |
| 3 | Sistem | Mengenali encounter milik kunjungan IGD; pembuat bukan akun dokter → dokter wajib dipilih (`400`) dan wajib lolos penentu IGD (`403`); lolos → pesanan `Pending` |
| 4 | Dokter pemberi instruksi | Membuka *Perlu Verifikasi* (worklist yang sudah ada) dan memverifikasi — tetap dia walaupun DPJP sudah dialihkan |
| — | Dokter | Memesan sendiri: `NotRequired`, tidak masuk worklist (tidak berubah) |

*Contoh.* Pukul 02.00 Tn. Budi di IGD punya DPJP dr. Ani, dan dr. Rudi berjadwal jaga IGD 00.00–08.00. Perawat Dimas
memesan darah lengkap dan memilih dr. Ani → `Pending`, muncul di daftar dr. Ani. Bila Dimas mengirim dr. Sari (radiolog
yang tidak bertugas pada kunjungan itu) lewat API → `403` *"Dokter yang dipilih tidak sedang bertugas atas pasien ini."*.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`LabOrderService.cs` (`CreateAsync`, `ResolveInstructionAsync`, `VerifyInstructionAsync`,
`GetInstructionVerificationWorklistAsync`, jalur `by-examinations`), `LabOrderController.cs` (pemetaan exception `POST`),
`RadOrderService.cs` (`CreateCoreAsync`, verifikasi, worklist), `RadOperationResult.cs`,
`EmergencyDoctorAssignmentService.cs` (`KueriBerjalan`, `AmbilAktifAsync`, `AmbilDokterJagaAktifAsync`,
`KueriJadwalIgdBerlaku`, `GetOperationalNow`), `EmergencyDoctorAssignmentController.cs`, `EmergencyDoctorAssignmentDtos.cs`,
`EmgVisit.cs`, `Program.cs` (registrasi DI), `QuilvianSystemBackend.csproj` (`Compile Remove="Tests\**"`), layar perawat
`emergency-assessment-slice.jsx` (endpoint yang dipanggil).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDoctorAssignmentService.cs` | +91: `AmbilKunjunganUntukEncounterAsync` (kunjungan IGD terbaru untuk satu encounter), `AmbilDokterPemberiInstruksiSahAsync` (`null` bila kunjungan tidak ada), `ApakahDokterPemberiInstruksiSahAsync`, dan satu-satunya penentu `HitungDokterPemberiInstruksiSahAsync` — penugasan berjalan (`KueriBerjalan`) ∪ jadwal shift IGD berlaku (`KueriJadwalIgdBerlaku(GetOperationalNow())`); DPJP lebih dulu, dokter rangkap tampil sekali |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyDoctorAssignmentController.cs` | +23: `GET instructing-doctor-options` |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyDoctorAssignmentDtos.cs` | +11: `EmergencyInstructingDoctorOptionResponse` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | +37/−3: dependensi `EmergencyDoctorAssignmentService`; `ResolveInstructionAsync` menerima `encounterId` dan, untuk pesanan tanpa episode, memanggil `ResolveInstructionIgdAsync` (baru) — bukan IGD → `NotRequired` seperti dahulu |
| `Areas/HealthServices/RadiologyManagement/Services/RadOrderService.cs` | +43/−1: dependensi opsional `EmergencyDoctorAssignmentService` (parameter terakhir, mengikuti pola `RadPhotoNumberService?`); cabang `else if (coverage == null)` untuk encounter IGD tanpa episode dengan kode galat yang sudah ada (`InstructingDoctorRequired`, `InstructingDoctorNotAssigned`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru (§10.7a). `POST /lab-orders` dan `POST /rad-orders` untuk encounter IGD oleh akun bukan dokter: tanpa dokter `400`, dokter tidak sah `403`, lolos `Pending` — **memutus** bagi perawat IGD. Bentuk request dan response tidak berubah. Worklist dan verifikasi tidak disunting: keduanya sudah tidak terikat episode, sehingga pesanan IGD `Pending` otomatis masuk daftar dokter pemberi instruksi dan tetap dapat diverifikasinya sesudah pengalihan (`IGD-DEC-363`) |
| Database | Nol migration, nol kolom, nol tabel; satu kueri baca tambahan `EmgVisit` per pesanan lab/rad tanpa episode |
| Keamanan/Auth | Endpoint baru `[AccessAction("Read", …)]` + `[AccessPermission("EmergencyDoctorAssignment", "Read")]` — butir yang sudah dipegang perawat (permission §9.1); nol hardcode peran. Keabsahan dokter adalah aturan data (penugasan dan jadwal), bukan peran |
| DI | Nol `Program.cs`: ketiga service sudah terdaftar `AddScoped`. Penelusuran konstruktor: 20 dependensi transitif `EmergencyDoctorAssignmentService`, tidak satu pun menjangkau `LabOrderService`/`RadOrderService` — tidak ada siklus |

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Doctor Assignment

Base URL: `api/v1/health-services/emergency-installation-management/emergency-doctor-assignments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/instructing-doctor-options?emergencyVisitId={guid}` | Dokter yang sah dipilih perawat sebagai pemberi instruksi pesanan lab/radiologi | `[AccessPermission("EmergencyDoctorAssignment", "Read")]` |

| Kode | Kapan | Pesan |
| --- | --- | --- |
| `200` | Kunjungan ada; daftar boleh kosong | *"Daftar dokter pemberi instruksi berhasil diambil."* |
| `400` | `emergencyVisitId` kosong | *"EmergencyVisitId wajib diisi."* |
| `404` | Kunjungan tidak ada atau terhapus | *"Kunjungan IGD tidak ditemukan."* |

Isi `data[]`: `doctorId`, `doctorName`, `basis` (`1` DPJP, `2` dokter jaga IGD), `label` (`"dr. Ani — DPJP"`,
`"dr. Rudi — Dokter jaga IGD"`).

#### Laboratory Management / Lab Order · Radiology Management / Rad Order — perilaku berubah

| Method | Path | Hak akses | Perubahan |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/laboratory-management/lab-orders` | `LabOrder : Create` | Encounter IGD, pembuat bukan dokter: `400` *"Pilih dokter yang memberi instruksi."*; `403` *"Dokter yang dipilih tidak sedang bertugas atas pasien ini."*; selain itu `Pending` |
| `POST` | `/api/v1/health-services/radiology-management/rad-orders` | `RadOrder : Create` | Sama, lewat `RadOperationResult.Validation`/`Forbidden` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` (cakupan `WorkingTree`) | 5 berkas dinilai; VIOLATION 0, REVIEW 0, INFO 0; *Final result: PASS* | `PASS` | Keluaran perintah 27 Oktober 2026 |
| Diff: komentar baru pada baris tambahan | 0 | `PASS` | `git diff -U0` disaring `//`, `/*` |
| Akhir baris lima berkas | CRLF utuh, tanpa BOM | `PASS` | Pemeriksaan byte |
| Siklus DI | Tidak ada | `PASS` | Penelusuran konstruktor seluruh `.cs` di luar `bin/obj/Tests/Migrations` |
| `dotnet build -p:RunAnalyzers=false` (Rizki, 27 Oktober 2026) | DLL `bin/Debug/net9.0` 15.29.22 memuat kode baru; 422 warning; belum ada pembanding jumlah warning sebelum task. Error `MSB3027` pada percobaan sebelumnya adalah kunci berkas, bukan kompilasi | `PASS` (pernyataan Rizki) | Waktu berkas DLL; proses `QuilvianSystemBackend.exe` PID 17152 mulai 15.45.09 dari `bin/Debug`, sesudah DLL |
| Uji API run 1 — skrip `uji-api.mjs` | 8 `PASS`, 2 `FAIL` (A10 rad tanpa dokter `500`; A1 verifikasi `403`); A2 tidak berjalan | Lihat 5.1 | `hasil.json`, `jaringan-api.json`, `00-konteks.json` |
| Uji ulang — skrip terpisah `uji-ulang.mjs` (17 `PASS`, 1 `FAIL`, 5 `NOT RUN`) dan `uji-ulang-k6.mjs` (6 `PASS`) | 23 `PASS`, 1 `FAIL` (dokter2 tertahan izin), 5 `NOT RUN`; satu `PASS` dinilai salah rancang. `uji-ulang-k10.mjs` tidak berjalan (backend sudah berhenti) | Lihat 5.1 | `20-*.json`, `30-*.json` |
| Izin efektif dua akun dokter — `baca-izin-efektif.mjs` | dr. Jenah Ali 327 izin (memuat `LabOrder`/`RadOrder` `Create`, `Read`; tanpa `Verify`); dr. B4 IGD Utama 53 izin (tanpa `LabOrder`/`RadOrder` sama sekali) | Fakta lingkungan | `21-izin-efektif-dokter.json` |
| Unit test | — | `NOT RUN` | Atas perintah pemilik: tanpa unit test |

Seluruh bukti di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-071-20261027/` (tidak ikut git). Akun perawat
yang dipakai adalah `dimas` (SuperAdmin, `doctorId` kosong): sah untuk aturan 13 karena aturan itu memeriksa tautan dokter,
bukan peran; **tidak sah** sebagai bukti izin `EmergencyDoctorAssignment : Read` perawat IGD sungguhan — itu diuji pada uji
bersama dengan akun perawat IGD.

### 5.1 Uji API — 27 Oktober 2026 (petang)

Kunjungan uji: Aldo Barbeto (RM `00-00-02-80`, encounter `13b40323…`), DPJP IGD aktif dr. Jenah Ali (penetapan *roster
override* "testing"). Kunjungan tanpa DPJP: Tn. X IGD 2710-0953 (RM `00-00-02-82`, `GET …/active` `404`). Dokter jaga IGD
aktif: **nol** — kelima dokter pada `on-duty-doctors` ber-`scheduleId` kosong. Dokter luar: dr. B4 IGD Pratama.

**Run 1 (`uji-api.mjs`, 08.47–08.49 UTC).**

| Pemeriksaan | Verdict | Catatan penilaian dari data mentah |
| --- | --- | --- |
| A7 isi pilihan; A7 penolakan `400`/`404` | `PASS` | Pilihan = `[dr. Jenah Ali, basis 1]`; dokter luar tidak tampil |
| A3 lab tanpa dokter `400`; A4 lab dokter luar `403` | `PASS` | Pesan tepat; jumlah pesanan encounter tetap |
| A1 lab DPJP | `PASS` | `Pending`, `instructingDoctorId` = Jenah |
| A10 rad tanpa dokter | **`FAIL`** — `500` | **Lingkungan, bukan kode:** server basis data `160.22.250.77:5432` tidak terjangkau 08.48.05–08.48.48 UTC; permintaan gagal di `AccessPermissionFilter` → `AccessPermissionService.HasAccessAsync` (`TimeoutException`), sebelum mencapai `RadOrderService`; enam *worker* latar gagal pada detik yang sama (log Serilog `quilvian-backend-20261027.json`). Cek *jumlah pesanan tidak berubah* pada baris ini juga tidak sah (GET daftar ikut `500`). Diukur ulang: U-A10 |
| A10 rad dokter luar `403`; A10 rad DPJP `Pending` | `PASS` | — |
| A9 lab oleh dokter | `PASS` | `NotRequired`; diturunkan dari data mentah: worklist Jenah (200, 08.49.07) hanya memuat pesanan A1 — pesanan A9 tidak masuk |
| A1 worklist dan verifikasi | **`FAIL`** | Pesanan A1 tampil tepat sekali di worklist Jenah (`PASS`); `PUT verify-instruction` `403` *"Anda tidak memiliki akses ke menu atau fitur ini."* — gerbang izin, bukan aturan service: posisi dokter IGD tidak memegang `LabOrder : Verify` (izin `IGD-DEC-352` hanya memberi 11 butir tanpa `Verify`). Tidak diukur ulang — memerlukan pemberian izin (`IGD-OQ-145`) |
| A2 lab dokter jaga | Tidak berjalan | Skrip melewatinya karena tidak ada dokter jaga |

Ketiga pesanan uji run 1 dibatalkan `200`.

**Uji ulang (`uji-ulang.mjs` 08.57 UTC; `uji-ulang-k6.mjs` sesudahnya).**

| Pemeriksaan | Kriteria | Verdict | Catatan |
| --- | ---: | --- | --- |
| U-A10 rad tanpa dokter | 3, 10 | `PASS` | `400` *"Pilih dokter yang memberi instruksi."*; jumlah pesanan tetap (GET 200) |
| K5 pilihan kunjungan tanpa DPJP dan tanpa dokter jaga | 5 | `PASS` | `200`, `[]` |
| K5 lab dengan Jenah (DPJP kunjungan lain); lab dengan B4 IGD Utama; rad dengan Jenah | 5, 10 | `PASS` ×3 | `403` pesan rawat inap; nol pesanan |
| K1 penginput terpisah | 1 | `PASS` | Lab `requestedByUserId` = perawat, `instructingDoctorId` = Jenah; rad `requestedByName` = perawat |
| K10 rad di worklist DPJP | 1, 10 | `PASS` | Tepat sekali |
| K6 pengalihan DPJP Jenah → B4 IGD Utama | 6 | `PASS` | `200`; DPJP aktif berganti; pilihan menjadi `[B4 IGD Utama, basis 1]` |
| K6 worklist sesudah pengalihan | 6 | `PASS` — **dua dari empat cek salah rancang** | Lab dan rad tetap di worklist Jenah (`200`) — sah. Cek *tidak di worklist DPJP baru* lolos palsu: worklist dr. B4 IGD Utama dijawab `403` (akun tanpa `LabOrder`/`RadOrder : Read`) dan skrip membaca respons gagal sebagai daftar kosong. Verdict tidak diubah; diukur ulang dengan arah dibalik (K6b) |
| K6 lab dengan Jenah sesudah pengalihan | 6, 8 | `PASS` | `403` — keabsahan mengikuti DPJP saat pesanan dibuat |
| K6 lab dengan DPJP baru | 6, 8 | `PASS` | `Pending`, instruksi B4 IGD Utama |
| K6 verifikasi lab, rad | 6, 10 | `NOT RUN` | Posisi dokter IGD tanpa `LabOrder : Verify`, `RadOrder : Verify` |
| K9 lab oleh dr. B4 IGD Utama (akun tertaut kedua) | 9 | **`FAIL`** | `403` gerbang izin — akun ini tidak memegang `LabOrder : Create` walaupun `/auth/me` menunjukkan departemen dan posisi yang sama dengan Jenah (izin dihitung dari baris penempatan `ApplicationUserOrganizations`, bukan dari profil). Prasyarat skrip, bukan aturan 13; tidak dapat diukur ulang tanpa mengubah penempatan akun |
| K11 rawat inap: lab dan rad tanpa dokter `400`; dengan dokter tidak bertugas `403` | 11 | `PASS` ×4 | Episode `d7d0de51…` (satu-satunya data rawat inap: lima episode, semuanya `Cancelled`). Aturan instruksi dijalankan tepat sesudah cek encounter dan kecocokan episode, tanpa cek status, sehingga penolakan ini menguji cabang episode; nol tulis |
| K11 rawat inap jalur `Pending`; K11 rawat jalan; K2 dokter jaga | 11, 2 | `NOT RUN` | Membuat pesanan pada episode `Cancelled` tidak dilakukan; nol encounter rawat jalan (`EncounterType` 1) pada 100 encounter terbaru dan `outpatient-encounters` 1–28 Oktober; nol jadwal jaga dokter IGD |
| K6b DPJP → B4 IGD Utama; pesanan lab dan rad atas instruksinya `Pending`; DPJP kembali ke Jenah | 6 | `PASS` ×4 | — |
| K6b lab, rad tidak di worklist DPJP baru (Jenah) | 6, 10 | `PASS` ×2 | Worklist `200` diperiksa eksplisit; pesanan tidak tampil; instruksi tetap B4 IGD Utama, status tetap `Pending` |

Audit seluruh GET ketiga log: non-`200` hanya penolakan yang disengaja (A7 `400`/`404`, `active` `404` kunjungan tanpa
DPJP), GET saat gangguan basis data (run 1), dan GET dr. B4 IGD Utama (`403`) — tidak ada lolos palsu lain.

**Jejak data yang ditinggalkan.** Pesanan uji: 3 (run 1) + 3 (uji ulang) + 2 (K6b), seluruhnya dibatalkan `200`. Kunjungan
Aldo Barbeto memperoleh empat baris riwayat DPJP baru (Jenah → B4 IGD Utama → Jenah, dua kali) dengan alasan uji; DPJP akhir
dr. Jenah Ali (terverifikasi), tetapi `effectiveFrom` penugasan berjalan kini 27 Oktober 2026, bukan 23 Oktober. Rad oleh
dokter (`uji-ulang-k10.mjs`) tidak berjalan 27 Oktober: backend sudah berhenti (port 7184 `ECONNREFUSED`) sebelum login — nol tulis.

**K10 rad oleh dokter (28 Oktober, backend dinyalakan Rizki 09.28, DLL sama).** `201`, `NotRequired`, tanpa dokter pemberi
instruksi, tidak masuk worklist (worklist `200`) — `PASS`. Cek *jumlah bertambah satu* `FAIL` karena **salah rancang**: daftar
rad dijawab sebagai array polos tanpa `totalData`, sedangkan skrip membaca `data.totalData ?? 0`. Verdict tidak diubah;
diukur ulang dari array mentah yang sama di `40-jaringan-api.json`: 3 → 4 pesanan, pesanan baru hanya ada pada daftar
sesudah — `PASS`. Pesanan dibatalkan `200`. Bukti `40-hasil.json`.

### 5.2 Sesudah `IGD-OQ-145` dipenuhi — 28 Oktober 2026 (siang)

Atas perintah Rizki, agen memakai akun SuperAdmin: (a) `LabOrder : Verify` dan `RadOrder : Verify` ke posisi Medis / Dokter Umum
(234 → 236 izin, nol izin lama hilang, cadangan `10-policy-dokter-umum-sebelum-terapkan.json`); (b) `PUT` nilai-sama atas
penugasan Medis / Dokter Umum dr. B4 IGD Utama memicu proyeksi izin yang belum pernah terbentuk (izin efektif 53 → 262 —
53 sama dengan izin posisi Medis / Dokter IGD, satu-satunya penugasan yang sebelumnya terproyeksi); (c) jadwal uji satu hari
`DSCH-RSMMC-00007` (28 Oktober 00.00–23.59, Klinik Gawat Darurat (IGD), tanpa kuota, walk-in, appointment, kiosk). Bukti
`test-with-agy/igd/langkah-oq145-20261028/`.

`uji-ulang-oq145.mjs` — 16/16 `PASS`: `K7` urutan `[Jenah 1, Utama 2]` dan dokter rangkap tampil sekali; `K2` lab dan rad
dengan dokter jaga `Pending`; `K9` akun dokter kedua `NotRequired`; `K6`/`K10` verifikasi oleh dokter pemberi instruksi
`200` → `Verified` atas namanya dan hilang dari worklist, juga sesudah pengalihan; DPJP baru tidak melihat; dokter lain `403`
aturan service. Audit: 43 GET seluruhnya `200`. DPJP Aldo dikembalikan ke dr. Jenah Ali; tujuh pesanan dibatalkan `200`.
Bukti `50-hasil.json`, `50-jaringan-api.json`.

**Layar (28 Oktober).** Uji layar `FE-IGD-057` membuktikan kriteria 1, 3, 5 dan 10 dari sisi perawat: pilihan =
respons `instructing-doctor-options`, payload membawa dokter terpilih, `201` `Pending`, tanpa dokter nol `POST`, kunjungan
tanpa dokter sah tidak menawarkan simpan; `FE-IGD-058` menampilkan pesanan itu di daftar dokter pemberi instruksi
(`test-with-agy/igd/uji-fe-igd-057-058-20261028`).

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria (kartu revisi 16) | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Perawat memesan lab dengan DPJP IGD aktif → `Pending`, penginput terpisah; masuk worklist dokter itu (`AT-IGD-209`) | **Terpenuhi** (API dan layar) | A1, K1, A1-worklist cek 1; layar `057-lab-pesan`, `058-ukur-ulang-a1.txt` |
| 2 | Sama dengan dokter jaga IGD aktif | **Terpenuhi** | 28 Oktober: dr. B4 IGD Utama dokter jaga (jadwal uji `DSCH-RSMMC-00007`) → lab dan rad `Pending`, instruksi Utama (`K2-lab-dokter-jaga`, `K2-rad-dokter-jaga`, `50-hasil.json`) |
| 3 | Tanpa dokter → `400`; nol pesanan (`AT-IGD-210`) | **Terpenuhi** | A3 (lab), U-A10 (rad) |
| 4 | Dokter bukan DPJP/dokter jaga → `403`; nol pesanan (`AT-IGD-249`) | **Terpenuhi** | A4 (lab), A10 dokter luar (rad) |
| 5 | Kunjungan tanpa DPJP dan dokter jaga: daftar kosong; pesanan perawat `403` (`AT-IGD-250`) | **Terpenuhi** | K5 ×4 |
| 6 | Pengalihan DPJP: dokter pemberi instruksi tetap memverifikasi; DPJP baru tidak melihatnya (`AT-IGD-250`) | **Terpenuhi** | 28 Oktober: sesudah DPJP dialihkan ke Utama, worklist Utama `200` tanpa pesanan; Jenah memverifikasi lab dan rad `200` → `Verified`, `instructionVerifiedByUserId` = Jenah, hilang dari worklistnya; Utama memverifikasi pesanan instruksi Jenah → `403` aturan service *"Hanya dokter pemberi instruksi yang dapat memverifikasi pesanan ini."*. Sebelumnya K6/K6b worklist |
| 7 | Isi dan urutan `instructing-doctor-options`; `404`; `400` | **Terpenuhi** | A7 (`400`, `404`, dokter luar tidak tampil); 28 Oktober: `[Jenah basis 1, Utama basis 2]`; Utama sebagai DPJP sekaligus dokter jaga → tampil sekali `basis 1` (`K7-*`) |
| 8 | Isi daftar = hasil pemeriksaan (satu fungsi penentu) | **Terpenuhi** | Diff (satu fungsi `HitungDokterPemberiInstruksiSahAsync`) + API sisi DPJP: anggota daftar lolos (A1, K6 DPJP baru), bukan anggota ditolak (A4, K5, K6 DPJP lama) |
| 9 | Dokter memesan → `NotRequired`, tidak masuk worklist | **Terpenuhi** | A9 (Jenah) dan 28 Oktober `K9-lab-oleh-dokter2-tertaut` (Utama) `NotRequired`. Risiko akun tak tertaut: bagian 7 |
| 10 | Radiologi sama dengan 1–9 | **Terpenuhi** | 1, 3, 4, 5, 6, 9 sebelumnya; 2 (`K2-rad-dokter-jaga`) dan verifikasi (`K10-rad-verifikasi-tanpa-pengalihan`, `K6-rad-*`) 28 Oktober |
| 11 | Regresi rawat inap dan rawat jalan | **Terpenuhi** (diff + uji API data yang ada) | Diff: cabang episode tidak disunting; encounter tanpa `EmgVisit` → `NotRequired` seperti dahulu. API: rawat inap tanpa dokter `400`, dokter tidak bertugas `403` (lab dan rad). Jalur `Pending` rawat inap dan rawat jalan tidak diuji karena datanya tidak ada di dev — sesuai bunyi kartu *"Diff + uji API bila data ada"*, diterima Rizki 28 Oktober (`IGD-DEC-371`) |
| 12 | Diff tiga berkas IGD + dua service Lab/Rad; nol `Program.cs`, migration, komentar baru; QBE lulus | **Terpenuhi** | Bagian 3.2, 5 |
| 13 | Build 0 error; warning dilaporkan | **Terpenuhi** | Build Rizki: DLL 15.29.22 berisi kode baru dan dijalankan (perilaku baru teramati pada uji API); 422 warning dengan `-p:RunAnalyzers=false`, tanpa pembanding |

**DoD.** Acceptance 1–13 terpenuhi; laporan ini ada; register, kartu, grafik, dan traceability ditandai ✅ 28 Oktober 2026; uji bersama `FE-IGD-057`, `FE-IGD-058` selesai (lulus). Rilis (commit, merge) menunggu perintah Rizki.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perintah build untuk Rizki | Sudah dijalankan Rizki 27 Oktober 2026 (bagian 5). *Sebelumnya:* hentikan backend, `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false`, jalankan ulang |
| Risiko kriteria 9 — dibuktikan 27 Oktober 2026 | **Nyata, sesuai rancangan.** `ResolveActorDoctorIdAsync` mengenali dokter lewat empat jalan: `Users.DoctorId` dokter aktif, klaim `doctor_id`, profil workforce → `MstDoctor`, atau email yang sama dengan `MstDoctor.Email`. Akun yang gagal di keempatnya diperlakukan sebagai perawat: pada encounter IGD, pesanan tanpa `instructingDoctorId` ditolak `400` (dibuktikan A3 dengan akun `doctorId` kosong), dan bila menyebut dirinya sendiri hanya lolos selama ia DPJP/dokter jaga, lalu `Pending` menunggu verifikasinya sendiri. Kedua akun dokter uji tertaut (Jenah `NotRequired` pada A9), sehingga tidak ada akun dokter tak tertaut yang dapat diuji langsung. Sebelum rilis, admin memastikan setiap akun dokter IGD tertaut lewat salah satu dari empat jalan itu — langkah operasional, bukan kode |
| Peringatan | Komentar dokumentasi lama `ResolveInstructionAsync` (Lab) dan blok instruksi `CreateCoreAsync` (Rad) masih menyebut *"pesanan poliklinik dan IGD melewati blok ini"* — kini tidak tepat untuk IGD; tidak disunting karena aturan pemilik melarang menyunting komentar lama |
| Masalah yang diketahui | Jalur `POST /lab-orders/by-examinations` tidak memanggil penentu instruksi sama sekali — juga untuk rawat inap (perilaku milik Laboratorium, sebelum task ini). Layar IGD tidak memakai jalur itu (`emergency-assessment-slice.jsx` memanggil `POST /lab-orders`). Dicatat, tidak diperbaiki |
| Risiko tersisa | (1) **Memutus** bagi perawat IGD sampai `FE-IGD-057` dirilis: layar hari ini mengirim DPJP otomatis sehingga pesanan pada jam tanpa DPJP ditolak; rilis bersama wajib (R3.16.5). (2) Akun dokter yang tidak tertaut ke data dokter (`ResolveActorDoctorIdAsync` kosong) kini diperlakukan seperti perawat pada encounter IGD — pesanan dokter tanpa `instructingDoctorId` ditolak `400`. (3) `RadOrderService` tanpa service IGD (hanya harness uji lama) menolak pesanan tanpa episode dengan `InvalidOperationException` — tidak terjadi lewat DI |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` lima berkas source pada 3.2 (belum di-commit); dokumen task ini. Bukti uji di `test-with-agy` frontend (tidak ikut git) |
| Prasyarat `IGD-OQ-145` | Dipenuhi 28 Oktober 2026 (bagian 5.2). Jadwal uji `DSCH-RSMMC-00007` hanya berlaku 28 Oktober — uji dokter jaga di hari lain butuh jadwal baru |
| Langkah berikutnya | Commit `BE-IGD-071` (5 berkas source + dokumen) bersama `FE-IGD-057`/`058` atas perintah Rizki; Rizki mengabari Yoga isi diff `LabOrderService`/`RadOrderService` |
