# Laporan Perubahan Frontend — `FE-OPT-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-010` |
| Judul | Menu Optik, daftar kunjungan, pendaftaran, dan detail kunjungan |
| Slice | Layar pertama Optik (`FE-OPT-N01`..`N03`); prasyarat `FE-OPT-011`, `012` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) — Rincian `FE-OPT-010`..`032` |
| Trace | `FR-OPT-010`, `FR-OPT-011`; `OPT-DEC-006`, `037`, `043`, `061`, `062`, `063`, `065`; `UAT-01`, `UAT-02`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 2, 3, 5.1–5.3, 6 |
| Contract version | `OPT-API-v1` grup Opt Visit, Opt Refraction Exam, Opt Prescription, Opt Order — `approved`, terkunci hash; tambahan rujukan bertaut dari `BE-OPT-003`; Registrasi dan Patient Management as-is untuk pilihan pasien, kartu, dan kunjungan asal |
| Wewenang UI | Screen brief `approved` untuk menu, route, tab, dan kolom; warna, ikon, dan lebar kolom `DEV_DISCRETION` (`OPT-FE-004`) |
| Dependency | `FE-OPT-000` ✅, `002` ✅, `BE-OPT-010` ✅, `BE-OPT-003` ✅ |
| Klasifikasi | `MEDIUM` — 3 route, 4 berkas view, 3 hook, 1 konstanta baru, 1 aturan baru, 1 menu, 2 berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji layar dengan backend lokal). Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Lapisan data Optik sudah ada dari `FE-OPT-002` dan `003`, tetapi belum ada menu, route, maupun layar. Fakta berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| `GET visits` tanpa `date` memulangkan **semua** tanggal | `OptVisitListQuery.Date`; komentar `FE-OPT-002` keliru menyebut "hari ini menurut server" | Layar selalu mengirim tanggal; komentar konstanta diperbaiki |
| `IsReferral` berarti rujukan luar dan mewajibkan nomor surat serta instansi | `EncounterIntakeService`; `BE-OPT-003` | Rujukan Poli Mata mengirim `isReferral: false` + `referralSourceEncounterId` |
| `/options` kartu pasien hanya untuk kiosk dan administrator | `PatientInsuranceController`, `PatientCompanyGuarantorController` | Layar memakai `/admin/options` berizin |
| `GET warranty-claims` tidak punya penyaring kunjungan | `OptWarrantyClaimListQuery` | Tab Keluhan dibaca dari `GET visits/{encounterId}` |
| `PaymentType` dan `EncounterStatus` dikirim sebagai angka; metadata Optik tidak memuatnya | `OptVisitListItemResponse`, `OptMetadataResponse` | Label cara bayar di konstanta; label status dari `encounterStatusLabel` jawaban |
| Registrasi menolak `CompanyGuarantor` untuk kunjungan penunjang | `EncounterIntakeService.LoadPaymentSourceReferenceAsync`; `BE-OPT-010` bagian 8 | Lihat bagian 8 |

---

## 2. Proses bisnis dari sisi pengguna

1. **Petugas membuka Kunjungan Optik.** Daftar menampilkan kunjungan hari ini menurut WIB. Mencentang *Semua yang masih terbuka* mengabaikan tanggal; *Ada pesanan terbuka* dan Cari ikut menyaring. Penyaring tetap sama saat kembali dari detail. **Contoh:** Sari Wulan tampil `ENC-RSMMC-00101`, 09.05, Tunai, rujukan `ENC-RSMMC-00100 · dr. Bima`, Periksa 1, Resep 1, Pesanan 1.
2. **Admisi mendaftarkan kunjungan.** Petugas memilih pasien, cara bayar, lalu bila perlu mencentang *Rujukan Poli Mata* dan memilih kunjungan asal pasien itu. Mengganti pasien membuang kartu dan kunjungan asal pasien sebelumnya. Sesudah simpan, layar membuka detail kunjungan baru.
3. **Jaringan putus atau `503` saat mendaftar.** Pesan server tampil dengan tombol *Coba lagi*, dan kiriman ulang memakai kunci yang sama sehingga tetap satu kunjungan. **Contoh:** Ibu Sari didaftarkan, layanan registrasi `503`, petugas menekan *Coba lagi* → satu kunjungan. Bila petugas mengganti cara bayar lebih dulu, itu niat baru dengan kunci baru.
4. **Penolakan server tampil apa adanya** di atas tombol dan isian tidak hilang, mis. *"Kunjungan asal rujukan bukan milik pasien ini. Pilih kunjungan pasien yang sama."*
5. **Alur keluhan** (`?patientId=&claimOrderId=`). Pasien diambil dari pesanan asal dan terkunci, dengan keterangan *"Keluhan atas pesanan OPT-ORD-RSMMC-000001 — sesudah simpan, lanjut ke form keluhan."* Pesanan yang gagal dimuat atau milik pasien lain menahan tombol Daftarkan.
6. **Detail kunjungan** menampilkan header dan lima tab. Kunjungan tertutup (Selesai, Dibatalkan, Tidak Hadir) tetap dapat dibaca tanpa tombol pembuat. Petugas tanpa izin baca pesanan tetap dapat memakai tab lain; tab Pesanan hanya menampilkan pesan server.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/` (`frontend-architecture`, `ui-consistency-checklist`, `test-policy`, `REPORT_TEMPLATE`); pola MCU (`mcu-patient-list-view`, `mcu-walk-in-view`, `mcu-order-detail-view`, hook pasangannya, `mcu-patient-list-screen.spec.mjs`); `use-lab-patient-registration-form`; `use-select-resource`, `data-filter`, `data-table`, `base-form-control`, `private-route-token-utils`. Backend (baca-saja): `OptVisitController`, `OptVisitService`, `OptVisitDtos`, DTO pemeriksaan/resep/pesanan/keluhan, `EncounterIntakeService`, `PatientEncounterController`, `PatientInsuranceController`, `PatientCompanyGuarantorController`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diperbarui.** Grup Optik sesudah Medical Check-Up, sebelum Operasi; butir Kunjungan Optik |
| `src/app/health-services/optical-management/visits/page.jsx` | **Baru.** Route daftar |
| `src/app/health-services/optical-management/visits/register/page.jsx` | **Baru.** Route pendaftaran; query alur keluhan divalidasi UUID |
| `src/app/health-services/optical-management/visits/[encounterId]/page.jsx` | **Baru.** Route detail; `?tab=` |
| `src/components/view/health-services/optical-management/visits/optical-visit-list-view.jsx` | **Baru.** Daftar |
| `src/components/view/health-services/optical-management/visits/optical-visit-register-view.jsx` | **Baru.** Form pendaftaran |
| `src/components/view/health-services/optical-management/visits/optical-visit-detail-view.jsx` | **Baru.** Header + lima tab |
| `src/components/view/health-services/optical-management/visits/optical-visit-table-columns.jsx` | **Baru.** Kolom daftar dan tab |
| `src/lib/hooks/health-services/optical-management/visits/use-optical-visit-list.jsx` | **Baru.** Controller daftar |
| `src/lib/hooks/health-services/optical-management/visits/use-optical-visit-register.jsx` | **Baru.** Controller pendaftaran dan alur keluhan |
| `src/lib/hooks/health-services/optical-management/visits/use-optical-visit-detail.jsx` | **Baru.** Controller detail; tab dimuat saat dibuka |
| `src/lib/constants/health-services/optical-management/optical-visit-constants.js` | **Baru.** Route, teks, tab, sumber pilihan, `OPTICAL_SCREEN_READY` |
| `src/lib/constants/health-services/optical-management/optical-constants.js` | **Diperbarui.** Komentar tanggal kosong diperbaiki; nilai tidak berubah |
| `src/utils/health-services/optical-management/optical-visit-rules.js` | **Baru.** Query daftar, validasi, isi pendaftaran, tombol tab, label |
| `tests/unit/optical-visit-rules.test.mjs` | **Baru.** 14 uji |
| `tests/e2e/optical-visit-screen.spec.mjs` | **Baru.** 22 skenario API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice/service → InstanceAxios`, mengikuti MCU. Seluruh elemen memakai komponen yang ada (`REUSE`); tidak ada base component, CSS, atau state baru. Slice `FE-OPT-002`/`003` dipakai tanpa perubahan.

**Penyimpangan dari checklist, beserta alasannya:**

- *Detail memakai `DataFilter` bertab, bukan `BaseDetailView`.* Mengikuti detail order MCU, acuan bentuk brief ini, karena header dan lima tab tidak cocok dengan `BaseDetailView`.
- *Resep berlaku pasien memakai state lokal di hook.* Slice resep hanya menampung satu daftar, dan daftar itu dipakai resep kunjungan ini.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Teks muat di tabel, daftar, dan detail; tombol nonaktif selama kiriman berjalan |
| Kosong | Kalimat brief *"Belum ada kunjungan Optik pada tanggal ini."*; varian *"Tidak ada kunjungan Optik yang masih terbuka."*; tiap tab punya keterangan dan aksi berikutnya |
| Gagal | Pesan server + *Coba lagi* pada daftar, detail, dan tab |
| Tanpa hak akses | `403` daftar/detail → `AccessDeniedGate`; tanpa `OptVisit : Create` → pendaftaran ditolak; `403` tab → pesan server tanpa *Coba lagi* |
| Data basi | Tautan token kedaluwarsa → pesan buka kembali dari daftar; baris slice kunjungan lain disaring per `encounterId` |
| Nonaktif | Tombol menuju halaman yang belum dibangun → nonaktif, *Fitur belum tersedia* |
| Validasi | Pasien, kartu, kunjungan asal, panjang catatan ditandai di dekat isian |

Privasi: daftar dan tab tidak menampilkan nilai resep walau server mengirimnya; tidak ada penulisan ke `console`.

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/visits` | Daftar | `OptVisit : Read` |
| `GET` | `/v1/health-services/optical-management/visits/{encounterId}` | Header, Ringkasan, tab Keluhan | `OptVisit : Read` |
| `POST` | `/v1/health-services/optical-management/visits` | Pendaftaran; header `Idempotency-Key` | `OptVisit : Create` + `PatientEncounter : Create` |

#### Health Services / Optical Management / Opt Refraction Exam

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/refraction-exams?encounterId=` | Tab Pemeriksaan | `OptRefractionExam : Read` |

#### Health Services / Optical Management / Opt Prescription

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/prescriptions?encounterId=` | Resep kunjungan ini | `OptPrescription : Read` |
| `GET` | `/v1/health-services/optical-management/prescriptions?patientId=&validOnly=true` | Resep berlaku pasien | `OptPrescription : Read` |

#### Health Services / Optical Management / Opt Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/orders?encounterId=` | Tab Pesanan | `OptOrder : Read` |
| `GET` | `/v1/health-services/optical-management/orders/{id}` | Pesanan asal alur keluhan | `OptOrder : Read` |

#### Pilihan lintas modul (as-is)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patients/admin/options` | Cari pasien | Izin baca pasien |
| `GET` | `/v1/health-services/patient-management/master-data/patient-insurances/admin/options` | Kartu asuransi | `PatientInsurance : Read` |
| `GET` | `/v1/health-services/patient-management/master-data/patient-company-guarantors/admin/options` | Kartu penjamin | `PatientCompanyGuarantor : Read` |
| `GET` | `/v1/health-services/registration-management/patient-encounters/admin/options` | Kunjungan asal (`encounterType=Outpatient`, `onlyActive=false`) | `PatientEncounter : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-visit-rules.test.mjs` | 14 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Uji mutasi: `isReferral` mengikuti centang / tanggal tidak diisi hari ini | Uji terkait gagal; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran perintah |
| E2E `tests/e2e/optical-visit-screen.spec.mjs` atas build produksi | 22 lulus, 0 gagal | `PASS` | Keluaran Playwright |
| Menu | Tampil bagi `OptVisit : Read`, hilang tanpa izin | `PASS` | Skenario menu |
| Penyaring daftar | Tanggal bawaan, terbuka tanpa tanggal, pesanan terbuka, cari, atur ulang, paginasi, kembali ke halaman 1, bertahan saat kembali dari detail | `PASS` | Skenario daftar |
| Pendaftaran | Validasi, kartu `/admin/options`, ganti cara bayar/pasien membuang isian, `isReferral: false`, `503` kunci sama, ubah isi kunci baru, klik ganda satu kiriman | `PASS` | Skenario pendaftaran |
| Alur keluhan | Pasien terkunci, lanjut ke URL form keluhan; pesanan pasien lain atau `403` menahan tombol | `PASS` | Skenario alur keluhan |
| Detail | Lima tab, kolom brief, nilai resep tidak tampil, tombol per izin, kunjungan tertutup, `403` tab, `404`, `?tab=` | `PASS` | Skenario detail |
| Backend lokal, query persis layar | Seluruh endpoint di bagian 5 menjawab `200`, kecuali kunjungan acak `404`; nama ruas cocok | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal, layar asli, superadmin | Pilih pasien → rujukan → Daftarkan → kunjungan `ENC-RSMMC-00186` tampil di detail dan daftar; empat tab lain menjawab `200` | `PASS` | Tangkapan layar di scratchpad |
| `POST visits` dengan pasien acak | `400` `OPT-REG-REJECTED`; bentuk kiriman layar diterima backend | `PASS` | Keluaran skrip |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.296 lulus, 72 gagal dari 4.372. Jumlah gagal sama dengan baseline `FE-OPT-003`; tidak ada di berkas Optik. Dibandingkan menurut jumlah dan berkas, bukan `diff` nama | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npm run build` (server lokal dimatikan lebih dulu) | `Compiled successfully in 62s`; 568 halaman statis; exit 0 | `PASS` | Keluaran build |

Uji manual: `PASS` — seluruh kontrol di atas dijalankan lewat Playwright pada build produksi, dan alur daftar–detail diulang dengan backend asli.

**Tidak dijalankan:** uji dengan akun petugas Optik non-superadmin, karena belum ada akun dengan izin Optik di devYoga (`403` hanya diuji dengan tiruan); kartu asuransi dengan data asli, karena pasien uji tidak punya kartu; `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh); `UAT-02` di backend asli lewat layar, karena pilihan layar hanya menawarkan kunjungan pasien yang sama (penolakan server terbukti di harness `BE-OPT-003`, penampilannya di E2E).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N01` dan brief §5.1 | Terpenuhi | Skenario daftar |
| Layar sesuai `FE-OPT-N02` dan brief §5.2, termasuk alur keluhan | Terpenuhi; form keluhan tujuan dibangun `FE-OPT-030` | Skenario pendaftaran |
| Layar sesuai `FE-OPT-N03` dan brief §5.3 | Terpenuhi untuk isi tab. Tombol dan klik baris menuju pemeriksaan, resep, pesanan, dan keluhan aktif bersama halaman tujuannya | Skenario detail |
| `UAT-01`: pasien terdaftar didaftarkan, kunjungan tampil di daftar | Terpenuhi, termasuk dengan backend asli | `ENC-RSMMC-00186` |
| `UAT-02`: rujukan pasien lain ditolak, tanpa kunjungan terbentuk | Terpenuhi di layar: pilihan disaring per pasien, ganti pasien membuang pilihan, pesan server tampil | Skenario pendaftaran; harness `BE-OPT-003` |
| Layar tidak menghitung uang atau status sendiri | Terpenuhi. Status tertutup hanya dipakai menyembunyikan tombol; server tetap penentu | Uji unit |
| DoD: tidak ada data sensitif di `console` | Terpenuhi: nihil penulisan `console`; tiga galat `console` saat uji berasal dari foto profil navbar | Keluaran skrip |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Penjamin Perusahaan selalu ditolak.** Pilihan tampil sesuai brief, tetapi Registrasi menjawab *"Penjamin perusahaan hanya dapat dipilih lewat pendaftaran loket."* padahal `OPT-DEC-030` memasukkan pasien perusahaan ke MVP. Perlu keputusan pemilik Registrasi (Muhammad Hamzah), sudah tercatat di `BE-OPT-010` |
| Peringatan | **Pilihan Kunjungan Asal belum khusus Poli Mata.** Query brief memuat semua kunjungan rawat jalan pasien, termasuk kunjungan Optik sendiri karena jenisnya juga `Outpatient`. Label menampilkan klinik atau unit sehingga petugas dapat membedakan; penyaringan butuh revisi brief |
| Peringatan | **Izin lintas modul.** Petugas pendaftaran juga butuh `PatientEncounter : Create`, `PatientEncounter : Read`, dan `PatientInsurance : Read`. UAT superadmin tidak dapat membuktikan ini; perlu diuji dengan akun petugas Optik |
| Delta brief | (1) Menu baru berisi Kunjungan Optik; butir lain ditambahkan task pemilik layarnya agar tidak menuju halaman kosong (`OPT-DEC-061`). (2) Tombol tab dan klik baris nonaktif sampai `OPTICAL_SCREEN_READY` dinyalakan `FE-OPT-011`, `012`, `020`, `030`; `POST refraction-exams` belum disambungkan agar tidak membuat draf yang berakhir di halaman 404. (3) Label status memakai label pada jawaban server, misalnya *Terdaftar*, bukan *Terbuka* pada sketsa brief. (4) Tab Keluhan hanya menampilkan nomor, status, dan waktu, sesuai isi `GET visits/{encounterId}`. (5) Pada 1366 px dengan sidebar terbuka, kolom Status perlu digulir di dalam kartu tabel, sesuai brief §6 |
| Data uji | Satu kunjungan `ENC-RSMMC-00186` (status Terdaftar, catatan *Uji FE-OPT-010*) tertulis di devYoga, dengan kunjungan asal rawat jalan `ENC-RSMMC-00172`. Dibiarkan sebagai data uji |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Dependency backend | `NONE` untuk layar ini. Penjamin Perusahaan menunggu keputusan di atas |
| Perubahan sampingan | `NONE`. Dua mutasi uji dibuat dan dipulihkan; Playwright memakai `--output` scratchpad sehingga `test-results/` tidak tersentuh |
| Interupsi | `NONE` |
| Status Git | `M optical-constants.js`, `M menu-items.jsx`; `??` folder route, view, dan hook Optik, `optical-visit-constants.js`, `optical-visit-rules.js`, dua berkas uji |
| Langkah berikutnya | `FE-OPT-011` (form pemeriksaan refraksi + riwayat revisi) |
