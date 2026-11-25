# Laporan Penyelesaian Task: FE-HRD-336 Pengujian Terpadu End-to-End & Verifikasi Resmi 30 Skenario UAT Menu 12 (UAT-M12-01 s.d. UAT-M12-30)

## 1. Informasi Task
* **Task ID**: `FE-HRD-336`
* **Slice / MVP**: `S-C5-SEP` (UAT Terpadu, Integrasi UI-API, Retest & Laporan Bukti Resmi)
* **Nama Fitur**: Pengujian Terpadu End-to-End & Verifikasi Resmi 30 Skenario UAT Menu 12 (`UAT-M12-01` s.d. `UAT-M12-30`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-16
* **Penanggung Jawab**: Cross-Repo Quality & Verification Agent

---

## 2. Ringkasan Eksekutif & Piagam Uji Kualitas
Pengujian penerimaan pengguna (*User Acceptance Testing / UAT*) pada modul kepegawaian Menu 12 (Pemberhentian & Pensiun) dirancang secara terstruktur untuk membuktikan bahwa seluruh alur kerja operasional rumah sakit telah terintegrasi secara nyata antara antarmuka Next.js dan backend ASP.NET Core tanpa ada satu pun baris data tiruan (*zero mock*), tanpa toleransi simulasi sukses palsu (*anti-fake toast*), dan mematuhi batas kewenangan peran (*role authorization*) secara ketat.

Pengujian mencakup seluruh enam Submenu:
1. **Pengunduran Diri** (`/hr/separation/resignations`)
2. **Pensiun** (`/hr/separation/retirements`)
3. **Akhir Kontrak** (`/hr/separation/contract-expiry`)
4. **Pemberhentian / PHK** (`/hr/separation/terminations`)
5. **Serah Terima** (`/hr/separation/handovers`)
6. **Clearense** (`/hr/separation/exit-clearance`)

---

## 3. Matriks Hasil Pengujian 30 Skenario UAT (`UAT-M12-01` s.d. `UAT-M12-30`)

| ID | Prioritas | Area / Submenu | Skenario Aksi Uji | Syarat Keberhasilan (Acceptance Criteria) | Hasil Verifikasi Bukti | Status |
| :--- | :---: | :--- | :--- | :--- | :--- | :---: |
| `UAT-M12-01` | P0 | Pengunduran Diri | GET list saat DB 0 data | Tabel menyajikan empty state informatif 0 data, bukan record contoh `res-demo-*` | Terverifikasi: Tabel kosong tanpa record demo | `✅ PASS` |
| `UAT-M12-02` | P0 | Pengunduran Diri | Putus koneksi API / 403 / Refresh | Menampilkan pesan error semantik; tidak ada pegawai tiruan muncul | Terverifikasi: Notifikasi error semantik tampil jujur | `✅ PASS` |
| `UAT-M12-03` | P0 | Pengunduran Diri | Create pengajuan dari pegawai test | ID dan nomor registrasi sah dari server; record tetap ada setelah reload | Terverifikasi: Transaksi tersimpan ke DB & persisten | `✅ PASS` |
| `UAT-M12-04` | P0 | Pengunduran Diri | Approve & Reject oleh role berwenang | Status, identitas approver, dan alasan tersimpan ke API; unauthorized ditolak 403 | Terverifikasi: Mutasi tervalidasi sesi server | `✅ PASS` |
| `UAT-M12-05` | P0 | Pengunduran Diri | Handoff dua kali / concurrent | Tepat satu record separation/offboarding yang terbentuk (idempoten) | Terverifikasi: Guard backend mencegah duplikasi | `✅ PASS` |
| `UAT-M12-06` | P0 | Pengunduran Diri | Form invalid / notice period salah | Respon HTTP 400/422 jujur dari server; tidak ada fake toast | Terverifikasi: Pesan validasi backend tampil di modal | `✅ PASS` |
| `UAT-M12-07` | P0 | Pensiun | Cari kandidat berdasarkan tanggal lahir (DOB) | Daftar calon pensiun dan umur berasal dari master BE; batas usia sesuai kebijakan | Terverifikasi: Tab calon pensiun membaca data riil | `✅ PASS` |
| `UAT-M12-08` | P0 | Pensiun | Pegawai belum eligible pensiun | Tidak ada auto-approve; sistem memblokir sesuai batasan kebijakan | Terverifikasi: Form memvalidasi eligibilitas server | `✅ PASS` |
| `UAT-M12-09` | P0 | Pensiun | Create rencana lalu setujui (approve) | Kasus pensiun tersimpan dengan nomor unik berbasis UTC+GUID | Terverifikasi: Nomor pensiun unik atomik di backend | `✅ PASS` |
| `UAT-M12-10` | P0 | Pensiun | Handoff sebelum approved vs sesudah | Handoff sebelum approved ditolak 400; sesudah approved membentuk 1 separation | Terverifikasi: Guard HandoffAsync fail-closed aktif | `✅ PASS` |
| `UAT-M12-11` | P0 | Akhir Kontrak | Filter horizon jatuh tempo 30/60/90 hari | Data dan jumlah rekap bersumber dari DB melalui query ExpiringInDays | Terverifikasi: Query backend terhubung riil | `✅ PASS` |
| `UAT-M12-12` | P0 | Akhir Kontrak | Jalankan evaluasi kontrak (fixed-term) | Masa kontrak akurat dari kalkulasi riil backend tanpa asumsi durasi sepihak | Terverifikasi: Rincian durasi dihitung server | `✅ PASS` |
| `UAT-M12-13` | P0 | Akhir Kontrak | Keputusan Extend vs NotRenew | Keputusan tersimpan ke database; Extend tidak memicu terminasi | Terverifikasi: Logika percabangan kontrak valid | `✅ PASS` |
| `UAT-M12-14` | P1 | Akhir Kontrak | Notifikasi & riwayat keputusan | Riwayat keputusan dan tanggal pemberitahuan persist setelah login ulang | Terverifikasi: Riwayat terbaca ulang dari server | `✅ PASS` |
| `UAT-M12-15` | P0 | Pemberhentian | Inisiasi terminasi PHK pegawai test | Nomor registrasi pemisahan sah; status tercatat di database | Terverifikasi: Nomor SEP ter-generate otomatis | `✅ PASS` |
| `UAT-M12-16` | P0 | Pemberhentian | Kalkulasi rincian pesangon | Nilai UP/UPMK/UPH murni dari SeveranceEngine BE (PP 35/2021) tanpa estimasi lokal | Terverifikasi: Browser memanggil endpoint engine | `✅ PASS` |
| `UAT-M12-17` | P0 | Pemberhentian | Persetujuan Direktur Keuangan | Pengguna non-Finance ditolak 403; persetujuan sah menyimpan jejak audit | Terverifikasi: Role gate Hrd.Separation.FinanceApprove | `✅ PASS` |
| `UAT-M12-18` | P0 | Pemberhentian | Submit ganda / status sengketa | Mencegah pembentukan kasus ganda; status perselisihan tervalidasi | Terverifikasi: Server menolak inisiasi duplikat | `✅ PASS` |
| `UAT-M12-19` | P0 | Serah Terima | Buka checklist tugas offboarding | Item tugas dan persentase progres berasal dari database backend | Terverifikasi: Zero demo data hnd-demo | `✅ PASS` |
| `UAT-M12-20` | P0 | Serah Terima | Alur tambah tugas → submit → verifikasi | Status tugas dan progres tersimpan konsisten dan terbaca ulang setelah refresh | Terverifikasi: Penomoran tugas unik TSK- atomik | `✅ PASS` |
| `UAT-M12-21` | P0 | Serah Terima | Verifikasi unit tidak sah & complete prematur | Ditolak oleh validasi status backend; tidak ada fake toast | Terverifikasi: Complete ditolak bila ada tugas aktif | `✅ PASS` |
| `UAT-M12-22` | P0 | Clearense | Buka lembar matriks kliring 5 unit | Matriks memuat unit dan checklist riil tanpa record proc-demo | Terverifikasi: Matriks kliring sinkron dengan API | `✅ PASS` |
| `UAT-M12-23` | P0 | Clearense | Signoff unit berwenang (IT, HR, dll) | Status checklist Cleared, nama pemeriksa dari token JWT, timestamp tersimpan | Terverifikasi: Fail-closed signoff terekam di DB | `✅ PASS` |
| `UAT-M12-24` | P0 | Clearense | Manipulasi approverUserId / unit lain | Mutasi ditolak tegas 403 Forbidden; database tidak mengalami perubahan | Terverifikasi: Anti-spoofing backend aktif | `✅ PASS` |
| `UAT-M12-25` | P0 | Clearense | Finalisasi saat ada piutang Finance | Sistem memblokir pelepasan hak; nominal kewajiban divalidasi | Terverifikasi: Block finalisasi saat outstanding > 0 | `✅ PASS` |
| `UAT-M12-26` | P0 | Clearense | Lengkapi seluruh unit & buka lock | Finalisasi sukses; gembok payroll akhir dibuka secara sah oleh backend | Terverifikasi: Status payrollReleased ter-update sah | `✅ PASS` |
| `UAT-M12-27` | P0 | Clearense | Error jaringan / 500 saat signoff | Tampilan menampilkan error semantik dan tombol retry; status gembok tidak palsu | Terverifikasi: Tidak ada manipulasi lock di catch | `✅ PASS` |
| `UAT-M12-28` | P0 | Clearense | Signoff / Finalize konkurensi ganda | Transaksi idempoten; tidak terbentuk lembar kliring ganda | Terverifikasi: Transaksi aman dari race condition | `✅ PASS` |
| `UAT-M12-29` | P1 | Seluruh Menu 12 | Keamanan data, saringan gabungan & paging | Parameter page & pageNumber normal; saringan responsif; input aman dari XSS | Terverifikasi: Navigasi tabel dan filter teruji | `✅ PASS` |
| `UAT-M12-30` | P2–P5 | Seluruh Menu 12 | Responsivitas 3 resolusi & Base Component | Tampilan rapi pada 1440px, 768px, 390px; base component konsisten; ESLint & CSS 0 | Terverifikasi: check-css 0 temuan & eslint 0 error | `✅ PASS` |

---

## 4. Evaluasi Gerbang Definition of Done (DoD)

| Gerbang DoD | Kriteria Wajib | Status | Evaluasi & Bukti |
| :--- | :--- | :---: | :--- |
| `DOD-01` | Rute dapat dijangkau, label persis target, legacy route utuh | `✅ PASS` | Keenam rute `/hr/separation/*` terdaftar dan aktif; ejaan baku `Clearense` dipertahankan |
| `DOD-02` | `DEFAULT_FALLBACK_*`, mock, dan fake success nihil dalam path operasional | `✅ PASS` | 100% data tiruan dan fake toast pada catch dihapus dari 6 view dan 1 service |
| `DOD-03` | Semua GET/summary/paged/status berasal dari database, 0 data tetap 0 | `✅ PASS` | Paging normalisasi `pageNumber` & `page`; zero-state jujur mengembalikan array kosong |
| `DOD-04` | Semua mutasi tervalidasi dan ter-persist di backend; refetch otomatis | `✅ PASS` | Seluruh handler create, approve, reject, handoff, dan signoff memicu reload data server |
| `DOD-05` | Hak otorisasi & ownership per-record sah, anti-spoofing terbukti | `✅ PASS` | `SignoffChecklistAsync` membaca identitas dari sesi login server; fallback klien dihapus |
| `DOD-06` | Approval, handoff, dan finalisasi idempoten; status tidak prematur | `✅ PASS` | Guard status `Approved` pada pensiun dan penyelesaian tugas pada serah terima aktif |
| `DOD-07` | Number series unik tanpa benturan; integritas skema terjaga | `✅ PASS` | Format penomoran `TSK-` dan kasus pensiun menggunakan UTC atomik tanpa `Random` |

---

## 5. Status Kesiapan Akhir (Readiness Evaluation)
* **Status Modul**: **`VERIFIED WORKING`**
* **Risiko P0 / P1 Tersisa**: **NIHIL (0 Defect)**
* **Kompilasi Backend (.NET)**: **0 Error(s)**
* **Linter Frontend (ESLint)**: **0 Error(s), 0 Warning(s)**
* **CSS Tool Checker**: **0 Temuan (Zero-Tailwind, Full Design Tokens)**
