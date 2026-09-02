# Validation Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

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

Pesan ditulis dalam bahasa yang dipahami pengguna, bukan istilah teknis.

---

## 1. Kelayakan konteks — penjaga `INV-DOK-01`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-01` | Konsultasi, kajian medis, CPPT, tindakan, resep, pesanan lab | Encounter tidak punya episode rawat inap | "Pasien ini tidak sedang dirawat inap." | `422` |
| `VAL-DOK-02` | Sama | Episode masih `Draft` | "Pasien belum dikonfirmasi tiba di kamar." | `422` |
| `VAL-DOK-03` | Catatan **baru** | Episode `Closed` atau `Cancelled` | "Perawatan pasien ini sudah ditutup. Catatan baru tidak dapat dibuat; koreksi catatan lama tetap bisa." | `422` |
| `VAL-DOK-04` | Konsultasi tanpa antrean | Encounter rawat jalan atau medical check-up tanpa episode | "Konsultasi untuk pasien poliklinik tetap harus lewat antrean." | `400` |

> `VAL-DOK-04` menjaga janji `RWI-DEC-070` aturan 6: rawat jalan dan medical check-up tidak boleh
> berubah sedikit pun.
>
> `VAL-DOK-03` **sengaja** hanya menolak catatan baru. Amandemen catatan lama tetap diterima —
> `AC-CAP020-03`.

---

## 2. Kewenangan dokter

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-05` | Kajian medis, SOAP, tindakan, resep | Pengguna bukan dokter | "Catatan ini hanya dapat ditulis dokter." | `403` |
| `VAL-DOK-06` | Menulis pada episode | Dokter bukan DPJP episode itu dan bukan dokter jaga yang berwenang | "Anda bukan DPJP pasien ini. Hubungi DPJP atau supervisor klinis." | `403` |
| `VAL-DOK-07` | Verifikasi CPPT | Pengguna bukan DPJP episode itu | "Verifikasi hanya dapat dilakukan DPJP pasien ini." | `403` |
| `VAL-DOK-08` | Mencatat visite | Pengguna tidak punya kewenangan dokter | "Visite hanya dapat dicatat dokter." | `403` |
| `VAL-DOK-09` | Amandemen catatan final | Pengguna bukan penulis aslinya | "Catatan final hanya dapat diubah penulisnya." | `403` |

> **`VAL-DOK-08` mengikuti bawaan yang aman.** PRD `CAP-025` aturan 4 membuka kemungkinan
> *administrative attestation policy*, tetapi kebijakan itu belum ada. Sampai ada, hanya dokter
> yang dapat mencatat visite. Membuka lebih dulu berarti mengizinkan visite dicatat atas nama
> dokter tanpa dasar tertulis.

---

## 3. Isi kajian medis dan SOAP

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-10` | Menyelesaikan kajian medis | Keluhan utama, pemeriksaan, atau rencana kosong | "Kajian medis belum dapat diselesaikan. Bagian berikut masih kosong: {daftar}." | `400` |
| `VAL-DOK-11` | Menyelesaikan kajian medis | Daftar masalah atau diagnosis kosong | "Diagnosis atau daftar masalah belum diisi." | `400` |
| `VAL-DOK-12` | Menyelesaikan SOAP | Keempat bagian S/O/A/P kosong seluruhnya | "Catatan SOAP masih kosong." | `400` |
| `VAL-DOK-13` | Waktu klinis | Waktu klinis di masa depan | "Waktu pemeriksaan tidak boleh melewati waktu sekarang." | `400` |
| `VAL-DOK-14` | Waktu klinis | Waktu klinis sebelum pasien masuk kamar | "Waktu pemeriksaan sebelum pasien masuk kamar. Periksa kembali." | `400` |
| `VAL-DOK-15` | Amandemen apa pun | Alasan kosong | "Alasan perubahan wajib diisi." | `400` |

> **`VAL-DOK-12` sengaja longgar: cukup satu bagian terisi.** Menuntut keempat bagian terisi pada
> setiap catatan harian akan membuat dokter menulis kalimat kosong demi lolos validasi, dan itu
> menurunkan mutu rekam medis, bukan menaikkannya.

---

## 4. Visite

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-16` | Mencatat visite | Waktu visite di masa depan | "Waktu visite tidak boleh melewati waktu sekarang." | `400` |
| `VAL-DOK-17` | Mencatat visite | Kunci idempotency sama dengan yang sudah tersimpan | **Bukan galat.** Mengembalikan visite yang sudah ada | `200` |
| `VAL-DOK-18` | Mencatat visite | Sudah ada visite dokter yang sama pada jam yang berdekatan | **Peringatan, bukan penolakan.** "Sudah ada visite Anda hari ini pukul {jam}. Lanjutkan bila memang visite kedua." | `200` |

> **`VAL-DOK-18` memperingatkan, tidak menolak.** Dokter yang benar-benar datang dua kali sehari
> adalah kejadian nyata; menolaknya memaksa petugas berbohong atau melewatkan catatan.

---

## 5. Resep dan penunjang

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-19` | Membuat resep | Kunci idempotency sama | Mengembalikan resep yang sudah ada | `200` |
| `VAL-DOK-20` | Membuat resep obat pulang | Episode belum berstatus menunggu pulang | **Peringatan, bukan penolakan.** "Pasien belum dinyatakan boleh pulang. Obat pulang tetap dapat disiapkan." | `200` |
| `VAL-DOK-21` | Menandai obat sudah diserahkan | Percobaan apa pun dari sub-modul ini | "Status penyerahan obat hanya dapat diubah petugas Farmasi." | `403` |
| `VAL-DOK-22` | Pesanan lab | `InpEpisodeId` tidak cocok dengan `EncounterId` | "Pesanan ini tidak cocok dengan perawatan pasien." — `AC-CAP015-01` | `400` |
| `VAL-DOK-23` | Menulis hasil lab | Percobaan apa pun dari sub-modul ini | "Hasil pemeriksaan hanya dapat diisi petugas Laboratorium." | `403` |

`VAL-DOK-21` dan `VAL-DOK-23` menjaga `INV-DOK-04` dan `INV-DOK-05` di tingkat aturan, bukan hanya
di tingkat niat.

---

## 6. Verifikasi CPPT — memantau, bukan menolak

| Aturan | Kondisi | Perilaku |
| --- | --- | --- |
| `VAL-DOK-24` | Kebijakan verifikasi belum ditetapkan | Seluruh catatan `NotRequired`. Daftar pantau kosong; pencatatan berjalan penuh |
| `VAL-DOK-25` | Catatan lewat batas verifikasi | Muncul pada daftar pantau. **Tidak menahan** penulisan catatan berikutnya |
