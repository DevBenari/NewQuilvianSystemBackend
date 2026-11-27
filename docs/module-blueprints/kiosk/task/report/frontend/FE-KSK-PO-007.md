# Laporan Perubahan Frontend — `FE-KSK-PO-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-007` |
| Judul | Daftar Pendaftaran Online dan butir menu |
| Slice | `EPIC KSK-PO-07` — layar petugas (`FR-KSK-PO-060`), gelombang 1, rilis `MVP-3` |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-007` |
| Trace | `KSK-DEC-050`; `KSK-PO-FE-005`; `KSK-DSN-PO-017` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`), grup *Online Registration* `GET /`, `GET /summary`; validation matrix `KSK-VAL-PO-068`, `069` |
| Wewenang UI | `KSK-PO-FE-005`: isi dan aksi sesuai `KSK-DEC-050`; letak menu dikunci di grup yang sama dengan "Reservasi Prioritas" (`KSK-DEC-054`); tata letak mengikuti pola layar Reservasi Prioritas (`DEV_DISCRETION`) |
| Dependency | `BE-KSK-PO-009` ✅ |
| Klasifikasi | `MEDIUM` — route petugas baru, slice Redux baru, butir menu dengan hak akses |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: route, view, kolom, hook, slice + registrasi store, service, konstanta, util, style, `menu-items.jsx`; laporan dan baris status di blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `508c8beb8` (`sukmagpV2`) + perubahan `FE-KSK-PO-001` s.d. `003` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `93f0fa1b` (`sukmagp`) + perubahan `BE-KSK-PO-008`/`010` yang belum di-commit |
| Tanggal | 2026-11-20 |
| Status | ✅ Selesai — AC 18 (ber-hak dan tanpa hak), keadaan memuat/kosong/gagal, dan butir menu terbukti di browser |

## 1. Keadaan yang ditemukan di awal

Backend `BE-KSK-PO-009` sudah menyediakan `GET /online-registrations` (saringan `visitDateFrom`, `visitDateTo`, `status`, `search`, `pageNumber`, `pageSize`) dan `GET /summary`, dengan hak `OnlineRegistration : Read`. Di frontend belum ada route, slice, service, maupun butir menu. Layar rujukan terdekat, dan yang diminta roadmap, adalah Daftar Reservasi Prioritas (`priority-reservations`), dengan bentuk kontrak yang sama: amplop `ApiResponse`, halaman `items/totalData/totalPage`, dan ringkasan per status.

## 2. Proses bisnis dari sisi pengguna

1. Petugas admisi yang punya hak `OnlineRegistration : Read` membuka menu **Rawat Jalan → Pendaftaran Online**. Butir ini tepat di bawah "Reservasi Prioritas".
2. Layar menampilkan lima kartu ringkasan untuk rentang tanggal yang dipilih: Menunggu Check-in, Menjadi Kunjungan, Jalur Lain, Tidak Datang, Dibatalkan. Contoh pada 20 Nov 2026: 13, 0, 2, 0, 2.
3. Saringan bawaan adalah **30 Hari ke Depan** (hari ini s.d. 29 hari ke depan), semua status. Petugas dapat:
   - memilih periode lain (Hari Ini, Besok, 7 Hari ke Depan, 7 Hari Terakhir, Bulan Ini);
   - mengisi Tanggal Awal/Akhir sendiri (Rentang Tanggal);
   - memilih status;
   - mencari dengan kode, nomor pendaftaran, nama, No. RM, atau NIK.

   Kata kunci kurang dari 3 huruf tidak dikirim, dan layar menampilkan "Kata kunci pencarian minimal 3 karakter.". Rentang lebih dari 62 hari tidak dikirim, dan layar menampilkan "Rentang tanggal tidak valid.".
4. Tabel menampilkan: tanggal kunjungan, nomor pendaftaran + waktu kirim, pasien (No. RM atau "Pasien baru"), poli + dokter + jam, jenis pasien + penjamin, kontak + status email, dan status. Kode check-in tidak pernah ada di layar. Klik dua kali baris membuka detail (`FE-KSK-PO-008`).
5. Petugas tanpa hak tidak melihat butir menu. Bila membuka alamatnya langsung, layar diganti "Ups! Akses Ditolak" karena backend menjawab `403`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Seluruh modul `priority-reservations` (route, list client/view, kolom, slice, hook, service, konstanta, util, style), `access-denied-gate.jsx`, `data-table.jsx`, `pagination.jsx`, `menu-items.jsx`, `store.jsx`. Di backend: `OnlineRegistrationController.cs`, `OnlineRegistrationDtos.cs` (`OnlineRegistrationListQuery`, `OnlineRegistrationListItemResponse`, `OnlineRegistrationSummaryResponse`), `OnlineRegistrationStatus.cs`.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/app/health-services/registration-management/online-registrations/page.jsx` | Baru | Route, judul "Pendaftaran Online" |
| `src/components/view/health-services/registration-management/online-registrations/online-registration-list-client.jsx` | Baru | Pembungkus client (`Suspense`) |
| `src/components/view/health-services/registration-management/online-registrations/online-registration-list-view.jsx` | Baru | Hero, `SummaryCards`, `DataFilter` (tanggal, periode, status, jumlah baris, cari), `DataTable` + `Pagination`, pesan rentang/kata kunci/gagal, `AccessDeniedGate` |
| `src/components/view/health-services/registration-management/online-registrations/online-registration-table-columns.jsx` | Baru | 8 kolom, `StatusBadge` |
| `src/lib/hooks/health-services/registration-management/online-registrations/use-online-registration-list.jsx` | Baru | Saringan, periode, rentang, kata kunci, paginasi, Atur ulang, buka detail; request dibatalkan saat berganti saringan atau unmount |
| `src/lib/state/slice/health-services/registration-management/online-registration-slice.js` | Baru | `fetchOnlineRegistrations`, `fetchOnlineRegistrationSummary`, filter; kode status galat disimpan untuk `403` |
| `src/lib/state/store.jsx` | Diubah | Reducer `onlineRegistration` |
| `src/lib/services/health-services/registration-management/online-registration.service.js` | Baru | `getOnlineRegistrations`, `getOnlineRegistrationSummary` |
| `src/lib/constants/health-services/registration-management/online-registration-constants.js` | Baru | Route, API, resource/aksi, status + label, periode (bawaan 30 hari ke depan), batas `068`/`069`, pesan |
| `src/utils/health-services/registration-management/online-registration/online-registration-staff.utils.js` | Baru | Rentang 62 hari, label/nada status, No. RM/"Pasien baru", kartu ringkasan; pembacaan amplop/halaman/periode memakai helper Reservasi Prioritas |
| `src/style/health-services/registration-management/online-registrations/online-registration.module.css` | Baru | Saringan satu baris di layar ≥ 1200 px (aturan sama dengan Reservasi Prioritas) |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah | Butir "Pendaftaran Online" di bawah "Reservasi Prioritas", `requiredPermission` `OnlineRegistration : Read` |

Tidak ada perubahan backend, migration, atau database. Satu akun fixture uji dibuat lewat API (bagian 6).

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Penerapan |
| --- | --- |
| Route tipis, view memanggil hook | `page.jsx` → client → view → `useOnlineRegistrationList` |
| Data server bersama di Redux | Slice `onlineRegistration`, tanpa persist (store tidak memakai redux-persist) |
| Tanpa abstraksi paralel | Pembacaan amplop, halaman, periode, dan format tanggal memakai helper Reservasi Prioritas yang kontraknya sama; tidak disalin |
| Hak akses dari layar Akses Role | Menu `requiredPermission`; layar mengikuti `403` backend lewat `AccessDeniedGate`; tidak ada pemeriksaan peran hardcode |
| Privasi | Kode check-in tidak ada di respons; data hanya di memori |

### 3.4 Gerbang keputusan base component

UI GATE: 10 elemen — REUSE 9, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Kepala layar | `Hero` | dipakai Reservasi Prioritas | REUSE | eyebrow "Rawat Jalan" |
| Ringkasan per status | `SummaryCards` (`summary-grid`) | sama | REUSE | 5 kartu, gagal → "—" |
| Saringan + cari + Atur ulang | `DataFilter` | sama | REUSE | — |
| Tanggal Awal/Akhir | `FilterDatePicker` | sama | REUSE | — |
| Periode, status, jumlah baris | `FilterSelect` | sama | REUSE | — |
| Tabel + paginasi | `DataTable`, `Pagination` | sama | REUSE | — |
| Status | `StatusBadge` | sama | REUSE | nada per status |
| Pesan rentang/kata kunci/gagal | `InformationAlert`, `BaseButton` | sama | REUSE | — |
| Tanpa hak | `AccessDeniedGate` | sama | REUSE | — |
| Saringan satu baris | CSS feature | aturan `.singleRowFilter` Reservasi Prioritas | COMPOSE | Aturan yang sama di CSS modul sendiri |

Untuk elemen COMPOSE (saringan satu baris) ada dua pilihan:
- **A. Salin 4 aturan tata letak ke CSS modul sendiri (rekomendasi, dijalankan).** Modul tidak bergantung pada stylesheet modul lain, dan perubahan di Reservasi Prioritas tidak ikut menggeser layar ini.
- **B. Impor `priority-reservation.module.css`.** Tanpa salinan, tetapi kedua modul jadi saling terikat lewat CSS.

Aturan itu memuat dua ukuran tata letak literal yang sama dengan sumbernya, yaitu `1200px` (titik henti) dan `8.5rem` (lebar Jumlah Baris). Keduanya tidak memuat warna maupun tipografi. Tidak ada elemen `NEW` atau `EXTEND`.

## 4. State yang ditangani di layar

| Keadaan | Penanganan |
| --- | --- |
| Memuat | Kerangka `DataTable` "Mengambil data pendaftaran online...", kartu ringkasan memuat, saringan nonaktif |
| Kosong | "Belum ada pendaftaran online pada saringan ini." + petunjuk Atur ulang (tombol Atur ulang di saringan) |
| Gagal | "Data gagal dimuat." + [Coba lagi]; ringkasan gagal → angka "—" |
| Kata kunci < 3 | "Kata kunci pencarian minimal 3 karakter."; kata kunci tidak dikirim |
| Rentang > 62 hari | "Rentang tanggal tidak valid."; daftar dan ringkasan tidak dikirim |
| Tanpa hak | Butir menu tersembunyi; `403` → `AccessDeniedGate` |

## 5. Endpoint yang dikonsumsi

| Endpoint | Pemakaian |
| --- | --- |
| `GET /api/v1/health-services/registration-management/online-registrations?visitDateFrom=&visitDateTo=&status=&search=&pageNumber=&pageSize=` | Tabel |
| `GET /api/v1/health-services/registration-management/online-registrations/summary?visitDateFrom=&visitDateTo=` | Kartu ringkasan |

## 6. Verifikasi

Lingkungan uji: dev server frontend di port 3000 dan backend uji di `https://localhost:7185` (build Release dari scratchpad). Request browser ke 7184 dialihkan ke 7185. Keduanya saya nyalakan sendiri, lalu saya matikan sesudah uji. Backend user di 7184 tidak disentuh. Akun uji:
- SuperAdmin, untuk memeriksa letak menu.
- `TEST-PO009 reader` (hak `OnlineRegistration : Read`).
- **`TEST-PO007 noread`**, akun baru tanpa hak `OnlineRegistration`, dibuat lewat API SuperAdmin di unit TEST-PO009. Akun ini perlu dibuat karena jabatan `TEST-PO009 noaccess` sudah diberi `Read` oleh uji `BE-KSK-PO-010`.

Kredensial tidak dicetak.

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` pada berkas yang disentuh | PASS — 0 error, 0 warning |
| `npm run lint:errors` | PASS — 0 error |
| `npm run build` | PASS — 855/855 halaman, `○ /health-services/registration-management/online-registrations` |
| Playwright 1366 px (`list-check.cjs`) | **22/22 PASS** |
| Paginasi ketat (`page-check.cjs`) | **PASS** — 10 baris halaman 1 ("Menampilkan 1 sampai 10 dari 17 data"); tautan Halaman 2 → `pageNumber=2&pageSize=10`, 7 baris ("11 sampai 17") |
| Grep konsistensi UI | Bersih: tanpa hex/rgba, tanpa ukuran huruf, tanpa `<button>`/`<table>` mentah, tanpa `!important`, tanpa inline style. Dua ukuran tata letak literal tercatat di 3.4 |

Rincian uji browser:

| Butir | Hasil |
| --- | --- |
| M1 | Menu "Pendaftaran Online" satu grup dengan "Reservasi Prioritas", tepat di bawahnya, menuju route yang benar (`KSK-PO-FE-005`) |
| R1 | Akun ber-hak: buka grup Rawat Jalan → butir menu tampil → klik membuka layar (DoD menu terjangkau) |
| R2–R5 | Hero dan 5 kartu ringkasan; request bawaan `visitDateFrom=2026-11-20&visitDateTo=2026-12-19`, semua status; 17 baris dengan "Pasien baru" untuk pasien baru; tidak ada teks berpola kode `XXXX-XXXX` |
| R6 | Status Menunggu Check-in → `status=1`, 13 baris semuanya berstatus itu |
| R7 | Kata kunci "ab" → petunjuk 3 karakter, tidak dikirim (`KSK-VAL-PO-068`) |
| R8–R10 | Cari nomor `PO-20261120-0014` → 1 baris; cari NIK pasien lama → 3 baris termasuk `0011` dan `0013`; cari nama → barisnya |
| R11–R12 | Kata kunci tanpa hasil → "Belum ada pendaftaran online pada saringan ini."; Atur ulang → kata kunci/status kosong dan rentang bawaan |
| R13–R14 | Rentang 1 Nov 2026 – 15 Jan 2027 (76 hari) → "Rentang tanggal tidak valid." dan tidak dikirim (`KSK-VAL-PO-069`); 1–30 Nov dikirim apa adanya |
| R16 | Daftar `500` (tiruan) → "Data gagal dimuat." + Coba lagi; sesudah pulih, Coba lagi memuat baris |
| R17–R19 | Klik dua kali baris → `/online-registrations/{id}`; 1366 px tanpa gulir mendatar; tanpa galat halaman |
| N1–N2 | Akun tanpa hak: butir menu tidak tampil; buka route langsung → backend `403` → "Ups! Akses Ditolak", tabel tidak tampil (AC 18) |

```
MANUAL TEST: Playwright — 22/22 + paginasi ketat 1/1
AUTOMATED TEST: SKIPPED (opsional) — tes otomatis baru tidak diwajibkan test-policy; bukti perilaku dari uji browser di atas
```

Catatan jalannya uji:
- Run pertama 13/22. Semua kegagalan berasal dari skrip:
  - Kartu ringkasan dan badge memakai huruf besar lewat CSS.
  - Kotak "Cari menu..." di sidebar tertangkap sebagai kotak cari.
  - Grup menu reader belum dibuka.
  - Akun `noaccess` ternyata sudah ber-hak (lihat di atas).
- Pada run kedua, R15 hanya "lewati", karena tombol paginasi berbentuk tautan `a[aria-label='Halaman 2']`. Skrip paginasi terpisah membuktikannya.
- Skrip dan tangkapan layar ada di scratchpad sesi (`fe-po007/list-check.cjs`, `page-check.cjs`, `list-1366.png`, `noaccess-1366.png`; `po002/fe007_fixture.py`) dan tidak di-commit.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Bukti | Status |
| --- | --- | --- |
| AC 18: petugas ber-hak membuka daftar; filter tanggal dan status; cari kode, nomor, nama, No. RM, NIK; hasil sesuai; kode tidak pernah tampil | R1–R14; pencarian kode dicocokkan backend lewat hash (`BE-KSK-PO-009`) dan dikirim apa adanya oleh layar | ✅ |
| AC 18: petugas tanpa hak → `403`, menu tidak tampil, `AccessDeniedGate` | N1, N2 | ✅ |
| Keadaan memuat, kosong, gagal sesuai PO-FE.3 | R11, R16, bagian 4 | ✅ |
| DoD: butir menu terjangkau | M1, R1 | ✅ |

## 8. Catatan penutup

- Pencarian dengan kode check-in tidak diuji ulang di browser, karena kode asli hanya tampil sekali kepada pasien. Layar mengirim kata kunci apa adanya, dan backend mencocokkan kode persis lewat hash (sudah terbukti di `BE-KSK-PO-009`).
- Fixture uji bertambah satu jabatan (`TEST-PO007 noread`) dan satu akun pegawai. Keduanya dibersihkan bersama fixture TEST-PO009.
- Layar detail yang dibuka lewat klik dua kali adalah task `FE-KSK-PO-008`. Sampai task itu selesai, rute detail belum punya halaman.
- Tidak ada commit, push, migration, atau eksekusi database langsung.

**Task berikutnya:** `FE-KSK-PO-008` (Detail, Ubah Jadwal, Ubah Kontak, Batalkan). Dependency `FE-KSK-PO-007` ✅ dan `BE-KSK-PO-010` ✅.
