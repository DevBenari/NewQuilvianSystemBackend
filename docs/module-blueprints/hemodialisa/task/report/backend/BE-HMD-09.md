# Laporan Perubahan Backend — `BE-HMD-09`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-09` |
| Judul | Pengelolaan Siklus Resep Hemodialisa (Draf, Aktivasi, Penggantian Terlacak, Pembatalan) |
| Slice | `MVP-2` — Permintaan HD Masuk, Program Episode, dan Resep Hemodialisa |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.3 |
| Trace | `FR-HMD-020`, `FR-HMD-021`, `FR-HMD-022`, `CAP-28`, `HMD-DEC-009`; `contracts/api-contract.md` grup Prescriptions; `state-transition-matrix.md` bagian 2; `HMD-VAL-020` s.d. `HMD-VAL-024` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-08` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
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

Parameter cuci darah — durasi, target penarikan cairan, dializer, aliran darah, dialisat,
antikoagulan — adalah instruksi dokter. Bila resep bisa diubah di tempat, riwayatnya hilang:
tidak ada yang tahu bahwa target penarikan cairan pasien pernah 2.000 ml sebelum menjadi
2.500 ml, dan sesi yang sudah berjalan tidak lagi dapat dicocokkan dengan instruksi yang
berlaku saat itu.

---

## 2. Proses bisnis

1. **Dokter** membuat resep `Draft` pada episode yang **aktif** (`422 HMD-VAL-020` bila tidak).
   Pembuatnya diturunkan dari akun yang sedang masuk, bukan dari isian — pelaku yang tidak
   tertaut ke `MstDoctor` ditolak `403`.
2. Parameter resep: frekuensi per minggu, durasi (menit), target ultrafiltrasi (ml), aliran darah
   QB dan dialisat QD (ml/menit), tipe dializer, komposisi dialisat, profil natrium/bikarbonat,
   suhu dialisat, rencana antikoagulan, akses vaskular yang dipakai, dan catatan klinis.
3. Draf masih boleh disunting. Resep `Active` **tidak** boleh disunting: `423 HMD-VAL-024`
   "Resep yang sudah aktif tidak dapat diubah. Buat resep baru untuk menggantikannya."
4. **Aktivasi** memeriksa frekuensi, durasi, dan target ultrafiltrasi terisi serta akses vaskular
   milik episode itu belum `NotUsable` (`422 HMD-VAL-021`). Lalu, dalam **satu transaksi**
   yang dikunci per episode (`pg_advisory_xact_lock`):
   1. resep aktif lama → `Superseded`, menunjuk resep penggantinya;
   2. resep baru → `Active`, mencatat `ActivatedAt` (waktu server) dan `ActivatedByUserId`.

   Bila salah satu gagal, keduanya dibatalkan dan resep lama tetap berlaku (`409 HMD-VAL-022`).
5. **Pembatalan** dengan alasan wajib (`400 HMD-VAL-080`); resep yang sedang dipakai sesi
   berjalan tidak dapat dibatalkan (`409 HMD-VAL-023`).

**Contoh.** dr. Rahmat menaikkan target penarikan cairan dari 2.000 ml menjadi 2.500 ml. Ia
membuat draf baru 2.500 ml lalu mengaktifkannya. Resep baru `Active`, resep lama `Superseded`
dengan `SupersededByPrescriptionId` menunjuk resep baru. Keduanya tetap terbaca lengkap dengan
dokter pembuat dan waktu pengaktifan.

**Mengapa dua langkah simpan dalam satu transaksi.** PostgreSQL memeriksa unique index
bersyarat "satu resep aktif per episode" per pernyataan. Resep lama harus sudah `Superseded`
sebelum resep baru menjadi `Active`; keduanya tetap dalam satu transaksi sehingga tidak pernah
ada keadaan setengah jadi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Prescriptions, `state-transition-matrix.md` bagian 2, `validation-matrix.md` `HMD-VAL-020` s.d. `024`
- `HmdPrescription`, `HmdVascularAccess`, `MstDoctor`, `InpatientClinicalContextService`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdPrescriptionService.cs` | Baru. Daftar, rincian, buat, ubah draf, aktifkan (transaksional), batalkan |
| `Controllers/HmdPrescriptionController.cs` | Baru, 6 endpoint |
| `DTOs/HmdPrescriptionDtos.cs` | Baru |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 6 endpoint kontrak terpenuhi; `POST /` menjawab `201` |
| Database | Tidak ada perubahan schema. `IX_HmdPrescription_EpisodeId_Active` menjadi lapis terakhir |
| Keamanan/Auth | `HemodialysisPrescription : Read/Create/Update/Activate/Cancel` |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Prescription

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar resep per episode beserta riwayat penggantiannya | `HemodialysisPrescription : Read` |
| `GET` | `/{id}` | Rincian resep | `HemodialysisPrescription : Read` |
| `POST` | `/` | Dokter membuat resep draf (`201`) | `HemodialysisPrescription : Create` |
| `PUT` | `/{id}` | Mengubah draf; resep aktif ditolak `423` | `HemodialysisPrescription : Update` |
| `POST` | `/{id}/activate` | Mengaktifkan resep dan menggantikan resep aktif lama secara atomik | `HemodialysisPrescription : Activate` |
| `POST` | `/{id}/cancel` | Membatalkan resep dengan alasan | `HemodialysisPrescription : Cancel` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | 6 endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source immutability | `UpdateAsync` pada resep aktif → `Locked` `HMD-VAL-024` | `PASS` | `HmdPrescriptionService.cs` baris 125–136 |
| Pemeriksaan source penggantian | Transaksi + kunci advisory; lama `Superseded` lalu baru `Active`; rollback → `HMD-VAL-022` | `PASS` | `HmdPrescriptionService.cs` baris 165–258 |
| Index di DB | `IX_HmdPrescription_EpisodeId_Active` unique bersyarat terpasang | `PASS` | `pg_indexes` `QuilvianNewDevHamzah` |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `PrescriptionLifecycleTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Draf baru diaktifkan → baru `Active`, lama `Superseded`, riwayat keduanya terbaca dengan dokter dan waktu pengaktifan | Terpenuhi | `ActivateAsync`; `SupersededByPrescriptionId`, `PrescribingDoctorId`, `ActivatedAt` |
| 2. `PUT` pada resep `Active` → `423` dengan anjuran membuat resep baru | Terpenuhi | `HMD-VAL-024` |
| DoD: siklus hidup, immutability, dan riwayat penggantian | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Sesi yang dijadwalkan dengan resep lama memakai resep aktif terbaru saat Mulai ditekan (`BE-HMD-13`), sesuai mitigasi roadmap "resep dikunci saat sesi dimulai" |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji runtime oleh pemilik; `BE-HMD-10` |
