# Laporan Perubahan Frontend — `FE-NV-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-004` |
| Judul | Layar interval TTV dan menu |
| Slice | `MVP-3` — interval ukur ulang TTV |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — kartu `FE-NV-004` |
| Trace | `FR-NV-020`–`022`; `NV-DEC-010`; `NV-AC-004`; `VAL-NV-09`–`13`; layar `FE-NVW-03` (`03-frontend-architecture.md` §3.3) |
| Contract version | `0.1.0` — `approved` |
| Wewenang UI | Isi layar dikunci skema §3.3; nama butir menu dan urutannya `DEV_DISCRETION` |
| Dependency | `BE-NV-004` 🟡 — source ada, belum di-build |
| Klasifikasi | `MEDIUM` — berkas diubah lebih dari 4, logika sedang, kontrak yang ada, keadaan baca saja berkaitan dengan izin |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `298f2cecc` + working tree (belum di-commit) |
| Commit backend yang dijadikan rujukan | `8e4991cb` + working tree `BE-NV-004` |
| Tanggal | 2026-11-02 |
| Status | 🟡 **SEBAGIAN** — source lengkap, `npx eslint` 0 error, smoke layar dengan API simulasi 15/15. **Belum:** uji gabungan dengan backend sungguhan, karena `BE-NV-004` belum di-build |

---

## 1. Keadaan yang ditemukan di awal

Belum ada layar maupun butir menu untuk interval. Tabelnya kosong dengan sengaja (`G-01`), dan
endpoint-nya baru ditulis pada `BE-NV-004`.

## 2. Proses bisnis dari sisi pengguna

1. Admin membuka Administrator › Master Data › **Interval TTV Nurse View**.
2. Layar menampilkan status. Saat kosong: "Belum dikonfigurasi — tanda lewat jadwal tidak tampil di TV."
3. Admin mengisi menit untuk tingkat yang disahkan (Rendah, Sedang, Tinggi, Kritis), nomor SK atau
   notulen, dan tanggal pengesahan, lalu menekan **Simpan Interval**. Tidak ada angka bawaan.
4. Sesudah tersimpan, muncul toast hijau, status berubah menjadi "Dikonfigurasi — SK-KOMKEP/012/X/2026,
   disahkan 20 Okt 2026.", dan di bawah form tampil "Terakhir diubah: …".

**Jalur tidak normal:**

| Kejadian | Hasil |
| --- | --- |
| Mengetik Kritis = 0 | Angka tetap 0 dan ditolak: "Interval harus antara 1 dan 1440 menit (24 jam)." |
| Rujukan atau tanggal kosong | Pesan di bawah isian + toast merah; tidak ada request |
| Semua interval kosong, belum dikonfigurasi | Toast "Belum Ada Interval"; tidak ada request |
| Semua interval kosong, sudah dikonfigurasi | Modal "Cabut Seluruh Interval?". Batal tidak mengirim apa pun; Ya mencabut, dan rujukan tetap wajib |
| Menyimpan tanpa perubahan | Toast "Tidak Ada Perubahan"; tidak ada request, supaya riwayat tidak bertambah baris kembar |
| Tanggal sesudah hari ini | Tidak bisa dipilih di kalender; backend juga menolak |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, `CLAUDE.md`; `frontend-architecture.md`, `status-task-roadmap.md`,
`grafik-dependency-roadmap.md`, `ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`; pola
`master-data-inpatient-setting-view.jsx`, `use-master-data-inpatient-setting.jsx`,
`master-data-inpatient-setting-slice.jsx`; `base-editor-view.jsx`, `base-editor-form.jsx`,
`base-editor-field.jsx`, `base-form-control.jsx`, `filter-date-picker.jsx`, `information-alert.jsx`,
`access-denied-gate.jsx`, `use-permission.jsx`, `permission-slice.jsx`; source `BE-NV-004`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/administrator/master-data/nurse-view-vital-sign-intervals/page.jsx` | **Baru.** Route tipis |
| `src/components/view/administrator/master-data/nurse-view-vital-sign-interval/master-data-nurse-view-vital-sign-interval-view.jsx` | **Baru.** `BaseEditorView` + status + konfirmasi cabut |
| `src/lib/hooks/administrator/master-data/nurse-view-vital-sign-interval/use-master-data-nurse-view-vital-sign-interval.jsx` | **Baru.** Muat, simpan, cabut, baca saja, coba lagi |
| `src/lib/state/slice/administrator/master-data/master-data-nurse-view-vital-sign-interval-slice.jsx` | **Baru.** Thunk `GET` dan `PUT` |
| `src/lib/constants/administrator/master-data/nurse-view-vital-sign-interval/nurse-view-vital-sign-interval-constants.jsx` | **Baru.** Tingkat EWS, field, batas, pesan matriks |
| `src/utils/administrator/master-data/nurse-view-vital-sign-interval/nurse-view-vital-sign-interval-utils.jsx` | **Baru.** Pemetaan, validasi, payload, teks status. `createToast` dan `getErrorMessage` memakai ulang `nurse-view-device-utils` |
| `src/lib/state/store.jsx` | +2 baris: registrasi reducer |
| `src/utils/menu-sidebar/menu-items.jsx` | +5 baris: butir "Interval TTV Nurse View" sesudah "Display Nurse View" |

### 3.3 Kepatuhan arsitektur frontend

Alur `page → view → hook → slice → InstanceAxios` dipatuhi; tidak ada CSS baru, dan `globals.css`
tidak disentuh. Bentuk mengikuti layar satu-pengaturan `inpatient-setting`.

**Isian menit sengaja tanpa `min` dan `max`.** `normalizeNumberInputValue` di
`base-form-control.jsx` meng-clamp nilai bila kedua props itu ada: mengetik `0` diam-diam menjadi `1`
menit. Untuk angka klinis, perilaku itu bertentangan dengan `UAT-08`, yang mensyaratkan `0` ditolak.

**Delta terhadap skema §3.3:**
- Tingkat EWS tampil sebagai empat isian bertumpuk, bukan tabel dua kolom, karena memakai
  `BaseEditorView`. Isinya sama.
- Modal konfirmasi cabut tidak digambar di skema. Ditambahkan karena itu satu-satunya aksi yang
  mematikan tanda keselamatan di TV.

**Tabel keputusan base component (UI GATE):**

| Elemen | Base component | Status |
| --- | --- | --- |
| Halaman dan form | `BaseEditorView`, `BaseEditorForm` | `REUSE` |
| Isian menit dan rujukan | `BaseTextField` lewat `field.type` | `REUSE` |
| Tanggal + batas hari ini | `BaseDateField`, props `max` yang sudah ada | `REUSE` |
| Status, baca saja, gagal muat, jejak ubah | `InformationAlert` | `REUSE` |
| Akses ditolak | `AccessDeniedGate` | `REUSE` |
| Konfirmasi cabut | `ConfirmModal` varian `warning` | `REUSE` |
| Tombol Coba lagi | `BaseButton` | `REUSE` |
| Toast | `ToastStack` milik `BaseEditorView` | `REUSE` |

`UI GATE: PASS — seluruh elemen REUSE.`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Isian terkunci |
| Kosong | "Belum dikonfigurasi — tanda lewat jadwal tidak tampil di TV." |
| Gagal | Pesan galat + tombol **Coba lagi**; tidak menampilkan "Belum dikonfigurasi" dan tidak ada tombol simpan |
| Tanpa hak akses | Layar "Ups! Akses Ditolak" |
| Hanya hak baca | "Mode baca saja: akun ini tidak memiliki hak mengubah interval."; isian terkunci, tombol simpan tidak ada |

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Vital Sign Interval

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/nurse-view-management/vital-sign-intervals` | Status dan isi form | `NurseViewVitalSignInterval : Read` |
| `PUT` | `/v1/health-services/nurse-view-management/vital-sign-intervals` | Simpan dan cabut | `NurseViewVitalSignInterval : Update` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 8 berkas | 0 error, 1 warning `react-hooks/set-state-in-effect` | `EXISTING WARNING` | Mengisi form dari data muatan; pola sama dengan `inpatient-setting` |
| Grep anti-regresi UI | Semua 0; juga nol `style={{` dan nol angka bawaan | `PASS` | 6 berkas baru |
| Nol komentar baris | Nol hasil | `PASS` | `grep` |
| Smoke layar dengan API **disimulasikan** di peramban | 15/15 cek lulus, `pageErrors` 0, nol tulis ke DB | `PASS` (layar saja) | `test-with-agy/nurse-view/uji-nv-004-20261102/smoke-2026-11-02T02-26-25-937Z/`. **Bukan bukti `BE-NV-004` dan bukan dasar status selesai** |
| Uji gabungan dengan backend sungguhan | Belum | `NOT RUN` | Menunggu build `BE-NV-004` |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dev server aktif di port 3000; DoD kartu meminta lint + uji layar |

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan proyek.`
`MANUAL TEST: REQUIRED — smoke simulasi selesai; uji gabungan menunggu build backend.`

### Skenario uji gabungan BE-NV-004 + FE-NV-004 — skrip siap, belum dijalankan

Skrip: `test-with-agy/nurse-view/uji-nv-004-20261102/uji-interval.mjs`. Verdict dihitung dari DOM,
respons, dan `SELECT` baca saja. Skrip berhenti tanpa menulis bila interval sudah terisi, dan uji
berakhir dalam keadaan kosong. Baris uji bertanda hapus tetap tersisa sebagai riwayat.

| No | Langkah | Hasil yang diharapkan | Membuktikan | Hasil |
| ---: | --- | --- | --- | --- |
| 1 | Klik sidebar Master Data › Interval TTV Nurse View | Butir sesudah Display Nurse View; `GET` `200`; "Belum dikonfigurasi"; empat isian kosong | Menu, `NV-AC-004`, `G-01` | `NOT RUN` |
| 2 | Simpan tanpa isian | Pesan rujukan dan tanggal + toast merah; nol `PUT` | `VAL-NV-12` | `NOT RUN` |
| 3 | Isi rujukan + tanggal saja | Toast "Belum Ada Interval"; nol `PUT`; tanggal masa depan nonaktif | `VAL-NV-13` | `NOT RUN` |
| 4 | Kritis 0, lalu 2000 | Angka tidak berubah; pesan batas; nol `PUT` | `UAT-08`, `VAL-NV-11` | `NOT RUN` |
| 5 | Rendah 720 + Sedang 240 → Simpan → F5 | `PUT` `200`; toast hijau; status + jejak ubah; nilai bertahan | `FR-NV-020` | `NOT RUN` |
| 6 | Simpan tanpa perubahan | Toast info; nol `PUT` | Riwayat tidak kembar | `NOT RUN` |
| 7 | Kosongkan Rendah, Sedang 180, Tinggi 60, rujukan baru | `GET` hanya dua tingkat baru; dua baris lama bertanda hapus di DB | Ganti-seluruh, `BE-NV-004` | `NOT RUN` |
| 8 | Kosongkan semua → Batal → Ya, Cabut | Batal nol `PUT`; Ya: `Items` kosong; status kembali kosong | `FR-NV-022` | `NOT RUN` |
| 9 | `PUT` langsung dengan isian salah (11 kasus) + batas 1 dan 1440 | `400` dengan pesan persis matriks; batas diterima | `VAL-NV-09`–`13` | `NOT RUN` |
| 10 | Akun LOKET | `GET` dan `PUT` `403`; "Ups! Akses Ditolak"; bukan "Belum dikonfigurasi" | Hak akses | `NOT RUN` |
| 11 | Baca saja (daftar izin disimulasikan) | Isian terkunci; tombol simpan tidak ada | Mode baca saja | `NOT RUN` |
| 12 | `GET` 500 (disimulasikan) → Coba lagi | Galat tampil, bukan "Belum dikonfigurasi"; pulih sesudah coba lagi | 500 bukan kosong | `NOT RUN` |
| 13 | Pindai log backend | Aksi dan rujukan pencabutan tercatat; nol kata sandi | `G-03`, `QBE-LOG-001` | `NOT RUN` |

No. 11 memakai daftar izin simulasi karena belum ada akun uji yang hanya punya hak baca interval.
Dengan begitu, konfigurasi Akses Role sungguhan untuk kepala ruang tidak ikut terbukti.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-004` tanpa interval tidak ada tanda | Status layar terpetakan; belum diuji nyata | Smoke; tanda di TV lingkup `FE-NV-005` |
| `VAL-NV-09`–`13` | Terpetakan; belum diuji nyata | `validateIntervalForm`; pesan matriks |
| `UAT-07`, `UAT-08` | `UAT-08` terpetakan dan lulus smoke; `UAT-07` butuh `BE-NV-005`/`FE-NV-005`. UAT belum dijalankan | Smoke |
| Status "Belum dikonfigurasi" saat kosong | Terpetakan; lulus smoke | `1-kosong.png` |
| DoD — lint | Terpenuhi | §6 |
| DoD — tangkapan layar kosong dan terisi | **Hanya dari simulasi** | Belum dari backend sungguhan |
| DoD — laporan ada | Terpenuhi | Berkas ini |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning lint satu, lihat §6 |
| Masalah yang diketahui | Komponen bersama `normalizeNumberInputValue` meng-clamp diam-diam bila `min`/`max` diisi. Layar lain yang memakai pola itu, misalnya `inpatient-setting`, berperilaku sama. Di luar lingkup, tidak diperbaiki |
| Dependency backend | `BE-NV-004` 🟡: menunggu build oleh Rizki |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M src/lib/state/store.jsx`, ` M src/utils/menu-sidebar/menu-items.jsx`, enam berkas baru di atas, dan dua berkas perbaikan `FE-NV-002`. Belum di-commit |
| Langkah berikutnya | Sesudah Rizki build dan menyalakan ulang backend: `node test-with-agy/nurse-view/uji-nv-004-20261102/uji-interval.mjs` |
