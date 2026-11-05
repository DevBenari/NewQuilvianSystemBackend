# Laporan Perubahan Frontend — `FE-OPT-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-012` |
| Judul | Form resep, salin resep luar, riwayat versi, dan cetak resep |
| Slice | Layar resep (`FE-OPT-N05`, `N06`); prasyarat `FE-OPT-020` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) — Rincian `FE-OPT-010`..`032` |
| Trace | `FR-OPT-032`; `OPT-DEC-007`, `017`, `045`, `071`, `072`, `073`; `OPT-FE-003`; `OPT-AC-004`, `012`, `033`; `UAT-05`, `UAT-06`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 3, 5.5, 5.6, 6 |
| Contract version | `OPT-API-v1` grup Opt Prescription, `GET reference-data/settings`; `OPT-STATE-v1` bagian 2; `OPT-VAL-v1` bagian 2 — `approved`, terkunci hash |
| Wewenang UI | Screen brief `approved` untuk route, mode form, panel riwayat, dan isi kartu cetak; tata letak isian OD/OS `DEV_DISCRETION` |
| Dependency | `FE-OPT-010` ✅, `011` ✅, `001` ✅, `002` ✅, `BE-OPT-012` ✅ |
| Klasifikasi | `MEDIUM` — 3 route, 5 berkas view, 5 hook, 1 konstanta, 1 aturan, perubahan kecil kunjungan dan pemeriksaan, 3 berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010`, `011` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji kontrak dengan backend lokal). Alur tulis dengan akun RO/Sp.M asli belum dapat dijalankan (bagian 6, 8). Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Slice resep sudah ada dari `FE-OPT-002`, dan aturan satu mata serta transposisi dari `FE-OPT-001`. Panel riwayat versi dari `FE-OPT-011` siap dipakai ulang. Tombol resep di kunjungan dan pemeriksaan masih nonaktif. Fakta backend berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| Server sendiri yang mentransposisi CYL plus lalu menyimpan teks asli | `OptPrescriptionRules.TransposeForCopy` | Layar mengirim nilai **apa adanya**; transposisi di layar hanya pratinjau dan dasar validasi |
| Resep luar dinilai sesudah transposisi | `CopyExternalAsync` | Validasi layar menilai hasil transposisi, termasuk rentang |
| Mata yang terpakai wajib ber-SPH (`0` untuk plano) | `ValidateSpectacleEye` | Aturan ditambahkan; aturan `FE-OPT-001` membolehkan SPH kosong per mata |
| Satu resep per jenis per pemeriksaan; koreksi lewat revisi | `CreateInternalAsync` | `409` tampil apa adanya, isian tidak hilang |
| Jawaban resep tidak memuat identitas pasien dan nomor pesanan pengunci | `OptPrescriptionResponse` | Identitas dari `GET visits/{id}`, nomor pesanan dari `GET orders/{id}`; keduanya pelengkap |
| `selectHasPermission` menjawab benar selama izin belum termuat | `permission-slice`, ditemukan E2E | Permintaan bantuan menunggu izin termuat |

---

## 2. Proses bisnis dari sisi pengguna

1. **Rina (RO) membuat resep dari pemeriksaan final.** Form terisi koreksi subjektif dan PD, dengan keterangan *"Nilai diambil dari OPT-EXM-RSMMC-000002; periksa sebelum menyimpan."* Nilai yang diubah sebelum simpan adalah yang tersimpan (`OPT-AC-033`). **Contoh:** OD −2,75 diubah ke −2,50 → resep berisi −2,50.
2. **Rina menyalin resep luar (`UAT-05`).** Petugas mengetik apa adanya dari kertas: +1,00 / +0,50 × 90. Sebelum simpan tampil *"teks asli +1,00 / +0,50 × 90 → tersimpan +1,50 / −0,50 × 180"*. Sesudah simpan, detail menampilkan teks aslinya. Penulis, fasilitas, dan tanggal wajib; tanggal tidak boleh melewati hari ini.
3. **Lensa kontak.** Isian power, CYL, AXIS, BC, dan DIA per mata, ditambah merk/jenis dan masa pakai. Rentang BC/DIA dari Pengaturan. Isian tidak diambil dari koreksi kacamata, dan keterangannya tampil di form.
4. **Ukuran di luar rentang.** **Contoh:** SPH −22,00 ditandai merah. RO tetap dapat menekan simpan dan ditolak server *"SPH di luar rentang −20,00..+20,00; hanya Sp.M yang dapat mengonfirmasi."* Sp.M mendapat isian *Alasan Melewati Rentang* yang wajib.
5. **Detail resep.** Menampilkan versi terkini, label *Kedaluwarsa*, dan tanda *Terkunci (OPT-ORD-RSMMC-000001)*. Resep terkunci tidak punya tombol Revisi.
6. **Revisi.** Isian versi terkini + alasan wajib. Silinder wajib minus, tanpa transposisi. Versi lama tetap utuh dan dapat dibandingkan. Bila lensa sudah dipesan, server menolak *"Resep terkunci karena lensa sudah dipesan."* (`UAT-06`).
7. **Cetak Resep.** Kartu berisi identitas pasien, jenis, nilai versi terkini, PD, pemeriksa + nomor izin (atau penulis + fasilitas untuk resep luar), serta tanggal terbit dan berlaku. Tanpa identitas pasien, tombol cetak nonaktif.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Slice dan service resep `FE-OPT-002`; `optical-prescription-rules.js`; panel riwayat `FE-OPT-011`; pola cetak Lab (`lab-monitoring-view`, `lab-order-print-document`); `base-letterhead`, `filter-date-picker`, `use-permission`. Backend (baca-saja): `OptPrescriptionController`, `OptPrescriptionService` (`OptPrescriptionRules`), `OptPrescriptionDtos`, `OptReferenceDataDtos`; kontrak `state-transition-matrix`, `validation-matrix`, `permission-audit-matrix`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/optical-management/visits/[encounterId]/prescriptions/new/page.jsx` | **Baru.** Route buat resep; `?examId=` divalidasi UUID |
| `src/app/health-services/optical-management/prescriptions/[prescriptionId]/page.jsx` | **Baru.** Route detail |
| `src/app/health-services/optical-management/prescriptions/[prescriptionId]/revise/page.jsx` | **Baru.** Route revisi |
| `src/components/view/health-services/optical-management/prescriptions/optical-prescription-fields.jsx` | **Baru.** Isian ukur, pratinjau transposisi, isian alasan |
| `src/components/view/health-services/optical-management/prescriptions/optical-prescription-create-view.jsx` | **Baru.** Form dua mode |
| `src/components/view/health-services/optical-management/prescriptions/optical-prescription-detail-view.jsx` | **Baru.** Detail, riwayat, tombol cetak |
| `src/components/view/health-services/optical-management/prescriptions/optical-prescription-revise-view.jsx` | **Baru.** Form revisi |
| `src/components/view/health-services/optical-management/prescriptions/optical-prescription-print-document.jsx` | **Baru.** Kartu cetak |
| `src/lib/hooks/health-services/optical-management/prescriptions/use-optical-prescription-form-state.jsx` | **Baru.** Keadaan isian, hak lewat rentang, pengaturan |
| `src/lib/hooks/health-services/optical-management/prescriptions/use-optical-prescription-create.jsx` | **Baru.** Controller buat |
| `src/lib/hooks/health-services/optical-management/prescriptions/use-optical-prescription-loader.jsx` | **Baru.** Token route, resep, kunjungan |
| `src/lib/hooks/health-services/optical-management/prescriptions/use-optical-prescription-detail.jsx` | **Baru.** Controller detail |
| `src/lib/hooks/health-services/optical-management/prescriptions/use-optical-prescription-revise.jsx` | **Baru.** Controller revisi |
| `src/lib/constants/health-services/optical-management/optical-prescription-constants.js` | **Baru.** Daftar isian, kalimat, teks |
| `src/utils/health-services/optical-management/optical-prescription-form-rules.js` | **Baru.** Penilaian form, kiriman, tampilan, cetak |
| `src/lib/constants/health-services/optical-management/optical-visit-constants.js` | **Diperbarui.** `OPTICAL_SCREEN_READY.prescription` menyala |
| `src/utils/health-services/optical-management/optical-visit-rules.js` | **Diperbarui.** `latestFinalOpticalExam` |
| `src/lib/hooks/health-services/optical-management/visits/use-optical-visit-detail.jsx`, `src/components/view/health-services/optical-management/visits/optical-visit-detail-view.jsx` | **Diperbarui.** Tombol resep dan klik baris resep |
| `src/lib/hooks/health-services/optical-management/refraction-exams/use-optical-refraction-exam.jsx`, `src/components/view/health-services/optical-management/refraction-exams/optical-refraction-exam-view.jsx` | **Diperbarui.** Tombol Buat Resep aktif |
| `tests/unit/optical-prescription-form-rules.test.mjs` | **Baru.** 19 uji |
| `tests/e2e/optical-prescription-screen.spec.mjs` | **Baru.** 20 skenario API tiruan |
| `tests/e2e/optical-visit-screen.spec.mjs`, `tests/e2e/optical-refraction-exam-screen.spec.mjs` | **Diperbarui.** Mengikuti tombol resep yang kini aktif |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice → InstanceAxios`, dengan slice `FE-OPT-002` tanpa perubahan. Aturan satu mata memakai `FE-OPT-001` apa adanya, tanpa salinan. Panel riwayat `FE-OPT-011` dipakai ulang. Cetak memakai `useReactToPrint` dan `BaseLetterhead` yang ada; tidak ada CSS baru.

**Kartu cetak memakai `<table>` mentah** bertanda `data-flat-table="true"`, sesuai checklist, karena `DataTable` membawa paginasi dan keadaan layar yang tidak relevan di kertas.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Keterangan muat; isian dan tombol nonaktif selama kiriman, dan tetap nonaktif sesudah berhasil sampai halaman berpindah |
| Kosong | Isian kosong tampil "—" pada detail dan kartu cetak |
| Gagal | Pesan server di atas tombol, isian tidak hilang; `503` ada *Coba lagi* |
| Tanpa hak akses | Tanpa `Create` atau `Revise` → akses ditolak; `403` muat detail → akses ditolak; `403` simpan → pesan server |
| Data basi | `409` *diubah petugas lain* pada revisi → detail dimuat ulang, form berisi data terbaru, pesan server tampil |
| Nonaktif | Pemeriksaan draf, pemeriksaan kunjungan lain, atau tanpa sumber → form tidak tampil, keterangan tampil; resep terkunci → simpan revisi nonaktif; identitas belum termuat → cetak nonaktif |
| Validasi | Galat per isian, kalimat sama dengan backend; di luar rentang ditandai tanpa menahan kiriman |

Privasi: nilai resep tidak ditulis ke `console` (ada skenario E2E); judul dokumen cetak hanya nomor resep.

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Prescription

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/prescriptions/{id}` | Detail + seluruh versi | `OptPrescription : Read` |
| `GET` | `/v1/health-services/optical-management/prescriptions/range-override` | Boleh melewati rentang | `OptPrescription : OverrideRange` |
| `POST` | `/v1/health-services/optical-management/prescriptions` | Buat dari pemeriksaan | `OptPrescription : Create` |
| `POST` | `/v1/health-services/optical-management/prescriptions/external` | Salin resep luar | `OptPrescription : Create` |
| `POST` | `/v1/health-services/optical-management/prescriptions/{id}/revisions` | Revisi | `OptPrescription : Revise` |

#### Health Services / Optical Management / Opt Refraction Exam

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/refraction-exams/{id}` | Isian awal dari pemeriksaan final | `OptRefractionExam : Read` |

#### Health Services / Optical Management / Opt Reference Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/reference-data/settings` | Rentang BC/DIA | `OptReferenceData : Read` |

#### Health Services / Optical Management / Opt Visit dan Opt Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/visits/{encounterId}` | Identitas pasien pada header dan kartu cetak | `OptVisit : Read` |
| `GET` | `/v1/health-services/optical-management/orders/{id}` | Nomor pesanan pengunci | `OptOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-prescription-form-rules.test.mjs` | 19 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Uji mutasi: tanpa transposisi / power lensa ikut terisi / alasan rentang selalu terkirim / cetak resep luar memakai penyalin / cetak tanpa identitas | Uji terkait gagal pada kelimanya; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran perintah |
| E2E resep + pemeriksaan + kunjungan atas build produksi | 61 lulus, 0 gagal (20 + 19 + 22) | `PASS` | Keluaran Playwright |
| Pintu masuk | Tab Resep ke pemeriksaan final **terbaru**, salin resep luar, dan detail; pemeriksaan ke form | `PASS` | Skenario masuk |
| Dari pemeriksaan | Terisi subjektif + PD, nilai diubah tersimpan, bentuk kiriman persis; `409` ganda tampil, isian utuh; lensa kontak tidak terisi | `PASS` | Skenario dari pemeriksaan |
| Masuk ditahan | Pemeriksaan draf, kunjungan lain, tanpa sumber, tanpa izin | `PASS` | Skenario masuk ditahan |
| Resep luar `UAT-05` | Pratinjau, kiriman apa adanya, data asal wajib, teks asli tampil di detail | `PASS` | Skenario resep luar |
| Rentang | RO: ditandai, tanpa isian alasan, tanpa panggilan `range-override`, ditolak server. Sp.M: alasan wajib dan terkirim, hilang saat kembali dalam rentang | `PASS` | Skenario rentang |
| Lensa kontak | Rentang dari pengaturan, kelengkapan, kiriman hanya isian lensa; pengaturan gagal → server menilai | `PASS` | Skenario lensa kontak, gangguan |
| Detail dan kunci | Nilai versi terkini, tanda kunci + nomor pesanan, tanpa izin pesanan tetap bertanda, kedaluwarsa, `404`, `403` | `PASS` | Skenario detail, kunci |
| Cetak | Isi kartu dibaca dari iframe cetak: kacamata, resep luar, lensa kontak; identitas gagal → tombol nonaktif | `PASS` | Skenario cetak |
| Revisi `UAT-06` | Isian terkini, alasan wajib, CYL plus ditolak tanpa pratinjau, versi baru, `409` terkunci, `409` basi, terkunci menahan tombol, tanpa izin ditolak | `PASS` | Skenario revisi |
| Backend lokal: `POST` buat kacamata, buat kontak, salin luar, revisi bentuk layar ke id acak | Keempatnya `404` `OPT-NOT-FOUND` sesudah lolos binder; kontrol negatif `od.axis: "x"` dan tanggal salah → `400`; jumlah resep tetap 0 | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal: `GET` pengaturan dan `range-override` | Nama ruas rentang cocok; superadmin `canOverrideRange: false` | `PASS` | Skrip kontrak |
| Tangkapan layar 1366 px: form resep luar, detail, kartu cetak | Susunan sesuai brief; `window.print` asli dan keluaran kertas tidak diuji | `PASS` | Tangkapan layar di scratchpad |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.334 lulus, 72 gagal dari 4.410; daftar berkas gagal **identik** dengan sesudah `FE-OPT-011` (`diff` kosong) | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` (server lokal dimatikan lebih dulu) | `Compiled successfully in 64s`; 568 halaman statis; exit 0 | `PASS` | Keluaran build |

Uji manual: `NOT FEASIBLE` untuk alur tulis dengan backend asli; `PASS` untuk seluruh kontrol lewat Playwright dengan API tiruan. Sebabnya sama dengan `FE-OPT-011`: kode profesi `OpticalManagement:Practitioner` kosong dan belum ada akun RO/Sp.M, sehingga devYoga juga belum punya pemeriksaan final sebagai sumber resep. Perilaku tulis backend terbukti di harness `BE-OPT-012` dan `BE-OPT-034`.

**Tidak dijalankan:** cetak ke kertas sungguhan (isi kartu dibaca dari iframe cetak); `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N05` dan brief §5.5 | Terpenuhi | Bagian 6 |
| Layar sesuai `FE-OPT-N06` dan brief §5.6 | Terpenuhi, memakai panel `FE-OPT-011` | Skenario detail |
| `OPT-AC-033`: terisi dari pemeriksaan, nilai diubah yang tersimpan | Terpenuhi di E2E API tiruan | Skenario dari pemeriksaan |
| `UAT-05`: resep luar plus tersimpan minus, teks asli terlihat | Terpenuhi di E2E API tiruan untuk layar; transposisi server di harness `BE-OPT-012` | Skenario resep luar |
| `UAT-06`: revisi resep terkunci ditolak | Terpenuhi: Revisi hilang, tombol tertahan, `409` tampil | Skenario kunci, revisi |
| `OPT-DEC-045`: isian di luar rentang ditandai; Sp.M alasan, RO ditolak server | Terpenuhi | Skenario rentang |
| `OPT-DEC-073`: Cetak Resep dari data di layar | Terpenuhi | Skenario cetak |
| Isian ukur tidak memakai `type: "number"` (`OPT-FE-003`) | Terpenuhi | Skenario dari pemeriksaan |
| DoD: tidak ada data sensitif di `console` | Terpenuhi | Skenario privasi |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Kode profesi RO/Sp.M masih kosong** (lihat `FE-OPT-011`): setiap akun ditolak `OPT-VAL-001`, jadi resep belum dapat dibuat sungguhan. Superadmin juga mendapat `canOverrideRange: false` karena alasan yang sama |
| Peringatan | **Kiriman ganda resep luar saat jaringan putus.** Endpoint resep tidak punya kunci idempoten (catatan `FE-OPT-002`). Klik ganda ditahan, tetapi bila jaringan putus sesudah server menyimpan, kiriman ulang membuat resep kedua. Resep dari pemeriksaan terlindung penolakan `409` ganda. *Coba lagi* karena itu hanya ditawarkan pada `503`, saat server pasti belum menyimpan |
| Keputusan pengembang | **Lensa kontak tidak terisi dari pemeriksaan.** Brief menyebut isian terisi dari koreksi subjektif, tetapi `OPT-DEC-071` hanya merinci nilai kacamata dan pemeriksaan tidak punya BC/DIA, sementara power lensa kontak dapat berbeda dari power kacamata. Mohon koreksi bila pemilik menghendaki power tetap terisi |
| Delta brief | (1) Buat Resep dari Pemeriksaan pada tab Resep memakai pemeriksaan final **terbaru**; pemeriksaan lain dibuka dari halamannya. (2) Revisi tidak menawarkan transposisi, sesuai `OptPrescriptionReviseRequest` |
| Perubahan yang tidak menyangkut layar | `use-optical-prescription-form-state` memakai `usePermission` karena `selectHasPermission` menjawab benar selama izin belum termuat. Pola `hasPermission` pada layar `FE-OPT-010`, `011` hanya mengatur tampilnya tombol, jadi tidak memicu permintaan dan dibiarkan |
| Data uji | Tidak ada tulisan baru di devYoga; jumlah resep tetap 0 |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Dependency backend | `NONE` untuk kode. Konfigurasi kode profesi menahan pemakaian nyata |
| Perubahan sampingan | `tests/e2e/optical-refraction-exam-screen.spec.mjs` sempat rusak oleh skrip sunting saya (pola `$` diikuti backtick pada `String.replace`). Dipulihkan dengan pemeriksaan bahwa bagian yang dibuang adalah salinan persis; 19 skenarionya lulus sesudahnya. Lima mutasi uji dibuat dan dipulihkan |
| Interupsi | `NONE` |
| Status Git | Di luar berkas `FE-OPT-010`, `011`: `??` `optical-prescription-constants.js`, `optical-prescription-form-rules.js`, dua berkas uji baru; folder Optik yang sudah `??` bertambah route, view, dan hook |
| Langkah berikutnya | `FE-OPT-020` (buat pesanan, katalog, tagihkan, ajukan diskon, status bayar, kirim ke pengerjaan, cetak bukti pesanan) |
