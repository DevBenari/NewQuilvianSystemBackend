# Laporan Perubahan Frontend — `FE-CRM-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-CRM-001` |
| Judul | Lapisan data ke rute `/admin` |
| Slice | `MVP-1`, `EPIC CRM-07` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/frontend-roadmap.md` § `FE-CRM-001` |
| Trace | `FR-CRM-063`; `CRM-DEC-018`; `CRM-FE-005`; `03-frontend-architecture.md` §3 |
| Contract version | API `v2`, `approved` (Yoga Aji Pratama, 2026-10-13) |
| Wewenang UI | Tidak ada perubahan tampilan. Task ini hanya lapisan data (slice Redux) |
| Dependency | `BE-CRM-011` 🟡, `BE-CRM-012` ✅, `BE-CRM-013` ✅, `BE-CRM-014` ✅, `BE-CRM-023` ✅. **Rilis bersama `BE-CRM-015`** |
| Klasifikasi | `MEDIUM`: satu slice (rute, 8 thunk baru, state riwayat/lookup), satu berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: slice membership dan `tests/unit/`. Backend hanya laporan ini serta roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f1dfd3edd` (branch `YogaV2`). Berkas membership tidak berubah sejak snapshot desain `f1e5e273a` |
| Commit backend yang dijadikan rujukan | `83ef27bd` + perubahan `BE-CRM-011`…`021` yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. `AT-24`/`UAT-14` lulus 2026-10-14 dengan akun petugas CRM sungguhan (Pendaftaran / Customer Service). Tetap dirilis bersama `BE-CRM-015` |

## 1. Keadaan yang ditemukan di awal

`master-data-patient-membership-slice.jsx` memanggil:

- `GET /`, `GET /options`, `POST /` milik policy `KioskRead`. Petugas non-administrator ditolak 403, dan `POST /` kini sudah dihapus backend (`BE-CRM-015`).
- `PUT`/`PATCH status`/`DELETE` yang kini 410.
- Rute tingkat `/v1/health-services/master-data/membership-tiers` yang tidak ada di backend, sehingga setiap pemuatan pilihan diawali satu permintaan gagal sebelum jatuh ke rute Administrator.

Belum ada thunk untuk perpanjangan, upgrade, batal, verifikasi, catatan, riwayat, maupun cari pasien.

## 2. Proses bisnis dari sisi pengguna

Task ini tidak mengubah layar. Dampaknya bagi petugas CRM:

1. Daftar, ringkasan, metadata filter, pilihan, dan detail membership dimuat lewat rute ber-izin `/admin`, sehingga petugas CRM yang punya `PatientMembership : Read` bisa membuka halaman walaupun bukan administrator.
2. Simpan pendaftaran baru mengirim `POST /admin` berisi pasien, tingkat, dan catatan saja. Isian lama (status, tanggal, nomor, poin) tidak dikirim lagi, karena diisi sistem.
3. Tombol ubah atau hapus pada halaman lama akan menampilkan pesan 410 dari backend, misalnya "Fitur ubah membership sudah tidak tersedia. Gunakan ubah catatan, upgrade, atau pembatalan.", sampai halamannya diganti di `FE-CRM-005`.
4. Layar berikutnya (`FE-CRM-003`/`005`) tinggal memakai thunk baru.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend (Redux, API/Axios, mode lintas repository, validasi, pelaporan),
slice membership, `src/lib/state/store.jsx`, empat hook pemakai slice (daftar, detail, editor,
detail pasien), `tests/unit/auth-permission-strict-decision.test.mjs` (pola tiruan
`InstanceAxios`), dan kontrak API `v2`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/health-services/patient-management/master-data/master-data-patient-membership-slice.jsx` | Semua panggilan pindah ke `/admin`. `createPatientMembership` mengirim `POST /admin` dengan tiga field. Thunk baru: `renewPatientMembership`, `upgradePatientMembership`, `cancelPatientMembership`, `verifyLegacyPatientMembership`, `updatePatientMembershipNotes`, `getPatientMembershipStatusHistories`, `lookupPatientsForMembership`. State dan selector baru untuk riwayat dan hasil cari pasien. Ringkasan membaca 5 hitungan status baru. Fungsi fallback rute tingkat lama dihapus. Thunk lama `update`/`delete`/`updateStatus` dipertahankan (menuju 410) agar pemakai tidak putus. Akhir baris CRLF dipertahankan |
| `tests/unit/crm-membership-slice-admin-routes.test.mjs` | Baru, 7 uji perilaku (thunk dipanggil dengan `InstanceAxios` tiruan) |

### 3.3 Kepatuhan arsitektur frontend

Tetap satu slice Redux yang sama (`masterDataPatientMembership`) dengan pola thunk, penanganan
error, dan selector yang sudah ada. Tidak ada Axios instance baru, service paralel, maupun
komponen baru. Nama ekspor lama tidak diubah.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah. Ditambah `historiesLoading` dan `lookupLoading` untuk layar berikutnya |
| Kosong | Tidak berubah |
| Gagal | Pesan dari `ApiResponse` backend diteruskan ke `actionError`/`lookupError`/`historiesError`, termasuk pesan 410 |
| Tanpa hak akses | Rute `/admin` mengikuti izin `PatientMembership : *`. Tanpa izin, backend menjawab 403 dan pesannya tampil seperti galat lain |

## 5. Endpoint yang dikonsumsi

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/.../patient-memberships/admin` | Daftar | `PatientMembership : Read` |
| `GET` | `.../admin/summary`, `/admin/filters/metadata`, `/admin/options`, `/admin/{id}` | Ringkasan, metadata, pilihan, detail | `PatientMembership : Read` |
| `POST` | `.../admin` | Daftar baru | `PatientMembership : Create` |
| `POST` | `.../admin/{id}/renewals`, `/admin/{id}/upgrades` | Perpanjangan, upgrade | `PatientMembership : Create` |
| `POST` | `.../admin/{id}/cancel` | Batal | `PatientMembership : Cancel` |
| `POST` | `.../admin/{id}/verify` | Verifikasi data lama | `PatientMembership : Verify` |
| `PATCH` | `.../admin/{id}/notes` | Ubah catatan | `PatientMembership : Update` |
| `GET` | `.../admin/{id}/status-histories`, `/admin/patient-lookup` | Riwayat, cari pasien | `PatientMembership : Read` |

#### Administrator / Master Data / Membership Tier

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/administrator/master-data/membership-tiers/options` | Pilihan tingkat (langsung, tanpa rute lama yang gagal) | `MembershipTier : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji: daftar/ringkasan/metadata/pilihan/detail ke `/admin` | Lulus. Tidak ada panggilan ke `/` maupun `/kiosk`. Parameter kosong tidak dikirim | `PASS` | `crm-membership-slice-admin-routes.test.mjs` |
| Uji: daftar baru hanya tiga field | Lulus. `memberNumber`, status, tanggal, dan poin dibuang | `PASS` | idem |
| Uji: lima perintah ke endpoint masing-masing | Lulus | `PASS` | idem |
| Uji: riwayat dan cari pasien tersimpan di state | Lulus | `PASS` | idem |
| Uji: pilihan tingkat tanpa rute lama | Lulus | `PASS` | idem |
| Uji: pesan 410 diteruskan ke `actionError` | Lulus | `PASS` | idem |
| Uji: ringkasan membaca 5 hitungan baru | Lulus | `PASS` | idem |
| `npx eslint` pada dua berkas yang berubah | Bersih | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Bersih | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2.778 lulus, 29 gagal | `UNRELATED EXISTING ISSUE` (29) | 29 kegagalan ada di 22 berkas (Hemodialisa, rawat inap, kamar operasi, kas kecil, resep RJ). Tidak satu pun memuat store maupun slice membership, dan semuanya kegagalan assertion modul lain. Baseline catatan 2026-10-06 adalah 8, naik setelah merge |
| `npm run build` | Berhasil ("Compiled successfully in 59s", standalone siap) | `PASS` | Log build |

Uji manual pada pengerjaan awal: `NOT FEASIBLE`, karena belum ada akun petugas CRM berizin. Sudah dijalankan kemudian, lihat di bawah.

### 6.1 `AT-24` / `UAT-14` dengan akun petugas CRM sungguhan (2026-10-14)

Persiapan atas instruksi owner:
- Jabatan **Pendaftaran / Customer Service** (sebelumnya 0 kebijakan) diberi `PatientMembership : Read, Create, Update, Cancel, Verify` dan `MembershipTier : Read`.
- Izin disimpan lewat `POST /api/v1/administrator/setting/role-access/policies` sebagai superadmin. Set dibaca sebelum dan sesudah: 0 → 6, tidak ada izin lain yang tertimpa.
- `PatientMembership : Delete` tidak diberikan karena endpointnya sudah 410.
- Akun: `citra.amelia@rsmmc.local`, satu-satunya akun jabatan itu, bertanda wajib ganti sandi. Backend tidak memblokir login karena tanda itu, sehingga sandi tidak diubah. Sandi diberikan owner, dipakai lewat variabel lingkungan, dan tidak disimpan.

Lingkungan sama dengan [`FE-CRM-005.md` §6.1](FE-CRM-005.md#61-uat-terhadap-backend-lokal-2026-10-14), kecuali sesi memakai akun di atas.

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Login dan izin efektif | Login 200; `isSuperAdmin=false`; tepat enam izin di atas | `PASS` | `GET /auth/permissions` |
| Menu CRM dan Daftar Membership | Grup menu **CRM** tampil; daftar termuat (`GET /admin` 200, tidak ada 403); tombol "+ Pendaftaran Membership" tampil | `PASS` | Tangkapan layar + log permintaan |
| Detail Menunggu Pembayaran `MBR-000002` | Tombol hanya Ubah Catatan dan Batalkan; ubah catatan `PATCH /notes` 200. Riwayat di DB mencatat pelaku `citra.amelia@rsmmc.local` | `PASS` | Log permintaan + kueri baca-saja |
| Detail Kedaluwarsa `UAT-LAMA-003` | Tanpa tombol aksi | `PASS` | Tangkapan layar |
| Pendaftaran (`AT-24`/`UAT-14`) | Cari RM 00-00-00-16 → Indra Gunawan → Silver → simpan: "Membership MBR-000003 terdaftar — Menunggu Pembayaran", iuran Rp 1.200.000, batas bayar 21 Okt 2026 13.02 | `PASS` | `POST /admin` 200 |
| Panggilan ditolak selama uji | Tidak ada 403. Sesudah perbaikan di bawah, juga tidak ada 404 | `PASS` | Log permintaan |

**Temuan: kriteria "tidak ada lagi permintaan gagal ke rute tingkat lama" ternyata belum terpenuhi di layar pendaftaran.**
- Slice memang sudah bersih. Namun isian tingkat di formulir pendaftaran memakai `optionsSource: "membershipTierOptions"`, dan `BaseGroupedEditorField` memperlakukannya sebagai pilihan jarak-jauh `membershipTiers`.
- Registri pilihan bersama mendahulukan entri Health Services (`health-service-select-resources.js`) yang menunjuk `/v1/health-services/master-data/membership-tiers/options`. Rute itu tidak ada di backend, sehingga 404 dua kali tiap formulir dibuka. Pilihan tetap benar karena opsi lokal yang dipakai.
- **Perbaikan:** isian tingkat tidak lagi memakai `optionsSource`/`optionsKey`, dan opsinya diberikan lewat `optionMap.membershipTierIdOptions` yang dibaca dari nama isian (`patient-membership-constants.jsx`, `use-patient-membership-registration.jsx`).
- Uji unit memastikan isian itu tanpa kunci sumber jarak-jauh, dan E2E pendaftaran memastikan tidak ada permintaan ke rute lama. Sesudah build ulang, `AT-24` hanya mencatat satu panggilan pilihan tingkat (200).
- Entri registri bersama sengaja tidak diubah, karena dipakai modul lain (lihat §8).

Regresi sesudah perbaikan: 28 skenario E2E CRM lulus, 54 uji unit CRM lulus. Suite penuh 2.854 uji dengan 29 gagal yang daftarnya identik dengan baseline. `lint:errors` dan build bersih.

Uji manual: `PASS` (lewat Playwright terhadap backend lokal dengan akun petugas sungguhan).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Petugas CRM tanpa role Administrator dapat memuat daftar (`AT-24`) | Terpenuhi 2026-10-14: akun Customer Service memuat daftar dan mendaftarkan membership | §6.1 |
| Tidak ada lagi permintaan gagal ke rute tingkat lama | Terpenuhi sesudah perbaikan formulir pendaftaran 2026-10-14. Klaim awal hanya berlaku untuk slice | Uji unit + E2E + §6.1 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Rilis berpasangan dengan `BE-CRM-015`.** Frontend lama tanpa task ini akan putus terhadap backend baru, dan sebaliknya |
| Masalah yang diketahui | Halaman ubah lama (`[slug]/update`) dan tombol hapus/ubah status masih ada dan kini menampilkan pesan 410. Diganti di `FE-CRM-005`. Formulir pendaftaran lama masih menampilkan isian status/tanggal/nomor, yang kini diabaikan saat simpan. Diganti di `FE-CRM-003` |
| Dependency backend | `BE-CRM-011` 🟡 (uji paralel `AT-12` di PostgreSQL), `BE-CRM-015` 🟡 (menunggu rilis bersama task ini) |
| Perubahan sampingan | `NONE`. Build menulis `.next/standalone` (keluaran build biasa) |
| Interupsi | `NONE` |
| Status Git | ` M src/lib/state/slice/.../master-data-patient-membership-slice.jsx`, `?? tests/unit/crm-membership-slice-admin-routes.test.mjs`. Belum di-commit |
| Temuan di luar cakupan | Registri pilihan bersama `src/lib/hooks/select/health-service/health-service-select-resources.js` masih menunjuk `membershipTiers` ke `/v1/health-services/master-data/membership-tiers/options` yang tidak ada. Karena `getSelectResource` mendahulukan registri Health Services, pemakai `useSelectResource("membershipTiers")` (mis. `use-health-service-select.js`) tetap mendapat 404. Perlu diputuskan pemilik registri: arahkan ke `/v1/administrator/master-data/membership-tiers/options` atau hapus entrinya |
| Langkah berikutnya | Commit dan rilis bersama `BE-CRM-015`. Laporkan temuan registri pilihan ke pemiliknya |
