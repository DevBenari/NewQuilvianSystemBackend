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


---

## 17. Bagian `S-B2` — Layar Administrasi Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08, `last_changed_in` `v12`, 4 November 2026)

Bagian ini mendokumentasikan spesifikasi antarmuka pengguna frontend untuk **Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit**, mencakup 6 layar terpadu (`FE-HRD-150` s.d. `FE-HRD-155`), visualisasi buku besar mutasi saldo cuti, formulir pengajuan cuti ESS dengan opsi setengah hari (half-day 0.5 hari), kotak masuk approval atasan berindikator *Safe Staffing Coverage Warning*, alur verifikasi surat sakit ber-SIP dokter dengan perlindungan privasi medis staf K3RS, serta modal inisiasi dan persetujuan *Leave Recall* darurat rumah sakit.

### 17.1 Peta Butir Menu Navigasi Sidebar Modul Cuti & Izin

| Kode Layar | Butir Menu Sidebar | Induk Menu | Route URL | Hak Akses Utama | Peran Pengguna |
| --- | --- | --- | --- | --- | --- |
| `FE-HRD-150` | Saldo & Kuota Cuti Saya | Cuti & Perizinan | `/hr/leave/my-balance` | `[AccessPermission("LeaveBalance", "ViewSelf")]` | Seluruh Pegawai RS (ESS) |
| `FE-HRD-151` | Pengajuan Cuti & Izin | Cuti & Perizinan | `/hr/leave/request` | `[AccessPermission("LeaveRequest", "Create")]` | Seluruh Pegawai RS (ESS) |
| `FE-HRD-152` | Persetujuan Cuti & Kuota Staf | Cuti & Perizinan | `/hr/leave/approval-inbox` | `[AccessPermission("LeaveApproval", "View")]` | Kepala Ruangan, Kepala Instalasi, Manajer |
| `FE-HRD-153` | Alokasi Saldo & Carry-Forward | Cuti & Perizinan | `/hr/leave/balances-admin` | `[AccessPermission("LeaveBalance", "Manage")]` | HR Leave Officer, HR Admin |
| `FE-HRD-154` | Pemanggilan Darurat (Leave Recall) | Cuti & Perizinan | `/hr/leave/recalls` | `[AccessPermission("LeaveRecall", "View")]` | Kepala Ruangan, HR Manager, Direktur Pelayanan |
| `FE-HRD-155` | Rekonsiliasi Akhir & Tutup Cuti | Cuti & Perizinan | `/hr/leave/reconciliations` | `[AccessPermission("LeaveReconciliation", "View")]` | HR Leave Officer, HR Payroll Officer, HR Manager |

### 17.2 Skema Fitur & Tata Letak Layar

#### 17.2.1 Layar `FE-HRD-150` — Dasbor Kuota & Saldo Cuti Pegawai ESS (`/hr/leave/my-balance`)
* **Tujuan:** Memberikan transparansi mutlak kepada setiap pegawai mengenai sisa saldo cuti aktif, rincian hak cuti tahunan berjalan (12 hari), saldo bawaan tahun lalu (*carry-forward* max 6 hari beserta countdown kedaluwarsa 30 Juni per `HRD-DEC-110`), serta riwayat mutasi buku besar (*balance ledger*).
* **Wilayah Layar:**
  1. *Kartu Ringkasan Saldo (Metric Cards):*
     * Total Hak Cuti Tahun Berjalan (12 Hari).
     * Saldo Aktif Dapat Dipakai (*Available Balance*).
     * Sisa Kuota Carry-Forward (disertai lencana kuning peringatan: *"Kedaluwarsa 30 Juni"*).
     * Total Hari Cuti Terpakai Tahun Ini.
  2. *Bagan Jejak Pemakaian (Usage Progress Bar):* Menampilkan persentase hak cuti yang telah diambil versus sisa waktu tahun kalender.
  3. *Tabel Riwayat Mutasi Buku Besar (Leave Balance Ledger):* Tanggal Transaksi, Jenis Transaksi (`Entitlement`, `CarryForward`, `Taken`, `CancellationRestore`, `RecallRestore`, `Forfeiture`), Jumlah Hari (+/-), Saldo Sesudah, Nomor Rujukan Dokumen, dan Keterangan.
  4. *Tombol Aksi:* Tombol **Ajukan Cuti Baru** (mengarahkan ke `FE-HRD-151`).

#### 17.2.2 Layar `FE-HRD-151` — Formulir Pengajuan Cuti & Cuti Sakit ESS (`/hr/leave/request`)
* **Tujuan:** Formulir mandiri staf untuk mengajukan cuti tahunan, cuti sakit, izin alasan penting, atau cuti bersama, dengan kalkulasi hari otomatis, opsi setengah hari (0.5 hari per `HRD-ASM-20`), dan gerbang unggah berkas surat dokter ber-SIP (`HRD-DEC-112`).
* **Wilayah Layar:**
  1. *Pilihan Jenis Cuti:* Dropdown master jenis cuti (Tahunan, Sakit, Bersama, Melahirkan, Ibadah, Izin Khusus). Menampilkan kuota tersisa saat jenis cuti dipilih.
  2. *Pemilih Rentang Tanggal & Mode Durasi:* Kalender tanggal mulai dan selesai. Toggle opsi **Cuti Setengah Hari (Half-Day)** dengan pilihan sesi: Pagi atau Siang/Sore (otomatis bernilai 0.5 hari kerja).
  3. *Deteksi Hari Libur & Shift Roster:* Sistem otomatis menampilkan ringkasan hari kerja efektif (tidak menghitung hari libur resmi atau hari lepas jaga staf).
  4. *Alasan & Kontak Darurat:* Input alasan cuti, nomor telepon darurat yang dapat dihubungi saat cuti, dan nama kontak darurat.
  5. *Zona Unggah Surat Keterangan Dokter (Khusus Cuti Sakit):*
     * Jika durasi sakit = 1 hari: opsional unggah surat pernyataan mandiri.
     * Jika durasi sakit $\ge 2$ hari: **wajib unggah berkas Surat Dokter resmi**. Input nama dokter pemeriksa, nomor SIP, dan nama faskes penerbit. Dilengkapi teks penjaminan: *"Diagnosa medis Anda dilindungi privasi K3RS dan tidak ditampilkan ke atasan langsung"* (`HRD-DEC-112`).
  6. *Tombol Submit & Konfirmasi:* Validasi instan saldo mencukupi sebelum submit (`LEV-B1`).

#### 17.2.3 Layar `FE-HRD-152` — Kotak Masuk Persetujuan Cuti Atasan & Safe Staffing Coverage (`/hr/leave/approval-inbox`)
* **Tujuan:** Kotak masuk bagi Kepala Ruangan/Instalasi untuk meninjau permohonan cuti bawahan, dengan fitur unggulan **Banner Peringatan Kuota Pelayanan (*Safe Staffing Coverage Warning*)** per `HRD-DEC-111`.
* **Wilayah Layar:**
  1. *Daftar Permohonan Menunggu:* Nama Pemohon, Jabatan/Unit, Jenis Cuti, Rentang Tanggal, Durasi Hari, Tanggal Submit, dan Indikator Status Kuota Unit.
  2. *Panel Evaluasi Ketersediaan Tenaga (Coverage Warning Banner):*
     * Jika permohonan aman: Ditampilkan lencana hijau: *"Ketersediaan tenaga shift memenuhi standar keselamatan"* (misal 5 dari min 4 perawat bertugas).
     * Jika permohonan kritis: Ditampilkan banner peringatan oranye/merah: *"PERINGATAN KUOTA LAYANAN: Persetujuan cuti ini menyebabkan jumlah perawat dinas pada 12 Nov Shift Pagi tersisa 3 orang (ambang batas minimal: 4 orang)"*.
  3. *Tampilan Detail Berkas Cuti Sakit (Masked Privacy):* Menampilkan durasi anjuran dokter, nama faskes/dokter, status verifikasi berkas, dengan diagnosa medis disamarkan (ikon gembok privasi).
  4. *Tombol Tindakan Atasan:*
     * **Setujui (Approve):** Aktif jika kuota staf aman.
     * **Setujui dengan Mitigasi (Approve with Mitigation):** Aktif saat banner kuota muncul; mewajibkan atasan mencatat rencana penugasan staf pengganti atau koordinasi rotasi sebelum submit.
     * **Minta Revisi Tanggal (Need Revision):** Mengembalikan form ke staf untuk penyesuaian tanggal.
     * **Tolak Permohonan (Reject):** Mewajibkan input alasan penolakan operasional.

#### 17.2.4 Layar `FE-HRD-153` — Manajemen Saldo, Alokasi & Carry-Forward HR (`/hr/leave/balances-admin`)
* **Tujuan:** Layar bagi HR Leave Administrator untuk memantau saldo seluruh pegawai rumah sakit, menjalankan scheduler alokasi awal tahun (12 hari), eksekusi batasan carry-forward (max 6 hari), eksekusi hangus saldo per 30 Juni, dan melakukan penyesuaian saldo resmi (`HRD-DEC-110`, `HRD-DEC-115`).
* **Wilayah Layar:**
  1. *Filter & Pencarian:* Filter unit kerja, profesi staf, filter saldo hampir kedaluwarsa, dan pencarian nama/NIP.
  2. *Tabel Saldo Agregat Seluruh Staf:* NIP, Nama, Unit, Hak Dasar 2026, Saldo Bawaan 2025, Total Terpakai, Sisa Saldo Aktif, Sisa Bawaan Hangus, dan Status Keaktifan.
  3. *Panel Tata Kelola Carry-Forward (Tengah Tahun):*
     * Widget pemantauan: Jumlah staf dengan sisa cuti bawaan 2025 yang belum terpakai per mendekati 30 Juni.
     * Tombol Aksi Batch: **Jalankan Eksekusi Hangus Saldo Carry-Forward (Forfeiture Runner)** untuk memposting pemotongan otomatis sisa bawaan.
  4. *Tombol Penyesuaian Saldo Resmi (HR Adjustment):* Membuka modal audit penyesuaian saldo dengan kewajiban input nomor surat keputusan/audit HR.

#### 17.2.5 Layar `FE-HRD-154` — Inisiasi & Persetujuan Pemanggilan Darurat (Leave Recall) (`/hr/leave/recalls`)
* **Tujuan:** Mengelola alur pemanggilan kembali pegawai yang sedang cuti saat situasi darurat rumah sakit (KLB, bencana alam, lonjakan pasien kritis) dan pemulihan saldo otomatis (`HRD-DEC-113`).
* **Wilayah Layar:**
  1. *Tabel Pemantauan Staf Sedang Cuti:* Menampilkan seluruh perawat/dokter yang saat ini berada dalam masa cuti aktif beserta tanggal mulai-selesai dan kontak darurat.
  2. *Modal Inisiasi Recall (Bagi Kepala Unit):*
     * Pilihan staf yang sedang cuti.
     * Tanggal efektif harus kembali dinas.
     * Kategori darurat (KLB Penyakit Menular, Bencana Alam/Korban Massal, Lonjakan Pasien IGD/ICU Cito).
     * Penjelasan alasan darurat pemanggilan.
     * Estimasi hari cuti yang batal dinikmati dan akan dipulihkan ke saldo pegawai.
  3. *Kotak Masuk Persetujuan HR Manager:*
     * HR Manager meninjau urgensi pemanggilan darurat.
     * Tombol **Setujui Pemanggilan & Pulihkan Saldo**: Otomatis menerbitkan SK tugas kembali, memulihkan saldo cuti pada buku besar (`RecallRestore`), dan mengubah status jadwal shift staf di Roster 06 menjadi aktif bertugas.
     * Tombol **Tolak Usulan Pemanggilan**: Menolak recall jika situasi dinilai masih dapat ditangani staf yang ada.

#### 17.2.6 Layar `FE-HRD-155` — Rekonsiliasi Akhir Cuti & Penutupan Periode Payroll (`/hr/leave/reconciliations`)
* **Tujuan:** Memastikan seluruh transaksi cuti terverifikasi rapi sebelum cut-off payroll bulanan, mengunci mutlak cuti `Completed` dari pembatalan bebas (menutup celah `/reverse` per `HRD-DEC-115`), dan menangani rekonsiliasi retrospektif cuti pending (`HRD-DEC-114`).
* **Wilayah Layar:**
  1. *Status Kesiapan Payroll:* Periode Penggajian (misal November 2026), Tanggal Cut-Off, dan Status Penutupan Cuti (`Open` / `Reconciled` / `PayrollClosed`).
  2. *Tabel Cuti Menggantung (Pending Cut-Off):* Daftar pengajuan cuti yang belum disetujui atasan saat tanggal cut-off. Sistem menampilkan label: *"Akan direkonsiliasi retrospektif pada payroll bulan berikutnya pasca-approval"*.
  3. *Tabel Rekonsiliasi Retrospektif Periode Lalu:* Menampilkan daftar cuti bulan lalu yang baru disetujui setelah cut-off, yang nilai penyesuaian potongannya otomatis diserahkan ke batch payroll bulan ini.
  4. *Tombol Aksi Utama:*
     * Tombol **Tutup & Kunci Periode Cuti**: Mengunci mutlak seluruh permohonan yang telah selesai menjadi *read-only/immutable* (menonaktifkan tombol pembatalan biasa).
     * Tombol **Serahkan Data ke Modul Payroll**: Mengirimkan snapshot absensi dan cuti terverifikasi ke Sub-Modul 10.

### 17.3 Komponen Modal & Drawer Pendukung

* **`SafeStaffingCoverageBannerModal`:** Menampilkan detail simulasi rasio ketenagaan shift unit bila cuti disetujui (daftar perawat yang dinas pada jam tersebut, jumlah pasien yang dirawat, dan selisih kuota minimal).
* **`MedicalCertificateUploadModal`:** Formulir upload berkas PDF/JPG surat keterangan dokter resmi berstempel dan ber-SIP, dengan pratinjau dokumen terbatas hak akses.
* **`LeaveRecallInitiationModal`:** Modal darurat pemanggilan staf cuti dengan kalkulasi otomatis pemulihan sisa hari cuti (*restored days calculation*).
* **`LeaveBalanceAdjustmentModal`:** Modal khusus HR Manager untuk menambah/mengurangi saldo cuti staf secara resmi dengan input nomor dokumen audit dan alasan operasional tertulis.


---

## 18. Bagian `S-B3` — Arsitektur Frontend Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09, `last_changed_in` `v13`, 4 November 2026)

Bagian ini mendefinisikan rancangan arsitektur antarmuka pengguna (UI/UX) untuk **Sub-modul 09. Overtime Management (Slice `S-B3`)**, menggunakan standar Next.js App Router, React Context, Redux Toolkit, Axios Client, dan Design System Quilvian.

### 18.1 Peta Menu & Navigasi Antarmuka

| ID Layar | Judul Layar | Rute Halaman (Next.js) | Tipe Layar | Hak Akses (Permission Guard) | Peran Pengguna Utama |
| --- | --- | --- | :---: | --- | --- |
| `FE-HRD-156` | Pengajuan SPKL & Klaim Cito Mandiri | `/hr/self-service/overtime` | Halaman Mandiri (ESS) | `[AccessPermission("OvertimeRequest", "ViewSelf")]` | Seluruh Staf Medis & Non-Medis |
| `FE-HRD-157` | Kotak Masuk Persetujuan Lembur Ruangan | `/hr/overtime/approvals` | Halaman Approval | `[AccessPermission("OvertimeApproval", "View")]` | Kepala Ruangan, Kepala Instalasi |
| `FE-HRD-158` | Pusat Verifikasi Presensi & Discrepancy HR | `/hr/overtime/verifications` | Halaman Administrasi HR | `[AccessPermission("OvertimeVerification", "Manage")]` | HR Officer, HR Overtime Admin |
| `FE-HRD-159` | Monitoring Saldo Cuti Pengganti (CTO) | `/hr/overtime/compensatory-leave` | Halaman Tata Kelola Saldo | `[AccessPermission("OvertimeCompensatoryLeave", "View")]` | Staf RS, HR Leave Admin |
| `FE-HRD-159-A` | Kotak Masuk Eskalasi Darurat Direktur Medis | `/hr/overtime/medical-director-inbox` | Halaman Approval Eksekutif | `[AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]` | Direktur Pelayanan Medik |
| `FE-HRD-160` | Penutupan Periode & Rekonsiliasi Payroll | `/hr/overtime/reconciliations` | Halaman Rekonsiliasi Finansial | `[AccessPermission("OvertimePeriod", "Manage")]` | HR Payroll Officer, HR Manager |
| `FE-HRD-161` | Pengaturan Kebijakan & Kuota Lembur | `/hr/overtime/settings` | Halaman Konfigurasi Master | `[AccessPermission("OvertimeSetting", "Manage")]` | HR Director, System Administrator |

---

### 18.2 Skema Fitur per Layar & Komponen UI

#### 18.2.1 Layar `FE-HRD-156` — Pengajuan SPKL & Klaim Cito Mandiri (`/hr/self-service/overtime`)
* **Tujuan:** Antarmuka bagi pegawai/nakes untuk mengajukan SPKL pra-dinas terencana (H-1) atau melaporkan klaim darurat medis pasca-dinas cito (H+1) (`HRD-DEC-116`).
* **Wilayah Layar:**
  1. *Banner Informasi Kuota Mingguan:* Menampilkan ringkasan jam lembur terpakai minggu ini (misal: "8 dari maksimal 18 jam reguler") berdasar PP 35/2021.
  2. *Tombol Aksi Utama:*
     * Tombol **+ Ajukan SPKL Terencana (Pra-Dinas)**: Membuka formulir pengajuan H-1.
     * Tombol **+ Lapor Lembur Darurat Medis (Pasca-Dinas Cito)**: Membuka formulir pelaporan tindakan cito medis H+1.
  3. *Tabel Riwayat Permohonan Lembur Pribadi:* Nomor SPKL, Kategori (`Planned` / `EmergencyCito`), Tanggal, Jam Mulai-Selesai, Durasi Klaim, Durasi Terverifikasi Mesin, Opsi Kompensasi (`Uang Lembur` / `CTO`), Status (`Draft`, `WaitingApproval`, `AutoVerified`, `FlaggedDiscrepancy`, `ApprovedForPayroll`), dan Tombol Aksi Detail.
  4. *Modal Input Dual-Mode SPKL:*
     * Tab 1: **Lembur Terencana (Pra-Dinas):** Pemilihan tanggal masa depan, estimasi jam dinas, pemilihan bentuk kompensasi (Uang Lembur / CTO), uraian tugas terencana.
     * Tab 2: **Lembur Darurat Cito (Pasca-Dinas):** Tanggal kerja kemarin/hari ini, jam aktual selesai, input wajib Nomor Rekam Medis Pasien / Nomor Insiden Darurat, serta deskripsi tindakan medis cito (`HRD-DEC-116`).

#### 18.2.2 Layar `FE-HRD-157` — Kotak Masuk Persetujuan Lembur Ruangan (`/hr/overtime/approvals`)
* **Tujuan:** Layar bagi Kepala Ruangan/Kepala Unit untuk memvalidasi urgensi riil pelayanan lembur staf unitnya dengan perlindungan pemblokiran persetujuan diri sendiri (`HRD-DEC-120`).
* **Wilayah Layar:**
  1. *Filter Cepat:* Filter Kategori (`Planned` vs `Cito Darurat`), Filter Status, dan Pencarian Staf.
  2. *Tabel Antrean Persetujuan:* NIP, Nama Staf, Profesi, Tanggal Lembur, Jam Dinas, Durasi, Kategori Lembur, Nomor Rekam Medis (jika cito), Akumulasi Jam Minggu Ini, dan Opsi Aksi.
  3. *Panel Perlindungan Anti-Self-Approval:* Jika terdapat pengajuan lembur atas nama diri Kepala Ruangan sendiri, baris ditandai badge *Self-Request* dan tombol persetujuan dinonaktifkan dengan tulisan: *"Pengajuan dialihkan ke Kepala Instalasi / Atasan Hierarkis"* (`HRD-DEC-120`).
  4. *Aksi Persetujuan:* Tombol **Setujui Lembur**, Tombol **Minta Koreksi Jam**, dan Tombol **Tolak Pengajuan** disertai catatan alasan wajib.

#### 18.2.3 Layar `FE-HRD-158` — Pusat Verifikasi Presensi & Discrepancy HR (`/hr/overtime/verifications`)
* **Tujuan:** Antarmuka bagi HR Officer untuk meninjau kecocokan data klaim lembur terhadap mesin biometrik presensi harian (07), menerapkan aturan durasi terkecil, dan menyelesaikan anomali *discrepancy* (`HRD-DEC-119`).
* **Wilayah Layar:**
  1. *Kartu Metrik Verifikasi:* Total SPKL Disetujui, Terverifikasi Otomatis Biometrik (*AutoVerified*), Anomali Selisih Waktu (*DurationMismatch*), dan Anomali Lupa Tap Biometrik (*MissingBiometric*).
  2. *Tabel Komparasi Silang Presensi:* NIP, Nama Pegawai, Tanggal, Durasi SPKL (Klaim), Jam Tap Masuk-Keluar Mesin, Durasi Kelebihan Fisik Biometrik, Durasi Sah Dibayar (*Min-Duration*), Status Verifikasi, dan Indikator Discrepancy.
  3. *Drawer Investigasi Anomali Biometrik:* Menampilkan detail perbandingan menit demi menit antara permohonan staf dan log mentah mesin absensi. Memfasilitasi HR mengunggah lampiran *scan logbook dinas ruangan* fisik dan menentukan durasi sah akhir (*Manual Adjustment*).

#### 18.2.4 Layar `FE-HRD-159` — Monitoring Saldo Cuti Pengganti (CTO) (`/hr/overtime/compensatory-leave`)
* **Tujuan:** Mengelola hak hari libur pengganti (*Compensatory Time Off*) pegawai pelaksana yang memilih kompensasi non-finansial (`HRD-DEC-118`).
* **Wilayah Layar:**
  1. *Widget Ringkasan Saldo CTO:* Total Jam CTO Aktif RS, Total Hari CTO Diterbitkan, Total Jam Hangus (Kedaluwarsa 90 Hari).
  2. *Tabel Rincian Saldo CTO Staf:* NIP, Nama Pegawai, Unit Kerja, Nomor SPKL Rujukan, Tanggal Terbit, Jam Diberikan, Sisa Jam Aktif, Tanggal Kedaluwarsa (H+90 Hari), dan Status (`Active`, `FullyUsed`, `Expired`).
  3. *Tombol Eksekusi Scheduler Hangus:* Menampilkan daftar saldo yang telah melewati 90 hari kalender untuk diproses hangus (*Forfeiture Runner*) berdasar `HRD-ASM-25`.

#### 18.2.5 Layar `FE-HRD-159-A` — Kotak Masuk Eskalasi Darurat Direktur Medis (`/hr/overtime/medical-director-inbox`)
* **Tujuan:** Layar khusus bagi Direktur Pelayanan Medik untuk meninjau dan memberikan otorisasi darurat tingkat tinggi atas klaim cito medis yang melampaui kuota baku PP 35/2021 (`HRD-DEC-117`).
* **Wilayah Layar:**
  1. *Peringatan Kelelahan Klinis (Fatigue Risk Indicator):* Menampilkan akumulasi jam dinas nakes selama 7 hari terakhir beserta indeks potensi risiko kelelahan nakes.
  2. *Detail Kasus Darurat:* Nomor Rekam Medis Pasien, Ringkasan Tindakan Pembedahan / Resusitasi Cito, Jam Lembur yang Melampaui Kuota, dan Catatan Kepala Ruangan.
  3. *Aksi Direktur:* Tombol **Beri Otorisasi Darurat Penyelamatan Nyawa** (dengan rekomendasi jadwal istirahat pengganti) atau Tombol **Tolak Eskalasi Kuota**.

#### 18.2.6 Layar `FE-HRD-160` — Penutupan Periode & Rekonsiliasi Payroll (`/hr/overtime/reconciliations`)
* **Tujuan:** Mengunci berkas lembur bulanan pada tanggal cut-off (tgl 20), memilah SPKL siap bayar, mengelola klaim pending untuk rekonsiliasi retrospektif, dan menyerahkan paket data ke Payroll Sub-modul 10 (`HRD-DEC-121`).
* **Wilayah Layar:**
  1. *Status Periode:* Periode Berjalan (misal November 2026), Tanggal Cut-Off (20 Nov 2026), Status Kunci (`Open`, `CutoffLocked`, `Closed`).
  2. *Tabel SPKL Siap Handoff:* Daftar lembur terverifikasi bersih, nominal pengali jam lembur (1.5x / 2x), total jam dibayar, dan total hak CTO yang telah diposting.
  3. *Tabel SPKL Pending (Retrospektif):* Pengajuan yang masih dalam status investigasi discrepancy saat cut-off tiba. Diberi badge transparan: *"Ditahan sementara; otomatis direkonsiliasi pada payroll Desember 2026 begitu disetujui"*.
  4. *Tabel Rapel Retrospektif Periode Lalu:* Menampilkan klaim periode Oktober yang baru selesai diverifikasi setelah cut-off lalu, yang otomatis disertakan pada pembayaran gaji bulan ini.
  5. *Tombol Kunci & Handoff:* Tombol **Kunci Snapshot Periode (Immutable)** dan **Eksekusi Serah Terima ke Payroll**.

#### 18.2.7 Layar `FE-HRD-161` — Pengaturan Kebijakan & Kuota Lembur (`/hr/overtime/settings`)
* **Tujuan:** Konfigurasi parameter operasional lembur rumah sakit (kuota jam harian/mingguan reguler, rumus pengali tarif, toleransi discrepancy mesin, masa berlaku saldo CTO).

---

### 18.3 Komponen Modal & Drawer Pendukung

* **`OvertimeDualModeFormModal`:** Formulir terpadu yang dapat beralih antara mode *Planned Pre-Overtime* dan *Emergency Cito Post-Claim*.
* **`BiometricDiscrepancyInvestigationDrawer`:** Drawer sisi kanan untuk meneliti log mentah mesin absensi vs jadwal form lembur staf, dilengkapi form unggah berkas logbook fisik unit.
* **`MedicalDirectorEscalationModal`:** Modal verifikasi darurat penyelamatan nyawa pasien dengan peringatan kelelahan klinis nakes.
* **`CtoConversionConfirmationModal`:** Dialog konfirmasi bagi staf saat memilih kompensasi cuti pengganti dengan peringatan masa kedaluwarsa 90 hari kalender.

---

## 19. Bagian `S-B5` — Arsitektur Frontend Penggajian Pegawai Rumah Sakit (Sub-modul 10, `last_changed_in` `v14`, 5 November 2026)

Bagian ini mendefinisikan arsitektur antarmuka pengguna (UI/UX) untuk **Sub-modul 10. Payroll Management (Slice `S-B5`)**, menggunakan standar Next.js App Router, React Context, Redux Toolkit, Axios Client, dan Design System Quilvian.

### 19.1 Peta Menu & Navigasi Antarmuka

| ID Layar | Judul Layar | Rute Halaman (Next.js) | Tipe Layar | Hak Akses (Permission Guard) | Peran Pengguna Utama |
| :--- | :--- | :--- | :---: | :--- | :--- |
| `FE-HRD-162` | Dasbor Siklus & Pelaksanaan Payroll | `/hr/payroll/runs` | Halaman Penggajian | `[AccessPermission("PayrollRun", "View")]` | Payroll Officer, HR Manager |
| `FE-HRD-163` | Lembar Kerja Detail Perhitungan & Variansi Gaji | `/hr/payroll/runs/[id]/worksheet` | Lembar Kerja Detail | `[AccessPermission("PayrollCalculation", "Execute")]` | Payroll Officer |
| `FE-HRD-164` | Kotak Masuk Approval Payroll Berjenjang | `/hr/payroll/approvals` | Halaman Approval SoD | `[AccessPermission("PayrollApproval", "Review")]` | HR Manager, Direktur SDM/Keuangan |
| `FE-HRD-165` | Dasbor Serah Terima & Rekonsiliasi Finance | `/hr/payroll/finance-handoff` | Halaman Integrasi Handoff | `[AccessPermission("PayrollHandoff", "Dispatch")]` | HR Payroll Officer, Finance Liaison |
| `FE-HRD-166` | Portal Slip Gaji Mandiri Pegawai ESS | `/hr/self-service/payslips` | Halaman Mandiri (ESS) | `[AccessPermission("SelfServicePayroll", "View")]` | Seluruh Staf Rumah Sakit |
| `FE-HRD-167` | Pengaturan Kebijakan & Formula Penggajian RS | `/hr/payroll/settings` | Halaman Konfigurasi Master | `[AccessPermission("PayrollSetting", "View")]` | HR Director, System Administrator |

---

### 19.2 Skema Fitur per Layar & Komponen UI

#### 19.2.1 Layar `FE-HRD-162` — Dasbor Siklus & Pelaksanaan Payroll (`/hr/payroll/runs`)
* **Tujuan:** Pusat kendali siklus penggajian bulanan bagi Petugas Payroll untuk membuka periode, memantau prasyarat penutupan waktu kerja (Presensi, Cuti, Lembur per tgl 20), dan memantau status pemrosesan (`HRD-DEC-122`).
* **Wilayah Layar:**
  1. *Banner Prasyarat Waktu Kerja:* Indikator visual status penutupan Sub-modul 07, 08, dan 09. Jika masih ada SPKL lembur atau izin cuti berstatus *Draft/Pending*, tombol *Inisiasi Periode Baru* dinonaktifkan dengan daftar transaksi gantung.
  2. *Kartu Metrik Periode Berjalan:* Status Siklus (`Draft`, `Calculating`, `UnderReview`, `WaitingFinalApproval`, `HandoffReady`, `Closed`), Total Pegawai Terproses, Estimasi Total Beban Gaji Kotor (*Gross Pay*), dan Total Potongan Pajak PPh 21 TER / BPJS.
  3. *Tabel Riwayat Periode Penggajian:* Daftar siklus penggajian dengan filter tahun dan status, dilengkapi tombol navigasi ke lembar kerja detail.
  4. *Tombol Aksi Utama:* Tombol **Inisiasi Siklus Penggajian Baru** (membuka modal pembentukan run) dan **Batalkan Draf Run**.

#### 19.2.2 Layar `FE-HRD-163` — Lembar Kerja Detail Perhitungan & Variansi Gaji (`/hr/payroll/runs/[id]/worksheet`)
* **Tujuan:** Lembar kerja audit individual pegawai bagi Petugas Payroll untuk mengeksekusi perhitungan, memeriksa rincian pendapatan/potongan, serta menindaklanjuti peringatan deviasi anomali gaji (`HRD-DEC-123`, `HRD-DEC-124`).
* **Wilayah Layar:**
  1. *Header Ringkasan Run:* Nomor Referensi Run, Periode Penggajian, Tanggal Cut-off (tgl 20), dan Status Verifikasi.
  2. *Filter Cepat Variansi:* Tab filter: Seluruh Pegawai, Bertanda Variansi (`VarianceAlert` $\ge 20\%$), Belum Terjustifikasi, dan Normal.
  3. *Tabel Matriks Gaji Individual:* Kolom NIK, Nama Staf, Unit Kerja, Gaji Pokok, Tunjangan Medis, Lembur Terverifikasi, Premi Shift Malam, Potongan PPh 21 TER, Potongan BPJS, Potongan Pinjaman, Gaji Bersih (*Net Pay*), Persentase Deviasi vs Bulan Lalu, dan Status Variansi.
  4. *Aksi Baris Pegawai:* Tombol **Rincian Komponen** (membuka modal breakdown formula) dan **Isi Justifikasi Variansi** (membuka drawer justifikasi).
  5. *Tombol Validasi Gerbang:* Tombol **Kirim untuk Tinjauan Manajer (Submit for Review)** — otomatis terkunci bila masih ada *VarianceAlert* tanpa catatan penjelasan tertulis (`VR-PAY-07`).

#### 19.2.3 Layar `FE-HRD-164` — Kotak Masuk Approval Payroll Berjenjang (`/hr/payroll/approvals`)
* **Tujuan:** Antarmuka persetujuan berjenjang dengan pemisahan tugas ketat (*3-Tier Segregation of Duties*) bagi HR Manager dan Direktur SDM/Keuangan (`HRD-DEC-125`).
* **Wilayah Layar:**
  1. *Indikator Jenjang Otorisasi:* Rantai visual proses: Tahap 1 (Payroll Officer Selesai) $	o$ Tahap 2 (HR Manager Review) $	o$ Tahap 3 (Direktur Final Lock).
  2. *Panel Audit Sampling Variansi:* Menampilkan daftar ringkas pegawai yang memiliki deviasi gaji $\ge 20\%$ beserta catatan penjelasan yang diisi oleh Petugas Payroll.
  3. *Rangkuman Komponen Agregat Rumah Sakit:* Rekapitulasi total gaji pokok, total biaya lembur IGD/OK, total insentif shift, dan total potongan pajak per unit pelayanan.
  4. *Proteksi Anti-Self-Approval:* Sistem menonaktifkan tombol persetujuan bila approver yang login memiliki ID yang sama dengan pembuat/penghitung payroll run.
  5. *Tombol Aksi Penilai:* Tombol **Setujui Tinjauan (Approve)**, **Minta Revisi (Request Revision)**, atau **Otorisasi Final & Kunci Permanen**.

#### 19.2.4 Layar `FE-HRD-165` — Dasbor Serah Terima & Rekonsiliasi Finance (`/hr/payroll/finance-handoff`)
* **Tujuan:** Antarmuka handoff resmi ke Departemen Keuangan dan pemantauan status kliring perbankan serta penanganan rollback penolakan (`HRD-DEC-126`).
* **Wilayah Layar:**
  1. *Status Serah Terima Berkas:* Status Handoff (`Dispatched`, `AcknowledgedByFinance`, `PaymentConfirmed`, `RejectedByFinance`), Tanggal Penyerahan, dan Kode Checksum SHA-256 Integritas Data.
  2. *Rekapitulasi Rekening Bank Mitra:* Total nominal yang harus ditransfer per bank mitra RS (misal Bank Mandiri, BCA, BNI).
  3. *Panel Notifikasi Penolakan Finance (Bila Ditolak):* Kotak peringatan merah menampilkan rincian alasan penolakan tertulis dari Finance dan tombol aksi **Buka Kunci untuk Koreksi (Controlled Rollback)**.
  4. *Tombol Unduh Berkas Handoff:* Tombol ekspor dokumen serah terima resmi ber-checksum untuk arsip audit gabungan HR dan Finance.

#### 19.2.5 Layar `FE-HRD-166` — Portal Slip Gaji Mandiri Pegawai ESS (`/hr/self-service/payslips`)
* **Tujuan:** Portal layanan mandiri bagi seluruh pegawai RS untuk mengakses riwayat dan rincian slip gaji bulanan mereka secara aman dan rahasia (`HRD-DEC-127`).
* **Wilayah Layar:**
  1. *Filter Riwayat Penggajian:* Pemilih Tahun dan Daftar Kartu Bulan Penggajian dengan status rilis (*Tersedia* atau *Terkunci Sementara*).
  2. *Dialog Kunci Keamanan PIN Sekunder:* Dialog modal yang mewajibkan input 6-digit PIN rahasia pegawai sebelum lembar rincian gaji dibuka di layar.
  3. *Lembar Slip Gaji Digital Resmi:* Tampilan terstruktur rincian pendapatan (gaji pokok, tunjangan profesi, lembur, shift malam) dan rincian potongan (PPh 21 TER, BPJS Kes/TK, pinjaman koperasi). Dilengkapi tombol *Toggle Hide/Show Amount* untuk privasi di nurse station.
  4. *Tombol Unduh PDF Ber-Password:* Tombol untuk mengunduh dokumen slip gaji dalam format PDF ber-watermark resmi rumah sakit dan dilindungi sandi kombinasi NIK/Tgl Lahir.

#### 19.2.6 Layar `FE-HRD-167` — Pengaturan Kebijakan & Formula Penggajian RS (`/hr/payroll/settings`)
* **Tujuan:** Halaman konfigurasi bagi HR Administrator untuk mengelola parameter operasional siklus penggajian: tanggal cut-off (default tgl 20), tanggal gajian resmi (default tgl 25), ambang variansi peringatan (20% / Rp 2 jt), tarif premi shift malam, dan parameter BPJS (`HRD-ASM-27` s.d. `HRD-ASM-30`).

---

### 19.3 Komponen Modal & Drawer Pendukung

* **`VarianceJustificationDrawer`:** Drawer sisi kanan bagi Petugas Payroll untuk meneliti riwayat komparasi gaji pegawai dan menginput catatan alasan formal atas deviasi variansi $\ge 20\%$.
* **`PayrollApprovalConfirmationModal`:** Dialog konfirmasi penilai manajerial/direksi yang memuat pernyataan kepatuhan audit dan ringkasan angka total sebelum run dikunci permanen.
* **`FinanceRejectionFeedbackModal`:** Modal dialog yang menampilkan catatan resmi penolakan dari Departemen Keuangan beserta opsi membuka kunci data untuk perbaikan.
* **`PayslipPinVerificationModal`:** Modal autentikasi sekunder pada portal ESS dengan antarmuka keypad PIN angka 6-digit dan penanganan penguncian akun jika salah 3 kali.
* **`PayrollWorksheetBreakdownModal`:** Modal rincian formula per baris pegawai yang menampilkan detail jam lembur asal Sub-09, jam shift malam asal Sub-07, dan tarif TER asal master PTKP.

## 20. Bagian `S-B6` — Arsitektur Frontend Restitusi & Pemotongan Penggajian Pegawai Rumah Sakit (Sub-modul 11, `last_changed_in` `v15`, 5 November 2026)

Bagian ini mendefinisikan arsitektur antarmuka pengguna (frontend Next.js App Router) untuk **Sub-modul 11. Restitusi & Integrasi Pemotongan Penggajian (Slice `S-B6`)**, mencakup navigasi menu portal HR dan ESS, skema tata letak fitur per layar, pemisahan hak akses tombol aksi, penegakan visual batas maksimal potongan upah (*Hard-Cap 50%*), formulir klaim digital mandiri, hingga modul kasir cito darurat.

### 20.1 Peta Menu & Navigasi Antarmuka

| Kode Layar | Nama Layar | Rute Halaman (Route URL) | Tipe Layar | Induk Menu | Butir Hak Akses Penjaga | Deskripsi Singkat |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FE-HRD-168` | Dasbor Restitusi & Pemotongan | `/corporate/hrd/payroll/deductions` | Halaman Penuh | Penggajian & Remunerasi | `[AccessPermission("HR_Deduction", "View")]` | Pusat kendali monitoring realisasi potongan, rasio upah 50%, dan klaim restitusi aktif |
| `FE-HRD-169` | Portal ESS Klaim Restitusi Mandiri | `/ess/restitution-claims` | Halaman Penuh | Portal Mandiri Karyawan (ESS) | `[AccessPermission("ESS_Restitution", "Create")]` | Layar pengajuan mandiri klaim kacamata/faskes luar darurat dan pemantauan kuota tahunan |
| `FE-HRD-170` | Lembar Kerja Jadwal Angsuran & Plafon 50% | `/corporate/hrd/payroll/deductions/schedules` | Halaman Penuh | Penggajian & Remunerasi | `[AccessPermission("HR_Deduction", "Manage")]` | Manajemen jadwal cicilan pinjaman Finance, evaluasi batas 50%, dan antrean tagihan tunda |
| `FE-HRD-171` | Drawer Investigasi Koreksi Restitusi Gaji | *Drawer Slide-Over* pada `FE-HRD-168` / `FE-HRD-170` | Drawer | Layar `FE-HRD-168` | `[AccessPermission("HR_Deduction", "Correct")]` | Investigasi salah potong periode lampau, penguncian slip asal, dan injeksi kompensasi gaji berjalan |
| `FE-HRD-172` | Persetujuan Klaim & Penerbitan Voucher Cito | `/corporate/hrd/payroll/deductions/approvals` | Halaman Penuh | Kotak Masuk Persetujuan | `[AccessPermission("HR_Deduction", "Approve")]` | Verifikasi kuitansi nota, persetujuan bertingkat, dan penerbitan voucher QR kasir cito 24/7 |
| `FE-HRD-173` | Pengaturan Plafon Restitusi & Batas Potongan | `/corporate/hrd/payroll/deductions/settings` | Halaman Penuh | Pengaturan HRD | `[AccessPermission("HR_Setting", "Manage")]` | Konfigurasi batas kuota kacamata/medis per grade, batas 50% PP 36/2021, dan izin cito kasir |

---

### 20.2 Skema Fitur per Layar & Komponen UI

#### 1. `FE-HRD-168` — Dasbor Restitusi & Pemotongan Gaji (*Restitution & Deduction Dashboard*)
- **Tujuan Layar:** Memberikan ringkasan eksekutif bagi HR C&B dan Payroll Officer mengenai total potongan non-regulasi bulan berjalan, pegawai yang mendekati batas hard-cap 50%, cicilan yang tertunda (*carry-over*), serta status klaim restitusi.
- **Header:** Judul Halaman, Periode Aktif Penggajian (badge status jadwal terkunci tgl 20), tombol *"Tarik Snapshot Keuangan"* (`FinanceDeductionSync`), tombol *"Simulasi Plafon 50%"*.
- **Kartu Ringkasan (Metric Cards):**
  1. *Total Potongan Terjadwal:* Akumulasi seluruh cicilan pinjaman, ganti rugi, dan koperasi (format Rupiah).
  2. *Potongan Terealisasi:* Nominal yang sah dipotong dari slip gaji periode berjalan.
  3. *Total Tagihan Ditunda (Deferred):* Nilai tagihan yang melampaui plafon 50% dan dialihkan ke bulan depan.
  4. *Klaim Restitusi Menunggu Approval:* Jumlah berkas klaim ESS yang butuh tindakan verifikasi.
- **Tabel Peringatan Rasio Potongan Gaji (>40% Gross Pay):**
  - Kolom: NIP, Nama Staf, Unit/Instalasi, Gaji Kotor (Gross Pay), Total Tagihan Potongan, Rasio Potongan (%), Status Plafon (Aman / Mentok 50%), Aksi (Detail).
  - Indikator Warna: Hijau (<40%), Oranye (40-49%), Merah Tebal (>=50% / Terpotong Sebagian).
- **Tab Navigasi Cepat:** Tab Ringkasan, Tab Antrean Cicilan Tertunda (*Deferred List*), Tab Riwayat Koreksi Salah Potong.

#### 2. `FE-HRD-169` — Portal ESS Klaim Restitusi Mandiri (*Employee Self-Service Restitution Claim*)
- **Tujuan Layar:** Memfasilitasi seluruh staf rumah sakit (dokter, perawat, penunjang, umum) untuk mengajukan penggantian biaya kacamata kerja, faskes darurat luar, atau dinas cito secara transparan dari gawai masing-masing.
- **Panel Saldo Plafon Pegawai (Annual Limit Tracker):**
  - Progress bar visual plafon kacamata: Plafon Tahunan, Terpakai, Sisa Kuota Tersedia (contoh: Rp 1.500.000 / Rp 2.500.000 tersisa).
  - Progress bar visual plafon faskes luar darurat.
- **Formulir Pengajuan Klaim Restitusi Baru:**
  - Pilihan Kategori Klaim: Kacamata Kerja, Faskes Luar Darurat, Biaya Dinas Operasional Cito.
  - Tanggal Pengeluaran (Date Picker, dibatasi maksimal mundur 60 hari dari hari ini).
  - Nominal Kuitansi (Format angka Rupiah dengan auto-currency masking).
  - Checkbox Darurat Cito Nakes: *"Insiden Pajanan Medis Cito (Butuh Pencairan Kasir 24/7)"* (khusus staf medis).
  - Area Dropzone Upload Bukti: Unggah foto kuitansi/struk asli + resume medis dokter (mendukung pratinjau instan foto dan validasi ukuran < 5MB).
  - Kolom Catatan / Keperluan Klinis: Alasan pengajuan klaim.
  - Tombol Submit: *"Ajukan Klaim Restitusi"* (menjalankan validasi sisa plafon di sisi klien sebelum dikirim).
- **Tabel Riwayat Pengajuan Klaim Pegawai:**
  - Kolom: Nomor Klaim, Tanggal Pengajuan, Kategori, Nominal Diajukan, Nominal Disetujui, Status (Draft, Dalam Review Atasan, Disetujui, Masuk Slip Gaji, Dicairkan di Kasir), Aksi (Lihat Detail / Unduh Bukti Voucher).

#### 3. `FE-HRD-170` — Lembar Kerja Jadwal Angsuran & Guard Plafon 50% (*Deduction Schedule & Hard-Cap Guard*)
- **Tujuan Layar:** Lembar kerja teknis bagi HR Payroll Officer untuk meninjau master pinjaman dari Finance, menyesuaikan prioritas potongan, dan memeriksa simulasi pemangkasan otomatis tagihan yang melanggar batas 50%.
- **Toolbar:** Filter Kategori Potongan (Ganti Rugi, Kasbon RS, Koperasi, Paguyuban), Filter Status (Aktif, Lunas, Ditunda), Kotak Pencarian Nama/NIP Karyawan, Tombol Export Excel.
- **Tabel Master Angsuran Berjalan:**
  - Kolom: NIP & Nama Pegawai, Jenis Potongan, Prioritas (Badge angka 1 s.d. 4), Referensi Piutang Finance, Pokok Awal, Angsuran per Bulan, Tenor Terbayar (contoh: 4/12 bulan), Sisa Saldo Pokok, Status Cicilan, Aksi (Lihat Histori, Jeda Cicilan, Rincian).
- **Simulasi Penegakan Hard-Cap 50% (Simulation Modal):**
  - Menampilkan skenario pemangkasan otomatis jika gaji pegawai anjlok (misal akibat izin tidak berbayar):
  - Kartu Perhitungan: Gaji Kotor = Rp 8.000.000 $
ightarrow$ Batas Maksimal 50% = Rp 4.000.000.
  - Rincian Eksekusi Berjenjang:
    - Prioritas 1 (Ganti Rugi): Rp 1.000.000 (Dipotong Penuh - Sisa Kuota Rp 3.000.000)
    - Prioritas 2 (Kasbon RS): Rp 2.500.000 (Dipotong Penuh - Sisa Kuota Rp 500.000)
    - Prioritas 3 (Koperasi): Rp 1.500.000 (Dipotong Parsial Rp 500.000, Ditunda Rp 1.000.000)
    - Prioritas 4 (Paguyuban): Rp 200.000 (Ditunda Penuh Rp 200.000)
  - Notifikasi: *"Total tagihan ditunda Rp 1.200.000 otomatis dialihkan ke siklus bulan berikutnya."*

#### 4. `FE-HRD-171` — Drawer Investigasi & Koreksi Restitusi Gaji (*Payroll Correction Drawer*)
- **Tujuan Layar:** Komponen slide-over yang dibuka saat HR menemukan atau menerima klaim salah potong periode masa lalu, memungkinkan pengembalian dana tanpa merevisi slip gaji lampau.
- **Panel Pencarian Slip Gaji Terdampak:** Input NIP pegawai, pilih siklus bulan penggajian terdampak (misal: "September 2026"), sistem memuat baris komponen potongan slip gaji tersebut.
- **Pemilihan Komponen Keliru:** Radio button memilih komponen yang salah potong (contoh: *"Koperasi Karyawan - Terpotong Ganda Rp 500.000"*).
- **Formulir Restitusi Koreksi:**
  - Nominal Pengembalian (Rupiah, tidak boleh melebihi nilai komponen asal).
  - Justifikasi Kesalahan (TextArea wajib diisi, minimal 20 karakter).
  - Dokumen Bukti Pendukung: Unggah surat pernyataan Finance / rekapitulasi rekening koran.
  - Periode Pengembalian: Default pada siklus penggajian aktif berjalan.
  - Komponen Penggajian Sasaran: Otomatis terkunci pada `RestitutionAdjustment` (Kategori: Tunjangan Non-Pajak).
- **Tombol Aksi:** *"Simpan Draf Koreksi"*, *"Kirim Otorisasi Manajer SDM & Finance"*.

#### 5. `FE-HRD-172` — Layar Persetujuan Klaim & Penerbitan Voucher Cito (*Claim Approval & Voucher Issuer*)
- **Tujuan Layar:** Panel persetujuan terpadu bagi Kepala Ruangan, Supervisor, HR C&B, dan Manajer SDM untuk memverifikasi dokumen klaim restitusi serta mencetak voucher cito kasir.
- **Daftar Antrean Verifikasi (Verification Queue):**
  - Kartu Berkas: Foto Kuitansi Asli (dapat di-zoom dan diputar), Data Pemohon, Golongan, Kuota Plafon Tersisa, Nilai Klaim yang Diajukan.
  - Kotak Penyesuaian Nilai Disetujui: HR C&B dapat menyetujui nominal penuh atau sebagian (misal: kuitansi Rp 1.200.000, namun sisa kuota hanya Rp 1.000.000 $
ightarrow$ Sistem otomatis menyetel Approved Amount = Rp 1.000.000).
- **Otorisasi Cito Kasir 24/7:**
  - Jika klaim bertanda `UrgentDisbursementCito`: Muncul tombol khusus *"Setujui & Terbitkan Voucher Kasir 24/7"*.
  - Modal Konfirmasi Voucher: Menampilkan Kode Voucher, Nilai Tunai, Token SHA-256, Waktu Kedaluwarsa (48 jam), dan tombol *"Cetak PDF Voucher"* / *"Kirim Kode ke WhatsApp Pegawai"*.
  - Peringatan Otomatis: *"Klaim ini akan dicairkan via Kasir RS. Sistem otomatis MENGECUALIKAN nominal ini dari transfer slip gaji bulanan."*

#### 6. `FE-HRD-173` — Pengaturan Plafon Restitusi & Kebijakan Potongan (*Policy & Cap Settings*)
- **Tujuan Layar:** Pengaturan parameter kebijakan tunjangan restitusi dan ambang proteksi gaji.
- **Tabel Konfigurasi per Grade:**
  - Matriks per baris: Grade Level, Kategori Manfaat, Batas Plafon Tahunan (Rupiah), Minimum Masa Kerja Pegawai (bulan), Izinkan Kasir Cito (Toggle On/Off).
- **Pengaturan Kebijakan Pemotongan Upah:**
  - Ambang Batas Pemotongan Non-Regulasi: Field terkunci pada default **50.00%** (dengan referensi hukum Pasal 65 PP 36/2021).
  - Toleransi Peringatan Dini: Input persentase (default: 40.00%) untuk menandai pegawai berisiko gagal bayar.
  - Batas Kedaluwarsa Voucher Cito: Input jam (default: 48 jam).

---

### 20.3 Komponen Modal & Drawer Pendukung

1. `ModalClaimDocumentPreview`: Pratinjau interaktif berkas foto struk/kuitansi asli dengan fitur perbesaran (*zoom*), rotasi gambar, dan kontras tinggi untuk memudahkan petugas membaca nomor nota dan cap stempel kasir.
2. `ModalVoucherQrViewer`: Dialog pop-up yang memunculkan QR Code dan token 6 digit voucher kasir untuk di-scan oleh barcode scanner loket Kasir RS 24/7.
3. `ModalFinanceSyncConfirm`: Dialog konfirmasi saat menarik data cicilan H-3 dari modul Keuangan, menampilkan ringkasan jumlah pegawai dan total tagihan baru yang masuk.
4. `ModalOutstandingReceivablePrint`: Dialog penerbitan *Surat Tagihan Sisa Piutang Tertunggak* bagi staf PHK/Resign yang memiliki sisa utang melebihi nilai pesangon akhir, lengkap dengan slip setoran kasir.


---

## 21. Desain Arsitektur Frontend Sub-Modul 12 (Slice S-D3 Benefit Management — Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)

Bagian ini mendefinisikan rancangan arsitektur antarmuka pengguna (frontend) untuk **Sub-Modul 12 (Benefit Management — Slice `S-D3`)**, mencakup perancangan dasbor eksekutif manfaat kesehatan rumah sakit, matriks konfigurasi paket bertingkat 3-lapisan per grade (`HRD-DEC-134`), portal mandiri pendaftaran tanggungan keluarga di ESS dan antrean verifikasi berkas KK/akta oleh HR (`HRD-DEC-135`), modul pengecekan hak berobat nakes di RS sendiri dan mutasi buku besar plafon internal (`HRD-DEC-136`), formulir pengajuan klaim koordinasi manfaat (COB) berlampiran mandatory SEP BPJS Kesehatan (`HRD-DEC-137`), pemantauan aktivasi bertahap masa percobaan (`HRD-DEC-138`), serta monitoring kliring benefit pada saat offboarding pegawai (`HRD-DEC-139`).

---

### 21.1 Peta Butir Menu (Menu Item Map Target `S-D3`)

| Label Butir Menu | Level | Menu Induk | URL Route | Target Layar (Screen ID) | Butir Hak Akses Penjaga |
| :--- | :---: | :--- | :--- | :---: | :--- |
| **Dasbor & Analitik Manfaat** | 2 | Manajemen Manfaat | `/corporate/hrd/benefits/dashboard` | `FE-HRD-174` | `[AccessPermission("HR_Benefit", "View")]` |
| **Paket Manfaat & Kelayakan** | 2 | Manajemen Manfaat | `/corporate/hrd/benefits/plans` | `FE-HRD-175` | `[AccessPermission("HR_Benefit_Plan", "View")]` |
| **Kepesertaan Asuransi Pegawai**| 2 | Manajemen Manfaat | `/corporate/hrd/benefits/enrollments` | `FE-HRD-176` | `[AccessPermission("HR_Benefit_Enrollment", "View")]` |
| **Verifikasi Tanggungan Keluarga**| 2 | Manajemen Manfaat | `/corporate/hrd/benefits/dependents` | `FE-HRD-177` | `[AccessPermission("HR_Benefit_Dependent", "Verify")]` |
| **Plafon Berobat Internal RS** | 2 | Manajemen Manfaat | `/corporate/hrd/benefits/inhouse-ledger`| `FE-HRD-178` | `[AccessPermission("HR_Benefit_InHouse", "View")]` |
| **Klaim Koordinasi Manfaat (COB)**| 2 | Manajemen Manfaat | `/corporate/hrd/benefits/cob-claims` | `FE-HRD-179` | `[AccessPermission("HR_Benefit_Claim", "View")]` |
| *ESS — Manfaat & Asuransi Saya* | 1 | Portal Mandiri ESS | `/ess/my-benefits` | `FE-HRD-176` (ESS View) | `[AccessPermission("ESS_Benefit", "ViewSelf")]` |
| *ESS — Daftarkan Tanggungan* | 1 | Portal Mandiri ESS | `/ess/my-dependents` | `FE-HRD-177` (ESS View) | `[AccessPermission("ESS_Benefit", "ManageFamily")]` |
| *ESS — Pengajuan Klaim COB* | 1 | Portal Mandiri ESS | `/ess/claims/cob-submission` | `FE-HRD-179` (ESS View) | `[AccessPermission("ESS_Benefit", "SubmitClaim")]` |

---

### 21.2 Skema Fitur & Tata Letak Layar (`FE-HRD-174` s.d. `FE-HRD-179`)

#### 1. `FE-HRD-174`: Benefit Management Dashboard & Analytics
- **Tujuan Layar:** Menyajikan visibilitas menyeluruh bagi manajemen SDM mengenai status kepesertaan BPJS Kesehatan/TK tenaga medis, utilisasi saldo subsidi berobat di RS sendiri, rasio penyerapan plafon per grade, dan tren klaim COB penjamin sekunder.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Header & Filter** | Selector tahun anggaran, unit kerja, profesi medis (Dokter, Perawat, Penunjang, Umum), dan tombol ekspor PDF. | Local State / Master Unit | `HR_Benefit:View` | Dropdown unit default ke "Seluruh Rumah Sakit". |
  | **Kartu Metrik KPI** | Total Staf Terdaftar BPJS TK (H+0), Staf Terdaftar BPJS Kes (Pasca-Percobaan), Total Plafon RS Terpakai vs Pagu Tahunan, Total Klaim COB Tertagih. | `GET /api/v1/corporate/hrd/benefits/analytics/summary` | `HR_Benefit:View` | Menampilkan nilai "0 Staf / Rp 0" jika data periode belum diinisiasi. |
  | **Grafik Utilisasi Plafon** | Bar chart persentase serapan plafon berobat internal RS per kuartal, dikelompokkan berdasarkan rawat inap vs rawat jalan. | `GET /api/v1/corporate/hrd/benefits/analytics/utilization-trends` | `HR_Benefit:View` | Placeholder grafik "Belum ada transaksi berobat internal tercatat". |
  | **Tabel Anomali & Peringatan** | Daftar tanggungan yang mendekati batas usia 21/25 tahun (butuh surat dekanat baru) dan staf masa kerja > 3 bulan yang belum terdaftar BPJS. | `GET /api/v1/corporate/hrd/benefits/analytics/alerts` | `HR_Benefit:View` | Tanda centang hijau "Seluruh administrasi kepesertaan mutakhir". |

#### 2. `FE-HRD-175`: Master Benefit Plan & Eligibility Matrix
- **Tujuan Layar:** Mengonfigurasi paket manfaat berjenjang 3-lapisan (Regulasi BPJS, Asuransi Swasta Tambahan, Subsidi RS Sendiri) dan pemetaan hak per level golongan staf RS.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Daftar Paket Manfaat** | Tabel daftar paket: Kode, Nama Paket, Kategori (BPJS/Swasta/Internal RS), Tier Kamar, Pagu Plafon Tahunan, % Diskon RS, Status Aktif. | `GET /api/v1/corporate/hrd/benefits/plans` | `HR_Benefit_Plan:View` | "Belum ada paket manfaat terdaftar. Klik 'Tambah Paket'". |
  | **Drawer Detail & Edit Paket** | Form konfigurasi limit tahunan, toggle mandatory COB BPJS, minimal masa kerja (3 bulan), dan hak fasilitas. | `PUT /api/v1/corporate/hrd/benefits/plans/{id}` | `HR_Benefit_Plan:Manage`| Validasi form: Kode paket harus unik, limit >= 0. |
  | **Matriks Kelayakan Grade** | Tabel silang matriks Grade Level vs Paket Manfaat vs Hak Kelas Rawat Inap (Kelas 2, Kelas 1, VIP, VVIP). | `GET /api/v1/corporate/hrd/benefits/plans/eligibility-matrix` | `HR_Benefit_Plan:View` | Tampilkan matriks kosong jika grade belum dipetakan. |

#### 3. `FE-HRD-176`: Employee Insurance Enrollment & Tracking
- **Tujuan Layar:** Mengelola pendaftaran kepesertaan asuransi per pegawai, mencatat nomor kartu BPJS Kes, BPJS TK, polis swasta, serta mengontrol masa aktivasi bertahap H+0 vs pasca-percobaan.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Filter & Pencarian Staf** | Search bar (NIP, Nama Staf), Filter Status (`Draft`, `PendingActivation`, `Active`, `Terminated`), Unit Kerja. | Master Workforce / Local State | `HR_Benefit_Enrollment:View` | Pencarian instan (debounced 300ms). |
  | **Tabel Kepesertaan Staf** | Baris data: NIP, Nama Staf, Golongan, Status Kerja, No. BPJS Kes, No. BPJS TK, No. Polis Swasta, Status Aktivasi, Tanggal Efektif. | `GET /api/v1/corporate/hrd/benefits/enrollments` | `HR_Benefit_Enrollment:View` | "Tidak ditemukan data kepesertaan pegawai yang cocok". |
  | **Modal Edit Nomor Polis** | Form pengisian atau koreksi nomor identitas BPJS & asuransi komersial, dengan validasi format digit. | `PUT /api/v1/corporate/hrd/benefits/enrollments/{id}` | `HR_Benefit_Enrollment:Manage` | Notifikasi "Nomor BPJS berhasil diperbarui". |

#### 4. `FE-HRD-177`: Employee Family Dependents Portal & Verification
- **Tujuan Layar:** Memfasilitasi pendaftaran anggota keluarga sah melalui ESS dan antrean verifikasi bukti fisik (KK, Akta Nikah, Akta Lahir, Surat Dekanat) oleh HR Benefit Officer.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Antrean Verifikasi HR** | Tabel pengajuan tanggungan menunggu verifikasi: NIP Staf, Nama Tanggungan, Hubungan (Pasangan/Anak), Usia, Lampiran Dokumen. | `GET /api/v1/corporate/hrd/benefits/dependents/pending-verification` | `HR_Benefit_Dependent:Verify` | "Semua pengajuan tanggungan telah diverifikasi (Antrean Bersih)". |
  | **Modal Verifikasi Berkas** | Penampil dokumen scan KK & Akta berdampingan dengan form data NIK, tombol "Setujui" dan "Tolak dengan Catatan". | `POST /api/v1/corporate/hrd/benefits/dependents/{id}/verify` | `HR_Benefit_Dependent:Verify` | Validasi: Jika ditolak, kolom alasan wajib diisi minimal 15 karakter. |
  | **ESS Portal: Keluarga Saya** | Antarmuka mandiri pegawai: Ringkasan kuota terpakai (Pasangan: 1/1, Anak: 2/3), tombol "Daftarkan Anggota Keluarga", indikator status verifikasi. | `GET /api/v1/corporate/hrd/benefits/dependents/my-family` | `ESS_Benefit:ManageFamily` | Munculkan tombol tambah tanggungan jika kuota anak < 3. |

#### 5. `FE-HRD-178`: In-House Hospital Care Coverage & Ledger Inquiry
- **Tujuan Layar:** Menyediakan visibilitas buku besar mutasi saldo subsidi plafon berobat di RS Quilvian sendiri, melayani pengecekan hak staf secara langsung dan pencatatan transaksi kasir SIMRS.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Inquiry Hak Berobat Online** | Kartu pencarian nakes via NIP/NIK: Menampilkan Status Aktif, Plafon Awal, Saldo Tersisa, % Diskon Staf, Hak Kelas Kamar. | `GET /api/v1/corporate/hrd/benefits/inhouse/coverage-inquiry` | `HR_Benefit_InHouse:View` | "Pegawai tidak ditemukan atau tidak berhak subsidi internal". |
  | **Buku Besar Mutasi Saldo** | Riwayat mutasi double-entry: Tanggal, Tipe Mutasi (Alokasi/Pemakaian/Reset), No. Referensi Registrasi SIMRS, Debit/Kredit, Saldo Akhir. | `GET /api/v1/corporate/hrd/benefits/inhouse/ledger/{employeeId}` | `HR_Benefit_InHouse:View` | "Belum ada riwayat mutasi saldo untuk pegawai ini". |
  | **Modal Penyesuaian Saldo Manual** | Form adjustment saldo plafon oleh HR Manajer disertai alasan resmi (misal: koreksi pembatalan transaksi berobat di SIMRS). | `POST /api/v1/corporate/hrd/benefits/inhouse/ledger/adjust` | `HR_Benefit_InHouse:Adjust` | Saldo setelah penyesuaian tidak boleh kurang dari nol. |

#### 6. `FE-HRD-179`: COB Healthcare Claim Submission & Settlement
- **Tujuan Layar:** Mengelola pengajuan klaim selisih biaya kesehatan (COB) penjamin sekunder asuransi swasta/RS berlampiran SEP BPJS Kesehatan, verifikasi kuitansi, dan settlement pembayaran.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Daftar Klaim COB Berjalan** | Tabel transaksi klaim: No. Klaim, Nama Nakes/Pasien, RS Perawat, No. SEP BPJS, Biaya Total, Porsi BPJS, Klaim Sekunder, Ekses, Status. | `GET /api/v1/corporate/hrd/benefits/claims` | `HR_Benefit_Claim:View` | "Tidak ada pengajuan klaim koordinasi manfaat aktif". |
  | **Formulir Pengajuan Klaim** | Input Nomor SEP BPJS (Mandatory), Kategori Layanan, Rincian Kwitansi Bermaterai, Unggah Dokumen Pendukung (PDF/JPG). | `POST /api/v1/corporate/hrd/benefits/claims` | `ESS_Benefit:SubmitClaim` | Validasi error jika `sep_number` kosong (`VR-BEN-16`). |
  | **Drawer Verifikasi & Settlement** | Panel persetujuan C&B Officer: Verifikasi keabsahan kuitansi selisih biaya, penetapan nominal yang ditanggung sekunder vs ekses tagihan mandiri. | `POST /api/v1/corporate/hrd/benefits/claims/{id}/settle` | `HR_Benefit_Claim:Settle` | Tombol settlement terkunci jika dokumen SEP belum terverifikasi. |

---

### 21.3 Komponen Modal & Drawer Pendukung

1. `ModalDependentFamilyVerification`: Dialog pop-up verifikasi berkas pendaftaran anggota keluarga ESS dengan fitur preview dokumen PDF dan pembanding data NIK terhadap Kartu Keluarga.
2. `ModalSepVerificationCheck`: Komponen pengecekan nomor Surat Eligibilitas Peserta (SEP) BPJS Kesehatan secara online guna memastikan klaim memenuhi ketentuan *Primary Payer*.
3. `ModalInHouseCoveragePreview`: Dialog simulasi perhitungan tagihan pasien nakes di loket kasir SIMRS, menampilkan breakdown diskon staf, potongan saldo plafon, dan estimasi ekses tunai.
4. `ModalCobSettlementDetail`: Panel ringkasan penyelesaian klaim COB yang memisahkan secara transparan antara porsi INA-CBGs BPJS, santunan asuransi swasta tambahan, dan ekses biaya yang didelegasikan ke pemotongan payroll Sub-Modul 11 (`S-B6`).


---

## 22. Desain Target Arsitektur Frontend Sub-Modul 13 (Slice S-D4 HR Service Management & Request — Layanan Administrasi, Helpdesk SDM & Penerbitan Dokumen Kepegawaian RS)

Bagian ini mendefinisikan arsitektur antarmuka pengguna (*frontend*) untuk **Sub-Modul 13 (HR Service Management & Request — Slice `S-D4`)**, dirancang menggunakan Next.js App Router, JavaScript/JSX modern, Redux Toolkit, Axios, serta standar antarmuka Quilvian Design Tokens (`@/components/features/base-features/base-form-control`, `BaseButton`, `StatusBadge`, `InformationAlert`).

Antarmuka ini mewadahi seluruh keputusan bisnis yang disetujui: portal verifikasi dokumen publik ber-QR Code dengan proteksi 4-digit PIN rahasia untuk pengungkapan nominal gaji (`HRD-DEC-140`), dasbor helpdesk antrean tiket dengan penegakan SLA 3-tier dan indikator countdown otomatis (`HRD-DEC-141`), otomasi penarikan data slip gaji final Sub-10 pada formulir surat keterangan penghasilan (`HRD-DEC-142`), formulir mandiri pemutakhiran data profil ESS dan verifikasi berkas pendukung (`HRD-DEC-143`), editor template dokumen terpusat berbasis token variabel dinamis (`HRD-DEC-144`), serta pusat pengarsipan digital abadi *Employee E-Dossier* dengan kemampuan pencabutan dokumen sah (*revocation*) (`HRD-DEC-145`).

---

### 22.1 Peta Butir Menu (Menu Items Map)

| Modul Induk / Navigasi | Butir Menu | Tingkat | Rute Halaman (Route URL) | Layar yang Dituju | Butir Hak Akses Penjaga |
| :--- | :--- | :---: | :--- | :--- | :--- |
| **Corporate HRD** | **Layanan & Helpdesk SDM** | 1 | — | *Grup Induk Navigasi* | `HR_Service:Access` |
| ├── Layanan & Helpdesk SDM | Antrean Tiket & SLA Dashboard | 2 | `/corporate/hrd/services/tickets` | `FE-HRD-180` | `HR_Service_Ticket:View` |
| ├── Layanan & Helpdesk SDM | Katalog Layanan & Template Surat | 2 | `/corporate/hrd/services/catalog` | `FE-HRD-181` | `HR_Service_Catalog:Manage` |
| └── Layanan & Helpdesk SDM | Pusat Penerbitan Dokumen Resmi | 2 | `/corporate/hrd/services/issuances` | `FE-HRD-184` | `HR_Document:Issue` |
| **Portal Mandiri Staf (ESS)** | **Layanan Kepegawaian** | 1 | — | *Grup Induk Navigasi ESS* | `ESS_Service:Access` |
| ├── Layanan Kepegawaian | Buat Tiket & Ajukan Surat | 2 | `/portal/services/requests` | `FE-HRD-182` | `ESS_Service:CreateRequest` |
| └── Layanan Kepegawaian | Dokumen & Arsip Saya | 2 | `/portal/services/my-documents` | `FE-HRD-186` | `ESS_Document:ViewMyArchive` |
| **Portal Eksternal Publik** | **Verifikasi Keaslian Dokumen** | Public | `/verify/document/[hash]` | `FE-HRD-185` | *Publik (Tanpa Sesi Login)* |

---

### 22.2 Skema Layar & Fitur Per Layar

#### 1. `FE-HRD-180`: Dasbor Antrean Helpdesk SDM & Pemantauan SLA 3-Tier
- **Tujuan Layar:** Menyediakan visibilitas terpusat bagi supervisor dan analis HR atas seluruh tiket permohonan staf rumah sakit yang sedang berjalan, lengkap dengan indikator sisa waktu SLA real-time, filter tier (Cito/Standar/Khusus), serta kartu metrik rapor keterlambatan.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Kartu Metrik SLA Operasional** | 4 Kartu KPI: Total Tiket Aktif, Tiket Cito Kritis (< 2 jam tersisa), Peringatan Kuning SLA (>= 75%), dan Tiket Overdue/Eskalasi. | `GET /api/v1/corporate/hrd/services/analytics/sla-summary` | `HR_Service_Ticket:View` | Nilai `0` jika seluruh tiket terselesaikan tepat waktu. |
  | **Bilah Filter & Prioritas** | Filter Kategori Layanan, Tingkat Prioritas (Semua, Cito Medis, Standar, Khusus), Status Tiket, dan Status SLA (WithinSla, Warning75, Overdue). | `GET /api/v1/corporate/hrd/services/categories` | `HR_Service_Ticket:View` | Reset filter ke default "Tiket Aktif". |
  | **Tabel Antrean Tiket Utama** | Kolom: No. Tiket, Tgl Masuk, Pemohon (Nama/NIP/Unit), Kategori/Jenis Layanan, Badge Prioritas (Merah: Cito, Biru: Standar, Ungu: Khusus), Countdown Timer SLA, Petugas Penanggung Jawab, Tombol Aksi (Ambil Tiket / Buka Detail). | `GET /api/v1/corporate/hrd/services/tickets` | `HR_Service_Ticket:View` | "Tidak ada tiket layanan dalam antrean aktif". |
  | **Modal Re-assign Tiket Cepat** | Dialog penugasan ulang analis SDM untuk tiket berstatus peringatan kuning atau overdue ke analis yang memiliki beban kerja lebih rendah. | `POST /api/v1/corporate/hrd/services/tickets/{id}/reassign` | `HR_Service_Ticket:Assign` | Menampilkan peringatan beban kerja analis terpilih. |

#### 2. `FE-HRD-181`: Manajemen Katalog Layanan & Editor Template Dokumen Token Dinamis
- **Tujuan Layar:** Mengonfigurasi master kategori layanan SDM, menetapkan target jam SLA per jenis layanan, menyusun template naskah dinas resmi berbasis token HTML/Liquid, serta mengunci pola penomoran surat seri anti-lompat (*gapless*).
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Master Kategori & Tipe Layanan** | Tabel hierarki kategori layanan SDM, konfigurasi batas jam SLA (4/24/72 jam), toggle prasyarat berkas wajib, dan toggle approval atasan. | `GET /api/v1/corporate/hrd/services/catalog/types` | `HR_Service_Catalog:Manage` | "Belum ada kategori layanan terdaftar". |
  | **Editor Template Dokumen Resmi** | Editor naskah dinas (Rich Text / HTML) dengan panel sisipan token variabel (`{{NamaLengkap}}`, `{{NIP}}`, `{{UnitKerja}}`, `{{GajiNetto}}`, `{{NomorSurat}}`). | `GET /api/v1/corporate/hrd/services/document-types/{id}` | `HR_Service_Catalog:Manage` | Token yang tidak dikenali akan diberi highlight merah peringatan. |
  | **Panel Penomoran & Proteksi Gaji** | Konfigurasi pola nomor surat (contoh: `[KODE]/RSQ-SDM/[BULAN_ROMAWI]/[TAHUN]/[NO_URUT]`), counter nomor terakhir, toggle `is_financial_sensitive`, dan masa berlaku hari. | `PUT /api/v1/corporate/hrd/services/document-types/{id}` | `HR_Service_Catalog:Manage` | Nomor counter tidak boleh diturunkan secara manual. |

#### 3. `FE-HRD-182`: Portal Permohonan Tiket & Layanan Dokumen Mandiri Staf (ESS)
- **Tujuan Layar:** Memfasilitasi dokter, perawat, dan staf umum mengajukan tiket bantuan SDM, meminta penerbitan surat resmi (SKK, KPR, Visa), atau mengajukan perubahan rekening bank payroll secara mandiri.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Katalog Pilihan Layanan (Tiles)** | Grid kartu pilihan layanan populer: "Minta Surat Keterangan Kerja", "Surat Keterangan Gaji (KPR/Bank)", "Ubah Rekening Bank", "Izin Tugas Klinis Cito". | `GET /api/v1/corporate/hrd/services/catalog/ess-tiles` | `ESS_Service:CreateRequest` | Memuat ikon dan estimasi durasi SLA per layanan. |
  | **Wizard Pengajuan Tiket Interaktif** | Form dinamis 3 langkah: (1) Detail Permohonan & Keperluan; (2) Unggah Dokumen Persyaratan (Drag & drop PDF/JPG maks 5MB); (3) Konfirmasi & Persetujuan Pernyataan. | `POST /api/v1/corporate/hrd/services/requests` | `ESS_Service:CreateRequest` | Validasi error jika berkas wajib belum diunggah (`VR-HRS-01`). |
  | **Banner Peringatan Prioritas Cito** | Alert kuning tegas: "Layanan Cito (SLA 4 Jam) hanya untuk keperluan darurat penugasan klinis dan wajib menyertakan surat disposisi pimpinan". | — | — | Tampil otomatis saat pemohon memilih prioritas Cito. |
  | **Tabel Riwayat Tiket Saya** | Daftar tiket pemohon: No. Tiket, Judul, Kategori, Tanggal Pengajuan, Status Tiket (Submitted/InReview/WaitingStaffResponse/Resolved), Tombol Buka Percakapan. | `GET /api/v1/corporate/hrd/services/requests/my-tickets` | `ESS_Service:CreateRequest` | "Anda belum pernah mengajukan tiket layanan". |

#### 4. `FE-HRD-183`: Drawer Pemrosesan Tiket, Verifikasi Berkas & Utas Klarifikasi Staf
- **Tujuan Layar:** Panel drawer interaktif bagi analis HR untuk memeriksa rincian permohonan staf, memverifikasi dokumen bukti, berkomunikasi langsung melalui utas pesan klarifikasi, dan mengeksekusi penyelesaian tiket.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Header Tiket & Indikator SLA** | Info Nomor Tiket, Profil Singkat Staf Pemohon (Foto, NIP, Unit, Jabatan), Countdown Timer SLA, dan Tombol Status (Ambil Alih / Serahkan). | `GET /api/v1/corporate/hrd/services/tickets/{id}` | `HR_Service_Ticket:Process` | Timer menampilkan warna merah berkedip bila Overdue. |
  | **Penampil Dokumen Lampiran (Preview)** | Embedded viewer untuk memeriksa berkas scan (KTP, Buku Tabungan, KK, Surat Disposisi Cito) berdampingan dengan form data. | `GET /api/v1/corporate/hrd/services/tickets/{id}/attachments` | `HR_Service_Ticket:Process` | Tautan unduh alternatif jika browser tidak mendukung preview. |
  | **Utas Komunikasi Klarifikasi (Thread)** | Riwayat obrolan dua arah staf dan analis HR, kotak input balasan, toggle "Catatan Internal HR (Hanya Terlihat Tim SDM)", tombol minta perbaikan berkas. | `POST /api/v1/corporate/hrd/services/tickets/{id}/comments` | `HR_Service_Ticket:Process` | Status otomatis menjadi `WaitingStaffResponse` saat pesan minta klarifikasi dikirim. |
  | **Bilah Aksi Penyelesaian Tiket** | Tombol tindakan: "Setujui & Mutakhirkan Data Profil", "Loloskan ke Draf Surat Resmi", atau "Tolak Permohonan Disertai Alasan". | `POST /api/v1/corporate/hrd/services/tickets/{id}/resolve` | `HR_Service_Ticket:Process` | Dialog konfirmasi ringkasan dampak perubahan. |

#### 5. `FE-HRD-184`: Modal Otorisasi Surat Resmi, Penomoran Otomatis & QR Code Generator
- **Tujuan Layar:** Antarmuka otorisasi pejabat berwenang (Supervisor C&B / Manajer SDM) untuk memeriksa draf surat naskah dinas, memeriksa penarikan nominal gaji otomatis dari Sub-10, membubuhi tanda tangan digital, menerbitkan nomor seri gapless, dan merilis berkas PDF ber-QR Code.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Pratinjau Draf Dokumen Resmi** | Lembar visual naskah dinas ber-kop surat resmi RS Quilvian, teks isi berparameter token yang telah digantikan data asli pemohon. | `GET /api/v1/corporate/hrd/services/document-requests/{id}/preview` | `HR_Document:Issue` | "Gagal memuat pratinjau draf dokumen". |
  | **Panel Verifikasi Data Finansial (C&B)** | Rincian data gaji yang ditarik otomatis dari Sub-10: Gaji Pokok, Tunjangan Tetap, Potongan Wajib, Gaji Netto, Periode Slip Gaji Terakhir. | `GET /api/v1/corporate/hrd/services/document-requests/{id}/salary-source` | `HR_Document:Issue` | Terkunci read-only; dilarang mengedit nominal di form (`HRD-DEC-142`). |
  | **Aksi Otorisasi & Penerbitan** | Input Tanda Tangan Digital / PIN Otorisasi Pejabat, Tombol "Terbitkan Dokumen Sah (Generate QR & Nomor Seri)", Tombol "Tolak Draf". | `POST /api/v1/corporate/hrd/services/document-requests/{id}/authorize` | `HR_Document:Issue` | Sistem mengunci nomor urut secara atomik dan menghasilkan URL hash unik. |
  | **Modal Pencabutan Dokumen (Revokasi)** | Form pembatalan surat terbit: Input Nomor Berita Acara (BAP), Alasan Pencabutan Resmi (min 20 karakter), konfirmasi Manajer SDM. | `POST /api/v1/corporate/hrd/services/documents/{id}/revoke` | `HR_Document:Revoke` | Peringatan tegas: "Tindakan ini tidak dapat dibatalkan dan akan langsung memicu peringatan merah di portal publik". |

#### 6. `FE-HRD-185`: Portal Verifikasi Dokumen Publik Ber-QR Code & Form Pembukaan PIN Gaji (`/verify/document/[hash]`)
- **Tujuan Layar:** Halaman publik responsif (diakses tanpa login oleh pihak bank, kedutaan, universitas, atau lembaga pemerintah melalui scan QR Code dokumen fisik) untuk memvalidasi keaslian surat kepegawaian RS Quilvian.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Banner Status Keabsahan Surat** | Banner status berukuran besar: Hijau ("DOKUMEN RESMI SAH & BERLAKU"), Oranye ("MASA BERLAKU TELAH HABIS / EXPIRED"), atau Merah ("DOKUMEN TELAH DICABUT / REVOKED"). | `GET /api/v1/public/documents/verify/{hash}` | *Publik* | "Dokumen tidak ditemukan atau kode QR tidak valid". |
  | **Metadata Keabsahan Naskah Dinas** | Rincian legalitas: Nomor Surat Resmi, Tanggal Terbit, Masa Berlaku, Nama Lengkap Staf, Jabatan & Unit Kerja, Pejabat Penandatangan Sah. | `GET /api/v1/public/documents/verify/{hash}` | *Publik* | Data tidak dapat dimanipulasi oleh pihak ketiga. |
  | **Wilayah Proteksi Informasi Gaji** | Jika surat finansial: Teks nominal disamarkan (`Rp ••••••••••`), kotak dialog input 4-digit PIN rahasia staf, tombol "Buka Rincian Penghasilan". | `POST /api/v1/public/documents/verify/{hash}/reveal-salary` | *Publik* | "PIN salah. Sisa percobaan: X kali sebelum dikunci sementara". |
  | **Banner Revokasi Khusus** | Menampilkan tanggal pencabutan, nomor surat pembatalan BAP, dan alasan resmi pembatalan bila status dokumen adalah `Revoked`. | `GET /api/v1/public/documents/verify/{hash}` | *Publik* | Tampil tegas di bagian paling atas halaman. |

#### 7. `FE-HRD-186`: Pusat Arsip Digital Kepegawaian (Employee E-Dossier & Riwayat Dokumen ESS)
- **Tujuan Layar:** Repositori arsip mandiri pegawai di portal ESS untuk melihat, mengunduh ulang salinan dokumen PDF resmi yang pernah diterbitkan sistem, serta memantau status keaktifan dokumen.
- **Tabel Wilayah Layar:**
  | Wilayah Layar | Komponen & Data yang Ditampilkan | Endpoint API Sumber | Hak Akses | Status Kosong / Gagal |
  | :--- | :--- | :--- | :--- | :--- |
  | **Koleksi Dokumen Resmi Saya** | Grid/Tabel arsip surat: Jenis Surat, Nomor Surat, Tanggal Diterbitkan, Masa Berlaku, Status (Aktif/Expired/Revoked), Tombol "Unduh PDF Resmi". | `GET /api/v1/corporate/hrd/services/documents/my-archive` | `ESS_Document:ViewMyArchive` | "Belum ada dokumen resmi yang diterbitkan untuk akun Anda". |
  | **Kartu PIN Rahasia Dokumen Finansial** | Tombol aman "Lihat PIN Rahasia Surat KPR": Menampilkan 4-digit PIN dengan proteksi verifikasi kata sandi ESS pegawai. | `GET /api/v1/corporate/hrd/services/documents/{id}/my-pin` | `ESS_Document:ViewMyArchive` | PIN disembunyikan secara default untuk mencegah intipan bahu (*shoulder surfing*). |
  | **Riwayat Permohonan Pemutakhiran Data** | Tabel rekam jejak tiket perubahan profil: Tanggal, Komponen Diubah (Rekening Bank / Status Nikah), Data Sebelum, Data Sesudah, Dokumen Pendukung. | `GET /api/v1/corporate/hrd/services/requests/my-profile-history` | `ESS_Document:ViewMyArchive` | "Belum ada riwayat permohonan pemutakhiran data profil". |

---

### 22.3 Komponen Modal, Drawer & Redux State Slice

1. `DrawerTicketProcessing`: Panel drawer sisi kanan untuk investigasi tiket, preview lampiran scan berkas berdampingan dengan formulir data, dan utas klarifikasi staf secara langsung.
2. `ModalDocumentAuthorization`: Dialog konfirmasi penerbitan surat resmi dengan live preview dokumen naskah dinas, penarikan gaji otomatis, dan generator hash QR Code.
3. `ModalDocumentRevocation`: Dialog pencatatan Berita Acara Pemeriksaan (BAP) untuk mencabut keabsahan dokumen kepegawaian sah.
4. `ModalPinRevealDialog`: Komponen dialog portal publik untuk memasukkan 4-digit PIN rahasia guna membuka nominal slip gaji pada surat keterangan penghasilan.
5. **Redux Toolkit Slice (`hrServiceSlice.js`):**
   - Mengelola state global antrean tiket, filter prioritas SLA, penugasan analis aktif, serta status caching katalog layanan dan template dokumen resmi.


---

## 23. Arsitektur Frontend Sub-Modul 14: Credentialing, License & Clinical Privilege (Slice `S-C1`, Kontrak `v18`)

Bagian ini mendefinisikan rancangan antarmuka pengguna (Next.js App Router, React, Redux Toolkit, Tailwind CSS, Quilvian Base Components) untuk **Sub-Modul 14 (Slice `S-C1`)** yang mencakup portal pengajuan kredensial tenaga medis, verifikasi keabsahan primer (PSV) STR/SIP, meja kerja penilaian panel Mitra Bestari (*Peer Review*), sidang pleno 3 komite mandiri, konsol pengesahan SPK/RKK Direktur Utama, registri kewenangan klinis aktif, konsol penugasan cepat darurat & sementara, serta radar siklus rekredensialing 3 tahunan sesuai [`HRD-DEC-146`](00-interview-decisions.md#L7165-L7185) s.d. [`HRD-DEC-151`](00-interview-decisions.md#L7250-L7270).

---

### 23.1 Peta Butir Menu (Menu Items Map)

| Modul Induk / Navigasi | Butir Menu | Tingkat | Rute Halaman (Route URL) | Layar yang Dituju | Butir Hak Akses Penjaga |
| :--- | :--- | :---: | :--- | :--- | :--- |
| **Self-Service Klinis** | **Kredensial & Kewenangan Saya** | 1 | — | *Grup Navigasi Staf Klinis* | `Clinical_Staff:Access` |
| ├── Kredensial & RKK Saya | Pengajuan Kredensial & Whitebook | 2 | `/self-service/credentials/apply` | `FE-HRD-187` | `Credential_Application:Create` |
| ├── Kredensial & RKK Saya | Dokumen SPK & RKK Aktif | 2 | `/self-service/credentials/my-privileges` | `FE-HRD-187` *(Tab 2)* | `Clinical_Privilege:View` |
| **Tata Kelola Klinis RS** | **Kredensialing & Tata Kelola Profesi** | 1 | — | *Grup Induk Tata Kelola* | `Credential_Governance:Access` |
| ├── Kredensialing & Profesi | Verifikasi PSV & Radar SIP 5 Tahun | 2 | `/corporate/hrd/credentialing/psv-verification` | `FE-HRD-188` | `Credential_Verification:Execute` |
| ├── Kredensialing & Profesi | Meja Asesmen Mitra Bestari (Peer Review) | 2 | `/corporate/hrd/credentialing/peer-review` | `FE-HRD-189` | `Peer_Review:Assess` |
| ├── Kredensialing & Profesi | Sidang Pleno Komite Kredensial | 2 | `/corporate/hrd/credentialing/committee-plenary` | `FE-HRD-190` | `Committee_Review:Approve` |
| ├── Kredensialing & Profesi | Konsol Pengesahan SPK-RKK Direktur Utama | 2 | `/corporate/hrd/credentialing/director-approval` | `FE-HRD-191` | `Spk_Rkk:SignDirector` |
| ├── Kredensialing & Profesi | Registri Kewenangan Klinis & Audit SIMRS | 2 | `/corporate/hrd/credentialing/clinical-privilege-registry` | `FE-HRD-192` | `Clinical_Privilege:Manage` |
| ├── Kredensialing & Profesi | Penugasan Cepat (Temporary & Emergency) | 2 | `/corporate/hrd/credentialing/fast-track-privilege` | `FE-HRD-193` | `FastTrack_Privilege:Grant` |
| └── Kredensialing & Profesi | Radar Rekredensialing 3 Tahun & OPPE | 2 | `/corporate/hrd/credentialing/recredentialing-cycle` | `FE-HRD-194` | `Recredentialing:Monitor` |

---

### 23.2 Skema Layar & Fitur Per Layar (`FE-HRD-187` s.d. `FE-HRD-194`)

#### 23.2.1 `FE-HRD-187` — Portal Kredensial Tenaga Medis (Pengajuan Berkas & Checklist Whitebook)

- **Deskripsi:** Layar portal self-service bagi tenaga medis/keperawatan/nakes lain untuk mengajukan berkas kredensial baru, mengunggah STR seumur hidup, SIP, sertifikat kompetensi, serta mengisi checklist butir rincian kewenangan klinis (*whitebook*) yang dimohonkan.
- **Komponen Utama:**
  - Tab Navigasi: (1) Permohonan Baru / Peningkatan Jenjang, (2) Dokumen SPK & RKK Berjalan Saya.
  - Form Wizard: Langkah 1 Identitas & Lisensi, Langkah 2 Unggah Berkas & Portofolio Logbook, Langkah 3 Checklist Butir Whitebook (Pilihan: Mandiri / Supervisi), Langkah 4 Pernyataan Etika & Submit.
  - Komponen Pemilih Komite Otomatis: Terkunci otomatis berdasarkan profesi pegawai (Komite Medik / Keperawatan / Nakes Lain).
- **Endpoint Data:**
  - `GET /api/v1/self-service/credentials/my-licenses`
  - `GET /api/v1/corporate/hrd/credentialing/whitebooks/catalog?committeeCode={code}`
  - `POST /api/v1/corporate/hrd/credentialing/applications`

#### 23.2.2 `FE-HRD-188` — Dashboard Verifikasi PSV Staf Kredensialing & Radar SIP 5 Tahun

- **Deskripsi:** Meja kerja sekretariat komite untuk melakukan validasi sumber primer (*Primary Source Verification* - PSV) STR ke SatuSehat SDMK / KKI, pencatatan bukti audit verifikasi digital, serta radar pemantau kedaluwarsa SIP 5 tahunan berjenjang (H-90 s.d. H-0).
- **Komponen Utama:**
  - KPI Card: Total Berkas Masuk, Menunggu PSV, SIP Kedaluwarsa < 90 Hari, SIP Kedaluwarsa < 30 Hari, Lisensi Suspended H-0.
  - Filter Cerdas: Komite Profesi, Status Verifikasi (`PendingPSV`, `Verified`, `Rejected`), Rentang Kedaluwarsa SIP.
  - Action Drawer Verifikasi: Tautan langsung ke portal SatuSehat SDMK / KKI, tombol konfirmasi keabsahan dokumen, input nomor verifikasi Kemenkes, dan unggah tangkapan layar bukti PSV.
- **Endpoint Data:**
  - `GET /api/v1/corporate/hrd/credentialing/licenses/pending-psv`
  - `POST /api/v1/corporate/hrd/credentialing/licenses/{id}/verify-psv`
  - `GET /api/v1/corporate/hrd/credentialing/licenses/expiration-radar`

#### 23.2.3 `FE-HRD-189` — Meja Kerja Asesmen Mitra Bestari (*Peer Review Desk*)

- **Deskripsi:** Layar khusus bagi anggota panel Mitra Bestari (*Peer Reviewers*) untuk menelaah logbook pengalaman klinis dokter/nakes, memberikan rekomendasi derajat wewenang (Level 4 Mandiri, Level 3 Supervisi, Level 1-2 Ditolak), dan menandatangani berita acara rekomendasi secara digital.
- **Komponen Utama:**
  - Ringkasan Berkas Pemohon: Data nakes, rumpun spesialisasi, resume logbook kasus, riwayat pelatihan kompetensi.
  - Checklist Interaktif Whitebook: Menampilkan rincian prosedur tindakan klinis; kolom radio button rekomendasi per butir: [ ] Mandiri (4) [ ] Supervisi (3) [ ] Ditolak (1-2).
  - Kolom Catatan Rekomendasi: Alasan objektif jika ditolak atau wajib di bawah supervisi.
  - Indikator Status Peer Review: Menampilkan status 2 reviewer sebidang (Reviewer 1: Selesai, Reviewer 2: Sedang Mereview).
- **Endpoint Data:**
  - `GET /api/v1/corporate/hrd/credentialing/peer-reviews/assigned-to-me`
  - `POST /api/v1/corporate/hrd/credentialing/peer-reviews/{id}/submit-assessment`

#### 23.2.4 `FE-HRD-190` — Sidang Pleno Komite Kredensial (Komite Medik / Keperawatan / Nakes Lain)

- **Deskripsi:** Layar sidang pleno pengurus komite kredensial untuk menelaah konsolidasi penilaian dari panel Mitra Bestari, memutuskan persetujuan kolektif, dan menerbitkan Surat Rekomendasi Ketua Komite ke Direktur Pelayanan & Direktur Utama.
- **Komponen Utama:**
  - Tab 3 Komite: Komite Medik, Komite Keperawatan, Komite Nakes Lainnya.
  - Matriks Konsensus Reviewer: Menyorot secara visual perbedaan skor rekomendasi antara Reviewer 1 dan Reviewer 2 untuk disidangkan.
  - Tombol Aksi Sidang: [Setujui Sesuai Peer Review], [Ubah Derajat Tindakan Tertentu Berdasarkan Pleno], [Kembalikan untuk Remedial Portofolio].
  - Generate Surat Rekomendasi: Menghasilkan naskah resmi rekomendasi Ketua Komite bertanda tangan elektronik.
- **Endpoint Data:**
  - `GET /api/v1/corporate/hrd/credentialing/committee-reviews/plenary-docket`
  - `POST /api/v1/corporate/hrd/credentialing/committee-reviews/{id}/finalize-recommendation`

#### 23.2.5 `FE-HRD-191` — Konsol Pengesahan SPK & RKK Direktur Utama (*Director Approval Console*)

- **Deskripsi:** Layar otorisasi eksekutif khusus Direktur Utama RS (didahului paraf Direktur Pelayanan Medik/Keperawatan) untuk mengesahkan penerbitan Surat Penugasan Klinis (SPK) dan Rincian Kewenangan Klinis (RKK).
- **Komponen Utama:**
  - Live PDF Previewer: Menampilkan naskah resmi SPK dan lampiran tabel RKK berstempel draft digital.
  - Panel Penetapan Supervisor: Menampilkan nama dokter supervisor penanggung jawab untuk setiap butir tindakan Level 3.
  - Otorisasi e-Sign Direktur Utama: Verifikasi kredensial tanda tangan elektronik bersertifikat / PIN otorisasi direktur.
  - Penerbitan Dokumen Sah: Generate QR Code verifikasi keabsahan publik dan pencatatan nomor register SPK/RKK gapless.
- **Endpoint Data:**
  - `GET /api/v1/corporate/hrd/credentialing/spk-issuances/pending-director-signature`
  - `POST /api/v1/corporate/hrd/credentialing/spk-issuances/{id}/sign-and-issue`

#### 23.2.6 `FE-HRD-192` — Registri Kewenangan Klinis Rumah Sakit & Audit Gate SIMRS EMR

- **Deskripsi:** Direktori pencarian wewenang klinis seluruh nakes rumah sakit yang dapat diakses oleh kepala unit, IGD, OK Bedah, Farmasi, serta log audit penegakan batas wewenang (*Hard-Stop*) dan bypass darurat (*Life-Saving Override*).
- **Komponen Utama:**
  - Mesin Pencari Instan: Cari berdasarkan Nama Dokter, NIP, Spesialisasi, atau Nama Tindakan Medis spesifik.
  - Rincian Hak Klinis Nakes: Daftar tindakan mandiri vs tindakan yang wajib didampingi supervisor.
  - Tab Audit Override Darurat: Menampilkan daftar eksekusi tombol bypass penyelamatan nyawa di SIMRS/EMR, alasan medis darurat, nama dokter pemohon, serta status audit oleh KMKP (*Komite Mutu & Keselamatan Pasien*).
- **Endpoint Data:**
  - `GET /api/v1/corporate/hrd/credentialing/privileges/registry`
  - `GET /api/v1/corporate/hrd/credentialing/privileges/emergency-override-audit-logs`
  - `POST /api/v1/corporate/hrd/credentialing/privileges/emergency-override/{id}/audit-verdict`

#### 23.2.7 `FE-HRD-193` — Konsol Penugasan Cepat (Temporary & Emergency Privilege)

- **Deskripsi:** Layar gerbang penugasan cepat klinis khusus dokter konsultan tamu dari luar RS (*Temporary Privilege* maks 3 bulan) atau penugasan darurat cito bencana massal (*Emergency Privilege* maks 72 jam) yang disahkan langsung oleh Direktur Pelayanan Medik.
- **Komponen Utama:**
  - Formulir Cepat Dokter Tamu: Input data STR faskes asal, institusi asal, dokter pendamping lokal RS, durasi tugas (maks 3 bulan).
  - Saklar Darurat Cito Kebencanaan: Otorisasi instan dokter relawan bencana dengan durasi maksimal 72 jam terbatas pada tindakan life-saving.
  - Badge Status Waktu: Timer hitung mundur sisa masa berlaku jam kewenangan darurat.
- **Endpoint Data:**
  - `POST /api/v1/corporate/hrd/credentialing/fast-track/temporary-privilege`
  - `POST /api/v1/corporate/hrd/credentialing/fast-track/emergency-privilege`

#### 23.2.8 `FE-HRD-194` — Monitoring Rekredensialing Berkala & Radar Suspended (Siklus 3 Tahun Sub-15)

- **Deskripsi:** Layar kontrol siklus 3 tahunan evaluasi kewenangan klinis rumah sakit, peringatan dini nakes yang memasuki masa rekredensialing, integrasi nilai kinerja klinis OPPE dari Sub-Modul 15, dan pemantauan nakes berstatus suspended.
- **Komponen Utama:**
  - Radar Timeline Siklus 3 Tahun: Visualisasi distribusi masa berlaku SPK seluruh staf medis rumah sakit (Tahun 1, Tahun 2, Tahun 3).
  - Antrean Berkas Rekredensialing: Menampilkan draf otomatis yang digenerate sistem pada H-60 beserta agregasi skor OPPE Sub-15.
  - Daftar Suspended Nakes: Daftar nakes yang izin pelayanannya ditangguhkan akibat belum selesai rekredensialing pada H-0 atau pelanggaran etik.
- **Endpoint Data:**
  - `GET /api/v1/corporate/hrd/credentialing/recredentialing/cycle-status`
  - `POST /api/v1/corporate/hrd/credentialing/recredentialing/initiate-batch`
  - `POST /api/v1/corporate/hrd/credentialing/privileges/{id}/suspend`

---

### 23.3 Komponen Modal, Drawer & Redux State Slice

#### 23.3.1 Komponen Modal & Drawer Pendukung

1. `PrimarySourceVerificationDrawer`: Drawer samping untuk memverifikasi dokumen STR ke portal SatuSehat SDMK / KKI, lengkap dengan validasi status seumur hidup dan pencatatan URL bukti audit.
2. `PeerReviewScoringModal`: Modal dialog interaktif untuk menilai butir prosedur klinis dan menetapkan derajat rekomendasi (Mandiri vs Supervisi).
3. `SpkDirectorSignatureModal`: Modal pengesahan dokumen SPK/RKK dengan otentikasi e-Sign Direktur Utama RS.
4. `EmergencyOverrideDetailModal`: Modal pemeriksaan rekam audit bypass penyelamatan nyawa SIMRS oleh komite mutu KMKP.
5. `TemporaryPrivilegeGrantModal`: Modal penerbitan kewenangan sementara bagi dokter spesialis tamu dari luar kota.

#### 23.3.2 Redux State Slice (`credentialingSlice.ts`)

```typescript
interface CredentialingState {
  myLicenses: WfpCredentialLicenseDto[];
  activePrivileges: WfpClinicalPrivilegeDto[];
  pendingPsvLicenses: PagedResult<WfpCredentialLicenseDto>;
  assignedPeerReviews: PeerReviewDocketDto[];
  plenaryApplications: PagedResult<CredentialingApplicationDto>;
  pendingDirectorSignatures: SpkRkkIssuanceDto[];
  overrideAuditLogs: PagedResult<EmergencyOverrideLogDto>;
  recredentialingCycles: RecredentialingCycleSummaryDto[];
  isLoading: boolean;
  error: string | null;
}
```

---

## 24. Arsitektur Frontend Sub-Modul 15: OPPE, FPPE & Recredentialing (`S-C1-OPPE`)

### 24.1 Struktur Menu & Navigasi

Sub-modul OPPE, FPPE & Recredentialing terintegrasi dalam kelompok menu portal tenaga medis dan portal manajemen Komite Medik / KMKP:

```text
Portal Tenaga Medis & Komite Medik
└── Kinerja & Mutu Klinis (Clinical Performance & Quality)
    ├── [FE-HRD-201] Dashboard Kinerja Klinis & Radar OPPE (/portal/performance/clinical-oppe)
    ├── [FE-HRD-202] Konfigurasi Periode & Matriks OPPE KSM (/admin/performance/oppe-periods)
    ├── [FE-HRD-203] Evaluasi Kualitatif Peer Review Sejawat (/portal/performance/peer-review-entry)
    ├── [FE-HRD-204] Pusat Deteksi Pemicu & Kasus FPPE (/admin/performance/fppe-triggers)
    ├── [FE-HRD-205] Pemantauan Proctoring Staf & Prosedur Baru (/admin/performance/fppe-proctoring)
    ├── [FE-HRD-206] Sidang Kesimpulan & Rekomendasi FPPE (/admin/performance/fppe-conclusions)
    ├── [FE-HRD-207] Kalkulator Rapor 3 Tahun Rekredensialing (/admin/credentialing/recred-composite-scorecard)
    └── [FE-HRD-208] Konsol Sinkronisasi Putusan ke SIMRS EMR (/admin/credentialing/recred-simrs-sync)
```

---

### 24.2 Spesifikasi Layar Antarmuka Pengguna

#### 24.2.1 [FE-HRD-201] Dashboard Kinerja Klinis & Radar OPPE Dokter Spesialis
- **Rute:** `/portal/performance/clinical-oppe`
- **Tujuan:** Menyajikan visualisasi komprehensif performa praktik dokter spesialis secara mandiri dan bagi Ketua KSM/Komite Medik.
- **Elemen Tampilan:**
  - Kartu Ringkasan Status OPPE Tahunan (Nilai Komposit, Status Lulus Evaluasi, Status Peringatan Dini).
  - Radar Chart 6 Area Kompetensi Standar ACGME/KARS:
    1. *Patient Care* (Tingkat Keberhasilan Klinis & Keselamatan Pasien).
    2. *Medical Knowledge* (Partisipasi CPD/SKP, Kesesuaian Diagnosis).
    3. *Practice-Based Learning* (Kepatuhan Clinical Pathway & Audit Kasus).
    4. *Interpersonal & Communication Skills* (Keluhan Pasien & Komunikasi SBAR).
    5. *Professionalism* (Etika Profesi & Kehadiran Tugas Jaga/Visite).
    6. *Systems-Based Practice* (Kepatuhan Formularium Nasional/RS & Efisiensi Utilisasi).
  - Breakdown Nilai 60% Kuantitatif (SIMRS/KMKP) dan 40% Kualitatif (Peer Review).
  - Tabel Riwayat Evaluasi Periode Sebelumnya beserta catatan rekomendasi pembinaan.
- **Interaksi:**
  - Filter tahun evaluasi dan jenis spesialisasi.
  - Unduh Rapor Ringkasan OPPE resmi bertanda tangan digital Komite Medik (format PDF).
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/clinical-performance/oppe/staff-evaluations/my-summary`
  - `GET /api/v1/corporate/hrd/clinical-performance/oppe/staff-evaluations/{id}/radar`

#### 24.2.2 [FE-HRD-202] Konfigurasi Periode & Matriks Indikator OPPE KSM
- **Rute:** `/admin/performance/oppe-periods`
- **Tujuan:** Fasilitas bagi Subkomite Mutu Profesi untuk membuka periode evaluasi tahunan/semesteran dan mengatur matriks indikator per KSM.
- **Elemen Tampilan:**
  - Tabel Daftar Periode OPPE (Tahun, Semester, Unit/KSM Sasaran, Batas Waktu, Status: *Draft*, *Active*, *EvaluationClosed*, *Finalized*).
  - Panel Konfigurasi Bobot Indikator KSM (Bobot Kuantitatif SIMRS vs Kualitatif Peer Review, default 60:40).
  - Master Katalog Indikator OPPE (Kode Indikator, Pilar ACGME, Sumber Data: SIMRS/Manual, Ambang Batas Target, Bobot %).
- **Interaksi:**
  - Tombol aksi: "Buka Periode Evaluasi Baru", "Kunci Penilaian Periode", "Tutup & Finalisasi Rapor".
  - Modal `OppeIndicatorConfigModal` untuk memodifikasi target spesifik KSM (misal: rasio IDO Bedah < 1.5%, keterlambatan resume EMR < 5%).
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/clinical-performance/oppe/periods`
  - `POST /api/v1/corporate/hrd/clinical-performance/oppe/periods`
  - `GET /api/v1/corporate/hrd/clinical-performance/oppe/metric-catalogs`
  - `PUT /api/v1/corporate/hrd/clinical-performance/oppe/metric-catalogs/{id}`

#### 24.2.3 [FE-HRD-203] Form Evaluasi Kualitatif Peer Review Sejawat KSM
- **Rute:** `/portal/performance/peer-review-entry`
- **Tujuan:** Lembar penilaian sejawat konfidensial oleh sesama dokter spesialis dalam KSM yang sama.
- **Elemen Tampilan:**
  - Banner Peringatan Legal Hak Istimewa Konfidensialitas (*Peer Review Privilege*). Penegasan bahwa data dilindungi undang-undang dan tidak dapat dibuka kepada publik.
  - Daftar Dokter yang Ditugaskan untuk Ditinjau (Anonim atau Berkode Reviewer).
  - Rubrik Penilaian Etika Profesi, Komunikasi Multidisiplin, Kehadiran Konferensi Kasus Sulit/M&M, dan Kedisiplinan Visite Pasien.
  - Kolom Narasi Kualitatif untuk catatan pengamatan perilaku profesional dan rekomendasi pembinaan.
- **Interaksi:**
  - Form validasi skor likert (1 s.d. 5) dan catatan wajib jika skor <= 2.
  - Tombol "Simpan Draf" dan "Submit Penilaian Sejawat (Final)".
  - Sebelum membuka detail ulasan sejawat, sistem memicu modal konfirmasi kerahasiaan dan mencatat audit pembacaan.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/clinical-performance/oppe/peer-reviews/assigned`
  - `POST /api/v1/corporate/hrd/clinical-performance/oppe/peer-reviews/{id}/submit`
  - `POST /api/v1/corporate/hrd/clinical-performance/peer-review-audit/log-access`

#### 24.2.4 [FE-HRD-204] Pusat Deteksi Pemicu Kritis & Pelacakan Kasus FPPE
- **Rute:** `/admin/performance/fppe-triggers`
- **Tujuan:** Dashboard monitoring pemicu insiden keselamatan pasien dan inisiasi evaluasi praktik profesional terfokus (FPPE).
- **Elemen Tampilan:**
  - Feed Pemicu Kritis Real-Time (Sentinel Event dari SIMRS/KMKP, Komplikasi Bedah > 2 SD, Keterlambatan Resume EMR > 48 jam > 20%, Surat Rujukan Etik Subkomite).
  - Status Indikator Bahaya (Merah: Membutuhkan Inisiasi FPPE Segera, Kuning: Pemantauan Intensif).
  - Tabel Kasus FPPE Berjalan (Nomor Kasus, Nama Dokter, KSM, Kategori Pemicu, Status Proctoring, Tanggal Mulai).
- **Interaksi:**
  - Tombol "Aktivasi Penugasan FPPE" dengan memilih pemicu, menunjuk Ketua Tim Evaluasi/Proctor, dan menetapkan durasi pemantauan.
  - Filter pencarian berdasarkan KSM, tipe pemicu, dan rentang tanggal insiden.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/clinical-performance/fppe/triggers/active`
  - `POST /api/v1/corporate/hrd/clinical-performance/fppe/engagements`
  - `GET /api/v1/corporate/hrd/clinical-performance/fppe/engagements`

#### 24.2.5 [FE-HRD-205] Pemantauan Proctoring Staf Baru & Prosedur Baru
- **Rute:** `/admin/performance/fppe-proctoring`
- **Tujuan:** Pemantauan kemajuan logbook kasus klinis yang disupervisi oleh proctor bagi staf baru (3 bulan / 10 kasus) atau prosedur baru (6 bulan / 5 kasus).
- **Elemen Tampilan:**
  - Kartu Metrik Kemajuan Kasus (Kasus Tercapai vs Target, misal: 7/10 Kasus).
  - Kartu Waktu Pemantauan Tersisa (Hari tersisa dalam masa observasi proctoring).
  - Logbook Kasus Terproctoring (Nomor RM Pasien Terpseudonimisasi, Nama Tindakan Medis, Tanggal Tindakan, Nama Proctor Pendamping, Status Verifikasi, Derajat Kemandirian Klinis).
- **Interaksi:**
  - Entri logbook kasus oleh dokter melalui modal `FppeCaseLogEntryModal`.
  - Verifikasi dan persetujuan kemandirian prosedur oleh Dokter Proctor pendamping.
  - Alert visual jika batas waktu pemantauan mendekati akhir namun target kasus belum terpenuhi.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}/case-logs`
  - `POST /api/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}/case-logs`
  - `PUT /api/v1/corporate/hrd/clinical-performance/fppe/case-logs/{logId}/verify`

#### 24.2.6 [FE-HRD-206] Sidang Kesimpulan & Rekomendasi Wewenang FPPE
- **Rute:** `/admin/performance/fppe-conclusions`
- **Tujuan:** Lembar kerja Subkomite Mutu Profesi untuk menetapkan kesimpulan hasil evaluasi praktik terfokus dan rekomendasi kelanjutan wewenang klinis.
- **Elemen Tampilan:**
  - Ringkasan Laporan Proctoring Kasus Klinis dan Rekapitulasi Komplikasi Pasien.
  - Formulir Penetapan Keputusan Sidang Pleno FPPE:
    1. *FullPrivilegeConfirmed* (Lolos evaluasi, wewenang klinis mandiri penuh disahkan).
    2. *ProctoringExtended* (Perpanjangan masa proctoring tambahan 3 bulan).
    3. *SupervisedPractice* (Penurunan derajat kewenangan menjadi didampingi supervisi).
    4. *PrivilegeRevoked* (Pencabutan wewenang klinis terkait).
  - Catatan Rincian Justifikasi Klinis dan Rencana Tindak Lanjut Pembinaan (*Remediation Plan*).
- **Interaksi:**
  - Tanda tangan digital anggota sidang pleno Subkomite Mutu Profesi.
  - Tombol "Finalisasi Kesimpulan FPPE" yang secara otomatis mengupdate status RKK tenaga medis.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}/conclusion`
  - `POST /api/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}/conclusion`
  - `POST /api/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}/close`

#### 24.2.7 [FE-HRD-207] Kalkulator Rapor Komposit 3 Tahun Rekredensialing
- **Rute:** `/admin/credentialing/recred-composite-scorecard`
- **Tujuan:** Perhitungan otomatis rapor komposit 3 tahun berbasis OPPE/FPPE sebagai dasar objektif sidang rekredensialing berkala dokter spesialis.
- **Elemen Tampilan:**
  - Formula Kalkulasi Progresif: Tahun 1 (Bobot 25%), Tahun 2 (Bobot 35%), Tahun 3 (Bobot 40%).
  - Kartu Rapor Komposit Akhir (Nilai Akhir 0-100) dan Penentuan Tier Kelayakan:
    - **Tier Hijau (Score >= 80 & Nol Sentinel):** Jalur Cepat (*Fast-Track*) Perpanjangan SPK/RKK Tanpa Pembatasan.
    - **Tier Kuning (Score 65–79 atau Catatan EMR):** Jalur Wawancara Klarifikasi KSM & Rencana Perbaikan Klinis.
    - **Tier Merah (Score < 65 atau Pernah Gagal FPPE):** Jalur Sidang Pleno Investigasi Khusus / Pembatasan Wewenang (*Restricted*).
  - Matriks perbandingan tren performa klinis dokter tahun ke tahun.
- **Interaksi:**
  - Tombol simulasi kalkulasi ulang jika terdapat data revisi SIMRS.
  - Modal `RecredentialingTierOverrideModal` bagi Ketua Komite Medik untuk menyesuaikan tier dengan catatan pertimbangan khusus.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/credentialing/recredentialing/scorecards/{staffId}`
  - `POST /api/v1/corporate/hrd/credentialing/recredentialing/scorecards/{staffId}/calculate`
  - `POST /api/v1/corporate/hrd/credentialing/recredentialing/scorecards/{staffId}/override-tier`

#### 24.2.8 [FE-HRD-208] Konsol Sinkronisasi Putusan Rekredensialing ke SIMRS EMR
- **Rute:** `/admin/credentialing/recred-simrs-sync`
- **Tujuan:** Monitoring dan pengendalian sinkronisasi 4 putusan rekredensialing ke modul EMR SIMRS secara otomatis dan terisolasi.
- **Elemen Tampilan:**
  - Tabel Status Sinkronisasi Wewenang Klinis SIMRS:
    - Putusan *Perpanjang As-Is* -> Level 4 Mandiri (Tersinkronisasi Aktif).
    - Putusan *Upgrade* -> Level 4 Mandiri untuk Prosedur Tambahan (Tersinkronisasi Aktif).
    - Putusan *Restricted* -> Level 3 Supervisi (Aktif dengan Validasi Co-Signature EMR 24 Jam).
    - Putusan *Revoked* -> Level 1/2 (Hard-Stop Mutlak / Blokir Akses Order Resep & Operasi SIMRS).
  - Status Sambungan Integrasi API SIMRS `INT-HR-76` (Waktu Sync Terakhir, Latensi, Status Payload).
  - Log Jejak Audit Percobaan Akses yang Ditolak oleh SIMRS akibat Status Restricted/Revoked.
- **Interaksi:**
  - Tombol "Trigger Sinkronisasi Ulang" untuk penanganan kegagalan jaringan antarsistem.
  - Modal `SimrsClinicalPrivilegeSyncModal` untuk meninjau payload JSON sebelum sinkronisasi darurat manual.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/credentialing/recredentialing/simrs-sync/status`
  - `POST /api/v1/corporate/hrd/credentialing/recredentialing/simrs-sync/{applicationId}/dispatch`
  - `GET /api/v1/corporate/hrd/credentialing/recredentialing/simrs-sync/audit-logs`

---

### 24.3 Komponen Modal, Drawer & Redux State Slice

#### 24.3.1 Komponen Modal & Drawer Pendukung

1. `OppeIndicatorConfigModal`: Modal pengaturan ambang target dan pembobotan indikator spesifik kelompok KSM (Bedah, Non-Bedah, Penunjang Medis).
2. `PeerReviewConfidentialityWarningModal`: Modal dialog peringatan kerahasiaan hukum (*Peer Review Privilege*) yang wajib dikonfirmasi sebelum mengakses telaah sejawat.
3. `FppeCaseLogEntryModal`: Modal dialog pencatatan kasus observasi klinis terproctoring lengkap dengan evaluasi kemandirian dokter pendamping.
4. `RecredentialingTierOverrideModal`: Modal persetujuan penyesuaian tier hasil perhitungan komposit 3 tahun beserta bukti justifikasi klinis Komite Medik.
5. `SimrsClinicalPrivilegeSyncModal`: Modal konfirmasi teknis payload sinkronisasi level 1-4 kewenangan klinis ke database SIMRS EMR.

#### 24.3.2 Redux State Slice (`clinicalPerformanceSlice.ts`)

```typescript
interface ClinicalPerformanceState {
  myOppeSummary: OppeStaffEvaluationSummaryDto | null;
  activeOppePeriods: OppeEvaluationPeriodDto[];
  metricCatalogs: OppeMetricCatalogDto[];
  assignedPeerReviews: OppePeerReviewAssignmentDto[];
  activeFppeTriggers: FppeTriggerFeedDto[];
  activeFppeEngagements: PagedResult<FppeEngagementSummaryDto>;
  currentCaseLogs: FppeCaseLogDto[];
  recredCompositeScorecard: RecredentialingCompositeScorecardDto | null;
  simrsSyncStatuses: PagedResult<SimrsPrivilegeSyncStatusDto>;
  isLoading: boolean;
  error: string | null;
}
```

---

## 25. Arsitektur Frontend Sub-Modul 16: Learning, Training & Competency (`S-C2`)

### 25.1 Struktur Menu & Navigasi

Sub-modul Learning, Training & Competency terintegrasi ke dalam portal mandiri pegawai (ESS) dan portal administrasi Diklat / Manajemen Rumah Sakit:

```text
Portal SDM & Tenaga Medis
└── Pendidikan, Pelatihan & Kompetensi (Learning & Development)
    ├── [FE-HRD-216] Dashboard Pelatihan Pegawai, Sertifikasi & Radar Kedaluwarsa (/portal/learning/dashboard)
    ├── [FE-HRD-217] Manajemen Kalender & Sesi Pelatihan Diklat RS (/admin/learning/training-sessions)
    ├── [FE-HRD-218] Antarmuka Presensi Sesi & Monitoring Kehadiran Kelas (/admin/learning/sessions/{id}/attendance)
    ├── [FE-HRD-219] Meja Asesmen Teori & Ujian Praktik Simulasi Klinis OSCE (/admin/learning/sessions/{id}/assessments)
    ├── [FE-HRD-220] Portal Pengajuan Diklat Eksternal & Pelacakan Proposal Belajar (/portal/learning/enrollment-requests)
    ├── [FE-HRD-221] Verifikasi Anggaran Diklat & Kontrak Ikatan Dinas (/admin/learning/budget-and-bonds)
    ├── [FE-HRD-222] Tracker Portofolio Satuan Kredit Profesi (SKP) Plataran Sehat (/portal/learning/skp-portfolio)
    └── [FE-HRD-223] Konsol Pengamanan Shift Dinas & Kepatuhan Pelatihan Wajib (/admin/learning/overdue-compliance)
```

---

### 25.2 Spesifikasi Layar Antarmuka Pengguna

#### 25.2.1 [FE-HRD-216] Dashboard Pelatihan Pegawai, Sertifikasi & Radar Kedaluwarsa
- **Rute:** `/portal/learning/dashboard`
- **Tujuan:** Menyajikan rekam jejak sertifikasi, status pemenuhan 5 pelatihan wajib akreditasi KARS, serta radar peringatan kedaluwarsa bagi individu pegawai dan Kepala Ruangan.
- **Elemen Tampilan:**
  - Kartu Ringkasan Kepatuhan Pelatihan Wajib Akreditasi RS:
    1. Bantuan Hidup Dasar (BHD / BLS) — Status: Aktif / Expiring Soon / Overdue.
    2. Pencegahan dan Pengendalian Infeksi (PPI).
    3. Keselamatan Pasien Rumah Sakit (SKP).
    4. Keselamatan dan Kesehatan Kerja Rumah Sakit & Kebakaran (K3RS / Code Red).
    5. Komunikasi Efektif (SBAR).
  - Radar Pemantau Kedaluwarsa Sertifikat (Timeline H-90, H-60, H-30, H-0).
  - Tabel Riwayat Sertifikat Kompetensi Aktif beserta tombol unduh PDF resmi ber-QR Code publik.
  - Banner peringatan prioritas tinggi jika terdapat sertifikat kritis yang mendekati jatuh tempo.
- **Interaksi:**
  - Tombol "Daftar Sesi Refresher Diklat" yang memicu modal `RefresherSessionReservationModal`.
  - Filter pencarian sertifikat berdasarkan rumpun kompetensi dan rentang tanggal terbit.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/learning/mandatory-trainings/my-status`
  - `GET /api/v1/corporate/hrd/learning/certificates/my-records`

#### 25.2.2 [FE-HRD-217] Manajemen Kalender & Sesi Pelatihan Diklat RS
- **Rute:** `/admin/learning/training-sessions`
- **Tujuan:** Antarmuka pengelola Bagian Diklat RS untuk merencanakan kalender pelatihan tahunan dan mengelola jadwal sesi kelas/praktikum.
- **Elemen Tampilan:**
  - Tampilan Kalender Interaktif (Jadwal Kelas Internal, Simulasi OSCE, dan Webinar).
  - Tabel Daftar Sesi Pelatihan (Kode Sesi, Nama Program Diklat, Tanggal Mulai/Selesai, Lokasi Ruangan, Nama Instruktur Utama, Kuota Peserta, Status: *Draft*, *OpenForEnrollment*, *Ongoing*, *Completed*, *Cancelled*).
  - Panel Ringkasan Pagu Anggaran Pelatihan Tahunan vs Realisasi Biaya.
- **Interaksi:**
  - Tombol aksi: "Buka Sesi Baru", "Tutup Pendaftaran", "Cetak Lembar Presensi", "Generate Sesi Refresher".
  - Filter pencarian berdasarkan kategori materi diklat (Wajib KARS, Klinis Spesifik, Umum).
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/learning/training-plans`
  - `GET /api/v1/corporate/hrd/learning/training-sessions`
  - `POST /api/v1/corporate/hrd/learning/training-sessions`

#### 25.2.3 [FE-HRD-218] Antarmuka Presensi Sesi & Monitoring Kehadiran Kelas
- **Rute:** `/admin/learning/sessions/{id}/attendance`
- **Tujuan:** Fasilitas check-in presensi peserta pelatihan di lokasi kelas atau laboratorium praktikum klinis.
- **Elemen Tampilan:**
  - Pemancar Kode QR Presensi Sesi Dinamis (berubah setiap 30 detik untuk mencegah kecurangan titip presensi).
  - Tabel Daftar Kehadiran Peserta Real-Time (Nama Nakes, NIP, Unit Kerja, Waktu Check-In, Metode: Tap RFID / QR Scanner / Manual Instruktur, Status Verifikasi Kehadiran).
  - Kartu Indikator Persentase Kehadiran Peserta (Ambang Batas Kelulusan: >= 80% umum atau 100% klinis khusus).
- **Interaksi:**
  - Tombol "Tandai Hadir Manual" oleh instruktur dengan input alasan khusus.
  - Ekspor log presensi ke format PDF/Excel resmi bertanda tangan digital instruktur.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/learning/training-sessions/{id}/attendances`
  - `POST /api/v1/corporate/hrd/learning/training-sessions/{id}/attendances/check-in`
  - `PUT /api/v1/corporate/hrd/learning/training-sessions/{id}/attendances/{participantId}/verify`

#### 25.2.4 [FE-HRD-219] Meja Asesmen Teori & Ujian Praktik Simulasi Klinis OSCE
- **Rute:** `/admin/learning/sessions/{id}/assessments`
- **Tujuan:** Lembar kerja evaluasi komprehensif bagi Trainer/Assessor klinis untuk menginput nilai pre-test, post-test, dan hasil ujian keterampilan klinis langsung (*OSCE*).
- **Elemen Tampilan:**
  - Tabel Evaluasi Kelulusan Ganda Peserta:
    - Kolom Jam Presensi Kehadiran (%).
    - Kolom Nilai Pre-Test & Post-Test Teori (Ambang Batas: >= 80.0 klinis / >= 75.0 umum).
    - Kolom Status Ujian Praktik Simulasi Klinis OSCE (*Lulus / Remedial / Tidak Lolos*).
    - Kolom Nama Assessor Klinis Penguji.
  - Status Kelulusan Akhir (*Passed / Failed / RemedialScheduled*).
- **Interaksi:**
  - Tombol "Input Rubrik Ujian OSCE" yang memicu modal `OscePracticalScoringModal`.
  - Tombol "Sahkan Kelulusan & Terbitkan Sertifikat Digital", yang memvalidasi pemenuhan syarat ganda sebelum sertifikat ber-QR terbit.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/learning/training-sessions/{id}/assessments`
  - `POST /api/v1/corporate/hrd/learning/training-sessions/{id}/assessments/submit`
  - `POST /api/v1/corporate/hrd/learning/training-sessions/{id}/finalize-results`

#### 25.2.5 [FE-HRD-220] Portal Pengajuan Diklat Eksternal & Pelacakan Proposal Belajar
- **Rute:** `/portal/learning/enrollment-requests`
- **Tujuan:** Fasilitas bagi pegawai untuk mengajukan keikutsertaan pelatihan/seminar/fellowship eksternal berbayar secara mandiri via ESS.
- **Elemen Tampilan:**
  - Tabel Riwayat Pengajuan Pelatihan Eksternal Pegawai (Judul Pelatihan, Penyelenggara Eksternal, Tanggal Pelaksanaan, Estimasi Biaya, Status Persetujuan: *Submitted*, *DepartmentApproved*, *BudgetVerified*, *DirectorApproved*, *Rejected*).
  - Visual Stepper Alur Persetujuan Berjenjang.
  - Kartu Peringatan Kewajiban Ikatan Dinas (jika estimasi biaya > Rp 10 Juta atau durasi > 1 bulan).
- **Interaksi:**
  - Tombol "Ajukan Permohonan Pelatihan Eksternal" yang memicu modal `ExternalTrainingProposalModal`.
  - Tombol unggah laporan hasil pelatihan dan bukti sertifikat kelulusan pasca-kegiatan.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/learning/enrollment-requests/my-requests`
  - `POST /api/v1/corporate/hrd/learning/enrollment-requests`
  - `POST /api/v1/corporate/hrd/learning/enrollment-requests/{id}/upload-completion-report`

#### 25.2.6 [FE-HRD-221] Verifikasi Anggaran Diklat & Kontrak Ikatan Dinas
- **Rute:** `/admin/learning/budget-and-bonds`
- **Tujuan:** Meja kerja Manajer Diklat dan HR Legal untuk mengendalikan pagu anggaran diklat dan menerbitkan perjanjian ikatan dinas retensi kerja.
- **Elemen Tampilan:**
  - Dashboard Alokasi & Realisasi Pagu Anggaran Diklat per Unit Kerja (`TrxTrainingBudget`).
  - Antrean Berkas Pengajuan Pelatihan Berbiaya Tinggi (> Rp 10 Juta / > 1 Bulan).
  - Tabel Daftar Nakes yang Sedang Menjalani Masa Ikatan Dinas (Nama Nakes, Judul Diklat yang Dibiayai, Total Investasi RS, Masa Retensi, Tanggal Selesai Ikatan Dinas, Status: *Active*, *Fulfilled*, *Breached*).
- **Interaksi:**
  - Tombol "Terbitkan Draf Perjanjian Ikatan Dinas" yang memicu modal `TrainingBondSignatureModal`.
  - Penolakan permohonan jika anggaran unit telah habis atau tidak relevan dengan kebutuhan rumah sakit.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/learning/budgets/status`
  - `GET /api/v1/corporate/hrd/learning/training-bonds/active`
  - `POST /api/v1/corporate/hrd/learning/training-bonds/issue`

#### 25.2.7 [FE-HRD-222] Tracker Portofolio Satuan Kredit Profesi (SKP) Plataran Sehat
- **Rute:** `/portal/learning/skp-portfolio`
- **Tujuan:** Dashboard pelacakan kecukupan perolehan Satuan Kredit Profesi (SKP) resmi Kemenkes bagi tenaga medis menuju perpanjangan SIP 5 tahun.
- **Elemen Tampilan:**
  - Circular Progress Bar Pemenuhan Target SKP 5 Tahun (misal: Tercapai 38 dari Target 50 SKP — 76%).
  - Breakdown 3 Kategori Resmi SKP Kemenkes:
    1. SKP Ranah Pembelajaran (Pelatihan, Workshop, Simposium, E-Learning).
    2. SKP Ranah Pelayanan (Pelayanan Medis/Keperawatan Harian, Visite).
    3. SKP Ranah Pengabdian Masyarakat & Profesi (Bakti Sosial, Pengurus Organisasi Profesi).
  - Tabel Rekam Jejak Kegiatan Ber-SKP (Nama Kegiatan, Penyelenggara, No SK Kemenkes, Tanggal, Poin SKP, Tautan Verifikasi Plataran Sehat).
- **Interaksi:**
  - Tombol "Klaim Perolehan SKP Baru" yang memicu modal `PlataranSehatVerificationModal`.
  - Tombol ekspor portofolio SKP resmi untuk lampiran permohonan perpanjangan SIP ke Subkomite Kredensial Sub-14.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/learning/skp-tracker/my-summary`
  - `POST /api/v1/corporate/hrd/learning/skp-tracker/claim`
  - `GET /api/v1/corporate/hrd/learning/skp-tracker/export-portfolio`

#### 25.2.8 [FE-HRD-223] Konsol Pengamanan Shift Dinas & Kepatuhan Pelatihan Wajib
- **Rute:** `/admin/learning/overdue-compliance`
- **Tujuan:** Pengawasan kepatuhan pelatihan wajib akreditasi rumah sakit dan pengendalian operasional shift nakes dengan sertifikat kedaluwarsa.
- **Elemen Tampilan:**
  - Angka Persentase Kepatuhan Pelatihan Wajib RS (Target Akreditasi: 100%).
  - Tabel Nakes Berstatus *Overdue Mandatory Training* (Nama Nakes, Ruangan/Unit, Jenis Pelatihan yang Expired, Hari Keterlambatan, Status Shift Dinas).
  - Status Penugasan Pendampingan (*Buddy System*): Nama pendamping perawat/dokter senior yang ditunjuk untuk shift dinas nakes terkait di Sub-06.
- **Interaksi:**
  - Tombol "Kirim Notifikasi Peringatan Terakhir" ke WhatsApp dan ESS nakes.
  - Tombol "Disposisi Penugasan Refresher Darurat" untuk mengalokasikan kursi prioritas pada kelas diklat terdekat.
- **API Pendukung:**
  - `GET /api/v1/corporate/hrd/learning/mandatory-trainings/overdue-list`
  - `POST /api/v1/corporate/hrd/learning/mandatory-trainings/{staffId}/assign-refresher-cito`
  - `POST /api/v1/corporate/hrd/learning/mandatory-trainings/{staffId}/escalate-credentialing`

---

### 25.3 Komponen Modal, Drawer & Redux State Slice

#### 25.3.1 Komponen Modal & Drawer Pendukung

1. `OscePracticalScoringModal`: Modal dialog penginputan rubrik penilaian simulasi keterampilan klinis OSCE per station uji (cth: RJP, Intubasi, APD) lengkap dengan tanda tangan digital Assessor.
2. `TrainingBondSignatureModal`: Modal penandatanganan elektronik (e-Sign) perjanjian ikatan dinas retensi kerja nakes dengan pratinjau klausul penalti ganti rugi.
3. `PlataranSehatVerificationModal`: Modal verifikasi nomor akreditasi Kemenkes dan ID sertifikat digital Plataran Sehat Kemenkes.
4. `ExternalTrainingProposalModal`: Modal pengajuan proposal pelatihan eksternal lengkap dengan rincian biaya pendaftaran, perjalanan, dan justifikasi klinis.
5. `RefresherSessionReservationModal`: Modal cepat bagi nakes untuk memilih slot jadwal pelatihan penyegaran saat menerima radar H-90 kedaluwarsa.

#### 25.3.2 Redux State Slice (`learningDevelopmentSlice.ts`)

```typescript
interface LearningDevelopmentState {
  myMandatoryStatus: MandatoryTrainingSummaryDto | null;
  myCertificates: PagedResult<TrainingCertificateSummaryDto>;
  activeTrainingPlans: TrainingPlanDto[];
  trainingSessions: PagedResult<TrainingSessionListItemDto>;
  currentSessionAttendance: SessionAttendanceDetailDto | null;
  currentSessionAssessments: SessionAssessmentDetailDto[];
  myEnrollmentRequests: EnrollmentRequestDto[];
  trainingBudgets: TrainingBudgetSummaryDto[];
  activeTrainingBonds: PagedResult<TrainingBondListItemDto>;
  mySkpSummary: SkpPortfolioSummaryDto | null;
  overdueStaffList: PagedResult<OverdueStaffComplianceDto>;
  isLoading: boolean;
  error: string | null;
}
```


## 26. Sub-Modul 17: Performance Management & Clinical Career (Slice `S-C3`)

Bagian ini mendefinisikan arsitektur antarmuka pengguna (frontend) Next.js App Router untuk tata kelola penilaian kinerja berkala rumah sakit dan jenjang karier klinis nakes. Arsitektur ini mencakup 8 layar terintegrasi ([`FE-HRD-231`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/03-frontend-architecture.md#L3020-L3055) s.d. [`FE-HRD-238`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/03-frontend-architecture.md#L3280-L3315)), 5 komponen dialog modal, dan manajemen state Redux `performanceManagementSlice.ts`.

### 26.1 Peta Butir Menu Navigasi Modul Kinerja Pegawai

```text
Dashboard & HR Analytics
└── Manajemen Kinerja & Karier (/hr/performance)
    ├── Dashboard Kinerja & Kalibrasi (/hr/performance/dashboard) ── [FE-HRD-231]
    ├── Katalog Metrik KPI & Nilai Budaya (/hr/performance/kpi-catalogs) ── [FE-HRD-232]
    ├── Siklus Kinerja & Cascading Target (/hr/performance/cycles) ── [FE-HRD-233]
    ├── Lembar Evaluasi & Persetujuan Berjenjang (/hr/performance/appraisals) ── [FE-HRD-234]
    ├── Sidang Kalibrasi Manajemen (/hr/performance/calibration) ── [FE-HRD-235]
    ├── Program Peningkatan Kinerja - PIP (/hr/performance/pip) ── [FE-HRD-236]
    ├── Jenjang Karier Klinis Nakes (/hr/performance/clinical-ladder) ── [FE-HRD-237]
    └── Banding & Sanggah Nilai Kinerja (/hr/performance/disputes) ── [FE-HRD-238]
```

---

### 26.2 Skema Rinci Layar Antarmuka (`FE-HRD-231` s.d. `FE-HRD-238`)

#### 26.2.1 [FE-HRD-231] Dashboard Kinerja Pegawai, Siklus Aktif & Matriks Sebaran Nilai
* **Rute URL:** `/hr/performance/dashboard`
* **Hak Akses:** `Performance_Cycle:View`
* **Kegunaan:** Menampilkan ringkasan eksekutif progres pengisian evaluasi kinerja rumah sakit, persentase kepatuhan pengisian self-assessment dan atasan, grafik sebaran kurva nilai aktual vs pedoman distribusi fleksibel (10-15% A, 70-80% B, 5-10% C/D), kartu metrik pegawai berstatus PIP aktif, serta jadwal tenggat waktu kalender penilaian.
* **Komponen & Sumber Data:**
  - Kartu Metrik KPI (`TotalPegawaiDinilai`, `RataRataSkorRS`, `TotalPegawaiDalamPip`, `SisaHariKalibrasi`).
  - Diagram Distribusi Nilai Komposit RS (Grafik Batang & Garis Kurva Normal Lonceng).
  - Tabel Peringatan Keterlambatan Pengisian Penilaian per Unit Kerja.

#### 26.2.2 [FE-HRD-232] Konfigurasi Master Metrik KPI & Indikator Core Values BerAKHLAK
* **Rute URL:** `/hr/performance/kpi-catalogs`
* **Hak Akses:** `Performance_Metric:Manage`
* **Kegunaan:** Mengelola katalog master target metrik kinerja kuantitatif (efisiensi biaya, mutu layanan, akurasi administrasi) dan rubrik deskriptor perilaku Core Values BerAKHLAK (Berorientasi Pelayanan, Akuntabel, Kompeten, Harmonis, Loyal, Adaptif, Kolaboratif) skala likert 1-5.
* **Komponen & Sumber Data:**
  - Tabel katalog metrik dengan filter kategori, rumpun profesi (Medis, Keperawatan, Penunjang, Umum).
  - Tombol aksi pembuka modal `KpiMetricCatalogModal` untuk pendaftaran indikator baru.

#### 26.2.3 [FE-HRD-233] Manajemen Siklus Evaluasi Kinerja & Cascading Sasaran Unit
* **Rute URL:** `/hr/performance/cycles`
* **Hak Akses:** `Performance_Cycle:Manage`
* **Kegunaan:** Mengatur pembukaan siklus evaluasi tahunan/semesteran, menetapkan kalender tenggat waktu (Self-appraisal, Manager review, Sidang kalibrasi), serta memfasilitasi cascading target sasaran mutu unit ke draf target individu staf melalui dialog `GoalCascadingModal`.
* **Komponen & Sumber Data:**
  - Tabel riwayat siklus kinerja tahunan dengan status badge (`Draft`, `Active`, `InReview`, `Calibrating`, `Closed`).
  - Panel visual pohon cascading target: Sasaran Strategis RS -> Sasaran Instalasi -> Sasaran Individu Staf.

#### 26.2.4 [FE-HRD-234] Portal Lembar Evaluasi Kinerja (Self-Assessment, Karu & Kabid)
* **Rute URL:** `/hr/performance/appraisals`
* **Hak Akses:** `Performance_Appraisal:Assess`
* **Kegunaan:** Portal kerja pengisian evaluasi komposit 60% KPI + 40% Perilaku Core Values dengan alur berjenjang: staf mengisi refleksi diri dan unggah bukti di ESS, atasan langsung (Karu) memberi skor dan catatan pembinaan, serta atasan kedua (Kabid) melakukan penelaahan dan otorisasi berkas.
* **Komponen & Sumber Data:**
  - Tab status berkas: `Menunggu Penilaian Mandiri`, `Menunggu Review Karu`, `Menunggu Otorisasi Kabid`, `Selesai Kalibrasi`.
  - Formulir penilaian interaktif memisahkan tabel butir KPI numerik dan tabel rubrik Core Values BerAKHLAK dengan kalkulator komposit real-time (`kpi * 0.6 + behavior * 0.4`).

#### 26.2.5 [FE-HRD-235] Konsol Sidang Pleno Kalibrasi Manajemen & Kurva Sebaran RS
* **Rute URL:** `/hr/performance/calibration`
* **Hak Akses:** `Performance_Calibration:Manage`
* **Kegunaan:** Ruang kerja digital Komite Kalibrasi (Direksi, HR Manager, Kabid) untuk meninjau distribusi skor seluruh ruangan, mendeteksi leniency bias antar kepala ruangan, melakukan penyesuaian skor dengan mencatat nota justifikasi resmi via `CalibrationOverrideModal`, dan mengesahkan nilai final komposit RS.
* **Komponen & Sumber Data:**
  - Visual kurva lonceng interaktif dengan indikator sebaran zona pedoman (*Guiding Distribution*).
  - Tabel perbandingan skor sebelum dan sesudah kalibrasi (*Pre- vs Post-Calibration*).
  - Tombol eksekusi pengesahan massal dan rilis nilai ke portal pegawai.

#### 26.2.6 [FE-HRD-236] Tata Kelola Program Peningkatan Kinerja (PIP 60 Hari) & Logbook
* **Rute URL:** `/hr/performance/pip`
* **Hak Akses:** `Performance_Pip:Manage`
* **Kegunaan:** Mengelola pelaksanaan program pembinaan intensif bagi staf berkinerja rendah (skor < 65), menerbitkan kontrak komitmen rencana aksi via `PipContractModal`, memantau logbook evaluasi mingguan mentor (Minggu 1 s.d. 8), serta mencatat putusan akhir sidang PIP (Lulus, Perpanjang, atau Eskalasi HR Legal).
* **Komponen & Sumber Data:**
  - Kartu progres pembinaan staf dengan timeline 60 hari kalender.
  - Tabel rincian logbook mingguan dengan indikator ketercapaian target (*On Track* / *At Risk*).

#### 26.2.7 [FE-HRD-237] Portofolio Asesmen Kenaikan Jenjang Karier Klinis Nakes PK I-V
* **Rute URL:** `/hr/performance/clinical-ladder`
* **Hak Akses:** `Clinical_Career:Manage`
* **Kegunaan:** Portal pengajuan dan verifikasi permohonan kenaikan jenjang karier klinis tenaga keperawatan dan nakes (PK I s.d. PK V), memvalidasi gate prasyarat administratif (skor KPI >= 75, kompetensi Sub-16, bebas sentinel Sub-15), memfasilitasi audit portofolio logbook klinis, dan mencatat rekomendasi sidang Komite Keperawatan.
* **Komponen & Sumber Data:**
  - Tabel permohonan jenjang klinis dengan status gate kelayakan (`Eligible` / `Ineligible`).
  - Drawer audit portofolio tindakan klinis dan tombol pembuka `ClinicalLadderReviewModal`.

#### 26.2.8 [FE-HRD-238] Portal Sanggah Banding Nilai Kinerja & Mediasi Sengketa
* **Rute URL:** `/hr/performance/disputes`
* **Hak Akses:** `Performance_Dispute:Manage`
* **Kegunaan:** Portal pendaftaran keberatan nilai oleh pegawai (maksimal 7 hari kalender pasca rilis nilai), pencatatan resume dokumen bukti pembanding, pengelolaan jadwal mediasi klarifikasi HR Performance, serta pencatatan putusan akhir sidang majelis banding dalam batas SLA 14 hari kalender.
* **Komponen & Sumber Data:**
  - Counter hitung mundur sisa waktu sanggah banding (*countdown SLA timer*).
  - Tabel rekapitulasi sengketa nilai dengan status: `Diajukan`, `Mediasi HR`, `Sidang Majelis`, `Putusan Final`.
  - Panel rincian putusan banding: Nilai Tetap, Nilai Disesuaikan, atau Pergantian Penilai.

---

### 26.3 Komponen Dialog Modal Pendukung

1. **`KpiMetricCatalogModal`:** Form dialog pendaftaran indikator kinerja kuantitatif dan butir Core Values BerAKHLAK dengan validasi tipe data, target numerik, dan formula polaritas.
2. **`GoalCascadingModal`:** Dialog penetapan target sasaran individu pegawai yang diturunkan dari KPI instalasi/unit, validasi batas 3 s.d. 8 butir sasaran, dan total bobot 100%.
3. **`CalibrationOverrideModal`:** Dialog otorisasi penyesuaian skor kinerja oleh Komite Kalibrasi, mewajibkan pengisian alasan pertimbangan klinis/manajerial dan lampiran nota risalah sidang.
4. **`PipContractModal`:** Dialog penerbitan kontrak pembinaan PIP 60 hari kalender, penetapan sasaran perbaikan terukur, penunjukan mentor, dan form entri logbook mingguan.
5. **`ClinicalLadderReviewModal`:** Dialog sidang Komite Keperawatan untuk mengevaluasi audit logbook klinis dan menetapkan rekomendasi kenaikan jenjang klinis (PK I s.d. PK V).

---

### 26.4 Redux State Management (`performanceManagementSlice.ts`)

```typescript
export interface PerformanceManagementState {
  activeCycle: PerformanceCycleDto | null;
  cyclesList: PagedResult<PerformanceCycleDto>;
  kpiCatalogs: KpiMetricCatalogDto[];
  myAppraisal: PerformanceAppraisalDetailDto | null;
  subordinateAppraisals: PagedResult<PerformanceAppraisalSummaryDto>;
  calibrationData: CalibrationDistributionDto | null;
  activePipList: PerformancePipDto[];
  clinicalLadderApplications: PagedResult<ClinicalLadderReviewDto>;
  activeDisputes: PerformanceDisputeDto[];
  isLoading: boolean;
  error: string | null;
}
```


---

## 27. Sub-Modul 18: Hubungan Industrial & Terminasi (Slice `S-C5`)

### 27.1 Peta Butir Menu Navigasi Modul Hubungan Industrial & Terminasi

Menu navigasi ditempatkan di bawah grup utama `Kepegawaian & Tata Tertib Rumah Sakit` dengan pemisahan hak akses berbasis peran (Staf Medis / Karyawan, Supervisor, HR Relations, Komite Disiplin, dan Direksi):

```text
Sidebar Menu: Hubungan Industrial & Terminasi (Sub-Modul 18)
├── [FE-HRD-246] Dasbor Hubungan Industrial & Radar Exit Clearance (/hrd/relations/dashboard)
├── [FE-HRD-247] Laporan Insiden Disiplin & Filter Just Culture (/hrd/relations/incidents)
├── [FE-HRD-248] Penerbitan & Monitoring Surat Peringatan (SP 1-3) (/hrd/relations/sanctions)
├── [FE-HRD-249] Manajemen Skorsing Investigasi (Paid Suspension) (/hrd/relations/suspensions)
├── [FE-HRD-250] Kanal Pengaduan Staf (ESS) & Whistleblowing Rahasia (/hrd/relations/grievance)
├── [FE-HRD-251] Risalah Perundingan Bipartit & Perjanjian Bersama (/hrd/relations/bipartite)
├── [FE-HRD-252] Pengajuan PHK & Kalkulator Pesangon PP 35/2021 (/hrd/separations)
└── [FE-HRD-253] Pusat Kendali Exit Clearance Multi-Departemen (/hrd/separations/exit-clearance)
```

---

### 27.2 Skema Rinci Layar Antarmuka (`FE-HRD-246` s.d. `FE-HRD-253`)

#### 27.2.1 [FE-HRD-246] Dasbor Hubungan Industrial & Radar Exit Clearance
* **Tujuan Layar:** Menyajikan ikhtisar eksekutif bagi Direktur SDM dan Manajer Hubungan Industrial mengenai kasus disiplin aktif, sanksi SP yang mendekati kedaluwarsa 6 bulan, pegawai dalam masa skorsing, serta proses offboarding berjalan yang menunggu penyelesaian kliring aset.
* **Komponen Visual Utama:**
  - *Statistik Metrik Kartu:* Total SP Aktif (SP1, SP2, SP3), Kasus Skorsing Berjalan, Tiket Grievance Terbuka, dan Proses Exit Clearance Aktif.
  - *Radar Kedaluwarsa SP (H-14):* Tabel peringatan sanksi disiplin yang akan habis masa 6 bulannya dalam 14 hari ke depan.
  - *Matriks Pelanggaran Berdasar Prinsip Just Culture:* Diagram donat proporsi Human Error vs At-Risk Behavior vs Reckless Misconduct.
  - *Tabel Proses Exit Clearance Kritis:* Daftar pegawai yang akan efektif keluar dalam 3 hari ke depan dengan indikator visual departemen kliring yang masih berstatus `Pending`.

#### 27.2.2 [FE-HRD-247] Konsol Laporan Insiden Disiplin, Sidang BAP & Filter Just Culture
* **Tujuan Layar:** Mencatat pelaporan insiden pelanggaran tata tertib, memvalidasi prasyarat audit klinis KMKP/Komite Medik (`INT-HR-86`), mengelola Berita Acara Pemeriksaan (BAP), dan mencatat kesimpulan sidang etik.
* **Fitur Utama:**
  - Form pelaporan insiden dengan penandaan otomatis: Apakah insiden melibatkan pelayanan pasien (*Clinical Event*)?
  - Jika ya: Menampilkan kotak validasi wajib memasukkan nomor registrasi audit keselamatan pasien KMKP (`KmKpAuditReferenceNumber`).
  - Panel Tripartit Just Culture: Toggle klasifikasi wajib (Human Error, At-Risk Behavior, Reckless Misconduct) dengan catatan rekomendasi pembinaan vs sanksi disiplin.
  - Tab Berita Acara Pemeriksaan (BAP): Pencatatan saksi-saksi, pembelaan staf, bukti dokumen fisik/digital, dan rekomendasi putusan tim pemeriksa.

#### 27.2.3 [FE-HRD-248] Modul Penerbitan & Monitoring Surat Peringatan (SP 1-3 Masa 6 Bulan)
* **Tujuan Layar:** Menerbitkan Surat Peringatan bertingkat (Teguran Lisan, SP 1, SP 2, SP 3) dengan tanggal kedaluwarsa otomatis 6 bulan kalender sesuai Pasal 52 PP No. 35 Tahun 2021, serta memfasilitasi proses sanggah banding.
* **Fitur Utama:**
  - Otomasi tanggal berlaku: Input `IssuedDate` otomatis menghitung `ValidUntil = IssuedDate + 6 Months`.
  - Riwayat Disiplin Terpadu: Menampilkan seluruh histori sanksi masa lalu staf dari e-Dossier Sub-02 (`INT-HR-85`).
  - Pengecekan Kenaikan Tingkat: Bila pegawai memiliki SP 1 aktif, sistem menyarankan penerbitan SP 2; untuk pelanggaran berat, tombol *Emergency Direct SP 3* dapat diaktifkan setelah persetujuan Direktur SDM.
  - Tombol aksi cetak PDF resmi dokumen SP dengan tanda tangan digital tersertifikasi.

#### 27.2.4 [FE-HRD-249] Manajemen Skorsing Investigasi (*Paid Suspension*) & Pemantauan Akses SIMRS
* **Tujuan Layar:** Mengelola status skorsing staf yang sedang menjalani proses pemeriksaan intensif menuju pemutusan hubungan kerja dengan perlindungan upah 100% dan pembekuan akses rumah sakit.
* **Fitur Utama:**
  - Pelacak Durasi Skorsing: Indikator visual progres waktu (Hari ke-N dari batas maksimal 180 hari / 6 bulan).
  - Verifikasi Hak Upah Penuh: Banner konfirmasi bahwa pegawai tetap berhak atas 100% gaji pokok dan tunjangan tetap di Sub-10 Payroll sesuai Pasal 157A UU Ketenagakerjaan.
  - Saklar Keamanan Digital & Fisik (`INT-HR-88`): Status real-time pemblokiran login SSO SIMRS, EMR, PACS, dan penonaktifan kartu akses fisik pintu rumah sakit.

#### 27.2.5 [FE-HRD-250] Portal Pengaduan Staf (ESS) & Whistleblowing Rahasia Terenkripsi
* **Tujuan Layar:** Memberikan fasilitas bagi seluruh staf untuk menyampaikan keluhan kerja harian (*grievance*) serta kanal pelaporan kecurangan rahasia (*confidential whistleblowing*) dengan jaminan anti-retaliasi.
* **Fitur Utama:**
  - Pilihan Dua Mode Pelaporan:
    1. *Keluhan Terbuka:* Untuk perselisihan jadwal shift, fasilitas kerja, dan hubungan kerja; terkirim ke supervisor/atasan langsung dengan batas SLA 14 hari kerja.
    2. *Kanal Whistleblower Rahasia:* Untuk dugaan korupsi, gratifikasi obat, pelecehan seksual, pemalsuan dokumen klinis; pelapor dapat memilih opsi `Anonim`, formulir dienkripsi end-to-end, dan hanya dapat dibuka oleh Tim Kepatuhan / Direktur SDM.
  - Garansi Anti-Retaliasi: Pernyataan kebijakan perlindungan saksi/pelapor dan enkripsi identitas pelapor.

#### 27.2.6 [FE-HRD-251] Manajemen Forum & Risalah Perundingan Bipartit (LKS Bipartit / Serikat Pekerja)
* **Tujuan Layar:** Mendokumentasikan pelaksanaan perundingan bipartit antara pihak manajemen rumah sakit dan serikat pekerja / perwakilan staf dalam menyelesaikan perselisihan hubungan industrial.
* **Fitur Utama:**
  - Registrasi agenda perundingan perselisihan hak, kepentingan, atau PHK.
  - Pencatatan notulen resmi (*Minutes of Meeting*) dan daftar hadir kedua belah pihak.
  - Unggah berkas dokumen Perjanjian Bersama (PB) yang telah didaftarkan ke Dinas Tenaga Kerja.

#### 27.2.7 [FE-HRD-252] Portal Pengajuan PHK & Kalkulator Pesangon Regulasi PP 35/2021
* **Tujuan Layar:** Mengelola pengajuan pengakhiran ikatan kerja (resign sukarela, pensiun, habis masa kontrak PKWT, pemutusan SP 3) dan menghitung rincian kompensasi finansial secara otomatis berbasis formula regulasi.
* **Fitur Utama:**
  - Pilihan Alasan Terminasi (Resign, Pensiun, Kontrak Selesai, SP3, Pelanggaran Berat, Meninggal).
  - Mesin Hitung Pesangon Real-Time:
    - Input otomatis masa kerja efektif dari `HireDate` Sub-02.
    - Kalkulasi otomatis nilai Uang Pesangon (UP), Uang Penghargaan Masa Kerja (UPMK), Uang Penggantian Hak (UPH: sisa cuti tahunan belum gugur), dan Uang Pisah sesuai formula PP 35/2021.
    - Perhitungan kompensasi PKWT proporsional `(Masa Kerja / 12) x 1 Bulan Upah`.
  - Alur persetujuan finansial: Lembar kompensasi wajib diverifikasi Manajer C&B dan disahkan Direktur Keuangan.

#### 27.2.8 [FE-HRD-253] Pusat Kendali Exit Clearance Multi-Departemen & Status Kunci Payroll Sub-10
* **Tujuan Layar:** Mengontrol penyelesaian checklist serah terima aset, dokumen, dan kewajiban staf dari 5 unit kerja sebelum slip gaji terakhir dan transfer pesangon dapat dicairkan.
* **Fitur Utama:**
  - Papan Kendali 5 Departemen:
    1. *Unit Kerja:* Serah terima tugas dan berkas fisik (`Handover Notes`).
    2. *Umum & Aset:* Seragam dinas, ID card, inventaris laptop/elektronik.
    3. *Farmasi / Alkes:* Pengembalian alkes dinas, stetoskop RS, kartu kendali obat.
    4. *Keuangan:* Bebas tunggakan koperasi rumah sakit, kasbon kerja.
    5. *IT & SIMRS:* Penutupan akun SSO, pencabutan email RS, pembatalan TTE.
  - Indikator Gembok Payroll (`INT-HR-87`): Menampilkan status visual `LOCKED` (Rilis Gaji Terkunci). Tombol `UNLOCK PAYROLL` hanya aktif otomatis ketika kelima unit di atas telah berstatus `Approved`.

---

### 27.3 Komponen Dialog Modal Pendukung

1. `DisciplinaryIncidentModal.tsx`: Dialog pencatatan insiden pelanggaran tata tertib dan input nomor audit KMKP.
2. `BapInvestigationModal.tsx`: Dialog pembuatan lembar Berita Acara Pemeriksaan (BAP) dan unggah bukti fisik.
3. `SeveranceCalculationModal.tsx`: Dialog kalkulator simulasi pesangon PP 35/2021 lengkap dengan rincian UP, UPMK, UPH.
4. `DepartmentClearanceSignoffModal.tsx`: Dialog persetujuan serah terima inventaris per unit kliring.
5. `WhistleblowerSubmissionModal.tsx`: Dialog pengaduan rahasia terenkripsi dengan pemilihan opsi anonimitas.

---

### 27.4 Redux State Management (`employeeRelationSlice.ts`)

```typescript
// employeeRelationSlice.ts
import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface DisciplinaryCaseState {
  incidentReports: any[];
  activeSanctions: any[];
  suspensions: any[];
  grievanceTickets: any[];
  bipartiteForums: any[];
  separations: any[];
  activeClearanceProcess: any | null;
  severanceBreakdown: any | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: DisciplinaryCaseState = {
  incidentReports: [],
  activeSanctions: [],
  suspensions: [],
  grievanceTickets: [],
  bipartiteForums: [],
  separations: [],
  activeClearanceProcess: null,
  severanceBreakdown: null,
  isLoading: false,
  error: null,
};

export const employeeRelationSlice = createSlice({
  name: 'employeeRelation',
  initialState,
  reducers: {
    setIncidentReports: (state, action: PayloadAction<any[]>) => {
      state.incidentReports = action.payload;
    },
    setActiveSanctions: (state, action: PayloadAction<any[]>) => {
      state.activeSanctions = action.payload;
    },
    setSuspensions: (state, action: PayloadAction<any[]>) => {
      state.suspensions = action.payload;
    },
    setGrievanceTickets: (state, action: PayloadAction<any[]>) => {
      state.grievanceTickets = action.payload;
    },
    setActiveClearanceProcess: (state, action: PayloadAction<any>) => {
      state.activeClearanceProcess = action.payload;
    },
    setSeveranceBreakdown: (state, action: PayloadAction<any>) => {
      state.severanceBreakdown = action.payload;
    },
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload;
    },
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload;
    },
  },
});

export const {
  setIncidentReports,
  setActiveSanctions,
  setSuspensions,
  setGrievanceTickets,
  setActiveClearanceProcess,
  setSeveranceBreakdown,
  setLoading,
  setError,
} = employeeRelationSlice.actions;

export default employeeRelationSlice.reducer;
```


---

## 28. Sub-Modul 19: Occupational Health & Staff Safety (Slice `S-C6`)

### 28.1 Peta Butir Menu Navigasi Modul K3RS & Staff Safety

Menu navigasi dikelompokkan di bawah grup utama `Kesehatan & Keselamatan Kerja Rumah Sakit (K3RS)` dengan segmentasi wewenang berbasis peran (Dokter K3RS, Perawat K3RS, Manajer Ruangan, HR Admin, dan Seluruh Staf Medis via ESS):

```text
Sidebar Menu: Kesehatan & Keselamatan Kerja (K3RS) (Sub-Modul 19)
├── [FE-HRD-261] Dasbor K3RS & Radar Kepatuhan MCU (/hrd/occupational-health/dashboard)
├── [FE-HRD-262] Meja Kerja Dokter K3RS & Rekam Medis Terenkripsi (/hrd/occupational-health/records)
├── [FE-HRD-263] Jadwal & Registri Hasil MCU Berkala Berbasis Risiko (/hrd/occupational-health/mcu)
├── [FE-HRD-264] Konsol Darurat Tertusuk Jarum & Pajanan Cito PEP (/hrd/occupational-health/needle-stick)
├── [FE-HRD-265] Radar Imunisasi & Vaksinasi Wajib Nakes (/hrd/occupational-health/vaccinations)
├── [FE-HRD-266] Surat Pembatasan Kerja & Return-to-Work (/hrd/occupational-health/restrictions)
├── [FE-HRD-267] Insiden Kecelakaan Kerja KAK/PAK & Klaim BPJS JKK (/hrd/occupational-health/injuries)
└── [FE-HRD-268] Portal Kesehatan Kerja Mandiri Pegawai (ESS) (/hrd/occupational-health/my-health)
```

---

### 28.2 Skema Rinci Layar Antarmuka (`FE-HRD-261` s.d. `FE-HRD-268`)

#### 28.2.1 [FE-HRD-261] Dasbor K3RS Eksekutif, Radar Kepatuhan MCU & Status Proteksi Vaksinasi RS
* **Tujuan Layar:** Menyajikan ikhtisar statistik keselamatan kerja bagi Direktur Utama, Komite K3RS, dan Manajer SDM mengenai status kesehatan staf, radar kepatuhan MCU tahunan per unit, status imunitas nakes, kasus tertusuk jarum dalam pemantauan, serta staf yang sedang dalam status pembatasan kerja (*work restriction*).
* **Komponen Visual Utama:**
  - *Kartu Metrik Kunci:* Kepatuhan MCU Institusi (% dari target STARKES KPS), Total Nakes Berstatus Fit with Restriction, Insiden Tertusuk Jarum Aktif, dan Jam Kerja Hilang (*Lost Time Injury* - LTI).
  - *Radar Kepatuhan MCU Unit:* Diagram batang progres pemeriksaan kesehatan per departemen/instalasi.
  - *Radar Imunitas Hepatitis B:* Donat chart persentase staf ber-titer protektif anti-HBs (>= 10 mIU/mL) vs Low Immunity / Non-Responder.
  - *Tabel Kasus Kritis K3RS:* Daftar pegawai yang mengalami pajanan infeksius dalam 30 hari terakhir dan status pemberian PEP.

#### 28.2.2 [FE-HRD-262] Meja Kerja Dokter K3RS & Rekam Medis Kesehatan Kerja Staf Terenkripsi
* **Tujuan Layar:** Antarmuka khusus Dokter Pemeriksa Kesehatan Tenaga Kerja RS untuk mengakses dan mencatat rekam medis klinis lengkap staf dengan enkripsi AES-256 (`HRD-DEC-176`).
* **Fitur Utama:**
  - Pencarian aman data kesehatan pegawai dengan verifikasi kredensial dokter K3RS.
  - Tab Rekam Medis Klinis Lengkap: Riwayat penyakit dahulu, anamnesis okupasional, hasil laboratorium darah detail, viewer ekspertise rontgen toraks, dan riwayat alergi.
  - Konsol Penetapan Status *Fitness to Work*: Pilihan status (`Fit`, `FitWithRestriction`, `TemporaryUnfit`, `PermanentUnfit`), input durasi berlaku, dan generator kalimat restriksi operasional untuk atasan/HR (tanpa diagnosis medis).

#### 28.2.3 [FE-HRD-263] Manajemen Jadwal & Registri Hasil Medical Check-Up (MCU) Berkala Berbasis Risiko
* **Tujuan Layar:** Mengelola penjadwalan MCU berkala tahunan bagi staf, segmentasi paket pemeriksaan sesuai risiko ruangan, dan pencatatan hasil penapisan.
* **Fitur Utama:**
  - Pemetaan Paket Otomatis: Memilih pegawai otomatis menetapkan paket pemeriksaan berdasarkan profil unit (Paket Radiasi TLD, Paket Serologi Bedah/Lab, Paket Paru/TB, Paket Kimia CSSD, atau Paket Umum).
  - Kalender Penjadwalan MCU & Sinkronisasi Roster Sub-06: Mencegah jadwal MCU bertabrakan dengan shift jaga nakes.
  - Mass Certification Gate: Pengesahan kelayakan kerja massal hasil MCU oleh Dokter K3RS.

#### 28.2.4 [FE-HRD-264] Konsol Cito Tanggap Darurat Tertusuk Jarum (*Needle-Stick*) & Pajanan Okupasional PEP
* **Tujuan Layar:** Konsol gawat darurat K3RS & PPI untuk merespons insiden tertusuk jarum suntik bekas pakai atau cipratan darah pasien infeksius secara seketika (*real-time*).
* **Fitur Utama:**
  - Emergency Alert Banner: Notifikasi visual berkedip merah saat ada laporan tertusuk jarum baru yang masuk dari ESS nakes.
  - Countdown SLA PEP < 4 Jam: Penghitung mundur waktu batas pemberian obat Profilaksis Pasca Pajanan (ARV / Hepatitis B Imunoglobulin).
  - Panel Pasien Sumber Pajanan: Input nomor rekam medis pasien sumber untuk penarikan riwayat serologi SIMRS secara cepat.
  - Kalender Radar Serologi Serial: Timeline jadwal kontrol darah berkala nakes terpapar pada Bulan ke-1, Bulan ke-3, dan Bulan ke-6.

#### 28.2.5 [FE-HRD-265] Radar Imunisasi, Vaksinasi Wajib Nakes & Pemantauan Titer Antibodi
* **Tujuan Layar:** Memantau cakupan vaksinasi seluruh staf medis dan tenaga kesehatan rumah sakit serta melacak masa proteksi kekebalan tubuh aktif.
* **Fitur Utama:**
  - Matriks Status Vaksinasi Per Nakes (Hepatitis B Dosis 1-3, Influenza, Varisela, MMR).
  - Pelacak Titer Antibodi Kuantitatif (anti-HBs): Indikator visual hijau jika >= 10 mIU/mL dan merah jika < 10 mIU/mL.
  - Tombol aksi *"Terbitkan Panggilan Booster"*: Mengirimkan notifikasi panggilan vaksinasi massal ke portal ESS bagi staf yang titer antibodinya telah menurun.

#### 28.2.6 [FE-HRD-266] Tata Kelola Surat Pembatasan Kerja (*Work Restriction*) & Evaluasi Return-to-Work
* **Tujuan Layar:** Menerbitkan surat rekomendasi pembatasan tugas klinis staf medis dan mengelola alur evaluasi pemulihan kelaikan kerja penuh.
* **Fitur Utama:**
  - Formulir Penerbitan Restriksi: Checklist larangan dinas (Dilarang Shift Malam, Dilarang Angkat Beban > 10 kg, Dilarang Kontak Ruang Isolasi TB, Dilarang Area Radiasi).
  - Status Integrasi Sub-06 (`INT-HR-89`): Indikator bahwa pembatasan dinas aktif mengunci roster jadwal kerja di modul scheduling.
  - Tombol Asesmen Return-to-Work: Modal pemeriksaan fisik oleh Dokter K3RS sebelum surat restriksi dapat dicabut secara sah.

#### 28.2.7 [FE-HRD-267] Registrasi Insiden Kecelakaan Kerja (KAK/PAK), Investigasi CAPA & Generator BPJS JKK
* **Tujuan Layar:** Pencatatan insiden kecelakaan di lingkungan rumah sakit, investigasi keselamatan kerja K3RS, dan pengurusan klaim Jaminan Kecelakaan Kerja (JKK) BPJS Ketenagakerjaan.
* **Fitur Utama:**
  - Form pencatatan KAK: Waktu kejadian, lokasi spesifik, uraian kronologis, anggota tubuh cedera, dan saksi.
  - Panel Investigasi Akar Masalah (RCA) & Penyusunan Tindakan Korektif (CAPA).
  - One-Click Generator Berkas Klaim BPJS JKK: Menghasilkan formulir PDF resmi BPJS Ketenagakerjaan Formulir 3a (Tahap I, batas 2x24 jam) dan Formulir 3b (Tahap II) lengkap dengan data terisi otomatis.

#### 28.2.8 [FE-HRD-268] Portal Mandiri Kesehatan Kerja Pegawai di ESS (Riwayat MCU, Vaksin, Status Fitness)
* **Tujuan Layar:** Antarmuka mandiri bagi pegawai di Employee Self-Service (ESS) untuk memantau status kesehatan kerja pribadi.
* **Fitur Utama:**
  - Tombol Darurat Cito: *"Lapor Tertusuk Jarum / Pajanan Cairan Tubuh"*.
  - Riwayat Hasil MCU Pribadi: Unduh resume hasil pemeriksaan kesehatan berkala dengan otentikasi PIN keamanan.
  - Kartu Vaksinasi Digital: Menampilkan riwayat vaksinasi dan status antibodi nakes.
  - Informasi Status Kelaikan Kerja & Surat Pembatasan Tugas Dinas Aktif.

---

### 28.3 Komponen Dialog Modal Pendukung

1. `NeedleStickEmergencyModal.tsx`: Dialog laporan darurat tertusuk jarum di ESS dengan pertanyaan pemandu cito.
2. `MedicalExaminationEntryModal.tsx`: Dialog input data anamnesis fisik dan hasil laboratorium terenkripsi oleh tim K3RS.
3. `WorkRestrictionIssuanceModal.tsx`: Dialog penerbitan surat rekomendasi pembatasan kerja medis bagi pegawai.
4. `ReturnToWorkAssessmentModal.tsx`: Dialog Berita Acara pemeriksaan fisik pemulihan kelaikan kerja sebelum pencabutan restriksi.
5. `BpjsJkkClaimGeneratorModal.tsx`: Dialog pratinjau dan unduh berkas klaim JKK BPJS Ketenagakerjaan Tahap I dan II.

---

### 28.4 Redux State Management (`occupationalHealthSlice.ts`)

```typescript
// occupationalHealthSlice.ts
import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface OccupationalHealthState {
  healthRecords: any[];
  medicalExaminations: any[];
  fitnessAssessments: any[];
  workRestrictions: any[];
  vaccinationRecords: any[];
  needleStickIncidents: any[];
  occupationalInjuries: any[];
  activePatientSource: any | null;
  activeInjuryClaim: any | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: OccupationalHealthState = {
  healthRecords: [],
  medicalExaminations: [],
  fitnessAssessments: [],
  workRestrictions: [],
  vaccinationRecords: [],
  needleStickIncidents: [],
  occupationalInjuries: [],
  activePatientSource: null,
  activeInjuryClaim: null,
  isLoading: false,
  error: null,
};

export const occupationalHealthSlice = createSlice({
  name: 'occupationalHealth',
  initialState,
  reducers: {
    setHealthRecords: (state, action: PayloadAction<any[]>) => {
      state.healthRecords = action.payload;
    },
    setMedicalExaminations: (state, action: PayloadAction<any[]>) => {
      state.medicalExaminations = action.payload;
    },
    setWorkRestrictions: (state, action: PayloadAction<any[]>) => {
      state.workRestrictions = action.payload;
    },
    setVaccinationRecords: (state, action: PayloadAction<any[]>) => {
      state.vaccinationRecords = action.payload;
    },
    setNeedleStickIncidents: (state, action: PayloadAction<any[]>) => {
      state.needleStickIncidents = action.payload;
    },
    setOccupationalInjuries: (state, action: PayloadAction<any[]>) => {
      state.occupationalInjuries = action.payload;
    },
    setActivePatientSource: (state, action: PayloadAction<any>) => {
      state.activePatientSource = action.payload;
    },
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload;
    },
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload;
    },
  },
});

export const {
  setHealthRecords,
  setMedicalExaminations,
  setWorkRestrictions,
  setVaccinationRecords,
  setNeedleStickIncidents,
  setOccupationalInjuries,
  setActivePatientSource,
  setLoading,
  setError,
} = occupationalHealthSlice.actions;

export default occupationalHealthSlice.reducer;
```


---

## 29. Arsitektur Frontend Sub-Modul 21: Employee Self-Service (ESS) & Unified Approval Inbox (V2 Hardening) — Slice `S-A7-V2`

### 29.1 Kebutuhan Antarmuka Pengguna & Hirarki Navigasi

Sub-modul ini menyediakan antarmuka terpadu bagi pegawai rumah sakit untuk mengajukan permohonan mandiri (ESS) dan pintu tunggal bagi para atasan, kepala ruangan, dokter penanggung jawab, dan direksi untuk memeriksa serta mengeksekusi persetujuan (*Unified Approval Inbox*) dari seluruh 15 modul HRD secara cepat, transparan, dan akuntabel.

```text
Portal Intranet Rumah Sakit
  ├── /hrd/self-service                                   (FE-HRD-280 - Portal Mandiri Staf ESS)
  │     ├── /hrd/self-service/my-requests                 (FE-HRD-281 - Pelacak Riwayat Permohonan)
  │     └── /hrd/self-service/revisions/[id]              (FE-HRD-282 - Lembar Perbaikan Draf Revisi)
  │
  └── /hrd/approvals                                      (Menu Otorisasi Atasan & Manajemen)
        ├── /hrd/approvals/inbox                          (FE-HRD-276 - Kotak Masuk Persetujuan Terpadu)
        ├── /hrd/approvals/[id]                           (FE-HRD-277 - Lembar Rincian & Riwayat Tiket)
        ├── /hrd/approvals/delegation                     (FE-HRD-278 - Pengaturan Delegasi Wewenang Plh)
        ├── /hrd/approvals/matrix                         (FE-HRD-279 - Konfigurasi Matriks Alur Persetujuan)
        └── /hrd/approvals/overdue                        (FE-HRD-283 - Konsol Monitoring Tiket Lewat SLA)
```

---

### 29.2 Rincian 8 Layar Utama Next.js App Router

| Kode Layar | Rute URL | Judul Layar | Peran Pengguna (Roles) | Fitur Utama & Komponen Baku |
|---|---|---|---|---|
| `FE-HRD-276` | `/hrd/approvals/inbox` | Kotak Masuk Persetujuan Terpadu | Kepala Ruangan, Supervisor, Manajer, Direksi | Papan kendali inbox dengan tab *Pending*, *Urgent (H-4 SLA)*, *Delegated*, dan *Approved*; filter per kategori request; tombol centang persetujuan massal bersyarat; indikator badge SLA |
| `FE-HRD-277` | `/hrd/approvals/[id]` | Lembar Detail & Riwayat Permohonan | Approver Berwenang, Pelaksana Harian (Plh) | Rincian lengkap permohonan; ringkasan verifikasi otomatis (indikator hijau/merah bentrok jadwal atau saldo cuti); thread percakapan revisi; tombol tri-state: *Approve*, *Reject*, *Return for Revision* |
| `FE-HRD-278` | `/hrd/approvals/delegation` | Pengaturan Pendelegasian Wewenang | Seluruh Pejabat Approver, HR Admin | Tabel riwayat delegasi wewenang; status delegasi aktif/kedaluwarsa; tombol pembuatan delegasi baru berbatas waktu (*time-bound*); pilihan penunjukan Plh |
| `FE-HRD-279` | `/hrd/approvals/matrix` | Konfigurasi Matriks Alur Persetujuan | HR Admin, Direktur SDM | Tabel konfigurasi alur persetujuan per tipe permohonan dan unit kerja; visualisasi pohon hierarki step persetujuan; konfigurasi batas waktu SLA |
| `FE-HRD-280` | `/hrd/self-service` | Portal Layanan Mandiri Pegawai (ESS) | Seluruh Pegawai Rumah Sakit | Dasbor ringkasan profil kepegawaian staf; widget sisa saldo cuti; jadwal dinas minggu ini; riwayat absensi; kartu pintas permohonan mandiri cepat |
| `FE-HRD-281` | `/hrd/self-service/my-requests` | Pelacak Riwayat Permohonan Mandiri | Seluruh Pegawai Rumah Sakit | Tabel pelacak status seluruh permohonan staf; stepper visual perjalanan tahapan persetujuan; penanda tiket berstatus `ReturnedForRevision` |
| `FE-HRD-282` | `/hrd/self-service/revisions/[id]` | Formulir Perbaikan Permohonan Revisi | Staf Pemohon | Formulir pengeditan data permohonan yang dikembalikan atasan; catatan revisi dari approver; unggah ulang berkas lampiran; tombol ajukan ulang (*Resubmit*) |
| `FE-HRD-283` | `/hrd/approvals/overdue` | Konsol Monitoring Tiket Lewat SLA | Manajer SDM, Direksi, Kepala Bagian Mutu | Radar pemantauan tiket persetujuan yang telah melewati batas SLA; tabel metrik kecepatan respon per kepala ruangan; tombol intervensi ambil alih/eskalasi |

---

### 29.3 Daftar 5 Modal Dialog Interaktif

1. **`ApproveConfirmModal.tsx`**: Dialog konfirmasi persetujuan tunggal dengan input catatan opsional dan konfirmasi PIN keamanan atau tanda tangan digital.
2. **`RejectReasonModal.tsx`**: Dialog penolakan permohonan dengan validasi input alasan penolakan tertulis wajib minimal 15 karakter (`VR-WF-05`).
3. **`ReturnForRevisionModal.tsx`**: Dialog pengembalian berkas ke staf pemohon dengan kolom rincian perbaikan yang diminta dan lampiran instruksi.
4. **`BulkApprovalModal.tsx`**: Dialog persetujuan massal bersyarat yang menampilkan daftar tiket tervalidasi bersih (indikator hijau) beserta ringkasan risiko sebelum eksekusi 1-klik PIN.
5. **`NewDelegationModal.tsx`**: Dialog penunjukan Pelaksana Harian (Plh) dengan pemilihan nama pegawai setara, rentang tanggal mulai dan selesai, serta pilihan kategori permohonan yang didelegasikan.

---

### 29.4 State Management Redux Toolkit (`unifiedApprovalSlice.ts`)

```typescript
interface UnifiedApprovalState {
  inbox: {
    items: WorkflowInstanceDto[];
    totalCount: number;
    pendingCount: number;
    urgentCount: number;
    delegatedCount: number;
    selectedIds: string[];
    filter: InboxFilterState;
    isLoading: boolean;
    error: string | null;
  };
  currentDetail: WorkflowInstanceDetailDto | null;
  delegations: {
    items: ApprovalDelegationDto[];
    isLoading: boolean;
  };
  myRequests: {
    items: WorkflowInstanceDto[];
    totalCount: number;
    isLoading: boolean;
  };
  slaMetrics: SlaComplianceReportDto | null;
}
```
