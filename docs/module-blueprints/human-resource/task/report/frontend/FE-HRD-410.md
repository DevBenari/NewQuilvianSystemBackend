# Laporan Perubahan Frontend — `FE-HRD-410`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-410` |
| Judul | Halaman unit menjadi "Struktur Organisasi" dengan mode Daftar dan Hierarki |
| Slice | `S-FIN-M05` amandemen — Menu Organisasi (`PLAN-REPAIR-HRD-008`, `FIX-HRD-008-03`) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` bagian 32 |
| Trace | `HRD-DEC-248`; `ISS-HRD-008-01`, `ISS-HRD-008-T1`, `ISS-HRD-008-T4`; `03-frontend-architecture.md` 34.7; skema `PLAN-REPAIR-HRD-008` 3.2–3.3 |
| Contract version | `v30` tetap; endpoint `organization-units` sudah ada dan tidak berubah untuk task ini |
| Wewenang UI | Susunan sesuai skema plan repair 3.2–3.3; mode bawaan Daftar; pohon tetap hanya untuk dilihat |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — 4 berkas, satu halaman, memindahkan komponen pohon |
| Task mode | `FRONTEND` (disetujui pengguna 24 November 2026: "oke saya setujui kerjakan semua dan selesaikan") |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/**`; laporan ini |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fe8aebf2f` (branch `HamzahV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `3ef27c96` (branch `MHamzah`) |
| Tanggal | 2026-11-24 |
| Status | ✅ Selesai — 6/6 AC; lint, build, uji unit sidebar, pemeriksa CSS, dan Playwright lulus. Uji dengan backend dan akun nyata `NOT RUN` |

---

## 1. Keadaan yang ditemukan di awal

- Pohon unit berada di halaman Departemen/Posisi lama (`/hr/master-data/organization`, tombol Hierarki), sedangkan tambah/ubah unit berada di menu lain, "Unit Organisasi" (`/hr/master-data/organization-unit`).
- Tombol "Lihat Detail Unit Lengkap" di pohon membuka `/hr/master-data/organization-unit?search=<kode>`, tetapi hook daftar unit tidak membaca `?search=`. Petugas mendarat di daftar penuh (`organization-hierarchy-view.jsx:178-183` pada `fe8aebf2f`).
- Teks Inggris: "Refresh", "Parent Unit", "Root (Unit Teratas)", tipe unit mentah ("Installation"), kolom "Child", kartu "Root Unit".

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas HR membuka **SDM → Master Data → Organisasi & Struktur Jabatan → Struktur Organisasi**.
2. Halaman terbuka di mode **Daftar**: kartu ringkasan, penyaring, tabel unit, dan tombol "+ Tambah Unit Organisasi".
3. Petugas menekan **Hierarki** untuk melihat pohon. Ia membuka cabang, memilih "Instalasi Farmasi", dan membaca panel ringkasan (tipe "Instalasi", level, unit induk).
4. Petugas menekan **Buka Detail Unit**. Halaman detail unit itu langsung terbuka.

Jalur tidak normal: pohon kosong → "Belum Ada Data Hierarki"; gagal memuat → "Terjadi Kesalahan" + "Coba Lagi"; tanpa hak `OrganizationUnit : Read` → `AccessDeniedGate`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`master-data-organization-unit-view.jsx`, `master-data-organization-view.jsx`, `organization-hierarchy-view.jsx`, `organization-unit-constants.jsx`, `use-master-data-organization-unit.jsx` (`openDetail`), `private-route-token-utils.js`, `organization-hierarchy.module.css`, `src/app/hr/master-data/organization-unit/[slug]/page.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/master-data/organization-unit/master-data-organization-unit-view.jsx` | State mode `Daftar`/`Hierarki`, tombol mode di `Hero` (`aria-pressed`), mode Hierarki merender `OrganizationHierarchyView` |
| `src/components/view/hr/master-data/organization/hierarchy/organization-hierarchy-view.jsx` | "Buka Detail Unit" membuat token rute lewat `registerPrivateRouteToken` (scope/type dari `ORGANIZATION_UNIT_CONFIG`) dan membuka detail; label tipe unit Indonesia; teks "Muat Ulang", "Unit Induk", "Unit Teratas"; `aria-label` cabang berbahasa Indonesia |
| `src/lib/constants/hr/master-data/organization-unit/organization-unit-constants.jsx` | Judul "Struktur Organisasi" dan deskripsinya; kartu "Unit Teratas"; kolom "Unit Induk", "Sub-Unit" |
| `src/app/hr/master-data/organization-unit/page.jsx` | Judul tab peramban "Struktur Organisasi" |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap route → view → hook → slice/service yang sudah ada. Komponen pohon dipakai ulang tanpa dipindah folder (perubahan kecil, sesuai plan repair). Tidak ada hook, slice, atau komponen baru.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/master-data/organization/master-data-organization-view.jsx` (pola tombol mode yang sudah ada).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Kepala halaman | `Hero` | `REUSE` | — |
| Tombol mode Daftar/Hierarki | `organization-hierarchy.module.css` (`modeToggle*`) | `REUSE` | Kelas yang sama dengan halaman lama |
| Kartu, penyaring, tabel | `SummaryCards`, `DataFilter`, `DataTable` | `REUSE` | — |
| Pohon dan panel | `OrganizationHierarchyView` | `REUSE` | — |

`UI GATE: PASS — 4 elemen REUSE, 0 COMPOSE, 0 NEW`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <6 berkas jsx yang diubah pada slice ini>
OK — 6 berkas diperiksa, 0 temuan.
$ grep -nE "className=\"[^\"]*\b(flex|grid|p-[0-9]|m-[0-9]|text-(sm|lg|xl)|bg-[a-z]+-[0-9]{3}|rounded-(md|lg))\b" <berkas view>
(kosong)
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hr/master-data/organization-unit` (Daftar) | `QuilvianSystemFrontendDev/test-with-agy/screenshots/organization-menu-1124/FE-HRD-410-01-daftar.png` | Sesuai skema 3.2 |
| `/hr/master-data/organization-unit` (Hierarki) | `.../FE-HRD-410-02-hierarki.png` | Sesuai skema 3.3; tipe "Instalasi"/"Direktorat" |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka kartu/tabel; pohon "Memuat struktur hierarki..." |
| Kosong | "Data unit organisasi tidak ditemukan."; pohon "Belum Ada Data Hierarki" |
| Gagal | Pesan galat di atas tabel; pohon "Terjadi Kesalahan" + "Coba Lagi" |
| Tanpa hak akses | `AccessDeniedGate` menutup halaman. Halaman dan pohon kini satu hak akses (`OrganizationUnit : Read`) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Master Data / Organization Unit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/organization-units` | Tabel mode Daftar | `OrganizationUnit : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/organization-units/summary` | Kartu ringkasan | `OrganizationUnit : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/organization-units/hierarchy` | Pohon mode Hierarki | `OrganizationUnit : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error (exit 0) | `PASS` | Keluaran perintah |
| `npm run test:unit` | 5.374 uji: 5.270 lulus, 104 gagal. Ke-104 kegagalan bawaan: 24 berkas uji yang gagal dijalankan ulang dengan berkas HEAD `fe8aebf2f` dan menghasilkan jumlah gagal identik per berkas | `EXISTING / ENVIRONMENT ISSUE` | Perbandingan baseline sesi ini |
| `tests/unit/hr-master-data-sidebar-grouping.test.mjs` | 12/12 lulus | `PASS` | Keluaran perintah |
| `npm run build` | Kompilasi sukses, 927 halaman (dijalankan dua kali; terakhir setelah perbaikan label "Periode") | `PASS` | Log build |
| `check-css-usage.mjs` | 0 temuan | `PASS` | Bagian 3.4 |
| Playwright `tests/e2e/human-resource-organization-menu.spec.mjs` › FE-HRD-410 (build produksi :3710, API dimock) | Lulus | `PASS` | 5/5 spec lulus; screenshot bagian 3.4 |
| Uji dengan backend dan akun nyata | Tidak dijalankan | `NOT RUN` | Tidak ada izin penulisan DB dev pada sesi ini; layar ini hanya membaca |

Uji manual: `NOT FEASIBLE` dengan backend nyata pada sesi ini; digantikan Playwright dengan API dimock.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Judul "Struktur Organisasi", tombol Daftar/Hierarki, daftar + "+ Tambah Unit Organisasi" tampil pertama | Terpenuhi | Playwright FE-HRD-410 (`aria-pressed=true` pada Daftar); screenshot 01 |
| 2. "Buka Detail Unit" langsung membuka detail (`/organization-unit/<token>`) | Terpenuhi | Playwright: URL cocok `/hr/master-data/organization-unit/[^/?]+$`, tanpa `search=` |
| 3. Tipe unit Indonesia di panel pohon | Terpenuhi | Playwright: baris Tipe Unit berisi "Instalasi"; "Installation" 0 kemunculan |
| 4. Unit baru muncul saat beralih ke Hierarki | Terpenuhi di source | Pohon dipasang ulang saat mode dibuka, sehingga `fetchHierarchy` berjalan lagi |
| 5. Satu hak akses (`OrganizationUnit : Read`) | Terpenuhi | Ketiga endpoint di bagian 5 memakai `OrganizationUnit` |
| 6. Tidak ada "Refresh", "Parent Unit", "Child", "Root Unit" | Terpenuhi | Playwright ("Refresh" 0); diff konstanta dan view |
| DoD: laporan dan tanda roadmap | Terpenuhi | Laporan ini; roadmap frontend bagian 32 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | ESLint non-quiet menampilkan 4 warning `react-hooks/set-state-in-effect` di hook editor unit yang sudah ada sebelumnya (bukan dari task ini) |
| Masalah yang diketahui | Komponen pohon tetap di folder `organization/hierarchy/` walau kini dipakai halaman unit; dipindah hanya bila diminta |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `FE-HRD-412` (seluruh slice belum di-commit) |
| Langkah berikutnya | Rilis bersama `FE-HRD-411` dan `FE-HRD-412` |
