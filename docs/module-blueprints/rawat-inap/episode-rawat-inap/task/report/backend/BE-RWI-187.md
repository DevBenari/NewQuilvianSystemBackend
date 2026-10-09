# Laporan Perubahan Backend — `BE-RWI-187`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-187` |
| Judul | Service baca pasien di `PatientManagement` |
| Slice | Slice A; gelombang `RWA-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-187` |
| Trace | `FR-RWA-003`, `021`, `052`, `082`; `RWI-DEC-252`, `257`, `259`, `264`, `266`; `INV-RWA-14`; backend 13.6, 13.8.4; `INT-RWA-01`, `02` |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`); perubahan service modul lain disetujui `RWI-DEC-266` |
| Dependency | — |
| Klasifikasi | `MEDIUM` — modul lain, hanya baca, ekstraksi tanpa perubahan perilaku |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/MasterData/Services/` (folder baru), `PatientController.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Perbandingan keluaran QR dan regresi endpoint pasien runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PatientManagement` (pemilik), dipanggil `InPatientManagement` |
| Prefix registry | `Mst` (entity yang dibaca) — `ACTIVE`; tidak ada entity baru |
| Keberlakuan | `NEW CODE` (service), `TOUCHED LEGACY` (`PatientController`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-MOD-001`, `QBE-NAM-001` |
| Wewenang | Service baca dan ekstraksi builder disetujui `RWI-DEC-266`; tidak ada tabel/endpoint `PatientManagement` yang berubah |

---

## 1. Kebutuhan

Workspace PPRI butuh identitas pasien, isi QR No. RM, relasi, dan kontak darurat tanpa menulis query
sendiri ke tabel master pasien (`INV-RWA-14`) dan tanpa memberi petugas hak `Patient : Read`
(`RWI-DEC-257`).

## 2. Proses bisnis

1. `GetIdentityAsync(patientId)` → No. RM terformat, nama, nama panggilan, tanggal lahir, jenis kelamin,
   agama, status nikah, jenis/nomor identitas, telepon, email, alamat beserta nama kecamatan, kota,
   provinsi, kode pos, penanda bayi baru lahir, nama ibu, dan isi QR. Pasien tidak ada/terhapus →
   kosong; pasien tanpa No. RM → isi QR kosong (tidak dikarang).
2. `GetPartyCandidatesAsync(patientId)` → relasi aktif dengan jenis terstruktur (utama lebih dulu), lalu
   kontak darurat aktif dengan teks hubungan apa adanya. Kontak yang bertanda "sama dengan alamat
   pasien" dan alamatnya kosong memakai alamat pasien. Kontak darurat tidak pernah dicocokkan lewat teks.
3. Isi QR dibentuk `PatientQrPayloadBuilder`. Builder ini diekstraksi apa adanya dari `PatientController`,
   dan controller kini memanggilnya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstPatient`, `MstPatientRelationship`, `MstPatientEmergencyContact`; `PatientController.BuildPatientQrPayload`
dan dua pembantunya.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `PatientManagement/MasterData/Services/PatientQrPayloadBuilder.cs` | Baru — `Build`, `NormalizeToRawDigits`, `FormatMedicalRecordNumber` (logika lama tanpa perubahan) |
| `PatientManagement/MasterData/Services/PatientProfileQueryService.cs` | Baru — `GetIdentityAsync`, `GetPartyCandidatesAsync`; `PatientIdentityProfile`, `PatientPartyCandidate`, `PatientPartyCandidateSource` |
| `PatientManagement/MasterData/Controllers/PatientController.cs` | Tiga pembantu privat mendelegasikan ke builder |
| `Program.cs` | `AddScoped<PatientProfileQueryService>` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru maupun berubah |
| Database | Tidak ada |
| Keamanan/Auth | Mengurangi kebutuhan hak baca master pasien bagi petugas Workspace PPRI |

## 4. Dokumentasi endpoint

Tidak ada endpoint baru (`RWI-DEC-264` butir 1). Endpoint pasien yang memuat QR tidak berubah bentuknya.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight; batas modul | Service milik `PatientManagement`, hanya `AsNoTracking` | `PASS` | Review source |
| Ekstraksi builder tanpa perubahan perilaku | Isi method dipindah utuh; controller mendelegasikan | `PASS` (review) | Diff `PatientController.cs` |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Perbandingan QR sampel dan regresi endpoint pasien | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `GetIdentityAsync` lengkap; pasien tidak ada/terhapus → kosong | Terpenuhi (source) | `PatientProfileQueryService.GetIdentityAsync` |
| 2. Isi QR builder sama dengan method lama | Terpenuhi (review ekstraksi) | Logika dipindah tanpa perubahan; uji sampel runtime dikecualikan |
| 3. Hanya relasi dan kontak aktif pasien itu; jenis terstruktur vs teks apa adanya | Terpenuhi (source) | Saringan `PatientId`, `IsActive`, `!IsDelete` |
| 4. Tidak ada tabel/endpoint `PatientManagement` berubah | Terpenuhi | Diff: hanya service baru dan delegasi privat |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `IdentityType` pasien tersimpan sebagai teks bebas; pemetaan ke enum pihak dokumen dilakukan di Workspace PPRI, bukan di service ini |
| Masalah yang diketahui | — |
| Risiko tersisa | Regresi QR belum dibuktikan runtime |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | `??` folder `PatientManagement/MasterData/Services/`; `M` `PatientController.cs`, `Program.cs` |
| Langkah berikutnya | Bandingkan isi QR satu pasien sampel sebelum/sesudah pada uji pengguna |
