# Laporan Perubahan Backend — `BE-HRD-410`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-410` |
| Judul | Tutup bypass CRUD penempatan langsung dan validator siklus atasan |
| Slice | `S-FIN-M07` / Atasan Langsung (alias `FIN-HRD-07-BE-004`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.4, kartu `BE-HRD-410` |
| Trace | `HRD-DEC-031/036/047/048/055`; instruksi Menu 07 `FIN-HRD-07-BE-004` |
| Contract version | Kontrak SK Kepegawaian dan penempatan existing (`BE-HRD-024`, `BE-HRD-027`); tidak ada endpoint baru |
| Dependency | — |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Controllers/WfpManagerAssignmentController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Deteksi siklus atasan sudah ada di jalur SK, dan `PUT`/`PATCH`/`DELETE` atas atasan utama aktif sudah ditolak. Dua jalan memutar tersisa di `WfpManagerAssignmentController`: (1) `POST` atasan utama untuk pegawai yang belum punya atasan utama tidak memeriksa siklus — A→B→C lalu C diatasi A tetap tersimpan; (2) `PUT` dapat menaikkan penugasan non-utama menjadi atasan utama tanpa SK.

## 2. Proses bisnis

1. Penetapan atasan utama resmi lewat SK Mutasi atau Pengajuan Perubahan Atasan.
2. Data awal (pegawai belum punya atasan utama) boleh dibuat lewat `POST`, tetapi ditolak bila membentuk rantai melingkar per tanggal mulai.
3. `PUT` tidak dapat menjadikan penugasan non-utama sebagai atasan utama; atasan utama aktif/terjadwal tidak dapat diubah (`PUT`), dinonaktifkan (`PATCH /status`), dipindah status utamanya (`PATCH /primary`), atau dihapus (`DELETE`).

**Contoh.** Perawat C → Kepala Ruang B → Kabid A. `POST` atasan utama A = C → ditolak "rantai atasan melingkar".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpManagerAssignmentController.cs` (semua aksi tulis), `HrdPersonnelActionOrderService.WouldCreateCycleAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpManagerAssignmentController.cs` | Cek siklus pada `POST` atasan utama; `PUT` menolak promosi ke atasan utama; helper `WouldCreateManagerCycleAsync` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; dua penolakan `400` baru |
| Database | Tanpa migration |
| Keamanan/Auth | Menutup jalan memutar SK |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Manager Assignment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/manager-assignments` | Data awal atasan; siklus ditolak | `WfpManagerAssignment : Create` |
| `PUT` | `…/manager-assignments/{id}` | Ubah penugasan non-utama; promosi ke utama ditolak | `WfpManagerAssignment : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning nullable dari build pertama diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | 3 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: `POST` C diatasi A pada rantai A→B→C | `400` siklus | `PASS` | `WouldCreateManagerCycleAsync` |
| Penelusuran: `PUT` non-utama → utama | `400` | `PASS` | guard baru |
| Penelusuran: `DELETE` atasan utama | `400` | `PASS` | guard lama |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) A→B→C tidak dapat diubah menjadi C→A lewat SK maupun endpoint langsung | Terpenuhi | SK: `WouldCreateCycleAsync`; endpoint: `WouldCreateManagerCycleAsync` + larangan promosi |
| (2) PUT/PATCH/DELETE primary terjadwal ditolak | Terpenuhi | guard `IsPrimaryManager && IsActive` pada PUT/PATCH; DELETE menolak primary/berbasis SK |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | `BE-HRD-411` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
