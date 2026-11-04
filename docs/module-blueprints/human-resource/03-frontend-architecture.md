# Human Resource — Arsitektur Frontend Target

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `03-frontend-architecture.md` |
| Revision | `3` — 3 November 2026: bagian 12 menambah layar `S-D1` Perencanaan Tenaga Kerja. Bagian 1–11 tidak berubah |
| Status | `draft` — **belum** `approved` |
| Owner desain | Technical owner (`HRD-DEC-015`), dengan ruang `DEV_DISCRETION` sesuai `HRD-FE-03` |
| `approved_by` / `approved_at` | **Hanya bagian 12** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| Repository target | `QuilvianSystemFrontendDev` |
| Frontend SHA | `fff76a1b394d4b247c70a04f106c8ec098c9696e` (branch `AgentCodexFrontend`) |
| Backend SHA rujukan kontrak | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| `input_revision` — arsitektur backend | `02-backend-architecture.md` revision `2` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `16` (Amendment Pass Workforce Core & Employee 360, `HRD-DEC-046` s.d. `HRD-DEC-057`) |
| `input_hash` — decision log | `26da64e129b0c34a17b67b0a41f47986b1aedae92a600077d936b0741eb574ee` |
| Kompatibilitas | Tidak ada halaman existing yang dihapus. Satu halaman dipindahkan lokasinya (`S-A6`) dengan pengalihan |

---

## 0. Apa yang dokumen ini kunci, dan apa yang tetap bebas

Dokumen ini mengunci **keterjangkauan dan sumber data**, bukan rupa.

| Dikunci dokumen ini | Tetap `DEV_DISCRETION` |
| --- | --- |
| Layar apa saja yang harus ada | Nama butir menu dan ikonnya |
| Layar mana yang mendapat butir menu, dan layar mana yang dicapai dari layar induk | Urutan butir dan pengelompokan visualnya |
| Route yang dituju setiap butir | Susunan visual, jarak, warna |
| Bagian apa saja yang ada di setiap layar | Apakah sebuah bagian memakai tab, modal, atau drawer |
| Dari endpoint mana setiap bagian mengambil datanya | Component library dan bentuk kontrolnya |
| Tombol apa yang ada dan hak akses yang menjaganya | Penempatan tombol dan animasinya |
| Bunyi keadaan kosong, gagal, dan sedang memuat | Bentuk kerangka pemuatan |

Hierarki wewenang yang dipakai, dari yang paling mengikat:

```text
keamanan / privasi / invariant
  -> brief produk dan UI yang disetujui
  -> design system dan konvensi project
  -> DEV_DISCRETION
```

`HRD-FE-03` mendelegasikan bentuk tampilan daftar, form, dan modal kepada developer, dengan
syarat **wajib mengikuti halaman master data terdekat** dan **dilarang membuat design system
baru**. Karena itu dokumen ini tidak menggambar warna maupun komponen.

---

## 1. Keadaan frontend HR hari ini

Ini titik berangkatnya. Angka di bawah adalah hasil hitung pada snapshot, bukan perkiraan.

| Yang sudah ada | Jumlah | Bukti |
| --- | ---: | --- |
| Kelompok halaman master data HR | 62 folder di `src/app/hr/master-data/` | Pola lengkap `page.jsx` → `create/` → `[slug]/` → `[slug]/update/` |
| Halaman layanan mandiri | 2 | `src/app/self-services/human-resource/employee/dashboard/`, `src/app/karyawan/Absensi-Karyawan/FormAbsensi/` |
| Redux slice HR | 3 kelompok | `src/lib/state/slice/hr/master-data/`, `.../self-service/`, `.../workforce-profile/` |
| Hook HR | 3 kelompok | `src/lib/hooks/hr/**` |
| Konstanta endpoint HR | 2 kelompok | `src/lib/constants/hr/master-data/`, `.../workforce-profile/` |
| Antarmuka atasan | **0** | Tidak ada `src/app/manajer`, tidak ada kotak masuk persetujuan mana pun |
| Test frontend untuk HR | **0** | `HRD-TF-007` |

| Yang menjadi masalah hari ini | Bukti | Status |
| --- | --- | --- |
| Enam butir menu `Administrasi Kepegawaian` menunjuk halaman yang tidak ada | `src/utils/menu-sidebar/menu-items.jsx` baris 516–557 versus seluruh `page.jsx` di bawah `src/app/hr/` | `REPAIR` — `HRD-TF-005`, ditutup `HRD-DEC-012` |
| Halaman absensi berada di luar konvensi | `src/app/karyawan/Absensi-Karyawan/FormAbsensi/` memakai Bahasa Indonesia dan PascalCase | `REPAIR` — ditutup `HRD-DEC-007` |
| 577 endpoint operasional tanpa satu pun pemanggil | Perbandingan 1.343 endpoint backend versus 66 pola URL literal di frontend | `MISSING` — inti pekerjaan dokumen ini |
| Frontend menormalkan huruf besar-kecil field | `attendance-capture-slice.jsx` baris 15–45 menulis `data.userId ?? data.UserId` untuk hampir setiap field | Pertanda kontrak pernah berubah. Setelah kontrak dikunci, lapisan ini tidak lagi diperlukan |

**Koreksi penting yang mengubah prioritas pekerjaan.** Enam menu yang tampak mati itu **bukan**
kemampuan yang hilang. Kontraknya sudah ada, Redux-nya sudah ada, dan komponen editornya sudah
ada — semuanya dipakai lewat halaman detail pegawai di
`/hr/master-data/employee/{employeeSlug}/workforce/{resourceKey}`. Yang hilang hanya **halaman
daftar lintas-pegawainya**. Pekerjaannya jauh lebih kecil daripada dugaan awal.

---

## 2. Kebutuhan layar

Setiap layar punya ID stabil berpola `FE-HRD-nn`. ID **MUST NOT** didaur ulang untuk isi yang
berbeda.

### 2.1 Kelompok A — Administrasi Kepegawaian lintas-pegawai (`S-A1`)

| ID | Layar | Route | Slice | Disposisi |
| --- | --- | --- | --- | --- |
| `FE-HRD-01` | Daftar Perubahan Data Karyawan | `/hr/workforce-core/employee-profile-changes` | `S-A1` | `MISSING / NEW` — halaman transaksi verifikasi HR (`HRD-DEC-052`, `HRD-DEC-055`) |
| `FE-HRD-02` | Daftar Penempatan Organisasi | `/hr/workforce-core/organization-assignments` | `S-A1` | `MISSING / NEW` — daftar pantau penempatan unit (`HRD-DEC-047`, `HRD-DEC-055`) |
| `FE-HRD-03` | Daftar Penempatan Jabatan | `/hr/workforce-core/position-assignments` | `S-A1` | `MISSING / NEW` — daftar pantau jabatan pegawai (`HRD-DEC-049`) |
| `FE-HRD-04` | Daftar Relasi Atasan | `/hr/workforce-core/manager-assignments` | `S-A1` | `MISSING / NEW` — daftar pantau atasan langsung (`HRD-DEC-049`) |
| `FE-HRD-05` | Daftar Riwayat Kepegawaian | `/hr/workforce-core/employment-histories` | `S-A1` | `MISSING / NEW` — linimasa peristiwa dan SK kepegawaian |
| `FE-HRD-06` | Daftar Penetapan Gaji | `/hr/workforce-core/salary-assignments` | `S-A1` | `MISSING / NEW` — nominal disembunyikan (`HRD-DEC-033`) |
| `FE-HRD-07` | Detail & Verifikasi Perubahan Data | `/hr/workforce-core/employee-profile-changes/[slug]` | `S-A1` | `MISSING / NEW` — layar verifikasi berkas bukti & approval (`HRD-DEC-052`) |
| `FE-HRD-08` | Daftar Dokumen Peristiwa Kepegawaian (SK) | `/hr/workforce-core/personnel-actions` | `S-A1` | `MISSING / NEW` — daftar SK mutasi/promosi/rotasi/demosi (`HRD-DEC-048`) |
| `FE-HRD-09` | Formulir Pembuatan Dokumen SK | `/hr/workforce-core/personnel-actions/create` | `S-A1` | `MISSING / NEW` — layar pembuat SK & unggah scan bertandatangan (`HRD-DEC-056`) |
| `FE-HRD-09a` | Detail, Unggah Scan & Persetujuan SK | `/hr/workforce-core/personnel-actions/[slug]` | `S-A1` | `MISSING / NEW` — pratinjau SK, lampiran scan, approval T8, pembatalan (`HRD-DEC-057`) |
| `FE-HRD-09b` | Tampilan Employee 360 Terpadu | `/hr/workforce-core/employee-360/[slug]` | `S-A1` | `MISSING / NEW` — tampilan 360: HR penuh vs Atasan terbatas (`HRD-DEC-053`) |
| `FE-HRD-09c` | Batch Data Awal Penempatan | `/hr/workforce-core/initial-batches` | `S-A1` | `MISSING / NEW` — impor & validasi data awal penempatan pegawai (`HRD-DEC-054`) |
| `FE-HRD-09d` | Form Pengajuan Perubahan Data Atas Nama | `/hr/workforce-core/employee-profile-changes/create-on-behalf` | `S-A1` | `MISSING / NEW` — HR mengajukan ubah data atas nama pegawai (`HRD-DEC-052`) |

### 2.2 Kelompok B — Layanan mandiri pegawai (`S-A2` s.d. `S-A6`)

Seluruhnya di bawah `src/app/self-services/human-resource/employee/**` sesuai `HRD-DEC-007`.

| ID | Layar | Route | Slice | Disposisi |
| --- | --- | --- | --- | --- |
| `FE-HRD-10` | Beranda Pegawai | `/self-services/human-resource/employee/dashboard` | `S-A6` | `EXISTING / REUSE` |
| `FE-HRD-11` | Catat Kehadiran | `/self-services/human-resource/employee/attendance` | `S-A6` | `EXTEND` — pindah dari `src/app/karyawan/**` |
| `FE-HRD-12` | Riwayat Kehadiran Saya | `/self-services/human-resource/employee/attendance/history` | `S-A6` | `MISSING / NEW` |
| `FE-HRD-13` | Saldo Cuti Saya | `/self-services/human-resource/employee/leave/balances` | `S-A2` | `MISSING / NEW` |
| `FE-HRD-14` | Daftar Pengajuan Cuti Saya | `/self-services/human-resource/employee/leave/requests` | `S-A2` | `MISSING / NEW` |
| `FE-HRD-15` | Formulir Pengajuan Cuti | `/self-services/human-resource/employee/leave/requests/create` | `S-A2` | `MISSING / NEW` — layar anak |
| `FE-HRD-16` | Detail Pengajuan Cuti | `/self-services/human-resource/employee/leave/requests/[slug]` | `S-A2` | `MISSING / NEW` — layar anak |
| `FE-HRD-17` | Kalender Cuti Unit | `/self-services/human-resource/employee/leave/calendar` | `S-A2` | `MISSING / NEW` |
| `FE-HRD-18` | Pembatalan Cuti | `/self-services/human-resource/employee/leave/cancellations` | `S-A2` | `MISSING / NEW` |
| `FE-HRD-19` | Kembali Kerja | `/self-services/human-resource/employee/leave/return-to-work` | `S-A2` | `MISSING / NEW` |
| `FE-HRD-20` | Daftar Pengajuan Lembur Saya | `/self-services/human-resource/employee/overtime` | `S-A3` | `MISSING / NEW` |
| `FE-HRD-21` | Formulir Pengajuan Lembur | `/self-services/human-resource/employee/overtime/create` | `S-A3` | `MISSING / NEW` — layar anak |
| `FE-HRD-22` | Detail Pengajuan Lembur | `/self-services/human-resource/employee/overtime/[slug]` | `S-A3` | `MISSING / NEW` — layar anak |
| `FE-HRD-23` | Permohonan Ubah Jadwal | `/self-services/human-resource/employee/schedule-change-requests` | `S-A4` | `MISSING / NEW` |
| `FE-HRD-24` | Permohonan Tukar Shift | `/self-services/human-resource/employee/shift-swap-requests` | `S-A4` | `MISSING / NEW` |
| `FE-HRD-25` | Permohonan Koreksi Kehadiran | `/self-services/human-resource/employee/attendance-corrections` | `S-A5` | `MISSING / NEW` |
| `FE-HRD-26` | Permohonan Perubahan Data Saya (Kelola Data Saya) | `/self-services/human-resource/employee/profile-changes` | `S-A5` | `EXISTING / REUSE` — Diimplementasikan penuh via PLAN-REPAIR-HRD-003 (`HRD-DEC-091..094`) |
| `FE-HRD-27` | Permohonan Pengunduran Diri | `/self-services/human-resource/employee/resignation-requests` | `S-A5` | `MISSING / NEW` |

### 2.3 Kelompok C — Kotak masuk persetujuan atasan (`S-A7`)

| ID | Layar | Route | Slice | Disposisi |
| --- | --- | --- | --- | --- |
| `FE-HRD-30` | Kotak Masuk Persetujuan | `/self-services/human-resource/manager/approval-inbox` | `S-A7` | `MISSING / NEW` |
| `FE-HRD-31` | Detail Tugas Persetujuan | `/self-services/human-resource/manager/approval-inbox/[slug]` | `S-A7` | `MISSING / NEW` — layar anak dari `FE-HRD-30` |
| `FE-HRD-32` | Delegasi Persetujuan Saya | `/self-services/human-resource/manager/approval-delegations` | `S-A7` | `MISSING / NEW` |

### 2.4 Kelompok D — Administrasi waktu kerja (`S-B1` s.d. `S-B4`)

| ID | Layar | Route | Slice | Disposisi |
| --- | --- | --- | --- | --- |
| `FE-HRD-40` | Periode Kehadiran | `/hr/attendance/periods` | `S-B1` | `MISSING / NEW` |
| `FE-HRD-41` | Detail Periode Kehadiran | `/hr/attendance/periods/[slug]` | `S-B1` | `MISSING / NEW` — layar anak |
| `FE-HRD-42` | Kehadiran Harian | `/hr/attendance/dailies` | `S-B1` | `MISSING / NEW` |
| `FE-HRD-43` | Detail Kehadiran Harian | `/hr/attendance/dailies/[slug]` | `S-B1` | `MISSING / NEW` — layar anak |
| `FE-HRD-44` | Rekaman Mentah Kehadiran | `/hr/attendance/raw-logs` | `S-B1` | `MISSING / NEW` |
| `FE-HRD-45` | Pemantauan Koreksi Kehadiran | `/hr/attendance/correction-monitoring` | `S-B1` | `MISSING / NEW` |
| `FE-HRD-46` | Pemrosesan Kehadiran | `/hr/attendance/processing` | `S-B1` | `MISSING / NEW` |
| `FE-HRD-47` | Serah Terima Kehadiran ke Payroll | `/hr/attendance/payroll-handoff` | `S-B1`, `S-B5` | `MISSING / NEW` |
| `FE-HRD-50` | Saldo Cuti Pegawai | `/hr/leave/balances` | `S-B2` | `MISSING / NEW` |
| `FE-HRD-51` | Detail Saldo dan Buku Besar | `/hr/leave/balances/[slug]` | `S-B2` | `MISSING / NEW` — layar anak |
| `FE-HRD-52` | Periode Hak Cuti | `/hr/leave/entitlement-periods` | `S-B2` | `MISSING / NEW` |
| `FE-HRD-53` | Penyesuaian Saldo Cuti | `/hr/leave/adjustments` | `S-B2` | `MISSING / NEW` |
| `FE-HRD-54` | Proses Akrual Cuti | `/hr/leave/accrual-runs` | `S-B2` | `MISSING / NEW` |
| `FE-HRD-55` | Proses Sisa Cuti Dibawa | `/hr/leave/carry-forward-runs` | `S-B2` | `MISSING / NEW` |
| `FE-HRD-56` | Eksekusi Cuti | `/hr/leave/executions` | `S-B2` | `MISSING / NEW` |
| `FE-HRD-57` | Pemanggilan Kembali dari Cuti | `/hr/leave/recalls` | `S-B2` | `MISSING / NEW` |
| `FE-HRD-60` | Periode Lembur | `/hr/overtime/periods` | `S-B3` | `MISSING / NEW` |
| `FE-HRD-61` | Rencana Lembur | `/hr/overtime/plans` | `S-B3` | `MISSING / NEW` |
| `FE-HRD-62` | Detail Rencana Lembur | `/hr/overtime/plans/[slug]` | `S-B3` | `MISSING / NEW` — layar anak |
| `FE-HRD-63` | Realisasi Lembur | `/hr/overtime/realizations` | `S-B3` | `MISSING / NEW` |
| `FE-HRD-64` | Verifikasi Lembur | `/hr/overtime/verifications` | `S-B3` | `MISSING / NEW` |
| `FE-HRD-65` | Cuti Pengganti | `/hr/overtime/compensatory-leaves` | `S-B3` | `MISSING / NEW` |
| `FE-HRD-66` | Serah Terima Lembur ke Payroll | `/hr/overtime/payroll-handoffs` | `S-B3`, `S-B5` | `MISSING / NEW` |
| `FE-HRD-70` | Penempatan Jadwal Kerja | `/hr/scheduling/work-schedule-assignments` | `S-B4` | `MISSING / NEW` |
| `FE-HRD-71` | Periode Roster | `/hr/scheduling/roster-periods` | `S-B4` | `MISSING / NEW` |
| `FE-HRD-72` | Penyusunan Roster | `/hr/scheduling/roster-periods/[slug]` | `S-B4` | `MISSING / NEW` — layar anak |
| `FE-HRD-73` | Penugasan Shift Harian | `/hr/scheduling/shift-assignments` | `S-B4` | `MISSING / NEW` |
| `FE-HRD-74` | Penggantian Shift | `/hr/scheduling/shift-replacements` | `S-B4` | `MISSING / NEW` |
| `FE-HRD-75` | Tenaga Darurat | `/hr/scheduling/emergency-staffing` | `S-B4` | `MISSING / NEW` |
| `FE-HRD-76` | Penugasan Siaga | `/hr/scheduling/on-call-assignments` | `S-B4` | `MISSING / NEW` |
| `FE-HRD-77` | Administrasi Ubah Jadwal | `/hr/scheduling/schedule-change-requests` | `S-B4` | `MISSING / NEW` |
| `FE-HRD-78` | Administrasi Tukar Shift | `/hr/scheduling/shift-swap-requests` | `S-B4` | `MISSING / NEW` |

### 2.5 Kelompok E — Pengembangan orang (`S-C2` s.d. `S-C5`)

| ID | Layar | Route | Slice | Disposisi |
| --- | --- | --- | --- | --- |
| `FE-HRD-80` | Rekaman Pelatihan Pegawai | `/hr/learning/training-records` | `S-C2` | `MISSING / NEW` |
| `FE-HRD-81` | Asesmen Kompetensi | `/hr/learning/competency-assessments` | `S-C2` | `MISSING / NEW` |
| `FE-HRD-82` | Pemenuhan Pelatihan Wajib | `/hr/learning/mandatory-training-compliance` | `S-C2` | `MISSING / NEW` |
| `FE-HRD-85` | Penilaian Kinerja | `/hr/performance/reviews` | `S-C3` | `MISSING / NEW` |
| `FE-HRD-86` | Detail Penilaian Kinerja | `/hr/performance/reviews/[slug]` | `S-C3` | `MISSING / NEW` — layar anak |
| `FE-HRD-90` | Pengunduran Diri | `/hr/lifecycle/resignation-requests` | `S-C4` | `MISSING / NEW` |
| `FE-HRD-91` | Detail Pengunduran Diri | `/hr/lifecycle/resignation-requests/[slug]` | `S-C4` | `MISSING / NEW` — layar anak |
| `FE-HRD-92` | Daftar Periksa Offboarding | `/hr/lifecycle/offboarding-checklists` | `S-C4` | `MISSING / NEW` |
| `FE-HRD-95` | Tindakan Disiplin | `/hr/employee-relation/disciplinary-actions` | `S-C5` | `MISSING / NEW` |
| `FE-HRD-96` | Detail Tindakan Disiplin | `/hr/employee-relation/disciplinary-actions/[slug]` | `S-C5` | `MISSING / NEW` — layar anak |

### 2.6 Layar yang **tidak** dibuat pada pass ini

| Kemampuan | Alasan |
| --- | --- |
| Kredensial, lisensi, kewenangan klinis, SPK/RKK, OPPE, FPPE | `S-C1` `BLOCKED` |
| Rekam kesehatan kerja staf | `S-C6` `BLOCKED` |
| Rekrutmen, benefit, tiket HR | `S-D2` s.d. `S-D4` `BLOCKED`. Perencanaan tenaga kerja (`S-D1`) dirancang pada bagian 12 |
| Perjalanan dinas dan reimbursement | `S-D5` `DEFERRED` |
| Izin pulang cepat sebagai kemampuan berdiri sendiri | Backend-nya `MISSING`, dan policy-nya `[OPEN]` `HRD-Q-47`. Membuat layarnya sekarang berarti membuat layar tanpa endpoint |
| Layar payroll di luar dua serah terima | Kalkulasi run-level `MISSING` di backend, dan `HRD-Q-49` belum dijawab. Layar tanpa endpoint bukan pekerjaan yang bisa diselesaikan |

---

## 3. Peta butir menu

### 3.1 Cara resolver menu bekerja

Nama field bukan pilihan bebas. Resolver sidebar hanya membaca `item.subMenu` pada tingkat 0 dan
`subItems` pada grup di bawahnya. Butir daun memakai `pathname`. Berkas yang disunting saat
implementasi adalah `src/utils/menu-sidebar/menu-items.jsx`.

### 3.2 Pohon menu target

```text
Sumber Daya Manusia                        <- tingkat 0, sudah ada, anaknya di subMenu
├── Master Data                            <- grup tingkat 1, sudah ada, 62 butir
├── Administrasi Kepegawaian               <- grup tingkat 1, sudah ada, 6 butir menunjuk halaman kosong
│   ├── Perubahan Data Karyawan            -> /hr/workforce-core/employee-profile-changes
│   ├── Penempatan Organisasi              -> /hr/workforce-core/organization-assignments
│   ├── Penempatan Jabatan                 -> /hr/workforce-core/position-assignments
│   ├── Relasi Atasan                      -> /hr/workforce-core/manager-assignments
│   ├── Riwayat Kepegawaian                -> /hr/workforce-core/employment-histories
│   └── Penetapan Gaji                     -> /hr/workforce-core/salary-assignments
├── Kehadiran                              <- grup tingkat 1, BARU
│   ├── Periode Kehadiran                  -> /hr/attendance/periods
│   ├── Kehadiran Harian                   -> /hr/attendance/dailies
│   ├── Rekaman Mentah                     -> /hr/attendance/raw-logs
│   ├── Pemantauan Koreksi                 -> /hr/attendance/correction-monitoring
│   ├── Pemrosesan                         -> /hr/attendance/processing
│   └── Serah Terima ke Payroll            -> /hr/attendance/payroll-handoff
├── Cuti                                   <- grup tingkat 1, BARU
│   ├── Saldo Cuti                         -> /hr/leave/balances
│   ├── Periode Hak Cuti                   -> /hr/leave/entitlement-periods
│   ├── Penyesuaian Saldo                  -> /hr/leave/adjustments
│   ├── Proses Akrual                      -> /hr/leave/accrual-runs
│   ├── Proses Sisa Cuti Dibawa            -> /hr/leave/carry-forward-runs
│   ├── Eksekusi Cuti                      -> /hr/leave/executions
│   └── Pemanggilan Kembali                -> /hr/leave/recalls
├── Lembur                                 <- grup tingkat 1, BARU
│   ├── Periode Lembur                     -> /hr/overtime/periods
│   ├── Rencana Lembur                     -> /hr/overtime/plans
│   ├── Realisasi                          -> /hr/overtime/realizations
│   ├── Verifikasi                         -> /hr/overtime/verifications
│   ├── Cuti Pengganti                     -> /hr/overtime/compensatory-leaves
│   └── Serah Terima ke Payroll            -> /hr/overtime/payroll-handoffs
├── Penjadwalan                            <- grup tingkat 1, BARU
│   ├── Penempatan Jadwal Kerja            -> /hr/scheduling/work-schedule-assignments
│   ├── Periode Roster                     -> /hr/scheduling/roster-periods
│   ├── Penugasan Shift Harian             -> /hr/scheduling/shift-assignments
│   ├── Penggantian Shift                  -> /hr/scheduling/shift-replacements
│   ├── Tenaga Darurat                     -> /hr/scheduling/emergency-staffing
│   ├── Penugasan Siaga                    -> /hr/scheduling/on-call-assignments
│   ├── Administrasi Ubah Jadwal           -> /hr/scheduling/schedule-change-requests
│   └── Administrasi Tukar Shift           -> /hr/scheduling/shift-swap-requests
├── Pengembangan Orang                     <- grup tingkat 1, BARU
│   ├── Rekaman Pelatihan                  -> /hr/learning/training-records
│   ├── Asesmen Kompetensi                 -> /hr/learning/competency-assessments
│   ├── Pemenuhan Pelatihan Wajib          -> /hr/learning/mandatory-training-compliance
│   └── Penilaian Kinerja                  -> /hr/performance/reviews
├── Lifecycle Kepegawaian                  <- grup tingkat 1, BARU
│   ├── Pengunduran Diri                   -> /hr/lifecycle/resignation-requests
│   └── Daftar Periksa Offboarding         -> /hr/lifecycle/offboarding-checklists
└── Hubungan Karyawan                      <- grup tingkat 1, BARU
    └── Tindakan Disiplin                  -> /hr/employee-relation/disciplinary-actions

Layanan Kepegawaian                        <- tingkat 0, BARU
├── Beranda Saya                           -> /self-services/human-resource/employee/dashboard
├── Kehadiran Saya                         <- grup tingkat 1, BARU
│   ├── Catat Kehadiran                    -> /self-services/human-resource/employee/attendance
│   ├── Riwayat Kehadiran                  -> .../employee/attendance/history
│   └── Koreksi Kehadiran                  -> .../employee/attendance-corrections
├── Cuti Saya                              <- grup tingkat 1, BARU
│   ├── Saldo Cuti                         -> .../employee/leave/balances
│   ├── Pengajuan Cuti                     -> .../employee/leave/requests
│   ├── Kalender Cuti Unit                 -> .../employee/leave/calendar
│   ├── Pembatalan Cuti                    -> .../employee/leave/cancellations
│   └── Kembali Kerja                      -> .../employee/leave/return-to-work
├── Lembur Saya                            -> .../employee/overtime
├── Jadwal Saya                            <- grup tingkat 1, BARU
│   ├── Ubah Jadwal                        -> .../employee/schedule-change-requests
│   └── Tukar Shift                        -> .../employee/shift-swap-requests
├── Data Saya                              -> .../employee/profile-changes
├── Pengunduran Diri                       -> .../employee/resignation-requests
└── Persetujuan Saya                       <- grup tingkat 1, BARU — hanya untuk atasan
    ├── Kotak Masuk Persetujuan            -> .../manager/approval-inbox
    └── Delegasi Persetujuan               -> .../manager/approval-delegations
```

### 3.3 Tabel butir menu

Kolom **Butir hak akses** disalin dari
[`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md). Butir menu yang
mengarah ke layar yang tidak dapat diakses peran mana pun adalah cacat desain, bukan pilihan
tampilan.

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Sumber Daya Manusia | 0 | — | — | — | — | Sudah ada |
| Master Data | 1 | Sumber Daya Manusia | — | — | — | Sudah ada |
| Administrasi Kepegawaian | 1 | Sumber Daya Manusia | — | — | — | Sudah ada |
| Perubahan Data Karyawan | 2 | Administrasi Kepegawaian | `/hr/workforce-core/employee-profile-changes` | `FE-HRD-01` | `EmployeeProfileChange : Read` | Sudah ada, halaman **belum** ada |
| Penempatan Organisasi | 2 | Administrasi Kepegawaian | `/hr/workforce-core/organization-assignments` | `FE-HRD-02` | `WfpOrganizationAssignment : Read` | Sudah ada, halaman **belum** ada |
| Penempatan Jabatan | 2 | Administrasi Kepegawaian | `/hr/workforce-core/position-assignments` | `FE-HRD-03` | `WfpPositionAssignment : Read` | Sudah ada, halaman **belum** ada |
| Relasi Atasan | 2 | Administrasi Kepegawaian | `/hr/workforce-core/manager-assignments` | `FE-HRD-04` | `WfpManagerAssignment : Read` | Sudah ada, halaman **belum** ada |
| Riwayat Kepegawaian | 2 | Administrasi Kepegawaian | `/hr/workforce-core/employment-histories` | `FE-HRD-05` | `WfpEmploymentHistory : Read` | Sudah ada, halaman **belum** ada |
| Penetapan Gaji | 2 | Administrasi Kepegawaian | `/hr/workforce-core/salary-assignments` | `FE-HRD-06` | `WfpSalaryAssignment : Read` | Sudah ada, halaman **belum** ada |
| Dokumen SK Kepegawaian | 2 | Administrasi Kepegawaian | `/hr/workforce-core/personnel-actions` | `FE-HRD-08` | `PersonnelActionOrder : Read` | Baru |
| Batch Data Awal | 2 | Administrasi Kepegawaian | `/hr/workforce-core/initial-batches` | `FE-HRD-09c` | `InitialPlacementBatch : Read` | Baru |
| Kehadiran | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Periode Kehadiran | 2 | Kehadiran | `/hr/attendance/periods` | `FE-HRD-40` | `AttendancePeriod : Read` | Baru |
| Kehadiran Harian | 2 | Kehadiran | `/hr/attendance/dailies` | `FE-HRD-42` | `AttendanceDaily : Read` | Baru |
| Rekaman Mentah | 2 | Kehadiran | `/hr/attendance/raw-logs` | `FE-HRD-44` | `AttendanceRawLog : Read` | Baru |
| Pemantauan Koreksi | 2 | Kehadiran | `/hr/attendance/correction-monitoring` | `FE-HRD-45` | `AttendanceCorrection : Read` | Baru |
| Pemrosesan | 2 | Kehadiran | `/hr/attendance/processing` | `FE-HRD-46` | `AttendanceProcessing : Read` | Baru |
| Serah Terima ke Payroll | 2 | Kehadiran | `/hr/attendance/payroll-handoff` | `FE-HRD-47` | `AttendancePayrollHandoff : Read` | Baru |
| Cuti | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Saldo Cuti | 2 | Cuti | `/hr/leave/balances` | `FE-HRD-50` | `LeaveBalance : Read` | Baru |
| Periode Hak Cuti | 2 | Cuti | `/hr/leave/entitlement-periods` | `FE-HRD-52` | `LeaveEntitlementPeriod : Read` | Baru |
| Penyesuaian Saldo | 2 | Cuti | `/hr/leave/adjustments` | `FE-HRD-53` | `LeaveAdjustment : Read` | Baru |
| Proses Akrual | 2 | Cuti | `/hr/leave/accrual-runs` | `FE-HRD-54` | `LeaveAccrualRun : Read` | Baru |
| Proses Sisa Cuti Dibawa | 2 | Cuti | `/hr/leave/carry-forward-runs` | `FE-HRD-55` | `LeaveCarryForwardRun : Read` | Baru |
| Eksekusi Cuti | 2 | Cuti | `/hr/leave/executions` | `FE-HRD-56` | `LeaveExecution : Read` | Baru |
| Pemanggilan Kembali | 2 | Cuti | `/hr/leave/recalls` | `FE-HRD-57` | `LeaveRecall : Read` | Baru |
| Lembur | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Periode Lembur | 2 | Lembur | `/hr/overtime/periods` | `FE-HRD-60` | `OvertimePeriod : Read` | Baru |
| Rencana Lembur | 2 | Lembur | `/hr/overtime/plans` | `FE-HRD-61` | `OvertimePlan : Read` | Baru |
| Realisasi | 2 | Lembur | `/hr/overtime/realizations` | `FE-HRD-63` | `OvertimeRealization : Read` | Baru |
| Verifikasi | 2 | Lembur | `/hr/overtime/verifications` | `FE-HRD-64` | `OvertimeVerification : Read` | Baru |
| Cuti Pengganti | 2 | Lembur | `/hr/overtime/compensatory-leaves` | `FE-HRD-65` | `OvertimeCompensatoryLeave : Read` | Baru |
| Serah Terima ke Payroll | 2 | Lembur | `/hr/overtime/payroll-handoffs` | `FE-HRD-66` | `OvertimePayrollHandoff : Read` | Baru |
| Penjadwalan | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Penempatan Jadwal Kerja | 2 | Penjadwalan | `/hr/scheduling/work-schedule-assignments` | `FE-HRD-70` | `WorkScheduleAssignment : Read` — **Rencana**, lihat catatan | Baru |
| Periode Roster | 2 | Penjadwalan | `/hr/scheduling/roster-periods` | `FE-HRD-71` | `RosterPeriod : Read` — **Rencana** | Baru |
| Penugasan Shift Harian | 2 | Penjadwalan | `/hr/scheduling/shift-assignments` | `FE-HRD-73` | `ShiftAssignment : Read` — **Rencana** | Baru |
| Penggantian Shift | 2 | Penjadwalan | `/hr/scheduling/shift-replacements` | `FE-HRD-74` | `ShiftReplacement : Read` — **Rencana** | Baru |
| Tenaga Darurat | 2 | Penjadwalan | `/hr/scheduling/emergency-staffing` | `FE-HRD-75` | `EmergencyStaffing : Read` — **Rencana** | Baru |
| Penugasan Siaga | 2 | Penjadwalan | `/hr/scheduling/on-call-assignments` | `FE-HRD-76` | `OnCallAssignment : Read` — **Rencana** | Baru |
| Administrasi Ubah Jadwal | 2 | Penjadwalan | `/hr/scheduling/schedule-change-requests` | `FE-HRD-77` | `ScheduleChangeRequest : Read` | Baru |
| Administrasi Tukar Shift | 2 | Penjadwalan | `/hr/scheduling/shift-swap-requests` | `FE-HRD-78` | `ShiftSwapRequest : Read` | Baru |
| Pengembangan Orang | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Rekaman Pelatihan | 2 | Pengembangan Orang | `/hr/learning/training-records` | `FE-HRD-80` | `WorkforceTrainingRecord : Read` | Baru |
| Asesmen Kompetensi | 2 | Pengembangan Orang | `/hr/learning/competency-assessments` | `FE-HRD-81` | `WorkforceCompetencyAssessment : Read` | Baru |
| Pemenuhan Pelatihan Wajib | 2 | Pengembangan Orang | `/hr/learning/mandatory-training-compliance` | `FE-HRD-82` | `WorkforceTrainingRecord : Read` | Baru |
| Penilaian Kinerja | 2 | Pengembangan Orang | `/hr/performance/reviews` | `FE-HRD-85` | `PerformanceReview : Read` | Baru |
| Lifecycle Kepegawaian | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Pengunduran Diri | 2 | Lifecycle Kepegawaian | `/hr/lifecycle/resignation-requests` | `FE-HRD-90` | `ResignationRequest : Read` | Baru |
| Daftar Periksa Offboarding | 2 | Lifecycle Kepegawaian | `/hr/lifecycle/offboarding-checklists` | `FE-HRD-92` | `OffboardingChecklist : Read` — **Rencana** | Baru |
| Hubungan Karyawan | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Tindakan Disiplin | 2 | Hubungan Karyawan | `/hr/employee-relation/disciplinary-actions` | `FE-HRD-95` | `WorkforceDisciplinaryAction : Read` | Baru |
| Layanan Kepegawaian | 0 | — | — | — | — | Baru |
| Beranda Saya | 1 | Layanan Kepegawaian | `/self-services/human-resource/employee/dashboard` | `FE-HRD-10` | — konteks pengguna, tanpa permission khusus | Baru |
| Kehadiran Saya | 1 | Layanan Kepegawaian | — | — | — | Baru |
| Catat Kehadiran | 2 | Kehadiran Saya | `.../employee/attendance` | `FE-HRD-11` | — **tidak dijaga `[AccessPermission]`**, lihat catatan | Baru |
| Riwayat Kehadiran | 2 | Kehadiran Saya | `.../employee/attendance/history` | `FE-HRD-12` | — sama | Baru |
| Koreksi Kehadiran | 2 | Kehadiran Saya | `.../employee/attendance-corrections` | `FE-HRD-25` | `MyAttendanceCorrection : Read` | Baru |
| Cuti Saya | 1 | Layanan Kepegawaian | — | — | — | Baru |
| Saldo Cuti | 2 | Cuti Saya | `.../employee/leave/balances` | `FE-HRD-13` | `MyLeaveBalance : Read` | Baru |
| Pengajuan Cuti | 2 | Cuti Saya | `.../employee/leave/requests` | `FE-HRD-14` | `MyLeaveRequest : Read` | Baru |
| Kalender Cuti Unit | 2 | Cuti Saya | `.../employee/leave/calendar` | `FE-HRD-17` | `MyLeaveCalendar : Read` | Baru |
| Pembatalan Cuti | 2 | Cuti Saya | `.../employee/leave/cancellations` | `FE-HRD-18` | `MyLeaveCancellation : Read` | Baru |
| Kembali Kerja | 2 | Cuti Saya | `.../employee/leave/return-to-work` | `FE-HRD-19` | `MyReturnToWork : Read` | Baru |
| Lembur Saya | 1 | Layanan Kepegawaian | `.../employee/overtime` | `FE-HRD-20` | `MyOvertime : Read` | Baru |
| Jadwal Saya | 1 | Layanan Kepegawaian | — | — | — | Baru |
| Ubah Jadwal | 2 | Jadwal Saya | `.../employee/schedule-change-requests` | `FE-HRD-23` | `MyScheduleChange : Read` | Baru |
| Tukar Shift | 2 | Jadwal Saya | `.../employee/shift-swap-requests` | `FE-HRD-24` | `MyShiftSwap : Read` | Baru |
| Data Saya | 1 | Layanan Kepegawaian | `.../employee/profile-changes` | `FE-HRD-26` | `MyProfileChange : Read` | Baru |
| Pengunduran Diri | 1 | Layanan Kepegawaian | `.../employee/resignation-requests` | `FE-HRD-27` | `MyResignation : Read` | Baru |
| Persetujuan Saya | 1 | Layanan Kepegawaian | — | — | — | Baru |
| Kotak Masuk Persetujuan | 2 | Persetujuan Saya | `.../manager/approval-inbox` | `FE-HRD-30` | `ApprovalInbox : Read` | Baru |
| Delegasi Persetujuan | 2 | Persetujuan Saya | `.../manager/approval-delegations` | `FE-HRD-32` | `ApprovalDelegation : Read` | Baru |

### 3.4 Layar yang sengaja **tidak** mendapat butir menu

| Layar | Dicapai dari |
| --- | --- |
| `FE-HRD-07` Detail & Verifikasi Perubahan Data | `FE-HRD-01`, dengan menekan baris pada daftar (`HRD-DEC-055`) |
| `FE-HRD-09` Formulir Pembuatan Dokumen SK | `FE-HRD-08`, tombol "Buat Dokumen SK", atau tombol "Mulai SK" dari baris layar pantau (`HRD-DEC-055`) |
| `FE-HRD-09a` Detail, Unggah Scan & Persetujuan SK | `FE-HRD-08`, dengan menekan nomor SK pada tabel |
| `FE-HRD-09b` Tampilan Employee 360 Terpadu | Seluruh layar Administrasi Kepegawaian (`FE-HRD-01` s.d. `06`), tombol "Lihat Profil 360", atau kotak masuk persetujuan atasan (`HRD-DEC-053`) |
| `FE-HRD-09d` Form Pengajuan Perubahan Data Atas Nama | `FE-HRD-01`, tombol "Ajukan Atas Nama Pegawai" (`HRD-DEC-052`) |
| `FE-HRD-15` Formulir Pengajuan Cuti | `FE-HRD-14`, tombol Ajukan Cuti |
| `FE-HRD-16` Detail Pengajuan Cuti | `FE-HRD-14`, menekan baris |
| `FE-HRD-21` Formulir Pengajuan Lembur | `FE-HRD-20`, tombol Ajukan Lembur |
| `FE-HRD-22` Detail Pengajuan Lembur | `FE-HRD-20`, menekan baris |
| `FE-HRD-31` Detail Tugas Persetujuan | `FE-HRD-30`, menekan baris |
| `FE-HRD-41` Detail Periode Kehadiran | `FE-HRD-40`, menekan baris |
| `FE-HRD-43` Detail Kehadiran Harian | `FE-HRD-42`, menekan baris |
| `FE-HRD-51` Detail Saldo dan Buku Besar | `FE-HRD-50`, menekan baris |
| `FE-HRD-62` Detail Rencana Lembur | `FE-HRD-61`, menekan baris |
| `FE-HRD-72` Penyusunan Roster | `FE-HRD-71`, menekan baris periode |
| `FE-HRD-86` Detail Penilaian Kinerja | `FE-HRD-85`, menekan baris |
| `FE-HRD-91` Detail Pengunduran Diri | `FE-HRD-90`, menekan baris |
| `FE-HRD-96` Detail Tindakan Disiplin | `FE-HRD-95`, menekan baris |

**Aturan yang mengikat:** layar yang tidak terdaftar di menu **dan** tidak punya jalur masuk dari
layar mana pun dihitung **belum selesai**, walaupun kodenya sudah ada dan build-nya lulus.
Pendaftaran butir menu **MUST** menjadi acceptance criteria salah satu task layar, atau task
tersendiri bila tidak masuk task mana pun.

### 3.5 Dua catatan yang tidak boleh dilewat

**Pertama — butir hak akses bertanda "Rencana".** Sembilan butir menu menunjuk layar yang
endpoint-nya belum ada. Butir hak aksesnya juga belum ada di backend. Butir menu itu **MUST NOT**
dipasang sebelum endpoint dan permission-nya benar-benar ada, karena akan menghasilkan menu yang
menunjuk halaman kosong — persis cacat yang sedang diperbaiki `S-A1`.

**Kedua — dua layar tanpa penjagaan hak akses per aksi.** `FE-HRD-11` dan `FE-HRD-12` memanggil
`AttendanceSelfServiceController`, yang **tidak memiliki `[AccessPermission]` pada action-nya**.
Layarnya tetap memerlukan login, tetapi tidak ada butir hak akses yang dapat dipakai untuk
menyembunyikan tombolnya. Ini dicatat sebagai temuan pada
[`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md), bukan
diselesaikan dengan mengarang nama permission di frontend.

---

## 4. Skema fitur per layar

Bentuk yang berulang cukup digambar sekali lalu dirujuk. Enam layar daftar tidak perlu enam
gambar yang sama.

### 4.1 Pola A — Layar daftar transaksi

Dipakai oleh: `FE-HRD-01` s.d. `FE-HRD-06`, `FE-HRD-14`, `FE-HRD-20`, `FE-HRD-40`, `FE-HRD-42`,
`FE-HRD-44`, `FE-HRD-50`, `FE-HRD-53`, `FE-HRD-56`, `FE-HRD-57`, `FE-HRD-61`, `FE-HRD-63`,
`FE-HRD-64`, `FE-HRD-77`, `FE-HRD-78`, `FE-HRD-80`, `FE-HRD-81`, `FE-HRD-85`, `FE-HRD-90`,
`FE-HRD-95`.

```text
+- <Judul Layar> --------------------------------------------- FE-HRD-nn -+
| [ringkasan: total | menunggu | selesai | bermasalah]                     |
+---------------------------------------------------------------------------+
| [cari]  [Unit v] [Status v] [Periode v] [Urutkan v]        [Aksi utama]   |
+---------------------------------------------------------------------------+
| Kolom 1 | Kolom 2 | Kolom 3 | Status | Tanggal |                          |
| ------- | ------- | ------- | chip   | ------- | [Detail]                 |
+---------------------------------------------------------------------------+
| memuat -> kerangka baris, bukan layar kosong                             |
| kosong -> "Belum ada data pada saringan ini."          [Atur ulang]      |
| gagal  -> "Data gagal dimuat."                         [Coba lagi]       |
+- Halaman 1 dari n ------------------------ [< Sebelumnya] [Berikutnya >] +
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Jumlah per keadaan | `GET <base>/summary` | `<Resource> : Read` | Gagal → kartu ringkasan diganti garis putus dengan tombol coba lagi; tabel tetap dimuat |
| Saringan | Pilihan unit, status, periode, dan urutan | `GET <base>/filters/metadata` | `<Resource> : Read` | Gagal → saringan dinonaktifkan, tabel tetap dimuat tanpa saringan |
| Tabel | Daftar berhalaman | `GET <base>/` | `<Resource> : Read` | Kosong → "Belum ada data pada saringan ini." Gagal → "Data gagal dimuat." dengan tombol coba lagi |
| Aksi utama | Tombol yang berbeda per layar | endpoint aksi masing-masing | Butir aksi masing-masing | Tombol yang tidak berhak **MUST** disembunyikan, bukan ditampilkan lalu ditolak |

**Aturan yang mengikat pola ini:** ketiga endpoint pendukung — `filters/metadata`, `summary`,
dan list utama — memang tersedia seragam di hampir seluruh controller HR `[EXISTING]`. Frontend
**MUST NOT** menghitung sendiri isi saringan maupun angka ringkasan.

### 4.2 `FE-HRD-06` — Daftar Penetapan Gaji

Layar ini digambar terpisah karena datanya sensitif. Aturan keterlihatannya sudah diputuskan `[DECISION]` `HRD-DEC-033`.

```text
+- Penetapan Gaji ---------------------------------------------- FE-HRD-06 -+
| [total penetapan | berlaku bulan ini | menunggu persetujuan]              |
+----------------------------------------------------------------------------+
| [cari nama/NIK]  [Unit v] [Periode berlaku v] [Status v]                   |
+----------------------------------------------------------------------------+
| Pegawai | Unit | Kelas Gaji | Berlaku Sejak | Status | Utama |             |
| ------- | ---- | ---------- | ------------- | chip   | ya   | [Detail]    |
+----------------------------------------------------------------------------+
| Kolom nominal gaji TIDAK ADA pada daftar ini - HRD-DEC-033.                |
| Nominal dibaca terpisah di halaman detail, dan hanya oleh pemegang         |
| butir sensitif ViewAmount. Nilainya tidak dikirim, bukan disamarkan.       |
+----------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Jumlah penetapan pada periode berjalan | `GET .../salary-assignments/summary` | `WfpSalaryAssignment : Read` | Gagal → kartu diganti tombol coba lagi |
| Tabel | Daftar lintas-pegawai | `GET /hr/workforce-core/salary-assignments` — **Rencana (belum tersedia)** | `WfpSalaryAssignment : ReadAll` — **Rencana** | Kosong → "Belum ada penetapan gaji pada periode ini." |
| Kolom nominal | **Tidak ada pada daftar** `[DECISION]` `HRD-DEC-033` | — | — | Nominal dibaca terpisah pada halaman detail lewat `GET .../{id}/amount` dengan butir `WfpSalaryAssignment : ViewAmount` |

**Aturan yang berlaku** `[DECISION]` `HRD-DEC-033` — `SALARY_AMOUNT_HIDDEN_BY_DEFAULT`:

| Tempat | Nominal gaji | Yang boleh tampil |
| --- | --- | --- |
| Daftar lintas pegawai — layar ini | **Tidak ada** | Pegawai, unit, kelas gaji, mata uang, tanggal berlaku, penanda utama, status persetujuan |
| Detail gaji satu pegawai | Boleh, **hanya** bagi pemegang `WfpSalaryAssignment : ViewAmount` | Seluruh isi penetapan |
| Laporan atau ekspor massal | **Tidak diberikan pada MVP** | — |

**Dua aturan pelaksanaan yang mengikat frontend:**

1. **Kolom nominal tidak dikirim, bukan disembunyikan.** Backend **MUST NOT** menyertakan
   nilainya pada response daftar. Menyembunyikannya di layar berarti nominal tetap melintas
   jaringan dan tetap terbaca siapa pun yang membuka alat pengembang peramban.
2. **Butir baca umum tidak membuka nominal.** `WfpSalaryAssignment : Read` maupun `: ReadAll`
   **tidak** menyiratkan `: ViewAmount`. Frontend **MUST NOT** menyimpulkan sebaliknya.

### 4.2a `FE-HRD-08` dan `FE-HRD-09a` — Dokumen SK Kepegawaian (`HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`)

Layar ini mengelola seluruh peristiwa kepegawaian (Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara) dalam satu dokumen utuh.

```text
+- Dokumen SK Kepegawaian ---------------------------------------- FE-HRD-08 -+
| [Total SK: 12 | Menunggu Persetujuan: 3 | Dijadwalkan: 4 | Berlaku: 5]        |
+-------------------------------------------------------------------------------+
| [cari nomor SK/nama] [Kategori v] [Status v] [Periode Tanggal v] [+ Buat SK]  |
+-------------------------------------------------------------------------------+
| No SK       | Pegawai | Kategori | Dari -> Ke          | Tgl Berlaku | Status |
| ----------- | ------- | -------- | ------------------- | ----------- | ------ |
| SK/001/2026 | Rina    | Promosi  | Rawat Inap -> ICU   | 01/12/2026  | Jadwal |
|             |         | + Mutasi | Staf -> Ketua Tim   |             |        |
+-------------------------------------------------------------------------------+
| Aksi per baris: [Lihat Detail & Scan SK] [Batalkan (jika Dijadwalkan)]        |
+-------------------------------------------------------------------------------+
```

```text
+- Detail & Persetujuan Dokumen SK ------------------------------ FE-HRD-09a -+
| Nomor SK: SK/001/2026   | Kategori: Promosi & Mutasi | Status: Menunggu       |
| Pegawai : Ns. Rina (NIK: 3171xxx)                    | Tgl Efektif: 01/12/2026|
+-------------------------------------------------------------------------------+
| Perubahan Penempatan:                                                         |
| - Unit Organisasi: Rawat Inap Lantai 3 -> ICU                                |
| - Jabatan        : Perawat Pelaksana -> Ketua Tim                             |
| - Atasan Langsung: Ka. Ruang Rawat Inap -> Ka. Ruang ICU                      |
| - Tautan Gaji    : Penyesuaian Gaji Promosi (Alur Terpisah - HRD-DEC-048)     |
+-------------------------------------------------------------------------------+
| Lampiran Wajib: Scan SK Bertandatangan                                        |
| [Pratinjau PDF SK: sk-rina-promosi-signed.pdf]                [Unduh Berkas]  |
+-------------------------------------------------------------------------------+
| Alasan Peristiwa: Pengembangan kompetensi dan rotasi berkala unit kritis.    |
| Jejak Pengajuan : Dibuat oleh Sari (HR Admin) pada 20/11/2026                 |
+-------------------------------------------------------------------------------+
| Aksi Penyetuju (HR Manager Dodi):                                             |
| [Setujui SK]  [Tolak SK (Alasan wajib)]  [Minta Perbaikan]                    |
| *Penyetuju dilarang sama dengan pembuat (HRD-DEC-031). Tombol disable jika self|
+-------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Angka SK per status | `GET .../personnel-action-orders/summary` | `PersonnelActionOrder : Read` | Gagal -> kartu ringkasan diganti tombol coba lagi |
| Tabel SK | Daftar peristiwa kepegawaian | `GET .../personnel-action-orders` | `PersonnelActionOrder : Read` | Kosong -> "Belum ada dokumen peristiwa kepegawaian." |
| Unggah Scan SK | Berkas scan bertandatangan | `POST .../{id}/attachments` | `PersonnelActionOrder : Update` | Wajib terunggah sebelum tombol Ajukan aktif (`HRD-DEC-056`) |
| Persetujuan | Setujui / Tolak SK | `POST .../{id}/approve` atau `/reject` | `PersonnelActionOrder : Approve` | Pembuat dilarang menyetujui sendiri (`HRD-DEC-031`). Gagal periode tertutup -> 409 (`HRD-DEC-050`) |
| Pembatalan | Pengajuan batal SK terjadwal | `POST .../{id}/cancel-request` | `PersonnelActionOrder : Cancel` | Hanya diizinkan sebelum tanggal berlaku tiba (`HRD-DEC-057`) |

### 4.2b `FE-HRD-09b` — Tampilan Employee 360 Terpadu (`HRD-DEC-053`)

Layar ini menyajikan profil utuh seorang pegawai dengan kendali akses berbasis peran (`HR` vs `Atasan Langsung`).

```text
+- Employee 360 ------------------------------------------------ FE-HRD-09b -+
| [Foto] Ns. Rina, S.Kep                                                        |
| No. Pegawai: EMP-00421  | Status: Tetap | Tgl Masuk: 01/03/2022               |
+-------------------------------------------------------------------------------+
| [Tab: Profil Kerja] [Tab: Riwayat SK] [Tab: Kehadiran & Cuti] [Tab: Pribadi*] |
+-------------------------------------------------------------------------------+
| Unit Utama    : ICU (berlaku sejak 01/12/2026)                                |
| Jabatan Utama : Ketua Tim                                                     |
| Atasan Langsung: Ns. Andi (Kepala Ruangan ICU)                                |
| Unit Tambahan : Tim Tanggap Bencana (Plt / Sekunder)                          |
| Masa Kontrak  : Berlaku s.d. 31/12/2027 (Aktif)                               |
| Sertifikasi   : BTCLS (berlaku s.d. 2028), Kardiologi Dasar                   |
+-------------------------------------------------------------------------------+
| Ringkasan Tahun Ini:                                                          |
| - Kehadiran: 98.4% (Hadir: 210, Terlambat: 2, Koreksi: 1)                     |
| - Saldo Cuti: 8 hari tersisa dari hak 12 hari                                 |
+-------------------------------------------------------------------------------+
| *Tab Pribadi & Kompensasi (HANYA UNTUK HR - DIBLOKIR UNTUK ATASAN):          |
| - Gaji Pokok & Tunjangan: Hanya pemegang hak ViewAmount                       |
| - Rekening Bank & NPWP  : Tersembunyi untuk Atasan Langsung (HRD-DEC-053)     |
| - Catatan Kedisiplinan  : Tersembunyi untuk Atasan Langsung                   |
+-------------------------------------------------------------------------------+
```

| Wilayah | Tampilan HR | Tampilan Atasan Langsung (`HRD-DEC-053`) | Bukan Atasan Langsung |
| --- | --- | --- | --- |
| Identitas Kerja | Nama, NIK, No. Pegawai, Unit, Jabatan, Atasan, Status Kontrak | Nama, No. Pegawai, Unit, Jabatan, Atasan, Status Kontrak (**NIK & Paspor disembunyikan**) | **Ditolak 403 Forbidden** |
| Riwayat Karir & SK | Seluruh riwayat mutasi, promosi, rotasi, demosi | Riwayat mutasi/promosi/rotasi unit & jabatan | Ditolak 403 |
| Kompetensi & Diklat | Lengkap beserta sertifikat | Lengkap sertifikasi kompetensi | Ditolak 403 |
| Kehadiran & Cuti | Ringkasan dan detail kehadiran/cuti | Ringkasan kehadiran dan cuti bawahan | Ditolak 403 |
| Gaji & Rekening | Tampil jika memegang `ViewAmount` | **DILARANG TAMPIL (HRD-DEC-053)** | Ditolak 403 |
| Catatan Disiplin | Sesuai izin HR | **DILARANG TAMPIL (HRD-DEC-053)** | Ditolak 403 |

### 4.2c `FE-HRD-01` dan `FE-HRD-07` — Daftar & Detail Verifikasi Perubahan Data Karyawan (`HRD-DEC-052`, `HRD-DEC-055`)

```text
+- Perubahan Data Karyawan (Sisi HR) ---------------------------- FE-HRD-01 -+
| [Total Permohonan: 15 | Menunggu Verifikasi: 6 | Perlu Revisi: 2 | Disetujui: 7]
+-------------------------------------------------------------------------------+
| [Cari nama/NIK] [Kategori v] [Status v]            [+ Ajukan Atas Nama Pegawai]|
+-------------------------------------------------------------------------------+
| No Pengajuan | Pegawai | Kategori | Diajukan Oleh | Tgl Pengajuan | Status    |
| ------------ | ------- | -------- | ------------- | ------------- | --------- |
| REQ-CHG-0081 | Budi    | Rekening | HR Sari (ONB) | 20/11/2026    | Menunggu  |
| REQ-CHG-0082 | Siti    | Alamat   | Mandiri       | 21/11/2026    | Diverifikasi
+-------------------------------------------------------------------------------+
| Aksi: [Buka Verifikasi] -> menuju FE-HRD-07                                  |
+-------------------------------------------------------------------------------+
```

```text
+- Detail & Verifikasi Perubahan Data --------------------------- FE-HRD-07 -+
| No: REQ-CHG-0081 | Pegawai: Budi Santoso (NIK: 3201xxx) | Kategori: Rekening  |
| Pengaju: HR Sari (Atas Nama Pegawai - HRD-DEC-052)                            |
| Alasan Pengajuan Atas Nama: Pegawai dinas lapangan dan kesulitan akses portal|
+-------------------------------------------------------------------------------+
| Rincian Perubahan Field Sensitif:                                             |
| - Nama Bank        : BCA -> Mandiri                                           |
| - Nomor Rekening   : 1234567890 -> 9876543210 (Sensitif)                      |
| - Nama Pemilik     : Budi Santoso -> Budi Santoso                             |
+-------------------------------------------------------------------------------+
| Berkas Bukti Wajib:                                                           |
| [Lampiran: foto-buku-tabungan-mandiri.jpg]                     [Lihat Bukti]  |
+-------------------------------------------------------------------------------+
| Verifikasi HR (Petugas Dodi):                                                 |
| [Mulai Verifikasi] [Setujui] [Minta Revisi (Alasan)] [Tolak (Alasan)] [Terapkan]|
| *Verifikator MUST NOT sama dengan pengaju (Sari tidak bisa verifikasi sendiri)|
+-------------------------------------------------------------------------------+
```

Status keputusan ini **`SECURITY_APPROVED`** — diputuskan pemilik produk dan ditandatangani
`Project final decision authority — Security` pada 2026-08-30.

### 4.3 `FE-HRD-30` — Kotak Masuk Persetujuan

Ini layar yang paling menentukan apakah seluruh rantai persetujuan HR punya muka atau tidak.
Hari ini **tidak ada satu pun antarmuka persetujuan** di frontend.

```text
+- Kotak Masuk Persetujuan ------------------------------------- FE-HRD-30 -+
| [menunggu saya: n] [didelegasikan ke saya: n] [lewat batas waktu: n]      |
+----------------------------------------------------------------------------+
| [Menunggu] [Riwayat]   [Jenis transaksi v] [Unit v] [Batas waktu v]        |
+----------------------------------------------------------------------------+
| Jenis    | Pemohon | Ringkas         | Diajukan | Batas Waktu | Status |   |
| Cuti     | ------- | 3 hari, 1-3 Sep | -------- | 2 hari lagi | chip   |[>]|
| Lembur   | ------- | 4 jam, 28 Ags   | -------- | terlewat    | chip   |[>]|
+----------------------------------------------------------------------------+
| memuat -> kerangka baris                                                  |
| kosong -> "Tidak ada pengajuan yang menunggu persetujuan Anda."           |
| gagal  -> "Kotak masuk gagal dimuat."                    [Coba lagi]     |
+- Halaman 1 dari n ------------------------ [< Sebelumnya] [Berikutnya >] +
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Jumlah menunggu, didelegasikan, dan lewat batas waktu | `GET /approval-inbox/summary` | `ApprovalInbox : Read` | Gagal → kartu diganti tombol coba lagi |
| Saringan | Jenis transaksi, unit, batas waktu | `GET /approval-inbox/filters/metadata` | `ApprovalInbox : Read` | Gagal → saringan dinonaktifkan |
| Tab Menunggu | Tugas yang belum diputuskan | `GET /approval-inbox?view=open` | `ApprovalInbox : Read` | Kosong → "Tidak ada pengajuan yang menunggu persetujuan Anda." |
| Tab Riwayat | Tugas yang sudah diputuskan | `GET /approval-inbox?view=completed` | `ApprovalInbox : Read` | Kosong → "Belum ada pengajuan yang Anda putuskan." |
| Didelegasikan ke saya | Tugas yang dialihkan orang lain kepada saya | `GET /approval-inbox/delegated-to-me` | `ApprovalInbox : Read` | Kosong → "Tidak ada pelimpahan persetujuan untuk Anda." |
| Baris | Ringkasan seragam lintas jenis transaksi | bagian dari list yang sama | `ApprovalInbox : Read` | — |

**Pagar terpenting layar ini** `[DECISION]` `HRD-DEC-018`: yang boleh diseragamkan hanya
**bentuk baris ringkasan, cara memfilter dan mengurutkan, penanda status, dan cara berpindah ke
detail**. Aturan bisnis tetap milik domain masing-masing.

**Contoh supaya tidak salah bangun.** Seorang kepala unit membuka kotak masuk dan melihat dua
baris: satu permohonan cuti dan satu permohonan lembur. Keduanya tampil dengan bentuk ringkasan
yang seragam. Namun ketika ia membuka permohonan cuti, yang diperiksa adalah aturan saldo cuti
dan matriks persetujuan cuti; ketika ia membuka permohonan lembur, yang diperiksa adalah aturan
kelayakan lembur dan matriks persetujuan lembur. Batas waktu tanggapan dan jalur eskalasi keduanya
juga berbeda, dan **perbedaan itu tetap berlaku**.

Karena itu, keputusan Setujui dan Tolak **boleh** dilakukan dari kotak masuk lewat
`POST /approval-inbox/{assignmentId}/approve` dan `.../reject`, tetapi setiap keputusan yang
memerlukan konteks domain — misalnya melihat sisa saldo cuti sebelum menyetujui — **MUST**
membuka layar detail transaksinya.

### 4.4 `FE-HRD-31` — Detail Tugas Persetujuan

```text
+- Cuti Tahunan - Perawat Melati (samaran) --------------------- FE-HRD-31 -+
| Menunggu Persetujuan   diajukan 3 hari lalu   batas waktu 2 hari lagi     |
| [Setujui] [Tolak] [Minta Perbaikan] [Kembalikan] [Verifikasi] [Akui]      |
+---------------+------------------------------------------------------------+
| - Ringkasan   |                                                            |
| - Isi         |   isi bagian terpilih                                      |
|   Pengajuan   |                                                            |
| - Riwayat     |                                                            |
|   Persetujuan |                                                            |
| - Komentar    |                                                            |
| - Lampiran    |                                                            |
+---------------+------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Jenis transaksi, pemohon, status, batas waktu | `GET /approval-inbox/{assignmentId}` | `ApprovalInbox : Read` | Gagal → seluruh layar diganti pesan beserta tombol coba lagi |
| Tombol Setujui | — | `POST /approval-inbox/{assignmentId}/approve` | `ApprovalInbox : Approve` | Tombol yang tidak berhak **MUST** disembunyikan |
| Tombol Tolak | Alasan **wajib** diisi | `POST /approval-inbox/{assignmentId}/reject` | `ApprovalInbox : Reject` | Sama |
| Tombol Minta Perbaikan | Alasan **wajib** diisi | `POST /approval-inbox/{assignmentId}/request-revision` | `ApprovalInbox : RequestRevision` | Sama |
| Tombol Kembalikan | Alasan **wajib** diisi | `POST /approval-inbox/{assignmentId}/return` | `ApprovalInbox : Return` | Sama |
| Tombol Verifikasi | Hanya muncul pada langkah bertipe verifikasi | `POST /approval-inbox/{assignmentId}/verify` | `ApprovalInbox : Verify` | Sama |
| Tombol Akui | Hanya muncul pada langkah bertipe akui | `POST /approval-inbox/{assignmentId}/acknowledge` | `ApprovalInbox : Acknowledge` | Sama |
| Riwayat persetujuan | Urutan langkah beserta pelaku dan waktunya | `GET /workflow-instances/{id}` | `WorkflowInstance : Read` | Kosong → "Belum ada keputusan pada pengajuan ini." |
| Komentar | Percakapan pada pengajuan | `GET /workflow-instances/{id}/comments` | `WorkflowComment : Read` | Kosong → "Belum ada komentar." |
| Lampiran | Berkas pendukung | `GET /workflow-instances/{id}/attachments` | `WorkflowAttachment : Read` | Kosong → "Tidak ada lampiran." |
| Tautan ke detail transaksi | Membuka layar domain aslinya | route domain masing-masing | butir hak akses domain | Selalu ada. **Satu kemampuan, satu tempat** — kotak masuk hanya menautkan |

### 4.5 `FE-HRD-40` dan `FE-HRD-41` — Periode Kehadiran

Layar inilah yang menentukan apakah payroll dapat berjalan tepat waktu.

```text
+- Detail Periode Kehadiran September ---------------------------FE-HRD-41 -+
| Open   1-30 September   1.240 pegawai   18 pengecualian belum selesai     |
| [Pratinjau Tutup] [Antrikan Pemrosesan] [Tutup Periode] [Batalkan]        |
+---------------------------------------------------------------------------+
| Penghalang penutupan:                                                     |
|  - 18 pengecualian pemblokir berstatus Open / Under Review                |
|  - 3 permohonan koreksi masih berjalan                                    |
|  [Lihat pengecualian]  [Lihat koreksi]                                    |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Kode periode, status, rentang tanggal, jumlah pegawai | `GET /attendance/periods/{id}` | `AttendancePeriod : Read` | Gagal → seluruh layar diganti pesan |
| Penghalang penutupan | Daftar hal yang membuat periode belum bisa ditutup | `GET /attendance/periods/{id}/close-preview` | `AttendancePeriod : Read` | Kosong → "Tidak ada penghalang. Periode siap ditutup." |
| Tombol Antrikan Pemrosesan | — | `POST /attendance/periods/{id}/enqueue-processing` | `AttendancePeriod : Process` | Disembunyikan bila tidak berhak |
| Tombol Tutup Periode | — | `POST /attendance/periods/{id}/close` | `AttendancePeriod : Close` | **Dinonaktifkan** selama masih ada penghalang, dengan keterangan alasannya. Frontend **MUST NOT** menghitung sendiri kelayakan penutupan — ia menampilkan jawaban `close-preview` |
| Tombol Buka Kembali | Hanya muncul saat status `Closed` | `POST /attendance/periods/{id}/reopen` | `AttendancePeriod : Reopen` | Disembunyikan bila tidak berhak |
| Tombol Batalkan | — | `POST /attendance/periods/{id}/cancel` | `AttendancePeriod : Cancel` | Disembunyikan bila tidak berhak |

### 4.6 `FE-HRD-14` s.d. `FE-HRD-16` — Pengajuan cuti pegawai

```text
+- Ajukan Cuti ------------------------------------------------- FE-HRD-15 -+
| Saldo Cuti Tahunan: 8,5 hari tersisa dari 12 hari                        |
+---------------------------------------------------------------------------+
| Jenis Cuti      [v]  <- dari GET leave/requests/balances/options          |
| Tanggal Mulai   [ ]  Tanggal Selesai [ ]                                 |
| ( ) Sehari penuh  ( ) Setengah hari  ( ) Per jam                         |
|     Jam mulai [ ] Jam selesai [ ]   <- hanya bila Per jam dipilih        |
| Alasan          [ ]  <- dari GET leave/requests/reasons/options           |
| Keterangan      [ ]                                                       |
| Lampiran        [Pilih berkas]                                            |
+---------------------------------------------------------------------------+
| Perhitungan: 3 hari akan dipotong dari saldo                             |
| <- angka ini SELALU dari POST leave/requests/calculate, tidak dihitung   |
|    frontend                                                               |
+---------------------------------------------------------------------------+
| [Simpan Draft] [Ajukan]                                                   |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala saldo | Sisa saldo per jenis cuti | `GET /self-services/human-resource/leave/requests/balances/options` | `MyLeaveRequest : Read` | Kosong → "Anda belum memiliki saldo cuti pada periode ini." Ajukan **dinonaktifkan** |
| Pilihan jenis cuti | Jenis yang berhak diambil pegawai ini | endpoint yang sama | `MyLeaveRequest : Read` | Gagal → form dinonaktifkan dengan tombol coba lagi |
| Pilihan alasan | Alasan baku | `GET .../leave/requests/reasons/options` | `MyLeaveRequest : Read` | Gagal → isian alasan menjadi teks bebas |
| Perhitungan | Jumlah hari yang akan dipotong | `POST .../leave/requests/calculate` | `MyLeaveRequest : Read` | Gagal → tombol Ajukan **dinonaktifkan** dengan keterangan "Perhitungan gagal. Coba lagi sebelum mengajukan." |
| Lampiran | Berkas pendukung | `POST .../leave/requests/{id}/attachments` | `MyLeaveRequest : Update` | Gagal unggah → pengajuan tetap tersimpan sebagai draft; berkas dapat diunggah ulang |
| Tombol Simpan Draft | — | `POST .../leave/requests` | `MyLeaveRequest : Create` | — |
| Tombol Ajukan | — | `POST .../leave/requests/{id}/submit` | `MyLeaveRequest : Submit` | Ditolak backend → pesan penolakan ditampilkan apa adanya dari backend |

**Aturan yang paling mengikat layar ini:** frontend **MUST NOT** menghitung ulang saldo maupun
kelayakan cuti. Seluruh angka berasal dari backend. Alasannya bukan gaya arsitektur — bila
frontend dan backend menghitung dengan cara sedikit berbeda, pegawai akan melihat angka yang
tidak sama dengan yang benar-benar dipotong dari saldonya.

### 4.7 `FE-HRD-24` — Permohonan Tukar Shift

Layar ini berbeda dari permohonan lain karena melibatkan **dua pihak** dan **dua tahap
persetujuan**.

```text
+- Ajukan Tukar Shift ------------------------------------------ FE-HRD-24 -+
| Shift saya      [v]  <- GET shift-swap-requests/assignment-options        |
| Rekan yang dituju [v] <- GET shift-swap-requests/target-options           |
| Shift rekan     [v]  <- GET shift-swap-requests/target-assignment-options |
+---------------------------------------------------------------------------+
| Pratinjau: apakah pertukaran ini melanggar aturan istirahat?             |
| <- POST shift-swap-requests/validate-preview                              |
+---------------------------------------------------------------------------+
| [Simpan Draft] [Kirim ke Rekan]                                           |
+---------------------------------------------------------------------------+

Alur dua tahap yang WAJIB terlihat di layar:
  Kirim ke Rekan -> menunggu jawaban rekan -> rekan menerima
    -> baru boleh Teruskan ke Atasan -> menunggu persetujuan atasan
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Pilihan shift saya | Penugasan shift saya yang dapat ditukar | `GET .../shift-swap-requests/assignment-options` | `MyShiftSwap : Read` | Kosong → "Anda tidak memiliki shift yang dapat ditukar pada periode ini." |
| Pilihan rekan | Rekan yang memenuhi syarat | `GET .../shift-swap-requests/target-options` | `MyShiftSwap : Read` | Kosong → "Tidak ada rekan yang memenuhi syarat pertukaran." |
| Pratinjau | Hasil pemeriksaan aturan | `POST .../shift-swap-requests/validate-preview` | `MyShiftSwap : Read` | Gagal → Kirim ke Rekan **dinonaktifkan** |
| Tombol Kirim ke Rekan | — | `POST .../shift-swap-requests/{id}/submit-to-target` | `MyShiftSwap : Submit` | — |
| Tombol Jawab (di sisi rekan) | Terima atau Tolak | `POST .../shift-swap-requests/{id}/target-response` | `MyShiftSwap : Respond` | — |
| Tombol Teruskan ke Atasan | **Hanya aktif** setelah rekan menerima | `POST .../shift-swap-requests/{id}/submit-approval` | `MyShiftSwap : Submit` | Ditekan sebelum rekan menerima → ditolak backend. Frontend **MUST** menonaktifkannya lebih dulu agar penolakan itu tidak pernah terjadi |

**Mengapa dua tahap ini tidak boleh disederhanakan.** Backend menegakkan guard eksplisit:
permohonan tidak dapat maju ke persetujuan atasan bila rekan belum menerima `[EXISTING]`. Status
`TargetRejected` adalah keadaan akhir yang **tidak pernah** mencapai persetujuan atasan.
Menggabungkan kedua tahap di layar akan menghasilkan tombol yang selalu ditolak.

### 4.8 `FE-HRD-25` — Permohonan Koreksi Kehadiran

```text
+- Ajukan Koreksi Kehadiran ------------------------------------ FE-HRD-25 -+
| Hari yang dikoreksi: 27 Agustus 2026                                     |
| Tercatat: masuk 08:15, pulang 17:00, terlambat 15 menit                  |
+---------------------------------------------------------------------------+
| Jenis koreksi  [v]                                                        |
| Usulan perbaikan:  masuk [ ]  pulang [ ]                                 |
| Alasan         [ ]  <- WAJIB                                             |
| Bukti          [Pilih berkas]                                             |
+---------------------------------------------------------------------------+
| Rekaman mentah hari itu TIDAK BERUBAH oleh koreksi ini.                  |
| Yang berubah adalah hasil olahannya, setelah disetujui.                  |
+---------------------------------------------------------------------------+
| [Simpan Draft] [Ajukan]                                                   |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Data kehadiran hari itu apa adanya | `GET /self-services/human-resource/attendance/{id}` | — **tidak dijaga `[AccessPermission]`**, lihat bagian 3.5 | Gagal → seluruh layar diganti pesan |
| Alasan | Teks wajib | — | `MyAttendanceCorrection : Create` | Kosong → Ajukan **dinonaktifkan** |
| Bukti | Berkas pendukung | `POST .../attendance-corrections/{id}/evidence` | `MyAttendanceCorrection : Update` | Unggah kedua **menggantikan** yang pertama; tidak perlu menghapus lebih dulu `[EXISTING]` |
| Keterangan rekaman mentah | Kalimat tetap | — | — | Selalu ditampilkan. Ini bukan hiasan — ia mencegah pegawai mengira koreksi mengubah bukti absensinya |
| Tombol Ajukan | — | `POST .../attendance-corrections/{id}/submit` | `MyAttendanceCorrection : Submit` | — |

**Catatan untuk sisi HR (`FE-HRD-45`).** `[DECISION]` `HRD-DEC-028` menambahkan jalur koreksi
**atas nama pegawai**. Layar HR memerlukan satu tombol tambahan, "Ajukan atas nama pegawai", yang
membuka form yang sama ditambah pemilih pegawai dan isian alasan mengapa pegawai tidak dapat
mengajukan sendiri. Endpointnya `POST /attendance/correction-requests/on-behalf` —
**Rencana (belum tersedia)**.

### 4.9 `FE-HRD-71` dan `FE-HRD-72` — Roster

Seluruh layar penjadwalan roster memanggil endpoint yang **belum ada**. Layarnya dirancang, tetapi
tidak boleh dibangun sebelum backend `EXTEND` selesai.

```text
+- Penyusunan Roster - Unit Melati, September ------------------ FE-HRD-72 -+
| Draft   1-30 September   24 pegawai   3 hari belum tercukupi              |
| [Validasi] [Ajukan] [Terbitkan] [Kunci] [Batalkan]                        |
+---------------------------------------------------------------------------+
|        | 1 | 2 | 3 | 4 | 5 | ... | 30 |                                   |
| Nama A | P | P | S | S | L | ... | P  |   P=Pagi S=Siang M=Malam L=Libur  |
| Nama B | S | S | M | M | L | ... | S  |                                   |
+---------------------------------------------------------------------------+
| Peringatan: 3 hari tidak memenuhi jumlah tenaga minimum                   |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Unit, periode, jumlah pegawai, jumlah masalah | `GET /scheduling/roster-periods/{id}` — **Rencana** | `RosterPeriod : Read` — **Rencana** | Gagal → seluruh layar diganti pesan |
| Kisi jadwal | Penugasan shift per pegawai per tanggal | `GET /scheduling/roster-periods/{id}/assignments` — **Rencana** | `RosterAssignment : Read` — **Rencana** | Kosong → "Roster belum disusun. Mulai dari template atau salin periode sebelumnya." |
| Peringatan | Hari yang tidak memenuhi jumlah tenaga minimum | bagian dari response yang sama | sama | Kosong → tidak ada peringatan ditampilkan |
| Tombol Terbitkan | — | `POST /scheduling/roster-periods/{id}/publish` — **Rencana** | `RosterPeriod : Publish` — **Rencana** | Disembunyikan bila tidak berhak |

---

## 5. Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md),
bukan dikarang ulang di sini.

| Peran rumah sakit | Yang dapat dilakukan di frontend HR | Yang **tidak** dapat dilakukan |
| --- | --- | --- |
| Pegawai | Mencatat kehadiran; melihat kehadiran, saldo, dan pengajuannya sendiri; mengajukan cuti, lembur, ubah jadwal, tukar shift, koreksi kehadiran, perubahan data, pengunduran diri | Melihat data pegawai lain; menyetujui apa pun |
| Atasan atau kepala unit | Seluruh kemampuan pegawai; membuka kotak masuk persetujuan; memutuskan pengajuan yang ditugaskan kepadanya; mendelegasikan persetujuannya | Menyetujui pengajuan yang **tidak** ditugaskan kepadanya — gate `AssignedApproverUserId` |
| HR Admin | Master data; administrasi kepegawaian; administrasi kehadiran, cuti, lembur, penjadwalan; pengembangan orang; lifecycle | Melihat nominal gaji pada daftar lintas-pegawai selama `HRD-Q-20` belum dijawab |
| HR Manager | Seluruh kemampuan HR Admin; membuka kembali periode; override acknowledgement pemanggilan kembali | Mengubah histori `Completed` saat payroll terkunci |
| Petugas payroll | Menutup periode kehadiran dan lembur; menjalankan serah terima; melihat rekonsiliasi | Mengubah status pembayaran — tidak ada endpointnya sama sekali |
| Auditor | Membaca daftar dan riwayat | Mengubah data apa pun |

**Peringatan yang harus dibaca sebelum mengunci matriks ini.** Pemetaan peran rumah sakit di
atas adalah **usulan**, bukan bukti. Audit `PHASE 2A.1` membuktikan bahwa peran seperti
`Supervisor`, `Manager`, `HrAdmin`, dan `Payroll` pada domain lembur **tidak terhubung** ke
pemeriksaan identitas apa pun di backend — keduanya hanya nilai default field. Penegakan nyata
adalah `[AccessPermission]` generik per aksi, yang terputus dari kosakata peran. Pemetaan peran
ke butir hak akses yang sebenarnya masih `[OPEN]` — `HRD-Q-33`.

---

## 6. Data dan status yang dikonsumsi

### 6.1 Cara frontend HR memanggil backend

Modul HR **tidak** memakai folder `src/lib/services/`. Panggilan API dibuat langsung di dalam
Redux thunk memakai `InstanceAxios`, dengan alamat disimpan sebagai konstanta di
`src/lib/constants/hr/**`. Pola baru **MUST** mengikuti pola ini, bukan memperkenalkan lapisan
service baru.

```text
URL
  -> src/app/hr/<domain>/<fitur>/page.jsx        (tipis)
  -> src/components/view/hr/<domain>/...          (isi layar)
  -> src/lib/hooks/hr/<domain>/use-....jsx        (orkestrasi)
  -> src/lib/state/slice/hr/<domain>/...-slice.jsx (thunk + reducer)
  -> src/lib/constants/hr/<domain>/...-constants.jsx (alamat endpoint)
  -> src/lib/axiosInstance/InstanceAxios.jsx
  -> Backend API
```

### 6.2 Cara menampilkan status

Setiap status ditampilkan sebagai penanda yang dapat dibaca orang umum, **bukan** nilai mentah
dari backend. Pemetaannya diambil dari `GET <base>/filters/metadata`, yang memang menyediakan
daftar status beserta labelnya `[EXISTING]`.

| Kelompok status | Cara menampilkan |
| --- | --- |
| Menunggu keputusan (`Submitted`, `WaitingApproval`, `UnderReview`, `PendingTarget`, `PendingApproval`) | Penanda netral, dengan keterangan siapa yang sedang ditunggu |
| Berhasil (`Approved`, `Applied`, `Completed`, `Verified`, `Posted`) | Penanda positif |
| Perlu tindakan pemohon (`NeedRevision`, `TargetRejected`) | Penanda perhatian, disertai tombol tindak lanjut |
| Berakhir tanpa hasil (`Rejected`, `Cancelled`, `Expired`) | Penanda netral gelap, tanpa tombol tindak lanjut |
| Bermasalah (`Failed`, `Error`, `Conflict`) | Penanda bahaya, disertai keterangan dan tombol coba lagi bila backend menyediakannya |

**Aturan:** frontend **MUST NOT** menerjemahkan status dengan tabel yang ditulis tangan di kode.
Bila sebuah status baru muncul dari backend dan tidak dikenali, ia ditampilkan apa adanya, bukan
disembunyikan.

---

## 7. Penanganan keadaan

| Keadaan | Perlakuan yang mengikat |
| --- | --- |
| Sedang memuat | Kerangka baris pada tabel, kerangka kartu pada ringkasan. **Bukan** layar kosong dan **bukan** pemutar yang menutup seluruh layar |
| Kosong | Kalimat yang menjelaskan sebabnya, bukan sekadar "Tidak ada data". Bila kosong karena saringan, sediakan tombol Atur Ulang |
| Gagal | Kalimat yang dibaca petugas, bukan istilah teknis, disertai tombol Coba Lagi. Kode teknis boleh ditampilkan kecil di bawahnya untuk keperluan pelaporan |
| Data basi | Setelah aksi yang mengubah data, daftar yang terkait **MUST** dimuat ulang. Contoh: setelah menyetujui satu tugas, kotak masuk dimuat ulang dan ringkasannya ikut berubah |
| Pengiriman ganda | Tombol aksi **MUST** dinonaktifkan selama permintaan berjalan. Untuk aksi yang tidak idempoten — mengajukan cuti, menjalankan serah terima — ini wajib, bukan pilihan |
| Kehilangan hak akses di tengah sesi | Response `403` ditampilkan sebagai "Anda tidak punya hak akses untuk tindakan ini (kode 403)", dan tombolnya disembunyikan pada pemuatan berikutnya |
| Sesi berakhir | Response `401` mengarahkan ke halaman masuk, dengan isian yang belum tersimpan diperingatkan lebih dulu |

### 7.1 Contoh nyata penanganan pengiriman ganda

> Seorang petugas payroll menekan tombol **Jalankan Serah Terima** dua kali karena halamannya
> terasa lambat. Serah terima di backend memang idempoten — menjalankannya dua kali tidak
> menghasilkan dua snapshot `[EXISTING]`. Namun frontend tetap **wajib** menonaktifkan tombolnya,
> karena dua permintaan yang berjalan bersamaan akan menampilkan dua pesan hasil yang saling
> menimpa, dan petugas tidak tahu mana yang benar.

---

## 8. Kewenangan UI dan ruang `DEV_DISCRETION`

| Keputusan | Wewenang | Dasar |
| --- | --- | --- |
| Layar apa yang harus ada | Dokumen ini | Diturunkan dari kemampuan backend dan keputusan yang dikunci |
| Route setiap layar | Dokumen ini | `HRD-DEC-007` untuk layanan mandiri; pola `src/app/hr/**` untuk administrasi |
| Layar mana yang mendapat butir menu | Dokumen ini | Keterjangkauan, bukan rupa |
| Sumber data setiap bagian layar | Dokumen ini | Kontrak API |
| Butir hak akses yang menjaga setiap tombol | `contracts/permission-audit-matrix.md` | Keamanan |
| Bunyi kalimat kosong dan gagal | Dokumen ini dan `contracts/validation-matrix.md` | Konsistensi bahasa |
| Nama butir menu | `OPEN` — pemilik produk | `HRD-FE-04`: label baru harus disetujui pemilik produk |
| Urutan butir menu dan pengelompokan visualnya | `DEV_DISCRETION` | `HRD-FE-03` |
| Ikon | `DEV_DISCRETION` | `HRD-FE-03` |
| Tab, modal, atau drawer | `DEV_DISCRETION` | `HRD-FE-03` — tidak ada brief yang mengunci ini |
| Warna, jarak, tipografi | `DEV_DISCRETION` dengan syarat mengikuti design token yang ada | `HRD-FE-03`; dilarang membuat design system baru |
| Component library | `DEV_DISCRETION` dengan syarat memakai base component yang sudah ada | `HRD-FE-03` |
| Bentuk kerangka pemuatan | `DEV_DISCRETION` | — |

**Nama butir menu berstatus `OPEN`.** Nama yang dipakai pada bagian 3 adalah **usulan kerja**
agar pohon menu dapat dibaca, bukan keputusan. `HRD-FE-04` menyatakan label menu baru harus
disetujui pemilik produk. Nama-nama itu boleh diganti tanpa mengubah dokumen ini, selama
`pathname` dan layar yang ditunjuk tidak berubah.

---

## 8.1 Keamanan data gaji di sisi layar — `HRD-DEC-038` dan `HRD-DEC-040`

Seluruhnya **kontrak sasaran**, belum diimplementasikan.

### 8.1.1 Alur konfirmasi kata sandi sebelum gaji ditampilkan

```text
sesi terautentikasi
  → pegawai membuka layar Gaji / Slip Gaji
  → layar meminta konfirmasi kata sandi
  → backend memverifikasi lewat Identity canonical
  → sesi gaji sensitif terbit, berlaku 5 menit
  → data gaji dapat ditampilkan
```

Setelah lima menit, layar **MUST** meminta konfirmasi kata sandi lagi sebelum menampilkan data
gaji berikutnya. Layar **MUST NOT** memperpanjang sesi itu sendiri.

### 8.1.2 Larangan penyimpanan yang mengikat frontend

| Yang dilarang | Berlaku pada |
| --- | --- |
| Menyimpan **kata sandi** | `localStorage`; `sessionStorage`; state yang dipersistkan; persistensi Redux; log; analytics |
| Menyimpan **response gaji sensitif** | Redux yang dipersistkan; `localStorage`; cache peramban berumur panjang; payload analytics |

**Kata sandi dikirim untuk diverifikasi, lalu dilupakan.** Ia **MUST NOT** disimpan di mana pun,
termasuk di dalam state komponen yang bertahan setelah layar berpindah.

### 8.1.3 Nominal gaji tidak dikirim, bukan disembunyikan

Layar **MUST NOT** menerima nominal gaji lalu menyembunyikannya. Nilai yang tetap dikirim tetap
melintas jaringan dan tetap terbaca lewat alat pengembang peramban. Penyembunyian di layar **bukan**
kendali keamanan.

### 8.1.4 Unduhan slip gaji

Tautan unduh **MUST NOT** menunjuk URL statis yang dapat ditebak. Ia memanggil endpoint
terautentikasi yang memeriksa otentikasi, kepemilikan, dan sesi gaji sensitif yang masih berlaku.

### 8.1.5 Kewenangan UI yang **tidak** bebas

Berbeda dari kebanyakan keputusan rupa pada dokumen ini, empat butir berikut **bukan**
`DEV_DISCRETION`:

| Butir | Alasan |
| --- | --- |
| Adanya langkah konfirmasi kata sandi | Ia gerbang keamanan, bukan pilihan pengalaman pengguna |
| Larangan menyimpan kata sandi dan response gaji | Sama |
| Ketiadaan nominal pada daftar lintas pegawai | Sama |
| Unduhan lewat endpoint terautentikasi | Sama |

Yang tetap `DEV_DISCRETION`: bentuk dialog konfirmasi, susunan layar gaji, penempatan tombol
unduh, dan cara memberi tahu pengguna bahwa sesi sensitifnya berakhir.

---

## 9. Ketergantungan test

`HRD-TF-007` mencatat bahwa frontend hanya memiliki empat berkas test di seluruh repository, dan
tidak satu pun untuk HR. Kebijakan test frontend project ini: runner `node:test` untuk unit test
dan Playwright untuk e2e; menulis test baru bersifat **opsional**.

Karena opsional, dokumen ini tidak mewajibkannya. Yang diwajibkan adalah **verifikasi interaktif
manual** untuk setiap layar, dengan bukti yang dicatat pada laporan task:

| Yang wajib diverifikasi manual per layar | Alasan |
| --- | --- |
| Keempat keadaan — memuat, kosong, gagal, berisi | Ketiga keadaan pertama yang paling sering lupa dibuat, dan justru paling sering ditemui saat modul baru dipakai |
| Tombol yang tidak berhak benar-benar **tidak muncul** | Tombol yang muncul lalu ditolak `403` adalah cacat yang sudah terbaca sejak desain |
| Butir menu benar-benar membuka halaman yang dimaksud | Ini persis cacat `HRD-TF-005` yang sedang diperbaiki |
| Pengiriman ganda tidak menghasilkan data ganda | — |

---

## 10. Traceability

| Keputusan frontend | Sumber |
| --- | --- |
| Route layanan mandiri wajib `src/app/self-services/human-resource/**` kebab-case | `HRD-DEC-007` |
| Enam halaman daftar lintas-pegawai dibuat, menu tidak dihapus | `HRD-DEC-012` |
| Satu kotak masuk terpadu lintas jenis transaksi | `HRD-DEC-011` |
| Kotak masuk hanya menyeragamkan pengalaman pengguna | `HRD-DEC-018` |
| Route canonical kebab-case, alias lama tetap hidup | `HRD-DEC-016` |
| Bentuk tampilan didelegasikan dengan syarat mengikuti master data terdekat | `HRD-FE-03` |
| Label menu baru menunggu pemilik produk | `HRD-FE-04` |
| Layar koreksi kehadiran atas nama pegawai | `HRD-DEC-028` |
| Layar roster dan penjadwalan operasional | `HRD-DEC-026` |
| Batas waktu dan eskalasi terlihat di kotak masuk | `HRD-DEC-030` |
| Izin pulang cepat tidak dibuatkan layar pada pass ini | `HRD-DEC-029` + `HRD-Q-47` |
| Kolom nominal gaji tidak ditampilkan pada daftar lintas-pegawai | `HRD-DEC-033` — **`SECURITY_APPROVED`** |
| Perubahan penempatan dan remunerasi wajib disetujui pihak yang berbeda | `HRD-DEC-031` |
| Peran fungsional tidak disamakan dengan peran Identity | `HRD-DEC-032` |
| Empat transaksi penempatan dan remunerasi punya layar dan jejak audit terpisah | `HRD-DEC-036` |
| Konfirmasi kata sandi sebelum data gaji ditampilkan | `HRD-DEC-038` |
| Larangan persistensi kata sandi dan response gaji sensitif | `HRD-DEC-040` |
| Calon penyesuaian gaji ditampilkan sebagai usulan, bukan perubahan gaji | `HRD-DEC-043` |
| Payroll Officer tidak melihat nominal gaji | `HRD-DEC-044` |
| Kemampuan yang dirancang berasal dari `HRD-CAP-01` s.d. `HRD-CAP-27` | `01-existing-capability-map.md` |

---

## 11. Yang sengaja tidak dikerjakan pada dokumen ini

1. Tidak ada satu baris source frontend yang diubah.
2. Tidak ada layar untuk `S-C1`, `S-C6`, `S-D1` s.d. `S-D5`.
3. Tidak ada layar payroll di luar dua serah terima yang endpoint-nya memang sudah ada.
4. Tidak ada keputusan warna, ikon, urutan menu, atau component library.
5. Tidak ada nama butir menu yang dinyatakan final.
6. Tidak ada dokumen yang ditandai `approved`.

---

## 12. Slice `S-D1` — Perencanaan Tenaga Kerja (revision `3`, 3 November 2026)

| Field | Value |
| --- | --- |
| Status | `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-03, bersama bagian 16 arsitektur backend, kontrak `v7`, dan `04-prd-to-mvp.md` bagian 21 |
| Frontend SHA | `66b7e5600e1707e4153ca07981ddef054856c8c9` (branch `HamzahV2`) |
| Kontrak yang dikonsumsi | `contracts/api-contract.md` bagian 13 — seluruh endpoint **Rencana (belum tersedia)** |
| Keadaan hari ini | Tidak ada satu pun route, view, service, maupun butir menu perencanaan tenaga (`HRD-CAP-65`) |
| Pola layar | Mengikuti halaman HR terdekat: daftar transaksi pola A (bagian 4.1), dan layar dokumen bertahap seperti SK (bagian 4.2a) |

### 12.1 Kebutuhan layar

| ID | Layar | Route | Jalan masuk | Status |
| --- | --- | --- | --- | --- |
| `FE-HRD-97` | Pemantauan Gap Tenaga | `/hr/workforce-planning/staffing-gaps` | Butir menu | `MISSING / NEW` |
| `FE-HRD-98` | Daftar Standar Kebutuhan Tenaga | `/hr/workforce-planning/staffing-standards` | Butir menu | `MISSING / NEW` |
| `FE-HRD-99` | Penyusun dan Detail Standar | `/hr/workforce-planning/staffing-standards/[id]` | Layar anak dari `FE-HRD-98` | `MISSING / NEW` |
| `FE-HRD-100` | Angka Beban | `/hr/workforce-planning/staffing-workloads` | Butir menu | `MISSING / NEW` |
| `FE-HRD-101` | Formasi Jabatan | `/hr/workforce-planning/position-formations` | Butir menu | `MISSING / NEW` |
| `FE-HRD-102` | Batch Formasi Awal | `/hr/workforce-planning/initial-formation-batches` | Butir menu | `MISSING / NEW` |
| `FE-HRD-103` | Detail Batch Formasi Awal | `/hr/workforce-planning/initial-formation-batches/[id]` | Layar anak dari `FE-HRD-102` | `MISSING / NEW` |
| `FE-HRD-104` | Daftar Rencana Tahunan dan Addendum | `/hr/workforce-planning/manpower-plans` | Butir menu | `MISSING / NEW` |
| `FE-HRD-105` | Penyusun dan Detail Rencana | `/hr/workforce-planning/manpower-plans/[id]` | Layar anak dari `FE-HRD-104` | `MISSING / NEW` |
| `FE-HRD-106` | Daftar Permintaan Headcount | `/hr/workforce-planning/headcount-requests` | Butir menu | `MISSING / NEW` |
| `FE-HRD-107` | Form dan Detail Permintaan | `/hr/workforce-planning/headcount-requests/[id]` | Layar anak dari `FE-HRD-106`; juga dari tombol "Ajukan permintaan" pada `FE-HRD-97` | `MISSING / NEW` |
| `FE-HRD-108` | Antrean Rekrutmen | `/hr/workforce-planning/recruitment-queue` | Butir menu | `MISSING / NEW` |
| `FE-HRD-109` | Pengaturan Perencanaan | `/hr/workforce-planning/settings` | Butir menu | `MISSING / NEW` |

Persetujuan `T9` s.d. `T13` **tidak** punya layar baru. Tugasnya muncul di Kotak Masuk Persetujuan
`FE-HRD-30` dan detailnya di `FE-HRD-31`; layar detail dokumen (`FE-HRD-99`, `FE-HRD-103`,
`FE-HRD-105`, `FE-HRD-107`) dibuka dari tautan tugas itu.

### 12.2 Peta butir menu

```text
Sumber Daya Manusia                         <- tingkat 0, sudah ada, anaknya di field `subMenu`
└── Perencanaan Tenaga Kerja                <- grup tingkat 1, BARU, anaknya di field `subItems`
    ├── Pemantauan Gap Tenaga               -> /hr/workforce-planning/staffing-gaps
    ├── Permintaan Headcount                -> /hr/workforce-planning/headcount-requests
    ├── Antrean Rekrutmen                   -> /hr/workforce-planning/recruitment-queue
    ├── Rencana Tahunan                     -> /hr/workforce-planning/manpower-plans
    ├── Formasi Jabatan                     -> /hr/workforce-planning/position-formations
    ├── Standar Kebutuhan Tenaga            -> /hr/workforce-planning/staffing-standards
    ├── Angka Beban                         -> /hr/workforce-planning/staffing-workloads
    ├── Batch Formasi Awal                  -> /hr/workforce-planning/initial-formation-batches
    └── Pengaturan Perencanaan              -> /hr/workforce-planning/settings
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Perencanaan Tenaga Kerja | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Pemantauan Gap Tenaga | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/staffing-gaps` | `FE-HRD-97` | `WorkforceStaffingGap : Read` | Baru |
| Permintaan Headcount | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/headcount-requests` | `FE-HRD-106` | `WorkforceHeadcountRequest : Read` | Baru |
| Antrean Rekrutmen | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/recruitment-queue` | `FE-HRD-108` | `WorkforceRecruitmentQueue : Read` | Baru |
| Rencana Tahunan | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/manpower-plans` | `FE-HRD-104` | `WorkforceManpowerPlan : Read` | Baru |
| Formasi Jabatan | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/position-formations` | `FE-HRD-101` | `WorkforcePositionFormation : Read` | Baru |
| Standar Kebutuhan Tenaga | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/staffing-standards` | `FE-HRD-98` | `WorkforceStaffingStandard : Read` | Baru |
| Angka Beban | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/staffing-workloads` | `FE-HRD-100` | `WorkforceStaffingWorkload : Read` | Baru |
| Batch Formasi Awal | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/initial-formation-batches` | `FE-HRD-102` | `WorkforceInitialFormationBatch : Read` | Baru |
| Pengaturan Perencanaan | 2 | Perencanaan Tenaga Kerja | `/hr/workforce-planning/settings` | `FE-HRD-109` | `WorkforcePlanningSetting : Read` | Baru |

**Butir menu lama yang bertabrakan makna (`HRD-TF-036`).** Butir "Kebutuhan Tenaga Kerja" pada grup
Master Data › Pegawai & Workforce (`key: corporateWorkforceRequirement`, `menu-items.jsx:251`) membuka
master **persyaratan berkas**, bukan perencanaan tenaga. Yang mengikat: kedua butir **MUST NOT**
memakai nama bermakna "kebutuhan tenaga". Usulan kerja untuk butir lama: **"Persyaratan Berkas Tenaga
Kerja"**; `pathname`-nya tidak berubah. Nama akhir milik pemilik produk (`HRD-FE-04`).

Seluruh nama butir di atas adalah **usulan kerja** — status `OPEN`, `HRD-FE-04`. Urutan dan ikon
`DEV_DISCRETION`. Pendaftaran grup ini **MUST** menjadi acceptance criteria task layar pertama
`S-D1` yang dikerjakan.

### 12.3 Skema fitur per layar

#### 12.3.1 `FE-HRD-97` — Pemantauan Gap Tenaga

```text
+- Pemantauan Gap Tenaga ------------------------------------------ FE-HRD-97 -+
| [Rumah sakit v] [Unit v] [Tanggal acuan: 01-01-2027]   [Lihat potret bulanan]|
+-------------------------------------------------------------------------------+
| Unit | Jabatan | Formasi | Terisi | Akan kosong | Cuti panjang | Belum      |
|      |         |         |        |             |              | kompeten   |
|      |         |         |        |             |              |            |
| Gap permanen | Gap sementara | Gap kompetensi | Rangkap | Alih daya | [Ajukan]|
+-------------------------------------------------------------------------------+
| sel kosong data -> "data belum tersedia" (miring), BUKAN angka 0             |
| memuat -> kerangka baris                                                      |
| kosong -> "Belum ada formasi untuk saringan ini. Isi formasi awal lebih dulu."|
| gagal  -> "Data gap gagal dimuat."                            [Coba lagi]     |
+-------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Saringan | Rumah sakit, unit, tanggal acuan | Master yang sudah ada | `WorkforceStaffingGap : Read` | — |
| Tabel gap | Lapisan dan tiga gap per unit × jabatan | `GET .../staffing-gaps` | `WorkforceStaffingGap : Read` | Nilai `null` tampil "data belum tersedia"; baris yang belum lengkap diberi penanda "data belum lengkap" |
| Tombol Ajukan | Membuka `FE-HRD-107` berisi unit dan jabatan baris itu | — | `WorkforceHeadcountRequest : Create` | Disembunyikan bila tidak berhak |
| Potret bulanan | Daftar potret | `GET .../staffing-gaps/snapshots` | `WorkforceStaffingGap : Read` | Bulan tanpa potret tampil "potret tidak tersedia"; potret susulan berlabel "direkonstruksi" |

#### 12.3.2 `FE-HRD-99` — Penyusun dan Detail Standar

```text
+- Standar STD-HRD-000014 - ICU, versi 3 ---------------------- FE-HRD-99 -+
| Draft   berlaku mulai 01-12-2026   menggantikan versi 2                  |
| [Simpan] [Ajukan] [Batalkan]                                             |
+--------------------------------------------------------------------------+
| Baris jabatan                                                            |
| Jabatan | Metode | Isian metode | Faktor cadangan | Pembulatan | Alih daya |
|         |        |              | (wajib dipilih) | (wajib)    | dihitung? |
+--------------------------------------------------------------------------+
| Skill mix per shift                                                      |
| Shift | Kompetensi | Minimal orang | Wajib?                              |
+--------------------------------------------------------------------------+
| Rekomendasi periode [Jan 2027 v]                                         |
| Perawat Pelaksana: 7 ÷ 1 × 1 = 7 per shift; 7 × 3 = 21; 21 × 1,5 = 31,5 → 32|
+--------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Nomor, unit, versi, status, tanggal berlaku | `GET .../staffing-standards/{id}` | `WorkforceStaffingStandard : Read` | Gagal → seluruh layar diganti pesan dan tombol coba lagi |
| Tombol | Simpan, Ajukan, Batalkan | — | `: Update`, `: Submit`, `: Cancel` | Hanya tampil pada `Draft` dan `Returned`; versi `Active` hanya baca |
| Baris jabatan | Isian per metode; kolom yang tidak dipakai metode itu disembunyikan | Sama | `: Update` | Isian kurang → pesan per baris dari `VR-WFP-03`, `VR-WFP-04` |
| Metode | Tiga pilihan; Analisis Beban Kerja **tidak** muncul sebagai pilihan | `GET .../staffing-standards/filters/metadata` | — | — |
| Rekomendasi | Angka dan rinciannya | `GET .../staffing-standards/{id}/recommendation` | `: Read` | Angka beban belum diisi → "angka beban belum diisi" beserta tautan ke `FE-HRD-100` |
| Riwayat persetujuan | Langkah dan penyetuju | Detail instance workflow yang sudah ada | `ApprovalInbox : Read` | — |

#### 12.3.3 `FE-HRD-105` — Penyusun dan Detail Rencana

```text
+- Rencana RMP-HRD-000003 - Tahun 2027 (Addendum 01) --------- FE-HRD-105 -+
| Submitted - menunggu Finance     tanggal acuan 01-01-2027                 |
| [Simpan] [Hitung ulang] [Ajukan] [Batalkan]   [Koreksi biaya]             |
+---------------------------------------------------------------------------+
| Unit | Jabatan | Grade | Formasi kini | Rekomendasi | Usulan | Perubahan |
|      |         |       |              | (rincian)   |        | Alasan    |
| Berlaku mulai | Estimasi biaya (estimasi) | Biaya perubahan           |
+---------------------------------------------------------------------------+
| Total estimasi: Rp …   Koreksi Finance: Rp … ("ditambah tunjangan …")    |
| Potret gap saat diajukan  |  angka sekarang                              |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Tombol penyusun | Simpan, Hitung ulang, Ajukan, Batalkan | — | `WorkforceManpowerPlan : Update`, `: Submit`, `: Cancel` | Hanya pada `Draft`/`Returned`; rencana `Approved` hanya baca dan menampilkan tombol "Buat addendum" (`: Create`) |
| Koreksi biaya | Isian angka dan alasan | `POST .../finance-adjustment` | `WorkforcePlanningCost : Adjust` | Hanya tampil bagi penyetuju langkah Finance yang sedang aktif |
| Kolom biaya | Estimasi, berlabel "estimasi", dengan rincian "2 × gaji tengah grade III × 9 bulan" | `GET .../manpower-plans/{id}` | `WorkforcePlanningCost : Read` | Tidak berhak → "Biaya tidak ditampilkan untuk peran Anda."; tarif tidak ada → "tarif belum tersedia" |
| Potret | Angka saat diajukan berdampingan dengan angka sekarang | `GET .../manpower-plans/{id}/gap-snapshots` dan `GET .../staffing-gaps` | `WorkforceStaffingGap : Read` | — |

#### 12.3.4 `FE-HRD-107` — Form dan Detail Permintaan Headcount

```text
+- Permintaan HC-2027-014 - ICU, Perawat Pelaksana ------------ FE-HRD-107 -+
| ReadyForRecruitment   di luar formasi   1 dari 2 kursi terisi   [lewat batas]|
| [Ajukan] [Tarik kembali] [Ubah rencana pemenuhan] [Batalkan kursi]         |
+---------------------------------------------------------------------------+
| Sisa kursi saat ini: 0,5 (formasi 30 - terisi 28,5 - permintaan lain 1)  |
+---------------------------------------------------------------------------+
| Kursi | Porsi | Sumber | Status | Digantikan | Dipenuhi oleh | [Tautkan]  |
+---------------------------------------------------------------------------+
| Potret saat diajukan 3,5 | sekarang 2,5      | Estimasi biaya (estimasi) |
| Riwayat: perubahan rencana pemenuhan, pembatalan, persetujuan            |
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Nomor, unit, jabatan, jenis, status, penanda melebihi formasi dan lewat batas | `GET .../headcount-requests/{id}` | `WorkforceHeadcountRequest : Read` | — |
| Sisa kursi | Angka langsung beserta rinciannya | `GET .../headcount-requests/remaining-seats` | `: Read` | Formasi belum ada → "Unit ini belum punya formasi; permintaan diproses sebagai di luar formasi." |
| Tombol | Ajukan, Tarik kembali, Ubah rencana pemenuhan, Batalkan kursi | — | `: Submit`, `: Withdraw`, `: ChangeFulfilmentPlan`, `: RequestCancellation` | Disembunyikan bila tidak berhak atau statusnya tidak mengizinkan |
| Kursi | Daftar kursi; tombol Tautkan dan Lepas per kursi | Sama | `: LinkFulfilment` | Kursi `Frozen` diberi keterangan "dibekukan sesudah addendum" |
| Biaya | Estimasi dan koreksi Finance | Sama | `WorkforcePlanningCost : Read` | Tidak berhak → kalimat VR-WFP-37 |

#### 12.3.5 Layar daftar dan layar sederhana

`FE-HRD-98`, `FE-HRD-100`, `FE-HRD-101`, `FE-HRD-102`, `FE-HRD-104`, `FE-HRD-106`, dan `FE-HRD-108`
mengikuti **Pola A** bagian 4.1. Yang khas per layar:

| Layar | Kolom inti | Bunyi keadaan kosong | Yang khas |
| --- | --- | --- | --- |
| `FE-HRD-98` | Unit, versi, status, berlaku mulai, berlaku sampai | "Belum ada standar untuk saringan ini." | Tombol "Buat versi baru" (`WorkforceStaffingStandard : Create`) |
| `FE-HRD-100` | Unit, jabatan, periode, angka, sumber | "Belum ada angka beban untuk periode ini." | Isian langsung di layar ini (`: Create`, `: Update`) |
| `FE-HRD-101` | Unit, jabatan, formasi, berlaku mulai, dokumen sumber | "Belum ada formasi. Isi lewat Batch Formasi Awal." | **Tanpa** tombol tambah, ubah, atau hapus. Klik baris membuka riwayat versi |
| `FE-HRD-102` | Nomor batch, dokumen sumber, status, jumlah baris valid/tidak valid | "Belum ada batch formasi awal." | Tombol "Ajukan" pada detail `FE-HRD-103` nonaktif selama ada baris tidak valid atau scan belum diunggah |
| `FE-HRD-104` | Nomor, jenis, tahun, status, total estimasi | "Belum ada rencana untuk tahun ini." | Kolom total biaya hanya bagi pemegang `WorkforcePlanningCost : Read` |
| `FE-HRD-106` | Nomor, unit, jabatan, jenis, status, kursi terisi/total | "Belum ada permintaan pada saringan ini." | Saringan "lewat batas" dan "melebihi formasi" |
| `FE-HRD-108` | Nomor permintaan, unit, jabatan, kursi, tanggal dibutuhkan | "Tidak ada kursi yang menunggu direkrut." | **Tanpa** kolom biaya |
| `FE-HRD-109` | Dua isian angka dan alasan | — | Simpan hanya `WorkforcePlanningSetting : Update` |

### 12.4 Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` bagian 11.2.

| Peran | Layar yang terlihat | Aksi |
| --- | --- | --- |
| HR Admin | Seluruh layar kecuali aksi khusus HR Manager | Menyusun standar, batch, rencana; menautkan pemenuhan |
| HR Manager | Seluruh layar | Ditambah mengubah rencana pemenuhan dan pengaturan; menyetujui lewat kotak masuk |
| Kepala Unit | Gap, permintaan, standar, angka beban, formasi, rencana — **unitnya sendiri** | Mengajukan permintaan dan standar; mengisi angka beban |
| Finance / Anggaran | Rencana, permintaan, gap | Koreksi biaya pada langkah Finance |
| Direktur | Standar, rencana, permintaan, gap | Mengesahkan lewat kotak masuk |
| Recruiter | Antrean Rekrutmen | Menautkan pemenuhan |
| Auditor | Seluruh layar baca | Tidak ada |

### 12.5 Penanganan keadaan yang khas `S-D1`

Bagian 7 berlaku seluruhnya. Tambahannya:

| Keadaan | Perlakuan yang mengikat |
| --- | --- |
| Data belum tersedia | Nilai `null` dari backend **MUST** tampil "data belum tersedia", **MUST NOT** diubah menjadi 0 di sisi layar (`UNKNOWN_IS_NOT_ZERO`) |
| Angka berubah sejak diajukan | Layar detail dokumen yang sedang disetujui menampilkan angka potret dan angka sekarang berdampingan (`SUBMITTED_VS_CURRENT`) |
| Dua petugas menyunting draf yang sama | `409` → "Dokumen ini sudah diubah orang lain. Muat ulang sebelum menyimpan." Isian yang belum tersimpan tidak dibuang diam-diam |
| Pengiriman ganda | Tombol Ajukan, Tautkan, dan Batalkan kursi dinonaktifkan selama permintaan berjalan |
| Sesudah aksi | Mengajukan permintaan memuat ulang gap dan sisa kursi; menautkan kursi memuat ulang antrean Recruiter |
| Biaya | Angka biaya **MUST** selalu berlabel "estimasi" dan tidak disimpan di penyimpanan sisi klien |

### 12.6 Kewenangan UI `S-D1`

Bagian 8 berlaku seluruhnya. Yang khas:

| Keputusan | Wewenang |
| --- | --- |
| Tiga belas layar, route, dan jalan masuknya | Dokumen ini |
| Formasi tanpa tombol ubah | Mengikat — `HRD-DEC-062` |
| Nilai `null` sebagai "data belum tersedia" | Mengikat — `HRD-DEC-061` |
| Penyusun standar dan rencana sebagai satu layar berbagian atau berlangkah | `DEV_DISCRETION` |
| Nama grup dan butir menu, termasuk pengganti nama "Kebutuhan Tenaga Kerja" | `OPEN` — pemilik produk, `HRD-FE-04` |

### 12.7 Ketergantungan test

| Uji | Bergantung pada |
| --- | --- |
| Unit test service frontend `S-D1` dengan pola `tests/unit/workforce-core-cross-employee.test.mjs` | Kontrak `api-contract.md` bagian 13 |
| Uji tampilan gap dengan data belum lengkap | Database dev dengan pengaturan perencanaan kosong |
| Uji alur persetujuan | Matriks persetujuan, jabatan direktur, dan definisi `T9` s.d. `T13` terisi |


---

## 13. Slice `S-D2` — Rekrutmen dan Hiring (revision `4`, 3 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — mengikuti hasil wawancara `HRD-DEC-073` s.d. `HRD-DEC-080` |
| Frontend SHA | `cfa82fbb3` (branch `HamzahV2`) |
| Kontrak yang dikonsumsi | `contracts/api-contract.md` bagian 14 — seluruh endpoint Rencana `[Tags("RecruitmentManagement")]` |
| Keadaan hari ini | Tidak ada satu pun route, view, service, maupun butir menu rekrutmen |
| Pola layar | Mengikuti standar field base component Quilvian (`@/components/features/base-features/base-form-control`, `BaseButton`, `StatusBadge`, `InformationAlert`) |

### 13.1 Kebutuhan layar

| ID | Layar | Route | Jalan Masuk | Status |
| --- | --- | --- | --- | --- |
| `FE-HRD-110` | Antrean & Permintaan Requisition | `/hr/recruitment/requisitions` | Butir menu | `MISSING / NEW` |
| `FE-HRD-111` | Detail & Draf Requisition | `/hr/recruitment/requisitions/[id]` | Layar anak dari `FE-HRD-110` | `MISSING / NEW` |
| `FE-HRD-112` | Pengelolaan Lowongan Kerja (*Job Vacancies*) | `/hr/recruitment/vacancies` | Butir menu | `MISSING / NEW` |
| `FE-HRD-113` | Detail Lowongan & Pelamar Masuk | `/hr/recruitment/vacancies/[id]` | Layar anak dari `FE-HRD-112` | `MISSING / NEW` |
| `FE-HRD-114` | Direktori Data Pelamar (*Candidate Directory*) | `/hr/recruitment/candidates` | Butir menu | `MISSING / NEW` |
| `FE-HRD-115` | Detail Pelamar & Penapisan Berkas (*Screening*) | `/hr/recruitment/candidates/[id]` | Layar anak dari `FE-HRD-114` | `MISSING / NEW` |
| `FE-HRD-116` | Formulir Wawancara HR | `/hr/recruitment/evaluations/hr/[id]` | Layar anak dari `FE-HRD-115` | `MISSING / NEW` |
| `FE-HRD-117` | Formulir Uji Klinis / Asesor Unit | `/hr/recruitment/evaluations/clinical/[id]` | Layar anak dari `FE-HRD-115` | `MISSING / NEW` |
| `FE-HRD-118` | Daftar Penawaran Kerja (*Job Offers*) | `/hr/recruitment/offers` | Butir menu | `MISSING / NEW` |
| `FE-HRD-119` | Detail Penawaran & Persetujuan Bertingkat | `/hr/recruitment/offers/[id]` | Layar anak dari `FE-HRD-118` | `MISSING / NEW` |
| `FE-HRD-120` | Pemantauan Kelaikan MCU Pra-Kerja | `/hr/recruitment/mcu-clearance` | Butir menu | `MISSING / NEW` |
| `FE-HRD-121` | Bank Kandidat Cadangan (*Talent Pool*) | `/hr/recruitment/talent-pool` | Butir menu | `MISSING / NEW` |
| `FE-HRD-122` | Serah Terima ke Onboarding (*Hiring Handoff*) | `/hr/recruitment/handoffs` | Butir menu | `MISSING / NEW` |

### 13.2 Peta butir menu

```text
Sumber Daya Manusia                         <- tingkat 0, sudah ada
  ├── Pengadaan & Rekrutmen                <- tingkat 1, grup baru
  │     ├── Permintaan Requisition         <- /hr/recruitment/requisitions (Recruitment : Read)
  │     ├── Lowongan Kerja                 <- /hr/recruitment/vacancies (Recruitment : Read)
  │     ├── Bank Pelamar                   <- /hr/recruitment/candidates (Candidate : Read)
  │     ├── Surat Penawaran (Offer)        <- /hr/recruitment/offers (JobOffer : Read)
  │     ├── Pemantauan MCU Pra-Kerja       <- /hr/recruitment/mcu-clearance (Recruitment : Read)
  │     ├── Talent Pool Cadangan           <- /hr/recruitment/talent-pool (Recruitment : Read)
  │     └── Serah Terima Onboarding        <- /hr/recruitment/handoffs (Recruitment : Read)
```

### 13.3 Skema fitur per layar

#### A. Layar Penapisan Pelamar Klinis (`FE-HRD-115`)
- Menampilkan dokumen STR dengan status verifikasi.
- Jika tanggal masa berlaku STR < tanggal hari ini, badge merah **"STR KADALUWARSA / GUGUR ADMINISTRASI"** terkunci otomatis (`HRD-DEC-076`).
- Tombol "Jadwalkan Wawancara" dinonaktifkan permanen jika STR tidak valid.

#### B. Layar Uji Klinis Unit (`FE-HRD-117`)
- Menampilkan rubrik skor keterampilan klinis dan SOP darurat.
- Rekomendasi Unit: `RECOMMENDED`, `CONSIDERED`, `NOT_RECOMMENDED`.
- Jika dipilih `NOT_RECOMMENDED`, muncul peringatan konfirmasi: *"Penolakan klinis bersifat hak veto mutlak demi patient safety dan tidak dapat dianulir tim HR"* (`HRD-DEC-077`).

#### C. Layar Penawaran Kerja & Persetujuan Bertingkat (`FE-HRD-119`)
- Menampilkan input gaji pokok & tunjangan, otomatis membaca batas `MinimumSalary` s.d. `MaximumSalary` dari `SalaryGrade`.
- Jika penawaran > `MaximumSalary`, muncul badge kuning *"Penawaran Melebihi Plafon Grade — Wajib Persetujuan Direktur Keuangan dan Direktur Utama"* (`HRD-DEC-079`).

#### D. Layar Pemantauan MCU Pra-Kerja (`FE-HRD-120`)
- **Masking Privasi Medis**: Staf HR hanya melihat status `FIT`, `FIT_WITH_RESTRICTIONS`, `TEMPORARILY_UNFIT`, `PERMANENTLY_UNFIT` beserta catatan pembatasan tugas kerja (`HRD-DEC-078`).
- Rincian hasil tes lab, rontgen, dan diagnosa klinis disembunyikan total (*medical confidentiality*).

#### E. Layar Talent Pool & Promosi Cepat (`FE-HRD-121`)
- Daftar kandidat cadangan berperingkat aktif 6 bulan (`HRD-DEC-080`).
- Menyediakan tombol *"Promosikan ke Penawaran Baru"* yang langsung aktif jika kursi requisition sebelumnya berstatus batal/mundur (*Offer Rejected / No-Show*).

### 13.4 Penanganan kesalahan dan state khusus

| Kasus | Penanganan di Layar |
| --- | --- |
| Pembukaan lowongan tanpa requisition disetujui | Tombol "Buat Lowongan" meminta pemilihan requisition aktif; form menolak submit jika kuota kursi = 0 |
| Pelamar klinis tanpa STR aktif | Status otomatis `Screening Rejected`, sistem menampilkan pesan alasan penolakan regulasi STR |
| Asesor klinis menolak kandidat | Tombol "Buat Job Offer" otomatis hilang/terkunci pada kandidat tersebut (`HRD-DEC-077`) |
| Diagnosa MCU sensitif | Komponen UI tidak pernah memuat field `MedicalDiagnosisDetail`; hanya menampilkan `FitnessStatus` dan `Restrictions` |
| Kandidat menolak tawaran | Muncul modal opsi: "Tutup Kursi" atau "Pilih Kandidat Cadangan dari Talent Pool" |

### 13.5 Kewenangan UI S-D2

- Seluruh form menggunakan base-form-control tanpa styling inline / warna custom.
- Penanda hak veto klinis dan validasi plafon grade gaji bersifat mengikat arsitektur.
- Layout tabel, tab filter status (Draft/Screening/Interview/Offer), dan pagination bersifat `DEV_DISCRETION`.


---

## 14. Slice `S-C4-ONB` — Onboarding dan Kontrak Kerja, sub-modul 05 (revision `5`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan" |
| Frontend SHA | `44990b3d3` (branch `HamzahV2`) |
| Kontrak yang dikonsumsi | `contracts/api-contract.md` bagian 15 — seluruh endpoint **Rencana (belum tersedia)** |
| Keadaan hari ini | Tidak ada satu pun route, view, service, maupun butir menu onboarding, kontrak kerja, masa percobaan, atau pendampingan (`HRD-TF-039`). Tab riwayat kontrak di editor workforce mengirim masa percobaan tanpa memeriksa jenis kontrak (`HRD-TF-043`) |
| Pola layar | Mengikuti halaman HR terdekat: daftar transaksi pola A (bagian 4.1), layar dokumen bertahap seperti SK (bagian 4.2a), dan layar batch seperti batch penempatan awal |
| Prinsip | `HRD-DEC-081`: satu tombol keputusan per peran, data yang sudah ada tidak diketik ulang, dan tombol yang tidak berhak disembunyikan |

### 14.1 Kebutuhan layar

| ID | Layar | Route | Jalan masuk | Status |
| --- | --- | --- | --- | --- |
| `FE-HRD-123` | Perlu Tindakan | `/hr/lifecycle/worklist` | Butir menu | `MISSING / NEW` |
| `FE-HRD-124` | Daftar Kasus Onboarding | `/hr/lifecycle/onboarding-cases` | Butir menu | `MISSING / NEW` |
| `FE-HRD-125` | Detail Kasus Onboarding | `/hr/lifecycle/onboarding-cases/[id]` | Layar anak dari `FE-HRD-124`; juga dari `FE-HRD-123` dan dari layar serah terima `FE-HRD-122` | `MISSING / NEW` |
| `FE-HRD-126` | Daftar Kontrak Kerja | `/hr/lifecycle/employment-contracts` | Butir menu | `MISSING / NEW` |
| `FE-HRD-127` | Detail Kontrak Kerja | `/hr/lifecycle/employment-contracts/[id]` | Layar anak dari `FE-HRD-126`; juga dari `FE-HRD-123`, `FE-HRD-125`, dan tab riwayat kontrak editor workforce | `MISSING / NEW` |
| `FE-HRD-128` | Daftar Masa Percobaan | `/hr/lifecycle/probations` | Butir menu | `MISSING / NEW` |
| `FE-HRD-129` | Detail Masa Percobaan | `/hr/lifecycle/probations/[id]` | Layar anak dari `FE-HRD-128`; juga dari `FE-HRD-123` dan tugas kotak masuk `T15` | `MISSING / NEW` |
| `FE-HRD-130` | Daftar Pendampingan Klinis | `/hr/lifecycle/clinical-preceptorships` | Butir menu | `MISSING / NEW` |
| `FE-HRD-131` | Detail Pendampingan Klinis | `/hr/lifecycle/clinical-preceptorships/[id]` | Layar anak dari `FE-HRD-130`; juga dari `FE-HRD-125` | `MISSING / NEW` |
| `FE-HRD-132` | Daftar Template Onboarding | `/hr/lifecycle/onboarding-templates` | Butir menu | `MISSING / NEW` |
| `FE-HRD-133` | Penyusun Template Onboarding | `/hr/lifecycle/onboarding-templates/[id]` | Layar anak dari `FE-HRD-132` | `MISSING / NEW` |
| `FE-HRD-134` | Daftar Batch Data Awal Kontrak | `/hr/lifecycle/initial-contract-batches` | Butir menu | `MISSING / NEW` |
| `FE-HRD-135` | Detail Batch Data Awal Kontrak | `/hr/lifecycle/initial-contract-batches/[id]` | Layar anak dari `FE-HRD-134`; juga dari tugas kotak masuk `T18` | `MISSING / NEW` |
| `FE-HRD-136` | Permintaan Akses Pegawai Baru | `/hr/lifecycle/onboarding-access-requests` | Butir menu | `MISSING / NEW` |
| `FE-HRD-137` | Pengaturan Onboarding dan Kontrak | `/hr/lifecycle/settings` | Butir menu | `MISSING / NEW` |

Persetujuan `T15` s.d. `T18` **tidak** punya layar baru. Tugasnya muncul di Kotak Masuk Persetujuan
`FE-HRD-30` dan detailnya di `FE-HRD-31`; dokumen dibuka dari tautan tugas itu (`FE-HRD-127`,
`FE-HRD-129`, `FE-HRD-135`).

### 14.2 Peta butir menu

```text
Sumber Daya Manusia                         <- tingkat 0, sudah ada, anaknya di field `subMenu`
└── Onboarding & Kontrak                    <- grup tingkat 1, BARU, anaknya di field `subItems`
    ├── Perlu Tindakan                      -> /hr/lifecycle/worklist
    ├── Kasus Onboarding                    -> /hr/lifecycle/onboarding-cases
    ├── Kontrak Kerja                       -> /hr/lifecycle/employment-contracts
    ├── Masa Percobaan                      -> /hr/lifecycle/probations
    ├── Pendampingan Klinis                 -> /hr/lifecycle/clinical-preceptorships
    ├── Permintaan Akses Pegawai Baru       -> /hr/lifecycle/onboarding-access-requests
    ├── Template Onboarding                 -> /hr/lifecycle/onboarding-templates
    ├── Batch Data Awal Kontrak             -> /hr/lifecycle/initial-contract-batches
    └── Pengaturan                          -> /hr/lifecycle/settings
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- | --- |
| Onboarding & Kontrak | 1 | Sumber Daya Manusia | — | — | — | Baru |
| Perlu Tindakan | 2 | Onboarding & Kontrak | `/hr/lifecycle/worklist` | `FE-HRD-123` | `WorkforceLifecycleWorklist : Read` | Baru |
| Kasus Onboarding | 2 | Onboarding & Kontrak | `/hr/lifecycle/onboarding-cases` | `FE-HRD-124` | `WorkforceOnboardingCase : Read` | Baru |
| Kontrak Kerja | 2 | Onboarding & Kontrak | `/hr/lifecycle/employment-contracts` | `FE-HRD-126` | `WorkforceEmploymentContract : Read` | Baru |
| Masa Percobaan | 2 | Onboarding & Kontrak | `/hr/lifecycle/probations` | `FE-HRD-128` | `WorkforceProbation : Read` | Baru |
| Pendampingan Klinis | 2 | Onboarding & Kontrak | `/hr/lifecycle/clinical-preceptorships` | `FE-HRD-130` | `WorkforcePreceptorship : Read` | Baru |
| Permintaan Akses Pegawai Baru | 2 | Onboarding & Kontrak | `/hr/lifecycle/onboarding-access-requests` | `FE-HRD-136` | `WorkforceOnboardingAccessRequest : Read` | Baru |
| Template Onboarding | 2 | Onboarding & Kontrak | `/hr/lifecycle/onboarding-templates` | `FE-HRD-132` | `WorkforceOnboardingTemplate : Read` | Baru |
| Batch Data Awal Kontrak | 2 | Onboarding & Kontrak | `/hr/lifecycle/initial-contract-batches` | `FE-HRD-134` | `WorkforceInitialContractBatch : Read` | Baru |
| Pengaturan | 2 | Onboarding & Kontrak | `/hr/lifecycle/settings` | `FE-HRD-137` | `WorkforceLifecycleSetting : Read` | Baru |

Butir "Exit Clearance Karyawan" yang sudah ada (`menu-items.jsx:687`) **tidak** dipindah; ia milik 20.
Seluruh nama butir di atas **usulan kerja** — status `OPEN`, `HRD-FE-04`. Urutan dan ikon
`DEV_DISCRETION`. Pendaftaran grup ini **MUST** menjadi acceptance criteria task layar pertama
`S-C4-ONB` yang dikerjakan. Penanda jumlah pada "Perlu Tindakan" membaca
`GET .../worklist/counts`.

### 14.3 Skema fitur per layar

#### 14.3.1 `FE-HRD-123` — Perlu Tindakan

```text
+- Perlu Tindakan --------------------------------------------- FE-HRD-123 -+
| [Semua] [Kontrak akan berakhir 4] [Eskalasi 1] [Tanpa keputusan 1]       |
| [Masa percobaan 2] [Kontrak belum ditandatangani 1] [Terlambat mulai 0]  |
| [Scan data awal 12] [Kontrak belum tercatat 3]          [Unit v]        |
+--------------------------------------------------------------------------+
| Pegawai | Unit | Kategori | Tenggat | Keterangan             | [Buka]   |
| Budi    | Farmasi | Kontrak akan berakhir | 31-03-2027 | PKWT, 3 th  |  |
+--------------------------------------------------------------------------+
| memuat -> kerangka baris                                                  |
| kosong -> "Tidak ada yang perlu ditindaklanjuti saat ini."               |
| gagal  -> "Daftar gagal dimuat."                            [Coba lagi]  |
+--------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Penyaring kategori | Kategori beserta jumlah; kategori yang tidak berlaku bagi peran pemanggil tidak tampil | `GET .../worklist/counts` | `WorkforceLifecycleWorklist : Read` | Jumlah tidak tampil bila gagal; daftar tetap dapat dimuat |
| Tabel | Butir sesuai kategori, cakupan unit, dan peran | `GET .../worklist` | `WorkforceLifecycleWorklist : Read` | Kosong dan gagal seperti gambar |
| Tombol Buka | Membuka dokumen menurut `targetType`: kasus `FE-HRD-125`, kontrak `FE-HRD-127`, masa percobaan `FE-HRD-129` | — | Butir `Read` layar tujuan | Disembunyikan bila tidak berhak |

#### 14.3.2 `FE-HRD-124`, `FE-HRD-126`, `FE-HRD-128`, `FE-HRD-130`, `FE-HRD-134`, `FE-HRD-136` — layar daftar

Keenamnya berbagi bentuk daftar pola A (bagian 4.1): saringan di atas, tabel, paginasi, dan keempat
keadaan. Yang berbeda hanya kolom dan saringannya.

| Layar | Saringan | Kolom | Sumber data | Bunyi kosong |
| --- | --- | --- | --- | --- |
| `FE-HRD-124` | Status, unit, rentang tanggal mulai, koordinator, cari nama | Nomor kasus, pegawai, unit, jabatan, tanggal mulai, kemajuan "7 dari 10 butir", status | `GET .../onboarding-cases` | "Belum ada kasus onboarding pada saringan ini." |
| `FE-HRD-126` | Status, jenis kontrak, unit, berakhir dalam N hari, data awal tanpa scan | Pegawai, nomor kontrak, jenis, mulai, berakhir, status, tanda "scan belum ada" | `GET .../employment-contracts` | "Belum ada kontrak pada saringan ini." |
| `FE-HRD-128` | Status, unit, perlu dinilai | Pegawai, unit, mulai, berakhir, sisa hari, status | `GET .../probations` | "Tidak ada masa percobaan pada saringan ini." |
| `FE-HRD-130` | Status, unit, preceptor | Pegawai, unit, preceptor, didampingi sejak, status | `GET .../clinical-preceptorships` | "Tidak ada pendampingan pada saringan ini." |
| `FE-HRD-134` | Status | Nomor batch, jumlah baris, baris gagal, status, diajukan oleh | `GET .../initial-contract-batches` | "Belum ada batch data awal." + tombol Buat batch (`WorkforceInitialContractBatch : Create`) |
| `FE-HRD-136` | Jenis permintaan, status | Pegawai, unit, jenis, status, pesan gagal, diminta pada | `GET .../onboarding-access-requests` | "Tidak ada permintaan akses yang menunggu." |

Gagal memuat pada keenamnya: "Data gagal dimuat." beserta tombol Coba lagi.

#### 14.3.3 `FE-HRD-125` — Detail Kasus Onboarding

```text
+- Kasus ONB-2027-000015 - Rina, Perawat Pelaksana ICU ---------- FE-HRD-125 -+
| Onboarding berjalan   mulai 01-02-2027   koordinator Dodi   7 dari 10 butir |
| ! Kewenangan klinis (SPK/RKK) belum tercatat                                |
| [Catat Hadir Hari Pertama] [Nyatakan Siap Kerja] [Batal Bergabung] [Alihkan]|
+-------------------------------------------------------------------------------+
| Checklist                                                                     |
| Lapis | Butir                        | Oleh        | Tenggat | Status | Aksi  |
| Umum  | Hadir hari pertama           | HRD         | 01-02   | Selesai|       |
| Umum  | Kontrak sudah ditandatangani | Sistem      | 01-02   | Belum  | -     |
| Umum  | Orientasi umum (bukti)       | HRD         | 02-02   | Belum  |[Centang]|
| Umum  | Atasan langsung              | Kepala Unit | 03-02   | Belum  |[Pilih]|
| Klinis| BTCLS berlaku                | Sistem      | 01-02   | Selesai|       |
| Klinis| Preceptor                    | Kepala Unit | 03-02   | Belum  |[Pilih]|
| ICU   | Orientasi ventilator         | Kepala Unit | 03-02   | Belum  |[Centang]|
+-------------------------------------------------------------------------------+
| Kontrak pertama: PKWT 01-02-2027 s.d. 31-01-2028  Draf  [Buka kontrak]        |
| Akun: dibuat, belum aktif   |   Akses kerja: belum diminta                    |
+-------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Nomor, nama, jabatan, unit, status, tanggal mulai, koordinator, kemajuan, peringatan | `GET .../onboarding-cases/{id}` | `WorkforceOnboardingCase : Read` | Gagal → seluruh layar diganti pesan dan tombol coba lagi; di luar cakupan → "Data tidak ditemukan." |
| Tombol Catat Hadir Hari Pertama | Form tanggal hadir | `POST .../first-day-attendance` | `WorkforceOnboardingCase : CompleteItem` | Tampil hanya bila `availableActions` memuatnya |
| Tombol Nyatakan Siap Kerja | Konfirmasi dengan ringkasan butir | `POST .../declare-ready` | `WorkforceOnboardingCase : DeclareReady` | Nonaktif dengan keterangan "Masih ada {n} butir wajib" bila belum lengkap |
| Tombol Batal Bergabung | Form alasan dan catatan | `POST .../cancel-join` | `WorkforceOnboardingCase : CancelJoin` | Tidak tampil sesudah hadir hari pertama |
| Tombol Alihkan | Pilih petugas HRD | `PUT .../coordinator` | `WorkforceOnboardingCase : Reassign` | — |
| Checklist | Butir per lapis, urut tenggat | `items[]` pada detail | `WorkforceOnboardingCase : Read` | Kasus tanpa Template Umum menampilkan "Template Umum RS belum ada; hanya butir sistem yang tampil." |
| Aksi butir HRD | Centang, unggah bukti, batal centang | `POST .../items/{itemId}/complete`, `/undo` | `WorkforceOnboardingCase : CompleteItem` | Butir otomatis tidak punya tombol |
| Aksi butir unit | Centang, unggah bukti, pilih orang, batal centang | `POST .../unit-items/{itemId}/complete`, `/undo` | `WorkforceOnboardingCase : CompleteUnitItem` | Pilihan orang hanya pegawai aktif di unit itu |
| Kontrak pertama | Jenis, tanggal, status, tautan | `firstContract` pada detail | `WorkforceEmploymentContract : Read` | — |
| Akun dan akses | Status kedua permintaan, pesan gagal | `accessRequests[]` pada detail | `WorkforceOnboardingCase : Read` | Pesan gagal tampil apa adanya, misalnya "Email pegawai belum diisi" |

#### 14.3.4 `FE-HRD-127` — Detail Kontrak Kerja

```text
+- Kontrak 045/PKWT/2026 - Budi, Petugas Farmasi ---------------- FE-HRD-127 -+
| PKWT   01-04-2026 s.d. 31-03-2027   Berjalan   total PKWT 3 tahun           |
| [Cetak] [Tandai Sudah Ditandatangani] [Ajukan Keputusan] [Ajukan Koreksi]   |
+-------------------------------------------------------------------------------+
| Rantai kontrak        | 2024 PKWT -> 2025 PKWT -> 2026 PKWT (berjalan)        |
| Masa percobaan        | Tidak berlaku untuk PKWT                              |
| Keputusan             | belum ada                                             |
| Koreksi               | belum ada                                             |
| Scan                  | 045-PKWT-2026.pdf  [Unduh]                            |
+-------------------------------------------------------------------------------+
| Ajukan Keputusan: ( ) Perpanjang [12] bulan -> berakhir 31-03-2028            |
|                   ( ) Angkat jadi Pegawai Tetap                               |
|                   ( ) Tidak Diperpanjang  alasan [...........]                |
|                   pratinjau: total PKWT menjadi 4 tahun          [Ajukan]     |
+-------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Nomor, pegawai, jenis, tanggal, status, total PKWT | `GET .../employment-contracts/{id}` | `WorkforceEmploymentContract : Read` | Gagal → pesan dan coba lagi |
| Tombol Cetak | Draf siap cetak; gaji tampil hanya bila berhak | `GET .../{id}/print-data` | `WorkforceEmploymentContract : Read` (+ `WfpSalaryAssignment : ViewAmount` untuk nominal) | Tanpa hak nominal: baris gaji berbunyi "disembunyikan" |
| Tombol Tandai Sudah Ditandatangani | Unggah scan dan tanggal tanda tangan | `POST .../{id}/mark-signed` | `WorkforceEmploymentContract : MarkSigned` | Tampil hanya untuk kontrak `Draft` |
| Tombol Ajukan Keputusan | Form tiga pilihan dengan pratinjau 5 tahun | `GET .../{id}/fixed-term-check`, `POST .../{id}/decisions` | `WorkforceContractDecision : Submit` | Pilihan Perpanjang dinonaktifkan dengan keterangan bila melewati 5 tahun |
| Tombol Ajukan Koreksi | Form nilai baru, alasan, scan perbaikan | `POST .../{id}/corrections` | `WorkforceEmploymentContract : RequestCorrection` | — |
| Keputusan dan koreksi | Daftar beserta statusnya; tombol Catat Pemberitahuan untuk Tidak Diperpanjang yang disetujui | Detail kontrak; `POST .../decisions/{id}/notification` | `WorkforceContractDecision : RecordNotification` | Kosong → "belum ada" |
| Scan | Unduh; unggah susulan untuk data awal | `GET`/`POST .../{id}/document` | `WorkforceEmploymentContract : Read` / `: UploadDocument` | Tanpa scan → "Scan belum diunggah" |

#### 14.3.5 `FE-HRD-129` — Detail Masa Percobaan

```text
+- Masa Percobaan - Andi, Apoteker --------------------------- FE-HRD-129 -+
| Berjalan   01-02-2027 s.d. 30-04-2027   sisa 14 hari                     |
+--------------------------------------------------------------------------+
| Hasil: ( ) Lulus   ( ) Tidak Lulus                                       |
| Alasan [..............................................]                  |
| Lampiran penilaian [Pilih berkas]  (wajib bila Tidak Lulus)    [Ajukan]  |
+--------------------------------------------------------------------------+
| Riwayat penilaian: PRB-2027-000003  Lulus  Dikembalikan  "lengkapi..."   |
+--------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Pegawai, status, tanggal, sisa hari | `GET .../probations/{id}` | `WorkforceProbation : Read` | Lulus otomatis tampil "Lulus otomatis pada {tanggal}" |
| Form hasil | Hasil, alasan, lampiran | `POST .../probations/{id}/assessments` | `WorkforceProbation : Submit` | Form tidak tampil sesudah tanggal akhir atau bila ada penilaian terbuka |
| Riwayat | Seluruh penilaian dan catatan HRD | Detail | `WorkforceProbation : Read` | Kosong → "Belum ada penilaian." |

#### 14.3.6 `FE-HRD-131` — Detail Pendampingan Klinis

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Pegawai, unit, status, didampingi sejak, preceptor berjalan | `GET .../clinical-preceptorships/{id}` | `WorkforcePreceptorship : Read` | Gagal → pesan dan coba lagi |
| Tombol Ganti Preceptor | Pilih preceptor dan alasan | `PUT .../{id}/preceptor` | `WorkforcePreceptorship : Manage` | Pilihan hanya tenaga klinis aktif di unit itu |
| Tombol Siap Mandiri | Tanggal, catatan, lampiran wajib | `POST .../{id}/declare-independent` | `WorkforcePreceptorship : Manage` | Tampil hanya saat Didampingi |
| Riwayat preceptor | Nama, dari, sampai, alasan | Detail | `WorkforcePreceptorship : Read` | — |

#### 14.3.7 `FE-HRD-133` — Penyusun Template Onboarding

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kepala | Kode, nama, lapis (Umum / Tenaga Klinis / Unit), unit bila lapis Unit, target hari, aktif | `GET .../onboarding-templates/{id}` | `WorkforceOnboardingTemplate : Read` | — |
| Daftar butir | Kode, nama, kelompok, penanggung jawab, jenis butir, sumber otomatis dan jenis sertifikat, wajib, tenggat hari | Detail | `WorkforceOnboardingTemplate : Update` | Butir sistem tampil terkunci dengan label "Butir sistem" |
| Simpan | Simpan template | `POST`/`PUT .../onboarding-templates` | `: Create` / `: Update` | Pesan dari `VR-ONB-20` s.d. `VR-ONB-25` |

#### 14.3.8 `FE-HRD-135` — Detail Batch Data Awal Kontrak

Mengikuti layar batch penempatan awal (`HRD-DEC-054`): unduh templat, unggah, tabel baris dengan
kolom hasil pemeriksaan per baris, tombol Ajukan yang nonaktif selama masih ada baris gagal.
Sumber: `GET .../initial-contract-batches/template`, `POST .../validate`, `POST`/`PUT`, `POST .../submit`.
Butir hak akses: `WorkforceInitialContractBatch : Read`, `: Create`, `: Update`, `: Submit`, `: Cancel`.
Kosong → "Belum ada baris. Unduh templat untuk mulai."

#### 14.3.9 `FE-HRD-137` — Pengaturan

Empat isian angka dengan satuan hari dan keterangan bawaannya (60, 14, 14, 3). Sumber
`GET`/`PUT .../lifecycle-settings`; simpan hanya untuk `WorkforceLifecycleSetting : Update`.
Pemegang `: Read` saja melihat isian baca saja.

### 14.4 Perubahan pada layar yang sudah ada

| Layar | Perubahan | Dasar |
| --- | --- | --- |
| Tab riwayat kontrak pada editor workforce (`src/components/features/workforce/workforce-resource-editor.jsx`) | Menjadi **baca saja**. Tombol tambah, ubah, ubah status, jadikan berjalan, dan hapus dihapus; isian `probationEndDate` dihapus dari form. Satu tautan "Kelola di Kontrak Kerja" membuka `FE-HRD-127` atau `FE-HRD-126` | `VR-ONB-32`, `HRD-DEC-085` butir 2, `HRD-TF-043` |
| Label "Probation" sebagai jenis kepegawaian dan status pegawai (`workforce-profile-dashboard.jsx:3998-4025`) | Tidak dipakai untuk menandai masa percobaan; profil menampilkan "Masa percobaan s.d. {tanggal}" dari data masa percobaan bila ada | `HRD-DEC-085` butir 2 |
| Form master pegawai (`employee-grouped-form.config.jsx`) | Isian jenis kepegawaian, status kepegawaian, tanggal masuk, tanggal akhir kontrak, dan tanggal akhir masa percobaan menjadi baca saja untuk pegawai yang sudah punya kontrak, dengan keterangan "Mengikuti kontrak kerja" | `VR-ONB-33` |
| Form dan layar persetujuan tawaran `S-D2` | Isian jenis kontrak (pilihan) dan lama kontrak (bulan) beserta tanggal berakhir hasil hitung; tampil juga di surat tawaran | `HRD-DEC-095`, `VR-ONB-50`, `VR-ONB-51` |
| Layar Serah Terima `FE-HRD-122` (`S-D2`) | Sesudah serah terima berhasil, menampilkan tautan ke kasus onboarding yang terbentuk | `HRD-DEC-083` |
| Kotak Masuk `FE-HRD-30` / `FE-HRD-31` | Empat jenis tugas baru `T15` s.d. `T18` dengan tautan ke dokumennya | `HRD-DEC-081` |
| Penjadwalan 06 | Menampilkan penanda "Didampingi — {preceptor}" dan pesan penolakan `VR-ONB-46`, `VR-ONB-47` | `HRD-DEC-087`, `HRD-DEC-089` — dikerjakan pada desain 06 |

### 14.5 Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` bagian 13.2, tidak dikarang ulang.

| Peran | Layar yang terlihat | Tombol yang terlihat |
| --- | --- | --- |
| HR Admin | Seluruh layar bagian 14.1 kecuali tombol ubah di `FE-HRD-137` | Catat hadir, centang butir HRD, alihkan, siap kerja, batal bergabung, cetak, tandai ditandatangani, ajukan koreksi, catat pemberitahuan, unggah scan susulan, kelola template, kelola batch, ulang akun |
| HR Manager | Sama dengan HR Admin | Ditambah simpan pengaturan; kategori eskalasi di Perlu Tindakan |
| Kepala Unit | `FE-HRD-123` s.d. `FE-HRD-131` untuk unitnya | Centang butir unit, pilih atasan langsung dan preceptor, ajukan keputusan kontrak, ajukan hasil masa percobaan, ganti preceptor, siap mandiri |
| Atasan langsung | `FE-HRD-123`, `FE-HRD-127`, `FE-HRD-129` untuk bawahannya | Ajukan keputusan kontrak, ajukan hasil masa percobaan |
| Administrator / IT | `FE-HRD-136` | Tandai akses selesai |
| Auditor | Seluruh layar, baca saja | Tidak ada tombol tulis |

### 14.6 Data dan status yang dikonsumsi

| Layar | Endpoint yang dibaca | Status yang ditampilkan |
| --- | --- | --- |
| `FE-HRD-124`, `FE-HRD-125` | `.../onboarding-cases` | Onboarding berjalan, Selesai, Dibatalkan; butir Belum, Selesai |
| `FE-HRD-126`, `FE-HRD-127` | `.../employment-contracts` | Draf, Berjalan, Selesai, Berakhir, Berakhir tanpa keputusan, Dibatalkan |
| `FE-HRD-128`, `FE-HRD-129` | `.../probations` | Berjalan, Menunggu keputusan HRD, Lulus, Lulus otomatis, Tidak Lulus |
| `FE-HRD-130`, `FE-HRD-131` | `.../clinical-preceptorships` | Preceptor ditetapkan, Didampingi, Mandiri, Dibatalkan |
| `FE-HRD-134`, `FE-HRD-135` | `.../initial-contract-batches` | Draf, Diajukan, Dikembalikan, Ditolak, Diterapkan, Dibatalkan |
| `FE-HRD-136` | `.../onboarding-access-requests` | Diminta, Selesai, Gagal, Dibatalkan |

Pemetaan nilai enum ke label Indonesia ditempatkan di satu berkas konstanta layar, bukan di tiap
komponen. Label adalah `DEV_DISCRETION` selama maknanya sama dengan kolom di atas.

### 14.7 Penanganan keadaan

| Keadaan | Penanganan |
| --- | --- |
| Memuat | Kerangka baris pada daftar; kerangka bagian pada detail |
| Kosong | Kalimat bagian 14.3 |
| Gagal | "Data gagal dimuat." dengan tombol Coba lagi; detail di luar cakupan → "Data tidak ditemukan." |
| Data basi | Request tulis membawa `rowVersion`; jawaban `409` menampilkan "Data ini sudah diubah orang lain. Muat ulang lalu ulangi." dan memuat ulang |
| Pengiriman ganda | Tombol aksi nonaktif selama request berjalan; serah terima dan siap kerja aman diulang di backend (`VR-ONB-01`) |
| Penolakan aturan | Pesan `422` dari `contracts/validation-matrix.md` bagian 14 ditampilkan apa adanya di dekat tombol |
| Berkas | Pemeriksaan jenis dan ukuran di sisi layar sebelum unggah, sama dengan `VR-ONB-09`; backend tetap memeriksa |

### 14.8 Kewenangan UI dan ruang `DEV_DISCRETION`

| Hal | Keputusan | Sumber |
| --- | --- | --- |
| Tombol yang tidak berhak | **Disembunyikan**, bukan ditampilkan lalu ditolak | Template frontend 2.5 |
| Tombol aksi kasus | Mengikuti `availableActions` dari backend | `AGENTS.md` — backend otoritatif atas aksi |
| Nominal gaji di cetak kontrak | Hanya bagi pemegang `ViewAmount`; tidak disimpan di Redux maupun penyimpanan peramban | `HRD-DEC-033`, `HRD-DEC-040` |
| Nama butir menu dan label status | Usulan kerja | `HRD-FE-04` `OPEN` |
| Tab, modal, drawer, warna, ikon, urutan | `DEV_DISCRETION` | Template frontend 2.1 |
| Form keputusan kontrak dan hasil masa percobaan sebagai modal atau panel | `DEV_DISCRETION` | — |

### 14.9 Ketergantungan test

Test frontend memakai kerangka uji repository frontend. Yang wajib dibuktikan per layar: keempat
keadaan, tombol tersembunyi bagi peran yang tidak berhak, penanganan `409`, dan pesan `422`.
Uji layar bergantung pada endpoint bagian 15 kontrak API; sebelum backend siap, layar diuji dengan
data tiruan yang mengikuti bentuk response kontrak.

### 14.10 Traceability

| Keputusan | Layar |
| --- | --- |
| `HRD-DEC-081` | Seluruh layar: satu tombol keputusan per peran; persetujuan lewat kotak masuk |
| `HRD-DEC-083` | `FE-HRD-124`, `FE-HRD-125`, perubahan `FE-HRD-122` |
| `HRD-DEC-084`, `HRD-DEC-095` | `FE-HRD-125`, `FE-HRD-127`, perubahan layar tawaran `S-D2` |
| `HRD-DEC-085` | `FE-HRD-128`, `FE-HRD-129`, perubahan tab riwayat kontrak dan label "Probation" |
| `HRD-DEC-086` | `FE-HRD-125`, `FE-HRD-132`, `FE-HRD-133` |
| `HRD-DEC-087` | `FE-HRD-130`, `FE-HRD-131` |
| `HRD-DEC-088` | `FE-HRD-125`, `FE-HRD-136` |
| `HRD-DEC-089` | `FE-HRD-123`, `FE-HRD-126`, `FE-HRD-127` |
| `HRD-DEC-090` | `FE-HRD-134`, `FE-HRD-135` |


---

## 15. Slice `S-B4` — Penjadwalan, Roster, dan On-Call, sub-modul 06 (revision `6`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — Siap diajukan untuk approval pengguna |
| Frontend SHA saat bagian ini ditulis | `44990b3d3` (branch `HamzahV2`) |
| Backend SHA | `38e892b0` (branch `MHamzah`) |
| `input_revision` — decision log | `00-interview-decisions.md` revision `20`, bagian 36 (`HRD-DEC-097` s.d. `HRD-DEC-103`, `HRD-ASM-05` s.d. `HRD-ASM-14`) |
| Perubahan halaman | 1 halaman diperbarui (`/hr/scheduling/roster`), 2 halaman baru (`/hr/scheduling/on-call`, `/hr/scheduling/settings`), 3 modal/drawer komponen interaktif baru |

### 15.1 Kebutuhan layar `S-B4`

| ID Layar | Nama layar | Route URL | Status | Deskripsi |
| --- | --- | --- | --- | --- |
| `FE-HRD-137` | Kisi Roster Bulanan Unit | `/hr/scheduling/roster` | `Diperbarui` | Layar kerja utama Kepala Unit untuk menyusun jadwal shift bulanan, salin periode lalu, terapkan pola shift, menampilkan penanda Didampingi, dan status badge |
| `FE-HRD-138` | Drawer Validasi & Alasan Roster | Komponen Drawer pada `/hr/scheduling/roster` | `Baru` | Menampilkan hasil periksa jadwal: memisahkan error memblokir (`B1`–`B7`) dan peringatan (`W1`–`W7`). Menyediakan dropdown alasan wajib untuk setiap peringatan sebelum tombol Terbitkan aktif |
| `FE-HRD-139` | Modal Ganti Petugas Cepat | Komponen Modal pada `/hr/scheduling/roster` | `Baru` | Memungkinkan Kepala Unit mengganti petugas shift terbit secara langsung, memilih alasan standar, dan otomatis memvalidasi kelayakan pengganti |
| `FE-HRD-140` | Manajemen Siaga (*On-Call Coverage*) | `/hr/scheduling/on-call` | `Baru` | Layar visual penugasan siaga dokter spesialis dan staf penunjang cito per unit dan per tanggal |
| `FE-HRD-141` | Modal Pencatatan Pemanggilan Siaga (*Call-In*) | Komponen Modal pada `/hr/scheduling/on-call` & Layar Unit | `Baru` | Tombol dan form pemanggilan siaga darurat, mencatat waktu panggil, waktu tiba, dan kalkulasi kepatuhan target SLA respon menit |
| `FE-HRD-142` | Pengaturan Kebijakan Penjadwalan HR | `/hr/scheduling/settings` | `Baru` | Layar admin HR untuk menyetel angka jeda istirahat minimal (default 11 jam), batas hari kerja berturut-turut, toleransi koreksi mundur (default 3 hari), dan toggle eskalasi aturan |

### 15.2 Peta butir menu

| Label menu | Level | Induk menu | Route | Layar target | Permission penutup |
| --- | --- | --- | --- | --- | --- |
| Roster Kerja | Level 2 | Penjadwalan & Shift | `/hr/scheduling/roster` | `FE-HRD-137` | `[AccessPermission("HrdScheduling", "Read")]` |
| Penugasan Siaga (On-Call) | Level 2 | Penjadwalan & Shift | `/hr/scheduling/on-call` | `FE-HRD-140` | `[AccessPermission("HrdOnCall", "Read")]` |
| Pengaturan Waktu Kerja | Level 2 | Penjadwalan & Shift | `/hr/scheduling/settings` | `FE-HRD-142` | `[AccessPermission("HrdSchedulingSetting", "Manage")]` |

### 15.3 Skema fitur per layar

#### 15.3.1 `FE-HRD-137` — Kisi Roster Bulanan Unit (`/hr/scheduling/roster`)

```text
+----------------------------------------------------------------------------------------------------+
|  Navigasi: Human Resource > Penjadwalan & Shift > Roster Kerja                                     |
|  [Filter Unit: ICU v] [Periode: Februari 2027 v] [Status: Published] [Total Staf: 18 Perawat]       |
|  [+ Buat Roster] [Salin Periode Lalu] [Terapkan Pola]  [Periksa Jadwal] [Terbitkan Roster]          |
+----------------------------------------------------------------------------------------------------+
| Pegawai            | 01 Kam | 02 Jum | 03 Sab | 04 Min | ... | 14 Sab          | ... | 28 Min        |
+----------------------------------------------------------------------------------------------------+
| Sari (Kepala Unit) | Pagi   | Pagi   | Libur  | Libur  | ... | Pagi            | ... | Libur         |
| Budi (Mandiri)     | Siang  | Siang  | Malam  | Lepas  | ... | [Malam (Diganti)| ... | Pagi          |
| Tono (Mandiri)     | Pagi   | Siang  | Pagi   | Siang  | ... | [Malam (Ganti)] | ... | Siang         |
| Rina [Didampingi]  | Pagi   | Pagi   | Libur  | Libur  | ... | Siang           | ... | Pagi          |
+----------------------------------------------------------------------------------------------------+
| Kuota Shift: Pagi  | 3/3 OK | 3/3 OK | 3/3 OK | 3/3 OK | ... | 3/3 OK          | ... | 3/3 OK        |
| Kuota Shift: Malam | 3/3 OK | 3/3 OK | 2/3 [!] Peringatan: Kekurangan Tenaga | ... | 3/3 OK        |
+----------------------------------------------------------------------------------------------------+
```

- **Tabel Wilayah Layar:**
  - **Header & Filter:** Memilih unit kerja dan bulan kalender. Tombol aksi sesuai status periode.
  - **Grid Jadwal:** Baris berisi pegawai eligible. Kolom tanggal menampilkan kode shift. Klik pada kotak shift terbit memunculkan popover: **Ganti Petugas**, **Koreksi Shift** (jika `today - date <= 3`), atau **Lihat Riwayat Audit**.
  - **Penanda Khusus:** Badge kuning `[Didampingi — Tono]` pada nama perawat orientasi (`HRD-DEC-087`, `HRD-DEC-100`).
  - **Kondisi Terkunci:** Bila periode berstatus `Closed`, seluruh sel tabel terkunci (*read-only*) dengan banner informasi: *"Periode telah ditutup oleh Payroll HRD pada 28/02/2027. Jadwal tidak dapat diubah."*

#### 15.3.2 `FE-HRD-138` — Drawer Validasi & Alasan Roster

- **Wilayah Kesalahan Memblokir (Merah):** Daftar pelanggaran `B1`–`B7`. Selama bagian ini terisi, tombol **Terbitkan** terkunci total (*disabled*). Setiap butir memiliki tombol pintas *"Arahkan ke Kotak Jadwal"*.
- **Wilayah Peringatan (Kuning):** Daftar peringatan `W1`–`W7`. Masing-masing butir memiliki dropdown wajib:
  - Pilihan alasan: `Kekurangan tenaga`, `Permintaan pegawai`, `Volume pasien/BOR rendah`, atau `Lainnya (wajib catatan)`.
  - Tombol **Terbitkan** hanya aktif setelah 100% peringatan memiliki alasan terisi.

#### 15.3.3 `FE-HRD-139` — Modal Ganti Petugas Cepat

- **Field Input:**
  - Pegawai asal (terisi otomatis, *read-only*): Budi.
  - Tanggal & Shift: 14 Februari 2027, Shift Malam (21.00–07.00).
  - Pilih Pegawai Pengganti: dropdown hanya menampilkan pegawai unit yang bebas shift dan lolos validasi `B1`–`B7`.
  - Kategori Alasan: radio button / select (`Sakit Mendadak`, `Izin/Keluarga`, `Lonjakan Pasien`, `Pengurangan Staf`, `Lainnya`).
  - Catatan tambahan (opsional kecuali bila memilih `Lainnya`).
- **Aksi:** Tombol **Konfirmasi Penggantian** langsung menyimpan mutasi, status roster tetap `Published`, dan mengirim notifikasi real-time.

#### 15.3.4 `FE-HRD-140` & `FE-HRD-141` — Penugasan Siaga & Modal Call-In

- **Tampilan Kalender Siaga:** Grid penugasan siaga per profesi (Dokter Spesialis & Staf Cito). Menampilkan status badge: `Scheduled` (Biru), `Confirmed` (Kuning), `Active` (Hijau).
- **Modal Call-In:**
  - Input waktu panggilan darurat (*CallInTime*).
  - Input waktu kehadiran dokter (*ArrivalTime*).
  - Indikator real-time: Target SLA (misal 30 menit) vs Realisasi (25 menit) -> Badge Hijau `[MEMENUHI SLA (25 min)]`.

### 15.4 Aksi per peran

| Peran Pengguna | Hak Akses & Aksi Utama |
| --- | --- |
| **Kepala Unit (Sari)** | Membuat periode draft unitnya, menyusun shift, salin periode lalu, validasi jadwal, memilih alasan peringatan, menerbitkan roster, melakukan penggantian petugas langsung (*override*), dan koreksi mundur maksimal 3 hari |
| **Kepala Bidang (Pak Dodi)** | Melihat (*read-only*) seluruh roster unit di bawah koordinasinya, melihat riwayat peringatan & alasan yang dipilih kepala unit, menerima notifikasi saat roster terbit atau terjadi mutasi shift |
| **Pegawai (Budi, Rina)** | Melihat jadwal pribadi di halaman profil/layanan mandiri, menerima notifikasi saat jadwal terbit atau saat digantikan/ditugaskan shift pengganti |
| **Analis Lab / Perawat Unit Pemanggil** | Membaca daftar dokter siaga aktif, menekan tombol **Panggil Siaga (*Call-In*)**, mencatat waktu kedatangan |
| **Admin HR / Payroll Officer** | Mengelola setelan kebijakan waktu kerja (`FE-HRD-142`), menyusun siaga dokter bersama Komite Medik, dan menutup periode roster (**`Closed`**) saat cut-off payroll |

### 15.5 Penanganan keadaan (*State Handling*)

| Keadaan | Penanganan pada Layar |
| --- | --- |
| **Loading** | Skeleton loader pada kisi kalender jadwal; tombol aksi berstatus loading spinner |
| **Empty** | Layar menyajikan pesan ramah: *"Belum ada jadwal dinas untuk periode ini. Klik 'Buat Roster' atau 'Salin dari Periode Lalu' untuk memulai."* |
| **Error / Validasi Gagal** | Drawer validasi terbuka otomatis menampilkan rincian error pemblokir dengan penyorotan warna merah pada sel tabel terkait |
| **Period Closed** | Banner penguncian mutlak berwarna abu-abu; seluruh kontrol pengeditan dinonaktifkan (*disabled*) |

### 15.6 Kewenangan UI dan ruang `DEV_DISCRETION`

- **Terkunci Blueprint (Wajib):** Peta menu, route URL, alur validasi dua tingkat (blokir vs peringatan ber-alasan), status badge, penanda *Didampingi*, pembatasan koreksi mundur 3 hari, dan modal call-in dengan SLA.
- **`DEV_DISCRETION`:** Pilihan warna badge status, layout density kisi tabel (compact vs comfortable view), animasi transisi drawer, format penanggalan tampilan kalender, dan ikon visual.

### 15.7 Ketergantungan test

- Seluruh komponen form dan grid tabel wajib memiliki `data-testid` terstandarisasi:
  - `data-testid="roster-grid-table"`
  - `data-testid="btn-recheck-roster"`
  - `data-testid="btn-publish-roster"`
  - `data-testid="btn-quick-replacement"`
  - `data-testid="drawer-validation-result"`
  - `data-testid="select-warning-reason"`
  - `data-testid="badge-accompanied-staff"`
  - `data-testid="modal-oncall-callin"`

### 15.8 Traceability keputusan ke frontend

| Keputusan | Komponen & Layar Frontend |
| --- | --- |
| `HRD-DEC-097` | `FE-HRD-137` (Filter unit bergiliran; tidak ada layar permintaan tenaga darurat) |
| `HRD-DEC-098` | `FE-HRD-137` (Tombol Terbitkan langsung oleh Kepala Unit; fitur salin & pola; hak pantau Kepala Bidang) |
| `HRD-DEC-099` | `FE-HRD-138` (Pemisahan error `B1`–`B6` merah vs `W1`–`W5` kuning dengan dropdown alasan) |
| `HRD-DEC-100` | `FE-HRD-137` (Badge `[Didampingi — Preceptor]`), `FE-HRD-138` (Pemeriksaan `B7`, `W6`, `W7`) |
| `HRD-DEC-101` | `FE-HRD-139` (Modal Ganti Petugas langsung, alasan wajib, status `Published` terjaga) |
| `HRD-DEC-102` | `FE-HRD-137` (Batas edit mundur 3 hari; banner dan disable total saat status `Closed`) |
| `HRD-DEC-103` | `FE-HRD-140`, `FE-HRD-141` (Kalender siaga, modal call-in, badge status SLA respon) |


---

## 16. Bagian `S-B1` — Layar Administrasi Kehadiran dan Presensi Staf Rumah Sakit (Sub-modul 07, `last_changed_in` `v11`, 4 November 2026)

Bagian ini mendokumentasikan spesifikasi antarmuka pengguna frontend untuk **Slice `S-B1` — Administrasi Kehadiran dan Presensi Staf Rumah Sakit 24/7**, mencakup 6 layar terpadu (`FE-HRD-144` s.d. `FE-HRD-149`), komponen drawer log biometrik mentah, modal pengajuan koreksi presensi on-behalf, modal izin pulang cepat, modal penugasan dinas luar GPS, serta layar tata kelola penutupan periode.

### 16.1 Peta Butir Menu Navigasi Sidebar Modul Kehadiran

| Kode Layar | Butir Menu Sidebar | Induk Menu | Route URL | Hak Akses Utama | Peran Pengguna |
| --- | --- | --- | --- | --- | --- |
| `FE-HRD-144` | Monitoring Presensi | Presensi & Kehadiran | `/hr/attendance/monitoring` | `[AccessPermission("AttendanceDaily", "View")]` | HR Officer, Kepala Ruangan, Manager |
| `FE-HRD-145` | Koreksi Kehadiran | Presensi & Kehadiran | `/hr/attendance/corrections` | `[AccessPermission("AttendanceCorrection", "View")]` | HR Officer, Kepala Unit, Pegawai |
| `FE-HRD-146` | Izin Pulang Cepat | Presensi & Kehadiran | `/hr/attendance/early-leave` | `[AccessPermission("EarlyLeavePermission", "View")]` | HR Officer, Atasan Langsung, Pegawai |
| `FE-HRD-147` | Dinas Luar & Home Care | Presensi & Kehadiran | `/hr/attendance/official-duty` | `[AccessPermission("OfficialDuty", "View")]` | HR Officer, Koordinator Pelayanan, Pegawai |
| `FE-HRD-148` | Periode & Tutup Buku | Presensi & Kehadiran | `/hr/attendance/periods` | `[AccessPermission("AttendancePeriod", "View")]` | HR Attendance Officer, HR Manager |
| `FE-HRD-149` | Pengaturan Kebijakan Presensi | Presensi & Kehadiran | `/hr/attendance/settings` | `[AccessPermission("AttendanceSetting", "Manage")]` | HR Administrator |

### 16.2 Skema Fitur & Tata Letak Layar

#### 16.2.1 Layar `FE-HRD-144` — Monitoring Presensi Harian Staf Unit (`/hr/attendance/monitoring`)
* **Tujuan:** Menampilkan matriks status kehadiran harian seluruh staf rumah sakit per tanggal dan per unit (rawat inap, IGD, ICU, farmasi, penunjang), mendeteksi otomatis keterlambatan, shift malam lintas hari (`HRD-DEC-104`), absensi tidak lengkap 1 tap (`HRD-DEC-106`), dan status dinas luar (`HRD-DEC-109`).
* **Wilayah Layar:**
  1. *Filter & Pencarian:* Pemilihan tanggal operasional, filter unit/instalasi, filter profesi klinis, dan pencarian nama/NIP staf.
  2. *Kartu Ringkasan Harian:* Total Staf Terjadwal, Hadir Tepat Waktu (`Present`), Terlambat (`Late`), Pulang Cepat (`EarlyLeave`), Lupa Tap (`IncompletePunch`), Mangkir (`Absent`), dan Dinas Luar (`OnDuty`).
  3. *Tabel Presensi Harian:* Kolom Nama Staf, Unit, Shift Roster Resmi, Jam Masuk Roster, Jam Tap Masuk Aktual, Jam Tap Keluar Aktual, Jam Kerja Efektif, Menit Terlambat, Badge Status Kehadiran, dan Kolom Aksi.
  4. *Tombol Aksi per Baris:* Buka Drawer Detail Log Biometrik, dan Aksi Cepat Buat Koreksi On-Behalf (khusus HR Admin).

#### 16.2.2 Layar `FE-HRD-145` — Manajemen & Persetujuan Koreksi Kehadiran (`/hr/attendance/corrections`)
* **Tujuan:** Mengelola daftar permohonan koreksi presensi yang diajukan secara mandiri oleh pegawai maupun diajukan oleh HR atas nama pegawai (*on-behalf* per `HRD-DEC-028`).
* **Wilayah Layar:**
  1. *Tabs Status Koreksi:* Menunggu Persetujuan (`Pending Approval`), Disetujui (`Approved`), Ditolak (`Rejected`), dan Diterapkan (`Applied`).
  2. *Tabel Pengajuan:* Tanggal Pengajuan, Nama Pegawai, Tanggal Shift yang Dikoreksi, Tipe Koreksi (Lupa Tap Masuk / Lupa Tap Pulang / Jam Salah), Jam Usulan Baru, Pemohon (Pegawai vs HR On-Behalf), Alasan Koreksi, Tautan Bukti Surat/Foto, dan Kolom Status.
  3. *Tombol Tindakan:* Tombol **Setujui** (*Approve*) dan **Tolak** (*Reject*) bagi atasan langsung pegawai; Tombol **Terapkan Koreksi** (*Apply*) bagi HR Officer untuk memicu hitung ulang jam kerja harian.
  4. *Modal Kuasa HR On-Behalf:* Form khusus bagi HR Admin untuk mengoreksi absensi pegawai yang terkendala akses ESS dengan input wajib: pilihan pegawai, tanggal shift, jam koreksi, alasan tertulis, dan unggah berkas bukti.

#### 16.2.3 Layar `FE-HRD-146` — Permohonan & Persetujuan Izin Pulang Cepat (`/hr/attendance/early-leave`)
* **Tujuan:** Mengelola izin pulang cepat administratif yang bersifat *non-deductible* (tidak memotong kuota saldo cuti tahunan per `HRD-DEC-107`).
* **Wilayah Layar:**
  1. *Form Pengajuan Izin:* Pemilihan tanggal shift dinas, jam rencana keluar lebih awal (`RequestedEarlyLeaveAt`), kategori alasan mendesak (Keluarga Darurat, Sakit Mendadak di Tempat Kerja, Penugasan Dinas Khusus), dan catatan penjelasan.
  2. *Daftar Persetujuan Kepala Ruangan:* Memverifikasi kelayakan izin darurat dan memastikan ada perawat pengganti di shift tersebut sebelum menyetujui.
  3. *Riwayat Izin Pulang Cepat:* Rekapitulasi frekuensi staf mengambil izin pulang cepat dalam 1 tahun sebagai bahan evaluasi kedisiplinan dan pembinaan pegawai.

#### 16.2.4 Layar `FE-HRD-147` — Surat Tugas Dinas Luar & Home Care (`/hr/attendance/official-duty`)
* **Tujuan:** Mengakomodasi penugasan staf di luar rumah sakit (ambulans rujukan, kurir sampel lab, workshop akreditasi RS, home care per `HRD-DEC-109`).
* **Wilayah Layar:**
  1. *Form Surat Tugas:* Nomor surat tugas, kategori dinas luar, lokasi tujuan, tanggal & jam dinas, serta daftar staf yang ditugaskan.
  2. *Peta & Bukti Kunjungan:* Menampilkan koordinat GPS lokasi pelaksanaan tugas luar dan tangkapan foto bukti kegiatan (bila staf tap melalui mobile ESS).
  3. *Tabel Rekapitulasi:* Memastikan seluruh staf yang bertugas luar terdata otomatis hadir dengan status `OnDuty` dan jam kerjanya diakui penuh untuk perhitungan payroll.

#### 16.2.5 Layar `FE-HRD-148` — Penutupan Periode Kehadiran & Sinkronisasi Payroll (`/hr/attendance/periods`)
* **Tujuan:** Dasbor bulanan bagi HR Attendance Officer dan HR Manager untuk melakukan verifikasi pra-penutupan (*pre-close validation*), mengunci periode presensi (*Period Close*), dan buka kembali darurat (*Controlled Reopen* per `HRD-DEC-108`).
* **Wilayah Layar:**
  1. *Status Periode:* Kode Periode (misal `ATT-2026-11`), Rentang Tanggal (26 Okt s.d. 25 Nov), dan Status Periode (`Open`, `Closed`, `Reopened`).
  2. *Pemeriksaan Pra-Penutupan (Pre-Close Guard):* Widget verifikasi otomatis yang menampilkan:
     * Jumlah koreksi presensi yang masih menggantung (*Pending Approval*) — **Wajib 0 untuk bisa menutup periode**.
     * Jumlah absensi 1 tap (*IncompletePunch*) — Ditampilkan peringatan bahwa saat tombol tutup ditekan, sisa incomplete punch otomatis berubah menjadi `Absent` (Mangkir).
  3. *Tombol Aksi Utama:*
     * Tombol **Tutup Periode Kehadiran**: Mengunci seluruh data presensi menjadi *read-only*.
     * Tombol **Buka Kembali Periode Darurat (Reopen)**: Hanya aktif untuk peran HR Manager; memunculkan modal alasan audit dan peringatan pembatalan (*rollback*) batch payroll jika serah terima sudah berjalan.
     * Tombol **Serah Terima Payroll (Execute Handoff)**: Mengirimkan rekapitulasi kehadiran final ke Sub-Modul 10 (Payroll).

#### 16.2.6 Layar `FE-HRD-149` — Pengaturan Kebijakan Presensi HR (`/hr/attendance/settings`)
* **Tujuan:** Layar konfigurasi parameter waktu kerja presensi rumah sakit per `HRD-DEC-105`.
* **Wilayah Layar:**
  1. *Form Toleransi:* Input toleransi keterlambatan (*Grace Period Minutes*, default 10 menit), toggle aturan hitung penuh keterlambatan bila lewat toleransi.
  2. *Form Batasan Koreksi:* Batas maksimal hari koreksi tanggal mundur (*MaxBackdatedCorrectionDays*, default 3 hari), toggle kewajiban melampirkan berkas bukti pada koreksi on-behalf.
  3. *Tombol Simpan Pengaturan:* Memperbarui konfigurasi terpusat pada backend.

### 16.3 Komponen Modal & Drawer Pendukung

* **`RawAttendanceLogDrawer`:** Menampilkan seluruh riwayat mentah tap biometrik pegawai pada hari tersebut (jam tepat, ID mesin, metode biometrik, status sinkronisasi) untuk pembuktian saat audit internal.
* **`AttendanceCorrectionModal`:** Formulir pengajuan koreksi presensi mandiri dan *on-behalf* dengan penanganan upload lampiran.
* **`ReopenPeriodConfirmModal`:** Modal konfirmasi buka kembali periode yang mewajibkan input alasan tertulis dan menampilkan status serah terima payroll yang akan di-*rollback*.
