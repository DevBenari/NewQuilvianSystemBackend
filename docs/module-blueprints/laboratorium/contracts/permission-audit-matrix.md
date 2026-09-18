# Permission dan Audit Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-PERM-v1` |
| Revision | `7` |
| Revision 7 approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18** |
| Isi amandemen revision 7 | **`approved` — 2026-09-18.** Dua resource baru — `LabPathologyParameter` dan `LabPathologyCategory`, masing-masing `Read`/`Create`/`Update`, **nol `Delete`**; keberlakuan parameter dan pemetaan jenis pemeriksaan ikut `LabPathologyCategory : Update`, bukan resource sendiri. Mengisi, memfinalkan, dan membuka kembali laporan PA **tidak menambah hak akses** — memakai `LabExamination : Update` yang sudah ada. **Satu pemisahan yang disengaja: konteks klinis pesanan memakai `LabOrder : Update`**, sebab penulisnya **dokter pemesan, bukan patolog** (`LAB-DEC-091`, `INV-40`). Lima kejadian audit baru; **`PathologyReport.Reopen` dan `PathologyReport.AmendValue` wajib beralasan**. Membawa **pembatasan logger dan DTO paling ketat pada modul ini**: nol isi parameter, nol diagnosa, nol riwayat penyakit boleh masuk log atau layar non-klinis. Disetujui bersama `LAB-API-v1` `r25` dan `LAB-VAL-v1` `r8` pada hari yang sama. Lihat bagian 9 |
| Revision 6 approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18** |
| Isi amandemen revision 6 | **`approved` — 2026-09-18.** Dua resource baru — `LabOrganism` dan `LabAntibiotic`, masing-masing `Read`/`Create`/`Update`, **nol `Delete`**. Pengisian hasil Mikrobiologi dan Patologi Anatomi **tidak menambah hak akses**: keduanya memakai `LabExamination : Update` yang sudah ada, dan pemecahan izin per disiplin **sengaja ditolak** karena `LAB-OPEN-034` belum dijawab. Empat kejadian audit baru; dua di antaranya — penghapusan baris isolat dan baris kepekaan — **wajib beralasan** |
| Status | `approved` — revision 1-3 dikunci 2026-09-02; **revision 4 disetujui pemilik modul 2026-09-14**; **revision 5 disetujui 2026-09-15** |
| Revision 5 approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-15 |
| Revision 4 approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-14 |
| Isi amandemen revision 4 | Satu resource baru `LabSpecimenType` dengan tiga action (`Read`, `Create`, `Update`; tidak ada `Delete`). Tiga kemampuan baru lain **tidak menambah hak akses** karena sudah dijaga `LabSpecimen : Plan`, `LabExamination : Create`, dan `LabSpecimen : Accept`. Dua kejadian audit baru ditambahkan, termasuk kewajiban mencatat selisih waktu penerimaan fisik terhadap `ReceivedAt` sistem |
| Batas penguncian | **Terkunci penuh sejak 2026-09-02.** `LAB-OPEN-021` dijawab: penamaan memakai prefix `Lab`, sehingga tidak ada lagi bagian yang dikecualikan |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Backend SHA | `c87d9c0` |

String `[AccessPermission(...)]` ditulis apa adanya agar implementer menyalin, bukan
menerjemahkan.

Konvensi project: **`GET` tidak dicatat logger.** Create, Update, perpindahan status, dan
Delete dicatat. Payload log hanya memuat `EntityId`, controller, action, dan status — **tidak
boleh** memuat kolom bertanda sensitif pada kamus data.

---

## 1. Kewenangan yang Sudah Ada

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /lab-orders` | `LabOrder` | `Read` | `[AccessPermission("LabOrder", "Read")]` | Tidak |
| `GET /lab-orders/{id}` | `LabOrder` | `Read` | `[AccessPermission("LabOrder", "Read")]` | Tidak |
| `POST /lab-orders` | `LabOrder` | `Create` | `[AccessPermission("LabOrder", "Create")]` | Ya |
| `POST /lab-orders/by-examinations` | `LabOrder` | `Create` | `[AccessPermission("LabOrder", "Create")]` | Ya |
| `PUT /lab-orders/{id}/start-process` | `LabOrder` | `Process` | `[AccessPermission("LabOrder", "Process")]` | Ya |
| `PUT /lab-orders/{id}/complete` | `LabOrder` | `Process` | `[AccessPermission("LabOrder", "Process")]` | Ya |
| `PUT /lab-orders/{id}/hold` | `LabOrder` | `Hold` | `[AccessPermission("LabOrder", "Hold")]` | Ya |
| `PUT /lab-orders/{id}/resume` | `LabOrder` | `Hold` | `[AccessPermission("LabOrder", "Hold")]` | Ya |
| `PUT /lab-orders/{id}/cancel` | `LabOrder` | `Update` | `[AccessPermission("LabOrder", "Update")]` | Ya |
| `GET /lab-specimens/rejection-reasons` | `LabSpecimen` | `Read` | `[AccessPermission("LabSpecimen", "Read")]` | Tidak |
| `GET /lab-specimens/by-order/{id}` | `LabSpecimen` | `Read` | `[AccessPermission("LabSpecimen", "Read")]` | Tidak |
| `GET /lab-specimens/by-order/{id}/history` | `LabSpecimen` | `Read` | `[AccessPermission("LabSpecimen", "Read")]` | Tidak |
| `POST /lab-specimens/by-order/{id}` | `LabSpecimen` | `Plan` | `[AccessPermission("LabSpecimen", "Plan")]` | Ya |
| `POST /lab-specimens/{id}/collect` | `LabSpecimen` | `Collect` | `[AccessPermission("LabSpecimen", "Collect")]` | Ya |
| `POST /lab-specimens/{id}/receive` | `LabSpecimen` | `Receive` | `[AccessPermission("LabSpecimen", "Receive")]` | Ya |
| `POST /lab-specimens/{id}/accept` | `LabSpecimen` | `Accept` | `[AccessPermission("LabSpecimen", "Accept")]` | Ya |
| `POST /lab-specimens/{id}/reject` | `LabSpecimen` | `Accept` | `[AccessPermission("LabSpecimen", "Accept")]` | Ya |
| `POST /lab-specimens/{id}/request-recollection` | `LabSpecimen` | `Accept` | `[AccessPermission("LabSpecimen", "Accept")]` | Ya |
| `POST /lab-specimens/{id}/hold` | `LabSpecimen` | `Hold` | `[AccessPermission("LabSpecimen", "Hold")]` | Ya |
| `POST /lab-specimens/{id}/resume` | `LabSpecimen` | `Hold` | `[AccessPermission("LabSpecimen", "Hold")]` | Ya |
| `POST /lab-specimens/{id}/cancel` | `LabSpecimen` | `Update` | `[AccessPermission("LabSpecimen", "Update")]` | Ya |

**Catatan.** Menolak dan meminta ambil ulang sengaja memakai kewenangan yang sama dengan
menyatakan layak, karena `LAB-INH-007` memperlakukan "penerimaan/penolakan" sebagai satu
kewenangan. Yang dipisah tegas adalah **mengambil** dan **menetapkan kelayakan** — dan
pemisahan itu sudah dijaga pengujian
`LaboratoryAuthorityTests.cs#PermissionPengambilanDanPenetapanLayak_TidakBolehSama@c87d9c0`.

---

## 2. Kewenangan Baru

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /lab-orders/by-discipline/{discipline}` | `LabOrder` | `Read` | `[AccessPermission("LabOrder", "Read")]` | Tidak |
| `PUT /lab-examinations/{id}/urgency` | `LabExamination` | `Update` | `[AccessPermission("LabExamination", "Update")]` | Ya |
| `PUT /lab-examinations/{id}/duplo` | `LabExamination` | `Update` | `[AccessPermission("LabExamination", "Update")]` | Ya |
| `GET /lab-examinations/by-order/{id}` | `LabExamination` | `Read` | `[AccessPermission("LabExamination", "Read")]` | Tidak |
| `GET /lab-examinations/by-specimen/{id}` | `LabExamination` | `Read` | `[AccessPermission("LabExamination", "Read")]` | Tidak |
| `POST /lab-examinations/by-order/{id}` | `LabExamination` | `Create` | `[AccessPermission("LabExamination", "Create")]` | Ya |
| `POST /lab-examinations/{id}/cancel` | `LabExamination` | `Update` | `[AccessPermission("LabExamination", "Update")]` | Ya |
| `GET /lab-value-bounds` | `LabValueBound` | `Read` | `[AccessPermission("LabValueBound", "Read")]` | Tidak |
| `GET /lab-value-bounds/{id}` | `LabValueBound` | `Read` | `[AccessPermission("LabValueBound", "Read")]` | Tidak |
| `GET /lab-value-bounds/{id}/history` | `LabValueBound` | `Read` | `[AccessPermission("LabValueBound", "Read")]` | Tidak |
| `POST /lab-value-bounds` | `LabValueBound` | `Create` | `[AccessPermission("LabValueBound", "Create")]` | Ya |
| `PUT /lab-value-bounds/{id}` | `LabValueBound` | `Update` | `[AccessPermission("LabValueBound", "Update")]` | Ya |
| `PUT /lab-value-bounds/{id}/deactivate` | `LabValueBound` | `Update` | `[AccessPermission("LabValueBound", "Update")]` | Ya |
| `GET /lab-value-bounds/{id}/critical-change-requests` | `LabCriticalBound` | `Read` | `[AccessPermission("LabCriticalBound", "Read")]` | Tidak |
| `POST /lab-value-bounds/{id}/critical-change-requests` | `LabValueBound` | `Update` | `[AccessPermission("LabValueBound", "Update")]` | Ya |
| `POST /…/critical-change-requests/{id}/approve` | `LabCriticalBound` | `Approve` | `[AccessPermission("LabCriticalBound", "Approve")]` | Ya |
| `POST /…/critical-change-requests/{id}/reject` | `LabCriticalBound` | `Approve` | `[AccessPermission("LabCriticalBound", "Approve")]` | Ya |
| `POST /…/critical-change-requests/{id}/withdraw` | `LabValueBound` | `Update` | `[AccessPermission("LabValueBound", "Update")]` | Ya |
| `GET /lab-worklists/pending` | `LabWorklist` | `Read` | `[AccessPermission("LabWorklist", "Read")]` | Tidak |
| `GET /lab-worklists/cito-overdue` | `LabWorklist` | `Read` | `[AccessPermission("LabWorklist", "Read")]` | Tidak |
| `GET /lab-rejection-reasons` | `LabRejectionReason` | `Read` | `[AccessPermission("LabRejectionReason", "Read")]` | Tidak |
| `POST /lab-rejection-reasons` | `LabRejectionReason` | `Create` | `[AccessPermission("LabRejectionReason", "Create")]` | Ya |
| `PUT /lab-rejection-reasons/{id}` | `LabRejectionReason` | `Update` | `[AccessPermission("LabRejectionReason", "Update")]` | Ya |
| `PUT /lab-rejection-reasons/{id}/activation` | `LabRejectionReason` | `Update` | `[AccessPermission("LabRejectionReason", "Update")]` | Ya |
| `PUT /lab-rejection-reasons/{id}/system-flags` | `LabRejectionReason` | `SystemFlag` | `[AccessPermission("LabRejectionReason", "SystemFlag")]` | Ya |

### Kewenangan untuk slice pendaftaran, katalog, dan monitoring

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /lab-patient-registrations/patient-search` | `LabPatientRegistration` | `Read` | `[AccessPermission("LabPatientRegistration", "Read")]` | Tidak |
| `POST /lab-patient-registrations/walk-in` | `LabPatientRegistration` | `Create` | `[AccessPermission("LabPatientRegistration", "Create")]` | Ya |
| `POST /lab-patient-registrations/external-referral` | `LabPatientRegistration` | `Create` | `[AccessPermission("LabPatientRegistration", "Create")]` | Ya |
| `GET /lab-catalog/examinations` | `LabCatalog` | `Read` | `[AccessPermission("LabCatalog", "Read")]` | Tidak |
| `GET /lab-catalog/examinations/{procedureId}/price` | `LabCatalog` | `Read` | `[AccessPermission("LabCatalog", "Read")]` | Tidak |
| `GET /lab-catalog/tariffs` | `LabCatalog` | `Read` | `[AccessPermission("LabCatalog", "Read")]` | Tidak |
| `GET /lab-monitoring/clinical-pathology` | `LabMonitoring` | `Read` | `[AccessPermission("LabMonitoring", "Read")]` | Tidak |
| `GET /lab-monitoring/anatomic-pathology` | `LabMonitoring` | `Read` | `[AccessPermission("LabMonitoring", "Read")]` | Tidak |
| `GET /lab-monitoring/microbiology` | `LabMonitoring` | `Read` | `[AccessPermission("LabMonitoring", "Read")]` | Tidak |

**Catatan tentang `LabCatalog`.** Seluruh aksinya hanya `Read`. **Tidak ada** aksi `Create`,
`Update`, maupun `Delete` — karena Laboratorium hanya menyajikan, tidak mengelola
(`LAB-DEC-033`).

**Catatan tentang `LabPatientRegistration`.** Kewenangan ini mengizinkan petugas **mengajukan**
pembuatan kunjungan. Apakah pengajuan itu diterima tetap diputuskan Registrasi menurut
kewenangannya sendiri. Punya `LabPatientRegistration : Create` **tidak berarti** otomatis boleh
membuat kunjungan.

### Dua kewenangan yang memisahkan wewenang secara sengaja

| Kewenangan | Kenapa dipisah | Decision ID |
|---|---|---|
| `LabCriticalBound : Approve` | Perubahan batas kritis menentukan kapan pasien dinyatakan dalam bahaya. Pemegangnya adalah pihak klinis, bukan pengelola data induk | `LAB-DEC-023` |
| `LabRejectionReason : SystemFlag` | Penanda kesalahan internal menentukan siapa menanggung biaya ambil ulang. Menurut `LAB-INH-010`, akibat finansial bukan wewenang Laboratorium | `LAB-DEC-019` |

Keduanya **tidak boleh** diberikan bersamaan dengan kewenangan pengelolaan biasa kepada orang
yang sama, karena itu menghapus makna pemisahannya.

---

## 3. Pembatasan Tambahan di Luar Sistem Kewenangan

Beberapa aturan tidak dapat dinyatakan lewat kewenangan per aksi, dan wajib ditegakkan sebagai
aturan bisnis di dalam service.

| Aturan | Kenapa tidak cukup lewat kewenangan | Ditegakkan di |
|---|---|---|
| Hanya dokter pemesan yang boleh menandai cito | Kewenangan menjawab "boleh menandai?", bukan "apakah ini pesanan miliknya?" | `LabOrderService` |
| Pengambil sampel tidak boleh menyatakan kelayakan wadah yang sama | Kewenangan tidak membandingkan pelaku sebelumnya pada baris yang sama | `LabSpecimenService` |
| Pengaju perubahan batas kritis tidak boleh menyetujui pengajuannya sendiri | Sama seperti di atas | `LabValueBoundService` |

**Catatan penting untuk implementer.** Ketiganya adalah pola yang sama dengan `CAP-16` pada
capability map: sistem kewenangan bekerja **per aksi**, bukan **per orang pada satu baris
data**. Menganggapnya bisa ditutup permission adalah kesalahan yang paling mahal pada modul ini.

---

## 4. Kejadian yang Wajib Menghasilkan Jejak Audit

Selain logger, perpindahan status berikut wajib menghasilkan satu baris permanen pada
`LabTransitionHistory`.

| Kejadian | `Scope` | `Action` | Alasan wajib |
|---|---|---|:---:|
| Pesanan dibuat | `LabOrder` | `Order.Create` | Tidak |
| Pesanan ditandai cito atau dikembalikan biasa | `LabOrder` | `Order.SetUrgency` | Tidak |
| Pesanan mulai dikerjakan | `LabOrder` | `Order.StartProcess` | Tidak |
| Pesanan diselesaikan | `LabOrder` | `Order.Complete` | Tidak |
| Pesanan ditahan atau dilanjutkan | `LabOrder` | `Order.Hold` / `Order.Resume` | **Ya** |
| Pesanan dibatalkan | `LabOrder` | `Order.Cancel` | **Ya** |
| Wadah direncanakan | `LabSpecimen` | `Specimen.Plan` | Tidak |
| Wadah diambil | `LabSpecimen` | `Specimen.Collect` | Tidak |
| Wadah diterima | `LabSpecimen` | `Specimen.Receive` | Tidak |
| Wadah dinyatakan layak | `LabSpecimen` | `Specimen.Accept` | Tidak |
| Wadah ditolak | `LabSpecimen` | `Specimen.Reject` | **Ya** |
| Ambil ulang diminta | `LabSpecimen` | `Specimen.RequestRecollection` | **Ya** |
| Wadah ditahan, dilanjutkan, dibatalkan | `LabSpecimen` | `Specimen.Hold` / `Resume` / `Cancel` | **Ya** |
| Pemeriksaan ditambahkan | `LabExamination` | `Examination.Add` | Tidak |
| Pemeriksaan menjadi layak tagih | `LabExamination` | `Examination.ChargeEligible` | Tidak |
| Pemeriksaan gugur karena wadah ditolak | `LabExamination` | `Examination.Void` | Tidak |
| Pemeriksaan dibatalkan | `LabExamination` | `Examination.Cancel` | **Ya** |
| Pemeriksaan ditandai cito atau dikembalikan biasa | `LabExamination` | `Examination.SetUrgency` | Tidak |
| Pemeriksaan ditandai dikerjakan ganda atau penandaannya dibatalkan | `LabExamination` | `Examination.SetDuplo` | Tidak |

Setiap baris menyimpan status asal dan tujuan, pelaku, waktu, dan penghubung rangkaian tindakan.

### Jejak terpisah untuk batas nilai

| Kejadian | Disimpan di | Menyimpan penyetuju |
|---|---|:---:|
| Perubahan satuan, batas normal, daftar pilihan, batas waktu cito | `LabValueBoundHistory` | Tidak |
| Perubahan batas kritis yang disetujui | `LabValueBoundHistory` | **Ya** |
| Pengajuan, persetujuan, penolakan, penarikan | `LabValueBoundChangeRequest` | **Ya** |

---

## 5. Privasi

| Kolom sensitif | Tabel | Aturan |
|---|---|---|
| `RejectionNote` | `LabSpecimen` | Tidak masuk logger; tinjau penyamaran pada response bagi pengguna non-klinis |
| `RecollectionReason` | `LabSpecimen` | Sama seperti di atas |
| `ReasonNote` | `LabTransitionHistory` | Sama seperti di atas |

Barcode wadah **tidak** memuat identitas pasien — sudah dijaga pengujian
`#BarcodeSampel_UnikDanTidakMemuatIdentitasPasien@c87d9c0`. Aturan itu tetap berlaku setelah
pemisahan wadah dan pemeriksaan.

---

## 6. Traceability

| Kewenangan | Decision ID | Acceptance criteria |
|---|---|---|
| `LabExamination : Update` untuk cito dan duplo | `LAB-DEC-013`, `LAB-DEC-026` | AC-18, AC-39, AC-40 |
| `LabExamination : *` | `LAB-DEC-024` | AC-35, AC-37 |
| `LabValueBound : *` | `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021` | AC-24, AC-28 |
| `LabCriticalBound : Approve` | `LAB-DEC-023` | AC-33 |
| `LabRejectionReason : SystemFlag` | `LAB-DEC-019` | AC-26 |
| `LabWorklist : Read` | `LAB-DEC-013` | AC-10, AC-17 |

---

## 7. Amandemen 2026-09-14 — Penerimaan Sampling/Specimen

Menurunkan `LAB-DEC-040`, `LAB-DEC-041`, dan `LAB-DEC-042` dari decision log revision 26.

### 7.1 Kewenangan baru

| Resource | Action | String `[AccessPermission]` | Diberikan kepada | Kegunaan |
|---|---|---|---|---|
| `LabSpecimenType` | `Read` | `[AccessPermission("LabSpecimenType", "Read")]` | Petugas lab, kepala instalasi | Melihat daftar pilihan jenis specimen dan daftar pantau `Lainnya` |
| `LabSpecimenType` | `Create` | `[AccessPermission("LabSpecimenType", "Create")]` | Kepala instalasi | Menambah jenis specimen |
| `LabSpecimenType` | `Update` | `[AccessPermission("LabSpecimenType", "Update")]` | Kepala instalasi | Mengubah nama, keterangan, urutan, dan status aktif |

**Tidak ada `Delete`.** Jenis yang pernah dipakai dinonaktifkan, bukan dihapus — sama seperti
`MstLabRejectionReason`.

**Mengikuti `RJ-BIL-GATE-DEC-003`.** Hak membaca daftar jenis specimen **tidak** memberi hak
mengelolanya. Petugas penerimaan memerlukan `Read` agar dapat memilih jenis saat mencatat wadah;
ia tidak memerlukan `Create` maupun `Update`, dan tidak mendapatkannya dari jabatannya.

### 7.2 Kewenangan yang **tidak** bertambah

| Kemampuan baru | Hak akses yang dipakai | Kenapa tidak ada yang baru |
|---|---|---|
| Mencatat jenis, volume, dan waktu penerimaan fisik | `LabSpecimen : Plan` yang sudah ada | Ketiganya adalah ruas tambahan pada pencatatan wadah, bukan kemampuan tersendiri |
| Mengisi Qty pada daftar pemeriksaan | `LabExamination : Create` yang sudah ada | Qty hanya memperbanyak baris yang sudah boleh dibuat pemegang hak itu |
| Menandai pemeriksaan sudah diproses | `LabSpecimen : Accept` yang sudah ada | `AC-56` — tidak ada aksi tersendiri; penguncian melekat pada penetapan kelayakan |

Menambah hak akses untuk sesuatu yang sudah dijaga hak akses lain hanya melahirkan dua pintu
untuk satu ruangan, dan salah satunya pasti lupa dikunci.

### 7.3 Kejadian yang wajib menghasilkan jejak audit

| Kejadian | Yang wajib tercatat |
|---|---|
| Jenis specimen ditambahkan, diubah, atau dinonaktifkan | Pelaku, waktu, nilai sebelum dan sesudah |
| Wadah dicatat dengan jenis `Lainnya` | Pelaku, waktu, dan **keterangan yang diketik** — inilah bahan daftar pantau `AC-60` |
| Waktu penerimaan fisik diisi atau diubah | Pelaku, waktu, nilai sebelum dan sesudah, **beserta `ReceivedAt` sistem** agar selisihnya dapat ditelusuri (`AC-67`) |

**Kenapa selisih kedua waktu itu perlu tercatat, bukan dihitung ulang nanti.** `BR-37` memakai
selisih itu untuk membedakan keterlambatan yang wajar — sampel datang pukul 21.00 dan dicatat
pukul 08.00 keesokan hari — dari keterlambatan yang perlu ditanyakan. Bila hanya nilai akhirnya
yang tersimpan, pertanyaan "kapan ini sebenarnya diisi" tidak lagi dapat dijawab.

### 7.4 Privasi

Tidak satu pun kolom baru amandemen ini bersifat sensitif. Jenis specimen, volume, satuan, dan
waktu penerimaan tidak memuat identitas pasien.

Satu batas tetap berlaku: `SpecimenTypeOtherNote` adalah kolom teks yang diisi bebas petugas.
Layar pengisiannya **tidak boleh** mengajak petugas menuliskan identitas pasien di sana —
kolom itu untuk jenis bahan, bukan untuk catatan tentang orangnya.

### 7.5 Traceability

| Kewenangan | Decision ID | Acceptance criteria |
|---|---|---|
| `LabSpecimenType : Read` | `LAB-DEC-040` | AC-58, AC-60 |
| `LabSpecimenType : Create`, `: Update` | `LAB-DEC-040` | AC-60 |
| Jejak audit waktu penerimaan | `LAB-DEC-042` | AC-65, AC-67 |

---

## 8. Amandemen revision 6 — Hasil Mikrobiologi dan Patologi Anatomi, 2026-09-18

> ### ✅ STATUS: `approved` — 2026-09-18
>
> Disetujui **Yoga Aji Pratama** selaku pemilik modul pada 2026-09-18, bersama `LAB-API-v1` `r24`
> dan `LAB-VAL-v1` `r7`. `approved_by` / `approved_at`:
> Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-18.
>
> Menurunkan `LAB-DEC-027`, `LAB-DEC-080`, `LAB-DEC-084`, dan `LAB-DA-001` rev 6.
> **Dua resource baru; nol resource yang sudah ada berubah.**

### 8.1 Kewenangan yang **tidak** bertambah — dan ini butir terpentingnya

| Kemampuan baru | Hak akses yang dipakai | Kenapa tidak ada yang baru |
|---|---|---|
| Mengisi hasil Mikrobiologi beserta isolat dan kepekaannya | `LabExamination : Update` yang sudah ada | Isolat dan kepekaan adalah **isi sebuah hasil**, bukan kemampuan tersendiri. Mengikuti alasan `r21` |
| Mengisi laporan Patologi Anatomi | `LabExamination : Update` yang sudah ada | Sama |
| Membaca hasil kedua disiplin | `LabExamination : Read` yang sudah ada | Sama |

> **Memecah izin pengisian hasil per disiplin ditolak, dan alasannya bukan kemalasan.**
> `LAB-DEC-079` memberikan wewenang **klinis** per disiplin, dan `LAB-OPEN-034` menanyakan
> apakah wewenang **validasi** melintasi disiplin — keduanya **belum dijawab**. Membuat
> `LabExamination : UpdateMicrobiology` sekarang berarti menetapkan pembagian wewenang yang
> justru sedang ditanyakan, dan menetapkannya lewat pintu belakang bernama permission.

### 8.2 Kewenangan baru — hanya untuk kedua data induk

| Resource | Action | String `[AccessPermission]` | Diberikan kepada | Kegunaan |
|---|---|---|---|---|
| `LabOrganism` | `Read` | `[AccessPermission("LabOrganism", "Read")]` | Petugas lab, kepala instalasi | Memilih kuman saat mencatat isolat |
| `LabOrganism` | `Create` | `[AccessPermission("LabOrganism", "Create")]` | Kepala instalasi | Menambah organisme |
| `LabOrganism` | `Update` | `[AccessPermission("LabOrganism", "Update")]` | Kepala instalasi | Mengubah nama, urutan, dan status aktif |
| `LabAntibiotic` | `Read` | `[AccessPermission("LabAntibiotic", "Read")]` | Petugas lab, kepala instalasi | Memilih antibiotik saat mencatat kepekaan |
| `LabAntibiotic` | `Create` | `[AccessPermission("LabAntibiotic", "Create")]` | Kepala instalasi | Menambah antibiotik ke panel uji |
| `LabAntibiotic` | `Update` | `[AccessPermission("LabAntibiotic", "Update")]` | Kepala instalasi | Mengubah nama, urutan, dan status aktif |

**Nol `Delete` pada keduanya**, mengikuti pola `LabSpecimenType` dan `MstLabRejectionReason`.
Menghapus organisme yang sudah dipakai berarti menghapus temuan pasien.

**Hak membaca daftar tidak memberi hak mengelolanya.** Analis memerlukan `Read` agar dapat
memilih; ia tidak memerlukan `Create` maupun `Update`, dan tidak mendapatkannya dari jabatannya.

### 8.3 Endpoint dan kewenangannya

| Endpoint | Resource | Action | Dicatat logger |
|---|---|---|:---:|
| `PUT /lab-examinations/{id}/result/microbiology` | `LabExamination` | `Update` | **Ya** |
| `GET /lab-examinations/{id}/result/microbiology` | `LabExamination` | `Read` | Tidak |
| `PUT /lab-examinations/{id}/result/pathology` | `LabExamination` | `Update` | **Ya** |
| `GET /lab-examinations/{id}/result/pathology` | `LabExamination` | `Read` | Tidak |
| `GET /lab-organisms`, `GET /lab-organisms/options` | `LabOrganism` | `Read` | Tidak |
| `POST /lab-organisms` | `LabOrganism` | `Create` | **Ya** |
| `PUT /lab-organisms/{id}` | `LabOrganism` | `Update` | **Ya** |
| `GET /lab-antibiotics`, `GET /lab-antibiotics/options` | `LabAntibiotic` | `Read` | Tidak |
| `POST /lab-antibiotics` | `LabAntibiotic` | `Create` | **Ya** |
| `PUT /lab-antibiotics/{id}` | `LabAntibiotic` | `Update` | **Ya** |

> ### ⚠ Peringatan logger yang khusus berlaku di sini
>
> Payload log kedua `PUT` hasil **hanya** boleh memuat `EntityId`, controller, action, dan
> status. Ia **tidak boleh** memuat nama organisme, pola resistensi, maupun ketiga ruas narasi
> Patologi Anatomi — ketiganya **diagnosis pasien**, dan lebih berat daripada kolom sensitif
> mana pun yang sudah ada pada modul ini.

### 8.4 Kejadian yang wajib menghasilkan jejak audit

| Kejadian | `Scope` | `Action` | Alasan wajib |
|---|---|---|:---:|
| Hasil Mikrobiologi diisi atau diubah | `LabExamination` | `Examination.EnterMicrobiologyResult` | Tidak |
| Hasil Patologi Anatomi diisi atau diubah | `LabExamination` | `Examination.EnterPathologyResult` | Tidak |
| Baris isolat **dihapus** | `LabExamination` | `Examination.RemoveIsolate` | **Ya** |
| Baris kepekaan antibiotik **dihapus** | `LabExamination` | `Examination.RemoveSusceptibility` | **Ya** |
| Organisme atau antibiotik ditambahkan, diubah, atau dinonaktifkan | — | Logger + jejak data induk | Tidak |

> **Kenapa penghapusan baris menuntut alasan, sedangkan pengisian tidak.** Pada Patologi Klinik,
> hasil berubah dengan **ditimpa** — nilai lamanya tetap terlihat pada jejaknya. Pada
> mikrobiologi, hasil berubah dengan **baris hilang**, dan baris yang hilang tanpa alasan adalah
> **temuan yang lenyap tanpa ada yang tahu ia pernah ada**. Kuman yang sempat tercatat lalu
> dihapus adalah pertanyaan audit, bukan koreksi ketikan.

### 8.5 Privasi — kolom sensitif yang bertambah

| Kolom sensitif | Tabel | Aturan |
|---|---|---|
| `Note` | `LabMicrobiologyIsolate` | Tidak masuk logger; tinjau penyamaran pada response bagi pengguna non-klinis |
| `PathologyMacroscopic` | `LabExamination` | Sama. **Diagnosis pasien** |
| `PathologyMicroscopic` | `LabExamination` | Sama |
| `PathologyConclusion` | `LabExamination` | Sama |

**Satu batas baru yang tidak punya padanan pada amandemen sebelumnya.** Ketiga ruas narasi
Patologi Anatomi adalah **isi rekam medis**, bukan catatan operasional. Response yang membawanya
**tidak boleh** dipakai pada layar yang dibuka pengguna non-klinis — misalnya layar kasir atau
daftar pantau umum — dan pembatasannya wajib ditegakkan pada **bentuk DTO**, bukan hanya pada
hak akses endpoint.

### 8.6 Pembatasan di luar sistem kewenangan

| Aturan | Kenapa tidak cukup lewat kewenangan | Ditegakkan di |
|---|---|---|
| Jalur `/microbiology` hanya untuk pemeriksaan berbentuk mikrobiologi | Kewenangan menjawab "boleh mengisi hasil?", bukan "apakah bentuk hasil pemeriksaan ini cocok?" | `LabExaminationService` (`VAL-84`) |
| Organisme nonaktif ditolak pada baris baru, tetapi baris lama tetap terbaca | Kewenangan tidak mengenal umur sebuah baris | `LabExaminationService` (`VAL-85`, `INV-31`) |

### 8.7 Traceability usulan

| Kewenangan | Decision ID | Invariant |
|---|---|---|
| `LabExamination : Update` dipakai ulang | `LAB-DEC-027`, alasan `r21` | `INV-24` |
| `LabOrganism : *`, `LabAntibiotic : *` | `LAB-DEC-084` | `INV-30`, `INV-31` |
| Jejak audit penghapusan baris | `LAB-DA-001` rev 6 bagian A3.11 | — |
| Nol pemecahan izin per disiplin | `LAB-OPEN-034` **belum dijawab** | — |

---

## 9. Amandemen revision 7 — Laporan Patologi Anatomi per pesanan, 2026-09-18

> ### ✅ STATUS: `approved` — **DISETUJUI 2026-09-18**
>
> Disetujui bersama `LAB-API-v1` `r25` dan `LAB-VAL-v1` `r8`. `approved_by` / `approved_at`:
> Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18**.
> Menurunkan `LAB-DEC-085` sampai `LAB-DEC-094` dan `LAB-DA-001` rev 7.

### 9.1 Dua hak akses yang dipakai ulang, dan satu pemisahan yang disengaja

| Kemampuan | Hak akses | Alasan |
|---|---|---|
| Mengisi, memfinalkan, membuka kembali laporan PA | `LabExamination : Update` **yang sudah ada** | Mengikuti alasan `r21` dan `r24`: memecah izin per disiplin berarti menetapkan pembagian wewenang yang justru ditanyakan `LAB-OPEN-034` |
| Membaca laporan PA | `LabExamination : Read` **yang sudah ada** | Sama |
| **Menulis konteks klinis pesanan** | **`LabOrder : Update`** | **Penulisnya dokter pemesan, bukan patolog** (`LAB-DEC-091`, `INV-40`). Memakai `LabExamination : Update` akan memberi patolog hak menulis konteks yang bukan pengamatannya |

> **Pemisahan pada baris ketiga adalah satu-satunya hak akses baru yang benar-benar memisahkan
> dua orang pada halaman ini.** Dokter pemesan menulis konteks; patolog menulis laporan. Bila
> keduanya berbagi satu izin, **patolog dapat menulis riwayat penyakit pasien yang tidak pernah
> ia tanyakan** — dan itu masuk rekam medis.

### 9.2 Kewenangan baru — hanya untuk data induk

| Resource | Action | Diberikan kepada | Kegunaan |
|---|---|---|---|
| `LabPathologyParameter` | `Read` | Dokter Lab, Petugas Lab | Menampilkan formulir sesuai kategori |
| `LabPathologyParameter` | `Create`, `Update` | Kepala instalasi | Mengelola daftar parameter |
| `LabPathologyCategory` | `Read` | Dokter Lab, Petugas Lab | Menampilkan kategori dan keberlakuannya |
| `LabPathologyCategory` | `Create`, `Update` | Kepala instalasi | Mengelola kategori, **keberlakuan parameter**, dan **pemetaan jenis pemeriksaan** |

**Nol `Delete` pada keduanya.** Menghapus parameter yang sudah dipakai berarti menghapus isi
laporan diagnostik pasien.

**Keberlakuan dan pemetaan ikut `LabPathologyCategory : Update`, bukan resource sendiri.**
Keduanya adalah cara kategori dipakai, bukan benda yang berdiri sendiri — dan menambah dua
resource lagi hanya melahirkan pintu yang salah satunya pasti lupa dikunci.

### 9.3 Endpoint dan kewenangannya

| Endpoint | Resource | Action | Dicatat logger |
|---|---|---|:---:|
| `GET /lab-orders/{id}/pathology-report` | `LabExamination` | `Read` | Tidak |
| `PUT /lab-orders/{id}/pathology-report` | `LabExamination` | `Update` | **Ya** |
| `POST /lab-orders/{id}/pathology-report/finalize` | `LabExamination` | `Update` | **Ya** |
| `POST /lab-orders/{id}/pathology-report/reopen` | `LabExamination` | `Update` | **Ya** |
| `GET /lab-orders/{id}/pathology-context` | `LabOrder` | `Read` | Tidak |
| `PUT /lab-orders/{id}/pathology-context` | `LabOrder` | `Update` | **Ya** |
| Data induk — `GET` dan `GET /options` | masing-masing | `Read` | Tidak |
| Data induk — `POST` dan `PUT` | masing-masing | `Create` / `Update` | **Ya** |

> ### ⚠ Peringatan logger yang paling berat pada modul ini
>
> Payload log keempat jalur laporan **hanya** boleh memuat `EntityId`, controller, action, dan
> status. Ia **tidak boleh** memuat isi parameter mana pun — makroskopik, mikroskopik, kesimpulan,
> diagnosa PA, maupun status temuan. **Seluruhnya diagnosis pasien.**
>
> Hal yang sama berlaku pada konteks klinis: riwayat penyakit dan masa terakhir haid **tidak boleh**
> masuk log.

### 9.4 Kejadian yang wajib menghasilkan jejak audit

| Kejadian | `Action` | Alasan wajib |
|---|---|:---:|
| Laporan PA diisi atau diubah | `PathologyReport.Write` | Tidak |
| **Laporan difinalkan** | `PathologyReport.Finalize` | Tidak |
| **Laporan dibuka kembali** | `PathologyReport.Reopen` | **Ya** |
| Nilai parameter berubah **sesudah** laporan pernah difinalkan | `PathologyReport.AmendValue` | **Ya** |
| Konteks klinis ditulis atau diubah | `PathologyContext.Write` | Tidak |
| Parameter, kategori, keberlakuan, atau pemetaan diubah | Logger + jejak data induk | Tidak |

> **Kenapa `Reopen` dan perubahan sesudahnya menuntut alasan, sedangkan pengisian pertama tidak.**
> Sebelum difinalkan, laporan masih ditulis — perubahan adalah bagian dari menulis. **Sesudah
> difinalkan, patolog sudah menyatakan diagnosisnya selesai.** Mengubahnya kembali adalah
> pernyataan bahwa pernyataan sebelumnya perlu diperbaiki, dan itu perlu dapat dijelaskan.
>
> **`ReopenCount` saja tidak cukup** — yang dibutuhkan riwayat per kejadian beserta alasannya.

### 9.5 Privasi

| Kolom sensitif | Tabel | Aturan |
|---|---|---|
| `Value` | `LabPathologyReportValue` | **Diagnosis pasien.** Tidak masuk logger; tidak muncul pada layar non-klinis |
| `InitialDiagnosis`, `RelevantHistory`, `LastMenstrualPeriod`, `ClinicalNote` | `LabPathologyOrderContext` | **Isi rekam medis.** Aturan yang sama |
| `FindingStatus` | `LabPathologyReport` | Bukan teks bebas, tetapi **menyatakan tingkat bahaya pasien**. Tidak masuk logger |

**Satu batas yang lebih ketat daripada amandemen mana pun sebelumnya.** Response yang membawa
nilai parameter **tidak boleh** dipakai pada layar yang dibuka pengguna non-klinis — kasir, papan
pemantauan, daftar pantau umum. Pembatasannya wajib ditegakkan pada **bentuk DTO**, bukan hanya
pada hak akses endpoint. `LAB-DEC-068` memberi Petugas Lab hak mencetak; itu **tidak** berarti
memberi layar lain hak menampilkan isinya.

### 9.6 Traceability

| Kewenangan | Decision ID | Invariant |
|---|---|---|
| `LabExamination : Update` dipakai ulang | `LAB-DEC-090`, alasan `r21` | — |
| `LabOrder : Update` untuk konteks klinis | `LAB-DEC-091` | `INV-40` |
| `LabPathologyParameter : *`, `LabPathologyCategory : *` | `LAB-DEC-086`, `LAB-DEC-087` | `INV-37`, `INV-39` |
| Jejak `Reopen` beralasan | `LAB-DEC-088`; `LAB-DA-001` rev 7 A4.9 | `INV-35` |
