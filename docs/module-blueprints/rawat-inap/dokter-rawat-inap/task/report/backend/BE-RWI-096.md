# Laporan Perubahan Backend — `BE-RWI-096`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-096` |
| Judul | Daftar tunggu verifikasi memuat episode tertutup |
| Slice | Gelombang 4 — `DOK-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-096` |
| Trace | `FR-DOK-084`; `RWI-DEC-126`, `RWI-DEC-127` |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-095` — implementasi tersedia pada working tree aktif |
| Klasifikasi | `MEDIUM` — endpoint baca, authorisasi DPJP, agregasi episode, dan indikator overdue |
| Task mode | `BACKEND` |
| Model | GPT-5 |
| Commit backend saat dikerjakan | `36db5e6d1f1e6ee8b3a3dce8aa6c9d4adcc8060a`, branch `MHamzah` |
| Tanggal | 2026-09-16 |
| Status | ✅ **SELESAI 16 September 2026** berdasarkan validasi source; `dotnet build` tidak dijalankan sesuai instruksi pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / ClinicalManagement` |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001` |
| Penempatan aturan | Agregasi dan pemilihan episode berada pada `CpptVerificationService`; controller menangani identitas dokter dan paging |
| Boundary pasien | Daftar hanya membaca nama dan nomor rekam medis selain identitas episode |

## 1. Proses bisnis

Daftar kerja menggabungkan dua sumber:

1. episode `Admitted` atau `DischargePending` yang pengguna login menjadi DPJP aktifnya; dan
2. episode `Closed` yang pengguna login merupakan DPJP terakhirnya, bila
   `includeClosedEpisodes=true`.

Hanya episode yang masih mempunyai catatan `Pending`/`Overdue` yang masuk hasil. Untuk episode
tertutup, catatan yang waktu klinisnya sesudah `ClosedAt` disisihkan karena tidak dapat diproses oleh
pengecualian `BE-RWI-095`. Setelah seluruh catatan yang relevan terverifikasi, episode otomatis
hilang dari daftar tanpa flag tambahan.

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `CpptVerificationService.cs` | Query episode aktif, seleksi episode `Closed` milik DPJP terakhir, agregasi pending, cutoff `ClosedAt`, identitas minimum, dan perhitungan overdue |
| `PatientIntegratedProgressNoteController.cs` | Endpoint `verification-worklist`, resolusi dokter akun login, `includeClosedEpisodes`, dan paging response |
| `CpptVerificationService.cs` | Juga mendefinisikan bentuk item worklist: episode, identitas minimum, jumlah pending, waktu tertua, status closed exception, overdue, dan menit terlambat |

## 3. Dokumentasi endpoint

| Method | Path | Query | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/verification-worklist` | `includeClosedEpisodes=true`, `pageNumber=1`, `pageSize=25` | `PatientIntegratedProgressNote : Read` |

Akun yang tidak tertaut ke data dokter menerima `403`, bukan daftar kosong yang menyesatkan.

## 4. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Episode closed milik DPJP terakhir | **PASS statis** — kandidat harus `Closed` dan resolver terakhir harus sama dengan dokter login |
| Episode closed milik dokter lain | **PASS statis** — tidak ditambahkan ke daftar episode tertutup |
| Keluar setelah seluruh entry selesai | **PASS statis** — item hanya dibentuk dari entry `Pending`/`Overdue` |
| Overdue | **PASS statis** — dihitung dari `VerificationDueAt < nowUtc`; menit terlambat dari due paling awal |
| Cancelled episode | **PASS statis** — tidak masuk sumber aktif maupun sumber closed exception |
| Entri pascapenutupan | **PASS statis** — tidak ditawarkan pada worklist |
| Episode `Closed` tanpa `ClosedAt` | **PASS statis/fail closed** — tidak ditawarkan karena batas prapenutupan tidak dapat dibuktikan |
| Identitas minimum | **PASS statis** — pasien diproyeksikan hanya `Id`, `FullName`, `MedicalRecordNumber` |
| QBE checker `Strict` | **PASS** — nol finding |
| `dotnet build` | **NOT RUN — instruksi eksplisit pengguna** |
| Uji kontrak/runtime | **NOT RUN** |

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — memuat episode `Closed` milik DPJP terakhir | Terpenuhi secara statis | Kandidat status `Closed` dan resolver DPJP terakhir |
| AC-2 — hilang setelah seluruh entri verified | Terpenuhi secara statis | Agregasi hanya pending/overdue |
| AC-3 — episode milik DPJP lain tidak muncul | Terpenuhi secara statis | Perbandingan exact doctor ID |
| AC-4 — lewat batas ditandai overdue | Terpenuhi secara statis | `IsOverdue` dan `LateByMinutes` |
| Laporan, roadmap, traceability | Terpenuhi | Laporan ini dan pembaruan dokumen delivery |

## 6. Catatan penutup

Task ditandai selesai tanpa build sesuai instruksi pemilik. Verifikasi runtime dan Swagger aktual
tetap menunggu build mandiri pemilik. Tidak ada operasi database, Git, atau deployment.
