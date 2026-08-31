# Laporan Perubahan Frontend — `FE-RWI-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-024` |
| Judul | Penjamin dan kelas perawatan dipilih, bukan diasumsikan |
| Slice | `FE-INP-03`, langkah **Pembayaran** pada admisi rawat inap |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/frontend-roadmap.md` revision `5`, kartu `FE-RWI-024` |
| Trace | `RWI-CAP-002` **Wajib**; `FLOW-RI-MVP-001` langkah 3; `05-skema-tampilan.md` bagian 3.5 |
| Contract version | Kontrak runtime controller backend pada commit `aa52ca4d13dbb7af737957e758083e2ff47b82a5`; kontrak lintas modul ini diverifikasi langsung dari route, DTO, dan permission source. Roadmap revision `5` masih `DRAFT` |
| Wewenang UI | Bentuk pemilihan penjamin `DEV_DISCRETION`; kelas perawatan wajib dipilih di langkah ini |
| Dependency | `FE-RWI-023` selesai; tidak ada perubahan source backend pada task ini |
| Klasifikasi | `MEDIUM` — 8 berkas source dalam satu slice: 6 diubah dan 2 dibuat; mencakup view, domain wrapper, hook, service, utility, constant, dan CSS module; tanpa route, Redux store, package, atau base component baru |
| Task mode | `FRONTEND`, dengan wewenang lintas repository terbatas untuk laporan, roadmap, dan requirement traceability |
| Target tulis | Source pada `QuilvianSystemFrontendDev`; laporan dan dua register pada `NewQuilvianSystemBackend` |
| Model | GPT-5 (Codex) |
| Commit frontend saat dikerjakan | `baca9650848ded164538ab85405190fafe8785a3` |
| Commit backend yang dijadikan rujukan | `aa52ca4d13dbb7af737957e758083e2ff47b82a5` |
| Tanggal | 2026-08-31 |
| Status | **Implementasi kelima acceptance criteria tersedia; `npm run lint:errors` dan `npm run build` lulus. Test `.mjs`, E2E, dan uji manual tidak menjadi validasi akhir sesuai instruksi pengguna, sehingga DoD yang mewajibkan E2E belum lengkap** |

---

## 1. Keadaan yang ditemukan di awal

Langkah Pembayaran sudah mempunyai implementasi awal dan laporan lama, tetapi bukti source tidak sesuai dengan klaim selesai. Pembacaan kartu pasien masih menuju daftar `/admin`, bukan endpoint opsi `/admin/options`; callback pemuatan opsi provider tidak mengikuti kontrak base select yang dipakai; nama dan kode provider dinormalisasi dari field yang berbeda dengan DTO backend. Akibatnya, petugas dapat melihat struktur layar tetapi daftar kartu atau pilihan provider berisiko kosong atau berlabel `-` pada runtime.

Implementasi awal juga memuat terlalu banyak UI khusus task: modal portal, field, badge, kartu detail, serta status visual dibuat sendiri walaupun base component yang sesuai tersedia. State cara bayar sudah ditujukan tanpa nilai bawaan, tetapi validasi kategori, sumber kartu, kelayakan kartu, pemuatan parsial, dan retensi input saat server menolak belum dipusatkan pada domain hook/utility.

`RWI-UI-GAP-002` tetap terbuka untuk `FE-RWI-025`, karena request pembuatan kunjungan belum membuktikan dukungan penjamin perusahaan. Gap itu tidak menghalangi `FE-RWI-024` untuk mendaftarkan dan memilih kartu pasien melalui controller payer yang sudah mempunyai route dan RBAC `/admin`.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna utama adalah petugas admisi. Layar dibuka setelah pasien baru berhasil didaftarkan atau pasien lama dipilih pada langkah sebelumnya.

1. Petugas melihat tiga cara bayar: **Tunai**, **Asuransi**, dan **Penjamin Perusahaan**. Tidak ada pilihan yang aktif secara otomatis.
2. Jika memilih Tunai, area kartu penjamin disembunyikan dan layar menampilkan ringkasan bahwa Tunai telah dipilih.
3. Jika memilih Asuransi atau Penjamin Perusahaan, layar hanya menampilkan kartu pasien dari jenis yang sama. Kartu nonaktif, tidak layak, atau tidak dapat dipakai tidak dapat dipilih.
4. Jika kartu belum tersedia, petugas membuka modal pendaftaran, mencari provider melalui opsi server, mengisi nomor kartu/polis atau nomor pegawai, lalu menyimpan.
5. Kartu yang berhasil dibuat langsung dipilih. Bila server menolak, pesan server tampil dan isian modal tetap ada agar dapat dikoreksi.
6. Petugas memilih kelas perawatan rawat inap. Informasi kelas hak pada kartu hanya menjadi informasi; kelas perawatan tetap merupakan pilihan sadar pada langkah ini.
7. Tombol **Lanjut** baru aktif setelah cara bayar dan kelas dipilih, serta kartu yang sesuai telah dipilih untuk pembayaran non-tunai.

Jalur tidak normal ditangani sebagai berikut: kegagalan daftar kartu menampilkan aksi **Coba Lagi**; daftar kosong mengarahkan petugas untuk mendaftarkan kartu; kegagalan opsi kelas/provider tampil di dekat kontrol terkait; perpindahan kategori menghapus kartu terpilih agar kartu dari sumber lain tidak terbawa. Respons `401`/`403` tetap ditangani oleh lapisan autentikasi dan Axios existing—layar tidak membuat data atau hak akses pengganti.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap, requirement traceability, `05-skema-tampilan.md` bagian 3.5, dan laporan `FE-RWI-023`.
- Controller, route, permission, serta DTO backend untuk patient insurance, patient company guarantor, insurance provider, company guarantor, dan patient class.
- `BaseCheckboxCard`, `BaseButton`, `BaseDetailCard`, `BaseModal`, base form controls, `DataTable`, `StatusBadge`, `InformationAlert`, dan `ResourceFilterSelect`.
- Implementasi pembayaran admisi yang sudah ada beserta pola payer pada emergency registration sebagai bukti reuse, bukan sebagai dependency runtime baru.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-admission-payment-step.jsx` | Menyusun ulang langkah Pembayaran memakai base component, tiga pilihan tanpa default, tabel terfilter, ringkasan Tunai, detail kartu terpilih, kelas wajib, state gagal/kosong, dan guard Lanjut |
| `src/components/view/health-services/inpatient-management/inpatient-admission-payer-modal.jsx` | Domain wrapper baru di atas `BaseModal` dan base form controls; mempertahankan input saat server menolak |
| `src/components/view/health-services/inpatient-management/inpatient-admission-payment-table-columns.jsx` | Definisi kolom `DataTable` baru dengan `StatusBadge`, sanitasi teks, dan status kelayakan kartu |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-payment.jsx` | Memusatkan state cara bayar, filter sumber kartu, abort request, retry, server-side option, pagination, guard simpan ganda, validasi, serta auto-select setelah create |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-payment.service.js` | Mengoreksi pembacaan kartu ke `/admin/options`, menambah parameter aktif/usable, memakai endpoint create `/admin`, dan menyelaraskan opsi provider/kelas |
| `src/utils/health-services/inpatient-management/inpatient-admission-payment-utils.jsx` | Menyelaraskan normalizer dengan DTO, memfilter kategori, memeriksa kartu selectable, memusatkan validasi langkah/form, dan mencegah kartu baru diam-diam menjadi primary |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Merapikan ikon teks pilihan admisi yang rusak encoding tanpa mengubah kontrak nilai |
| `src/style/health-services/inpatient-management/inpatient-admission.module.css` | Menghapus field, modal, badge, dan status custom yang sudah digantikan base component; menyisakan komposisi/layout domain |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi tetap `view → hook → service → InstanceAxios`, dengan normalisasi dan validasi murni di utility. State bersifat lokal pada langkah Pembayaran karena belum menjadi state lintas halaman; nomor kartu tidak ditaruh di URL maupun Redux. Tidak ada route, package, global token, base component, atau abstraction framework baru.

UI gate menghasilkan `REUSE 4`, `COMPOSE 2`, `WRAP 1`, `EXTEND 0`, `NEW 0`:

| Elemen | Keputusan | Bukti reuse |
| --- | --- | --- |
| Tiga pilihan cara bayar | `REUSE` | `BaseCheckboxCard` |
| Ringkasan Tunai | `COMPOSE` | `InformationAlert` + `StatusBadge` |
| Daftar dan status kartu | `COMPOSE` | `DataTable` + `StatusBadge` |
| Detail kartu terpilih | `REUSE` | `BaseDetailCard` |
| Modal pendaftaran | `WRAP` | Domain wrapper tipis di atas `BaseModal` dan base form controls |
| Kelas perawatan | `REUSE` | `ResourceFilterSelect` |
| Aksi dan pesan | `REUSE` | `BaseButton` + `InformationAlert` |

Pilihan yang diterapkan adalah komposisi base alert untuk Tunai, `DataTable` untuk kartu, dan wrapper `BaseModal`; alternatif kartu/table/portal custom tidak dipilih. Pemeriksaan anti-regresi pada berkas UI task tidak menemukan literal warna, `rgba()`, `!important`, raw `<button>`, raw `<table>`, atau utility typography Bootstrap baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `DataTable` menampilkan state memuat untuk kartu; dropdown kelas/provider menampilkan loading base select |
| Kosong | Untuk non-tunai: pemberitahuan bahwa kartu belum tersedia dan tombol untuk mendaftarkan kartu; Tunai tidak menampilkan area kartu |
| Gagal | Pesan API tampil melalui `InformationAlert`; daftar kartu menyediakan **Coba Lagi**; error kelas/provider berada dekat kontrolnya |
| Tanpa hak akses | Request `/admin` menerima `401`/`403` dari backend dan mengikuti handler autentikasi existing; layar tidak menampilkan data tiruan atau mencoba route kiosk |
| Belum lengkap | Pesan menjelaskan cara bayar, kartu, atau kelas yang masih wajib; tombol **Lanjut** nonaktif |
| Berhasil | Pilihan aktif terlihat, kartu terpilih berstatus **Dipilih**, kelas terisi, dan **Lanjut** aktif |
| Server menolak create | Error server tampil di modal dan semua isian yang sudah diketik dipertahankan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Patient Management / Master Data / Patient Insurance

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patient-insurances/admin/options` | Memuat kartu asuransi aktif dan usable milik pasien | `PatientInsurance : Read` |
| `POST` | `/v1/health-services/patient-management/master-data/patient-insurances/admin` | Mendaftarkan kartu asuransi pasien | `PatientInsurance : Create` |

#### Health Services / Patient Management / Master Data / Patient Company Guarantor

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patient-company-guarantors/admin/options` | Memuat kartu penjamin perusahaan aktif milik pasien | `PatientCompanyGuarantor : Read` |
| `POST` | `/v1/health-services/patient-management/master-data/patient-company-guarantors/admin` | Mendaftarkan penjamin perusahaan pasien | `PatientCompanyGuarantor : Create` |

#### Administrator / Master Data / Insurance Provider

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/administrator/master-data/insurance-providers/admin/options` | Mencari provider pada modal asuransi | `InsuranceProvider : Read` |

#### Administrator / Master Data / Company Guarantor

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/administrator/master-data/company-guarantors/admin/options` | Mencari perusahaan pada modal penjamin | `CompanyGuarantor : Read` |

#### Health Services / Master Data / Patient Class

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/patient-classes/options` | Memilih kelas dengan `isForInpatient=true` dan `activeOnly=true` | `PatientClass : Read` |

Semua endpoint operasional payer memakai route `/admin` dan permission petugas. Route kiosk tidak dipakai.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error, exit code `0` | `PASS` | ESLint `9.39.5`, mode `--quiet` |
| `npm run build` | Compiled successfully; 245 halaman statis selesai; standalone runtime siap | `PASS` | Next.js `16.2.12`, exit code `0` |
| Audit anti-regresi UI pada berkas task | Tidak ada literal warna, `rgba()`, `!important`, raw `<button>`, raw `<table>`, `fw-*`, atau `fs-*` | `PASS` | Hasil `rg` kosong |
| Automated test `.mjs` | Tidak menjadi validasi akhir sesuai instruksi pengguna untuk hanya memakai lint dan build | `NOT RUN` | Tidak ada berkas test baru yang dipertahankan pada task ini |
| E2E ketiga cara bayar dan nol request tanpa kartu | Tidak dijalankan sesuai batas validasi pengguna | `NOT RUN` | DoD E2E tetap terbuka |

Uji manual: `NOT FEASIBLE` — validasi dibatasi pengguna pada lint dan build; sesi petugas dan data master environment nyata juga tidak disediakan.

**Tidak dijalankan:** unit test, Playwright E2E, dan uji manual. Ketiganya tidak dinyatakan lulus.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ketiga cara bayar tersedia dan dipilih sadar; tidak ada nilai bawaan tersimpan diam-diam | Terimplementasi; belum dibuktikan E2E | `paymentCategory` dimulai `null`; perubahan pasien me-reset pilihan; tiga opsi dirender dari constant |
| 2. Asuransi dan penjamin perusahaan menuntut kartu dipilih/didaftarkan; tanpa kartu tidak dapat lanjut | Terimplementasi; belum dibuktikan E2E | `validateInpatientPaymentSelection` menolak non-tunai tanpa `selectedPayer`; tombol Lanjut memakai `disabled={!payment.canContinue}` |
| 3. Kelas perawatan dipilih di langkah ini | Terimplementasi; belum dibuktikan E2E | `ResourceFilterSelect` berada pada langkah Pembayaran dan validasi mewajibkan `patientClassId` |
| 4. Nomor kartu dan peserta tidak muncul di luar langkah ini/formulir cetak | Terimplementasi berdasarkan audit source | Data payer berada pada hook lokal dan hanya dirender oleh step/modal Pembayaran; tidak disimpan ke URL atau Redux |
| 5. Isian tidak hilang ketika server menolak | Terimplementasi; belum dibuktikan E2E | Modal hanya menjalankan `form.reset` setelah `onSave` sukses; jalur gagal mengembalikan pesan tanpa reset |
| Verification roadmap: E2E tiga cara bayar dan nol request saat lanjut tanpa kartu | Belum terpenuhi | Tidak dijalankan sesuai instruksi pengguna |
| DoD: kelima kriteria lulus dan E2E ada/lulus | Belum terpenuhi penuh | Implementasi tersedia serta lint/build lulus, tetapi E2E tidak tersedia/lulus |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Roadmap revision `5` dan skema `0.4` masih `DRAFT`; instruksi pengguna memberi wewenang task, bukan approval menyeluruh atas revision tersebut |
| Masalah yang diketahui | Pilihan payer masih state lokal langkah Pembayaran dan belum menjadi payload encounter; integrasi itu milik `FE-RWI-025`. E2E task belum dijalankan |
| Dependency backend | `RWI-UI-GAP-002` tetap menahan dukungan payer perusahaan pada request encounter di `FE-RWI-025`, tetapi tidak menahan pendaftaran/pemilihan kartu pada task ini |
| Perubahan sampingan | Artefak test runner sementara dipulihkan; instalasi browser Playwright yang sempat dimulai dihentikan setelah pengguna membatasi validasi. Tidak ada source backend, dependency package, lockfile, route, atau Redux yang diubah |
| Interupsi | Satu interupsi pengguna; pekerjaan dilanjutkan dengan scope validasi baru: lint dan build saja |
| Status Git | Frontend: 6 berkas source berubah dan 2 berkas source baru. Backend: laporan ini, `frontend-roadmap.md`, dan `requirement-traceability.md` berubah. Tidak ada staging/commit/push |
| Langkah berikutnya | Jika DoD penuh dibutuhkan, jalankan E2E tiga cara bayar pada environment berizin. Setelah itu lanjutkan `FE-RWI-025` untuk meneruskan payer terpilih ke encounter/episode |
