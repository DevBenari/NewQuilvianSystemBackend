# Human Resource — Arsitektur Frontend Target

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `03-frontend-architecture.md` |
| Revision | `1` |
| Status | `draft` — **belum** `approved` |
| Owner desain | Technical owner (`HRD-DEC-015`), dengan ruang `DEV_DISCRETION` sesuai `HRD-FE-03` |
| `approved_by` / `approved_at` | **Belum ada** |
| Repository target | `QuilvianSystemFrontendDev` |
| Frontend SHA | `fff76a1b394d4b247c70a04f106c8ec098c9696e` (branch `AgentCodexFrontend`) |
| Backend SHA rujukan kontrak | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| `input_revision` — arsitektur backend | `02-backend-architecture.md` revision `1` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `10` |
| `input_hash` — decision log | `91d62d4ea81aa11fd5bf4c1c922b6c8dbe1ad273a1609e4897bae0ecafa590c0` |
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
| `FE-HRD-01` | Daftar Perubahan Data Karyawan | `/hr/workforce-core/employee-profile-changes` | `S-A1` | `MISSING / NEW` |
| `FE-HRD-02` | Daftar Penempatan Organisasi | `/hr/workforce-core/organization-assignments` | `S-A1` | `MISSING / NEW` |
| `FE-HRD-03` | Daftar Penempatan Jabatan | `/hr/workforce-core/position-assignments` | `S-A1` | `MISSING / NEW` |
| `FE-HRD-04` | Daftar Relasi Atasan | `/hr/workforce-core/manager-assignments` | `S-A1` | `MISSING / NEW` |
| `FE-HRD-05` | Daftar Riwayat Kepegawaian | `/hr/workforce-core/employment-histories` | `S-A1` | `MISSING / NEW` |
| `FE-HRD-06` | Daftar Penetapan Gaji | `/hr/workforce-core/salary-assignments` | `S-A1` | `MISSING / NEW` — hak aksesnya `[OPEN]` `HRD-Q-20` |
| `FE-HRD-07` | Detail Permohonan Perubahan Data | `/hr/workforce-core/employee-profile-changes/[slug]` | `S-A1` | `MISSING / NEW` — layar anak dari `FE-HRD-01` |

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
| `FE-HRD-26` | Permohonan Perubahan Data Saya | `/self-services/human-resource/employee/profile-changes` | `S-A5` | `MISSING / NEW` |
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
| Perencanaan tenaga kerja, rekrutmen, benefit, tiket HR | `S-D1` s.d. `S-D4` `BLOCKED` |
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
| `FE-HRD-07` Detail Permohonan Perubahan Data | `FE-HRD-01`, dengan menekan baris pada daftar |
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

Layar ini digambar terpisah karena datanya sensitif dan hak aksesnya belum diputuskan.

```text
+- Penetapan Gaji ---------------------------------------------- FE-HRD-06 -+
| [total penetapan | berlaku bulan ini | menunggu persetujuan]              |
+----------------------------------------------------------------------------+
| [cari nama/NIK]  [Unit v] [Periode berlaku v] [Status v]                   |
+----------------------------------------------------------------------------+
| Pegawai | Unit | Kelas Gaji | Berlaku Sejak | Status | Utama |             |
| ------- | ---- | ---------- | ------------- | chip   | ya   | [Detail]    |
+----------------------------------------------------------------------------+
| Kolom nominal gaji ditampilkan hanya bila peran berhak. Bila tidak,        |
| kolomnya TIDAK ADA sama sekali - bukan ditampilkan lalu disamarkan.        |
+----------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Jumlah penetapan pada periode berjalan | `GET .../salary-assignments/summary` | `WfpSalaryAssignment : Read` | Gagal → kartu diganti tombol coba lagi |
| Tabel | Daftar lintas-pegawai | `GET /hr/workforce-core/salary-assignments` — **Rencana (belum tersedia)** | `WfpSalaryAssignment : ReadAll` — **Rencana** | Kosong → "Belum ada penetapan gaji pada periode ini." |
| Kolom nominal | Gaji pokok dan tunjangan | bagian dari response yang sama | **`[OPEN]` `HRD-Q-20`** | **Tidak digambar sebagai bagian yang pasti ada.** Sampai `HRD-Q-20` dijawab, kolom nominal **MUST NOT** ditampilkan pada daftar lintas-pegawai |

**Alasan pembatasan itu.** Daftar lintas-pegawai berarti satu layar menampilkan gaji banyak
orang sekaligus. Siapa yang boleh melihatnya, dan sampai tingkat apa, adalah keputusan pemilik
produk bersama keamanan — bukan pilihan teknis. Sampai dijawab, layarnya tetap dibuat, tetapi
**tanpa kolom nominal**, dan nominal hanya terlihat di halaman detail pegawai yang sudah punya
penjagaan hari ini.

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
| Kolom nominal gaji tidak ditampilkan pada daftar lintas-pegawai | `HRD-Q-20` masih terbuka |
| Kemampuan yang dirancang berasal dari `HRD-CAP-01` s.d. `HRD-CAP-27` | `01-existing-capability-map.md` |

---

## 11. Yang sengaja tidak dikerjakan pada dokumen ini

1. Tidak ada satu baris source frontend yang diubah.
2. Tidak ada layar untuk `S-C1`, `S-C6`, `S-D1` s.d. `S-D5`.
3. Tidak ada layar payroll di luar dua serah terima yang endpoint-nya memang sudah ada.
4. Tidak ada keputusan warna, ikon, urutan menu, atau component library.
5. Tidak ada nama butir menu yang dinyatakan final.
6. Tidak ada dokumen yang ditandai `approved`.
