# Matriks Transisi Status — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.15.0` — amandemen 18 Oktober 2026 (`IGD-DEC-324`), **Rencana (belum tersedia)**, status **`approved`** (Rizki Gunawan, 18 Oktober 2026): bagian 11.8 — tindak lanjut rawat inap yang dijalankan petugas admisi: promosi kunjungan ke `Disposed`, _Catat Tiba_ sebagai titik pemicu ke-5 (§9.2), dan penahan "pasien belum tiba"; dikerjakan `BE-IGD-083`. *Sebelumnya:* `0.14.0` — amandemen 16 Oktober 2026 (`IGD-DEC-311`), **Tersedia**, status **`approved`** (Rizki Gunawan, 16 Oktober 2026): bagian 11.7 — transisi status kunjungan non-ranap (sequential promotion ke `AwaitingDisposition` sebelum `Disposed`) dan pemicu status `InTreatment`. *Sebelumnya:* `0.13.0` — amandemen 15 Oktober 2026 (`IGD-DEC-308`, `309`): bagian 11.6 — reaktivasi `CancelledByAdmission` dan transisi penutupan kunjungan IGD dua arah. *Sebelumnya:* `0.12.0` — amandemen 14 Oktober 2026 (`IGD-DEC-304`, `305`): bagian 11.5. *Sebelumnya:* `0.11.0` — Form Permintaan Rawat Inap dari IGD, 12 Oktober 2026 (`IGD-DEC-294`). |
| Status | `draft`, **kecuali bagian 11 (Form Permintaan Rawat Inap dari IGD, Non-Ranap, dan Penutupan Kunjungan) yang `approved`** (`IGD-DEC-294`, 12 Oktober 2026; amandemen 11.5 `IGD-DEC-306`, 14 Oktober 2026; amandemen 11.6 `IGD-DEC-308`/`309`, 15 Oktober 2026; amandemen 11.7 `IGD-DEC-311`, 16 Oktober 2026; amandemen 11.8 dan baris §9.2 nomor 5 `IGD-DEC-324`, 18 Oktober 2026); `draft`, **kecuali bagian 10 (Ruang Kerja Dokter IGD) yang `approved`** (`IGD-DEC-230`, 6 Oktober 2026); **kecuali bagian 1, 1.1, 1.2, dan bagian 8 (encounter-first) yang `approved`**. |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | **Rizki Gunawan / 2026-10-18** — amandemen bagian 11.8 (`IGD-DEC-324`). **Rizki Gunawan / 2026-10-16** — amandemen bagian 11.7 (`IGD-DEC-311`). **Rizki Gunawan & Muhammad Hamzah / 2026-10-15** — amandemen bagian 11.6 (`IGD-DEC-308`, `309`). **Rizki Gunawan / 2026-10-14** — amandemen bagian 11.5 (`IGD-DEC-306`). **Rizki Gunawan / 2026-10-06** — bagian 10 (`IGD-DEC-230`). **Rizki Gunawan / 2026-08-24** — bagian 1, 1.1, 1.2 (`IGD-DEC-093`). |
| `input_revision` | `0.15.0`: `IGD-DEC-324` (18 Oktober 2026; dasar `IGD-DEC-323`, `IGD-FACT-128`, `130`…`132`). `0.14.0`: `IGD-DEC-311` (16 Oktober 2026). `0.13.0`: `IGD-DEC-308`, `309` (15 Oktober 2026). `0.12.0`: `IGD-DEC-304`, `305` (14 Oktober 2026). |
| Versi sebelumnya | `0.14.0`, `0.13.0`, `0.12.0`, `0.11.0`, `0.10.0`, `0.9.0`, `0.8.0`, `0.7.0`, `0.6.0`, `0.5.0`, `0.4.0`, `0.3.0`, sebelumnya `0.2.0` |

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
| Pesanan `Rejected` wajib punya baris pengganti ber-`SupersedesOrderItemId` sebelum kunjungan ditutup — *kecuali milik kepergian yang fisiknya `Cancelled` (ditambahkan pada `0.9.0`; validation §5.1)* | `IGD-DEC-102` butir (c); `IGD-DEC-205` |
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

**Status bagian ini: `approved`** untuk 9.1–9.4 — `IGD-DEC-170`, Rizki Gunawan, 23 September 2026; terkunci hash
(manifest bagian 0j). **Bagian 9.5 dan baris §9.4 *eskalasi observasi pada kunjungan `Disposed`*: `approved`** —
`IGD-DEC-175`, Rizki Gunawan, 30 September 2026 (sementara, `IGD-DEC-174`); amendment `IGD-DEC-171`, `172`.
**Bagian 9.6 dan baris §9.4 *aksi observasi pada kunjungan yang sudah berakhir*: `approved`** — `IGD-DEC-186`,
Rizki Gunawan, 3 Oktober 2026 (sementara); amendment `IGD-DEC-183`, `184`.

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
| 4 | Sikap atas pesanan yang ditolak unit penerima ditetapkan. *Diselaraskan pada `0.9.0` dengan validation §11 aturan 4 (`IGD-DEC-203`): termasuk penetapan sikap pertama atas pesanan tanpa sikap* | Sesudah sikap pesanan disimpan |
| 5 | Pasien dicatat tiba di unit tujuan (`POST …/emergency-departures/{id}/arrive`) — **baru pada `0.15.0`** (§11.8, `IGD-DEC-324`) | Sesudah status fisik `Arrived` disimpan |

Titik 2, 3, 4, dan 5 hanya berbuat sesuatu bila kunjungan itu memang **menunggu penutupan** — yaitu punya disposisi
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
| Eskalasi observasi pada kunjungan `Disposed` — **baru pada `0.7.0`** | `IGD-DEC-172` — kunjungan yang tindak lanjutnya sudah dilaksanakan tidak dibuka kembali ke `InTreatment`, dan eskalasi tanpa perpindahan status akan melepas observasi dari daftar penahan penutupan (§9.5; validation §11 aturan 14) |
| Menyelesaikan, mengeskalasi, atau mengaktifkan observasi pada kunjungan `Completed`/`Cancelled` — **baru pada `0.8.0`** | `IGD-DEC-184`, `IGD-DEC-166` — kunjungan yang sudah berakhir tidak dibuka kembali, dan observasinya tidak diubah lagi. Penolakannya sudah terjadi sebelumnya lewat penjaga transisi; yang baru adalah pesannya sendiri (§9.6; validation §11.2 aturan 18). *Batalkan* tetap lolos |

### 9.5 Pemetaan observasi → kunjungan pada kunjungan `Disposed` — baru pada `0.7.0`

Keputusan: `IGD-DEC-171`, `IGD-DEC-172` (amendment pass 30 September 2026), disetujui sementara lewat
`IGD-DEC-174`. Bagian 1 **tidak** diubah: baris `Disposed` tetap hanya mengizinkan `Completed`.

**Masalah yang ditutup.** Aksi ubah status observasi memindahkan status kunjungan sesuai status tujuan observasinya
(`IGD-FACT-029`). Pada kunjungan `Disposed`, dua pemetaan itu mencoba transisi yang memang tidak sah menurut
bagian 1 — `Disposed` → `AwaitingDisposition` dan `Disposed` → `InTreatment` — sehingga keduanya ditolak `409`
(`IGD-FACT-030`). Akibatnya observasi yang masih berjalan sesudah disposisi dilaksanakan hanya dapat dibatalkan, dan
kesimpulannya hilang.

| Status tujuan observasi | Kunjungan berjalan selain `Disposed` (tidak berubah) | Kunjungan `Disposed` |
| --- | --- | --- |
| `Active` | Kunjungan dipindahkan ke `UnderObservation` lewat penjaga `BE-IGD-018` | **Tidak berubah** — penjaga menolak `409` dengan pesannya sendiri |
| `Completed` | Kunjungan dipindahkan ke `AwaitingDisposition` lewat penjaga | **Baru — status kunjungan tetap `Disposed`**; penjaga tidak dipanggil karena tidak ada perpindahan. Observasi dan kesimpulannya disimpan, lalu penutupan susulan dicoba (§9.2 titik 2) — `IGD-DEC-171` |
| `Escalated` | Kunjungan dipindahkan ke `InTreatment` lewat penjaga | **Baru — ditolak lebih dulu** dengan pesan validation §11 aturan 14, menggantikan pesan teknis penjaga. Observasi, kunjungan, dan catatannya tidak berubah — `IGD-DEC-172` |
| `Cancelled` | Tidak memindahkan status kunjungan | **Tidak berubah** — tidak memindahkan; penutupan susulan dicoba (§9.2 titik 2) |

Aturan `Completed` berlaku dari `Active` maupun dari `Escalated`, karena keduanya asal yang sah menuju `Completed`
pada rangkaian status observasi.

**Mengapa eskalasi ditolak, bukan diterima tanpa memindahkan status.** Penjaga penutupan hanya menghitung observasi
`Active` sebagai penahan (`IGD-FACT-031`). Eskalasi yang diterima diam-diam akan mengubah observasi menjadi
`Escalated`, melepasnya dari daftar penahan, lalu penutupan susulan dapat **menutup kunjungan tepat saat pasien
memburuk**. Dengan penolakan, observasinya tetap `Active` dan tetap menahan penutupan. Cara mencatat pasien yang
memburuk sesudah tindak lanjutnya dilaksanakan belum diputuskan — `IGD-OQ-111`, tidak menahan bagian ini.

*Contoh.* Pasien diputuskan pulang dan disposisinya dilaksanakan pukul 14.00; observasinya masih berjalan, sehingga
kunjungan menunggu penutupan. Pukul 15.00 perawat menyelesaikan observasi dengan kesimpulan *"tanda vital stabil"*:
observasi `Completed`, kesimpulan tersimpan, kunjungan tidak dicoba dipindahkan ke `AwaitingDisposition`, dan pada
penyimpanan yang sama kunjungan tertutup bila tak ada penahan lain. Bila pukul 15.00 perawat justru menekan eskalasi,
permintaan ditolak, observasi tetap `Active`, dan kunjungan tetap menunggu penutupan.

**Yang tidak berubah oleh bagian ini.**

| Keadaan | Perilaku | Catatan |
| --- | --- | --- |
| Aksi observasi pada kunjungan `Completed` atau `Cancelled` | Tetap ditolak penjaga `409` untuk target `Active`, `Completed`, `Escalated`; `Cancelled` tetap lolos | Di luar `IGD-DEC-171`, yang hanya menyebut `Disposed`. **Catatan, bukan keputusan:** observasi `Escalated` tidak menahan penutupan (`IGD-FACT-031`), sehingga dapat tertinggal pada kunjungan yang sudah `Completed` dan hanya dapat dibatalkan. *Sejak `0.8.0` catatan ini dijawab §9.6: `Escalated` menahan penutupan (`IGD-DEC-183`), dan penolakan pada kunjungan berakhir memakai pesannya sendiri (`IGD-DEC-184`)* |
| Kunjungan `Disposed` yang tidak punya disposisi `Executed` (bila ada pada data lama) | Aturan `Completed` tetap berlaku; penutupan susulan dilewati karena tidak ada pemicunya (§9.2) | Kunjungan tetap `Disposed`. Pada source hari ini `Disposed` hanya dicapai lewat disposisi `Executed` |

### 9.6 Observasi Dieskalasi sebagai penahan penutupan — baru pada `0.8.0`

Keputusan: `IGD-DEC-183`, `IGD-DEC-184` (amendment pass 3 Oktober 2026); dikerjakan lewat `BE-IGD-061`
(`IGD-DEC-185`). Bagian 1 **tidak** diubah: nol status baru dan nol transisi baru. Aturan pesannya di validation
§11.2.

**Status bagian ini: `approved`** — `IGD-DEC-186`, Rizki Gunawan, 3 Oktober 2026 (sementara, pola `IGD-DEC-174`).

**Masalah yang ditutup.** Rangkaian status observasi memperlakukan `Escalated` sebagai keadaan antara: periode itu
masih menerima pemantauan (`IGD-DEC-126`), dan baru tuntas lewat `Escalated → Completed` atau
`Escalated → Cancelled` (`IGD-FACT-033`, `034`). Penjaga penutupan justru memperlakukannya sebagai sudah selesai
(`IGD-FACT-031`). Dari selisih itulah kunjungan dapat tertutup sementara observasinya masih Dieskalasi (temuan uji
`BE-IGD-061` S6).

**Penahan penutupan menurut status observasi.**

| Status observasi | Periode sudah ditutup? (`IGD-DEC-126`) | Menahan penutupan kunjungan? | Sejak |
| --- | :-: | :-: | --- |
| `Active` | Belum | **Ya** | Sudah ada |
| `Escalated` | Belum | **Ya** | **`0.8.0`** — sebelumnya tidak |
| `Completed` | Sudah | Tidak | Sudah ada |
| `Cancelled` | Sudah | Tidak | Sudah ada |

Dengan baris kedua, kedua aturan kini memakai arti yang sama: periode yang belum ditutup menahan penutupan
kunjungan.

**Titik pemicu §9.2 nomor 2 dibaca ulang.** *"Observasi berpindah keluar dari status aktif"* berarti observasi
berpindah ke status yang **tidak** menahan, yaitu `Completed` atau `Cancelled`, dari `Active` maupun `Escalated`.
Perpindahan ke `Escalated` pada kunjungan `Disposed` tetap ditolak (§9.5), sehingga tidak pernah menjadi pemicu.

**Aksi observasi menurut status kunjungan.**

| Status tujuan observasi | Kunjungan berjalan selain `Disposed` | Kunjungan `Disposed` | Kunjungan `Completed`/`Cancelled` |
| --- | --- | --- | --- |
| `Active` | Kunjungan → `UnderObservation` (tidak berubah) | Ditolak penjaga (tidak berubah) | **Ditolak dengan pesan sendiri** — validation §11.2 aturan 18 |
| `Completed` | Kunjungan → `AwaitingDisposition` (tidak berubah) | Status kunjungan tetap; penutupan susulan dicoba (§9.5) | **Ditolak dengan pesan sendiri** — aturan 18 |
| `Escalated` | Kunjungan → `InTreatment` (tidak berubah) | Ditolak, pesan validation §11.1 aturan 14 (§9.5) | **Ditolak dengan pesan sendiri** — aturan 18 |
| `Cancelled` | Tidak memindahkan (tidak berubah) | Tidak memindahkan; penutupan susulan dicoba (§9.5) | Tidak memindahkan, tidak memicu apa pun — **tetap lolos** (aturan 19) |

Kolom ketiga hanya mengganti **pesan**: ketiga target itu sudah ditolak penjaga transisi sebelumnya, dengan kalimat
teknis seperti *"Status kunjungan tidak dapat berubah dari Completed ke AwaitingDisposition."*

**Data lama.** Observasi `Escalated` yang sudah tertinggal pada kunjungan berakhir dibiarkan apa adanya
(`IGD-DEC-184`): tidak diubah massal dan kunjungannya tidak dibuka kembali (`IGD-DEC-166`). Jumlahnya dihitung per
lingkungan oleh pemilik (`IGD-OQ-112`).

*Contoh.* Pukul 10.00 observasi pasien dieskalasi: kunjungan kembali `InTreatment`, observasi `Escalated`. Pukul
13.00 disposisi pulang dilaksanakan: kunjungan `Disposed`, tetapi penjaga menolak penutupan karena observasi masih
`Escalated`, sehingga kunjungan menunggu penutupan. Pukul 13.10 perawat menyelesaikan observasi itu:
`Escalated → Completed`, status kunjungan tidak dipindahkan, lalu penutupan susulan berhasil —
`Disposed → Completed` atas nama perawat itu, dengan `ClosedByDispositionId` menunjuk disposisi pukul 13.00.

---

## 10. Ruang Kerja Dokter IGD — baru pada `0.10.0`, **Rencana (belum tersedia)**

Untuk `IGD-DEC-220`…`229`. Status **`approved`** (`IGD-DEC-230`, Rizki Gunawan, 6 Oktober 2026). **Nol status baru.** Bagian ini mengunci dua hal
pada status milik IGD, dan mencatat status milik modul lain yang dipakai layar dokter tanpa diubah.

### 10.1 `EmergencyDispositionStatus` — kelahiran dan konfirmasi

| Dari | Ke | Lewat | Syarat | Bila syarat gagal |
| --- | --- | --- | --- | --- |
| *(belum ada)* | `Draft` (1) | `POST /emergency-dispositions` | Ruas wajib sah | `400` ruas |
| *(belum ada)* | `Confirmed` (2), `Executed` (3), `Cancelled` (4) | `POST /emergency-dispositions` | **Tidak sah** — `IGD-CONF-11` | `400` validation §12 aturan 14 |
| `Draft` | `Confirmed` | `PATCH …/disposition-status` | Kunjungan punya diagnosis yang dihitung (validation §12 aturan 15) | `409`; status tetap `Draft` |

Transisi lain pada §5 tidak berubah. Karena status awal selalu `Draft`, setiap tindak lanjut yang sampai `Executed`
pasti melewati `PATCH`, sehingga pemicu penutupan susulan (§9) tidak dapat dilewati.

### 10.2 Status milik modul lain yang dipakai layar dokter — tidak diubah

| Objek | Pemilik | Jalur status yang dipakai | Catatan |
| --- | --- | --- | --- |
| Catatan dokter (`TrxDoctorConsultation`) | ClinicalManagement | Draf → selesai (terkunci) → koreksi lewat addendum | `IGD-DEC-227`; penyelesaian hanya oleh penulisnya |
| Tindakan keperawatan IGD (`TrxPatientProcedure`) | ClinicalManagement | Lahir `Completed`, sumber `NursingAction`; pembatalan memakai jalur pembatalan yang sudah ada | Tindakan sudah dikerjakan saat dicatat, sehingga tidak melewati `Planned`/`Ordered` |
| Verifikasi instruksi pesanan lab dan radiologi | Laboratorium, Radiologi | `NotRequired` (pesanan dokter) atau `Pending` → terverifikasi (pesanan perawat atas instruksi) | `IGD-DEC-224`, menunggu `IGD-OQ-117` |
| Pesanan darah, hemodialisa, gizi | Modul masing-masing | Status masing-masing modul | Dibaca apa adanya |

### 10.3 Status yang sengaja tidak dibuat

| Tidak dibuat | Sebab |
| --- | --- |
| Status tindak lanjut *menunggu diagnosis* | Diagnosis diperiksa pada saat konfirmasi; draf tanpa diagnosis memang keadaan sah |
| Status *ditangani dokter* pada kunjungan | DPJP sudah tercatat sebagai rentang waktu penugasan (§6); daftar dokter membaca penugasan itu |

## 11. Form Permintaan Rawat Inap dari IGD — baru pada `0.11.0`, **Rencana (belum tersedia)**

Untuk `IGD-DEC-267`…`293`. Status **`approved`** (`IGD-DEC-294`, Rizki Gunawan, 12 Oktober 2026). Satu lingkaran status baru milik IGD
(`EmergencyAdmissionRequestStatus`), satu perluasan status milik Rawat Inap (sumber dan penyebab batal; nilai status
`InpAdmissionReferralStatus` **tidak** bertambah), dan satu syarat baru pada `EmergencyDispositionStatus`.

### 11.1 `EmergencyAdmissionRequestStatus` — milik IGD

`Draft = 1`, `AwaitingSignature = 2`, `Signed = 3`, `Cancelled = 4`. Status awal: `Draft`. Terminal: `Signed`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| *(belum ada)* | Buat form | `Draft` | Perawat IGD | Validation §14 aturan 1–4 | `409` |
| `Draft` | Simpan draf | `Draft` | Perawat IGD | Aturan 5–10 | `400`/`409` |
| `Draft` | Minta TTD | `AwaitingSignature` | Perawat IGD | Aturan 11–12 | `400`/`409` |
| `AwaitingSignature` | Kembalikan + catatan | `Draft` | DPJP IGD aktif | Aturan 13, 14, 17 | `403`/`409`/`400` |
| `AwaitingSignature` | Tandatangani | `Signed` (+ permintaan admisi `Pending` lahir) | DPJP IGD aktif | Aturan 13–16 | `403`/`409`/`503` |
| `Draft`, `AwaitingSignature` | Batalkan + alasan | `Cancelled` | Perawat IGD | Aturan 19 | `400` |
| `Draft`, `AwaitingSignature` | Tindak lanjut `RANAP` dibatalkan | `Cancelled` | Sistem (akibat pembatalan oleh dokter) | — | — |
| `Signed` | Batalkan + alasan | `Signed` (form tetap); permintaan → `Cancelled`, sumber `EmergencyNurse` | Perawat IGD | Permintaan `Pending` | `409` aturan 19 |
| `Signed` | Tindak lanjut `RANAP` dibatalkan | `Signed` (form tetap); permintaan → `Cancelled`, sumber `EmergencyDispositionCancelled` | Sistem | Permintaan `Pending`; bila `Completed` pembatalan tindak lanjut ditolak (aturan 24) | `409` |

**Transisi tidak sah** (selalu `409`): `Draft` → `Signed` langsung; `AwaitingSignature` → ubah isian; `Signed` → `Draft`,
`AwaitingSignature`, atau ubah isian; `Cancelled` → apa pun. Form yang dibatalkan tidak dibuka kembali; perawat membuat
form baru.

### 11.2 Keadaan tampil (`displayStatus`) — turunan, tidak disimpan

| `status` form | Status permintaan admisi | `displayStatus` | Bunyi di layar |
| --- | --- | --- | --- |
| `Draft` | — | `Draft` | Draf |
| `AwaitingSignature` | — | `AwaitingSignature` | Menunggu TTD dr. {DPJP aktif} |
| `Signed` | `Pending` | `Sent` | Terkirim ke petugas admisi |
| `Signed` | `Completed` | `Completed` | Selesai — admisi dibuka |
| `Signed` | `Cancelled` sumber `AdmissionOfficer` | `CancelledByAdmission` | Dibatalkan admisi — {alasan} |
| `Signed` | `Cancelled` sumber lain | `Cancelled` | Dibatalkan — {alasan} |
| `Cancelled` | — | `Cancelled` | Dibatalkan — {alasan} |

### 11.3 `InpAdmissionReferral` — **butuh persetujuan pemilik Rawat Inap**

Nilai status tetap `Pending`, `Completed`, `Cancelled`. Yang baru:

| Dari | Tindakan | Ke | Siapa | `CancellationSource` |
| --- | --- | --- | --- | --- |
| *(belum ada)* | Form IGD ditandatangani | `Pending` (`SourceKind = Emergency`) | Sistem, dalam transaksi tanda tangan | — |
| `Pending` | Perawat IGD membatalkan form | `Cancelled` | Perawat IGD | `EmergencyNurse` |
| `Pending` | Tindak lanjut `RANAP` dibatalkan | `Cancelled` | Sistem | `EmergencyDispositionCancelled` |
| `Pending` | Petugas admisi membatalkan | `Cancelled` | Petugas admisi | `AdmissionOfficer` |
| `Pending` | Admisi dibuka dari permintaan | `Completed` | Petugas admisi (sudah ada) | — |
| `Pending` | Keputusan kamar pulih berubah | `Cancelled` | Sistem (sudah ada) | `RecoveryDecisionChanged` |

Tidak sah: `Completed` atau `Cancelled` → apa pun (sudah ada).

### 11.4 `EmergencyDispositionStatus` — syarat baru untuk jenis `RequiresAdmissionRequest`

| Dari | Ke | Syarat baru | Bila gagal |
| --- | --- | --- | --- |
| `Confirmed` | `Executed` | Ada form `Signed` untuk tindak lanjut ini dengan permintaan `Pending` atau `Completed` | `409` validation §14 aturan 23 |
| `Confirmed` | `Cancelled` | Permintaan admisi tindak lanjut ini tidak `Completed`; form aktif dan permintaan `Pending` ikut batal | `409` aturan 24 |

Transisi lain pada §5 dan §10 tidak berubah; jenis tindak lanjut tanpa `RequiresAdmissionRequest` tidak terdampak.

### 11.5 Amandemen `0.12.0` — Antrean Transfer IGD/Poli (`IGD-DEC-304`, `305`)

**Status: `approved`** — `IGD-DEC-306`, Rizki Gunawan, 14 Oktober 2026. Menggantikan baris `Signed` dan baris *Tandatangani* di
11.1, seluruh 11.2, seluruh 11.3, dan seluruh 11.4.

#### 11.5.1 Baris 11.1 yang berubah

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `AwaitingSignature` | Tandatangani | `Signed`; pasien tampil di Antrean Transfer IGD/Poli | DPJP IGD aktif | Validation §14 aturan 13–16, 27 | `403`/`409`/`503` |
| `Signed` | Batalkan + alasan | `Cancelled` (kejadian `CancelledAfterSend`); pasien keluar dari antrean | Perawat IGD | Belum diproses petugas admisi | `409` aturan 19 |
| `Signed` | Tindak lanjut rawat inap dibatalkan | `Cancelled` (kejadian `AutoCancelled`) | Sistem (akibat pembatalan oleh dokter) | Belum diproses; bila sudah, pembatalan tindak lanjut ditolak | `409` aturan 24 |

Terminal: `Cancelled`. Form `Signed` yang sudah diproses petugas admisi tidak dapat berubah lagi; ia tetap `Signed` dengan
keadaan tampil *Selesai*.

#### 11.5.2 Keadaan tampil (`displayStatus`) — turunan, tidak disimpan

| `status` form | Sudah diproses petugas admisi? | `displayStatus` | Bunyi di layar |
| --- | --- | --- | --- |
| `Draft` | — | `Draft` | Draf |
| `AwaitingSignature` | — | `AwaitingSignature` | Menunggu TTD dr. {DPJP aktif} |
| `Signed` | Belum | `Sent` | Terkirim — pasien di Antrean Transfer IGD/Poli, menunggu petugas admisi |
| `Signed` | Sudah | `Completed` | Selesai — petugas admisi sudah memproses pasien |
| `Cancelled` | — | `Cancelled` | Dibatalkan — {alasan} |

`CancelledByAdmission` tidak dihasilkan lagi; nilainya tetap ada pada enum agar nomor nilai lain tidak bergeser.

#### 11.5.3 `InpAdmissionReferral` — dicabut

Rencana 11.3 tidak dijalankan. Status dan sumber `InpAdmissionReferral` milik Rawat Inap **tidak berubah**.

#### 11.5.4 `EmergencyDispositionStatus` untuk jenis `RequiresAdmissionRequest`

| Dari | Ke | Lewat | Syarat | Bila gagal |
| --- | --- | --- | --- | --- |
| `Confirmed` | `Executed` | `PATCH …/disposition-status` dari IGD | **Tidak sah** | `409` validation §14.8 aturan 23 |
| `Confirmed` | `Executed` | *Admit* Transfer (Rawat Inap, sudah ada — `BE-RWI-207`) | Form *Signed* (aturan 28), kunjungan asal aktif, pasien sama | Kode Rawat Inap yang sudah ada |
| `Confirmed` | `Cancelled` | `PATCH …/disposition-status` dari IGD | Belum diproses petugas admisi; semua form yang belum batal ikut `Cancelled` | `409` aturan 24 |

**Kunjungan IGD.** Tetap ditutup lewat jalur kepergian `MVP-8`: aksi kepergian (catat tiba, terima dokumen) menutup kunjungan
bila tindak lanjutnya sudah *Executed* (`IGD-FACT-121`). Bila pasien dicatat tiba **sebelum** admisi diproses, kunjungan
menunggu penutupan (`IGD-OQ-130`). Jenis tindak lanjut tanpa `RequiresAdmissionRequest` tidak terdampak.

> **Dikoreksi pada `0.15.0` (§11.8, `IGD-DEC-324`).** Kalimat "aksi kepergian (catat tiba, terima dokumen) menutup kunjungan"
> tidak berlaku sampai `0.14.0`: _Catat Tiba_ belum menjadi titik pemicu, dan kunjungan tidak pernah dipromosikan ke
> `Disposed` sesudah _admit_ (`IGD-FACT-128`, `130`). Keduanya ditetapkan §11.8.

### 11.6 Amandemen `0.13.0` — Reaktivasi `CancelledByAdmission` & Penutupan Dua Arah (`IGD-DEC-308`, `309`)

**Status: `approved`** — Rizki Gunawan & Muhammad Hamzah, 15 Oktober 2026 (`IGD-DEC-308`, `IGD-DEC-309`).

#### 11.6.1 Reaktivasi `CancelledByAdmission`
- Sesuai `IGD-DEC-309`, bila form ditolak oleh petugas admisi di Antrean Transfer:
  - Form `EmgAdmissionRequest` berpindah dari `Signed` ke `Cancelled`.
  - Kejadian baru `AdmissionRejected` dicatat pada `EmgAdmissionRequestEvent`.
  - Nilai turunan `displayStatus` menjadi `CancelledByAdmission`.
  - Keterangan di layar: *"Dibatalkan admisi — {alasan penolakan}"*.
  - Disposisi `EmgDisposition` tetap `Confirmed`.
  - Perawat dapat membuat form baru (`Draft`), atau dokter dapat membatalkan tindak lanjut.

#### 11.6.2 Transisi Penutupan Kunjungan IGD Dua Arah
- Sesuai `IGD-DEC-308`:
  - **Alur 1 (Normal: Admisi dulu, antar kemudian):** Kunjungan IGD tertutup saat pasien dicatat tiba di ruangan rawat inap dan serah terima dokumen SBAR selesai (mekanisme `MVP-8`).
  - **Alur 2 (Terbalik: Pasien tiba dulu, admisi kemudian):** Tepat sesudah transaksi *admit* transfer Rawat Inap selesai dan tindak lanjut `EmgDisposition` berpindah ke `Executed`, backend Rawat Inap memanggil `EvaluasiPenutupanKunjunganSusulanAsync` milik IGD. Bila status kedatangan fisik pasien sudah `Arrived` dan dokumen kepergian sudah diterima, kunjungan IGD otomatis berpindah dari `Disposed` ke `Completed`.
- **Diselaraskan pada `0.15.0` (§11.8, `IGD-DEC-324`).** Kedua alur baru berjalan sesudah kunjungan dipromosikan ke `Disposed` (§11.8 baris 1); Alur 1 tertutup saat _Catat Tiba_ (§11.8 baris 2); syarat penutupan pada Alur 2 hanya kedatangan fisik `Arrived` — dokumen yang belum diterima tidak menahan (`IGD-DEC-106`; §11.8 baris 3).

### 11.7 Amandemen `0.14.0` — Transisi Alur Disposisi Non-Ranap & Pemicu Status InTreatment (`IGD-DEC-311`)

**Status: `approved`** — Rizki Gunawan, 16 Oktober 2026 (`IGD-DEC-311`).

#### 11.7.1 Promosi Berurutan (*Sequential Promotion*) Disposisi Non-Ranap
- Matriks Bagian 1 melarang transisi langsung dari `Triaged` (3) atau `InTreatment` (4) menuju `Disposed` (7).
- Pada tindak lanjut non-ranap (Pulang / Kontrol Poli / Rujuk Balik), pasien tidak selalu melalui fase observasi (`UnderObservation`).
- Untuk menjamin keabsahan transisi tanpa melanggar `CanTransition` (menghindari HTTP 409 Conflict):
  1. Saat tindak lanjut dikonfirmasi dokter (`Confirmed`), kunjungan berstatus `Triaged` atau `InTreatment` otomatis bertransisi ke `AwaitingDisposition` (6).
  2. Saat perawat menekan *Jalankan* (`Executed`), backend melakukan *sequential promotion* jaring pengaman: bila kunjungan masih `Triaged` atau `InTreatment`, ia dipromosikan ke `AwaitingDisposition` terlebih dahulu sebelum bertransisi ke `Disposed`.
  3. Setelah berstatus `Disposed`, kunjungan dievaluasi oleh `TryCloseAfterDispositionAsync` dan ditutup otomatis menjadi `Completed` (9) bila seluruh kewajiban klinis selesai.

#### 11.7.2 Pemicu Transisi `Triaged` $\rightarrow$ `InTreatment`
- Status kunjungan berpindah dari `Triaged` (3) ke `InTreatment` (4 / Sedang Ditangani) melalui dua pemicu:
  1. **Pemicu Otomatis:** Penyimpanan pengkajian awal keperawatan (`EmergencyAssessmentInitialTab`) atau catatan SOAP (`EmergencyAssessmentSoapTab`).
  2. **Pemicu Manual:** Tombol *"Mulai Penanganan"* di bilah tindakan atas Ruang Kerja Pemeriksaan IGD.
- Kedua pemicu memanggil endpoint `PATCH /v1/health-services/emergency-installation-management/emergency-visits/{id}/visit-status` dengan `visitStatus = 4`, dan UI melakukan auto-refresh reaktif terhadap status kunjungan.



### 11.8 Amandemen `0.15.0` — Tindak lanjut rawat inap yang dijalankan petugas admisi (`IGD-DEC-324`)

**Status: `approved`** — Rizki Gunawan, 18 Oktober 2026 (`IGD-DEC-324`, menjawab `IGD-OQ-133`). Dasar: `IGD-DEC-323`,
`IGD-FACT-128`, `130`…`132`. Dikerjakan `BE-IGD-083`.

**Latar.** Sejak `IGD-DEC-304`, tindak lanjut rawat inap bertanda `RequiresAdmissionRequest` tidak dijalankan dari IGD,
melainkan menjadi _Executed_ saat petugas admisi meng-_admit_ pasien dari Antrean Transfer IGD/Poli (`BE-RWI-207`, tidak
berubah). Langkah promosi kunjungan milik tombol _Jalankan_ (§11.7.1 butir 2) tidak diambil alih siapa pun, sehingga
kunjungan tertahan `AwaitingDisposition` dan tidak pernah tertutup (Putaran B6, B6-06).

**Bagian 1 tidak diubah:** nol status baru, nol transisi baru. Ketiga baris di bawah hanya menentukan **kapan** transisi
yang sudah sah dijalankan dan **apa** yang menahan penutupan. Ketiganya dirilis bersama.

| # | Baris | Ketentuan | Bila tidak terpenuhi |
| ---: | --- | --- | --- |
| 1 | **Promosi ke `Disposed`** | Bila kunjungan masih berjalan, punya tindak lanjut _Executed_, dan belum `Disposed`, aksi tulis IGD yang mencoba penutupan memindahkannya ke `Disposed` lebih dulu — bertahap seperti §11.7.1 butir 2 (`Triaged`/`InTreatment` → `AwaitingDisposition` → `Disposed`) dan selalu lewat penjaga transisi `BE-IGD-018`. Aksi itu: (a) `EvaluasiPenutupanKunjunganSusulanAsync` yang dipanggil dari transaksi _admit_ Rawat Inap; (b) titik pemicu §9.2; (c) _Selesaikan_ (`PATCH /emergency-visits/{id}/complete`). Pada (a) dan (b) promosi disimpan walaupun penutupan masih tertahan. Pada (c) penolakan `409` tetap berarti tidak ada yang berubah | Penjaga menolak (misalnya `UnderObservation` → `Disposed`): tidak ada yang berubah; alasan dikembalikan — §6 aturan 2 bila masih ada observasi aktif atau dieskalasi, selain itu pesan penjaga |
| 2 | **_Catat Tiba_ menjadi titik pemicu ke-5** | Sesudah status fisik kepergian `Arrived` disimpan, penutupan dicoba seperti titik 3 (§9.2 baris 5) | — (efek samping aksi _Catat Tiba_) |
| 3 | **Penahan "pasien belum tiba"** | Kunjungan yang tindak lanjut _Executed_ terakhirnya bertanda `RequiresAdmissionRequest` baru boleh `Disposed` → `Completed` bila ada kepergian yang status fisiknya `Arrived`. Diperiksa sesudah validation §6 aturan 3 | `409` _"Pasien belum dicatat tiba di unit rawat inap."_ — validation §14.10 aturan 32 |

**Yang tidak berubah.** Jalur _Jalankan_ IGD (§11.7.1) berperilaku sama persis; penahan baris 3 tidak menyentuh jenis
tindak lanjut tanpa `RequiresAdmissionRequest`, dan jenis bertanda tidak dapat dijalankan dari IGD (validation §14.8 aturan
23). Status kunjungan tidak ditulis di jalur baca (daftar, rincian, lencana). Rawat Inap tetap menulis _Executed_ seperti
`BE-RWI-207` dan tidak menulis status kunjungan IGD (`IGD-DEC-323`).

**Akibat yang terlihat di luar transfer.** Kunjungan non-ranap yang punya kepergian kini tertutup saat _Catat Tiba_, tidak
lagi menunggu _Terima Dokumen_ — sejalan dengan `IGD-DEC-106`.

_Contoh alur normal._ Sari meng-_admit_ Tn. Budi pukul 10.40; bila Rawat Inap sudah memanggil fungsi evaluasi, kunjungan
menjadi `Disposed` saat itu tetapi tetap terbuka (baris 3). Dimas mencatat berangkat pukul 10.50; perawat Bangsal Melati
menekan _Terima Dokumen_ pukul 10.58 lalu _Catat Tiba_ pukul 11.00 → kunjungan `Completed` pukul 11.00 (baris 2).

_Contoh alur pasien tiba dulu._ Ny. Ani dicatat tiba di bangsal pukul 10.30 saat tindak lanjutnya masih _Dikonfirmasi_,
jadi tidak ada yang ditutup. _Admit_ selesai pukul 10.45 → fungsi evaluasi mempromosikan lalu menutup kunjungan dalam
penyimpanan _admit_ yang sama.
