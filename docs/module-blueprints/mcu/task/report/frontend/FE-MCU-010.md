# Laporan Perubahan Frontend — `FE-MCU-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-010` |
| Judul | Lapisan data pemeriksaan, resume, penyerahan |
| Slice | `MVP-2` (pasangan `BE-MCU-020`..`025`; resume dan penyerahan dipakai `MVP-3`) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-010`* |
| Trace | `FR-MCU-030`..`053`; `MCU-FE-003` (privasi ringkasan perusahaan, `MCU-DEC-010`); `03-frontend-architecture.md` §4 (*Data basi*, *Kiriman ganda*, *Hasil Lab/Rad berubah*) |
| Contract version | `MCU-API-v1` grup `Mcu Examination`, `Mcu Resume`, dan `Mcu Delivery` — `approved` 2026-10-21. Ke-21 endpoint cocok dengan `McuExaminationController`, `McuResumeController`, dan `McuDeliveryController` (route, method, nama ruas DTO, nilai `variant`) |
| Wewenang UI | `NOT APPLICABLE` — tanpa tampilan |
| Dependency | Kontrak API ✅; backend `BE-MCU-021`..`025`, `030`, `031`, `033`, `034` sudah melayani endpoint |
| Klasifikasi | `MEDIUM` — satu repository, berkas diperiksa 9–20, berkas diubah 4–8, logika sedang (penjaga versi, muat ulang basi per grup), kontrak yang sudah ada, tanpa database, keamanan berkaitan (privasi ringkasan, `version`, pesan `403`), tanpa UI |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: konstanta, service, slice `mcuExamination`, satu reducer di slice `mcuOrder`, registrasi store, uji. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `1b7ff563` (branch `yoga`) |
| Tanggal | 2026-10-27 |
| Status | Selesai — kriteria terbukti uji unit; lint dan build lulus |

---

## 1. Keadaan yang ditemukan di awal

- Lapisan data order dan check-in (`FE-MCU-006`) sudah ada: `mcu-order.service.js`, slice `mcuOrder`, dan pembaca
  galat bersama `mcu-request-rules.js` (`readMcuServerFailure`, `isMcuStaleOrderFailure`, `isMcuOrderVersion`).
- Ketiga controller backend sudah ada dan semuanya berada di bawah `/orders/{id}`; resume di
  `/orders/{id}/resumes`.
- **`version` di setiap badan permintaan adalah `Version` order**, bukan versi komponen atau resume. Setiap service
  membandingkan `order.Version != request.Version` dan menjawab `409` `VAL-MCU-46` *"Data order sudah diubah petugas
  lain. Muat ulang lalu coba lagi."*.
- Tiga aksi ubah **tidak** membawa `version`: tambah komponen, simpan hasil Pemeriksaan MCU, dan muat ulang status
  komponen. Ketiganya tetap bisa menjawab `409` `VAL-MCU-46` saat bentrok baris (`DbUpdateConcurrencyException`).
- Hampir setiap aksi ubah menaikkan `Version` order (`Touch`) dan bisa memindah status order (mis. muat ulang
  komponen → *Menunggu Resume*, approve → *Disetujui Dokter*). Jawaban resume, penyerahan, dan persetujuan memuat
  `orderVersion`, tetapi tidak memuat status order. Jawaban batal/jadwal ulang komponen tidak memuat versi sama
  sekali.
- `GET result-document` memilih bentuk jawaban dari `?variant`: `full` → `McuResultDocumentResponse`,
  `fitness-summary` → `McuFitnessSummaryDocumentResponse` (tanpa nilai pemeriksaan, temuan, maupun nomor RM). Nilai
  lain ditolak `400`.
- Daftar riwayat dari server terurut terbaru lebih dulu: resume menurut `VersionNumber`, penyerahan menurut
  `DeliveredAt`, persetujuan menurut `ConsentGivenAt`.
- Label enum ada di `McuLabels` backend (mis. saluran *Ambil langsung*/*WhatsApp*/*Email*/*Telepon*).

---

## 2. Proses bisnis dari sisi pengguna

Tanpa layar. Lapisan ini dipakai Layar pemeriksaan (`FE-MCU-011`), Resume dokter (`FE-MCU-012`), dan
Verifikasi/cetak/penyerahan (`FE-MCU-013`):

- **Pemeriksaan** — petugas menekan *Mulai pemeriksaan*; server memulangkan detail order beserta komponen dan nomor
  pesanan Lab/Rad, dan detail itu langsung dipakai tanpa memuat ulang. *Muat ulang status komponen* memperbarui
  daftar komponen dan detail order. Dokter menambah, membatalkan, atau menjadwal ulang komponen, lalu mengisi hasil
  Pemeriksaan MCU. Nilai isian dikirim apa adanya, mis. `6.4` tetap `"6.4"`.
- **Resume** — draf, approve, kembalikan, verifikasi, dan buka ulang. Buka ulang membuat versi baru dan versi lama
  menjadi *Dikoreksi*, jadi seluruh versi dimuat ulang. `selectMcuCurrentResume` memberi versi yang berlaku.
- **Penyerahan** — hasil lengkap untuk cetak diambil dengan `variant=full`. **Ringkasan untuk perusahaan diambil
  terpisah dengan `variant=fitness-summary`** dan disimpan persis seperti jawaban server. Tidak ada jalur yang
  menyalin atau menyaring hasil lengkap menjadi ringkasan di browser. Bila ringkasan gagal dimuat, ringkasan tetap
  kosong walaupun hasil lengkap sudah ada di state. Penyerahan, pemberitahuan koreksi, serta persetujuan pelepasan
  dan pencabutannya dicatat dengan `version` order.
- **Data basi** — bila petugas lain lebih dulu mengubah order, server menjawab `409` `VAL-MCU-46`. Lapisan ini
  memuat ulang detail order **dan** data grup yang sedang dikerjakan sebelum galat sampai ke layar:
  - aksi komponen → `GET /components`;
  - simpan hasil → `GET /components` dan `GET …/examination-result`;
  - resume → `GET /resumes`;
  - penyerahan → `GET /deliveries`;
  - persetujuan → `GET /release-consents`.

  Pesan server tampil apa adanya, dan `actionStale` menandai kejadian ini.
- **Berhasil** — data grup diganti jawaban server, misalnya status komponen ikut menjadi *Selesai* saat hasil
  ditandai selesai, atau penyerahan baru ditaruh paling atas. Detail order dimuat ulang supaya layar memegang versi
  dan status order terbaru.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/medical-checkup-management/medical-checkup-constants.js` | `MCU_RESULT_DOCUMENT_VARIANT` (`full`, `fitness-summary`); komentar `MCU_API.orders` menyebut tiga grup baru |
| `src/lib/constants/health-services/medical-checkup-management/mcu-patients/mcu-examination-constants.js` (baru) | Enum angka `McuOrderComponentStatus`, `McuResumeStatus`, `McuDeliveryRecipient`/`Content`/`Channel`, `McuConsentEvidenceType`, dengan opsi berlabel sama persis dengan `McuLabels` |
| `src/lib/services/health-services/medical-checkup-management/mcu-examination.service.js` (baru) | 22 fungsi lewat `InstanceAxios` untuk 21 endpoint; `result-document` punya dua fungsi, satu per `variant`. Id di-`encodeURIComponent` |
| `src/lib/state/slice/health-services/medical-checkup-management/mcu-examination-slice.jsx` (baru) | 7 thunk bacaan dan 15 thunk ubah. `createWrite` mengurus penjaga versi, muat ulang basi per grup, dan muat ulang detail sesudah berhasil. State tiap bacaan punya data, penanda muat, galat, dan kode status. Tersedia pula `selectMcuCurrentResume`, `clearMcuExamination`, dan `clearMcuResultDocuments` |
| `src/lib/state/slice/health-services/medical-checkup-management/mcu-order-slice.jsx` | Reducer `receiveMcuOrderDetail`: detail hasil *Mulai pemeriksaan* langsung dipakai, dan baris daftar yang sama ikut diperbarui |
| `src/lib/state/store.jsx` | Registrasi `mcuExamination` |
| `tests/unit/mcu-examination-slices.test.mjs` (baru) | 10 uji `node:test` |

### 3.2 Kepatuhan arsitektur frontend

- Alur `slice → service → InstanceAxios`, sama dengan `mcu-order-slice.jsx`. Alamat hanya di konstanta domain.
- Satu service dan satu slice untuk tiga grup, karena ketiganya melayani satu order dalam satu alur (pemeriksaan →
  resume → penyerahan). Detail order tetap milik `mcuOrder`; slice baru mengimpor dari `mcuOrder`, tidak sebaliknya,
  sehingga tidak ada impor melingkar.
- Pembaca galat dan penjaga versi memakai `mcu-request-rules.js` yang sudah ada. Pesan *"Versi order tidak
  diketahui…"* sama dengan `FE-MCU-006`.
- Tidak ada dependency, abstraksi HTTP, atau pola state baru.

---

## 4. State yang ditangani

| State | Yang tersedia untuk layar |
| --- | --- |
| Memuat | Untuk setiap bacaan tersedia `<bacaan>Loading`: `components`, `examinationResult`, `resumes`, `resultDocument`, `fitnessSummary`, `deliveries`, `releaseConsents`. Tersedia juga `componentsRefreshing` dan `actionLoading` |
| Kosong | Larik kosong / `null` sesudah muat berhasil |
| Gagal | `<bacaan>Error` = kalimat server apa adanya, dan `<bacaan>ErrorStatus` = kode HTTP. Aksi ubah memakai `actionError`, `actionErrorStatus`, dan `actionErrorCode` (`VAL-MCU-xx`). Muat ulang status komponen memakai `componentsRefreshError`, terpisah dari formulir aksi |
| Data basi | `actionStale: true`; detail order dan data grup sudah dimuat ulang |
| Tanpa hak akses | `403` dengan pesan server, mis. `VAL-MCU-38` *dokter yang ditetapkan*, di ruas galat yang sama |
| Pembatalan | Permintaan bacaan yang dibatalkan (`meta.aborted`) tidak menulis galat |
| Privasi | `fitnessSummary` hanya diisi dari `variant=fitness-summary`; `clearMcuResultDocuments` mengosongkan kedua dokumen cetak saat layar cetak ditutup |

---

## 5. Endpoint yang dikonsumsi

Base URL: `/v1/health-services/medical-checkup-management/orders/{id}`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/start-examination` | Mulai pemeriksaan (`version`) | `McuExamination : Start` |
| `GET` | `/components` | Komponen + status + pesanan Lab/Rad | `McuExamination : Read` |
| `POST` | `/components/refresh` | Muat ulang status komponen | `McuExamination : Read` |
| `POST` | `/components` | Tambah komponen di luar paket | `McuExamination : AddComponent` |
| `PATCH` | `/components/{componentId}/cancel` | Batal komponen (`version`) | `McuExamination : CancelComponent` |
| `PATCH` | `/components/{componentId}/reschedule` | Jadwal ulang komponen (`version`) | `McuExamination : Update` |
| `GET` | `/components/{componentId}/examination-result` | Isian + hasil Pemeriksaan MCU | `McuExamination : Read` |
| `PUT` | `/components/{componentId}/examination-result` | Simpan hasil | `McuExamination : Update` |
| `GET` | `/resumes` | Seluruh versi resume | `McuResume : Read` |
| `PUT` | `/resumes/current` | Simpan draf (`version`) | `McuResume : Update` |
| `POST` | `/resumes/current/approve` | Approve dokter (`version`) | `McuResume : Approve` |
| `POST` | `/resumes/current/return` | Kembalikan ke dokter (`version`) | `McuResume : Verify` |
| `POST` | `/resumes/current/verify` | Verifikasi administratif (`version`) | `McuResume : Verify` |
| `POST` | `/resumes/reopen` | Buka ulang (`version`) | `McuResume : Reopen` |
| `GET` | `/result-document?variant=full` / `?variant=fitness-summary` | Data cetak hasil lengkap / ringkasan kelayakan | `McuDelivery : Read` |
| `GET` | `/deliveries` | Riwayat penyerahan | `McuDelivery : Read` |
| `POST` | `/deliveries` | Catat penyerahan (`version`) | `McuDelivery : Create` |
| `PATCH` | `/deliveries/{deliveryId}/correction-notice` | Catat pemberitahuan koreksi (`version`) | `McuDelivery : Create` |
| `GET` | `/release-consents` | Persetujuan pelepasan | `McuDelivery : Read` |
| `POST` | `/release-consents` | Catat persetujuan (`version`) | `McuDelivery : ManageConsent` |
| `PATCH` | `/release-consents/{consentId}/revoke` | Cabut persetujuan (`version`) | `McuDelivery : ManageConsent` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-examination-slices.test.mjs` | 10/10 lulus | `PASS` | `# pass 10`, `# fail 0` |
| Kriteria: uji unit thunk | 21 endpoint → 21 pasangan method + path unik yang sama persis dengan `MCU-API-v1` | `PASS` | Uji *setiap thunk…* |
| **Kriteria `MCU-FE-003`:** ringkasan dari `variant=fitness-summary` | Hasil lengkap mengirim `{ variant: "full" }`. Sesudah hasil lengkap (berisi *HBsAg reaktif*, nomor RM) ada di state, `fitnessSummary` tetap `null`. Ringkasan mengirim `{ variant: "fitness-summary" }` ke path yang sama dan tersimpan `deepEqual` jawaban server; teksnya tidak memuat *HBsAg*, *Reaktif*, nomor RM, maupun rekomendasi klinis | `PASS` | Uji *hasil lengkap memakai variant=full…* |
| Ringkasan gagal dimuat | `409` → `fitnessSummary` tetap `null`, galat = pesan server + `409`, hasil lengkap tidak dipakai sebagai cadangan; pemeriksaan teks sumber: tidak ada penugasan `fitnessSummary = …resultDocument` | `PASS` | Uji *ringkasan gagal dimuat…* |
| Aksi ber-`version` | Ke-12 aksi mengirim `version: 7` di body. `undefined`, `null`, `"7"`, `-1`, dan `1.5` ditolak tanpa satu pun permintaan | `PASS` | Uji *aksi ber-version…* |
| Aksi tanpa `version` | Tambah komponen, simpan hasil, dan muat ulang status tidak meminta `version`; badan simpan hasil = `{ values: [{ examinationFieldId, value: "6.4" }], conclusion, complete: true }` | `PASS` | Uji *tambah komponen…* |
| `409` `VAL-MCU-46` | Untuk ke-15 aksi, urutan permintaan = aksi → `GET /{id}` → bacaan grup (lihat §2). Detail dan baris daftar menjadi versi 10. Galat = pesan server persis + `409` + `VAL-MCU-46` + `stale: true`. Muat ulang status komponen menulis ke `componentsRefreshError`, bukan `actionError` | `PASS` | Uji *409 VAL-MCU-46…* |
| `403` / `409` lain / `400` | Satu permintaan saja, tanpa muat ulang; pesan server persis; `actionStale: false` | `PASS` | Uji *403 dan 409 lain…* |
| Sesudah berhasil | *Mulai pemeriksaan* tidak memuat ulang (detail jawaban dipakai; daftar order ikut versi 10); approve memuat ulang `GET /{id}`; komponen baru di bawah; hasil selesai mengubah status komponen; buka ulang memuat ulang `GET /resumes`; penyerahan baru di atas; pemberitahuan koreksi dan pencabutan mengganti baris yang sama | `PASS` | Uji *mulai pemeriksaan…*, *komponen, hasil, resume…* |
| Pembatalan dan pengosongan | Bacaan yang dibatalkan tidak menulis galat; `clearMcuExamination` mengosongkan seluruh data | `PASS` | Uji *pembatalan permintaan…* |
| Uji kontrol negatif | Thunk ringkasan diubah sementara agar memakai `getMcuResultDocument` (`variant=full`) → dua uji ringkasan **gagal** (8 lulus, 2 gagal); berkas dipulihkan dari salinan, kembali 10/10 | `PASS` | Keluaran `node --test` |
| Seluruh uji unit MCU (10 berkas) | 78/78 lulus, termasuk `mcu-order-slices.test.mjs` sesudah reducer baru | `PASS` | `# pass 78` |
| `npx eslint` atas 7 berkas yang diubah | Tanpa galat maupun peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.907 uji: 3.865 lulus, 42 gagal. Daftar nama yang gagal **identik** dengan baseline `FE-MCU-006` (`diff` kosong); semuanya di luar MCU | `EXISTING ISSUE` | Keluaran `node --test` + `diff` |
| `npm run build` | Exit 0: *Compiled successfully in 56s*; standalone siap | `PASS` | Keluaran build |

`MANUAL TEST: NOT APPLICABLE` — tanpa kontrol interaktif; kontrol layar diuji di `FE-MCU-011`..`013`.
`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU, identik baseline; 78/78 uji MCU PASS)`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Uji unit thunk | Terpenuhi | §6 |
| Ringkasan perusahaan diambil dari `result-document?variant=fitness-summary`, tidak disusun dari data lengkap di browser (`MCU-FE-003`) | Terpenuhi | §6 |
| DoD: uji unit lulus | Terpenuhi untuk uji MCU; suite penuh membawa 42 kegagalan existing | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta kontrak | Kolom *Status* di `contracts/api-contract.md` untuk ketiga grup masih *Rencana (belum tersedia)*, padahal endpoint sudah dilayani backend (`BE-MCU-021`..`034` ✅). Route, method, dan DTO cocok dengan kontrak. Kolom status itu tidak diubah di task ini |
| Peringatan | (1) Setiap aksi ubah yang berhasil memicu satu `GET /{id}` tambahan, karena jawaban komponen/resume/penyerahan tidak membawa status order. (2) Seperti `FE-MCU-006`, `detailLoading` sempat menyala sesaat sesudah aksi; layar `FE-MCU-011`..`013` sebaiknya tidak mengunci seluruh halaman karenanya |
| Masalah yang diketahui | Bentuk bukti persetujuan pelepasan menunggu `MCU-OQ-003` (menahan go-live); lapisan data mengikuti enum backend saat ini |
| Dependency backend | Tidak ada; endpoint sudah dilayani |
| Perubahan sampingan | `NONE`. Uji kontrol negatif memakai salinan berkas di direktori sementara; berkas dipulihkan dan 10/10 lulus sesudahnya |
| Interupsi | Sesi dilanjutkan dari ringkasan konteks sebelum pekerjaan dimulai; tidak ada berkas setengah jadi |
| Status Git (frontend) | ` M medical-checkup-constants.js`, ` M store.jsx`; `??` `mcu-examination.service.js`, `mcu-examination-slice.jsx`, `mcu-patients/mcu-examination-constants.js`, `tests/unit/mcu-examination-slices.test.mjs`. `mcu-order-slice.jsx` masih `??` dari `FE-MCU-006` dan kini memuat `receiveMcuOrderDetail` |
| Langkah berikutnya | `FE-MCU-011` (Layar pemeriksaan) — kini siap; data uji `MCU-RSMMC-000002`/`000004` (Hadir) tersedia di devYoga untuk UAT *Mulai pemeriksaan* |
