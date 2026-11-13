# Laporan Perubahan Frontend — `FE-RHB-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-004` |
| Judul | Episode: daftar (FE-RHB-04) dan kerangka ruang kerja (FE-RHB-05), plus butir menu *Episode Rehab* |
| Slice | FS-B — Permintaan dan episode |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-004` (r2) |
| Trace | FEAT-007..029 (tampilan); FR-RHB-062 (menu); UAT-15; `03-frontend-architecture.md` skema FE-RHB-04/05 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05. API yang dipakai: *Rehab Episode* (`BE-RHB-008` ✅), baca saja |
| Dependency | `FE-RHB-003` ✅, `BE-RHB-008` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-06 |
| Status | **✅ Selesai** (2026-11-06). ESLint: 0 error, 0 warning. Uji browser + jaringan Playwright pada putaran akhir: 22/22. Dua FAIL di putaran awal berasal dari cara ukur skrip, bukan dari aplikasi (§5). |

## 1. Masalah yang diselesaikan

Episode rehab terbuka otomatis saat permintaan diterima, tetapi belum ada layar untuk melihatnya.
Dokter rehab dan terapis perlu tiga hal:
- daftar episode dengan saringan *perlu telaah* dan *evaluasi jatuh tempo*;
- satu halaman ruang kerja per episode;
- jalan dari permintaan yang sudah diterima langsung ke episodenya.

Ruang kerja ini baru kerangka. Asesmen, program, edukasi, sesi, reasesmen, dan ringkasan akhir
dibangun task berikutnya, jadi bagian-bagian itu belum boleh menampilkan tombol yang belum bekerja.

**Contoh.** Dr. Sp.KFR membuka *Rehab Medik → Episode Rehab* dan mencari `RHB-EPS-2026-000001`.
Dari baris itu ia membuka ruang kerja dan melihat:
- ringkasan pasien ANISA PRAMESTI;
- layanan *Uji BE-RHB-007* (Diminta);
- tujuh bagian dokumen yang masing-masing bertuliskan "Belum ada …";
- riwayat status "— → Menunggu asesmen medis" oleh SuperAdmin.

Dari rincian permintaan `RHB-REQ-2026-000001` (Diterima), tombol *Buka Episode RHB-EPS-2026-000001*
membawanya ke halaman yang sama.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-04/05 dan peta menu `03-frontend-architecture.md`. Navigasi antarbagian ruang kerja adalah `DEV_DISCRETION`; dipilih bagian bertumpuk yang bisa dilipat (opsi yang direkomendasikan di percakapan task). |
| Izin owner | ESLint dan uji browser (backend + `next dev` + Playwright; akun uji dinonaktifkan sesudahnya), sesuai instruksi task 2026-11-06. Tanpa build, tanpa commit. |
| Menyentuh berkas bersama | `menu-items.jsx`: menambah satu butir. `rehab-request-utils.js`: menambah `formatDate`. `rehab-request-detail-view.jsx`: menambah tombol tautan. Tidak ada base component yang diubah. |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/lib/services/health-services/rehabilitation-management/rehab-episode.service.js` | Baru | 6 fungsi GET: metadata filter, ringkasan, daftar, rincian, timeline, riwayat status |
| `src/lib/constants/health-services/rehabilitation-management/episodes/rehab-episode-constants.js` | Baru | Nada badge per status, filter bawaan, opsi *Perlu telaah* dan *Evaluasi*, kartu ringkasan, dan definisi 7 bagian dokumen ruang kerja (judul, bunyi kosong, kunci hitungan `documents`, task pengisinya) |
| `src/lib/hooks/health-services/rehabilitation-management/episodes/use-rehab-episodes.js` | Baru | Controller daftar. Request sebelumnya dibatalkan dengan `AbortController`, dan halaman kembali ke 1 setiap saringan berubah. Ringkasan memakai saringan yang sama, tanpa status dan paginasi. |
| `.../episodes/use-rehab-episode-workspace.js` | Baru | Controller ruang kerja. Galat rincian disimpan utuh (dengan `statusCode`) agar gerbang 403 mengenalinya. Kegagalan timeline atau riwayat tidak menjatuhkan halaman. |
| `src/components/view/.../episodes/rehab-episodes-view.jsx` | Baru | FE-RHB-04 |
| `src/components/view/.../episodes/rehab-episode-workspace-view.jsx` | Baru | FE-RHB-05 (kerangka) |
| `src/app/health-services/rehabilitation-management/episodes/page.jsx`, `[episodeId]/page.jsx`, `[episodeId]/episode-route-param.js` | Baru | Route tipis. Id selain UUID langsung dijawab 404 tanpa memanggil API. |
| `src/components/view/.../requests/rehab-request-detail-view.jsx` | Diperbarui | Tombol *Buka Episode {nomor}* tampil bila `episodeId` ada **dan** pengguna memegang `RehabEpisode : Read` |
| `src/utils/health-services/rehabilitation-management/requests/rehab-request-utils.js` | Diperbarui | `formatDate` untuk `DateOnly` (`yyyy-MM-dd`), dibaca sebagai tanggal lokal, bukan UTC |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui (+8 di task ini) | Butir *Episode Rehab* di blok *Rehab Medik*, `requiredPermission: { resource: "RehabEpisode", action: "Read" }` |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Daftar: hero, ringkasan, filter, tabel, badge, paginasi, aksi baris, gerbang 403 | `REUSE` | `Hero`, `SummaryGrid`, `DataFilter`, `FilterDatePicker`, `FilterSelect`, `DataTable`, `StatusBadge`, `RowActionMenu`, `AccessDeniedGate` |
| 2 | Kepala ruang kerja + ringkasan | `REUSE` | `BaseDetailView` (`renderDetailActions`, `afterDetail`) |
| 3 | Bagian Layanan | `COMPOSE` | `BaseDetailSection` + `DataTable` |
| 4 | Tujuh bagian dokumen (hitungan + bunyi kosong) | `COMPOSE` | `BaseDetailSection` (`collapsible`, `count`, `soft`) |
| 5 | Timeline dan Riwayat Status | `COMPOSE` | `BaseDetailSection` + `DataTable` |
| 6 | Gerbang URL tanpa hak | `REUSE` | `AccessDeniedGate` |
| 7 | Butir menu | `REUSE` | `menu-items.jsx` + `requiredPermission` |

`UI GATE: PASS — 0 NEW, 0 EXTEND, 3 COMPOSE (direkomendasikan; pilihan bernomor disajikan di percakapan task).`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| State | Service + hook per halaman, tanpa slice Redux, mengikuti pola `FE-RHB-003` |
| Isi bagian dokumen | Hanya hitungan dari `documents` rincian episode, plus bunyi "Belum ada …" bila nol. Tanpa tombol (AC 3). Setiap task layar berikutnya mengganti isi bagiannya sendiri. |
| Kolom *Telaah* | Badge *Perlu telaah* tampil bila `needsReview`; selain itu "-" |
| Riwayat status | Label status dan tindakan memakai peta label dari `FE-RHB-003` (sudah memuat status episode). Timeline memakai `itemTypeName` dari server. |
| Status ringkasan akhir | Angka `RehabRecordStatus` dipetakan ke Draf/Final/Dibatalkan |
| Baris *Status* di ringkasan | Tidak ditampilkan karena status sudah ada di kepala halaman. Bila ditampilkan, `BaseDetailView` memindahkannya sendirian ke "Informasi Tambahan". |
| Tautan dari permintaan | Juga dijaga `usePermission(RehabEpisode, Read)`. Tanpa butir itu ruang kerja pasti menolak, jadi tombolnya tidak ditampilkan. |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas service, konstanta, hook, view, route episode, serta `rehab-request-detail-view.jsx`, `rehab-request-utils.js`, `menu-items.jsx` | 0 error, 0 warning |
| Grep anti-regresi UI di view episode (`<button>`/`<table>` mentah, kelas `fw-*`/`fs-*`, `style={{`) | 0. Tidak ada CSS baru; kelas `sectionBody`/`hint` dari CSS rehab `FE-RHB-002` dipakai ulang. |
| Grep privasi (`console`, `localStorage`, `sessionStorage`) di berkas baru | 0 |
| Review diff | 3 berkas existing berubah (lihat §3.1); sisanya berkas baru di folder rehab |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser dan jaringan (MANUAL TEST)

**Lingkungan (2026-11-06):**
- backend Debug di `https://localhost:7184`;
- `next dev` di port 3000;
- Playwright Chromium dijalankan dari scratchpad;
- respons jaringan direkam lewat `page.on("response")`.

DB dev berisi 3 episode, semuanya *Menunggu asesmen medis* dan belum punya program. Karena itu
hasil yang benar untuk saringan *perlu telaah* dan *evaluasi jatuh tempo* adalah kosong. Setiap
hasil layar dicocokkan dengan jawaban API untuk saringan yang sama.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | **AC5**: SuperAdmin melihat *Episode Rehab* di blok *Rehab Medik* | PASS |
| 2 | Daftar memuat 3 baris, sama dengan `totalData` API; kolom sesuai skema | PASS |
| 3 | **AC1**: *Perlu telaah* → request daftar **dan** ringkasan membawa `needsReview=true`; hasil 0 = API 0, dan bunyi "Belum ada episode pada saringan ini." tampil | PASS |
| 4 | **AC1**: gabungan *Perlu telaah* + *Evaluasi jatuh tempo* → kedua parameter terkirim; hasil = API | PASS |
| 5 | Reset → request tanpa kedua parameter; daftar kembali 3 baris | PASS |
| 6 | **AC1**: *Evaluasi jatuh tempo* saja → `evaluationDue=true`; hasil = API | PASS |
| 7 | Pencarian `RHB-EPS-2026-000001` → satu baris episode itu | PASS |
| 8 | Klik dua kali → ruang kerja: kepala episode, Layanan, 7 bagian dokumen, Timeline, Riwayat Status | PASS |
| 9 | **AC3**: tombol di area konten hanya *Kembali*, *Lihat Permintaan*, *Muat Ulang* (di luar sidebar dan navbar; tombol *Next.js Dev Tools* milik mode dev); bagian kosong berbunyi "Belum ada …" | PASS |
| 10 | Ruang kerja memanggil `/timeline` dan `/status-history` (200); riwayat 1 baris *Buka episode* | PASS |
| 11 | *Lihat Permintaan* → rincian permintaan asal | PASS |
| 12 | **AC2**: rincian `RHB-REQ-2026-000001` (Diterima) menampilkan *Buka Episode RHB-EPS-2026-000001* → ruang kerja episode yang benar | PASS ×2 |
| 13 | Akun uji, kebijakan **`RehabEpisode : Read` saja** → menu *Episode Rehab* ada, *Permintaan Rehab* tidak; daftar dan ruang kerja termuat | PASS ×3 |
| 14 | Akun uji, kebijakan **`RehabRequest : Read` saja** → menu *Episode Rehab* tidak ada, *Permintaan Rehab* ada (AC5); tombol *Buka Episode* tidak tampil (AC2) | PASS ×2 |
| 15 | **AC4 / UAT-15**: akun pada #14 membuka URL ruang kerja dan URL daftar episode → *Ups! Akses Ditolak*; nomor episode tidak tampil | PASS ×2 |

Rekap putaran akhir: **22/22 PASS**. Diagnosa tambahan: tombol konten 1/1, sidebar tanpa
`RehabEpisode : Read` pada akun baru 2/2, urutan kebijakan A → B 4/4.

**Kegagalan di putaran awal**, keduanya salah ukur skrip:

| Putaran | FAIL | Penyebab | Perbaikan skrip |
| --- | --- | --- | --- |
| 1 | Klik saringan *Perlu Telaah* kena kartu ringkasan berjudul sama | `getByText` menemukan kartu dulu | Pemicu dicari lewat `button[aria-haspopup="listbox"]` |
| 2 | AC3 menghitung tombol sidebar | Lingkup `<main>` ikut memuat sidebar | Tombol di dalam `aside`/`nav`/`header` dikeluarkan |
| 2–4 | AC5 putaran B: "Episode Rehab" terbaca ada | Nama akun uji "Uji **Episode Rehab** FE-RHB-004" tampil di navbar (`nav.app-navbar`). Respons `/auth/permissions` saat itu benar, hanya `RehabRequest : Read`. Artinya PASS AC5 putaran A di uji yang sama juga belum sah. | Nama akun diganti "Uji Akses FE-RHB-004" dan navbar dikeluarkan dari pembacaan sidebar. Putaran akhir mengulang A dan B dengan cara baca baru: keduanya PASS. |

**Pembersihan.** Dicek di DB dev dengan kueri READ ONLY:
- setiap putaran mengosongkan kebijakan pasangan uji dan menonaktifkan akunnya;
- 0 dari 16 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pada pasangan uji: 0 baris.

Kedua server dihentikan.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Filter *perlu telaah* dan *evaluasi jatuh tempo* bekerja | Terpenuhi | §5 #3–6: parameter terkirim ke daftar dan ringkasan, hasil sama dengan API, reset bersih. Catatan: data dev belum punya episode yang lolos saringan, jadi yang terbukti adalah jalur kosong. |
| (2) Tautan episode dari rincian permintaan *Accepted* | Terpenuhi | §5 #12, #14 |
| (3) Bagian yang belum dibangun tidak menampilkan tombol tak berfungsi | Terpenuhi | §5 #9 |
| (4) URL tanpa hak → `access-denied-gate` (UAT-15) | Terpenuhi | §5 #15 |
| (5) Butir menu *Episode Rehab* ber-`requiredPermission` RehabEpisode:Read | Terpenuhi | §3.1, §5 #1, #13, #14 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Data saringan berisi | Baru bisa dibuktikan setelah ada program aktif bertanda telaah atau berjadwal evaluasi (`BE-RHB-011`/`FE-RHB-007`). Uji ulang singkat disarankan saat itu. |
| Ruang kerja tumbuh | `FE-RHB-005..011` mengganti isi bagian masing-masing di `REHAB_EPISODE_WORKSPACE_SECTIONS` / `WorkspaceDocumentSection`. Bunyi kosong tetap dipakai sampai dokumen pertama ada. |
| Footer tetap aplikasi | Masih menutupi sebagian konten di bawah layar (temuan lama `FE-RHB-002`, di luar cakupan) |
| Dependency backend | Tidak ada yang baru. Task lanjutan tetap tertahan `BE-RHB-004` (bukti `RHB-OQ-012`) → `BE-RHB-009`/`010`. |

## Revisi 2026-11-10 — tombol hero ruang kerja episode

Diminta langsung oleh user. Berkas: `src/components/view/health-services/rehabilitation-management/episodes/rehab-episode-workspace-view.jsx`.

| Perubahan | Keputusan base component | Isi |
| --- | --- | --- |
| Tombol *Muat Ulang* di hero dihapus | `REUSE` (`BaseDetailView.renderDetailActions`) | `reload` tidak lagi diambil dari hook; hook tidak diubah. |
| *Lihat Permintaan* menjadi tombol utama | `REUSE` (`BaseButton variant="primary"`, gradient) | Sebelumnya `secondary`. *Kembali* tetap `secondary`. |

UI GATE: semua `REUSE`. `npx eslint <berkas>` PASS (exit 0). AUTOMATED TEST: SKIPPED (opsional) — perubahan presentasi. MANUAL TEST: NOT FEASIBLE pada sesi ini — server tidak dijalankan. Status roadmap tetap.

## Revisi 2026-11-10 — tabel daftar episode

Diminta langsung oleh user, disamakan dengan daftar permintaan (lihat `FE-RHB-003.md` §8). Berkas: `src/components/view/health-services/rehabilitation-management/episodes/rehab-episodes-view.jsx`.

| Perubahan | Keputusan base component |
| --- | --- |
| Tombol *Muat Ulang* di header filter dihapus (*Coba lagi* pada alert error tetap) | `REUSE` (`DataFilter`) |
| Kolom *Pasien* + *No. RM* digabung: nama di atas, nomor RM (tanpa label) di bawah | `COMPOSE` (utilitas `fw-semibold` / `text-muted small`) |
| Kolom aksi (`RowActionMenu`) dihapus; ruang kerja dibuka dengan klik dua kali baris | `REUSE` (`DataTable.onRowDoubleClick`) |

`npx eslint <berkas>` PASS (exit 0). AUTOMATED TEST: SKIPPED (opsional). MANUAL TEST: NOT FEASIBLE pada sesi ini — server tidak dijalankan.
