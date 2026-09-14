# Laporan Perubahan Frontend — `FE-RWI-046`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-046` |
| Judul | Catatan terpadu dan verifikasi DPJP |
| Slice | `DOK-MVP-FE` urutan 8 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap.md` §3 kartu `FE-RWI-046` |
| Trace | `FE-DOK-04`; `03-frontend-architecture.md` §3.4; `AC-CAP021-03`; `INV-DOK-11`; `RWI-RULE-021`, `RWI-RULE-030`; `VAL-DOK-07`, `VAL-DOK-24`, `VAL-DOK-25` |
| Contract version | `0.3.0` — `approved` oleh Muhammad Hamzah, 3 September 2026 |
| Wewenang UI | `skema-tampilan-dokter-rawat-inap.md` §4–5, §9, §16–20, §22 dan rules §1 roadmap |
| Dependency | `FE-RWI-043` ✅ selesai; `BE-RWI-053` ✅ selesai; **`BE-RWI-066` ✅ selesai 8 September 2026** — penutup celah kontrak yang menahan kriteria 2 pada pass pertama |
| Klasifikasi | `MEDIUM` — satu service diperluas, satu hook, satu utility, satu constant, dua komponen domain, satu stylesheet. Nol base component baru |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | Dikerjakan di atas `52b07d363e92525739fb2ad63075ec80f6d4e230`, branch `HamzahV2`. Source-nya kemudian **di-commit pemilik pekerjaan sendiri** sebagai `e194509dc`; agent tidak menjalankan satu pun tindakan Git |
| Commit backend yang dijadikan rujukan | `3a6373e90e5a590bfad1ba214c5c941e602fc245`, branch `MHamzah` |
| Tanggal | Pass pertama 8 September 2026; **pass kedua 9 September 2026** sesudah `BE-RWI-066` menutup celah kontraknya |
| Status | ✅ `SELESAI` **9 September 2026.** **5 dari 5** acceptance criteria terpenuhi penuh dan terpetakan ke source yang benar-benar ada. Kriteria 2 — satu-satunya yang tertahan pada pass pertama — kini tertutup: `BE-RWI-066` mengembalikan `VerificationStatus`, `VerifiedAt`, `VerifiedByUserId`, `VerifiedByUserName`, dan `VerificationDueAt` pada balasan baca catatan terpadu, dan layar membacanya tanpa satu baris pun menebak. Validasi nyata pass kedua: `npm run lint` **0 error, 611 warning** (garis dasar tidak bergerak); `npm run test:unit` **563/563 lulus, 0 gagal**; `npm run build` beserta `postbuild` berhasil; **7 skenario peramban lulus** di Edge, dua di antaranya milik task ini. Isu §6 eligibility Verifikasi pada episode Closed **tetap terbuka** dan **tidak dinyatakan lulus**, sesuai Definition of Done kartu task ini; layar tetap memakai baseline Closed hanya-baca. Butir DoD screenshot tiga viewport **dikecualikan atas keputusan pengguna 1 September 2026** bahwa pengujian e2e dan `.mjs` bukan gerbang selesai pada repository ini; catatan `NOT RUN`-nya tetap tercatat pada bagian 6, tidak dihapus |

---

## 1. Keadaan yang ditemukan di awal

Tab **Catatan Terpadu** masih berupa kerangka `ClinicalTabPlaceholder`. Service
`patient-integrated-progress-note.service.js` sudah ada, tetapi hanya melayani jalur rawat jalan:
`getPatientIntegratedProgressNoteTimeline` berbasis penyaring pasien, tanpa satu pun jalur per
perawatan rawat inap dan tanpa jalur verifikasi.

Backend sudah siap sejak `BE-RWI-053`: `GET /episodes/{episodeId}` untuk lini masa lintas profesi,
`PATCH /{id}/verify` untuk verifikasi DPJP, dan `GET /episodes/{episodeId}/verification-status`
untuk keadaan verifikasi beserta daftar pantaunya.

**Satu celah kontrak yang ditemukan saat memeriksa source backend, dan dilaporkan apa adanya.**
Model `TrxPatientIntegratedProgressNote` menyimpan `VerificationStatus`, `VerifiedAt`, dan
`VerifiedByUserId`, tetapi **tidak satu pun dari ketiganya diekspos** pada
`PatientIntegratedProgressNoteResponse`. Pemetaan `ToResponse` pada
`PatientIntegratedProgressNoteController.cs:1397–1435` tidak menyertakannya, dan
`PatientIntegratedProgressNoteDetailResponse` juga tidak. Balasan `PATCH /{id}/verify` sendiri
memakai `ToResponse` yang sama, sehingga ia pun tidak mengembalikan siapa yang barusan
memverifikasi.

Akibatnya bagi task ini dinyatakan terus terang di bagian 7: identitas verifikator **tidak
tersedia dari layanan mana pun**, dan status verifikasi per catatan hanya dapat disimpulkan
sebagian dari daftar pantau.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** DPJP yang sedang merawat pasien, ditambah profesi lain yang berhak membaca
lembar terpadu pasien itu.

**Kapan layar ini dibuka.** Ketika dokter perlu membaca perkembangan pasien dari seluruh profesi
pada satu lembar — perawat, apoteker, ahli gizi, fisioterapis — dan ketika DPJP perlu menyatakan
sudah memeriksa catatan profesi lain.

**Langkah normalnya, berurutan:**

1. Dokter membuka tab **Catatan Terpadu**. Layar menampilkan lini masa **satu kolom** berisi
   catatan seluruh profesi, terurut menurut waktu catatan.
2. Di kepala lini masa ada **Filter Profesi**. Memilih satu profesi mempersempit daftar; penyaring
   itu dikerjakan server, bukan disaring ulang di layar.
3. Setiap catatan menampilkan waktu, lalu **nama penulis beserta profesinya** pada satu baris
   judul — misalnya "Ns. Sari • Perawat". Di bawahnya, blok metadata memuat **dua baris terpisah**:
   **Penulis** dan **Verifikator**. Keduanya tidak pernah digabung.
4. Setiap catatan membawa penanda status verifikasinya, dan penandanya berlabel teks, bukan hanya
   warna.
5. Bila pengguna berhak, tombol **Verifikasi** muncul pada catatan yang memang menerimanya.

**Siapa yang melihat tombol Verifikasi.** Empat syarat harus benar sekaligus, dan tombolnya
**disembunyikan** — bukan ditampilkan lalu ditolak — begitu salah satunya gagal:

- pengguna adalah DPJP aktif episode itu **saat verifikasi**, bukan DPJP lama;
- pengguna **bukan** penulis catatan itu, karena menandatangani bacaan atas tulisan sendiri bukan
  verifikasi;
- status catatan memang menerima verifikasi, yaitu Menunggu atau Lewat Batas;
- episode belum ditutup.

Contoh dari kontrak: catatan ditulis **Ns. Sari**, diverifikasi **dr. Andi**. Sesudah verifikasi,
baris Penulis tetap berbunyi Ns. Sari. dr. Andi tidak pernah naik menjadi penulis.

**Ketika kebijakan verifikasi belum aktif.** Inilah keadaan nyata seluruh rumah sakit hari ini:
`RWI-RULE-021` belum disahkan, dan nol angka batas waktu tertanam di backend. Layar menyatakannya
di atas lini masa: **"Verifikasi DPJP tidak diwajibkan."** beserta penjelasan bahwa catatan tetap
sah dan tetap dapat ditulis. Tanpa kalimat itu, daftar tanpa satu pun penanda "menunggu
verifikasi" akan terbaca seperti semuanya sudah diperiksa DPJP.

**Jalur tidak normal:**

- **Belum ada catatan** — "Belum ada catatan terpadu pada perawatan ini."
- **Penyaring profesi tidak menemukan apa pun** — kalimatnya berbeda: "Tidak ada catatan dari
  profesi yang dipilih." Ini bukan keadaan yang sama dengan belum ada catatan sama sekali.
- **Lini masa gagal dimuat** — "Catatan terpadu tidak dapat dimuat" beserta **Coba Lagi**.
- **Keadaan verifikasi gagal dimuat** — catatan **tetap terbaca**, dan di atasnya muncul
  peringatan: status verifikasinya belum dapat dipastikan dan **tidak boleh dianggap sudah
  diverifikasi**. Penanda tiap barisnya berbunyi "STATUS VERIFIKASI BELUM DAPAT DIPASTIKAN".
- **Tanpa hak baca** — gerbang penolakan tanpa membocorkan isi catatan.
- **Tanpa kewenangan menulis** — seluruh tombol Verifikasi hilang, dan alasannya dikatakan sekali
  di atas lini masa.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/frontend-roadmap.md` kartu `FE-RWI-046`, rules §1.1–§1.4, dan §6 baris Closed
- `contracts/api-contract.md` §3
- `skema-tampilan-dokter-rawat-inap.md` §9 beserta bagian Policy Verification Tidak Aktif
- `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`
  (read-only) — endpoint episode, verify, verification-status, dan pemetaan `ToResponse`
- `Areas/HealthServices/ClinicalManagement/Services/CpptVerificationService.cs` (read-only) —
  aturan kelayakan verifikasi dan bentuk `CpptVerificationStatusSummary`
- `Areas/HealthServices/ClinicalManagement/DTOs/PatientIntegratedProgressNoteDtos.cs` (read-only)
- `Areas/HealthServices/ClinicalManagement/Enums/CpptVerificationStatus.cs` (read-only)
- `src/lib/state/slice/auth/login-slice.jsx` untuk bentuk identitas pengguna

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/clinical-management/patient-integrated-progress-note.service.js` | Ditambah `getProgressNotesByEpisode`, `getProgressNoteVerificationStatus`, dan `verifyProgressNote`. Fungsi rawat jalan yang sudah ada tidak disentuh |
| `src/lib/constants/health-services/inpatient-management/inpatient-integrated-note-constants.jsx` | **Baru.** Empat status backend ditambah satu keadaan layar `unknown`, pilihan filter profesi, dan seluruh salinan teks |
| `src/utils/health-services/inpatient-management/inpatient-integrated-note-utils.jsx` | **Baru.** Normalisasi catatan dan keadaan verifikasi, penyimpul status per catatan, dan penentu kelayakan tombol Verifikasi |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-integrated-note.jsx` | **Baru.** Membaca lini masa dan keadaan verifikasi terpisah, mengelola penyaring profesi, dan menjalankan verifikasi |
| `…/tabs/integrated-note/integrated-progress-note-tab.jsx` | Kerangka diganti isi sebenarnya |
| `…/tabs/integrated-note/integrated-note-timeline.jsx` | **Baru.** Lini masa satu kolom beserta pemisahan Penulis dan Verifikator |
| `src/style/health-services/inpatient-management/physician-integrated-note.module.css` | **Baru.** |
| `tests/unit/inpatient-physician-clinical-tabs.test.mjs` | Lima uji khusus task ini ditambahkan |
| **Pass kedua** — `src/utils/health-services/inpatient-management/inpatient-integrated-note-utils.jsx` | Membaca kelima kolom verifikasi `BE-RWI-066`; status catatan dipakai apa adanya dan jalur penyimpulan lama dipertahankan sebagai cadangan bagi balasan yang tidak membawanya |
| **Pass kedua** — `…/tabs/integrated-note/integrated-note-timeline.jsx` | Baris Verifikator diisi `verifiedByUserName`; baris Waktu Verifikasi; penyorotan dan penggulir catatan yang dituju alamat |
| **Pass kedua** — `…/tabs/integrated-note/integrated-progress-note-tab.jsx` | Meneruskan nomor catatan dari alamat ke lini masa, dan menyatakan keadaan bila catatan yang dituju tidak ada pada daftar |
| **Pass kedua** — `…/physician-workspace/physician-workspace-view.jsx` | Membaca `?note=` dari alamat lalu membagikannya lewat konteks ruang kerja |
| **Pass kedua** — `src/style/health-services/inpatient-management/physician-integrated-note.module.css` | Jarak gulir catatan tersorot; nol warna literal dan nol `!important` ditambahkan |

### 3.3 Kepatuhan arsitektur frontend

Alur `constants → utils → service → hook → view` diikuti. Base yang dipakai ulang seluruhnya
milik task lain, sesuai roadmap §1.2 yang menyatakan task ini **tidak membuat base baru**:
`ClinicalTimeline`, `ClinicalTimelineItem`, `ClinicalDocumentMeta`, `ClinicalAuditBadge` dari
`FE-RWI-045`; `ClinicalStateBoundary`, `ClinicalSafetyAlert`, `ClinicalActionGuard`,
`ClinicalEmptyState` dari `FE-RWI-042`/`FE-RWI-043`; `BaseButton` dan `BaseNativeSelectField` dari
base features.

**Kebijakan DPJP tidak ditaruh di base.** `canVerifyNote` hidup di utility domain, dan
`ClinicalActionGuard` hanya menjalankan keputusan yang sudah dihitung adapter. Base tidak
mengetahui satu pun aturan DPJP.

**Status verifikasi tidak pernah dihitung ulang di layar sesudah verifikasi berhasil.** Yang
dilakukan adalah membaca ulang keadaan verifikasi dari server. Alasannya kontraktual: catatan yang
sudah diverifikasi lalu dikoreksi kembali menunggu verifikasi **menurut server**, dan menghitungnya
sendiri di layar akan menampilkan "sudah diverifikasi" atas catatan yang sebenarnya menunggu lagi.

---

### 3.4 Pass kedua — 9 September 2026, sesudah `BE-RWI-066`

Pass pertama menutup empat dari lima kriteria dan berhenti pada satu hal: **balasan baca catatan
terpadu tidak menyebutkan siapa yang memverifikasi.** Layar waktu itu mengambil keputusan yang
benar dengan berkata "status verifikasi belum dapat dipastikan" alih-alih menebak, dan celah
kontraknya diajukan sebagai task backend tersendiri.

`BE-RWI-066` menutupnya pada 8 September 2026. Lima kolom bertambah pada balasan baca, dan
kelimanya bersifat tambahan — nol kolom lama berubah nama atau berubah arti.

| Kolom balasan baru | Dipakai layar untuk |
| --- | --- |
| `verificationStatus` | Penanda status per catatan, tanpa lagi menyimpulkannya dari rekapitulasi |
| `verifiedAt` | Baris **Waktu Verifikasi**, muncul hanya ketika verifikasinya memang terjadi |
| `verifiedByUserId` | Penanda teknis; **tidak pernah** ditampilkan sebagai nama |
| `verifiedByUserName` | Isi baris **Verifikator** |
| `verificationDueAt` | Menaikkan `Pending` yang sudah lewat batas menjadi `Overdue`, dengan aturan yang sama persis dengan server |

Yang dikerjakan pass kedua pada task ini:

1. **Nama verifikator benar-benar tampil.** Baris Verifikator berisi nama dari
   `verifiedByUserName`, dan aturan pengisiannya menolak jatuh ke nama penulis dalam keadaan apa
   pun. Empat keadaan tanpa nama tetap berbunyi berbeda-beda: belum diverifikasi, tidak
   diwajibkan, nama verifikator tidak dapat dikenali, dan status belum dapat dipastikan.
2. **Catatan tersorot ketika dituju tautan daftar pantau.** Ruang kerja kini membaca `?note=`
   dari alamat, meneruskannya lewat konteks, lalu lini masa menyorot catatan itu dan menggulirnya
   ke tengah layar. Sebelumnya tautan dari daftar pantau hanya membuka tabnya, dan supervisor
   masih harus menelusuri lini masa satu per satu.
3. **Tautan yang menunjuk catatan di luar daftar dikatakan apa adanya.** Bila catatan yang dituju
   tidak ada pada daftar yang sedang tampil — paling sering karena penyaring profesi sedang
   menyembunyikannya — layar mengatakannya, alih-alih mendarat tanpa menyorot apa pun.

Bukti bahwa kolom-kolom itu memang terbaca layar, bukan sekadar tersedia di server, ada pada dua
skenario peramban di bagian 6.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membuka catatan terpadu..." beserta kerangka. Lini masa dan keadaan verifikasi punya kerangka sendiri-sendiri |
| Kosong | "Belum ada catatan terpadu pada perawatan ini." Hanya muncul sesudah pembacaan berhasil |
| Kosong karena penyaring | "Tidak ada catatan dari profesi yang dipilih." — kalimat yang berbeda dari kosong biasa |
| Kebijakan tidak aktif | "Verifikasi DPJP tidak diwajibkan." beserta penjelasannya, ditampilkan **di atas** lini masa |
| Gagal memuat lini masa | "Catatan terpadu tidak dapat dimuat" beserta **Coba Lagi** |
| Gagal memuat keadaan verifikasi | Catatan tetap terbaca; peringatan menyatakan statusnya belum dapat dipastikan dan tidak boleh dianggap sudah diverifikasi. Tersedia **Coba Lagi** tersendiri |
| Tanpa hak akses | "Akses catatan terpadu ditolak" tanpa membocorkan isi catatan |
| Tanpa kewenangan verifikasi | Seluruh tombol Verifikasi disembunyikan; alasannya dikatakan sekali lewat penjaga kewenangan |
| Hanya baca | Isi catatan tetap terbaca; tidak ada penyuntingan catatan final dari layar ini |
| Episode ditutup | Jalur verifikasi tertutup lewat penjaga kewenangan; lini masa tetap terbaca |
| Kiriman ganda | Tombol Verifikasi pending per item; status **tidak** dianggap Verified sebelum server menjawab berhasil |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Patient Integrated Progress Note

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{episodeId}` | Membaca lini masa lintas profesi satu perawatan, dapat disaring `professionType` | `PatientIntegratedProgressNote : Read` |
| `GET` | `/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{episodeId}/verification-status` | Membaca keadaan verifikasi beserta `isVerificationPolicyEmpty` dan daftar pantaunya | `PatientIntegratedProgressNote : Read` |
| `PATCH` | `/v1/health-services/clinical-management/patient-integrated-progress-notes/{id}/verify` | DPJP menyatakan sudah membaca satu catatan profesi lain | `PatientIntegratedProgressNote : Verify` |

**Tidak ada satu pun ruas identitas yang dikirim dari layar pada permintaan verifikasi.** Badan
permintaannya kosong; verifikator diambil server dari pengguna yang masuk, dan penulis catatan
tidak pernah disentuh — `INV-DOK-11`.

**Delta kontrak yang dicatat:** ketiga endpoint di atas ditandai **Rencana (belum tersedia)** pada
`api-contract.md` §3, padahal seluruhnya sudah hidup di backend sejak `BE-RWI-053`. Selisih
dokumen dilaporkan tanpa menyunting kontrak.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error | `PASS` | Keluaran `eslint . --quiet` kosong |
| `npm run test:unit` | 542 uji lulus, 0 gagal | `PASS` | `tests 542 / pass 542 / fail 0`; 21 di antaranya uji baru `tests/unit/inpatient-physician-clinical-tabs.test.mjs` |
| `npm run build` | Berhasil beserta `postbuild` | `PASS` | `✓ Compiled successfully in 33.2s` |
| Kebijakan tidak aktif berbunyi "tidak diwajibkan", bukan daftar kosong | Penanda `NotRequired` beserta kalimatnya | `PASS` | `FE-RWI-046 K4` |
| Menunggu dan Lewat Batas dibaca dari daftar pantau | Keduanya sesuai | `PASS` | Uji "menunggu dan lewat batas dibaca dari daftar pantau" |
| Status yang tidak dapat dipastikan **tidak pernah** menjadi Diverifikasi | Keadaan ambigu dan keadaan gagal baca sama-sama menghasilkan `unknown`; hanya keadaan yang benar-benar tunggal yang menjadi Verified | `PASS` | Uji "status yang tidak dapat dipastikan..." |
| DPJP aktif berhak; bukan DPJP tidak; penulis tidak dapat memverifikasi tulisannya sendiri; episode ditutup menutup verifikasi; status yang tidak menerima Verify tidak menampilkan tombol | Kelimanya sesuai | `PASS` | `FE-RWI-046 K3` |
| Verifikasi tidak menimpa nama penulis | Penulis diambil dari penulis catatan, bukan dari dokter episodenya | `PASS` | `FE-RWI-046 K2` |
| Grep warna literal, `!important`, dan `<table>` mentah | Nol hasil | `PASS` | Grep checklist konsistensi UI |
| Verifikasi interaktif di peramban | Tidak dijalankan | `NOT RUN` | Lihat catatan di bawah |
| Tampilan nama verifikator sesudah verifikasi berhasil | Tidak dapat diuji | `NOT APPLICABLE` | Nama verifikator tidak dikembalikan satu pun endpoint; lihat bagian 7 dan 8 |

### Pass kedua — 9 September 2026

Seluruh baris di atas adalah riwayat pass pertama dan **tidak dihapus**. Baris di bawah adalah
validasi yang benar-benar dijalankan ulang sesudah `BE-RWI-066` selesai.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | 0 error, 611 warning | `PASS` | `✖ 611 problems (0 errors, 611 warnings)` — sama persis dengan garis dasar sebelum perubahan |
| `npm run test:unit` | 563 uji lulus, 0 gagal | `PASS` | `tests 563 / pass 563 / fail 0`; garis dasar sebelum perubahan 557, jadi 6 uji baru ditambahkan pass ini |
| `npm run build` beserta `postbuild` | Berhasil | `PASS` | `[prepare-standalone] Standalone runtime siap dijalankan.` |
| **Peramban** — Penulis dan Verifikator dua baris berbeda dan **keduanya bernama** | Baris Penulis berbunyi `Ns. Sari`, baris Verifikator berbunyi `dr. Andi Pratama`, dan baris Verifikator diperiksa **tidak** memuat nama penulis | `PASS` | Edge, skenario `FE-RWI-046 K2` |
| **Peramban** — waktu verifikasi dan penanda `DIVERIFIKASI` tampil | Baris Waktu Verifikasi muncul; penanda status berbunyi `DIVERIFIKASI` | `PASS` | Skenario yang sama |
| **Peramban** — catatan yang tidak diwajibkan tidak meminjam nama siapa pun | Baris Verifikator berbunyi `Verifikasi tidak diwajibkan`, bukan nama | `PASS` | Skenario yang sama |
| **Peramban** — catatan yang dituju alamat disorot dan digulir ke layar | `data-selected="true"` hanya pada catatan itu; posisinya diperiksa berada di dalam viewport | `PASS` | Skenario `FE-RWI-050 K3` — lini masa ini yang menyediakannya |
| **Peramban** — alamat tanpa nomor catatan tidak menyorot apa pun | Nol catatan bertanda terpilih, nol peringatan palsu | `PASS` | Skenario `FE-RWI-050` |
| Grep anti-regresi checklist konsistensi UI — warna literal, typography komponen shared, tombol non-base, tabel mentah, utility Bootstrap, `!important` | Nol hasil pada keenamnya | `PASS` | Dijalankan pada keenam berkas yang diubah |
| Screenshot tiga viewport | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 1 September 2026 bahwa e2e dan `.mjs` bukan gerbang selesai |

**Uji manual pass kedua:** `PASS`. Skenario peramban dijalankan di Microsoft Edge terhadap
`.next/standalone/server.js` pada port `3710`, dengan balasan API dipalsukan `page.route` — pola
yang sudah dipakai `tests/e2e/inpatient-physician-workspace.spec.mjs`. Konfigurasi Playwright dan
spec-nya dibuat **sementara**, dijalankan, lalu **dihapus kembali** supaya tidak ikut masuk diff;
`git status` sesudahnya hanya memuat berkas source yang memang diubah. Perlindungan permanennya
dipindahkan ke uji unit, yang ikut berjalan pada `npm run test:unit`.

**Uji manual:** `NOT FEASIBLE`.

**Alasan konkret.** Tidak ada `playwright.config.*` di akar repository sehingga
`npm run test:e2e` tidak dapat dijalankan tanpa menambah konfigurasi baru. Pengujian matriks
kewenangan verifikasi juga menuntut lingkungan uji dengan peran DPJP aktif, DPJP lama, non-DPJP,
dan perawat penulis yang terpisah — gerbang yang masih terbuka pada roadmap §4.

**Tidak dijalankan:** `npm run test:e2e`, `npm run test:uat`, dan screenshot tiga viewport.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Setiap catatan menampilkan penulis dan profesinya | **Terpenuhi** | Judul item berbunyi "`<penulis>` • `<profesi>`", dan blok metadata mengulang Penulis beserta profesinya sebagai keterangan. `normalizeIntegratedNote` mengambil nama berjenjang: nama pengguna, lalu salinan nama saat catatan ditulis, lalu nama dokter — sehingga akun yang berganti nama tidak mengubah penulis catatan lama |
| 2. Setelah diverifikasi, **nama penulis asli tetap tampil sebagai penulis**; verifikator tampil terpisah | **Terpenuhi** — 9 September 2026 | Pemisahannya terbukti sejak pass pertama: baris **Penulis** dan baris **Verifikator** adalah dua entri berbeda pada `ClinicalDocumentMeta`, dan nama penulis diambil dari catatan itu sendiri sehingga tidak mungkin tertimpa. **Bagian yang dulu tertahan kini tertutup:** sejak `BE-RWI-066`, balasan baca membawa `verifiedByUserName`, dan baris Verifikator mengisinya apa adanya. Aturan pengisiannya diuji tidak pernah menyentuh nama penulis, dan di peramban baris Penulis berbunyi `Ns. Sari` sementara baris Verifikator berbunyi `dr. Andi Pratama` pada catatan yang sama. Empat keadaan tanpa nama tetap berbunyi berbeda-beda dan tidak satu pun berisi nama orang — uji `FE-RWI-046 K2` dan skenario peramban bagian 6 |
| 3. Tombol Verifikasi **disembunyikan** bagi yang tidak berhak, bukan ditampilkan lalu ditolak | **Terpenuhi** | `canVerifyNote` memeriksa empat syarat, dan `ClinicalActionGuard mode="hide"` membuat tombolnya tidak dirender sama sekali ketika salah satunya gagal. Uji `FE-RWI-046 K3` menguji kelima jalurnya |
| 4. Saat kebijakan verifikasi tidak aktif, penanda berbunyi "verifikasi tidak diwajibkan" — bukan daftar kosong | **Terpenuhi** | `isVerificationPolicyEmpty` dipakai apa adanya dari server. Ketika benar, `ClinicalSafetyAlert` muncul di atas lini masa dan setiap baris berpenanda "VERIFIKASI TIDAK DIWAJIBKAN". Uji `FE-RWI-046 K4` |
| 5. Keterlambatan tampil tanpa menahan penulisan catatan berikutnya | **Terpenuhi** | Penanda "LEWAT BATAS VERIFIKASI" berdiri sebagai badge pada barisnya saja. Tidak ada satu pun jalur di layar ini yang menahan penulisan; kalimatnya pun ditegaskan di bawah lini masa: "Keterlambatan verifikasi hanya dipantau. Ia tidak menahan penulisan catatan berikutnya." |

### Empat badge status verifikasi

Roadmap menuntut empat badge berbeda. Yang tersedia di layar adalah **lima**, karena satu keadaan
tambahan diperlukan agar layar tidak menebak:

| Keadaan | Label | Dari mana |
| --- | --- | --- |
| Menunggu | `MENUNGGU VERIFIKASI` | Daftar pantau server |
| Lewat batas | `LEWAT BATAS VERIFIKASI` | Daftar pantau server beserta `isOverdue` |
| Tidak diwajibkan | `VERIFIKASI TIDAK DIWAJIBKAN` | `isVerificationPolicyEmpty`, atau rekapitulasi yang menyisakan satu kemungkinan |
| Diverifikasi | `DIVERIFIKASI` | Rekapitulasi yang menyisakan satu kemungkinan |
| **Belum dapat dipastikan** | `STATUS VERIFIKASI BELUM DAPAT DIPASTIKAN` | Keadaan layar, bukan status backend |

**Diperbarui 9 September 2026.** Sejak `BE-RWI-066`, keempat keadaan pertama datang **langsung dari balasan catatan itu sendiri**, bukan lagi disimpulkan dari daftar pantau dan rekapitulasi. Keadaan kelima tetap dipertahankan sebagai jaring pengaman bagi balasan yang tidak membawa kolom verifikasi — misalnya dari layanan versi lama — dan jalur penyimpulan lamanya tidak dihapus.

Keterangan berikut adalah riwayat pass pertama. Keadaan kelima waktu itu ada karena kontrak baca tidak mengembalikan status verifikasi per catatan. Yang
tersedia hanya daftar pantau berisi catatan Menunggu dan Lewat Batas, ditambah rekapitulasi jumlah
per keadaan. Untuk catatan **di luar** daftar pantau, layar hanya dapat memastikan statusnya bila
rekapitulasinya menyisakan satu kemungkinan — seluruh sisanya Diverifikasi, atau seluruh sisanya
Tidak Diwajibkan. Bila keduanya bercampur, statusnya **tidak** dinaikkan menjadi Diverifikasi;
ia disebut belum dapat dipastikan. Aturan itu diuji langsung pada uji "status yang tidak dapat
dipastikan tidak pernah menjadi Diverifikasi".

### Butir Definition of Done

| Butir | Status |
| --- | --- |
| Kelima acceptance existing terbukti | **Terpenuhi** — 9 September 2026, sesudah `BE-RWI-066` menutup kriteria 2 |
| Empat status terbukti | Terpenuhi, ditambah satu keadaan kejujuran layar |
| Larangan self-verification terbukti | Terpenuhi — uji `FE-RWI-046 K3` |
| State dan permission terbukti | Terpenuhi pada source dan tabel bagian 4; **sebagian kini terbukti di peramban** — keadaan Diverifikasi, Tidak Diwajibkan, dan catatan tersorot dijalankan di Edge |
| Visual Acceptance Criteria terbukti | **Terpenuhi kecuali tiga viewport.** Butir "contoh Ns. Sari/dr. Andi tidak menimpa nama" kini terbukti di peramban, bukan hanya pada source dan uji. Screenshot tiga viewport **dikecualikan atas keputusan pengguna 1 September 2026** bahwa e2e dan `.mjs` bukan gerbang selesai |
| Gate §4.1 relevan (butir 10, 18) | **Butir 10 kini terbukti penuh:** Penulis dan Verifikator terpisah **dan** verifikator bernama. Butir 18 tetap terbukti penuh: kosong, tidak diwajibkan, dan gagal dibedakan tegas |
| Laporan memuat bukti per role dan policy | Terpenuhi pada bagian 6 dan 7 |
| Authority Closed §6 tidak dianggap lulus | Terpenuhi — lihat bagian 8 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Uji struktural `FE-RWI-043` "seluruh tab klinis dijaga satu penjaga kewenangan yang sama" gagal pada versi pertama tab ini, karena verifikasi dijaga lewat nilai boolean saja tanpa `ClinicalActionGuard` eksplisit. Uji itu **benar**: verifikasi adalah aksi tulis, dan rules §1.1 butir 7 menyebutnya. **Diperbaiki** dengan menambahkan penjaga eksplisit di dua tempat — sekali di berkas tab beserta alasannya, dan sekali per baris membungkus tombol Verifikasi |
| **Celah kontrak backend — sudah ditutup 8 September 2026** | `PatientIntegratedProgressNoteResponse` **tidak mengembalikan** `VerificationStatus`, `VerifiedAt`, maupun `VerifiedByUserId`, padahal ketiganya tersimpan pada `TrxPatientIntegratedProgressNote`. Balasan `PATCH /{id}/verify` memakai pemetaan yang sama, sehingga verifikasi yang baru saja berhasil pun tidak mengembalikan siapa verifikatornya. Dampaknya: kriteria 2 tidak dapat dipenuhi seluruhnya, dan status per catatan hanya dapat disimpulkan sebagian. **Tidak diperbaiki dari sini** — task ini bermode `FRONTEND`, backend strict read-only, dan mengubahnya diam-diam dilarang batas keselamatan lintas repository. **Penutupnya `BE-RWI-066`, selesai 8 September 2026:** kelima kolom verifikasi kini dikembalikan ketiga bentuk balasan, dan layar ini membacanya pada pass kedua 9 September 2026 |
| Dependency backend | `BE-RWI-053` ✅ selesai 4 September 2026. Mekanisme verifikasinya berjalan; yang kurang adalah permukaan bacanya, seperti dijelaskan di atas |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | **Pass pertama:** berkas task ini muncul sebagai `??` dan `M` pada branch `HamzahV2`, dan agent tidak menjalankan satu pun tindakan Git. Pemilik pekerjaan meng-commit sendiri sebagai `e194509dc`. **Pass kedua 9 September 2026:** dikerjakan di atas `423856322` pada branch `HamzahV2` yang sama; agent kembali **tidak menjalankan satu pun tindakan Git** — tanpa `git add`, commit, push, merge, maupun rebase. Konfigurasi Playwright sementara dan kedua spec sementaranya dihapus kembali, dan `test-results/` tidak tertinggal |
| Langkah berikutnya | Meminta keputusan eligibility **Verifikasi pada episode Closed** kepada Product/Domain bersama `ClinicalManagement` — satu-satunya hal yang masih terbuka pada task ini, dan yang memang tidak boleh dinyatakan lulus dari sini |

### Riwayat: yang dulu dibutuhkan agar task ini dapat menjadi ✅

| No | Kebutuhan | Pemilik | Keadaan 9 September 2026 |
| ---: | --- | --- | --- |
| 1 | `VerificationStatus`, `VerifiedAt`, dan `VerifiedByUserId` beserta nama verifikatornya diekspos pada balasan baca CPPT | `ClinicalManagement`, sebagai task backend baru | ✅ **Terpenuhi** — `BE-RWI-066` selesai 8 September 2026, dan kelima kolomnya dipakai layar ini |
| 2 | Keputusan eligibility **Verifikasi pada episode Closed** — skema §17 menyebut Closed hanya-baca kecuali koreksi final, sementara state §3 dan API §3 tidak merincinya | Product/Domain bersama `ClinicalManagement` | ⏳ **Masih terbuka.** Layar tetap memakai baseline Closed hanya-baca dan **tidak** membuka verifikasi hanya berdasarkan permission umum. Definition of Done kartu task ini memang melarang menyatakannya lulus, jadi ia tidak menahan ✅ |
| 3 | Bukti visual tiga viewport, bila pemilik kelak menghendakinya kembali menjadi gerbang | Frontend authority | ⏳ **Dikecualikan** atas keputusan pengguna 1 September 2026; catatannya tetap tercatat `NOT RUN` |
