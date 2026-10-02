# Laporan Perubahan Frontend — `FE-HMD-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-01` |
| Judul | Integrasi Resolver Sidebar, Peta Butir Menu, dan Routing Modul Hemodialisa |
| Slice | `MVP-0` — Navigasi, State Management, dan Fondasi Modul |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.1 |
| Trace | `CAP-38`, `CAP-39`, `03-frontend-architecture.md` Bagian 3; `contracts/permission-audit-matrix.md` Bagian 2 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk ikon menu (menggunakan `RiHeartPulseLine`, `RiDashboardLine`, `RiFileList3Line`, `RiUserLine`, `RiCheckboxCircleLine`, `RiDatabase2Line`), tata letak layout modul, dan salinan teks Bahasa Indonesia sesuai `page-composition-patterns.md` |
| Dependency | `BLOCKER`: Approval blueprint dan kontrak masukan `HMD-CONTRACT-v1` (terpenuhi) |
| Klasifikasi | `LIGHT` — skor 4: repository 0, berkas diperiksa 4, berkas diubah 3, logika 1, kontrak API 0, database 0, keamanan 1, UI 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/utils/menu-sidebar/menu-items.jsx`, `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx`, `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx`, `src/app/health-services/hemodialysis-management/**`, `tests/unit/hemodialysis-sidebar-navigation.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `f09426938` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `89028993` pada branch `MHamzah` |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis dan build Next.js sukses |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini dijalankan, modul Hemodialisa belum memiliki representasi menu aktif di antarmuka pengguna (`CAP-39`). Meskipun kunci `menuHemodialisa` telah dipesan sebelumnya pada resolver menu bersarang `src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:9`, pohon menu navigasi di `src/utils/menu-sidebar/menu-items.jsx` belum memuat entri untuk Hemodialisa.

Selain itu:
1. Struktur rute App Router di bawah `src/app/health-services/hemodialysis-management/` belum tersedia sama sekali, sehingga navigasi ke URL modul akan menghasilkan status 404 (Not Found).
2. Mekanisme penyaringan butir menu sidebar berdasarkan peran kerja (`filterMenuItemsByRole.jsx`) belum memetakan peran unit Hemodialisa (seperti Koordinator HD) vs peran unit luar (seperti Staf Bangsal rawat inap yang hanya berhak membuat permintaan).

Contoh akibatnya: Koordinator unit Hemodialisa tidak dapat mengakses ruang kerja unit melalui navigasi sidebar, dan tidak ada struktur layout baku yang dapat digunakan sebagai wadah rute-rute anak berikutnya.

---

## 2. Proses bisnis dari sisi pengguna

1. **Pengguna Masuk Sistem**: Koordinator unit Hemodialisa login ke sistem Quilvian dengan peran `Koordinator HD`.
2. **Navigasi Sidebar**: Di sidebar kiri, di bawah kelompok besar **Pelayanan Kesehatan**, muncul grup menu **Hemodialisa**.
3. **Membuka Submenu Operasional**: Koordinator dapat mengklik:
   - **Beranda Hemodialisa**: Mengarah ke `/health-services/hemodialysis-management` (ringkasan statistik dan status operasional).
   - **Permintaan Masuk**: Mengarah ke `/health-services/hemodialysis-management/orders` (antrean permintaan HD dari bangsal/IGD).
   - **Daftar Pasien Hemodialisa**: Mengarah ke `/health-services/hemodialysis-management/patients` (berkas program HD pasien).
   - **Jadwal & Daftar Kerja**: Mengarah ke `/health-services/hemodialysis-management/worklist` (jadwal per shift dan alokasi mesin).
   - **Kesiapan Unit**: Mengarah ke `/health-services/hemodialysis-management/unit-readiness` (lembar kelaikan operasional shift dan air).
4. **Membuka Grup Master Data**: Koordinator membuka grup tingkat 1 **Master Data**, yang memperluas 4 subitems:
   - **Mesin Hemodialisa**: Mengarah ke `.../master-data/machines`.
   - **Station Hemodialisa**: Mengarah ke `.../master-data/stations`.
   - **Butir Persiapan**: Mengarah ke `.../master-data/checklist-items`.
   - **Pengaturan Unit**: Mengarah ke `.../master-data/settings`.
5. **Jalur Pengguna Luar Unit (Staf Bangsal)**: Staf perawat atau dokter dari bangsal rawat inap yang hanya memiliki kewenangan membuat permintaan order (`HemodialysisOrder:Create`) tidak akan melihat menu internal operasional unit seperti **Kesiapan Unit** dan **Master Data**. Mereka hanya melihat antrean **Permintaan Masuk** untuk koordinasi pasien bangsal.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 3, 3.1, 3.2, 5, 7
- `docs/module-blueprints/hemodialisa/contracts/permission-audit-matrix.md` Bagian 2, 3
- `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` Bagian 4.1 (`FE-HMD-01`)
- `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx`
- `QuilvianSystemFrontendDev/src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx`
- `QuilvianSystemFrontendDev/src/components/features/left-sidebar/left-sidebar-menu-handle.jsx`
- `QuilvianSystemFrontendDev/src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Menambahkan pohon menu `healthServicesHemodialysisManagement` di bawah Pelayanan Kesehatan dengan 6 submenu (`subMenu`) dan 4 subitems Master Data (`subItems` dan `menuHemodialisa`), lengkap dengan pemetaan hak akses `permission` |
| `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx` | Memperbarui fungsi penyaringan menu untuk mendukung pemfilteran berbasis peran kerja (`role`) dan klaim hak akses pengguna (`userPermissions`), menyembunyikan menu internal unit bagi staf bangsal |
| `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx` | Menghubungkan Redux slice `authPermission` (`selectPermissionKeys`, `selectIsSuperAdminPermission`, `selectPermissionLoaded`) ke filter menu sidebar |
| `src/app/health-services/hemodialysis-management/layout.jsx` | Berkas baru. Layout dasar pembungkus modul Hemodialisa dengan metadata terstandar MMC |
| `src/app/health-services/hemodialysis-management/page.jsx` | Berkas baru. Halaman Beranda Hemodialisa (`FE-HMD-01`) |
| `src/app/health-services/hemodialysis-management/orders/page.jsx` | Berkas baru. Halaman Permintaan Masuk (`FE-HMD-02`) |
| `src/app/health-services/hemodialysis-management/patients/page.jsx` | Berkas baru. Halaman Daftar Pasien Hemodialisa (`FE-HMD-03`) |
| `src/app/health-services/hemodialysis-management/worklist/page.jsx` | Berkas baru. Halaman Jadwal & Daftar Kerja (`FE-HMD-04`) |
| `src/app/health-services/hemodialysis-management/unit-readiness/page.jsx` | Berkas baru. Halaman Kesiapan Unit (`FE-HMD-05`) |
| `src/app/health-services/hemodialysis-management/master-data/machines/page.jsx` | Berkas baru. Halaman Master Mesin Hemodialisa (`FE-HMD-08`) |
| `src/app/health-services/hemodialysis-management/master-data/stations/page.jsx` | Berkas baru. Halaman Master Station Hemodialisa (`FE-HMD-09`) |
| `src/app/health-services/hemodialysis-management/master-data/checklist-items/page.jsx` | Berkas baru. Halaman Master Butir Persiapan (`FE-HMD-10`) |
| `src/app/health-services/hemodialysis-management/master-data/settings/page.jsx` | Berkas baru. Halaman Pengaturan Unit Hemodialisa (`FE-HMD-11`) |
| `tests/unit/hemodialysis-sidebar-navigation.test.mjs` | Berkas baru. Unit test otomatis yang menguji pendaftaran menu, ketersediaan 9 rute, resolusi path sidebar, dan logika penyaringan menu berdasarkan role serta permission |

### 3.3 Kepatuhan arsitektur frontend

- **Pola Komponen Base**: Menggunakan `Hero` dari `src/components/features/base-features/hero.jsx` pada seluruh halaman awal tanpa membuat komponen duplikat.
- **Pemisahan Boundary**: `layout.jsx` dan `page.jsx` tetap sebagai Server Component bersih tanpa `"use client"` yang tidak perlu, sesuai kaidah Next.js App Router pada `rules/frontend/frontend-architecture.md`.
- **UI Gate**:
```text
UI GATE: 5 elemen — REUSE 4, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```
- `Sidebar Menu Tree` (`menu-items.jsx`): `REUSE` struktur menu items standar.
- `Sidebar Menu Handler` (`left-sidebar-menu-handle.jsx`): `REUSE` `NESTED_MENU_KEYS` dan helper resolusi path menu.
- `Sidebar Items Virtualized` (`left-sidebar-items-virtualized.jsx`): `REUSE` `VirtualizedSideBarItems` dengan integrasi kewenangan.
- `Module Route Layout` (`layout.jsx`): `COMPOSE` Server Component layout boundary tipis.
- `Placeholder Pages` (9 routes): `REUSE` `Hero` dari `src/components/features/base-features/hero.jsx`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Skeleton loading / kerangka layout bawaan Next.js App Router |
| Kosong | Halaman awal menampilkan kartu `Hero` dengan judul modul, deskripsi fitur, dan breadcrumb navigasi |
| Gagal | Error boundary standar aplikasi `error.jsx` menangkap kegagalan render rute |
| Tanpa hak akses | Menu yang tidak memiliki hak akses disembunyikan dari sidebar sebelum diklik; jika rute diakses langsung via URL, guard otorisasi backend/middleware menolak dengan HTTP 403 |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — Task `FE-HMD-01` adalah task fondasi navigasi, registrasi menu, dan routing App Router tanpa pemanggilan API HTTP langsung. Integrasi Axios services dan Redux slice akan dikerjakan pada task `FE-HMD-02`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-sidebar-navigation.test.mjs` | `6 pass, 0 fail, duration 134ms` | `PASS` | 6 skenario unit test lulus: registrasi menu, ketersediaan 9 berkas page, resolusi path resolver, visibilitas Koordinator HD, penyaringan Staf Bangsal, dan penyaringan berbasis permission |
| `npx eslint "src/utils/menu-sidebar/menu-items.jsx" "src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx" "src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx" "src/app/health-services/hemodialysis-management/**" "tests/unit/hemodialysis-sidebar-navigation.test.mjs"` | `0 error(s), 0 warning(s)` | `PASS` | Linter ESLint bersih tanpa pelanggaran |
| `npx next build` | Build sukses dengan exit code 0 | `PASS` | Seluruh 9 rute baru berhasil dikompilasi sebagai static/prerendered routes tanpa error |

Uji manual: `NOT APPLICABLE` — Pengujian antarmuka dan resolusi menu diverifikasi secara komprehensif melalui unit test `hemodialysis-sidebar-navigation.test.mjs`.

**Tidak dijalankan:** `test:e2e` Playwright karena tidak diminta dan task berfokus pada fondasi menu/routing tingkat unit.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. *Contoh Tampilan Menu*: Pengguna dengan peran Koordinator HD login; di sidebar kiri muncul grup "Hemodialisa" dengan submenu Beranda, Permintaan Masuk, Daftar Pasien, Jadwal & Daftar Kerja, Kesiapan Unit, dan grup Master Data. | Terpenuhi | `tests/unit/hemodialysis-sidebar-navigation.test.mjs` membuktikan pengguna dengan peran "Koordinator HD" melihat tepat 6 submenu dan 4 subitems Master Data |
| 2. Mengklik setiap butir menu mengarahkan pengguna ke URL rute yang benar tanpa reload layar (SPA navigation Next.js). | Terpenuhi | Seluruh 9 rute didaftarkan dengan `pathname` absolut, memiliki berkas `page.jsx` fisik yang valid, dan di-resolve oleh `findFullPathByPathname` |
| 3. Pengguna staf bangsal (yang hanya memiliki hak create order) tidak melihat menu internal unit seperti Kesiapan Unit dan Master Data. | Terpenuhi | `filterMenuItemsByRole` memverifikasi peran "Staf Bangsal" dan klaim hak akses `["hemodialysisorder:read"]` menyembunyikan menu Kesiapan Unit dan Master Data |
| DoD: Menu sidebar terintegrasi, rute modul dapat diakses tanpa error 404, pemeriksaan izin menu lulus uji. | Terpenuhi | Peta menu terdaftar di `menu-items.jsx`, 9 rute page App Router terpasang dan lolos build Next.js, test otomatis 100% lulus |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Tidak ada |
| Dependency backend | `BE-HMD-01` s/d `BE-HMD-19` telah selesai, sehingga kontrak backend siap dikonsumsi pada task frontend berikutnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas yang diubah dan dibuat di `QuilvianSystemFrontendDev`: `src/utils/menu-sidebar/menu-items.jsx`, `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx`, `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx`, `src/app/health-services/hemodialysis-management/**`, `tests/unit/hemodialysis-sidebar-navigation.test.mjs` |
| Langkah berikutnya | Implementasi task `FE-HMD-02` (Manajemen State Redux, Axios API Services, Constants, dan Hook Transisi Status) |
