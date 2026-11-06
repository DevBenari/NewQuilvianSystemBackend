# Laporan Perubahan Frontend — `FE-OPT-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-011` |
| Judul | Form pemeriksaan refraksi dan riwayat revisi |
| Slice | Layar pemeriksaan (`FE-OPT-N04`, `N06`); panel riwayat versi dipakai lagi `FE-OPT-012` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) — Rincian `FE-OPT-010`..`032` |
| Trace | `FR-OPT-020`, `FR-OPT-021`; `OPT-DEC-005`, `012`, `042`, `047`, `056`, `059`, `072`; `OPT-FE-003`; `OPT-AC-001`, `002`; `UAT-03`, `UAT-04`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 3, 5.4, 5.6, 6 |
| Contract version | `OPT-API-v1` grup Opt Refraction Exam, `OPT-STATE-v1` bagian 1, `OPT-VAL-v1` bagian 3 — `approved`, terkunci hash |
| Wewenang UI | Screen brief `approved` untuk route, keadaan, modal, dan panel riwayat; tata letak isian OD/OS `DEV_DISCRETION` |
| Dependency | `FE-OPT-010` ✅, `001` ✅, `002` ✅, `BE-OPT-011` ✅, `BE-OPT-031` ✅ |
| Klasifikasi | `MEDIUM` — 2 route, 4 berkas view, 3 hook, 1 konstanta, 2 aturan, perubahan kecil detail kunjungan, 2 berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji kontrak dengan backend lokal). Alur tulis dengan akun RO asli belum dapat dijalankan (bagian 6, 8). Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Slice pemeriksaan sudah ada dari `FE-OPT-002`. Detail kunjungan dari `FE-OPT-010` sudah menampilkan daftar pemeriksaan, dengan tombol *Mulai Pemeriksaan* nonaktif. Fakta backend berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| Simpan final memakai isian draf terakhir **di server** | `OptRefractionExamFinalizeRequest` | Isian yang belum tersimpan dikirim dulu lewat `PUT`, baru `POST finalize` dengan versi hasil simpan |
| Setiap simpan mengganti seluruh set hasil ukur | `OptRefractionMeasurementValues` | Seluruh 26 isian selalu dikirim, termasuk dua catatan yang tidak ada di sketsa brief |
| Hasil ukur tidak wajib kelipatan 0,25, berbeda dari resep | `OptRefractionExamService.ValidateMeasurements` | Validasi resep `FE-OPT-001` tidak dipakai; layar memakai cermin aturan pemeriksaan sendiri |
| `billingMessage` hanya ada pada jawaban final/revisi | `OptRefractionExamResponse` | Saat dimuat ulang, tanda belum tertagih dibaca dari `billingFactEmittedAt` yang kosong |
| Jawaban pemeriksaan tidak memuat nama pasien dan nomor kunjungan | `OptRefractionExamResponse` | Header membaca `GET visits/{encounterId}`; bila gagal, halaman tetap terbuka tanpa nama |

---

## 2. Proses bisnis dari sisi pengguna

1. **Rina (RO) memulai pemeriksaan** dari tab Pemeriksaan. Halaman draf terbuka; bila kunjungan sudah punya draf, draf itu yang dibuka. Penolakan seperti *"STR Anda tidak berlaku sejak 31-10-2026."* tampil apa adanya di tab.
2. **Rina mengisi hasil ukur.** **Contoh:** visus jauh OD `6/21`, OS `6/24`; autoref OD −2,50 / −0,50 × 180; PD 63,5. Tanda minus dan koma tetap utuh, dan minus tipografis hasil salinan dipahami. Kesalahan ditandai di dekat isian dengan kalimat yang sama dengan server, mis. *"AXIS autorefraktometer mata kanan wajib diisi bila ada silinder."*
3. **Simpan Final.** Modal mengingatkan *"Tarif pemeriksaan akan ditagihkan."* Isian terakhir di layar ikut final walau Simpan Draf belum ditekan. Tanpa visus jauh kedua mata, modal tidak terbuka dan pesan muncul pada mata yang kosong.
4. **Tarif gagal tercatat.** Pemeriksaan tetap final, dengan peringatan kuning berisi pesan server.
5. **Koreksi sesudah final (`UAT-03`).** Rina menekan *Revisi*, mengubah OS 6/15 → 6/12, dan menulis alasan *"salah ketik visus OS"*. Versi 2 bertambah, versi 1 tetap utuh, dan tarif tidak ditagih ulang.
6. **Membandingkan versi.** Petugas memilih dua versi lalu *Bandingkan*. Dua kolom tampil berdampingan, terlama di kiri, dan baris yang berbeda bertanda *Berbeda*.
7. **Draf dibatalkan** dengan alasan wajib. Halaman menjadi baca-saja dengan alasannya.
8. **Dua petugas menyimpan bersamaan.** Yang kedua membaca *"Pemeriksaan sudah diubah petugas lain. Muat ulang halaman."* dan form langsung berisi data terbaru.
9. **Petugas penjualan** (hanya `Read`) melihat hasil ukur tanpa form dan tanpa tombol.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Slice dan service pemeriksaan `FE-OPT-002`; `optical-prescription-rules.js`; `lab-decimal-input-rules.js`; `base-form-control`, `confirm-modal`, `toast-stack`; pola MCU (`mcu-order-action-modals`, `use-mcu-order-detail`). Backend (baca-saja): `OptRefractionExamController`, `OptRefractionExamService`, `OptRefractionExamDtos`, `OptPractitionerGuard`; kontrak `state-transition-matrix`, `validation-matrix`, `permission-audit-matrix`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/optical-management/refraction-exams/[examId]/page.jsx` | **Baru.** Route pemeriksaan |
| `src/app/health-services/optical-management/refraction-exams/[examId]/revise/page.jsx` | **Baru.** Route revisi |
| `src/components/view/health-services/optical-management/refraction-exams/optical-refraction-exam-view.jsx` | **Baru.** Draf, final, dibatalkan, modal final dan batal |
| `src/components/view/health-services/optical-management/refraction-exams/optical-refraction-exam-revise-view.jsx` | **Baru.** Form revisi |
| `src/components/view/health-services/optical-management/refraction-exams/optical-exam-measurements.jsx` | **Baru.** Form dan tampilan baca hasil ukur |
| `src/components/view/health-services/optical-management/shared/optical-version-history-panel.jsx` | **Baru.** Panel riwayat versi + pembanding (§5.6), untuk pemeriksaan dan resep |
| `src/lib/hooks/health-services/optical-management/refraction-exams/use-optical-exam-loader.jsx` | **Baru.** Token route, muat pemeriksaan dan kunjungan |
| `src/lib/hooks/health-services/optical-management/refraction-exams/use-optical-refraction-exam.jsx` | **Baru.** Controller halaman pemeriksaan |
| `src/lib/hooks/health-services/optical-management/refraction-exams/use-optical-refraction-exam-revise.jsx` | **Baru.** Controller revisi |
| `src/lib/constants/health-services/optical-management/optical-exam-constants.js` | **Baru.** Daftar isian, kalimat, teks |
| `src/utils/health-services/optical-management/optical-exam-rules.js` | **Baru.** Form ↔ kiriman, validasi, tombol, format |
| `src/utils/health-services/optical-management/optical-version-compare-rules.js` | **Baru.** Pilihan dan perbandingan versi |
| `src/lib/constants/health-services/optical-management/optical-visit-constants.js` | **Diperbarui.** `OPTICAL_SCREEN_READY.refractionExam` menyala |
| `src/lib/hooks/health-services/optical-management/visits/use-optical-visit-detail.jsx` | **Diperbarui.** Mulai Pemeriksaan dan buka pemeriksaan |
| `src/components/view/health-services/optical-management/visits/optical-visit-detail-view.jsx` | **Diperbarui.** Tombol tab bekerja, klik baris pemeriksaan, pesan penolakan |
| `tests/unit/optical-exam-rules.test.mjs` | **Baru.** 19 uji |
| `tests/e2e/optical-refraction-exam-screen.spec.mjs` | **Baru.** 19 skenario API tiruan |
| `tests/unit/optical-visit-rules.test.mjs`, `tests/e2e/optical-visit-screen.spec.mjs` | **Diperbarui.** Mengikuti tombol Mulai Pemeriksaan yang kini aktif |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice → InstanceAxios`, dengan slice `FE-OPT-002` tanpa perubahan. Isian desimal memakai masukan teks ber-`inputMode: decimal`, bukan `type: "number"` (`OPT-FE-003`). Seluruh elemen memakai komponen yang ada; tidak ada CSS baru.

**Pola yang ditambahkan, beserta alasannya:** `OPTICAL_EXAM_FIELD_GROUPS` menjadi satu-satunya daftar isian, karena form, tampilan baca, kiriman, dan pembanding harus selalu sama lengkapnya; satu isian yang terlewat berarti data terhapus saat simpan. Uji unit mencocokkan daftar ini dengan DTO backend.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Keterangan muat; isian dan tombol nonaktif selama aksi; label tombol *Menyimpan…* |
| Kosong | Isian kosong tampil "—" pada tampilan baca; panel riwayat hanya sesudah final |
| Gagal | Pesan server di atas tombol, isian tidak hilang; `503` atau jaringan putus ada *Coba lagi* dengan isian terbaru |
| Tanpa hak akses | `403` muat → akses ditolak; `403` aksi (izin, STR, SIP) → pesan server tanpa ulang; tanpa `Update` → baca-saja; tanpa `Revise` → halaman revisi ditolak |
| Data basi | `409` *diubah petugas lain* → detail dimuat ulang, form berisi data terbaru, pesan server tampil |
| Nonaktif | *Buat Resep* nonaktif, *Fitur belum tersedia*, sampai `FE-OPT-012`; revisi atas draf menahan tombol simpan |
| Validasi | Galat per isian, kalimat sama dengan backend; server tetap penentu akhir |

Privasi: nilai ukur dan catatan tidak ditulis ke `console` (ada skenario E2E); tautan memakai token route.

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Refraction Exam

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/refraction-exams/{id}` | Detail + seluruh versi | `OptRefractionExam : Read` |
| `POST` | `/v1/health-services/optical-management/refraction-exams` | Mulai Pemeriksaan | `OptRefractionExam : Create` |
| `PUT` | `/v1/health-services/optical-management/refraction-exams/{id}/measurements` | Simpan Draf; juga sebelum final bila isian berubah | `OptRefractionExam : Update` |
| `POST` | `/v1/health-services/optical-management/refraction-exams/{id}/finalize` | Simpan Final | `OptRefractionExam : Finalize` |
| `POST` | `/v1/health-services/optical-management/refraction-exams/{id}/revisions` | Revisi | `OptRefractionExam : Revise` |
| `POST` | `/v1/health-services/optical-management/refraction-exams/{id}/cancel` | Batalkan draf | `OptRefractionExam : Cancel` |

#### Health Services / Optical Management / Opt Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/visits/{encounterId}` | Nama pasien dan nomor kunjungan pada header | `OptVisit : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-exam-rules.test.mjs` | 19 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Uji mutasi: catatan klinis hilang dari daftar / CYL plus diterima / tanda berbeda selalu palsu | Uji terkait gagal; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran perintah |
| E2E pemeriksaan + kunjungan atas build produksi | 41 lulus, 0 gagal (19 + 22) | `PASS` | Keluaran Playwright |
| Mulai | Kirim `{ encounterId }`, buka draf; klik baris membuka pemeriksaan; kembali ke tab; penolakan STR tampil tanpa pindah halaman | `PASS` | Skenario mulai |
| Draf | Minus dan koma utuh, minus tipografis, seluruh 26 isian + versi terkirim, versi berikutnya terpakai, galat per isian tanpa mengirim | `PASS` | Skenario draf |
| Final | Tanpa visus ditahan; Batal tidak mengirim; isian baru → `PUT` lalu `finalize` versi baru; tanpa perubahan → `finalize` saja; pesan tagihan; `PUT` berhasil + final ditolak tetap draf | `PASS` | Skenario final |
| Bentrok dan gangguan | `409` basi memuat ulang; `503` Coba lagi mengirim isian terbaru; kunci rekam medis tanpa Coba lagi | `PASS` | Skenario bentrok, gangguan |
| Batal | Alasan wajib, penolakan di modal, sesudah batal baca-saja + alasan | `PASS` | Skenario batal |
| Revisi | Isian versi terkini, alasan wajib, seluruh set tanpa `finalize`, kembali ke halaman baca; penolakan tidak menghilangkan isian; draf menahan tombol; tanpa izin ditolak | `PASS` | Skenario revisi |
| Riwayat | Terbaru dulu, pilihan ketiga dikunci, berbeda ditandai, baris kosong dibuang, lepas pilihan menutup pembanding | `PASS` | Skenario riwayat |
| Backend lokal: `PUT`, `finalize`, `revisions`, `cancel` bentuk layar ke id acak | Keempatnya `404` `OPT-NOT-FOUND` sesudah lolos binder; kontrol negatif `autorefOdAxis: "x"` → `400` | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal, layar asli, superadmin: Mulai Pemeriksaan pada `ENC-RSMMC-00186` | `403` *"Anda tidak berwenang membuat resep atau pemeriksaan refraksi."* tampil di tab; jumlah pemeriksaan tetap 0 | `PASS` | Tangkapan layar |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.315 lulus, 72 gagal dari 4.391; daftar berkas gagal **identik** dengan sesudah `FE-OPT-010` (`diff` kosong) | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` (server lokal dimatikan lebih dulu) | `Compiled successfully in 61s`; 568 halaman statis; exit 0 | `PASS` | Keluaran build |

Uji manual: `NOT FEASIBLE` untuk alur tulis dengan backend asli; `PASS` untuk seluruh kontrol lewat Playwright dengan API tiruan. Alur Mulai → Final → Revisi tidak dapat dijalankan pada devYoga karena `OpticalManagement:Practitioner` kosong di `appsettings.json` dan `appsettings.Development.json`, sehingga setiap akun ditolak `OPT-VAL-001`, dan belum ada akun pegawai RO/Sp.M ber-SIP dan STR untuk uji. Perilaku tulis backend terbukti di harness `BE-OPT-011` dan `BE-OPT-031`.

**Tidak dijalankan:** `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh); pemeriksaan satu baris tarif `UAT-03` di layar kasir, karena butuh alur tulis asli di atas.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N04` dan brief §5.4 (tiga keadaan, modal, peringatan tagihan, isian desimal, galat STR/SIP) | Terpenuhi | Skenario draf, final, batal |
| Layar sesuai `FE-OPT-N06` dan brief §5.6 | Terpenuhi | Skenario riwayat |
| Isian ukur tidak memakai `type: "number"` (`OPT-FE-003`) | Terpenuhi: `-1,50` tetap `-1,5` di kiriman | Skenario draf |
| Tombol mengikuti status dan izin (`OPT-FE-005`) | Terpenuhi | Uji unit; skenario izin |
| `UAT-03`: final lalu revisi dua kali, tiga versi di riwayat | Terpenuhi di E2E API tiruan untuk layar. Satu tarif di kasir terbukti di harness `BE-OPT-031` | Skenario riwayat; `OPT-AC-024` |
| `UAT-04`: SIP berakhir, mulai ditolak dengan tanggalnya | Terpenuhi di E2E API tiruan | Skenario mulai |
| `OPT-AC-001`: bukan RO/Sp.M ditolak | Terpenuhi, termasuk dengan backend asli | `403` superadmin |
| DoD: tidak ada data sensitif di `console` | Terpenuhi | Skenario privasi |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Kode profesi RO/Sp.M kosong.** Selama `OpticalManagement:Practitioner:RefractionistProfessionCodes` dan `OphthalmologistSpecializationCodes` tidak diisi, tidak ada akun yang dapat memulai pemeriksaan atau menulis resep (`OPT-DEC-058`). Nilainya perlu ditetapkan pemilik modul per lingkungan, dan pegawainya perlu `ProfessionId` serta SIP dan STR terverifikasi di HR |
| Delta brief | (1) *Catatan Keratometri* dan *Catatan Klinis* ditambahkan ke form walau tidak ada di sketsa brief, karena ada di kontrak dan akan terhapus bila tidak dikirim. (2) Peringatan tagihan juga tampil saat halaman dimuat ulang (`billingFactEmittedAt` kosong), dengan kalimat layar karena `billingMessage` tidak ikut pada `GET`. (3) *Buat Resep* dan *Buat Resep dari Pemeriksaan* tetap nonaktif sampai `FE-OPT-012` |
| Perilaku yang perlu diketahui | Sesudah `409` basi, isian yang belum tersimpan digantikan data terbaru, sesuai brief §6; petugas perlu mengetik ulang perubahannya |
| Data uji | Tidak ada tulisan baru di devYoga; jumlah pemeriksaan tetap 0 |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Dependency backend | `NONE` untuk kode. Konfigurasi kode profesi di atas menahan pemakaian nyata |
| Perubahan sampingan | `NONE`. Tiga mutasi uji dibuat dan dipulihkan; Playwright memakai `--output` scratchpad |
| Interupsi | `NONE` |
| Status Git | Di luar berkas `FE-OPT-010`: `??` `optical-exam-constants.js`, `optical-exam-rules.js`, `optical-version-compare-rules.js`, dua berkas uji baru; folder Optik yang sudah `??` bertambah route, view, dan hook |
| Langkah berikutnya | `FE-OPT-012` (form resep, salin resep luar, riwayat versi, cetak resep) |
