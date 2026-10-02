# Validation Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | **`0.6.0`** |
| `last_changed_in` | **`0.6.0`** — bagian 10 lahir: `VAL-DOK-41` s.d. `VAL-DOK-59`. Kontrak ini melompati `0.5.0` karena isinya tidak bergerak pada Gelombang 1A |
| Status | **`draft`** untuk `0.6.0`. `0.4.0` **`approved`** — disetujui Muhammad Hamzah, 2026-09-09 |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`) |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-09** untuk `0.4.0`; `0.3.0` disetujui 2026-09-03 |
| `input_revision` | `02-backend-architecture.md` `0.2`; arsitektur domain `0.2` |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Compatibility impact | `0.4.0`: lima aturan baru `VAL-DOK-36` s.d. `VAL-DOK-40` untuk diagnosis terstruktur. Sebelumnya `0.3.0` menambah `VAL-DOK-32` s.d. `VAL-DOK-35`. **Nol aturan dicabut, dan nol aturan lama dilonggarkan** — `VAL-DOK-38` justru menuliskan secara tegas bahwa jalur rawat jalan tidak berubah |
| Tanggal | 2 September 2026; diamendemen 9 September 2026 |

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
| `VAL-DOK-11` | Menyelesaikan kajian medis | Daftar masalah **dan** diagnosis kerja sama-sama kosong | "Diagnosis atau daftar masalah belum diisi." | `400` |
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
>
> **`VAL-DOK-11` dipertajam pada `0.4.0`, bukan diperketat.** Sejak bagian 9 ada, daftar masalah
> punya **dua** bentuk sah: diagnosis kerja berupa teks bebas, dan diagnosis terstruktur berkode
> ICD. Kajian medis lolos bila **salah satu** terisi. Menuntut keduanya akan memaksa dokter
> mengetik hal yang sama dua kali.

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

---

## 9. Diagnosis terstruktur — `CAP-022` aturan 5 ★ baru pada `0.4.0`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-36` ★ | Mencatat diagnosis terstruktur | **Nomor konsultasi dan perawatan rawat inap sama-sama kosong** | "Diagnosis harus melekat pada catatan dokter atau pada perawatan pasien yang sedang berjalan." | `400` |
| `VAL-DOK-37` ★ | Mencatat diagnosis terstruktur | Nomor konsultasi dan perawatan sama-sama terisi tetapi **menunjuk pasien atau kunjungan yang berbeda** | "Diagnosis ini tidak cocok dengan perawatan pasien. Periksa kembali pasien yang sedang Anda buka." | `400` |
| `VAL-DOK-38` ★ | Mencatat diagnosis terstruktur pada kunjungan **rawat jalan atau medical check-up** | Nomor konsultasi kosong | Kalimat penolakan **sama persis** seperti sebelum `0.4.0` | `400` |
| `VAL-DOK-39` ★ | Mencatat diagnosis dari kajian medis | Pengguna tidak berwenang menulis kajian medis pasien itu | "Anda bukan DPJP pasien ini. Hubungi DPJP atau supervisor klinis." | `403` |
| `VAL-DOK-40` ★ | Mencatat diagnosis terstruktur | Perawatan yang disebut **bukan milik pasien** pada permintaan itu | "Diagnosis ini tidak cocok dengan perawatan pasien. Periksa kembali pasien yang sedang Anda buka." | `400` |

> **`VAL-DOK-38` adalah aturan yang paling penting di bagian ini, dan ia tidak menambah kemampuan
> apa pun.** Ia menuliskan hitam di atas putih bahwa pelonggaran ini **tidak menetes** ke
> poliklinik dan medical check-up. Bunyi penolakannya sengaja diikat pada "sama persis seperti
> sebelumnya" supaya pengujiannya tidak dapat lolos hanya dengan menolak — kode **dan** kalimatnya
> dibandingkan utuh, cara yang sama yang dipakai `BE-RWI-043` membuktikan `RWI-AC-143`.
>
> **`VAL-DOK-39` tidak dapat dijaga mesin hak akses**, sebab mesin itu tahu peran dan tidak tahu
> pasien. Ia memakai pemeriksaan dokter aktif per episode yang **sudah ada** — penjaga yang sama
> dengan `VAL-DOK-06`. Dicatat pada [`permission-audit-matrix.md`](./permission-audit-matrix.md)
> bagian 3.
>
> **`VAL-DOK-37` dan `VAL-DOK-40` sengaja berbagi satu kalimat** dengan `VAL-DOK-26`. Bagi dokter
> yang sedang membuka layar, ketiganya adalah kesalahan yang sama: ia menulis untuk pasien yang
> keliru. Membedakan kalimatnya hanya memberi tahu penyerang bagian mana yang tidak cocok.
>
> **Contoh `VAL-DOK-36`.** dr. Sari baru selesai memeriksa Tn. Budi yang masuk kamar tadi pagi.
> Ia menambahkan diagnosis "J18.9 Pneumonia" dari layar kajian medis. Permintaannya menyebut
> perawatan rawat inap Tn. Budi, tanpa nomor konsultasi — dan **diterima**, karena catatan harian
> pertama memang belum ada. Bila layar keliru mengirim keduanya kosong, permintaan ditolak dan
> dokter diminta membuka pasiennya kembali.

---

## 10. Perubahan pada `contract_version` `0.6.0` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status `draft`.** Penomoran melanjutkan `VAL-DOK-40`. Pesan ditulis sebagaimana dibaca dokter dan perawat. Seluruh
aturan pada bagian ini **hanya** menyala untuk kunjungan yang punya episode rawat inap, kecuali yang disebut berlaku
umum. Aturan lama `VAL-DOK-01` s.d. `VAL-DOK-40` tidak dicabut dan tidak dilonggarkan.

### 10.1 Kewenangan penulis dan waktu — `INV-DOK-14`, `INV-DOK-15`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-41` | `PUT`, `PATCH /soap`, `PATCH /complete` pada catatan dokter; `PUT`, `PATCH /complete` pada kajian medis; `PUT` konsep CPPT profesi dokter | Pengguna bukan penulis konsep | "Konsep ini ditulis dokter lain. Hanya penulisnya yang dapat mengubah atau menyelesaikannya." | `403` |
| `VAL-DOK-42` | Membuat catatan dokter, kajian medis, CPPT dokter, pesanan tindakan dokter | Tidak ada penugasan dokter yang **aktif saat disimpan**, walaupun ada penugasan pada waktu klinis | "Anda tidak sedang bertugas atas pasien ini. Minta kepala ruangan membuatkan penugasan singkat untuk menulis catatan terlambat." | `403` |
| `VAL-DOK-43` | Mengubah atau menyelesaikan konsep | Registrasi konsep berstatus `LockedUnsigned` | "Catatan ini terkunci karena perawatan pasien sudah ditutup. Lengkapi lewat addendum dari Catatan Saya." | `409` |
| `VAL-DOK-58` | Membuat dokumen **baru** apa pun, termasuk tindakan dari catatan dokter | Episode `Closed` atau `Cancelled` | "Perawatan pasien ini sudah ditutup. Catatan baru tidak dapat dibuat; tambahkan addendum pada catatan yang sudah ada." | `422` |

**Contoh `VAL-DOK-42`.** Penugasan dokter jaga dr. Yoga berlaku Senin 22.00 s.d. Selasa 07.00. Selasa 08.10 ia
mengirim catatan pemasangan infus berwaktu klinis 05.00 → ditolak `403` dengan pesan di atas. 08.30 kepala ruangan
membuat penugasan singkat 08.30–09.30. 08.40 kiriman ulang → diterima, karena penugasan lamanya berlaku pukul 05.00
dan penugasan singkatnya aktif pukul 08.40.

**Pengecualian yang bukan pelanggaran `VAL-DOK-42`.** Menyelesaikan konsep milik sendiri tanpa penugasan aktif
**diterima** bila konsep dibuat saat penugasan aktif dan waktu klinisnya berada di dalam periode itu —
`RWI-DEC-128` butir (2). Contoh: konsep SOAP Joko pukul 06.30, penugasan berakhir 07.00, diselesaikan 08.00 → `200`.

### 10.2 Verifikasi CPPT — `INV-DOK-16`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-44` | `PATCH /{id}/verify` | Episode belum `Closed` dan dokter tidak memegang penugasan **berperan DPJP** yang aktif pada detik verifikasi | "Hanya DPJP yang sedang bertugas atas pasien ini yang dapat memverifikasi catatan." | `403` |
| `VAL-DOK-44a` | Sama | Episode `Closed` dan dokter bukan DPJP terakhir | "Perawatan ini sudah ditutup. Hanya DPJP terakhir pasien yang dapat memverifikasi catatan yang tertinggal." | `403` |
| `VAL-DOK-44b` | Sama | Episode `Closed` dan entri ditulis **setelah** waktu penutupan | "Catatan ini ditulis setelah perawatan ditutup sehingga tidak termasuk verifikasi DPJP." | `422` |

**Contoh `VAL-DOK-44a`.** Budi ditutup Senin 13.00. dr. Ahmad, DPJP sebelum dr. Rina, memverifikasi entri Sabtu 21.00
→ `403`. dr. Rina, DPJP terakhir, memverifikasi → `200`; daftar pantau tetap mencatat entri itu terlambat 19 jam dari
target 24 jam.

### 10.3 Pesanan tindakan dan instruksi — `INV-DOK-17`, `RWI-DEC-114`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-45` | Menyimpan pesanan tindakan | `ConsultationId` dan `InpEpisodeId` **keduanya** kosong | "Pesanan tindakan belum terhubung ke perawatan pasien. Buka kembali pasiennya lalu ulangi." | `400` |
| `VAL-DOK-45a` | Menyimpan pesanan tindakan pada kunjungan rawat jalan atau MCU | `ConsultationId` kosong | Pesan lama apa adanya — `RWI-AC-143`. **Perilaku poliklinik tidak berubah** | `400` |
| `VAL-DOK-46` | Perawat membuat pesanan tindakan, laboratorium, atau radiologi | Dokter pemberi instruksi tidak dipilih | "Pilih dokter yang memberi instruksi." | `400` |
| `VAL-DOK-47` | Sama | Dokter pemberi instruksi tidak punya penugasan aktif atas pasien saat pesanan dibuat | "Dokter yang dipilih tidak sedang bertugas atas pasien ini." | `403` |
| `VAL-DOK-48` | Mengubah pesanan tindakan yang belum dilaksanakan | Pengguna bukan penginput | "Pesanan ini dibuat petugas lain. Hanya penginputnya yang dapat mengubah isi pesanan." | `403` |
| `VAL-DOK-49` | Membatalkan pesanan tindakan yang belum dilaksanakan | Pengguna bukan penginput dan bukan DPJP aktif | "Pesanan hanya dapat dibatalkan penginputnya atau DPJP pasien." | `403` |
| `VAL-DOK-49a` | Sama | Alasan kosong | "Alasan pembatalan wajib diisi." | `400` |
| `VAL-DOK-49b` | Sama | Pesanan sudah menimbulkan tagihan | Pesan lama apa adanya — penolakan tagihan yang sudah ada di source | `409` |
| `VAL-DOK-50` | Verifikasi instruksi pada tindakan, laboratorium, radiologi | Pengguna bukan dokter pemberi instruksi | "Hanya dokter pemberi instruksi yang dapat memverifikasi pesanan ini." | `403` |
| `VAL-DOK-50a` | Sama | Status verifikasi bukan `Pending` | "Pesanan ini sudah diverifikasi atau tidak memerlukan verifikasi." | `409` |

**Contoh `VAL-DOK-47`.** 23.00 Ns. Siti memilih dr. Ahmad sebagai pemberi instruksi untuk Budi, padahal dr. Ahmad
tidak punya penugasan atas Budi. Layar tidak menawarkan dr. Ahmad; bila permintaan tetap dikirim langsung → `403`,
dan tidak ada pesanan yang diteruskan ke laboratorium.

### 10.4 Resep, penghentian butir, dan template — `RWI-DEC-121`, `122`, `135`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-51` | `PATCH /items/{itemId}/stop` | Dokter tidak punya penugasan aktif atas pasien | "Anda tidak sedang bertugas atas pasien ini." | `403` |
| `VAL-DOK-51a` | Sama | Alasan kosong | "Alasan penghentian wajib diisi." | `400` |
| `VAL-DOK-51b` | Sama | Butir sudah dihentikan atau resepnya dibatalkan | "Obat ini sudah dihentikan." | `409` |
| `VAL-DOK-56` | Template resep: buat, buat-dari-resep, ubah — **berlaku umum, termasuk poliklinik** | `OwnerDoctorId` berbeda dari dokter akun login, atau akun tidak tertaut dokter | "Template hanya dapat dibuat atau diubah atas nama Anda sendiri." | `403` |
| `VAL-DOK-56a` | Template resep: ubah dan hapus — berlaku umum | Pengguna bukan pemilik | "Template ini milik dokter lain." | `403` |
| `VAL-DOK-56b` | Template resep: simpan — berlaku umum | Tidak ada satu pun obat maupun racikan | "Template harus berisi sekurang-kurangnya satu obat." | `400` |
| `VAL-DOK-56c` | Pakai template pada resep berkonteks rawat inap | Template bukan milik dokter login, **atau** pemakai bukan dokter | "Template ini tidak dapat dipakai dari ruang kerja rawat inap." | `403` |
| `VAL-DOK-57` | Menyimpan draft resep hasil template | Masih ada butir bertanda bentrok alergi atau obat tidak tersedia | "Masih ada obat yang bentrok alergi atau tidak tersedia: {daftar}. Hapus atau ganti sebelum menyimpan." | `400` |

**Contoh `VAL-DOK-56`.** Asisten poliklinik membuat template "ISPA anak" dengan `OwnerDoctorId` dr. Rina dari akunnya
sendiri → `403`, nol baris tersimpan. Hari ini permintaan yang sama diterima (`RWI-FACT-035`) — inilah perubahan
perilaku poliklinik yang wajib diberitahukan pemilik `rawat-jalan`.

### 10.5 Rekonsiliasi obat — `RWI-DEC-132` s.d. `134`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-52` | `POST /{id}/decisions` | Pengguna bukan dokter yang berwenang menulis resep untuk pasien | "Keputusan per obat hanya dapat diambil dokter yang merawat pasien ini." | `403` |
| `VAL-DOK-52a` | Mengganti keputusan | Butir resep hasil keputusan sebelumnya **tidak lagi draft** | "Resep dari keputusan sebelumnya sudah aktif. Ubah terapi lewat resep atau hentikan obatnya." | `409` |
| `VAL-DOK-53` | `POST /` rekonsiliasi | `DrugId` kosong, atau obat tidak ditemukan pada master obat | "Pilih obat dari master obat. Bila belum ada, daftarkan dulu sebagai obat non-formularium." | `400` |
| `VAL-DOK-53a` | `PATCH /{id}/cancel` rekonsiliasi | Baris sudah punya keputusan dokter | "Obat ini sudah diputuskan dokter sehingga tidak dapat dibatalkan." | `409` |
| `VAL-DOK-53b` | `POST /drugs/non-formulary-registrations` | Nama atau kategori kosong | "Nama dan kategori obat wajib diisi." | `400` |

Jalur non-formularium tidak punya aturan "penanda formularium harus diisi `false`": server **selalu** menulis `false`
sehingga tidak ada isian yang dapat keliru — `RWI-AC-194`.

### 10.6 Sliding scale — `RWI-DEC-145` s.d. `147`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-54` | Menyimpan versi template atau versi order | Dua rentang bertumpuk | "Rentang {a} dan {b} bertumpuk. Setiap nilai gula darah harus jatuh ke tepat satu rentang." | `400` |
| `VAL-DOK-54a` | Sama | Ada nilai yang tidak jatuh ke rentang mana pun, termasuk tidak ada rentang terbuka di bawah atau di atas | "Rentang belum menutup seluruh nilai gula darah. Tambahkan rentang untuk nilai di bawah {x} atau di atas {y}." | `400` |
| `VAL-DOK-54b` | Sama | Dosis negatif, atau satuan gula darah belum dipilih | "Dosis tidak boleh negatif dan satuan gula darah wajib dipilih." | `400` |
| `VAL-DOK-54c` | `POST /versions/{versionId}/approve` | Pengesah adalah pengubah terakhir versi | "Versi ini terakhir diubah oleh Anda. Pengesahan harus dilakukan pengguna lain." | `403` |
| `VAL-DOK-54d` | Mengubah versi | Versi sudah `Approved` atau `Retired` | "Versi yang sudah disahkan tidak dapat diubah. Buat versi baru." | `409` |
| `VAL-DOK-55` | `POST /sliding-scale-orders` | Versi template yang dipilih bukan `Approved` | "Protokol ini belum disahkan sehingga belum dapat dipesan." | `409` |
| `VAL-DOK-55a` | Sama, atau menyesuaikan | Rentang atau dosis berbeda dari template tetapi alasan kosong | "Alasan penyesuaian wajib diisi." | `400` |
| `VAL-DOK-55b` | `POST /sliding-scale-orders` | Butir resep bukan insulin berdosis skala, atau sudah punya order | "Butir resep ini tidak dapat dipasangi protokol sliding scale." | `409` |
| `VAL-DOK-55c` | Menyesuaikan order | `ExpectedVersionNumber` tidak sama dengan versi sekarang | "Protokol sudah diubah dokter lain. Muat ulang lalu periksa kembali." | `409` |
| `VAL-DOK-55d` | Menyesuaikan atau menghentikan | Order sudah `Stopped` | "Protokol sudah dihentikan. Pesan protokol baru bila diperlukan." | `409` |
| `VAL-DOK-55e` | Membuka menu sliding scale | Belum ada satu pun template dengan versi `Approved` | **Keadaan layar, bukan galat:** "Belum ada protokol sliding scale yang disahkan." | `200` |

**Contoh `VAL-DOK-54` dan `VAL-DOK-54a`.** dr. Rina mengubah rentang menjadi 200–260 dan 250–299 → nilai 255 jatuh ke
dua rentang → `400`. Template draft "v2" yang dimulai dari 150 tanpa rentang di bawahnya → nilai 120 tidak punya
rentang → `400` sampai rentang "< 150" ditambahkan.

### 10.7 Jenis catatan CPPT — `RWI-DEC-140`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-DOK-59` | `POST /` catatan terpadu | `NoteKind` tidak cocok dengan profesi penulis dari akun login: perawat mengirim selain `NursingSoap`/`NursingNarrative`, dokter mengirim selain `PhysicianNote` | "Jenis catatan tidak sesuai dengan profesi Anda." | `400` |
| `VAL-DOK-59a` | Sama | Akun penulis tidak tertaut data pegawai berprofesi | "Akun Anda belum terhubung ke data profesi, sehingga catatan tidak dapat disimpan." | `403` |

### 10.8 Aturan yang dijaga sub-modul lain dan hanya dirujuk

| Aturan | Dijaga | Isinya |
| --- | --- | --- |
| Penugasan singkat wajib dokter jaga dan berbatas waktu | `episode-rawat-inap` `VAL-INP-01` | `RWI-DEC-130` |
| Pelaksanaan sliding scale tanpa order aktif; GDS laboratorium sebagai sumber | `keperawatan` `VAL-KEP-27`, `VAL-KEP-28` | `RWI-AC-219`, `RWI-AC-228` |
| Tanda tangan resume oleh bukan DPJP aktif | `episode-rawat-inap`, sudah ada | `RWI-RULE-032` |
