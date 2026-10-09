# Laporan Perubahan Frontend — `FE-RWI-208`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-208` |
| Judul | Perbaikan tampilan, kontrol input, dan sinkronisasi tab Pemesanan Ruangan Bedah FE-INP-25 |
| Slice | Perubahan antarmuka pasca-pengujian (UI/UX Refinement); bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/frontend-roadmap-v2.md` |
| Trace | `ISSUE-EPS-004`; `PLAN-REPAIR-EPS-004` (`FIX-EPS-004-01` s.d. `FIX-EPS-004-06`); Keputusan Pemilik K-01 (Pilihan A) & K-02 (Pilihan A) disetujui 7 Oktober 2026 |
| Contract version | `0.10.0` — tidak disentuh (perbaikan internal komponen frontend `FE-INP-25`) |
| Dependency | Ruang Kerja Keperawatan Rawat Inap (`FE-KEP-07` / `FE-INP-25`), context tab sekunder |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/` (`surgery-booking-section.jsx`, `surgery-booking-form.jsx`, `patient-surgery-cases-table.jsx`); Blueprint: `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Wewenang UI | `PLAN-REPAIR-EPS-004` (keputusan pemilik K-01 dan K-02 disetujui 7 Oktober 2026) |
| Model | Claude Opus 5.5 |
| Tanggal | 7 Oktober 2026 |
| Status | Selesai. Verifikasi manual di peramban `NOT RUN` (sesuai kebijakan pemilik) |

---

## 1. Masalah yang diperbaiki

Berdasarkan audit dan diagnosa masalah pada dokumen `issue-004-pemesanan-ruangan-bedah.md`:

1. **Tab Navigasi Ganda dan Desinkronisasi (`ISS-EPS-004-01`)**:
   Terdapat dua tingkatan navigasi tab untuk memilih jenis bedah: Secondary Tab Bar di atas ruang kerja dan tombol tab manual di dalam kartu formulir. Ketika perawat beralih ke sub-tab "Bedah Obgyn" di bilah atas, formulir di dalam kartu tetap berada pada pilihan "Bedah Operasi", membingungkan pengguna dan berpotensi salah mencatat kamar operasi.
2. **Kontrol Radio Button Berdesakan (`ISS-EPS-004-02`)**:
   Pilihan Jenis Kasus (Elektif, Cito, Emergency, dsb.) dan Sisi Tubuh (Kiri, Kanan, Bilateral) menggunakan radio button native peramban berukuran kecil tanpa pembungkus pill, dengan teks label berhimpitan dan sulit ditekan pada layar sentuh/tablet perawat.
3. **Formulir Kosong dan Buntu Saat Belum Ada Order Dokter (`ISS-EPS-004-03`)**:
   Bila pasien rawat inap belum memiliki order tindakan operasi dokter DPJP (`procedures.length === 0`), formulir pemesanan tetap terbuka namun dropdown tindakan kosong, tombol simpan mati, dan tidak ada kejelasan langkah klinis apa yang harus diambil perawat.
4. **Validasi Pasif / Disabled Button (`ISS-EPS-004-04`)**:
   Tombol submit "Pesan Ruang Bedah" dalam kondisi disabled bila form belum lengkap, tanpa indikator visual isian mana yang belum terpenuhi.
5. **Formulir Terpotong Footer Aplikasi (`ISS-EPS-004-05`)**:
   Formulir berada di bagian bawah halaman tanpa bantalan bawah (*safe padding*) yang cukup, sehingga tombol submit dan catatan terancam tertutup oleh floating action footer atau bilah status peramban.
6. **Inkonsistensi Komponen & Spasi Teks (`ISS-EPS-004-06`)**:
   Teks dokter operator tidak memiliki spasi pemisah rapi dan tombol muat ulang tabel riwayat kasus menggunakan tombol HTML biasa tanpa memanfaatkan `BaseButton`.

---

## 2. Proses bisnis

Alur proses bisnis pemesanan ruangan bedah rawat inap kini berjalan sebagai berikut:

1. **Pemilihan Mode Layanan Lewat Tab Sekunder Ruang Kerja**:
   Perawat membuka menu *Pemesanan Ruangan Bedah* pada tab sekunder ruang kerja keperawatan. Pilihan sub-tab di bilah sekunder ("Bedah Operasi" atau "Bedah Obgyn") menjadi kendali navigasi tunggal (*single source of truth*). Tombol tab ganda di dalam formulir telah dihilangkan dan digantikan oleh badge status mode layanan aktif ("Mode: Bedah Operasi" atau "Mode: Bedah Obgyn (Kasus Obstetri)").
2. **Penguncian Otomatis Kasus Obgyn**:
   Bila sub-tab "Bedah Obgyn" dipilih, sistem otomatis mengunci nilai `caseType` ke `Obstetri` dan menampilkan badge konfirmasi kategori kebidanan/kandungan.
3. **Pemeriksaan Ketersediaan Order Dokter (Clinical Guardrail)**:
   - Jika dokter DPJP **belum menerbitkan order tindakan operasi** (`procedures.length === 0`), formulir pemesanan buntu disembunyikan. Sistem menyajikan panel edukasi alur klinis yang ramah pengguna, menginformasikan bahwa tindakan operasi wajib diinput oleh Dokter DPJP melalui form CPPT / Order Tindakan terlebih dahulu, lengkap dengan tombol interaktif `[ Segarkan Daftar Order ]`.
   - Jika order tindakan **sudah tersedia**, formulir pemesanan terbuka dengan daftar tindakan resmi dari dokter DPJP.
4. **Pengisian Formulir yang Terstruktur & Ergonomis**:
   - Pilihan Jenis Kasus dan Sisi Tubuh (Laterality) disajikan dalam bentuk *Radio Pills* dengan batas klik lega, kontras warna jelas, dan transisi halus.
   - Kolom Tanggal Operasi, Jam Operasi, dan Estimasi Durasi (menit) ditata dalam grid 2-kolom yang proporsional.
   - Kolom Diagnosa Pra-Bedah dan Catatan Khusus menggunakan textarea bertinggi optimal (`rows={3}`).
5. **Validasi Interaktif & Panduan Kesalahan**:
   - Tombol "Pesan Ruang Bedah" selalu aktif dan siap ditekan perawat.
   - Jika tombol ditekan saat ada data wajib yang belum diisi, sistem menampilkan pesan peringatan inline merah dan secara otomatis memindahkan kursor (*auto-focus*) langsung ke elemen isian pertama yang belum lengkap.
6. **Bantalan Bawah Aman (Safe Area)**:
   Kontainer pemesanan memiliki safe bottom padding (`pb-28`) sehingga seluruh formulir dan tombol submit tetap berada di atas bilah footer aplikasi tanpa risiko terhalang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-section.jsx` | 1. Mengakses `activeTab` dan `setActiveTab` dari `useNursingWorkspaceContext()`.<br>2. Menetapkan `currentTab` turunan untuk sinkronisasi sub-tab.<br>3. Menambahkan logika otomatisasi pada fungsi *rebook* agar beralih ke sub-tab Obgyn jika jenis kasus adalah Obstetri.<br>4. Menambahkan kelas utilitas Tailwind `pb-28` pada kontainer utama untuk menjamin safe padding bawah. |
| `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-form.jsx` | 1. Mengeliminasi tombol navigasi tab ganda lokal di dalam header kartu.<br>2. Menambahkan `Badge` indikator mode layanan aktif.<br>3. Mengimplementasikan guardrail klinis saat `procedures.length === 0`: menyembunyikan form buntu dan menampilkan panel edukasi klinis lengkap dengan tombol `[ Segarkan Daftar Order ]`.<br>4. Merombak pilihan radio Jenis Kasus dan Sisi Tubuh menjadi komponen *Radio Pills* dengan batas klik lega.<br>5. Menata grid input 2-kolom untuk tanggal, waktu, durasi, dan catatan.<br>6. Menghubungkan tombol submit ke `BaseButton` dengan validasi interaktif inline serta *auto-focus* ke input yang belum lengkap.<br>7. Mempertahankan seluruh teks literal penting untuk validasi unit test. |
| `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/patient-surgery-cases-table.jsx` | 1. Memperbarui tombol "Muat ulang" riwayat kasus bedah menggunakan `BaseButton` varian `secondary`.<br>2. Menambahkan spasi pemisah yang rapi pada label dokter operator. |

### 3.2 Radius dampak

| Bagian | Dampak |
| --- | --- |
| Menu Pemesanan Ruangan Bedah (`FE-INP-25`) | Seluruh perbaikan tampilan, kontrol form, dan alur tombol submit terisolasi dengan aman pada sub-seksi ini. |
| Ruang Kerja Keperawatan Rawat Inap (`FE-KEP-07`) | Navigasi sub-tab atas kini langsung mengendalikan tampilan pemesanan bedah tanpa konflik status internal. |
| Modul Lain | Tidak ada dampak pada modul lain di luar rawat inap. |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — payload yang dikirim ke endpoint booking bedah tetap identik dengan skema kontrak yang ada. |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Tabel keputusan base component

| Elemen | Status | Bukti Penggunaan |
| --- | --- | --- |
| Tombol Simpan Form & Segarkan Order | `REUSE` | `BaseButton` (varian `primary` dan `secondary`) |
| Tombol Muat Ulang Tabel Riwayat | `REUSE` | `BaseButton` (varian `secondary`, ukuran `sm`) |
| Indikator Mode Layanan | `REUSE` | `Badge` (varian `primary` dan `info`) |
| Panel Edukasi Alur Klinis | `REUSE` | `Alert` / `InformationAlert` (varian `warning` dan `info`) |
| Kontrol Radio Pills | `COMPOSE` | Komposisi token Tailwind standar (`rounded-lg border px-3 py-2 text-xs font-semibold`) tanpa membuat file custom component baru di luar folder |

`UI GATE: PASS` — seluruh komponen memanfaatkan token dan base component resmi Quilvian.

---

## 5. Verifikasi

| Skenario atau Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Unit test `surgery-booking.test.mjs` (`node --test tests/unit/surgery-booking.test.mjs`) | 5 dari 5 test lulus (tampilan sub-tab tunggal, guardrail order kosong, pemisahan error vs empty state, penghapusan Scheduled fiktif, verifikasi tarif, status rawat inap). | `PASS` |
| Suite unit keperawatan (`node --test tests/unit/inpatient-nursing-transfer-and-unavailable.test.mjs tests/unit/inpatient-nursing-workspace-v2.test.mjs tests/unit/inpatient-nursing-finishing-roadmap.test.mjs`) | 21 dari 21 test lulus. | `PASS` |
| ESLint sub-modul (`cmd /c npx eslint src/.../surgery-booking/`) | 0 errors. | `PASS` |
| Standalone Build Next.js (`npm run build`) | Build Next.js sukses tanpa error. | `PASS` |
| Grep anti-regresi UI pada baris baru | Nol warna literal terlarang, nol `!important`, nol inline style kotor. | `PASS` |
| Uji coba interaktif di peramban | Dikecualikan sesuai kebijakan pengujian (*Browser verification NOT RUN*). | `NOT RUN` |

---

## 6. Kesimpulan & Penutupan Task

Task `FE-RWI-208` telah berhasil diselesaikan dengan memenuhi seluruh kriteria penerimaan AC-01 sampai AC-23. Seluruh tampilan pada sub-menu Pemesanan Ruangan Bedah (Bedah Operasi dan Bedah Obgyn) kini tampil bersih, terstandar, ergonomis, bebas dari tab ganda paralel, memiliki pemisahan tegas antara loading, error, empty, dan ready state, selaras dengan kontrak backend Planned/Ordered tanpa status Scheduled fiktif, dan terintegrasi mulus dengan panduan tata kelola UI/UX Quilvian.
