# Laporan Perubahan Backend — `BE-HRD-358`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-358` |
| Judul | Verifikasi `POST-MVP` `A52` |
| Slice | Amandemen `A52` — `POST-MVP` (`EPIC HRD-115` s.d. `HRD-119`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B88 |
| Trace | `FR-PK-019` s.d. `FR-PK-031`; `AT-HRD-PK-15` s.d. `-18`, `-32` s.d. `-36`, `-40`, `-41` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-352` ✅, `BE-HRD-353` ✅, `BE-HRD-354` ✅, `BE-HRD-355` ✅, `BE-HRD-356` ✅, `BE-HRD-357` ✅ |
| Klasifikasi | `MEDIUM` — verifikasi tanpa perubahan source aplikasi; uji API devYoga dengan data uji sementara (termasuk workflow uji atas persetujuan pengguna) |
| Task mode | `BACKEND` (verifikasi) |
| Target tulis | Laporan ini; dokumen roadmap/traceability; `Tests/QuilvianSystemBackend.HumanResourceTests/AcceptanceTestIndex.md`. **Tidak ada perubahan source aplikasi** |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`) + perubahan `BE-HRD-357`, `353`, `354`, `355`, `356` yang belum di-commit |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — proyek uji HR 237/237; semua AT `POST-MVP` lulus di harness dan lewat API devYoga. `AT-HRD-PK-35` bagian "diterima untuk diputuskan" semula terhalang cacat lama integrasi koreksi ↔ workflow; cacat itu diperbaiki di [`BE-HRD-FIX-002`](BE-HRD-FIX-002.md) dan AT tersebut lulus (pembaruan 17 November 2026) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate`, `SelfServices` |
| Module / Submodule | `HumanResource` / `SchedulingManagement`, `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | Verifikasi; tidak ada kode baru |
| QBE yang berlaku | `QBE-PERM-001` (registri dan 403 non-superadmin diverifikasi) |
| Tidak berlaku | QBE penulisan kode lain — task ini tidak mengubah source |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diselesaikan

Bukti bahwa seluruh uji penerimaan epic `HRD-115` s.d. `HRD-119` lulus pada kondisi kode gabungan `BE-HRD-352` s.d. `357`, dengan
akun non-superadmin untuk uji 403, dan bahwa startup Development tetap bebas galat registri.

---

## 2. Yang diverifikasi

| Epic | Task | AT | Isi |
| --- | --- | --- | --- |
| `HRD-115` Cadangan siaga dan catatan panggilan | `BE-HRD-352` | `AT-HRD-PK-15`–`18` | Tiga transisi panggilan, status penugasan tetap, `INT-HR-39`; kedaluwarsa dan cadangan berikutnya; cadangan habis; ringkasan dari panggilan diterima pertama |
| `HRD-116` Dispensasi | `BE-HRD-353`, `354` | `AT-HRD-PK-32`–`34` | Dispensasi disetujui → terlambat 0, log mesin tetap; Kepala Unit tidak menyetujui dispensasinya sendiri; batas mundur |
| `HRD-117` Tugas belajar | `BE-HRD-355` | `AT-HRD-PK-12` (data dari API baru) + status harian | Blokir roster `B8`; rekap "Tugas Belajar" |
| `HRD-118` Gangguan mesin | `BE-HRD-356` | `AT-HRD-PK-35`, `-36` | Koreksi merujuk gangguan tanpa lampiran; mencatat kejadian tidak menambah log mesin |
| `HRD-119` Dashboard | `BE-HRD-357` | `AT-HRD-PK-40`, `-41` | Angka kartu = jumlah baris rincian; pegawai biasa 403 |

---

## 3. Dampak

| Aspek | Dampak |
| --- | --- |
| Source aplikasi | Tidak ada perubahan |
| Database devYoga | Data uji sementara `UJI-A52-358` (workflow `ATTENDANCE_CORRECTION`, satu langkah, kejadian gangguan, koreksi, izin `MyAttendanceCorrection` gilang) dibuat lalu dinonaktifkan/dihapus lunak; izin dikembalikan identik |
| Kontrak API | Tidak berubah |

---

## 4. Endpoint

Tidak ada endpoint baru. Endpoint yang diverifikasi tercantum di laporan `BE-HRD-352` s.d. `357`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`, SQLite di memori) | **237/237** lulus | `PASS` | `be358.trx` di scratchpad sesi (`trx358`) |
| — Per kelas `POST-MVP` | `OnCallCallTests` 9/9, `IntHr39RegressionTests` 1/1, `AttendanceDispensationTests` 8/8, `AttendanceDispensationProcessingTests` 5/5, `StudyAssignmentTests` 4/4, `DeviceIncidentTests` 4/4, `AttendanceDashboardTests` 6/6 | `PASS` | `be358.trx` |
| — Hak akses | `A52PermissionRegistryTests` 86/86: validator registri bersih untuk semua controller `A52`, 30 resource/aksi terdaftar, setiap endpoint `A52` baru (termasuk 17 siaga, 9 dispensasi, 5 tugas belajar, 3 gangguan mesin, 2 dashboard) menolak pegawai tanpa kebijakan Akses Role | `PASS` | `be358.trx` |
| Kontrol negatif | Dijalankan per task (`BE-HRD-352` s.d. `357`), per putaran mutasi; setiap mutasi tertangkap uji yang dimaksud dan kode dipulihkan byte demi byte | `PASS` | Laporan masing-masing task |
| Startup Development | "Permission registry valid. 2457 identitas kanonik dari 2457 kemampuan terdaftar." | `PASS` | `be358.log` |
| **API devYoga per AT** (akun non-superadmin `gilang.mahendra` dan dr. Bima; data uji sementara dipulihkan) | | | |
| `AT-HRD-PK-15`–`18` | Lulus; `INT-HR-39` identik; 403 non-superadmin | `PASS` | Laporan `BE-HRD-352` |
| `AT-HRD-PK-32` | gilang terlambat 40 → dr. Bima menyetujui → job `ATT-DISP-…` `Completed` → 0 menit; scan 07.40 utuh | `PASS` | Laporan `BE-HRD-354` |
| `AT-HRD-PK-33` | gilang menyetujui dispensasinya sendiri → `403` | `PASS` | Laporan `BE-HRD-353` |
| `AT-HRD-PK-34` | 5 hari ke belakang (batas 3) → `422` | `PASS` | Laporan `BE-HRD-353` |
| `AT-HRD-PK-12` (data dari API tugas belajar) | Ganti Petugas ke pegawai tugas belajar → `422 B8`; shift tidak berubah | `PASS` | Laporan `BE-HRD-355` |
| `AT-HRD-PK-36` | Log mesin 73 → 73 sesudah catat dan batal kejadian | `PASS` | Laporan `BE-HRD-356` |
| `AT-HRD-PK-35` bagian 2 | 09.30 (di luar rentang) tanpa lampiran → `422` "Jam di luar rentang gangguan wajib disertai lampiran" | `PASS` | Laporan `BE-HRD-356` |
| `AT-HRD-PK-35` bagian 1 — putaran `BE-HRD-358` dengan workflow uji sementara `ATTENDANCE_CORRECTION` (atas persetujuan pengguna; satu langkah, penyetuju superadmin) | Draf 06.50 merujuk kejadian 06.30–09.00 tanpa lampiran → ajukan: **gerbang lampiran lolos**, instance workflow terbentuk dengan satu penugasan penyetuju; lalu `WorkflowService.SubmitAsync` gagal dan `AttendanceCorrectionService.SoftDeleteFailedDraftWorkflowAsync` melempar `DbUpdateConcurrencyException` → `500`. Koreksi tetap `Draft` | `PARTIAL` saat itu — diselesaikan oleh `BE-HRD-FIX-002` (baris berikut) | `at-358.mjs`, `at-log.jsonl` (entri "358 …"), `be358.log` |
| `AT-HRD-PK-35` bagian 1 — sesudah `BE-HRD-FIX-002` | Ajukan 06.50 tanpa lampiran → `200`; koreksi `UnderReview`, workflow `InProgress`, 1 tugas penyetuju; tarik → `200` | `PASS` | Laporan `BE-HRD-FIX-002`; `at-fx2.mjs` |
| `AT-HRD-PK-40` | Semua angka kartu = `TotalData` rincian (superadmin dan Kepala Bidang uji) | `PASS` | Laporan `BE-HRD-357` |
| `AT-HRD-PK-41` | gilang tanpa izin → `403`; berizin tanpa jabatan → `403 VR-PK-27`; Kepala Bidang uji ke IGD → `403` | `PASS` | Laporan `BE-HRD-357` |
| Pemulihan putaran `BE-HRD-358` | Izin gilang dikirim ulang (32 butir) dan dibaca identik; definisi workflow uji `Inactive` + dihapus lunak (0 definisi `ATTENDANCE_CORRECTION` aktif tersisa); langkah, instance, penugasan penyetuju, langkah instance, riwayat status, koreksi, dan kejadian uji dihapus lunak | `PASS` | `at-log.jsonl` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Semua AT `POST-MVP` lulus | Terpenuhi — harness 237/237; API devYoga semua lulus (`AT-HRD-PK-35` bagian 1 sesudah `BE-HRD-FIX-002`) | Bagian 5 |
| Uji 403 dengan akun non-superadmin | Terpenuhi (harness 86 kasus registri; devYoga per task) | Bagian 5 |
| Startup Development tanpa galat registri | Terpenuhi | `be358.log` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan (di luar `A52`) — **diperbaiki di `BE-HRD-FIX-002`** | **Pengajuan koreksi presensi ke workflow gagal 500 di PostgreSQL.** Urutan: `WorkflowService.CreateAsync` berhasil → `SaveChanges` koreksi → `WorkflowService.SubmitAsync` gagal (pesan aslinya tertutup) → `SoftDeleteFailedDraftWorkflowAsync` memuat instance yang sudah dilacak dan menyimpan → `DbUpdateConcurrencyException` → `500`. Jalur ini belum pernah berjalan di devYoga karena workflow `ATTENDANCE_CORRECTION` belum pernah ada. Butuh task perbaikan terpisah (pemilik modul workflow/koreksi), lalu `AT-HRD-PK-35` bagian 1 diuji ulang |
| Prasyarat lingkungan | devYoga belum punya definisi workflow `ATTENDANCE_CORRECTION` permanen; master ini perlu disiapkan (penyetuju = keputusan bisnis) |
| Izin belum diberikan | Aksi baru `POST-MVP` belum diberikan ke jabatan mana pun: `OnCallAssignment : ManageBackup/RecordCall`, `AttendanceDispensation : Read/Create/Approve`, `MyAttendanceDispensation : Read/Create/Cancel`, `StudyAssignment : Read/Create/Update/Cancel`, `AttendanceDeviceIncident : Read/Create/Cancel`, `AttendanceDashboard : Read` |
| Keputusan pemilik modul yang tertunda | Dari laporan task: penerima lonceng dispensasi atas nama (`BE-HRD-353`); tafsir `VR-PK-22` vs `HRD-ASM-77` dan `RequireAttachmentForCorrection` (`BE-HRD-356`); aturan hitung kartu dashboard (`BE-HRD-357`) |
| Perubahan sampingan | `NONE`. Data uji devYoga sudah dipulihkan |
| Interupsi | `NONE` |
| Status Git | Laporan ini (baru); 2 dokumen roadmap/traceability; `AcceptanceTestIndex.md`. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-359` tetap ⛔ (`DSG-Q-02`). Frontend `A52` `POST-MVP`: `FE-HRD-317` s.d. `FE-HRD-322` (bergantung `FE-HRD-306`); `FE-HRD-323` verifikasi browser |
