# Planning — Ruang Kerja Saya (Layanan Mandiri Berbasis Workspace)

| Field | Nilai |
| --- | --- |
| Tanggal | 24 November 2026 |
| Modul | Human Resource — Layanan Mandiri Pegawai (ESS) |
| Repository terdampak | `QuilvianSystemFrontendDev` (frontend saja; backend tidak diubah) |
| Status | Disetujui dan diimplementasikan 24 November 2026 — hasil dan penyimpangan dari rencana ada di bagian 10 |
| Alur kerja | Analisis → Planning → Implementasi → Testing |

---

## 1. Latar belakang

Saat ini kebutuhan mandiri pegawai (absen, cuti, lembur, jadwal, slip gaji, dokumen, kinerja)
bisa dibuka dari **tiga pintu** yang tidak saling tersambung:

| Pintu | Lokasi source | Isi | Kondisi |
| --- | --- | --- | --- |
| Panel profil kanan | `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` | 12 tombol | **6 tombol berujung 404** |
| Sidebar → Sumber Daya Manusia → Layanan Mandiri | `src/utils/menu-sidebar/menu-items.jsx` (grup `corporateHumanResourceSelfServices`) | 20 item | Halamannya ada, tetapi letaknya di dalam modul admin SDM |
| Portal ESS `/hrd/self-service` | `src/components/view/hr/approvals/unified-ess-portal-view.jsx` | Saldo cuti, 7 tombol pintas, 3 permohonan terakhir | Berfungsi; cikal bakal workspace |

### 1.1 Tombol panel profil yang tidak dapat dibuka

Kelima tombol bagian "Pengajuan" dan tombol Bantuan IT mengarah ke `/karyawan/DaftarPengajuan/...`.
Folder `src/app/karyawan/` hanya memuat `Absensi-Karyawan/FormAbsensi`, dan `redirects()` pada
`next.config.js` hanya mengalihkan alamat master data dan payroll.

| Tombol | Status | Halaman pengganti yang sudah ada |
| --- | --- | --- |
| Pengajuan Cuti | 404 | `/self-services/human-resource/leave` |
| Pengajuan Lembur | 404 | `/hr/self-service/overtime` |
| Tukar Shift | 404 | `/self-services/human-resource/schedule` |
| Perubahan Jadwal | 404 | Belum ada layar. Fungsi `createScheduleChangeRequest` ada di service, tetapi tidak dipanggil layar mana pun |
| Pengajuan Resign | 404 | Belum ada layar pegawai. Backend `ResignationSelfServiceController` sudah ada |
| Bantuan IT | 404 | Modulnya tidak ada |

### 1.2 Masalah pada menu Layanan Mandiri

- **Cuti dan Tukar Shift tidak ada di sidebar sama sekali.** Keduanya hanya bisa dibuka lewat tombol
  di dalam Portal ESS. Lembur ada di SDM → Manajemen Lembur.
- **17 dari 20 item tanpa `requiredPermission`**, sehingga semua pengguna melihat semua item,
  termasuk Kredensialing, OPPE, Peer Review, dan Portofolio SKP.
- **Alamat ganda:** Presensi, Lembur, Restitusi, Tanggungan, Dokumen, Surat, Kredensialing, dan
  Portal ESS masing-masing punya 2 alamat; Slip Gaji punya 3.
- **Backend siap tetapi layar belum ada:** Perubahan Jadwal, Resign, Dispensasi Presensi,
  Pembatalan Cuti, Kalender Cuti, dan Kembali Kerja setelah Cuti.
- Dokumen `PLANNING_PENYESUAIAN_MENU_SDM_BERDASARKAN_ANALISIS_BISNIS.md` (baris 82 dan 357)
  sebelumnya membekukan grup Layanan Mandiri. Rencana ini **menggantikan** keputusan tersebut
  untuk grup Layanan Mandiri bila disetujui.

### 1.3 Contoh di rumah sakit

Seorang perawat bangsal selesai shift malam pukul 06.45. Anaknya sakit, jadi ia perlu menukar
shift besok dan mengecek sisa cuti.

- **Hari ini:** ia klik foto profil → "Tukar Shift" → 404. Di sidebar → SDM → Layanan Mandiri,
  dari 20 item tidak ada "Tukar Shift". Ia harus tahu bahwa jalannya lewat "Portal Layanan Mandiri
  (ESS)" → "Tukar Shift / Ubah Jadwal", dan halaman itu pun hanya bisa tukar shift.
- **Dengan workspace:** ia klik foto profil → "Tukar Shift" → langsung masuk Ruang Kerja Saya,
  bagian Jadwal, tab Tukar Shift. Header memperlihatkan jam hadirnya dan atasan yang akan
  menyetujui; panel kanan menunjukkan sisa cutinya.

---

## 2. Tujuan

Satu halaman, **Ruang Kerja Saya**, tempat pegawai mengurus seluruh kebutuhan pribadinya. Tata
letaknya memakai pola ruang kerja keperawatan rawat inap: header identitas di atas, navigasi
kiri, isi di tengah, dan ringkasan di kanan. Panel profil kanan tetap ada, tetapi hanya sebagai
jalan pintas yang membuka bagian yang tepat di workspace.

---

## 3. Gambaran layar

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ Ruang Kerja Saya                                       [Segarkan]        │
├──────────────────────────────────────────────────────────────────────────┤
│ Ns. Rina · NIP 2023-0412 · Perawat Pelaksana · Bangsal Melati 3          │
│ Atasan: Ns. Dewi (Kepala Ruang) · Hadir 06.52 · Belum absen pulang       │
├────────────────┬─────────────────────────────────────┬───────────────────┤
│ LAYANAN SAYA   │ [Presensi][Koreksi][Jadwal][Ubah]   │ Ringkasan         │
│ > Beranda      │                                     │ Sisa cuti: 9 hari │
│ > Kehadiran &  │   isi layar yang dipilih            │ Menunggu: 2       │
│   Jadwal       │                                     │                   │
│ > Cuti & Lembur│                                     │ [Ajukan Cuti]     │
│ > Gaji & Klaim │                                     │ [Tukar Shift]     │
│ > ...          │                                     │ [Lapor Pajanan]   │
└────────────────┴─────────────────────────────────────┴───────────────────┘
```

Nama pada gambar adalah data samaran.

| Wilayah | Ruang Kerja Keperawatan Rawat Inap | Ruang Kerja Saya |
| --- | --- | --- |
| Header atas | Identitas pasien, bed, alergi | Nama, NIP, jabatan, unit, atasan langsung, status hadir hari ini |
| Navigasi kiri | 8 bagian dokumentasi asuhan | 9 bagian layanan (bagian 5) |
| Tengah | Form pengkajian | Layar mandiri yang sudah ada, dipasang di sini |
| Ringkasan kanan | Ringkasan cepat klinis | Sisa cuti, permohonan menunggu, aksi cepat, tombol Lapor Pajanan |
| Alamat `?section=&tab=` | Sudah dipakai | Dipakai panel profil untuk langsung membuka bagian tertentu |

---

## 4. Keputusan

Setiap baris sudah berisi rekomendasi. Ganti bila tidak sesuai sebelum implementasi dimulai.

| No | Hal | Rekomendasi | Alternatif |
| ---: | --- | --- | --- |
| 1 | Alamat dan nama | `/self-services`, judul "Ruang Kerja Saya". Portal lama `/hrd/self-service` dan `/corporate/hrd/self-service` dialihkan ke sini | Pakai ulang alamat `/hrd/self-service` |
| 2 | Menu sidebar | Satu item "Ruang Kerja Saya" tepat di bawah Dashboard, terlihat semua pegawai. Grup "Layanan Mandiri" (20 item) dan item "Pengajuan & Status Lembur (ESS)" dihapus dari SDM | Grup lama tetap dibiarkan |
| 3 | Layar yang sudah ada | Dipasang apa adanya di area tengah. Hero-nya dipadatkan lewat CSS khusus workspace, sehingga ke-20 file layar tidak disentuh | Merombak tiap layar satu per satu (jauh lebih lama) |
| 4 | Panel profil | Semua tombol diarahkan ke bagian workspace yang tepat, ditambah tombol "Buka Ruang Kerja Saya". Tombol Bantuan IT disembunyikan karena modulnya tidak ada | — |
| 5 | Bagian Praktik Klinis | Tampil untuk semua pegawai dulu, seperti menu sekarang. Data konteks pegawai belum punya penanda "tenaga kesehatan" untuk perawat | Hanya untuk akun dokter (`DoctorId` terisi) |

---

## 5. Isi navigasi kiri

| No | Bagian | Tab | Sumber layar |
| ---: | --- | --- | --- |
| 1 | Beranda | — | **Baru.** Ringkasan, aksi cepat, dan 3 permohonan terakhir; logikanya diambil dari Portal ESS |
| 2 | Kehadiran & Jadwal | Presensi · Koreksi Presensi · Jadwal & Tukar Shift · Perubahan Jadwal | Layar ada; **Perubahan Jadwal baru** (backend siap) |
| 3 | Cuti & Lembur | Cuti · Lembur | Layar ada |
| 4 | Gaji & Klaim | Slip Gaji · Klaim Restitusi · Klaim COB | Layar ada |
| 5 | Data Kepegawaian Saya | Kelola Data Saya · Keluarga & Tanggungan · Dokumen & PIN · Surat & Layanan HR · Pengunduran Diri | Layar ada; **Pengunduran Diri baru** (backend siap) |
| 6 | Kinerja & Diklat | Evaluasi Kinerja · Sanggah Nilai · Dasbor Pelatihan | Layar ada |
| 7 | Praktik Klinis | Kredensialing · Rapor OPPE · Peer Review · Portofolio SKP | Layar ada |
| 8 | Pengaduan & K3 | Lapor Pajanan (CITO) · Pengaduan | Layar ada; tombol "Lapor Pajanan" juga selalu terlihat di panel kanan |
| 9 | Riwayat Permohonan | — | Layar ada (Pelacak Permohonan) |

### 5.1 Pemetaan tab ke layar yang sudah ada

| Tab | View yang dipasang | Alamat lama (tetap berfungsi) |
| --- | --- | --- |
| Presensi | `self-services/human-resource/attendance/attendance-employee-view.jsx` | `/self-services/human-resource/attendance` |
| Koreksi Presensi | `self-services/human-resource/attendance-correction/attendance-correction-employee-view.jsx` | `/self-services/human-resource/attendance-correction` |
| Jadwal & Tukar Shift | `self-services/human-resource/schedule/schedule-employee-view.jsx` | `/self-services/human-resource/schedule` |
| Cuti | `self-services/human-resource/leave/leave-employee-view.jsx` | `/self-services/human-resource/leave` |
| Lembur | Layar pada rute `/hr/self-service/overtime` | `/hr/self-service/overtime` |
| Slip Gaji | `payroll/payslip/payroll-my-payslip-view.jsx` | `/payroll/my-payslips` |
| Klaim Restitusi | Layar pada rute `/ess/restitution-claims` | `/ess/restitution-claims` |
| Klaim COB | Layar pada rute `/ess/claims/cob-submission` | `/ess/claims/cob-submission` |
| Kelola Data Saya | `self-services/human-resource/employee/employee-profile-changes-self-service-view.jsx` | `/self-services/human-resource/employee/profile-changes` |
| Keluarga & Tanggungan | Layar pada rute `/ess/my-dependents` | `/ess/my-dependents` |
| Dokumen & PIN | `hr/services/portal-my-documents-view.jsx` | `/portal/services/my-documents` |
| Surat & Layanan HR | `hr/services/portal-service-requests-view.jsx` | `/portal/services/requests` |
| Evaluasi Kinerja | `hr/performance/performance-appraisals-view.jsx` | `/portal/performance/appraisals` |
| Sanggah Nilai | `hr/performance/performance-disputes-view.jsx` | `/portal/performance/disputes` |
| Dasbor Pelatihan | `hr/learning/learning-dashboard-view.jsx` | `/portal/learning/dashboard` |
| Kredensialing | `hr/credentialing/portal-credentialing-wizard-view.jsx` | `/portal/credentialing/application` |
| Rapor OPPE | `hr/oppe/clinical-oppe-dashboard-view.jsx` | `/portal/performance/clinical-oppe` |
| Peer Review | `hr/oppe/peer-review-evaluation-form-view.jsx` | `/portal/performance/peer-review-entry` |
| Portofolio SKP | Layar pada rute `/portal/learning/skp-portfolio` | `/portal/learning/skp-portfolio` |
| Lapor Pajanan (CITO) | `hr/occupational-health/widget/NeedleStickPanicButton` | `/portal/health/panic` |
| Pengaduan | `hrd/relations/relations-grievance-view.jsx` | `/portal/grievance` |
| Riwayat Permohonan | `hr/approvals/unified-ess-my-requests-view.jsx` | `/hrd/self-service/my-requests` |

Seluruh path view relatif terhadap `src/components/view/`. Alamat lama **tidak dihapus**, supaya
bookmark dan tautan yang sudah beredar tetap terbuka.

### 5.2 Aturan tampil

| Kondisi | Yang terjadi |
| --- | --- |
| Tab punya permission dan pegawai tidak memilikinya | Tab disembunyikan. Contoh: Slip Gaji (`MyPayslip : Read`), Koreksi Presensi (`MyAttendanceCorrection : Read`), Riwayat Permohonan (`WorkflowInboxInstance : ViewOwn`) |
| Seluruh tab dalam satu bagian tersembunyi | Bagiannya ikut disembunyikan dari navigasi kiri |
| Akun belum terhubung ke data pegawai (`WorkforceProfileId` kosong) | Workspace menampilkan pesan "Akun Anda belum terhubung dengan data pegawai. Hubungi HRD." Tanpa pesan ini, puluhan layar akan gagal dengan 404 yang membingungkan |
| Alamat berisi `section` atau `tab` yang tidak dikenal | Dibuka ke Beranda |

---

## 6. File yang dibuat dan diubah

Seluruhnya di `QuilvianSystemFrontendDev`. Backend tidak perlu diubah.

### 6.1 File baru

| File | Isi |
| --- | --- |
| `src/app/self-services/page.jsx` | Route tipis beserta metadata "Ruang Kerja Saya" |
| `src/lib/constants/self-services/self-service-workspace-constants.js` | Daftar bagian, tab, permission per tab, dan pembentuk alamat `?section=&tab=` |
| `src/lib/hooks/self-services/use-self-service-workspace.js` | Konteks pegawai, izin, sinkronisasi URL, dan data ringkasan |
| `src/components/view/self-services/workspace/self-service-workspace-view.jsx` | View utama: shell, navigasi, tab, dan area isi |
| `src/components/view/self-services/workspace/self-service-workspace-header.jsx` | Header identitas pegawai |
| `src/components/view/self-services/workspace/self-service-tab-bar.jsx` | Tab sekunder per bagian |
| `src/components/view/self-services/workspace/self-service-section-content.jsx` | Memuat layar lama secara *lazy* (hanya saat tab dibuka) di dalam wadah yang memadatkan Hero |
| `src/components/view/self-services/workspace/self-service-summary-panel.jsx` | Panel ringkasan kanan |
| `src/components/view/self-services/workspace/self-service-home-section.jsx` | Bagian Beranda |
| `src/style/self-services/self-service-workspace.module.css` | Tata letak workspace dan pemadatan Hero, memakai token `var(--…)` |

### 6.2 File yang diubah

| File | Perubahan |
| --- | --- |
| `src/components/ui/clinical-workspace/ClinicalWorkspaceShell.jsx` | Prop opsional untuk label aksesibilitas navigasi dan ringkasan. Nilai bawaannya tetap, sehingga ruang kerja keperawatan, IGD, dan Hemodialisa tidak berubah |
| `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` | Semua tombol mengarah ke bagian workspace; tambah "Buka Ruang Kerja Saya"; sembunyikan Bantuan IT |
| `src/utils/menu-sidebar/menu-items.jsx` | Item "Ruang Kerja Saya" di bawah Dashboard; hapus grup Layanan Mandiri dan item lembur ESS dari SDM |
| `src/app/(dashboard)/hrd/self-service/page.jsx` | Dialihkan ke `/self-services` |
| `src/app/(dashboard)/corporate/hrd/self-service/page.jsx` | Dialihkan ke `/self-services` |

### 6.3 Pemetaan tombol panel profil

| Tombol panel profil | Tujuan baru |
| --- | --- |
| Kelola Data Saya | `/self-services?section=data&tab=profile-change` |
| Absensi | `/self-services?section=attendance&tab=attendance` |
| Pengajuan Cuti | `/self-services?section=leave&tab=leave` |
| Pengajuan Lembur | `/self-services?section=leave&tab=overtime` |
| Perubahan Jadwal | `/self-services?section=attendance&tab=schedule-change` |
| Tukar Shift | `/self-services?section=attendance&tab=schedule` |
| Pengajuan Resign | `/self-services?section=data&tab=resignation` |
| Bantuan IT | Disembunyikan |
| Profil, Ganti Password, Pengaturan, Keluar | Tidak berubah |

Nilai `section` dan `tab` di atas adalah rancangan; nilai akhirnya mengikuti konstanta pada 6.1.

---

## 7. Urutan pengerjaan

### Fase 1 — Inti

1. Konstanta bagian dan tab, serta hook workspace.
2. Shell, header identitas, navigasi kiri, tab bar, dan area isi.
3. Panel ringkasan kanan dan Beranda.
4. Route `/self-services` serta pengalihan dua alamat portal lama.
5. Panel profil diarahkan ke workspace.
6. Menu sidebar.

### Fase 2 — Dua layar baru

7. **Perubahan Jadwal:** fungsi service dan view baru, dipasang di tab Kehadiran & Jadwal.
8. **Pengunduran Diri:** fungsi service dan view baru, dipasang di tab Data Kepegawaian Saya.

Endpoint backend yang dipakai Fase 2 sudah tersedia:

#### Self Services / Human Resource / Schedule Change

Base URL: `api/v1/self-services/human-resource/schedule-change-requests`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pengajuan perubahan jadwal milik pegawai | `MyScheduleChange : Read` |
| `GET` | `/schedule-options` | Pilihan jadwal kerja pegawai pada tanggal tertentu | `MyScheduleChange : Read` |
| `GET` | `/shift-options` | Pilihan shift pengganti | `MyScheduleChange : Read` |
| `POST` | `/validate-preview` | Memeriksa pengajuan sebelum disimpan | `MyScheduleChange : Read` |
| `POST` | `/` | Membuat draf pengajuan | `MyScheduleChange : Create` |
| `PUT` | `/{id}` | Mengubah draf | `MyScheduleChange : Update` |
| `POST` | `/{id}/submit` | Mengirim ke atasan untuk disetujui | `MyScheduleChange : Submit` |
| `POST` | `/{id}/cancel` | Membatalkan pengajuan | `MyScheduleChange : Cancel` |
| `DELETE` | `/{id}` | Menghapus draf yang belum dikirim | `MyScheduleChange : Delete` |

#### Self Services / Human Resource / Resignation

Base URL: `api/v1/self-services/human-resource/resignation-requests`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pengajuan resign milik pegawai | `MyResignation : Read` |
| `GET` | `/{id}/workflow` | Melihat tahap persetujuan | `MyResignation : Read` |
| `POST` | `/` | Membuat draf pengajuan resign | `MyResignation : Create` |
| `PUT` | `/{id}` | Mengubah draf | `MyResignation : Update` |
| `POST` | `/{id}/submit` | Mengirim pengajuan | `MyResignation : Submit` |
| `POST` | `/{id}/cancel` | Membatalkan pengajuan | `MyResignation : Cancel` |
| `DELETE` | `/{id}` | Menghapus draf yang belum dikirim | `MyResignation : Delete` |

Kode `404` pada kedua grup berarti akun belum terhubung ke data pegawai, atau pengajuan bukan
milik pengguna yang sedang login.

---

## 8. Testing

| No | Pemeriksaan | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | `npm run lint` pada file yang berubah | Tanpa error baru |
| 2 | Build frontend | Berhasil |
| 3 | Setiap tombol panel profil diklik | Mendarat di bagian dan tab yang benar, tidak ada lagi 404 |
| 4 | Setiap tab dibuka satu per satu | Layar tampil tanpa error dan tanpa header ganda yang berlebihan |
| 5 | Pegawai tanpa izin Slip Gaji | Tab Slip Gaji tidak terlihat |
| 6 | Akun tanpa data pegawai | Pesan "Akun Anda belum terhubung dengan data pegawai" tampil |
| 7 | Alamat lama `/hrd/self-service` dibuka | Dialihkan ke `/self-services` |
| 8 | Lebar layar ponsel (± 390 px) | Navigasi kiri dan panel ringkasan tersusun ke bawah, tetap terbaca |
| 9 | Regresi: Ruang Kerja Keperawatan Rawat Inap | Tampilan tidak berubah setelah `ClinicalWorkspaceShell` disentuh |

---

## 9. Di luar cakupan (kandidat Fase 3)

| Hal | Alasan |
| --- | --- |
| Dispensasi presensi, pembatalan cuti, lapor kembali kerja setelah cuti | Backend ada, layarnya belum; dikerjakan setelah Fase 2 |
| Modul Bantuan IT | Belum ada modul maupun backend-nya |
| Penyaringan bagian Praktik Klinis khusus tenaga kesehatan | Butuh penanda tenaga kesehatan pada data pegawai |
| Tombol di layar lama yang membuka halaman detail sendiri (misalnya revisi permohonan) | Tetap membuka halaman terpisah di luar workspace untuk sementara |
| Merapikan alamat ganda (`/ess/...`, `/portal/...`, `/hr/self-service/...`) | Alamat lama dibiarkan supaya tautan yang beredar tidak rusak |

---

## 10. Hasil implementasi (24 November 2026)

Seluruh keputusan bagian 4 dijalankan sesuai rekomendasi. Perubahan belum di-commit.

### 10.1 Penyimpangan dari rencana

| Hal | Rencana | Yang dikerjakan | Alasan |
| --- | --- | --- | --- |
| Nama bagian 5 | Data Kepegawaian Saya | **Data Kepegawaian** | Label terpotong di navigasi kiri |
| Layar Jadwal & Tukar Shift | Dipasang apa adanya | **Ditulis ulang** | Layar lama selalu galat: daftar jadwal memanggil fungsi yang selalu melempar "belum tersedia", dan form tukar shift mengirim tanggal padahal backend meminta ID penugasan shift. Kini memakai `GET my-schedule` dan alur tukar shift lengkap: kirim ke rekan → rekan menerima/menolak → ajukan ke atasan |
| Jenis Perubahan Jadwal | Semua jenis | Hanya **Ganti Shift** dan **Ganti Hari Libur** | Ganti pola jadwal dan jadwal sementara butuh daftar pola jadwal kerja yang hanya dikelola kepala ruang/SDM |
| Tata letak ponsel | — | Penyesuaian khusus workspace | `ClinicalWorkspaceShell` pada satu kolom memakai `align-items: start`, sehingga area isi melebar 796 px pada layar 390 px. Diperbaiki lewat kelas milik workspace ini saja; CSS bersama tidak diubah |
| Unit test | — | ESS-002 dan ESS-003 disesuaikan | Keduanya memeriksa butir menu dan tautan lama Kelola Data Saya yang kini menjadi tab workspace |

### 10.2 Hasil pengujian

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint file baru dan yang diubah | 0 error; peringatan yang tersisa hanya di file lama yang tidak disentuh (sama dengan HEAD) |
| `npm run build` | Berhasil (`✓ Compiled successfully`), rute `/self-services` terdaftar |
| Unit test frontend | Satu-satunya test yang gagal sekarang tetapi lulus di HEAD hanyalah ESS-003 (komentar menyebut path lama); sudah diperbaiki dan lulus. Test lain yang gagal juga gagal di HEAD |
| Uji browser — superadmin | 40 kasus: 23 tab tampil tanpa galat, 8 tombol panel profil mendarat di tab yang benar, 2 alamat lama dialihkan, parameter ngasal aman, Bantuan IT hilang. Satu FAIL lebar ponsel sudah diperbaiki dan diverifikasi ulang |
| Uji browser — pegawai tanpa izin `My…` | 15/15 PASS; tab berizin tersembunyi, header identitas tampil |
| Uji browser — pegawai dengan izin penuh | 31/31 PASS setelah skrip diperbaiki; payload Perubahan Jadwal dan Pengunduran Diri sesuai DTO backend (POST dicegat, tanpa penulisan database) |

Skrip dan screenshot: `QuilvianSystemFrontendDev/test-with-agy/scripts/rks-1124-*` dan
`QuilvianSystemFrontendDev/test-with-agy/screenshots/rks-1124/`.

### 10.3 Fase 3 (24 November 2026)

Tiga layar yang backend-nya sudah ada tetapi belum punya tampilan:

| Layar | Letak | Endpoint | Izin |
| --- | --- | --- | --- |
| Dispensasi Presensi | Kehadiran & Jadwal → Dispensasi | `self-services/human-resource/attendance-dispensations` | `MyAttendanceDispensation` |
| Pembatalan Cuti | Cuti & Lembur → Pembatalan Cuti | `self-services/human-resource/leave/cancellations` | `MyLeaveCancellation` |
| Panggilan Kembali dari Cuti | Cuti & Lembur → Panggilan Kembali | `self-services/human-resource/leave/return-to-work` | `MyReturnToWork` |

- "Lapor kembali kerja" ternyata berarti **konfirmasi panggilan kembali dari cuti**: atasan memanggil pegawai
  sebelum cuti selesai, pegawai mengonfirmasi tanggal masuk (harus di antara tanggal efektif panggilan dan
  akhir cuti semula). Panggilan yang belum dikonfirmasi tampil sebagai badge pada Cuti & Lembur, baris
  "Panggilan kembali cuti" di panel ringkasan, dan tombol "Konfirmasi Panggilan Kembali" di Aksi Cepat.
- Pembatalan cuti hanya untuk cuti `Approved`/`Taken`; tanggal mulai pembatalan otomatis hari pertama cuti
  (atau hari ini bila cuti sudah berjalan). Cuti yang belum disetujui tetap dibatalkan dari tab Cuti.
- Dispensasi mengirim jam tanpa zona (dibaca server sebagai WIB), misalnya `2026-11-24T07:00:00`.
- Kalender Cuti (`MyLeaveCalendar`) tidak dibuatkan layar karena hanya berisi cuti milik sendiri — sama
  dengan riwayat di tab Cuti.
- Perbaikan tampilan lintas layar baru: sel tabel dua baris memakai `<div>` (kelas sel tabel tidak
  `display: block`), dan modal form memakai `modalClassName="region-confirm-modal-wide"` (550 px) dengan
  isi rata kiri — `size="lg"` tidak melebarkan ConfirmModal.

| Pemeriksaan Fase 3 | Hasil |
| --- | --- |
| ESLint | 0 error; peringatan tersisa hanya di file lama |
| `check-css-usage.mjs` pada 15 file JSX baru/diubah | 0 temuan di file baru; 3 temuan di baris lama panel profil yang tidak disentuh |
| Unit test | 104 gagal dari 5374 — tidak ada test baru yang gagal dibanding putaran sebelumnya |
| `npm run build` | Berhasil |
| Uji browser Fase 3 | 10/10 PASS: validasi form kosong, jam terbalik, payload dispensasi/pembatalan/konfirmasi (POST dicegat; data cuti dan panggilan disuntikkan lewat GET tiruan) |
| Uji ulang form Fase 2 sesudah perubahan modal | 8/8 PASS |
