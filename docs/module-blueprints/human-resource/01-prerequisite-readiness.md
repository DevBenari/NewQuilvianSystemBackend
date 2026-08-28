# Human Resource — Prerequisite Readiness

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Revision | `2` |
| Backend SHA | `ecdc135` |
| Frontend SHA | `2a1cea784` |
| Masukan | `00-interview-decisions.md` rev `3`, `01-existing-capability-map.md` rev `1.1` |

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
| Dibutuhkan oleh | `HRD-PH-002` dan `HRD-PH-008` |
| Dampak pemblokiran | Belum ada bukti bahwa HR dapat meminta pembuatan akun saat onboarding maupun pencabutan akses saat offboarding. Selama itu belum jelas, offboarding tidak dapat dinyatakan tuntas dari sisi keamanan |
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
| `HRD-PH-007` domain yang diturunkan ulang | `HRD-Q-05` | **Tidak** |
| `HRD-PH-008` pengembangan orang | `HRD-DEP-003`, `HRD-DEP-006` | **Ya** |
| `HRD-PH-009` ratchet penamaan saat disentuh | `HRD-DEP-001` | **Ya.** Bukan fase terjadwal; menempel pada task lain |

Tujuh dari sembilan fase boleh jalan. Itu dasar mengapa status modul `PARTIAL` dan bukan
`BLOCKED`.

---

## 3. Prasyarat yang bukan dependency modul lain

Tiga hal berikut bukan ketergantungan pada pihak luar, melainkan syarat internal yang harus
dipenuhi sebelum pekerjaan tertentu boleh dimulai.

| ID | Prasyarat | Berlaku untuk | Alasan |
| --- | --- | --- | --- |
| `HRD-PRE-001` | Registry mengenali `Hrd` dan `Wfp` lebih dulu | Pembuatan entity HR baru mana pun, dan setiap ratchet `S-E` | `HRD-DEC-019` dan `QBE-MOD-002` mensyaratkan prefix sudah terdaftar sebelum entity operasional dibuat |
| `HRD-PRE-002` | Audit consumer selesai | Penghapusan alias route lama | `HRD-DEC-016` melarang mematikan route lama sebelum audit selesai dan masa deprecation berakhir. Lihat `HRD-Q-15` |
| `HRD-PRE-003` | Isi tabel 67 entity yang benar-benar belum punya API diketahui | Seluruh `HRD-PH-007` | `HRD-Q-05`. Tanpa ini, penurunan ulang ERD berpotensi menghasilkan migration yang membuang data |
