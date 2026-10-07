# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-015` |
| Judul | Menu dan halaman Konsultasi Tertunda |
| Slice | `MVP-0` Amendment MT — seluruh `MT-FR-01`..`09` |
| Roadmap | [roadmap/doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `16` |
| Trace | `RJ-DOC-DEC-045`..`054`, `RJ-DOC-FE-014`..`016`; `03-frontend-architecture.md` *Amendment MT* (MT-FE.1..9); `04-prd-to-mvp.md` *Amendment MT* |
| Contract version | `RJ-DOC-PENDCONS-001@1.0.0` (`approved`, tidak berubah) |
| Wewenang UI | Susunan menu, alur Simpan/Batalkan, dan pengingat: `approved` (`RJ-DOC-FE-014`, `RJ-DOC-FE-015`, `RJ-DOC-DEC-046`..`048`). Kolom, route, nama parameter URL, ikon, letak pengingat: `DEV_DISCRETION` (`RJ-DOC-FE-016`) |
| Dependency | `[BE] RJ-DOC-REV-BE-012` ✅ (endpoint `pending-consultations`) |
| Klasifikasi | `MEDIUM` — 1 route baru, 1 view baru, 3 hook baru, 1 view besar diubah, menu; tanpa backend, tanpa state Redux baru |
| Task mode | `FRONTEND` (laporan dan tanda status di repository backend sesuai wewenang laporan) |
| Target tulis | `V2QuilvianSystemFrontendDev/src/**`; laporan ini, roadmap bagian 16, `requirement-traceability.md` bagian 10 |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `9acc42027` (`sukmagpV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `85962dc4` (`sukmagp`) |
| Tanggal | 7 Oktober 2026 |
| Status | ✅ `COMPLETE` — seluruh acceptance criteria terbukti, kecuali dua butir yang dinyatakan `NOT FEASIBLE` di layar (lihat §7). Dikerjakan ulang 7 Okt 2026 untuk cakupan per akun dokter: konsultasi tertunda (§9) dan antrean hari ini (§10) |

---

## 1. Keadaan yang ditemukan di awal

- Konsultasi tertunda tampil sebagai tab **Hari ini / Tertunda** di panel kiri Klinis Dokter
  (`RJ-DOC-REV-FE-012`, commit `79d5721b3`). Panel itu sempit; pada gambar pemilik ada
  **Tertunda (8)** yang harus dibaca satu per satu di kartu kecil.
- Data daftar berasal dari hook `useDoctorPendingConsultations`. Hook itu memuat **50 baris
  sekaligus tanpa pencarian**, sekaligus menangani pembatalan.
- Backend sudah menyediakan semua yang dibutuhkan: `GET .../doctor-queues/pending-consultations`
  menerima `search`, `queueId`, `pageNumber`, `pageSize`. Service frontend belum meneruskan `search`.
- Sidebar mendukung menu bertingkat lewat `subItems` (contoh: Sumber Daya Manusia → Master Data).
- Pola rujukan: Daftar Pasien Rawat Jalan (`outpatient-encounters/*`) dengan aksi baris *Batalkan Kunjungan*.
- Temuan sampingan: `ConfirmModal` batal konsultasi di Klinis Dokter mengirim `message` **dan**
  `children`. Karena `children` menggantikan `message`, kalimat konfirmasi hilang begitu galat muncul.
  Diperbaiki pada task ini dengan pola Daftar Pasien Rawat Jalan.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** dokter Rawat Jalan (dan pengguna jalur super admin yang melihat semua dokter).

1. Dokter membuka **Klinis Dokter**. Bila ada konsultasi tertunda, panel kiri menampilkan
   *"Ada 9 konsultasi tertunda dari hari sebelumnya."* dengan tautan **Buka Konsultasi Tertunda**.
   Antrean hari ini tampil seperti biasa, tanpa tab.
2. Dokter membuka menu **Dokter → Rawat Jalan → Konsultasi Tertunda** (atau lewat tautan tadi).
   Tabel menampilkan Tanggal, Tertunda (hari), No. Kunjungan, Pasien/No. RM, Klinik, Dokter, dan
   Resep draf/Tindakan, urut dari yang paling lama. Dokter dapat mencari dengan no. kunjungan,
   no. RM, atau nama pasien.
3. **Jalur Batalkan:** dokter memilih ⋮ → **Batalkan Konsultasi**, mengisi alasan (wajib, maks.
   250 karakter), lalu menekan **Batalkan Konsultasi**. Baris hilang dan muncul pesan
   *"Konsultasi dibatalkan. Minta petugas membatalkan kunjungan ENC-RSMMC-00219 di Daftar Pasien
   Rawat Jalan."*
4. **Jalur Simpan:** dokter memilih ⋮ → **Simpan Konsultasi**. Klinis Dokter terbuka dengan
   konsultasi itu langsung terpilih. Banner kuning *"Kunjungan tanggal 28 Sep 2026 — tertunda
   9 hari…"* muncul, dan modal Simpan **tidak** terbuka sendiri. Dokter meninjau SOAP, resep, dan
   tindakan, lalu menekan **Simpan Konsultasi**. Konfirmasi kunjungan lampau dari `RJ-DOC-FE-011`
   tetap berlaku. Setelah berhasil, dokter kembali ke daftar dengan pesan *"Konsultasi kunjungan
   ENC-RSMMC-00216 berhasil disimpan."*
5. Saat meninjau, dokter juga dapat menekan **Batalkan Konsultasi** di workspace. Setelah berhasil,
   dokter kembali ke daftar dengan pesan untuk petugas.

**Jalur tidak normal:**

- **Validasi gagal saat Simpan** (mis. SOAP belum lengkap): dokter tetap di Klinis Dokter, dan
  modal menampilkan daftar kekurangan dari backend.
- **Konsultasi sudah diproses di tab lain**: Klinis Dokter menampilkan *"Konsultasi tertunda tidak
  ditemukan atau sudah diselesaikan."* dengan tautan *Kembali ke Konsultasi Tertunda*.
- **Daftar gagal dimuat**: pesan galat merah tampil di atas tabel.
- **Jumlah tertunda gagal dimuat** di Klinis Dokter: pengingat tidak tampil, dan antrean hari ini
  tetap berjalan.
- **Tanpa hak akses** (`403`): halaman diganti `AccessDeniedGate`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend, `rules/frontend/*` suite skill, `03-frontend-architecture.md` *Amendment MT*,
`outpatient-encounter-list-view.jsx` dan hook-hooknya, `doctor-queue-view.jsx`,
`useDoctorConsultationWorkspace.js`, `useDoctorPendingConsultations.js`, `QueuePatientCard.jsx`,
`left-sidebar-menu-handle.jsx`, `row-action-menu.jsx`, `confirm-modal.jsx`, `information-alert.jsx`,
`toast-stack.jsx`, `DoctorQueueController.cs` (`GetPendingConsultations`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Dokter → Rawat Jalan menjadi grup `subItems`: *Klinis Dokter* (rute lama) dan *Konsultasi Tertunda* (rute baru) |
| `src/app/health-services/registration-management/doctor-pending-consultations/page.jsx` | **Baru.** Route tipis, `metadata.title = "Konsultasi Tertunda"` |
| `src/components/view/.../doctor-pending-consultations/doctor-pending-consultation-list-client.jsx` | **Baru.** Pembungkus `Suspense` untuk `useSearchParams` |
| `src/components/view/.../doctor-pending-consultations/doctor-pending-consultation-list-view.jsx` | **Baru.** Halaman daftar berpola Daftar Pasien Rawat Jalan; modal batal; toast |
| `src/components/view/.../doctor-pending-consultations/doctor-pending-consultation-table-columns.jsx` | **Baru.** Definisi kolom dan `RowActionMenu` (Simpan / Batalkan bila `canCancelConsultation`) |
| `src/lib/constants/.../doctor-queue/doctor-pending-consultation.constants.js` | **Baru.** Route, nama parameter URL, opsi jumlah baris, batas alasan |
| `src/lib/hooks/.../doctor-queue/use-doctor-pending-consultation-list.js` | **Baru.** Daftar: pencarian, pagination, abort, `403`, toast hasil dari Klinis Dokter |
| `src/lib/hooks/.../doctor-queue/use-doctor-pending-consultation-cancel.js` | **Baru.** Batalkan konsultasi (dipakai daftar dan workspace); galat tetap di modal |
| `src/lib/hooks/.../doctor-queue/use-doctor-pending-consultation-review.js` | **Baru.** Klinis Dokter: jumlah tertunda (`pageSize=1`), memuat satu konsultasi dari parameter URL, baca ulang hitungan untuk modal Simpan |
| `src/lib/hooks/.../doctor-queue/useDoctorPendingConsultations.js` | **Dihapus.** Digantikan tiga hook di atas; tidak ada pemakai lain |
| `src/lib/services/.../doctor-queue.service.js` | `getDoctorPendingConsultations` meneruskan `search` |
| `src/utils/.../doctor-queue/doctor-pending-consultation-utils.js` | Tautan tinjau/kembali dan pesan sukses; nilai `"-"` tidak dianggap nomor kunjungan |
| `src/components/view/.../doctor-queues/doctor-queue-view.jsx` | Tab dan daftar tertunda dihapus; pengingat (`InformationAlert` + `Link`); buka item dari `?pendingQueueId=`, lalu parameter dibersihkan; pesan tidak ditemukan; kembali ke daftar sesudah Simpan/Batalkan; modal batal diperbaiki (`children`) |
| `src/components/view/.../doctor-queues/doctor-queue-client.jsx` | Dibungkus `Suspense` karena view kini memakai `useSearchParams` |
| `src/components/features/health-services/doctor-queue-features/QueuePatientCard.jsx` | Dikembalikan ke versi sebelum `FE-012` (cabang `pendingMode` tidak lagi dipakai) |
| `src/style/.../doctor-queues/doctor-queue-view.module.css` | `leftPanel_withTabs` → `leftPanel_withNotice`; class `pendingErrorState` dan `pendingDaysText` yang tidak terpakai dihapus |

### 3.3 Kepatuhan arsitektur frontend

- Alur `app → view → hook → service → InstanceAxios` dipertahankan. View tidak memanggil Axios.
- Data server satu halaman memakai hook + service (pola `outpatient-encounters`), tanpa Redux slice baru.
- Route, nama parameter, dan opsi statis ada di constants. Penyusun tautan dan pesan ada di utils murni.
- Penyesuaian state dari query dilakukan saat render (pola `useOutpatientEncounterList`), bukan
  `setState` di dalam effect.
- **Delta terhadap desain:** nama parameter URL `pendingQueueId` (desain memberi contoh `?queueId=`,
  `DEV_DISCRETION`). Nama yang lebih spesifik dipilih supaya tidak bentrok dengan parameter lain
  di Klinis Dokter.

#### Gerbang keputusan base component

```
UI GATE: 11 elemen — REUSE 9, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `base-features/hero.jsx`, dipakai Daftar Pasien RJ | REUSE | `eyebrow`, `title`, `description` |
| Pencarian + jumlah baris + reset | `DataFilter`, `FilterSelect` | dipakai Daftar Pasien RJ | REUSE | — |
| Tabel + paginasi | `DataTable`, `Pagination` | dipakai Daftar Pasien RJ | REUSE | kolom di `*-table-columns.jsx` |
| Aksi baris | `RowActionMenu` | `tone`, `icon`, aksi `false` disaring | REUSE | Simpan (`primary`), Batalkan (`danger`) |
| Konfirmasi batal beralasan | `ConfirmModal` | `requireReason`, `maxReasonLength` | REUSE | ringkasan lewat `children` |
| Pesan galat halaman/modal | `InformationAlert` | — | REUSE | — |
| Notifikasi hasil aksi | `ToastStack` | dipakai Daftar Pasien RJ | REUSE | — |
| Tanpa hak akses | `AccessDeniedGate` | dipakai Daftar Pasien RJ | REUSE | — |
| Tombol batal di workspace | `BaseButton` | sudah dipakai `FE-012` | REUSE | — |
| Pengingat jumlah di Klinis Dokter | `InformationAlert` + `next/link` | `children` didukung | COMPOSE | lihat pilihan di bawah |
| Pesan tidak ditemukan + tautan kembali | `InformationAlert` + `next/link` | `children` didukung | COMPOSE | sama |

Pilihan untuk dua baris `COMPOSE` (tanpa jawaban, opsi A dijalankan):

- **A. `InformationAlert` (`info`/`warning`) berisi teks dan `Link` — Rekomendasi, dipakai.** Gaya
  peringatan sama dengan banner kunjungan lampau, tanpa komponen baru, dan tanpa mengubah base.
- **B. Tambah prop `action` pada `InformationAlert`.** Lebih rapi untuk tombol, tetapi mengubah base
  component yang dipakai banyak modul. Biaya dan risiko regresi lebih besar.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel: "Mengambil data konsultasi tertunda...". Klinis Dokter: antrean seperti biasa; pengingat muncul setelah jumlah termuat |
| Kosong | "Tidak ada konsultasi tertunda." — "Semua konsultasi hari sebelumnya sudah disimpan atau dibatalkan." Klinis Dokter: pengingat tidak tampil |
| Gagal | Daftar: pesan server atau "Konsultasi tertunda gagal dimuat." di atas tabel; tombol reset/cari memuat ulang. Batal: pesan server di dalam modal, modal tetap terbuka. Konsultasi dari URL: "Konsultasi tertunda tidak ditemukan atau sudah diselesaikan." + tautan kembali |
| Tanpa hak akses | `AccessDeniedGate` dengan pesan server (`401`/`403`) |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Doctor Queue

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/registration-management/doctor-queues/pending-consultations?pageNumber&pageSize&search` | Tabel Konsultasi Tertunda | `DoctorQueue : Read` |
| `GET` | `/v1/health-services/registration-management/doctor-queues/pending-consultations?pageSize=1` | Jumlah untuk pengingat Klinis Dokter | `DoctorQueue : Read` |
| `GET` | `/v1/health-services/registration-management/doctor-queues/pending-consultations?queueId={id}&pageSize=1` | Memuat konsultasi yang dibuka dari daftar; baca ulang hitungan saat modal Simpan | `DoctorQueue : Read` |
| `POST` | `/v1/health-services/registration-management/doctor-queues/{id}/finish-consultation` | Simpan Konsultasi (tidak berubah) | `DoctorQueue : FinishConsultation` |

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/v1/health-services/clinical-management/doctor-consultations/{consultationId}/cancel` | Batalkan Konsultasi, body `{ "cancelReason": "..." }` | `DoctorConsultation : Cancel` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 14 berkas yang disentuh | `0 error, 0 warning` (run pertama 1 warning `react-hooks/set-state-in-effect`, diperbaiki) | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil; route `○ /health-services/registration-management/doctor-pending-consultations` terdaftar | `PASS` | `fe015_build.log` (scratchpad) |
| `npm run test:unit` | `2443 pass, 8 fail`. Ke-8 kegagalan identik pada baseline `HEAD`: empat berkas test yang membaca `menu-items.jsx` dijalankan ulang dengan `menu-items.jsx` versi `HEAD` → hasil sama (`31 pass, 6 fail`). Dua lainnya tidak membaca berkas yang disentuh | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| Grep anti-regresi UI (warna literal, typography, `<button>`, `<table>`, `fw-`/`fs-`, `!important`, inline style) | Tidak ada temuan pada berkas baru dan diff | `PASS` | Keluaran grep |
| Uji layar Playwright — fase *save* | `19/19 PASS` | `PASS` | `ui_fe015_save2.log`, `fe015-*.png` |
| Uji layar Playwright — fase *ws-cancel* | `7/7 PASS` | `PASS` | `ui_fe015_wscancel2.log` |
| Uji layar Playwright — fase *list-cancel* | `6/6 PASS` | `PASS` | `ui_fe015_listcancel.log` |

Uji manual: `PASS` — **32/32** pada run akhir.

**Lingkungan uji layar.** Server dev frontend milik pemilik di `localhost:3000` (source `sukmagpV2`
+ perubahan ini). Backend dev pemilik di `7184` masih kode lama (`pending-consultations` → `404`),
sehingga backend `HEAD 85962dc4` di-build Release ke scratchpad dan dijalankan di `https://localhost:7185`
terhadap `QuilvianNewDevSukma`. Panggilan `7184` dialihkan ke `7185` hanya di browser uji. Akun
`uji.rjdp.dokter` diaktifkan sementara lewat endpoint status dokter dan dinonaktifkan lagi sesudahnya.
Data uji pasien `KSKTEST-RM-07` dibuat lewat endpoint aplikasi; `QueueDate` dimundurkan lewat SQL
dengan izin pemilik (`RJ-DOC-DEC-052`).

| ID | UAT | Skenario | Hasil |
| --- | --- | --- | --- |
| M1 | `UAT-MT-01` | Sidebar: Dokter → Rawat Jalan → Klinis Dokter, Konsultasi Tertunda, lalu Rawat Inap | `PASS` |
| K1, K3 | `UAT-MT-01` | Panel kiri tanpa tab; antrean hari ini tetap tampil | `PASS` |
| K2, K4 | `UAT-MT-02` | "Ada 9 konsultasi tertunda dari hari sebelumnya."; tautan membuka daftar | `PASS` |
| L1–L3 | `UAT-MT-03` | Hero + tabel; baris `ENC-RSMMC-00216` "28 Sep 2026 · 9 hari · 0 resep draf · 0 tindakan"; request `pageSize=20` | `PASS` |
| L4–L5 | `UAT-MT-03` | Cari no. kunjungan → 1 baris (`search=` terkirim); kata kunci tanpa hasil → keadaan kosong | `PASS` |
| S1–S5 | `UAT-MT-05` | Simpan Konsultasi: Klinis Dokter terbuka dengan konsultasi terpilih, banner "tertunda 9 hari", modal Simpan tidak terbuka, parameter URL bersih, tombol Batalkan tampil | `PASS` |
| S6–S8 | `UAT-MT-05` | Selesaikan → kembali ke daftar, toast "Konsultasi kunjungan ENC-RSMMC-00216 berhasil disimpan.", baris hilang; DB: kunjungan status 7, konsultasi `Completed` | `PASS` |
| S9 | `UAT-MT-12` | Buka Klinis Dokter lagi → "Belum Ada Pasien Dipilih" (tidak terbuka ulang) | `PASS` |
| V1–V2 | `UAT-MT-10` | Konsultasi kosong → finalisasi ditolak ("Objective… wajib diisi", "Diagnosis utama wajib…"); tetap di Klinis Dokter | `PASS` |
| W1–W4 | `UAT-MT-06`, `UAT-MT-07` | Batalkan dari workspace: konfirmasi nonaktif tanpa alasan; sesudah berhasil kembali ke daftar dengan pesan petugas; baris hilang | `PASS` |
| N1 | `UAT-MT-09` | `?pendingQueueId=` konsultasi yang sudah batal → pesan tidak ditemukan + tautan kembali, workspace kosong | `PASS` |
| X1–X4 | `UAT-MT-04`, `UAT-MT-07` | Batalkan dari daftar: modal menyebut pasien, tanggal, dan kunjungan; konfirmasi nonaktif tanpa alasan; toast pesan petugas; `PATCH …/cancel` terkirim **sekali**; baris hilang | `PASS` |
| — | `UAT-MT-04` | Petugas membatalkan kunjungan lewat `PATCH /outpatient-encounters/{id}/cancel` | `200` |
| Y1 | — | `pending-consultations` dipaksa `500` → pesan galat di atas tabel | `PASS` |
| Y2 | — | Jumlah gagal dimuat → pengingat tidak tampil, antrean hari ini tetap | `PASS` |

Run awal: fase *save* `16/19` dan *ws-cancel* `5/7`. Seluruh kegagalannya berasal dari skrip, bukan
dari kode:

- **M1:** skrip menekan butir *Rawat Jalan* yang salah.
- **L1:** header tabel ditampilkan kapital lewat CSS.
- **S7, W3:** toast menghilang otomatis 3,5 detik, sedangkan skrip baru memeriksa sesudah 5 detik.
- **N1:** halaman belum selesai dimuat saat diperiksa. Probe terpisah membuktikan pesan tampil dalam 2 detik.

Skrip diperbaiki memakai polling, lalu dijalankan ulang dengan data baru.

| UAT | Status |
| --- | --- |
| `UAT-MT-08` tanpa izin batal | `MANUAL TEST: NOT FEASIBLE` di layar — akun uji memegang `DoctorConsultation : Cancel` lewat jabatan dan role SuperAdmin. Terbukti lewat kode: aksi baris dan tombol workspace hanya dirender bila `canCancelConsultation = true` (`canCancelPendingConsultation`), dan nilai itu dihitung server dari `HasAccessAsync(User, "DoctorConsultation", "Cancel")` |
| `UAT-MT-11` dokter lain | `MANUAL TEST: NOT FEASIBLE` di layar — tidak ada akun dokter kedua tanpa SuperAdmin. Cakupan dokter ditegakkan server dan sudah terbukti di `RJ-DOC-REV-BE-012` (`AT-KT-04`); frontend tidak menyaring sendiri |

**Tidak dijalankan:** `npm run test:e2e` (tidak diminta task); muat ulang jumlah lewat event realtime
(WebSocket SignalR uji tetap ke 7184, sama seperti `FE-012`). Badge Next.js "1 Issue" di tangkapan
layar berasal dari kegagalan WebSocket itu.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MT-01` sidebar dua butir | Terpenuhi | M1, tangkapan `fe015-after-save.png` (butir aktif menyala sendiri) |
| `AT-MT-02` Klinis Dokter tanpa tab, antrean hari ini sama | Terpenuhi | K1, K3, S9 |
| `AT-MT-03` pengingat `n > 0` / `n = 0` | Terpenuhi | K2, K4, Y2 |
| `AT-MT-04` daftar, pencarian, jumlah baris, pagination | Terpenuhi | L1–L5. Pagination memakai `DataTable` + `Pagination` dengan `totalData`/`totalPage` server, seperti Daftar Pasien RJ |
| `AT-MT-05` batal dari daftar | Terpenuhi | X1–X4 |
| `AT-MT-06` tanpa `canCancelConsultation` | Terpenuhi lewat kode; layar `NOT FEASIBLE` | §6 |
| `AT-MT-07` Simpan Konsultasi membuka tinjau | Terpenuhi | S1–S5 |
| `AT-MT-08` Selesaikan sukses → kembali | Terpenuhi | S6–S8 |
| `AT-MT-09` batal dari workspace → kembali | Terpenuhi | W1–W4 |
| `AT-MT-10` tidak ditemukan | Terpenuhi | N1 |
| `AT-MT-11` validasi gagal tetap di Klinis Dokter | Terpenuhi | V1–V2 |
| `AT-MT-12` lint dan build | Terpenuhi | §6 |
| DoD: tidak ada perubahan backend | Terpenuhi | `git status` backend hanya berkas `docs/` |
| DoD: dokter lain (`UAT-MT-11`) | Terpenuhi di backend; layar `NOT FEASIBLE` | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server dev backend pemilik (7184) masih kode lama. Restart agar daftar dan pengingat berfungsi; tanpa restart, halaman Konsultasi Tertunda menampilkan galat `404` |
| Masalah yang diketahui | 8 test unit gagal yang sudah ada sebelum task ini (hemodialisa, Bank Darah, petty cash, parity resep/tindakan). Filter Dokter ditunda (`RJ-DOC-OQ-016`) |
| Dependency backend | `NONE` — `RJ-DOC-REV-BE-012` sudah ✅ |
| Perubahan sampingan | Perbaikan `ConfirmModal` batal di Klinis Dokter (kalimat konfirmasi hilang saat ada galat), karena modal itu disentuh task ini |
| Interupsi | `NONE` |
| Data uji | `ENC-RSMMC-00215`, `00216`: konsultasi `Completed`, kunjungan status 7. `00217`, `00218`, `00219`: konsultasi dibatalkan, kunjungan dibatalkan petugas. Akun `uji.rjdp.dokter`: dinonaktifkan kembali dan bypass lokasi dimatikan (login `401`, sama seperti sebelum uji). Backend uji 7185 dihentikan |
| Status Git | Frontend: `M` QueuePatientCard.jsx, doctor-queue-client.jsx, doctor-queue-view.jsx, doctor-queue.service.js, doctor-queue-view.module.css, doctor-pending-consultation-utils.js, menu-items.jsx; `D` useDoctorPendingConsultations.js; `??` route dan view `doctor-pending-consultations/`, constants, tiga hook baru. Belum di-commit |
| Langkah berikutnya | Restart backend dev 7184; commit oleh pemilik (frontend `sukmagpV2`, dokumen blueprint di backend `sukmagp`) |

---

## 9. Revisi 1 — cakupan per akun dokter (`RJ-DOC-DEC-053`, 7 Oktober 2026)

### 9.1 Temuan pemilik

Pemilik login sebagai **dr. Maya Permata Sari**. Klinis Dokter menampilkan *"Ada 8 konsultasi
tertunda dari hari sebelumnya."*, padahal hanya 2 baris di daftar yang milik dr. Maya. Daftar
Konsultasi Tertunda juga memuat 8 baris dari 4 dokter.

**Sebab:** akun dr. Maya dikenali backend sebagai SuperAdmin (`IsCurrentUserSuperAdminAsync`).
Untuk akun seperti itu, `GET .../pending-consultations` tanpa `doctorId` mengembalikan konsultasi
tertunda **semua** dokter (`DoctorQueueController.cs:228-231`). Pengingat dan daftar sama-sama
memanggil endpoint itu tanpa `doctorId`.

### 9.2 Keputusan

`RJ-DOC-DEC-053`: akun yang tertaut ke data dokter hanya melihat konsultasi tertundanya sendiri,
di pengingat **dan** di daftar, termasuk bila akunnya SuperAdmin. Akun SuperAdmin tanpa tautan dokter
(mis. admin) tetap melihat semua. Bila jumlahnya 0, pengingat tidak tampil.

### 9.3 Perubahan

Frontend saja, memakai parameter `doctorId` yang sudah ada pada `RJ-DOC-PENDCONS-001@1.0.0`.
Tampilan tidak berubah, jadi gerbang UI di §3.3 tetap berlaku.

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/.../doctor-queue.service.js` | `getDoctorPendingConsultations` meneruskan `doctorId` |
| `src/utils/.../doctor-queue/doctor-pending-consultation-utils.js` | `getSessionDoctorId(userInfo)` — `doctorId` sesi dari `login-slice`, kosong bila akun tidak tertaut dokter |
| `src/lib/hooks/.../use-doctor-pending-consultation-list.js` | Daftar mengirim `doctorId` sesi (`selectUserInfo`) |
| `src/lib/hooks/.../use-doctor-pending-consultation-review.js` | Jumlah pengingat, konsultasi dari URL, dan baca ulang hitungan mengirim `doctorId` |
| `src/components/view/.../doctor-queues/doctor-queue-view.jsx` | Meneruskan `doctorId` sesi ke hook pengingat |

Contoh request sesudah perubahan:
`GET /v1/health-services/registration-management/doctor-queues/pending-consultations?pageNumber=1&pageSize=1&doctorId=30c2b05e-…`

`AuthWrapper` baru merender halaman sesudah `authChecked`, sehingga `userInfo.doctorId` sudah
tersedia saat request pertama dikirim. Tidak ada request awal tanpa `doctorId`; D1 dan D4 memeriksa
bahwa **setiap** request membawa `doctorId`.

### 9.4 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` 14 berkas | `0 error, 0 warning` (run pertama 6 warning `react-hooks/preserve-manual-memoization` karena `useSelector` diletakkan sebelum `useState`; dipindah) | `PASS` |
| `npm run build` | Berhasil | `PASS` |

Uji layar memakai akun `uji.rjdp.dokter` (SuperAdmin + tertaut dokter), yaitu keadaan yang sama
dengan akun dr. Maya. Backend uji 7185 dan `QuilvianNewDevSukma` dipakai seperti §6 (izin
`RJ-DOC-DEC-052`). Saat uji, ada 8 konsultasi tertunda milik dokter lain (Bagus 2, Maya 2, Arif 1,
Rendy 3).

| ID | Skenario | Hasil |
| --- | --- | --- |
| D1 (0) | Request jumlah di Klinis Dokter membawa `doctorId` dokter uji | `PASS` |
| D2 (0) | Dokter uji tanpa konsultasi tertunda: pengingat **tidak tampil**, walaupun 8 milik dokter lain ada | `PASS` |
| D3 (0) | Daftar kosong, tidak memuat 8 baris dokter lain | `PASS` |
| D4 (0) | Request daftar membawa `doctorId` | `PASS` |
| D1–D4 (1) | Satu konsultasi tertunda dibuat untuk dokter uji (`ENC-RSMMC-00220`): pengingat "Ada 1 konsultasi tertunda…", daftar 1 baris milik dr. UJI-RJDP Dokter | `PASS` |

Uji manual: `PASS` — **8/8**. Data uji `ENC-RSMMC-00220`: konsultasi dan kunjungan dibatalkan. Akun
uji dinonaktifkan kembali (login `401`), dan backend uji 7185 dihentikan.

| UAT | Status |
| --- | --- |
| Akun SuperAdmin tanpa tautan dokter tetap melihat semua | Terbukti lewat kode: `getSessionDoctorId` mengembalikan string kosong, service tidak mengirim `doctorId`, dan backend memakai cakupan super admin (§9.1). Uji layar `NOT FEASIBLE` — tidak ada kata sandi akun admin uji di sesi ini yang boleh dipakai login layar |

### 9.5 Catatan

- `03-frontend-architecture.md` MT-FE.5 masih menulis jalur super admin "Ya, semua dokter".
  Kalimat itu kini diperjelas oleh `RJ-DOC-DEC-053` untuk akun yang tertaut dokter. Penyuntingan
  dokumen desain berada di luar wewenang laporan ini.
- Antrean **hari ini** di Klinis Dokter tidak diubah pada revisi ini; cakupannya diubah pada revisi 2 (§10).

---

## 10. Revisi 2 — antrean hari ini per akun dokter (`RJ-DOC-DEC-054`, 7 Oktober 2026)

### 10.1 Temuan pemilik

Pemilik login sebagai **dr. Rendy Pangalila**. Panel *Pasien Dokter* di Klinis Dokter menampilkan
**AGNES YULIANI RAJA GUK GUK**, padahal pasien itu milik dr. Maya. *Total Antrean* juga menghitung
pasien itu.

**Sebab:** sama dengan §9.1. Akun dr. Rendy dikenali sebagai SuperAdmin, dan `GET /doctor-queues`,
`/summary`, serta `/call-lock` dipanggil tanpa `doctorId`. Filter `doctorId` sudah didukung oleh
`buildDoctorQueueParams` dan `buildDoctorQueueSummaryParams`, tetapi nilai awalnya selalu kosong.

### 10.2 Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/.../doctor-queue/use-doctor-queue.js` | `buildInitialFilters(doctorId)` mengisi `doctorId` sesi (`getSessionDoctorId(selectUserInfo)`). Dipakai untuk filter awal, muat awal, reset, cadangan muat ulang realtime, dan kunci panggil. Akibatnya daftar, ringkasan, kunci panggil, dan grup realtime (`buildDoctorRealtimeIds`) hanya untuk dokter yang login |

Akun tanpa tautan dokter mengirim `doctorId` kosong, sehingga cakupan server tetap seperti sebelumnya.

### 10.3 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint use-doctor-queue.js` | `0 error`; 1 warning `react-hooks/exhaustive-deps` di efek muat awal (baris 1752), **sudah ada di `HEAD`**. Tiga warning baru `sessionDoctorId` diperbaiki dengan menambahkannya ke dependency array (nilainya tetap selama sesi) | `PASS` |
| `npm run build` | Berhasil | `PASS` |

Uji layar memakai akun `uji.rjdp.dokter` (SuperAdmin + tertaut dokter). Saat uji, satu-satunya
antrean hari ini adalah AGNES milik dr. Maya.

| ID | Skenario | Hasil |
| --- | --- | --- |
| T1–T3 | Request `GET /doctor-queues`, `/summary`, dan `/call-lock` membawa `doctorId` dokter uji | `PASS` |
| T4 | AGNES (pasien dr. Maya) tidak tampil | `PASS` |
| T5–T6 | Tanpa antrean milik sendiri: "0 pasien status dokter hari ini", "Belum ada pasien dokter", Total Antrean 0 | `PASS` |
| T1–T6 (2) | Satu antrean hari ini dibuat untuk dokter uji (`ENC-RSMMC-00221`, Menunggu Dokter): hanya pasien itu tampil, Total Antrean 1, AGNES tetap tidak tampil | `PASS` |

Uji manual: `PASS` — **12/12**. Run pertama `0/6` karena muat awal (`useEffect` di akhir hook) masih
memanggil `buildInitialFilters()` tanpa `doctorId`; seluruh pemanggilan diperbaiki, lalu diuji ulang.
Data uji `ENC-RSMMC-00221` dibatalkan petugas. Akun uji dinonaktifkan kembali, dan backend uji 7185
dihentikan.

| UAT | Status |
| --- | --- |
| Muat ulang lewat event realtime | Tidak diuji langsung (WebSocket uji tetap ke 7184, sama seperti §6). Grup realtime kini dibentuk dari `filters.doctorId` |
| Akun SuperAdmin tanpa tautan dokter | Sama dengan §9.4 — terbukti lewat kode, layar `NOT FEASIBLE` |
