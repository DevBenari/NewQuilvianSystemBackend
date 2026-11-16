# MASTER INSTRUKSI AGENT — REPAIR IMPLEMENTASI RIIL HRD MENU 12: PEMBERHENTIAN & PENSIUN

**Jenis dokumen:** Master prompt implementasi langsung, revisi atas `INSTRUKSI_AGENT_HRD_MENU_12_PEMBERHENTIAN_DAN_PENSIUN.md`; bukan laporan pekerjaan ataupun klaim hasil UAT.  
**Tanggal dasar audit:** 16 November 2026.  
**Target penyimpanan pengganti:** `NewQuilvianSystemBackend/docs/module-blueprints/human-resource/docs/update/pemberhentian-dan-pensiun/INSTRUKSI_AGENT_HRD_MENU_12_PEMBERHENTIAN_DAN_PENSIUN.md`.  
**Target:** Menu Kepegawaian **12 — Pemberhentian & Pensiun**, tepat enam Submenu.  
**Mandat:** **KERJAKAN SOURCE BACKEND + DATABASE PERSISTENCE TERLEBIH DAHULU, baru FRONTEND**, kemudian pengujian API dan UAT lintas role. Jangan berhenti dengan audit atau dokumentasi.  
**Mode kerja bertahap:** `MODULE BLUEPRINT MODE` **hanya jika sinkronisasi task/kontrak benar-benar diperlukan dan diizinkan** → `BACKEND MODE` → `FRONTEND MODE` → validasi terkoordinasi `CROSS-REPO MODE` yang terbatas. Target tulis dinyatakan per tahap di bawah.  
**Status awal:** `PARTIAL / REPAIR REQUIRED`, bukan `READY FOR PRODUCTION`; implementasi dan keberhasilan database/runtime belum dibuktikan oleh audit source ini.

> **PENTING:** Ini adalah revisi instruksi kerja untuk Agent IDE yang memiliki workspace dan registered runtime Skill Quilvian. Dokumen ini **tidak mengklaim telah memperbaiki source**, mengubah repository, atau menguji database. Agent wajib mengimplementasikan perubahan nyata dalam wewenang mode aktif.

---

## 1. Role

Anda adalah **Senior Backend Engineer ASP.NET Core/EF Core + Fullstack Engineer Next.js Quilvian V2 + QA Integrasi HRD Rumah Sakit**. Anda bertanggung jawab menuntaskan semua celah operasional Menu 12 yang tercantum di sini, terutama data master/relasi pegawai nyata, transaksi persisten, otorisasi actor, transisi status, dan kebenaran hasil API–UI.

**Repositori dan izin fase:**

| Fase | Repository | Branch WAJIB | Target tulis | Read-only |
|---|---|---|---|---|
| Backend-first | `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | Source Backend dalam domain HRD sesuai task approved | Source FE `HamzahV2`, Engineering Skills `main` |
| Frontend setelah BE lulus | `DevBenari/QuilvianSystemFrontendDev` | `HamzahV2` | Source FE `src/app/hr/separation/`, `src/components/view/hr/separation/`, service/hook/style/menu terkait | Source BE `MHamzah`, Engineering Skills `main` |
| Dokumen blueprint jika perlu | `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | Hanya folder `docs/module-blueprints/human-resource/` dalam `MODULE BLUEPRINT MODE` yang sah | Seluruh source aplikasi |
| Pelaporan task | `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | Laporan tracked `task/report/backend/` dan `task/report/frontend/`, status roadmap/traceability **hanya** sesuai rule | Source di luar scope |

**Aturan invokasi runtime wajib:** untuk **masing-masing** task backend yang sah, panggil registered Skill `quilvian-engineering-skills:build-module-backend` **sebelum mengubah source Backend**; untuk **masing-masing** task frontend yang sah, panggil `quilvian-engineering-skills:build-module-frontend` **sebelum mengubah source Frontend**. Skill `hrd-finishing` pada ChatGPT bukan pengganti dua Skill runtime tersebut. Jika Skill runtime tidak tersedia, keluarkan status persis **`BLOCKED — Quilvian runtime skill unavailable`** untuk fase terkait; jangan melakukan implementasi langsung di luar aturan.

Baca pada branch yang ditetapkan: `AGENTS.md`, `CLAUDE.md`, `QuilvianEngineeringSkills/agents/rules/GLOBAL_RULES.md`, aturan Backend/Frontend/Testing/Rule Output relevan, serta QBE kontrak + registry. Jangan memakai aturan lama yang tersisa pada folder repo aplikasi. Patuhi identitas branch/upstream, jangan checkout/pull/fetch/commit/push/merge/rebase, jangan deploy, jangan membuat/menjalankan migration atau mengeksekusi SQL/database langsung tanpa kewenangan eksplisit terpisah. Pertahankan seluruh uncommitted changes pengguna.

**Cara bertindak:** lakukan preflight terbatas dan impact check singkat, lalu **langsung memperbaiki kode** untuk task/kontrak approved yang relevan. Jangan mengulangi sesi tanya-jawab requirement yang sudah dikunci. Area yang masih memerlukan keputusan owner (misalnya kebijakan usia pensiun atau pemegang hak akhir payroll) diblokir **hanya pada tindakan bergantung kepadanya**; kerjakan area independen terlebih dahulu.

---

## 2. Task

### 2.1 Tujuan bisnis dan hasil akhir yang tidak dapat ditawar

Tuntaskan Menu **Pemberhentian & Pensiun** sebagai proses nyata rumah sakit, bukan prototipe. Pertahankan enam label dan route yang sudah dibuat:

| No. | Label target persis | Route Frontend existing | Data authoritative yang wajib diintegrasikan |
|---|---|---|---|
| 1 | **Pengunduran Diri** | `/hr/separation/resignations` | Profil tenaga kerja, `TrxResignationRequest`, workflow keputusan, relasi separation |
| 2 | **Pensiun** | `/hr/separation/retirements` | `MstEmployee`/`MstWorkforceProfile`, `TrxRetirement`, kebijakan pensiun, separation |
| 3 | **Akhir Kontrak** | `/hr/separation/contract-expiry` | Kontrak kerja existing, keputusan kontrak existing, histori kontrak |
| 4 | **Pemberhentian** | `/hr/separation/terminations` | `TrxEmployeeSeparation`/controller owner, alasan legal, engine pesangon, Finance |
| 5 | **Serah Terima** | `/hr/separation/handovers` | `WfpOffboardingChecklist` dan `WfpOffboardingTask` yang benar-benar persisten |
| 6 | **Clearense** | `/hr/separation/exit-clearance` | Aggregate exit clearance owner, sign-off lintas unit, status Finance/akses |

**Jangan ganti ejaan UI `Clearense`** tanpa keputusan baru. Route yang sudah ada harus tetap bisa dipakai; menu lama seperti `/hr/exit-clearance`, `/hrd/relations/separations`, dan `/hr/lifecycle/employment-contracts` tidak boleh rusak. Gunakan reuse/adapter alih-alih dua master data dan dua transaksi paralel.

**Definisi REAL DATA dan selesai:**

1. GET list/filter/summary/detail hanya menampilkan data **server berdasarkan database**; `HTTP 200 + items=[]` menampilkan **empty state 0 data**. API offline, `401/403/404/409/422/500`, timeout, ataupun parsing error menampilkan state galat/retry yang jujur, **tidak pernah** `DEFAULT_FALLBACK_*`, dummy ID, `Math.random`, sample person, angka default sukses, atau rekonstruksi transaksi lokal.
2. POST/PUT/DELETE berhasil **hanya** bila server menyatakan sukses dan identitas transaksi valid; setelah mutasi refetch dari API dan cek respons server. Jika gagal, **tidak ada toast berhasil, perubahan status optimistis permanen, atau data lokal palsu**.
3. Simpan, submit, approve, reject, handoff, keputusan kontrak, perhitungan pesangon, penyerahan tugas, sign-off, dan finalisasi wajib memiliki **validasi dan persistence authoritative di backend**, audit, izin, transisi sah, perlindungan double submit dan idempotensi yang sesuai kontrak.
4. Pilih pegawai dari master pegawai sungguhan via resource selector; tidak ada input UUID manual atau pegawai contoh. Jangan gunakan informasi pasien klinis mentah pada domain HRD.
5. Data harus stabil setelah reload, logout/login ulang, pencarian, pagination, membuka dari akun berwenang kedua, dan mengakses detail melalui URL. DB side effect hanya melalui **API/service resmi yang diizinkan**, bukan frontend atau SQL ad-hoc.
6. Rujuk hak pekerja dan kewajiban Finance melalui kontrak domain yang disetujui. Jangan mengasumsikan bahwa penyelesaian Clearense otomatis mengizinkan/menahan hak normatif pembayaran tertentu.
7. Hasil akhir memuat source yang diubah, bukti endpoint/persistence, hasil build/lint, screenshot browser, dan semua UAT P0/P1 yang benar-benar berjalan. Tanpa runtime, status maksimal `IMPLEMENTED, NOT VERIFIED`, bukan 100%.

### 2.2 Matriks cacat P0/P1 yang **wajib** diperbaiki

**Bukti audit:** FE `HamzahV2` HEAD `20478cd652651e8e9b620aaa83cdbce3dcc1648e`; BE `MHamzah` HEAD `1b8e56f3f7abebde9a613ef592430d3da0dfc4d1`. Ini **snapshot 16 Nov 2026**, wajib dicek ulang sebelum menulis source.

| Severity | File/symbol (snapshot audit) | Cacat nyata | Tindakan agent yang diwajibkan |
|---|---|---|---|
| **P0** | FE `resignation-list-view.jsx` (`DEFAULT_FALLBACK_RESIGNATIONS`, `fetchData`, `handleCreate`, approve/reject/handoff) | GET kosong/error menunjukkan contoh; mutasi error menghasilkan record lokal dan toast sukses | Hapus seluruh fallback operasional; implementkan error/refetch authoritative; periksa seluruh action gagal |
| **P0** | FE `retirement-list-view.jsx` (`DEFAULT_FALLBACK_CASES`, `DEFAULT_FALLBACK_ELIGIBLE`, semua handler) | Pensiun dan calon pegawai simulasi, approval dan handoff bisa sukses di state lokal | Ganti dengan API live, policy & action dari server; tidak ada rekayasa data saat error |
| **P0** | FE `contract-expiry-view.jsx` (`DEFAULT_FALLBACK_CONTRACTS`, fixed-term-check, keputusan) | Daftar/analisis/keputusan PKWT bisa merupakan simulasi | Gunakan hasil kontrak & keputusan DB nyata, error state dan refetch |
| **P0** | FE `termination-list-view.jsx` (`DEFAULT_FALLBACK_TERMINATIONS`, kalkulasi dan approve) | PHK simulasi dan nominal pesangon buatan klien | Hilangkan rumus `9 bulan + 3 bulan` dan contoh nominal, hanya engine API server |
| **P0** | FE `handover-list-view.jsx` (`DEFAULT_FALLBACK_HANDOVERS`, `catch` add/submit/verify/complete) | Error API tetap memunculkan toast berhasil | Tidak ada sukses tanpa commit server; tampilkan konflik item/izin |
| **P0** | FE `exit-clearance-matrix-view.jsx` (`DEFAULT_FALLBACK_*`, `catch` sign-off/complete) | Status kliring dan pembukaan gembok payroll bisa dipalsukan di React | Hapus fallback; finalisasi/lock harus hanya hasil GET/POST server |
| **P0 SECURITY** | BE `EmployeeRelationManagement/Services/ExitClearanceService.cs`, `SignoffChecklistAsync` | `item.ApprovedByUserId = resolvedActorId ?? request.ApproverUserId` mempercayai ID browser jika actor gagal di-resolve | Wajib fail-closed: actor dari principal/server; jika tak ter-resolve -> `401/403` tanpa update DB; cek hak unit dan kasus |
| **P0 BUSINESS** | BE `LifecycleManagement/Services/RetirementService.cs`, `HandoffAsync` | Kode yang diperiksa tidak tampak menolak handoff saat belum `Approved`; membuat `SeparationStatus = Approved` | Validasi status/approval sah dan tanggal; action tidak boleh mem-bypass tahapan; uji proses invalid |
| **P1** | BE `RetirementService.cs` | `AddYears(-55)`, `AddYears(58)` dan FE `minAge:55`, default `retirementAge:58` | Gunakan sumber kebijakan terverifikasi untuk syarat pensiun; 55/58 bukan ketetapan universal |
| **P1** | BE `RetirementService.cs`, nomor `RET-`, `SEP-RET-`, `EXC-RET-` | `new Random().Next(...)` rawan benturan nomor bisnis | Integrasikan number-series existing dengan mekanisme unik/atomik dan transaksi aman |
| **P1** | BE `OffboardingHandoverService.cs`, `SubmitTaskAsync`/`VerifyTaskAsync` | Pada method yang dibaca belum tampak cek actor pemilik/atasan per item sebelum perubahan | Perketat per-object access & transisi status, bukan hanya permission controller |
| **P1** | FE `separation-service.js` dan kontrak controller BE | Ada beda parameter paging per endpoint; banyak adapter `res?.items || []`/`res?.data?.items || []` | Normalisasi per `ApiResponse<PagedResult<T>>` aktual; periksa `pageNumber`, `pageSize`, status/error metadata, `totalItems`, `items` dengan kode BE |
| **P1** | BE dua keluarga clearance | `LifecycleManagement/ExitClearance*` dan `EmployeeRelationManagement/MultiDeptExitClearance*` hidup berdampingan | Tentukan aggregate owner, pemetaan identifier, adapter yang benar, hindari 2 clearance untuk 1 separation |
| **P1 EVIDENCE** | `docs/update/.../LAPORAN_IMPLEMENTASI_DAN_UAT_MENU_12_*.md` | Laporan terdahulu mengklaim 100% berdasarkan 7 skenario layar; master lama mewajibkan 30 UAT | Tandai laporan sebagai bukti historis, lakukan retest 30 skenario, jangan salin PASS lama menjadi verifikasi runtime |

**Larangan perbaikan palsu:** hanya mengganti nama konstanta `DEFAULT_FALLBACK_*`, menyembunyikan data demo di UI tetapi tetap membuat local success, mengubah `catch` menjadi `console.log` saja, hardcode `totalCount`, membuat endpoint yang me-return `[]` permanen, atau memanipulasi DB uji menjadi seolah transaksi nyata bukan acceptance yang sah.

### Submenu 1 - Pengunduran Diri

**Tujuan RS:** pegawai mengajukan berhenti secara tertib, atasan/HRD memproses keputusan, pekerjaan dan jadwal pelayanan dialihkan, tanggal efektif terkendali; data dan audit pengajuan tetap tercatat. **Aktor:** pegawai ESS, atasan yang memang berwenang, HR Admin/Manager, pejabat keputusan. Hak role dibuktikan oleh backend, tidak ditetapkan dari label jabatan UI.

**Input:** identitas pegawai dari master/profile, tanggal pengajuan dari server, usulan hari kerja terakhir, alasan, rencana serah terima, catatan, lampiran hanya bila fasilitas storage existing/approved mendukung. **Output:** `RequestNumber`, `Id`, status workflow, history actor/time/reason, relasi satu `EmployeeSeparationId` ketika handoff sah. **Normal:** draf → submit → review/keputusan yang dikunci kontrak → approved → handoff idempoten ke separation+offboarding; **alternatif:** reject, batal/tarik sesuai status yang sah, pengajuan ganda, tanggal/notice tidak memenuhi ketentuan yang disetujui, akses tanpa izin.

**Backend — REPAIR/EXTEND:** reuse `ResignationController.cs`, `ResignationRequestService.cs`, `ResignationWorkflowIntegrationService.cs`, `ResignationLifecycleHandoffService.cs`, `TrxResignationRequest`; pastikan endpoint GET list/detail/summary dan POST create/approve/reject/handoff sesuai DTO sebenarnya. Periksa bahwa create admin (`CreateAdminDraftAsync`) tidak otomatis menyamar menjadi status sudah diajukan jika kontrak mensyaratkan draf; buat transisi submit eksplisit hanya melalui endpoint existing/approved. Validasi profil aktif, ownership ESS, kebijakan notice yang terkonfigurasi, otoritas approver, anti-self-approval bila disyaratkan, audit actor, konflik 409, dan handoff di transaction boundary untuk satu peristiwa keluar. Pertahankan riwayat workforce tanpa mengubah status pegawai saat baru mengajukan. Jangan menambah tabel jika model existing cukup.

**Frontend — REPAIR:** file `resignation-list-view.jsx` dan `separation-service.js`. Buang tiga record `res-demo-*`, `wp-demo-*` dan seluruh konstruksi `res-${Date.now()}`, nomor random serta success toast dalam `catch`. Halaman list dari API, tombol **Ajukan / Catat Pengunduran Diri** hanya bila authorized, `ResourceFilterSelect` pegawai, detail timeline, modal approve/reject beralasan, handoff status. Setelah aksi sukses, refetch detail, summary, list, dan tampilkan nomor sah dari server. Bila request gagal, modal tetap terbuka dengan error yang dapat dipahami, input tidak hilang, tidak ada perubahan palsu.

**Selesai bila:** dari akun uji yang diotorisasi create sah mengembalikan ID+nomor server; setelah reload record tetap ada; approve tanpa hak ditolak server; create/ganda/handoff dua kali tidak membentuk kasus offboarding baru; API nol data menjadi tampilan kosong, bukan record Ahmad/Dewi/Budi contoh. UAT: `UAT-M12-01, 02, 03, 04, 05, 06, 24, 25`.

### Submenu 2 - Pensiun

**Tujuan RS:** mengidentifikasi pegawai mendekati pensiun dengan kebijakan resmi, menjaga keberlanjutan pelayanan dan rencana pengganti, mengesahkan proses serta handoff akhir masa kerja. **Aktor:** HRD, Kepala Unit, pengambil keputusan berwenang, Finance sebagai konsumen administrasi resmi.

**Input:** workforce profile dan tanggal lahir/masa kerja dari master, kategori pensiun yang didukung, tanggal pensiun direncanakan/efektif, alasan, dokumen pendukung bila existing mendukung, benefit plan bila legal, catatan keputusan. **Output:** `RetirementNumber`, status case, histori, tanggal sah, link separation/offboarding.

**Backend — REPAIR/EXTEND:** reuse `RetirementController.cs`, `RetirementService.cs`, `TrxRetirement`, `MstEmployee`, `MstWorkforceProfile`, settings lifecycle atau policy owner yang relevan. Ganti usia hardcoded `55`/`58` dengan pengaturan resmi yang dipilih berdasarkan jenis pegawai/status kerja bila canonical memang membedakan; bila kebijakan RS belum approved, tampilkan `POLICY_NOT_CONFIGURED`/validasi dan **blokir penetapan pensiun otomatis**, tetapi tetap bisa membaca kasus existing. Validate tanggal lahir, hubungan kerja, status case, syarat approval sebelum `HandoffAsync`, anti double-approval/handoff, histori audit. Jangan pakai `new Random().Next(...)` untuk `RET`, `SEP`, `EXC`; gunakan number series official dengan keunikan DB. Jaga transaction boundary untuk pembuatan separation dan clearance canonical. Pastikan search `eligible-employees` benar-benar membaca master pegawai yang aktif, bukan hanya record `TrxRetirement` dan bukan data 55/58 contoh.

**Frontend — REPAIR:** `retirement-list-view.jsx`, dua tab **Kasus Pensiun** dan **Calon Pensiun** dari API, bukan `DEFAULT_FALLBACK_CASES`/`DEFAULT_FALLBACK_ELIGIBLE`. Syarat usia dan tanggal proyeksi dari response/settings; jangan default `retirementAge=58` tanpa policy. Form rencana pension dengan pilih pegawai, tanggal, kategori, catatan, aksi **Ajukan/Setujui/Handoff** sesuai `AvailableActions`. Status pending/approved dan empty/error/403 jelas.

**Selesai bila:** daftar calon pensiun berasal dari data tanggal lahir pegawai test yang tersimpan; policy tidak tersedia tidak memicu auto-approve; pensiun tidak sah tidak bisa handoff; nomor unik; status, relasi separation dan audit tetap ada setelah reload. UAT `UAT-M12-07, 08, 09, 10, 24, 25`.

### Submenu 3 - Akhir Kontrak

**Tujuan RS:** mendeteksi PKWT/kontrak yang segera berakhir, memperoleh rekomendasi unit dan keputusan berwenang tepat waktu, lalu memilih perpanjang, perubahan status, atau nonrenewal tanpa salah membuat PHK. **Aktor:** HRD, Kepala Unit, pemutus yang ditetapkan workflow, pegawai penerima notifikasi.

**Input:** kontrak sumber, `ContractId`, `WorkforceProfileId`, tanggal mulai/akhir, status tanda tangan, riwayat PKWT, keputusan dan alasan, tanggal rencana, bukti pemberitahuan bila ada. **Output:** keputusan kontrak server, histori, status, tautan kontrak baru atau separation ketika nonrenewal efektif.

**Backend — REUSE/REPAIR:** gunakan `HrdEmploymentContractController.cs` dan `HrdEmploymentContractService.cs` + `HrdContractDecision`, handler `fixed-term-check`, `decisions`, `decisions/{decisionId}/notify`. Ikuti kontrak PKWT existing dan keputusan `T16`, kaitan `BE-HRD-078/080`; jangan membuat tabel kontrak kedua. Pastikan list page server dengan filter `expiringInDays`/periode yang didukung, proyeksi status tanda tangan, keputusan terakhir sesuai response; bila filter belum tersedia, tambahkan lewat API owner setelah kontrak approved, bukan hanya filter lokal atas 10 data. Hitung masa PKWT dengan aturan yang tercatat pada canonical, tidak mengarang kewajiban otomatis beralih ke PKWTT. Nonrenewal yang sah memicu handoff separation/offboarding satu kali sesuai tanggal efektif; perpanjangan tidak memicu terminasi. Log dan validasi status/tanggal/konflik secara server-side.

**Frontend — REPAIR:** `contract-expiry-view.jsx`: hapus `DEFAULT_FALLBACK_CONTRACTS`, simulasi `totalUsedMonths=24` dan success update di `catch`. Tabel kontrak nyata, filter 30/60/90 hari bila API mendukung, riwayat status/tanggal, link ke `/hr/lifecycle/employment-contracts`, drawer hasil `fixed-term-check` asli, modal keputusan perpanjang/tetap/tidak perpanjang sesuai enum BE. Gunakan tombol notif yang mencatat bukti server, bukan sekadar tooltip.

**Selesai bila:** kontrak test riil dengan tanggal yang berbeda terfilter akurat; keputusan dan notifikasi persist setelah refresh; tidak ada duplicate decision/handoff; error 409/422 tertampil; tidak muncul data Rina/Dimas/Fajar contoh. UAT `UAT-M12-11, 12, 13, 14, 24, 25`.

### Submenu 4 - Pemberhentian

**Tujuan RS:** menangani inisiasi dan persetujuan pemberhentian yang sah; memastikan dasar/riwayat/keberatan dan perhitungan hak keuangan benar; tidak menonaktifkan tenaga klinis sebelum keputusan dan tanggal efektif. **Aktor:** HRD, Kepala Unit, hubungan industrial/Legal sesuai organisasi, pejabat persetujuan, Finance dengan izin khusus nominal.

**Input:** pegawai dari master, separation type/reason resmi, dasar keputusan/kronologi, notice/effective date, bukti, catatan dan status legal, dasar hitung upah dari sumber payroll berwenang/kontrak yang disetujui. **Output:** satu separation resmi, rincian pesangon dari server dan status persetujuan Finance, hubungan ke clearance dan perubahan status kerja pada saat sah.

**Backend — REPAIR:** reuse `EmployeeSeparationController.cs`, owner service dan `SeveranceCalculationEngine`, task existing `BE-HRD-282/283/285`, identity dan izin `EmployeeSeparation`. Cegah `InitiateSeparation` menggandakan kasus aktif dari resign/pensiun/kontrak yang sama. Validasi kewenangan legal & Finance, anti-self-approval, status transisi, mutasi ter-audit, angka pesangon dan tanggal dari server; rules PP 35/2021 tidak boleh disederhanakan menjadi rumus tetap 9x + 3x + hari cuti. Engine server menolak nominal/parameter yang tidak diotorisasi atau tidak punya sumber sah. Pertahankan status sengketa/penundaan bila ada pada canonical; jangan menutup kasus tanpa keputusan legal. Hasil `approve-severance` adalah otorisasi finansial, **bukan** bukti transfer payroll berhasil.

**Frontend — REPAIR:** `termination-list-view.jsx`: hapus `DEFAULT_FALLBACK_TERMINATIONS`, nama `Pegawai Baru Diinisiasi`, kode contoh, perhitungan pesangon hardcode, breakdown default, dan semua `catch` yang mengubah status menjadi Approved. Form **Inisiasi Pemberhentian** memakai `ResourceFilterSelect`, status legal/keputusan, detail & timeline, kalkulasi real dari API, rincian ditampilkan hanya untuk role authorized. Tombol Finance disabled jika backend menolak. Nilai input sumber gaji manual hanya jika kontrak bisnis approved mengizinkan; jika tidak, API harus mengambil dari sumber payroll/komponen sah.

**Selesai bila:** separations 0 menunjukkan kosong, bukan Surya/Bambang/Eko contoh; create menghasilkan nomor server, kalkulasi dan rincian dari database/engine, finance denial tetap denial setelah refresh, server menyimpan audit lengkap. UAT `UAT-M12-15, 16, 17, 18, 24, 25`.

### Submenu 5 - Serah Terima

**Tujuan RS:** memastikan pekerjaan, inventaris, tanggung jawab administratif dan akses kerja dialihkan sebelum pegawai berhenti sehingga pelayanan pasien tidak terganggu. Data klinis pasien tidak disalin ke berkas HRD; cukup referensi aman/konfirmasi proses yang diotorisasi. **Aktor:** pegawai yang keluar, penerima tugas, Kepala Unit/verifikator sah, HRD pengawas.

**Input:** satu separation/offboarding checklist, item dari template/penugasan nyata, kategori tugas, penerima, tenggat, catatan dan bukti file/rujukan approved, hasil verifikasi. **Output:** `WfpOffboardingChecklist` + item `WfpOffboardingTask`, status tiap item, aktor submit/verify, progres server, tanggal penyelesaian.

**Backend — REPAIR:** reuse `OffboardingHandoverController.cs` dan `OffboardingHandoverService.cs`. GET list/detail/summary persisten; tambah tugas idempoten sesuai kebutuhan, `SubmitTaskAsync` hanya oleh pegawai/penerima berwenang menurut desain, `VerifyTaskAsync` hanya verifikator unit/item, tolak verifikasi oleh actor yang tidak tepat. Enforce transisi `Pending -> Submitted -> Completed/Returned` (atau enum kontrak final), tidak boleh `Completed` jika belum disubmit. Pemeriksaan `IsRequired` dan lampiran wajib dijalankan sebelum finalisasi. `TaskCode = TSK-{Count + 1}` harus dicegah benturan oleh penomoran aman atau uniqueness sesuai DB, tidak boleh mengandalkan hitungan tanpa penguncian. Pastikan mengubah child task menghitung ulang total/progres dengan akurat; `CompleteChecklistAsync` tidak menganggap 0 tugas sebagai 100% tanpa kebijakan yang disetujui. Gunakan storage dokumen existing; jangan percaya string path file bebas dari browser.

**Frontend — REPAIR:** `handover-list-view.jsx`: hapus `DEFAULT_FALLBACK_HANDOVERS`; `loadChecklistDetail` tidak memakai `allItems[0]` bila API gagal. Semua `catch` add/submit/verify/complete menampilkan error, tidak toast sukses. Halaman memperlihatkan pemilik/penerima, checklist riil, progres server, modal tambah item/serahkan/verifikasi, detail status/tanggal aktor, bukti dan alasan pengembalian; aksi mengikuti role/status backend, refetch setelah sukses.

**Selesai bila:** dua pengguna berbeda melihat progres sama setelah refresh; submit/verify tidak bisa dengan role salah; item required tertunda menahan finalisasi; bukti tidak dapat ditautkan ke checklist orang lain; tidak ada `hnd-demo-*`. UAT `UAT-M12-19, 20, 21, 24, 25`.

### Submenu 6 - Clearense

**Tujuan RS:** menyelesaikan kewajiban pegawai keluar lintas unit, termasuk serah terima, aset, akses IT dan status Finance, dengan sign-off yang benar; tidak menyembunyikan tunggakan atau memberi status selesai palsu. **Aktor:** Kepala Unit, GA/Aset, unit terkait sesuai policy RS, IT, Finance, HRD; masing-masing hanya berwenang pada item/unit yang dipetakan oleh server.

**Input:** satu separation identifier canonical, satu clearance case, daftar kewajiban unit yang dikonfigurasi, `checklistId`, hasil verifikasi unit, catatan, bukti, informasi kewajiban Finance authoritative. **Output:** status per unit + actor server + timestamp + total kewajiban server, status final clearance yang sah, sinyal terbatas ke Identity/Payroll sesuai kontrak.

**Backend — P0 SECURITY/REPAIR:** periksa `LifecycleManagement/Controllers/ExitClearanceController.cs` & `LifecycleManagement/Services/ExitClearanceService.cs` serta `EmployeeRelationManagement/Controllers/MultiDeptExitClearanceController.cs` & `EmployeeRelationManagement/Services/ExitClearanceService.cs`. Buat keputusan owner/adapter berbasis `EmployeeSeparationId`/`SeparationId` dan kontrak yang disetujui agar satu kasus tidak membentuk dua clearance independen. Di `SignoffChecklistAsync`, **hapus `resolvedActorId ?? request.ApproverUserId`**: jika actor tidak tervalidasi dari principal server -> fail closed; validasi juga hak pada unit/kasus, status item, bukti, jumlah tanggungan (angka Finance berasal dari source berwenang, bukan browser). `FinalizeExitClearanceAsync` memeriksa seluruh required checklist, kewajiban, workflow dan audit; perubahan `IsFinalPayrollUnlocked` tidak boleh dianggap otomatis pembayaran hak normatif. Pengakhiran akun SIMRS/identity hanya melalui integrasi/pemilik domain yang disetujui, dengan jejak audit serta tanggal efektif yang benar. Jika Finance adapter atau IAM belum tersedia, tampilkan `INTEGRATION_PENDING`/`BLOCKED` yang benar—jangan mengaku clear.

**Frontend — P0 REPAIR:** `exit-clearance-matrix-view.jsx`: hapus `DEFAULT_FALLBACK_SEPARATIONS`, `DEFAULT_FALLBACK_CLEARANCES`, `DEFAULT_FALLBACK_PROCESS_DETAIL`, semua `proc-demo-1`, `emp-demo-1`, `chk-1`, dan semua `setProcessDetail`/`setLockStatus` yang memalsukan sukses. Selector separation dari API, tab **Matriks Clearense** dan **Daftar Kasus** live, status unit dari server, visual finance only authorized, modal sign-off dengan alasan/bukti dan `currentUser` hanya sebagai konteks tampilan (identitas keputusan tetap dari server), modal finalisasi hanya saat response backend mengizinkan. Jangan meminta `approverUserId` manual.

**Selesai bila:** API 200 kosong -> matriks kosong dengan petunjuk pilih separation; error 404 -> pesan belum ada case; actor tak valid -> 401/403 tanpa perubahan status; sign-off satu unit tidak bisa diambilalih unit lain; finalisasi tidak membuka lock palsu; data dan aktor konsisten setelah refresh. UAT `UAT-M12-22, 23, 24, 25, 26, 27, 28`.

### 2.3 Invariant integrasi enam Submenu

- **Satu sumber identitas:** tentukan mapping `MstEmployee.Id` ↔ `MstWorkforceProfile.Id` ↔ `TrxEmployeeSeparation.Id` ↔ `TrxRetirement.Id` ↔ `WfpOffboardingChecklist.Id` ↔ clearance process ID. Tidak boleh mengirim `workforceProfileId` sebagai `employeeId` tanpa adapter atau sebaliknya. Catat relasi FK dan owner data dalam dokumen task.
- **Satu peristiwa keluar:** satu pengunduran diri/pensiun/nonrenewal/pemberhentian yang disahkan menghasilkan **maksimal satu** separation untuk event tersebut dan satu offboarding/clearance canonical; retry dan dua tab bersamaan harus mempertahankan invariant.
- **Status authoritative:** hanya service BE memutuskan `AvailableActions`, transisi, aktor yang boleh melakukan, tanggal efektif, perubahan status pegawai, dan dampak Payroll/Identity. UI tidak memutuskan dengan string hardcode.
- **Sumber status Finance:** komponen dan rincian hak berasal dari domain sah. Clearense dan payroll release merupakan dua konsekuensi yang harus didefinisikan oleh owner, tidak boleh menjadikan semua kewajiban administrasi alasan otomatis menahan upah normatif.
- **Resource search:** semua pilihan pegawai/atasan/kontrak/status mendapatkan opsi live terfilter sesuai izin; tidak ketik UUID, tidak menaruh data pasien sensitif ke localStorage, sessionStorage, console, log, screenshot, atau laporan.
- **Database:** gunakan EF Core persistence yang sudah ada, `SaveChangesAsync`, transaksi ketika perlu, unique constraint/number-series sesuai QBE, audit + optimistic concurrency/idempotency. **Jangan menambah tabel paralel** untuk fitur yang sudah punya entity. Jika DDL/migration diperlukan, tulis impact dan `BLOCKED — separate schema/migration authorization required` pada area tersebut, bukan diam-diam membuat/menerapkannya.

### 2.4 Penomoran task mengikuti `build-module-backend` dan `build-module-frontend`

**Fakta registry saat audit:** `roadmap/backend-roadmap.md` memiliki ID sampai **`BE-HRD-333`** dan `roadmap/frontend-roadmap.md` sampai **`FE-HRD-305`**. Format task resmi adalah `BE-HRD-NNN` dan `FE-HRD-NNN`, satu task tracked = satu pemanggilan Build Module Skill = satu file laporan `<TASK-ID>.md`. Task existing yang perlu dikaitkan adalah `BE-HRD-014`, `BE-HRD-078`, `BE-HRD-080`, `BE-HRD-282`, `BE-HRD-283`, `BE-HRD-284`, `BE-HRD-285`, `FE-HRD-017`, `FE-HRD-064`, `FE-HRD-257`, `FE-HRD-258`, `FE-HRD-259`, `FE-HRD-260`. Status lama tidak membuktikan lulus implementasi baru.

**Peraturan anti-ID palsu:** tabel berikut berisi **nomor yang diusulkan untuk revisi Menu 12, bukan klaim ID sudah tercatat dan approved di roadmap**. Angka diambil setelah nomor terakhir teramati pada snapshot, bukan hasil pengesahan. Saat Agent memulai, periksa HEAD/roadmap dan laporan terkini:

1. Bila ID usulan sudah terpakai atau owner roadmap memilih mendaur ulang task yang memang punya acceptance sama, **jangan menimpa ID**; tentukan nomor kosong sah menurut prosedur `plan-module-delivery`/`manage-module-blueprint` serta keputusan pemilik. Pertahankan referensi existing.
2. Bila kontrak approved dan ID belum terdaftar, sinkronkan kartu task, acceptance, dependency, diagram dependency, dan traceability **hanya dalam `MODULE BLUEPRINT MODE` dengan kewenangan yang sesuai**. Instruksi pengguna ini mengunci **tujuan perbaikan bisnis**, namun tidak boleh disalahartikan sebagai izin Git/migration/operasi DB atau memalsukan approval canonical. Jangan mengedit roadmap struktur jika mode hanya mengizinkan update status.
3. **Task yang sudah approved pada roadmap dapat langsung dikerjakan ulang menggunakan ID existing** bila acceptance-nya memang sama; perbarui laporan tracked lama, bukan laporan baru. Untuk task usulan yang belum disahkan, tandai `PENDING_CANONICAL_TASK_REGISTRATION`, lanjutkan task existing/independen yang sah. Dilarang memanggil Build Module seolah nomor usulan sudah approved.
4. Setiap task backend dan frontend dijalankan **satu per satu**, bukan satu invocation untuk semua 19 task. Catat klasifikasi HEAVY/MEDIUM sesuai `TASK_CLASSIFICATION.md` dan pecah EPIC bila diperlukan.

#### Gelombang BACKEND FIRST — daftar task usulan dan instruksi implementasi

| Urutan | ID usulan* | Prioritas | Pemilik area/scope nyata | Dependensi | Kaitan task canonical lama |
|---|---|---|---|---|---|
| B01 | `BE-HRD-334` | P0 | Audit-kontrak ringkas + adapter GET/paging data nyata enam Submenu | Governance/approved contract | `BE-HRD-014/282/284/285` |
| B02 | `BE-HRD-335` | P0 | Actor identity, per-record access, permission dan audit yang tidak dapat dipalsukan | B01 | `BE-HRD-282/284/285` |
| B03 | `BE-HRD-336` | P0 | One-separation owner, idempotensi, number series, transaction safety | B01, B02 | `BE-HRD-014/078/282/284` |
| B04 | `BE-HRD-337` | P0 | Pengunduran diri: status, approval, handoff, persistence | B01–B03 | `BE-HRD-282` |
| B05 | `BE-HRD-338` | P0 | Pensiun: policy, approval gate, handoff aman dan nomor sah | B01–B03 | `BE-HRD-282` |
| B06 | `BE-HRD-339` | P1 | Akhir kontrak: list/filter riil, keputusan T16, nonrenewal handoff | B01–B03 | `BE-HRD-078/080/282` |
| B07 | `BE-HRD-340` | P0 | Pemberhentian dan perhitungan/otorisasi pesangon authoritative | B01–B03 | `BE-HRD-282/283` |
| B08 | `BE-HRD-341` | P0 | Serah Terima: task owner, verify, progress dan finalisasi persisten | B02, B03 | `BE-HRD-014` |
| B09 | `BE-HRD-342` | P0 | Clearense: canonical owner, signoff per unit, kewajiban Finance, lock integration | B02, B03, B08 | `BE-HRD-014/284/285` |
| B10 | `BE-HRD-343` | P0 | Pemeriksaan API/DB via service, concurrency, keselarasan 6 alur, report & retest | B04–B09 | `BE-HRD-285` |

\* **USULAN/RESERVED IN THIS INSTRUCTION ONLY:** jangan mengklaim semua ID di atas sudah ada pada roadmap. Periksa dan sahkan sebelum Build Module sesuai rule.

**Rincian BE yang wajib masuk ke tiap kartu task ketika registrasi disahkan:**

#### BE-HRD-334 — Kontrak API, query dan database nyata

- **Scope source:** `ResignationController/Service`, `RetirementController/Service`, `HrdEmploymentContractController/Service`, `EmployeeSeparationController/Service`, `OffboardingHandoverController/Service`, kedua controller/service Clearense serta DTO `ApiResponse<PagedResult<T>>` terkait.
- **Implement:** cek `DbSet`, FK, predicate `IsDelete/IsActive`, master workforce, paging/filter/sort, query summary dan list, fields yang diperlukan tabel UI, data lifecycle yang sama di semua view; perbaiki parameter query sesuai pola controller, `pageNumber` vs `page`, `totalItems` vs `totalCount`, `Items` vs `Data`, dan response handling. Tambah read-only projection/metadata endpoint hanya bila diperlukan dan approved. Tidak membuat `seed` contoh untuk menyenangkan UI.
- **Bukti selesai:** enam GET list + summary/detail mengembalikan data terstruktur dari EF query, semua `200 + []` benar-benar kosong, unauthorized `401/403`, unknown id `404`; lakukan validasi build BE. Laporan memuat method/tag/DTO/pagination sebenarnya, bukan bentuk tebak-tebakan.

#### BE-HRD-335 — Identitas aktor, hak objek, audit

- **Scope source:** `ExitClearanceService.SignoffChecklistAsync`, `MultiDeptExitClearanceController`, `OffboardingHandoverService`, `ResignationRequestService`, `RetirementService`, `EmployeeSeparation` beserta mapping role.
- **Implement:** hilangkan fallback identitas actor dari payload; derive principal server yang terverifikasi; tolak actor missing tanpa side-effect; pastikan pemegang role juga anggota unit/person/owner yang boleh melakukan tindakan pada **record tertentu**. Status `AvailableActions` dari server jika tersedia, tanpa mengarang claim baru. Cek self-approval, privilege escalation, multi-department, audit actor dan timestamp; tangani `401`, `403`, `404`, `409`, `422` sesuai kontrak. Jangan memperluas izin hanya agar tombol bekerja.
- **Bukti selesai:** user unit A ditolak menandatangani item unit B, user tanpa hak tidak bisa memfinalisasi, tidak ada fallback `ApproverUserId` browser, audit sesudah mutasi sesuai session actor.

#### BE-HRD-336 — Aggregate exit event, idempotensi, number series

- **Scope source:** `ResignationLifecycleHandoffService`, `RetirementService.HandoffAsync`, `HrdEmploymentContractService`, `EmployeeSeparationService`, offboarding/clearance service, `HrdNumberSeriesService` (jika owner mendukung) dan entity FK relevan.
- **Implement:** tentukan separation canonical dan linking key idempoten; satu event yang sama tak boleh diproses oleh dua jalur hingga dua separation/clearance. Gunakan transaction boundary sesuai QBE, token/idempotency key atau unique invariant yang sesuai kontrak, anti double-submit/race, nomor unik disediakan server, bukan `new Random()`, `Count+1`, atau browser. Jika constraint DB baru diperlukan, jangan membuat/menjalankan migration tanpa otorisasi; implement source independen dan laporkan gap schema.
- **Bukti selesai:** retry, dua tab, dua actor, response timeout lalu retry tidak menggandakan case; nomor tidak bentrok; FK/identitas jelas; alur yang membutuhkan migration diberi status BLOCKED, bukan dicatat PASS.

#### BE-HRD-337 — Pengunduran diri end-to-end

- **Implement:** validasi profile/notice/status; workflow draf/submit/approve/reject/cancel sesuai keputusan; auth admin vs ESS, timeline audit, hanya approval sah dapat handoff, create offboarding yang sama tidak mengganda, status pegawai tetap benar sampai tanggal efektif. Gunakan controller, DTO, service yang ada.
- **Bukti selesai:** API create/review/reject/handoff mengembalikan persisted record; error role/date/status ditolak, refetch konsisten; bukti data existing dan newly created di lingkungan dev yang sah.

#### BE-HRD-338 — Pensiun end-to-end

- **Implement:** baca kebijakan retirement owner, hilangkan usia/hari default hardcoded pada keputusan bisnis, aktifkan validasi jenis pensiun/kategori/status dan approval gate, terbitkan `RET-` dari number series aman, linked separation/clearance idempotent, employee eligibility berdasarkan DOB master. Bila kebijakan belum sah, blokir keputusan usia otomatis tetapi jangan menghapus kemampuan baca.
- **Bukti selesai:** employee under-age tidak auto-final; detail & summary konsisten setelah refresh; tidak ada handoff dari `Planned`; nomor & FK benar; tak ada `new Random().Next` untuk nomor bisnis.

#### BE-HRD-339 — Akhir kontrak nyata

- **Implement:** extend owner `HrdEmploymentContractService` untuk data kontrak yang akan berakhir dan `HrdContractDecision` yang sudah ada; pastikan pilihan jenis keputusan cocok enum/contract; verifikasi batas PKWT dari sumber resmi existing, workflow T16, catat notifikasi. Integrasikan `NotRenew` ke separation hanya setelah sah dan tanggal efektif. `Extend`/`Promote` tidak boleh men-trigger clearance.
- **Bukti selesai:** data/filter tanggal dari DB, hasil fixed-term-check server, perubahan keputusan persist, tidak ada kontrak ganda atau nonrenewal palsu.

#### BE-HRD-340 — Pemberhentian + kompensasi

- **Implement:** periksa `EmployeeSeparationService` & `SeveranceCalculationEngine` untuk rule, wage source, legal status, state transition, approve finance, audit, pemetaan role; pastikan finansial tidak diambil dari contoh UI; larang finalisasi sengketa atau belum sah dan cegah duplikasi lintas jenis separation. Dokumentasikan apakah API `calculate-severance` menyimpan snapshot DB atau hanya simulasi server dan bedakan jelas hasilnya; persetujuan Finance harus punya persistence yang nyata.
- **Bukti selesai:** hasil breakdown berasal dari engine BE sesuai tipe kasus/policy; angka tidak bisa dipalsukan dengan payload terlarang; approval finance multi-role/negative test terlacak; GET kembali sama setelah reload.

#### BE-HRD-341 — Serah Terima

- **Implement:** mengaitkan checklist real ke separation canonical, permission per penanggung jawab, status task yang sah, keunikan kode task, kategori, target date, bukti yang aman, penghitungan progress/required item; finalisasi hanya setelah seluruh butir wajib dan verifikasi sah. Pertahankan `WfpOffboarding*`; tidak membuat entity paralel.
- **Bukti selesai:** dua akun berwenang mendapatkan hasil sama, unauthorized 403, transisi tidak sah 409/422, selesai 100% hanya jika syarat terpenuhi.

#### BE-HRD-342 — Clearense multi-departemen dan lock server

- **Implement:** putuskan canonical owner berbasis source & approved contract sebelum menghubungkan dua model clearance; buat adapter bila perlu. Sign-off pakai actor server & unit-assignment, finance liability dari sumber resminya, complete dengan checklist required + audit, outcome ke identity/payroll sesuai kontrak. Bila external integration belum tersedia, tandai pending authoritative, jangan buat success placeholder. `isFinalPayrollUnlocked` tidak boleh ditafsirkan sebagai transfer uang sudah dilakukan.
- **Bukti selesai:** 5-unit (atau konfigurasi unit resmi yang dipakai RS) di-read dari server; signoff lintas unit ditolak; outstanding liability menahan tindakan yang memang disyaratkan; case selesai hanya jika semua gating yang sah terpenuhi; tidak ada clearance ganda.

#### BE-HRD-343 — Gerbang validasi Backend sebelum Frontend

- **Implement:** verifikasi query/API sesungguhnya, build aplikasi utama, schema readiness dengan metadata/migration status **tanpa eksekusi database langsung**, request sukses & gagal di environment test yang authorized, audit persistence melalui API GET sesudah reload/re-login, negative permission, concurrency dan idempotensi. Buat **API-to-UI contract sheet** final yang disimpan di report task/blueprint yang diizinkan: method, URL, query, enum, DTO fields, envelope, paging, `AvailableActions`, status 400/401/403/404/409/422, state transition.
- **Bukti selesai:** tujuh kelompok API (resign, retirement, contract, separation, handover, lifecycle-clearance, relation-clearance) diverifikasi source & runtime bila lingkungan tersedia. Jika schema/DB runtime belum siap, catat `BACKEND_RUNTIME_BLOCKED` dan **jangan mengizinkan frontend mengklaim integrated PASS**.

#### Gelombang FRONTEND SETELAH BACKEND — daftar task usulan

| Urutan | ID usulan* | Prioritas | Pemilik area/scope nyata | Dependensi Backend | Kaitan task canonical lama |
|---|---|---|---|---|---|
| F01 | `FE-HRD-306` | P0 | Service/adapter error+paging, anti-fallback seluruh Menu 12 | `BE-HRD-334/343` | `FE-HRD-257/259` |
| F02 | `FE-HRD-307` | P0 | UI Pengunduran Diri data nyata dan aksi sah | `BE-HRD-337` | `FE-HRD-257` |
| F03 | `FE-HRD-308` | P0 | UI Pensiun kandidat dan kasus riil | `BE-HRD-338` | `FE-HRD-257` |
| F04 | `FE-HRD-309` | P1 | UI Akhir Kontrak real data | `BE-HRD-339` | `FE-HRD-064` |
| F05 | `FE-HRD-310` | P0 | UI Pemberhentian & server-only severance | `BE-HRD-340` | `FE-HRD-257/258` |
| F06 | `FE-HRD-311` | P0 | UI Serah Terima dan progres nyata | `BE-HRD-341` | `FE-HRD-017` |
| F07 | `FE-HRD-312` | P0 | UI Clearense dan payroll lock authoritative | `BE-HRD-342` | `FE-HRD-017/259/260` |
| F08 | `FE-HRD-313` | P1 | Navigasi, base component, role, loading/error/empty, responsive | F01–F07 | `FE-HRD-017/247` |
| F09 | `FE-HRD-314` | P0 | UAT browser 30 test, retest, integrasi UI API, report | F01–F08 | `FE-HRD-260` |

\* Sama seperti backend, ID di atas **nomor USULAN pada instruksi, bukan ID canonical yang sudah approved**. Agent hanya memakai nomor yang disahkan/tersedia saat Build Module; nomor yang bentrok wajib direkonsiliasi dulu.

#### FE-HRD-306 — Adapter API riil, anti mock dan state

- **File utama:** `src/lib/services/hr/separation-service.js`; hook state per Submenu bila mengikuti pola repo; view existing enam halaman.
- **Implement:** cocokkan `InstanceAxios` base URL + prefiks `/v1` dengan `[Route("api/v1/...")]` BE, bentuk response `ApiResponse<T>`, list normalizer per endpoint, query `pageNumber`, `pageSize`, keyword/filter, sort dari server, summary total, `AbortController`/stale response guard. Hapus `DEFAULT_FALLBACK_*` pada semua view, sample item dan local fake mutation. Buat helper error mapping *dengan pola existing*, bukan HTTP client baru; selalu tampilkan pesan Backend/HTTP yang aman ke pengguna dan tombol Retry.
- **Bukti selesai:** API 200+empty -> tabel empty, API 500/403 -> error/denied, failed POST -> no success/no row, pending state disable duplicate submit. Dua unit yang sama tidak membuat daftar palsu.

#### FE-HRD-307 — Pengunduran Diri

- **Implement:** susun list+filter real, detail/timeline, grouped editor create, modal approve/reject/handoff sesuai permission; resource pegawai live; render status dari enum BE; refetch setelah berhasil. Tidak ada hardcode contoh.
- **Bukti selesai:** UAT create + decision + reload + role negatif, CSS checker 0 temuan.

#### FE-HRD-308 — Pensiun

- **Implement:** dua tab kasus/eligible berdata API, policy banner, filter usia/tanggal yang sah, create+approve+handoff, disable bila age policy tidak terkonfigurasi, bukti relasi separation setelah selesai. Jangan hardcode 58.
- **Bukti selesai:** eligible riil dari master, empty/error jujur, sesuai API.

#### FE-HRD-309 — Akhir Kontrak

- **Implement:** list PKWT riil, filter horizon, decision + validation result server, riwayat, link kontrak, notify, loading/error/409, decision refetch.
- **Bukti selesai:** filter dan keputusan persist, tidak ada simulasi fixed-term check.

#### FE-HRD-310 — Pemberhentian

- **Implement:** list/summary, form inisiasi, legal status/detail, severance server-only, modal rincian dari engine, finance approval hanya berizin, audit timeline. Tidak ada contoh gaji/pesangon sebagai nilai final.
- **Bukti selesai:** calculation API failed -> no fictional value; unauthorized -> no approval.

#### FE-HRD-311 — Serah Terima

- **Implement:** list, detail checklist per item, step submit/verify/return, progress dari server, modal reason/bukti, linked separation, status & actor timestamp; invalid action server ditolak tanpa local update.
- **Bukti selesai:** hasil berubah hanya sesudah server sukses, lalu survive reload.

#### FE-HRD-312 — Clearense

- **Implement:** daftar + matriks lintas unit dari API owner, separation picker, finance status dan lock yang jelas, action signoff hanya untuk unit berwenang, finalisasi setelah semua gate server true, audit; tidak ada actor ID browser, `proc-demo`, atau `setLockStatus(false)` dalam catch.
- **Bukti selesai:** role unit B tidak bisa signoff unit A, lock status tidak berubah saat API error, refresh sama.

#### FE-HRD-313 — Konsistensi UI/menu

- **Implement:** pertahankan 6 route dan label; tambah active breadcrumb/permission guard, jaga legacy route; gunakan `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `Pagination`, `StatusBadge`, `ConfirmModal`, `BaseDetailView`, `BaseGroupedEditorView`, keluarga `BaseFormControl`, `ResourceFilterSelect`, `ToastStack`, `AccessDeniedGate`. Jalankan Base Component Decision Gate per elemen, design tokens + Poppins; detail responsive desktop/tablet/mobile, screenshot & CSS checker. Jangan Tailwind, CSS hex, modal mentah atau raw UUID.
- **Bukti selesai:** lint, CSS usage 0, screenshot 6 halaman pada breakpoint wajib, error/empty/403 terlihat, menu lama tidak rusak.

#### FE-HRD-314 — UAT integrasi live dan regresi

- **Implement:** headed Chromium di staging/local aman, UAT `UAT-M12-01` s.d. `UAT-M12-30`; verifikasi network API dan perubahan state yang benar-benar ter-persist, laporan screenshot dan hasil. Retest setiap FAIL dalam scope; jika environment tidak tersedia, catat `UAT NOT RUN` dan tidak mengklaim lulus.
- **Bukti selesai:** hasil eksekusi dengan timestamp, role, URL, HTTP code, screenshot, bukti persisted, status PASS/FAIL/BLOCKED per test, tanpa data sensitif.

### 2.5 Grafik urutan dependency yang wajib dipatuhi

```text
[Preflight governance + kontrak approved + ID task sah]
                   |
                   v
            BE-HRD-334 (real query/API)
                   |
                   v
            BE-HRD-335 (actor/security)
                   |
                   v
            BE-HRD-336 (number/idempotency/aggregate)
                   |
       +-----------+-----------+---------------+
       |           |           |               |
       v           v           v               v
  BE-HRD-337  BE-HRD-338  BE-HRD-339      BE-HRD-340
  Resign      Retirement   Contract        Termination
       |           |           |               |
       +-----------+-----------+---------------+
                   |
                   v
            BE-HRD-341 (handover)
                   |
                   v
            BE-HRD-342 (clearance + payroll/IT)
                   |
                   v
            BE-HRD-343 (API contract/runtime GATE)
                   |
           BACKEND GATE PASS
                   |
                   v
            FE-HRD-306 (service anti-mock)
                   |
       +-----------+-----------+---------------+
       |           |           |               |
       v           v           v               v
  FE-HRD-307  FE-HRD-308  FE-HRD-309      FE-HRD-310
  Resign       Pensiun     Kontrak         PHK
       |           |           |               |
       +-----------+-----------+---------------+
                   |
                   v
            FE-HRD-311 (handover)
                   |
                   v
            FE-HRD-312 (clearense)
                   |
                   v
            FE-HRD-313 (menu, UX, responsive)
                   |
                   v
            FE-HRD-314 (UAT 30 skenario)
```

Grafik ini **grafik instruksi usulan**, bukan pengganti grafik canonical roadmap. Pada registrasi resmi, salin/rekonsiliasi ketergantungan sesuai format `rules/rule-output/grafik-dependency-roadmap.md` tanpa mengubah dependency approved secara sepihak. Bila per-task dependency tidak saling terkait, Agent boleh menuntaskan task independen sesuai aturan, tetapi **Frontend tidak boleh diklaim selesai sebelum Backend yang dipakai valid**.

### 2.6 Skema tampilan Frontend WAJIB (enam halaman riil)

**Prinsip UI:** jangan membuat menu/route paralel. **Perbaiki enam halaman existing** dan gunakan pola layout dari `rules/frontend/page-composition-patterns.md`; jika diperlukan split view besar menjadi hook, table columns, detail/editor khusus domain dengan struktur folder repository, tanpa refactor massal. Urutan untuk setiap halaman list: **Hero → SummaryGrid → DataFilter → InformationAlert → DataTable → ToastStack**, dibungkus `AccessDeniedGate` menurut pola existing. Seluruh statistik berasal dari server; **jangan hitung total keseluruhan dari 10 baris halaman jika server punya `totalItems`**. Actions mengambil ketersediaan dari API/permission, bukan toggle admin palsu.

**Matriks base component dan tujuan:**

| Region/interaksi | Base component wajib dicek di source | Kebijakan |
|---|---|---|
| Header, deskripsi, tindakan utama | `Hero`, `BaseButton` | Primary hanya action sah, `danger` untuk tindakan destruktif, `secondary` untuk batal/kembali |
| Kartu angka statistik | `SummaryGrid` | Server summary / aggregate yang benar; skeleton saat loading |
| Search, filter unit/status/tanggal | `DataFilter`, `FilterSelect`, `FilterDatePicker`, `ResourceFilterSelect` | Query server, reset filter & page ke 1; jangan input UUID |
| Tabel, status, paginasi | `DataTable`, `StatusBadge`, `Pagination` | Loading, empty, total pages, sort server; status badge memakai key semantik sah |
| Info penolakan, sesi/izin, error | `InformationAlert`, `AccessDeniedGate` | UI membedakan 0 data, 401, 403, 404, 409, 422, 500 |
| Form banyak kelompok | `BaseGroupedEditorView`, `BaseEditorField`, keluarga `BaseFormControl` | Validasi field, error server, state submit, role, preservation input |
| Detail riwayat | `BaseDetailView`, `BaseDetailCard`/`BaseDetailSidePanel` | Timeline dengan actor+timestamp dari server |
| Persetujuan, alasan, finalisasi | `ConfirmModal` | Alasan wajib bila kontrak mewajibkan, tidak auto-confirm |
| Notifikasi setelah mutasi | `ToastStack` | `success` **hanya setelah server success**, bukan dalam `catch` |

**Tabel keputusan Base Component** wajib dibuat per task UI sebelum JSX/CSS sesuai gate `REUSE → EXTEND → COMPOSE → WRAP → NEW`. `NEW` dan `EXTEND` yang mengubah perilaku default memerlukan keputusan pemilik; untuk bagian lain gunakan `REUSE` atau `COMPOSE`. Gunakan stylesheet existing `src/style/hr/separation/separation.module.css` dengan CSS variables `--color-*`, `--font-size-*`, `--space-*`, `--radius-*`, `--shadow-*` dari `globals.css`, jangan literal warna, layout Tailwind yang tidak terpasang atau typography override pada shared base. Font Poppins terpasang existing.

#### UI-12.1 — Pengunduran Diri (`resignation-list-view.jsx`)

```text
+--------------------------------------------------------------------------------------+
| HERO: Pengunduran Diri                          [ + Catat Pengunduran Diri ]          |
| Kelola pengajuan dan proses penyerahan pegawai sesuai izin akun.                     |
+--------------------------------------------------------------------------------------+
| SUMMARY: [Total] [Draft] [Menunggu review] [Disetujui] [Handoff selesai]            |
+--------------------------------------------------------------------------------------+
| FILTER: [Cari nama/NIP/nomor] [Unit] [Status] [Tanggal] [Reset]                     |
+--------------------------------------------------------------------------------------+
| ALERT error/izin/loading jika ada (tidak pernah contoh pegawai).                    |
+--------------------------------------------------------------------------------------+
| NO | NOMOR   | PEGAWAI | UNIT | TGL AJUAN | TERAKHIR KERJA | STATUS | AKSI          |
|  1 | RSG-... |  nyata  | real |   ...     | ...            | Badge  | Detail        |
|  2 | ...     |  nyata  | real |   ...     | ...            | Badge  | Review        |
|                 [Halaman X dari Y] [Sebelumnya] [Berikutnya]                       |
+--------------------------------------------------------------------------------------+
| DETAIL: [Ringkasan] [Alasan] [Rencana Handover] [Dokumen] [Timeline keputusan]      |
| AKSI (berizin): [Simpan Draf] [Submit] [Approve] [Reject] [Handoff]                 |
+--------------------------------------------------------------------------------------+
```

- **Form `BaseGroupedEditorView`**: Grup 1 **Pegawai** (`ResourceFilterSelect` terikat workforce profile, unit/jabatan read-only dari master); Grup 2 **Pengajuan** (tanggal server read-only, usulan tanggal hari terakhir, alasan, rencana serah terima); Grup 3 **Lampiran & Catatan** (hanya attachment flow yang benar-benar didukung); Grup 4 **Konfirmasi** (validasi notice & warning ketidaksesuaian sebelum submit). Jangan menampilkan input UUID.
- **Modal keputusan:** Approve = catatan dan ringkasan tanggal; Reject = alasan wajib; Handoff = ringkasan link separation/checklist jika API sah. Aktor/tanggal di timeline selalu dari server.
- **Empty/error state:** “Belum ada pengajuan pengunduran diri untuk filter ini.” jika GET benar-benar kosong; “Gagal memuat data pengajuan” dan `[Coba Lagi]` saat API gagal. Pada `403`, `AccessDeniedGate`, bukan daftar demo.

#### UI-12.2 — Pensiun (`retirement-list-view.jsx`)

```text
+--------------------------------------------------------------------------------------+
| HERO: Pensiun                                     [ + Rencanakan Pensiun ]          |
+--------------------------------------------------------------------------------------+
| SUMMARY: [Kasus Aktif] [Menunggu] [Disetujui] [Handoff] [Mendekati Pensiun]        |
+--------------------------------------------------------------------------------------+
| TAB: [Kasus Pensiun] | [Calon Pensiun / Kelayakan]                                  |
+--------------------------------------------------------------------------------------+
| FILTER: [Nama/NIP] [Unit] [Jenis] [Status] [Tanggal] [Reset]                       |
+--------------------------------------------------------------------------------------+
| KASUS: No | Nomor RET | Pegawai | Profesi/Unit | Jenis | Efektif | Status | Aksi    |
| CALON: No | Pegawai | Unit | Tgl Lahir | Usia | Usia/Aturan Berlaku | Proyeksi    |
+--------------------------------------------------------------------------------------+
| DETAIL: Data pegawai | Aturan kelayakan resmi | Tanggal | Keputusan | Timeline     |
| AKSI: [Buat rencana] [Ubah draf] [Setujui] [Handoff ke Offboarding]                |
+--------------------------------------------------------------------------------------+
```

- **Policy ribbon** di atas tab calon: “Perhitungan menggunakan kebijakan pensiun RS yang aktif sejak tanggal ...” berdasarkan server, atau “Kebijakan pensiun belum terkonfigurasi; keputusan otomatis dinonaktifkan” bila belum ada. Jangan tampilkan angka `55`/`58` seolah kebijakan universal.
- **Form rencana:** Profil pegawai terpilih, umur dan masa kerja dari backend read-only, jenis pensiun dari metadata, tanggal rencana/efektif, dasar dokumen/benefit plan bila eligible, catatan. Tunjukkan validasi server per-field.
- **Detail:** kasus riil, nomor RET server, hasil keputusan, tombol handoff hanya bila `Approved` + available action. Riwayat payroll/benefit di-ringkas tanpa membocorkan data ke role non-Finance.

#### UI-12.3 — Akhir Kontrak (`contract-expiry-view.jsx`)

```text
+--------------------------------------------------------------------------------------+
| HERO: Akhir Kontrak                    [Lihat Semua Kontrak Kerja (route existing)] |
+--------------------------------------------------------------------------------------+
| SUMMARY: [Akan Habis 30 Hari] [60 Hari] [90 Hari] [Lewat Tanggal] [Keputusan]     |
+--------------------------------------------------------------------------------------+
| FILTER: [Cari pegawai/kontrak] [Unit] [Jatuh Tempo] [Status] [Reset]              |
+--------------------------------------------------------------------------------------+
| No | Nomor Kontrak | Pegawai | Mulai | Berakhir | Jenis | Status | Keputusan | Aksi  |
+--------------------------------------------------------------------------------------+
| DETAIL: Identitas | Kontrak sumber & tanda tangan | Riwayat perpanjangan           |
|        [Periksa batas PKWT] [Ajukan Keputusan] [Riwayat keputusan] [Catat Notif]    |
+--------------------------------------------------------------------------------------+
```

- **Panel pemeriksaan batas PKWT:** tanggal mulai seri, total bulan berlaku, batas menurut kebijakan/regulasi yang diterapkan BE, sisa bulan/status eligibility, catatan server. Saat gagal, tampil pesan galat dan dilarang menampilkan angka 24/36 bulan contoh.
- **Modal keputusan:** `decisionType` option **hanya** yang disediakan kontrak backend, tanggal mulai/end baru jika perpanjangan, durasi, alasan, ringkasan dampak ke status hubungan kerja, konfirmasi. Pastikan keputusan tidak otomatis berarti kontrak baru sudah ditandatangani.
- **Detail & notif:** tampilkan sumber kontrak, status tanda tangan dan histori keputusan; nonrenewal sah memiliki link separation; jika masih proses approval, status jelas dan tidak melakukan clearance.

#### UI-12.4 — Pemberhentian (`termination-list-view.jsx`)

```text
+--------------------------------------------------------------------------------------+
| HERO: Pemberhentian                               [ + Ajukan Pemberhentian ]      |
+--------------------------------------------------------------------------------------+
| SUMMARY: [Total Kasus] [Usulan] [Review Legal] [Keputusan] [Final]                 |
+--------------------------------------------------------------------------------------+
| FILTER: [Nama/NIP/nomor] [Alasan] [Status] [Tanggal Efektif] [Reset]              |
+--------------------------------------------------------------------------------------+
| No | Nomor SEP | Pegawai | Unit | Dasar | Usulan | Status Legal | Status | Aksi     |
+--------------------------------------------------------------------------------------+
| DETAIL: Profil pegawai | Kronologi & dokumen | Keputusan/tanggal | Audit timeline   |
| PANEL FINANCE (role tertentu): [Hitung Hak via Server] [Lihat Rincian] [Approve]     |
+--------------------------------------------------------------------------------------+
```

- **Form inisiasi:** pegawai dari resource, alasan dari opsi server, notice date, effective date, ringkasan kronologi, dokumen kalau ada endpoint upload resmi, catatan keputusan. Jangan meminta data payroll sensitif kepada role umum.
- **Modal kalkulasi:** jelaskan bahwa sumber upah/masa kerja dipakai backend; input tambahan jika BE kontrak explicitly mengizinkan dan diverifikasi. Hasil `UP`, `UPMK`, `UPH`, komponen lain, dasar perhitungan, tanggal snapshot, catatan legal — semua **data server**. Jika server gagal, kosongkan hasil/beri error, **jangan menghitung angka baru di FE**.
- **Modal approval Finance:** rincian yang akan diotorisasi, `isApproved` & catatan yang diizinkan, status/aktor persetujuan setelah refetch. Jangan menuliskan “dibayarkan” jika baru disetujui Finance.

#### UI-12.5 — Serah Terima (`handover-list-view.jsx`)

```text
+--------------------------------------------------------------------------------------+
| HERO: Serah Terima                           [Filter Kasus] [Detail Checklist]     |
+--------------------------------------------------------------------------------------+
| SUMMARY: [Jumlah Checklist] [Dalam Proses] [Butuh Verifikasi] [Selesai]           |
+--------------------------------------------------------------------------------------+
| FILTER: [Cari Pegawai/Checklist] [Unit] [Status] [Tenggat] [Reset]                 |
+--------------------------------------------------------------------------------------+
| No | Checklist | Pegawai | Unit | Tenggat | Tugas Selesai | Progres | Status | Aksi |
+--------------------------------------------------------------------------------------+
| DETAIL CHECKLIST: Profil Pegawai | Separation | Tanggal target | Progress server   |
| ITEM: Nama | Kategori | Penerima | Wajib? | Status | Bukti | Diverifikasi Oleh | Aksi|
| [Tambah Item (jika berizin)] [Serahkan] [Verifikasi/Tolak] [Selesaikan]           |
+--------------------------------------------------------------------------------------+
```

- **Detail:** gunakan daftar item dengan progres backend, sorot item wajib yang tertunda; kategori pekerjaan, alat, dokumen administrasi, akses, unit terkait sesuai template approved. Hindari payload data medis pasien.
- **Modal Tambah:** nama tugas, kategori, penerima (`ResourceFilterSelect`), due date, required, deskripsi; validasi server. **Modal Serahkan:** catatan dan attachment resmi. **Modal Verifikasi:** setujui/return plus alasan, actor server. **Modal Finalisasi:** daftar item belum complete, confirmation gate backend.
- **State:** jika checklist belum ada, tampil empty yang mengarahkan ke separation/offboarding; API gagal tidak mengambil checklist pertama dari fallback.

#### UI-12.6 — Clearense (`exit-clearance-matrix-view.jsx`)

```text
+--------------------------------------------------------------------------------------+
| HERO: Clearense                              [Lihat Kasus] [Finalisasi (berizin)] |
+--------------------------------------------------------------------------------------+
| SUMMARY: [Kasus Aktif] [Menunggu Unit] [Finance Pending] [Selesai]                |
+--------------------------------------------------------------------------------------+
| TAB: [Matriks Clearense] | [Daftar Kasus Clearense]                              |
+--------------------------------------------------------------------------------------+
| PILIH SEPARATION: [Cari nomor/pegawai dari API]  [Nomor] [Tanggal Efektif]        |
+--------------------------------------------------------------------------------------+
| UNIT          | Item Wajib | Status      | Tanggungan (izin) | Actor/Waktu | Aksi |
| Unit Asal     | ...        | Pending     | -                 | -           | ...  |
| GA/Aset       | ...        | Completed   | -                 | real        | ...  |
| IT            | ...        | Pending     | -                 | -           | ...  |
| Finance       | ...        | In Review   | data berizin      | real        | ...  |
| HRD           | ...        | Pending     | -                 | -           | ...  |
+--------------------------------------------------------------------------------------+
| DETAIL: Riwayat sign-off | Catatan | Bukti | Issue / tanggungan dari server         |
| PAYROLL / IDENTITY STATUS (informatif): [Menunggu verifikasi sumber otoritatif]      |
| FINALISASI: [Syarat belum lengkap: n] [Tombol Finalisasi hanya jika API membolehkan] |
+--------------------------------------------------------------------------------------+
```

- **Unit pada matriks** disusun dari konfigurasi/response backend. Lima baris pada sketsa adalah **contoh komposisi UI**, **bukan** lima record hardcoded pada produksi. Jika unit resmi RS berbeda, tampilkan daftar resmi. Semua label/kewajiban datang dari service owner.
- **Modal Sign-off:** nama unit/Item+identitas pegawai read-only, pilihan pengembalian, catatan, bukti, nilai outstanding hanya dapat diinput oleh role/unit authorized dan harus diverifikasi backend dengan source Finance. User tidak mengisi `approverUserId`.
- **Panel status:** `AvailableActions` dan lock dari backend; tulisan `Dapat diproses` berarti **status workflow administratif**, bukan perintah transfer dana. Jika sumber Finance belum tersedia, gunakan `BELUM TERVERIFIKASI`, bukan `0` atau “bebas tanggungan”.
- **Finalisasi:** modal merangkum syarat backend; `POST complete` dilakukan sekali; setelah berhasil refetch. Jika gagal, tetap belum selesai, tampil 409/422 dan tidak mengubah local `isLocked`.

### 2.7 State machine UI dan aturan data yang harus direalisasi

| State | Perlakuan UI yang wajib | Larangan |
|---|---|---|
| `INITIAL/LOADING` | skeleton tabel/kartu dan disable aksi | Langsung menampilkan contoh |
| `SUCCESS_WITH_ROWS` | render item server, total server, pagination yang tepat | Mencampur rows API dengan demo |
| `SUCCESS_EMPTY` | empty title/description + reset filter/aksi sah | Mengganti array kosong dengan `DEFAULT_FALLBACK_*` |
| `UNAUTHORIZED 401` | flow login/session standar | “Tidak ada data” menutupi masalah login |
| `FORBIDDEN 403` | `AccessDeniedGate`, aksi disabled | Beralih ke data mock atau bypass role |
| `NOT_FOUND 404` | pesan kasus tidak ditemukan, kembali ke daftar | Pakai detail `proc-demo-1` |
| `CONFLICT 409` | jelaskan status berubah/double submit, refresh data | Mengubah status lokal menjadi sukses |
| `VALIDATION 400/422` | tampilkan kesalahan field dan ringkasan | Menutup modal dan membuang isian |
| `NETWORK/5XX` | alert dengan retry, log aman di dev | Record dummy atau toast sukses |
| `MUTATION_PENDING` | tombol loading/disabled, cegah submit berulang | POST ganda dari double-click |
| `MUTATION_SUCCESS` | toast sukses lalu refetch detail/list/summary; identitas baru server | Menambah row dengan `Date.now()` |
| `MUTATION_FAILED` | modal tetap ada, pesan error, input utuh; GET reconciling bila timeout uncertain | Optimistic permanent update, membuka payroll lock |

**Detail responsive dan aksesibilitas wajib:** desktop 1440px, tablet 768px, mobile 390px. Card wrap 2–4 kolom sesuai lebar; tabel memakai scroll horizontal terkontrol dan tidak menghilangkan kolom identitas/status; filter bisa tersusun vertikal; modal di mobile scroll dengan footer tombol selalu dapat dijangkau; fokus keyboard/tab, label `aria`, status loading, error yang dibaca screen reader, dan tombol disabled jelas. Pertahankan hierarchy Poppins dan design token existing. Screenshot masing-masing enam page pada tiga ukuran ketika browser tersedia, plus modal kritis per page.

**UI yang tidak perlu dibuat ulang:** aplikasi sudah memiliki enam `src/app/hr/separation/*/page.jsx`, enam view, `separation-service.js`, menu sidebar, dan satu stylesheet domain. Prioritaskan **REPAIR/reuse**, bukan copy-paste 6 controller React/HTTP client baru atau penggantian desain global. Pecah view besar ke hook/tabel/form hanya bila benar-benar mengurangi duplikasi dan tidak melanggar pola module existing.

---

## 3. Context

### 3.1 Snapshot branch sumber dan batas kesimpulan

| Peran | Repository @ branch | HEAD diperiksa pada 16 Nov 2026 | Status bukti |
|---|---|---|---|
| Backend | `DevBenari/NewQuilvianSystemBackend` @ `MHamzah` | `1b8e56f3f7abebde9a613ef592430d3da0dfc4d1` | **SOURCE SEEN**, beberapa EF query dan `SaveChangesAsync`; **DB runtime NOT VERIFIED** |
| Frontend | `DevBenari/QuilvianSystemFrontendDev` @ `HamzahV2` | `20478cd652651e8e9b620aaa83cdbce3dcc1648e` | **SOURCE SEEN**, semua enam view memiliki mock/fallback/fake success |
| Governance | `DevBenari/QuilvianEngineeringSkills` @ `main` | `201af64552b27873fd9af83c6722cf11b441bb1e` | Aturan global dan format task dibaca; **runtime plugin invocation hanya dapat dilakukan Agent IDE** |

**Referensi kode yang harus dibuka lagi oleh Agent sebelum mengubahnya:**

**Backend — target scoped:**

- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/ResignationController.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/ResignationRequestService.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/ResignationLifecycleHandoffService.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/RetirementController.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/RetirementService.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/HrdEmploymentContractController.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentContractService.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdNumberSeriesService.cs` (pola integrasi perlu dicek, bukan diasumsikan mendukung semua jenis nomor)
- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/OffboardingHandoverController.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/OffboardingHandoverService.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/ExitClearanceController.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/ExitClearanceService.cs`
- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Controllers/EmployeeSeparationController.cs`
- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Controllers/MultiDeptExitClearanceController.cs`
- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/ExitClearanceService.cs`
- `Repositories/ApplicationDbContext.cs`, DTO/entity/validation, service and DI `Program.cs` sesuai kebutuhan terbukti.

**Frontend — target scoped:**

- `src/utils/menu-sidebar/menu-items.jsx`
- `src/lib/services/hr/separation-service.js`
- `src/lib/axiosInstance/InstanceAxios.jsx` (**read-only reference, jangan mengubah auth global tanpa sebab tepat**)
- `src/components/view/hr/separation/resignation-list-view.jsx`
- `src/components/view/hr/separation/retirement-list-view.jsx`
- `src/components/view/hr/separation/contract-expiry-view.jsx`
- `src/components/view/hr/separation/termination-list-view.jsx`
- `src/components/view/hr/separation/handover-list-view.jsx`
- `src/components/view/hr/separation/exit-clearance-matrix-view.jsx`
- `src/app/hr/separation/page.jsx` dan 6 `src/app/hr/separation/{resignations,retirements,contract-expiry,terminations,handovers,exit-clearance}/page.jsx`
- `src/style/hr/separation/separation.module.css`
- Base components `src/components/features/base-features/`, hook resource pegawai existing dan `src/app/globals.css`.

### 3.2 Endpoint nyata yang terlihat pada source — bukan janji runtime

> Base backend controller lazim menggunakan `api/v1/...`, sedangkan FE `separation-service.js` menyimpan path relatif `/v1/...`. **Jangan otomatis menganggap mismatch:** `InstanceAxios` bisa mengatur prefix `/api`; verifikasi `baseURL` dan request network aktual. Table ini adalah **route controller yang dilihat di source pada SHA di atas**; kontrak request/response perlu dibaca DTO dan bukan ditebak dari nama endpoint.

| Swagger group / controller | Method | Path controller yang terlihat | Kegunaan / catatan |
|---|---|---|---|
| `[Tags] ResignationRequest` | `GET` | `api/v1/corporate/human-resource/lifecycle-management/resignation-requests` | List filter/paged |
| `ResignationRequest` | `GET` | `.../resignation-requests/summary`, `.../resignation-requests/{id}` | Summary/detail |
| `ResignationRequest` | `POST` | `.../resignation-requests` | Create |
| `ResignationRequest` | `POST` | `.../resignation-requests/{id}/approve`, `.../reject`, `.../handoff` | Decision dan linking offboarding |
| `RetirementAdministration` | `GET` | `api/v1/corporate/human-resource/lifecycle-management/retirements` | List/paged cases |
| `RetirementAdministration` | `GET` | `.../retirements/eligible-employees`, `.../summary`, `.../retirements/{id}` | Candidate dari DOB master, summary/detail |
| `RetirementAdministration` | `POST/PUT` | `.../retirements`, `.../retirements/{id}` | Create/update case |
| `RetirementAdministration` | `POST` | `.../retirements/{id}/approve`, `.../handoff` | Approval/handoff |
| `WorkforceEmploymentContract` | `GET` | `api/v1/corporate/human-resource/employment-contracts`, `.../{id}`, `.../{id}/fixed-term-check` | Contract list/detail/fixed-term result |
| `WorkforceEmploymentContract` | `POST` | `.../employment-contracts/{id}/decisions` dan `.../decisions/{decisionId}/notify` | T16 decision/notifikasi |
| `EmployeeSeparation` | `GET/POST` | `api/v1/hrd/separations` | List/initiate |
| `EmployeeSeparation` | `POST` | `api/v1/hrd/separations/{separationId}/calculate-severance`, `.../approve-severance` | Calculation / Finance approval |
| `EmployeeSeparation` | `GET` | `api/v1/hrd/separations/{separationId}/severance-breakdown` | Detail komponen hak |
| `OffboardingHandoverAdministration` | `GET` | `api/v1/corporate/human-resource/lifecycle-management/handovers`, `.../summary`, `.../{id}` | List/detail checklist |
| `OffboardingHandoverAdministration` | `POST` | `.../handovers/{id}/tasks`, `.../handovers/{id}/complete` | Tambah task/complete |
| `OffboardingHandoverAdministration` | `PUT` | `.../handovers/{id}/tasks/{taskId}/submit`, `.../verify` | Submit/verify item |
| `ExitClearance` lifecycle | `GET` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances`, `.../summary`, `.../by-separation/{separationId}` | Satu keluarga aggregate clearance |
| `ExitClearance` lifecycle | `POST/PUT` | `.../exit-clearances` dan endpoint checklist/verify-finance/complete | Mutasi lifecycle clearance |
| `MultiDeptExitClearance` | `GET` | `api/v1/hrd/exit-clearance/{separationId}`, `.../payroll-lock-status/{employeeId}` | Keluarga relasi/kliring & lock |
| `MultiDeptExitClearance` | `PUT` | `api/v1/hrd/exit-clearance/checklists/{checklistId}/signoff` | Sign-off unit (P0 security) |
| `MultiDeptExitClearance` | `POST` | `api/v1/hrd/exit-clearance/{clearanceProcessId}/complete` | Complete keluarga relasi |

**Wajib dibuat Agent sebagai kontrak nyata, bukan asumsi:** `DTO field mapping` antara `ApiResponse<PagedResult<T>>` dan `separation-service.js`, server pagination keys per endpoint, permission exact name dari `[AccessPermission]`, nilai enum/status, actor claims, validasi tanggal, error response, dan asal resource ID. **Dua controller clearance tidak otomatis dua sumber data yang kompatibel**; tetapkan owner dan adapter sebelum menggabungkan UI.

### 3.3 Sumber canonical HRD dan aturan task

Backend `docs/module-blueprints/human-resource/`:

1. `README.md`, `MODULE-STATUS.md`, `00-interview-decisions.md`, `01-existing-capability-map.md`, `00-business-overview.md`, `01-prerequisite-readiness.md`.
2. `roadmap/00-slice-roadmap.md`, `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`.
3. `roadmap/backend-unfinished-work.md`, `roadmap/frontend-unfinished-work.md` **hanya catatan audit historis**, harus recheck.
4. `contracts/`, `flowcharts/`, `data/`, dan laporan aktual `task/report/backend/<TASK-ID>.md` serta `task/report/frontend/<TASK-ID>.md`.
5. `docs/update/pemberhentian-dan-pensiun/INSTRUKSI_AGENT_HRD_MENU_12_PEMBERHENTIAN_DAN_PENSIUN.md` ini sebagai mandat terbaru setelah disimpan; laporan `LAPORAN_IMPLEMENTASI_DAN_UAT_MENU_12_PEMBERHENTIAN_DAN_PENSIUN.md` versi sebelumnya adalah **HISTORICAL CLAIM**, bukan proof UAT 30 skenario.

Governance `QuilvianEngineeringSkills/agents/rules/`: `GLOBAL_RULES.md`, `backend/TASK_RULES.md`, `backend/TASK_CLASSIFICATION.md`, `backend/API_RULES.md`, `backend/DATABASE_RULES.md`, `backend/CROSS_REPO_RULES.md`, `backend/REVIEW_RULES.md`, `backend/TEST_POLICY.md`, `backend/role-access-rules.md`, `backend/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `frontend/frontend-architecture.md`, `frontend/base-component-catalog.md`, `frontend/base-component-decision-gate.md`, `frontend/page-composition-patterns.md`, `frontend/design-tokens.md`, `frontend/test-policy.md`, `frontend/ui-consistency-checklist.md`, `testing/` rules, `rule-output/` rules. Ikuti file `AGENTS.md` dan `CLAUDE.md` kedua repositori terlebih dahulu.

**Kontrak izin yang ditegaskan:**

- `BACKEND MODE`: **Backend writable**, FE hanya referensi read-only. Tidak ada perubahan FE dalam fase ini.
- `FRONTEND MODE`: **FE writable**, Backend source read-only; hanya laporan tracked/roadmap status narrow ke backend docs menurut governance.
- `MODULE BLUEPRINT MODE`: **docs HRD saja** dengan izin dan workflow approval yang benar; tidak mengubah source BE/FE dalam fase ini.
- `CROSS-REPO MODE`: hanya untuk integrasi terkoordinasi yang secara eksplisit diizinkan (prompt ini mengizinkan koordinasi dalam scope Menu 12, **bukan** bypass satu-task-per-Skill atau sumber izin migrasi/Git).

### 3.4 Keputusan terkunci dan yang belum boleh diasumsikan

**Dikunci oleh permintaan revisi ini:** (a) Menu 12 memiliki tepat enam Submenu dan label `Clearense`; (b) seluruh tampilan operasional **harus data nyata dari Backend/database**, tanpa dummy/fallback/fake success; (c) implementasi **Backend terlebih dahulu** baru Frontend; (d) setiap pekerjaan dipecah task-ID pola Build Module backend/frontend; (e) skema UI harus terimplementasi melalui base component Quilvian; (f) UAT dengan API/persistence serta laporan evidence-based.

**Bukan izin untuk mengarang policy:** kebijakan usia pensiun berdasarkan kelompok pegawai, urutan approver legal/HRD/Finance bila belum dipetakan, keputusan pembayaran normatif ketika ada outstanding clearance, otomatisasi penutupan SIMRS/akun, kompensasi PKWT untuk semua kasus, identitas satu-satunya aggregate clearance bila source bertentangan, atau DDL/migration baru. Jika canonical `00-interview-decisions.md` punya keputusan approved, patuhi itu; bila belum ada, tandai `DECISION_REQUIRED` dengan dampak area yang tepat dan kerjakan pekerjaan lainnya.

---

## 4. Reasoning / Execution Workflow

### 4.1 Preflight governance singkat — maksimal cakupan yang relevan

**Urutan operasional nyata (bukan laporan audit sebagai deliverable akhir):**

1. Pada dua workspace, jalankan pembacaan Git **read-only** seperti `git branch --show-current`, `git rev-parse HEAD`, `git status --short --branch`, dan cek upstream yang sudah dikonfigurasi tanpa `git fetch/pull`. Konfirmasi BE `MHamzah`, FE `HamzahV2`, tidak mengubah file user yang tidak terkait. Bila branch salah, STOP fase terdampak dan laporkan.
2. Baca `AGENTS.md`, `CLAUDE.md`, `GLOBAL_RULES.md`, QBE, aturan task/review/API/DB/FE/testing dan `README` HRD. Verifikasi runtime Skill Build terdaftar; **jangan menggantinya dengan membaca SKILL.md manual**.
3. Baca kontrak/decision dan report yang terkait enam Submenu serta task existing yang disebut di 2.4. Klasifikasi task, cek source FE caller dan BE owner secara ringkas; hasil catat path/symbol/commit.
4. Verifikasi apakah nomor usulan B01–B10/F01–F09 sudah ada pada roadmap HEAD. Ikuti aturan registrasi nomor task pada 2.4. Jika dibutuhkan sinkronisasi canonical yang memang diizinkan, jalankan `MODULE BLUEPRINT MODE` **terbatas** sebelum Build Module. Tidak boleh mengganti status lama `✅` menjadi `✅` lagi tanpa bukti baru.
5. **Mulai dari perbaikan source backend untuk task approved pertama yang unblock**. Jangan menghabiskan satu sesi hanya untuk inventaris atau permintaan konfirmasi yang sudah dijawab.

### 4.2 Tahap A — Backend database/persistence SEBELUM Frontend

**Satu task per invokasi `quilvian-engineering-skills:build-module-backend`:** mulai B01 → B02 → B03 → B04...B10 setelah ID sah, hormati dependency. Baca QBE naming/owner/prefix dan patterns sebelum setiap modifikasi entity/API. Gunakan service & EF Core existing; jangan membuat repository generic paralel.

**Bukti DB real yang harus dapat ditelusuri tanpa SQL mutasi langsung:**

| Tahap | Pemeriksaan | PASS hanya jika |
|---|---|---|
| 1. Read source | GET list/summary/detail memanggil EF Core entity owner | `MstEmployee`, `MstWorkforceProfile`, `TrxResignationRequest`, `TrxRetirement`, kontrak, separation, offboarding, clearance query benar; bukan constant list |
| 2. Source mutation | POST/PUT memvalidasi dan menulis lewat service | Terdapat `SaveChangesAsync`/transaction/approval audit sesuai pola owner; bukan in-memory dictionary |
| 3. API valid | Controller menerima request dan mengembalikan envelope/status sah | HTTP/DTO/field/permission sesuai kontrak; tidak hanya “200 OK” tanpa record |
| 4. Persistence runtime | Pada dev/staging berizin: GET awal → POST/PUT test → GET detail → refresh/re-login → GET ulang | Record sama, ID/nomor FK/status/actor/timestamp sah; tidak hilang karena restart UI |
| 5. Negative & isolation | 401/403/404/409/422/500, foreign actor, invalid state, duplicate submit | Tidak ada side-effect ketika gagal; status server tetap benar |
| 6. Cross-feature integrity | resign/retirement/contract/PHK menghasilkan separation/offboarding canonical | Tidak ada kasus ganda, tidak ada penonaktifan pegawai prematur, tidak ada actor palsu |
| 7. Database operations | Periksa skema melalui source/migration metadata; DB read-only jika diberikan izin terpisah | Tidak menjalankan SQL/migration/seed secara otomatis; schema blocker dilaporkan tepat |

**Ketentuan khusus test backend:** `rules/backend/TEST_POLICY.md` menetapkan **jangan otomatis membuat project test backend** (`Tests/`, xUnit/NUnit/MSTest, dll.) tanpa permintaan eksplisit. Wajib tetap `dotnet build` project aplikasi untuk tiap task BE yang menyentuh source; `dotnet restore` bila dependency/project disentuh; verifikasi QBE, API contract, state/business, diff, dan runtime manual yang tersedia. Laporkan `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis`. Jangan mengarang hasil query DB, HTTP, atau build.

**Gerbang BACKEND→FRONTEND:**

- Enam domain memiliki GET list/detail (atau sumber owner existing yang equivalent), DTO/envelope/paging konsisten, user login punya role/permission yang tepat, nomor/persisted mutation server sah, bug actor identity P0 ditutup, status negative/retry teruji setidaknya pada API relevan.
- Buat kontrak BE aktual per Submenu: `HTTP method + relative URL + sample shape yang diperoleh dari DTO/source + status enum + permission + error semantics + IDs`. Jangan hardcode JSON contoh yang tidak cocok DTO.
- Jika Backend source sudah diperbaiki tetapi **DB runtime/scheme belum bisa dibuktikan**, beri `BACKEND IMPLEMENTED / RUNTIME NOT VERIFIED`, bukan PASS. FE boleh menyiapkan adapter/empty/error tanpa klaim UAT end-to-end; task FE yang bergantung runtime tetap `BLOCKED` sampai lingkungan/izin sah.

### 4.3 Tahap B — Frontend SETELAH kontrak Backend fixed

**Satu task per invokasi `quilvian-engineering-skills:build-module-frontend`:** F01 anti-mock/service dahulu; kemudian F02–F07 masing-masing per Submenu; F08 konsistensi base component dan navigation; F09 UAT. Agent **harus menulis code** di enam existing view dan service, bukan kembali membuat rancangan/PRD.

1. Bandingkan `separation-service.js` dengan controller/DTO aktual dan Axios config; normalisasi `{ data: { data: ... } }` serta `PagedResult` sesuai benar-benar yang dilihat. Hapus seluruh sample arrays dan fallback paths dari mode operasional. Audit `catch`, callback dan derived totals pada enam view; jalankan pencarian sumber yang setara dengan `DEFAULT_FALLBACK_`, `demo-`, `mock`, `Math.random`, `Date.now`, `fallback`, `setApiItems`, `setProcessDetail`, `setLockStatus`, `pushToast("success"` secara kontekstual. Tidak semua `Date.now` adalah salah (timestamp UI boleh), tetapi tidak boleh menjadi ID transaksi.
2. Per page, pindahkan fetch/actions ke service/hook existing pattern; state loading/empty/error/unauthorized/pending/refetch terstruktur. Gunakan server pagination jika data banyak. Search dan combined filters harus mempengaruhi request backend jika kontrak mendukung, bukan hanya data 10 baris.
3. Perbaiki semua modal create/approve/reject/decision/finance/signoff/finalize agar tidak ada optimistic success jika request reject/timeout. Jika request timeout namun server mungkin sudah commit, GET reconcile sebelum user coba lagi; rekomendasi idempotency key harus cocok kontrak approved.
4. Terapkan UI sketches 2.6 melalui base component dan tokens existing; `AccessDeniedGate` untuk unauthorized, `ConfirmModal` untuk tindakan kritis, `ResourceFilterSelect` pegawai, `DataTable` dan `Pagination` untuk daftar. Jangan mengubah UI global, membuat CSS Tailwind, raw token/font atau memalsukan foto/identitas pegawai.
5. Cek route sidebar pengguna dan backward compatibility. Verifikasi akses lebih dari satu role; tidak cukup login SuperAdmin. Render data master/nama manusia hanya bila berasal dari API, bukan const array hardcoded.
6. Jalankan CSS checker `node <runtime-rules-root>/frontend/tools/check-css-usage.mjs` untuk **file JSX yang diubah** sesuai aturan dan perintah lint/build dari `AGENTS.md`; target checker 0 temuan. Buka tiap page di Chrome headed, screenshot, periksa modal, tabel, filter, smartphone, dan keyboard. Bila tidak bisa membuka browser, catat `VISUAL CHECK: NOT FEASIBLE`, task UI maksimal 🟡.

### 4.4 Tahap C — UAT live 30 skenario dan retest

**Syarat eksekusi:** Chrome/Chromium **headed** pada aplikasi lokal/staging yang memang diizinkan, akun multi-role uji, master pegawai uji non-produksi yang sah, dan backend runtime yang sudah dapat diakses. Bila environment/kredensial belum tersedia, jangan melakukan data manipulation ke pegawai rumah sakit sungguhan; laporkan `UAT: NOT RUN/BLOCKED` dan lanjutkan validasi source yang aman. Browser test harus berinteraksi (klik, isi, submit, kembali, refresh, ganti user), bukan screenshot statis. Seluruh bukti memakai data yang disamarkan.

| ID | Level | Area | Skenario aksi yang WAJIB dijalankan | Syarat PASS yang dapat diukur |
|---|---|---|---|---|
| `UAT-M12-01` | P0 | Pengunduran Diri | GET list saat DB 0 | Tabel 0/empty, bukan `res-demo` |
| `UAT-M12-02` | P0 | Pengunduran Diri | Putus koneksi API/403 dan refresh | Error/denied; tak ada pegawai contoh |
| `UAT-M12-03` | P0 | Pengunduran Diri | Create request dari pegawai test | ID dan nomor dari server, GET ulang record sama |
| `UAT-M12-04` | P0 | Pengunduran Diri | Approve dan reject dengan role yang tepat | Status+actor+alasan tersimpan pada API; unauthorized ditolak |
| `UAT-M12-05` | P0 | Pengunduran Diri | Handoff dua kali / dua tab | Satu separation/clearance/offboarding untuk satu event |
| `UAT-M12-06` | P0 | Pengunduran Diri | Form invalid/notice tidak sesuai/role lain | 400/403/409/422 jujur, tidak ada fake success |
| `UAT-M12-07` | P0 | Pensiun | Cari kandidat berdasarkan DOB pegawai test | Daftar/umur berasal dari master BE, policy yang berlaku ditampilkan |
| `UAT-M12-08` | P0 | Pensiun | Data belum eligible / policy belum ada | Tidak auto-approve; error/block sesuai policy |
| `UAT-M12-09` | P0 | Pensiun | Create rencana kemudian approve | `TrxRetirement` tersimpan, nomor unik dan status sah |
| `UAT-M12-10` | P0 | Pensiun | Handoff sebelum Approved, lalu setelah Approved | Sebelum approved ditolak; setelah sah tepat satu separation |
| `UAT-M12-11` | P0 | Akhir Kontrak | Filter kontrak berdasarkan status/tanggal 30/60/90 | Hasil dan total dari DB, bukan `ctr-demo` |
| `UAT-M12-12` | P0 | Akhir Kontrak | Jalankan `fixed-term-check` | Masa kontrak akurat dari API, tanpa default 24/36 bulan |
| `UAT-M12-13` | P0 | Akhir Kontrak | Keputusan Extend vs NotRenew | Extend tidak memicu termination; nonrenewal sah handoff satu kali |
| `UAT-M12-14` | P1 | Akhir Kontrak | Notifikasi & buka riwayat setelah relogin | Status/tanggal/catatan keputusan persist dan sesuai izin |
| `UAT-M12-15` | P0 | Pemberhentian | Inisiasi PHK dari master pegawai test | Nomor SEP asli, relasi & status benar sesudah refresh |
| `UAT-M12-16` | P0 | Pemberhentian | Kalkulasi & buka rincian pesangon | Angka dari engine BE; saat API error tidak ada angka simulasinya |
| `UAT-M12-17` | P0 | Pemberhentian | Approve Finance dengan role sah dan bukan Finance | Role tidak sah 403, keputusan sah menyimpan audit |
| `UAT-M12-18` | P0 | Pemberhentian | Kasus belum sah/sengketa/duplicate submit | Tidak auto-final dan tidak mengganda record |
| `UAT-M12-19` | P0 | Serah Terima | Buka checklist linked dari separation riil | Item dan progress berasal dari BE, tanpa `hnd-demo` |
| `UAT-M12-20` | P0 | Serah Terima | Add → submit → verify → refresh & re-login | Status/progress/actor/time sama di akun yang berwenang |
| `UAT-M12-21` | P0 | Serah Terima | Verifikasi oleh unit lain dan complete saat pending | 403/409/422 dari BE, tidak ada toast selesai palsu |
| `UAT-M12-22` | P0 | Clearense | Pilih separation dari API, buka matriks | Case benar, tidak `proc-demo`, unit dari server |
| `UAT-M12-23` | P0 | Clearense | Sign-off unit berwenang | Checklist status, actor server, timestamp dan bukti persist |
| `UAT-M12-24` | P0 | Clearense | Spoof `approverUserId`, actor missing, role unit lain | Mutasi ditolak; tidak ada perubahan DB |
| `UAT-M12-25` | P0 | Clearense | Outstanding Finance masih ada | Gate benar menolak finalisasi yang belum boleh; nominal sah |
| `UAT-M12-26` | P0 | Clearense | Lengkapi required items dan complete | Status final dari API, lock hanya berubah menurut kontrak sah |
| `UAT-M12-27` | P0 | Clearense | 404/500 saat load/signoff/complete | Detail/lock tidak dipalsukan; error+retry terlihat |
| `UAT-M12-28` | P0 | Clearense | Signoff/finalize ganda & concurrent | Idempoten, tidak ada dua clearance atau double release |
| `UAT-M12-29` | P1 | Semua | Refresh, logout/login, 0-record, search, filter gabungan, pagination, panjang teks & XSS | Data authoritative stabil, permission privat, no local demo; error states konsisten |
| `UAT-M12-30` | P2–P5 | Semua | Journey lintas Kepala Unit/HRD/Finance/IT; screenshot 1440/768/390, keyboard, modal, periksa nav lama | Semua 6 route usable, base component tepat, responsive dan a11y tidak regresi |

**Untuk semua test P0/P1 tambahan yang lintas route:** jalankan `401`, `403`, `404`, `409`, `422`, `500`, timeout, double-click dan lihat bahwa **tidak ada sukses palsu**. Jangan hanya memberi label PASS berdasarkan halaman terbuka. Bila API mengembalikan 200 tetapi `success: false` atau data tidak berisi ID transaksi sah, perlakukan sebagai gagal sesuai envelope backend. Screenshot wajib menunjukkan hasil yang tidak menyertakan data pribadi asli; network evidence disanitasi.

**Format bukti per skenario yang harus dicatat:** `ID`, tanggal/waktu absolut, browser/resolusi, route & status auth role tersamarkan, input sumber pegawai test, endpoint + method + HTTP status (tanpa token), sebelum/aksi/sesudah, ID/nomor transaksi yang telah disamarkan bila perlu, hasil GET setelah reload, screenshot path, `PASS/FAIL/BLOCKED/NOT RUN`, defect yang ditemukan, patch file dan hasil retest. Untuk status PASS persistence, **harus ada bukti read-after-write**; screenshot tabel saja tidak cukup.

### 4.5 Gate akhir Definition of Done (DoD)

| Gate | Wajib PASS sebelum status akhir `VERIFIED WORKING` |
|---|---|
| `DOD-01` | Keenam route dapat dijangkau, label persis target, legacy route tidak rusak |
| `DOD-02` | `DEFAULT_FALLBACK_*`, demo/mock/fake success di enam halaman **nihil dalam path operasional** |
| `DOD-03` | Semua GET/summary/paged/status berasal dari database via API, 0 data tetap 0 |
| `DOD-04` | Semua mutasi hanya sukses bila backend memvalidasi dan mem-persist; refetch sesudah mutasi |
| `DOD-05` | Actor/permission/ownership per-record benar, anti spoofing clearance terbukti |
| `DOD-06` | Approval/handoff/finalize sah dan idempoten; tanggal efektif dan status pegawai tidak prematur |
| `DOD-07` | Number series, FK dan separation/clearance tidak ganda; schema readiness dipastikan |
| `DOD-08` | Kontrak data, pagination, enum/error mapping FE–BE cocok dengan controller/DTO aktual |
| `DOD-09` | Finance, severance, payroll status berasal dari sumber resmi, bukan hitungan atau lock UI |
| `DOD-10` | `dotnet build` BE, lint/Checker FE, visual screenshots dan QA benar-benar dijalankan serta dicatat |
| `DOD-11` | UAT P0/P1 relevan PASS dengan hasil read-after-write; test yang tak berjalan diberi NOT RUN |
| `DOD-12` | Laporan tracked tiap task approved dan status roadmap/traceability tersinkron menurut wewenang |

**Status yang diperbolehkan:** `VERIFIED WORKING` hanya jika seluruh gate relevan PASS; `IMPLEMENTED, NOT VERIFIED` untuk source selesai tetapi runtime/DB/UAT belum dibuktikan; `PARTIAL` untuk acceptance belum lengkap; `REPAIR` jika perilaku palsu/bug ada; `BLOCKED` jika governance/DB schema/izin tak terpenuhi. Jangan mengklaim `100% selesai/siap produksi` hanya karena build 0 error atau 7 halaman tampak benar.

---

## 5. Stop conditions

**JANGAN berhenti hanya karena:** ditemukan kode existing, sudah menyusun planning, membuat dokumen, schema class tersedia, API 200 sekali, CSS checker 0 temuan, lint/build lolos, screenshot halaman ada, atau regresi muncul dalam scope. **Perbaiki source, validasi ulang, dan lanjutkan task independen.**

**WAJIB berhenti pada bagian terdampak saja** ketika:

1. Branch/upstream berbeda dari `MHamzah`/`HamzahV2` atau working tree memiliki konflik perubahan user yang berisiko hilang; jangan checkout/reset/stash/pull.
2. `AGENTS.md`, global rules, QBE atau `CLAUDE.md` wajib tidak tersedia: `BLOCKED — canonical governance unavailable`; atau registered Build Module Skill tidak tersedia: `BLOCKED — Quilvian runtime skill unavailable`.
3. ID task baru belum approved/terdaftar, blueprint API target berstatus draft/stale/superseded, atau izin `MODULE BLUEPRINT MODE` untuk mengubah roadmap/kontrak tidak ada; **task existing yang sah tetap dikerjakan** bila independen.
4. Memerlukan schema change, pembuatan migration, eksekusi migration/SQL, seed database bersama, bypass auth, aksi Finance/Identity di luar owner, atau deployment; minta otorisasi tindakan terpisah dan jangan melakukannya diam-diam.
5. Kebijakan HR sensitif yang belum disetujui mengubah hak pekerja, batas pensiun, pemutusan legal, kompensasi normatif, data klinis, atau pencabutan akses. Tandai `DECISION_REQUIRED`, lanjutkan bagian tidak bergantung.
6. Belum tersedia environment dev/staging dan akun UAT yang sah: jangan membuat record pekerja/pasien real untuk test. Tulis `UAT NOT RUN/BLOCKED` dan selesaikan validasi statis/source yang diizinkan.

**Dilarang** auto `git add/commit/push/pull/fetch/merge/rebase/reset`, deploy, membuat atau menjalankan migration, mengubah database langsung, menginstal dependency/menaikkan Next/React, mengganti pola arsitektur global, mengubah rules/Engineering Skills, atau mematikan validasi/otorisasi demi melewati test. Setiap blocker harus menyebut satu langkah pemulihan yang konkret, file/contract/task yang terpengaruh, dan pekerjaan independen yang sudah dilanjutkan.

---

## 6. Output

**Wajib dihasilkan Agent setelah benar-benar menulis source sesuai task resmi:**

1. **Ringkasan task terurut** `BE-HRD-NNN` dahulu, lalu `FE-HRD-NNN`: nomor resmi yang benar-benar dipakai (jangan menulis nomor usulan sebagai sudah registered), klasifikasi, dependency, status `✅/🟡/⛔`, file modified/new, diff summary dan acceptance yang telah terpenuhi.
2. **Matriks 6 Submenu:** route FE → service method → endpoint BE → controller/service/DTO → entity/DB owner → mutation/read-after-write → role/AvailableActions → hasil source/runtime/UAT → blocker. Tulis `SOURCE SEEN` terpisah dari `RUNTIME PASS`.
3. **Bukti Backend:** laporan QBE preflight dengan Area/Module/owner/prefix dan rule IDs; `dotnet build` real (command, date, exit code, errors/warnings), kontrak HTTP/DTO/status, bukti transaksi dan isolation via API yang diotorisasi, status schema/migration yang benar, daftar kelemahan yang masih ada.
4. **Bukti Frontend:** bukti mock/fallback/fake success dihapus (path+line), mapping pagination/error/role nyata, hasil lint/check-css-usage dan UI Gate, keputusan base component, screenshot enam route (1440/768/390) serta detail modal kritis, hasil manual test.
5. **UAT resmi Menu 12:** 30 baris `UAT-M12-01..30` dengan PASS/FAIL/BLOCKED/NOT RUN dan bukti per skenario. Perbaiki bug dalam scope dan tulis retest/hasil terbaru, bukan mengulang klaim “7 screenshot = 100%”.
6. **Laporan tracked sesuai governance:** per task Backend `docs/module-blueprints/human-resource/task/report/backend/<TASK-ID>.md`; per task FE `docs/module-blueprints/human-resource/task/report/frontend/<TASK-ID>.md` pada repository backend yang menyimpan blueprint. Perbarui **status dan tautan bukti saja** di roadmap serta `requirement-traceability.md` sesuai wewenang; jangan merombak task existing. Jika ada laporan lama yang mengklaim selesai tetapi bertentangan dengan source, catat koreksi evidence dan status terbaru tanpa menghapus jejak historis.
7. **Status penutup yang jujur:** readiness `VERIFIED WORKING`/`IMPLEMENTED NOT VERIFIED`/`PARTIAL`/`BLOCKED`, risiko P0/P1 tersisa, perintah validasi yang dijalankan, status uncommitted Git tanpa melakukan commit, dan **satu next action** yang konkret untuk blocker tersisa. Jangan membuat enam dokumen instruksi baru atau mengirim “audit selesai” sebagai pengganti implementasi.

### Format ringkasan yang harus dikirim Agent di akhir

```text
HRD MENU 12 — PERBAIKAN DATA RIIL
Backend branch / SHA: MHamzah / (isi dari runtime)
Frontend branch / SHA: HamzahV2 / (isi dari runtime)
Skills branch / SHA: main / (isi dari runtime)
Task approved BE selesai / sebagian / blocked: (isi berdasarkan bukti)
Task approved FE selesai / sebagian / blocked: (isi berdasarkan bukti)
6 Submenu real GET: PASS / PARTIAL / BLOCKED per layar
6 Submenu real MUTATION: PASS / PARTIAL / BLOCKED per layar
Anti-Dummy / Fake Success: PASS / FAIL (path dan cek bukti)
Security P0 approver/permission: PASS / FAIL (bukti)
Database/EF persistence via API read-after-write: PASS / NOT RUN / FAIL
Backend build: (command, exit code, errors/warnings nyata)
FE lint & CSS: (command, exit code, findings nyata)
UAT headed 30: PASS n / FAIL n / BLOCKED n / NOT RUN n
Perubahan file: (daftar path)
Report tracked & roadmap: (link/path nyata)
Schema/migration: NONE / NEEDS AUTHORIZATION (jangan jalankan otomatis)
Git status: (status read-only; tidak ada commit/push)
Keputusan akhir: VERIFIED WORKING / IMPLEMENTED NOT VERIFIED / PARTIAL / BLOCKED
Blocker prioritas: (bukti dan satu next action)
```

**PERINTAH TERAKHIR KEPADA AGENT:** **Mulai implementasi source BACKEND yang diizinkan sekarang**, satu task-ID approved per invokasi `build-module-backend`. Buktikan query, mutation, idempotensi, otorisasi dan persistence pada API/backend terlebih dahulu. Setelah kontrak Backend benar dan gate terpenuhi, lanjutkan **Frontend** satu task-ID approved per invokasi `build-module-frontend`, hapus seluruh fallback/dummy/fake success, bangun enam UI sesuai skema di atas dan lakukan UAT 30 skenario. **Jangan mengakhiri pekerjaan hanya dengan audit, build pass, laporan, atau screenshot.**

