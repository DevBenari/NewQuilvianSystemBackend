# Laporan Perubahan Frontend — `FE-RWI-047`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-047` |
| Judul | Riwayat visite beserta pencatatan dan pembatalannya |
| Slice | `DOK-MVP-FE` urutan 5 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap.md` §3 kartu `FE-RWI-047` |
| Trace | `FE-DOK-05`; `03-frontend-architecture.md` §3.5; `RWI-DEC-084`, `RWI-DEC-085`; `RWI-AC-150` s.d. `RWI-AC-156`; `VAL-DOK-27` |
| Contract version | `0.3.0` — `approved` oleh Muhammad Hamzah, 3 September 2026 |
| Wewenang UI | `skema-tampilan-dokter-rawat-inap.md` §4–5, §10, §16–20, §22 dan rules §1 roadmap |
| Dependency | `FE-RWI-043` ✅ selesai; `BE-RWI-048` 🟡 sebagian — uji concurrency PostgreSQL dilewati atas keputusan pemilik 5 September 2026; `BE-RWI-049` ✅ selesai |
| Klasifikasi | `MEDIUM` — satu service baru, satu hook, satu utility, satu constant, empat komponen domain, satu stylesheet. Tidak ada base component baru |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | Dikerjakan di atas `52b07d363e92525739fb2ad63075ec80f6d4e230`, branch `HamzahV2`. Source-nya kemudian **di-commit pemilik pekerjaan sendiri** sebagai `e194509dc`; agent tidak menjalankan satu pun tindakan Git |
| Commit backend yang dijadikan rujukan | `3a6373e90e5a590bfad1ba214c5c941e602fc245`, branch `MHamzah` |
| Tanggal | 8 September 2026 |
| Status | ✅ `SELESAI`. Keenam acceptance criteria terpetakan ke source yang ada dan seluruh validasi dijalankan. Butir DoD bukti visual dikecualikan atas keputusan pengguna bahwa e2e dan `.mjs` bukan gerbang selesai. Dua isu §6 — hak Cancel konsulen dan mutasi event pada episode Closed — **tetap terbuka** dan tidak dinyatakan lulus |

---

## 1. Keadaan yang ditemukan di awal

Tab **Visite** masih berupa kerangka `ClinicalTabPlaceholder`. Di sisi frontend belum ada satu pun
berkas yang menyebut visite: `grep -rn "physician-visit" src/lib/services` mengembalikan nol hasil.

Yang paling berbahaya dari keadaan awal ini bukan tab yang kosong, melainkan **anggapan bahwa
visite dapat dihitung dari catatan SOAP**. Backend sudah menutup anggapan itu sejak `BE-RWI-048`
dengan membuat visite sebagai kejadian tersendiri, dan `INV-DOK-07` melarang menghitungnya dari
catatan. Selama tab ini kosong, satu-satunya jejak kunjungan dokter di layar adalah catatan
perkembangan — dan itulah yang membuat "tiga catatan perkembangan" mudah disalahbaca sebagai
"tiga kali visite".

Backend sudah siap sepenuhnya: delapan endpoint pada
`Areas/HealthServices/ClinicalManagement/Controllers/PhysicianVisitController.cs`, termasuk
`GET /episodes/{episodeId}` yang mengembalikan kejadian yang dibatalkan beserta alasannya.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter yang berwenang atas pasien — DPJP, konsulen, atau dokter jaga sesuai
kewenangannya masing-masing.

**Kapan layar ini dibuka.** Setiap kali dokter selesai mengunjungi pasien dan perlu mencatat
kunjungannya, atau ketika seseorang perlu memeriksa berapa kali pasien benar-benar dikunjungi.

**Langkah normalnya, berurutan:**

1. Dokter membuka tab **Visite** pada ruang kerja pasien.
2. Ia melihat **Riwayat Visite** berupa daftar kejadian, terurut menurut waktu kunjungan. Setiap
   baris memuat waktu, nama dokter, perannya, catatan bila ada, dokumen yang tertaut, siapa yang
   mencatat, dan status kejadiannya.
3. Untuk mencatat kunjungan baru ia menekan **Catat Visite**. Modal terbuka berisi empat isian:
   **Waktu Visite** (sudah terisi waktu sekarang dan boleh diubah mundur), **Peran**, **Catatan**,
   dan **Tautkan Dokumen** yang sifatnya opsional.
4. Bila ada visite lain pada jam berdekatan, modal menampilkan **peringatan** — misalnya
   *"Visite lain tercatat 07 Sep 2026 07.20, sekitar 20 menit dari waktu yang Anda isi. Anda tetap
   dapat melanjutkan."* Peringatan itu **tidak** menonaktifkan tombol apa pun. Dua visite nyata
   pada hari yang sama memang boleh, dan `RWI-DEC-085` melarang menolaknya.
5. Menekan **Catat Visite** mengirim permintaan beserta kunci permintaan. Tombolnya nonaktif
   selama permintaan berjalan.

**Membatalkan kunjungan yang salah catat.** Tombol **Batalkan** pada baris kejadian membuka modal
yang menuntut alasan. Tombol simpannya mati selama alasan masih kosong. Sesudah dibatalkan, baris
itu **tetap berdiri di riwayat** — diredupkan, diberi penanda "Dibatalkan", dan membawa alasan,
nama pembatal, serta waktu pembatalannya.

**Membetulkan waktu atau peran.** Tidak ada tombol **Sunting**, dan itu disengaja. Yang salah
catat dibatalkan beralasan, lalu dicatat ulang. Kalimat itu tertulis di bawah daftar supaya dokter
tidak mencari tombol yang memang tidak ada.

**Jalur tidak normal:**

- **Belum ada visite** — "Belum ada visite tercatat." beserta penjelasan bahwa catatan
  perkembangan yang sudah ditulis **tidak** dihitung sebagai visite. Kalimat ini tetap berbunyi
  demikian walaupun pasien sudah punya tiga catatan perkembangan.
- **Gagal memuat riwayat** — "Riwayat visite tidak dapat dimuat" beserta **Coba Lagi**.
- **Gagal mencatat atau membatalkan** — pesan galat muncul terpisah dari riwayat, dan riwayatnya
  tetap terbaca.
- **Tanpa hak baca** — gerbang penolakan tanpa membocorkan data.
- **Tanpa hak tulis** — tombol Catat Visite dan Batalkan tidak ditampilkan sama sekali, dan
  alasannya dikatakan sekali di atas daftar lewat `ClinicalActionGuard`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/frontend-roadmap.md` kartu `FE-RWI-047`, rules §1.1–§1.4, dan §6 baris konsulen
- `contracts/api-contract.md` §4 beserta §4.1 dan §4.2
- `skema-tampilan-dokter-rawat-inap.md` §10
- `Areas/HealthServices/ClinicalManagement/Controllers/PhysicianVisitController.cs` (read-only)
- `Areas/HealthServices/ClinicalManagement/DTOs/PhysicianVisitDtos.cs` (read-only)
- `Areas/HealthServices/ClinicalManagement/Enums/PhysicianVisitRole.cs`,
  `PhysicianVisitStatus.cs` (read-only)
- `roadmap/backend-roadmap.md` untuk status `BE-RWI-048` dan `BE-RWI-049`
- `modals/correction-modal.jsx` sebagai pola modal berisian, dan `confirm-modal.jsx` untuk
  `requireReason`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/clinical-management/physician-visit.service.js` | **Baru.** Lima fungsi: baca per episode, baca satu event, catat, batalkan, dan perbarui tautan. Kunci permintaan dikirim pada badan permintaan **dan** header `Idempotency-Key` sekaligus, mengikuti kontrak §4 yang menerima keduanya |
| `src/lib/constants/health-services/inpatient-management/inpatient-physician-visit-constants.jsx` | **Baru.** Enum peran dan status, pilihan tautan dokumen, ambang peringatan visite berdekatan, dan seluruh salinan teks |
| `src/utils/health-services/inpatient-management/inpatient-physician-visit-utils.jsx` | **Baru.** Normalisasi kejadian, penyusun kunci permintaan, pencari visite berdekatan, dan penyusun payload |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-physician-visit.jsx` | **Baru.** Membaca riwayat dengan `includeCancelled`, mencatat, dan membatalkan |
| `…/tabs/physician-visit/physician-visit-tab.jsx` | Kerangka diganti isi sebenarnya |
| `…/tabs/physician-visit/physician-visit-timeline.jsx` | **Baru.** Event timeline; kejadian batal diredupkan dan diberi penanda, bukan disembunyikan |
| `…/tabs/physician-visit/record-visit-modal.jsx` | **Baru.** Empat isian beserta peringatan visite berdekatan |
| `…/tabs/physician-visit/cancel-visit-modal.jsx` | **Baru.** Alasan wajib lewat `requireReason` milik `ConfirmModal` |
| `src/style/health-services/inpatient-management/physician-visit.module.css` | **Baru.** Seluruh nilai visual memakai token |
| `tests/unit/inpatient-physician-clinical-tabs.test.mjs` | Empat uji khusus task ini ditambahkan |

### 3.3 Kepatuhan arsitektur frontend

Alur `constants → utils → service → hook → view` diikuti penuh. Base component yang dipakai ulang:
`ClinicalTimeline`, `ClinicalTimelineItem`, `ClinicalDocumentMeta`, `ClinicalAuditBadge` dari
`FE-RWI-045`; `ClinicalStateBoundary`, `ClinicalStatusBadge`, `ClinicalEmptyState`,
`ClinicalActionGuard` dari `FE-RWI-042`/`FE-RWI-043`; `BaseButton`, `BaseTextAreaField`,
`BaseNativeSelectField`, `BaseTextField`, `ConfirmModal` dari base features.

**Nol base component baru dibuat**, sesuai roadmap §1.2 yang menyatakan task ini hanya memakai
ulang base milik task lain. `ClinicalTimeField` dipakai ulang dari folder `progress-note`
`FE-RWI-045`, bukan disalin.

**Catatan tentang kunci permintaan.** Kuncinya **diturunkan dari isi kiriman**, bukan diacak
setiap kali tombol ditekan. Bentuknya `visit-<episodeId>-<waktu tanpa pemisah>-<peran>`, ditambah
`-<correctsVisitId>` bila pencatatan itu koreksi. Konsekuensinya persis seperti yang dituntut
`RWI-AC-152` dan `RWI-AC-155`: kiriman ulang atas isi yang sama membawa kunci yang sama sehingga
server mengembalikan kejadian yang sama, sementara dua kunjungan yang benar-benar berbeda
menghasilkan dua kunci berbeda dan tetap tersimpan sebagai dua baris.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membuka riwayat visite..." beserta kerangka baris |
| Kosong | "Belum ada visite tercatat." beserta "Catatan perkembangan yang sudah ditulis tidak dihitung sebagai visite. Kunjungan dicatat sebagai kejadian tersendiri lewat tombol Catat Visite." |
| Gagal | "Riwayat visite tidak dapat dimuat" beserta **Coba Lagi** |
| Gagal mencatat atau membatalkan | Pesan galat server ditampilkan terpisah; riwayat dan isian modal tetap utuh |
| Tanpa hak akses | "Akses riwayat visite ditolak" tanpa membocorkan data |
| Tanpa hak tulis | Tombol Catat Visite dan Batalkan tidak dirender; alasannya dikatakan sekali lewat penjaga kewenangan |
| Episode ditutup | Jalur tulis tertutup lewat penjaga yang sama; riwayat tetap terbaca sepenuhnya |
| Kiriman ganda | Tombol **Catat Visite** dan **Batalkan Visite** nonaktif selama permintaannya berjalan, dan kunci permintaan menjamin kiriman ulang menghasilkan satu kejadian |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Physician Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/physician-visits/episodes/{episodeId}` | Membaca riwayat kunjungan satu perawatan, `includeCancelled=true` supaya kejadian yang dibatalkan ikut terbaca | `PhysicianVisit : Read` |
| `POST` | `/v1/health-services/clinical-management/physician-visits` | Mencatat kejadian kunjungan; kunci permintaan wajib | `PhysicianVisit : Create` |
| `PATCH` | `/v1/health-services/clinical-management/physician-visits/{id}/cancel` | Membatalkan kejadian yang salah catat beserta alasan wajib | `PhysicianVisit : Cancel` |
| `PATCH` | `/v1/health-services/clinical-management/physician-visits/{id}/links` | Menautkan dokumen klinis pada kejadian; tersedia di service, belum dipakai layar | `PhysicianVisit : Update` |

**Delta kontrak yang dicatat:** `api-contract.md` §4 menandai seluruh grup sebagai
**Rencana (belum tersedia)**. Pada backend SHA rujukan, kelima endpoint itu **sudah tersedia**,
dibuat `BE-RWI-048` dan `BE-RWI-049`. Backend juga menyediakan tiga endpoint baca tambahan
(`filters/metadata`, `summary`, dan daftar umum) di luar lima yang tercantum kontrak — selisih
yang sudah dicatat pada laporan `BE-RWI-048`. Layar ini hanya memakai endpoint yang memang
tercantum kontrak.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error | `PASS` | Keluaran `eslint . --quiet` kosong |
| `npm run test:unit` | 542 uji lulus, 0 gagal | `PASS` | `tests 542 / pass 542 / fail 0`; 21 di antaranya uji baru `tests/unit/inpatient-physician-clinical-tabs.test.mjs` |
| `npm run build` | Berhasil beserta `postbuild` | `PASS` | `✓ Compiled successfully in 33.2s` |
| Kiriman ulang memakai kunci yang sama; dua visite berbeda memakai kunci berbeda; koreksi memakai kunci baru; kunci ≤ 100 huruf | Keempatnya sesuai | `PASS` | `FE-RWI-047 K3` |
| Visite berdekatan diperingatkan; visite berjauhan tidak; kejadian yang dibatalkan tidak memicu peringatan palsu | Ketiganya sesuai | `PASS` | `FE-RWI-047 K4` |
| Kejadian yang dibatalkan tetap terbaca beserta alasannya | Baris tidak dihapus; `cancelReason` terbaca | `PASS` | `FE-RWI-047 K1` |
| Pemindaian tombol Sunting/Edit pada keempat berkas tab | Nol hasil | `PASS` | `FE-RWI-047 K6` |
| Grep warna literal dan `!important` pada stylesheet baru | Nol hasil | `PASS` | Grep checklist konsistensi UI |
| Verifikasi interaktif di peramban | Tidak dijalankan | `NOT RUN` | Lihat catatan di bawah |
| Perilaku serentak dua permintaan sungguhan | Tidak dijalankan | `NOT RUN` | Bukti concurrency masih terbuka di `BE-RWI-048` |

**Uji manual:** `NOT FEASIBLE`.

**Alasan konkret.** Repository tidak memiliki `playwright.config.*` di akar sehingga
`npm run test:e2e` tidak dapat dijalankan tanpa menambah konfigurasi baru. Pengujian matriks
kewenangan yang bermakna juga menuntut lingkungan uji dengan peran DPJP, konsulen, dokter jaga,
dan supervisor yang terpisah — gerbang yang masih terbuka pada roadmap §4.

**Tidak dijalankan:** `npm run test:e2e`, `npm run test:uat`, screenshot tiga viewport, dan uji
dua permintaan serentak. Yang terakhir memang belum dapat dibuktikan dari sisi frontend selama
bukti concurrency `BE-RWI-048` masih terbuka; kunci permintaan yang stabil sudah diuji, tetapi
perilakunya di bawah dua permintaan serentak **tidak** diklaim terbukti.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Riwayat menampilkan kejadian yang **dibatalkan beserta alasannya**, tidak disembunyikan | Terpenuhi | Service memanggil dengan `includeCancelled: true`; `PhysicianVisitTimeline` merender baris batal dengan `muted`, `ClinicalAuditBadge` bertone `cancelled`, dan blok alasan bertestid `physician-visit-cancel-reason-<id>`. Uji `FE-RWI-047 K1` |
| 2. Keadaan kosong berbunyi "belum ada visite tercatat" **walaupun sudah ada tiga catatan perkembangan** | Terpenuhi | Daftar dibaca dari endpoint visite, sama sekali tidak dari catatan SOAP. Kalimat kosongnya menyebut visite secara eksplisit dan menjelaskan bahwa catatan perkembangan tidak dihitung |
| 3. Tombol Catat Visite nonaktif selama permintaan berjalan, dan penekanan dua kali menghasilkan satu kejadian | Terpenuhi | `ConfirmModal` menerima `loading={recording}`; `BaseButton` menonaktifkan diri saat `loading`. Kunci permintaan diturunkan dari isi, sehingga kiriman kedua membawa kunci yang sama. Uji `FE-RWI-047 K3` |
| 4. Visite pada jam berdekatan **diperingatkan, bukan ditolak**, dan dapat dilanjutkan | Terpenuhi | `findNearbyVisit` hanya menghasilkan peringatan; `disabled` pada modal ditentukan `issues.length > 0`, dan peringatan berdekatan **tidak** masuk ke `issues`. Uji `FE-RWI-047 K4` |
| 5. Tombol Batalkan menuntut alasan; tombol simpan nonaktif selama alasan kosong | Terpenuhi | `CancelVisitModal` memakai `requireReason` milik `ConfirmModal`, yang sudah menonaktifkan tombol konfirmasi selama alasan kosong |
| 6. **Tidak ada tombol Sunting** pada kejadian visite | Terpenuhi | Uji pemindaian `FE-RWI-047 K6` atas keempat berkas tab mengembalikan nol hasil. Service pun tidak memuat fungsi penyunting waktu maupun peran, karena endpoint-nya memang tidak ada |

### Butir Definition of Done

| Butir | Status |
| --- | --- |
| Keenam acceptance existing terbukti | Terpenuhi |
| Seluruh state dan permission terbukti | Terpenuhi pada source dan tabel bagian 4; belum diverifikasi di peramban |
| Visual Acceptance Criteria terbukti | **Belum diverifikasi di peramban.** Dikecualikan atas keputusan pengguna bahwa e2e dan `.mjs` bukan gerbang selesai |
| Gate §4.1 relevan (butir 11, 12, 19) | Terbukti pada source: kejadian batal tetap terlihat beserta alasan, tidak ada tombol Edit, tidak ada global finalize |
| Laporan menampilkan cancelled event dan modal nyata | **Belum** — menunggu bukti visual |
| Dependency `BE-RWI-048`/`BE-RWI-049` tetap | Terpenuhi. `BE-RWI-048` 🟡 dicatat apa adanya, termasuk bukti concurrency yang masih terbuka |
| Isu authority §6 tidak dianggap selesai | Terpenuhi — lihat bagian 8 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu peringatan ESLint `react-hooks/set-state-in-effect` muncul pada versi pertama `RecordVisitModal`, karena isian disiapkan ulang di dalam `useEffect`. **Diperbaiki** dengan memindahkan penyiapan ulang ke fase render, sehingga tidak ada satu frame pun yang masih memegang waktu kunjungan sebelumnya saat modal dibuka kembali |
| Masalah yang diketahui | `api-contract.md` §4 masih menandai seluruh grup sebagai **Rencana**, padahal endpoint-nya sudah hidup. Selisih dokumen dilaporkan, tidak diperbaiki dari sini |
| Dependency backend | `BE-RWI-048` 🟡 **sebagian** — butir "test concurrency PostgreSQL hijau" belum terpenuhi, dilewati atas keputusan pemilik 5 September 2026. Dampaknya di layar: kunci permintaan sudah stabil dan teruji, tetapi perilakunya di bawah dua permintaan serentak sungguhan belum dibuktikan dari sisi mana pun. `BE-RWI-049` ✅ selesai |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Saat pekerjaan agent selesai, berkas task ini muncul sebagai `??` dan `M` pada branch `HamzahV2`, dan **tidak ada tindakan Git yang dijalankan agent** — tanpa `git add`, commit, push, merge, maupun rebase. Pemilik pekerjaan kemudian meng-commit sendiri sebagai `e194509dc`, sehingga `git status --short` pada repository frontend kini bersih |
| Langkah berikutnya | Meminta keputusan pemilik atas dua isu §6 di bawah, lalu menjalankan verifikasi interaktif begitu lingkungan uji berperan terpisah tersedia |

### Isu §6 yang **tetap terbuka** dan tidak dinyatakan lulus

| Isu | Keadaan sekarang di layar | Yang dibutuhkan |
| --- | --- | --- |
| Hak **Cancel** bagi dokter konsulen. Arsitektur §4 mengizinkan, `permission-audit-matrix.md` §2 hanya memberi konsulen Read dan Create, `state-transition-matrix.md` §5 menuntut pemilik event atau supervisor | Layar **tidak** memutuskan sendiri dari nama peran. Tombol Batalkan mengikuti kewenangan menulis pada episode yang sudah dihitung shell `FE-RWI-043`, dan penolakan akhirnya tetap milik server | Keputusan Product/Domain beserta pemilik permission tentang hak konsulen |
| Mutasi event pada episode **Closed**. Skema §17 menyebut Closed hanya-baca kecuali koreksi final; state §5 dan API §4 tidak merinci eligibility Cancel sesudah Closed | Layar memakai baseline **Closed hanya-baca**: jalur tulis tertutup, riwayat tetap terbaca. Tidak ada mutasi yang dibuka hanya berdasarkan permission umum | Keputusan lifecycle aksi spesifik dari Product/Domain dan ClinicalManagement |
