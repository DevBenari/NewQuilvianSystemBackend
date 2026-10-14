# Laporan Perubahan Frontend — `FE-CRM-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-CRM-002` |
| Judul | Menu CRM |
| Slice | `MVP-1` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/frontend-roadmap.md` § `FE-CRM-002` |
| Trace | `FR-CRM-060`; `CRM-FE-001`, `CRM-FE-002`; `03-frontend-architecture.md` §1 |
| Contract version | Izin `v1`, `approved` (Yoga Aji Pratama, 2026-10-13) |
| Wewenang UI | Posisi menu `DEV_DISCRETION` (roadmap). Label menu mengikuti `CRM-FE-001` |
| Dependency | Tidak ada |
| Klasifikasi | `SMALL`: satu grup menu di satu berkas, satu berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: `menu-items.jsx` dan `tests/unit/`. Backend hanya laporan ini serta roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f1dfd3edd` (branch `YogaV2`) |
| Tanggal | 2026-10-13 |
| Status | Selesai. E2E dua peran (izin disuapkan) lulus |

## 1. Keadaan yang ditemukan di awal

Halaman membership pasien (`/health-services/patient-management/master-data/patient-memberships`
dan `/create`) sudah ada, tetapi tidak muncul di menu samping. Petugas hanya bisa membukanya
lewat URL langsung.

## 2. Proses bisnis dari sisi pengguna

1. Di menu samping, setelah **Manajemen Pasien**, muncul grup **CRM**.
2. Grup itu berisi **Pendaftaran Membership** (membuka form daftar baru) dan **Daftar Membership** (membuka daftar).
3. Petugas yang punya izin `PatientMembership : Read` dan `Create` melihat keduanya. Petugas yang hanya punya `Read` hanya melihat **Daftar Membership**. Petugas tanpa izin membership tidak melihat grup CRM sama sekali.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend, `src/utils/menu-sidebar/menu-items.jsx`,
`src/utils/menu-sidebar/permission/filter-menu-items-by-permission.js`, dan rute halaman
patient-memberships.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Impor ikon `RiVipCrownLine`. Grup `CRM` (`key: healthServicesCrm`) setelah **Manajemen Pasien**, berisi dua menu dengan `requiredPermission` `PatientMembership : Create` dan `PatientMembership : Read`. Akhir baris CRLF dipertahankan |
| `tests/unit/crm-membership-menu.test.mjs` | Baru, 3 uji |

### 3.3 Kepatuhan arsitektur frontend

Memakai struktur butir menu dan penyaring izin yang sudah ada (`requiredPermission`). Tidak ada
mekanisme gating baru.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Tanpa hak akses | Grup CRM disembunyikan oleh penyaring izin menu yang sudah ada |
| Hak akses sebagian | Hanya menu yang izinnya dimiliki yang tampil |

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Penyaringan memakai izin yang sudah dimuat aplikasi.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji: grup CRM berlabel persis, setelah Manajemen Pasien, berisi dua menu | Lulus | `PASS` | `crm-membership-menu.test.mjs` |
| Uji: setiap menu menunjuk halaman yang ada dan dijaga izin `PatientMembership` | Lulus | `PASS` | idem |
| Uji: penyaringan izin (dua menu / tanpa izin / hanya Read) | Lulus | `PASS` | idem |
| `npx eslint` berkas yang berubah | Bersih | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Bersih | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2.787 lulus, 29 gagal | `UNRELATED EXISTING ISSUE` (29) | 29 kegagalan sama dengan laporan `FE-CRM-001` (modul non-CRM). Tidak satu pun memuat menu atau membership |
| `npm run build` | Berhasil ("Compiled successfully in 50s") | `PASS` | Log build |
| E2E: petugas CRM (izin `PatientMembership : Read/Create`) melihat grup CRM dengan dua menu | Lulus | `PASS` | `tests/e2e/crm-membership-registration-screen.spec.mjs` (dijalankan saat `FE-CRM-003`) |
| E2E: pengguna tanpa izin `PatientMembership` tidak melihat grup CRM | Lulus | `PASS` | idem |

Catatan uji: `menu-items.jsx` memuat JSX dan tidak bisa diimpor loader uji (tanpa transform
JSX). Karena itu struktur diperiksa dari teks sumber, dan perilaku penyaringan diuji pada butir
yang bentuknya sama.

E2E dua peran dijalankan dengan menyuapkan `/v1/auth/permissions` (`isSuperAdmin: false`), cara
yang sudah dipakai untuk menguji peran tanpa akun. Akun petugas CRM sungguhan belum ada di Role Access saat itu.

**Susulan 2026-10-14:** jabatan Pendaftaran / Customer Service diberi izin CRM. Dengan akun
`citra.amelia@rsmmc.local` (bukan administrator, `isSuperAdmin=false`), grup menu **CRM** tampil
terhadap backend lokal, dan halaman Daftar Membership serta Pendaftaran Membership terbuka tanpa 403
(`PASS`). Rincian di [`FE-CRM-001.md` §6.1](FE-CRM-001.md#61-at-24--uat-14-dengan-akun-petugas-crm-sungguhan-2026-10-14).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pengguna tanpa izin tidak melihat menu | Terpenuhi | Uji penyaringan + E2E |
| Pengguna CRM melihat dua menu | Terpenuhi | Uji penyaringan + E2E |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | **Pendaftaran Membership** masih membuka form lama, yang menampilkan isian status/tanggal/nomor. Diganti di `FE-CRM-003` |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE`. Build menulis `.next/standalone` (keluaran build biasa) |
| Interupsi | `NONE` |
| Status Git | ` M src/utils/menu-sidebar/menu-items.jsx`, `?? tests/unit/crm-membership-menu.test.mjs`. Belum di-commit |
| Langkah berikutnya | `FE-CRM-006`, lalu `FE-CRM-003`/`004`/`005`/`007` |
