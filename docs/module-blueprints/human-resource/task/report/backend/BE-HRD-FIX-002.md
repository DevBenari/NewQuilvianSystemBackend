# Laporan Perubahan Backend — `BE-HRD-FIX-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-FIX-002` |
| Judul | Pengajuan koreksi presensi ke workflow gagal 500 |
| Slice | Perbaikan cacat di luar roadmap; temuan `BE-HRD-358` (penghadang `AT-HRD-PK-35` bagian 1) |
| Roadmap | Tidak ada kartu. Dicatat pada `roadmap/backend-roadmap.md` bagian 24 di bawah `BE-HRD-358` |
| Trace | `AT-HRD-PK-35`; `HRD-DEC-202` butir 5 (koreksi tetap diputuskan atasan lewat workflow); `POST self-services/human-resource/attendance-corrections/{id}/submit`, `/{id}/cancel` |
| Contract version | `v26` tidak berubah. Perbaikan memulihkan perilaku endpoint yang sudah ada |
| Dependency | `BE-HRD-356` ✅ (rujukan kejadian gangguan), `BE-HRD-358` ✅ (temuan) |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), sekitar 8 berkas diperiksa (1), 2 berkas diubah (0), modul workflow bersama yang dipakai banyak alur HR (3), tanpa perubahan API (0), tanpa schema (0), tanpa keamanan (0), ditambah 1 untuk verifikasi provider-nyata |
| Task mode | `BACKEND` (perbaikan bug atas instruksi pengguna 17 November 2026: "perbaiki bug pengajuan koreksi ke workflow") |
| Target tulis | `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs`, `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceCorrectionService.cs`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`) + perubahan `BE-HRD-357`, `353`–`356` yang belum di-commit; `WorkflowService.cs` bersih sebelum task ini |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — proyek uji HR 237/237; lewat API devYoga (PostgreSQL) koreksi diajukan `200` → `UnderReview`/`InProgress`, lalu ditarik `200` → `Cancelled`/`Withdrawn`; kontrol negatif membuktikan akar masalah dan perbaikan penutup galat |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `WorkflowManagement`, `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru |
| Keberlakuan | `TOUCHED LEGACY` — perbaikan minimal tanpa refactor (legacy ratchet) |
| QBE yang berlaku | `QBE-TXN-001` (penanganan kegagalan transaksi yang tidak mengirim ulang perubahan gagal), `QBE-SVC-001` |
| Tidak berlaku | `QBE-API-001`/`QBE-DTO-001` (kontrak tidak berubah), `QBE-DB-*` (tanpa schema), `QBE-NAM-*` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat | Akibat |
| --- | --- |
| **Akar masalah — `WorkflowService.AddStatusHistory` dan `AddComment`.** Riwayat status dan komentar baru (Id `Guid.NewGuid()` sudah terisi) hanya ditambahkan ke koleksi `instance.StatusHistories`/`instance.Comments`. Pada aksi terhadap instance yang sudah ada, instance dimuat terlacak (`LoadTrackedInstanceAsync`); EF memperlakukan entitas baru ber-Id terisi pada koleksi entitas terlacak sebagai baris lama → `UPDATE` 0 baris → `DbUpdateConcurrencyException`, ditangkap sebagai `DbUpdateException` → "Submit workflow gagal karena terjadi konflik data…" (`409`) | Submit dan aksi lain (tarik, setujui, tolak, kembalikan, minta revisi) pada instance workflow yang sudah dibuat **selalu gagal**. Saat instance dibuat (`CreateAsync`) seluruh graf baru, sehingga pembuatan berhasil — kegagalan baru muncul pada langkah berikutnya. Berlaku untuk semua alur HR yang memakai workflow umum; devYoga belum punya satu pun instance workflow aktif |
| **Penutup galat — `AttendanceCorrectionService.SubmitAsync`.** Sesudah `WorkflowService.SubmitAsync` gagal (dan me-rollback transaksinya), perubahan yang gagal masih tertinggal di change tracker yang sama; `SoftDeleteFailedDraftWorkflowAsync` lalu memanggil `SaveChanges` yang mengirim ulang perubahan itu → `DbUpdateConcurrencyException` tak tertangkap | Pengguna melihat `500` "Terjadi kesalahan pada server", galat aslinya tertutup, dan draf workflow yang gagal tidak terhapus |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs` | `AddStatusHistory`: sesudah `instance.StatusHistories.Add(history)` juga `_dbContext.Set<TrxWorkflowStatusHistory>().Add(history)`. `AddComment` (kini bukan `static`): komentar disimpan di variabel lalu ditambahkan ke koleksi **dan** `_dbContext.Set<TrxWorkflowComment>().Add(...)`. Pola sama dengan `TrxApprovalAction` di berkas yang sama (sudah `Set<>().Add`). CRLF dipertahankan |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceCorrectionService.cs` | Pada cabang submit gagal: `_dbContext.ChangeTracker.Clear()` sebelum `SoftDeleteFailedDraftWorkflowAsync`, sehingga pesan galat workflow sampai ke pengguna dan draf yang gagal terhapus. CRLF dipertahankan |

Penugasan penyetuju dan langkah instance hanya dibuat di `CreateAsync` (graf baru), sehingga tidak terkena pola ini. Tidak ada
perubahan kontrak, schema, atau hak akses.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat | `PASS` | Keluaran build |
| Proyek uji HR (`dotnet test`) — regresi | 237/237 lulus | `PASS` | Keluaran `dotnet test` |
| Uji harness khusus untuk `WorkflowService` | Tidak ditambahkan: rantai dependensi `WorkflowService` (`HumanResourceContextService`, `WorkflowReferenceLifecycleService` beserta layanan siklus cuti/lembur/profil) terlalu berat untuk harness HR; pola EF yang sama sudah dibuktikan diagnosa `AddDetails` di `BE-HRD-356`. Bukti diambil dari PostgreSQL di bawah | `NOT RUN` | — |
| API devYoga (17 November 2026), sebelum perbaikan (`BE-HRD-358`) | Ajukan koreksi → `500`; log: `DbUpdateConcurrencyException` di `SoftDeleteFailedDraftWorkflowAsync`; tarik → `409` | Bukti cacat | `be358.log`, `at-log.jsonl` ("358 …") |
| API devYoga sesudah perbaikan — workflow uji sementara `ATTENDANCE_CORRECTION` (pola yang disetujui pengguna di `BE-HRD-358`; satu langkah, penyetuju superadmin), kejadian gangguan 06.30–09.00, izin `MyAttendanceCorrection` sementara pada jabatan gilang | Draf 06.50 merujuk kejadian, tanpa lampiran → ajukan **`200`** "Attendance correction berhasil di-submit ke workflow."; koreksi `UnderReview`, workflow `InProgress`, 1 tugas penyetuju | `PASS` — juga menuntaskan `AT-HRD-PK-35` bagian 1 | `at-fx2.mjs`, `at-log.jsonl` ("fx2 …") |
| — Aksi kedua pada instance yang sama | Tarik → **`200`** "Attendance correction dan workflow berhasil dibatalkan."; `Cancelled`/`Withdrawn`; 3 riwayat status, 2 komentar tersimpan | `PASS` | `at-log.jsonl` |
| Kontrol negatif: perbaikan `WorkflowService` dicabut sementara (perbaikan penutup galat tetap), dibangun ulang, uji API diulang | Ajukan → **`409`** "Submit workflow gagal karena terjadi konflik data atau request yang sama sudah diproses." (bukan `500`); koreksi tetap `Draft`; draf workflow gagal terhapus lunak, tanpa instance aktif. Membuktikan (a) akar masalah memang penambahan lewat koleksi, (b) galat tidak lagi tertutup. Kode dipulihkan (dibandingkan byte demi byte) dan dibangun ulang | `PASS` | `befx2-mutasi.log`, `at-log.jsonl` |
| Pemulihan devYoga | Izin gilang dikirim ulang (32 butir) dan dibaca identik; definisi workflow uji `Inactive` + dihapus lunak (0 definisi `ATTENDANCE_CORRECTION` aktif); instance, penugasan, langkah instance, riwayat status, komentar, aksi persetujuan, koreksi, dan kejadian uji dihapus lunak — 0 baris aktif tersisa | `PASS` | `at-log.jsonl` ("fx2 Sisa data uji") |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pengajuan koreksi presensi ke workflow tidak lagi `500` | Terpenuhi | devYoga `200` |
| Koreksi yang diajukan menunggu keputusan atasan (`HRD-DEC-202` uji (c), `AT-HRD-PK-35`) | Terpenuhi | `UnderReview` / `InProgress` dengan tugas penyetuju |
| Kegagalan workflow ditampilkan apa adanya | Terpenuhi | Kontrol negatif `409` dengan pesan workflow |
| Tidak ada regresi | Terpenuhi | 237/237 |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dampak lebih luas | Perbaikan di `WorkflowService` berlaku untuk semua alur HR yang memakai workflow umum. **Uji asap 17 November 2026** (data uji sementara `UJI-A52-ASAP` atas persetujuan pengguna: workflow satu langkah penyetuju dr. Bima, jenis + kebijakan cuti uji `CalendarDays` tanpa potong saldo, kebijakan + tarif lembur global; semuanya dihapus lunak sesudahnya, izin gilang dan dr. Bima dikembalikan identik): **cuti** ajukan → setujui dari kotak persetujuan → `Approved`/`Completed`, ajukan → tarik → `Cancelled`/`Withdrawn`; **lembur** (BeforeShift 19.00–21.00 sebelum shift Malam) ajukan → setujui → `ApprovedForWork`/`Completed`, ajukan → tarik → `Cancelled`/`Withdrawn`. Alur lain (perubahan profil, penyesuaian/pembatalan cuti, tukar shift, resign) belum diuji |
| Temuan uji asap | **Lembur yang mulai 00.00–06.59 WIB selalu ditolak** `OVERTIME_DATE_MISMATCH`: `OvertimeSelfServiceService` membandingkan `OvertimeDate` dengan `DateOnly.FromDateTime(startAt)` dalam UTC; `NormalizeUtc` juga membaca jam tanpa zona sebagai UTC (berbeda dari `AppDateTimeHelper.ToUtc` = WIB). **Diperbaiki di [`BE-HRD-FIX-003`](BE-HRD-FIX-003.md)** (17 November 2026). Koreksi: lembur AfterShift sesudah shift malam (mulai 07.00 WIB hari berikutnya) **bukan** akibat cacat ini — itu aturan tanggal lembur yang menunggu keputusan pemilik (lihat laporan `FIX-003` bagian 5) |
| Prasyarat lingkungan | devYoga belum punya definisi workflow `ATTENDANCE_CORRECTION` permanen (penyetuju = keputusan bisnis) |
| Risiko tersisa | `WorkflowService` menangkap `DbUpdateException` tanpa mencatat galat dalamnya (tidak ada logger); kegagalan serupa di masa depan sulit didiagnosis. Tidak diubah di task ini (di luar cakupan minimal) |
| Perubahan sampingan | `NONE`. Mutasi kontrol negatif sudah dipulihkan; data uji devYoga dihapus lunak |
| Interupsi | `NONE` |
| Status Git | 2 berkas source diubah (`WorkflowService.cs` baru disentuh; `AttendanceCorrectionService.cs` juga berisi perubahan `BE-HRD-356`); laporan ini (baru); dokumen roadmap/traceability/laporan `BE-HRD-358`. Tidak ada commit |
