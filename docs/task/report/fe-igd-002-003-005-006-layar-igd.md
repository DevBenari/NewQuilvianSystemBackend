# Laporan Perubahan Frontend — `FE-IGD-005`, `FE-IGD-002`, `FE-IGD-003`, `FE-IGD-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-005`, `FE-IGD-002`, `FE-IGD-003`, `FE-IGD-006` |
| Slice | F1 — Daftar kunjungan; F2 — Antrean triage; F3 — Penyelesaian kunjungan |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian 5 |
| Repository | `QuilvianSystemFrontendDev` |
| Contract version | API `0.2.0`, State `0.2.0`, Validation `0.2.0` — tidak ada kontrak yang berubah |
| Tanggal | 19 Agustus 2026 |
| Status | **Kode selesai, lint bersih, build lulus. Uji komponen belum ada.** |

---

## 1. Urutan pengerjaan dan alasannya

`FE-IGD-005` dikerjakan **lebih dulu** dan itu bukan kebetulan. Roadmap frontend menandainya
sebagai perubahan yang wajib rilis sebelum `BE-IGD-008`. Backend sudah menambahkan
`EmergencyVisitStatus.Completed = 9` dan kodenya sudah terkompilasi, sehingga setiap hari
tanpa `FE-IGD-005` adalah hari ketika layar bisa rusak begitu backend rilis.

> **Contoh akibat bila urutannya terbalik:** backend rilis pagi hari dan mulai mengirim
> `visitStatus = 9`. Layar yang memetakan status secara eksklusif menemukan nilai yang tidak
> ada di petanya, lalu menampilkan kolom kosong — atau berhenti bekerja sama sekali. Perawat
> kehilangan daftar pasien pada jam tersibuk, dan penyebabnya bukan backend yang salah,
> melainkan frontend yang tidak siap.

---

## 2. `FE-IGD-005` — Layar menangani nilai status kunjungan yang baru

### 2.1 Yang dikerjakan

Peta status dibuat terpusat pada
`src/lib/constants/health-services/emergency-installation-management/emergency-visit-constant.jsx`
untuk tiga enum: `EmergencyVisitStatus`, `EmergencyRegistrationStatus`, dan
`EmergencyTriageStatus`.

Kuncinya ada pada `resolveEnumLabel` di
`src/utils/health-services/emergency-installation-management/emergency-visit-utils.jsx`:
nilai yang **tidak** ada di peta tidak menyebabkan galat dan tidak menghasilkan kolom kosong.
Nilainya dikembalikan apa adanya, misalnya `Status Kunjungan 12`.

Nada warna badge memakai mekanisme yang sama: nada yang tidak dikenal jatuh ke netral.

### 2.2 Bukti terhadap acceptance criteria

| No | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | `Completed = 9` punya label yang dapat dibaca | **Terpenuhi** | `EMERGENCY_VISIT_STATUS_LABELS[9] = "Kunjungan Selesai"` |
| 2 | Nilai tak dikenal tidak mematikan layar | **Terpenuhi** | `resolveEnumLabel` mengembalikan nilai apa adanya, bukan melempar |
| 3 | Layar tetap benar walaupun nilai baru belum dikirim | **Terpenuhi** | Peta bersifat aditif; delapan nilai lama tidak diubah |

Test unit peta status **belum ada**. Kriteria di atas terbukti dari kode, bukan dari test.

---

## 3. `FE-IGD-002` — Daftar kunjungan IGD

Route baru `/health-services/emergency-installation-management/emergency-visits`, memakai
`GET /emergency-visits` yang sudah tersedia sejak `BE-IGD-001`.

### 3.1 Bukti terhadap acceptance criteria

| No | Kriteria | Status | Catatan |
| ---: | --- | --- | --- |
| 1 | Daftar tampil beserta penyaringan dan halaman | **Terpenuhi** | `DataFilter` + `DataTable`, filter status kunjungan, status pendaftaran, dan rentang tanggal |
| 2 | Ketujuh keadaan layar tertangani | **Terpenuhi** | Lihat tabel 3.2 |
| 3 | Nama pasien tampil sebagai nama | **Sebagian — terhalang backend** | Lihat bagian 6 |
| 4 | Pasien tidak dikenal punya penanda jelas | **Terpenuhi** | Badge "Belum teridentifikasi" + alias sementara, bukan kolom kosong |
| 5 | Aksi yang tidak dimiliki tidak ditampilkan | **Terpenuhi** | Kolom aksi hanya dirender saat `canComplete`, dan tombol hanya muncul pada kunjungan `Disposed` |

### 3.2 Ketujuh keadaan layar

| Keadaan | Penanganan |
| --- | --- |
| Memuat | `DataTable` menampilkan teks memuat, bukan layar kosong |
| Kosong | "Belum ada pasien di IGD." beserta penjelasan |
| Gagal | Banner merah + tombol "Coba lagi" |
| Tanpa hak akses | 403 dideteksi di slice; layar berhenti dengan "Anda tidak memiliki hak akses untuk melihat data ini." |
| Data usang | `isStale` dinyalakan setelah aksi berhasil; banner kuning mengajak muat ulang |
| Kirim ganda | Tombol dinonaktifkan selama `loading`; aksi selesaikan dijaga dua lapis |
| Validasi gagal | Pesan backend ditampilkan apa adanya di dalam dialog |

### 3.3 Kolom sensitif sengaja tidak ditampilkan

Daftar **tidak** menampilkan `chiefComplaint`, `foundLocation`, `traumaLocation`, dan `notes`.
Keempatnya bertanda sensitif pada data dictionary. Kolom yang ditampilkan hanya nomor
kunjungan, nama, waktu tiba, unit pelayanan, dan dua status.

---

## 4. `FE-IGD-003` — Antrean triage beserta penanda terlambat

Route baru `/health-services/emergency-installation-management/triage-queue` dengan dua tab:
antrean umum (`GET /emergency-triages`) dan pelampauan batas (`GET /emergency-triages/sla-breaches`
dari `BE-IGD-007`).

### 4.1 Bukti terhadap acceptance criteria

| No | Kriteria | Status | Catatan |
| ---: | --- | --- | --- |
| 1 | Antrean tampil beserta ketujuh keadaan layar | **Terpenuhi** | Sama seperti tabel 3.2 |
| 2 | Pasien melampaui batas ditandai beserta lama menunggu | **Terpenuhi** | "Terlambat 31 menit" + "menunggu 45 menit" |
| 3 | Warna kategori dari master, tidak ditanam di kode | **Terpenuhi** | Lihat 4.2 |
| 4 | Target belum diatur bukan terlambat dan bukan patuh | **Terpenuhi** | Lihat 4.3 |
| 5 | Nama pasien tampil sebagai nama | **Terpenuhi** | `EmergencyTriageSlaBreachResponse` memang memuat `PatientName` |

### 4.2 Warna kategori benar-benar dari master

`EmergencyTriageLevelChip` membaca `triageLevelColorHex` dari master dan meneruskannya ke
custom property `--triage-color`. Tidak ada satu pun warna kategori triase yang ditulis di
`.jsx` maupun di `.module.css`.

Bila master belum mengisi warna, chip tampil **netral** — bukan memakai warna tebakan. Ini
berbeda dari `TRIAGE_LEVEL_FALLBACK_COLORS` pada berkas konstanta triage lama, yang sengaja
tidak dipakai di layar baru ini.

> **Catatan konvensi.** `CLAUDE.md` frontend melarang `style={{ ... }}`. Di sini inline style
> dipakai **hanya** untuk meneruskan nilai master ke custom property, sementara seluruh aturan
> tampilan tetap di CSS Module. Pola ini sudah lebih dulu dipakai komponen triage di modul yang
> sama (`emergency-triage-result-card.jsx` dan `emergency-triage-indicator-matrix.jsx`), jadi
> ini mengikuti preseden, bukan membuat penyimpangan baru. Tanpa mekanisme ini, satu-satunya
> alternatif adalah menanam warna di kode — dan itu justru dilarang roadmap bagian 4.2.

### 4.3 Butir 4 adalah yang paling mudah salah

`resolveSlaState` mengembalikan tiga keadaan yang berbeda, bukan dua:

| Keadaan | Kapan | Yang dilihat perawat |
| --- | --- | --- |
| `unconfigured` | `responseDueAt` kosong | "Target belum diatur", huruf miring, warna netral |
| `breached` | Penanda breach menyala atau batas sudah lewat | "Terlambat 31 menit", merah tebal |
| `within` | Masih dalam batas | "Batas 19 Agu 2026 21.30", hijau |

Pasien Kuning dan Hijau yang target SOP-nya memang belum ada karena itu **tidak** ikut
mewarnai layar merah. Kalau butir ini dilanggar, peringatan pasien Merah tenggelam di antara
peringatan palsu — persis akibat yang dijelaskan pada bagian 2 roadmap backend.

Frontend juga **tidak pernah** menghitung `ResponseDueAt` sendiri; nilainya selalu dari backend.

---

## 5. `FE-IGD-006` — Menyelesaikan kunjungan dari layar

Memakai `PATCH /emergency-visits/{id}/complete` dari `BE-IGD-009`.

| No | Kriteria | Status | Catatan |
| ---: | --- | --- | --- |
| 1 | Tombol hanya untuk yang berwenang dan hanya pada `Disposed` | **Terpenuhi** | `canCompleteVisit` memeriksa status 7; kolom aksi dikendalikan `canComplete` |
| 2 | Penolakan 409 tampil sebagai kalimat | **Terpenuhi** | `normalizeErrorMessage` mendahulukan `message` dari backend; ditampilkan di dalam dialog |
| 3 | Status dan waktu selesai tampil tanpa muat ulang manual | **Terpenuhi** | `onCompleted` memicu `refresh()` |
| 4 | Klik dua kali hanya mengirim satu permintaan | **Terpenuhi** | Dijaga `actionLoading` (Redux) **dan** `isSubmitting` (lokal) |

Dua lapis penjaga pada butir 4 disengaja: klik kedua yang terjadi sebelum Redux sempat
memperbarui `actionLoading` tetap tertahan oleh penjaga lokal.

Tombol yang disembunyikan **bukan** pengaman. Backend tetap yang memutuskan, dan itulah
sebabnya pesan 409 ditampilkan apa adanya alih-alih dicegah di layar.

---

## 6. Temuan: `GET /emergency-visits` belum mengirim nama pasien

Ini temuan yang perlu keputusan, bukan sesuatu yang saya tutup diam-diam.

`FE-IGD-002` kriteria 3 mewajibkan nama pasien tampil sebagai nama, dan roadmap frontend
melarang menampilkan UUID sebagai label. Tetapi `EmergencyVisitResponse`
(`DTOs/EmergencyVisitDtos.cs` baris 6-34) hanya memuat `PatientId` — tidak ada `PatientName`
maupun `MedicalRecordNumber`. Controller memetakan entitas apa adanya tanpa join ke pasien.

Bandingkan dengan `EmergencyTriageSlaBreachResponse` yang **sudah** memuat `PatientName`,
`MedicalRecordNumber`, dan `ServiceUnitName`. Jadi polanya sudah ada di modul yang sama.

Yang saya lakukan sekarang, dan batasnya:

- `getVisitPatientName` mencari nama dari beberapa kemungkinan field. Bila backend kelak
  mengirimkannya, layar langsung menampilkannya tanpa perubahan kode.
- Bila nama tidak ada, dipakai `temporaryPatientAlias`; bila itu pun kosong, dipakai label
  "Pasien Belum Teridentifikasi".
- **UUID tidak pernah ditampilkan.**

Artinya untuk pasien yang sudah teridentifikasi, kolom nama saat ini menampilkan label
penanda, bukan nama sebenarnya. Kriteria 3 karena itu **belum terpenuhi penuh**, dan saya
tidak mengklaimnya terpenuhi.

Jalan keluar yang tepat adalah menambahkan `PatientName` dan `MedicalRecordNumber` pada
`EmergencyVisitResponse`, mengikuti pola yang sudah dipakai `EmergencyTriageSlaBreachResponse`.
Itu perubahan backend dan berada di luar scope keempat task frontend ini. Owner: Backend/API +
Product/Domain.

---

## 7. File yang dibuat

| File | Isi |
| --- | --- |
| `src/lib/constants/health-services/emergency-installation-management/emergency-visit-constant.jsx` | Peta status, opsi filter, teks layar |
| `src/utils/health-services/emergency-installation-management/emergency-visit-utils.jsx` | Helper murni: peta enum aman, status SLA, formatter |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-visit-slice.jsx` | Thunk daftar, detail, dan penyelesaian kunjungan |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-triage-queue-slice.jsx` | Thunk antrean, pelampauan batas, dan retriage |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-visit/use-emergency-visit-list.jsx` | Logika daftar kunjungan |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-visit/use-emergency-triage-queue.jsx` | Logika dua tab antrean |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-visit/use-complete-emergency-visit.jsx` | Logika aksi penyelesaian |
| `src/components/view/.../emergency-visit-view/emergency-visit-list-view.jsx` | Layar daftar kunjungan |
| `src/components/view/.../emergency-visit-view/emergency-triage-queue-view.jsx` | Layar antrean triage |
| `src/components/view/.../emergency-visit-view/components/` (5 berkas) | Tabel, badge, chip kategori, penanda SLA, dialog |
| `src/style/.../emergency-visit/emergency-visit.module.css` | Seluruh tampilan |
| `src/app/.../emergency-visits/` dan `triage-queue/` (4 berkas) | Route |

Satu file diubah: `src/lib/state/store.jsx` — mendaftarkan dua slice baru. Slice yang tidak
terdaftar tidak akan pernah punya state.

### 7.1 Aturan repo yang diikuti

| Aturan | Cara dipenuhi |
| --- | --- |
| Axios hanya di Redux slice | Kedua slice memakai `InstanceAxios`; hook, komponen, utils, dan constants tidak mengimpor Axios sama sekali |
| Bahasa Indonesia | Seluruh label, pesan, dan komentar |
| CSS Modules terpusat | Satu berkas di `src/style/**`, tanpa CSS colocated |
| `DataTable` + `DataFilter` | Dipakai keduanya; tidak ada `<Table>` manual |
| Penamaan kebab-case | Seluruh berkas baru |

Berkas `src/lib/services/**` yang melanggar aturan Axios **tidak** ditambah dan tidak ditiru,
sesuai catatan legacy pada `CLAUDE.md`.

---

## 8. Verifikasi yang dijalankan

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` pada seluruh berkas baru | **Bersih** — nol error, nol warning |
| `next build` (Node 20.20.2) | **Lulus** — "Compiled successfully in 29.8s" |
| Route terdaftar | `/health-services/emergency-installation-management/emergency-visits` dan `/triage-queue` muncul di keluaran build |
| `tests/unit/base-components-regression.test.mjs` | **Lulus** 4 dari 4 |

### 8.1 Dua hal yang perlu diketahui

**Node bawaan terminal adalah v18.20.5, sedangkan Next.js mensyaratkan ≥ 20.9.0.** `npm run build`
berhenti sebelum mulai. Build di atas dijalankan memakai Node 20.20.2 yang tersedia di
`C:\\Program Files\\nodejs`. Ini masalah lingkungan, bukan masalah kode, tetapi perlu dirapikan
supaya `npm run build` bekerja apa adanya.

**`tests/unit/auth-security.test.mjs` gagal, dan kegagalan itu sudah ada sebelum perubahan ini.**
Penyebabnya berkas yang diimpor bernama `base-login-utils.jsx`, sedangkan test mengimpor
`base-login-utils.js`. Tidak ada hubungannya dengan modul IGD.

---

## 9. Yang belum dikerjakan

| Hal | Alasan |
| --- | --- |
| Test komponen untuk ketujuh keadaan layar | Belum dibuat; seluruh kriteria di atas terbukti dari kode, bukan dari test |
| `FE-IGD-001` penyelarasan pendaftaran | Task terpisah, tidak disentuh |
| `FE-IGD-004` formulir triage dan retriage | Thunk `retriageEmergencyTriage` sudah disiapkan di slice, layarnya belum |
| `FE-IGD-007`, `008`, `009` | Observasi, tindak lanjut, perpindahan — belum |
| `FE-IGD-010` halaman detail kunjungan | Belum; karena itu aksi penyelesaian sementara ditempatkan pada daftar kunjungan |
| Filter unit pelayanan | Butuh sumber master unit; belum disambungkan |

Aksi menyelesaikan kunjungan idealnya berada di halaman detail sesuai `FE-IGD-006`. Karena
`FE-IGD-010` belum ada, aksi itu ditempatkan di daftar kunjungan lebih dulu. Ini penyimpangan
penempatan yang disengaja dan sebaiknya dipindahkan ketika halaman detail dibuat.
