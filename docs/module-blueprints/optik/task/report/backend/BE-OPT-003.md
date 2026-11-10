# Laporan Perubahan Backend — `BE-OPT-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-003` |
| Judul | Rujukan kunjungan asal |
| Slice | `MVP-1` — `EPIC OPT-01` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-003` |
| Trace | `FR-OPT-011`; `OPT-DEC-037`, `039`; `INT-OPT-01`, `INT-OPT-10`; `OPT-AC-023`; `UAT-02` |
| Contract version | `OPT-INT-v1` `INT-OPT-01`, `INT-OPT-10`; `OPT-API-v1` grup Opt Visit — `approved` sisi Optik |
| Penyetuju modul lain | Registrasi `OPT-APR-008`, `010` ✅ — Muhammad Hamzah (`OPT-DEC-052`, `OPT-EVD-002`), disampaikan Yoga Aji Pratama |
| Dependency | Tidak ada task Optik. `BE-OPT-010` ✅ sudah menyediakan isian `ReferralSourceEncounterId` yang sebelumnya ditolak |
| Klasifikasi | `MEDIUM` — satu kolom pada tabel milik Registrasi, satu migration, validasi di service bersama, proyeksi tampilan di Optik |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RegistrationManagement/{Models,DTOS,Services}/`, `Repositories/Configurations/HealthServices/RegPatientEncounterConfiguration.cs`, `Migrations/`, `Areas/HealthServices/OpticalManagement/{Services,DTOs}/`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`) |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Migration **dibuat, belum diterapkan** ke basis data mana pun. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (pemilik kolom dan service); `HealthServices` / `OpticalManagement` (pemakai) |
| Registry | `Reg` (Registrasi) dan `Opt` / `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` untuk `RegPatientEncounter`, configuration, DTO, dan `EncounterIntakeService` (hanya baris tambahan yang dinilai); `NEW CODE` untuk uji `Be003ReferralSourceTests` |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-CFG-001` (configuration khusus yang sudah ada diperluas), `QBE-DB-001` (migration bernama, FK `Restrict`), `QBE-VAL-001`, `QBE-SVC-001`, `QBE-LOG-001` |
| Wewenang terpisah | Source ✅ · pembuatan migration ✅ (task roadmap + preferensi pemilik 2026-10-05 di `AGENTS.md`) · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Kunjungan Optik hasil rujukan Poli Mata belum bisa menunjuk kunjungan Poli Mata asalnya. Akibatnya riwayat
"resep kacamata ini lahir dari pemeriksaan mana" tidak bisa ditelusuri (`OPT-DEC-037`), dan isian tautan di
pendaftaran Optik selama ini ditolak dengan pesan "belum tersedia".

---

## 2. Proses bisnis

1. **Dokter Poli Mata merujuk ke Optik.** **Contoh:** dr. Bima (Sp.M) memeriksa Ibu Sari (samaran) di Poli Mata pada kunjungan `ENC-RSMMC-00100`, lalu merujuknya ke Optik.
2. **Petugas Optik mendaftarkan kunjungan dengan tautan.** Petugas memilih Ibu Sari dan kunjungan `ENC-RSMMC-00100` sebagai asal rujukan. Registrasi memeriksa bahwa kunjungan itu ada dan milik Ibu Sari, lalu menyimpan tautannya pada kunjungan Optik yang baru.
3. **Tautan tampil di daftar dan ringkasan kunjungan Optik (`OPT-AC-023`).** Contoh tampilan: "Rujukan dari `ENC-RSMMC-00100`, dr. Bima, Sp.M".
4. **Tanpa surat rujukan.** Rujukan antar-unit di rumah sakit yang sama tidak membawa surat atau instansi perujuk, sehingga tautan ini **tidak** mewajibkan penanda rujukan luar (`IsReferral`). Dokter dan instansi perujuk dari luar tetap dapat diisi seperti sebelumnya.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Kunjungan asal milik pasien lain (`UAT-02`) | `400` `OPT-REG-REJECTED` "Kunjungan asal rujukan bukan milik pasien ini. Pilih kunjungan pasien yang sama." Tidak ada kunjungan terbentuk |
| Kunjungan asal tidak ada atau sudah dihapus | `400` `OPT-REG-REJECTED` "Kunjungan asal rujukan tidak ditemukan." |
| Pemanggil MCU mengisi tautan | Ditolak Registrasi: "Tautan kunjungan asal rujukan hanya berlaku untuk kunjungan penunjang." |
| Simpan ditekan dua kali dengan kunci sama | Satu kunjungan; jawaban kedua `IsReplay = true` dan tetap membawa tautan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EncounterIntakeService.cs` (urutan validasi, `ValidateReferralAsync`, `ResolveVisitProfileAsync`, `PersistAsync`, balapan idempoten), `RegPatientEncounter.cs` (`SourceEncounterId` bermakna transfer), `RegPatientEncounterConfiguration.cs`, `EncounterIntakeDtos.cs`, `OptVisitService.cs`, `OptVisitDtos.cs`, pemanggil lama `LabPatientRegistrationService`, `McuCheckInService`, `McuExaminationService`, `RadReportService`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs` | **Diperbarui.** `ReferralSourceEncounterId` (`Guid?`) + navigasi `ReferralSourceEncounter` |
| `Repositories/Configurations/HealthServices/RegPatientEncounterConfiguration.cs` | **Diperbarui.** Relasi ke diri sendiri `Restrict` + index |
| `Areas/HealthServices/RegistrationManagement/DTOS/EncounterIntakeDtos.cs` | **Diperbarui.** `EncounterIntakeRequest.ReferralSourceEncounterId` opsional |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs` | **Diperbarui.** `ValidateReferralSourceEncounterAsync` (jalur penunjang saja; kunjungan ada, tidak terhapus, pasien sama) dan penyimpanan kolom. Kosong atau `Guid.Empty` = tidak diperiksa |
| `Areas/HealthServices/OpticalManagement/Services/OptVisitService.cs` | **Diperbarui.** Penolakan "belum tersedia" dihapus; tautan diteruskan ke Registrasi; proyeksi daftar/ringkasan membaca nomor dan dokter kunjungan asal |
| `Areas/HealthServices/OpticalManagement/DTOs/OptVisitDtos.cs` | **Diperbarui.** Keterangan isian permintaan; tiga ruas jawaban baru (bagian 4) |
| `Migrations/20261104020212_AddEncounterReferralSource.cs` + `.Designer.cs` | **Baru.** `AddColumn` nullable, `CreateIndex`, `AddForeignKey` `Restrict`; `Down` membalik ketiganya |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | **Diperbarui.** +12 / −0 baris, hanya `RegPatientEncounter` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be003ReferralSourceTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 7 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/Be010VisitTests.cs` | **Diperbarui.** Uji `Rujukan_bertaut_ditolak_sampai_BE_OPT_003` dihapus karena perilakunya sengaja berubah; penggantinya ada di `Be003ReferralSourceTests` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Isian `ReferralSourceEncounterId` pada `POST /visits` kini diterima sesuai `OPT-API-v1`. Tambahan di luar kontrak, tanpa mengubah kontrak: `ReferralSourceEncounterId`, `ReferralSourceEncounterNumber`, `ReferralSourceDoctorName` pada `OptVisitListItemResponse` (juga terbaca lewat `OptVisitDetailResponse.Visit`) untuk `OPT-AC-023` |
| Kontrak integrasi | `INT-OPT-01`/`INT-OPT-10` terpenuhi. Dua aturan tambahan yang tidak tertulis eksplisit di kontrak: hanya jalur `Outpatient` (sesuai `02-backend-architecture.md` baris `EncounterIntakeService` "pada jalur penunjang") dan kunjungan asal terhapus ditolak |
| Database | Satu kolom `uuid NULL`, satu index, satu FK ke `RegPatientEncounter` sendiri `ON DELETE RESTRICT`. Baris lama tetap `NULL`; tanpa backfill. **Belum diterapkan** |
| Keamanan/Auth | Tidak ada izin baru. Validasi pasien sama mencegah riwayat pasien lain tertaut ke pasien ini. Audit pendaftaran tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Optical Management / Opt Visit

Base URL: `api/v1/health-services/optical-management/visits` — tidak ada endpoint baru; bentuk berubah sebagai berikut.

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | `ReferralSourceEncounterId` diterima dan divalidasi Registrasi | `OptVisit : Create` + Registrasi `PatientEncounter : Create` |
| `GET` | `/` dan `/{encounterId}` | Jawaban memuat `ReferralSourceEncounterId`, `ReferralSourceEncounterNumber`, `ReferralSourceDoctorName` (kosong bila bukan rujukan bertaut) | `OptVisit : Read` |

Contoh potongan jawaban:

```json
{
  "encounterNumber": "ENC-RSMMC-00101",
  "isReferral": false,
  "referralSourceEncounterId": "3f0c…",
  "referralSourceEncounterNumber": "ENC-RSMMC-00100",
  "referralSourceDoctorName": "dr. Bima, Sp.M"
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet`, `node`, atau `QuilvianSystemBackend` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error; 424 warning, tidak satu pun dari berkas yang disentuh; 2 menit 4 detik | `PASS` | Keluaran build |
| `dotnet ef migrations add AddEncounterReferralSource --no-build` | Berhasil | `PASS` | `20261104020212_AddEncounterReferralSource` |
| Isi migration | 1 `AddColumn` nullable, 1 `CreateIndex`, 1 `AddForeignKey` `Restrict`; tidak ada tabel lain | `PASS` | Tinjauan `Up`/`Down` |
| `dotnet ef migrations script 20261103050001_AddOpticalManagement 20261104020212_AddEncounterReferralSource --no-build` (luring) | SQL sama dengan DDL kamus data, kecuali nama FK (bagian 7) | `PASS` | Skrip di scratchpad sesi |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 129 lulus, 0 gagal (7 baru, 1 lama dihapus) | `PASS` | Keluaran `dotnet test` |
| Kriteria (1): rujukan ke kunjungan pasien sama | Diterima; tersimpan di `ReferralSourceEncounterId`, `SourceEncounterId` tetap kosong, `IsReferral` tetap `false`; nomor dan dokter asal tampil di daftar dan ringkasan | `PASS` | Uji `Kriteria_1_…` dengan `EncounterIntakeService` asli |
| Kriteria (2) / `UAT-02`: kunjungan pasien lain | `400 OPT-REG-REJECTED`; jumlah kunjungan tidak bertambah | `PASS` | Uji `Kriteria_2_…` |
| Kunjungan asal tidak ada / terhapus | `400` "Kunjungan asal rujukan tidak ditemukan." | `PASS` | Uji teori dua kasus |
| Simpan dua kali dengan tautan | Satu kunjungan Optik; jawaban kedua `IsReplay = true` dengan tautan yang sama | `PASS` | Uji |
| Kriteria (3): pemanggil lama | Panggilan bentuk Lab/Rad tanpa isian baru → kolom `NULL`, `Outpatient`, tanpa antrean; `Guid.Empty` diperlakukan kosong | `PASS` | Uji `Kriteria_3_…` |
| Jalur MCU dengan tautan | Ditolak dengan pesan jalur penunjang; tidak ada kunjungan terbentuk | `PASS` | Uji |
| Pemanggil lama tidak menyetel ruas baru | `ReferralSourceEncounterId` hanya muncul di 6 berkas task ini; Lab, MCU, Rad tidak menyentuhnya | `PASS` | `grep` |
| QBE checker `-Mode Strict -Path` atas 6 berkas source | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif: `ZzzRegistryProbe` ber-`[Table]` di folder tak terdaftar | Terkena QBE-ENT-001, QBE-CFG-001, QBE-MOD-002 | `PASS` | Keluaran checker; probe dihapus, `git status --short Areas/` hanya berkas task |

**Tidak dijalankan:**
- `dotnet ef database update` atau skrip SQL apa pun ke basis data mana pun (wewenang database tidak diberikan).
- Penegakan FK `Restrict` di basis data: harness SQLite sengaja mematikan FK. Penegakannya terbukti dari isi migration dan snapshot saja.
- Panggilan HTTP ke backend yang menyala (lihat risiko di bagian 7).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Rujukan ke kunjungan pasien yang sama diterima dan tersimpan | Terpenuhi di harness | Uji `Kriteria_1_…` |
| (2) Rujukan ke kunjungan pasien lain ditolak `400` | Terpenuhi di harness | Uji `Kriteria_2_…` (`UAT-02`) |
| (3) Pemanggil lama (Lab, Rad, MCU) tidak berubah perilaku | Terpenuhi | Uji `Kriteria_3_…`; `grep` pemanggil; 129 uji Optik lulus |
| `OPT-AC-023` tampilan kunjungan asal dan dokter perujuk | Terpenuhi di harness | Uji `Kriteria_1_…` membaca ringkasan |
| DoD: migration belum diterapkan tanpa izin | Terpenuhi | Tidak ada perintah database dijalankan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan — wajib dibaca sebelum deploy | Kolom ini ada di tabel kunjungan yang dibaca **seluruh modul**. Bila kode ini berjalan di server yang basis datanya belum menerima migration `20261104020212_AddEncounterReferralSource`, setiap pembacaan entity `RegPatientEncounter` gagal "column ReferralSourceEncounterId does not exist", bukan hanya layar Optik. Terapkan migration lebih dulu atau bersamaan dengan deploy |
| Delta terhadap kamus data | Nama FK di DDL kamus data `FK_RegPatientEncounter_RegPatientEncounter_ReferralSourceEncounterId` (68 karakter) melebihi batas identifier PostgreSQL 63 karakter. EF memotongnya menjadi `FK_RegPatientEncounter_RegPatientEncounter_ReferralSourceEncou~`. Kamus data terkunci hash; penyegarannya dicatat untuk pass blueprint berikutnya |
| Keputusan pelaksanaan yang perlu dikonfirmasi | (a) Tautan tidak mewajibkan `IsReferral`. (b) Hanya jalur penunjang. (c) Kunjungan asal berstatus batal atau tidak hadir **tetap diterima**, karena kontrak hanya mensyaratkan pasien sama; bila Rawat Jalan ingin menolaknya, itu keputusan pemilik Registrasi/Rawat Jalan. (d) Unit kunjungan asal tidak dibatasi ke Poli Mata |
| Risiko tersisa | ~~Migration belum pernah dijalankan di PostgreSQL~~ — lihat baris berikut |
| Penerapan ke devYoga (2026-11-04, atas instruksi pemilik) | `migrations list`: hanya migration ini yang tertunda (`AddOpticalManagement` sudah diterapkan 2026-11-03). Skrip `<AddOpticalManagement> <AddEncounterReferralSource>` naik dan turun dibaca; uji kering naik → turun dalam satu transaksi lalu `ROLLBACK` (riwayat 379 → 380 → 379); lalu diterapkan dengan `COMMIT`. Hasil di `QuilvianNewDevYoga`: riwayat **380, nol tertunda**; kolom, index, dan FK ada; 185 baris kunjungan tidak berubah |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | `M` 5 berkas `Areas/` + `Repositories/Configurations/HealthServices/RegPatientEncounterConfiguration.cs` + `Migrations/ApplicationDbContextModelSnapshot.cs` + roadmap dan traceability Optik; `??` dua berkas migration baru dan laporan ini. Berkas uji di `Tests/` diabaikan Git |
| Langkah berikutnya | `MVP-1` backend selesai. Lanjut `MVP-2`: `BE-OPT-030` (perluasan Billing, penyetuju Yasmina) atau `BE-OPT-033` (jenis dokumen stok Optik, penyetuju Ikbal; bagian pengembalian stok menunggu `OPT-OQ-015`). Penerapan dua migration Optik ke devYoga sebagai izin terpisah |
