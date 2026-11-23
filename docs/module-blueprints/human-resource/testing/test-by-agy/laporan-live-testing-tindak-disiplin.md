# Laporan Live Testing — Tindak Disiplin

| Field | Nilai |
| --- | --- |
| Menu | Hubungan Industrial & Terminasi → **Tindak Disiplin** (`/hrd/relations/sanctions`; dahulu berlabel "Teguran") |
| Pemilik menu | HR Relations |
| Task | `FE-HRD-401`, layar `FE-HRD-395`, pensiun halaman lama `FE-HRD-394`, label `FE-HRD-400` |
| Tanggal uji | 23 November 2026 |
| Lingkungan | Frontend build produksi `http://localhost:3000`; backend lokal `https://localhost:7184`; database dev `QuilvianNewDevHamzah` |
| Peramban | Chromium headed, `slowMo` 450 ms, desktop dan ponsel 390 px |
| Akun uji | `uat.pkb.relations@rsmmc.local` (HR Relations), `uat.pkb.asesor@rsmmc.local` (tanpa hak sanksi) |
| Hasil | **7 dari 7 skenario lulus**; 1 temuan UX ringan (P5) |

## 1. Ringkasan

Butir menu kini berlabel **Tindak Disiplin**. Pencarian menu "Teguran" tidak menemukan butir apa pun, dan Teguran Lisan dicari lewat filter Tingkat. Daftar menampilkan dua surat peringatan uji yang terbit lewat alur resmi. Filter Status mengirim `status=Active`, dan filter Tingkat "Teguran Lisan" menyisakan satu baris. Alamat lama `/hr/employee-relations/disciplinary-actions` dialihkan ke layar ini. Selama seluruh uji tidak ada satu pun panggilan ke alamat usang `/v1/hr/workforce-profile/disciplinary-actions`.

## 2. Alur proses bisnis yang diuji

Contoh: Bu Rina (HR Relations) memeriksa teguran lisan yang masih berlaku.

1. Bu Rina memilih **Hubungan Industrial & Terminasi → Tindak Disiplin**.
2. Daftar memuat `SP1/HRD/11/2026/0001` (SP 1) dan `VerbalWarning/HRD/11/2026/0002` (Teguran Lisan) milik pegawai uji "UAT PKB Tono Bawahan".
3. Bu Rina memilih Status **Berlaku**; daftar diminta ulang dengan `status=Active`.
4. Bu Rina memilih Tingkat **Teguran Lisan**; hanya baris teguran lisan yang tersisa.

## 3. Data uji (dibuat lewat API resmi)

| Data | Langkah |
| --- | --- |
| SP 1 `SP1/HRD/11/2026/0001`, terbit 23 Nov 2026, berlaku s.d. 23 Mei 2027 | Insiden "Perilaku Berisiko" → investigasi (2 pemeriksa) → temuan SP1 → terbitkan sanksi |
| Teguran Lisan `VerbalWarning/HRD/11/2026/0002`, terbit 4 Mei 2026, berlaku s.d. 4 Agu 2026 | Terbitkan sanksi tingkat VerbalWarning |

## 4. Rincian test case

| ID | Prioritas | UAT | Skenario | Harapan | Hasil | Status | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| L01 | P1 | HRD-129 | Label menu dan pencarian "Teguran" | Tindak Disiplin → `/hrd/relations/sanctions`; cari "Teguran" kosong (`AT-HRD-PKB-23`) | Sesuai; 0 hasil | ✅ PASS | — |
| L02 | P0 | HRD-128 | Daftar sanksi alur resmi | Dua SP uji tampil | Sesuai | ✅ PASS | `fe-hrd-401/relations-01-tindak-disiplin.png` |
| L03 | P2 | HRD-128 | Filter Status Berlaku | Request `status=Active` 200 | HTTP 200 | ✅ PASS | — |
| L04 | P2 | HRD-128 | Filter Tingkat Teguran Lisan | Hanya baris teguran lisan | Sesuai; SP 1 tersembunyi | ✅ PASS | `fe-hrd-401/relations-02-filter-teguran-lisan.png` |
| L07 | P1 | HRD-128 | Alamat lama | Dialihkan ke `/hrd/relations/sanctions` (`AT-HRD-PKB-24`) | Sesuai | ✅ PASS | — |
| L09 | P4 | HRD-128 | Tampilan ponsel | Tanpa gulir mendatar | 0 px | ✅ PASS | `fe-hrd-401/relations-05-tindak-disiplin-ponsel.png` |
| A04 | P1 | HRD-128 | Asesor membuka alamat langsung | Data sanksi tidak tampil | Tidak ada data; lihat temuan TD-01 | ✅ PASS | `fe-hrd-401/asesor-03-tindak-disiplin-tanpa-hak.png` |

## 5. Endpoint yang diuji

### `[Tags("Corporate - HRD Employee Disciplinary & Just Culture")]`

| Method | Path | Deskripsi | Auth | Request | Response teramati |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/hrd/disciplinary/sanctions` | Daftar sanksi | `EmployeeDisciplinary : ViewSanctions` | `status`, `level`, `search`, halaman | `200`, `PagedResult<SanctionListItemDto>` |
| `POST` | `/api/v1/hrd/disciplinary/incidents` | Data uji: lapor insiden | `EmployeeDisciplinary : CreateIncident` | `CreateIncidentReportRequestDto` | `201` |
| `POST` | `/api/v1/hrd/disciplinary/investigations` | Data uji: buka BAP | `EmployeeDisciplinary : ConductInvestigation` | `InitiateInvestigationRequestDto` | `201` |
| `PUT` | `/api/v1/hrd/disciplinary/investigations/{investigationId}/findings` | Data uji: temuan BAP | `EmployeeDisciplinary : ConductInvestigation` | `RecordBapFindingsRequestDto` | `200` |
| `POST` | `/api/v1/hrd/disciplinary/sanctions` | Data uji: terbitkan SP | `EmployeeDisciplinary : IssueSanction` | `IssueSanctionRequestDto` | `201` |

## 6. Temuan

| ID | Prioritas | Temuan | Dampak | Usulan |
| --- | --- | --- | --- | --- |
| TD-01 | P5 | Pengguna tanpa `EmployeeDisciplinary : ViewSanctions` yang membuka alamat layar langsung hanya melihat judul halaman kosong, tanpa pesan "belum punya hak". Data tidak bocor. | Pengguna bingung karena halaman tampak kosong | Tambahkan pesan hak seperti layar Riwayat Penyelesaian ("Anda belum punya hak untuk melihat …"). Layar ini sudah ada sebelum Paket B; perlu task terpisah. |
| TD-02 | Info (backend) | Nomor surat teguran lisan memakai kode Inggris `VerbalWarning/HRD/11/2026/0002`. | Nomor surat tidak berbahasa Indonesia | Pertimbangkan kode tingkat yang lebih pendek (misalnya `TL`); keputusan pemilik dan backend. |
| TD-03 | Info (backend) | Teguran lisan uji sudah melewati masa berlakunya tetapi masih "Berlaku", karena worker kedaluwarsa backend (EMP-W1) berjalan tiap 6 jam. | Status bisa tertinggal hingga 6 jam | Diterima sebagai perilaku backend; baris ini akan pindah ke Riwayat Penyelesaian setelah worker berjalan. |
