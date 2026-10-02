# Laporan Perubahan Frontend — `FE-RWI-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-025` |
| Judul | Kunjungan dan episode terbentuk beserta penjaminnya — titik tulis 1 |
| Slice | Langkah **Dokter** pada alur admisi dua jalur; titik tulis pertama |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 4, entri `FE-RWI-025` |
| Trace | `RWI-CAP-002` **Wajib**; `FLOW-RI-MVP-001` langkah 2, 3, 4; `05-skema-tampilan.md` bagian 3.6; `RWI-DEC-075`, `RWI-DEC-076`; `RWI-UI-GAP-002` |
| Contract version | [`RWI-ENC-PAYER-001` versi `1.0.0`](../../../contracts/encounter-company-guarantor-contract.md), status `APPROVED` |
| Wewenang UI | Susunan isian `DEV_DISCRETION`. **Batasnya:** peringatan tentang langkah yang tidak dapat dimundurkan wajib tampil **sebelum** disimpan — batas ini dipatuhi, lihat bagian 3.4 |
| Dependency | `FE-RWI-024` (selesai); `BE-RWI-035` — **selesai 31 Agustus 2026**, sehingga gerbang task ini terbuka |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 1, database 0, keamanan/auth 1, UI/workflow 2 |
| Task mode | `FRONTEND` — backend strict read-only, kecuali laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; `NewQuilvianSystemBackend` **hanya** untuk laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `786bd247db47a3b7c97b8c08fb6ec633f57d0c72`, branch `HamzahV2` — **cocok dengan snapshot frontend pada kontrak** |
| Commit backend yang dijadikan rujukan | `29dd9d41a298db5e255f37f2f3e2a1c336a3b7b9`, branch `MHamzah` — memuat implementasi `BE-RWI-035` |
| Tanggal | 31 Agustus 2026 |
| Status | **Selesai untuk lingkup source.** Ketujuh acceptance criteria dipetakan ke bukti implementasi; `npm run lint` dan `npm run build` lulus. Bukti E2E **tidak dijalankan**, sesuai instruksi pengguna yang tercatat pada roadmap |

---

## 1. Keadaan yang ditemukan di awal

Alur admisi berlangkah sudah berdiri sampai langkah **Pembayaran**, tetapi berhenti di sana.

| Yang sudah ada | Bukti |
| --- | --- |
| Kerangka alur dua jalur, langkah tersimpan di URL | `use-inpatient-admission-flow.jsx` (`FE-RWI-022`) |
| Langkah Pendaftaran dan Pasien Lama | `inpatient-admission-registration-step.jsx`, `inpatient-admission-existing-patient-step.jsx` (`FE-RWI-023`) |
| Langkah Pembayaran: pilih cara bayar, pilih atau daftarkan kartu penjamin, pilih kelas | `use-inpatient-admission-payment.jsx`, `inpatient-admission-payment-step.jsx` (`FE-RWI-024`) |

| Yang belum ada | Akibatnya |
| --- | --- |
| Langkah **Dokter** | Slug `doctor` jatuh ke `PendingStep` — hanya menampilkan kalimat "akan dilengkapi task lanjutan". Alur mati total di sini |
| Service kunjungan di modul rawat inap | `patient-encounters` hanya dipanggil modul IGD dan kiosk. Tidak ada satu pun pemanggil dari rawat inap |
| Jangkar kunjungan pada episode | `buildAdmissionPayload` di `inpatient-admission-utils.jsx` **tidak punya** `encounterId` sama sekali |

**Akibat nyatanya bagi pengguna.** Penjamin yang sudah dipilih dengan susah payah pada langkah
Pembayaran tidak pernah sampai ke mana pun. Bahkan seandainya episode dibuat lewat layar admisi
lama, `InpEpisodeService.BuildInpatientEncounter` di backend akan membuatkan kunjungannya sendiri
dengan `PaymentType = Cash` yang ditanam di kode dan tanpa baris sumber pembayaran — jalur
`RWI-OQ-046`. Pasien berpenjamin perusahaan tetap tercatat tunai, dan tagihannya diarahkan ke
pasien, bukan ke perusahaannya.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas admisi yang memiliki hak akses `PatientEncounter : Create`.

**Kapan layar ini dibuka.** Setelah petugas menyelesaikan langkah Pembayaran, yaitu sudah memilih
cara bayar, kartu penjamin bila bukan tunai, dan kelas perawatan.

### 2.1 Langkah yang dilakukan berurutan

1. Petugas melihat panel **Penjamin dari Langkah Pembayaran** di bagian atas. Panel ini menampilkan cara bayar, nama penjamin, dan nomor polis atau nomor karyawan — supaya petugas tahu penjamin apa yang akan melekat pada kunjungan **sebelum** menekan simpan.
2. Petugas memilih **Unit Layanan**. Daftarnya hanya berisi unit bertipe rawat inap.
3. Petugas memilih **DPJP**.
4. Petugas boleh mengisi **Catatan Admisi**, paling panjang 1000 karakter.
5. Bila perlu, petugas menyalakan **Pasien membutuhkan isolasi**. Kolom keterangan baru muncul setelah sakelar dinyalakan, dan menjadi wajib.
6. Petugas membaca peringatan bahwa langkah ini tidak dapat dimundurkan.
7. Petugas menekan **Simpan & Cari Tempat Tidur**.

### 2.2 Yang terjadi ketika disimpan

Tiga permintaan dijalankan **berurutan**, tidak paralel:

| Urutan | Permintaan | Bila gagal |
| ---: | --- | --- |
| 1 | `POST /patient-encounters/admin` — kunjungan beserta satu sumber pembayarannya | **Berhenti total.** Tidak meneruskan ke pembuatan episode. Pesan server ditampilkan apa adanya, seluruh isian utuh |
| 2 | `POST /episodes` dengan `encounterId` terisi | Kunjungan yang sudah terbentuk **dipertahankan di memori**, sehingga menekan simpan lagi memakai ulang kunjungan yang sama dan tidak membuat kunjungan kedua |
| 3 | `PATCH /episodes/{id}/isolation-requirement` — hanya bila isolasi dinyalakan | Dilaporkan sebagai **peringatan**, bukan kegagalan. Kunjungan dan episode sudah tersimpan; menggagalkan seluruh langkah justru menyesatkan petugas |

### 2.3 Jalur tidak normal

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Halaman dimuat ulang tepat di langkah Dokter | Peringatan merah: penjamin belum terbaca, kembali ke langkah Pembayaran lalu pilih ulang. Tombol simpan **mati**. Penjelasannya pada bagian 3.5 |
| Unit layanan atau DPJP belum dipilih | Tombol simpan mati; pesan menyebut isian mana yang kurang |
| Isolasi menyala tanpa keterangan | Tombol simpan mati; pesan menyebut keterangan wajib diisi |
| Jenis kartu tidak cocok dengan cara bayar | Peringatan merah dan tombol simpan mati, sebelum permintaan dikirim |
| Kunjungan gagal dibuat | Pesan server apa adanya; isian tidak dihapus; episode **tidak** dibuat |
| Episode gagal dibuat padahal kunjungan sudah ada | Pesan menyebut kunjungan sudah terbentuk dan simpan ulang akan memakai kunjungan yang sama |
| Isolasi gagal ditetapkan | Peringatan kuning: kunjungan dan episode tersimpan, kebutuhan isolasi belum tercatat, tetapkan ulang dari detail episode |
| Sudah berhasil disimpan | Tombol Kembali **dinonaktifkan** disertai alasan; tombol utama berubah menjadi "Lanjut ke Pilih Bed" |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola**

- `AGENTS.md` frontend; `rules/GLOBAL_RULES.md`; `rules/frontend/frontend-architecture.md`; `rules/frontend/base-component-catalog.md`; `rules/frontend/base-component-decision-gate.md`; `rules/frontend/design-tokens.md`; `rules/frontend/page-composition-patterns.md`; `rules/frontend/ui-consistency-checklist.md`; `rules/frontend/test-policy.md`; `rules/frontend/REPORT_TEMPLATE.md`

**Blueprint dan kontrak**

- `05-skema-tampilan.md` bagian 3.6; `roadmap/frontend-roadmap.md`; `roadmap/requirement-traceability.md`; `contracts/encounter-company-guarantor-contract.md`

**Source frontend**

- `inpatient-admission-view.jsx`, `inpatient-admission-payment-step.jsx`, `inpatient-admission-registration-step.jsx`
- `use-inpatient-admission-flow.jsx`, `use-inpatient-admission-payment.jsx`, `use-inpatient-admission.jsx`
- `inpatient-admission-flow-constants.jsx`, `inpatient-admission-constants.jsx`
- `inpatient-admission-payment.service.js`, `inpatient-api.service.js`, `inpatient-episode.service.js`
- `inpatient-admission-payment-utils.jsx`, `inpatient-admission-utils.jsx`
- `base-form-control.jsx`, `base-form-control.module.css`, `resource-filter-select.jsx`, `filter-select.jsx`
- `nurse-station-cluster-staff-clinic-multi-select.jsx` — preseden pemakaian gaya field base di luar `base-features/`
- `inpatient-admission.module.css`, `src/app/globals.css`

**Source backend (read-only, sebagai kontrak otoritatif)**

- `PatientEncounterController.cs`, `PatientEncounterDtos.cs`
- `EncounterType.cs`, `VisitType.cs`, `EncounterRegistrationSource.cs`, `EncounterPaymentType.cs`
- `InpatientEpisodeDtos.cs` — `OpenAdmissionRequest`
- `ServiceUnitController.cs`, `ServiceUnitType.cs`, `DoctorController.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-encounter.service.js` (baru) | Service kunjungan rawat inap: `POST /patient-encounters/admin`, opsi unit layanan bertipe rawat inap, opsi DPJP. Bentuk penanganan error disamakan dengan service langkah Pembayaran |
| `src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx` (baru) | Pembangun payload kunjungan dan episode, pemetaan penjamin ke tipe pembayaran, normalisasi opsi dan hasil, serta empat validasi layar |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` (baru) | Controller langkah Dokter: isian, opsi ber-paginasi, penjagaan klik ganda, dan tiga permintaan berurutan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-doctor-step.jsx` (baru) | Tampilan langkah Dokter: panel tinjauan penjamin, tujuan perawatan, kebutuhan isolasi, peringatan, dan aksi |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Menambah bagian `FE-RWI-025`: nilai enum kunjungan, nilai awal isian, batas panjang, dan dua kalimat tetap |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Memasang controller langkah Dokter di level view dan mengarahkan slug `doctor` ke langkahnya |
| `src/style/health-services/inpatient-management/inpatient-admission.module.css` | Menambah tata letak langkah Dokter. Hanya tata letak — nol typography, nol warna literal |
| `src/components/features/base-features/resource-filter-select.jsx` | Meneruskan prop `id` ke `FilterSelect` yang memang sudah menerimanya. Aditif murni; lihat bagian 3.3 |

### 3.3 Gerbang keputusan base component

Layar dipecah menjadi delapan elemen. **Tidak ada satu pun berstatus `NEW`**, sehingga tidak ada
butir yang perlu menunggu keputusan pengguna.

| Elemen | Status | Komponen yang dipakai | Bukti |
| --- | --- | --- | --- |
| Judul langkah | `REUSE` | `admissionStyles.sectionHeading` | Dipakai seluruh langkah alur ini |
| Panel tinjauan penjamin | `REUSE` | `BaseDetailCard` | Dipakai `inpatient-admission-payment-step.jsx` |
| Pilihan unit layanan | `REUSE` | `ResourceFilterSelect` | Pola sama dengan pilihan kelas pada langkah Pembayaran |
| Pilihan DPJP | `REUSE` | `ResourceFilterSelect` | Sama seperti di atas |
| Catatan admisi | `REUSE` | `BaseTextAreaField` | Diekspor `base-form-control.jsx`, persis nama yang disebut skema 3.6 |
| Sakelar kebutuhan isolasi | `REUSE` | `BaseCheckboxField` | Diekspor `base-form-control.jsx`, persis nama yang disebut skema 3.6 |
| Peringatan dan pesan galat | `REUSE` | `InformationAlert` varian `warning`/`danger`/`success` | Dipakai seluruh modul |
| Tombol aksi | `REUSE` | `BaseButton` varian `primary`/`secondary` | Dipakai seluruh modul |

**Satu `EXTEND` pada base component, dan alasannya.** Skema 3.6 memberi label pada kedua pilihan
(`Unit Layanan *`, `DPJP *`), tetapi `ResourceFilterSelect` tidak menyediakan label sendiri.

Mula-mula label dibuat sebagai `<span>` beserta kelas CSS sendiri. Itu **ditarik kembali** karena
melahirkan typography baru di CSS feature yang menduplikasi — dan berisiko menyimpang dari —
label yang dirender `BaseFieldShell` tepat di bawahnya pada kolom Catatan Admisi. Gantinya,
kerangka field base dipakai langsung:

```jsx
<div className={formStyles.field}>
  <label className={formStyles.labelText} htmlFor="inpatient-doctor-dpjp">
    DPJP
    <b aria-hidden="true">*</b>
  </label>
  <ResourceFilterSelect id="inpatient-doctor-dpjp" ... />
</div>
```

Preseden pemakaian `base-form-control.module.css` dari luar `base-features/` sudah ada di
`nurse-station-cluster-staff-clinic-multi-select.jsx`, untuk kebutuhan yang persis sama.

Supaya `htmlFor` benar-benar menunjuk sesuatu, `ResourceFilterSelect` diberi satu prop `id` yang
diteruskan ke `FilterSelect`. `FilterSelect` **sudah** menerima `id` dan memakainya sebagai
`triggerId` dengan baris `const triggerId = id || ...`, sehingga nilai `undefined` jatuh ke
perilaku lama. Perubahan ini karena itu **tidak mengubah perilaku default** base component dan
tidak menunggu keputusan pengguna; hasilnya nol regresi bagi seluruh pemanggil yang sudah ada,
dan label yang benar-benar tertaut bagi pemanggil berikutnya.

### 3.4 Kepatuhan arsitektur frontend

| Aspek | Kepatuhan |
| --- | --- |
| Alur dependensi | `view → hook → service → InstanceAxios`. Komponen tidak memanggil Axios langsung |
| Penempatan folder | Service di `src/lib/services/health-services/inpatient-management/`, hook di `src/lib/hooks/...`, utils di `src/utils/...`, view di `src/components/view/...`, konstanta di `src/lib/constants/...` — sama persis dengan `FE-RWI-023` dan `FE-RWI-024` |
| Pola state | `useState` lokal pada hook di level view, sama seperti `useInpatientAdmissionPayment`. Tidak ada Redux slice baru |
| Pola HTTP | `InstanceAxios` beserta `AbortController` untuk opsi, sama seperti service langkah Pembayaran |
| Design token | Seluruh nilai visual memakai `var(--token)`. Nol `#hex`, nol `rgba()`, nol `px` mentah, nol `!important` |
| Batas wewenang UI | Peringatan tampil **sebelum** disimpan. Sesudah tersimpan, peringatan berganti menjadi keterangan penguncian dan tombol Kembali dinonaktifkan disertai alasan — skema 3.6 aturan 4 |
| Pola baru | `NONE` |

### 3.5 Penjagaan penjamin yang hilang saat halaman dimuat ulang

Ini temuan arsitektur yang tidak disebut roadmap, dan berdampak langsung pada acceptance
criteria 1.

`useInpatientAdmissionPayment` dipasang di level `InpatientAdmissionView`, sehingga pilihan
penjamin **bertahan** ketika petugas berpindah langkah. Tetapi alur ini hanya menyimpan `entry`,
`step`, dan `patientId` pada URL — pilihan penjamin **tidak** ikut tersimpan.

Akibatnya, menekan F5 tepat di langkah Dokter mengosongkan pilihan penjamin. Tanpa penjagaan,
`buildInpatientEncounterPayload` akan jatuh ke `paymentType = 1` dan kunjungan terbentuk sebagai
**Tunai secara diam-diam** — persis cacat yang hendak ditutup `RWI-UI-GAP-002`.

Karena itu `validateInpatientPayerStillSelected` menahan penyimpanan dan mengarahkan petugas
kembali ke langkah Pembayaran. Menyimpan pilihan penjamin ke URL atau penyimpanan peramban
sengaja **tidak** dilakukan: keduanya menaruh keputusan pembayaran di tempat yang dapat disunting
pengguna, dan itu keputusan yang lebih besar dari wewenang task ini.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat opsi | Daftar pilihan menampilkan "Memuat unit layanan..." atau "Memuat daftar dokter..." di dalam kontrol |
| Menyimpan | Tombol utama berubah menjadi "Menyimpan..." dan seluruh isian dinonaktifkan, sehingga klik kedua tidak mungkin |
| Kosong | "Unit layanan rawat inap tidak ditemukan" atau "Dokter tidak ditemukan" |
| Gagal memuat opsi | Pesan merah di bawah kontrol yang bersangkutan, isian lain tetap dapat dipakai |
| Gagal menyimpan | Pesan server apa adanya di atas tombol aksi; seluruh isian utuh |
| Berhasil sebagian | Peringatan kuning ketika kunjungan dan episode tersimpan tetapi isolasi belum tercatat |
| Penjamin tidak terbaca | Peringatan merah beserta arahan kembali ke langkah Pembayaran; tombol simpan mati |
| Tanpa hak akses | `NOT APPLICABLE` di lapisan layar ini — penjagaan `403` ditangani `InstanceAxios` dan `access-denied-gate` pada tingkat route, tidak diubah task ini |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/patient-encounters/admin` | Membuat kunjungan rawat inap beserta satu sumber pembayarannya | `PatientEncounter : Create` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/inpatient-management/episodes` | Membuka episode `Draft` yang berjangkar pada kunjungan di atas | `InpatientEpisode : Create` |
| `PATCH` | `/v1/health-services/inpatient-management/episodes/{id}/isolation-requirement` | Menetapkan kebutuhan isolasi | `InpatientEpisode : SetIsolation` |

#### Health Services / Master Data / Service Unit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/service-units/options?serviceUnitType=2` | Pilihan unit layanan **bertipe rawat inap saja** | `ServiceUnit : Read` |

#### Corporate / Human Resource / Master Data / Doctor

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/doctors/admin/options` | Pilihan DPJP | `Doctor : Read` |

**Route `/admin` dipilih dengan sengaja.** `GET /doctors/options` tanpa `/admin` dijaga policy
`KioskRead`, sehingga petugas admisi biasa ditolak di sana. Jebakan yang sama sudah dicatat
`FE-RWI-023` dan `FE-RWI-024`. `service-units/options` adalah pengecualian: ia hanya punya satu
varian, dan varian itu memang sudah dijaga `ServiceUnit : Read`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | `0 errors, 571 warnings` — seluruh warning berasal dari berkas lama yang tidak disentuh | `PASS` | Keluaran perintah |
| Warning lint pada berkas yang dibuat/diubah task ini | Nol | `PASS` | Keluaran lint disaring nama berkas |
| `npm run build` | `✓ Compiled successfully in 27.9s`; route `/health-services/inpatient-management/admissions` terbentuk | `PASS` | Keluaran perintah |
| Grep anti-regresi 1 — warna literal pada style baru | Nol temuan | `PASS` | `grep -nEi "#[0-9a-f]{3,8}\b\|rgba?\("` |
| Grep anti-regresi 2 — typography menimpa komponen shared | Nol temuan | `PASS` | Setelah label diubah memakai kerangka field base, bagian 3.3 |
| Grep anti-regresi 3 — tombol non-base | Nol temuan | `PASS` | `grep -nE "<button\|className=\"btn"` |
| Grep anti-regresi 4 — tabel tanpa kontrak typography | Nol temuan | `PASS` | Layar ini tidak memakai `<table>` |
| Grep anti-regresi 5 — utility `fw-`/`fs-` | Nol temuan | `PASS` | `grep -nE "fw-(bold\|semibold\|light)\|fs-[0-9]"` |
| Grep anti-regresi 6 — `!important` baru | Nol temuan | `PASS` | `grep -n "!important"` |
| Verifikasi drift snapshot frontend | HEAD `786bd247d…` cocok dengan snapshot kontrak | `PASS` | `git rev-parse HEAD` |
| Bukti E2E dan uji di peramban | Sengaja tidak dijalankan | `NOT RUN` | Instruksi pengguna yang tercatat pada roadmap: hanya `lint` dan `build`, tidak menjalankan test `.mjs` |

`AUTOMATED TEST: SKIPPED (opsional) — repository ini tidak memakai Jest, dan `test-policy.md`
menyatakan penulisan test baru bersifat opsional serta bukan gerbang selesai. Instruksi pengguna
pada roadmap juga secara khusus menyatakan test `.mjs` tidak dijalankan.`

**Uji manual: `NOT FEASIBLE`.** Alasannya konkret: memverifikasi ketujuh acceptance criteria di
peramban memerlukan aplikasi backend berjalan, database yang sudah dimigrasikan, akun dengan
`PatientEncounter : Create`, serta data master unit layanan rawat inap, dokter, kelas perawatan,
dan kartu penjamin. Roadmap secara eksplisit membatasi verifikasi task ini pada `lint` dan
`build`, sehingga menjalankan runtime berada di luar wewenangnya.

**Tidak dijalankan:** uji di peramban, uji E2E Playwright, dan test `.mjs` — ketiganya atas
instruksi pengguna yang tercatat pada roadmap.

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Kunjungan yang terbentuk bertipe `Inpatient` dan **membawa penjamin yang dipilih pada langkah Pembayaran** — dibuktikan dari permintaan dan jawaban, bukan dari kalimat di layar | **Terpenuhi** | `buildInpatientEncounterPayload` menetapkan `encounterType: 3` dan memanggil `buildInpatientEncounterPayerFields`, yang menulis ketiga referensi penjamin secara eksplisit sehingga tepat satu terisi. `normalizeInpatientEncounterResult` membaca kembali `payment` dari jawaban server, termasuk `patientCompanyGuarantorId` dan `employeeNumberSnapshot` |
| 2 | `POST /episodes` dikirim dengan `EncounterId` **terisi**; episode terbentuk berstatus `Draft` | **Terpenuhi** | `buildInpatientAdmissionEpisodePayload` mengisi `encounterId` dari hasil langkah 1; hook menolak melanjutkan bila `encounterId` kosong dengan galat eksplisit |
| 3 | Admisi tanpa DPJP ditolak dan pesannya menyebut DPJP wajib | **Terpenuhi** | `validateInpatientDoctorStep` memulangkan pesan "DPJP wajib dipilih. Episode rawat inap tidak boleh tersimpan tanpa dokter penanggung jawab." Tombol simpan mati sebelum permintaan dikirim |
| 4 | Kebutuhan isolasi yang menyala **wajib** disertai keterangan | **Terpenuhi** | `validateInpatientDoctorStep` menandai `isolationNote` wajib ketika sakelar menyala; kolomnya juga baru muncul setelah sakelar dinyalakan |
| 5 | Unit layanan yang dapat dipilih hanya yang bertipe rawat inap | **Terpenuhi** | `fetchInpatientServiceUnitOptions` mengirim `serviceUnitType=2` yang cocok dengan `ServiceUnitType.Inpatient`. Penyaringan dilakukan server supaya paginasinya tetap benar |
| 6 | Menekan simpan dua kali hanya menghasilkan satu kunjungan dan satu episode | **Terpenuhi** | Tiga lapis: `saveInFlight` berupa `ref` yang menahan klik kedua saat itu juga; `isSaved` menonaktifkan tombol; dan kunjungan yang sudah terbentuk dipakai ulang bila hanya episodenya yang gagal |
| 7 | Sebelum disimpan, layar menyatakan bahwa penjamin **tidak dapat diubah** setelah langkah ini | **Terpenuhi** | `INPATIENT_DOCTOR_STEP_WARNING` ditampilkan sebagai `InformationAlert` varian `warning` di atas tombol aksi, dan hanya berganti menjadi keterangan penguncian **setelah** penyimpanan berhasil |

### 7.1 Definition of Done

| Butir DoD | Status | Catatan |
| --- | --- | --- |
| Ketujuh kriteria dipetakan ke bukti implementasi | **Terpenuhi** | Tabel 7 |
| Lint dan build lulus | **Terpenuhi** | `0 errors`; `✓ Compiled successfully` |
| Laporan melampirkan payload tiga payer | **Terpenuhi** | Bagian 7.2 |
| Laporan mencatat test `.mjs`/E2E tidak dijalankan | **Terpenuhi** | Bagian 6 |

### 7.2 Payload tiga payer

Ketiganya dihasilkan `buildInpatientEncounterPayload`. Perhatikan bahwa ketiga referensi penjamin
**selalu** ditulis, sehingga kombinasi campuran tidak mungkin lolos tanpa terlihat.

```jsonc
// Tunai
{ "encounterType": 3, "visitType": 1, "registrationSource": 1,
  "paymentType": 1, "paymentMethodId": null,
  "patientInsuranceId": null, "patientCompanyGuarantorId": null }

// Asuransi
{ "encounterType": 3, "visitType": 1, "registrationSource": 1,
  "paymentType": 2, "paymentMethodId": null,
  "patientInsuranceId": "<MstPatientInsurance.Id>", "patientCompanyGuarantorId": null }

// Penjamin Perusahaan
{ "encounterType": 3, "visitType": 1, "registrationSource": 1,
  "paymentType": 3, "paymentMethodId": null,
  "patientInsuranceId": null, "patientCompanyGuarantorId": "<MstPatientCompanyGuarantor.Id>" }
```

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` memulangkan 571 warning, seluruhnya dari berkas lama yang tidak disentuh task ini. Berkas yang dibuat dan diubah task ini menghasilkan **nol** warning |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |

### 8.1 Delta kontrak yang dilaporkan

**`RegistrationSource = InpatientAdmission` tidak ada di source backend.**

Roadmap `FE-RWI-025` bagian **Scope** menyebut `RegistrationSource=InpatientAdmission`. Nilai itu
**tidak ada** pada `EncounterRegistrationSource`; isinya hanya `FrontDesk = 1`, `Kiosk = 2`,
`Appointment = 3`, `MobileApp = 4`, dan `WalkIn = 5`.

Sesuai urutan presedensi pada `rules/GLOBAL_RULES.md` bagian 3 — untuk hal yang diturunkan dari
source, source yang berlaku dan selisihnya dilaporkan — implementasi memakai `FrontDesk = 1`.
Admisi rawat inap memang dikerjakan petugas di meja admisi, sehingga nilai itu yang paling jujur
di antara kelimanya.

**Yang perlu diputuskan pemilik:** apakah `EncounterRegistrationSource` perlu menambah nilai
`InpatientAdmission`, atau roadmap yang dikoreksi mengikuti source. Menambah nilai enum adalah
pekerjaan backend dan berada di luar wewenang task frontend ini.

### 8.2 Masalah yang diketahui

| Masalah | Keterangan |
| --- | --- |
| Pilihan penjamin hilang saat halaman dimuat ulang | Ditahan penjagaan yang menolak menyimpan dan mengarahkan kembali ke langkah Pembayaran. Membuatnya benar-benar bertahan menuntut keputusan tempat penyimpanan yang lebih besar dari wewenang task ini — lihat bagian 3.5 |
| `motherEpisodeId` diteruskan tetapi belum dapat dipilih | Kontrol episode ibu masih placeholder sejak `FE-RWI-022`. Payload sudah menyalurkannya, sehingga ketika kontrolnya dihubungkan tidak perlu menyentuh hook ini |
| `RWI-OQ-046` tetap terbuka di backend | Jalur `POST /episodes` tanpa `encounterId` masih membuat kunjungan tunai sendiri. Layar ini selalu mengirim `encounterId`, tetapi jalurnya tetap terbuka bagi pemanggil lain |

### 8.3 Dependency backend

| Dependency | Status |
| --- | --- |
| `BE-RWI-035` | **Selesai 31 Agustus 2026.** Migration sudah diterapkan ke database dev pemilik, sehingga langkah ini dapat langsung diuji di sana |
| `BE-RWI-034` | **Belum dikerjakan.** `PATCH /episodes/{id}/isolation-requirement` memeriksa `InpatientEpisode : SetIsolation`, dan pasangan hak akses itu **tidak pernah didaftarkan** `AccessMenuSeeder`. Akibatnya permintaan isolasi akan dibalas `403` untuk siapa pun kecuali SuperAdmin. Layar sudah menangani kegagalan itu sebagai peringatan sehingga kunjungan dan episode tetap tersimpan, tetapi kebutuhan isolasi **belum dapat dipakai petugas sungguhan** sampai `BE-RWI-034` selesai |

### 8.4 Status Git

Repository frontend `QuilvianSystemFrontendDev`, branch `HamzahV2`:

```text
 M src/components/features/base-features/resource-filter-select.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx
 M src/style/health-services/inpatient-management/inpatient-admission.module.css
?? src/components/view/health-services/inpatient-management/inpatient-admission-doctor-step.jsx
?? src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx
?? src/lib/services/health-services/inpatient-management/inpatient-admission-encounter.service.js
?? src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx
```

Seluruh perubahan di atas dibuat task ini. Tidak ada perubahan pengguna yang sudah ada sebelumnya
di working tree ketika task dimulai. Tidak ada `git add`, `commit`, `push`, `pull`, `merge`,
`rebase`, atau `switch` yang dijalankan pada repository mana pun.

### 8.5 Langkah berikutnya

| Urutan | Langkah | Penanggung jawab |
| ---: | --- | --- |
| 1 | Menjalankan alur di peramban terhadap dev pemilik dan membuktikan ketujuh acceptance criteria dengan mata, terutama payload tiga payer | Frontend/QA |
| 2 | Menyelesaikan `BE-RWI-034` supaya kebutuhan isolasi benar-benar dapat ditetapkan petugas non-SuperAdmin | Backend/API |
| 3 | Memutuskan delta `RegistrationSource` pada bagian 8.1 | Product/Domain bersama Backend/API |
| 4 | Melanjutkan ke `FE-RWI-026` — Pilih Bed dan Booking Bed, titik tulis 2 | Frontend |
