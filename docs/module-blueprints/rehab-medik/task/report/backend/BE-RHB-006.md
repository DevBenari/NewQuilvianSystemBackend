# Laporan Perubahan Backend — `BE-RHB-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-006` |
| Judul | Lookup, pemeriksaan encounter, kewenangan profesi |
| Slice | S-A — Fondasi (`MVP-0`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-006` |
| Trace | FR-RHB-004; RHB-DEC-016; RHB-INV-01, 08; INT-RHB-01..04, 07 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Lookup* |
| Dependency | `BE-RHB-005` ✅ |
| Klasifikasi | `STANDARD` — endpoint baca baru dan layanan penolong, tanpa schema |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs,Constants}`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-05 |
| Status | **✅ Selesai** (2026-11-05). Build `PASS`, QBE Strict `PASS`, uji API manual 35/35 (dua fase), cek DB anti-IDOR `PASS`. Dua cabang hanya terbukti statis karena datanya tidak ada di DB dev; lihat §6. |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE` (2026-11-05). Tidak ada entity baru. |
| Jenis capability | **Read-only lookup** (arketipe transaksi read-only): seluruhnya `GET`, tanpa `options`/`status`/`DELETE` |
| QBE yang berlaku | QBE-NAM (prefix `Rhb`/`Rehab`), QBE-SVC-001 (controller tanpa `ApplicationDbContext`, tanpa generic repository), QBE-API-001, standar hak akses (konstanta `RehabilitationPermissions.Lookup`) |
| Lintas modul | Hanya **baca**: `RegPatientEncounter`, `InpEpisode`, `MstPatient`, `MstDoctor`, `MstProfession`, `MstProcedure`, `CliDoctorCertificate` (lewat `EncounterReferralService`), `CliClinicalInstrument` (lewat `ClinicalInstrumentService`). Tidak ada perubahan source modul lain. |

## 1. Masalah yang diperbaiki

Form permintaan dan sesi rehab perlu memilih pasien, kunjungan, surat rujukan, dokter, layanan,
tindakan, dan instrumen. Kunjungan dan pasien milik modul pendaftaran, dan petugas rehab sengaja
**tidak** diberi izin `PatientEncounter:Read` (RHB-DEC-016). Tanpa lookup milik rehab, form tidak
bisa diisi. Di sisi lain, kalau pilihan kunjungan tidak disaring per pasien, petugas bisa
menelusuri kunjungan pasien lain (IDOR).

**Contoh.** Petugas memilih pasien A. Daftar kunjungan hanya menampilkan 22 kunjungan A yang masih
terbuka. Kunjungan A yang sudah `Completed` tidak muncul, begitu juga kunjungan pasien B.

## 2. Proses bisnis

1. Petugas mengetik minimal 3 huruf nama atau nomor RM → daftar pasien, paling banyak 20.
2. Setelah memilih pasien → daftar kunjungan terbuka milik pasien itu, bisa disaring per jenis
   kunjungan. Surat rujukan internal yang masih bisa dipakai juga hanya milik pasien itu.
3. Dokter memilih layanan aktif → tindakan aktif layanan itu.
4. Saat asesmen → instrumen yang berlaku bagi pasien. Instrumen yang belum disahkan komite tetap
   tercantum, tetapi `isAvailable = false` beserta alasannya, sehingga layar beralih ke pengukuran
   terstruktur.
5. Layanan rehab berikutnya (`BE-RHB-007`, `009`, `012`) memakai `RhbServiceSupport.CheckEncounterAsync`
   untuk menolak kunjungan yang tidak sah (`RHB-VAL-051`/`052`), dan `RhbProfessionAuthority` untuk
   menolak penulis yang profesinya tidak sesuai (`RHB-VAL-031`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa (pola dan reuse)

- `HemodialysisManagement/Services/HmdServiceSupport.cs` — pola pemeriksaan kunjungan, kunci, nomor
- `ClinicalManagement/Services/InpatientClinicalContextService.cs` — resolusi profesi dari relasi akun
- `ClinicalManagement/Services/ClinicalInstrumentService.cs` — `ResolveForPatientAsync`, `IsDraftAllowedInThisEnvironment`
- `RegistrationManagement/Services/EncounterReferralService.cs` — `GetInternalLetterOptionsAsync`
- Model `RegPatientEncounter`, `InpEpisode`, `MstPatient`, `MstDoctor`, `MstEmployee`, `MstExternalUser`, `ApplicationUser`

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Services/RhbServiceSupport.cs` | Baru (190 baris) | `RhbEncounterCheck` (memisahkan *berakhir* dari *sudah pulang fisik* untuk kode 051/052), `OpenEncountersOfPatient`, `CheckEncounterAsync`, `NormalizePaging`, `AcquireLocksAsync`, `AllocateNumberAsync` (deret tahunan, 6 digit), `IsUniqueViolation` |
| `Services/RhbProfessionAuthority.cs` | Baru (121 baris) | `ResolveProfessionIdsAsync` (dokter + pegawai + pengguna eksternal, langsung atau lewat profil tenaga kerja, hanya profesi klinis aktif) dan `CanActForDisciplineAsync` (cocokkan `RhbDiscipline.ProfessionId`) |
| `Services/RhbLookupService.cs` | Baru (307 baris) | Tujuh operasi lookup |
| `DTOs/RehabLookupDtos.cs` | Baru (106 baris) | `RehabPatientLookupQuery`, `RehabPatientOption`, `RehabEncounterOption`, `RehabReferralLetterOption`, `RehabDoctorOption`, `RehabDisciplineLookupOption`, `RehabProcedureOption`, `RehabInstrumentLookupQuery`, `RehabResolvedInstrument` |
| `Controllers/RehabLookupController.cs` | Baru (106 baris) | 7 endpoint `GET` |
| `Constants/RhbErrorCodes.cs` | Diperbarui | **Delta:** `RHB-VAL-077` pencarian pasien kurang dari 3 karakter |
| `Program.cs` | Diperbarui | `AddScoped<RhbLookupService>()`, `AddScoped<RhbProfessionAuthority>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru *Rehab Lookup*, 7 endpoint sesuai kontrak v1 |
| Database | Tidak ada perubahan schema. Tidak ada migration. Endpoint lookup tidak menulis. |
| Keamanan/Auth | Satu Resource `RehabLookup`, aksi `Read`. Hak akses ditentukan admin di Akses Role. Kewenangan profesi melekat pada data (`MstProfession.Id`); tidak ada nama peran yang dibandingkan. |
| Privasi | Lookup pasien mengembalikan nama, nomor RM, tanggal lahir, dan jenis kelamin. Isinya dibatasi 20 hasil, dan endpoint baca tidak dicatat ke logger (`permission-audit-matrix.md` §6). |

## 4. Dokumentasi endpoint

**[Tags("Health Services / Rehabilitation Management / Rehab Lookup")]** — base URL
`/api/v1/health-services/rehabilitation-management/rehab-lookups`. Hak akses semua endpoint:
`RehabLookup : Read`.

| Method | Path | Request | Response | Kode status |
| --- | --- | --- | --- | --- |
| GET | `/patients` | `search` (≥ 3 karakter, wajib), `pageSize` (bawaan dan maks. 20) | `List<RehabPatientOption>` | 200, 400 (`RHB-VAL-077`, `fields: ["search"]`) |
| GET | `/encounters` | `patientId` (wajib), `encounterType?` (1 RJ, 2 IGD, 3 RWI, 4 MCU, 5 Tele) | `List<RehabEncounterOption>` | 200, 400 (`RHB-VAL-001`, `fields: ["patientId"]`) |
| GET | `/internal-referral-letters` | `patientId` (wajib) | `List<RehabReferralLetterOption>` | 200, 400 (`RHB-VAL-001`) |
| GET | `/doctors` | `search?` | `List<RehabDoctorOption>`, maks. 50 | 200 |
| GET | `/disciplines` | — | `List<RehabDisciplineLookupOption>` (aktif dan berprofesi) | 200 |
| GET | `/disciplines/{disciplineId}/procedures` | — | `List<RehabProcedureOption>` (aktif di layanan dan di master tindakan) | 200, 404 (layanan tidak ada/tidak aktif) |
| GET | `/instruments` | `patientId` (wajib), `disciplineId?` (kosong = asesmen medis) | `List<RehabResolvedInstrument>` | 200, 400 (`RHB-VAL-001`), 404 (pasien atau layanan) |

Contoh respons `/instruments` pada DB dev (instrumen nyeri baru berupa konsep):

```json
{
  "statusCode": 200,
  "data": [
    { "instrumentKind": 3, "instrumentKindName": "Skala Nyeri (NRS/VAS)", "isAvailable": false,
      "unavailableReason": "Instrumen Monitoring Nyeri (draft) belum disahkan komite.", "instrument": null },
    { "instrumentKind": 8, "instrumentKindName": "Indeks Barthel", "isAvailable": false,
      "unavailableReason": "Tidak ada instrumen untuk usia pasien ini.", "instrument": null }
  ]
}
```

**Kenapa `400` memakai `RHB-VAL-001`.** Artinya sama, yaitu "pilih pasien". Statusnya 400 karena
yang kurang adalah parameter query, bukan aturan bisnis permintaan (V-REQ-01 tetap 422).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE conformance `-Mode Strict` (6 berkas) | 0 violation untuk setiap berkas | `PASS` | Dijalankan agent 2026-11-05 |
| `dotnet build` Debug, perintah owner | `0 Error(s)`, 426 warning, 0 warning Rehab, `EXIT=0`, 18 menit 8 detik (14:53:01–15:11:17) | `PASS` | Log `build-BE-RHB-006.log` |
| Uji API manual fase 1 (backend dev, SuperAdmin, kredensial dibaca dari config tanpa dicetak) | 30/30 | `PASS` | Tabel di bawah |
| Uji API manual fase 2 (backend dinyalakan ulang dengan env `ClinicalConfiguration__AllowDraftVersionsForTesting=true`, tanpa mengubah berkas config maupun DB) | 5/5 | `PASS` | Tabel di bawah |
| Cek DB anti-IDOR (read-only) | Id kunjungan yang dikembalikan dicocokkan dengan `RegPatientEncounter` | `PASS` | Rincian di bawah |

Hasil uji API (2026-11-05):

| Pemeriksaan | Hasil |
| --- | --- |
| Tanpa login | 401 |
| `/patients?search=ab`, tanpa `search`, dan `search="  a  "` | 400 `RHB-VAL-077` / `["search"]` (×3) |
| `/patients?search=000&pageSize=50` | 200, 0 hasil (≤ 20) |
| `/patients?search=pasien&pageSize=5` | 200, 2 hasil |
| `/patients?search=ana&pageSize=1` (2 pasien cocok) | 200, **1** hasil, jadi pemotongan berjalan |
| `/patients?search=ana&pageSize=50` | 200, 2 hasil |
| `/encounters` tanpa `patientId` dan `patientId` = Guid nol | 400 `RHB-VAL-001` / `["patientId"]` |
| `/encounters` pasien A | 200, 22 kunjungan; kunjungan `Completed` A (`4a48f1aa…`) **tidak** muncul |
| `/encounters` pasien B | 200, 16 kunjungan, **tidak ada irisan** dengan A |
| `/encounters` pasien acak | 200, daftar kosong |
| `/encounters?encounterType=1` pasien A | 200, semuanya jenis 1 |
| `/internal-referral-letters` tanpa/ dengan pasien A | 400 `RHB-VAL-001` / 200 (0 surat di DB dev) |
| `/doctors`, `/doctors?search=zzzzqqq` | 200 (12 dokter) / 200 kosong |
| `/disciplines` (seed semuanya tidak aktif) | 200, 0 layanan; fisioterapi tidak muncul |
| `/disciplines/{fisioterapi tidak aktif}/procedures`, `/disciplines/{acak}/procedures` | 404, 404 |
| Layanan sementara `RHB_UJI_C1B144EF` aktif + profesi → `/disciplines` | Muncul dengan `professionId` yang benar |
| `/disciplines/{sementara}/procedures` | Tindakan aktif `fd23fd73…`; setelah dilepas → daftar kosong |
| `/instruments` tanpa pasien / pasien acak / layanan tidak aktif | 400 `RHB-VAL-001` / 404 / 404 |
| `/instruments` asesmen medis pasien A | Jenis `[3, 8]`; jenis 3 (konsep) `isAvailable=false`, alasan "belum disahkan komite"; jenis 8 tidak tersedia, alasan dari mesin instrumen |
| `/instruments` layanan sementara | Jenis `[3, 9]` |
| **Fase 2** — konsep diizinkan lingkungan | Jenis 3 `isAvailable=true`, `instrument.isApproved=false`, `isDraftAllowedInThisEnvironment=true`; jenis 8 tetap tidak tersedia |

Cek DB anti-IDOR:

| Kelompok | Milik A | Milik B | Tertutup | Dibatalkan | Jumlah |
| --- | --- | --- | --- | --- | --- |
| Hasil `/encounters` pasien A | ya | tidak | tidak | tidak | 22 |
| Hasil `/encounters` pasien B | tidak | ya | tidak | tidak | 16 |

Jumlah kunjungan terbuka pasien A langsung di DB juga 22, jadi tidak ada yang hilang.

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `/encounters` tanpa `patientId` → 400 | Terpenuhi (runtime) | §5 |
| (2) Hanya kunjungan milik pasien yang belum berakhir; kunjungan RWI pasien yang sudah pulang fisik tidak muncul | Terpenuhi. Kepemilikan dan status berakhir terbukti di runtime + DB. Cabang **RWI sudah pulang fisik terbukti statis**: tidak ada kunjungan RWI terbuka dengan `PhysicallyLeftAt` terisi di DB dev (query 0 baris). Filternya ada di `RhbServiceSupport.OpenEncountersOfPatient` dan `CheckEncounterAsync`, dengan logika yang sama seperti `HmdServiceSupport`. | §5 |
| (3) `/patients` minimal 3 karakter dan maksimal 20 hasil | Terpenuhi. Batas 3 karakter dan pemotongan hasil terbukti di runtime. Batas 20 terbukti statis (`Take`, dibatasi ke 20), karena di DB dev tidak ada kata kunci yang cocok dengan lebih dari 20 pasien. | §5 |
| (4) `/instruments` hanya mengembalikan versi `Approved`, atau konsep bila lingkungan mengizinkan | Terpenuhi (runtime, dua fase) | §5 |
| (5) `RhbProfessionAuthority` mencocokkan `MstProfession.Id`, bukan teks | Terpenuhi (statis): pencocokan memakai `RhbDiscipline.ProfessionId` terhadap kumpulan `ProfessionId`; tidak ada nama atau kode profesi yang dibaca. Belum ada endpoint pemakainya. Uji runtime-nya terjadi di `BE-RHB-009` (asesmen profesi, `RHB-VAL-031`). | `RhbProfessionAuthority.cs` |
| Verifikasi kartu: API manual dengan dua pasien (anti-IDOR) | Terpenuhi | §5 |
| DoD: build owner, laporan | Terpenuhi | §5, berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta kontrak | (a) Kode teknis `RHB-VAL-077` (pencarian kurang dari 3 karakter). (b) `RHB-VAL-001` dipakai dengan status 400 untuk parameter `patientId` yang kosong (§4). (c) Field DTO tidak dirinci kontrak (`DEV_DISCRETION`); bentuknya tercatat di §4. (d) `/instruments` mencantumkan juga jenis yang belum tersedia (`isAvailable=false` + alasan), supaya layar tahu harus beralih ke pengukuran terstruktur (RHB-DEC-017). |
| Keputusan desain | `RhbProfessionAuthority` mengumpulkan **seluruh** profesi klinis milik akun, sedangkan CPPT hanya mengambil satu menurut urutan. Alasannya: satu profil bisa punya baris dokter sekaligus pegawai, dan pencocokan dengan disiplin tidak boleh bergantung pada urutan. |
| Peringatan | Surat rujukan internal tidak disaring ke poliklinik rehab, karena kontrak hanya menyebut "surat rujukan internal pasien". Bila owner ingin hanya surat yang bertujuan poliklinik `ClinicType.Rehabilitation`, cukup tambahkan satu filter; ini keputusan bisnis. |
| Risiko tersisa | Cabang RWI pulang fisik dan batas 20 belum teramati di runtime (data dev tidak ada). |
| Sisa data uji | Layanan sementara `RHB_UJI_C1B144EF` (`fd0d0d0b-ab13-4b1d-a77a-f080e1930b1a`) terhapus lunak, beserta 1 tindakan nonaktif dan 2 instrumen, di `QuilvianNewDevSukma`. |
| Perubahan sampingan | `NONE`. Backend sudah dimatikan; port 7184 kosong. |
| Interupsi | `NONE` |
| Status Git | `M Program.cs`, `M Constants/RhbErrorCodes.cs`, `??` 5 berkas baru di `Areas/HealthServices/RehabilitationManagement/` |
| Langkah berikutnya | `BE-RHB-007` (permintaan, disposisi, buka episode) kini 🟡. Task itu juga menjalankan uji 409 `RHB-VAL-072` yang diteruskan dari `BE-RHB-005`. |
