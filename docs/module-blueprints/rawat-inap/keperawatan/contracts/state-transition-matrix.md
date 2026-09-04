# State Transition Matrix — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Compatibility impact | `0.3.0`: status **`Amended` dicabut** dari mesin pengkajian dan mesin catatan tindakan. Koreksi kini dipegang mesin addendum `MedicalRecordManagement`, sejalan `RWI-DEC-091`. Mesin butir rencana asuhan **tidak berubah**. Nol nilai status baru, nol enum baru |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.3`; `PRD-RWI-FINAL-001` v1.0.0; decision log `13` |
| Keputusan yang mengikat | `RWI-DEC-091` (koreksi dibedakan dari perkembangan), `RWI-DEC-086`, `RWI-DEC-087`, `RWI-FACT-016` |
| Tanggal | 2 September 2026 |

---

## 0. Tiga mesin status, dan satu yang **bukan** milik sub-modul ini

| Mesin status | Milik | Dibahas di sini |
| --- | --- | --- |
| Pengkajian keperawatan | `ClinicalManagement` | Ya |
| Butir rencana asuhan | `ClinicalManagement` | Ya |
| Catatan tindakan keperawatan | `ClinicalManagement` | Ya |
| **Status episode** | `episode-rawat-inap` | **Tidak.** `RWI-DEC-009` mengunci lima nilainya, dan `AC-CAP012-03` melarang menambahnya |
| **Keutuhan dan koreksi dokumen** | **`MedicalRecordManagement`** | **Tidak.** Mesinnya sudah ada dan dipakai apa adanya sejak `RWI-DEC-091`; sub-modul ini **tidak** membuat mesin koreksi tandingan |

Kosakata ketiga mesin di bawah **tidak beririsan** dengan kosakata status episode. Itulah salah
satu alasan sub-modul ini lolos uji pemecahan.

> **Perubahan terbesar `0.3.0`.** Sampai `0.2.0`, dua dari tiga mesin di bawah punya status `Amended`
> sendiri beserta salinan versi. `RWI-DEC-091` mencabutnya: **koreksi** dokumen keperawatan kini memakai
> mesin addendum milik `MedicalRecordManagement`, sama seperti dokumen dokter, sehingga satu lembar
> catatan terpadu tidak memuat dua bentuk koreksi.
>
> Yang **tidak** ikut berubah adalah butir rencana asuhan. Perubahan rencana asuhan bukan pembetulan
> kesalahan melainkan perkembangan klinis — `PRD-RWI-FINAL-001` `CAP-013` aturan 5 menyebutnya
> "diperbarui berdasarkan Reassessment dengan history". Menyeretnya ke mesin addendum akan mengaburkan
> perbedaan antara *pasien membaik* dan *perawat salah tulis*.

---

## 1. Pengkajian keperawatan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| *(tidak ada baris)* | Membuat pengkajian | `Draft` | Perawat penanggung jawab, kepala ruangan | Episode berstatus `Admitted` | `422` — episode belum menerima pasien |
| `Draft` | Menyimpan sebagian | `InProgress` | Penulisnya | — | — |
| `Draft` / `InProgress` | Menyelesaikan | `Completed` | Penulisnya, kepala ruangan | Isian wajib terisi | `400` |
| `Draft` / `InProgress` | Membatalkan | `Cancelled` | Penulisnya, kepala ruangan | Alasan wajib diisi | `400` |
| `Completed` | **Mengoreksi** | **Tetap `Completed`** | Penulisnya, kepala ruangan | Alasan koreksi wajib; **addendum bernomor urut** tersimpan pada mesin keutuhan; isi asli **tidak berubah sedikit pun** | `400` bila alasan kosong; `403` bagi selain keduanya |
| `Completed` | Mengoreksi lagi | **Tetap `Completed`** | Sama | Setiap koreksi menambah satu addendum bernomor, bukan satu versi dokumen | — |

### 1.1 Transisi yang **tidak sah**

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| `Completed` | `Draft` atau `InProgress` | Membuka kembali pengkajian final akan menghapus jejak bahwa ia pernah final. Yang benar adalah **koreksi lewat addendum** |
| `Completed` | `Amended` | **Status `Amended` sudah tidak ada sejak `0.3.0`.** Menambahkannya kembali membuat dua sumber jawaban atas pertanyaan "apakah dokumen ini pernah dikoreksi": status dokumen dan riwayat addendum. Yang berlaku adalah riwayat addendum |
| `Draft` / `InProgress` | Menerima addendum | Dokumen yang belum final **ditolak** mesin koreksi, dengan arahan membetulkan langsung pada isinya — `RWI-FACT-013` |
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

> **Mesin ini sengaja tidak diubah `0.3.0`.** Baris "Memperbarui tujuan atau rencana" tetap menyalin versi lama,
> dan itu **bukan** koreksi kesalahan. Rencana asuhan memang berubah ketika keadaan pasien berubah, dan
> `AC-CAP013-02` menuntut versi sebelumnya tetap menyimpan **penulis dan waktu aslinya** — bukan penulis
> yang mengubah. Menggantinya dengan addendum akan menghilangkan justru sifat itu.

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
| `Recorded` | Menyatakan final | `Finalized` | Penulisnya, kepala ruangan | **Perpindahan ini sekaligus mendaftarkan catatan ke mesin keutuhan sebagai tertanda tangan, dalam transaksi yang sama** | Bila pendaftaran gagal, finalisasi ikut batal |
| `Finalized` | **Mengoreksi** | **Tetap `Finalized`** | Penulisnya, kepala ruangan | Alasan koreksi wajib; addendum bernomor urut tersimpan; isi asli tidak berubah | `403` bagi selain keduanya — `AC-CAP014-03` |

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

---

## 4. Satu syarat teknis yang wajib dibaca sebelum dibangun

`RWI-DEC-091` memakai mesin keutuhan dokumen milik `MedicalRecordManagement`. Pembacaan source
2026-09-02 (`RWI-FACT-016`) menemukan mesinnya **sudah ada, sudah dipakai, dan sudah menyediakan
nomor** bagi jenis dokumen yang dibutuhkan — tetapi **belum menegakkannya**.

| Hal | Keadaannya |
| --- | --- |
| Nilai enum yang dibutuhkan | `Assessment` untuk pengkajian, `Procedure` untuk catatan tindakan. **Keduanya sudah ada** pada `ClinicalDocumentKind`; **nol nilai enum baru** |
| Pendaftaran | `RegisterAsync` **tidak** menyaring jenis dokumen, sehingga pendaftaran pengkajian dan tindakan sudah dapat dilakukan hari ini |
| Penegakan | `EnsureMutableAsync` **membiarkan lewat** jenis yang belum ditegakkan. Daftar yang ditegakkan hari ini hanya berisi `ProgressNote`, sesuai `RM-DEC-019` |
| Akibatnya bila dibangun apa adanya | Pengkajian dan tindakan **terdaftar** tetapi **tidak terkunci**. Dokumen final masih dapat disunting, dan seluruh mesin status di atas kehilangan penjaganya |
| Yang diminta | Menambahkan `Assessment` dan `Procedure` ke daftar jenis yang ditegakkan — perubahan kecil, tetapi **milik `MedicalRecordManagement`** dan diatur `RM-DEC-019` |
| Statusnya | **`RWI-OQ-051`, terbuka.** Tidak memblokir desain; **memblokir implementasi** `BE-RWI-057` dan `BE-RWI-062` |

> **Jangan menyiasatinya dengan penjaga sendiri.** Membuat pemeriksaan penguncian di dalam
> `ClinicalManagement` akan melahirkan mesin koreksi tandingan — persis yang dilarang
> `RWI-DEC-087` dan yang baru saja dihindari `RWI-DEC-091`. Bila `RWI-OQ-051` ditolak, yang benar
> adalah kembali ke `/qv-grill`, bukan membangun penjaga kedua.

> **Catatan untuk pembaca `RM-FE-009`.** Selama jenis dokumen keperawatan belum ditegakkan, keadaan
> itu **wajib dinyatakan terbuka di layar**, bukan didiamkan — aturan itu berasal dari
> `MedicalRecordManagement` sendiri dan berlaku sama bagi sub-modul ini.
