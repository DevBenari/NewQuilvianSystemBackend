# Laporan Perubahan Frontend — `FE-OPT-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-030` |
| Judul | Keluhan garansi: catat (alur dari pesanan), periksa ulang, putuskan |
| Slice | Layar `FE-OPT-N12` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) |
| Trace | `FR-OPT-070`, `071`; `OPT-DEC-015`, `041`, `043`, `068`; `OPT-AC-013`, `022`, `026`; `UAT-16`, `17`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 3, 5.2, 5.3, 5.8, 5.9 |
| Contract version | `OPT-API-v1` grup Opt Warranty Claim, `OPT-STATE-v1` bagian 4, `OPT-VAL-v1` bagian 5 — `approved`, terkunci hash; tambahan `PrescriptionVersionId?` pada keputusan dari `BE-OPT-050` |
| Wewenang UI | Screen brief `approved` untuk route, kolom, form, dan modal; warna dan lebar kolom `DEV_DISCRETION` |
| Dependency | `FE-OPT-021` ✅, `BE-OPT-050` ✅ |
| Klasifikasi | `MEDIUM` — 3 route, 4 berkas view, 4 hook, 1 konstanta, 1 aturan, perubahan kecil pada pesanan, kunjungan, dan menu; 2 berkas uji baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `086bb452f` (branch `YogaV2`) + `FE-OPT-010`..`022` di working tree |
| Commit backend yang dijadikan rujukan | `fedd5e98` (branch `yoga`) |
| Tanggal | 2026-11-05 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan, uji kontrak dengan backend lokal). Alur tulis asli belum dapat dijalankan karena devYoga belum punya pesanan diserahkan dan kode profesi RO/Sp.M masih kosong. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Lapisan data keluhan sudah ada dari `FE-OPT-003`. Pendaftaran `?claimOrderId=` sudah mengarah ke form keluhan sejak `FE-OPT-010`, dan tombol Catat Keluhan sudah disiapkan `FE-OPT-020` dalam keadaan nonaktif. Fakta berikut menentukan bentuk layar:

| Fakta | Bukti | Akibat di layar |
| --- | --- | --- |
| Status garansi dan batasnya dihitung server saat keluhan dicatat | `OptWarrantyClaimService.CreateAsync` | Form menampilkan masa dari snapshot pesanan; tanggal batas baru tampil di detail, dari `WarrantyDeadlineSnapshot` |
| Keluhan wajib memakai kunjungan baru milik pasien yang sama | `OPT-VAL-061` | Layar mengikuti alur pendaftaran; penolakan server tampil apa adanya |
| Pemeriksaan ulang dijaga profesi RO/Sp.M | `OptPractitionerGuard` | Modal memberi tahu syaratnya; penolakan `403` tampil di modal dan isian tetap |
| Ringkasan kunjungan memuat pemeriksaan dengan status | `OptVisitDocumentItem` | Pilihan pemeriksaan ulang disaring ke yang final |
| Keputusan meminta id baris pesanan asal dan id versi resep | `OptWarrantyClaimDecisionRequest` | Barang dibaca dari pesanan asal saat modal dibuka; versi dibaca saat resep dipilih |
| Respons keluhan hanya memuat id pemeriksa, tanpa nama | `OptWarrantyClaimResponse` | Bagian Pemeriksaan Ulang menampilkan waktu dan temuan (bagian 8) |
| Respons pesanan tidak memuat `ParentOrderId` | `OptOrderResponse` | Pesanan remake tidak dapat menautkan ke pesanan asal (bagian 8) |

---

## 2. Proses bisnis dari sisi pengguna

**Contoh (`UAT-16`):** kacamata Ibu Sari (samaran) diserahkan 3 November 2026 dengan garansi ukuran 30 hari. Pada 13 November ia mengeluh pusing saat membaca.

1. **Dedi menekan Catat Keluhan** pada detail pesanan. Layar pendaftaran terbuka dengan pasien terkunci dan keterangan *"Keluhan atas pesanan OPT-ORD-RSMMC-000001"*.
2. **Dedi mendaftarkan kunjungan keluhan.** Layar lanjut ke form keluhan, dengan pesanan asal terkunci dan masa garansi tampil.
3. **Dedi memilih jenis keluhan dan menulis uraian.** Sesudah disimpan, detail menampilkan *Dalam garansi* dan batas 03-12-2026 dari server. Bila jenisnya kerusakan karena pemakaian, form memberi tahu lebih dulu bahwa itu tidak bergaransi.
4. **Rina (RO) memeriksa ulang.** Ia menulis temuan, misalnya *"AXIS salah ukur"*, dan boleh memilih pemeriksaan final pada kunjungan keluhan. Status menjadi *Diperiksa*.
5. **Kepala Optik memutuskan.** Bila menyetujui, ia boleh memilih barang yang dibuat ulang (kosong berarti semua barang) dan resep baru. Pesanan remake terbentuk dan tertaut dari detail keluhan, tanpa tagihan.
6. **Bila ditolak,** alasan wajib ditulis, dan detail menampilkan *"Proses sebagai pesanan berbayar baru."*

**Di luar garansi (`UAT-17`):** keluhan ukuran pada hari ke-31 berlabel *Di luar garansi*, dan modal Putuskan memperingatkan lebih dulu. Bila tetap disetujui, server menolak dengan *"Keluhan di luar masa garansi. Proses sebagai pesanan berbayar."* Modal tetap terbuka agar Kepala Optik dapat memilih Tolak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Slice dan service keluhan `FE-OPT-003`; `use-optical-visit-register`, `use-optical-order-detail`, `use-optical-visit-detail`, `optical-order-rules`, `confirm-modal`. Backend (baca-saja): `OptWarrantyClaimController`, `OptWarrantyClaimService`, `OptWarrantyClaimDtos`, `OptWarrantyClaim`, `OptVisitDtos`, `OptLabels`; laporan `BE-OPT-050`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/optical-management/warranty-claims/page.jsx` | **Baru.** Route daftar |
| `src/app/health-services/optical-management/warranty-claims/[claimId]/page.jsx` | **Baru.** Route detail |
| `src/app/health-services/optical-management/visits/[encounterId]/warranty-claims/new/page.jsx` | **Baru.** Route catat; `?orderId=` |
| `src/components/view/health-services/optical-management/warranty-claims/optical-warranty-claim-list-view.jsx` | **Baru.** Daftar |
| `src/components/view/health-services/optical-management/warranty-claims/optical-warranty-claim-form-view.jsx` | **Baru.** Form catat |
| `src/components/view/health-services/optical-management/warranty-claims/optical-warranty-claim-detail-view.jsx` | **Baru.** Detail |
| `src/components/view/health-services/optical-management/warranty-claims/optical-warranty-claim-modals.jsx` | **Baru.** Periksa Ulang, Putuskan |
| `src/lib/hooks/health-services/optical-management/warranty-claims/use-optical-warranty-claim-list.jsx` | **Baru.** Controller daftar |
| `src/lib/hooks/health-services/optical-management/warranty-claims/use-optical-warranty-claim-form.jsx` | **Baru.** Controller form |
| `src/lib/hooks/health-services/optical-management/warranty-claims/use-optical-warranty-claim-detail.jsx` | **Baru.** Controller detail dan Periksa Ulang |
| `src/lib/hooks/health-services/optical-management/warranty-claims/use-optical-warranty-claim-decision.jsx` | **Baru.** Modal Putuskan |
| `src/lib/constants/health-services/optical-management/optical-warranty-claim-constants.js` | **Baru.** Enum, kalimat, teks |
| `src/utils/health-services/optical-management/optical-warranty-claim-rules.js` | **Baru.** Query, lencana, validasi, kiriman, aksi, pilihan |
| `src/lib/constants/health-services/optical-management/optical-visit-constants.js` | **Diperbarui.** `warrantyClaim` menyala |
| `src/lib/hooks/health-services/optical-management/orders/use-optical-order-detail.jsx` | **Diperbarui.** Catat Keluhan menuju pendaftaran |
| `src/lib/hooks/health-services/optical-management/visits/use-optical-visit-detail.jsx`, `src/components/view/health-services/optical-management/visits/optical-visit-detail-view.jsx` | **Diperbarui.** Klik baris tab Keluhan |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diperbarui.** Butir Keluhan & Garansi, `OptWarrantyClaim : Read` |
| `tests/unit/optical-warranty-claim-rules.test.mjs` | **Baru.** 10 uji |
| `tests/e2e/optical-warranty-claim-screen.spec.mjs` | **Baru.** 15 skenario API tiruan |
| `tests/e2e/optical-order-fulfillment-screen.spec.mjs` | **Diperbarui.** Catat Keluhan kini aktif |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `route → view → hook → slice → InstanceAxios`, dengan slice `FE-OPT-003` tanpa perubahan. Dua pembacaan memakai service langsung dan state lokal hook, sama dengan alur pendaftaran: pesanan asal, agar tidak menimpa detail pesanan di slice, dan versi resep, sama dengan form pesanan. Tidak ada CSS baru.

Layar tidak menghitung garansi. Label dan tanggal batas seluruhnya dari server.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Keterangan muat di daftar, detail, form, dan pilihan modal; *"Membaca versi resep..."* menahan Simpan Keputusan |
| Kosong | Daftar dan pilihan modal punya keterangan masing-masing |
| Gagal | Daftar dan detail gagal → pesan server + **Coba lagi**; penolakan tindakan → pesan server di modal atau di atas tombol, isian tidak hilang; kunjungan gagal tidak menahan Periksa Ulang karena pemeriksaan opsional; pesanan asal gagal tidak menahan Putuskan, dengan keterangan semua barang dibuat ulang |
| Tanpa hak akses | `403` daftar dan detail → akses ditolak; form tanpa `Create` ditolak; menu dan tombol mengikuti izin tiap aksi |
| Data basi | `409` versi → modal tertutup, detail dimuat ulang, pesan server tampil dengan judul *Data sudah berubah* |
| Nonaktif | Tombol nonaktif selama mengirim; form ditahan tanpa `orderId` atau bila pesanan gagal dibaca |
| Validasi | Jenis, uraian, temuan, keputusan, alasan tolak, batas panjang; kalimat sama dengan backend |

Privasi: daftar tidak memuat uraian keluhan dan temuan (ada skenario E2E); nihil penulisan `console`.

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Warranty Claim

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/warranty-claims` | Daftar | `OptWarrantyClaim : Read` |
| `GET` | `/v1/health-services/optical-management/warranty-claims/{id}` | Detail | `OptWarrantyClaim : Read` |
| `POST` | `/v1/health-services/optical-management/warranty-claims` | Catat keluhan | `OptWarrantyClaim : Create` |
| `POST` | `/v1/health-services/optical-management/warranty-claims/{id}/examination` | Periksa ulang | `OptWarrantyClaim : Examine` + profesi RO/Sp.M |
| `POST` | `/v1/health-services/optical-management/warranty-claims/{id}/decision` | Putuskan | `OptWarrantyClaim : Decide` |

#### Health Services / Optical Management / modul Optik lain

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/orders/{id}` | Pesanan asal pada form dan Putuskan | `OptOrder : Read` |
| `GET` | `/v1/health-services/optical-management/visits/{encounterId}` | Kunjungan keluhan dan pilihan pemeriksaan | `OptVisit : Read` |
| `GET` | `/v1/health-services/optical-management/prescriptions/options?patientId=` | Pilihan resep baru | `OptPrescription : Read` |
| `GET` | `/v1/health-services/optical-management/prescriptions/{id}` | Id versi terkini | `OptPrescription : Read` |
| `GET` | `/v1/health-services/optical-management/reference-data/metadata` | Label jenis dan status | `OptReferenceData : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-warranty-claim-rules.test.mjs` | 10 lulus, 0 gagal, termasuk dalam `tests/unit/` di bawah | `PASS` | Keluaran perintah |
| Uji mutasi 14 aturan: alasan, keputusan, kiriman saat menolak, pemeriksaan final, baris batal, jenis resep, izin, status, label garansi, temuan, uraian | Keempat belasnya terdeteksi; berkas dipulihkan dan terbukti identik | `PASS` | Keluaran skrip |
| E2E seluruh layar Optik atas build produksi akhir | 118 lulus, 0 gagal (15 + 10 + 15 + 17 + 22 + 20 + 19) | `PASS` | Keluaran Playwright |
| Daftar | Kolom brief; label garansi; tanpa uraian dan temuan; status, cari, reset, dan paginasi mengubah query; kembali dari detail mempertahankan penyaring; gagal, kosong, `403`; menu mengikuti izin | `PASS` | Skenario daftar |
| Alur `UAT-16` | Catat Keluhan → pendaftaran terkunci → form → validasi → bentuk kiriman persis → detail dengan garansi dan batas dari server | `PASS` | Skenario alur |
| Catat | Penolakan `OPT-VAL-061` tampil dan isian tetap; keterangan kerusakan; tanpa `orderId`, pesanan gagal, dan tanpa izin menahan form | `PASS` | Skenario catat |
| Periksa ulang | Temuan wajib; hanya pemeriksaan final; pemeriksaan opsional; bukan RO/Sp.M ditolak di modal; kunjungan gagal tidak menahan; bentrok versi | `PASS` | Skenario periksa ulang |
| Putuskan `UAT-16` | Keputusan wajib; tolak butuh alasan; setujui sebagian barang + resep baru; barang kosong = semua; hanya frame tanpa pilihan resep; pesanan asal gagal tidak menahan; tautan remake | `PASS` | Skenario putuskan |
| Putuskan `UAT-17` | Label dan peringatan di luar garansi; penolakan server tampil sekali di modal; dapat lanjut menolak | `PASS` | Skenario putuskan |
| Keadaan | Aksi mengikuti izin; status akhir tanpa aksi; `404` + Coba lagi; `403`; tab Keluhan kunjungan membuka detail | `PASS` | Skenario keadaan, kunjungan |
| Backend lokal: `POST` catat, periksa ulang, setujui, tolak bentuk layar ke id acak | Seluruhnya `404` sesudah lolos binder | `PASS` | Skrip kontrak di scratchpad |
| Backend lokal: kontrol negatif jenis, `approve`, barang bukan UUID, dan status | Keempatnya `400`, membuktikan pemeriksaan di atas tidak lolos palsu | `PASS` | Skrip kontrak |
| Backend lokal: `GET` daftar dengan penyaring layar; metadata | `200`; label jenis dan status terbaca; jumlah keluhan tetap 0 | `PASS` | Skrip kontrak |
| Tangkapan layar 1366 px: daftar, form, dua modal, detail disetujui | Satu cacat ditemukan dan diperbaiki (catatan di bawah). Sesudahnya susunan sesuai brief; tabel meluap 30 px dan bergulir di dalam kartunya | `PASS` | Tangkapan layar di scratchpad |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.375 lulus, 72 gagal dari 4.451; daftar 40 berkas gagal **identik** dengan sesudah `FE-OPT-022` (`diff` kosong), tanpa berkas Optik | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar berkas |
| `npm run build` (tidak ada server lokal menyala) | `Compiled successfully in 62s`; exit 0 | `PASS` | Keluaran build |

**Cacat yang ditemukan sesudah uji awal.** (1) Tangkapan layar menunjukkan nomor keluhan terlipat dan lencana *Diperiksa* terpotong pada 1366 px; lebar kolom disesuaikan. (2) Uji `UAT-17` membuka bahwa peringatan dan penolakan server menampilkan kalimat yang sama dua kali; peringatan kini tidak tampil bila penolakan itu sudah tampil.

Uji manual: `NOT FEASIBLE` untuk alur tulis dengan backend asli; `PASS` untuk seluruh kontrol lewat Playwright dengan API tiruan. Sebabnya: devYoga belum punya pesanan diserahkan, dan kode profesi `OpticalManagement:Practitioner` kosong sehingga Periksa Ulang ditolak untuk semua akun. Perilaku tulis backend terbukti di harness `BE-OPT-050`.

**Tidak dijalankan:** rantai remake sampai diserahkan dengan backend asli; `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar sesuai `FE-OPT-N12` dan brief §5.9 | Terpenuhi, dengan selisih bagian 8 | Skenario daftar, alur, detail |
| `OPT-DEC-068`: satu alur dari pesanan | Terpenuhi | Skenario alur |
| `UAT-16`, `OPT-AC-013`: keluhan dalam garansi → periksa ulang → remake tertaut | Terpenuhi di E2E API tiruan; remake Rp 0 dan stok di harness | Skenario alur, putuskan |
| `UAT-17`, `OPT-AC-022`: di luar garansi ditolak | Terpenuhi di E2E API tiruan; perhitungan batas di harness | Skenario putuskan |
| `OPT-AC-026`: kunjungan keluhan wajib baru | Terpenuhi: alur mewajibkan pendaftaran; penolakan server tampil | Skenario alur, catat |
| Tombol mengikuti izin; server tetap penentu | Terpenuhi | Skenario keadaan |
| DoD: tidak ada data sensitif di `console` | Terpenuhi: nihil penulisan `console` | Kode |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Kunjungan keluhan terbentuk lebih dulu.** Bila petugas berhenti di form sesudah mendaftar, kunjungan tanpa keluhan tertinggal. Ini akibat urutan alur `OPT-DEC-043`; kunjungan itu tampil di Daftar Kunjungan |
| Peringatan | **Data dan izin prasyarat.** Kode profesi RO/Sp.M masih kosong, sehingga Periksa Ulang ditolak untuk semua akun. Petugas Periksa Ulang juga perlu `OptVisit : Read`, dan Kepala Optik perlu `OptOrder : Read` serta `OptPrescription : Read`. Tanpa izin itu modal tetap dapat dipakai, tetapi tanpa pilihan pemeriksaan, barang, dan resep |
| Delta brief | (1) Form menampilkan **masa** garansi dari snapshot, bukan tanggal batas seperti tertulis di brief, karena tanggal dihitung server per jenis keluhan saat disimpan; tanggal tampil di detail. (2) Pilihan Setujui tetap tersedia untuk keluhan di luar garansi, agar penolakannya dari server, sesuai `UAT-17`. (3) Pilihan resep baru hanya tampil bila barang remake memuat lensa; kecocokan resep tetap diperiksa server. (4) Tombol Buka Pesanan Asal dan Buka Kunjungan Keluhan ditambahkan pada detail |
| Dependency backend | **Dua selisih data, tidak menahan task ini.** (a) `OptOrderResponse` tidak memuat `ParentOrderId`, sehingga brief §5.8 *"tautan ke pesanan asal"* belum dapat dipenuhi; pesanan remake menampilkan label *Remake*, dan riwayat statusnya menyebut pesanan asal. (b) `OptWarrantyClaimResponse` hanya memuat id pemeriksa, sehingga nama pemeriksa belum tampil. Keduanya perlu tambahan ruas pada backend bila diinginkan |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Perubahan sampingan | Empat belas mutasi uji dibuat dan dipulihkan. Tidak ada tulisan ke devYoga |
| Interupsi | Satu pengambilan tangkapan layar kehabisan waktu pada pemuatan pertama sesudah build; diulang sesudah server dipastikan siap |
| Status Git | Di luar berkas `FE-OPT-010`..`022`: `??` `optical-warranty-claim-constants.js`, `optical-warranty-claim-rules.js`, dua berkas uji baru; folder Optik yang sudah `??` bertambah route, view, dan hook; ` M` `menu-items.jsx` |
| Langkah berikutnya | `FE-OPT-031` (katalog barang dan pengaturan). `FE-OPT-032` juga siap dikerjakan |
