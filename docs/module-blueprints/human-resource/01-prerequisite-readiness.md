# Human Resource — Prerequisite Readiness

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Revision | `3` — 3 November 2026: penutupan prasyarat `S-D1`, lihat bagian 4. Revision `2` tetap utuh di bawahnya |
| Backend SHA | `ecdc135` untuk bagian 1–3; **`396c8363`** (branch `MHamzah`) untuk bagian 4 |
| Frontend SHA | `2a1cea784` untuk bagian 1–3; **`9c4000724`** (branch `HamzahV2`) untuk bagian 4 |
| Masukan | `00-interview-decisions.md` rev `3`, `01-existing-capability-map.md` rev `1.1`; bagian 4 memakai `00-interview-decisions.md` rev `17` bagian 32 |

Dokumen ini menjawab satu pertanyaan: **apa yang harus siap lebih dulu sebelum sebuah fase boleh
jalan.** Satu baris untuk satu prasyarat yang material.

Status kemampuan memakai tepat satu nilai dari taksonomi baku: `READY TO REUSE`,
`REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, atau `UNKNOWN`.

Jenis dependency memakai `MODULE_FOUNDATION`, `PHASE`, `INTEGRATION`, atau `EXTERNAL`.

**Aturan yang paling penting di dokumen ini:** sebuah dependency yang terblokir hanya memblokir
fase yang bergantung padanya. Fase lain yang aman tetap boleh berjalan, dan itu dicatat eksplisit
pada kolom terakhir.

---

## 1. Daftar dependency

### `HRD-DEP-001` — Registry kepemilikan dan prefix modul

| Field | Isi |
| --- | --- |
| Kemampuan | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Jenis | `MODULE_FOUNDATION` |
| Pemilik | Pemilik registry backend |
| Bukti | `BE@ecdc135 docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:9` mencatat `Human Resource / BUSINESS DOMAIN / Hrd / ACTIVE-LEGACY` |
| Status kemampuan | `REPAIR` |
| Dibutuhkan oleh | `HRD-PH-001`, dan menjadi prasyarat pembuatan entity HR baru mana pun |
| Dampak pemblokiran | Empat puluh entity `Wfp*` memakai prefix yang tidak punya baris registry. Selama itu belum diperbaiki, kepemilikan 40 entity tidak terbaca dari namanya, dan entity baru tidak punya dasar formal untuk memilih prefix |
| Fase lain boleh jalan | **Ya.** Seluruh pekerjaan frontend dan pemakaian endpoint existing tidak bergantung pada registry |
| SHA sumber | `ecdc135` |
| Tindakan berikutnya | Daftarkan `Wfp` sebagai prefix **yang sah** untuk keluarga workforce HR, sesuai `HRD-DEC-019`. Pakai kolom dan kosakata lifecycle yang sudah ada di registry; jangan mengarang nilai baru |

Tiga catatan yang mengikat, seluruhnya dari `HRD-DEC-019`:

1. **`Wfp` bukan legacy.** Ia prefix yang sah dan tetap dipakai untuk entity baru yang memang
   termasuk keluarga workforce. Jangan melabelinya sebagai legacy yang akan dihapus, kecuali ada
   keputusan manusia terpisah di kemudian hari.
2. **`Mst` tidak diubah.** Entity master atau referensi tetap memakai `Mst` walaupun berada di
   domain HR.
3. **Registry tidak boleh menyatakan HR memiliki seluruh prefix `Trx`.** Hitungan pada snapshot
   menunjukkan 178 entity `Trx*` berada di bawah HR dan **40 lainnya milik Health Services**.
   Kepemilikan ditetapkan dari domain, lokasi berkas, dan bukti — bukan dari prefix.

Perubahan berkas registry yang sebenarnya dikerjakan lewat task implementasi terpisah. Blueprint
ini hanya menetapkan targetnya.

### `HRD-DEP-002` — Mesin workflow dan persetujuan bersama

| Field | Isi |
| --- | --- |
| Kemampuan | `Areas/Corporate/HumanResource/WorkflowManagement` |
| Jenis | `MODULE_FOUNDATION` |
| Pemilik | HR, berperan sebagai shared platform capability dengan prefix terdaftar `Wfl` |
| Bukti | `BE@ecdc135` 6 controller, 48 endpoint, 9 model termasuk `TrxWorkflowInstance`, `TrxWorkflowStepInstance`, `TrxApprovalAction`, `TrxApprovalDelegation` |
| Status kemampuan | `EXTEND` |
| Dibutuhkan oleh | `HRD-PH-002` khususnya `S-A7`, dan seluruh alur persetujuan pada `HRD-PH-003` |
| Dampak pemblokiran | Tanpa mesin ini, kotak masuk terpadu tidak punya sumber data. Mesinnya ada, yang belum ada adalah antarmukanya |
| Fase lain boleh jalan | **Ya**, kecuali `S-A7` |
| SHA sumber | `ecdc135` |
| Tindakan berikutnya | Pastikan mesin ini dapat menjawab pertanyaan "apa saja yang menunggu persetujuan orang ini, lintas jenis transaksi". Bila belum, itu pekerjaan `EXTEND` pada `S-A7` |

`HRD-DEC-018` mengikat bentuk pemakaiannya: kotak masuk hanya menyeragamkan tampilan. Workflow,
policy, permission, validasi, SLA, dan eskalasi tetap milik masing-masing jenis transaksi.

### `HRD-DEP-003` — Identitas dan hak akses aplikasi

| Field | Isi |
| --- | --- |
| Kemampuan | Akun aplikasi, role, permission, dan pencabutan akses |
| Jenis | `INTEGRATION` |
| Pemilik | Administrator / Identity |
| Bukti | `BE@ecdc135 Shared/HumanResource/Services/HumanResourceContextService.cs` menurunkan konteks HR dari pengguna terautentikasi; `[AccessController]` dan `[AccessPermission]` dipakai 150 dari 150 controller HR |
| Status kemampuan | `UNKNOWN` |
| Dibutuhkan oleh | `HRD-PH-002` dan `HRD-PH-008`; sejak 3 November 2026 juga **rilis** `S-D1` (`HRD-PH-007`) untuk pemetaan peran fungsional Finance / Anggaran, Direktur, dan Recruiter yang berstatus `MAPPING_REQUIRED` (`HRD-DEC-064`, `HRD-DEC-066`) |
| Dampak pemblokiran | Belum ada bukti bahwa HR dapat meminta pembuatan akun saat onboarding maupun pencabutan akses saat offboarding. Selama itu belum jelas, offboarding tidak dapat dinyatakan tuntas dari sisi keamanan. Untuk `S-D1`, tanpa pemetaan peran, rantai `T10` dan `T12` tidak dapat diajukan (`UNRESOLVED_STEP_FAILS_CLOSED`); **desain tidak terhalang** |
| Fase lain boleh jalan | **Ya.** Yang terdampak hanya bagian onboarding dan offboarding pada `S-C4` |
| SHA sumber | `ecdc135` |
| Tindakan berikutnya | Telusuri apakah ada kontrak antara HR dan Identity. Bila tidak ada, ini menjadi pertanyaan baru untuk pemilik Identity |

### `HRD-DEP-004` — Finance untuk penyelesaian payroll

| Field | Isi |
| --- | --- |
| Kemampuan | Pembayaran, posting akuntansi, pajak, dan pelaporan payroll |
| Jenis | `INTEGRATION` |
| Pemilik | Finance |
| Bukti | `BE@ecdc135 AttendanceManagement/Controllers/AttendancePayrollHandoffController.cs` menyediakan `execute`, `repair`, dan `rollback` per `payrollRunId` |
| Status kemampuan | `UNKNOWN` |
| Dibutuhkan oleh | Bagian akhir `HRD-PH-004` |
| Dampak pemblokiran | Batas tanggung jawab sudah final lewat `HRD-DEC-009`, tetapi bentuk data serah terima dan perilaku bila Finance menolak batch belum disepakati. Bagian itu **tidak boleh difinalkan** |
| Fase lain boleh jalan | **Ya.** Perhitungan payroll sisi HR, penutupan periode kehadiran, dan rekonsiliasi tetap boleh dirancang |
| SHA sumber | `ecdc135` |
| Tindakan berikutnya | Jawab `HRD-Q-10` dan `HRD-Q-11` bersama pemilik Finance |

### `HRD-DEP-005` — Health Services untuk pengecekan kewenangan klinis

| Field | Isi |
| --- | --- |
| Kemampuan | Pengecekan kewenangan klinis saat pelayanan, dan sumber angka OPPE/FPPE |
| Jenis | `INTEGRATION` |
| Pemilik | Health Services |
| Bukti | Tidak ditemukan integrasi apa pun dari sisi HR pada `ecdc135` |
| Status kemampuan | `MISSING` |
| Dibutuhkan oleh | `HRD-PH-005` |
| Dampak pemblokiran | Rantai kredensial sampai izin praktik tidak terbentuk |
| Fase lain boleh jalan | **Ya.** `HRD-DEC-006` sudah memastikan HR bukan jalur kritis pendaftaran pasien, sehingga seluruh fase administratif berdiri sendiri |
| SHA sumber | `ecdc135` |
| Tindakan berikutnya | Menunggu `hospital-domain-architect`. Jangan merancang bentuk integrasinya sekarang |

### `HRD-DEP-006` — Penyimpanan berkas dan dokumen

| Field | Isi |
| --- | --- |
| Kemampuan | Penyimpanan ijazah, STR, SIP, sertifikat, hasil MCU, dan lampiran bukti |
| Jenis | `EXTERNAL` |
| Pemilik | Shared platform |
| Bukti | `BE@ecdc135 AttendanceCorrectionController.cs:128` memiliki `GET {id}/evidence/download`, sehingga sudah ada pola lampiran bukti pada koreksi kehadiran |
| Status kemampuan | `UNKNOWN` |
| Dibutuhkan oleh | `HRD-PH-003` bagian koreksi kehadiran, `HRD-PH-008` bagian sertifikat |
| Dampak pemblokiran | Bila cara penyimpanannya berbeda antar domain, bukti akan tersebar dan sulit ditelusuri auditor |
| Fase lain boleh jalan | **Ya** |
| SHA sumber | `ecdc135` |
| Tindakan berikutnya | Periksa apakah pola lampiran pada koreksi kehadiran memang pola bersama, atau khusus domain itu saja |

### `HRD-DEP-007` — Arsitektur domain rumah sakit untuk slice klinis

| Field | Isi |
| --- | --- |
| Kemampuan | Bounded context, batas aggregate, lifecycle, dan batas keselamatan klinis untuk kredensial, kewenangan klinis, OPPE, FPPE, dan kesehatan kerja |
| Jenis | `PHASE` |
| Pemilik | `requirement-completeness-gate` lalu `hospital-domain-architect` |
| Bukti | Folder `evidence/` modul HR kosong. Sebagai pembanding, `rawat-inap` dan `billing-kasir` masing-masing memiliki `02-requirement-completeness-gate.md` dan `03-hospital-domain-architecture.md` |
| Status kemampuan | `MISSING` |
| Dibutuhkan oleh | `HRD-PH-005` dan `HRD-PH-006` |
| Dampak pemblokiran | Kedua fase itu **tidak boleh dirancang**. Merancangnya berarti mengarang batas kewenangan praktik dokter dan aturan akses data kesehatan pribadi |
| Fase lain boleh jalan | **Ya.** Tujuh fase administratif berdiri sendiri dan tidak menyentuh keputusan klinis |
| SHA sumber | — |
| Tindakan berikutnya | Jalankan `requirement-completeness-gate` untuk slice klinis, lalu `hospital-domain-architect` |

---

## 2. Ringkasan dampak terhadap fase

| Fase | Dependency yang dibutuhkan | Boleh jalan sekarang? |
| --- | --- | --- |
| `HRD-PH-001` fondasi | `HRD-DEP-001` | **Ya** — perbaikan registry justru isi fase ini |
| `HRD-PH-002` layanan mandiri dan kotak masuk | `HRD-DEP-002`, `HRD-DEP-003` | **Ya** |
| `HRD-PH-003` administrasi waktu kerja | `HRD-DEP-002`, `HRD-DEP-006` | **Ya** |
| `HRD-PH-004` payroll sisi HR | `HRD-DEP-004` | **Sebagian.** Perhitungan ya, serah terima tidak |
| `HRD-PH-005` kredensial dan kewenangan klinis | `HRD-DEP-005`, `HRD-DEP-007` | **Tidak** |
| `HRD-PH-006` kesehatan kerja | `HRD-DEP-007` | **Tidak** |
| `HRD-PH-007` domain yang diturunkan ulang | `HRD-Q-05` | **Sebagian sejak 3 November 2026.** `S-D1` **ya** untuk desain — `HRD-Q-05` ditutup untuk sepuluh tabel `WorkforcePlanning` lewat `HRD-DEC-059`, bukti pada bagian 4. `S-D2` s.d. `S-D5` **tidak** |
| `HRD-PH-008` pengembangan orang | `HRD-DEP-003`, `HRD-DEP-006` | **Ya** |
| `HRD-PH-009` ratchet penamaan saat disentuh | `HRD-DEP-001` | **Ya.** Bukan fase terjadwal; menempel pada task lain |

Tujuh dari sembilan fase boleh jalan. Itu dasar mengapa status modul `PARTIAL` dan bukan
`BLOCKED`. Sejak 3 November 2026 `HRD-PH-007` juga boleh jalan **sebagian**, terbatas pada desain
`S-D1`.

---

## 3. Prasyarat yang bukan dependency modul lain

Tiga hal berikut bukan ketergantungan pada pihak luar, melainkan syarat internal yang harus
dipenuhi sebelum pekerjaan tertentu boleh dimulai.

| ID | Prasyarat | Berlaku untuk | Alasan |
| --- | --- | --- | --- |
| `HRD-PRE-001` | Registry mengenali `Hrd` dan `Wfp` lebih dulu | Pembuatan entity HR baru mana pun, dan setiap ratchet `S-E` | `HRD-DEC-019` dan `QBE-MOD-002` mensyaratkan prefix sudah terdaftar sebelum entity operasional dibuat |
| `HRD-PRE-002` | Audit consumer selesai | Penghapusan alias route lama | `HRD-DEC-016` melarang mematikan route lama sebelum audit selesai dan masa deprecation berakhir. Lihat `HRD-Q-15` |
| `HRD-PRE-003` | Isi tabel 67 entity yang benar-benar belum punya API diketahui | `HRD-PH-007`. **Sejak 3 November 2026 terpenuhi untuk sepuluh tabel `S-D1`** (bagian 4); tetap berlaku untuk `S-D2` s.d. `S-D5` | `HRD-Q-05`. Tanpa ini, penurunan ulang ERD berpotensi menghasilkan migration yang membuang data |

---

## 4. Penutupan prasyarat `S-D1` — 3 November 2026

Bagian ini mencatat bukti yang disyaratkan aturan manifest sebelum `S-D1` (Perencanaan Tenaga
Kerja) boleh naik dari `BLOCKED`. Satu-satunya prasyarat yang menahan `S-D1` adalah `HRD-PRE-003`
/ `HRD-Q-05`: apakah sepuluh tabel `WorkforcePlanning` yang akan diturunkan ulang sudah berisi
data, sehingga perubahan skema berisiko membuang data.

### 4.1 Bukti

| Bukti | Isi | Sumber |
| --- | --- | --- |
| Aplikasi tidak dapat menulis | Sepuluh model `WorkforcePlanning` tidak punya controller, service, maupun DTO. Satu-satunya model yang punya API, `MstWorkforceRequirement`, bukan bagian penurunan ulang | `HRD-TF-022`, `HRD-TF-023` @ BE `396c8363` |
| Database dev kosong | Kesepuluh tabel berisi **0 baris** pada `QuilvianNewDevHamzah`, diperiksa read-only | `HRD-TF-024`, 2 November 2026 |
| Staging dan produksi | Ada di pipeline (`dev-to-staging.yml`, `staging-to-production.yml`), tetapi connection string-nya tidak ada di repository sehingga tidak dapat diperiksa dari sesi audit | `HRD-TF-030` |
| Pernyataan pemilik | Pengguna, sebagai pemilik teknis dan produk (`HRD-DEC-015`), menyatakan tidak ada impor manual atau skrip ke kesepuluh tabel itu | `HRD-DEC-059`, `approved` 2026-11-02 |
| Pengaman sisa risiko | Setiap migration yang mengubah atau membuang kesepuluh tabel **MUST** berhenti bila tabelnya berisi data (`EMPTY_TABLE_GUARD`); rilis produksi lewat gerbang `database_ready` disertai hasil hitung baris (`PRODUCTION_GATE_CHECK`); pengaman **MUST NOT** dilewati (`NO_GUARD_BYPASS`) | `HRD-DEC-059` |

### 4.2 Kesimpulan gerbang

| Field | Isi |
| --- | --- |
| Prasyarat | `HRD-PRE-003` untuk sepuluh tabel `S-D1` |
| Keadaan | **Terpenuhi bersyarat** |
| Syarat yang membuatnya batal | Bila `EMPTY_TABLE_GUARD` berhenti di lingkungan mana pun, `HRD-DEC-059` kembali `draft` (`GUARD_FAILURE_REOPENS_DECISION`), dan `S-D1` kembali `BLOCKED` sampai pemetaan data lama ditambahkan ke desain |
| Akibat pada status | `S-D1` boleh naik menjadi **`READY FOR DESIGN`**. Keputusan bisnis yang dibutuhkan desain sudah lengkap: `HRD-DEC-058` s.d. `HRD-DEC-071`, seluruhnya `approved` |
| Yang **tidak** berubah | `HRD-Q-05` tetap terbuka untuk `S-D2` s.d. `S-D5`. `HRD-BLK-004` tetap berlaku bagi keempat slice itu |

### 4.3 Kebergantungan `S-D1` yang tidak menghalangi desain

Butir berikut lahir dari keputusan bagian 32 decision log. Semuanya memengaruhi **implementasi
atau rilis**, bukan desain. Yang bersifat antar-slice di dalam modul HR dicatat di sini sebagai
rujukan saja; urutan pengerjaannya disusun `plan-module-delivery` pada grafik dependency roadmap,
bukan sebagai `HRD-DEP-*` baru.

| Kebutuhan | Pemilik | Keadaan hari ini | Memengaruhi |
| --- | --- | --- | --- |
| Kolom porsi kerja pada penempatan | 02 Workforce Core | Belum ada (`HRD-TF-031`) | Implementasi hitungan tenaga tersedia (`HRD-DEC-061`) |
| Isi master jenis tenaga yang nyata | 01 Master Data HR | Hanya nilai `Legacy` (`HRD-TF-031`) | Lapisan alih daya dan dokter mitra |
| Penempatan jabatan pegawai yang ada | 02, batch `HRD-DEC-054` | 0 baris (`HRD-TF-028`) | Angka terisi; sebelum terisi tampil "data belum tersedia" |
| Pemeriksaan roster terhadap standar, ringkasan kekurangan tenaga | 06 Penjadwalan (`HRD-DEC-026`, `HRD-DEC-058`) | Belum dirancang | Kontrak baca standar dan ringkasan |
| Requisition dari antrean | 04 Rekrutmen (`S-D2`, `BLOCKED`) | Belum ada API | Diganti tautan manual sementara (`HRD-DEC-066`) |
| Pemetaan peran Finance / Anggaran, Direktur, Recruiter | Administrator — `HRD-DEP-003` | `MAPPING_REQUIRED` | Rilis `T10`, `T12`, dan antrean Recruiter |
| Penarikan otomatis angka beban dari data bed/kunjungan | Health Services — `HRD-DEP-005` | `MISSING` | `LATER SLICE`; rilis pertama memakai isian manual (`HRD-DEC-060`) |
| Nilai kebijakan standar per unit | Pemilik kebijakan rumah sakit (`HRD-Q-01`) dan pemilik klinis | Belum ada | Isi standar sebelum dipakai produksi |
