# Laporan Perubahan Frontend — `FE-IGD-057`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-057` |
| Judul | Perawat memesan laboratorium dan radiologi atas instruksi dokter |
| Slice | R3.14 slice P · `SCR-IGD-P01` tab Penunjang |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14, kartu revisi 16 |
| Trace | `FR-IGD-102`; `AT-IGD-209`, `210`, `249`, `250`; `IGD-DEC-224`, `230` (6, 7), `362`, `364`, `365`, `367` |
| Contract version | API **`0.21.0`** §10.7, §10.7a; validation **`0.21.0`** §12.4 aturan 13–13c; `03` §15 tabel D baris Penunjang — seluruhnya `approved` (`IGD-DEC-368`, manifest 0u) |
| Wewenang UI | `DEV_DISCRETION` untuk rupa pemilih. Mengikat: pilihan hanya dari `GET …/instructing-doctor-options`, wajib dipilih, tidak diisi otomatis; daftar kosong → *"Belum ada DPJP atau dokter jaga IGD aktif untuk kunjungan ini."* dan simpan tidak ditawarkan |
| Dependency | `FE-IGD-056` ✅; `BE-IGD-071` 🟡 — endpoint dan aturan yang dikonsumsi task ini sudah ada dan terbukti lewat uji API 27 Oktober 2026 (A7, K5, K6); sisa acceptance backend adalah prasyarat lingkungan (`IGD-OQ-145`). Dikerjakan atas perintah Rizki, dirilis dan diuji bersama `BE-IGD-071` dan `FE-IGD-058` |
| Klasifikasi | `MEDIUM` — 10 berkas (8 diubah, 2 baru), satu section state dan satu thunk baru, dua panel memutus perilaku lama, nol route baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan ini, roadmap, traceability di `NewQuilvianSystemBackend` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `1c97ebbf9` + working tree |
| Commit backend yang dijadikan rujukan | `1c00e31d` + working tree `BE-IGD-071` (belum di-commit) |
| Tanggal | 28 Oktober 2026 |
| Status | ✅ **SELESAI — 28 Oktober 2026 (siang).** Acceptance 1–6 terpenuhi: sesudah jadwal jaga uji dibuat (`IGD-OQ-145`), putaran `057j` membuktikan pilihan DPJP lalu dokter jaga sama dengan respons server dan pesanan dengan dokter jaga `Pending`. Dirilis bersama `BE-IGD-071` ✅ dan `FE-IGD-058` ✅; belum di-commit. *Sebelumnya:* 🟡 **SEBAGIAN — 28 Oktober 2026 (pagi), sesudah uji layar.** Acceptance 2–6 terbukti; acceptance 1 terbukti untuk DPJP (pilihan = respons server, tidak terisi otomatis) — bagian *dokter jaga* tidak dapat diamati karena nol dokter IGD berjadwal jaga (`IGD-FACT-258`). Uji layar putaran akhir 4/4 skenario `PASS`; `npm run build` lulus (BUILD_ID `xyBFHuOLnm2WBo0PDNvBU`); `eslint` nol temuan baru. Tetap 🟡 sampai `BE-IGD-071` ✅ (kriteria 2 backend dan pilihan dokter jaga) — dirilis bersama |

*Riwayat.* Versi 12 Oktober 2026 laporan ini mencatat pembangunan panel radiologi dan bank darah dengan DPJP diisi
otomatis. Versi itu tidak lagi memenuhi kartu revisi 16 (`IGD-DEC-353`, penilaian ulang 27 Oktober) dan digantikan
laporan ini. Bukti uji lamanya tetap di [`testing/2026-10-12-laporan-uji-penunjang-medis-fe-igd-056-057.md`](../../testing/2026-10-12-laporan-uji-penunjang-medis-fe-igd-056-057.md).

---

## 1. Keadaan yang ditemukan di awal

Panel lab dan radiologi perawat menampilkan isian *"Dokter Penanggung Jawab Pasien (DPJP)"* yang baca saja, diisi dari
penugasan aktif (`activeAssignment.doctorId`), lalu mengirimnya sebagai `instructingDoctorId`. Dokter jaga IGD tidak
pernah dapat dipilih, dan pada kunjungan tanpa DPJP payload dikirim tanpa dokter. Sejak `BE-IGD-071`, kiriman itu
ditolak `400` *"Pilih dokter yang memberi instruksi."* tanpa perawat bisa memperbaikinya dari layar. Layar juga tidak
pernah memanggil `GET …/instructing-doctor-options` (endpoint baru `BE-IGD-071`). Bukti: `emergency-assessment-lab-panel.jsx`
baris 89–105, 205, 239–253 dan padanannya di panel radiologi pada `HEAD` `1c97ebbf9`.

---

## 2. Proses bisnis dari sisi pengguna

| Langkah | Yang dilakukan perawat IGD | Yang terjadi di layar |
| ---: | --- | --- |
| 1 | Membuka pasien di Pengkajian IGD, tab *Penunjang* | Daftar dokter pemberi instruksi untuk kunjungan itu dimuat sekali bersama daftar pesanan |
| 2 | Pada *Pesan Laboratorium* atau *Pesan Radiologi*, membuka bagian *1. Dokter Pemberi Instruksi* | Pilihan berisi DPJP IGD aktif lalu dokter jaga IGD aktif, persis dari server (label server, misalnya *"dr. Jenah Ali — DPJP"*). Tidak ada yang terpilih otomatis |
| 3 | Memilih dokter, pemeriksaan, lalu *Kirim Pesanan* | Tombol kirim baru aktif sesudah dokter dan pemeriksaan dipilih; ringkasan menampilkan *Dokter Pemberi Instruksi* |
| 4 | — | Berhasil: layar pindah ke daftar pesanan dan menampilkan *"Pesanan tersimpan dan menunggu verifikasi dr. …"* |

Jalur tidak normal:

| Keadaan | Yang terjadi |
| --- | --- |
| Kunjungan tanpa DPJP dan tanpa dokter jaga aktif | Peringatan *"Belum ada DPJP atau dokter jaga IGD aktif untuk kunjungan ini."*; tombol kirim **tidak ditampilkan** (`IGD-DEC-364`) |
| Daftar dokter gagal dimuat atau ditolak `403` | Pesan server tampil apa adanya bersama tombol *Coba lagi*; tombol kirim nonaktif dengan alasan yang sama |
| Server menolak pesanan `400`/`403` | Pesan server tampil apa adanya di bawah formulir; isian tetap. Pada `403` daftar dokter dimuat ulang — dokter yang tidak lagi sah (misalnya DPJP baru saja dialihkan) hilang dari pilihan dan pilihannya dikosongkan |

*Contoh.* Pukul 02.00 Tn. Budi punya DPJP dr. Ani dan dr. Rudi berjaga. Perawat Dimas membuka *Pesan Laboratorium*,
memilih *"dr. Rudi — Dokter jaga IGD"* dan darah lengkap, lalu mengirim. Layar menampilkan *"Pesanan tersimpan dan
menunggu verifikasi dr. Rudi."*. Pada pasien yang baru tiba tanpa DPJP pada jam tanpa dokter jaga, Dimas melihat
peringatan dan tidak ada tombol kirim — dokter memesan sendiri dari layarnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, `base-component-decision-gate.md`,
`base-component-catalog.md`, `REPORT_TEMPLATE.md`; kartu `FE-IGD-057` revisi 16; laporan `BE-IGD-071`. Source:
kedua panel, `emergency-assessment-diagnostic-support-tab.jsx`, `emergency-assessment-detail-view.jsx`,
`emergency-assessment-form-card.jsx`, `emergency-assessment-work-panel.jsx`, `emergency-assessment-slice.jsx`,
`use-emergency-assessment-detail.jsx`, `emergency-doctor-option-utils.js`, `emergency-assessment-constant.jsx`,
`BaseSelectField`, `BaseNativeSelectField`, `InformationAlert`, `BaseButton`, rujukan reuse
`nursing-lab-order-modal.jsx` (rawat inap). Backend (baca saja): `EmergencyDoctorAssignmentController`,
`EmergencyInstructingDoctorOptionResponse`, `LabOrderController`, `RadOrderController`, DTO daftar dan detail pesanan.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` | URL `instructing-doctor-options`; thunk `fetchInstructingDoctorOptions` (meneruskan `signal`, `forbidden` pada `403`); section `instructingDoctorOptions` di state awal, di `resetEmergencyAssessmentContext`, dan lewat `attachSection` |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-assessment-detail.jsx` | Thunk baru ikut loader tab *Penunjang*; section diteruskan lewat `sections` |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-instructing-doctor.jsx` (**baru**) | Pengendali pemilih: opsi, pilihan (dikosongkan bila tidak lagi ada di daftar), petunjuk alasan tombol nonaktif, muat ulang, muat ulang sesudah penolakan `403`, pemberitahuan *menunggu verifikasi* dari respons `POST` |
| `src/utils/health-services/emergency-installation-management/emergency-doctor-option-utils.js` | `mapInstructingDoctorSelectOptions` — `doctorId` → `value`, `label` server → `label` |
| `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx` | `INSTRUCTING_DOCTOR_MESSAGES` (kalimat kosong persis kartu, placeholder, memuat, wajib, coba lagi) |
| `src/components/view/.../components/emergency-assessment-instructing-doctor-field.jsx` (**baru**) | Rangkaian `BaseSelectField` / `InformationAlert` (kosong, gagal + `BaseButton` *Coba lagi*) |
| `src/components/view/.../components/emergency-assessment-form-card.jsx` | Prop opsional `showSubmit` (bawaan `true`): bila `false` tombol kirim dan petunjuk tombol nonaktif tidak dirender, dan submit diabaikan |
| `src/components/view/.../components/emergency-assessment-diagnostic-support-tab.jsx` | Meneruskan `instructingDoctorSection` ke panel lab dan radiologi |
| `src/components/view/.../components/emergency-assessment-lab-panel.jsx` | Isian DPJP baca saja diganti pemilih; `instructingDoctorId` dari pilihan; alasan nonaktif; `showSubmit`; ringkasan *Dokter Pemberi Instruksi*; pemberitahuan sesudah simpan di atas daftar. Import `BaseTextField` dan `formatEmergencyDoctorLabel` dilepas karena tidak dipakai lagi |
| `src/components/view/.../components/emergency-assessment-radiology-panel.jsx` | Sama dengan panel lab |

Panel bank darah, tindakan, dan pemakaian alat tidak disentuh — di luar cakupan kartu (`IGD-DEC-230` pilihan 6).

### 3.3 Kepatuhan arsitektur frontend

Alur `view → hook → slice → InstanceAxios` dipertahankan: view tidak memanggil Axios, endpoint di slice, kalimat di
constants, pemetaan opsi di utils (fungsi murni), logika pemilih di hook domain. Thunk baru meniru pola
`fetchEmergencyDoctorOptions`/`fetchLabOrders` (`unwrapItems`, `normalizeError`, `isForbidden`, `signal`); section
memakai `attachSection` yang sudah ada. Satu hook dan satu komponen rangkaian dipakai kedua panel supaya aturan pilih
tidak ditulis dua kali.

**Gerbang base component.** `UI GATE: 7 elemen — REUSE 5, EXTEND 1, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Pemilih dokter pemberi instruksi | `BaseSelectField` | `ui/form-pemeriksaan-ui/BaseSelectField.jsx` — `label`, `required`, `isLoading`, `disabled`, `placeholder`, `noOptionsMessage`; satu keluarga dengan `BaseTextField` yang digantikannya | REUSE | Dipakai apa adanya |
| Keterangan daftar kosong | `InformationAlert` | `base-features/information-alert.jsx` | REUSE | `variant="warning"`, `closable={false}`, kalimat persis |
| Gagal muat + coba lagi | `InformationAlert` + `BaseButton` | keduanya base-features; `InformationAlert` menerima `children` | COMPOSE | Opsi A (rekomendasi) dijalankan; opsi B tanpa tombol ditolak karena perawat harus pindah tab |
| Simpan tidak ditawarkan | `EmergencyAssessmentFormCard` (komponen domain, 9 pemakai) | tombol submit selalu dirender | EXTEND | Opsi A (rekomendasi): prop opsional `showSubmit`, bawaan tidak berubah. Opsi B (`canSubmit=false`) ditolak karena tombol tetap tampil nonaktif |
| Penolakan `400`/`403` di tempat aksi | `saveError` `EmergencyAssessmentFormCard` | sudah ada | REUSE | — |
| Status menunggu verifikasi terbaca | `InformationAlert` | — | REUSE | `variant="success"` di atas daftar sesudah simpan |
| Ringkasan pilihan | markup ringkasan yang ada | panel lab baris 433, rad 418 (`HEAD`) | REUSE | Label dan nilai diganti |

Tidak ada CSS baru; tidak ada nilai visual literal baru. Inline style lama di panel (warna heksadesimal) tidak disentuh
— peninggalan di luar cakupan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Pemilih menampilkan indikator memuat dan nonaktif; tombol kirim nonaktif dengan *"Memuat dokter pemberi instruksi..."* |
| Kosong | *"Belum ada DPJP atau dokter jaga IGD aktif untuk kunjungan ini."*; tombol kirim tidak ditampilkan |
| Gagal | Pesan server (bawaan *"Gagal mengambil daftar dokter pemberi instruksi."*) + tombol *Coba lagi* |
| Tanpa hak akses | `403` pada daftar dokter tampil sebagai pesan server di alert yang sama; `403` pada pesanan tampil di bawah formulir |
| Belum memilih | Tombol kirim nonaktif: *"Pilih dokter yang memberi instruksi."* |
| Berhasil | Pindah ke daftar; *"Pesanan tersimpan dan menunggu verifikasi dr. …"* |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Doctor Assignment

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-doctor-assignments/instructing-doctor-options?emergencyVisitId=` | Isi pemilih dokter pemberi instruksi | `EmergencyDoctorAssignment : Read` |

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/laboratory-management/lab-orders` | Pesanan lab dengan `instructingDoctorId` pilihan (bentuk payload tidak berubah) | `LabOrder : Create` |

#### Health Services / Radiology Management / Rad Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/radiology-management/rad-orders` | Pesanan radiologi dengan `instructingDoctorId` pilihan | `RadOrder : Create` |

**Delta source backend ↔ kebutuhan layar.** `GET /lab-orders` dan `GET /rad-orders` (daftar) tidak memuat
`instructionVerificationStatus` maupun `instructingDoctorId`; hanya detail dan respons `POST` yang memuatnya. Karena
itu status *menunggu verifikasi* ditampilkan dari respons `POST` sesaat sesudah simpan, bukan sebagai lencana per baris
daftar. Lencana per baris membutuhkan perubahan DTO daftar milik Laboratorium/Radiologi — dicatat, tidak dikerjakan.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `eslint` pada 10 berkas, dibandingkan dengan `git show HEAD:<berkas> \| eslint --stdin` | 0 error di semua berkas; peringatan lab 1 → 0, rad 3 → 2 (dua sisanya `preserve-manual-memoization` lama pada `modalityOptions`/`allRadProcedures`), 8 berkas lain 0 → 0; **nol temuan baru** | `PASS` | Keluaran perintah 28 Oktober 2026 |
| Komentar baru (`git diff -U0` disaring `//`, `/*`, `*`, `{/*`; berkas baru di-grep) | 0 | `PASS` | Keluaran perintah |
| Akhir baris | Empat berkas CRLF tetap CRLF utuh, empat LF tetap LF, dua berkas baru LF; nol campuran | `PASS` | Hitung byte per berkas |
| `globals.css`, `src/style/**` | Tidak berubah | `PASS` | `git diff --stat` |
| `npm run build` (28 Oktober, sesudah `next dev` pemilik berhenti) | Tiga kali, semuanya exit 0, *Compiled successfully*, tanpa warning di log; build terakhir sesudah dua perbaikan dari uji layar, BUILD_ID `xyBFHuOLnm2WBo0PDNvBU` | `PASS` | Log build |
| `npm run test:unit` | — | `NOT RUN` | Atas perintah pemilik: tanpa unit test |
| Uji layar — `uji-layar-057.mjs` (Playwright, server standalone, akun perawat `dimas`) | Putaran akhir: 4/4 skenario `PASS`, 23 cek | `PASS` | `test-with-agy/igd/uji-fe-igd-057-058-20261028/057-hasil.json`, `057-jaringan.json`, PNG `057-*` |

**Riwayat putaran uji layar** (berkas tiap putaran disimpan apa adanya):

| Putaran | Hasil | Sebab | Tindakan |
| --- | --- | --- | --- |
| 1 | Lab `PASS`; rad berhenti (tombol tetap nonaktif) | **Salah rancang skrip:** modalitas dipilih sebelum dokter, padahal petunjuk dokter didahulukan sehingga cek *"Pilih modalitas"* tidak pernah terpicu | Skrip diubah; `057-hasil-run1.json` |
| 2 | Rad berhenti lagi | **Salah rancang skrip:** opsi modalitas pertama adalah *"Semua Modalitas"* bernilai kosong | Skrip memilih modalitas nyata lebih dulu; `057-hasil-run2.json` |
| 3 | 4/4 `PASS` | Tangkapan layar menunjukkan kalimat kosong tampil **dua kali** (petunjuk tombol + alert) padahal tombolnya disembunyikan — **cacat kode task ini** | `EmergencyAssessmentFormCard` hanya menampilkan petunjuk tombol bila tombolnya tampil; build ulang; `057-hasil-run3.json`, `057-lab-d-kunjungan-tanpa-dokter-run3-kalimat-ganda.png` |
| 4 (akhir) | 4/4 `PASS`, termasuk cek baru *kalimat tampil tepat sekali* | — | `057-hasil.json` |
| `057j` (28 Oktober siang, sesudah jadwal jaga uji) | 2/2 `PASS` (lab, rad); skenario kosong `NOT APPLICABLE` — ada dokter jaga sepanjang hari itu | Pilihan layar `["dr. Jenah Ali — DPJP", "dr. B4 IGD Utama — Dokter jaga IGD"]` = respons; pilih dokter jaga → payload `32d0301c…`, `201` `Pending` | `057j-hasil.json`, `057j-jaringan.json`, PNG `057j-*` |

Nilai terukur putaran akhir: pilihan di layar `["dr. Jenah Ali — DPJP"]` = respons `instructing-doctor-options` (`200`);
placeholder tampil tanpa nilai awal; tanpa dokter tombol nonaktif dengan *"Pilih dokter yang memberi instruksi."* dan nol
`POST`; payload lab dan rad membawa `instructingDoctorId` `8ec5dfff…` (dr. Jenah Ali), respons `201` `Pending`;
*"Pesanan tersimpan dan menunggu verifikasi dr. Jenah Ali."* tampil; kunjungan Tn. X IGD 2710-0953 → respons `200 []`,
kalimat kosong persis, nol tombol kirim, kalimat tampil sekali — lab dan radiologi. Semua pesanan uji dibatalkan `200`.

Uji manual: `PASS` (otomatisasi peramban terhadap aplikasi berjalan). Akun perawat `dimas` adalah SuperAdmin — sah untuk
perilaku layar dan aturan 13, **tidak** untuk bukti izin `EmergencyDoctorAssignment : Read` perawat IGD sungguhan.

**Tidak dijalankan:** acceptance 1 bagian *dokter jaga* tidak dapat diamati di lingkungan dev sampai ada jadwal jaga IGD
(`IGD-FACT-258`, `IGD-OQ-145`).

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria (kartu revisi 16) | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pilihan memuat tepat DPJP IGD aktif dan dokter jaga IGD aktif — sama dengan respons `GET …/instructing-doctor-options` — dan tidak terisi otomatis | **Terpenuhi** | DPJP saja (`057`) dan DPJP + dokter jaga (`057j`): pilihan layar = respons server, urutan DPJP lalu dokter jaga, tanpa nilai awal |
| 2 | Pesanan lab dengan dr. Ani dipilih tersimpan menunggu verifikasi; payload membawa `instructingDoctorId` dr. Ani | **Terpenuhi** | `POST` `201` `Pending`, payload `instructingDoctorId` = dokter terpilih (`057-jaringan.json`) |
| 3 | Tanpa memilih dokter → pesanan tidak terkirim atau pesan `400` tampil apa adanya; nol pesanan | **Terpenuhi** | Tombol nonaktif dengan alasan persis; klik paksa → nol `POST` |
| 4 | Kunjungan tanpa DPJP dan dokter jaga → kalimat persis, simpan tidak ditawarkan | **Terpenuhi** | Tn. X IGD 2710-0953: respons `200 []`, kalimat persis tampil sekali, nol tombol kirim (`057-lab-d-kunjungan-tanpa-dokter.png`) |
| 5 | Radiologi berperilaku sama dengan 1–4 | **Terpenuhi** | `057-rad-pesan`, `057-rad-kosong`, `057j-rad-pesan` `PASS` |
| 6 | Diff, komentar, akhiran baris, `globals.css`; `eslint` nol temuan baru terhadap `HEAD`; `npm run build` lulus | **Terpenuhi** | Bagian 6 |

**DoD.** Acceptance 1–6 terpenuhi; laporan ini ada; register, grafik, dan traceability ditandai ✅ 28 Oktober 2026. Catatan:
perawat uji `dimas` SuperAdmin — izin `EmergencyDoctorAssignment : Read` perawat IGD sungguhan belum dibuktikan dengan akun
perawat non-SuperAdmin (tidak tersedia di lingkungan uji).

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Memutus bila dirilis terpisah dari `BE-IGD-071`:** backend lama mengabaikan `instructingDoctorId` dan tidak mengenal endpoint opsi — tanpa `BE-IGD-071` daftar dokter gagal dimuat (`404`) dan tombol kirim nonaktif |
| Masalah yang diketahui | (1) Status verifikasi tidak tampil per baris daftar — delta DTO daftar (bagian 5). (2) Daftar dokter dimuat saat tab *Penunjang* dibuka dan sesudah penolakan `403`; pengalihan DPJP dari modal dokter di layar yang sama tidak langsung memuat ulang daftar — pesanan dengan dokter lama akan ditolak `403` lalu daftar segar dimuat. (3) Pilihan dokter dipertahankan untuk pesanan berikutnya selama panel terbuka (dipilih manual, bukan terisi otomatis); dikosongkan otomatis bila dokter itu hilang dari daftar. (4) Perawat uji `dimas` adalah SuperAdmin — izin `EmergencyDoctorAssignment : Read` perawat IGD sungguhan harus dibuktikan dengan akun perawat IGD pada uji bersama |
| Dependency backend | `BE-IGD-071` 🟡 (prasyarat `IGD-OQ-145`). Endpoint dan aturan yang dipakai layar ini sudah terbukti lewat API |
| Perubahan sampingan | `NONE`. Berkas Rizki yang belum di-commit (`doctor-emergency-view.jsx`, `procedure-catalog-panel.jsx`, `emergency-physician-constant.jsx`) tidak disentuh |
| Interupsi | Backend berhenti 27 Oktober malam; Rizki menyalakannya kembali 28 Oktober 09.28 — uji layar dan build dijalankan sesudahnya |
| Status Git | `M` `emergency-assessment-diagnostic-support-tab.jsx`, `emergency-assessment-form-card.jsx`, `emergency-assessment-lab-panel.jsx`, `emergency-assessment-radiology-panel.jsx`, `emergency-assessment-constant.jsx`, `use-emergency-assessment-detail.jsx`, `emergency-assessment-slice.jsx`, `emergency-doctor-option-utils.js`; `??` `emergency-assessment-instructing-doctor-field.jsx`, `use-emergency-instructing-doctor.jsx`. Ditambah tiga `M` milik Rizki di atas |
| Langkah berikutnya | Commit bersama `BE-IGD-071` dan `FE-IGD-058` atas perintah Rizki |
