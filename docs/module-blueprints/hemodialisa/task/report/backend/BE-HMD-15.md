# Laporan Perubahan Backend — `BE-HMD-15`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-15` |
| Judul | Pencatatan Pemberian Obat Intra-HD dan Penerusan Pemakaian ke Bounded Context Farmasi |
| Slice | `MVP-4` — Pelaksanaan Sesi, Checklist Pra-HD, Pemantauan, dan Farmasi |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.5 |
| Trace | `FR-HMD-061`, `FR-HMD-062`, `CAP-23`, `CAP-33`, `NFR-007`; `contracts/api-contract.md` grup Session Medications; `integration-contract.md` bagian 5; `HMD-VAL-056` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-13` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 2, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
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
| Keberlakuan | `NEW CODE`; memanggil `DrugUsageService` milik `PharmacyManagement` tanpa mengubahnya |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |

---

## 1. Masalah yang diperbaiki

Obat yang diberikan selama cuci darah — heparin, eritropoietin, zat besi IV, antibiotik — harus
tercatat di sesi pasien **dan** mengurangi stok Farmasi. Bila keduanya dijadikan satu transaksi,
gangguan sesaat di Farmasi akan membatalkan catatan klinis, padahal obatnya sudah masuk ke tubuh
pasien. Itu kesalahan yang jauh lebih berbahaya daripada stok yang terlambat berkurang.

---

## 2. Proses bisnis

1. Perawat mencatat pemberian obat (`POST /{id}/medications`) selama sesi berjalan atau sesudahnya
   sebelum dokumentasi diajukan: obat, dosis, satuan dosis, rute (bolus IV, infus kontinu, sirkuit
   ekstrakorporeal, subkutan, oral, lainnya), waktu pemberian, dokter pemberi instruksi, dan —
   untuk Farmasi — lokasi stok, satuan stok, serta jumlah dalam satuan stok.
2. Obat, dosis lebih dari nol, satuan, dan rute wajib; bila tidak → `400 HMD-VAL-056`
   "Catatan pemberian obat belum lengkap. Isi obat, dosis, satuan, dan jalur pemberiannya."
3. Catatan disimpan ke `HmdSessionMedication` **lebih dulu**, dengan status penerusan `Pending`.
4. Baru kemudian fakta pemakaian diteruskan ke Farmasi lewat `DrugUsageService.CreateAsync`
   (pemakaian berstatus draf; Farmasi yang mengurangi stok). Kunci idempotency
   `HMD-MED-{id catatan}` menjamin pengulangan tidak pernah membuat pemakaian kedua.
5. Hasilnya:

| Keadaan | Status penerusan | Catatan klinis |
| --- | --- | --- |
| Farmasi menerima | `Succeeded`, `DrugUsageId` terisi | Tersimpan |
| Data pendukung belum lengkap (kunjungan, lokasi stok, jumlah, satuan stok, atau akun tak tertaut profil tenaga kerja) | `Pending`, alasan di `HandoffError` | Tersimpan |
| Farmasi gagal/timeout | `Pending`, pesan galat di `HandoffError`; sisa entity Farmasi yang sempat ditambahkan dilepas | Tersimpan |

   Respons tetap **`201 Created`** pada ketiga keadaan.
6. Petugas mengulang penerusan (`POST /{id}/medications/{medicationId}/pharmacy-handoff/retry`),
   boleh sambil melengkapi lokasi stok, satuan stok, atau jumlah — bukan isi klinisnya. Catatan
   yang sudah `Succeeded` tidak diteruskan ulang. Pengulangan tetap boleh setelah sesi disahkan,
   karena data penerusan tidak termasuk sidik jari catatan final (`RecordHash`, `BE-HMD-17`).

**Contoh.** Perawat memberi Heparin 2.000 IU. Layanan Farmasi sedang gangguan. Permintaan tetap
dijawab `201`; catatan heparin ada di sesi pasien dengan status penerusan `Pending` dan alasan
kegagalannya. Satu jam kemudian petugas menekan Ulangi; Farmasi menerima, status menjadi
`Succeeded`, dan stok berkurang sekali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Medications, `integration-contract.md` bagian 5
- `DrugUsageService`, `CreateDrugUsageRequest`, `DrugUsageItemInput`, `PhmDrugUsage`, `MstDrug`, `ApplicationUser.WorkforceProfileId`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdSessionService.cs` | Baca/catat obat, ulangi penerusan, `ForwardToPharmacyAsync` (baris 740–856 dan 1395–1490) |
| `Controllers/HmdMedicationController.cs` | Baru, 3 endpoint |
| `DTOs/HmdSessionDtos.cs` | Request/response pemberian obat dan pengulangan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 1 endpoint kontrak terpenuhi. **Delta**: (a) `GET /{id}/medications`; (b) `POST /{id}/medications/{medicationId}/pharmacy-handoff/retry` — "antrean percobaan ulang" pada roadmap diwujudkan sebagai baris berstatus `Pending` yang dapat dibaca dan diulang, bukan pekerja latar otomatis; (c) nama kolom status adalah `HandoffStatus` sesuai kamus data, bukan `PharmacySyncStatus` seperti teks roadmap |
| Database | Tidak ada perubahan schema. Menulis `PhmDrugUsage` lewat service Farmasi |
| Keamanan/Auth | `HemodialysisMedication : Read/Administer`. Hemodialisa tidak mengurangi stok sendiri |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Session

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}/medications` | Obat yang diberikan beserta status penerusan ke Farmasi | `HemodialysisMedication : Read` |
| `POST` | `/{id}/medications` | Mencatat pemberian obat; catatan klinis selalu tersimpan (`201`) | `HemodialysisMedication : Administer` |
| `POST` | `/{id}/medications/{medicationId}/pharmacy-handoff/retry` | Mengulang penerusan yang tertunda | `HemodialysisMedication : Administer` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 12.19 WIB |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source urutan simpan | `SaveChangesAsync` catatan klinis sebelum `ForwardToPharmacyAsync` | `PASS` | `HmdSessionService.cs` baris 814–820 |
| Pemeriksaan source kegagalan Farmasi | `catch` → `Pending` + `HandoffError`, entity Farmasi dilepas, respons tetap `201` | `PASS` | `HmdSessionService.cs` baris 1440–1490 |
| Uji dengan Farmasi dimatikan | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `MedicationAdministrationTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji kegagalan Farmasi dan runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Farmasi gangguan → tetap `201`, catatan tersimpan, status sinkron `Pending` | Terpenuhi, **dengan delta nama kolom** | `HandoffStatus = Pending` |
| 2. Tanpa dosis atau rute → `400` dengan pesan jelas | Terpenuhi | `400 HMD-VAL-056` |
| DoD: endpoint obat selesai, isolasi kegagalan Farmasi | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nilai enum `HmdPharmacyHandoffStatus.Failed` tidak dipakai: kegagalan apa pun tetap `Pending` supaya dapat diulang |
| Masalah yang diketahui | Tidak ada pengulangan otomatis; baris `Pending` harus diulang petugas |
| Risiko tersisa | Bila penyimpanan status sesudah Farmasi menerima gagal, `DrugUsageId` tidak tercatat; pengulangan aman karena kunci idempotency yang sama |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji dengan layanan Farmasi dimatikan oleh pemilik; pertimbangkan pekerja latar untuk baris `Pending` |
