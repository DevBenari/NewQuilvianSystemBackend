# State Transition Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-STATE-v1` |
| Revision | **`7` — `approved`** 2026-09-25, bagian 9 (penjaga penyelesaian order) — disetujui Yoga Aji Pratama. Sebelumnya: **`6` — `approved`** 2026-09-25, bagian 8 (`S4d-1` Mikrobiologi) — disetujui Yoga Aji Pratama. Sebelumnya: **`5` — `approved`** 2026-09-25, bagian 7 (`S4`: Tervalidasi dan Dirilis sebagai keadaan turunan). Terakhir `approved`: `4` — **`approved`** 2026-09-24, bagian 6 |
| Status | `approved` — `r2` dikunci 2026-09-02; **amandemen `r3` disetujui pemilik modul 2026-09-15** lewat `LAB-DEC-061` dan `LAB-DEC-063` |
| Isi amandemen `r3` | **Status `Confirmed` masuk sebagai status pesanan antara `Requested` dan `Accepted`**, beserta konfirmator, waktu konfirmasi, dan dokter pemeriksa. Konfirmasi hanya sah sekali. **Pembatalan dipersempit** menjadi hanya sah pada `Requested` dan `Confirmed`, dan wajib beralasan. Jalur `Requested` → `Accepted` **tidak dicabut** — lihat bagian 1a |
| Batas penguncian | **Terkunci penuh sejak 2026-09-02.** `LAB-OPEN-021` dijawab: penamaan memakai prefix `Lab`, sehingga tidak ada lagi bagian yang dikecualikan |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
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


## 1a. Amandemen `r3` — Status `Confirmed`, 2026-09-15

**Disetujui pemilik modul 2026-09-15** lewat `LAB-DEC-061`, menutup bagian `LAB-P0-002` yang
paling sering ditanyakan: di mana `Confirmed` berdiri.

### Transisi yang sah — tambahan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `Requested` | Mengonfirmasi pesanan | `Confirmed` | Petugas laboratorium berwenang mengonfirmasi | Dokter pemeriksa sudah dipilih | `409` bila status bukan `Requested` |
| `Confirmed` | Wadah pertama dinyatakan layak | `Accepted` | Turunan otomatis sistem | Ada wadah berstatus layak | — |

### Transisi yang **tidak sah** dan wajib ditolak — tambahan

| Dari status | Tindakan | Alasan penolakan | Kode |
|---|---|---|---|
| `Confirmed` | Mengonfirmasi lagi | Konfirmasi hanya sah sekali | `409` |
| `Accepted`, `InProcess`, `Completed`, `Cancelled` | Mengonfirmasi | Pesanan sudah melewati tahap konfirmasi | `409` |
| `InProcess`, `Completed`, `Cancelled` | Membatalkan | Pembatalan hanya sah pada `Requested` dan `Confirmed` (`LAB-DEC-063`) | `409` |

### `Requested` → `Accepted` **tetap sah**, dan itu keputusan sadar

Jalur lama tidak dicabut. Pesanan yang tidak pernah dikonfirmasi tetap dapat mencapai `Accepted`
ketika wadah pertamanya dinyatakan layak.

**Alasannya bukan kemalasan.** Menjadikan `Confirmed` wajib berarti mengetatkan jalur yang sedang
dipakai: seluruh pesanan yang hari ini berstatus `Requested` — dan seluruh wadah yang sedang
berjalan di atasnya — akan berhenti dapat diproses sampai seseorang mengonfirmasinya satu per
satu. `BE-LAB-21` sudah menunjukkan berapa mahal harga pengetatan diam-diam pada endpoint yang
sedang dipakai.

**Apakah konfirmasi kelak menjadi wajib adalah keputusan tersendiri**, dan ia menuntut dua hal
yang belum ada: perlakuan atas pesanan yang sedang berjalan, dan kepastian bahwa setiap jalur
pembuat pesanan melewati layar yang punya tombol Konfirmasi. Dicatat sebagai `LAB-OPEN-027`.

### Pembatalan — batas barunya

| Dari status | Tindakan | Ke status | Syarat |
|---|---|---|---|
| `Requested`, `Confirmed` | Membatalkan | `Cancelled` | **Alasan pembatalan wajib terisi** (`LAB-DEC-063`) |

Baris "Selain `Cancelled`, `Completed` → Membatalkan" pada bagian 1 **dipersempit** oleh baris
ini. Pembatalan dari `Accepted`, `InProcess`, atau `OnHold` **tidak lagi sah**.

> **Ini satu-satunya pengetatan pada amandemen `r3`, dan ia disengaja.** Pesanan yang wadahnya
> sudah dinyatakan layak berarti bahan pasien sudah diambil dan pekerjaan sudah dimulai;
> membatalkannya bukan lagi pembatalan melainkan koreksi — dan aturan koreksi belum diputuskan
> (`LAB-P0-003` tetap terbuka untuk bagian itu). Dampaknya pada data berjalan wajib diperiksa
> sebelum ditegakkan.

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

**Yang terjadi saat disetujui:** batas kritis pada `LabValueBound` diperbarui, dan satu baris
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

---

## 6. Amandemen `r4` — Penulisan hasil Patologi Klinik dan Mikrobiologi, 2026-09-24

| Field | Nilai |
|---|---|
| Revision | `r4` |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-24 — instruksi *"kerjakan semua langkah"*, lihat `LAB-API-v1` `r33` bagian 28 |
| `input_revision` | decisions rev 71; `LAB-API-v1` `r33`; `LAB-VAL-v1` `r11` |
| Keputusan | `LAB-DEC-080`, `LAB-DEC-097`, `LAB-DEC-135`, `LAB-DEC-141`, `LAB-DEC-147` |

### 6.1 Ini bukan status baru

`LabExaminationStatus` **tidak disentuh** dan tetap `Ordered`/`ChargeEligible`/`Voided`/`Cancelled`
(`LAB-DEC-080`). Keadaan penulisan hasil **diturunkan dari fakta** yang tersimpan:

| Keadaan | Diturunkan dari |
|---|---|
| Belum diisi | `ResultEnteredAt` kosong |
| Draft | `ResultEnteredAt` terisi, `FinalizedAt` kosong |
| Final | `FinalizedAt` terisi |

Label order *Dalam Pemeriksaan* dan *Selesai* (`LAB-DEC-135`) bergantung pada rilis, yang
belum ada. Keduanya **tidak** dikontrakkan di sini dan lahir bersama `S4`.

### 6.2 Tindakan yang sah

Berlaku bagi Patologi Klinik dan Mikrobiologi. Pelakunya pemegang
`LabExaminationResult : Update` (`LAB-PERM-v1` revision 10).

| Dari | Tindakan | Ke | Syarat | Yang tercatat |
|---|---|---|---|---|
| Belum diisi | Simpan hasil | Draft | Pemeriksaan tidak `Voided`/`Cancelled`; bentuk hasil sah (`VAL-79`..`VAL-82`) | `ResultEnteredAt`, `ResultEnteredByUserId` |
| Draft | Simpan hasil | Draft | Sama | Keduanya diperbarui |
| Draft | Final | Final | Hasil sudah diisi | `FinalizedAt`, `FinalizedByUserId` |
| Draft | Catat konsultasi | Draft | Waktu tidak di masa depan (`VAL-108`) | `ConsultedByUserId`, `ConsultedToName`, `ConsultedAt` |
| Final | Reopen | Draft | Alasan wajib; **sebelum validasi** — penjaganya lahir bersama `S4` | `FinalizedAt` dikosongkan, `ReopenCount` + 1, satu baris `LabTransitionHistory` |

### 6.3 Tindakan yang tidak sah

| Dari | Tindakan | Kenapa ditolak | Kode |
|---|---|---|---|
| Final | Simpan hasil | Final harus stabil (`VAL-120`) | `409` |
| Final | Catat konsultasi | Sama (`VAL-121`) | `409` |
| Final | Final lagi | Sudah Final — aturan yang sudah berjalan | `409` |
| Belum diisi | Final | Tidak ada yang dinyatakan selesai | `422` |
| Belum diisi atau Draft | Reopen | Belum pernah Final (`VAL-107`) | `422` |
| Final | Reopen tanpa alasan | Jejak pembukaan wajib beralasan | `422` |
| Mana pun, pada pemeriksaan Patologi Anatomi | Final, Reopen, konsultasi | Hasilnya tinggal di laporan per pesanan (`VAL-122`) | `422` |
| Mana pun | Tindakan hasil oleh pengguna tanpa `LabExaminationResult : Update` | Bukan pemegang izin hasil (`LAB-DEC-146`) | `403` |

### 6.4 Contoh jalur lengkap

> Order Patologi Klinik pasien Andi berisi Kalium cito dan Hemoglobin.
>
> 1. Analis Sari menyimpan Kalium 6,4 → Kalium **Draft**.
> 2. Sari melihat sampel hemolisis, mengulang, menyimpan 4,6 → tetap **Draft**.
> 3. Sari menekan Final pada Kalium pukul 09.10 → Kalium **Final**. Hemoglobin masih **Belum diisi**.
> 4. Pukul 09.12 tab lama rekannya mengirim Kalium 6,1 → **ditolak `409`**, Kalium tetap 4,6.
> 5. Pukul 09.30 Sari menyadari satuan salah ketik, menekan Reopen beralasan → Kalium **Draft**,
>    `ReopenCount` = 1.
> 6. Sari membetulkan dan menekan Final lagi pukul 09.32 → Kalium **Final**, `FinalizedAt` 09.32.
>
> Jalur gagal: pada langkah 3, bila dr. Rina — dokter pemesan — memanggil
> `POST /{id}/result/finalize` dari akunnya, sistem menjawab **`403`** walaupun ia memegang
> `LabExamination : Update` untuk cito.

## 7. Amandemen `r5` — Validasi dan rilis hasil Patologi Klinik (`S4`), 2026-09-25

| Field | Nilai |
|---|---|
| Revision | `r5` |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-25 — instruksi *"Setujui kelima kontrak beserta 10 butir itu dan lanjut ke /quilvian-engineering-skills:plan-module-delivery"*, lihat `LAB-API-v1` `r34` bagian 29 |
| `input_revision` | decisions rev 74; `LAB-DA-001` rev 8 bagian A5; `LAB-API-v1` `r34`; `LAB-VAL-v1` `r12` |
| Keputusan | `LAB-DEC-003`, `LAB-DEC-008`, `LAB-DEC-080`, `LAB-DEC-120`, `LAB-DEC-135`, `LAB-DEC-138` |
| Berlaku bagi | **Patologi Klinik saja.** Mikrobiologi dan Patologi Anatomi tetap berhenti di Final sampai `S4d`/`S4e` |

### 7.1 Tetap bukan status baru

`LabExaminationStatus` **tidak disentuh** (`LAB-DEC-080`). Bagian 6.1 bertambah dua keadaan
**turunan**:

| Keadaan | Diturunkan dari | `resultStatus` |
|---|---|---|
| Belum diisi | `ResultEnteredAt` kosong | `NotEntered` |
| Draft | `ResultEnteredAt` terisi, `FinalizedAt` kosong | `Draft` |
| Final | `FinalizedAt` terisi, `ValidatedAt` kosong | `Final` |
| **Tervalidasi** | `ValidatedAt` terisi, `ReleasedAt` kosong | `Validated` |
| **Dirilis** | `ReleasedAt` terisi | `Released` |

**Label order** — dititipkan 6.1 kepada `S4` — kini dikontrakkan sebagai ruas turunan
`resultProgress`: *Dalam Pemeriksaan* selama masih ada pemeriksaan tidak batal yang belum dirilis;
*Selesai* bila seluruhnya dirilis. Pemeriksaan batal atau gugur **tidak menahan** label itu
(`AC-199`). Ia **bukan** `LabOrderStatus.Completed` — lihat `LAB-CONFLICT-014`.

### 7.2 Tindakan yang sah

| Dari | Tindakan | Ke | Siapa | Syarat | Yang tercatat |
|---|---|---|---|---|---|
| Final | Validasi | **Tervalidasi** | Pemegang `Validate` yang **ditunjuk** validasi Patologi Klinik | `VAL-124`, `VAL-128`; bukan pengisi — atau pengecualian beralasan (`VAL-129`, `VAL-132`); pengisi tercatat (`VAL-130`) | `ValidatedAt`, `ValidatedByUserId`, snapshot jabatan, dasar kewenangan, alasan pengecualian bila ada; satu baris riwayat `ValidateResult` |
| Tervalidasi | Rilis | **Dirilis** | Pemegang `Release` yang **ditunjuk** rilis Patologi Klinik | `VAL-133`, `VAL-128`; bukan pemvalidasi — atau pengecualian beralasan (`VAL-131`, `VAL-132`) | Kolom rilis; satu baris riwayat `ReleaseResult`; **satu** dokumen rekam medis tertanda tangan dan terkunci — gagal bersama (`VAL-137`) |
| Tervalidasi | *Kembalikan ke analis* | **Draft** | Pemegang `Return` yang ditunjuk validasi **atau** rilis | Alasan dari daftar koreksi (`VAL-135`) | Kolom validasi dan `FinalizedAt` dikosongkan; `ReopenCount` **tidak** naik; satu baris riwayat `ReturnResultToAnalyst` berkode alasan; baris `ValidateResult` sebelumnya **tetap** |
| Final | Reopen | Draft | Pemegang `LabExaminationResult : Update` | **Hanya bila belum divalidasi** (`VAL-136`) — penjaga yang dititipkan 6.2 | Tidak berubah dari 6.2, kini menaikkan `Version` |

**Setiap tindakan menaikkan `Version`.** Dua tindakan pada detik yang sama — misalnya Reopen dan
Validasi — tidak dapat sama-sama berhasil; yang kalah menerima `409`.

### 7.3 Tindakan yang tidak sah

| Dari | Tindakan | Kenapa ditolak | Kode |
|---|---|---|---|
| Belum diisi atau Draft | Validasi | Belum Final (`VAL-124`) | `422` |
| Tervalidasi atau Dirilis | Validasi | Sudah divalidasi (`VAL-125`) | `409` |
| Belum diisi, Draft, atau Final | Rilis | Belum divalidasi (`VAL-133`) | `422` |
| Dirilis | Rilis | Sudah dirilis (`VAL-133`) | `409` |
| Belum diisi, Draft, atau Final | *Kembalikan* | Belum divalidasi — analis cukup Reopen (`VAL-134`) | `422` |
| Dirilis | *Kembalikan* | Hasil yang sudah dirilis hanya lewat koreksi `S6` (`VAL-134`, `AC-207`) | `409` |
| Tervalidasi atau Dirilis | Reopen | `VAL-136` | `409` |
| Tervalidasi atau Dirilis | Simpan hasil, konsultasi | `FinalizedAt` tetap terisi (`VAL-120`, `VAL-121`) | `409` |
| Mana pun, pada pemeriksaan batal atau gugur | Validasi, rilis, *Kembalikan* | `VAL-127` | `422` |
| Mana pun, pada Mikrobiologi atau Patologi Anatomi | Validasi, rilis, *Kembalikan* | `VAL-126` | `422` |
| Dirilis | **Batal pemeriksaan** | `VAL-143` — diturunkan dari `LAB-DEC-138` dan `LAB-DEC-063`; arah yang berlaku sampai `DEC-LAB-019` dijawab | `422` |
| Mana pun | Tindakan oleh pengguna tanpa aksinya, atau tanpa penunjukan yang berlaku | Lapis jabatan / lapis orang (`VAL-128`, `VAL-138`) | `403` |

> **Baris "Batal pemeriksaan" pada hasil yang sudah dirilis adalah penjaga baru** pada
> `POST /{id}/cancel` yang sudah ada, dengan pesan *"Pemeriksaan yang hasilnya sudah dirilis tidak
> dapat dibatalkan. Hasilnya hanya dapat diperbaiki lewat koreksi."* Ia menegakkan
> `ARCH-GAP-LAB-09` sebagai `VAL-143`, dan **wajib disetujui
> bersama** kontrak ini, sebab `LAB-DEC-049` membiarkan pembatalan pemeriksaan terbuka tanpa batas
> atas.

### 7.4 Contoh jalur lengkap

> Order Patologi Klinik pasien IGD berisi Kalium cito dan Hemoglobin. Nama tenaga di bawah adalah
> samaran.
>
> 1. 01.55 — analis Sari mengisi Kalium 7,2 dan menekan Final → Kalium **Final**, masuk antrean
>    tahap *menunggu validasi* di urutan teratas karena cito.
> 2. 02.12 — dr. Contoh memvalidasi → Kalium **Tervalidasi**. Order tetap *Dalam Pemeriksaan*.
> 3. 02.13 — dr. Contoh menekan Rilis dan ditolak `VAL-131`; ia memilih alasan *Shift tunggal* →
>    Kalium **Dirilis** pukul 02.14, berpenanda *"Dirilis oleh pemvalidasi sendiri"*. Satu dokumen
>    `LaboratoryResult` terdaftar di rekam medis. Hemoglobin masih Draft; order tetap
>    *Dalam Pemeriksaan* (`AC-198`).
> 4. 02.20 — Sari menekan Reopen pada Kalium → **ditolak `409`** (`VAL-136`).
> 5. 06.30 — Hemoglobin Final, divalidasi dr. Contoh pukul 07.10, dan pukul 07.25 perilis pagi
>    menyadari tabungnya tertukar. Ia menekan *Kembalikan ke analis* dengan alasan
>    *Sampel tertukar* → Hemoglobin **Draft**; riwayat tetap menyebut *divalidasi dr. Contoh 07.10*.
> 6. 08.05 — Hemoglobin Final lagi, divalidasi pukul 08.20, dirilis perilis pagi pukul 08.30 —
>    tanpa pengecualian, dua orang berbeda. Order kini *Selesai*.
>
> **Jalur gagal:** pada langkah 2, bila penunjukan dr. Contoh sedang ditangguhkan, validasi
> ditolak `403` *"Penunjukan validasi Patologi Klinik Anda sedang ditangguhkan."* — dan Kalium
> 7,2 **tetap menunggu**, tidak ada jalur pintas. Jalan keluarnya milik `DEC-LAB-011` sisa.

## 8. Amandemen `r6` — Validasi dan rilis hasil Mikrobiologi (`S4d-1`), 2026-09-25

| Field | Nilai |
|---|---|
| Revision | `r6` |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-25 — instruksi *"Setujui keempat kontrak beserta lima butir di atas, termasuk perubahan bunyi VAL-126, lalu jalankan /plan-module-delivery untuk MVP-10a sampai MVP-10c"*, lihat `LAB-API-v1` `r35` bagian 30 |
| `input_revision` | decisions rev 76; `LAB-DA-001` rev 9 bagian A6; `LAB-API-v1` `r35`; `LAB-VAL-v1` `r13` |
| Berlaku bagi | Patologi Klinik **dan Mikrobiologi**. Patologi Anatomi tetap berhenti di Final sampai `S4e` |

### 8.1 Keadaan turunan

Bagian 7.1 berlaku apa adanya bagi Mikrobiologi. **Kualifikasi `Sementara` bukan keadaan** —
ia nilai isi hasil (`LAB-DEC-114`). Hasil `Sementara` yang Final tetap berkeadaan **Final**, hanya
tidak dapat melangkah lebih jauh.

### 8.2 Tindakan yang sah — tambahan syarat bagi Mikrobiologi

| Dari | Tindakan | Ke | Syarat tambahan |
|---|---|---|---|
| Final | Validasi | Tervalidasi | Kualifikasi **bukan** `Sementara` (`VAL-144`); pelaku ditunjuk validasi **Mikrobiologi** |
| Tervalidasi | Rilis | Dirilis | Kualifikasi bukan `Sementara`; pelaku ditunjuk rilis **Mikrobiologi** |
| Tervalidasi | *Kembalikan ke analis* | Draft | Pelaku ditunjuk validasi atau rilis **Mikrobiologi** |

### 8.3 Tindakan yang tidak sah — tambahan

| Dari | Tindakan | Kenapa ditolak | Kode |
|---|---|---|---|
| Final berkualifikasi `Sementara` | Validasi | `VAL-144` — `DEC-LAB-020` belum dijawab | `422` |
| Tervalidasi berkualifikasi `Sementara` | Rilis | Sama — keadaan ini **tidak dapat terjadi** lewat sistem, tetapi penjaganya tetap ada pada rilis | `422` |
| Tervalidasi atau Dirilis | Mengubah isolat, antibiogram, status temuan, atau kualifikasi | `VAL-120` — `INV-53` | `409` |
| Mana pun, pada Patologi Anatomi | Validasi, rilis, *Kembalikan* | `VAL-126` bunyi baru | `422` |

### 8.4 Contoh jalur lengkap

> Order Mikrobiologi berisi kultur urin dan kultur darah. Nama tenaga di bawah samaran.
>
> 1. Senin — kultur urin Final berkualifikasi `Sementara`. Kultur urin **tidak** muncul di antrean
>    validasi.
> 2. Rabu — analis membuka kembali kultur urin, mengisi *Escherichia coli* dan antibiogramnya,
>    mengubah kualifikasi menjadi `Definitif`, dan Final ulang. Kultur urin masuk antrean.
> 3. Rabu 10.00 — dr. Nabila memvalidasi. dr. Contoh, pemegang kode validasi **Patologi Klinik**
>    saja, yang mencoba memvalidasinya sesaat sebelumnya **ditolak `403`** (`AC-241`).
> 4. Rabu 10.20 — perilis merilis. Satu dokumen `LaboratoryResult` terdaftar di rekam medis.
>    Respons hasil kini memuat *Validasi oleh: dr. Nabila* dan *Petugas Otorisasi: {perilis}*.
>    Order tetap *Dalam Pemeriksaan*, sebab kultur darah belum dirilis.
> 5. Rabu 10.30 — analis mencoba menambah isolat kedua pada kultur urin → **`409`** (`VAL-120`).

## 9. Amandemen `r7` — Penjaga penyelesaian order (`LAB-DEC-154`), 2026-09-25

| Field | Nilai |
|---|---|
| Revision | `r7` |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-25 — instruksi *"Setujui r36, r14, r7 beserta empat butir 22.7, lalu rencanakan BE-LAB-81"*, lihat `LAB-API-v1` `r36` bagian 31 |
| `input_revision` | decisions rev 77; `LAB-API-v1` `r36`; `LAB-VAL-v1` `r14` |
| Mengubah | Baris `InProcess` → `Completed` pada bagian 1. Baris aslinya dibiarkan sebagai jejak revisi 2 |

### 9.1 Transisi yang sah — syarat baru

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `InProcess` | Menyelesaikan | `Completed` | Petugas berwenang memproses (`LabOrder : Process`) | **Setiap pemeriksaan yang tidak batal dan tidak gugur sudah dirilis** (`VAL-146`). Order tanpa pemeriksaan tidak batal diterima | `409` beserta rincian pemeriksaan yang menahan |

**Nol nilai `LabOrderStatus` baru.** Tidak ada status *Menunggu Validasi* pada tingkat order —
*Menunggu Validasi* adalah label **pemeriksaan** (`LAB-DEC-156`).

### 9.2 Transisi yang tidak sah — tambahan

| Dari | Tindakan | Kenapa ditolak | Kode |
|---|---|---|---|
| `InProcess` dengan satu saja pemeriksaan tidak batal yang belum dirilis | Menyelesaikan | `VAL-146` — `LAB-DEC-154` | `409` |
| `InProcess` dengan pemeriksaan Patologi Anatomi atau Mikrobiologi yang belum dapat divalidasi | Menyelesaikan | Pemeriksaan tanpa jalur validasi tidak pernah dirilis — `LAB-DEC-154` butir 5 | `409` |
| Selain `InProcess` | Menyelesaikan | Sudah tertulis pada bagian 1 sebagai `409`; **kode hari ini menjawab `400`** dan diselaraskan oleh `r36` | `409` |

### 9.3 Contoh jalur lengkap

> Order `LAB-RSMMC-000000123` berisi Kalium, Hemoglobin, dan Glukosa. Nama tenaga di bawah samaran.
>
> 1. 08.30 — Glukosa dibatalkan. Order tetap `InProcess`.
> 2. 09.15 — Kalium dirilis. Label hasil order: *Dalam Pemeriksaan*.
> 3. 09.40 — Hemoglobin divalidasi, belum dirilis. Petugas menekan Selesai → **`409`**, rincian
>    *Hemoglobin — Tervalidasi*. Order tetap `InProcess`.
> 4. 09.50 — Hemoglobin dirilis. Label hasil order: *Selesai*; status order masih `InProcess`.
> 5. 10.00 — petugas menekan Selesai → **`Completed`**. Sejak saat ini order menolak pemeriksaan dan
>    wadah baru, seperti sebelumnya.
