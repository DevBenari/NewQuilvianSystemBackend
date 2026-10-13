# Laporan Perubahan Frontend — `FE-RWI-244`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-244` |
| Judul | Langkah 1 Pasien Lama dan Informasi Pasien memeriksa kelayakan admisi; Pilih Bed memeriksa ulang sebelum kunjungan dibuat |
| Slice | Perbaikan `PLAN-REPAIR-EPS-010` — `FIX-EPS-010-02` |
| Roadmap | [`frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) bagian 3 dan 4.9 |
| Trace | `ISS-EPS-010-01`, `-02`, `-03`, `-T1`, `-T2`, `-T3`; `RWI-DEC-297`, `RWI-AC-432`–`435`; `05-skema-tampilan.md` 3.5.1 beserta amandemen `PLAN-REPAIR-EPS-010` |
| Contract version | API `0.11.0` `approved` + amandemen `PLAN-REPAIR-EPS-010` (`GET /episodes/admission-eligibility`, `BE-RWI-230`) |
| Wewenang UI | Skema sebelum–sesudah `PLAN-REPAIR-EPS-010` bagian 3, disetujui pemilik 13-10-2026 |
| Dependency | `BE-RWI-230` ✅ (source). **Runtime:** endpoint baru baru tersedia setelah backend di-build dan dijalankan ulang — lihat bagian 8 |
| Klasifikasi | `MEDIUM` — satu alur, 1 hook + 1 util + 1 komponen view baru, 5 berkas diubah, tanpa CSS |
| Task mode | `FRONTEND` (laporan di repository backend sesuai `AGENTS.md`) |
| Target tulis | `QuilvianSystemFrontendDev`: berkas pada bagian 3.2 |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `1b0ad5dd4b347fa3493554a3b6484514cf2c2a43` (`HamzahV2`). Working tree sudah berisi 3 perubahan pihak lain sebelum task (`inpatient-consent-form.jsx`, `inpatient-admission-workspace-constants.js`, `inpatient-admission-workspace-utils.js`) — tidak disentuh |
| Commit backend yang dijadikan rujukan | `8fce8c966803d21d763faa31de4ff3c049d557cf` (`MHamzah`) + perubahan `BE-RWI-230` di working tree |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ Selesai — source, ESLint, test unit, dan build lulus. Uji layar `NOT FEASIBLE` (bagian 6) |

---

## 1. Keadaan yang ditemukan di awal

Langkah 1 Pasien Lama (`InpatientAdmissionExistingPatientSplitStep`) hanya mematikan tombol
**Lanjut ke Jenis Kunjungan & Kategori** bila belum ada pasien terpilih; menekannya langsung menulis
URL langkah 2. Badge kartu identitas selalu "Siap Melanjutkan". Akibatnya (`ISSUE-EPS-010`):

1. Pasien yang sedang dirawat (contoh pada laporan: "BED 001 — Kamar 02", Sedang Dirawat) lolos lima
   langkah. Di Pilih Bed, `POST /patient-encounters/admin` dan `POST /episodes` berhasil, lalu
   pemesanan bed ditolak 409 `PATIENT_ALREADY_ADMITTED` — kunjungan dan episode `Draft` tertinggal.
2. Pasien dengan kunjungan rawat jalan aktif baru ditolak di Pilih Bed oleh pendaftaran
   ("Pasien masih memiliki kunjungan aktif bernomor ENC-RSMMC-00xxx …").
3. Permintaan admisi Kamar Pulih yang menunggu juga baru ditolak di Pilih Bed.

---

## 2. Proses bisnis dari sisi pengguna

| Butir | Isi |
| --- | --- |
| Pelaku | Petugas admisi rawat inap |
| Pemicu | Memilih kartu pasien hasil pencarian No. RM/NIK; menekan Lanjut; menekan Lanjut ke Pemesanan di Pilih Bed |
| Langkah normal | Pilih pasien → badge "Memeriksa kelayakan..." → "Siap Melanjutkan" → Lanjut ("Memeriksa kelayakan..." sesaat) → langkah 2 |
| Pasien sedang dirawat | Badge "Sedang Dirawat"; alert merah "Pasien tidak dapat diadmisi" berisi kalimat server; tombol **Pindah Tempat Tidur** (Detail Episode, bagian Perpindahan Tempat Tidur) dan **Lihat Detail Episode**; Lanjut tetap di langkah 1, alert digulir ke pandangan dan difokus |
| Kunjungan aktif | Badge "Kunjungan Aktif"; alert berisi kalimat pendaftaran + petunjuk jalur Transfer IGD/Poli; Lanjut tetap di langkah 1 |
| Permintaan Kamar Pulih | Badge "Permintaan Kamar Pulih"; tombol **Buka Jalur Kamar Pulih** (hanya di jalur Pasien Lama) |
| Admisi lain masih Draft | Badge "Ada Admisi Disiapkan"; alert kuning dengan tombol **Lanjutkan Admisi {nomor}**; Lanjut tetap maju (keputusan "Draft ganda bukan penolakan") |
| Gagal diperiksa | Badge "Belum Terperiksa"; alert merah berisi alasan + **Periksa Ulang**; Lanjut tetap di langkah 1 |
| Pilih Bed | Sebelum kunjungan dibuat, kelayakan diperiksa ulang; ditolak/gagal → kalimatnya tampil pada alert langkah Pilih Bed, tanpa permintaan tulis |

**Contoh.** Petugas mencari RM Tn. Umar (pseudonim) yang dirawat di Kamar 02 BED 001. Kartu kiri
menampilkan badge "Sedang Dirawat"; di bawah kedua panel muncul "Tn. Umar sudah dirawat pada episode
RI-… di Kamar 02 BED 001. Bila memang pindah kamar, pakai perpindahan, bukan admisi baru." Petugas
menekan **Pindah Tempat Tidur** dan langsung tiba di bagian perpindahan pada Detail Episode.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`inpatient-admission-existing-patient-step.jsx`, `inpatient-admission-view.jsx`,
`inpatient-admission-registration-step.jsx` (pola alert bertombol), `inpatient-episode-worklist-view.jsx`
(tautan Lanjutkan Admisi), `inpatient-episode-detail-view.jsx` (bagian perpindahan),
`inpatient-monitoring-table-columns.jsx` (pemakaian `buildInpatientEpisodeTransferRoute`),
`use-inpatient-admission-patient.jsx`, `use-inpatient-admission-doctor.jsx`,
`use-inpatient-admission-bed.jsx`, `use-inpatient-admission-confirmation.jsx` (pola hook baca),
`use-inpatient-admission-flow.jsx` (`selectEntryMode`), `inpatient-api.service.js`,
`inpatient-episode.service.js`, `inpatient-admission-flow-constants.jsx`, `inpatient-episode-constants.jsx`,
`inpatient-setting-utils.jsx`, `base-patient-verification-card.jsx`, `information-alert.jsx`,
`base-button.jsx`, `InstanceAxios.jsx` (galat diteruskan mentah).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/inpatient-management/inpatient-episode.service.js` | `getInpatientAdmissionEligibility({ patientId, admissionReferralId, signal })` |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | `INPATIENT_ADMISSION_ELIGIBILITY_CODE`, `…_STATUS`, `…_COPY` (judul, badge, petunjuk, label tombol, kalimat galat) |
| `src/utils/health-services/inpatient-management/inpatient-admission-eligibility-utils.js` (baru) | `normalizeInpatientAdmissionEligibility` (tanpa `canContinue` → `null`, tidak pernah dianggap boleh), `resolveInpatientAdmissionEligibilityBadge`, `buildInpatientAdmissionEligibilityNotice`, `getInpatientAdmissionEligibilityErrorMessage` (tidak pernah meneruskan teks Axios Inggris), `getInpatientAdmissionEligibilityBlockMessage` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-eligibility.jsx` (baru) | Pemeriksaan otomatis saat pasien berganti (permintaan lama dibatalkan, hanya permintaan terakhir menulis keadaan), `proceed(onAllowed)` untuk tombol Lanjut, `retry`, callback ref `registerNotice` + gulir/fokus sesudah penolakan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-eligibility-notice.jsx` (baru) | Rangkaian `InformationAlert` + `BaseButton` (tautan `as={Link}` atau aksi) |
| `src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx` | Split step dan Review step: badge dari hasil pemeriksaan, alert kelayakan, Lanjut lewat `proceed` dengan `loading` "Memeriksa kelayakan..." |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Memasang hook kelayakan (hanya pada langkah `existing-patient` / `existing-patient-information`, bukan alur pelanjutan), meneruskan ke langkah, "Buka Jalur Kamar Pulih" = `flow.selectEntryMode(RECOVERY)` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | `submit`: pemeriksaan ulang kelayakan sebelum `POST /patient-encounters/admin` (hanya bila kunjungan belum ada); ditolak/gagal → `fail(kalimat)` tanpa permintaan tulis |
| `tests/unit/inpatient-admission-eligibility.test.mjs` (baru) | 8 test util |

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Bukti |
| --- | --- |
| View tidak memanggil Axios | Permintaan hanya di service; hook memanggil service; komponen menerima props |
| Normalisasi di utils, tanpa React | `inpatient-admission-eligibility-utils.js` murni, diuji 8 test |
| Konstanta dan salinan teks di `lib/constants` | `INPATIENT_ADMISSION_ELIGIBILITY_*` dibekukan `Object.freeze` |
| Request dapat dibatalkan | `AbortController` per permintaan, dibatalkan saat pasien berganti dan saat unmount |
| Tanpa CSS baru | Tata letak alert memakai kelas `duplicateIdentityAlert*` yang sudah ada di `inpatient-admission.module.css` |
| Tanpa UUID tampil | `episodeId` hanya dipakai di `href`; yang tampil nomor episode, lokasi, nomor kunjungan |

---

## 4. State yang ditangani di layar

| Keadaan | Badge | Alert | Tombol Lanjut |
| --- | --- | --- | --- |
| Belum ada pasien | — (kartu kosong) | — | mati (perilaku lama) |
| Memeriksa | "Memeriksa kelayakan..." | hasil sebelumnya untuk pasien yang sama, bila ada | aktif; saat ditekan loading |
| Boleh | "Siap Melanjutkan" | — | maju |
| Ditolak | sesuai sebab | danger + tindak lanjut | tidak maju, alert digulir & difokus |
| Draft lain | "Ada Admisi Disiapkan" | warning + Lanjutkan Admisi | maju |
| Gagal (jaringan, 403, 404, 5xx, jawaban tak dikenali) | "Belum Terperiksa" | danger + Periksa Ulang | tidak maju |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/admission-eligibility?patientId=&admissionReferralId=` | Kelayakan admisi (`BE-RWI-230`) | `InpatientEpisode : Read` |

Endpoint lama yang ikut terlindungi: `POST /patient-encounters/admin` dan `POST /episodes` (tidak
dipanggil bila pemeriksaan ulang menolak).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-eligibility.test.mjs` | 8/8 PASS | `PASS` | Keluaran perintah |
| Suite unit penuh `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | 2832 test, 2802 PASS, 30 FAIL (garis dasar pagi 13-10: 2776/2742/34) | `PASS` (tanpa regresi) | Satu-satunya kegagalan yang membaca berkas sentuhan task ini adalah `inpatient-admission-deposit` "FE-RWI-058: tombol lanjut tidak pernah terkunci" — kegagalan lama `minimumAmount` yang sudah tercatat sebelum task; 22 berkas gagal lain tidak membaca berkas task ini |
| `npx eslint <9 berkas yang disentuh>` | 0 error, 1 warning lama (`inpatient-admission-view.jsx:135`, `MotherEpisodeSelector`, tidak disentuh) | `PASS` | Keluaran perintah |
| `npx eslint src --quiet` (pengganti `npm run lint:errors` yang rusak oleh `test-with-agy/`) | exit 0, 0 error, 3 m 48 d | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Tidak dijalankan | `EXISTING / ENVIRONMENT ISSUE` | Perintah baku berhenti di plugin `react-hooks` pada `test-with-agy/` (catatan 23-09-2026) |
| `npm run build` (Next 16.2.12; dev server berjalan di `.next/dev`, terpisah) | Compiled successfully 54 s; 492/492 halaman; exit 0; 1 m 35 d | `PASS` | Keluaran perintah; build kedua sesudah perbaikan kalimat galat |
| Grep anti-regresi (tombol mentah, `<table>`, utility Bootstrap typography, CSS baru) | Kosong | `PASS` | Bagian G checklist konsistensi UI |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-eligibility.test.mjs — PASS`

`MANUAL TEST: NOT FEASIBLE` — backend lokal berjalan `dotnet run --no-build` dari DLL sebelum
`BE-RWI-230`, sehingga endpoint kelayakan belum ada di runtime; uji layar menunggu pemilik build dan
menjalankan ulang backend (bagian 8). Skenario uji: `PLAN-REPAIR-EPS-010` bagian 8.

---

## 7. Keputusan base component

`UI GATE: 4 elemen — REUSE 2, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Badge kelayakan di kartu identitas | `BasePatientVerificationCard` prop `badgeText` | `base-patient-verification-card.jsx:165`, `:276-278` | REUSE | Teks dari `resolveInpatientAdmissionEligibilityBadge` |
| Tombol Lanjut dengan keadaan memeriksa | `BaseButton` `loading`, `loadingLabel` | `base-button.jsx:25-27` | REUSE | "Memeriksa kelayakan..." |
| Alert kelayakan bertombol | `InformationAlert` (`children`) + `BaseButton` | `information-alert.jsx`; pola alert NIK terdaftar `inpatient-admission-registration-step.jsx:257-292` | COMPOSE | Rangkai di view, kelas tata letak yang sama |
| Tombol tautan tindak lanjut | `BaseButton as={Link}` | dipakai `inpatient-episode-worklist-view.jsx:241-266` | COMPOSE | Tanpa komponen baru |

Pilihan yang disajikan untuk baris bukan `REUSE`:

> **Keputusan: alert kelayakan bertombol**
>
> - **A. Rangkai `InformationAlert` + `BaseButton` dengan kelas `duplicateIdentityAlert*` yang sudah ada — Rekomendasi (dikerjakan).** Tampil sama dengan alert NIK terdaftar pada alur yang sama; tanpa CSS baru; risiko regresi nol karena tidak ada base component yang diubah.
> - **B. Tambah prop `actions` pada `InformationAlert`.** Lebih ringkas di pemakai, tetapi mengubah base component yang dipakai ratusan layar; butuh persetujuan.
>
> **Keputusan: warna badge "Sedang Dirawat"**
>
> - **A. Pakai `badgeText` apa adanya — Rekomendasi (dikerjakan).** Badge tetap gaya teal netral; warna merah dibawa alert di bawahnya. Tanpa perubahan base component yang juga dipakai radiologi.
> - **B. Extend `BasePatientVerificationCard` dengan prop opsional `badgeVariant`.** Badge merah/kuning lebih jelas, perilaku bawaan tidak berubah, tetapi menambah varian CSS pada base. Dapat dikerjakan menyusul bila pemilik meminta.

---

## 8. Acceptance criteria dan Definition of Done

| Kriteria (`FIX-EPS-010-02`) | Status | Bukti |
| --- | --- | --- |
| AC 1 — pasien `Admitted`: badge "Sedang Dirawat", alert kalimat server, Pindah Tempat Tidur + Lihat Detail Episode, Lanjut tidak maju | Terpenuhi pada source + test | `buildInpatientAdmissionEligibilityNotice` (test "pasien yang sedang dirawat…"), `proceed` hanya memanggil `onNext` bila `canContinue` |
| AC 2 — kunjungan aktif: badge "Kunjungan Aktif", kalimat server + petunjuk Transfer IGD/Poli, Lanjut tidak maju | Terpenuhi pada source + test | idem |
| AC 3 — permintaan Kamar Pulih: tombol jalur Kamar Pulih hanya di Pasien Lama; jalur Kamar Pulih dengan permintaannya sendiri boleh lanjut | Terpenuhi pada source + test | `allowRecoveryEntry`; `admissionReferralId` diteruskan view → hook → server |
| AC 4 — Draft lain: warning + Lanjutkan Admisi ke langkah yang tepat; Lanjut tetap maju | Terpenuhi pada source + test | `INPATIENT_ADMISSION_RESUME_STEPS` menurut `hasActiveReservation` |
| AC 5 — pasien bersih: "Siap Melanjutkan", maju seperti sebelumnya | Terpenuhi | test badge; `proceed` |
| AC 6 — gagal diperiksa: alasan + Periksa Ulang, Lanjut tidak maju | Terpenuhi pada source + test | `getInpatientAdmissionEligibilityErrorMessage` (test galat), `retry` |
| AC 7 — ganti pasien membatalkan pemeriksaan sebelumnya; hasil pasien lama tidak tampil | Terpenuhi pada source | Kunci `patientId|referralId`, `AbortController`, nomor urut permintaan, `current` diturunkan dari kunci |
| AC 8 — Pilih Bed tidak mengirim `POST /patient-encounters/admin`/`POST /episodes` bila ditolak | Terpenuhi pada source | Blok pemeriksaan ulang di `submit` sebelum `createInpatientPatientEncounter` |
| AC 9 — tanpa UUID tampil | Terpenuhi | `episodeId` hanya di `href` |
| DoD: ESLint, build, test unit | Terpenuhi | Bagian 6 |
| DoD: uji layar | `NOT FEASIBLE` | Dikecualikan atas keputusan pemilik 01-09/10-09-2026; menunggu backend dijalankan ulang |

---

## 9. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Urutan rilis** | Frontend ini **bergantung** pada `BE-RWI-230` di runtime. Selama backend lama masih berjalan, langkah 1 Pasien Lama menampilkan "Pemeriksaan kelayakan admisi belum tersedia di server…" dan Lanjut tidak maju (sengaja *fail-closed*). Build dan jalankan ulang backend lebih dulu, atau bersamaan |
| Hak akses | Pemeriksaan memakai `InpatientEpisode : Read`. Petugas admisi tanpa hak itu melihat "Anda belum memiliki akses membaca Episode Rawat Inap…" dan tidak dapat lanjut |
| Masalah yang diketahui | Bila pasien ditempatkan petugas lain di antara pemeriksaan ulang Pilih Bed dan `POST /episodes` (detik), kunjungan rawat inap baru tetap tertinggal; episodenya ditolak server (`BE-RWI-230`) |
| Risiko tersisa | Satu `GET` tambahan saat pasien dipilih, saat Lanjut, dan saat Lanjut ke Pemesanan |
| Perubahan sampingan | `NONE` (`.next/` hasil build tidak dilacak Git) |
| Interupsi | `NONE` |
| Status Git frontend | ` M` 5 berkas task ini; `??` 4 berkas baru task ini (setiap hunk pada berkas itu diperiksa milik task ini). Di luar itu: 3 berkas pihak lain yang sudah ada sebelum task, ditambah suntingan agen lain yang berjalan bersamaan untuk `ISSUE-EPS-011` (tombol batalkan admisi) pada `inpatient-admission-bed-step.jsx`, `-confirmation-step.jsx`, `-deposit-step.jsx`, `-doctor-step.jsx`, `-payment-step.jsx`, `-visit-category-step.jsx`, `use-inpatient-admission-deposit.jsx`, `use-inpatient-bed-board.jsx`, `use-inpatient-bed-transfer.js`, `inpatient-admission-session-utils.js` — tidak disentuh task ini. Hasil build/test di bagian 6 mencerminkan working tree pada saat dijalankan, termasuk suntingan bersamaan itu |
| Langkah berikutnya | Pemilik: build + jalankan ulang backend, lalu uji `PLAN-REPAIR-EPS-010` bagian 8 |
