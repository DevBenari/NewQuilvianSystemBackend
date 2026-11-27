# Laporan Perubahan Frontend — `FE-HRD-420`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-420` |
| Nomor lama | `FE-HRD-412` (dinomori ulang saat merge `QuilvianIntegrationBackEnd` 27 November 2026 agar `FE-HRD-412` milik `A52-B` tidak berubah) |
| Judul | Sidebar baru, pengalihan alamat lama, tautan Penempatan Awal, dan uji sidebar |
| Slice | `S-FIN-M05` amandemen — Menu Organisasi (`PLAN-REPAIR-HRD-008`, `FIX-HRD-008-04`) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` bagian 33 (nomor lama bagian 32) |
| Trace | `HRD-DEC-257` (lama `HRD-DEC-248`), `HRD-DEC-258` (lama `HRD-DEC-249`); `ISS-HRD-008-01`, `ISS-HRD-008-02`; `03-frontend-architecture.md` 34.7 |
| Contract version | `NOT APPLICABLE` — navigasi saja |
| Wewenang UI | Susunan sidebar sesuai 34.7; tanpa `requiredPermission` baru |
| Dependency | `FE-HRD-419` ✅ (nomor lama `FE-HRD-411`) |
| Klasifikasi | `LIGHT` — 4 berkas + 1 berkas dihapus |
| Task mode | `FRONTEND` (disetujui pengguna 24 November 2026) |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/**`; laporan ini |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fe8aebf2f` (branch `HamzahV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `3ef27c96` (branch `MHamzah`) |
| Tanggal | 2026-11-24 |
| Status | ✅ Selesai — 6/6 AC; lint, build, uji unit sidebar 12/12, Playwright lulus |

---

## 1. Keadaan yang ditemukan di awal

Grup "Organisasi & Struktur Jabatan" berisi 11 butir, termasuk "Struktur Organisasi" (`/hr/master-data/organization`) dan "Unit Organisasi" (`/hr/master-data/organization-unit`) untuk data unit yang sama. Departemen/Posisi tidak punya butir. Layar Penempatan Awal Pegawai mengarahkan perbaikan data ke `/hr/master-data/organization?type=positions|departments` (`initial-placement-batch-view.jsx:44-45`).

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka grup **Organisasi & Struktur Jabatan** dan melihat: Struktur Organisasi, Departemen, Posisi, Legal Entity, Fasilitas Rumah Sakit, Lokasi Kerja, Pusat Biaya, Rumpun Jabatan, Level Jabatan, Grade Karyawan, Uraian Tugas, dan Atasan Langsung.
2. Saat petugas berada di halaman tambah, detail, atau ubah unit, butir **Struktur Organisasi** menyala. Di halaman tambah departemen, butir **Departemen** yang menyala.
3. Petugas yang membuka bookmark lama `/hr/master-data/organization` otomatis dialihkan ke halaman yang benar.
4. Di Penempatan Awal Pegawai, tautan "perbaiki data" membawa petugas langsung ke halaman Posisi atau Struktur Organisasi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`menu-items.jsx`, `left-sidebar-menu-handle.jsx` (`findFullPathByPathname`, awalan terpanjang), `filter-menu-items-by-permission.jsx`, `initial-placement-batch-view.jsx`, `src/app/finance/master-data/petty-cash-budget/page.jsx` (pola pengalih), `tests/unit/hr-master-data-sidebar-grouping.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | `corporateOrganizationUnit` berlabel "Struktur Organisasi" di urutan pertama; butir baru `corporateDepartment` (`/hr/master-data/organization/departments`) dan `corporatePosition` (`/positions`); `corporateOrganization` dan butir "Unit Organisasi" dibuang |
| `src/app/hr/master-data/organization/page.jsx` | Halaman pengalih: `?type=positions` → Posisi, `?type=departments` → Departemen, selain itu → Struktur Organisasi. Pencarian tujuan memakai `hasOwnProperty` agar nilai seperti `constructor` tidak terbaca sebagai properti objek |
| `src/app/hr/master-data/organization/organization-client.jsx` | Dihapus (tidak dipakai lagi) |
| `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` | `SAFE_DESTINATIONS.MASTER_POSITION` dan `MASTER_DEPARTMENT` menunjuk halaman baru; `MASTER_ORGANIZATION_UNIT` tetap |
| `tests/unit/hr-master-data-sidebar-grouping.test.mjs` | Grup organisasi 12 butir; total leaf 55; uji baru `HR-12` untuk menu aktif di halaman turunan dan ketiadaan butir "Unit Organisasi" |

### 3.3 Kepatuhan arsitektur frontend

Invariant 34.3 tetap: `key` dan `pathname` unik; butir grup ini tetap tanpa `requiredPermission`. Pengalih memakai pola `redirect()` halaman yang sudah ada di repository.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Butir sidebar | Sidebar yang sudah ada; ikon `RiFlowChart`, `RiGovernmentLine`, `RiUserLine` yang sudah diimpor | `REUSE` | — |

`UI GATE: PASS — 1 elemen REUSE, 0 COMPOSE, 0 NEW`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <6 berkas jsx yang diubah pada slice ini>
OK — 6 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Sidebar di `/hr/master-data/organization-unit` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/organization-menu-1124/FE-HRD-412-01-sidebar-struktur.png` | "Struktur Organisasi" menyala; Departemen dan Posisi di bawahnya *(nama berkas historis `FE-HRD-412`)* |
| Sidebar di `/hr/master-data/organization/departments/create` | `.../FE-HRD-412-02-sidebar-departemen-create.png` | "Departemen" menyala *(nama berkas historis `FE-HRD-412`)* |

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — task ini mengubah navigasi, bukan layar data. State layar tujuan dibahas di `FE-HRD-418` dan `FE-HRD-419` *(nomor lama `FE-HRD-410` dan `FE-HRD-411`)*.

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada panggilan API baru.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | exit 0 | `PASS` | Keluaran perintah |
| `node --test tests/unit/hr-master-data-sidebar-grouping.test.mjs` | 12/12 lulus | `PASS` | Keluaran perintah |
| `npm run test:unit` | 104 gagal, seluruhnya bawaan (jumlah identik dengan HEAD per berkas, termasuk berkas uji lain yang membaca `menu-items.jsx`) | `EXISTING / ENVIRONMENT ISSUE` | Lihat `FE-HRD-418` *(lama `FE-HRD-410`)* bagian 6 |
| `npm run build` | Sukses, 927 halaman; `/hr/master-data/organization` menjadi rute dinamis (pengalih) | `PASS` | Log build |
| Playwright › FE-HRD-412 *(deskripsi spec historis)* | Lulus: tiga pengalihan, menu aktif, tanpa butir "Unit Organisasi" | `PASS` | 5/5 spec lulus |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. 12 butir berurutan tanpa "Unit Organisasi" | Terpenuhi | Uji `HR-01` (urutan `key`/`pathname`), `HR-12`; Playwright |
| 2. Halaman tambah/detail/ubah unit menyalakan "Struktur Organisasi" | Terpenuhi | Uji `HR-12` (`/organization-unit/create`, `/tok-123/update`) |
| 3. `/organization/departments/create` menyalakan "Departemen" | Terpenuhi | Uji `HR-12`; Playwright (`aria-current="page"`) |
| 4. Tiga alamat lama dialihkan dengan benar | Terpenuhi | Playwright FE-HRD-412 |
| 5. Tautan Penempatan Awal: `POSITION_NOT_ASSIGNED` → Posisi; `ORGANIZATION_UNIT_DEPARTMENT_MISMATCH` → Struktur Organisasi | Terpenuhi di source | `SAFE_DESTINATIONS` baris 44–46 |
| 6. Uji sidebar lulus | Terpenuhi | 12/12 |
| DoD: laporan dan tanda roadmap | Terpenuhi | Laporan ini; roadmap frontend bagian 33 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Risiko rilis (`HRD-DEC-258`, nomor lama `HRD-DEC-249`): peran yang hanya memegang `Organization : Read` kini melihat "akses ditolak" di menu Struktur Organisasi; Departemen dan Posisi tetap terbuka. Admin perlu memeriksa peran semacam itu sebelum rilis |
| Masalah yang diketahui | — |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali: untuk membandingkan uji unit dengan HEAD, berkas slice ini dicadangkan ke folder scratch, diganti versi HEAD selama uji, lalu dipulihkan dan diverifikasi byte-per-byte (`cmp`) |
| Status Git | `M CLAUDE.md` (perubahan pengguna, bukan task ini); `M` 15 berkas `src/**` dan `tests/unit/hr-master-data-sidebar-grouping.test.mjs`; `D src/app/hr/master-data/organization/organization-client.jsx`; `?? src/app/hr/master-data/organization/[type]/page.jsx`; `?? tests/e2e/human-resource-organization-menu.spec.mjs` |
| Langkah berikutnya | Commit slice `FE-HRD-418` s.d. `FE-HRD-422` *(nomor lama `FE-HRD-410` s.d. `FE-HRD-414`)* atas instruksi pengguna |
