# Laporan Perubahan Frontend — Perbaikan Register Masalah SDM (`MSL-03`, `MSL-28` s.d. `MSL-33`)

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `MSL-03`, `MSL-28`, `MSL-29`, `MSL-30`, `MSL-31`, `MSL-32`, `MSL-33`, ditambah penyesuaian layar untuk `MSL-02`, `MSL-07`, `MSL-09` |
| Judul | Perbaikan masalah frontend dari register masalah modul SDM |
| Roadmap | [register-masalah-hr.md](../../../roadmap/register-masalah-hr.md) bagian 3 dan 6 |
| Wewenang | Permintaan eksplisit pemilik, 18 November 2026: "sekarang perbaiki seluruh masalah nya"; `DEC-FER-04` opsi A (rekomendasi) dipakai untuk `MSL-30` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source); laporan dan baris status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `f7a1f0917` (branch `HamzahV2`), perubahan belum di-commit |
| Tanggal | Rabu, 18 November 2026 |
| Status | 🟡 Source, lint, test unit, dan CSS gate selesai. `npm run build` tertahan galat di pekerjaan paralel pengguna (layar `lifecycle` baru). Screenshot browser belum diambil |

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
