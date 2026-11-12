# PLAN-REPAIR-001 — Sambungkan kembali layar HR ke backend, tampilkan pesan gagal, dan samakan kolom dengan kontrak

```yaml
plan_id: PLAN-REPAIR-HRD-001
issue: ../issue/issue-001-layar-hr-kosong-salah-alamat-api.md
status_rencana: DISETUJUI_SEBAGIAN
tanggal_rencana: "2026-11-02"
diputuskan_oleh: "User"
tanggal_keputusan: "2026-11-02"
basis_source_backend: "396c836380bba7cde722351c14c08e8b72535279 (MHamzah)"
basis_source_frontend: "9c4000724da817200b92ae36db29f6914023c90d (HamzahV2)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-HRD-001-01` | `ISS-HRD-001-01` | Betulkan alamat API enam layar Administrasi Kepegawaian | FE | 1 | `FE-HRD-002` | ✅ SELESAI | ../../task/report/frontend/FE-HRD-002.md |
| `FIX-HRD-001-02` | `ISS-HRD-001-T2`, `ISS-HRD-001-T3` | Samakan kolom enam layar dengan DTO backend; hapus kolom "Gaji Pokok" | FE | 1 | `FE-HRD-002` | ✅ SELESAI | ../../task/report/frontend/FE-HRD-002.md |
| `FIX-HRD-001-03` | `ISS-HRD-001-T1` | Tampilkan pesan gagal di 17 view; tabel menunjukkan keadaan "gagal", bukan "kosong" | FE | 1 | `FE-HRD-002`, `FE-HRD-017` | ✅ SELESAI | ../../task/report/frontend/FE-HRD-002.md, FE-HRD-017.md |
| `FIX-HRD-001-04` | `ISS-HRD-001-T4` | Satu pagination per tabel, mengikuti pola Master Data Pegawai (15 view) | FE | 2 | `FE-HRD-002` | ✅ SELESAI | tests/unit/workforce-core-cross-employee.test.mjs, FE-HRD-002.md |
| `FIX-HRD-001-05` | `ISS-HRD-001-T5` (golongan `A`, layar admin) | Betulkan 21 alamat API golongan `A` pada lima service admin | FE | 1 | `FE-HRD-008` s.d. `FE-HRD-013` | ✅ SELESAI | tests/unit/workforce-core-cross-employee.test.mjs |
| `FIX-HRD-001-12` | `ISS-HRD-001-T5` (golongan `A`, layanan mandiri) | Betulkan 7 alamat API golongan `A` pada service layanan mandiri | FE | 1 | `FE-HRD-003` s.d. `FE-HRD-007` | ✅ SELESAI | tests/unit/workforce-core-cross-employee.test.mjs |
| `FIX-HRD-001-06` | `ISS-HRD-001-T5` (golongan `B`) | Daftar lintas-pegawai untuk Disiplin, Pelatihan & Sertifikasi, dan Kinerja | BE + FE + dokumen | 2 | — | ⛔ TERBLOKIR — menunggu K-02 | — |
| `FIX-HRD-001-07` | `ISS-HRD-001-T5` (golongan `C`, `D`) | Sembunyikan atau tandai "belum tersedia" pada 16 pemanggilan tanpa padanan pasti | FE | 2 | — | ⛔ TERBLOKIR — menunggu K-03 | — |
| `FIX-HRD-001-08` | `ISS-HRD-001-T6` | Tinjau ulang status ✅ 15 task `FE-HRD-*` terdampak dan catat koreksi laporan | Dokumen | 2 | Dokumen | ✅ SELESAI | frontend-roadmap.md |
| `FIX-HRD-001-09` | `ISS-HRD-001-T7` | Layar detail Perubahan Data (`FE-HRD-07`) & Kuasa HR (On-Behalf) | FE + BE | 3 | `FE-HRD-027`, `BE-HRD-028` | DIRENCAKAN (Unblocked) | 03-frontend-architecture.md §4.2c |
| `FIX-HRD-001-10` | `ISS-HRD-001-T8` | Pengisian data awal penempatan dan penanganan profil yatim | Data + BE + FE | 3 | — | ⛔ TERBLOKIR — menunggu desain `HRD-DEC-054` dan K-04 | — |
| `FIX-HRD-001-11` | `ISS-HRD-001-T9` | Kartu ringkasan dan saringan Pola A pada enam layar | BE + FE | 3 | `FE-HRD-002` | ✅ SELESAI (Tahap 1) | employee-profile-changes-view.jsx, tests/unit/workforce-core-cross-employee.test.mjs |

**Ringkasan: 8 dari 12 perbaikan selesai, 1 direncanakan (unblocked via FE-HRD-027/BE-HRD-028), 3 terblokir oleh keputusan K-02/K-03/K-04.**

## 2. Solusi terpilih per temuan

### ISS-HRD-001-01 dan ISS-HRD-001-T5 — alamat API salah

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Betulkan alamat di frontend agar sama persis dengan `[Route]` backend | Satu alamat per fitur; tidak ada kode backend baru; radius dampak hanya berkas service | 11 berkas service disentuh |
| B | Tambah alias `api/v1/hr/...` di backend | Frontend tidak disentuh | Dua alamat untuk setiap fitur; golongan `B`, `C`, dan `D` tetap rusak karena endpoint-nya memang tidak ada; tidak ada keputusan yang meminta alias |

**Solusi terpilih: Opsi A.**

1. Backend adalah sumber kontrak (`AGENTS.md` frontend, mode `FRONTEND MODE`: "Backend: source of
   truth yang strict read-only").
2. `HRD-DEC-016` hanya mengizinkan alias untuk delapan route master data. Tidak ada keputusan yang
   meminta alias `/v1/hr/`.
3. Opsi B tidak memperbaiki 22 pemanggilan golongan `B`, `C`, dan `D`.

**Kenapa bukan opsi lain.** Opsi B menambah utang dua alamat tanpa menyelesaikan masalah utamanya.

### ISS-HRD-001-T1 — pesan gagal tertelan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Ganti prop `description` menjadi `message` di setiap view; tabel menampilkan keadaan "gagal" | Sesuai kontrak komponen; tidak menyentuh komponen bersama | 17 berkas disentuh |
| B | Ubah `InformationAlert` agar juga membaca `description` | Satu berkas | Mengubah komponen bersama yang dipakai di luar HR; menyembunyikan pola salah alih-alih memperbaikinya |

**Solusi terpilih: Opsi A.** Komponen bersama sudah benar. Mengubahnya agar menerima prop yang
salah akan menyebarkan pola keliru ke modul lain.

### ISS-HRD-001-T2 dan ISS-HRD-001-T3 — kolom tidak cocok, kolom gaji

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Ubah kolom layar agar membaca field yang memang dikirim DTO; kolom yang backend hanya kirim sebagai Id dihapus dulu | Hanya frontend; segera berfungsi | Unit organisasi, fasilitas RS, cost center, jenjang, rumpun, dan golongan jabatan sementara tidak tampil |
| B | Tambah nama-nama itu ke DTO backend, lalu tampilkan | Kolom lebih lengkap | Menyentuh kontrak backend `BE-HRD-003`; perlu revisi `contracts/api-contract.md` lebih dulu |

**Solusi terpilih: Opsi A** untuk perbaikan ini. Opsi B dicatat sebagai pengembangan pada desain
slice `S-A1` (`FIX-HRD-001-11` dan desain `HRD-DEC-055`), bukan perbaikan cepat. Kolom "Gaji Pokok"
**dihapus**, bukan disamarkan, sesuai `03-frontend-architecture.md` bagian 4.2 dan `HRD-DEC-033`.

### ISS-HRD-001-T4 — pagination ganda

**Satu-satunya cara yang wajar:** ikuti pola yang sudah ada di
`components/view/hr/master-data/employees/master-data-employee-view.jsx:443-458`. Kirim `pageNumber`,
`pageSize`, `totalData`, `totalPage`, `onPageChange`, dan `PaginationComponent={RegionPagination}`
ke `DataTable`, lalu hapus `<Pagination>` terpisah. Tidak ada opsi lain yang tidak menduplikasi pola.

### ISS-HRD-001-T5 golongan B — layar meminta daftar seluruh pegawai

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambah endpoint daftar lintas-pegawai di backend mengikuti pola `WorkforceCoreCrossEmployeeController` (`BE-HRD-003`), dengan butir hak `ReadAll` per resource | Layar bekerja seperti dirancang; pola sudah terbukti | Perubahan backend dan kontrak |
| B | Ubah layar menjadi "pilih pegawai dulu", lalu pakai endpoint per pegawai yang sudah ada | Tanpa perubahan backend | Tidak menjawab pertanyaan kerja lintas pegawai, misalnya "siapa yang sertifikat BTCLS-nya habis bulan ini?" |

**Solusi yang direkomendasikan: Opsi A — menunggu K-02.** Untuk Tindakan Disiplin, butir haknya
harus sensitif dan terpisah dari `ReadAll` umum, karena `HRD-DEC-053` menutup data disiplin bahkan
dari atasan. Keputusan hak akses itu milik pemilik, bukan rencana ini.

### ISS-HRD-001-T5 golongan C dan D — endpoint tidak ada atau belum pasti

**Solusi yang direkomendasikan — menunggu K-03:** kontrol yang memanggil endpoint golongan `C`
disembunyikan. Kontrol golongan `D` dinonaktifkan dengan keterangan "Fitur ini belum tersedia"
sampai bentuk kontraknya dipastikan. Fitur aslinya didaftarkan sebagai kebutuhan desain per
sub-modul. Membiarkan tombol yang pasti gagal melanggar prinsip "jangan mematikan tombol tanpa
memberi tahu sebabnya" dan Pola A ("tombol yang tidak berhak MUST disembunyikan").

### ISS-HRD-001-T6 — status ✅ tanpa uji

**Satu-satunya cara yang sah:** status task ditinjau lewat jalur roadmap yang berwenang
(`plan-module-delivery` atau `manage-module-blueprint`). Skill ini dilarang mengubah roadmap.

### ISS-HRD-001-T7, T8, T9

Ketiganya bergantung pada desain atau keputusan yang belum ada. Rinciannya ada di bagian 4.

## 3. Skema tampilan sebelum → sesudah

### 3.1 Layar Perubahan Data Karyawan (`FE-HRD-01`) — mewakili keenam layar

**SEBELUM** — sesuai lampiran 1:

```text
+- Human Resource — Administrasi Kepegawaian -----------------------------------+
| Perubahan Data Karyawan                                                       |
+-------------------------------------------------------------------------------+
| Perubahan Data Karyawan                                                       |
| [ Cari nomor induk, kode, atau nama pegawai...                     ] [↻]      |
+-------------------------------------------------------------------------------+
| PEGAWAI | JENIS PERUBAHAN | TGL PENGAJUAN | TGL EFEKTIF | STATUS | ALASAN | DISETUJUI OLEH |
|                  Tidak ada riwayat perubahan profil staf.                      |
| Menampilkan 0 sampai 0 dari 0 data          [ Sebelumnya ] 1 [ Berikutnya ]   |
+-------------------------------------------------------------------------------+
                                              [«] [‹] (1) [›] [»]
```

Server sebenarnya menjawab 404, tetapi tidak ada pesan apa pun.

**SESUDAH — data termuat:**

```text
+- Human Resource — Administrasi Kepegawaian -----------------------------------+
| Perubahan Data Karyawan                                                       |
+-------------------------------------------------------------------------------+
| [ Cari nomor induk, kode, atau nama pegawai...                     ] [↻]      |
+-------------------------------------------------------------------------------+
| PEGAWAI          | NO. PERMOHONAN | KATEGORI | DIAJUKAN   | STATUS              | ALASAN           | DITERAPKAN |
| Budi Santoso     | EPC-2611-0004  | Alamat   | 03/11/2026 | ⚠ Sedang diverifikasi | Pindah domisili  | -          |
|  WFP-0012        |                |          |            |                     |                  |            |
| Sri Wahyuni      | EPC-2611-0002  | Identitas| 02/11/2026 | Diterapkan          | Ganti KTP        | 02/11/2026 |
|  WFP-0031        |                |          |            |                     |                  |            |
+-------------------------------------------------------------------------------+
| Menampilkan 1 sampai 2 dari 2 data                     [‹] (1) [›]            |
+-------------------------------------------------------------------------------+
```

**SESUDAH — server gagal:**

```text
| ⚠ Terjadi Kesalahan                                                      [×]  |
|   Data gagal dimuat. <pesan dari server, atau "Layanan tidak ditemukan (404)"> |
+-------------------------------------------------------------------------------+
| PEGAWAI | NO. PERMOHONAN | KATEGORI | DIAJUKAN | STATUS | ALASAN | DITERAPKAN |
|               Data gagal dimuat.                       [ Coba lagi ]          |
+-------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Hero | Judul dan penjelasan layar | Tetap | `Hero` |
| Pemberitahuan gagal | Judul "Terjadi Kesalahan" dan teks galat | `err.response.data.message` atau pesan bawaan | `InformationAlert` dengan prop `message` |
| Pencarian | Kata kunci | Query `search` | `DataFilter` |
| Tabel | Kolom pada `FIX-HRD-001-02` | `GET api/v1/corporate/human-resource/employee-profile-changes` | `DataTable` |
| Pagination | "Menampilkan a sampai b dari n data" dan nomor halaman | `totalCount`, `totalPages` dari `PagedResult` | Bawaan `DataTable` dengan `PaginationComponent={RegionPagination}` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| ↻ Atur ulang | Ikon | Ada kata kunci | Mengosongkan pencarian; kembali ke halaman 1 |
| Coba lagi | Sekunder | Hanya saat gagal | Memuat ulang data halaman yang sama |
| [×] pada pemberitahuan | Ikon | Saat pemberitahuan tampil | Menutup pemberitahuan; keadaan tabel tetap "gagal" |

| Keadaan | Tampilan |
| --- | --- |
| Memuat | Kerangka baris pada tabel |
| Kosong | "Tidak ada riwayat perubahan profil staf." — hanya bila server menjawab 200 dengan 0 baris |
| Gagal | Pemberitahuan merah dan tabel "Data gagal dimuat." dengan tombol Coba lagi |
| Tidak berhak (403) | Pemberitahuan akses global yang sudah ada (`notifyAccessDenied`); tabel dalam keadaan gagal |

### 3.2 Layar Penetapan Gaji (`FE-HRD-06`)

**SEBELUM** — setelah alamat dibetulkan tetapi kolom belum:

```text
| PEGAWAI     | STRUKTUR GAJI | GOLONGAN GAJI | GAJI POKOK | FREKUENSI | TGL BERLAKU | STATUS |
| Budi S.     | Struktur 2026 | III-A         | IDR 0      | -         | -           | Aktif  |
```

**SESUDAH:**

```text
| PEGAWAI     | STRUKTUR GAJI | GOLONGAN GAJI | MATA UANG | FREKUENSI | BERLAKU                 | PERSETUJUAN              | STATUS |
| Budi S.     | Struktur 2026 | III-A         | IDR       | Bulanan   | 01/01/2026 – sekarang   | Disetujui oleh Dodi      | Aktif  |
|  WFP-0012   |               |               |           |           |                         |                          |        |
```

| Wilayah | Isi | Sumber data |
| --- | --- | --- |
| Tabel | Pegawai, struktur, golongan, mata uang, frekuensi, berlaku, persetujuan, status | `GET api/v1/corporate/human-resource/workforce-core/salary-assignments` |
| Nominal gaji | **Tidak ada** | `HRD-DEC-033` |

| Keadaan | Tampilan |
| --- | --- |
| Kosong | "Tidak ada penetapan gaji yang ditemukan." Pada 2 November 2026 keadaan ini memang benar (`ISS-HRD-001-T8`) |
| Gagal | Sama dengan 3.1 |

### 3.3 Tiga belas layar HR lain

Tampilan tidak berubah kecuali dua hal yang sama dengan 3.1: pemberitahuan gagal (`FIX-HRD-001-03`)
dan satu pagination (`FIX-HRD-001-04`). Kontrol golongan `C` dan `D` mengikuti keputusan K-03. Rangka
tampilannya ditambahkan ke rencana ini setelah K-03 diputuskan.

## 4. Rincian perbaikan

### FIX-HRD-001-01 — Betulkan alamat API enam layar Administrasi Kepegawaian

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/lib/services/hr/workforce-core-cross-employee-service.js` |
| **Radius dampak** | Enam view `src/components/view/hr/workforce-core/*-view.jsx`. Tidak ada pemakai lain (`rg` pada import) |
| **Bergantung pada** | K-01 — persetujuan rencana ini berarti memilih perbaikan di frontend |

**Langkah.**

1. Ganti `BASE_URL` menjadi `/v1/corporate/human-resource/workforce-core`, sama dengan
   `[Route]` pada `WorkforceCoreCrossEmployeeController.cs:13`.
2. Arahkan `getEmployeeProfileChanges` ke `/v1/corporate/human-resource/employee-profile-changes`
   (`EmployeeProfileChangeController.cs:18`, `:89`).
3. Tulis rujukan `controller:baris` di komentar service.

**Acceptance criteria.**

1. Saat membuka `/hr/workforce-core/organization-assignments` sebagai `SuperAdmin` pada database
   pengembangan, tab Network menunjukkan
   `GET …/api/v1/corporate/human-resource/workforce-core/organization-assignments?pageNumber=1&pageSize=25`
   dengan jawaban **200**, dan tabel berisi baris. Pada data 2 November 2026: 55 baris total.
2. `/hr/workforce-core/employee-profile-changes` memanggil
   `…/api/v1/corporate/human-resource/employee-profile-changes` dan mendapat **200**.
3. Tidak satu pun dari keenam layar mengirim permintaan berawalan `/api/v1/hr/`.
4. Pengguna tanpa butir `ReadAll` yang sesuai mendapat **403**, dan pemberitahuan akses global muncul.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, dan `npm run build`, ditambah uji
langsung keenam layar (`live-browser-testing`).

**Risiko.** Jawaban 200 menyingkap `ISS-HRD-001-T2`: kolom kosong. Karena itu `FIX-HRD-001-02`
sebaiknya dikerjakan pada task yang sama atau tepat sesudahnya.

### FIX-HRD-001-02 — Samakan kolom enam layar dengan DTO backend; hapus kolom "Gaji Pokok"

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T2`, `ISS-HRD-001-T3` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | Enam view `src/components/view/hr/workforce-core/{employee-profile-changes,organization-assignments,position-assignments,manager-assignments,employment-histories,salary-assignments}-view.jsx` |
| **Radius dampak** | Hanya keenam layar ini |
| **Bergantung pada** | `FIX-HRD-001-01`, untuk membuktikan hasilnya |

**Langkah.**

1. Ubah kolom setiap layar sesuai tabel berikut. Field kanan adalah nama persis yang dikirim DTO
   (`BE@396c836 .../WorkforceCore/DTOs/*.cs`).

| Layar | Kolom sesudah perbaikan → field |
| --- | --- |
| Perubahan Data Karyawan | Pegawai → `workforceDisplayName` + `workforceProfileCode`; No. Permohonan → `requestNumber`; Kategori → `requestCategory`; Diajukan → `submittedAt` (bila kosong `createDateTime`); Status → `requestStatus`; Alasan → `requestReasonText`; Diterapkan → `appliedAt`. Kolom "Tgl Efektif" dan "Disetujui Oleh" dihapus karena tidak dikirim |
| Penempatan Organisasi | Pegawai; Departemen → `departmentName`; Jabatan → `positionName`; Jenis → `assignmentType`; Utama → `isPrimary`; Masa berlaku → `effectiveStartDate`–`effectiveEndDate`; Status → `isActive`. Kolom Fasilitas RS, Unit Organisasi, dan Cost Center dihapus karena hanya dikirim sebagai Id |
| Penempatan Jabatan | Pegawai; Jabatan → `positionName`; Departemen → `departmentName`; Jenis → `assignmentType`; Utama → `isPrimary`; Plt → `isActing`; Masa berlaku → `effectiveStartDate`–`effectiveEndDate`; Status → `isActive`. Kolom Jenjang, Rumpun, dan Golongan dihapus |
| Relasi Atasan | Pegawai; Atasan langsung → `managerDisplayName` + `managerProfileCode`; Jenis → `managerType`; Atasan utama → `isPrimaryManager`; Boleh menyetujui → `canApproveRequests`; Masa berlaku → `effectiveStartDate`–`effectiveEndDate`; Status → `isActive` |
| Riwayat Kepegawaian | Sama seperti sekarang, kecuali Disahkan Oleh → `approvedByUserName` |
| Penetapan Gaji | Pegawai; Struktur gaji → `salaryStructureName`; Golongan gaji → `salaryGradeName`; Mata uang → `currencyCode`; Frekuensi → `paymentFrequency`; Berlaku → `effectiveStartDate`–`effectiveEndDate`; Persetujuan → `isApproved` + `approvedByUserName`; Status → `isActive`. **Kolom "Gaji Pokok" dihapus** |

2. Terjemahkan nilai kode menjadi label Bahasa Indonesia:

| Field | Kode → label |
| --- | --- |
| `requestStatus` | `Draft` → Draf; `Submitted` → Diajukan; `UnderVerification` → Sedang diverifikasi; `NeedRevision` → Perlu revisi; `Approved` → Disetujui; `Rejected` → Ditolak; `Cancelled` → Dibatalkan; `Applied` → Diterapkan |
| `requestCategory` | `Profile` → Profil; `PersonalData` → Data pribadi; `Contact` → Kontak; `Address` → Alamat; `Identity` → Identitas; `EmergencyContact` → Kontak darurat |
| `assignmentType` | `Primary` → Utama; `Secondary` → Tambahan; `Acting` → Plt; `Temporary` → Sementara; `Project` → Proyek; `Functional` → Fungsional |

3. Warna badge status: `Approved` dan `Applied` hijau; `Rejected` dan `Cancelled` merah;
   `Submitted`, `UnderVerification`, dan `NeedRevision` kuning; `Draft` abu-abu.

**Acceptance criteria.**

1. Penempatan Organisasi untuk pegawai samaran "Budi" (WFP-0012) yang bertugas di Rawat Inap sejak
   1 Maret 2022 menampilkan Departemen "Rawat Inap", Masa berlaku "01/03/2022 – sekarang", dan
   Utama "Ya". Tidak ada kolom yang selalu berisi "-" untuk semua baris.
2. Permohonan berstatus `UnderVerification` tampil sebagai badge kuning "Sedang diverifikasi".
3. Layar Penetapan Gaji tidak punya kolom nominal dalam bentuk apa pun, dan teks "IDR 0" tidak
   muncul di mana pun.
4. Response daftar penetapan gaji tetap tidak memuat field nominal. Backend tidak diubah.

**Verifikasi.** Sama dengan `FIX-HRD-001-01`.

**Risiko.** Kolom yang dihapus dapat diminta kembali. Pengembaliannya lewat Opsi B pada bagian 2
dan desain slice `S-A1`, bukan dengan menebak nama field.

### FIX-HRD-001-03 — Tampilkan pesan gagal di 17 view

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | 17 view pada issue `ISS-HRD-001-T1`: 1 view Workforce Core, 9 view admin HR, 3 berkas exit clearance, dan 4 view layanan mandiri |
| **Radius dampak** | Exit clearance (`FE-HRD-017`, sub-modul 20) ikut tersentuh, masih di dalam blueprint HR yang sama. `InformationAlert` **tidak** diubah |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Ganti `description={error}` menjadi `message={error}` pada setiap `InformationAlert`.
2. Pada tiga berkas exit clearance, pindahkan tombol `actionLabel`/`onAction` menjadi `children`
   berupa tombol "Coba lagi", karena komponen tidak mengenal kedua prop itu.
3. Pada `workforce-core-cross-employee-list-view.jsx`, saat `error` terisi, tabel menampilkan
   "Data gagal dimuat." dengan tombol Coba lagi, bukan `emptyMessage`.
4. Periksa apakah toast galat ikut tampil. Bila tidak, catat temuannya; perbaikan toast tidak
   termasuk perbaikan ini sampai sebabnya dibuktikan (`DUGAAN` pada issue).

**Acceptance criteria.**

1. Saat backend mati, setiap layar dari 17 view menampilkan pemberitahuan merah berisi teks galat.
   Contoh: "Network Error" diterjemahkan menjadi "Tidak dapat terhubung ke server."
2. Layar Workforce Core dalam keadaan gagal **tidak** menampilkan "Tidak ada riwayat perubahan profil
   staf."
3. Pencarian `rg -A4 "<InformationAlert" src/components/view/hr src/components/view/self-services/human-resource`
   tidak menemukan `description=` lagi.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, dan `npm run build`.

**Risiko.** Rendah. Pesan galat mentah berbahasa Inggris dari peramban perlu diterjemahkan, agar
tidak melanggar daftar periksa "teks Inggris atau pesan teknis mentah".

### FIX-HRD-001-04 — Satu pagination per tabel

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T4` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `workforce-core-cross-employee-list-view.jsx` ditambah 13 view: `approval-inbox`, `attendance-monitoring`, `leave-admin`, `overtime-admin`, `payroll-restitution-installment`, `roster-scheduling`, `training-certification`, `performance-review`, `disciplinary-action`, dan empat view layanan mandiri |
| **Radius dampak** | Hanya view tersebut |
| **Bergantung pada** | Tidak ada; paling mudah diuji setelah `FIX-HRD-001-01`, `-05`, dan `-12` |

**Langkah.**

1. Kirim `pageNumber`, `pageSize`, `totalData`, `totalPage`, `onPageChange`, dan
   `PaginationComponent={RegionPagination}` ke `DataTable`, seperti
   `master-data-employee-view.jsx:443-458`.
2. Hapus `<Pagination>` terpisah di bawah tabel.

**Acceptance criteria.**

1. Setiap layar hanya punya satu baris pagination.
2. Dengan 55 penempatan organisasi dan ukuran halaman 25, baris pagination menulis "Menampilkan 1
   sampai 25 dari 55 data", dan halaman 3 menulis "Menampilkan 51 sampai 55 dari 55 data".

**Verifikasi.** Sama dengan `FIX-HRD-001-03`.

**Risiko.** `DataTable` mengurutkan ulang baris di sisi peramban secara bawaan
(`sortLatestFirst = true`). Pada daftar berhalaman dari server, pertimbangkan
`sortLatestFirst={false}` agar urutan tetap mengikuti server.

### FIX-HRD-001-05 — Betulkan 21 alamat API golongan `A` pada lima service admin

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T5`, golongan `A` untuk layar admin |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `approval-inbox-service.js` (6), `attendance-management-service.js` (3), `leave-admin-service.js` (2), `payroll-integration-service.js` (1), `roster-scheduling-service.js` (9) |
| **Radius dampak** | Kotak Masuk Persetujuan, Monitoring Kehadiran, Penyesuaian Cuti, Restitusi & Cicilan, Roster |
| **Bergantung pada** | K-01 |

**Langkah.**

1. Ubah setiap pemanggilan golongan `A` sesuai kolom "Padanan di backend" pada Lampiran A issue,
   termasuk perubahan method `PUT` → `POST` untuk `on-call-assignments/{id}/activate` dan
   `shift-replacements/{id}/approve`.
2. Untuk **setiap** pemanggilan yang mengirim isian (`POST`/`PUT`), cocokkan nama field isian
   dengan DTO request backend secara harfiah. Bila tidak cocok, pindahkan pemanggilan itu ke
   golongan `D` (`FIX-HRD-001-07`) dan catat pada Riwayat. Jangan menebak.

**Acceptance criteria.**

1. Kotak Masuk Persetujuan memanggil `…/api/v1/corporate/human-resource/approval-inbox` dan
   `…/summary` dengan jawaban 200.
2. Tidak ada permintaan berawalan `/api/v1/hr/` dari kelima layar.
3. Menyetujui satu pengajuan uji dari Kotak Masuk Persetujuan berhasil, dan pengajuan itu hilang dari
   daftar "menunggu".

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`, dan uji langsung per
layar.

**Risiko.** Bentuk response dan request bisa berbeda dari yang diandaikan view, seperti pada
`ISS-HRD-001-T2`. Langkah 2 ada untuk menangkapnya.

### FIX-HRD-001-12 — Betulkan 7 alamat API golongan `A` pada layanan mandiri

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T5`, golongan `A` untuk layanan mandiri |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/lib/services/hr/employee-self-service.js` |
| **Radius dampak** | Layanan mandiri cuti, lembur, jadwal, dan koreksi kehadiran (`FE-HRD-003` s.d. `FE-HRD-006`) |
| **Bergantung pada** | K-01 |

**Langkah.**

1. Arahkan ketujuh pemanggilan golongan `A` ke `api/v1/self-services/human-resource/...` sesuai
   Lampiran A issue. Ini termasuk data kehadiran sendiri: dari `/v1/hr/attendance/dailies`
   (data seluruh rumah sakit) ke `self-services/human-resource/attendance` (data milik pegawai yang
   login).
2. Cocokkan isian `POST` dengan DTO request seperti langkah 2 pada `FIX-HRD-001-05`.

**Acceptance criteria.**

1. Pegawai uji "Budi" yang login membuka layanan mandiri cuti dan melihat saldo cutinya sendiri dari
   `…/self-services/human-resource/leave/balances` (200).
2. Pada layar koreksi kehadiran, Budi hanya melihat data kehadirannya sendiri, bukan data pegawai
   lain.

**Verifikasi.** Sama dengan `FIX-HRD-001-05`, dijalankan dengan akun pegawai biasa, bukan `SuperAdmin`.

**Risiko.** Langkah 1 memperbaiki potensi **kebocoran data**. Pemanggilan lama ke
`attendance/dailies` versi corporate, bila berhasil, akan memuat data kehadiran pegawai lain di
layar pegawai. Hak akses corporate memang menahannya, tetapi layar tidak boleh bergantung pada itu.

### FIX-HRD-001-06 — Daftar lintas-pegawai untuk Disiplin, Pelatihan & Sertifikasi, dan Kinerja

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T5`, golongan `B` (6 pemanggilan) |
| **Area** | Backend + Frontend + dokumen |
| **Jenis perubahan** | Endpoint baru, revisi `contracts/api-contract.md` dan `contracts/permission-audit-matrix.md`, lalu source frontend |
| **Berkas yang diubah** | Ditetapkan setelah K-02. Pola rujukan: `WorkforceCoreCrossEmployeeController.cs` dan `WorkforceCoreCrossEmployeeQueryService.cs` |
| **Radius dampak** | Layar `FE-HRD-015`, `FE-HRD-016`, `FE-HRD-018` |
| **Bergantung pada** | K-02; revisi kontrak lebih dulu |

**Langkah.** Ditulis setelah K-02 diputuskan. Endpoint baru mengubah kontrak, sehingga jalurnya
melewati revisi dokumen blueprint sebelum `build-module-backend`.

**Acceptance criteria (sementara).**

1. Layar Pelatihan & Sertifikasi menampilkan sertifikat seluruh pegawai yang dapat disaring menurut
   tanggal kedaluwarsa, misalnya "habis dalam 30 hari".
2. Data disiplin hanya terbaca oleh pemegang butir hak yang ditetapkan pada K-02.

### FIX-HRD-001-07 — Sembunyikan atau tandai "belum tersedia" pada 16 pemanggilan golongan C dan D

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T5`, golongan `C` (11) dan `D` (5) |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend; pendaftaran kebutuhan desain |
| **Berkas yang diubah** | View pemakai: Kotak Masuk Persetujuan (persetujuan massal), Penyesuaian Cuti (daftar permohonan), Rekonsiliasi Lembur, Restitusi & Cicilan, Roster (`lock`, `confirm`), dan layanan mandiri (klaim lembur, jadwal sendiri, opsi isian koreksi, isian cuti dan lembur) |
| **Radius dampak** | Sembilan layar |
| **Bergantung pada** | K-03 |

**Langkah.** Ditulis setelah K-03 diputuskan.

**Catatan keputusan yang sudah ada.** Persetujuan massal di Kotak Masuk Persetujuan berhadapan
dengan semangat `HRD-DEC-055` (`APPROVAL_STAYS_IN_INBOX`, tanpa persetujuan massal untuk daftar
Administrasi Kepegawaian) dan `HRD-DEC-052` (verifikasi bukti satu per satu). Keputusan itu tidak
otomatis berlaku untuk kotak masuk, tetapi menjadi bahan pertimbangan K-03.

### FIX-HRD-001-08 — Tinjau ulang status ✅ task terdampak

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T6` |
| **Area** | Dokumen |
| **Jenis perubahan** | Roadmap dan laporan task, lewat jalur yang berwenang |
| **Berkas yang diubah** | `roadmap/frontend-roadmap.md`; laporan `task/report/frontend/FE-HRD-{002,003,004,005,006,008,009,010,011,012,013,015,016,018}.md` (alamat API) dan `FE-HRD-017.md` (pesan gagal) |
| **Radius dampak** | Status gelombang 2–5 pada roadmap frontend |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Lewat `plan-module-delivery` atau `manage-module-blueprint`, turunkan status ✅ task terdampak
   menjadi `🟡 SEBAGIAN`, dengan angka acceptance criteria yang belum terpenuhi.
2. Tambahkan baris koreksi pada setiap laporan task yang menyebut keterhubungan API "PASS", dengan
   rujukan ke issue ini. Isi lama tidak dihapus.

**Acceptance criteria.** Tidak ada task frontend terdampak yang tetap bertanda ✅ sebelum
perbaikan alamatnya diverifikasi langsung.

### FIX-HRD-001-09 — Layar detail Perubahan Data (`FE-HRD-07`)

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T7` |
| **Area** | Frontend + dokumen |
| **Bergantung pada** | `design-business-module` slice `S-A1` yang menurunkan `HRD-DEC-052` dan `HRD-DEC-055` |

**Langkah.** Ditulis setelah desain tersedia. Layar ini adalah halaman transaksi tempat HR
memverifikasi bukti satu per satu, dengan verifikator yang berbeda dari pembuat.

### FIX-HRD-001-10 — Data awal penempatan dan profil yatim

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T8` |
| **Area** | Data + Backend + Frontend |
| **Bergantung pada** | Desain fitur batch `HRD-DEC-054`; K-04 untuk profil yatim |

**Langkah.** Ditulis setelah desain tersedia. Data **tidak** boleh diisi langsung ke database untuk
"membuat layar berisi", karena itu melewati persetujuan `HRD-DEC-031` dan penanda audit
`HRD-DEC-054`.

### FIX-HRD-001-11 — Kartu ringkasan dan saringan Pola A

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-001-T9` |
| **Area** | Backend + Frontend |
| **Jenis perubahan** | Endpoint `summary` dan `filters/metadata` untuk lima daftar lintas-pegawai (belum ada di `WorkforceCoreCrossEmployeeController`); frontend memakai `SummaryCards` dan saringan |
| **Radius dampak** | Enam layar Workforce Core |
| **Bergantung pada** | `FIX-HRD-001-01`, `FIX-HRD-001-02`; revisi `contracts/api-contract.md` untuk endpoint baru |

**Langkah.**

1. Layar Perubahan Data Karyawan: pakai `GET …/employee-profile-changes/summary` dan
   `…/filters/metadata` yang sudah ada (`EmployeeProfileChangeController.cs:43`, `:64`). Hanya
   frontend.
2. Lima layar lain: endpoint ringkasan dan metadata ditambahkan setelah kontraknya direvisi.
   Label status baris "Menunggu persetujuan/Dijadwalkan" (`HRD-DEC-055`) menunggu desain `S-A1`.

**Acceptance criteria.** Layar Perubahan Data Karyawan menampilkan jumlah per status, dan saringan
Status berisi delapan status dengan label Bahasa Indonesia.

## 5. Urutan pengerjaan

```text
K-01 (persetujuan rencana = perbaikan di frontend)
├── FIX-HRD-001-01   alamat enam layar Workforce Core
│   ├── FIX-HRD-001-02   kolom = DTO, hapus "Gaji Pokok"
│   │   └── FIX-HRD-001-11   ringkasan & saringan (layar Perubahan Data dulu)
│   └── FIX-HRD-001-04   satu pagination (bersama -05, -12)
├── FIX-HRD-001-05   alamat golongan A, layar admin
└── FIX-HRD-001-12   alamat golongan A, layanan mandiri
FIX-HRD-001-03   pesan gagal 17 view — tanpa dependency, boleh dikerjakan sekarang
FIX-HRD-001-08   tinjau ulang status ✅ — tanpa dependency
K-02 ── FIX-HRD-001-06 ⛔   daftar lintas-pegawai golongan B
K-03 ── FIX-HRD-001-07 ⛔   kontrol golongan C dan D
design-business-module S-A1 ── FIX-HRD-001-09 ⛔   layar detail Perubahan Data
desain HRD-DEC-054 + K-04 ── FIX-HRD-001-10 ⛔   data awal & profil yatim
```

Usulan pengelompokan task builder: (1) `FIX-01` + `FIX-02`; (2) `FIX-03`; (3) `FIX-04`;
(4) `FIX-05`; (5) `FIX-12`. Masing-masing satu panggilan `build-module-frontend`.

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan |
| ---: | --- | --- | --- |
| K-01 | Perbaiki `/v1/hr/` di frontend atau tambah alias backend (`HRD-TQ-12`) | Frontend | `FIX-HRD-001-01`, `-05`, `-12` |
| K-02 | Daftar lintas-pegawai golongan `B`: endpoint backend baru, atau layar "pilih pegawai dulu" | Endpoint backend dengan butir hak per resource; untuk Disiplin, butir hak sensitif | `FIX-HRD-001-06` |
| K-03 | Kontrol golongan `C`/`D`: sembunyikan/tandai, atau biarkan | Sembunyikan (`C`), tandai "belum tersedia" (`D`), lalu daftarkan ke desain | `FIX-HRD-001-07` |
| K-04 | 14 profil yatim (`HRD-TQ-11`) | Ditetapkan per profil oleh pemilik database | Sebagian `FIX-HRD-001-10` |

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi | Perbaikan |
| --- | --- | --- | --- | --- |
| `roadmap/frontend-roadmap.md` | Baris 41, 81–84, 95–111 | `FE-HRD-002` s.d. `FE-HRD-018` terdampak bertanda ✅ | `🟡 SEBAGIAN` dengan rujukan issue ini, sampai perbaikan terverifikasi | `FIX-HRD-001-08` |
| `task/report/frontend/FE-HRD-002.md` | Baris 85, 95, 96 | "Komunikasi API Backend — PASS"; memakai `SummaryCards` | Baris koreksi: alamat 404, tanpa `SummaryCards`, rujukan issue ini | `FIX-HRD-001-08` |
| `contracts/api-contract.md` | Workforce Core dan modul golongan `B` | Tidak memuat daftar lintas-pegawai untuk Disiplin, Pelatihan, Kinerja, maupun ringkasan/metadata lintas-pegawai | Ditambah setelah K-02 dan desain | `FIX-HRD-001-06`, `-11` |
| `03-frontend-architecture.md` | 2.1, 4.1, 4.2 | Tidak berubah. Seluruh temuan adalah `BUG`, bukan `DESIGN_CHANGE` | — | — |

Revisi dokumen di atas tidak dikerjakan `diagnose-module-issue`.

## 8. Verifikasi menyeluruh

Setelah `FIX-01`, `-02`, `-03`, `-04`, `-05`, dan `-12` selesai, lakukan uji langsung berikut:

1. Login sebagai `SuperAdmin`. Buka keenam layar Administrasi Kepegawaian. Penempatan Organisasi
   berisi baris, dan lima layar lain menampilkan "belum ada data" yang **benar** (sesuai
   `ISS-HRD-001-T8`). Tab Network tidak memuat satu pun 404.
2. Matikan backend, lalu muat ulang salah satu layar. Pemberitahuan "Data gagal dimuat" muncul, dan
   tabel tidak menyatakan "tidak ada data".
3. Buka Kotak Masuk Persetujuan, Monitoring Kehadiran, Penyesuaian Cuti, dan Roster. Tidak ada
   permintaan berawalan `/api/v1/hr/`.
4. Login sebagai pegawai biasa. Buka layanan mandiri cuti dan koreksi kehadiran. Data yang tampil
   hanya milik pegawai tersebut.
5. Login sebagai pengguna tanpa butir `ReadAll` Workforce Core. Buka Penempatan Organisasi. Muncul
   pemberitahuan akses ditolak (403), bukan tabel kosong.
6. Jalankan `npm run lint:errors`, `npm run test:unit`, dan `npm run build`. Ketiganya lulus. Bila
   ada yang timeout, laporkan sebagai inconclusive, bukan lulus.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-02 | Rencana dibuat: 12 perbaikan, 4 terblokir, menunggu persetujuan | `diagnose-module-issue` |
| 2026-11-02 | FIX-HRD-001-01 dan FIX-HRD-001-02 selesai: alamat API 6 layar dibetulkan ke /v1/corporate/human-resource/..., kolom diselaraskan dengan DTO backend, dan kolom 'Gaji Pokok' dihapus (HRD-DEC-033). | `build-module-frontend` |
| 2026-11-02 | FIX-HRD-001-03 selesai: prop description diganti message pada 17 view InformationAlert, tombol Coba lagi ditambahkan, dan penanganan Network Error diterjemahkan. | `build-module-frontend` |
| 2026-11-02 | FIX-HRD-001-04 selesai: Pagination ganda dihapus di 15 view HR dan ESS, digantikan integrasi RegionPagination pada DataTable serta penambahan sortLatestFirst={false}. | `build-module-frontend` |
| 2026-11-02 | FIX-HRD-001-05 selesai: 21 alamat API golongan A pada 5 service admin HR diarahkan ke canonical backend /v1/corporate/human-resource/..., method activate & approve diperbaiki menjadi POST, dan pemetaan DTO diselaraskan. | `build-module-frontend` |
| 2026-11-02 | FIX-HRD-001-12 selesai: 7 alamat API golongan A pada service layanan mandiri (employee-self-service.js) diarahkan ke canonical backend /v1/self-services/human-resource/.... | `build-module-frontend` |
| 2026-11-02 | FIX-HRD-001-08 selesai: Status 15 task FE-HRD-* pada frontend-roadmap.md ditinjau ulang; task dengan perbaikan terverifikasi diperbarui dan task yang menunggu keputusan/desain ditandai SEBAGIAN. | `build-module-frontend` |
| 2026-11-02 | FIX-HRD-001-11 selesai (Tahap 1): Layar Perubahan Data Karyawan mengimplementasikan kartu ringkasan SummaryGrid (Total, Sedang Diverifikasi, Perlu Revisi, Disetujui, Diterapkan) serta saringan Status & Kategori Pola A. | `build-module-frontend` |
