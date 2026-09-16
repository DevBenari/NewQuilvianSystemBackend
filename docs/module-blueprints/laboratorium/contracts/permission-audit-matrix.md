# Permission dan Audit Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-PERM-v1` |
| Revision | `5` |
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
