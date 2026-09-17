# Laporan Perubahan Backend — `BE-IGD-040`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-040` |
| Judul | Kesimpulan observasi tersimpan saat periode diselesaikan |
| Slice | `IGD-S08` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.8, kartu `BE-IGD-040` |
| Trace | `IGD-DEC-115`, `IGD-DEC-119`, `IGD-DEC-121`, `IGD-OQ-083` (bagian `Cancelled` dikecualikan); bukti `IGD-EV-110`; validation-matrix §8; api-contract §5 baris `Emergency Observation` |
| Contract version | API `0.5.0` dan validation `0.5.0` (penyelarasan teks 15 September 2026, status berkas `draft`). Aturannya sendiri `approved` lewat `IGD-DEC-115`, `119`, dan `121` (Rizki Gunawan, 15 September 2026). Bentuk request/response tidak berubah |
| Dependency | `IGD-DEC-115` ✅, `IGD-DEC-119` ✅ |
| Klasifikasi | `LIGHT` — satu method pada satu controller existing; tanpa entity, DTO, konfigurasi, migration, atau registrasi service |
| Task mode | `BACKEND` — frontend hanya dibaca untuk memeriksa pemanggil |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `EmergencyObservationController.cs`; laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `7b0c2ece` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 15 September 2026 |
| Status | **Implementation complete.** Build **Not Verified** — tidak dijalankan agent sesuai alur kerja owner; perintahnya di bagian 5. **Runtime not verified.** Bukan UAT |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `EmergencyInstallationManagement` / Emergency |
| Registry | Prefix `Emg`, kategori BUSINESS DOMAIN / MODULE, lifecycle `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `TOUCHED LEGACY` — controller existing yang memakai `ApplicationDbContext` langsung. Pola itu **tidak** diperluas dan **tidak** di-refactor (legacy ratchet) |
| QBE yang berlaku | `QBE-API-001` (envelope `ApiResponse`, kode status existing), `QBE-VAL-001` (validasi sebelum mutasi), `QBE-PERM-001` (metadata akses tidak berubah) |
| QBE yang dicatat, tidak ditangani | `QBE-SVC-001` — controller mengakses context langsung (legacy); `QBE-LOG-001` — log `UpdateObservationStatus` belum menyertakan aktor (legacy, di luar lingkup) |

---

## 1. Masalah yang diperbaiki

**Kondisi sebelum.** `PATCH .../emergency-observations/{id}/observation-status` menerima
`{ observationStatus, notes }`. Perlakuan `notes` sebelumnya:

| Status tujuan | Yang terjadi pada `notes` |
| --- | --- |
| `Escalated` | Dirapikan lalu disimpan ke `EscalationReason` (kosong → isi lama dipertahankan) |
| `Completed` | **Dibuang tanpa pesan galat** |
| `Cancelled` | Dibuang |
| `Active` | Dibuang |

Ditambah satu cabang refleksi untuk semua status:
`entity.GetType().GetProperty("Notes")?.SetValue(...)`.

**Root cause.** Tidak ada pemetaan eksplisit `Completed → CompletionSummary`. Satu-satunya jalan
generik adalah refleksi yang mencari properti bernama `Notes`, padahal `EmgObservation` **tidak
punya** properti itu — kolomnya bernama `CompletionSummary` dan `EscalationReason`. Karena
`GetProperty("Notes")` bernilai `null`, cabang itu tidak pernah menulis apa pun, dan kesimpulan
perawat hilang diam-diam dengan balasan `200`.

**Akibat samping yang ikut tertutup.** Kolom `EscalationReason` berkapasitas 1000 karakter
(`EmgObservationConfiguration.cs`), tetapi DTO mengizinkan `Notes` sampai 2000. Catatan eskalasi
1001–2000 karakter dulu lolos ke `SaveChangesAsync` yang tidak dibungkus `try/catch`, sehingga
berpotensi berakhir sebagai galat basis data (`500`). Sekarang ditolak `400` lebih dulu.

*Contoh:* perawat menutup observasi dengan catatan *"Nyeri dada hilang setelah 2 jam, EKG ulang
normal, siap disposisi"*. Sebelum perubahan, balasannya `200` dan `completionSummary` tetap
kosong. Sesudah perubahan, kalimat itu tersimpan dan tampil pada riwayat observasi.

---

## 2. Proses bisnis

**Pelaku:** perawat atau dokter IGD yang memegang hak `EmergencyObservation : Update`.

**Pemicu:** periode observasi pasien diselesaikan, dieskalasi, atau dibatalkan.

Urutan pemeriksaan pada satu permintaan:

| Urutan | Pemeriksaan | Bila gagal |
| ---: | --- | --- |
| 1 | Periode observasi ada dan belum dihapus | `404` *"Data observasi IGD tidak ditemukan."* |
| 2 | Transisi status observasi sah (`EmergencyObservationService.CanTransition`) | `400` *"Perubahan status dari … ke … tidak diperbolehkan."* |
| 3 | Transisi status kunjungan sah (penjaga `BE-IGD-018`) | `409` *"Status kunjungan tidak dapat berubah dari … ke …."* — pesan tidak berubah |
| 4 | **Baru.** Target `Completed`/`Escalated` dengan catatan > 1000 karakter | `400` *"Catatan paling banyak 1000 karakter."* |
| 5 | Status kunjungan diterapkan, status observasi diubah, `EndedAt` diisi, catatan dipetakan, disimpan | — |

**Pemetaan catatan — eksplisit per status tujuan**

| Status tujuan | Kolom tujuan | Catatan kosong/null |
| --- | --- | --- |
| `Completed` | `CompletionSummary` | Isi lama **dipertahankan** (`IGD-DEC-121`) |
| `Escalated` | `EscalationReason` | Isi lama dipertahankan — perilaku lama |
| `Cancelled` | **Tidak disimpan** | — (`IGD-OQ-083` masih terbuka) |
| `Active` | Tidak disimpan | — perilaku lama |

**Jalur tidak normal**

- Kunjungan sudah `Disposed` lalu periode diselesaikan dengan catatan 1.250 karakter → `409` dari
  penjaga kunjungan, bukan `400`. Aturan bisnis yang lebih penting dijawab lebih dulu.
- Catatan 1.250 karakter pada periode `Active` yang sah diselesaikan → `400`; status observasi,
  `EndedAt`, `CompletionSummary`, dan status kunjungan **tidak berubah**.
- Periode sudah `Completed` lalu dikirim `Completed` lagi dengan catatan baru, selama kunjungan
  masih `AwaitingDisposition` → transisi status-sama diterima penjaga yang sudah ada, dan
  `CompletionSummary` diganti catatan baru. Ini akibat dari aturan transisi existing, bukan
  aturan baru; bila kunjungan sudah bergerak ke `Disposed`, permintaan itu `409`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyObservationController.cs`
- `.../DTOs/EmergencyObservationDtos.cs` — `UpdateEmergencyObservationObservationStatusRequest`
- `.../Models/EmgObservation.cs`
- `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgObservationConfiguration.cs`
- `.../Services/EmergencyObservationService.cs` (`CanTransition`)
- `.../Services/EmergencyVisitService.cs` (`CanTransition`, `TryApplyVisitStatus`)
- `.../Enums/EmergencyObservationStatus.cs`
- `Program.cs` — registrasi `AddDbContext` (scoped) dan tidak adanya `InvalidModelStateResponseFactory` kustom
- Frontend (read-only): `emergency-assessment-slice.jsx` (`updateObservationStatus`),
  `emergency-assessment-observation-tab.jsx`, `emergency-assessment-constant.jsx`
  (`OBSERVATION_STATUS_ACTIONS`)
- Dokumen: `MODULE-STATUS.md`, `00-interview-decisions.md` (`IGD-DEC-115`, `119`, `121`,
  `IGD-OQ-083`), `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`,
  `contracts/api-contract.md` `0.5.0`, `contracts/validation-matrix.md` `0.5.0`,
  `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
  `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyObservationController.cs` | (1) Konstanta `MaxObservationStatusNotesLength = 1000`. (2) Pemeriksaan panjang catatan untuk `Completed`/`Escalated` sebelum `TryApplyVisitStatus`, dilewati bila transisi kunjungan tidak sah sehingga `409` tetap didahulukan. (3) `switch` eksplisit `Completed → CompletionSummary`, `Escalated → EscalationReason`. (4) Cabang refleksi `GetProperty("Notes")` dihapus |

`git diff --stat -- Areas`: 1 berkas, 36 baris ditambah, 4 dihapus.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request/response **tidak berubah**. Perilaku baru: `Completed` menyimpan `notes`; `400` baru untuk catatan `Completed`/`Escalated` 1001–2000 karakter. Sesuai api-contract `0.5.0` §5 dan validation `0.5.0` §8 |
| Database | **Tidak ada** perubahan schema, entity, konfigurasi, atau migration. Kolom `CompletionSummary` sudah ada (`HasMaxLength(1000)`). Tidak ada perintah database yang dijalankan |
| Keamanan/Auth | `NOT APPLICABLE` — `[AccessAction]`/`[AccessPermission("EmergencyObservation", "Update")]` tidak disentuh |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Observation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/observation-status` | Menyelesaikan, mengeskalasi, atau membatalkan periode observasi beserta catatannya | `EmergencyObservation : Update` |

| Field request | Tipe | Wajib | Aturan |
| --- | --- | :---: | --- |
| `observationStatus` | `int` (`Active`=1, `Completed`=2, `Escalated`=3, `Cancelled`=4) | Ya | Transisi mengikuti `CanTransition` |
| `notes` | `string?` | Tidak | Dirapikan (trim). `Completed`/`Escalated`: paling banyak 1000 karakter sesudah dirapikan. DTO tetap `[MaxLength(2000)]` |

| Kode | Kapan |
| --- | --- |
| `200` | Berhasil; balasan memuat `completionSummary` dan `escalationReason` terkini |
| `400` | Transisi observasi tidak sah; atau catatan `Completed`/`Escalated` > 1000 karakter; atau catatan > 2000 karakter (validasi model, lihat delta) |
| `404` | Periode observasi tidak ditemukan |
| `409` | Transisi status kunjungan ditolak penjaga |

**Contoh uji API manual** (belum dijalankan):

| # | Request | Harapan |
| ---: | --- | --- |
| 1 | Periode `Active`, kunjungan `UnderObservation`: `{"observationStatus":2,"notes":"  Nyeri dada hilang setelah 2 jam, EKG ulang normal, siap disposisi  "}` | `200`; `completionSummary` = kalimat tanpa spasi tepi; `endedAt` terisi; kunjungan `AwaitingDisposition` |
| 2 | Periode `Active` dengan `CompletionSummary` lama berisi, `{"observationStatus":2}` | `200`; `completionSummary` tetap isi lama |
| 3 | `{"observationStatus":3,"notes":"Saturasi turun ke 88%"}` | `200`; `escalationReason` berisi catatan; `completionSummary` tidak berubah |
| 4 | `{"observationStatus":4,"notes":"Salah buka periode"}` | `200`; `completionSummary` dan `escalationReason` tidak berubah |
| 5 | `{"observationStatus":2,"notes":"<1.250 karakter>"}` | `400` *"Catatan paling banyak 1000 karakter."*; `GET /{id}` menunjukkan status tetap `Active` |
| 6 | Kunjungan `Disposed`, `{"observationStatus":2,"notes":"<1.250 karakter>"}` | `409` *"Status kunjungan tidak dapat berubah dari Disposed ke AwaitingDisposition."* |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false` | **Tidak dijalankan agent** — owner menjalankan build backend sendiri (build berat RAM, backend sering berjalan di mesin owner) | `NOT RUN` — **Build = Not Verified** | — |
| `dotnet test` | Tidak ada proyek test di repository (`**/*Test*.csproj` nol berkas; dihapus 11 September 2026, `IGD-DEC-110`) | `NOT APPLICABLE` | Hasil pencarian berkas |
| Tinjauan diff | Satu berkas source; nol berkas di `Migrations/`, `Repositories/`, `Program.cs`, `DTOs/`, `Models/` | `PASS` | `git status --short -- Areas Migrations Repositories Program.cs` |
| Pemeriksaan pemanggil frontend (read-only) | Aksi *Selesaikan* tidak mengirim `notes` (`requiresReason` tidak diset), sehingga layar hari ini tetap mempertahankan `CompletionSummary` lama; aksi *Eskalasi* tetap mengirim alasan ke `EscalationReason` | `PASS` | `OBSERVATION_STATUS_ACTIONS`, `buildStatusThunk` |
| Uji API manual contoh 1–6 | Tidak dijalankan | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` — agent tidak menjalankan aplikasi backend, tidak memegang kredensial
petugas, dan dilarang mengakses basis data.

**Perintah untuk owner** (dari folder `NewQuilvianSystemBackend`):

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

Yang diperhatikan pada keluaran: `0 Error(s)`, dan tidak ada peringatan `CS` baru yang menunjuk
`EmergencyObservationController.cs`.

**Tidak dijalankan:** build, uji API manual, migration (tidak diperlukan), perintah basis data
apa pun.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `Completed` + `notes` → `200`; `CompletionSummary` berisi catatan yang sudah dirapikan; `EndedAt` terisi; status kunjungan berpindah lewat penjaga seperti sekarang | Terpenuhi pada source | `case Completed: entity.CompletionSummary = catatan ?? …` dengan `catatan = NormalizeText(request.Notes)`; `EndedAt ??= now` dan `TryApplyVisitStatus` tidak diubah |
| 2. `Completed` tanpa `notes` → `200`; `CompletionSummary` lama tidak terhapus | Terpenuhi pada source | `catatan ?? entity.CompletionSummary` |
| 3. `Escalated` tetap menulis ke `EscalationReason` | Terpenuhi pada source | `case Escalated: entity.EscalationReason = catatan ?? entity.EscalationReason` — setara baris lama |
| 4. `Cancelled` tidak berubah perilakunya — catatan tidak disimpan | Terpenuhi pada source | Tidak ada `case Cancelled`; batas panjang hanya berlaku bila `catatanDisimpan` (`Completed`/`Escalated`); transisi, `EndedAt`, dan status kunjungan untuk `Cancelled` tidak disentuh |
| 5. `Completed`/`Escalated` dengan `notes` > 1000 karakter → `400` *"Catatan paling banyak 1000 karakter."*, diperiksa sebelum data apa pun berubah; tanpa pemotongan | Terpenuhi pada source, **dengan delta tercatat** | Pemeriksaan berada sebelum `TryApplyVisitStatus` dan sebelum mutasi `entity`; tidak ada `Substring`. **Delta:** catatan > 2000 karakter ditolak lebih dulu oleh `[MaxLength(2000)]` DTO dengan `400` bawaan validasi model (pesan framework). Tetap `400`, tanpa perubahan data. DTO sengaja tidak diubah — **keputusan pengguna 15 September 2026** |
| 6. Cabang refleksi `GetProperty("Notes")` pada method ini dibuang; refleksi serupa di controller lain hanya dicatat | Terpenuhi | Cabang dihapus; daftar temuan di bagian 7 |
| 7. Penolakan `409` dari penjaga status kunjungan tetap terjadi lebih dulu dan pesannya tidak berubah | Terpenuhi pada source | Pemeriksaan panjang hanya jalan bila `_emergencyVisitService.CanTransition(visit.VisitStatus, target)` bernilai benar; bila tidak, `TryApplyVisitStatus` menjawab `409` dengan pesan yang sama |

**Definition of Done**

| Butir | Status |
| --- | --- |
| Acceptance 1–7 terpetakan ke source | Ya |
| Contoh request/response uji API manual untuk kriteria 1–5 (`IGD-DEC-110`) | Ya — bagian 4; **belum dijalankan** |
| Perintah build untuk Rizki | Ya — bagian 5 |
| Laporan tracked | Ya — berkas ini |
| Roadmap dan traceability diperbarui | Ya |
| QBE preflight | Ya — bagian Metadata |
| Tanpa UAT PASS | Ya |

**Pembedaan status:** Requirement approved = Ya (keputusan) · Delivery planned = Ya ·
**Implementation complete = Ya** · Build = **Not Verified** · **Runtime verified = Belum**.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | (1) **Delta pesan** untuk catatan > 2000 karakter — lihat kriteria 5. (2) Refleksi `GetProperty("Notes")` yang sama masih ada, **tidak diubah**: `EmergencyVisitController.cs` baris 384, 432, 497; `EmergencyResuscitationController.cs` baris 317; `EmergencyTriageController.cs` baris 474 (pada working tree 15 September 2026). (3) Panjang diukur dengan `string.Length` (unit UTF-16); karakter di luar BMP seperti emoji terhitung dua, sehingga batasnya sedikit lebih ketat dari kapasitas kolom — tidak pernah lebih longgar. (4) `Cancelled` belum menyimpan alasan — `IGD-OQ-083` terbuka |
| Risiko tersisa | Rendah. Build belum dibuktikan; pola sintaks yang dipakai (`is … or …`, property pattern `{ Length: > konstanta }`) didukung C# pada `net9.0` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source: ` M Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyObservationController.cs`. Dokumen `docs/module-blueprints/igd/**` juga berubah — sebagian dari sesi sebelumnya yang belum di-commit |
| Langkah berikutnya | Rizki menjalankan build di atas, lalu uji API manual contoh 1–6. Sesudah itu `FE-IGD-024` (isian Kesimpulan) boleh dimulai |
