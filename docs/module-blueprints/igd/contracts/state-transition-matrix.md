# Matriks Transisi Status — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.4.0` — revisi 6. **Aditif**: bagian 6a murni baru, nol bagian lama diubah |
| Status | `draft`, **kecuali bagian 1, 1.1, dan 1.2 yang `approved`** |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | **Rizki Gunawan / 2026-08-24** — terbatas pada bagian 1, 1.1, 1.2 (`EmergencyVisitStatus`) lewat `IGD-DEC-093`. Bagian 2 sampai 7 tetap `draft` |
| Versi sebelumnya | `0.3.0`, sebelumnya `0.2.0` |

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
