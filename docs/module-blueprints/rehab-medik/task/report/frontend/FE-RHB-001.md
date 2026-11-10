# Laporan Perubahan Frontend — `FE-RHB-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-001` |
| Judul | Fondasi: menu, konstanta, helper |
| Slice | FS-A — Fondasi dan konfigurasi (`MVP-0`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-001` |
| Trace | FR-RHB-062; RHB-FE-01..03; `03-frontend-architecture.md` §2, §6; `permission-audit-matrix.md` §2 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 |
| Dependency | — |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930` |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-05 |
| Status | **✅ Selesai** (2026-11-06). Source selesai untuk cakupan yang disetujui, dan ESLint `PASS` (0 temuan pada 3 berkas). Butir menu dipindah ke task layar atas keputusan owner (roadmap r2). |

## 1. Masalah yang diselesaikan

Layar-layar Rehab Medik berikutnya butuh satu tempat baku untuk alamat endpoint, rute, nama
butir hak akses, dan cara membaca galat backend. Tanpa itu, setiap layar akan menulis alamat dan
penanganan galatnya sendiri. Akibatnya alamat bisa salah ketik, dan galat 409 basi bisa
diperlakukan berbeda dari layar ke layar.

## 2. Keputusan owner selama task (2026-11-05)

| Pertanyaan | Keputusan | Akibat |
| --- | --- | --- |
| Kapan butir menu didaftarkan? | **Per task layar.** Layanan Rehab di `FE-RHB-002`, Permintaan di `FE-RHB-003`, Episode di `FE-RHB-004`, Sesi di `FE-RHB-008`. | Tidak ada tautan menu yang berujung 404 bagi super admin selama layar belum ada. AC (1)–(2) kartu ini pindah ke keempat task itu. |
| Halaman akar modul? | **`layout.jsx` saja, tanpa `page.jsx`.** | Tidak ada beranda dekoratif. `AccessDeniedGate` dipasang di setiap layar data. |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` FE, `rules/frontend/frontend-architecture.md`
- `src/app/health-services/hemodialysis-management/{layout,page}.jsx` (modul rujukan terdekat)
- `src/lib/constants/health-services/hemodialysis-management/hemodialysisConstants.js`
- `src/lib/services/health-services/hemodialysis-management/hmdServiceHelper.js`
- `src/utils/menu-sidebar/menu-items.jsx`, `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx`, `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx`
- `src/components/features/base-features/access-denied-gate.jsx`
- BE `Responses/ApiResponse.cs` (bentuk `Fail(statusCode, message, errors)`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/rehabilitation-management/rehabConstants.js` | Baru. `REHAB_API_ROOT`, `REHAB_API` (12 grup + addendum RM), `REHAB_ROUTES`, `REHAB_PERMISSION_RESOURCES` (13 resource, sama persis dengan matriks hak akses), `REHAB_PERMISSION_ACTIONS`, `REHAB_STATE_COPY`, `REHAB_ERROR_CODES`. |
| `src/lib/services/health-services/rehabilitation-management/rehabServiceHelper.js` | Baru. `unwrapApiResponse` dan `normalizeRehabError` (status, `code`/`fields` dari `errors`, penanda `isStale`/`isLocked`/`isForbidden`/`isNotFound`/`isCanceled`). Tanpa console. |
| `src/app/health-services/rehabilitation-management/layout.jsx` | Baru. Metadata modul, tanpa pembungkus berkelas, karena kelas pembungkus Hemodialisa tidak punya CSS. |

### 3.3 Tabel keputusan base component (UI GATE)

| Elemen | Status | Alasan |
| --- | --- | --- |
| — | `NOT APPLICABLE` | Task ini tidak merender UI. Konstanta, helper, dan layout tanpa markup. |

`UI GATE: NOT APPLICABLE — tidak ada elemen tampilan pada task ini.`

### 3.4 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| Field hak akses menu | Desain (`03-frontend-architecture.md` §2) menulis `permission: "<Resource>:Read"` mengikuti blok Hemodialisa. Source membuktikan sidebar hanya menegakkan `requiredPermission: { resource, action }` (`filter-menu-items-by-permission.jsx`, FE-BD-006). Field `permission` string **tidak dibaca**. Task layar wajib memakai `requiredPermission`. |
| Menu dan halaman akar | Lihat §2 |

## 4. Verifikasi

| Pemeriksaan | Hasil | Bukti |
| --- | --- | --- |
| Review diff/scope | Hanya 3 berkas baru di folder rehab. Tidak ada berkas existing yang diubah. | `git status --short`: tiga folder `??` |
| Arah dependensi | `services` → `constants`. Tidak ada impor view/ui. Alias `@/`. | Isi berkas |
| Kesesuaian nama resource dengan matriks hak akses | 13 resource identik dengan `permission-audit-matrix.md` §2 | Perbandingan manual |
| Bentuk galat backend | `Code`/`Fields` dibaca dari `errors`, sesuai `ApiResponse.Fail` | `Responses/ApiResponse.cs` |
| Grep privasi (`console`, `localStorage`, `sessionStorage`) | Bersih | grep |
| ESLint (konfigurasi repo `eslint.config.mjs`; perintah `npx eslint` atas 3 berkas task, karena `npm run lint` = `eslint .` memindai seluruh repo), dijalankan agent dengan izin owner 2026-11-06 | `PASS` — 0 error, 0 warning, `ESLINT_EXIT=0` | Keluaran terminal |
| Kecocokan `REHAB_API` dengan route backend yang sudah dibangun (`BE-RHB-005`..`008`) | `rehab-disciplines`, `rehab-lookups`, `rehab-requests`, `rehab-episodes` identik dengan `[Route]` controller. Helper membaca `errors.code`/`errors.fields` (camelCase), dan itulah bentuk yang terbukti di kabel pada uji `BE-RHB-005`..`007`. | grep `[Route]`; laporan BE |
| `npm run build` / `npm run dev` | `NOT RUN` | Dilarang owner |

AUTOMATED TEST: SKIPPED (opsional) — helper berupa normalisasi galat sederhana. Test unit `node:test` boleh ditambahkan kelak, tetapi menjalankan test dilarang owner pada sesi ini.

MANUAL TEST: NOT APPLICABLE — tidak ada kontrol interaktif. Butir menu dipindah ke task layar.

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Butir menu tampil hanya untuk pemegang `<Resource>:Read` terkait | **Dipindahkan** ke `FE-RHB-002/003/004/008` (keputusan owner) | §2 |
| (2) Route tujuan sesuai peta menu | Terpenuhi untuk konstanta: `REHAB_ROUTES` sesuai peta menu §2 desain. Pendaftaran menu dipindahkan. | `rehabConstants.js` |
| (3) Helper tidak mencetak payload ke console | Terpenuhi | grep |
| (4) Tidak ada endpoint mock | Terpenuhi | Isi berkas |
| DoD: laporan | Terpenuhi | Berkas ini |
| (5) ESLint tanpa galat baru pada tiga berkas ini | Terpenuhi | §4 |
| DoD: lint | Terpenuhi | §4 |

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan legacy (tidak diubah) | (a) Blok menu Hemodialisa memakai `permission: "X:Read"` yang tidak ditegakkan, sehingga menu HD tampil untuk semua pengguna. (b) `left-sidebar-items-virtualized.jsx` meng-hardcode role "finance/keuangan/admin/manajer" untuk menu Finance, yang melanggar larangan hardcode role. Keduanya dicatat sebagai temuan, bukan diperbaiki. |
| Risiko tersisa | AC menu harus benar-benar ditambahkan ke empat kartu task layar lewat revisi roadmap (`plan-module-delivery`). Pembangun task tidak berwenang mengubah AC roadmap. |
| Dependency backend | Tidak ada untuk task ini |
| Status Git FE | `?? src/app/health-services/rehabilitation-management/`, `?? src/lib/constants/health-services/rehabilitation-management/`, `?? src/lib/services/health-services/rehabilitation-management/` |
| Langkah berikutnya | `FE-RHB-002` (konfigurasi layanan rehab; `BE-RHB-005` ✅) dan `FE-RHB-003` (permintaan; `BE-RHB-007` ✅) kini 🟡 |
