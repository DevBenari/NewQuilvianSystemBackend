# Laporan Perubahan Frontend — `FE-HRD-400`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-400` |
| Judul | Butir menu Paket B dan label "Tindak Disiplin" |
| Slice | `S-PKB` / `MVP-74` · `EPIC HRD-129` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 30 |
| Trace | `FR-PKB-19`, `FR-PKB-20` (serta sisi frontend `FR-PKB-04`); `HRD-DEC-231`, `HRD-DEC-239`; frontend 34.1; `VR-PKB-11`; Permission 33.1; `AT-HRD-PKB-09`, `AT-HRD-PKB-23`, `AT-HRD-PKB-25` |
| Contract version | `v29`, disetujui 23 November 2026 |
| Wewenang UI | Frontend 34.1 (`pathname` mengikat; urutan dan ikon `DEV_DISCRETION`) |
| Dependency | `FE-HRD-396` ✅, `FE-HRD-397` ✅, `FE-HRD-398` ✅, `FE-HRD-399` ✅ |
| Klasifikasi | `STANDARD` — satu berkas menu dan suite unitnya |
| Task mode | `CROSS-REPO` (source frontend; laporan dan tanda status di repository backend) |
| Target tulis | `menu-items.jsx`, `hr-master-data-sidebar-grouping.test.mjs`, spec Playwright Paket B; laporan, roadmap, traceability HRD |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `6c05df212` (branch `HamzahV2`); hasil task di-commit pengguna sebagai `6c1d4e368` |
| Commit backend yang dijadikan rujukan | `a9c1af31` (branch `MHamzah`) |
| Tanggal | 23 November 2026 |
| Status | 🟡 Sebagian — dua `pathname` ganda bawaan di luar Paket B masih ada (bagian 8) |

---

## 1. Keadaan yang ditemukan di awal

Menu SDM belum memuat Dashboard SDM, Seleksi Administrasi, Tes/Wawancara, dan Riwayat Penyelesaian; butir `corporateRelationsSanctions` masih berlabel "Teguran". Uji HR-08 mengunci label "Teguran".

Pemeriksaan seluruh daun menu SDM pada HEAD menemukan duplikat yang sudah ada sebelum task ini:

| Duplikat | Butir | Asal |
| --- | --- | --- |
| `key` `corporateTrainingCertificates` dan `pathname` `/corporate/hrd/learning/certificates` | Kompetensi & Kredensial → "Sertifikasi Kompetensi"; Diklat → "Registri Portofolio e-Sertifikat" | Adapter sementara `FE-HRD-384` 🟡 (`FIN-HRD-09`, "READY TO REUSE") |
| `pathname` `/hr/master-data/competency` | Master Data → "Kompetensi"; Kompetensi & Kredensial → "Persyaratan Jabatan" | Adapter sementara `FE-HRD-385` 🟡 (`FIN-HRD-09`, "REUSE WITH ADAPTER") |

---

## 2. Proses bisnis dari sisi pengguna

Rina (Manajer SDM) melihat **Dashboard SDM** tepat di bawah Master Data pada menu Sumber Daya Manusia. Dodi (Recruiter) melihat **Seleksi Administrasi** dan **Tes/Wawancara** di Rekrutmen & Seleksi, sesudah Pelamar. Petugas HR Relations melihat **Tindak Disiplin** (dahulu "Teguran", alamat tetap layar Sanksi) dan **Riwayat Penyelesaian** di Hubungan Industrial & Terminasi. Setiap butir hanya tampil bila pengguna memegang hak layar tujuannya; akun tanpa `HrdWorkforceDashboard : Read` tidak melihat Dashboard SDM. Teguran Lisan dicari lewat filter Tingkat di layar Tindak Disiplin (`HRD-DEC-239`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/utils/menu-sidebar/menu-items.jsx` dan `filter-menu-items-by-permission.js`
- `src/components/features/left-sidebar/left-sidebar.jsx`, `left-sidebar-items-virtualized.jsx` (daun di `subMenu`, `aria-current`, lebar desktop ≥ 1300 px)
- `tests/unit/hr-master-data-sidebar-grouping.test.mjs`
- Frontend 34.1, `acceptance-test-matrix.md` (`AT-HRD-PKB-09`, `23`, `25`), `PLANNING_PENYESUAIAN_MENU_SDM_BERDASARKAN_ANALISIS_BISNIS.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir baru: Dashboard SDM (tingkat 1, `HrdWorkforceDashboard : Read`), Seleksi Administrasi (`CandidateScreening : Read`), Tes/Wawancara (`CandidateEvaluation : Read`), Riwayat Penyelesaian (`EmployeeDisciplinary : ViewSanctions`). Label "Teguran" → "Tindak Disiplin" dengan `key`, `pathname`, dan hak tetap. `key` butir "Sertifikasi Kompetensi" diganti menjadi `corporateCompetencyCertifications` agar unik; label, alamat, dan hak tidak berubah. Ikon memakai impor yang sudah ada. |
| `tests/unit/hr-master-data-sidebar-grouping.test.mjs` | HR-08 kini mengunci label "Tindak Disiplin". Uji baru HR-09 (key unik di seluruh menu SDM, butir Paket B lengkap dengan hak dan `page.jsx` nyata, pathname ganda hanya boleh dua adapter yang tercatat), HR-10 (tidak ada "Teguran"; satu pintu Tindak Disiplin), HR-11 (penyaringan hak Dashboard SDM). |
| `tests/e2e/human-resource-package-b.spec.mjs` | Dua skenario Playwright sidebar desktop (viewport 1440). |

### 3.3 Kepatuhan arsitektur frontend

Hanya data menu dan uji yang berubah. Visibilitas memakai `requiredPermission` dan `filterMenuItemsByPermission` yang sudah ada; tidak ada komponen sidebar yang diubah.

### 3.4 Gerbang tampilan

**Modul referensi visual:** butir daun tingkat 1 "Beranda Akuntansi" (`corporateAccountingHome`) dan butir Hubungan Industrial yang sudah ada.

| Elemen | Sumber | Keputusan | Alasan |
| --- | --- | --- | --- |
| Butir menu dan ikon | Struktur `menu-items.jsx`, ikon `react-icons/ri` yang sudah diimpor | `REUSE` | Bentuk butir yang sama dengan butir lain. |
| Penyaringan hak | `filterMenuItemsByPermission` | `REUSE` | Tanpa perubahan. |

`UI GATE: PASS — 2 REUSE, 0 EXTEND, 0 COMPOSE, 0 NEW.`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs src/utils/menu-sidebar/menu-items.jsx
OK — 1 berkas diperiksa, 0 temuan.
```

Grep anti-regresi bagian G: `menu-items.jsx` adalah data menu tanpa elemen tampilan baru; empat butir baru memakai `className="fs-4"` pada ikon, konvensi yang sama dengan seluruh butir lain di berkas ini.

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hr/dashboard` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-400-menu-dashboard-sdm.png` | Dashboard SDM aktif di bawah Master Data. |
| `/hr/recruitment/screenings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-400-menu-rekrutmen.png` | Seleksi Administrasi aktif; Tes/Wawancara di bawahnya; butir rekrutmen lain tersembunyi karena akun uji tidak memegang haknya. |
| `/hrd/relations/sanctions/history` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hr-paket-b-1123/FE-HRD-400-menu-hubungan-industrial.png` | Tindak Disiplin dan Riwayat Penyelesaian (aktif); tidak ada "Teguran". |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memegang hak layar tujuan | Butir tampil dan aktif saat layar dibuka (`aria-current="page"`). |
| Tidak memegang hak | Butir tidak dirender. |
| Pencarian menu "Teguran" | Tidak ada butir yang cocok. |

---

## 5. Endpoint yang dikonsumsi

Tidak ada. Butir menuju route yang sudah ada: `/hr/dashboard` (`FE-HRD-399`), `/hr/recruitment/screenings` (`FE-HRD-396`), `/hr/recruitment/evaluations` (`FE-HRD-397`), `/hrd/relations/sanctions/history` (`FE-HRD-398`), `/hrd/relations/sanctions` (layar existing).

---

## 6. Verifikasi

| Skenario/perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --test tests/unit/hr-master-data-sidebar-grouping.test.mjs` | 11/11 lulus | `PASS` | HR-01 s.d. HR-11 |
| `npm run test:unit` (suite penuh) | 5.187 test, 5.114 lulus, 73 gagal; tidak ada kegagalan yang disebabkan task ini | `PASS` untuk cakupan task; suite penuh `FAIL` bawaan | Bagian 6.1 |
| `npm run lint:errors` | Exit code `0` | `PASS` | ESLint seluruh repository |
| `npm run build` | Exit code `0`; 894 halaman statis | `PASS` | Build yang sama dengan `FE-HRD-399` |
| Pemeriksa CSS | 1 berkas, 0 temuan | `PASS` | Bagian 3.4 |
| Playwright Chromium headed, viewport 1440 | 2/2 lulus | `PASS` | Butir aktif di tiga layar, "Teguran" tidak ada, Dashboard SDM tersembunyi tanpa hak |

`AUTOMATED TEST: npx playwright test tests/e2e/human-resource-package-b.spec.mjs --grep "FE-HRD-400" --headed --workers=1 — PASS (2/2)`

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/hr-master-data-sidebar-grouping.test.mjs — PASS (11/11)`

`AUTOMATED TEST: npm run test:unit — FAIL (73 gagal bawaan, 0 baru)`

### 6.1 Pembanding suite unit penuh

Suite yang sama dijalankan pada salinan HEAD `6c05df212` (git worktree sementara di `%TEMP%\qfh2`, dihapus sesudahnya) dan dibandingkan per nama test:

| Keadaan | Jumlah test | Gagal |
| --- | ---: | ---: |
| HEAD `6c05df212` | 5.179 | 84 |
| Berkas kerja (sesudah `FE-HRD-393`, `399`, `400`) | 5.187 | 73 |

- 69 kegagalan sama persis di kedua keadaan (billing, rawat inap, payroll, lab, logistik, hemodialisa, master data administrator, dan lain-lain); tidak satu pun berada di suite menu SDM atau Paket B.
- 4 test hanya gagal di berkas kerja: `billing-single-payment-action`, `bpjs-feature-deactivation`, `payroll-policy`, dan `payroll-simulation`. Keempatnya memeriksa `existsSync(../NewQuilvianSystemBackend/...)`; di worktree sementara backend tidak bersebelahan sehingga test itu dilewati, sedangkan di berkas kerja test dijalankan terhadap source backend. Isinya (penyelesaian tagihan, saklar BPJS, PPh 21) tidak menyentuh berkas task ini.
- 15 test hanya gagal di HEAD karena alasan sebaliknya (membutuhkan source backend yang tidak ada di worktree sementara).
- 8 test tambahan di berkas kerja adalah test baru task ini (HR-09, HR-10, HR-11, dan 5 test Dashboard SDM); semuanya lulus.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC1 — Setiap `pathname` dan `key` menu SDM unik; tidak ada butir menuju placeholder (`AT-HRD-PKB-25`) | **Sebagian** | `key` unik di seluruh menu SDM (HR-09), keempat butir Paket B punya `page.jsx` nyata. Dua `pathname` ganda bawaan adapter `FE-HRD-384`/`385` masih ada (bagian 8). |
| AC2 — Butir tampil hanya dengan hak layar tujuan; tanpa `HrdWorkforceDashboard : Read` Dashboard SDM tidak tampil (`AT-HRD-PKB-09`) | Terpenuhi | HR-11; skenario headed 2 |
| AC3 — "Tindak Disiplin" membuka daftar sanksi; tidak ada butir "Teguran" (`AT-HRD-PKB-23`) | Terpenuhi | HR-10; skenario headed 1 |
| AC4 — Urutan dan ikon `DEV_DISCRETION` | Terpenuhi | Mengikuti sketsa 34.1 |
| Uji anti-duplikasi menu diperbarui | Terpenuhi | HR-08 s.d. HR-11 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Belum terpenuhi | `AT-HRD-PKB-25` meminta `pathname` unik di seluruh menu SDM. `/corporate/hrd/learning/certificates` (Sertifikasi Kompetensi + Registri Portofolio e-Sertifikat) dan `/hr/master-data/competency` (Kompetensi + Persyaratan Jabatan) masih dipakai dua butir. Keduanya adapter sementara task lain; menghapus salah satu pintu adalah keputusan produk, bukan wewenang task ini. HR-09 mengizinkan tepat dua duplikat ini dan gagal bila muncul duplikat baru. |
| Peringatan | `key` "Sertifikasi Kompetensi" berganti. Sidebar hanya menyimpan key grup yang terbuka di `sessionStorage`; butir ini daun, sehingga keadaan tersimpan tidak terdampak. |
| Dependency backend | Tidak ada (hak `HrdWorkforceDashboard : Read` sudah terdaftar oleh `BE-HRD-448`). |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Di-commit pengguna sebagai `6c1d4e368` (23 November 2026, 15.17 WIB). |
| Langkah berikutnya | Keputusan pengguna atas dua pintu ganda, lalu `FE-HRD-401`. |
