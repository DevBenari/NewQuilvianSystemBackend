# Laporan Perubahan Backend — `BE-HMD-14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-14` |
| Judul | Pencatatan Pemantauan Berkala, Parameter Mesin, dan Komplikasi Klinis Intra-HD |
| Slice | `MVP-4` — Pelaksanaan Sesi, Checklist Pra-HD, Pemantauan, dan Farmasi |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.5 |
| Trace | `FR-HMD-022`, `FR-HMD-060`, `FR-HMD-063`, `CAP-32`, `CAP-34`, `NFR-004`; `contracts/api-contract.md` grup Observations & Complications; `validation-matrix.md` bagian 4 (`HMD-VAL-054`, `HMD-VAL-055`, `HMD-VAL-057`) |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-13` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — dua acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-PAGE-001` |

---

## 1. Masalah yang diperbaiki

Selama empat jam cuci darah, perawat mencatat tanda vital dan parameter mesin setiap 30–60 menit.
Bila catatan baru menimpa catatan lama, kronologi hilang — padahal penurunan tekanan darah
bertahap dari 130 ke 80 mmHg adalah justru yang dicari dokter saat menilai komplikasi.

---

## 2. Proses bisnis

**Pemantauan berkala.**

1. Perawat mencatat observasi (`POST /{id}/observations`) **hanya** saat sesi `InProgress`
   (`422 HMD-VAL-054`).
2. Isian: tekanan darah sistolik/diastolik, nadi, laju napas, suhu, saturasi oksigen, QB, QD,
   tekanan arteri (AP), tekanan vena (VP), TMP, volume ultrafiltrasi terkumpul, dan catatan.
   Parameter mesin tinggal di `HmdSessionObservation`, **tidak** dipaksakan masuk
   `TrxPatientVitalSign` (`integration-contract.md` bagian 3).
3. Waktu pengamatan boleh diisi perawat (mencatat pukul 07.30 pada pukul 07.34), tetapi tidak
   boleh lebih awal dari jam mulai sesi (`400 HMD-VAL-055`) dan tidak boleh melewati waktu server.
4. Setiap observasi adalah **baris baru** dengan nomor urut per sesi. Nomor urut diambil di dalam
   transaksi berkunci per sesi, dan unique `(SessionId, SequenceNumber)` menjadi lapis terakhir.
5. `GET /{id}/observations` mengurutkan menurut waktu pengamatan lalu nomor urut.

**Contoh.** Lima observasi pukul 07.30, 08.00, 08.30, 09.00, dan 09.30 tersimpan sebagai nomor
urut 1–5. Tidak ada endpoint ubah atau hapus observasi, sehingga tidak ada yang tertimpa.

**Komplikasi.**

1. Perawat mencatat komplikasi (`POST /{id}/complications`) selama sesi berjalan atau sesudahnya
   sebelum dokumentasi diajukan: jenis (hipotensi intradialitik, kram, menggigil/demam,
   perdarahan akses, nyeri dada, aritmia, mual/muntah, sakit kepala, reaksi alergi, …), waktu
   kejadian, derajat keparahan, tanda dan gejala, intervensi, instruksi dokter, hasil, dampak
   pada sesi, dan tujuan rujukan.
2. Jenis, tanda/gejala, dan intervensi wajib (`400 HMD-VAL-057`).
3. **Sistem tidak pernah menyimpulkan komplikasi** dari nilai observasi. Tekanan darah 80/50
   pada pukul 08.30 tidak membuat catatan komplikasi apa pun; perawat yang mencatat secara sadar
   "Hipotensi intradialitik" dengan intervensi "Bolus NaCl 0,9% 100 ml dan penurunan kecepatan UF".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Observations & Complications, `validation-matrix.md` bagian 4, `integration-contract.md` bagian 3
- `HmdSessionObservation`, `HmdSessionComplication`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdSessionService.cs` | Baca/catat observasi (baris 625–735) dan komplikasi (baris 857–925) |
| `Controllers/HmdObservationController.cs`, `Controllers/HmdComplicationController.cs` | Baru — satu controller per Resource hak akses, berbagi base URL sesi |
| `DTOs/HmdSessionDtos.cs` | Request/response observasi dan komplikasi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 3 endpoint kontrak terpenuhi. **Delta**: `GET /{id}/complications` untuk membaca kembali komplikasi yang tercatat |
| Database | Tidak ada perubahan schema |
| Keamanan/Auth | `HemodialysisObservation : Read/Create`, `HemodialysisComplication : Read/Create`. Sesi final ditolak `423` |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Session

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}/observations` | Riwayat observasi terurut kronologis | `HemodialysisObservation : Read` |
| `POST` | `/{id}/observations` | Mencatat satu observasi baru (`201`) | `HemodialysisObservation : Create` |
| `GET` | `/{id}/complications` | Komplikasi beserta penanganannya | `HemodialysisComplication : Read` |
| `POST` | `/{id}/complications` | Mencatat komplikasi secara sadar oleh perawat (`201`) | `HemodialysisComplication : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 12.19 WIB |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source observasi | Hanya `InProgress`; batas waktu; nomor urut di bawah kunci; urutan baca | `PASS` | `HmdSessionService.cs` baris 625–735 |
| Pemeriksaan source komplikasi | Isian wajib; tidak ada kode yang membuat komplikasi dari observasi | `PASS` | `HmdSessionService.cs` baris 875–925 |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `IntraDialysisMonitoringTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Lima observasi tersimpan utuh dan terbaca urut tanpa terhapus atau tertimpa | Terpenuhi | Baris baru per observasi; tanpa endpoint ubah/hapus; unique nomor urut |
| 2. Tekanan darah 80/50 tidak memicu diagnosis otomatis; perawat mencatat komplikasi beserta intervensi | Terpenuhi | `CreateComplicationAsync` hanya dipanggil dari aksi perawat |
| DoD: endpoint observasi dan komplikasi sesuai kontrak, terurut tanpa penimpaan | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Mitigasi roadmap "menerima batching bila diperlukan" tidak dibuat: kontrak mendefinisikan satu observasi per permintaan |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Observasi yang salah ketik tidak dapat dikoreksi di tempat; koreksi setelah pengesahan lewat addendum Rekam Medis (`BE-HMD-17`) |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji runtime oleh pemilik |
