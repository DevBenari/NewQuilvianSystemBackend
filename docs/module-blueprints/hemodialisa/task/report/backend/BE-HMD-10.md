# Laporan Perubahan Backend — `BE-HMD-10`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-10` |
| Judul | Penjadwalan Sesi HD dengan Validasi Tabrakan Pasien-Mesin-Station dan Isolasi |
| Slice | `MVP-3` — Penjadwalan Sesi dan Pencegahan Tabrakan Sumber Daya |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.4 |
| Trace | `FR-HMD-030`, `FR-HMD-031`, `FR-HMD-032`, `CAP-29`, `NFR-002`; `contracts/api-contract.md` grup Schedule; `state-transition-matrix.md` bagian 3; `validation-matrix.md` bagian 2 (`HMD-VAL-030` s.d. `HMD-VAL-036`) |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-04` ✅, `BE-HMD-08` ✅, `BE-HMD-09` ✅ |
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
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-CODE-001` s.d. `QBE-CODE-006` |

---

## 1. Masalah yang diperbaiki

Tanpa penjadwalan terpusat, dua koordinator dapat memasang dua pasien pada mesin yang sama di jam
yang sama, atau memasang pasien Hepatitis B pada mesin umum — risiko kontaminasi silang yang
nyata.

---

## 2. Proses bisnis

1. **Koordinator** membuat jadwal (`POST /`): episode, tanggal, shift, jam mulai–selesai, mesin,
   station, kunjungan, dokter penanggung jawab, daftar perawat, dan — bila sesi lahir dari
   permintaan — `OrderId`.
2. Sistem memeriksa berurutan:

| Pemeriksaan | Bila gagal |
| --- | --- |
| Jam selesai setelah jam mulai, tanggal jam mulai sama dengan tanggal jadwal, rentang paling lama 24 jam | `400` |
| Episode `Active` dan memiliki resep aktif | `422 HMD-VAL-030` |
| Mesin aktif, dapat dijadwalkan, dan `Ready`; station aktif dan `Available`; keduanya di unit episode | `422 HMD-VAL-034` |
| Keputusan isolasi yang berlaku pada tanggal itu: pasien isolasi wajib mesin khusus kebutuhan yang sama **dan** station isolasi; mesin khusus tidak dipakai pasien non-isolasi | `422 HMD-VAL-035` |
| Kunjungan sah milik pasien; permintaan (bila ada) sudah `Accepted` dan milik pasien | `422 HMD-VAL-036` / `422` |
| Kewenangan petugas dan rasio perawat (`BE-HMD-11`) | `422 HMD-VAL-037/038` bila sakelar ditegakkan, selain itu peringatan |

3. Di dalam **satu transaksi**, service mengambil `pg_advisory_xact_lock` atas mesin, station, dan
   pasien, lalu memeriksa tiga tabrakan. Tumpang tindih berarti
   `mulai_lain < selesai_baru` **dan** `selesai_lain > mulai_baru`; sesi `Cancelled` tidak dihitung.
   - pasien sudah punya sesi pada jam itu → `409 HMD-VAL-031`;
   - mesin terpakai → `409 HMD-VAL-032`;
   - station terpakai → `409 HMD-VAL-033`.
4. Bila lolos, sesi terbentuk dengan status **`Scheduled`**, nomor `HD-SES-xxxxxxxx`, menautkan
   **resep aktif** episode saat itu, dan penugasan petugas. Permintaan asal berpindah ke `Fulfilled`
   di transaksi yang sama.
5. Jadwal dapat diubah (`PATCH /{id}/schedule`) dengan pemeriksaan yang sama, atau dibatalkan
   (`POST /{id}/cancel`) dengan alasan wajib (`400 HMD-VAL-080`). Sesi yang sedang berjalan
   tidak dapat dibatalkan (`422`, gunakan penghentian sesi); sesi final ditolak `423`.

**Contoh berangka.** Pasien B dijadwalkan ke `M-01` pukul 07.00–11.00. Koordinator lain meminta
`M-01` untuk Pasien D pukul 09.00–13.00 tanggal yang sama. Rentang `[07.00, 11.00)` dan
`[09.00, 13.00)` beririsan dua jam → `409 HMD-VAL-032`. Jadwal 11.00–15.00 diterima, karena
batas akhir tidak dihitung tumpang tindih.

**Dua permintaan bersamaan.** Kedua koordinator menekan Simpan pada detik yang sama. Kunci
advisory mengantrekan keduanya: yang pertama tersimpan, yang kedua baru memeriksa setelahnya,
melihat jadwal pertama, dan ditolak `409`. Tidak pernah ada dua pasien pada mesin yang sama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Schedule, `state-transition-matrix.md` bagian 3, `validation-matrix.md` bagian 2, `data-dictionary.md` bagian 3.8
- `HmdSession`, `HmdSessionStaffAssignment`, `HmdIsolationDecision`, `HmdMachine`, `HmdStation`, `HmdSetting`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdScheduleService.cs` | Baru. Buat jadwal, ubah jadwal, batalkan, validasi sumber daya/isolasi/konteks, tiga tabrakan di bawah kunci |
| `Services/HmdServiceSupport.cs` | `AcquireLocksAsync` (`pg_advisory_xact_lock`), `ToUtc`/`LocalDate` (waktu tanpa zona dianggap Asia/Jakarta lalu disimpan UTC), `GetActiveIsolationAsync` |
| `Services/HmdSessionProjection.cs` | Baru. Proyeksi `HmdSessionResponse` bersama |
| `Controllers/HmdScheduleController.cs` | Baru; endpoint penjadwalan (worklist dan penugasan dilaporkan pada `BE-HMD-11`) |
| `DTOs/HmdSessionDtos.cs` | Request/response jadwal |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 3 endpoint penjadwalan kontrak terpenuhi. **Delta**: (a) teks tabrakan mengikuti `validation-matrix.md` — "Mesin ini sudah dipakai pasien lain pada jam tersebut." — bukan contoh roadmap "Mesin M-01 telah terpakai …"; teks isolasi "Pasien ini memerlukan mesin atau ruang khusus …", bukan "Pasien memerlukan alokasi mesin isolasi."; (b) status `Planned` tidak dipakai — sesi langsung `Scheduled`; (c) mesin khusus isolasi ditolak untuk pasien non-isolasi — tafsiran "mesin khusus" pada `FR-HMD-032` |
| Database | Tidak ada perubahan schema. Index biasa `(MachineId/StationId/EpisodeId, ScheduledStartAt, …)` mempercepat pemeriksaan tabrakan |
| Keamanan/Auth | `HemodialysisSchedule : Create/Update/Cancel` |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Schedule

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Menjadwalkan sesi tanpa tabrakan pasien, mesin, dan station (`201`) | `HemodialysisSchedule : Create` |
| `PATCH` | `/{id}/schedule` | Mengubah tanggal, jam, mesin, atau station dengan pemeriksaan yang sama | `HemodialysisSchedule : Update` |
| `POST` | `/{id}/cancel` | Membatalkan jadwal dengan alasan | `HemodialysisSchedule : Cancel` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source tiga tabrakan | Kueri tumpang tindih setengah-terbuka; urutan pasien → mesin → station | `PASS` | `HmdScheduleService.cs` baris 737–765 |
| Pemeriksaan source kunci | Transaksi + `AcquireLocksAsync` sebelum pemeriksaan tabrakan | `PASS` | `HmdScheduleService.cs` baris 250–265 |
| Pemeriksaan source isolasi | `HMD-VAL-035` dua arah | `PASS` | `HmdScheduleService.cs` baris 676–690 |
| Perbaikan selama pengerjaan | `request.Staff` kosong kini diperlakukan sebagai daftar kosong, bukan error | `PASS` | `HmdScheduleService.cs` baris 231 dan 522 |
| Uji konkurensi paralel | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `ScheduleCollisionTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji konkurensi dan uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `M-01` 07.00–11.00 lalu 09.00–13.00 pada tanggal sama → `409` | Terpenuhi, **dengan delta teks** | `409 HMD-VAL-032`; teks mengikuti `validation-matrix.md` |
| 2. Pasien Hepatitis B berstatus isolasi dijadwalkan ke mesin umum → `422` | Terpenuhi, **dengan delta teks** | `422 HMD-VAL-035` |
| 3. Sesi terbentuk berstatus `Scheduled` dan menautkan resep aktif saat itu | Terpenuhi | `SessionStatus = Scheduled`, `PrescriptionId = prescriptionId` |
| DoD: deteksi benturan dan isolasi lolos pengujian konkurensi | Mekanisme ada (kunci advisory + transaksi); pengujian konkurensi otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |
| DoD: endpoint create schedule sesuai kontrak | Terpenuhi | Audit akses |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Roadmap menyebut "transaksi serializable"; yang dipakai transaksi biasa dengan kunci advisory per mesin, station, dan pasien — memberi jaminan yang sama untuk sumber daya yang diperebutkan tanpa kegagalan serialisasi acak |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Tabrakan hanya dijaga service; penulisan langsung ke tabel di luar service tidak terjaga, karena index jadwal sengaja bukan unique (`data-dictionary.md` 3.8) |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji konkurensi oleh pemilik (dua permintaan paralel pada mesin sama); `BE-HMD-11` |
