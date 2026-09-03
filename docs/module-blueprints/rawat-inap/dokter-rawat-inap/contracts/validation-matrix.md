# Validation Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.2`; arsitektur domain `0.2` |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Compatibility impact | `0.3.0`: empat aturan baru `VAL-DOK-32` s.d. `VAL-DOK-35` untuk koreksi dokumen. Sebelumnya `0.2.0` menambah `VAL-DOK-26` s.d. `VAL-DOK-31`. Nol aturan dicabut |
| Tanggal | 2 September 2026 |

Pesan ditulis dalam bahasa yang dipahami pengguna, bukan istilah teknis.

---

## 1. Kelayakan konteks — penjaga `INV-DOK-01` s.d. `INV-DOK-03`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-01` | Catatan dokter, kajian medis, CPPT, tindakan, resep, pesanan lab dan radiologi, event visite | Kunjungan tidak punya episode rawat inap | "Pasien ini tidak sedang dirawat inap." | `422` |
| `VAL-DOK-02` | Sama | Episode masih `Draft` | "Pasien belum dikonfirmasi tiba di kamar." | `422` |
| `VAL-DOK-03` | Dokumen **baru** | Episode `Closed` atau `Cancelled` | "Perawatan pasien ini sudah ditutup. Catatan baru tidak dapat dibuat; koreksi catatan lama tetap bisa." | `422` |
| `VAL-DOK-04` | Catatan dokter tanpa antrean | Kunjungan rawat jalan atau medical check-up tanpa episode | "Konsultasi untuk pasien poliklinik tetap harus lewat antrean." | `400` |
| `VAL-DOK-26` ★ | Seluruh dokumen yang membawa penanda episode | Penanda episode terisi tetapi **tidak cocok** dengan episode milik kunjungan itu | "Catatan ini tidak cocok dengan perawatan pasien. Periksa kembali pasien yang sedang Anda buka." | `400` |

> `VAL-DOK-04` menjaga janji `RWI-DEC-070` aturan 6 dan diuji `RWI-AC-143`: rawat jalan dan medical
> check-up tidak boleh berubah sedikit pun.
>
> `VAL-DOK-03` **sengaja** hanya menolak dokumen baru. Koreksi lewat addendum tetap diterima.
>
> `VAL-DOK-26` **baru pada `0.2.0`.** Ia menjaga `INV-DOK-01` dan `INV-DOK-02` ketika penanda
> episode dan kunjungan saling bertentangan — keadaan yang justru paling berbahaya karena kedua
> nilainya masuk akal bila dilihat sendiri-sendiri.

---

## 2. Kewenangan dokter — penjaga `INV-DOK-13`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-05` | Kajian medis, catatan dokter, tindakan, resep | Pengguna bukan dokter | "Catatan ini hanya dapat ditulis dokter." | `403` |
| `VAL-DOK-06` | Menulis pada episode | Dokter bukan penanggung jawab pasien itu dan bukan dokter jaga yang berwenang | "Anda bukan DPJP pasien ini. Hubungi DPJP atau supervisor klinis." | `403` |
| `VAL-DOK-07` | Verifikasi CPPT | Pengguna bukan DPJP yang aktif saat itu | "Verifikasi hanya dapat dilakukan DPJP pasien ini." | `403` |
| `VAL-DOK-08` | Mencatat visite | Pengguna tidak punya kewenangan dokter | "Visite hanya dapat dicatat dokter." | `403` |
| `VAL-DOK-09` | Koreksi dokumen final | Pengguna bukan penulis aslinya dan tidak punya pendelegasian yang sah | "Catatan final hanya dapat dikoreksi penulisnya atau penulis pengganti yang ditunjuk." | `403` |

> **`VAL-DOK-08` mengikuti bawaan yang aman.** `RWI-RULE-017` current menyatakan pencatatan visite
> atas nama dokter oleh petugas administrasi **tidak tersedia** sampai ada kebijakan eksplisit.
> Membukanya lebih dulu berarti mengizinkan visite dicatat atas nama dokter tanpa dasar tertulis.

---

## 3. Isi kajian medis dan catatan dokter

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-10` | Menyelesaikan kajian medis | Keluhan utama, pemeriksaan, atau rencana kosong | "Kajian medis belum dapat diselesaikan. Bagian berikut masih kosong: {daftar}." | `400` |
| `VAL-DOK-11` | Menyelesaikan kajian medis | Daftar masalah atau diagnosis kosong | "Diagnosis atau daftar masalah belum diisi." | `400` |
| `VAL-DOK-12` | Menyelesaikan catatan dokter | Keempat bagian S/O/A/P kosong seluruhnya | "Catatan masih kosong." | `400` |
| `VAL-DOK-13` | Waktu klinis | Waktu klinis melewati waktu sekarang | "Waktu pemeriksaan tidak boleh melewati waktu sekarang." | `400` |
| `VAL-DOK-14` | Waktu klinis | Waktu klinis sebelum pasien masuk kamar | "Waktu pemeriksaan sebelum pasien masuk kamar. Periksa kembali." | `400` |
| `VAL-DOK-15` | Koreksi apa pun | Alasan kosong | "Alasan perubahan wajib diisi." | `400` |

> **`VAL-DOK-12` sengaja longgar: cukup satu bagian terisi.** Menuntut keempat bagian terisi pada
> setiap catatan harian akan membuat dokter menulis kalimat kosong demi lolos validasi, dan itu
> menurunkan mutu rekam medis, bukan menaikkannya.
>
> **Contoh `VAL-DOK-14`.** Tn. Budi masuk kamar pukul 10.40 tanggal 1 September. Catatan dengan
> waktu klinis 1 September pukul 08.00 ditolak, karena pada jam itu ia belum berada di kamar.
> Catatan dengan waktu 1 September pukul 11.00 diterima.

---

## 4. Event visite

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-16` | Mencatat visite | Waktu visite melewati waktu sekarang | "Waktu visite tidak boleh melewati waktu sekarang." | `400` |
| `VAL-DOK-17` | Mencatat visite | Kunci permintaan sama dengan yang sudah tersimpan | **Bukan galat.** Mengembalikan event yang sudah ada | `200` |
| `VAL-DOK-18` | Mencatat visite | Sudah ada visite dokter yang sama pada jam berdekatan | **Peringatan, bukan penolakan.** "Sudah ada visite Anda hari ini pukul {jam}. Lanjutkan bila memang visite kedua." | `200` |
| `VAL-DOK-27` ★ | Mencatat visite | Kunci permintaan **kosong** | "Permintaan tidak lengkap. Muat ulang halaman lalu coba lagi." | `400` |
| `VAL-DOK-28` ★ | Membatalkan visite | Alasan pembatalan kosong | "Alasan pembatalan wajib diisi." | `400` |
| `VAL-DOK-29` ★ | Membatalkan visite | Event sudah berstatus batal | "Visite ini sudah dibatalkan sebelumnya." | `409` |

> **`VAL-DOK-18` memperingatkan, tidak menolak — dan `RWI-DEC-085` menegaskannya.** Dokter yang
> benar-benar datang dua kali sehari adalah kejadian nyata, dan keduanya **wajib** terhitung dua.
> Menolak yang kedua memaksa petugas berbohong atau melewatkan catatan.
>
> **`VAL-DOK-27` baru karena kunci permintaan kini wajib.** Pada `0.1.0` kuncinya opsional,
> sehingga `INV-DOK-06` tidak dapat dijamin: dua kiriman tanpa kunci menghasilkan dua event.

---

## 5. Resep dan penunjang

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-19` | Membuat resep | Kunci permintaan sama | Mengembalikan resep yang sudah ada | `200` |
| `VAL-DOK-20` | Membuat resep obat pulang | Episode belum berstatus menunggu pulang | **Peringatan, bukan penolakan.** "Pasien belum dinyatakan boleh pulang. Obat pulang tetap dapat disiapkan." | `200` |
| `VAL-DOK-21` | Menandai obat sudah diserahkan | Percobaan apa pun dari sub-modul ini | "Status penyerahan obat hanya dapat diubah petugas Farmasi." | `403` |
| `VAL-DOK-22` | Pesanan lab dan radiologi | Penanda episode tidak cocok dengan kunjungan | "Pesanan ini tidak cocok dengan perawatan pasien." | `400` |
| `VAL-DOK-23` | Menulis hasil lab atau radiologi | Percobaan apa pun dari sub-modul ini | "Hasil pemeriksaan hanya dapat diisi petugas Laboratorium atau Radiologi." | `403` |

`VAL-DOK-21` dan `VAL-DOK-23` menjaga `RUL-DOK-01` dan `RUL-DOK-02` di tingkat aturan, bukan hanya
di tingkat niat.

---

## 6. Pembacaan hasil penunjang — penjaga `INV-DOK-12`

| Aturan | Kondisi | Perilaku |
| --- | --- | --- |
| `VAL-DOK-30` ★ | Hasil belum final atau belum diverifikasi modul pemiliknya | Ditampilkan dengan penanda **"belum final"** dan **tidak boleh** disajikan sebagai dasar keputusan klinis |
| `VAL-DOK-31` ★ | Hasil milik kunjungan di luar episode yang sedang dibuka | Tidak ditampilkan sama sekali |

> Keduanya adalah aturan **tampilan dan pembacaan**, bukan penolakan permintaan tulis. Ia tetap
> ditulis di sini karena akibatnya klinis: angka yang masih berubah, atau angka milik pasien lain,
> adalah dua cara paling langsung menghasilkan keputusan terapi yang salah.

---

## 7. Verifikasi CPPT — memantau, bukan menolak

| Aturan | Kondisi | Perilaku |
| --- | --- | --- |
| `VAL-DOK-24` | Kebijakan verifikasi belum ditetapkan | Seluruh catatan `NotRequired`. Daftar pantau kosong; pencatatan berjalan penuh |
| `VAL-DOK-25` | Catatan lewat batas verifikasi | Muncul pada daftar pantau. **Tidak menahan** penulisan catatan berikutnya |

> **Contoh `VAL-DOK-25`.** Perawat menulis CPPT untuk Ibu Sari pada 13 September pukul 01.15. Bila
> batas verifikasi disetel 24 jam dan dr. Andi baru memverifikasi pukul 06.30 tanggal 14 September,
> episode Ibu Sari muncul di daftar pantau dengan keterangan terlambat **5 jam 15 menit**. Selama
> rentang itu tidak ada satu pun tindakan yang tertahan. Angka 24 jam adalah **contoh**;
> `RWI-RULE-021` belum `approved` dan nilainya menunggu pemilik klinis.

---

## 8. Koreksi dokumen yang sudah final

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-32` ★ | Menambah koreksi | Catatan **belum** final | "Catatan ini belum final. Perbaiki langsung pada catatannya." | `400` |
| `VAL-DOK-33` ★ | Menyunting isi catatan | Catatan sudah final | "Catatan yang sudah diselesaikan tidak dapat diubah. Tambahkan koreksi beserta alasannya." | `409` |
| `VAL-DOK-34` ★ | Menerbitkan penetapan berhalangan | Masa berlaku kosong | "Masa berlaku penetapan wajib diisi." | `400` |
| `VAL-DOK-35` ★ | Mengoreksi atas nama dokter lain | Pengguna **bukan DPJP yang aktif** pada episode pasien itu | "Koreksi atas nama dokter lain hanya dapat dilakukan DPJP yang sedang bertanggung jawab atas pasien ini." | `403` |

> **`VAL-DOK-33` adalah celah yang sedang ditutup, bukan aturan baru.** Hari ini penyuntingan
> setelah selesai memang sudah ditolak, tetapi jalur koreksinya juga tertutup karena catatan dokter
> tidak pernah terdaftar pada mesin keutuhan — sehingga pesannya menjanjikan sesuatu yang belum ada.
> Setelah pendaftaran berjalan, pesan itu baru benar.
>
> **`VAL-DOK-35` tidak dapat dijaga mesin hak akses.** Penetapan berhalangan bersifat milik penulis
> dan tidak menyebut penggantinya, sehingga pemeriksaan "apakah pengguna ini DPJP aktif pasien itu"
> wajib berada di dalam perintah bisnis — `INV-DOK-13`.
>
> **Contoh `VAL-DOK-32`.** dr. Andi menulis catatan dan menyimpannya sebagai konsep, lalu mencoba
> menambahkan koreksi. Permintaan ditolak, dan pesannya mengarahkan ia menyunting langsung — karena
> catatan itu memang belum final, sehingga tidak ada apa pun yang perlu dikoreksi.
