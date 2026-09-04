# Laporan Perubahan Frontend — `FE-RWI-035`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-035` |
| Judul | Alur bisnis utama terbukti berjalan ujung ke ujung |
| Slice | `F13` — Perapian dan kesiapan |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 5, kartu `FE-RWI-035` |
| Trace | `03-frontend-architecture.md` bagian 10; `RWI-DEC-051`; `GUARD-INP-01` s.d. `GUARD-INP-04`; `FLOW-RI-MVP-001`; `05-skema-tampilan.md` bagian 3–25 |
| Contract version | API `0.4.0` berstatus `draft`; `RWI-ENC-PAYER-001` `1.0.0`; `RWI-BED-BOARD-RESERVATION-001` `1.0.0`. Ditambah satu operasi baca yang dibuka `BE-RWI-034` pada 1 September 2026 dan **belum** tercatat pada nomor versi kontrak |
| Wewenang UI | Kartu `FE-RWI-035` memberi **`Tidak ada`**. Perubahan tampilan pada layar Kelayakan Keuangan memakai wewenang milik `FE-RWI-013`, yang berbunyi **`Bebas`**, karena penyelesaian kriteria tertahan task itu memang masuk `Scope` `FE-RWI-035` |
| Dependency | `FE-RWI-020` s.d. `FE-RWI-034` ✅ selesai; `FE-RWI-036`, `037`, `038`, `040`, `041` ✅ selesai; **`FE-RWI-039` ⛔ `BLOCKED`** |
| Klasifikasi | `HEAVY` — satu berkas e2e baru 1.100+ baris yang melintasi 5 layar dan 3 titik tulis, ditambah perubahan source pada 4 berkas, 1 berkas test unit, dan 3 berkas e2e yang sudah ada |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev/src/`, `QuilvianSystemFrontendDev/tests/`; ditambah wewenang lintas repository yang sempit untuk laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` |
| Model | `claude-opus-5` |
| Commit frontend saat dikerjakan | `5f587bb1c` pada branch `HamzahV2` — perubahan task ini **masih lokal, belum di-commit** |
| Commit backend yang dijadikan rujukan | `514b1d8` pada branch `MHamzah` — dibaca saja, tidak diubah |
| Tanggal | 1 September 2026 |
| Status | 🟡 **Sebagian.** 5 dari 8 acceptance criteria terpenuhi, 2 terpenuhi sebagian, 1 tidak dapat diselesaikan karena dependency-nya belum dikerjakan |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Yang sudah ada

Modul Rawat Inap sudah memiliki **17 berkas e2e** berisi 8.118 baris di `tests/e2e/`, dan
**22 berkas test unit** di `tests/unit/`. Cakupannya luas: papan tempat tidur, census, daftar
kerja episode, detail episode, keputusan pulang, kelayakan keuangan, penutupan, pencatatan
kepergian, sesi koreksi, daftar pantau, pembatalan admisi, butir administrasi, dan pengaturan.

### 1.2 Celah yang membuat outcome belum tercapai

Satu bagian alur bisnis **tidak pernah dilalui e2e mana pun**: alur admisi itu sendiri.

Bukti: tidak ada berkas `tests/e2e/inpatient-admission*.spec.mjs` selain
`inpatient-admission-cancellation.spec.mjs`, yang menguji pembatalan — bukan pembuatannya.
Akibatnya empat hal berikut tidak pernah dibuktikan di peramban:

1. Sembilan langkah jalur pasien baru dan jalur pasien lama benar-benar berurutan.
2. Ketiga titik tulis — kunjungan, pemesanan tempat tidur, penguncian admisi — benar-benar
   terkirim dengan isi yang benar.
3. Penjamin yang dipilih petugas benar-benar melekat pada kunjungan, bukan tergantikan tunai.
4. Rangkaian dari admisi sampai episode `Closed` benar-benar tersambung oleh episode yang sama.

### 1.3 Temuan yang mengubah dasar sebuah kriteria tertahan

Kriteria 7 task ini meminta empat kriteria yang tertahan sejak revision 2 **diselesaikan atau
dinyatakan tertahan beserta alasannya yang masih berlaku**. Keempat alasan itu diperiksa ulang
terhadap source dan kontrak hari ini, dan **satu di antaranya sudah gugur**:

| Kriteria tertahan | Alasan lama | Keadaan 1 September 2026 |
| --- | --- | --- |
| `FE-RWI-003` kriteria 2 | Menu tidak dapat disembunyikan per peran | **Masih berlaku.** [`filter-menu-items-by-role.jsx`](../../../../../../../../QuilvianSystemFrontendDev/src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx) masih mengembalikan submenu apa adanya; seluruh cabang penyaring perannya berupa comment |
| `FE-RWI-004` kriteria 4 | Mewarisi `FE-RWI-003` kriteria 2 | **Masih berlaku**, sebab pewarisannya masih berlaku |
| `FE-RWI-012` kriteria 2 | Dua cara pulang tertahan `RWI-DEC-059` yang berstatus `draft` | **Masih berlaku.** `00-interview-decisions.md` masih mencatat `RWI-DEC-059` sebagai `draft` yang **tidak dapat naik ke `approved`** tanpa pemilik klinis |
| `FE-RWI-013` kriteria 3 | Kontrak `0.4.0` tidak menyediakan `GET .../financial-clearance` | ⛔ **Alasannya sudah gugur.** `BE-RWI-034` membuka endpoint itu pada 1 September 2026 |

Bukti gugurnya alasan keempat, dari source backend `514b1d8`:

- [`InpatientDischargeController.cs:414`](../../../../../../../../NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs)
  mendaftarkan `[HttpGet("{episodeId:guid}/financial-clearance")]` dengan hak akses
  `InpatientDischarge : ReadFinancialClearance`.
- [`contracts/api-contract.md`](../../../contracts/api-contract.md) baris 151 menandainya
  ✅ **Tersedia** — dibuka `BE-RWI-034`.

Sementara itu sisi frontend masih memakai jalan lama. Sebelum task ini,
`use-inpatient-financial-clearance.jsx` hanya membaca dua endpoint saat halaman dimuat —
detail episode dan `closure-readiness` — dan riwayat penandaan baru terisi **setelah ada
penandaan baru dikirim dari layar itu**. Karena alasan tertahannya sudah gugur, kriteria itu
harus **diselesaikan**, bukan dinyatakan tertahan lagi.

### 1.4 Dependency yang belum selesai

`FE-RWI-039` — repair layar Selisih Tempat Tidur — berstatus ⛔ `BLOCKED` pada roadmap,
menunggu approval skema. Ini dependency langsung `FE-RWI-035` lewat kriteria 8.

Ketidakhadirannya bukan dugaan. Berkas e2e `tests/e2e/inpatient-bed-drift.spec.mjs` sudah
ditulis terhadap desain targetnya dan **gagal** pada `bed-drift-admin-state-notice`; penanda
itu memang tidak ada di `src/` mana pun. Layar Selisih Tempat Tidur karena itu belum memenuhi
bentuk yang diminta `FE-RWI-039`, dan `FE-RWI-035` **tidak boleh** menambalnya di sini —
roadmap menyatakan tegas bahwa task ini "hanya memverifikasi hasil akhir dan tidak boleh
menjadi tempat menyisipkan repair yang belum dikerjakan".

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Alur yang dibuktikan

Penggunanya petugas admisi, dibantu petugas kasir dan petugas ruangan pada bagian akhirnya.
Alurnya berurutan seperti ini:

1. **Petugas membuka Admisi Rawat Inap** dan memilih jalur — pasien baru atau pasien lama.
2. **Pasien ditetapkan.** Pada jalur pasien lama, petugas mencari dengan nomor rekam medis
   atau NIK, lalu meninjau identitasnya pada layar Periksa Data Pasien sebelum melanjutkan.
3. **Tipe pasien dipilih** — Umum, Ibu, Bayi Baru Lahir, Anak, Pegawai, atau Korporat.
4. **Cara bayar dan kelas perawatan ditetapkan.** Tidak ada cara bayar yang terpilih otomatis.
   Untuk penjamin asuransi atau perusahaan, petugas memilih satu kartu penjamin milik pasien;
   kartu yang tidak layak tampil sebagai **Tidak Layak** dan tidak dapat dipilih.
5. **Unit layanan dan DPJP ditetapkan.** Sebelum tombol simpan ditekan, layar menampilkan
   peringatan **"Langkah ini tidak dapat dimundurkan"**. Menekan **Simpan & Cari Tempat Tidur**
   adalah **titik tulis 1**: kunjungan terbentuk beserta penjaminnya, lalu episode `Draft`
   terbentuk berjangkar pada kunjungan itu. Setelah ini penjamin tidak dapat diubah lagi.
6. **Tempat tidur dipilih lalu dipesan.** Daftar tempat tidur datang apa adanya dari server;
   layar tidak menyaring ulang. Menekan **Pesan Tempat Tidur** lalu **Ya, Pesan** adalah
   **titik tulis 2**. Sisa waktu pemesanan terbaca di layar, dan layar menyatakan dengan jelas
   bahwa pasien **belum** menjadi Sedang Dirawat.
7. **Admisi dikunci.** Layar Konfirmasi menyatakan bahwa mengunci admisi **tidak** menempatkan
   pasien. Menekan **Kunci Admisi & Cetak Persetujuan** adalah **titik tulis 3**.
8. **Persetujuan dicetak**, lalu kartu pasien pada jalur pasien baru.
9. **Kedatangan dikonfirmasi di Papan Tempat Tidur.** Barulah episode menjadi `Admitted`.
10. **Keputusan pulang, kelayakan keuangan, dan penutupan** dikerjakan pada layar masing-masing
    sampai episode berstatus `Closed`.

### 2.2 Jalur tidak normal yang ikut dibuktikan

- **Alur ditinggalkan di tengah.** Bila petugas menutup alurnya setelah titik tulis 1, episode
  `Draft` sudah terbentuk di server. Episode itu dapat ditemukan kembali dari Daftar Kerja
  Episode lewat penyaring `Draft`, lalu dilanjutkan. Pelanjutan **tidak** membuat kunjungan
  maupun episode kedua untuk pasien yang sama.
- **Pelanjutan tanpa pemesanan aktif.** Layar menyatakan bahwa episode sedang tidak memegang
  pemesanan tempat tidur dan meminta petugas memilih ulang — tanpa mengklaim pemesanan
  sebelumnya "sudah gugur", karena kontrak baca hari ini tidak dapat membedakan pemesanan yang
  lewat batas dari yang tidak pernah dibuat.
- **Riwayat kelayakan keuangan tidak dapat dibaca.** Bila akun petugas belum punya hak akses
  `ReadFinancialClearance`, layar menyatakan keterbatasannya apa adanya dan **tetap** mengizinkan
  penandaan, karena hak menandai dan hak membaca adalah dua butir yang terpisah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Dokumen blueprint dan tata kelola:**

- `roadmap/frontend-roadmap.md` — kartu `FE-RWI-035`, `036` s.d. `041`, dan tabel gerbang bagian 6
- `roadmap/archive/revision-2/frontend-roadmap.md` — kartu `FE-RWI-003`, `004`, `012`, `013`
- `roadmap/requirement-traceability.md`
- `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`
- `00-interview-decisions.md` — status `RWI-DEC-059`
- `05-skema-tampilan.md` bagian 23 — inventaris 19 layar
- `AGENTS.md` frontend; `rules/frontend/` — arsitektur, test policy, checklist konsistensi UI, template laporan

**Source backend (read-only):**

- `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs`
- `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs`

**Source frontend:**

- Seluruh `src/components/view/health-services/inpatient-management/inpatient-admission-*.jsx`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-*.jsx`
- `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`
- `src/lib/services/health-services/inpatient-management/*.js`
- `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx`,
  `inpatient-closure-utils.jsx`, `inpatient-financial-clearance-utils.jsx`,
  `inpatient-admission-payment-utils.jsx`
- `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx`
- `src/components/features/base-features/base-checkbox-card.jsx`, `confirm-modal.jsx`,
  `access-denied-gate.jsx`, `filter-select.jsx`
- `src/style/components/features/footer.css`, `base-checkbox-card.module.css`
- Seluruh 17 berkas `tests/e2e/*.spec.mjs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `tests/e2e/inpatient-admission-flow.spec.mjs` | **Baru.** Empat kasus e2e untuk kriteria 1 s.d. 4, memakai satu peladen tiruan yang **menyimpan keadaan** sehingga episode yang dibuat langkah Dokter adalah episode yang sama yang dipesankan tempat tidur, dikonfirmasi kedatangannya, lalu ditutup |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-financial-clearance.jsx` | Menambahkan pembacaan `GET .../financial-clearance` saat halaman dimuat. Penolakan `401`/`403` ditangkap terpisah dari `loadError` supaya halaman **tidak** berubah menjadi Akses Ditolak seluruhnya; dua keadaan baru `clearanceReadDenied` dan `clearanceReadFailed` diekspor |
| `src/lib/constants/health-services/inpatient-management/inpatient-financial-clearance-constants.jsx` | `HISTORY_NOT_READABLE` dan `STATUS_NOT_READABLE` diubah sebabnya — dari "kontrak belum menyediakan endpoint" menjadi "hak akses baca belum dimiliki". Ditambah `HISTORY_READ_FAILED` untuk kegagalan pembacaan |
| `src/components/view/health-services/inpatient-management/inpatient-financial-clearance-view.jsx` | Riwayat kini punya **tiga** keadaan berbeda, bukan dua: ditolak hak akses, gagal dibaca, dan memang belum pernah ditandai |
| `src/utils/health-services/inpatient-management/inpatient-financial-clearance-utils.jsx` | Comment `readFinancialConditionFromReadiness` diperbarui: fungsi itu kini **cadangan**, bukan satu-satunya jalan baca |
| `tests/unit/inpatient-financial-clearance.test.mjs` | Test "riwayat yang belum terbaca dibedakan dari riwayat yang memang kosong" diperluas dari dua keadaan menjadi tiga, dan ditambah pemeriksaan bahwa hook benar-benar memanggil endpoint bacanya |
| `tests/e2e/inpatient-financial-clearance.spec.mjs` | Peladen tiruannya diajari menjawab `GET .../financial-clearance` yang sebelumnya tidak ada, dengan bawaan `403` — keadaan kasir yang hanya berwenang menandai. Ditambah **tiga kasus baru** yang membuktikan kemampuan baca barunya: riwayat terbaca saat halaman dibuka, penolakan hak akses tidak mencabut hak menandai, dan pembacaan yang gagal dibedakan dari yang ditolak |
| `tests/e2e/inpatient-episode-worklist.spec.mjs` | Satu selector usang diperbaiki. Sejak `FE-RWI-033` menambahkan menu operasional dan judul wilayah tabel, kalimat "Daftar Kerja Episode" muncul **tiga kali** pada halaman itu, sehingga pencarian tanpa tingkat judul mengenai lebih dari satu elemen dan melanggar strict mode. Hanya selector-nya yang diubah; tidak satu pun assertion perilaku disentuh |
| `tests/e2e/inpatient-dashboard.spec.mjs` | Selector usang yang sama jenisnya diperbaiki pada satu baris |

### 3.3 Kepatuhan arsitektur frontend

- **Tidak ada arsitektur baru.** Pembacaan baru memakai `inpatientDischargeService` yang sudah
  ada, yang dibangun `createInpatientApiService` di atas `InstanceAxios` — tidak ada instance
  Axios kedua, tidak ada service baru, tidak ada slice Redux baru.
- **Alur dependensi tetap** `view → hook → service → InstanceAxios`. Keadaan baru
  (`clearanceReadDenied`, `clearanceReadFailed`) tinggal di hook, bukan di komponen.
- **Tidak ada base component baru.** Gerbang keputusan base component dijalankan; seluruh
  elemen berstatus `REUSE` — lihat bagian 3.4.
- **Tidak ada berkas style yang diubah.**
- Berkas e2e baru mengikuti pola 17 spec tetangganya: `createSessionAssertion` untuk memasang
  cookie sesi, `page.route("**/v1/**")` untuk memalsukan API, dan tidak menambah framework test
  apa pun.

### 3.4 Gerbang keputusan base component

Task ini menyentuh tampilan pada satu layar saja — Kelayakan Keuangan (`FE-INP-08`). Modul
referensi visualnya adalah layar itu sendiri, yang sudah berdiri sejak `FE-RWI-013`.

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kalimat riwayat gagal dibaca | `REUSE` | `<p className="small text-secondary mb-0">` — bentuk yang sama persis dengan kalimat `financial-history-unavailable` di sebelahnya, hanya penanda dan isinya yang berbeda |
| Kalimat riwayat ditolak hak akses | `REUSE` | Elemen existing, hanya syarat tampilnya yang dipersempit |
| Kalimat keterbatasan nilai terkini | `REUSE` | Elemen existing, hanya syarat tampilnya yang dipersempit |
| Seluruh tombol, badge, alert, form field | `REUSE` | Tidak disentuh sama sekali |

`UI GATE: TIDAK ADA ELEMEN NON-REUSE.` Karena tidak ada elemen berstatus `EXTEND`, `COMPOSE`,
`WRAP`, maupun `NEW`, tidak ada pilihan bernomor yang perlu diajukan dan tidak ada keputusan
user yang ditunggu.

---

## 4. State yang ditangani di layar

Layar yang tampilannya berubah hanya Kelayakan Keuangan.

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kalimat "Memuat kelayakan keuangan episode..." pada wilayah isi; tidak ada layar kosong |
| Kosong | Bila pembacaan berhasil dan memang belum pernah ada penandaan: "Belum ada penandaan yang tercatat pada episode ini." |
| Gagal | Bila pembacaan riwayat gagal bukan karena hak akses: "Riwayat penandaan gagal dibaca dari server. Muat ulang halaman ini untuk mencobanya lagi." Halaman selebihnya tetap utuh dan penandaan tetap dapat dikirim |
| Tanpa hak akses | Bila `GET .../financial-clearance` ditolak `401`/`403`: "Riwayat penandaan sebelumnya tidak dapat dibaca dari layar ini, karena akun Anda belum memiliki hak akses baca kelayakan keuangan. Riwayat tetap tampil setelah ada penandaan baru dikirim dari layar ini." Halaman **tidak** berubah menjadi Akses Ditolak, karena hak menandai dan hak membaca adalah butir terpisah |

Keadaan tanpa hak akses pada tingkat halaman tetap dipegang `AccessDeniedGate` seperti sebelumnya,
dan itu hanya menyala bila pembacaan detail episode atau `closure-readiness` yang ditolak.

---

## 5. Endpoint yang dikonsumsi

Task ini menambah **satu** pemanggilan baru. Sisanya sudah dipanggil layar-layar yang ada dan
hanya dilewati oleh e2e.

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/discharges/{episodeId}/financial-clearance` | **Baru dipakai frontend.** Membaca nilai terkini kelayakan keuangan beserta seluruh riwayat penandaannya saat halaman dimuat | `InpatientDischarge : ReadFinancialClearance` |
| `POST` | `/v1/health-services/inpatient-management/discharges/{episodeId}/financial-clearance` | Menandai kelayakan keuangan beserta catatannya | `InpatientDischarge : MarkFinancialClearance` |
| `GET` | `/v1/health-services/inpatient-management/discharges/{episodeId}/closure-readiness` | Membaca kelima syarat penutupan | `InpatientDischarge : Read` |
| `POST` | `/v1/health-services/inpatient-management/discharges/{episodeId}/decide` | Keputusan pulang | `InpatientDischarge : Update` |
| `POST` | `/v1/health-services/inpatient-management/discharges/{episodeId}/close` | Menutup episode | `InpatientDischarge : Close` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/inpatient-management/episodes` | Titik tulis 1 bagian kedua — episode `Draft` | `InpatientEpisode : Create` |
| `GET` | `/v1/health-services/inpatient-management/episodes/{id}` | Detail episode | `InpatientEpisode : Read` |
| `GET` | `/v1/health-services/inpatient-management/episodes` | Daftar kerja episode | `InpatientEpisode : Read` |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-occupancies/available-beds` | Tempat tidur yang lolos kelayakan | `InpatientBedOccupancy : Read` |
| `GET` | `/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Papan beserta metadata pemesanan | `InpatientBedOccupancy : Read` |
| `POST` | `/v1/health-services/inpatient-management/bed-occupancies/reservations` | Titik tulis 2 — pemesanan tempat tidur | `InpatientBedOccupancy : Create` |
| `POST` | `/v1/health-services/inpatient-management/bed-occupancies/placements` | Konfirmasi kedatangan pasien | `InpatientBedOccupancy : Create` |

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/patient-encounters/admin` | Titik tulis 1 bagian pertama — kunjungan beserta penjaminnya | `PatientEncounter : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa keluaran, berarti `0 errors` | `PASS` | Keluaran perintah kosong |
| `npm run build` | `✓ Compiled successfully`, lalu `postbuild` menyiapkan `.next/standalone` | `PASS` | Keluaran perintah, exit code `0` |
| `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | 292 test, **291 lulus, 1 gagal** | `EXISTING / ENVIRONMENT ISSUE` | Satu-satunya kegagalan `tests/unit/auth-security.test.mjs`, karena mengimpor `src/utils/auth/base-login-utils.jsx` yang **tidak ada di repository**. Tidak berkaitan dengan task ini |
| `node --import ./tests/helpers/register.mjs --test "tests/unit/inpatient-financial-clearance.test.mjs"` | 17 test, **17 lulus** | `PASS` | Keluaran perintah |
| e2e `tests/e2e/inpatient-admission-flow.spec.mjs` | **4 dari 4 lulus**, dijalankan dua kali berturut-turut untuk memastikan tidak flaky (15,2 detik dan 15,0 detik) | `PASS` | Keluaran `npx playwright test` |
| e2e `tests/e2e/inpatient-bed-drift.spec.mjs` | Gagal pada `bed-drift-admin-state-notice` | `EXISTING / ENVIRONMENT ISSUE` | Penanda itu tidak ada di `src/` mana pun; layar Selisih Tempat Tidur belum menerima repair `FE-RWI-039` yang berstatus ⛔ `BLOCKED` |
| Rangkaian e2e penuh `npx playwright test` | **119 lulus, 10 gagal** dalam 9,6 menit. Turun dari 21 gagal sebelum perbaikan berkas e2e pada task ini | `EXISTING / ENVIRONMENT ISSUE` | Rincian per kegagalan beserta pemiliknya ada di bagian 6.2 |

**AUTOMATED TEST:** `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` — `FAIL (1 dari 292, pre-existing: auth-security.test.mjs mengimpor berkas yang tidak ada)`

**MANUAL TEST:** `NOT FEASIBLE`. Verifikasi manual di peramban terhadap data sungguhan terhalang
`RWI-UI-GAP-007`: environment target belum memiliki unit layanan bertipe rawat inap, kamar,
tempat tidur, kelas perawatan, penjamin, baris pengaturan `DEFAULT`, maupun butir administrasi
awal. Tanpa data itu, alur admisi tidak dapat dijalankan seorang pun sampai selesai. Sebagai
gantinya seluruh kontrol interaktif yang disentuh dijalankan lewat e2e di peramban Edge
sungguhan — bukan simulasi DOM — dan payload yang benar-benar terkirim diperiksa satu per satu.

### 6.1 Batas yang harus dibaca sebelum mempercayai e2e ini

Jawaban server pada e2e ini dipalsukan `page.route`, mengikuti pola 17 spec e2e yang sudah ada.
Yang dibuktikan karena itu adalah **perilaku frontend**: urutan langkah, isi payload yang
terkirim, penjaga yang tampil, dan keadaan yang dirender.

Ia **bukan** bukti runtime terhadap data master sungguhan, dan **tidak menutup**
`RWI-UI-GAP-007`. Batas ini ditulis juga sebagai comment kepala berkas spec-nya supaya pembaca
berikutnya tidak salah membacanya sebagai penutupan gap.

### 6.2 Rangkaian e2e penuh

Rangkaian penuh dijalankan `npx playwright test` terhadap server standalone hasil `npm run build`
pada `127.0.0.1:3710`, memakai peramban Edge sistem. Repository ini **tidak memiliki**
`playwright.config.*`, sehingga config sementara dibuat untuk menjalankannya lalu dihapus lagi
agar tidak ikut masuk diff — lihat bagian 8 baris **Perubahan sampingan**.

Hasil akhirnya, dicatat apa adanya dan tanpa satu pun kasus ditandai dilewati:

```text
  10 failed
  119 passed (9.6m)
```

Sebelum perbaikan pada berkas e2e yang dijelaskan di bawah, jalannya berbunyi **21 gagal**.
Sebelas di antaranya ditutup task ini; sepuluh sisanya dibiarkan apa adanya karena pemiliknya
task lain.

**Kegagalan yang benar-benar disebabkan task ini — dan sudah diperbaiki:**

| Kasus | Sebab | Penyelesaian |
| --- | --- | --- |
| 3 kasus pada `inpatient-financial-clearance.spec.mjs` | Regresi nyata dari perubahan task ini. Peladen tiruan spec itu ditulis ketika `GET .../financial-clearance` **belum ada**, sehingga permintaan baru jatuh ke jawaban penampung yang berisi halaman kosong. Akibatnya layar membaca `currentStatus = 0` dan menampilkan **"Belum diperiksa"**, sedangkan spec masih menunggu kalimat lama **"Belum lunas"** | Peladen tiruan spec diajari menjawab endpoint itu, dengan bawaan `403` — keadaan kasir yang hanya berwenang menandai, yang justru mempertahankan arti asli ketiga kasus itu. Ditambah **tiga kasus baru** yang membuktikan kemampuan bacanya. Spec kini **15 dari 15 lulus** |

Perlu dicatat bahwa perbedaan kalimat itu **bukan kemunduran**: "Belum diperiksa" adalah nilai
yang sebenarnya, dan kemampuan membedakannya dari "Tertahan" persis itulah yang selama ini
ditahan `RWI-UI-GAP-004`. Yang usang adalah harapan spec-nya, bukan perilaku layarnya.

**Kegagalan yang berupa selector usang, bukan cacat produk — dan sudah diperbaiki:**

| Kasus | Sebab | Penyelesaian |
| --- | --- | --- |
| 8 kasus pada `inpatient-episode-worklist.spec.mjs` | Sejak `FE-RWI-033` menambahkan menu operasional dan judul wilayah tabel, kalimat "Daftar Kerja Episode" muncul **tiga kali** pada halaman itu. Pencarian tanpa tingkat judul karena itu mengenai lebih dari satu elemen dan melanggar strict mode Playwright | Pencariannya menyebut `level: 1`. Spec kini **10 dari 10 lulus** |
| 1 pelanggaran strict mode pada `inpatient-dashboard.spec.mjs` | Sebab yang sama | Perbaikan yang sama. **Perlu dicatat: ini belum membuat spec-nya lulus** — pelanggaran strict mode-nya hilang, tetapi ketiga kasus pada berkas itu tetap gagal karena sebab lain yang bukan milik task ini, lihat tabel berikutnya |

**Kegagalan yang dibiarkan apa adanya, beserta pemiliknya:**

| Kasus | Sebab | Pemilik |
| --- | --- | --- |
| 1 kasus pada `inpatient-bed-drift.spec.mjs` | `bed-drift-admin-state-notice` tidak ada di `src/` mana pun. Layar Selisih Tempat Tidur belum menerima repair `FE-RWI-039` yang berstatus ⛔ `BLOCKED` | `FE-RWI-039`. **Sengaja tidak ditambal di sini** — roadmap melarangnya |
| 2 kasus pada `inpatient-clearance-item.spec.mjs` | Daftar butir administrasi tampil **kosong** padahal peladen tiruan mengirim barisnya; hook hasil penulisan ulang `FE-RWI-040` tampaknya tidak membaca bentuk jawaban yang dipakai spec-nya sendiri | `FE-RWI-040`, yang source-nya masih berupa perubahan lokal belum di-commit di working tree |
| 1 kasus pada `inpatient-census.spec.mjs` | Penyaring bernama `Unit Layanan` tidak ditemukan; label penyaringnya berubah ketika `FE-RWI-037` menyusun ulang layar census | `FE-RWI-037` |
| 3 kasus pada `inpatient-dashboard.spec.mjs` | Penanda `dashboard-census-service-unit-<id>` tidak ada; angka `dashboard-monitoring-pending-closures` terbaca `0` padahal server mengirim `2`; dan parameter penyaring yang diharapkan pada navigasi ke daftar kerja terbaca `undefined`. Ketiganya soal bentuk widget dan navigasi beranda, bukan selector | `FE-RWI-021` bersama `FE-RWI-033` |
| 2 kasus pada `auth-security.spec.mjs` | Di luar modul Rawat Inap | Di luar cakupan task ini |
| 1 kasus pada `route-smoke.spec.mjs` | `expect(applicationRoutes).toHaveLength(219)` sementara repository kini memiliki **483** route. Karena assertion jumlah ini dijalankan lebih dulu, pemeriksaan "tidak ada halaman yang crash atau blank" pada baris sesudahnya **tidak sempat berjalan**, sehingga hasilnya tidak dapat diklaim | Di luar modul Rawat Inap |

Keempat kegagalan pada baris kedua sampai kelima **tidak diperbaiki task ini**. Ketiganya menyangkut
source milik task lain yang sudah ditandai selesai, dan memperbaikinya dari sini akan
menyembunyikan temuan yang justru berguna: **tiga dari enam layar repair belum benar-benar
terbukti berjalan lewat e2e-nya sendiri.**

### 6.3 Checklist konsistensi UI dan grep anti-regresi

Dijalankan pada berkas yang diubah, bukan seluruh repo.

| Grep | Hasil pada baris yang **ditambahkan** task ini |
| --- | --- |
| Warna literal di stylesheet baru | `NOT APPLICABLE` — tidak ada berkas style yang diubah |
| Typography menimpa komponen shared | Bersih |
| Tombol non-base | Bersih |
| Tabel mentah | Bersih |
| Utility typography di dalam blok tabel | Bersih |
| `!important` baru | Bersih |

Dua temuan muncul bila grep dijalankan pada **seluruh isi** berkas view, dan keduanya
**pre-existing**, bukan tambahan task ini: `className="btn btn-outline-secondary btn-sm"` pada
baris 139 dan tiga pemakaian `fw-semibold`. Keduanya sudah ada sejak `FE-RWI-013` dan sengaja
tidak disentuh, mengikuti aturan cakupan perubahan `AGENTS.md`.

**Tidak dijalankan:** `npm run test:uat` — tidak diminta task dan `AGENTS.md` melarangnya tanpa
permintaan eksplisit.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Satu e2e menjalankan `FLOW-RI-MVP-001` jalur pasien baru dari langkah 1 sampai episode `Closed` | 🟡 **Sebagian** | Test `kriteria 1` lulus dan benar-benar merangkai satu episode dari alur admisi → titik tulis 1, 2, 3 → konfirmasi kedatangan di papan → kelayakan keuangan → penutupan, dengan `state.episodeStatus` berakhir `CLOSED`. **Yang tidak dikendarai** adalah formulir pendaftaran pasien baru pada langkah 2, karena ia memakai `FilterDatePicker` — kalender tanpa input tanggal biasa — dan lima pilihan wilayah berantai milik pendaftaran IGD. Urutan langkah 1 → langkah 2 tetap dibuktikan; isi formulirnya dipegang verifikasi `FE-RWI-023` |
| 2. Satu e2e menjalankan jalur pasien lama sampai tempat tidur `Reserved` | ✅ **Terpenuhi** | Test `kriteria 2` lulus: pencarian pasien, tinjauan identitas, tipe pasien, pembayaran, titik tulis 1, pemilihan tempat tidur, lalu `POST /reservations` dengan `{ episodeId, bedId }` yang benar. Papan kemudian membaca baris itu sebagai **Dipesan** oleh nomor episode yang sama |
| 3. Kunjungan yang terbentuk terbukti membawa penjamin yang dipilih, bukan tunai bawaan | ✅ **Terpenuhi** | Test `kriteria 3` lulus: ketiga kartu cara bayar terbukti **tidak** ada yang tercentang saat langkah dibuka; setelah Penjamin Perusahaan dipilih, `POST /patient-encounters/admin` terkirim dengan `paymentType = 3` (`COMPANY_GUARANTOR`), bukan `1` (`CASH`), dan membawa id kartu penjamin pasien. Episode yang menyusul berjangkar pada `encounterId` yang sama |
| 4. Alur yang ditinggal setelah titik tulis 1 terbukti dapat ditemukan kembali dan dilanjutkan | ✅ **Terpenuhi** | Test `kriteria 4` lulus: episode `Draft` ditemukan pada Daftar Kerja Episode lewat penyaring status, lalu dilanjutkan lewat route pelanjutan. Nol `POST /patient-encounters/admin` dan nol `POST /episodes` terkirim selama pelanjutan — pelanjutan tidak membuat kunjungan maupun episode kedua |
| 5. Setiap layar dari kesembilan belas terbukti tertutup bagi peran yang tidak berhak | 🟡 **Sebagian** | **8 dari 19 layar** punya e2e gerbang peran: `FE-INP-06` keputusan pulang, `FE-INP-07` penutupan, `FE-INP-08` kelayakan keuangan, `FE-INP-09` daftar pantau, `FE-INP-10` selisih tempat tidur, `FE-INP-11` sesi koreksi, `FE-INP-14` pencatatan kepergian, `FE-INP-16` daftar kerja episode. Sebelas sisanya **tidak punya penjaga peran di sisi layar untuk dibuktikan**, dan sebabnya struktural: frontend belum menerima katalog hak akses per butir — cacat yang sama yang menahan `FE-RWI-003` kriteria 2. Yang mengunci sebelas layar itu server, lewat `AccessDeniedGate` ketika permintaannya ditolak `403` |
| 6. Keempat aturan penjaga `GUARD-INP-01` s.d. `GUARD-INP-04` terbukti terlihat di layar | ✅ **Terpenuhi** | Sudah dimiliki e2e yang ada, bukan ditulis ulang di sini: `GUARD-INP-01` oleh `inpatient-episode-detail.spec.mjs` "tombol pindah nonaktif bagi dokter yang bukan DPJP episode ini"; `GUARD-INP-02` dan `03` oleh `inpatient-discharge.spec.mjs` "kriteria 1: <peran> tidak melihat satu pun aksi pulang maupun resume" beserta pasangannya untuk DPJP aktif; `GUARD-INP-04` oleh `inpatient-episode-detail.spec.mjs` "kewenangan tombol kebutuhan isolasi" |
| 7. Empat kriteria yang tertahan sejak revision 2 diselesaikan atau dinyatakan tertahan beserta alasannya yang masih berlaku | ✅ **Terpenuhi** | Keempatnya diperiksa ulang terhadap source dan kontrak hari ini — lihat bagian 1.3. Tiga **tetap tertahan dengan alasan yang masih berlaku dan buktinya diverifikasi ulang**; satu (`FE-RWI-013` kriteria 3) alasannya sudah gugur dan karena itu **diselesaikan**, bukan dinyatakan tertahan lagi |
| 8. Keenam layar bukti runtime mempunyai state berisi/kosong/gagal dan aksi sesuai `FE-RWI-036` s.d. `041` | ❌ **Belum terpenuhi — dan temuannya lebih luas dari yang diperkirakan roadmap** | **Tiga dari enam layar** belum terbukti berjalan lewat e2e-nya sendiri: **(a)** Selisih Tempat Tidur `FE-RWI-039` ⛔ `BLOCKED`, penanda `bed-drift-admin-state-notice` memang belum ada di source; **(b)** Butir Administrasi `FE-RWI-040` ✅ ditandai selesai, tetapi daftarnya tampil **kosong** padahal peladen tiruan mengirim barisnya; **(c)** Census `FE-RWI-037` ✅ ditandai selesai, tetapi penyaring `Unit Layanan` yang dicari spec-nya tidak ada lagi. Tiga sisanya — Papan `FE-RWI-036`, Daftar Pantau `FE-RWI-038`, Pengaturan `FE-RWI-041` — lulus e2e-nya. `FE-RWI-035` dilarang roadmap menambal repair yang belum dikerjakan, sehingga ketiganya dilaporkan, bukan diperbaiki di sini |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Kedelapan kriteria lulus | ❌ **Belum.** 5 terpenuhi, 2 sebagian, 1 belum |
| Keluaran rangkaian e2e terlampir | 🟡 Keluaran berkas e2e baru terlampir dan lulus 4/4; keluaran rangkaian penuh ada di bagian 6.2 |
| Bukti enam layar terlampir | 🟡 Lima dari enam; layar keenam tertahan `FE-RWI-039` |

### Gerbang skema

Kartu task menuntut `RWI-UI-GAP-001` s.d. `007` **tertutup atau dinyatakan tertahan dengan owner
dan bukti yang masih berlaku**, dan melarang e2e menyamarkannya dengan mock. Keadaannya
diperiksa ulang hari ini:

| Gap | Keadaan setelah task ini | Owner |
| --- | --- | --- |
| `RWI-UI-GAP-001` jumlah langkah pasien lama | **Tertahan.** Kartu revision `3` menulis delapan langkah; `INPATIENT_ADMISSION_EXISTING_PATIENT_STEPS` di source berisi **sembilan**, dengan Cetak Persetujuan sebagai langkah kesembilan. Salah satu dokumen perlu dikoreksi | Pemilik skema tampilan |
| ~~`RWI-UI-GAP-002`~~ | ✅ **Tertutup**, dan kini juga terbukti di peramban lewat kriteria 3 | — |
| ~~`RWI-UI-GAP-003`~~ | ✅ **Tertutup**, dan kini juga terbukti di peramban lewat kriteria 2 dan 4 | — |
| `RWI-UI-GAP-004` baca kelayakan keuangan | ✅ **Tertutup task ini.** Endpoint bacanya dibuka `BE-RWI-034`; frontend kini memanggilnya | — |
| `RWI-UI-GAP-005` baca sesi koreksi | **Tertahan.** `contracts/api-contract.md` dan `InpatientEpisodeController.cs` sama-sama hanya punya `POST` membuka sesi dan `PATCH` menutupnya; tidak ada operasi baca. Memuat ulang halaman tidak memulihkan sesi yang terbuka | Backend/API |
| ~~`RWI-UI-GAP-006`~~ | ✅ **Tertutup** pada level kontrak/source | — |
| `RWI-UI-GAP-007` data master/runtime belum layak | **Tertahan, dan ini yang menahan bukti runtime task ini.** e2e di sini memakai mock dan menyatakan batas itu di kepala berkasnya, bukan menyamarkannya | Admin Master Data/Tim Master Data |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint:errors` bersih. `npm run lint` penuh masih memunculkan warning garis dasar repository, tidak satu pun pada berkas task ini |
| Masalah yang diketahui | **Tiga, dan ketiganya milik task lain.** Pertama, `inpatient-bed-drift.spec.mjs` gagal pada `bed-drift-admin-state-notice` — layar Selisih Tempat Tidur belum menerima repair `FE-RWI-039` yang ⛔ `BLOCKED`. Kedua, `inpatient-clearance-item.spec.mjs` gagal karena daftar butir administrasi tampil kosong padahal peladen tiruan mengirim barisnya; ini menyangkut source `FE-RWI-040` yang masih berupa perubahan lokal belum di-commit. Ketiga, `inpatient-census.spec.mjs` gagal karena label penyaring berubah ketika `FE-RWI-037` menyusun ulang layarnya. Ketiganya `UNRELATED EXISTING ISSUE` dan sengaja **tidak** diperbaiki di sini. **Dua di antaranya berarti task yang sudah ditandai ✅ belum benar-benar terbukti berjalan** — itu temuan yang perlu diputuskan pemilik pekerjaan, bukan ditutup diam-diam |
| Dependency backend | `RWI-UI-GAP-005` — tidak ada operasi baca sesi koreksi, sehingga memuat ulang halaman tidak memulihkan sesi yang terbuka. `RWI-DEC-059` masih `draft`, sehingga dua cara pulang tetap di luar MVP. Keduanya tidak diselesaikan task ini karena keduanya wewenang backend dan pemilik klinis |
| Perubahan sampingan | Dua, keduanya berupa keluaran Playwright dan **tidak satu pun menyentuh source aplikasi**. Pertama, `playwright.config.mjs` sementara yang dibuat untuk menjalankan e2e — repository ini memang tidak punya `playwright.config.*` — **sudah dihapus** supaya tidak ikut masuk diff. Kedua, `test-results/.last-run.json` — berkas tracked milik repository — terbarui isinya karena rangkaian e2e memang dijalankan. Ia **sengaja dibiarkan apa adanya**: itu keluaran jalan yang task ini diminta melakukannya, bukan perubahan yang tidak disengaja. Direktori artefak kegagalan di bawah `test-results/` juga dibiarkan; seluruhnya untracked dan dikelola Playwright sendiri |
| Interupsi | Satu. Sesi terputus saat rangkaian e2e penuh berjalan di latar belakang. Pemulihannya: memeriksa `git status --short` lebih dulu, memastikan server uji pada port `3710` masih hidup, lalu melanjutkan dari kondisi terverifikasi terakhir — bukan mengulang seluruh pekerjaan |
| Status Git | Lihat bagian 8.1. Tidak ada `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun perpindahan branch yang dijalankan; seluruh perubahan tetap lokal |
| Langkah berikutnya | **Putuskan dulu dua temuan pada `FE-RWI-037` dan `FE-RWI-040`** — keduanya ditandai ✅ tetapi e2e-nya tidak lulus, dan itu perlu diputuskan pemilik pekerjaan sebelum modul dinyatakan siap. Setelah itu **kerjakan `FE-RWI-039`.** Ia satu-satunya dependency `FE-RWI-035` yang belum selesai, dan satu-satunya yang menahan kriteria 8. Statusnya ⛔ `BLOCKED` menunggu approval skema/roadmap revision 5, jadi langkah nyatanya adalah meminta approval itu kepada pemilik pekerjaan. Setelah `FE-RWI-039` selesai, `FE-RWI-035` cukup dijalankan ulang — kriteria 8 akan tertutup tanpa perubahan source tambahan |

### 8.1 Status Git

```text
 M src/components/view/health-services/inpatient-management/inpatient-financial-clearance-view.jsx
 M src/components/view/health-services/inpatient-management/inpatient-setting-view.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-financial-clearance-constants.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-clearance-item-detail.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-clearance-item-editor.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-clearance-items.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-financial-clearance.jsx
 M src/utils/health-services/inpatient-management/inpatient-clearance-item-utils.jsx
 M src/utils/health-services/inpatient-management/inpatient-financial-clearance-utils.jsx
 M test-results/.last-run.json
 M tests/e2e/inpatient-dashboard.spec.mjs
 M tests/e2e/inpatient-episode-worklist.spec.mjs
 M tests/e2e/inpatient-financial-clearance.spec.mjs
 M tests/unit/inpatient-clearance-item.test.mjs
 M tests/unit/inpatient-financial-clearance.test.mjs
 M tests/unit/inpatient-setting.test.mjs
?? tests/e2e/inpatient-admission-flow.spec.mjs
```

Pada repository backend, yang berubah **hanya** ketiga berkas dokumentasi yang memang diberi
wewenang task ini:

```text
 M docs/module-blueprints/rawat-inap/roadmap/frontend-roadmap.md
 M docs/module-blueprints/rawat-inap/roadmap/requirement-traceability.md
?? docs/module-blueprints/rawat-inap/task/report/frontend/FE-RWI-035.md
```

**Dua perubahan lain terlihat pada working tree backend dan bukan hasil task ini:**
`docs/Modul-RS/PRD-Modul-Rawat-Inap.md` terhapus dan `docs/Modul-RS/Rawat-Inap/` muncul sebagai
folder baru. Keduanya belum ada ketika task ini dimulai, berada di luar `docs/module-blueprints/`,
dan **sengaja tidak disentuh** — tidak diperiksa isinya, tidak dipulihkan, tidak dihapus. Bila itu
bukan pekerjaan pemilik repository, sebaiknya diperiksa sebelum apa pun di-commit.

Enam berkas pertama yang **tidak** disebut bagian 3.2 — `inpatient-setting-view.jsx`, ketiga
hook `clearance-item`, `inpatient-clearance-item-utils.jsx`, serta test unit
`inpatient-clearance-item` dan `inpatient-setting` — adalah pekerjaan `FE-RWI-040` dan
`FE-RWI-041` yang sudah ada di working tree sebelum task ini dimulai. Keduanya **tidak
disentuh** task ini dan sengaja dibiarkan apa adanya.
