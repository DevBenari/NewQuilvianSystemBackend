# Laporan Perubahan Frontend — `FE-BD-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-005` |
| Judul | Golongan darah, bukti kecocokan, pemberian, dan jalur darurat |
| Slice | 3 — Kantong darah, pemberian, dan penyelesaiannya |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md) — kartu `FE-BD-005` |
| Trace | `DEC-BD-013/017/027/028/038/040/042` · `03-frontend-architecture.md` §3 `FE-BD-05` · kewajiban layar `FE-BD-005`, `FE-BD-007`, `FE-BD-008`, `FE-BD-012`, `FE-BD-013`, `FE-BD-018`, `FE-BD-021` · `validation-matrix.md` `VAL-BD-017/018/019/020/020b/021/065/066/070/071/072/078/079` |
| Contract version | api-contract `v5` (`approved`, `Sukmagp` 19 September 2026). Endpoint yang dipakai tidak berubah sejak `v4`; kartu roadmap masih menulis `v4` |
| Wewenang UI | Layar `FE-BD-05` (detail kantong) sebatas bukti kecocokan, pemberian jalur normal, dan jalur darurat. Rupa layar `DEV_DISCRETION`. **Tidak** mencakup layar `FE-BD-06` Pemeriksaan Golongan Darah, koreksi (`FE-BD-008`), maupun penyelesaian `PendingReview` (`FE-BD-007`) — keputusan pemilik bagian 1.1 |
| Dependency | `BE-BD-005` ✅, `BE-BD-007` ✅, `BE-BD-008` ✅, **`BE-BD-021` ✅** (proyeksi gerbang, api-contract `v5` `D7`; ter-commit `49c8af97` dan di-push — dependency lanjutan 25 September 2026) |
| Klasifikasi | `MEDIUM` — satu layar existing diperluas; tiga endpoint tulis; satu hook dan satu komponen dialog baru; nol route, nol menu, nol Redux |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `V2QuilvianSystemFrontendDev` (source dan test); laporan ini beserta tautan roadmap dan traceability pada repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | **Lanjutan 25 September 2026:** basis `c803b9beb` di `sukmagpV2`; source lanjutan **belum di-commit**. **Riwayat:** basis `b8ea49a1fe4511afd17057ff7c6b8c3c75f75c25`. **Source task ini ter-commit sebagai `88bd2b1c7a8ce8e03889c0352f66d048c82cc1f0`** di `sukmagpV2` (`feat(bank-darah): complete FE-BD-005 compatibility evidence and issuance workflow`, 10 berkas), atas persetujuan pemilik sesudah validasi final. Belum di-push |
| Commit backend yang dijadikan rujukan | `b39c6cc4901a40c9ba772332b82575045ba0dbfd` (`sukmagp`) |
| Tanggal | 24 September 2026 |
| Status | 🟡 **Sebagian — diperbarui 25 September 2026 (lanjutan sesudah `BE-BD-021`, bagian 6.4).** `FE-BD-008`, `FE-BD-013`, dan `FE-BD-021` kini **terpenuhi**: gerbang pemberian terbaca dan tombol Berikan tertahan **sebelum** ditekan, masa berlaku bukti tampil, dan gerbang jalur darurat ditetapkan dari proyeksi backend. `lint:errors` `PASS`, `build` `PASS` (371 halaman), `test:unit` 1624 test — 1617 lulus termasuk 5 test baru, 7 kegagalan lama sama dengan baseline; **runtime `G0`–`G9` 10 dari 10 `PASS`** di Chromium terhadap backend sungguhan. **Tetap 🟡** karena dua butir: outcome **mencatat golongan darah** (di luar cakupan keputusan pemilik no. 1–2; layar `FE-BD-06` hanya validasi/penyelesaian konflik lewat `FE-BD-009`), dan `FE-BD-007` **menahan tombol** — penanda konflik kini terlihat, tetapi backend tidak menjadikan konflik golongan darah gerbang pemberian (`VAL-BD-034`) dan layar dilarang membuat aturan sendiri; bukti penanda positif memakai jawaban tersimulasi karena database tidak punya pasien yang menahan konflik. **Riwayat (24 September 2026):** 🟡 **Sebagian** — cakupan yang diputuskan pemilik terbukti. `lint:errors` `PASS`, `build` `PASS`, `test:unit` `PASS` untuk cakupan task, dan **validasi runtime `R0`–`R9` seluruhnya `PASS`** (14 skenario) di Chromium terhadap backend sungguhan, 24 September 2026 (bagian 6.2). Cacat UX pemilih dialog yang ditemukan runtime **sudah diperbaiki** dan dibuktikan ulang pada `R3` dan `R9` (bagian 6.3). Tetap 🟡 karena empat butir kartu tidak dapat dipenuhi tanpa data backend yang belum ada (golongan darah `FE-BD-06`, `FE-BD-013`, `FE-BD-007`, `FE-BD-008`). Gap itu dicatat sebagai backlog atas keputusan pemilik, bukan dibuka sebagai task (bagian 8). **Riwayat:** 🟡 dengan cacat UX pemilih dialog belum diperbaiki;  🟡 dengan runtime `NOT RUN` karena cookie sesi belum tersedia |

---

## 1. Keadaan yang ditemukan di awal

- Detail kantong (`FE-BD-05`) sudah ada dari `FE-BD-004` (alokasi) dan `FE-BD-012` (penyimpanan
  lokasi). Backend sudah menawarkan aksi `RecordCompatibilityEvidence`, `Issue`, dan `EmergencyIssue`
  pada `AvailableActions` kantong berstatus Dialokasikan, tetapi layar sengaja mengabaikannya.
- Ketiga endpoint backend sudah terbukti runtime (`BE-BD-007`, `BE-BD-008`).
- Layar `FE-BD-06` Pemeriksaan Golongan Darah belum punya satu berkas source pun, dan butir menunya
  belum terdaftar.
- **Celah kontrak yang ditemukan audit** (belum ada di backend, bukan cacat frontend):
  1. `GET /blood-units/{id}` tidak memulangkan masa berlaku bukti (`validUntil`/kedaluwarsa) maupun
     hasil penilaian gerbang pemberian. Penilaian itu hanya ada di dalam
     `BbkBloodUnitService.EvaluateIssuanceGateAsync`.
  2. Keadaan gerbang untuk jalur darurat (`EvidenceGateClosed`, `LocationGateClosed`) juga hanya ada di
     dalam service (`EvaluateEmergencyBypassAsync`).
  3. Tidak ada gerbang kantong yang memeriksa golongan darah sah pasien. `VAL-BD-034` tidak ditegakkan
     pada alokasi, bukti, pemberian, maupun jalur darurat.

### 1.1 Keputusan pemilik, 24 September 2026

Sesudah audit, `Sukmagp` memutuskan:

| No | Keputusan |
| ---: | --- |
| 1 | Perluas detail kantong yang sudah ada; **tidak** membuat halaman baru |
| 2 | Cakupan: catat bukti kecocokan, berikan kantong, jalur darurat. **Tidak termasuk** koreksi (`FE-BD-008`) dan penyelesaian `PendingReview` (`FE-BD-007`) |
| 3 | Frontend **tidak** menghitung masa berlaku bukti, kecocokan, gerbang lokasi, maupun konflik golongan darah. Backend menjadi sumber kebenaran |
| 4 | Jalur normal: Catat Bukti Kecocokan dan Berikan. Jalur darurat: tombol terpisah yang tampil sebagai jalur tidak normal |
| 5 | Pakai `AvailableActions` backend, penjagaan hak akses, dan kode/pesan galat backend |
| — | Validasi runtime memakai cookie sesi superadmin lokal lewat berkas di scratchpad; data uji dibiarkan pada kantong `TEST-` dan ID-nya dicatat |

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Mencatat bukti kecocokan

1. Petugas Bank Darah membuka detail kantong berstatus **Dialokasikan** dari daftar Kantong Darah.
2. Tombol **Catat Bukti Kecocokan** tampil bila backend menawarkan aksinya **dan** petugas memegang
   `BloodUnit : Compatibility`.
3. Dialog menampilkan pasien tujuan dari alokasi aktif. Petugas memilih hasil (**Cocok** atau
   **Tidak cocok**) dan waktu pemeriksaan. Tanggal dan jam terisi waktu sekarang dan boleh diubah.
4. Bila memilih Tidak cocok, dialog mengingatkan bahwa bukti itu tetap disimpan tetapi tidak membuka
   pemberian jalur normal.
5. Backend menyimpan bukti. Pasien tujuan dan pencatat ditentukan backend. Tabel **Bukti Kecocokan**
   bertambah satu baris, terbaru di atas.

Contoh: kantong `PMI-2026-0050` dialokasikan untuk Budi. Petugas mencatat hasil Cocok pukul 08.30
waktu setempat. Layar mengirim `checkedAt` dalam UTC (`01:30Z` untuk WIB), karena backend menolak
waktu yang bukan UTC.

### 2.2 Memberikan kantong lewat jalur normal

1. Tombol **Berikan** tampil bila backend menawarkan aksinya dan petugas memegang
   `BloodUnit : Issue`.
2. Dialog konfirmasi menyebut bahwa pemberian bersifat akhir, dan menampilkan pasien tujuan serta
   lokasi kantong saat ini.
3. Backend menilai ulang seluruh gerbang saat tombol ditekan: kantong masih dialokasikan, lokasi
   penyimpanannya aktif, dan bukti kecocokan yang berlaku untuk pasien tujuan belum lewat masa
   berlaku serta menyatakan cocok.
4. Bila berhasil, status menjadi **Diberikan**, baris **Diberikan** muncul dengan penanda
   **Jalur normal**, dan bukti yang dipakai bertanda **Dipakai untuk pemberian**.

**Bila backend menolak (`422`).** Dialog ditutup. Pesan backend tampil apa adanya pada kepala layar
dengan judul **Pemberian ditahan**, dan tombol Berikan dinonaktifkan untuk versi kantong itu.
Contoh:

| Kode backend | Pesan yang dibaca petugas |
| --- | --- |
| `VAL-BD-065` | "Kantong ini berada di lokasi penyimpanan yang sudah tidak aktif dan belum dapat diberikan. Pindahkan dulu ke lokasi yang aktif." |
| `VAL-BD-079` | "Hasil pemeriksaan kecocokan menyatakan kantong ini tidak cocok untuk pasien tersebut. Kantong tidak dapat diberikan." |
| `VAL-BD-020` | "Bukti kecocokan sudah lewat masa berlaku. Diperlukan bukti kecocokan yang baru." |

Penahan dilepas begitu kantong berubah, misalnya bukti baru dicatat atau kantong dipindahkan, atau
begitu petugas menekan **Muat ulang**. Layar tidak menilai sendiri apakah gerbang sudah terbuka.
Pada percobaan berikutnya backend yang menilai lagi.

### 2.3 Jalur darurat — jalur tidak normal

1. Tombol **Jalur Darurat** berwarna peringatan dan terpisah dari Berikan. Tombol ini tampil bila
   backend menawarkan aksinya dan petugas memegang `BloodUnit : EmergencyIssue` **dan**
   `BloodBankReason : Read`. Butir kedua diperlukan karena alasan wajib dipilih dari daftar.
2. Dialog dibuka dengan peringatan **Jalur tidak normal**, pasien tujuan, lokasi saat ini (beserta
   penanda nonaktif dari backend), dan penolakan pemberian normal terakhir bila ada.
3. Penerbit memilih **alasan darurat**, **gerbang yang dilewati**, dan **diterbitkan sebagai** (Dokter
   Bank Darah atau Dokter penanggung jawab pasien), lalu menulis **keadaan kedaruratan**. Ketiga
   pilihan **tidak punya nilai bawaan**, dan peran penerbit tidak disimpulkan dari akun.
4. Bila pilihan gerbang tidak sesuai keadaan kantong, backend menolak dengan `VAL-BD-066`. Pesannya
   tampil di dialog, dan dialog tetap terbuka supaya pilihan dapat dibetulkan.
5. Bila berhasil, baris **Diberikan** bertanda **Jalur darurat**, dan bagian **Otorisasi Jalur
   Darurat** menampilkan waktu, peran, gerbang, alasan, dan keadaan yang tercatat.

### 2.4 Jalur tidak normal lainnya

| Keadaan | Yang terjadi |
| --- | --- |
| Petugas tidak memegang butir hak akses | Tombolnya tidak tampil. Tidak ditampilkan lalu ditolak `403` |
| Kantong bukan Dialokasikan | Ketiga tombol tidak tampil, karena backend tidak menawarkan aksinya |
| Kantong diubah petugas lain saat dialog terbuka (`409`) | Dialog ditutup, pesan backend tampil, detail dimuat ulang |
| Belum ada alasan darurat aktif | Dialog darurat menampilkan pesan dan tombol konfirmasi tertahan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Backend: `BbkBloodUnitController.cs`, `BbkBloodUnitService.cs` (gerbang pemberian, bypass darurat,
  `AvailableActionsFor`, pemetaan label), `CompatibilityEvidenceDtos.cs`,
  `EmergencyAuthorizationDtos.cs`, `BloodUnitDtos.cs`, enum `BbkCompatibilityResult`,
  `BbkEmergencyBypassScope`, `BbkEmergencyAuthorizerRole`, `MstBloodBankReason` (kategori `Emergency`),
  `Program.cs` (nama cookie JWT).
- Blueprint: kartu `FE-BD-005`, `03-frontend-architecture.md` §3 `FE-BD-05`/`FE-BD-06`,
  `api-contract.md` bagian Blood Unit, `validation-matrix.md`.
- Frontend: detail kantong, `use-blood-unit-detail.jsx`, `use-blood-unit-storage.jsx`, service, constants,
  utils, CSS module, spec e2e `FE-BD-004`/`FE-BD-012`, dan pola tanggal+jam
  `emergency-assessment-date-time-field.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../blood-unit-constants.jsx` | Tiga nama aksi `AvailableActions`; cerminan tiga enum beserta label backend; kategori alasan `Emergency`; batas 500 karakter keterangan darurat; salinan deskripsi hero |
| `src/lib/services/.../blood-unit.service.js` | `recordBloodUnitCompatibilityEvidence`, `issueBloodUnit`, `emergencyIssueBloodUnit` |
| `src/utils/.../blood-unit-utils.js` | Fungsi murni: tanggal+jam lokal → ISO UTC, isi permintaan bukti dan darurat (tanpa nilai bawaan, enum di luar kontrak dikirim `null`), normalisasi daftar bukti/otorisasi, ringkasan hasil dari data backend |
| `src/lib/hooks/.../use-blood-unit-issuance.jsx` | **Baru.** Gerbang tampil tombol (`AvailableActions` + hak akses), tiga dialog, kirim, penanganan `409`/`422`, penahan tombol Berikan sesudah vonis backend |
| `src/lib/hooks/.../use-blood-unit-detail.jsx` | Merangkai hook baru; membuka daftar bukti dan otorisasi darurat; Muat ulang melepas penahan; `handleStorageResult` → `handleActionResult` karena kini dipakai dua hook |
| `src/components/view/.../blood-unit-detail-view.jsx` | Tiga tombol di kepala layar; peringatan **Pemberian ditahan**; baris **Diberikan** + penanda jalur; bagian **Bukti Kecocokan** dan **Otorisasi Jalur Darurat** |
| `src/components/view/.../blood-unit-issuance-dialogs.jsx` | **Baru.** Tiga `ConfirmModal`: bukti kecocokan, pemberian, jalur darurat. **Perbaikan 24 September 2026:** keempat isian `FilterSelect` dibungkus `<div>`, bukan `<label>`. Judulnya menjadi `<label htmlFor>` yang ditautkan ke `id` tombol pemicu, sehingga nama aksesibel tombol tetap judul isian |
| `src/style/.../blood-unit.module.css` | `.dateTimeRow` (tanggal dan jam berdampingan, bertumpuk di layar sempit) dan `.formHint`; token saja. Selektor judul isian `.formField > span` diperluas ke `.formField > label` supaya judul tetap sama rupanya sesudah perbaikan pembungkus |
| `tests/unit/blood-unit-issuance.test.mjs` | **Baru.** 12 test untuk fungsi murni |
| `tests/e2e/blood-unit-issuance-screen.spec.mjs` | **Baru.** Skenario runtime `R0`–`R9` terhadap backend sungguhan; alamat backend dan cookie dibaca dari environment; tanpa keduanya spec dilewati. Fixture ditemukan dari backend saat run; kantong B dapat disematkan lewat `FE005_UNIT_B`. **14 dari 14 skenario `PASS`** (bagian 6.2) |

Nol route baru, nol butir menu baru, nol Redux slice, nol perubahan base component.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi sesuai `rules/frontend/frontend-architecture.md`:
  view → hook → service → `InstanceAxios`. Transformasi ada di `utils`, konfigurasi di `constants`.
  View tidak memanggil Axios.
- Pola hook mengikuti `useBloodUnitStorage` (`FE-BD-012`): hook anak dirangkai `useBloodUnitDetail`,
  keputusan hak akses ketat `useEffectivePermissions`, dan kunci pengiriman ganda.
- **Nol logika bisnis baru di frontend.** Pemeriksaan statik pada source yang diubah menemukan nol
  pembandingan status kantong, nol perhitungan masa berlaku, dan nol penilaian keaktifan lokasi.
  Penanda **Lokasi nonaktif** hanya menampilkan flag backend `isCurrentStorageLocationActive`. Penahan
  tombol Berikan hanya mengingat vonis `422` backend untuk versi kantong yang sama.

#### Gerbang keputusan base component

`UI GATE: 9 elemen — REUSE 7, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol Catat Bukti / Berikan / Jalur Darurat | `BaseButton` | varian `secondary`/`primary`/`warning` ada di `base-button.jsx` | REUSE | Jalur Darurat memakai `warning` |
| Dialog bukti, pemberian, darurat | `ConfirmModal` | `children`, `variant="warning"` dipakai modul billing | REUSE | — |
| Pilihan hasil, alasan, gerbang, peran | `FilterSelect` | dipakai dialog alokasi `FE-BD-004` | REUSE | Opsi dari constants |
| Waktu pemeriksaan (tanggal + jam) | `FilterDatePicker` + `FilterTimePicker` | dirangkai di `emergency-assessment-date-time-field.jsx` | COMPOSE | Dirangkai langsung di dialog |
| Keterangan kondisi darurat | `textarea` bergaya modul | pola `FE-BD-012` di `blood-unit.module.css` | COMPOSE | Pakai kelas yang sudah ada |
| Tabel bukti dan otorisasi | `BaseDetailSection` + `DataTable` | detail kantong existing | REUSE | — |
| Hasil / jalur / keadaan | `StatusBadge` | detail kantong existing | REUSE | — |
| Pesan vonis backend | `InformationAlert` | detail kantong existing | REUSE | — |
| Baris Diberikan | `BaseDetailView` `detailRows` | detail kantong existing | REUSE | `hidden` bila belum diberikan |

**Keputusan COMPOSE — waktu pemeriksaan.** A. Rangkai `FilterDatePicker` + `FilterTimePicker`,
seperti asesmen IGD — **Rekomendasi**, dijalankan. Pemilih yang sama dengan layar lain, nol perubahan
base. B. `input type="datetime-local"` bawaan browser: rupanya berbeda per browser dan menyimpang dari
design system.

**Keputusan COMPOSE — keterangan darurat.** A. Kelas `textarea` yang sudah dipakai dialog penyimpanan
`FE-BD-012` — **Rekomendasi**, dijalankan. Konsisten di dalam layar yang sama, nol CSS baru.
B. `BaseFormControl` bertipe textarea: tampilannya berbeda dari keterangan penempatan pada dialog
sebelahnya.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel "Memuat bukti kecocokan..."; tombol pemicu jalur darurat berlabel "Menyiapkan..." saat alasan dimuat; tombol konfirmasi dalam keadaan memuat saat dikirim |
| Kosong | "Belum ada hasil pemeriksaan kecocokan yang tercatat pada kantong ini." Bagian Otorisasi Jalur Darurat tidak tampil bila belum ada otorisasi. Alasan darurat kosong → "Belum ada alasan aktif untuk jalur darurat." |
| Gagal | Pesan backend apa adanya: di dialog untuk galat masukan, di kepala layar ("Pemberian ditahan") untuk penolakan gerbang pemberian. `409` → dialog ditutup dan detail dimuat ulang |
| Tanpa hak akses | Tombol tidak tampil (`03-frontend-architecture.md` §4) |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Blood Bank Management / Blood Unit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/blood-bank-management/blood-units/{id}` | Detail, `AvailableActions`, `CompatibilityEvidences`, `EmergencyAuthorizations`, `IssuedAt`, `IssuedViaEmergency`, `Version` | `BloodUnit : Read` |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/compatibility-evidence` | Catat bukti. Isi: `evidenceResult` (0/1), `checkedAt` (UTC), `version` | `BloodUnit : Compatibility` |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/issue` | Berikan jalur normal. Isi: `version` | `BloodUnit : Issue` |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/emergency-issue` | Jalur darurat. Isi: `reasonCode`, `bypassScope`, `authorizerRole`, `emergencyConditionNote`, `version` | `BloodUnit : EmergencyIssue` |

#### Health Services / Master Data / Blood Bank Reason

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/blood-bank-reasons/options?category=Emergency` | Pilihan alasan darurat | `BloodBankReason : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Kode keluar `0` (dijalankan dua kali, terakhir atas permintaan pemilik) | `PASS` | Log perintah. `npx eslint` pada berkas yang diubah: 0 error. Satu warning `react-hooks/exhaustive-deps` di `blood-unit-detail-view.jsx` sudah ada di `HEAD` (dibuktikan dengan lint `git show HEAD:<berkas>`). Dua warning `set-state-in-effect` di `use-blood-unit-detail.jsx` ada pada baris yang tidak diubah |
| `npx eslint --quiet tests/e2e/blood-unit-issuance-screen.spec.mjs` | Kode keluar `0` | `PASS` | Log perintah |
| `npm run test:unit` | 1595 test — **1588 lulus, 7 gagal**. Ke-12 test baru lulus | `PASS` untuk cakupan task | Ketujuh kegagalan sama dengan baseline `FE-BD-012` (1583 test, 7 gagal): `route, menu, dan store terdaftar`, empat `FE-RWI-042`, `FE-RWI-043`, `M0` menu Setup Bank Darah. Nol di berkas task ini |
| `npm run build` | Kode keluar `0`, `Compiled successfully in 39.8s`, 370 halaman, standalone siap (dijalankan dua kali) | `PASS` | `/blood-units` dan `/blood-units/[slug]` terdaftar |
| Grep anti-regresi `ui-consistency-checklist.md` §G | Nol temuan: warna literal, typography, `<button>` mentah, `<table>`, utility Bootstrap, `!important`, inline style | `PASS` | Log perintah |
| `npx playwright test tests/e2e/blood-unit-issuance-screen.spec.mjs --workers=1` (run 2, lengkap) | **13 passed, 1 failed** (5,8 menit). `R8–R9` timeout — cacat UX, bagian 6.2 | `NEW ERROR` untuk `R9`; `PASS` untuk sisanya | Bagian 6.2 |
| `npx playwright test … --grep "R0:\|R7:\|R8–R9"` (run 3, kantong B disematkan) | **3 passed** (14,5 detik) | `PASS` | Bagian 6.2 |
| Validasi runtime `R0`–`R9` gabungan | **14 dari 14 skenario `PASS`** terhadap backend sungguhan | `PASS` | Bagian 6.2 |
| Sesudah perbaikan pemilih: `npm run lint:errors` | Kode keluar `0` | `PASS` | Bagian 6.3 |
| Sesudah perbaikan pemilih: `npm run build` | Kode keluar `0`, `Compiled successfully in 39.0s`, 370 halaman, standalone siap | `PASS` | Bagian 6.3 |
| Sesudah perbaikan pemilih: `npx playwright test … --grep "R0:\|R3:\|R8–R9"` (run 4) | **3 passed** (22,2 detik), **tanpa** klik bantuan; setiap pemilihan dibuktikan menutup daftarnya sendiri | `PASS` | Bagian 6.3 |

`AUTOMATED TEST: npm run test:unit — PASS` (12 test baru lulus; 7 kegagalan lain sudah ada sebelumnya).

`MANUAL TEST: PASS` — 14 skenario di Chromium sungguhan terhadap backend sungguhan (bagian 6.2). Cacat UX pemilih dialog yang ditemukan sudah diperbaiki dan dibuktikan ulang pada `R3` dan `R9` (bagian 6.3).

### 6.1 Skenario runtime

Spec `tests/e2e/blood-unit-issuance-screen.spec.mjs` mengikuti pola `FE-BD-004` dan `FE-BD-012`.
Hasil build standalone dijalankan pada `http://127.0.0.1:3710` dan diuji lewat Playwright
Chromium. Setiap request `/v1/**` diteruskan (`route.fetch`) ke **backend sungguhan** milik pemilik:
`QuilvianSystemBackend` Debug di `https://localhost:7184`, database `QuilvianNewDevSukma`, sesi
`superadmin`. **Jawaban bisnis backend tidak pernah dikarang.** Satu-satunya jawaban yang dipasang
adalah daftar kewenangan pada `R2a`–`R2f`, supaya ketiadaan satu butir dapat dibuktikan tanpa
membuat akun baru. Angka pembanding diambil langsung dari backend di dalam test.

### 6.2 Hasil validasi runtime — 24 September 2026

**Fixture yang ditemukan backend (`R0`).** Kantong A `TEST-BD008-20260916125637-03`, kantong B
`TEST-BD009-20260917100009-04` (lokasi `TBD009-LOCA`), kantong Tersedia
`TEST-BD006-20260914094559-03`, dan alasan darurat `TBD008-DARURAT`. Pemilihannya dibatasi pada
kantong yang komponennya punya masa berlaku bukti dan belum punya bukti apa pun. Pengintaian awal
menemukan bahwa kantong `TEST-BD006-*` memakai komponen `TBD006-PRC` **tanpa** masa berlaku.
Backend menolaknya lebih dulu dengan `VAL-BD-020b`, sehingga `VAL-BD-079` maupun pemberian normal
tidak akan pernah dapat dibuktikan pada kantong itu.

| Kode | Skenario | Hasil |
| --- | --- | --- |
| `R0` | Fixture dari backend: 2 kantong `TEST-` Dialokasikan, 1 Tersedia, 1 alasan `Emergency` aktif | `PASS` |
| `R1` | Hak penuh: **Catat Bukti Kecocokan**, **Berikan**, dan **Jalur Darurat** tampil. Kelas Jalur Darurat memuat `warning` dan berbeda dari Berikan. Baris Diberikan tidak tampil | `PASS` |
| `R2a` | Kewenangan penuh (daftar dipersempit, `isSuperAdmin: false`) → ketiga tombol ada | `PASS` |
| `R2b` | Tanpa `BloodUnit : Compatibility` → Catat Bukti Kecocokan tidak ada; dua lainnya ada | `PASS` |
| `R2c` | Tanpa `BloodUnit : Issue` → Berikan tidak ada | `PASS` |
| `R2d` | Tanpa `BloodUnit : EmergencyIssue` → Jalur Darurat tidak ada | `PASS` |
| `R2e` | Tanpa `BloodBankReason : Read` → Jalur Darurat tidak ada | `PASS` |
| `R2f` | Kantong Tersedia dengan kewenangan penuh → ketiga tombol tidak ada (sisi `AvailableActions`) | `PASS` |
| `R3` | Bukti **Tidak cocok** lewat layar → `POST /compatibility-evidence` `200`. Isi permintaan **tepat** `checkedAt`, `evidenceResult`, `version`: `evidenceResult` = `1`, `checkedAt` berakhiran `Z`, `version` = versi backend. Bukti terbaru di backend berlabel Tidak cocok dan tampil di tabel | `PASS` |
| `R4` | Berikan → `POST /issue` **`422`, `errors.code` `VAL-BD-079`**, isi hanya `version`. Dialog tertutup; judul **Pemberian ditahan** dan pesan backend tampil apa adanya; Berikan **nonaktif**. Sesudah **Muat ulang**, Berikan aktif kembali | `PASS` |
| `R5` | Dialog bukti terbuka; "petugas lain" mencatat bukti lewat API sungguhan → simpan dari layar mendapat **`409` dari backend**; dialog tertutup; detail dimuat ulang | `PASS` |
| `R6` | Bukti **Cocok** (`evidenceResult` = `0`) `200` → Berikan `200`. Backend: status **Diberikan**, `issuedViaEmergency` = `false`. Layar: **Jalur normal**, **Dipakai untuk pemberian**, dan ketiga tombol hilang | `PASS` |
| `R7` | Bukti Cocok bertanggal 400 hari lalu dicatat lewat API → Berikan **`422 VAL-BD-020`**; pesan backend tampil; Berikan nonaktif | `PASS` |
| `R8` | Lokasi `TBD009-LOCA` dinonaktifkan lewat API; penanda **Lokasi nonaktif** tampil → Berikan **`422 VAL-BD-065`**. Pesannya memuat "lokasi" dan tidak memuat "bukti" (`FE-BD-012`), tampil apa adanya, dan Berikan nonaktif | `PASS` |
| `R9` | **Jalur Darurat**: judul **Jalur tidak normal**, penolakan pemberian normal terakhir tampil, dan pemilih peran serta gerbang masih menampilkan placeholder (**tanpa nilai bawaan**). Pilih alasan, gerbang "Melewati bukti kecocokan", peran "Dokter penanggung jawab pasien", isi keadaan → **`422 VAL-BD-066`** (`bypassScope` = `0`), pesan tampil di dialog dan dialog tetap terbuka. Gerbang diganti "… dan lokasi penyimpanan tidak aktif" → **`200`**. Isi permintaan tepat `authorizerRole`, `bypassScope`, `emergencyConditionNote`, `reasonCode`, `version` (`bypassScope` `2`, `authorizerRole` `1`). Backend: **Diberikan**, `issuedViaEmergency` = `true`. Layar: **Jalur darurat**, bagian **Otorisasi Jalur Darurat** dengan peran dan gerbang dari backend | `PASS` |

**Riwayat percobaan, apa adanya.**

1. **Run 1** — 8 lulus (`R0`–`R2f`), `R3` gagal, sisanya tidak berjalan. Sebabnya **cacat spec**:
   `getByText("Pasien tujuan alokasi aktif")` juga cocok dengan paragraf pembuka dialog, karena
   pencocokan teks tanpa `exact`. Kegagalan terjadi **sebelum** tombol simpan ditekan, jadi nol
   kiriman tulis. Spec diperbaiki dengan `exact: true`.
2. **Run 2** (lengkap) — 13 lulus (`R0`–`R7`); `R8–R9` gagal karena **timeout 5 menit**. Bagian
   `R8` sudah lulus: penanda lokasi nonaktif, `422 VAL-BD-065`, dan Berikan nonaktif terlihat pada
   snapshot halaman. Penyebab macet di `R9` adalah **cacat UX produk**: pemilih yang dibungkus
   `<label>` terbuka lagi sesudah opsi dipilih, lalu menutupi pemilih berikutnya. Uji debug
   baca-saja membuktikan opsi berada di dalam `<label>` dan daftar tetap `aria-expanded=true`
   sesudah dipilih. Karena test ditutup paksa, blok `finally` gagal mengaktifkan kembali lokasi
   `TBD009-LOCA`. Lokasi itu **diaktifkan kembali segera secara manual** lewat
   `PATCH /status` (`200`, `isActive=true` terverifikasi). Kantong B tetap Dialokasikan tanpa
   otorisasi darurat.
3. **Perbaikan spec, bukan source** — atas instruksi pemilik "jangan ubah source". Sesudah memilih
   opsi, spec mengklik judul dialog, seperti yang dilakukan pengguna. Klik diberi batas 15 detik
   supaya `finally` tetap sempat berjalan. Alasan darurat memilih `TBD008-DARURAT`, alasan uji
   jalur darurat `BE-BD-008`; `TBD006-DARURAT` berteks "alasan kategori salah". Kantong B dapat
   disematkan lewat `FE005_UNIT_B`.
4. **Run 3** — `R0`, `R7`, dan `R8–R9` dengan kantong B disematkan ke
   `TEST-BD009-20260917100009-04`, yang keadaannya disiapkan run 2 (Dialokasikan, hanya bukti lama).
   **3 dari 3 lulus.** `R1`–`R6` tidak diulang: kantong A sudah Diberikan, source produk tidak
   berubah di antara run, dan perubahan spec hanya menyangkut cara menutup daftar pilihan.

**Keadaan database sesudah run** (diperiksa langsung ke backend):

| Kantong | Keadaan akhir |
| --- | --- |
| `TEST-BD008-20260916125637-03` | **Diberikan**, jalur normal; 3 bukti uji: Tidak cocok (`R3`), Tidak cocok (`R5` petugas lain), Cocok (`R6`, dipakai) |
| `TEST-BD009-20260917100009-04` | **Diberikan**, jalur darurat; 2 bukti Cocok bertanggal 400 hari lalu (`R7` run 2 dan run 3); 1 otorisasi darurat: melewati bukti dan lokasi, Dokter penanggung jawab pasien, alasan `TBD008-DARURAT` |
| `TEST-BD010-20260917135556-04` | Tidak disentuh (hanya dibaca `R0` run 3) |
| Lokasi `TBD009-LOCA` | **Aktif** kembali |

Data ini sengaja **dibiarkan** sesuai keputusan pemilik. Catatan log aplikasi backend dari aksi uji
tidak dihapus.

**Kebersihan sesudah run:** server standalone uji dihentikan; berkas cookie dan skrip bantu di
scratchpad dihapus; nol kemunculan token di `test-results/` dan `playwright-report/`; dua berkas
tracked di `test-results/` yang diubah Playwright dipulihkan dengan `git restore` pada kedua path
itu saja; spec debug sementara dihapus.

### 6.3 Perbaikan cacat pemilih dialog dan validasi ulang — 24 September 2026

**Keputusan pemilik:** perbaiki cacat pemilih, **hanya** markup pembungkus `FilterSelect`, tanpa
mengubah logika bisnis maupun kontrak API. Lalu jalankan `lint:errors`, `build`, dan ulangi `R3`
dan `R9`.

**Yang diubah.** Pada `blood-unit-issuance-dialogs.jsx`, keempat isian `FilterSelect` — hasil
pemeriksaan, alasan darurat, gerbang yang dilewati, dan diterbitkan sebagai — kini dibungkus
`<div className={styles.formField}>`. Judulnya menjadi `<label htmlFor={id}>`, dan `id` yang sama
dikirim ke `FilterSelect` untuk tombol pemicunya (`useId`). Isian textarea keadaan kedaruratan tetap
dibungkus `<label>`, karena textarea adalah kontrol bawaan yang tidak punya daftar pilihan. Pada CSS,
hanya selektor judul yang diperluas agar rupanya sama. Nol perubahan pada hook, service, utils,
constants, maupun isi permintaan.

**Kenapa ini memperbaiki cacatnya.** Opsi daftar tidak lagi berada di dalam `<label>`, sehingga
klik pada opsi tidak diteruskan browser ke tombol pemicu. Klik pada judul isian tetap membuka
pemilih lewat `htmlFor`.

**Pembuktian.** Klik bantuan pada judul dialog di spec **dibuang**. Fungsi `pilih` kini mewajibkan
`aria-expanded="true"` sesudah pemicu diklik, lalu `aria-expanded="false"` dan nol `listbox` sesudah
opsi dipilih. Syarat itu berlaku pada setiap pemilihan di `R3` (hasil pemeriksaan) dan `R9` (alasan,
gerbang dua kali, dan peran).

| Kode | Fixture run 4 | Hasil |
| --- | --- | --- |
| `R0` | Kantong A `TEST-BD010-20260917135556-04`; kantong B disematkan ke `TEST-BD007-20260914171359-09` (komponen `TBD007-V` 24 jam, bukti terbaru Cocok 16 September — sudah lewat masa berlaku), lokasi `TBD007-LOCY`; alasan `TBD008-DARURAT` | `PASS` |
| `R3` | Bukti **Tidak cocok** lewat pemilih yang menutup sendiri → `200`; isi tepat `checkedAt`/`evidenceResult`/`version` | `PASS` |
| `R8–R9` | `422 VAL-BD-065` menyebut lokasi → jalur darurat: tiga pemilih menutup sendiri → gerbang salah `422 VAL-BD-066` → gerbang benar `200`, **Jalur darurat**, bagian Otorisasi | `PASS` |

**Keadaan database sesudah run 4:** `TEST-BD010-20260917135556-04` tetap **Dialokasikan** dengan satu
bukti uji Tidak cocok (`R3`). `TEST-BD007-20260914171359-09` **Diberikan** lewat jalur darurat (melewati
bukti dan lokasi, Dokter penanggung jawab pasien). Lokasi `TBD007-LOCY` **aktif** kembali, diverifikasi
langsung ke backend. Seluruh data uji dibiarkan sesuai keputusan pemilik.

**Kebersihan:** server uji dihentikan; berkas cookie dan skrip bantu dihapus; nol kemunculan token di
artefak; dua berkas tracked di `test-results/` dipulihkan dengan `git restore`.

### 6.4 Lanjutan sesudah `BE-BD-021` — 25 September 2026

**Perintah pemilik `Sukmagp`, 25 September 2026.** `BE-BD-021` sudah ditutup dan di-push. Lanjutkan
penyelesaian `FE-BD-005` dengan empat butir:

1. Indikator gerbang pemberian sebelum Berikan ditekan: pesan backend, ditambah `validUntil` bila ada.
2. Tombol Berikan nonaktif bila `isOpen = false`.
3. Dialog Jalur Darurat memakai `evidenceGateClosed`/`locationGateClosed`, tanpa meminta petugas
   menebak.
4. Penanda konflik golongan darah pasien tujuan dari `GET /blood-group-exams/patient/{patientId}/valid`,
   memakai `isConflictHeld` dan pesan backend.

Larangan: mengubah backend, membuat endpoint baru, dan membuat aturan bisnis di frontend. Audit
read-only dilakukan lebih dulu, lalu implementasi.

#### 6.4.1 Yang berubah bagi petugas

1. **Sebelum Berikan ditekan.** Kantong Dialokasikan yang gerbangnya tertutup kini langsung
   menampilkan peringatan **Pemberian ditahan** berisi pesan backend, dan tombol **Berikan**
   nonaktif. Contoh `G2`: kantong `TEST-BD007-20260914171359-04` punya bukti Cocok dari 48 jam lalu,
   sedangkan komponennya berlaku 24 jam. Layar menampilkan "Bukti kecocokan sudah lewat masa berlaku.
   Diperlukan bukti kecocokan yang baru. **Bukti kecocokan berlaku sampai** …". Tanggalnya diambil dari
   `validUntil` backend (`2026-09-24T04:05:05Z`) dan hanya diformat ke waktu setempat; layar tidak
   membandingkannya.
2. **Gerbang terbuka.** Tidak ada peringatan dan Berikan aktif. Dialog Berikan menampilkan **Masa
   berlaku bukti kecocokan — Berlaku sampai …**.
3. **Jalur darurat.** Dialog menyebut gerbang yang dilewati menurut backend, misalnya "Menurut
   backend: lokasi penyimpanan tidak aktif." Pemilih **Gerbang yang Dilewati** terisi dari proyeksi
   dan tidak dapat diubah, dan keterangannya menyebut cakupan yang ditetapkan. Bila kedua gerbang
   terbuka, dialog menyatakan tidak ada gerbang yang perlu dilewati. Tombol konfirmasi tidak dapat
   ditekan karena tidak ada cakupan yang sah (tabel kontrak `D7`).
4. **Konflik golongan darah pasien tujuan.** Bila backend menyatakan `isConflictHeld = true`,
   peringatan **Golongan darah pasien tujuan bertentangan** tampil dengan pesan backend. Layar tidak
   menahan tombol, karena backend tidak menjadikan konflik ini gerbang pemberian. Tanpa hak
   `BloodGroupExam : Read`, golongan darah tidak diambil dan penanda tidak tampil.
5. **Cadangan.** Proyeksi adalah potret saat detail dibaca. Penahan sesudah vonis `422` dari
   pekerjaan 24 September tetap ada untuk keadaan yang berubah sesudah detail dibaca. Peringatannya
   tidak tampil dua kali bila proyeksi sudah menahan.

#### 6.4.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/.../blood-unit-utils.js` | Empat fungsi baru. `normalizeIssuanceGate` dan `normalizeEmergencyBypass` membaca proyeksi apa adanya (`null` bila tidak dikirim). `bypassScopeFromProjection` memetakan dua boolean ke enum menurut tabel tetap `D7`. `bypassScopeLabel` mengambil label cakupan |
| `src/lib/hooks/.../use-blood-unit-issuance.jsx` | Menerima `issuanceGate` dan `emergencyBypass`. `issueHeld` bernilai benar bila gerbang tertutup **atau** ada vonis `422` pada versi yang sama. Cakupan darurat diisi dari proyeksi dan dikunci (`bypassScopeLocked`); pemilih bebas hanya muncul bila backend tidak mengirim proyeksi |
| `src/lib/hooks/.../use-blood-unit-detail.jsx` | Membaca kedua proyeksi dari detail. Mengambil golongan darah sah **pasien tujuan alokasi aktif** lewat `getValidBloodGroup` yang sudah ada (`FE-BD-009`), dijaga `BloodGroupExam : Read`. Jawaban hanya dipakai untuk pasien yang sama |
| `src/components/view/.../blood-unit-detail-view.jsx` | Peringatan **Pemberian ditahan** dari proyeksi (pesan + `validUntil`) dan penanda konflik golongan darah. Vonis `422` tidak ditampilkan dua kali |
| `src/components/view/.../blood-unit-issuance-dialogs.jsx` | Masa berlaku bukti pada dialog Berikan. Keadaan gerbang dan pemilih cakupan terkunci pada dialog darurat |
| `tests/unit/blood-unit-issuance.test.mjs` | 5 test baru untuk fungsi murni di atas |
| `tests/e2e/blood-unit-gate-projection-screen.spec.mjs` | **Baru.** Skenario runtime `G0`–`G9` terhadap backend sungguhan |

Nol route, menu, Redux, service baru, CSS, dan perubahan base component. **Nol logika bisnis baru:**
layar tidak membandingkan tanggal dan tidak menilai status, lokasi, bukti, maupun golongan darah.
Satu-satunya pemetaan adalah tabel tetap kontrak `D7`, dari dua boolean ke enum.

`UI GATE: 5 elemen — REUSE 5, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Peringatan gerbang pemberian | `InformationAlert` | Sudah dipakai untuk "Pemberian ditahan" di layar ini | REUSE | `variant="warning"` |
| Tombol Berikan tertahan | `BaseButton` | Prop `disabled`/`title` sudah dipakai | REUSE | — |
| Gerbang darurat terkunci | `FilterSelect` + `InformationAlert` | Prop `disabled` `FilterSelect`; alert di dialog yang sama | REUSE | Pemilih tetap tampil agar nilai yang dikirim terbaca |
| Penanda konflik golongan darah | `InformationAlert` | Pola `ValidBloodGroupNotice` pada detail `FE-BD-06` | REUSE | Judul dan pesan backend |
| Batas berlaku bukti | `formatDateTimeId` | Helper tanggal yang sama pada tabel bukti | REUSE | — |

#### 6.4.3 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Kode keluar `0` | `PASS` | Warning per berkas dibandingkan dengan `HEAD`. `use-blood-unit-detail.jsx`: 2 warning `set-state-in-effect` lama, sama dengan `HEAD`. `blood-unit-detail-view.jsx`: 1 warning `exhaustive-deps` lama. Tiga warning baru yang sempat muncul dari baris task (`set-state-in-effect` dan dua `preserve-manual-memoization`) **dihilangkan** sebelum validasi akhir |
| `npx eslint --quiet tests/e2e/blood-unit-gate-projection-screen.spec.mjs` | Kode keluar `0` | `PASS` | Log perintah |
| `npm run test:unit` | 1624 test — **1617 lulus, 7 gagal**; ke-5 test baru lulus | `PASS` untuk cakupan task | Ketujuh kegagalan sama dengan baseline `FE-BD-005`/`FE-BD-009`: `route, menu, dan store terdaftar`, empat `FE-RWI-042`, `FE-RWI-043`, dan `M0` menu Setup Bank Darah |
| `npm run build` | Kode keluar `0`, `Compiled successfully in 46s`, 371 halaman, standalone siap | `PASS` | Log perintah |
| Grep anti-regresi `ui-consistency-checklist.md` §G pada baris tambahan | Nol temuan | `PASS` | Nol berkas CSS berubah |
| Runtime `G0`–`G9` (run 2) | **10 dari 10 `PASS`** (59 detik) | `PASS` | Bagian 6.4.4 |

`AUTOMATED TEST: npm run test:unit — PASS` (5 test baru lulus; 7 kegagalan lain sudah ada sebelumnya).

`MANUAL TEST: PASS` — skenario `G0`–`G9` dijalankan di Chromium sungguhan terhadap backend sungguhan.

#### 6.4.4 Hasil validasi runtime — 25 September 2026

Build standalone dijalankan pada `http://127.0.0.1:3710`. Setiap request `/v1/**` diteruskan ke
**backend sungguhan** hasil build `BE-BD-021` pada `http://localhost:5217`, dengan database
`QuilvianNewDevSukma`. Sesinya `superadmin` dari login seed; cookie disimpan sementara di scratchpad,
tidak dicetak, lalu dihapus. Jawaban yang dipasang hanya dua: daftar kewenangan pada `G8`, dan **satu
jawaban tersimulasi pada `G7`**.

| Kode | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| `G0` | Fixture dan proyeksi awal dari backend: `…BD006-…-02` `VAL-BD-018`; `…BD007-…-04` `VAL-BD-019` (bukti tertutup, lokasi terbuka); `…BD010-…-04` Dialokasikan; `…BD007-…-01` Diberikan tanpa proyeksi | Sesuai | `PASS` |
| `G1` | `VAL-BD-018` | **Pemberian ditahan** dan pesan backend tampil saat halaman dibuka, tanpa "berlaku sampai". Berikan **nonaktif**; nol `POST /issue` | `PASS` |
| `G2` | Bukti Cocok 48 jam lalu dicatat lewat API pada `…BD007-…-04` → `VAL-BD-020`, `validUntil` terisi | Pesan backend dan "Bukti kecocokan berlaku sampai …" tampil. Berikan nonaktif; nol `POST /issue` | `PASS` |
| `G3` | Bukti Cocok 1 jam lalu pada `…BD010-…-04` → gerbang terbuka | Tanpa peringatan; Berikan **aktif**. Dialog Berikan menampilkan masa berlaku, lalu dibatalkan; nol `POST /issue` | `PASS` |
| `G6` | Pasien tujuan tanpa konflik (data sungguhan) | `/valid` dipanggil **hanya** untuk pasien alokasi aktif: `200`, `isConflictHeld = false`. Penanda **tidak** tampil | `PASS` |
| `G7` | **SIMULASI** jawaban `/valid` dengan `isConflictHeld = true` | Penanda **Golongan darah pasien tujuan bertentangan** dan pesannya tampil. Berikan **tetap aktif**, karena layar tidak menahan | `PASS` (simulasi — bukan bukti perilaku backend) |
| `G8` | Tanpa `BloodGroupExam : Read` | Nol request `/valid`; penanda tidak tampil | `PASS` |
| `G4` | Lokasi `TBD010-LOCA` dinonaktifkan lewat API → `VAL-BD-065`, bypass lokasi saja | Pesan lokasi dari backend tampil dan Berikan nonaktif. Dialog darurat menampilkan "Menurut backend: lokasi penyimpanan tidak aktif.", pemilih cakupan **nonaktif**, dan keterangan "Ditetapkan … Melewati lokasi penyimpanan tidak aktif."; dialog dibatalkan. Lokasi **dipulihkan** (`200`) di blok `finally` | `PASS` |
| `G5` | `…BD006-…-04` (bukti tertutup, lokasi terbuka): jalur darurat dari layar | Dialog menyebut gerbang bukti dan pemilih cakupan nonaktif. Petugas hanya memilih alasan, peran, dan keadaan → `POST /emergency-issue` **`200`** dengan `bypassScope = 0` pada isi permintaan. Backend: **Diberikan**, `issuedViaEmergency = true`, proyeksi `null` | `PASS` |
| `G9` | Kantong Diberikan | Tanpa peringatan gerbang, tanpa tombol Berikan dan Jalur Darurat | `PASS` |

**Riwayat percobaan, apa adanya.** Run 1 berhenti di `G2` (2 lulus, 1 gagal) karena **ekspektasi spec
yang keliru**. Bukti Cocok 48 jam lalu dicatat pada `…BD010-…-04`, padahal kantong itu sudah punya bukti
Tidak cocok yang lebih baru (24 September 09.11 UTC), sehingga backend benar menjawab `VAL-BD-079`.
Skenario itu dipindahkan ke `…BD007-…-04`, yang pasien tujuannya belum punya bukti. Run 2 berjalan
lengkap, 10 dari 10 `PASS`. Source produk tidak berubah di antara kedua run.

**Keadaan database sesudah run** (diperiksa langsung):

| Kantong | Keadaan akhir |
| --- | --- |
| `TEST-BD006-20260914094559-04` | **Diberikan jalur darurat** (`G5`), dengan satu otorisasi berlingkup bukti kecocokan |
| `TEST-BD007-20260914171359-04` | Dialokasikan; +1 bukti Cocok bertanggal 48 jam lalu (`G2`) |
| `TEST-BD010-20260917135556-04` | Dialokasikan; +2 bukti Cocok: 48 jam lalu (run 1, bukan bukti terbaru) dan 1 jam lalu (`G3`). Gerbangnya kini terbuka |
| `TEST-BD006-20260914094559-02`, `TEST-BD007-20260914171359-01` | Tidak berubah |
| Lokasi `TBD010-LOCA` | **Aktif** kembali |

Data uji dibiarkan dan dicatat, sama dengan perlakuan data uji sebelumnya.

**Kebersihan:** kedua server uji dihentikan dan berkas cookie dihapus. Nol kemunculan token di
`test-results/` dan `playwright-report/`. Dua berkas tracked di `test-results/` yang diubah Playwright
dipulihkan dengan `git restore`, hanya pada kedua path itu.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Outcome: petugas mencatat **golongan darah** | **Belum terpenuhi** — di luar cakupan keputusan pemilik no. 1–2 | Layar `FE-BD-06` tidak dibangun; butir menu Pemeriksaan Golongan Darah belum terdaftar |
| Outcome: petugas mencatat bukti kecocokan beserta hasilnya, lalu memberikan kantong | **Terpenuhi** | `R3`, `R6` |
| `FE-BD-021` hasil tidak cocok **menutup tombol Berikan** dengan pesan yang benar | **Terpenuhi (25 September 2026)** | Sejak `BE-BD-021`: tombol tertutup **sebelum** ditekan untuk setiap `isOpen = false`, dengan pesan backend (`G1`, `G2`, `G4`). Kode `VAL-BD-079` sendiri tidak ditembakkan di layar pada run ini; jalurnya identik (penahan membaca `isOpen`, bukan kode), dan proyeksi `079` terbukti di backend (`BE-BD-021` R4). **Riwayat (24 September 2026):** **Sebagian** — `R4`: tombol tertutup dengan pesan `VAL-BD-079` backend **sesudah** percobaan pertama ditolak. Sebelum itu tombol tetap terbuka, karena gerbang tidak diekspos backend dan frontend dilarang menilainya (keputusan no. 3) |
| `FE-BD-018` peran penerbit dipilih sendiri | **Terpenuhi** | `R9`: tanpa nilai bawaan dan tidak disimpulkan dari akun; isi permintaan membawa peran yang dipilih. Unit test "gerbang dan peran penerbit yang tidak dipilih dikirim null" |
| `FE-BD-013` gerbang yang dilewati mencerminkan keadaan kantong, bukan pilihan bebas | **Terpenuhi (25 September 2026)** | `G4` (lokasi saja) dan `G5` (bukti saja, diterima backend `200` dengan `bypassScope = 0`): pemilih terisi dari proyeksi dan nonaktif. **Riwayat (24 September 2026):** **Belum terpenuhi** — Pilihan tidak diisi otomatis: keadaan gerbang tidak ada di `GET /{id}`, dan keputusan no. 3 melarang menghitungnya. Backend menolak pilihan yang salah (`R9`, `422 VAL-BD-066`), dan penolakan itu tampil di dialog |
| `FE-BD-007` penanda konflik golongan darah terlihat dan menahan | **Sebagian (25 September 2026)** | **Terlihat:** penanda dan pesan backend tampil bila `isConflictHeld` (`G7`, jawaban **tersimulasi**; data sungguhan tanpa konflik `G6`; tanpa hak baca `G8`). **Menahan: belum** — backend tidak menahan pemberian karena konflik golongan darah (`VAL-BD-034`), dan layar dilarang membuat aturan sendiri. **Riwayat (24 September 2026):** **Belum terpenuhi** — Tidak dibangun (keputusan no. 3). Backend juga tidak punya gerbang golongan darah pada kantong |
| `FE-BD-008` penanda bukti kedaluwarsa terlihat **sebelum** Berikan ditekan | **Terpenuhi (25 September 2026)** | `G2`: pesan `VAL-BD-020` dan batas berlaku dari `validUntil` tampil saat halaman dibuka; Berikan nonaktif. **Riwayat (24 September 2026):** **Belum terpenuhi** — Backend tidak memulangkan masa berlaku. Sesudah ditekan, pesan `VAL-BD-020` tampil dan tombol tertahan (`R7`) |
| `FE-BD-012` penolakan lokasi nonaktif menyebut lokasi | **Terpenuhi** | `R8`: `422 VAL-BD-065`, pesan memuat "lokasi" dan tidak memuat "bukti", tampil apa adanya |
| `FE-BD-005` jalur darurat tampil jelas sebagai jalur tidak normal | **Terpenuhi** | `R1`: tombol `warning` terpisah dari Berikan. `R9`: dialog `warning`, peringatan **Jalur tidak normal**, penanda **Jalur darurat**, bagian Otorisasi |
| Gerbang hak akses (keputusan pemilik no. 5) | **Terpenuhi** | `R2a`–`R2f` |
| Penanganan galat backend (keputusan pemilik no. 5) | **Terpenuhi** | `R4` `422`, `R5` `409`, `R7` `422`, `R8` `422`, `R9` `422` — kode dan pesan asli backend |
| Nol logika bisnis di frontend (keputusan pemilik no. 3) | **Terpenuhi** | Pemeriksaan statik bagian 3.3; status, jalur, dan penolakan dibaca dari backend (`R4`, `R6`–`R9`) |
| DoD kartu | `NOT APPLICABLE` | Kartu `FE-BD-005` tidak memuat baris DoD |
| DoD pemilik: `lint:errors` + `build` | **Terpenuhi** | Bagian 6 |
| DoD pemilik: validasi runtime | **Terpenuhi** | Bagian 6.2 — 14 dari 14 skenario `PASS` |
| DoD pemilik: laporan + roadmap | **Terpenuhi** | Laporan ini; roadmap dan traceability ditandai 🟡 |

**Ringkasan — 25 September 2026:** dari 9 butir, **7 terpenuhi** (outcome bukti dan pemberian, `FE-BD-021`, `FE-BD-018`, `FE-BD-013`, `FE-BD-008`, `FE-BD-012`, `FE-BD-005`), **1 sebagian** (`FE-BD-007`: terlihat, belum menahan), dan **1 belum terpenuhi** (outcome mencatat golongan darah, di luar cakupan keputusan pemilik). Task tetap **🟡**.

**Riwayat ringkasan (24 September 2026):** dari 9 butir outcome dan acceptance kartu, **4 terpenuhi** (outcome bukti dan
pemberian, `FE-BD-018`, `FE-BD-012`, `FE-BD-005`), **1 sebagian** (`FE-BD-021`), dan **4 belum
terpenuhi** (golongan darah, `FE-BD-013`, `FE-BD-007`, `FE-BD-008`). Keempat butir terakhir tertahan
data backend yang belum ada dan keputusan pemilik agar frontend tidak menghitungnya. Karena itu task
ini tetap **🟡**, walaupun seluruh DoD pemilik terpenuhi dan runtime `PASS`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **25 September 2026:** `git fetch --dry-run` sempat dijalankan untuk memeriksa upstream frontend — tidak mengubah ref lokal, tetapi di luar izin `AGENTS.md`; tidak diulang. **Riwayat:** **Token sesi `superadmin` backend lokal ditempel pemilik di percakapan agent.** Token itu disimpan sementara di berkas scratchpad, tidak pernah dicetak ke log atau berkas lain (nol kemunculan di artefak Playwright), dan berkasnya sudah dihapus. Token tetap berlaku sampai kedaluwarsa bawaannya, jadi disarankan logout atau rotasi sesi `superadmin` lokal. Pemberian bersifat akhir: dua kantong `TEST-` kini Diberikan di `QuilvianNewDevSukma` (keputusan pemilik; bagian 6.2) |
| Masalah yang diketahui | **25 September 2026:** backlog proyeksi gerbang pemberian **ditutup** oleh `BE-BD-021` ✅ dan dipakai layar (bagian 6.4). Tersisa: (a) gerbang golongan darah `VAL-BD-034` tidak ditegakkan backend pada kantong, sehingga penanda konflik hanya terlihat dan tidak menahan; (b) penanda konflik positif baru dibuktikan dengan jawaban tersimulasi, karena `QuilvianNewDevSukma` tidak punya pasien yang menahan konflik. **Riwayat (24 September 2026):** **Diperbaiki — pemilih dialog terbuka lagi sesudah dipilih.** Ditemukan runtime run 2 dan diperbaiki atas keputusan pemilik (bagian 6.3). **Di luar cakupan:** dialog alokasi, pembatalan alokasi, dan penyimpanan lokasi milik `FE-BD-004`/`FE-BD-012` di `blood-unit-detail-view.jsx` masih memakai pola `<label>` + `FilterSelect` yang sama, sehingga besar kemungkinan cacat yang sama ada di sana. Tidak disentuh task ini. **BACKLOG (keputusan pemilik 24 September 2026: dicatat, tidak dibuka sebagai task) — proyeksi gerbang pemberian.** Untuk memenuhi `FE-BD-008`, `FE-BD-013`, dan `FE-BD-021` sebelum tombol ditekan, `GET /blood-units/{id}` perlu memulangkan hasil fungsi service yang sudah ada, misalnya `issuanceGate { isOpen, validationCode, message, validUntil }` dan `emergencyBypass { evidenceGateClosed, locationGateClosed }`. Aditif, tanpa aturan baru. **GAP BACKEND — gerbang golongan darah.** `VAL-BD-034` tidak ditegakkan pada kantong; butuh keputusan klinis sebelum `FE-BD-007` dapat menahan tombol. **Data uji:** komponen `TBD006-PRC` dan `TBD007-N` belum punya masa berlaku bukti, sehingga kantongnya selalu ditolak `VAL-BD-020b`. **Nama pelaku** pada bukti dan otorisasi hanya GUID, jadi tidak ditampilkan (sama dengan `placedByName` pada `FE-BD-012`). **Dokumen:** kartu masih menulis kontrak `v4` dan tidak punya baris DoD — di luar wewenang tulis roadmap task ini |
| Dependency backend | **25 September 2026:** `BE-BD-021` ✅ terpakai. Yang tersisa untuk ✅ penuh: keputusan klinis dan task backend gerbang golongan darah (`VAL-BD-034`), serta keputusan cakupan pencatatan golongan darah pada layar `FE-BD-06`. **Riwayat:** Nihil yang tertunda untuk cakupan yang diputuskan. Proyeksi gerbang pemberian dan gerbang golongan darah tercatat sebagai **backlog**; tidak dibuka sebagai task atas keputusan pemilik 24 September 2026 |
| Perubahan sampingan | Dua berkas tracked di `test-results/` berubah pada setiap run Playwright (`.last-run.json` dimodifikasi; `…inpatient-admiss-…/error-context.md` terhapus). Keduanya bersih saat task dimulai dan dipulihkan dengan `git restore` sesudah setiap run. Spec debug sementara `tests/e2e/zz-debug-fe-bd-005.spec.mjs` dibuat lalu dihapus |
| Interupsi | Validasi runtime tertunda dua kali karena cookie sesi belum tersedia, lalu dilanjutkan sesudah pemilik memberikannya. Satu run berakhir timeout dan meninggalkan lokasi uji nonaktif; lokasi itu dipulihkan dan diverifikasi sebelum run berikutnya |
| Status Git | **25 September 2026:** frontend `sukmagpV2` — 6 berkas diubah + 1 spec baru, **belum di-commit**; backend — laporan ini, `frontend-roadmap.md`, dan `requirement-traceability.md` berubah, belum di-commit. **Riwayat:** Frontend: ter-commit `88bd2b1c7` di `sukmagpV2`; working tree bersih; belum di-push. Backend: laporan ini, `frontend-roadmap.md`, dan `requirement-traceability.md` berubah dan **belum** di-commit |
| Langkah berikutnya | **25 September 2026:** pemilik meninjau diff lanjutan lalu memberi instruksi commit. Untuk ✅ penuh diperlukan dua keputusan pemilik: gerbang golongan darah di backend (`VAL-BD-034`), dan apakah outcome "mencatat golongan darah" tetap bagian kartu ini atau dipindahkan. **Riwayat:** Task frontend berikutnya: `FE-BD-009`, `FE-BD-007`, `FE-BD-008`. Opsional: perbaikan pola `<label>` + `FilterSelect` yang sama pada dialog `FE-BD-004`/`FE-BD-012`. `FE-BD-005` dapat naik ke ✅ bila backlog proyeksi gerbang pemberian dikerjakan, atau bila pemilik mengubah acceptance kartu |
