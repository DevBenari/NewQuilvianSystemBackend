# Laporan Perubahan Frontend — `FE-NV-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-002` |
| Judul | Form perangkat dan aksi login |
| Slice | `MVP-1` — pengelolaan perangkat display |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — kartu `FE-NV-002` |
| Trace | `FR-NV-001`–`004`; `NV-DEC-016`, `022`; `NV-AC-016`, `018`; `VAL-NV-01`–`08`; layar `FE-NVW-02` (`03-frontend-architecture.md` §3.2) |
| Contract version | `0.1.0` — `approved` |
| Wewenang UI | Tombol dan isi layar dikunci skema §3.2; bentuk konfirmasi (modal) `DEV_DISCRETION` |
| Dependency | `BE-NV-002` ✅; `FE-NV-001` ✅ (constants, slice, utils bersama) |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika sedang (1), kontrak yang ada (1), auth berkaitan — kata sandi perangkat (1), workflow terbatas (1) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fc5af984f`; perbaikan no. 5 di atas `298f2cecc` + working tree (belum di-commit) |
| Commit backend yang dijadikan rujukan | `4af76bcb` + working tree `BE-NV-002`; perbaikan no. 5: `8e4991cb` |
| Tanggal | 2026-10-29; perbaikan dan uji ulang no. 5: 2026-11-02 |
| Status | ✅ **SELESAI 2 November 2026** — uji layar gabungan 29 Okt 19/20 `PASS`; no. 5 diperbaiki dan uji ulang 2 Nov `PASS` (19/19 cek terukur). Verifikasi developer, bukan UAT. Riwayat: 🟡 SEBAGIAN 29 Okt karena no. 5 `FAIL` |

---

## 1. Keadaan yang ditemukan di awal

Belum ada form maupun halaman detail perangkat. Membuat login, mengatur ulang, dan menonaktifkan
hanya bisa lewat Swagger.

## 2. Proses bisnis dari sisi pengguna

1. **Tambah:** admin mengisi nama, memilih lantai dari daftar (misalnya "Lantai 3 · 20 bed"),
   lokasi, dan keterangan, lalu menekan **Simpan Perangkat Nurse View**. Sistem menerbitkan kode
   `NVW-000001` dan membuka halaman detail.
2. **Detail:** tombol yang tampil mengikuti `AvailableActions` dari backend:

   | Status | Tombol |
   | --- | --- |
   | Belum ada login | Kembali, Perbarui, **Buat Login**, **Nonaktifkan** |
   | Aktif | Kembali, Perbarui, **Atur Ulang Login**, **Nonaktifkan** |
   | Nonaktif | Kembali, Perbarui, **Atur Ulang Login** (= aktifkan kembali) |

3. **Buat/Atur Ulang Login:** konfirmasi → modal "Catat Login Perangkat" berisi nama pengguna
   dan kata sandi, tombol **Salin**, dan tombol **Sudah Saya Catat**. Setelah ditutup, kata sandi
   tidak bisa dilihat lagi.
4. **Nonaktifkan:** konfirmasi dengan alasan wajib 5–500 karakter. Contoh: "TV hilang, laporan
   satpam no. 45".
5. **Perbarui:** form yang sama; mengirim `version` dari detail. Bila admin lain sudah mengubah
   lebih dulu, muncul toast "Data perangkat sudah diubah orang lain…".

**Delta terhadap standar master data (disengaja):** `AGENTS.md` membatasi aksi detail master
data ke `Kembali`, `Perbarui`, `Hapus`, tetapi mengizinkan aksi untuk "workflow administratif
ketika pola tersebut sudah ada". Pembanding `queue-display-device` sudah menaruh Generate Login
dan Reset Login di halaman detail. **Hapus tidak dirender** karena kontrak melarang hapus
perangkat (state matrix §1); pengganti penghapusan adalah Nonaktifkan yang beraudit.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Sama dengan `FE-NV-001` §3.1.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/administrator/master-data/nurse-view-device/use-master-data-nurse-view-device-detail.jsx` | **Baru.** Detail, token route, aksi berdasarkan `AvailableActions`, buat/atur ulang login, salin kredensial, nonaktifkan beralasan (validasi 5–500 sebelum dikirim) |
| `src/lib/hooks/administrator/master-data/nurse-view-device/use-master-data-nurse-view-device-editor.jsx` | **Baru.** Create/update; opsi lantai dari `GET /floors`; `version` disertakan saat update |
| `src/components/view/administrator/master-data/nurse-view-device/detail/nurse-view-device-detail-view.jsx` | **Baru.** `BaseDetailView` + tiga `ConfirmModal` |
| `src/components/view/administrator/master-data/nurse-view-device/add/nurse-view-device-form-view.jsx` | **Baru.** `BaseEditorView` |
| `src/app/administrator/master-data/nurse-view-devices/create/page.jsx`, `[slug]/page.jsx`, `[slug]/update/page.jsx` | **Baru.** Route tipis; token kosong/cadangan/> 180 karakter → `notFound()` |
| `src/lib/hooks/administrator/master-data/nurse-view-device/use-master-data-nurse-view-device-editor.jsx` | **Diubah 2 Nov (no. 5).** Sesudah simpan, toast tampil di form dan pindah halaman ditunda 1,5 detik (`SUCCESS_REDIRECT_DELAY_MS`, `redirectTimerRef`). Selama jeda, `actionLoading` tetap `true` sehingga isian dan tombol terkunci. Penanda `isMountedRef` dan pembersihan timer mencegah pengalihan sesudah pengguna pindah menu. Berlaku untuk tambah dan perbarui |
| `src/utils/administrator/master-data/nurse-view-device/nurse-view-device-utils.jsx` | **Diubah 2 Nov (no. 5).** `createToast` mengisi `variant: type`, karena `ToastStack` membaca `toast.variant`. Sebelumnya semua toast modul ini jatuh ke gaya `info` dan tidak mungkin hijau atau merah |

### 3.3 Kepatuhan arsitektur dan privasi

| Aturan | Keadaan |
| --- | --- |
| Kata sandi tidak masuk Redux, storage, maupun log | Disimpan hanya di state lokal hook detail selama modal terbuka; thunk login tidak menulis `lastActionResponse`; tidak ada `console` |
| Kode bisnis tidak dikirim saat create | `deviceCode` tidak ada di field form maupun payload |
| Kirim ganda | Tombol aksi dan `ConfirmModal` terkunci selama `actionLoading`. Sejak 2 Nov, form juga terkunci selama jeda 1,5 detik sesudah simpan, dan `handleSubmit` menolak kiriman selama jeda itu |
| Pindah halaman sesudah simpan | Keputusan Rizki 2 Nov: pakai ulang pola redirect tertunda `queue-display-device` dan `kiosk-device` (24 hook editor memakainya). **Delta terhadap standar master data:** rujukan `job-level` dan standar itu memakai "toast hijau lalu `router.push`", yang justru membuat toast hilang. Penjaga kirim ganda selama jeda adalah tambahan; modul rujukan tidak memilikinya |

**Tabel keputusan base component (UI GATE):**

| Elemen | Base component | Status |
| --- | --- | --- |
| Halaman detail | `BaseDetailView` (`renderDetailActions`, `children`) | `REUSE` |
| Tombol aksi | `BaseButton` | `REUSE` |
| Konfirmasi buat/atur ulang login | `ConfirmModal` varian `warning` | `REUSE` |
| Modal kredensial sekali tampil | `ConfirmModal` varian `success` + `children` + `hideCancel` | `REUSE` (lewat props yang sudah ada) |
| Konfirmasi nonaktif beralasan | `ConfirmModal` `requireReason` | `REUSE` |
| Form | `BaseEditorView` + `optionMap.floors` | `REUSE` |
| Toast | `ToastStack` milik `BaseDetailView`/`BaseEditorView` | `REUSE` |

`UI GATE: PASS — seluruh elemen REUSE.` Perbaikan 2 Nov tidak menambah elemen UI; hanya hook dan utility yang berubah.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat detail | "Mengambil detail perangkat Nurse View..." |
| Tidak ditemukan | "Data perangkat Nurse View tidak ditemukan." |
| Tautan rusak/kedaluwarsa | "Tautan detail perangkat tidak valid. Silakan buka ulang dari daftar data." |
| Gagal aksi | Toast merah berisi pesan backend apa adanya (`409`, `422`, `400`) |
| Form belum lengkap | Pesan per field + toast merah "Form Belum Lengkap" |
| Simpan berhasil | Toast hijau "Data Berhasil Disimpan", tombol "Menyimpan..." terkunci, lalu sekitar 1,5 detik kemudian pindah ke detail |

Koreksi 2 Nov: sebelum perbaikan, seluruh toast modul ini bergaya `info`. Kata "merah" pada laporan
29 Okt ditulis tanpa mengukur warnanya.

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Display Device

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floors` | Pilihan lantai di form | `NurseViewDisplayDevice : Read` |
| `GET` | `/{id}` | Detail dan isi form update | `NurseViewDisplayDevice : Read` |
| `POST` | `/` | Simpan perangkat baru | `NurseViewDisplayDevice : Create` |
| `PUT` | `/{id}` | Perbarui perangkat | `NurseViewDisplayDevice : Update` |
| `POST` | `/{id}/login` | Buat Login | `NurseViewDisplayDevice : Update` |
| `POST` | `/{id}/login/reset` | Atur Ulang Login | `NurseViewDisplayDevice : Update` |
| `PATCH` | `/{id}/deactivate` | Nonaktifkan | `NurseViewDisplayDevice : Update` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` (lihat `FE-NV-001` §6) | 0 error, 2 warning | `EXISTING WARNING` | Pola sama dengan rujukan `job-level` |
| Grep anti-regresi UI | Semua 0 | `PASS` | `FE-NV-001` §6 |
| `npm run build` | Lulus | `PASS (pernyataan owner)` | Rizki menyatakan sudah build; `.next/BUILD_ID` `7H3ZrbK00TFmEZdHVvLuE` 2026-10-29 10:26, lebih baru dari source Nurse View terakhir (09:57). Layar uji dilayani `next dev` (lencana N), sah selama pengembangan |
| Uji layar gabungan BE-NV-002 + FE-NV-001/002 | 19 `PASS`, 1 `FAIL` (no. 5) | `FAIL` | Hasil per skenario di bawah |
| `npx eslint` atas dua berkas perbaikan (2 Nov) | 0 error, 1 warning `react-hooks/set-state-in-effect` pada `setResolvedId("")` | `EXISTING WARNING` | Baris lama, tidak disentuh perbaikan ini |
| Grep anti-regresi UI atas dua berkas perbaikan (2 Nov) | Semua 0 | `PASS` | Tidak ada JSX atau style yang berubah |
| Uji ulang no. 5 dari layar (2 Nov) | 19/19 cek terukur lulus | `PASS` | Bagian "Uji ulang no. 5" di bawah |
| `npm run build` sesudah perbaikan 2 Nov | Tidak dijalankan | `NOT RUN` | Dev server aktif di port 3000, dan DoD kartu hanya meminta lint + uji layar. Build terakhir tetap yang 29 Okt |

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan proyek.`
`MANUAL TEST: DONE — Playwright (Edge headless, ignoreHTTPSErrors) 2026-10-29, dua putaran; 19/20 PASS, no. 5 FAIL. Uji ulang no. 5 2026-11-02: PASS.`

### Uji ulang no. 5 — hasil 2026-11-02

Aktor `QUILVIAN_SUPERADMIN_*` (sandi hanya dari variabel lingkungan). Menu dibuka lewat klik sidebar.
Verdict dihitung skrip dari DOM dan respons, tidak ditulis tangan; `pageErrors` 0 dan seluruh `GET`
modul ini `200`. Bukti: `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-002-no5-20261102/run-2026-11-02T01-54-16-959Z/`
(di-ignore git). JSON bukti sudah dipindai: nol kata sandi dan nol email.

| Yang diukur | Hasil terukur | Bukti |
| --- | --- | --- |
| Simpan | `POST` `201`; payload `{deviceName:"TV NS Lantai 3", floorCode:"3", locationName:null, description:null}` tanpa `deviceCode` | `S05.json` |
| Toast | Judul "Data Berhasil Disimpan", isi "Perangkat Nurse View berhasil didaftarkan.", kelas `region-toast-success` | `S05.json`, `S05a-toast-di-form.png` |
| Warna | Latar kepala toast `rgba(22, 163, 74, 0.12)`, ikon `rgb(22, 163, 74)`: hijau | `S05.json` |
| Urutan | Toast tampil saat masih di `/create`; pindah ke detail 1.826 ms sesudah respons `POST` | `S05.json` |
| Kirim ganda | Selama jeda tombol `disabled`, `aria-busy="true"`, teks "Menyimpan..."; `requestSubmit()` sungguh memicu event submit, tetapi `POST` tetap satu kali | `S05.json` |
| Detail | URL tanpa UUID; `GET` detail `200`; kode `NVW-000003` sama dengan respons `POST`; "Status: Belum ada login" | `S05.json`, `S05b-detail.png` |

Dua pemeriksaan tambahan atas perubahan yang sama. Keduanya bukan nomor skenario:

| Pemeriksaan | Hasil terukur | Bukti |
| --- | --- | --- |
| Perbarui tanpa mengubah isian, hanya pada perangkat buatan uji ini | `PASS`: `PUT` `200`; toast hijau tampil di `/update`; tombol terkunci; kembali ke detail | `T1-perbarui.json`, `T1-perbarui-toast-di-form.png` |
| Jumlah data dan toast galat | `PASS`: total daftar 2 → 3 (kedua `GET` `200`); simpan kosong memunculkan toast `region-toast-error` berlatar `rgba(239, 68, 68, 0.12)`: merah; tanpa request tulis | `T2-galat-dan-jumlah.json`, `T2-toast-galat-merah.png` |

Skenario no. 1–4 dan 6–20 tidak diulang, sesuai instruksi Rizki. Hasilnya tetap yang 29 Okt.

### Skenario uji gabungan dari layar — hasil 2026-10-29

Akun uji: SuperAdmin, atau akun yang di Akses Role sudah diberi `NurseViewDisplayDevice` Read,
Create, dan Update.

Uji dijalankan agen lewat Playwright yang sudah ada di frontend (`lib-layar.mjs` + `lib2.mjs`),
tanpa SQL. Menu dibuka lewat klik sidebar, bukan `page.goto`. Pengecualiannya: no. 19 (langkahnya
memang "buka URL daftar"), tab kedua no. 10 yang masuk lewat daftar lalu klik dua kali baris (token
route ada di `sessionStorage` per tab), dan uji `403` akun perangkat di no. 16. Bukti ada di
`QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-mvp1-20261029/` (di-ignore git).

Aktor:
- no. 1–18: `QUILVIAN_SUPERADMIN_*`;
- no. 19: `QUILVIAN_LOKET_*`. `QUILVIAN_PERAWAT_*` tidak dipakai karena memegang tiga entri
  `NurseViewDisplayDevice`;
- no. 13, 15, 16: akun perangkat `nvw-000001`, di konteks peramban terpisah (incognito).

Kata sandi hanya dipegang di memori skrip:
- PNG modal kredensial dan kolom kata sandi di halaman login disamarkan dengan `mask`;
- ruas `password` di JSON ditulis `<disamarkan>`;
- skrip membatalkan penulisan bila menemukan kata sandi di bukti;
- pencarian `Nvw@<hex>` atas seluruh folder bukti: nol.

**Putaran 1 (10.50–10.53 WIB)** — `run-2026-10-29T03-51-41-193Z/`. Saat itu ke-68 kamar aktif
ber-`floorCode = null`, sehingga hanya no. 1, 2, 4, 19 yang bisa dijalankan (`GET /floors` `[]`).
Run sebelumnya, `run-2026-10-29T03-50-08-692Z`, digantikan karena dua cacat **skrip**: urutan menu
terhitung ganda `li` + `a`, dan label "Semua lantai" tidak tertangkap. Run itu nol request tulis.

**Putaran 2 (11.47–11.51 WIB, sesudah Rizki mengisi data lantai)** — `run-lanjutan-2026-10-29T04-47-18-042Z/`.
Isinya satu `.json` + `.png` per skenario, dan `ringkasan.json` dengan `pageErrors: []` dan
`getNon200` hanya dua `403` (uji akun perangkat no. 16, sesuai harapan). Percobaan sebelumnya
berhenti di pemilihan lantai pada form, karena skrip mengklik kalimat bantuan, bukan pemicu
select. Percobaan itu nol request tulis; daftar perangkat masih `totalData` 0 sebelum putaran 2.

Cek no. 17 di putaran 2 keliru menolak teks "Lantai 3 " yang berasal dari **nama** perangkat
"TV NS Lantai 3 (tab 1)". Karena itu no. 17 diukur ulang dari layar per sel kolom
(`S17-ulang-2026-10-29T04-50-08-805Z.json` + `.png`).

PNG kembar yang memang diharapkan:
- `S05b` = `S06`: keadaan layar yang sama, tanpa aksi di antaranya;
- `S13` = `S15`: halaman login dengan kolom sandi disamarkan dan tanpa pesan galat di layar.
  Penolakannya diukur dari HTTP `401` dan dari halaman yang tidak meninggalkan `/login`.

| No | Langkah | Hasil yang diharapkan | Membuktikan | Hasil 29 Okt |
| ---: | --- | --- | --- | --- |
| 1 | Buka sidebar Administrator › Master Data | Butir "Display Nurse View" tampil di bawah "Display Antrian" | Menu `FE-NV-001` | `PASS`: urutan terukur Perangkat Kiosk › Perangkat Scanner › Display Antrian › **Display Nurse View** › Cluster (P1 `S01`) |
| 2 | Klik butir itu | Halaman daftar; bila belum ada data: "Belum ada perangkat Nurse View." | `GET /` | `PASS`: route `/administrator/master-data/nurse-view-devices`; `GET /` `200`, `totalData` 0; teks kosong tampil (P1 `S02`) |
| 3 | Buka saringan Lantai | Lantai dari data kamar, contoh "Lantai 3 · 20 bed" | `GET /floors` | `PASS` (P2): `GET /floors` `200`; opsi "Semua lantai", "Lantai 2 · 17 bed", "Lantai 3 · 66 bed", "Lantai 4 · 43 bed", "Lantai 5 · 30 bed", "Lantai 8 · 13 bed", "Lantai 9 · 20 bed", sama jumlahnya dengan data API (`S03`). P1: `BLOCKED (data)` |
| 4 | **+ Tambah Perangkat Nurse View** → klik Simpan tanpa isian | Pesan "Nama Perangkat wajib diisi." dan "Lantai wajib dipilih."; tidak ada request | Validasi form | `PASS`: kedua pesan tampil di bawah isian + toast "Form Belum Lengkap"; 0 request non-GET (P1 `S04`) |
| 5 | Isi "TV NS Lantai 3", pilih Lantai 3 → Simpan | Toast hijau, pindah ke detail, kode `NVW-000001`, status "Belum ada login" | `FR-NV-001` | **`FAIL`**: `POST` `201` dengan payload `{deviceName, floorCode:"3"}` tanpa `deviceCode`; pindah ke detail; `NVW-000001`; "Status: Belum ada login". **Toast hijau "Data Berhasil Disimpan" tidak tampil** di halaman detail (`S05b.png`). Penyebab (dibaca dari source, tidak diperbaiki): `use-master-data-nurse-view-device-editor.jsx` memanggil `addToast` lalu langsung `router.push` ke detail; toast tersimpan di state editor yang ikut hilang saat halaman berganti. Rujukan `job-level` memakai pola yang sama. **Uji ulang 2 Nov: `PASS`** (`NVW-000003`, lihat bagian uji ulang di atas). Saat perbaikan ditemukan sebab kedua: `createToast` tidak mengisi `variant`, sehingga toast tidak mungkin hijau walau tampil |
| 6 | Perhatikan tombol detail | Kembali, Perbarui, Buat Login, Nonaktifkan; **tidak ada** Hapus | `AvailableActions` | `PASS`: keempat tombol masing-masing 1; Hapus 0; Atur Ulang Login 0 (`S06`) |
| 7 | Perhatikan URL | Tidak ada UUID | Token route privat | `PASS`: URL detail tanpa pola UUID dan tanpa id perangkat (`S07.json`) |
| 8 | **Buat Login** → Ya, Buat Login | Modal berisi nama pengguna `nvw-000001` dan kata sandi; Salin bekerja | `FR-NV-002` | `PASS`: `POST /login` `200` (payload `version` 0); modal "Catat Login Perangkat" berisi `nvw-000001` + kata sandi terisi; Salin → toast "Tersalin" dan papan klip memuat keduanya (dicocokkan di memori) (`S08`, sandi disamarkan) |
| 9 | Tutup modal → muat ulang halaman (F5) | Status "Aktif"; kata sandi tidak tampil di mana pun; tombol menjadi Atur Ulang Login | `NV-AC-018` | `PASS`: "Status: Aktif"; kata sandi tidak ada di teks, HTML, `localStorage`, maupun `sessionStorage`; `GET /{id}` `200` tanpa ruas password; Buat Login 0, Atur Ulang Login 1 (`S09`) |
| 10 | Buka tab peramban kedua di detail yang sama → di tab 1 ubah nama → simpan; di tab 2 ubah nama lagi → simpan | Tab 2: toast "Data perangkat sudah diubah orang lain. Muat ulang lalu coba lagi." | `VAL-NV-04` | `PASS`: tab 1 `PUT` `200`; tab 2 `PUT` `409` dengan `version` yang sama; toast pesan persis tampil di tab 2; nama yang tersimpan milik tab 1 (`S10`) |
| 11 | **Nonaktifkan** → isi alasan "TV" | Toast "Alasan Belum Valid…"; modal tetap terbuka | Validasi 5 karakter | `PASS`: toast tampil, modal tetap terbuka, 0 `PATCH` (`S11`) |
| 12 | Isi alasan "TV hilang, laporan satpam no. 45" → Ya, Nonaktifkan | Status "Nonaktif"; bagian Riwayat Penonaktifan terisi alasan, waktu, pelaku | `FR-NV-004` | `PASS`: `PATCH` `200` dengan alasan persis; tanpa muat ulang langsung "Status: Nonaktif" dan riwayat berisi alasan; sesudah muat ulang: "Terakhir Dinonaktifkan 29 Okt 2026, 11.47 · Oleh SuperAdmin · Alasan TV hilang, laporan satpam no. 45" (`S12`, `S12b`) |
| 13 | Di jendela incognito, login dengan `nvw-000001` + kata sandi langkah 8 | Ditolak (akun terkunci) | Kunci akun | `PASS`: `401` "Account is temporarily locked due to too many failed login attempts."; tetap di `/login` (`S13`) |
| 14 | Kembali ke detail → **Atur Ulang Login** | Kata sandi baru; status "Aktif" | `FR-NV-003` | `PASS`: `POST /login/reset` `200`; sandi baru terisi dan berbeda dari yang lama; nama pengguna tetap `nvw-000001`; "Status: Aktif" (`S14a`, `S14b`) |
| 15 | Incognito: login dengan kata sandi **lama** | Ditolak | Sandi lama tidak berlaku | `PASS`: `401` "Invalid email or password."; tetap di `/login` (`S15`) |
| 16 | Incognito: login dengan kata sandi **baru** | Login berhasil sebagai akun biasa tanpa menu berhak. Pengalihan ke papan baru ada di `BE-NV-003`/`FE-NV-003` | Akun aktif | `PASS`: `200` "Login successful.", masuk ke `/`; membuka halaman perangkat Nurse View → `GET` `403` + "Ups! Akses Ditolak" (`S16`). Sidebar tetap menampilkan grup menu (tidak disaring per izin, lihat pengamatan) |
| 17 | Perbarui perangkat: ganti lantai | Kolom Lantai di daftar ikut berubah | `PUT` | `PASS`: `PUT` `200` `floorCode` "4"; ukur ulang per sel: kolom LANTAI baris `NVW-000001` = "Lantai 4", data daftar `floorCode` "4" (`S17`, `S17-ulang-*`) |
| 18 | Kembali ke daftar → saring Status "Nonaktif", lalu "Aktif" | Hasil sesuai; halaman kembali ke 1 | Saringan | `PASS`: Nonaktif → `GET` `200` `status=3&pageNumber=1`, 0 baris + teks kosong; Aktif → `GET` `200` `status=2&pageNumber=1`, baris `NVW-000001` berstatus Aktif (`S18a`, `S18b`). Saat uji tidak ada perangkat Nonaktif, jadi saringan Nonaktif terbukti lewat hasil kosong dengan GET `200` |
| 19 | Login dengan akun **tanpa** hak `NurseViewDisplayDevice`, buka URL daftar | Layar akses ditolak | `403` | `PASS`: LOKET → `GET /` dan `/floors` `403`; layar "Ups! Akses Ditolak" + "Anda tidak memiliki akses ke menu atau fitur ini."; tabel kosong tidak tampil (P1 `S19`) |
| 20 | Periksa folder `Logs` backend | Tidak ada kata sandi | `NFR-006` | `PASS`: `Logs/quilvian-backend-20261029.json` dipindai dengan kata sandi di memori. Kedua sandi uji layar dan kedua sandi uji HTTP tidak ditemukan; pola `Nvw@<hex>` nol; ruas `"password":"…"` nol; 91 baris log baru tertulis selama putaran (`S20-pindai-log-layar.json`, `BE-NV-002` §5 no. 12) |

Lewat Swagger saja (tidak bisa dari layar): `POST /` dengan `floorCode` `"99"` → `422`, dan
Buat Login kedua kalinya → `409`. Di layar keduanya tercegah karena lantai berupa pilihan dan
tombol mengikuti status. **Hasil 29 Okt:** keduanya `PASS` (`BE-NV-002` §5 no. 3 dan 5).

**Pengamatan (bukan kegagalan skenario):**
- Akun tanpa hak (LOKET, dan akun perangkat) tetap melihat butir "Display Nurse View" di sidebar,
  sama seperti butir lain dalam grup Master Data Administrator. Sidebar memang tidak disaring per
  izin; perlindungannya ada di `403` dan layar akses ditolak.
- Login yang ditolak (`401`) tidak menampilkan pesan galat apa pun di halaman login (`S13`, `S15`).
  Halaman login milik bersama, di luar lingkup modul ini.
- Login perangkat yang dinonaktifkan dijawab "terkunci karena terlalu banyak percobaan gagal".
  Pesannya menyesatkan; lihat `BE-NV-002` §7.
- Tepat sesudah simpan, kartu audit detail menampilkan "Tanggal Dibuat -" / "Dibuat Oleh -"
  (`S05b`). Sesudah detail dimuat ulang, tanggal dibuat terisi (`S08`).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-016` perangkat dinonaktifkan → ditolak | Terpenuhi untuk lingkup task ini | Kunci akun teruji di no. 13 (`401`). Penolakan runtime papan adalah lingkup `BE-NV-003` dan **belum dikerjakan** |
| `NV-AC-018` kata sandi sekali tampil | Terpenuhi | No. 8–9: sandi hanya di modal; tidak ada di halaman, storage, respons detail, maupun log |
| `VAL-NV-01`–`08` tampil di bawah isian / toast | Terpenuhi untuk yang diuji | No. 4 (wajib isi), 10 (`VAL-NV-04`), 11 (alasan) |
| `UAT-01`–`03` | Sisi layar perangkat terverifikasi developer; UAT **belum dijalankan** | No. 5, 8–16. Tampilnya papan pada `UAT-01` butuh `FE-NV-003` |
| DoD — lint | Terpenuhi | §6 |
| DoD — rekaman alur buat → login → reset → nonaktif | Terpenuhi | No. 5–16 terekam (JSON + PNG) 29 Okt. No. 5 yang `FAIL` saat itu `PASS` pada uji ulang 2 Nov |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kredensial di modal ditampilkan dengan `<dl>` polos tanpa CSS baru; tampilannya mengikuti gaya bawaan modal |
| Masalah yang diketahui | **No. 5 ditutup 2 Nov.** Temuan di luar lingkup, tidak diperbaiki: (1) form login bersama (`base-login-view.jsx`) tanpa `method="post"`, sehingga klik sebelum hydration mengirim email dan sandi lewat query URL; (2) 90 dari 109 `createToast`, termasuk `job-level`, tidak mengisi `variant`, jadi berpotensi salah warna (belum diaudit per modul); (3) standar master data di suite skill masih meresepkan "toast hijau lalu `router.push`"; (4) skrip lama `test-with-agy/igd` memuat kata sandi literal (folder di-ignore git) |
| Dependency backend | `BE-NV-002` ✅ (12/12 uji HTTP) |
| Data uji di DB dev | `NVW-000001` "TV NS Lantai 3 (tab 1)", Lantai 4, Aktif (dari uji layar); `NVW-000002` dari uji HTTP; `NVW-000003` "TV NS Lantai 3", Lantai 3, Belum ada login (uji ulang 2 Nov, sudah satu kali `PUT` tanpa perubahan isian) |
| Perubahan sampingan | `NONE` |
| Interupsi | Run pertama 2 Nov (08.51 WIB) gagal di login: skrip mengklik sebelum hydration, sehingga form terkirim native dan email + sandi SuperAdmin masuk query `GET /login`. Nilainya sampai ke log dev server, keluaran alat sesi agen, dan satu `ringkasan.json`. Folder run itu dihapus; run itu nol request tulis. Skrip kini menunggu hydration, memblokir URL berisi `password`, dan membersihkan semua bentuk encoding. Fakta ini diteruskan ke lead; tindakannya milik lead |
| Status Git | Frontend: ` M .../nurse-view-device/use-master-data-nurse-view-device-editor.jsx`, ` M .../nurse-view-device/nurse-view-device-utils.jsx`. Backend: hanya berkas `docs/module-blueprints/nurse-view/` ini. Belum di-commit |
| Langkah berikutnya | `BE-NV-004` + `FE-NV-004` (keputusan Rizki 2 Nov) |
