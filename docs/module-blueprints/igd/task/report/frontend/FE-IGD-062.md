# Laporan Perubahan Frontend — `FE-IGD-062`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-062` |
| Judul | Layar admisi Rawat Inap memakai permintaan bersumber IGD |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.16 · `SCR-INP-ADM` — **layar milik Rawat Inap** |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-113`, `115` (sisi admisi); `AT-IGD-229`…`231` |
| Keputusan | `IGD-DEC-275`, `276`, `279`, `286`, `288`, `292`, `299` (persetujuan lisan pemilik Rawat Inap), `301` (pengisian unit ditahan) |
| Contract version | API **`0.17.1`** §12.4; validation **`0.16.0`** §14 aturan 20, 21; `03-frontend-architecture.md` §17.4 D |
| Pasangan backend | `BE-IGD-073` (migration 2 diterapkan, build 14 Oktober 2026 0 error) — dirilis **bersama** |
| Wewenang UI | Pemilik layar Rawat Inap; rupa tampilan `DEV_DISCRETION` dalam konvensi layar admisi |
| Task mode | `FRONTEND` — izin pemilik 14 Oktober 2026: *"lanjutkan FE-IGD-062"* |
| Repository / branch | `QuilvianSystemFrontendDev` / `RizkiV2`, HEAD `2ab4d6e46` + working tree |
| Model | Claude Opus 5.5 |
| Tanggal | 14 Oktober 2026 |
| Status | ⛔ **DIBATALKAN — 14 Oktober 2026, `IGD-DEC-304`.** 4 berkas Rawat Inap dikembalikan ke `HEAD`, 3 berkas baru dihapus; pasien IGD kini diproses dari Antrean Transfer IGD/Poli yang sudah ada (`FE-RWI-228`/`229`), tanpa perubahan layar Rawat Inap |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti |
| --- | --- |
| Admisi dari permintaan berjalan lewat mode **Kamar Pulih**: langkah `recovery-referral` merender `InpatientAdmissionRecoveryReferralStep`, yang memuat `GET /admission-referrals?status=Pending` — sesudah `BE-IGD-073` daftar itu otomatis ikut memuat permintaan IGD, tetapi kartunya menampilkan *Tindakan: -*, *Operator: dr. {DPJP usulan}*, *Asal: Emergency* seolah pasien pasca operasi | `inpatient-admission-recovery-referral-step.jsx`; `inpatient-admission-view.jsx` `:344` |
| `RecoveryAdmissionReferralsList` diimpor tetapi tidak dirender di mana pun | `grep '<RecoveryAdmissionReferralsList'` nihil — tidak disentuh |
| Jalur kamar pulih tidak mengisi awal kelas maupun DPJP; admisi hanya membawa `admissionReferralId` | `use-inpatient-admission-doctor.jsx` `:63`, `:560` |
| Hook dokter mengekspos `handleChange`; hook pembayaran mengekspos `setPatientClassId(id, option)` yang menyimpan label kelas | `use-inpatient-admission-doctor.jsx` `:364`; `use-inpatient-admission-payment.jsx` `:338` |
| Hook pembayaran memulihkan pilihan simpanan ketika pasien berganti | `use-inpatient-admission-payment.jsx` `:186` — pengisian awal harus terjadi sesudah pasien permintaan aktif |
| Enum Rawat Inap dikirim sebagai angka; layar membandingkan `requestedCareLevel` dengan teks `"icu"` | Temuan lama — bagian 8 |

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Yang terlihat |
| ---: | --- | --- |
| 1 | Sari (petugas admisi) membuka Admisi Rawat Inap | Pilihan *Admisi dari Kamar Pulih* berlencana `PASIEN PASCA OPERASI [ n ] · DARI IGD [ m ]` |
| 2 | Memilih mode itu | Daftar memuat kedua sumber; CITO paling atas (urutan server); kartu IGD berlencana **DARI IGD** dan **CITO**, menampilkan *Unit: Bangsal Melati · Kelas 2* dan *DPJP usulan: dr. Sinta · Asal: IGD*; tab baru **Dari IGD** menyaring hanya permintaan IGD |
| 3 | Memilih kartu IGD | Panel kanan *Form Permintaan Rawat Inap IGD*: no. kunjungan IGD, unit & kelas, DPJP usulan, diagnosis, alergi, indikasi klinis, rencana perawatan, kebutuhan khusus, estimasi lama rawat, penanda tangan; catatan bahwa unit belum diisikan otomatis |
| 4a | **Pilih & Lanjutkan Admisi** | Alur admisi biasa; **kelas perawatan** di langkah Pembayaran dan **DPJP** di langkah Dokter sudah terisi dari permintaan dan tetap dapat diubah. Unit **tidak** diisi (`IGD-DEC-301`) |
| 4b | **Batalkan Permintaan** (butuh `InpatientAdmissionReferral : Cancel`) | Konfirmasi beralasan (wajib, maks. 500); sukses → *"Permintaan admisi dibatalkan."* dan daftar dimuat ulang; perawat IGD melihat *Dibatalkan admisi* (`FE-IGD-061`) |

**Jalur tidak normal.**

| Keadaan | Di layar |
| --- | --- |
| Rincian IGD gagal dimuat | Pesan + **Coba Lagi** di panel kanan |
| Pembatalan ditolak (sudah selesai/dibatalkan, `rowVersion` basi, sumber kamar pulih) | Pesan server tampil apa adanya di modal |
| Tanpa izin `Cancel` | Tombol *Batalkan Permintaan* tidak tampil |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`inpatient-admission-view.jsx`, `inpatient-admission-recovery-referral-step.jsx` (+ CSS module-nya, dibaca saja),
`recovery-admission-referrals-list.jsx`, `inpatient-admission-doctor-step.jsx`, `use-inpatient-admission-doctor.jsx`,
`use-inpatient-admission-payment.jsx`, `inpatient-admission-flow-constants.jsx`, `inpatient-admission-referral.service.js`,
`inpatient-physician-workspace-utils.js` (label alergi), `emergency-admission-request.utils.js` (deskripsi diagnosis, kebutuhan
khusus, waktu), backend `InpatientAdmissionReferralDtos.cs` (bentuk respons).

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-referral.service.js` | `M` +12 | `cancelAdmissionReferral(id, payload)` |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | `M` +43 | `INPATIENT_ADMISSION_REFERRAL_SOURCE`, `INPATIENT_EMERGENCY_REFERRAL_TEXT` |
| `src/utils/health-services/inpatient-management/inpatient-admission-referral-utils.js` | `??` baru, 91 baris | `isEmergencyAdmissionReferral`, `matchesAdmissionReferralSearch` (pencarian lama + DPJP usulan + unit), `normalizeEmergencyReferralDetail` |
| `src/lib/hooks/health-services/inpatient-management/use-admission-referral-emergency-detail.jsx` | `??` baru, 103 baris | Muat rincian permintaan IGD terpilih; batalkan beralasan |
| `src/lib/hooks/health-services/inpatient-management/use-emergency-referral-admission-seed.jsx` | `??` baru, 46 baris | Sekali per permintaan IGD, sesudah pasiennya aktif: kelas ke `payment.setPatientClassId`, DPJP ke `doctor.handleChange("doctorId")`. Unit tidak diisi |
| `src/components/view/health-services/inpatient-management/admission-referrals/inpatient-admission-recovery-referral-step.jsx` | `M` +177/−34 | Lencana sumber dan CITO; isi kartu IGD; tab *Dari IGD*; pencarian lewat util; panel rincian IGD; tombol dan modal *Batalkan Permintaan*; pemberitahuan sukses; kalimat pembuka langkah |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | `M` +24/−4 | Panggil hook pengisian awal; hitungan lencana mode dipisah kamar pulih / IGD |

Hook dokter dan pembayaran Rawat Inap **tidak** diubah. Komponen pendaftaran IGD (`PatientEntryChoiceStep` dan berkas
`registration-management/emergency-registration/**`) tidak disentuh. Nol CSS baru atau berubah. Nol komentar baru. Seluruh
berkas CRLF tanpa BOM.

### 3.3 Gerbang keputusan base component

`UI GATE: 7 elemen — REUSE 6, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Elemen | Status | Base component / gaya | Bukti |
| --- | --- | --- | --- |
| Lencana *DARI IGD* dan *CITO* | `REUSE` | `StatusBadge` | Pola lencana IGD |
| Isi kartu IGD | `REUSE` | Kelas kartu langkah yang ada (`procedureRow`, `surgeonMetaRow`) | Modul CSS langkah |
| Tab *Dari IGD* | `REUSE` | `FILTER_TABS` + kelas tab yang ada | Pola tab langkah |
| Panel rincian IGD | `COMPOSE` | Kelas pratinjau langkah (`previewSection`, `previewDataBox`, `previewDataRow`) + `InformationAlert` + `BaseButton` | Bagian di bawah |
| Tombol *Batalkan Permintaan* | `REUSE` | `BaseButton` | — |
| Modal batal beralasan | `REUSE` | `ConfirmModal requireReason` | Pola batal form IGD |
| Pemberitahuan sukses / catatan unit | `REUSE` | `InformationAlert` | — |

**Pilihan untuk panel rincian IGD (`COMPOSE`):**

1. **Panel pratinjau yang sama dengan kamar pulih, isinya berganti untuk sumber IGD (Direkomendasikan, dipakai).** Petugas membaca kedua sumber di tempat yang sama dengan rupa sama; tanpa CSS baru.
2. Modal rincian IGD terpisah. Menambah satu klik dan satu pola tampilan baru di langkah yang sudah punya panel pratinjau.

### 3.4 Delta terhadap kartu dan kontrak

| No | Kartu/kontrak | Implementasi | Alasan |
| ---: | --- | --- | --- |
| 1 | Acceptance 3: unit, kelas, DPJP terisi dari permintaan | Kelas dan DPJP terisi; **unit tidak** | `IGD-DEC-301` menahan pengisian unit sampai `IGD-OQ-127` dijawab; layar menampilkan catatannya |
| 2 | DPJP terisi | Bila DPJP usulan tidak berada di halaman pertama daftar langkah Dokter, pilihannya tersimpan tetapi namanya baru tampil sesudah dicari | Perilaku yang sama dengan pengisian awal jalur transfer yang sudah ada |
| 3 | Lencana jumlah pada pilihan mode | `PASIEN PASCA OPERASI [ n ]` kini hanya menghitung kamar pulih (sebelumnya menghitung semua permintaan `Pending`); jumlah IGD ditambahkan sebagai `· DARI IGD [ m ]`; peringatan *melewati batas* hanya untuk kamar pulih | Sesudah `BE-IGD-073` hitungan lama akan menyebut pasien IGD sebagai pasien pasca operasi |
| 4 | — | Kalimat pembuka langkah menyebut permintaan IGD | Daftar memang memuat kedua sumber |

## 4. State yang ditangani di layar

| State | Penanganan |
| --- | --- |
| Loading rincian IGD | *"Memuat isi Form Permintaan Rawat Inap..."* |
| Error rincian | Pesan + **Coba Lagi** |
| Unauthorized | Tanpa `Cancel` tombol tidak tampil; perilaku `Read`/`Create` langkah tidak berubah |
| Stale / ditolak | Pesan server di modal batal |
| Disabled | Tombol batal nonaktif selama permintaan berjalan |

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Referral

Base URL: `/api/v1/health-services/inpatient-management/admission-referrals`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/?status=Pending` | Daftar kedua sumber (sudah dipakai) | `InpatientAdmissionReferral : Read` |
| `GET` | `/{id}` | Blok `emergencyRequest` untuk kartu IGD terpilih | `InpatientAdmissionReferral : Read` |
| `POST` | `/{id}/cancel` | Batalkan permintaan IGD beralasan; body `{ reason, rowVersion }` | `InpatientAdmissionReferral : Cancel` |

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` atas 7 berkas task | 0 error, 1 warning `react-hooks/set-state-in-effect` di `inpatient-admission-view.jsx` `:137` (`fetchMothers`) — sudah ada di `HEAD` (baris 133), di luar perubahan | `PASS`; warning `EXISTING WARNING` |
| Komentar baru | Nol pada baris tambahan dan 3 berkas baru | `PASS` |
| Akhiran baris | 7 berkas CRLF, tanpa BOM, nol LF tunggal | `PASS` |
| Grep anti-regresi UI pada baris tambahan | Nol warna literal, `style={}`, kelas `btn`, `<table`, `fw-*`/`fs-*`, `!important` | `PASS` |
| Komponen pendaftaran IGD | `patient-entry-choice-step.jsx` tidak berubah; 5 berkas `registration-management` yang `M` milik sesi lain, sudah berubah sebelum task | `PASS` |
| `npm run build` | Lulus 14 Oktober 2026 13.01.48–13.03.33: *Compiled successfully in 59s*, 492/492 halaman statis, 0 baris peringatan, `postbuild` standalone siap; `BUILD_ID` `CdZrb6rn2PX6y3xbZLSNX`; `data-testid` baru task ini ditemukan di `.next/static/chunks`. Server standalone pemilik (PID 27640) sudah berhenti saat izin diberikan; tidak dinyalakan ulang | `PASS` |

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tim: unit test tidak ditulis atau dijalankan untuk task ini.`

`MANUAL TEST: NOT FEASIBLE — agent tidak memegang kredensial akun petugas admisi; diuji pada putaran B.`

**Skenario uji layar untuk putaran B.**

| ID | Langkah | Hasil yang diharapkan | AT |
| --- | --- | --- | --- |
| 062-U1 | Sesudah form IGD ditandatangani, buka Admisi Rawat Inap → *Admisi dari Kamar Pulih* | Lencana mode `· DARI IGD [ 1 ]`; kartu IGD berlencana *DARI IGD* (+ *CITO* bila dicentang) di atas permintaan kamar pulih yang lebih lama | `AT-IGD-229` |
| 062-U2 | Pilih kartu IGD | Panel rincian memuat diagnosis, alergi, indikasi, rencana, kebutuhan khusus, lama rawat, penanda tangan | — |
| 062-U3 | **Pilih & Lanjutkan Admisi** sampai langkah Dokter | Kelas di langkah Pembayaran dan DPJP di langkah Dokter terisi; unit tidak; keduanya dapat diganti; admisi selesai → form IGD *Selesai* | `AT-IGD-229` |
| 062-U4 | Admisi biasa (*Pasien Lama*) untuk pasien yang punya permintaan IGD `Pending` | Penolakan `409` *"Pasien punya permintaan admisi yang menunggu. Buka admisi dari permintaan itu."* | `AT-IGD-230` |
| 062-U5 | **Batalkan Permintaan** tanpa alasan, lalu dengan alasan *"Kelas 2 penuh"* | Tanpa alasan tombol nonaktif; dengan alasan → kartu hilang, perawat IGD melihat *Dibatalkan admisi* dan *Jalankan* tertahan | `AT-IGD-231` |
| 062-U6 | Regresi kamar pulih: pilih kartu pasien pasca operasi dan lanjutkan admisi | Kartu, panel, dan alur sama seperti sebelumnya; tidak ada pengisian awal kelas/DPJP; tidak ada tombol batal | — |

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Daftar menampilkan permintaan IGD dan kamar pulih, CITO paling atas, dengan lencana sumber | Kode terpenuhi; uji layar belum | Urutan server (`BE-IGD-073`) + lencana |
| 2 | Rincian permintaan IGD menampilkan diagnosis, alergi, indikasi, rencana, kebutuhan khusus, lama rawat, penanda tangan | Kode terpenuhi; uji layar belum | `normalizeEmergencyReferralDetail` + panel |
| 3 | Admisi dari permintaan IGD terisi unit, kelas, DPJP dan tetap dapat diubah | **Sebagian (delta 1)** — kelas dan DPJP; unit ditahan `IGD-DEC-301` | `useEmergencyReferralAdmissionSeed` |
| 4 | *Batalkan permintaan* beralasan; penolakan tampil apa adanya | Kode terpenuhi; uji layar belum | `useAdmissionReferralEmergencyDetail.cancel` + `ConfirmModal` |
| 5 | Regresi mode *Kamar Pulih* tidak berubah | Kode: cabang kamar pulih memakai markup lama tanpa diindentasi ulang isinya selain isi kartu; pengisian awal hanya untuk sumber IGD; uji layar belum | Diff |
| 6 | Diff hanya berkas Cakupan; komponen pendaftaran IGD tidak berubah; nol komentar baru; `eslint` 0 error; `npm run build` lulus | Terpenuhi — build 14 Oktober 2026 lulus | Bagian 6 |

DoD: persetujuan Rawat Inap tercatat (`IGD-DEC-299`); laporan ini ✅; acceptance 3 sebagian, 6 terpenuhi (build 14 Oktober 2026); uji layar pada putaran B; rilis
bersama `BE-IGD-073`.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan lama (bukan task ini) | Langkah kamar pulih membandingkan `requestedCareLevel` dengan teks `"icu"`, padahal backend mengirim angka (`InpRequestedCareLevel` tanpa konverter teks). Akibatnya lencana *BUTUH ICU* dan tab *Perlu ICU* tidak pernah menyala untuk kamar pulih maupun IGD. Untuk permintaan IGD, kebutuhan ICU tetap terbaca di panel (*Kebutuhan khusus: Perawatan intensif*). Diserahkan ke pemilik Rawat Inap |
| Menunggu | Konfirmasi Muhammad Hamzah atas pembatasan "batal oleh petugas admisi hanya untuk sumber IGD" (`BE-IGD-073` delta 1); jawaban analis `IGD-OQ-127` untuk membuka pengisian unit |
| Risiko | Sedang — layar milik Rawat Inap; regresi kamar pulih wajib diuji (062-U6) |
| Perubahan sampingan | `NONE` |
| Status Git | 4 `M` + 3 `??` milik task ini; berkas `M`/`??` lain milik sesi lain dan tidak disentuh. Nol stage, nol commit |
| Langkah berikutnya | Tidak ada — task dibatalkan |

## Pembatalan — 14 Oktober 2026 (`IGD-DEC-304`)

Owner menetapkan tugas IGD berakhir saat pasien masuk Antrean Transfer IGD/Poli di *Admisi Rawat Inap* milik Muhammad Hamzah. Jalur permintaan admisi bersumber IGD yang dibangun task ini tidak dipakai. Empat berkas (`inpatient-admission-recovery-referral-step.jsx`, `inpatient-admission-view.jsx`, `inpatient-admission-flow-constants.jsx`, `inpatient-admission-referral.service.js`) dikembalikan dengan `git restore --source=HEAD`; tiga berkas baru (`use-admission-referral-emergency-detail.jsx`, `use-emergency-referral-admission-seed.jsx`, `inpatient-admission-referral-utils.js`) dihapus. ESLint dua berkas yang dipulihkan: 0 error, 1 warning lama `fetchMothers` yang ada di `HEAD`.

Validasi pembongkaran: `git status` menunjukkan berkas task ini kembali sama dengan `HEAD`. Dokumen kontrak yang memuat jalur lama dicatat perlu amandemen pada `IGD-DEC-304`.
