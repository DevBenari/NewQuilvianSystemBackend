# Matriks Transisi Status — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.6.0` — penutupan kunjungan lewat disposisi, 23 September 2026, **Rencana (belum tersedia)**, status `draft`: bagian 9 baru (pemicu penutupan kunjungan, `IGD-DEC-163`…`167`). **Aditif** — nol status baru, nol transisi baru. Sebelumnya `0.5.0` — encounter-first, 22 September 2026, **Rencana (belum tersedia)**. **Aditif**: bagian 8 baru (titik lahir kunjungan, pengakhiran encounter Emergency oleh IGD, enum `EmergencyArrivalTimeSource` dan `EmergencyReconciliationRunStatus`); bagian 1–7 **tidak** diubah. Sebelumnya `0.4.0` — revisi 6. **Aditif**: bagian 6a murni baru, nol bagian lama diubah |
| Status | `draft`, **kecuali bagian 1, 1.1, 1.2, dan bagian 8 (encounter-first) yang `approved`**. Bagian 9 **`approved`** (`IGD-DEC-170`, 23 September 2026) |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | **Rizki Gunawan / 2026-08-24** — terbatas pada bagian 1, 1.1, 1.2 (`EmergencyVisitStatus`) lewat `IGD-DEC-093`. **Rizki Gunawan / 2026-09-22** — bagian 8 (encounter-first) lewat `IGD-DEC-157`. Bagian 2 sampai 7 tetap `draft` |
| Versi sebelumnya | `0.4.0`, `0.3.0`, sebelumnya `0.2.0` |

---

## 1. `EmergencyVisitStatus`

| Dari \ Ke | Arrived | WaitingForTriage | Triaged | InTreatment | UnderObservation | AwaitingDisposition | Disposed | Completed | Cancelled |
| --- | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: |
| **Arrived** | — | ✓ | — | ✓ | — | — | — | — | ✓ |
| **WaitingForTriage** | — | — | ✓ | ✓ | — | — | — | — | ✓ |
| **Triaged** | — | — | — | ✓ | ✓ | ✓ | — | — | ✓ |
| **InTreatment** | — | — | — | — | ✓ | ✓ | — | — | ✓ |
| **UnderObservation** | — | — | — | ✓ | — | ✓ | — | — | ✓ |
| **AwaitingDisposition** | — | — | — | ✓ | — | — | ✓ | — | ✓ |
| **Disposed** | — | — | — | — | — | — | — | ✓ | — |
| **Completed** | — | — | — | — | — | — | — | — | — |
| **Cancelled** | — | — | — | — | — | — | — | — | — |

### 1.1 Perubahan terhadap `0.2.0`

| Perubahan | Sebab |
| --- | --- |
| Transisi ke `Triaged` **hanya** dari `WaitingForTriage` | Menutup `IGD-GAP-014`. Sebelumnya controller triase menulis `Triaged` dari status mana pun |
| Penilaian ulang **tidak** mengubah status kunjungan | Pasien yang sudah `InTreatment` tetap `InTreatment` setelah dinilai ulang |
| `Disposed` → `Completed` **hanya** lewat aksi selesaikan kunjungan | Sudah berlaku sejak `0.2.0`; kini ditegakkan konsisten |

### 1.2 Aturan penegakan

| Aturan | Sebab |
| --- | --- |
| **Seluruh** penulisan `VisitStatus` wajib lewat `CanTransition` | Dua controller pernah menulisnya langsung — `IGD-CONF-05` |
| Triase yang diselesaikan pada kunjungan `Disposed`, `Completed`, atau `Cancelled` **ditolak** `409` | Mencegah kunjungan tertutup terbuka kembali |
| `Completed` bersifat final; `Completed` → `Completed` pun ditolak | Sudah berlaku sejak `0.2.0` |

---

## 2. `EmergencyPhysicalStatus` — baru

| Dari \ Ke | Prepared | Departed | Arrived | Cancelled |
| --- | :-: | :-: | :-: | :-: |
| **Prepared** | — | ✓ | — | ✓ |
| **Departed** | — | — | ✓ | ✓ |
| **Arrived** | — | — | — | — |
| **Cancelled** | — | — | — | — |

| Transisi | Pemilik klinis sesudahnya | Siapa yang boleh | Keputusan |
| --- | --- | --- | --- |
| → `Prepared` | IGD | Perawat IGD | `IGD-DEC-072` |
| → `Departed` | **Tetap IGD** | Perawat IGD | `IGD-DEC-072` |
| → `Arrived` | **Unit penerima** | Petugas berwenang atas unit tujuan | `IGD-DEC-064`, `IGD-DEC-086` |
| → `Cancelled` | IGD | Perawat IGD; alasan wajib | `IGD-DEC-069` |

`Arrived` bersifat final pada rangkaian ini. Koreksinya **tidak** memakai transisi, melainkan
kejadian `Amended` atau `Reversed` pada `TrxEmergencyDepartureEvent`.

---

## 3. `EmergencyHandoverStatus` — baru

| Dari \ Ke | Submitted | Pending | Accepted | Rejected | Cancelled |
| --- | :-: | :-: | :-: | :-: | :-: |
| **Submitted** | — | ✓ | ✓ | ✓ | ✓ |
| **Pending** | — | — | ✓ | ✓ | ✓ |
| **Accepted** | — | — | — | — | — |
| **Rejected** | — | ✓ | ✓ | — | ✓ |
| **Cancelled** | — | — | — | — | — |

`Rejected` → `Pending` mewakili pengirim memperbaiki dokumen lalu mengajukannya kembali.
`Rejected` **bukan** status terminal, karena serah terima yang ditolak tetap wajib dituntaskan
(`IGD-DEC-062`).

---

## 4. Kombinasi dua rangkaian

| Fisik | Dokumen | Sah | Arti |
| --- | --- | :-: | --- |
| `Prepared` | `Submitted` | ✓ | Menunggu keberangkatan |
| `Prepared` | `Pending` | ✓ | Dokumen menunggu peninjauan, pasien masih di IGD |
| `Prepared` | `Accepted` | ✗ | **Ditolak** — penerima tidak dapat menerima pasien yang belum berangkat |
| `Departed` | `Pending` | ✓ | Pasien di perjalanan, dokumen belum ditinjau |
| `Departed` | `Accepted` | ✓ | Dokumen selesai lebih dulu daripada kedatangan |
| `Arrived` | `Pending` | ✓ | **Keadaan sah** yang menjadi alasan pemisahan dua rangkaian — `IGD-DEC-070` |
| `Arrived` | `Rejected` | ✓ | Pemilik klinis sudah unit penerima; dokumen tetap outstanding — `IGD-DEC-062` |
| `Arrived` | `Accepted` | ✓ | Tuntas |
| `Cancelled` | selain `Cancelled` | ✗ | **Ditolak** — pembatalan fisik membatalkan dokumennya |

---

## 5. `EmergencyDispositionStatus`

Tidak berubah bentuknya. Yang berubah adalah akibatnya:

| Transisi | Akibat pada `0.2.0` | Akibat pada `0.3.0` |
| --- | --- | --- |
| → `Executed` | Kunjungan **selalu** menjadi `Disposed` | Kunjungan menjadi `Disposed` **hanya bila** `ClosesEmergencyVisit` bernilai benar pada jenis tindak lanjutnya |

---

## 6. `TrxEmergencyDoctorAssignment` — bukan status, melainkan rentang waktu

Entity ini tidak memiliki kolom status. Keadaannya ditentukan `EffectiveTo`:

| Keadaan | Ditandai oleh | Invariant |
| --- | --- | --- |
| Aktif | `EffectiveTo IS NULL` | **Tepat satu** per kunjungan IGD, dijaga unique index bersyarat |
| Berakhir | `EffectiveTo` terisi | Tidak pernah ditimpa |

Pengalihan dokter menutup baris lama dan membuka baris baru dalam **satu transaksi**.

---

## 6a. `EmergencyOrderAcceptanceStatus` — baru pada revisi 6

Penerimaan **per pesanan**, terpisah dari `EmergencyHandoverStatus` milik dokumen serah terima.
Ditetapkan `IGD-DEC-102`.

| Dari \ Ke | NotRequired | Pending | Accepted | Rejected |
| --- | :-: | :-: | :-: | :-: |
| **NotRequired** | — | — | — | — |
| **Pending** | — | — | ✓ | ✓ |
| **Accepted** | — | — | — | — |
| **Rejected** | — | — | — | — |

### 6a.1 Nilai awal ditentukan sikap pesanan

| `Action` | `AcceptanceStatus` awal | Sebab |
| --- | --- | --- |
| `Continue` | `NotRequired` | Pesanan tetap berjalan di tempatnya; tidak ada unit penerima yang perlu menerima |
| `Cancel` | `NotRequired` | Pesanan dihentikan; tidak ada yang diserahkan |
| `Handover` | `Pending` | Menunggu penerimaan eksplisit unit penerima |

### 6a.2 Aturan penegakan

| Aturan | Sebab |
| --- | --- |
| `Accepted` dan `Rejected` bersifat **final** pada barisnya | Perubahan sesudahnya ditulis sebagai baris baru, bukan menimpa — `IGD-DEC-102` butir (c) |
| `Rejected` **wajib** beralasan | `IGD-DEC-102` |
| `Rejected` **tidak** mengubah `EmergencyHandoverStatus` maupun status kepergian pasien | `IGD-DEC-102` butir (a) dan (d). Perpindahan pasien tetap sah |
| Pesanan `Rejected` wajib punya baris pengganti ber-`SupersedesOrderItemId` sebelum kunjungan ditutup | `IGD-DEC-102` butir (c) |
| Baris pengganti boleh ber-`Action` `Continue`, `Handover` ke penerima lain, atau `Cancel` | `IGD-DEC-102` butir (c) |

### 6a.3 Tiga rangkaian yang berjalan sendiri-sendiri

`IGD-DEC-102` menyatakan penerimaan pasien, dokumen serah terima, dan setiap pesanan adalah
fakta yang terpisah. Akibatnya modul ini punya **tiga** rangkaian yang tidak saling menahan:

| Rangkaian | Enum | Menjawab |
| --- | --- | --- |
| Keadaan fisik pasien | `EmergencyPhysicalStatus` | Pasien sudah berangkat dan tiba? |
| Dokumen serah terima | `EmergencyHandoverStatus` | Unit tujuan menerima **pasiennya**? |
| Tiap pesanan | `EmergencyOrderAcceptanceStatus` | Unit tujuan menerima **pesanan ini**? |

Satu pesanan yang `Rejected` **tidak** menggeser dua rangkaian di atasnya. Inilah alasan
penerimaan pesanan tidak ditumpangkan pada `EmergencyHandoverStatus`.

---

## 7. Status yang sengaja tidak dibuat

| Yang dipertimbangkan | Ditolak karena |
| --- | --- |
| Status `InTransit` tersendiri | Sudah diwakili rangkaian fisik `Departed` |
| Status `BedAllocated` | Alokasi tempat tidur milik Rawat Inap — `IGD-DEC-069` |
| Status `ReadyToTransfer` tersendiri | Sudah diwakili `Prepared` |
| Status kunjungan `Reopened` | Kunjungan tertutup **tidak boleh** dibuka kembali — `IGD-GAP-014` |
| Rangkaian ketiga untuk serah terima dokter | `IGD-DEC-079` memilih satu dokumen untuk rilis pertama |
| Status pesanan yang mencerminkan keadaan di laboratorium | `LabOrder` tidak punya kolom status. Sistem **dilarang** menebaknya — `IGD-DEC-101` |
| Menumpangkan penerimaan pesanan pada `EmergencyHandoverStatus` | Satu pesanan yang ditolak akan menggagalkan penerimaan pasien, yang dilarang `IGD-DEC-102` butir (d) |

---

## 8. Encounter-first — baru pada `0.5.0`, **Rencana (belum tersedia)**

Bagian 1, 1.1, dan 1.2 (`approved`) **tidak** diubah: tidak ada transisi `EmergencyVisitStatus` baru. Yang
baru adalah **titik lahir** kunjungan, keadaan encounter Emergency yang dikelola IGD, dan dua status milik
tabel baru. Keputusan: `IGD-DEC-139`, `142`, `143`, `147`, `148`, `153`.

**Status bagian ini: `approved`** — `IGD-DEC-157`, 22 September 2026; terkunci hash (manifest bagian 2).

### 8.1 Titik lahir kunjungan

Kunjungan tidak lagi lahir di loket. Ia lahir dari encounter Emergency yang belum berakhir, lewat satu aksi:

| Aksi | Status awal kunjungan | Catatan |
| --- | --- | --- |
| **Mulai Triage** | `WaitingForTriage` | Triage pertama yang tersimpan memindahkannya ke `Triaged` seperti hari ini (bagian 1) |
| **Tangani Segera** | `InTreatment` (`TreatmentStartedAt` = waktu server) | Triage disusulkan tanpa memundurkan status (`IGD-DEC-104` huruf b) |
| Jalur lama `POST /emergency-visits` | Sesuai request, seperti hari ini | Tidak dipakai layar sesudah `FE-IGD-036`; dipertahankan untuk data lama |

**Tabrakan** Mulai Triage dan Tangani Segera pada encounter yang sama: satu kunjungan, hasil akhir
`InTreatment` (`IGD-DEC-143`). Tangani Segera pada kunjungan `WaitingForTriage`/`Arrived` memakai transisi
yang sudah sah pada bagian 1; pada status lain tidak mengubah apa pun.

### 8.2 Encounter Emergency yang dikelola IGD

`EncounterStatus` milik Registration Management; tabel di bawah hanya mengatur **siapa yang boleh
mengakhiri encounter bertipe Emergency** dan kapan. Tipe lain tidak berubah.

| Keadaan encounter | Artinya | Bagaimana berakhir | Pelaku | Hasil |
| --- | --- | --- | --- | --- |
| Belum berakhir, **tanpa** kunjungan | *Menunggu Triage* | Pasien pergi sebelum ditriage | Perawat triage — `POST /no-show` | `NoShow` + `NoShowAt`/`By`/`Reason` — **final** |
| Belum berakhir, **tanpa** kunjungan | *Menunggu Triage* | Salah daftar / duplikat | Petugas pendaftaran — `PATCH /patient-encounters/{id}/cancel` | Pembatalan Registrasi apa adanya (`IsCancel`, `CancelledAt`/`By`, `CancelReason`, `IsActive = false`) |
| Belum berakhir, **dengan** kunjungan | Episode berjalan | Kunjungan `Completed` | Sistem, pada penyimpanan aksi selesaikan kunjungan | `Completed` + `CompletedAt` + penguncian catatan (`RM-DEC-003`) |
| Belum berakhir, **dengan** kunjungan | Episode berjalan | Kunjungan `Cancelled` | Sistem, pada penyimpanan aksi batal kunjungan | `Cancelled` + tanda pembatalan |
| Belum berakhir, kunjungannya **dihapus lunak** (K4) | Tidak normal | **Tidak** berakhir otomatis | — | Dilaporkan rekonsiliasi |
| Sudah berakhir | — | — | — | Tidak ditulis ulang oleh aksi mana pun |

**Transisi yang ditolak** (validation §10.1 aturan 8–9, §10.3):

| Upaya | Ditolak karena |
| --- | --- |
| `PATCH /patient-encounters/{id}/status` pada Emergency | Jalur umum tanpa aturan IGD (`IGD-DEC-153`) |
| `PATCH …/cancel` pada Emergency yang sudah punya kunjungan | Akan menciptakan "kunjungan berjalan, encounter batal" |
| NoShow pada encounter yang sudah punya kunjungan | Kunjungan punya jalur penutupannya sendiri |
| Menghidupkan kembali encounter yang sudah `NoShow` | NoShow final (`IGD-DEC-142`); pasien didaftarkan ulang |

### 8.3 `EmergencyArrivalTimeSource` — sumber waktu tiba kunjungan (enum baru)

| Nilai | Nama | Arti |
| ---: | --- | --- |
| `0` | `Unverified` | Data lama, atau diisi lewat jalur lama di loket. **Bawaan** kolom untuk baris yang sudah ada |
| `1` | `Fallback` | Diisi otomatis dari `RegisteredAt` karena kunjungan lahir lewat Tangani Segera |
| `2` | `Confirmed` | Diisi atau dikonfirmasi perawat (Mulai Triage, atau `PATCH /{id}/arrival-time`) |

| Dari \ Ke | `Unverified` | `Fallback` | `Confirmed` |
| --- | :-: | :-: | :-: |
| **`Unverified`** | — | — | ✓ konfirmasi/koreksi |
| **`Fallback`** | — | — | ✓ konfirmasi/koreksi |
| **`Confirmed`** | — | — | ✓ koreksi ulang (tetap `Confirmed`, pelaku dan waktu diperbarui) |

Tidak ada jalan kembali ke `Unverified` atau `Fallback`.

### 8.4 `EmergencyReconciliationRunStatus` — status run rekonsiliasi (enum baru)

| Nilai | Nama | Arti |
| ---: | --- | --- |
| `1` | `Executed` | Baris K1/K1-Outpatient sudah ditutup |
| `2` | `Reversed` | Run sudah dibalik (seluruh atau sebagian baris — yang dilewati tercatat per baris) |

| Dari \ Ke | `Executed` | `Reversed` |
| --- | :-: | :-: |
| **`Executed`** | — | ✓ `POST /runs/{id}/reverse` |
| **`Reversed`** | — | — (final) |

Preview **tidak** membuat run.

### 8.5 Yang sengaja tidak dibuat

| Tidak dibuat | Sebab |
| --- | --- |
| Status "Menunggu Triage" sebagai nilai baru di `EncounterStatus` | Keadaan itu sudah terbaca dari "encounter belum berakhir dan belum punya kunjungan"; menambah nilai enum di tabel global menyentuh seluruh modul pembaca status |
| Status `LeftWithoutBeingSeen` tersendiri | `IGD-DEC-142` memilih `NoShow` yang sudah ada |
| Pembatalan NoShow | `IGD-DEC-142` — final |

## 9. Pemicu penutupan kunjungan — baru pada `0.6.0`, **Rencana (belum tersedia)**

Bagian 1 dan 8 **tidak** diubah: nol status baru, nol transisi baru. Yang baru hanya **siapa yang memicu**
perpindahan `Disposed` → `Completed` yang sudah sah. Keputusan: `IGD-DEC-163`…`167`.

**Status bagian ini: `draft`** — menunggu approval pemilik.

### 9.1 Dua jalan menuju `Completed`

| Jalan | Pemicu | Pelaku | Penjaga | Penanda asal |
| --- | --- | --- | --- | --- |
| Manual (sudah ada) | Petugas menekan selesaikan kunjungan (`PATCH /emergency-visits/{id}/complete`) | Petugas itu | Penjaga penutupan §6 | `ClosedByDispositionId` **kosong** |
| Susulan disposisi (**baru**) | Disposisi berpindah ke `Executed`, atau penahan terakhir dibereskan sesudahnya | Petugas yang melakukan aksi pemicu | Penjaga penutupan §6 yang **sama** | `ClosedByDispositionId` **terisi** |

Kedua jalan memakai penjaga transisi `BE-IGD-018` yang sama, sehingga `Completed` tetap satu-satunya titik akhir
dan tetap final.

### 9.2 Empat titik pemicu

| # | Aksi petugas | Kapan penutupan dicoba |
| ---: | --- | --- |
| 1 | Disposisi berpindah ke `Executed` | Segera sesudah kunjungan dipindahkan ke `Disposed` pada aksi yang sama |
| 2 | Observasi berpindah keluar dari status aktif | Sesudah status observasi disimpan |
| 3 | Serah terima diterima, ditolak, atau dibatalkan | Sesudah status serah terima disimpan |
| 4 | Sikap atas pesanan yang ditolak unit penerima ditetapkan | Sesudah sikap pesanan disimpan |

Titik 2, 3, dan 4 hanya berbuat sesuatu bila kunjungan itu memang **menunggu penutupan** — yaitu punya disposisi
`Executed` dan belum selesai. Bila tidak, aksinya berjalan seperti biasa tanpa efek samping.

### 9.3 Keadaan "menunggu penutupan"

Bukan status baru pada `EmergencyVisitStatus`, melainkan **keadaan yang terbaca** dari data yang sudah ada:
kunjungan `Disposed`, punya disposisi `Executed`, dan penjaga penutupan masih menolak. Status enum tidak ditambah
karena keadaan ini sementara dan sudah dapat dihitung — menambah nilai enum akan menyentuh seluruh modul pembaca
status kunjungan.

### 9.4 Transisi yang ditolak

| Upaya | Ditolak karena |
| --- | --- |
| Membatalkan disposisi `Executed` pada kunjungan yang sudah `Completed` | `IGD-DEC-166` — penyelesaian kunjungan final (validation §11 aturan 8) |
| Penutupan susulan atas kunjungan `Completed`/`Cancelled` | Penjaga transisi `BE-IGD-018`; dilewati diam-diam, bukan galat |
| Membuka kembali kunjungan yang sudah selesai | Tidak ada jalur mana pun yang menyediakannya |
