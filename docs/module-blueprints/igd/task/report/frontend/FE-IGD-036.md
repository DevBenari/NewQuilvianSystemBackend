# Laporan Perubahan Frontend — `FE-IGD-036`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-036` |
| Judul | Mulai Triage dan Tangani Segera melahirkan kunjungan; loket berhenti membuat kunjungan |
| Slice | `S3`, `S8` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian R3.12 |
| Trace | `FR-IGD-069`, `075`, `076`, `077` (sisi layar), `085`; `IGD-DEC-139`, `143`, `147`, `151`, `158`, `161`, `128`; `AT-IGD-166`, `172`…`174`, `184`; `03-frontend-architecture.md` §13.3 A, B, C, §13.4 |
| Contract version | API `0.11.0` §8.3.2 (`POST /start-triage`), §8.3.1 (baris `triage-queue`); validation `0.8.0` §10.2 — `approved` (`IGD-DEC-157`). Berkas kontrak kini API `0.13.0` / validation `0.10.0`; hash cocok dengan manifest 0j, 0j.1, 0j.2. **Pengerjaan ulang 1 Oktober 2026:** API §8.3.2 dengan lima ruas baru dan validation §10.2 aturan 14–15 (`IGD-DEC-179`, manifest 0j.3) |
| Wewenang UI | `DEV_DISCRETION` (03 §13.6) untuk bentuk Mulai Triage dan bentuk isian tanggal-jam. **Isi** isiannya dikunci `IGD-DEC-147`/`161` |
| Dependency | `BE-IGD-053` ✅, `BE-IGD-055` 🟡 (delta `IGD-DEC-179` belum di-build dan diuji; perilaku lamanya ✅), `FE-IGD-035` ✅, `FE-IGD-038` ✅, `BE-IGD-059` 🟡 (source selesai, menunggu build pemilik) — pemilik mengizinkan task ini dikerjakan; **perilisannya** tetap menunggu `BE-IGD-059` aktif |
| Klasifikasi | `HEAVY` — skor 8 (repository 0, berkas diperiksa 2, berkas diubah 2, logika 1, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 2), dinaikkan satu tingkat karena dua faktor berada di tingkat `HEAVY`: lebih dari 20 berkas diperiksa dan 17 berkas diubah, pada alur pintu masuk pasien |
| Task mode | `FRONTEND` — wewenang pemilik 1 Oktober 2026 (*"Lanjut sesuai urutan: `BE-IGD-059` → `FE-IGD-036` → …"*). Backend baca-saja, kecuali laporan ini dan status pada roadmap/traceability |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `aa0b1168b` (`RizkiV2`, upstream `origin/RizkiV2`) + working tree `FE-IGD-038` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `b9076c71` + working tree `BE-IGD-053`, `057`, `058`, `059` |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 2 Oktober 2026.** Build pemilik terbukti. Uji layar: **12 dari 14 terbukti** pada bukti mentah (U1–U6, U9–U14), termasuk lima isian `IGD-DEC-179`. **Belum terbukti:** U7 (tangkapan layar diambil saat permintaan masih berjalan; `409` tercatat di log backend, tampilannya di dialog tidak terekam) dan U8 (dijalankan lewat dua panggilan API, bukan dua tab). *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete, termasuk pengerjaan ulang `IGD-DEC-179` (bagian 9).** Dialog Mulai Triage kini memuat lima isian opsional — lokasi kedatangan, lokasi pasien ditemukan, lokasi trauma, waktu trauma, catatan kunjungan — dan mengirimnya ke `start-triage` (3 berkas, +90 baris; `eslint` 0 error). **Belum:** `npm run build` dan uji layar U1–U14 (milik pemilik). **Jangan dirilis** sebelum `BE-IGD-059` aktif dan delta `BE-IGD-055` di-build. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** 17 berkas (13 source diubah, 1 baru, 3 test); `eslint` 0 error; unit test berkas terkait 70/70. **Belum:** `npm run build` dan uji layar U1–U9 (milik pemilik). **Jangan dirilis** sebelum `BE-IGD-059` aktif |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Loket membuat **dua** hal berurutan: encounter, lalu kunjungan IGD | `handleSubmitRegistration` memanggil `submitEmergencyPatientEncounter` lalu `submitEmergencyVisit` |
| Waktu tiba diisi di loket, dan bila kosong jatuh ke jam komputer loket | `emergency-visit-step.jsx` mengisi tanggal/jam dari `new Date()`; hook mengisi `arrivalDateTime` dari `new Date()`; `buildEmergencyVisitPayload` memakai `toIsoDateOrNow` (`IGD-EV-141`) |
| Baris *Menunggu Triage* tanpa kunjungan tampil tanpa tombol | `emergency-triage-patient-table.jsx` menulis *"Belum ada kunjungan IGD"* (sengaja ditinggal `FE-IGD-035`) |
| Backend sudah siap melahirkan kunjungan dari encounter | `POST /emergency-visits/start-triage` (`BE-IGD-055` ✅); baris `triage-queue` membawa `availableActions` |
| Tangani Segera yang ada hanya untuk baris yang **sudah** punya kunjungan | `startEmergencyImmediateCare` memanggil `PATCH /{id}/visit-status` (`FE-IGD-030`) |

---

## 2. Proses bisnis dari sisi pengguna

**Petugas loket — Pendaftaran Pasien IGD.**

1. Memilih pasien, mengisi **kategori kunjungan** dan **keluhan utama**, lalu pembayaran. Tidak ada lagi isian waktu
   tiba, cara datang, jenis kasus, lokasi, maupun penanda tanpa identitas.
2. Menekan **Selesaikan Pendaftaran**. Layar mengirim **satu** permintaan: `POST /patient-encounters`.
3. Layar Selesai berbunyi *"Pasien sudah terdaftar di IGD dan menunggu triage"* dan menampilkan nomor encounter.

**Perawat triage — Triage Pasien.**

1. Pasien yang baru didaftarkan tampil *Menunggu Triage — Terdaftar 09.35* dengan dua tombol.
2. **Mulai Triage** membuka dialog. Waktu tiba **sudah terisi** waktu terdaftar; perawat boleh memundurkannya.
   Cara datang, jenis kasus, keluhan, dan penanda pasien tanpa identitas bersifat pilihan.
3. Menekan *Mulai Triage* di dialog → kunjungan IGD lahir (*Menunggu Triage*) dan layar **langsung membuka Detail
   triage** pasien itu.
4. **Tangani Segera** menampilkan konfirmasi **tanpa isian** → kunjungan lahir *Sedang ditangani* dengan waktu tiba
   sementara, dan layar langsung membuka Pengkajian IGD.

*Contoh.* RAYYAN didaftarkan 09.35. Perawat menekan Mulai Triage, mengubah waktu tiba menjadi 09.20, menekan
tombolnya, dan formulir triage RAYYAN terbuka. Kembali ke daftar, barisnya kini berbunyi *Tiba 09.20* dengan tombol
*Isi Triage*.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Waktu tiba dikosongkan | Dialog menolak: *"Waktu tiba wajib diisi untuk memulai triage."* — nol permintaan |
| Waktu tiba di masa depan | Server menjawab `400` *"Waktu tiba tidak boleh melewati waktu sekarang."*; kalimatnya tampil di dialog. Layar **tidak** membandingkan dengan jam komputer |
| Pasien tanpa identitas tanpa nama sementara | Dialog menolak sebelum mengirim |
| `409` (encounter sudah berakhir, kunjungannya pernah dihapus, atau pasien punya kunjungan lain yang berjalan) | Pesan server tampil apa adanya; daftar dimuat ulang |
| Tangani Segera ditolak | Konfirmasi tertutup, pesan server tampil pada spanduk merah halaman |
| Dua perawat menekan Mulai Triage pada pasien yang sama | Server menjawab kunjungan yang sama; keduanya membuka triage pasien itu |
| Pilihan cara datang atau jenis kasus gagal dimuat | Dialog memberi tahu; keduanya tidak wajib, triage tetap dapat dimulai |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md` frontend; `rules/frontend/` (`frontend-architecture`, `base-component-decision-gate`,
  `base-component-catalog`, `ui-consistency-checklist`, `test-policy`, `REPORT_TEMPLATE`).
- Blueprint: kartu `FE-IGD-036`; API §8.3.1, §8.3.2; validation §10.2; `03-frontend-architecture.md` §13.
- Source frontend: seluruh berkas pada 3.2, ditambah `emergency-registration-slice.jsx`,
  `emergency-registration.service.js`, `emergency-registration-fields.jsx`, `use-emergency-visit-options.js`,
  `emergency-visit-options.service.js`, `emergency-triage-retriage-dialog.jsx`,
  `emergency-assessment-date-time-field.jsx`, `emergency-assessment-soap-tab.jsx`, `emergency-triage.module.css`.
- Source backend (baca saja): `StartEmergencyVisitRequest`, `EmergencyVisitService.StartVisitAsync` dan
  `MulaiKunjunganDalamKunciAsync`, `EmgVisitConfiguration`.

### 3.2 Berkas yang berubah

**Loket.**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/…/use-emergency-registration.js` | Berhenti membuat kunjungan: sesudah encounter tersimpan, wizard langsung ke langkah Selesai. Efek yang mengisi waktu tiba dari jam browser dihapus. Pilihan `emergencyVisitContext` dihapus |
| `src/utils/…/emergency-registration.utils.js` | `buildEmergencyVisitPayload` **dihapus** — satu-satunya jalur yang melewatkan waktu tiba ke `toIsoDateOrNow` |
| `src/lib/constants/…/emergency-registration.constants.js` | Nilai awal formulir kunjungan tinggal `chiefComplaint` dan `duplicateEpisodeOverrideReason` |
| `src/components/view/…/emergency-visit-step.jsx` | Isian waktu tiba, cara datang, jenis kasus, tiga lokasi, waktu trauma, izin penanganan segera, penanda tanpa identitas, dan catatan kunjungan dihapus. Tersisa kategori kunjungan, keluhan utama, alasan pendaftaran ganda |
| `src/components/view/…/verification-step.jsx` | Ringkasan dan kartu *Proses Penyimpanan* menyebut satu permintaan; peringatan "encounter sudah terbentuk, kunjungan belum" dihapus; label tombol saat menyimpan |
| `src/components/view/…/registration-success-step.jsx` | Kalimat *terdaftar dan menunggu triage*; nomor yang ditampilkan nomor encounter; kotak *Emergency Visit ID* menjadi *Status: Menunggu Triage* |
| `src/components/view/…/emergency-registration-page.jsx` | Prop `emergencyVisitContext` dihapus |

**Daftar triage.**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/…/emergency-management-triage-constant.jsx` | + `TRIAGE_START_MODE`, batas panjang keluhan (1000) dan nama sementara (100), nilai awal formulir, pesan |
| `src/utils/…/emergency-management-triage-utils.jsx` | + `buildStartTriagePayload`, `buildTriageStartFormValues`, `validateTriageStartForm`, `toTriageArrivalInputValue`, `fromTriageArrivalInputValue` |
| `src/lib/state/slice/…/emergency-management-triage-slice.jsx` | + thunk `startEmergencyVisitFromEncounter` (`POST /start-triage`), cabang state `startVisit`, reducer pembersihnya |
| `src/lib/hooks/…/use-emergency-management-list.jsx` | + `startVisitFromEncounter` (memuat ulang daftar saat berhasil dan saat `409`), `clearStartVisit` |
| `src/components/view/…/components/emergency-triage-start-dialog.jsx` (**baru**) | Dialog Mulai Triage |
| `src/components/view/…/components/emergency-triage-patient-table.jsx` | Baris tanpa kunjungan: tombol **Mulai Triage** dan **Tangani Segera** menurut `availableActions`; seluruh tombol baris nonaktif selama satu aksi berjalan |
| `src/components/view/…/emergency-triage-patient-list-view.jsx` | Merangkai dialog; Tangani Segera untuk baris tanpa kunjungan memakai `start-triage` tanpa isian; spanduk galat |

**Test.**

| Berkas | Perubahan |
| --- | --- |
| `tests/unit/emergency-registration-payload.test.mjs` | Empat kasus yang menguji payload kunjungan dari loket (`FE-IGD-001 K2` dua kasus, `FE-IGD-029 K1`, `K4`) **diganti** tiga kasus `FE-IGD-036`: pembangun payload kunjungan tidak ada lagi; payload encounter tidak membawa ruas kunjungan; nilai awal formulir loket tanpa waktu tiba |
| `tests/unit/emergency-registration-existing-visit.test.mjs` | Kasus `FE-IGD-038 K2` kini hanya menuntut alasan pada payload encounter |
| `tests/unit/emergency-triage-utils.test.mjs` | + tujuh kasus `FE-IGD-036` untuk payload dan validasi Mulai Triage |

Nol baris komentar baru pada source maupun test. Akhiran baris CRLF dipertahankan. **Tidak disentuh:** CSS mana pun,
`globals.css`, route, slice dan service pendaftaran, backend.

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi tetap: view → hook → slice → util/constant. Permintaan memakai `InstanceAxios` lewat thunk; logika
payload dan validasi berada di `src/utils` sebagai fungsi murni.

`UI GATE: 7 elemen — REUSE 6, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Wadah dialog Mulai Triage | `Modal` + `Form` react-bootstrap dengan kelas `dialogBody`, `dialogHint`, `dialogField` | `emergency-triage-retriage-dialog.jsx` (dialog pada layar yang sama) | `COMPOSE` | Lihat keputusan di bawah |
| Isian waktu tiba | `Form.Control type="datetime-local"` | `emergency-assessment-soap-tab.jsx` | `REUSE` | — |
| Pilihan cara datang, jenis kasus | `Form.Select`; opsi dari `useEmergencyVisitOptions` | Dialog retriage; hook loket yang sudah ada | `REUSE` | — |
| Keluhan, nama sementara, penanda tanpa identitas | `Form.Control`, `Form.Check` | Dialog retriage | `REUSE` | — |
| Tombol baris Mulai Triage / Tangani Segera | Tombol baris tabel `primaryMiniButton` | Tombol *Isi Triage* dan *Tangani Segera* (`FE-IGD-030`) di kolom yang sama | `REUSE` | — |
| Konfirmasi Tangani Segera | `ConfirmModal` | Sudah dipakai layar ini | `REUSE` | — |
| Pesan galat | `errorBanner` | Sudah dipakai layar ini | `REUSE` | — |

> **Keputusan: bentuk Mulai Triage** (`DEV_DISCRETION`)
>
> - **A. Dialog mengikuti dialog *Nilai Ulang Pasien* — dijalankan.** Perawat tetap di daftar, gaya dan tombolnya sama
>   dengan dialog yang sudah dikenalnya, nol CSS baru. Biaya paling kecil.
> - **B. `ConfirmModal` berisi formulir.** Nol komponen baru, tetapi komponen itu dirancang untuk satu kalimat dan
>   satu isian alasan; lima isian di dalamnya menyimpang dari pemakaiannya di tempat lain.
> - **C. Halaman atau laci tersendiri.** Paling lapang, tetapi menambah route dan satu perpindahan layar sebelum
>   triage dapat diisi.

**Temuan grep anti-regresi yang dipertahankan.** Dua `<button>` baru pada tabel memakai kelas tombol baris yang sama
dengan tombol di sebelahnya. Memakai `BaseButton` di sana akan menampilkan dua bentuk tombol dalam satu kolom. Nol
`<table>`, nol utilitas tipografi Bootstrap, nol stylesheet berubah.

### 3.4 Selisih terhadap kartu

| Butir | Isi |
| --- | --- |
| **Ruas kunjungan yang tidak punya tujuan** | **Ditutup 1 Oktober 2026 lewat `IGD-DEC-179` — lihat bagian 9.** Keadaan semula: Kartu butir 2 menyebut lokasi dan waktu trauma *pindah* ke Mulai Triage. Kontrak `start-triage` (API §8.3.2) **tidak** punya ruas lokasi kedatangan, lokasi ditemukan, lokasi trauma, waktu trauma, maupun catatan kunjungan. Layar tidak mengirim ruas yang tidak ada di kontrak, jadi keempatnya **tidak lagi dapat diisi** dari layar mana pun. Bila masih dibutuhkan, itu perubahan kontrak dan backend — keputusan pemilik |
| Keluhan tidak tampil terisi di dialog | Baris `triage-queue` tidak membawa keluhan (API §8.3.1). Isian dibiarkan kosong; bila kosong, server menyalin keluhan dari pendaftaran — hasil akhirnya sama dengan "terisi awal dari encounter" |
| Tanpa pemeriksaan "masa depan" di layar | 03 §13.4: waktu tiba tidak pernah berasal dari jam browser. Jam browser juga tidak dipakai sebagai pembanding, supaya komputer yang jamnya terlambat tidak menolak nilai yang sah. Server yang memutuskan |
| Tombol menurut hak akses | Tombol diturunkan dari `availableActions` saja. Layar tidak menyaring menurut izin pengguna; pengguna tanpa izin melihat tombol dan mendapat pesan penolakan server |
| Isian alasan pendaftaran ganda tetap di langkah Emergency Visit | Langkah itu tidak kosong (kategori kunjungan dan keluhan tetap di sana), sehingga syarat pemindahan pada kartu tidak terpenuhi dan kalimat peringatan `FE-IGD-038` tetap benar |
| Label *Encounter Type* | Dua blok yang ditulis ulang semula berbunyi *Outpatient (1)*; dibetulkan menjadi *Emergency (2)*, sesuai nilai yang dikirim sejak `FE-IGD-014` |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol dialog *"Memulai..."*; tombol baris *"Memproses..."*; seluruh tombol baris nonaktif. Loket: *"Mendaftarkan pasien..."* |
| Kosong | Tidak berubah — *"Tidak ada pasien yang menunggu triage."* |
| Gagal | Dialog: pesan server di dalam dialog. Tangani Segera: spanduk merah halaman. Loket: kotak *Pendaftaran belum selesai* |
| Tanpa hak akses | Daftar: tidak berubah. Aksi: pesan penolakan server |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/emergency-installation-management/emergency-visits/start-triage` | Mulai Triage (`mode: "Triage"` + waktu tiba) dan Tangani Segera (`mode: "ImmediateCare"`) pada baris tanpa kunjungan | `EmergencyVisit : Create` |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits/triage-queue` | Daftar; dimuat ulang sesudah aksi | `EmergencyVisit : Read` |
| `PATCH` | `/v1/health-services/emergency-installation-management/emergency-visits/{id}/visit-status` | Tangani Segera pada baris yang sudah punya kunjungan — tidak berubah | `EmergencyVisit : Update` |

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/patient-encounters` | Satu-satunya permintaan pembuatan dari loket | `PatientEncounter : Create` |

**Tidak lagi dipanggil loket:** `POST /emergency-visits`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 17 berkas task | 0 error, 3 warning (`react-hooks/preserve-manual-memoization` pada hook loket) | `PASS` — warning `EXISTING WARNING` | Tiga warning yang sama sudah ada sebelum task (laporan `FE-IGD-038`) |
| `node --test` empat berkas IGD terkait | 70 dari 70 lulus | `PASS` | `emergency-registration-payload`, `emergency-triage-utils`, `emergency-registration-existing-visit`, `emergency-visit-status` |
| Suite unit penuh | 2166 dari 2172 lulus; 6 gagal | `UNRELATED EXISTING ISSUE` | Enam yang sama dengan laporan `FE-IGD-038`: Hemodialisa (4), menu Setup Bank Darah (1), petty cash (1) |
| Baris komentar pada tambahan | Nol | `PASS` | Pencarian pada `git diff` dan berkas baru |
| Grep anti-regresi UI | Dua `<button>` baru (alasan di 3.3); selebihnya bersih | `PASS` | Bagian 3.3 |
| `npm run build` | — | `NOT RUN` | Milik pemilik |
| Uji layar U1–U9 | — | `NOT RUN` | Milik pemilik |

`AUTOMATED TEST: npm run test:unit — FAIL (6 kegagalan di modul lain; 70/70 berkas terkait lulus)`

`MANUAL TEST: NOT FEASIBLE — backend tidak berjalan pada sesi ini dan uji layar dijalankan pemilik`

Uji manual: `REQUIRED`.

### 6.1 Skenario uji layar untuk pemilik

Prasyarat: backend dengan `BE-IGD-059` terpasang; `npm run build` lalu jalankan hasil build.

| # | Langkah | Yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| U1 | Loket: daftarkan pasien tanpa episode sampai selesai | Langkah Emergency Visit hanya berisi kategori kunjungan, keluhan, dan alasan pendaftaran ganda. Panel jaringan: tepat **satu** `POST /patient-encounters`, **nol** `POST /emergency-visits`. Layar Selesai memuat nomor encounter dan *Menunggu Triage* | 1, 5 |
| U2 | Buka Triage Pasien | Pasien U1 tampil *Menunggu Triage — Terdaftar hh.mm* dengan tombol **Mulai Triage** dan **Tangani Segera** | 3, 4 |
| U3 | Tekan Mulai Triage; mundurkan waktu tiba 15 menit; tekan *Mulai Triage* | Dialog terisi awal waktu terdaftar. `POST /start-triage` membawa `mode: "Triage"` dan `arrivalDateTime` yang dimundurkan → `201`. Detail triage pasien terbuka. Kembali ke daftar: baris menjadi *Tiba hh.mm* dengan *Isi Triage* | 3, 5 |
| U4 | Pasien baru: Mulai Triage, kosongkan waktu tiba; lalu isi waktu satu jam ke depan | Kosong: pesan wajib, nol permintaan. Masa depan: `400` *"Waktu tiba tidak boleh melewati waktu sekarang."* tampil di dialog | 5 |
| U5 | Pasien baru: Tangani Segera pada baris tanpa kunjungan | Konfirmasi **tanpa isian**. `POST /start-triage` berisi hanya `encounterId` dan `mode: "ImmediateCare"`. Pengkajian IGD terbuka; kunjungan *Sedang ditangani* | 4 |
| U6 | Pasien rekam pengganti: Mulai Triage, centang *Pasien tanpa identitas*, isi nama sementara | Payload `isUnknownPatient: true` + nama sementara. Baris daftar menampilkan keterangan tanpa identitas. Tanda vital, SOAP, dan pesanan lab dapat dibuat | 2 |
| U7 | Pasien hasil pendaftaran ganda beralasan yang kunjungan lamanya masih berjalan: Mulai Triage pada encounter keduanya | `409` dengan kalimat server tampil di dialog; daftar dimuat ulang | 6 |
| U8 | Dua tab: tekan Mulai Triage untuk pasien yang sama di keduanya | Tab kedua mendapat `200` dan membuka kunjungan yang sama; tidak ada kunjungan kedua | 3 |
| U9 | Loket: pasien yang masih menunggu triage, isi alasan pendaftaran ganda, selesaikan | Encounter kedua tersimpan lewat satu `POST`; nol `POST /emergency-visits` | 1 |
| U10 | Pasien baru: Mulai Triage; isi lokasi kedatangan, lokasi pasien ditemukan, lokasi trauma, waktu trauma 30 menit lalu, dan catatan kunjungan; tekan *Mulai Triage* | Badan `POST /start-triage` membawa `arrivalLocation`, `foundLocation`, `traumaLocation`, `traumaDateTime`, `notes` → `201`; respons memuat kelima nilai yang sama; Detail triage terbuka | `IGD-DEC-179` |
| U11 | Pasien baru: Mulai Triage tanpa mengisi kelima isian baru | Badan permintaan **tidak** memuat kelima kunci itu → `201` | `IGD-DEC-179` |
| U12 | Pasien baru: Mulai Triage dengan waktu trauma satu jam ke depan | `400` *"Waktu trauma tidak boleh melewati waktu sekarang."* tampil di dialog; dialog tetap terbuka dan isian lain tidak hilang | `IGD-DEC-179` |
| U13 | Ketik lebih dari 250 karakter pada salah satu isian lokasi, dan lebih dari 1000 pada catatan | Isian berhenti menerima ketikan pada 250 dan 1000 karakter | `IGD-DEC-179` |
| U14 | Tangani Segera pada baris tanpa kunjungan | Tetap konfirmasi **tanpa isian**; badan permintaan hanya `encounterId` dan `mode` | 4 |

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pendaftaran pasien → tepat satu permintaan pembuatan; nol `POST /emergency-visits`; nol baris antrean | **Terpenuhi pada source** | Hook tidak lagi mengimpor `submitEmergencyVisit`; pembangun payload kunjungan dihapus; unit test `FE-IGD-036 K1`. Antrean: `BE-IGD-053`. Uji layar U1, U9 belum |
| 2 | Pasien tanpa identitas lewat rekam pengganti; Mulai Triage mencatat penanda dan nama sementara; tanda vital, SOAP, lab dapat dibuat | **Terpenuhi pada source** (bagian layar ini) | Dialog + unit test `FE-IGD-036 K2`. Uji layar U6 belum |
| 3 | Mulai Triage → kunjungan lahir dan formulir triage terbuka; klik kedua membuka kunjungan yang sama | **Terpenuhi pada source** | `handleSubmitStartTriage`; unit test `FE-IGD-036 K3`. Uji layar U3, U8 belum |
| 4 | Tangani Segera pada baris tanpa kunjungan → tanpa isian; Pengkajian IGD terbuka | **Terpenuhi pada source** | `handleConfirmImmediateCare`; unit test `FE-IGD-036 K4`. Uji layar U5 belum |
| 5 | Waktu tiba wajib, terisi awal waktu terdaftar; nol jalur dari jam browser; loket tanpa isian waktu tiba | **Terpenuhi pada source** | `buildTriageStartFormValues`; efek dan fungsi lama dihapus; unit test `FE-IGD-036 K5` (empat kasus). Uji layar U1, U3, U4 belum |
| 6 | Pesan `409` tampil apa adanya | **Terpenuhi pada source** | Thunk meneruskan pesan server; dialog dan spanduk menampilkannya. Uji layar U7 belum |
| 7 | `eslint` 0 error; unit test lulus, test usang diperbarui; nol CSS baru; build dan uji layar — milik pemilik | **Sebagian** | `eslint` 0 error; 70/70; nol CSS; build dan uji layar belum |

DoD: laporan tracked ✅ (berkas ini). **Urutan rilis yang dipakai:** penjaga backend `BE-IGD-053` ✅ dan `FE-IGD-038` ✅
sudah aktif; `BE-IGD-059` **harus aktif lebih dulu** sebelum task ini dirilis. Dirilis bersama `FE-IGD-035` dan
`FE-IGD-039` supaya baris tanpa kunjungan tidak pernah tampil dengan tombol yang kurang.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sesudah task ini, loket **tidak** membuat kunjungan. Pasien baru terlihat di Pengkajian IGD hanya sesudah perawat menekan Mulai Triage atau Tangani Segera |
| Masalah yang diketahui | (1) Empat ruas kunjungan tidak lagi dapat diisi (3.4, baris pertama) — **ditutup lewat `IGD-DEC-179`, bagian 9**. (2) Thunk `submitEmergencyVisit`, service `createEmergencyVisit`, dan state `completedEmergencyVisit` tertinggal tanpa pemanggil — slice dan service pendaftaran di luar daftar berkas kartu. (3) Komentar lama di atas `handleSubmitRegistration` masih menyebut langkah membuat kunjungan; tidak disunting. (4) Mulai Triage untuk encounter kedua hasil pendaftaran ganda ditolak selama kunjungan lama berjalan (`IGD-OQ-108`). (5) Enam unit test modul lain gagal — bukan akibat task ini |
| Dependency backend | `BE-IGD-059` 🟡 — belum di-build. Tanpa itu encounter tanpa kunjungan masih dapat ditutup lewat jalur umum Registrasi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 16 berkas berubah dan 1 berkas baru oleh task ini, bercampur dengan perubahan `FE-IGD-038` pada lima berkas yang sama; `src/utils/menu-sidebar/menu-items.jsx` berubah sebelum task (milik pemilik). Tanpa stage, commit, atau push |
| Langkah berikutnya | `FE-IGD-039` (aksi *Pergi sebelum ditriage*), lalu `FE-IGD-040` |

---

## 9. Pengerjaan ulang 1 Oktober 2026 — lima isian kunjungan pada dialog Mulai Triage (`IGD-DEC-179`)

| Field | Nilai |
| --- | --- |
| Jenis | Pengerjaan ulang `FE-IGD-036` — menutup selisih bagian 3.4 baris pertama, bukan kartu baru |
| Wewenang | Rizki Gunawan, 1 Oktober 2026: *"pulihkan lima ruas opsional di start-triage dan lima isian di dialog Mulai Triage … saya perintahkan untuk di pulihkan"* |
| Contract version | API §8.3.2 (lima ruas baru), validation §10.2 aturan 14–15 — `IGD-DEC-179`, manifest 0j.3 |
| Wewenang UI | `DEV_DISCRETION` (03 §13.6) untuk bentuk dan urutan isian; **isi** isiannya ditetapkan `IGD-DEC-179` |
| Dependency | `BE-IGD-055` 🟡 — delta backend selesai pada source hari yang sama, belum di-build dan diuji pemilik |
| Klasifikasi | `LIGHT` — 3 berkas, nol endpoint baru, nol state baru, nol CSS |
| Commit frontend saat dikerjakan | `aa0b1168b` (`RizkiV2`) + working tree R3.13/R3.14 yang belum di-commit |

### 9.1 Yang berubah bagi perawat

Dialog Mulai Triage kini memuat lima isian tambahan di antara *Keluhan utama* dan *Pasien tanpa identitas*.
Kelimanya **tidak wajib**; dialog tetap dapat disimpan hanya dengan waktu tiba.

| Isian | Bentuk | Batas |
| --- | --- | --- |
| Lokasi kedatangan | Teks satu baris | 250 karakter |
| Lokasi pasien ditemukan | Teks satu baris | 250 karakter |
| Lokasi trauma | Teks satu baris | 250 karakter |
| Waktu trauma | Tanggal dan jam | Tidak boleh melewati waktu sekarang — diputuskan server |
| Catatan kunjungan | Teks dua baris | 1000 karakter |

*Contoh.* Korban kecelakaan diantar warga. Perawat menekan Mulai Triage, mengisi lokasi trauma "Jl. Sudirman km 3"
dan waktu trauma 08.50, lalu menekan *Mulai Triage*. Kunjungan lahir dengan kedua nilai itu tersimpan.

Tangani Segera **tidak** berubah: tetap konfirmasi tanpa isian.

### 9.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/…/emergency-management-triage-constant.jsx` | + `TRIAGE_START_LOCATION_MAX_LENGTH` (250), `TRIAGE_START_NOTES_MAX_LENGTH` (1000); lima kunci baru pada `DEFAULT_TRIAGE_START_FORM_VALUES` — +9 baris |
| `src/utils/…/emergency-management-triage-utils.jsx` | `buildStartTriagePayload` menyertakan kelima ruas **hanya bila terisi** (pola `chiefComplaint`); waktu trauma diubah ke ISO lewat `fromTriageArrivalInputValue` yang sudah ada — +10 baris |
| `src/components/view/…/emergency-triage-start-dialog.jsx` | Lima `Form.Group` baru — +71 baris |

Nol perubahan pada slice, hook, CSS, route, menu, dan berkas test. Nol baris komentar ditambah.

### 9.3 Keputusan komponen

| Elemen | Bukti pemakaian yang sudah ada | Status |
| --- | --- | --- |
| Isian teks satu baris (tiga lokasi) | `Form.Group` + `Form.Control type="text"` berkelas `dialogField` — isian *Nama sementara* pada dialog yang sama | `REUSE` |
| Isian tanggal-jam (waktu trauma) | `Form.Control type="datetime-local"` — isian *Waktu tiba* pada dialog yang sama | `REUSE` |
| Isian teks banyak baris (catatan) | `Form.Control as="textarea"` — isian *Keluhan utama* pada dialog yang sama | `REUSE` |
| Keterangan di bawah isian | `Form.Text muted` — keterangan *Waktu tiba* | `REUSE` |

`UI GATE: PASS` — seluruh elemen `REUSE`; nol `NEW`, nol `EXTEND`, nol kelas CSS baru, nol nilai visual literal.

### 9.4 Selisih dan keterbatasan

| Butir | Isi |
| --- | --- |
| Tanpa pemeriksaan "masa depan" di layar untuk waktu trauma | Sama dengan waktu tiba (3.4 baris ketiga): jam browser tidak dipakai sebagai pembanding. Penolakan datang dari server dan tampil di dialog |
| Nilai tersimpan belum tampil di layar | Sebelum pengerjaan ulang ini, nama keempat ruas lokasi dan waktu trauma tidak muncul di source frontend sama sekali — tidak ada layar yang menampilkannya. Pengerjaan ulang ini hanya memulihkan **pengisian**; pembuktiannya lewat panel jaringan atau respons API |
| Tidak dapat diubah sesudah kunjungan lahir | Dialog hanya muncul sekali. Sesudah itu kelima ruas hanya dapat diubah lewat `PUT /emergency-visits/{id}`, yang tidak dipakai layar mana pun |
| Dialog bertambah panjang | Sebelas isian. Dialog menggulir bersama halaman seperti modal Bootstrap bawaan; tidak ada pengelompokan atau lipatan baru |

### 9.5 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` tiga berkas | 0 error, 0 warning | `PASS` |
| Baris komentar pada tambahan | Nol | `PASS` |
| Grep anti-regresi UI pada dialog | Nol warna literal, nol `style={{`, nol `<button>` mentah baru | `PASS` |
| Format berkas | CRLF dipertahankan | `PASS` |
| `npm run build` | — | `NOT RUN` — milik pemilik |
| Uji layar U10–U14 (bagian 6.1) | — | `NOT RUN` — milik pemilik |

`AUTOMATED TEST: SKIPPED (opsional) — atas perintah pemilik 1 Oktober 2026 (tanpa unit test)`. Berkas test lama tidak
disentuh; tambahan pada pembentuk payload bersifat bersyarat sehingga kasus lama tidak berubah hasilnya — **tidak
dijalankan** untuk membuktikannya.

`MANUAL TEST: NOT FEASIBLE` — backend tidak berjalan pada sesi ini; delta `BE-IGD-055` belum di-build.

Uji manual: `REQUIRED` — U10–U14, sesudah backend di-build.

---

## Pemeriksaan bukti uji gabungan — 2 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-tahap-1.json`…`results-tahap-4.json`, skrip `test-tahap-*.mjs`, tangkapan layar `<ID>.png`) dan log backend `Logs/quilvian-backend-20261001.json`, `quilvian-backend-20261002.json`. Ringkasan agen penguji ([laporan uji gabungan](../../../testing/2026-10-01-laporan-uji-gabungan-r313-r314.md)) **tidak** dipakai sebagai bukti: uraian skenarionya pada beberapa task tidak sama dengan panduan, dan daftar `FAIL`-nya tidak cocok dengan JSON mentah.

`npm run build` pemilik terbukti dari artefak: `.next/BUILD_ID` bertanggal 1 Oktober 2026 15.22, sesudah edit source terakhir (14.59). **Uji layar dilayani `next dev`** (lencana "N" pada setiap tangkapan layar), bukan hasil build. Source di-commit pemilik sebagai `de70687a9`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `036-U1` | **Terbukti** | Layar Selesai memuat nomor encounter dan *Menunggu Triage*; satu `POST /patient-encounters`, nol `POST /emergency-visits` |
| `036-U2` | **Terbukti** | Tangkapan layar memperlihatkan baris *Menunggu Triage — Terdaftar 08.21* dengan Mulai Triage, Tangani Segera, Pergi. Skrip menulis `FAIL` karena pemilih elemennya meleset |
| `036-U3` | **Terbukti** | `201`, `arrivalDateTime` dimundurkan 15 menit; formulir triage terbuka dengan baris hijau *Tiba 08.06 · dikonfirmasi SuperAdmin* |
| `036-U4` | **Terbukti** | `400` *"Waktu tiba tidak boleh melewati waktu sekarang."* tampil di dialog |
| `036-U5` | **Terbukti** | Badan permintaan hanya `encounterId` dan `mode`; `201`; Pengkajian IGD terbuka |
| `036-U6` | **Terbukti** | Badan permintaan `isUnknownPatient: true` + nama sementara; `201` pada log backend; baris daftar berbunyi *Tanpa identitas · nama sementara Mr. X Korban Kecelakaan*. Tanda vital, SOAP, dan lab tidak diuji |
| `036-U7` | **Belum terbukti** | Log backend mencatat `start-triage` `409`, tetapi tangkapan layar diambil saat tombol masih *Memulai…*; pesan di dialog tidak terekam. Baris pasien tetap tanpa kunjungan |
| `036-U8` | **Belum terbukti** | Skrip mengirim dua `POST start-triage` lewat API (`201` lalu `200`, kunjungan sama). Dua tab pada layar tidak dijalankan |
| `036-U9` | **Terbukti** | Layar Selesai untuk encounter kedua; satu `POST`, nol `POST /emergency-visits` |
| `036-U10` | **Terbukti** | Badan permintaan memuat `arrivalLocation`, `foundLocation`, `traumaLocation`, `traumaDateTime`, `notes`; `201` |
| `036-U11` | **Terbukti** | Badan permintaan tidak memuat kelima kunci; `201` |
| `036-U12` | **Terbukti** | `400` *"Waktu trauma tidak boleh melewati waktu sekarang."* tampil di dialog; dialog tetap terbuka, isian *Lokasi Trauma Sah* utuh |
| `036-U13` | **Terbukti** | Isian berhenti pada 250 dan 1000 karakter |
| `036-U14` | **Terbukti** | Konfirmasi Tangani Segera tanpa isian apa pun |

**Catatan.**

- Laporan agen menulis U2, U6, U7 `FAIL`. Dari bukti mentah: U2 dan U6 terbukti; U7 tidak terbukti karena waktu pengambilan gambar, bukan karena dialog menutup sendiri — kode menyimpan pesan server di dialog (`setStartTriageError`).
- Kosmetik, terlihat pada `036-U5.png`: saat menutup, modal Tangani Segera sesaat menampilkan varian beralasan dan nama *"Pasien belum teridentifikasi"*.

Putusan: **🟡 sebagian** — Build pemilik terbukti. Uji layar: **12 dari 14 terbukti** pada bukti mentah (U1–U6, U9–U14), termasuk lima isian `IGD-DEC-179`. **Belum terbukti:** U7 (tangkapan layar diambil saat permintaan masih berjalan; `409` tercatat di log backend, tampilannya di dialog tidak terekam) dan U8 (dijalankan lewat dua panggilan API, bukan dua tab).
