# Laporan Live Browser Testing — Perencanaan Tenaga Kerja (Gelombang 4, `FE-HRD-R42`)

| Item | Keterangan |
| --- | --- |
| Menu yang diuji | **Sumber Daya Manusia → Perencanaan Tenaga Kerja** (slice `S-D1`, 10 rute) |
| Task roadmap | `FE-HRD-R42` — Verifikasi `S-D1` ([frontend-unfinished-work.md](../../roadmap/frontend-unfinished-work.md)) |
| Tanggal uji | Rabu, 18 November 2026, pagi hingga 11.45 WIB |
| Penguji | AI Agent (Claude Code, skill `live-browser-testing`) dengan Google Chrome (Chromium) mode visual/headed |
| Lingkungan | Frontend `http://localhost:3000` (`QuilvianSystemFrontendDev`, branch `HamzahV2`); backend `https://localhost:7184` (`NewQuilvianSystemBackend`, branch `MHamzah`, build scratch, env `Runtime__Role=Web`) |
| Database | `QuilvianNewDevHamzah` (database pengembang bersama) |
| Akun | `superadmin@admin.com` (admin, tanpa profil pegawai) dan `dewi.anggraeni@rsmmc.local` (pegawai non-admin, bukan atasan). Kata sandi tidak dicantumkan. |
| Persetujuan pemilik | 18 Nov 2026: backend boleh dinyalakan apa adanya (worker latar ikut berjalan); uji hanya membaca; kegagalan server disimulasikan di browser. Tidak ada data yang ditulis dari menu ini. |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: seluruh layar Perencanaan Tenaga Kerja terjangkau dari menu, dan hak akses tampil benar.** Acceptance criteria `FE-HRD-R42` ("Seluruh layar terjangkau dari menu") terpenuhi.

Contoh di rumah sakit: Kepala SDM membuka menu **Permintaan Headcount**, menekan **+ Buat Permintaan**, lalu sampai di formulir permintaan baru. Perawat pelaksana yang tidak berhak tidak melihat menu-menu tersebut. Bila ia mengetik alamatnya langsung, layar menolak dengan pesan "belum punya hak".

| Ringkasan | Jumlah |
| --- | ---: |
| Test case | 22 |
| ✅ Lulus | 21 |
| ❌ Gagal | 0 |
| ⚠️ Lulus dengan catatan | 1 (LBT-W4-D1-01, P3) |
| Temuan unik | 1 (P3) |

**Yang sudah berjalan baik:**
- Sembilan butir menu `S-D1` terlihat oleh admin dan membuka layar yang benar.
- Layar ke-10, formulir permintaan baru (`/headcount-requests/baru`), sengaja tidak ada di menu dan dicapai lewat tombol **+ Buat Permintaan** di daftar permintaan.
- Untuk pegawai non-admin, kesembilan menu tersembunyi karena `requiredPermission`. Sembilan dari sepuluh layar menampilkan pesan "belum punya hak" saat dibuka lewat alamat langsung.
- Tidak ada HTTP 500, crash JavaScript, layar kosong, atau pesan Axios mentah di kesepuluh rute.

---

## 2. Alur Proses Bisnis yang Diuji

1. **Pemicu** — petugas SDM membuka sidebar **Sumber Daya Manusia → Perencanaan Tenaga Kerja**.
2. **Navigasi menu** — agent mencari setiap label menu lewat kotak **Cari menu...**. Sidebar memakai daftar virtual, sehingga submenu yang tertutup tidak dirender. Setelah itu agent memastikan tautan rute muncul.
3. **Buka layar** — setiap rute dibuka, lalu agent mencatat status HTTP, galat JavaScript, layar kosong, dan pesan galat yang tampil.
4. **Tombol masuk ke layar turunan** — dari **Permintaan Headcount**, tombol **+ Buat Permintaan** membuka `/hr/workforce-planning/headcount-requests/baru`.
5. **Peran tidak berhak** — langkah 2–3 diulang dengan akun pegawai non-admin. Hasil yang diharapkan: menu tersembunyi, dan layar yang dibuka lewat alamat langsung menolak dengan sopan.

## 3. Persiapan Lingkungan & Data

| Langkah | Keterangan |
| --- | --- |
| Backend | `dotnet build -o <scratch>` lalu dijalankan dari folder scratch agar DLL repo tidak terkunci; env `ASPNETCORE_ENVIRONMENT=Development`, `Runtime__Role=Web` |
| Frontend | Build produksi standalone di port 3000 untuk sapu awal, lalu `next dev` di port 3000 untuk verifikasi perbaikan (CORS backend hanya mengizinkan `:3000`) |
| Peramban | Chrome headed, CDP port 9337, profil `test-with-agy/chrome-profile-hrw4-1118`; login perlu izin geolokasi |
| Data uji | Tidak dibuat. Uji `S-D1` bersifat baca saja |
| Catatan worker | `Runtime__Role=Web` ternyata tidak mematikan ±50 hosted service di `Program.cs`. Pemilik menyetujui backend tetap dinyalakan |

## 4. Endpoint yang Diuji

Semua endpoint dipanggil oleh layar saat dimuat (metode `GET`). Auth: cookie sesi JWT + `[AccessPermission]` per aksi.

### `[Tags("Corporate / Human Resource / Workforce Planning / Setting")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/corporate/human-resource/workforce-planning/settings` | Parameter perencanaan per rumah sakit | Admin `200`; non-admin `403` (pilihan rumah sakit) |

### `[Tags("Corporate / Human Resource / Workforce Planning / Position Formation")]`, `[Tags(".../Initial Formation Batch")]`, `[Tags(".../Staffing Workload")]`, `[Tags(".../Staffing Standard")]`, `[Tags(".../Staffing Gap")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/corporate/human-resource/workforce-planning/position-formations` | Daftar formasi jabatan | Admin `200`; non-admin ditolak di layar |
| GET | `/api/v1/corporate/human-resource/workforce-planning/initial-formation-batches` | Batch formasi awal | Admin `200` |
| GET | `/api/v1/corporate/human-resource/workforce-planning/staffing-workloads` | Angka beban kerja | Admin `200` |
| GET | `/api/v1/corporate/human-resource/workforce-planning/staffing-standards` | Standar ketenagaan | Admin `200` |
| GET | `/api/v1/corporate/human-resource/workforce-planning/staffing-gaps` | Potret gap tenaga | Admin `200` |

### `[Tags("Corporate / Human Resource / Workforce Planning / Headcount Request")]`, `[Tags(".../Recruitment Queue")]`, `[Tags(".../Manpower Plan")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/corporate/human-resource/workforce-planning/headcount-requests` | Daftar permintaan headcount | Admin `200` |
| GET | `/api/v1/corporate/human-resource/workforce-planning/recruitment-queue` | Antrean rekrutmen | Admin `200` |
| GET | `/api/v1/corporate/human-resource/workforce-planning/manpower-plans` | Rencana tenaga tahunan | Admin `200` |

## 5. Rincian Test Case

### P0 — Critical / Fatal Functional

| ID | Skenario | Hasil |
| --- | --- | --- |
| D1-P0-01 s.d. -10 | Sepuluh rute `S-D1` dibuka admin: tidak blank, tidak crash, tidak ada HTTP 500 | ✅ 10/10 |

### P1 — Business Compliance (hak akses)

| ID | Skenario | Hasil |
| --- | --- | --- |
| D1-P1-01 | Admin melihat 9 butir menu `S-D1` lewat pencarian menu | ✅ 9/9 |
| D1-P1-02 | Non-admin tidak melihat 9 butir menu `S-D1` | ✅ 9/9 tersembunyi |
| D1-P1-03 s.d. -11 | Non-admin membuka 9 layar data lewat alamat langsung → pesan "belum punya hak" | ✅ 9/9 |
| D1-P1-12 | Non-admin membuka **Pengaturan Perencanaan** lewat alamat langsung | ⚠️ Tanpa pesan "belum punya hak" (LBT-W4-D1-01); tombol simpan tidak tampil |

### P2 & P3 — Keterjangkauan

| ID | Skenario | Hasil |
| --- | --- | --- |
| D1-P2-01 | `/headcount-requests/baru` dicapai dari tombol **+ Buat Permintaan** (bukan dari menu) | ✅ (`headcount-requests-view.jsx:108`) |

### P4 — Visual & P5 — UI/UX

Setiap layar memakai Hero, kartu ringkasan, dan tabel base component. Tidak ada layar yang tampil polos. Lihat screenshot pada lampiran.

## 6. Daftar Temuan & Rekomendasi

### P3 — Fungsional & validasi

| ID | Temuan | Contoh dampak | Rekomendasi |
| --- | --- | --- | --- |
| LBT-W4-D1-01 | **Pengaturan Perencanaan** yang dibuka lewat alamat langsung oleh pegawai tanpa hak tetap menampilkan formulir. Pilihan rumah sakit kosong tanpa penjelasan karena `GET hospital-sites/options` dijawab `403`. | Perawat yang menebak alamat melihat formulir kosong dan mengira datanya hilang. Tidak ada risiko perubahan data karena tombol simpan tidak tampil. | Tampilkan `InformationAlert` "Anda belum punya hak untuk melihat pengaturan perencanaan" bila hak baca tidak ada, seperti layar `S-D1` lainnya. |

## 7. Tidak Dapat Diuji / Tertahan

| Hal | Alasan |
| --- | --- |
| Simpan pengaturan, buat/ajukan permintaan headcount | Di luar izin. Pada Gelombang 4 pemilik hanya mengizinkan satu penulisan data (delegasi Plh, `FE-HRD-R45`). Alur tulis `S-D1` sudah dilaporkan pada `FE-HRD-R36`. |

## 8. Data Uji yang Tertinggal di `QuilvianNewDevHamzah`

Tidak ada. Uji menu ini hanya membaca data.

## 9. Lampiran Teknis

| Berkas | Isi |
| --- | --- |
| `QuilvianSystemFrontendDev/test-with-agy/scripts/hrw4-1118-routes.json` | Daftar rute per task/slice |
| `.../scripts/hrw4-1118-sweep.mjs` | Sapu rute per akun (hanya membaca) |
| `.../scripts/hrw4-1118-menu.mjs` | Pencarian label menu dan pencocokan tautan |
| `.../screenshots/hrw4-1118/R42-admin/` | 10 screenshot admin + `hasil.jsonl` |
| `.../screenshots/hrw4-1118/R42-nonadmin/` | 10 screenshot non-admin + `hasil.jsonl` |
| `.../screenshots/hrw4-1118/menu-admin/menu.json`, `menu-nonadmin/menu.json` | Hasil keterlihatan menu |

## 10. Tindak Lanjut

1. Frontend: tambahkan pesan tanpa hak di Pengaturan Perencanaan (LBT-W4-D1-01).
2. Uji alur tulis `S-D1` per peran setelah pemilik mengizinkan penulisan data uji.
