# Laporan Perubahan Backend — `BE-HRD-468`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-468` |
| Judul | Verifikasi `A52-B` |
| Slice | `A52-B` / Verifikasi · `MVP-77` gelombang 4 |
| Roadmap | `roadmap/backend-roadmap.md` bagian 32, kartu `BE-HRD-468`, Grafik B102 |
| Trace | `FR-PK-033` s.d. `FR-PK-038`; `HRD-DEC-254`; `AT-HRD-PK-47`, `-48`, `-57` |
| Contract version | `v31` — Acceptance 40; Permission 35.1, 35.3; PRD 44.11 — approved 24 November 2026 |
| Dependency | `BE-HRD-359` ✅, `BE-HRD-463` ✅, `BE-HRD-464` ✅, `BE-HRD-465` ✅, `BE-HRD-466` ✅, `BE-HRD-467` ✅; G2 ✅ (lihat di bawah) |
| Gerbang G2 | **Diberikan 26 November 2026** oleh Pengguna (Yoga) untuk menulis data uji `UJI-A52` di devYoga bagi `AT-HRD-PK-47` dan `-48`, termasuk menutup periode uji (jawaban: "Izinkan keduanya") |
| Klasifikasi | `MEDIUM` — verifikasi; tambahan uji registri; bukti API devYoga dengan data uji. **Tidak ada perubahan source aplikasi** |
| Task mode | `BACKEND` (verifikasi; instruksi pengguna 26 November 2026: "lanjut kerjakan BE-HRD-468 yaa") |
| Target tulis | `Tests/QuilvianSystemBackend.HumanResourceTests/A52PermissionRegistryTests.cs`, `AcceptanceTestIndex.md`, laporan ini, roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `cce1dbf5` (branch `yoga`) + perubahan `BE-HRD-467` yang belum di-commit |
| Tanggal | 26 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi. Butir DoD 44.11 "hak akses sudah diberikan" tetap terbuka (G3, wewenang Admin Sistem) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate`, `SelfServices` |
| Module / Submodule | `HumanResource` / `SchedulingManagement`, `AttendanceManagement`, `WorkforceCore` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | Verifikasi; tidak ada kode aplikasi baru. Uji baru hanya di proyek uji |
| QBE yang berlaku | `QBE-PERM-001` (registri tujuh hak, validator startup, dan 403 non-superadmin pada 14 endpoint A52-B). `QBE Strict` dijalankan ulang atas 22 berkas source slice |
| Tidak berlaku | QBE penulisan kode lain, karena task ini tidak mengubah source aplikasi |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `CLAUDE.md` backend |
| Wewenang terpisah | Source aplikasi ✖ (tidak diubah); migration ✖ (tidak ada); eksekusi database: hanya penulisan data uji lewat API backend lokal atas izin G2, ditambah kueri baca-saja untuk bukti; deployment ✖ |

---

## 1. Yang diverifikasi

| AC | Isi | Hasil |
| --- | --- | --- |
| 1. Tujuh string Permission 35.1 terdaftar saat startup | Harness: registri memuat **persis** tujuh string (modul + resource + aksi), tidak lebih. Tiga controller A52-B lolos `PermissionRegistryValidator`, dan 14 endpointnya menolak pegawai tanpa kebijakan Akses Role. Startup backend lokal ke devYoga: "Permission registry valid. 2713 identitas kanonik dari 2713 kemampuan terdaftar."; ketujuh aksi tersinkron aktif di `SysActionAccess` | Terpenuhi |
| 2. Regresi proyek uji HR tidak bertambah gagal dari baseline | 480/480 lulus (baseline `BE-HRD-467` 458/458 + 22 uji baru) | Terpenuhi |
| 3. Bukti devYoga: shift pengganti tampil di baris pengganti; periode uji dengan rekap Belum Lengkap tertutup | Roster `ROST-202611-0002` dan periode `ATP-202611-0001` (ulang `UAT-PK-09`) lewat API lokal | Terpenuhi (rincian bagian 3) |

Daftar periksa Permission 35.3 menyatakan "daftar ini diperiksa ulang terhadap registri saat verifikasi". Seluruh string per peran (Kepala Unit, Kepala Bidang, Admin HRD, Petugas HR Scheduling, Petugas IT, Pegawai) ternyata ada di registri, termasuk nama aksi `AttendanceDashboard : Read`.

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Tests/QuilvianSystemBackend.HumanResourceTests/A52PermissionRegistryTests.cs` | Tiga uji baru: `Registri_memuat_persis_tujuh_hak_akses_A52B`, `Controller_A52B_lolos_PermissionRegistryValidator` (14 endpoint terlihat validator), `Daftar_periksa_peran_35_3_seluruhnya_terdaftar` (6 peran). `EndpointA52Baru` ditambah tiga controller A52-B, sehingga uji 403 non-superadmin mencakup 14 endpoint baru |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AcceptanceTestIndex.md` | Baris `AT-HRD-PK-57`. Bukti devYoga `AT-HRD-PK-47`/`48` merujuk laporan ini. Baris `AT-HRD-PK-49` s.d. `52` dikoreksi: sebelumnya menulis "bukti API devYoga di `BE-HRD-468`", padahal kartu hanya mewajibkan `AT-HRD-PK-47`/`48`. Baris kelas untuk tiga uji baru |

Tidak ada perubahan source aplikasi, migration, maupun konfigurasi.

---

## 3. Bukti devYoga (atas izin G2)

Backend lokal `https://localhost:7184` (Development, `--launch-profile QuilvianSystemBackend`) dengan akun superadmin seed. Skrip `a468.mjs` dan log `a468-log.jsonl` disimpan di scratchpad sesi (`uji468/`).

### `AT-HRD-PK-47` — kisi roster menampilkan pengganti (ulang `UAT-PK-03`)

Data: roster ICU `ROST-202611-0002` (Februari–Agustus 2027, `Published`), data uji `UJI-A52` dari `BE-HRD-337`/`338` dan UAT `FE-HRD-316`. Pada UAT itu, Pagi 16 Februari sudah diganti Indra → Raka, dan Malam 14 Februari serta Pagi 17 Februari sudah diganti ke Arif. Arif tidak punya penugasan di roster ini. Raka punya baris penugasan sendiri (dibuat saat UAT-PK-06) tanpa shift lain.

| Pemeriksaan | Hasil |
| --- | --- |
| `GET roster-periods/{id}` tanpa sesi | `401` |
| `GET roster-periods/{id}` | `200`. Baris **Arif Maulana**: `isReplacementOnly = true`, `id = 00000000-…`, `assignmentStatus = Published`, `plannedShiftCount = 2`, berisi Malam 14 Feb (`isReplacement = true`, asal Bagas Saputra) dan Pagi 17 Feb (asal Indra Setiawan). Baris **Raka Firmansyah** (punya penugasan sendiri): Pagi 16 Feb `isReplacement = true`, `originalWorkforceDisplayName = "Indra Setiawan"`. Baris **Indra Setiawan** tetap ada tanpa shift 14–18 Feb |
| Ganti Petugas Pagi 16 Feb dengan `expectedWorkforceProfileId` = Indra (petugas awal, sudah usang) | `409` "Shift ini baru saja diubah orang lain. Muat ulang lalu coba lagi." Baris shift tidak berubah (petugas tetap Raka, `UpdateDateTime` sama) |

Jalur "Ganti Petugas berikutnya dengan `expectedWorkforceProfileId` = petugas sekarang diterima" tidak diulang di devYoga, supaya riwayat revisi roster uji tidak bertambah. Jalur ini dibuktikan di harness `RosterReplacementGridTests.Ganti_berikutnya_dari_sel_pengganti_memakai_petugas_sekarang`.

### `AT-HRD-PK-48` — tutup periode dengan rekap Belum Lengkap (ulang `UAT-PK-09`)

Data: periode `ATP-202611-0001` (ICU, 17 November 2026, `Open`) dari UAT `FE-HRD-316`. Rekap **Gilang Mahendra** 17 November adalah hasil pemrosesan sungguhan (`ScheduleSource=PublishedRoster; RawPunch=1`): status `IncompletePunch` dengan pengecualian `MissingCheckOut` (`Open`, memblokir payroll) dan `Late` (`Open`, tidak memblokir). Inilah kasus yang dulu menghalangi tutup pada UAT-PK-09 dan memicu `HRD-DEC-249`. Data sungguhan ini dipakai sehingga tidak perlu membuat periode baru.

| Pemeriksaan | Hasil |
| --- | --- |
| `GET periods/{id}/close-preview` | `200`, `canClose = true`, `conversionCandidateCount = 1`, `conversionExceptionCount = 1`, `openPayrollBlockingException = 0`. Isu yang tersisa hanya `INCOMPLETE_PUNCH_ATTENDANCE` (Warning, tidak memblokir) |
| `POST periods/{id}/close` | `200` "…dikonversi 1 hari Belum Lengkap/Tidak Ada Scan menjadi Absent, dan ditutup 1 pengecualian presensi pada hari tersebut.", `closedConversionExceptionCount = 1`, `currentStatus = Closed` |
| Rekap sesudah tutup | `Absent`, `IsAbsent = true`, `IsLocked = true` |
| Pengecualian sesudah tutup | `MissingCheckOut` menjadi `Closed`, `ResolvedByUserId` terisi, `ResolvedAt` = jam tutup, catatan "Diselesaikan oleh penutupan periode ATP-202611-0001 (HRD-DEC-249): hari dikonversi menjadi Absent". `Late` (bukan tipe konversi) tetap `Open` |
| Log aplikasi | `AttendancePeriod.Close`: "Menutup attendance period ATP-202611-0001: 1 hari dikonversi menjadi Absent, 1 pengecualian konversi ditutup (HRD-DEC-249)." |
| Tutup ulang | `409` "Attendance period hanya dapat ditutup dari status Open atau Reopened." |

Pada data asli tidak ada `ScheduleConflict` maupun kontrol negatif `MissingCheckOut` di hari yang bukan calon konversi. Bagian AT itu dibuktikan di harness (`AttendancePeriodCloseConversionExceptionTests.Pengecualian_lain_tetap_memblokir_tutup`). Pengecualian semacam itu tidak sengaja disisipkan ke devYoga.

### Data devYoga yang berubah

| Data | Perubahan |
| --- | --- |
| Periode `ATP-202611-0001` (`UJI-A52 FE-HRD-316`) | `Open` → `Closed`; rekap Gilang 17 November `Absent` dan terkunci; pengecualian `MissingCheckOut` `Closed` |
| Roster `ROST-202611-0002` | Tidak berubah (permintaan 409 ditolak tanpa tulis) |
| Registri hak akses | Startup menyinkronkan resource `MyEarlyLeavePermission`, `MyOfficialDuty`, `TemporaryAssignment` beserta tujuh aksinya (perilaku startup biasa). Tidak ada kebijakan Akses Role yang diubah |

---

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `A52PermissionRegistryTests` | 108/108 lulus (termasuk 3 uji baru dan 14 kasus 403 baru) | `PASS` (AC1) | `reg468.log` di scratchpad sesi |
| Kontrol negatif registri: 3 mutasi (aksi `TemporaryAssignment` diganti nama, `MyOfficialDuty : Cancel` → `Update`, modul ESS izin salah) | 3/3 tertangkap; hash SHA-256 ketiga controller sama dengan sebelum mutasi | `PASS` | `mut468.cjs`, `mut468-hasil.log`, `hash468.txt` di scratchpad sesi |
| Regresi penuh proyek uji HR | 480/480 lulus, 20 menit 44 detik | `PASS` (AC2) | `regresi468.log` (salinan proyek uji tanpa empat berkas `WorkforcePayrollData*Tests.cs` yang tidak terkompilasi sejak `815ce085`) |
| Startup backend lokal ke devYoga | "Permission registry valid. 2713 identitas kanonik dari 2713 kemampuan terdaftar." Tujuh aksi A52-B aktif di `SysActionAccess`. Galat scheduler "`SystemActorUserId` belum dikonfigurasi" adalah kondisi lama devYoga, bukan dari slice ini | `PASS` (AC1) | `uji468/be468.log`, `a468-log.jsonl` |
| Bukti API devYoga `AT-HRD-PK-47`, `-48` | Lihat bagian 3 | `PASS` (AC3) | `a468-log.jsonl` |
| QBE `Strict` atas 22 berkas source A52-B (controller, DTO, service, helper dari `BE-HRD-359`, `463`–`467`) | 0 violation, 0 review, `PASS`. Probe `TrxZzzQbeProbe468` (ber-`[Table]`) terdeteksi 4 violation (`QBE-NAM-001`, `-ENT-001`, `-CFG-001`, `-MOD-002`) lalu dihapus | `PASS` | Keluaran checker |

---

## 5. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: `plannedShiftCount` pada baris penugasan milik sendiri adalah kolom tersimpan yang menghitung shift menurut penugasan asal, sehingga tidak ikut berubah saat Ganti Petugas. Di devYoga, baris Indra menulis 6 padahal dua shiftnya kini dipegang orang lain; baris Raka menulis 0 padahal memuat shift 16 Feb. Baris pengganti-saja menghitung dari shift yang tampil (`BE-HRD-463`) | Angka "jumlah shift" per baris di layar roster bisa berbeda dengan sel yang terlihat | Di luar AT-47 dan kontrak 37.2. Dicatat untuk `FE-HRD-411`/pass blueprint: tentukan apakah angka per baris harus dihitung dari shift yang tampil |
| Indeks uji menulis "bukti API devYoga di `BE-HRD-468`" untuk `AT-HRD-PK-49` s.d. `52` | Klaim bukti yang tidak ada | Dikoreksi. Bukti data asli ESS dan cakupan butuh akun pegawai/Kepala Unit berizin, dan menyusul uji layar `FE-HRD-410`/`411` (`AT-HRD-PK-53` s.d. `55`) |
| Hak ketujuh string belum dipegang peran mana pun | Pegawai dan Kepala Unit belum bisa memakai layar baru | G3: Admin Sistem memberikannya lewat Akses Role mengikuti daftar periksa 35.3 (seluruh string sudah dipastikan ada di registri) |
| Celah kontrak dari `BE-HRD-466`/`467` (alasan batal tidak disimpan, rujukan "36.4" di API 37.5, status API 37.4/37.5 "Rencana") | Dokumen kontrak tertinggal dari implementasi | Tetap dicatat untuk pass blueprint; kontrak terkunci hash tidak disunting |

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Penulisan devYoga hanya lewat API backend lokal atas izin G2 (bagian 3). Kueri langsung hanya baca-saja (`BEGIN READ ONLY`) |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: laporan ini. Berkas diubah: `A52PermissionRegistryTests.cs`, `AcceptanceTestIndex.md`, roadmap, traceability |
| Server lokal | Backend lokal dinyalakan untuk bukti lalu dihentikan |
| Dampak frontend | Tidak ada perubahan API. Temuan `plannedShiftCount` relevan untuk `FE-HRD-411` |
| Sisa DoD `A52-B` (PRD 44.11) | Butir backend selesai. Yang masih terbuka: G3 (hak diberikan Admin Sistem) dan task frontend `FE-HRD-410` s.d. `417`, `FE-HRD-100`, `FE-HRD-324` |
| Task berikutnya | Frontend `A52-B` (`FE-HRD-410` dan seterusnya); di backend tidak ada task `A52-B` tersisa |
