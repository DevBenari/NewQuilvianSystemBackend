# Laporan Perubahan Frontend — `FE-HRD-409`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-409` |
| Nomor lama | `FE-HRD-401` — dinomori ulang saat merge `QuilvianIntegrationBackend` 23 November 2026; nomor lama kini milik amandemen `S-A7-V2` (lihat `blueprint-manifest.md` revisi `69`) |
| Judul | Verifikasi browser lima pintu Paket B |
| Slice | `S-PKB` / Verifikasi |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 31 |
| Trace | UAT PRD 43.10 (`HRD-125` s.d. `HRD-130`); DoD PRD 43.11; seluruh kontrak `v30` |
| Contract version | `v30`, disetujui 23 November 2026 |
| Dependency | `FE-HRD-401` ✅, `FE-HRD-402` ✅, `FE-HRD-408` 🟡, `BE-HRD-462` ✅ |
| Klasifikasi | `VERIFY` — tanpa perubahan source aplikasi |
| Task mode | `CROSS-REPO` (uji di frontend; laporan di repository backend) |
| Izin pemilik | 23 November 2026: "buatkan semua akun dan sandi ya dan lakukan testing" — akun uji dan data uji di DB dev bersama, lewat API resmi |
| Model | Claude Opus 5.5 |
| Commit frontend | `6c1d4e368` (memuat `FE-HRD-401`, `407`, `408`); build yang diuji dibuat dari isi yang sama |
| Commit backend | `a9c1af31`; backend yang diuji adalah proses milik pengguna (`bin/Debug/net9.0`, `https://localhost:7184`) |
| Tanggal | 23 November 2026 |
| Status | 🟡 Sebagian — AC1 belum penuh karena keterbatasan data dan backend (bagian 7) |

---

## 1. Persiapan

### 1.1 Gerbang G3 dan G4

Kelima akun uji dibuat lewat API resmi aplikasi, bukan SQL langsung:

- `POST master-data/organization/positions` untuk membuat jabatan uji;
- `POST master-data/employees` dengan `CreateLoginAccount = true` untuk membuat pegawai beserta akunnya;
- `PUT self-services/profile/password` untuk mengganti sandi awal;
- `POST administrator/setting/role-access/policies` untuk menyimpan policy hak.

Hak di aplikasi ini melekat pada pasangan Departemen × Jabatan. Karena itu setiap akun diberi **jabatan uji baru** agar policy-nya tidak mengubah hak 21 akun nyata di "Human Resource / Manajer HR" dan "Staff HR".

| Peran | Email | Departemen / jabatan uji | Hak yang diberikan |
| --- | --- | --- | --- |
| HR Relations | `uat.pkb.relations@rsmmc.local` | Human Resource / UAT Paket B - HR Relations | `EmployeeDisciplinary` (ViewSanctions, ViewIncidents, CreateIncident, ConductInvestigation, IssueSanction, AppealSanction), `HrdEmployee360 : Read`, `HrdPersonnelActionOrder : Read`, `Employee : Read` |
| Recruiter | `uat.pkb.recruiter@rsmmc.local` | Human Resource / UAT Paket B - Recruiter | `CandidateScreening` (Read, Evaluate), `CandidateEvaluation` (Read, **ViewAll**, Evaluate), `JobVacancy`, `Candidate`, `JobRequisition` : Read |
| Asesor klinis | `uat.pkb.asesor@rsmmc.local` | Keperawatan / UAT Paket B - Asesor Klinis | `CandidateEvaluation` (Read, EvaluateClinical) |
| Atasan langsung | `uat.pkb.atasan@rsmmc.local` | Keperawatan / UAT Paket B - Atasan Langsung | `HrdEmployee360 : Read` + relasi atasan langsung atas pegawai uji "UAT PKB Tono Bawahan" |
| Manajer SDM | `uat.pkb.manajer@rsmmc.local` | Human Resource / UAT Paket B - Manajer SDM | **`HrdWorkforceDashboard : Read`**, `HrdEmployee360 : Read`, `HrdPersonnelActionOrder : Read` |

Setiap akun juga mendapat `Profile : Read/Update`, hak dasar yang diperlukan untuk mengganti sandi awal. Sandi disimpan di `QuilvianSystemFrontendDev/test-with-agy/scripts/pkb-1123-data.json` (folder di-ignore Git) dan tidak ditulis di dokumen ini. Kelima akun login normal tanpa paksaan ganti sandi; `GET /auth/permissions` setiap akun berisi persis hak di atas.

### 1.2 Data uji

| Data | Endpoint |
| --- | --- |
| 6 pegawai "UAT PKB …" (5 berakun, 1 bawahan tanpa akun) | `POST master-data/employees` |
| Relasi atasan langsung bawahan → atasan uji | `POST workforce-profiles/{id}/manager-assignments` |
| Insiden A → BAP → temuan SP1 → `SP1/HRD/11/2026/0001` | `POST /hrd/disciplinary/incidents`, `/investigations`, `PUT …/findings`, `POST /sanctions` |
| Insiden B (Human Error) → BAP → temuan Coaching → `ResolvedNoSanction` | idem tanpa sanksi |
| Teguran Lisan `VerbalWarning/HRD/11/2026/0002` (terbit 4 Mei 2026) | `POST /hrd/disciplinary/sanctions` |
| Catatan disiplin lama (LEGACY) | `POST workforce-profiles/{id}/disciplinary-actions` |

Data rekrutmen **tidak** dibuat. Database dev tidak memiliki pengajuan headcount, permintaan rekrutmen, lowongan, kandidat, atau lamaran, dan pembuatan lowongan mensyaratkan pengajuan headcount yang disetujui melalui workflow Perencanaan SDM bertahap.

### 1.3 Lingkungan

Port 3000 semula dipakai proyek lain milik pengguna; pengguna menghentikannya. Frontend dijalankan dari build produksi terakhir (`node .next/standalone/server.js`, port 3000, origin yang diizinkan CORS backend), dengan `SESSION_SIGNING_SECRET` khusus uji di environment proses karena `.env` tidak memuatnya. Tidak ada berkas pengguna yang diubah.

---

## 2. Hasil verifikasi

Chromium headed (Playwright, `slowMo` 450 ms), login lewat form aplikasi per akun. Skrip: `QuilvianSystemFrontendDev/test-with-agy/scripts/pkb-1123-1*.mjs`; hasil mentah: `test-with-agy/screenshots/fe-hrd-401/hasil-<akun>.json`.

| Akun | Skenario | Lulus | Panggilan alamat usang | Galat halaman |
| --- | ---: | ---: | ---: | ---: |
| Manajer SDM | 8 | 8 | 0 | 0 |
| Recruiter | 7 | 7 | 0 | 0 |
| Asesor | 4 | 4 | 0 | 0 |
| HR Relations | 9 | 9 | 0 | 0 |
| Atasan langsung | 4 | 4 | 0 | 0 |
| **Jumlah** | **32** | **32** | **0** | **0** |

Satu skenario atasan (T03) sempat gagal karena skrip membaca elemen `<main>` yang tidak ada di layar Employee 360. Layarnya sendiri benar menampilkan pesan akses ditolak; skrip diperbaiki dan dijalankan ulang (lulus).

Tangkapan Manajemen Role (superadmin, baca-saja): `fe-hrd-401/superadmin-01-akses-role-manajer-sdm.png` (Hrd Workforce Dashboard : Read tercentang) dan `fe-hrd-401/superadmin-02-akses-role-recruiter.png` (Candidate Evaluation 3 hak terpilih).

Laporan uji per menu (`testing/test-by-agy/`):

| Menu | Laporan | Hasil |
| --- | --- | --- |
| Dashboard SDM | [laporan](../../../testing/test-by-agy/laporan-live-testing-dashboard-sdm.md) | 10/10 |
| Seleksi Administrasi | [laporan](../../../testing/test-by-agy/laporan-live-testing-seleksi-administrasi.md) | 5/5 |
| Tes/Wawancara | [laporan](../../../testing/test-by-agy/laporan-live-testing-tes-wawancara.md) | 5/5 |
| Tindak Disiplin | [laporan](../../../testing/test-by-agy/laporan-live-testing-tindak-disiplin.md) | 7/7, 1 temuan P5 |
| Riwayat Penyelesaian | [laporan](../../../testing/test-by-agy/laporan-live-testing-riwayat-penyelesaian.md) | 2/2, 1 temuan backend P1 |
| Employee 360 — disiplin | [laporan](../../../testing/test-by-agy/laporan-live-testing-employee-360-disiplin.md) | 5/5 |

`npm run lint:errors` (exit 0) dan `npm run build` (exit 0, 894 halaman) dijalankan pada sesi yang sama; sesudahnya tidak ada perubahan source aplikasi, hanya skrip uji di folder yang di-ignore Git.

---

## 3. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC1 — Jalur berhasil dan gagal UAT `HRD-125` s.d. `HRD-130` lulus di layar | **Sebagian** | `HRD-125`, `HRD-129`, `HRD-130` penuh. `HRD-128`: insiden ditutup tanpa sanksi lulus; sanksi selesai belum ada di DB (RP-01, RP-02). `HRD-126`/`HRD-127`: jalur hak dan keadaan kosong lulus; jalur dengan lamaran/evaluasi belum dapat diuji karena DB dev tanpa data rekrutmen. |
| AC2 — `HrdWorkforceDashboard : Read` dan `CandidateEvaluation : ViewAll` terbukti diberikan ke peran uji | Terpenuhi | Tangkapan Manajemen Role dan `GET /auth/permissions` |
| AC3 — Tidak ada panggilan ke `/v1/hr/workforce-profile/disciplinary-actions` (`AT-HRD-PKB-24`) | Terpenuhi | Pencatat request di 32 skenario: 0 |
| AC4 — `npm run lint:errors` dan `npm run build` lulus | Terpenuhi | Exit 0 / exit 0 |
| Tangkapan layar tanpa NIK | Terpenuhi | Bagian disiplin difoto per elemen; layar lain tidak memuat NIK |
| Desktop dan ponsel | Terpenuhi | Dashboard SDM, Seleksi Administrasi, Tindak Disiplin pada 390 px tanpa gulir mendatar |

---

## 4. Temuan

| ID | Prioritas | Temuan | Pemilik |
| --- | --- | --- | --- |
| RP-01 | P1 | Tidak ada kode backend yang menetapkan status sanksi `RevokedOnAppeal`; banding hanya dicatat. "Dicabut lewat Banding" tidak pernah muncul. | Backend / pemilik produk |
| RP-02 | Info | Status `Expired` hanya diisi worker EMP-W1 tiap 6 jam; sanksi yang lewat masa berlaku masih "Berlaku" sampai worker berjalan. | Backend |
| TD-01 | P5 | Layar Tindak Disiplin tanpa hak `ViewSanctions` tampil kosong tanpa pesan hak (data tidak bocor). | Frontend, task terpisah |
| TD-02 | Info | Nomor surat teguran lisan memakai kode Inggris `VerbalWarning/…`. | Backend / pemilik produk |
| DATA-01 | — | DB dev tanpa rantai rekrutmen (headcount → permintaan → lowongan → kandidat → lamaran). | Pemilik data uji |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Belum terpenuhi | AC1 untuk jalur rekrutmen berisi data (`HRD-126`, `HRD-127`) dan bagian sanksi selesai (`HRD-128`). Ketiganya sudah dibuktikan dengan response tiruan di `FE-HRD-404`/`405`/`406`. |
| Data uji yang tertinggal di DB dev | 5 jabatan "UAT Paket B - …", 6 pegawai "UAT PKB …" (5 berakun), 5 policy hak, 1 relasi atasan, 2 insiden, 2 BAP, 2 sanksi, 1 catatan disiplin lama. Angka Dashboard SDM kini termasuk 6 pegawai uji. Boleh dinonaktifkan sesudah verifikasi; ID tercatat di `pkb-1123-data.json`. |
| Server uji | Frontend standalone di port 3000 dijalankan sesi ini dan masih menyala untuk dicoba pengguna. |
| Langkah berikutnya | Uji ulang `HRD-126`/`HRD-127` setelah satu lowongan uji tersedia. Verifikasi bagian sanksi selesai setelah worker EMP-W1 berjalan. Putuskan RP-01. |
