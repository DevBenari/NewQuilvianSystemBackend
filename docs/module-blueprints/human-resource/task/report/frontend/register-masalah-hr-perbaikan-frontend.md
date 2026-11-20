# Laporan Perubahan Frontend — Perbaikan Register Masalah SDM (`MSL-03`, `MSL-28` s.d. `MSL-33`)

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `MSL-03`, `MSL-28`, `MSL-29`, `MSL-30`, `MSL-31`, `MSL-32`, `MSL-33`, ditambah penyesuaian layar untuk `MSL-02`, `MSL-07`, `MSL-09`; putaran 2: `MSL-10`, `MSL-14`, `MSL-15` tahap 1; putaran 3: `MSL-15` tahap 2, `MSL-16` |
| Judul | Perbaikan masalah frontend dari register masalah modul SDM |
| Roadmap | [register-masalah-hr.md](../../../roadmap/register-masalah-hr.md) bagian 3 dan 6 |
| Wewenang | Permintaan eksplisit pemilik, 18 November 2026: "sekarang perbaiki seluruh masalah nya"; `DEC-FER-04` opsi A (rekomendasi) dipakai untuk `MSL-30` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source); laporan dan baris status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `f7a1f0917` (branch `HamzahV2`), perubahan belum di-commit |
| Tanggal | Rabu, 18 November 2026 (putaran 1–2); Kamis, 19 November 2026 (putaran 3) |
| Status | 🟡 Putaran 1: source, lint, test unit, dan CSS gate selesai; `npm run build` sempat tertahan galat di pekerjaan paralel pengguna (lulus di putaran 2). Putaran 3: 6 layar kredensial `MSL-16` lulus uji visual dan uji live baca, `npm run build` lulus 839 halaman; layar unggah `MSL-15` tahap 2 belum dibuka di peramban (bagian 6) |

---

## 1. Ringkasan untuk pembaca umum

| No. | Masalah | Hasil | Contoh di rumah sakit |
|---|---|---|---|
| `MSL-03` | `npm run build` gagal karena impor `getTodayDate` | ✅ | Layar Uraian Tugas Pegawai memakai `todayInputValue`; galat ini tidak muncul lagi di build |
| `MSL-28` | Menu SDM terlihat oleh pegawai tanpa hak | ✅ | Perawat tidak lagi melihat menu "Insiden Disiplin" atau "Pusat MCU"; Kepala K3RS tetap melihatnya |
| `MSL-29` | Pengaturan Perencanaan diam-diam kosong bagi pegawai tanpa hak | ✅ | Pegawai yang membuka alamatnya melihat pesan "Akses Ditolak" yang jelas |
| `MSL-30` | Dua layar untuk pengajuan lembur | ✅ | Alamat lama `/self-services/human-resource/overtime` dialihkan ke `/hr/self-service/overtime` |
| `MSL-31` | Tombol CTO tidak mengisi jenis cuti | ✅ | "Gunakan untuk Cuti" membuka formulir dengan Cuti Pengganti Lembur terpilih |
| `MSL-32` | Roster tidak bisa menambah shift ke sel kosong | ✅ | Kepala ruang mengklik sel kosong, memilih shift, lalu menyimpan |
| `MSL-33` | Status R14–R20 di roadmap masih "tertahan 401" | ✅ | 16 baris status diberi catatan bahwa hambatan itu sudah tidak berlaku |

---

## 2. Perubahan per masalah

### 2.1 `MSL-28` — Gerbang hak akses menu

- 64 butir menu SDM diberi `requiredPermission` yang nama resource/aksinya diverifikasi terhadap `[AccessPermission]` controller backend. Grupnya: Onboarding & Kontrak 9, Persetujuan V2 3, Cuti 4, Lembur 3, Manfaat 7, Kredensial (OPPE) 2, Diklat 8, Kinerja 8, Hubungan Industrial 8, K3RS 8, SPPD 4.
- Butir layanan mandiri sengaja tetap terbuka: **Dasbor Saldo & Cuti Saya**, **Formulir Pengajuan Cuti**, dan **Pengajuan & Status Lembur (ESS)**.
- `filter-menu-items-by-permission.jsx` kini juga menerima **daftar** hak (`[{ resource, action }, …]`, cukup salah satu). Ini dibutuhkan layar dua audiens. Contoh: **Lembar Evaluasi Berjenjang** tampil bagi pegawai (`PerformanceAppraisal : ViewOwn`) maupun atasan (`ViewSubordinates`). Bentuk objek tunggal yang lama tidak berubah perilakunya.

| Contoh butir | Hak |
|---|---|
| Laporan Insiden & Just Culture | `EmployeeDisciplinary : ViewIncidents` **atau** `CreateIncident` |
| Konsol Tertusuk Jarum CITO | `NeedleStickEmergency : Read` **atau** `ReportNeedleStickEmergency` |
| Pendelegasian Wewenang (Plh) | `WorkflowDelegation : ViewDelegationList` **atau** `ViewOwn` **atau** `CreateDelegation` |
| Persetujuan Cuti Staf | `LeaveApproval : View` |
| Matriks Tarif & Kebijakan SPPD | `TravelRatePolicy : ViewRate` |

### 2.2 `MSL-29` — Pengaturan Perencanaan

Memakai pola Pengaturan Presensi: `usePermission("WorkforcePlanningSetting", "Read")`. Bila daftar hak sudah termuat dan hak itu tidak ada, layar menampilkan `AccessDeniedAlert`, formulir disembunyikan, dan pengaturan tidak diminta ke server.

### 2.3 `MSL-30` — Rute lama lembur

`src/app/self-services/human-resource/overtime/page.jsx` kini `redirect("/hr/self-service/overtime")`. View lama `overtime-employee-view.jsx` dibiarkan, karena masih dirujuk test `workforce-core-cross-employee.test.mjs`.

### 2.4 `MSL-31` — Prefill jenis cuti

- Halaman `/hr/leave/request` membaca `?jenisCuti=<kode>` dari `searchParams` (server), lalu meneruskannya ke `LeaveRequestView` sebagai `initialLeaveTypeCode`.
- Formulir memilih opsi pertama yang bisa diajukan dengan kode sama, atau yang memuat kode itu, selama pegawai belum memilih sendiri. Tidak ada `setState` di efek.
- Tombol **Gunakan untuk Cuti** di Saldo Libur Pengganti Lembur kini membuka `/hr/leave/request?jenisCuti=CTO`. Backend mengenali jenis CTO dari kode yang memuat "CTO".

### 2.5 `MSL-32` — Tambah shift di sel kosong

- Sel kosong kini bisa diklik bila periode belum ditutup dan pengguna punya `RosterPeriod : Create`. Sel berisi shift tetap memakai hak `ShiftReplacement : Update`.
- Modal baru `AddRosterShiftModal` memanggil `POST roster-periods/{id}/assignments` dengan satu shift.
- Bila roster sudah terbit, kategori alasan dan keterangan wajib diisi.
- Bila server menjawab `422`, pelanggaran blokir (B1–B8) ditampilkan dan simpan dikunci. Peringatan (W1–W7) ditampilkan, dan tombol berubah menjadi **Simpan dengan Alasan**, yang mengirim ulang beserta `warningReasons`.

### 2.6 `MSL-33` — Status roadmap

16 baris status di [frontend-unfinished-work.md](../../../roadmap/frontend-unfinished-work.md) (tabel dan kartu `FE-HRD-R14`–`R20` serta `R25`–`R29`) diberi catatan: hambatan `401` sudah tidak berlaku, layar dibuka di Gelombang 4 lewat `R42`/`R43`/`R44`/`R45`/`R46`, dan alur tulis menunggu data uji (`MSL-34`). Tanda status tidak diubah.

### 2.7 Penyesuaian terhadap perbaikan backend

| Layar | Perubahan | Terkait |
|---|---|---|
| Plafon & Klaim Restitusi Saya | Riwayat klaim memakai `GET claims/mine` dengan hak `ViewOwn`; service mendapat `getMyClaims` dan `getMyClaimById` | `MSL-07` |
| Slip Gaji Saya | Petunjuk sandi PDF: "tanggal lahir Anda dengan format DDMMYYYY. Contoh, lahir 3 Juni 1999 berarti 03061999." | `MSL-09` |
| Penilaian Kinerja | Pesan untuk draf yang belum tersimpan; kunci baris memakai `metricCatalogId` saat ID kosong | `MSL-02` |
| Keberatan Penilaian | "Belum ada siklus" ditampilkan sebagai info, bukan galat | `MSL-02` |
| Logbook FPPE | Tidak diubah — sudah mengirim ID profil dari sesi, sama dengan pemeriksaan server baru | `MSL-06` |

---

## 3. Validasi

### 3.1 Berkas yang berubah

`src/components/view/hr/workforce-core/employee-job-descriptions-view.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx`, `tests/unit/menu-permission-filter.test.mjs`, `src/components/view/hr/workforce-planning/workforce-planning-settings-view.jsx`, `src/app/self-services/human-resource/overtime/page.jsx`, `src/app/hr/leave/request/page.jsx`, `src/components/view/hr/leave/leave-request-view.jsx`, `src/lib/constants/hr/overtime/overtime-constants.js`, `src/components/view/hr/overtime/overtime-compensatory-leave-view.jsx`, `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx`, `src/components/view/hr/scheduling/modals/AddRosterShiftModal.jsx` (baru), `src/lib/services/hr/restitution-service.js`, `src/components/view/hr/restitution/ess-restitution-claims-view.jsx`, `src/components/view/hr/payroll/payslips-ess-view.jsx`, `src/components/view/hr/performance/performance-appraisals-view.jsx`, `src/components/view/hr/performance/performance-disputes-view.jsx`.

Catatan: pada saat yang sama pengguna menyunting `menu-items.jsx` (grup baru "Kontrak & Masa Kerja") dan berkas `lifecycle`. Perubahan itu bukan bagian laporan ini.

### 3.2 Hasil perintah

| Perintah | Hasil |
|---|---|
| `npx eslint` pada seluruh berkas yang diubah | 0 error. 1 warning `react-hooks/set-state-in-effect` di Pengaturan Perencanaan, yang sama persis sudah ada di HEAD (dibandingkan lewat worktree HEAD) |
| `node --test tests/unit/menu-permission-filter.test.mjs …` (5 berkas test menu) | Test baru **M9** lulus. 9 test gagal (hitungan Master Data, CRM, Bank Darah) — **sama persis** dengan HEAD `f7a1f0917` di worktree bersih, jadi bukan regresi |
| `npm run build` | Gagal hanya di `contract-renewals-list-view.jsx` dan `employment-status-changes-view.jsx` (pekerjaan paralel pengguna; `DECISION_STATUS_FILTER_OPTIONS` belum diekspor). Galat `getTodayDate` (`MSL-03`) tidak muncul lagi |
| `AUTOMATED TEST` | `node --test tests/unit/menu-permission-filter.test.mjs` — PASS untuk M1–M9 (M0 gagal juga di HEAD) |

### 3.3 Verifikasi manual

`VISUAL CHECK: NOT FEASIBLE` — sesi ini belum mendapat izin menyalakan backend di DB dev bersama (backend menulis registri hak akses saat start), sehingga login dan pengambilan screenshot belum bisa dilakukan. `MANUAL TEST: NOT FEASIBLE` dengan alasan yang sama.

### 3.4 Gerbang UI

| Elemen | Keputusan | Sumber gaya |
|---|---|---|
| Pesan tanpa hak (`MSL-29`) | `REUSE` `AccessDeniedAlert` | Bawaan base component |
| Modal Tambah Shift (`MSL-32`) | `COMPOSE` `ConfirmModal` + `HrResourceField` + `BaseSelectField` + `BaseTextAreaField` + `InformationAlert` + `HrInfoGrid`, persis pola `RetroactiveShiftCorrectionModal` | `hr-shared.module.css` (sudah ada) |
| Sel roster kosong yang bisa diklik | `REUSE` tombol sel yang ada (`gridStyles.shiftButton`) | CSS Module roster yang ada |
| Petunjuk sandi PDF | `REUSE` `InformationAlert variant="info"` | Bawaan |
| Pesan draf penilaian | `REUSE` `InformationAlert variant="info"` | Bawaan |
| Penyaring menu daftar hak | Util, bukan komponen visual | — |

`UI GATE: PASS — 5 elemen REUSE, 1 COMPOSE, 0 NEW`

Pemeriksa CSS:

```text
node <rules>/frontend/tools/check-css-usage.mjs <12 berkas jsx yang diubah>
OK — 12 berkas diperiksa, 0 temuan.
```

Screenshot: belum ada (lihat 3.3).

---

## 4. Catatan penutup

| Hal | Isi |
|---|---|
| Risiko | Butir menu bergerbang hanya tampil bila daftar hak termuat (`selectPermissionDecision` ketat). Selama daftar hak belum termuat, butir itu tersembunyi — perilaku yang sama dengan butir bergerbang lain |
| Dependency | Screenshot dan uji per peran menunggu izin menyalakan backend serta akun/data uji (`MSL-34`). `npm run build` penuh menunggu pekerjaan `lifecycle` pengguna tersambung |
| Task berikutnya | Uji live: menu sebagai pegawai biasa vs petugas, tambah shift di roster terbit (W1–W7), formulir cuti `?jenisCuti=CTO`, riwayat klaim restitusi ESS |

---

## 5. Putaran 2 — 18 November 2026 sore

**Wewenang.** "Oke ikuti semua rekomendasi, lanjut kerjakan": opsi A untuk `DEC-FER-02` (base component unggah berkas) dan `DEC-FER-05` (daftar lintas pegawai), serta perbaikan galat build Menu 08.

### 5.1 Build kembali lulus

| Masalah | Perbaikan |
|---|---|
| Commit pengguna `89c070547` mengembalikan impor `getTodayDate` di Uraian Tugas Pegawai (`MSL-03` terulang) | `getTodayDate` kini diekspor dari `utils/hr/workforce-core/workforce-core-display-utils.js` (opsi kedua di rekomendasi `MSL-03`), sehingga impor itu sah |
| Dua layar Menu 08 (`contract-renewals-list-view`, `employment-status-changes-view`) mengimpor `DECISION_STATUS_FILTER_OPTIONS` yang belum ada | Ditambahkan di `lib/constants/hr/lifecycle/lifecycle-constants.js` |

`npm run build`: **lulus**, 839 halaman (dijalankan dua kali: sesudah perbaikan di atas, dan sesudah seluruh perubahan putaran 2).

### 5.2 `MSL-10` — Layar Pengaturan Lembur dapat diubah

`overtime-settings-view.jsx` kini punya tombol **Ubah Parameter** untuk hak `OvertimeSetting : Manage` (`OVERTIME_R35_PERMISSIONS.manageSettings` baru).
- Modal berisi 8 isian angka, dengan validasi yang sama dengan server. Isian tidak memakai `min`/`max` HTML, karena `BaseInputField` akan memotong nilai diam-diam dan pesan galat tidak pernah muncul.
- Bila server menjawab `isPersisted: false`, layar menampilkan peringatan bahwa tabel belum dibuat.

### 5.3 `MSL-14` — 8 layar memakai daftar seluruh pegawai

| Layar | Sebelumnya | Sekarang |
|---|---|---|
| Pembatasan Tugas Medis | Tabel hanya berisi pembatasan yang diterbitkan pada sesi ini | Daftar server dengan pencarian, filter status (bawaan: Aktif), paginasi; asesmen kembali bekerja per baris |
| Rekam Kesehatan & Kelaikan Kerja | Wajib memilih pegawai satu per satu | Daftar kelaikan kerja seluruh pegawai + filter kelaikan; **Lihat** membuka ringkasan pegawai itu |
| Kecelakaan Kerja KAK/PAK | Tabel sesi | Daftar server + pencarian; status lapor BPJS 2×24 jam per baris |
| Radar Imunisasi | Hanya radar booster dan buku per pegawai | Ditambah panel daftar vaksinasi seluruh pegawai |
| Surat Peringatan | Wajib memilih pegawai | Daftar SP seluruh pegawai + filter tingkat; **Riwayat** membuka riwayat pegawai itu |
| Skorsing | Tabel sesi | Daftar server + pencarian; **Pulihkan** memuat ulang daftar |
| Sanggahan Nilai | Tabel sesi | Majelis (`ManageDispute`) melihat daftar seluruh pegawai; pegawai biasa tetap melihat sanggahannya sendiri |
| Jenjang Karier Klinis | Tabel sesi | Komite (`ManageClinicalCareer`) melihat daftar seluruh pegawai; pegawai tetap melihat pengajuannya sendiri |

Stub service "belum tersedia" (`getWorkRestrictions`, `getHealthRecords`, `getOccupationalInjuries`, `getVaccinations`, `getSanctions`, `getSuspensions`, `getDisputes`, `getClinicalLadderApplications`) kini memanggil endpoint nyata.

### 5.4 `MSL-15` — Unggah berkas (`DEC-FER-02` opsi A), tahap 1

| Elemen | Keputusan | Sumber gaya |
|---|---|---|
| `BaseFileUpload` (`components/features/base-features/base-file-upload.jsx`) | `NEW`, disetujui pemilik lewat `DEC-FER-02` opsi A | `style/components/features/base-features/base-file-upload.module.css` (semua nilai `var(--…)`) |
| `HrFileField` | `WRAP` → kini memakai `BaseFileUpload`. Lima layar Lifecycle dan formulir cuti ikut berganti tampilan tanpa perubahan pemanggil | — |
| `HrAttachmentField` (`components/features/hr/shared/hr-attachment-field.jsx`) | `COMPOSE` `BaseFileUpload` + `BaseButton`: unggah saat berkas dipilih, simpan kunci, tombol **Lihat** mengunduh lewat endpoint ber-hak akses | Bawaan |

Formulir yang memakainya:
- **Klaim restitusi ESS:** nota wajib, resume medis opsional. Pratinjau berkas menampilkan lampiran dari server lewat object URL.
- **Settlement SPPD:** foto struk per butir pengeluaran, dikirim sebagai `receiptFilePath`.
- **Penilaian diri:** bukti capaian per indikator, wajib untuk nilai 5 (VR-PRF-22).
- **Sanggahan nilai:** bukti pendukung.

`UI GATE: PASS — 1 NEW (disetujui DEC-FER-02), 1 WRAP, 1 COMPOSE`

### 5.5 Validasi putaran 2

| Perintah | Hasil |
|---|---|
| `npx eslint` seluruh berkas putaran 2 | 0 error, 0 warning |
| `check-css-usage.mjs` 19 berkas (8 layar MSL-14, overtime settings, 9 berkas MSL-15, base component) | `0 temuan` |
| `npm run build` | Lulus, 839 halaman |
| Uji live pegawai non-admin (`msl-1118-nonadmin.mjs`, akun Dewi) | `MSL-28`: 7 grup SDM terlarang tidak tampil. `MSL-29`: "Akses Ditolak" tampil. `MSL-30`: dialihkan ke `/hr/self-service/overtime`. `MSL-31`: Cuti Pengganti Lembur terpilih otomatis (opsi saldo dipenuhi lewat `page.route`, karena DB dev hanya punya jenis cuti `LEG_DEFAULT`). Screenshot: `test-with-agy/screenshots/hrw4-1118/msl-1118-nonadmin/` |
| Uji live akun Mira (`msl-1118-mira.mjs`: Plh, penilaian, slip gaji, restitusi, modal Tambah Shift dengan respons tiruan) | `MANUAL TEST: NOT FEASIBLE` — DB dev `160.22.250.77` tidak bisa dijangkau (koneksi timeout), login menggantung "Memproses...". Skrip siap dijalankan ulang |
| Layar `MSL-14`, `MSL-15`, dan Pengaturan Lembur | `VISUAL CHECK: NOT FEASIBLE` dengan alasan yang sama; endpoint baru juga butuh build backend terbaru, yang tertahan galat Menu 08 di `HrdEmploymentContractService.cs:369` |

## 6. Putaran 3 — 19 November 2026

**Wewenang.** "Oke lanjut" atas rencana "teruskan sisa `MSL-15`, lalu `MSL-16`". Opsi A di semua keputusan tetap berlaku.

### 6.1 `MSL-15` tahap 2 — Unggah berkas di 6 layar lagi

Semua memakai `HrAttachmentField` (tahap 1). Berkas diunggah saat dipilih, lalu kunci berkasnya dikirim bersama formulir.

| Layar | Sebelumnya | Sekarang |
|---|---|---|
| Klaim COB ESS (`ess-cob-submission-view.jsx`) | Pegawai menyerahkan SEP BPJS dan kuitansi secara fisik | Unggah SEP BPJS dan kuitansi asli. Sesudah klaim tersimpan, keduanya didaftarkan sebagai dokumen klaim (`BPJS_SEP`, `Receipt_Original`); bila pendaftaran dokumen gagal, muncul peringatan agar pegawai melengkapinya dari detail klaim |
| Detail kandidat (`candidate-detail-screening-view.jsx`) | Panel dokumen hanya baca, dengan catatan "server belum menyediakan unggah" | Tombol **+ Unggah Dokumen** (hak `Candidate : UploadDocument`): jenis dokumen (9 pilihan sesuai enum server), nomor, judul, berkas. Kolom **Berkas** berisi tombol **Unduh** untuk dokumen yang diunggah lewat sistem |
| MCU pra-kerja (`mcu-clearance-view.jsx`) | Isian path sertifikat diketik | Unggah sertifikat MCU |
| Monitoring presensi — koreksi atas nama (`attendance-monitoring-view.jsx`) | Isian path bukti diketik | Unggah bukti koreksi |
| Dinas luar Workforce Core (`workforce-core/official-duties-view.jsx`) | Isian URL lampiran | Unggah surat tugas |
| Dinas luar Presensi (`attendance/official-duties-view.jsx`) | Tautan lampiran dibuka sebagai URL | Lampiran lama (URL `http…`) tetap dibuka di tab baru; lampiran baru diunduh lewat endpoint ber-hak akses |

Belum dikerjakan: logbook lembur dan nota override rekredensial (butuh kolom berkas baru, artinya migration), serta impor spreadsheet batch kontrak awal (butuh endpoint impor).

### 6.2 `MSL-16` — 6 layar kredensial tidak lagi placeholder

Sebelumnya keenam layar hanya menampilkan peringatan "belum dapat dioperasikan tanpa data semu".

| Layar (rute) | Hak menu | Isi sekarang |
|---|---|---|
| Meja Kerja Mitra Bestari (`/corporate/hrd/credentialing/peer-review`) | `SpkRkkIssuance : AssessPeer` | Worklist permohonan yang menunjuk akun login, kolom "Penilaian Masuk" dan "Penilaian Saya". Tombol **Nilai** membuka lembar penilaian: satu pilihan tingkat kewenangan dan catatan per butir yang diajukan, plus rekomendasi keseluruhan. Tombol kirim aktif hanya bila semua butir sudah dipilih. Akun tanpa profil tenaga kerja mendapat penjelasan, bukan tabel kosong yang membingungkan |
| Sidang Pleno Komite (`/corporate/hrd/credentialing/committee-plenary`) | `SpkRkkIssuance : ApproveCommittee` | Worklist dengan status "Siap dipleno" / "Menunggu penilaian". **Buka Matriks** menampilkan nama Mitra Bestari 1 dan 2, lalu per butir: rekomendasi tiap reviewer (lencana Mandiri / Supervisi / Proctoring / Tidak diizinkan) dan pilihan kewenangan akhir (bawaan = rekomendasi paling ketat). Formulir BAP: nomor, tanggal, keputusan, catatan Ketua. Ada peringatan bila belum semua reviewer menilai |
| Pengesahan SPK-RKK Direktur (`/corporate/hrd/credentialing/director-approval`) | `SignDirector` **atau** `ParafDirector` (menu kini any-of) | Panel **Menunggu Tanda Tangan** (tombol **Tandatangani** dengan konfirmasi) dan panel **Antrean Draf** (tombol **Terbitkan Draf**: tanggal mulai, jenis SPK, catatan; masa berlaku 3 tahun dihitung server). Tiap panel hanya tampil untuk pemegang haknya |
| Registri Kewenangan Klinis (`/corporate/hrd/credentialing/privilege-registry`) | `WorkforceClinicalPrivilege : Read` | Daftar kewenangan seluruh tenaga kesehatan dengan pencarian, filter status, dan paginasi server |
| Radar Siklus Rekredensialing (`/corporate/hrd/credentialing/recredentialing-cycle`) | `RecredentialingQuality : Read` | SPK-RKK yang habis dalam 60/90/180/365 hari, lencana sisa waktu (merah ≤ 30 hari atau lewat, kuning ≤ 60 hari), status berkas rekredensial, nilai OPPE final terakhir |
| Verifikasi publik (`/verify/clinical-privilege/[hash]`) | Tanpa login | Status SAH / KEDALUWARSA / DIBEKUKAN / DICABUT / BELUM DISAHKAN beserta data yang tercetak di SPK |

**Perbaikan rute publik.** `/verify/...` belum terdaftar sebagai jalur publik. Akibatnya pemindai QR tanpa akun dilempar ke halaman login, dan bila sempat tampil, halaman itu terbungkus sidebar aplikasi internal. Hal ini juga berlaku untuk verifikasi naskah dinas SDM yang sudah ada (`/verify/document/[hash]`). `/verify` kini ditambahkan ke empat daftar jalur terbuka, mengikuti pola `/priority-arrival`: `src/proxy.js`, `components/ui/auth-wrapper.jsx`, `components/providers/root-layout-shell.jsx` (tanpa sidebar/navbar), dan `components/providers/session-activity-manager.jsx` (tanpa penyegaran sesi). Kedua halaman verifikasi diberi jarak tepi `p-3 p-md-4` (utilitas Bootstrap yang ada).

**Berkas.** `lib/services/hr/credentialing-service.js` (10 fungsi baru), `lib/constants/hr/credentialing/credentialing-constants.js` (hak akses, tingkat supervisi + label pendek, keputusan pleno, jenis SPK, status verifikasi publik, status/jenis kewenangan, jendela radar), 6 view di `components/view/hr/credentialing/`, `components/view/hr/services/public-document-verify-view.jsx` (jarak tepi), `utils/menu-sidebar/menu-items.jsx`, `src/proxy.js`, `components/ui/auth-wrapper.jsx`, `components/providers/root-layout-shell.jsx`, `components/providers/session-activity-manager.jsx`.

### 6.3 Gerbang UI putaran 3

| Elemen | Keputusan | Sumber gaya |
|---|---|---|
| Hero, DataFilter, FilterSelect, DataTable + RegionPagination, StatusBadge/HrStatusBadge, BaseButton, ConfirmModal, BaseInputField/BaseDateField/BaseSelectField/BaseTextAreaField, HrPanel, HrInfoGrid, InformationAlert, ToastStack | `REUSE` | Bawaan base component |
| Tata letak formulir di modal | `REUSE` `hr-shared.module.css` (`formGrid`, `formFull`, `formNote`) | CSS Module yang sudah ada |
| Lebar modal penilaian, matriks pleno, dan draf SPK | `REUSE` kelas global `region-confirm-modal-wide` (550 px) lewat `modalClassName`. `size="xl"` tidak dipakai karena gaya bawaan `ConfirmModal` mengunci lebar 430 px | `modal-confirm.css` yang sudah ada |
| Matriks pleno (butir × reviewer) | `REUSE` `HrInfoGrid` per butir. `DataTable` ditolak karena lebar minimumnya 840 px (`min-width: max(840px, 100%)`) sehingga selalu harus digeser di modal; ini terlihat saat uji live | Bawaan |
| Jarak tepi halaman publik | `REUSE` utilitas Bootstrap `p-3 p-md-4` (ada di `style/bootstrap.min.css`) | Global |

`UI GATE: PASS — semua elemen REUSE, tanpa komponen atau CSS baru`

### 6.4 Validasi putaran 3

| Perintah | Hasil |
|---|---|
| `npx eslint` berkas `MSL-15` tahap 2 | 0 error; 1 warning lama di `workforce-core/official-duties-view.jsx` (sudah ada sebelum putaran ini) |
| `npx eslint` 14 berkas `MSL-16` (6 view, service, konstanta, menu, `proxy.js`, `auth-wrapper.jsx`, `root-layout-shell.jsx`, `session-activity-manager.jsx`, `public-document-verify-view.jsx`) | 0 error; 1 warning lama `react-hooks/set-state-in-effect` di `root-layout-shell.jsx` (identik di HEAD) |
| `check-css-usage.mjs` 6 berkas `MSL-15` tahap 2 | `OK — 6 berkas diperiksa, 0 temuan.` |
| `check-css-usage.mjs` 6 view `MSL-16` + `public-document-verify-view.jsx` | `OK — 6 berkas diperiksa, 0 temuan.` dan `OK — 2 berkas diperiksa, 0 temuan.` (diulang sesudah setiap perubahan tata letak) |
| `npm run build` | Lulus, 839 halaman (dijalankan ulang sesudah setiap perbaikan hasil uji live; terakhir lulus) |
| Uji live | Lihat bagian 6.5 |
| `AUTOMATED TEST` | `SKIPPED (opsional) — test-policy: menulis test baru bukan gerbang selesai` |

### 6.5 Uji live putaran 3 (19 November 2026)

DB dev `160.22.250.77` kembali bisa dijangkau. Backend scratch dijalankan dengan `Runtime__Role=Web` dan frontend standalone di `localhost:3000`. Chrome dibuka headed lewat CDP port 9337. Data kredensial di DB dev kosong (0 permohonan, 0 SPK-RKK, 0 kewenangan klinis, 0 rekredensial), dan penulisan data uji belum diizinkan. Karena itu ada dua jenis uji: **baca ke server sungguhan** dan **interaksi dengan respons tiruan** (`page.route`; POST dicegat dan diperiksa, tidak ada yang sampai ke server).

| No. | Skenario | Prioritas | Hasil |
|---|---|---|---|
| 1 | Lima layar kredensial memuat daftar dari server (superadmin) | P0 | Lulus: 6 request `200`, tanpa galat halaman. Keadaan kosong menampilkan judul dan petunjuk yang jelas |
| 2 | Antrean draf SPK-RKK | P0 | **Gagal lalu diperbaiki:** `500` karena kolom `jsonb` dibandingkan dengan ILIKE (lihat laporan BE bagian 9.3). Sesudah perbaikan: `200` |
| 3 | Worklist Mitra Bestari untuk akun yang punya profil tenaga kerja (Mira) | P0 | Lulus `200`: kueri `jsonb @>` benar-benar berjalan (tidak ada pesan "akun belum terhubung") |
| 4 | Filter status registri kewenangan: pilih "Aktif" | P2 | Lulus: request membawa `status=Active` |
| 5 | Verifikasi publik tanpa login, hash tak dikenal dan bentuk salah | P0/P1 | Lulus: tetap di halaman verifikasi (tidak dilempar ke login), tanpa sidebar, pesan "Dokumen tidak dikenal…" |
| 6 | Lembar penilaian: tombol kirim nonaktif sampai semua butir dipilih | P2 | Lulus (nonaktif → aktif) |
| 7 | Body POST penilaian | P1 | Lulus: `reviewerWorkforceProfileId` dari lembar server, `recommendedSupervisionLevel` angka (1 dan 3), rekomendasi keseluruhan ikut |
| 8 | Pleno: tombol sahkan nonaktif tanpa nomor BAP; kewenangan akhir bawaan = paling ketat | P1/P2 | Lulus: nonaktif → aktif; body berisi level 1 dan 3 (reviewer memberi 1/1 dan 2/3) |
| 9 | Direktur: terbitkan draf dan tandatangani | P1 | Lulus: body `{ applicationId, validFrom, clinicalPrivilegeType, notes }` dan `{ spkRkkIssuanceId }` |
| 10 | Halaman publik keadaan SAH | P4 | Lulus: lencana SAH, nomor SPK/RKK, masa berlaku, pengesah |
| 11 | Tata letak modal matriks pleno | P4 | **Diperbaiki:** semula modal 430 px dan tabel 840 px sehingga kolom reviewer terpotong. Kini modal 550 px dan matriks per butir tanpa geser |

Skrip: `test-with-agy/scripts/msl-1119-credentialing.mjs` (baca), `msl-1119-credentialing-mock.mjs` (interaksi tiruan), `msl-1119-shot.mjs` (screenshot). Screenshot: `test-with-agy/screenshots/hrw4-1118/msl-1119-credentialing/` (01–08 baca, 11–41 interaksi) dan `msl-1119-credentialing-mira/`.

`VISUAL CHECK: PASS` untuk 6 layar `MSL-16`. Layar `MSL-15` tahap 2 (unggah COB, kandidat, MCU, koreksi presensi, dinas luar) belum dibuka di peramban pada putaran ini: `VISUAL CHECK: NOT RUN — fokus uji putaran ini MSL-16; skrip unggah perlu berkas uji dan menulis ke server`.
