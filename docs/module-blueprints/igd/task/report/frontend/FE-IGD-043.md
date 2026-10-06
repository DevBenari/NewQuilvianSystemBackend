# Laporan Perubahan Frontend — `FE-IGD-043`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-043` |
| Judul | Galat aksi kepergian tampil di dalam modal konfirmasi |
| Slice | `IGD-S07` · `EPIC IGD-08` (`MVP-6`) — pasangan layar `BE-IGD-039`; menumpang tab Transfer `EPIC IGD-05` (`FE-IGD-016`, `017`) |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian R3.13.2 |
| Trace | `FR-IGD-053`, `FR-IGD-054` (sisi layar); `IGD-DEC-202`, `IGD-DEC-195`; fakta `IGD-FACT-050`; bukti lama `039-U1.png`, `039-U2.png` |
| Contract version | Validation `0.13.0` §4, §5 aturan 1, §7 aturan 1 dan 3 — bagian ini tidak berubah sejak `0.12.0` yang dirujuk kartu; `approved` (`IGD-DEC-108`, `IGD-DEC-199`, `IGD-DEC-209`). API `0.14.0` §2 tidak berubah. Nol perubahan backend |
| Wewenang UI | Rupa dan letak pesan `DEV_DISCRETION` (`IGD-DEC-202`), dengan dua syarat mengikat: tampil di dalam modal yang masih terbuka dan terbaca tanpa kursor. Nol komponen baru, nol CSS |
| Dependency | `BE-IGD-039` ✅ (`IGD-DEC-201`, 5 Oktober 2026) |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 1, berkas diubah 0 (1 berkas), logika 1, kontrak API 0, database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `FRONTEND` — izin `build-module-frontend` dari pemilik, 5 Oktober 2026 (sore). Backend baca-saja, kecuali laporan ini dan baris status pada roadmap/traceability |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`), satu berkas; laporan di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `553501053` (`RizkiV2`), working tree bersih sebelum task |
| Commit backend yang dijadikan rujukan | `8d81d361` (`rizkiG`) + working tree `EmergencyUnitAuthorityService.cs` (`BE-IGD-039`) |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ **SELESAI — 6 Oktober 2026 (sore): 9 dari 9 acceptance terbukti** (`IGD-DEC-218`; bagian 9). Acceptance 1–7 terbukti pada uji layar putaran bersama di hasil build dengan akun `PERAWAT` nyata (`043-U1`…`U5`, `044-U8`) — bukti mentah dan log backend diperiksa agent; bukti diterima dengan penyimpangan tercatat. Tanpa UAT — diserahkan ke tim UAT. *Sebelumnya:* 🟡 **SEBAGIAN — implementasi selesai 5 Oktober 2026 (sore).** **2 dari 9** acceptance terbukti: 8 (diff satu berkas, CRLF, nol komentar baru) dan 9 (`eslint` 0 error 0 warning; uji IGD lama 91/91 lulus; `npm run build` lulus 15.21 WIB, nol warning). Acceptance 1–7 **terpetakan ke source**, tetapi uji layarnya belum dijalankan — dijadwalkan pada satu putaran uji Antigravity bersama `BE-IGD-041`, `BE-IGD-064`, dan `FE-IGD-044` (`IGD-DEC-208`). Tanpa UAT |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Saat server menolak aksi kepergian, modal konfirmasi tetap terbuka tanpa keterangan apa pun | `emergency-assessment-transfer-tab.jsx` `@553501053`: `ConfirmModal` (baris 302–315) tanpa `children`; `runAction` (baris 130–140) hanya menutup modal bila berhasil dan tidak berbuat apa-apa bila gagal. `IGD-FACT-050`, bukti `039-U1.png`, `039-U2.png` |
| Pesan server sebenarnya sudah tersedia | Thunk `runDepartureAction` menolak dengan `{ message: normalizeError(...) }`; `normalizeError` (slice baris 56) membaca `response.data.message` |
| Pesan yang sama tersimpan di `transfers.saveError`, tetapi hanya terbaca pada kartu formulir *Buat Kepergian* di segmen *Formulir* — bukan di tempat aksi dijalankan | `attachCreate(builder, runDepartureAction, "transfers")` pada slice; `formContent` meneruskan `saveError={section.saveError}` |
| Pola galat-di-modal sudah dipakai layar IGD lain | Tab Observasi (`FE-IGD-042`): state lokal `actionError`, dikosongkan saat modal dibuka dan ditutup, ditampilkan lewat `InformationAlert variant="danger"` di dalam `children` `ConfirmModal` |
| Sejak 15.04 WIB frontend bergerak `57b1d360f` → `553501053` | *Pull* pemilik; nol berkas IGD, `ConfirmModal`, `InformationAlert`, atau slice IGD berubah (manifest bagian 0m.1) |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat atau dokter IGD pada Ruang Kerja Pemeriksaan IGD, tab *Transfer Pasien*, segmen *Riwayat*.

1. Petugas menekan salah satu tombol aksi pada kartu kepergian: *Ajukan Serah Terima* / *Ajukan Ulang Dokumen*,
   *Terima Dokumen*, *Tolak Dokumen*, *Catat Berangkat*, *Catat Tiba*, atau *Batalkan*.
2. Modal konfirmasi terbuka dengan judul aksi dan kalimat *"Perubahan akan dicatat sebagai kejadian baru dalam riwayat
   kepergian pasien."* Untuk *Tolak Dokumen* dan *Batalkan*, petugas mengisi alasan.
3. Petugas menekan tombol konfirmasi. Selama permintaan berjalan, tombol menampilkan putaran dan nonaktif.
4. **Berhasil:** modal tertutup dan riwayat kepergian dimuat ulang — sama seperti sebelum task ini.
5. **Ditolak server** (`400`, `403`, `409`): modal **tetap terbuka**. Di bawah kalimat konfirmasi muncul kotak merah berisi
   pesan server apa adanya. Alasan yang sudah diketik tetap ada.
6. Petugas memilih *Batal* (modal tertutup, pesan hilang), atau membereskan keadaannya lalu menekan konfirmasi lagi —
   pesan lama dihapus saat permintaan baru dikirim.
7. Saat modal dibuka lagi untuk aksi berikutnya, kotak pesan kosong.

*Contoh.* Perawat IGD `K-TIDAK-TIBA` yang tidak ditugaskan di Rawat Inap menekan *Catat Tiba*, lalu mengonfirmasi. Server
menjawab `403`. Modal *"Catat Tiba?"* tetap terbuka dan menampilkan *"Anda tidak bertugas di unit Rawat Inap, sehingga tidak
dapat mencatat kedatangan pasien."* Kepergian tetap *Berangkat*. Perawat menekan *Batal*, lalu meminta perawat Rawat Inap
mencatat kedatangannya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, `base-component-decision-gate.md`, `test-policy.md`,
  `ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`
- Kartu `FE-IGD-043` (roadmap frontend R3.13.2)
- `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-transfer-tab.jsx`
- `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-observation-tab.jsx` (rujukan pola `actionError`)
- `src/components/features/base-features/confirm-modal.jsx`, `information-alert.jsx`
- `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` (`normalizeError`, `runDepartureAction`, `attachCreate`, `fetchTransfers`)
- Backend baca-saja: `EmergencyDepartureController.cs` (route, `[Tags]`, `[AccessPermission]`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-transfer-tab.jsx` | +28/−5. Impor `InformationAlert`; konstanta modul `ACTION_CONFIRM_MESSAGE` menggantikan string kalimat konfirmasi yang kini dipakai dua kali; state lokal `actionError`; `openAction` dan `closeAction` mengosongkan pesan saat modal dibuka dan ditutup; `runAction` mengosongkan pesan sebelum permintaan dan, bila ditolak, mengisinya dari `result.payload?.message` (cadangan *"Gagal memperbarui kepergian pasien."*); `ConfirmModal` menerima `children` berisi kalimat konfirmasi dan `InformationAlert variant="danger"` hanya ketika ada pesan |

Akhiran baris: 318 → 341 baris, seluruhnya CRLF (0 LF tunggal, 0 CR tunggal). Nol baris komentar baru; tiga blok komentar
lama tidak disunting dan tetap benar isinya.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi tidak berubah: view → slice (`runDepartureAction`) → `InstanceAxios`. View tidak memanggil Axios.
- Nol slice, konstanta domain, CSS, komponen baru, route, atau menu.
- Pola mengikuti tab Observasi (`FE-IGD-042`) pada modul yang sama — bukan pola baru.

**Gerbang base component.**

```
UI GATE: 3 elemen — REUSE 2, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Modal konfirmasi aksi kepergian | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx`; `children` menggantikan `message` (baris 181–184); alasan dikosongkan pada `onExited` (baris 148) | REUSE | Dipakai apa adanya |
| Pesan galat server | `InformationAlert` | `src/components/features/base-features/information-alert.jsx`; `variant="danger"` memberi `role="alert"` | REUSE | Tanpa props baru |
| Kalimat konfirmasi + pesan galat di badan modal | `ConfirmModal` + `InformationAlert` | Rangkaian yang sama di `emergency-assessment-observation-tab.jsx` (`FE-IGD-042`) | COMPOSE | Dirangkai di view |

Pilihan untuk baris COMPOSE, disajikan kepada pemilik sebelum kode ditulis:

- **A. Rangkai di view mengikuti tab Observasi — Rekomendasi, dijalankan.** Rupanya sama dengan layar IGD lain, base
  component tidak berubah, risiko regresi ke modul lain nol, cukup satu berkas.
- **B. Tambah prop `error` pada `ConfirmModal`.** Lebih rapi untuk ke depan, tetapi mengubah base component yang dipakai
  banyak modul; kartu ini melarang komponen dan perubahan di luar satu berkas.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol konfirmasi menampilkan putaran dan nonaktif; *Batal* dan tombol tutup ikut nonaktif (`loading={section.saving}`, tidak berubah) |
| Kosong | `NOT APPLICABLE` — task ini hanya menyangkut modal aksi; keadaan kosong riwayat tidak berubah (*"Belum ada kepergian pasien."*) |
| Gagal | Modal tetap terbuka; kalimat konfirmasi tetap tampil, di bawahnya kotak merah berisi pesan server apa adanya; isian alasan tetap utuh |
| Tanpa hak akses | Penolakan `403` dari pemeriksaan kewenangan unit (`BE-IGD-039`) tampil sebagai pesan di modal — inilah inti task ini. Penolakan baca tab (`section.forbidden`) tidak berubah |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru dan tidak ada perubahan request. Base URL:
`/api/v1/health-services/emergency-installation-management/emergency-departures`.

#### Health Services / Emergency Installation Management / Emergency Departure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/submit-handover` | *Ajukan Serah Terima* / *Ajukan Ulang Dokumen* | `EmergencyDeparture : Update` |
| `POST` | `/{id}/accept-handover` | *Terima Dokumen* | `EmergencyDeparture : Update` |
| `POST` | `/{id}/reject-handover` | *Tolak Dokumen* (`rejectionReason`) | `EmergencyDeparture : Update` |
| `POST` | `/{id}/depart` | *Catat Berangkat* | `EmergencyDeparture : Update` |
| `POST` | `/{id}/arrive` | *Catat Tiba* | `EmergencyDeparture : Update` |
| `PATCH` | `/{id}/cancel` | *Batalkan* (`cancellationReason`) | `EmergencyDeparture : Update` |
| `GET` | `/?emergencyVisitId=` | Muat ulang riwayat sesudah aksi berhasil (tidak berubah) | `EmergencyDeparture : Read` |

Bentuk galat yang dibaca: `response.data.message` (lewat `normalizeError`), dengan cadangan `title` lalu pesan Axios.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node_modules/.bin/eslint <berkas task>` | 0 error, 0 warning | `PASS` | Keluaran perintah, exit 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/emergency-*.test.mjs` | 91 lulus, 0 gagal | `PASS` | Keluaran perintah (Node `v20.20.2`) |
| `Get-NetTCPConnection -LocalPort 3000` sebelum build | Port kosong | `PASS` | Keluaran perintah |
| `npm run build` | Lulus 15.19.41–15.21.08 WIB: kompilasi 48 detik, 470/470 halaman, `postbuild` standalone siap; nol baris *warning*/*error* pada log | `PASS` | Log build di scratchpad sesi |
| Akhiran baris berkas task | 341 CRLF, 0 LF tunggal, 0 CR tunggal | `PASS` | Hitungan byte lewat Node |
| `git diff --stat` | 1 berkas, +28/−5 | `PASS` | Keluaran perintah |
| Baris tambahan mengandung `//`, `/*`, `*/` | Nol | `PASS` | `git diff -U0` |
| Grep anti-regresi pada baris tambahan | Warna literal 0, `<table` 0, utilitas tipografi Bootstrap 0, `!important` 0, inline style 0. `<button` 1 — baris tombol aksi lama yang hanya diganti `onClick`-nya; bukan tombol baru | `PASS` (temuan dipertahankan dengan alasan) | `git diff -U0` |
| Uji layar acceptance 1–7 | Belum dijalankan | `NOT RUN` | Dijadwalkan putaran Antigravity bersama tiga kartu lain (`IGD-DEC-208`) |

Uji manual: `REQUIRED` — belum dijalankan. Menurut aturan pemilik, uji layar dikerjakan agen Antigravity dari panduan yang
disiapkan agent, lalu agent memutus per skenario dari bukti mentah; panduannya disusun sesudah keempat kartu putaran ini
dibangun.

```
AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/emergency-*.test.mjs — PASS (91/91)
AUTOMATED TEST: SKIPPED (opsional) — perubahan hanya komposisi view; pemilik melarang unit test frontend baru
MANUAL TEST: NOT RUN — dijadwalkan putaran uji Antigravity (IGD-DEC-208)
```

**Tidak dijalankan:** `npm run lint:errors` seluruh repository (kartu meminta `eslint` berkas task); `npm run test:unit`
(perintah pengganti dari pemilik dipakai); `test:e2e` dan `test:uat` (tidak diminta).

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | *Catat Tiba* oleh perawat yang tidak ditugaskan di unit tujuan → `403`; modal tetap terbuka dan menampilkan pesan server apa adanya; kepergian tetap *Berangkat* | **Terpenuhi** — `043-U1` (bagian 9) | `runAction` baris 149–155: modal ditutup hanya bila `fulfilled`, selain itu `setActionError(result.payload?.message …)`; `children` baris 332–337. Uji `039-U2` ulang |
| 2 | *Terima Dokumen* ke unit yang belum dipetakan → kalimat `IGD-DEC-195` persis | **Terpenuhi** — `043-U2` (bagian 9) | Pesan diambil dari server tanpa diubah; `InformationAlert` hanya menyaring nilai berbentuk UUID. Uji `039-U1` ulang |
| 3 | Keenam tombol memakai jalur yang sama | **Terpenuhi** — `043-U3` (penolakan *Ajukan* `400`) di samping `043-U1`, `U2`, `U4`; keenam tombol lewat source (bagian 9) | Keenam aksi dari `actionsFor` (baris 63–80) masuk `openAction` → `ConfirmModal` → `runAction` yang sama |
| 4 | Terbaca tanpa kursor pada 1440 × 900; PNG viewport | **Terpenuhi** — PNG viewport `043-U1`, `043-U2` (1440 × 900) | Pesan berupa teks di badan modal (`InformationAlert`), bukan tooltip atau `title` |
| 5 | Alasan pada *Tolak Dokumen* dan *Batalkan* tetap utuh sesudah penolakan | **Terpenuhi** — kaki *Tolak Dokumen* `043-U4`; kaki *Batalkan* lewat source (bagian 9) | Modal tidak ditutup saat gagal; `ConfirmModal` baru mengosongkan alasan pada `onExited` (baris 148) |
| 6 | Pesan hanya milik aksi yang baru dijalankan | **Terpenuhi** — `043-U5` (bagian 9) | `openAction` (baris 133–136) dan `closeAction` (baris 138–141) mengosongkan pesan; `runAction` mengosongkan sebelum permintaan (baris 148) |
| 7 | Regresi: aksi berhasil menutup modal dan memuat ulang riwayat; tombol konfirmasi nonaktif selama permintaan | **Terpenuhi** — `044-U8`: modal tertutup, riwayat dan pesanan dimuat ulang; tombol nonaktif lewat source (bagian 9) | Cabang `fulfilled` tidak berubah (`setPendingAction(null)`, `reload()`); `loading={section.saving}` tidak berubah |
| 8 | Diff satu berkas; nol slice, konstanta domain, CSS, komponen baru, backend; nol komentar baru; komentar lama tidak disunting; tetap CRLF | **Terpenuhi** | Bagian 6 |
| 9 | `eslint` 0 error; uji IGD lama lulus; tanpa unit test baru; `npm run build` lulus | **Terpenuhi** | Bagian 6 |

**DoD:** laporan tracked ✅; register, node grafik R3.13.2 dan ringkasan, serta traceability ditandai ✅; uji layar pada
hasil build dengan bukti mentah diperiksa agent ✅ (6 Oktober 2026 sore, bagian 9, `IGD-DEC-218`). *Sebelumnya: uji layar belum.*

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nihil pada `eslint` dan build |
| Masalah yang diketahui | Di luar kartu, dicatat tanpa dikerjakan: (1) formulir *Koreksi Waktu* tidak menerima `saveError`, sehingga galatnya jatuh ke segmen *Formulir*; (2) penolakan aksi tetap ikut mengisi `transfers.saveError` bersama, sehingga pesan yang sama juga terbaca pada kartu *Buat Kepergian* di segmen *Formulir* — perilaku slice yang tidak disentuh kartu satu berkas ini; (3) tab Transfer tidak memuat ulang kartu pasien sesudah aksi; (4) kartu pasien kosong (`IGD-OQ-116`) dijawab `BE-IGD-064`; (5) `ServiceUnit : Read` pada Perawat IGD adalah konfigurasi peran (C3) |
| Komentar lama yang basi | Nihil — tiga blok komentar lama di berkas ini tetap benar isinya |
| Dependency backend | `BE-IGD-039` ✅ — tidak ada yang menahan layar |
| Perubahan sampingan | `NONE`. Keluaran build (`.next`) tidak muncul pada `git status` |
| Interupsi | `NONE` |
| Status Git | Frontend: ` M src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-transfer-tab.jsx`. Tidak di-*stage*, tidak di-commit |
| Langkah berikutnya | `build-module-backend` `BE-IGD-041` atas izin pemilik, lalu `BE-IGD-064` dan `FE-IGD-044`; sesudahnya satu panduan uji Antigravity yang memuat acceptance 1–7 kartu ini (`039-U1`, `039-U2` ulang, satu penolakan *Ajukan*, uji alasan utuh, buka–tolak–tutup–buka) |

---

## 9. Pemeriksaan bukti uji putaran bersama — 6 Oktober 2026 (sore)

Uji layar dijalankan agen Antigravity atas nama pemilik dengan panduan agent
[`2026-10-06-panduan-uji-putaran-bersama.md`](../../testing/2026-10-06-panduan-uji-putaran-bersama.md) (Blok A, aturan
A1–A18). Agent memutus dari bukti mentah `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-bersama-20261006/`
(JSON per skenario, PNG viewport, skrip per versi) dicocokkan dengan log backend `Logs/quilvian-backend-20261006.json`,
bukan dari ringkasan [laporan penguji](../../testing/2026-10-06-laporan-uji-putaran-bersama.md). Keputusan pemilik:
`IGD-DEC-218`; fakta `IGD-FACT-073`…`078` di decision log.

**Lingkungan.** Source kartu ini ter-commit pemilik di frontend `2a985f6d5` (5 Oktober 2026); working tree
`emergency-assessment-transfer-tab.jsx` kini juga memuat perubahan `FE-IGD-044` di atasnya. Layar dilayani
`node .next/standalone/server.js` dari `BUILD_ID` 11.13.16 WIB (build `FE-IGD-044` yang memuat source kartu ini),
viewport 1440 × 900, `nextjsPortalNull` benar di setiap skenario layar. Seluruh langkah layar memakai akun `PERAWAT`
(`dimas.kurniawan`, `IGD-DEC-215`; `isSuperAdmin: false`); nol permintaan SuperAdmin dan nol permintaan ke
`/api/v1/administrator/**` sejak 04.20 UTC. Penugasan HR akun itu di simpul Instalasi Gawat Darurat, bukan di simpul
Instalasi Rawat Inap (Q-P2); unit tujuan *ICU* sengaja belum dipetakan (Q-UNIT).

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| `043-U1` | **Terbukti** — acceptance 1, 4 | `POST …/arrive` `403` 05.18.15 UTC; `message` persis *"Anda tidak bertugas di unit Rawat Inap, sehingga tidak dapat mencatat kedatangan pasien."*; PNG: modal *Catat Tiba?* tetap terbuka, kalimat itu terbaca di kotak merah di bawah kalimat konfirmasi; sesudah ditutup, kartu tetap *Fisik: Sudah Berangkat* |
| `043-U5` | **Terbukti** — acceptance 6 | Modal *Catat Tiba* dibuka ulang tanpa konfirmasi: memuat *"Perubahan akan dicatat sebagai kejadian baru dalam riwayat kepergian pasien."*; tidak memuat kalimat galat sebelumnya; tidak ada kotak galat; nol permintaan jaringan |
| `043-U2` | **Terbukti** — acceptance 2, 4 | `POST …/accept-handover` `403` 05.18.32 UTC; `message` persis kalimat `IGD-DEC-195` untuk unit *ICU*; modal tetap terbuka dan memuatnya (PNG); kalimat lama *"Lanjutkan dengan menyertakan alasan"* tidak muncul di modal maupun respons |
| `043-U4` | **Terbukti** — acceptance 5 (kaki *Tolak Dokumen*) | Alasan `Uji 043 alasan utuh` diketik lalu dikonfirmasi; `POST …/reject-handover` `403` 05.18.36 UTC dengan badan `rejectionReason` = alasan itu; modal tetap terbuka, kotak alasan masih berisi `Uji 043 alasan utuh` persis (PNG dan nilai input) |
| `043-U3` | **Terbukti** — acceptance 3 (satu penolakan lain) | `POST …/submit-handover` `400` 05.18.51 UTC; `message` persis *"Masih ada 3 pesanan yang belum ditentukan sikapnya."*; modal *Ajukan Serah Terima?* tetap terbuka dan memuatnya (PNG) |
| `044-U8` (`043-U6`) | **Terbukti** — acceptance 7 (bagian modal tertutup dan riwayat dimuat ulang) | `POST …/submit-handover` `200` 05.19.03 UTC; modal tertutup sendiri; disusul `GET …/emergency-departures` dan `GET …/order-items` `200`; kartu *Dokumen: Menunggu Unit Tujuan* (PNG). Atribut `disabled` tombol konfirmasi selama permintaan tidak direkam (butir panduan *"bila tertangkap"*) |

Pencocokan: seluruh permintaan di `jaringan[]` keenam skenario cocok dengan log (method, path, kode status, akun);
putusan dihitung ulang dari `pemeriksaan` — enam `PASS`. Bagian acceptance yang dinyatakan lewat source sesuai pemetaan
panduan bagian 8: keenam tombol (acceptance 3), kaki *Batalkan* (acceptance 5), dan tombol nonaktif selama permintaan
(acceptance 7, `loading={section.saving}` tidak berubah).

**Penyimpangan — bukti diterima dengan penyimpangan tercatat (`IGD-DEC-218`).**

| # | Penyimpangan | Aturan | Penanganan |
| ---: | --- | --- | --- |
| a | Persiapan, satu sesi layar 04.47–04.50 UTC, dan lingkup percobaan 1–2 tidak dilaporkan lengkap; bukti percobaan 1–2 tertimpa. Percobaan 2 sudah menjalankan Blok A pada VA1 dan VA2 — seluruhnya ditolak `403`, sehingga keadaan kunjungan yang dipakai ulang percobaan 3 tidak bergeser | A11, A12 | Dicatat (`IGD-FACT-076`) |
| b | Laporan penguji menulis `043-U4` sebagai *"Uji alasan tetap … batal modal; buka kembali"*; bukti mentah menunjukkan langkah persis panduan | A12 | Dicatat (`IGD-FACT-078`); putusan dari JSON, PNG, dan skrip; laporan penguji tidak disunting |
| c | Lencana kartu dicocokkan tanpa membedakan huruf besar (*Sudah berangkat* terbaca *Sudah Berangkat* karena `.badge` global mengkapitalkan teks); kalimat pesan di modal tetap dicocokkan persis | A7 | Dicatat; bukan cacat layar |

| Hal | Isi |
| --- | --- |
| Status | ✅ **SELESAI — 6 Oktober 2026 (sore)**: 9 dari 9 acceptance terbukti (`IGD-DEC-218`) |
| Implementation | Selesai — satu berkas, ter-commit `2a985f6d5` |
| Developer verification | `eslint` 0/0, uji IGD lama 91/91, `npm run build` lulus (bagian 6); uji layar Blok A dan `044-U8` pada hasil build, bukti mentah dan log diperiksa agent |
| UAT | Belum dijalankan — diserahkan ke tim UAT; bukan klaim UAT |
| Komentar lama yang basi | Nihil |
| Masalah yang diketahui | Tetap seperti bagian 8; butir (3) dijawab `FE-IGD-044` (kartu pasien dimuat ulang sesudah sikap pesanan) dan butir (4) dijawab `BE-IGD-064` ✅ |
| Langkah berikutnya | Pemilik: commit dokumen, lalu rilis bersama pasangan `BE-IGD-041` dan `FE-IGD-044` (`IGD-DEC-204`) |
