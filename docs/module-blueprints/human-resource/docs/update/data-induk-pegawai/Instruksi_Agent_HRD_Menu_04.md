# MASTER PROMPT — IMPLEMENTASI LANGSUNG HRD MENU 04: DATA INDUK PEGAWAI

**Jenis:** Satu dokumen master instruksi eksekusi Agent Developer Fullstack Quilvian V2; bukan laporan hasil implementasi.  
**Target:** Kepegawaian → **Menu 04 — Data Induk Pegawai**; tepat **6 Submenu**.  
**Tanggal penyusunan:** 16 November 2026 (WIB).  
**Task mode:** **`CROSS-REPO MODE`** untuk perubahan source lokal yang terbatas pada fitur Menu 04, dengan urutan backend-first lalu frontend. **Target tulis source yang dideklarasikan:** frontend `DevBenari/QuilvianSystemFrontendDev` branch `HamzahV2`; backend `DevBenari/NewQuilvianSystemBackend` branch `MHamzah`. **Target baca saja:** `DevBenari/QuilvianEngineeringSkills` branch `main`. **`MODULE BLUEPRINT MODE`** merupakan fase terpisah dan hanya untuk sinkronisasi dokumen HRD canonical yang tepat, apabila diizinkan governance; jangan memakai mode itu untuk mengubah source.  
**Snapshot pemeriksaan sumber (bukan SHA hasil Agent):** frontend `20478cd652651e8e9b620aaa83cdbce3dcc1648e`; backend `1b8e56f3f7abebde9a613ef592430d3da0dfc4d1`; Engineering Skills `201af64552b27873fd9af83c6722cf11b441bb1e`.  
**Revisi 3 - 16 November 2026:** Penomoran finishing memakai **namespace independen `FIN-HRD-04`**: **16 task backend `FIN-HRD-04-BE-001..016` dan 15 task frontend `FIN-HRD-04-FE-001..015`**. ID ini tidak menggantikan, memperpanjang, atau mengubah ID roadmap reguler `BE-HRD-*` dan `FE-HRD-*`; setiap FIN task mempunyai jenis pekerjaan, prioritas, referensi task existing, dependency, acceptance criteria, dan UAT. Nomor FIN adalah **PROPOSED / FINISHING_REGISTER_PENDING** sampai direkonsiliasi di register finishing canonical sesuai governance. Roadmap reguler tertinggi saat referensi dibaca: `BE-HRD-333` dan `FE-HRD-305` (informasi historis, bukan dasar alokasi FIN). Dokumen ini tetap satu master instruksi Menu 04.  
**Prinsip mutlak:** **AGENT WAJIB LANGSUNG MENGIMPLEMENTASIKAN perbaikan dan fitur Menu 04** pada workspace/branch yang sah sesudah pemeriksaan governance singkat; **jangan mengulang wawancara requirement yang telah disetujui, jangan berhenti setelah audit, roadmap, desain, atau penulisan laporan.**

> **Batas izin:** Dokumen ini menyatakan target tulis source lokal sesuai tugas user; ini bukan otorisasi untuk mengubah repository remote. DILARANG otomatis `git commit`, `push`, `pull`, `merge`, `rebase`, mengganti branch, deploy, membuat/menjalankan migration, atau menjalankan operasi database. Apabila perubahan skema memerlukan migration, kerjakan bagian independen yang aman, laporkan kebutuhan izin migration terpisah dan jangan mengklaim fitur baru telah persisten. Jangan mengakses data pribadi riil tanpa otorisasi.

---

## 1. Role

Anda adalah **Senior Fullstack Developer Quilvian V2, analis proses bisnis SDM rumah sakit, engineer keamanan data pegawai, UI/UX engineer, dan QA/UAT live-browser engineer**. Tugas Anda adalah menyelesaikan kapabilitas **Data Induk Pegawai** yang benar-benar berfungsi dengan data API nyata, persistensi yang benar, satu identitas tenaga kerja, permission berbasis peran, audit perubahan, serta pengalaman pengguna yang konsisten dengan Quilvian V2.

**Mandat eksekusi:** Telusuri implementasi existing secara terarah agar tahu apa yang harus digunakan kembali, lalu **ubah kode** (bukan hanya memberi rekomendasi), buat/memperbaiki service, endpoint, DTO, guard otorisasi, Redux/hook, route/view dan UI, uji hasilnya, serta perbaiki bug yang ditemukan sampai batas izin. Tidak boleh membuat CRUD atau halaman paralel hanya untuk mengejar tampilan menu apabila resource existing sudah mampu melayani proses bisnis tersebut.

**Urutan kepemilikan dan otorisasi:**

1. **`BACKEND MODE`**: target tulis hanya backend `NewQuilvianSystemBackend` @ `MHamzah`; FE sumber referensi read-only.
2. **`FRONTEND MODE`**: target tulis hanya FE `QuilvianSystemFrontendDev` @ `HamzahV2`; BE sumber kontrak read-only.
3. **`CROSS-REPO MODE`**: boleh dipakai untuk integrasi lokal FE+BE yang terkoordinasi dalam cakupan Menu 04 karena kedua target tulis dinyatakan secara eksplisit di atas; tetap pisahkan perubahan bertahap agar mudah ditinjau.
4. **`MODULE BLUEPRINT MODE`**: hanya untuk sinkronisasi keputusan yang sudah disetujui pada `docs/module-blueprints/human-resource/`, di luar pekerjaan source dan dengan pembatasan penulisan governance yang tepat. Jangan memodifikasi dokumen governance atau registry tanpa wewenang dan alasan resmi.
5. **Engineering Skills `main`**: `strict read-only`; hanya dipanggil/diikuti sebagai registered runtime Skill dan sumber aturan, **tidak pernah ditulis oleh Agent dalam task ini**.

Anda **WAJIB memanggil registered runtime skills** `quilvian-engineering-skills:build-module-backend` sebelum menyentuh source backend dan `quilvian-engineering-skills:build-module-frontend` sebelum menyentuh source frontend. Bila registry runtime tidak menemukan skill, hentikan pekerjaan sumber terkait dan laporkan **`BLOCKED — Quilvian runtime skill unavailable`** secara persis; jangan mengganti invokasi skill dengan membaca manual `SKILL.md` dan jangan mengklaim skill telah dijalankan.

---

## 2. Task

### 2.1 Tujuan bisnis, hasil implementasi, dan cakupan yang disetujui

**BANGUN/SELESAIKAN satu Menu resmi bernama persis `Data Induk Pegawai` dengan enam label Submenu berikut, dalam urutan yang tidak boleh diganti:**

1. **Profil Pegawai**
2. **Keluarga**
3. **Pendidikan**
4. **Riwayat Pekerjaan**
5. **Dokumen Personal**
6. **Data Profesi**

**Sumber cakupan:** registry pengguna 12 Menu / 61 Submenu pada Skill `hrd-finishing` (16 November 2026). Nama/nomor Menu pengguna **tidak sama** dengan kode slice canonical backend; jangan menamai ulang Submenu atau mengubah ownership domain hanya karena roadmap lama memakai istilah `Workforce Core` atau `Employee 360`.

**Alur utuh yang wajib berfungsi:** HR masuk → mencari/memilih orang berdasarkan identitas resmi → sistem memakai profil workforce yang sudah ada atau membuat satu profil baru bila memang belum ada → membuka enam kelompok data yang relevan → menyimpan atau mengajukan perubahan sesuai kelas data sensitif → backend memvalidasi permission dan persistensi → perubahan yang disetujui terlihat konsisten di detail/Employee 360, administrasi kepegawaian, kredensial, kontrak, perencanaan dan proses offboarding tanpa kehilangan histori.

**Keputusan dari pembahasan user saat ini:** implementasikan perbaikan profil ganda; penguncian edit data sensitif; kontrol dokumen pribadi; dukungan riwayat pekerjaan sebelum bergabung ke RS; pemanfaatan seluruh fitur pendidikan, keluarga dan profesi existing; navigasi enam Submenu serta UAT. Seluruhnya telah diterima user sebagai cakupan finishing Menu 04. Hal yang membutuhkan kebijakan/izin khusus (migration, penetapan hak akses baru lintas domain, kebijakan credentialing, dsb.) bukan persetujuan otomatis untuk mengeksekusi tindakan terlarang.

### 2.2 Keputusan bisnis canonical yang WAJIB dipertahankan

| Keputusan | Invarian yang harus diterapkan |
|---|---|
| `HRD-DEC-047` | Unit/jabatan berlaku-efektif berasal dari catatan penempatan; form master tidak boleh mengubah penempatan yang sudah berjalan langsung. Gunakan proses SK resmi untuk perubahan. |
| `HRD-DEC-048`, `049`, `050`, `056`, `057` | Peristiwa mutasi/promosi/rotasi/demosi ditautkan pada dokumen SK, tanggal efektif, aturan penempatan utama, batas retroaktif, persetujuan dan pembatalan resmi; jangan membuat editor riwayat yang membypass SK. |
| `HRD-DEC-051` | **Satu orang = satu `MstWorkforceProfile`** berdasarkan NIK untuk WNI/paspor untuk WNA, lintas Pegawai, Dokter, dan Pengguna Eksternal. Satu profil dapat mempunyai beberapa peran; **tautkan, bukan duplikasi**; jangan membuat profil yatim. |
| `HRD-DEC-052` | NIK/paspor, data rekening, NPWP, status perkawinan dan keluarga/tanggungan yang memengaruhi pajak, BPJS atau benefit **tidak boleh diubah langsung**; wajib permohonan dengan bukti dan diverifikasi petugas yang berbeda dari pembuat. Nilai lama tetap berlaku sampai diterapkan. Non-sensitif boleh diedit sesuai permission dengan audit nilai lama/baru. Co-sign keamanan sebelum rilis produksi tetap menjadi gate. |
| `HRD-DEC-053` | Employee 360: hanya HR berwenang mendapat profil penuh; atasan **utama** aktif hanya melihat profil pekerjaan bawahan langsung dan tidak menerima gaji, rekening, NIK/NPWP, alamat privat, keluarga, dokumen pribadi, detail kesehatan, atau disiplin. Bukan bawahan = `403`. |
| `HRD-DEC-054`, `055` | Inisialisasi penempatan awal dan daftar pantau lintas pegawai memiliki alur sendiri; jangan menduplikasi atau mengubah data penempatan via edit inline Menu 04. |
| Menu 06 selesai menurut laporan user | **Gunakan kembali** Menu 06 `Pengajuan Perubahan Data`, workflow/verifikasi, on-behalf, bukti, dan penerapan; jangan membuat persetujuan data sensitif versi kedua. Status real runtime Menu 06 tetap diuji, bukan diasumsikan lulus. |
| Menu 12 selesai menurut laporan user | Pertahankan sejarah pegawai dan hubungan ke offboarding/exit; profil nonaktif **tidak hilang** ketika relasi kerja berakhir. Jangan mengubah ulang logika terminasi di luar regresi Menu 04. |

**Sumber approved:** `docs/module-blueprints/human-resource/00-interview-decisions.md` pada backend branch `MHamzah`, khususnya bagian `HRD-DEC-047` s.d. `057`; `roadmap/00-slice-roadmap.md` bagian `S-A1`; `contracts/` dan `roadmap/requirement-traceability.md`. Rekomendasi baru berupa pengalaman kerja eksternal dan penyusunan enam submenu UI berasal dari pembahasan user dan **wajib direkonsiliasi** dengan canonical jika belum tertulis (status `NEEDS_SYNC`, bukan klaim sudah resmi tercatat).

### 2.3 Instruksi langsung per Submenu

### Submenu 1 - Profil Pegawai

**Task terkait:** existing `BE-HRD-003`, `BE-HRD-026`, `BE-HRD-028`, `FE-HRD-025`, `FE-HRD-027`, `FE-HRD-028`; finishing backend **`FIN-HRD-04-BE-001`, `FIN-HRD-04-BE-002`, `FIN-HRD-04-BE-003`, `FIN-HRD-04-BE-004`, `FIN-HRD-04-BE-014`**; finishing frontend **`FIN-HRD-04-FE-002`, `FIN-HRD-04-FE-003`, `FIN-HRD-04-FE-012`**. Rincian dan dependensi berada pada Register Task bernomor (bagian 2.6).

**Tujuan rumah sakit dan aktor.** Menyediakan satu rekam identitas tenaga kerja tepercaya untuk HR Admin/Manager, pimpinan/atasan dalam batas akses, pegawai sendiri (melalui ESS yang sah), serta pemilik integrasi payroll dan profesi. Profil dapat mewakili staf nonmedis, perawat, dokter atau orang yang memiliki lebih dari satu relasi/peran.

**Input/output dan alur normal.** Tampilkan identitas resmi (jenis, NIK/paspor sesuai status WNI/WNA), kode/nomor pegawai, nama, foto, tanggal lahir, jenis kelamin, komunikasi, alamat, status relasi kerja, profesi, serta ringkasan unit/jabatan berlaku-efektif **read-only**. HR mencari nama atau identitas, membuka detail; jika orang telah memiliki `MstWorkforceProfile`, tambahkan relasi `MstEmployee`, `MstDoctor` atau `MstExternalUser` sesuai perannya pada profil yang **sama**, bukan profil baru. Bila tidak ada, buat profil baru dalam transaksi atomik dengan relasi nyata; validasi foreign key dan hindari profil yatim.

**Existing yang WAJIB di-reuse.** FE: `/hr/master-data/employee`, `/hr/master-data/employee/[employeeSlug]`, `/hr/master-data/employee/[employeeSlug]/workforce`, `/hr/master-data/doctor`, `/hr/employee-360/[id]`; `src/components/view/hr/master-data/employees/**`, `src/components/features/workforce/**`, `src/lib/hooks/hr/master-data/employee/**`, `src/lib/state/slice/hr/master-data/master-data-employee-slice.jsx`. BE: `MasterData/Workforce/Controllers/{EmployeeController,DoctorController,ExternalUserController}.cs`, `Models/{MstWorkforceProfile,MstEmployee,MstDoctor,MstExternalUser}.cs`, `WorkforceCore/Services/HrdEmployee360Service.cs`. Existing `EmployeeController` telah memiliki guard mencegah edit unit/jabatan langsung: **pertahankan**.

**Temuan REPAIR yang harus diimplementasikan (P0).** Pada snapshot, `CreateEmployee`, `CreateDoctorForAdmin` dan `CreateExternalUser` masing-masing membuat `new MstWorkforceProfile` sekalipun orang dengan NIK/paspor itu mungkin telah ada di tabel lain; keunikan hanya pada jenis masing-masing. `MstWorkforceProfile.UserType` berupa satu nilai. Perbaiki jalur identitas lintas tiga tipe dengan **shared identity resolution dan transaction/concurrency guard** di domain pemilik resmi, termasuk normalisasi yang kompatibel (NIK 16 digit untuk WNI; paspor yang berlaku untuk WNA), pemeriksaan soft-delete, peran aktif, akun login, hubungan foreign key, dan penanganan konflik duplikasi `409`. Jangan otomatis menggabung data duplikat historis tanpa persetujuan; buat deteksi + laporan aman, dan gunakan link/merge hanya pada prosedur yang telah diputuskan (prosedur merger historis `HRD-Q-67` masih belum approved). Tinjau `UserType` legacy: utamakan representasi peran turunan/relasional berdasarkan tabel existing jika cukup; jangan memaksakan single enum sebagai satu-satunya identitas, dan jangan merusak caller lama.

**Temuan REPAIR data sensitif (P0).** Endpoint `EmployeeController.UpdateEmployee` saat ini memuat penggantian `IdentityNumber`, `MaritalStatus` dan field profil lainnya. `DoctorController` dan `ExternalUserController` juga perlu diperiksa. Implementasikan server-side guard granular untuk **perubahan data yang sudah ada** pada field sensitif, termasuk kasus payload PUT penuh mengirim field tak berubah. Jika ada perubahan sensitif, balas kegagalan/konflik terarah beserta petunjuk ke **Pengajuan Perubahan Data Menu 06**; jangan membuat mutasi diam-diam. Perubahan sensitif yang lolos approval diterapkan hanya dari backend workflow yang sah, bukan lewat parameter yang dikontrol frontend. Pastikan initial create yang sah tetap dapat mengisi identitas dengan validasi semestinya; jangan memblokir pembuatan pegawai baru hanya karena guard pembaruan.

**Implementasi FE.** Pertahankan `BaseGroupedEditorView` existing, table/filter/search/detail/Employee 360 dan UX lama. Tambahkan notifikasi konflik identitas dan alur `Tautkan peran` yang sah, pilihan pegawai existing, penanda profesi/peran, serta tindakan `Ajukan Perubahan Data` untuk field sensitif. Gunakan `AccessDeniedGate`, `Hero`, `DataTable`, `BaseButton`, `ToastStack` dan base component sesuai UI gate; tombol disabled bukan pengganti guard backend. Untuk manager, periksa `Employee360Service` dan jangan melakukan GET data rahasia di UI yang kemudian hanya disembunyikan lewat CSS.

**Validasi, exception, audit.** Unik lintas jenis, multi-request bersamaan, bukan-atasan `403`, input identitas tidak valid `400`, konflik penggabungan `409`/kode sepadan yang kontraknya disetujui, master referensi tidak valid ditolak, data terverifikasi tidak menghilang setelah refresh, `CreateBy/UpdateBy`/timestamp serta jejak nilai sensitif terjaga. Sinkronisasi role/organisasi/akun mengikuti service existing tanpa side-effect ke penempatan SK.

**Kriteria selesai & UAT:** `UAT-04-01` s.d. `UAT-04-11`, `UAT-04-38`, `UAT-04-42`. Buat Dokter dengan NIK uji lalu tambah peran Pegawai: **jumlah profil workforce tetap satu**; ulangi arah sebaliknya dan Pengguna Eksternal; request paralel tidak menghasilkan dua profil; atasan bukan pemilik tidak mendapat detail penuh; edit NIK langsung ditolak dan melalui Menu 06 berhasil setelah diverifikasi dan diterapkan.

### Submenu 2 - Keluarga

**Task terkait:** existing `BE-HRD-028` dan `FE-HRD-027` untuk workflow perubahan data; finishing backend **`FIN-HRD-04-BE-004`, `FIN-HRD-04-BE-006`, `FIN-HRD-04-BE-007`**; finishing frontend **`FIN-HRD-04-FE-004`, `FIN-HRD-04-FE-005`**.

**Tujuan rumah sakit dan aktor.** HR mengelola informasi pasangan, anak, keluarga lain, kontak darurat dan tanggungan yang menjadi sumber data administratif, benefit dan koordinasi kondisi darurat. Data keluarga yang memengaruhi pajak/benefit harus diperlakukan sebagai data sensitif. Pegawai boleh melihat/mengajukan perubahan pada dirinya sesuai self-service dan permission yang ada.

**Input/output dan alur normal.** Pilih orang menggunakan `WorkforceProfileId`, tampilkan daftar anggota keluarga (`WfpFamilyMember`), hubungan, identitas/TTL seperlunya, status kontak darurat dan tanggungan (`WfpDependent`). Hubungkan tanggungan ke anggota keluarga existing, bukan menyalin nama sebagai entitas baru; bedakan `familyMember` vs `dependent`, tanggal efektif kelayakan, kategori pajak/manfaat/asuransi, dan riwayat status. Validasi referensi anggota keluarga milik profil yang sama, tidak terhapus, relasi dan identitas wajar, serta keterkaitan benefit tanpa menghitung ulang manfaat yang dimiliki modul lain.

**Existing yang WAJIB di-reuse.** FE `familyMembers`, `dependents`, `emergencyContacts` pada `src/lib/constants/hr/workforce-profile/employee-workforce-tabs.jsx`, `src/lib/state/slice/hr/workforce-profile/workforce-profile-all.jsx`, dan `src/components/features/workforce/workforce-profile-dashboard.jsx`; BE `WorkforceCore/Controllers/WfpFamilyMemberController.cs`, `WfpDependentController.cs`, `WfpEmergencyContactController.cs` serta DTO/model pasangannya. GET, POST, PUT, PATCH status dan DELETE existing sudah tertulis di source.

**REPAIR P0.** Pada snapshot, `WfpFamilyMemberController.Update` dapat mengganti `Relationship`, `IdentityNumber`, `MaritalStatusText` langsung; `WfpDependentController.UpdateDependent` dapat mengganti kelayakan pajak, benefit dan asuransi secara langsung. Untuk perubahan yang berdampak pada status keluarga/tanggungan sensitif, tutup jalur ubah langsung **di backend**; arahkan melalui `EmployeeProfileChange`/approval yang sudah disetujui, termasuk unggahan bukti dan dual-actor verification. Periksa cakupan field Menu 06 secara nyata: bila keluarga/tanggungan belum didukung oleh field dictionary/applier, **perluas service/kontrak existing** secara terbatas, jangan buat controller persetujuan terpisah atau fallback langsung. Perubahan kontak darurat non-sensitif dapat tetap langsung dengan audit jika sesuai `HRD-DEC-052`.

**Implementasi BE/FE.** Buat form keluarga dengan field relevan, indikator apakah perubahan harus diajukan, tampilan status permohonan, pratinjau dokumen pembuktian melalui endpoint berotorisasi, dan tombol tindakan hanya sesuai `AvailableActions`/permission. Periksa dampak ke benefit/Payroll/Insurance hanya melalui kontrak owner yang existing; bila integrasi belum ada, pertahankan data lama, catat `PENDING_INTEGRATION` tanpa memalsukan sinkronisasi. Jangan membuka NIK pasangan/anak kepada atasan di Employee 360.

**Kriteria selesai & UAT:** `UAT-04-12` s.d. `UAT-04-17`. Tambah anggota dan kontak yang diizinkan; perubahan status pasangan atau tanggungan sensitif langsung menghasilkan penolakan, sedangkan permohonan berbukti menghasilkan status menunggu verifikasi. Pembuat tidak boleh memverifikasi permohonan sendiri. Setelah disetujui dan diterapkan oleh aktor berbeda, data yang tampil setelah reload benar, dan akun yang tidak berwenang memperoleh `403`.

### Submenu 3 - Pendidikan

**Task terkait:** finishing backend **`FIN-HRD-04-BE-008`**; finishing frontend **`FIN-HRD-04-FE-006`**. Kapabilitas CRUD existing direuse; nomor finishing mencakup validasi dan perbaikan, bukan mengganti master pendidikan.

**Tujuan rumah sakit dan aktor.** Menyimpan jenjang pendidikan, lembaga, jurusan, tahun kelulusan, dokumen ijazah, penanda pendidikan tertinggi dan status verifikasi sebagai data dasar penugasan dan pengembangan SDM. HR/Admin atau verifikator pendidikan bekerja sesuai role yang sudah didaftarkan; atasan hanya melihat ringkasan pendidikan yang memang diizinkan.

**Input/output dan alur normal.** Pilih profil → tampilkan daftar pendidikan → tambah/ubah data institusi, `EducationLevel`, jurusan, tanggal, `GraduationYear`, `CountryId`, nomor sertifikat dan dokumen → simpan melalui API → verifier yang berhak memverifikasi → HR menandai satu pendidikan tertinggi yang berlaku → hasil masuk ringkasan profil dan dapat dikonsumsi modul kompetensi tanpa menyalin tabel.

**Existing yang WAJIB di-reuse.** BE `WfpEducationController` dan `WfpEducationDtos` mendukung filter/summary/list/detail, POST/PUT, status, `PATCH highest`, `PATCH verify`, upload/download/delete file dengan validasi PDF/JPG/PNG dan ukuran maksimal **10 MB** sesuai metadata aktual. FE `educations` pada `employee-workforce-tabs.jsx`, `workforce-profile-all.jsx`, `workforce-resource-editor.jsx`, dan editor workforce existing. **Tidak perlu membuat master pendidikan baru**.

**Pekerjaan finishing (IMPLEMENTED, NOT VERIFIED).** Perbaiki bila ada mismatch `multipart/FormData`, `ReplaceExistingFile`, status verifikasi setelah edit, nama file, metadata MIME, validasi rentang tanggal, fallback error, serta perilaku satu pendidikan tertinggi pada permintaan serentak. Untuk perubahan pendidikan non-sensitif tetap ada audit `CreateBy/UpdateBy` dan status verifikasi yang jujur; jangan menganggap unggah file sama dengan verifikasi. Jika re-verifikasi diwajibkan oleh kontrak saat dokumen berubah, terapkan pada backend dan FE. Gunakan `getWfpResourceList`/`createWfpResourceItem`/`updateWfpResourceItem`/`deleteWfpResourceItem` yang sudah ada dan periksa penanganan file serta pagination.

**Implementasi UI.** Daftar/tabel pendidikan dengan jenjang, institusi, jurusan, tahun, penanda tertinggi, status verifikasi, tombol upload/lihat/unduh yang benar, empty/loading/error, konfirmasi hapus, dan detail responsif. Gunakan base components; jangan menampilkan status `Terverifikasi` jika API gagal. Lakukan pengecekan akses berkas berdasarkan authorization backend, bukan URL yang dapat ditebak.

**Kriteria selesai & UAT:** `UAT-04-18` s.d. `UAT-04-23`. Tambah D3 dan S1 untuk profil uji, unggah berkas PDF valid, tandai salah satunya tertinggi, periksa ringkasan dan reload; file lebih dari 10 MB atau tipe terlarang ditolak; perubahan pada pendidikan terverifikasi mengikuti aturan re-verifikasi; dua operasi tertinggi bersamaan tidak menyisakan dua record utama.

### Submenu 4 - Riwayat Pekerjaan

**Task terkait:** existing `BE-HRD-023`, `BE-HRD-024`, `BE-HRD-025`, `FE-HRD-022`, `FE-HRD-024`; finishing backend **`FIN-HRD-04-BE-009`, `FIN-HRD-04-BE-010`**; finishing frontend **`FIN-HRD-04-FE-007`, `FIN-HRD-04-FE-008`**. Task pengalaman kerja eksternal memiliki gate perubahan skema terpisah.

**Tujuan rumah sakit dan aktor.** Membentuk **dua sumber riwayat yang dibedakan jelas dalam satu Submenu**: (A) pengalaman bekerja **sebelum** bergabung dengan rumah sakit, yang diinput/di-review HR berdasarkan dokumen, dan (B) riwayat **kepegawaian internal** yang dihasilkan peristiwa resmi (masuk kerja, penempatan, mutasi, promosi, demosi, rotasi, perubahan status, serta akhir hubungan kerja).

**Input/output dan alur normal.** Tampilkan kronologi pengalaman dan peristiwa kerja pada pegawai yang dipilih. Untuk pengalaman eksternal, sediakan data terstruktur **nama institusi/perusahaan asal, bidang/jenis institusi, posisi/jabatan sebelumnya, mulai–selesai bekerja, deskripsi tugas, bukti surat pengalaman kerja dan status verifikasi bila contract mendukung**. Untuk riwayat internal, tampilkan jenis peristiwa, tanggal efektif, perubahan status/unit/jabatan, alasan, nomor/referensi SK, status approved, serta tautan bukti resmi; data peristiwa internal harus mengalir dari pemilik **SK/kontrak/offboarding**, bukan diubah manual untuk memalsukan histori.

**Existing yang WAJIB di-reuse.** BE `WfpEmploymentHistory` (`HistoryType`, `EffectiveDate`, `EndDate`, `Old*/New*`, `ReferenceType`, `ReferenceId`, file, approval), `WfpEmploymentHistoryController`, service SK `HrdPersonnelActionOrderService` dan implementasi Menu 06/12. FE tab `employmentHistories`, detail pegawai, serta view `/hr/workforce-core/employment-histories` sebagai daftar pantau administrasi kepegawaian. Jangan menghapus, mengubah FK, atau menimpa histori peristiwa yang sudah dipakai Menu 06/12.

**REPAIR/PENAMBAHAN P1.** Model yang diperiksa **belum menunjukkan atribut tempat kerja eksternal yang terstruktur**. Tambahkan cara simpan pengalaman eksternal yang tidak mencampuradukkan catatan SK internal. Tinjau model/schema nyata dan pilih **perluasan aman** `WfpEmploymentHistory` dengan jenis eksplisit dan field terstruktur ATAU model `Wfp*` khusus riwayat pengalaman eksternal jika bukti owner/registry mengharuskan; jangan mengambil keputusan dengan menciptakan tabel ganda tanpa memeriksa data existing. Terapkan DTO, validation, API, service, pagination, filter, audit, upload file dan view; tindakan `edit/delete` riwayat internal dari resource generik harus dibatasi bila record tersebut merupakan hasil peristiwa resmi. Event internal resmi tetap terbit dari lifecycle owner. **Bila penambahan field memerlukan migration dan izin terpisah belum ada, jangan membuat/menerapkannya; laporkan `BLOCKED — schema migration permission required` hanya untuk fungsi persistence baru, selesaikan integrasi read-only dan fungsi independen.**

**Validasi/exception.** Tanggal mulai tidak sesudah tanggal berakhir, nilai institusi tidak kosong, jenis source jelas `EXTERNAL_PRIOR`/jenis kontrak resmi, identitas profil benar, dokumen bukti tidak bisa diakses di luar izin; pengalaman eksternal dapat beririsan bila fakta pekerjaan merangkap (jangan menciptakan larangan overlap tanpa keputusan owner). Retensi histori internal tetap terjaga saat pegawai resign/pensiun, rehire atau pindah unit.

**Implementasi FE.** Satu halaman/tampilan terhubung dengan dua filter/segmen `Pengalaman Sebelumnya` dan `Riwayat Kepegawaian di RS` di dalam Submenu **Riwayat Pekerjaan**. Link ke SK dan kontrak existing bersifat read-only/aksi resmi; tidak membuat menu ketujuh. Integrasikan unggah bukti dan status API nyata.

**Kriteria selesai & UAT:** `UAT-04-24` s.d. `UAT-04-29`. Tambah satu pengalaman RS lama dengan tanggal dan surat pengalaman, muat ulang dan lihat seluruh atribut; peristiwa mutasi Menu 06 tampil otomatis dengan referensi sah dan tidak dapat diedit untuk membypass SK; pegawai yang selesai di Menu 12 masih memiliki histori internal dan eksternal.

### Submenu 5 - Dokumen Personal

**Task terkait:** finishing backend **`FIN-HRD-04-BE-011`, `FIN-HRD-04-BE-012`**; finishing frontend **`FIN-HRD-04-FE-009`, `FIN-HRD-04-FE-010`**. Fokus otorisasi berkas, verifikasi, expiry dan persistence.

**Tujuan rumah sakit dan aktor.** Mengelola dokumen pribadi tenaga kerja (identitas, kartu keluarga, ijazah, bukti administrasi personal lain sesuai katalog) dengan kontrol klasifikasi, masa berlaku, autentikasi, izin baca/ubah/unduh, penyimpanan file dan histori verifikasi. **Jangan mencampur dokumen personal dengan Surat Keputusan/Surat Tugas resmi Menu 06 atau STR/SIP sebagai workflow kredensial Menu 09**, walaupun file yang sama dapat direferensikan dengan aman sesuai kontrak.

**Input/output dan alur normal.** Pilih profil → tampil daftar `WfpDocument` dan jenis, nomor, tanggal terbit/kedaluwarsa, instansi penerbit, `IsConfidential`, `IsVerified`, verifier dan bukti → tambah/unggah → validasi → pratinjau/unduh lewat otorisasi → verifikasi/penolakan sesuai role dan transaksi existing → tampilkan status mendekati kedaluwarsa; metadata tetap ada setelah refresh. Ukuran/tipe berkas mengikuti metadata server, bukan angka hardcode di UI.

**Existing yang WAJIB di-reuse.** BE `WfpDocumentController`, `WfpDocumentDtos`, `WfpDocument`; API GET/POST/PUT, `PATCH status`, `PATCH verify`, `GET {id}/file`, DELETE file dan record; metadata `IsConfidential`, `IsVerified`, tanggal kedaluwarsa dan ukuran maksimal **10 MB**. FE tab `documents`, WFP Axios/Redux multipart, `workforce-profile-dashboard.jsx` (preview), dan modul arsip administrasi Menu 06.

**REPAIR P0/P1:** Pada snapshot, `GetDocumentById` dan `DownloadDocumentFile` memeriksa izin `WorkforceDocument.Read`; pemeriksaan khusus `IsConfidential` per aktor/per pemilik **tidak terlihat di metode unduh yang diperiksa**. `VerifyDocument` menggunakan `WorkforceDocument.Update`, sama dengan edit umum; belum cukup sebagai pemisahan tugas bila keputusan akses mengharuskannya. Audit semua jalur list/detail/download/preview/URL publik dan pastikan **dokumen rahasia dilindungi di server**, bukan hanya tombol disembunyikan. Tambahkan guard profil/konteks sesuai permission matrix canonical, pembatasan aktor berbeda jika diperlukan untuk verifikasi dokumen sensitif, audit pengunduh/pemverifikasi, pembatasan tipe/ukuran serta kesesuaian MIME, validasi path traversal, sanitasi nama berkas dan tidak membuka file pribadi via URL publik tanpa otorisasi. Pertahankan metode penyimpanan existing kecuali ada ketentuan security resmi yang mengharuskan perubahan.

**REPAIR integrasi perubahan identitas.** Perubahan isi data sensitif (nomor NIK/paspor, dll.) harus melalui Menu 06 berdasarkan `HRD-DEC-052`. Unggah dokumen bukti baru **bukan** izin otomatis untuk menerapkan data identitas baru; status request dan data efektif harus konsisten. Tinjau apakah `FileUrl` yang dikirim backend dapat diekspos untuk `IsConfidential`; jangan mengandalkan endpoint unduh terlindungi apabila masih ada alamat statis yang bisa diakses langsung.

**Implementasi FE.** Tabel dokumen dengan filter kategori/status/verifikasi/kedaluwarsa, badge, modal pratinjau aman, tindakan download dan verifikasi yang sesuai hak, konfirmasi penghapusan, feedback 401/403/404/413/415, progress/error unggah serta tampilan responsif. Jika kebijakan akses file sensitif belum terpenuhi, tampilkan status pembatasan dan jangan membuat *preview success* dari URL langsung.

**Kriteria selesai & UAT:** `UAT-04-30` s.d. `UAT-04-36`. Upload PDF/JPG valid, tolak file berukuran berlebih/ekstensi salah, akses sendiri/HR berwenang berhasil, akun tidak berhak dan manager tidak dapat mengunduh dokumen rahasia (`403`), URL file langsung tidak membypass autentikasi, verifikasi dan audit aktor persisten, expired status dihitung benar terhadap tanggal efektif.

### Submenu 6 - Data Profesi

**Task terkait:** existing `BE-HRD-026`/`FE-HRD-025` untuk Employee 360; finishing backend **`FIN-HRD-04-BE-001`, `FIN-HRD-04-BE-002`, `FIN-HRD-04-BE-013`, `FIN-HRD-04-BE-014`**; finishing frontend **`FIN-HRD-04-FE-002`, `FIN-HRD-04-FE-011`, `FIN-HRD-04-FE-012`**. Master profesi/kredensial existing direuse, tidak diambil alih.

**Tujuan rumah sakit dan aktor.** Mencatat profesi serta kategori tenaga kerja (medis, tenaga kesehatan, nonmedis sesuai master), termasuk spesialisasi untuk dokter dan relasi kerja yang berlaku. Data profesi dipakai kebutuhan organisasi, pencarian tenaga, kredensial dan pelayanan tetapi **bukan** izin otomatis melakukan tindakan klinis.

**Input/output dan alur normal.** HR membuka profil yang sudah dipilih, melihat profesi dan spesialisasi resmi berdasarkan `MstProfession` dan `MstSpecialization`, memilih/memperbaiki relasi profesi sesuai kebijakan peran; perubahan yang sah tersimpan di entitas pemilik (`MstEmployee.ProfessionId`, `MstDoctor.ProfessionId`, `SpecializationId`, dan relasi existing). Pegawai yang juga dokter tetap memiliki **satu** profil orang dan kedua peran ditampilkan konsisten; profesi dapat berubah sesuai record yang sah tanpa menghapus sejarah profesional.

**Existing yang WAJIB di-reuse.** FE `/hr/master-data/profession`, `/hr/master-data/specialization`, `/hr/master-data/doctor`, field `professionId` pada form pegawai, `employee-workforce-tabs.jsx` dan workflow detail Dokter; BE `MasterData/CompetencyAndCredential/Controllers/ProfessionController.cs`, `SpecializationController.cs`, `MasterData/Workforce/Controllers/EmployeeController.cs`, `DoctorController.cs`, model `MstProfession`, `MstSpecialization`, `MstEmployee`, `MstDoctor`. Kredensial/sertifikasi berada di `CredentialingManagement` (mis. `WfpCredentialLicenseController`, `WfpCertificationController`) dan dimiliki **Menu 09**, bukan diperbanyak sebagai tabel baru di Menu 04.

**REPAIR/PENAMBAHAN P1.** Satukan representasi profesi/peran dalam detail profil dengan memanfaatkan data terhubung dari employee/doctor; cek validasi `ProfessionId`/`SpecializationId` referensi aktif, konsistensi profesi klinis/non-klinis, perubahan peran ganda dan konflik pemilihan profesi. Bila profesi memiliki `RequiresLicense`/`RequiresCredentialing`, tampilkan status/referensi dari owner Menu 09 **tanpa** menyimpulkan sudah mendapat kewenangan klinis dari pemilihan profesi. Jangan menambahkan aturan kedokteran baru, mengaktifkan kewenangan, atau memberi izin menjadwalkan dokter berdasarkan profesi saja tanpa keputusan owner klinis.

**Implementasi FE.** Pada Submenu `Data Profesi`, sediakan akses lihat/kelola relasi profesi orang yang dipilih dengan dropdown master API, ringkasan peran `Pegawai`, `Dokter`, `Eksternal` bila berlaku, link ke halaman master profesi existing dan rujukan kredensial. Jangan menggandakan master `/hr/master-data/profession`; menu ini adalah konteks hubungan profesi **ke orang**. Gunakan komponen base yang sudah ada dan status backend.

**Kriteria selesai & UAT:** `UAT-04-37` s.d. `UAT-04-41`. Pilih profesi terdaftar untuk staf, ubah dan reload; pilih spesialisasi untuk dokter; profesi nonaktif/invalid ditolak; orang dengan dua peran tetap satu profil; pengguna tanpa izin tidak dapat mengubah data profesi; tidak ada tindakan atau privilege klinis otomatis hanya karena profesi dipilih.

### 2.4 Navigasi enam Submenu — WAJIB tersedia tanpa menduplikasi domain

**Target UX:** pada sidebar HRD tersedia **satu group/menu `Data Induk Pegawai` dan enam anak** dengan label persis pada 2.1. Enam anak harus dapat dibuka dari sidebar, mempunyai route yang valid, menyertakan pemilihan orang (search/filter berdasarkan profile yang berwenang) bila belum ada konteks pegawai, dan mengarahkan ke editor/data existing dari `WorkforceProfileId` yang sama. **Jangan mengubah nama menu existing `Master Data > Karyawan`, `Dokter`, `Profesi`, `Administrasi Kepegawaian` atau route lama secara destruktif**; lakukan alias/redirect/komposisi minimal yang aman.

**Rekomendasi route UI baru hanya jika memang diperlukan untuk navigasi enam submenu:**

| Label submenu | Calon route wrapper/alias (BARU, belum terbukti existing) | Reuse tampilan/kontrak existing |
|---|---|---|
| Profil Pegawai | `/hr/data-induk-pegawai/profil` | `/hr/master-data/employee`, detail pegawai, Employee 360 dan picker profil |
| Keluarga | `/hr/data-induk-pegawai/keluarga` | Profil terpilih → workforce `familyMembers`, `dependents`, `emergencyContacts` |
| Pendidikan | `/hr/data-induk-pegawai/pendidikan` | Profil terpilih → workforce `educations` |
| Riwayat Pekerjaan | `/hr/data-induk-pegawai/riwayat-pekerjaan` | Profil terpilih → `employmentHistories` + pengalaman eksternal, link SK existing |
| Dokumen Personal | `/hr/data-induk-pegawai/dokumen-personal` | Profil terpilih → workforce `documents` dengan secure viewer |
| Data Profesi | `/hr/data-induk-pegawai/data-profesi` | Profil terpilih → relasi profesi dan link master profesi/dokter |

Route di atas adalah **desain implementasi yang disetujui sebagai arah kerja, bukan klaim telah ada**. Periksa navigasi, struktur folder Next.js, route/permission existing dan teknik alias yang aman. Boleh menyesuaikan path teknis bila konflik terbukti, tetapi **label dan enam perilaku harus tetap** serta setiap perubahan path dijelaskan dalam laporan. **Satu workspace pemilihan profil dan tampilan resource yang digunakan bersama** lebih baik daripada enam CRUD terpisah dengan data ganda. Pertahankan deep link `.../[employeeSlug]/workforce/[resourceKey]` existing, route token aman, state saat refresh, dan fallback 404/403.

### 2.5 Ketentuan lintas Submenu dan dependensi

1. **Sumber identitas dan foreign key:** gunakan `MstWorkforceProfile.Id` tunggal lintas person-role dan seluruh tabel `Wfp*`. Jangan menyatukan record ber-NIK sama secara otomatis bila sudah ada duplikasi historis karena `HRD-Q-67` belum disetujui.
2. **Mutasi data sensitif:** hanya melalui `EmployeeProfileChangeController`, `EmployeeProfileChangeService`, on-behalf, bukti, verifikasi aktor lain, `apply` dari backend dan workflow engine Menu 06 yang sah. Jangan membuat endpoint approval dummy, state lokal `Approved` palsu atau mengeksekusi `approve` langsung bila controller menginstruksikan memakai Workflow Engine/Approval Inbox.
3. **Penempatan, status dan histori:** `HrdPersonnelActionOrder`/catatan penempatan sebagai sumber unit dan jabatan berlaku; kontrak serta exit mengikuti modul pemilik. Data Induk menampilkan dan mengaitkan, tidak mengambil alih peristiwa resmi.
4. **Pendidikan vs profesi vs kredensial:** ijazah berada pada Pendidikan; profesi adalah master/relasi person-role; STR/SIP, kewenangan praktik, sertifikasi profesional dan keputusan clinical privilege dikelola melalui Menu 09/owner asli. Jangan menggandakan ledger atau memberi klaim siap klinis.
5. **Dokumen dan privacy:** data keluarga, identitas dan berkas hanya ditransfer melalui endpoint yang berhak, dengan role-level/record-level guard. Periksa juga payload list, export/preview, bukan hanya halaman detail.
6. **Tanggung jawab:** HR mengelola kepegawaian; Finance memegang payroll/pajak; komite medis/kredensial memegang kelayakan praktik; Identity memegang akun/permission. Integrasi tidak boleh mengubah bounded context lain di luar cakupan yang terotorisasi.
7. **Bahasa dan UI:** seluruh tampilan Bahasa Indonesia, sesuai desain dan Base Component Quilvian. Loading/error/empty, sorting, paging, responsive tablet/desktop, keyboard focus dan aksesibilitas wajib diperhatikan.


### 2.6 Register Task Finishing Bernomor - namespace FIN tersendiri (tetap trace ke roadmap Backend dan Frontend)

**Tujuan register:** mengubah seluruh temuan per Submenu menjadi unit kerja yang memiliki nomor, judul, prioritas, dependency, acceptance criteria, verifikasi/UAT, status, owner repo dan sumber laporan. **Ini register finishing pendamping, bukan roadmap reguler baru atau penggantian task resmi existing.** Agent harus mengerjakan task-task sumber yang sah, bukan berhenti setelah mengaudit register.

#### 2.6.1 Konvensi nomor, status dan kepemilikan

- **Roadmap sumber resmi:** `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` dan `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md`, branch BE `MHamzah`, snapshot BE `1b8e56f3f7abebde9a613ef592430d3da0dfc4d1`. **Nomor tertinggi terkonfirmasi:** `BE-HRD-333` dan `FE-HRD-305`; bukan berarti semua nomor terdahulu telah lulus UAT.
- **Existing task:** ID yang sudah benar-benar tercantum di roadmap **TIDAK BOLEH** ditimpa, dialokasikan ulang, diganti judul/cakupan secara diam-diam, atau dibuat duplikat. Baca report tracked lama dan edit hanya berdasarkan scope perbaikan yang tepat.
- **Namespace finishing Menu 04:** `FIN-HRD-04-BE-001..016` dan `FIN-HRD-04-FE-001..015` adalah **PROPOSED / FINISHING_REGISTER_PENDING**, terpisah dari namespace `BE-HRD-NNN` / `FE-HRD-NNN`. Angka FIN dimulai dari 001 untuk setiap layer dan setiap nomor Menu, **tidak mengikuti angka terbesar roadmap reguler**. Agent wajib cek benturan **khusus namespace `FIN-HRD-04-*`** dalam dokumen/register/report aktual sebelum mencatatnya secara resmi. Jika collision sudah ada, jangan menimpa/mengganti ID yang telah dipakai; eskalasi mapping dan kelola nomor baru melalui register finishing sesuai governance.
- **Penanda slice/gelombang internal:** `FINISH-M04` / `M04-G1..G4` adalah kelompok eksekusi finishing, **bukan** slice atau MVP canonical baru. Tetap gunakan referensi existing `S-A1` Workforce Core dan kepemilikan `MasterData/Workforce` untuk traceability. Jangan menciptakan nomor MVP global atau menomori ulang roadmap reguler.
- **Status awal SEMUA nomor baru:** `PROPOSED | NOT STARTED | NOT VERIFIED`; jangan tulis `✅`, `PASS`, `100%`, laporan tracked, atau tautan report untuk ID baru seolah sudah ada. Status `✅` pada existing roadmap hanya berarti *menurut register historis*; bukan bukti UAT pada HEAD saat Agent bekerja.
- **Pendaftaran dan traceability:** pertahankan ID roadmap existing; catat nomor FIN dalam **register finishing tersendiri** pada dokumentasi HRD sesuai mode dan izin dokumen, lalu tautkan `FIN ID -> legacy roadmap ID -> requirement/decision -> changed source -> UAT`. Jika governance mensyaratkan appendix pada `backend-roadmap.md` / `frontend-roadmap.md`, tambahkan **section trace FIN** yang jelas tanpa mencampur urutan `BE-HRD-NNN` / `FE-HRD-NNN` dan tanpa mengubah status task lama. Perbarui `requirement-traceability.md` hanya lewat mode dokumentasi yang sah. Permintaan pengguna ini adalah revisi file master lokal, **bukan izin menulis GitHub**. Jika pendaftaran belum diizinkan, tandai `FINISHING_REGISTER_PENDING` dan **lanjutkan implementasi source independen**. Jangan membuat report FIN palsu sebelum ID diakui dalam sistem pelaporan resmi.
- **Jenis task:** `FIX` = perbaikan perilaku; `ADD` = kemampuan baru; `SECURITY` = otorisasi/privasi; `INTEGRATION` = penyelarasan lintas modul/API; `UAT` = pengujian dan retest. Jenis adalah klasifikasi operasional; jangan anggap satu jenis memberi izin menulis domain lain.
- **Bukti hasil:** tiap task mempunyai `FIN Task ID`, `Jenis`, `Prioritas`, `Task mode`, `Scope target`, `Referensi roadmap existing`, `Dependency`, `Acceptance Criteria`, `Validation`, `Status`, `Changed files`, `Source SHA`, dan `Report` sesuai kontrak yang berlaku. `PROPOSED` menjadi `REGISTERED` hanya setelah keberadaan ID FIN terbukti pada register finishing canonical yang diterima governance, bukan karena tercantum dalam instruksi atau karena task reguler berstatus DONE.

#### 2.6.2 Task existing resmi yang harus di-REUSE / di-regresi

| Task resmi existing | Peran terhadap Menu 04 | Status tercatat pada roadmap saat diperiksa | Tindakan Agent |
|---|---|---|---|
| `BE-HRD-003` (`S-A1`) | Daftar lintas-pegawai Workforce Core | `✅` pada roadmap | Reuse query/auth/rekap; tidak menyalin endpoint baru. |
| `BE-HRD-023`, `024`, `025` (`S-A1`) | Model, service, approval dan API SK kepegawaian | `✅` pada roadmap | Reuse sumber riwayat internal; wajib uji regresi SK. |
| `BE-HRD-026` (`S-A1`) | API Employee 360 dual-mode | `✅` pada roadmap | Reuse, harden masking dan otorisasi di task finishing baru. |
| `BE-HRD-027` (`S-A1`) | Batch penempatan awal | `✅` pada roadmap | Jangan mengubah jalur batch di luar regresi identitas/penempatan. |
| `BE-HRD-028` (`S-A1`) | Pengajuan perubahan profil atas nama dan lock master | `✅` pada roadmap | Reuse service/flow; perkuat sensitive field/dual actor. |
| `FE-HRD-002` (`S-A1`) | Daftar lintas-pegawai | `✅` pada roadmap | Reuse komponen, navigasi dan akses; tidak membuat tabel master kedua. |
| `FE-HRD-022`, `023`, `024` (`S-A1`) | Daftar/form/detail dokumen SK | `✅` pada roadmap | Reuse tautan bukti untuk riwayat, regression Menu 06. |
| `FE-HRD-025` (`S-A1`) | Employee 360 dual-view | `✅` pada roadmap | Reuse UI; test masking/role, no sensitive preload. |
| `FE-HRD-026` (`S-A1`) | Batch penempatan awal | `🟡 SEBAGIAN` pada roadmap | Cek dependensi; tidak menyatakan selesai karena Menu 04. |
| `FE-HRD-027` (`S-A1`) | On-behalf Pengajuan Perubahan Data | `🟡 SEBAGIAN` pada roadmap/report | Reuse dan selesaikan defect jika berhubungan; jangan mengklaim selesai tanpa apply+UAT. |
| `FE-HRD-028` (`S-A1`) | Lock edit unit/jabatan dari Master Data | `✅` pada roadmap | Pertahankan lock; regresi perubahan status/identitas. |
| `FE-HRD-R56` (audit remediasi, **bukan** nomor urut baru) | Menautkan beberapa task partial, termasuk `FE-HRD-027` | `BLOCKED` pada audit 11 Nov | Cross-check dependensi/kondisi terbaru; jangan duplikasi cakupan dan jangan paksa reopen seluruh scope unrelated. |

**Aturan baseline:** task existing di atas tetap memakai report asalnya. Task finishing baru hanya memuat **gap atau perbaikan yang belum tercakup/terbukti oleh acceptance lama**. Bila setelah membaca task lama ternyata persis sama, gunakan **task existing yang relevan sebagai repair** dan tandai nomor finishing terkait `MERGED_INTO_EXISTING` (beserta alasan/trace), bukan implementasi ganda.

#### 2.6.3 Backend - 16 task FIN (register finishing tersendiri, terkait roadmap existing)

**Target:** `DevBenari/NewQuilvianSystemBackend` / `MHamzah` / `BACKEND MODE`; semua `Status = PROPOSED`, gelombang lokal `FINISH-M04`.

| Task FIN | Jenis | Prioritas | Target Submenu / judul kerja | Ref. roadmap existing | Dependency utama | Acceptance dan UAT |
|---|---|---|---|---|---|---|
| `FIN-HRD-04-BE-001` | FIX | P0 | Profil Pegawai - resolver identitas resmi lintas tipe & model peran jamak | `BE-HRD-003`, `BE-HRD-026` | `BE-HRD-003`, `HRD-DEC-051`; schema gate | Satu ID orang -> satu WFP; `UAT-04-02..07`, `38` |
| `FIN-HRD-04-BE-002` | FIX | P0 | Profil Pegawai - create/link Pegawai, Dokter, Eksternal atomik, cegah race | `BE-HRD-003`, `BE-HRD-026` | `FIN-HRD-04-BE-001` | Tidak ada profil ganda/orphan; `UAT-04-03..06`, `38` |
| `FIN-HRD-04-BE-003` | SECURITY | P0 | Profil Pegawai - server guard mutasi langsung field sensitif semua tipe | `BE-HRD-028` | `BE-HRD-028`, `HRD-DEC-052` | PUT/PATCH bypass ditolak; `UAT-04-08`, `09`, `42` |
| `FIN-HRD-04-BE-004` | INTEGRATION | P0 | Profil Pegawai/Keluarga - perluasan workflow Menu 06, evidence dan dual actor | `BE-HRD-028` | `BE-HRD-028`, `FIN-HRD-04-BE-003` | Creator != verifier; apply atomik; `UAT-04-09..11`, `14` |
| `FIN-HRD-04-BE-005` | SECURITY | P0 | Profil Pegawai - guard rekening bank dan sinkronisasi approved value | `BE-HRD-028` | `FIN-HRD-04-BE-004` | Rekening tidak bisa PUT langsung; `UAT-04-08..11` + negative API |
| `FIN-HRD-04-BE-006` | SECURITY | P0 | Keluarga - guard field identitas/relasi sensitif dan audit | `BE-HRD-028` | `FIN-HRD-04-BE-004` | Langsung ditolak, non-sensitif ter-audit; `UAT-04-12..15`, `17` |
| `FIN-HRD-04-BE-007` | SECURITY | P0 | Keluarga - tanggungan pajak/benefit melalui apply sah, validasi FK | `BE-HRD-028` | `FIN-HRD-04-BE-004`, `FIN-HRD-04-BE-006` | Tidak bocor/terubah sebelum apply; `UAT-04-13..17` |
| `FIN-HRD-04-BE-008` | FIX | P1 | Pendidikan - API file/verifikasi/pendidikan tertinggi konsisten dan race-safe | `BE-HRD-003` | `BE-HRD-003` | 1 pendidikan tertinggi, file aman; `UAT-04-18..23` |
| `FIN-HRD-04-BE-009` | ADD | P1 | Riwayat Pekerjaan - data pengalaman eksternal terstruktur & persisten | `BE-HRD-003` | `BE-HRD-003`; schema gate | Employer/role/period/bukti persisten; `UAT-04-24..25`, `28..29` |
| `FIN-HRD-04-BE-010` | SECURITY | P0 | Riwayat Pekerjaan - histori internal read-only dari SK/approval, no bypass CRUD | `BE-HRD-024`, `BE-HRD-025` | `BE-HRD-024`, `BE-HRD-025` | Peristiwa resmi tak dapat ditulis sembarang; `UAT-04-26..27`, `29` |
| `FIN-HRD-04-BE-011` | SECURITY | P0 | Dokumen Personal - record-level authorization file/detail/list/link | `BE-HRD-026` | `BE-HRD-003`, `HRD-DEC-053` | Tak ada public bypass; `UAT-04-30..32`, `42` |
| `FIN-HRD-04-BE-012` | SECURITY | P1 | Dokumen Personal - segregasi verifikator, audit, expiry, penggantian file | `BE-HRD-028` | `FIN-HRD-04-BE-011` | Verifikasi berhak dan file konsisten; `UAT-04-33..36` |
| `FIN-HRD-04-BE-013` | INTEGRATION | P1 | Data Profesi - hubungan profesi/spesialisasi dengan peran jamak & referensi valid | `BE-HRD-003`, `BE-HRD-026` | `FIN-HRD-04-BE-001`, `FIN-HRD-04-BE-002` | Profesi aktif tepat; tak ada privilege otomatis; `UAT-04-37..40` |
| `FIN-HRD-04-BE-014` | SECURITY | P0 | Profil Pegawai/Data Profesi - hardening Employee 360 dual-mode dan masking API | `BE-HRD-026` | `BE-HRD-026`, `FIN-HRD-04-BE-001` | Atasan hanya bawahan utama; 403 dan masked; `UAT-04-17`, `31`, `42` |
| `FIN-HRD-04-BE-015` | INTEGRATION | P0 | Lintas Submenu - integrasi aman Menu 06/09/12, data historic/status | `BE-HRD-024`, `BE-HRD-028` | `FIN-HRD-04-BE-004`, `FIN-HRD-04-BE-010`, `FIN-HRD-04-BE-013`, `FIN-HRD-04-BE-014` | No regression SK, offboarding, clinical; `UAT-04-26`, `29`, `39..40`, `47..48` |
| `FIN-HRD-04-BE-016` | UAT | P0 | Lintas Submenu - targeted unit/integration/security/concurrency tests & backend release gate | `BE-HRD-003`, `BE-HRD-026`, `BE-HRD-028` | Semua task BE terkait yang dapat dijalankan | Catat PASS/FAIL/NOT RUN, bukan asumsi; `UAT-04-02..48` |

##### FIN-HRD-04-BE-001 - Resolver identitas resmi & representasi multi-peran
- **Implementasi:** audit model `MstWorkforceProfile` dan hubungan `MstEmployee`, `MstDoctor`, `MstExternalUser`; tambah shared resolver orang berdasarkan NIK WNI / paspor WNA yang telah dinormalisasi, secara konsisten lintas tiga tabel dan soft-delete. Pertahankan kompatibilitas `UserType` lama untuk caller yang belum dipindah, tetapi **jangan** menjadikannya satu-satunya sumber informasi peran jamak.
- **Acceptance:** API/query dapat menemukan profil existing secara aman tanpa membuka identitas sensitif untuk role tidak berhak; satu orang tidak dipetakan ke dua profil baru. Keunikan dalam kondisi concurrency didukung constraint/transaction yang benar bila skema mengizinkan. Jangan auto-merge profil historis (`HRD-Q-67`).
- **File awal:** `MasterData/Workforce/Models/MstWorkforceProfile.cs`, `Repositories/ApplicationDbContext.cs`, tiga master controller, service resolver milik domain existing. Perubahan entity/constraint = **SCHEMA BLOCKED** bila belum mendapat izin migration terpisah.

##### FIN-HRD-04-BE-002 - Tautan create/upsert peran Pegawai, Dokter, Eksternal
- **Implementasi:** ubah `CreateEmployee`, `CreateDoctorForAdmin`, `CreateExternalUser` supaya melakukan lookup/link profil existing melalui resolver terotorisasi; pembuatan profil baru hanya bila belum ada, sekaligus membuat relasi riil dalam transaction. Tangani retry/race, `409`, FK, akun login, rehire dan rollback; tidak boleh dua profil pada nomor identitas sama.
- **Acceptance:** Dokter -> Pegawai dan sebaliknya, plus Pengguna Eksternal, menghasilkan `WorkforceProfileId` identik; jumlah profil bertambah **nol** ketika menautkan. Test parallel submit dan duplicate exception (tidak mengekspose NIK di log).

##### FIN-HRD-04-BE-003 - Guard semua surface update data sensitif
- **Implementasi:** server-side comparator *old vs new*, bukan larangan blanket pada PUT penuh yang tidak mengubah nilai. Guard identitas resmi, status menikah dan field sensitive existing di `EmployeeController`, `DoctorController`, `ExternalUserController` dan endpoint lain yang memutasi profil; initial-create sah tetap bisa menyimpan identitas.
- **Acceptance:** update non-sensitif aman; perubahan sensitif bypass dari HTTP langsung ditolak dengan kesalahan jelas; jangan memakai payload frontend/role admin sebagai bypass. Uji `400/403/409` sesuai kontrak.

##### FIN-HRD-04-BE-004 - Workflow perubahan sensitif Menu 06 sebagai satu jalur sah
- **Implementasi:** periksa `EmployeeProfileChangeController.cs` dan `EmployeeProfileChangeService.cs` (on-behalf, evidence, start verification, decision, workflow apply, field dictionary). Tambahkan cakupan field/data keluarga/rekening/identity **hanya jika benar-benar belum tercakup**; verifier harus berbeda dari request creator; nilai lama aktif sampai apply; duplikasi request/penerapan ditangani idempotent.
- **Acceptance:** request dengan bukti -> verifikasi berbeda aktor -> approved -> apply server atomik; perubahan muncul pada profil secara konsisten, audit nilai lama dan baru disimpan aman. Reuse Menu 06, jangan buat approval controller versi lain.

##### FIN-HRD-04-BE-005 - Rekening bank tidak dapat diubah diam-diam
- **Implementasi:** tutup `WfpBankAccountController.UpdateBankAccount` dan jalur `Create/Verify/Primary/Status` yang dapat menyiasati perubahan rekening sensitif; penggantian nomor/bank/pemilik rekening existing diarahkan ke `EmployeeProfileChange` sesuai keputusan. Pastikan petugas verifikasi berbeda dari pembuat, audit dan status payroll/Finance tidak didorong sebelum apply.
- **Acceptance:** direct change rekening existing ditolak, request approved memakai jalur service yang aman; tidak ada efek ganda atau pemutakhiran payroll palsu.

##### FIN-HRD-04-BE-006 - Keluarga: kunci field yang memengaruhi benefit/pajak
- **Implementasi:** pada `WfpFamilyMemberController`, bedakan perubahan administratif non-sensitif dari perubahan hubungan/identitas/status menikah yang berdampak pajak, BPJS, benefit; tambahkan guard server dan pengajuan perubahan berbukti; audit riwayat. Cegah pemakaian FK anggota keluarga milik profil lain.
- **Acceptance:** API PUT/PATCH/DELETE tidak bisa membypass keputusan `HRD-DEC-052`; kontak darurat non-sensitif sesuai izin tetap berfungsi; negative permission dan data setelah reload konsisten.

##### FIN-HRD-04-BE-007 - Tanggungan: perubahan efektif lewat approval sah
- **Implementasi:** `WfpDependentController` melakukan validasi keluarga, masa berlaku, hubungan, benefit eligibility/tax flags; perubahan yang memengaruhi benefit/pajak wajib melalui workflow Menu 06. Hilangkan jalur direct-write untuk sensitive fields; jangan mengarang integrasi payroll/benefit. Pertahankan data lama sampai diterapkan.
- **Acceptance:** tidak ada tanggungan bertaut ke profil lain, status kelayakan tidak berganti sebelum apply, peran tanpa izin tidak dapat baca/edit data sensitif.

##### FIN-HRD-04-BE-008 - Pendidikan: validasi lifecycle dan file
- **Implementasi:** reuse `WfpEducationController`/DTO/form-data; perbaiki upload/replace/download, metadata dan extension/size validation, status verifikasi dan pendidikan tertinggi, concurrency ketika dua user memilih highest. Jangan mengubah schema jika fitur existing cukup.
- **Acceptance:** tepat satu pendidikan tertinggi aktif per profil, file aman, audit verifier benar, file lama tidak tersedia sesudah replace bila kontrak mengharuskan; `UAT-04-18..23`.

##### FIN-HRD-04-BE-009 - Pengalaman kerja eksternal terstruktur
- **Implementasi:** pisahkan pengalaman di rumah sakit/perusahaan sebelumnya dari `WfpEmploymentHistory` internal yang bersumber dari SK. Utamakan perluasan model/existing resource yang aman; jika perlu entity baru, verifikasi ownership `Wfp`, registry prefix dan desain schema non-destruktif. Field minimum: nama pemberi kerja, jabatan, lokasi opsional, tanggal mulai/akhir, ringkasan tanggung jawab, bukti, penanda sumber.
- **Acceptance:** GET/POST/PUT pada data pengalaman eksternal yang sah persisten setelah reload dan dapat disaring tanpa tercampur SK; validasi tanggal dan file; **SCHEMA BLOCKED** bila membutuhkan migration yang belum diizinkan, tetapi task independen lain lanjut.

##### FIN-HRD-04-BE-010 - Lindungi histori kepegawaian internal
- **Implementasi:** jadikan histori peristiwa status, mutasi, promosi, rotasi dan terminasi yang berasal dari `HrdPersonnelActionOrder`/Menu 06/12 **read-only pada jalur Menu 04**; mutasi legal tetap melalui service/SK pemilik. Endpoint generic `WfpEmploymentHistoryController` tidak boleh membypass approval dengan create/update/delete atas peristiwa resmi; perubahan historis harus punya referensi yang benar.
- **Acceptance:** manipulasi langsung riwayat SK ditolak tanpa merusak riwayat; filter internal/external stabil dan penonaktifan pegawai tidak menghapus histori.

##### FIN-HRD-04-BE-011 - Security dokumen personal (daftar, detail, file, URL)
- **Implementasi:** audit `WfpDocumentController` semua action/read projection `IsConfidential`; terapkan **record-level** policy pada list/detail/preview/download/file path/export. Uji static/public file route agar URL metadata tidak menjadi direct bypass; bila perlu pindahkan private file ke storage terproteksi/endpoint authorized tanpa data loss.
- **Acceptance:** atasan dan akun tanpa izin tidak dapat membaca dokumen personal/private lewat API ataupun static URL; semua role HR sah tetap berfungsi; tidak sekadar menyembunyikan tombol.

##### FIN-HRD-04-BE-012 - Verifikasi dan siklus berkas dengan audit
- **Implementasi:** enforce verifier permission berbutir dan separation-of-duties yang disyaratkan; `WfpDocumentController.VerifyDocument` saat ini memakai `WorkforceDocument.Update` umum. Perbaiki verifikasi/undo, expired H-30, replacement cleanup, upload rollback/orphan cleanup, MIME/extension/size, audit actor. Jangan menampilkan PII dalam log.
- **Acceptance:** role tidak berhak mendapat 403, file atau metadata tidak terpisah akibat gagal simpan, verifikasi benar setelah reload, expiry konsisten.

##### FIN-HRD-04-BE-013 - Profesi untuk person-role jamak
- **Implementasi:** reuse `ProfessionController`, `SpecializationController`, `MstEmployee.ProfessionId`, `MstDoctor.ProfessionId/SpecializationId`; validasi master aktif/referensi dan tampilkan role pegawai+doctor dari profil yang sama, tanpa mengkloning WFP. Pemetaan profesi tidak boleh otomatis memberi STR/SIP atau clinical privileges.
- **Acceptance:** pilihan profesi dan detail konsisten, role masing-masing tidak mengubah data role lain, credentialing/clinical Menu 09 tetap domain source of truth.

##### FIN-HRD-04-BE-014 - Masking Employee 360 dan relasi atasan
- **Implementasi:** harden `HrdEmployee360Service.cs`, `HrdEmployee360Controller`, daftar/detil yang terkait: HR penuh hanya sesuai permission, atasan utama aktif hanya bawahan langsung, `403` untuk bukan bawahan; filter financial, family, identity, private docs dan health sensitive pada **server response**, bukan hanya FE hide.
- **Acceptance:** matrix HR, DirectManager, non-manager benar; perubahan tanggal efektif atasan langsung mengubah otorisasi; tidak ada field sensitif di payload atasan.

##### FIN-HRD-04-BE-015 - Integrasi/kompatibilitas Menu 06, 09, 12
- **Implementasi:** tes kontrak dan dampak data WFP ke SK, pengajuan profil, offboarding/clearance, profesi/kredensial, payroll/benefit read-scope; gunakan event/API resmi dan jangan mengubah bounded context lain. Prioritas keamanan: jangan mengaktifkan kembali akun pegawai selesai/offboard hanya karena update profil.
- **Acceptance:** UAT/regresi Menu 06 dan 12 lulus, kredensial tidak berubah otomatis dan FK tetap valid; gap lintas owner dicatat sebagai blocker terisolasi.

##### FIN-HRD-04-BE-016 - Test dan gate finishing Backend Menu 04
- **Implementasi:** targeted unit, API integration dan negative permission tests untuk setiap task; concurrency create identity/highest education, double apply, rollback, invalid data, private file download, dokumentasi command dan waktu, cross-module regressions. Jalankan hanya command aman sesuai test policy (bukan DB production/migration otomatis).
- **Acceptance:** matriks hasil `PASS/FAIL/NOT RUN/BLOCKED` per subcase dan task tersedia; task P0 tidak dinyatakan selesai jika auth/persistence belum dibuktikan. Perbaiki FAIL di scope lalu retest.

#### 2.6.4 Frontend - 15 task FIN (register finishing tersendiri, terkait roadmap existing)

**Target:** `DevBenari/QuilvianSystemFrontendDev` / `HamzahV2` / `FRONTEND MODE`; semua `Status = PROPOSED`, gelombang lokal `FINISH-M04`.

| Task FIN | Jenis | Prioritas | Target Submenu / judul kerja | Ref. roadmap existing | Dependency utama | Acceptance dan UAT |
|---|---|---|---|---|---|---|
| `FIN-HRD-04-FE-001` | ADD | P0 | Lintas Submenu - sidebar/menu resmi 6 anak dengan reuse route lama | `FE-HRD-002` | `FE-HRD-002` | Enam menu hidup, RBAC tepat; `UAT-04-01`, `46` |
| `FIN-HRD-04-FE-002` | FIX | P0 | Profil Pegawai - detail orang tunggal dan pemilihan/tambah peran | `FE-HRD-025`, `FE-HRD-028` | `FIN-HRD-04-BE-001`, `FIN-HRD-04-BE-002`, `FE-HRD-025` | Satu profil multi-role; `UAT-04-02..07`, `38` |
| `FIN-HRD-04-FE-003` | INTEGRATION | P0 | Profil Pegawai - form sensitive read-only & permohonan Menu 06 | `FE-HRD-027`, `FE-HRD-028` | `FIN-HRD-04-BE-003`, `FIN-HRD-04-BE-004`, `FE-HRD-027` | Bukan inline edit; request status benar; `UAT-04-08..11` |
| `FIN-HRD-04-FE-004` | FIX | P0 | Keluarga - detail anggota, kontak darurat, perubahan sensitif | `FE-HRD-025` | `FIN-HRD-04-BE-006` | Reuse API, request sensitif sah; `UAT-04-12..15`, `17` |
| `FIN-HRD-04-FE-005` | INTEGRATION | P0 | Keluarga - panel tanggungan/eligibility dan workflow | `FE-HRD-027` | `FIN-HRD-04-BE-007`, `FIN-HRD-04-FE-004` | FK benar, status approval akurat; `UAT-04-14..17` |
| `FIN-HRD-04-FE-006` | FIX | P1 | Pendidikan - form/daftar/upload/verify/highest dari API | `FE-HRD-025` | `FIN-HRD-04-BE-008` | File/lifecycle benar; `UAT-04-18..23` |
| `FIN-HRD-04-FE-007` | ADD | P1 | Riwayat Pekerjaan - pengalaman eksternal terstruktur | `FE-HRD-025` | `FIN-HRD-04-BE-009` | Edit/filter/persist tanpa data dummy; `UAT-04-24..25`, `28..29` |
| `FIN-HRD-04-FE-008` | FIX | P0 | Riwayat Pekerjaan - timeline internal dan tautan SK tanpa edit langsung | `FE-HRD-022`, `FE-HRD-024` | `FIN-HRD-04-BE-010`, `FE-HRD-022` | No manual edit SK; `UAT-04-26..27`, `29` |
| `FIN-HRD-04-FE-009` | SECURITY | P0 | Dokumen Personal - secure list/preview/download dan error | `FE-HRD-025` | `FIN-HRD-04-BE-011` | Tidak preload confidential; `UAT-04-30..32` |
| `FIN-HRD-04-FE-010` | FIX | P1 | Dokumen Personal - modal verifikasi, expiry, replacement | `FE-HRD-025` | `FIN-HRD-04-BE-012`, `FIN-HRD-04-FE-009` | Verifier yang sah saja; `UAT-04-33..36` |
| `FIN-HRD-04-FE-011` | INTEGRATION | P1 | Data Profesi - master selector, detail multi-role dan navigasi credentialing | `FE-HRD-025` | `FIN-HRD-04-BE-013`, `FIN-HRD-04-FE-002` | Profesi konsisten; `UAT-04-37..40` |
| `FIN-HRD-04-FE-012` | SECURITY | P0 | Profil Pegawai/Data Profesi - tampilan Employee 360 tersegregasi | `FE-HRD-025` | `FIN-HRD-04-BE-014`, `FE-HRD-025` | Masking dan 403 benar; `UAT-04-17`, `31`, `42` |
| `FIN-HRD-04-FE-013` | FIX | P1 | Lintas Submenu - state API, cache per profil, error dan double-submit | `FE-HRD-002` | `FIN-HRD-04-FE-001..012` | Tidak ada data silang/sukses palsu; `UAT-04-41`, `43..45` |
| `FIN-HRD-04-FE-014` | FIX | P1 | Lintas Submenu - Base Component, layout tablet/mobile, aksesibilitas | `FE-HRD-025` | `FIN-HRD-04-FE-001..013` | UI gate/lint CSS/responsive; `UAT-04-46` |
| `FIN-HRD-04-FE-015` | UAT | P0 | Lintas Submenu - UAT 48 kasus, regresi Menu 06/09/12 dan retest | `FE-HRD-002`, `FE-HRD-027` | `FIN-HRD-04-FE-001..014`, `FIN-HRD-04-BE-016` | Bukti live + honest status; `UAT-04-01..48` |

##### FIN-HRD-04-FE-001 - Navigasi resmi enam Submenu
- **Implementasi:** tambahkan menu induk persis `Data Induk Pegawai` dan enam child ke `src/utils/menu-sidebar/menu-items.jsx`; temukan route existing `master-data/employee`, WFP resources, profesi/dokter dan Employee 360. Reuse/redirect/alias aman agar tidak membuat kedua master data terpisah; permission konsisten terhadap role dan deep link.
- **Acceptance:** `UAT-04-01`, enam child bisa diakses, halaman lama tetap bekerja; `403/denied` tidak menjadi layar kosong atau redirect tak berujung.

##### FIN-HRD-04-FE-002 - Detail person dan alur tautkan peran
- **Implementasi:** rapikan detail pegawai dan profil bersama berbasis `WorkforceProfileId`; tampilkan role terhubung dan conflict resolution yang sesuai API, tidak pernah membuat profil client-side. Reuse `BaseGroupedEditorView`, list/detail hooks, `Employee360Service` dan base components; admin dapat memilih orang existing dengan aman.
- **Acceptance:** create/link/refresh dan transisi profile pegawai-dokter tidak menduplikasi identitas; error `409` informatif tanpa membocorkan NIK.

##### FIN-HRD-04-FE-003 - Sensitif -> ajukan perubahan, bukan edit form
- **Implementasi:** form existing tidak mengizinkan edit field sensitif inline; sediakan CTA `Ajukan Perubahan Data` ke Menu 06, status request dari server, upload bukti dan on-behalf saat diizinkan. Reuse/repair `FE-HRD-027` sebagai legacy baseline jika scope overlap dan tidak membuat form approval paralel.
- **Acceptance:** pencoba PUT lewat DevTools tidak bisa bypass backend; UI tetap menampilkan old value sampai `Applied`, dan reload menampilkan server state.

##### FIN-HRD-04-FE-004 - Anggota Keluarga dan Kontak Darurat
- **Implementasi:** gunakan `familyMembers`, `emergencyContacts` dalam WFP Redux dan editor existing; klasifikasi sensitive/normal, detail/contact form, error/empty/loading dan audit feedback. Identitas keluarga tidak terkirim ke views atasan yang tidak berhak.
- **Acceptance:** tambah data yang diizinkan nyata tersimpan, upaya sensitif mengarahkan ke permohonan, tidak memanggil direct UPDATE untuk sensitive.

##### FIN-HRD-04-FE-005 - Tanggungan, kelayakan dan permohonan
- **Implementasi:** reuse `dependents` dan koneksi anggota keluarga, tampilkan benefit/tax/insurance flags hanya pada role berwenang, periode efektif, status pending/applied dari Menu 06; tidak mengarang kuota benefit atau payroll calculation.
- **Acceptance:** FK/eligibility dan status sesuai server, tidak menampilkan data dari profil sebelumnya setelah pindah pegawai.

##### FIN-HRD-04-FE-006 - Pendidikan dan dokumen ijazah
- **Implementasi:** pakai `educations` pada WFP slice, multipart FormData, reusable upload modal, highest/verify actions dari endpoint existing, status file dan form validation; peran atasan hanya menerima ringkasan sah.
- **Acceptance:** list/detail/CRUD persisten, highest konsisten, ukuran file invalid memunculkan error nyata, tidak ada mock data.

##### FIN-HRD-04-FE-007 - Pengalaman kerja eksternal
- **Implementasi:** bentuk tab/daftar/filter pengalaman eksternal yang jelas terpisah dari riwayat SK; field pemberi kerja, jabatan, tanggal, ringkasan dan dokumen bukti memakai kontrak API `FIN-HRD-04-BE-009` yang telah disetujui. Bila API/schema belum tersedia, tampilkan `NOT AVAILABLE`/disabled yang jujur, bukan data local/hardcode.
- **Acceptance:** satu pengalaman tersimpan dan muncul kembali setelah reload; timeline periode valid dan tidak terhitung sebagai peristiwa SK.

##### FIN-HRD-04-FE-008 - Riwayat internal read-only dari transaksi resmi
- **Implementasi:** tampilkan timeline mutasi/promosi/rotasi/pemberhentian dari API WFP/SK; berikan tautan buka dokumen resmi pada route Menu 06 sesuai permission; sembunyikan dan server menolak edit/hapus langsung riwayat resmi.
- **Acceptance:** data efektif dan referensi SK sesuai database, tanpa local-only mutation atau tombol edit yang salah.

##### FIN-HRD-04-FE-009 - Dokumen Personal privat
- **Implementasi:** list dokumen, badge akses, preview dan download melalui authenticated endpoint/proxy; jangan membangun direct URL public untuk dokumen rahasia. Role gate, expired state dan 401/403/404/410 jelas; thumbnail tidak boleh dimuat sebagai leak tanpa hak.
- **Acceptance:** role tidak berhak tidak menerima file maupun preview, file tersimpan benar, klik download tidak menghasilkan halaman error diam-diam.

##### FIN-HRD-04-FE-010 - Verifikasi dokumen/expiry/penggantian
- **Implementasi:** verifikasi/undo hanya muncul untuk role yang berwenang, tampilkan actor, tanggal, due H-30/expired, indikator validasi file, replacement dan confirm. Hak verifikator diverifikasi server; jika gagal tampilkan error, jangan ubah badge lokal menjadi `Verified`.
- **Acceptance:** setelah upload/verify/reload hasil dan metadata tidak berubah sendiri, file lama tidak dapat diunduh jika sudah diganti sesuai kontrak.

##### FIN-HRD-04-FE-011 - Profil profesi lintas peran
- **Implementasi:** selector master aktif `Profession`/`Specialization`, detail peran Pegawai/Dokter dalam profil bersama, akses tautan riwayat sertifikasi/STR/SIP ke Menu 09 hanya melalui read API yang diizinkan; jangan melakukan clinical privilege approval di Menu 04.
- **Acceptance:** perubahan profesi yang sah persist, dua peran tertaut ke satu WFP, izin praktik tidak naik otomatis.

##### FIN-HRD-04-FE-012 - Employee 360 role-specific
- **Implementasi:** reuse `src/app/hr/employee-360/[id]` dan `Employee360Service`; render kondisi HR/DirectManager dari backend, tanpa memuat sensitive resource bagi atasan dan tanpa CSS-only hiding. Tampilkan 403 saat bukan bawahan dan error as-of-date secara benar.
- **Acceptance:** `UAT-04-42`, network payload tidak mengandung field dilarang untuk atasan, hak akses mengikuti atasan utama aktif.

##### FIN-HRD-04-FE-013 - Konsistensi state dan error handling
- **Implementasi:** audit WFP slice/hooks terhadap cache antar `WorkforceProfileId`, pagination/tab, abort/race of requests, `400/401/403/404/409/413/415`, duplicate-submit, refresh stale, cross-submenu propagation; hilangkan fallback fake success/demo dalam path produksi Menu 04.
- **Acceptance:** perpindahan pegawai A->B tidak menampilkan A; data setelah save survive reload; error tampil jelas dan tidak ada optimistic success palsu.

##### FIN-HRD-04-FE-014 - UI/UX Base Component dan QA tampilan
- **Implementasi:** jalankan `base-component-decision-gate` dan audit Base Component untuk Hero, DataTable, DataFilter, grouped form, buttons, dialogs, toast, StatusBadge; pakai CSS Modules/design tokens dan semua breakpoint. Jangan membuat komponen baru/extend breaking tanpa approval sesuai rule; lakukan lint + check-css-usage.
- **Acceptance:** enam child konsisten, keyboard accessible, tidak ada overflow di tablet, loading/skeleton/empty/denied/error terbaca.

##### FIN-HRD-04-FE-015 - UAT E2E dan regresi aplikasi nyata
- **Implementasi:** jalankan **seluruh 48 UAT Menu 04** dalam browser headed/live dengan akun uji berizin, network, screenshot aman; uji fungsi multi-role, upload/download, negatif, refresh, double-submit, Menu 06/09/12. Catat bug, perbaiki dalam scope, retest; bila environment tidak tersedia, `NOT RUN/BLOCKED` bukan PASS.
- **Acceptance:** status setiap `UAT-04-01..48` dapat ditelusuri ke BE/FE task dan evidence; tidak menyatakan complete jika P0/P1 masih gagal atau tidak diuji.

#### 2.6.5 Urutan eksekusi dan mekanisme report (wajib)

1. **M04-G1 - Backend P0 tanpa ketergantungan skema:** kerjakan `FIN-HRD-04-BE-003`, `FIN-HRD-04-BE-004`, `FIN-HRD-04-BE-005`, `FIN-HRD-04-BE-006`, `FIN-HRD-04-BE-007`, `FIN-HRD-04-BE-010`, `FIN-HRD-04-BE-011`, `FIN-HRD-04-BE-014` sejauh dependency memungkinkan. Bersamaan siapkan resolver `FIN-HRD-04-BE-001` / `FIN-HRD-04-BE-002` tanpa melanggar migration gate. Jangan menunda semua P0 karena satu schema blocker.
2. **M04-G2 - Backend P1 dan integrasi:** lanjutkan `FIN-HRD-04-BE-008`, `FIN-HRD-04-BE-009`, `FIN-HRD-04-BE-012`, `FIN-HRD-04-BE-013`, `FIN-HRD-04-BE-015`; selesaikan bagian `FIN-HRD-04-BE-001` / `FIN-HRD-04-BE-002` yang sah; tutup dengan `FIN-HRD-04-BE-016` dan targeted regression. Penambahan data eksternal `FIN-HRD-04-BE-009` boleh schema-gated.
3. **M04-G3 - Frontend functional:** `FIN-HRD-04-FE-001` diikuti `FIN-HRD-04-FE-002..012` berdasarkan readiness API tiap Submenu, tanpa menahan child lain yang sudah punya API nyata. Frontend tidak mengarang endpoint sebelum backend siap.
4. **M04-G4 - UI hardening dan UAT:** `FIN-HRD-04-FE-013`, `FIN-HRD-04-FE-014`, `FIN-HRD-04-FE-015`; validasi security, responsive, lint/build, 48 UAT, retest Menu 06/09/12; status live disimpan jujur.
5. **Setiap kali menyelesaikan satu task FIN:** catat jenis, source changed, AC checked, uji dan status; bedakan `SOURCE_CHANGED`, `TEST_PASSED`, `UAT_ACCEPTED`. Report `task/report/backend/FIN-HRD-04-BE-NNN.md` dan `task/report/frontend/FIN-HRD-04-FE-NNN.md` **hanya boleh dibuat setelah namespace FIN didaftarkan dan pola nama diterima governance/rule-output**; jangan memaksa report parser lama bila hanya menerima `BE-HRD-*`/`FE-HRD-*`. Untuk ID masih usulan, laporan progres ada pada ringkasan Agent dan ditandai `FINISHING_REGISTER_PENDING`. Report resmi task existing tetap menggunakan ID asalnya, tidak berubah.
6. **Rekonsiliasi canonical jika berwenang:** tautkan requirement/decision -> existing roadmap task -> `FIN-HRD-04-*` -> owner -> dependency -> acceptance -> `UAT-04-xx` -> report dalam register finishing khusus dan/atau appendix trace ke `backend-roadmap.md`, `frontend-roadmap.md`, `requirement-traceability.md` melalui **MODULE BLUEPRINT MODE / otorisasi dokumentasi** yang benar. **Jangan** memasukkan nomor FIN sebagai baris `BE-HRD-NNN` / `FE-HRD-NNN`; namespace dan nomor reguler tetap independen. Bila tidak berwenang, jangan mutasi dokumen tersebut; lanjutkan source yang sah lalu laporkan entri siap didaftarkan.
7. **Kriteria penutupan nomor FIN:** `PROPOSED` atau `FINISHING_REGISTER_PENDING` tidak sama dengan `DONE`. Setelah nomor FIN terdaftar pada register yang sah, source dan uji membuktikan AC, baru perbarui status berdasarkan evidence. Jangan menaikkan status `FE-HRD-027`/`FE-HRD-026` hanya karena task FIN frontend lulus lint.

---

## 3. Context

### 3.1 Repository, branch, SHA sumber, dan catatan bukti

| Repo | Branch wajib | HEAD saat dokumen disusun | Status |
|---|---|---|---|
| `DevBenari/QuilvianSystemFrontendDev` | `HamzahV2` | `20478cd652651e8e9b620aaa83cdbce3dcc1648e` | Source dibaca GitHub, **belum UAT** |
| `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | `1b8e56f3f7abebde9a613ef592430d3da0dfc4d1` | Source dibaca GitHub, **belum UAT** |
| `DevBenari/QuilvianEngineeringSkills` | `main` | `201af64552b27873fd9af83c6722cf11b441bb1e` | Governance read-only |

**Catatan revisi:** HEAD branch bergerak sejak draft pertama. Saat revisi kedua, status branch dan beberapa simbol kunci FE/BE (menu, WFP tab, create Employee/Doctor, update NIK/rekening, dokumen) diperiksa ulang; ini **bukan** audit penuh semua file pada commit baru. Agent wajib menilai delta source pada HEAD lokal.

Verifikasi ulang `HEAD`, upstream, working tree dan perubahan sejak snapshot sebelum menyentuh kode. Jangan otomatis checkout/pull untuk menyamakan SHA; laporkan bila berbeda dan revisi source evidence. `MODULE-STATUS.md`/roadmap dapat menyebut SHA historis dan label DONE; **itu bukan bukti kelulusan UAT pada HEAD ini**.

**Source FE utama yang telah dilihat:**

- `src/utils/menu-sidebar/menu-items.jsx`: submenu HR master `Karyawan`, `Dokter`, `Profesi`, `Spesialisasi`; Menu `Administrasi Kepegawaian` berada terpisah.
- `src/app/hr/master-data/employee/page.jsx`, `[employeeSlug]/page.jsx`, `[employeeSlug]/workforce/page.jsx`, `[employeeSlug]/workforce/[resourceKey]/page.jsx`; detail doktor serta `/hr/employee-360/[id]`.
- `src/components/view/hr/master-data/employees/master-data-employee-view.jsx` memakai base components seperti `AccessDeniedGate`, `BaseButton`, `DataFilter`, `DataTable`, `Hero`, `SummaryCards`; `detail/employee-detail-view.jsx`, `workforce/employee-workforce-manage-view.jsx`, `form/employee-form-view.jsx` (`BaseGroupedEditorView`).
- `src/components/features/workforce/workforce-profile-dashboard.jsx`, `workforce-resource-editor.jsx`, `workforce-profile-side-tabs.jsx`; `src/lib/constants/hr/workforce-profile/employee-workforce-tabs.jsx` dengan `personalInfo`, `familyMembers`, `educations`, `employmentHistories`, `documents`, `dependents` dan tab terkait.
- `src/lib/hooks/hr/master-data/employee/use-master-data-employee-detail.jsx` memanggil `getWfpResourceList`, `createWfpResourceItem`, `updateWfpResourceItem`, `deleteWfpResourceItem`, refresh summary; `src/lib/state/slice/hr/workforce-profile/workforce-profile-all.jsx` memetakan resource ke endpoint dan `multipart` untuk Pendidikan/Dokumen/Riwayat Pekerjaan.
- `src/lib/state/slice/hr/master-data/master-data-employee-slice.jsx` memakai `/v1/corporate/human-resource/master-data/employees`; `src/lib/services/hr/employee-360-service.js` memakai `/v1/corporate/human-resource/workforce-profiles/{id}/employee-360`.

**Source BE utama yang telah dilihat:**

- `Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/EmployeeController.cs`, `DoctorController.cs`, `ExternalUserController.cs` dan `Models/MstWorkforceProfile.cs`, `MstEmployee.cs`, `MstDoctor.cs`.
- `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpFamilyMemberController.cs`, `WfpDependentController.cs`, `WfpEmergencyContactController.cs`, `WfpEducationController.cs`, `WfpEmploymentHistoryController.cs`, `WfpDocumentController.cs`, `WfpBankAccountController.cs`.
- `Areas/Corporate/HumanResource/WorkforceCore/Controllers/EmployeeProfileChangeController.cs`, `Services/EmployeeProfileChangeService.cs`, `Services/HrdEmployee360Service.cs`.
- `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Controllers/ProfessionController.cs`, `SpecializationController.cs` serta controller credentialing dalam `CredentialingManagement`.
- `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpEmploymentHistory.cs`, `DTOs/WfpDocumentDtos.cs`, `DTOs/WfpEducationDtos.cs`, `DTOs/WfpFamilyMemberDtos.cs`.

**Bukti sumber langsung (read-only):**

- FE Menu/route: https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/HamzahV2/src/utils/menu-sidebar/menu-items.jsx
- FE tab/profile mapping: https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/HamzahV2/src/lib/constants/hr/workforce-profile/employee-workforce-tabs.jsx
- FE state API: https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/HamzahV2/src/lib/state/slice/hr/workforce-profile/workforce-profile-all.jsx
- BE pembuatan pegawai: https://github.com/DevBenari/NewQuilvianSystemBackend/blob/MHamzah/Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/EmployeeController.cs
- BE pembuatan dokter: https://github.com/DevBenari/NewQuilvianSystemBackend/blob/MHamzah/Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/DoctorController.cs
- BE pengajuan perubahan data: https://github.com/DevBenari/NewQuilvianSystemBackend/blob/MHamzah/Areas/Corporate/HumanResource/WorkforceCore/Controllers/EmployeeProfileChangeController.cs
- BE pengelolaan dokumen: https://github.com/DevBenari/NewQuilvianSystemBackend/blob/MHamzah/Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpDocumentController.cs
- Keputusan bisnis: https://github.com/DevBenari/NewQuilvianSystemBackend/blob/MHamzah/docs/module-blueprints/human-resource/00-interview-decisions.md
- Referensi instruksi implementasi Menu 06: `docs/module-blueprints/human-resource/docs/update/administrasi-kepegawaian/INSTRUKSI_AGENT_ADMINISTRASI_KEPEGAWAIAN_UAT_LIVE.md`.
- Referensi instruksi implementasi Menu 12: `docs/module-blueprints/human-resource/docs/update/pemberhentian-dan-pensiun/INSTRUKSI_AGENT_HRD_MENU_12_PEMBERHENTIAN_DAN_PENSIUN.md`.

### 3.2 Matriks status awal, reuse, repair, dan add

| Submenu | Bukti FE | Bukti BE | Status source awal, **BUKAN** hasil UAT | Tindakan wajib |
|---|---|---|---|---|
| Profil Pegawai | Master employee, detail, workforce tabs, Employee 360 | `EmployeeController`, `DoctorController`, `ExternalUserController`, `MstWorkforceProfile` | **REPAIR** | Identitas lintas tipe tunggal; blokir perubahan sensitif langsung; peran jamak dan hak akses |
| Keluarga | Tab `familyMembers`/`dependents` dan WFP Redux | `WfpFamilyMemberController`, `WfpDependentController`, DTO | **REPAIR** | Tutup mutasi sensitif langsung, gunakan workflow Menu 06 |
| Pendidikan | Tab `educations`, upload multipart, editor | `WfpEducationController`, DTO/form data/file | **IMPLEMENTED, NOT VERIFIED** | Validasi API/upload/verifikasi/tertinggi dan UAT, perbaiki defect yang terbukti |
| Riwayat Pekerjaan | Tab `employmentHistories`, laporan lintas pegawai | `WfpEmploymentHistoryController`, `WfpEmploymentHistory` | **PARTIAL** | Bedakan internal SK vs pengalaman eksternal; tambah data terstruktur jika diizinkan skema |
| Dokumen Personal | Tab `documents`, dashboard preview | `WfpDocumentController`, `IsConfidential`, file endpoints | **REPAIR** | Keamanan file/detail/download, kontrol verifikasi dan data rahasia |
| Data Profesi | Master profesi, form pegawai/dokter | `ProfessionController`, `MstEmployee`, `MstDoctor` | **PARTIAL / REPAIR** | Integrasi profesi untuk person-role ganda, validasi dan tautan kredensial |

**Belum dilakukan:** menjalankan aplikasi live, memeriksa database aktual, seluruh hasil API dengan akun role berbeda, test concurrency, UAT browser headed, build/lint terhadap perubahan baru. Jangan mengklaim enam Submenu telah PASS. **Menu 06 dan Menu 12 dilaporkan sudah diimplementasikan oleh user, bukan bukti semua regresi Menu 04 telah lulus.**

### 3.3 Kontrak API existing yang HARUS divalidasi ulang sebelum dipanggil FE

Base URL FE melalui `InstanceAxios` menggunakan `/v1`, sedangkan atribut controller BE berawalan `api/v1`; gunakan konfigurasi axios existing, tidak hardcode host.

| Resource | Metode dan endpoint BE existing (pola) | Permission di controller (contoh aktual) |
|---|---|---|
| Master pegawai | `GET/POST /api/v1/corporate/human-resource/master-data/employees`, `GET/PUT /{id}` | `Employee: Read/Create/Update` |
| Master dokter | `/api/v1/corporate/human-resource/master-data/doctors` dan varian `/admin` | `Doctor: Read/Create/Update` pada operasi admin |
| Master pengguna eksternal | `/api/v1/corporate/human-resource/master-data/external-users` | `ExternalUser: Read/Create/Update` |
| Keluarga | `GET/POST /api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/family-members`, `PUT /{id}` | `WfpFamilyMember: Read/Create/Update` |
| Tanggungan | `.../workforce-profiles/{workforceProfileId}/dependents` | `WorkforceDependent: Read/Create/Update` |
| Pendidikan | `.../workforce-profiles/{workforceProfileId}/educations`, `PATCH /{id}/highest`, `PATCH /{id}/verify`, `GET /{id}/file` | `WorkforceEducation: Read/Create/Update` |
| Riwayat internal | `.../workforce-profiles/{workforceProfileId}/employment-histories`, `PATCH /{id}/approval`, `GET /{id}/file` | `WfpEmploymentHistory: Read/Create/Update` |
| Dokumen | `.../workforce-profiles/{workforceProfileId}/documents`, `PATCH /{id}/verify`, `GET /{id}/file` | `WorkforceDocument: Read/Create/Update` |
| Profesi | `/api/v1/corporate/human-resource/master-data/professions` | `Profession: Read/Create/Update` |
| Employee 360 | `GET /api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employee-360` | `HrdEmployee360: Read` ditambah guard HR/direct-manager pada service |
| Permohonan perubahan data | `POST /api/v1/corporate/human-resource/employee-profile-changes/on-behalf`, `POST /{id}/start-verification`, `POST /{id}/verifications/{verificationId}/decision`, `POST /{id}/apply`, endpoint baca workflow/bukti | `EmployeeProfileChange: Create/Update/Read` ditambah pemeriksaan workflow role |

**Catatan penting:** `EmployeeProfileChangeController` memiliki endpoint `POST {id}/approve`/`reject` yang dalam source **menginstruksikan penggunaan Workflow Engine/Approval Inbox**; jangan mengasumsikan endpoint tersebut menjalankan approval bisnis. Lacak alur/status/DTO service dan integrasi yang sebenarnya sebelum menghubungkan tombol. Jangan mengarang URL baru atau response field; untuk data eksternal pekerjaan yang belum ada, tandai endpoint usulan sebagai `NEW` sampai diimplementasi dan diberikan test.

### 3.4 Governance dan dokumen yang wajib dibaca Agent

Sebelum mengubah source, baca dari **branch yang benar**:

1. FE `AGENTS.md`, `CLAUDE.md` dan BE `AGENTS.md`, `CLAUDE.md`; skill governance `agents/rules/GLOBAL_RULES.md`, `agents/rules/README.md` pada Engineering Skills `main`.
2. FE: `rules/frontend/frontend-architecture.md`, `base-component-catalog.md`, `base-component-decision-gate.md`, `page-composition-patterns.md`, `master-data-feature-standard.md`, `design-tokens.md`, `ui-consistency-checklist.md`, `test-policy.md`, `REPORT_TEMPLATE.md` sejauh task terkait.
3. BE: `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `API_RULES.md`, `DATABASE_RULES.md`, `CROSS_REPO_RULES.md`, `REVIEW_RULES.md`, `TEST_POLICY.md` serta kebijakan akses. `QBE-MOD-002`, `QBE-NAM-004`, naming owner/prefix dan legacy ratchet wajib diikuti; jangan memakai `Trx*` baru sebagai jalan pintas.
4. HRD source of truth: `docs/module-blueprints/human-resource/{README.md,MODULE-STATUS.md,00-business-overview.md,00-interview-decisions.md,01-existing-capability-map.md,01-prerequisite-readiness.md}`, `roadmap/00-slice-roadmap.md`, `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`, `contracts/`, `task/report/` dan audit unfinished yang masih relevan.
5. UAT/testing: aturan `rules/testing/` versi yang tersedia dari suite Engineering Skills dan aturan laporan/screenshot browser headed; jangan menggunakan laporan historis sebagai hasil UAT sekarang.

**Pemetaan nomor task:** existing `BE-HRD-003`, `BE-HRD-023..028`, `FE-HRD-002`, `FE-HRD-022..028` berasal dari roadmap resmi. Finishing `FIN-HRD-04-BE-001..016` dan `FIN-HRD-04-FE-001..015` adalah **FINISHING_REGISTER_PENDING** dalam namespace baru yang tidak tergantung nomor akhir roadmap reguler. Tabel §2.6 mengandung kolom referensi task lama dan jenis pekerjaan. Verifikasi HEAD, collision pada namespace FIN dan status terbaru sebelum registrasi; source lebih baru dapat mengubah prioritas repair, tetapi **tidak** menggugurkan mandat implementasi.

### 3.5 Prasyarat dan batas data

- Dilarang memuat NIK, nomor rekening, kartu keluarga, dokumen medis atau identitas pegawai **riil** ke prompt, memory, screenshot, laporan task, maupun console log. Gunakan data uji yang disetujui (dummy berlabel test di environment UAT; jangan membuat fallback dummy ke aplikasi produksi).
- Gunakan user/account UAT yang sah sesuai hak yang dimiliki; jangan mencari atau mengakali credential. Minimal uji: HR Admin, HR Verifier terpisah, HR Manager, atasan utama, pegawai, dokter yang juga pegawai, dan pengguna tanpa akses. Jika tidak tersedia, status `NOT RUN — missing authorized account`, bukan PASS.
- Jangan melakukan schema/data reset, impor massal, remapping referensi profil atau penggabungan record existing tanpa evaluasi dan izin spesifik. Tindakan perbaikan data historis adalah pekerjaan terpisah.
- `docs/module-blueprints/human-resource/memory.md` **belum ditemukan** pada branch saat diperiksa: `MEMORY_NOT_INITIALIZED`; jangan mengklaim synced. Perbarui checkpoint hanya jika mode dokumentasi dan hak tulis memang ada.

---

## 4. Reasoning / Execution Workflow

Bagian ini adalah **urutan kerja operasional yang dapat diverifikasi**, bukan permintaan mengungkap penalaran privat Agent.

### 4.1 Preflight singkat — lalu langsung IMPLEMENTASI

1. Pastikan workspace memuat FE `HamzahV2` dan BE `MHamzah`, cek `git branch --show-current`, upstream, `git rev-parse HEAD`, `git status --short` **read-only**. Jangan mengubah branch/upstream otomatis; bedakan perubahan lokal milik orang lain dan jangan menimpa.
2. Baca governance 3 repo yang relevan dari §3.4; pastikan registered runtime skills tersedia dan benar-benar diinvoke sebelum edit source. Cocokkan path/kontrak di §3 dengan HEAD saat Agent bekerja. Bila ada drift, sesuaikan tindakan berdasarkan source aktual, jangan batalkan mandat implementasi karena audit historis berbeda.
3. Cek canonical untuk keputusan `HRD-DEC-051`, `052`, `053`, dan keputusan riwayat eksternal. **Requirement enam Submenu dan empat temuan P0/P1 sudah dikunci dalam pembahasan**; jangan menjalankan loop wawancara/grill-me atas hal yang sama. Jika canonical perlu sinkronisasi, lakukan fase `MODULE BLUEPRINT MODE` **dengan target dokumen yang sah** untuk menyerap keputusan baru dan traceability; laporkan `NEEDS_SYNC` bila tidak ada hak tulis, namun kerjakan bagian source lain yang sudah approved.
4. Jalankan pencarian terarah pada endpoint/model/route yang disebut, buat matriks `REUSE/REPAIR/ADD` per file dan catat temuan maksimum seperlunya. **Begitu preflight terpenuhi, mulai membuat perubahan nyata — bukan menyerahkan matriks sebagai hasil akhir.**
5. **Periksa register §2.6 satu kali sebagai preflight task:** pastikan existing task sudah ada, task baru belum bertabrakan di roadmap HEAD, catat status PROPOSED/REGISTERED yang benar. Gunakan daftar ini untuk **langsung mengimplementasikan** task BE/FE sesuai dependency dan gate; registrasi blueprint yang belum berizin tidak boleh menghambat implementasi independen.

### 4.2 Backend-first: perbaikan P0 yang wajib dikerjakan dahulu (`FIN-HRD-04-BE-001..007`, `FIN-HRD-04-BE-010..011`, `FIN-HRD-04-BE-014`)

1. **Identitas tunggal lintas peran.** Implementasikan resolver/transaksi lintas `EmployeeController`, `DoctorController`, `ExternalUserController`, model profil dan relasi user yang sekarang benar-benar dipakai. Reuse WFP dari identitas sah yang sudah ada, jangan clone, cegah race create ganda; bila perubahan struktur diperlukan, pisahkan bagian blocked per gate migration. Audit/review akses berbasis `UserType` single agar tidak merusak login/role legacy. Tambahkan unit/integration tests untuk overlap peran, duplicated NIK/paspor, rehire dan concurrent-create.
2. **Guard data sensitif lintas seluruh write surface.** Periksa tidak hanya form FE tetapi PUT/PATCH/POST langsung pada `EmployeeController`, Doctor, External, `WfpBankAccountController`, `WfpFamilyMemberController`, `WfpDependentController`, dan API terkait. Terbitkan error yang konsisten ketika sensitivitas memerlukan permohonan; jangan memakai izin `Update` generik untuk mengesampingkan `HRD-DEC-052`. Jangan merusak inisialisasi pegawai baru atau update non-sensitif yang sah.
3. **Workflow Menu 06.** Gunakan `EmployeeProfileChangeService` dan applier existing untuk NIK, status perkawinan, keluarga/tanggungan serta rekening yang memang disetujui. Perluas field support/projection dan bukti jika belum lengkap, simpan nilai lama sampai `apply` server berhasil. Verifier harus berbeda dari pembuat; uji role actor, request status dan error case. **Jangan bypass approval lewat pemanggilan endpoint `approve` stub atau Redux success lokal.**
4. **Proteksi dokumen.** Audit daftar/detail/file/public-URL/export dan filter `IsConfidential`; tutup semua jalur data rahasia tanpa otorisasi record-level. Pisahkan hak verifikasi dan audit bila diperlukan kontrak. Jangan menghilangkan akses HR yang sah.
5. **Uji backend P0 sebelum lanjut:** request/response autentikasi, status HTTP, transaksi rollback, persistensi, dua request serentak, dan regresi Menu 06/12 yang terdampak. Jika test gagal, **perbaiki langsung dalam target** dan jalankan ulang.

### 4.3 Backend P1: lanjutkan pendidikan, riwayat pekerjaan, profesi, dan konsistensi (`FIN-HRD-04-BE-008..009`, `FIN-HRD-04-BE-012..013`, `FIN-HRD-04-BE-015..016`)

1. Pendidikan: stabilkan `multipart`, validasi file, hanya satu pendidikan tertinggi, verifikasi/status, audit, data hasil filter dan ringkasan; jangan tambah schema tanpa kebutuhan yang terbukti.
2. Riwayat pekerjaan: tambah data pengalaman eksternal yang benar-benar **persisten** sesuai izin schema, dengan penanda sumber terstruktur; lindungi peristiwa resmi SK/offboarding dari edit generic. Periksa migration existing dan ikuti aturan prefix/owner. **Jangan membuat atau menjalankan migration secara otomatis**; bila belum diizinkan, siapkan perubahan independen dan tulis blocker yang spesifik untuk storage baru.
3. Profesi: pastikan relasi profesi/spesialisasi legal/aktif, peran dokter+pegawai berkaitan ke satu profil; jangan mengubah izin praktik atau credentialing decision milik Menu 09.
4. Kontrak FE/BE: DTO menyertakan field yang diperlukan view namun tidak bocor, status/error code terdefinisi, summary dihitung dari DB, tidak dari angka contoh atau state server sementara. Tambahkan test dan jalankan rule QBE sesuai task, catat temuan/fix.

### 4.4 Frontend: benar-benar bangun enam akses UI dan integrasi API (`FIN-HRD-04-FE-001..015`)

1. Gunakan `quilvian-engineering-skills:build-module-frontend` terlebih dahulu. Lakukan base-component-decision-gate sebelum menulis JSX/CSS; urutan preferensi `REUSE` → `EXTEND` → `COMPOSE` → `WRAP` → `NEW`. **Komponen baru `NEW` yang benar-benar tidak memiliki ekuivalen memerlukan keputusan owner**; kerjakan dahulu seluruh komposisi yang tidak bergantung padanya.
2. Integrasikan `Data Induk Pegawai` dan 6 anak ke `menu-items.jsx`/permission menu existing. Setiap child wajib bisa dibuka, memiliki pemilihan profil yang benar, deep link dan back link stabil, serta tidak membuat duplikasi editor/data. Nama submenu tidak berubah. Pertahankan halaman master Karyawan/Dokter/Profesi, Administrasi Kepegawaian dan Pemberhentian & Pensiun tetap berfungsi.
3. Gunakan `BaseGroupedEditorView`, `DataTable`, `Hero`, `BaseButton`, `DataFilter`, `StatusBadge`, `SummaryCards`, `ConfirmModal`, `ToastStack`, `AccessDeniedGate`, `EmployeeSelect`/filter existing sesuai catalog. Hormati `design-tokens`, CSS Modules, spacing/layout dan responsive. Tidak boleh membuat layout berantakan/tabel meluber di tablet, teks terpotong, modal terselip, form dua kolom yang rusak di mobile.
4. Gunakan `InstanceAxios`, Redux/hook/services existing dan `getWfpResourceList` + CRUD WFP yang memang sesuai. Tambahkan service hanya jika ada kontrak baru; jangan buat axios baru, panggilan host hardcode atau aksi yang hanya menyimpan data ke localStorage/local state.
5. Lengkapi state **loading, error, empty, denied, success nyata, validation** per Submenu. Tanggapi `400/401/403/404/409/413/415` tanpa mengubahnya menjadi sukses palsu. Pasca simpan gunakan refetch dan pastikan data benar setelah reload/ubah tab/navigasi ke pegawai lain; jangan bawa resource pegawai A ke pegawai B lewat cache Redux.
6. Untuk field sensitif, sembunyikan aksi edit langsung dan tampilkan alur `Ajukan Perubahan Data` dari Menu 06 dengan status draft/verification/approved/applied sesuai server; pertahankan nilai efektif sebelum applied. UI tidak boleh dapat mengubah aturan lewat DevTools maupun manipulasi payload jika guard backend benar.
7. Pastikan semua aksi file melalui API/proxy terautentikasi dengan error handling; jangan menjadikan `IsConfidential` sebatas badge. `Dokumen Personal` hanya menampilkan data yang memang diizinkan role. Jangan menampilkan NIK/paspor/kontak keluarga pada Employee 360 atasan.

### 4.5 Integrasi lintas repo dan regresi yang wajib dicakup

- **Menu 06 Administrasi Kepegawaian:** ajukan perubahan data atas nama/ESS, bukti, verifikator berbeda, workflow apply, peristiwa SK, ringkasan Employee 360, nomor unit/jabatan. Data master yang baru terhubung harus tampil benar tanpa menciptakan jalur bypass.
- **Menu 12 Pemberhentian & Pensiun:** pegawai nonaktif tidak menghilangkan profil atau histori; proses clearance tetap mengakses person ID yang sama; jangan membuat akun aktif kembali akibat update data induk.
- **Menu 09 Kompetensi & Kredensial:** profesi/pendidikan bersumber dari data sah; status STR/SIP dan privilege tetap domain owner, tidak naik otomatis ketika form profesi berubah.
- **Finance/Benefit:** perubahan rekening/tanggungan efektif baru boleh diproyeksikan setelah approval; jangan menulis langsung tabel payroll/benefit di Menu 04 tanpa kontrak owner. Jika event/integrasi belum mendukung, catat gap nyata, bukan mengarang berhasil.
- **Identity/permissions:** role HR, verifier, atasan, pegawai, doctor dan non-authorized tidak saling melihat data sensitif; endpoint guard mengalahkan hide/show UI.

### 4.6 Test case UAT live browser dan API — wajib jalankan, perbaiki, retest

**Trace task UAT:** setiap kasus `UAT-04-01..48` harus menunjuk ID `FIN-HRD-04-BE-001..016` dan/atau `FIN-HRD-04-FE-001..015` pada §2.6, plus task existing yang digunakan kembali. Case dengan beberapa owner mencantumkan beberapa ID; jangan menyimpulkan task registered hanya karena memiliki mapping UAT.

UAT ditulis sebagai **satu laporan Menu 04** dengan bukti **browser headed/live** (apabila runtime dan akun UAT berizin tersedia). Untuk setiap kasus simpan: ID, prioritas, persona, langkah klik/HTTP, data uji anonim, expected vs actual, network request/response ringkas tanpa PII, screenshot sebelum/sesudah, status `PASS`, `FAIL`, `NOT RUN`, atau `BLOCKED`, bug, file yang diperbaiki, dan hasil retest. **Jangan klaim PASS dari membaca source atau unit test saja.**

| ID | Prioritas | Skenario UAT / expected |
|---|---|---|
| `UAT-04-01` | P0 | HR membuka `Data Induk Pegawai`; enam submenu muncul tepat, seluruh link/route hidup dan permission tepat. |
| `UAT-04-02` | P0 | Buat pegawai WNI baru dengan NIK uji valid; 1 profil workforce terbuat dan detail persisten setelah refresh. |
| `UAT-04-03` | P0 | Daftarkan Dokter dengan NIK profil Pegawai yang sudah ada; **tautkan peran**, tidak membuat profil kedua. |
| `UAT-04-04` | P0 | Daftarkan Pegawai dari Dokter yang sudah ada; 1 profil, dua relasi, tampilan peran konsisten. |
| `UAT-04-05` | P0 | Tambahkan Pengguna Eksternal dengan identitas yang sudah ada; satu profil, tidak muncul orphan. |
| `UAT-04-06` | P0 | Dua submit concurrent pada identitas yang sama; tidak menghasilkan profil ganda/duplicate side effect. |
| `UAT-04-07` | P0 | NIK WNI invalid/paspor WNA tidak sah sesuai kontrak ditolak tanpa write. |
| `UAT-04-08` | P0 | PUT langsung mengganti NIK pegawai existing ditolak; field sensitif efektif tidak berubah. |
| `UAT-04-09` | P0 | HR membuat permohonan data sensitif via Menu 06 dengan bukti; request tersimpan, nilai lama tetap. |
| `UAT-04-10` | P0 | Pembuat mencoba memverifikasi request sendiri; ditolak dan tidak diterapkan. |
| `UAT-04-11` | P0 | HR verifikator lain menyelesaikan workflow yang sah; `apply` menerapkan nilai baru sekali, audit dan reload benar. |
| `UAT-04-12` | P0 | Tambah anggota keluarga pegawai dan baca ulang setelah refresh; relasi ke profil benar. |
| `UAT-04-13` | P0 | PUT data anggota keluarga yang berdampak pajak/benefit secara langsung ditolak. |
| `UAT-04-14` | P0 | Pengajuan perubahan pasangan/tanggungan dengan bukti via Menu 06; dua aktor, nilai lama berlaku sampai applied. |
| `UAT-04-15` | P1 | Ubah kontak darurat non-sensitif sesuai izin; audit dan tampilan tersinkron. |
| `UAT-04-16` | P1 | Kaitkan anggota keluarga dengan tanggungan; FK profil salah/anggota terhapus ditolak. |
| `UAT-04-17` | P0 | Manajer dan akun tanpa hak tidak dapat melihat rincian keluarga/tanggungan bawahan atau pegawai lain. |
| `UAT-04-18` | P1 | Tambah pendidikan D3 dan S1; field institusi, jurusan dan tahun tersimpan setelah reload. |
| `UAT-04-19` | P1 | Unggah PDF ijazah yang valid; server simpan dan unduh hanya untuk role yang sah. |
| `UAT-04-20` | P1 | File lebih 10 MB/ekstensi terlarang ditolak; tidak ada metadata/file setengah tersimpan. |
| `UAT-04-21` | P1 | Set pendidikan tertinggi; ringkasan benar dan tepat satu entry tertinggi. |
| `UAT-04-22` | P1 | Dua request bersamaan menetapkan pendidikan tertinggi; tidak terjadi dua primary aktif. |
| `UAT-04-23` | P1 | Verifikasi, ubah, replace/delete file pendidikan; status server/UI mengikuti aturan, audit benar. |
| `UAT-04-24` | P1 | Tambah pengalaman kerja sebelum RS dengan institusi, jabatan, periode, bukti; semua atribut terstruktur persisten. |
| `UAT-04-25` | P1 | Filter pengalaman sebelumnya vs peristiwa kerja internal tidak mencampur referensi. |
| `UAT-04-26` | P0 | Mutasi/promosi via SK Menu 06 muncul pada riwayat internal, dengan referensi yang benar. |
| `UAT-04-27` | P0 | Percobaan edit/hapus peristiwa internal resmi melalui CRUD generic ditolak. |
| `UAT-04-28` | P1 | Tanggal pengalaman tidak valid dan dokumen bukti tidak sesuai ditolak tanpa data parsial. |
| `UAT-04-29` | P1 | Pegawai pensiun/resign (Menu 12) masih mempunyai histori eksternal dan internal yang sama. |
| `UAT-04-30` | P0 | HR berwenang unggah/lihat/unduh dokumen privat melalui endpoint authenticated; refresh konsisten. |
| `UAT-04-31` | P0 | Akun tanpa izin dan manager tidak bisa GET detail/download file `IsConfidential` (403/404 sesuai kontrak). |
| `UAT-04-32` | P0 | Akses URL `FileUrl`/static path langsung tidak mem-bypass otorisasi dokumen rahasia. |
| `UAT-04-33` | P1 | Verify dokumen oleh aktor sah dan gagal oleh aktor yang tidak berhak; audit verifier persisten. |
| `UAT-04-34` | P1 | File salah MIME/ekstensi/ukuran ditolak; tidak ada success palsu. |
| `UAT-04-35` | P1 | Dokumen expired/dalam 30 hari ditampilkan benar sesuai waktu acuan, status/filter berfungsi. |
| `UAT-04-36` | P1 | Ganti dan hapus file personal tidak meninggalkan akses lama terbuka; metadata tetap konsisten. |
| `UAT-04-37` | P1 | Pilih profesi dari master aktif untuk Pegawai; detail, list dan refresh konsisten. |
| `UAT-04-38` | P0 | Dokter sekaligus pegawai mempunyai dua role tetapi satu `WorkforceProfileId`; riwayat/relasi tidak terbelah. |
| `UAT-04-39` | P1 | Dokter memakai profesi+spesialisasi valid; profesi inactive/invalid ditolak. |
| `UAT-04-40` | P0 | Menetapkan profesi tidak otomatis mengaktifkan STR/SIP, izin praktik atau kewenangan klinis. |
| `UAT-04-41` | P1 | Tidak ada master profesi baru/duplikat; Submenu Data Profesi mengarah ke data peran existing. |
| `UAT-04-42` | P0 | Atasan utama mendapat Employee 360 bawahan terbatas; bukan bawahan 403; gaji, NIK, dokumen, keluarga tidak bocor lewat API. |
| `UAT-04-43` | P1 | Pindah pegawai A ke B dan refresh/tab navigasi; cache/tab FE tidak menampilkan data A pada B. |
| `UAT-04-44` | P1 | Double-submit simpan pada form/dokumen; backend tidak menggandakan mutation tanpa sengaja. |
| `UAT-04-45` | P1 | API offline/401/403/404/409/413/415 tampil sebagai error yang jelas; UI tidak menunjukkan sukses palsu. |
| `UAT-04-46` | P2 | Desktop/tablet/mobile: sidebar, tabel, forms, viewer file dan modal responsif, fokus keyboard dan pesan validasi terbaca. |
| `UAT-04-47` | P0 | Regresi Menu 06: pengajuan/approval dan SK tidak rusak setelah guard data induk diterapkan. |
| `UAT-04-48` | P0 | Regresi Menu 12: offboarding/nonaktif dan arsip riwayat tetap benar, tidak muncul profil baru. |

**Kewajiban validasi teknis tambahan:**

- Backend: targeted unit/integration tests untuk identity resolution, permission record-level, apply workflow, DB transaction/rollback, file download conf guard, multi-user concurrency, history integrity dan invalid DTO. Jangan query/ubah DB production; gunakan database test yang sah bila ada, atau `NOT RUN`.
- FE: lint, type/build sesuai policy, test route/data mapping/form, `check-css-usage`, tes base component/UI gate. Perbaiki warning baru yang disebabkan perubahan Menu 04; jangan refactor massal di luar tugas.
- Browser: **headed mode** pada aplikasi yang sah, login sebagai beberapa role, klik tombol nyata, inspeksi network, reload, retest setelah bug. Bila tidak ada aplikasi hidup atau akun authorized, tandai `NOT RUN/BLOCKED`, jangan mengganti browser dengan simulasi laporan PASS.
- Keamanan: cek akses horizontal profil A/B, `IsConfidential`, file link, role, sensitive-write bypass lewat direct HTTP, payload/DTO masking dan audit aktor. Jangan menyimpan screenshot bukti dengan PII nyata.

### 4.7 Definition of Done — KAPAN boleh menyatakan Menu 04 selesai

**Semua ketentuan berikut wajib terbukti, bukan dicentang berdasarkan asumsi:**

1. Enam Submenu persis muncul dan dapat diakses lewat sidebar/route, dan mereuse sumber data canonical.
2. Seluruh guard P0 (profil ganda, perubahan sensitif, data rahasia, record-level permission) tersedia di backend dan lulus negative test.
3. Seluruh data hasil create/update/approval persist setelah reload; tidak ada data demo/hardcode success, state in-memory bisnis atau referensi file bocor.
4. Riwayat pekerjaan luar RS tersimpan terstruktur dan histori internal terlindungi; jika perubahan skema masih blocked, **Menu 04 belum dapat dinyatakan selesai**.
5. Pendidikan, keluarga, profesi, dokumen dan Employee 360 mengikuti permission/kontrak, tak ada kerusakan Menu 06/12, dan status kredensial Menu 09 tidak dinaikkan otomatis.
6. Validasi teknis sesuai test policy dicatat lengkap (command, waktu, hasil sebenarnya). UAT headed untuk kasus P0/P1 selesai PASS, atau status belum siap tetap **PARTIAL/BLOCKED/NOT VERIFIED**.
7. Nomor FIN dan referensi roadmap existing pada §2.6 direkonsiliasi tanpa benturan, semua status berbukti. Laporan tracked FIN hanya dibuat bila format ID FIN dan pendaftarannya **disetujui governance**, requirement traceability diperbarui hanya lewat mode/izin dokumentasi sah, dan satu laporan UAT Menu 04 memuat evidence aktual. Jangan menyatakan FIN terdaftar atau report tersimpan tanpa bukti.
8. Tunjukkan daftar file source yang benar-benar diubah, status Git read-only, risiko residual, dependency yang perlu izin, dan tindakan berikutnya. Tidak ada commit/push/deploy otomatis.

---

## 5. Stop conditions

**JANGAN BERHENTI karena:** source sudah mempunyai sebagian CRUD, roadmap berlabel `DONE`, berhasil membuat desain/layout, lint pass, laporan sudah ditulis, ada bug yang masih bisa diperbaiki di scope, atau Anda merasa audit sudah cukup. Setiap hasil pemeriksaan adalah masukan untuk **langsung memperbaiki kode** pada tahap yang sah.

**BERHENTI HANYA PADA AREA TERDAMPAK** dengan alasan eksplisit dan tetap lanjutkan pekerjaan lain yang independen bila:

1. Workspace bukan pada branch `HamzahV2`/`MHamzah` yang diberi wewenang atau status upstream/working tree berisiko menimpa pekerjaan orang lain; jangan checkout/pull/reset otomatis.
2. `AGENTS.md`, dokumen kontrak backend, root `rules/` atau suite governance canonical tidak terbaca: laporkan `BLOCKED — canonical governance unavailable` beserta file tepat. Jika `build-module-backend`/`build-module-frontend` tidak tersedia di runtime: laporkan `BLOCKED — Quilvian runtime skill unavailable` dan jangan bypass.
3. Perlu **membuat/menjalankan migration** atau menulis DB untuk model baru, constraint identitas global atau riwayat pengalaman eksternal tetapi user belum mengizinkan secara terpisah; **jangan buat/jalankan**. Catat model/kolom yang perlu skema, rencana migration non-destruktif, risiko dan langkah persetujuan; lanjutkan kode independen.
4. Diperlukan kebijakan owner baru terkait merger dua profil historis (`HRD-Q-67`), hak kredensial/klinis, kewajiban hukum ketenagakerjaan, akses Finance, atau perubahan base component `NEW`/breaking; jangan menentukan sendiri kebijakan material yang belum disetujui. Gunakan implementasi yang tidak membutuhkan keputusan tersebut.
5. Akses UAT, akun uji, server runtime atau database test tidak tersedia: **jangan mengarang PASS**. Lanjutkan source/test yang sah, tandai browser/db test `NOT RUN`, laporkan kebutuhan minimum.
6. Bukti mengarah ke perubahan lintas domain di luar Menu 04 yang belum diberi wewenang, misalnya engine payroll, clinical privilege, production Identity atau SK Menu 06 yang berisiko; isolasi defect, gunakan endpoint resmi, jangan mengubah modul owner tanpa persetujuan.
7. Terlihat kemungkinan kehilangan data, pengeksposan data pribadi riil, tindakan destruktif, atau kebutuhan mengubah database/shared production; hentikan tindakan berisiko, catat insiden tanpa menyalin data sensitif dan minta otorisasi spesifik.

**Semua operasi berikut selalu dilarang secara otomatis tanpa instruksi eksplisit terpisah:** `git commit`, `git push`, `git pull`, `git merge`, `git rebase`, checkout branch, deploy, membuat/menjalankan migration, menjalankan update database/SQL destruktif, mengubah production shared DB atau mengirim data pegawai asli ke log/chat. Izin mengedit kode lokal **tidak** memberi izin untuk langkah tersebut.

---

## 6. Output

### 6.1 Hasil nyata yang wajib dikembalikan Agent

**Bukan sekadar analisis atau proposal.** Agent menyerahkan perubahan source sesuai batas izin, disertai bukti dan format ringkas:

1. **Matriks keenam Submenu** dalam urutan persis: `IMPLEMENTED`, `PARTIAL`, `REPAIR`, `BLOCKED`, `NOT VERIFIED` atau `VERIFIED WORKING` jika memang ada uji memadai; sertakan bukti route/UI/API/service/model, test yang membuktikan, serta bug/fitur yang benar-benar diselesaikan.
2. **Daftar file yang diubah** terpisah FE dan BE, class/route/function, alasan, ringkasan perubahan/diff, endpoint/method DTO/mapping, dan catatan perubahan kontrak serta dampak backward compatibility.
3. **Kriteria keamanan**: bukti satu profil lintas role, guard sensitive writes, akses dokumen rahasia, permission Employee 360, dan histori internal resmi tidak dapat diedit liar, termasuk failed negative tests dan perbaikan berikut retest.
4. **Hasil test teknis**: command, timestamp, `PASS/FAIL/NOT RUN`, ringkasan error, jumlah test, lint, build, test policy, CSS checker, QBE checker dan perbaikan yang telah dilakukan. Jangan menulis sukses jika tidak dieksekusi.
5. **Satu laporan UAT per Menu 04** (bukan enam laporan utama UAT), meliputi seluruh `UAT-04-01` s.d. `UAT-04-48`, role/persona, request/response aman, screenshot atau bukti yang memang dibuat, bug/retest, serta status belumnya yang jujur. Lokasi mengikuti `rules/testing/` dan `rules/rule-output/` saat runtime; jika lokasinya tidak ditemukan, jangan karang path.
6. **Register finishing bernomor FIN** (§2.6) berisi status seluruh `FIN-HRD-04-BE-001..016` dan `FIN-HRD-04-FE-001..015`, jenis, referensi roadmap existing, dependency, perubahan file, acceptance dan UAT. Bila aturan pelaporan resmi sudah menerima namespace FIN **dan ID terdaftar**, buat report yang diizinkan pada `docs/module-blueprints/human-resource/task/report/backend/<FIN-TASK-ID>.md` atau `task/report/frontend/<FIN-TASK-ID>.md`; bila belum, jangan mengarang report dan tandai `FINISHING_REGISTER_PENDING` / `REPORT_FORMAT_GATE_PENDING` sesuai kondisi. Laporan existing tetap menggunakan ID aslinya. Dokumen master instruksi ChatGPT tetap satu.
7. **Daftar blocker & izin yang masih diperlukan** (terutama schema migration, merger profil historis, data uji berotorisasi, akses database/UAT). Pisahkan dari bagian yang berhasil dikerjakan; beri satu langkah tindak lanjut spesifik per blocker.
8. **Status Git read-only dan handoff**: branch/upstream/HEAD aktual, jumlah file lokal berubah, tidak ada commit/push yang dilakukan, checkpoint `memory.md` hanya bila berizin atau `MEMORY_PENDING_SYNC/MEMORY_NOT_INITIALIZED`.

### 6.2 Lokasi master instruksi dan cara pemakaian

Nama file master instruksi ini adalah **`INSTRUKSI_AGENT_HRD_MENU_04_DATA_INDUK_PEGAWAI.md`**. Bila perlu disimpan dalam workspace backend dengan izin dokumentasi yang sah, lokasi rencana: `docs/module-blueprints/human-resource/docs/update/data-induk-pegawai/INSTRUKSI_AGENT_HRD_MENU_04_DATA_INDUK_PEGAWAI.md`. File tersebut **tidak otomatis berada di GitHub hanya karena instruksi ini dibuat**; jangan commit/push tanpa izin. Jangan membuat master prompt terpisah per Submenu.

### 6.3 Format laporan akhir Agent

Gunakan Bahasa Indonesia, dengan urutan:

1. **Register task bernomor** — untuk semua 16 BE + 15 FE finishing: `FIN ID | Jenis | Referensi roadmap existing | PROPOSED/REGISTERED | SOURCE_CHANGED | TEST_PASSED/FAIL/NOT RUN | UAT | Blocker | Evidence`, plus daftar task existing yang direuse; jelaskan jika ID ditautkan/digabung ke task existing atau terjadi collision FIN tanpa menimpa ID lama.
2. **Perubahan kode yang benar-benar dilakukan** — ringkasan per Submenu.
3. **Status semua Submenu** — tabel 6 baris, bukti FE/BE dan runtime.
4. **Validasi & UAT** — hasil nyata, gagal, perbaikan dan retest, bukti browser.
5. **Keamanan dan integrasi Menu 06/09/12** — PASS/FAIL/NOT RUN yang terbukti.
6. **Blocker/izin dan next action** — hanya yang material.
7. **Status Git & memory** — jujur, tanpa klaim sync/commit yang tidak terjadi.

**URUTAN TASK WAJIB:** gunakan Register §2.6 sebagai antrian bernomor, dahulukan BACKEND P0, backend P1 yang independen, FRONTEND per Submenu, lalu QA/UAT; perbaiki FAIL dan jalankan retest. Status PROPOSED/REGISTRATION_PENDING tidak boleh digunakan untuk menyembunyikan pekerjaan source yang belum dikerjakan.

**PERINTAH FINAL UNTUK AGENT: Setelah preflight branch, governance dan runtime Skill dinyatakan sah, MULAI SEKARANG memperbaiki source backend kemudian frontend untuk keenam Submenu Data Induk Pegawai. Lakukan implementasi, pengujian dan perbaikan ulang dalam cakupan yang diizinkan. JANGAN kembali melakukan wawancara atas keputusan bisnis yang sudah disetujui, dan JANGAN mengakhiri pekerjaan hanya dengan audit, desain, laporan atau roadmap.**
