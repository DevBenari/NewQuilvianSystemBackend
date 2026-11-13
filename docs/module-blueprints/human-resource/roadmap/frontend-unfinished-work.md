# Human Resource — Pekerjaan Frontend yang Belum Selesai dan Rencana Gelombang

| Field | Nilai |
|---|---|
| Jenis dokumen | Daftar kerja hasil audit beserta rencana perbaikan. **Bukan** pengganti roadmap. Status resmi task tetap di [frontend-roadmap.md](frontend-roadmap.md) |
| Dasar | Audit `FE-HRD-001` s.d. `FE-HRD-305` terhadap source, 11 November 2026 |
| Verdict audit | **`NOT_READY`** |
| `blueprint_id` / revision / status | `HRD-BP-001` / `59` / `DRAFT` |
| Roadmap yang diaudit | `frontend-roadmap.md` revisi `16`, commit `c0a2588e` |
| Source frontend | `d09dc74da510f9bb6f59aa6062c77cff44a5c518` (branch `HamzahV2`) |
| Source backend | `f751f8cac4e477f216d3ccb444a1fae8a052b3aa` (branch `MHamzah`) |
| Pembaca | Pemilik modul HR dan developer frontend HR |
| Cara audit | Membaca kode. `npm run build`, `npm run lint`, dan uji browser **belum dijalankan** |

Manifest masih mencatat `frontend_source_sha: 44990b3d3`, sedangkan source yang diaudit sudah lebih baru.
Bukti pada manifest untuk source frontend karena itu berstatus `STALE` (sudah tertinggal).

---

## 1. Ringkasan

Roadmap memuat **305 task** dan menandai **268** di antaranya selesai. Audit menemukan **137 dari 268**
task itu belum benar-benar berfungsi. Halamannya bisa dibuka dan build-nya bisa lulus, tetapi datanya
contoh, atau komponennya tidak terpasang di layar mana pun.

| Kondisi di kode | Jumlah | Slice atau task |
|---|---:|---|
| Tersambung ke API | 81 | Terutama `S-A1` s.d. `S-A7`, `S-B2`, `S-B4`, `S-C4-ONB`, `S-D2` |
| Pindah ke modul `/payroll` | 9 | `S-B5`; kesetaraan fungsinya belum diuji |
| Non-layar, verifikasi, navigasi, tanpa laporan | 41 | Tidak dinilai sebagai layar |
| **Hanya tampilan, datanya contoh di kode** | **81** | `S-B3`, `S-C1`, `S-C1-OPPE`, `S-D4`, `S-A7-V2`, `S-D5`, sebagian `S-D3` dan `S-C6` |
| **Menampilkan data contoh saat API gagal** | **49** | `S-C2`, `S-C3`, `S-C5`, `S-C6` |
| **Tidak bisa dibuka dari layar mana pun** | **7** | `FE-HRD-135`, `142`, `152`, `153`, `155`, `196`, `197` |
| Belum dikerjakan | 29 | `S-D1` (`029`–`043`), `S-B1` (`088`–`101`) |
| Sebagian | 6 | `011`, `015`, `016`, `018`, `026`, `027` |
| Terblokir | 2 | `014`, `019` |
| **Total** | **305** | |

Pemakaian base component juga jauh dari standar. Hanya **39 dari 268 berkas UI** di luar
master data yang bersih, dan **97 berkas** tidak memakai base component sama sekali.

### 1.1 Skor kesiapan

| Dimensi | Bobot | Skor | Denominator | Gap utama |
|---|---:|---:|---|---|
| Fondasi | 10 | 100% | 23 task non-layar punya berkas | Sebagian service dan slice tidak dipakai layar mana pun |
| Backend | 20 | Tidak dinilai | — | Lihat [backend-unfinished-work.md](backend-unfinished-work.md) |
| Frontend (layar) | 30 | 34% | 90 dari 264 task layar | Kelompok A, B, C, dan D |
| Integrasi dan runtime | 25 | 71% | 372 dari 525 panggilan service punya pasangan di backend | Kelompok E |
| Verifikasi | 15 | 2% | 5 dari 273 laporan memuat baris `UI GATE` | Gerbang base component dilewati |

Skor ini mengukur kemajuan. `READY` baru sah setelah seluruh skenario penerimaan terbukti berjalan.

### 1.2 Cara memperbarui dokumen ini

Bila sebuah task perbaikan selesai, tulis tanggal dan tautan laporan pada kolom **Status** di
[Tabel Task](#tabel-task), lalu perbarui status task asalnya di roadmap seperti biasa. Jangan menghapus barisnya.

---

## 2. Kelompok A — Belum dikerjakan

Layarnya belum ada sama sekali, padahal backend-nya sudah selesai.

**Contoh di rumah sakit.** Kepala Ruangan ingin melihat berapa perawat yang seharusnya ada di ICU
dibandingkan yang sekarang bertugas. Backend sudah bisa menghitungnya, tetapi layar Pemantauan Gap
Tenaga belum dibuat, jadi angkanya tidak bisa dilihat siapa pun.

| Slice | Task | Isi | Prasyarat backend |
|---|---|---|---|
| `S-D1` Perencanaan Tenaga Kerja | `FE-HRD-029` s.d. `FE-HRD-043` (15 task) | Pengaturan, formasi jabatan, batch formasi awal, angka beban, standar, gap tenaga, permintaan headcount, antrean rekrutmen, rencana tahunan | ✅ sudah selesai. Kartu di roadmap yang masih menulis "menunggu BE" sudah usang |
| `S-B1` Presensi dan Kehadiran | `FE-HRD-088` s.d. `FE-HRD-101` (14 task) | Monitoring harian, log biometrik mentah, koreksi, izin pulang cepat, dinas luar, penutupan periode, pengaturan | ✅ sudah selesai |

Bukti: pencarian `workforce-planning` pada `src/` tidak menemukan apa pun, dan `src/app/hr/attendance`
hanya berisi satu halaman peninggalan `FE-HRD-009`.

---

## 3. Kelompok B — Hanya tampilan, datanya contoh di kode

Layar bisa dibuka dan terlihat lengkap, tetapi isinya ditulis langsung di kode. Tidak ada satu pun
data yang diambil dari server, dan tidak ada tindakan yang tersimpan.

**Contoh di rumah sakit.** Kepala Ruangan menekan **Setujui** pada permohonan lembur. Baris itu
berubah menjadi disetujui di layarnya. Perawat yang mengajukan tidak pernah menerima persetujuan,
karena tidak ada data yang dikirim ke server. Saat halaman dimuat ulang, permohonan kembali seperti semula.

| Slice | Task | Bukti |
|---|---|---|
| `S-B3` Lembur | `FE-HRD-117` s.d. `128` | `overtime-approvals-view.jsx`: `useState` berisi data contoh, `currentUserId = "USER-SUPERVISOR-01"`. `overtime-slice.js` punya 9 thunk tetapi tidak dipakai layar mana pun |
| `S-C1` Kredensial | `FE-HRD-187` s.d. `195`, `198`, `199` | `credentialing-psv-dashboard-view.jsx`: daftar antrean ditulis di kode. `credentialing-slice.js` punya 17 thunk tanpa pemakai |
| `S-C1-OPPE` OPPE, FPPE, Rekredensial | `FE-HRD-202` s.d. `214` | `clinical-performance-service.js` tidak dipanggil layar mana pun |
| `S-D4` Layanan SDM | `FE-HRD-173` s.d. `184` | `hr-service-catalog-view.jsx`: katalog ditulis di kode. `hr-service-slice.js` punya 17 thunk tanpa pemakai |
| `S-A7-V2` Persetujuan Terpadu | `FE-HRD-277` s.d. `289` | `unified-approval-inbox-view.jsx`: tombol hanya memanggil reducer lokal seperti `approveItemSuccess`. `unified-approval-service.js` tidak dipanggil |
| `S-D5` Perjalanan Dinas | `FE-HRD-292` s.d. `304` | `businessTravelSlice.js`: `initialState` memakai `MOCK_TRAVEL_REQUESTS`. `business-travel-service.js` tidak dipanggil |
| `S-D3` Benefit (sebagian) | `FE-HRD-161`, `167`, `168`, `170` | `benefits-dashboard-view.jsx`, `benefit-inhouse-ledger-view.jsx`, `ess-cob-submission-view.jsx` |
| `S-C6` K3RS (sebagian) | `FE-HRD-272`, `273`, `274` | `ohs-mcu-view.jsx`, `ohs-injuries-view.jsx` |

---

## 4. Kelompok C — Menampilkan data contoh saat API gagal

Ini kelompok **paling berbahaya**. Layar memanggil API, tetapi bila API gagal, service-nya diam-diam
mengembalikan data contoh atau menjawab "berhasil". Pengguna tidak punya cara untuk tahu bahwa
datanya tidak nyata.

**Contoh di rumah sakit.** Dokter K3RS menilai seorang perawat **tidak laik kerja** setelah tertusuk
jarum. Server menolak permintaan itu. Layar tetap menampilkan "berhasil", karena `assessFitnessToWork`
mengembalikan `success: true` di blok `catch`. Perawat itu tetap terjadwal masuk ruangan.

Kegagalannya kemungkinan besar **selalu** terjadi. Keempat service di bawah tidak memakai
`InstanceAxios`, sehingga cookie sesi login tidak ikut terkirim. Token yang dicari di `localStorage`
juga tidak pernah ditulis oleh aplikasi. Akibatnya server menjawab "belum login" (kode 401), lalu
layar menampilkan data contoh.

| Slice | Task | Service | Jawaban "berhasil" palsu |
|---|---|---|---:|
| `S-C2` Pelatihan | `FE-HRD-217` s.d. `229` | `learning-service.js` | 22 |
| `S-C3` Kinerja | `FE-HRD-232` s.d. `244` | `performance-service.js` | 26 |
| `S-C5` Hubungan Industrial | `FE-HRD-248` s.d. `260` | `employee-relation-service.js` | 20 |
| `S-C6` K3RS | `FE-HRD-262` s.d. `271` | `occupational-health-service.js` | 17 |

Pola yang sama juga ada di `clinical-performance-service.js` (10) dan `unified-approval-service.js`
(6). Keduanya belum dipanggil layar, tetapi akan berbahaya begitu disambungkan.

---

## 5. Kelompok D — Tidak bisa dibuka dari layar mana pun

Berkasnya ada, tetapi tidak ada rute atau layar yang memakainya.

**Contoh di rumah sakit.** HR ingin membuka antrean persetujuan klaim restitusi. Komponennya sudah
ditulis, tetapi alamat `/corporate/hrd/payroll/deductions/approvals` tidak pernah dibuat.

| Task | Berkas | Masalah |
|---|---|---|
| `FE-HRD-135` | `payroll-variances-view.jsx`, `PayrollVarianceDrawer.jsx` | Rute tidak ada, dan modul `/payroll` belum punya layar anomali variansi |
| `FE-HRD-142` | `payroll-settings-view.jsx` | Rute tidak ada, dan modul `/payroll` belum punya layar pengaturan ini |
| `FE-HRD-152` | `restitution-approvals-view.jsx` | Rute `/corporate/hrd/payroll/deductions/approvals` tidak ada |
| `FE-HRD-153` | `ModalClaimDocumentPreview.jsx` | Tidak dipakai layar mana pun |
| `FE-HRD-155` | `ModalVoucherQrViewer.jsx` | Tidak dipakai layar mana pun |
| `FE-HRD-196` | `ClinicalPrivilegeHardStopModal.jsx` | Belum dipasang di layar SIMRS/EMR |
| `FE-HRD-197` | `ModalSupervisorCoSignature.jsx` | Belum dipasang di layar SIMRS/EMR |

**Modul Payroll.** `next.config.js` memindahkan `/hr/payroll/*` ke modul `/payroll`. Sembilan task
(`FE-HRD-013`, `132`, `133`, `134`, `136`, `137`, `138`, `139`, `140`) dianggap pindah dan tidak
masuk angka 137. Tujuh view lama di `src/components/view/hr/payroll/` kini tidak terpakai.

Selain itu ada **24 rute** yang tidak muncul di menu dan tidak ditautkan dari layar lain.
Daftarnya ada di task `FE-HRD-R52`.

---

## 6. Kelompok E — Endpoint tidak cocok dengan backend

Frontend memanggil alamat yang tidak ada di backend, sehingga server menjawab "tidak ditemukan"
(kode 404).

**Contoh.** `credentialing-service.js` memanggil `/corporate/hrd/credentialing/licenses`. Karena
alamat dasar aplikasi berakhiran `/api`, alamat lengkapnya menjadi `/api/corporate/hrd/...`. Backend
menyediakannya di `/api/v1/corporate/hrd/...`. Satu segmen `/v1` yang kurang membuat seluruh panggilan gagal.

Dari 525 panggilan yang bisa dinilai, **153 tidak punya pasangan**:

| Jenis masalah | Jumlah | Tindakan |
|---|---:|---|
| Kurang `/v1` (`credentialing-service.js`, `hr-service.js`) | 58 | Tambahkan `/v1`. Frontend saja |
| Nama alamat berbeda, padanan ada di backend | 66 | Ganti alamat di service. Frontend saja |
| Endpoint belum ada di backend | 29 | Perlu keputusan `DEC-FER-05` atau `K-02`/`K-03` |

Daftar lengkapnya ada di [Lampiran C](#lampiran-c--endpoint-yang-harus-diperbaiki). Tabel per service:

| Service | Klien HTTP | Panggilan | Cocok | Tidak cocok |
|---|---|---:|---:|---:|
| `approval-inbox-service.js` | `InstanceAxios` | 7 | 6 | 1 |
| `attendance-management-service.js` | `InstanceAxios` | 3 | 3 | 0 |
| `benefits-service.js` | `InstanceAxios` | 27 | 20 | 7 |
| `business-travel-service.js` | axios tanpa sesi login | 18 | 10 | 8 |
| `clinical-performance-service.js` | axios tanpa sesi login | 15 | 0 | 15 |
| `credentialing-service.js` | `InstanceAxios` | 24 | 0 | 24 |
| `employee-360-service.js` | `InstanceAxios` | 1 | 1 | 0 |
| `employee-benefit-enrollment-service.js` | `InstanceAxios` | 21 | 21 | 0 |
| `employee-relation-service.js` | axios tanpa sesi login | 20 | 18 | 2 |
| `employee-relations-service.js` | `InstanceAxios` | 2 | 0 | 2 |
| `employee-self-service.js` | `InstanceAxios` | 24 | 21 | 3 |
| `exit-clearance-service.js` | `InstanceAxios` | 9 | 9 | 0 |
| `hr-service.js` | `InstanceAxios` | 34 | 0 | 34 |
| `initial-placement-batch-service.js` | `InstanceAxios` | 7 | 7 | 0 |
| `learning-development-service.js` | `InstanceAxios` | 2 | 0 | 2 |
| `learning-service.js` | axios tanpa sesi login | 22 | 12 | 10 |
| `leave-admin-service.js` | `InstanceAxios` | 2 | 1 | 1 |
| `leave-service.js` | `InstanceAxios` | 22 | 22 | 0 |
| `lifecycle-service.js` | `InstanceAxios` | 35 | 25 | 10 |
| `occupational-health-service.js` | axios tanpa sesi login | 17 | 13 | 4 |
| `on-call-service.js` | `InstanceAxios` | 8 | 8 | 0 |
| `overtime-admin-service.js` | `InstanceAxios` | 2 | 0 | 2 |
| `overtime-service.js` | `InstanceAxios` | 29 | 23 | 6 |
| `payroll-service.js` | `InstanceAxios` | 25 | 18 | 7 |
| `performance-management-service.js` | `InstanceAxios` | 2 | 0 | 2 |
| `performance-service.js` | axios tanpa sesi login | 26 | 20 | 6 |
| `personnel-action-order-service.js` | `InstanceAxios` | 10 | 10 | 0 |
| `recruitment-service.js` | `InstanceAxios` | 49 | 49 | 0 |
| `restitution-service.js` | `InstanceAxios` | 25 | 25 | 0 |
| `roster-scheduling-service.js` | `InstanceAxios` | 13 | 13 | 0 |
| `scheduling-setting-service.js` | `InstanceAxios` | 2 | 2 | 0 |
| `unified-approval-service.js` | axios tanpa sesi login | 10 | 4 | 6 |
| `workforce-core-cross-employee-service.js` | `InstanceAxios` | 12 | 11 | 1 |


---

## 7. Kelompok F — Pemakaian base component

Base component adalah komponen standar Quilvian seperti `BaseButton`, `DataTable`, dan `Hero`. Bila
setiap layar membuat versinya sendiri, tampilan menjadi tidak seragam dan satu perbaikan harus
diulang di puluhan tempat.

**Contoh di rumah sakit.** Petugas HR terbiasa dengan tabel yang punya paginasi dan pencarian di
modul Rekrutmen. Saat membuka modul K3RS, tabelnya berbeda, tidak ada paginasi, dan tombolnya berbentuk lain.

### 7.1 Pelanggaran per jenis elemen

| Elemen | Melanggar | Perlu ditinjau | Unik sah |
|---|---:|---:|---:|
| Input (`BaseFormControl`, `DataFilter`) | 344 | 75 | 16 |
| Tombol (`BaseButton`) | 262 | 198 | 21 |
| Select (`FilterSelect`, `BaseSelectField`) | 139 | 0 | 0 |
| Textarea (`BaseTextAreaField`) | 109 | 0 | 0 |
| Header halaman (`Hero`) | 106 | 0 | 0 |
| Tabel (`DataTable`) | 96 | 7 | 3 |
| Dialog (`ConfirmModal`) | 84 | 2 | 22 |
| Badge (`StatusBadge`) | 64 | 0 | 0 |
| Kartu ringkasan (`SummaryGrid`) | 0 | 37 | 0 |
| **Total** | **1204** | **319** | **62** |


### 7.2 Pelanggaran per slice

| Slice | Berkas UI | Tanpa base sama sekali | Melanggar | Perlu ditinjau | Unik sah | Prop salah | Class CSS hilang | Isian UUID |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| S-C5 | 12 | 12 | 163 | 28 | 0 | 0 | 59 | 0 |
| S-C6 | 13 | 13 | 150 | 21 | 0 | 0 | 0 | 0 |
| S-C3 | 15 | 14 | 96 | 46 | 0 | 0 | 0 | 0 |
| S-C2 | 14 | 13 | 94 | 16 | 1 | 0 | 0 | 0 |
| S-D5 | 14 | 14 | 93 | 14 | 6 | 0 | 0 | 0 |
| S-B2 | 14 | 13 | 67 | 16 | 6 | 0 | 0 | 0 |
| S-A7-V2 | 13 | 13 | 63 | 18 | 6 | 0 | 0 | 0 |
| S-C1 | 19 | 3 | 61 | 16 | 2 | 7 | 8 | 0 |
| S-D3 | 15 | 0 | 58 | 16 | 7 | 12 | 0 | 0 |
| S-C1-OPPE | 13 | 0 | 53 | 20 | 2 | 11 | 0 | 0 |
| S-B3 | 15 | 2 | 53 | 12 | 1 | 7 | 0 | 0 |
| S-D4 | 12 | 0 | 44 | 18 | 7 | 3 | 0 | 0 |
| S-B6 | 10 | 0 | 42 | 14 | 6 | 17 | 0 | 0 |
| S-B4 | 10 | 0 | 39 | 13 | 2 | 6 | 0 | 0 |
| S-B5 | 13 | 0 | 38 | 10 | 8 | 23 | 0 | 0 |
| S-D2 | 13 | 0 | 32 | 28 | 0 | 1 | 0 | 13 |
| S-A2..S-A7 lama | 7 | 0 | 23 | 1 | 1 | 2 | 0 | 0 |
| S-C4-ONB | 15 | 0 | 17 | 4 | 6 | 37 | 0 | 0 |
| FE-020 Benefit lama | 12 | 0 | 7 | 5 | 0 | 9 | 0 | 0 |
| S-A1 | 14 | 0 | 5 | 2 | 1 | 4 | 0 | 0 |
| S-B1 (layar lama) | 1 | 0 | 3 | 0 | 0 | 1 | 0 | 0 |
| S-C4 Exit | 4 | 0 | 3 | 1 | 0 | 11 | 0 | 0 |
| IN-01 | 206 | 9 | 0 | 0 | 0 | 0 | 0 | 0 |


"Melanggar" berarti seharusnya memakai base component. "Perlu ditinjau" berarti butuh penilaian
manusia. Master data (`IN-01`) tidak punya pelanggaran.

### 7.3 Base component yang dipakai dengan salah

Base component tidak melapor bila prop-nya salah, jadi layar tampak berjalan padahal salah.

| Temuan | Jumlah | Contoh dan akibat |
|---|---:|---|
| `BaseButton variant` tidak sah | 105 | `variant="outline"` diam-diam menjadi `primary`, jadi tombol Batal terlihat sama dengan tombol Simpan |
| `StatusBadge` salah pakai | 46 | `status="success"` bukan kunci yang sah, jadi lencananya tampil tanpa warna |
| Class CSS Module tidak ada | 67 | 59 di `S-C5`. `styles.modalOverlay` tidak ada, jadi dialog tampil tanpa latar gelap |
| Isian meminta UUID | 13 | Layar Rekrutmen meminta petugas mengetik pengenal teknis yang tidak pernah mereka ketahui |

### 7.4 Yang memang unik dan tidak perlu base component

Ada 62 elemen yang sah dibuat khusus: keypad PIN, sel kalender dan roster, stepper, kartu yang
bisa diklik, matriks, dan dokumen cetak.

Di dalam angka itu juga termasuk drawer (22) dan unggah berkas (10). Keduanya belum punya base
component padahal sering berulang, jadi diusulkan menjadi base component baru pada `DEC-FER-01` dan `DEC-FER-02`.

---

## 8. Kelompok G — Ketidaksesuaian dokumen roadmap

| Butir | Masalah | Perbaikan |
|---|---|---|
| Register `FE-HRD-058` s.d. `087` | Kartu berstatus selesai, tetapi kolom status register kosong | `FE-HRD-R01` |
| `FE-HRD-216` s.d. `305` | 90 task tidak ada di register | `FE-HRD-R01` |
| `FE-HRD-027` | Register ✅, kartu 🟡 | `FE-HRD-R01` |
| Kartu `S-D1` dan `S-B1` | Masih menulis "menunggu BE" padahal backend ✅ | `FE-HRD-R01` |
| 137 task pada Kelompok B, C, D | Tertulis ✅ padahal kriterianya belum terbukti | `FE-HRD-R01` turunkan ke 🟡 |
| `FE-HRD-014`, `019` | Masih ⛔, sementara penggantinya `S-C1` dan `S-C6` ditandai selesai. `DEC-HRD-008` dan `DEC-HRD-010` tidak ditemukan di decision log | `DEC-FER-07` |
| Laporan task | Hanya 5 dari 273 laporan memuat `UI GATE`; `FE-HRD-021` tidak punya laporan | `FE-HRD-R47` |

---

## 9. Keputusan yang dibutuhkan pemilik

Task yang menunggu keputusan tidak diberi nomor gelombang. Pekerjaan lain tetap bisa berjalan tanpa menunggunya.

### `DEC-FER-01` — Base component Drawer

Panel geser dari samping dipakai 22 kali, misalnya saat HR membuka buku besar saldo cuti tanpa meninggalkan daftar.

- **A. Buat `BaseDrawer` di `base-features` — Rekomendasi.** Tampilan seragam, satu perbaikan berlaku untuk 22 tempat.
- **B. Ganti semua drawer menjadi `ConfirmModal`.** Tanpa komponen baru, tetapi tabel di belakangnya tertutup saat petugas membaca detail.
- **C. Biarkan buatan sendiri.** Tanpa biaya sekarang, tetapi tampilannya tetap berbeda-beda.

### `DEC-FER-02` — Base component unggah berkas

Unggah berkas dipakai 10 kali, misalnya surat dokter dan kuitansi. Batas ukuran dan jenis berkasnya kini diatur sendiri-sendiri.

- **A. Buat `BaseFileUpload` — Rekomendasi.** Aturan batas 5 MB dan jenis berkas cukup ditulis sekali.
- **B. Biarkan per layar.** Tanpa biaya sekarang, tetapi pesan galat dan batasnya bisa berbeda antar layar.

### `DEC-FER-03` — Sisa layar Payroll lama

Layar anomali variansi dan pengaturan payroll belum ada padanannya di modul `/payroll`. Tanpa layar anomali, HR tidak bisa memeriksa gaji yang naik tidak wajar sebelum disetujui.

- **A. Bangun keduanya di modul `/payroll`, lalu hapus 7 view lama — Rekomendasi.** Payroll tetap satu pintu sesuai `next.config.js`.
- **B. Hidupkan lagi rute `/hr/payroll/*`.** Lebih cepat, tetapi ada dua pintu untuk satu proses.

### `DEC-FER-04` — Rute utama

Satu layar kini bisa punya hingga tiga alamat (`/hr/*`, `/hrd/*`, `/corporate/hrd/*`). Contoh: Pelatihan tersedia di `/hr/learning/*` dan `/corporate/hrd/learning/*`.

- **A. Alamat yang ada di menu menjadi alamat utama; sisanya dialihkan — Rekomendasi.** Tautan lama tetap berjalan.
- **B. Biarkan semua alamat aktif.** Tanpa perubahan, tetapi hak akses dan menu aktif rawan berbeda.

### `DEC-FER-05` — Endpoint daftar yang belum ada di backend

Beberapa layar butuh daftar seluruh pegawai, tetapi backend hanya menyediakan data per pegawai. Contoh: daftar pembatasan kerja K3RS dan daftar sanksi disiplin.

- **A. Tambah endpoint daftar di backend, dengan hak akses tersendiri untuk data sensitif — Rekomendasi.** Layar bekerja seperti dirancang. Perlu revisi kontrak.
- **B. Ubah layar menjadi "pilih pegawai dulu".** Tanpa perubahan backend, tetapi tidak menjawab pertanyaan seperti "siapa saja yang sedang dibatasi tugasnya?".

### `DEC-FER-06` — Tempat modal SIMRS

Modal hard-stop dan co-signature harus muncul saat dokter menulis order di EMR.

- **A. Pasang di layar order modul klinis, lewat task bersama pemilik modul klinis — Rekomendasi.** Pengaman benar-benar bekerja di titik pelayanan.
- **B. Tunda sampai integrasi SIMRS diputuskan.** Tanpa risiko regresi modul klinis, tetapi pengaman wewenang klinis belum aktif.

### `DEC-FER-07` — Nasib `FE-HRD-014` dan `FE-HRD-019`

- **A. Tandai digantikan oleh `S-C1` dan `S-C6`, dengan persetujuan Komite Medik dan K3RS dicatat di decision log — Rekomendasi.** Status menjadi jelas.
- **B. Biarkan ⛔ dan hentikan `S-C1` serta `S-C6`.** Paling konservatif, tetapi layar yang sudah dibangun tertahan.

### `K-02` dan `K-03` (sudah ada sebelumnya)

Keduanya berasal dari [plan-repair-001](../docs/plan-repair/plan-repair-001-layar-hr-kosong-salah-alamat-api.md).
`K-02` menentukan daftar lintas-pegawai untuk disiplin, pelatihan, dan kinerja. `K-03` menentukan
perlakuan pemanggilan tanpa padanan pasti. Keduanya menahan `FE-HRD-R56`.

---

## 10. Proses perbaikan satu layar

Bagian ini menjelaskan alur kerja yang berlaku untuk setiap task perbaikan.

| Unsur | Penjelasan |
|---|---|
| Tujuan | Layar menampilkan data nyata, jujur saat gagal, dan memakai komponen standar |
| Pelaku | Developer frontend mengerjakan; pemilik modul memutuskan; petugas HR atau pengguna ruangan menguji |
| Pemicu | Task pada gelombang yang sedang berjalan dan seluruh prasyaratnya selesai |
| Prasyarat | Kontrak API slice terkunci, endpoint backend tersedia atau dicatat belum tersedia |

Langkah:

1. Developer membuka kartu task dan kartu asalnya di `frontend-roadmap.md`.
2. Developer memecah layar menjadi elemen dan mengisi tabel keputusan `UI GATE`.
3. Developer mengganti data contoh dengan panggilan API lewat `InstanceAxios`.
4. Developer mengganti elemen buatan sendiri dengan base component.
5. Developer menjalankan `check-css-usage.mjs`, `npm run lint`, dan `npm run build`.
6. Developer menguji tombol utama di browser dengan akun yang punya hak dan yang tidak punya hak.
7. Developer menulis laporan di `task/report/frontend/`, lalu memperbarui status di roadmap dan dokumen ini.

Aturan bisnisnya:

- Layar tidak boleh menampilkan data yang tidak berasal dari server.
- Layar tidak boleh menyatakan berhasil sebelum server menyatakan berhasil.
- Nilai yang tidak tersedia tampil sebagai "data belum tersedia", bukan angka karangan.

**Contoh.** Server gagal saat HR menyimpan pembatasan kerja. Layar menampilkan "Gagal menyimpan.
Silakan coba lagi." lewat `InformationAlert`, dan isian formulir tetap ada sehingga petugas tidak mengetik ulang.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| ✅ (klaim lama) | Koreksi hasil audit | 🟡 | Pemilik modul lewat `FE-HRD-R01` | Task termasuk Kelompok B, C, atau D |
| 🟡 atau tanpa tanda | Task perbaikan selesai | ✅ | Developer | Kriteria terbukti, validasi dijalankan, laporan ada |
| Tanpa tanda | Prasyarat berupa keputusan belum ada | ⛔ | Pemilik modul | Nama keputusan disebut |

Jalur tidak normal:

- **Endpoint belum ada.** Bagian itu menampilkan "data belum tersedia" dan dicatat pada `FE-HRD-R53`. Task tetap boleh selesai untuk bagian lain, dengan status 🟡.
- **Kontrak berbeda dengan backend.** Developer berhenti pada bagian itu dan melaporkannya ke pemilik.
- **Base component tidak mencukupi.** Developer mengajukan pilihan lewat `base-component-decision-gate`, tidak membuat komponen baru sendiri.

Hasil akhirnya, layar memakai data nyata, statusnya terbukti di roadmap, dan petugas bisa memercayai apa yang tampil.

---

## Legenda Tanda Status

| Tanda | Arti |
|:---:|---|
| ✅ | Kriteria dan Definition of Done sudah terbukti |
| 🟡 | Source sudah ada, kriteria belum terbukti penuh |
| ⛔ | Tertahan keputusan atau gerbang |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

Cara membaca:

- `A ─> B` artinya **A harus selesai dulu, baru B boleh dimulai**. Prasyarat selalu di kiri.
- Node dalam kurung kurawal adalah gerbang keputusan, bukan task.
- `[G1]`, `[G2]`, dan `[G3]` adalah cermin baca-saja dari task yang tempat aslinya ada di grafik tersebut.

### Grafik G1 — Service, layar, dan migrasi base component

```text
FE-HRD-R02 ✅ ─> FE-HRD-R14 ─┬─> FE-HRD-R37
                             │
                             ├─> FE-HRD-R38
                             │
                             ├─> FE-HRD-R39
                             │
                             ├─> FE-HRD-R40
                             │
                             └─> FE-HRD-R41

FE-HRD-R03 ✅ ─> FE-HRD-R15

FE-HRD-R04 ✅ ─> FE-HRD-R16

FE-HRD-R05 ✅ ─> FE-HRD-R17

FE-HRD-R06 ✅ ─┬─> FE-HRD-R18 ─> FE-HRD-R30
               │
               └─> FE-HRD-R19 ─> FE-HRD-R31

FE-HRD-R07 ✅ ─> FE-HRD-R20 ─> FE-HRD-R32

FE-HRD-R08 ✅ ─> FE-HRD-R21 ─> FE-HRD-R33

FE-HRD-R09 ✅ ─> FE-HRD-R22 ─> FE-HRD-R34

FE-HRD-R10 🟡 ─> FE-HRD-R25 🟡

FE-HRD-R11 ✅ ─┬─> FE-HRD-R23 ─> FE-HRD-R35
               │
               └─> FE-HRD-R24
```

### Grafik G2 — Layar yang belum dikerjakan (`S-D1`, `S-B1`)

```text
FE-HRD-R12 🟡 ─┬─> FE-HRD-R26 🟡 ─> FE-HRD-R36 ─┬─> FE-HRD-R42
               │                             │
               └─> FE-HRD-R27 🟡 ────────────┘

FE-HRD-R13 🟡 ─┬─> FE-HRD-R28 🟡 ─┬─> FE-HRD-R43
               │               │
               └─> FE-HRD-R29 🟡 ─┘
```

### Grafik G3 — Verifikasi dan audit akhir

```text
FE-HRD-R14 [G1] ─┐
                 │
FE-HRD-R15 [G1] ─┴─┐
                   │
FE-HRD-R30 [G1] ───┴─┐
                     │
FE-HRD-R32 [G1] ─────┴─> FE-HRD-R44

FE-HRD-R31 [G1] ─┐
                 │
FE-HRD-R33 [G1] ─┴─┐
                   │
FE-HRD-R34 [G1] ───┴─┐
                     │
FE-HRD-R35 [G1] ─────┴─> FE-HRD-R45

FE-HRD-R16 [G1] ─┐
                 │
FE-HRD-R17 [G1] ─┴─┐
                   │
FE-HRD-R24 [G1] ───┴─┐
                     │
FE-HRD-R25 🟡 [G1] ──┴─┐
                       │
FE-HRD-R37 [G1] ───────┴─┐
                         │
FE-HRD-R38 [G1] ─────────┴─┐
                           │
FE-HRD-R39 [G1] ───────────┴─┐
                             │
FE-HRD-R40 [G1] ─────────────┴─┐
                               │
FE-HRD-R41 [G1] ───────────────┴─> FE-HRD-R46

FE-HRD-R01 ✅ ───┐
                 │
FE-HRD-R42 [G2] ─┴─┐
                   │
FE-HRD-R43 [G2] ───┴─┐
                     │
FE-HRD-R44 [G3] ─────┴─┐
                       │
FE-HRD-R45 [G3] ───────┴─┐
                         │
FE-HRD-R46 [G3] ─────────┴─> FE-HRD-R47
```

### Grafik G4 — Task yang tertahan keputusan

```text
{DEC-FER-01 ⛔} ─> FE-HRD-R48 ⛔ ─┬─> FE-HRD-R50 ⛔
                                  │
{DEC-FER-02 ⛔} ─> FE-HRD-R49 ⛔ ─┘

{DEC-FER-03 ⛔} ─> FE-HRD-R51 ⛔

{DEC-FER-04 ⛔} ─> FE-HRD-R52 ⛔

{DEC-FER-05 ⛔} ─> FE-HRD-R53 ⛔

{DEC-FER-06 ⛔} ─> FE-HRD-R54 ⛔

{DEC-FER-07 ⛔} ─> FE-HRD-R55 ⛔

{K-02 ⛔} ─┬─> FE-HRD-R56 ⛔
           │
{K-03 ⛔} ─┘
```

## Tabel Gelombang Eksekusi

Task dalam satu gelombang boleh dikerjakan paralel. Bila hanya ada satu developer, kerjakan sesuai urutan nomor task.

| Gelombang | Boleh mulai setelah | Task |
|---:|---|---|
| **1** | — | `FE-HRD-R01`, `FE-HRD-R02`, `FE-HRD-R03`, `FE-HRD-R04`, `FE-HRD-R05`, `FE-HRD-R06`, `FE-HRD-R07`, `FE-HRD-R08`, `FE-HRD-R09`, `FE-HRD-R10`, `FE-HRD-R11`, `FE-HRD-R12`, `FE-HRD-R13` |
| **2** | Service atau paket fondasi masing-masing pada gelombang 1 | `FE-HRD-R14`, `FE-HRD-R15`, `FE-HRD-R16`, `FE-HRD-R17`, `FE-HRD-R18`, `FE-HRD-R19`, `FE-HRD-R20`, `FE-HRD-R21`, `FE-HRD-R22`, `FE-HRD-R23`, `FE-HRD-R24`, `FE-HRD-R25`, `FE-HRD-R26`, `FE-HRD-R27`, `FE-HRD-R28`, `FE-HRD-R29` |
| **3** | Task bagian 1, pola rujukan `FE-HRD-R14`, atau paket `S-B1` dan `S-D1` pada gelombang 2 | `FE-HRD-R30`, `FE-HRD-R31`, `FE-HRD-R32`, `FE-HRD-R33`, `FE-HRD-R34`, `FE-HRD-R35`, `FE-HRD-R36`, `FE-HRD-R37`, `FE-HRD-R38`, `FE-HRD-R39`, `FE-HRD-R40`, `FE-HRD-R41`, `FE-HRD-R43` |
| **4** | Seluruh layar pada kelompok verifikasinya | `FE-HRD-R42`, `FE-HRD-R44`, `FE-HRD-R45`, `FE-HRD-R46` |
| **5** | Seluruh verifikasi dan `FE-HRD-R01` | `FE-HRD-R47` |
| **⛔** | menunggu `{DEC-FER-01}` | `FE-HRD-R48` |
| **⛔** | menunggu `{DEC-FER-02}` | `FE-HRD-R49` |
| **⛔** | menunggu `FE-HRD-R48`, `FE-HRD-R49` | `FE-HRD-R50` |
| **⛔** | menunggu `{DEC-FER-03}` | `FE-HRD-R51` |
| **⛔** | menunggu `{DEC-FER-04}` | `FE-HRD-R52` |
| **⛔** | menunggu `{DEC-FER-05}` | `FE-HRD-R53` |
| **⛔** | menunggu `{DEC-FER-06}` | `FE-HRD-R54` |
| **⛔** | menunggu `{DEC-FER-07}` | `FE-HRD-R55` |
| **⛔** | menunggu `{K-02}`, `{K-03}` | `FE-HRD-R56` |


Tujuan tiap gelombang:

| Gelombang | Tujuan | Hasil bagi pengguna |
|---:|---|---|
| **1** | Menghentikan informasi palsu dan menyiapkan fondasi | Layar tidak lagi berbohong. Bila server gagal, pengguna tahu |
| **2** | Menyambungkan layar utama dan memulai `S-D1` serta `S-B1` | Data K3RS, disiplin, kinerja, pelatihan, kredensial, dan persetujuan mulai nyata |
| **3** | Menuntaskan layar lanjutan dan migrasi base component | Seluruh alur berjalan dan tampilannya seragam |
| **4** | Menguji per kelompok | Bukti uji tersedia per slice |
| **5** | Audit akhir | Status roadmap sesuai kenyataan |

**Urutan keputusan.** Keputusan pada bagian 9 sebaiknya diambil selama Gelombang 1, supaya task yang
tertahan bisa bergabung pada Gelombang 3. Dari semuanya, `DEC-FER-05` paling banyak membuka pekerjaan.

**Audit akhir dan task tertahan.** Bila masih ada task ⛔ saat `FE-HRD-R47` berjalan, verdict tertinggi
yang boleh diberikan adalah `READY_WITH_CONDITIONS`, dengan pemilik dan mitigasi per task.

---

## Tabel Task

| Task ID | Gelombang | Slice | Pekerjaan | Task asal | Dependency | Status |
|---|:---:|---|---|---|---|---|
| `FE-HRD-R01` | 1 | Semua | Sinkronisasi status dan register roadmap | Kelompok G | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R01.md) |
| `FE-HRD-R02` | 1 | S-C6 | Service K3RS jujur dan tersambung | `FE-HRD-261`, `262`–`271` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R02.md) |
| `FE-HRD-R03` | 1 | S-C5 | Service Hubungan Industrial jujur dan tersambung | `FE-HRD-246`, `248`–`260` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R03.md) |
| `FE-HRD-R04` | 1 | S-C3 | Service Kinerja jujur dan tersambung | `FE-HRD-231`–`244` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R04.md) |
| `FE-HRD-R05` | 1 | S-C2 | Service Pelatihan jujur dan tersambung | `FE-HRD-216`–`229` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R05.md) |
| `FE-HRD-R06` | 1 | S-C1, S-D4 | Perbaikan alamat Kredensial dan Layanan SDM | `FE-HRD-186`, `172` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R06.md) |
| `FE-HRD-R07` | 1 | S-C1-OPPE | Service OPPE jujur dan tersambung | `FE-HRD-201` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R07.md) |
| `FE-HRD-R08` | 1 | S-A7-V2 | Service Persetujuan Terpadu jujur dan tersambung | `FE-HRD-276` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R08.md) |
| `FE-HRD-R09` | 1 | S-D5 | Service Perjalanan Dinas jujur dan tersambung | `FE-HRD-291` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R09.md) |
| `FE-HRD-R10` | 1 | S-C4-ONB, S-A1 | Perbaikan alamat Onboarding, ESS, dan Administrasi | `FE-HRD-058`, `026-ESS` | — | 🟡 11 Nov 2026 — alamat cocok backend; tombol belum diuji di browser karena menulis data — [laporan](../task/report/frontend/FE-HRD-R10.md) |
| `FE-HRD-R11` | 1 | S-B3, S-D3 | Perbaikan alamat Lembur dan Benefit | `FE-HRD-116`, `158` | — | ✅ 11 Nov 2026 — [laporan](../task/report/frontend/FE-HRD-R11.md) |
| `FE-HRD-R12` | 1 | S-D1 | Bangun `S-D1` MVP-7: Pengaturan, Formasi, Batch Formasi | `FE-HRD-029`, `030`, `031` | — | 🟡 11 Nov 2026 — `029` ✅, `030` dan `031` 🟡 (belum ada data uji) — [laporan](../task/report/frontend/FE-HRD-R12.md) |
| `FE-HRD-R13` | 1 | S-B1 | Bangun `S-B1` MVP-21: Monitoring Presensi | `FE-HRD-088`–`091`, `099` | — | 🟡 11 Nov 2026 — `088`, `091`, `099` ✅; `089`, `090` 🟡 — [laporan](../task/report/frontend/FE-HRD-R13.md) |
| `FE-HRD-R14` | 2 | S-C6 | Layar K3RS: sambung dan migrasi (pola rujukan) | `FE-HRD-262`–`274` | `FE-HRD-R02` | Belum |
| `FE-HRD-R15` | 2 | S-C5 | Layar Hubungan Industrial: sambung dan migrasi | `FE-HRD-248`–`260` | `FE-HRD-R03` | Belum |
| `FE-HRD-R16` | 2 | S-C3 | Layar Kinerja: sambung dan migrasi | `FE-HRD-232`–`244` | `FE-HRD-R04` | Belum |
| `FE-HRD-R17` | 2 | S-C2 | Layar Pelatihan: sambung dan migrasi | `FE-HRD-217`–`229` | `FE-HRD-R05` | Belum |
| `FE-HRD-R18` | 2 | S-C1 | Kredensial bagian 1: ESS, PSV, radar, katalog | `FE-HRD-187`–`190` | `FE-HRD-R06` | Belum |
| `FE-HRD-R19` | 2 | S-D4 | Layanan SDM bagian 1: katalog dan helpdesk | `FE-HRD-173`–`177` | `FE-HRD-R06` | Belum |
| `FE-HRD-R20` | 2 | S-C1-OPPE | OPPE bagian 1: dasbor, periode, peer review | `FE-HRD-202`–`206` | `FE-HRD-R07` | Belum |
| `FE-HRD-R21` | 2 | S-A7-V2 | Persetujuan bagian 1: kotak masuk, detail, aksi | `FE-HRD-277`–`283` | `FE-HRD-R08` | Belum |
| `FE-HRD-R22` | 2 | S-D5 | Perjalanan Dinas bagian 1: dasbor, tarif, SPPD | `FE-HRD-292`–`297` | `FE-HRD-R09` | Belum |
| `FE-HRD-R23` | 2 | S-B3 | Lembur bagian 1: pengajuan dan persetujuan ruangan | `FE-HRD-117`–`120` | `FE-HRD-R11` | Belum |
| `FE-HRD-R24` | 2 | S-D3 | Benefit: sambung sisa dan migrasi | `FE-HRD-161`, `167`, `168`, `170` | `FE-HRD-R11` | Belum |
| `FE-HRD-R25` | 2 | S-C4-ONB | Onboarding dan Kontrak: migrasi | `FE-HRD-059`–`073` | `FE-HRD-R10` | 🟡 source, lint, build, dan CSS gate lulus; menunggu uji browser serta endpoint unggah batch. [Laporan](../task/report/frontend/FE-HRD-R25.md) |
| `FE-HRD-R26` | 2 | S-D1 | Bangun `S-D1` MVP-8: Angka Beban, Standar, Gap | `FE-HRD-032`–`035` | `FE-HRD-R12` | 🟡 source, lint, build, CSS gate lulus; menunggu uji browser/peran. [Laporan](../task/report/frontend/FE-HRD-R26.md) |
| `FE-HRD-R27` | 2 | S-D1 | Bangun `S-D1` MVP-10: Rencana Tahunan | `FE-HRD-041`, `042` | `FE-HRD-R12` | 🟡 source/build lulus; endpoint cancel dan availableActions Finance tidak ada. [Laporan](../task/report/frontend/FE-HRD-R27.md) |
| `FE-HRD-R28` | 2 | S-B1 | Bangun `S-B1` MVP-22: Koreksi, Izin, Dinas Luar | `FE-HRD-095`–`098`, `100` | `FE-HRD-R13` | 🟡 tiga layar admin aktif; upload koreksi kuasa dan ekstensi ESS terblokir kontrak. [Laporan](../task/report/frontend/FE-HRD-R28.md) |
| `FE-HRD-R29` | 2 | S-B1 | Bangun `S-B1` MVP-23: Penutupan Periode | `FE-HRD-092`–`094` | `FE-HRD-R13` | 🟡 guard tutup/buka aktif; metrik rekap tidak tersedia di DTO. [Laporan](../task/report/frontend/FE-HRD-R29.md) |
| `FE-HRD-R30` | 3 | S-C1 | Kredensial bagian 2: komite, direktur, registri | `FE-HRD-191`–`195`, `198`, `199` | `FE-HRD-R18` | Belum |
| `FE-HRD-R31` | 3 | S-D4 | Layanan SDM bagian 2: ESS, surat, verifikasi publik | `FE-HRD-178`–`184` | `FE-HRD-R19` | Belum |
| `FE-HRD-R32` | 3 | S-C1-OPPE | OPPE bagian 2: FPPE dan rekredensial | `FE-HRD-207`–`214` | `FE-HRD-R20` | Belum |
| `FE-HRD-R33` | 3 | S-A7-V2 | Persetujuan bagian 2: delegasi, matriks, ESS | `FE-HRD-284`–`289` | `FE-HRD-R21` | Belum |
| `FE-HRD-R34` | 3 | S-D5 | Perjalanan Dinas bagian 2: uang muka, settlement, klaim | `FE-HRD-298`–`304` | `FE-HRD-R22` | Belum |
| `FE-HRD-R35` | 3 | S-B3 | Lembur bagian 2: verifikasi, CTO, penutupan | `FE-HRD-121`–`128` | `FE-HRD-R23` | Belum |
| `FE-HRD-R36` | 3 | S-D1 | Bangun `S-D1` MVP-9: Headcount dan Antrean Rekrutmen | `FE-HRD-036`–`040` | `FE-HRD-R26` | Belum |
| `FE-HRD-R37` | 3 | S-B2 | Migrasi base component Cuti | `FE-HRD-103`–`114` | `FE-HRD-R14` | Belum |
| `FE-HRD-R38` | 3 | S-B4 | Migrasi base component Roster | `FE-HRD-077`–`085` | `FE-HRD-R14` | Belum |
| `FE-HRD-R39` | 3 | S-D2 | Migrasi Rekrutmen dan perbaikan isian UUID | `FE-HRD-044`–`056` | `FE-HRD-R14` | Belum |
| `FE-HRD-R40` | 3 | S-B6 | Migrasi Restitusi dan pasang layar yang tertinggal | `FE-HRD-146`–`156` | `FE-HRD-R14` | Belum |
| `FE-HRD-R41` | 3 | S-A1 s.d. S-A7, S-C4, FE-020 | Migrasi layar administrasi dan ESS lama | `FE-HRD-002`–`008`, `017`, `020` | `FE-HRD-R14` | Belum |
| `FE-HRD-R42` | 4 | S-D1 | Verifikasi `S-D1` | `FE-HRD-043` | `FE-HRD-R27`, `FE-HRD-R36` | Belum |
| `FE-HRD-R43` | 3 | S-B1 | Verifikasi `S-B1` | `FE-HRD-101` | `FE-HRD-R28`, `FE-HRD-R29` | Belum |
| `FE-HRD-R44` | 4 | S-C6, S-C5, S-C1, S-C1-OPPE | Verifikasi layar klinis dan keselamatan | `FE-HRD-200`, `215`, `275` | `FE-HRD-R14`, `FE-HRD-R15`, `FE-HRD-R30`, `FE-HRD-R32` | Belum |
| `FE-HRD-R45` | 4 | S-A7-V2, S-D5, S-D4, S-B3 | Verifikasi layar operasional | `FE-HRD-290`, `305`, `185`, `129` | `FE-HRD-R31`, `FE-HRD-R33`, `FE-HRD-R34`, `FE-HRD-R35` | Belum |
| `FE-HRD-R46` | 4 | S-C2, S-C3, S-D3, S-C4-ONB, S-B2, S-B4, S-D2, S-B6, lama | Verifikasi layar pengembangan dan administrasi | `FE-HRD-230`, `245`, `171`, `074`, `115`, `087`, `057`, `157` | `FE-HRD-R16`, `FE-HRD-R17`, `FE-HRD-R24`, `FE-HRD-R25`, `FE-HRD-R37`, `FE-HRD-R38`, `FE-HRD-R39`, `FE-HRD-R40`, `FE-HRD-R41` | Belum |
| `FE-HRD-R47` | 5 | Semua | Audit akhir dan pembaruan status | Seluruh task | `FE-HRD-R01`, `FE-HRD-R42`, `FE-HRD-R43`, `FE-HRD-R44`, `FE-HRD-R45`, `FE-HRD-R46` | Belum |
| `FE-HRD-R48` | ⛔ | Bersama | Base component `BaseDrawer` | Kelompok F | `{DEC-FER-01}` | ⛔ |
| `FE-HRD-R49` | ⛔ | Bersama | Base component `BaseFileUpload` | Kelompok F | `{DEC-FER-02}` | ⛔ |
| `FE-HRD-R50` | ⛔ | Bersama | Adopsi drawer dan unggah berkas di HR | Kelompok F | `FE-HRD-R48`, `FE-HRD-R49` | ⛔ |
| `FE-HRD-R51` | ⛔ | S-B5 | Layar anomali dan pengaturan Payroll | `FE-HRD-135`, `142` | `{DEC-FER-03}` | ⛔ |
| `FE-HRD-R52` | ⛔ | Semua | Penataan rute utama dan menu | Kelompok D | `{DEC-FER-04}` | ⛔ |
| `FE-HRD-R53` | ⛔ | S-C2, S-C3, S-C5, S-C6, S-D5, S-D3, S-B3 | Layar daftar yang menunggu endpoint baru | Lampiran C | `{DEC-FER-05}` | ⛔ |
| `FE-HRD-R54` | ⛔ | S-C1 | Pasang modal SIMRS | `FE-HRD-196`, `197` | `{DEC-FER-06}` | ⛔ |
| `FE-HRD-R55` | ⛔ | S-C1, S-C6 | Penutupan `FE-HRD-014` dan `019` | `FE-HRD-014`, `019` | `{DEC-FER-07}` | ⛔ |
| `FE-HRD-R56` | ⛔ | S-A1, S-B3, S-C2, S-C3, S-C5 | Sisa task sebagian | `FE-HRD-011`, `015`, `016`, `018`, `026`, `027` | `{K-02}`, `{K-03}` | ⛔ |


---

## Kartu Task

### Catatan umum untuk seluruh kartu

Ketentuan ini berlaku untuk setiap kartu agar tidak diulang.

**Kriteria umum service (`AC-S`):**

1. Service memakai `InstanceAxios`, sehingga sesi login ikut terkirim.
2. Tidak ada blok `catch` yang mengembalikan data contoh atau `success: true`. Galat diteruskan ke pemanggil.
3. Setiap alamat cocok dengan backend menurut Lampiran C. Yang belum ada ditandai dan dicatat pada `FE-HRD-R53`.

**Kriteria umum layar (`AC-L`):**

1. Tidak ada data contoh di kode. Seluruh data berasal dari API.
2. Saat API gagal, layar menampilkan pesan yang dimengerti pengguna. Pesan bawaan Axios tidak ditampilkan.
3. Elemen memakai base component menurut katalog; elemen unik dicatat alasannya pada `UI GATE`.
4. `check-css-usage.mjs` memberikan 0 temuan pada berkas yang disentuh.
5. Tombol yang tidak berhak disembunyikan sesuai `requiredPermission` atau `availableActions`.

**Bukti verifikasi standar (`V`):** `npm run lint`, `npm run build`, `check-css-usage.mjs`, uji manual
dengan akun berhak dan tidak berhak, dan baris `UI GATE` pada laporan.

**Definition of Done standar (`DoD`):** kriteria terbukti, validasi dijalankan, laporan ada di
`task/report/frontend/`, status di `frontend-roadmap.md` dan dokumen ini diperbarui.

**Pemilik seluruh task:** Frontend HR UI Lead.

**Handoff.** Setiap task dikerjakan lewat `quilvian-engineering-skills:build-module-frontend`. Pilihan
susunan tampilan yang belum disetujui berstatus `DEV_DISCRETION`. Task yang perlu perubahan backend
diselesaikan lewat `build-module-backend`, dan QBE preflight dijalankan saat eksekusi dari `AGENTS.md` backend.

#### ✅ `FE-HRD-R01` — Sinkronisasi status dan register roadmap
* **Slice / Gelombang**: Semua / 1
* **Task asal**: Kelompok G
* **Outcome**: Roadmap menggambarkan keadaan sebenarnya, sehingga tim tidak mengira layar demo sudah siap dipakai.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: —
* **Reuse**: Aturan `status-task-roadmap.md`
* **Cakupan**: `frontend-roadmap.md`: turunkan 137 task ke 🟡 dengan menyebut jumlah kriteria, isi status `058`–`087`, tambah `216`–`305` ke register, samakan `027`, perbarui kartu `S-D1` dan `S-B1`. Tidak menyentuh source.
* **Dependency**: —
* **Acceptance criteria**: Jumlah task ✅ sama dengan hasil audit. Setiap task Kelompok B, C, D berstatus 🟡 beserta alasannya.
* **Bukti verifikasi**: Tinjauan dokumen oleh pemilik
* **Risiko & pemilik**: Risiko kecil. Hanya dokumen. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R02` — Service K3RS jujur dan tersambung
* **Slice / Gelombang**: S-C6 / 1
* **Task asal**: `FE-HRD-261`, `262`–`271`
* **Outcome**: Penilaian kelaikan kerja dan pembatasan tugas tidak lagi tampil berhasil bila server gagal.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v23 bagian 29
* **Reuse**: `InstanceAxios`
* **Cakupan**: `occupational-health-service.js` dan penanganan galat pada view pemanggilnya.
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: `assessFitnessToWork` meneruskan galat. Contoh: server menjawab 403, layar menampilkan "Anda tidak punya hak", bukan "berhasil".
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko: layar terlihat kosong karena endpoint daftar belum ada. Mitigasi: tampilkan "data belum tersedia". Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R03` — Service Hubungan Industrial jujur dan tersambung
* **Slice / Gelombang**: S-C5 / 1
* **Task asal**: `FE-HRD-246`, `248`–`260`
* **Outcome**: Sanksi, skorsing, dan keluhan benar-benar tersimpan atau jelas gagal.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v22 bagian 28
* **Reuse**: `InstanceAxios`
* **Cakupan**: `employee-relation-service.js` dan penanganan galat pada view pemanggilnya.
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: Penerbitan Surat Peringatan yang gagal menampilkan pesan gagal dan tidak menambah baris di daftar.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko: data disiplin sensitif. Mitigasi: hak akses mengikuti backend. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R04` — Service Kinerja jujur dan tersambung
* **Slice / Gelombang**: S-C3 / 1
* **Task asal**: `FE-HRD-231`–`244`
* **Outcome**: Nilai kinerja dan PIP berasal dari server.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v21 bagian 27
* **Reuse**: `InstanceAxios`
* **Cakupan**: `performance-service.js` dan penanganan galat pada view pemanggilnya.
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: Alamat `pip/active`, `pip/contract`, `disputes/submit`, dan `disputes/{id}/adjudicate` mengikuti Lampiran C.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R05` — Service Pelatihan jujur dan tersambung
* **Slice / Gelombang**: S-C2 / 1
* **Task asal**: `FE-HRD-216`–`229`
* **Outcome**: Sertifikat dan kepatuhan pelatihan berasal dari server.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v20 bagian 26
* **Reuse**: `InstanceAxios`
* **Cakupan**: `learning-service.js` dan penanganan galat pada view pemanggilnya.
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: Alamat `bonds` dan `skp` beralih ke grup `training` sesuai Lampiran C.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko: `assessments` backend memerlukan `sessionId`. Mitigasi: layar memilih sesi lebih dulu. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R06` — Perbaikan alamat Kredensial dan Layanan SDM
* **Slice / Gelombang**: S-C1, S-D4 / 1
* **Task asal**: `FE-HRD-186`, `172`
* **Outcome**: Kedua service siap dipakai layar.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v18 bagian 24, v17 bagian 23
* **Reuse**: `InstanceAxios`
* **Cakupan**: `credentialing-service.js`, `hr-service.js`: tambahkan `/v1`.
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: 58 panggilan cocok dengan backend.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko kecil. Belum dipakai layar. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R07` — Service OPPE jujur dan tersambung
* **Slice / Gelombang**: S-C1-OPPE / 1
* **Task asal**: `FE-HRD-201`
* **Outcome**: Service OPPE, FPPE, dan rekredensial mengikuti backend.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v19 bagian 25
* **Reuse**: `InstanceAxios`
* **Cakupan**: `clinical-performance-service.js`: ganti seluruh alamat sesuai Lampiran C, hapus jawaban palsu.
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: 13 panggilan yang punya padanan cocok dengan backend.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko kecil. Belum dipakai layar. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R08` — Service Persetujuan Terpadu jujur dan tersambung
* **Slice / Gelombang**: S-A7-V2 / 1
* **Task asal**: `FE-HRD-276`
* **Outcome**: Service persetujuan V2 mengikuti backend.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract bagian 30
* **Reuse**: `InstanceAxios`
* **Cakupan**: `unified-approval-service.js`: ganti klien dan alamat aksi ke `workflow/approvals/*`.
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: Tidak ada lagi jawaban seperti "Persetujuan berhasil disahkan (mode terdistribusi)" saat server gagal.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko: keputusan persetujuan bernilai hukum. Mitigasi: uji dengan dua akun. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R09` — Service Perjalanan Dinas jujur dan tersambung
* **Slice / Gelombang**: S-D5 / 1
* **Task asal**: `FE-HRD-291`
* **Outcome**: Service perjalanan dinas mengikuti backend.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract bagian 31
* **Reuse**: `InstanceAxios`
* **Cakupan**: `business-travel-service.js`: ganti klien dan alamat sesuai Lampiran C.
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: Pembatalan memakai `PUT requests/{id}/cancel`.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko kecil. Belum dipakai layar. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### 🟡 `FE-HRD-R10` — Perbaikan alamat Onboarding, ESS, dan Administrasi
* **Slice / Gelombang**: S-C4-ONB, S-A1 / 1
* **Task asal**: `FE-HRD-058`, `026-ESS`
* **Outcome**: Tombol yang kini gagal 404 mulai berfungsi.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v9 bagian 15, v6
* **Reuse**: `InstanceAxios`
* **Cakupan**: `lifecycle-service.js` (10), `employee-self-service.js` (3), `workforce-core-cross-employee-service.js` (1).
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: Contoh: `employment-contracts/{id}/decision` menjadi `.../decisions`.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko: bentuk isian berbeda. Mitigasi: cocokkan DTO backend. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ✅ `FE-HRD-R11` — Perbaikan alamat Lembur dan Benefit
* **Slice / Gelombang**: S-B3, S-D3 / 1
* **Task asal**: `FE-HRD-116`, `158`
* **Outcome**: Service siap dipakai layar Lembur dan klaim COB.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v13 bagian 19, v16 bagian 22
* **Reuse**: `InstanceAxios`
* **Cakupan**: `overtime-service.js` (6), `benefits-service.js` (7).
* **Dependency**: —
* **Acceptance criteria**: `AC-S`, ditambah: Klaim memakai grup `hr-benefits/claims`.
* **Bukti verifikasi**: `V`, ditambah uji panggilan API yang gagal dan yang berhasil
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### 🟡 `FE-HRD-R12` — Bangun `S-D1` MVP-7: Pengaturan, Formasi, Batch Formasi
* **Slice / Gelombang**: S-D1 / 1
* **Task asal**: `FE-HRD-029`, `030`, `031`
* **Outcome**: HR dapat mengatur perencanaan dan melihat formasi resmi.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v7 bagian 13
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sesuai kartu asal. Dibangun langsung memakai base component.
* **Dependency**: —
* **Acceptance criteria**: `AC-L`, ditambah: Seluruh kriteria kartu asal terpenuhi.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: nama menu `HRD-FE-04` masih terbuka. Mitigasi: label dipusatkan. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### 🟡 `FE-HRD-R13` — Bangun `S-B1` MVP-21: Monitoring Presensi
* **Slice / Gelombang**: S-B1 / 1
* **Task asal**: `FE-HRD-088`–`091`, `099`
* **Outcome**: HR memantau kehadiran harian dan log biometrik mentah.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v11 bagian 17
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sesuai kartu asal. `attendance-monitoring-view.jsx` lama ikut diselaraskan.
* **Dependency**: —
* **Acceptance criteria**: `AC-L`, ditambah: Seluruh kriteria kartu asal terpenuhi.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R14` — Layar K3RS: sambung dan migrasi (pola rujukan)
* **Slice / Gelombang**: S-C6 / 2
* **Task asal**: `FE-HRD-262`–`274`
* **Outcome**: Seluruh layar K3RS memakai data nyata dan komponen standar, sekaligus menjadi rujukan migrasi slice lain.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v23 bagian 29
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 13 berkas, 150 pelanggaran. Termasuk `ohs-mcu-view.jsx` dan `ohs-injuries-view.jsx`.
* **Dependency**: `FE-HRD-R02`
* **Acceptance criteria**: `AC-L`, ditambah: Data medis hanya tampil bagi pemegang `OccupationalHealthRecord : ViewClinicalDetail`. Pola migrasi ditulis pada laporan agar dipakai `FE-HRD-R37` s.d. `R41`.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: privasi medis. Mitigasi: uji akun tanpa hak. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R15` — Layar Hubungan Industrial: sambung dan migrasi
* **Slice / Gelombang**: S-C5 / 2
* **Task asal**: `FE-HRD-248`–`260`
* **Outcome**: Kasus disiplin dan keluhan tersimpan nyata dan tampil seragam.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v22 bagian 28
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 12 berkas, 163 pelanggaran, 59 class CSS hilang.
* **Dependency**: `FE-HRD-R03`
* **Acceptance criteria**: `AC-L`, ditambah: Dialog tampil dengan latar gelap dan tidak lagi berupa blok biasa.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko sedang. Slice dengan pelanggaran terbanyak. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R16` — Layar Kinerja: sambung dan migrasi
* **Slice / Gelombang**: S-C3 / 2
* **Task asal**: `FE-HRD-232`–`244`
* **Outcome**: Kinerja pegawai tampil dari data nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v21 bagian 27
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 15 berkas, 96 pelanggaran.
* **Dependency**: `FE-HRD-R04`
* **Acceptance criteria**: `AC-L`, ditambah: Seluruh kriteria kartu asal terpenuhi.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R17` — Layar Pelatihan: sambung dan migrasi
* **Slice / Gelombang**: S-C2 / 2
* **Task asal**: `FE-HRD-217`–`229`
* **Outcome**: Radar sertifikat menampilkan staf yang sebenarnya.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v20 bagian 26
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 14 berkas, 94 pelanggaran.
* **Dependency**: `FE-HRD-R05`
* **Acceptance criteria**: `AC-L`, ditambah: Contoh: staf dengan sertifikat habis H-30 tampil pada radar sesuai data server.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R18` — Kredensial bagian 1: ESS, PSV, radar, katalog
* **Slice / Gelombang**: S-C1 / 2
* **Task asal**: `FE-HRD-187`–`190`
* **Outcome**: Dokter mengajukan kredensial dan staf memverifikasi sumber primer dengan data nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v18 bagian 24
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Pakai `credentialing-slice.js` yang sudah ada. Hapus daftar contoh di view.
* **Dependency**: `FE-HRD-R06`
* **Acceptance criteria**: `AC-L`, ditambah: Radar SIP membaca data server.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: keselamatan klinis. Mitigasi: uji per peran komite. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R19` — Layanan SDM bagian 1: katalog dan helpdesk
* **Slice / Gelombang**: S-D4 / 2
* **Task asal**: `FE-HRD-173`–`177`
* **Outcome**: Katalog layanan dan antrean tiket memakai data nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v17 bagian 23
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Pakai `hr-service-slice.js` yang sudah ada.
* **Dependency**: `FE-HRD-R06`
* **Acceptance criteria**: `AC-L`, ditambah: Target SLA tersimpan di server.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R20` — OPPE bagian 1: dasbor, periode, peer review
* **Slice / Gelombang**: S-C1-OPPE / 2
* **Task asal**: `FE-HRD-202`–`206`
* **Outcome**: Evaluasi kinerja klinis memakai data nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v19 bagian 25
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sambungkan service, migrasi 5 berkas modal dan view.
* **Dependency**: `FE-HRD-R07`
* **Acceptance criteria**: `AC-L`, ditambah: Akses dokumen peer review tercatat lewat `peer-review-audit/log-access`.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: kerahasiaan peer review. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R21` — Persetujuan bagian 1: kotak masuk, detail, aksi
* **Slice / Gelombang**: S-A7-V2 / 2
* **Task asal**: `FE-HRD-277`–`283`
* **Outcome**: Atasan menyetujui permohonan dan keputusannya benar-benar diteruskan.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract bagian 30
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Kotak masuk, tab, kartu metrik, detail, alur tahapan, modal setujui, tolak, dan kembalikan.
* **Dependency**: `FE-HRD-R08`
* **Acceptance criteria**: `AC-L`, ditambah: Contoh: setelah Setujui dan halaman dimuat ulang, status tetap disetujui.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko tinggi. Kotak masuk dipakai semua modul. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R22` — Perjalanan Dinas bagian 1: dasbor, tarif, SPPD
* **Slice / Gelombang**: S-D5 / 2
* **Task asal**: `FE-HRD-292`–`297`
* **Outcome**: SPPD dan tarif berasal dari server.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract bagian 31
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Hapus `MOCK_*` dari `businessTravelSlice.js`.
* **Dependency**: `FE-HRD-R09`
* **Acceptance criteria**: `AC-L`, ditambah: Kalkulator lumpsum memakai tarif server.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R23` — Lembur bagian 1: pengajuan dan persetujuan ruangan
* **Slice / Gelombang**: S-B3 / 2
* **Task asal**: `FE-HRD-117`–`120`
* **Outcome**: Perawat mengajukan lembur dan Kepala Ruangan menyetujuinya secara nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v13 bagian 19
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Pakai `overtime-slice.js`. Hapus `USER-SUPERVISOR-01`, ambil identitas dari sesi.
* **Dependency**: `FE-HRD-R11`
* **Acceptance criteria**: `AC-L`, ditambah: Pemohon tidak bisa menyetujui permohonannya sendiri.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: dampak ke gaji. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R24` — Benefit: sambung sisa dan migrasi
* **Slice / Gelombang**: S-D3 / 2
* **Task asal**: `FE-HRD-161`, `167`, `168`, `170`
* **Outcome**: Dasbor, buku besar, dan klaim COB memakai data nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v16 bagian 22
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 15 berkas, 58 pelanggaran.
* **Dependency**: `FE-HRD-R11`
* **Acceptance criteria**: `AC-L`, ditambah: Klaim COB tidak dapat dikirim tanpa SEP BPJS.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### 🟡 `FE-HRD-R25` — Onboarding dan Kontrak: migrasi
* **Slice / Gelombang**: S-C4-ONB / 2
* **Task asal**: `FE-HRD-059`–`073`
* **Outcome**: Tampilan seragam dan prop benar.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v9 bagian 15
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 15 berkas, 17 pelanggaran, 37 prop salah.
* **Dependency**: `FE-HRD-R10`
* **Acceptance criteria**: `AC-L`, ditambah: Tombol Batal tampil berbeda dari tombol Simpan.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`
* **Status**: 🟡 **SEBAGIAN 12 November 2026.** Enam dari tujuh kriteria R25 terpetakan ke source; lint lulus, build final lintas R25–R29 menghasilkan 813 halaman, dan CSS gate 8 berkas/0 temuan. Uji browser belum dapat dijalankan dan endpoint unggah/template batch belum tersedia. Bukti: [laporan](../task/report/frontend/FE-HRD-R25.md).

#### 🟡 `FE-HRD-R26` — Bangun `S-D1` MVP-8: Angka Beban, Standar, Gap
* **Slice / Gelombang**: S-D1 / 2
* **Task asal**: `FE-HRD-032`–`035`
* **Outcome**: HR melihat gap tenaga per unit dan jabatan.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v7 bagian 13
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sesuai kartu asal.
* **Dependency**: `FE-HRD-R12`
* **Acceptance criteria**: `AC-L`, ditambah: Seluruh kriteria kartu asal terpenuhi.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`
* **Status**: 🟡 **SEBAGIAN 12 November 2026.** Empat layar dan menu tersambung ke kontrak backend; ESLint 0 error/warning, build final lintas R25–R29 menghasilkan 813 halaman, dan CSS gate 4 berkas/0 temuan. Uji browser dengan akun berhak/tidak berhak belum dapat dijalankan. Bukti: [laporan](../task/report/frontend/FE-HRD-R26.md).

#### 🟡 `FE-HRD-R27` — Bangun `S-D1` MVP-10: Rencana Tahunan
* **Slice / Gelombang**: S-D1 / 2
* **Task asal**: `FE-HRD-041`, `042`
* **Outcome**: Rencana tenaga tahunan dapat disusun.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v7 bagian 13
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sesuai kartu asal.
* **Dependency**: `FE-HRD-R12`
* **Acceptance criteria**: `AC-L`, ditambah: Biaya selalu berlabel "estimasi".
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`
* **Status**: 🟡 **SEBAGIAN 12 November 2026.** Daftar, penyusun, addendum, kalkulasi, biaya estimasi, Finance, dan potret gap terpasang; endpoint cancel/availableActions Finance serta uji browser belum tersedia. [Laporan](../task/report/frontend/FE-HRD-R27.md).

#### 🟡 `FE-HRD-R28` — Bangun `S-B1` MVP-22: Koreksi, Izin, Dinas Luar
* **Slice / Gelombang**: S-B1 / 2
* **Task asal**: `FE-HRD-095`–`098`, `100`
* **Outcome**: HR mengelola koreksi presensi dan perizinan.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v11 bagian 17
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sesuai kartu asal.
* **Dependency**: `FE-HRD-R13`
* **Acceptance criteria**: `AC-L`, ditambah: Koreksi atas nama wajib menyertakan bukti.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`
* **Status**: 🟡 **SEBAGIAN 12 November 2026.** Tiga layar admin aktif; modal koreksi kuasa berkas dan ekstensi ESS belum aman karena kontrak backend. [Laporan](../task/report/frontend/FE-HRD-R28.md).

#### 🟡 `FE-HRD-R29` — Bangun `S-B1` MVP-23: Penutupan Periode
* **Slice / Gelombang**: S-B1 / 2
* **Task asal**: `FE-HRD-092`–`094`
* **Outcome**: Periode presensi dapat ditutup dan dibuka kembali.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v11 bagian 17
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sesuai kartu asal.
* **Dependency**: `FE-HRD-R13`
* **Acceptance criteria**: `AC-L`, ditambah: Penutupan ditolak bila masih ada koreksi tertunda.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: dampak ke gaji. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`
* **Status**: 🟡 **SEBAGIAN 12 November 2026.** Guard tutup, issue pemblokir, dan buka darurat terpasang; metrik rekap kartu tidak ada di DTO dan uji browser belum tersedia. [Laporan](../task/report/frontend/FE-HRD-R29.md).

#### `FE-HRD-R30` — Kredensial bagian 2: komite, direktur, registri
* **Slice / Gelombang**: S-C1 / 3
* **Task asal**: `FE-HRD-191`–`195`, `198`, `199`
* **Outcome**: Alur komite sampai SPK dan RKK berjalan dengan data nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v18 bagian 24
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sisa layar `S-C1` dan halaman verifikasi publik.
* **Dependency**: `FE-HRD-R18`
* **Acceptance criteria**: `AC-L`, ditambah: Penilai tidak dapat menilai dirinya sendiri.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: keselamatan klinis. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R31` — Layanan SDM bagian 2: ESS, surat, verifikasi publik
* **Slice / Gelombang**: S-D4 / 3
* **Task asal**: `FE-HRD-178`–`184`
* **Outcome**: Pegawai mengajukan layanan dan surat dapat diverifikasi publik.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v17 bagian 23
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sisa layar `S-D4`.
* **Dependency**: `FE-HRD-R19`
* **Acceptance criteria**: `AC-L`, ditambah: Halaman publik tidak menampilkan gaji tanpa PIN.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: privasi gaji. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R32` — OPPE bagian 2: FPPE dan rekredensial
* **Slice / Gelombang**: S-C1-OPPE / 3
* **Task asal**: `FE-HRD-207`–`214`
* **Outcome**: Pemantauan FPPE dan rekredensial memakai data nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v19 bagian 25
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sisa layar `S-C1-OPPE`.
* **Dependency**: `FE-HRD-R20`
* **Acceptance criteria**: `AC-L`, ditambah: Daftar kasus pleno menunggu endpoint pada `FE-HRD-R53`.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko sedang. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R33` — Persetujuan bagian 2: delegasi, matriks, ESS
* **Slice / Gelombang**: S-A7-V2 / 3
* **Task asal**: `FE-HRD-284`–`289`
* **Outcome**: Delegasi Plh dan portal ESS berjalan nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract bagian 30
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sisa layar `S-A7-V2`.
* **Dependency**: `FE-HRD-R21`
* **Acceptance criteria**: `AC-L`, ditambah: Pencabutan delegasi memakai `PUT delegations/{id}/revoke`.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko sedang. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R34` — Perjalanan Dinas bagian 2: uang muka, settlement, klaim
* **Slice / Gelombang**: S-D5 / 3
* **Task asal**: `FE-HRD-298`–`304`
* **Outcome**: Uang muka dan settlement tercatat nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract bagian 31
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sisa layar `S-D5`.
* **Dependency**: `FE-HRD-R22`
* **Acceptance criteria**: `AC-L`, ditambah: Radar overdue menunggu endpoint pada `FE-HRD-R53`.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: dampak keuangan. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R35` — Lembur bagian 2: verifikasi, CTO, penutupan
* **Slice / Gelombang**: S-B3 / 3
* **Task asal**: `FE-HRD-121`–`128`
* **Outcome**: Verifikasi, saldo CTO, dan penutupan periode berjalan nyata.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v13 bagian 19
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sisa layar `S-B3`.
* **Dependency**: `FE-HRD-R23`
* **Acceptance criteria**: `AC-L`, ditambah: Saldo CTO 90 hari berasal dari server.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: dampak ke gaji. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R36` — Bangun `S-D1` MVP-9: Headcount dan Antrean Rekrutmen
* **Slice / Gelombang**: S-D1 / 3
* **Task asal**: `FE-HRD-036`–`040`
* **Outcome**: Permintaan headcount tersambung ke antrean rekrutmen.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v7 bagian 13
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Sesuai kartu asal.
* **Dependency**: `FE-HRD-R26`
* **Acceptance criteria**: `AC-L`, ditambah: Seluruh kriteria kartu asal terpenuhi.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R37` — Migrasi base component Cuti
* **Slice / Gelombang**: S-B2 / 3
* **Task asal**: `FE-HRD-103`–`114`
* **Outcome**: Tampilan Cuti seragam.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v12 bagian 18
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 14 berkas, 67 pelanggaran. Data sudah tersambung.
* **Dependency**: `FE-HRD-R14`
* **Acceptance criteria**: `AC-L`, ditambah: Fungsi cuti tidak berubah.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: regresi saldo cuti. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R38` — Migrasi base component Roster
* **Slice / Gelombang**: S-B4 / 3
* **Task asal**: `FE-HRD-077`–`085`
* **Outcome**: Tampilan Roster seragam, kisi tetap khusus.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v10 bagian 16
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 10 berkas, 39 pelanggaran.
* **Dependency**: `FE-HRD-R14`
* **Acceptance criteria**: `AC-L`, ditambah: Kisi roster tetap dipertahankan sebagai elemen unik.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R39` — Migrasi Rekrutmen dan perbaikan isian UUID
* **Slice / Gelombang**: S-D2 / 3
* **Task asal**: `FE-HRD-044`–`056`
* **Outcome**: Petugas memilih nama, tidak mengetik UUID.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v8 bagian 14
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`, `ResourceFilterSelect`
* **Cakupan**: 13 berkas, 32 pelanggaran, 13 isian UUID.
* **Dependency**: `FE-HRD-R14`
* **Acceptance criteria**: `AC-L`, ditambah: Contoh: isian Pelamar menampilkan nama yang dapat dicari.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R40` — Migrasi Restitusi dan pasang layar yang tertinggal
* **Slice / Gelombang**: S-B6 / 3
* **Task asal**: `FE-HRD-146`–`156`
* **Outcome**: Antrean persetujuan klaim bisa dibuka.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v15 bagian 21
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 10 berkas, 42 pelanggaran. Buat rute `deductions/approvals`, pasang `ModalClaimDocumentPreview` dan `ModalVoucherQrViewer` (`FE-HRD-152`, `153`, `155`).
* **Dependency**: `FE-HRD-R14`
* **Acceptance criteria**: `AC-L`, ditambah: Ketiga komponen dapat dibuka dari menu.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R41` — Migrasi layar administrasi dan ESS lama
* **Slice / Gelombang**: S-A1 s.d. S-A7, S-C4, FE-020 / 3
* **Task asal**: `FE-HRD-002`–`008`, `017`, `020`
* **Outcome**: Sisa layar lama seragam.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v6
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: 38 berkas, 41 pelanggaran. Termasuk layar slip gaji ESS yang masih dipakai (`payslips-ess-view.jsx`, `PayslipPinKeypadModal.jsx`).
* **Dependency**: `FE-HRD-R14`
* **Acceptance criteria**: `AC-L`, ditambah: Fungsi layar tidak berubah.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R42` — Verifikasi `S-D1`
* **Slice / Gelombang**: S-D1 / 4
* **Task asal**: `FE-HRD-043`
* **Outcome**: Menu, hak akses, dan keterjangkauan `S-D1` terbukti.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v7
* **Reuse**: `live-browser-testing`
* **Cakupan**: Seluruh layar `S-D1`.
* **Dependency**: `FE-HRD-R27`, `FE-HRD-R36`
* **Acceptance criteria**: Seluruh layar terjangkau dari menu.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R43` — Verifikasi `S-B1`
* **Slice / Gelombang**: S-B1 / 3
* **Task asal**: `FE-HRD-101`
* **Outcome**: Menu, hak akses, dan keterjangkauan `S-B1` terbukti.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v11
* **Reuse**: `live-browser-testing`
* **Cakupan**: Seluruh layar `S-B1`.
* **Dependency**: `FE-HRD-R28`, `FE-HRD-R29`
* **Acceptance criteria**: Seluruh layar terjangkau dari menu.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R44` — Verifikasi layar klinis dan keselamatan
* **Slice / Gelombang**: S-C6, S-C5, S-C1, S-C1-OPPE / 4
* **Task asal**: `FE-HRD-200`, `215`, `275`
* **Outcome**: Alur klinis terbukti berjalan.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: Kontrak masing-masing slice
* **Reuse**: `live-browser-testing`
* **Cakupan**: Uji browser P0 dan P1.
* **Dependency**: `FE-HRD-R14`, `FE-HRD-R15`, `FE-HRD-R30`, `FE-HRD-R32`
* **Acceptance criteria**: Skenario keberhasilan dan kegagalan server diuji.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: butuh data uji. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R45` — Verifikasi layar operasional
* **Slice / Gelombang**: S-A7-V2, S-D5, S-D4, S-B3 / 4
* **Task asal**: `FE-HRD-290`, `305`, `185`, `129`
* **Outcome**: Alur persetujuan, dinas, layanan, dan lembur terbukti.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: Kontrak masing-masing slice
* **Reuse**: `live-browser-testing`
* **Cakupan**: Uji browser P0 dan P1.
* **Dependency**: `FE-HRD-R31`, `FE-HRD-R33`, `FE-HRD-R34`, `FE-HRD-R35`
* **Acceptance criteria**: Keputusan tetap tersimpan setelah halaman dimuat ulang.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: butuh data uji. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R46` — Verifikasi layar pengembangan dan administrasi
* **Slice / Gelombang**: S-C2, S-C3, S-D3, S-C4-ONB, S-B2, S-B4, S-D2, S-B6, lama / 4
* **Task asal**: `FE-HRD-230`, `245`, `171`, `074`, `115`, `087`, `057`, `157`
* **Outcome**: Sisa slice terbukti tidak regresi.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: Kontrak masing-masing slice
* **Reuse**: `live-browser-testing`
* **Cakupan**: Uji browser P1 dan P2.
* **Dependency**: `FE-HRD-R16`, `FE-HRD-R17`, `FE-HRD-R24`, `FE-HRD-R25`, `FE-HRD-R37`, `FE-HRD-R38`, `FE-HRD-R39`, `FE-HRD-R40`, `FE-HRD-R41`
* **Acceptance criteria**: Tidak ada regresi fungsi.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### `FE-HRD-R47` — Audit akhir dan pembaruan status
* **Slice / Gelombang**: Semua / 5
* **Task asal**: Seluruh task
* **Outcome**: Status roadmap sesuai kenyataan.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: Seluruh kontrak
* **Reuse**: `verify-module-readiness`
* **Cakupan**: Audit ulang, isi laporan yang kurang (termasuk `FE-HRD-021`), perbarui manifest.
* **Dependency**: `FE-HRD-R01`, `FE-HRD-R42`, `FE-HRD-R43`, `FE-HRD-R44`, `FE-HRD-R45`, `FE-HRD-R46`
* **Acceptance criteria**: Verdict berdasarkan bukti.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R48` — Base component `BaseDrawer`
* **Slice / Gelombang**: Bersama / ⛔ tertahan keputusan
* **Task asal**: Kelompok F
* **Outcome**: Panel geser seragam.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: —
* **Reuse**: `ConfirmModal` sebagai rujukan
* **Cakupan**: Komponen baru di `base-features`.
* **Dependency**: `{DEC-FER-01}`
* **Acceptance criteria**: `AC-L`, ditambah: Mendukung tutup dengan tombol dan Esc.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R49` — Base component `BaseFileUpload`
* **Slice / Gelombang**: Bersama / ⛔ tertahan keputusan
* **Task asal**: Kelompok F
* **Outcome**: Unggah berkas seragam.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: —
* **Reuse**: `BaseFormControl`
* **Cakupan**: Komponen baru di `base-features`.
* **Dependency**: `{DEC-FER-02}`
* **Acceptance criteria**: `AC-L`, ditambah: Contoh: berkas 6 MB ditolak dengan pesan batas 5 MB.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R50` — Adopsi drawer dan unggah berkas di HR
* **Slice / Gelombang**: Bersama / ⛔ tertahan keputusan
* **Task asal**: Kelompok F
* **Outcome**: 22 drawer dan 10 unggahan memakai komponen standar.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: —
* **Reuse**: `BaseDrawer`, `BaseFileUpload`
* **Cakupan**: Seluruh berkas Lampiran B yang memuat drawer dan input file.
* **Dependency**: `FE-HRD-R48`, `FE-HRD-R49`
* **Acceptance criteria**: `AC-L`, ditambah: Tidak ada overlay buatan sendiri tersisa.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: menyentuh banyak slice. Mitigasi: kerjakan setelah migrasi slice. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R51` — Layar anomali dan pengaturan Payroll
* **Slice / Gelombang**: S-B5 / ⛔ tertahan keputusan
* **Task asal**: `FE-HRD-135`, `142`
* **Outcome**: HR memeriksa anomali gaji sebelum persetujuan.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v14 bagian 20
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Mengikuti `DEC-FER-03`.
* **Dependency**: `{DEC-FER-03}`
* **Acceptance criteria**: `AC-L`, ditambah: Layar dapat dibuka dari menu Payroll.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: dampak ke gaji. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R52` — Penataan rute utama dan menu
* **Slice / Gelombang**: Semua / ⛔ tertahan keputusan
* **Task asal**: Kelompok D
* **Outcome**: Satu layar punya satu alamat utama.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: —
* **Reuse**: `menu-items.jsx`, `next.config.js`
* **Cakupan**: Rute tanpa tautan: `/hrd/occupational-health`, `/hrd/separations/exit-clearance`, `/corporate/hrd/benefits`, `/corporate/hrd/services/dashboard`, `/ess/credentialing/application`, `/ess/my-documents`, `/ess/services/requests`, `/hr/attendance`, `/hr/benefits`, `/hr/employee-relations/disciplinary-actions`, `/hr/initial-placement-batch`, `/hr/learning/training-records`, `/hr/leave/adjustments`, `/hr/overtime/reconciliation`, `/hr/performance/reviews`, `/hr/personnel-action-orders`, `/hr/workforce-core/employee-profile-changes/on-behalf`, `/hr/workforce-core/personnel-actions/create`, `/portal/learning/certificates`, `/self-services/human-resource/attendance`, `/self-services/human-resource/attendance-correction`, `/self-services/human-resource/leave`, `/self-services/human-resource/overtime`, `/self-services/human-resource/schedule`.
* **Dependency**: `{DEC-FER-04}`
* **Acceptance criteria**: `AC-L`, ditambah: Alamat lama dialihkan ke alamat utama.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: bookmark lama. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R53` — Layar daftar yang menunggu endpoint baru
* **Slice / Gelombang**: S-C2, S-C3, S-C5, S-C6, S-D5, S-D3, S-B3 / ⛔ tertahan keputusan
* **Task asal**: Lampiran C
* **Outcome**: Layar daftar lintas-pegawai berfungsi.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: Revisi kontrak
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: Endpoint berlabel `Rencana (belum tersedia)` pada Lampiran C. Perlu task backend.
* **Dependency**: `{DEC-FER-05}`
* **Acceptance criteria**: `AC-L`, ditambah: Tidak ada lagi tulisan "data belum tersedia" karena endpoint kosong.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: perlu perubahan backend dan hak akses baru. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R54` — Pasang modal SIMRS
* **Slice / Gelombang**: S-C1 / ⛔ tertahan keputusan
* **Task asal**: `FE-HRD-196`, `197`
* **Outcome**: Pengaman wewenang klinis aktif saat order.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v18 bagian 24
* **Reuse**: `ConfirmModal`
* **Cakupan**: Mengikuti `DEC-FER-06`.
* **Dependency**: `{DEC-FER-06}`
* **Acceptance criteria**: `AC-L`, ditambah: Dokter tanpa wewenang tidak bisa melanjutkan order.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko tinggi: menyentuh modul klinis. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R55` — Penutupan `FE-HRD-014` dan `019`
* **Slice / Gelombang**: S-C1, S-C6 / ⛔ tertahan keputusan
* **Task asal**: `FE-HRD-014`, `019`
* **Outcome**: Status kedua task menjadi jelas.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: —
* **Reuse**: —
* **Cakupan**: Dokumen saja.
* **Dependency**: `{DEC-FER-07}`
* **Acceptance criteria**: Keputusan tercatat di decision log.
* **Bukti verifikasi**: Tinjauan dokumen oleh pemilik
* **Risiko & pemilik**: Risiko kecil. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`

#### ⛔ `FE-HRD-R56` — Sisa task sebagian
* **Slice / Gelombang**: S-A1, S-B3, S-C2, S-C3, S-C5 / ⛔ tertahan keputusan
* **Task asal**: `FE-HRD-011`, `015`, `016`, `018`, `026`, `027`
* **Outcome**: Sisa kriteria lama tuntas.
* **Requirement/decision**: kartu asal pada `frontend-roadmap.md`; temuan audit dokumen ini
* **Kontrak**: API Contract v6
* **Reuse**: `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `BaseFormControl`, `InformationAlert`, `ToastStack`
* **Cakupan**: `FE-HRD-026`: alamatnya kini 7 dari 7 cocok, jadi perlu diuji ulang. Lainnya mengikuti `K-02` dan `K-03`.
* **Dependency**: `{K-02}`, `{K-03}`
* **Acceptance criteria**: `AC-L`, ditambah: Kriteria kartu asal terbukti.
* **Bukti verifikasi**: `V`
* **Risiko & pemilik**: Risiko: bergantung keputusan lama. Pemilik: Frontend HR UI Lead.
* **Definition of Done**: `DoD`



---

## Traceability

| Slice | Decision dan kontrak | Task asal | Task perbaikan (`FE-HRD-…`) | Bukti saat ini | Status |
|---|---|---|---|---|---|
| `S-A7-V2` | Kontrak bagian 30 | `FE-HRD-276`–`290` | `R08`, `R21`, `R33`, `R45` | Audit source 11 November 2026 | Belum |
| `S-B1` | `HRD-DEC-104`–`109`, v11 | `FE-HRD-088`–`101` | `R13`, `R28`, `R29`, `R43` | Audit source 11 November 2026 | Belum |
| `S-B2` | `HRD-DEC-110`–`115`, v12 | `FE-HRD-102`–`115` | `R37`, `R46` | Audit source 11 November 2026 | Belum |
| `S-B3` | `HRD-DEC-116`–`121`, v13 | `FE-HRD-116`–`129` | `R11`, `R23`, `R35`, `R45` | Audit source 11 November 2026 | Belum |
| `S-B4` | `HRD-DEC-097`–`103`, v10 | `FE-HRD-075`–`087` | `R38`, `R46` | Audit source 11 November 2026 | Belum |
| `S-B5` | `HRD-DEC-122`–`127`, v14 | `FE-HRD-130`–`143` | `R51` | Audit source 11 November 2026 | Belum |
| `S-B6` | `HRD-DEC-128`–`133`, v15 | `FE-HRD-144`–`157` | `R40`, `R46` | Audit source 11 November 2026 | Belum |
| `S-C1` | v18 | `FE-HRD-186`–`200` | `R06`, `R18`, `R30`, `R44`, `R54` | Audit source 11 November 2026 | Belum |
| `S-C1-OPPE` | v19 | `FE-HRD-201`–`215` | `R07`, `R20`, `R32`, `R44` | Audit source 11 November 2026 | Belum |
| `S-C2` | v20 | `FE-HRD-216`–`230` | `R05`, `R17`, `R46` | Audit source 11 November 2026 | Belum |
| `S-C3` | v21 | `FE-HRD-231`–`245` | `R04`, `R16`, `R46` | Audit source 11 November 2026 | Belum |
| `S-C4-ONB` | `HRD-DEC-081`–`090`, `095`, `096`, v9 | `FE-HRD-058`–`074` | `R10`, `R25`, `R46` | Audit source 11 November 2026 | Belum |
| `S-C5` | v22 | `FE-HRD-246`–`260` | `R03`, `R15`, `R44` | Audit source 11 November 2026 | Belum |
| `S-C6` | v23 | `FE-HRD-261`–`275` | `R02`, `R14`, `R44` | Audit source 11 November 2026 | Belum |
| `S-D1` | `HRD-DEC-058`–`072`, v7 | `FE-HRD-029`–`043` | `R12`, `R26`, `R27`, `R36`, `R42` | Audit source 11 November 2026 | Belum |
| `S-D2` | `HRD-DEC-073`–`080`, v8 | `FE-HRD-044`–`057` | `R39`, `R46` | Audit source 11 November 2026 | Belum |
| `S-D3` | `HRD-DEC-134`–`139`, v16 | `FE-HRD-158`–`171` | `R11`, `R24`, `R46` | Audit source 11 November 2026 | Belum |
| `S-D4` | v17 | `FE-HRD-172`–`185` | `R06`, `R19`, `R31`, `R45` | Audit source 11 November 2026 | Belum |
| `S-D5` | Kontrak bagian 31 | `FE-HRD-291`–`305` | `R09`, `R22`, `R34`, `R45` | Audit source 11 November 2026 | Belum |


Masukan: `frontend-roadmap.md` revisi `16`, `blueprint-manifest.md` revisi `59`, `contracts/api-contract.md`
bagian yang disebut, source frontend `d09dc74da`, dan source backend `f751f8ca`.

---

## Batas audit

- Build, lint, dan uji browser belum dijalankan. "Tersambung API" berarti kodenya memanggil API lewat sesi aplikasi, bukan sudah terbukti lulus seluruh kriteria.
- Klasifikasi elemen memakai pola kode. Kategori "Melanggar" dibuat konservatif, kasus abu-abu masuk "Perlu ditinjau".
- Padanan endpoint pada Lampiran C berasal dari pencocokan nama. Builder wajib mengonfirmasinya terhadap kontrak.
- Tiga panggilan pada `employee-benefit-enrollment-service.js` memakai alamat dari berkas konstanta dan tidak ikut dinilai.
- Kesetaraan layar Payroll lama dan baru dinilai dari nama dan fungsi berkas, belum diuji terhadap kriteria.

---

## Lampiran A — Status per task

| Task | Slice | Klaim roadmap | Hasil audit | Laporan | Judul |
|---|---|---|---|:--:|---|
| FE-HRD-001 | S0-B | Selesai | Navigasi, tidak dinilai | ada | Penyelarasan Rute Kebab-Case pada Endpoint Constants Master Data |
| FE-HRD-002 | S-A1 | Selesai | Tersambung API | ada | Layar Daftar Lintas-Pegawai Administrasi Kepegawaian |
| FE-HRD-003 | S-A2 | Selesai | Tersambung API | ada | Portal Layanan Mandiri Cuti Pegawai (ESS) |
| FE-HRD-004 | S-A3 | Selesai | Tersambung API | ada | Portal Layanan Mandiri Lembur Pegawai (ESS) |
| FE-HRD-005 | S-A4 | Selesai | Tersambung API | ada | Portal Layanan Mandiri Ubah Jadwal & Tukar Shift (ESS) |
| FE-HRD-006 | S-A5 | Selesai | Tersambung API | ada | Portal Layanan Mandiri Koreksi Absensi Mandiri (ESS) |
| FE-HRD-007 | S-A6 | Selesai | Tersambung API | ada | Migrasi & Konsolidasi Form Absensi Harian ke Standar Self-Service Karyawan |
| FE-HRD-008 | S-A7 | Selesai | Tersambung API | ada | Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox) |
| FE-HRD-009 | S-B1 | Selesai | Tersambung API | ada | Layar Administrasi Monitoring Presensi & Rekapitulasi Kehadiran HR |
| FE-HRD-010 | S-B2 | Selesai | Tersambung API | ada | Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan |
| FE-HRD-011 | S-B3 | Sebagian | Sebagian | ada | Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR |
| FE-HRD-012 | S-B4 | Selesai | Tersambung API | ada | Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7 |
| FE-HRD-013 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Layar Penggajian Restitusi & Potongan Cicilan Piutang RS |
| FE-HRD-014 | S-C1 | Terblokir | Terblokir | tidak | Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter |
| FE-HRD-015 | S-C2 | Sebagian | Sebagian | ada | Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi |
| FE-HRD-016 | S-C3 | Sebagian | Sebagian | ada | Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review |
| FE-HRD-017 | S-C4 | Selesai | Tersambung API | ada | Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan |
| FE-HRD-018 | S-C5 | Sebagian | Sebagian | ada | Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan |
| FE-HRD-019 | S-C6 | Terblokir | Terblokir | tidak | Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS |
| FE-HRD-020 | S-D3 | Selesai | Tersambung API | ada | Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan |
| FE-HRD-021 | IN-01 | Selesai | Tanpa laporan | tidak | Pemeliharaan dan Standarisasi 62 Layar Master Data HR |
| FE-HRD-022 | S-A1 | Selesai | Tersambung API | ada | Layar Daftar Dokumen SK Kepegawaian (/hr/workforce-core/personnel-action-orders) |
| FE-HRD-023 | S-A1 | Selesai | Tersambung API | ada | Form Penerbitan SK Kepegawaian Baru (/hr/workforce-core/personnel-action-orders/create) |
| FE-HRD-024 | S-A1 | Selesai | Tersambung API | ada | Layar Detail & Aksi Persetujuan SK (/hr/workforce-core/personnel-action-orders/[id]) |
| FE-HRD-025 | S-A1 | Selesai | Tersambung API | ada | Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View (/employee-360/[id]) |
| FE-HRD-026 | S-A1 | Sebagian | Sebagian | ada | Layar Batch Inisialisasi Penempatan Awal Pegawai (/initial-placement-batch) |
| FE-HRD-027 | S-A1 | Sebagian | Sebagian | ada | Form Permohonan Perubahan Data Kuasa HR (On-Behalf) (/employee-profile-changes/on-behalf) |
| FE-HRD-028 | S-A1 | Selesai | Tersambung API | ada | Penguncian Form Master Data Pegawai (Lock Unit & Jabatan Menjadi Read-Only) |
| FE-HRD-029 | S-D1 | Belum | Belum dikerjakan | tidak | Fondasi layar S-D1 dan layar Pengaturan Perencanaan |
| FE-HRD-030 | S-D1 | Belum | Belum dikerjakan | tidak | Layar Formasi Jabatan baca saja beserta riwayat versi |
| FE-HRD-031 | S-D1 | Belum | Belum dikerjakan | tidak | Layar Batch Formasi Awal dan detailnya |
| FE-HRD-032 | S-D1 | Belum | Belum dikerjakan | tidak | Layar Angka Beban |
| FE-HRD-033 | S-D1 | Belum | Belum dikerjakan | tidak | Layar Daftar Standar Kebutuhan Tenaga |
| FE-HRD-034 | S-D1 | Belum | Belum dikerjakan | tidak | Layar Penyusun dan Detail Standar beserta rekomendasi |
| FE-HRD-035 | S-D1 | Belum | Belum dikerjakan | tidak | Layar Pemantauan Gap Tenaga — gap langsung |
| FE-HRD-036 | S-D1 | Belum | Belum dikerjakan | tidak | Layar Daftar Permintaan Headcount |
| FE-HRD-037 | S-D1 | Belum | Belum dikerjakan | tidak | Form dan Detail Permintaan Headcount, serta tombol "Ajukan" di Pemantauan Gap |
| FE-HRD-038 | S-D1 | Belum | Belum dikerjakan | tidak | Aksi kursi pada detail permintaan: tautkan/lepas, ubah rencana pemenuhan, batalkan kursi |
| FE-HRD-039 | S-D1 | Belum | Belum dikerjakan | tidak | Layar Antrean Rekrutmen beserta dialog tautan pemenuhan |
| FE-HRD-040 | S-D1 | Belum | Belum dikerjakan | tidak | Panel potret gap pada Pemantauan Gap Tenaga |
| FE-HRD-041 | S-D1 | Sebagian | Tersambung API | ada | Daftar Rencana Tahunan dan Addendum; endpoint pembatalan belum tersedia ([R27](../task/report/frontend/FE-HRD-R27.md)) |
| FE-HRD-042 | S-D1 | Sebagian | Tersambung API | ada | Penyusun dan Detail Rencana; menunggu uji browser/peran ([R27](../task/report/frontend/FE-HRD-R27.md)) |
| FE-HRD-043 | S-D1 | Belum | Belum dikerjakan | tidak | Verifikasi menu, hak akses, dan keterjangkauan seluruh layar S-D1 |
| FE-HRD-044 | S-D2 | Selesai | Tersambung API | ada | Layar Antrean & Permintaan Requisition |
| FE-HRD-045 | S-D2 | Selesai | Tersambung API | ada | Layar Detail & Draf Requisition |
| FE-HRD-046 | S-D2 | Selesai | Tersambung API | ada | Layar Pengelolaan Lowongan Kerja (*Job Vacancies*) |
| FE-HRD-047 | S-D2 | Selesai | Tersambung API | ada | Layar Detail Lowongan & Pelamar Masuk |
| FE-HRD-048 | S-D2 | Selesai | Tersambung API | ada | Layar Direktori Data Pelamar & NIK Rehire Flow |
| FE-HRD-049 | S-D2 | Selesai | Tersambung API | ada | Layar Detail Pelamar & Penapisan STR Klinis Otomatis |
| FE-HRD-050 | S-D2 | Selesai | Tersambung API | ada | Layar Formulir Evaluasi Wawancara HR |
| FE-HRD-051 | S-D2 | Selesai | Tersambung API | ada | Layar Formulir Uji Klinis / Asesor Unit dengan Hak Veto Klinis |
| FE-HRD-052 | S-D2 | Selesai | Tersambung API | ada | Layar Daftar Penawaran Kerja (*Job Offers*) |
| FE-HRD-053 | S-D2 | Selesai | Tersambung API | ada | Layar Detail Penawaran & Rantai Persetujuan Bertingkat T13/T14 |
| FE-HRD-054 | S-D2 | Selesai | Tersambung API | ada | Layar Pemantauan Kelaikan MCU Pra-Kerja & Masking Medis K3RS |
| FE-HRD-055 | S-D2 | Selesai | Tersambung API | ada | Layar Bank Kandidat Cadangan (*Talent Pool*) & Fast-Track Promotion |
| FE-HRD-056 | S-D2 | Selesai | Tersambung API | ada | Layar Serah Terima ke Onboarding (*Hiring Handoff*) |
| FE-HRD-057 | S-D2 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi Menu, Hak Akses, dan Keterjangkauan Seluruh Layar Rekrutmen S-D2 |
| FE-HRD-058 | S-C4-ONB | Selesai | Non-layar | ada | Fondasi layanan API dan label status S-C4-ONB |
| FE-HRD-059 | S-C4-ONB | Selesai | Tersambung API | ada | Template Onboarding (FE-HRD-132, FE-HRD-133) dan grup menu |
| FE-HRD-060 | S-C4-ONB | Selesai | Tersambung API | ada | Daftar dan Detail Kasus Onboarding (FE-HRD-124, FE-HRD-125) |
| FE-HRD-061 | S-C4-ONB | Selesai | Tersambung API | ada | Aksi checklist pada Detail Kasus Onboarding |
| FE-HRD-062 | S-C4-ONB | Selesai | Tersambung API | ada | Siap kerja, batal bergabung, dan tautan dari layar serah terima |
| FE-HRD-063 | S-C4-ONB | Selesai | Tersambung API | ada | Daftar dan Detail Kontrak Kerja (FE-HRD-126, FE-HRD-127) |
| FE-HRD-064 | S-C4-ONB | Selesai | Tersambung API | ada | Keputusan dan koreksi kontrak pada Detail Kontrak Kerja |
| FE-HRD-065 | S-C4-ONB | Selesai | Tersambung API | ada | Masa Percobaan (FE-HRD-128, FE-HRD-129) |
| FE-HRD-066 | S-C4-ONB | Selesai | Tersambung API | ada | Pendampingan Klinis (FE-HRD-130, FE-HRD-131) |
| FE-HRD-067 | S-C4-ONB | Selesai | Tersambung API | ada | Batch Data Awal Kontrak (FE-HRD-134, FE-HRD-135) |
| FE-HRD-068 | S-C4-ONB | Selesai | Tersambung API | ada | Permintaan Akses Pegawai Baru (FE-HRD-136) |
| FE-HRD-069 | S-C4-ONB | Selesai | Tersambung API | ada | Pengaturan Onboarding dan Kontrak (FE-HRD-137) |
| FE-HRD-070 | S-C4-ONB | Selesai | Tersambung API | ada | Perlu Tindakan (FE-HRD-123) |
| FE-HRD-071 | S-C4-ONB | Selesai | Tersambung API | ada | Perubahan layar lama: tab riwayat kontrak, label "Probation", dan form master pegawai |
| FE-HRD-072 | S-D2 | Selesai | Tersambung API | ada | Amandemen layar tawaran S-D2: jenis dan lama kontrak |
| FE-HRD-073 | S-C4-ONB | Selesai | Tersambung API | ada | Empat jenis tugas T15 s.d. T18 di Kotak Masuk |
| FE-HRD-074 | S-C4-ONB | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi menu, hak akses, dan keterjangkauan seluruh layar S-C4-ONB |
| FE-HRD-075 | S-B4 | Selesai | Non-layar | ada | Service Client API Roster Penjadwalan (roster-scheduling-service.js diperbarui) |
| FE-HRD-076 | S-B4 | Selesai | Non-layar | ada | State Management Redux Roster Unit bulanan & cache invalidation |
| FE-HRD-077 | S-B4 | Selesai | Tersambung API | ada | Layar Kisi Roster Bulanan Unit (FE-HRD-138 /hr/scheduling/roster) |
| FE-HRD-078 | S-B4 | Selesai | Tersambung API | ada | Komponen Aksi Massal: Salin Periode Lalu & Terapkan Pola Shift |
| FE-HRD-079 | S-B4 | Selesai | Tersambung API | ada | Komponen Drawer Validasi Jadwal: Blokir B1–B7 & Peringatan W1–W7 dengan Dropdown Alasan |
| FE-HRD-080 | S-B4 | Selesai | Tersambung API | ada | Aksi UI Tombol Terbitkan Roster Mandiri Kepala Unit dengan Validasi Form Alasan |
| FE-HRD-081 | S-B4 | Selesai | Tersambung API | ada | Komponen Modal Ganti Petugas Cepat (*Quick Replacement*) dengan Alasan Wajib |
| FE-HRD-082 | S-B4 | Selesai | Tersambung API | ada | Aksi UI Koreksi Mundur Jadwal (<= 3 hari) & Proteksi Periode Terkunci (Closed Banner) |
| FE-HRD-083 | S-B4 | Selesai | Non-layar | ada | Service Client API On-Call & SLA (on-call-service.js) |
| FE-HRD-084 | S-B4 | Selesai | Tersambung API | ada | Layar Kalender Penugasan Siaga (*On-Call Coverage*) (FE-HRD-141 /hr/scheduling/on-call) |
| FE-HRD-085 | S-B4 | Selesai | Tersambung API | ada | Komponen Modal Panggil Siaga (*Call-In*) & Display Badge Pemenuhan SLA Menit |
| FE-HRD-086 | S-B4 | Selesai | Tersambung API | ada | Layar Pengaturan Kebijakan Penjadwalan HR (FE-HRD-143 /hr/scheduling/settings) |
| FE-HRD-087 | S-B4 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi menu, hak akses peran, dan keterjangkauan seluruh layar S-B4 |
| FE-HRD-088 | S-B1 | Belum | Belum dikerjakan | tidak | Service Client API Presensi, Perizinan, dan Pengaturan HR (attendance-service.js) |
| FE-HRD-089 | S-B1 | Belum | Belum dikerjakan | tidak | Redux Slice State Management Presensi Harian & Periode (attendanceSlice.js) |
| FE-HRD-090 | S-B1 | Belum | Belum dikerjakan | tidak | Layar Dasbor Administrasi & Monitoring Presensi Harian (FE-HRD-144 /hr/attendance) |
| FE-HRD-091 | S-B1 | Belum | Belum dikerjakan | tidak | Komponen Drawer Riwayat Log Mesin Biometrik Mentah (RawPunchAuditDrawer.jsx) |
| FE-HRD-092 | S-B1 | Sebagian | Tersambung API | ada | Layar Rekapitulasi & Penutupan Periode; metrik DTO belum lengkap ([R29](../task/report/frontend/FE-HRD-R29.md)) |
| FE-HRD-093 | S-B1 | Sebagian | Tersambung API | ada | Guard Penutupan Periode & Peringatan Koreksi Pending ([R29](../task/report/frontend/FE-HRD-R29.md)) |
| FE-HRD-094 | S-B1 | Sebagian | Tersambung API | ada | Modal Buka Kembali Periode Darurat HR Manager ([R29](../task/report/frontend/FE-HRD-R29.md)) |
| FE-HRD-095 | S-B1 | Sebagian | Tersambung API | ada | Monitoring koreksi; penanda jalur ESS/kuasa belum eksplisit di DTO ([R28](../task/report/frontend/FE-HRD-R28.md)) |
| FE-HRD-096 | S-B1 | Terblokir | Kontrak backend belum lengkap | tidak | Endpoint unggah bukti kuasa HR belum tersedia ([R28](../task/report/frontend/FE-HRD-R28.md)) |
| FE-HRD-097 | S-B1 | Sebagian | Tersambung API | ada | Pengelolaan Izin Pulang Cepat; jam tap aktual belum tersedia di DTO ([R28](../task/report/frontend/FE-HRD-R28.md)) |
| FE-HRD-098 | S-B1 | Sebagian | Tersambung API | ada | Pengelolaan Surat Penugasan Dinas Luar ([R28](../task/report/frontend/FE-HRD-R28.md)) |
| FE-HRD-099 | S-B1 | Belum | Belum dikerjakan | tidak | Layar Pengaturan Parameter Presensi & Toleransi Waktu HR (FE-HRD-149 /hr/attendance/settin |
| FE-HRD-100 | S-B1 | Terblokir | Kontrak backend belum tersedia | tidak | Endpoint self-service izin pulang cepat/dinas luar tidak ditemukan ([R28](../task/report/frontend/FE-HRD-R28.md)) |
| FE-HRD-101 | S-B1 | Belum | Belum dikerjakan | tidak | Verifikasi menu, hak akses peran, dan keterjangkauan seluruh layar S-B1 |
| FE-HRD-102 | S-B2 | Selesai | Non-layar | ada | Service Client API Cuti, Perizinan, dan Redux State Management (leave-service.js, leaveSli |
| FE-HRD-103 | S-B2 | Selesai | Tersambung API | ada | Komponen Metrik Kartu Saldo & Progress Bar Pemakaian Cuti Tahunan (LeaveBalanceMetricCards |
| FE-HRD-104 | S-B2 | Selesai | Tersambung API | ada | Layar FE-HRD-150 — Dasbor Kuota & Saldo Cuti Pegawai ESS (/hr/leave/my-balance) & Drawer B |
| FE-HRD-105 | S-B2 | Selesai | Tersambung API | ada | Layar FE-HRD-153 — Manajemen Alokasi Saldo, Carry-Forward & Runner Kedaluwarsa HR (/hr/lea |
| FE-HRD-106 | S-B2 | Selesai | Tersambung API | ada | Modal Penyesuaian Saldo Resmi HR Berdokumen Audit (LeaveBalanceAdjustmentModal) |
| FE-HRD-107 | S-B2 | Selesai | Tersambung API | ada | Komponen Kalender Cuti, Pemilih Sesi Half-Day 0.5 Hari & Validasi Saldo (LeaveDateRangePic |
| FE-HRD-108 | S-B2 | Selesai | Tersambung API | ada | Modal Unggah Surat Sakit Dokter Ber-SIP & Masking Privasi Medis K3RS (MedicalCertificateUp |
| FE-HRD-109 | S-B2 | Selesai | Tersambung API | ada | Layar FE-HRD-151 — Formulir Pengajuan Cuti Mandiri & Cuti Sakit ESS (/hr/leave/request) |
| FE-HRD-110 | S-B2 | Selesai | Tersambung API | ada | Komponen Banner Peringatan Kuota Pelayanan Staf (SafeStaffingCoverageWarningBanner) |
| FE-HRD-111 | S-B2 | Selesai | Tersambung API | ada | Layar FE-HRD-152 — Kotak Masuk Approval Atasan & Aksi Approve with Mitigation (/hr/leave/a |
| FE-HRD-112 | S-B2 | Selesai | Tersambung API | ada | Layar FE-HRD-154 — Inisiasi & Pemantauan Pemanggilan Darurat (Leave Recall) (/hr/leave/rec |
| FE-HRD-113 | S-B2 | Selesai | Tersambung API | ada | Kotak Masuk Approval Leave Recall HR Manager & Restore Saldo Sisa (LeaveRecallApprovalInbo |
| FE-HRD-114 | S-B2 | Selesai | Tersambung API | ada | Layar FE-HRD-155 — Rekonsiliasi Akhir Cuti, Tabel Retrospektif & Payroll Close (/hr/leave/ |
| FE-HRD-115 | S-B2 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi ujung-ke-ujung antarmuka, linter, dan akses peran Slice S-B2 |
| FE-HRD-116 | S-B3 | Selesai | Non-layar | ada | API service client, tipe TypeScript & Redux slice lembur |
| FE-HRD-117 | S-B3 | Selesai | Hanya tampilan | ada | Komponen form dual-mode SPKL & kalkulator kuota lembur PP 35/2021 |
| FE-HRD-118 | S-B3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-156 — Pengajuan SPKL Pra-Dinas & Klaim Cito Pasca-Dinas ESS |
| FE-HRD-119 | S-B3 | Selesai | Hanya tampilan | ada | Komponen kotak masuk persetujuan berjenjang & anti-self-approval |
| FE-HRD-120 | S-B3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-157 — Kotak Masuk Approval Lembur Ruangan (Penyelia & Ka. Unit) |
| FE-HRD-121 | S-B3 | Selesai | Hanya tampilan | ada | Drawer investigasi anomali presensi biometrik & bukti logbook fisik dinas |
| FE-HRD-122 | S-B3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-158 — Pusat Verifikasi Presensi & Rekonsiliasi Discrepancy HR |
| FE-HRD-123 | S-B3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-159-A — Kotak Masuk Approval Eskalasi Direktur Medik |
| FE-HRD-124 | S-B3 | Selesai | Hanya tampilan | ada | Komponen kartu tiket saldo hak libur pengganti CTO (90 hari masa aktif) |
| FE-HRD-125 | S-B3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-159 — Monitoring Saldo Cuti Pengganti (CTO) & Riwayat Konversi |
| FE-HRD-126 | S-B3 | Selesai | Hanya tampilan | ada | Komponen tabel rekonsiliasi & indikator rapel retrospektif lembur |
| FE-HRD-127 | S-B3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-160 — Penutupan Periode Lembur, Cut-Off & Rekonsiliasi Payroll |
| FE-HRD-128 | S-B3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-161 — Pengaturan Kebijakan & Kuota Lembur RS (PP 35/2021) |
| FE-HRD-129 | S-B3 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi ujung-ke-ujung antarmuka, linter, dan akses peran Slice S-B3 |
| FE-HRD-130 | S-B5 | Selesai | Non-layar | ada | Fondasi frontend modul Payroll, Redux slice payrollSlice.js, Axios client & tipe data kano |
| FE-HRD-131 | S-B5 | Selesai | Non-layar | ada | Layanan API service frontend integrasi endpoint Run Cycles & Settings |
| FE-HRD-132 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Layar FE-HRD-162 — Dasbor Ringkasan & Monitoring Siklus Penggajian (/hr/payroll/runs) |
| FE-HRD-133 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Layar FE-HRD-163 — Wizard Inisiasi Batch & Validasi Precondition Gate Cut-off tgl 20 (/hr/ |
| FE-HRD-134 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Layar FE-HRD-164 — Lembar Kerja Kalkulasi Gaji, Komponen Rincian & Filter Unit/Profesi (/h |
| FE-HRD-135 | S-B5 | Selesai | Tidak bisa dibuka | ada | Layar FE-HRD-165 — Drawer Investigasi Anomali Variansi & Penginputan Justifikasi Audit (/h |
| FE-HRD-136 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Komponen Modal Detail Rincian Gaji Pegawai & Komparasi Slip Gaji vs Histori Bulan Lalu (Pa |
| FE-HRD-137 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Layanan API service frontend & Redux actions untuk alur otorisasi 3-jenjang & anti-self-ap |
| FE-HRD-138 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Layar FE-HRD-166 — Dasbor Otorisasi 3-Jenjang Penggajian & Rekapitulasi Eksekutif Finansia |
| FE-HRD-139 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Komponen Modal Konfirmasi Penguncian Immutable & Trigger Serah Terima Finance SHA-256 (Pay |
| FE-HRD-140 | S-B5 | Selesai | Pindah ke modul Payroll | ada | Dasbor Pemantauan Status Serah Terima Finance & Webhook Reconciler (/hr/payroll/handoffs) |
| FE-HRD-141 | S-B5 | Selesai | Tersambung API | ada | Layar FE-HRD-167 — Portal Slip Gaji Mandiri Pegawai (ESS) & Keypad Proteksi PIN Sekunder 6 |
| FE-HRD-142 | S-B5 | Selesai | Tidak bisa dibuka | ada | Layar Pengaturan Parameter Penggajian RS, Batas Plafon BPJS & Threshold Variansi (/hr/payr |
| FE-HRD-143 | S-B5 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi ujung-ke-ujung antarmuka, linter CI/CD, build Next.js, dan hak akses peran Slic |
| FE-HRD-144 | S-B6 | Selesai | Non-layar | ada | Service API client, adapter TypeScript/Axios, Redux slice & state management restitusi dan |
| FE-HRD-145 | S-B6 | Selesai | Non-layar | ada | Desain token, status badge, format mata uang & indikator rasio upah 50% |
| FE-HRD-146 | S-B6 | Selesai | Tersambung API | ada | Layar FE-HRD-168 — Dasbor Utama Restitusi & Pemotongan Gaji (Bagian 1: Ringkasan & Metrik) |
| FE-HRD-147 | S-B6 | Selesai | Tersambung API | ada | Layar FE-HRD-168 — Tabel Peringatan Rasio Potongan Gaji >40% & Trigger Sinkronisasi Financ |
| FE-HRD-148 | S-B6 | Selesai | Tersambung API | ada | Layar FE-HRD-170 — Lembar Kerja Jadwal Angsuran Master Pinjaman (Bagian 1) |
| FE-HRD-149 | S-B6 | Selesai | Tersambung API | ada | Layar FE-HRD-170 — Modal Simulasi Penegakan Hard-Cap 50% & Visualisasi Pemangkasan Berjenj |
| FE-HRD-150 | S-B6 | Selesai | Tersambung API | ada | Layar FE-HRD-169 — Pelacak Kuota Plafon Tahunan Pegawai di Portal ESS (Bagian 1) |
| FE-HRD-151 | S-B6 | Selesai | Tersambung API | ada | Layar FE-HRD-169 — Formulir Pengajuan Klaim Restitusi Mandiri di ESS & Dropzone Kuitansi ( |
| FE-HRD-152 | S-B6 | Selesai | Tidak bisa dibuka | ada | Layar FE-HRD-172 — Kotak Antrean Verifikasi Persetujuan Bertingkat 3-Tier (Bagian 1) |
| FE-HRD-153 | S-B6 | Selesai | Tidak bisa dibuka | ada | Komponen ModalClaimDocumentPreview — Pratinjau Interaktif Zoom Berkas Nota Kuitansi & Resu |
| FE-HRD-154 | S-B6 | Selesai | Tersambung API | ada | Komponen Drawer Slide-Over FE-HRD-171 — Investigasi & Koreksi Restitusi Salah Potong Perio |
| FE-HRD-155 | S-B6 | Selesai | Tidak bisa dibuka | ada | Layar FE-HRD-172 & ModalVoucherQrViewer — Penerbitan Voucher Kasir Cito 24/7 & Penampil QR |
| FE-HRD-156 | S-B6 | Selesai | Tersambung API | ada | Layar FE-HRD-173 — Pengaturan Kebijakan Plafon Benefit per Grade & Batas Pemotongan Upah |
| FE-HRD-157 | S-B6 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi ujung-ke-ujung antarmuka UI/UX, linter CI/CD, build Next.js, dan hak akses pera |
| FE-HRD-158 | S-D3 | Selesai | Non-layar | ada | Service API client, adapter TypeScript/Axios, Redux slice & state management manfaat keseh |
| FE-HRD-159 | S-D3 | Selesai | Non-layar | ada | Registrasi peta navigasi sidebar, rute /corporate/hrd/benefits/*, rute ESS & permission gu |
| FE-HRD-160 | S-D3 | Selesai | Navigasi, tidak dinilai | ada | Komponen reusable & token UI manfaat (StatusBadge, kartu limit, meteran utilisasi, uploade |
| FE-HRD-161 | S-D3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-174 — Dasbor Analitik Manfaat Kesehatan, Utilisasi Plafon & Alert Batas Usia  |
| FE-HRD-162 | S-D3 | Selesai | Tersambung API | ada | Layar FE-HRD-175 — Master Katalog Paket Manfaat 3-Lapisan, Pagu Plafon & Diskon Tarif Staf |
| FE-HRD-163 | S-D3 | Selesai | Tersambung API | ada | Komponen Drawer FE-HRD-175 — Matriks Kelayakan Grade Staf Rumah Sakit ke Kelas Kamar Rawat |
| FE-HRD-164 | S-D3 | Selesai | Tersambung API | ada | Layar FE-HRD-176 — Monitoring Kepesertaan Asuransi Pegawai, Aktivasi H+0 vs Pasca-Percobaa |
| FE-HRD-165 | S-D3 | Selesai | Tersambung API | ada | Layar FE-HRD-177 — Antrean Meja Kerja Verifikasi Dokumen Kependudukan Tanggungan Keluarga  |
| FE-HRD-166 | S-D3 | Selesai | Tersambung API | ada | Layar FE-HRD-177 (ESS View) — Portal Mandiri ESS Keluarga Saya & Indikator Kuota 1 Pasanga |
| FE-HRD-167 | S-D3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-178 — Inquiry Hak Berobat Staf di RS Sendiri & Buku Besar Mutasi Saldo Plafon |
| FE-HRD-168 | S-D3 | Selesai | Hanya tampilan | ada | Modal Dialog FE-HRD-178 — Penyesuaian Manual Saldo Plafon Berobat RS oleh HR Manajer |
| FE-HRD-169 | S-D3 | Selesai | Tersambung API | ada | Layar FE-HRD-179 — Meja Kerja Monitoring, Audit Kuitansi & Settlement Klaim Koordinasi Man |
| FE-HRD-170 | S-D3 | Selesai | Hanya tampilan | ada | Layar FE-HRD-179 (ESS View) — Formulir Mandiri Pengajuan Klaim COB Berlampiran Mandatory S |
| FE-HRD-171 | S-D3 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi ujung-ke-ujung antarmuka UI/UX, linter CI/CD, build Next.js, dan hak akses pera |
| FE-HRD-172 | S-D4 | Selesai | Non-layar | ada | Redux Toolkit Slice hrServiceSlice & Endpoint Client Axios |
| FE-HRD-173 | S-D4 | Selesai | Hanya tampilan | ada | Antarmuka Pengaturan Katalog Layanan SDM & Target Jam SLA (FE-HRD-181) |
| FE-HRD-174 | S-D4 | Selesai | Hanya tampilan | ada | Editor Template Naskah Dinas Token Dinamis & Konfigurasi Pola Penomoran Gapless (FE-HRD-18 |
| FE-HRD-175 | S-D4 | Selesai | Hanya tampilan | ada | Dasbor Meja Bantuan (Helpdesk) SDM & Kartu Metrik KPI SLA Operasional (FE-HRD-180) |
| FE-HRD-176 | S-D4 | Selesai | Hanya tampilan | ada | Antrean Tiket Helpdesk Berjenjang 3-Tier dengan Indikator Countdown Timer SLA Real-Time (F |
| FE-HRD-177 | S-D4 | Selesai | Hanya tampilan | ada | Modal Re-assign Penugasan Analis SDM Cepat & Penyeimbangan Beban Kerja (FE-HRD-180) |
| FE-HRD-178 | S-D4 | Selesai | Hanya tampilan | ada | Portal ESS Mandiri: Katalog Layanan Interaktif (*Service Tiles*) & Estimasi Durasi (FE-HRD |
| FE-HRD-179 | S-D4 | Selesai | Hanya tampilan | ada | Formulir Pengajuan Tiket ESS Multi-Langkah & Validasi Drag-and-Drop Lampiran 5MB (FE-HRD-1 |
| FE-HRD-180 | S-D4 | Selesai | Hanya tampilan | ada | Drawer Pemrosesan Tiket Analis, Penampil Berkas Side-by-Side & Utas Obrolan Klarifikasi (F |
| FE-HRD-181 | S-D4 | Selesai | Hanya tampilan | ada | Antarmuka Verifikasi Berkas Perubahan Rekening/Status & Eksekusi Pemutakhiran Profil Sub-0 |
| FE-HRD-182 | S-D4 | Selesai | Hanya tampilan | ada | Modal Otorisasi Surat Resmi Pejabat, Pratinjau Live Naskah Dinas & Generator QR Code (FE-H |
| FE-HRD-183 | S-D4 | Selesai | Hanya tampilan | ada | Modal Pencabutan Keabsahan Surat Resmi (Revokasi) Ber-Input Nomor BAP (FE-HRD-184) |
| FE-HRD-184 | S-D4 | Selesai | Hanya tampilan | ada | Halaman Portal Publik Verifikasi Keabsahan Dokumen (/verify/document/[hash]) & Dialog Inpu |
| FE-HRD-185 | S-D4 | Selesai | Verifikasi (bukti dokumen) | ada | Pusat Arsip Digital Kepegawaian (Dokumen Saya ESS), Kartu PIN Rahasia & Verifikasi Ujung-k |
| FE-HRD-186 | S-C1 | Selesai | Non-layar | ada | Redux Toolkit Slice credentialingSlice, Axios Client & TypeScript Interfaces |
| FE-HRD-187 | S-C1 | Selesai | Hanya tampilan | ada | Portal Kredensial Tenaga Medis: Wizard Pendaftaran & Checklist Whitebook (FE-HRD-187) |
| FE-HRD-188 | S-C1 | Selesai | Hanya tampilan | ada | Dashboard Verifikasi PSV Staf Kredensialing & Drawer SatuSehat SDMK (FE-HRD-188) |
| FE-HRD-189 | S-C1 | Selesai | Hanya tampilan | ada | Komponen Visual Radar Pemantau Kedaluwarsa SIP 5 Tahun & Peringatan Dini |
| FE-HRD-190 | S-C1 | Selesai | Hanya tampilan | ada | Komponen Manajemen 3 Komite Mandiri & Katalog Whitebook Kompetensi |
| FE-HRD-191 | S-C1 | Selesai | Hanya tampilan | ada | Meja Kerja Asesmen Mitra Bestari / Peer Review Desk (FE-HRD-189) |
| FE-HRD-192 | S-C1 | Selesai | Hanya tampilan | ada | Sidang Pleno Komite Kredensial & Konsolidasi Penilaian Reviewer (FE-HRD-190) |
| FE-HRD-193 | S-C1 | Selesai | Hanya tampilan | ada | Konsol Pengesahan SPK & RKK Direktur Utama (*Director Approval Console* — FE-HRD-191) |
| FE-HRD-194 | S-C1 | Selesai | Hanya tampilan | ada | Halaman Publik Responsif Verifikasi Keabsahan Dokumen SPK-RKK (/public/credentials/verify/ |
| FE-HRD-195 | S-C1 | Selesai | Hanya tampilan | ada | Registri Kewenangan Klinis Rumah Sakit & Direktori Pencarian Nakes (FE-HRD-192) |
| FE-HRD-196 | S-C1 | Selesai | Tidak bisa dibuka | ada | Dialog Penegakan Hard-Stop & Banner Lisensi Kedaluwarsa di SIMRS/EMR |
| FE-HRD-197 | S-C1 | Selesai | Tidak bisa dibuka | ada | Modal Persetujuan Co-Signature Supervisi & Timer 24 Jam Supervisor |
| FE-HRD-198 | S-C1 | Selesai | Hanya tampilan | ada | Komponen Life-Saving Emergency Override & Console Audit Insiden Mutu KMKP |
| FE-HRD-199 | S-C1 | Selesai | Hanya tampilan | ada | Konsol Penugasan Cepat Temporary & Emergency (FE-HRD-193) serta Radar Rekredensialing (FE- |
| FE-HRD-200 | S-C1 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi Integrasi Menyeluruh Next.js, Linting 0 Error & UAT Acceptance S-C1 |
| FE-HRD-201 | S-C1-OPPE | Selesai | Non-layar | ada | Inisialisasi Redux Slice clinicalPerformanceSlice.ts, Axios Client API & DTO Types Kontrak |
| FE-HRD-202 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Dashboard Kinerja Klinis & Visualisasi Radar OPPE Dokter Spesialis (FE-HRD-201) |
| FE-HRD-203 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Manajemen Periode & Pengaturan Matriks Indikator OPPE KSM (FE-HRD-202) |
| FE-HRD-204 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Modal Pengaturan Ambang Batas & Bobot Indikator Spesifik KSM (OppeIndicatorConfigModal) |
| FE-HRD-205 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Formulir Evaluasi Kualitatif Peer Review Sejawat KSM (FE-HRD-203) |
| FE-HRD-206 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Komponen Peringatan Kerahasiaan Hak Istimewa Peer Review Privilege & Modal Read-Audit Log |
| FE-HRD-207 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Dashboard Pusat Deteksi Pemicu Kritis & Pelacakan Kasus FPPE (FE-HRD-204) |
| FE-HRD-208 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Modal Inisiasi Penugasan FPPE & Penetapan Dokter Proctor Senior |
| FE-HRD-209 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Dashboard Pemantauan Proctoring Staf Baru & Prosedur Baru (FE-HRD-205) |
| FE-HRD-210 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Modal Entri Logbook Kasus Terpseudonim & Antarmuka Verifikasi Proctor (FppeCaseLogEntryMod |
| FE-HRD-211 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Sidang Pleno Kesimpulan & Rekomendasi Wewenang FPPE (FE-HRD-206) |
| FE-HRD-212 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Kalkulator Rapor Komposit 3 Tahun Rekredensialing & Visualisasi Tier (FE-HRD-207) |
| FE-HRD-213 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Modal Diskresi Penyesuaian Tier Kelayakan Rekredensialing (RecredentialingTierOverrideModa |
| FE-HRD-214 | S-C1-OPPE | Selesai | Hanya tampilan | ada | Konsol Sinkronisasi Putusan Rekredensialing ke SIMRS EMR & Audit Bypass (FE-HRD-208) |
| FE-HRD-215 | S-C1-OPPE | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi Integrasi Menyeluruh Next.js, Linting 0 Error & UAT Acceptance S-C1-OPPE |
| FE-HRD-216 | S-C2 | Selesai | Non-layar | ada | Inisialisasi Redux Slice learningDevelopmentSlice.ts, Axios Client API & DTO Types Kontrak |
| FE-HRD-217 | S-C2 | Selesai | Data contoh saat API gagal | ada | Dashboard Pelatihan Pegawai, Sertifikasi & Radar Kedaluwarsa (FE-HRD-216) |
| FE-HRD-218 | S-C2 | Selesai | Data contoh saat API gagal | ada | Konfigurasi Aturan Pelatihan Wajib Akreditasi & Penugasan Unit (FE-HRD-217) |
| FE-HRD-219 | S-C2 | Selesai | Data contoh saat API gagal | ada | Modal Konfigurasi Aturan Pelatihan Wajib & Ambang Nilai (MandatoryTrainingRuleModal) |
| FE-HRD-220 | S-C2 | Selesai | Data contoh saat API gagal | ada | Manajemen Sesi Pelatihan, Kalender Batch Kelas & Presensi Kehadiran (FE-HRD-218) |
| FE-HRD-221 | S-C2 | Selesai | Data contoh saat API gagal | ada | Modal Presensi QR Code / RFID & Rekapitulasi Hadir Sesi (TrainingAttendanceQrModal) |
| FE-HRD-222 | S-C2 | Selesai | Data contoh saat API gagal | ada | Lembar Penilaian Asesmen Teori Post-Test & Ujian Simulasi Klinis OSCE (FE-HRD-219) |
| FE-HRD-223 | S-C2 | Selesai | Data contoh saat API gagal | ada | Modal Rubrik Checklist OSCE & Tanda Tangan Digital Assessor (OsceClinicalEvaluationModal) |
| FE-HRD-224 | S-C2 | Selesai | Data contoh saat API gagal | ada | Registri Portofolio e-Sertifikat Pegawai & Verifikasi Dokumen Eksternal (FE-HRD-220) |
| FE-HRD-225 | S-C2 | Selesai | Data contoh saat API gagal | ada | Rencana Program Tahunan TNA & Monitoring Pagu Anggaran Unit (FE-HRD-221) |
| FE-HRD-226 | S-C2 | Selesai | Data contoh saat API gagal | ada | Tata Kelola Kontrak Ikatan Dinas Retensi Nakes & Perhitungan Denda Resign (FE-HRD-222) |
| FE-HRD-227 | S-C2 | Selesai | Data contoh saat API gagal | ada | Modal Draf Akta Ikatan Dinas & Rincian Tagihan Penalti Resign (TrainingBondContractModal) |
| FE-HRD-228 | S-C2 | Selesai | Data contoh saat API gagal | ada | Portofolio 50 SKP Nakes 5 Tahunan & Visual Progress Bar Radial (FE-HRD-223) |
| FE-HRD-229 | S-C2 | Selesai | Data contoh saat API gagal | ada | Modal Verifikasi ID Sertifikat & Sinkronisasi Plataran Sehat Kemenkes (PlataranSehatSyncMo |
| FE-HRD-230 | S-C2 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi Integrasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance S-C2 |
| FE-HRD-231 | S-C3 | Selesai | Non-layar | ada | Redux Slice State, Base Types & Service API Client Kontrak v21 |
| FE-HRD-232 | S-C3 | Selesai | Data contoh saat API gagal | ada | Dashboard Kinerja & Karier Klinis Nakes (FE-HRD-231) |
| FE-HRD-233 | S-C3 | Selesai | Data contoh saat API gagal | ada | Katalog Indikator KPI & Core Values BerAKHLAK (FE-HRD-232) |
| FE-HRD-234 | S-C3 | Selesai | Data contoh saat API gagal | ada | Siklus Kinerja & Cascading Sasaran Mutu Unit (FE-HRD-233) |
| FE-HRD-235 | S-C3 | Selesai | Data contoh saat API gagal | ada | Modal Input Sasaran Kerja Pegawai & Bobot Target (KpiGoalFormModal) |
| FE-HRD-236 | S-C3 | Selesai | Data contoh saat API gagal | ada | Portal Lembar Penilaian Berjenjang Tiga Tingkat (FE-HRD-234) |
| FE-HRD-237 | S-C3 | Selesai | Data contoh saat API gagal | ada | Modal Peer Review Sejawat & Umpan Balik Kualitatif (PeerReviewFeedbackModal) |
| FE-HRD-238 | S-C3 | Selesai | Data contoh saat API gagal | ada | Konsol Sidang Kalibrasi Manajemen & Guiding Distribution (FE-HRD-235) |
| FE-HRD-239 | S-C3 | Selesai | Data contoh saat API gagal | ada | Modal Nota Justifikasi Kalibrasi Manajemen (CalibrationJustificationModal) |
| FE-HRD-240 | S-C3 | Selesai | Data contoh saat API gagal | ada | Tata Kelola PIP 60 Hari Kalender & Logbook Mingguan Mentor (FE-HRD-236) |
| FE-HRD-241 | S-C3 | Selesai | Data contoh saat API gagal | ada | Modal Logbook Mingguan & Sidang Putusan PIP (PipWeeklyLogbookModal) |
| FE-HRD-242 | S-C3 | Selesai | Data contoh saat API gagal | ada | Portofolio Kenaikan Jenjang Klinis Perawat/Nakes PK I-V (FE-HRD-237) |
| FE-HRD-243 | S-C3 | Selesai | Data contoh saat API gagal | ada | Portal Pengajuan Sanggah Banding Nilai SLA 7/14 Hari (FE-HRD-238) |
| FE-HRD-244 | S-C3 | Selesai | Data contoh saat API gagal | ada | Modal Putusan Majelis Banding Direktur SDM (DisputeResolutionModal) |
| FE-HRD-245 | S-C3 | Selesai | Verifikasi (bukti dokumen) | ada | Verifikasi Menyeluruh Next.js App Router, Linting 0 Error & UAT Acceptance S-C3 |
| FE-HRD-246 | S-C5 | Selesai | Non-layar | ada | Redux State Slice employeeRelationSlice.ts, Axios Services & Types Definition |
| FE-HRD-247 | S-C5 | Selesai | Navigasi, tidak dinilai | ada | Sidebar Menu Navigasi, Breadcrumbs & RBAC Route Guards /hrd/relations |
| FE-HRD-248 | S-C5 | Selesai | Data contoh saat API gagal | ada | Layar Dasbor Hubungan Industrial & Radar Exit Clearance (/hrd/relations/dashboard) |
| FE-HRD-249 | S-C5 | Selesai | Data contoh saat API gagal | ada | Layar Pelaporan Insiden Disiplin, Filter Just Culture RS & Validasi KMKP (/hrd/relations/i |
| FE-HRD-250 | S-C5 | Selesai | Data contoh saat API gagal | ada | Komponen Dialog Modal Berita Acara Pemeriksaan (BAP) & Unggah Bukti Sidang (BapRecordModal |
| FE-HRD-251 | S-C5 | Selesai | Data contoh saat API gagal | ada | Layar Penerbitan & Monitoring Surat Peringatan (SP 1-3 Masa 6 Bulan PP 35/2021) (/hrd/rela |
| FE-HRD-252 | S-C5 | Selesai | Data contoh saat API gagal | ada | Komponen Modal Sanggah Banding Sanksi SP & Risalah Putusan Sidang Direksi (SanctionAppealM |
| FE-HRD-253 | S-C5 | Selesai | Data contoh saat API gagal | ada | Layar Pengelolaan Skorsing Investigasi (*Paid Suspension*) & Status Akses SIMRS (/hrd/rela |
| FE-HRD-254 | S-C5 | Selesai | Data contoh saat API gagal | ada | Layar Pengaduan Keluhan Staf (Grievance ESS) & Notulen Mediasi SLA 14 Hari (/hrd/relations |
| FE-HRD-255 | S-C5 | Selesai | Data contoh saat API gagal | ada | Komponen Modal & Portal Whistleblower Terenkripsi Asimetris Anti-Retaliasi (WhistleblowerM |
| FE-HRD-256 | S-C5 | Selesai | Data contoh saat API gagal | ada | Layar Manajemen Notulen & Risalah Forum Perundingan Bipartit RS (/hrd/relations/bipartite) |
| FE-HRD-257 | S-C5 | Selesai | Data contoh saat API gagal | ada | Layar Pengajuan Terminasi Kerja (Resign 30 Hari, Kontrak, Pensiun, SP3) & Radar PKWT (/hrd |
| FE-HRD-258 | S-C5 | Selesai | Data contoh saat API gagal | ada | Komponen Kalkulator Pesangon Deterministik PP 35/2021 & Lembar Rincian Finansial (Severanc |
| FE-HRD-259 | S-C5 | Selesai | Data contoh saat API gagal | ada | Layar Pusat Kendali Exit Clearance Multi-Departemen 5 Unit (/hrd/separations/exit-clearanc |
| FE-HRD-260 | S-C5 | Selesai | Data contoh saat API gagal | ada | Modal Signoff Kliring Unit, Indikator Gembok Payroll Final Sub-10 & Uji Linter Build |
| FE-HRD-261 | S-C6 | Selesai | Non-layar | ada | Setup TypeScript Interfaces, Redux Slice occupationalHealthSlice.ts & Axios Client |
| FE-HRD-262 | S-C6 | Selesai | Data contoh saat API gagal | ada | Komponen Visual K3RS: StatusBadge Kelaikan, Alert Kerahasiaan Medis & Indikator |
| FE-HRD-263 | S-C6 | Selesai | Data contoh saat API gagal | ada | Layar Dasbor Utama K3RS Rumah Sakit (FE-HRD-261) pada /hrd/occupational-health |
| FE-HRD-264 | S-C6 | Selesai | Data contoh saat API gagal | ada | Layar Rekam Kesehatan Staf & Evaluasi Fitness to Work (FE-HRD-262) |
| FE-HRD-265 | S-C6 | Selesai | Data contoh saat API gagal | ada | Modal Dialog Pengesahan Kelaikan Kerja Dokter Hiperkes (FitnessToWorkModal.tsx) |
| FE-HRD-266 | S-C6 | Selesai | Data contoh saat API gagal | ada | Layar Manajemen Pembatasan Tugas Medis (*Work Restriction*) (FE-HRD-263) |
| FE-HRD-267 | S-C6 | Selesai | Data contoh saat API gagal | ada | Modal Asesmen Kembali Bekerja (*Return-to-Work*) & Pelepasan Hard-Stop Roster |
| FE-HRD-268 | S-C6 | Selesai | Data contoh saat API gagal | ada | Layar Konsol Tanggap Darurat Pajanan Darah & Tertusuk Jarum (FE-HRD-264) |
| FE-HRD-269 | S-C6 | Selesai | Data contoh saat API gagal | ada | Widget Tombol Panik Darurat Tertusuk Jarum Mobile ESS Staf (NeedleStickPanicButton.tsx) |
| FE-HRD-270 | S-C6 | Selesai | Data contoh saat API gagal | ada | Layar Radar Imunisasi & Vaksinasi Tenaga Medis (FE-HRD-265) |
| FE-HRD-271 | S-C6 | Selesai | Data contoh saat API gagal | ada | Layar Surveilans Paparan Bahaya Ruangan Khusus TLD Radiasi (FE-HRD-266) |
| FE-HRD-272 | S-C6 | Selesai | Hanya tampilan | ada | Layar Pusat Penjadwalan & Pelaksanaan MCU Berkala STARKES (FE-HRD-267) |
| FE-HRD-273 | S-C6 | Selesai | Hanya tampilan | ada | Layar Registrasi Kecelakaan Kerja KAK/PAK & Olah TKP (FE-HRD-268) |
| FE-HRD-274 | S-C6 | Selesai | Hanya tampilan | ada | Modal Generator Ekspor Formulir Klaim JKK BPJS Ketenagakerjaan Tahap I-II |
| FE-HRD-275 | S-C6 | Selesai | Non-layar | ada | Integrasi Navigasi Sidebar K3RS, Indikator Hard-Stop Roster Sub-06 & Uji Linter Build E2E |
| FE-HRD-276 | S-A7-V2 | Selesai | Non-layar | ada | State Management Redux Toolkit unifiedApprovalSlice.ts & Sinkronisasi Token Auth |
| FE-HRD-277 | S-A7-V2 | Selesai | Hanya tampilan | ada | Layar Utama Kotak Masuk Persetujuan Terpadu (/hrd/approvals/inbox - FE-HRD-276) |
| FE-HRD-278 | S-A7-V2 | Selesai | Hanya tampilan | ada | Komponen Tab Filter Antrean Kotak Masuk (*Pending*, *Urgent H-4*, *Delegated*, *Approved*) |
| FE-HRD-279 | S-A7-V2 | Selesai | Hanya tampilan | ada | Kartu Rangkuman Metrik Kotak Masuk & Penanda Badge SLA Realtime |
| FE-HRD-280 | S-A7-V2 | Selesai | Hanya tampilan | ada | Layar Detail & Riwayat Permohonan Lengkap (/hrd/approvals/[id] - FE-HRD-277) |
| FE-HRD-281 | S-A7-V2 | Selesai | Hanya tampilan | ada | Visual Alur Tahapan Persetujuan & Kartu Indikator Risiko Hijau/Merah |
| FE-HRD-282 | S-A7-V2 | Selesai | Hanya tampilan | ada | Modal Dialog Persetujuan Tunggal & Verifikasi PIN Otorisasi |
| FE-HRD-283 | S-A7-V2 | Selesai | Hanya tampilan | ada | Modal Dialog Penolakan Berkas (Wajib Min 15 Karakter) & Modal Pengembalian Revisi |
| FE-HRD-284 | S-A7-V2 | Selesai | Hanya tampilan | ada | Layar Pengaturan Pendelegasian Wewenang Plh (/hrd/approvals/delegation - FE-HRD-278) |
| FE-HRD-285 | S-A7-V2 | Selesai | Hanya tampilan | ada | Modal Surat Kuasa Digital Penunjukan Plh & Pemilihan Pengganti Sah |
| FE-HRD-286 | S-A7-V2 | Selesai | Hanya tampilan | ada | Modal Persetujuan Massal Bersyarat (*Conditional Bulk Approval Modal*) & Filter Guard Item |
| FE-HRD-287 | S-A7-V2 | Selesai | Hanya tampilan | ada | Layar Konfigurasi Matriks Alur Persetujuan (/hrd/approvals/matrix - FE-HRD-279) & Konsol M |
| FE-HRD-288 | S-A7-V2 | Selesai | Hanya tampilan | ada | Layar Portal Layanan Mandiri Karyawan ESS (/hrd/self-service - FE-HRD-280) & Menu Cepat Pe |
| FE-HRD-289 | S-A7-V2 | Selesai | Hanya tampilan | ada | Layar Pelacak Riwayat Permohonan Mandiri (FE-HRD-281) & Lembar Perbaikan Draf Revisi (FE-H |
| FE-HRD-290 | S-A7-V2 | Selesai | Navigasi, tidak dinilai | ada | Pendaftaran Navigasi Sidebar, Prompt Otomasi Plh Cuti (INT-HR-94) & Uji Build Linting Stan |
| FE-HRD-291 | S-D5 | Selesai | Non-layar | ada | State Management Redux Toolkit businessTravelSlice.ts & Sinkronisasi Token |
| FE-HRD-292 | S-D5 | Selesai | Hanya tampilan | ada | Layar Dasbor Utama Perjalanan Dinas & Manajemen Biaya (/hrd/travel/dashboard - FE-HRD-291) |
| FE-HRD-293 | S-D5 | Selesai | Hanya tampilan | ada | Kartu Analitik Realisasi Anggaran Unit Cost Center & Radar Settlement Aktif |
| FE-HRD-294 | S-D5 | Selesai | Hanya tampilan | ada | Layar Matriks Tarif Lumpsum Uang Harian Per Diem & Zonasi Wilayah (/hrd/travel/rates-and-p |
| FE-HRD-295 | S-D5 | Selesai | Hanya tampilan | ada | Layar Pusat Penugasan & Riwayat SPPD Pegawai (/hrd/travel/requests - FE-HRD-292) |
| FE-HRD-296 | S-D5 | Selesai | Hanya tampilan | ada | Layar Formulir Pengajuan SPPD Mandiri Multi-Step (/hrd/travel/requests/create - FE-HRD-293 |
| FE-HRD-297 | S-D5 | Selesai | Hanya tampilan | ada | Komponen Kalkulator Otomatis Lumpsum Uang Harian & Pilihan Hotel Sesuai Plafon Grade |
| FE-HRD-298 | S-D5 | Selesai | Hanya tampilan | ada | Layar Konsol Manajemen Uang Muka Kerja (Advance) (/hrd/travel/advances - FE-HRD-294) & Mod |
| FE-HRD-299 | S-D5 | Selesai | Hanya tampilan | ada | Layar Lembar Rekonsiliasi & Settlement Pengeluaran Riil (/hrd/travel/settlements - FE-HRD- |
| FE-HRD-300 | S-D5 | Selesai | Hanya tampilan | ada | Modal Formulir Deklarasi Biaya Darurat Tanpa Kuitansi (*MissingReceiptDeclarationModal.tsx |
| FE-HRD-301 | S-D5 | Selesai | Hanya tampilan | ada | Modal Tanda Terima Setor Balik Kasir Kelebihan Uang Muka (*RefundSettlementModal.tsx*) & B |
| FE-HRD-302 | S-D5 | Selesai | Hanya tampilan | ada | Layar Radar Pemantau Keterlambatan Settlement Overdue (/hrd/travel/overdue-settlements - F |
| FE-HRD-303 | S-D5 | Selesai | Hanya tampilan | ada | Layar Portal Klaim Pengeluaran Operasional Non-Trip (/hrd/travel/expenses - FE-HRD-296) |
| FE-HRD-304 | S-D5 | Selesai | Hanya tampilan | ada | Modal Upload Struk Bukti Kasir & Drawer Pratinjau Faktur Pajak / Boarding Pass |
| FE-HRD-305 | S-D5 | Selesai | Non-layar | ada | Integrasi Navigasi Sidebar, Penanda Indikator Hard-Stop Roster Sub-06 & Uji Linter Build S |


---

## Lampiran B — Berkas yang melanggar base component

| Berkas | Slice | Melanggar | Tinjau | Unik | Base yang dipakai |
|---|---|---:|---:|---:|---|
| `components/view/hr/occupational-health/ohs-injuries-view.jsx` | S-C6 | 29 | 3 | 0 | tidak ada |
| `components/view/hrd/relations/relations-grievance-view.jsx` | S-C5 | 26 | 2 | 0 | tidak ada |
| `components/view/hrd/relations/relations-separations-view.jsx` | S-C5 | 21 | 2 | 0 | tidak ada |
| `components/view/hr/occupational-health/ohs-mcu-view.jsx` | S-C6 | 20 | 6 | 0 | tidak ada |
| `components/view/hrd/relations/relations-bipartite-view.jsx` | S-C5 | 20 | 2 | 0 | tidak ada |
| `components/view/hrd/relations/relations-suspensions-view.jsx` | S-C5 | 19 | 2 | 0 | tidak ada |
| `components/view/hr/travel/unified-travel-create-view.jsx` | S-D5 | 17 | 2 | 6 | tidak ada |
| `components/view/hrd/relations/relations-sanctions-view.jsx` | S-C5 | 17 | 6 | 0 | tidak ada |
| `components/view/hr/occupational-health/ohs-vaccinations-view.jsx` | S-C6 | 16 | 1 | 0 | tidak ada |
| `components/view/hr/occupational-health/ohs-work-restrictions-view.jsx` | S-C6 | 16 | 1 | 0 | tidak ada |
| `components/view/hr/performance/kpi-catalogs-view.jsx` | S-C3 | 16 | 5 | 0 | tidak ada |
| `components/view/hr/occupational-health/ohs-surveillance-view.jsx` | S-C6 | 14 | 1 | 0 | tidak ada |
| `components/view/hr/performance/performance-cycles-view.jsx` | S-C3 | 14 | 3 | 0 | tidak ada |
| `components/view/hr/restitution/restitution-settings-view.jsx` | S-B6 | 14 | 4 | 1 | base-button |
| `components/view/hrd/relations/relations-incidents-view.jsx` | S-C5 | 14 | 3 | 0 | tidak ada |
| `components/view/hr/payroll/payroll-settings-view.jsx` | S-B5 | 13 | 0 | 0 | base-button |
| `components/view/hrd/relations/modal/WhistleblowerReportModal.jsx` | S-C5 | 12 | 4 | 0 | tidak ada |
| `components/features/hr/overtime/OvertimeFormSwitch.jsx` | S-B3 | 11 | 3 | 0 | base-button |
| `components/view/hr/learning/modal/MandatoryTrainingRuleModal.jsx` | S-C2 | 11 | 3 | 0 | tidak ada |
| `components/view/hr/occupational-health/modal/FitnessToWorkModal.jsx` | S-C6 | 11 | 1 | 0 | tidak ada |
| `components/view/hr/occupational-health/widget/NeedleStickPanicButton.jsx` | S-C6 | 11 | 1 | 0 | tidak ada |
| `components/view/hr/credentialing/credentialing-fast-track-view.jsx` | S-C1 | 10 | 1 | 0 | base-button, status-badge |
| `components/view/hr/learning/modal/OsceClinicalEvaluationModal.jsx` | S-C2 | 10 | 1 | 1 | tidak ada |
| `components/view/hr/learning/modal/PlataranSehatSyncModal.jsx` | S-C2 | 10 | 1 | 0 | tidak ada |
| `components/view/hr/leave/leave-approval-inbox-view.jsx` | S-B2 | 10 | 0 | 0 | tidak ada |
| `components/view/hr/travel/modal/ExpenseReceiptUploadModal.jsx` | S-D5 | 10 | 1 | 0 | tidak ada |
| `components/view/hrd/relations/modal/BapRecordModal.jsx` | S-C5 | 10 | 2 | 0 | tidak ada |
| `components/view/hr/approvals/unified-approval-inbox-view.jsx` | S-A7-V2 | 9 | 7 | 0 | tidak ada |
| `components/view/hr/learning/learning-dashboard-view.jsx` | S-C2 | 9 | 1 | 0 | tidak ada |
| `components/view/hr/leave/leave-reconciliations-view.jsx` | S-B2 | 9 | 3 | 0 | tidak ada |
| `components/view/hr/occupational-health/ohs-needlestick-view.jsx` | S-C6 | 9 | 3 | 0 | tidak ada |
| `components/view/hrd/relations/modal/SanctionAppealModal.jsx` | S-C5 | 9 | 1 | 0 | tidak ada |
| `components/features/self-service-profile/self-service-profile-page.jsx` | S-A2..S-A7 lama | 8 | 1 | 1 | hero, toast-stack |
| `components/view/hr/approvals/unified-approval-detail-view.jsx` | S-A7-V2 | 8 | 1 | 0 | tidak ada |
| `components/view/hr/credentialing/credentialing-whitebook-catalog-view.jsx` | S-C1 | 8 | 0 | 0 | base-button, status-badge |
| `components/view/hr/leave/leave-recalls-view.jsx` | S-B2 | 8 | 2 | 0 | tidak ada |
| `components/view/hr/occupational-health/ohs-health-records-view.jsx` | S-C6 | 8 | 0 | 0 | tidak ada |
| `components/view/hr/oppe/peer-review-evaluation-form-view.jsx` | S-C1-OPPE | 8 | 0 | 0 | base-button, status-badge |
| `components/view/hr/overtime/overtime-settings-view.jsx` | S-B3 | 8 | 2 | 0 | base-button |
| `components/view/hr/performance/modal/PipWeeklyLogbookModal.jsx` | S-C3 | 8 | 8 | 0 | tidak ada |
| `components/view/hr/scheduling/on-call-scheduling-view.jsx` | S-B4 | 8 | 1 | 0 | hero, base-button, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack |
| `components/view/hr/services/hr-service-tickets-view.jsx` | S-D4 | 8 | 1 | 0 | base-button, status-badge |
| `components/view/hr/travel/unified-travel-settlements-view.jsx` | S-D5 | 8 | 0 | 0 | tidak ada |
| `components/features/corporate/hr/oppe/FppeCaseLogEntryModal.jsx` | S-C1-OPPE | 7 | 2 | 1 | base-button |
| `components/view/hr/approvals/modal/CreateDelegationModal.jsx` | S-A7-V2 | 7 | 2 | 0 | tidak ada |
| `components/view/hr/approvals/unified-ess-portal-view.jsx` | S-A7-V2 | 7 | 1 | 0 | tidak ada |
| `components/view/hr/benefit/employee-benefit-enrollment-detail-view.jsx` | FE-020 Benefit lama | 7 | 3 | 0 | hero, base-button, filter-select, data-table, pagination, status-badge, information-alert, toast-stack, access-denied-gate, confirm-modal |
| `components/view/hr/benefits/ess-cob-submission-view.jsx` | S-D3 | 7 | 0 | 0 | base-button |
| `components/view/hr/credentialing/portal-credentialing-wizard-view.jsx` | S-C1 | 7 | 1 | 0 | base-button, status-badge |
| `components/view/hr/learning/modal/TrainingBondContractModal.jsx` | S-C2 | 7 | 4 | 0 | tidak ada |
| `components/view/hr/learning/training-assessments-view.jsx` | S-C2 | 7 | 0 | 0 | tidak ada |
| `components/view/hr/learning/training-certificates-view.jsx` | S-C2 | 7 | 0 | 0 | tidak ada |
| `components/view/hr/leave/leave-request-view.jsx` | S-B2 | 7 | 0 | 0 | tidak ada |
| `components/view/hr/occupational-health/modal/ReturnToWorkModal.jsx` | S-C6 | 7 | 1 | 0 | tidak ada |
| `components/view/hr/oppe/oppe-period-management-view.jsx` | S-C1-OPPE | 7 | 1 | 0 | base-button, status-badge |
| `components/view/hr/performance/modal/PeerReviewFeedbackModal.jsx` | S-C3 | 7 | 3 | 0 | tidak ada |
| `components/view/hr/performance/performance-appraisals-view.jsx` | S-C3 | 7 | 5 | 0 | tidak ada |
| `components/view/hr/performance/performance-disputes-view.jsx` | S-C3 | 7 | 2 | 0 | tidak ada |
| `components/view/hr/scheduling/roster-scheduling-view.jsx` | S-B4 | 7 | 1 | 0 | hero, base-button, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack |
| `components/view/hr/services/hr-service-catalog-view.jsx` | S-D4 | 7 | 3 | 0 | base-button, status-badge |
| `components/view/hr/travel/modal/AdvanceDisbursementModal.jsx` | S-D5 | 7 | 1 | 0 | tidak ada |
| `components/view/hr/travel/modal/MissingReceiptDeclarationModal.jsx` | S-D5 | 7 | 1 | 0 | tidak ada |
| `components/view/hrd/relations/modal/ClearanceSignoffModal.jsx` | S-C5 | 7 | 2 | 0 | tidak ada |
| `components/features/corporate/hr/oppe/FppeInitiateModal.jsx` | S-C1-OPPE | 6 | 1 | 0 | base-button |
| `components/features/hr/benefits/BenefitPlanFormModal.jsx` | S-D3 | 6 | 2 | 1 | base-button |
| `components/features/hr/leave/LeaveRecallInitiationModal.jsx` | S-B2 | 6 | 2 | 0 | tidak ada |
| `components/features/hr/restitution/PastPeriodCorrectionDrawer.jsx` | S-B6 | 6 | 1 | 1 | base-button |
| `components/features/portal/services/TicketSubmissionWizard.jsx` | S-D4 | 6 | 2 | 1 | base-button, status-badge, base-form-control |
| `components/view/hr/learning/annual-plans-view.jsx` | S-C2 | 6 | 1 | 0 | tidak ada |
| `components/view/hr/learning/mandatory-rules-view.jsx` | S-C2 | 6 | 0 | 0 | tidak ada |
| `components/view/hr/learning/skp-tracker-view.jsx` | S-C2 | 6 | 0 | 0 | tidak ada |
| `components/view/hr/learning/training-bonds-view.jsx` | S-C2 | 6 | 0 | 0 | tidak ada |
| `components/view/hr/learning/training-sessions-view.jsx` | S-C2 | 6 | 0 | 0 | tidak ada |
| `components/view/hr/leave/leave-balances-admin-view.jsx` | S-B2 | 6 | 0 | 1 | tidak ada |
| `components/view/hr/performance/modal/CalibrationJustificationModal.jsx` | S-C3 | 6 | 2 | 0 | tidak ada |
| `components/view/hr/performance/modal/KpiGoalFormModal.jsx` | S-C3 | 6 | 5 | 0 | tidak ada |
| `components/view/hr/travel/modal/RefundSettlementModal.jsx` | S-D5 | 6 | 1 | 0 | tidak ada |
| `components/view/hr/travel/modal/UnderpaidPaymentModal.jsx` | S-D5 | 6 | 1 | 0 | tidak ada |
| `components/view/hr/travel/unified-travel-dashboard-view.jsx` | S-D5 | 6 | 6 | 0 | tidak ada |
| `components/view/hr/travel/unified-travel-expenses-view.jsx` | S-D5 | 6 | 0 | 0 | tidak ada |
| `components/view/hr/travel/unified-travel-overdue-view.jsx` | S-D5 | 6 | 0 | 0 | tidak ada |
| `components/view/hr/travel/unified-travel-requests-view.jsx` | S-D5 | 6 | 0 | 0 | tidak ada |
| `components/features/corporate/hr/oppe/OppeIndicatorConfigModal.jsx` | S-C1-OPPE | 5 | 1 | 0 | base-button |
| `components/features/ess/FamilyRegistrationFormModal.jsx` | S-D3 | 5 | 4 | 1 | base-button |
| `components/features/hr/leave/LeaveBalanceAdjustmentModal.jsx` | S-B2 | 5 | 4 | 0 | tidak ada |
| `components/features/hr/leave/MedicalCertificateUploadModal.jsx` | S-B2 | 5 | 2 | 1 | tidak ada |
| `components/features/workforce/workforce-profile-editor-view.jsx` | S-A1 | 5 | 0 | 0 | status-badge, toast-stack |
| `components/view/hr/approvals/unified-approval-delegation-view.jsx` | S-A7-V2 | 5 | 0 | 0 | tidak ada |
| `components/view/hr/approvals/unified-approval-matrix-view.jsx` | S-A7-V2 | 5 | 2 | 2 | tidak ada |
| `components/view/hr/approvals/unified-ess-my-requests-view.jsx` | S-A7-V2 | 5 | 0 | 0 | tidak ada |
| `components/view/hr/benefits/benefit-plans-view.jsx` | S-D3 | 5 | 0 | 0 | base-button |
| `components/view/hr/credentialing/credentialing-peer-review-view.jsx` | S-C1 | 5 | 1 | 0 | base-button, status-badge |
| `components/view/hr/credentialing/credentialing-privilege-registry-view.jsx` | S-C1 | 5 | 1 | 0 | base-button, status-badge |
| `components/view/hr/occupational-health/modal/BpjsJkkExportModal.jsx` | S-C6 | 5 | 2 | 0 | tidak ada |
| `components/view/hr/oppe/fppe-conclusions-plenary-view.jsx` | S-C1-OPPE | 5 | 5 | 1 | base-button, status-badge |
| `components/view/hr/overtime/overtime-compensatory-leave-view.jsx` | S-B3 | 5 | 0 | 0 | base-button |
| `components/view/hr/overtime/overtime-director-inbox-view.jsx` | S-B3 | 5 | 1 | 0 | base-button |
| `components/view/hr/payroll/payroll-run-wizard-view.jsx` | S-B5 | 5 | 1 | 0 | base-button |
| `components/view/hr/payroll/payroll-worksheet-view.jsx` | S-B5 | 5 | 2 | 0 | base-button |
| `components/view/hr/performance/modal/ClinicalLadderReviewModal.jsx` | S-C3 | 5 | 5 | 0 | tidak ada |
| `components/view/hr/performance/modal/DisputeResolutionModal.jsx` | S-C3 | 5 | 5 | 0 | tidak ada |
| `components/view/hr/restitution/deduction-schedules-view.jsx` | S-B6 | 5 | 0 | 0 | base-button |
| `components/view/hr/scheduling/modals/OnCallCallInModal.jsx` | S-B4 | 5 | 1 | 0 | base-button |
| `components/view/hr/services/hr-service-template-editor-view.jsx` | S-D4 | 5 | 1 | 0 | base-button, status-badge |
| `components/view/hr/travel/unified-travel-advances-view.jsx` | S-D5 | 5 | 0 | 0 | tidak ada |
| `components/features/clinical/ModalEmergencyOverrideLifeSaving.jsx` | S-C1 | 4 | 1 | 0 | base-button, status-badge |
| `components/features/hr/benefits/DependentVerificationModal.jsx` | S-D3 | 4 | 1 | 1 | base-button |
| `components/features/hr/benefits/PolicyNumberModal.jsx` | S-D3 | 4 | 1 | 1 | base-button |
| `components/features/hr/restitution/EssClaimFormModal.jsx` | S-B6 | 4 | 2 | 1 | base-button |
| `components/features/hr/restitution/ModalClaimDocumentPreview.jsx` | S-B6 | 4 | 3 | 1 | base-button |
| `components/view/hr/approvals/modal/ReturnForRevisionModal.jsx` | S-A7-V2 | 4 | 1 | 0 | tidak ada |
| `components/view/hr/approvals/unified-ess-revision-view.jsx` | S-A7-V2 | 4 | 0 | 0 | tidak ada |
| `components/view/hr/benefits/benefit-dependents-view.jsx` | S-D3 | 4 | 0 | 0 | base-button |
| `components/view/hr/benefits/benefit-enrollments-view.jsx` | S-D3 | 4 | 0 | 0 | base-button |
| `components/view/hr/benefits/cob-claims-workstation-view.jsx` | S-D3 | 4 | 0 | 0 | base-button |
| `components/view/hr/credentialing/credentialing-committee-plenary-view.jsx` | S-C1 | 4 | 0 | 0 | base-button, status-badge |
| `components/view/hr/credentialing/credentialing-kmkp-audit-view.jsx` | S-C1 | 4 | 4 | 0 | base-button, status-badge |
| `components/view/hr/leave/leave-admin-view.jsx` | S-B2 | 4 | 0 | 0 | hero, base-button, data-filter, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack, confirm-modal |
| `components/view/hr/leave/my-leave-balance-view.jsx` | S-B2 | 4 | 0 | 1 | tidak ada |
| `components/view/hr/overtime/overtime-ess-view.jsx` | S-B3 | 4 | 2 | 0 | base-button |
| `components/view/hr/overtime/overtime-verifications-view.jsx` | S-B3 | 4 | 0 | 0 | base-button |
| `components/view/hr/payroll/payroll-runs-view.jsx` | S-B5 | 4 | 1 | 0 | base-button |
| `components/view/hr/payroll/payroll-variances-view.jsx` | S-B5 | 4 | 1 | 0 | base-button |
| `components/view/hr/performance/clinical-ladder-view.jsx` | S-C3 | 4 | 0 | 0 | tidak ada |
| `components/view/hr/performance/performance-calibration-view.jsx` | S-C3 | 4 | 1 | 0 | tidak ada |
| `components/view/hr/performance/performance-pip-view.jsx` | S-C3 | 4 | 0 | 0 | tidak ada |
| `components/view/hr/recruitment/candidates-directory-view.jsx` | S-D2 | 4 | 2 | 0 | hero, base-button, data-table, status-badge, information-alert, base-form-control |
| `components/view/hr/recruitment/mcu-clearance-view.jsx` | S-D2 | 4 | 3 | 0 | hero, base-button, data-table, status-badge, information-alert, base-form-control |
| `components/view/hr/recruitment/vacancies-list-view.jsx` | S-D2 | 4 | 3 | 0 | hero, base-button, data-table, status-badge, information-alert, base-form-control |
| `components/view/hr/scheduling/modals/QuickShiftReplacementModal.jsx` | S-B4 | 4 | 1 | 0 | base-button |
| `components/view/hr/scheduling/modals/RetroactiveShiftCorrectionModal.jsx` | S-B4 | 4 | 1 | 0 | base-button |
| `components/view/hr/scheduling/scheduling-settings-view.jsx` | S-B4 | 4 | 3 | 0 | hero, base-button, information-alert, toast-stack |
| `components/view/hrd/relations/relations-dashboard-view.jsx` | S-C5 | 4 | 2 | 0 | tidak ada |
| `components/view/hrd/relations/relations-exit-clearance-view.jsx` | S-C5 | 4 | 0 | 0 | tidak ada |
| `components/view/self-services/human-resource/attendance-correction/attendance-correction-employee-view.jsx` | S-A2..S-A7 lama | 4 | 0 | 0 | hero, base-button, data-filter, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack, confirm-modal |
| `components/view/self-services/human-resource/overtime/overtime-employee-view.jsx` | S-A2..S-A7 lama | 4 | 0 | 0 | hero, base-button, data-filter, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack, confirm-modal |
| `components/features/corporate/hr/oppe/RecredentialingTierOverrideModal.jsx` | S-C1-OPPE | 3 | 4 | 0 | base-button |
| `components/features/corporate/hr/services/ModalDocumentRevocation.jsx` | S-D4 | 3 | 1 | 0 | base-button, status-badge |
| `components/features/corporate/hr/services/ModalReassignTicket.jsx` | S-D4 | 3 | 1 | 0 | base-button, base-form-control |
| `components/features/hr/benefits/GradeEligibilityDrawer.jsx` | S-D3 | 3 | 2 | 1 | base-button |
| `components/features/hr/benefits/ManualBalanceAdjustmentModal.jsx` | S-D3 | 3 | 3 | 1 | base-button |
| `components/features/hr/overtime/OvertimeApprovalTable.jsx` | S-B3 | 3 | 1 | 0 | base-button |
| `components/features/hr/overtime/OvertimeDiscrepancyDrawer.jsx` | S-B3 | 3 | 1 | 1 | base-button |
| `components/features/hr/overtime/OvertimeReconciliationTable.jsx` | S-B3 | 3 | 0 | 0 | base-button |
| `components/view/hr/approvals/modal/ApprovalConfirmationModal.jsx` | S-A7-V2 | 3 | 1 | 2 | tidak ada |
| `components/view/hr/approvals/modal/RejectRequestModal.jsx` | S-A7-V2 | 3 | 2 | 0 | tidak ada |
| `components/view/hr/attendance/attendance-monitoring-view.jsx` | S-B1 (layar lama) | 3 | 0 | 0 | hero, base-button, data-filter, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack, confirm-modal |
| `components/view/hr/benefits/benefit-inhouse-ledger-view.jsx` | S-D3 | 3 | 0 | 0 | base-button |
| `components/view/hr/benefits/benefits-dashboard-view.jsx` | S-D3 | 3 | 1 | 0 | base-button |
| `components/view/hr/credentialing/credentialing-psv-dashboard-view.jsx` | S-C1 | 3 | 1 | 1 | base-button, status-badge |
| `components/view/hr/credentialing/credentialing-recredentialing-cycle-view.jsx` | S-C1 | 3 | 1 | 0 | base-button, status-badge |
| `components/view/hr/exit-clearance/exit-clearance-detail-view.jsx` | S-C4 Exit | 3 | 1 | 0 | hero, base-button, status-badge, information-alert, toast-stack, confirm-modal |
| `components/view/hr/learning/modal/TrainingAttendanceQrModal.jsx` | S-C2 | 3 | 3 | 0 | tidak ada |
| `components/view/hr/occupational-health/ohs-dashboard-view.jsx` | S-C6 | 3 | 1 | 0 | tidak ada |
| `components/view/hr/oppe/recred-simrs-sync-console-view.jsx` | S-C1-OPPE | 3 | 2 | 0 | base-button, status-badge |
| `components/view/hr/overtime/overtime-approvals-view.jsx` | S-B3 | 3 | 0 | 0 | base-button |
| `components/view/hr/overtime/overtime-reconciliations-view.jsx` | S-B3 | 3 | 2 | 0 | base-button |
| `components/view/hr/performance/performance-dashboard-view.jsx` | S-C3 | 3 | 2 | 0 | tidak ada |
| `components/view/hr/recruitment/candidate-clinical-exam-view.jsx` | S-D2 | 3 | 1 | 0 | hero, base-button, information-alert, base-form-control |
| `components/view/hr/recruitment/hiring-handoffs-view.jsx` | S-D2 | 3 | 3 | 0 | hero, base-button, data-table, status-badge, information-alert, base-form-control |
| `components/view/hr/recruitment/job-offers-list-view.jsx` | S-D2 | 3 | 2 | 0 | hero, base-button, data-table, status-badge, information-alert, base-form-control |
| `components/view/hr/restitution/restitution-approvals-view.jsx` | S-B6 | 3 | 0 | 0 | base-button |
| `components/view/hr/scheduling/modals/ApplyShiftPatternModal.jsx` | S-B4 | 3 | 3 | 0 | base-button |
| `components/view/hr/services/portal-my-documents-view.jsx` | S-D4 | 3 | 0 | 1 | base-button, status-badge |
| `components/view/hr/travel/unified-travel-rates-view.jsx` | S-D5 | 3 | 1 | 0 | tidak ada |
| `components/view/self-services/human-resource/leave/leave-employee-view.jsx` | S-A2..S-A7 lama | 3 | 0 | 0 | hero, base-button, data-filter, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack, confirm-modal |
| `components/view/self-services/human-resource/schedule/schedule-employee-view.jsx` | S-A2..S-A7 lama | 3 | 0 | 0 | hero, base-button, data-filter, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack, confirm-modal |
| `components/features/clinical/ClinicalPrivilegeHardStopModal.jsx` | S-C1 | 2 | 2 | 0 | base-button, status-badge |
| `components/features/clinical/ModalSupervisorCoSignature.jsx` | S-C1 | 2 | 2 | 1 | base-button, status-badge |
| `components/features/corporate/hr/services/DrawerTicketProcessing.jsx` | S-D4 | 2 | 5 | 1 | base-button, status-badge |
| `components/features/corporate/hr/services/ModalDocumentAuthorization.jsx` | S-D4 | 2 | 2 | 0 | base-button, status-badge |
| `components/features/corporate/hr/services/ProfileChangeVerificationPanel.jsx` | S-D4 | 2 | 0 | 0 | base-button, status-badge |
| `components/features/hr/leave/LeaveDateRangePicker.jsx` | S-B2 | 2 | 1 | 2 | tidak ada |
| `components/features/hr/payroll/PayrollVarianceDrawer.jsx` | S-B5 | 2 | 1 | 1 | base-button |
| `components/view/hr/approvals/modal/BulkApprovalModal.jsx` | S-A7-V2 | 2 | 1 | 2 | tidak ada |
| `components/view/hr/benefits/ess-my-dependents-view.jsx` | S-D3 | 2 | 0 | 0 | base-button |
| `components/view/hr/credentialing/credentialing-director-console-view.jsx` | S-C1 | 2 | 1 | 0 | base-button, status-badge |
| `components/view/hr/lifecycle/clinical-preceptorships-list-view.jsx` | S-C4-ONB | 2 | 0 | 0 | hero, base-button, data-table, status-badge, information-alert |
| `components/view/hr/lifecycle/employment-contract-detail-view.jsx` | S-C4-ONB | 2 | 0 | 2 | hero, base-button, status-badge, information-alert, base-form-control |
| `components/view/hr/lifecycle/employment-contracts-list-view.jsx` | S-C4-ONB | 2 | 0 | 0 | hero, base-button, data-table, status-badge, information-alert |
| `components/view/hr/lifecycle/onboarding-cases-list-view.jsx` | S-C4-ONB | 2 | 1 | 0 | hero, base-button, data-table, status-badge, information-alert |
| `components/view/hr/lifecycle/probations-list-view.jsx` | S-C4-ONB | 2 | 0 | 0 | hero, base-button, data-table, status-badge, information-alert |
| `components/view/hr/oppe/clinical-oppe-dashboard-view.jsx` | S-C1-OPPE | 2 | 1 | 0 | base-button, status-badge |
| `components/view/hr/oppe/fppe-proctoring-dashboard-view.jsx` | S-C1-OPPE | 2 | 0 | 0 | base-button, status-badge |
| `components/view/hr/oppe/fppe-triggers-dashboard-view.jsx` | S-C1-OPPE | 2 | 1 | 0 | base-button, status-badge |
| `components/view/hr/oppe/recred-composite-scorecard-view.jsx` | S-C1-OPPE | 2 | 1 | 0 | base-button, status-badge |
| `components/view/hr/payroll/payroll-handoffs-view.jsx` | S-B5 | 2 | 1 | 0 | base-button |
| `components/view/hr/recruitment/candidate-hr-evaluation-view.jsx` | S-D2 | 2 | 0 | 0 | hero, base-button, information-alert, base-form-control |
| `components/view/hr/recruitment/job-offer-detail-view.jsx` | S-D2 | 2 | 2 | 0 | hero, base-button, data-table, status-badge, information-alert |
| `components/view/hr/recruitment/requisitions-list-view.jsx` | S-D2 | 2 | 2 | 0 | hero, base-button, data-table, status-badge, information-alert, base-form-control |
| `components/view/hr/recruitment/talent-pool-view.jsx` | S-D2 | 2 | 3 | 0 | hero, base-button, data-table, status-badge, information-alert, base-form-control |
| `components/view/hr/restitution/deductions-dashboard-view.jsx` | S-B6 | 2 | 1 | 0 | base-button |
| `components/view/hr/restitution/ess-restitution-claims-view.jsx` | S-B6 | 2 | 1 | 0 | base-button |
| `components/view/hr/scheduling/drawers/RosterValidationDrawer.jsx` | S-B4 | 2 | 1 | 1 | base-button |
| `components/view/hr/scheduling/modals/CopyPreviousPeriodModal.jsx` | S-B4 | 2 | 1 | 0 | base-button |
| `components/view/hr/services/portal-service-requests-view.jsx` | S-D4 | 2 | 2 | 0 | base-button, status-badge |
| `components/features/corporate/hr/oppe/PeerReviewConfidentialityWarningModal.jsx` | S-C1-OPPE | 1 | 1 | 0 | base-button |
| `components/features/hr/benefits/ClaimSettlementDrawer.jsx` | S-D3 | 1 | 2 | 1 | base-button |
| `components/features/hr/leave/LeaveBalanceMetricCards.jsx` | S-B2 | 1 | 0 | 0 | tidak ada |
| `components/features/hr/overtime/CompensatoryTimeOffCard.jsx` | S-B3 | 1 | 0 | 0 | base-button |
| `components/features/hr/payroll/PayrollApprovalCard.jsx` | S-B5 | 1 | 0 | 0 | base-button |
| `components/features/hr/restitution/DeductionCapSimulationModal.jsx` | S-B6 | 1 | 2 | 1 | base-button |
| `components/features/hr/restitution/ModalVoucherQrViewer.jsx` | S-B6 | 1 | 0 | 1 | base-button |
| `components/view/hr/approvals/components/WorkflowStepperCard.jsx` | S-A7-V2 | 1 | 0 | 0 | tidak ada |
| `components/view/hr/credentialing/credentialing-license-radar-view.jsx` | S-C1 | 1 | 0 | 0 | base-button |
| `components/view/hr/credentialing/public-spk-rkk-verify-view.jsx` | S-C1 | 1 | 0 | 0 | status-badge |
| `components/view/hr/lifecycle/clinical-preceptorship-detail-view.jsx` | S-C4-ONB | 1 | 0 | 1 | hero, base-button, status-badge, information-alert, base-form-control |
| `components/view/hr/lifecycle/initial-contract-batches-list-view.jsx` | S-C4-ONB | 1 | 0 | 1 | hero, base-button, data-table, information-alert |
| `components/view/hr/lifecycle/lifecycle-worklist-view.jsx` | S-C4-ONB | 1 | 0 | 0 | hero, base-button, data-table, status-badge, information-alert |
| `components/view/hr/lifecycle/onboarding-case-detail-view.jsx` | S-C4-ONB | 1 | 1 | 1 | hero, base-button, status-badge, information-alert, base-form-control |
| `components/view/hr/lifecycle/onboarding-template-detail-view.jsx` | S-C4-ONB | 1 | 2 | 0 | hero, base-button, information-alert, base-form-control |
| `components/view/hr/lifecycle/onboarding-templates-list-view.jsx` | S-C4-ONB | 1 | 0 | 0 | hero, base-button, data-table, information-alert, base-form-control |
| `components/view/hr/lifecycle/probation-detail-view.jsx` | S-C4-ONB | 1 | 0 | 1 | hero, base-button, status-badge, information-alert, base-form-control |
| `components/view/hr/occupational-health/components/OhsVisualComponents.jsx` | S-C6 | 1 | 0 | 0 | tidak ada |
| `components/view/hr/payroll/payroll-approvals-view.jsx` | S-B5 | 1 | 1 | 0 | base-button |
| `components/view/hr/payroll/payslips-ess-view.jsx` | S-B5 | 1 | 0 | 0 | base-button |
| `components/view/hr/recruitment/candidate-detail-screening-view.jsx` | S-D2 | 1 | 1 | 0 | hero, base-button, data-table, status-badge, information-alert, base-form-control |
| `components/view/hr/recruitment/requisition-detail-view.jsx` | S-D2 | 1 | 2 | 0 | hero, base-button, status-badge, information-alert |
| `components/view/hr/recruitment/vacancy-detail-view.jsx` | S-D2 | 1 | 4 | 0 | hero, base-button, data-table, status-badge, information-alert |
| `components/view/hr/services/public-document-verify-view.jsx` | S-D4 | 1 | 0 | 4 | base-button, status-badge |
| `components/view/hr/workflow/approval-inbox-view.jsx` | S-A2..S-A7 lama | 1 | 0 | 0 | hero, base-button, data-filter, filter-select, data-table, pagination, summary-grid, status-badge, information-alert, toast-stack, confirm-modal |


Lokasi elemen (baris dan jenis):

- `components/view/hr/occupational-health/ohs-injuries-view.jsx` — 186:button, 385:button, 393:button, 583:button, 590:button, 679:button, 686:button, 312:table, 278:input, 436:input, 448:input, 478:input, 492:input, 503:input, 514:input, 526:input, 550:input, 561:input, 573:input, 298:select, 461:select, 667:select, 538:textarea, 637:textarea, 647:textarea, 657:textarea, 177:hero
- `components/view/hrd/relations/relations-grievance-view.jsx` — 163:button, 178:button, 227:button, 241:button, 256:button, 271:button, 379:button, 382:button, 471:button, 474:button, 571:button, 500:table, 307:input, 317:input, 344:input, 355:input, 413:input, 423:input, 329:select, 459:select, 367:textarea, 435:textarea, 447:textarea, 155:hero
- `components/view/hrd/relations/relations-separations-view.jsx` — 177:button, 380:button, 383:button, 510:button, 524:button, 606:button, 542:table, 239:input, 249:input, 262:input, 290:input, 300:input, 337:input, 348:input, 358:input, 502:input, 272:select, 370:textarea, 171:hero
- `components/view/hr/occupational-health/ohs-mcu-view.jsx` — 182:button, 189:button, 405:button, 526:button, 681:button, 688:button, 794:button, 801:button, 322:table, 741:table, 286:input, 614:input, 643:input, 654:input, 306:select, 626:select, 666:select, 173:hero
- `components/view/hrd/relations/relations-bipartite-view.jsx` — 135:button, 267:button, 270:button, 357:button, 360:button, 449:button, 379:table, 196:input, 222:input, 234:input, 245:input, 256:input, 328:input, 340:input, 209:select, 314:select, 302:textarea, 129:hero
- `components/view/hrd/relations/relations-suspensions-view.jsx` — 139:button, 329:button, 332:button, 412:button, 441:button, 457:button, 351:table, 201:input, 211:input, 223:input, 247:input, 257:input, 270:input, 280:input, 295:input, 234:textarea, 133:hero
- `components/view/hr/travel/unified-travel-create-view.jsx` — 537:button, 198:input, 219:input, 230:input, 256:input, 267:input, 278:input, 335:input, 349:input, 362:input, 432:input, 453:input, 204:select, 241:select, 321:select, 289:textarea, 130:hero
- `components/view/hrd/relations/relations-sanctions-view.jsx` — 157:button, 402:button, 405:button, 524:button, 540:button, 436:table, 281:input, 291:input, 317:input, 331:input, 342:input, 387:input, 304:select, 354:textarea, 151:hero
- `components/view/hr/occupational-health/ohs-vaccinations-view.jsx` — 111:button, 265:button, 268:button, 337:button, 287:table, 165:input, 175:input, 187:input, 213:input, 228:input, 238:input, 250:input, 199:select, 103:hero
- `components/view/hr/occupational-health/ohs-work-restrictions-view.jsx` — 125:button, 273:button, 276:button, 376:button, 305:table, 179:input, 189:input, 201:input, 228:input, 238:input, 262:input, 212:select, 250:textarea, 117:hero
- `components/view/hr/performance/kpi-catalogs-view.jsx` — 91:button, 376:button, 156:table, 126:input, 269:input, 280:input, 332:input, 342:input, 135:select, 294:select, 307:select, 321:select, 354:select, 85:hero
- `components/view/hr/occupational-health/ohs-surveillance-view.jsx` — 90:button, 225:button, 228:button, 247:table, 144:input, 154:input, 166:input, 178:input, 188:input, 201:input, 212:input, 82:hero
- `components/view/hr/performance/performance-cycles-view.jsx` — 94:button, 238:button, 352:button, 114:table, 267:input, 291:input, 304:input, 314:input, 327:input, 337:input, 280:select, 88:hero
- `components/view/hr/restitution/restitution-settings-view.jsx` — 719:button, 586:table, 665:table, 383:input, 401:input, 428:input, 452:input, 477:input, 501:input, 734:input, 753:input, 772:input, 259:hero
- `components/view/hrd/relations/relations-incidents-view.jsx` — 111:button, 208:button, 363:button, 155:table, 247:input, 257:input, 270:input, 280:input, 331:input, 292:select, 348:textarea, 105:hero
- `components/view/hr/payroll/payroll-settings-view.jsx` — 134:input, 158:input, 182:input, 204:input, 243:input, 266:input, 304:input, 326:input, 349:input, 362:input, 386:input, 399:input, 86:hero
- `components/view/hrd/relations/modal/WhistleblowerReportModal.jsx` — 134:button, 313:button, 316:button, 213:input, 224:input, 253:input, 266:input, 276:input, 302:input, 239:select, 290:textarea
- `components/features/hr/overtime/OvertimeFormSwitch.jsx` — 91:button, 104:button, 152:input, 167:input, 178:input, 187:input, 244:input, 258:input, 130:select, 272:textarea, 317:textarea
- `components/view/hr/learning/modal/MandatoryTrainingRuleModal.jsx` — 311:button, 314:button, 141:input, 156:input, 186:input, 202:input, 235:input, 278:input, 170:select, 219:select
- `components/view/hr/occupational-health/modal/FitnessToWorkModal.jsx` — 236:button, 239:button, 119:input, 129:input, 186:input, 196:input, 208:input, 141:select, 170:select, 225:textarea
- `components/view/hr/occupational-health/widget/NeedleStickPanicButton.jsx` — 74:button, 204:button, 207:button, 121:input, 131:input, 144:input, 170:input, 154:select, 179:select, 193:textarea
- `components/view/hr/credentialing/credentialing-fast-track-view.jsx` — 326:table, 171:input, 184:input, 197:input, 257:input, 270:input, 283:input, 210:select, 296:select, 112:hero
- `components/view/hr/learning/modal/OsceClinicalEvaluationModal.jsx` — 283:button, 298:button, 301:button, 156:input, 178:input, 200:input, 222:input, 239:textarea
- `components/view/hr/learning/modal/PlataranSehatSyncModal.jsx` — 219:button, 263:button, 266:button, 157:input, 181:input, 193:input, 205:input, 169:select
- `components/view/hr/leave/leave-approval-inbox-view.jsx` — 410:button, 427:button, 445:button, 462:button, 539:button, 555:button, 498:input, 522:textarea, 164:hero
- `components/view/hr/travel/modal/ExpenseReceiptUploadModal.jsx` — 162:button, 165:button, 80:input, 91:input, 104:input, 116:input, 66:select, 150:select, 128:textarea
- `components/view/hrd/relations/modal/BapRecordModal.jsx` — 171:button, 72:input, 83:input, 94:input, 136:input, 151:select, 108:textarea, 122:textarea
- `components/view/hr/approvals/unified-approval-inbox-view.jsx` — 158:button, 431:button, 439:button, 447:button, 298:table, 261:input, 281:select, 142:hero
- `components/view/hr/learning/learning-dashboard-view.jsx` — 97:button, 101:button, 376:button, 384:button, 313:table, 278:input, 286:select, 88:hero
- `components/view/hr/leave/leave-reconciliations-view.jsx` — 434:button, 190:table, 248:table, 341:input, 360:input, 382:input, 401:textarea, 118:hero
- `components/view/hr/occupational-health/ohs-needlestick-view.jsx` — 191:button, 194:button, 279:button, 213:table, 140:select, 180:textarea, 95:hero
- `components/view/hrd/relations/modal/SanctionAppealModal.jsx` — 217:button, 220:button, 160:input, 175:select, 189:select, 137:textarea, 149:textarea, 203:textarea
- `components/features/self-service-profile/self-service-profile-page.jsx` — 541:button, 1107:button, 1223:button, 1232:button, 1241:button, 1278:button, 1352:button, 527:input
- `components/view/hr/approvals/unified-approval-detail-view.jsx` — 147:button, 161:button, 256:button, 309:button, 208:table, 302:input, 138:hero
- `components/view/hr/credentialing/credentialing-whitebook-catalog-view.jsx` — 132:button, 153:table, 204:input, 217:input, 230:input, 243:select, 113:hero
- `components/view/hr/leave/leave-recalls-view.jsx` — 143:button, 250:button, 377:button, 394:button, 213:table, 287:table, 136:hero
- `components/view/hr/occupational-health/ohs-health-records-view.jsx` — 121:button, 135:button, 150:button, 165:button, 256:button, 202:table, 96:hero
- `components/view/hr/oppe/peer-review-evaluation-form-view.jsx` — 164:button, 188:button, 212:button, 236:button, 260:textarea, 282:textarea, 77:hero
- `components/view/hr/overtime/overtime-settings-view.jsx` — 60:button, 90:input, 108:input, 158:input, 176:input, 222:input, 240:input, 46:hero
- `components/view/hr/performance/modal/PipWeeklyLogbookModal.jsx` — 159:button, 289:button, 118:input, 103:select, 137:textarea, 274:textarea
- `components/view/hr/scheduling/on-call-scheduling-view.jsx` — 407:input, 419:input, 429:input, 453:input, 333:select, 442:select, 467:textarea
- `components/view/hr/services/hr-service-tickets-view.jsx` — 373:button, 410:button, 431:button, 299:table, 254:input, 268:select, 279:select, 190:hero
- `components/view/hr/travel/unified-travel-settlements-view.jsx` — 179:button, 299:button, 192:input, 203:input, 214:input, 119:select, 103:hero
- `components/features/corporate/hr/oppe/FppeCaseLogEntryModal.jsx` — 127:input, 163:input, 175:input, 189:select, 216:textarea, 231:textarea
- `components/view/hr/approvals/modal/CreateDelegationModal.jsx` — 171:button, 174:button, 109:input, 120:input, 89:select, 159:textarea
- `components/view/hr/approvals/unified-ess-portal-view.jsx` — 97:button, 111:button, 125:button, 139:button, 168:table, 29:hero
- `components/view/hr/benefit/employee-benefit-enrollment-detail-view.jsx` — 716:input, 737:input, 752:input, 818:input, 666:select, 632:textarea, 768:textarea
- `components/view/hr/benefits/ess-cob-submission-view.jsx` — 306:table, 167:input, 186:input, 203:input, 217:input, 235:input, 100:hero
- `components/view/hr/credentialing/portal-credentialing-wizard-view.jsx` — 259:table, 181:input, 193:input, 206:input, 166:select, 277:select, 81:hero
- `components/view/hr/learning/modal/TrainingBondContractModal.jsx` — 151:button, 241:button, 249:button, 185:input, 189:input, 197:input
- `components/view/hr/learning/training-assessments-view.jsx` — 280:button, 301:button, 331:button, 223:table, 211:input, 168:hero
- `components/view/hr/learning/training-certificates-view.jsx` — 193:button, 200:button, 132:table, 115:input, 123:select, 72:hero
- `components/view/hr/leave/leave-request-view.jsx` — 280:button, 369:button, 385:button, 326:input, 216:select, 350:textarea, 150:hero
- `components/view/hr/occupational-health/modal/ReturnToWorkModal.jsx` — 141:button, 144:button, 104:input, 118:input, 87:select, 129:textarea
- `components/view/hr/oppe/oppe-period-management-view.jsx` — 150:table, 290:input, 317:input, 329:input, 302:select, 118:hero
- `components/view/hr/performance/modal/PeerReviewFeedbackModal.jsx` — 85:button, 141:button, 68:select, 102:textarea, 115:textarea
- `components/view/hr/performance/performance-appraisals-view.jsx` — 401:button, 411:button, 295:table, 319:input, 388:textarea, 141:hero
- `components/view/hr/performance/performance-disputes-view.jsx` — 201:button, 289:button, 164:table, 273:input, 257:textarea, 102:hero
- `components/view/hr/scheduling/roster-scheduling-view.jsx` — 258:input, 330:input, 343:input, 353:input, 272:select, 365:textarea
- `components/view/hr/services/hr-service-catalog-view.jsx` — 172:button, 290:button, 208:table, 372:input, 385:input, 157:hero
- `components/view/hr/travel/modal/AdvanceDisbursementModal.jsx` — 110:button, 113:button, 72:input, 77:input, 82:input, 95:textarea
- `components/view/hr/travel/modal/MissingReceiptDeclarationModal.jsx` — 140:button, 143:button, 98:input, 129:input, 83:select, 113:textarea
- `components/view/hrd/relations/modal/ClearanceSignoffModal.jsx` — 201:button, 204:button, 166:input, 190:input, 144:select, 176:textarea
- `components/features/corporate/hr/oppe/FppeInitiateModal.jsx` — 157:input, 139:select, 172:select, 188:select, 204:textarea
- `components/features/hr/benefits/BenefitPlanFormModal.jsx` — 97:input, 112:input, 144:input, 161:input, 127:select, 195:textarea
- `components/features/hr/leave/LeaveRecallInitiationModal.jsx` — 288:button, 186:input, 139:select, 206:select, 254:textarea
- `components/features/hr/restitution/PastPeriodCorrectionDrawer.jsx` — 123:input, 143:input, 186:input, 205:input, 163:select, 263:textarea
- `components/features/portal/services/TicketSubmissionWizard.jsx` — 512:button, 398:input, 418:input, 254:select, 438:textarea
- `components/view/hr/learning/annual-plans-view.jsx` — 101:button, 105:button, 210:button, 178:table, 92:hero
- `components/view/hr/learning/mandatory-rules-view.jsx` — 85:button, 204:button, 138:table, 126:input, 76:hero
- `components/view/hr/learning/skp-tracker-view.jsx` — 103:button, 107:button, 261:table, 249:input, 94:hero
- `components/view/hr/learning/training-bonds-view.jsx` — 87:button, 212:button, 142:table, 130:input, 78:hero
- `components/view/hr/learning/training-sessions-view.jsx` — 71:button, 163:button, 170:button, 85:input, 62:hero
- `components/view/hr/leave/leave-balances-admin-view.jsx` — 153:button, 171:button, 235:table, 194:input, 208:select, 146:hero
- `components/view/hr/performance/modal/CalibrationJustificationModal.jsx` — 164:button, 97:input, 111:select, 138:textarea
- `components/view/hr/performance/modal/KpiGoalFormModal.jsx` — 184:button, 119:input, 129:input, 139:input
- `components/view/hr/travel/modal/RefundSettlementModal.jsx` — 110:button, 113:button, 78:input, 89:input, 100:textarea
- `components/view/hr/travel/modal/UnderpaidPaymentModal.jsx` — 110:button, 113:button, 89:input, 77:select, 100:textarea
- `components/view/hr/travel/unified-travel-dashboard-view.jsx` — 378:button, 295:table, 267:input, 277:select, 78:hero
- `components/view/hr/travel/unified-travel-expenses-view.jsx` — 73:button, 178:button, 116:table, 89:input, 99:select, 64:hero
- `components/view/hr/travel/unified-travel-overdue-view.jsx` — 143:button, 152:button, 192:button, 195:button, 105:table, 46:hero
- `components/view/hr/travel/unified-travel-requests-view.jsx` — 156:button, 103:table, 73:input, 83:select, 52:hero
- `components/features/corporate/hr/oppe/OppeIndicatorConfigModal.jsx` — 142:input, 162:input, 184:input, 118:select
- `components/features/ess/FamilyRegistrationFormModal.jsx` — 166:input, 183:input, 214:input, 199:select, 261:select
- `components/features/hr/leave/LeaveBalanceAdjustmentModal.jsx` — 276:button, 195:input, 218:input, 242:textarea
- `components/features/hr/leave/MedicalCertificateUploadModal.jsx` — 210:button, 100:input, 121:input, 145:input
- `components/features/workforce/workforce-profile-editor-view.jsx` — 553:button, 612:button, 688:button, 901:button, 537:hero
- `components/view/hr/approvals/unified-approval-delegation-view.jsx` — 71:button, 163:button, 94:table, 62:hero
- `components/view/hr/approvals/unified-approval-matrix-view.jsx` — 115:button, 177:button, 255:button, 79:hero
- `components/view/hr/approvals/unified-ess-my-requests-view.jsx` — 167:button, 94:table, 73:input, 59:hero
- `components/view/hr/benefits/benefit-plans-view.jsx` — 199:table, 177:input, 184:select, 142:hero
- `components/view/hr/credentialing/credentialing-peer-review-view.jsx` — 147:table, 188:input, 168:select, 218:textarea, 94:hero
- `components/view/hr/credentialing/credentialing-privilege-registry-view.jsx` — 93:table, 157:table, 79:input, 70:hero
- `components/view/hr/occupational-health/modal/BpjsJkkExportModal.jsx` — 62:button, 70:button, 237:button, 240:button
- `components/view/hr/oppe/fppe-conclusions-plenary-view.jsx` — 131:table, 314:select, 331:textarea, 113:hero
- `components/view/hr/overtime/overtime-compensatory-leave-view.jsx` — 119:button, 126:button, 140:button, 224:table, 111:hero
- `components/view/hr/overtime/overtime-director-inbox-view.jsx` — 119:button, 146:table, 255:textarea, 103:hero
- `components/view/hr/payroll/payroll-run-wizard-view.jsx` — 142:select, 162:select, 185:select, 321:textarea, 99:hero
- `components/view/hr/payroll/payroll-worksheet-view.jsx` — 483:table, 406:input, 420:select, 439:select, 309:hero
- `components/view/hr/performance/modal/ClinicalLadderReviewModal.jsx` — 183:button, 154:input, 168:textarea
- `components/view/hr/performance/modal/DisputeResolutionModal.jsx` — 198:button, 161:input, 183:textarea
- `components/view/hr/restitution/deduction-schedules-view.jsx` — 236:table, 178:select, 195:select, 212:select, 140:hero
- `components/view/hr/scheduling/modals/OnCallCallInModal.jsx` — 99:input, 109:input, 138:textarea
- `components/view/hr/services/hr-service-template-editor-view.jsx` — 178:button, 137:input, 149:input, 195:textarea, 103:hero
- `components/view/hr/travel/unified-travel-advances-view.jsx` — 151:button, 160:button, 94:table, 80:input, 64:hero
- `components/features/clinical/ModalEmergencyOverrideLifeSaving.jsx` — 121:button, 177:select, 206:textarea
- `components/features/hr/benefits/DependentVerificationModal.jsx` — 164:button, 172:button, 180:button, 135:textarea
- `components/features/hr/benefits/PolicyNumberModal.jsx` — 106:input, 125:input, 143:input, 156:input
- `components/features/hr/restitution/EssClaimFormModal.jsx` — 192:input, 216:input, 167:select, 330:textarea
- `components/features/hr/restitution/ModalClaimDocumentPreview.jsx` — 127:button, 133:button, 136:button, 139:button
- `components/view/hr/approvals/modal/ReturnForRevisionModal.jsx` — 77:button, 80:button, 65:textarea
- `components/view/hr/approvals/unified-ess-revision-view.jsx` — 169:button, 122:input, 133:textarea, 78:hero
- `components/view/hr/benefits/benefit-dependents-view.jsx` — 188:table, 166:input, 173:select, 136:hero
- `components/view/hr/benefits/benefit-enrollments-view.jsx` — 214:table, 192:input, 199:select, 162:hero
- `components/view/hr/benefits/cob-claims-workstation-view.jsx` — 199:table, 177:input, 184:select, 147:hero
- `components/view/hr/credentialing/credentialing-committee-plenary-view.jsx` — 93:table, 133:select, 167:textarea, 63:hero
- `components/view/hr/credentialing/credentialing-kmkp-audit-view.jsx` — 167:table, 393:textarea, 131:hero
- `components/view/hr/leave/leave-admin-view.jsx` — 273:input, 283:input, 262:select, 293:textarea
- `components/view/hr/leave/my-leave-balance-view.jsx` — 255:button, 187:table, 92:hero
- `components/view/hr/overtime/overtime-ess-view.jsx` — 137:button, 177:table, 157:select, 106:hero
- `components/view/hr/overtime/overtime-verifications-view.jsx` — 173:button, 204:table, 187:select, 149:hero
- `components/view/hr/payroll/payroll-runs-view.jsx` — 206:table, 162:select, 180:select, 105:hero
- `components/view/hr/payroll/payroll-variances-view.jsx` — 250:table, 211:select, 227:select, 133:hero
- `components/view/hr/performance/clinical-ladder-view.jsx` — 180:button, 133:table, 69:hero
- `components/view/hr/performance/performance-calibration-view.jsx` — 98:button, 201:table, 92:hero
- `components/view/hr/performance/performance-pip-view.jsx` — 173:button, 207:table, 68:hero
- `components/view/hr/recruitment/candidates-directory-view.jsx` — 231:input, 239:input, 247:select, 324:select
- `components/view/hr/recruitment/mcu-clearance-view.jsx` — 264:input, 272:select, 410:select, 459:textarea
- `components/view/hr/recruitment/vacancies-list-view.jsx` — 222:input, 230:select, 241:select, 350:textarea
- `components/view/hr/scheduling/modals/QuickShiftReplacementModal.jsx` — 83:select, 100:select, 116:textarea
- `components/view/hr/scheduling/modals/RetroactiveShiftCorrectionModal.jsx` — 91:select, 109:select, 126:textarea
- `components/view/hr/scheduling/scheduling-settings-view.jsx` — 137:input, 172:input, 207:input, 242:input
- `components/view/hrd/relations/relations-dashboard-view.jsx` — 240:table, 294:table, 82:hero
- `components/view/hrd/relations/relations-exit-clearance-view.jsx` — 215:button, 324:button, 110:hero
- `components/view/self-services/human-resource/attendance-correction/attendance-correction-employee-view.jsx` — 272:input, 284:input, 295:input, 307:textarea
- `components/view/self-services/human-resource/overtime/overtime-employee-view.jsx` — 245:input, 255:input, 264:input, 274:textarea
- `components/features/corporate/hr/oppe/RecredentialingTierOverrideModal.jsx` — 174:input, 153:textarea
- `components/features/corporate/hr/services/ModalDocumentRevocation.jsx` — 141:input, 167:textarea
- `components/features/corporate/hr/services/ModalReassignTicket.jsx` — 139:select, 165:textarea
- `components/features/hr/benefits/GradeEligibilityDrawer.jsx` — 206:input, 221:input, 187:select
- `components/features/hr/benefits/ManualBalanceAdjustmentModal.jsx` — 150:input, 167:input, 182:textarea
- `components/features/hr/overtime/OvertimeApprovalTable.jsx` — 44:table, 191:textarea
- `components/features/hr/overtime/OvertimeDiscrepancyDrawer.jsx` — 171:button, 161:input, 194:textarea
- `components/features/hr/overtime/OvertimeReconciliationTable.jsx` — 156:table, 122:input, 130:select
- `components/view/hr/approvals/modal/ApprovalConfirmationModal.jsx` — 144:button, 102:textarea
- `components/view/hr/approvals/modal/RejectRequestModal.jsx` — 100:button, 80:textarea
- `components/view/hr/attendance/attendance-monitoring-view.jsx` — 357:input, 366:input, 376:textarea
- `components/view/hr/benefits/benefit-inhouse-ledger-view.jsx` — 295:table, 178:input, 143:hero
- `components/view/hr/benefits/benefits-dashboard-view.jsx` — 247:table, 89:select, 83:hero
- `components/view/hr/credentialing/credentialing-psv-dashboard-view.jsx` — 120:table, 237:textarea, 111:hero
- `components/view/hr/credentialing/credentialing-recredentialing-cycle-view.jsx` — 121:button, 144:table, 73:hero
- `components/view/hr/exit-clearance/exit-clearance-detail-view.jsx` — 451:textarea, 485:textarea, 517:textarea
- `components/view/hr/learning/modal/TrainingAttendanceQrModal.jsx` — 184:button
- `components/view/hr/occupational-health/ohs-dashboard-view.jsx` — 242:table, 63:hero
- `components/view/hr/oppe/recred-simrs-sync-console-view.jsx` — 143:table, 92:hero
- `components/view/hr/overtime/overtime-approvals-view.jsx` — 158:button, 132:select, 126:hero
- `components/view/hr/overtime/overtime-reconciliations-view.jsx` — 140:button, 107:hero
- `components/view/hr/performance/performance-dashboard-view.jsx` — 297:table, 72:hero
- `components/view/hr/recruitment/candidate-clinical-exam-view.jsx` — 200:select, 224:textarea, 241:textarea
- `components/view/hr/recruitment/hiring-handoffs-view.jsx` — 272:input, 410:input, 280:select
- `components/view/hr/recruitment/job-offers-list-view.jsx` — 241:input, 249:select, 263:select
- `components/view/hr/restitution/restitution-approvals-view.jsx` — 286:table, 335:input, 218:hero
- `components/view/hr/scheduling/modals/ApplyShiftPatternModal.jsx` — 54:select, 94:select
- `components/view/hr/services/portal-my-documents-view.jsx` — 317:input, 90:hero
- `components/view/hr/travel/unified-travel-rates-view.jsx` — 46:button, 88:table, 37:hero
- `components/view/self-services/human-resource/leave/leave-employee-view.jsx` — 263:input, 272:input, 281:textarea
- `components/view/self-services/human-resource/schedule/schedule-employee-view.jsx` — 240:input, 251:input, 262:textarea
- `components/features/clinical/ClinicalPrivilegeHardStopModal.jsx` — 242:button
- `components/features/clinical/ModalSupervisorCoSignature.jsx` — 237:textarea
- `components/features/corporate/hr/services/DrawerTicketProcessing.jsx` — 328:button, 295:textarea
- `components/features/corporate/hr/services/ModalDocumentAuthorization.jsx` — 263:input
- `components/features/corporate/hr/services/ProfileChangeVerificationPanel.jsx` — 93:table, 170:textarea
- `components/features/hr/leave/LeaveDateRangePicker.jsx` — 35:input, 55:input
- `components/features/hr/payroll/PayrollVarianceDrawer.jsx` — 231:textarea
- `components/view/hr/approvals/modal/BulkApprovalModal.jsx` — 207:button
- `components/view/hr/benefits/ess-my-dependents-view.jsx` — 208:table, 117:hero
- `components/view/hr/credentialing/credentialing-director-console-view.jsx` — 126:table, 61:hero
- `components/view/hr/lifecycle/clinical-preceptorships-list-view.jsx` — 132:input, 146:select
- `components/view/hr/lifecycle/employment-contract-detail-view.jsx` — 372:select
- `components/view/hr/lifecycle/employment-contracts-list-view.jsx` — 144:input, 158:select
- `components/view/hr/lifecycle/onboarding-cases-list-view.jsx` — 174:input, 188:select
- `components/view/hr/lifecycle/probations-list-view.jsx` — 134:input, 148:select
- `components/view/hr/oppe/clinical-oppe-dashboard-view.jsx` — 331:table, 82:hero
- `components/view/hr/oppe/fppe-proctoring-dashboard-view.jsx` — 179:table, 80:hero
- `components/view/hr/oppe/fppe-triggers-dashboard-view.jsx` — 141:table, 96:hero
- `components/view/hr/oppe/recred-composite-scorecard-view.jsx` — 222:table, 69:hero
- `components/view/hr/payroll/payroll-handoffs-view.jsx` — 202:table, 136:hero
- `components/view/hr/recruitment/candidate-hr-evaluation-view.jsx` — 186:select, 209:textarea
- `components/view/hr/recruitment/job-offer-detail-view.jsx` — 402:textarea, 455:textarea
- `components/view/hr/recruitment/requisitions-list-view.jsx` — 212:input, 220:select
- `components/view/hr/recruitment/talent-pool-view.jsx` — 260:input, 268:select
- `components/view/hr/restitution/deductions-dashboard-view.jsx` — 216:table, 111:hero
- `components/view/hr/restitution/ess-restitution-claims-view.jsx` — 207:table, 122:hero
- `components/view/hr/scheduling/drawers/RosterValidationDrawer.jsx` — 129:select
- `components/view/hr/scheduling/modals/CopyPreviousPeriodModal.jsx` — 53:select
- `components/view/hr/services/portal-service-requests-view.jsx` — 241:table, 147:hero
- `components/features/hr/benefits/ClaimSettlementDrawer.jsx` — 200:textarea
- `components/features/hr/payroll/PayrollApprovalCard.jsx` — 222:textarea
- `components/features/hr/restitution/DeductionCapSimulationModal.jsx` — 162:input
- `components/view/hr/credentialing/credentialing-license-radar-view.jsx` — 81:hero
- `components/view/hr/credentialing/public-spk-rkk-verify-view.jsx` — 94:table
- `components/view/hr/lifecycle/lifecycle-worklist-view.jsx` — 158:select
- `components/view/hr/lifecycle/onboarding-template-detail-view.jsx` — 254:select
- `components/view/hr/lifecycle/probation-detail-view.jsx` — 174:select
- `components/view/hr/payroll/payroll-approvals-view.jsx` — 141:hero
- `components/view/hr/payroll/payslips-ess-view.jsx` — 102:hero
- `components/view/hr/recruitment/candidate-detail-screening-view.jsx` — 317:select
- `components/view/hr/recruitment/requisition-detail-view.jsx` — 249:textarea
- `components/view/hr/recruitment/vacancy-detail-view.jsx` — 345:textarea
- `components/view/hr/workflow/approval-inbox-view.jsx` — 429:textarea


---

## Lampiran C — Endpoint yang harus diperbaiki

Kurang `/v1` (58 panggilan, tidak dirinci satu per satu). Tambahkan `/v1` pada konstanta berikut:

| Service | Konstanta | Sekarang | Seharusnya |
|---|---|---|---|
| `credentialing-service.js` | `LICENSES_URL` | `/corporate/hrd/credentialing/licenses` | `/v1/corporate/hrd/credentialing/licenses` |
| `credentialing-service.js` | `CREDENTIALING_URL` | `/corporate/hrd/credentialing` | `/v1/corporate/hrd/credentialing` |
| `credentialing-service.js` | `CLINICAL_GATE_URL` | `/corporate/hrd/clinical-privileges` | `/v1/corporate/hrd/clinical-privileges` |
| `hr-service.js` | `CATALOG_URL` | `/corporate/hrd/services` | `/v1/corporate/hrd/services` |
| `hr-service.js` | `TICKETS_URL` | `/corporate/hrd/services/tickets` | `/v1/corporate/hrd/services/tickets` |
| `hr-service.js` | `DOCUMENTS_URL` | `/corporate/hrd/services/documents` | `/v1/corporate/hrd/services/documents` |

Konstanta `ESS_TICKETS_URL` dan `PUBLIC_VERIFY_URL` pada `hr-service.js` diperiksa terhadap kontrak saat `FE-HRD-R06`.

Arti kode status bagi pengguna: **401** berarti sesi login habis atau tidak terkirim; **403** berarti
pengguna tidak punya hak untuk tindakan ini; **404** berarti alamat atau data tidak ditemukan; **409**
berarti data sudah berubah oleh orang lain; **422** berarti isian melanggar aturan bisnis.

Endpoint berlabel `Rencana (belum tersedia)` belum ada di backend.

### Corporate - HRD Business Travel Requests

Base URL: `api/v1/hrd/travel/requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `Rencana (belum tersedia)` | Ajukan SPPD untuk disetujui | - | - | - | `POST api/v1/hrd/travel/requests/{id}/submit` | `business-travel-service.js` |
| PUT | `/{id}/cancel` | Batalkan SPPD | `BusinessTravelRequest : Cancel` | `body: CancelTravelRequestDto` | `BusinessTravelResponseDto` | `POST api/v1/hrd/travel/requests/{id}/cancel` | `business-travel-service.js` |
| GET | `/{id}/document-pdf` | Unduh surat tugas | `BusinessTravelRequest : Read` | - | - | `GET api/v1/hrd/travel/requests/{id}/document` | `business-travel-service.js` |
| GET | `Rencana (belum tersedia)` | Angka serapan anggaran dinas | - | - | - | `GET api/v1/hrd/travel/analytics/budget` | `business-travel-service.js` |

### Corporate - HRD Business Travel Settlements

Base URL: `api/v1/hrd/travel/settlements`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/{id}/missing-receipts` | Deklarasi biaya tanpa kuitansi | `BusinessTravelSettlement : DeclareMissingReceipt` | `body: MissingReceiptItemDto` | `MissingReceiptDeclarationDto` | `POST api/v1/hrd/travel/settlements/missing-receipt` | `business-travel-service.js` |
| POST | `Rencana (belum tersedia)` | Catat setor balik kelebihan uang muka | - | - | - | `POST api/v1/hrd/travel/settlements/{id}/refund` | `business-travel-service.js` |
| POST | `Rencana (belum tersedia)` | Catat pembayaran kekurangan | - | - | - | `POST api/v1/hrd/travel/settlements/{id}/payout` | `business-travel-service.js` |
| GET | `Rencana (belum tersedia)` | Radar settlement terlambat | - | - | - | `GET api/v1/hrd/travel/overdue-settlements` | `business-travel-service.js` |

### Corporate - HRD Clinical Competency & OSCE Assessment

Base URL: `api/v1/corporate/hrd/training`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/sessions/{id}/attendances/check-in` | Catat kehadiran diklat | `TrainingAttendance : Record` | `body: RecordAttendanceCheckInRequestDto` | - | `POST api/v1/corporate/hrd/learning/sessions/{id}/attendances/check-in` | `learning-service.js` |
| POST | `/sessions/{id}/assessments/theory-post-test` | Simpan nilai post-test | `TheoryPostTestAssessment : Grade` | `body: SubmitTheoryPostTestRequestDto` | - | `POST api/v1/corporate/hrd/learning/assessments/theory-post-test` | `learning-service.js` |
| POST | `/sessions/{id}/assessments/clinical-osce` | Simpan nilai OSCE | `ClinicalOsceAssessment : Grade` | `body: SubmitClinicalOsceRequestDto` | - | `POST api/v1/corporate/hrd/learning/assessments/clinical-osce` | `learning-service.js` |
| POST | `/certificates/issue` | Terbitkan sertifikat | `TrainingCertificate : Issue` | `body: IssueCertificateRequestDto` | - | `POST api/v1/corporate/hrd/learning/certificates/issue` | `learning-service.js` |

### Corporate - HRD Employee Disciplinary & Just Culture

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Daftar sanksi seluruh pegawai | - | - | - | `GET api/v1/hrd/disciplinary/sanctions` | `employee-relation-service.js` |
| GET | `Rencana (belum tersedia)` | Daftar skorsing seluruh pegawai | - | - | - | `GET api/v1/hrd/disciplinary/suspensions` | `employee-relation-service.js` |

### Corporate - HRD FPPE Incident & Focused Evaluation

Base URL: `api/v1/corporate/hrd/clinical-performance/fppe`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/triggers/feed` | Daftar pemicu FPPE | `FppeFocusedEvaluation : ViewTrigger` | - | `List<FppeTriggerFeedDto>` | `GET api/v1/clinical-performance/fppe/triggers` | `clinical-performance-service.js` |
| POST | `/engagements` | Mulai penugasan FPPE | `FppeFocusedEvaluation : CreateEngagement` | `body: CreateFppeEngagementRequestDto` | `FppeEngagementResponseDto` | `POST api/v1/clinical-performance/fppe/assignments` | `clinical-performance-service.js` |
| POST | `/engagements/{id}/case-logs` | Catat kasus proctoring | `FppeFocusedEvaluation : AddCaseLog` | `body: CreateFppeCaseLogRequestDto` | `FppeCaseLogResponseDto` | `POST api/v1/clinical-performance/fppe/proctoring/{id}/cases` | `clinical-performance-service.js` |
| PUT | `/case-logs/{id}/verify` | Verifikasi kasus proctoring | `FppeFocusedEvaluation : VerifyCaseLog` | `body: VerifyFppeCaseLogRequestDto` | `FppeCaseLogResponseDto` | `PUT api/v1/clinical-performance/fppe/proctoring/cases/{id}/verify` | `clinical-performance-service.js` |
| GET | `Rencana (belum tersedia)` | Daftar kasus sidang pleno FPPE | - | - | - | `GET api/v1/clinical-performance/fppe/plenary-cases` | `clinical-performance-service.js` |
| POST | `/engagements/{id}/conclude` | Simpulkan hasil FPPE | `FppeFocusedEvaluation : ApproveConclusion` | `body: ConcludeFppeEngagementRequestDto` | `FppeConclusionResponseDto` | `POST api/v1/clinical-performance/fppe/plenary-cases/{id}/conclude` | `clinical-performance-service.js` |

### Corporate - HRD OPPE Clinical Performance Evaluation

Base URL: `api/v1/corporate/hrd/clinical-performance/oppe`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/periods` | Daftar periode OPPE | `OppeOngoingEvaluation : ViewPeriod` | `query: int?` | `List<OppeEvaluationPeriodDto>` | `GET api/v1/clinical-performance/oppe-periods` | `clinical-performance-service.js` |
| POST | `/periods` | Buat periode OPPE | `OppeOngoingEvaluation : ManagePeriod` | `body: CreateOppeEvaluationPeriodRequestDto` | `OppeEvaluationPeriodDto` | `POST api/v1/clinical-performance/oppe-periods` | `clinical-performance-service.js` |
| PUT | `Rencana (belum tersedia)` | Ubah bobot indikator KSM | - | - | - | `PUT api/v1/clinical-performance/ksm-indicators/{id}` | `clinical-performance-service.js` |
| GET | `/peer-reviews/assigned/{id}` | Daftar peer review yang ditugaskan | `OppeOngoingEvaluation : AssessPeerReview` | - | `List<AssignedPeerReviewDto>` | `GET api/v1/clinical-performance/peer-review/assignments` | `clinical-performance-service.js` |
| POST | `/peer-reviews/{id}/submit` | Kirim hasil peer review | `OppeOngoingEvaluation : AssessPeerReview` | `body: SubmitPeerReviewEvaluationRequestDto` | `bool` | `POST api/v1/clinical-performance/peer-review/assignments/{id}/submit` | `clinical-performance-service.js` |

### Corporate - HRD Occupational Health & Medical Privacy

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Daftar rekam kesehatan seluruh pegawai | - | - | - | `GET api/v1/hrd/occupational-health/records` | `occupational-health-service.js` |

### Corporate - HRD Peer Review Confidentiality & Audit Log

Base URL: `api/v1/corporate/hrd/clinical-performance/peer-review-audit`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/log-access` | Catat akses dokumen peer review | `ClinicalPeerReviewAudit : LogAccess` | `body: LogPeerReviewAccessRequestDto` | `ClinicalPeerReviewAuditLogDto` | `POST api/v1/clinical-performance/peer-review/assignments/{id}/read-audit` | `clinical-performance-service.js` |

### Corporate - HRD Performance Calibration & Dispute Resolution

Base URL: `api/v1/corporate/hrd/performance/disputes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Daftar sanggahan nilai | - | - | - | `GET api/v1/corporate/hrd/performance/disputes` | `performance-service.js` |
| POST | `/submit` | Ajukan sanggahan nilai | `PerformanceCalibration : SubmitDispute` | `query: Guid?, body: SubmitAppraisalDisputeRequestDto` | `AppraisalDisputeDto` | `POST api/v1/corporate/hrd/performance/disputes` | `performance-service.js` |
| POST | `/{id}/adjudicate` | Putuskan sanggahan nilai | `PerformanceCalibration : ManageDispute` | `query: Guid?, body: AdjudicateDisputeRequestDto` | `AppraisalDisputeDto` | `POST api/v1/corporate/hrd/performance/disputes/{id}/verdict` | `performance-service.js` |

### Corporate - HRD Performance Improvement Plan & Clinical Ladder

Base URL: `api/v1/corporate/hrd/performance/pip`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/active-list` | Daftar PIP aktif | `PerformancePipAndCareer : ViewPip` | - | `List<PipActiveSummaryDto>` | `GET api/v1/corporate/hrd/performance/pip/active` | `performance-service.js` |
| POST | `/issue-contract` | Terbitkan kontrak PIP | `PerformancePipAndCareer : ManagePip` | `body: IssuePipContractRequestDto` | `PipActiveSummaryDto` | `POST api/v1/corporate/hrd/performance/pip/contract` | `performance-service.js` |
| GET | `Rencana (belum tersedia)` | Daftar jenjang karier klinis | - | - | - | `GET api/v1/corporate/hrd/performance/clinical-ladder` | `performance-service.js` |

### Corporate - HRD Plataran Sehat SKP & OPPE Feed

Base URL: `api/v1/corporate/hrd/training/plataran-sehat-skp`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/portfolio/{id}` | Ringkasan SKP pegawai | `PlataranSehatSkp : Read` | - | - | `GET api/v1/corporate/hrd/learning/skp/staff/{id}/summary` | `learning-service.js` |
| POST | `/` | Catat kegiatan SKP | `PlataranSehatSkp : Create` | `body: RecordSkpActivityRequestDto` | - | `POST api/v1/corporate/hrd/learning/skp/records` | `learning-service.js` |
| POST | `/sync-oppe/{id}` | Sinkronkan SKP | `PlataranSehatSkp : Sync` | - | - | `POST api/v1/corporate/hrd/learning/skp/sync-plataran-sehat/{id}` | `learning-service.js` |

### Corporate - HRD Recredentialing Quality & Composite Scorecard

Base URL: `api/v1/corporate/hrd/credentialing/recredentialing`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/scorecards/{id}/override-tier` | Sesuaikan tier rekredensial | `RecredentialingQuality : Decide` | `body: OverrideRecredTierRequestDto` | `RecredentialingScorecardDto` | `POST api/v1/clinical-performance/recredentialing/{id}/override-tier` | `clinical-performance-service.js` |
| GET | `/simrs-sync/audit-logs` | Status kirim putusan ke SIMRS | `RecredentialingQuality : ViewSyncAudit` | - | `List<SimrsPrivilegeSyncAuditLogDto>` | `GET api/v1/clinical-performance/simrs-sync/status` | `clinical-performance-service.js` |
| POST | `/verdicts/{id}/dispatch-simrs` | Kirim putusan ke SIMRS | `RecredentialingQuality : DispatchSimrs` | `body: DispatchSimrsPrivilegeRequestDto` | `SimrsPrivilegeSyncAuditLogDto` | `POST api/v1/clinical-performance/simrs-sync/dispatch` | `clinical-performance-service.js` |

### Corporate - HRD Staff Vaccination & Immunity Tracking

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Daftar vaksinasi seluruh pegawai | - | - | - | `GET api/v1/hrd/occupational-health/vaccinations` | `occupational-health-service.js` |

### Corporate - HRD Training Enrollment & Budget Governance

Base URL: `api/v1/corporate/hrd/training/bonding-contracts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/staff/{id}` | Kontrak ikatan dinas pegawai | `TrainingBonding : Read` | - | - | `GET api/v1/corporate/hrd/learning/bonds/staff/{id}` | `learning-service.js` |
| POST | `/` | Buat kontrak ikatan dinas | `TrainingBonding : Manage` | `body: GenerateBondContractRequestDto` | - | `POST api/v1/corporate/hrd/learning/bonds/generate-contract` | `learning-service.js` |
| POST | `/{id}/process-breach-penalty` | Proses pelanggaran ikatan dinas | `TrainingBonding : Manage` | `body: ProcessBreachPenaltyRequestDto` | - | `POST api/v1/corporate/hrd/learning/bonds/{id}/record-breach` | `learning-service.js` |

### Corporate - HRD Work Restriction & Injury BPJS JKK

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Daftar pembatasan kerja | - | - | - | `GET api/v1/hrd/occupational-health/restrictions` | `occupational-health-service.js` |
| GET | `Rencana (belum tersedia)` | Daftar kecelakaan kerja | - | - | - | `GET api/v1/hrd/occupational-health/injuries` | `occupational-health-service.js` |

### Corporate - HRD Workflow Approval Actions

Base URL: `api/v1/hrd/workflow/approvals`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/{id}/approve` | Setujui permohonan | `WorkflowApprovalAction : ApproveRequest` | `body: ApproveActionRequestDto` | `WorkflowActionV2ResponseDto` | `POST api/v1/hrd/workflow/inbox/{id}/approve` | `unified-approval-service.js` |
| POST | `/{id}/reject` | Tolak permohonan | `WorkflowApprovalAction : RejectRequest` | `body: RejectActionRequestDto` | `WorkflowActionV2ResponseDto` | `POST api/v1/hrd/workflow/inbox/{id}/reject` | `unified-approval-service.js` |
| POST | `/{id}/return` | Kembalikan untuk diperbaiki | `WorkflowApprovalAction : ReturnForRevision` | `body: ReturnForRevisionRequestDto` | `WorkflowActionV2ResponseDto` | `POST api/v1/hrd/workflow/inbox/{id}/return` | `unified-approval-service.js` |
| POST | `/bulk-approve` | Setujui massal | `WorkflowApprovalAction : BulkApprove` | `body: BulkApproveRequestDto` | `BulkApproveResultDto` | `POST api/v1/hrd/workflow/inbox/bulk-approve` | `unified-approval-service.js` |

### Corporate - HRD Workflow Delegations

Base URL: `api/v1/hrd/workflow/delegations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| PUT | `/{id}/revoke` | Cabut delegasi Plh | `WorkflowDelegation : RevokeDelegation` | `body: RevokeDelegationRequestDto` | `ApprovalDelegationV2ResponseDto` | `DELETE api/v1/hrd/workflow/delegations/{id}` | `unified-approval-service.js` |

### Corporate - HRD Workflow Inbox & Instances

Base URL: `api/v1/hrd/workflow/instances`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/{id}` | Detail permohonan | `WorkflowInboxInstance : ViewInstanceDetail` | - | `WorkflowInstanceDetailV2ResponseDto` | `GET api/v1/hrd/workflow/inbox/{id}` | `unified-approval-service.js` |

### Corporate / Human Resource / Lifecycle Management / Employment Contracts

Base URL: `api/v1/corporate/human-resource/employment-contracts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/{id}/print-data` | Data cetak kontrak | `WorkforceEmploymentContract : Read` | - | `EmploymentContractPrintDataResponse` | `GET api/v1/corporate/human-resource/employment-contracts/{id}/print` | `lifecycle-service.js` |
| POST | `/{id}/record-signed` | Catat kontrak ditandatangani | `WorkforceEmploymentContract : MarkSigned` | `form: RecordContractSignedRequest` | `EmploymentContractResponse` | `POST api/v1/corporate/human-resource/employment-contracts/{id}/mark-signed` | `lifecycle-service.js` |
| POST | `/{id}/decisions` | Putuskan kontrak | `WorkforceEmploymentContract : SubmitDecision` | `body: SubmitContractDecisionRequest` | `ContractDecisionResponse` | `POST api/v1/corporate/human-resource/employment-contracts/{id}/decision` | `lifecycle-service.js` |
| POST | `/{id}/corrections` | Koreksi kontrak | `WorkforceEmploymentContract : RequestCorrection` | `form: SubmitContractCorrectionRequest` | `ContractCorrectionResponse` | `POST api/v1/corporate/human-resource/employment-contracts/{id}/correction` | `lifecycle-service.js` |

### Corporate / Human Resource / Lifecycle Management / Initial Contract Batch

Base URL: `api/v1/corporate/human-resource/initial-contract-batches`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/{id}/rows` | Unggah baris batch kontrak | `WorkforceInitialContractBatch : Update` | `body: AddInitialContractBatchRowRequest` | - | `POST api/v1/corporate/human-resource/initial-contract-batches/upload` | `lifecycle-service.js` |

### Corporate / Human Resource / Lifecycle Management / Probation

Base URL: `api/v1/corporate/human-resource/probations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/{id}/assessments` | Simpan penilaian masa percobaan | `WorkforceProbation : Submit` | - | - | `POST api/v1/corporate/human-resource/probations/{id}/evaluate` | `lifecycle-service.js` |

### Corporate / Human Resource / Overtime Management / Overtime Plan

Base URL: `api/v1/corporate/human-resource/overtime-management`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/plans` | Daftar rencana lembur | `OvertimePlan : Read` | `query: OvertimePlanQueryRequest` | - | `GET api/v1/corporate/human-resource/overtime/plans` | `overtime-service.js` |
| PUT | `/plans/{id}` | Ubah rencana lembur | `OvertimePlan : Update` | `body: UpdateOvertimePlanRequest` | - | `PUT api/v1/corporate/human-resource/overtime/plans/{id}` | `overtime-service.js` |
| POST | `/plans/{id}/validate` | Validasi rencana lembur | `OvertimePlan : Validate` | - | - | `POST api/v1/corporate/human-resource/overtime/plans/{id}/validate` | `overtime-service.js` |
| POST | `/plans/{id}/publish` | Terbitkan rencana lembur | `OvertimePlan : Publish` | - | - | `POST api/v1/corporate/human-resource/overtime/plans/{id}/publish` | `overtime-service.js` |

### Corporate / Human Resource / Overtime Management / Overtime Realization

Base URL: `api/v1/corporate/human-resource/overtime-management`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/realizations` | Daftar realisasi lembur | `OvertimeRealization : Read` | `query: OvertimeRealizationQueryRequest` | - | `GET api/v1/corporate/human-resource/overtime/realizations` | `overtime-service.js` |

### Corporate / Human Resource / Workflow Management / Approval Inbox

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `Rencana (belum tersedia)` | Setujui banyak tugas sekaligus di kotak masuk lama | - | - | - | `POST api/v1/corporate/human-resource/approval-inbox/batch-approve` | `approval-inbox-service.js` |

### Corporate / Human Resource / Workforce Core / Employee Profile Change Administration

Base URL: `api/v1/corporate/human-resource/employee-profile-changes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/{id}/start-verification` | Mulai verifikasi perubahan data | `EmployeeProfileChange : Update` | - | - | `POST api/v1/corporate/human-resource/employee-profile-changes/{id}/verify/start` | `workforce-core-cross-employee-service.js` |

### HR Benefits - COB Claims & Settlement

Base URL: `api/hr-benefits`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/claims` | Daftar klaim COB | `BenefitClaim : Read` | `query: BenefitClaimListQuery` | - | `GET api/hr-benefits` | `benefits-service.js` |
| GET | `/claims/my-claims` | Klaim milik pegawai | `BenefitClaim : ViewOwn` | - | - | `GET api/hr-benefits/my-claims` | `benefits-service.js` |
| GET | `/claims/{id}` | Detail klaim | `BenefitClaim : Read` | - | - | `GET api/hr-benefits/{id}` | `benefits-service.js` |
| POST | `/claims` | Ajukan klaim | `BenefitClaim : SubmitClaim` | `body: CreateEssClaimRequestDto` | - | `POST api/hr-benefits` | `benefits-service.js` |
| POST | `/claims/{id}/verify` | Verifikasi klaim | `BenefitClaim : Verify` | `body: VerifyClaimRequestDto` | - | `POST api/hr-benefits/{id}/verify` | `benefits-service.js` |
| POST | `/claims/{id}/settle` | Selesaikan pembayaran klaim | `BenefitClaim : Settle` | `body: SettleClaimRequestDto` | - | `POST api/hr-benefits/{id}/settle` | `benefits-service.js` |
| GET | `Rencana (belum tersedia)` | Angka dasbor manfaat | - | - | - | `GET api/hr-benefits/analytics` | `benefits-service.js` |

### HR Leave - Requests & ESS

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Daftar cuti seluruh pegawai | - | - | - | `GET api/v1/corporate/human-resource/leave/requests` | `leave-admin-service.js` |

### HR Overtime - Realizations & Cito Claims

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `Rencana (belum tersedia)` | Unggah logbook lembur | - | - | - | `POST api/v1/corporate/human-resource/overtime/realizations/upload-logbook` | `overtime-service.js` |

### HR Payroll - Runs & Batches

Base URL: `api/v1/corporate/human-resource/payroll/runs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/{id}/calculate-pph21` | Hitung PPh 21 | `PayrollRunModern : CalculatePph21` | - | `CalculatePph21ResultDto` | `POST api/v1/corporate/human-resource/payroll/runs/{id}/calculate-tax` | `payroll-service.js` |
| POST | `/{id}/evaluate-variances` | Periksa anomali gaji | `PayrollRunModern : EvaluateVariances` | - | `EvaluateVarianceResultDto` | `POST api/v1/corporate/human-resource/payroll/runs/{id}/run-variance-check` | `payroll-service.js` |
| POST | `/{id}/submit` | Ajukan persetujuan gaji | `PayrollRunModern : SubmitForReview` | `body: SubmitPayrollRunCommand` | `PayrollApprovalResultDto` | `POST api/v1/corporate/human-resource/payroll/runs/{id}/submit-approval` | `payroll-service.js` |
| POST | `/{id}/approve-final` | Setujui penggajian | `PayrollRunModern : ApproveFinalByDirector` | - | `PayrollApprovalResultDto` | `POST api/v1/corporate/human-resource/payroll/runs/{id}/approve` | `payroll-service.js` |
| GET | `/{id}/approvals` | Riwayat persetujuan gaji | `PayrollRunModern : ViewApprovalHistory` | - | `List<PayrollApprovalHistoryDto>` | `GET api/v1/corporate/human-resource/payroll/runs/{id}/approvals/history` | `payroll-service.js` |
| POST | `/{id}/dispatch-finance-handoff` | Serahkan ke Keuangan | `PayrollRunModern : DispatchFinanceHandoff` | - | `FinanceHandoffBatchResultDto` | `POST api/v1/corporate/human-resource/payroll/runs/{id}/finance-handoff` | `payroll-service.js` |

### HRD Clinical Preceptorships

Base URL: `api/v1/corporate/human-resource/clinical-preceptorships`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/{id}/declare-independent` | Nyatakan siap mandiri | `WorkforcePreceptorship : Manage` | `form: DeclareClinicalIndependentRequest` | - | `POST api/v1/corporate/human-resource/clinical-preceptorships/{id}/independent` | `lifecycle-service.js` |
| PUT | `/{id}/preceptor` | Ganti pendamping klinis | `WorkforcePreceptorship : Manage` | `body: ChangePreceptorRequest` | - | `POST api/v1/corporate/human-resource/clinical-preceptorships/{id}/change-preceptor` | `lifecycle-service.js` |

### HRD Onboarding Cases

Base URL: `api/v1/corporate/human-resource/onboarding-cases`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| POST | `/{id}/first-day-attendance` | Catat hadir hari pertama | `WorkforceOnboardingCase : CompleteItem` | `body: RecordFirstDayAttendanceRequest` | - | `POST api/v1/corporate/human-resource/onboarding-cases/{id}/first-day-attended` | `lifecycle-service.js` |
| POST | `/{id}/items/{id}/uncomplete` | Batalkan centang checklist | `WorkforceOnboardingCase : CompleteItem` | - | - | `POST api/v1/corporate/human-resource/onboarding-cases/{id}/items/{id}/uncheck` | `lifecycle-service.js` |

### Layar lama

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Daftar tindakan disiplin (`K-02`) | - | - | - | `GET api/v1/hr/workforce-profile/disciplinary-actions` | `employee-relations-service.js` |
| GET | `Rencana (belum tersedia)` | Detail tindakan disiplin (`K-02`) | - | - | - | `GET api/v1/hr/workforce-profile/disciplinary-actions/{id}` | `employee-relations-service.js` |
| GET | `Rencana (belum tersedia)` | Rekam pelatihan (`K-02`) | - | - | - | `GET api/v1/hr/workforce-profile/training-records` | `learning-development-service.js` |
| GET | `Rencana (belum tersedia)` | Sertifikasi (`K-02`) | - | - | - | `GET api/v1/hr/workforce-profile/certifications` | `learning-development-service.js` |
| GET | `Rencana (belum tersedia)` | Daftar lembur layar admin lama (`K-03`) | - | - | - | `GET api/v1/hr/overtime/requests` | `overtime-admin-service.js` |
| GET | `Rencana (belum tersedia)` | Perhitungan lembur layar admin lama (`K-03`) | - | - | - | `GET api/v1/hr/overtime/calculations` | `overtime-admin-service.js` |
| GET | `Rencana (belum tersedia)` | Daftar penilaian kinerja (`K-02`) | - | - | - | `GET api/v1/hr/workforce-profile/performance-reviews` | `performance-management-service.js` |
| GET | `Rencana (belum tersedia)` | Detail penilaian kinerja (`K-02`) | - | - | - | `GET api/v1/hr/workforce-profile/performance-reviews/{id}` | `performance-management-service.js` |

### Payroll

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Daftar persetujuan gaji tertunda | - | - | - | `GET api/v1/corporate/human-resource/payroll/runs/approvals/pending` | `payroll-service.js` |

### Self Services / Human Resource / Overtime

Base URL: `api/v1/self-services/human-resource`

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `/overtime` | Klaim lembur milik pegawai | `MyOvertime : Read` | `query: MyOvertimeQueryRequest` | - | `GET api/v1/self-services/human-resource/overtime/claims` | `employee-self-service.js` |
| POST | `/overtime` | Ajukan klaim lembur | `MyOvertime : Create` | `body: CreateMyOvertimeRequest` | - | `POST api/v1/self-services/human-resource/overtime/claims` | `employee-self-service.js` |

### Self Services / Human Resource / Schedule Change

| Method | Path | Kegunaan | Hak akses | Request | Response | Dipanggil frontend sekarang | Service |
|---|---|---|---|---|---|---|---|
| GET | `Rencana (belum tersedia)` | Jadwal kerja milik pegawai | - | - | - | `GET api/v1/self-services/human-resource/schedule/assignments` | `employee-self-service.js` |


