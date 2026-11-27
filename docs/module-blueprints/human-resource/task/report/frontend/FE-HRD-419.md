# Laporan Perubahan Frontend — `FE-HRD-419`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-419` |
| Nomor lama | `FE-HRD-411` (dinomori ulang saat merge `QuilvianIntegrationBackEnd` 27 November 2026 agar `FE-HRD-411` milik `A52-B` tidak berubah) |
| Judul | Halaman Departemen dan halaman Posisi berdiri sendiri |
| Slice | `S-FIN-M05` amandemen — Menu Organisasi (`PLAN-REPAIR-HRD-008`, `FIX-HRD-008-02`) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` bagian 33 (nomor lama bagian 32) |
| Trace | `HRD-DEC-257` (nomor lama `HRD-DEC-248`); `ISS-HRD-008-02`, `ISS-HRD-008-T4`; skema `PLAN-REPAIR-HRD-008` 3.4 |
| Contract version | `v30` tetap; endpoint `organization/departments` dan `organization/positions` tidak berubah |
| Wewenang UI | Susunan sesuai skema 3.4 (tanpa tab, tanpa mode Hierarki) |
| Dependency | `FE-HRD-418` ✅ (nomor lama `FE-HRD-410`) |
| Klasifikasi | `MEDIUM` — 1 halaman baru, 7 berkas diubah, perubahan sumber jenis data dari `?type=` ke segmen route |
| Task mode | `FRONTEND` (disetujui pengguna 24 November 2026) |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fe8aebf2f` (branch `HamzahV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `3ef27c96` (branch `MHamzah`) |
| Tanggal | 2026-11-24 |
| Status | ✅ Selesai — 5/5 AC; lint, build, Playwright lulus. Uji dengan backend nyata `NOT RUN` |

---

## 1. Keadaan yang ditemukan di awal

Departemen dan Posisi hanya berupa tab di halaman `/hr/master-data/organization` (judul "Manajemen Organization"). Jenis data dibaca dari `?type=` (`use-master-data-organization.jsx:315-317`), dan tombol kembali dari tambah/detail/ubah menuju `/hr/master-data/organization?type=…`. Teks "Organization" muncul di judul, pesan memuat, pesan kosong, dan judul tab peramban "Tambah Organization".

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas HR membuka menu **Departemen**, lalu melihat daftar departemen.
2. Petugas menekan **+ Tambah Departemen**, mengisi, dan menyimpan. Tombol kembali mengarah ke `/hr/master-data/organization/departments`.
3. Untuk posisi, petugas membuka menu **Posisi**, menyaring per departemen, menambah posisi, atau menekan **Buka Uraian** untuk Uraian Tugas.

Jalur tidak normal: alamat jenis tidak dikenal (mis. `/hr/master-data/organization/abc`) → halaman "tidak ditemukan" aplikasi (layar Coming Soon, `src/app/not-found.jsx`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`master-data-organization-view.jsx`, `use-master-data-organization.jsx`, `use-master-data-organization-editor.jsx`, `use-master-data-organization-detail.jsx`, `organization-constants.jsx`, halaman `src/app/hr/master-data/organization/[type]/**`, `route-param-utils.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| **Baru** `src/app/hr/master-data/organization/[type]/page.jsx` | Halaman daftar; hanya menerima `departments`/`positions` (selain itu `notFound()`); judul tab peramban per jenis lewat `generateMetadata` |
| `src/components/view/hr/master-data/organization/master-data-organization-view.jsx` | Menerima `organizationType`; tab, tombol mode, dan impor pohon dibuang; judul `Hero` per jenis; pesan memuat/kosong per jenis; placeholder "Periode" |
| `src/lib/hooks/hr/master-data/organization/use-master-data-organization.jsx` | Jenis aktif dari argumen `type` (segmen route); `useSearchParams`, `usePathname`, dan `changeType` (khusus tab) dibuang |
| `src/lib/constants/hr/master-data/organization/organization-constants.jsx` | "Daftar Departemen"/"Daftar Posisi", `heroDescription` per jenis, label periode bawaan "Periode" |
| `src/lib/hooks/hr/master-data/organization/use-master-data-organization-editor.jsx`, `use-master-data-organization-detail.jsx` | Alamat kembali `/hr/master-data/organization/{type}`; konstanta `ORGANIZATION_TABLE_PATH` yang tak terpakai dihapus |
| `src/app/hr/master-data/organization/[type]/create/page.jsx` | Judul tab peramban "Tambah Departemen"/"Tambah Posisi" |

### 3.3 Kepatuhan arsitektur frontend

View, hook, slice, dan service tetap milik domain organisasi. Halaman baru hanya route tipis yang merender view yang sama dengan jenis tetap. Tidak ada komponen atau hook baru.

### 3.4 Gerbang tampilan

**Modul referensi visual:** tampilan tab lama di halaman yang sama.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Kepala halaman | `Hero` | `REUSE` | — |
| Kartu, penyaring, tabel | `SummaryCards`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `DataTable` | `REUSE` | Prop `tabs` tidak lagi dikirim |

`UI GATE: PASS — 2 elemen REUSE, 0 COMPOSE, 0 NEW`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <6 berkas jsx yang diubah pada slice ini>
OK — 6 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hr/master-data/organization/departments` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/organization-menu-1124/FE-HRD-411-01-departemen.png` | Sesuai skema 3.4 (diambil sebelum perbaikan label "Periode"; build akhir memuat label baru; nama berkas historis `FE-HRD-411`) |
| `/hr/master-data/organization/positions` | `.../FE-HRD-411-02-posisi.png` | Sesuai skema 3.4; tombol "Buka Uraian" tetap (nama berkas historis `FE-HRD-411`) |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil data departemen..." / "Mengambil data posisi..." |
| Kosong | "Data departemen tidak ditemukan." / "Data posisi tidak ditemukan." |
| Gagal | `InformationAlert` (sudah ada) |
| Tanpa hak akses | `AccessDeniedGate` (hak `Organization : Read`) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Master Data / Organization

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/organization/departments` | Daftar departemen | `Organization : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/organization/positions` | Daftar posisi | `Organization : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/organization/summary` | Kartu ringkasan | `Organization : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/organization/departments/options` | Penyaring departemen pada Posisi | `Organization : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | exit 0 | `PASS` | Keluaran perintah |
| `npm run test:unit` | 104 gagal, seluruhnya bawaan (jumlah gagal identik dengan HEAD per berkas) | `EXISTING / ENVIRONMENT ISSUE` | Lihat `FE-HRD-418` (lama `FE-HRD-410`) bagian 6 |
| `npm run build` | Sukses, 927 halaman; route `ƒ /hr/master-data/organization/[type]` terdaftar | `PASS` | Log build |
| Playwright › FE-HRD-411 *(deskripsi spec historis)* | Lulus: judul, tombol tambah, tanpa tab/Hierarki, tanpa teks "Organization", navigasi ke `/positions/create`, `/abc` → layar tidak ditemukan | `PASS` | 5/5 spec lulus |
| Uji dengan backend nyata | Tidak dijalankan | `NOT RUN` | Sama dengan `FE-HRD-418` |

Uji manual: `NOT FEASIBLE` dengan backend nyata pada sesi ini.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `/departments` berjudul "Departemen", "+ Tambah Departemen", tanpa tab Posisi dan tombol Daftar/Hierarki | Terpenuhi | Playwright FE-HRD-411; screenshot 01 |
| 2. `/positions` berjudul "Posisi", penyaring "Semua Departemen", "+ Tambah Posisi", "Buka Uraian" | Terpenuhi | Playwright; screenshot 02 |
| 3. Kembali dari tambah/detail/ubah menuju halaman jenisnya | Terpenuhi di source | `backUrl` = `/hr/master-data/organization/{type}` di hook editor dan detail |
| 4. `/hr/master-data/organization/abc` → halaman tidak ditemukan | Terpenuhi | Playwright: layar "COMING SOON!" (halaman tidak ditemukan aplikasi). Status HTTP 200 karena halaman di-stream, sama dengan rute dinamis lain |
| 5. Tanpa kata "Organization" di judul halaman, judul tab peramban, pesan memuat, atau pesan kosong | Terpenuhi | Playwright (0 kemunculan); diff |
| DoD: laporan dan tanda roadmap | Terpenuhi | Laporan ini; roadmap frontend bagian 33 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | Kartu ringkasan tetap menampilkan jumlah departemen dan posisi sekaligus di kedua halaman (perilaku lama, tidak diminta diubah) |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `FE-HRD-420` (nomor lama `FE-HRD-412`) |
| Langkah berikutnya | Rilis bersama `FE-HRD-418` dan `FE-HRD-420` |
