# State Transition Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.1.0` |
| `last_changed_in` | `0.1.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement` (`RWI-DEC-081`, PRD 23.1) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.1`; `PRD-RWI-FINAL-001` v1.0.0 |
| Tanggal | 2 September 2026 |

---

## 0. Mesin status yang dibahas, dan yang **bukan** milik sub-modul ini

| Mesin | Milik | Dibahas di sini |
| --- | --- | --- |
| Konsultasi dan SOAP | `ClinicalManagement` | Ya |
| Kajian medis | `ClinicalManagement` | Ya |
| Verifikasi CPPT | `ClinicalManagement` | Ya |
| Catatan tindakan dokter | `ClinicalManagement` | Ya |
| Pemenuhan resep | **`PharmacyManagement`** | **Dibaca saja** — `INV-DOK-04` |
| Pesanan laboratorium | **`LaboratoryManagement`** | **Dibaca saja** — `INV-DOK-05` |
| Status episode | `episode-rawat-inap` | **Tidak** — `RWI-DEC-009` mengunci lima nilainya |

---

## 1. Konsultasi dan SOAP

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| *(tidak ada baris)* | Membuat konsultasi | `Draft` | DPJP, dokter jaga berwenang | Episode `Admitted` | `422` |
| `Draft` | Mengisi S/O/A/P | `InProgress` | Penulisnya | — | — |
| `Draft` / `InProgress` | Menyelesaikan | `Completed` | Penulisnya | Isian wajib terisi | `400` |
| `Draft` / `InProgress` | Membatalkan | `Cancelled` | Penulisnya, supervisor klinis | Alasan wajib | `400` |
| `Completed` | Mengamandemen | `Amended` | Penulisnya | Alasan wajib; versi lama tersalin | `403` bagi selain penulis |

### 1.1 Transisi yang **tidak sah**

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| `Completed` | `Draft` / `InProgress` | Membuka kembali catatan final menghapus jejak bahwa ia pernah final |
| Apa pun | Terhapus | PRD `CAP-020` aturan 5 melarang hard-delete |
| Apa pun | Status baru saat episode `Closed` | `INV-DOK-02`. **Kecuali** amandemen, yang `AC-CAP020-03` izinkan justru karena ia **tidak** mengaktifkan kembali episode |

> **Baris terakhir adalah pembeda halus yang penting.** Episode tertutup menolak catatan **baru**,
> tetapi menerima **amandemen** catatan lama. Menyamakan keduanya akan membuat kesalahan tulis pada
> episode yang sudah ditutup tidak pernah dapat dibetulkan.

---

## 2. Kajian medis

Memakai mesin status yang sama dengan pengkajian keperawatan — `Draft`, `InProgress`, `Completed`,
`Cancelled`, `Amended` — karena tabelnya sama. Pembedanya `AssessmentType` dan siapa yang boleh
menulisnya.

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| *(tidak ada baris)* | Membuat kajian medis | `Draft` | **Dokter saja** | Episode `Admitted` |
| `Completed` | Mengamandemen | `Amended` | Penulisnya | Alasan wajib; `AC-CAP022-03` |

| Transisi tidak sah | Kenapa |
| --- | --- |
| Kajian medis ditimpa oleh SOAP harian | PRD `CAP-022` aturan 3 melarangnya tegas. Keduanya tabel berbeda, jadi penimpaan **tidak mungkin terjadi secara struktur** |
| Perawat membuat kajian medis | `VAL-DOK-05` |

---

## 3. Verifikasi CPPT

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `NotRequired` | — | — | — | Kebijakan verifikasi tidak aktif | — |
| `Pending` | DPJP memverifikasi | `Verified` | **DPJP episode itu** | Bukan penulis aslinya bila kebijakan menuntut demikian | `403` |
| `Pending` | Lewat batas waktu | `Overdue` | *(sistem)* | Kebijakan aktif punya batas | — |
| `Overdue` | DPJP memverifikasi | `Verified` | DPJP | — | — |
| `Verified` | Mengamandemen catatan | `Pending` | Penulis asli | Alasan wajib; verifikasi ulang dibutuhkan | `400` bila alasan kosong |

### 3.1 Aturan yang tidak boleh dilanggar

| Aturan | Sumbernya |
| --- | --- |
| **Verifikasi tidak pernah mengubah penulis asli.** `ProviderUserId` tetap; `VerifiedByUserId` terpisah | PRD `CAP-021` aturan 5, `AC-CAP021-03` |
| Bawaan `NotRequired`, bukan `Pending` | Menyalakan kewajiban verifikasi sebagai bawaan membuat daftar pantau penuh pada rumah sakit yang tidak mewajibkannya |
| `Overdue` **tidak menahan** penulisan catatan berikutnya | Verifikasi adalah pemantauan mutu, bukan gerbang pelayanan |

---

## 4. Catatan tindakan dokter

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| *(tidak ada baris)* | Mencatat rencana tindakan | `Ordered` | Dokter berwenang | Episode `Admitted` |
| *(tidak ada baris)* | Mencatat tindakan yang sudah dilakukan | `Performed` | Dokter pelaksana | Episode `Admitted`; waktu tidak di masa depan |
| `Ordered` | Melaksanakan | `Performed` | Dokter pelaksana | — |
| `Ordered` | Membatalkan | `Cancelled` | Dokter perencana, supervisor | Alasan wajib |
| `Performed` | Mengamandemen | `Amended` | Pelaksananya | Alasan wajib |

### 4.1 Status pengiriman tagihan — mesin **terpisah**

Sama persis dengan `keperawatan`, dan memakai enum yang sama.

| Dari | Tindakan | Ke | Keterangan |
| --- | --- | --- | --- |
| `Pending` | Kirim ke Billing | `Dispatched` | Memakai kunci idempotency |
| `Pending` | Gagal | `Failed` | **Catatan klinisnya tetap `Performed`** — PRD `CAP-024` aturan 5 |
| `Failed` | Coba lagi | `Dispatched` / `Failed` | Tidak menyentuh catatan klinis |

---

## 5. Status milik modul lain yang **hanya dibaca**

| Mesin | Nilai yang dibaca | Yang **tidak boleh** dilakukan sub-modul ini |
| --- | --- | --- |
| Pemenuhan resep | `WaitingForClinicalFinalization` dan seterusnya, milik `PharmacyManagement` | **Menulis** status apa pun. `INV-DOK-04` |
| Pesanan laboratorium | `Requested`, `OnHold`, dan seterusnya | Menulis status maupun hasil. `INV-DOK-05` |

Menampilkannya di layar adalah membaca. Mengubahnya adalah pelanggaran batas.
