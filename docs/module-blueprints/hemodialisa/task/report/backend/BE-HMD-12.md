# Laporan Perubahan Backend — `BE-HMD-12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-12` |
| Judul | Checklist Pra-HD, Validasi Prasyarat Keselamatan, dan Gerbang Pelolosan Dokter |
| Slice | `MVP-4` — Pelaksanaan Sesi, Checklist Pra-HD, Pemantauan, dan Farmasi |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.5 |
| Trace | `FR-HMD-050`, `FR-HMD-051`, `FR-HMD-052`, `CAP-30`, `HMD-ASM-001`, `HMD-GATE-002`, `HMD-DEC-010`; `contracts/api-contract.md` grup Session Checklist; `validation-matrix.md` bagian 3 (`HMD-VAL-040` s.d. `HMD-VAL-046`); `integration-contract.md` bagian 3 |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-03` ✅, `BE-HMD-05` ✅, `BE-HMD-10` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — dua acceptance criteria terpetakan ke source; satu celah kontrak integrasi dicatat terbuka (bagian 7) |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE`; menulis `TrxPatientVitalSign` milik Clinical Management lewat entity yang sudah ada |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-CODE-001` s.d. `QBE-CODE-006` |

---

## 1. Masalah yang diperbaiki

Sebelum cuci darah dimulai, perawat wajib memastikan 12 hal: identitas, kunjungan, program HD,
resep, persetujuan tindakan, alergi, akses vaskular, isolasi, mesin, station, air, dan BMHP.
Tanpa checklist yang ditegakkan sistem, satu butir yang terlewat — misalnya persetujuan tindakan
— baru ketahuan setelah pasien tersambung ke mesin.

---

## 2. Proses bisnis

1. **Check-in** (`POST /{id}/check-in`): sesi `Scheduled` → `CheckedIn` setelah kunjungan
   diperiksa sah dan milik pasien (`422 HMD-VAL-036`). Sistem menyalin 12 butir checklist aktif
   menjadi baris sesi berstatus `NotChecked`.
2. **Perawat mengisi checklist** (`PUT /{id}/checklist`): tiap butir `Met`, `NotMet`,
   `NotApplicable`, atau kembali `NotChecked`. Setiap butir yang diisi mencatat perawat pemeriksa
   (`VerifiedByUserId`) dan waktu server (`VerifiedAt`).
3. **Penilaian Pra-HD** (`PUT /{id}/pre-hd`): berat badan, tanda vital, keluhan, dan kondisi
   akses. Tanda vital ditulis ke `TrxPatientVitalSign` milik Clinical Management bernomor
   `VTS-HD-xxxxxxxx`; sesi berpindah ke `PreCheck`.
4. **Dokter melewati butir** (`POST /{id}/checklist/{itemId}/override`) dengan alasan wajib:
   - alasan kosong → `400 HMD-VAL-046`;
   - pelaku tidak tertaut ke data dokter → `403`;
   - `IsOverridable` butir itu dibaca **langsung dari master saat itu**; bila `false` →
     `422 HMD-VAL-044` "Butir ini tidak dapat dilewati. Lengkapi lebih dulu sebelum sesi dimulai."
   - bila boleh, tersimpan `IsOverridden`, alasan, `OverriddenByUserId`, dan waktu server.
5. Gerbang siap (`BE-HMD-13`) menerima butir **wajib** hanya bila `Met` atau dilewati secara sah.

**Perbaikan selama penulisan laporan (22 September 2026).** Semula gerbang juga menghitung
`NotApplicable` sebagai terpenuhi. Akibatnya butir wajib yang ditolak dilewati (`HMD-VAL-044`)
dapat diloloskan cukup dengan menandainya "tidak berlaku" — bertentangan dengan bunyi
`HMD-VAL-040` "belum terpenuhi **dan tidak dilewati secara sah**" dan dengan gerbang kesiapan
unit yang sudah mewajibkan `Met`. Sekarang `NotApplicable` hanya meloloskan butir yang **tidak**
wajib.

**Contoh.** Perawat membuka Pra-HD Ibu Sinta. Sebelas butir `Met`, butir "Akses vaskular layak
dipakai" `NotMet` karena lengan bengkak. Dokter menekan Lewati dengan alasan → ditolak `422`,
karena seluruh butir bawaan tidak boleh dilewati (`HMD-ASM-001`). Menandai butir itu "tidak
berlaku" juga tidak meloloskan sesi. Yang dapat dilakukan: menahan sesi (`hold`) sampai akses
diperbaiki.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Session Checklist, `validation-matrix.md` bagian 3, `state-transition-matrix.md` bagian 3, `integration-contract.md` bagian 3
- `HmdSessionChecklist`, `HmdChecklistItem`, `HmdSessionAssessment`, `TrxPatientVitalSign`, `TrxPatientConsent`, `PatientConsentStatus`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdSessionService.cs` | Check-in, baca/simpan checklist, override, penilaian Pra-HD, `ReadChecklistAsync`. Perbaikan `IsSatisfied` (baris 1221–1256) |
| `Controllers/HmdSessionController.cs` | Endpoint check-in, checklist, override, dan Pra-HD |
| `DTOs/HmdSessionDtos.cs` | Request/response checklist, override, dan penilaian |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 5 endpoint kontrak terpenuhi. **Delta**: hasil butir memakai enum `NotChecked/Met/NotMet/NotApplicable` sesuai kamus data, bukan `Checked = true` seperti contoh roadmap |
| Database | Tidak ada perubahan schema. Menulis `TrxPatientVitalSign` (sumber `ProcedureMonitoring`) |
| Keamanan/Auth | `HemodialysisSession : CheckIn/Read/Update/OverrideChecklist`. Override dijaga tiga lapis: hak akses, pelaku harus dokter, dan `IsOverridable` master |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Session

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/check-in` | Pasien tiba; kunjungan diperiksa dan checklist disiapkan | `HemodialysisSession : CheckIn` |
| `GET` | `/{id}/checklist` | 12 butir checklist beserta hasil, pemeriksa, dan status lolos | `HemodialysisSession : Read` |
| `PUT` | `/{id}/checklist` | Perawat menyimpan hasil butir | `HemodialysisSession : Update` |
| `POST` | `/{id}/checklist/{itemId}/override` | Dokter melewati butir yang boleh dilewati, dengan alasan | `HemodialysisSession : OverrideChecklist` |
| `PUT` | `/{id}/pre-hd` | Menyimpan penilaian Pra-HD dan tanda vital | `HemodialysisSession : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` sesudah perbaikan `IsSatisfied` | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa, waktu 57 detik | `PASS` | Log build 22 September 2026 12.19 WIB |
| QBE Strict atas 94 berkas sesudah perbaikan | `PASS`, 0 violation, 0 review | `PASS` | 22 September 2026 |
| Audit akses reflektif | 5 endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source override | Alasan → dokter → `IsOverridable` master; jejak disimpan | `PASS` | `HmdSessionService.cs` baris 260–325 |
| Pemeriksaan source pengisian | `VerifiedByUserId` dan `VerifiedAt` tiap butir | `PASS` | `HmdSessionService.cs` baris 204–258 |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `PreDialysisChecklistTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Override butir `IDENTITY` (`IsOverridable = false`) → `422` dengan pesan kebijakan keselamatan | Terpenuhi | `422 HMD-VAL-044`; teks mengikuti `validation-matrix.md` |
| 2. Perawat mengisi 12 butir; status pemenuhan, perawat pemeriksa, dan waktu tercatat | Terpenuhi | `SaveChecklistAsync` |
| DoD: endpoint checklist dan override selesai; penolakan override non-overridable | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Perbaikan gerbang `NotApplicable` dilakukan sesudah build pertama; build dan QBE diulang dengan hasil sama |
| Masalah yang diketahui | **Celah kontrak integrasi, terbuka.** `integration-contract.md` bagian 3 meminta Hemodialisa **membaca** `TrxPatientConsent` untuk memeriksa ada tidaknya persetujuan sah. Pembacaan itu **belum** diimplementasikan: kontrak tidak menetapkan status persetujuan mana yang sah (`Signed`, `Verified`, atau `Approved`) dan jenis persetujuan mana yang mencakup HD — menetapkannya adalah keputusan klinis-hukum. Selama belum diputuskan, butir `CONSENT` wajib, tidak dapat dilewati, dan dicentang manual oleh perawat, sehingga sesi tidak pernah lolos tanpa pernyataan persetujuan |
| Risiko tersisa | Butir `CONSENT` bergantung pada ketelitian perawat sampai pembacaan otomatis ada |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Keputusan pemilik: definisi "persetujuan sah" untuk HD, lalu task lanjutan pembacaan `TrxPatientConsent`; uji runtime |
