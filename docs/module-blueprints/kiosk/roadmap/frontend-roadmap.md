# Frontend Roadmap — Modul Kiosk

| Field | Nilai |
| --- | --- |
| Roadmap ID | `KSK-RM-FE-001` |
| Revision | `1` |
| Status | `approved` — Sukma Giri Pratama, 2026-09-30 |
| Blueprint ID | `KSK-BP-001` r1, status `approved` (Sukma Giri Pratama, 2026-09-30) |
| SHA baseline | FE `4ec51b0b`, BE `419b910f` (branch `sukmagp`) |
| Kontrak masukan | `KSK-CONTRACT-v1` — `approved` (hash per berkas di `blueprint-manifest.md#artifact_hashes`) |
| Masukan desain | `03-frontend-architecture.md`, `04-prd-to-mvp.md` §10, §20 |
| Owner | Sukma Giri Pratama |

> **Batas dokumen.**
> 1. Roadmap ini **bukan** izin menulis kode. Setiap task dikerjakan lewat `build-module-frontend` dengan `TASK MODE: FRONTEND` yang dinyatakan eksplisit; backend menjadi source of truth read-only.
> 2. Pilihan rupa (warna, ikon, bentuk kontrol, modal vs banner) tetap `DEV_DISCRETION` sesuai `03-frontend-architecture.md` §7. Teks pesan dan isi layar **mengikat**.
> 3. Laporan task ditulis di repository backend: `docs/module-blueprints/kiosk/task/report/frontend/<TASK-ID>.md`, mengikuti `rules/frontend/REPORT_TEMPLATE.md`.
> 4. Pakai `InstanceAxios` existing; jangan membuat instance Axios baru dan jangan meng-hardcode host backend.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan DoD terbukti |
| 🟡 | Source ada, kriteria belum terbukti penuh |
| ⛔ | Prasyarat berupa keputusan/gerbang belum terpenuhi |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
FE-KSK-001

FE-KSK-002

BE-KSK-001 ✅ [BE] ─> FE-KSK-003 ─┬─> FE-KSK-004 ─┬─> FE-KSK-005
                                  │               │
                                  │               └──────────┐
                                  │                          │
                                  └─> FE-KSK-006 ────────────┴─┬─> FE-KSK-007 ⛔ ─┬─> FE-KSK-008 ⛔
                                                               │                  │
                                           {KSK-OQ-004 ⛔} ────┘                  │
                                                                                  │
                                           BE-KSK-003 ⛔ [BE] ────────────────────┘
```

`[BE]` = task backend pada `backend-roadmap.md`, cermin baca-saja. `{KSK-OQ-004}` = pencatatan amendment `FE-LAB-13` / `AC-93` di blueprint Laboratorium (keputusannya `KSK-DEC-002/014` sudah `approved`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `FE-KSK-001`, `FE-KSK-002` — mandiri, boleh paralel |
| 1 | `BE-KSK-001` [BE] tersedia di dev | `FE-KSK-003` |
| 2 | `FE-KSK-003` | `FE-KSK-004`, `FE-KSK-006` — boleh paralel |
| 3 | `FE-KSK-004` | `FE-KSK-005` |
| — | ⛔ menunggu `KSK-OQ-004`, `FE-KSK-004`, `FE-KSK-006` | `FE-KSK-007` |
| — | ⛔ menunggu `FE-KSK-007` dan `BE-KSK-003` [BE] | `FE-KSK-008` |

Jumlah pasangan prasyarat → task: 9 (`BE-KSK-001→003`, `003→004`, `004→005`, `003→006`, `004→007`, `006→007`, `KSK-OQ-004→007`, `007→008`, `BE-KSK-003→008`).

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-KSK-001` | Dropdown Poliklinik/Spesialis di atas card | EPIC KSK-06; `FR-KSK-050/051`; KSK-SCH-001 | — (tanpa API) | `ResourceFilterSelect` apa adanya | CSS jadwal dokter | — | Kartu | Lint, build, browser 2 resolusi | Perubahan CSS merembet ke layar lain / Sukma | Kartu |
| `FE-KSK-002` | Bulan pendek di Review & Konfirmasi; nama utuh | `FR-KSK-025`; `KSK-DEC-015`; KSK-BASE-001/002 | — | `formatShortDateId` existing | 2 step view | — | Kartu | Lint, build, browser | — / Sukma | Kartu |
| `FE-KSK-003` | Service, helper, dan konstanta lookup siap dipakai layar | EPIC KSK-01/02; `FR-KSK-011`; `KSK-DSN-009` | api §Kiosk Patient Lookup; validation §1, §2, §4 | `InstanceAxios` | Service, helper murni, konstanta | `BE-KSK-001` | Kartu | Lint, build, uji fungsi murni, panggilan nyata ke BE dev | Aturan normalisasi FE dan BE berbeda / Sukma | Kartu |
| `FE-KSK-004` | Layar Cek No. RM berfungsi dari tile Beranda sampai handoff | EPIC KSK-02; `FR-KSK-010..015`; `KSK-DEC-012` | api; validation §2; state §1 | `BasePatientCard`, pola auth kiosk | Page, view, hook, slice handoff, tile | `FE-KSK-003` | Kartu | Lint, build, browser + Network | Error terbaca sebagai pasien baru / Sukma | Kartu |
| `FE-KSK-005` | Sesi Kiosk dibersihkan setelah 120 detik tanpa sentuhan | EPIC KSK-05; `FR-KSK-040..043`; `KSK-DEC-010` | state §1–2 | `resetRegistrationDraft`, `clearHandoff` | Hook + pemasangan di 2 halaman | `FE-KSK-004` | Kartu | Lint, build, browser (nilai uji lewat env) | Timeout memotong request / Sukma | Kartu |
| `FE-KSK-006` | Step 1 tanpa KTP/HP di URL dan console | EPIC KSK-03; `FR-KSK-023/024`; `KSK-DEC-016` | validation §4 | Lookup service `FE-KSK-003` | Step find (input ketik) + service lama | `FE-KSK-003` | Kartu | Lint, build, browser Network + Console | Regresi pencarian nama/RM / Sukma | Kartu |
| `FE-KSK-007` ⛔ | Flow Pasien Lama 8 step; sesi kiosk sekali di Step 3 | EPIC KSK-03; `FR-KSK-020..022`; `KSK-DEC-002/012/014`; `KSK-DSN-004/007` | state §2; api `scan-result` | Hook & step existing | Hook, rules, find (pindai), service-target, view | `FE-KSK-004`, `FE-KSK-006`, `KSK-OQ-004` | Kartu | Lint, build, browser + cek DB sesi | Sesi ganda / Laboratorium kehilangan target / Sukma | Kartu |
| `FE-KSK-008` ⛔ | Penjamin Utama per kunjungan, termasuk Perusahaan | EPIC KSK-04; `FR-KSK-032..034`; `KSK-DEC-008/009/013` | api §Patient Encounter; validation §3 | Step payment existing | Payment step, payload encounter, hook | `FE-KSK-007`, `BE-KSK-003` | Kartu | Lint, build, browser + cek DB kunjungan | Default pasien ikut berubah / Sukma | Kartu |

## Kartu task

### `FE-KSK-001` — Dropdown Jadwal Dokter di atas card

| Aspek | Isi |
| --- | --- |
| Gelombang | `MVP-0`, gelombang 1 |
| File | Diperbarui: `src/style/kiosk/registration/doctor-schedule/kiosk-doctor-schedule.module.css` (bila perlu, className pada `kiosk-doctor-schedule-view.jsx`) |
| Tidak termasuk | `filter-select.jsx`, `resource-filter-select.jsx`, dan CSS-nya |

**Acceptance criteria:** (1) dropdown Poliklinik dan Spesialis tampil utuh di atas seluruh card pada 1080×1920 dan 1920×1080; (2) tetap di atas saat card di-hover; (3) daftar panjang bisa di-scroll di dalam menu; (4) diff tidak menyentuh berkas komponen dasar.

**Verifikasi:** `npm run lint` pada berkas yang diubah, `npm run build`, tangkapan layar dua resolusi (sebelum/sesudah). **DoD:** AC 1–4 di laporan `FE-KSK-001.md`.

### `FE-KSK-002` — Format tanggal lahir dan nama utuh

| Aspek | Isi |
| --- | --- |
| Gelombang | Isi epic `MVP-2`, tetapi mandiri → gelombang 1 |
| File | Diperbarui: `.../old-patient/kiosk-old-patient-step-review.jsx`, `.../kiosk-old-patient-step-confirm.jsx` |

**Acceptance criteria:** (1) pasien lahir 12 September 1990 tampil `12 Sep 1990` di Review dan Konfirmasi; (2) nama lima kata tampil utuh (boleh terbungkus baris) di Review, Konfirmasi, dan Tiket; (3) tanggal kunjungan/tiket tidak ikut berubah format.

**Verifikasi:** lint, build, browser. **DoD:** AC 1–3 di `FE-KSK-002.md`.

### `FE-KSK-003` — Service, helper, dan konstanta lookup

| Aspek | Isi |
| --- | --- |
| Gelombang | `MVP-1`, gelombang 1 (setelah `BE-KSK-001` tersedia) |
| File | Baru: `src/lib/services/kiosk/registration/kiosk-patient-lookup.service.js`, `src/lib/helpers/kiosk/registration/kiosk-patient-lookup.helpers.js`, `src/lib/constants/kiosk/registration/kiosk-medical-record-check.constants.js` |
| Isi | `lookupKioskPatient({ searchType, value, signal })` (POST, timeout 20 detik, `signal` untuk abort); `normalizeIdentityNumber`, `validateIdentityNumber`, `normalizePhoneNumber`, `validatePhoneNumber`, `classifyIdentificationInput` (`KSK-DSN-009`); konstanta `searchType`/`result` angka dan seluruh teks `validation-matrix.md` §2 |

**Acceptance criteria:** (1) tabel contoh normalisasi `validation-matrix.md` §1 menghasilkan keluaran yang sama; (2) `classifyIdentificationInput` memberi KTP / HP / RM / nama sesuai tabel §4; (3) service membedakan `429`, `5xx`, timeout, dan abort sebagai error bertipe (bukan `NotFound`); (4) tidak ada `console.log`; (5) panggilan nyata ke BE dev untuk satu KTP samaran menghasilkan `result = 1`.

**Verifikasi:** lint, build, eksekusi fungsi murni lewat skrip node sekali pakai di scratchpad (bukan folder test baru), satu panggilan ke BE dev. **DoD:** AC 1–5 di `FE-KSK-003.md`.

### `FE-KSK-004` — Layar Cek Nomor Rekam Medis dan handoff

| Aspek | Isi |
| --- | --- |
| Gelombang | `MVP-1`, gelombang 2 |
| File | Baru: `src/app/kiosk/registration/medical-record-check/page.jsx`, `src/components/view/kiosk/registration/medical-record-check/kiosk-medical-record-check-view.jsx`, `src/lib/hooks/kiosk/registration/medical-record-check/use-kiosk-medical-record-check.jsx`, `src/lib/state/slice/health-services/registration-management/kiosk-patient-handoff-slice.jsx`, `src/style/kiosk/registration/kiosk-medical-record-check.module.css`. Diperbarui: `src/lib/state/store.jsx`, `src/components/view/kiosk/kiosk-home-view.jsx` |
| Catatan | Slice hanya menyimpan `patientId`. Halaman Pasien Lama belum membaca handoff pada task ini (dibaca di `FE-KSK-007`). Sampai `FE-KSK-007` selesai, tombol "Lanjut Pendaftaran Pasien Lama" membuka Pasien Lama dengan urutan lama tanpa pasien terisi; keadaan antara ini dicatat di laporan |

**Acceptance criteria:** (1) tile Beranda membuka layar; (2) validasi tanpa request (Network kosong) untuk KTP 15 digit / HP `12345`; (3) lima hasil tampil dengan teks persis `validation-matrix.md` §2; (4) `429`/`5xx`/timeout → `ERROR` + Coba Lagi, tanpa tombol Pasien Baru; (5) tekan Cek dua kali cepat → satu request; (6) mengubah isian membatalkan request berjalan dan membuang hasil lama; (7) "Daftar Sebagai Pasien Baru" membuka `/kiosk/registration/new-patient` tanpa query; (8) "Lanjut Pendaftaran Pasien Lama" menyimpan `patientId` ke slice lalu membuka `/kiosk/registration/old-patient` tanpa query; (9) akun non-kiosk dialihkan seperti halaman kiosk lain.

**Verifikasi:** lint, build, browser + Network + Redux DevTools. **DoD:** AC 1–9 di `FE-KSK-004.md`.

### `FE-KSK-005` — Inactivity timeout

| Aspek | Isi |
| --- | --- |
| Gelombang | `MVP-1`, gelombang 3 |
| File | Baru: `src/lib/hooks/kiosk/use-kiosk-inactivity-timeout.jsx`. Diperbarui: hook/view Cek No. RM, `use-kiosk-old-patient-registration.jsx` (pemasangan saja) |

**Acceptance criteria:** (1) 105 detik tanpa sentuhan → peringatan 15 detik; (2) Lanjutkan mengulang hitungan; (3) 120 detik → state pasien, hasil pindai, draf, dan handoff kosong, lalu layar ke Beranda; (4) selama request berjalan timeout tidak terpicu; (5) Back browser setelah timeout tidak menampilkan data pasien; (6) nilai dapat ditimpa `NEXT_PUBLIC_KIOSK_IDLE_SECONDS` untuk uji.

**Verifikasi:** lint, build, browser dengan nilai uji 20 detik. **DoD:** AC 1–6 di `FE-KSK-005.md`.

### `FE-KSK-006` — Privasi Step 1 (input ketik)

| Aspek | Isi |
| --- | --- |
| Gelombang | `MVP-2`, gelombang 2 |
| File | Diperbarui: `.../old-patient/kiosk-old-patient-step-find.jsx` (jalur ketik), `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js` (hapus `console.log`) |
| Tidak termasuk | Jalur pindai kartu dan pembentukan sesi (tetap seperti hari ini sampai `FE-KSK-007`) |

**Acceptance criteria:** (1) mengetik KTP 16 digit atau HP → POST lookup; tidak ada GET berisi nilai itu di Network; (2) nama dan No. RM tetap memakai pencarian existing dan hasilnya sama seperti sebelumnya; (3) `MultipleMatch`/`ContactStaff` di Step 1 menampilkan arahan petugas, tanpa daftar pasien; (4) Console browser tidak memuat URL, diagnostik, atau body respons pasien pada seluruh alur Pasien Lama.

**Verifikasi:** lint, build, browser Network + Console. **DoD:** AC 1–4 di `FE-KSK-006.md`.

### `FE-KSK-007` ⛔ — Urutan 8 step dan sesi kiosk di Step 3

| Aspek | Isi |
| --- | --- |
| Gelombang | `MVP-2`; ⛔ menunggu `KSK-OQ-004` |
| Blocker | Amendment `FE-LAB-13` / `AC-93` belum dicatat di blueprint Laboratorium. Keputusannya sudah `approved` (`KSK-DEC-002/014`); yang kurang pencatatan. Pemilik: Sukma |
| Yang tetap bisa jalan | `FE-KSK-001..006` |
| File | Diperbarui: `use-kiosk-old-patient-registration.jsx` (urutan, step awal `find`, `handleReviewContinue` → `serviceTarget`, `handleSelectServiceTarget`/`handleSelectPhysicianRequest` membentuk sesi, `handleResetAll` → `find`, baca `consumeHandoffPatient`), `kiosk-service-target-rules.js`, `kiosk-old-patient-step-find.jsx` (jalur pindai: tanpa sesi, `searchType 3/4`, simpan hasil pindai di memori), `kiosk-old-patient-step-service-target.jsx`, `kiosk-old-patient-view.jsx` |

**Acceptance criteria:** (1) bar langkah poliklinik persis 8 step urutan PRD; Laboratorium: Identifikasi → Review Data → Tujuan Layanan → Selesai; (2) membuka halaman langsung → step Identifikasi; (3) dari Cek No. RM → Step 1 menampilkan pasien itu, URL tanpa query, slice kosong setelah dibaca; (4) memindai kartu di Step 1 **tidak** membentuk sesi (Network: tidak ada `scan-result`); (5) memilih Poliklinik → tepat satu `scan-result` tanpa `targetService`, lalu kunjungan membawa `kioskScanSessionId` sesi itu; (6) memilih Laboratorium + surat dokter → tepat satu `scan-result` dengan `targetService = 2`, lalu layar handoff lab; (7) `scan-result` gagal → tetap di Step 3 + pesan `KSK-VAL-012`; (8) tidak bisa melompat dari Review ke Jenis Kunjungan; (9) kartu asuransi/member dikenali lewat lookup `searchType 3/4`; (10) deep link existing `?patientId=` dari Pasien Baru tetap berfungsi dan kini mendarat di Step 1 terisi.

**Verifikasi:** lint, build, browser; query baca-saja ke `QuilvianNewDevSukma` (dengan izin) untuk menghitung sesi per pasien sesudah AC 5–6. **DoD:** AC 1–10 di `FE-KSK-007.md`; `KSK-OQ-004` tertutup.

### `FE-KSK-008` ⛔ — Penjamin Utama per kunjungan

| Aspek | Isi |
| --- | --- |
| Gelombang | `MVP-3`; ⛔ menunggu `FE-KSK-007` dan `BE-KSK-003` |
| File | Diperbarui: `kiosk-old-patient-step-payment.jsx`, `kiosk-old-patient-registration.service.js` (`buildSelectedGuarantor`, `createOldPatientEncounter`), `use-kiosk-old-patient-registration.jsx` (`handlePaymentContinue`), `kiosk-old-patient-step-confirm.jsx` (baris penjamin) |

**Acceptance criteria:** (1) Kondisi C → layar Pilih Penjamin Utama tanpa preselect; Lanjutkan ditolak sebelum memilih; (2) Kondisi A/B → alur existing; (3) memilih Perusahaan → payload `paymentType = 3` + `patientCompanyGuarantorId`, kunjungan tersimpan, tanpa error "Asuransi pasien belum dipilih"; (4) memilih Asuransi → `paymentType = 2`; (5) tidak ada tombol "Jadikan Utama" dan tidak ada request `PATCH …/primary`; penanda utama di data pasien tidak berubah (cek DB baca-saja); (6) penolakan `400` dari backend tampil dengan awalan "Penjamin perusahaan tidak dapat dipakai:"; (7) setelah tiket tercetak tidak ada jalan kembali ke Pembayaran.

**Verifikasi:** lint, build, browser, cek DB baca-saja. **DoD:** AC 1–7 di `FE-KSK-008.md`.

## Coverage gap

| Requirement | Status |
| --- | --- |
| `FR-KSK-010..015` | `FE-KSK-003/004` |
| `FR-KSK-020..025` | `FE-KSK-002/006/007` |
| `FR-KSK-032..034` | `FE-KSK-008` |
| `FR-KSK-040..043` | `FE-KSK-005` |
| `FR-KSK-050/051` | `FE-KSK-001` |
| Uji di perangkat Kiosk fisik (layar sentuh, pemindai infrared) | **Gap** — tidak tersedia di lingkungan dev; dibuktikan saat UAT oleh pemilik |
