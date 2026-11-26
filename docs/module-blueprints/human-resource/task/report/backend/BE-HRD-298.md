# Laporan Penyelesaian Task: BE-HRD-298 Service Surveilans Bahaya Ruangan, TLD Radiasi & Worker Alarm Batas Dosis OHS-W5

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-298`
* **Nama Task**: Service Surveilans Bahaya Ruangan, TLD Radiasi & Worker Alarm Batas Dosis OHS-W5
* **Modul / Sub-Modul**: Human Resource / Sub-modul 19: Occupational Health & Staff Safety (K3RS & Medical Check-Up)
* **Slice Delivery**: `S-C6` (Kontrak API `v23`, Gelombang `MVP-58`)
* **Status**: `✅ Selesai (2026-11-18)` — `AT-HRD-OHS-17` terbukti lewat harness dan ambang dosis kini kumulatif sesuai `VR-OHS-17`; lihat bagian "Pengerjaan Ulang 18 November 2026" di akhir laporan
* **Deliverables**:
  - `Implementasi IHazardSurveillanceService, HazardSurveillanceService, dan RadiationDoseThresholdAlertWorker (OHS-W5).`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `OccupationalHealthManagement` (Sub-modul 19: K3RS & MCU)
* **Prefix Registry**: `Hrd` / `Trx` / `Wfp` (Kepatuhan Kamus Data Bagian 24)
* **Klasifikasi Kode**: `NEW CODE & EXTENSION`
* **Wewenang**: Implementasi backend sesuai kontrak terkunci `v23` dan aturan bisnis `VR-OHS-01` s.d. `VR-OHS-25`.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung (`dotnet ef database update`) ke database server development atau master tanpa otorisasi tertulis terpisah.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam tata kelola kesehatan kerja rumah sakit (K3RS), keselamatan fisik staf medis dan kepatuhan terhadap regulasi Kemenkes/Bapeten/BPJS memiliki implikasi hukum, klinis, dan keselamatan pasien yang kritis:
1. **Perlindungan Privasi Medis Staf (HRD-DEC-176, VR-OHS-01/02)**: Rekam medis staf dipisahkan secara ketat di tingkat kolom (*Field-Level Access Separation*). User berrole HR dan Manajemen hanya menerima status kelaikan operasional (*Fit*, *Fit with Restriction*, *Unfit*) tanpa diagnosis penyakit ICD-10, sedangkan rincian klinis terenkripsi kuat menggunakan AES-256.
2. **Kelaikan Kerja & Gembok Hard-Stop Roster (INT-HR-89, VR-OHS-03..07)**: Penerbitan status kelaikan kerja operasional oleh dokter bersertifikat Hiperkes aktif. Surat pembatasan tugas kerja (*Work Restriction*) maksimal 180 hari memicu gembok validasi ketat (*hard-stop*) pada Sub-06 Penjadwalan agar staf tidak ditugaskan pada shift malam atau zona bahaya. Gembok hanya dapat dicabut melalui asesmen *Return-to-Work* (RTW) resmi.
3. **Tanggap Darurat Tertusuk Jarum Cito & Terapi PPP < 4 Jam (INT-HR-91, VR-OHS-08..11)**: Penanganan insiden tertusuk jarum suntik bekas infeksius melalui tombol panik ESS dengan transmisi cito < 5 detik, pengawalan batas emas keselamatan terapi profilaksis pasca pajanan (PPP/PEP ARV/HBIG) dalam < 4 jam, serta jadwal surveilans serologi serial darah pada Bulan 1, 3, dan 6.
4. **Radar Imunisasi Nakes & Akreditasi STARKES (VR-OHS-12..17, AT-HRD-OHS-22)**: Pemantauan titer antibodi Anti-HBs kuantitatif (< 10 mIU/mL memicu recall booster), pemantauan dosis radiasi badge TLD kuartalan (> 5 mSv) dan tahunan (> 20 mSv), serta laporan pemenuhan standar STARKES instrumen KPS.
5. **Klaim Kecelakaan Kerja BPJS Ketenagakerjaan JKK (INT-HR-90, VR-OHS-18..21)**: Penegakan batas waktu pelaporan insiden KAK/PAK ke BPJS Ketenagakerjaan dalam 2x24 jam (Tahap I) dan reimbursement klaim (Tahap II).

---

## 4. Rincian Implementasi & Alur Teknis
* **Ringkasan Pekerjaan**: Layanan pemantauan bahaya spesifik lingkungan kerja rumah sakit (pencatatan dosis radiasi badge TLD kuartalan/tahunan staf radiologi & kedokteran nuklir). Dilengkapi background worker RadiationDoseThresholdAlertWorker (OHS-W5) yang memantau dosis kumulatif triwulanan (> 5 mSv) dan tahunan (> 20 mSv) sesuai regulasi Bapeten untuk memicu alarm bahaya dan rekomendasi rotasi dinas (VR-OHS-17).
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 24, dan matriks validasi Sub-modul 19.
* **Kriteria Penerimaan (Acceptance Criteria)**: Seluruh method service, model, DTO, worker, dan controller terimplementasi rapi, terisolasi dalam transaksi database ACID, dan lolos uji kompilasi serta conformance check.

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS** (0 Violation).
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1q (`AT-HRD-OHS-17`).


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok B — data karangan |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | 🟡 **SEBAGIAN 10 November 2026** (dikerjakan ulang; klaim ✅ 9 November 2026 diturunkan) |

### Masalah yang ditemukan audit

Radar dosis radiasi menampilkan `yearlyDose = 4,8 mSv` dan `quarterlyDose = 1,2 mSv` untuk semua pekerja, apa pun dosis yang dicatat. Radiografer dengan dosis 6 mSv tidak memicu alarm. Nama unit kerja juga teks tetap.

### Perubahan yang dikerjakan

- Kolom baru `MeasuredDoseMsv numeric(10,3)` pada `TrxEmployeeHealthSurveillance`, diisi saat endpoint `surveillance/radiation-dose` mencatat dosis.
- Radar menghitung dosis kumulatif tahunan = jumlah seluruh pembacaan tahun target; dosis triwulanan = jumlah pembacaan pada triwulan terakhir yang punya catatan. Catatan lama tanpa kolom itu dibaca dari teks `Dosis Tercatat: x mSv`.
- Nama unit kerja diambil dari departemen utama pegawai; hasil diurutkan dari dosis tertinggi.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/OccupationalHealthManagement/Services/HazardSurveillanceService.cs`
- `Areas/Corporate/HumanResource/OccupationalHealthManagement/Models/TrxEmployeeHealthSurveillance.cs`
- `Repositories/Configurations/Corporate/HumanResource/OccupationalHealthManagement/TrxEmployeeHealthSurveillanceConfiguration.cs`
- `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Radar dosis dihitung dari catatan dosis nyata | Terpenuhi di source | kolom `MeasuredDoseMsv` + agregasi |
| Alarm > 5 mSv/triwulan dan > 20 mSv/tahun (VR-OHS-17) | Terpenuhi di source | ambang tidak berubah, kini dibandingkan dengan dosis nyata |

### Yang belum terpenuhi

- Migration belum dijalankan; worker `RadiationDoseThresholdAlertWorker` tetap hanya menulis log (kelompok D, menunggu `BE-HRD-314`).

### Database dan migration

Migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`). Snapshot model EF tidak dipakai sebagai dasar migration ini karena sudah lama tidak sinkron dengan migration SQL manual slice sebelumnya (lihat catatan risiko).

### Catatan penutup

- Temuan: `SurveillanceNumber` masih dibentuk dari potongan GUID (`RAD-202611-A1B2`), bukan deret urut; tidak termasuk daftar kelompok E dan dibiarkan untuk task lanjutan.
- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (worker hanya menulis log)

`RadiationDoseThresholdAlertWorker` (`OHS-W5`) kini mengirim alarm prioritas kritis untuk setiap rekaman surveilans radiasi yang dinyatakan tidak memenuhi ambang (`IsCompliant = false`), berisi dosis terukur (`MeasuredDoseMsv`), ke pegawai radiasi dan ke pemegang hak akses `OccupationalMedicalExamination : RecordHazardSurveillance`, satu kali per rekaman.

Pusat pemberitahuan dalam aplikasi (keputusan Gelombang 2, `BE-HRD-314`): pemberitahuan disimpan di tabel `HrdNotification`, dibaca lewat endpoint `api/v1/self-service/hr/notifications`, dan didorong realtime lewat hub SignalR `/hubs/notifications`. Penerima "petugas" ditentukan dari hak akses yang diberikan admin di layar Akses Role (`SysAccessPolicy`), bukan nama peran di kode. Kunci anti-duplikat mencegah worker mengirim pengingat yang sama berulang.

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Status 🟡: migration `20261110050000` (kolom dosis) dan `20261111020000` (tabel pemberitahuan) belum dijalankan.


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.


## Pembaruan 11 November 2026 — Penuntasan Temuan

Nomor surveilans radiasi kini dari `NumberSeriesAllocator`, bukan potongan GUID.

`dotnet build` 0 error, 602 warning. QBE Strict `PASS`. Aplikasi dijalankan pada 11 November 2026: seluruh worker berjalan tanpa galat query dan lima dokumen Swagger berhasil dibuat. Uji API dengan akun sungguhan `NOT RUN`.

## Pengerjaan Ulang 18 November 2026 — `AT-HRD-OHS-17` dan ambang dosis kumulatif

| Field | Nilai |
| --- | --- |
| Task mode | `BACKEND` (instruksi pengguna 18 November 2026 "lanjut BE-HRD-298") |
| Commit backend saat dikerjakan | `d7d4eb3b` (branch `yoga`, sesudah merge `origin/QuilvianIntegrationBackend`) |
| Backend Governance Preflight | Area `Corporate`; Module `HumanResource` / `OccupationalHealthManagement`; prefix `Trx` (berkas lama `TrxEmployeeHealthSurveillance`); keberlakuan `TOUCHED LEGACY`; QBE berlaku `QBE-SVC-001`, `QBE-VAL-001`; tanpa entity baru, tanpa migration |
| Status | ✅ Selesai — `AT-HRD-OHS-17` terbukti lewat harness; satu cacat `VR-OHS-17` diperbaiki |

### Cacat yang ditemukan

`VR-OHS-17` menilai dosis **kumulatif**: triwulanan > 5 mSv wajib investigasi K3RS, tahunan > 20 mSv wajib rotasi. Kontrak API menyebut dosis TLD diimpor **bulanan** (`INT-HR-91`). Namun `RecordRadiationDoseAsync` hanya menilai satu bacaan (> 5 mSv), sehingga:

| Contoh | Sebelum perbaikan | Seharusnya |
| --- | --- | --- |
| Januari 3 mSv, Februari 3 mSv (triwulan 1 = 6 mSv) | Keduanya "Dalam batas aman", tanpa alarm | Bacaan Februari melampaui ambang triwulanan |
| Bacaan November 1,5 mSv yang membuat total tahun 20,5 mSv | "Dalam batas aman"; worker `OHS-W5` tidak memberi alarm (radar tetap menampilkan rekomendasi rotasi) | Tidak patuh, pesan wajib rotasi, alarm `OHS-W5` |

### Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/OccupationalHealthManagement/Services/HazardSurveillanceService.cs` | `RecordRadiationDoseAsync` menjumlahkan bacaan pegawai yang sama pada triwulan dan tahun periode bacaan (cara baca sama dengan radar, termasuk catatan lama "Dosis Tercatat: x mSv"). `IsCompliant` = tidak melampaui ambang triwulanan **dan** tahunan. Teks hasil kini memuat pesan `VR-OHS-17` ("Batas Dosis Radiasi Terlampaui (Bapeten): … wajib dirotasi sementara") atau "wajib investigasi K3RS", ditambah kumulatif triwulan dan tahun. "Dosis Tercatat: x mSv" tetap di awal teks sehingga pembacaan catatan lama tidak berubah. CRLF dipertahankan; +22 −2 |
| `Tests/QuilvianSystemBackend.HumanResourceTests/RadiationDoseThresholdTests.cs` (baru) | 3 uji atas `HazardSurveillanceService`, radar, worker `RadiationDoseThresholdAlertWorker` (`OHS-W5`), dan `HrdNotificationService` yang sesungguhnya |

"Tiket rotasi dinas" pada kartu roadmap tidak ada di kontrak; kontrak dan matriks uji meminta **rekomendasi rotasi**, yang diberikan radar (`RotasiWajibKeluarZonaRadiasi`).

### Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Januari 3 + Februari 3 mSv | Januari patuh; Februari tidak patuh, "kumulatif triwulanan 5 mSv; wajib investigasi K3RS", "Kumulatif triwulan 1/2026: 6.0 mSv" | `PASS` | `Kumulatif_triwulan_melebihi_5_mSv_dari_bacaan_bulanan` |
| `AT-HRD-OHS-17`: Februari, Mei, Agustus 5,0; Oktober 4,0 (tahun 19,0, semua patuh); November 1,5 | November tidak patuh dengan pesan `VR-OHS-17` wajib dirotasi, "tahun 2026: 20.5 mSv"; radar: 20,5 mSv, melampaui tahunan, `RotasiWajibKeluarZonaRadiasi` | `PASS` | `OHS17_kumulatif_tahunan_di_atas_20_mSv_alarm_dan_rekomendasi_rotasi` |
| — Worker `OHS-W5` dijalankan satu siklus | Alarm `Critical` kategori kesehatan kerja, merujuk rekaman November, ke akun pegawai **dan** petugas K3RS pemegang `OccupationalMedicalExamination : RecordHazardSurveillance`; isi menyebut dosis dan rotasi tugas | `PASS` | idem |
| — Worker dijalankan lagi | Tidak ada alarm ganda (tetap 2) | `PASS` | idem |
| Batas tahun, bacaan tunggal, catatan lama | Desember 2025 4,0 + Januari 2026 3,0 tidak dijumlahkan; satu bacaan 5,5 tetap tidak patuh; catatan lama tanpa kolom dosis ("4,2 mSv" di teks) ikut dijumlahkan (triwulan 3 = 5,2) | `PASS` | `Batas_tahun_bacaan_tunggal_dan_catatan_lama` |
| Kontrol negatif (berkas dipulihkan identik tiap putaran) | K1 service sebelum perbaikan → 3 uji gagal; K2 worker membalik syarat tidak patuh → uji alarm gagal; K3 worker tanpa kunci anti-duplikat → uji alarm ganda gagal | `PASS` | Keluaran `dotnet test` |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`, build penuh) | 287/287 lulus (284 + 3 baru) | `PASS` | Keluaran `dotnet test` |
| Pemeriksa QBE | Dua pemanggilan dengan probe kontrol (kena 2 `QBE-CODE-002` + 2 `QBE-SVC-001`), service tanpa temuan; `Strict`: 0 VIOLATION, 0 REVIEW, PASS; probe dihapus | `PASS` | Keluaran pemeriksa |
| Uji lewat API devYoga | Tidak dijalankan; uji harness tanpa menulis basis data (pola keputusan pemilik pada `BE-HRD-235`/`240`) | `NOT RUN` | — |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Rekaman dosis TLD tersimpan per periode | Terpenuhi | Rekaman per bulan dengan periode; kumulatif triwulan/tahun dihitung |
| Dosis melebihi ambang 20 mSv memicu alarm bahaya dan rekomendasi rotasi | Terpenuhi (harness) | Rekaman tidak patuh → alarm `OHS-W5`; radar `RotasiWajibKeluarZonaRadiasi` |
| `AT-HRD-OHS-17` | Terpenuhi lewat harness | Bagian verifikasi |

### Catatan

| Hal | Isi |
| --- | --- |
| Pengamatan (tidak diubah) | Teks alarm worker memformat dosis menurut budaya server ("1,5 mSv" pada Windows berbahasa Indonesia). Radar menghitung triwulan "terakhir yang punya catatan", sedangkan pencatat menilai triwulan periode bacaan — keduanya konsisten untuk impor berurutan |
| Perubahan sampingan | `NONE`. Probe QBE dihapus; tidak ada data devYoga yang ditulis |
| Status Git | 1 berkas source diubah, 1 berkas uji baru, laporan ini, roadmap/traceability/indeks uji. Tidak ada commit |
