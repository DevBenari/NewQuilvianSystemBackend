# Validation Matrix — Sub-modul `keperawatan` (Rawat Inap)

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

Pesan ditulis dalam bahasa yang dipahami pengguna, bukan istilah teknis.

---

## 1. Kelayakan konteks — penjaga `INV-KEP-01`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-01` | Membuat pengkajian, rencana asuhan, tindakan | Encounter tidak punya episode rawat inap | "Pasien ini tidak sedang dirawat inap. Pengkajian rawat inap hanya untuk pasien yang sudah masuk kamar." | `422` |
| `VAL-KEP-02` | Sama | Episode ada tetapi masih `Draft` | "Pasien belum dikonfirmasi tiba di kamar. Catatan keperawatan dapat dibuat setelah pasien benar-benar masuk." | `422` |
| `VAL-KEP-03` | Sama | Episode `Closed` atau `Cancelled` | "Perawatan pasien ini sudah ditutup. Catatannya hanya dapat dibaca." | `422` |
| `VAL-KEP-04` | Membuat pengkajian tanpa antrean | Encounter bertipe rawat jalan atau medical check-up **tanpa** episode rawat inap | "Pengkajian untuk pasien poliklinik tetap harus lewat antrean." | `400` |

> `VAL-KEP-04` menjaga janji `RWI-DEC-070`: pelonggaran hanya untuk rawat inap dan IGD; rawat
> jalan dan medical check-up tidak boleh berubah sedikit pun.

---

## 2. Kewenangan perawat

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-05` | Menulis dokumentasi | Pengguna bukan perawat penanggung jawab episode dan bukan kepala ruangan | "Anda bukan perawat penanggung jawab pasien ini. Hubungi kepala ruangan bila perlu mencatat." | `403` |
| `VAL-KEP-06` | Menyunting catatan tindakan | Pengguna bukan penulis catatan dan catatannya belum final | "Catatan ini ditulis petugas lain. Anda tidak dapat mengubahnya." | `403` |
| `VAL-KEP-07` | Mengamandemen catatan final | Pengguna bukan penulis dan bukan kepala ruangan | "Catatan yang sudah final hanya dapat diubah penulisnya atau kepala ruangan, dan perubahannya tercatat." | `403` |

> **Episode tanpa perawat penanggung jawab tidak menahan pencatatan.** `RWI-DEC-047` menyatakan
> ketiadaan perawat hanya memunculkan baris pada daftar pantau. Bila `InpNurseAssignment` kosong,
> `VAL-KEP-05` jatuh ke kewenangan unit: perawat yang bertugas di unit layanan episode itu
> diizinkan. Menahan pencatatan karena penugasan belum diisi akan menghentikan pekerjaan nyata
> demi kelengkapan administrasi.

---

## 3. Isi pengkajian

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-08` | Menyelesaikan pengkajian | Isian wajib menurut kebijakan aktif belum terisi | "Pengkajian belum dapat diselesaikan. Bagian berikut masih kosong: {daftar}." | `400` |
| `VAL-KEP-09` | Skor risiko jatuh | Skor terisi tetapi kategorinya tidak | "Kategori risiko jatuh belum dipilih." | `400` |
| `VAL-KEP-10` | Skrining gizi | Hasil skrining berisiko tinggi tetapi rujukan gizi tidak dibuat | **Peringatan, bukan penolakan.** "Hasil skrining menunjukkan risiko gizi. Rujukan ke Gizi disarankan." | `200` |
| `VAL-KEP-11` | Pengkajian awal kedua | Sudah ada pengkajian awal aktif pada episode yang sama | "Pengkajian awal untuk pasien ini sudah ada. Gunakan pengkajian ulang." | `409` |
| `VAL-KEP-12` | Amandemen | Alasan kosong | "Alasan perubahan wajib diisi." | `400` |

> `VAL-KEP-10` sengaja **tidak** menolak. Modul Gizi berstatus `PLANNED`; menolak penyelesaian
> pengkajian karena rujukan tidak dapat dibuat akan menahan pekerjaan perawat karena modul yang
> belum ada.

---

## 4. Tindakan keperawatan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-13` | Mencatat tindakan | Waktu tindakan di masa depan | "Waktu tindakan tidak boleh melewati waktu sekarang." | `400` |
| `VAL-KEP-14` | Mencatat tindakan | Waktu tindakan sebelum pasien masuk kamar | "Waktu tindakan sebelum pasien masuk kamar. Periksa kembali waktunya." | `400` |
| `VAL-KEP-15` | Mencatat tindakan | Kunci idempotency sama dengan yang sudah tersimpan | **Bukan galat.** Mengembalikan catatan yang sudah ada, kode `200` | `200` |
| `VAL-KEP-16` | Menutup butir asuhan sebagai tercapai | Belum ada satu pun evaluasi | "Butir ini belum punya catatan evaluasi, sehingga belum dapat dinyatakan tercapai." | `400` |

---

## 5. Keterlambatan — memantau, bukan menolak

| Aturan | Berlaku pada | Kondisi | Perilaku |
| --- | --- | --- | --- |
| `VAL-KEP-17` | Pemantauan tenggat | `MstClinicalAssessmentPolicy` kosong | **Tidak ada yang dinyatakan terlambat.** `DueAt` tidak terisi; pencatatan tetap berjalan penuh |
| `VAL-KEP-18` | Pemantauan tenggat | Pengkajian lewat tenggat kebijakan aktif | Muncul pada daftar pantau. **Tidak menahan tindakan apa pun** |

> Keterlambatan pengkajian **tidak pernah** menjadi gerbang. `INV-KEP-03` melarangnya, dan PRD
> 16.3 menyatakan dokter tidak perlu menunggu pengkajian selesai.
