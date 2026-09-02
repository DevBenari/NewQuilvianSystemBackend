# State Transition Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-STATE-v1` |
| Revision | `2` |
| Status | `approved` — dikunci 2026-09-02 |
| Batas penguncian | Terkunci **kecuali** penamaan `MstLabValueBound` dan `MstLabValueOption`, yang menunggu `LAB-OPEN-021` |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:75d285252aa5bce7fcaf5d90242da0d30fbd58a92a16aca3377683243be45f61` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Backend SHA | `c87d9c0` |

Transisi yang **tidak sah** ikut dituliskan, bukan hanya yang sah. Tanpa itu, implementer tidak
tahu apa yang harus ditolak.

---

## 1. Pesanan Laboratorium — `LabOrderStatus`

### Transisi yang sah

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Membuat pesanan | `Requested` | Dokter pemesan | Kunjungan pasien ada dan belum ditutup | `404` bila kunjungan tidak ada |
| `Requested` | Wadah pertama dinyatakan layak | `Accepted` | Turunan otomatis sistem | Ada wadah berstatus layak | — |
| `Accepted` | Mulai dikerjakan | `InProcess` | Petugas berwenang memproses | — | `409` bila status bukan `Accepted` |
| `InProcess` | Menyelesaikan | `Completed` | Petugas berwenang memproses | — | `409` bila status bukan `InProcess` |
| Selain `OnHold`, `Cancelled`, `Completed` | Menahan | `OnHold` | Petugas berwenang menahan | Status sebelumnya disimpan | `409` |
| `OnHold` | Melanjutkan | Status sebelum ditahan | Petugas berwenang menahan | Status sebelumnya diketahui | `409` bila status bukan `OnHold` |
| Selain `Cancelled`, `Completed` | Membatalkan | `Cancelled` | Petugas berwenang membatalkan | — | `409` |

### Transisi yang **tidak sah** dan wajib ditolak

| Dari status | Tindakan | Alasan penolakan | Kode |
|---|---|---|---|
| `Completed` | Membatalkan | Pesanan yang sudah selesai tidak dapat dibatalkan | `409` |
| `Cancelled` | Menambah wadah atau pemeriksaan | Pesanan yang sudah dibatalkan tidak menerima apa pun lagi | `409` |
| `Cancelled` | Melanjutkan, menahan, memproses | Status terminal | `409` |
| `Requested` | Menyelesaikan langsung | Wajib melewati `Accepted` dan `InProcess` | `409` |
| `OnHold` | Memproses atau menyelesaikan | Wajib dilanjutkan lebih dulu | `409` |

### Penandaan cito dan duplo — **pada pemeriksaan, bukan pesanan**

`LAB-DEC-026` memindahkan penanda ini dari pesanan ke pemeriksaan terpesan. Satu pesanan boleh
memuat pemeriksaan cito dan pemeriksaan biasa sekaligus.

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Routine` | Menandai satu pemeriksaan cito | `Cito` | **Dokter pemesan pesanan itu** | Pemeriksaan belum `Voided` maupun `Cancelled` | `403` bila bukan dokter pemesan |
| `Cito` | Mengembalikan pemeriksaan ke biasa | `Routine` | Dokter pemesan | Sama seperti di atas | `403` |
| `IsDuplo = false` | Menandai dikerjakan ganda | `IsDuplo = true` | Petugas berwenang menetapkan kelayakan atau analis | Wadah penopang belum ditolak | `409` |

Setiap penandaan menyimpan `UrgencyMarkedAt` dan `UrgencyMarkedByUserId` pada baris
pemeriksaan, serta menghasilkan satu baris riwayat berlingkup `LabExamination`.

**Yang belum diputuskan:** apakah penanda cito dan duplo berdampak pada tarif. Dicatat sebagai
`LAB-OPEN-013`. Selama belum diputuskan, keduanya **tidak** mengubah salinan tarif pada baris
pemeriksaan.

---

## 2. Wadah Fisik — `LabSpecimenStatus`

### Transisi yang sah

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Merencanakan wadah | `Planned` | Petugas berwenang merencanakan | Pesanan belum dibatalkan; sekurang-kurangnya satu pemeriksaan disertakan | `409` |
| `Planned` | Mencatat pengambilan | `Collected` | Petugas berwenang mengambil | — | `409` |
| `Collected` | Mencatat tiba di lab | `Received` | Petugas berwenang menerima | — | `409` |
| `Received` | Menyatakan layak | `Accepted` | Petugas berwenang menetapkan kelayakan | Wajib sudah `Received` | `409` bila belum |
| `Received` | Menolak | `Rejected` | Petugas berwenang menetapkan kelayakan | Alasan terkendali wajib; catatan wajib bila alasan menuntutnya | `422` |
| `Rejected` | Meminta ambil ulang | `RecollectionRequired` | Petugas berwenang menetapkan kelayakan | Sebab ambil ulang wajib; alasan wajib untuk sebab selain kesalahan internal | `422` |
| `RecollectionRequired` | Membuat wadah pengganti | `Planned` pada wadah baru | Petugas berwenang merencanakan | Wadah baru menunjuk wadah lama | — |
| Selain terminal dan `OnHold` | Menahan | `OnHold` | Petugas berwenang menahan | Status sebelumnya disimpan | `409` |
| `OnHold` | Melanjutkan | Status sebelum ditahan | Petugas berwenang menahan | — | `409` |
| Selain terminal | Membatalkan | `Cancelled` | Petugas berwenang membatalkan | — | `409` |

Status terminal: `Accepted`, `Rejected`, `Cancelled`.

### Transisi yang **tidak sah** dan wajib ditolak

| Dari status | Tindakan | Alasan penolakan | Kode |
|---|---|---|---|
| `Planned` | Menyatakan layak | Wajib melewati pengambilan dan penerimaan | `409` |
| `Collected` | Menyatakan layak | Wajib melewati penerimaan lebih dulu | `409` |
| `Accepted` | Menolak | Wadah yang sudah dinyatakan layak tidak dapat ditolak | `409` |
| `Rejected` | Menyatakan layak | Wadah yang sudah ditolak tidak dapat dibalik | `409` |
| `Accepted` | Menyatakan layak lagi | Diperlakukan **idempoten**: dikembalikan hasil yang sama, tidak menggandakan kelayakan tagih | `200` |
| Mana pun | Menolak **sebagian** pemeriksaan pada satu wadah | Keputusan kelayakan melekat pada wadah, bukan pemeriksaan | `422` |
| `Cancelled` | Tindakan apa pun | Status terminal | `409` |

---

## 3. Pemeriksaan Terpesan — `LabExaminationStatus`

Status pemeriksaan sebagian besar **mengikuti** wadah penopangnya. Ia tidak dipindahkan
langsung oleh petugas kecuali saat dibatalkan.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Menambah pemeriksaan | `Ordered` | Petugas berwenang merencanakan | Jenis pemeriksaan wajib berpenanda `IsLaboratory`; pesanan belum dibatalkan | `422` |
| `Ordered` | Wadah penopang dinyatakan layak | `ChargeEligible` | Turunan otomatis sistem | Wadah berstatus layak | — |
| `Ordered` | Wadah penopang ditolak | `Voided` | Turunan otomatis sistem | Wadah berstatus ditolak | — |
| `Ordered` | Membatalkan pemeriksaan | `Cancelled` | Petugas berwenang membatalkan | Wadah penopang belum dinyatakan layak | `409` bila sudah layak |
| `ChargeEligible` | Membatalkan pemeriksaan | `Cancelled` | Petugas berwenang membatalkan | Menerbitkan fakta pembatalan klinis ke Billing | — |

### Transisi yang **tidak sah** dan wajib ditolak

| Dari status | Tindakan | Alasan penolakan | Kode |
|---|---|---|---|
| `Voided` | Tindakan apa pun | Pemeriksaan gugur bersama wadahnya | `409` |
| `Cancelled` | Tindakan apa pun | Status terminal | `409` |
| Mana pun | Memindahkan pemeriksaan ke wadah lain | Tidak ada jalur pemindahan pada rilis ini | `422` |
| Mana pun | Menyatakan layak langsung pada pemeriksaan | Kelayakan melekat pada wadah | `422` |

> **Yang sengaja tidak ada.** Status hasil — `Pending`, `InProcess`, `Completed`, `Validated`,
> `Released` — **tidak** ditambahkan pada rilis ini. Slice hasil masih terblokir `LAB-SIGN-001`.

---

## 4. Pengajuan Perubahan Batas Kritis — `LabBoundChangeStatus`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Mengajukan perubahan | `Submitted` | Pengelola batas nilai | Alasan pengajuan wajib | `422` |
| `Submitted` | Menyetujui | `Approved` | Pemegang kewenangan persetujuan batas kritis | Bukan pengaju yang sama | `403` |
| `Submitted` | Menolak | `Rejected` | Pemegang kewenangan persetujuan batas kritis | Bukan pengaju yang sama | `403` |
| `Submitted` | Menarik | `Withdrawn` | Pengaju itu sendiri | — | `403` bila bukan pengaju |

Status terminal: `Approved`, `Rejected`, `Withdrawn`.

**Yang terjadi saat disetujui:** batas kritis pada `MstLabValueBound` diperbarui, dan satu baris
`LabValueBoundHistory` dibuat dengan `ApprovedByUserId` terisi. Sebelum disetujui, batas yang
berlaku **tidak berubah sama sekali**.

### Transisi yang **tidak sah** dan wajib ditolak

| Dari status | Tindakan | Alasan penolakan | Kode |
|---|---|---|---|
| `Approved`, `Rejected`, `Withdrawn` | Memutuskan ulang | Status terminal | `409` |
| `Submitted` | Menyetujui oleh pengaju sendiri | Persetujuan batas kritis tidak boleh diberikan pengaju | `403` |
| Mana pun | Mengubah batas kritis langsung lewat `PUT /lab-value-bounds/{id}` | Batas kritis hanya berubah lewat pengajuan yang disetujui | `422` |

---

## 5. Contoh Jalur Lengkap

> Pesanan pasien Andi berisi Fungsi hati dan Fungsi ginjal, keduanya dari satu tabung serum.
>
> 1. dr. Rina membuat pesanan → pesanan `Requested`, kedua pemeriksaan `Ordered`.
> 2. dr. Rina menandai cito → `Urgency` menjadi `Cito`, tercatat pukul 08.00.
> 3. Perawat Dewi merencanakan satu wadah berisi dua pemeriksaan → wadah `Planned`.
> 4. Dewi mengambil darah → wadah `Collected`.
> 5. Wadah tiba di lab → wadah `Received`.
> 6. Budi menyatakan wadah layak → wadah `Accepted`, **kedua** pemeriksaan menjadi
>    `ChargeEligible`, dan **dua** fakta kelayakan tagih diterbitkan.
> 7. Pesanan otomatis menjadi `Accepted`.
>
> Jalur gagal: bila pada langkah 6 Budi menolak wadah karena serum keruh, wadah menjadi
> `Rejected` dan **kedua** pemeriksaan menjadi `Voided` serentak. Tidak ada fakta kelayakan
> tagih yang terbit.
