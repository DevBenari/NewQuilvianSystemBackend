# Laporan Perubahan Backend — `BE-HMD-07`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-07` |
| Judul | Alur Permintaan HD Masuk, Konteks Kunjungan, dan Tindakan Terima/Tahan/Tolak |
| Slice | `MVP-2` — Permintaan HD Masuk, Program Episode, dan Resep Hemodialisa |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.3 |
| Trace | `FR-HMD-001` s.d. `FR-HMD-004`, `CAP-36` (`HMD-CAP-001`), `HMD-DEC-008`, `HMD-ASM-003`; `contracts/api-contract.md` grup Order; `state-transition-matrix.md` bagian 1; `HMD-VAL-001` s.d. `HMD-VAL-006` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-01` ✅, `BE-HMD-03` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
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
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-PAGE-001`, `QBE-CODE-001` s.d. `QBE-CODE-006` |

---

## 1. Masalah yang diperbaiki

Permintaan cuci darah dari bangsal, IGD, atau poli sebelumnya hanya lewat telepon atau kertas.
Tidak ada jejak siapa yang meminta, kapan, atas alasan apa, dan siapa yang menolak. Permintaan
cito bisa terselip di antara permintaan rutin.

---

## 2. Proses bisnis

1. **Dokter atau perawat unit peminta** membuat permintaan (`POST /`) dengan pasien, kunjungan,
   alasan klinis, sumber (rawat inap/IGD/rawat jalan), dan penanda cito. Sistem memeriksa:
   pasien ada, kunjungan sah dan milik pasien itu, episode rawat inap (bila diisi) milik pasien
   itu, dan dokter peminta aktif. Nomor `HD-ORD-xxxxxxxx` dialokasikan penyedia nomor seri.
   Bila pasien sudah punya program HD aktif, permintaan langsung ditautkan ke program itu.
2. **Koordinator unit HD** memilah permintaan masuk:
   - **Terima** (`accept`) — hanya dari `Requested`. Penerimaan **tidak** membuat sesi;
     penjadwalan adalah langkah terpisah (`BE-HMD-10`). Baru ketika sesi dibentuk dan menunjuk
     permintaan ini, statusnya berpindah otomatis ke `Fulfilled`.
   - **Tahan** (`hold`) dengan alasan operasional wajib, dari `Requested` atau `Accepted`.
   - **Lepas tahanan** (`release-hold`) — kembali ke status sebelum ditahan.
3. **Dokter** menolak (`reject`) dengan alasan klinis wajib, dari `Requested` atau `OnHold`.
   Penolakan bersifat akhir.
4. **Pembuat permintaan** dapat membatalkan (`cancel`) selama belum diterima unit HD.

**Jalur tidak normal.**

| Keadaan | Jawaban |
| --- | --- |
| Kunjungan kosong, tidak sah, atau bukan milik pasien; alasan klinis kosong | `400 HMD-VAL-001` |
| Alasan tahan kosong | `400 HMD-VAL-003` |
| Lepas tahanan pada permintaan yang tidak ditahan | `409 HMD-VAL-004` |
| Penolak bukan dokter — diturunkan dari relasi akun ke `MstDoctor`, bukan dari nama role | `403 HMD-VAL-005` |
| Membatalkan permintaan yang sudah diterima | `409 HMD-VAL-006` |
| Aksi pada permintaan yang masih dalam alur tetapi sudah diproses orang lain (`Accepted`, `OnHold`) | `409 HMD-VAL-002` |
| Aksi pada permintaan final (`Rejected`, `Cancelled`, `Fulfilled`) | `422` "Permintaan ini sudah berstatus … dan bersifat final." |
| Dua koordinator menekan tombol bersamaan | Yang kedua `409 HMD-VAL-002` lewat token `Version` |

**Contoh.** dr. Rahmat menolak permintaan karena ketidakstabilan hemodinamik berat → status
`Rejected`. Koordinator kemudian menekan Terima pada permintaan itu → `422`, karena penolakan
klinis bersifat akhir. Bila kondisi pasien membaik, dibuat permintaan baru supaya riwayat
penolakan tetap terbaca.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Order, `state-transition-matrix.md` bagian 1, `validation-matrix.md`
- Pola order penunjang `LabOrder`/`RadOrder`; `InpatientClinicalContextService.ResolveActorDoctorIdAsync`; `NumberSeriesAllocator`
- `RegPatientEncounter`, `InpEpisode`, `MstDoctor`, `MstPatient`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdOrderService.cs` | Baru. Ringkasan, daftar berfilter status/urgensi/cito, rincian, buat, terima, tahan, lepas tahanan, tolak, batal |
| `Controllers/HmdOrderController.cs` | Baru, 10 endpoint |
| `DTOs/HmdOrderDtos.cs` | Baru |
| `Services/HmdServiceSupport.cs` | `CheckEncounterAsync` (kunjungan sah dan milik pasien) dan `AllocateNumberAsync` (`HMD_ORDER`/`HD-ORD`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 8 endpoint kontrak terpenuhi. **Delta**: (a) `filters/metadata` dan `summary` untuk layar antrean; (b) `POST /` menjawab `201 Created`; (c) aksi pada status final menjawab `422`, sedangkan permintaan yang masih dalam alur tetapi sudah diproses orang lain menjawab `409 HMD-VAL-002` — memisahkan "sudah final" dari "sudah diambil orang lain" sesuai `validation-matrix.md` |
| Database | Tidak ada perubahan schema. `IsCito` dan `OrderStatus` terindeks |
| Keamanan/Auth | `HemodialysisOrder : Read/Create/Accept/Hold/Reject/Cancel`. `Reject` dijaga dua lapis: hak akses dan pemeriksaan bahwa pelaku tertaut ke data dokter |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Order

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-orders`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan filter status, urgensi, sumber, dan urutan | `HemodialysisOrder : Read` |
| `GET` | `/summary` | Jumlah permintaan per status dan jumlah cito | `HemodialysisOrder : Read` |
| `GET` | `/` | Antrean permintaan berfilter status/urgensi, cito di atas | `HemodialysisOrder : Read` |
| `GET` | `/{id}` | Rincian permintaan beserta riwayat keputusannya | `HemodialysisOrder : Read` |
| `POST` | `/` | Membuat permintaan HD (`201`) | `HemodialysisOrder : Create` |
| `POST` | `/{id}/accept` | Koordinator menerima permintaan; tidak membuat sesi | `HemodialysisOrder : Accept` |
| `POST` | `/{id}/hold` | Menahan dengan alasan operasional | `HemodialysisOrder : Hold` |
| `POST` | `/{id}/release-hold` | Melepas tahanan ke status sebelumnya | `HemodialysisOrder : Hold` |
| `POST` | `/{id}/reject` | Dokter menolak dengan alasan klinis; bersifat akhir | `HemodialysisOrder : Reject` |
| `POST` | `/{id}/cancel` | Pembuat membatalkan selama belum diterima | `HemodialysisOrder : Cancel` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation (termasuk alokasi nomor tanpa Count/Max+1) | `PASS` | 22 September 2026 |
| Audit akses reflektif | 8 endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source validasi kunjungan | `HMD-VAL-001` bila kunjungan kosong/tidak sah/bukan milik pasien | `PASS` | `HmdOrderService.cs` baris 206–222 |
| Pemeriksaan source penolakan | Alasan wajib, pelaku harus dokter, hanya dari `Requested`/`OnHold` | `PASS` | `HmdOrderService.cs` baris 360–385 |
| Pemeriksaan source status final | `RejectTransition` → `422` untuk status final | `PASS` | `HmdOrderService.cs` baris 428–439 |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `HemodialysisOrderWorkflowTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Permintaan tanpa `EncounterId` sah → `400` dengan pesan deskriptif | Terpenuhi | `400 HMD-VAL-001` |
| 2. Koordinator (bukan dokter) menolak → `403` | Terpenuhi | Lapis 1: `[AccessPermission("HemodialysisOrder", "Reject")]` — admin hanya memberi butir ini kepada dokter. Lapis 2: pelaku yang tidak tertaut ke `MstDoctor` → `403 HMD-VAL-005` |
| 3. Permintaan `Rejected` lalu `accept` → `422` | Terpenuhi | `RejectTransition` |
| DoD: controller, service, dan DTO order selesai | Terpenuhi | — |
| DoD: pengujian transisi status lulus 100% | **Dikecualikan atas keputusan pengguna 22 September 2026** | Diganti pemeriksaan source |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE`. Perpindahan `Accepted → Fulfilled` tidak punya endpoint sendiri, sesuai kontrak: terjadi otomatis ketika koordinator membentuk sesi yang menunjuk permintaan ini (`HmdSession.OrderId`), di dalam transaksi penjadwalan (`HmdScheduleService.cs` baris 301–312, `BE-HMD-10`). Permintaan yang tidak lagi `Accepted` saat itu ditolak `409 HMD-VAL-002` |
| Risiko tersisa | Tautan ke program HD aktif dibuat saat permintaan dibuat; bila program itu kemudian ditutup, koordinator memilih episode lain saat menerima |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji runtime oleh pemilik |
