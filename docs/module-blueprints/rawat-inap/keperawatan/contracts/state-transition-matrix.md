# State Transition Matrix — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.1.0` |
| `last_changed_in` | `0.1.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.1`; `PRD-RWI-FINAL-001` v1.0.0 |
| Tanggal | 2 September 2026 |

---

## 0. Tiga mesin status, dan satu yang **bukan** milik sub-modul ini

| Mesin status | Milik | Dibahas di sini |
| --- | --- | --- |
| Pengkajian keperawatan | `ClinicalManagement` | Ya |
| Butir rencana asuhan | `ClinicalManagement` | Ya |
| Catatan tindakan keperawatan | `ClinicalManagement` | Ya |
| **Status episode** | `episode-rawat-inap` | **Tidak.** `RWI-DEC-009` mengunci lima nilainya, dan `AC-CAP012-03` melarang menambahnya |

Kosakata ketiga mesin di bawah **tidak beririsan** dengan kosakata status episode. Itulah salah
satu alasan sub-modul ini lolos uji pemecahan.

---

## 1. Pengkajian keperawatan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| *(tidak ada baris)* | Membuat pengkajian | `Draft` | Perawat penanggung jawab, kepala ruangan | Episode berstatus `Admitted` | `422` — episode belum menerima pasien |
| `Draft` | Menyimpan sebagian | `InProgress` | Penulisnya | — | — |
| `Draft` / `InProgress` | Menyelesaikan | `Completed` | Penulisnya, kepala ruangan | Isian wajib terisi | `400` |
| `Draft` / `InProgress` | Membatalkan | `Cancelled` | Penulisnya, kepala ruangan | Alasan wajib diisi | `400` |
| `Completed` | **Mengamandemen** | `Amended` | Penulisnya, kepala ruangan | Alasan wajib; versi lama tersalin | `400` bila alasan kosong |
| `Amended` | Mengamandemen lagi | `Amended` | Sama | Setiap amandemen menambah satu versi | — |

### 1.1 Transisi yang **tidak sah**

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| `Completed` | `Draft` atau `InProgress` | Membuka kembali pengkajian final akan menghapus jejak bahwa ia pernah final. Yang benar adalah amandemen |
| `Cancelled` | Status apa pun | Pembatalan bersifat akhir. Yang dibutuhkan adalah pengkajian baru, bukan menghidupkan yang dibatalkan |
| Apa pun | Terhapus dari database | `CAP-012` aturan 12 melarang hard-delete pengkajian final |
| Apa pun | Status baru mana pun saat episode `Closed` | `INV-KEP-02`. Episode tertutup membuat dokumentasinya hanya-baca |

> **`NotStarted` tidak ada di tabel ini, dan itu disengaja.** "Belum dikaji" berarti tidak ada
> barisnya sama sekali. Lihat `02-backend-architecture.md` bagian 9.

---

## 2. Butir rencana asuhan keperawatan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| *(tidak ada baris)* | Menambah masalah keperawatan | `Active` | Perawat penanggung jawab, kepala ruangan | Episode `Admitted` | `422` |
| `Active` | Memperbarui tujuan atau rencana | `Active` | Sama | **Versi lama tersalin lebih dulu** | `409` bila penyalinan gagal |
| `Active` | Mencatat evaluasi | `Active` | Sama | — | — |
| `Active` | Menyatakan tercapai | `Resolved` | Sama | Sekurang-kurangnya satu evaluasi tercatat | `400` |
| `Active` | Menutup karena tidak lagi relevan | `Discontinued` | Sama | Alasan wajib | `400` |
| `Resolved` / `Discontinued` | Membuka kembali | `Active` | Kepala ruangan | Alasan wajib | `403` bila bukan kepala ruangan |

### 2.1 Transisi yang **tidak sah**

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| Apa pun | Terhapus | `CAP-013` aturan 6: menutup butir tidak boleh menghapus tindakan dan evaluasi sebelumnya |
| `Active` | `Resolved` tanpa evaluasi | Menyatakan masalah teratasi tanpa satu pun evaluasi membuat rekam medis tidak dapat menunjukkan dasarnya |

---

## 3. Catatan tindakan keperawatan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| *(tidak ada baris)* | Mencatat tindakan | `Recorded` | Perawat mana pun yang bertugas di unit | Episode `Admitted`; waktu tindakan tidak di masa depan | `400` / `422` |
| `Recorded` | Menyunting | `Recorded` | **Penulisnya saja** | Belum final | `403` bila bukan penulisnya |
| `Recorded` | Menyatakan final | `Finalized` | Penulisnya, kepala ruangan | — | — |
| `Finalized` | Mengamandemen | `Amended` | Penulisnya, kepala ruangan | Alasan wajib; isi lama tersalin | `403` bagi selain keduanya — `AC-CAP014-03` |

### 3.1 Status pengiriman tagihan — mesin **terpisah**

`AC-CAP014-02` menuntut catatan klinis tetap tersimpan walaupun pengiriman ke Billing gagal.
Karena itu status tagihan **bukan** status catatan.

| Dari | Tindakan | Ke | Keterangan |
| --- | --- | --- | --- |
| `NotApplicable` | — | — | Tindakan yang tidak dapat ditagih |
| `Pending` | Kirim ke Billing | `Dispatched` | Memakai kunci idempotency |
| `Pending` | Pengiriman gagal | `Failed` | **Catatan klinisnya tetap `Recorded`/`Finalized`** |
| `Failed` | Coba lagi | `Dispatched` atau `Failed` | Percobaan ulang tidak mengubah catatan klinis |

> **Ini pemisahan yang paling penting pada dokumen ini.** Kegagalan sistem tagihan **tidak boleh**
> membuat tindakan yang benar-benar dilakukan perawat hilang dari rekam medis.
