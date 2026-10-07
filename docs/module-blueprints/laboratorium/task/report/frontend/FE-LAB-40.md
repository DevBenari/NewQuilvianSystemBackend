# Laporan Perubahan Frontend — `FE-LAB-40`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-40` |
| Judul | Antrean validasi |
| Slice | Gelombang `MVP-9c` — `EPIC-LAB-15` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-9`, bagian `FE-LAB-40`; *Keputusan 2026-09-25 malam* (nama tahap) |
| Trace | `FR-15.9`, bagian antrean `FR-15.19`; `LAB-DEC-135` butir 2, `LAB-DEC-149`, `LAB-DEC-156`, `LAB-FE-001`, `LAB-FE-006`, `LAB-FE-012` |
| Contract version | `LAB-API-v1` `r34` 29.4 (`approved` 2026-09-25); `LAB-VAL-v1` `r12` `VAL-139`. Backend yang berjalan sudah `r35` 30.4 (`BE-LAB-80`) — selisihnya pada bagian 3.3 |
| Wewenang UI | Diputuskan: route saudara `cito-overdue` (`LAB-FE-001`); urutan dari backend (`LAB-FE-006`); aksi baris membuka Halaman Hasil, **nol tombol Validasi/Rilis** (`LAB-DEC-149`); nama tahap *Menunggu Validasi* = label keadaan (`LAB-DEC-156`). `DEV_DISCRETION` yang dipakai: dua tahap sebagai **tab** pada `DataFilter`; urutan kolom; No. RM di bawah nama pasien; kolom *Divalidasi oleh* hanya pada tahap rilis; lama menunggu di bawah waktu; nama berkas |
| Dependency | `FE-LAB-39` ⚠ (halaman yang dibuka sudah dapat bertindak); `BE-LAB-77` ✅; `BE-LAB-80` ✅ (sudah berjalan, lihat 3.3) |
| Klasifikasi | `MEDIUM` — 6 berkas diubah, 4 berkas baru (route, view, hook, uji); satu route baru; nol perubahan backend |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend` hanya laporan ini, roadmap frontend, dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `e613321c5` (branch `YogaV2`, upstream `origin/YogaV2`), di atas `FE-LAB-35`..`FE-LAB-39` yang belum ter-commit |
| Commit backend yang dijadikan rujukan | `55b032b0` (branch `yoga`) + `BE-LAB-67`..`87` yang belum ter-commit — biner lokal tidak lebih tua dari satu pun berkas `.cs`; `https://localhost:7184` di atas `QuilvianNewDevYoga` |
| Tanggal | 2026-10-02 |
| Status | ✅ **`SELESAI`** (naik 2026-10-06) — antrean berisi hasil PK Final **asli**; validasi sungguhan di halaman memindahkan baris ke *Menunggu Rilis*, rilis mengeluarkannya. Lihat 9. *(Semula: ⚠ — antrean hidup terhadap backend asli; nol tombol tindakan; uji unit 22/22 baru (77/77 berkas terdampak), lint nol peringatan, build hijau, layar **26/26**, nol tulis. **Batas:** dev belum punya satu pun hasil Patologi Klinik Final dan belum ada dokter pemvalidasi yang ditunjuk — antrean berisi dan perpindahan tahap sesudah validasi dibuktikan dengan baris suapan (lihat 6))* |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Belum ada layar antrean. Pemvalidasi hanya dapat menemukan hasil yang menunggu dengan membuka order satu per satu | Tidak ada route `lab-worklists/validation-queue`; `lab-worklist.service.js` hanya punya `/pending` dan `/cito-overdue` |
| Backend sudah menyediakan `GET /lab-worklists/validation-queue` | `LabWorklistController.GetValidationQueue` (`BE-LAB-77`) |
| **Backend sudah melangkah ke `r35`**: `discipline` kosong kini berarti Patologi Klinik **dan** Mikrobiologi | `LabWorklistService.ResolveQueueDisciplines` (`BE-LAB-80`). Di dev, `?stage=AwaitingValidation` tanpa `discipline` mengembalikan *Pewarnaan BTA Sputum* (Mikrobiologi, `LAB-RSMMC-000014`); dengan `discipline=ClinicalPathology` mengembalikan nol baris |
| Respons baris memuat `discipline` dan `resultQualifier` (`r35`) di samping ruas `r34` | `LabValidationQueueItemResponse` |
| Pencarian backend hanya mencari kode/nama pemeriksaan dan barcode wadah — **bukan** nama pasien, No. RM, atau No. order | `LabWorklistService.TerapkanPencarian` |
| Pola acuan: layar Daftar Kerja dan pantau cito memakai satu view, satu hook, satu slice | `lab-worklist-view.jsx`, `use-lab-worklist.jsx`, `lab-worklist-slice.jsx` |
| Penanda rujukan dan label keadaan sudah punya sumber bersama | `LAB_REFERENCE_FLAG_*` (`FE-LAB-36`), `LAB_RESULT_STATUS` (`FE-LAB-39`) |
| Data dev: nol hasil Patologi Klinik Final, Tervalidasi, atau Dirilis | Kedua tahap dengan `discipline=ClinicalPathology` → `200`, `totalData = 0` |

---

## 2. Proses bisnis dari sisi pengguna

**Dokter pemvalidasi** membuka menu **Laboratorium → Antrean Validasi** (tepat di bawah *Pantau Keterlambatan Cito*):

1. Tab **Menunggu Validasi** terbuka lebih dulu. Setiap baris satu pemeriksaan Patologi Klinik yang sudah
   dinyatakan selesai analis: pasien dan No. RM, No. order, pemeriksaan, kesegeraan (**CITO**/**Biasa**),
   penanda (**H** *di atas nilai rujukan*, **L** *di bawah nilai rujukan*, atau **Di luar rujukan**), dan
   kapan mulai menunggu beserta lamanya — misalnya *"2 Okt 2026, 09.29 · Menunggu 30 menit"*.
2. Cito selalu di atas, lalu yang paling lama menunggu. Urutan itu dari backend; layar tidak menyusunnya ulang.
3. Ia menekan **Buka Hasil** pada baris Kalium. Halaman Hasil Patologi Klinik order itu terbuka — tempat ia
   membaca Kalium bersama hasil lain pasien yang sama, lalu menekan **Validasi** di sana.
4. Ia kembali (tombol *back* peramban). Antrean dimuat ulang pada tab yang sama; Kalium sudah tidak ada di
   *Menunggu Validasi*.

**Perilis** membuka tab **Menunggu Rilis**: barisnya sama, ditambah kolom **Divalidasi oleh** supaya ia tahu
siapa yang sudah mengesahkan sebelum ia menjadi orang kedua. **Buka Hasil** → Rilis di halaman hasil.

**Tidak ada tombol Validasi, Rilis, atau Kembalikan di antrean** — sebuah keterangan di atas tabel
menyebutnya: *"Tekan Buka Hasil pada baris untuk membuka halaman hasil order itu. Validasi, Rilis, dan
Kembalikan ke analis ada di halaman tersebut."*

**Penyaring:** pencarian (kode/nama pemeriksaan, barcode wadah), *Hanya Cito*, jumlah baris, halaman.
**Atur ulang** membersihkan penyaring tetapi **tidak** memindahkan tab.

**Jalur tidak normal:** antrean kosong menulis sebabnya per tahap; tanpa `LabWorklist : Read` → halaman
*Akses Ditolak*; galat server → pesan backend di atas tabel, baris lama dikosongkan, **Muat ulang** memulihkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-40`, `FE-LAB-42`, *Keputusan 2026-09-25 malam*; `03-frontend-architecture.md` amandemen 2026-09-25 (pertama dan kedua) bagian *Antrean validasi*; `contracts/api-contract.md` 29.3-29.5; `acceptance-test-matrix.md` `AC-196` | Cakupan, wujud, kontrak, kriteria |
| `task/report/backend/BE-LAB-77.md`, `BE-LAB-80` di `backend-roadmap.md`; `task/report/frontend/FE-LAB-39.md` | Perilaku antrean di backend; halaman yang dibuka |
| Backend `LabWorklistController.cs`, `LabWorklistService.cs` (`GetValidationQueueAsync`, `TerapkanPencarian`), `LabWorklistDtos.cs` | Query, respons, urutan, cakupan pencarian, `VAL-139`/`VAL-145` |
| Frontend `lab-worklists/*` (route, view, kolom), `use-lab-worklist.jsx`, `lab-worklist-slice.jsx`, `lab-worklist-rules.js`, `lab-worklist.service.js`, `lab-worklist-constants.jsx`, `lab-result-status-constants.jsx`, `lab-clinical-pathology-result-constants.jsx`, `menu-items.jsx`, `data-filter.jsx`, `data-table.jsx`, `access-denied-utils.jsx`, `InstanceAxios.jsx` | Pola yang diikuti dan komponen yang dipakai ulang |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/laboratory-management/lab-worklists/validation-queue/page.jsx` (baru) | Route tipis, sejajar `cito-overdue/page.jsx` |
| `src/components/view/health-services/laboratory-management/lab-worklists/lab-validation-queue-view.jsx` (baru) | Hero, `DataFilter` bertab dua tahap, keterangan aksi baris, `DataTable` dengan `sortLatestFirst={false}`, `AccessDeniedGate` |
| `src/lib/hooks/health-services/laboratory-management/use-lab-validation-queue.jsx` (baru) | Controller antrean: penyaring di Redux, satu permintaan per perubahan, permintaan lama dibatalkan, tahap, *Buka Hasil* dengan token privat yang sama dengan Daftar Kerja |
| `src/components/view/.../lab-worklists/lab-worklist-table-columns.jsx` | `buildLabValidationQueueColumns`, `getLabValidationQueueRowId` — ditaruh di berkas kolom Daftar Kerja supaya pembantu format tanggal tidak terduplikasi |
| `src/lib/hooks/.../lab-worklist-rules.js` | `normalizeValidationQueueStage`, `isAwaitingReleaseStage`, `buildValidationQueueParams`, `readQueueReferenceMarker`, `formatWaitingDuration`, `resolveQueueResultRoute` — fungsi murni |
| `src/lib/state/slice/.../lab-worklist-slice.jsx` | Cabang `validationQueue*` dan thunk `fetchLabValidationQueue`; atur ulang mempertahankan tahap; gagal memuat mengosongkan baris lama |
| `src/lib/services/.../lab-worklist.service.js` | `getLabValidationQueue` |
| `src/lib/constants/.../lab-worklist-constants.jsx` | Route `validationQueue`; `LAB_VALIDATION_QUEUE_STAGES` (label *Menunggu Validasi* **diambil** dari `LAB_RESULT_STATUS.Final`), tab, disiplin tetap, penyaring bawaan, salinan teks |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir **Antrean Validasi** sesudah *Pantau Keterlambatan Cito*, tanpa `requiredPermission` — sama dengan saudaranya |
| `tests/unit/lab-validation-queue-rules.test.mjs` (baru) | 22 uji — lihat 6 |

### 3.3 Kepatuhan arsitektur dan keputusan kecil

| Hal | Keputusan dan alasan |
| --- | --- |
| **Selisih `r34` lawan backend `r35`** | `r34` (kontrak task ini): antrean **selalu** Patologi Klinik, `discipline` diabaikan. Backend yang berjalan (`r35`, `BE-LAB-80`): `discipline` kosong = PK **dan** Mikrobiologi. Layar mengirim `discipline=ClinicalPathology` **selalu** (`LAB_VALIDATION_QUEUE_DISCIPLINE`) — isi `r34` terjaga pada kedua versi backend. Tanpanya, BTA Mikrobiologi dev muncul di antrean dan membuka halaman yang belum dapat memvalidasinya (`FE-LAB-41` belum). **`FE-LAB-42` menggantinya dengan penyaring disiplin** |
| Route halaman hasil | Dibaca dari `discipline` baris bila ada, **jatuh ke Patologi Klinik** bila tidak (bentuk `r34`) — pemetaan yang sama dengan Daftar Kerja (`resolveResultPageRoute`). Baris berdisiplin lain tidak pernah membuka halaman PK |
| Urutan | Dari backend apa adanya — `DataTable` `sortLatestFirst={false}`; tidak ada pengurutan pilihan petugas di antrean (amandemen: *layar tidak mengurutkan ulang*) |
| Dua tahap | **Tab** pada `DataFilter` — tahap wajib (`VAL-139`), sehingga nilai kosong/keliru dinormalkan ke *Menunggu Validasi* sebelum terkirim; tab tersimpan di Redux sehingga kembali dari halaman hasil membuka tab yang sama |
| Nama tahap | *Menunggu Validasi* dibaca dari konstanta label keadaan bersama — tidak ditulis ulang; *Menunggu Rilis* nama tahap |
| Penanda | Kata yang **sama** dengan Halaman Hasil (`LAB_REFERENCE_FLAG_*`); huruf `H`/`L` diberi kalimat pendamping; `Normal`/kosong/tak dikenal → `-`; nol penanda kritis (`S5`) |
| Kolom *Divalidasi oleh* | Hanya pada tahap rilis — ruas kontrak `validatedByName`; perilis perlu tahu orang pertamanya |
| No. RM | Di bawah nama pasien (pola daftar Pemeriksaan). Kolom terpisah membuat tahap rilis lebih lebar dari kartunya dan tombol *Buka Hasil* terpotong di 1440 px — ditemukan saat uji layar, diperbaiki, diuji ulang (S15d) |
| Satu slice, bukan slice baru | Antrean saudara Daftar Kerja dan pantau cito; cabangnya terpisah sehingga ketiganya tidak saling mengosongkan (uji unit) |
| Menu | Tanpa `requiredPermission`, sama dengan `cito-overdue` — roadmap: *butir menu mengikuti cara `cito-overdue` ditampilkan*. Jabatan tanpa `LabWorklist : Read` melihat butirnya lalu *Akses Ditolak* |

`UI GATE: 9 elemen — REUSE 9` — `Hero`, `DataFilter` (termasuk `tabs`), `FilterSelect`, `InformationAlert`,
`DataTable`, `Pagination`, `StatusBadge`, `BaseButton`, `AccessDeniedGate`. Nol komponen baru, nol CSS baru.
Grep anti-regresi pada berkas JSX baru: nol `<button` mentah, nol `<table`, nol `fw-`/`fs-`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Mengambil antrean validasi..."* di dalam tabel; tab, penyaring, dan tombol *Muat ulang* nonaktif |
| Kosong — Menunggu Validasi | **Tidak ada hasil yang menunggu validasi** — *"Belum ada hasil Patologi Klinik yang dinyatakan selesai analis pada penyaring ini. Hasil yang masih Draft belum masuk antrean."* |
| Kosong — Menunggu Rilis | **Tidak ada hasil yang menunggu rilis** — *"Seluruh hasil tervalidasi pada penyaring ini sudah dirilis."* |
| Gagal | Pesan backend di atas tabel (cadangan *"Antrean validasi gagal dimuat."*); baris lama dikosongkan supaya hasil tidak tampak di tahap yang salah; **Muat ulang** memulihkan |
| Tanpa hak akses (`403`) | Halaman **Ups! Akses Ditolak** (`AccessDeniedGate`) |
| `422` `VAL-139` | Tidak dapat terjadi dari layar — tahap selalu dinormalkan; bila tetap datang, pesannya tampil sebagai galat |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Worklist

Base URL: `api/v1/health-services/laboratory-management/lab-worklists`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/validation-queue` | Isi kedua tab. Parameter: `stage` (**selalu**), `discipline=ClinicalPathology` (**selalu**), `pageNumber`, `pageSize`, `onlyCito` (bila dipilih), `search` (bila diisi) | `LabWorklist : Read` |

Ruas respons yang dibaca: `examinationId`, `labOrderId`, `orderNumber`, `patientName`, `medicalRecordNumber`,
`procedureName`, `urgency`, `referenceFlag`, `waitingSince`, `validatedByName`, `discipline` (bila ada).
`resultStatus`, `finalizedAt`, `validatedAt`, `encounterId`, `resultQualifier` tidak ditampilkan — tahap
sudah menyatakan keadaannya.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-validation-queue-rules.test.mjs tests/unit/lab-worklist-rules.test.mjs tests/unit/lab-worklist-result-entry.test.mjs tests/unit/lab-clinical-pathology-result-rules.test.mjs` | **77/77** (22 baru) | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2262 uji: 2256 lolos, 6 gagal — **sama dengan baseline `FE-LAB-39`**: Hemodialisa ×4 (`hemodialysis-navigation-and-privacy-audit`, `hemodialysis-sidebar-navigation`, `menu-permission-filter`), Bank Darah M0, petty cash. Pesan galatnya menyangkut butir Hemodialisa, Bank Darah, dan label *Keuangan* — nol menyangkut butir Laboratorium | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npx eslint --max-warnings=0` sepuluh berkas yang disentuh | Nol masalah | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `npm run build` (server BE/FE dimatikan dulu, port dipastikan bebas) | Exit 0; *Compiled successfully*; route `/health-services/laboratory-management/lab-worklists/validation-queue` ada | `PASS` | Keluaran build |
| HTTP backend lokal, superadmin | Lihat 1: `discipline` kosong memuat BTA Mikrobiologi; `discipline=ClinicalPathology` nol baris; tanpa `stage` `422` `VAL-139` | `PASS` | Keluaran skrip |

**Uji unit baru (22):** nama tahap sama dengan label `Final`; urutan tab; route saudara `cito-overdue`; tahap
kosong/keliru/angka/huruf kecil → *Menunggu Validasi*; parameter bawaan; `stage` selalu terkirim; `discipline`
selalu `ClinicalPathology` walau penyaring membawa nilai lain; `onlyCito` hanya bila dipilih; penanda `H`/`L`/
*Di luar rujukan*; `Normal`/kosong/tak dikenal/`Critical` tanpa penanda; lama menunggu (menit, jam, hari,
*kurang dari 1 menit*, waktu kosong/rusak/masa depan); route baris tanpa disiplin → PK, Mikrobiologi → halaman
Mikrobiologi, token kosong → tanpa route; reducer: keadaan awal, berhasil memuat apa adanya, gagal mengosongkan
baris, batal bukan galat, atur ulang mempertahankan tab, Daftar Kerja dan pantau cito tidak tersentuh.

**Verifikasi manual** — `next dev` `localhost:3000` terhadap backend lokal `https://localhost:7184`, login
superadmin sungguhan lewat formulir, Playwright. S1–S3 **backend asli**; S4–S18 baris antrean **disuapkan**
lewat pencegatan `GET /validation-queue` (dev nol hasil PK Final), dengan `labOrderId` order nyata
`LAB-RSMMC-000001` sehingga *Buka Hasil* membuka halaman hasil sungguhan. Setiap permintaan non-`GET` ke API
dicegat dan dicatat: **nol tulis**.

| Skenario | Hasil sebenarnya |
| --- | --- |
| S1 Menu | Butir *Antrean Validasi* ada di grup Laboratorium, tepat di bawah *Pantau Keterlambatan Cito*; menekannya membuka route baru |
| S2 Backend asli, tab validasi | Permintaan `stage=AwaitingValidation&discipline=ClinicalPathology&pageNumber=1&pageSize=20`; tab aktif; *Tidak ada hasil yang menunggu validasi*; BTA Mikrobiologi **tidak** tampil |
| S3 Backend asli, tab rilis | `stage=AwaitingRelease`; tab berpindah (`aria-pressed`); *Tidak ada hasil yang menunggu rilis* |
| S4 Urutan | Hemoglobin cito (30 menit) → Leukosit (3 jam) → Urinalisis Protein (1 jam) — persis urutan backend, tidak disusun ulang menurut lama menunggu. `sortLatestFirst={false}` adalah penjaga: baris antrean hari ini tidak membawa ruas tanggal yang dibaca pengurutan bawaan `DataTable` (`createdAt`, `updatedAt`, dan sejenisnya), tetapi ruas seperti itu kelak dapat ditambahkan |
| S5–S7 Kolom | **CITO**/**Biasa**; **H** *di atas nilai rujukan*, **L** *di bawah nilai rujukan*, **Di luar rujukan**; pasien, `RM-UJI-0001`, `LAB-RSMMC-000001`; *Menunggu 30 menit*, *Menunggu 3 jam* |
| S8 Nol tombol tindakan | Tombol bernama Validasi/Rilis/*Kembalikan ke analis*: **0**; *Buka Hasil*: 3, masing-masing bernama aksesibel *"Buka hasil Hemoglobin pasien …"* |
| S9 Kolom | Seluruh kolom wajib ada; *Divalidasi oleh* tidak ada pada tahap validasi |
| S10–S12 Penyaring | Pencarian → `search=Hemoglobin`, halaman 1, tahap dan disiplin tetap; *Hanya Cito* → `onlyCito=true` digabung pencarian, label terpilih tampil; atur ulang → keduanya hilang, tahap tetap, isian cari kosong |
| S13–S14 Paginasi | 45 baris: *Halaman berikutnya* → `pageNumber=2`; 50 baris → `pageSize=50`, kembali ke halaman 1 |
| S15a *Buka Hasil* | Halaman Hasil PK `LAB-RSMMC-000001` terbuka **dari backend asli** (nomor order dan baris Hemoglobin tampil); alamat memakai token privat, bukan id order |
| S15b–c Kembali sesudah validasi | Keadaan backend tiruan diubah menjadi *Hemoglobin tervalidasi*; *back* peramban → antrean **dimuat ulang** pada tab *Menunggu Validasi*, Hemoglobin hilang (2 baris); tab *Menunggu Rilis* → Hemoglobin dengan *dr. Contoh Pemvalidasi, Sp.PK* dan *Menunggu 1 menit* |
| S15d Lebar | 1440 px, kedua tahap: tabel tanpa gulir menyamping, *Buka Hasil* terlihat utuh |
| S16 Tahap rilis | Nol tombol Rilis/*Kembalikan*; atur ulang tidak memindahkan tab |
| S17 `403` | *Ups! Akses Ditolak* |
| S18 `500` | Pesan backend tampil, nol baris basi; *Muat ulang* memulihkan |
| S19 390 px | Tanpa gulir horizontal halaman (`scrollWidth` 375 ≤ 390) |
| S20–S21 | Nol tulis ke backend; nol galat runtime halaman |

Layar: **26/26** `PASS`.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-validation-queue-rules.test.mjs — PASS`

`MANUAL TEST: PASS` — dengan batas di bawah.

**Batas verifikasi:**

- Roadmap meminta *"hasil cito di atas; memvalidasi dari halaman lalu kembali → baris pindah ke tahap Menunggu
  Rilis"* terhadap backend. Antrean **berjalan terhadap backend asli**, tetapi dev belum punya satu pun hasil
  Patologi Klinik Final, dan belum ada dokter pemvalidasi yang ditunjuk pada kredensial Human Resource
  (batas yang sama dengan `FE-LAB-39`, `BE-LAB-73`..`75`). Urutan cito dan perpindahan tahap dibuktikan dengan
  baris suapan berbentuk respons sungguhan; urutan dan isi tahap di backend sendiri sudah terbukti pada harness
  `BE-LAB-77` (21/21) dan `BE-LAB-80` (17/17).
- Akun pemegang `LabWorklist : Read` selain superadmin tidak dipakai — sandi akun analis tidak tersedia pada sesi
  ini. `403` asli sudah terbukti pada endpoint yang sama oleh `BE-LAB-77` (Kepala Instalasi).

**Tidak dijalankan:** `npm run test:e2e` — tidak diminta dan tidak ada spec antrean.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Bagian antarmuka `AC-196` — Kalium Draft tidak ada di antrean | Terpenuhi pada tampilan: antrean menampilkan **hanya** yang dikirim backend tanpa menyaring ulang, dan backend mengeluarkan Draft (`BE-LAB-77` harness); salinan teks kosong menyebut Draft belum masuk antrean | S2; uji unit reducer *apa adanya* |
| Nol tombol Validasi/Rilis di antrean | **Terpenuhi** | S8, S16 |
| Route `lab-worklists/validation-queue`; butir menu seperti `cito-overdue` | **Terpenuhi** | S1; build |
| Dua tahap *Menunggu Validasi*/*Menunggu Rilis*; nama tahap sama dengan label keadaan | **Terpenuhi** | S2, S3; uji unit |
| Kolom pasien, No. RM, No. order, pemeriksaan, cito, penanda `L`/`H`, menunggu sejak | **Terpenuhi** | S5–S7, S9 |
| Urutan dari backend | **Terpenuhi** | S4 |
| Aksi baris membuka Halaman Hasil order itu | **Terpenuhi** — halaman asli | S15a |
| Verifikasi — hasil cito di atas | Terpenuhi pada baris suapan; **belum** dengan data dev | S4; batas 6 |
| Verifikasi — validasi dari halaman lalu kembali → pindah ke *Menunggu Rilis* | Perilaku layar terpenuhi (muat ulang, tab tetap, isi baru); validasi sungguhan **menunggu** dokter yang ditunjuk dan hasil PK Final | S15b–c; batas 6 |
| DoD — antrean hidup; nol tombol tindakan; laporan | **Terpenuhi** | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan lint pada berkas yang disentuh; build tanpa error |
| Masalah yang diketahui | **1.** Pencarian backend tidak mencari nama pasien, No. RM, atau No. order — pemvalidasi yang mencari pasien tertentu harus memakai nama pemeriksaan. Bukan cacat layar; placeholder menyebut cakupan sebenarnya. Pelebaran pencarian adalah keputusan backend. **2.** Tombol *Kembali* Halaman Hasil PK (`FE-LAB-36`) selalu menuju daftar Pemeriksaan PK, bukan ke antrean; kembali ke antrean lewat *back* peramban atau menu. Tidak diubah — di luar cakupan |
| Dependency backend | Nol perubahan. Untuk menaikkan ke ✅: satu hasil PK Final di dev dan dokter pemvalidasi yang ditunjuk (berizin `Validate` dan `LabWorklist : Read`) — langkah yang sama dengan `FE-LAB-39` |
| Perubahan sampingan | `NONE` di repository. **Lingkungan:** disk C: penuh (`0 GB`, `ENOSPC`) saat `next dev` berjalan — penyebabnya cache Turbopack `.next/dev/cache/turbopack` yang membengkak menjadi **47,5 GB** sejak 2026-08-19 (dan sudah menandai dirinya tidak sah). Folder cache itu saja yang dihapus — tergenerasi ulang otomatis; ruang kosong kembali 53 GB. Sesudah `TaskStop`, proses `next dev` kembali tertinggal dan dihentikan sebelum build; port 3000/7184/5107 dipastikan bebas. `test-results/` tidak tersentuh (skrip Playwright sendiri, bukan runner) |
| Interupsi | `NONE` — kegagalan login pertama karena disk penuh; diulang sesudah ruang dipulihkan |
| Status Git | Frontend (`e613321c5`): ` M` `lab-worklist-table-columns.jsx`, `lab-worklist-constants.jsx`, `lab-worklist-rules.js`, `lab-worklist.service.js`, `lab-worklist-slice.jsx`, `menu-items.jsx` (keenamnya juga memuat perubahan `FE-LAB-37`..`39` yang belum ter-commit); `??` `lab-worklists/validation-queue/`, `lab-validation-queue-view.jsx`, `use-lab-validation-queue.jsx`, `tests/unit/lab-validation-queue-rules.test.mjs`. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `FE-LAB-41` (pola yang sama untuk Mikrobiologi), lalu `FE-LAB-42` — **mengganti** `LAB_VALIDATION_QUEUE_DISCIPLINE` yang tetap dengan penyaring disiplin dan menampilkan kolom disiplin; aksi baris sudah membuka halaman Mikrobiologi untuk baris Mikrobiologi |

## 9. Verifikasi nyata 2026-10-06 — sesudah setup langkah rilis di devYoga

**Status: `FE-LAB-40` ✅ `SELESAI`.** Batas lama — *dev nol hasil PK Final dan belum ada dokter pemvalidasi* — tertutup; perpindahan tahap kini dibuktikan dengan data asli, bukan baris suapan.

**Konteks.** Atas persetujuan pemilik modul, langkah rilis `MVP-9d`/`MVP-10c` dijalankan sebagai **setup uji di devYoga**
([`backend-roadmap.md`](../../../roadmap/backend-roadmap.md) 6ak.10, 6al.5): kode kewenangan `LAB-*` di katalog Human Resource,
kredensial dr. Bima (`LAB-VAL-PK`/`LAB-REL-PK`) dan dr. Nabila (`LAB-VAL-MB`/`LAB-REL-MB`), serta `Validate`/`Release`/`Return`,
`LabWorklist : Read`, dan hak baca kedua daftar alasan bagi jabatan *Kepala Instalasi Laboratorium* dan *Dokter Penanggung Jawab
Laboratorium*. Hemoglobin dan Leukosit `LAB-RSMMC-000001` dinyatakan selesai sebagai hasil uji.

**Temuan uji — langkah rilis 4 kurang dua izin baca.** Dokter berizin `Validate` tetap *Akses Ditolak* di Halaman Hasil: kedua
jabatan tidak memegang `LabOrder : Read` (`GET /lab-orders/{id}`) dan `LabExamination : Read` (`GET /lab-examinations/by-order/{id}/results`,
`/result/microbiology`). Keduanya ditambahkan atas persetujuan pemilik modul; langkah rilis 4 `MVP-9d`/`MVP-10c` wajib memuatnya.

**Akun asli:** dr. Bima Prasetya, Sp.PK (Kepala Instalasi) dan Vina (analis). Tulis yang diteruskan hanya tindakan pengesahan pada
Hemoglobin/Leukosit `LAB-RSMMC-000001` dan Validasi BTA `LAB-RSMMC-000014` (ditolak `403`).

| ID | Task | Skenario | Hasil |
| --- | --- | --- | --- |
| Q1 | 40, 42 | dr. Bima: antrean *Semua Disiplin* memuat Hemoglobin dan Leukosit (PK) serta BTA (Mikrobiologi) dari backend asli, satu permintaan tanpa `discipline` | `PASS` |
| Q2 | 42 | *Patologi Klinik* → hanya Hemoglobin dan Leukosit; `discipline=ClinicalPathology` | `PASS` |
| P1 | 39 | Halaman Hasil PK, dr. Bima (`Validate` tanpa `Update`): Hemoglobin *Menunggu Validasi*; hanya tombol Validasi | `PASS` |
| P2 | 39 | Validasi Hemoglobin → **`200`**; *Tervalidasi*, *"Validasi oleh: dr. Bima Prasetya, Sp.PK — Kepala Instalasi Laboratorium"*; Rilis dan Kembalikan ditawarkan | `PASS` |
| P3 | 39 | Leukosit: Validasi `200`, lalu *Kembalikan ke analis* — tanpa alasan nol permintaan; dengan *"Sampel tertukar"* **`200`**; kembali *Draft* | `PASS` |
| Q3 | 40, 42 | Kembali ke antrean: Hemoglobin pindah ke *Menunggu Rilis* dengan *Divalidasi oleh dr. Bima*; Leukosit (dikembalikan) di kedua tahap tidak ada | `PASS` |
| P4 | 39 | Rilis oleh pemvalidasi sendiri: panel menyebut rekam medis dan meminta alasan pengecualian (*"Shift tunggal…"*) → **`200`**; *Dirilis*, *Otorisasi oleh* dr. Bima, penanda pengecualian sebagai teks; nol tombol | `PASS` |
| RM | 39 | Baris rekam medis: satu `MrcClinicalDocumentIntegrity` bagi Hemoglobin — ditandatangani dan dikunci atas nama dr. Bima saat rilis | `PASS` |
| Q4 | 40 | Sesudah dirilis Hemoglobin hilang dari *Menunggu Rilis* | `PASS` |
| M1 | 41 | dr. Bima (berizin `Validate`, **tanpa** kewenangan Mikrobiologi) memvalidasi BTA → **`403` lapis orang asli**: *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Mikrobiologi."* tampil apa adanya | `PASS` |
| V1 | 39 | Vina (analis): Hemoglobin *Dirilis* nol tombol; Leukosit yang dikembalikan dapat diisi lagi, nol tombol pengesahan | `PASS` |
| Z1 | — | Nol tulis di luar tindakan yang diizinkan; nol galat runtime | `PASS` |

**Jejak di devYoga:** Hemoglobin `LAB-RSMMC-000001` **Dirilis** (validasi dan rilis oleh dr. Bima, berpenanda pengecualian empat
mata); Leukosit dikembalikan ke *Draft* dengan alasan *Sampel tertukar* (`SAMPEL-TERTUKAR`; *dikoreksi 2026-10-06 dari riwayat transisi — semula tertulis Salah ketik hasil*). Data uji.

**Nol perubahan kode** pada task ini. **Nol operasi Git dijalankan.**
