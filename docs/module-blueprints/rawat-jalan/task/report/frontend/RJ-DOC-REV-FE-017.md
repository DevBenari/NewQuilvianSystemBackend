# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-017` |
| Judul | Revisi layar Pendaftaran Pasien Rawat Jalan (jalur A) |
| Slice | Amendment PM — Revisi Pendaftaran Pasien manual |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `18` |
| Trace | `RJ-DOC-DEC-062`..`066`; fakta `F-PM-1`..`3` ([00-interview-decisions.md](../../../00-interview-decisions.md), *Amendment PM*) |
| Contract version | Endpoint yang sudah ada tanpa perubahan kontrak |
| Wewenang UI | `RJ-DOC-DEC-062`..`065`: cari pasien satu kolom, pesan merah, pilihan dokter dapat dicari, Jenis Kunjungan ringkas di samping Jadwal Dokter, No. HP 13 angka. Pendaftaran IGD tidak boleh berubah |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — 12 berkas, satu komponen bersama IGD diperluas secara opt-in |
| Task mode | `FRONTEND` (`RJ-DOC-DEC-066`) |
| Target tulis | `V2QuilvianSystemFrontendDev/src/**`; laporan dan baris status di blueprint ini |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `de323430` (`sukmagpV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `77caf434` (`sukmagp`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Keadaan yang ditemukan di awal

- Cari pasien lama punya dua mode: *No. Rekam Medis* atau *Nama + Tanggal Lahir*. Kiosk memakai satu kolom yang mengenali No. KTP, No. HP, No. RM, dan nama.
- Pesan "Pilih poliklinik untuk menampilkan jadwal dokter" berwarna biru (info).
- Jadwal Dokter berupa kartu besar per dokter, tanpa pencarian. Bila satu poli punya banyak dokter, daftar menjadi panjang.
- Jenis Kunjungan berupa dua kartu besar di bawah Jadwal Dokter.
- No. HP pasien baru tanpa batas panjang.
- Endpoint pencarian `GET /patients?search=` sudah mencari ke No. RM, nama, No. KTP, No. HP, dan WhatsApp.

## 2. Proses bisnis dari sisi pengguna

1. Petugas memilih *Pasien Lama* lalu mengetik satu isian di kolom *No. RM / No. KTP / No. HP / Nama Lengkap*. Isian dikenali dengan aturan yang sama dengan Kiosk:
   - 16 digit (spasi diabaikan), contoh `3322 0307 0795 0004`: dibaca sebagai No. KTP.
   - Berawalan `08`, `62`, atau `+62`, contoh `0817-8168-3909`: dibaca sebagai No. HP dan cocok juga dengan nomor tersimpan `+6281781683909`.
   - Pola `00-00-00-15` atau 8 digit `00000015`: dibaca sebagai No. RM.
   - Isian berhuruf, contoh `ikbal yuliyanto`: dibaca sebagai nama lengkap. Kode RM non-angka seperti `KSKTEST-RM-07` juga dicocokkan.
2. Hasil harus cocok tepat dengan **satu** pasien. Bila ada dua pasien dengan No. HP sama, muncul *Ditemukan lebih dari satu pasien* dan petugas diarahkan memakai No. RM atau No. KTP. Isian yang tidak dikenali (misalnya `12`) ditolak tanpa request ke server.
3. Di *Data Kunjungan*, sebelum poliklinik dipilih, kotak Jadwal Dokter menampilkan pesan merah.
4. Setelah poliklinik dipilih, Jadwal Dokter berupa pilihan yang dapat dicari. Contoh: mengetik *Sanjaya* menyaring ke *dr. Bagus Purnama Sanjaya*. Tiap baris menampilkan nama, spesialis, jam, sesi, dan ruang. Dokter terpilih ditampilkan ulang di bawah pilihan. Bila poliklinik tidak mewajibkan dokter, tersedia opsi *Tanpa dokter*.
5. Jenis Kunjungan tampil ringkas di kanan Jadwal Dokter, Umum di atas Rujukan. Pada layar ≤ 820px kolomnya turun ke bawah.
6. Pada *Pasien Baru → Input Manual*, kolom No. Handphone berhenti di 13 angka.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`patient-selection-step.jsx`, `new-patient-form.jsx`, `emergency-registration-fields.jsx`, `filter-select.jsx`, `base-text-field.jsx`, `kiosk-patient-lookup.helpers.js`, `kiosk-old-patient-step-find.jsx`, `input-normalizer-utils.jsx`, `PatientController.ApplyStandardFilter` (backend, baca saja).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/registration-management/outpatient-registration/outpatient-registration.utils.js` | `resolveOutpatientPatientSearch` (klasifikasi memakai `classifyIdentificationInput`, `normalizeIdentityNumber`, `normalizePhoneNumber` milik Kiosk); `buildOutpatientScheduleSelectOptions`, `getScheduleDetailLabel`, `OUTPATIENT_NO_DOCTOR_VALUE` |
| `src/components/view/health-services/registration-management/emergency-registration/patient-selection-step.jsx` | Prop opt-in `identificationSearch` (satu kolom) dan `phoneMaxLength`; default tetap dua mode IGD |
| `src/components/view/health-services/registration-management/emergency-registration/new-patient-form.jsx` | Prop opt-in `phoneMaxLength` ke `maxLength` `BaseTextField` |
| `src/components/view/health-services/registration-management/emergency-registration/emergency-registration-fields.jsx` | `EmergencySelectField` meneruskan `searchable`, `renderOption`, `searchPlaceholder`, `emptyText` ke `FilterSelect` hanya bila diisi |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-registration-page.jsx` | Mengaktifkan cari satu kolom dan batas HP 13 |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-visit-step.jsx` | Kartu jadwal diganti `ScheduleSelect` (pesan merah, pilihan dapat dicari); Jenis Kunjungan ringkas di samping |
| `src/lib/constants/health-services/registration-management/outpatient-registration/outpatient-registration.constants.js` | `OUTPATIENT_PHONE_MAX_LENGTH = 13` |
| `src/style/health-services/registration-management/outpatient-registration/outpatient-registration.module.css` | Grid dua kolom, kolom Jenis Kunjungan, varian kartu ringkas; hanya token `var(--space-*)`, `var(--color-text-muted)`, `var(--app-text-xs)` |

### 3.3 Kepatuhan arsitektur frontend

Normalisasi dan klasifikasi ada di `utils`, bukan di view. Tidak ada `InstanceAxios` di view. Tidak ada komponen baru.

**UI GATE: 5 elemen — REUSE 2, EXTEND 2, COMPOSE 1, WRAP 0, NEW 0**

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Pesan merah Jadwal Dokter | `EmergencyInlineAlert` | `tone="error"` sudah ada | REUSE | Ganti `tone` |
| Pilihan dokter dapat dicari | `FilterSelect` via `EmergencySelectField` | `FilterSelect` punya `searchable`, `renderOption`, `searchText`; wrapper belum meneruskannya | EXTEND | Prop opt-in, default tidak berubah (dipakai IGD dan form pasien baru) |
| Cari pasien satu kolom | `PatientSelectionStep` (view bersama IGD) | Markup baris cari No. RM dipakai ulang | EXTEND | Prop opt-in `identificationSearch`; IGD tidak mengirimnya |
| Jenis Kunjungan ringkas di samping | `.visitTypeCard` IGD | Kelas kartu existing + grid | COMPOSE | Varian `visitTypeCompact` di CSS RJ |
| No. HP 13 angka | `BaseTextField` | Prop `maxLength` diteruskan ke `<input>` | REUSE | Lewat prop opt-in `phoneMaxLength` |

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol *Mencari...*; pilihan poliklinik *Memuat poliklinik...* (existing) |
| Kosong | *Pasien tidak ditemukan* dengan alasan sesuai jenis isian; *Tidak ada jadwal dokter pada …*; *Dokter tidak ditemukan* di pencarian dokter |
| Gagal | *Pencarian pasien gagal* + pesan backend; *Poliklinik dan jadwal dokter gagal dimuat* + *Coba lagi* (existing) |
| Isian tidak sah | *Data pencarian belum lengkap*, tanpa request |
| Tanpa hak akses | Mengikuti guard halaman existing (`PatientEncounter : Create`) |

## 5. Endpoint yang dikonsumsi

#### Patient

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/patient-management/master-data/patients?search=` | Cari pasien lama (tanpa perubahan) | Existing |

Endpoint jadwal, poliklinik, dan pembuatan kunjungan tidak berubah.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 8 berkas | 0 error, 5 warning lama (`set-state-in-effect`, `exhaustive-deps` pada effect existing `patient-selection-step.jsx`) | `PASS` | Baris warning bukan baris yang diubah |
| `npm run build` | exit 0, dua kali (sebelum dan sesudah perbaikan pencocok nama) | `PASS` | Log build scratchpad |
| Uji fungsi murni (loader `tests/helpers/register.mjs`) | 16/16 | `PASS` | KTP, HP 08/+62, RM pola/8 digit, nama, kode RM non-angka, isian tak sah, opsi dokter |
| Uji layar Playwright, dev server `localhost:3000`, backend `localhost:7184`, DB `QuilvianNewDevSukma`, akun SuperAdmin | 21/21 | `PASS` | U1–U21 di bawah |

Uji layar (tanpa submit kunjungan, tanpa data tersimpan):

| ID | Skenario | Hasil |
| --- | --- | --- |
| U1 | Satu kolom cari, tanpa tombol mode | PASS |
| U2–U6 | RM `00-00-00-15`, KTP berspasi, HP `0817-8168-3909` (tersimpan `+62`), nama huruf kecil, RM 8 digit → pasien IKBAL YULIYANTO; `search` terkirim `00-00-00-15` / `3322030707950004` / `81781683909` / `ikbal yuliyanto` / `00-00-00-15` | PASS |
| U7 | HP milik dua pasien → *Ditemukan lebih dari satu pasien* | PASS |
| U8 | KTP tidak terdaftar → *Pasien tidak ditemukan* | PASS |
| U9 | Isian `12` → peringatan, 0 request | PASS |
| U10 | Pesan pilih poliklinik bertone error (teks `rgb(156,55,64)`, latar `rgb(255,244,245)`) | PASS |
| U11–U12 | Jenis Kunjungan di kanan Jadwal Dokter (x 1091 vs 314+761); Umum di atas Rujukan, tinggi kartu 77px | PASS |
| U13–U16 | Poli Penyakit Dalam → pilihan dokter muncul; pencarian *Sanjaya* menyaring 1, kata acak → *Dokter tidak ditemukan*; dokter terpilih + jam/sesi/ruang tampil | PASS |
| U17 | Lanjut ke Pembayaran dengan dokter terpilih | PASS |
| U18 | Lebar 760px: kolom turun, tanpa scroll horizontal | PASS |
| U19 | RJ: No. HP berhenti di `0812345678901` | PASS |
| U20–U21 | IGD: dua mode cari tetap; No. HP IGD tidak dibatasi | PASS |

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru di repository; uji fungsi memakai skrip scratchpad.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. KTP 16 digit, HP `08…`/`+628…` dengan pemisah, nama, dan No. RM menemukan pasien yang sama lewat `GET /patients?search=` | Terpenuhi | U2–U6 |
| 2. Sebelum poliklinik dipilih, pesan Jadwal Dokter tampil merah | Terpenuhi | U10 |
| 3. Dokter dapat dicari; pilihan menampilkan nama dan jam/sesi/ruang; dokter tidak wajib dapat dilepas | Terpenuhi | U14–U16; opsi *Tanpa dokter* dibuktikan uji fungsi (DB uji: seluruh poli mewajibkan dokter) |
| 4. Jenis Kunjungan di samping Jadwal Dokter, Umum di atas Rujukan; layar sempit tetap terbaca | Terpenuhi | U11, U12, U18 |
| 5. Input No. HP berhenti di 13 angka | Terpenuhi | U19 |
| 6. Pendaftaran IGD tidak berubah | Terpenuhi | U20, U21; seluruh perluasan opt-in |
| 7. Base component dan util normalizer yang sudah ada dipakai | Terpenuhi | Tabel UI GATE; helper Kiosk dipakai ulang |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Opsi *Tanpa dokter* tidak teruji di layar karena seluruh poliklinik DB uji `IsDoctorRequired` |
| Masalah yang diketahui | Nama yang sama persis milik dua pasien ditolak sebagai *lebih dari satu pasien* (sesuai Kiosk); petugas memakai No. RM atau No. KTP |
| Dependency backend | Tidak ada. Jalur B (`RJ-DOC-DEC-067`) belum punya task |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
