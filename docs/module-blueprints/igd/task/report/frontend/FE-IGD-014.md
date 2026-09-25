# Laporan Perubahan Frontend — `FE-IGD-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-014` |
| Judul | Pendaftaran IGD mengikuti `EncounterType.Emergency` |
| Slice | `IGD-S02`, `IGD-S03` · `EPIC IGD-01`, `EPIC IGD-02` |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md) kartu `FE-IGD-014` |
| Trace | `IGD-DEC-084` (satu pasien satu episode), `IGD-DEC-090`; `IGD-EV-123` butir 1; validation matrix aturan pesan episode ganda; `BE-IGD-025` |
| Contract version | API `0.8.0` — `GET emergency-visits` dengan filter `patientId` sudah ada dan **tidak diubah** |
| Wewenang UI | Kartu `FE-IGD-014` acceptance 2; pemilik menugaskan backlog ini pada 21 September 2026 sesudah `EPIC IGD-04` tuntas. Wewenang memilih tata letak peringatan: `DEV_DISCRETION` dalam pola `EmergencyInlineAlert` yang sudah dipakai modul pendaftaran |
| Dependency | `BE-IGD-023` ✅, `BE-IGD-025` ✅ |
| Klasifikasi | `MEDIUM` — 5 berkas source diubah di satu modul, tanpa perubahan kontrak, backend, maupun CSS; memakai endpoint dan route token yang sudah ada. Penilaian agent; skor rinci tidak dihitung |
| Task mode | `FRONTEND` dengan wewenang laporan lintas repository yang sempit |
| Target tulis | `QuilvianSystemFrontendDev` (source), laporan ini dan tautan bukti pada roadmap/traceability di `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `16c767916` pada branch `RizkiV2`. **Kemudian di-commit dan di-push pemilik sebagai `198d56d9e`** ("melanjutkan asigment pasien", 21 September 2026) — memuat kelima berkas source dan test baru, persis seperti daftar bagian 3.2 |
| Commit backend yang dijadikan rujukan | `267b56a0` pada branch `rizkiG` |
| Tanggal | 21 September 2026 |
| Status | ✅ **Dinilai ulang 21 September 2026 (malam) — selesai atas penilaian pemilik, dengan celah `IGD-OQ-093` dinyatakan** (bagian 9). *Status sebelum penilaian ulang:* 🟡 **Implementation Complete** — kriteria 2 kini ada di source. `eslint` berkas yang diubah 0 error, 9 unit test baru lulus. **`npm run build` lulus** — dijalankan pemilik 21 September 2026 (malam), ekor keluaran dilampirkan pemilik: `postbuild` `prepare-standalone` berhasil, yang hanya berjalan sesudah `next build` berhasil. **Uji lewat layar oleh pemilik 21 September 2026 (malam): kotak peringatan tampil dengan nomor dan status yang benar dan tombol *Buka Kunjungan IGD* tampil** (bagian 6.1); **hasil klik tombol belum dikonfirmasi**. Tangkapan layar yang sama membuktikan encounter yatim benar-benar terjadi (peringatan *Encounter sudah terbentuk*). Kriteria 1 dan 3 tidak berubah. UAT belum dan tidak diklaim |

**UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0** (bagian 3.3).

---

## 1. Keadaan yang ditemukan di awal

Kartu meminta: *"Pendaftaran kedua untuk pasien yang sama menampilkan nomor kunjungan pertama, dan
petugas dapat langsung membukanya — bukan sekadar pesan gagal."*

| Temuan | Bukti |
| --- | --- |
| Backend menolak pendaftaran ganda dengan `409` dan pesan berisi nomor kunjungan yang sudah ada | `EmergencyVisitService.PesanEpisodeGanda`; `EmergencyVisitController` baris 208–213 |
| Nomor itu **hanya** ada di dalam teks pesan. Badan `409` tidak memuat `id` atau nomor terstruktur | `ApiResponse<object>.Fail(409, pesan)` |
| Penolakan hanya terjadi bila **alasan pendaftaran ganda kosong** | Kondisi `episodeAktif != null && string.IsNullOrWhiteSpace(alasan)` |
| Frontend membuang status HTTP dan hanya meneruskan teks pesan | `handleSubmitRegistration` menangkap galat tahap `emergencyVisit` lalu mengembalikan `message` |
| Tidak ada tautan, tombol, atau navigasi ke kunjungan yang sudah ada | `verification-step.jsx` hanya menampilkan `EmergencyInlineAlert` berisi teks |

Pilihan yang **ditolak**: mengurai nomor kunjungan dari teks pesan. Pesan itu ditulis untuk dibaca
manusia dan dapat berubah kalimatnya; mengurainya sama dengan menebak kontrak.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas pendaftaran IGD.

1. Petugas mendaftarkan pasien yang **masih punya kunjungan IGD berjalan**, dan tidak mengisi kolom
   *Alasan Pendaftaran Episode Ganda*.
2. Pada langkah **Verifikasi & Konfirmasi**, petugas menekan **Selesaikan Pendaftaran**. Backend menolak.
3. Seperti sebelumnya, pesan backend tampil pada kotak merah *Pendaftaran belum selesai*.
4. **Baru:** aplikasi mencari kunjungan pasien itu yang masih berjalan, lalu menampilkan kotak kuning
   *Pasien ini masih punya kunjungan IGD yang berjalan* berisi **nomor kunjungan** dan **status**-nya,
   beserta tombol **Buka Kunjungan IGD**.
5. Petugas memilih:
   - **Buka Kunjungan IGD** — pasien yang belum ditriage (*Pasien tiba*, *Menunggu triage*) dibuka di
     layar **Triage**; pasien yang sudah ditriage dibuka di layar **Assesmen IGD**; atau
   - kembali ke langkah *Emergency Visit* dan mengisi alasan bila pasien memang datang kembali sebagai
     peristiwa baru.

*Contoh.* Pasien RAYYAN DHAFIR didaftarkan pukul 09.35 dan berstatus *Menunggu triage*. Pukul 09.40
petugas lain mendaftarkannya lagi. Kotak kuning menampilkan *"Nomor kunjungan IGD-0001 (Menunggu triage)"*
dan tombol yang membuka layar Triage pasien itu.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Alasan pendaftaran ganda **terisi** | Pencarian tidak dijalankan; perilaku lama utuh |
| Pencarian kunjungan gagal (jaringan, hak akses) | Diabaikan diam-diam; kotak merah dengan pesan backend tetap tampil dan tetap menyebut nomornya |
| Tidak ada kunjungan berjalan yang ditemukan | Kotak kuning tidak tampil |
| Kunjungan belum tertaut encounter dan statusnya belum ditriage | Kotak kuning tampil dengan nomornya, **tanpa** tombol (layar Triage butuh encounter) |
| Route token gagal dibentuk | Petugas tetap di layar verifikasi; tidak terjadi perpindahan |
| Kegagalan bukan karena episode ganda (mis. galat validasi) tetapi pasien punya kunjungan berjalan | Kotak kuning tetap tampil. Kalimatnya hanya menyatakan **fakta** (pasien punya kunjungan berjalan), bukan penyebab kegagalan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; `rules/frontend/frontend-architecture.md`;
`base-component-decision-gate.md`; `ui-consistency-checklist.md`; kartu `FE-IGD-014`; bukti `IGD-EV-123`;
`emergency-visit-step.jsx`, `verification-step.jsx`, `use-emergency-registration.js`,
`emergency-registration.service.js`, `emergency-registration.utils.js`,
`emergency-registration-slice.jsx`; daftar triage dan daftar pengkajian (pola route token);
`EmergencyVisitController.cs`, `EmergencyVisitService.cs`, `EmergencyVisitDtos.cs`,
`EmergencyVisitStatus.cs` di backend.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/registration-management/emergency-registration.service.js` | Fungsi baru `fetchEmergencyVisitsByPatient({ patientId })` — `GET emergency-visits?patientId=…` (25 terbaru), memakai `InstanceAxios`, `requestJson`, dan `getCollectionItems` yang sudah ada di berkas itu |
| `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` | Dua fungsi murni: `pickActiveEmergencyVisit` (kunjungan pertama yang bukan `Completed`/`Cancelled`, memakai `isEmergencyVisitClosed` yang sudah ada) dan `resolveExistingVisitDestination` (`"triage"` / `"assessment"` / `""`) |
| `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js` | Pada jalur gagal tahap `emergencyVisit`, bila alasan pendaftaran ganda kosong, hook mencari kunjungan berjalan lalu mengembalikan `existingVisit` bersama hasil gagal. Pencarian dibungkus `try/catch` |
| `src/components/view/health-services/registration-management/emergency-registration/verification-step.jsx` | State `existingVisit`, penangan `handleOpenExistingVisit` (route token + `router.push`), dan kotak peringatan dengan tombol |
| `src/lib/constants/health-services/emergency-installation-management/emergency-management-triage-constant.jsx` | Ekspor `EMERGENCY_TRIAGE_ROUTE_TOKEN_SCOPE`. Daftar dan form triage memegang salinan lokal bernilai sama; keduanya **tidak disentuh** |
| `tests/unit/emergency-registration-existing-visit.test.mjs` | Berkas baru — 9 uji untuk kedua fungsi murni |

**Tidak disentuh:** `globals.css` dan berkas CSS mana pun; slice Redux; `emergency-visit-step.jsx`;
seluruh backend; kontrak.

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi: `verification-step` (view) → `use-emergency-registration` (hook) →
`emergency-registration.service` (`InstanceAxios`) → backend; normalisasi dan pemilihan ada di `utils`
sebagai fungsi murni. View tidak memanggil Axios. Endpoint memakai `EMERGENCY_REGISTRATION_API_URLS`
yang sudah ada, bukan string baru. Navigasi memakai `registerPrivateRouteToken` dan `router.push`,
pola yang sama dengan daftar triage dan daftar pengkajian.

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Peringatan berisi nomor kunjungan | `EmergencyInlineAlert` (komponen lokal modul pendaftaran) | Dipakai `verification-step.jsx`, `emergency-visit-step.jsx`; mendukung `tone="warning"` | REUSE | Nada `warning`, bukan `error`, karena ini informasi pelengkap dari kotak galat |
| Tombol **Buka Kunjungan IGD** | `BaseButton` | `components/features/base-features/base-button`; dipakai `emergency-visit-step.jsx` baris 480 dengan `size="sm" variant="secondary"` dalam `inlineAlertActionRow` | REUSE | Sama persis dengan tombol *Muat Ulang* pada berkas tetangga |

Elemen `NEW` atau `EXTEND`: **nol**, jadi tidak ada keputusan yang menunggu pemilik.

**Pengamatan pola.** Tombol *Kembali* dan *Selesaikan Pendaftaran* pada berkas yang sama memakai
`<button>` mentah (kode lama, tidak diubah). Tombol baru memakai `BaseButton` sesuai checklist.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol utama berlabel *Membuat Encounter…* / *Menyimpan Emergency Visit…* (tidak berubah). Pencarian kunjungan berjalan setelah penolakan dan tidak punya penanda tersendiri — hasilnya muncul beberapa saat sesudah kotak merah |
| Kosong | Tidak ada kunjungan berjalan → kotak kuning tidak tampil |
| Gagal | Pencarian gagal → tidak ada kotak kuning; kotak merah dengan pesan backend tetap tampil |
| Tanpa hak akses | Pencarian ditolak `403` diperlakukan sama seperti gagal — kotak merah tetap menyebut nomor kunjungan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/emergency-installation-management/emergency-visits?patientId={id}&sortBy=arrivalDateTime&sortDirection=desc&pageNumber=1&pageSize=25` | Mencari kunjungan pasien yang masih berjalan setelah pendaftaran ditolak | `EmergencyVisit : Read` |
| `POST` | `…/emergency-visits` | Tidak berubah — pendaftaran kunjungan | `EmergencyVisit : Create` |

Field yang dibaca: `id`, `encounterId`, `patientId`, `patientName`, `emergencyVisitNumber`, `visitStatus`
(angka, sejajar `EmergencyVisitStatus`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 5 berkas source yang diubah | 0 error, 3 warning | `PASS` | Ketiga warning `react-hooks/preserve-manual-memoization` pada hook **sudah ada di `HEAD`** — salinan `HEAD` di-lint dan menghasilkan tiga warning yang sama pada baris setara. Nol warning baru |
| `node --import ./tests/helpers/register.mjs --test tests/unit/emergency-registration-existing-visit.test.mjs` | 9 dari 9 lulus | `PASS` | Keluaran perintah |
| `npm run test:unit` (seluruh suite) | 1419 test: **1410 lulus, 9 gagal** | `EXISTING / ENVIRONMENT ISSUE` | Kesembilan yang gagal: delapan `FE-RWI-042`/`043`/`044` dan satu `accounting-reconciliation` ("route, menu, dan store terdaftar") yang memeriksa menu rute Rawat Inap. Nol rujukan ke berkas yang saya ubah. Sudah tercatat gagal sebelum task ini |
| `npm run lint:errors` (seluruh proyek) | 4 error | `EXISTING / ENVIRONMENT ISSUE` | Semuanya di `inpatient-management/.../clinical-instrument-form-renderer.jsx` (`BaseCheckboxCard`, `BaseTextField`, `BaseFormControl` tidak terdefinisi). Nol pada berkas saya |
| Grep anti-regresi checklist UI, baris yang ditambahkan (194 baris) | Warna literal 0, typography 0, `<button` mentah 0, `<table` 0, `fw-`/`fs-` 0, `!important` 0, inline style 0; berkas CSS berubah 0 | `PASS` | Keluaran perintah |
| Pendaftaran ganda lewat layar: kotak kuning tampil dengan nomor dan status | **Tampil**: *"Nomor kunjungan IGD-260917023643-4A7A93 (Sudah ditriage)"* dengan tombol *Buka Kunjungan IGD* | `PASS` (tampilan) | Tangkapan layar pemilik, 21 September 2026 malam (bagian 6.1) |
| Klik *Buka Kunjungan IGD* membuka layar yang benar | Tidak ada di tangkapan layar | `NOT RUN` | Menunggu konfirmasi pemilik. Status `Triaged` (3) seharusnya membuka layar **Assesmen IGD** |
| `npm run build` | Lulus; `postbuild` `prepare-standalone` berhasil menyalin static dan public assets | `PASS` | Dijalankan pemilik 21 September 2026 (malam) pada working tree yang memuat perubahan task ini; yang dilampirkan hanya **ekor** keluaran (legenda rute dan `postbuild`), bukan tabel rute atau hitungan galat — agent tidak mengulang build |

Uji manual: `NOT FEASIBLE` oleh agent (tidak ada sesi login; uji layar menulis ke basis data yang dilarang bagi agent); **dijalankan pemilik** — bagian 6.1.

### 6.1 Uji layar pemilik — 21 September 2026 (malam)

Pemilik mendaftarkan pasien yang masih punya kunjungan IGD berjalan (`IGD-260917023643-4A7A93`, status *Sudah ditriage*)
tanpa mengisi alasan pendaftaran ganda. Yang tampak pada langkah **Verifikasi & Konfirmasi**, dari atas ke bawah:

| Kotak | Isi | Asal |
| --- | --- | --- |
| Kuning — *Encounter sudah terbentuk* | *"Patient Encounter sudah berhasil dibuat pada percobaan sebelumnya, tetapi Emergency Visit belum berhasil…"* | Perilaku lama. **Muncul karena percobaan sebelumnya sudah membuat encounter** |
| Merah — *Pendaftaran belum selesai* | *"Pasien ini masih punya kunjungan IGD yang berjalan, nomor IGD-260917023643-4A7A93 (status Triaged)…"* | Pesan backend apa adanya |
| Kuning — *Pasien ini masih punya kunjungan IGD yang berjalan* | *"Nomor kunjungan IGD-260917023643-4A7A93 (Sudah ditriage)…"* + tombol **Buka Kunjungan IGD** | **Perubahan task ini** |

**Yang dibuktikan.** Nomor kunjungan dan statusnya pada kotak baru **sama dengan** yang disebut backend, sehingga pencarian
lewat `GET emergency-visits?patientId=` menemukan kunjungan yang benar. Label status memakai bahasa layar (*Sudah ditriage*).

**Tiga pengamatan, tidak diperbaiki pada task ini.**

1. **Encounter yatim terjadi di layar.** Kotak kuning pertama hanya muncul bila encounter dari percobaan sebelumnya sudah tersimpan
   — bukti langsung `IGD-DEC-138`. Uji ini **meninggalkan minimal satu encounter tanpa kunjungan** di basis data dev; ia akan muncul
   pada kueri audit `BE-IGD-050` acceptance 8 dan **tidak boleh dibersihkan** tanpa audit referensi.
2. **Dua kotak menyampaikan hal yang sama.** Kotak merah menyuruh *"buka dari daftar kunjungan IGD"*, kotak kuning memberi tombolnya.
   Bila kotak kuning tampil, kotak merah sebenarnya berlebih. Menyembunyikannya adalah pilihan tampilan `DEV_DISCRETION`; **tidak
   dilakukan** karena kotak merah tetap satu-satunya penjelasan bila pencarian gagal.
3. **Status pada pesan backend berbahasa Inggris** (*Triaged*), sedangkan kotak baru memakai *Sudah ditriage*. Sumbernya
   `EmergencyVisitService.PesanEpisodeGanda` yang menulis nama enum. Pesan itu bagian dari kontrak validasi; mengubahnya keputusan
   pemilik, **dicatat sebagai coverage gap**.

**Yang dibuktikan unit test** (logika murni saja): kunjungan berjalan terbaru dipilih; `Completed` dan
`Cancelled` dilewati sedangkan `Disposed` tidak; masukan rusak tidak melempar galat; tujuan `triage`
untuk status 1–2 dan `assessment` untuk 3–7; triage tanpa encounter tidak menawarkan tombol.

**Tidak dibuktikan:** rangkaian hook → service → tombol → perpindahan layar, dan tampilan kotak kuning.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pendaftaran IGD baru terkirim sebagai `Emergency` | Terpenuhi (tidak berubah) | `emergency-registration.utils.js` `encounterType: ENCOUNTER_TYPE.Emergency` |
| 2. Pendaftaran kedua menampilkan nomor kunjungan pertama, dan petugas dapat langsung membukanya | **Terpenuhi (dinilai ulang 21 September 2026 malam).** Bagian "menampilkan nomor" terbukti lewat layar sebelumnya; bagian "membukanya" kini termasuk dalam **pernyataan pemilik bahwa uji layar `FE-IGD-034` lulus**. *Riwayat:* sebelumnya "membukanya belum dikonfirmasi". Sumber data kotak kini pra-cek `FE-IGD-034`, bukan heuristik bagian 3.2 | Bagian 3.2, 6.1, 9; `verification-step.jsx`; [laporan `FE-IGD-034`](FE-IGD-034.md) |
| 3. Jalan keluar beralasan tersedia dan alasannya wajib diisi | Terpenuhi (tidak berubah) | `emergency-visit-step.jsx` `duplicateEpisodeOverrideReason`; backend menolak tanpa alasan |

**Definition of Done.** Laporan tracked ada (berkas ini); roadmap dan traceability diperbarui. *Riwayat:* status **🟡** karena
uji layar kriteria 2 dan penanganan encounter yatim (`IGD-DEC-138`) belum. **Dinilai ulang 21 September 2026 (malam) →
✅ dengan celah yang dinyatakan** — lihat bagian 9: uji layar dilaporkan lulus oleh pemilik; penanganan encounter yatim lapis A
selesai lewat `BE-IGD-050` + `FE-IGD-034`; **`IGD-OQ-093` tetap `open`** dan dinyatakan apa adanya.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sesudah penolakan episode ganda, **encounter sudah terlanjur dibuat** (sisi backend menolak kunjungan, bukan encounter). Layar sudah memberi peringatan *"Encounter sudah terbentuk"* dan menyuruh menekan simpan lagi. Bila petugas memilih **Buka Kunjungan IGD** alih-alih mengulang, encounter itu tetap **tanpa kunjungan IGD**. Ini perilaku yang sudah ada sebelum task ini, tidak diperburuk, tetapi kini jalur "tidak mengulang" menjadi lebih mudah ditempuh. **Diputuskan pemilik 21 September 2026 (`IGD-DEC-138`): encounter itu TIDAK boleh dibiarkan yatim.** Audit backend menegaskan penyebabnya (encounter di modul Registrasi commit lebih dulu, validasi duplikat baru di modul IGD). Penanganannya: `BE-IGD-050` (pra-cek baca-saja) → `FE-IGD-034` (layar memanggilnya sebelum membuat encounter); sisa celah `IGD-OQ-093`. **`FE-IGD-014` belum boleh dianggap final end-to-end** sebelum itu. Pembersihan encounter yatim yang sudah ada dilarang tanpa audit referensi |
| Masalah yang diketahui | (1) Kotak kuning muncul pada **setiap** kegagalan tahap kunjungan bila alasan kosong dan pasien punya kunjungan berjalan — bukan hanya `409` episode ganda, karena status HTTP tidak diteruskan thunk. Kalimatnya sengaja hanya menyatakan fakta. (2) Aturan "berjalan" (bukan `Completed`, bukan `Cancelled`) **diduplikasi** dari backend; bila backend menambah status akhir baru, `isEmergencyVisitClosed` perlu diperbarui. (3) Hanya 25 kunjungan terbaru pasien yang dicari |
| Alternatif yang tidak dipilih | Menambahkan `existingVisit { id, number, status }` pada badan `409` backend menghilangkan tebakan nomor (1) dan (2), tetapi butuh task backend dan versi kontrak baru. Dipilih jalur frontend murni karena endpoint dan filternya sudah ada dan kriteria hanya menuntut nomor dan tautan |
| Dependency backend | Tidak ada yang belum selesai |
| Perubahan sampingan | `NONE`. Berkas sementara untuk membandingkan warning `HEAD` dihapus segera setelah dipakai |
| Interupsi | `NONE` |
| Status Git | Saat laporan ini ditulis: 5 berkas `M` dan 1 `??` di atas `16c767916`, tanpa stage atau commit oleh agent. **Diperbarui 21 September 2026 (malam):** pemilik sudah meng-commit dan push semuanya sebagai `198d56d9e`; working tree frontend bersih, sinkron dengan `origin/RizkiV2` |
| Langkah berikutnya | (1) ~~Pemilik `npm run build`~~ — **lulus 21 September 2026 (malam)**. (2) Uji layar: daftarkan satu pasien IGD, lalu daftarkan pasien yang sama sekali lagi tanpa alasan — kotak kuning tampil dengan nomor dan tombol membuka layar yang benar. (3) ~~Putuskan nasib encounter menggantung~~ — **diputuskan**: `IGD-DEC-138`; lanjut `BE-IGD-050` → `FE-IGD-034`. Build dan uji layar kriteria 2 di atas tetap boleh dijalankan sekarang |

---

## 9. Penilaian ulang — 21 September 2026 (malam)

Diminta pemilik sesudah `FE-IGD-034` dinyatakan ✅. Kartu dan roadmap menetapkan dua syarat: `FE-IGD-014` boleh ✅ end-to-end
**sesudah** `FE-IGD-034` ✅, **kecuali** celah `IGD-OQ-093` yang harus tetap dinyatakan apa adanya.

### 9.1 Yang berubah sejak laporan ini ditulis

| Hal | Keadaan sekarang | Bukti |
| --- | --- | --- |
| Sumber data kotak kuning | **Pra-cek `GET emergency-visits/active-episode`** milik backend (`BE-IGD-050`), bukan tebakan dari `GET emergency-visits?patientId=` | `FE-IGD-034` ✅; `pickActiveEmergencyVisit`, `isEmergencyVisitClosed`, dan `fetchEmergencyVisitsByPatient` (bagian 3.2 dan 8 laporan ini) **sudah dihapus** — nol sisa pada `src/` dan `tests/` |
| Kapan kotak tampil | **Sebelum** encounter dibuat (pendaftaran dihentikan), bukan sesudah encounter yatim terlanjur lahir; juga pada jalur galat tahap kunjungan | [Laporan `FE-IGD-034`](FE-IGD-034.md) bagian 2 |
| Encounter yatim pada kasus normal | **Tidak lagi terbentuk** untuk pasien berepisode berjalan tanpa alasan (lapis A `IGD-DEC-138`) | Kode; uji layar dilaporkan lulus oleh pemilik |
| Masalah yang diketahui (2) laporan ini — aturan "berjalan" diduplikasi di frontend | **Selesai** — aturan hanya di backend | Test guard `FE-IGD-034 K5` |
| Masalah yang diketahui (3) — hanya 25 kunjungan terbaru dicari | **Tidak berlaku lagi** — tidak ada pencarian daftar | — |
| Kekhawatiran `BE-IGD-050`: pencarian daftar butuh `EmergencyVisit : Read` dan dapat gagal senyap | **Hilang** untuk layar ini; ia kini butuh `EmergencyVisit : Create` yang memang dipakai `POST` | Laporan `FE-IGD-034` bagian 5 |
| Test kriteria 2 | Berkas `emergency-registration-existing-visit.test.mjs` ditulis ulang: 14 test, empat di antaranya tetap menguji tujuan layar dari laporan ini | `node --test` 14/14 lulus |
| Commit | `198d56d9e` → **`c941012ac`** (pemilik, 21 September 2026 16:01; belum di-push) | `git show --stat` |

### 9.2 Penilaian per kriteria

| # | Kriteria | Sebelum | Sesudah | Dasar |
| ---: | --- | --- | --- | --- |
| 1 | Pendaftaran IGD baru terkirim sebagai `Emergency` | Terpenuhi | **Terpenuhi** — tidak berubah | `buildEmergencyEncounterPayload` tidak disentuh `FE-IGD-034`; alur pendaftaran normal (U3) termasuk dalam pernyataan pemilik |
| 2 | Pendaftaran kedua menampilkan nomor kunjungan pertama, dan petugas dapat langsung membukanya | Sebagian ("membukanya" belum dikonfirmasi) | **Terpenuhi** | Pernyataan pemilik bahwa uji layar `FE-IGD-034` lulus (U1–U2), tanpa lampiran |
| 3 | Jalan keluar beralasan tersedia dan alasannya wajib diisi | Terpenuhi | **Terpenuhi** — tidak berubah | Alasan terisi melewati pra-cek (U4; test `FE-IGD-034 K3`) |

### 9.3 Keputusan status

**✅ — dengan celah yang dinyatakan apa adanya.** Ketiga kriteria kartu terpenuhi, dan kedua syarat kartu terpenuhi:
`FE-IGD-034` ✅, dan `IGD-OQ-093` **tidak disembunyikan**.

**Arti ✅ ini, dan batasnya:**

| ✅ ini menyatakan | ✅ ini **tidak** menyatakan |
| --- | --- |
| Layar pendaftaran IGD mengirim `Emergency`, menampilkan dan membuka kunjungan yang sudah berjalan, dan menyediakan jalan keluar beralasan | Bahwa **tidak ada encounter yatim dalam segala keadaan** |
| Pendaftaran ganda biasa tidak lagi meninggalkan encounter yatim | Bahwa encounter yatim yang **sudah ada** di basis data sudah diaudit atau dibersihkan (`BE-IGD-050` acceptance 8 belum dilaporkan; pembersihan dilarang tanpa audit referensi) |

**Celah yang tetap terbuka — `IGD-OQ-093` (`open`):** (a) dua pendaftaran serentak untuk pasien yang sama; (b) klien yang memanggil
`POST patient-encounters` lalu `POST emergency-visits` tanpa pra-cek. Penutupnya butuh jaminan sisi server (B1 guard di Registrasi /
B2 endpoint orkestrasi) dan keputusan pemilik Registrasi. Ini **backend gap eksplisit**, bukan cacat `FE-IGD-014`.

**Catatan bukti.** Seperti `FE-IGD-034`, kesahihan bagian uji layar bertumpu pada **pernyataan pemilik** tanpa lampiran; agent
tidak mengamati layar. Bila pemilik membaca larangan *"`FE-IGD-014` tidak boleh ditandai final end-to-end atas nama celah
`IGD-OQ-093`"* (`00-interview-decisions.md`) lebih ketat daripada teks kartu, status dapat dikembalikan ke 🟡 sampai
`IGD-OQ-093` diputuskan — penilaian ini mengikuti teks kartu, yang secara eksplisit mengizinkan ✅ **dengan** celah dinyatakan.
