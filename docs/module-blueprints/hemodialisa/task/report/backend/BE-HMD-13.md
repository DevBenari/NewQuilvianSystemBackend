# Laporan Perubahan Backend — `BE-HMD-13`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-13` |
| Judul | Pernyataan Sesi Siap dan Transaksi Memulai Sesi HD Berpenanda Idempotensi |
| Slice | `MVP-4` — Pelaksanaan Sesi, Checklist Pra-HD, Pemantauan, dan Farmasi |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.5 |
| Trace | `FR-HMD-052` s.d. `FR-HMD-055`, `CAP-10`, `CAP-31`, `NFR-001`, `NFR-003`, `NFR-004`, `HMD-DEC-009`; `contracts/api-contract.md` grup Session (`ready`, `start`); `state-transition-matrix.md` bagian 3; `HMD-VAL-040` s.d. `HMD-VAL-053`; `integration-contract.md` bagian 3 |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-12` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 2, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — tiga acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE`; membuat baris `TrxPatientProcedure` milik Clinical Management lewat entity yang sudah ada |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-CODE-004` |

---

## 1. Masalah yang diperbaiki

Menekan tombol Mulai adalah titik paling berisiko: pasien akan tersambung ke mesin. Dua hal harus
dijamin: kondisi yang tadi dinyatakan siap masih benar **detik itu juga**, dan klik ganda karena
jaringan lambat tidak membuat dua tindakan tertagih.

---

## 2. Proses bisnis

1. **Nyatakan siap** (`POST /{id}/ready`), dari `PreCheck`. Gerbang diperiksa berurutan:

| Syarat | Bila gagal |
| --- | --- |
| Seluruh butir checklist wajib `Met` atau dilewati secara sah | `422 HMD-VAL-040` beserta daftar butirnya |
| Penilaian Pra-HD berisi berat badan, tekanan darah, dan nadi | `422 HMD-VAL-041` |
| Kesiapan unit tanggal dan shift itu `Ready` | `422 HMD-VAL-042` |
| Dokter penanggung jawab sesi sudah ditetapkan | `422 HMD-VAL-043` |
| Mesin masih `Ready` dan station masih `Available` | `422 HMD-VAL-051` |
| Kunjungan masih sah | `422 HMD-VAL-052` |

   Lolos → `Ready`, `ReadyAt`/`ReadyByUserId` tercatat.
2. **Tahan** (`POST /{id}/hold`) dari `PreCheck`/`Ready` dengan alasan wajib (`400 HMD-VAL-045`);
   **lanjutkan** (`POST /{id}/resume`) kembali ke `PreCheck` sehingga gerbang siap diulang.
3. **Mulai** (`POST /{id}/start`) dengan `IdempotencyKey` wajib:
   1. Kunci yang sudah dipakai sesi ini → jawab data sesi apa adanya, tanpa membuat apa pun.
      Kunci milik sesi lain → `409`.
   2. Transaksi dibuka dan sesi dikunci (`pg_advisory_xact_lock`), sehingga dua klik bersamaan
      diantrekan. Sesi yang sudah `InProgress` dengan kunci sama → jawab sukses; kunci berbeda →
      `409 HMD-VAL-053` "Sesi ini sudah dimulai."
   3. Sesi harus `Ready` (`422 HMD-VAL-050`).
   4. **Pemeriksaan tepat waktu**: seluruh gerbang siap di atas diulang detik itu juga.
   5. Resep aktif episode **saat ini** dibaca dan ditautkan ulang ke sesi — bila resep saat
      penjadwalan sudah digantikan, sesi memakai resep terbaru, bukan instruksi usang.
   6. Satu `TrxPatientProcedure` dibuat berstatus `InProgress`: `DoctorId` = dokter penanggung
      jawab sesi, `InstructingDoctorId` = dokter pembuat resep (`HMD-DEC-009`), tindakan dari
      `HmdSetting.ProcedureId`, `IsBillable = true`, kunci `HMD-SES-{id}`.
   7. Sesi → `InProgress`, `StartedAt` = **waktu server**, `StartedByUserId` = perawat.
   8. Commit. Bila unique index menolak (klik kedua lolos bersamaan), transaksi dibatalkan dan
      data sesi yang sudah berjalan dikembalikan.

**Contoh mesin mendadak rusak.** Sesi dinyatakan siap pukul 06.55. Pukul 06.58 teknisi
memblokir `M-01`. Pukul 07.02 perawat menekan Mulai → pemeriksaan tepat waktu mendapati `M-01`
bukan `Ready` → `422 HMD-VAL-051` "Mesin berubah status dan tidak lagi dapat digunakan …".
Tidak ada tindakan yang terbentuk.

**Contoh klik ganda.** `POST …/start` dengan kunci `abc-123` terkirim dua kali dalam 500 ms.
Permintaan pertama memulai sesi; yang kedua menunggu kunci, lalu melihat sesi sudah
`InProgress` dengan kunci yang sama dan menjawab sukses. Tepat satu `TrxPatientProcedure`;
unique `IX_HmdSession_PatientProcedureId` dan `IX_HmdSession_IdempotencyKey` menjadi lapis
terakhir.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Session, `state-transition-matrix.md` bagian 3, `validation-matrix.md`, `integration-contract.md` bagian 3 (pemetaan dokter)
- `TrxPatientProcedure`, `PatientProcedureStatus`, `PatientProcedureSource`, `MstProcedure`, `HmdSetting`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdSessionService.cs` | Rincian sesi, nyatakan siap, tahan, lanjutkan, mulai, `EvaluateReadinessGateAsync` (baris 1119–1160), `GuardWritableAsync` |
| `Controllers/HmdSessionController.cs` | Endpoint rincian, siap, tahan, lanjutkan, mulai |
| `DTOs/HmdSessionDtos.cs` | `StartHmdSessionRequest`, `HoldHmdSessionRequest`, `HmdSessionDetailResponse` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 4 endpoint kontrak terpenuhi (`GET /{id}`, `ready`, `hold`, `start`). **Delta**: (a) `POST /{id}/resume` untuk kembali dari `Held`, memakai hak `Hold`; (b) resep ditautkan ulang ke resep aktif saat Mulai |
| Database | Tidak ada perubahan schema. Menulis satu `TrxPatientProcedure` per sesi |
| Keamanan/Auth | `HemodialysisSession : Read/DeclareReady/Hold/Start`. Rincian sesi (`GET /{id}`) dicatat logger karena memuat data klinis |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Session

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}` | Rincian sesi beserta aksi yang tersedia pada statusnya; pembacaannya dicatat | `HemodialysisSession : Read` |
| `POST` | `/{id}/ready` | Menyatakan sesi siap setelah seluruh gerbang lolos | `HemodialysisSession : DeclareReady` |
| `POST` | `/{id}/hold` | Menahan sesi dengan alasan | `HemodialysisSession : Hold` |
| `POST` | `/{id}/resume` | Melanjutkan sesi yang ditahan ke persiapan | `HemodialysisSession : Hold` |
| `POST` | `/{id}/start` | Memulai cuci darah secara idempoten dengan waktu server | `HemodialysisSession : Start` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 12.19 WIB |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source mulai | Kunci idempotency, kunci advisory, gerbang tepat waktu, satu `TrxPatientProcedure`, waktu server | `PASS` | `HmdSessionService.cs` baris 479–622 |
| Index di DB | `IX_HmdSession_IdempotencyKey` dan `IX_HmdSession_PatientProcedureId` unique bersyarat | `PASS` | `pg_indexes` `QuilvianNewDevHamzah` |
| Uji konkurensi dua klik | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `SessionStartIdempotencyTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji konkurensi dan runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Mesin diblokir antara siap dan Mulai → Mulai ditolak `422` | Terpenuhi | Gerbang tepat waktu `HMD-VAL-051` |
| 2. Kunci `abc-123` dikirim dua kali dalam 500 ms → sesi dimulai sekali, tepat 1 `TrxPatientProcedure` | Terpenuhi | Kunci advisory + pemeriksaan kunci + dua unique index |
| 3. Jam mulai memakai waktu server | Terpenuhi | `StartedAt = DateTime.UtcNow`; request tidak membawa jam mulai |
| DoD: transaksi mulai atomik, uji idempotensi dan uji tepat waktu | Atomik pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Waktu server memakai `DateTime.UtcNow` (disimpan `timestamp with time zone`), setara `DateTimeOffset.UtcNow` pada teks roadmap |
| Masalah yang diketahui | `HmdSetting.SessionStartGraceMinutes` (bawaan 60 menit) tersimpan tetapi **belum dipakai** memeriksa apa pun: kontrak tidak menetapkan akibat bila toleransi terlewati — ditolak, diberi peringatan, atau hanya ditandai terlambat — dan tidak ada kode `HMD-VAL` untuknya. Perlu keputusan pemilik |
| Risiko tersisa | Bila `HmdSetting.ProcedureId` kosong atau tindakannya nonaktif, Mulai ditolak `422` dengan anjuran menghubungi admin unit |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Keputusan pemilik soal toleransi mulai; uji dua klik paralel oleh pemilik |
