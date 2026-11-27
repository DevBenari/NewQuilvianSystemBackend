# Laporan Verifikasi Frontend — `FE-HRD-316`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-316` |
| Judul | Verifikasi browser MVP `A52` |
| Slice | `A52` / `Verifikasi` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-316` |
| Trace | `04-prd-to-mvp.md` 40.18 (`UAT-PK-01` s.d. `-15`), 40.19 (Definition of Done) |
| Contract version | `v26` |
| Dependency | `FE-HRD-306` s.d. `FE-HRD-315` ✅ |
| Klasifikasi | `MEDIUM` — verifikasi lintas layar dengan data asli, ditambah dua perbaikan frontend kecil dari temuan |
| Task mode | `FRONTEND` verifikasi (instruksi pengguna 24 November 2026: "lanjutkan FE-HRD-316") |
| Keputusan pengguna (24 November 2026) | (1) izin uji diberikan **sementara** ke jabatan akun uji lalu dipulihkan identik; (2) boleh menulis data uji `UJI-A52` di devYoga lewat API + sisip DB; (3) akun non-superadmin = gilang (dr. Bima tidak dipakai); (4) UAT yang tidak dapat dijalankan persis memakai **padanan data devYoga + harness** |
| Lingkungan | Backend lokal `https://localhost:7184` (devYoga, Development, build 23 November 2026 09.49, branch `yoga` `4d2c0162`; perubahan sesudah build hanya modul Workflow yang tidak dipakai UAT ini) + frontend dev `http://localhost:3000` (branch `YogaV2` `004524f4c` + working tree `FE-HRD-313`–`315`). Login sungguhan lewat halaman login (geolokasi geofence), Playwright Chromium. Kedua server dihentikan sesudah uji (sebelum task juga mati) |
| Model | Claude Opus 5.5 |
| Tanggal | 24 November 2026 |
| Status | 🟡 Verifikasi selesai — 13 dari 15 UAT lulus (`UAT-PK-04` lewat harness); `UAT-PK-03` dan `-09` gagal karena dua cacat backend (bagian 3) |

---

## 1. Data uji yang dipakai dan dibuat

Pemetaan nama UAT ke profil devYoga (data `UJI-A52` sejak 13 November 2026): Budi = Bagas Saputra / Arif Maulana, Tono =
Raka Firmansyah (penempatan sementara IGD 1–14 Mar 2027, menggantikan HCU), Rina/Sari (pegawai berakun) = Gilang Mahendra,
Andi (jadwal kantor) = Dewi Anggraeni.

| Data | Cara | Keadaan akhir |
| --- | --- | --- |
| Master shift "Uji Singkat (UJI-A52-316)" 12.30–12.55 | API | Tetap |
| Roster `UJI-A52-316` ICU 24 Nov 2026 (gilang, Published) | Sisip DB | Tetap |
| Roster `UJI-A52-316-IGD` IGD 24 Nov 2026 (Raka Pagi, Published) | Sisip DB | Tetap |
| Ganti Petugas Indra → Raka, Pagi 16 Feb 2027 (`ROST-202611-0002`) | Layar | Tetap (tercatat di Riwayat Revisi) |
| Tambah lalu hapus Siang 21 Feb 2027 Raka | Layar | Shift `Cancelled`; dua baris riwayat |
| Scan masuk gilang 17 Nov 07.12 (`UJI-A52-316-PK08-IN`) | Sisip DB | Tetap; rekap 17 Nov "Belum Lengkap", terlambat 12 |
| Toleransi 10 → 15 → 10 | Layar Setelan | Kembali 10 |
| Dua scan gilang 18 Nov dari mesin `UJI-A52-GDB-316` | API raw log batch | Dilepas; mesin terdaftar aktif |
| Periode kehadiran `ATP-202611-0001` (ICU 17 Nov 2026) | API | **Open** — penutupan terblokir (temuan T2) |
| Hari libur `UJI-A52-316-LIBUR` 24 Nov 2026 | Layar | Dihapus lunak lewat layar; Dewi diproses ulang |
| Izin jabatan gilang (Penunjang Medis / Analis Laboratorium) | Role Access, sementara | Dipulihkan identik (32 butir) dua kali |
| Penempatan berjabatan `UJI-A52-316-KU-ICU`, `UJI-A52-316-KB-RANAP` | Sisip DB, sementara | Dihapus lunak |

---

## 2. Hasil per UAT

| UAT | Langkah di devYoga | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- | --- |
| `UAT-PK-01` | gilang sebagai Kepala Unit ICU membuka roster IGD, lalu mencoba Hapus Shift dan Ganti Petugas di IGD | Layar: "Anda tidak berwenang melihat data unit ini."; kedua aksi `403` "Anda tidak berwenang mengubah roster unit ini."; roster ICU sendiri terbaca (`200`) | `PASS` | `01a-ku-buka-roster-igd.png` |
| `UAT-PK-02` | gilang sebagai Kepala Bidang (Instalasi Rawat Inap, induk ICU) membuka roster ICU, lalu Ganti Petugas | Kisi terbaca, tombol aksi roster tersembunyi; Ganti Petugas `403` "Anda tidak memiliki akses ke menu atau fitur ini." | `PASS` | `02a-kb-roster-icu.png` |
| `UAT-PK-03` | Ganti Petugas Indra → Raka, Pagi 16 Feb 2027, "Petugas sakit mendadak" | Tersimpan (DB: pemilik shift Raka); roster tetap Terbit; Riwayat Revisi 6 → 7 baris ("Indra Setiawan → Raka Firmansyah"). **Kisi roster tetap menampilkan shift di baris Indra** | `FAIL` (temuan T1, backend) | `03a-ganti-petugas.png`, `03b-riwayat-revisi.png` |
| `UAT-PK-04` | Pengganti Didampingi tanpa perawat Mandiri (B7) | devYoga tidak punya pegawai Didampingi. Bukti harness BE `ReplacementTests` 11/11 (B7, 18 Nov 2026, diterima pemilik roadmap untuk `AT-HRD-PK-02`) + layar `FE-HRD-307` (API tiruan); jalur tampilan 422 yang sama terbukti dengan data asli di `UAT-PK-05` | `PASS` (harness) | Laporan `BE-HRD-338`, `FE-HRD-307` |
| `UAT-PK-05` | Raka (Temporary IGD 1–14 Mar) ditambahkan ke ICU 5 Mar lewat layar | "B3: VR-PK-01 (B3): Raka Firmansyah ditempatkan di Instalasi Gawat Darurat 1–14 Maret 2027."; tidak ada shift tersimpan | `PASS` | `05-tolak-penempatan-igd.png` |
| `UAT-PK-06` | Tambah lalu hapus Siang 21 Feb Raka lewat layar dengan alasan | Sel kosong; shift `Cancelled`; roster tetap Terbit; riwayat `AddShift` + `RemoveShift` dengan alasan | `PASS` | `06a-…`, `06b-hapus-siang-21feb.png` |
| `UAT-PK-07` | gilang shift 12.30–12.55 hari ini tanpa scan; diproses 12.27, 12.35, 12.56 (`POST attendance/processing/process`, setara job harian — job sistem tidak berjalan karena `SystemActorUserId` kosong) | 12.27 dan 12.35: "Belum Hadir"; 12.56: "Tidak Ada Scan"; tidak ada label Alpa | `PASS` | `07-berjalan.png`, `07-tengah-shift.png`, `07-selesai.png` |
| `UAT-PK-08` | gilang Pagi 17 Nov 07.00, scan 07.12; toleransi diubah di layar Setelan | 10 → "Terlambat 12 menit"; 15 → tepat waktu; kembali 10 → terlambat 12 | `PASS` | `08a`–`08d` |
| `UAT-PK-09` | Periode ICU 17 Nov; gilang "Belum Lengkap"; pratinjau lalu Tutup lewat layar | Pratinjau menampilkan gilang "Belum Lengkap" dan "Tutup & Konversi 1 Hari"; **penutupan diblokir** "Masih terdapat attendance exception yang memblokir payroll" (pengecualian `MissingCheckOut` milik hari itu sendiri) | `FAIL` (temuan T2, backend) | `09a-pratinjau-tutup.png` |
| `UAT-PK-10` | 2 scan gilang 18 Nov dari `UJI-A52-GDB-316` (belum terdaftar); IT mendaftarkan dari baris Mesin Tak Dikenal | Sebelum: scan `AwaitingDeviceRegistration`, rekap "Tidak Ada Scan", baris tak dikenal 2 scan. Formulir terisi otomatis (kode, ID eksternal, nama). Sesudah: scan `Matched`, job `ATT-DEVREL-…` `Completed`, rekap "Hadir" 20.55–07.02 tanpa scan ulang | `PASS` | `10a`–`10e` |
| `UAT-PK-11` | Hari libur nasional uji 24 Nov dicatat lewat layar Hari Libur | Dewi (jadwal kantor) "Hari Libur"; gilang (roster) tetap "Tidak Ada Scan"; sesudah dihapus Dewi kembali "Belum Hadir" | `PASS` | `11a`–`11c` |
| `UAT-PK-12` | gilang membuka Jadwal Saya (November) dan Roster Unit | Jadwal Saya: 17 Nov Pagi, 18 Nov Malam "Berubah — sebelumnya Siang (UJI-A52) 14.00–21.00", 24 Nov Uji Singkat; hari lain "Tidak Ada Jadwal Terbit". Roster Unit: hanya roster terbit ICU (`ROST-202611-0004`, `-0005`, `UJI-A52-316`, `UJI-A52-MYSCH`); Arif yang hanya ada di roster Draft tidak tampil; Raka 17 Nov (tugas belajar) "Libur" tanpa alasan; Desember "Belum ada roster terbit" | `PASS` (sesudah perbaikan P2) | `12a`–`12d` |
| `UAT-PK-13` | gilang memanggil `my-schedule?…&workforceProfileId=<Raka>` | `200`, hanya jadwal gilang; respons tidak memuat ID Raka | `PASS` | Log uji |
| `UAT-PK-14` | Superadmin: Keterlambatan ICU 17 Nov (terbuka) dan Ketidakhadiran ICU 1–7 Okt (ditutup). Kepala Unit (gilang): Keterlambatan 17 Nov tanpa pemilih unit | Baris berkas = layar (1 = 1, 4 = 4, Kepala Unit 1 = 1); kepala berkas memuat unit/cakupan, rentang, status periode; sakit hanya "Sakit"; tiga log `[AUD] AttendanceReport.Export` (Kepala Unit: "unit: Semua unit dalam cakupan") | `PASS` | `14a`–`14c`, berkas `.xlsx`, `be.log` |
| `UAT-PK-15` | Kepala Unit ICU mengekspor laporan IGD | `403` "Anda tidak berwenang melihat data unit ini", tanpa berkas | `PASS` | Log uji |

---

## 3. Temuan

| ID | Temuan | Bukti | Dampak | Tindak lanjut |
| --- | --- | --- | --- | --- |
| T1 | **Kisi roster menampilkan shift hasil Ganti Petugas di baris petugas lama** (backend). `GET roster-periods/{id}` mengelompokkan `shiftAssignments` per `TrxRosterAssignment` (baris awal), dan butir shift tidak membawa `workforceProfileId`/nama petugas saat ini. Contoh: Malam 14 Feb milik Arif tampil di baris Bagas; Pagi 16 Feb milik Raka tampil di baris Indra; Arif tidak punya baris | DB `TrxShiftAssignment.WorkforceProfileId` ≠ `TrxRosterAssignment.WorkforceProfileId`; respons API; `03a`, `06b` | Kepala Unit melihat petugas yang salah; membuka Ganti Petugas lagi pada sel itu mengirim `expectedWorkforceProfileId` petugas lama → 409 | Usulan task backend: butir shift memuat petugas saat ini (atau dikelompokkan per petugas saat ini) + task frontend menampilkan pengganti di kisi |
| T2 | **Tutup periode terblokir oleh hari yang seharusnya dikonversi** (backend). `AttendancePeriodService` menolak tutup bila ada pengecualian `IsPayrollBlocking` berstatus Open; setiap hari "Belum Lengkap" membawa `MissingCheckOut` (High, memblokir payroll). Bertentangan dengan `HRD-DEC-216` (Belum Lengkap → Alpa saat tutup) | `AttendancePeriodService.cs:675`, `:801`; `09a` | `UAT-PK-09` tidak dapat dijalankan dengan data nyata tanpa menyelesaikan pengecualian satu per satu | Usulan task backend: pengecualian kehadiran milik calon konversi (`MissingCheckOut`/`MissingCheckIn`) tidak dihitung sebagai pemblokir tutup |
| T3 | Satu kali alert "Gagal memuat rekapitulasi presensi harian." pada muatan pertama layar monitoring (dev, kompilasi pertama); tidak tereproduksi pada 10 muatan berikutnya, tanpa galat di log backend | `00-monitoring-awal.png` | Rendah | Pantau |
| T4 | Sel Roster Unit (Jadwal Saya, `FE-HRD-314`) menampilkan kode sistem master shift "SFT-RSMMC-00003" yang terpotong tiga baris; API tiruan memakai kode pendek sehingga tidak terlihat | `12b-roster-unit.png` | Sel tidak terbaca | **Diperbaiki (P2)** |
| T5 | Kisi roster terbit menampilkan petunjuk "Klik sel bershift untuk mengganti petugas…" kepada Kepala Bidang yang hanya boleh membaca | `02a-kb-roster-icu.png` | Petunjuk menyesatkan | **Diperbaiki (P1)** |

### 3.1 Perbaikan frontend dari temuan

| ID | Berkas | Perubahan | Verifikasi |
| --- | --- | --- | --- |
| P1 | `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx`; `tests/e2e/hr-roster-published-change.spec.mjs` | Roster terbit tanpa hak ganti/tambah/hapus: petunjuk menjadi "Kisi hanya dapat dibaca: Anda tidak berwenang mengubah roster ini." | Spec roster terbit 10/10 (skenario Kepala Bidang ditambah pemeriksaan teks) |
| P2 | `src/utils/hr/self-service/my-schedule-utils.js`; `tests/unit/hr-my-schedule-utils.test.mjs`; `tests/e2e/hr-my-schedule.spec.mjs` | Label sel = dua huruf awal nama shift (sama dengan kisi roster HR `shiftShortLabel`); kode sistem pindah ke tooltip | Data asli: sel Nadia "SI"/"PA", tooltip "Siang (UJI-A52) · SFT-RSMMC-00003" (`12d`); unit 6/6; spec Jadwal Saya 4/4 |

Build produksi berhasil; `npm run lint:errors` (seluruh repository) tanpa error; `npx eslint` atas berkas yang diubah tanpa error maupun peringatan.

---

## 4. Izin dan pemulihan

Superadmin dipakai untuk penyiapan data dan alur positif. Kasus cakupan/403 memakai gilang dengan izin sementara pada
jabatan Penunjang Medis / Analis Laboratorium (backup set Role Access, kirim ulang identik — diperiksa ulang 32 butir) dan
penempatan berjabatan sementara (dihapus lunak di akhir). Butir DoD 40.19 "hak akses baru sudah diberikan ke jabatan sesuai
40.14" **tetap terbuka** sesuai keputusan pengguna.

---

## 5. Acceptance criteria

| Kriteria | Status |
| --- | --- |
| Semua UAT MVP lulus | Belum: `UAT-PK-03` dan `-09` `FAIL` karena temuan backend T1, T2; 13 UAT lain lulus (`-04` lewat harness) |
| Akun non-superadmin untuk kasus 403 | Terpenuhi: `UAT-PK-01`, `-02`, `-13`, `-15` memakai gilang |

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dependency backend | T1 dan T2 butuh task backend baru (di luar wewenang task frontend ini) |
| Skrip uji | Di `scratchpad/uat316` sesi ini (tidak masuk repository karena menulis ke devYoga dan butuh sandi); layar MVP sudah punya spec E2E API tiruan di `tests/e2e` (`FE-HRD-307` s.d. `315`) |
| Perubahan sampingan | `NONE` — keluaran Playwright diarahkan ke scratchpad |
| Status Git | Frontend: ` M src/components/view/hr/scheduling/roster-matrix-detail-view.jsx`, ` M src/utils/hr/self-service/my-schedule-utils.js` (berkas `FE-HRD-314`), ` M tests/e2e/hr-roster-published-change.spec.mjs`, spec/uji `FE-HRD-314` diperbarui; ditambah berkas `FE-HRD-313`–`315`. Backend: laporan ini (baru), roadmap. Tidak ada commit |
